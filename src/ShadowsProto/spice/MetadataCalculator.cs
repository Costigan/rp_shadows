using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using ITOSLIB;

namespace UVSLib.Data
{
    public class MetadataCalculator
    {
        public const double DegToRad = Math.PI / 180d;
        public const double RadToDeg = 180d / Math.PI;
        public const double LunarRadiusD = 1737.4d;
        public const float LunarRadius = 1737.4f;
        public const float MinTerrain = -6.6695f + LunarRadius;
        public const float MaxTerrain = 10.774f + LunarRadius;

        public static MetadataCalculator Calculator;

        public const string LadeeBodyFrame = "LADEE_SC_PROP";
        public const string TelescopeFrame = "LADEE_UVSTEL";
        public const string SolarViewerFrame = "LADEE_UVSSOL";

        public CSpice Spice;
        private readonly TerrainManager _terrainManager;

        public string SpiceRoot;
        public long LatestSpiceTime42;

        public AttitudeSupplier AttitudeSupplier;
        public const long QuaternionTimeout = 65536L * 3600L;  // 1 hour: If the quaternion returned from the supplier is out of date more than this, then ignore it

        public double[] OriginVector = new[] { 0d, 0d, 0d };

        public const double FOVHalfAngle = 0.5d*Math.PI/180d;  // Half angle for both telescope and solar viewer

        public static MetadataCalculator GetCalculator(CSpice spice, string spiceRoot, string telemetryRoot = null)
        {
            if (Calculator != null)
                return Calculator;
            Calculator = new MetadataCalculator(spice, spiceRoot, telemetryRoot);
            return Calculator;
        }

        public MetadataCalculator(CSpice spice, string spiceRoot, string telemetryRoot = null)
        {
            Spice = spice;
            _terrainManager = new TerrainManager();
            SpiceRoot = spiceRoot;
            FurnishSpiceKernels(spiceRoot);

            if (telemetryRoot != null)
                LoadAttitudeFromTelemetry(telemetryRoot);
        }

        public void FurnishSpiceKernels(string spiceKernelRoot)
        {
            FurnishSclk(spiceKernelRoot);
            FurnishFk(spiceKernelRoot);
            FurnishTrajectories(spiceKernelRoot);
            FurnishAttitudes(spiceKernelRoot);
        }

        public void FurnishFk(string root)
        {
            var clockRoot = Path.Combine(root, @"fk\");
            var di = new DirectoryInfo(clockRoot);
            var files = di.GetFiles("*.tf");
            files = files.OrderBy(a => a.Name).ToArray();
            if (files.Length < 1)
            {
                Console.WriteLine(@"Cannot find the frames kernel (.tf)");
                return;
            }
            Furnish(files[0].FullName);
        }

        public void FurnishSclk(string root)
        {
            var clockRoot = Path.Combine(root, @"sclk\");
            var di = new DirectoryInfo(clockRoot);
            var files = di.GetFiles("*.tsc");
            files = files.OrderBy(a => a.Name).ToArray();
            if (files.Length < 1)
            {
                Console.WriteLine(@"Cannot find the clock correction file (.tsc)");
                return;
            }
            Furnish(files[files.Length - 1].FullName);
        }

        public void FurnishTrajectories(string root)
        {
            var trajectoryRoot = Path.Combine(root, @"spk\definitive\");

            var di = new DirectoryInfo(trajectoryRoot);
            var files = di.GetFiles("*.bsp");
            files = files.OrderBy(a => a.Name).Where(a => !a.Name.Equals("ladee_r_13290_13321_v01.bsp")).ToArray();
            if (files.Length < 1)
            {
                Console.WriteLine(@"Cannot find the preliminary definitive ephemeris.");
                return;
            }
            var currentDefinitive = files[files.Length - 1].FullName;
            var definitiveEnd = GetEndDay(currentDefinitive);

            var predictedRoot = Path.Combine(root, @"spk\predicted\");
            var pdi = new DirectoryInfo(predictedRoot);
            var pfiles = pdi.GetFiles("*.bsp").Select(p => p.FullName).OrderByDescending(a => a).ToArray();
            var loadPredicts = new List<string>();
            int beginDay = 15000;
            foreach (var pf in pfiles)
            {
                var en = GetEndDay(pf);
                if (en < definitiveEnd) continue;
                var bg = GetBeginDay(pf);         
                if (bg < beginDay)
                {
                    loadPredicts.Add(pf);
                    beginDay = bg;
                }
                if (beginDay < definitiveEnd)
                    break;
            }

            loadPredicts.Reverse();
            foreach (var p in loadPredicts)
                FurnishTrajectory(p);

            FurnishTrajectory(Path.Combine(trajectoryRoot, "ladee_r_13250_13279_v01.bsp"));
            FurnishTrajectory(Path.Combine(trajectoryRoot, "ladee_r_13279_13286_v01.bsp"));
            FurnishTrajectory(Path.Combine(trajectoryRoot, "ladee_r_13286_13293_v01.bsp"));
            FurnishTrajectory(currentDefinitive);
        }

        private static int GetBeginDay(string path)
        {
            var n = Path.GetFileName(path);
            if (n == null) return 15000;   // Past the end of the mission
            if (n.Length < 20) return 15000;
            var s = n.Substring(8, 5);
            int r;
            return !int.TryParse(s, out r) ? 15000 : r;
        }

        private static int GetEndDay(string path)
        {
            var n = Path.GetFileName(path);
            if (n == null) return 15000;   // Past the end of the mission
            if (n.Length < 20) return 15000;
            var s = n.Substring(14, 5);
            int r;
            return !int.TryParse(s, out r) ? 15000 : r;
        }

        public void FurnishTrajectory(string path)
        {
            if (File.Exists(path))
                Furnish(path);
            else
                Console.Error.WriteLine("Tried to furnish non-existant kernel: {0}", path);
        }

        public static void Furnish(string path)
        {
            Console.WriteLine(@"Furnishing {0}", path);
            CSpice.furnsh_c(path);
        }

        /// <summary>
        /// Furnish attitudes from earliest to latest, ignoring previous versions of files
        /// </summary>
        /// <param name="root"></param>
        private void FurnishAttitudes(string root)
        {
            var attitudeRoot = Path.Combine(root, @"ck\definitive\");
            var di = new DirectoryInfo(attitudeRoot);
            var fileArray = di.GetFiles("*.bc");
            fileArray = fileArray.OrderBy(a => a.Name).Reverse().ToArray();
            var files = new List<FileInfo>();
            var previousFile = "";  // This is the previous filename without the version spec (_vxx)
            foreach (var fi in fileArray)
            {
                var withoutVersion = fi.Name.Substring(0, 20);
                if (previousFile.Equals(withoutVersion, StringComparison.InvariantCultureIgnoreCase))
                    continue;
                previousFile = withoutVersion;
                files.Add(fi);
            }

            // Work out the bounds of definitive attitude

            var earliest = long.MaxValue;
            var latest = long.MinValue;
            foreach (var fname in files)
            {
                long b;
                long e;
                CSpice.CkGetCoverage(fname.FullName, out b, out e);
                earliest = Math.Min(earliest, b);
                latest = Math.Max(latest, e);
            }

            // Furnish the attitude files

            files.Reverse();  // Furnish them from the earliest to the latest
            foreach (var fi in files)
                Furnish(fi.FullName);

            LatestSpiceTime42 = latest;
            
            // Load latest attitudes from telemetry
            Console.WriteLine(@"Latest attitude in spice CK kernels: {0}", TimeUtilities.Time42ToString(LatestSpiceTime42));
        }

        public void LoadAttitudeFromTelemetry(string uvsRoot)
        {
            if (Calculator == null || Calculator.AttitudeSupplier != null)
                return;
            var supplier = new TelemetryAttitudeSupplier();
            Calculator.AttitudeSupplier = supplier;

            // Load telemetry from before the SOC's CK files
            // Now covered by new CKs from Cory (not checked in, though)
            //LoadTelemetryForDay(new DateTime(2013, 9, 14), uvsRoot);  // doy 258
            //LoadTelemetryForDay(new DateTime(2013, 9, 15), uvsRoot);  // doy 258
            //LoadTelemetryForDay(new DateTime(2013, 9, 17), uvsRoot);  // doy 260
            //LoadTelemetryForDay(new DateTime(2013, 9, 18), uvsRoot);  // doy 261

            // Load telemetry after the SOC's CK files
            var l = TimeUtilities.Time42ToDateTime(Calculator.LatestSpiceTime42);
            var day = new DateTime(l.Year, l.Month, l.Day);
            // Load a day's worth of attitude telemetry until there is no more
            var flag = true;
            while (flag)
            {
                flag = LoadTelemetryForDay(day, uvsRoot);
                day = day.AddDays(1);
            }

            supplier.Sort();

            const string filename = "temporary.ck";
            Console.WriteLine(@"Writing {0} quaternions to temporary ck file {1}", supplier.Quaternions.Count, filename);
            AttitudeSupplier.WriteQuaternionsToFile(supplier.Quaternions, filename);
            CSpice.furnsh_c(filename);
        }

        private bool LoadTelemetryForDay(DateTime day, string uvsRoot)
        {
            Console.WriteLine(@"Loading telemetry for day {0}, {1}", day, TimeUtilities.DateTimeToString(day));
            var flag = false;
            var yearDay = string.Format(@"{0:D4}_{1:D3}", day.Year, day.DayOfYear);
            var di = new DirectoryInfo(Path.Combine(uvsRoot, yearDay));
            foreach (var childDi in di.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                foreach (var fi in childDi.EnumerateFiles("SC_HK_*.HK.0"))
                {
                    flag = true;

                    var c = Calculator.AttitudeSupplier as TelemetryAttitudeSupplier;
                    //Console.WriteLine(@"before Span is [{0}, {1}]", TimeUtilities.Time42ToString(c.BeginSpan), TimeUtilities.Time42ToString(c.EndSpan));

                    Calculator.AttitudeSupplier.Load(fi.FullName);


                    Console.WriteLine(@"Loading attitude from {0}.", fi.FullName);
                    if (c == null)
                        Console.WriteLine(@"  Span is unknown");
                    else
                        Console.WriteLine(@"  Span is now [{0}, {1}]", TimeUtilities.Time42ToString(c.BeginSpan), TimeUtilities.Time42ToString(c.EndSpan));
                }
            }
            return flag;
        }

        public bool AddGeometricMetadata(long rawTime42, BaseSpectrum m)
        {
            var qstk = StkAttitudeAtTime(rawTime42);
            if (qstk == null)
                return false;
            Calculator.AddGeometricMetadata(rawTime42, m, qstk);
            return true;
        }

        public double[] StkAttitudeAtTime(long rawTime42)
        {
            var qstk = new double[4];
            if (StkAttitudeAtTime(rawTime42, qstk))
                return qstk;
            if (AttitudeSupplier == null)
                return null;
            var qd = AttitudeSupplier.QuaternionAt(rawTime42);
            if (Math.Abs(rawTime42 - qd.Timestamp) > QuaternionTimeout)
                return null;
            qstk[0] = qd.X;
            qstk[1] = qd.Y;
            qstk[2] = qd.Z;
            qstk[3] = qd.W;
            //Console.WriteLine("  Got quaternion from supplier");
            return qstk;
        }

        public bool StkAttitudeAtTime(long rawTime42, double[] qstk)
        {
            /*  This worked.
            var et = TimeUtilities.Time42ToET(rawTime42);

            const int sc = CSpice.LadeeId;
            const int sc1 = -12000;

            var sclkdp = 0d;  // Encoded spacecraft time

            CSpice.sce2c_c(sc, et, ref sclkdp);

            var cmat = new double[3, 3];
            var clkout = 0d;
            var found = 0;
            CSpice.ckgp_c(sc1, sclkdp, 0d, "J2000", cmat, ref clkout, ref found);
             * */

            var sclkdp = (double)rawTime42;
            const int sc1 = -12000;
            var cmat = new double[3, 3];
            var clkout = 0d;
            var found = 0;
            CSpice.ckgp_c(sc1, sclkdp, 0d, "J2000", cmat, ref clkout, ref found);

            if (found == 0)
            {
                var s = TimeUtilities.Time42ToString((long) sclkdp);
                //Console.Error.WriteLine(@"Couldn't find an attitude at time {0}, {1}", sclkdp, s);
                return false;
            }

            var qspice = new double[4];
            CSpice.m2q_c(cmat, qspice);
            Calculator.Spice2StkQuat(qspice, qstk);
            return true;
        }


        public void AddGeometricMetadata(long rawTime42, BaseSpectrum m, double[] qstk)
        {
            if (rawTime42==0)
                Console.Error.WriteLine("Time is 0.");
            const int scId = CSpice.LadeeId;
            var sclkdp = (double) rawTime42;
            var etCorrected = 0d;
            CSpice.sct2e_c(scId, sclkdp, ref etCorrected);

            var trueTime42 = TimeUtilities.ETToTime42(etCorrected);
            m.Timestamp = trueTime42;
            //Console.WriteLine(@"raw={0}, diff={1}", TimeUtilities.Time42ToString(rawTime42),
            //                  (m.Timestamp - m.RawTimestamp)/65536d);

            //var timestring = TimeUtilities.Time42ToSTK(time);

            //-------------------------------------------
            // Metadata related to the s/c position
            //-------------------------------------------

            var qspice = new double[4];
            Stk2SpiceQuat(qstk, qspice);

            m.QuaternionW = qspice[0];
            m.QuaternionX = qspice[1];
            m.QuaternionY = qspice[2];
            m.QuaternionZ = qspice[3];

            // Ladee State and Position in ME frame
            var ladeeState = new double[6];
            var lposme = new double[3];
            var lt = 0d;

            // The LADEE's state in ME frame
            CSpice.spkgeo_c(CSpice.LadeeId, etCorrected, "MOON_ME", 301, ladeeState, ref lt);
            lposme[0] = ladeeState[0];
            lposme[1] = ladeeState[1];
            lposme[2] = ladeeState[2];

            m.MoonFixedX = lposme[0];
            m.MoonFixedY = lposme[1];
            m.MoonFixedZ = lposme[2];
            m.MoonFixedVx = ladeeState[3];
            m.MoonFixedVy = ladeeState[4];
            m.MoonFixedVz = ladeeState[5];

            double ladeeLon = 0d, ladeeLat = 0d, ladeeAlt = 0d;
            CSpice.recpgr_c("MOON", lposme, CSpice.MoonRadius, 0d, ref ladeeLon, ref ladeeLat, ref ladeeAlt);

            var longitude = ladeeLon * 180d / 3.141592653589d;
            var lattitude = ladeeLat * 180d / 3.141592653589d;
            var altitude = ladeeAlt;
            m.Longitude = longitude;
            m.Lattitude = lattitude;
            m.Altitude = altitude;
            //m.AltitudeOffTerrain = altitude;

            // Ladee position in Solar Frame
            // Convert epoch to ephemeris time
            var ladeePosSolar = new double[3];
            var rotMeToSolar = Spice.GetMoonFixedToMoonSolarTransform(etCorrected);
            // transform position to local time

            CSpice.mxv_c(rotMeToSolar, lposme, ladeePosSolar);

            // I don't understand the bug, currently, but the solar latitude is the inverse of what it should be
            ladeePosSolar[2] = -ladeePosSolar[2];

            // convert to lon, and lat
            var solarLongitude = 0d;
            var solarLattitude = 0d;
            var solarAltitude = 0d;
            CSpice.recpgr_c("MOON", ladeePosSolar, CSpice.MoonRadius, CSpice.MoonFlattening, ref solarLongitude,
                           ref solarLattitude, ref solarAltitude);
            solarLattitude = CSpice.ToDegrees(solarLattitude);
            solarLongitude = CSpice.ToDegrees(solarLongitude);

            m.SolarLattitude = solarLattitude;
            m.SolarLongitude = solarLongitude;

            //-------------------------------------------
            // Metadata related to the telescope boresight
            //-------------------------------------------

            // Calculate rotation from J2000 to LADEE_SC_PROP
            var j22Sc = new double[3, 3];
            CSpice.q2m_c(qspice, j22Sc);

            // Calculate the rotation from J2000 to Moon ME frame, preparing to convert pointing vector to ME frame
            var j22Me = new double[3, 3];
            CSpice.pxform_c("J2000", "MOON_ME", etCorrected, j22Me);

            var telvecj2 = new double[3];
            var telvecme = new double[3];
            CalculateBoresight(j22Sc, j22Me, TelescopeFrame, telvecj2, telvecme);

            double[] telLimbPtMe;
            double telLimbLongitudeMe;
            double telLimbLattitudeMe;
            double telLimbLonSol;
            double telLimbLatSol;
            double[] telTargetPointMe;
            double telTargetAltitude;
            CalculateLimbRelatedMetadata(lposme, ladeeState, telvecme, rotMeToSolar,
                                                out telLimbPtMe, out telLimbLongitudeMe, out telLimbLattitudeMe,
                                                out telLimbLonSol, out telLimbLatSol,
                                                out telTargetPointMe, out telTargetAltitude);

            //Console.WriteLine("calculated telescope target altitude: {0}", telTargetAltitude);

            //-------------------------------------------
            // Metadata related to the solar viewer
            //-------------------------------------------

            var solvecj2 = new double[3];
            var solvecme = new double[3];
            CalculateBoresight(j22Sc, j22Me, SolarViewerFrame, solvecj2, solvecme);

            double[] solLimbPtMe;
            double solLimbLongitudeMe;
            double solLimbLattitudeMe;
            double solLimbLonSol;
            double solLimbLatSol;
            double[] solTargetPointMe;
            double solTargetAltitude;
            CalculateLimbRelatedMetadata(lposme, ladeeState, solvecme, rotMeToSolar,
                                                out solLimbPtMe, out solLimbLongitudeMe, out solLimbLattitudeMe,
                                                out solLimbLonSol, out solLimbLatSol,
                                                out solTargetPointMe, out solTargetAltitude);

            //Console.WriteLine("calculated solar viewer target altitude: {0}", solTargetAltitude);

            //var t = TimeUtilities.Time42ToSTK(time);

            /*
            Console.WriteLine("Time mark {0}", Time);
            Console.WriteLine("ET mark {0}", ET);
            Write("QSPICE mark");Write(QSPICE);
            Write("J22SC mark");Write(J22SC);
            Write("SC2UVS mark"); Write(SC2UVS);
            Write("J22UVS mark");Write(J22UVS);
            Write("TVECJ2 mark");Write(TVECJ2);
            Write("TVECME mark");Write(TVECME);
            Write("LPOSME mark"); Write(LPOSME);
            Console.WriteLine("Time = {0}", TimeUtilities.Time42ToSTK(Time));


            //for (var t = 1; t < 1000; t++)
            //    WriteAttitude(TimeUtilities.Time42ToET(Time) + t, QSTK);
            //EndRun();

            throw new Exception("hi there");
             */

            var telGzPtMe = new double[3];
            double telGzLonMe;
            double telGzLateMe;
            double telGzAltitude;
            double telGzLonSol;
            double telGzLatSol;
            CalculateGrazingPointRelatedMetadata(lposme, telvecme, rotMeToSolar, telGzPtMe,
                                                 out telGzLonMe, out telGzLateMe, out telGzAltitude,
                                                 out telGzLonSol, out telGzLatSol);

            var solGzPtMe = new double[3];
            double solGzLonMe;
            double solGzLateMe;
            double solGzAltitude;
            double solGzLonSol;
            double solGzLatSol;
            CalculateGrazingPointRelatedMetadata(lposme, solvecme, rotMeToSolar, solGzPtMe,
                                                 out solGzLonMe, out solGzLateMe, out solGzAltitude,
                                                 out solGzLonSol, out solGzLatSol);

            //Console.WriteLine("Calculated telescope grazing LLA [{0},{1},{2}]", telGzLonMe, telGzLateMe, telGzAltitude);

            m.Longitude = longitude;
            m.Lattitude = lattitude;
            m.Altitude = altitude;
            m.AltitudeAboveTerrain = altitude;

            m.SolarLongitude = solarLongitude;
            m.SolarLattitude = solarLattitude;

            m.TelescopeBoresightJ2000X = telvecj2[0];
            m.TelescopeBoresightJ2000Y = telvecj2[1];
            m.TelescopeBoresightJ2000Z = telvecj2[2];
            m.TelescopeBoresightMEX = telvecme[0];
            m.TelescopeBoresightMEY = telvecme[1];
            m.TelescopeBoresightMEZ = telvecme[2];

            m.TelescopeTargetLongitude = telLimbLongitudeMe;
            m.TelescopeTargetLattitude = telLimbLattitudeMe;
            m.TelescopeTargetAltitude = telTargetAltitude;
            m.TelescopeTargetSolarLongitude = telLimbLonSol;
            m.TelescopeTargetSolarLattitude = telLimbLatSol;

            m.TelescopeGrazeLongitude = telGzLonMe;
            m.TelescopeGrazeLattitude = telGzLateMe;
            m.TelescopeGrazeAltitude = telGzAltitude;
            m.TelescopeGrazeSolarLongitude = telGzLonSol;
            m.TelescopeGrazeSolarLattitude = telGzLatSol;

            m.SolarViewerBoresightJ2000X = solvecj2[0];
            m.SolarViewerBoresightJ2000Y = solvecj2[1];
            m.SolarViewerBoresightJ2000Z = solvecj2[2];
            m.SolarViewerBoresightMEX = solvecme[0];
            m.SolarViewerBoresightMEY = solvecme[1];
            m.SolarViewerBoresightMEZ = solvecme[2];

            m.SolarViewerTargetLongitude = solLimbLongitudeMe;
            m.SolarViewerTargetLattitude = solLimbLattitudeMe;
            m.SolarViewerTargetAltitude = solTargetAltitude;
            m.SolarViewerTargetSolarLongitude = solLimbLonSol;
            m.SolarViewerTargetSolarLattitude = solLimbLatSol;

            m.SolarViewerGrazeLongitude = solGzLonMe;
            m.SolarViewerGrazeLattitude = solGzLateMe;
            m.SolarViewerGrazeAltitude = solGzAltitude;
            m.SolarViewerGrazeSolarLongitude = solGzLonSol;
            m.SolarViewerGrazeSolarLattitude = solGzLatSol;

            //
            // Solar elongation and friends
            //

            var ladeeSunMe = new double[3];            // Ladee-Sun vector in ME frame
            CSpice.spkezp_c(CSpice.SunId, etCorrected, "MOON_ME", "NONE", CSpice.LadeeId, ladeeSunMe, ref lt);
            var sunDistance = CSpice.vnorm_c(ladeeSunMe);
            CSpice.Normalize(ladeeSunMe);
            m.TelescopeSolarElongation = Math.Acos(CSpice.vdot_c(ladeeSunMe, telvecme))*180d/Math.PI;
            m.SolarViewerSolarElongation = Math.Acos(CSpice.vdot_c(ladeeSunMe, solvecme)) * 180d / Math.PI;

            var ladeeEarthMe = new double[3];
            CSpice.spkezp_c(CSpice.EarthId, etCorrected, "MOON_ME", "NONE", CSpice.LadeeId, ladeeEarthMe, ref lt);
            CSpice.Normalize(ladeeEarthMe);
            m.TelescopeEarthElongation = Math.Acos(CSpice.vdot_c(ladeeEarthMe, telvecme))*180d/Math.PI;

            //
            // Telescope Sun Vector
            //

            var telTemp = new double[3];
            CSpice.spkezp_c(CSpice.SunId, etCorrected, TelescopeFrame, "NONE", CSpice.LadeeId, telTemp, ref lt);
            CSpice.Normalize(telTemp);

            var telSunAzimuth = Math.Atan2(-telTemp[0], telTemp[2]);
            var d = Math.Sqrt(telTemp[0] * telTemp[0] + telTemp[2] * telTemp[2]);
            var telSunElevation = d < double.Epsilon ? Math.PI / 2d : Math.Atan2(telTemp[1], d);

            m.TelescopeSunAzimuth = telSunAzimuth * RadToDeg;
            m.TelescopeSunElevation = telSunElevation * RadToDeg;

            if (double.IsNaN(m.TelescopeSunElevation) || double.IsNaN(m.TelescopeSunAzimuth))
                throw new Exception("Shouldn't generate a NaN");

            //if (m.ActivityType == (byte) BaseSpectrum.ActivityTypeEnum.Occultation)
            //{
            //    Console.WriteLine("here");
            //}

            //
            // Altitude above terrain
            //

            // This is correct for the s/c altitude, but may not be correct for the others if the terrain is negative
            var terrainHeight = _terrainManager.ReadTerrainHeight(m.Lattitude, m.Longitude) / 1000d;
            m.AltitudeAboveTerrain = Math.Max(0d, m.Altitude - terrainHeight);

            // Rethink this
            terrainHeight = _terrainManager.ReadTerrainHeight(m.TelescopeGrazeLattitude, m.TelescopeGrazeLongitude) / 1000d;
            m.TelescopeGrazeAltitudeAboveTerrain = Math.Max(0d, m.TelescopeGrazeAltitude - terrainHeight);
            terrainHeight = _terrainManager.ReadTerrainHeight(m.SolarViewerGrazeLattitude, m.SolarViewerGrazeLongitude) / 1000d;
            m.SolarViewerGrazeAltitudeAboveTerrain = Math.Max(0d, m.SolarViewerGrazeAltitude - terrainHeight);

            //
            // Phase, Incidence and Emission angles
            //

            // Illumination Angles

            if (Math.Abs(m.TelescopeGrazeAltitude) < double.Epsilon)
            {

                const string method = "Ellipsoid";
                const string target = "MOON";
                const string fixref = "MOON_ME";
                const string abcorr = "NONE";
                const string obsrvr = "-12";
                var trgepc = 0d;
                var phase = 0d;
                var solar = 0d;
                var emissn = 0d;
                var srfvec = new double[3];

                CSpice.ilumin_c(method, target, etCorrected, fixref, abcorr, obsrvr, telGzPtMe, ref trgepc, srfvec,
                                ref phase, ref solar, ref emissn);

                phase *= CSpice.RadToDeg;
                solar *= CSpice.RadToDeg;
                emissn *= CSpice.RadToDeg;

                m.PhaseAngle = phase;
                m.SolarIncidenceAngle = solar;
                m.EmissionAngle = emissn;
            }
            else
            {
                m.PhaseAngle = 0d;
                m.SolarIncidenceAngle = 0d;
                m.EmissionAngle = 0d;    
            }

            CalculateGrazingAltitudesCarefully(etCorrected ,m, lposme, sunDistance, ladeeSunMe, m.AltitudeAboveTerrain);

            m.TelescopeFovGrazeAltitudeAboveTerrain = GrazingAltitudeOfFOV(lposme, m.AltitudeAboveTerrain, telvecme, FOVHalfAngle);
            m.SolarViewerFovGrazeAltitudeAboveTerrain = GrazingAltitudeOfFOV(lposme, m.AltitudeAboveTerrain, solvecme, FOVHalfAngle);

            //
            // Aperture Targets
            //

            //13-353-20:50:45.744
            //var timestring = TimeUtilities.Time42ToSTK(time);

            // Telescope graze altitude above terrain isn't correct yet.  Graze altitude clamps at 9, so negative terrain makes this 
            // go positive when it maybe shouldn't.

            if (m.TelescopeFovGrazeAltitudeAboveTerrain == 0d)   // Are we looking at surface
                m.TelescopeTarget =
                    (byte)((m.TelescopeGrazeSolarLongitude < 90 || m.TelescopeGrazeSolarLongitude > 270)
                                ? BaseSpectrum.ApertureTarget.UnlitMoon
                                : BaseSpectrum.ApertureTarget.LitMoon);
            else if (m.TelescopeEarthElongation < 2.5d)
                m.TelescopeTarget = (byte)BaseSpectrum.ApertureTarget.Earth;
            else if (m.TelescopeSolarElongation < 4d)
                m.TelescopeTarget = (byte) BaseSpectrum.ApertureTarget.Sun;
            else
                m.TelescopeTarget = (byte) BaseSpectrum.ApertureTarget.DarkSky;

            if (m.SolarViewerGrazeAltitudeAboveTerrain < 3.5d)   // Are we looking at surface
                m.SolarViewerTarget =
                    (byte)((m.SolarViewerGrazeSolarLongitude < 90 || m.SolarViewerGrazeSolarLongitude > 270)
                                ? BaseSpectrum.ApertureTarget.UnlitMoon
                                : BaseSpectrum.ApertureTarget.LitMoon);
            else if (m.SolarViewerSolarElongation < 0.8d)
                m.SolarViewerTarget = (byte)BaseSpectrum.ApertureTarget.Sun;
            else
                m.SolarViewerTarget = (byte)BaseSpectrum.ApertureTarget.DarkSky;
        }

        private void CalculateGrazingAltitudesCarefully(double etCorrected, BaseSpectrum m, double[] lposme, double sunDistance, double[] ladeeSunMe, double maxGraze)
        {

            var ladeeMoonMe = new[] { -m.MoonFixedX, -m.MoonFixedY, -m.MoonFixedZ };
            CSpice.Normalize(ladeeMoonMe);
            var rotVec = new double[3];
            CSpice.ucrss_c(ladeeSunMe, ladeeMoonMe, rotVec);

            const double sunRadius = 696342d; // km
            var sunHalfAngle = Math.Atan2(sunRadius, sunDistance);
            var rotMat = new double[3,3];
            CSpice.axisar_c(rotVec, sunHalfAngle, rotMat);
            var sunLowerEdgeVecMe = new double[3];
            CSpice.mxv_c(rotMat, ladeeSunMe, sunLowerEdgeVecMe);

            m.SunGrazeAltitudeAboveTerrain = CarefullyCalculateGrazingAltitude(lposme, sunLowerEdgeVecMe, maxGraze);

            CSpice.axisar_c(rotVec, -sunHalfAngle, rotMat);
            var sunUpperEdgeVecMe = new double[3];
            CSpice.mxv_c(rotMat, ladeeSunMe, sunUpperEdgeVecMe);

            // Calculate InSun at the timestamp
            var topOfSunGraze = CarefullyCalculateGrazingAltitude(lposme, sunUpperEdgeVecMe, maxGraze);
            var InSunAtTimestamp = topOfSunGraze == 0d ? 0 : 1;

            // Ladee State and Position in ME frame
            var ladeeState = new double[6];
            var lt = 0d;

            var etStartIntegration = etCorrected - (m.IntegrationTime / 1000d) - 0.1d;  //0.082852 xmit time (/ (* 2121 9) 230400.0)
            var etConservativeDark = etStartIntegration - 1d;
            CSpice.spkgeo_c(CSpice.LadeeId, etConservativeDark, "MOON_ME", 301, ladeeState, ref lt);
            var conservativeDarkGraze = CarefullyCalculateGrazingAltitude(ladeeState, sunUpperEdgeVecMe, maxGraze);
            var InSunConservativeDark = conservativeDarkGraze == 0d ? 0 : 2;

            var etConservativeLight = etCorrected + 1d;
            CSpice.spkgeo_c(CSpice.LadeeId, etConservativeLight, "MOON_ME", 301, ladeeState, ref lt);
            var conservativeLightGraze = CarefullyCalculateGrazingAltitude(ladeeState, sunUpperEdgeVecMe, maxGraze);
            var InSunConservativeLight = conservativeLightGraze == 0d ? 0 : 4;

            m.InSun = (byte) (InSunAtTimestamp | InSunConservativeDark | InSunConservativeLight);

            /*
            var testMat = new double[3, 3];
            var testVec = new double[3];
            CSpice.pxform_c("MOON_ME", LadeeBodyFrame, etCorrected, testMat);
            CSpice.mxv_c(testMat, sunLowerEdgeVecMe, testVec);

            ///Testing
            var dt = TimeUtilities.Time42ToDateTime(m.Timestamp);
            if (dt > new DateTime(2014, 1, 5, 22, 41, 0)) //  05 Jan 2014 22:40:08.535
                Console.WriteLine(@"here");
            */
        }

        private double GrazingAltitudeOfFOV(double[] lposme, double altAboveTerrain, double[] boresightMe, double halfAngle)
        {
            var ladeeMoonMe = new[] { -lposme[0], -lposme[1], -lposme[2] };
            CSpice.Normalize(ladeeMoonMe);
            var rotVec = new double[3];
            CSpice.ucrss_c(boresightMe, ladeeMoonMe, rotVec);

            var rotMat = new double[3, 3];
            CSpice.axisar_c(rotVec, halfAngle, rotMat);
            var fovEdge = new double[3];
            CSpice.mxv_c(rotMat, boresightMe, fovEdge);

            return CarefullyCalculateGrazingAltitude(lposme, fovEdge, altAboveTerrain);
        }

        private double CarefullyCalculateGrazingAltitude(double[] lposme, double[] vec, double altAboveTerrain)
        {
            const double step = 1d;

            var edgeGzAlt = -1d;
            var edgeGzPt = new double[3];

            CSpice.nplnpt_c(lposme, vec, OriginVector, edgeGzPt, ref edgeGzAlt);

            if ((vec[0]*(edgeGzPt[0] - lposme[0]) + vec[1]*(edgeGzPt[1] - lposme[1]) + vec[2]*(edgeGzPt[2] - lposme[2])) <
                0d)
                return altAboveTerrain;  // The closest point is behind us

            if (edgeGzAlt < MinTerrain)
                return 0d;

            double minGraze = double.MaxValue;
            double plon = 0d, plat = 0d, palt = 0d;
            var p = new[] { edgeGzPt[0], edgeGzPt[1], edgeGzPt[2] };

            var vx = vec[0] * -step;
            var vy = vec[1] * -step;
            var vz = vec[2] * -step;

            // Step towards LADEE
            while (true)
            {
                var a = AltOfPoint(p);
                CSpice.recpgr_c("MOON", p, MinTerrain, CSpice.MoonFlattening, ref plon, ref plat, ref palt);
                plon = CSpice.ToDegrees(plon);
                plat = CSpice.ToDegrees(plat);
                var terrainAlt = LunarRadiusD + _terrainManager.ReadTerrainHeight(plat, plon) / 1000d;
                var graze = a - terrainAlt;

                if (graze <= 0d)
                    return 0d;

                if (minGraze > graze)
                    minGraze = graze;

                if (a > MaxTerrain)
                    break;

                p[0] += vx;
                p[1] += vy;
                p[2] += vz;
            }

            // Go back to the closest point and step away from LADEE

            p[0] = edgeGzPt[0];
            p[1] = edgeGzPt[1];
            p[2] = edgeGzPt[2];

            vx = vec[0] * step;
            vy = vec[1] * step;
            vz = vec[2] * step;

            while (true)
            {
                var a = AltOfPoint(p);
                CSpice.recpgr_c("MOON", p, MinTerrain, CSpice.MoonFlattening, ref plon, ref plat, ref palt);
                plon = CSpice.ToDegrees(plon);
                plat = CSpice.ToDegrees(plat);
                var terrainAlt = LunarRadiusD + _terrainManager.ReadTerrainHeight(plat, plon) / 1000d;
                var graze = a - terrainAlt;

                if (graze <= 0d)
                    return 0d;

                if (minGraze > graze)
                    minGraze = graze;

                if (a > MaxTerrain)
                    break;

                p[0] += vx;
                p[1] += vy;
                p[2] += vz;
            }

            return minGraze;
        }

        private void WalkGrazingAltitude(double[] point, double[] vector, double step, double maxAlt, ref double minGraze, ref double lon, ref double lat)
        {
            var p = new[] {point[0], point[1], point[2]};
            var vx = vector[0] * step;
            var vy = vector[1] * step;
            var vz = vector[2] * step;

            var plon = 0d;
            var plat = 0d;
            var palt = 0d;

            for (var a = AltOfPoint(p); a <= maxAlt; a = AltOfPoint(p))
            {
                CSpice.recpgr_c("MOON", p, CSpice.MoonRadius, CSpice.MoonFlattening, ref plon, ref plat, ref palt);
                plon = CSpice.ToDegrees(plon);
                plat = CSpice.ToDegrees(plat);
                var terrainAlt = LunarRadiusD + _terrainManager.ReadTerrainHeight(plat, plon) / 1000d;
                var graze = palt - terrainAlt;
                if (graze < 0d)
                    Console.WriteLine("Bug in WalkGrazingAltitude");
                if (minGraze > graze)
                {
                    minGraze = graze;
                    lon = plon;
                    lat = plat;
                }
                p[0] += vx;
                p[1] += vy;
                p[2] += vz;
            }
        }

        // Not implemented yet
        private void WalkGrazeToSurface(double[] point, double[] vector, double step, double maxAlt, ref double minGraze, ref double lon, ref double lat)
        {
            var p = new[] { point[0], point[1], point[2] };
            var vx = vector[0] * step;
            var vy = vector[1] * step;
            var vz = vector[2] * step;

            var plon = 0d;
            var plat = 0d;
            var palt = 0d;

            for (var a = AltOfPoint(p); a <= maxAlt; a = AltOfPoint(p))
            {
                CSpice.recpgr_c("MOON", p, CSpice.MoonRadius, CSpice.MoonFlattening, ref plon, ref plat, ref palt);
                plon = CSpice.ToDegrees(plon);
                plat = CSpice.ToDegrees(plat);
                var terrainAlt = LunarRadiusD + _terrainManager.ReadTerrainHeight(plat, plon) / 1000d;
                var graze = palt - terrainAlt;
                if (graze < 0d)
                    Console.WriteLine("Bug in WalkGrazingAltitude");
                if (minGraze > graze)
                {
                    minGraze = graze;
                    lon = plon;
                    lat = plat;
                }
                p[0] += vx;
                p[1] += vy;
                p[2] += vz;
            }
        }

        private double AltOfPoint(double[] v)
        {
            return Math.Sqrt(v[0]*v[0] + v[1]*v[1] + v[2]*v[2]);
        }

        private void CalculateBoresight(double[,] j22Sc, double[,] j22Me, string instrumentFrame, double[] tvecj2, double[] tvecme)
        {
            var z = new[] { 0d, 0d, 1d };
            //var tstring = TimeUtilities.Time42ToSTK(time);
            //var et = TimeUtilities.Time42ToET(time);

            // Calculate rotation from SC to TelescopeFrame
            var sc2Uvstel = new double[3, 3];
            CSpice.pxform_c(LadeeBodyFrame, instrumentFrame, 0D, sc2Uvstel);  // was "LADEE_SC_PROP"

            var tvecsc = new double[3];
            CSpice.mxv_c(sc2Uvstel, z, tvecsc);

            // Calculate the rotation from J2000 to LADEE_UVS
            var j22UVS = new double[3, 3];
            //CSpice.mxm_c(J22SC, SC2UVSTEL, J22UVS);  // Think this should be right
            CSpice.mxm_c(sc2Uvstel, j22Sc, j22UVS);    // But this seems to work
            CSpice.mtxv_c(j22UVS, z, tvecj2);          // This seems to work.  Is there an offsetting error?

            var temp1 = new double[3, 3];
            var temp2 = new double[3];
            CSpice.mxm_c(j22Sc, sc2Uvstel, temp1);
            CSpice.mxv_c(temp1, z, temp2);

            // Calculate the UVS telescope pointing vector in ME frame
            CSpice.mxv_c(j22Me, tvecj2, tvecme);
            //var hack2 = new double[3];
            //CSpice.mxv_c(J22ME, hack, hack2);       // matches STK

            //tstring = tstring;


        }

        private void CalculateLimbRelatedMetadata(
            // Inputs
            double[] ladeePosMe, double[] ladeeState, double[] telBoreMe, double[,] rotMeToSolar,
            // Outputs
            out double[] gzPntMe, out double gzLongitudeMe, out double gzLattitudeMe, out double gzLonSol,
            out double gzLatSol,
            out double[] targetPointMe, out double targetAltitude
            )
        {
            // ==================================================

            var ladeeToMoonMe = new double[3];
            CSpice.vminus_c(ladeePosMe, ladeeToMoonMe);
            var limb = new CSpice.SpiceEllipse();
            var plane = new CSpice.SpicePlane();
            var limbIntersectionCount = 0;
            var grz1 = new double[3];
            var grz2 = new double[3];

            // Calculate the graze limb point
            CSpice.edlimb_c(CSpice.MoonRadius, CSpice.MoonRadius, CSpice.MoonRadius, ladeeState, ref limb);
            // First, get the limb ellipse
            CSpice.psv2pl_c(ladeeState, ladeeToMoonMe, telBoreMe, ref plane);

            // Get a plane to cut it in the ram and anti ram directions
            CSpice.inelpl_c(ref limb, ref plane, ref limbIntersectionCount, grz1, grz2); // grz1 = point in direction
            // Confirm there are two limb points
            if (limbIntersectionCount != 2)
                Console.WriteLine("Problem here");

            CSpice.Normalize(telBoreMe); // Needed?

            var vecGrz1 = new double[3];
            vecGrz1[0] = grz1[0] - ladeePosMe[0];
            vecGrz1[1] = grz1[1] - ladeePosMe[1];
            vecGrz1[2] = grz1[2] - ladeePosMe[2];
            CSpice.Normalize(vecGrz1);

            var vecGrz2 = new double[3];
            vecGrz2[0] = grz2[0] - ladeePosMe[0];
            vecGrz2[1] = grz2[1] - ladeePosMe[1];
            vecGrz2[2] = grz2[2] - ladeePosMe[2];
            CSpice.Normalize(vecGrz2);

            var ang1 = CSpice.vdot_c(telBoreMe, vecGrz1);
            var ang2 = CSpice.vdot_c(telBoreMe, vecGrz2);

            gzPntMe = ang1 > ang2 ? grz1 : grz2;

            gzLongitudeMe = 0d;
            gzLattitudeMe = 0d;
            var gzAltitudeMe = 0d;

            CSpice.recpgr_c("MOON", gzPntMe, CSpice.MoonRadius, 0d, ref gzLongitudeMe, ref gzLattitudeMe, ref gzAltitudeMe);
            //Console.WriteLine("   pnt: lon={0} lat={1} alt={2}", ToDegrees(grazeLongitude), ToDegrees(grazeLattitude), grazeAltitude);

            gzLongitudeMe = CSpice.ToDegrees(gzLongitudeMe);
            gzLattitudeMe = CSpice.ToDegrees(gzLattitudeMe);

            // Calculate the grazing point in solar lat/lon
            var gzPtSol = new double[3];
            CSpice.mxv_c(rotMeToSolar, gzPntMe, gzPtSol);

            gzLonSol = 0d;
            gzLatSol = 0d;
            var gzAltSol = 0d;
            CSpice.recpgr_c("MOON", gzPtSol, CSpice.MoonRadius, CSpice.MoonFlattening, ref gzLonSol, ref gzLatSol,
                           ref gzAltSol);
            gzLonSol = CSpice.ToDegrees(gzLonSol);
            gzLatSol = -CSpice.ToDegrees(gzLatSol);  // compensating for bug

            // Calculate the solar angle
            // Not sure of the definition yet

            // Calculate the target point in ME.  First, calculate a plane 
            var targetPlaneMe = new CSpice.SpicePlane();
            var moonCenterMe = new[] { 0d, 0d, 0d };
            var targetPlaneVec2 = new double[3];
            CSpice.ucrss_c(ladeePosMe, gzPntMe, targetPlaneVec2);
            CSpice.psv2pl_c(moonCenterMe, gzPntMe, targetPlaneVec2, ref targetPlaneMe);
            int targetPointCount = 0;
            targetPointMe = new double[3];
            CSpice.inrypl_c(ladeePosMe, telBoreMe, ref targetPlaneMe, ref targetPointCount, targetPointMe);

            // Calculate target altitude
            var targetRadius = CSpice.vnorm_c(targetPointMe);
            targetAltitude = targetRadius - CSpice.MoonRadius;
        }

        private void CalculateGrazingPointRelatedMetadata(double[] lposme, double[] telvecme, double[,] rotMeToSolar,
            double[] telGzPtMe, out double telGzLonMe, out double telGzLatMe, out double telGzAltitude, out double telGzLonSol, out double telGzLatSol)
        {
            telGzAltitude = -1d;
            CSpice.npedln_c(CSpice.MoonRadius, CSpice.MoonRadius, CSpice.MoonRadius, lposme, telvecme, telGzPtMe, ref telGzAltitude);

            telGzLonMe = telGzLatMe = -1d;
            var tempAlt = 0d;
            CSpice.recpgr_c("MOON", telGzPtMe, CSpice.MoonRadius, CSpice.MoonFlattening, ref telGzLonMe, ref telGzLatMe, ref tempAlt);
            telGzLonMe = CSpice.ToDegrees(telGzLonMe);
            telGzLatMe = CSpice.ToDegrees(telGzLatMe);

            var telGzPtSol = new double[3];
            CSpice.mxv_c(rotMeToSolar, telGzPtMe, telGzPtSol);

            telGzLonSol = telGzLatSol = -1d;

            CSpice.recpgr_c("MOON", telGzPtSol, CSpice.MoonRadius, CSpice.MoonFlattening, ref telGzLonSol, ref telGzLatSol, ref tempAlt);
            telGzLatSol = -CSpice.ToDegrees(telGzLatSol);  // compensating for bug
            telGzLonSol = CSpice.ToDegrees(telGzLonSol);
        }


        // Validated against Boris' fortran example
        public void Spice2StkQuat(double[] qspice, double[] qstk)
        {
            qstk[0] = -qspice[1];
            qstk[1] = -qspice[2];
            qstk[2] = -qspice[3];
            qstk[3] = qspice[0];
        }

        public void Stk2SpiceQuat(double[] qstk, double[] qspice)
        {
            qspice[0] = qstk[3];
            qspice[1] = -qstk[0];
            qspice[2] = -qstk[1];
            qspice[3] = -qstk[2];
        }

    }
}

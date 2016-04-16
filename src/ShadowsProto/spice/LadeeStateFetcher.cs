using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OpenTK;

namespace Shadow.spice
{
    public class LadeeStateFetcher
    {
        public enum StateFrame
        {
            EarthFixed,
            MoonFixed,
            EarthCenteredJ2000,
            EarthCenteredMoonFollowing,
            SelenocentricSolarEcliptic,
            LadeeSolarEcliptic,
            LadeeLVLH,
            RP_Landing_Site
        }

        public const int LadeeAttitudeId = -12000;
        public const int LadeeId = -12;
        public const int EarthId = 399;
        public const int MoonId = 301;
        public const int SunId = 10;
        public const double DegToRad = Math.PI / 180d;
        public const double RadToDeg = 180d / Math.PI;
        public const double LunarRadiusD = 1737.4d;
        public const float LunarRadius = 1737.4f;
        public const float MinTerrain = -6.6695f + LunarRadius;
        public const float MaxTerrain = 10.774f + LunarRadius;

        public const string J2000Frame = "J2000";
        public const string MoonFrame = "MOON_ME";
        public const string LadeeBodyFrame = "LADEE_SC_PROP";
        public const string TelescopeFrame = "LADEE_UVSTEL";
        public const string SolarViewerFrame = "LADEE_UVSSOL";
        public const string DEMFrame = "RP_SITE1_TOPO";
        public const int DEMId = 397990; // 1397990;
        public const double FOVHalfAngle = 0.5d * Math.PI / 180d; // Half angle for both telescope and solar viewer

        public static LadeeStateFetcher StateFetcher;

        public long LatestSpiceTime42;
        public double[] OriginVector = new[] { 0d, 0d, 0d };
        public CSpice Spice;
        public string SpiceRoot;

        public DateTime SpiceStartDate = new DateTime(2013, 9, 8, 0, 0, 0);
        public DateTime SpiceStopDate = new DateTime(2014, 2, 9, 17, 0, 0);
        private StateFrame _frame = StateFrame.EarthFixed;
        private string _spiceFrame = DEMFrame; //  J2000Frame;
        private int _spiceObserver = EarthId;

        #region Initialization

        public LadeeStateFetcher(string spiceRoot)
        {
            Spice = new CSpice();
            //Spice.LoadStandardKernels();
            FurnishSpiceKernels(spiceRoot, "metakernel.txt");
            SpiceRoot = spiceRoot;
        }

        public static LadeeStateFetcher GetFetcher(string spiceRoot)
        {
            if (StateFetcher != null)
                return StateFetcher;
            StateFetcher = new LadeeStateFetcher(spiceRoot);
            return StateFetcher;
        }

        public static LadeeStateFetcher GetFetcher()
        {
            return StateFetcher;
        }

        // I used to use a spice metakernel, but that didn't interact well with my relative path
        // infrastructure.  So, I've implemented a simple version of its functionality
        public void FurnishSpiceKernels(string spiceKernelRoot, string rootFile)
        {
            var metakernelPath = Path.Combine(spiceKernelRoot, rootFile);
            foreach (var line in File.ReadAllLines(metakernelPath).Where(line => !string.IsNullOrEmpty(line) && line[0] != ' '))
                Furnish(Path.Combine(spiceKernelRoot, CanonicalizeDirectorySeparators(line)));
        }

        public static string CanonicalizeDirectorySeparators(string path)
        {
            path = path.Replace('/', Path.DirectorySeparatorChar);
            path = path.Replace('\\', Path.DirectorySeparatorChar);
            return path;
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

        #endregion Initialization

        #region Frames

        public StateFrame Frame
        {
            get { return _frame; }
            set
            {
                _frame = value;
                switch (_frame)
                {
                    case StateFrame.EarthCenteredJ2000:
                    case StateFrame.EarthFixed:
                        _spiceFrame = J2000Frame;
                        _spiceObserver = EarthId;
                        break;
                    case StateFrame.SelenocentricSolarEcliptic:
                        _spiceFrame = "LADEE_SSE";
                        _spiceObserver = MoonId;
                        break;
                    case StateFrame.EarthCenteredMoonFollowing:
                        _spiceFrame = "LADEE_MF";
                        _spiceObserver = MoonId;
                        break;
                    case StateFrame.LadeeLVLH:
                        _spiceFrame = "LADEE_LVLH";
                        _spiceObserver = MoonId;
                        break;
                    case StateFrame.MoonFixed:
                        _spiceFrame = MoonFrame;
                        _spiceObserver = MoonId;
                        break;
                    case StateFrame.RP_Landing_Site:
                    default:
                        _spiceFrame = DEMFrame;
                        _spiceObserver = DEMId;
                        break;
                }
            }
        }

        #endregion Frames

        public bool SpiceAttitudeAtTime(long rawTime42, double[] qspice)
        {
            var sclkdp = (double)rawTime42;
            const int sc1 = -12000;
            var cmat = new double[3, 3];
            double clkout = 0d;
            int found = 0;
            CSpice.ckgp_c(sc1, sclkdp, 0d, J2000Frame, cmat, ref clkout, ref found);

            if (found == 0)
            {
                //var s = TimeUtilities.Time42ToString((long) sclkdp);
                //Console.Error.WriteLine(@"Couldn't find an attitude at time {0}, {1}", sclkdp, s);
                return false;
            }

            CSpice.m2q_c(cmat, qspice);
            return true;
        }

        private void sct2e_c(int id, long rawClock, ref double etCorrected)
        {
            etCorrected = TimeUtilities.Time42ToET(rawClock);
        }

        public void Fetch(long rawTime42, Vector3d position, Quaternion attitude)
        {
            if (rawTime42 == 0)
                Console.Error.WriteLine("Time is 0.");
            const int scId = CSpice.LadeeId;
            var sclkdp = (double)rawTime42;
            var etCorrected = 0d;
            //CSpice.sct2e_c(scId, sclkdp, ref etCorrected);
            sct2e_c(scId, rawTime42, ref etCorrected);

            long trueTime42 = TimeUtilities.ETToTime42(etCorrected);
            var qspice = new double[4];
            SpiceAttitudeAtTime(trueTime42, qspice);

            attitude.W = (float)qspice[0];
            attitude.X = (float)qspice[1];
            attitude.Y = (float)qspice[2];
            attitude.Z = (float)qspice[3];

            // Ladee State and Position in ME frame
            var ladeeState = new double[6];
            double lt = 0d;

            // The LADEE's state in ME frame
            CSpice.spkgeo_c(CSpice.LadeeId, etCorrected, _spiceFrame, _spiceObserver, ladeeState, ref lt);
            position.X = ladeeState[0];
            position.Y = ladeeState[1];
            position.Z = ladeeState[2];
        }

        public void FetchPosition(int id, long rawTime42, ref Vector3d position)
        {
            var sclkdp = (double)rawTime42;
            var etCorrected = 0d;
            //CSpice.sct2e_c(CSpice.LadeeId, sclkdp, ref etCorrected);
            sct2e_c(CSpice.LadeeId, rawTime42, ref etCorrected);

            var ladeeState = new double[6];
            var lt = 0d;
            CSpice.spkgeo_c(id, etCorrected, _spiceFrame, _spiceObserver, ladeeState, ref lt);


            //var sb = new StringBuilder();
            //CSpice.et2utc_c(etCorrected, "C", 14, 40, sb);
            //Console.WriteLine(@"FetchPosition etCorrected={0}", sb);
            //CSpice.spkpos_c("-12", etCorrected, _spiceFrame, "NONE", "MOON", ladeeState, ref lt);


            position.X = ladeeState[0];
            position.Y = ladeeState[1];
            position.Z = ladeeState[2];
        }

        public bool FetchPositionAndAttitude(int id, long rawTime42, ref Vector3d position, ref Quaternion q)
        {
            //var dt = TimeUtilities.Time42ToDateTime(rawTime42);
            //Console.WriteLine(@"Fetch pos and att at {0}", dt);
            //if (dt.Year == 2013)
            //    Console.WriteLine(@"here");

            var sclkdp = (double)rawTime42;
            var etCorrected = 0d;
            //CSpice.sct2e_c(CSpice.LadeeId, sclkdp, ref etCorrected);
            sct2e_c(CSpice.LadeeId, rawTime42, ref etCorrected);

            var ladeeState = new double[6];
            var lt = 0d;
            CSpice.spkgeo_c(id, etCorrected, _spiceFrame, _spiceObserver, ladeeState, ref lt);
            position.X = ladeeState[0];
            position.Y = ladeeState[1];
            position.Z = ladeeState[2];

            var cmat = new double[3, 3];
            if (id == LadeeId)
            {
                const int attId = -12000;
                var clkout = 0d;
                var found = 0;
                CSpice.ckgp_c(attId, sclkdp, 0d, _spiceFrame, cmat, ref clkout, ref found);
                if (found == 0)
                    return false;
            }
            else
            {
                if (id == MoonId)
                    CSpice.pxform_c(_spiceFrame, "MOON_ME", etCorrected, cmat);
                else if (id == EarthId)
                    CSpice.pxform_c(_spiceFrame, "IAU_EARTH", etCorrected, cmat);
                else if (id == SunId)
                    CSpice.pxform_c(_spiceFrame, "IAU_SUN", etCorrected, cmat);
            }

            var qspice = new double[4];
            CSpice.m2q_c(cmat, qspice);

            q.W = (float)qspice[0];
            q.X = (float)qspice[1];
            q.Y = (float)qspice[2];
            q.Z = (float)qspice[3];
            return true;
        }

        public bool FetchLADEEPositionVelocityAndAttitude(long rawTime42, ref Vector3d position, ref Quaternion q, ref Vector3d velocity)
        {
            //var dt = TimeUtilities.Time42ToDateTime(rawTime42);
            //Console.WriteLine(@"Fetch pos and att at {0}", dt);
            //if (dt.Year == 2013)
            //    Console.WriteLine(@"here");

            var sclkdp = (double)rawTime42;
            var etCorrected = 0d;
            //CSpice.sct2e_c(CSpice.LadeeId, sclkdp, ref etCorrected);
            sct2e_c(CSpice.LadeeId, rawTime42, ref etCorrected);

            var ladeeState = new double[6];
            var lt = 0d;
            CSpice.spkgeo_c(LadeeStateFetcher.LadeeId, etCorrected, _spiceFrame, _spiceObserver, ladeeState, ref lt);
            position.X = ladeeState[0];
            position.Y = ladeeState[1];
            position.Z = ladeeState[2];

            velocity.X = ladeeState[3];
            velocity.Y = ladeeState[4];
            velocity.Z = ladeeState[5];

            //Console.WriteLine(@"--Vel=[{0},{1},{2}]", ladeeState[3], ladeeState[4], ladeeState[5]);
            velocity.NormalizeFast();
            //Console.WriteLine(@"  Vel=[{0},{1},{2}]", velocity.X, velocity.Y, velocity.Z);

            var cmat = new double[3, 3];

            const int attId = -12000;
            var clkout = 0d;
            var found = 0;
            CSpice.ckgp_c(attId, sclkdp, 0d, _spiceFrame, cmat, ref clkout, ref found);
            if (found == 0)
                return false;

            var qspice = new double[4];
            CSpice.m2q_c(cmat, qspice);

            q.W = (float)qspice[0];
            q.X = (float)qspice[1];
            q.Y = (float)qspice[2];
            q.Z = (float)qspice[3];
            return true;
        }

        public bool FetchPositionAndAttitudeInMoonMe(int id, long rawTime42, ref Vector3d position,
                                                     ref Vector3d velocity, ref Quaternion q)
        {
            var sclkdp = (double)rawTime42;
            double etCorrected = 0d;
            //CSpice.sct2e_c(CSpice.LadeeId, sclkdp, ref etCorrected);
            sct2e_c(CSpice.LadeeId, rawTime42, ref etCorrected);

            var ladeeState = new double[6];
            double lt = 0d;
            CSpice.spkgeo_c(id, etCorrected, MoonFrame, MoonId, ladeeState, ref lt);
            position.X = ladeeState[0];
            position.Y = ladeeState[1];
            position.Z = ladeeState[2];
            velocity.X = ladeeState[3];
            velocity.Y = ladeeState[4];
            velocity.Z = ladeeState[5];

            //
            var cmat = new double[3, 3];

            if (id == LadeeId)
            {
                const int attId = -12000;
                double clkout = 0d;
                int found = 0;
                CSpice.ckgp_c(attId, sclkdp, 0d, MoonFrame, cmat, ref clkout, ref found);
                if (found == 0)
                    return false;
            }
            else
            {
                if (id == MoonId)
                    CSpice.pxform_c(MoonFrame, "MOON_ME", etCorrected, cmat);
                else if (id == EarthId)
                    CSpice.pxform_c(MoonFrame, "IAU_EARTH", etCorrected, cmat);
                else if (id == SunId)
                    CSpice.pxform_c(MoonFrame, "IAU_SUN", etCorrected, cmat);
            }

            var qspice = new double[4];
            CSpice.m2q_c(cmat, qspice);

            q.W = (float)qspice[0];
            q.X = (float)qspice[1];
            q.Y = (float)qspice[2];
            q.Z = (float)qspice[3];
            return true;
        }

        public bool FetchJ200Attitude(long rawTime42, ref Quaternion q)
        {
            var sclkdp = (double)rawTime42;
            var etCorrected = 0d;
            //CSpice.sct2e_c(CSpice.LadeeId, sclkdp, ref etCorrected);
            sct2e_c(CSpice.LadeeId, rawTime42, ref etCorrected);

            var cmat = new double[3, 3];
            CSpice.pxform_c(_spiceFrame, "J2000", etCorrected, cmat);

            var qspice = new double[4];
            CSpice.m2q_c(cmat, qspice);

            q.W = (float)qspice[0];
            q.X = (float)qspice[1];
            q.Y = (float)qspice[2];
            q.Z = (float)qspice[3];
            return true;
        }

        // Validated against Boris' fortran example
        public void Spice2StkQuat(double[] qspice, double[] qstk)
        {
            qstk[0] = -qspice[1]; // x
            qstk[1] = -qspice[2]; // y
            qstk[2] = -qspice[3]; // z
            qstk[3] = qspice[0]; // w
        }

        public void Stk2SpiceQuat(double[] qstk, double[] qspice)
        {
            qspice[0] = qstk[3]; // w
            qspice[1] = -qstk[0]; // x
            qspice[2] = -qstk[1]; // y
            qspice[3] = -qstk[2]; // z
        }
    }
}
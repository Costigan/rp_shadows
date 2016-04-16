using System;
using System.Collections.Generic;
using Shadow.spice;
using OpenTK;

namespace Shadow.viz
{
    public class World
    {
        public enum LODGuideType
        {
            LADEE,
            Telescope,
            SolarViewer
        };

        public readonly List<Shape> FarShapes = new List<Shape>();
        public readonly List<Shape> NearShapes = new List<Shape>();

        public readonly List<OpenGLControlWrapper> Wrappers = new List<OpenGLControlWrapper>();
        public TexturedBall Earth;

        public LadeeStateFetcher Fetcher;
        public LODGuideType LODGuide = LODGuideType.Telescope;
        public double LadeeMoonAlt;
        public double LadeeMoonLat, LadeeMoonLon;

        public Vector3d LadeePositionMe;
        public Quaternion LadeeQuaternionMe;
        public Vector3d LadeeToMoonNormalized;
        public Vector3d LadeeVelocityMe;
        public Vector3d LadeeVelocityMeNormalized;
        public Shape Moon;
        public Ball CentralBall;
        public string SpiceDir;
        public StarBackground Stars;
        public TexturedBall Sun;
        public Vector3d TempVector;
        private long _time;

        public World(string spiceDir)
        {
            SpiceDir = spiceDir;
            Fetcher = LadeeStateFetcher.GetFetcher(SpiceDir);
        }

        public long Time
        {
            get { return _time; }
            set { Update(value); }
        }

        public LadeeStateFetcher.StateFrame Frame
        {
            get { return Fetcher.Frame; }
            set { Fetcher.Frame = value; }
        }

        public void Tick()
        {
            Fetcher.FetchJ200Attitude(_time, ref Stars.Attitude);
            Fetcher.FetchPositionAndAttitude(LadeeStateFetcher.EarthId, _time, ref Earth.Position, ref Earth.Attitude);
            Fetcher.FetchPositionAndAttitude(LadeeStateFetcher.MoonId, _time, ref Moon.Position, ref Moon.Attitude);
            Fetcher.FetchPositionAndAttitude(LadeeStateFetcher.SunId, _time, ref Sun.Position, ref Sun.Attitude);

            //var success = Fetcher.FetchPositionAndAttitudeInMoonMe(LadeeStateFetcher.LadeeId, _time,
            //                                                        ref LadeePositionMe, ref LadeeVelocityMe,
            //                                                        ref LadeeQuaternionMe);

            var success = false;
            if (success)
                UpdateMoonLOD(_time);

            var count = Wrappers.Count;
            for (var i = 0; i < count; i++)
            {
                var w = Wrappers[i];
                w.CameraMode.Tick();
                w.Invalidate();
            }
        }

        public void UpdateMoonLOD(long time)
        {
            double lat, lon;
            switch (LODGuide)
            {
                case LODGuideType.Telescope:
                    CalculateBoresightGrazingPointLatLon("LADEE_UVSTEL", time, out lat, out lon);
                    break;
                case LODGuideType.SolarViewer:
                    CalculateBoresightGrazingPointLatLon("LADEE_UVSSOL", time, out lat, out lon);
                    break;
                case LODGuideType.LADEE:
                default:
                    CalculateLadeeLatLon(time, out lat, out lon);
                    break;
            }
            if (lon > 180d)
                lon -= 360d;
            //Console.WriteLine(@"[lat,lon]=[{0},{1}]", lat, lon);
            var moon = Moon as MoonDEM;
            if (moon != null)
                moon.SetLevelOfDetail(lat, lon);
        }

        protected void CalculateLadeeLatLon(long time, out double lat, out double lon)
        {
            var lposme = new[] {LadeePositionMe.X, LadeePositionMe.Y, LadeePositionMe.Z};
            CSpice.recpgr_c("MOON", lposme, CSpice.MoonRadius, 0d, ref LadeeMoonLon, ref LadeeMoonLat, ref LadeeMoonAlt);
            LadeeMoonLon *= 180d/Math.PI;
            LadeeMoonLat *= 180d/Math.PI;
            lat = LadeeMoonLat;
            lon = LadeeMoonLon;
        }

        protected void CalculateBoresightGrazingPointLatLon(string instrumentFrame, long time, out double lat,
                                                            out double lon)
        {
            var et = TimeUtilities.Time42ToET(time);
            // Calculate rotation from SC to TelescopeFrame
            var mat = new double[3,3];
            CSpice.pxform_c(instrumentFrame, "MOON_ME", et, mat);

            var z = new[] {0d, 0d, 1d}; // Z axis
            var vec = new double[3]; // instrument boresight in ME frame
            CSpice.mxv_c(mat, z, vec);

            var lposme = new[] {LadeePositionMe.X, LadeePositionMe.Y, LadeePositionMe.Z};
            var gzPtMe = new double[3];
            var gzAltitude = -1d;
            CSpice.npedln_c(CSpice.MoonRadius, CSpice.MoonRadius, CSpice.MoonRadius, lposme, vec, gzPtMe, ref gzAltitude);

            var gzLonMe = -1d;
            var gzLatMe = -1d;
            var tempAlt = 0d;
            CSpice.recpgr_c("MOON", gzPtMe, CSpice.MoonRadius, CSpice.MoonFlattening, ref gzLonMe, ref gzLatMe,
                            ref tempAlt);
            gzLonMe = CSpice.ToDegrees(gzLonMe);
            gzLatMe = CSpice.ToDegrees(gzLatMe);
            lat = gzLatMe;
            lon = gzLonMe;
        }

        public void Update(long t)
        {
            _time = t;
            Tick();
        }
    }
}
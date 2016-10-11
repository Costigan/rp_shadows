using System;
using System.IO;
using System.Linq;

namespace LightMap.spice
{
    /// <summary>
    /// Wraps spice and doesn't load it if running in 32-bit mode.
    /// </summary>
    public class SpiceManager
    {
        SpiceMethods Methods;
        public SpiceManager()
        {
            if (Program.Verbose)
                Console.WriteLine("Is64BitProcess={0}", Environment.Is64BitProcess);
            Methods = Environment.Is64BitProcess && Environment.Is64BitOperatingSystem ? new FullSpiceMethods() : (SpiceMethods)new DummySpiceMethods();
        }

        public void SunEarthAzEl(DateTime time, out double sunAzimuth, out double sunElevation, out double earthAzimuth, out double earthElevation)
        {
            Methods.SunEarthAzEl(time, out sunAzimuth, out sunElevation, out earthAzimuth, out earthElevation);
        }
    }

    public abstract class SpiceMethods
    {
        public static DateTime Epoch = new DateTime(2000, 1, 1, 11, 58, 55, 816);
        public const int EarthId = 399;
        public const int MoonId = 301;
        public const int SunId = 10;

        public abstract void SunEarthAzEl(DateTime time, out double sunAzimuth, out double sunElevation, out double earthAzimuth, out double earthElevation);

        public static double DateTimeToET(DateTime time)
        {
            return (time - Epoch).TotalSeconds + 3d;
            // The 3D accounts for leap seconds since 2000.  This is valid only for dates after Jul 1 2012.
        }
    }

    public class DummySpiceMethods : SpiceMethods
    {
        public override void SunEarthAzEl(DateTime time, out double sunAzimuth, out double sunElevation, out double earthAzimuth, out double earthElevation)
        {
            sunAzimuth = sunElevation = earthAzimuth = earthElevation = 0d;
        }
    }

    public class FullSpiceMethods : SpiceMethods
    {
        public FullSpiceMethods()
        {
            FurnishSpiceKernels("", "kernels/metakernel.txt");
        }

        public void FurnishSpiceKernels(string spiceKernelRoot, string rootFile)
        {
            string metakernelPath = Path.Combine(spiceKernelRoot, rootFile);
            foreach (string line in File.ReadAllLines(metakernelPath).Where(line => !string.IsNullOrEmpty(line) && line[0] != ' ' && line[0] != '#'))
                Furnish(Path.Combine(spiceKernelRoot, CanonicalizeDirectorySeparators(line)));
            if (File.Exists(Program.SiteFrameFilename))
                Furnish(Program.SiteFrameFilename);
        }

        public static void Furnish(string path)
        {
            if (Program.Verbose)
                Console.WriteLine(@"Spice furnishing {0}", path);
            CSpice.furnsh_c(path);
        }

        public static string CanonicalizeDirectorySeparators(string path)
        {
            path = path.Replace('/', Path.DirectorySeparatorChar);
            path = path.Replace('\\', Path.DirectorySeparatorChar);
            return path;
        }

        public override void SunEarthAzEl(DateTime time, out double sunAzimuth, out double sunElevation, out double earthAzimuth, out double earthElevation)
        {
            //sunAzimuth = sunElevation = earthAzimuth = earthElevation = 0d;
            //return;

            var et = DateTimeToET(time);
            var state = new double[6];
            double lt = 0d;
            double[] pos = new double[] { 0d, 0d, 0d };

            CSpice.spkgeo_c(SunId, et, "MOON_ME", MoonId, state, ref lt);

            CSpice.spkcpo_c("SUN", et, "SITE_TOPO", "CENTER", "NONE", pos, "MOON", "MOON_ME", state, ref lt);
            //var d = Math.Sqrt(state[0] * state[0] + state[1] * state[1] + state[2] * state[2]);
            //Console.WriteLine(@"HERMITE: Sun= dist={3} vec=[{0},{1},{2}]", state[0], state[1], state[2], d);

            sunAzimuth = -Math.Atan2(state[1], state[0]);
            var flatd = Math.Sqrt(state[0] * state[0] + state[1] * state[1]);
            sunElevation = Math.Atan2(state[2], flatd);

            CSpice.spkcpo_c("EARTH", et, "SITE_TOPO", "CENTER", "NONE", pos, "MOON", "MOON_ME", state, ref lt);
            //d = Math.Sqrt(state[0] * state[0] + state[1] * state[1] + state[2] * state[2]);
            //Console.WriteLine(@"HERMITE: Earth= dist={3} vec=[{0},{1},{2}]", state[0], state[1], state[2], d);

            earthAzimuth = -Math.Atan2(state[1], state[0]);
            flatd = Math.Sqrt(state[0] * state[0] + state[1] * state[1]);
            earthElevation = Math.Atan2(state[2], flatd);
        }
    }
}

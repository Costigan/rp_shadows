using CommandLineParser.Arguments;
using CommandLineParser.Exceptions;
using LightMap.math;
using LightMap.raycaster;
using OSGeo.GDAL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightMap
{
    public class Program
    {
        public const string SiteFrameFilename = "site.fk";

        public static bool Verbose = false;
        public static bool EstimateTime = false;

        public bool ParsingTest = false;
        public string DEMFilename = "dem.tiff";
        public string OutputFilename = "lightmap.tiff";
        public DateTime Timestamp = DateTime.Now;
        public bool ReplaceOutputFile = false;

        public CommandLineParser.CommandLineParser Parser;
        public float RayDensity = 1f;
        public int SunRayVerticalCount = 1;
        public int SunRayHorizontalCount = 1;

        public string Projection;
        public double[] AffineTransform;
        public int Width = -1;
        public int Height = -1;
        public int Stride;

        static void Main(string[] args)
        {
            (new Program()).Parse(args);
        }

        void Parse(string[] args)
        {
            Parser = new CommandLineParser.CommandLineParser();

            //var showArgument = new SwitchArgument('s', "show", "Set whether show or not", true);
            //var version = new ValueArgument<decimal>('v', "version", "Set desired version");
            //var color = new EnumeratedValueArgument<string>('c', "color", new string[] { "red", "green", "blue" });

            var demFilenameArg = new ValueArgument<string>('d', "dem", "DEM Filename");
            var outputFilenameArg = new ValueArgument<string>('o', "output", "Output lightmap filename");
            var timestampArg = new ValueArgument<DateTime>('t', "timestamp", "Timestamp for calculating the sun angle");
            var parsingTestArg = new SwitchArgument('u', "debug", "Switch indicating don't run the lightmap generator", false);
            var verboseArg = new SwitchArgument('v', "verbose", "Print extra messages", false);
            var replaceArg = new SwitchArgument('r', "replace", "Replace output file", false);
            var areaCountArg = new ValueArgument<int>('a', "area", "Number of horizontal and vertical slices to divide the sun into");
            var widthArg = new ValueArgument<int>('w', "width", "Clip the DEM to this width");
            var heightArg = new ValueArgument<int>('h', "height", "Clip the DEM to this height");
            var estimateTimeArg = new SwitchArgument('e', "estimate", "Estimate the time-per-ray needed to generate this lightmap but don't generate it", false);
            var superResolution = new ValueArgument<int>('s', "superresolution", "Number of rays per pixel of resolution (1-axis).  Defaults to 1.");

            Parser.Arguments.Add(demFilenameArg);
            Parser.Arguments.Add(outputFilenameArg);
            Parser.Arguments.Add(timestampArg);
            Parser.Arguments.Add(parsingTestArg);
            Parser.Arguments.Add(replaceArg);
            Parser.Arguments.Add(verboseArg);
            Parser.Arguments.Add(areaCountArg);
            Parser.Arguments.Add(widthArg);
            Parser.Arguments.Add(heightArg);
            Parser.Arguments.Add(estimateTimeArg);
            Parser.Arguments.Add(superResolution);

            try
            {
                Parser.ParseCommandLine(args);
                Parser.ShowParsedArguments();

                if (parsingTestArg.Parsed)
                    ParsingTest = parsingTestArg.Value;
                if (demFilenameArg.Parsed)
                    DEMFilename = demFilenameArg.Value;
                if (timestampArg.Parsed)
                    Timestamp = timestampArg.Value;
                if (outputFilenameArg.Parsed)
                    OutputFilename = outputFilenameArg.Value;
                if (replaceArg.Parsed)
                    ReplaceOutputFile = replaceArg.Value;
                if (verboseArg.Parsed)
                    Verbose = verboseArg.Value;
                if (areaCountArg.Parsed)
                {
                    SunRayVerticalCount = areaCountArg.Value;
                    SunRayHorizontalCount = SunRayVerticalCount;
                }
                if (superResolution.Parsed)
                    RayDensity = superResolution.Value;
                if (widthArg.Parsed)
                    Width = widthArg.Value;
                if (heightArg.Parsed)
                    Height = heightArg.Value;

                if (!File.Exists(DEMFilename))
                    ShowError(string.Format("The DEM {0} file doesn't exist", DEMFilename));
                if (!ReplaceOutputFile && File.Exists(OutputFilename))
                    ShowError(string.Format("The lightmap file already exists.  Use -o or --overwrite to replace it."));
                if (estimateTimeArg.Parsed)
                    EstimateTime = estimateTimeArg.Value;

                GdalConfiguration.ConfigureGdal();
                float[,] data = GetFloatArray();
                if (data == null)
                    ShowError(string.Format("Couldn't read float data from {0}", DEMFilename));

                // Clip the terrain if necessary
                if (Width < 0)
                {
                    Width = data.GetLength(0);  // Is this right?
                    Height = data.GetLength(1);
                }
                else
                {
                    var data2 = new float[Width, Height];
                    int w = Math.Min(Width, data.GetLength(0));
                    int h = Math.Min(Height, data.GetLength(1));
                    for (int y = 0; y < h; y++)
                        for (int x = 0; x < w; x++)
                            data2[x, y] = data[x, y];
                    data = data2;
                    Width = w;
                    Height = h;
                }

                // Calculate the sun vector
                Vector3d sunvec = CalculateSunVector();

                Console.WriteLine(@"Sun vector=[{0}, {1}, {2}]", sunvec.X, sunvec.Y, sunvec.Z);

                var terrain = new Terrain { HeightMap = data };
                terrain.SetMinMax();
                terrain.Clear();

                var UseClipper = false;
                if (UseClipper)
                    terrain.UpdateToSunV3(sunvec, 1f, (float)(0.25d * Math.PI / 180d), RayDensity, SunRayVerticalCount, SunRayHorizontalCount);
                else
                    terrain.UpdateToSunV4(sunvec, 1f, (float)(0.25d * Math.PI / 180d), RayDensity, SunRayVerticalCount, SunRayHorizontalCount);

                if (!EstimateTime)
                {
                    var bitmap = terrain.ShadowBufferToScaledImageV4();
                    bitmap.Save(OutputFilename);
                    if (Verbose)
                        Console.WriteLine(@"Done.");
                }
            }
            catch (CommandLineException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        void ShowError(string msg)
        {
            Console.WriteLine(msg);
            Parser.ShowUsage();
            Environment.Exit(-1);
        }

        float[,] GetFloatArray()
        {
            using (Dataset dataset = Gdal.Open(DEMFilename, Access.GA_ReadOnly))
            {
                if (dataset == null)
                    throw new Exception("Couldn't open the DEM file");
                if (dataset.RasterCount < 1)
                    throw new Exception(@"There are no bands in the DEM file");
                int width = dataset.RasterXSize;
                int height = dataset.RasterYSize;
                Projection = dataset.GetProjectionRef();
                AffineTransform = new double[6];
                dataset.GetGeoTransform(AffineTransform);

                Band band = dataset.GetRasterBand(1);
                int blockX, blockY;
                band.GetBlockSize(out blockX, out blockY);
                switch (band.DataType)
                {
                    case DataType.GDT_Float32:
                        {
                            //band.ReadRaster()
                            var data = new float[height, width];
                            System.Runtime.InteropServices.GCHandle pinnedData = System.Runtime.InteropServices.GCHandle.Alloc(data, System.Runtime.InteropServices.GCHandleType.Pinned);
                            IntPtr pointer = pinnedData.AddrOfPinnedObject();
                            band.ReadRaster(0, 0, width, height, pointer, width, height, DataType.GDT_Float32, 0, 0);
                            pinnedData.Free();
                            band.Dispose();
                            return data;
                        }
                    case DataType.GDT_Int16:
                    default:
                        return null;
                }
            }
        }

        Vector3d CalculateSunVector()
        {
            double lat, lon;
            CalculateLatLonOfDEMCenter(out lat, out lon);
            if (Verbose)
                Console.WriteLine(@"Center of map: lat={0} lon={1}", lat, lon);
            CreateSiteFrameFile(lat, lon);

            var manager = new spice.SpiceManager();
            double sunAzimuth, sunElevation, earthAzimuth, earthElevation;
            manager.SunEarthAzEl(Timestamp, out sunAzimuth, out sunElevation, out earthAzimuth, out earthElevation);
            if (Verbose)
            {
                Console.WriteLine(@"Sun Azimuth     = {0}", ToDeg(sunAzimuth));
                Console.WriteLine(@"Sun Elevation   = {0}", ToDeg(sunElevation));
                Console.WriteLine(@"Earth Azimuth   = {0}", ToDeg(earthAzimuth));
                Console.WriteLine(@"Earth Elevation = {0}", ToDeg(earthElevation));
            }

            double z = Math.Sin(sunElevation);
            double d = Math.Cos(sunElevation);
            double x = d * Math.Cos(sunAzimuth);
            double y = d * Math.Sin(sunAzimuth);

            return new Vector3d(x, y, z);
        }

        double ToDeg(double rad) => rad * 180d / Math.PI;

        void CalculateLatLonOfDEMCenter(out double lat, out double lon)
        {
            if (Projection == null)
                throw new Exception("Projection hasn't been initialized");

            string latlonProjection = "GEOGCS[\"Moon 2000\", DATUM[\"D_Moon_2000\", SPHEROID[\"Moon_2000_IAU_IAG\",1737400.0,0.0]], PRIMEM[\"Greenwich\",0], UNIT[\"Decimal_Degree\",0.0174532925199433]]";

            var src = new OSGeo.OSR.SpatialReference(Projection);
            var dst = new OSGeo.OSR.SpatialReference(latlonProjection);

            var pixelToLatLon = new OSGeo.OSR.CoordinateTransformation(src, dst);

            if (pixelToLatLon == null || AffineTransform == null)
                throw new Exception(@"No coordinate transform has been provided to go from pixels to lat/lon");

            double pixelX = Width / 2d;
            double pixelY = Height / 2d;

            double x = pixelX * AffineTransform[1] + AffineTransform[0];
            double y = pixelY * AffineTransform[5] + AffineTransform[3];

            var o = new double[3];
            pixelToLatLon.TransformPoint(o, x, y, 0d);
            lat = o[1];
            lon = o[0];
        }

        private void CreateSiteFrameFile(double lat, double lon, string siteFrameFilename = SiteFrameFilename)
        {
            double normalized_lat = lat;
            double normalized_lon = NormalizeLon(lon);
            double colat = 90d - normalized_lat;
            double minus_colat = -colat;
            double minus_lon = -normalized_lon;

            string template = "This is a site frame\r\n\r\nlat = {0} from gdal\r\nlon = {1}\r\nnormalized lat ={2}\r\nnormalized lon ={3}\r\ncolat = {4}\r\n\r\n\\begindata\r\n\r\n   FRAME_SITE_TOPO = 6000001\r\n   FRAME_6000001_NAME = 'SITE_TOPO'\r\n   FRAME_6000001_CLASS = 4\r\n   FRAME_6000001_CLASS_ID = 6000001\r\n   FRAME_6000001_CENTER = 399017\r\n\r\n   OBJECT_399017_FRAME = 'SITE_TOPO'\r\n\r\n   TKFRAME_SITE_TOPO_RELATIVE = 'MOON_ME'\r\n   TKFRAME_SITE_TOPO_SPEC = 'ANGLES'\r\n   TKFRAME_SITE_TOPO_UNITS = 'DEGREES'\r\n   TKFRAME_SITE_TOPO_AXES = (3, 2, 3 )\r\n   TKFRAME_SITE_TOPO_ANGLES = ( {5}\r\n                     {6}\r\n                     180.0 )\r\n\\begintext\r\n";
            string text = string.Format(template,
                DoubleConverter.ToExactString(lat),
                DoubleConverter.ToExactString(lon),
                DoubleConverter.ToExactString(normalized_lat),
                DoubleConverter.ToExactString(normalized_lon),
                DoubleConverter.ToExactString(colat),
                DoubleConverter.ToExactString(minus_lon),
                DoubleConverter.ToExactString(minus_colat));
            File.WriteAllText(siteFrameFilename, text);
        }

        private double NormalizeLon(double l)
        {
            if (l < 0d) l += 360d;
            if (l > 360d) l -= 360d;
            return l;
        }
    }
}
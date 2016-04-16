using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OSGeo.GDAL;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using BitMiracle.LibTiff.Classic;
using Shadow.terrain;
using Shadow.viz;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Shadow.spice;

namespace Shadow
{
    public partial class Form1 : Form
    {
        const int bytesPerPixel = 4; // This constant must correspond with the pixel format of the converted bitmap.
        public TerrainManager Manager;
        public World TheWorld;

        public OpenGLControlWrapper Viewport;
        public ShaderProgram EarthShader;
        public ShaderProgram MoonShader;
        public PhongRejection1 MoonShaderPhongRejection1;
        public ShaderProgram MoonShaderTexturedPhong;

        public const float FarUnit = 1000f; // 1000 km
        public const float NearUnit = 1f; // 1 m
        public const double Meters = 1d;
        public const double Kilometers = 0.001d;
        public const long Minutes = 65536L * 60;
        public const long Seconds = 65536L;
        public const long Hours = 65536L * 60 * 60;
        public const long Days = 65536L * 60 * 60 * 24;

        public Form1()
        {
            InitializeComponent();

            //openToolStripMenuItem_Click(null, null);

            TheWorld = new World(@"c:\git\rp_shadows\data\kernels");
            TheWorld.Frame = LadeeStateFetcher.StateFrame.RP_Landing_Site;

            Viewport = new OpenGLControlWrapper();
            Viewport.VSync = false;
            Viewport.Dock = DockStyle.Fill;
            //Viewport.TopLevel = false;
            Viewport.BackColor = System.Drawing.Color.Black;
            Viewport.Load += new EventHandler(viewport_Load);
            Viewport.Paint += new PaintEventHandler(viewport_Paint);
            Viewport.Resize += new EventHandler(viewport_Resize);
            Controls.Add(Viewport);
            Viewport.Visible = true;
        }

        void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var d = new OpenFileDialog { DefaultExt = ".tif", CheckFileExists = true };
            //if (d.ShowDialog() != DialogResult.OK) return;
            //var filename = d.FileName;

            var filename = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";

            float[,] heightMap;
            int width, height;
            using (var stream = File.Open(filename, FileMode.Open))
            {
                var tiffDecoder = new TiffBitmapDecoder(
                    stream,
                    BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreImageCache,
                    BitmapCacheOption.None);
                //stream.Dispose();

                BitmapSource firstFrame = tiffDecoder.Frames[0];

                width = firstFrame.PixelWidth;
                height = firstFrame.PixelHeight;
                var buf = new byte[width * height * bytesPerPixel];
                firstFrame.CopyPixels(buf, width * bytesPerPixel, 0);

                heightMap = new float[width, height];
                Buffer.BlockCopy(buf, 0, heightMap, 0, buf.Length);
            }

            float maxy = float.MinValue;
            float miny = float.MaxValue;
            for (var i = 0; i < width; i++)
                for (var j = 0; j < height; j++)
                {
                    var v = heightMap[i, j];
                    if (v > maxy) maxy = v;
                    if (v < miny) miny = v;
                }
            Console.WriteLine(@"max={0} min={1}", maxy, miny);

            Manager = new TerrainManager { HeightMap = heightMap, Box = new BoundingBox(-250f, 250f, -250f, 250f, 0f, maxy - miny) };
            Console.WriteLine(@"Loaded.");
        }

        static float GetPixel(byte[] buf, int x, int y, int width)
        {
            var index = (y * (width * bytesPerPixel) + (x * bytesPerPixel));
            return BitConverter.ToSingle(buf, index);
        }

        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder() { };
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        public static void TestTiffLib()
        {
            var image = Tiff.Open(@"C:\git\rp_shadows\data\synthetic-lunar-patch.tif", "r");
            Console.WriteLine(@"Width= {0}", image.GetField(TiffTag.IMAGEWIDTH)[0].ToInt());
            Console.WriteLine(@"Height={0}", image.GetField(TiffTag.IMAGELENGTH)[0].ToInt());
            Console.WriteLine(@"depth={0}", image.GetField(TiffTag.IMAGEDEPTH)[0]);
        }

        private void viewport_Load(object sender, EventArgs e)
        {
            Viewport.Loaded = true;
            Viewport.MakeCurrent();

            CreateShaders();

            Viewport.TheWorld = TheWorld;

            LoadObjects();

            GL.ClearColor(System.Drawing.Color.Black);
            //SetupViewport();

            GL.Enable(EnableCap.Lighting); // Turn off lighting to get color
            GL.Enable(EnableCap.Light0);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest); //??
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                            (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                            (int)TextureMagFilter.Linear);

            GL.ShadeModel(ShadingModel.Smooth);

            // Enable Light 0 and set its parameters.
            //GL.Light(LightName.Light0, LightParameter.Position, SunPosition);

            const float ambient = 0.35f;
            const float diffuse = 1f;

            GL.Light(LightName.Light0, LightParameter.Ambient, new[] { ambient, ambient, ambient, 1.0f });
            //GL.Light(LightName.Light0, LightParameter.Ambient, new[] { 0.6f, 0.6f, 0.6f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new[] { diffuse, diffuse, diffuse, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Specular, new[] { 1f, 1f, 1f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.SpotExponent, new[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.LightModel(LightModelParameter.LightModelAmbient, new[] { 0f, 0f, 0f, 1.0f });
            GL.LightModel(LightModelParameter.LightModelLocalViewer, 0);
            GL.LightModel(LightModelParameter.LightModelTwoSide, 0);

            //GL.Material(MaterialFace.Front, MaterialParameter.Ambient, new float[] { 0.3f, 0.3f, 0.3f, 1.0f });
            //GL.Material(MaterialFace.Front, MaterialParameter.Diffuse, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            //GL.Material(MaterialFace.Front, MaterialParameter.Specular, new float[] { 0.5f, 0.5f, 0.5f, 1.0f });
            //GL.Material(MaterialFace.Front, MaterialParameter.Emission, new float[] { 0.0f, 0.0f, 0.0f, 1.0f });

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.ColorMaterial); // lets me use colors rather than changing materials
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Normalize); // Do I need this?  (this make a difference, although I don't know why)

            GL.PointSize(5f);
            GL.Enable(EnableCap.PointSmooth);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

            long t = TimeUtilities.DateTimeToTime42(new DateTime(2022,6,1));
            TheWorld.Update(t);

            Viewport.CameraMode = new ArcBall(Viewport, TheWorld.CentralBall)
            {
                RelativePosition = new Vector3d(0d, 100 * Meters, 0d)
            };

            if (false)
            {
                var oldEye = new Vector3d(Viewport.CameraMode.Eye);
                var m = new JoystickCamera(Viewport, Viewport.CameraMode.Target)
                {
                    Eye = oldEye
                };
                m.ResetVectors();
                Viewport.CameraMode = m;
            }
            TheWorld.Tick();
        }

        private void LoadObjects()
        {
            TheWorld.Moon = new MoonDEM
            {
                Name = "Moon",
                Position = new Vector3d(384400 * Kilometers, 0d, 0d),
                TextureFilename = @"Resources\moon_8k_color_brim16.jpg",
                ShowAxes = false,
                AxisScale = 4f,
                Shininess = 1f,
                Shader = MoonShader
            };
            ((MoonDEM)TheWorld.Moon).Load();
            TheWorld.NearShapes.Add(TheWorld.Moon);

            // earth_800x400.jpg
            // land_shallow_topo_2011_8192.jpg
            TheWorld.Earth = new Earth
            {
                Name = "Earth",
                Position = new Vector3d(0d, 0d, 0d),
                TextureFilename = @"Resources\earth_800x400.jpg",
                NightFilename = @"Resources\earth_night_800x400.jpg",
                Radius = (float)(6371 * Kilometers),
                XSize = 48,
                YSize = 24,
                ShowAxes = false,
                AxisScale = 10f,
                Specularity = new[] { 1f, 1f, 1f },
                Shininess = 10f,
                Shader = EarthShader
            };
            TheWorld.Earth.Load();
            TheWorld.Earth.LoadTexture();
            TheWorld.FarShapes.Add(TheWorld.Earth);

            // earth_800x400.jpg
            // land_shallow_topo_2011_8192.jpg
            TheWorld.Sun = new TexturedBall
            {
                Name = "Sun",
                Position = new Vector3d(0d, 0d, 0d),
                TextureFilename = @"Resources\sun.png",
                Color = System.Drawing.Color.Yellow,
                Radius = (float)(695500 * Kilometers),
                XSize = 32,
                YSize = 16,
                ShowAxes = false,
                AxisScale = 10f
            };
            TheWorld.Sun.Load();
            TheWorld.Sun.LoadTexture();

            TheWorld.Stars = new StarBackground();
            TheWorld.Stars.Load();

            TheWorld.CentralBall = new Ball() { ShowAxes = true };
            TheWorld.NearShapes.Add(TheWorld.CentralBall);
        }

        private void CreateShaders()
        {
            EarthShader = new EarthShaderProgram(@"Resources\earth_vs_120.glsl", @"Resources\earth_fs_120.glsl");
            //MoonShader = new ShaderProgram("moon1_vs_120.glsl", "moon1_fs_120.glsl");
            MoonShaderPhongRejection1 = new PhongRejection1(@"Resources\textured_phong_vs_120.glsl", @"Resources\phong_rejection1_fs_120.glsl");
            MoonShaderTexturedPhong = new ShaderProgram(@"Resources\textured_phong_vs_120.glsl", @"Resources\textured_phong_fs_120.glsl");
            MoonShader = null;
        }

        private void viewport_Paint(object sender, PaintEventArgs e)
        {
            if (!Viewport.Loaded) return;
            Viewport.PaintScene();
        }

        private void viewport_Resize(object sender, EventArgs e)
        {
            if (!Viewport.Loaded) return;
            Viewport.SetupViewport();
            if (MoonShaderPhongRejection1 != null)
            {
                MoonShaderPhongRejection1.CenterX = Viewport.Width / 2f;
                MoonShaderPhongRejection1.CenterY = Viewport.Height / 2f;
                MoonShaderPhongRejection1.AngleFactor = 1 / 50f;
            }
        }
    }
}

using Shadow.terrain;
using Shadow.viz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Shadow.spice;
using System.Windows.Media.Imaging;
using System.IO;

namespace Shadows
{
    public partial class TerrainViz : Form
    {
        public const float Kilometers = 1000f;

        public OpenGLControlWrapper Viewport;
        public TerrainManager Manager;
        public World TheWorld;

        public ShaderProgram EarthShader;
        public ShaderProgram MoonShader;
        public PhongRejection1 MoonShaderPhongRejection1;
        public ShaderProgram MoonShaderTexturedPhong;

        const int bytesPerPixel = 4; // This constant must correspond with the pixel format of the converted bitmap.
        public Terrain TheTerrain;


        public TerrainViz()
        {
            InitializeComponent();

            TheWorld = new World();
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

        private void viewport_Load(object sender, EventArgs e)
        {
            Viewport.Loaded = true;
            Viewport.MakeCurrent();

            CreateShaders();

            Viewport.TheWorld = TheWorld;

            LoadObjects();

            GL.ClearColor(Color.Black);
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
            GL.Light(LightName.Light0, LightParameter.Position, new[] { 1000f, 1000f, 100f } );

            const float ambient = 0.5f;
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

            long t = TimeUtilities.DateTimeToTime42(new DateTime(2022, 6, 1));
            TheWorld.Update(t);

            if (true)
            {
                Viewport.CameraMode = new ArcBall(Viewport, TheWorld.CentralBall)
                {
                    RelativePosition = new Vector3(0f, 100f, 0f)
                };
            }
            else
            {
                var m = new JoystickCamera(Viewport, TheWorld.CentralBall)
                {
                    Eye = new Vector3(10, 10, 3)
                };
                m.ResetVectors();
                Viewport.CameraMode = m;
            }
            TheWorld.Tick();
        }

        private void CreateShaders()
        {
            EarthShader = new EarthShaderProgram(@"Resources\earth_vs_120.glsl", @"Resources\earth_fs_120.glsl");
            //MoonShader = new ShaderProgram("moon1_vs_120.glsl", "moon1_fs_120.glsl");
            MoonShaderPhongRejection1 = new PhongRejection1(@"Resources\textured_phong_vs_120.glsl", @"Resources\phong_rejection1_fs_120.glsl");
            MoonShaderTexturedPhong = new ShaderProgram(@"Resources\textured_phong_vs_120.glsl", @"Resources\textured_phong_fs_120.glsl");
            MoonShader = null;
        }

        private void LoadObjects()
        {
            // earth_800x400.jpg
            // land_shallow_topo_2011_8192.jpg
            TheWorld.Sun = new Ball
            {
                Name = "Sun",
                Position = new Vector3(0f, 0f, 0f),
                //TextureFilename = @"Resources\sun.png",
                Color = Color.Yellow,
                Radius = 695500 * Kilometers,
                XSize = 32,
                YSize = 16,
                ShowAxes = false,
                AxisScale = 10f
            };
            TheWorld.Sun.Load();
            //TheWorld.Sun.LoadTexture();
            TheWorld.FarShapes.Add(TheWorld.Sun);

            //TheWorld.Stars = new StarBackground();
            //TheWorld.Stars.Load();

            TheWorld.CentralBall = new Ball() { ShowAxes = true, DrawLines = true, XSize = 24, YSize = 12 };
            TheWorld.CentralBall.Load();
            TheWorld.NearShapes.Add(TheWorld.CentralBall);

            var flat = new Flat { Position = new Vector3(-50,-50,0), Length = 100, Width = 100, XSize = 100, YSize = 100, DrawLines = true };
            flat.Load();
            TheWorld.NearShapes.Add(flat);
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

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";
            LoadFileToHeightField(filename);
            var patch = new TerrainPatch
            {
                TheTerrain = TheTerrain,
                Bounds = new Rectangle(0, 0, 2000, 2000),
                TextureFilename = @"C:\git\rp_shadows\data\lightmap-2000x2000-az245el0.14.png",
                Shader = MoonShaderTexturedPhong
            };
            patch.Load();
            TheWorld.NearShapes.Add(patch);
            TheWorld.Patch = patch;
            TheWorld.Patch.DrawLines = false;
            foreach (var flat in TheWorld.NearShapes.OfType<Flat>())
                flat.Visible = false;
            TheWorld.Patch.Position += new Vector3(125, 125, 0);
            Viewport.Invalidate();
        }

        void LoadFileToHeightField(string filename)
        {
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

            float maxz = float.MinValue;
            float minz = float.MaxValue;
            for (var i = 0; i < width; i++)
                for (var j = 0; j < height; j++)
                {
                    var v = heightMap[i, j];
                    if (v > maxz) maxz = v;
                    if (v < minz) minz = v;
                }
            Console.WriteLine(@"max={0} min={1}", maxz, minz);

            TheTerrain = new Terrain
            {
                HeightMap = heightMap,
                Box = new BoundingBox(-250f, 250f, -250f, 250f, 0f, maxz - minz),
                MinPZ = minz,
                MaxPZ = maxz,
                MinPX = 0f,
                MaxPX = 500f,
                MinPY = 0f,
                MaxPY = 500f
            };
            Console.WriteLine(@"Loaded.");
        }

        private void open400X400ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";
            LoadFileToHeightField(filename);
            TheTerrain = SubsetHeightField(TheTerrain, new Rectangle(0, 0, 400, 400));
            var patch = new TerrainPatch { TheTerrain = TheTerrain, Bounds = new Rectangle(0, 0, 400, 400) };
            patch.Load();
            TheWorld.NearShapes.Add(patch);
            patch.DrawLines = false;
            foreach (var flat in TheWorld.NearShapes.OfType<Flat>())
                flat.Visible = false;
            Viewport.Invalidate();
        }

        private Terrain SubsetHeightField(Terrain theTerrain, Rectangle rectangle)
        {
            var newMap = new float[rectangle.Width, rectangle.Height];
            var oldMap = theTerrain.HeightMap;
            var x1 = rectangle.Left;
            var y1 = rectangle.Top;
            var x2 = rectangle.Right;
            var y2 = rectangle.Bottom;
            for (var x = rectangle.Left; x < rectangle.Right; x++)
                for (var y = rectangle.Top; y < rectangle.Bottom; y++)
                    newMap[x - rectangle.Left, y - rectangle.Top] = oldMap[x, y];
            float maxz = float.MinValue;
            float minz = float.MaxValue;
            for (var i = 0; i < rectangle.Width; i++)
                for (var j = 0; j < rectangle.Height; j++)
                {
                    var v = newMap[i, j];
                    if (v > maxz) maxz = v;
                    if (v < minz) minz = v;
                }
            var t = new Terrain
            {
                HeightMap = newMap,
                Box = new BoundingBox(0f, theTerrain.Width * theTerrain.GridResolution, -0f, theTerrain.Height * theTerrain.GridResolution, 0f, maxz - minz),
                MinPZ = minz,
                MaxPZ = maxz,
                MinPX = 0f,
                MaxPX = rectangle.Width * theTerrain.GridResolution,
                MinPY = 0f,
                MaxPY = rectangle.Height * theTerrain.GridResolution
            };
            return t;
        }

        private void open8X8ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";
            LoadFileToHeightField(filename);
            TheTerrain = SubsetHeightField(TheTerrain, new Rectangle(0, 0, 8, 8));
            var patch = new TerrainPatch { TheTerrain = TheTerrain, Bounds = new Rectangle(0, 0, 8, 8) };
            patch.Load();
            TheWorld.NearShapes.Add(patch);
            patch.DrawLines = false;
            foreach (var flat in TheWorld.NearShapes.OfType<Flat>())
                flat.Visible = false;
            Viewport.Invalidate();
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TheWorld.Patch.Position += new Vector3(10, 10, 0);
            Viewport.Invalidate();
        }

        private void subtractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TheWorld.Patch.Position += new Vector3(-10, -10, 0);
            Viewport.Invalidate();
        }

        private void trackBallToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Viewport.CameraMode = new ArcBall(Viewport, TheWorld.CentralBall)
            {
                RelativePosition = new Vector3(0f, 100f, 0f)
            };
            Viewport.Invalidate();
        }

        private void joystickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var m = new JoystickCamera(Viewport, TheWorld.CentralBall)
            {
                Eye = new Vector3(10, 10, 3)
            };
            m.ResetVectors();
            Viewport.CameraMode = m;
            Viewport.Invalidate();
        }

        private void open2000X2000ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";
            LoadFileToHeightField(filename);
            var r = new Rectangle(0, 0, 2000, 2000);
            TheTerrain = SubsetHeightField(TheTerrain, r);
            TheWorld.Patch = new TerrainPatch
            {
                TheTerrain = TheTerrain,
                Bounds = new Rectangle(0, 0, 2000, 2000),
                TextureFilename = @"C:\git\rp_shadows\data\lightmap-2000x2000-az245el0.14.png",
                Shader = MoonShaderTexturedPhong
            };
            TheWorld.Patch.Load();
            TheWorld.NearShapes.Add(TheWorld.Patch);
            TheWorld.Patch.DrawLines = false;
            foreach (var flat in TheWorld.NearShapes.OfType<Flat>())
                flat.Visible = false;
            Viewport.Invalidate();
        }

        private void showTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showTextureToolStripMenuItem.Checked = !showTextureToolStripMenuItem.Checked;
            if (TheWorld.Patch != null)
                TheWorld.Patch.ShowTexture = showTextureToolStripMenuItem.Checked;
            Viewport.Invalidate();
        }
    }
}

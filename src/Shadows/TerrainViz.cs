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

            long t = TimeUtilities.DateTimeToTime42(new DateTime(2022, 6, 1));
            TheWorld.Update(t);

            if (false)
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

    }
}

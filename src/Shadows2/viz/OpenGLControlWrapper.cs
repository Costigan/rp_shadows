using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Shadow.viz
{
    public class OpenGLControlWrapper : GLControl
    {
        public readonly float[] SunPosition = {0f, -1000f, 0f, 0f};
        public Camera CameraMode;
        public float[,] Frustum = new float[6,4];
        public bool Loaded = false;

        public bool ShowTrajectory = false;
        public World TheWorld;
        private bool _dragging;

        //This now explicitly requests 24 bits of depth buffer rather than defaulting.
        public OpenGLControlWrapper() :
            base(new GraphicsMode(32, 24, 0, 0, 0, 2, false))
        {
            CameraMode = new Camera(this, null);
        }

        #region Convenience Methods

        #endregion

        internal void StartDragging()
        {
        }

        #region Mouse Handling

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _dragging = true;
            CameraMode.DragStart(this, e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!_dragging) return;
            CameraMode.Drag(this, e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            _dragging = false;
            CameraMode.DragStop(this, e);
        }

        #endregion

        #region Painting

        public void SetupViewport(float fov = 10f, float nearPlane = 0.01f, float farPlane = 2000f)
        {
            const float degrees = 3.141592653f/180f;
            int w = Width;
            int h = Height;

            //MakeCurrent();

            //TODO: This shows that we're getting a 16-bit depth buffer
            //var gm = GraphicsMode;

            GL.Viewport(0, 0, w, h);
            float aspectRatio = w/(float) h;
            Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(fov*degrees, aspectRatio, nearPlane, farPlane);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perpective);
            GL.MatrixMode(MatrixMode.Modelview);

            //ExtractFrustum();
            //TransformOrigin();
        }

        public void PaintScene()
        {
            MakeCurrent();
            CameraMode.Tick();

            CameraMode.Paint(this);
        }

        public void DrawStars()
        {
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.DepthTest);

            TheWorld.Sun.Draw(false, CameraMode.Eye);

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Lighting);
        }

        public void DrawLine(Vector3 start, Vector3 stop, Color color)
        {
            GL.Disable(EnableCap.Lighting);
            GL.Color3(color);
            GL.Begin(BeginMode.Lines);
            GL.Vertex3(start);
            GL.Vertex3(stop);
            GL.End();
            GL.Enable(EnableCap.Lighting);
        }

        public void DrawAxes()
        {
            GL.Disable(EnableCap.Lighting);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.Scale(100f, 100f, 100f);

            GL.Begin(BeginMode.Lines);
            GL.Color3(Color.Blue);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(1f, 0f, 0f);
            GL.Color3(Color.Green);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, 1f, 0f);
            GL.Color3(Color.Red);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, 0f, 1f);
            GL.End();

            GL.PopMatrix();
            GL.Enable(EnableCap.Lighting);
        }

        protected void ExtractFrustum()
        {
            var proj = new float[16];
            var modl = new float[16];
            var clip = new float[16];
            float t;

            /* Get the current PROJECTION matrix from OpenGL */
            GL.GetFloat(GetPName.ProjectionMatrix, proj);

            /* Get the current MODELVIEW matrix from OpenGL */
            GL.GetFloat(GetPName.ProjectionMatrix, modl);

            /* Combine the two matrices (multiply projection by modelview) */
            clip[0] = modl[0]*proj[0] + modl[1]*proj[4] + modl[2]*proj[8] + modl[3]*proj[12];
            clip[1] = modl[0]*proj[1] + modl[1]*proj[5] + modl[2]*proj[9] + modl[3]*proj[13];
            clip[2] = modl[0]*proj[2] + modl[1]*proj[6] + modl[2]*proj[10] + modl[3]*proj[14];
            clip[3] = modl[0]*proj[3] + modl[1]*proj[7] + modl[2]*proj[11] + modl[3]*proj[15];

            clip[4] = modl[4]*proj[0] + modl[5]*proj[4] + modl[6]*proj[8] + modl[7]*proj[12];
            clip[5] = modl[4]*proj[1] + modl[5]*proj[5] + modl[6]*proj[9] + modl[7]*proj[13];
            clip[6] = modl[4]*proj[2] + modl[5]*proj[6] + modl[6]*proj[10] + modl[7]*proj[14];
            clip[7] = modl[4]*proj[3] + modl[5]*proj[7] + modl[6]*proj[11] + modl[7]*proj[15];

            clip[8] = modl[8]*proj[0] + modl[9]*proj[4] + modl[10]*proj[8] + modl[11]*proj[12];
            clip[9] = modl[8]*proj[1] + modl[9]*proj[5] + modl[10]*proj[9] + modl[11]*proj[13];
            clip[10] = modl[8]*proj[2] + modl[9]*proj[6] + modl[10]*proj[10] + modl[11]*proj[14];
            clip[11] = modl[8]*proj[3] + modl[9]*proj[7] + modl[10]*proj[11] + modl[11]*proj[15];

            clip[12] = modl[12]*proj[0] + modl[13]*proj[4] + modl[14]*proj[8] + modl[15]*proj[12];
            clip[13] = modl[12]*proj[1] + modl[13]*proj[5] + modl[14]*proj[9] + modl[15]*proj[13];
            clip[14] = modl[12]*proj[2] + modl[13]*proj[6] + modl[14]*proj[10] + modl[15]*proj[14];
            clip[15] = modl[12]*proj[3] + modl[13]*proj[7] + modl[14]*proj[11] + modl[15]*proj[15];

            /* Extract the numbers for the RIGHT plane */
            Frustum[0, 0] = clip[3] - clip[0];
            Frustum[0, 1] = clip[7] - clip[4];
            Frustum[0, 2] = clip[11] - clip[8];
            Frustum[0, 3] = clip[15] - clip[12];

            /* Normalize the result */
            t =
                (float)
                Math.Sqrt(Frustum[0, 0]*Frustum[0, 0] + Frustum[0, 1]*Frustum[0, 1] + Frustum[0, 2]*Frustum[0, 2]);
            Frustum[0, 0] /= t;
            Frustum[0, 1] /= t;
            Frustum[0, 2] /= t;
            Frustum[0, 3] /= t;

            /* Extract the numbers for the LEFT plane */
            Frustum[1, 0] = clip[3] + clip[0];
            Frustum[1, 1] = clip[7] + clip[4];
            Frustum[1, 2] = clip[11] + clip[8];
            Frustum[1, 3] = clip[15] + clip[12];

            /* Normalize the result */
            t =
                (float)
                Math.Sqrt(Frustum[1, 0]*Frustum[1, 0] + Frustum[1, 1]*Frustum[1, 1] + Frustum[1, 2]*Frustum[1, 2]);
            Frustum[1, 0] /= t;
            Frustum[1, 1] /= t;
            Frustum[1, 2] /= t;
            Frustum[1, 3] /= t;

            /* Extract the BOTTOM plane */
            Frustum[2, 0] = clip[3] + clip[1];
            Frustum[2, 1] = clip[7] + clip[5];
            Frustum[2, 2] = clip[11] + clip[9];
            Frustum[2, 3] = clip[15] + clip[13];

            /* Normalize the result */
            t =
                (float)
                Math.Sqrt(Frustum[2, 0]*Frustum[2, 0] + Frustum[2, 1]*Frustum[2, 1] + Frustum[2, 2]*Frustum[2, 2]);
            Frustum[2, 0] /= t;
            Frustum[2, 1] /= t;
            Frustum[2, 2] /= t;
            Frustum[2, 3] /= t;

            /* Extract the TOP plane */
            Frustum[3, 0] = clip[3] - clip[1];
            Frustum[3, 1] = clip[7] - clip[5];
            Frustum[3, 2] = clip[11] - clip[9];
            Frustum[3, 3] = clip[15] - clip[13];

            /* Normalize the result */
            t =
                (float)
                Math.Sqrt(Frustum[3, 0]*Frustum[3, 0] + Frustum[3, 1]*Frustum[3, 1] + Frustum[3, 2]*Frustum[3, 2]);
            Frustum[3, 0] /= t;
            Frustum[3, 1] /= t;
            Frustum[3, 2] /= t;
            Frustum[3, 3] /= t;

            /* Extract the FAR plane */
            Frustum[4, 0] = clip[3] - clip[2];
            Frustum[4, 1] = clip[7] - clip[6];
            Frustum[4, 2] = clip[11] - clip[10];
            Frustum[4, 3] = clip[15] - clip[14];

            /* Normalize the result */
            t =
                (float)
                Math.Sqrt(Frustum[4, 0]*Frustum[4, 0] + Frustum[4, 1]*Frustum[4, 1] + Frustum[4, 2]*Frustum[4, 2]);
            Frustum[4, 0] /= t;
            Frustum[4, 1] /= t;
            Frustum[4, 2] /= t;
            Frustum[4, 3] /= t;

            /* Extract the NEAR plane */
            Frustum[5, 0] = clip[3] + clip[2];
            Frustum[5, 1] = clip[7] + clip[6];
            Frustum[5, 2] = clip[11] + clip[10];
            Frustum[5, 3] = clip[15] + clip[14];

            /* Normalize the result */
            t =
                (float)
                Math.Sqrt(Frustum[5, 0]*Frustum[5, 0] + Frustum[5, 1]*Frustum[5, 1] + Frustum[5, 2]*Frustum[5, 2]);
            Frustum[5, 0] /= t;
            Frustum[5, 1] /= t;
            Frustum[5, 2] /= t;
            Frustum[5, 3] /= t;
        }

        private bool PointInFrustum(float x, float y, float z)
        {
            int p;

            for (p = 0; p < 6; p++)
                if (Frustum[p, 0]*x + Frustum[p, 1]*y + Frustum[p, 2]*z + Frustum[p, 3] <= 0)
                    return false;
            return true;
        }

        private bool SphereInFrustum(float x, float y, float z, float radius)
        {
            int p;

            for (p = 0; p < 6; p++)
                if (Frustum[p, 0]*x + Frustum[p, 1]*y + Frustum[p, 2]*z + Frustum[p, 3] <= -radius)
                    return false;
            return true;
        }

        private float SphereInFrustumDistance(float x, float y, float z, float radius)
        {
            int p;
            float d = 0f;

            for (p = 0; p < 6; p++)
            {
                d = Frustum[p, 0]*x + Frustum[p, 1]*y + Frustum[p, 2]*z + Frustum[p, 3];
                if (d <= -radius)
                    return 0;
            }
            return d + radius;
        }

        #endregion

        public void SaveScreenshot()
        {
            var b = GrabScreenshot2();
            var dialog = new SaveFileDialog { DefaultExt = ".bmp", Filter = "Image Files (*.bmp, *.jpg, *.png)|*.bmp;*.jpg;*.png|All Files (*.*)|*.*" };
            if (dialog.ShowDialog() != DialogResult.OK) return;
            b.Save(dialog.FileName);
        }

        // Returns a System.Drawing.Bitmap with the contents of the current framebuffer
        public Bitmap GrabScreenshot2()
        {
            if (GraphicsContext.CurrentContext == null)
                throw new GraphicsContextMissingException();

            var bmp = new Bitmap(ClientSize.Width, ClientSize.Height);
            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, ClientSize.Width, ClientSize.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }

        public Bitmap GrabScreenshot2(Bitmap bmp = null)
        {
            if (GraphicsContext.CurrentContext == null)
                throw new GraphicsContextMissingException();

            if (bmp == null || bmp.Size != Size)
                bmp = new Bitmap(ClientSize.Width, ClientSize.Height);
            System.Drawing.Imaging.BitmapData data =
                bmp.LockBits(ClientRectangle, System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, ClientSize.Width, ClientSize.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            bmp.UnlockBits(data);

            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            return bmp;
        }

        public void SynchronizedInvoke(Action action)
        {
            // If the invoke is not required, then invoke here and get out.
            if (!InvokeRequired)
            {
                // Execute action.
                action();

                // Get out.
                return;
            }

            // Marshal to the required context.
            Invoke(action, new object[] { });
        }

    }
}
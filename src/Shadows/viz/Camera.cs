using System;
using System.Drawing;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using ButtonState = OpenTK.Input.ButtonState;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Shadow.viz
{
    public class Camera
    {
        public const double NearFieldDistanceThreshold = 0.01d;
        public float FieldOfView = 10f;
        public Shape Target;
        public Matrix4 ViewMatrix = Matrix4.Identity;
        public OpenGLControlWrapper Wrapper;
        protected bool _dirty = false;
        protected Vector3 _eye;
        protected Vector3 _lookat; // position we're looking at
        protected Vector3 _up = new Vector3(0f, 0f, 1f); // always normalized

        public Camera(OpenGLControlWrapper w, Shape t)
        {
            Wrapper = w;
            Target = t;
        }

        public virtual Vector3 Eye
        {
            get { return _eye; }
            set { _eye = value; }
        }

        public virtual Vector3 Lookat
        {
            get { return _lookat; }
            set { _lookat = value; }
        }

        public virtual Vector3 Up
        {
            get { return _up; }
            set { _up = value; }
        }

        public virtual bool Dirty
        {
            get { return _dirty; }
            set { _dirty = value; }
        }

        #region Ticking

        public virtual void Tick()
        {
        }

        public bool IsDirty()
        {
            return Dirty;
        }

        #endregion

        #region Dragging

        public virtual void DragStart(OpenGLControlWrapper w, MouseEventArgs e)
        {
        }

        public virtual void Drag(OpenGLControlWrapper w, MouseEventArgs e)
        {
        }

        public virtual void DragStop(OpenGLControlWrapper w, MouseEventArgs e)
        {
        }

        #endregion Dragging

        #region Painting

        public virtual void Paint(OpenGLControlWrapper w)
        {
            var eye = new Vector3(0f, 0f, 0f);
            Vector3 target = Lookat - Eye;
            ViewMatrix = Matrix4.LookAt(eye, target, Up);

            //todo
            //Console.WriteLine(ViewMatrix);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref ViewMatrix);

            PaintInternal(w);
            w.SwapBuffers();
        }

        public virtual void PaintInternal(OpenGLControlWrapper w)
        {
            // Position the lights in view space
            Vector3 p = w.TheWorld.Sun.Position - Eye;
            w.SunPosition[0] = p[0];
            w.SunPosition[1] = p[1];
            w.SunPosition[2] = p[2];
            w.SunPosition[3] = 0f;
            GL.Light(LightName.Light0, LightParameter.Position, w.SunPosition);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            w.SetupViewport(FieldOfView, 1000f, 2000000f);
            w.DrawStars();
            w.SetupViewport(FieldOfView, 0.01f, 8000f);

            for (int i = 0; i < w.TheWorld.FarShapes.Count; i++)
                w.TheWorld.FarShapes[i].Draw(false, Eye);

            double nearClip = 0.5f;
            double farClip = 20000f;

            GL.Clear(ClearBufferMask.DepthBufferBit);

            //w.SetupViewport(FieldOfView, 1f, 5000f);
            w.SetupViewport(FieldOfView, (float)nearClip, (float)farClip);

            for (int i = 0; i < w.TheWorld.NearShapes.Count; i++)
                w.TheWorld.NearShapes[i].Draw(true, Eye);
        }

        #endregion
    }

    public class ArcBall : Camera
    {
        public enum DragState
        {
            Idle,
            Scaling,
            ArcBalling,
            RotatingUp
        };

        public const float ZoomFactor = 1.1f;
        public const float ZoomGain = 0.01f;
        public const float YawGain = (float)(-Math.PI / 500f);
        public const float PitchGain = (float)(-Math.PI / 500f);
        private DragState _dragging;
        private Point _lastLocation;
        private Vector3 _startPosition;

        public ArcBall(OpenGLControlWrapper w, Shape t)
            : base(w, t)
        {
            _lookat = new Vector3(0f, 0f, 0f);
            _eye = new Vector3(10f, 0f, 0f);
        }

        public override Vector3 Eye
        {
            get { return Target == null ? _eye : Target.Position + _eye; }
            set { _eye = Target == null ? value : value - Target.Position; }
        }

        public override Vector3 Lookat
        {
            get { return Target == null ? _lookat : Target.Position + _lookat; }
            set { _lookat = Target == null ? value : value - Target.Position; }
        }

        public Vector3 RelativePosition
        {
            get { return _eye - _lookat; }
            set { _eye = value - _lookat; }
        }

        public override void Tick()
        {
        }

        public override void DragStart(OpenGLControlWrapper w, MouseEventArgs e)
        {
            _lastLocation = e.Location;
            _startPosition = _eye - _lookat;
            double circleDistance = 0.4d * Math.Min(w.Width, w.Height);
            int dx = e.X - w.Width / 2;
            int dy = e.Y - w.Height / 2;
            double d = Math.Sqrt(dx * dx + dy * dy);
            if (d > circleDistance)
                _dragging = DragState.RotatingUp;
            else
                switch (e.Button)
                {
                    case MouseButtons.Left:
                        _dragging = DragState.ArcBalling;
                        break;
                    case MouseButtons.Right:
                        _dragging = DragState.Scaling;
                        break;
                    default:
                        _dragging = DragState.Idle;
                        break;
                }
        }

        public override void Drag(OpenGLControlWrapper w, MouseEventArgs e)
        {
            if (_dragging == DragState.Idle)
                return;
            Lookat = Target.Position;
            switch (_dragging)
            {
                case DragState.ArcBalling: // moving
                    {
                        Vector3 relativePosition = _eye - _lookat;
                        var lookToward = new Vector3(-relativePosition);
                        lookToward.Normalize();
                        Vector3 right = Vector3.Cross(lookToward, Up);
                        float yaw = YawGain * (e.Location.X - _lastLocation.X);
                        Matrix4 yawMatrix = Matrix4.CreateFromAxisAngle(Up, yaw);
                        float pitch = PitchGain * (e.Location.Y - _lastLocation.Y);
                        Matrix4 pitchMatrix = Matrix4.CreateFromAxisAngle(right, pitch);
                        float distance = relativePosition.Length;
                        relativePosition = Vector3.TransformVector(relativePosition, yawMatrix * pitchMatrix);
                        relativePosition.Normalize();
                        relativePosition = relativePosition * distance;

                        _eye = _lookat + relativePosition;
                        relativePosition.Normalize();
                        Up = Vector3.Cross(relativePosition, right);
                        Up.Normalize();
                        _lastLocation = e.Location;
                    }
                    break;
                case DragState.Scaling: // scaling
                    {
                        float d = ZoomGain * (e.Location.Y - _lastLocation.Y);
                        float zoom = (float)Math.Exp(d);
                        Vector3 rp = _startPosition * (ZoomFactor * zoom);
                        _eye = _lookat + rp;
                    }
                    break;
                case DragState.RotatingUp:
                    {
                        int centerX = w.Width / 2;
                        int centerY = w.Height / 2;
                        var v1 = new Vector3(_lastLocation.X - centerX, _lastLocation.Y - centerY, 0f);
                        var v2 = new Vector3(e.Location.X - centerX, e.Location.Y - centerY, 0f);
                        Vector3 v3 = Vector3.Cross(v1, v2);
                        float v3Mag = v3[2];
                        float angDeg = (float)Math.Asin(v3Mag / (v1.Length * v2.Length)); //*180d/Math.PI
                        Vector3 axis = _lookat - _eye;
                        axis.Normalize();
                        Matrix4 mat = Matrix4.CreateFromAxisAngle(axis, -angDeg);
                        Up = Vector3.TransformVector(Up, mat); // was _startUp
                        Up.Normalize();
                        _lastLocation = e.Location; // not sure about this
                    }
                    break;
            }
            Dirty = true;
            w.Invalidate();
        }

        public override void DragStop(OpenGLControlWrapper w, MouseEventArgs e)
        {
            _dragging = DragState.Idle;
        }
    }

    public class ZoomTo : Camera
    {
        private readonly Vector3 _eye2;
        public float Distance = 1000f;
        public float LookRelaxation = 0.999f;
        public float LookThreshold = 0.99f;
        public Camera Next;
        public float ZoomRelaxation = 0.95f;
        public float ZoomThreshold = 0.1f;

        private int _state; // 1 means relaxing lookat, 2 means relaxing eye, 0 means idle

        public ZoomTo(OpenGLControlWrapper w, Shape t, float distanceFrom, float threshold, Camera next)
            : base(w, t)
        {
            Wrapper = w;
            _eye = w.CameraMode.Eye;
            _lookat = w.CameraMode.Lookat;
            Target = t;
            Next = next;
            ZoomThreshold = threshold;

            _eye2 = (t.Position - _eye);
            _eye2.NormalizeFast();
            _eye2 = _eye2 * -distanceFrom + t.Position;

            _state = 1;
        }

        public override void Tick()
        {
            switch (_state)
            {
                case 0:
                    break;
                case 1:
                    {
                        Vector3 p = (Lookat - Target.Position) * LookRelaxation;
                        Lookat = p + Target.Position;
                        Vector3 d1 = Lookat - Eye;
                        Vector3 d2 = Target.Position - Eye;
                        d1.NormalizeFast();
                        d2.NormalizeFast();
                        float dot = Vector3.Dot(d1, d2);
                        if (dot > LookThreshold)
                        {
                            _state = 2;
                            Lookat = Target.Position;
                        }
                    }
                    break;
                case 2:
                    {
                        Vector3 p = (Eye - _eye2) * ZoomRelaxation;
                        Eye = p + _eye2;
                        float d = (Eye - _eye2).LengthFast;
                        //Console.WriteLine(@"relax distance={0} eye={1}", d, Eye);
                        if (d < ZoomThreshold)
                        {
                            _state = 0;
                            Wrapper.CameraMode = Next;
                            Next.Eye = Eye;
                            Next.Lookat = Lookat;
                            Next.Target = Target;
                        }
                    }
                    break;
            }
            Wrapper.Invalidate();
        }
    }

    public class InstrumentView : Camera
    {
        public static Vector3 X = new Vector3(1f, 0f, 0f);
        public static Vector3 Y = new Vector3(0f, 1f, 0f);
        public static Vector3 Z = new Vector3(0f, 0f, 1f);
        public float Azimuth = 0f;
        public float Elevation = 0f;
        public Shape Satellite;

        public InstrumentView(OpenGLControlWrapper w, Shape t)
            : base(w, t)
        {
            Satellite = t;
        }

        public override void Tick()
        {
            Eye = Satellite.Position;
        }

        public override void Paint(OpenGLControlWrapper w)
        {
            float e = MathHelper.DegreesToRadians(Elevation);
            float a = MathHelper.DegreesToRadians(Azimuth);
            var z = (float)Math.Sin(e);
            double d = Math.Cos(e);
            var x = (float)(Math.Cos(a) * d);
            var y = (float)(Math.Sin(a) * d);
            var target = new Vector3(x, y, z);

            var eye = new Vector3(0f, 0f, 0f);
            var up = new Vector3(0f, 0f, 1f);
            ViewMatrix = Matrix4.LookAt(eye, target, up);

            GL.MatrixMode(MatrixMode.Modelview); // This worked with the lookat in s/c frame
            GL.LoadMatrix(ref ViewMatrix);

            Vector3 axis;
            float angle;
            Satellite.Attitude.ToAxisAngle(out axis, out angle);
            GL.Rotate(angle * 180.0f / 3.141593f, axis);

            PaintInternal(w);

            w.SwapBuffers();
        }
    }

    public class TelescopeView : InstrumentView
    {
        public TelescopeView(OpenGLControlWrapper w, Shape center, float fov = 4f)
            : base(w, center)
        {
            Azimuth = 170.1f;
            Elevation = 0f;
            FieldOfView = fov;
        }

        public override void PaintInternal(OpenGLControlWrapper w)
        {
            base.PaintInternal(w);

            GL.Disable(EnableCap.Lighting);

            GL.MatrixMode(MatrixMode.Modelview); // This worked with the lookat in s/c frame
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            int width = w.Width;
            int height = w.Height;
            float aspectRatio = width / (float)height;

            GL.Ortho(-aspectRatio, aspectRatio, -1.0, 1.0, 0.0, 4.0);
            GL.Viewport(0, 0, width, height);

            const double solarViewerFOVRadius = 0.5d;
            double radius = solarViewerFOVRadius * (2f / FieldOfView);
            const int numPoints = 32;
            const double frac = (2d * Math.PI) / numPoints;

            GL.Color3(Color.CornflowerBlue);
            GL.Begin(BeginMode.LineStrip);
            for (int i = 0; i <= numPoints; i++)
            {
                double px = radius * Math.Cos(i * frac);
                double py = radius * Math.Sin(i * frac);
                GL.Vertex2(px, py);
            }
            GL.End();

            GL.Enable(EnableCap.Lighting);
        }
    }

    public class SolarViewerView : InstrumentView
    {
        public float HackAngle = 0f;

        public SolarViewerView(OpenGLControlWrapper w, Shape center, float fov = 4f)
            : base(w, center)
        {
            Azimuth = 157.657f;
            Elevation = -0.109f;
            FieldOfView = fov;
        }

        public override void PaintInternal(OpenGLControlWrapper w)
        {
            base.PaintInternal(w);

            GL.Disable(EnableCap.Lighting);

            GL.MatrixMode(MatrixMode.Modelview); // This worked with the lookat in s/c frame
            GL.LoadIdentity();
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();

            int width = w.Width;
            int height = w.Height;
            float aspectRatio = width / (float)height;

            GL.Ortho(-aspectRatio, aspectRatio, -1.0, 1.0, 0.0, 4.0);
            GL.Viewport(0, 0, width, height);

            const double solarViewerFOVRadius = 0.5d;
            double radius = solarViewerFOVRadius * (2f / FieldOfView);
            const int numPoints = 32;
            const double frac = (2d * Math.PI) / numPoints;

            GL.Color3(Color.Yellow);
            GL.Begin(BeginMode.LineStrip);
            for (int i = 0; i <= numPoints; i++)
            {
                double px = radius * Math.Cos(i * frac);
                double py = radius * Math.Sin(i * frac);
                GL.Vertex2(px, py);
            }
            GL.End();

            GL.Enable(EnableCap.Lighting);
        }
    }

    public class JoystickCamera : Camera
    {
        public const float InitialWalkSpeed = 0.02f;
        public const float InitialTurnSpeed = 0.005f; //0.005d;
        public const float InitialStrafeSpeed = 0.003f;
        public const float RotateUpSpeed = -InitialWalkSpeed / 3f; //0.02d;

        private const float Slowdown = 2f;

        // Used to scale the motion based on framerate
        public float MotionScale = .2f;
        public float RotateSpeed = InitialTurnSpeed;
        public float StrafeSpeed = InitialStrafeSpeed;
        public float TurnSpeed = InitialTurnSpeed;
        public float WalkSpeed = InitialWalkSpeed;
        private Vector3 _forward;

        private bool _lastGoFast;
        private bool _lastGoSlow;
        private bool _lastTurnFast;
        private bool _lastTurnSlow;

        private Vector3 _right;

        public JoystickCamera(OpenGLControlWrapper w, Shape t)
            : base(w, t)
        {
            Wrapper = w;

            //_eye = new Vector3d(10d, 10d, 5d);
            // _lookat = new Vector3d(0.0d, 0.0d, 0.0d);

            ReverseSpeeds();  // Testing things
        }

        public void ReverseSpeeds()
        {
            RotateSpeed = -RotateSpeed;
            TurnSpeed = -TurnSpeed;
        }

        public override Vector3 Eye
        {
            get { return Target == null ? _eye : Target.Position + _eye; }
            set { _eye = Target == null ? value : value - Target.Position; }
        }

        public override Vector3 Lookat
        {
            get { return Target == null ? _lookat : Target.Position + _lookat; }
            set { _lookat = Target == null ? value : value - Target.Position; }
        }


        // Rebuild _forward and _right from _lookat, _eye and _up
        public void ResetVectors()
        {
            _forward = _lookat - _eye;
            _forward.Normalize();
            _up.Normalize();
            _right = Vector3.Cross(_forward, _up);
            _right.Normalize();
            _lookat = _eye + _forward;
        }

        public void Reset()
        {
            _eye = new Vector3(10f, 10f, 5f);
            _lookat = new Vector3(0.0f, 0.0f, 0.0f);
            StrafeSpeed = InitialStrafeSpeed;
            TurnSpeed = InitialTurnSpeed;
            WalkSpeed = InitialWalkSpeed;
        }

        public void MoveForward(float d)
        {
            Vector3 t = _forward * MotionScale * d;
            _eye += t;
            _lookat = _eye + _forward;
            _dirty = true;
            //Console.WriteLine(@"Joystick move forward");
        }

        public void MoveRight(float d)
        {
            Vector3 t = _right * MotionScale * d;
            _eye += t;
            _lookat = _eye + _forward;
            _dirty = true;
            //Console.WriteLine(@"Joystick move right");
        }

        public void Strafe(float xd, float yd)
        {
            Vector3 x = _right * MotionScale * xd;
            Vector3 y = _up * MotionScale * yd;
            _eye = _eye - x + y;
            _lookat = _eye + _forward;
            _dirty = true;
            //Console.WriteLine(@"Joystick strafe");
        }

        public void Rotate(float x, float y)
        {
            _forward = _forward + x * MotionScale * _right;
            _forward.Normalize();
            _right = Vector3.Cross(_forward, _up);
            _right.Normalize();

            _forward = _forward + y * MotionScale * _up;
            _forward.Normalize();
            _up = Vector3.Cross(_right, _forward);
            _up.Normalize();

            _lookat = _eye + _forward;
            _dirty = true;
            //Console.WriteLine(@"Joystick rotate");
        }

        public void RotateUp(float x)
        {
            _right = _right + x * RotateUpSpeed * _up;
            _right.Normalize();
            _up = Vector3.Cross(_right, _forward);
            _up.Normalize();
            _dirty = true;
            //Console.WriteLine(@"Joystick rotate up");
        }

        public void SlowDown()
        {
            WalkSpeed /= Slowdown;
            //TurnSpeed /= Slowdown;
            StrafeSpeed /= Slowdown;

            Console.WriteLine(@"WalkSpeed={0}", WalkSpeed);
        }

        public void SpeedUp()
        {
            WalkSpeed *= Slowdown;
            //TurnSpeed *= Slowdown;
            StrafeSpeed *= Slowdown;

            Console.WriteLine(@"WalkSpeed={0}", WalkSpeed);
        }

        public void TurnFaster()
        {
            RotateSpeed *= 1.2f;
            Console.WriteLine(@"Turn faster");
        }

        public void TurnSlower()
        {
            RotateSpeed /= 1.2f;
            Console.WriteLine(@"Turn slower");
        }

        public void Describe()
        {
            Console.WriteLine(@"Camera info:");
            Console.WriteLine(@"eye:    [" + Eye.X + "," + Eye.Y + "," + Eye.Z + "]");
            Console.WriteLine(@"lookat: [" + Lookat.X + "," + Lookat.Y + "," + Lookat.Z + "]");
        }

        public override void Tick()
        {
            UpdateInputState();
            //Console.WriteLine(@"Tick");
            //Describe();
            Wrapper.Invalidate();
        }

        public void UpdateInputState()
        {
            JoystickState state = Joystick.GetState(0);
            if (state.IsConnected)
            {
                if (false)
                {
                    Console.WriteLine(@"axis0={0}", state.GetAxis(JoystickAxis.Axis0)); // side to side
                    Console.WriteLine(@"axis1={0}", state.GetAxis(JoystickAxis.Axis1)); // forward back
                    Console.WriteLine(@"axis2={0}", state.GetAxis(JoystickAxis.Axis2)); // knob
                    Console.WriteLine(@"axis3={0}", state.GetAxis(JoystickAxis.Axis3)); // z-rotation
                    Console.WriteLine(@"axis4={0}", state.GetAxis(JoystickAxis.Axis4));
                    Console.WriteLine(@"axis5={0}", state.GetAxis(JoystickAxis.Axis5));
                    Console.WriteLine(@"axis6={0}", state.GetAxis(JoystickAxis.Axis6));
                    Console.WriteLine(@"axis7={0}", state.GetAxis(JoystickAxis.Axis7));

                    Console.WriteLine(@"button0={0}", state.GetButton(JoystickButton.Button0));
                    Console.WriteLine(@"button1={0}", state.GetButton(JoystickButton.Button1));
                    Console.WriteLine(@"button2={0}", state.GetButton(JoystickButton.Button2));
                    Console.WriteLine(@"button3={0}", state.GetButton(JoystickButton.Button3));
                    Console.WriteLine(@"button4={0}", state.GetButton(JoystickButton.Button4));
                    Console.WriteLine(@"button5={0}", state.GetButton(JoystickButton.Button5));
                    Console.WriteLine(@"button6={0}", state.GetButton(JoystickButton.Button6)); // F3
                    Console.WriteLine(@"button7={0}", state.GetButton(JoystickButton.Button7)); // F4
                    Console.WriteLine(@"button8={0}", state.GetButton(JoystickButton.Button8));
                    Console.WriteLine(@"button9={0}", state.GetButton(JoystickButton.Button9));
                    Console.WriteLine(@"button10={0}", state.GetButton(JoystickButton.Button10)); // hat up
                    Console.WriteLine(@"button11={0}", state.GetButton(JoystickButton.Button11)); // hat left
                    Console.WriteLine(@"button12={0}", state.GetButton(JoystickButton.Button12)); // hat right
                    Console.WriteLine(@"button13={0}", state.GetButton(JoystickButton.Button13)); // hat down
                    Console.WriteLine(@"button14={0}", state.GetButton(JoystickButton.Button14));
                    Console.WriteLine(@"button15={0}", state.GetButton(JoystickButton.Button15));

                    Console.ReadKey();
                }

                float x = state.GetAxis(JoystickAxis.Axis0); // used for rotating up
                float y = state.GetAxis(JoystickAxis.Axis1);
                float z = state.GetAxis(JoystickAxis.Axis3);
                float forward = state.GetAxis(JoystickAxis.Axis2);

                bool strafeUp = state.GetButton(JoystickButton.Button10) == ButtonState.Pressed;
                bool strafeLeft = state.GetButton(JoystickButton.Button11) == ButtonState.Pressed;
                bool strafeRight = state.GetButton(JoystickButton.Button12) == ButtonState.Pressed;
                bool strafeDown = state.GetButton(JoystickButton.Button13) == ButtonState.Pressed;
                bool reset = state.GetButton(JoystickButton.Button8) == ButtonState.Pressed;
                bool goSlow = state.GetButton(JoystickButton.Button1) == ButtonState.Pressed;
                bool goFast = state.GetButton(JoystickButton.Button3) == ButtonState.Pressed;
                bool turnSlow = state.GetButton(JoystickButton.Button6) == ButtonState.Pressed;
                bool turnFast = state.GetButton(JoystickButton.Button7) == ButtonState.Pressed;


                if (Math.Abs(forward) > 0.1f)
                    MoveForward(-forward * WalkSpeed);

                if (Math.Abs(x) > 0.1f)
                    RotateUp(x);

                if (strafeUp || strafeDown
                    || strafeRight
                    || strafeLeft)
                {
                    float strafeX = strafeLeft ? StrafeSpeed : strafeRight ? -StrafeSpeed : 0f;
                    float strafeY = strafeUp ? StrafeSpeed : strafeDown ? -StrafeSpeed : 0f;
                    Strafe(strafeX, strafeY);
                }

                if (z > 0.05f || z < -0.05f || y > 0.05f || y < -0.05f)
                {
                    //Console.Out.WriteLine("x="+z+" y="+z);
                    Rotate(z * RotateSpeed, y * RotateSpeed);
                }

                if (reset)
                    Reset();

                if (!_lastGoSlow && goSlow)
                    SlowDown();
                if (!_lastGoFast && goFast)
                    SpeedUp();
                _lastGoSlow = goSlow;
                _lastGoFast = goFast;

                if (!_lastTurnFast && turnFast)
                    TurnFaster();
                if (!_lastTurnSlow && turnSlow)
                    TurnSlower();
                _lastTurnFast = turnFast;
                _lastTurnSlow = turnSlow;
            }

            if (false) // this.Focused || GraphicsWindow.Focused)
            {
                //DirectInput.KeyboardState ks = keyboardDevice.GetCurrentKeyboardState();
                //float m = 1f;
                //if (ks[DirectInput.Key.LeftShift]) m = 10f;
                //if (ks[DirectInput.Key.W]) camera.MoveForward(m);
                //if (ks[DirectInput.Key.S]) camera.MoveForward(-m);
                //if (ks[DirectInput.Key.A]) camera.Strafe(-m, 0f);
                //if (ks[DirectInput.Key.D]) camera.Strafe(m, 0f);
                //if (ks[DirectInput.Key.E]) camera.Strafe(0f, m);
                //if (ks[DirectInput.Key.C]) camera.Strafe(0f, -m);
                //if (ks[DirectInput.Key.P]) camera.describe();
            }
        }
    }
}
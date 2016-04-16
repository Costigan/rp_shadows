using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OrbitOverview.Viz
{
    /// <summary>
    /// Summary description for MyCamera.
    /// </summary>
    public class MyCamera
    {
        private const float InitialWalkSpeed = 0.02f;
        private const float InitialTurnSpeed = 0.005f;
        private const float InitialStrafeSpeed = 0.003f;

        private const float Slowdown = 2f;
        private readonly Vector3 _up;

        public Vector3 Eye;
        public Vector3 LookAt;

        // Used to scale the motion based on framerate
        public float MotionScale = .2f;
        public float StrafeSpeed = 0.003f;
        public float TurnSpeed = 0.005f;
        public float WalkSpeed = 0.02f;
        private bool _dirty = true;
        private bool _lastGoFast;
        private bool _lastGoSlow;

        private Matrix4 _viewMatrix;
        //Matrix ProjectionMatrix;

        public MyCamera()
        {
            Eye = new Vector3(10f, 10f, 5f);
            LookAt = new Vector3(0.0f, 0.0f, 0.0f);
            _up = new Vector3(0.0f, 0.0f, 1.0f);

            // Set up our view matrix. A view matrix can be defined given an eye point,
            // a point to lookat, and a direction for which way is up. Here, we set the
            // eye five units back along the z-axis and up three units, look at the 
            // origin, and define "up" to be in the y-direction.
            //viewMatrix = Matrix.LookAtLH(eye, lookat, up);
            _viewMatrix = Matrix4.LookAt(Eye, LookAt, _up);

            // For the projection matrix, we set up a perspective transform (which
            // transforms geometry from 3D view space to 2D viewport space, with
            // a perspective divide making objects smaller in the distance). To build
            // a perpsective transform, we need the field of view (1/4 pi is common),
            // the aspect ratio, and the near and far clipping planes (which define at
            // what distances geometry should be no longer be rendered).


            // ProjectionMatrix = Matrix.PerspectiveFovLH( (float)(Math.PI / 4), 1.0f, 1.0f, 500.0f );

            // Done in StationViewer.cs
            //ProjectionMatrix = Matrix.PerspectiveFovLH( (float)(Math.PI / 4), 1.0f, 1.0f, 4000.0f );
        }

        public Matrix4 ViewMatrix
        {
            get { return _viewMatrix; }
        }

        public void Reset()
        {
            SetEye(10f, 10f, 5f);
            SetLookat(0.0f, 0.0f, 0.0f);
            StrafeSpeed = InitialStrafeSpeed;
            TurnSpeed = InitialTurnSpeed;
            WalkSpeed = InitialWalkSpeed;
        }

        public void SetEye(float x, float y, float z)
        {
            Eye.X = x;
            Eye.Y = y;
            Eye.Z = z;
            _dirty = true;
        }

        public void SetLookat(float x, float y, float z)
        {
            LookAt.X = x;
            LookAt.Y = y;
            LookAt.Z = z;
            _dirty = true;
        }

        public void MoveForward(float d)
        {
            Vector3 t = LookAt - Eye;
            d = d*MotionScale;
            t = (d/t.Length)*t;
            Eye += t;
            LookAt += t;
            _dirty = true;
        }

        public void MoveRight(float d)
        {
            Vector3 t = LookAt - Eye;
            Vector3 c = Vector3.Cross(t, _up);
            d = d*MotionScale;
            t = (d/(-c.Length))*c;
            Eye += t;
            LookAt += t;
            _dirty = true;
        }

        public void Strafe(float xd, float yd)
        {
            //Console.Out.WriteLine("StrafeX="+xd+" StrafeY="+yd);
            Vector3 forward = LookAt - Eye;
            Vector3 xv = Vector3.Cross(forward, _up);
            xv = xv*(1.0f/xv.Length);
            Vector3 yv = Vector3.Cross(forward, xv);
            yv = yv*(1.0f/yv.Length);

            xd = xd*MotionScale;
            yd = yd*MotionScale;

            xv = xv*(-xd); // - is a correction factor
            yv = yv*(-yd);

            xv += yv; // combine into one

            Eye += xv;
            LookAt += xv;

            _dirty = true;
        }

        public void Rotate(float x, float y)
        {
            //Console.Out.WriteLine("Rotate X="+x+" y="+y);

            Vector3 forward = LookAt - Eye;
            forward = forward*(1.0f/forward.Length);
            Vector3 xv = Vector3.Cross(forward, _up);
            xv = xv*(1.0f/xv.Length);
            Vector3 yv = Vector3.Cross(forward, xv);
            yv = yv*(1.0f/yv.Length);

            //x = x * motionScale * 0.2f;
            //y = y * motionScale * 0.2f;

            // Took out the motion scaling on rotation
            x = x*0.2f;
            y = y*0.2f;

            forward -= x*xv;
            forward -= y*yv;
            forward = forward*(1.0f/forward.Length);

            LookAt = Eye + forward;

            _dirty = true;
        }

        public void SlowDown()
        {
            WalkSpeed /= Slowdown;
            //TurnSpeed /= Slowdown;
            StrafeSpeed /= Slowdown;
        }

        public void SpeedUp()
        {
            WalkSpeed *= Slowdown;
            //TurnSpeed *= Slowdown;
            StrafeSpeed *= Slowdown;
        }

        private void UpdateView()
        {
            _viewMatrix = Matrix4.LookAt(Eye, LookAt, _up);
            _dirty = false;
        }

        public void SetupMatrices()
        {
            if (_dirty)
                UpdateView();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref _viewMatrix);

            //Matrix4 lookat = Matrix4.LookAt(10, 10, 5, 0, 0, 0, 0, 1, 0);
            //Matrix4 m = Matrix4.LookAt(Eye, LookAt, up);
            //GL.MatrixMode(MatrixMode.Modelview);
            //GL.LoadMatrix(ref m);

            // Moved to StationViewer.cs
            //device.Transform.Projection = ProjectionMatrix;
        }

        public void Describe()
        {
            Console.WriteLine(@"Camera info:");
            Console.WriteLine(@"eye:    [" + Eye.X + "," + Eye.Y + "," + Eye.Z + "]");
            Console.WriteLine(@"lookat: [" + LookAt.X + "," + LookAt.Y + "," + LookAt.Z + "]");
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
                    Console.WriteLine(@"button6={0}", state.GetButton(JoystickButton.Button6));
                    Console.WriteLine(@"button7={0}", state.GetButton(JoystickButton.Button7));
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

                if (Math.Abs(forward) > 0.1f)
                    MoveForward(-forward*WalkSpeed);

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
                    Rotate(-z*TurnSpeed, y*TurnSpeed);
                }

                if (reset)
                    Reset();

                if (!_lastGoSlow && goSlow)
                    SlowDown();
                if (!_lastGoFast && goFast)
                    SpeedUp();
                _lastGoSlow = goSlow;
                _lastGoFast = goFast;
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
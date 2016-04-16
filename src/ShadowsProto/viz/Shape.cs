using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Shadow.viz
{
    public abstract class Shape
    {
        public Quaternion Attitude = Quaternion.Identity;
        public float AxisScale = 1f;

        // The bounding sphere is defined wrt Position.
        // This isn't necessarily the most efficient, but
        // it's simple, since Position gets updated.
        public bool BoundingSphereDefined = false;
        public double BoundingSphereRadius;
        public VBO Buffer;
        public string Name;
        public Vector3d Position;

        public ShaderProgram Shader = null;
        public float Shininess = 0f;
        public bool ShowAxes;
        public float[] Specularity = new[] {0f, 0f, 0f};
        public bool Visible = true;

        public VBO LoadVBO<TVertex>(TVertex[] vertices, ushort[] elements, InterleavedArrayFormat format)
            where TVertex : struct
        {
            var handle = new VBO {VertexFormat = format};
            int size;

            // To create a VBO:
            // 1) Generate the buffer handles for the vertex and element buffers.
            // 2) Bind the vertex buffer handle and upload your vertex data. Check that the buffer was uploaded correctly.
            // 3) Bind the element buffer handle and upload your element data. Check that the buffer was uploaded correctly.

            GL.GenBuffers(1, out handle.VboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (vertices.Length*BlittableValueType.StrideOf(vertices)),
                          vertices,
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length*BlittableValueType.StrideOf(vertices) != size)
                throw new ApplicationException("Vertex data not uploaded correctly");

            GL.GenBuffers(1, out handle.EboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr) (elements.Length*sizeof (ushort)), elements,
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (elements.Length*sizeof (ushort) != size)
                throw new ApplicationException("Element data not uploaded correctly");

            handle.NumElements = elements.Length;
            return handle;
        }

        public VBO LoadVBO<TVertex>(TVertex[] vertices, uint[] elements, InterleavedArrayFormat format)
            where TVertex : struct
        {
            var handle = new VBO {VertexFormat = format};
            int size;

            // To create a VBO:
            // 1) Generate the buffer handles for the vertex and element buffers.
            // 2) Bind the vertex buffer handle and upload your vertex data. Check that the buffer was uploaded correctly.
            // 3) Bind the element buffer handle and upload your element data. Check that the buffer was uploaded correctly.

            GL.GenBuffers(1, out handle.VboID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, handle.VboID);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (vertices.Length*BlittableValueType.StrideOf(vertices)),
                          vertices,
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length*BlittableValueType.StrideOf(vertices) != size)
                throw new ApplicationException("Vertex data not uploaded correctly");

            GL.GenBuffers(1, out handle.EboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, handle.EboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr) (elements.Length*sizeof (uint)), elements,
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (elements.Length*sizeof (uint) != size)
                throw new ApplicationException("Element data not uploaded correctly");

            handle.NumElements = elements.Length;
            return handle;
        }

        public void TranslateFar(Vector3d pos, Vector3d eye)
        {
            Vector3d p = Position - eye;
            //Console.WriteLine(@"Translate  far {0} {1}", Name, p/1000d);
            //Console.WriteLine(@"  distance={0}", (p / 1000d).Length);
            GL.Translate((float) (p.X/1000d), (float) (p.Y/1000d), (float) (p.Z/1000d));
        }

        public void TranslateNear(Vector3d pos, Vector3d eye)
        {
            Vector3d nearpos = (pos - eye)*1000000d; // from units of 1000km to meters
            //Console.WriteLine(@"Translate near {0} {1}", Name, nearpos);
            GL.Translate(nearpos);
        }

        //todo
        public virtual void DrawInFrustum(bool near, Vector3d eye, float[,] frustum)
        {
            if (!BoundingSphereDefined || SphereInFrustum(Position, (float) BoundingSphereRadius, frustum))
                Draw(near, eye);
            else
            {
                Draw(near, eye);
            }
        }

        public virtual void Draw(bool near, Vector3d eye)
        {
            if (!Visible) return;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            Transform(near, eye); // Pushed one level
            Paint(); // Must be stack even
            GL.PopMatrix(); // Pop the matrix that was pushed in Transform.
        }

        public virtual void Transform(bool near, Vector3d eye)
        {
            if (near)
                TranslateNear(Position, eye);
            else
                TranslateFar(Position, eye);

            Vector3 axis;
            float angle;
            Attitude.ToAxisAngle(out axis, out angle);
            GL.Rotate(-angle*180.0f/3.141593f, axis); // Why - angle???
        }

        public virtual void Paint()
        {
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, Specularity);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, Shininess);

            // To draw a VBO:
            // 1) Ensure that the VertexArray client state is enabled.
            // 2) Bind the vertex and element buffer handles.
            // 3) Set up the data pointers (vertex, normal, color) according to your vertex format.
            // 4) Call DrawElements. (Note: the last parameter is an offset into the element buffer
            //    and will usually be IntPtr.Zero).
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.IndexArray);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer.VboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Buffer.EboID);

            GL.InterleavedArrays(Buffer.VertexFormat, 0, IntPtr.Zero);

            //GL.VertexPointer(3, VertexPointerType.Float, BlittableValueType.StrideOf(CubeVertices), new IntPtr(0));
            //GL.ColorPointer(4, ColorPointerType.UnsignedByte, BlittableValueType.StrideOf(CubeVertices), new IntPtr(12));

            GL.DrawElements(BeginMode.Triangles, Buffer.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);
            //GL.DrawElements(BeginMode.Lines, handle.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);
            //GL.DrawElements<Vector3>(BeginMode.Triangles, handle.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.IndexArray);
        }

        public static void FindNormal(ref Vector3 a, ref Vector3 b, ref Vector3 c, out Vector3 result)
        {
            Vector3 temp1, temp2;
            Vector3.Subtract(ref a, ref b, out temp1);
            temp1.Normalize();
            Vector3.Subtract(ref c, ref b, out temp2);
            temp2.Normalize();
            Vector3.Cross(ref temp1, ref temp2, out result);
            result *= -1.0f;
            result.Normalize();
        }

        public virtual void DrawSensors(Vector3d eye)
        {
        }

        public void PaintAxes(float scale)
        {
            GL.Disable(EnableCap.Lighting);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.Scale(scale, scale, scale);

            GL.Begin(BeginMode.Lines);
            GL.Color3(Color.Red);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(1f, 0f, 0f);
            GL.Color3(Color.Green);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, 1f, 0f);
            GL.Color3(Color.Blue);
            GL.Vertex3(0f, 0f, 0f);
            GL.Vertex3(0f, 0f, 1f);
            GL.End();

            GL.PopMatrix();
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

        public virtual void CalculateBoundingSphere()
        {
        }

        private bool SphereInFrustum(Vector3d pos, float radius, float[,] frustum)
        {
            Console.WriteLine(@"Testing frustum: {0}", Name);
            for (int p = 0; p < 6; p++)
                if (frustum[p, 0]*pos.X + frustum[p, 1]*pos.Y + frustum[p, 2]*pos.Z + frustum[p, 3] <= -radius)
                {
                    Console.WriteLine(@"SphereInFrustum {0} false", Name);
                    return false;
                }
            Console.WriteLine(@"SphereInFrustum {0} true", Name);
            return true;
        }
    }

    public class CubeShape : Shape
    {
        public float Scale = 1f;

        public override void Transform(bool near, Vector3d eye)
        {
            base.Transform(near, eye);
            GL.Scale(Scale, Scale, Scale);
        }

        public override void Paint()
        {
            GL.Disable(EnableCap.Lighting);
            GL.Begin(BeginMode.Quads);

            GL.Color3(Color.Silver);
            GL.Vertex3(-1.0f, -1.0f, -1.0f);
            GL.Vertex3(-1.0f, 1.0f, -1.0f);
            GL.Vertex3(1.0f, 1.0f, -1.0f);
            GL.Vertex3(1.0f, -1.0f, -1.0f);

            GL.Color3(Color.Honeydew);
            GL.Vertex3(-1.0f, -1.0f, -1.0f);
            GL.Vertex3(1.0f, -1.0f, -1.0f);
            GL.Vertex3(1.0f, -1.0f, 1.0f);
            GL.Vertex3(-1.0f, -1.0f, 1.0f);

            GL.Color3(Color.Moccasin);

            GL.Vertex3(-1.0f, -1.0f, -1.0f);
            GL.Vertex3(-1.0f, -1.0f, 1.0f);
            GL.Vertex3(-1.0f, 1.0f, 1.0f);
            GL.Vertex3(-1.0f, 1.0f, -1.0f);

            GL.Color3(Color.IndianRed);
            GL.Vertex3(-1.0f, -1.0f, 1.0f);
            GL.Vertex3(1.0f, -1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 1.0f);
            GL.Vertex3(-1.0f, 1.0f, 1.0f);

            GL.Color3(Color.PaleVioletRed);
            GL.Vertex3(-1.0f, 1.0f, -1.0f);
            GL.Vertex3(-1.0f, 1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, -1.0f);

            GL.Color3(Color.ForestGreen);
            GL.Vertex3(1.0f, -1.0f, -1.0f);
            GL.Vertex3(1.0f, 1.0f, -1.0f);
            GL.Vertex3(1.0f, 1.0f, 1.0f);
            GL.Vertex3(1.0f, -1.0f, 1.0f);

            GL.End();
            GL.Enable(EnableCap.Lighting);
        }
    }
}
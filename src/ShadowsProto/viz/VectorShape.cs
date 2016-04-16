using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LadeeViz.Viz
{
    public class VectorShape : Shape
    {
        public bool Prepared = false;

        public Color Color;
        public Vector3 Scale;
        //public Vector3d Vector = new Vector3d(0.5d, 0.5d, 0.707107d);
        public Vector3d Vector = new Vector3d(1d, 0d, 0d);

        public VertexNormal[] Vertices;
        public uint[] Elements;
        public int ElementCount;

        public VectorShape(Color color, float scaleX, float scaleY, float scaleZ)
        {
            if (!Prepared)
                Prepare();
            Color = color;
            const float factor = 30f;
            Scale = new Vector3(scaleX / factor, scaleY / factor, scaleZ / factor);
        }

        private void Prepare()
        {
            const float c = 44f;
            const float b = 40f;
            const float h = 2f;

            var v = new VertexNormal[13 * 4];

            // Bottom
            var p = 0;
            v[4*p+0].Position = new Vector3(0f, 1f, 1f);
            v[4*p+1].Position = new Vector3(0f, 1f, -1f);
            v[4*p+2].Position = new Vector3(0f, -1f, -1f);
            v[4*p+3].Position = new Vector3(0f, -1f, 1f);
            var n = new Vector3(-1f, 0f, 0f);
            v[4 * p + 0].Normal = n;
            v[4 * p + 1].Normal = n;
            v[4 * p + 2].Normal = n;
            v[4 * p + 3].Normal = n;

            // Bottom of arrowhead
            p++;
            v[4 * p + 0].Position = new Vector3(b, h, h);
            v[4 * p + 1].Position = new Vector3(b, h, -h);
            v[4 * p + 2].Position = new Vector3(b, -h, -h);
            v[4 * p + 3].Position = new Vector3(b, -h, h);
            v[4 * p + 0].Normal = n;
            v[4 * p + 1].Normal = n;
            v[4 * p + 2].Normal = n;
            v[4 * p + 3].Normal = n;

            // Side +Z
            p++;
            v[4 * p + 0].Position = new Vector3(0, 1, 1);
            v[4 * p + 1].Position = new Vector3(b, 1, 1);
            v[4 * p + 2].Position = new Vector3(b, -1, 1);
            v[4 * p + 3].Position = new Vector3(0, -1, 1);
            n = new Vector3(0f, 0f, 1f);
            v[4 * p + 0].Normal = n;
            v[4 * p + 1].Normal = n;
            v[4 * p + 2].Normal = n;
            v[4 * p + 3].Normal = n;

            // Side -Z
            p++;
            v[4 * p + 0].Position = new Vector3(0, 1, -1);
            v[4 * p + 1].Position = new Vector3(b, 1, -1);
            v[4 * p + 2].Position = new Vector3(b, -1, -1);
            v[4 * p + 3].Position = new Vector3(0, -1, -1);
            n = new Vector3(0f, 0f, -1f);
            v[4 * p + 0].Normal = n;
            v[4 * p + 1].Normal = n;
            v[4 * p + 2].Normal = n;
            v[4 * p + 3].Normal = n;

            // Side +Y
            p++;
            v[4 * p + 0].Position = new Vector3(0, 1, 1);
            v[4 * p + 1].Position = new Vector3(b, 1, 1);
            v[4 * p + 2].Position = new Vector3(b, 1, -1);
            v[4 * p + 3].Position = new Vector3(0, 1, -1);
            n = new Vector3(0f, 1f, 0f);
            v[4 * p + 0].Normal = n;
            v[4 * p + 1].Normal = n;
            v[4 * p + 2].Normal = n;
            v[4 * p + 3].Normal = n;

            // Side -Y
            p++;
            v[4 * p + 0].Position = new Vector3(0, -1, 1);
            v[4 * p + 1].Position = new Vector3(b, -1, 1);
            v[4 * p + 2].Position = new Vector3(b, -1, -1);
            v[4 * p + 3].Position = new Vector3(0, -1, -1);
            n = new Vector3(0f, -1f, 0f);
            v[4 * p + 0].Normal = n;
            v[4 * p + 1].Normal = n;
            v[4 * p + 2].Normal = n;
            v[4 * p + 3].Normal = n;

            // Side of arrowhead +Z
            p++;
            v[4 * p + 0].Position = new Vector3(c, 0, 0);
            v[4 * p + 1].Position = new Vector3(c, 0, 0);
            v[4 * p + 2].Position = new Vector3(b, 2, 2);
            v[4 * p + 3].Position = new Vector3(b, -2, 2);
            n = new Vector3(0f, 0f, 1f);  // Not right
            v[4 * p + 0].Normal = n;
            v[4 * p + 1].Normal = n;
            v[4 * p + 2].Normal = n;
            v[4 * p + 3].Normal = n;

            // Side of arrowhead -Z
            p++;
            v[4 * p + 0].Position = new Vector3(c, 0, 0);
            v[4 * p + 1].Position = new Vector3(c, 0, 0);
            v[4 * p + 2].Position = new Vector3(b, 2, -2);
            v[4 * p + 3].Position = new Vector3(b, -2, -2);
            n = new Vector3(0f, 0f, -1f);  // Not right
            v[4 * p + 0].Normal = n;
            v[4 * p + 1].Normal = n;
            v[4 * p + 2].Normal = n;
            v[4 * p + 3].Normal = n;

            // Side of arrowhead +Y
            p++;
            v[4 * p + 0].Position = new Vector3(c, 0, 0);
            v[4 * p + 1].Position = new Vector3(c, 0, 0);
            v[4 * p + 2].Position = new Vector3(b, 2, 2);
            v[4 * p + 3].Position = new Vector3(b, 2, -2);
            n = new Vector3(0f, 1f, 0f);  // Not right
            v[4 * p + 0].Normal = n;
            v[4 * p + 1].Normal = n;
            v[4 * p + 2].Normal = n;
            v[4 * p + 3].Normal = n;

            // Side of arrowhead -Y
            p++;
            v[4 * p + 0].Position = new Vector3(c, 0, 0);
            v[4 * p + 1].Position = new Vector3(c, 0, 0);
            v[4 * p + 2].Position = new Vector3(b, -2, 2);
            v[4 * p + 3].Position = new Vector3(b, -2, -2);
            n = new Vector3(0f, -1f, 0f);  // Not right
            v[4 * p + 0].Normal = n;
            v[4 * p + 1].Normal = n;
            v[4 * p + 2].Normal = n;
            v[4 * p + 3].Normal = n;

            var elts = new List<uint>();
            for (var i=0; i<7; i++)
                for (var i1=0; i1<4; i1++)
                    elts.Add((uint)(i*4+i1));

            Elements = elts.ToArray();
            Vertices = v;
            ElementCount = p * 4 + 4;

            Buffer = LoadVBO<VertexNormal>(v, Elements, OpenTK.Graphics.OpenGL.InterleavedArrayFormat.N3fV3f);
        }

        public override void Paint()
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.Scale(Scale);

            //GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.CullFace);
            Rotate();

            GL.Begin(BeginMode.Quads);
            GL.Color3(Color);
            for (var i = 0; i < ElementCount; i++)
            {
                GL.Normal3(Vertices[i].Normal);
                GL.Vertex3(Vertices[i].Position);
            }
            GL.End();

            //GL.Material(MaterialFace.Front, MaterialParameter.Specular, Specularity);
            //GL.Material(MaterialFace.Front, MaterialParameter.Shininess, Shininess);

            // To draw a VBO:
            // 1) Ensure that the VertexArray client state is enabled.
            // 2) Bind the vertex and element buffer handles.
            // 3) Set up the data pointers (vertex, normal, color) according to your vertex format.
            // 4) Call DrawElements. (Note: the last parameter is an offset into the element buffer
            //    and will usually be IntPtr.Zero).
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.IndexArray);

            //GL.ShadeModel(ShadingModel.Flat);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer.VboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Buffer.EboID);

            GL.InterleavedArrays(Buffer.VertexFormat, 0, IntPtr.Zero);

            //GL.VertexPointer(3, VertexPointerType.Float, BlittableValueType.StrideOf(CubeVertices), new IntPtr(0));
            //GL.ColorPointer(4, ColorPointerType.UnsignedByte, BlittableValueType.StrideOf(CubeVertices), new IntPtr(12));

            //GL.DrawElements(BeginMode.Quads, Buffer.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);
            GL.DrawElements(BeginMode.Quads, 4, DrawElementsType.UnsignedShort, IntPtr.Zero);
            //GL.DrawElements(BeginMode.Lines, Buffer.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);
            //GL.DrawElements<Vector3>(BeginMode.Triangles, handle.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.IndexArray);

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.CullFace);

            GL.PopMatrix(); 
        }

        private void Rotate()
        {
            var yrot = Math.Atan2(-Vector[2], Math.Sqrt(Vector[0] * Vector[0] + Vector[1] * Vector[1])) * 180d / Math.PI;
            var zrot = Math.Atan2(Vector[1], Vector[0]) * 180d / Math.PI;

            //Console.WriteLine(@"yrot={0}  zrot={1}", yrot, zrot);
            GL.Rotate(zrot, 0d, 0d, 1d);
            GL.Rotate(yrot, 0d, 1d, 0d);

        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shadow.terrain;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace Shadow.viz
{
    public class TerrainPatch : Shape
    {
        public Terrain TheTerrain;
        public Rectangle Bounds;
        public int Skip = 1;

        public bool DrawChildren = false;
        public List<TerrainPatch> Children = new List<TerrainPatch>();

        public NormalAverager[] NormalBuffer = new NormalAverager[0];

        public ShaderProgram Shader = null;
        public bool ShowTexture = true;
        public string TextureFilename;
        protected int TextureHandle;

        // For now, this doesn't include a texture
        public void Load()
        {
            LoadTexture();

            var xSize = Bounds.Width / Skip;
            var ySize = Bounds.Height / Skip;
            var vertices = new Vertex[xSize * ySize];
            var idx = 0;
            var left = Bounds.Left;
            var right = Bounds.Right;
            var top = Bounds.Top;
            var bottom = Bounds.Bottom;
            var width = (float)Bounds.Width;
            var height = (float)Bounds.Height;

            for (var x = left; x < right; x += Skip)
                for (var y = top; y < bottom; y += Skip)
                {
                    var v = TheTerrain.PointAt(x, y);
                    vertices[idx].Position.X = v.X;
                    vertices[idx].Position.Y = v.Y;
                    vertices[idx].Position.Z = v.Z;    // debugging - the vertical scale doesn't look right

                    vertices[idx].TexCoord.X = (x - left) / width;
                    vertices[idx].TexCoord.Y = (y - top) / height;

                    idx++;
                }

            // Define a mesh
            int xMax = xSize - 1;
            int yMax = ySize - 1;
            var elements = new uint[xMax * yMax * 6];
            int ptr = 0;
            for (int x = 0; x < xMax; x++)
                for (int y = 0; y < yMax; y++)
                {
                    int v = y * xSize + x;
                    // Reversing the rotation of these
                    elements[ptr++] = (uint)(v + xSize);
                    elements[ptr++] = (uint)(v + 1);
                    elements[ptr++] = (uint)v;

                    elements[ptr++] = (uint)(v + xSize);
                    elements[ptr++] = (uint)(v + xSize + 1);
                    elements[ptr++] = (uint)(v + 1);
                }

            var buf = GetNormalAverager(vertices.Length);
            for (int i = 0; i < vertices.Length; i++)
                buf[i].Reset();

            int triCount = elements.Length / 3;
            ptr = 0;
            for (int tri = 0; tri < triCount; tri++)
            {
                uint p1 = elements[ptr++];
                uint p2 = elements[ptr++];
                uint p3 = elements[ptr++];
                Vector3 v1 = vertices[p1].Position;
                Vector3 v2 = vertices[p2].Position;
                Vector3 v3 = vertices[p3].Position;

                Vector3 n;
                Shape.FindNormal(ref v1, ref v2, ref v3, out n);

                if (float.IsNaN(n.X))
                    throw new Exception(@"Got NaN when creating a mesh normal");

                buf[p1].Add(n);
                buf[p2].Add(n);
                buf[p3].Add(n);
            }
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 norm = buf[i].Normal / buf[i].Count;
                norm.Normalize();
                vertices[i].Normal = -norm;   // negated the normal at the same time I reversed the rotation of the tris
            }
            Color = Color.LightGray;
            Buffer = LoadVBO(vertices, elements, Vertex.Format);
        }

        public NormalAverager[] GetNormalAverager(int count)
        {
            if (NormalBuffer != null && NormalBuffer.Length >= count)
                return NormalBuffer;
            NormalBuffer = new NormalAverager[count];
            return NormalBuffer;
        }

        public void LoadTexture()
        {
            if (TextureFilename == null)
                return;
            GL.GenTextures(1, out TextureHandle);
            GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
            var bitmap1 = new Bitmap(TextureFilename);

            if (bitmap1.PixelFormat != System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
                throw new Exception("The texture's pixel format isn't Format8bppIndexed");

            var bitmap = new Bitmap(bitmap1.Width, bitmap1.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bitmap))
                g.DrawImageUnscaled(bitmap1, new Point(0, 0));

            // Debugging - this compensates for a bug in the shadow generation program
            // RotateNoneFlipY is close but maybe not  right
            //bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                              ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float)TextureEnvMode.Modulate);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                            (float)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                            (float)TextureMagFilter.Linear);

            // tell OpenGL to build mipmaps out of the bitmap data
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1.0f);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                          OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
        }

        public override void Paint()
        {
            if (ShowAxes)
                PaintAxes(AxisScale);

            if (ShowTexture && Shader != null)
                Shader.UseProgram();

            GL.Color3(Color);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, Specularity);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, Shininess);
            GL.ShadeModel(ShadingModel.Smooth);

            if (ShowTexture && TextureHandle != 0)
            {
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
            }

            // To draw a VBO:
            // 1) Ensure that the VertexArray client state is enabled.
            // 2) Bind the vertex and element buffer handles.
            // 3) Set up the data pointers (vertex, normal, color) according to your vertex format.
            // 4) Call DrawElements. (Note: the last parameter is an offset into the element buffer
            //    and will usually be IntPtr.Zero).
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.IndexArray);

            //GL.Disable(EnableCap.CullFace);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer.VboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Buffer.EboID);

            GL.InterleavedArrays(Buffer.VertexFormat, 0, IntPtr.Zero);

            //GL.VertexPointer(3, VertexPointerType.Float, BlittableValueType.StrideOf(CubeVertices), new IntPtr(0));
            //GL.ColorPointer(4, ColorPointerType.UnsignedByte, BlittableValueType.StrideOf(CubeVertices), new IntPtr(12));

            if (DrawLines)
                GL.DrawElements(BeginMode.Lines, Buffer.NumElements, DrawElementsType.UnsignedInt, IntPtr.Zero);
            else
                GL.DrawElements(BeginMode.Triangles, Buffer.NumElements, DrawElementsType.UnsignedInt, IntPtr.Zero);

            //GL.DrawElements<Vector3>(BeginMode.Triangles, handle.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);

            //GL.Enable(EnableCap.CullFace);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.IndexArray);

            if (ShowTexture && TextureHandle != 0)
            {
                GL.Disable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            if (ShowTexture && Shader != null)
                Shader.StopUsingProgram();
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NormalAverager
    {
        public int Count;
        public Vector3 Normal;

        public void Reset()
        {
            Count = 0;
            Normal.X = Normal.Y = Normal.Z = 0f;
        }

        public void Add(Vector3 v)
        {
            Count++;
            Normal.X += v.X;
            Normal.Y += v.Y;
            Normal.Z += v.Z;
        }
    }
}

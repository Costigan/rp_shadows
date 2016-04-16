using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Shadow.viz
{
    public abstract class TexturedShape : Shape
    {
        public static bool ShowTexture = true;
        public Color Color = Color.White;
        public int Texture = -1;
        public string TextureFilename = null;

        public virtual void LoadTexture()
        {
            if (TextureFilename == null)
                return;
            GL.GenTextures(1, out Texture);
            GL.BindTexture(TextureTarget.Texture2D, Texture);
            var bitmap = new Bitmap(TextureFilename);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                              ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (float) TextureEnvMode.Modulate);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                            (float) TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                            (float) TextureMagFilter.Linear);

            // tell OpenGL to build mipmaps out of the bitmap data
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1.0f);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                          OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            bitmap.UnlockBits(data);
        }

        public override void Paint()
        {
            // To draw a VBO:
            // 1) Ensure that the VertexArray client state is enabled.
            // 2) Bind the vertex and element buffer handles.
            // 3) Set up the data pointers (vertex, normal, color) according to your vertex format.
            // 4) Call DrawElements. (Note: the last parameter is an offset into the element buffer
            //    and will usually be IntPtr.Zero).

            if (ShowAxes)
                PaintAxes(AxisScale);

            if (Shader != null)
                Shader.UseProgram();

            GL.Color3(Color);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, Specularity);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, Shininess);
            GL.ShadeModel(ShadingModel.Smooth);

            GL.Enable(EnableCap.Texture2D);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Buffer.VboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Buffer.EboID);
            if (TextureFilename != null)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, Texture);
            }

            GL.InterleavedArrays(Buffer.VertexFormat, 0, IntPtr.Zero);

            if (ShowTexture)
                GL.DrawElements(BeginMode.Triangles, Buffer.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);
            else
                GL.DrawElements(BeginMode.LineStrip, Buffer.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);
            //GL.DrawElements<Vector3>(BeginMode.Triangles, handle.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.IndexArray);
            GL.Disable(EnableCap.Texture2D);

            if (TextureFilename != null)
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, 0);
            }

            if (Shader != null)
                Shader.StopUsingProgram();
        }
    }
}
using System;
using System.IO;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;

/*
 *             mvpMatrixLocation = GL.GetUniformLocation(Handle, "MVP");
            if (mvpMatrixLocation == -1)
                Console.WriteLine(@"Can't find location for MVP");
            mvMatrixLocation = GL.GetUniformLocation(Handle, "MV");
            if (mvMatrixLocation == -1)
                Console.WriteLine(@"Can't find location for MV");
            mMatrixLocation = GL.GetUniformLocation(Handle, "M");
            if (mMatrixLocation == -1)
                Console.WriteLine(@"Can't find location for M");
            LightPosition_worldspaceLocation = GL.GetUniformLocation(Handle, "LightPosition_worldspaceLocation");
            if (LightPosition_worldspaceLocation == -1)
                Console.WriteLine(@"Can't find location for LightPosition_worldspaceLocation");
 */

namespace Shadow.viz
{
    public class ShaderProgram
    {
        public static ShaderProgram CurrentProgram = null;

        public int Handle;
        // ReSharper disable InconsistentNaming
        public int LightPosition_worldspaceLocation;
        public int mMatrixLocation;
        public int mvMatrixLocation;
        public int mvpMatrixLocation;
        // ReSharper restore InconsistentNaming

        public ShaderProgram(string vertexShader, string fragmentShader)
        {
            LoadShaderProgram(vertexShader, fragmentShader, out Handle);
        }

        protected virtual void LoadShaderProgram(string vertexShader, string fragmentShader, out int programHandle)
        {
            Console.WriteLine(@"Loading {0} and {1}.", vertexShader, fragmentShader);
            string vertexShaderSource;
            string fragmentShaderSource;
            using (var sr = new StreamReader(vertexShader))
                vertexShaderSource = sr.ReadToEnd();
            using (var sr = new StreamReader(fragmentShader))
                fragmentShaderSource = sr.ReadToEnd();

            int vertexShaderHandle = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShaderHandle = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShaderHandle, vertexShaderSource);
            GL.ShaderSource(fragmentShaderHandle, fragmentShaderSource);

            GL.CompileShader(vertexShaderHandle);
            GL.CompileShader(fragmentShaderHandle);

            Console.WriteLine(GL.GetShaderInfoLog(vertexShaderHandle));
            Console.WriteLine(GL.GetShaderInfoLog(fragmentShaderHandle));

            // Create program
            programHandle = GL.CreateProgram();

            GL.AttachShader(programHandle, vertexShaderHandle);
            GL.AttachShader(programHandle, fragmentShaderHandle);

            GL.LinkProgram(programHandle);

            string infoString;
            int statusCode = 1;

            GL.GetProgram(programHandle, ProgramParameter.ValidateStatus, out statusCode);
            GL.GetProgramInfoLog(programHandle, out infoString);

            if (statusCode != 1 && !"".Equals(infoString))
            {
                MessageBox.Show("Error validating the shader: " + infoString);
            }
        }

        public virtual void UseProgram()
        {
            if (CurrentProgram == this) return;
            CurrentProgram = this;
            GL.UseProgram(Handle);
        }

        public virtual void StopUsingProgram()
        {
            GL.UseProgram(0);
            CurrentProgram = null;
        }

        private void BindTexture(ref int textureId, TextureUnit textureUnit, string uniformName)
        {
            GL.ActiveTexture(textureUnit);
            GL.BindTexture(TextureTarget.Texture2D, textureId);
            int loc = GL.GetUniformLocation(Handle, uniformName);
            if (loc > -1)
                GL.Uniform1(loc, textureUnit - TextureUnit.Texture0);
        }

        private int GenVertexArray(VBO vbo)
        {
            Console.WriteLine(@"In GenVertexArray");
            int vaoHandle;
            GL.GenVertexArrays(1, out vaoHandle);
            GL.BindVertexArray(vaoHandle);
            switch (vbo.VertexFormat)
            {
                case InterleavedArrayFormat.T2fN3fV3f:
                    {
                        int stride = 32;
                        GL.EnableVertexAttribArray(0);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VboID);
                        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 20);
                        GL.BindAttribLocation(Handle, 0, "vertexPosition_modelspace");

                        GL.EnableVertexAttribArray(1);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VboID);
                        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, true, stride, 8);
                        GL.BindAttribLocation(Handle, 1, "vertexNormal_modelspace");

                        GL.EnableVertexAttribArray(2);
                        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VboID);
                        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 0);
                        GL.BindAttribLocation(Handle, 2, "in_texture");
                        GL.BindVertexArray(0);
                    }
                    break;
                default:
                    throw new Exception(@"unrecognized InterleavedArrayFormat");
            }
            return vaoHandle;
        }
    }

    public class EarthShaderProgram : ShaderProgram
    {
        public int TextureUnit0Location, TextureUnit1Location;

        public EarthShaderProgram(string vertexShader, string fragmentShader) :
            base(vertexShader, fragmentShader)
        {
            TextureUnit0Location = GL.GetUniformLocation(Handle, "tex0");
            if (TextureUnit0Location == -1)
                Console.WriteLine(@"Can't find location for tex0");
            TextureUnit1Location = GL.GetUniformLocation(Handle, "tex1");
            if (TextureUnit1Location == -1)
                Console.WriteLine(@"Can't find location for tex1");
        }

        public override void UseProgram()
        {
            base.UseProgram();
            if (TextureUnit0Location > -1)
                GL.Uniform1(TextureUnit0Location, 0);
            if (TextureUnit1Location > -1)
                GL.Uniform1(TextureUnit1Location, 1);
        }
    }

    public class MoonShaderProgram : ShaderProgram
    {
        public int TextureUnit0Location;
        public int TextureUnit1Location;

        public MoonShaderProgram(string vertexShader, string fragmentShader) :
            base(vertexShader, fragmentShader)
        {
            TextureUnit0Location = GL.GetUniformLocation(Handle, "tex0");
            if (TextureUnit0Location == -1)
                Console.WriteLine(@"Can't find location for tex0");
            TextureUnit1Location = GL.GetUniformLocation(Handle, "tex1");
            if (TextureUnit1Location == -1)
                Console.WriteLine(@"Can't find location for tex1");
        }

        public override void UseProgram()
        {
            base.UseProgram();
            if (TextureUnit0Location > -1)
                GL.Uniform1(TextureUnit0Location, 0);
            if (TextureUnit1Location > -1)
                GL.Uniform1(TextureUnit1Location, 1);
        }
    }

    public class PhongRejection1 : ShaderProgram
    {
        public float AngleFactor = 0.01f;
        public int AngleFactorLocation;
        public float CenterX = 500f;
        public int CenterXLocation;
        public float CenterY = 500f;
        public int CenterYLocation;

        public PhongRejection1(string vertexShader, string fragmentShader) :
            base(vertexShader, fragmentShader)
        {
            CenterXLocation = GL.GetUniformLocation(Handle, "centerX");
            if (CenterXLocation == -1)
                Console.WriteLine(@"Can't find location for centerX");
            CenterYLocation = GL.GetUniformLocation(Handle, "centerY");
            if (CenterYLocation == -1)
                Console.WriteLine(@"Can't find location for centerY");
            AngleFactorLocation = GL.GetUniformLocation(Handle, "angleFactor");
            if (AngleFactorLocation == -1)
                Console.WriteLine(@"Can't find location for angleFactor");
        }

        public override void UseProgram()
        {
            base.UseProgram();
            if (CenterXLocation > -1)
                GL.Uniform1(CenterXLocation, CenterX);
            if (CenterYLocation > -1)
                GL.Uniform1(CenterYLocation, CenterY);
            if (AngleFactorLocation > -1)
                GL.Uniform1(AngleFactorLocation, AngleFactor);
        }
    }
}
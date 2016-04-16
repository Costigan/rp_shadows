using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace LadeeViz.Viz
{
    public class STKModel : Shape
    {
        public const float TelescopeProjection = 3000f;
        public const float SolarViewerProjection = 3000f;
        public static Vector3[] ConeVertices;
        public static Vector3[] ConeNormals;
        public static short[] ConeIndices;
        public float BoundRadius = 1f;
        public List<STKComponent> Components = new List<STKComponent>();
        public STKComponent Root;
        public bool ShowModel = true;

        public bool ShowSolarViewer = false;
        public bool ShowTelescope = false;
        public float SolarViewerAzimuth = 157.657f;
        public Color SolarViewerColor = Color.Yellow;
        public float SolarViewerElevation = -0.109f;
        public float SolarViewerRadius = (float) Math.Atan(0.5d*Math.PI/180d)*SolarViewerProjection;
        public float TelescopeAzimuth = 170.1f;
        public Color TelescopeColor = Color.LightBlue;
        public float TelescopeElevation = 0f;
        public float TelescopeRadius = (float) Math.Atan(0.5d*Math.PI/180d)*TelescopeProjection;

        public STKComponent[] Thrusters;

        public VectorShape VelocityVector = new VectorShape(Color.Red, 0.9f, 0.9f, 0.9f);
        public VectorShape MoonVector = new VectorShape(Color.White, 0.8f, 0.8f, 0.8f);
        public VectorShape EarthVector = new VectorShape(Color.LightBlue, 1f, 1f, 1f);
        public VectorShape SunVector = new VectorShape(Color.Yellow, 0.7f, 0.7f, 0.7f);

        public static STKModel Load(string filename)
        {
            var p = new STKModelParser(filename);
            var m = p.Load();
            m.Link();
            m.Install();
            m.FindThrusters();
            return m;
        }

        public void Link()
        {
            foreach (var c in Components)
                if (c.Root)
                    Root = c;
            foreach (var c in Components)
                c.Link(this);
        }

        public STKComponent Lookup(string name)
        {
            return Components.FirstOrDefault(c => name.Equals(c.Name));
        }

        public void Install()
        {
            foreach (var c in Components)
                c.Install();
        }

        public void FindThrusters()
        {
            var thrusters = Components.Where(c => c.Meshes.Count > 0 && c.Meshes[0].TextureHandle > 0).ToList();
            Thrusters = new STKComponent[thrusters.Count];
            Thrusters[4] = thrusters[0];
            Thrusters[0] = thrusters[1];
            Thrusters[1] = thrusters[2];
            Thrusters[3] = thrusters[3];
            Thrusters[2] = thrusters[4];
        }

        public void SetThrusterVisibility(byte visible)
        {
            Thrusters[0].Visible = (visible & 1) != 0;
            Thrusters[1].Visible = (visible & 2) != 0;
            Thrusters[2].Visible = (visible & 4) != 0;
            Thrusters[3].Visible = (visible & 8) != 0;
            Thrusters[4].Visible = (visible & 16) != 0;
        }

        public override void Draw(bool near, Vector3d eye)
        {
            if (!Visible) return;
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();  // Outer experiment
            GL.PushMatrix();
            Transform(near, eye); // Pushed one level
            Paint(); // Must be stack even
            GL.PopMatrix(); // Pop the matrix that was pushed in Transform.

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            if (near)
                TranslateNear(Position, eye);
            else
                TranslateFar(Position, eye);
            GL.Enable(EnableCap.Normalize);
            if (VelocityVector.Visible) VelocityVector.Paint();
            if (MoonVector.Visible) MoonVector.Paint();
            if (EarthVector.Visible) EarthVector.Paint();
            if (SunVector.Visible) SunVector.Paint();
            GL.Disable(EnableCap.Normalize);
            GL.PopMatrix(); // Pop the matrix that was pushed in Transform.
            GL.PopMatrix(); // Outer experiment
        }

        public override void Paint()
        {
            if (ShowAxes)
                PaintAxes(AxisScale);

            //if (VelocityVector.Visible)
            //    VelocityVector.Paint();

            if (!ShowModel) return;

            GL.EnableClientState(ArrayCap.VertexArray);
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.IndexArray);

            Root.Draw();

            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
            GL.DisableClientState(ArrayCap.IndexArray);

            const float x1 = -0.985109f; // azimuth=170.1 elevation=0
            const float y1 = 0.171929f;
            const float z1 = 0.0f;

            const float x2 = -0.924923f;
            const float y2 = 0.38015f;
            const float z2 = -1.902408e-3f;

            //DrawLine(new Vector3(0f, 0f, 0f), new Vector3(x1, y1, z1), Color.Blue);
            //DrawLine(new Vector3(0f, 0f, 0f), new Vector3(x2, y2, z2), Color.Yellow);
        }

        public static void MakeCone(int faces)
        {
            float f = 1f/faces;
            var temp = new List<Vector3>(faces);
            var origin = new Vector3(0f, 0f, 0f);

            for (int i = 0; i < faces + 1; i++)
            {
                double angle = i*f*2*Math.PI;
                temp.Add(new Vector3(1f, (float) Math.Cos(angle), (float) Math.Sin(angle)));
            }

            var vertices = new List<Vector3>(3*faces);
            var normals = new List<Vector3>(3*faces);
            var indices = new List<short>(3*faces);
            for (int face = 0; face < faces; face++)
            {
                Vector3 a = temp[face];
                Vector3 b = temp[face + 1];
                Vector3 c = origin;
                Vector3 n = Vector3.Cross(b - a, c - b);
                n.NormalizeFast();
                var ptr = (short) vertices.Count;
                vertices.Add(c);
                vertices.Add(b);
                vertices.Add(a);
                normals.Add(n);
                normals.Add(n);
                normals.Add(n);

                indices.Add(ptr++);
                indices.Add(ptr++);
                indices.Add(ptr);
            }

            ConeVertices = vertices.ToArray();
            ConeNormals = normals.ToArray();
            ConeIndices = indices.ToArray();
        }

        public override void DrawSensors(Vector3d eye)
        {
            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            TranslateNear(Position, eye);

            Vector3 axis;
            float angle;
            Attitude.ToAxisAngle(out axis, out angle);
            GL.Rotate(-angle*180.0f/3.141593f, axis); // removed the - angle

            if (!ShowTelescope && !ShowSolarViewer) return;
            GL.Disable(EnableCap.Lighting);
            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            if (ShowTelescope)
            {
                GL.PushMatrix();
                GL.Rotate(TelescopeElevation, 0f, 1f, 0f);
                GL.Rotate(TelescopeAzimuth, 0f, 0f, 1f);
                GL.Scale(TelescopeProjection, TelescopeRadius, TelescopeRadius);
                DrawCone(TelescopeColor);
                GL.PopMatrix();
            }

            if (ShowSolarViewer)
            {
                GL.PushMatrix();
                GL.Rotate(SolarViewerElevation, 0f, 1f, 0f);
                GL.Rotate(SolarViewerAzimuth, 0f, 0f, 1f);
                GL.Scale(SolarViewerProjection, SolarViewerRadius, SolarViewerRadius);
                DrawCone(SolarViewerColor);
                GL.PopMatrix();
            }

            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.Lighting);
            GL.PopMatrix();
        }

        public void DrawCone(Color c)
        {
            if (ConeNormals == null) return;

            GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();

            GL.Color4(c.R, c.G, c.B, (byte) 100);
            GL.Begin(BeginMode.Triangles);
            var length = ConeVertices.Length;
            for (var i = 0; i < length; i++)
            {
                GL.Normal3(ConeNormals[i]);
                GL.Vertex3(ConeVertices[i]);
            }
            GL.End();

            GL.PopMatrix();
        }
    }

    public class STKComponent : Shape
    {
        public string AttachPoint;
        public List<STKMesh> Meshes = new List<STKMesh>();
        public List<STKReference> References = new List<STKReference>(2);
        public bool Root = false;
        public Vector3 Translate;

        public void Link(STKModel m)
        {
            foreach (var r in References)
                r.Component = m.Lookup(r.Name);
        }

        public void Draw()
        {
            if (!Visible)
                return;
            var count = Meshes.Count;
            for (int i = 0; i < count; i++)
                Meshes[i].Draw();
            count = References.Count;
            for (int i = 0; i < count; i++)
                References[i].Draw();
        }

        internal void Install()
        {
            foreach (STKMesh m in Meshes)
                m.Install();
        }
    }

    public class STKReference
    {
        public STKComponent Component;
        public string Name;
        public Vector3 Translate;

        public void Draw()
        {
            if (Component == null) return;
            GL.PushMatrix();
            //GL.Translate(Translate);   // The translations apparently are just for attachment points, not the geometry
            Component.Draw();
            GL.PopMatrix();
        }
    }

    public class STKMesh
    {
        public static float[] BlankMaterial = new[] {0f, 0f, 0f};
        public bool BackfaceCullable = true;
        public Color FaceColor = Color.White;
        public Color FaceEmissionColor;
        public VBO Handle;
        public bool NoDiffuseLighting = false; // not implemented
        public int NumPolys = 0;
        public int NumVerts = 0;
        public List<short> Polys;
        public float Shininess = 0f;
        public bool Show = true;
        public bool SmoothShading = true;
        public float[] Specularity = new[] {0f, 0f, 0f};
        public List<Vector2> TextureCoords;
        public int TextureHandle;
        public STKTexture TextureSpec;
        public float Translucency = 0f;
        public InterleavedArrayFormat VertexFormat = InterleavedArrayFormat.N3fV3f;
        public List<Vector3> Vertices;

        public void Install()
        {
            if (Handle.NumElements > 0) return; // already installed
            if (Vertices == null || Vertices.Count < 1) return;

            Handle = new VBO();
            int size;

            Vector3[] normals = CalculateNormals();
            short[] elements = Polys.ToArray();

            if (TextureSpec == null)
            {
                // Build the vertices array without texture coords
                var vertices = new VertexNormal[Vertices.Count];

                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].Normal = normals[i];
                    vertices[i].Position = Vertices[i];
                }

                GL.GenBuffers(1, out Handle.VboID);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Handle.VboID);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (vertices.Length*BlittableValueType.StrideOf(vertices)),
                              vertices, BufferUsageHint.StaticDraw);
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
                if (vertices.Length*BlittableValueType.StrideOf(vertices) != size)
                    throw new ApplicationException("Vertex data not uploaded correctly");

                // Element buffer will be created below
            }
            else
            {
                // Build the vertices array with texture coords
                var vertices = new Vertex[Vertices.Count];
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].TexCoord = TextureCoords[i];
                    vertices[i].Normal = normals[i];
                    vertices[i].Position = Vertices[i];
                }

                GL.GenBuffers(1, out Handle.VboID);
                GL.BindBuffer(BufferTarget.ArrayBuffer, Handle.VboID);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (vertices.Length*BlittableValueType.StrideOf(vertices)),
                              vertices, BufferUsageHint.StaticDraw);
                GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
                if (vertices.Length*BlittableValueType.StrideOf(vertices) != size)
                    throw new ApplicationException("Vertex data not uploaded correctly");

                VertexFormat = InterleavedArrayFormat.T2fN3fV3f;

                GL.GenTextures(1, out TextureHandle);
                GL.BindTexture(TextureTarget.Texture2D, TextureHandle); // Not handling error

                var bitmap = new Bitmap(TextureSpec.RGB);
                Console.WriteLine(@"Binding {0} to {1}", TextureHandle, TextureSpec.RGB);

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                  ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

                GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode,
                          (float) TextureEnvMode.Modulate);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                                (float) TextureMinFilter.LinearMipmapLinear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                                (float) TextureMagFilter.Linear);

                // tell OpenGL to build mipmaps out of the bitmap data
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1.0f);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                              OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

                bitmap.UnlockBits(data);

                //Show = false;  // This turns everything with a texture off.  Right now, that's just the flames.
            }

            GL.GenBuffers(1, out Handle.EboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle.EboID);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr) (elements.Length*sizeof (short)), elements,
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (elements.Length*sizeof (short) != size)
                throw new ApplicationException("Element data not uploaded correctly");

            Handle.NumElements = elements.Length;
        }

        public Vector3[] CalculateNormals()
        {
            var normals = new Vector3[Vertices.Count];
            for (int i = 0; i < NumPolys; i++)
            {
                short p1 = Polys[i*3];
                short p2 = Polys[i*3 + 1];
                short p3 = Polys[i*3 + 2];
                Vector3 v1 = Vertices[p2] - Vertices[p1];
                Vector3 v2 = Vertices[p3] - Vertices[p2];
                Vector3 n = Vector3.Cross(v2, v1);
                n.Normalize();
                normals[p1] += n;
                normals[p2] += n;
                normals[p3] += n;
            }
            for (int i = 0; i < normals.Length; i++)
            {
                Vector3 n = normals[i];
                n.Normalize();
                normals[i] = -n; // not sure
            }
            return normals;
        }

        public void Draw()
        {
            if (!Show)
                return;

            if (!SmoothShading)
                GL.ShadeModel(ShadingModel.Flat);

            //GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
            //GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Shininess, 128f);  // Shininess
            //GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, 1f);  //Specularity

            if (TextureHandle > 0)
            {
                GL.Disable(EnableCap.Lighting);
                GL.Enable(EnableCap.Texture2D);
                GL.BindTexture(TextureTarget.Texture2D, TextureHandle);
                //Console.WriteLine(@"TextureHandle={0}", TextureHandle);
                GL.Color3(Color.White);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            else
                GL.Color3(FaceColor);

            GL.Material(MaterialFace.Front, MaterialParameter.Specular, Specularity);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, Shininess);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Handle.VboID);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, Handle.EboID);

            GL.InterleavedArrays(VertexFormat, 0, IntPtr.Zero);
            GL.DrawElements(BeginMode.Triangles, Handle.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);

            if (TextureHandle > 0)
            {
                GL.BindTexture(TextureTarget.Texture2D, 0);
                GL.Disable(EnableCap.Texture2D);
                GL.Disable(EnableCap.Blend);
                GL.Enable(EnableCap.Lighting);
            }

            if (!SmoothShading)
                GL.ShadeModel(ShadingModel.Smooth);
        }
    }

    public class STKTexture
    {
        public string Alpha;
        public string Parm;
        public string RGB;
    }

    public class STKModelParser
    {
        public static char[] SplitChars = new[] {' ', '\t'};
        public string Filename;

        public STKModelParser(string filename)
        {
            Filename = filename;
        }

        public STKModel Load()
        {
            var m = new STKModel();
            var components = new List<STKComponent>();
            string[] tokens;
            using (var sr = new StreamReader(Filename))
                while ((tokens = ReadTokens(sr)) != null)
                    switch (tokens[0].ToLower())
                    {
                        case "component":
                            components.Add(ReadComponent(sr, tokens[1]));
                            break;
                        case "boundradius":
                            m.BoundRadius = float.Parse(tokens[1]);
                            break;
                        default:
                            throw new Exception(string.Format(@"Unrecognized line: {0}", Concat(tokens)));
                    }
            m.Components = components;
            return m;
        }

        private string Concat(string[] tokens)
        {
            var sb = new StringBuilder();
            foreach (var t in tokens)
            {
                sb.Append(t);
                sb.Append(' ');
            }
            return sb.ToString();
        }

        private STKComponent ReadComponent(StreamReader sr, string name)
        {
            var c = new STKComponent {Name = name};
            string[] tokens;
            while ((tokens = ReadTokens(sr)) != null)
                switch (tokens[0].ToLower())
                {
                    case "endcomponent":
                        return c;
                    case "polygonmesh":
                        c.Meshes.Add(ReadPolygonMesh(sr));
                        break;
                    case "attachpoint":
                        c.AttachPoint = tokens[1];
                        break;
                    case "translate":
                        c.Translate = ReadVector3(tokens);
                        break;
                    case "refer":
                        c.References.Add(ReadReference(sr));
                        break;
                    case "root":
                        c.Root = true;
                        break;
                    default:
                        throw new Exception(string.Format(@"Unrecognized line: {0}", Concat(tokens)));
                }
            return c;
        }

        private string ReadLine(StreamReader sr)
        {
            string l;
            for (l = sr.ReadLine(); !string.IsNullOrEmpty(l) && l[0] == '#';)
                l = sr.ReadLine();
            if (l == null) return null;
            int i = l.IndexOf('#');
            if (i > -1)
                l = l.Substring(0, i);
            return l;
        }

        private string[] ReadTokens(StreamReader sr)
        {
            string l;
            return (l = ReadLine(sr)) == null ? null : l.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
        }

        private string Argument(string l, int n)
        {
            if (l == null) return null;
            var tokens = l.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            return tokens.Length < n - 1 ? null : tokens[n];
        }

        private STKMesh ReadPolygonMesh(StreamReader sr)
        {
            var m = new STKMesh();
            string[] tokens;
            while ((tokens = ReadTokens(sr)) != null)
                switch (tokens[0].ToLower())
                {
                    case "endpolygonmesh":
                        return m;
                    case "facecolor":
                        m.FaceColor = ReadColor(tokens);
                        break;
                    case "faceemissioncolor":
                        m.FaceEmissionColor = ReadColor(tokens);
                        break;
                    case "nodiffuselighting":
                        m.NoDiffuseLighting = true;
                        break;
                    case "backfacecullable":
                        m.BackfaceCullable = ReadYesNo(tokens);
                        break;
                    case "smoothshading":
                        m.SmoothShading = ReadYesNo(tokens);
                        break;
                    case "translucency":
                        m.Translucency = ReadFloat(tokens);
                        break;
                    case "specularity":
                        {
                            float s = ReadFloat(tokens);
                            m.Specularity = new[] {s, s, s};
                        }
                        break;
                    case "shininess":
                        m.Shininess = ReadFloat(tokens);
                        break;
                    case "texture":
                        m.TextureSpec = ReadTexture(sr);
                        break;
                    case "numverts":
                        m.NumVerts = ReadInt(tokens);
                        break;
                    case "datatx":
                        ReadTexturedVertices(sr, m);
                        break;
                    case "data":
                        ReadVertices(sr, m);
                        break;
                    case "numpolys":
                        m.NumPolys = ReadInt(tokens);
                        break;
                    case "polys":
                        ReadPolys(sr, m);
                        break;
                    default:
                        throw new Exception(string.Format(@"Unrecognized line: {0}", Concat(tokens)));
                }
            return m;
        }

        private void ReadVertices(StreamReader sr, STKMesh m)
        {
            int numVerts = m.NumVerts;
            var verts = new List<Vector3>(numVerts);
            for (int i = 0; i < numVerts; i++)
                verts.Add(ReadVector3(sr));
            m.Vertices = verts;
        }

        private Vector3 ReadVector3(StreamReader sr)
        {
            string l = ReadLine(sr);
            string[] tokens = l.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
            return new Vector3(float.Parse(tokens[0]),
                               float.Parse(tokens[1]),
                               float.Parse(tokens[2]));
        }

        private Vector3 ReadVector3(string[] tokens) // assume a keyword before 3 floats
        {
            return new Vector3(float.Parse(tokens[1]), float.Parse(tokens[2]), float.Parse(tokens[3]));
        }

        private void ReadTexturedVertices(StreamReader sr, STKMesh m)
        {
            int numVerts = m.NumVerts;
            var verts = new List<Vector3>(numVerts);
            var texts = new List<Vector2>(numVerts);
            for (int i = 0; i < numVerts; i++)
            {
                string l = ReadLine(sr);
                string[] t = l.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                float[] n = t.Select(float.Parse).ToArray();
                verts.Add(new Vector3(n[0], n[1], n[2]));
                texts.Add(new Vector2(n[3], n[4]));
            }
            m.Vertices = verts;
            m.TextureCoords = texts;
        }

        private void ReadPolys(StreamReader sr, STKMesh m)
        {
            var polys = new List<short>();
            int numPolys = m.NumPolys;
            for (int i = 0; i < numPolys; i++)
            {
                string l = ReadLine(sr);
                string[] t = l.Split(SplitChars, StringSplitOptions.RemoveEmptyEntries);
                short[] n = t.Select(short.Parse).ToArray();
                if (n.Length != n[0] + 1)
                    throw new Exception("Malformed poly: " + l);
                if (n[0] < 3)
                    throw new Exception("Poly too small: " + l);
                if (n[0] == 3)
                {
                    polys.Add(n[1]); // was 1,2,3
                    polys.Add(n[2]);
                    polys.Add(n[3]);
                }
                else
                {
                    for (int j = 2; j < n.Length - 1; j++)
                    {
                        polys.Add(n[1]);
                        polys.Add(n[j]);
                        polys.Add(n[j + 1]);
                    }
                }
                m.Polys = polys;
            }
        }

        private int ReadInt(string[] tokens)
        {
            return int.Parse(tokens[1]);
        }

        private STKTexture ReadTexture(StreamReader sr)
        {
            var t = new STKTexture();
            string[] tokens;
            while ((tokens = ReadTokens(sr)) != null)
                switch (tokens[0].ToLower())
                {
                    case "endtexture":
                        return t;
                    case "rgb":
                        t.RGB = tokens[1];
                        break;
                    case "alpha":
                        t.Alpha = tokens[1];
                        break;
                    case "parm":
                        t.Parm = tokens[1];
                        break;
                    default:
                        throw new Exception(string.Format(@"Unrecognized line: {0}", Concat(tokens)));
                }
            return t;
        }

        private float ReadFloat(string[] tokens)
        {
            return float.Parse(tokens[1]);
        }

        private bool ReadYesNo(string[] tokens)
        {
            return "yes".Equals(tokens[1].ToLowerInvariant());
        }

        private Color ReadColor(string[] tokens)
        {
            string s = tokens[1];
            string red = s.Substring(1, 3);
            string green = s.Substring(4, 3);
            string blue = s.Substring(7, 3);
            return Color.FromArgb(int.Parse(red), int.Parse(green), int.Parse(blue));
        }

        private STKReference ReadReference(StreamReader sr)
        {
            var r = new STKReference();
            string[] tokens;
            while ((tokens = ReadTokens(sr)) != null)
                switch (tokens[0].ToLower())
                {
                    case "endrefer":
                        return r;
                    case "component":
                        r.Name = tokens[1];
                        break;
                    case "translate":
                        r.Translate = ReadVector3(tokens);
                        break;
                    case "articulation":
                        ReadArticulation(sr);
                        break;
                    default:
                        throw new Exception(string.Format(@"Unrecognized line: {0}", Concat(tokens)));
                }
            return r;
        }

        private void ReadArticulation(StreamReader sr)
        {
            string[] tokens;
            while ((tokens = ReadTokens(sr)) != null)
                switch (tokens[0].ToLower())
                {
                    case "endarticulation":
                        return;
                }
        }
    }
}
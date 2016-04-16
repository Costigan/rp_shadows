using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Drawing;

namespace LadeeViz.Viz
{

    public class WavefrontShape : Shape
    {
        public string WavefrontFilename = null;
        public int _vboid;
        List<ColoredTriArray> _coloredIndices;

        public bool ShowTelescope = true;
        public const float TelescopeProjection = 300f;
        public float TelescopeRadius = (float)Math.Atan(0.5d*Math.PI/180d)*TelescopeProjection;
        public float TelescopeAzimuth = 170.1f;
        public float TelescopeElevation = 0f;
        public Color TelescopeColor = Color.LightBlue;

        public bool ShowSolarViewer = true;
        public const float SolarViewerProjection = 300f;
        public float SolarViewerRadius = (float)Math.Atan(0.5d * Math.PI / 180d) * TelescopeProjection;
        public float SolarViewerAzimuth = 157.657f;
        public float SolarViewerElevation = -0.109f;
        public Color SolarViewerColor = Color.Yellow;

        public bool ShowModel = true;

        public float DebugFactor = 0f;
        public InterleavedArrayFormat VertexFormat;

        public void Load()
        {
            if (WavefrontFilename == null)
                return;

            var mesh = new WavefrontLoader().LoadFile(WavefrontFilename);

            Vertex[] vertices;
            int size;

            mesh.OpenGLArrays(out vertices, out _coloredIndices);

            // To create a VBO:
            // 1) Generate the buffer handles for the vertex and element buffers.
            // 2) Bind the vertex buffer handle and upload your vertex data. Check that the buffer was uploaded correctly.
            // 3) Bind the element buffer handle and upload your element data. Check that the buffer was uploaded correctly.

            GL.GenBuffers(1, out _vboid);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vboid);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * BlittableValueType.StrideOf(vertices)), vertices,
                          BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * BlittableValueType.StrideOf(vertices) != size)
                throw new ApplicationException("Vertex data not uploaded correctly");

            VertexFormat = InterleavedArrayFormat.T2fN3fV3f;
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

            if (ShowModel)
            {
                GL.EnableClientState(ArrayCap.ColorArray);
                GL.EnableClientState(ArrayCap.VertexArray);
                GL.EnableClientState(ArrayCap.IndexArray);
                //GL.EnableClientState(ArrayCap.TextureCoordArray);
                //GL.Enable(EnableCap.Texture2D);

                GL.BindBuffer(BufferTarget.ArrayBuffer, _vboid);
                GL.InterleavedArrays(VertexFormat, 0, IntPtr.Zero);

                foreach (var c in _coloredIndices)
                {
                    GL.Color3(c.Color.X, c.Color.Y, c.Color.Z);
                    var indices = c.Indices;

                    GL.DrawElements(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedShort, indices);
                    //GL.DrawElements(BeginMode.Lines, indices.Length, DrawElementsType.UnsignedShort, indices);
                    //GL.DrawElements<Vector3>(BeginMode.Triangles, indices.Length, DrawElementsType.UnsignedShort, indices);
                }

                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.DisableClientState(ArrayCap.ColorArray);
                GL.DisableClientState(ArrayCap.VertexArray);
                GL.DisableClientState(ArrayCap.IndexArray);

            }
        }

        public static Vector3[] ConeVertices;
        public static Vector3[] ConeNormals;
        public static short[] ConeIndices;

        public static void MakeCone(int faces)
        {
            float f = 1f / faces;
            var temp = new List<Vector3>(faces);
            var origin = new Vector3(0f, 0f, 0f);

            for (var i = 0; i < faces + 1; i++)
            {
                var angle = i * f * 2 * Math.PI;
                temp.Add(new Vector3(1f, (float)Math.Cos(angle), (float)Math.Sin(angle)));
            }

            var vertices = new List<Vector3>(3 * faces);
            var normals = new List<Vector3>(3 * faces);
            var indices = new List<short>(3 * faces);
            for (var face = 0; face < faces; face++)
            {
                var a = temp[face];
                var b = temp[face + 1];
                var c = origin;
                var n = Vector3.Cross(b - a, c - b);
                n.NormalizeFast();
                var ptr = (short)vertices.Count;
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
            GL.Rotate(-angle * 180.0f / 3.141593f, axis);  // removed the - angle

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

            GL.Color4(c.R, c.G, c.B, (byte)100);
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

    public class WavefrontLoader
    {
        readonly Dictionary<string, MyVector3> _materials = new Dictionary<string, MyVector3>();
        public MeshData LoadStream(Stream stream)
        {
            var reader = new StreamReader(stream);
            var points = new List<MyVector3>();
            var normals = new List<MyVector3>();
            var texCoords = new List<MyVector2>();
            var tris = new ColoredTris();
            var coloredTris = new List<ColoredTris>();
            string line;
            char[] splitChars = { ' ' };
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim(splitChars);
                line = line.Replace("  ", " ");
                var parameters = line.Split(splitChars);
                switch (parameters[0])
                {
                    case "mtllib":
                        LoadMaterials(parameters[1]);
                        break;
                    case "p":
                        // Point
                        break;

                    case "v":
                        // Vertex
                        float x = float.Parse(parameters[1], CultureInfo.InvariantCulture.NumberFormat);
                        float y = float.Parse(parameters[2], CultureInfo.InvariantCulture.NumberFormat);
                        float z = float.Parse(parameters[3], CultureInfo.InvariantCulture.NumberFormat);
                        points.Add(new MyVector3(x, y, z));
                        break;

                    case "vt":
                        // TexCoord
                        float u = float.Parse(parameters[1], CultureInfo.InvariantCulture.NumberFormat);
                        float v = float.Parse(parameters[2], CultureInfo.InvariantCulture.NumberFormat);
                        texCoords.Add(new MyVector2(u, v));
                        break;

                    case "vn":
                        // Normal
                        float nx = float.Parse(parameters[1], CultureInfo.InvariantCulture.NumberFormat);
                        float ny = float.Parse(parameters[2], CultureInfo.InvariantCulture.NumberFormat);
                        float nz = float.Parse(parameters[3], CultureInfo.InvariantCulture.NumberFormat);
                        normals.Add(new MyVector3(nx, ny, nz));
                        break;

                    case "f":
                        // Face
                        tris.Tris.AddRange(ParseFace(parameters));
                        break;

                    case "usemtl":
                        {
                            if (tris.Tris.Count > 0)
                                coloredTris.Add(tris);
                            if (!_materials.ContainsKey(parameters[1]))
                                throw new Exception(string.Format(@"Unrecognized material: {0}", parameters[1]));
                            tris = new ColoredTris {Color = _materials[parameters[1]]};
                        }
                        break;
                }
            }

            MyVector3[] p = points.ToArray();
            MyVector2[] tc = texCoords.ToArray();
            MyVector3[] n = normals.ToArray();

            // If there are no specified texcoords or normals, we add a dummy one.
            // That way the Points will have something to refer to.
            if (tc.Length == 0)
            {
                tc = new MyVector2[1];
                tc[0] = new MyVector2(0, 0);
            }
            if (n.Length == 0)
            {
                n = new MyVector3[1];
                n[0] = new MyVector3(1, 0, 0);
            }

            return new MeshData(p, n, tc, coloredTris);
        }

        public MeshData LoadFile(string file)
        {
            // Silly me, using() closes the file automatically.
            using (var s = File.Open(file, FileMode.Open))
            {
                return LoadStream(s);
            }
        }

        private static Tri[] ParseFace(string[] indices)
        {
            var p = new MyPoint[indices.Length - 1];
            for (int i = 0; i < p.Length; i++)
            {
                p[i] = ParsePoint(indices[i + 1]);
            }
            return Triangulate(p);
            //return new Face(p);
        }

        // Takes an array of points and returns an array of triangles.
        // The points form an arbitrary polygon.
        private static Tri[] Triangulate(MyPoint[] ps)
        {
            var ts = new List<Tri>();
            if (ps.Length < 3)
            {
                throw new Exception("Invalid shape!  Must have >2 points");
            }

            MyPoint lastButOne = ps[1];
            MyPoint lastButTwo = ps[0];
            for (int i = 2; i < ps.Length; i++)
            {
                Tri t = new Tri(lastButTwo, lastButOne, ps[i]);
                lastButOne = ps[i];
                lastButTwo = ps[i - 1];
                ts.Add(t);
            }
            return ts.ToArray();
        }

        private static MyPoint ParsePoint(string s)
        {
            char[] splitChars = { '/' };
            var parameters = s.Split(splitChars);
            int tex = 0, norm = 0;
            var vert = int.Parse(parameters[0]) - 1;
            // Texcoords and normals are optional in .obj files
            if (parameters[1] != "")
            {
                tex = int.Parse(parameters[1]) - 1;
            }
            if (parameters[2] != "")
            {
                norm = int.Parse(parameters[2]) - 1;
            }
            return new MyPoint(vert, norm, tex);
        }

        private void LoadMaterials(string p)
        {
            string currentMaterial = null;
            using (var reader = new StreamReader(p))
            {
                string line;
                char[] splitChars = { ' ' };
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim(splitChars);
                    line = line.Replace("  ", " ");

                    string[] parameters = line.Split(splitChars);
                    switch (parameters[0])
                    {
                        case "newmtl":
                            currentMaterial = parameters[1];
                            break;
                        case "Kd":
                            {
                                float x = float.Parse(parameters[1], CultureInfo.InvariantCulture.NumberFormat);
                                float y = float.Parse(parameters[2], CultureInfo.InvariantCulture.NumberFormat);
                                float z = float.Parse(parameters[3], CultureInfo.InvariantCulture.NumberFormat);
                                _materials[currentMaterial] = new MyVector3(x, y, z);  
                            }
                            break;
                    }
                }
            }
        }
    }

    public class ColoredTris
    {
        public List<Tri> Tris = new List<Tri>();
        public MyVector3 Color = new MyVector3(1d, 1d, 1d);
    }

    /**
     * <summary>
     * A class containing all the necessary data for a mesh: Points, normal vectors, UV coordinates,
     * and indices into each.
     * Regardless of how the mesh file represents geometry, this is what we load it into,
     * because this is most similar to how OpenGL represents geometry.
     * We store data as arrays of vertices, UV coordinates and normals, and then a list of Triangle
     * structures.  A Triangle is a struct which contains integer offsets into the vertex/normal/texcoord
     * arrays to define a face.
     * </summary>
     */
    // XXX: Sources: http://www.opentk.com/files/ObjMeshLoader.cs, OOGL (MS3D), Icarus (Colladia)
    public class MeshData
    {
        public MyVector3[] Vertices;
        public MyVector2[] TexCoords;
        public MyVector3[] Normals;
        public List<ColoredTris> Tris;

        public MeshData(MyVector3[] vert, MyVector3[] norm, MyVector2[] tex, List<ColoredTris> tri)
        {
            Vertices = vert;
            TexCoords = tex;
            Normals = norm;
            Tris = tri;

            //Verify();
        }

        /// <summary>
        /// Returns an array containing the coordinates of all the <value>Vertices</value>.
        /// So {1,1,1, 2,2,2} will turn into {1,1,1,2,2,2}
        /// </summary>
        /// <returns>
        /// A double[]"/>
        /// </returns>
        public double[] VertexArray()
        {
            var verts = new double[Vertices.Length*3];
            for (var i = 0; i < Vertices.Length; i++)
            {
                verts[i*3] = Vertices[i].X;
                verts[i*3 + 1] = Vertices[i].Y;
                verts[i*3 + 2] = Vertices[i].Z;
            }

            return verts;
        }

        /// <summary>
        /// Returns an array containing the coordinates of the Normals, similar to VertexArray.
        /// </summary>
        /// <returns>
        /// A System.Double[]"/>
        /// </returns>
        public double[] NormalArray()
        {
            var norms = new double[Normals.Length*3];
            for (var i = 0; i < Normals.Length; i++)
            {
                norms[i*3] = Normals[i].X;
                norms[i*3 + 1] = Normals[i].Y;
                norms[i*3 + 2] = Normals[i].Z;
            }

            return norms;
        }

        /// <summary>
        /// Returns an array containing the coordinates of the TexCoords, similar to VertexArray. 
        /// </summary>
        /// <returns>
        /// A System.Double[]"/>
        /// </returns>
        public double[] TexcoordArray()
        {
            var tcs = new double[TexCoords.Length*2];
            for (var i = 0; i < TexCoords.Length; i++)
            {
                tcs[i*3] = TexCoords[i].X;
                tcs[i*3 + 1] = TexCoords[i].Y;
            }

            return tcs;
        }

        /*
        public void IndexArrays(out int[] verts, out int[] norms, out int[] texcoords) {
            List<int> v = new List<int>();
            List<int> n = new List<int>();
            List<int> t = new List<int>();
            foreach(Face f in Faces) {
                foreach(Point p in f.Points) {
                    v.Add(p.Vertex);
                    n.Add(p.Normal);
                    t.Add(p.TexCoord);
                }
            }
            verts = v.ToArray();
            norms = n.ToArray();
            texcoords = t.ToArray();
        }
        */

        public void OpenGLArrays(out Vertex[] verts, out List<ColoredTriArray> indexList)
        {
            if (Vertices.Length != Normals.Length || Vertices.Length != TexCoords.Length)
                throw new Exception("Can't handle this .obj file");

            verts = new Vertex[Vertices.Length];
            for (var i = 0; i < Vertices.Length; i++)
            {
                verts[i].Position.X = (float)Vertices[i].X;
                verts[i].Position.Y = (float)Vertices[i].Y;
                verts[i].Position.Z = (float)Vertices[i].Z;
                verts[i].Normal.X = (float)Normals[i].X;
                verts[i].Normal.Y = (float)Normals[i].Y;
                verts[i].Normal.Z = (float)Normals[i].Z;
                verts[i].TexCoord.X = (float)TexCoords[i].X;
                verts[i].TexCoord.Y = (float)TexCoords[i].Y;
            }

            indexList = new List<ColoredTriArray>();
            foreach (var t in Tris)
            {
                var indices = new ushort[t.Tris.Count * 3];
                var ptr = 0;
                for (var i = 0; i < t.Tris.Count; i++)
                {
                    indices[ptr++] = (ushort)t.Tris[i].P1.Vertex;
                    indices[ptr++] = (ushort)t.Tris[i].P2.Vertex;
                    indices[ptr++] = (ushort)t.Tris[i].P3.Vertex;
                }
                var c = new ColoredTriArray { Color = t.Color, Indices = indices };
                indexList.Add(c);
            }
        }

        public override string ToString()
        {
            var s = new StringBuilder();
            s.AppendLine("Vertices:");
            foreach (var v in Vertices)
            {
                s.AppendLine(v.ToString());
            }

            s.AppendLine("Normals:");
            foreach (var n in Normals)
            {
                s.AppendLine(n.ToString());
            }
            s.AppendLine("TexCoords:");
            foreach (var t in TexCoords)
            {
                s.AppendLine(t.ToString());
            }
            s.AppendLine("Tris:");
            foreach (var t in Tris)
            {
                s.AppendLine(t.ToString());
            }
            return s.ToString();
        }

        // XXX: Might technically be incorrect, since a (malformed) file could have vertices
        // that aren't actually in any face.
        // XXX: Don't take the names of the out parameters too literally...
        public void Dimensions(out double width, out double length, out double height)
        {
            double maxx = 0d, minx = 0d, maxy = 0d, miny = 0d, maxz = 0d, minz = 0d;
            foreach (var vert in Vertices)
            {
                if (vert.X > maxx) maxx = vert.X;
                if (vert.Y > maxy) maxy = vert.Y;
                if (vert.Z > maxz) maxz = vert.Z;
                if (vert.X < minx) minx = vert.X;
                if (vert.Y < miny) miny = vert.Y;
                if (vert.Z < minz) minz = vert.Z;
            }
            width = maxx - minx;
            length = maxy - miny;
            height = maxz - minz;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MyVertex
    { // mimic InterleavedArrayFormat.T2fN3fV3f
        public Vector2 TexCoord;
        public Vector3 Normal;
        public Vector3 Position;
    }

    public struct MyVector2
    {
        public double X;
        public double Y;

        public MyVector2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return String.Format("<{0},{1}>", X, Y);
        }
    }

    public struct MyVector3
    {
        public double X;
        public double Y;
        public double Z;

        public MyVector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public override string ToString()
        {
            return String.Format("<{0},{1},{2}>", X, Y, Z);
        }
    }

    public struct MyPoint
    {
        public int Vertex;
        public int Normal;
        public int TexCoord;

        public MyPoint(int v, int n, int t)
        {
            Vertex = v;
            Normal = n;
            TexCoord = t;
        }

        public override string ToString()
        {
            return String.Format("Point: {0},{1},{2}", Vertex, Normal, TexCoord);
        }
    }

    public class Tri
    {
        public MyPoint P1, P2, P3;

        public Tri()
        {
            P1 = new MyPoint();
            P2 = new MyPoint();
            P3 = new MyPoint();
        }

        public Tri(MyPoint a, MyPoint b, MyPoint c)
        {
            P1 = a;
            P2 = b;
            P3 = c;
        }

        public MyPoint[] Points()
        {
            return new[] { P1, P2, P3 };
        }

        public override string ToString()
        {
            return String.Format("Tri: {0}, {1}, {2}", P1, P2, P3);
        }
    }

    public class ColoredTriArray
    {
        public ushort[] Indices;
        public MyVector3 Color = new MyVector3(1d, 1d, 1d);
    }

}

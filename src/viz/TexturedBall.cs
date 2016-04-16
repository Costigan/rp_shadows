using System;
using OpenTK.Graphics.OpenGL;

namespace Shadow.viz
{
    public class TexturedBall : TexturedShape
    {
        public float Radius = 1f;
        public int XSize = 32;
        public int YSize = 16;

        public void Load()
        {
            var vertices = new Vertex[XSize*YSize];
            if (vertices.Length > 65536)
                throw new Exception("Mesh too large");
            float xf = 1f/(XSize - 1);
            float yf = 1f/(YSize - 1);
            for (int y = 0; y < YSize; y++)
            {
                double ya = (yf*y - 0.5f)*Math.PI;
                var t = (float) Math.Cos(ya);
                var pz = (float) Math.Sin(ya);
                for (int x = 0; x < XSize; x++)
                {
                    int idx = x*YSize + y;
                    vertices[idx].TexCoord.X = x*xf;
                    vertices[idx].TexCoord.Y = 1f - y*yf;

                    double xa = (x*xf*2f*Math.PI);
                    float px = t*(float) Math.Cos(xa);
                    float py = t*(float) Math.Sin(xa);

                    px = -px; // Helps get the textures aligned to the frame
                    py = -py;

                    vertices[idx].Normal.X = px;
                    vertices[idx].Normal.Y = py;
                    vertices[idx].Normal.Z = pz;

                    vertices[idx].Position.X = px*Radius;
                    vertices[idx].Position.Y = py*Radius;
                    vertices[idx].Position.Z = pz*Radius;
                }
            }

            // Define a mesh
            var elements = new ushort[(XSize - 1)*(YSize - 1)*6];
            int ptr = 0;
            int xMax = XSize - 1;
            int yMax = YSize - 1;
            for (int x = 0; x < xMax; x++)
                for (int y = 0; y < yMax; y++)
                {
                    int v = x*YSize + y;

                    //Console.WriteLine(@"v={0}", v);

                    elements[ptr++] = (ushort) (v + YSize); // a
                    elements[ptr++] = (ushort) (v + 1); // b
                    elements[ptr++] = (ushort) v; // c

                    //Console.WriteLine(@"tri [{0}, {1}, {2}]", x + YSize, v + 1, v);

                    elements[ptr++] = (ushort) (v + YSize); // a
                    elements[ptr++] = (ushort) (v + YSize + 1); // b
                    elements[ptr++] = (ushort) (v + 1); // c

                    //Console.WriteLine(@"tri [{0}, {1}, {2}]", v + YSize, v + YSize + 1, v + 1);
                }
            Buffer = LoadVBO(vertices, elements, InterleavedArrayFormat.T2fN3fV3f);

            BoundingSphereRadius = Radius;
            BoundingSphereDefined = true;
        }
    }
}
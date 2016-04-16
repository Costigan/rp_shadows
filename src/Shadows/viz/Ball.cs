using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Shadow.viz
{
    public class Ball : Shape
    {
        public float Radius = 1f;
        public int XSize = 32;
        public int YSize = 32;

        public void Load()
        {
            var vertices = new Vector3[XSize*YSize];
            for (int y = 0; y < YSize; y++)
            {
                double ay = y*Math.PI/(YSize - 1) - Math.PI/2f;
                float z = Radius*(float) Math.Sin(ay);
                float t = Radius*(float) Math.Cos(ay);
                for (int x = 0; x < XSize; x++)
                {
                    int idx = x*YSize + y;
                    double ax = x*2d*Math.PI/(XSize - 1);
                    vertices[idx].X = t*(float) Math.Sin(ax);
                    vertices[idx].Y = t*(float) Math.Cos(ax);
                    vertices[idx].Z = z;
                }
            }
            if (vertices.Length > 65536)
                throw new Exception("Mesh too large");

            // Define a mesh
            var elements = new ushort[XSize*YSize*6];
            int ptr = 0;
            int xMax = XSize - 1;
            int yMax = YSize - 1;
            for (int x = 0; x < xMax; x++)
                for (int y = 0; y < yMax; y++)
                {
                    int v = y*XSize + x;
                    elements[ptr++] = (ushort) v;
                    elements[ptr++] = (ushort) (v + 1);
                    elements[ptr++] = (ushort) (v + XSize);
                    elements[ptr++] = (ushort) (v + 1);
                    elements[ptr++] = (ushort) (v + XSize + 1);
                    elements[ptr++] = (ushort) (v + XSize);
                }
            Buffer = LoadVBO(vertices, elements, InterleavedArrayFormat.V3f);
        }
    }
}
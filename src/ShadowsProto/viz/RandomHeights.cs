using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace LadeeViz.Viz
{
    public class RandomHeights : Shape
    {
        public float Length = 1f;
        public float Width = 1f;
        public int XSize = 4;
        public int YSize = 4;

        public void Load()
        {
            var r = new Random();
            var vertices = new Vector3[XSize*YSize];
            float xf = Length/(XSize - 1);
            float yf = Width/(YSize - 1);
            for (int y = 0; y < YSize; y++)
            {
                for (int x = 0; x < XSize; x++)
                {
                    int idx = x*YSize + y;
                    vertices[idx].X = x*xf;
                    vertices[idx].Y = y*yf;
                    vertices[idx].Z = 0.5f*(float) r.NextDouble();
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
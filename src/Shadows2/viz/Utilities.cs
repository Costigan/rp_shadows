using System.Runtime.InteropServices;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Shadow.viz
{
    public struct VBO
    {
        public int EboID, NumElements;
        public int VboID;
        public InterleavedArrayFormat VertexFormat;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        // mimic InterleavedArrayFormat.T2fN3fV3f
        public Vector2 TexCoord;
        public Vector3 Normal;
        public Vector3 Position;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexNormal
    {
        // mimic InterleavedArrayFormat.N3fV3f
        public Vector3 Normal;
        public Vector3 Position;
    }
}
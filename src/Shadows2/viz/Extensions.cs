using OpenTK;

namespace Shadow.viz
{
    public static class Extensions
    {
        public static Vector3d Divide(this Vector3d a, double b)
        {
            return new Vector3d(a.X/b, a.Y/b, a.Z/b);
        }

        public static Vector3 ToFar(this Vector3d a)
        {
            return new Vector3((float) (a.X/1000d), (float) (a.Y/1000d), (float) (a.Z/1000d));
        }

        public static Vector3 ToNear(this Vector3d a)
        {
            return new Vector3((float) (a.X*1000d), (float) (a.Y*1000d), (float) (a.Z*1000d));
        }

        public static Vector3 ToFloat(this Vector3d a)
        {
            return new Vector3((float) a.X, (float) a.Y, (float) a.Z);
        }
    }
}
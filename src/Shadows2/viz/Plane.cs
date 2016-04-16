using System;
using OpenTK;

namespace Shadow.viz
{
    public struct Plane
    {
        #region Public Fields

        public float D;
        public Vector3 Normal;

        #endregion Public Fields

        #region Constructors

        public Plane(Vector4 value)
            : this(new Vector3(value.X, value.Y, value.Z), value.W)
        {
        }

        public Plane(Vector3 normal, float d)
        {
            Normal = normal;
            D = d;
        }


        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;

            Vector3 cross = Vector3.Cross(ab, ac);
            Normal = Vector3.Normalize(cross);
            D = -(Vector3.Dot(cross, a));
        }

        public Plane(float a, float b, float c, float d)
            : this(new Vector3(a, b, c), d)
        {
        }

        #endregion Constructors

        #region Public Methods

        #endregion

        #region Internal Methods

        // Indicating which side (positive/negative) of a plane a point is on.
        // Returns > 0 if on the positive side, < 0 if on the negative size, 0 if on the plane.
        internal static float ClassifyPoint(ref Vector3 point, ref Plane plane)
        {
            return point.X*plane.Normal.X + point.Y*plane.Normal.Y + point.Z*plane.Normal.Z + plane.D;
        }

        // Calculates the perpendicular distance from a point to a plane
        internal static float PerpendicularDistance(ref Vector3 point, ref Plane plane)
        {
            // dist = (ax + by + cz + d) / sqrt(a*a + b*b + c*c)
            return (float) Math.Abs((plane.Normal.X*point.X +
                                     plane.Normal.Y*point.Y +
                                     plane.Normal.Z*point.Z)/
                                    Math.Sqrt(plane.Normal.X*plane.Normal.X +
                                              plane.Normal.Y*plane.Normal.Y +
                                              plane.Normal.Z*plane.Normal.Z));
        }

        #endregion
    }
}
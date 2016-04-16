#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Authors:
Olivier Dufour (Duff)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.Text;
using OpenTK;

namespace Shadow.Viz
{
    public class BoundingFrustum : IEquatable<BoundingFrustum>
    {
        #region Private Fields

        private Matrix4 matrix;
        private Plane bottom;
        private Plane far;
        private Plane left;
        private Plane right;
        private Plane near;
        private Plane top;
        private Vector3[] corners;

        #endregion Private Fields

        #region Public Constructors

        public BoundingFrustum(Matrix4 value)
        {
            this.matrix = value;
            CreatePlanes();
            CreateCorners();
        }

        #endregion Public Constructors

        #region Public Properties

        public Plane Bottom
        {
            get { return this.bottom; }
        }

        public Plane Far
        {
            get { return this.far; }
        }

        public Plane Left
        {
            get { return this.left; }
        }

        public Matrix4 Matrix
        {
            get { return this.matrix; }
            set
            {
                this.matrix = value;
                this.CreatePlanes(); // FIXME: The odds are the planes will be used a lot more often than the matrix
                this.CreateCorners(); // is updated, so this should help performance. I hope ;)
            }
        }

        public Plane Near
        {
            get { return this.near; }
        }

        public Plane Right
        {
            get { return this.right; }
        }

        public Plane Top
        {
            get { return this.top; }
        }

        #endregion Public Properties

        #region Public Methods

        public static bool operator ==(BoundingFrustum a, BoundingFrustum b)
        {
            if (object.Equals(a, null))
                return (object.Equals(b, null));

            if (object.Equals(b, null))
                return (object.Equals(a, null));

            return a.matrix == (b.matrix);
        }


        public bool Contains(BoundingSphere sphere)
        {
            float val;

            // We first check if the sphere is inside the frustum
            var b = this.Contains(sphere.Center);
            if (b)
                return true;

            //duff idea : test if all corner is in same side of a plane if yes and outside it is disjoint else intersect
            // issue is that we can have some times when really close aabb 



            // If we're here, the the sphere's centre was outside of the frustum. This makes things hard :(
            // We can't use perpendicular distance anymore. I'm not sure how to code this.
            throw new NotImplementedException();
        }

        public bool Contains(Vector3 point)
        {
            float val;
            // If a point is on the POSITIVE side of the plane, then the point is not contained within the frustum

            // Check the top
            val = Plane.ClassifyPoint(ref point, ref top);
            if (val > 0)
                return false;

            // Check the bottom
            val = Plane.ClassifyPoint(ref point, ref bottom);
            if (val > 0)
                return false;

            // Check the left
            val = Plane.ClassifyPoint(ref point, ref left);
            if (val > 0)
                return false;

            // Check the right
            val = Plane.ClassifyPoint(ref point, ref right);
            if (val > 0)
                return false;

            // Check the near
            val = Plane.ClassifyPoint(ref point, ref near);
            if (val > 0)
                return false;

            // Check the far
            val = Plane.ClassifyPoint(ref point, ref far);
            if (val > 0)
                return false;

            // If we get here, it means that the point was on the correct side of each plane to be
            // contained. Therefore this point is contained
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(256);
            sb.Append("{Near:");
            sb.Append(this.near.ToString());
            sb.Append(" Far:");
            sb.Append(this.far.ToString());
            sb.Append(" Left:");
            sb.Append(this.left.ToString());
            sb.Append(" Right:");
            sb.Append(this.right.ToString());
            sb.Append(" Top:");
            sb.Append(this.top.ToString());
            sb.Append(" Bottom:");
            sb.Append(this.bottom.ToString());
            sb.Append("}");
            return sb.ToString();
        }

        #endregion Public Methods

        #region Private Methods

#warning Move this to the PlaneHelper class
        private void CreateCorners()
        {
            this.corners = new Vector3[8];
            this.corners[0] = IntersectionPoint(ref this.near, ref this.left, ref this.top);
            this.corners[1] = IntersectionPoint(ref this.near, ref this.right, ref this.top);
            this.corners[2] = IntersectionPoint(ref this.near, ref this.right, ref this.bottom);
            this.corners[3] = IntersectionPoint(ref this.near, ref this.left, ref this.bottom);
            this.corners[4] = IntersectionPoint(ref this.far, ref this.left, ref this.top);
            this.corners[5] = IntersectionPoint(ref this.far, ref this.right, ref this.top);
            this.corners[6] = IntersectionPoint(ref this.far, ref this.right, ref this.bottom);
            this.corners[7] = IntersectionPoint(ref this.far, ref this.left, ref this.bottom);
        }

        private void CreatePlanes()
        {
            // Pre-calculate the different planes needed
            this.left = new Plane(-this.matrix.M14 - this.matrix.M11, -this.matrix.M24 - this.matrix.M21,
                                  -this.matrix.M34 - this.matrix.M31, -this.matrix.M44 - this.matrix.M41);

            this.right = new Plane(this.matrix.M11 - this.matrix.M14, this.matrix.M21 - this.matrix.M24,
                                   this.matrix.M31 - this.matrix.M34, this.matrix.M41 - this.matrix.M44);

            this.top = new Plane(this.matrix.M12 - this.matrix.M14, this.matrix.M22 - this.matrix.M24,
                                 this.matrix.M32 - this.matrix.M34, this.matrix.M42 - this.matrix.M44);

            this.bottom = new Plane(-this.matrix.M14 - this.matrix.M12, -this.matrix.M24 - this.matrix.M22,
                                    -this.matrix.M34 - this.matrix.M32, -this.matrix.M44 - this.matrix.M42);

            this.near = new Plane(-this.matrix.M13, -this.matrix.M23, -this.matrix.M33, -this.matrix.M43);


            this.far = new Plane(this.matrix.M13 - this.matrix.M14, this.matrix.M23 - this.matrix.M24,
                                 this.matrix.M33 - this.matrix.M34, this.matrix.M43 - this.matrix.M44);

            this.NormalizePlane(ref left);
            this.NormalizePlane(ref right);
            this.NormalizePlane(ref top);
            this.NormalizePlane(ref bottom);
            this.NormalizePlane(ref near);
            this.NormalizePlane(ref far);
        }

        private static Vector3 IntersectionPoint(ref Plane a, ref Plane b, ref Plane c)
        {
            // Formula used
            //                d1 ( N2 * N3 ) + d2 ( N3 * N1 ) + d3 ( N1 * N2 )
            //P =   -------------------------------------------------------------------------
            //                             N1 . ( N2 * N3 )
            //
            // Note: N refers to the normal, d refers to the displacement. '.' means dot product. '*' means cross product

            Vector3 v1, v2, v3;
            float f = -Vector3.Dot(a.Normal, Vector3.Cross(b.Normal, c.Normal));

            v1 = (a.D*(Vector3.Cross(b.Normal, c.Normal)));
            v2 = (b.D*(Vector3.Cross(c.Normal, a.Normal)));
            v3 = (c.D*(Vector3.Cross(a.Normal, b.Normal)));

            Vector3 vec = new Vector3(v1.X + v2.X + v3.X, v1.Y + v2.Y + v3.Y, v1.Z + v2.Z + v3.Z);
            return vec/f;
        }

        private void NormalizePlane(ref Plane p)
        {
            float factor = 1f/p.Normal.Length;
            p.Normal.X *= factor;
            p.Normal.Y *= factor;
            p.Normal.Z *= factor;
            p.D *= factor;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using OpenTK;

namespace Shadow.Viz
{
    public struct BoundingSphere
    {
        #region Public Fields

        public Vector3 Center;
        public float Radius;

        #endregion Public Fields

        #region Constructors

        public BoundingSphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        #endregion Constructors

        #region Public Methods

        public static BoundingSphere CreateFromFrustum(BoundingFrustum frustum)
        {
            return CreateFromPoints(frustum.GetCorners());
        }

        public static BoundingSphere CreateFromPoints(IEnumerable<Vector3> points)
        {
            if (points == null)
                throw new ArgumentNullException("points");

            float radius = 0;
            Vector3 center = new Vector3();
            // First, we'll find the center of gravity for the point 'cloud'.
            int num_points = 0; // The number of points (there MUST be a better way to get this instead of counting the number of points one by one?)
            
            foreach (Vector3 v in points)
            {
                center += v;    // If we actually knew the number of points, we'd get better accuracy by adding v / num_points.
                ++num_points;
            }
            
            center /= (float)num_points;

            // Calculate the radius of the needed sphere (it equals the distance between the center and the point further away).
            foreach (Vector3 v in points)
            {
                float distance = ((Vector3)(v - center)).Length;
                
                if (distance > radius)
                    radius = distance;
            }


            return new BoundingSphere(center, radius);
        }

        public static BoundingSphere CreateMerged(BoundingSphere original, BoundingSphere additional)
        {
            Vector3 ocenterToaCenter = Vector3.Subtract(additional.Center, original.Center);
            float distance = ocenterToaCenter.Length;
            if (distance <= original.Radius + additional.Radius)//intersect
            {
                if (distance <= original.Radius - additional.Radius)//original contain additional
                    return original;
                if (distance <= additional.Radius - original.Radius)//additional contain original
                    return additional;
            }


            //else find center of new sphere and radius
            float leftRadius = System.Math.Max(original.Radius - distance, additional.Radius);
            float Rightradius = System.Math.Max(original.Radius + distance, additional.Radius);
            ocenterToaCenter = ocenterToaCenter + (((leftRadius - Rightradius) / (2 * ocenterToaCenter.Length)) * ocenterToaCenter);//oCenterToResultCenter
            
            BoundingSphere result = new BoundingSphere();
            result.Center = original.Center + ocenterToaCenter;
            result.Radius = (leftRadius + Rightradius) / 2;
            return result;
        }

        public static void CreateMerged(ref BoundingSphere original, ref BoundingSphere additional, out BoundingSphere result)
        {
            result = BoundingSphere.CreateMerged(original, additional);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{{Center:{0} Radius:{1}}}", this.Center.ToString(), this.Radius.ToString());
        }

        #endregion Public Methods
    }
}
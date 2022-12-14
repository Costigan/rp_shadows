using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shadows.math
{
    public static class Extensions
    {
        public static float Distance(this PointF a, PointF b)
        {
            var dx = a.X - b.X;
            var dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static bool Equivalent(this PointF a, PointF b, float epsilon = 0.01f)
        {
            return a.Distance(b) < epsilon;
        }

        public static PointF Plus(this PointF a, PointF b)
        {
            return new PointF(a.X + b.X, a.Y + b.Y);
        }
    }
}

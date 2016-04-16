using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace Shadow.viz
{
    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Target;

        public bool Intersects(BoundingBox box)
        {
            return false;
        }
    }
}

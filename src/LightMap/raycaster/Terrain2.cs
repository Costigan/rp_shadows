using LightMap.math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace LightMap.raycaster
{
    public partial class Terrain
    {
        #region Version 4

        protected uint[,] _hitbuf;
        public uint[,] Hitbuf => _hitbuf ?? (_hitbuf = new uint[Width, Height]);

        const uint HitIncrement =  0x00010001;
        const uint MissIncrement = 0x00000001;

        public void UpdateToSunV4(Vector3d sun1,
            float sunFraction = 1f,
            float sunHalfAngle = (float)(0.25d * Math.PI / 180d),
            float rayDensity = 1f,
            int sunRayVerticalCount = 0,
            int sunRayHorizontalCount = 0)
        {
            var gridResolution = GridResolution;
            var invGridResolution = 1f / gridResolution;
            var step = gridResolution / rayDensity;
            var gridCellArea = step * step;
            var sunPosVec = new Vector3((float)sun1.X, (float)sun1.Y, (float)sun1.Z);

            var ixmax = HeightMap.GetLength(0);
            var iymax = HeightMap.GetLength(1);

            var origin = new Vector3(0f, 0f, 0f);
            var zAxis = new Vector3(0f, 0f, 1f);

            var bounds = new RectangleF(MinPX, MinPY, MaxPX - MinPX, MaxPY - MinPY);

            var starts = CalculateStartsV4(sunPosVec, step, bounds);
            if (false)
                starts = new List<Vector2>() { starts[10] };

            if (false)
                foreach (var s in starts)
                    Console.WriteLine(s);

            var rays = CalculateSunRaysV4(sunPosVec, sunRayVerticalCount, sunRayHorizontalCount);
            var raysCount = rays.Count;
            var totalRayCount = starts.Count * raysCount;
            Console.WriteLine(@"totalRayCount={0}", totalRayCount);

            var clipper = new LightingTriangleClipper();
            clipper.Width = Width;
            clipper.Height = Height;

            var lightPerHit = 1f / (rayDensity * rayDensity * (1 + sunRayVerticalCount * sunRayVerticalCount));

            var ignore = Hitbuf[0,0];
            Array.Clear(Hitbuf, 0, Hitbuf.Length);

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Enumerable.Range(0, totalRayCount).AsParallel().ForAll(index =>
            {
                var startIndex = index / raysCount;
                var rayIndex = index - startIndex * raysCount;
                var p = starts[startIndex];
                var ray = rays[rayIndex];
                var toSun = ray.ToSun;
                var walkVec = toSun.Xy.Normalized() * -step;

                var cross = Vector3.Cross(toSun, zAxis);
                var up = Vector3.Cross(-toSun, cross);
                var m = Matrix4.LookAt(toSun, origin, up);

                var pz = MinPZ;
                var lastZTransformed = p.X * m.Row0.Y + p.Y * m.Row1.Y + pz * m.Row2.Y + 1f * m.Row3.Y;
                var ix = (int)((p.X - MinPX) * invGridResolution);
                var iy = (int)((p.Y - MinPY) * invGridResolution);

                while (ix >= 0 && ix < ixmax && iy >= 0 && iy < iymax)
                {
                    pz = InterpolatedHeightMapV4(p.X, p.Y, gridResolution, clipper.Width, clipper.Height);
                    var pzTransformed = p.X * m.Row0.Y + p.Y * m.Row1.Y + pz * m.Row2.Y + 1f * m.Row3.Y;

                    // Debugging
                    //Console.Write(@"[{0},{1}]", ix, iy);

                    if (pzTransformed >= lastZTransformed) // not sure how to handle equality here
                    {
                        _hitbuf[ix,iy] += HitIncrement;
                        lastZTransformed = pzTransformed;
                    } else
                    {
                        _hitbuf[ix, iy] += MissIncrement;
                    }

                    p += walkVec;
                    ix = (int)((p.X - MinPX) * invGridResolution);
                    iy = (int)((p.Y - MinPY) * invGridResolution);
                }
            });

            stopwatch.Stop();
            Console.WriteLine(@"Elapsed time={0}  time_per_ray={1} usec", stopwatch.Elapsed, (double)(1000L * stopwatch.ElapsedMilliseconds) / totalRayCount);

            var ignore2 = ShadowBuf;
            // Fill shadow buf
            for (var ix = 0; ix < _hitbuf.GetLength(0); ix++)
                for (var iy = 0; iy < Hitbuf.GetLength(1);iy++)
                {
                    var h = _hitbuf[ix, iy];
                    var hits = h >> 16;
                    var total = h & 0xFFFF;
                    _shadowBuf[ix, iy] = hits / (float)total;
                }
        }

        public List<Vector2> CalculateStartsV4(Vector3 sunPosVec, float stepSize, RectangleF bounds)
        {
            var flat = sunPosVec.Xy;
            flat.Normalize();
            var sunCorner = GetCornerV4(flat);

            var halfStep = stepSize / 2f;
            var stepVec = flat * -stepSize;

            var starts = new List<Vector2>();
            Vector2 start, edgeStep1, edgeStep2;

            ConfigureStepVectors(sunCorner, bounds, stepVec, out start, out edgeStep1, out edgeStep2);

            var p = start;
            while (bounds.Contains(p.X, p.Y))
            {
                starts.Add(p);
                p += edgeStep1;
            }
            p = start;
            while (bounds.Contains(p.X, p.Y))
            {
                starts.Add(p);
                p += edgeStep2;
            }
            starts.RemoveAt(0);  // remove the corner, which was added twice
            return starts;
        }

        private void ConfigureStepVectors(CornerId sunCorner, RectangleF bounds, Vector2 stepVec,
            out Vector2 start, out Vector2 edgeStep1, out Vector2 edgeStep2)
        {
            var stepSize = stepVec.Length;
            var halfStep = stepSize / 2f;
            var stepDirection = stepVec.Normalized();

            start =
                sunCorner == CornerId.NN ? new Vector2(bounds.Left + halfStep, bounds.Top + halfStep) :
                sunCorner == CornerId.NP ? new Vector2(bounds.Left + halfStep, bounds.Bottom - halfStep) :
                sunCorner == CornerId.PN ? new Vector2(bounds.Right - halfStep, bounds.Top + halfStep) :
                new Vector2(bounds.Right - halfStep, bounds.Bottom - halfStep);

            switch (sunCorner)
            {
                case CornerId.NN:
                    edgeStep1 = new Vector2(stepVec.Y == 0f ? float.MaxValue : stepSize / stepDirection.Y, 0f);
                    edgeStep2 = new Vector2(0f, stepVec.X == 0f ? float.MaxValue : stepSize / stepDirection.X);
                    break;
                case CornerId.NP:
                    edgeStep1 = new Vector2(stepVec.Y == 0f ? float.MaxValue : -stepSize / stepDirection.Y, 0f);
                    edgeStep2 = new Vector2(0f, stepVec.X == 0f ? float.MinValue : -stepSize / stepDirection.X);
                    break;
                case CornerId.PN:
                    edgeStep1 = new Vector2(stepVec.Y == 0f ? float.MinValue : -stepSize / stepDirection.Y, 0f);
                    edgeStep2 = new Vector2(0f, stepVec.X == 0f ? float.MaxValue : -stepSize / stepDirection.X);
                    break;
                case CornerId.PP:
                default:
                    edgeStep1 = new Vector2(stepVec.Y == 0f ? float.MinValue : stepSize / stepDirection.Y, 0f);
                    edgeStep2 = new Vector2(0f, stepVec.X == 0f ? float.MinValue : stepSize / stepDirection.X);
                    break;
            }
        }

        /// <summary>
        /// This version doesn't use the same steps horizontally as vertically
        /// </summary>
        /// <param name="toSun"></param>
        /// <param name="horizontalCount"></param>
        /// <returns></returns>
        public List<SunRay> CalculateSunRaysV4(Vector3 toSun, int verticalCount, int horizontalCount = -1)
        {
            const float sunRadiusDeg = 0.25f;
            float sunRadiusRad = DegToRad(sunRadiusDeg);

            if (verticalCount < 0)
                verticalCount = horizontalCount;

            var toSun4 = new Vector4(toSun);

            var zAxis = new Vector3(0f, 0f, 1f);
            var sunInPlane = new Vector3(toSun.Xy);
            var rotateSunInPlane = Matrix4.CreateFromAxisAngle(zAxis, DegToRad(90f));
            var upDownAxis4 = Vector4.Transform(new Vector4(toSun.Xy), rotateSunInPlane);
            var upDownAxis = new Vector3(upDownAxis4);

            var vSteps = 2 * horizontalCount + 1;
            var vStepf = 2f / vSteps;
            var hSteps = 2 * verticalCount + 1;
            var hStepf = 2f / hSteps;
            var origin = new PointF(0f, 0f);
            var rays = new List<SunRay>();
            for (var i = -horizontalCount; i <= horizontalCount; i++)  // horizontal 
                for (var j = -verticalCount; j <= verticalCount; j++)  // vertical
                {
                    var p = new PointF(i * vStepf, j * hStepf);
                    var nearCorner = new PointF(vStepf * Math.Max(0f, Math.Abs(i) - 0.5f), Math.Max(0f, hStepf * Math.Abs(j) - 0.5f));
                    var nearDistance = origin.Distance(nearCorner);
                    if (nearDistance >= 1f)
                        continue;

                    // Calculate the ray by rotating the initial ray
                    var farCorner = new PointF(vStepf * Math.Abs(i) + 0.5f, hStepf * Math.Abs(j) + 0.5f);
                    Matrix4 rotate1;
                    Matrix4.CreateFromAxisAngle(zAxis, i * vStepf * sunRadiusRad, out rotate1);
                    Matrix4 rotate2;
                    Matrix4.CreateFromAxisAngle(upDownAxis, j * hStepf * sunRadiusRad, out rotate2);
                    Matrix4 product;
                    Matrix4.Mult(ref rotate1, ref rotate2, out product);  // Not sure about the order here
                    var transformedToSun = Vector4.Transform(toSun4, product);

                    // How much of the square is covered
                    var farDistance = origin.Distance(farCorner);
                    if (farDistance <= 1f)
                    {
                        var v = new Vector3(transformedToSun);
                        v.Normalize();
                        rays.Add(new SunRay { ToSun = v, SunFraction = 1f });  // Will normalize later
                    }
                    else
                    {
                        var fraction = (1f - nearDistance) / (farDistance - nearDistance);
                        if (fraction > 0.25f)  // Just clip if the ray is too far outside the sun
                        {
                            var v = new Vector3(transformedToSun);
                            v.Normalize();
                            rays.Add(new SunRay { ToSun = v, SunFraction = fraction });
                        }
                    }
                }

            var sum = rays.Sum(r => r.SunFraction);
            var normalizedRays = rays.Select(r => new SunRay { ToSun = r.ToSun, SunFraction = r.SunFraction / sum }).ToList();
            return normalizedRays;
        }

        public CornerId GetCornerV4(Vector2 v)
        {
            if (v.X > 0f)
                return (v.Y > 0f) ? CornerId.PP : CornerId.PN;
            return (v.Y > 0f) ? CornerId.NP : CornerId.NN;
        }


        float InterpolatedHeightMapV4(float xTerrain, float yTerrain, float frameStep, int xmax, int ymax)
        {
            // xTerrain,yTerrain is in units of meters.
            // x,y is in units of grid cells
            var x = (xTerrain - MinPX) / frameStep;
            var y = (yTerrain - MinPX) / frameStep;
            var x1 = (int)x;
            var y1 = (int)y;
            var x2 = x1 + 1;
            var y2 = y1 + 1;

            if (x1 < 0 || y1 < 0 || x2 >= xmax || y2 >= ymax)  // Do nothing for now
                return float.MinValue;

            var f11 = HeightMap[x1, y1];
            var f12 = HeightMap[x1, y2];
            var f21 = HeightMap[x2, y1];
            var f22 = HeightMap[x2, y2];

            var sz = (f11 * (x2 - x) * (y2 - y) + f21 * (x - x1) * (y2 - y) + f12 * (x2 - x) * (y - y1) + f22 * (x - x1) * (y - y1));
            return sz;
        }

        public Vector2 Copy(Vector2 f)
        {
            return new Vector2(f.X, f.Y);
        }

        #endregion Version 4
    }
}

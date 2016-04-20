using Shadow.viz;
using Shadows.math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace Shadow.terrain
{
    public class Terrain
    {
        public float[,] HeightMap;
        public BoundingBox Box;

        public float MinPX, MinPY;   // Min and max in ME frame of the terrain (pz is inverted)
        public float MaxPX, MaxPY;
        public float MaxPZ, MinPZ;

        public bool SingleRay;
        protected float[,] _shadowBuf;
        public float RayFraction = 0.5f;

        public float[,] ShadowBuf => _shadowBuf ?? (_shadowBuf = new float[Width, Height]);
        public int Width => HeightMap != null ? HeightMap.GetLength(0) : 0;
        public int Height => HeightMap != null ? HeightMap.GetLength(1) : 0;
        public float GridResolution => (MaxPX - MinPX) / Width;

        bool debugFlag = false;

        public enum CornerId
        {
            PP,
            PN,
            NN,
            NP
        };

        public CornerId GetCorner(Vector3 v)
        {
            if (v.X > 0f)
                return (v.Y > 0f) ? CornerId.PN : CornerId.PP;
            return (v.Y > 0f) ? CornerId.NN : CornerId.NP;
        }

        public Vector3 Copy(Vector3 f)
        {
            return new Vector3(f.X, f.Y, 0f);
        }

        public void Clear()
        {
            Array.Clear(ShadowBuf, 0, ShadowBuf.Length);
        }

        /// <summary>
        /// This version uses bilinear interpolation
        /// </summary>
        /// <param name="sun1"></param>
        public void UpdateToSunV3(Vector3d sun1, float sunFraction = 1f, float sunHalfAngle = (float)(0.25d * Math.PI / 180d))
        {
            var gridResolution = GridResolution;
            var step = 1f * gridResolution; // was 0.75 of the resolution of the grid
            var gridCellArea = step * step;
            var sunPosVec = new Vector3((float)sun1.X, (float)sun1.Y, (float)sun1.Z);

            var ixmax = HeightMap.GetLength(0);
            var iymax = HeightMap.GetLength(1);

            var origin = new Vector3(0f, 0f, 0f);
            var zAxis = new Vector3(0f, 0f, 1f);

            var starts = CalculateStartsV3(sunPosVec, gridResolution);
            //if (SingleRay)
            //    starts = new List<Vector3> { starts[(int)((starts.Count - 1) * RayFraction)] };
            if (false)
                starts = new List<Vector3>() { starts[10] };

            var sideCount = 9;
            var rays = CalculateSunRays(sunPosVec, sideCount);
            var raysCount = rays.Count;
            var totalRayCount = starts.Count * raysCount;

            Enumerable.Range(0, totalRayCount).AsParallel().ForAll(index =>
            {
                var startIndex = index / raysCount;
                var rayIndex = index - startIndex * raysCount;
                var s = starts[startIndex];
                var ray = rays[rayIndex];
                var toSun = ray.ToSun;

                var cross = Vector3.Cross(toSun, zAxis);
                var up = Vector3.Cross(-toSun, cross);
                var m = Matrix4.LookAt(toSun, origin, up);

                var walkVec = -toSun;
                walkVec.Z = 0f;
                walkVec = walkVec.Normalized() * step;

                // Calculate offsets to get the corners of the lit square
                var toForward = walkVec / 2f;
                var toRight = new Vector3(toForward.Y, -toForward.X, 0f);
                var toP1 = toForward + toRight;
                var toP2 = -toForward + toRight;
                var toP3 = -toForward - toRight;
                var toP4 = toForward - toRight;

                var resolution = gridResolution;
                var clipper = new LightingTriangleClipper();
                clipper.GridResolution = resolution;
                clipper.toP1 = new PointF(toP1.X, toP1.Y);
                clipper.toP2 = new PointF(toP2.X, toP2.Y);
                clipper.toP3 = new PointF(toP3.X, toP3.Y);
                clipper.toP4 = new PointF(toP4.X, toP4.Y);
                clipper.Width = Width;
                clipper.Height = Height;
                clipper.ShadowArray = _shadowBuf;
                clipper.InvertedGridCellArea = 1f / gridCellArea;

                var max = ixmax + iymax;
                var six = (int)Math.Round((s.X - MinPX) / resolution);
                var siy = (int)Math.Round((s.Y - MinPY) / resolution);
                if (six < 0 || six >= ixmax || siy < 0 || siy >= iymax)
                    throw new Exception(@"start is out of bounds");
                var startX = s.X;
                var startY = s.Y;
                var pz = HeightMap[six, siy];  // not interpolated
                var lastZTransformed = startX * m.Row0.Y + startY * m.Row1.Y + pz * m.Row2.Y + 1f * m.Row3.Y;
                for (var i = 1; i < max; i++)
                {
                    var x = startX + walkVec.X * i;
                    var y = startY + walkVec.Y * i;
                    var z = InterpolatedHeightMap(x, y, resolution, clipper.Width, clipper.Height);
                    var nextZTransformed = x * m.Row0.Y + y * m.Row1.Y + z * m.Row2.Y + 1f * m.Row3.Y;

                    //if (z > 0f)
                    //    Console.WriteLine("here");

                    if (nextZTransformed >= lastZTransformed)
                    //if (true)
                    {
                        // not sure how to handle equality here
                        DistributeClippedLight(x, y, 1f, clipper);
                        lastZTransformed = nextZTransformed;
                    }
                }
            });
        }

        float InterpolatedHeightMap(float x, float y, float frameStep, int xmax, int ymax)
        {
            var x1 = (int)((x - MinPX) / frameStep);
            var y1 = (int)((y - MinPY) / frameStep);
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

        float FakeInterpolatedHeightMap(float x, float y, float frameStep, int xmax, int ymax)
        {
            var x1 = (int)Math.Round((x - MinPX) / frameStep);
            var y1 = (int)Math.Round((y - MinPY) / frameStep);
            if (x1 < 0 || y1 < 0 || x1 >= xmax || y1 >= ymax)  // Do nothing for now
                return float.MinValue;
            return HeightMap[x1, y1];
        }

        void FakeDistributeLight(float x, float y, float sunFraction, LightingTriangleClipper clipper)
        {
            var x1 = (int)Math.Round((x - MinPX) / clipper.GridResolution);
            var y1 = (int)Math.Round((y - MinPY) / clipper.GridResolution);
            if (x1 < 0 || y1 < 0 || x1 >= clipper.Width || y1 >= clipper.Height)  // Do nothing for now
                return;

            // This looks pretty good, but it's not right.
            //_shadowBuf[x1, y1] = light;

            _shadowBuf[x1, y1] += sunFraction;
        }

        void DistributeClippedLight(float x, float y, float sunFraction, LightingTriangleClipper clipper)
        {
            // x1, y1 is the x,y corner of the square that contains the center of the lit square. It's
            // the primary grid cell that will be lit.
            var x1 = (int)Math.Floor((x - MinPX) / clipper.GridResolution);
            var y1 = (int)Math.Floor((y - MinPY) / clipper.GridResolution);

            //Console.WriteLine(@"x1={0} y1={1}   x={2} y={3}", x1, y1, x, y);

            var p = new PointF(x, y);
            var triangle1 = new Triangle { A = clipper.toP1.Plus(p), B = clipper.toP2.Plus(p), C = clipper.toP3.Plus(p) };
            var triangle2 = new Triangle { A = clipper.toP1.Plus(p), B = clipper.toP3.Plus(p), C = clipper.toP4.Plus(p) };
            var rect = new RectangleF { Width = clipper.GridResolution, Height = clipper.GridResolution };
            var halfSun = sunFraction;  // / 2f;

            // debugging
            for (var xoffset = -1; xoffset < 2; xoffset++)
                for (var yoffset = -1; yoffset < 2; yoffset++)
                //var xoffset = 0;
                //var yoffset = 0;
                {
                    var ix = x1 + xoffset;
                    var iy = y1 + yoffset;

                    if (ix >= 0 && ix < clipper.Width && iy >= 0 && iy < clipper.Height)
                    {
                        rect.X = ix * clipper.GridResolution;
                        rect.Y = iy * clipper.GridResolution;

                        if (ix == 100 && iy == 100)
                        {
                            Console.WriteLine(@"Clipping to rect {0}", rect);
                        }

                        clipper.AddLight(triangle1, rect, ix, iy, halfSun * clipper.InvertedGridCellArea);
                        clipper.AddLight(triangle2, rect, ix, iy, halfSun * clipper.InvertedGridCellArea);
                    }
                }
        }

        private List<Vector3> CalculateStartsV3(Vector3 sunPosVec, float resolution)
        {
            var sunCorner = GetCorner(sunPosVec);
            var flat = new Vector2(sunPosVec.X, sunPosVec.Y);
            flat.Normalize();

            var gridResolution = GridResolution;
            var halfResolution = gridResolution / 2f;
            var xmin = MinPX;
            var xmax = xmin + (HeightMap.GetLength(0) - 1) * gridResolution;
            var ymin = MinPY;
            var ymax = ymin + (HeightMap.GetLength(1) - 1) * gridResolution;

            //step = 500f*step; // this is for testing only
            //SetCorners();
            var starts = new List<Vector3>();
            float step;
            switch (sunCorner)
            {
                case CornerId.PP:
                    {
                        //var p = Copy(CornerNP);
                        var p = new Vector3(xmin + halfResolution, ymin + halfResolution, 0f);
                        step = flat.Y == 0f ? float.MaxValue : Math.Abs(gridResolution / flat.Y);
                        while (p.X < xmax)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.X += step;
                        }

                        //p = Copy(CornerPP);
                        p = new Vector3(xmax - halfResolution, ymin + halfResolution, 0f);
                        step = flat.X == 0f ? float.MaxValue : Math.Abs(gridResolution / flat.X);
                        while (p.Y < ymax)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.Y += step;
                        }
                    }
                    break;
                case CornerId.PN:
                    {
                        //var p = Copy(CornerNN);
                        var p = new Vector3(xmin + halfResolution, ymax - halfResolution, 0f);
                        step = flat.Y == 0f ? float.MaxValue : Math.Abs(gridResolution / flat.Y);
                        while (p.X < xmax)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.X += step;
                        }

                        step = flat.X == 0f ? float.MaxValue : Math.Abs(gridResolution / flat.X);
                        //p = Copy(CornerPN);
                        p = new Vector3(xmax - halfResolution, ymax - halfResolution, 0f);
                        while (p.Y > ymin)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.Y -= step;
                        }
                    }
                    break;
                case CornerId.NN:
                    {
                        //var p = Copy(CornerNP);
                        var p = new Vector3(xmin + halfResolution, ymin + halfResolution, 0f);
                        step = flat.X == 0f ? float.MaxValue : Math.Abs(gridResolution / flat.X);
                        while (p.Y < ymax)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.Y += step;
                        }

                        step = flat.Y == 0f ? float.MaxValue : Math.Abs(gridResolution / flat.Y);
                        //p = Copy(CornerNN);
                        p = new Vector3(xmin + halfResolution, ymax - halfResolution, 0f);
                        while (p.X < xmax)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.X += step;
                        }
                    }
                    break;
                case CornerId.NP:
                    {
                        //var p = Copy(CornerNN);
                        var p = new Vector3(xmin + halfResolution, ymax - halfResolution, 0f);
                        step = flat.X == 0f ? float.MaxValue : Math.Abs(gridResolution / flat.X);
                        while (p.Y > ymin)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.Y -= step;
                        }
                        //p = Copy(CornerNP);
                        step = flat.Y == 0f ? float.MaxValue : Math.Abs(gridResolution / flat.Y);
                        p = new Vector3(xmin + halfResolution, ymin + halfResolution, 0f);
                        while (p.X < xmax)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.X += step;
                        }
                    }
                    break;
            }
            return starts;
        }

        public Bitmap HeightFieldToBitmap(Bitmap input)
        {
            var width = Width;
            var height = Height;
            Bitmap result = input;
            if (input == null || input.Width != width || input.Height != height)
                result = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            var palette = result.Palette;
            for (var i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            result.Palette = palette;
            var bitmapData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            unsafe
            {
                var range = MaxPZ - MinPZ;
                byte* ptr = (byte*)bitmapData.Scan0;
                for (var i = 0; i < height; i++)
                    for (var j = 0; j < width; j++)
                    {
                        var v1 = HeightMap[i, j];
                        var v2 = (byte)(255f * (v1 - MinPZ) / range);
                        *ptr++ = v2;
                    }
            }
            result.UnlockBits(bitmapData);
            return result;
        }

        public unsafe Bitmap ShadowBufferToScaledImageV4(Bitmap input)
        {
            var width = Width;
            var height = Height;
            Bitmap result = input;
            if (input == null || input.Width != width || input.Height != height)
                result = new Bitmap(width, height, PixelFormat.Format8bppIndexed);
            var palette = result.Palette;
            for (var i = 0; i < 256; i++)
                palette.Entries[i] = Color.FromArgb(i, i, i);
            result.Palette = palette;

            float maxv = float.MinValue;
            for (var i = 0; i < height; i++)
                for (var j = 0; j < width; j++)
                {
                    var v = _shadowBuf[i, j];
                    if (v > maxv) maxv = v;
                }
            Console.WriteLine(@"maxv={0}", maxv);

            const byte white = 255;
            var bitmapData = result.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
            unsafe
            {
                var range = MaxPZ - MinPZ;
                byte* ptr = (byte*)bitmapData.Scan0;
                for (var i = 0; i < height; i++)
                {
                    var iflipped = height - 1 - i;
                    for (var j = 0; j < width; j++)
                    {
                        var lightFraction = _shadowBuf[iflipped, j];
                        var val = (byte)(255 * lightFraction / maxv);
                        *(ptr++) = val;
                    }
                }
            }
            result.UnlockBits(bitmapData);
            return result;
        }

        /// <summary>
        /// Calculate multiple vectors toward the sun.  Each one represents a fraction of the sun's visible surface.
        /// </summary>
        /// <param name="toSun"></param>
        /// <param name="radialCount"></param>
        /// <returns></returns>
        public List<SunRay> CalculateSunRays(Vector3 toSun, int radialCount)
        {
            const float sunRadiusDeg = 0.25f;
            float sunRadiusRad = DegToRad(sunRadiusDeg);

            var toSun4 = new Vector4(toSun);

            var zAxis = new Vector3(0f, 0f, 1f);
            var sunInPlane = new Vector3(toSun.Xy);
            var rotateSunInPlane = Matrix4.CreateFromAxisAngle(zAxis, DegToRad(90f));
            var upDownAxis4 = Vector4.Transform(new Vector4(toSun.Xy), rotateSunInPlane);
            var upDownAxis = new Vector3(upDownAxis4);

            var steps = 2 * radialCount + 1;
            var stepf = 2f / steps;
            var origin = new PointF(0f, 0f);
            var rays = new List<SunRay>();
            for (var i = -radialCount; i <= radialCount; i++)
                for (var j = -radialCount; j <= radialCount; j++)
                {
                    var p = new PointF(i * stepf, j * stepf);
                    var nearCorner = new PointF(stepf * Math.Max(0f, Math.Abs(i) - 0.5f), Math.Max(0f, stepf * Math.Abs(j) - 0.5f));
                    var nearDistance = origin.Distance(nearCorner);
                    if (nearDistance >= 1f)
                        continue;

                    // Calculate the ray by rotating the initial ray
                    var farCorner = new PointF(stepf * Math.Abs(i) + 0.5f, stepf * Math.Abs(j) + 0.5f);
                    Matrix4 rotate1;
                    Matrix4.CreateFromAxisAngle(zAxis, i * stepf * sunRadiusRad, out rotate1);
                    Matrix4 rotate2;
                    Matrix4.CreateFromAxisAngle(upDownAxis, j * stepf * sunRadiusRad, out rotate2);
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

        float DegToRad(float deg)
        {
            return deg * 3.141592653f / 180f;
        }

        #region Version 4

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

            var rays = CalculateSunRaysV4(sunPosVec, sunRayVerticalCount, sunRayHorizontalCount);
            var raysCount = rays.Count;
            var totalRayCount = starts.Count * raysCount;
            Console.WriteLine(@"totalRayCount={0}", totalRayCount);

            var clipper = new LightingTriangleClipper();
            clipper.Width = Width;
            clipper.Height = Height;

            var lightPerHit = 1f / (rayDensity * rayDensity * (1 + sunRayVerticalCount * sunRayVerticalCount));

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

                while (ix >= 0 && ix < ixmax && iy>=0 && iy< iymax)
                {
                    pz = InterpolatedHeightMap(p.X, p.Y, gridResolution, clipper.Width, clipper.Height);
                    var pzTransformed = p.X * m.Row0.Y + p.Y * m.Row1.Y + pz * m.Row2.Y + 1f * m.Row3.Y;
                    if (pzTransformed >= lastZTransformed)
                    {
                        // not sure how to handle equality here
                        _shadowBuf[ix, iy] += lightPerHit;
                        lastZTransformed = pzTransformed;
                    }
                    p += walkVec;
                    ix = (int)((p.X - MinPX) * invGridResolution);
                    iy = (int)((p.Y - MinPY) * invGridResolution);
                }
            });

            stopwatch.Stop();
            Console.WriteLine(@"Elapsed time={0}  time_per_ray={1} usec", stopwatch.Elapsed, (double)(1000L * stopwatch.ElapsedMilliseconds) / totalRayCount);
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

        public Vector2 Copy(Vector2 f)
        {
            return new Vector2(f.X, f.Y);
        }

        #endregion Version 4
    }

    public struct SunRay
    {
        public Vector3 ToSun;
        public float SunFraction;
    }
}

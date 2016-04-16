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

        /// <summary>
        /// This version uses bilinear interpolation
        /// </summary>
        /// <param name="sun1"></param>
        public void UpdateToSunV3(Vector3d sun1)
        {
            var gridResolution = GridResolution;
            var step = 0.75f * gridResolution; // less than the resolution of the grid
            var sunPosVec = new Vector3((float)sun1.X, (float)sun1.Y, (float)sun1.Z);
            var sunCorner = GetCorner(sunPosVec);

            var ixmax = HeightMap.GetLength(0);
            var iymax = HeightMap.GetLength(1);

            var origin = new Vector3(0f, 0f, 0f);
            var zAxis = new Vector3(0f, 0f, 1f);
            var cross = Vector3.Cross(sunPosVec, zAxis);
            var up = Vector3.Cross(-sunPosVec, cross);
            var m = Matrix4.LookAt(sunPosVec, origin, up);

            var sunXY = Math.Sqrt(sunPosVec.X * sunPosVec.X + sunPosVec.Y * sunPosVec.Y);
            var sunAngle = Math.Atan2(sunPosVec.Z, sunXY);
            const float sunHalfAngle = (float)(0.25d * Math.PI / 180d);
            var highSlope = (float)Math.Tan(sunAngle + sunHalfAngle);
            var lowSlope = (float)Math.Tan(sunAngle - sunHalfAngle);
            var deltaSlope = highSlope - lowSlope;

            var walkVec = -sunPosVec;
            walkVec.Z = 0f;
            walkVec = walkVec.Normalized() * step;
            //Console.WriteLine(@"walkVec={0}", walkVec);
            var starts = CalculateStartsV2(sunCorner, step);
            if (SingleRay)
                starts = new List<Vector3> { starts[(int)((starts.Count - 1) * RayFraction)] };

            Array.Clear(ShadowBuf, 0, ShadowBuf.Length);
            starts.AsParallel().ForAll(s => //starts.ForEach(s =>
            {
                var xmax = Width;  // Width of heightmap
                var ymax = Height;
                var frameStep = gridResolution;
                var max = ixmax + iymax;
                var six = (int)Math.Round((s.X - MinPX) / frameStep);
                var siy = (int)Math.Round((s.Y - MinPY) / frameStep);
                if (six < 0 || six >= ixmax || siy < 0 || siy >= iymax)
                    throw new Exception(@"start is out of bounds");
                var px = s.X;
                var py = s.Y;
                var pz = HeightMap[six, siy];  // not interpolated
                var pIndex = 0;
                var pzTransformed = px * m.Row0.Y + py * m.Row1.Y + pz * m.Row2.Y + 1f * m.Row3.Y;
                int ix1 = -1, iy1 = -1;
                for (var i = 1; i < max; i++)
                {
                    var x = px + walkVec.X * i;
                    var y = py + walkVec.Y * i;

                    var sz = FakeInterpolatedHeightMap(x, y, frameStep, xmax, ymax);

                    var slope = (pz - sz) / (step * (i - pIndex));
                    if (slope > highSlope)
                    { }  // In shadow.  No ligt to distribute
                    else if (slope < lowSlope)
                        FakeDistributeLight(x, y, 1f, frameStep, xmax, ymax);
                    else
                    {
                        var f = (slope - lowSlope) / deltaSlope;
                        var f1 = 1f - f;
                        FakeDistributeLight(x, y, f1, frameStep, xmax, ymax);
                    }
                    var szTransformed = x * m.Row0.Y + y * m.Row1.Y + sz * m.Row2.Y + 1f * m.Row3.Y;
                    if (!(szTransformed > pzTransformed))
                    {
                        //_shadowBuf[ix, iy] = 0;
                    }
                    else
                    {
                        //_shadowBuf[ix, iy] = 9;
                        pzTransformed = szTransformed;
                        pIndex = i;
                        pz = sz;
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
            if (x1 < 0 || y1 < 0 || x1 >= xmax || x1 >= ymax)  // Do nothing for now
                return float.MinValue;
            return HeightMap[x1, y1];
        }

        void DistributeLight(float x, float y, float light, float frameStep, int xmax, int ymax)
        {
            var x1 = (int)((x - MinPX) / frameStep);
            var y1 = (int)((y - MinPY) / frameStep);
            var x2 = x1 + 1;
            var y2 = y1 + 1;
            var x3 = x1 - 1;
            var y3 = y1 - 1;

            if (x3 >= 0)
            {
                if (y3 >= 0)
                    DistributeLight(x, y, x3, y3);
                DistributeLight(x, y, x3, y1);
                if (y2 < ymax)
                    DistributeLight(x, y, x3, y2);
            }
            if (y3 >= 0)
                DistributeLight(x, y, x1, y3);
            DistributeLight(x, y, x1, y1);
            if (y2 < ymax)
                DistributeLight(x, y, x1, y2);
            if (x2 < xmax)
            {
                if (y3 >= 0)
                    DistributeLight(x, y, x2, y3);
                DistributeLight(x, y, x2, y1);
                if (y2 < ymax)
                    DistributeLight(x, y, x2, y2);
            }
        }

        void DistributeLight(float x, float y, int gridx, int gridy)
        {
            var dx = x - gridx;
            if (dx < 0f) dx = -dx;
            if (dx > 0.5f) return;
            var dy = y - gridy;
            if (dy < 0f) dy = -dy;
            if (dy > 0.5f) return;
            _shadowBuf[gridx, gridy] += (1f - dx) * (1f - dy);
        }

        void FakeDistributeLight(float x, float y, float light, float frameStep, int xmax, int ymax)
        {
            var x1 = (int)Math.Round((x - MinPX) / frameStep);
            var y1 = (int)Math.Round((y - MinPY) / frameStep);
            if (x1 < 0 || y1 < 0 || x1 >= xmax || x1 >= ymax)  // Do nothing for now
                return;
            _shadowBuf[x1, y1] = light;
        }

        private List<Vector3> CalculateStartsV2(CornerId sunCorner, float step)
        {
            var gridResolution = GridResolution;
            var xmin = MinPX;
            var xmax = xmin + (HeightMap.GetLength(0) - 1) * gridResolution;
            var ymin = MinPY;
            var ymax = ymin + (HeightMap.GetLength(1) - 1) * gridResolution;

            //step = 500f*step; // this is for testing only
            //SetCorners();
            var starts = new List<Vector3>();
            switch (sunCorner)
            {
                case CornerId.PP:
                    {
                        //var p = Copy(CornerNP);
                        var p = new Vector3(xmin, ymin, 0f);
                        while (p.X < xmax)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.X += step;
                        }
                        //p = Copy(CornerPP);
                        p = new Vector3(xmax, ymin, 0f);
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
                        var p = new Vector3(xmin, ymax, 0f);
                        while (p.X < xmax)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.X += step;
                        }

                        //p = Copy(CornerPN);
                        p = new Vector3(xmax, ymax, 0f);
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
                        var p = new Vector3(xmin, ymin, 0f);
                        while (p.Y < ymax)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.Y += step;
                        }
                        //p = Copy(CornerNN);
                        p = new Vector3(xmin, ymax, 0f);
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
                        var p = new Vector3(xmin, ymax, 0f);
                        while (p.Y > ymin)
                        {
                            starts.Add(Copy(p));
                            //CheckPosition(p);
                            p.Y -= step;
                        }
                        //p = Copy(CornerNP);
                        p = new Vector3(xmin, ymin, 0f);
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
            if (input == null || input.Width != width || input.Height!= height)
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
            for (var j=0;j< width;j++)
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
                        var val = (byte)(255 * lightFraction/maxv);
                        *(ptr++) = val;
                    }
                }
            }
            result.UnlockBits(bitmapData);
            return result;
        }

    }
}

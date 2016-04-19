using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BitMiracle.LibTiff.Classic;
using System.Windows.Media.Imaging;
using Shadow.terrain;
using Shadow.viz;
using System.Drawing.Imaging;
using Shadows.math;

namespace Shadows2
{
    public partial class Form1 : Form
    {
        const int bytesPerPixel = 4; // This constant must correspond with the pixel format of the converted bitmap.
        public Terrain TheTerrain;

        public Bitmap Rendering;
        float _azimuth = 0f, _elevation= 0.050f;

        public Form1()
        {
            InitializeComponent();
        }

        private void trackAzimuth_Scroll(object sender, EventArgs e)
        {
            _azimuth = trackAzimuth.Value;
            tbAzimuth.Text = string.Format("{0:f3}", _azimuth);
            if (autoUpdateAfterAzElChangeToolStripMenuItem.Checked)
                UpdateToAzimuthAndElevation();
        }

        private void trackElevation_Scroll(object sender, EventArgs e)
        {
            _elevation = trackElevation.Value/100f;
            tbElevation.Text = string.Format("{0:f3}", _elevation);
            if (autoUpdateAfterAzElChangeToolStripMenuItem.Checked)
                UpdateToAzimuthAndElevation();
        }

        private void btnUpdateFromAzEl_Click(object sender, EventArgs e)
        {
            Console.WriteLine(@"Starting");
            UpdateToAzimuthAndElevation();
            Console.WriteLine(@"Done");
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var d = new OpenFileDialog { DefaultExt = ".tif", CheckFileExists = true };
            //if (d.ShowDialog() != DialogResult.OK) return;
            //var filename = d.FileName;

            var filename = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";
            LoadFileToHeightField(filename);
            ShowBitmap(TheTerrain.HeightFieldToBitmap(Rendering));
        }

        void LoadFileToHeightField(string filename)
        {
            float[,] heightMap;
            int width, height;
            using (var stream = File.Open(filename, FileMode.Open))
            {
                var tiffDecoder = new TiffBitmapDecoder(
                    stream,
                    BitmapCreateOptions.PreservePixelFormat | BitmapCreateOptions.IgnoreImageCache,
                    BitmapCacheOption.None);
                //stream.Dispose();

                BitmapSource firstFrame = tiffDecoder.Frames[0];

                width = firstFrame.PixelWidth;
                height = firstFrame.PixelHeight;
                var buf = new byte[width * height * bytesPerPixel];
                firstFrame.CopyPixels(buf, width * bytesPerPixel, 0);

                heightMap = new float[width, height];
                Buffer.BlockCopy(buf, 0, heightMap, 0, buf.Length);
            }

            float maxz = float.MinValue;
            float minz = float.MaxValue;
            for (var i = 0; i < width; i++)
                for (var j = 0; j < height; j++)
                {
                    var v = heightMap[i, j];
                    if (v > maxz) maxz = v;
                    if (v < minz) minz = v;
                }
            Console.WriteLine(@"max={0} min={1}", maxz, minz);

            TheTerrain = new Terrain
            {
                HeightMap = heightMap,
                Box = new BoundingBox(-250f, 250f, -250f, 250f, 0f, maxz - minz),
                MinPZ = minz,
                MaxPZ = maxz,
                MinPX = 0f,
                MaxPX = 500f,
                MinPY = 0f,
                MaxPY = 500f
            };
            Console.WriteLine(@"Loaded.");
        }

        void ShowBitmap(Bitmap bitmap)
        {
            Rendering = bitmap;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.Image = Rendering;
        }

        void UpdateToAzimuthAndElevation()
        {
            var a = _azimuth * Math.PI / 180d;
            var e = _elevation * Math.PI / 180d;
            var z = Math.Sin(e);
            var ec = Math.Cos(e);
            var x = Math.Cos(a) * ec;
            var y = Math.Sin(a) * ec;
            var v = new Vector3d(x, y, z);
            UpdateToSun(v);
        }

        private void renderHeightFieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (TheTerrain == null) return;
            ShowBitmap(TheTerrain.HeightFieldToBitmap(Rendering));
        }

        private void open20X20ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";
            LoadFileToHeightField(filename);
            TheTerrain = SubsetHeightField(TheTerrain, new Rectangle(0, 0, 20, 20));
            ShowBitmap(TheTerrain.HeightFieldToBitmap(Rendering));
        }

        private void open100X100ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";
            LoadFileToHeightField(filename);
            TheTerrain = SubsetHeightField(TheTerrain, new Rectangle(0, 0, 100, 100));
            ShowBitmap(TheTerrain.HeightFieldToBitmap(Rendering));
        }

        private void open400X400ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";
            LoadFileToHeightField(filename);
            TheTerrain = SubsetHeightField(TheTerrain, new Rectangle(0, 0, 400, 400));
            ShowBitmap(TheTerrain.HeightFieldToBitmap(Rendering));
        }

        private void open500X500ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";
            LoadFileToHeightField(filename);
            TheTerrain = SubsetHeightField(TheTerrain, new Rectangle(0, 0, 500, 500));
            ShowBitmap(TheTerrain.HeightFieldToBitmap(Rendering));
        }

        private void open1000X1000ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var filename = @"C:\git\rp_shadows\data\synthetic-lunar-patch.tif";
            LoadFileToHeightField(filename);
            TheTerrain = SubsetHeightField(TheTerrain, new Rectangle(0, 0, 1000, 1000));
            ShowBitmap(TheTerrain.HeightFieldToBitmap(Rendering));
        }

        private Terrain SubsetHeightField(Terrain theTerrain, Rectangle rectangle)
        {
            var newMap = new float[rectangle.Width, rectangle.Height];
            var oldMap = theTerrain.HeightMap;
            var x1 = rectangle.Left;
            var y1 = rectangle.Top;
            var x2 = rectangle.Right;
            var y2 = rectangle.Bottom;
            for (var x = rectangle.Left; x < rectangle.Right; x++)
                for (var y = rectangle.Top; y < rectangle.Bottom; y++)
                    newMap[x - rectangle.Left, y - rectangle.Top] = oldMap[x, y];
            float maxz = float.MinValue;
            float minz = float.MaxValue;
            for (var i = 0; i < rectangle.Width; i++)
                for (var j = 0; j < rectangle.Height; j++)
                {
                    var v = newMap[i, j];
                    if (v > maxz) maxz = v;
                    if (v < minz) minz = v;
                }
            var t = new Terrain
            {
                HeightMap = newMap,
                Box = new BoundingBox(0f, theTerrain.Width * theTerrain.GridResolution, -0f, theTerrain.Height * theTerrain.GridResolution, 0f, maxz - minz),
                MinPZ = minz,
                MaxPZ = maxz,
                MinPX = 0f,
                MaxPX = theTerrain.Width*theTerrain.GridResolution,
                MinPY = 0f,
                MaxPY = theTerrain.Height * theTerrain.GridResolution
            };
            return t;
        }

        private void autoUpdateAfterAzElChangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            autoUpdateAfterAzElChangeToolStripMenuItem.Checked = !autoUpdateAfterAzElChangeToolStripMenuItem.Checked;
        }

        private void singleRayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            singleRayToolStripMenuItem.Checked = !singleRayToolStripMenuItem.Checked;
            if (TheTerrain != null)
                TheTerrain.SingleRay = singleRayToolStripMenuItem.Checked;
        }

        private void synthesize500X500ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var rectangle = new Rectangle { Width = 500, Height = 500 };
            var t = new Terrain
            {
                HeightMap = new float[rectangle.Width, rectangle.Height],
                MinPX = 0f,
                MaxPX = 500f,
                MinPY = 0f,
                MaxPY = 500f
            };
            SyntheticHeightMap(t);
            float maxz = float.MinValue;
            float minz = float.MaxValue;
            var heightMap = t.HeightMap;
            for (var i = 0; i < rectangle.Width; i++)
                for (var j = 0; j < rectangle.Height; j++)
                {
                    var v = heightMap[i, j];
                    if (v > maxz) maxz = v;
                    if (v < minz) minz = v;
                }
            t.Box = new BoundingBox(-250f, 250f, -250f, 250f, 0f, maxz - minz);
            t.MinPZ = minz;
            t.MaxPZ = maxz;
            TheTerrain = t;
            ShowBitmap(TheTerrain.HeightFieldToBitmap(Rendering)); 
        }

        void SyntheticHeightMap(Terrain terrain)
        {
            var a = terrain.HeightMap;
            Array.Clear(a, 0, a.GetLength(0) * a.GetLength(1));

            Tower(a, 4f);
            //CrinkleHeight(a, 2f);
            //FakeCraters(a, 10, terrain.MaxPX / 10f, -10f, new RectangleF(terrain.MinPX,terrain.MinPY,terrain.MaxPX-terrain.MinPX,terrain.MaxPY-terrain.MinPY));
            //ScatterRocks(a, 500, +40f, new RectangleF(terrain.MinPX, terrain.MinPY, terrain.MaxPX - terrain.MinPX, terrain.MaxPY - terrain.MinPY));
        }

        private void Tower(float[,] a, float height)
        {
            var xmax = a.GetLength(0);
            var ymax = a.GetLength(1);
            var ystart = ymax / 2;
            for (var i = 0; i < xmax; i++)
                for (var j = ystart; j < ystart+3; j++)
                    a[j, i] += height;
        }

        void CrinkleHeight(float[,] a, float range)
        {
            var width = a.GetLength(0);
            var height = a.GetLength(1);
            var r = new Random();
            for (var ix=0;ix< width;ix++)
                for (var iy=0;iy< height;iy++)
                    a[ix, iy] += (float)r.NextDouble()*range;
        }

        void FakeCraters(float[,] a, int count, float maxRadius, float depth, RectangleF rect)
        {
            var width = a.GetLength(0);
            var height = a.GetLength(1);
            var r = new Random();
            for (var i=0;i< count;i++)
            {
                var radius = (float)r.NextDouble() * maxRadius;
                var center = new PointF((float)r.NextDouble() * rect.Width, (float)r.NextDouble() * rect.Height);
                for (var x = center.X - radius; x < center.X + radius; x += 1f)
                    for (var y = center.Y - radius; y < center.Y + radius; y += 1f)
                    {
                        var ix = (int)x;
                        var iy = (int)y;
                        if (ix < 0 || iy < 0 || ix >= width || iy >= height)
                            continue;
                        var p = new PointF(x, y);
                        if (p.Distance(center) < radius)
                            a[ix, iy] += depth;
                    }
            }
        }

        void ScatterRocks(float[,] a, int count, float maxHeight, RectangleF rect)
        {
            var width = a.GetLength(0);
            var height = a.GetLength(1);
            var r = new Random();
            for (var i = 0; i < count; i++)
            {
                var rockHeight = (float)r.NextDouble() * maxHeight;
                var rockRadius = (int)((float)r.NextDouble() * 3f);
                var center = new Point((int)((float)r.NextDouble() * rect.Width), (int)((float)r.NextDouble() * rect.Height));
                for (var ix = center.X - rockRadius; ix < center.X + rockRadius; ix += 1)
                    for (var iy = center.Y - rockRadius; iy < center.Y + rockRadius; iy += 1)
                    {
                        if (ix < 0 || iy < 0 || ix >= width || iy >= height)
                            continue;
                        a[ix, iy] += rockHeight;
                    }
            }
        }

        private void trackAzimuth_MouseUp(object sender, MouseEventArgs e)
        {
            UpdateToAzimuthAndElevation();
        }

        private void trackElevation_MouseUp(object sender, MouseEventArgs e)
        {
            UpdateToAzimuthAndElevation();
        }

        private void tbScale_ValueChanged(object sender, EventArgs e)
        {
            pictureBox1.Size = new Size(tbScale.Value*500, tbScale.Value*500);
        }

        private void tbSunRadius_TextChanged(object sender, EventArgs e)
        {
            double r;
            tbSunRadius.ForeColor = double.TryParse(tbSunRadius.Text, out r) ? Color.Black : Color.Red;
        }

        private void synthesize8000X8000ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var rectangle = new Rectangle { Width = 8000, Height = 8000 };
            var t = new Terrain
            {
                HeightMap = new float[rectangle.Width, rectangle.Height],
                MinPX = 0f,
                MaxPX = 500f,
                MinPY = 0f,
                MaxPY = 500f
            };
            SyntheticHeightMap(t);
            float maxz = float.MinValue;
            float minz = float.MaxValue;
            var heightMap = t.HeightMap;
            for (var i = 0; i < rectangle.Width; i++)
                for (var j = 0; j < rectangle.Height; j++)
                {
                    var v = heightMap[i, j];
                    if (v > maxz) maxz = v;
                    if (v < minz) minz = v;
                }
            t.Box = new BoundingBox(-250f, 250f, -250f, 250f, 0f, maxz - minz);
            t.MinPZ = minz;
            t.MaxPZ = maxz;
            TheTerrain = t;
            ShowBitmap(TheTerrain.HeightFieldToBitmap(Rendering));
        }

        private void synthesize400X400ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var rectangle = new Rectangle { Width = 400, Height = 400 };
            var t = new Terrain
            {
                HeightMap = new float[rectangle.Width, rectangle.Height],
                MinPX = 0f,
                MaxPX = 500f,
                MinPY = 0f,
                MaxPY = 500f
            };
            SyntheticHeightMap(t);
            float maxz = float.MinValue;
            float minz = float.MaxValue;
            var heightMap = t.HeightMap;
            for (var i = 0; i < rectangle.Width; i++)
                for (var j = 0; j < rectangle.Height; j++)
                {
                    var v = heightMap[i, j];
                    if (v > maxz) maxz = v;
                    if (v < minz) minz = v;
                }
            t.Box = new BoundingBox(-250f, 250f, -250f, 250f, 0f, maxz - minz);
            t.MinPZ = minz;
            t.MaxPZ = maxz;
            TheTerrain = t;
            ShowBitmap(TheTerrain.HeightFieldToBitmap(Rendering));
        }

        private void synthesize10X10ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var rectangle = new Rectangle { Width = 10, Height = 10 };
            var t = new Terrain
            {
                HeightMap = new float[rectangle.Width, rectangle.Height],
                MinPX = 0f,
                MaxPX = 500f,
                MinPY = 0f,
                MaxPY = 500f
            };
            SyntheticHeightMap(t);
            float maxz = float.MinValue;
            float minz = float.MaxValue;
            var heightMap = t.HeightMap;
            for (var i = 0; i < rectangle.Width; i++)
                for (var j = 0; j < rectangle.Height; j++)
                {
                    var v = heightMap[i, j];
                    if (v > maxz) maxz = v;
                    if (v < minz) minz = v;
                }
            t.Box = new BoundingBox(-250f, 250f, -250f, 250f, 0f, maxz - minz);
            t.MinPZ = minz;
            t.MaxPZ = maxz;
            TheTerrain = t;
            ShowBitmap(TheTerrain.HeightFieldToBitmap(Rendering));
        }

        void UpdateToSun(Vector3d v)
        {
            if (TheTerrain == null) return;
            TheTerrain.Clear();

            if (!cbRaycastMulti.Checked)
            {
                TheTerrain.UpdateToSunV3(v);
            }
            else
            {
                double sunRadiusDeg;
                if (!double.TryParse(tbSunRadius.Text, out sunRadiusDeg)) return;
                TheTerrain.UpdateToSunV3(v, 0.5f);
                var axis = new Vector3d(0f, 0f, 1f);
                var angle = (sunRadiusDeg * Math.PI / 180d);
                var v0 = new Vector4d(v);
                var m1 = Matrix4d.CreateFromAxisAngle(axis, angle);
                var v1 = Vector4d.Transform(v0, m1);
                var m2 = Matrix4d.CreateFromAxisAngle(axis, -angle);
                var v2 = Vector4d.Transform(v0, m2);
                TheTerrain.UpdateToSunV3(new Vector3d(v1), 0.25f, 0f);
                TheTerrain.UpdateToSunV3(new Vector3d(v2), 0.25f, 0f);
            }

            ShowBitmap(TheTerrain.ShadowBufferToScaledImageV4(Rendering));
        }

    }
}

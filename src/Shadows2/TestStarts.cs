using Shadow.terrain;
using Shadows.math;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shadows2
{
    public partial class TestStarts : Form
    {
        Pen thePen = new Pen(Brushes.Black, 0);
        List<Vector2> Starts = new List<Vector2>();
        RectangleF ClipBounds = new RectangleF(100f,100f,400f,400f);
        Terrain TheTerrain = new Terrain();
        Vector3 ToSun = new Vector3(1f, 0f, 0f);
        bool Dragging = false;

        public TestStarts()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            //g.Transform = GetTransform();

            //g.DrawLine(thePen, 0f, 0f, 1f, 1f);
            g.DrawRectangle(Pens.Black, ClipBounds.X, ClipBounds.Y, ClipBounds.Width, ClipBounds.Height);

            foreach (var v in Starts)
            {
                g.FillRectangle(Brushes.Red, v.X, v.Y, 2f, 2f);
            }

            var a = ClipBoundsCenter();
            var b = ToSunArrowTip();
            g.DrawLine(Pens.Green, a, b);
        }

        public PointF ClipBoundsCenter()
        {
            return new PointF((ClipBounds.Left + ClipBounds.Right) / 2f, (ClipBounds.Top + ClipBounds.Bottom) / 2f);
        }

        PointF ToSunArrowTip()
        {
            var center = ClipBoundsCenter();
            var arrow = ToSun * (ClipBounds.Width / 3f);
            return new PointF(center.X + arrow.X, center.Y + arrow.Y);
        }

        System.Drawing.Drawing2D.Matrix GetTransform()
        {
            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
            //m.Translate(200f, 200f);
            //m.Scale(10000f, 10000f);
            return m;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ToSun = new Vector3(1f, 0f, 0f);
            Starts = TheTerrain.CalculateStartsV4(ToSun, 20f, ClipBounds);
            Console.WriteLine(@"There are {0} starts", Starts.Count);
            panel1.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ToSun = new Vector3(1f, 1f, 0f);
            Starts = TheTerrain.CalculateStartsV4(ToSun, 10f, ClipBounds);
            Console.WriteLine(@"There are {0} starts", Starts.Count);
            panel1.Invalidate();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ToSun = new Vector3(-1f, 1f, 0f);
            Starts = TheTerrain.CalculateStartsV4(ToSun, 10f, ClipBounds);
            Console.WriteLine(@"There are {0} starts", Starts.Count);
            panel1.Invalidate();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ToSun = new Vector3(-1f, -1f, 0f);
            Starts = TheTerrain.CalculateStartsV4(ToSun, 10f, ClipBounds);
            Console.WriteLine(@"There are {0} starts", Starts.Count);
            panel1.Invalidate();
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            var center = ToSunArrowTip();
            var mouse = new PointF(e.X, e.Y);
            Dragging = center.Distance(mouse) <= 5f;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Dragging) return;
            var center = ClipBoundsCenter();
            var toSun = new Vector3(e.X - center.X, e.Y - center.Y, 0f);
            ToSun = toSun.Normalized();
            Starts = TheTerrain.CalculateStartsV4(ToSun, 10f, ClipBounds);
            panel1.Invalidate();
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            Dragging = false;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ToSun = new Vector3(-1f, -1f, 0f);
            Starts = TheTerrain.CalculateStartsV4(ToSun, 3f, ClipBounds);
            Console.WriteLine(@"There are {0} starts", Starts.Count);
            panel1.Invalidate();
        }
    }
}

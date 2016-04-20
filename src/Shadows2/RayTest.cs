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
    public partial class RayTest : Form
    {
        Pen thePen = new Pen(Brushes.Black, 0);
        List<SunRay> SunRays = new List<SunRay>();

        public RayTest()
        {
            InitializeComponent();
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Transform = GetTransform();

            //g.DrawLine(thePen, 0f, 0f, 1f, 1f);
            var radius = 1f;
            g.DrawEllipse(thePen, new RectangleF(-radius, -radius, 2f * radius, 2f * radius));

            foreach (var r in SunRays)
            {
                var v = r.ToSun;
                g.FillRectangle(Brushes.Red, v.Y, v.Z, 0.001f, 0.001f);
            }
        }

        System.Drawing.Drawing2D.Matrix GetTransform()
        {
            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
            m.Translate(200f, 200f);
            m.Scale(10000f, 10000f);
            return m;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var terrain = new Terrain();
            var toSun = new Vector3(1f, 0f, 0f);
            int verticalCount;
            if (!int.TryParse(tbVerticalCount.Text, out verticalCount)) return;
            int horizontalCount;
            if (!int.TryParse(tbHorizontalCount.Text, out horizontalCount)) return;
            SunRays = terrain.CalculateSunRaysV4(toSun, verticalCount, horizontalCount);
            Console.WriteLine(@"There are {0} rays", SunRays.Count);
            panel1.Invalidate();

            foreach (var r in SunRays)
            {
                var v = r.ToSun;
                Console.WriteLine(@"[{0},{1}] => {2}", v.Y, v.Z, r.SunFraction);
            }
        }
    }
}

using Shadows2.math;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shadows2
{
    public partial class TriangleTests : Form
    {
        RectangleF clipRegion = new RectangleF(0f, 0f, 200f, 200f);
        //Triangle StartTriangle = new Triangle(new PointF(10f,10f),new PointF(100f, 20f), new PointF(50f, 150f));

        Triangle StartTriangle = new Triangle(new PointF(300f, 100f), new PointF(100f, 50f), new PointF(100f, 150f));

        List<Triangle> ClippedTriangles = new List<Triangle>();
        List<Brush> TriangleBrushes = new List<Brush>() { Brushes.LightGreen, Brushes.Goldenrod, Brushes.LightBlue, Brushes.LightCoral, Brushes.LightSalmon,
            Brushes.LightSeaGreen, Brushes.LightBlue, Brushes.PaleGreen, Brushes.PaleGoldenrod, Brushes.PaleTurquoise };

        int pointDragging = 0; // 1=A, 2=B, 3=C

        public TriangleTests()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var c = new TriangleClipper();

            var p00 = new PointF(0f, 0f);
            var p01 = new PointF(0f, 1f);
            var p10 = new PointF(1f, 0f);
            var p11 = new PointF(1f, 1f);

            var p = c.ClipLineVertical(p00, p10, 0.5f);
            Debug.Assert(p.X == 0.5f && p.Y == 0f);

            p = c.ClipLineVertical(p00, p10, 1.5f);
            Debug.Assert(p.X == 1.5f && p.Y == 0f);

            p = c.ClipLineVertical(p00, p11, 0.5f);
            Debug.Assert(p.X == 0.5f && p.Y == 0.5f);

            p = c.ClipLineVertical(p00, p11, 1.0f);
            Debug.Assert(p.X == 1.0f && p.Y == 1.0f);

            p = c.ClipLineHorizontal(p00, p01, 0.5f);
            Debug.Assert(p.X == 0f && p.Y == 0.5f);

            p = c.ClipLineHorizontal(p00, p11, 0.5f);
            Debug.Assert(p.X == 0.5f && p.Y == 0.5f);

            Debug.Assert(c.OppositeSidesOfLine(p01, p10, p00, p11));
            Debug.Assert(!c.OppositeSidesOfLine(p01, p01, p00, p11));

            var t1 = new Triangle(new PointF(2f, 2f), new PointF(4f, 4f), new PointF(4f, 2f));
            var t2 = new Triangle(new PointF(-1f, 4f), new PointF(1f, 4f), new PointF(-1f, 6f));
            var t3 = new Triangle(new PointF(1f, 4f), new PointF(-1f, 6f), new PointF(-1f, 4f));

            Debug.Assert(t1.Equivalent(t1));
            Debug.Assert(t2.Equivalent(t2));
            Debug.Assert(!t1.Equivalent(t2));
            Debug.Assert(!t2.Equivalent(t1));
            Debug.Assert(t2.Equivalent(t3));
            Debug.Assert(t3.Equivalent(t2));

            Console.WriteLine(@"Pass.");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var c = new TriangleClipper();

            var r = new RectangleF(0f, 0f, 10f, 10f);

            var t1 = new Triangle(new PointF(2f, 2f), new PointF(4f, 4f), new PointF(4f, 2f));

            Debug.Assert(t1.IsValid);
            Debug.Assert(!(new Triangle(new PointF(), new PointF(), new PointF())).IsValid);

            var recording = RunRecording(c, t1, r);
            var handled = recording.WithName("Handle");
            Debug.Assert(handled.Count == 1);
            Debug.Assert(t1.Equivalent(handled[0].Triangle));

            var t2 = new Triangle(new PointF(-1f, 4f), new PointF(1f, 4f), new PointF(-1f, 6f));
            recording = RunRecording(c, t2, r);
            handled = recording.WithName("Handle");
            Debug.Assert(handled.Count == 1);
            Debug.Assert(handled[0].Triangle.Equivalent(new Triangle(new PointF(1f, 4f), new PointF(0f, 4f), new PointF(0f, 5f))));

            var t3 = new Triangle(new PointF(-1f, 1f), new PointF(2f, 1f), new PointF(2f, 4f));
            recording = RunRecording(c, t3, r);
            handled = recording.WithName("Handle");
            Debug.Assert(handled.Count == 2);
            Debug.Assert(EquivalentCoverage(t3, recording));

            var clipRegion = new RectangleF(0f, 0f, 200f, 200f);
            var t4 = new Triangle(new PointF(300f, 100f), new PointF(100f, 50f), new PointF(100f, 150f));
            recording = RunRecording(c, t4, clipRegion);
            handled = recording.WithName("Handle");

            Console.WriteLine(@"Pass");
        }

        Recording RunRecording(TriangleClipper c, Triangle t, RectangleF r)
        {
            c.Records.Clear();
            c.AddLight(t, r);
            return new Recording { Records = c.Records };
        }

        /// <summary>
        /// This determines whether the  list of handled triangles in the recording is equivalent to the original triangle.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        bool EquivalentCoverage(Triangle t, Recording r)
        {
            var tris = r.WithName("Handle");
            var area = t.Area;
            var rArea = tris.Sum(t1 => t1.Triangle.Area);
            if (0.01f > Math.Abs(area - rArea)) return false;
            return true;
        }

        System.Drawing.Drawing2D.Matrix GetTransform()
        {
            System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
            m.Translate(100f, 400f);
            m.Scale(1f, -1f);
            return m;
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Transform = GetTransform();

            g.DrawRectangle(Pens.Black, clipRegion.X, clipRegion.Y, clipRegion.Width, clipRegion.Height);
            g.DrawLine(Pens.Green, 0f, 0f, clipRegion.Width, clipRegion.Height/4f);

            if (ClippedTriangles.Count <= TriangleBrushes.Count)
            {
                for (var i = 0; i < ClippedTriangles.Count; i++)
                    g.FillPolygon(TriangleBrushes[i], ClippedTriangles[i].Points);
            }
            else
            {
                Console.WriteLine(@"Too many clipped triangles to draw.  Not enough brushes.");
            }

            g.DrawPolygon(Pens.Red, StartTriangle.Points);
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            var p = SelectedPoint(e);
            if (p.Equivalent(StartTriangle.A, 5f))
                pointDragging = 1;
            else if (p.Equivalent(StartTriangle.B, 5f))
                pointDragging = 2;
            else if (p.Equivalent(StartTriangle.C, 5f))
                pointDragging = 3;
            else
                pointDragging = 0;
            //Console.WriteLine(@"pointDragging={0}", pointDragging);
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            pointDragging = 0;
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (pointDragging == 0) return;
            var p = SelectedPoint(e);
            //Console.WriteLine(@"dragging point={0}", p);
            switch (pointDragging)
            {
                case 1:
                    StartTriangle.A = p;
                    break;
                case 2:
                    StartTriangle.B = p;
                    break;
                case 3:
                    StartTriangle.C = p;
                    break;
            }
            panel1.Invalidate();

            btnClipTriangle_Click(null, null);
        }

        PointF SelectedPoint(MouseEventArgs e)
        {
            var m = GetTransform();
            m.Invert();
            var ary = new PointF[] { new PointF(e.X, e.Y) };
            m.TransformPoints(ary);
            return ary[0];
        }

        private void btnClipTriangle_Click(object sender, EventArgs e)
        {
            var c = new TriangleClipper();
            var recording = RunRecording(c, StartTriangle, clipRegion);
            var handled = recording.WithName("Handle");
            ClippedTriangles = handled.Select(t => t.Triangle).ToList();
            panel1.Invalidate();

            lblTris.Text = ClippedTriangles.Count.ToString();
            lblArea.Text = ClippedTriangles.Sum(t => t.Area).ToString();
        }

        private void btnRotateTriangle_Click(object sender, EventArgs e)
        {
            StartTriangle = RotatePoints(StartTriangle);
            btnClipTriangle_Click(sender, e);
        }

        Triangle RotatePoints(Triangle t)
        {
            return new Triangle(t.B, t.C, t.A);
        }

        Triangle FlipPoints(Triangle t)
        {
            return new Triangle(t.C, t.B, t.A);
        }

        private void btnFlipTriangle_Click(object sender, EventArgs e)
        {
            StartTriangle = FlipPoints(StartTriangle);
            btnClipTriangle_Click(sender, e);
        }
    }

    public class Recording
    {
        public List<LabeledTriangle> Records = new List<LabeledTriangle>();

        public List<LabeledTriangle> WithName(string name)
        {
            return Records.Where(r => name.Equals(r.Label)).ToList();
        }
    }
}

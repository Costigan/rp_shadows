using System.Drawing;
using System.Windows.Forms;

namespace LadeeViz.Viz
{
    public class DarkComboBox : ComboBox
    {
        public DarkComboBox()
        {
            base.DrawMode = DrawMode.OwnerDrawFixed;
            HighlightColor = Color.Gray;
            DrawItem += DarkComboBox_DrawItem;
        }

        public Color HighlightColor { get; set; }


        private void DarkComboBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            var combo = sender as ComboBox;
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e.Graphics.FillRectangle(new SolidBrush(HighlightColor),
                                         e.Bounds);
            else
                e.Graphics.FillRectangle(new SolidBrush(combo.BackColor),
                                         e.Bounds);

            e.Graphics.DrawString(combo.Items[e.Index].ToString(), e.Font,
                                  new SolidBrush(combo.ForeColor),
                                  new Point(e.Bounds.X, e.Bounds.Y));

            e.DrawFocusRectangle();
        }
    }
}
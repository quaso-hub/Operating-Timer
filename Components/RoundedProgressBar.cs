using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Operating_Timer.Components
{
    public class RoundedProgressBar : Control
    {
        private int value = 0;
        public int Value
        {
            get => value;
            set
            {
                this.value = Math.Max(0, Math.Min(100, value));
                this.Invalidate();
            }
        }

        public Color BarColor { get; set; } = Color.LightGreen;
        public Color BackBarColor { get; set; } = Color.FromArgb(35, 55, 90);

        public RoundedProgressBar()
        {
            this.DoubleBuffered = true;
            this.Height = 10;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Rectangle rectBack = new Rectangle(0, 0, this.Width, this.Height);
            Rectangle rectBar = new Rectangle(0, 0, this.Width * value / 100, this.Height);

            using (Brush back = new SolidBrush(BackBarColor))
                g.FillRectangle(back, rectBack);

            using (GraphicsPath path = RoundedRect(rectBar, 5))
            using (Brush brush = new SolidBrush(BarColor))
                g.FillPath(brush, path);
        }

        private System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
            path.AddArc(bounds.Right - radius, bounds.Y, radius, radius, 270, 90);
            path.AddArc(bounds.Right - radius, bounds.Bottom - radius, radius, radius, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

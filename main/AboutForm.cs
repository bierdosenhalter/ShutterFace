using System.Drawing;

namespace MotionTrackerFaceBlur
{
    internal sealed partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        internal static Bitmap CreateAppIcon()
        {
            var bmp = new Bitmap(30, 30);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.FromArgb(0, 120, 215));

            using var pen = new Pen(Color.White, 2f);
            g.DrawRectangle(pen, 4, 7, 22, 16);
            g.DrawEllipse(pen, 10, 11, 10, 10);

            g.FillPolygon(new SolidBrush(Color.White), new[]
            {
                new PointF(24, 7),
                new PointF(27, 7),
                new PointF(28.5f, 10),
                new PointF(22.5f, 10)
            });

            g.FillRectangles(new SolidBrush(Color.White), new[]
            {
                new Rectangle(6, 4, 4, 3),
                new Rectangle(13, 4, 4, 3),
                new Rectangle(20, 4, 4, 3)
            });

            return bmp;
        }
    }
}

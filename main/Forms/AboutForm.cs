using ShutterFace.Resources;
using System.Drawing;

namespace ShutterFace
{
    internal sealed partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
            UpdateTexts();
        }

        private void UpdateTexts()
        {
            lblTitle.Text = ControlResourceManager.GetString("TitleAbout");
            lblSubtitle.Text = ControlResourceManager.GetString("SubtitleAbout");
            btnOk.Text = ControlResourceManager.GetString("BtnOk");
        }

        private void BtnOk_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        internal static Bitmap CreateAppIcon()
        {
            var asm = typeof(AboutForm).Assembly;
            using var stream = asm.GetManifestResourceStream("ShutterFace.icon.ico");
            using var icon = new Icon(stream!);
            return icon.ToBitmap();
        }

        internal static Icon CreateAppIconAsIcon()
        {
            var asm = typeof(AboutForm).Assembly;
            using var stream = asm.GetManifestResourceStream("ShutterFace.icon.ico");
            return new Icon(stream!);
        }
    }
}

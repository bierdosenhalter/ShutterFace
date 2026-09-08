using System.ComponentModel;
using System.Windows.Forms;

namespace MotionTrackerFaceBlur
{
    public partial class SettingsForm : Form
    {
        private int _blurCellSize;
        private int _bigPixels;
        private float _confidenceThreshold;

        public SettingsForm()
        {
            InitializeComponent();
            LoadDefaults();
        }

        public int BlurCellSize => _blurCellSize;
        public int BigPixels => _bigPixels;
        public float ConfidenceThreshold => _confidenceThreshold;

        private void LoadDefaults()
        {
            nudBlurCellSize.Value = 8m;
            nudBigPixels.Value = 16m;
            nudConfidenceThreshold.Value = 0.7m;
        }

        private void SettingsForm_Load(object? sender, EventArgs e)
        {
            nudBlurCellSize.Minimum = 2;
            nudBlurCellSize.Maximum = 50;
            nudBlurCellSize.Increment = 1m;

            nudBigPixels.Minimum = 4;
            nudBigPixels.Maximum = 64;
            nudBigPixels.Increment = 1m;

            nudConfidenceThreshold.Minimum = 0.1m;
            nudConfidenceThreshold.Maximum = 1m;
            nudConfidenceThreshold.Increment = 0.05m;
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            _blurCellSize = (int)nudBlurCellSize.Value;
            _bigPixels = (int)nudBigPixels.Value;
            _confidenceThreshold = (float)nudConfidenceThreshold.Value;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnCancel_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }

    internal static class SettingsFormDesigner
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static SettingsForm CreateDefault() => new();
    }
}

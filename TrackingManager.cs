using System.Drawing;
using System.Windows.Forms;

namespace MotionTrackerFaceBlur
{
    /// <summary>Manages tracking rectangles and their UI indicators.</summary>
    public class TrackingManager
    {
        private readonly Label? _trackRangeIndicator;
        private int _totalFrames = 0;
        private (float startPercent, float endPercent)? _cachedRange;

        public TrackingManager(Label? trackRangeIndicator)
        {
            _trackRangeIndicator = trackRangeIndicator;
        }

        public void SetTotalFrames(int totalFrames) => _totalFrames = totalFrames;

        public void ShowFrameRangeOnSlider(int startFrame, int endFrame, TrackBar frameSlider, Label statusLabel)
        {
            frameSlider.Minimum = 0;
            frameSlider.Maximum = _totalFrames - 1;
            frameSlider.TickFrequency = Math.Max(1, (_totalFrames - 1) / 20);

            statusLabel.Text = $"Track range: Frame {startFrame} to {endFrame}";

            UpdateTrackRangeIndicator(startFrame, endFrame);
        }

        public void UpdateTrackRangeIndicator(int startFrame, int endFrame)
        {
            if (_trackRangeIndicator == null || _totalFrames <= 0) return;

            float startPercent = (float)startFrame / _totalFrames;
            float endPercent = (float)endFrame / _totalFrames;

            _cachedRange = (startPercent, endPercent);

            var range = _cachedRange.Value;
            _trackRangeIndicator.Visible = true;

            _trackRangeIndicator.Paint -= TrackRangeIndicator_Paint;
            _trackRangeIndicator.Paint += TrackRangeIndicator_Paint;
            _trackRangeIndicator.Invalidate();
        }

        private void TrackRangeIndicator_Paint(object? sender, PaintEventArgs e)
        {
            if (_cachedRange == null) return;

            float startPercent = _cachedRange.Value.startPercent;
            float endPercent = _cachedRange.Value.endPercent;

            if (sender is not Panel panel) return;

            int panelWidth = panel.Width;
            int panelHeight = panel.Height;

            e.Graphics.Clear(SystemColors.Control);

            int startX = (int)(startPercent * panelWidth);
            int endX = (int)(endPercent * panelWidth);
            int width = endX - startX;

            if (width > 0)
            {
                using Brush brush = new SolidBrush(Color.FromArgb(120, Color.LimeGreen));
                e.Graphics.FillRectangle(brush, startX, 0, width, panelHeight);

                using Pen pen = new(Color.LimeGreen, 2);
                e.Graphics.DrawRectangle(pen, startX, 0, width, panelHeight);
            }
        }

        public void UpdateProperties(Form form, Tracking tracking, int totalFrames, TextBox txtName, NumericUpDown txtWidth, NumericUpDown txtHeight, NumericUpDown txtStartFrame, NumericUpDown txtEndFrame)
        {
            txtName.Text = tracking.Name;
            txtWidth.Value = tracking.InitialRect.Width;
            txtHeight.Value = tracking.InitialRect.Height;

            txtStartFrame.Minimum = 0;
            txtStartFrame.Maximum = totalFrames - 1;
            txtStartFrame.Value = Math.Max(0, Math.Min(tracking.StartFrame, totalFrames - 1));

            txtEndFrame.Minimum = 0;
            txtEndFrame.Maximum = totalFrames - 1;
            txtEndFrame.Value = Math.Max(0, Math.Min(tracking.EndFrame, totalFrames - 1));
        }
    }
}

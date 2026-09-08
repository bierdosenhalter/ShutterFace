using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Globalization;
using System.Diagnostics;
using System.Text.Json;

namespace MotionTrackerFaceBlur
{
    public partial class VideoPlayer : Form, IDisposable
    {
        #region Tracking Management

        #region Tracking Management

        private void AddTrackingBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Click and drag on the video to create a tracking rectangle.");
        }

        private void DeleteTrackingBtn_Click(object sender, EventArgs e)
        {
            if (_selectedTrackingIndex.HasValue && _selectedTrackingIndex.Value < _trackingRects.Count)
            {
                _trackingRects.RemoveAt(_selectedTrackingIndex.Value);
                TrackingListBox.Items.RemoveAt(_selectedTrackingIndex.Value);
                _selectedTrackingIndex = null;
                gprTracking.Enabled = false;
                AnalyzeBtn.Visible = false;
                DeleteTrackingBtn.Enabled = false;
                DisplayFrame(_currentFrame);
            }
        }

        private void TrackingListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TrackingListBox.SelectedIndex >= 0 && TrackingListBox.SelectedIndex < _trackingRects.Count)
            {
                _selectedTrackingIndex = TrackingListBox.SelectedIndex;
                var tracking = _trackingRects[_selectedTrackingIndex.Value];

                ShowFrameRangeOnSlider(tracking.StartFrame, tracking.EndFrame);

                if (_currentFrameIndex < tracking.StartFrame)
                {
                    LoadFrame(tracking.StartFrame);
                    FrameSlider.Value = tracking.StartFrame;
                }
                else if (_currentFrameIndex > tracking.EndFrame)
                {
                    LoadFrame(tracking.EndFrame);
                    FrameSlider.Value = tracking.EndFrame;
                }
                else
                {
                    DisplayFrame(_currentFrame);
                }

                UpdateTrackingProperties();
                gprTracking.Enabled = true;
                AnalyzeBtn.Visible = true;
                DeleteTrackingBtn.Enabled = true;
            }
            else
            {
                // No track selected, hide the indicator
                if (trackRangeIndicator != null)
                    trackRangeIndicator.Visible = false;
            }
        }

        private void ShowFrameRangeOnSlider(int startFrame, int endFrame)
        {
            FrameSlider.Minimum = 0;
            FrameSlider.Maximum = _totalFrames - 1;
            FrameSlider.TickFrequency = Math.Max(1, (_totalFrames - 1) / 20);

            tssStatusLabel.Text = $"Track range: Frame {startFrame} to {endFrame}";

            // Update the track range indicator
            UpdateTrackRangeIndicator(startFrame, endFrame);
        }

        private void UpdateTrackRangeIndicator(int startFrame, int endFrame)
        {
            if (trackRangeIndicator == null || _totalFrames <= 0) return;

            // Calculate the position and width based on frame range
            float startPercent = (float)startFrame / _totalFrames;
            float endPercent = (float)endFrame / _totalFrames;

            int indicatorLeft = (int)(startPercent * trackRangeIndicator.Width);
            int indicatorWidth = (int)((endPercent - startPercent) * trackRangeIndicator.Width);

            // Create a new panel or update existing one
            // Since we can't easily overlay on the panel itself, we'll update the panel's background
            // to show the range using a painted indicator

            // For simplicity, we'll use the panel's width to represent the range
            // and position it accordingly
            trackRangeIndicator.Visible = true;

            // We need to create a visual indicator within the panel
            // Let's use a simple approach: paint the range on the panel
            trackRangeIndicator.Paint -= TrackRangeIndicator_Paint;
            trackRangeIndicator.Paint += TrackRangeIndicator_Paint;
            trackRangeIndicator.Tag = new { StartPercent = startPercent, EndPercent = endPercent };
            trackRangeIndicator.Invalidate();
        }

        private void TrackRangeIndicator_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel || panel.Tag == null) return;

            var range = (dynamic)panel.Tag;
            float startPercent = range.StartPercent;
            float endPercent = range.EndPercent;

            int panelWidth = panel.Width;
            int panelHeight = panel.Height;

            // Clear the panel
            e.Graphics.Clear(SystemColors.Control);

            // Draw the range indicator
            int startX = (int)(startPercent * panelWidth);
            int endX = (int)(endPercent * panelWidth);
            int width = endX - startX;

            if (width > 0)
            {
                using Brush brush = new SolidBrush(Color.FromArgb(120, Color.LimeGreen));
                e.Graphics.FillRectangle(brush, startX, 0, width, panelHeight);

                // Draw border around the range
                using Pen pen = new(Color.LimeGreen, 2);
                e.Graphics.DrawRectangle(pen, startX, 0, width, panelHeight);
            }
        }

        private void UpdateTrackingProperties()
        {
            if (_selectedTrackingIndex.HasValue && _selectedTrackingIndex.Value < _trackingRects.Count)
            {
                var tracking = _trackingRects[_selectedTrackingIndex.Value];
                txtTrackingName.Text = tracking.Name;
                txtBoxWidth.Value = tracking.InitialRect.Width;
                txtBoxHeight.Value = tracking.InitialRect.Height;

                txtStartFrame.Minimum = 0;
                txtStartFrame.Maximum = _totalFrames - 1;
                txtStartFrame.Value = Math.Max(0, Math.Min(tracking.StartFrame, _totalFrames - 1));

                txtEndFrame.Minimum = 0;
                txtEndFrame.Maximum = _totalFrames - 1;
                txtEndFrame.Value = Math.Max(0, Math.Min(tracking.EndFrame, _totalFrames - 1));
            }
        }

        private void BtnApplyChanges_Click(object sender, EventArgs e)
        {
            if (_selectedTrackingIndex.HasValue && _selectedTrackingIndex.Value < _trackingRects.Count)
            {
                var tracking = _trackingRects[_selectedTrackingIndex.Value];

                int newWidth = (int)txtBoxWidth.Value;
                int newHeight = (int)txtBoxHeight.Value;
                tracking.InitialRect = new Rect(
                    tracking.InitialRect.X,
                    tracking.InitialRect.Y,
                    newWidth,
                    newHeight
                );

                int newStartFrame = (int)txtStartFrame.Value;
                int newEndFrame = (int)txtEndFrame.Value;

                if (newStartFrame > newEndFrame)
                {
                    MessageBox.Show("Start frame cannot be after end frame. Adjusting values.");
                    newStartFrame = newEndFrame;
                    txtStartFrame.Value = newStartFrame;
                }

                bool frameRangeChanged = (tracking.StartFrame != newStartFrame) || (tracking.EndFrame != newEndFrame);

                if (tracking.IsAnalyzed && frameRangeChanged)
                {
                    var positionsToRemove = tracking.GetRectPositions()
                        .Where(kvp => kvp.Key < newStartFrame || kvp.Key > newEndFrame)
                        .Select(kvp => kvp.Key)
                        .ToList();

                    foreach (var frame in positionsToRemove)
                        tracking.RemoveFramePosition(frame);
                }
                else if (!tracking.IsAnalyzed && frameRangeChanged)
                {
                    tracking.ClearPositions();
                }

                tracking.StartFrame = newStartFrame;
                tracking.EndFrame = newEndFrame;

                if (!string.IsNullOrEmpty(txtTrackingName.Text))
                {
                    tracking.Name = txtTrackingName.Text;
                    TrackingListBox.Items[_selectedTrackingIndex.Value] = tracking.Name;
                }

                DisplayFrame(_currentFrame);
                MessageBox.Show("Tracking properties updated successfully!");
            }
        }

        #endregion

        #endregion
    }
}

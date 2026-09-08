using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Globalization;
using System.Diagnostics;
using System.Text.Json;

namespace MotionTrackerFaceBlur
{
    public partial class VideoPlayer : Form, IDisposable
    {
        #region Frame Display & Drawing

        #region Frame Display & Drawing

        private void DisplayFrame(Mat frame)
        {
            if (frame == null) return;

            using Mat displayFrame = frame.Clone();

            try
            {
                for (int i = 0; i < _trackingRects.Count; i++)
                {
                    var tracking = _trackingRects[i];
                    if (_currentFrameIndex >= tracking.StartFrame && _currentFrameIndex <= tracking.EndFrame)
                    {
                        Rect? rect = tracking.GetRectAtFrame(_currentFrameIndex);

                        if (!rect.HasValue)
                            rect = tracking.InitialRect;

                        if (rect.HasValue)
                        {
                            Scalar color;
                            if (!tracking.IsAnalyzed)
                                color = Scalar.Red;
                            else
                                color = (i == _selectedTrackingIndex) ? Scalar.LimeGreen : Scalar.Gray;

                            int thickness = (i == _selectedTrackingIndex) ? 3 : 1;

                            Cv2.Rectangle(displayFrame, rect.Value, color, thickness);
                            Cv2.PutText(displayFrame, tracking.Name,
                                new OpenCvSharp.Point(rect.Value.X, rect.Value.Y - 10),
                                HersheyFonts.HersheySimplex, 0.5, color, 1);

                            if (i == _selectedTrackingIndex)
                            {
                                RectGeometry.DrawResizeHandles(displayFrame, rect.Value);
                            }
                        }
                    }
                }

                using Bitmap bitmap = BitmapConverter.ToBitmap(displayFrame);

                void SetImage()
                {
                    VideoBox.Image?.Dispose();
                    VideoBox.Image = bitmap;
                }

                if (VideoBox.InvokeRequired)
                    VideoBox.Invoke((MethodInvoker)SetImage);
                else
                    SetImage();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                displayFrame.Dispose();
            }
        }

        private void UpdateTrackingListColors()
        {
            for (int i = 0; i < _trackingRects.Count; i++)
            {
                var tracking = _trackingRects[i];
                TrackingListBox.Items[i] = tracking.IsAnalyzed
                    ? $"1 {tracking.Name}"
                    : $"0 {tracking.Name}";
            }
        }

        private void FrameSlider_SizeChanged(object sender, EventArgs e)
        {
            UpdateTrackRangeIndicatorPosition();
        }

        private void UpdateTrackRangeIndicatorPosition()
        {
            if (trackRangeIndicator == null || FrameSlider == null) return;

            // Position the indicator just above the slider thumb area
            // The slider track area typically starts at about 10px from left and ends about 10px from right
            int sliderLeft = FrameSlider.Left + 25;
            int sliderWidth = FrameSlider.Width - 50;
            int sliderTop = FrameSlider.Top + FrameSlider.Height - 5; // Position above the slider thumb

            trackRangeIndicator.Location = new System.Drawing.Point(sliderLeft, sliderTop);
            trackRangeIndicator.Width = sliderWidth;
        }

        #endregion

        #endregion
    }
}

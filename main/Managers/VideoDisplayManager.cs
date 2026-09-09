using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Windows.Forms;

namespace ShutterFace
{
    /// <summary>Manages video frame display and tracking visualization on the form.</summary>
    public class VideoDisplayManager
    {
        private readonly PictureBox _videoBox;
        private readonly TrackBar _frameSlider;
        private readonly Label? _trackRangeIndicator;

        public VideoDisplayManager(PictureBox videoBox, TrackBar frameSlider, Label? trackRangeIndicator)
        {
            _videoBox = videoBox;
            _frameSlider = frameSlider;
            _trackRangeIndicator = trackRangeIndicator;
        }

        public void DisplayFrame(Form form, Mat frame, IReadOnlyList<TrackingRect> trackingRects, int currentFrameIndex, int selectedTrackingIndex)
        {
            if (frame == null) return;

            using Mat displayFrame = frame.Clone();

            try
            {
                for (int i = 0; i < trackingRects.Count; i++)
                {
                    var tracking = trackingRects[i];
                    if (currentFrameIndex >= tracking.StartFrame && currentFrameIndex <= tracking.EndFrame)
                    {
                        Rect? rect = tracking.GetRectAtFrame(currentFrameIndex);

                        if (!rect.HasValue)
                            rect = tracking.InitialRect;

                        if (rect.HasValue)
                        {
                            Scalar color;
                            if (!tracking.IsAnalyzed)
                                color = Scalar.Red;
                            else
                                color = (i == selectedTrackingIndex) ? Scalar.LimeGreen : Scalar.Gray;

                            int thickness = (i == selectedTrackingIndex) ? 3 : 1;

                            Cv2.Rectangle(displayFrame, rect.Value, color, thickness);
                            Cv2.PutText(displayFrame, tracking.Name,
                                new OpenCvSharp.Point(rect.Value.X, rect.Value.Y - 10),
                                HersheyFonts.HersheySimplex, 0.5, color, 1);

                            if (i == selectedTrackingIndex)
                            {
                                RectGeometry.DrawResizeHandles(displayFrame, rect.Value);
                            }
                        }
                    }
                }

                using Bitmap bitmap = BitmapConverter.ToBitmap(displayFrame);

                void SetImage()
                {
                    _videoBox.Image?.Dispose();
                    _videoBox.Image = bitmap;
                }

                if (_videoBox.InvokeRequired)
                    _videoBox.Invoke((MethodInvoker)SetImage);
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

        public static void UpdateListBoxColors(ListBox listBox, IReadOnlyList<TrackingRect> trackingRects)
        {
            for (int i = 0; i < trackingRects.Count; i++)
            {
                var tracking = trackingRects[i];
                listBox.Items[i] = tracking.IsAnalyzed
                    ? $"1 {tracking.Name}"
                    : $"0 {tracking.Name}";
            }
        }

        public void UpdateRangeIndicatorPosition()
        {
            if (_trackRangeIndicator == null || _frameSlider == null) return;

            int sliderLeft = _frameSlider.Left + 25;
            int sliderWidth = _frameSlider.Width - 50;
            int sliderTop = _frameSlider.Top + _frameSlider.Height - 5;

            _trackRangeIndicator.Location = new System.Drawing.Point(sliderLeft, sliderTop);
            _trackRangeIndicator.Width = sliderWidth;
        }
    }
}

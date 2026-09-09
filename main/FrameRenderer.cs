using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace ShutterFace
{
    /// <summary>
    /// Draws tracking rectangles and resize handles onto a frame and pushes
    /// the result into the PictureBox (thread-safe).
    /// </summary>
    internal sealed class FrameRenderer(PlayerModel model)
    {
        /// <summary>The PictureBox to draw into. Set by the form.</summary>
        public PictureBox? Target;

        public void RenderTo(Mat frame, int? selectedTrackingIndex)
        {
            if (frame == null || Target == null) return;

            using Mat displayFrame = frame.Clone();

            try
            {
                for (int i = 0; i < model.TrackingRects.Count; i++)
                {
                    var tracking = model.TrackingRects[i];
                    if (model.CurrentFrameIndex < tracking.StartFrame || model.CurrentFrameIndex > tracking.EndFrame)
                        continue;

                    Rect? rect = tracking.GetRectAtFrame(model.CurrentFrameIndex) ?? tracking.InitialRect;
                    if (!rect.HasValue)
                        continue;

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

                Bitmap bitmap = BitmapConverter.ToBitmap(displayFrame);

                void SetImage()
                {
                    Target.Image?.Dispose();
                    Target.Image = bitmap;
                }

                if (Target.InvokeRequired)
                    Target.Invoke((MethodInvoker)SetImage);
                else
                    SetImage();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// Calculates the actual video display area within the PictureBox
        /// accounting for aspect ratio preservation (Zoom mode).
        /// </summary>
        public (Rectangle videoRect, float scaleX, float scaleY) GetVideoDisplayArea()
        {
            if (model.CurrentFrame == null || Target == null || Target.Width == 0 || Target.Height == 0)
                return (Rectangle.Empty, 1f, 1f);

            int pbWidth = Target.Width;
            int pbHeight = Target.Height;

            int videoWidth = model.CurrentFrame.Width;
            int videoHeight = model.CurrentFrame.Height;

            float pbAspect = (float)pbWidth / pbHeight;
            float videoAspect = (float)videoWidth / videoHeight;

            int displayWidth, displayHeight;
            int offsetX = 0, offsetY = 0;

            if (pbAspect > videoAspect)
            {
                displayHeight = pbHeight;
                displayWidth = (int)(pbHeight * videoAspect);
                offsetX = (pbWidth - displayWidth) / 2;
            }
            else
            {
                displayWidth = pbWidth;
                displayHeight = (int)(pbWidth / videoAspect);
                offsetY = (pbHeight - displayHeight) / 2;
            }

            float scaleX = (float)videoWidth / displayWidth;
            float scaleY = (float)videoHeight / displayHeight;

            return (new Rectangle(offsetX, offsetY, displayWidth, displayHeight), scaleX, scaleY);
        }

        public static string GetTrackingListText(TrackingRect tracking)
        {
            return (tracking.IsAnalyzed ? "1 " : "0 ") + tracking.Name;
        }
    }
}

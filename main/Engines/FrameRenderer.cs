using OpenCvSharp;
using OpenCvSharp.Extensions;
using ShutterFace.DataObjects;

namespace ShutterFace.Engines
{
    /// <summary>
    /// Draws tracking rectangles and resize handles onto a frame and pushes
    /// the result into the PictureBox (thread-safe).
    /// </summary>
    internal sealed class FrameRenderer(TrackerState model)
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

                    // Clamp to frame bounds so OpenCvSharp doesn't receive malformed rects.
                    Rect clamped = TrackerBox.GetClampedRect(rect.Value, model.CurrentFrame.Width, model.CurrentFrame.Height);
                    if (clamped.Width <= 0 || clamped.Height <= 0)
                        continue;

                    Scalar color;
                    if (!tracking.IsAnalyzed)
                        color = Scalar.Red;
                    else
                        color = i == selectedTrackingIndex ? Scalar.LimeGreen : Scalar.Gray;

                    int thickness = i == selectedTrackingIndex ? 3 : 1;

                    Cv2.Rectangle(displayFrame, clamped, color, thickness);
                    Cv2.PutText(displayFrame, tracking.Name, new OpenCvSharp.Point(clamped.X, clamped.Y - 10), HersheyFonts.HersheySimplex, 0.5, color, 1);

                    if (i == selectedTrackingIndex)
                    {
                        TrackerGeometry.DrawResizeHandles(displayFrame, clamped);
                    }
                }

                Bitmap bitmap = displayFrame.ToBitmap();

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
            catch (Exception e) when (e is not InvalidOperationException)
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
                if (Math.Abs(videoAspect - 1f) < 0.01f)
                    offsetY = (pbHeight - displayHeight) / 2;
            }
            else
            {
                displayWidth = pbWidth;
                displayHeight = (int)(pbWidth / videoAspect);
                offsetY = (pbHeight - displayHeight) / 2;
                if (pbAspect == videoAspect) offsetX = 0;
            }

            if (offsetX < 0) offsetX = 0;
            if (offsetY < 0) offsetY = 0;

            float scaleX = (float)videoWidth / displayWidth;
            float scaleY = (float)videoHeight / displayHeight;

            return (new Rectangle(offsetX, offsetY, displayWidth, displayHeight), scaleX, scaleY);
        }
    }
}

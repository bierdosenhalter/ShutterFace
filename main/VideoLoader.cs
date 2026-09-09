using OpenCvSharp;
using System.Globalization;

namespace MotionTrackerFaceBlur
{
    /// <summary>
    /// Video loading, frame access and time formatting.
    /// All state lives in PlayerModel; UI feedback is reported through callbacks.
    /// </summary>
    internal sealed class VideoLoader(PlayerModel model)
    {
        /// <summary>Shows a message on the UI thread. Set by the form.</summary>
        public Action<string>? ShowMessage;

        /// <summary>Runs an action on the UI thread. Set by the form.</summary>
        public Action<Action>? RunOnUi;

        /// <summary>Renders the current frame. Set by the form (thread-safe renderer).</summary>
        public Action<Mat?>? RenderFrame;

        public static bool IsVideoFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            string ext = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);
            return ext is ".mp4" or ".avi" or ".mov" or ".wmv" or ".mkv" or ".flv" or ".webm";
        }

        /// <summary>
        /// Opens the video and prepares the slider. Returns false when the file
        /// could not be opened or contains no frames.
        /// </summary>
        public bool OpenVideo(string path, out int totalFrames)
        {
            totalFrames = 0;
            model.VideoCapture?.Dispose();
            model.VideoCapture = new VideoCapture(path);
            model.VideoPath = path;

            if (!model.VideoCapture.IsOpened())
            {
                ShowMessage?.Invoke("Failed to open video file.");
                return false;
            }

            totalFrames = (int)model.VideoCapture.FrameCount;
            if (totalFrames <= 0)
            {
                ShowMessage?.Invoke("No frames could be read from this video file.");
                model.VideoCapture.Dispose();
                model.VideoCapture = null!;
                model.CurrentFrame?.Dispose();
                model.CurrentFrame = null!;
                return false;
            }

            model.TotalFrames = totalFrames;
            return true;
        }

        public bool IsOpen => model.VideoCapture != null && model.VideoCapture.IsOpened();

        /// <summary>Seeks and reads the given frame into CurrentFrame (thread-safe).
        /// Rendering and time display run outside the lock so the UI thread can
        /// never block on it.</summary>
        public void LoadFrame(int frameIndex, bool display = true)
        {
            bool frameRead = false;

            lock (model.VideoLock)
            {
                if (model.VideoCapture == null || !model.VideoCapture.IsOpened())
                    return;

                model.VideoCapture.Set(VideoCaptureProperties.PosFrames, frameIndex);

                model.CurrentFrame?.Dispose();
                model.CurrentFrame = new Mat();

                if (model.VideoCapture.Read(model.CurrentFrame))
                {
                    model.CurrentFrameIndex = frameIndex;
                    frameRead = true;
                }
                else
                {
                    model.CurrentFrame.Dispose();
                    model.CurrentFrame = null!;
                }
            }

            if (frameRead && display)
            {
                RenderFrame?.Invoke(model.CurrentFrame);
                RefreshTimeDisplay();
            }
        }

        /// <summary>Updates the UI time labels. Safe to call from any thread.</summary>
        public void RefreshTimeDisplay()
        {
            if (model.VideoCapture == null) return;

            double fps = GetFps();

            TimeSpan startTime = TimeSpan.FromSeconds(0);
            TimeSpan currentTime = TimeSpan.FromSeconds(model.CurrentFrameIndex / fps);
            TimeSpan endTime = TimeSpan.FromSeconds(model.TotalFrames / fps);

            RunOnUi?.Invoke(() =>
            {
                UpdateTimeLabels?.Invoke(startTime, currentTime, endTime);
            });
        }

        /// <summary>The labels are assigned by the form via this callback.</summary>
        public Action<TimeSpan, TimeSpan, TimeSpan>? UpdateTimeLabels;

        public double GetFps() => model.VideoCapture != null && model.VideoCapture.Fps > 0 ? model.VideoCapture.Fps : 30;
    }
}

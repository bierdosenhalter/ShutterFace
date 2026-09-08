using OpenCvSharp;
using System.Globalization;
using System.Windows.Forms;

namespace MotionTrackerFaceBlur
{
    /// <summary>Handles video file loading, frame extraction, and time display updates.</summary>
    public class VideoLoader
    {
        private readonly Form _form;
        private readonly TrackBar _frameSlider;
        private readonly Label _currentStatusLabel;

        private readonly VideoCapture? _videoCaptureRef; // only for reading FPS
        private int _totalFrames;
        private int _currentFrameIndex;
        private Mat? _currentFrame;

        public VideoLoader(Form form, TrackBar frameSlider, Label currentStatusLabel)
        {
            _form = form;
            _frameSlider = frameSlider;
            _currentStatusLabel = currentStatusLabel;
            _videoCaptureRef = null; // Will be set before use
        }

        public bool IsVideoFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            string ext = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);
            return ext is ".mp4" or ".avi" or ".mov" or ".wmv" or ".mkv" or ".flv" or ".webm";
        }

        public bool LoadVideo(Form form, ref VideoCapture? videoCapture, string videoPath, ref Mat? currentFrame, TrackBar frameSlider, Label statusLabel, int totalFrames, int currentFrameIndex, Action updateTrackingProperties, bool displayFrame)
        {
            videoCapture?.Dispose();
            videoCapture = new VideoCapture(videoPath);

            if (!videoCapture.IsOpened())
            {
                form.MessageBoxShow("Failed to open video file.");
                return false;
            }

            int totalFramesCount = (int)videoCapture.FrameCount;
            if (totalFramesCount <= 0)
            {
                form.MessageBoxShow("No frames could be read from this video file.");
                videoCapture.Dispose();
                videoCapture = null!;
                currentFrame?.Dispose();
                currentFrame = null!;
                return false;
            }

            frameSlider.Maximum = totalFramesCount - 1;
            frameSlider.Value = 0;

            currentFrameIndex = 0;
            LoadFrameCore(ref videoCapture, ref currentFrame, 0, displayFrame, statusLabel);

            UpdateTimeDisplayCore(videoCapture, totalFramesCount, currentFrameIndex, statusLabel);

            return true;
        }

        public void HandleSliderScroll(Form form, bool isAnalyzing, bool isExporting, TrackBar frameSlider, ref VideoCapture? videoCapture, ref Mat? currentFrame, int currentFrameIndex, Action loadFrame, Action displayFrame)
        {
            if (isAnalyzing || isExporting) return;

            loadFrame();

            if (currentFrame != null)
                displayFrame();
        }

        private void LoadFrameCore(ref VideoCapture? videoCapture, ref Mat? currentFrame, int frameIndex, bool display, Label statusLabel)
        {
            lock (new object())
            {
                if (videoCapture == null || !videoCapture.IsOpened())
                    return;

                videoCapture.Set(VideoCaptureProperties.PosFrames, frameIndex);

                currentFrame?.Dispose();
                currentFrame = new Mat();

                if (videoCapture.Read(currentFrame) && !currentFrame.Empty())
                {
                    if (display)
                        UpdateTimeDisplayCore(videoCapture, 0, frameIndex, statusLabel);
                }
            }
        }

        private void UpdateTimeDisplayCore(VideoCapture videoCapture, int totalFramesCount, int currentFrameIndex, Label currentStatusLabel)
        {
            if (videoCapture == null) return;

            double fps = videoCapture.Fps;
            if (fps <= 0) fps = 30;

            TimeSpan startTime = TimeSpan.FromSeconds(0);
            TimeSpan currentTime = TimeSpan.FromSeconds(currentFrameIndex / fps);
            TimeSpan endTime = TimeSpan.FromSeconds(totalFramesCount / fps);

            currentStatusLabel.Invoke((MethodInvoker)delegate
            {
                try
                {
                    currentStatusLabel.Text = $"Frame: {currentFrameIndex}/{totalFramesCount} | Time: {currentTime:hh\\:mm\\:ss} | FPS: {videoCapture.Fps}";
                }
                catch { }
            });
        }

        public (VideoCapture? videoCapture, Mat? currentFrame, int totalFrames, int currentFrameIndex) State =>
            (_videoCaptureRef, _currentFrame, _totalFrames, _currentFrameIndex);
    }
}

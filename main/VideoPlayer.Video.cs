using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Globalization;
using System.Diagnostics;
using System.Text.Json;

namespace MotionTrackerFaceBlur
{
    public partial class VideoPlayer : Form, IDisposable
    {
        #region Video Loading & Frame Management

        #region Video Loading & Frame Management

        private void OpenVideoBtn_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Video Files|*.mp4;*.avi;*.mov;*.wmv;*.mkv;*.flv;*.webm|All Files|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _videoPath = openFileDialog.FileName;
                LoadVideo();
            }
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            if (drgevent.Data?.GetData(DataFormats.FileDrop) is string[] files)
            {
                if (files.Any(f => IsVideoFile(f)))
                    drgevent.Effect = DragDropEffects.Copy;
            }
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);

            if (drgevent.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                string videoFile = files.FirstOrDefault(f => IsVideoFile(f)) ?? string.Empty;
                if (!string.IsNullOrEmpty(videoFile))
                    LoadVideoFromPath(videoFile);
            }
        }

        private static bool IsVideoFile(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            string ext = Path.GetExtension(path).ToLower(CultureInfo.InvariantCulture);
            return ext is ".mp4" or ".avi" or ".mov" or ".wmv" or ".mkv" or ".flv" or ".webm";
        }

        internal void LoadVideoFromPath(string path)
        {
            _videoPath = path;
            LoadVideo();
        }

        private void LoadVideo()
        {
            _videoCapture?.Dispose();
            _videoCapture = new VideoCapture(_videoPath);

            if (!_videoCapture.IsOpened())
            {
                MessageBox.Show("Failed to open video file.");
                return;
            }

            _totalFrames = (int)_videoCapture.FrameCount;

            if (_totalFrames <= 0)
            {
                MessageBox.Show("No frames could be read from this video file.");
                _videoCapture.Dispose();
                _videoCapture = null!;
                _currentFrame?.Dispose();
                _currentFrame = null!;
                return;
            }

            FrameSlider.Maximum = _totalFrames - 1;
            FrameSlider.Value = 0;

            _currentFrameIndex = 0;
            LoadFrame(0);

            AddTrackingBtn.Enabled = true;
            ExportVideoBtn.Enabled = true;

            UpdateTimeDisplay();
        }

        private void LoadFrame(int frameIndex, bool display = true)
        {
            lock (videoLock)
            {
                if (_videoCapture == null || !_videoCapture.IsOpened())
                    return;

                _videoCapture.Set(VideoCaptureProperties.PosFrames, frameIndex);

                // Dispose the old frame before creating a new one
                _currentFrame?.Dispose();
                _currentFrame = new Mat();

                if (_videoCapture.Read(_currentFrame))
                {
                    if (display)
                        DisplayFrame(_currentFrame);
                    _currentFrameIndex = frameIndex;
                    if (display)
                        UpdateTimeDisplay();
                }
            }
        }

        private void UpdateTimeDisplay()
        {
            if (_videoCapture == null) return;

            double fps = _videoCapture.Fps;
            if (fps <= 0) fps = 30;

            TimeSpan startTime = TimeSpan.FromSeconds(0);
            TimeSpan currentTime = TimeSpan.FromSeconds(_currentFrameIndex / fps);
            TimeSpan endTime = TimeSpan.FromSeconds(_totalFrames / fps);

            Invoke((MethodInvoker)delegate
            {
                try
                {
                    lblStartTime.Text = startTime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                    lblCurrentTime.Text = currentTime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                    lblEndTime.Text = endTime.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                    tssStatusLabel.Text = $"Frame: {_currentFrameIndex}/{_totalFrames} | Time: {currentTime:hh\\:mm\\:ss} | FPS: {_videoCapture.Fps}";
                }
                catch { }
            });
        }

        private void FrameSlider_Scroll(object sender, EventArgs e)
        {
            if (_isAnalyzing || _isExporting) return;
            LoadFrame(FrameSlider.Value);

            if (_currentFrame != null)
                DisplayFrame(_currentFrame);
        }

        #endregion

        #endregion
    }
}

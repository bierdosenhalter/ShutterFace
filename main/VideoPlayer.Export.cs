using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Globalization;
using System.Diagnostics;
using System.Text.Json;

namespace MotionTrackerFaceBlur
{
    public partial class VideoPlayer : Form, IDisposable
    {
        #region Export

        #region Export

        private void ExportVideoBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_videoPath)) return;

            var unanalyzedTracks = _trackingRects.Where(t => !t.IsAnalyzed).ToList();
            if (unanalyzedTracks.Count > 0)
            {
                var result = MessageBox.Show(
                    $"There are {unanalyzedTracks.Count} unanalyzed tracking(s). Do you want to analyze them before exporting?",
                    "Unanalyzed Tracks",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    AnalyzeAllUnanalyzedTracks();
                    return;
                }
                else if (result == DialogResult.Cancel)
                {
                    return;
                }
            }

            using SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "Video Files|*.mp4|All Files|*.*";
            saveFileDialog.DefaultExt = "mp4";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _isExporting = true;
                ExportVideoBtn.Enabled = false;

                Task.Run(() => ExportVideo(saveFileDialog.FileName));
            }
        }

        private void AnalyzeAllUnanalyzedTracks()
        {
            var unanalyzedTracks = _trackingRects.Where(t => !t.IsAnalyzed).ToList();

            Task.Run(() =>
            {
                foreach (var tracking in unanalyzedTracks)
                {
                    tracking.ClearPositions();
                    _isAnalyzing = true;

                    Invoke((MethodInvoker)delegate
                    {
                        AnalyzeBtn.Visible = false;
                        StopAnalyzeBtn.Visible = true;
                        AddTrackingBtn.Enabled = false;
                        DeleteTrackingBtn.Enabled = false;
                        FrameSlider.Enabled = false;
                    });

                    PerformAnalysis(tracking);
                }

                Invoke((MethodInvoker)delegate
                {
                    _isAnalyzing = false;
                    AnalyzeBtn.Visible = true;
                    StopAnalyzeBtn.Visible = false;
                    AddTrackingBtn.Enabled = true;
                    DeleteTrackingBtn.Enabled = true;
                    FrameSlider.Enabled = true;
                });
            });
        }

        /// <summary>
        /// Exports the video with pixelated tracking regions.
        /// Attempts to preserve original video codec settings.
        /// </summary>
        private void ExportVideo(string outputPath)
        {
            try
            {
                int fps = (int)_videoCapture.Fps;
                int width = _currentFrame.Width;
                int height = _currentFrame.Height;

                int fourCC = (int)_videoCapture.Get(VideoCaptureProperties.FourCC);

                if (fourCC == 0)
                {
                    string extension = Path.GetExtension(_videoPath).ToLower(CultureInfo.InvariantCulture);
                    fourCC = extension switch
                    {
                        ".mp4" => FourCC.MP4V,
                        ".avi" => FourCC.MP42,
                        ".wmv" => FourCC.WMV1,
                        _ => FourCC.MP4V
                    };
                }

                _videoWriter = new VideoWriter(outputPath,
                    fourCC, fps, new OpenCvSharp.Size(width, height));

                Invoke((MethodInvoker)delegate
                {
                    Text = "Exporting video...";
                    tssStatusLabel.Text = "Export: 0%";
                    tsspProgressBar.Visible = true;
                    tsspProgressBar.Value = 0;
                    tsspProgressBar.Maximum = _totalFrames;
                });

                for (int i = 0; i < _totalFrames && _isExporting; i++)
                {
                    lock (videoLock)
                    {
                        if (_videoCapture == null || !_videoCapture.IsOpened())
                            break;

                        _videoCapture.Set(VideoCaptureProperties.PosFrames, i);
                        _currentFrame?.Dispose();
                        _currentFrame = new Mat();
                        if (!_videoCapture.Read(_currentFrame))
                            break;

                        _currentFrameIndex = i;
                    }

                    using Mat exportFrame = _currentFrame.Clone();

                    foreach (var tracking in _trackingRects)
                    {
                        if (i >= tracking.StartFrame && i <= tracking.EndFrame)
                        {
                            Rect? rect = tracking.GetRectAtFrame(i);

                            if (!rect.HasValue)
                                rect = tracking.InitialRect;

                            if (rect.HasValue)
                            {
                                PixelateRegion(exportFrame, rect.Value);
                            }
                        }
                    }

                    _videoWriter.Write(exportFrame);
                    exportFrame.Dispose();

                    if (i % Math.Max(1, Math.Min(_totalFrames / 20, 100)) == 0)
                    {
                        int frameIndex = i;
                        Invoke((MethodInvoker)delegate
                        {
                            int progress = (frameIndex * 100 / _totalFrames);
                            tssStatusLabel.Text = $"Export: {progress}%";
                            tsspProgressBar.Value = frameIndex;
                        });
                    }
                }

                Invoke((MethodInvoker)delegate
                {
                    tsspProgressBar.Value = tsspProgressBar.Maximum;
                    tssStatusLabel.Text = "Export: 100%";
                    MessageBox.Show("Video exported successfully!");
                    Text = "Motion Tracker";
                    ExportVideoBtn.Enabled = true;
                    _isExporting = false;
                    Thread.Sleep(500);
                    tsspProgressBar.Visible = false;
                    tssStatusLabel.Text = "Export complete";
                });
            }
            catch (Exception ex)
            {
                Invoke((MethodInvoker)delegate
                {
                    tsspProgressBar.Visible = false;
                    MessageBox.Show($"Error exporting video: {ex.Message}");
                    ExportVideoBtn.Enabled = true;
                    _isExporting = false;
                    tssStatusLabel.Text = "Export failed";
                });
            }
            finally
            {
                _videoWriter?.Dispose();
            }
        }

        /// <summary>
        /// Applies pixelation effect to a region of the image by downscaling and upscaling.
        /// Ensures the long side of the region has approximately 16 big pixels for consistent blurring.
        /// </summary>
        private void PixelateRegion(Mat image, Rect region)
        {
            Rect safeRegion = new(
                Math.Max(0, region.X),
                Math.Max(0, region.Y),
                Math.Max(0, Math.Min(region.Width, image.Width - Math.Max(region.X, 0))),
                Math.Max(0, Math.Min(region.Height, image.Height - Math.Max(region.Y, 0)))
            );

            if (safeRegion.Width <= 0 || safeRegion.Height <= 0)
                return;

            Mat roi = new(image, safeRegion);

            int longestSide = Math.Max(safeRegion.Width, safeRegion.Height);
            int numberOfBlocks = _bigPixels;

            int dynamicBlockSize = Math.Max(1, longestSide / numberOfBlocks);

            dynamicBlockSize = Math.Max(dynamicBlockSize, 4);

            int smallWidth = Math.Max(1, safeRegion.Width / dynamicBlockSize);
            int smallHeight = Math.Max(1, safeRegion.Height / dynamicBlockSize);

            Mat small = new();
            Cv2.Resize(roi, small, new OpenCvSharp.Size(smallWidth, smallHeight),
                0, 0, InterpolationFlags.Linear);

            Mat pixelated = new();
            Cv2.Resize(small, pixelated, new OpenCvSharp.Size(safeRegion.Width, safeRegion.Height),
                0, 0, InterpolationFlags.Nearest);

            pixelated.CopyTo(roi);

            roi.Dispose();
            small.Dispose();
            pixelated.Dispose();
        }

        #endregion

        #endregion
    }
}

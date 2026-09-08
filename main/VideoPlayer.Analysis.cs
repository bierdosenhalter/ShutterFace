using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Globalization;
using System.Diagnostics;
using System.Text.Json;

namespace MotionTrackerFaceBlur
{
    public partial class VideoPlayer : Form, IDisposable
    {
        #region Analysis

        #region Analysis

        private void AnalyzeBtn_Click(object sender, EventArgs e)
        {
            if (_selectedTrackingIndex == null)
            {
                MessageBox.Show("Please select a tracking point first.");
                return;
            }

            var tracking = _trackingRects[_selectedTrackingIndex.Value];
            tracking.ClearPositions();

            _isAnalyzing = true;
            AnalyzeBtn.Visible = false;
            StopAnalyzeBtn.Visible = true;
            AddTrackingBtn.Enabled = false;
            DeleteTrackingBtn.Enabled = false;
            FrameSlider.Enabled = false;

            Task.Run(() => PerformAnalysis(tracking));
        }

        private void StopAnalyzeBtn_Click(object sender, EventArgs e)
        {
            _isAnalyzing = false;
            AnalyzeBtn.Visible = true;
            StopAnalyzeBtn.Visible = false;
            AddTrackingBtn.Enabled = true;
            DeleteTrackingBtn.Enabled = true;
            FrameSlider.Enabled = true;
        }

        /// <summary>
        /// Performs template matching analysis on the selected tracking rectangle
        /// across its frame range. Updates status periodically for user feedback.
        /// </summary>
        private void PerformAnalysis(TrackingRect tracking)
        {
            int startFrame = tracking.StartFrame;
            tracking.PreviousRect = tracking.InitialRect;
            tracking.AddFramePosition(startFrame, tracking.InitialRect);

            Invoke((MethodInvoker)delegate
            {
                tssStatusLabel.Text = $"Analyzing: {tracking.Name}...";
                tsspProgressBar.Visible = true;
                tsspProgressBar.Value = 0;
                tsspProgressBar.Maximum = Math.Max(1, tracking.EndFrame - startFrame);
            });

            try
            {
                for (int i = startFrame + 1; i <= tracking.EndFrame && _isAnalyzing; i++)
                {
                    lock (videoLock)
                    {
                        LoadFrame(i, i % 10 == 0);
                        if (!TrackObject(tracking, i))
                        {
                            tracking.EndFrame = i - 1;
                            Invoke((MethodInvoker)delegate
                            {
                                tssStatusLabel.Text = $"Analysis stopped at frame {i} - object lost";
                            });
                            break;
                        }
                    }

                    if (i % Math.Max(1, (tracking.EndFrame - startFrame) / 5) == 0)
                    {
                        int frameIndex = i;
                        Invoke((MethodInvoker)delegate
                        {
                            int progress = ((frameIndex - startFrame) * 100 / (tracking.EndFrame - startFrame));
                            tssStatusLabel.Text = $"Analyzing: {tracking.Name} - {progress}% complete";
                            tsspProgressBar.Value = frameIndex - startFrame;
                        });
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            Invoke((MethodInvoker)delegate
            {
                tracking.IsAnalyzed = true;
                _isAnalyzing = false;
                AnalyzeBtn.Visible = true;
                StopAnalyzeBtn.Visible = false;
                AddTrackingBtn.Enabled = true;
                DeleteTrackingBtn.Enabled = true;
                FrameSlider.Enabled = true;
                UpdateTrackingListColors();
                tsspProgressBar.Value = tsspProgressBar.Maximum;
                tssStatusLabel.Text = $"Analyzing: {tracking.Name} - 100% complete";

                LoadFrame(tracking.StartFrame);
                FrameSlider.Value = tracking.StartFrame;

                Thread.Sleep(500);
                tsspProgressBar.Visible = false;
                tssStatusLabel.Text = $"Analysis complete for {tracking.Name}";
                UpdateTimeDisplay();
                DisplayFrame(_currentFrame);
                statusStripBottom.Text = $"Analysis complete for {tracking.Name}";
            });
        }

        /// <summary>
        /// Tracks object using template matching between consecutive frames.
        /// Returns false if tracking confidence is too low or object moves out of bounds.
        /// </summary>
        private bool TrackObject(TrackingRect tracking, int frameIndex)
        {
            if (tracking.PreviousRect == null) return false;

            Mat previousFrame = new();
            _videoCapture.Set(VideoCaptureProperties.PosFrames, Math.Max(0, frameIndex - 1));
            _videoCapture.Read(previousFrame);

            if (previousFrame == null) return false;

            Mat template = new(previousFrame, tracking.PreviousRect.Value);
            Mat result = new();

            try
            {
                Cv2.MatchTemplate(_currentFrame, template, result, TemplateMatchModes.CCoeffNormed);
                Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

                if (maxVal > 0.3)
                {
                    var newRect = new Rect(
                        maxLoc.X,
                        maxLoc.Y,
                        tracking.InitialRect.Width,
                        tracking.InitialRect.Height
                    );

                    bool edgeInBounds = IsEdgeInBounds(newRect, tracking.PreviousRect.Value);

                    if (!edgeInBounds)
                    {
                        return false;
                    }

                    tracking.PreviousRect = newRect;
                    tracking.AddFramePosition(frameIndex, newRect);
                }
                else
                {
                    return false;
                }

                return true;
            }
            finally
            {
                template?.Dispose();
                result?.Dispose();
                previousFrame?.Dispose();
            }
        }

        /// <summary>
        /// Checks if at least one corner of the new rectangle overlaps with the previous rectangle.
        /// This prevents the tracker from jumping to unrelated areas.
        /// </summary>
        private static bool IsEdgeInBounds(Rect newRect, Rect previousRect)
        {
            OpenCvSharp.Point[] newCorners =
            [
                new OpenCvSharp.Point(newRect.X, newRect.Y),
                new OpenCvSharp.Point(newRect.X + newRect.Width, newRect.Y),
                new OpenCvSharp.Point(newRect.X, newRect.Y + newRect.Height),
                new OpenCvSharp.Point(newRect.X + newRect.Width, newRect.Y + newRect.Height)
            ];

            foreach (var corner in newCorners)
            {
                if (corner.X >= previousRect.X && corner.X <= previousRect.X + previousRect.Width &&
                    corner.Y >= previousRect.Y && corner.Y <= previousRect.Y + previousRect.Height)
                {
                    return true;
                }
            }

            OpenCvSharp.Point[] prevCorners =
            [
                new OpenCvSharp.Point(previousRect.X, previousRect.Y),
                new OpenCvSharp.Point(previousRect.X + previousRect.Width, previousRect.Y),
                new OpenCvSharp.Point(previousRect.X, previousRect.Y + previousRect.Height),
                new OpenCvSharp.Point(previousRect.X + previousRect.Width, previousRect.Y + previousRect.Height)
            ];

            foreach (var corner in prevCorners)
            {
                if (corner.X >= newRect.X && corner.X <= newRect.X + newRect.Width &&
                    corner.Y >= newRect.Y && corner.Y <= newRect.Y + newRect.Height)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #endregion
    }
}

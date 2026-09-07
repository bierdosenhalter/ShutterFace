## Program.cs 
 
```csharp 
namespace MotionTrackerFaceBlur
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new VideoPlayer());
        }
    }
} 
``` 
 
 
## RectJsonConverter.cs 
 
```csharp 
﻿using OpenCvSharp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MotionTrackerFaceBlur
{
    // Custom JSON converter for OpenCvSharp.Rect
    public class RectJsonConverter : JsonConverter<Rect>
    {
        public override Rect Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
        {
            if (reader.TokenType != System.Text.Json.JsonTokenType.StartObject)
                throw new System.Text.Json.JsonException();

            int x = 0, y = 0, width = 0, height = 0;

            while (reader.Read())
            {
                if (reader.TokenType == System.Text.Json.JsonTokenType.EndObject)
                    return new Rect(x, y, width, height);

                if (reader.TokenType == System.Text.Json.JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString()!;
                    reader.Read();

                    switch (propertyName.ToLower())
                    {
                        case "x":
                        case "left":
                            x = reader.GetInt32();
                            break;
                        case "y":
                        case "top":
                            y = reader.GetInt32();
                            break;
                        case "width":
                        case "size":
                            // Handle both "Width" and "Size" property names
                            if (propertyName.Equals("width", StringComparison.CurrentCultureIgnoreCase))
                                width = reader.GetInt32();
                            else if (propertyName.Equals("size", StringComparison.CurrentCultureIgnoreCase))
                            {
                                // Size is an object, skip it
                                reader.Skip();
                            }
                            break;
                        case "height":
                            height = reader.GetInt32();
                            break;
                        case "location":
                        case "topleft":
                        case "bottomright":
                            // Skip these complex objects
                            reader.Skip();
                            break;
                    }
                }
            }

            return new Rect(x, y, width, height);
        }

        public override void Write(Utf8JsonWriter writer, Rect value, System.Text.Json.JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("X", value.X);
            writer.WriteNumber("Y", value.Y);
            writer.WriteNumber("Width", value.Width);
            writer.WriteNumber("Height", value.Height);
            writer.WriteEndObject();
        }
    }
}
 
``` 
 
 
## TrackingData.cs 
 
```csharp 
﻿namespace MotionTrackerFaceBlur
{
    [Serializable]
    public class TrackingData
    {
        public List<TrackingRect> TrackingRects { get; set; } = new();
        public string VideoPath { get; set; } = string.Empty;
    }
} 
``` 
 
 
## TrackingRect.cs 
 
```csharp 
using OpenCvSharp;

namespace MotionTrackerFaceBlur
{
    public class TrackingRect
    {
        public string Name { get; set; } = null!;
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
        public Rect InitialRect { get; set; }
        public Rect? PreviousRect { get; set; }
        public bool IsAnalyzed { get; set; }

        private readonly Dictionary<int, Rect> framePositions = [];

        // Method to get all frame positions
        public Dictionary<int, Rect> GetRectPositions()
        {
            return new Dictionary<int, Rect>(framePositions);
        }

        // Method to remove a specific frame position
        public void RemoveFramePosition(int frameIndex)
        {
            framePositions.Remove(frameIndex);
        }

        // Method to check if a frame has a tracked position
        public bool HasTrackedPosition(int frameIndex)
        {
            return framePositions.ContainsKey(frameIndex);
        }

        public void AddFramePosition(int frameIndex, Rect rect)
        {
            framePositions[frameIndex] = rect;
        }

        // Add this method
        public void ClearPositions()
        {
            framePositions.Clear();
            PreviousRect = null;
            IsAnalyzed = false;
        }

        public Rect? GetRectAtFrame(int frameIndex)
        {
            if (framePositions.TryGetValue(frameIndex, out Rect value))
                return value;

            // Interpolate if exact frame not found
            var keys = framePositions.Keys.OrderBy(k => k).ToList();
            if (keys.Count == 0) return null;

            if (frameIndex <= keys.First())
                return framePositions[keys.First()];

            if (frameIndex >= keys.Last())
                return framePositions[keys.Last()];

            // Find surrounding frames for interpolation
            int lowerKey = keys.Last(k => k <= frameIndex);
            int upperKey = keys.First(k => k >= frameIndex);

            if (lowerKey == upperKey)
                return framePositions[lowerKey];

            // Linear interpolation
            float t = (float)(frameIndex - lowerKey) / (upperKey - lowerKey);
            var lowerRect = framePositions[lowerKey];
            var upperRect = framePositions[upperKey];

            return new Rect(
                (int)(lowerRect.X + (upperRect.X - lowerRect.X) * t),
                (int)(lowerRect.Y + (upperRect.Y - lowerRect.Y) * t),
                (int)(lowerRect.Width + (upperRect.Width - lowerRect.Width) * t),
                (int)(lowerRect.Height + (upperRect.Height - lowerRect.Height) * t)
            );
        }
    }
}
 
``` 
 
 
## VideoPlayer.cs 
 
```csharp 
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Diagnostics;
using System.Text.Json;

namespace MotionTrackerFaceBlur
{
    public partial class VideoPlayer : Form, IDisposable
    {
        #region Fields

        private VideoCapture _videoCapture = null!;
        private Mat _currentFrame = null!;
        private string _videoPath = string.Empty;
        private VideoWriter _videoWriter = null!;

        private readonly List<TrackingRect> _trackingRects = [];
        private bool _isAnalyzing = false;
        private int _currentFrameIndex = 0;
        private int _totalFrames = 0;
        private bool _isExporting = false;
        private int? _selectedTrackingIndex = null;

        private bool _isDragging = false;
        private OpenCvSharp.Point _dragStartPoint;
        private Rectangle? _dragRectangle;

        private readonly object videoLock = new();

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new RectJsonConverter() }
        };

        #endregion

        #region Constructor & Initialization

        public VideoPlayer()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            AnalyzeBtn.Visible = false;
            StopAnalyzeBtn.Visible = false;
            gprTracking.Enabled = false;
            DeleteTrackingBtn.Enabled = false;

            // This will be updated when the form is resized or slider layout changes
            UpdateTrackRangeIndicatorPosition();

            // Add it to the tableLayoutPanel1, same cell as FrameSlider
            trackRangeIndicator.BringToFront();

            // Handle resize to update indicator position
            this.Resize += (s, e) => UpdateTrackRangeIndicatorPosition();
        }

        #endregion

        #region Video Loading & Frame Management

        private void OpenVideoBtn_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Video Files|*.mp4;*.avi;*.mov;*.wmv;*.flv|All Files|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _videoPath = openFileDialog.FileName;
                LoadVideo();
            }
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
                    lblStartTime.Text = startTime.ToString(@"hh\:mm\:ss");
                    lblCurrentTime.Text = currentTime.ToString(@"hh\:mm\:ss");
                    lblEndTime.Text = endTime.ToString(@"hh\:mm\:ss");

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

        #region Frame Display & Drawing

        private void DisplayFrame(Mat frame)
        {
            if (frame == null) return;

            using Mat displayFrame = frame.Clone();

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
                    }
                }
            }

            if (_dragRectangle.HasValue)
            {
                Cv2.Rectangle(displayFrame, new Rect(_dragRectangle.Value.X, _dragRectangle.Value.Y,
                    _dragRectangle.Value.Width, _dragRectangle.Value.Height), Scalar.Yellow, 2);
            }

            VideoBox.Image?.Dispose();
            VideoBox.Image = BitmapConverter.ToBitmap(displayFrame);
            displayFrame.Dispose();
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

        #region Mouse Interaction & Rectangle Drawing

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (!AddTrackingBtn.Enabled) return;

            var (videoRect, scaleX, scaleY) = GetVideoDisplayArea();

            _dragStartPoint = new OpenCvSharp.Point(
                (int)((e.X - videoRect.X) * scaleX),
                (int)((e.Y - videoRect.Y) * scaleY)
            );

            _isDragging = true;
            _dragRectangle = null;
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !AddTrackingBtn.Enabled) return;

            _isDragging = false;

            if (_dragRectangle.HasValue && _dragRectangle.Value.Width > 5 && _dragRectangle.Value.Height > 5)
            {
                var tracking = new TrackingRect
                {
                    Name = $"Track {_trackingRects.Count + 1}",
                    StartFrame = _currentFrameIndex,
                    EndFrame = _totalFrames - 1,
                    InitialRect = new Rect(
                        _dragRectangle.Value.X,
                        _dragRectangle.Value.Y,
                        _dragRectangle.Value.Width,
                        _dragRectangle.Value.Height
                    )
                };

                _trackingRects.Add(tracking);
                TrackingListBox.Items.Add(tracking.Name);
                _selectedTrackingIndex = _trackingRects.Count - 1;
                TrackingListBox.SelectedIndex = _selectedTrackingIndex.Value;

                UpdateTrackingProperties();
            }

            _dragRectangle = null;
            DisplayFrame(_currentFrame);
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && AddTrackingBtn.Enabled)
            {
                var (videoRect, scaleX, scaleY) = GetVideoDisplayArea();

                int videoX = (int)((e.X - videoRect.X) * scaleX);
                int videoY = (int)((e.Y - videoRect.Y) * scaleY);

                int width = videoX - _dragStartPoint.X;
                int height = videoY - _dragStartPoint.Y;

                _dragRectangle = new Rectangle(
                    Math.Min(_dragStartPoint.X, videoX),
                    Math.Min(_dragStartPoint.Y, videoY),
                    Math.Abs(width),
                    Math.Abs(height)
                );

                DisplayFrame(_currentFrame);
            }
        }

        /// <summary>
        /// Calculates the actual video display area within the PictureBox
        /// accounting for aspect ratio preservation (Zoom mode).
        /// </summary>
        private (Rectangle videoRect, float scaleX, float scaleY) GetVideoDisplayArea()
        {
            if (_currentFrame == null || VideoBox.Width == 0 || VideoBox.Height == 0)
                return (Rectangle.Empty, 1f, 1f);

            int pbWidth = VideoBox.Width;
            int pbHeight = VideoBox.Height;

            int videoWidth = _currentFrame.Width;
            int videoHeight = _currentFrame.Height;

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

            var videoRect = new Rectangle(offsetX, offsetY, displayWidth, displayHeight);

            return (videoRect, scaleX, scaleY);
        }

        /// <summary>
        /// Clamps a rectangle to stay within frame boundaries
        /// </summary>
        private static Rect ClampRectToFrame(Rect rect, int frameWidth, int frameHeight)
        {
            int x = Math.Max(0, Math.Min(rect.X, frameWidth - rect.Width));
            int y = Math.Max(0, Math.Min(rect.Y, frameHeight - rect.Height));
            int width = Math.Min(rect.Width, frameWidth - x);
            int height = Math.Min(rect.Height, frameHeight - y);

            return new Rect(x, y, width, height);
        }

        #endregion

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

                LoadFrame(tracking.StartFrame);
                FrameSlider.Value = tracking.StartFrame;

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
                    if (!_isAnalyzing) break;

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

                    using SaveFileDialog saveFileDialog = new();
                    saveFileDialog.Filter = "Video Files|*.mp4|All Files|*.*";
                    saveFileDialog.DefaultExt = "mp4";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        _isExporting = true;
                        ExportVideoBtn.Enabled = false;
                        Task.Run(() => ExportVideo(saveFileDialog.FileName));
                    }
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
                    string extension = Path.GetExtension(_videoPath).ToLower();
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
                    tssStatusLabel.Text = "Exporting: 0%";
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
                                PixelateRegion(exportFrame, rect.Value, 15);
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
                            tssStatusLabel.Text = $"Exporting: {progress}%";
                        });
                    }
                }

                Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show("Video exported successfully!");
                    Text = "Motion Tracker";
                    ExportVideoBtn.Enabled = true;
                    _isExporting = false;
                    tssStatusLabel.Text = "Export complete";
                });
            }
            catch (Exception ex)
            {
                Invoke((MethodInvoker)delegate
                {
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
        /// </summary>
        private static void PixelateRegion(Mat image, Rect region, int blockSize)
        {
            Rect safeRegion = new(
                Math.Max(0, region.X),
                Math.Max(0, region.Y),
                Math.Min(region.Width, image.Width - region.X),
                Math.Min(region.Height, image.Height - region.Y)
            );

            if (safeRegion.Width <= 0 || safeRegion.Height <= 0)
                return;

            Mat roi = new(image, safeRegion);

            Mat small = new();
            Cv2.Resize(roi, small, new OpenCvSharp.Size(
                Math.Max(1, safeRegion.Width / blockSize),
                Math.Max(11, safeRegion.Height / blockSize)
            ), 0, 0, InterpolationFlags.Linear);

            Mat pixelated = new();
            Cv2.Resize(small, pixelated, new OpenCvSharp.Size(safeRegion.Width, safeRegion.Height),
                0, 0, InterpolationFlags.Nearest);

            pixelated.CopyTo(roi);

            roi.Dispose();
            small.Dispose();
            pixelated.Dispose();
        }

        #endregion

        #region Save/Load Tracking Data

        private void SaveTrackingBtn_Click(object sender, EventArgs e)
        {
            if (_trackingRects.Count == 0)
            {
                MessageBox.Show("No tracking data to save.");
                return;
            }

            using SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = "Tracking Data|*.track|All Files|*.*";
            saveFileDialog.DefaultExt = "track";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var trackingData = new TrackingData
                    {
                        TrackingRects = _trackingRects,
                        VideoPath = _videoPath
                    };

                    var json = System.Text.Json.JsonSerializer.Serialize(trackingData, _jsonSerializerOptions);
                    File.WriteAllText(saveFileDialog.FileName, json);

                    MessageBox.Show("Tracking data saved successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving tracking data: {ex.Message}");
                }
            }
        }

        private void LoadTrackingBtn_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Tracking Data|*.track|All Files|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var json = File.ReadAllText(openFileDialog.FileName);

                    var trackingData = System.Text.Json.JsonSerializer.Deserialize<TrackingData>(json, _jsonSerializerOptions);

                    if (trackingData == null)
                    {
                        MessageBox.Show("Failed to load tracking data.");
                        return;
                    }

                    _trackingRects.Clear();
                    TrackingListBox.Items.Clear();
                    _selectedTrackingIndex = null;
                    gprTracking.Enabled = false;
                    AnalyzeBtn.Visible = false;
                    DeleteTrackingBtn.Enabled = false;

                    foreach (var tracking in trackingData.TrackingRects)
                    {
                        // Reset analysis state for loaded tracks
                        tracking.IsAnalyzed = false;
                        tracking.PreviousRect = null;
                        tracking.ClearPositions();

                        _trackingRects.Add(tracking);
                        TrackingListBox.Items.Add(tracking.Name);
                    }

                    if (!string.IsNullOrEmpty(trackingData.VideoPath) && trackingData.VideoPath != _videoPath)
                    {
                        if (File.Exists(trackingData.VideoPath))
                        {
                            _videoPath = trackingData.VideoPath;
                            LoadVideo();
                        }
                        else
                        {
                            MessageBox.Show("The original video file was not found. Please open the video manually.");
                        }
                    }

                    if (_currentFrame != null)
                        DisplayFrame(_currentFrame);

                    MessageBox.Show($"Loaded {_trackingRects.Count} tracking rectangles successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading tracking data: {ex.Message}");
                }
            }
        }


        #endregion

        #region Form Events

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isAnalyzing = false;
            _isExporting = false;

            _videoCapture?.Dispose();
            _videoWriter?.Dispose();
            _currentFrame?.Dispose();

            // Dispose PictureBox image
            if (VideoBox.Image != null)
            {
                VideoBox.Image.Dispose();
                VideoBox.Image = null;
            }

            base.OnFormClosing(e);
        }

        #endregion

    }
}
 
``` 
 
 
## VideoPlayer.Designer.cs 
 
```csharp 
﻿using System.Diagnostics;

namespace MotionTrackerFaceBlur
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    partial class VideoPlayer
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            VideoBox = new PictureBox();
            FrameSlider = new TrackBar();
            OpenVideoBtn = new Button();
            ExportVideoBtn = new Button();
            AddTrackingBtn = new Button();
            AnalyzeBtn = new Button();
            StopAnalyzeBtn = new Button();
            DeleteTrackingBtn = new Button();
            TrackingListBox = new ListBox();
            mainTable = new TableLayoutPanel();
            buttonLayoutPanel = new FlowLayoutPanel();
            gprTracking = new GroupBox();
            SaveTrackingBtn = new Button();
            LoadTrackingBtn = new Button();
            lblStartFrame = new Label();
            txtBoxHeight = new NumericUpDown();
            txtBoxWidth = new NumericUpDown();
            labelHeight = new Label();
            labelWidth = new Label();
            txtEndFrame = new NumericUpDown();
            txtStartFrame = new NumericUpDown();
            txtTrackingName = new TextBox();
            lblEndFrame = new Label();
            lblTrackingName = new Label();
            btnApplyChanges = new Button();
            timePanel = new Panel();
            lblStartTime = new Label();
            lblCurrentTime = new Label();
            lblEndTime = new Label();
            pnlFrameSlider = new Panel();
            trackRangeIndicator = new Panel();
            statusStripBottom = new StatusStrip();
            tssStatusLabel = new ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)VideoBox).BeginInit();
            ((System.ComponentModel.ISupportInitialize)FrameSlider).BeginInit();
            mainTable.SuspendLayout();
            buttonLayoutPanel.SuspendLayout();
            gprTracking.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)txtBoxHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtBoxWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtEndFrame).BeginInit();
            ((System.ComponentModel.ISupportInitialize)txtStartFrame).BeginInit();
            timePanel.SuspendLayout();
            pnlFrameSlider.SuspendLayout();
            statusStripBottom.SuspendLayout();
            SuspendLayout();
            // 
            // VideoBox
            // 
            VideoBox.BackColor = SystemColors.ControlDark;
            VideoBox.BorderStyle = BorderStyle.FixedSingle;
            VideoBox.Dock = DockStyle.Fill;
            VideoBox.Location = new Point(3, 3);
            VideoBox.Name = "VideoBox";
            VideoBox.Size = new Size(1194, 1102);
            VideoBox.SizeMode = PictureBoxSizeMode.Zoom;
            VideoBox.TabIndex = 0;
            VideoBox.TabStop = false;
            VideoBox.MouseDown += PictureBox_MouseDown;
            VideoBox.MouseMove += PictureBox_MouseMove;
            VideoBox.MouseUp += PictureBox_MouseUp;
            // 
            // FrameSlider
            // 
            FrameSlider.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            FrameSlider.Location = new Point(3, 3);
            FrameSlider.Maximum = 100;
            FrameSlider.Name = "FrameSlider";
            FrameSlider.Size = new Size(1188, 44);
            FrameSlider.TabIndex = 1;
            FrameSlider.TickFrequency = 10;
            FrameSlider.Scroll += FrameSlider_Scroll;
            FrameSlider.SizeChanged += FrameSlider_SizeChanged;
            // 
            // OpenVideoBtn
            // 
            OpenVideoBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            OpenVideoBtn.Location = new Point(3, 3);
            OpenVideoBtn.Name = "OpenVideoBtn";
            OpenVideoBtn.Size = new Size(273, 44);
            OpenVideoBtn.TabIndex = 2;
            OpenVideoBtn.Text = "Open Video";
            OpenVideoBtn.Click += OpenVideoBtn_Click;
            // 
            // ExportVideoBtn
            // 
            ExportVideoBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            ExportVideoBtn.Enabled = false;
            ExportVideoBtn.Location = new Point(3, 103);
            ExportVideoBtn.Name = "ExportVideoBtn";
            ExportVideoBtn.Size = new Size(273, 44);
            ExportVideoBtn.TabIndex = 3;
            ExportVideoBtn.Text = "Export Video";
            ExportVideoBtn.Click += ExportVideoBtn_Click;
            // 
            // AddTrackingBtn
            // 
            AddTrackingBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            AddTrackingBtn.Enabled = false;
            AddTrackingBtn.Location = new Point(3, 53);
            AddTrackingBtn.Name = "AddTrackingBtn";
            AddTrackingBtn.Size = new Size(273, 44);
            AddTrackingBtn.TabIndex = 4;
            AddTrackingBtn.Text = "Add Tracking Point";
            AddTrackingBtn.Click += AddTrackingBtn_Click;
            // 
            // AnalyzeBtn
            // 
            AnalyzeBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            AnalyzeBtn.Location = new Point(3, 153);
            AnalyzeBtn.Name = "AnalyzeBtn";
            AnalyzeBtn.Size = new Size(273, 44);
            AnalyzeBtn.TabIndex = 5;
            AnalyzeBtn.Text = "Analyze";
            AnalyzeBtn.Click += AnalyzeBtn_Click;
            // 
            // StopAnalyzeBtn
            // 
            StopAnalyzeBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            StopAnalyzeBtn.Location = new Point(3, 203);
            StopAnalyzeBtn.Name = "StopAnalyzeBtn";
            StopAnalyzeBtn.Size = new Size(273, 44);
            StopAnalyzeBtn.TabIndex = 6;
            StopAnalyzeBtn.Text = "Stop Analyzing";
            StopAnalyzeBtn.Click += StopAnalyzeBtn_Click;
            // 
            // DeleteTrackingBtn
            // 
            DeleteTrackingBtn.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            DeleteTrackingBtn.Enabled = false;
            DeleteTrackingBtn.Location = new Point(3, 253);
            DeleteTrackingBtn.Name = "DeleteTrackingBtn";
            DeleteTrackingBtn.Size = new Size(273, 44);
            DeleteTrackingBtn.TabIndex = 7;
            DeleteTrackingBtn.Text = "Delete Tracking";
            DeleteTrackingBtn.Click += DeleteTrackingBtn_Click;
            // 
            // TrackingListBox
            // 
            TrackingListBox.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            TrackingListBox.Location = new Point(3, 303);
            TrackingListBox.Name = "TrackingListBox";
            TrackingListBox.Size = new Size(273, 260);
            TrackingListBox.TabIndex = 8;
            TrackingListBox.SelectedIndexChanged += TrackingListBox_SelectedIndexChanged;
            // 
            // mainTable
            // 
            mainTable.ColumnCount = 2;
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
            mainTable.Controls.Add(VideoBox, 0, 0);
            mainTable.Controls.Add(buttonLayoutPanel, 1, 0);
            mainTable.Controls.Add(timePanel, 0, 2);
            mainTable.Controls.Add(pnlFrameSlider, 0, 1);
            mainTable.Dock = DockStyle.Fill;
            mainTable.Location = new Point(0, 0);
            mainTable.Name = "mainTable";
            mainTable.RowCount = 3;
            mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            mainTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            mainTable.Size = new Size(1480, 1208);
            mainTable.TabIndex = 9;
            // 
            // buttonLayoutPanel
            // 
            buttonLayoutPanel.Controls.Add(OpenVideoBtn);
            buttonLayoutPanel.Controls.Add(AddTrackingBtn);
            buttonLayoutPanel.Controls.Add(ExportVideoBtn);
            buttonLayoutPanel.Controls.Add(AnalyzeBtn);
            buttonLayoutPanel.Controls.Add(StopAnalyzeBtn);
            buttonLayoutPanel.Controls.Add(DeleteTrackingBtn);
            buttonLayoutPanel.Controls.Add(TrackingListBox);
            buttonLayoutPanel.Controls.Add(gprTracking);
            buttonLayoutPanel.Dock = DockStyle.Fill;
            buttonLayoutPanel.Location = new Point(1203, 3);
            buttonLayoutPanel.Name = "buttonLayoutPanel";
            buttonLayoutPanel.Size = new Size(274, 1102);
            buttonLayoutPanel.TabIndex = 10;
            // 
            // gprTracking
            // 
            gprTracking.Controls.Add(SaveTrackingBtn);
            gprTracking.Controls.Add(LoadTrackingBtn);
            gprTracking.Controls.Add(lblStartFrame);
            gprTracking.Controls.Add(txtBoxHeight);
            gprTracking.Controls.Add(txtBoxWidth);
            gprTracking.Controls.Add(labelHeight);
            gprTracking.Controls.Add(labelWidth);
            gprTracking.Controls.Add(txtEndFrame);
            gprTracking.Controls.Add(txtStartFrame);
            gprTracking.Controls.Add(txtTrackingName);
            gprTracking.Controls.Add(lblEndFrame);
            gprTracking.Controls.Add(lblTrackingName);
            gprTracking.Controls.Add(btnApplyChanges);
            gprTracking.Location = new Point(3, 569);
            gprTracking.Name = "gprTracking";
            gprTracking.Size = new Size(270, 428);
            gprTracking.TabIndex = 9;
            gprTracking.TabStop = false;
            gprTracking.Text = "Tracking Properties";
            // 
            // SaveTrackingBtn
            // 
            SaveTrackingBtn.Location = new Point(8, 324);
            SaveTrackingBtn.Name = "SaveTrackingBtn";
            SaveTrackingBtn.Size = new Size(254, 44);
            SaveTrackingBtn.TabIndex = 0;
            SaveTrackingBtn.Text = "Save";
            SaveTrackingBtn.Click += SaveTrackingBtn_Click;
            // 
            // LoadTrackingBtn
            // 
            LoadTrackingBtn.Location = new Point(8, 374);
            LoadTrackingBtn.Name = "LoadTrackingBtn";
            LoadTrackingBtn.Size = new Size(254, 44);
            LoadTrackingBtn.TabIndex = 1;
            LoadTrackingBtn.Text = "Load";
            LoadTrackingBtn.Click += LoadTrackingBtn_Click;
            // 
            // lblStartFrame
            // 
            lblStartFrame.AutoSize = true;
            lblStartFrame.Location = new Point(12, 175);
            lblStartFrame.Name = "lblStartFrame";
            lblStartFrame.Size = new Size(99, 32);
            lblStartFrame.TabIndex = 9;
            lblStartFrame.Text = "Strat Fr.:";
            // 
            // txtBoxHeight
            // 
            txtBoxHeight.Location = new Point(112, 128);
            txtBoxHeight.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            txtBoxHeight.Name = "txtBoxHeight";
            txtBoxHeight.Size = new Size(150, 39);
            txtBoxHeight.TabIndex = 7;
            txtBoxHeight.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // txtBoxWidth
            // 
            txtBoxWidth.Location = new Point(114, 83);
            txtBoxWidth.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            txtBoxWidth.Name = "txtBoxWidth";
            txtBoxWidth.Size = new Size(150, 39);
            txtBoxWidth.TabIndex = 6;
            txtBoxWidth.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // labelHeight
            // 
            labelHeight.AutoSize = true;
            labelHeight.Location = new Point(12, 133);
            labelHeight.Name = "labelHeight";
            labelHeight.Size = new Size(91, 32);
            labelHeight.TabIndex = 3;
            labelHeight.Text = "Height:";
            // 
            // labelWidth
            // 
            labelWidth.AutoSize = true;
            labelWidth.Location = new Point(12, 88);
            labelWidth.Name = "labelWidth";
            labelWidth.Size = new Size(83, 32);
            labelWidth.TabIndex = 2;
            labelWidth.Text = "Width:";
            // 
            // txtEndFrame
            // 
            txtEndFrame.Location = new Point(112, 218);
            txtEndFrame.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            txtEndFrame.Name = "txtEndFrame";
            txtEndFrame.Size = new Size(150, 39);
            txtEndFrame.TabIndex = 8;
            // 
            // txtStartFrame
            // 
            txtStartFrame.Location = new Point(114, 173);
            txtStartFrame.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
            txtStartFrame.Name = "txtStartFrame";
            txtStartFrame.Size = new Size(150, 39);
            txtStartFrame.TabIndex = 7;
            // 
            // txtTrackingName
            // 
            txtTrackingName.Location = new Point(112, 38);
            txtTrackingName.Name = "txtTrackingName";
            txtTrackingName.Size = new Size(150, 39);
            txtTrackingName.TabIndex = 5;
            // 
            // lblEndFrame
            // 
            lblEndFrame.AutoSize = true;
            lblEndFrame.Location = new Point(12, 218);
            lblEndFrame.Name = "lblEndFrame";
            lblEndFrame.Size = new Size(91, 32);
            lblEndFrame.TabIndex = 4;
            lblEndFrame.Text = "End Fr.:";
            // 
            // lblTrackingName
            // 
            lblTrackingName.AutoSize = true;
            lblTrackingName.Location = new Point(12, 43);
            lblTrackingName.Name = "lblTrackingName";
            lblTrackingName.Size = new Size(83, 32);
            lblTrackingName.TabIndex = 1;
            lblTrackingName.Text = "Name:";
            // 
            // btnApplyChanges
            // 
            btnApplyChanges.Location = new Point(8, 274);
            btnApplyChanges.Name = "btnApplyChanges";
            btnApplyChanges.Size = new Size(256, 44);
            btnApplyChanges.TabIndex = 0;
            btnApplyChanges.Text = "Apply";
            btnApplyChanges.Click += BtnApplyChanges_Click;
            // 
            // timePanel
            // 
            timePanel.Controls.Add(lblStartTime);
            timePanel.Controls.Add(lblCurrentTime);
            timePanel.Controls.Add(lblEndTime);
            timePanel.Dock = DockStyle.Fill;
            timePanel.Location = new Point(3, 1171);
            timePanel.Name = "timePanel";
            timePanel.Size = new Size(1194, 34);
            timePanel.TabIndex = 11;
            // 
            // lblStartTime
            // 
            lblStartTime.AutoSize = true;
            lblStartTime.Location = new Point(10, 10);
            lblStartTime.Name = "lblStartTime";
            lblStartTime.Size = new Size(102, 32);
            lblStartTime.TabIndex = 0;
            lblStartTime.Text = "00:00:00";
            // 
            // lblCurrentTime
            // 
            lblCurrentTime.AutoSize = true;
            lblCurrentTime.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblCurrentTime.Location = new Point(450, 10);
            lblCurrentTime.Name = "lblCurrentTime";
            lblCurrentTime.Size = new Size(112, 32);
            lblCurrentTime.TabIndex = 1;
            lblCurrentTime.Text = "00:00:00";
            // 
            // lblEndTime
            // 
            lblEndTime.AutoSize = true;
            lblEndTime.Location = new Point(900, 10);
            lblEndTime.Name = "lblEndTime";
            lblEndTime.Size = new Size(102, 32);
            lblEndTime.TabIndex = 2;
            lblEndTime.Text = "00:00:00";
            // 
            // pnlFrameSlider
            // 
            pnlFrameSlider.Controls.Add(trackRangeIndicator);
            pnlFrameSlider.Controls.Add(FrameSlider);
            pnlFrameSlider.Dock = DockStyle.Fill;
            pnlFrameSlider.Location = new Point(3, 1111);
            pnlFrameSlider.Name = "pnlFrameSlider";
            pnlFrameSlider.Size = new Size(1194, 54);
            pnlFrameSlider.TabIndex = 12;
            // 
            // trackRangeIndicator
            // 
            trackRangeIndicator.Anchor = AnchorStyles.None;
            trackRangeIndicator.BackColor = Color.FromArgb(100, 50, 205, 50);
            trackRangeIndicator.Location = new Point(3, 41);
            trackRangeIndicator.Name = "trackRangeIndicator";
            trackRangeIndicator.Size = new Size(274, 10);
            trackRangeIndicator.TabIndex = 13;
            // 
            // statusStripBottom
            // 
            statusStripBottom.ImageScalingSize = new Size(24, 24);
            statusStripBottom.Items.AddRange(new ToolStripItem[] { tssStatusLabel });
            statusStripBottom.Location = new Point(0, 1166);
            statusStripBottom.Name = "statusStripBottom";
            statusStripBottom.Size = new Size(1480, 42);
            statusStripBottom.TabIndex = 10;
            statusStripBottom.Text = "statusStrip1";
            // 
            // tssStatusLabel
            // 
            tssStatusLabel.Name = "tssStatusLabel";
            tssStatusLabel.Size = new Size(78, 32);
            tssStatusLabel.Text = "Ready";
            // 
            // VideoPlayer
            // 
            AutoScaleDimensions = new SizeF(13F, 32F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1480, 1208);
            Controls.Add(statusStripBottom);
            Controls.Add(mainTable);
            Name = "VideoPlayer";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Motion Tracker";
            ((System.ComponentModel.ISupportInitialize)VideoBox).EndInit();
            ((System.ComponentModel.ISupportInitialize)FrameSlider).EndInit();
            mainTable.ResumeLayout(false);
            buttonLayoutPanel.ResumeLayout(false);
            gprTracking.ResumeLayout(false);
            gprTracking.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)txtBoxHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtBoxWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtEndFrame).EndInit();
            ((System.ComponentModel.ISupportInitialize)txtStartFrame).EndInit();
            timePanel.ResumeLayout(false);
            timePanel.PerformLayout();
            pnlFrameSlider.ResumeLayout(false);
            pnlFrameSlider.PerformLayout();
            statusStripBottom.ResumeLayout(false);
            statusStripBottom.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.PictureBox VideoBox;
        private System.Windows.Forms.TrackBar FrameSlider;
        private System.Windows.Forms.Button OpenVideoBtn;
        private System.Windows.Forms.Button ExportVideoBtn;
        private System.Windows.Forms.Button AddTrackingBtn;
        private System.Windows.Forms.Button AnalyzeBtn;
        private System.Windows.Forms.Button StopAnalyzeBtn;
        private System.Windows.Forms.Button DeleteTrackingBtn;
        private System.Windows.Forms.ListBox TrackingListBox;
        private System.Windows.Forms.TableLayoutPanel mainTable;
        private System.Windows.Forms.FlowLayoutPanel buttonLayoutPanel;
        private System.Windows.Forms.GroupBox gprTracking;
        private System.Windows.Forms.NumericUpDown txtEndFrame;
        private System.Windows.Forms.NumericUpDown txtStartFrame;
        private System.Windows.Forms.TextBox txtTrackingName;
        private System.Windows.Forms.Label lblEndFrame;
        private System.Windows.Forms.Label lblTrackingName;
        private System.Windows.Forms.Button btnApplyChanges;
        private System.Windows.Forms.Label lblStartTime;
        private System.Windows.Forms.Label lblCurrentTime;
        private System.Windows.Forms.Label lblEndTime;
        private System.Windows.Forms.Panel timePanel;
        private System.Windows.Forms.StatusStrip statusStripBottom;
        private System.Windows.Forms.ToolStripStatusLabel tssStatusLabel;
        private Button SaveTrackingBtn;
        private Button LoadTrackingBtn;
        private System.Windows.Forms.NumericUpDown txtBoxWidth;
        private System.Windows.Forms.NumericUpDown txtBoxHeight;
        private System.Windows.Forms.Label labelWidth;
        private System.Windows.Forms.Label labelHeight;

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
        private Label lblStartFrame;
        private Panel pnlFrameSlider;
        private Panel trackRangeIndicator;
    }
} 
``` 
 
 

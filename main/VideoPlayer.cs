using OpenCvSharp;
using System.Globalization;

namespace MotionTrackerFaceBlur
{
    /// <summary>
    /// The form class. Owns every event handler and every control reference;
    /// all logic lives in the service classes (VideoLoader, FrameRenderer,
    /// ResizeController, TrackingManager, AnalysisEngine, ExportEngine),
    /// which share state through PlayerModel.
    /// </summary>
    public partial class VideoPlayer : Form, IDisposable
    {
        private readonly PlayerModel _model = new();
        private readonly VideoLoader _videoLoader;
        private readonly FrameRenderer _renderer;
        private readonly ResizeController _resizer;
        private readonly TrackingManager _trackingManager;
        private readonly AnalysisEngine _analysis;
        private readonly ExportEngine _exporter;

        public VideoPlayer()
        {
            InitializeComponent();

            _videoLoader = new VideoLoader(_model);
            _renderer = new FrameRenderer(_model) { Target = VideoBox };
            _resizer = new ResizeController(_model) { Renderer = _renderer };
            _trackingManager = new TrackingManager(_model);
            _analysis = new AnalysisEngine(_model) { FrameLoader = _videoLoader };
            _exporter = new ExportEngine(_model);

            WireCallbacks();
            InitializeCustomComponents();
        }

        private void WireCallbacks()
        {
            _videoLoader.ShowMessage = msg => MessageBox.Show(msg);
            _videoLoader.RunOnUi = action => Invoke(action);
            _videoLoader.RenderFrame = frame => DisplayFrame(frame);
            _videoLoader.UpdateTimeLabels = (start, current, end) =>
            {
                lblStartTime.Text = start.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                lblCurrentTime.Text = current.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                lblEndTime.Text = end.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                tssStatusLabel.Text = $"Frame: {_model.CurrentFrameIndex}/{_model.TotalFrames} | Time: {current:hh\\:mm\\:ss} | FPS: {_videoLoader.GetFps()}";
            };

            _trackingManager.ShowMessage = msg => MessageBox.Show(msg);
            _analysis.ShowMessage = msg => MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _exporter.ShowMessage = msg => MessageBox.Show(msg);

            _analysis.ReportStarted = (name, maxFrames) => Invoke((MethodInvoker)delegate
            {
                tssStatusLabel.Text = $"Analyzing: {name}...";
                tsspProgressBar.Visible = true;
                tsspProgressBar.Value = 0;
                tsspProgressBar.Maximum = maxFrames;
            });
            _analysis.ReportProgress = (name, framePos) => Invoke((MethodInvoker)delegate
            {
                int pct = framePos * 100 / Math.Max(1, tsspProgressBar.Maximum);
                tssStatusLabel.Text = $"Analyzing: {name} - {pct}% complete";
                tsspProgressBar.Value = Math.Min(framePos, tsspProgressBar.Maximum);
            });
            _analysis.ReportObjectLost = frame => Invoke((MethodInvoker)delegate
            {
                tssStatusLabel.Text = $"Analysis stopped at frame {frame} - object lost";
            });
            _analysis.ReportFinished = (name, msg) => Invoke((MethodInvoker)delegate
            {
                AnalyzeBtn.Visible = true;
                StopAnalyzeBtn.Visible = false;
                AddTrackingBtn.Enabled = true;
                DeleteTrackingBtn.Enabled = true;
                FrameSlider.Enabled = true;
                UpdateTrackingListColors();
                tsspProgressBar.Value = tsspProgressBar.Maximum;
                tssStatusLabel.Text = $"Analyzing: {name} - 100% complete";

                LoadFrame(_trackingManager.Selected!.StartFrame);
                FrameSlider.Value = _trackingManager.Selected.StartFrame;

                tsspProgressBar.Visible = false;
                tssStatusLabel.Text = msg;
                UpdateTimeDisplay();
                DisplayFrame(_model.CurrentFrame);
            });

            _exporter.ReportProgress = frameIndex => Invoke((MethodInvoker)delegate
            {
                int progress = frameIndex * 100 / _model.TotalFrames;
                tssStatusLabel.Text = $"Export: {progress}%";
                tsspProgressBar.Value = frameIndex;
            });
            _exporter.ReportFinished = msg => Invoke((MethodInvoker)delegate
            {
                tsspProgressBar.Value = tsspProgressBar.Maximum;
                tssStatusLabel.Text = "Export: 100%";
                MessageBox.Show("Video exported successfully!");
                Text = "Motion Tracker";
                mnuExportVideo.Enabled = true;
                _model.IsExporting = false;
                tsspProgressBar.Visible = false;
                tssStatusLabel.Text = msg;
            });
            _exporter.ReportFailed = err => Invoke((MethodInvoker)delegate
            {
                tsspProgressBar.Visible = false;
                MessageBox.Show(err);
                mnuExportVideo.Enabled = true;
                _model.IsExporting = false;
                tssStatusLabel.Text = "Export failed";
            });
        }

        private void InitializeCustomComponents()
        {
            AnalyzeBtn.Visible = false;
            StopAnalyzeBtn.Visible = false;
            gprTracking.Enabled = false;
            DeleteTrackingBtn.Enabled = false;

            UpdateTrackRangeIndicatorPosition();

            trackRangeIndicator.BringToFront();

            Resize += (s, e) => UpdateTrackRangeIndicatorPosition();

            AllowDrop = true;
            VideoBox.AllowDrop = true;
            VideoBox.DragEnter += OnVideoBoxDragEnter;
            VideoBox.DragDrop += OnVideoBoxDragDrop;
        }

        #region Video Events

        private void StartVideo(string path)
        {
            if (!_videoLoader.OpenVideo(path, out int totalFrames))
                return;

            FrameSlider.Maximum = totalFrames - 1;
            FrameSlider.Value = 0;
            _model.CurrentFrameIndex = 0;
            _videoLoader.LoadFrame(0);

            AddTrackingBtn.Enabled = true;
            mnuExportVideo.Enabled = true;
            mnuLoadTracking.Enabled = true;

            UpdateTimeDisplay();
        }

        private void MnuOpenVideo_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Video Files|*.mp4;*.avi;*.mov;*.wmv;*.mkv;*.flv;*.webm|All Files|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                StartVideo(openFileDialog.FileName);
            }
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            if (drgevent.Data?.GetData(DataFormats.FileDrop) is string[] files)
            {
                if (files.Any(VideoLoader.IsVideoFile))
                    drgevent.Effect = DragDropEffects.Copy;
            }
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);

            if (drgevent.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                string videoFile = files.FirstOrDefault(VideoLoader.IsVideoFile) ?? string.Empty;
                if (!string.IsNullOrEmpty(videoFile))
                    LoadVideoFromPath(videoFile);
            }
        }

        private void OnVideoBoxDragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files)
            {
                if (files.Any(VideoLoader.IsVideoFile))
                    e.Effect = DragDropEffects.Copy;
            }
        }

        private void OnVideoBoxDragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                string videoFile = files.FirstOrDefault(VideoLoader.IsVideoFile) ?? string.Empty;
                if (!string.IsNullOrEmpty(videoFile))
                    LoadVideoFromPath(videoFile);
            }
        }

        internal void LoadVideoFromPath(string path)
        {
            StartVideo(path);
        }

        private void FrameSlider_Scroll(object sender, EventArgs e)
        {
            if (_model.IsAnalyzing || _model.IsExporting) return;
            LoadFrame(FrameSlider.Value);

            if (_model.CurrentFrame != null)
                DisplayFrame(_model.CurrentFrame);
        }

        #endregion

        #region Time & Slider Display

        private void LoadFrame(int frameIndex, bool display = true)
        {
            _videoLoader.LoadFrame(frameIndex, display);
        }

        private void UpdateTimeDisplay()
        {
            _videoLoader.RefreshTimeDisplay();
        }

        private void FrameSlider_SizeChanged(object sender, EventArgs e)
        {
            UpdateTrackRangeIndicatorPosition();
        }

        private void UpdateTrackRangeIndicatorPosition()
        {
            if (trackRangeIndicator == null || FrameSlider == null) return;

            // Position the indicator just above the slider thumb area
            int sliderLeft = FrameSlider.Left + 25;
            int sliderWidth = FrameSlider.Width - 50;
            int sliderTop = FrameSlider.Top + FrameSlider.Height - 5;

            trackRangeIndicator.Location = new System.Drawing.Point(sliderLeft, sliderTop);
            trackRangeIndicator.Width = sliderWidth;
        }

        #endregion

        #region Frame Display

        private void DisplayFrame(Mat? frame)
        {
            if (frame == null) return;
            _renderer.RenderTo(frame, _model.SelectedTrackingIndex);
        }

        private void UpdateTrackingListColors()
        {
            for (int i = 0; i < _model.TrackingRects.Count; i++)
            {
                TrackingListBox.Items[i] = FrameRenderer.GetTrackingListText(_model.TrackingRects[i]);
            }
        }

        #endregion

        #region Mouse Events

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (_resizer.TryBeginResize(e.Location))
                return;

            if (!AddTrackingBtn.Enabled) return;

            var (vx, vy) = _resizer.PointToVideo(e.Location);
            _model.DragStartPoint = new OpenCvSharp.Point(vx, vy);
            _model.IsDragging = true;
            _model.DragRectangle = null;
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_model.IsDragging || !AddTrackingBtn.Enabled) return;

            _model.IsDragging = false;
            _model.IsResizing = false;

            if (_model.DragRectangle.HasValue && _model.DragRectangle.Value.Width > 5 && _model.DragRectangle.Value.Height > 5)
            {
                var tracking = _trackingManager.CreateFromDrag(
                    _model.DragRectangle.Value, _model.CurrentFrameIndex, _model.TotalFrames);

                TrackingListBox.Items.Add(tracking.Name);
                TrackingListBox.SelectedIndex = _model.SelectedTrackingIndex!.Value;

                UpdateTrackingProperties();
            }

            _model.DragRectangle = null;
            DisplayFrame(_model.CurrentFrame);
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            // Hover feedback for resize handles of the selected tracking rectangle
            if (!_model.IsDragging && !_model.IsResizing)
            {
                Cursor? hoverCursor = _resizer.GetHoverCursor(e.Location);
                VideoBox.Cursor = hoverCursor ?? Cursors.Default;
            }

            if (_model.IsDragging && AddTrackingBtn.Enabled)
            {
                var (videoX, videoY) = _resizer.PointToVideo(e.Location);

                int width = videoX - _model.DragStartPoint.X;
                int height = videoY - _model.DragStartPoint.Y;

                _model.DragRectangle = new System.Drawing.Rectangle(
                    Math.Min(_model.DragStartPoint.X, videoX),
                    Math.Min(_model.DragStartPoint.Y, videoY),
                    Math.Abs(width),
                    Math.Abs(height)
                );

                DisplayFrame(_model.CurrentFrame);
            }

            if (_model.IsResizing && !AddTrackingBtn.Enabled)
            {
                var (videoX, videoY) = _resizer.PointToVideo(e.Location);

                if (_resizer.ApplyResize(videoX, videoY))
                {
                    DisplayFrame(_model.CurrentFrame);
                }
            }
        }

        #endregion

        #region Tracking Events

        private void AddTrackingBtn_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Click and drag on the video to create a tracking rectangle.");
        }

        private void DeleteTrackingBtn_Click(object sender, EventArgs e)
        {
            if (_model.SelectedTrackingIndex.HasValue)
            {
                int index = _model.SelectedTrackingIndex.Value;
                _trackingManager.DeleteSelected();
                TrackingListBox.Items.RemoveAt(index);
                gprTracking.Enabled = false;
                AnalyzeBtn.Visible = false;
                DeleteTrackingBtn.Enabled = false;
                DisplayFrame(_model.CurrentFrame);
            }
        }

        private void TrackingListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TrackingListBox.SelectedIndex >= 0 && TrackingListBox.SelectedIndex < _model.TrackingRects.Count)
            {
                _model.SelectedTrackingIndex = TrackingListBox.SelectedIndex;
                var tracking = _trackingManager.Selected!;

                ShowFrameRangeOnSlider(tracking.StartFrame, tracking.EndFrame);

                if (_model.CurrentFrameIndex < tracking.StartFrame)
                {
                    LoadFrame(tracking.StartFrame);
                    FrameSlider.Value = tracking.StartFrame;
                }
                else if (_model.CurrentFrameIndex > tracking.EndFrame)
                {
                    LoadFrame(tracking.EndFrame);
                    FrameSlider.Value = tracking.EndFrame;
                }
                else
                {
                    DisplayFrame(_model.CurrentFrame);
                }

                UpdateTrackingProperties();
                gprTracking.Enabled = true;
                AnalyzeBtn.Visible = true;
                DeleteTrackingBtn.Enabled = true;

                mnuSaveTracking.Enabled = _model.TrackingRects.Count > 0;
            }
            else
            {
                if (trackRangeIndicator != null)
                    trackRangeIndicator.Visible = false;
            }
        }

        private void ShowFrameRangeOnSlider(int startFrame, int endFrame)
        {
            FrameSlider.Minimum = 0;
            FrameSlider.Maximum = _model.TotalFrames - 1;
            FrameSlider.TickFrequency = Math.Max(1, (_model.TotalFrames - 1) / 20);

            tssStatusLabel.Text = $"Track range: Frame {startFrame} to {endFrame}";

            UpdateTrackRangeIndicator(startFrame, endFrame);
        }

        private void UpdateTrackRangeIndicator(int startFrame, int endFrame)
        {
            if (trackRangeIndicator == null || _model.TotalFrames <= 0) return;

            float startPercent = (float)startFrame / _model.TotalFrames;
            float endPercent = (float)endFrame / _model.TotalFrames;

            trackRangeIndicator.Visible = true;

            trackRangeIndicator.Paint -= TrackRangeIndicator_Paint;
            trackRangeIndicator.Paint += TrackRangeIndicator_Paint;
            trackRangeIndicator.Tag = (StartPercent: startPercent, EndPercent: endPercent);
            trackRangeIndicator.Invalidate();
        }

        private void TrackRangeIndicator_Paint(object? sender, PaintEventArgs e)
        {
            if (sender is not Panel panel || panel.Tag is not (float startPercent, float endPercent)) return;

            int panelWidth = panel.Width;
            int panelHeight = panel.Height;

            e.Graphics.Clear(SystemColors.Control);

            int startX = (int)(startPercent * panelWidth);
            int endX = (int)(endPercent * panelWidth);
            int width = endX - startX;

            if (width > 0)
            {
                using Brush brush = new SolidBrush(Color.FromArgb(120, Color.LimeGreen));
                e.Graphics.FillRectangle(brush, startX, 0, width, panelHeight);

                using Pen pen = new(Color.LimeGreen, 2);
                e.Graphics.DrawRectangle(pen, startX, 0, width, panelHeight);
            }
        }

        private void UpdateTrackingProperties()
        {
            var tracking = _trackingManager.Selected;
            if (tracking == null) return;

            txtTrackingName.Text = tracking.Name;
            txtBoxWidth.Value = tracking.InitialRect.Width;
            txtBoxHeight.Value = tracking.InitialRect.Height;

            txtStartFrame.Minimum = 0;
            txtStartFrame.Maximum = _model.TotalFrames - 1;
            txtStartFrame.Value = Math.Max(0, Math.Min(tracking.StartFrame, _model.TotalFrames - 1));

            txtEndFrame.Minimum = 0;
            txtEndFrame.Maximum = _model.TotalFrames - 1;
            txtEndFrame.Value = Math.Max(0, Math.Min(tracking.EndFrame, _model.TotalFrames - 1));
        }

        private void BtnApplyChanges_Click(object sender, EventArgs e)
        {
            if (_model.SelectedTrackingIndex.HasValue)
            {
                var tracking = _trackingManager.Selected!;
                string oldName = tracking.Name;

                _trackingManager.ApplyProperties(
                    txtTrackingName.Text,
                    (int)txtBoxWidth.Value,
                    (int)txtBoxHeight.Value,
                    (int)txtStartFrame.Value,
                    (int)txtEndFrame.Value);

                if (tracking.Name != oldName)
                    TrackingListBox.Items[_model.SelectedTrackingIndex.Value] = tracking.Name;

                DisplayFrame(_model.CurrentFrame);
                MessageBox.Show("Tracking properties updated successfully!");
            }
        }

        #endregion

        #region Analysis Events

        private void AnalyzeBtn_Click(object sender, EventArgs e)
        {
            var tracking = _trackingManager.Selected;
            if (tracking == null)
            {
                MessageBox.Show("Please select a tracking point first.");
                return;
            }

            tracking.ClearPositions();

            _model.IsAnalyzing = true;
            AnalyzeBtn.Visible = false;
            StopAnalyzeBtn.Visible = true;
            AddTrackingBtn.Enabled = false;
            DeleteTrackingBtn.Enabled = false;
            FrameSlider.Enabled = false;
            mnuLoadTracking.Enabled = false;
            mnuSaveTracking.Enabled = false;

            Task.Run(() => _analysis.Analyze(tracking));
        }

        private void StopAnalyzeBtn_Click(object sender, EventArgs e)
        {
            _model.IsAnalyzing = false;
            AnalyzeBtn.Visible = true;
            StopAnalyzeBtn.Visible = false;
            AddTrackingBtn.Enabled = true;
            DeleteTrackingBtn.Enabled = true;
            FrameSlider.Enabled = true;
            mnuLoadTracking.Enabled = true;
            mnuSaveTracking.Enabled = true;
        }

        private void PerformAnalysis(TrackingRect tracking)
        {
            _analysis.Analyze(tracking);
        }

        #endregion

        #region Export Events

        private void MnuExportVideo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_model.VideoPath)) return;

            var unanalyzedTracks = _model.TrackingRects.Where(t => !t.IsAnalyzed).ToList();
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
                _model.IsExporting = true;
                mnuExportVideo.Enabled = false;
                Text = "Exporting video...";
                tssStatusLabel.Text = "Export: 0%";
                tsspProgressBar.Visible = true;
                tsspProgressBar.Value = 0;
                tsspProgressBar.Maximum = _model.TotalFrames;

                Task.Run(() => _exporter.Export(saveFileDialog.FileName));
            }
        }

        private void AnalyzeAllUnanalyzedTracks()
        {
            var unanalyzedTracks = _model.TrackingRects.Where(t => !t.IsAnalyzed).ToList();

            Task.Run(() =>
            {
                foreach (var tracking in unanalyzedTracks)
                {
                    tracking.ClearPositions();
                    _model.IsAnalyzing = true;

                    Invoke((MethodInvoker)delegate
                    {
                        AnalyzeBtn.Visible = false;
                        StopAnalyzeBtn.Visible = true;
                        AddTrackingBtn.Enabled = false;
                        DeleteTrackingBtn.Enabled = false;
                        FrameSlider.Enabled = false;
                    });

                    _analysis.Analyze(tracking);
                }

                Invoke((MethodInvoker)delegate
                {
                    _model.IsAnalyzing = false;
                    AnalyzeBtn.Visible = true;
                    StopAnalyzeBtn.Visible = false;
                    AddTrackingBtn.Enabled = true;
                    DeleteTrackingBtn.Enabled = true;
                    FrameSlider.Enabled = true;
                });
            });
        }

        #endregion

        #region Save/Load Tracking Data

        private void MnuSaveTracking_Click(object sender, EventArgs e)
        {
            if (_model.TrackingRects.Count == 0)
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
                    File.WriteAllText(saveFileDialog.FileName, TrackingStore.Serialize(_model.TrackingRects, _model.VideoPath));
                    MessageBox.Show("Tracking data saved successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving tracking data: {ex.Message}");
                }
            }
        }

        private void MnuLoadTracking_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = "Tracking Data|*.track|All Files|*.*";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var json = File.ReadAllText(openFileDialog.FileName);
                    var trackingData = TrackingStore.Deserialize(json);

                    if (trackingData == null)
                    {
                        MessageBox.Show("Failed to load tracking data.");
                        return;
                    }

                    _model.TrackingRects.Clear();
                    TrackingListBox.Items.Clear();
                    _model.SelectedTrackingIndex = null;
                    gprTracking.Enabled = false;
                    AnalyzeBtn.Visible = false;
                    DeleteTrackingBtn.Enabled = false;
                    mnuSaveTracking.Enabled = false;

                    foreach (var tracking in trackingData.TrackingRects)
                    {
                        // Reset analysis state for loaded tracks
                        tracking.IsAnalyzed = false;
                        tracking.PreviousRect = null;
                        tracking.ClearPositions();

                        _model.TrackingRects.Add(tracking);
                        TrackingListBox.Items.Add(tracking.Name);
                    }

                    if (!string.IsNullOrEmpty(trackingData.VideoPath) && trackingData.VideoPath != _model.VideoPath)
                    {
                        if (File.Exists(trackingData.VideoPath))
                        {
                            LoadVideoFromPath(trackingData.VideoPath);
                        }
                        else
                        {
                            MessageBox.Show("The original video file was not found. Please open the video manually.");
                        }
                    }

                    if (_model.CurrentFrame != null)
                        DisplayFrame(_model.CurrentFrame);

                    MessageBox.Show($"Loaded {_model.TrackingRects.Count} tracking rectangles successfully!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading tracking data: {ex.Message}");
                }
            }
        }

        #endregion

        #region Settings & Form Events

        private void MnuSettings_Click(object? sender, EventArgs e)
        {
            using SettingsForm settings = new();
            if (settings.ShowDialog(this) == DialogResult.OK)
            {
                _model.BlurCellSize = settings.BlurCellSize;
                _model.BigPixels = settings.BigPixels;
                _model.ConfidenceThreshold = settings.ConfidenceThreshold;

                AddTrackingBtn.Enabled = _model.SelectedTrackingIndex is not null;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Signal background work to stop and release video resources
            _model.IsAnalyzing = false;
            _model.IsExporting = false;

            lock (_model.VideoLock)
            {
                _model.VideoCapture?.Dispose();
                _model.VideoCapture = null!;
                _model.CurrentFrame?.Dispose();
                _model.CurrentFrame = null!;
            }
        }

        #endregion
    }
}

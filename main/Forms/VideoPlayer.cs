using OpenCvSharp;
using ShutterFace.DataObjects;
using ShutterFace.Engines;
using ShutterFace.FileHandling;
using ShutterFace.Resources;
using ShutterFace.Tracking;
using System.Globalization;

namespace ShutterFace
{
    /// <summary>
    /// The form class. Owns every event handler and every control reference;
    /// all logic lives in the service classes (VideoLoader, FrameRenderer,
    /// TrackerResizer, TrackerFactory, Tracker, VideoExporter),
    /// which share state through TrackerState.
    /// </summary>
    public partial class VideoPlayer : Form, IDisposable
    {
        private readonly TrackerState _model = new();
        private readonly VideoLoader _videoLoader;
        private readonly FrameRenderer _renderer;
        private readonly TrackerResizer _resizer;
        private readonly TrackerFactory _trackingManager;
        private readonly Tracker _analysis;
        private readonly ExportEngine _exporter;

        public VideoPlayer()
        {
            InitializeComponent();

            _videoLoader = new VideoLoader(_model);
            _renderer = new FrameRenderer(_model) { Target = VideoBox };
            _resizer = new TrackerResizer(_model) { Renderer = _renderer };
            _trackingManager = new TrackerFactory(_model);
            _analysis = new Tracker(_model) { FrameLoader = _videoLoader };
            _exporter = new ExportEngine(_model);

            WireCallbacks();
            InitializeCustomComponents();
            AddAboutMenuItem();
        }

        public void WriteLog(string message, LogSeverity severity = LogSeverity.Info)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
            var severityTag = severity switch
            {
                LogSeverity.Error => "[ERROR]",
                LogSeverity.Warning => "[WARN]",
                LogSeverity.Success => "[OK]",
                _ => "[INFO]",
            };

            var line = $"[{timestamp}] {severityTag} {message}{Environment.NewLine}";

            if (InvokeRequired)
            {
                Invoke((Action<string, LogSeverity>)WriteLog, message, severity);
                return;
            }

            logTextBox.AppendText(line);
            logTextBox.ScrollToCaret();
        }

        private void AddAboutMenuItem()
        {
            mnuCloseSeparator = new ToolStripSeparator();
            mnuFile.DropDownItems.Add(mnuCloseSeparator);
            mnuClose = new ToolStripMenuItem(ControlResourceManager.GetString("MenuExit"));
            mnuClose.Click += MnuClose_Click;
            mnuFile.DropDownItems.Add(mnuClose);

            mnuAbout = new ToolStripMenuItem(ControlResourceManager.GetString("MenuAbout"));
            mnuAbout.Click += MnuAbout_Click;
            menuStripTop.Items.Add(mnuAbout);

            AddLanguageMenu();
        }

        private void AddLanguageMenu()
        {
            var langMenu = new ToolStripMenuItem(ControlResourceManager.GetString("LanguageMenu"));
            var cultures = ControlResourceManager.AvailableCultures
                .Where(c => c != CultureInfo.InvariantCulture && !string.IsNullOrEmpty(c.TwoLetterISOLanguageName))
                .ToList();

            foreach (var culture in cultures)
            {
                var displayName = culture.TwoLetterISOLanguageName switch
                {
                    "en" => ControlResourceManager.GetString("LangEnglish"),
                    "de" => ControlResourceManager.GetString("LangGerman"),
                    _ => GetDisplayName(culture),
                };
                var item = new ToolStripMenuItem(displayName)
                {
                    CheckOnClick = true,
                    Tag = culture
                };
                item.Click += (s, e) => OnLanguageChanged(culture);
                langMenu.DropDownItems.Add(item);
            }

            menuStripTop.Items.Insert(1, langMenu);
            _langMenu = langMenu;
            UpdateLanguageCheckmarks();
        }

        private static string GetDisplayName(CultureInfo culture)
        {
            return culture.TwoLetterISOLanguageName switch
            {
                "en" => ControlResourceManager.GetString("LangEnglish"),
                "de" => ControlResourceManager.GetString("LangGerman"),
                _ => culture.DisplayName,
            };
        }

        private ToolStripMenuItem? _langMenu;

        private void OnLanguageChanged(CultureInfo culture)
        {
            Program.SetCulture(culture);
            UpdateLanguageCheckmarks();
            CultureConfig.SaveCulture(culture);
        }

        private void UpdateLanguageCheckmarks()
        {
            var ui = ControlResourceManager.Culture;
            if (_langMenu?.DropDownItems.Count > 0)
            {
                foreach (ToolStripMenuItem item in _langMenu.DropDownItems)
                {
                    var culture = item.Tag as CultureInfo;
                    var langName = culture?.TwoLetterISOLanguageName ?? string.Empty;
                    var uiLangName = ui?.TwoLetterISOLanguageName ?? string.Empty;
                    item.Checked = langName.Equals(uiLangName, StringComparison.Ordinal);
                }
            }
        }

        public void ApplyLanguage()
        {
            UpdateControlTexts();
            UpdateLanguageCheckmarks();
        }

        private void UpdateControlTexts()
        {
            mnuFile.Text = ControlResourceManager.GetString("FileMenu");
            if (_langMenu != null)
            {
                _langMenu.Text = ControlResourceManager.GetString("LanguageMenu");
            }

            mnuOpenVideo.Text = ControlResourceManager.GetString("MenuOpenVideo");
            mnuLoadTracking.Text = ControlResourceManager.GetString("MenuLoadProject");
            mnuSaveTracking.Text = ControlResourceManager.GetString("MenuSaveProject");
            mnuExportVideo.Text = ControlResourceManager.GetString("MenuExportVideo");
            mnuSettings.Text = ControlResourceManager.GetString("MenuSettings");
            mnuAbout.Text = ControlResourceManager.GetString("MenuAbout");
            mnuClose.Text = ControlResourceManager.GetString("MenuExit");

            AddTrackingBtn.Text = ControlResourceManager.GetString("BtnAddTracking");
            AnalyzeBtn.Text = ControlResourceManager.GetString("BtnAnalyze");
            StopAnalyzeBtn.Text = ControlResourceManager.GetString("BtnStopAnalyzing");
            DeleteTrackingBtn.Text = ControlResourceManager.GetString("BtnDeleteTracking");

            gprTracking.Text = ControlResourceManager.GetString("GroupTrackingProperties");
            btnSaveTracking.Text = ControlResourceManager.GetString("BtnSave");
            lblStartFrame.Text = ControlResourceManager.GetString("LabelStartFrame");
            txtBoxHeight.Tag = ControlResourceManager.GetString("LabelHeight");
            labelHeight.Text = ControlResourceManager.GetString("LabelHeight");
            labelWidth.Text = ControlResourceManager.GetString("LabelWidth");
            txtEndFrame.Tag = ControlResourceManager.GetString("LabelEndFrame");
            txtStartFrame.Tag = ControlResourceManager.GetString("LabelStartFrame");
            lblTrackingName.Text = ControlResourceManager.GetString("LabelTrackingName");
            txtTrackingName.Tag = ControlResourceManager.GetString("LabelTrackingName");
            lblEndFrame.Text = ControlResourceManager.GetString("LabelEndFrame");
            btnApplyChanges.Text = ControlResourceManager.GetString("BtnApply");

            Text = ControlResourceManager.GetString("AppName");
        }

        private void MnuClose_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void MnuAbout_Click(object? sender, EventArgs e)
        {
            using var about = new AboutForm();
            about.ShowDialog(this);
        }

        private void WireCallbacks()
        {
            _videoLoader.ShowMessage = msg => Invoke((Action<string>)(m => WriteLog(m, LogSeverity.Info)), msg);
            _videoLoader.RunOnUi = action => Invoke(action);
            _videoLoader.RenderFrame = frame => DisplayFrame(frame);
            _videoLoader.UpdateTimeLabels = (start, current, end) =>
            {
                lblStartTime.Text = start.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                lblCurrentTime.Text = current.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                lblEndTime.Text = end.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture);
                tssStatusLabel.Text = $"Frame: {_model.CurrentFrameIndex}/{_model.TotalFrames} | Time: {current.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture)} | FPS: {_videoLoader.GetFps()}";
            };

            _trackingManager.ShowMessage = msg => Invoke((Action<string>)(m => WriteLog(m, LogSeverity.Info)), msg);
            _analysis.ShowMessage = msg => Invoke((Action<string>)(m => WriteLog(m, LogSeverity.Error)), msg);
            _exporter.ShowMessage = msg => Invoke((Action<string>)(m => WriteLog(m, LogSeverity.Info)), msg);

            _analysis.ReportStarted = (name, maxFrames) => Invoke((MethodInvoker)delegate
            {
                tssStatusLabel.Text = ControlResourceManager.FormatString("StatusAnalyzing", name);
                tsspProgressBar.Visible = true;
                tsspProgressBar.Value = 0;
                tsspProgressBar.Maximum = maxFrames;
                WriteLog(ControlResourceManager.FormatString("MsgAnalysisStarted", name, maxFrames), LogSeverity.Info);
            });
            _analysis.ReportProgress = (name, framePos) => Invoke((MethodInvoker)delegate
            {
                int pct = framePos * 100 / Math.Max(1, tsspProgressBar.Maximum);
                tssStatusLabel.Text = ControlResourceManager.FormatString("StatusAnalyzing", $"{name} - {pct}% complete");
                tsspProgressBar.Value = Math.Min(framePos, tsspProgressBar.Maximum);
            });
            _analysis.ReportObjectLost = frame => Invoke((MethodInvoker)delegate
            {
                tssStatusLabel.Text = ControlResourceManager.FormatString("MsgAnalysisObjectLost", frame);
                WriteLog(ControlResourceManager.FormatString("MsgAnalysisObjectLost", frame), LogSeverity.Warning);
            });
            _analysis.ReportFinished = (name, msg) => Invoke((MethodInvoker)delegate
            {
                AnalyzeBtn.Visible = true;
                StopAnalyzeBtn.Visible = false;
                AddTrackingBtn.Enabled = true;
                DeleteTrackingBtn.Enabled = true;
                FrameSlider.Enabled = true;
                mnuLoadTracking.Enabled = true;
                mnuSaveTracking.Enabled = true;

                _model.Mode = InterfaceMode.Idle;
                UpdateTrackingListColors();
                tsspProgressBar.Value = tsspProgressBar.Maximum;
                tssStatusLabel.Text = ControlResourceManager.FormatString("StatusAnalyzingComplete", name);

                LoadFrame(_trackingManager.Selected!.StartFrame);
                FrameSlider.Value = _trackingManager.Selected.StartFrame;

                tsspProgressBar.Visible = false;
                tssStatusLabel.Text = msg;
                UpdateTimeDisplay();
                WriteLog(msg, LogSeverity.Success);
                DisplayFrame(_model.CurrentFrame);
            });

            _exporter.ReportProgress = frameIndex => Invoke((MethodInvoker)delegate
            {
                int progress = frameIndex * 100 / _model.TotalFrames;
                tssStatusLabel.Text = ControlResourceManager.FormatString("StatusExportingProgressFormat", progress);
                tsspProgressBar.Value = frameIndex;
            });
            _exporter.ReportFinished = msg => Invoke((MethodInvoker)delegate
            {
                _model.Mode = InterfaceMode.Idle;
                tsspProgressBar.Visible = false;
                tssStatusLabel.Text = msg;
                WriteLog(msg, LogSeverity.Success);
            });
            _exporter.ReportFailed = err => Invoke((MethodInvoker)delegate
            {
                tsspProgressBar.Visible = false;
                WriteLog(err, LogSeverity.Error);
                mnuExportVideo.Enabled = true;
                _model.Mode = InterfaceMode.Idle;
                tssStatusLabel.Text = ControlResourceManager.GetString("StatusExportComplete");
            });
        }

        private void InitializeCustomComponents()
        {
            AnalyzeBtn.Visible = false;
            StopAnalyzeBtn.Visible = false;
            gprTracking.Enabled = false;
            DeleteTrackingBtn.Enabled = false;
            FrameSlider.Enabled = false;

            Image analyzedIcon = CreateStatusIcon(Color.FromArgb(80, 200, 80));
            Image notAnalyzedIcon = CreateStatusIcon(Color.FromArgb(160, 160, 160));
            trackingImages.Images.Add("analyzed", analyzedIcon);
            trackingImages.Images.Add("not_analyzed", notAnalyzedIcon);

            UpdateTrackRangeIndicatorPosition();

            trackRangeIndicator.BringToFront();

            Resize += (s, e) => UpdateTrackRangeIndicatorPosition();

            AllowDrop = true;
            VideoBox.AllowDrop = true;
            VideoBox.DragEnter += OnVideoBoxDragEnter;
            VideoBox.DragDrop += OnVideoBoxDragDrop;
        }

        private static Bitmap CreateStatusIcon(Color color)
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using (var brush = new SolidBrush(color))
                {
                    g.FillEllipse(brush, 2, 2, 12, 12);
                }
            }
            return bmp;
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
            FrameSlider.Enabled = true;

            _model.Mode = InterfaceMode.Idle;
            UpdateTimeDisplay();
            WriteLog(ControlResourceManager.FormatString("MsgVideoLoaded", Path.GetFileName(path), totalFrames, _videoLoader.GetFps()), LogSeverity.Success);
        }

        private void MnuOpenVideo_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = ControlResourceManager.GetString("FilterVideoFiles");

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                StartVideo(openFileDialog.FileName);
            }
        }

        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            base.OnDragEnter(drgevent);

            ArgumentNullException.ThrowIfNull(drgevent);

            if (drgevent.Data?.GetData(DataFormats.FileDrop) is string[] files)
            {
                if (files.Any(VideoLoader.IsVideoFile))
                    drgevent.Effect = DragDropEffects.Copy;
            }
        }

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            base.OnDragDrop(drgevent);

            ArgumentNullException.ThrowIfNull(drgevent);

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
            if (_model.Mode != InterfaceMode.Idle) return;
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
                var tracking = _model.TrackingRects[i];
                TrackingListView.Items[i].ImageKey = tracking.IsAnalyzed ? "analyzed" : "not_analyzed";
            }
        }

        #endregion

        #region Mouse Events

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (_resizer.TryBeginResize(e.Location, _model.Mode))
                return;

            if (_model.Mode != InterfaceMode.Idle)
                return;

            var (vx, vy) = _resizer.PointToVideo(e.Location);
            _model.DragStartPoint = new OpenCvSharp.Point(vx, vy);
            _model.Mode = InterfaceMode.DragCreate;
            _model.DragRectangle = null;
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            InterfaceMode wasCreate = _model.Mode == InterfaceMode.DragCreate ? InterfaceMode.DragCreate : InterfaceMode.Idle;

            _model.ResizeEdge = default;
            _model.Mode = InterfaceMode.Idle;

            if (wasCreate == InterfaceMode.DragCreate && _model.DragRectangle is { } rect && rect.Width > 5 && rect.Height > 5)
            {
                var tracking = _trackingManager.CreateFromDrag(
                    rect, _model.CurrentFrameIndex, _model.TotalFrames);

                var listViewItem = new ListViewItem(tracking.Name) { ImageKey = "not_analyzed" };
                TrackingListView.Items.Add(listViewItem);
                _model.HasUnsavedChanges = true;
                TrackingListView.SelectedItems.Clear();
                var itemAdded = TrackingListView.Items[TrackingListView.Items.Count - 1];
                itemAdded.Selected = true;

                UpdateTrackingProperties();
                WriteLog(ControlResourceManager.FormatString("MsgTrackingCreated", tracking.Name), LogSeverity.Success);
            }

            _model.DragRectangle = null;
            AddTrackingBtn.Enabled = true;
            DisplayFrame(_model.CurrentFrame);
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            // Hover feedback for resize handles of the selected tracking rectangle
            if (_model.Mode != InterfaceMode.DragCreate && _model.Mode != InterfaceMode.ResizeDraggingAnchor)
            {
                Cursor? hoverCursor = _resizer.GetHoverCursor(e.Location);
                VideoBox.Cursor = hoverCursor ?? Cursors.Default;
            }

            if (_model.Mode == InterfaceMode.DragCreate)
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

            if (_model.Mode == InterfaceMode.ResizeDraggingAnchor)
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
            MessageBox.Show(ControlResourceManager.GetString("MsgClickDragTracking"));
        }

        private void DeleteTrackingBtn_Click(object sender, EventArgs e)
        {
            if (_model.SelectedTrackingIndex.HasValue)
            {
                int index = _model.SelectedTrackingIndex.Value;
                string name = TrackingListView.Items[index].Text;
                _trackingManager.DeleteSelected();
                TrackingListView.Items.RemoveAt(index);
                gprTracking.Enabled = false;
                AnalyzeBtn.Visible = false;
                DeleteTrackingBtn.Enabled = false;
                DisplayFrame(_model.CurrentFrame);
                WriteLog(ControlResourceManager.FormatString("MsgTrackingDeleted", name), LogSeverity.Warning);
            }
        }

        private void TrackingListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TrackingListView.SelectedIndices.Count > 0 && TrackingListView.SelectedIndices[0] < _model.TrackingRects.Count)
            {
                _model.SelectedTrackingIndex = TrackingListView.SelectedIndices[0];
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

            tssStatusLabel.Text = ControlResourceManager.FormatString("MsgTrackRange", startFrame, endFrame);

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
                    TrackingListView.SelectedItems[0].Text = tracking.Name;

                _model.HasUnsavedChanges = true;
                DisplayFrame(_model.CurrentFrame);
                WriteLog(ControlResourceManager.GetString("MsgTrackingUpdated"), LogSeverity.Success);
            }
        }
        #endregion

        #region Analysis Events



        private void AnalyzeBtn_Click(object sender, EventArgs e)
        {
            var tracking = _trackingManager.Selected;
            if (tracking == null)
            {
                WriteLog(ControlResourceManager.GetString("MsgSelectTrackingFirst"), LogSeverity.Warning);
                return;
            }

            tracking.ClearPositions();

            _model.DragRectangle = null;
            _model.Mode = InterfaceMode.Analyzing;

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
            _model.Mode = InterfaceMode.Idle;
            if (_analysis.ActiveTracker != null)
            {
                _analysis.ActiveTracker.EndFrame = _model.CurrentFrameIndex;
                WriteLog(ControlResourceManager.FormatString("MsgAnalysisStopped", _analysis.ActiveTracker.Name, _model.CurrentFrameIndex), LogSeverity.Warning);
            }
            AnalyzeBtn.Visible = true;
            StopAnalyzeBtn.Visible = false;
            AddTrackingBtn.Enabled = true;
            DeleteTrackingBtn.Enabled = true;
            FrameSlider.Enabled = true;
            mnuLoadTracking.Enabled = true;
            mnuSaveTracking.Enabled = true;

            _model.Mode = InterfaceMode.Idle;
        }

        private void PerformAnalysis(TrackerBox tracking)
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
                    ControlResourceManager.FormatString("MsgUnanalyzedTracksExport", unanalyzedTracks.Count),
                    ControlResourceManager.GetString("TitleUnanalyzedTracks"),
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
            saveFileDialog.Filter = ControlResourceManager.GetString("FilterSaveVideo");
            saveFileDialog.DefaultExt = "mp4";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                _model.Mode = InterfaceMode.Exporting;
                mnuExportVideo.Enabled = false;
                Text = ControlResourceManager.GetString("ExportingVideoTitle");
                tssStatusLabel.Text = ControlResourceManager.FormatString("StatusExportingProgressFormat", 0);
                WriteLog(ControlResourceManager.FormatString("MsgExportStarted", Path.GetFileName(saveFileDialog.FileName)), LogSeverity.Info);

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
                    _model.Mode = InterfaceMode.Analyzing;

                    Invoke((MethodInvoker)delegate
                    {
                        AnalyzeBtn.Visible = false;
                        StopAnalyzeBtn.Visible = true;
                        AddTrackingBtn.Enabled = false;
                        DeleteTrackingBtn.Enabled = false;
                        FrameSlider.Enabled = false;

                        _model.Mode = InterfaceMode.Idle;
                        _model.DragRectangle = null;
                    });

                    _analysis.Analyze(tracking);
                }

                Invoke((MethodInvoker)delegate
                {
                    _model.Mode = InterfaceMode.Idle;
                    AnalyzeBtn.Visible = true;
                    StopAnalyzeBtn.Visible = false;
                    AddTrackingBtn.Enabled = true;
                    DeleteTrackingBtn.Enabled = true;
                    FrameSlider.Enabled = true;

                    _model.Mode = InterfaceMode.Idle;
                });
            });
        }

        #endregion

        #region Save/Load Tracking Data

        private void MnuSaveTracking_Click(object sender, EventArgs e)
        {
            if (_model.TrackingRects.Count == 0)
            {
                WriteLog(ControlResourceManager.GetString("MsgNoTrackingToSave"), LogSeverity.Warning);
                return;
            }

            using SaveFileDialog saveFileDialog = new();
            saveFileDialog.Filter = ControlResourceManager.GetString("FilterProjectFiles");
            saveFileDialog.DefaultExt = "sft";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(saveFileDialog.FileName, TrackerStore.Serialize(_model.TrackingRects, _model.VideoPath));
                    WriteLog(ControlResourceManager.GetString("MsgTrackingSaved"), LogSeverity.Success);
                    _model.HasUnsavedChanges = false;
                }
                catch (IOException ex)
                {
                    WriteLog(ControlResourceManager.FormatString("ErrSaveTrackingData", ex.Message), LogSeverity.Error);
                }
            }
        }

        private void MnuLoadTracking_Click(object sender, EventArgs e)
        {
            using OpenFileDialog openFileDialog = new();
            openFileDialog.Filter = ControlResourceManager.GetString("FilterProjectFiles");

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var json = File.ReadAllText(openFileDialog.FileName);
                    var trackerSession = TrackerStore.Deserialize(json);

                    if (trackerSession == null)
                    {
                        WriteLog(ControlResourceManager.GetString("MsgFailedToLoadTracking"), LogSeverity.Error);
                        return;
                    }

                    _model.TrackingRects.Clear();
                    TrackingListView.Clear();
                    _model.SelectedTrackingIndex = null;
                    gprTracking.Enabled = false;
                    AnalyzeBtn.Visible = false;
                    DeleteTrackingBtn.Enabled = false;
                    mnuSaveTracking.Enabled = false;

                    foreach (var tracking in trackerSession.TrackingRects)
                    {
                        // Reset analysis state for loaded tracks
                        tracking.IsAnalyzed = false;
                        tracking.PreviousRect = null;
                        tracking.ClearPositions();

                        _model.TrackingRects.Add(tracking);
                        _model.HasUnsavedChanges = true;
                        var listViewItem = new ListViewItem(tracking.Name) { ImageKey = "not_analyzed" };
                        TrackingListView.Items.Add(listViewItem);
                    }

                    if (!string.IsNullOrEmpty(trackerSession.VideoPath) && trackerSession.VideoPath != _model.VideoPath)
                    {
                        if (File.Exists(trackerSession.VideoPath))
                        {
                            LoadVideoFromPath(trackerSession.VideoPath);
                        }
                        else
                        {
                            WriteLog(ControlResourceManager.GetString("MsgVideoNotFoundOpenManual"), LogSeverity.Warning);
                        }
                    }

                    if (_model.CurrentFrame != null)
                        DisplayFrame(_model.CurrentFrame);

                    tssStatusLabel.Text = ControlResourceManager.FormatString("MsgLoadedTracks", _model.TrackingRects.Count);
                    _model.HasUnsavedChanges = false;
                }
                catch (IOException ex)
                {
                    WriteLog(ControlResourceManager.FormatString("ErrLoadTrackingData", ex.Message), LogSeverity.Error);
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
            ArgumentNullException.ThrowIfNull(e);

            if (e.CloseReason == CloseReason.UserClosing && _model.HasUnsavedChanges && _model.Mode != InterfaceMode.Idle)
            {
                var result = MessageBox.Show(
                    ControlResourceManager.GetString("ConfirmUnsavedChanges"),
                    ControlResourceManager.GetString("TitleUnsavedChanges"),
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    MnuSaveTracking_Click(this, EventArgs.Empty);
                    if (_model.HasUnsavedChanges)
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnFormClosing(e);

            // Signal background work to stop and release video resources
            _model.Mode = InterfaceMode.Idle;

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

using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Globalization;
using System.Diagnostics;

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
        private bool _isAnalyzing;
        private int _currentFrameIndex;
        private int _totalFrames;
        private bool _isExporting;
        private int? _selectedTrackingIndex;

        private bool _isDragging;
        private OpenCvSharp.Point _dragStartPoint;
        private Rectangle? _dragRectangle;

        private int _blurCellSize = 8;
        private int _bigPixels = 16;
        private float _confidenceThreshold = 0.7f;
        private bool _isResizing;
        private EdgeKind _resizeEdge;
        private System.Drawing.Point _resizeStartPoint;

        private readonly object videoLock = new();

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

            // Enable drag & drop for video files (form-level handler)
            AllowDrop = true;
            VideoBox.AllowDrop = true;
        }

        private void MnuSettings_Click(object? sender, EventArgs e)
        {
            using SettingsForm settings = new();
            if (settings.ShowDialog(this) == DialogResult.OK)
            {
                _blurCellSize = settings.BlurCellSize;
                _bigPixels = settings.BigPixels;
                _confidenceThreshold = settings.ConfidenceThreshold;

                AddTrackingBtn.Enabled = _selectedTrackingIndex is not null;
            }
        }

        #endregion

        #region Form Events

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Signal background work to stop and release video resources
            _isAnalyzing = false;
            _isExporting = false;

            lock (videoLock)
            {
                _videoCapture?.Dispose();
                _videoCapture = null!;
                _currentFrame?.Dispose();
                _currentFrame = null!;
            }
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
                    File.WriteAllText(saveFileDialog.FileName, TrackingStore.Serialize(_trackingRects, _videoPath));
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
                    var trackingData = TrackingStore.Deserialize(json);

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
    }
}

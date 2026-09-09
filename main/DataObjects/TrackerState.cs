using OpenCvSharp;

namespace ShutterFace.DataObjects
{
    public enum LogSeverity
    {
        Info,
        Warning,
        Error,
        Success,
    }

    internal enum InterfaceMode
    {
        Idle,
        DragCreate,
        ResizeDraggingAnchor,
        Analyzing,
        Exporting,
    }

    /// <summary>
    /// Shared state object for the whole player. Every service class receives
    /// one instance and reads/writes the same fields the form used to own.
    /// Only the form itself touches the controls directly.
    /// </summary>
    internal sealed class TrackerState
    {
        // Video state
        public VideoCapture VideoCapture = null!;
        public Mat CurrentFrame = null!;
        public string VideoPath = string.Empty;
        public VideoWriter VideoWriter = null!;

        // Tracking state
        public readonly List<TrackerBox> TrackingRects = [];
        public int CurrentFrameIndex;
        public int TotalFrames;
        public int? SelectedTrackingIndex;

        // Mouse interaction state
        public InterfaceMode Mode;
        public OpenCvSharp.Point DragStartPoint;
        public Rectangle? DragRectangle;

        public EdgeKind ResizeEdge;

        // Change tracking
        public bool HasUnsavedChanges { get; set; }

        // Settings
        public int BlurCellSize = 8;
        public int BigPixels = 16;
        public float ConfidenceThreshold = 0.7f;

        /// <summary>
        /// Guards VideoCapture / CurrentFrame against the analysis and export threads.
        /// </summary>
        public readonly object VideoLock = new();
    }
}

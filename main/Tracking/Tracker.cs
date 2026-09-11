using OpenCvSharp;
using ShutterFace.DataObjects;
using ShutterFace.FileHandling;

namespace ShutterFace.Tracking
{
    /// <summary>
    /// Template-matching based object tracking across a frame range.
    /// State lives in TrackerState; UI feedback goes through callbacks.
    /// </summary>
    internal sealed class Tracker(TrackerState model)
    {
        public Action<string>? ShowMessage;

        /// <summary>UI hook: analysis started; receives the slider maximum (frame count).</summary>
        public Action<string, int>? ReportStarted;

        /// <summary>UI hook: called with (trackName, framePosition) during analysis.</summary>
        public Action<string, int>? ReportProgress;

        /// <summary>UI hook: analysis finished (or stopped early).</summary>
        public Action<string, string>? ReportFinished;

        /// <summary>UI hook: the object was lost, analysis stopped at this frame.</summary>
        public Action<int>? ReportObjectLost;

        /// <summary>The tracker currently being analyzed, or null if no analysis is running.</summary>
        public TrackerBox? ActiveTracker { get; private set; }

        /// <summary>Runs the analysis loop for one tracking rectangle. Blocks; run on a worker thread.</summary>
        public void Analyze(TrackerBox tracking)
        {
            int startFrame = tracking.StartFrame;
            tracking.PreviousRect = tracking.InitialRect;
            tracking.AddFramePosition(startFrame, tracking.InitialRect);

            ActiveTracker = tracking;
            ReportStarted?.Invoke(tracking.Name, Math.Max(1, tracking.EndFrame - startFrame));

            try
            {
                for (int i = startFrame + 1; i <= tracking.EndFrame && model.Mode == InterfaceMode.Analyzing; i++)
                {
                    lock (model.VideoLock)
                    {
                        FrameLoader.LoadFrame(i, i % 10 == 0);
                        model.CurrentFrameIndex = i;
                        if (!TrackObject(tracking, i))
                        {
                            tracking.EndFrame = i - 1;
                            ReportObjectLost?.Invoke(i);
                            break;
                        }

                        var rect = tracking.PreviousRect.Value;
                        tracking.AddFramePosition(i, rect);
                    }

                    int range = Math.Max(1, tracking.EndFrame - startFrame);
                    if (i % Math.Max(1, range / 5) == 0)
                    {
                        ReportProgress?.Invoke(tracking.Name, i - startFrame);
                    }
                }
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                ShowMessage?.Invoke(e.Message);
            }

            tracking.IsAnalyzed = true;
            ActiveTracker = null;
            model.Mode = InterfaceMode.Idle;
            ReportFinished?.Invoke(tracking.Name, $"Analysis complete for {tracking.Name}");
        }

        /// <summary>
        /// Tracks object using template matching between consecutive frames.
        /// Returns false if tracking confidence is too low or object moves out of bounds.
        /// </summary>
        public bool TrackObject(TrackerBox tracking, int frameIndex)
        {
            if (tracking.PreviousRect == null || model.VideoCapture == null || model.CurrentFrame == null)
                return false;

            using Mat previousFrame = new();
            model.VideoCapture.Set(VideoCaptureProperties.PosFrames, Math.Max(0, frameIndex - 1));
            model.VideoCapture.Read(previousFrame);

            if (previousFrame.Empty()) return false;

            // Clamp width/height to frame bounds — X/Y preserved so match results stay in video-space.
            var clampedRect = TrackerBox.GetClampedRect(tracking.PreviousRect.Value, previousFrame.Width, previousFrame.Height);
            if (clampedRect.Width <= 0 || clampedRect.Height <= 0)
                return false;

            using var template = new Mat(previousFrame, clampedRect);
            using var result = new Mat();

            Cv2.MatchTemplate(model.CurrentFrame, template, result, TemplateMatchModes.CCoeffNormed);
            Cv2.MinMaxLoc(result, out _, out double maxVal, out _, out OpenCvSharp.Point maxLoc);

            if (maxVal <= model.ConfidenceThreshold)
                return false;

            var newRect = new Rect(
                maxLoc.X,
                maxLoc.Y,
                tracking.InitialRect.Width,
                tracking.InitialRect.Height
            );

            if (!IsEdgeInBounds(newRect, tracking.PreviousRect.Value))
                return false;

            tracking.PreviousRect = newRect;
            return true;
        }

        /// <summary>
        /// Checks if at least one corner of the new rectangle overlaps with the previous rectangle.
        /// This prevents the tracker from jumping to unrelated areas.
        /// </summary>
        internal static bool IsEdgeInBounds(Rect newRect, Rect previousRect)
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

        public VideoLoader FrameLoader { get; set; } = null!;
    }
}
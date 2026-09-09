using OpenCvSharp;
using ShutterFace.DataObjects;

namespace ShutterFace.Tracking
{
    /// <summary>
    /// Creates, stores and edits tracker boxes. All state lives in
    /// TrackerState; the form wires UI callbacks and calls these methods
    /// from its event handlers.
    /// </summary>
    internal sealed class TrackerFactory(TrackerState model)
    {
        public Action<string>? ShowMessage;

        public TrackerBox CreateFromDrag(Rectangle dragRect, int startFrame, int totalFrames)
        {
            var tracking = new TrackerBox
            {
                Name = $"Face {model.TrackingRects.Count + 1}",
                StartFrame = startFrame,
                EndFrame = totalFrames - 1,
                InitialRect = new Rect(dragRect.X, dragRect.Y, dragRect.Width, dragRect.Height)
            };

            model.TrackingRects.Add(tracking);
            model.SelectedTrackingIndex = model.TrackingRects.Count - 1;
            return tracking;
        }

        public void DeleteSelected()
        {
            if (model.SelectedTrackingIndex.HasValue && model.SelectedTrackingIndex.Value < model.TrackingRects.Count)
            {
                model.TrackingRects.RemoveAt(model.SelectedTrackingIndex.Value);
                model.SelectedTrackingIndex = null;
            }
        }

        public TrackerBox? Selected
        {
            get
            {
                if (model.SelectedTrackingIndex.HasValue && model.SelectedTrackingIndex.Value < model.TrackingRects.Count)
                    return model.TrackingRects[model.SelectedTrackingIndex.Value];
                return null;
            }
        }

        public void ApplyProperties(string name, int newWidth, int newHeight, int newStartFrame, int newEndFrame)
        {
            var tracking = Selected;
            if (tracking == null) return;

            tracking.InitialRect = new Rect(tracking.InitialRect.X, tracking.InitialRect.Y, newWidth, newHeight);

            if (newStartFrame > newEndFrame)
            {
                ShowMessage?.Invoke("Start frame cannot be after end frame. Adjusting values.");
                newStartFrame = newEndFrame;
            }

            bool frameRangeChanged = tracking.StartFrame != newStartFrame || tracking.EndFrame != newEndFrame;

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

            if (!string.IsNullOrEmpty(name))
                tracking.Name = name;
        }
    }
}

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

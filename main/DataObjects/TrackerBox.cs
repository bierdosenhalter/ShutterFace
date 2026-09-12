using OpenCvSharp;

namespace ShutterFace.DataObjects
{
    internal class TrackerBox
    {
        public string Name { get; set; } = null!;
        public int StartFrame { get; set; }
        public int EndFrame { get; set; }
        public Rect InitialRect { get; set; }
        public Rect? PreviousRect { get; set; }
        public bool IsAnalyzed { get; set; }

        public readonly System.Collections.Generic.Dictionary<int, Rect> framePositions = [];

        public Dictionary<int, Rect> GetRectPositions() => new(framePositions);

        public void RemoveFramePosition(int frameIndex)
        {
            framePositions.Remove(frameIndex);
        }

        public bool HasTrackedPosition(int frameIndex) => framePositions.ContainsKey(frameIndex);

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

        /// <summary>
        /// Returns the intersection of *rect* with [0, width] × [0, height].
        /// Preserves coordinates outside bounds; only width/height are reduced.
        /// If fully outside bounds, returns a zero-size rect so callers can detect "no overlap".
        /// </summary>
        public static Rect GetClampedRect(Rect rect, int width, int height)
        {
            int clampedX = Math.Max(0, rect.X);
            int clampedY = Math.Max(0, rect.Y);
            int maxX = Math.Max(0, width - 1);  // prevent overflow when width/height == 0
            int maxY = Math.Max(0, height - 1);
            int clampedR = Math.Min(rect.X + rect.Width, maxX);
            int clampedB = Math.Min(rect.Y + rect.Height, maxY);
            return new Rect(clampedX, clampedY, Math.Max(0, clampedR - clampedX), Math.Max(0, clampedB - clampedY));
        }
    }
}

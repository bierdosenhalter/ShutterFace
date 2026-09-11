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
        /// Returns a clamped version of *rect* that fits within [0, width] × [0, height].
        /// Width and height are restricted to frame bounds; X/Y origin is preserved unchanged.
        /// If fully outside bounds (width or height clamp to zero), returns a zero-size rect
        /// at the original X/Y position so callers can detect "no overlap" without losing coordinates.
        /// </summary>
        public static Rect GetClampedRect(Rect rect, int width, int height)
        {
            int clampedW = Math.Max(0, Math.Min(rect.Width, width - rect.X));
            int clampedH = Math.Max(0, Math.Min(rect.Height, height - rect.Y));
            return new Rect(rect.X, rect.Y, clampedW, clampedH);
        }
    }
}

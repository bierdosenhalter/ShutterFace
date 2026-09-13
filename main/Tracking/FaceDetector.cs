using OpenCvSharp;
using ShutterFace.DataObjects;

namespace ShutterFace.Tracking
{
    /// <summary>
    /// Detects candidate face regions in the current frame using Core-only
    /// skin-tone segmentation (HSV thresholding) and contour bounding boxes.
    /// Only returns rectangles that are not already tracked (overlap check).
    /// </summary>
    internal sealed class FaceDetector(TrackerState model)
    {
        public int DetectNewFaces(out Rect[] detectedRects, out int detectedCount)
        {
            detectedRects = [];
            detectedCount = 0;

            var currentFrame = model.CurrentFrame;
            if (currentFrame == null || currentFrame.Empty())
                return -1;

            // Fast-path: nothing to track yet - treat all candidates equally.
            bool anyTracks = model.TrackingRects.Count > 0;

            using var hsv = new Mat();
            Cv2.CvtColor(currentFrame, hsv, ColorConversionCodes.BGR2HSV);

            // Skin-tone ranges in HSV (handles both warm skin and fair skin)
            Scalar lowerHw = new(0, 48, 0);
            Scalar upperHw = new(20, 255, 255);
            Scalar lowerHf = new(160, 48, 0);
            Scalar upperHf = new(172, 255, 255);

            Mat skinMask1 = new();
            Cv2.InRange(hsv, lowerHw, upperHw, skinMask1);

            Mat skinMask2 = new();
            Cv2.InRange(hsv, lowerHf, upperHf, skinMask2);

            // OR both masks for fair/skin ranges.
            Cv2.BitwiseOr(skinMask1, skinMask2, skinMask1);

            using var kernel = new Mat(5, 5, MatType.CV_8UC1, Scalar.All(1));
            Cv2.MorphologyEx(skinMask1, skinMask1, MorphTypes.Close, kernel);
            Cv2.MorphologyEx(skinMask1, skinMask1, MorphTypes.Open, kernel);

            Cv2.FindContours(skinMask1, out OpenCvSharp.Point[][] foundContours, out HierarchyIndex[] hierarchy, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            List<Rect> candidates = [];
            for (int i = 0; i < foundContours.Length; i++)
            {
                if (foundContours[i].Length < 3) continue;

                Rect rect = Cv2.BoundingRect(foundContours[i]);

                // Skip tiny contours and excessively wide regions.
                const int MinArea = 500;
                long area = (long)rect.Width * rect.Height;
                if (area < MinArea) continue;
                float aspectRatio = rect.Width / (float)Math.Max(1, rect.Height);
                if (aspectRatio > 1.3f) continue; // unlikely for a face

                candidates.Add(rect);
            }

            detectedCount = candidates.Count;
            detectedRects = new Rect[candidates.Count];
            for (int i = 0; i < candidates.Count; i++)
                detectedRects[i] = candidates[i];

            return anyTracks ? FilterNonOverlapping(detectedRects) : detectedCount;
        }

        private int FilterNonOverlapping(Rect[] rects)
        {
            List<Rect> result = [];
            foreach (var rect in rects)
            {
                if (!CandidateOverlapsTracked(rect, out _))
                    result.Add(rect);
                else if (OverlapsLarger(rect, rects))
                    result.Add(rect); // Keep larger candidate over existing track.
            }

            Array.Fill(rects, default);
            for (int i = 0; i < result.Count; i++)
                rects[i] = result[i];

            return result.Count;
        }

        private bool OverlapsLarger(Rect candidate, Rect[] all)
        {
            foreach (var other in all)
            {
                long otherArea = (long)other.Width * other.Height;
                long candArea = (long)candidate.Width * candidate.Height;
                if (otherArea >= candArea && CandidateOverlapsTracked(candidate, out _))
                    return true;
            }
            return false;
        }

        private bool CandidateOverlapsTracked(Rect candidate, out Rect existingTrack)
        {
            const float minOverlapRatio = 0.25f;
            foreach (var tracker in model.TrackingRects)
            {
                TrackerBox trackerBox = tracker as TrackerBox ?? throw new InvalidOperationException("Expected TrackerBox");

                if (trackerBox.InitialRect.Width == 0 && trackerBox.InitialRect.Height == 0) continue;

                int interX = Math.Max(candidate.X, trackerBox.InitialRect.X);
                int interY = Math.Max(candidate.Y, trackerBox.InitialRect.Y);
                int interR = Math.Min(candidate.X + candidate.Width, trackerBox.InitialRect.X + trackerBox.InitialRect.Width);
                int interB = Math.Min(candidate.Y + candidate.Height, trackerBox.InitialRect.Y + trackerBox.InitialRect.Height);

                int interW = Math.Max(0, interR - interX);
                int interH = Math.Max(0, interB - interY);
                if (interW == 0 || interH == 0) continue;

                double candidateArea = candidate.Width * candidate.Height;
                double intersectionArea = interW * interH;

                if (intersectionArea / candidateArea >= minOverlapRatio)
                {
                    existingTrack = trackerBox.InitialRect;
                    return true;
                }
            }

            existingTrack = default;
            return false;
        }
    }
}

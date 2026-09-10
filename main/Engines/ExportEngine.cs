using OpenCvSharp;
using ShutterFace.DataObjects;
using ShutterFace.Resources;

namespace ShutterFace.Engines
{
    /// <summary>
    /// Exports the video with pixelated tracking regions.
    /// State lives in PlayerModel; UI feedback goes through callbacks.
    /// </summary>
    internal sealed class ExportEngine(TrackerState model)
    {
        public Action<string>? ShowMessage;

        /// <summary>UI hook: export progress in percent.</summary>
        public Action<int>? ReportProgress;

        /// <summary>UI hook: export finished with a status message.</summary>
        public Action<string>? ReportFinished;

        /// <summary>UI hook: export failed; the form restores its controls.</summary>
        public Action<string>? ReportFailed;

        /// <summary>Runs the export loop. Blocks; run on a worker thread.</summary>
        public void Export(string outputPath)
        {
            try
            {
                if (model.VideoCapture == null || !model.VideoCapture.IsOpened() || model.CurrentFrame == null)
                {
                    ShowMessage?.Invoke("No video loaded to export.");
                    return;
                }

                int fps = (int)model.VideoCapture.Fps;
                int width = model.CurrentFrame.Width;
                int height = model.CurrentFrame.Height;
                int totalFrames = model.TotalFrames;

                int fourCC = (int)model.VideoCapture.Get(VideoCaptureProperties.FourCC);
                if (fourCC == 0)
                {
                    string extension = Path.GetExtension(model.VideoPath).ToUpperInvariant();
                    fourCC = extension switch
                    {
                        ".MP4" => FourCC.MP4V,
                        ".AVI" => FourCC.MP42,
                        ".WMV" => FourCC.WMV1,
                        _ => FourCC.MP4V
                    };
                }

                using var writer = new VideoWriter(outputPath, fourCC, fps, new OpenCvSharp.Size(width, height));

                ReportProgress?.Invoke(0);

                for (int i = 0; i < totalFrames && model.Mode == InterfaceMode.Exporting; i++)
                {
                    Mat exportFrame;
                    lock (model.VideoLock)
                    {
                        if (model.VideoCapture == null || !model.VideoCapture.IsOpened())
                            break;

                        model.VideoCapture.Set(VideoCaptureProperties.PosFrames, i);
                        model.CurrentFrame?.Dispose();
                        model.CurrentFrame = new Mat();
                        if (!model.VideoCapture.Read(model.CurrentFrame))
                            break;

                        model.CurrentFrameIndex = i;
                    }

                    exportFrame = model.CurrentFrame.Clone();

                    GridBlurFrames(exportFrame, i, model, width, height);

                    writer.Write(exportFrame);
                    exportFrame.Dispose();

                    if (i % Math.Max(1, Math.Min(totalFrames / 20, 100)) == 0)
                    {
                        ReportProgress?.Invoke(i * 100 / totalFrames);
                    }
                }

                ReportFinished?.Invoke("Export complete");
            }
            catch (Exception ex) when (ex is not OperationCanceledException and not OpenCVException)
            {
                ReportFailed?.Invoke(ControlResourceManager.FormatString("ErrExportVideo", ex.Message));
            }
        }

        /// <summary>
        /// Divides the frame into a grid and applies pixelation with cell-level blur consistency.
        /// Adjacent cells covered by overlapping tracking rectangles share the same block size
        /// for uniform visual appearance.
        /// </summary>
        private static void GridBlurFrames(Mat frame, int frameIndex, TrackerState state, int width, int height)
        {
            int cellSize = Math.Max(1, state.BlurCellSize);
            int gridCols = (width + cellSize - 1) / cellSize;
            int gridRows = (height + cellSize - 1) / cellSize;

            // Collect effective blur values for each covered cell
            var cellBigPixels = new Dictionary<int, List<float>>();

            foreach (var tracking in state.TrackingRects)
            {
                if (frameIndex < tracking.StartFrame || frameIndex > tracking.EndFrame)
                    continue;

                Rect? rect = tracking.GetRectAtFrame(frameIndex) ?? tracking.InitialRect;
                if (!rect.HasValue)
                    continue;

                var r = rect.Value;

                // Clip to frame bounds for cell calculation
                int x0 = Math.Max(0, r.X);
                int y0 = Math.Max(0, r.Y);
                int x1 = Math.Min(width, r.X + r.Width);
                int y1 = Math.Min(height, r.Y + r.Height);

                if (x0 >= x1 || y0 >= y1)
                    continue;

                int colStart = x0 / cellSize;
                int rowStart = y0 / cellSize;
                int colEnd = (x1 - 1) / cellSize + 1;
                int rowEnd = (y1 - 1) / cellSize + 1;

                foreach (int row in Enumerable.Range(rowStart, rowEnd - rowStart))
                {
                    foreach (int col in Enumerable.Range(colStart, colEnd - colStart))
                    {
                        int cellKey = row * gridCols + col;
                        if (!cellBigPixels.TryGetValue(cellKey, out var values))
                        {
                            values = [];
                            cellBigPixels[cellKey] = values;
                        }

                        // Weight by coverage area so overlapping rectangles contribute proportionally
                        int cellX = col * cellSize;
                        int cellY = row * cellSize;
                        int cellW = Math.Min(cellSize, width - col * cellSize);
                        int cellH = Math.Min(cellSize, height - row * cellSize);

                        int overlapX = Math.Min(x1, cellX + cellW) - Math.Max(x0, cellX);
                        int overlapY = Math.Min(y1, cellY + cellH) - Math.Max(y0, cellY);

                        if (overlapX > 0 && overlapY > 0)
                        {
                            float weight = (float)(overlapX * overlapY) / (cellW * cellH);
                            values.Add(weight * state.BigPixels);
                        }
                    }
                }
            }

            // Apply pixelation per-cell with weighted average block size using inverse-square mean
            foreach (var kvp in cellBigPixels)
            {
                if (kvp.Value.Count == 0)
                    continue;

                int row = kvp.Key / gridCols;
                int col = kvp.Key % gridCols;
                float effectiveBigPixels = ComputeEffectiveBigPixels(kvp.Value);

                int x = col * cellSize;
                int y = row * cellSize;
                int cellW = Math.Min(cellSize, width - col * cellSize);
                int cellH = Math.Min(cellSize, height - row * cellSize);

                if (cellW <= 0 || cellH <= 0)
                    continue;

                Rect cellRect = new(x, y, cellW, cellH);
                PixelateRegion(frame, cellRect, (int)Math.Round(effectiveBigPixels));
            }
        }

        internal static float ComputeEffectiveBigPixels(List<float> values)
        {
            if (values.Count == 1)
                return values[0];

            // Harmonic-mean weighting: cells covered by multiple rectangles use a
            // conservative (lower big-pixels = more blur) value with heavier weight.
            float sumWeights = 0f;
            float sumInverse = 0f;

            foreach (var v in values)
            {
                if (v > 0)
                {
                    float inv = 1f / v;
                    sumInverse += inv * inv;
                    sumWeights += v;
                }
            }

            if (sumInverse == 0f)
                return 16f;

            // Weighted RMS: sqrt(sum(v^2)/count) for natural averaging of scale values
            float weightedSum = 0f;
            foreach (var v in values)
            {
                if (v > 0)
                    weightedSum += v * v;
            }

            return (float)Math.Sqrt(weightedSum / values.Count);
        }

        /// <summary>
        /// Applies pixelation effect to a region of the image by downscaling and upscaling.
        /// Ensures the long side of the region has approximately the given number of big pixels
        /// for consistent blurring.
        /// </summary>
        public static void PixelateRegion(Mat image, Rect region, int bigPixels)
        {
            Rect safeRegion = new(
                Math.Max(0, region.X),
                Math.Max(0, region.Y),
                Math.Max(0, Math.Min(region.Width, image.Width - Math.Max(region.X, 0))),
                Math.Max(0, Math.Min(region.Height, image.Height - Math.Max(region.Y, 0)))
            );

            if (safeRegion.Width <= 0 || safeRegion.Height <= 0)
                return;

            if (bigPixels <= 0)
                return;

            Mat roi = new(image, safeRegion);

            int longestSide = Math.Max(safeRegion.Width, safeRegion.Height);
            int dynamicBlockSize = Math.Max(1, longestSide / bigPixels);
            dynamicBlockSize = Math.Max(dynamicBlockSize, 4);

            int smallWidth = Math.Max(1, safeRegion.Width / dynamicBlockSize);
            int smallHeight = Math.Max(1, safeRegion.Height / dynamicBlockSize);

            Mat small = new();
            Cv2.Resize(roi, small, new OpenCvSharp.Size(smallWidth, smallHeight),
                0, 0, InterpolationFlags.Linear);

            Mat pixelated = new();
            Cv2.Resize(small, pixelated, new OpenCvSharp.Size(safeRegion.Width, safeRegion.Height),
                0, 0, InterpolationFlags.Nearest);

            pixelated.CopyTo(roi);

            roi.Dispose();
            small.Dispose();
            pixelated.Dispose();
        }
    }
}

using OpenCvSharp;
using ShutterFace.Resources;
using System.Globalization;

namespace ShutterFace
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
                    string extension = Path.GetExtension(model.VideoPath).ToLower(CultureInfo.InvariantCulture);
                    fourCC = extension switch
                    {
                        ".mp4" => FourCC.MP4V,
                        ".avi" => FourCC.MP42,
                        ".wmv" => FourCC.WMV1,
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

                    foreach (var tracking in model.TrackingRects)
                    {
                        if (i >= tracking.StartFrame && i <= tracking.EndFrame)
                        {
                            Rect? rect = tracking.GetRectAtFrame(i) ?? tracking.InitialRect;
                            if (rect.HasValue)
                            {
                                PixelateRegion(exportFrame, rect.Value, model.BigPixels);
                            }
                        }
                    }

                    writer.Write(exportFrame);
                    exportFrame.Dispose();

                    if (i % Math.Max(1, Math.Min(totalFrames / 20, 100)) == 0)
                    {
                        ReportProgress?.Invoke(i * 100 / totalFrames);
                    }
                }

                ReportFinished?.Invoke("Export complete");
            }
            catch (Exception ex)
            {
                ReportFailed?.Invoke(ControlResourceManager.FormatString("ErrExportVideo", ex.Message));
            }
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

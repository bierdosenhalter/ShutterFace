using OpenCvSharp;

namespace ShutterFace
{
    /// <summary>
    /// Logic for creating a rectangle by dragging and resizing it by its handles.
    /// State lives in TrackerState; geometry math lives in TrackerGeometry.
    /// </summary>
    internal sealed class TrackerResizer(TrackerState model)
    {
        /// <summary>Converts PictureBox coordinates to video coordinates using the current display area.</summary>
        public (int videoX, int videoY) PointToVideo(System.Drawing.Point pictureBoxPoint)
        {
            var (videoRect, scaleX, scaleY) = Renderer.GetVideoDisplayArea();
            return ((int)((pictureBoxPoint.X - videoRect.X) * scaleX),
                    (int)((pictureBoxPoint.Y - videoRect.Y) * scaleY));
        }

        /// <summary>
        /// Starts a resize when the pointer is on a handle of the selected tracking
        /// rectangle. Returns true when a resize was started.
        /// </summary>
        public bool TryBeginResize(System.Drawing.Point pictureBoxPoint, InterfaceMode InterfaceMode)
        {
            if (InterfaceMode == InterfaceMode.DragCreate)
                return false;

            if (model.SelectedTrackingIndex is not int selIdx || selIdx >= model.TrackingRects.Count)
                return false;

            var tracking = model.TrackingRects[selIdx];
            if (model.CurrentFrameIndex < tracking.StartFrame || model.CurrentFrameIndex > tracking.EndFrame)
                return false;

            Rect? handleRect = tracking.GetRectAtFrame(model.CurrentFrameIndex) ?? tracking.InitialRect;
            if (!handleRect.HasValue)
                return false;

            var (videoRect, scaleX, scaleY) = Renderer.GetVideoDisplayArea();
            EdgeKind edge = TrackerGeometry.GetEdgeKindAtPoint(handleRect.Value, pictureBoxPoint, videoRect, scaleX, scaleY);

            if (edge == EdgeKind.None)
                return false;

            model.ResizeEdge = edge;
            model.Mode = InterfaceMode.ResizeDraggingAnchor;
            return true;
        }

        /// <summary>Cursor for hover over the selected tracking rectangle, or null for default.</summary>
        public Cursor? GetHoverCursor(System.Drawing.Point pictureBoxPoint)
        {
            if (model.Mode == InterfaceMode.DragCreate || model.Mode == InterfaceMode.ResizeDraggingAnchor)
                return null;

            if (model.SelectedTrackingIndex is not int hoverIdx || hoverIdx >= model.TrackingRects.Count)
                return null;

            var tracking = model.TrackingRects[hoverIdx];
            if (model.CurrentFrameIndex < tracking.StartFrame || model.CurrentFrameIndex > tracking.EndFrame)
                return null;

            Rect? hoverRect = tracking.GetRectAtFrame(model.CurrentFrameIndex) ?? tracking.InitialRect;
            if (!hoverRect.HasValue)
                return null;

            var (videoRect, scaleX, scaleY) = Renderer.GetVideoDisplayArea();
            return TrackerGeometry.GetCursorForHandle(hoverRect.Value, pictureBoxPoint, videoRect, scaleX, scaleY);
        }

        /// <summary>
        /// Applies a resize drag to the selected tracking rectangle.
        /// Returns true when the frame's rect was updated.
        /// </summary>
        public bool ApplyResize(int videoX, int videoY)
        {
            if (model.SelectedTrackingIndex is not int idx || idx >= model.TrackingRects.Count)
                return false;

            var tracking = model.TrackingRects[idx];
            if (model.CurrentFrameIndex < tracking.StartFrame || model.CurrentFrameIndex > tracking.EndFrame)
                return false;

            Rect? baseRect = tracking.GetRectAtFrame(model.CurrentFrameIndex) ?? tracking.InitialRect;
            if (!baseRect.HasValue)
                return false;

            if (model.CurrentFrame == null)
                return false;

            int handleThresh = 6;
            int minX = 0, minY = 0;
            int maxX = model.CurrentFrame.Width - 1;
            int maxY = model.CurrentFrame.Height - 1;

            int origLeft = baseRect.Value.X;
            int origTop = baseRect.Value.Y;
            int origRight = baseRect.Value.X + baseRect.Value.Width;
            int origBottom = baseRect.Value.Y + baseRect.Value.Height;

            Rect newRect;

            switch (model.ResizeEdge)
            {
                case EdgeKind.TopLeft:
                    newRect = new Rect(
                        Math.Min(Math.Max(videoX, minX), origRight - handleThresh),
                        Math.Min(Math.Max(videoY, minY), origBottom - handleThresh),
                        0, 0);
                    newRect.Width = origRight - newRect.X;
                    newRect.Height = origBottom - newRect.Y;
                    break;

                case EdgeKind.TopCenter:
                    newRect = new Rect(origLeft, Math.Min(Math.Max(videoY, minY), origBottom - handleThresh),
                        baseRect.Value.Width, 0);
                    newRect.Height = origBottom - newRect.Y;
                    break;

                case EdgeKind.TopRight:
                    newRect = new Rect(origLeft, Math.Min(Math.Max(videoY, minY), origBottom - handleThresh),
                        Math.Max(handleThresh, Math.Min(videoX, maxX) - origLeft), 0);
                    newRect.Height = origBottom - newRect.Y;
                    break;

                case EdgeKind.RightCenter:
                    newRect = new Rect(origLeft, origTop,
                        Math.Max(handleThresh, Math.Min(videoX, maxX) - origLeft), baseRect.Value.Height);
                    break;

                case EdgeKind.BottomRight:
                    newRect = new Rect(origLeft, origTop,
                        Math.Max(handleThresh, Math.Min(videoX, maxX) - origLeft),
                        Math.Max(handleThresh, Math.Min(videoY, maxY) - origTop));
                    break;

                case EdgeKind.BottomCenter:
                    newRect = new Rect(origLeft, origTop, baseRect.Value.Width,
                        Math.Max(handleThresh, Math.Min(videoY, maxY) - origTop));
                    break;

                case EdgeKind.BottomLeft:
                    newRect = new Rect(Math.Min(Math.Max(videoX, minX), origRight - handleThresh), origTop,
                        0, Math.Max(handleThresh, Math.Min(videoY, maxY) - origTop));
                    newRect.Width = origRight - newRect.X;
                    break;

                case EdgeKind.LeftCenter:
                    newRect = new Rect(Math.Min(Math.Max(videoX, minX), origRight - handleThresh), origTop,
                        0, baseRect.Value.Height);
                    newRect.Width = origRight - newRect.X;
                    break;

                default:
                    return false;
            }

            if (newRect.Width <= 0 || newRect.Height <= 0)
                return false;

            if (model.CurrentFrameIndex == tracking.StartFrame)
                tracking.InitialRect = newRect;
            else
                tracking.AddFramePosition(model.CurrentFrameIndex, newRect);

            return true;
        }

        public FrameRenderer Renderer { get; set; } = null!;
    }
}

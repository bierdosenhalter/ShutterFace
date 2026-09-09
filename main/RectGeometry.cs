using OpenCvSharp;

namespace ShutterFace
{
    /// <summary>Which resize handle of a tracking rectangle the pointer is over.</summary>
    internal enum EdgeKind { None, TopLeft, TopCenter, TopRight, RightCenter, BottomRight, BottomCenter, BottomLeft, LeftCenter }

    /// <summary>
    /// State-free geometry helpers for tracking rectangles: handle drawing,
    /// edge hit-testing and resize cursors. Pure functions over their inputs,
    /// so they can be unit tested without a Form.
    /// </summary>
    internal static class RectGeometry
    {
        private const int HandleSize = 8;
        private const int DrawHandleSize = 5;

        /// <summary>
        /// Draws resize handles around a rectangle for interactive resizing.
        /// </summary>
        public static void DrawResizeHandles(Mat displayFrame, Rect rect)
        {
            Scalar handleColor = Scalar.White;

            int[] corners = new int[]
            {
                rect.X - DrawHandleSize, rect.Y - DrawHandleSize,
                rect.X + rect.Width/2, rect.Y - DrawHandleSize,
                rect.X + rect.Width - DrawHandleSize, rect.Y - DrawHandleSize,
                rect.X + rect.Width, rect.Y + rect.Height/2 - DrawHandleSize,
                rect.X + rect.Width - DrawHandleSize, rect.Y + rect.Height - DrawHandleSize,
                rect.X + rect.Width/2, rect.Y + rect.Height - DrawHandleSize,
                rect.X - DrawHandleSize, rect.Y + rect.Height - DrawHandleSize,
                rect.X - DrawHandleSize, rect.Y + rect.Height/2,
            };

            for (int i = 0; i < corners.Length; i += 2)
            {
                Cv2.Rectangle(displayFrame, new Rect(corners[i], corners[i + 1], DrawHandleSize * 2, DrawHandleSize * 2), handleColor, -1);
            }
        }

        /// <summary>
        /// Gets the resize cursor based on mouse position relative to active handles.
        /// The video display mapping (rect, scale factors) comes from the caller.
        /// </summary>
        public static Cursor GetCursorForHandle(Rect rect, System.Drawing.Point pictureBoxPoint, Rectangle videoRect, float scaleX, float scaleY)
        {
            EdgeKind edge = GetEdgeKindAtPoint(rect, pictureBoxPoint, videoRect, scaleX, scaleY);
            return edge == EdgeKind.None ? Cursors.Default : ResizeCursorForEdge(edge);
        }

        /// <summary>
        /// Determines which resize edge/handle of a rectangle contains the given
        /// PictureBox coordinates, or EdgeKind.None when outside all handles.
        /// </summary>
        public static EdgeKind GetEdgeKindAtPoint(Rect rect, System.Drawing.Point pictureBoxPoint, Rectangle videoRect, float scaleX, float scaleY)
        {
            if (videoRect == Rectangle.Empty) return EdgeKind.None;

            int videoX = (int)((pictureBoxPoint.X - videoRect.X) * scaleX);
            int videoY = (int)((pictureBoxPoint.Y - videoRect.Y) * scaleY);

            bool Hit(OpenCvSharp.Point center) =>
                Math.Abs(videoX - center.X) <= HandleSize &&
                Math.Abs(videoY - center.Y) <= HandleSize;

            var topLeft = new OpenCvSharp.Point(rect.X, rect.Y);
            var topCenter = new OpenCvSharp.Point(rect.X + rect.Width / 2, rect.Y);
            var topRight = new OpenCvSharp.Point(rect.X + rect.Width, rect.Y);
            var rightCenter = new OpenCvSharp.Point(rect.X + rect.Width, rect.Y + rect.Height / 2);
            var bottomRight = new OpenCvSharp.Point(rect.X + rect.Width, rect.Y + rect.Height);
            var bottomCenter = new OpenCvSharp.Point(rect.X + rect.Width / 2, rect.Y + rect.Height);
            var bottomLeft = new OpenCvSharp.Point(rect.X, rect.Y + rect.Height);
            var leftCenter = new OpenCvSharp.Point(rect.X, rect.Y + rect.Height / 2);

            if (Hit(topLeft)) return EdgeKind.TopLeft;
            if (Hit(topCenter)) return EdgeKind.TopCenter;
            if (Hit(topRight)) return EdgeKind.TopRight;
            if (Hit(rightCenter)) return EdgeKind.RightCenter;
            if (Hit(bottomRight)) return EdgeKind.BottomRight;
            if (Hit(bottomCenter)) return EdgeKind.BottomCenter;
            if (Hit(bottomLeft)) return EdgeKind.BottomLeft;
            if (Hit(leftCenter)) return EdgeKind.LeftCenter;

            return EdgeKind.None;
        }

        private static Cursor ResizeCursorForEdge(EdgeKind edge) => edge switch
        {
            EdgeKind.TopLeft or EdgeKind.BottomRight => Cursors.SizeNWSE,
            EdgeKind.BottomLeft or EdgeKind.TopRight => Cursors.SizeNESW,
            EdgeKind.TopCenter or EdgeKind.BottomCenter => Cursors.SizeNS,
            EdgeKind.LeftCenter or EdgeKind.RightCenter => Cursors.SizeWE,
            _ => Cursors.Default,
        };
    }
}

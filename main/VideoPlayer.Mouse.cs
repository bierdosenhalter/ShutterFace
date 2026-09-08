using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Globalization;
using System.Diagnostics;
using System.Text.Json;

namespace MotionTrackerFaceBlur
{
    public partial class VideoPlayer : Form, IDisposable
    {
        #region Mouse Interaction & Rectangle Drawing

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            // Start resizing when grabbing a handle of the selected tracking rectangle
            if (_selectedTrackingIndex is int selIdx && selIdx < _trackingRects.Count
                && _currentFrameIndex >= _trackingRects[selIdx].StartFrame
                && _currentFrameIndex <= _trackingRects[selIdx].EndFrame)
            {
                Rect? handleRect = _trackingRects[selIdx].GetRectAtFrame(_currentFrameIndex) ?? _trackingRects[selIdx].InitialRect;
                EdgeKind edge = EdgeKind.None;
                if (handleRect.HasValue)
                {
                    var (vr, sx, sy) = GetVideoDisplayArea();
                    edge = RectGeometry.GetEdgeKindAtPoint(handleRect.Value, e.Location, vr, sx, sy);
                }
                if (edge != EdgeKind.None)
                {
                    _isResizing = true;
                    _resizeEdge = edge;
                    _resizeStartPoint = e.Location;
                    return;
                }
            }

            if (!AddTrackingBtn.Enabled) return;

            var (videoRect, scaleX, scaleY) = GetVideoDisplayArea();

            _dragStartPoint = new OpenCvSharp.Point(
                (int)((e.X - videoRect.X) * scaleX),
                (int)((e.Y - videoRect.Y) * scaleY)
            );

            _isDragging = true;
            _dragRectangle = null;
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !AddTrackingBtn.Enabled) return;

            _isDragging = false;
            _isResizing = false;

            if (_dragRectangle.HasValue && _dragRectangle.Value.Width > 5 && _dragRectangle.Value.Height > 5)
            {
                var tracking = new TrackingRect
                {
                    Name = $"Face {_trackingRects.Count + 1}",
                    StartFrame = _currentFrameIndex,
                    EndFrame = _totalFrames - 1,
                    InitialRect = new Rect(
                        _dragRectangle.Value.X,
                        _dragRectangle.Value.Y,
                        _dragRectangle.Value.Width,
                        _dragRectangle.Value.Height
                    )
                };

                _trackingRects.Add(tracking);
                TrackingListBox.Items.Add(tracking.Name);
                _selectedTrackingIndex = _trackingRects.Count - 1;
                TrackingListBox.SelectedIndex = _selectedTrackingIndex.Value;

                UpdateTrackingProperties();
            }

            _dragRectangle = null;
            DisplayFrame(_currentFrame);
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            // Hover feedback for resize handles of the selected tracking rectangle
            if (!_isDragging && !_isResizing && _selectedTrackingIndex is int hoverIdx && hoverIdx < _trackingRects.Count
                && _currentFrameIndex >= _trackingRects[hoverIdx].StartFrame
                && _currentFrameIndex <= _trackingRects[hoverIdx].EndFrame)
            {
                Rect? hoverRect = _trackingRects[hoverIdx].GetRectAtFrame(_currentFrameIndex) ?? _trackingRects[hoverIdx].InitialRect;
                if (hoverRect.HasValue)
                {
                    var (vr, sx, sy) = GetVideoDisplayArea();
                    VideoBox.Cursor = RectGeometry.GetCursorForHandle(hoverRect.Value, e.Location, vr, sx, sy);
                }
                else
                {
                    VideoBox.Cursor = Cursors.Default;
                }
            }

            if (_isDragging && AddTrackingBtn.Enabled)
            {
                var (videoRect, scaleX, scaleY) = GetVideoDisplayArea();

                int videoX = (int)((e.X - videoRect.X) * scaleX);
                int videoY = (int)((e.Y - videoRect.Y) * scaleY);

                int width = videoX - _dragStartPoint.X;
                int height = videoY - _dragStartPoint.Y;

                _dragRectangle = new Rectangle(
                    Math.Min(_dragStartPoint.X, videoX),
                    Math.Min(_dragStartPoint.Y, videoY),
                    Math.Abs(width),
                    Math.Abs(height)
                );

                DisplayFrame(_currentFrame);
            }

            if (_isResizing && _selectedTrackingIndex is int idx && !AddTrackingBtn.Enabled)
            {
                var (videoRect, scaleX, scaleY) = GetVideoDisplayArea();
                int videoX = (int)((e.X - videoRect.X) * scaleX);
                int videoY = (int)((e.Y - videoRect.Y) * scaleY);

                var tracking = _trackingRects[idx];
                if (_currentFrameIndex < tracking.StartFrame || _currentFrameIndex > tracking.EndFrame)
                    return;

                Rect? baseRect = tracking.GetRectAtFrame(_currentFrameIndex) ?? tracking.InitialRect;
                if (!baseRect.HasValue)
                    return;

                int handleThresh = 6;
                int minX = 0;
                int minY = 0;
                int maxX = _currentFrame!.Width - 1;
                int maxY = _currentFrame.Height - 1;

                int origLeft = baseRect.Value.X;
                int origTop = baseRect.Value.Y;
                int origRight = baseRect.Value.X + baseRect.Value.Width;
                int origBottom = baseRect.Value.Y + baseRect.Value.Height;

                Rect newRect;

                switch (_resizeEdge)
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
                        return;
                }

                if (newRect.Width > 0 && newRect.Height > 0)
                {
                    if (_currentFrameIndex == tracking.StartFrame)
                    {
                        tracking.InitialRect = newRect;
                    }
                    else
                    {
                        tracking.AddFramePosition(_currentFrameIndex, newRect);
                    }

                    UpdateDisplayForResize();
                }
            }
        }

        private void UpdateDisplayForResize()
        {
            DisplayFrame(_currentFrame);
        }

        /// <summary>
        /// Calculates the actual video display area within the PictureBox
        /// accounting for aspect ratio preservation (Zoom mode).
        /// </summary>
        private (Rectangle videoRect, float scaleX, float scaleY) GetVideoDisplayArea()
        {
            if (_currentFrame == null || VideoBox.Width == 0 || VideoBox.Height == 0)
                return (Rectangle.Empty, 1f, 1f);

            int pbWidth = VideoBox.Width;
            int pbHeight = VideoBox.Height;

            int videoWidth = _currentFrame.Width;
            int videoHeight = _currentFrame.Height;

            float pbAspect = (float)pbWidth / pbHeight;
            float videoAspect = (float)videoWidth / videoHeight;

            int displayWidth, displayHeight;
            int offsetX = 0, offsetY = 0;

            if (pbAspect > videoAspect)
            {
                displayHeight = pbHeight;
                displayWidth = (int)(pbHeight * videoAspect);
                offsetX = (pbWidth - displayWidth) / 2;
            }
            else
            {
                displayWidth = pbWidth;
                displayHeight = (int)(pbWidth / videoAspect);
                offsetY = (pbHeight - displayHeight) / 2;
            }

            float scaleX = (float)videoWidth / displayWidth;
            float scaleY = (float)videoHeight / displayHeight;

            var videoRect = new Rectangle(offsetX, offsetY, displayWidth, displayHeight);

            return (videoRect, scaleX, scaleY);
        }

        #endregion
    }
}

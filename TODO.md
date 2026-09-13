# ShutterFace - TODO

## Bugs
- [x] When saving/loading a project the track data is incomplete (tracking boxes not displayed on canvas) - seems like position is missing? Resolved by tracking fix.
- [x] Out-of-bounds rectangles drawn as zero-size and dropped — GetClampedRect() used `width - rect.X` which went negative when X ≥ width, producing clampedW=0. **Fixed**: replaced with proper rect ∩ frame bounds intersection that preserves out-of-bound coordinates, only reduces width/height to visible portion (TrackerBox.cs).

## Features
- [x] Settings dialog layout has excessive whitespace
- [x] Face recognition feature needed: scan current frame for faces that aren't already tracked (overlap check), adjust threshold automatically — Implemented via btnDetectFaces + FaceDetector.cs (HSV skin-tone segmentation + contour detection).

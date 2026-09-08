# MotionTrackerFaceBlur - TODO

## 1. "Track N" → "Face N" Umbenennung (Trivial)
- [x] Default format: "Face 1", "Face 2"; keep backward-compatible JSON key for existing `.track` files

## 2. Status bar progress indicator at bottom of window (Einfach)
- [ ] Show status during tracking/export with frames/percentage; auto-update

## 3. Drag and drop video support on main window (Einfach)
- [ ] Accept `.mp4`, `.avi`, `.mkv` dropped onto the main form

## 4. Settings panel in a menu dialog (Datei → Einstellungen) (Mittel)
- [ ] `MenuStrip` → `SettingsForm`: blur cell size, strength, confidence threshold

## 5. Resize handles on placed rectangle display in video frames (Mittel)
- [ ] Draw corner/edge resize handles when a rectangle is selected; handle drag to adjust size and visible display during playback

## 6. In-app log panel at bottom below video preview (Mittel)
- [ ] Text/log area with timestamps + severity; replace error popup dialogs

## 7. Global grid blur to avoid borders between adjacent tracks (Schwer)
- [ ] Divide frame into equal-sized cells; each cell uses one blur radius so adjacent rects share consistent style
- [ ] Configurable cell size (default ~5% of video dimensions) or presets

## 8. Persist analysis state on save (Schwer)
- [ ] Save analysis of the tracks within a `.track` file; allows load → verify → export without re-tracking

## 9. Visible-portion logic for frame boundaries (Am Schwersten)
- [ ] Allow out-of-bounds rectangles
- [ ] Clip out-of-bounds rectangles to visible bounds before passing to OpenCvSharp during tracking and move them with the tracking until the whole rectangle is in the frame
- [ ] Store full rect in `.track` but pass only visible portion on each frame

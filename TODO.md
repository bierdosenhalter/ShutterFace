# MotionTrackerFaceBlur - TODO

## 1. Visible-portion logic for frame boundaries (High impact)
- [ ] Clip out-of-bounds rectangles to visible bounds before passing to OpenCvSharp during tracking
- [ ] Store full rect in `.track` but pass only visible portion on each frame

## 2. Resize handles on placed rectangles (Quick Win)
- [ ] Draw corner/edge resize handles when a rectangle is selected; handle drag to adjust size

## 3. Rename "Track N" → "Face N" (Trivial)
- [ ] Default format: "Face 1", "Face 2"; keep backward-compatible JSON key for existing `.track` files

## 4. Persist analysis state on save (Simple fix)
- [ ] Don't reset `IsAnalyzed` when loading a `.track` file; allows load → verify → export without re-tracking

## 5. Resizable rectangle display during playback (Alongside #1)
- [ ] Show visible portion of out-of-bounds rectangles on video frames for visual feedback

---

## 6. Global grid blur to avoid borders between adjacent tracks
- [ ] Divide frame into equal-sized cells; each cell uses one blur radius so adjacent rects share consistent style
- [ ] Configurable cell size (default ~5% of video dimensions) or presets

## 7. Status bar progress indicator at bottom of window
- [ ] Show status during tracking/export with frames/percentage; auto-update

## 8. Settings panel in a menu dialog (Datei → Einstellungen)
- [ ] `MenuStrip` → `SettingsForm`: blur cell size, strength, confidence threshold

## 9. In-app log panel at bottom below video preview (replace dialog errors)
- [ ] Text/log area with timestamps + severity; auto-scroll; errors instead of popup dialogs

## 10. Drag and drop video support on main window
- [ ] Accept `.mp4`, `.avi`, `.mkv` dropped onto the main form

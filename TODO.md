# ShutterFace - TODO

## 1. Project file extension (.sft) (Einfach)
- [x] Decide whether `.sft` is a safe extension (not used by other apps) for shutter-face-tracking project files
- [x] Rename "Load tracking / Save tracking" to "Load project / Save project"

## 2. Analysis stop → end frame (Einfach)
- [x] When clicking "stop analysing", set the end frame to the current frame position

## 3. Visible tracking feedback (Einfach)
- [x] Show visibly whether a face was tracked or not — ListView with green/gray status dots

## 4. UI polish (Einfach)
- [x] Frame slider should disabled when no video is loaded
- [x] In-app log panel below video preview: text/log area with timestamps + severity; replace error popup dialogs

## 5. Anchor bugs (Mittel)
- [x] Anchors are not moveable. Only when video analysis is active (should be disabled during analysis)
- [x] Anchors (N, S, E, W) poorly placed — corner fine but edges positions off

## 6. Global grid blur (Schwer)
- [ ] Divide frame into equal-sized cells; each cell uses one blur radius so adjacent rects share consistent style
- [ ] Configurable cell size (default ~5% of video dimensions) or presets

## 7. Persist analysis state on save (Schwer)
- [ ] Save analysis results within a `.track` file — allows load → verify → export without re-tracking

## 8. Visible-portion logic for frame boundaries (Am Schwersten)
- [ ] Allow out-of-bounds rectangles to exist in the project
- [ ] Clip out-of-bounds portion before passing to OpenCvSharp during tracking
- [ ] Store full rect in `.track` but pass only visible portion on each frame

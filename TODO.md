# ShutterFace - TODO

## Bugs
-[x] When saving/loading a project the track data is incomplete (tracking boxes not displayed on canvas) - seems like position is missing? Resolved by tracking fix.
- [ ] It is not possible to draw out-of-bounds rectangles (auto-clamped to video size; should extend until last pixel enters screen) — partially done: live preview now shows full rect outside bounds (Bug #0/24 in session notes). Clamped rects on canvas still need work to fully respect non-shifting origin.

## Features
- [ ] Settings dialog layout has excessive whitespace
- [ ] Face recognition button needs design concept: scans current frame for untracked faces with auto-adjusted parameters/lowered threshold
- [ ] Face recognition feature needed: scan current frame for faces that aren't already tracked (overlap check), adjust threshold automatically

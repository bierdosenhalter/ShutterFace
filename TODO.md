# ShutterFace - TODO

# bugs
- when drawing a rectangle on the screen there is no preview until drag is over (when the rect is created)
- when stopping to analyze/track the length is of the tracker indicator is not updated and also not the parameters of the tracking (end frame)
- when analyzing the anchors of the selected rectangle/tracking should not be shown
- when saving and loading a project the track data is incomplete (the rectangles are not shown on the screen) - also a proper test is missing here i guess
~~when exporting a video the export menu entry gets disabled and another export is not possible anymore~~ - fixed (mnuExportVideo re-enabled in ReportFinished callback)
~~there is a save buttin in tracking properties that is not functional and obsolete~~ - removed btnSaveTracking

# features
- the progressbar is not really of use... the frame indicator could act as one as it shows the processed frame exactly
- there should be always a bit space around the video to draw out of bounds rectangles (that start off video and end in video)
- it is not possible to draw out of bounds rectangles (they automatically are clamped to the video size) -> out of bounds rectangles/trackings should move with the area until they are fully in screen and to the end of the video or as long as the last bit is on the screen)
~~open and save projects should be first in the file menu then a seperator and then open video~~ - reordered mnuFile.DropDownItems
- when analyzing the frame indicator should be disabled and show the actual frame that is analyzed (not every frame but with the prg update - update logic must be disabled)
- settings frame layout is not good form has to much whitespace
- face recognision button (need an idea for that) that scans the current frame for a face (adjust parameters/lower threshold until it finds one) that is not already found (overlap) and draws a rectangle/tracking
# ShutterFace — hide faces in video

<img src="icon.png" alt="ShutterFace icon" width="120" align="right">

**Track-and-blur face privacy for your videos — local, manual, precise.**

---

## Short description

ShutterFace blurs faces in your videos. Draw a box once and it follows the face through every frame — no cloud, no upload, your footage stays on your machine. Export the anonymized video and share it with confidence.

---

## Why ShutterFace?

Building privacy tools for content creators, journalists, and anyone who needs to protect identities:

- **No permissions.** You blur faces yourself — there's no need to ask permission from people in your footage.
- **Privacy-first.** Everything runs 100% locally on your machine. Your video never uploads anywhere.
- **Manual but powerful.** Draw a box around a face once; ShutterFace tracks it across all frames using template matching (OpenCvSharp4, powered by OpenCV).

---

## How it works

![icon.png](icon.png)

1. **Open** a video — via file picker or drag-and-drop.
2. **Draw** a bounding box around the face you want to blur. Add multiple faces if needed.
3. **Analyze** — ShutterFace tracks each region across every frame using template matching.
4. **Export** — produce a blurred version of your video ready for sharing.

### Key features / Workflow

| Step | Action |
|------|--------|
| Load video | Open via dialog or drag-and-drop onto the window (`*.mp4`, `*.avi`, `*.mov`, `*.wmv`, `*.mkv`, `*.flv`, `*.webm`) |
| Create tracking rects | Click "Add Tracking Point", then click-drag on a face in the video preview |
| Fine-tune | Select a track from the list to adjust its name, size (8 resize handles), and start/end frame range |
| Track | Hit **Analyze** — template matching (`CCoeffNormed`) follows each box across frames with progress bar feedback |
| Export | Save as `*.mp4` to disk (audio not yet preserved) |

### Settings

- **Blur cell size** — grid cell for the blur engine. Larger cells = wider, softer coverage across adjacent tracks.
- **Big pixels** — controls blur granularity (lower = coarser/pixelated blur).
- **Confidence threshold** — minimum template-match score (0.1–1.0) for tracking to continue per frame.

### Saving and reloading work

Tracking data saves to a `*.track` JSON file. Reload it later with the original video to export without re-running analysis.

---

## System requirements

- **Windows 10 or later** (WinForms .NET 8 desktop runtime)
- OpenCV pre-built binaries are bundled — no separate install needed

---

## Getting started

```powershell
dotnet run --project main/ShutterFace.csproj
```

Or open `ShutterFace.sln` in Visual Studio / VS Code.

### Building from source

```powershell
dotnet build

# Verify clean build (no warnings, no formatting diffs, tests pass)
dotnet format --verify-no-changes
dotnet test
```

---

## Tech stack

| Layer | Technology |
|-------|------------|
| UI | WinForms (.NET 8, C#) |
| Computer vision / tracking | OpenCvSharp4 + OpenCV (bundled `runtime.win`) |
| Video I/O | OpenCvSharp `VideoCapture` / `VideoWriter` |
| Export format | MP4 (FourCC auto-detected, falls back to `MP4V`) |

---

## What's on the roadmap

| Item | Status | Complexity |
|------|--------|------------|
| Renamed "Tracking N" → "Face N" | Done | Trivial |
| Progress bar in status bar | Done | Simple |
| Drag-and-drop video support | Done | Simple |
| Settings dialog | Done | Medium |
| Resize handles on tracks | Done | Medium |
| In-app log panel (replacing popup dialogs) | TODO | Medium |
| Global grid blur for adjacent track consistency | TODO | Hard |
| Persist analysis state in `.track` files | TODO | Hard |
| Handle out-of-bounds rectangles gracefully | TODO | Hardest |
| Preserve audio in exported video | TODO | — |

---

## License

(Add your license here, e.g. MIT)

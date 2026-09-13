# ShutterFace — hide faces in video

<img src="icon.png" alt="ShutterFace icon" width="120" align="right">

**Blurring faces in your videos — locally, precisely, privately.**

---

## What it does

Draw a box around a face and ShutterFace tracks it through every frame of the video, then applies pixelated blur so no one can be identified. Your footage never leaves your machine. No cloud, no upload, no account required.

Perfect for content creators, journalists, or anyone who needs to protect people's identities in their videos before sharing them.

---

## How to use it

1. **Open a video** — drag & drop or browse to your file (`*.mp4`, `*.avi`, `*.mov`, `*.wmv`, `*.mkv`, `*.flv`, `*.webm`)
2. **Find the faces** — click **Detect Faces** for an automatic scan, or draw boxes manually with your mouse
3. **Tweak if needed** — resize boxes, adjust names, set start/end frames
4. **Analyze** — watch the tracker follow each face across every frame
5. **Export** — save a blurred version of your video to disk

---

## Settings

- **Blur cell size** — controls how soft the blur spreads between adjacent tracked faces
- **Big pixels** — makes the blur finer or coarser (lower = blockier pixelation)
- **Confidence threshold** — how sensitive tracking is to movement (0.1–1.0)

---

## Saving your work

Save the project to disk as a `*.sft` file. Open it later with the original video and everything's ready to export — no re-tracking needed.

See [TODO.md](TODO.md) for planned features and bug fixes.

---

## Requirements

- **Windows 10 or later**
- Run: `dotnet run --project main/ShutterFace.csproj`

---

## License

This project is licensed under [GNU AFFERO GENERAL PUBLIC LICENSE Version 3 (AGPL-3.0)](LICENSE).

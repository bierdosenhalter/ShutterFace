# Project

MotionTrackerFaceBlur — WinForms desktop app, tracks face regions in videos via template matching, exports blurred output. C# / .NET 8.

- **Stack**: OpenCvSharp4 + runtime.win
- **State**: tracking + export working; no re-analysis needed after `.track` load

# Build & Run

```
dotnet build    # from repo root
dotnet run      # or via Visual Studio (.sln)
```

> Tests in IDE (Test Explorer). CLI: open MotionTrackerFaceBlur.sln, select test project.

# Conventions

- WinForms UI; slider for frame scrubbing + time setter
- Comments and UI text in German
- `Rect` is a struct — use `.X`, `.Y`, etc directly (not `.Value`)

# After every change: build & fix

- Run `dotnet build`; never leave the build broken

# Known issues

- Frame boundary issue: out-of-bounds rectangles must be clipped to visible bounds before passing to OpenCvSharp (see TODO #1)

# Known constraints

- Don't touch `.vs/`, build output, sample videos

# Git

- `git add -A && git commit` after each task; never push/rebase/rewrite
- Never commit without my confirmation

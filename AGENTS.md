# Project

MotionTrackerFaceBlur — WinForms desktop app, tracks face regions in videos via template matching, exports blurred output. C# / .NET 8.

- **Stack**: OpenCvSharp4 + runtime.win
- **State**: tracking + export working; no re-analysis needed after `.track` load

# Build & Run

```
dotnet build    # from repo root
dotnet run      # or via Visual Studio (.sln)
```

# Conventions

- WinForms UI; slider for frame scrubbing + time setter
- Comments and UI text in German
- `Rect` is a struct — use `.X`, `.Y`, etc directly (not `.Value`)

# Definition of done (every change)

Before showing me any result, run this loop from repo root:

1. `dotnet build` — zero errors, zero warnings
2. `dotnet format --verify-no-changes` — if it reports issues, run `dotnet format` once, then rebuild
3. `dotnet test` — all tests pass (if a test project exists)

Rules for this loop:

- Fix each warning/error at its cause, not by deleting or disabling the code that triggers it. Never suppress with `#pragma` or `#pragma warning disable` without asking me first.
- When a fix is ambiguous, prefer the minimal change. Ask me instead of guessing when behavior could change.
- Rebuild after each fix. Stop only when the loop is fully clean.
- If a failure cannot be fixed without changing behavior, stop and report it to me instead of forcing it.

# Known constraints

- Don't touch `.vs/`, build output, sample videos

# Git

- `git add -A && git commit` after each task; never push/rebase/rewrite
- Never commit without my confirmation

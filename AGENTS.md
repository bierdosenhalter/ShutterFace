# MotionTrackerFaceBlur

WinForms app (C#/.NET 8, OpenCvSharp4): face tracking via template matching + blurred video export.

## Environment
Windows, PowerShell: no bash syntax (`;` not `&&`), backslash paths, `%USERPROFILE%` not `~`. CRLF, never convert. Build via `dotnet`, VS not required.

## Build & DoD (every change)
```
dotnet build        # zero errors AND warnings
dotnet format --verify-no-changes   # if dirty: dotnet format
dotnet test         # all pass (if test project exists)
```
Fix warnings at their cause, no `#pragma`. Rebuild after each fix; stop only when clean. Minimal changes; behavior would change → stop and ask.

## Web research
Verify uncertain OpenCvSharp/.NET/WinForms APIs via `context7`/`websearch` first; on "API does not exist" errors check real docs. `webfetch` official docs only; summarize in 1–2 sentences, never paste excerpts. Never invent NuGet names/versions — search, give exact `dotnet add package` command.

## Working style
- Plan (3–6 steps) then execute without approval; decide minor things yourself, ask only if irreversible or approach-changing; note decisions in commit message
- On failure: 2 alternatives before reporting
- Never end with "shall I continue?" — run DoD loop, report done + next step

## Constraints
- Never touch `.vs/` or build output
- No push/rebase/history rewrite; commit only with my confirmation
- 32k context: small targeted edits, no full-file rewrites unless asked, summarize instead of echoing files
- One task per session; unrelated task → suggest fresh session

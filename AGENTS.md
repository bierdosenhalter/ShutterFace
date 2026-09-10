# ShutterFace

WinForms app (C#/.NET 8, OpenCvSharp4): face tracking via template matching + blurred video export.

## Build & DoD (every change)
```
dotnet build --no-incremental -v:normal /p:EnforceCodeStyleInBuild=true /p:AnalysisLevel=latest-all        # zero errors AND warnings, style warnings are ok
dotnet format --verify-no-changes   # if dirty: dotnet format
dotnet format analyzers --verify-no-changes   # if dirty: dotnet format analyzers
roslynator analyze --severity-level info
dotnet test         # all pass (if test project exists)
```
Fix at cause, no #pragma. Roslynator refactorings: list first, apply on my OK.

## WinForms
- UI edits go into `Xxx.Designer.cs` in its existing pattern; never delete designer sections
- Spacing grid: 11 margin/padding, 7 between controls, 4 label↔input; buttons bottom-right right-to-left OK/Cancel/Apply
- AutoScaleMode.Dpi + Padding/Margin only, no absolute Locations

## Working style
- Plan with `todowrite` before coding (3–6 items), execute item by item, update statuses; never mark done without DoD passing
- Uncertain API/behavior: verify via context7/websearch first; ask me only if research fails, the choice is irreversible, or GUI appearance needs a visual check (one proposal, then wait)
- On failure: 2 alternatives before reporting
- Never end with "shall I continue?" — run DoD loop, report done, commit + next step

## Versioning
- Bump the assembly/project version with every commit (patch increment)

## Constraints
- Never touch `.vs/` or build output; no push/rebase; commit only on my OK
- Context is 32k: targeted edits only, one task per session

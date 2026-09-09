# ShutterFace

WinForms app (C#/.NET 8, OpenCvSharp4): face tracking via template matching + blurred video export.

## Environment
Windows, PowerShell: no bash syntax (`;` not `&&`), backslash paths, `%USERPROFILE%` not `~`. CRLF, never convert. Build via `dotnet`, VS not required.

## Build & DoD (every change)
```
dotnet build --no-incremental -v:normal /p:EnforceCodeStyleInBuild=true /p:AnalysisLevel=latest-all        # zero errors AND warnings, style warnings are ok
dotnet format --verify-no-changes   # if dirty: dotnet format
dotnet format analyzers --verify-no-changes   # if dirty: dotnet format analyzers
roslynator analyze --severity-level info
dotnet test         # all pass (if test project exists)
```
Fix at cause, no #pragma. Roslynator refactorings: list first, apply on my OK.

## WinForms forms
UI changes go into `Xxx.Designer.cs` following its existing pattern (field + instantiate + properties + add to parent). Never hand-roll UI in constructors or delete designer sections — the VS Designer must keep loading the form.

## UI layout
Designer.cs values are 96-DPI units. Grid: 11 units margin/padding, 7 units between controls, 4 units label↔input. Buttons bottom-right, right-to-left (OK/Cancel/Apply), 7-unit rows. Heights: let the font decide; use Padding/Margin + AutoScaleMode.Dpi, never absolute Locations.

## Working style
- Plan with `todowrite` before coding (3–6 items), execute item by item, update statuses; never mark done without DoD passing
- Uncertain API/behavior: verify via context7/websearch, never invent NuGet names/versions. Ask me only if research fails or choice is irreversible/approach-changing
- On failure: 2 alternatives before reporting
- Never end with "shall I continue?" — run DoD loop, report done + next step

## Versioning
- Bump the assembly/project version with every commit (patch increment)

## Constraints
- Never touch `.vs/` or build output
- No push/rebase/history rewrite; commit only with my confirmation
- 32k context: small targeted edits, no full-file rewrites unless asked, summarize instead of echoing files
- One task per session; unrelated task → suggest fresh session

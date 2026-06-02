# [H1][RASM_APPUI_AGENTS]
>**Dictum:** *Build product UI intent; delegate native behavior.*

<br>

[CRITICAL] `Rasm.AppUi` is docs-only until a future implementation explicitly creates a project and production source. Do not add package references, `.csproj`, or C# files from this folder contract alone.

---
## [1][OWNER_CONTRACT]
>**Dictum:** *One public app-surface rail beats toolkit wrappers.*

<br>

- Use one typed app-surface operation rail for shell, screen, command, live-view, visual, and diagnostic concerns.
- Keep Avalonia, ReactiveUI, DynamicData, SkiaSharp, and ImGui.NET types internal until a real boundary requires exposure.
- Treat `ReactiveUI.Avalonia` as an adapter candidate to reselect at first consumer.
- Do not reserve pseudo-API names before production source lands.

---
## [2][HOST_BOUNDARY]
>**Dictum:** *Host execution belongs to host owners.*

<br>

| [INDEX] | [DO_NOT_DUPLICATE] | [OWNER] |
| :-----: | ------------------ | ------- |
|   [1]   | UI-thread dispatch, parent windows, Rhino document affinity | `Rasm.Rhino.UI` |
|   [2]   | Rhino panels, window specs, repaint, undo grouping | `Rasm.Rhino.UI` |
|   [3]   | GH2 canvas/editor/document scope | `Rasm.Grasshopper.UI` |
|   [4]   | GH2 subscriptions, repaint requests, paint hooks | `Rasm.Grasshopper.UI` |
|   [5]   | Runtime scheduling, drain, telemetry correlation | `Rasm.AppHost` |
|   [6]   | Durable state, migrations, support artifacts | `Rasm.Persistence` |

AppUi may emit product UI intent and receipts. Final mutation runs through the owner rails above.

---
## [3][EVIDENCE]
>**Dictum:** *Docs name evidence categories; source slices produce evidence.*

<br>

Documentation edits require manual consistency only. Executable proof begins when source, package references, or host scenarios land.

Future AppUi evidence categories:

- RhinoWIP macOS load and unload.
- GH2 editor/canvas coexistence.
- Focus, keyboard, z-order, parent identity.
- Retina/logical-pixel scaling.
- Native asset layout and disposal.
- Screenshot/support-bundle receipt.

---
## [4][REJECTIONS]
>**Dictum:** *Wrapper APIs are not platform APIs.*

<br>

- No Avalonia, ReactiveUI, DynamicData, SkiaSharp, or ImGui forwarding surface.
- No AppUi-owned Rhino/GH2 dispatch, repaint, undo, or lifecycle rail.
- No mixed MVVM idioms in one screen stack.
- No active package claim without project reference and owner-local evidence.

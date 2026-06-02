# [H1][RASM_APPUI]
>**Dictum:** *AppUi expresses product intent; Rhino and GH2 own host execution.*

<br>

`Rasm.AppUi` is the planned advanced product UI platform for future Rasm plugins and apps. It is docs-only today: no `.csproj`, no production C# files, no active package references, and no runtime surface.

---
## [1][PURPOSE]
>**Dictum:** *Future UI code should compose one app surface, not copy host widgets.*

<br>

`Rasm.AppUi` will provide a single app-surface rail for windows, panels, screens, command state, live projections, custom visuals, diagnostics, and support views. It will aggregate product UI intent and typed receipts, then delegate final host behavior to existing `Rasm.Rhino.UI` and `Rasm.Grasshopper.UI` rails.

It is not an Avalonia wrapper, ReactiveUI wrapper, Skia wrapper, ImGui wrapper, Eto replacement, Rhino panel clone, or GH2 canvas abstraction.

---
## [2][STATUS]
>**Dictum:** *Candidate packages are not runtime capability.*

<br>

| [INDEX] | [SURFACE] | [STATE] |
| :-----: | --------- | ------- |
|   [1]   | Project file | Not created |
|   [2]   | Production API | Not created |
|   [3]   | Package references | None |
|   [4]   | Runtime behavior | Unproven |
|   [5]   | Host proof | Pending future implementation |

Candidate UI packages must be rechecked for latest stable versions immediately before a real consumer lands.

---
## [3][MANUAL]
>**Dictum:** *Read the owner manual before inventing UI surface.*

<br>

| [INDEX] | [FILE] | [READ_FOR] |
| :-----: | ------ | ---------- |
|   [1]   | `_ARCHITECTURE.md` | Rail contract, host delegation, package candidates, proof states |
|   [2]   | `AGENTS.md` | Future-agent rules and forbidden duplicate host rails |
|   [3]   | `ROADMAP.md` | First slice, deferred work, promotion criteria |

---
## [4][NON_CLAIMS]
>**Dictum:** *The docs reserve intent, not API signatures.*

<br>

- No public API shape is reserved.
- No candidate package is active.
- No RhinoWIP or GH2 runtime behavior is proven.
- No UI code should bypass `Rasm.Rhino.UI` or `Rasm.Grasshopper.UI`.
- No ViewModel rail may mix ReactiveUI and another MVVM idiom in the same screen stack.

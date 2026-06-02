# [H1][RASM_APPUI_ROADMAP]
>**Dictum:** *The first UI slice proves host coexistence before visual ambition.*

<br>

`Rasm.AppUi` remains documentation-only. This roadmap sequences future implementation; it does not authorize package pins or source creation by itself.

---
## [1][READINESS]
>**Dictum:** *A UI package starts with a consumer, not a package list.*

<br>

Start implementation only when a concrete plugin/app needs a product UI surface that existing `Rasm.Rhino.UI` or `Rasm.Grasshopper.UI` cannot express alone.

Required decisions before source:

- Host target: Rhino window/panel, GH2 editor/canvas surface, or companion desktop window.
- First screen intent and command receipt.
- Host rail used for dispatch, repaint, parent identity, and disposal.
- Candidate package versions refreshed from latest stable source.

---
## [2][FIRST_SLICE]
>**Dictum:** *Shell-only proof comes before navigation, live data, and custom rendering.*

<br>

First slice: product shell intent lowered through existing Rhino/GH2 UI rails.

| [INDEX] | [LANDS] | [DEFERRED] |
| :-----: | ------- | ---------- |
|   [1]   | One shell/screen operation | Multi-screen navigation |
|   [2]   | One command outcome receipt | Command palette and shortcuts |
|   [3]   | Host parent/focus/disposal receipts | Skia visuals and ImGui overlay |
|   [4]   | ReactiveUI activation policy if needed | DynamicData live dashboards |

---
## [3][DONE_WHEN]
>**Dictum:** *Promotion requires host evidence, not desktop optimism.*

<br>

The first slice is complete when owner-local receipts identify the host, parent, focus behavior, command result, disposal path, and screenshot/support evidence. Runtime claims remain scoped to the proven host scenario.

---
## [4][DEFERRED_UNTIL]
>**Dictum:** *Advanced packages wait for advanced proof.*

<br>

| [INDEX] | [DEFERRED] | [UNBLOCKS_WHEN] |
| :-----: | ---------- | --------------- |
|   [1]   | Avalonia retained app surface | Host coexistence and adapter proof exist |
|   [2]   | DynamicData live projections | AppHost or Persistence emits real live state |
|   [3]   | SkiaSharp visuals | Native asset, Retina, screenshot, disposal proof exists |
|   [4]   | ImGui.NET debug overlay | Renderer/input/shutdown proof exists |
|   [5]   | ReactiveUI.Avalonia adapter | Latest stable compatible adapter is selected at consumer time |

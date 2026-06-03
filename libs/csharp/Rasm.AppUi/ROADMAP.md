# [H1][RASM_APPUI_ROADMAP]
>**Dictum:** *Scaffold the foundation, then consume the pinned packages as one surface.*

<br>

This roadmap sequences the build. The platform integrates all packages together from the foundation; it does not drip capability in behind separate consumers.

---
## [1][PHASE_0]
>**Dictum:** *Housekeeping lands and compiles before heavy work.*

<br>

- Use the existing root `Directory.Packages.props` AppUi matrix. `PackageReference` entries stay versionless. Do not repin or add package-manager aliases in this pass. Coupled version matrix: Avalonia ↔ ReactiveUI.Avalonia ↔ ReactiveUI ↔ DynamicData ↔ System.Reactive ↔ SkiaSharp (Avalonia-bundled, LiveCharts2-aligned). Target `net10.0` so System.Reactive pulls no WPF/WinForms facade.
- SkiaSharp gate (§4.3): confirm Rhino's bundled `libSkiaSharp` native major. If it matches, reference `SkiaSharp.NativeAssets.macOS` with `<ExcludeAssets>native</ExcludeAssets>` to share Rhino's loaded copy; a mismatched major is a hard build gate (same-named dylibs cannot co-load). Carry `HarfBuzzSharp.NativeAssets.macOS` unconditionally (not bundled by Rhino).
- `Rasm.AppUi.csproj` targets `net10.0`, is wired into `Workspace.slnx`, restores/builds, and resolves host assemblies as the sibling libs do.
- Scaffold the unified folder skeleton and canonical section order: `Shell.cs`, `Screen.cs`, `Command.cs`, `Live.cs`, `Visual.cs`, `Chart.cs`, `Diagnostic.cs`.
- Land `RasmUiScheduler` (Avalonia `Dispatcher` + ReactiveUI `RxApp.MainThreadScheduler`) — the one canonical UI scheduler boundary — before any live-projection work.
- Set `MacOSPlatformOptions.DisableAvaloniaAppDelegate = true` in the bootstrap; use `CreateEmbeddableTopLevel()`.
- Validate Avalonia embedding with Software rendering before Metal.

Phase 0 restore/build foundation is complete. Source scaffolding and runtime embedding proof remain.

---
## [2][INTEGRATION]
>**Dictum:** *One unified rail composes every package.*

<br>

Build the single typed app-surface rail and compose the packages into one paradigm:

| [INDEX] | [SURFACE]                  | [PACKAGE_ROLE]                                                                                        |
| :-----: | -------------------------- | ----------------------------------------------------------------------------------------------------- |
|   [1]   | Scheduler spine            | `RasmUiScheduler`: Avalonia `Dispatcher` + ReactiveUI scheduler + host-thread affinity                |
|   [2]   | Shell and screen rail      | Avalonia retained surface, ReactiveUI activation and commands, `ReactiveUI.Validation` Screen surface |
|   [3]   | ViewModel adapter          | `ReactiveUI.Avalonia`                                                                                 |
|   [4]   | Live projection            | DynamicData change-sets, observed on `RasmUiScheduler`, read-only projection                          |
|   [5]   | Custom visuals             | SkiaSharp thumbnails/offscreen only; viewport overlays via the Rhino/GH display conduit               |
|   [6]   | Charts and dashboards      | `LiveChartsCore.SkiaSharpView.Avalonia` retained data-viz on SkiaSharp                                |
|   [7]   | Tabular surfaces           | `Avalonia.Controls.DataGrid`                                                                          |
|   [8]   | Inspector surfaces         | `bodong.Avalonia.PropertyGrid`; custom editors remain the documented fallback (single-maintainer package) |
|   [9]   | Color surfaces             | `Avalonia.Controls.ColorPicker`                                                                       |
|  [10]   | Event triggers / drag-drop | `Xaml.Behaviors.Avalonia`                                                                             |
|  [11]   | Icon glyphs                | `Projektanker.Icons.Avalonia` + `Projektanker.Icons.Avalonia.MaterialDesign`                          |
|  [12]   | SVG icons / assets         | `Svg.Controls.Skia.Avalonia`                                                                          |
|  [13]   | In-panel dialogs           | `DialogHost.Avalonia` — no NSWindow                                                                   |
|  [14]   | Text shaping               | `SkiaSharp.HarfBuzz` + `HarfBuzzSharp.NativeAssets.macOS`                                             |

Command outcomes return `CommandReceipt`. Host execution lowers through `Rasm.Rhino/UI` and `Rasm.Grasshopper/UI`. The scheduler spine precedes live projection; DynamicData change-sets observe on `RasmUiScheduler` before binding.

---
## [3][HIGHER_ORDER]
>**Dictum:** *Higher-order capability rides on the unified rail.*

<br>

On the integrated foundation, build the higher-order product-UI capabilities a plugin needs — each as parameterized capability on the one rail, not a parallel subsystem:

| [INDEX] | [CAPABILITY]                | [BUILDS_ON]                                                                              |
| :-----: | --------------------------- | ---------------------------------------------------------------------------------------- |
|   [1]   | Multi-screen navigation     | `Shell` route + `NavStack`; `ReactiveUI.Avalonia` routing                                |
|   [2]   | Command palette / shortcuts | `Xaml.Behaviors.Avalonia` key-binding; scoped per `Screen`                               |
|   [3]   | Live dashboards             | `LiveChartsCore.SkiaSharpView.Avalonia`; series updates on `RasmUiScheduler`             |
|   [4]   | Viewport overlays           | Delegated to Rhino/GH display conduit — NOT Avalonia or AppUi SkiaSharp                  |
|   [5]   | Settings / preferences      | `Screen<PreferencesModel>` + custom editor surface; persisted via Persistence            |
|   [6]   | Notifications / toasts      | `DialogHost.Avalonia` in-panel host; no NSWindow                                         |
|   [7]   | Undo-redo surfacing         | Observe host undo availability from `Rasm.Rhino/UI` rail; `CanExecute` on `Command`      |
|   [8]   | Theming                     | `HostUtils.RunningInDarkMode` + reactive `ThemeVariant` on `WhenActivated`               |
|   [9]   | Drag-drop                   | Avalonia drag-drop + `Xaml.Behaviors.Avalonia`; routed through Eto for host-panel drops  |
|  [10]   | Clipboard                   | Avalonia `IClipboard` injected into `Screen`; no static access                           |
|  [11]   | Accessibility               | `AutomationProperties` on every interactive control; `AutomationPeer` for custom visuals |

---
## [4][EMBEDDING_VALIDATION]
>**Dictum:** *Embed correctly before shipping any panel.*

<br>

| [INDEX] | [STEP]               | [REQUIREMENT]                                                                   |
| :-----: | -------------------- | ------------------------------------------------------------------------------- |
|   [1]   | Software rendering   | Validate embedding with Software rendering before Metal path                    |
|   [2]   | NSView reparenting   | P/Invoke `objc_msgSend` shim; verify native handle via `TryGetPlatformHandle()` |
|   [3]   | Focus coordination   | Resign first responder on `PanelHidden`; restore on `PanelShown`                |
|   [4]   | Retina scale refresh | Handle `NSWindowDidChangeBackingProperties` + `WhenActivated`                   |
|   [5]   | Disposal order       | `Content = null` → await `TopLevel.Closed` → base dispose                       |
|   [6]   | GH2 embedding        | [DEFERRED] — no GH2 dockable panel-host API in current RhinoWIP; Rhino-panel embedding is supported. Trigger: `api query gh2 Panel` per WIP drop |

---
## [5][RUNTIME_EVIDENCE]
>**Dictum:** *Host claims require host evidence.*

<br>

Runtime claims are scoped to proven host scenarios. Owner-local `DiagnosticReceipt` records identify the host, parent handle, focus behavior, command result, disposal path, GPU/frame-pacing coexistence with the viewport, and screenshot/support evidence for RhinoWIP macOS load and GH2 coexistence.

---
## [6][PHASE_0_GATES]
>**Dictum:** *These need a machine with RhinoWIP + restore; resolve before heavy work.*

<br>

Items that could not be settled from research alone — each needs an agent with the macOS host (RhinoWIP.app) and/or `dotnet restore`. Each row gives the exact command, the decision it gates, and the action per outcome.

ALREADY CONFIRMED (no action): central AppUi package pins exist; `Rasm.AppUi.csproj` restores/builds in `Workspace.slnx`; `SkiaSharp` is aligned with `Avalonia.Skia`; RhinoWIP 9 ships no managed `SkiaSharp.dll` (bridge-verify artifact); GH2 has no plugin-panel host API (decompiled `Grasshopper2.dll`); `DisableAvaloniaAppDelegate`/`CreateEmbeddableTopLevel` confirmed from Avalonia source.

### [6.1][RESTORE_GATES] — need `dotnet restore` on the AppUi project

| [INDEX] | [GATE] | [COMMAND] | [DECISION / ACTION] |
| :-----: | ------ | --------- | ------------------- |
|   [1]   | Whole matrix resolves clean | `uv run python -m tools.quality static build libs/csharp/Rasm.AppUi` | Master gate. Current project restores/builds the active matrix. |
|   [2]   | `Avalonia.Controls.ColorPicker` stable line | `uv run python -m tools.quality api resolve Avalonia.Controls.ColorPicker all` | Keep direct reference only while restore/build stays clean. |
|   [3]   | `Xaml.Behaviors.Avalonia` v12 exists | `uv run python -m tools.quality api resolve Xaml.Behaviors.Avalonia all` | Pin tracks `Directory.Packages.props`. If latest is the 11.x line → find the Avalonia-12 build or hold; do not pin an Avalonia-11 package into the 12 matrix. |
|   [4]   | `ReactiveUI.Validation` ↔ ReactiveUI | `uv run python -m tools.quality api resolve ReactiveUI.Validation all` | If it forces a ReactiveUI downgrade → find a newer Validation or drop it for hand-rolled `INotifyDataErrorInfo`. |
|   [5]   | `bodong.Avalonia.PropertyGrid` resolves clean | `uv run python -m tools.quality api resolve bodong.Avalonia.PropertyGrid all` | Pinned and referenced; inspector surface consumes it. Custom editors remain the documented fallback (single-maintainer package). |
|   [6]   | Transitive pins (CPM transitive pinning ON) | read restore errors | Add only transitive `PackageVersion` entries required by a concrete direct consumer. |
|   [7]   | `Splat` floor for ReactiveUI.Avalonia | restore | `ReactiveUI.Avalonia` (version per `Directory.Packages.props`) carries a `Splat` floor; if CPM transitive pinning complains, add a `Splat` pin. |

### [6.2][RHINO_NATIVE_GATES] — need RhinoWIP.app on macOS

| [INDEX] | [GATE] | [COMMAND] | [DECISION / ACTION] |
| :-----: | ------ | --------- | ------------------- |
|   [8]   | Rhino's bundled `libSkiaSharp` major (the §4.3 gate) | `find /Applications/RhinoWIP.app -name 'libSkiaSharp*'` then `otool -L <path>`, or a `bridge verify` scenario printing `SkiaSharpVersion.Native` | If major matches the central SkiaSharp pin: reference `SkiaSharp.NativeAssets.macOS` with `<ExcludeAssets>native</ExcludeAssets>` and ship no second dylib. If it differs: hard build gate — Avalonia cannot downgrade Skia; render Avalonia/Skia offscreen or out-of-process, or escalate. |
|   [9]   | Rhino does NOT bundle HarfBuzz | `find /Applications/RhinoWIP.app -name 'libHarfBuzz*'` | We assume not, so `HarfBuzzSharp.NativeAssets.macOS` is carried unconditionally. If Rhino does bundle it and the major differs → apply the same §4.3 logic to HarfBuzz. |
|  [10]   | Re-confirm Rhino managed surface | `uv run python -m tools.quality api doctor` | Should list RhinoCommon/Rhino.UI/Eto/Grasshopper2 with no `SkiaSharp.dll` — confirms the §4.3 ground truth on the current WIP drop. |

### [6.3][EMBEDDING_RUNTIME_PROOFS] — need RhinoWIP.app; produce `DiagnosticReceipt` evidence

Run the `[4][EMBEDDING_VALIDATION]` sequence in order, software rendering first: NSView reparent via `objc_msgSend` (assert `TryGetPlatformHandle()` non-null) → `DisableAvaloniaAppDelegate` load-without-crash → focus resign/restore on panel hide/show → Retina scale on display change → disposal order (`Content = null` → `TopLevel.Closed` → dispose) → only then promote to Metal and prove frame-pacing coexistence with the viewport.

### [6.4][GH2_WATCH] — low priority, future WIP drops

| [INDEX] | [GATE] | [COMMAND] | [DECISION / ACTION] |
| :-----: | ------ | --------- | ------------------- |
|  [11]   | GH2 gains a plugin-panel host API | `uv run python -m tools.quality api query gh2 Panel` per WIP drop | If a dockable panel-host type appears → revisit GH2-Avalonia embedding (currently DEFERRED). Until then, Rhino-panel embedding is the only supported path. |

# [RASM_APPUI_API_PRODIAGNOSTICS]

`ProDiagnostics` is the maintained Avalonia-12 developer-tools UI — a MIT fork (wieslawsoltes, `github.com/wieslawsoltes/ProDataGrid`) of `Avalonia.Diagnostics` shipping under the ORIGINAL assembly and namespace (`Avalonia.Diagnostics.dll`, `Avalonia`/`Avalonia.Diagnostics`), so `this.AttachDevTools()` binds unchanged. It mounts a runtime inspector overlaying the live window: visual/logical/combined tree navigation, live property + style inspection with in-place value editing, routed-event tracking, and layout/renderer diagnostics overlays. The first-party `Avalonia.Diagnostics` line is feed-dead at 11.3.x with no Avalonia-12 asset; the pay-tiered Accelerate DevTools is license-gate REJECTED — `ProDiagnostics` is the sole admitted Avalonia-12 devtools owner. `Diagnostics/devloop.md` composes it as the dev-loop inspection surface, `Debug`-gated `PrivateAssets="all"` beside `HotAvalonia` (`csproj` Dev Loop group).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ProDiagnostics`
- package: `ProDiagnostics` (MIT, Wiesław Šoltés)
- assembly: `Avalonia.Diagnostics` (`lib/net10.0/Avalonia.Diagnostics.dll` binds the `net10.0` consumer directly; `net8.0` fallback asset)
- namespace: `Avalonia` (the `DevToolsExtensions` attach surface), `Avalonia.Diagnostics` (`DevToolsOptions`/`DevToolsViewKind`/`HotKeyConfiguration`/`Conventions`/`DevToolsSession`/`VisualExtensions`/`VisualTreeDebug`), `Avalonia.Diagnostics.Services` (`PropertyValueEditorService`)
- depends: `Avalonia` (12.x), `Avalonia.Controls.ColorPicker` (the inspector's color-value editor) — both already admitted; the `ProDataGrid`/`ProCharts` sibling packages are NOT admitted
- binding: `Debug`-only, `PrivateAssets="all"` — never flows to a downstream consumer, absent from the Release surface
- rail: dev-loop-inspection

## [02]-[PUBLIC_TYPES]

[INSPECTOR_OPTIONS]: `Avalonia.Diagnostics` attach configuration
- rail: dev-loop-inspection

| [INDEX] | [SYMBOL]                       | [KIND]                      |
| :-----: | :----------------------------- | :-------------------------- |
|  [01]   | `DevToolsOptions`              | attach configuration        |
|  [02]   | `DevToolsViewKind`             | launch-view enum            |
|  [03]   | `HotKeyConfiguration`          | hotkey configuration        |
|  [04]   | `IScreenshotHandler`           | pluggable capture sink      |
|  [05]   | `IDevToolsPropertyEditHandler` | property-commit interceptor |
|  [06]   | `Conventions`                  | static defaults holder      |

[DEVTOOLS_OPTIONS_LAUNCH]: `Gesture` defaults to `F12`; init-only `LaunchView : DevToolsViewKind` defaults to `CombinedTree`.

[DEVTOOLS_OPTIONS_WINDOW]: `Size` defaults to 1280×720; `ShowAsChildWindow`, `StartupScreenIndex : int?`, `ShowImplementedInterfaces`,
`ThemeVariant : ThemeVariant?`, and `FocusHighlighterBrush : IBrush?` govern window presentation.

[DEVTOOLS_OPTIONS_TABS]: `ShowMenu`, `ShowResourcesTab`, `ShowAssetsTab`, `ShowEventsTab`, and `ScopeEventsToRoot` govern tab and event scope.

[DEVTOOLS_OPTIONS_EXTENSIONS]: `ScreenshotHandler : IScreenshotHandler` and `PropertyEditHandler : IDevToolsPropertyEditHandler?` bind extension seams;
init-only `HotKeys : HotKeyConfiguration` binds the gesture rig.

[DEVTOOLS_VIEW_KIND]: `LogicalTree`, `VisualTree`, `Events`, `CombinedTree`, `Resources`, and `Assets` select the launch view.

[HOTKEY_CONFIGURATION]: `ValueFramesFreeze`, `ValueFramesUnfreeze`, `InspectHoveredControl`, `TogglePopupFreeze`, and
`ScreenshotSelectedControl` each carry a `KeyGesture`.

[SCREENSHOT_HANDLER]: `Conventions.DefaultScreenshotHandler` is the built-in file-picker handler.

[PROPERTY_EDIT_HANDLER]: `IDevToolsPropertyEditHandler` gates and records live property edits.

[CONVENTIONS]: `DefaultScreenshotHandler` carries the static screenshot default.

[INSPECTOR_RUNTIME]: session + render internals
- rail: dev-loop-inspection

| [INDEX] | [SYMBOL]                                        | [KIND]                 |
| :-----: | :---------------------------------------------- | :--------------------- |
|  [01]   | `DevTools` / `DevToolsSession` / `DevToolsView` | inspector runtime      |
|  [02]   | `PropertyValueEditorService`                    | property-editor engine |
|  [03]   | `VisualTreeDebug`                               | diagnostics overlay    |
|  [04]   | `VisualExtensions`                              | snapshot extensions    |

[DEVTOOLS_RUNTIME]: `DevTools`, `DevToolsSession`, and `DevToolsView` own the overlay window, per-`TopLevel` session, and root view; the
attach surface mounts them, and consumers do not construct them.

[PROPERTY_VALUE_EDITOR_SERVICE]: The live property and style value-editor engine carries typed converters and the commit-state machine
behind the property pane.

[VISUAL_TREE_DEBUG]: `VisualTreeDebug` projects the layout and renderer diagnostics overlay.

[VISUAL_EXTENSIONS]: `RenderTo(this Control, Stream, double dpi = 96.0)` writes the control snapshot consumed by the screenshot handler.

## [03]-[ENTRYPOINTS]

[ATTACH_SURFACE]: `Avalonia.DevToolsExtensions` — one polymorphic attach fold over `TopLevel`/`Application`
- rail: dev-loop-inspection

| [INDEX] | [SURFACE]                                                               | [SURFACE_ROOT]       | [RAIL]                               |
| :-----: | :---------------------------------------------------------------------- | :------------------- | :----------------------------------- |
|  [01]   | `AttachDevTools(this TopLevel root)`                                    | `DevToolsExtensions` | default-gesture window attach        |
|  [02]   | `AttachDevTools(this TopLevel root, KeyGesture gesture)`                | `DevToolsExtensions` | custom-gesture window attach         |
|  [03]   | `AttachDevTools(this TopLevel root, DevToolsOptions options)`           | `DevToolsExtensions` | full-config window attach            |
|  [04]   | `AttachDevTools(this Application application)`                          | `DevToolsExtensions` | app-lifetime attach (all top-levels) |
|  [05]   | `AttachDevTools(this Application application, DevToolsOptions options)` | `DevToolsExtensions` | full-config app attach               |
|  [06]   | `RenderTo(this Control source, Stream destination, double dpi = 96.0)`  | `VisualExtensions`   | control-snapshot capture             |

## [04]-[IMPLEMENTATION_LAW]

[DEVLOOP_LAW]:
- Package: `ProDiagnostics`
- Owns: the Avalonia-12 runtime inspector overlay — the visual/logical/combined tree navigator, the live property + style pane with in-place editing (`PropertyValueEditorService`), routed-event tracking (`ShowEventsTab`/`ScopeEventsToRoot`), and the layout/renderer diagnostics overlays (`VisualTreeDebug`); the `DevToolsViewKind` launch view, `HotKeyConfiguration` gestures, and `IScreenshotHandler`/`IDevToolsPropertyEditHandler` extension seams.
- Accept: `Diagnostics/devloop.md` mounts the inspector through `AttachDevTools(this Application, DevToolsOptions)` at composition time under the `Debug` gate; the `DevToolsOptions` launch view / tab visibility / hotkey rig are dev-loop policy rows; a custom `IScreenshotHandler` routes captures into the proof/capture lane where devloop needs deterministic snapshots.
- Reject: a Release-surface reference (the `PrivateAssets="all"` `Debug` gate is law — no devtools identity in the shipped product); a second Avalonia devtools binding (`Avalonia.Diagnostics` 11.3.x is feed-dead, Accelerate DevTools is license-gate REJECTED); re-implementing the tree/property inspector where this overlay owns it; a hand-rolled control-snapshot where `VisualExtensions.RenderTo` exists.

[STACKING]:
- `Diagnostics/devloop.md` is the sole consumer anchor — the HUD/hot-reload/replay-verify dev loop mounts this inspector beside `HotAvalonia`'s XAML hot-reload; the two share the `Debug` `PrivateAssets` gate and never co-mount in Release.
- Catalog depth is stub-plus-verified: every member above is decompile-verified against the restored `Avalonia.Diagnostics.dll`. The `DevToolsSession`/`PropertyValueEditorService` internals deepen when `devloop.md` first composes the option rig and the screenshot/property-edit handler seams into a concrete dev-loop policy fold.

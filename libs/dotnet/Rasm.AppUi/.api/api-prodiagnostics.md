# [RASM_APPUI_API_PRODIAGNOSTICS]

`ProDiagnostics` is the maintained Avalonia-12 developer-tools UI — a MIT fork (wieslawsoltes, `github.com/wieslawsoltes/ProDataGrid`) of `Avalonia.Diagnostics` shipping under the original assembly and namespace (`Avalonia.Diagnostics.dll`, `Avalonia`/`Avalonia.Diagnostics`), so `this.AttachDevTools()` binds unchanged. It mounts visual/logical tree navigation, live property and style editing, routed-event tracking, and layout/renderer overlays on the live window. Feed-dead first-party `Avalonia.Diagnostics` has no Avalonia-12 asset, and pay-tiered Accelerate DevTools fails license admission; `ProDiagnostics` therefore owns the `Debug`-gated dev-loop inspection surface beside `HotAvalonia`.

## [01]-[PUBLIC_TYPES]

[INSPECTOR_OPTIONS]: `Avalonia.Diagnostics` attach configuration
- concern: dev-loop-inspection

| [INDEX] | [SYMBOL]                        | [KIND]                      |
| :-----: | :------------------------------ | :-------------------------- |
|  [01]   | `DevToolsOptions`               | attach configuration        |
|  [02]   | `DevToolsViewKind`              | launch-view enum            |
|  [03]   | `HotKeyConfiguration`           | hotkey configuration        |
|  [04]   | `IScreenshotHandler`            | pluggable capture sink      |
|  [05]   | `IDevToolsPropertyEditHandler`  | property-commit interceptor |
|  [06]   | `DevToolsPropertyEdit`          | property-commit record      |
|  [07]   | `DevToolsResourceReferenceKind` | resource-reference enum     |

[DEVTOOLS_OPTIONS_LAUNCH]: `Gesture` defaults to `F12`; init-only `LaunchView : DevToolsViewKind` defaults to `CombinedTree`.

[DEVTOOLS_OPTIONS_WINDOW]: `Size` defaults to 1280×720; `ShowAsChildWindow`, `StartupScreenIndex : int?`, `ShowImplementedInterfaces`, `ThemeVariant : ThemeVariant?`, and `FocusHighlighterBrush : IBrush?` govern window presentation.

[DEVTOOLS_OPTIONS_TABS]: `ShowMenu`, `ShowResourcesTab`, `ShowAssetsTab`, `ShowEventsTab`, and `ScopeEventsToRoot` govern tab and event scope.

[DEVTOOLS_OPTIONS_EXTENSIONS]: `ScreenshotHandler : IScreenshotHandler` and `PropertyEditHandler : IDevToolsPropertyEditHandler?` bind extension points; init-only `HotKeys : HotKeyConfiguration` binds the gesture rig.

[DEVTOOLS_VIEW_KIND]: `LogicalTree`, `VisualTree`, `Events`, `CombinedTree`, `Resources`, and `Assets` select the launch view.

[HOTKEY_CONFIGURATION]: `ValueFramesFreeze`, `ValueFramesUnfreeze`, `InspectHoveredControl`, `TogglePopupFreeze`, and `ScreenshotSelectedControl` each carry a `KeyGesture`.

[SCREENSHOT_HANDLER]: `Task Take(Control control)` is the whole interface, so the consumer's implementation owns the snapshot and the awaited `Task` is where a typed result collapses. `DevToolsOptions.ScreenshotHandler` binds the package's own file-picker default through `internal static Conventions.DefaultScreenshotHandler`, which a consuming assembly cannot name — replacement is the only reachable choice.

[PROPERTY_EDIT_HANDLER]: `void OnPropertyEdited(DevToolsPropertyEdit edit)` is the whole interface, and its `void` return parks a typed refusal on the consumer's own evidence surface before the handler returns.

[DEVTOOLS_PROPERTY_EDIT]: this sealed commit record carries `InspectedObject : AvaloniaObject`, `Target : object`, `PropertyName` and `XamlPropertyName : string`, `PropertyType : Type`, `DeclaringType : Type?`, `OldValue`/`NewValue : object?`, `OldValueText`/`NewValueText : string?`, `IsAttached` and `IsAvaloniaProperty : bool`, `ResourceReferenceKind : DevToolsResourceReferenceKind`, `ResourceKey : object?`, and `ResourceKeyText : string?`; its public constructor takes every member positionally with the trailing resource triple defaulted.

[DEVTOOLS_RESOURCE_REFERENCE_KIND]: `None`, `Static`, and `Dynamic` classify the edited value's resource binding.

[INSPECTOR_RUNTIME]: session + render internals
- concern: dev-loop-inspection

| [INDEX] | [SYMBOL]                                        | [KIND]                 |
| :-----: | :---------------------------------------------- | :--------------------- |
|  [01]   | `DevTools` / `DevToolsSession` / `DevToolsView` | inspector runtime      |
|  [02]   | `PropertyValueEditorService`                    | property-editor engine |
|  [03]   | `VisualTreeDebug`                               | diagnostics overlay    |
|  [04]   | `VisualExtensions`                              | snapshot extensions    |

[DEVTOOLS_RUNTIME]: `DevTools`, `DevToolsSession`, and `DevToolsView` own the overlay window, per-`TopLevel` session, and root view; the attach surface mounts them, and consumers do not construct them.

[PROPERTY_VALUE_EDITOR_SERVICE]: Typed converters and commit state drive live property and style editing behind the property pane.

[VISUAL_TREE_DEBUG]: `VisualTreeDebug` projects the layout and renderer diagnostics overlay.

[VISUAL_EXTENSIONS]: `RenderTo(this Control, Stream, double dpi = 96.0)` writes the control snapshot consumed by the screenshot handler.

## [02]-[ENTRYPOINTS]

[ATTACH_SURFACE]: `Avalonia.DevToolsExtensions` — one polymorphic attach fold over `TopLevel`/`Application`
- concern: dev-loop-inspection

| [INDEX] | [SURFACE]                                                               | [SURFACE_ROOT]       | [CAPABILITY]                         |
| :-----: | :---------------------------------------------------------------------- | :------------------- | :----------------------------------- |
|  [01]   | `AttachDevTools(this TopLevel root)`                                    | `DevToolsExtensions` | default-gesture window attach        |
|  [02]   | `AttachDevTools(this TopLevel root, KeyGesture gesture)`                | `DevToolsExtensions` | custom-gesture window attach         |
|  [03]   | `AttachDevTools(this TopLevel root, DevToolsOptions options)`           | `DevToolsExtensions` | full-config window attach            |
|  [04]   | `AttachDevTools(this Application application)`                          | `DevToolsExtensions` | app-lifetime attach (all top-levels) |
|  [05]   | `AttachDevTools(this Application application, DevToolsOptions options)` | `DevToolsExtensions` | full-config app attach               |
|  [06]   | `RenderTo(this Control source, Stream destination, double dpi = 96.0)`  | `VisualExtensions`   | control-snapshot capture             |

[EXTENSION_POINTS]: two handler contracts admit a consumer implementation, each carrying exactly one member, so the interface IS the implementation and the package supplies no partial base
- concern: dev-loop-inspection

| [INDEX] | [SURFACE]                                     | [SURFACE_ROOT]                 | [CAPABILITY]                       |
| :-----: | :-------------------------------------------- | :----------------------------- | :--------------------------------- |
|  [01]   | `Task Take(Control control)`                  | `IScreenshotHandler`           | consumer-owned snapshot sink       |
|  [02]   | `void OnPropertyEdited(DevToolsPropertyEdit)` | `IDevToolsPropertyEditHandler` | consumer-owned commit interception |

## [03]-[IMPLEMENTATION_LAW]

[DEVLOOP_LAW]:
- Package: `ProDiagnostics`
- Owns: the Avalonia-12 runtime inspector overlay — the visual/logical/combined tree navigator, the live property + style pane with in-place editing (`PropertyValueEditorService`), routed-event tracking (`ShowEventsTab`/`ScopeEventsToRoot`), and the layout/renderer diagnostics overlays (`VisualTreeDebug`); the `DevToolsViewKind` launch view, `HotKeyConfiguration` gestures, and `IScreenshotHandler`/`IDevToolsPropertyEditHandler` extension points.
- Accept: `Diagnostics/devloop.md` mounts the inspector through `AttachDevTools(this Application, DevToolsOptions)` at composition time under the `Debug` gate; the `DevToolsOptions` launch view / tab visibility / hotkey rig are dev-loop policy rows; a custom `IScreenshotHandler` routes captures into the proof/capture lane where devloop needs deterministic snapshots; a custom `IDevToolsPropertyEditHandler` seals each commit onto the consumer's evidence stream from the whole `DevToolsPropertyEdit` record.
- Reject: a Release-surface reference (the `PrivateAssets="all"` `Debug` gate is law — no devtools identity in the shipped product); a second Avalonia devtools binding (`Avalonia.Diagnostics` 11.3.x is feed-dead, Accelerate DevTools is license-gate REJECTED); re-implementing the tree/property inspector where this overlay owns it; a hand-rolled control-snapshot where `VisualExtensions.RenderTo` exists; a consumer-side reference to `Conventions.DefaultScreenshotHandler`, which is `internal` and binds as the option default alone; a handler body re-deriving the commit from `InspectedObject` where the edit record already carries the target, both value renderings, and the resource-reference triple.

[STACKING]:
- `Diagnostics/devloop.md` is the sole consumer anchor — the HUD/hot-reload/replay-verify dev loop mounts this inspector beside `HotAvalonia`'s XAML hot-reload; the two share the `Debug` `PrivateAssets` gate and never co-mount in Release; its `InspectorCapture` and `InspectorEdits` bodies are the two extension points above, so the snapshot lands on the capture encode fold and the commit fires on the AppUi fact stream.
- Catalog depth is stub-plus-verified: every member above is decompile-verified against the restored `Avalonia.Diagnostics.dll`. `DevToolsSession` deepens when a consumer reaches its per-`TopLevel` session surface, and `PropertyValueEditorService` when a consumer drives its converter and commit state directly rather than through the property pane.

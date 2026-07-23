# [RASM_APPUI_API_DIALOGHOST]

`DialogHost.Avalonia` owns retained modal orchestration over an Avalonia overlay region: a `DialogHost` control marks the region and a static identifier-keyed surface drives it by `Identifier`, no control reference held. `Show` returns `Task<object?>` whose awaited value is the close parameter — dismissal-as-a-value, a confirm to its chosen result and a cancel to `null`. `DialogClosingEventArgs.Cancel()` vetoes a dismissal, `IsMultipleDialogsEnabled` stacks sessions, and AppUi binds every surface through one ReactiveUI `Interaction` seam that re-types the erased parameter onto the `Fin` rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DialogHost.Avalonia`
- package: `DialogHost.Avalonia` (MIT)
- assembly: `DialogHost.Avalonia`
- namespace: `DialogHostAvalonia`, `DialogHostAvalonia.Positioners`
- asset: managed library + embedded `avares://` XAML (`DialogHostStyles` theme resources); `lib/net8.0` is the sole shipped asset, bound by the `net10.0` workspace
- rail: dialogs

## [02]-[PUBLIC_TYPES]

[DIALOG_TYPES]: host control, session handle, event args, handler delegates, and style holders

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY] | [CAPABILITY]                                                  |
| :-----: | :------------------------------------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `DialogHost`                                             | class         | `ContentControl` host + static show/close/query surface       |
|  [02]   | `DialogSession`                                          | class         | one open dialog — `UpdateContent`/`Close`/`IsEnded`/`Content` |
|  [03]   | `DialogOpenedEventArgs`                                  | class         | routed args carrying the opened `Session`                     |
|  [04]   | `DialogClosingEventArgs`                                 | class         | routed args — `Parameter`, `Cancel()` veto, `CanBeCancelled`  |
|  [05]   | `DialogOpenedEventHandler` / `DialogClosingEventHandler` | delegate      | open/close handlers bound to the `*Callback` properties       |
|  [06]   | `DialogHostStyle`                                        | class         | static holder of the attached chrome properties               |
|  [07]   | `DialogHostStyles`                                       | class         | `Styles` resource — the default theme dictionary              |

[POSITIONER_TYPES]: popup placement contracts and their implementations

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------ | :------------ | :------------------------------------------------------- |
|  [01]   | `IDialogPopupPositioner`              | interface     | `Update(Size, Size) -> Rect` placement contract          |
|  [02]   | `IDialogPopupPositionerConstrainable` | interface     | adds `Constrain(Size) -> Size` bounds                    |
|  [03]   | `CenteredDialogPopupPositioner`       | class         | centers in the host; singleton `Instance`                |
|  [04]   | `AlignmentDialogPopupPositioner`      | class         | `HorizontalAlignment`/`VerticalAlignment`/`Margin` align |

## [03]-[ENTRYPOINTS]

[STATIC_DIALOG_OPS]: static `DialogHost` surface, identifier-keyed, no control reference; `Show` is the awaitable result rail

| [INDEX] | [SURFACE]                                                 | [CAPABILITY]            |
| :-----: | :-------------------------------------------------------- | :---------------------- |
|  [01]   | `Show(object?) -> Task<object?>`                          | sole-host result        |
|  [02]   | `Show(object?, string?) -> Task<object?>`                 | identifier-keyed result |
|  [03]   | `Show(object?, string? \| DialogHost, opened?, closing?)` | handler-bound overloads |
|  [04]   | `Close(string?, object?, object?)`                        | result-bearing close    |
|  [05]   | `Pop(string?, object?)`                                   | stacked-session pop     |
|  [06]   | `IsDialogOpen(string?, object?) -> bool`                  | open-state probe        |
|  [07]   | `GetDialogSession(string?) -> DialogSession?`             | session resolution      |

[SESSION_OPS]: methods and read-only properties on the resolved `DialogSession` and the routed event args

| [INDEX] | [SURFACE]                                               | [CAPABILITY]                            |
| :-----: | :------------------------------------------------------ | :-------------------------------------- |
|  [01]   | `DialogSession.UpdateContent(object)`                   | swap the live dialog content            |
|  [02]   | `DialogSession.Close([object?])`                        | end the session with an optional result |
|  [03]   | `DialogSession.IsEnded` / `Content`                     | terminal flag; current host content     |
|  [04]   | `DialogClosingEventArgs.Cancel()`                       | veto the in-flight close                |
|  [05]   | `DialogClosingEventArgs.IsCancelled` / `CanBeCancelled` | veto state; whether the veto is honored |
|  [06]   | `DialogClosingEventArgs.Parameter`                      | the close parameter being returned      |
|  [07]   | `DialogOpenedEventArgs.Session`                         | the session that opened                 |

[HOST_PROPERTIES]: styled and direct properties on `DialogHost`; the `*DialogCommand` pair are `ICommand` relays and `CurrentSession(s)` are computed

| [INDEX] | [SURFACE]                                        | [CAPABILITY]                                |
| :-----: | :----------------------------------------------- | :------------------------------------------ |
|  [01]   | `Identifier`                                     | host key for the static identifier surface  |
|  [02]   | `IsOpen`                                         | open state of the host                      |
|  [03]   | `IsMultipleDialogsEnabled`                       | enable the session stack                    |
|  [04]   | `CurrentSession` / `CurrentSessions`             | top session; the stacked-session set        |
|  [05]   | `DialogContent` / `DialogContentTemplate`        | single-dialog content and its data template |
|  [06]   | `DialogMargin`                                   | margin around the dialog                    |
|  [07]   | `OpenDialogCommand` / `CloseDialogCommand`       | `ICommand` open/close from XAML             |
|  [08]   | `CloseOnClickAway` / `CloseOnClickAwayParameter` | overlay-click dismiss and its result        |
|  [09]   | `DisableOpeningAnimation`                        | suppress the open transition                |

[CHROME_AND_PLACEMENT]: overlay/blur/positioner properties on `DialogHost`, the `DialogHostStyle` attached per-host chrome, and the positioner placement methods

| [INDEX] | [SURFACE]                                                     | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------------ | :------------------------------------------------------ |
|  [01]   | `OverlayBackground`                                           | scrim brush behind the dialog                           |
|  [02]   | `BlurBackground` / `BlurBackgroundRadius`                     | backdrop blur toggle and radius                         |
|  [03]   | `PopupPositioner` / `PopupTemplate`                           | `IDialogPopupPositioner` placement; popup-root template |
|  [04]   | `DialogHostStyle.GetCornerRadius`/`SetCornerRadius`           | attached per-host corner radius                         |
|  [05]   | `DialogHostStyle.SetBorderBrush` / `SetBorderThickness`       | attached per-host border (set-only)                     |
|  [06]   | `DialogHostStyle.GetBoxShadow`/`SetBoxShadow`                 | attached per-host box shadow                            |
|  [07]   | `DialogHostStyle.GetClipToBounds`/`SetClipToBounds`           | attached per-host clip flag                             |
|  [08]   | `IDialogPopupPositioner.Update(Size, Size) -> Rect`           | compute the popup rect                                  |
|  [09]   | `IDialogPopupPositionerConstrainable.Constrain(Size) -> Size` | clamp the popup to bounds                               |

[EVENTS]: routed events and their handler-property delegates on `DialogHost`

| [INDEX] | [SURFACE]                                        | [CAPABILITY]                                       |
| :-----: | :----------------------------------------------- | :------------------------------------------------- |
|  [01]   | `DialogOpened` / `DialogClosing`                 | `RoutedEvent` on open; vetoable event before close |
|  [02]   | `DialogOpenedCallback` / `DialogClosingCallback` | `*EventHandler` direct-property handlers           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Show(content, identifier)` returns `Task<object?>` whose awaited value is the close `Parameter`, and a session is reached by `Identifier` rather than by holding the control, so the dispatcher closes and queries a session it never constructed.
- `DialogClosing` arms `DialogClosingEventArgs.Cancel()` through `DialogClosingCallback`, blocking dismissal until a dirty form resolves; `CanBeCancelled` gates whether the veto is honored.
- `IsMultipleDialogsEnabled` promotes the host to a session stack — `CurrentSessions` is the set a retreat consults and `Pop` retreats one; a `Show` on a non-stacked host holding an open session folds onto it, probed via `IsDialogOpen(Identifier)`, rather than minting a parallel root.
- `UpdateContent` swaps a resolved session's content in place (progress -> result) without closing, so the awaited `Show` task stays the single result handle across content phases.

[STACKING]:
- `api-reactiveui`(`.api/api-reactiveui.md`): each `DialogIntent` case maps to `Show(request, Identifier)` through one per-root `Interaction<DialogIntent, object?>`, and the erased `object?` close parameter re-types onto the `Fin` rail once at `DialogSurface.Project`, never per call site.
- `api-avalonia-fluent`(`.api/api-avalonia-fluent.md`): `OverlayBackground`, `BlurBackground`, `PopupPositioner`, and `DialogHostStyle.CornerRadius` resolve through Fluent theme token keys, so a host inherits the app theme rather than carrying inline brushes.
- within-lib `DialogTopology`: one row binds identifier, stacking, close policy, and styling tokens per admitted surface, and `DialogSurface` folds `UpdateContent` with the toast/progress rail so a long-running session advances through content phases on one `Show` handle.

[LOCAL_ADMISSION]:
- Every modal, transient, and pick surface binds one `DialogTopology` row and reaches DialogHost only through the `Interaction` seam; the static identifier surface addresses every session, and overlay, blur, and chrome resolve through theme tokens.

[RAIL_LAW]:
- Package: `DialogHost.Avalonia`
- Owns: retained modal orchestration — identifier-addressed sessions, the awaitable close-parameter result rail, the vetoable `DialogClosing` seam, the session stack, and per-host overlay/blur/positioner/chrome.
- Accept: modal state as host-addressable, command- and identifier-driven surfaces; `Show` results as `Task<object?>` close parameters onto the `Fin` rail; the `Interaction` seam owning each per-surface binding.
- Reject: a control-reference show path where the static identifier surface addresses the session; host-specific modal service families; inline overlay/chrome literals where theme tokens resolve; re-typing the erased close parameter at each call site instead of one boundary capsule.

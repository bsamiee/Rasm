# [RASM_APPUI_API_DIALOGHOST]

`DialogHost.Avalonia` is a retained modal-dialog host for Avalonia 12: a `DialogHost` control marks an overlay region keyed by `Identifier`, and the static `DialogHost.Show`/`Close`/`Pop`/`IsDialogOpen`/`GetDialogSession` surface drives that region from anywhere by identifier with no control reference. `Show` returns `Task<object?>` whose awaited result is the close parameter — dismissal-as-a-value: a confirm dialog awaits to the chosen result, a cancelled dialog to `null`. Each open dialog is a `DialogSession` (`UpdateContent`/`Close`); `DialogOpened`/`DialogClosing` routed events plus the `DialogOpenedCallback`/`DialogClosingCallback` direct-property handler delegates gate the lifecycle, and `DialogClosingEventArgs.Cancel()` vetoes a close (`CanBeCancelled`/`IsCancelled`) so a dirty form blocks dismissal. `IsMultipleDialogsEnabled` turns the host into a session stack (`CurrentSession`/`CurrentSessions`, `Pop` retreats one). Overlay (`OverlayBackground`, `BlurBackground`/`BlurBackgroundRadius`), margin, popup positioning (`IDialogPopupPositioner`), and per-host chrome (`DialogHostStyle` attached `CornerRadius`/`BorderBrush`/`BoxShadow`) are all styled. AppUi resolves this through one ReactiveUI `Interaction` seam: a `DialogIntent` union maps to `DialogHost.Show(request, identifier)` per `DialogTopology` row, the awaited `object?` re-types onto the `Fin` rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `DialogHost.Avalonia`
- package: `DialogHost.Avalonia`
- assembly: `DialogHost.Avalonia`
- namespace: `DialogHostAvalonia`
- namespace: `DialogHostAvalonia.Positioners`
- namespace: `DialogHostAvalonia.Utilities` (vendored `BehaviorSubject<T>` backing the session stack — internal mechanism, not a consumer surface)
- asset: managed runtime library + embedded `avares://` XAML (`DialogHostStyles` resources)
- tfm: `net8.0` (sole shipped asset; consumed by the `net10.0` workspace)
- license: `MIT`
- rail: dialogs

## [02]-[PUBLIC_TYPES]

[DIALOG_TYPES]: host control, session handle, event args, and handler delegates
- rail: dialogs

| [INDEX] | [SYMBOL]                       | [KIND]              | [RAIL]                                              |
| :-----: | :----------------------------- | :------------------ | :------------------------------------------------- |
|  [01]   | `DialogHost`                   | `ContentControl`    | host control + static show/close/query surface     |
|  [02]   | `DialogSession`                | session handle      | one open dialog — `UpdateContent`/`Close`/`IsEnded` |
|  [03]   | `DialogOpenedEventArgs`        | routed event args   | carries the opened `Session`                        |
|  [04]   | `DialogClosingEventArgs`       | routed event args   | `Session`/`Parameter`, `Cancel()` veto             |
|  [05]   | `DialogOpenedEventHandler`     | delegate            | open handler bound to `DialogOpenedCallback`        |
|  [06]   | `DialogClosingEventHandler`    | delegate            | close handler bound to `DialogClosingCallback`      |
|  [07]   | `DialogHostStyle`              | static style holder | attached chrome properties (radius/border/shadow)  |
|  [08]   | `DialogHostStyles`             | `Styles` resource   | the included default theme `ResourceDictionary`     |

[POSITIONER_TYPES]: popup placement contracts
- rail: dialogs

| [INDEX] | [SYMBOL]                              | [KIND]               | [RAIL]                                       |
| :-----: | :------------------------------------ | :------------------- | :------------------------------------------- |
|  [01]   | `IDialogPopupPositioner`              | positioner contract  | `Update(Size, Size) -> Rect` placement       |
|  [02]   | `IDialogPopupPositionerConstrainable` | constrainable popup  | `Constrain(Size) -> Size` bounds             |
|  [03]   | `CenteredDialogPopupPositioner`       | centered positioner  | singleton `Instance`, centers in the host    |
|  [04]   | `AlignmentDialogPopupPositioner`      | aligned positioner   | `HorizontalAlignment`/`VerticalAlignment`/`Margin` |

## [03]-[ENTRYPOINTS]

[STATIC_DIALOG_OPS]: identifier-keyed show/close/query — no control reference required, `Show` is the awaitable result rail
- rail: dialogs

| [INDEX] | [SURFACE]                                                                       | [SURFACE_ROOT] | [RAIL]                                          |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :--------------------------------------------- |
|  [01]   | `Show(object? content) -> Task<object?>`                                         | `DialogHost`   | open on the only host; await the close result  |
|  [02]   | `Show(object? content, string? dialogIdentifier) -> Task<object?>`               | `DialogHost`   | open on the identified host; await the result  |
|  [03]   | `Show(content, [string? id \| DialogHost instance], opened?, closing?)`          | `DialogHost`   | open with opened/closing handlers; await result|
|  [04]   | `Close(string? dialogIdentifier, object? parameter = null, object? content)`     | `DialogHost`   | close the identified dialog with a result value|
|  [05]   | `Pop(string? dialogIdentifier, object? content)`                                 | `DialogHost`   | retreat one stacked session                     |
|  [06]   | `IsDialogOpen(string? dialogIdentifier, object? content)`                         | `DialogHost`   | open-state probe by identifier                  |
|  [07]   | `GetDialogSession(string? dialogIdentifier) -> DialogSession?`                    | `DialogHost`   | resolve the active session by identifier        |

[SESSION_OPS]: per-session lifecycle on the resolved `DialogSession`
- rail: dialogs

| [INDEX] | [SURFACE]                       | [SURFACE_ROOT]           | [RAIL]                               |
| :-----: | :------------------------------ | :----------------------- | :----------------------------------- |
|  [01]   | `UpdateContent(object content)` | `DialogSession`          | swap the live dialog content         |
|  [02]   | `Close()` / `Close(object?)`    | `DialogSession`          | end the session, optional result     |
|  [03]   | `IsEnded`                       | `DialogSession`          | session terminal flag                |
|  [04]   | `Content`                       | `DialogSession`          | current content (reads host content) |
|  [05]   | `Cancel()`                      | `DialogClosingEventArgs` | veto the in-flight close             |
|  [06]   | `IsCancelled` / `CanBeCancelled`| `DialogClosingEventArgs` | veto state of the closing event      |
|  [07]   | `Parameter`                     | `DialogClosingEventArgs` | the close parameter being returned   |
|  [08]   | `Session`                       | `DialogOpenedEventArgs`  | the session that opened              |

[HOST_PROPERTIES]: styled/direct properties on the `DialogHost` control
- rail: dialogs

| [INDEX] | [SURFACE]                  | [PROPERTY_KIND]  | [RAIL]                                 |
| :-----: | :------------------------- | :--------------- | :------------------------------------- |
|  [01]   | `Identifier`               | direct           | host key for the static identifier API |
|  [02]   | `IsOpen`                   | direct           | open state of the host                 |
|  [03]   | `IsMultipleDialogsEnabled` | direct           | enable the session stack               |
|  [04]   | `CurrentSession`           | computed         | top session of the stack               |
|  [05]   | `CurrentSessions`          | direct           | the stacked-session collection         |
|  [06]   | `DialogContent`            | styled           | content of the (single) dialog         |
|  [07]   | `DialogContentTemplate`    | styled           | data template for `DialogContent`      |
|  [08]   | `DialogMargin`             | styled           | margin around the dialog               |
|  [09]   | `OpenDialogCommand`        | direct (command) | `ICommand` to open from XAML           |
|  [10]   | `CloseDialogCommand`       | direct (command) | `ICommand` to close from XAML          |
|  [11]   | `CloseOnClickAway`         | direct           | dismiss on overlay click               |
|  [12]   | `CloseOnClickAwayParameter`| direct           | result passed by a click-away dismiss  |
|  [13]   | `DisableOpeningAnimation`  | direct           | suppress the open transition           |

[VISUAL_PROPERTIES]: overlay, blur, positioning, and per-host chrome
- rail: dialogs

| [INDEX] | [SURFACE]                             | [SURFACE_ROOT]                        | [RAIL]                                |
| :-----: | :------------------------------------ | :------------------------------------ | :------------------------------------ |
|  [01]   | `OverlayBackground`                   | `DialogHost`                          | scrim brush behind the dialog         |
|  [02]   | `BlurBackground` / `BlurBackgroundRadius` | `DialogHost`                      | toggle + radius for the backdrop blur |
|  [03]   | `PopupPositioner`                     | `DialogHost`                          | `IDialogPopupPositioner` placement    |
|  [04]   | `PopupTemplate`                       | `DialogHost`                          | control template for the popup root   |
|  [05]   | `GetCornerRadius` / `SetCornerRadius` | `DialogHostStyle`                     | attached per-host corner radius       |
|  [06]   | `GetBorderBrush` / `SetBorderBrush`   | `DialogHostStyle`                     | attached per-host border brush        |
|  [07]   | `GetBorderThickness`/`SetBorderThickness` | `DialogHostStyle`                 | attached per-host border thickness    |
|  [08]   | `GetBoxShadow` / `SetBoxShadow`       | `DialogHostStyle`                     | attached per-host box shadow          |
|  [09]   | `GetClipToBounds` / `SetClipToBounds` | `DialogHostStyle`                     | attached per-host clip flag           |
|  [10]   | `Update(Size anchor, Size size) -> Rect` | `IDialogPopupPositioner`           | compute the popup rect                |
|  [11]   | `Constrain(Size available) -> Size`   | `IDialogPopupPositionerConstrainable` | clamp the popup to bounds             |

[EVENTS]: routed events and handler-property delegates
- rail: dialogs

| [INDEX] | [SURFACE]               | [SURFACE_ROOT] | [RAIL]                                       |
| :-----: | :---------------------- | :------------- | :------------------------------------------- |
|  [01]   | `DialogOpened`          | `DialogHost`   | `RoutedEvent` raised on open                 |
|  [02]   | `DialogClosing`         | `DialogHost`   | `RoutedEvent` raised before close (vetoable) |
|  [03]   | `DialogOpenedCallback`  | `DialogHost`   | `DialogOpenedEventHandler` direct property   |
|  [04]   | `DialogClosingCallback` | `DialogHost`   | `DialogClosingEventHandler` direct property  |

## [04]-[IMPLEMENTATION_LAW]

[DIALOG_LAW]:
- `Show(content, identifier)` returns `Task<object?>`; the awaited value is the close `Parameter`, so the dialog result is a value on the async rail — a confirm awaits to its chosen result, a click-away or cancel to `null`. This is the dismissal-as-a-value contract the AppUi `Fin`-railed `DialogIntent` re-types onto.
- The static identifier-keyed surface (`Show`/`Close`/`Pop`/`IsDialogOpen`/`GetDialogSession`) is the addressing model: a session is reached by `Identifier`, never by holding the control, so the intent dispatcher closes/queries a session it never constructed.
- `DialogClosing` + `DialogClosingEventArgs.Cancel()` is the veto seam: a dirty-form session arms `Cancel` through `DialogClosingCallback` so dismissal blocks until the form resolves (`CanBeCancelled` gates whether the veto is honored).
- `IsMultipleDialogsEnabled` promotes the host to a session stack: `CurrentSessions` is the stacked set a `Retreat` veto consults, `Pop` retreats one, and a `Show` on a non-stacked host that already holds an open session folds onto the existing session (probed via `IsDialogOpen(Identifier)`) rather than minting a parallel root.

[STACKING_LAW]:
- AppUi binds DialogHost through one ReactiveUI `Interaction<TInput, TOutput>` seam, not call-site `Show`: a `DialogIntent` union case maps to `DialogHost.Show(request, state.Identifier)` per `DialogTopology` row, and the awaited `object?` close parameter projects onto the `Fin` rail at one boundary capsule (`DialogSurface.Project`) — the erased close parameter is re-typed once, never per call site.
- Per-host chrome composes from theme token keys, not literals: `OverlayBackground`, `BlurBackground`, `PopupPositioner`, and `DialogHostStyle.CornerRadius` resolve through the Fluent token rail so a dialog host inherits the app theme rather than carrying inline brushes.
- `UpdateContent` stacks with the progress/toast rail: a long-running session resolves its `DialogSession` by `GetDialogSession(Identifier)` and swaps content (progress -> result) without closing and reopening, so the awaited `Show` task stays the single result handle across content phases.

[MODALITY_LAW]:
- Package: `DialogHost.Avalonia`
- Owns: retained modal orchestration — identifier-addressed sessions, the awaitable result rail, the vetoable closing seam, the session stack, and per-host overlay/blur/positioner/chrome — for panel, companion, sidecar, diagnostic, and downstream app dialog surfaces through one dialog rail.
- Accept: modal state stays host-addressable and command/identifier-driven; `Show` results flow as `Task<object?>` close parameters onto the `Fin` rail; the `Interaction` seam owns the per-surface binding.
- Reject: free-floating modal logic; host-specific modal service families; a control-reference show path where the static identifier API addresses the session; inline overlay/chrome literals where theme tokens resolve; re-typing the erased close parameter at every call site instead of one boundary capsule.

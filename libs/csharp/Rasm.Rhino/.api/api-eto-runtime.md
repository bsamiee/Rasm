# [RASM_RHINO_API_ETO_RUNTIME]

`Eto.Forms` is the cross-platform UI framework the Rhino host binds; this catalog owns its ambient runtime — the process-wide singletons and static device/display state that sit beside the control tree rather than inside it. `Application` is the UI-thread marshalling and message-loop boundary every background producer crosses, and the transfer, timer, input, and tray surfaces ride the same runtime rail; control construction, painting, output, and platform hosting are the sibling `eto` catalogs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.Forms`
- package: `Eto.Forms` — host-provided, resolved from the Rhino host assembly set, never a central `PackageReference`
- assembly: `Eto`
- namespace: `Eto.Forms`
- asset: the `Eto` assembly the Rhino host loads; `macOS`, `WinForms`, and `Wpf` platform handlers back one managed surface
- rail: eto-runtime

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: application dispatch and clock — `Application` is the process singleton reached through `Application.Instance`, and UI-thread affinity routes every control mutation through one of its dispatch shapes.

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :------------ | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `Application` | class         | singleton UI-thread dispatch, message-loop iteration, notification hook |
|  [02]   | `UITimer`     | class         | widget-free repeating UI-thread clock with `Interval`/`Elapsed`         |

[PUBLIC_TYPE_SCOPE]: input and display state — static projections of live device and display state; `Cursors` is the closed handle roster.

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [CAPABILITY]                                                 |
| :-----: | :------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `Screen`       | class         | display enumeration, bounds, `LogicalPixelSize`, screen grab |
|  [02]   | `Mouse`        | static class  | pointer position, pressed buttons, cursor override           |
|  [03]   | `Keyboard`     | static class  | modifier state, lock-key query, `ModifiersChanged`           |
|  [04]   | `MouseButtons` | flags enum    | pressed-button set                                           |
|  [05]   | `Keys`         | flags enum    | key + modifier vocabulary                                    |
|  [06]   | `Cursor`       | class         | a cursor handle applied to a control or the pointer          |
|  [07]   | `Cursors`      | static class  | built-in cursor handle roster                                |

[PUBLIC_TYPE_SCOPE]: typed data transfer and drag — `Clipboard` (via `Clipboard.Instance`) and `DataObject` share one typed-payload contract keyed by a MIME `type`; `DragEventArgs` carries a `DataObject` through the drag protocol.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                           |
| :-----: | :-------------- | :------------ | :--------------------------------------------------------------------- |
|  [01]   | `Clipboard`     | class         | singleton typed clipboard read/write and `Clear`, keyed by MIME `type` |
|  [02]   | `DataObject`    | class         | drag-scoped typed payload keyed by MIME `type`                         |
|  [03]   | `IDataObject`   | interface     | the keyed get+set contract both carriers implement                     |
|  [04]   | `DragEffects`   | flags enum    | `Copy`/`Move`/`Link` allowed-effect negotiation                        |
|  [05]   | `DragEventArgs` | class         | drop location, `Data`, `AllowedEffects`, resolved `Effects`            |

[PUBLIC_TYPE_SCOPE]: notification and tray — `TrayIndicator` owns persistent tray presence, `Notification` the transient toast optionally anchored to it.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :-------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `TrayIndicator` | class         | persistent tray icon with a `ContextMenu`, show/hide |
|  [02]   | `Notification`  | class         | transient system toast, optionally tray-anchored     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: UI-thread dispatch and iteration

| [INDEX] | [SURFACE]                                     | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Application.Invoke(Action)`                  | instance | synchronous marshal onto the UI thread         |
|  [02]   | `Application.Invoke(Func<T>) -> T`            | instance | synchronous marshalled read of a UI value      |
|  [03]   | `Application.AsyncInvoke(Action)`             | instance | fire-and-forget post to the UI thread          |
|  [04]   | `Application.InvokeAsync(Action) -> Task`     | instance | awaitable marshalled action                    |
|  [05]   | `Application.InvokeAsync(Func<T>) -> Task<T>` | instance | awaitable marshalled read                      |
|  [06]   | `Application.EnsureUIThread()`                | instance | throws off the UI thread — the affinity assert |
|  [07]   | `Application.IsUIThread -> bool`              | property | tests current-thread affinity                  |
|  [08]   | `Application.RunIteration()`                  | instance | pump one message-loop pass                     |
|  [09]   | `UITimer(EventHandler<EventArgs>)`            | ctor     | construct a clock bound to an elapsed handler  |
|  [10]   | `UITimer.Start()` / `UITimer.Stop()`          | instance | run and halt the repeating clock               |

[ENTRYPOINT_SCOPE]: display, input, and cursor state

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                          |
| :-----: | :----------------------------------------------- | :------- | :------------------------------------ |
|  [01]   | `Screen.Screens -> IEnumerable<Screen>`          | static   | the connected-display roster          |
|  [02]   | `Screen.PrimaryScreen -> Screen`                 | static   | the primary display                   |
|  [03]   | `Screen.FromPoint(PointF) -> Screen`             | static   | the display containing a point        |
|  [04]   | `Screen.FromRectangle(RectangleF) -> Screen`     | static   | the display best covering a rectangle |
|  [05]   | `Screen.GetImage(RectangleF) -> Image`           | instance | screen-region capture                 |
|  [06]   | `Mouse.IsSupported -> bool`                      | static   | platform pointer-state availability   |
|  [07]   | `Mouse.IsAnyButtonPressed(MouseButtons) -> bool` | static   | pressed-button test                   |
|  [08]   | `Mouse.SetCursor(Cursor)`                        | static   | override the pointer cursor           |
|  [09]   | `Keyboard.IsKeyLocked(Keys) -> bool`             | static   | lock-key state                        |

[ENTRYPOINT_SCOPE]: typed transfer, drag, and tray

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `Clipboard.SetString(string, string)`              | instance | write a typed text payload                 |
|  [02]   | `Clipboard.SetData(byte[], string)`                | instance | write a typed byte payload                 |
|  [03]   | `Clipboard.SetDataStream(Stream, string)`          | instance | write a typed stream payload               |
|  [04]   | `Clipboard.SetObject(object, string)`              | instance | write a typed boxed payload                |
|  [05]   | `Clipboard.GetString(string) -> string`            | instance | read a typed text payload                  |
|  [06]   | `Clipboard.GetData(string) -> byte[]`              | instance | read a typed byte payload                  |
|  [07]   | `Clipboard.GetDataStream(string) -> Stream`        | instance | read a typed stream payload                |
|  [08]   | `Clipboard.GetObject<T>(string) -> T`              | instance | read a typed boxed payload                 |
|  [09]   | `Clipboard.Contains(string) -> bool`               | instance | presence probe for a MIME type             |
|  [10]   | `Clipboard.Clear()`                                | instance | empty the clipboard                        |
|  [11]   | `DataObject.SetString(string, string)`             | instance | write onto the drag payload                |
|  [12]   | `DataObject.GetObject<T>(string) -> T`             | instance | read from the drag payload                 |
|  [13]   | `DragEventArgs.SetDropDescription(string, string)` | instance | annotate the OS drop cursor                |
|  [14]   | `Notification.Show(TrayIndicator)`                 | instance | deliver a toast, optionally anchored       |
|  [15]   | `TrayIndicator.SetMenu(ContextMenu)`               | instance | bind the tray context menu                 |
|  [16]   | `TrayIndicator.Show()` / `TrayIndicator.Hide()`    | instance | show and hide the tray presence            |
|  [17]   | `Application.NotificationActivated`                | event    | route notification activation by user data |
|  [18]   | `Notification.UserData`                            | property | carry the activation correlation value     |
|  [19]   | `Notification.RequiresTrayIndicator`               | property | declare whether delivery needs a tray host |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every control-tree read or write executes on the UI thread; a background producer crosses through exactly one `Application` dispatch shape. `Invoke`/`Invoke<T>` block the caller and return the UI-side result, `AsyncInvoke` posts without completion, `InvokeAsync`/`InvokeAsync<T>` return an off-thread awaited `Task`. `EnsureUIThread` guards a UI-only method; `RunIteration` pumps the loop for a hand-rolled synchronous wait.
- `UITimer` fires `Elapsed` on the UI thread as the portable pace where a platform exposes no higher-fidelity display-link; `api-macos-native` owns the macOS display-link pace.
- `Clipboard` and `DataObject` are one typed-payload shape under two lifetimes — the clipboard process-external and persistent, the data object drag-scoped. Every payload keys by a MIME `type` string, and `SetString`/`SetData`/`SetDataStream`/`SetObject` pair one-to-one with the `Get*` readers and the `Contains` probe. `IDataObject` carries the whole keyed contract so one interface-typed body serves both lifetimes; only the stream pair (`GetDataStream`/`SetDataStream`) is class-level on each carrier and off the interface. Drag negotiation carries the `DataObject` on `DragEventArgs.Data`, declares `AllowedEffects`, and resolves the committed `Effects`.

[STACKING]:
- `LanguageExt`(`.api/api-languageext`): a UI-thread dispatch wraps into `Eff<A>`/`IO<A>` and folds its outcome to `Fin<A>`, so `Application.Invoke<T>`/`InvokeAsync<T>` compose as effectful reads rather than raw blocking calls threaded through domain code. `Option<A>` lifts every nullable transfer read (`Clipboard.GetString(type)`, `DataObject.GetObject<T>(type)` gated by `Contains(type)`) so an absent MIME payload is `None`. `UITimer` and `TrayIndicator` acquire and release through the `use` rail — `Start`/`Show` acquire, `Stop`/`Hide` release — so the clock and tray icon never leak past their owning scope.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): `Cursors`, `DragEffects`, and `MouseButtons` bind as `[SmartEnum]`/flag owners routed by generated `Switch`/`Map` instead of raw static-field reads and bitwise tests; a clipboard MIME `type` binds as `[ValueObject<string>]` so transfer access is keyed by a validated owner, never a bare `string type` argument.

[LOCAL_ADMISSION]:
- `Eto.Forms` runtime state is host-provided and never re-declared; a Rasm owner internalizes a dispatch, transfer, timer, or tray concern behind one canonical rail so downstream code composes a marshalled effect or a keyed payload.

[RAIL_LAW]:
- Package: `Eto.Forms`
- Owns: UI-thread dispatch and loop iteration, clock pacing, live input/display projection, typed clipboard and drag payloads, tray presence and toast delivery
- Accept: marshalled effects, keyed transfer payloads, resource-scoped clocks and tray icons composed through the canonical rail
- Reject: control/layout/window/menu construction (`api-eto-forms`), custom painting (`api-eto-drawing`), document output (`api-eto-printing`), platform selection and native hosting (`api-eto-platform`), and leaking `Application.Instance` or a stringy MIME key past the owning rail

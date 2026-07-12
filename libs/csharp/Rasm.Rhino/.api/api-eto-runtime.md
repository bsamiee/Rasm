# [RASM_RHINO_API_ETO_RUNTIME]

`Eto.Forms` is the native cross-platform UI framework the Rhino host binds, and this catalog owns its ambient runtime surface — the process-wide singletons and static state that sit beside the control tree rather than inside it. `Application` marshals work onto the UI thread across three distinct shapes (synchronous `Invoke`, fire-and-forget `AsyncInvoke`, task-returning `InvokeAsync`) and drives the message loop through `RunIteration`. `UITimer` is the framework clock; `Screen`, `Mouse`, `Keyboard`, and `Cursors` project display and input device state; `Clipboard` and `DataObject` are the typed string/byte/stream/object transfer pair keyed by a MIME `type`; `DragEffects` and `DragEventArgs` carry the drag payload and allowed-effect negotiation; `Notification` and `TrayIndicator` own system-tray presence and toast delivery. The construction surface (controls, layouts, windows, menus) is `api-eto-forms`; custom painting is `api-eto-drawing`; document output is `api-eto-printing`; platform selection and native-control hosting are `api-eto-platform`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto.Forms`

- package: `Eto.Forms` — host-provided, resolved from the Rhino host assembly set, not a central `PackageReference`
- assembly: `Eto`
- namespace: `Eto.Forms`
- asset: the `Eto` assembly the Rhino host loads; the `macOS`, `WinForms`, and `Wpf` platform handlers back the same managed surface
- rail: eto-runtime

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: application dispatch and clock

- namespace: `Eto.Forms`
- rail: eto-runtime

`Application` is the process singleton reached through `Application.Instance`; UI-thread affinity is a hard contract, so every control mutation routes through one of its dispatch shapes. `UITimer` is a widget-free repeating clock.

| [INDEX] | [SYMBOL]      | [KIND]            | [CAPABILITY]                                                  |
| :-----: | :------------ | :---------------- | :------------------------------------------------------------ |
|  [01]   | `Application` | process singleton | UI-thread dispatch, message-loop iteration, notification hook |
|  [02]   | `UITimer`     | timer             | repeating UI-thread clock with `Interval`/`Elapsed`           |

[PUBLIC_TYPE_SCOPE]: input and display state

- namespace: `Eto.Forms`
- rail: eto-runtime

`Mouse`, `Keyboard`, and `Screen` are static projections of live device and display state; `Cursors` is the closed roster of framework cursor handles.

| [INDEX] | [SYMBOL]       | [KIND]            | [CAPABILITY]                                                 |
| :-----: | :------------- | :---------------- | :----------------------------------------------------------- |
|  [01]   | `Screen`       | static + instance | display enumeration, bounds, `LogicalPixelSize`, screen grab |
|  [02]   | `Mouse`        | static state      | pointer position, pressed buttons, cursor override           |
|  [03]   | `Keyboard`     | static state      | modifier state, lock-key query, `ModifiersChanged`           |
|  [04]   | `MouseButtons` | flags enum        | the pressed-button set                                       |
|  [05]   | `Keys`         | flags enum        | key + modifier vocabulary                                    |
|  [06]   | `Cursor`       | handle            | a single cursor handle applied to a control or the pointer   |
|  [07]   | `Cursors`      | static roster     | the built-in cursor handle set                               |

[PUBLIC_TYPE_SCOPE]: typed data transfer and drag

- namespace: `Eto.Forms`
- rail: eto-runtime

`Clipboard` (the `Clipboard.Instance` singleton) and `DataObject` share one typed-payload contract: each stores and retrieves `string`, `byte[]`, `Stream`, and boxed `object` values under a MIME `type` key. `DragEventArgs` carries a `DataObject` through the drag protocol.

| [INDEX] | [SYMBOL]        | [KIND]     | [CAPABILITY]                                                  |
| :-----: | :-------------- | :--------- | :------------------------------------------------------------ |
|  [01]   | `Clipboard`     | singleton  | typed clipboard read/write keyed by MIME `type`, plus `Clear` |
|  [02]   | `DataObject`    | payload    | typed drag/transfer payload keyed by MIME `type`              |
|  [03]   | `IDataObject`   | interface  | the read contract a drop handler consumes                     |
|  [04]   | `DragEffects`   | flags enum | `Copy`/`Move`/`Link` allowed-effect negotiation               |
|  [05]   | `DragEventArgs` | event args | drop location, `Data`, `AllowedEffects`, resolved `Effects`   |

[PUBLIC_TYPE_SCOPE]: notification and tray

- namespace: `Eto.Forms`
- rail: eto-runtime

`TrayIndicator` owns the persistent tray presence; `Notification` is the transient toast, optionally anchored to a tray indicator.

| [INDEX] | [SYMBOL]        | [KIND]      | [CAPABILITY]                                            |
| :-----: | :-------------- | :---------- | :------------------------------------------------------ |
|  [01]   | `TrayIndicator` | tray widget | persistent tray icon with a `ContextMenu`, show/hide    |
|  [02]   | `Notification`  | toast       | transient system notification, optionally tray-anchored |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: UI-thread dispatch and iteration

- rail: eto-runtime

The three dispatch shapes are the boundary between a background thread and the control tree; `EnsureUIThread` asserts affinity and `RunIteration` pumps one message pass for a synchronous modal wait.

| [INDEX] | [SURFACE]                        | [CALL_SHAPE]                | [CAPABILITY]                                   |
| :-----: | :------------------------------- | :-------------------------- | :--------------------------------------------- |
|  [01]   | `Application.Invoke`             | `(Action)`                  | synchronous marshal onto the UI thread         |
|  [02]   | `Application.Invoke<T>`          | `(Func<T>) → T`             | synchronous marshalled read of a UI value      |
|  [03]   | `Application.AsyncInvoke`        | `(Action)`                  | fire-and-forget post to the UI thread          |
|  [04]   | `Application.InvokeAsync`        | `(Action) → Task`           | awaitable marshalled action                    |
|  [05]   | `Application.InvokeAsync<T>`     | `(Func<T>) → Task<T>`       | awaitable marshalled read                      |
|  [06]   | `Application.EnsureUIThread`     | `()`                        | throws off the UI thread — the affinity assert |
|  [07]   | `Application.RunIteration`       | `()`                        | pump one message-loop pass                     |
|  [08]   | `UITimer` ctor                   | `(EventHandler<EventArgs>)` | construct a clock bound to an elapsed handler  |
|  [09]   | `UITimer.Start` / `UITimer.Stop` | `()`                        | run and halt the repeating clock               |

[ENTRYPOINT_SCOPE]: display, input, and cursor state

- rail: eto-runtime

`Screen` enumerates displays and resolves the screen under a point or rectangle; `Mouse`/`Keyboard` read live device state; `Mouse.SetCursor` overrides the pointer glyph.

| [INDEX] | [SURFACE]                  | [CALL_SHAPE]            | [CAPABILITY]                          |
| :-----: | :------------------------- | :---------------------- | :------------------------------------ |
|  [01]   | `Screen.Screens`           | `→ IEnumerable<Screen>` | the connected-display roster          |
|  [02]   | `Screen.PrimaryScreen`     | `→ Screen`              | the primary display                   |
|  [03]   | `Screen.FromPoint`         | `(PointF) → Screen`     | the display containing a point        |
|  [04]   | `Screen.FromRectangle`     | `(RectangleF) → Screen` | the display best covering a rectangle |
|  [05]   | `Screen.GetImage`          | `(RectangleF) → Bitmap` | screen-region capture                 |
|  [06]   | `Mouse.IsSupported`        | `→ bool`                | platform pointer-state availability   |
|  [07]   | `Mouse.IsAnyButtonPressed` | `(MouseButtons) → bool` | pressed-button test                   |
|  [08]   | `Mouse.SetCursor`          | `(Cursor)`              | override the pointer cursor           |
|  [09]   | `Keyboard.IsKeyLocked`     | `(Keys) → bool`         | lock-key state                        |

[ENTRYPOINT_SCOPE]: typed transfer, drag, and tray

- rail: eto-runtime

`Clipboard` and `DataObject` expose the identical typed accessor set; a payload is set and read under one MIME `type` string. `DragEventArgs.SetDropDescription` annotates the OS drag cursor; `TrayIndicator` and `Notification` deliver tray presence.

| [INDEX] | [SURFACE]                          | [CALL_SHAPE]                    | [CAPABILITY]                         |
| :-----: | :--------------------------------- | :------------------------------ | :----------------------------------- |
|  [01]   | `Clipboard.SetString`              | `(string value, string type)`   | write a typed text payload           |
|  [02]   | `Clipboard.SetData`                | `(byte[] value, string type)`   | write a typed byte payload           |
|  [03]   | `Clipboard.SetDataStream`          | `(Stream stream, string type)`  | write a typed stream payload         |
|  [04]   | `Clipboard.SetObject`              | `(object value, string type)`   | write a typed boxed payload          |
|  [05]   | `Clipboard.GetString`              | `(string type) → string`        | read a typed text payload            |
|  [06]   | `Clipboard.GetData`                | `(string type) → byte[]`        | read a typed byte payload            |
|  [07]   | `Clipboard.GetDataStream`          | `(string type) → Stream`        | read a typed stream payload          |
|  [08]   | `Clipboard.GetObject<T>`           | `(string type) → T`             | read a typed boxed payload           |
|  [09]   | `Clipboard.Contains`               | `(string type) → bool`          | presence probe for a MIME type       |
|  [10]   | `Clipboard.Clear`                  | `()`                            | empty the clipboard                  |
|  [11]   | `DataObject.SetString`             | `(string value, string type)`   | write onto the drag payload          |
|  [12]   | `DataObject.GetObject<T>`          | `(string type) → T`             | read from the drag payload           |
|  [13]   | `DragEventArgs.SetDropDescription` | `(string format, string inner)` | annotate the OS drop cursor          |
|  [14]   | `Notification.Show`                | `(TrayIndicator = null)`        | deliver a toast, optionally anchored |
|  [15]   | `TrayIndicator.SetMenu`            | `(ContextMenu)`                 | bind the tray context menu           |
|  [16]   | `TrayIndicator.Show` / `.Hide`     | `()`                            | show and hide the tray presence      |

## [04]-[IMPLEMENTATION_LAW]

[THREAD_AFFINITY]:

- Every control-tree read or write executes on the UI thread; a background producer crosses through exactly one `Application` dispatch shape. Synchronous `Invoke`/`Invoke<T>` blocks the caller and returns the UI-side result; `AsyncInvoke` posts without a completion signal; `InvokeAsync`/`InvokeAsync<T>` returns a `Task` awaited off-thread. `EnsureUIThread` is the guard a UI-only method opens with; `RunIteration` pumps the loop for a hand-rolled synchronous wait rather than blocking it.
- `UITimer` is the framework clock and fires `Elapsed` on the UI thread; it is the portable fallback pace where a platform has no higher-fidelity display-link (`api-macos-native` owns the macOS display-link pace).

[TRANSFER_CONTRACT]:

- `Clipboard` and `DataObject` are the same typed-payload shape under two lifetimes: the clipboard is process-external and persistent, the data object is drag-scoped. A payload is always keyed by a MIME `type` string, and `SetString`/`SetData`/`SetDataStream`/`SetObject` pair one-to-one with the `Get*` readers plus `Contains`. Drag negotiation carries the `DataObject` on `DragEventArgs.Data`, declares `AllowedEffects`, and resolves the committed `Effects`.

[STACKING]:

- `LanguageExt`(`.api/api-languageext`): a UI-thread dispatch wraps into `Eff<A>`/`IO<A>` so the boundary composes marshalled work as an effect and folds the outcome to `Fin<A>` — `Application.Invoke<T>`/`InvokeAsync<T>` become effectful reads, never raw blocking calls threaded through domain code. `Option<A>` lifts every nullable transfer read (`Clipboard.GetString(type)`, `DataObject.GetObject<T>(type)` gated by `Contains(type)`) so an absent MIME payload is `None`, not a null. `UITimer` and `TrayIndicator` are resource-scoped through the `use` rail: `Start`/`Show` acquire, `Stop`/`Hide` release, so the clock and the tray icon never leak past their owning scope.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions`): the `Cursors` roster, `DragEffects`, and `MouseButtons` bind as `[SmartEnum]`/flag owners routed by generated `Switch`/`Map` instead of raw static-field reads and bitwise tests; a clipboard MIME `type` binds as a `[ValueObject<string>]` so transfer access is keyed by a validated owner rather than a bare `string type` argument.

[LOCAL_ADMISSION]:

- `Eto.Forms` runtime state is host-provided and never re-declared; a Rasm owner internalizes a dispatch, transfer, timer, or tray concern behind one canonical rail so downstream code composes a marshalled effect or a keyed payload, never `Application.Instance`, a stringy MIME argument, or a raw static-field cursor lookup.

[RAIL_LAW]:

- Package: `Eto.Forms`
- Owns: `Application` UI-thread dispatch and loop iteration, `UITimer`, `Screen`/`Mouse`/`Keyboard`/`Cursors` device and display state, `Clipboard`/`DataObject` typed transfer, `DragEffects`/`DragEventArgs`, `Notification`, `TrayIndicator`
- Accept: UI-thread marshalling, clock pacing, live input/display projection, typed clipboard and drag payloads, tray presence and toast delivery
- Reject: control/layout/window/menu construction (`api-eto-forms`), custom painting (`api-eto-drawing`), document output (`api-eto-printing`), platform selection and native hosting (`api-eto-platform`), and leaking `Application.Instance` or stringy MIME keys past the owning rail

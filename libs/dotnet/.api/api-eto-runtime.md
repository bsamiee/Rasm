# [RASM_API_ETO_RUNTIME]

`Eto.Forms` ambient runtime is the process-wide singleton set beside the control tree: UI-thread dispatch and message-loop iteration, the repeating clock, live input and cursor state, typed clipboard and drag transfer, tray presence and toast delivery, and per-display density. One Rhino process holds one `Application.Instance` and one `Clipboard.Instance`, so this surface admits no per-folder partition — the branch tier owns it whole and each host-boundary folder registers it and states its own composition law over it.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: application dispatch and clock — `Application` is the process singleton reached through `Application.Instance`, and UI-thread affinity routes every control mutation through one of its dispatch shapes

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                    |
| :-----: | :---------------------------- | :------------ | :-------------------------------------------------------------- |
|  [01]   | `Application`                 | class         | singleton UI-thread dispatch, message-loop iteration, lifecycle |
|  [02]   | `UITimer`                     | class         | widget-free repeating UI-thread clock                           |
|  [03]   | `UIThreadCheckMode`           | enum          | off-thread access policy                                        |
|  [04]   | `NotificationEventArgs`       | class         | notification-activation payload                                 |
|  [05]   | `LocalizeEventArgs`           | class         | localization payload                                            |
|  [06]   | `UnhandledExceptionEventArgs` | class         | unhandled-exception payload                                     |

[APPLICATION_STATE]: `Instance` `MainForm` `Windows` `Name` `Theme` `IsActive` `IsUIThread` `QuitIsSupported` `CommonModifier` `AlternateModifier` `BadgeLabel` `UIThreadCheckMode`
[UITIMER_STATE]: `Interval` `Started` `Elapsed`

[PUBLIC_TYPE_SCOPE]: input, cursor, and display state — static projections of live device and display state

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                                            |
| :-----: | :------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `Screen`             | class         | display enumeration, bounds, density, and screen grab   |
|  [02]   | `Keyboard`           | static class  | live modifier state, lock-key query, `ModifiersChanged` |
|  [03]   | `Mouse`              | static class  | live pointer position, pressed buttons, cursor override |
|  [04]   | `Keys`               | flags enum    | key and modifier vocabulary                             |
|  [05]   | `MouseButtons`       | flags enum    | pressed-button set                                      |
|  [06]   | `Cursor`             | class         | a cursor handle applied to a control or the pointer     |
|  [07]   | `Cursors`            | static class  | built-in cursor handle roster                           |
|  [08]   | `MouseEventArgs`     | class         | per-event pointer snapshot                              |
|  [09]   | `KeyEventArgs`       | class         | per-event key snapshot                                  |
|  [10]   | `TextInputEventArgs` | class         | composed-text event payload                             |
|  [11]   | `SystemColors`       | static class  | host-consistent theme colours                           |

[POINTER_ARGS]: `MouseEventArgs` carries `Modifiers` `Buttons` `Location` `Handled` `Pressure` (`float`) `Delta` (`SizeF`) and NOTHING else
[MODIFIER_MASK]: `Keys.ModifierMask = 0xF000` over `Shift = 0x1000` `Alt = 0x2000` `Control = 0x4000` `Application = 0x8000`; `F1 = 0x1B` through `F12 = 0x26` are contiguous
[BUTTON_SET]: `MouseButtons` is `[Flags]`: `None = 0` `Primary = 1` `Alternate = 2` `Middle = 4`
[KEYBOARD_STATE]: `Modifiers` `SupportedLockKeys` `ModifiersChanged`
[MOUSE_STATE]: `IsSupported` `Position` `Buttons`
[SCREEN_STATE]: `Bounds` `WorkingArea` `DisplayBounds` `LogicalPixelSize` `Scale` `RealScale` `DPI` `RealDPI` `BitsPerPixel` `IsPrimary`
[CURSOR_ROSTER]: `Default` `Arrow` `Crosshair` `Pointer` `IBeam` `Move` `SizeAll` `NotAllowed` `VerticalSplit` `HorizontalSplit` `SizeLeft` `SizeTop` `SizeRight` `SizeBottom` `SizeTopLeft` `SizeTopRight` `SizeBottomLeft` `SizeBottomRight`

- `MouseEventArgs` reports NO click count — the platform raises `MouseDoubleClick` as its own event, so a single-click derivation off one release is unspellable and `Pressure` defaults to `1f` when the constructor is handed none.
- `Keyboard.SupportedLockKeys` is an `IEnumerable<Keys>` membership set, never a `Keys` flag mask.
- `Mouse.Position` is settable — assigning it warps the system pointer.

[PUBLIC_TYPE_SCOPE]: typed transfer and drag — `Clipboard` and `DataObject` share one keyed-payload contract under two lifetimes, the clipboard process-external and persistent, the data object drag-scoped

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                                |
| :-----: | :-------------- | :------------ | :---------------------------------------------------------- |
|  [01]   | `Clipboard`     | class         | singleton typed clipboard read, write, and clear            |
|  [02]   | `DataObject`    | class         | drag-scoped typed payload keyed by MIME type                |
|  [03]   | `IDataObject`   | interface     | the keyed get-and-set contract both carriers implement      |
|  [04]   | `DataFormats`   | static class  | well-known transfer-type identifiers                        |
|  [05]   | `DragEffects`   | flags enum    | `Copy`/`Move`/`Link` allowed-effect negotiation             |
|  [06]   | `DragEventArgs` | class         | drop location, `Data`, `AllowedEffects`, resolved `Effects` |

[CLIPBOARD_STATE]: `Instance` `Types` `Text` `Html` `Image` `Uris` `ContainsText` `ContainsHtml` `ContainsImage` `ContainsUris`
[FORMAT_IDENTIFIERS]: `DataFormats` publishes `Text` `Html` `Color` ALONE — the typed image and URI payloads have no identifier row, so a well-known-format roster keys on the `Clipboard` accessor names, never on `DataFormats`
[DATAOBJECT_STATE]: `Text` `Html` `Image` `Uris` `Types` `TypeName` `Value`

[PUBLIC_TYPE_SCOPE]: notification and tray — `TrayIndicator` owns persistent tray presence, `Notification` the transient toast optionally anchored to it

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                                         |
| :-----: | :-------------- | :------------ | :--------------------------------------------------- |
|  [01]   | `TrayIndicator` | class         | persistent tray icon with a `ContextMenu`, show/hide |
|  [02]   | `Notification`  | class         | transient system toast, optionally tray-anchored     |

[NOTIFICATION_STATE]: `Title` `Message` `ContentImage` `UserData` `RequiresTrayIndicator` `Activated`
[TRAY_STATE]: `Title` `Image` `Menu` `Visible` `Activated`

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: UI-thread dispatch, iteration, and lifecycle

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :----------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `Application.Invoke(Action)`                     | instance | synchronous marshal onto the UI thread         |
|  [02]   | `Application.Invoke(Func<T>) -> T`               | instance | synchronous marshalled read of a UI value      |
|  [03]   | `Application.AsyncInvoke(Action)`                | instance | fire-and-forget post to the UI thread          |
|  [04]   | `Application.InvokeAsync(Action) -> Task`        | instance | awaitable marshalled action                    |
|  [05]   | `Application.InvokeAsync(Func<T>) -> Task<T>`    | instance | awaitable marshalled read                      |
|  [06]   | `Application.EnsureUIThread()`                   | instance | throws off the UI thread — the affinity assert |
|  [07]   | `Application.IsUIThread -> bool`                 | property | test current-thread affinity                   |
|  [08]   | `Application.RunIteration()`                     | instance | pump one message-loop pass                     |
|  [09]   | `Application.Quit()`                             | instance | application teardown                           |
|  [10]   | `Application.Open(string)`                       | instance | open a URL through the host                    |
|  [11]   | `Application.Localize(object, string) -> string` | instance | localization lookup                            |
|  [12]   | `Application.NotificationActivated`              | event    | route notification activation by user data     |
|  [13]   | `Application.Initialized`                        | event    | initialization hook                            |
|  [14]   | `Application.Terminating`                        | event    | termination hook                               |
|  [15]   | `Application.UnhandledException`                 | event    | unhandled-exception hook                       |
|  [16]   | `UITimer(EventHandler<EventArgs>)`               | ctor     | construct a clock bound to an elapsed handler  |
|  [17]   | `UITimer.Start()` / `UITimer.Stop()`             | instance | run and halt the repeating clock               |

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
|  [09]   | `Keyboard.IsKeyLocked(Keys) -> bool`             | static   | lock-key state read                   |

[ENTRYPOINT_SCOPE]: typed transfer, drag, and tray

Every payload keys by a MIME type string; `SetString`/`SetData`/`SetDataStream`/`SetObject` pair one-to-one with the `Get*` readers and the `Contains` probe.

| [INDEX] | [SURFACE]                                          | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `Clipboard.SetString(string, string)`              | instance | write a typed text payload           |
|  [02]   | `Clipboard.SetData(byte[], string)`                | instance | write a typed byte payload           |
|  [03]   | `Clipboard.SetDataStream(Stream, string)`          | instance | write a typed stream payload         |
|  [04]   | `Clipboard.SetObject(object, string)`              | instance | write a typed boxed payload          |
|  [05]   | `Clipboard.GetString(string) -> string`            | instance | read a typed text payload            |
|  [06]   | `Clipboard.GetData(string) -> byte[]`              | instance | read a typed byte payload            |
|  [07]   | `Clipboard.GetDataStream(string) -> Stream`        | instance | read a typed stream payload          |
|  [08]   | `Clipboard.GetObject<T>(string) -> T`              | instance | read a typed boxed payload           |
|  [09]   | `Clipboard.Contains(string) -> bool`               | instance | presence probe for a MIME type       |
|  [10]   | `Clipboard.Clear()`                                | instance | empty the clipboard                  |
|  [11]   | `DataObject.SetString(string, string)`             | instance | write onto the drag payload          |
|  [12]   | `DataObject.GetObject<T>(string) -> T`             | instance | read from the drag payload           |
|  [13]   | `DragEventArgs.SetDropDescription(string, string)` | instance | annotate the OS drop cursor          |
|  [14]   | `Notification.Show(TrayIndicator)`                 | instance | deliver a toast, optionally anchored |
|  [15]   | `TrayIndicator.SetMenu(ContextMenu)`               | instance | bind the tray context menu           |
|  [16]   | `TrayIndicator.Show()` / `TrayIndicator.Hide()`    | instance | show and hide the tray presence      |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every control-tree read or write executes on the UI thread and a background producer crosses through exactly one `Application` dispatch shape: `Invoke`/`Invoke<T>` block and return the UI-side result, `AsyncInvoke` posts without completion, `InvokeAsync`/`InvokeAsync<T>` return an awaited `Task`. `EnsureUIThread` guards a UI-only method and `RunIteration` pumps the loop for a synchronous wait.
- `UITimer` owns the portable repeating UI-thread tick where a platform exposes no higher-fidelity display link; the macOS display-link pace is `.api/api-macos-native.md`. Exactly one boundary owner per host leases the raw timer into its cadence surface; a second consumer composes that lease, never a second `UITimer` beside it.
- Input reads live: `Keyboard.Modifiers` and `Mouse.Position`/`Buttons` are ambient, distinct from the per-event `MouseEventArgs`/`KeyEventArgs` snapshots a control raises.
- Transfer is one keyed-payload shape under two lifetimes. `IDataObject` carries the whole keyed contract so one interface-typed body serves both; only the stream pair is class-level on each carrier and off the interface. Drag negotiation carries the `DataObject` on `DragEventArgs.Data`, declares `AllowedEffects`, and resolves the committed `Effects`.
- `Screen` carries the density facts a surface reads once per paint to place logical geometry into device pixels.

[STACKING]:
- `LanguageExt.Core`(`.api/api-languageext.md`): a UI-thread dispatch wraps into `Eff<A>`/`IO<A>` and folds to `Fin<A>`, so `Invoke<T>`/`InvokeAsync<T>` compose as effectful reads rather than raw blocking calls threaded through domain code; `Option<A>` lifts every nullable transfer read gated by `Contains(type)`; `UITimer` and `TrayIndicator` acquire and release through the `use` bracket — `Start`/`Show` acquire, `Stop`/`Hide` release — so the clock and tray icon never leak past their owning scope.
- `Thinktecture.Runtime.Extensions`(`.api/api-thinktecture-runtime-extensions.md`): `Cursors`, `DragEffects`, `MouseButtons`, `Keys`, and `UIThreadCheckMode` bind as `[SmartEnum]` and flag owners routed by generated `Switch`/`Map` instead of raw static-field reads and bitwise tests; the `Clipboard` typed-accessor names project onto a `[SmartEnum<string>]` payload-kind owner carrying its parse and serialize behaviour — `DataFormats` publishes only three identifiers and cannot key that roster, and a MIME type binds as `[ValueObject<string>]` so transfer access is keyed by a validated owner.
- `api-eto-forms`(`.api/api-eto-forms.md`): control invalidation and dialog presentation are the construction-side consumers that marshal through `Application.Instance`.
- `api-macos-native`(`.api/api-macos-native.md`): `Application.Invoke`, `UITimer`, and `Screen.LogicalPixelSize` are the host-neutral boundary the macOS layer replaces with `CADisplayLink` pacing and `NSScreen` refresh metrics for high-cadence work.

[LOCAL_ADMISSION]:
- A cross-thread marshal calls `Application.Instance` and a tick uses `UITimer`; a hand-rolled `SynchronizationContext` capture or `System.Threading.Timer` beside them is the deleted form.
- Transfer payloads ride the typed `Clipboard`/`DataObject` accessors keyed by `DataFormats`, and display density reads from `Screen`.
- Each boundary internalizes a dispatch, transfer, timer, or tray concern behind one canonical surface so downstream code composes a marshalled effect or a keyed payload, never `Application.Instance` or a stringy MIME key in a domain signature.

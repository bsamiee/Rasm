# [RASM_GRASSHOPPER_API_ETO_RUNTIME]

`Eto.Forms` owns the host-neutral runtime a GH2-hosted panel binds inside the Rhino process: UI-thread dispatch, repeating ticks, live input and cursor state, typed clipboard and drag transfer, OS notifications, and per-display density. `Application.Instance` is the sole UI-thread marshal seam every off-thread control mutation folds through; control construction, the drawing surface, and the binding rail stay with their own catalogs.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto`
- package: `Eto` (the cross-platform Eto.Forms UI framework, host-provided by RhinoWIP) (BSD-3-Clause)
- assembly: `Eto` (`Eto.dll`)
- namespace: `Eto.Forms`
- asset: host-provided — RhinoWIP ships `Eto.dll` under `RhCore.framework/Versions/A/Resources`; no NuGet admission
- rail: native UI

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: application dispatch and timers

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :---------------------------- | :------------ | :------------------------------ |
|  [01]   | `Application`                 | class         | UI-thread dispatch root         |
|  [02]   | `UITimer`                     | class         | repeating UI-thread tick        |
|  [03]   | `UIThreadCheckMode`           | enum          | off-thread access policy        |
|  [04]   | `NotificationEventArgs`       | class         | notification activation payload |
|  [05]   | `LocalizeEventArgs`           | class         | localization payload            |
|  [06]   | `UnhandledExceptionEventArgs` | class         | unhandled-exception payload     |

[Application]: `Instance` `MainForm` `Windows` `Name` `Theme` `IsActive` `IsUIThread` `QuitIsSupported` `CommonModifier` `AlternateModifier` `BadgeLabel` `UIThreadCheckMode`
[UITimer]: `Interval` `Started` `Elapsed`

[PUBLIC_TYPE_SCOPE]: keyboard, mouse, and cursor state

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                |
| :-----: | :------------------- | :------------ | :-------------------------- |
|  [01]   | `Keyboard`           | static class  | live modifier state         |
|  [02]   | `Mouse`              | static class  | live pointer state          |
|  [03]   | `Keys`               | enum          | modifier-key bitmask        |
|  [04]   | `MouseButtons`       | enum          | button bitmask              |
|  [05]   | `Cursor`             | class         | pointer glyph               |
|  [06]   | `Cursors`            | static class  | pointer-glyph roster        |
|  [07]   | `MouseEventArgs`     | class         | pointer-event payload       |
|  [08]   | `KeyEventArgs`       | class         | key-event payload           |
|  [09]   | `TextInputEventArgs` | class         | composed-text event payload |

[Keyboard]: `Modifiers` `SupportedLockKeys` `ModifiersChanged`
[Mouse]: `IsSupported` `Position` `Buttons`
[Cursors]: `Default` `Arrow` `Crosshair` `Pointer` `IBeam` `Move` `SizeAll` `NotAllowed` `VerticalSplit` `HorizontalSplit` `SizeLeft` `SizeTop` `SizeRight` `SizeBottom` `SizeTopLeft` `SizeTopRight` `SizeBottomLeft` `SizeBottomRight`

- `Keyboard.SupportedLockKeys`: an `IEnumerable<Keys>` membership set, not a `Keys` flag mask.
- `Mouse.Position`: settable — assigning it warps the system pointer.

[PUBLIC_TYPE_SCOPE]: typed data transfer

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                    |
| :-----: | :-------------- | :------------ | :------------------------------ |
|  [01]   | `Clipboard`     | class         | typed system transfer           |
|  [02]   | `DataObject`    | class         | typed transfer carrier          |
|  [03]   | `DataFormats`   | static class  | well-known transfer identifiers |
|  [04]   | `DragEffects`   | enum          | copy-move-link effect mask      |
|  [05]   | `DragEventArgs` | class         | drag payload                    |

[Clipboard]: `Instance` `Types` `Text` `Html` `Image` `Uris` `ContainsText` `ContainsHtml` `ContainsImage` `ContainsUris`
[DataObject]: `Text` `Html` `Image` `Uris` `Types` `TypeName` `Value`

[PUBLIC_TYPE_SCOPE]: notifications and screen metrics

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY] | [CAPABILITY]                  |
| :-----: | :-------------- | :------------ | :---------------------------- |
|  [01]   | `Notification`  | class         | operating-system alert        |
|  [02]   | `TrayIndicator` | class         | menu-bar item                 |
|  [03]   | `Screen`        | class         | per-display density metrics   |
|  [04]   | `SystemColors`  | static class  | host-consistent theme colours |

[Notification]: `Title` `Message` `ContentImage` `UserData` `RequiresTrayIndicator` `Activated`
[TrayIndicator]: `Title` `Image` `Menu` `Visible` `Activated`
[Screen]: `Bounds` `WorkingArea` `DisplayBounds` `RealScale` `RealDPI` `BitsPerPixel` `IsPrimary`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: application dispatch

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]                 |
| :-----: | :----------------------------------------------- | :------- | :--------------------------- |
|  [01]   | `Application.Invoke(Action)`                     | instance | blocking UI-thread dispatch  |
|  [02]   | `Application.Invoke(Func<T>) -> T`               | instance | blocking UI-thread dispatch  |
|  [03]   | `Application.AsyncInvoke(Action)`                | instance | non-blocking UI-thread queue |
|  [04]   | `Application.InvokeAsync(Action) -> Task`        | instance | awaitable UI-thread dispatch |
|  [05]   | `Application.InvokeAsync(Func<T>) -> Task<T>`    | instance | awaitable UI-thread dispatch |
|  [06]   | `Application.EnsureUIThread()`                   | instance | UI-thread assertion          |
|  [07]   | `Application.RunIteration()`                     | instance | single run-loop pump         |
|  [08]   | `Application.Quit()`                             | instance | application teardown         |
|  [09]   | `Application.Open(string)`                       | instance | URL open                     |
|  [10]   | `Application.Localize(object, string) -> string` | instance | localization                 |
|  [11]   | `Application.NotificationActivated`              | event    | notification activation hook |
|  [12]   | `Application.Initialized`                        | event    | initialization hook          |
|  [13]   | `Application.Terminating`                        | event    | termination hook             |
|  [14]   | `Application.UnhandledException`                 | event    | unhandled-exception hook     |

[ENTRYPOINT_SCOPE]: timer, input, and clipboard

| [INDEX] | [SURFACE]                                   | [SHAPE]  | [CAPABILITY]          |
| :-----: | :------------------------------------------ | :------- | :-------------------- |
|  [01]   | `UITimer.Start()`                           | instance | starts repeating tick |
|  [02]   | `UITimer.Stop()`                            | instance | stops repeating tick  |
|  [03]   | `Keyboard.IsKeyLocked(Keys) -> bool`        | static   | lock-key state read   |
|  [04]   | `Clipboard.SetString(string, string)`       | instance | typed string write    |
|  [05]   | `Clipboard.GetString(string) -> string`     | instance | typed string read     |
|  [06]   | `Clipboard.SetObject(object, string)`       | instance | typed object write    |
|  [07]   | `Clipboard.GetObject<T>(string) -> T`       | instance | typed object read     |
|  [08]   | `Clipboard.SetDataStream(Stream, string)`   | instance | raw stream write      |
|  [09]   | `Clipboard.GetDataStream(string) -> Stream` | instance | raw stream read       |

[ENTRYPOINT_SCOPE]: notification and screen resolution

| [INDEX] | [SURFACE]                          | [SHAPE]  | [CAPABILITY]              |
| :-----: | :--------------------------------- | :------- | :------------------------ |
|  [01]   | `Notification.Show(TrayIndicator)` | instance | posts optional tray alert |
|  [02]   | `Screen.PrimaryScreen`             | static   | primary display           |
|  [03]   | `Screen.Screens`                   | static   | display enumeration       |
|  [04]   | `Screen.LogicalPixelSize`          | property | logical-pixel projection  |
|  [05]   | `Screen.Scale`                     | property | display scale             |
|  [06]   | `Screen.DPI`                       | property | display density           |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Application.Instance` is the one UI-thread seam: `Invoke` blocks for a result, `AsyncInvoke` fire-and-forgets, `InvokeAsync` awaits, and every off-thread control mutation folds through one, never a raw thread hop.
- `UITimer` owns the repeating UI-thread tick; a paint or animation cadence subscribes `Elapsed` and toggles `Started`.
- Input reads live: `Keyboard.Modifiers` and `Mouse.Position`/`Buttons` are ambient, distinct from the per-event `MouseEventArgs`/`KeyEventArgs` snapshots a control raises.
- Transfer is typed and keyed by a `DataFormats` type string across `Clipboard` and `DataObject`, which share one accessor family.
- `Screen` carries the density facts a panel reads once per paint to place logical geometry into device pixels.

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): a UI-thread result marshal defers as `Eff<A>` run through `Application.Instance.Invoke(() => eff.Run())`, landing `Fin<A>`; a clipboard read null-gates `Optional(Clipboard.Instance.GetString(type)).ToFin(error)`; a `UITimer` cadence drives an `IO<A>` step chain per tick.
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the `DataFormats` identifiers project onto a `[SmartEnum<string>]` payload-kind owner carrying its parse/serialize behaviour; `UIThreadCheckMode`, `DragEffects`, and `MouseButtons` are the host enum vocabularies a smart-enum owner wraps.
- `api-eto-forms`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-forms.md`): control invalidation and dialog presentation are the panel-side consumers that marshal through `Application.Instance`.
- `api-macos-native`(`libs/csharp/Rasm.Grasshopper/.api/api-macos-native.md`): `Application.Invoke`, `UITimer`, and `Screen.LogicalPixelSize` are the host-neutral boundary the macOS layer replaces with `CADisplayLink` pacing and `NSScreen` refresh metrics for high-cadence work.

[LOCAL_ADMISSION]:
- `Eto.Forms` runtime is host-provided and composed directly — a cross-thread marshal calls `Application.Instance` and a tick uses `UITimer`.
- Transfer payloads ride the typed `Clipboard`/`DataObject` accessors keyed by `DataFormats`.
- Display density reads from `Screen`.

[RAIL_LAW]:
- Package: `Eto`
- Owns: UI-thread dispatch, timers, live input and cursor state, typed clipboard and drag payloads, notifications, and per-display density for GH2-hosted panels
- Accept: cross-thread marshalling, repeating ticks, input and modifier reads, typed data transfer, OS alerts, density resolution
- Reject: a hand-rolled `SynchronizationContext` capture or `System.Threading.Timer` beside `Application.Instance`/`UITimer`, a stringly-parsed clipboard blob past the typed accessors, a hardcoded scale constant past `Screen`

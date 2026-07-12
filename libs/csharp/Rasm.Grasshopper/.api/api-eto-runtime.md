# [RASM_GRASSHOPPER_API_ETO_RUNTIME]

`Eto.Forms` owns the host-neutral runtime every GH2-hosted panel drives, and this catalog fixes that verified surface — UI-thread marshalling, timers, live input state, typed data transfer, notifications, and screen metrics. `Application.Instance` is the single dispatch root: synchronous `Invoke`, fire-and-forget `AsyncInvoke`, awaitable `InvokeAsync`, and the run-loop iteration hook all cross the UI thread through it. `UITimer` owns the repeating tick, `Keyboard`/`Mouse`/`Cursors` own live input and pointer state, `Clipboard`/`DataObject` own typed binary, stream, object, string, image, HTML, and URI payloads, and `Notification`/`TrayIndicator` own the OS alert surface. `Screen` projects per-display bounds, working area, logical-pixel scale, and DPI.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Eto`
- package: `Eto` (the cross-platform Eto.Forms UI framework, host-provided by RhinoWIP)
- license: BSD-3-Clause
- assembly: `Eto` (`Eto.dll`)
- namespace: `Eto.Forms`
- asset: host-provided — RhinoWIP ships `Eto.dll` under `RhCore.framework/Versions/A/Resources`; no NuGet admission
- rail: native UI

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: application dispatch and timers
- rail: native UI

`Application` exposes static `Instance` and the `MainForm`, `Windows`, `Name`, `IsUIThread`, `QuitIsSupported`, `CommonModifier`, `AlternateModifier`, `BadgeLabel`, `IsActive`, and `UIThreadCheckMode` properties. `UITimer` exposes `Interval`, `Started`, `Start()`, `Stop()`, and `Elapsed`.

| [INDEX] | [SYMBOL]                      | [KIND]     | [CAPABILITY]                    |
| :-----: | :---------------------------- | :--------- | :------------------------------ |
|  [01]   | `Application`                 | service    | UI-thread dispatch root         |
|  [02]   | `UITimer`                     | timer      | repeating UI-thread tick        |
|  [03]   | `UIThreadCheckMode`           | enum       | off-thread access policy        |
|  [04]   | `NotificationEventArgs`       | event-args | notification activation payload |
|  [05]   | `LocalizeEventArgs`           | event-args | localization payload            |
|  [06]   | `UnhandledExceptionEventArgs` | event-args | unhandled-exception payload     |

[PUBLIC_TYPE_SCOPE]: keyboard, mouse, and cursor state
- rail: native UI

`Keyboard` exposes static `Modifiers`, `SupportedLockKeys`, `IsKeyLocked(Keys)`, and `ModifiersChanged`; `SupportedLockKeys` is an `IEnumerable<Keys>` membership set rather than flags. `Mouse` exposes static `IsSupported`, settable `Position` as `PointF`, and `Buttons`. `Cursors` exposes `Default`, `Arrow`, `Crosshair`, `Pointer`, `IBeam`, `Move`, `NotAllowed`, `SizeAll`, eight directional size cursors, and `GetCursor(CursorType)`.

| [INDEX] | [SYMBOL]             | [KIND]      | [CAPABILITY]                |
| :-----: | :------------------- | :---------- | :-------------------------- |
|  [01]   | `Keyboard`           | input state | live modifier state         |
|  [02]   | `Mouse`              | input state | live pointer state          |
|  [03]   | `Keys`               | enum        | modifier-key bitmask        |
|  [04]   | `MouseButtons`       | enum        | button bitmask              |
|  [05]   | `Cursor`             | resource    | pointer glyph               |
|  [06]   | `Cursors`            | static      | pointer-glyph roster        |
|  [07]   | `MouseEventArgs`     | event-args  | pointer-event payload       |
|  [08]   | `KeyEventArgs`       | event-args  | key-event payload           |
|  [09]   | `TextInputEventArgs` | event-args  | composed-text event payload |

[PUBLIC_TYPE_SCOPE]: typed data transfer
- rail: native UI

`Clipboard` exposes static `Instance`; `Types`, `ContainsText`, `ContainsHtml`, `ContainsImage`, and `ContainsUris`; `Text`, `Html`, `Image`, and `Uris`; and `Clear()`. Its typed access surface comprises `SetData(byte[], type)`, `GetData(type)`, `SetDataStream`, `GetDataStream`, `SetString`, `GetString`, `SetObject`, `GetObject<T>`, and `Contains(type)`. `DataObject` mirrors the typed and format accessors under the `IDataObject` contract. `Control.DoDragDrop(DataObject, DragEffects[, Image, PointF])` initiates a copy, move, or link drag and returns `void`; the settled effect arrives on `Control.DragEnd`.

| [INDEX] | [SYMBOL]        | [KIND]     | [CAPABILITY]                    |
| :-----: | :-------------- | :--------- | :------------------------------ |
|  [01]   | `Clipboard`     | service    | typed system transfer           |
|  [02]   | `DataObject`    | payload    | typed transfer carrier          |
|  [03]   | `DataFormats`   | constants  | well-known transfer identifiers |
|  [04]   | `DragEffects`   | enum       | copy-move-link effect mask      |
|  [05]   | `DragEventArgs` | event-args | drag payload                    |

[PUBLIC_TYPE_SCOPE]: notifications and screen metrics
- rail: native UI

`Notification` exposes `Title`, `Message`, `Icon`, `ContentImage`, string `UserData`, `RequiresTrayIndicator`, and `Activated`; `NotificationEventArgs` carries string `ID` and `UserData`. `TrayIndicator` exposes `Title`, `Image`, `Menu`, `Visible`, and `Activated`. `Screen` exposes static `PrimaryScreen` and `Screens` plus `Bounds`, `WorkingArea`, `DisplayBounds`, `LogicalPixelSize`, `DPI`, `RealDPI`, `Scale`, `RealScale`, `BitsPerPixel`, and `IsPrimary`.

| [INDEX] | [SYMBOL]        | [KIND]   | [CAPABILITY]                  |
| :-----: | :-------------- | :------- | :---------------------------- |
|  [01]   | `Notification`  | service  | operating-system alert        |
|  [02]   | `TrayIndicator` | service  | menu-bar item                 |
|  [03]   | `Screen`        | metrics  | per-display density metrics   |
|  [04]   | `SystemColors`  | resource | host-consistent theme colours |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: application dispatch
- rail: native UI

| [INDEX] | [SURFACE]                             | [RESULT]  | [CAPABILITY]                 |
| :-----: | :------------------------------------ | :-------- | :--------------------------- |
|  [01]   | `Application.Invoke(Action)`          | —         | blocking UI-thread dispatch  |
|  [02]   | `Application.Invoke<T>(Func<T>)`      | `T`       | blocking UI-thread dispatch  |
|  [03]   | `Application.AsyncInvoke(Action)`     | —         | non-blocking UI-thread queue |
|  [04]   | `Application.InvokeAsync(Action)`     | `Task`    | awaitable UI-thread dispatch |
|  [05]   | `Application.InvokeAsync<T>(Func<T>)` | `Task<T>` | awaitable UI-thread dispatch |
|  [06]   | `Application.EnsureUIThread()`        | —         | UI-thread assertion          |
|  [07]   | `Application.RunIteration()`          | —         | single run-loop pump         |
|  [08]   | `Application.Quit()`                  | —         | application teardown         |
|  [09]   | `Application.Open(string url)`        | —         | URL open                     |
|  [10]   | `Application.Localize(string)`        | `string`  | localization                 |
|  [11]   | `Application.NotificationActivated`   | event     | notification activation hook |
|  [12]   | `Application.Initialized`             | event     | initialization hook          |
|  [13]   | `Application.Terminating`             | event     | termination hook             |
|  [14]   | `Application.UnhandledException`      | event     | unhandled-exception hook     |

[ENTRYPOINT_SCOPE]: timer, input, and clipboard
- rail: native UI

| [INDEX] | [SURFACE]                               | [RESULT] | [CAPABILITY]          |
| :-----: | :-------------------------------------- | :------- | :-------------------- |
|  [01]   | `UITimer.Start()`                       | —        | starts repeating tick |
|  [02]   | `UITimer.Stop()`                        | —        | stops repeating tick  |
|  [03]   | `Keyboard.IsKeyLocked(Keys)`            | `bool`   | lock-key state read   |
|  [04]   | `Clipboard.SetString(string, type)`     | —        | typed string write    |
|  [05]   | `Clipboard.GetString(type)`             | `string` | typed string read     |
|  [06]   | `Clipboard.SetObject(object, type)`     | —        | typed object write    |
|  [07]   | `Clipboard.GetObject<T>(type)`          | `T`      | typed object read     |
|  [08]   | `Clipboard.SetDataStream(Stream, type)` | —        | raw stream write      |
|  [09]   | `Clipboard.GetDataStream(type)`         | `Stream` | raw stream read       |

[ENTRYPOINT_SCOPE]: notification and screen resolution
- rail: native UI

| [INDEX] | [SURFACE]                           | [KIND]     | [CAPABILITY]              |
| :-----: | :---------------------------------- | :--------- | :------------------------ |
|  [01]   | `Notification.Show(TrayIndicator?)` | method     | posts optional tray alert |
|  [02]   | `Screen.PrimaryScreen`              | static get | primary display           |
|  [03]   | `Screen.Screens`                    | static get | display enumeration       |
|  [04]   | `Screen.LogicalPixelSize`           | get        | logical-pixel projection  |
|  [05]   | `Screen.Scale`                      | get        | display scale             |
|  [06]   | `Screen.DPI`                        | get        | display density           |

## [04]-[IMPLEMENTATION_LAW]

[DISPATCH_TOPOLOGY]:
- `Application.Instance` is the one UI-thread seam: `Invoke` blocks for a result, `AsyncInvoke` fire-and-forgets, `InvokeAsync` awaits; a host callback that mutates a control off-thread routes through one of these, never a raw thread hop
- `UITimer` owns the repeating UI-thread tick; a paint or animation cadence subscribes `Elapsed` and toggles `Started`
- input state is read live: `Keyboard.Modifiers` and `Mouse.Position`/`Buttons` are ambient reads, distinct from the per-event `MouseEventArgs`/`KeyEventArgs` snapshots a control raises
- transfer is typed: `Clipboard` and `DataObject` carry the same `SetData`/`GetData`/`SetString`/`SetObject`/`SetDataStream` family keyed by a `DataFormats` type string
- `Screen` carries the density facts (`LogicalPixelSize`, `Scale`, `DPI`) a panel reads once per paint to place logical geometry into device pixels

[STACKING]:
- `api-languageext`(`libs/csharp/.api/api-languageext.md`): a UI-thread result marshal is an `Eff<A>` deferred until `Application.Instance.Invoke(() => eff.Run())`, landing on `Fin<A>`; a clipboard read null-gates through `Optional(Clipboard.Instance.GetString(type)).ToFin(error)`; a repeating `UITimer` cadence drives an `IO<A>` step chain per tick
- `api-thinktecture-runtime-extensions`(`libs/csharp/.api/api-thinktecture-runtime-extensions.md`): the transfer `DataFormats` identifiers project onto a `[SmartEnum<string>]` payload-kind owner carrying its parse/serialize behaviour; `UIThreadCheckMode`, `DragEffects`, and `MouseButtons` are the host enum vocabularies a smart-enum owner wraps where the panel attaches routing
- `api-eto-forms`(`libs/csharp/Rasm.Grasshopper/.api/api-eto-forms.md`): control invalidation and dialog presentation are the panel-side consumers that marshal through `Application.Instance`
- `api-macos-native`(`libs/csharp/Rasm.Grasshopper/.api/api-macos-native.md`): `Application.Invoke`, `UITimer`, and `Screen.LogicalPixelSize` are the host-neutral boundary the macOS layer replaces with `CADisplayLink` pacing and `NSScreen` refresh metrics for high-cadence work

[LOCAL_ADMISSION]:
- `Eto.Forms` runtime is host-provided and composed directly — a cross-thread marshal calls `Application.Instance` and a tick uses `UITimer`, never a hand-rolled `SynchronizationContext` capture or `System.Threading.Timer` beside them
- transfer payloads ride the typed `Clipboard`/`DataObject` accessors keyed by `DataFormats`, never a stringly-parsed blob
- display density is read from `Screen`, never a hardcoded scale constant

[RAIL_LAW]:
- Package: `Eto`
- Owns: UI-thread dispatch, timers, live input and cursor state, typed clipboard and drag payloads, notifications, and screen metrics for GH2-hosted panels
- Accept: cross-thread marshalling, repeating ticks, input and modifier reads, typed data transfer, OS alerts, per-display density resolution
- Reject: control construction and layout (`api-eto-forms`), the 2D drawing surface (`api-eto-drawing`), the data-binding rail (`api-eto-binding`), macOS-native compositing and pacing (`api-macos-native`)

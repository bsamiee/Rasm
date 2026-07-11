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

| [INDEX] | [SYMBOL]                                                                      | [KIND]     | [CAPABILITY]                                                                                                                                                                  |
| :-----: | :---------------------------------------------------------------------------- | :--------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Application`                                                                 | service    | dispatch root; static `Instance`, `MainForm`/`Windows`/`Name`/`IsUIThread`/`QuitIsSupported`/`CommonModifier`/`AlternateModifier`/`BadgeLabel`/`IsActive`/`UIThreadCheckMode` |
|  [02]   | `UITimer`                                                                     | timer      | repeating UI-thread tick; `Interval`/`Started`, `Start()`/`Stop()`, `Elapsed`                                                                                                 |
|  [03]   | `UIThreadCheckMode`                                                           | enum       | none/error/warning policy for off-thread access detection                                                                                                                     |
|  [04]   | `NotificationEventArgs` / `LocalizeEventArgs` / `UnhandledExceptionEventArgs` | event-args | notification activation, localization, and unhandled-exception payloads                                                                                                       |

[PUBLIC_TYPE_SCOPE]: keyboard, mouse, and cursor state
- rail: native UI

| [INDEX] | [SYMBOL]                                                 | [KIND]      | [CAPABILITY]                                                                                                                                                                     |
| :-----: | :------------------------------------------------------- | :---------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Keyboard`                                               | input state | live modifiers; static `Modifiers`/`SupportedLockKeys` (`IEnumerable<Keys>`, membership not flags), `IsKeyLocked(Keys)`, `ModifiersChanged`                                      |
|  [02]   | `Mouse`                                                  | input state | live pointer; static `IsSupported`/`Position` (`PointF`, settable)/`Buttons`                                                                                                     |
|  [03]   | `Keys` / `MouseButtons`                                  | enum        | modifier/key and button bitmask vocabularies                                                                                                                                     |
|  [04]   | `Cursor` / `Cursors`                                     | resource    | pointer glyph; `Cursors` static roster (`Default`/`Arrow`/`Crosshair`/`Pointer`/`IBeam`/`Move`/`NotAllowed`/`SizeAll` + eight directional size cursors), `GetCursor(CursorType)` |
|  [05]   | `MouseEventArgs` / `KeyEventArgs` / `TextInputEventArgs` | event-args  | pointer, key, and composed-text event payloads                                                                                                                                   |

[PUBLIC_TYPE_SCOPE]: typed data transfer
- rail: native UI

| [INDEX] | [SYMBOL]                        | [KIND]            | [CAPABILITY]                                                                                                                                                                               |
| :-----: | :------------------------------ | :---------------- | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Clipboard`                     | service           | system clipboard; static `Instance`, `Types`/`ContainsText`/`ContainsHtml`/`ContainsImage`/`ContainsUris`, `Text`/`Html`/`Image`/`Uris`, `Clear()`                                         |
|  [02]   | `Clipboard` typed access        | service           | `SetData(byte[], type)`/`GetData(type)`, `SetDataStream`/`GetDataStream`, `SetString`/`GetString`, `SetObject`/`GetObject<T>`, `Contains(type)`                                            |
|  [03]   | `DataObject`                    | payload           | drag/clipboard carrier mirroring the `Clipboard` typed and format accessors; `IDataObject` contract                                                                                        |
|  [04]   | `DataFormats`                   | constants         | well-known transfer-type identifiers                                                                                                                                                       |
|  [05]   | `DragEffects` / `DragEventArgs` | enum / event-args | copy/move/link effect mask and drag payload; `Control.DoDragDrop(DataObject, DragEffects[, Image, PointF])` initiates and returns `void` — the settled effect arrives on `Control.DragEnd` |

[PUBLIC_TYPE_SCOPE]: notifications and screen metrics
- rail: native UI

| [INDEX] | [SYMBOL]        | [KIND]   | [CAPABILITY]                                                                                                                                                            |
| :-----: | :-------------- | :------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `Notification`  | service  | OS alert; `Title`/`Message`/`Icon`/`ContentImage`/`UserData` (`string`)/`RequiresTrayIndicator`, `Activated`; `NotificationEventArgs` carries `ID`/`UserData` strings   |
|  [02]   | `TrayIndicator` | service  | menu-bar item; `Title`/`Image`/`Menu`/`Visible`, `Activated`                                                                                                            |
|  [03]   | `Screen`        | metrics  | one display; static `PrimaryScreen`/`Screens`, `Bounds`/`WorkingArea`/`DisplayBounds`/`LogicalPixelSize`/`DPI`/`RealDPI`/`Scale`/`RealScale`/`BitsPerPixel`/`IsPrimary` |
|  [04]   | `SystemColors`  | resource | theme-derived colour roster for host-consistent painting                                                                                                                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: application dispatch
- rail: native UI

| [INDEX] | [SURFACE]                                                                                  | [CALL_SHAPE]                                     | [CAPABILITY]                                    |
| :-----: | :----------------------------------------------------------------------------------------- | :----------------------------------------------- | :---------------------------------------------- |
|  [01]   | `Application.Invoke`                                                                       | `(Action)` / `<T>(Func<T>)` → `T`                | runs on the UI thread and blocks for the result |
|  [02]   | `Application.AsyncInvoke`                                                                  | `(Action)`                                       | queues on the UI thread without waiting         |
|  [03]   | `Application.InvokeAsync`                                                                  | `(Action)` → `Task` / `<T>(Func<T>)` → `Task<T>` | awaitable UI-thread marshal                     |
|  [04]   | `Application.EnsureUIThread` / `RunIteration`                                              | `()`                                             | thread assertion and one run-loop pump          |
|  [05]   | `Application.Quit` / `Open` / `Localize`                                                   | `()` / `(string url)` / `(string)` → `string`    | app teardown, URL open, and localization        |
|  [06]   | `Application.NotificationActivated` / `Initialized` / `Terminating` / `UnhandledException` | event                                            | lifecycle and notification-activation hooks     |

[ENTRYPOINT_SCOPE]: timer, input, and clipboard
- rail: native UI

| [INDEX] | [SURFACE]                                   | [CALL_SHAPE]                           | [CAPABILITY]                        |
| :-----: | :------------------------------------------ | :------------------------------------- | :---------------------------------- |
|  [01]   | `UITimer.Start` / `Stop`                    | `()`                                   | starts and stops the repeating tick |
|  [02]   | `Keyboard.IsKeyLocked`                      | `(Keys)` → `bool`                      | lock-key state read                 |
|  [03]   | `Clipboard.SetString` / `GetString`         | `(string, type)` / `(type)` → `string` | typed string transfer               |
|  [04]   | `Clipboard.SetObject` / `GetObject`         | `(object, type)` / `<T>(type)` → `T`   | typed object transfer               |
|  [05]   | `Clipboard.SetDataStream` / `GetDataStream` | `(Stream, type)` / `(type)` → `Stream` | raw stream transfer                 |

[ENTRYPOINT_SCOPE]: notification and screen resolution
- rail: native UI

| [INDEX] | [SURFACE]                                   | [CALL_SHAPE]       | [CAPABILITY]                                       |
| :-----: | :------------------------------------------ | :----------------- | :------------------------------------------------- |
|  [01]   | `Notification.Show`                         | `(TrayIndicator?)` | posts the alert, optionally through a tray item    |
|  [02]   | `Screen.PrimaryScreen` / `Screens`          | static get         | primary display and full enumeration               |
|  [03]   | `Screen.LogicalPixelSize` / `Scale` / `DPI` | get                | logical-pixel and density projection for a display |

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

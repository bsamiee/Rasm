# [RASM_APPUI_API_LIBMPV]

`HanumanInstitute.LibMpv` owns the managed libmpv client: `MpvContext` projects the mpv command, property, and option API as typed members over an embedded OpenGL, software, and native render path, and `HanumanInstitute.LibMpv.Avalonia` binds that path into an Avalonia visual tree through the `MpvView` control. Together they own the AppUi Editing MediaSurface decode-and-playback rail — the on-screen counterpart to the `FFmpeg.AutoGen` encode-out owner.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HanumanInstitute.LibMpv`
- package: `HanumanInstitute.LibMpv` (MIT)
- assembly: `HanumanInstitute.LibMpv`
- namespace: `HanumanInstitute.LibMpv` — context, command, option, property, and event types
- namespace: `HanumanInstitute.LibMpv.Core` — low-level `MpvApi` P/Invoke, `MpvFormat` / `MpvError` / `MpvLogLevel` / `MpvNode`
- target: `lib/net10.0` + `lib/netstandard2.0`
- asset: managed client over the libmpv native runtime
- rail: media

[PACKAGE_SURFACE]: `HanumanInstitute.LibMpv.Avalonia`
- package: `HanumanInstitute.LibMpv.Avalonia` (MIT)
- assembly: `HanumanInstitute.LibMpv.Avalonia`
- namespace: `HanumanInstitute.LibMpv.Avalonia` — `MpvView` and render controls
- target: `lib/net10.0`
- asset: Avalonia control library
- rail: media

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client context and command intake.

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [CAPABILITY]                 |
| :-----: | :------------------ | :-------------- | :--------------------------- |
|  [01]   | `MpvContext`        | playback facade | typed property/command owner |
|  [02]   | `MpvContextBase`    | client core     | raw libmpv client surface    |
|  [03]   | `MpvCommand`        | command request | deferred command invocation  |
|  [04]   | `MpvCommandOptions` | command policy  | prefixes and async behavior  |
|  [05]   | `MpvAsyncOptions`   | async policy    | timeout and error behavior   |
|  [06]   | `MpvException`      | failure rail    | libmpv error projection      |
|  [07]   | `MpvEventLoop`      | event-loop enum | loop strategy selection      |

[PUBLIC_TYPE_SCOPE]: typed property and option wrappers.
- `MpvOption<T> : MpvPropertyWrite<T,T> where T : struct` inherits `Get` / `Set` / `GetAsync` / `SetAsync` from the property base — option and property are one wrapper hierarchy.

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]   | [CAPABILITY]                        |
| :-----: | :---------------------------------------------- | :-------------- | :---------------------------------- |
|  [01]   | `MpvPropertyRead<T>`                            | read property   | value-typed property read           |
|  [02]   | `MpvPropertyWrite<T,TApi>`                      | read/write      | value-typed property write          |
|  [03]   | `MpvPropertyReadRef<T,TApi>`                    | read property   | reference-typed read                |
|  [04]   | `MpvPropertyWriteRef<T,TApi>`                   | read/write      | reference-typed write               |
|  [05]   | `MpvPropertyReadString`                         | read property   | string property read                |
|  [06]   | `MpvPropertyWriteString`                        | read/write      | string property write               |
|  [07]   | `MpvPropertyIndexRead<TI,T>`                    | indexed read    | track/list indexed read             |
|  [08]   | `MpvPropertyIndexWrite<TI,T>`                   | indexed write   | track/list indexed write            |
|  [09]   | `MpvPropertyIndexReadRef` / `…WriteRef`         | indexed ref     | reference-typed indexed             |
|  [10]   | `MpvOption<T>`                                  | startup option  | value option get/set (`T : struct`) |
|  [11]   | `MpvOptionEnum<T>`                              | startup option  | enum option get/set                 |
|  [12]   | `MpvOptionString`                               | startup option  | string option get/set               |
|  [13]   | `MpvOptionRef<T,TApi>`                          | startup option  | reference-typed option              |
|  [14]   | `MpvOptionList`                                 | list option     | additive list option                |
|  [15]   | `MpvOptionDictionary` / `…RefDictionary`        | dictionary      | key/value option map                |
|  [16]   | `MpvOptionWithAuto` / `…AutoNo`                 | sentinel option | `auto` / `no` special-value wrapper |
|  [17]   | `MpvOptionWithNo` / `…NoAlways` / `…YesNo`      | sentinel option | `no` / `yes`/`no` sentinel          |
|  [18]   | `MpvOptionWithDefault` / `…Inf` / `…AllCurrent` | sentinel option | `default` / `inf` / `all` sentinel  |
|  [19]   | `MpvOptionWithIndex` / `…Full`                  | sentinel option | indexed / full-range sentinel       |

[PUBLIC_TYPE_SCOPE]: event payloads.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]   | [CAPABILITY]                  |
| :-----: | :------------------------- | :-------------- | :---------------------------- |
|  [01]   | `MpvPropertyEventArgs`     | property event  | observed property change      |
|  [02]   | `MpvLogMessageEventArgs`   | log event       | libmpv log line and level     |
|  [03]   | `MpvStartFileEventArgs`    | lifecycle event | playback start                |
|  [04]   | `MpvEndFileEventArgs`      | lifecycle event | playback end and reason       |
|  [05]   | `MpvCommandReplyEventArgs` | command event   | async command completion      |
|  [06]   | `EndReason`                | reason enum     | end-of-file classifier        |
|  [07]   | `MpvEventLoop`             | event-loop enum | `Default` (simple) / `Thread` |

[PUBLIC_TYPE_SCOPE]: Avalonia view and render integration.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]   | [CAPABILITY]                 |
| :-----: | :-------------- | :-------------- | :--------------------------- |
|  [01]   | `MpvView`       | host control    | renderer-switching `Control` |
|  [02]   | `IVideoView`    | view contract   | `MpvContext`-bearing surface |
|  [03]   | `VideoRenderer` | renderer enum   | render-path selection        |
|  [04]   | `OpenGlView`    | render control  | `OpenGlControlBase` GL path  |
|  [05]   | `SoftwareView`  | render control  | CPU-blit `Control` path      |
|  [06]   | `NativeView`    | render control  | `NativeControlHost` path     |
|  [07]   | `MpvOverlay`    | overlay surface | drawn `bgra` image overlay   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: playback commands on `MpvContext` — `MpvCommand`-projected instance methods.

| [INDEX] | [SURFACE]                            | [CAPABILITY]             |
| :-----: | :----------------------------------- | :----------------------- |
|  [01]   | `LoadFile(path, append, appendPlay)` | open media into playlist |
|  [02]   | `LoadPlaylist(path, append)`         | open a playlist file     |
|  [03]   | `Seek(units, SeekOption)`            | relative/absolute seek   |
|  [04]   | `RevertSeek(mark)`                   | undo the last seek       |
|  [05]   | `FrameStep` / `FrameBackStep`        | single-frame advance     |
|  [06]   | `Stop`                               | halt playback and unload |
|  [07]   | `PlaylistNext` / `PlaylistPrev`      | move playlist position   |
|  [08]   | `SubAdd` / `SubRemove` / `SubReload` | external subtitle tracks |
|  [09]   | `AudioAdd` / `AudioRemove`           | external audio tracks    |
|  [10]   | `VideoAdd` / `VideoRemove`           | external video tracks    |
|  [11]   | `Screenshot(ScreenshotOptions)`      | frame capture            |
|  [12]   | `Add` / `Cycle` / `Multiply`         | relative property change |
|  [13]   | `Quit(exitCode)`                     | terminate the player     |

[ENTRYPOINT_SCOPE]: typed properties on `MpvContext`; the backing wrapper carries the read/write capability.

| [INDEX] | [SURFACE]                                                   | [CAPABILITY]                  |
| :-----: | :---------------------------------------------------------- | :---------------------------- |
|  [01]   | `Pause -> MpvOption<bool>`                                  | play/pause state              |
|  [02]   | `Speed -> MpvOption<double>`                                | playback rate                 |
|  [03]   | `Volume -> MpvOption<double>`                               | audio level                   |
|  [04]   | `Mute -> MpvOption<bool>`                                   | audio mute                    |
|  [05]   | `AudioDelay -> MpvOption<double>`                           | audio sync offset             |
|  [06]   | `TimePos -> MpvPropertyWrite<double>`                       | absolute time position        |
|  [07]   | `PercentPos -> MpvPropertyWrite<double>`                    | percentage position           |
|  [08]   | `PlaybackTime -> MpvPropertyWrite<double>`                  | playback clock                |
|  [09]   | `Duration -> MpvPropertyRead<double>`                       | media length                  |
|  [10]   | `TimeRemaining -> MpvPropertyRead<double>`                  | remaining time                |
|  [11]   | `EofReached -> MpvPropertyRead<bool>`                       | end-of-file flag              |
|  [12]   | `Seeking -> MpvPropertyRead<bool>`                          | seek-in-progress flag         |
|  [13]   | `AudioId` / `SubId` / `VideoId -> MpvOptionWithAutoNo<int>` | active track selection        |
|  [14]   | `LoopFile` / `LoopPlaylist -> MpvOptionString`              | file and playlist loop policy |
|  [15]   | `AbLoopA` / `AbLoopB` / `AbLoopCount -> MpvOptionString`    | A-B section loop policy       |

- `LoopFile` / `LoopPlaylist` admit `"inf"`, `"no"`, or a count string; `AbLoopA` / `AbLoopB` / `AbLoopCount` carry the section bounds and repetition count.

[ENTRYPOINT_SCOPE]: the raw libmpv client on `MpvContextBase` — command, property, observation, async-control, render, and diagnostics primitives; the wrapper `GetAsync` / `SetAsync` marshal through these.

| [INDEX] | [SURFACE]                                                                          | [CAPABILITY]               |
| :-----: | :--------------------------------------------------------------------------------- | :------------------------- |
|  [01]   | `MpvContextBase(MpvEventLoop)` / `Initialize()`                                    | create client, pick loop   |
|  [02]   | `RunCommand(MpvCommandOptions?, params object?[])`                                 | sync command, throw policy |
|  [03]   | `RunCommandAsync(ulong requestId, string[] args)`                                  | async command, reply event |
|  [04]   | `RunCommandNode(MpvNode, bool returnData)` / `…NodeAsync`                          | node-arg command with data |
|  [05]   | `RunCommandString(string args)`                                                    | flat-string command        |
|  [06]   | `GetProperty(name, MpvFormat, void*)`                                              | typed property read        |
|  [07]   | `GetPropertyString` / `GetPropertyOsdString`                                       | string and OSD read        |
|  [08]   | `SetProperty(name, MpvFormat, void*)`                                              | typed property write       |
|  [09]   | `SetPropertyDouble` / `…Long` / `…Flag` / `…String`                                | specialized property write |
|  [10]   | `ObserveProperty(ulong requestId, name, MpvFormat)`                                | property-change events     |
|  [11]   | `AbortAsyncCommand(ulong requestId)` / `WaitAsyncRequests()`                       | cancel / drain pending     |
|  [12]   | `StartOpenGlRendering(getProcAddress, ..)` / `OpenGlRender(w, h, fb, flipY)`       | bind + draw GL frame       |
|  [13]   | `StartSoftwareRendering(updateCallback)` / `SoftwareRender(w, h, surface, format)` | bind + blit CPU frame      |
|  [14]   | `StartNativeRendering(hw)` / `StopRendering()`                                     | embed native / release     |
|  [15]   | `RequestLogMessages(minLevel)`                                                     | enable log events          |

[ENTRYPOINT_SCOPE]: typed property and option wrapper operations.

| [INDEX] | [SURFACE]                            | [CAPABILITY]             |
| :-----: | :----------------------------------- | :----------------------- |
|  [01]   | `MpvPropertyRead.Get / GetAsync`     | read property value      |
|  [02]   | `MpvPropertyWrite.Set / SetAsync`    | write property value     |
|  [03]   | `MpvOption.Get / Set`                | startup option access    |
|  [04]   | `MpvOptionList.Add / AddAsync`       | append list entry        |
|  [05]   | `MpvOptionList.Remove / RemoveAsync` | drop list entry          |
|  [06]   | `MpvOptionList.Toggle / ToggleAsync` | flip list membership     |
|  [07]   | `MpvOptionList.Clear / ClearAsync`   | empty the list option    |
|  [08]   | `MpvCommand.Invoke / InvokeAsync`    | dispatch a built command |

[ENTRYPOINT_SCOPE]: Avalonia view and overlay surface.

| [INDEX] | [SURFACE]                            | [CAPABILITY]               |
| :-----: | :----------------------------------- | :------------------------- |
|  [01]   | `MpvView.MpvContext`                 | bound playback facade      |
|  [02]   | `MpvView.Renderer`                   | `VideoRenderer` selection  |
|  [03]   | `MpvView.InitRenderer`               | rebuild the render child   |
|  [04]   | `MpvView.MpvContextProperty`         | `DirectProperty` binding   |
|  [05]   | `MpvView.RendererProperty`           | `DirectProperty` binding   |
|  [06]   | `OpenGlView.MpvContext`              | GL-path context            |
|  [07]   | `MpvOverlay.Show(x, y, w, h, color)` | solid-color overlay        |
|  [08]   | `MpvOverlay.Show(x, y, w, h, draw)`  | drawn-content overlay      |
|  [09]   | `MpvOverlay.Hide`                    | clear the overlay          |
|  [10]   | `IVideoView.Dispose`                 | release the render context |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MpvContextBase` owns the raw client surface; `MpvContext` derives and projects the command set into named `MpvCommand` methods and the property/option set into typed wrappers keyed to mpv property names, so the wrapper `GetAsync` / `SetAsync` is the surface a UI binding awaits, never a base method.
- `MpvEventLoop` selects the loop strategy at construction: `Default` drives an `MpvSimpleEventLoop` (caller-pumped), `Thread` an `MpvThreadEventLoop` (dedicated event thread).
- Render paths are mutually exclusive — `StartOpenGlRendering`, `StartSoftwareRendering`, and `StartNativeRendering` each bind one strategy until `StopRendering` releases it; `MpvView` selects the path through `RendererProperty` and `InitRenderer` swaps in the matching `IVideoView` child (`IVideoView : IDisposable` exposing the bound `MpvContext`).
- `OpenGlView : OpenGlControlBase` renders inside the Avalonia GL surface, avoiding the `NativeControlHost` airspace `NativeView` requires.
- Events surface on `MpvContextBase` as payload events (`PropertyChanged`, `LogMessage`, `StartFile`, `EndFile`) and bare-signal events (`FileLoaded`, `PlaybackRestart`, `SeekRaised`, `Shutdown`, `Idle`, `Tick`, `PreRender`, `QueueOverflow`, `AudioReconfig`, `VideoReconfig`); a consumer subscribes the observed-property and lifecycle events it needs and never polls on a timer.
- Failures raise `MpvException` carrying the libmpv `MpvError` code; `MpvCommandOptions` and `MpvAsyncOptions` set throw-on-error, response timeout, and wait-for-response, and `MpvFormat` / `MpvNode` are the marshalling primitives `GetProperty` / `ObserveProperty` / `RunCommandNode` pass.

[STACKING]:
- `api-ffmpeg-autogen.md`(`.api/api-ffmpeg-autogen.md`): the seamed decode-in versus encode-out pair — `libmpv` owns media decode and on-screen OpenGL playback (Editing MediaSurface), `FFmpeg.AutoGen` owns the RGBA→MP4 encode-out (Render capture).
- `api-avalonia-gpu-interop.md`(`.api/api-avalonia-gpu-interop.md`): `OpenGlView : OpenGlControlBase` shares the compositor GL surface, so playback composites in-tree rather than in a `NativeControlHost` airspace window.
- `api-silk-webgpu-wgpu.md`(`.api/api-silk-webgpu-wgpu.md`): `LogMessage` and `EndFile` route into the AppUi receipt sink as counted media-fault rows, the decode-side peer of wgpu's `PfnLogCallback` `ViewportFault` stream.
- `api-reactiveui.md`(`.api/api-reactiveui.md`): observed `MpvPropertyRead` members marshalled onto `Dispatcher.UIThread` drive ReactiveUI transport bindings with no cross-thread hop.
- MediaSurface owner: `MpvView` with `Renderer` set to `VideoRenderer.OpenGl`; playback flows through the bound `IVideoView`'s `MpvContext` — `LoadFile` intake, `Pause` / `Speed` / `Volume` / `Mute` transport, `TimePos` / `PercentPos` seek, observed `MpvPropertyRead` and `PropertyChanged` state, `IVideoView.Dispose` teardown.

[LOCAL_ADMISSION]:
- Native libmpv provisions at the app-host distribution layer and binds at load; these assemblies ship no native binary.
- `MpvEventLoop.Default` (the `MpvSimpleEventLoop`) is the Avalonia path: events marshal onto the `Dispatcher.UIThread` the `MpvView` lives on, so `PropertyChanged` updates bindings without a cross-thread hop; `MpvEventLoop.Thread` serves a headless host where no dispatcher pumps.
- `LogMessage` (gated by `RequestLogMessages(minLevel)`) and `EndFile` (`MpvEndFileEventArgs.Reason` : `EndReason`) route into the AppUi receipt sink, so a decode failure or unsupported codec is a counted media-fault row, not a swallowed native print.
- Every `MpvContext`, view, and overlay releases through `IVideoView.Dispose` at teardown to free the render context.

[RAIL_LAW]:
- Package: `HanumanInstitute.LibMpv`, `HanumanInstitute.LibMpv.Avalonia`
- Owns: managed libmpv playback, the typed property/command/option surface, and the Avalonia OpenGL render integration.
- Accept: playback driven through `MpvContext` and hosted by `MpvView` on the OpenGL render path.
- Reject: a bundled libmpv native binary; a hand-rolled mpv command/property marshaller; `NativeControlHost` airspace where the OpenGL path serves; timer-polling for position where an observed `PropertyChanged` carries it.

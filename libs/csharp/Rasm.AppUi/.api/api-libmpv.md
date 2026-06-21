# [RASM_APPUI_API_LIBMPV]

`HanumanInstitute.LibMpv` supplies a managed libmpv client: an `MpvContext` facade over the mpv command, property, and option API plus an embedded OpenGL/software render path, and `HanumanInstitute.LibMpv.Avalonia` supplies the `MpvView` control that drives that render path inside an Avalonia visual tree. Together they serve the AppUi Editing MediaSurface rail for video and audio playback over the OpenGL libmpv render integration. The libmpv native runtime is provisioned by the app-host distribution layer, never bundled with these assemblies.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `HanumanInstitute.LibMpv`
- package: `HanumanInstitute.LibMpv`
- package: `HanumanInstitute.LibMpv.Avalonia`
- assembly: `HanumanInstitute.LibMpv`
- assembly: `HanumanInstitute.LibMpv.Avalonia`
- namespace: `HanumanInstitute.LibMpv`
- namespace: `HanumanInstitute.LibMpv.Core`
- namespace: `HanumanInstitute.LibMpv.Avalonia`
- framework: `net10.0`
- framework: `netstandard2.0`
- asset: managed client over the libmpv native runtime
- asset: Avalonia control library
- rail: media

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client context and command intake — rail: media

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]   | [RAIL]                       |
| :-----: | :------------------ | :-------------- | :--------------------------- |
|  [01]   | `MpvContext`        | playback facade | typed property/command owner |
|  [02]   | `MpvContextBase`    | client core     | raw libmpv client surface    |
|  [03]   | `MpvCommand`        | command request | deferred command invocation  |
|  [04]   | `MpvCommandOptions` | command policy  | prefixes and async behavior  |
|  [05]   | `MpvAsyncOptions`   | async policy    | timeout and error behavior   |
|  [06]   | `MpvException`      | failure rail    | libmpv error projection      |
|  [07]   | `MpvEventLoop`      | event-loop enum | loop strategy selection      |

[PUBLIC_TYPE_SCOPE]: typed property and option wrappers — rail: media

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [RAIL]                     |
| :-----: | :---------------------------- | :------------- | :------------------------- |
|  [01]   | `MpvPropertyRead<T>`          | read property  | value-typed property read  |
|  [02]   | `MpvPropertyWrite<T>`         | read/write     | value-typed property write |
|  [03]   | `MpvPropertyReadRef<T,TApi>`  | read property  | reference-typed read       |
|  [04]   | `MpvPropertyWriteRef<T,TApi>` | read/write     | reference-typed write      |
|  [05]   | `MpvPropertyReadString`       | read property  | string property read       |
|  [06]   | `MpvPropertyIndexRead<TI,T>`  | indexed read   | track/list indexed read    |
|  [07]   | `MpvPropertyIndexWrite<TI,T>` | indexed write  | track/list indexed write   |
|  [08]   | `MpvOption<T>`                | startup option | value option get/set       |
|  [09]   | `MpvOptionEnum<T>`            | startup option | enum option get/set        |
|  [10]   | `MpvOptionString`             | startup option | string option get/set      |
|  [11]   | `MpvOptionList`               | list option    | additive list option       |
|  [12]   | `MpvOptionDictionary`         | dictionary     | key/value option map       |

[PUBLIC_TYPE_SCOPE]: event payloads — rail: media

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]   | [RAIL]                   |
| :-----: | :----------------------- | :-------------- | :----------------------- |
|  [01]   | `MpvPropertyEventArgs`   | property event  | observed property change |
|  [02]   | `MpvLogMessageEventArgs` | log event       | libmpv log line          |
|  [03]   | `MpvStartFileEventArgs`  | lifecycle event | playback start           |
|  [04]   | `MpvEndFileEventArgs`    | lifecycle event | playback end and reason  |
|  [05]   | `EndReason`              | reason enum     | end-of-file classifier   |

[PUBLIC_TYPE_SCOPE]: Avalonia view and render integration — rail: media

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]   | [RAIL]                       |
| :-----: | :-------------- | :-------------- | :--------------------------- |
|  [01]   | `MpvView`       | host control    | renderer-switching `Control` |
|  [02]   | `IVideoView`    | view contract   | `MpvContext`-bearing surface |
|  [03]   | `VideoRenderer` | renderer enum   | render-path selection        |
|  [04]   | `OpenGlView`    | render control  | `OpenGlControlBase` GL path  |
|  [05]   | `SoftwareView`  | render control  | CPU-blit `Control` path      |
|  [06]   | `NativeView`    | render control  | `NativeControlHost` path     |
|  [07]   | `MpvOverlay`    | overlay surface | drawn `bgra` image overlay   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: playback commands on `MpvContext`
- rail: media

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY]  | [RAIL]                    |
| :-----: | :----------------------------------- | :-------------- | :------------------------ |
|  [01]   | `LoadFile(path, append, appendPlay)` | source intake   | open media into playlist  |
|  [02]   | `LoadPlaylist(path, append)`         | source intake   | open a playlist file      |
|  [03]   | `Seek(units, SeekOption)`            | transport       | relative or absolute seek |
|  [04]   | `RevertSeek(mark)`                   | transport       | undo the last seek        |
|  [05]   | `FrameStep` / `FrameBackStep`        | transport       | single-frame advance      |
|  [06]   | `Stop`                               | transport       | halt playback and unload  |
|  [07]   | `PlaylistNext` / `PlaylistPrev`      | playlist        | move playlist position    |
|  [08]   | `SubAdd` / `SubRemove` / `SubReload` | track           | external subtitle tracks  |
|  [09]   | `AudioAdd` / `AudioRemove`           | track           | external audio tracks     |
|  [10]   | `VideoAdd` / `VideoRemove`           | track           | external video tracks     |
|  [11]   | `Screenshot(ScreenshotOptions)`      | capture         | frame capture             |
|  [12]   | `Add` / `Cycle` / `Multiply`         | property mutate | relative property change  |
|  [13]   | `Quit(exitCode)`                     | shutdown        | terminate the player      |

[ENTRYPOINT_SCOPE]: transport and audio properties on `MpvContext`
- rail: media

| [INDEX] | [SURFACE]                       | [SURFACE_ROOT]             | [RAIL]                 |
| :-----: | :------------------------------ | :------------------------- | :--------------------- |
|  [01]   | `Pause`                         | `MpvOption<bool>`          | play/pause state       |
|  [02]   | `Speed`                         | `MpvOption<double>`        | playback rate          |
|  [03]   | `Volume`                        | `MpvOption<double>`        | audio level            |
|  [04]   | `Mute`                          | `MpvOption<bool>`          | audio mute             |
|  [05]   | `AudioDelay`                    | `MpvOption<double>`        | audio sync offset      |
|  [06]   | `TimePos`                       | `MpvPropertyWrite<double>` | absolute time position |
|  [07]   | `PercentPos`                    | `MpvPropertyWrite<double>` | percentage position    |
|  [08]   | `PlaybackTime`                  | `MpvPropertyWrite<double>` | playback clock         |
|  [09]   | `Duration`                      | `MpvPropertyRead<double>`  | media length           |
|  [10]   | `TimeRemaining`                 | `MpvPropertyRead<double>`  | remaining time         |
|  [11]   | `EofReached`                    | `MpvPropertyRead<bool>`    | end-of-file flag       |
|  [12]   | `Seeking`                       | `MpvPropertyRead<bool>`    | seek-in-progress flag  |
|  [13]   | `AudioId` / `SubId` / `VideoId` | `MpvOptionWithAutoNo<int>` | active track selection |

[ENTRYPOINT_SCOPE]: client core and render path on `MpvContextBase`
- rail: media

| [INDEX] | [SURFACE]                                  | [ENTRY_FAMILY]  | [RAIL]                      |
| :-----: | :----------------------------------------- | :-------------- | :-------------------------- |
|  [01]   | `Initialize`                               | lifecycle       | create the libmpv client    |
|  [02]   | `CommandAsync<T>(options, args)`           | command rail    | async typed command         |
|  [03]   | `GetPropertyAsync<T>(name, options)`       | property rail   | async typed property read   |
|  [04]   | `SetPropertyAsync<T>(name, value, opts)`   | property rail   | async typed property write  |
|  [05]   | `ObserveProperty(requestId, name, format)` | observation     | property-change events      |
|  [06]   | `StartOpenGlRendering(getProcAddress, ..)` | render setup    | bind the OpenGL render path |
|  [07]   | `OpenGlRender(width, height, fb, flipY)`   | render frame    | draw one GL frame           |
|  [08]   | `StartSoftwareRendering(updateCallback)`   | render setup    | bind the software path      |
|  [09]   | `SoftwareRender(w, h, surface, format)`    | render frame    | blit one CPU frame          |
|  [10]   | `StartNativeRendering(hw)`                 | render setup    | embed a native window       |
|  [11]   | `StopRendering`                            | render teardown | release the render context  |
|  [12]   | `RequestLogMessages(minLevel)`             | diagnostics     | enable log events           |

[ENTRYPOINT_SCOPE]: typed property and option wrapper operations
- rail: media

| [INDEX] | [SURFACE]                | [SURFACE_ROOT]     | [RAIL]                   |
| :-----: | :----------------------- | :----------------- | :----------------------- |
|  [01]   | `Get` / `GetAsync`       | `MpvPropertyRead`  | read property value      |
|  [02]   | `Set` / `SetAsync`       | `MpvPropertyWrite` | write property value     |
|  [03]   | `Get` / `Set`            | `MpvOption`        | startup option access    |
|  [04]   | `Add` / `AddAsync`       | `MpvOptionList`    | append list entry        |
|  [05]   | `Remove` / `RemoveAsync` | `MpvOptionList`    | drop list entry          |
|  [06]   | `Toggle` / `ToggleAsync` | `MpvOptionList`    | flip list membership     |
|  [07]   | `Clear` / `ClearAsync`   | `MpvOptionList`    | empty the list option    |
|  [08]   | `Invoke` / `InvokeAsync` | `MpvCommand`       | dispatch a built command |

[ENTRYPOINT_SCOPE]: Avalonia view and overlay surface
- rail: media

| [INDEX] | [SURFACE]                 | [SURFACE_ROOT] | [RAIL]                     |
| :-----: | :------------------------ | :------------- | :------------------------- |
|  [01]   | `MpvContext`              | `MpvView`      | bound playback facade      |
|  [02]   | `Renderer`                | `MpvView`      | `VideoRenderer` selection  |
|  [03]   | `InitRenderer`            | `MpvView`      | rebuild the render child   |
|  [04]   | `MpvContextProperty`      | `MpvView`      | `DirectProperty` binding   |
|  [05]   | `RendererProperty`        | `MpvView`      | `DirectProperty` binding   |
|  [06]   | `MpvContext`              | `OpenGlView`   | GL-path context            |
|  [07]   | `Show(x, y, w, h, color)` | `MpvOverlay`   | solid-color overlay        |
|  [08]   | `Show(x, y, w, h, draw)`  | `MpvOverlay`   | drawn-content overlay      |
|  [09]   | `Hide`                    | `MpvOverlay`   | clear the overlay          |
|  [10]   | `Dispose`                 | `IVideoView`   | release the render context |

## [04]-[IMPLEMENTATION_LAW]

[MEDIA_TOPOLOGY]:
- `HanumanInstitute.LibMpv` carries 241 types across `HanumanInstitute.LibMpv` and `HanumanInstitute.LibMpv.Core`; `HanumanInstitute.LibMpv.Avalonia` carries 11 types in one namespace.
- `MpvContextBase` owns the raw libmpv client: `Initialize`, the async `CommandAsync`/`GetPropertyAsync`/`SetPropertyAsync` rails, property observation, event surfacing, and the render entrypoints.
- `MpvContext` derives from `MpvContextBase` and projects the libmpv command set into named `MpvCommand` methods and the property/option set into hundreds of typed `MpvOption`, `MpvOptionEnum`, `MpvPropertyRead`, and `MpvPropertyWrite` members keyed to mpv property names such as `time-pos`, `volume`, and `pause`.
- Render paths are mutually exclusive: `StartOpenGlRendering`/`OpenGlRender`, `StartSoftwareRendering`/`SoftwareRender`, and `StartNativeRendering` each bind one strategy until `StopRendering` releases it.
- `VideoRenderer` admits `Auto`, `Software`, `OpenGl`, and `Native`; `MpvView` selects one and swaps in the matching `IVideoView` child (`OpenGlView`, `SoftwareView`, or `NativeView`).
- `OpenGlView` derives from `OpenGlControlBase` and renders inside the Avalonia GL surface, avoiding the `NativeControlHost` airspace that `NativeView` requires.
- Events surface through `MpvContextBase`: `PropertyChanged`, `LogMessage`, `StartFile`, `EndFile`, `FileLoaded`, `PlaybackRestart`, `SeekRaised`, and `Shutdown`.
- Failures raise `MpvException` carrying the libmpv `MpvError` code; `MpvCommandOptions` and `MpvAsyncOptions` set throw-on-error, response timeout, and wait-for-response behavior.

[LOCAL_ADMISSION]:
- The MediaSurface composes `MpvView` with `Renderer` set to `VideoRenderer.OpenGl`, taking the OpenGL render path so playback shares the Avalonia GL surface instead of a separate native window.
- Playback control flows through the `MpvContext` exposed by the bound `IVideoView`: `LoadFile` for source intake, `Pause`/`Speed`/`Volume`/`Mute` options for transport and audio, and `TimePos`/`PercentPos` for seek.
- Position and state surface through observed `MpvPropertyRead` members and `PropertyChanged`; the surface never polls libmpv on a timer.
- Every `MpvContext`, view, and overlay is disposed through `IVideoView.Dispose` at teardown to free the render context.

[RAIL_LAW]:
- Package: `HanumanInstitute.LibMpv`, `HanumanInstitute.LibMpv.Avalonia`
- Owns: managed libmpv playback, the typed property/command surface, and the Avalonia OpenGL render integration
- Accept: media playback driven through `MpvContext` and hosted by `MpvView` on the OpenGL render path
- Reject: a bundled libmpv native binary, a hand-rolled mpv command/property marshaller, or `NativeControlHost` airspace embedding when the OpenGL path serves the surface

> [!IMPORTANT]
> The libmpv native runtime (`libmpv` >= 0.40.0) is provisioned at the app-host distribution layer through `brew`, `apt`, or a side-loaded binary. These assemblies bind it at load time and never ship it.

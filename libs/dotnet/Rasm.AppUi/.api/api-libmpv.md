# [RASM_APPUI_API_LIBMPV]

`HanumanInstitute.LibMpv` owns the managed libmpv client: `MpvContext` projects the mpv command, property, and option API as typed members over an embedded OpenGL, software, and native render path, and `HanumanInstitute.LibMpv.Avalonia` binds that path into an Avalonia visual tree through the `MpvView` control. Together they own the AppUi Editing MediaSurface decode-and-playback rail — the on-screen counterpart to the `FFmpeg.AutoGen` encode-out owner.

## [01]-[PUBLIC_TYPES]

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
- `MpvProperty<TNull,TRaw>` is the shared base: PUBLIC `PropertyName`, protected `Format` derived once through `MpvFormatter.GetMpvFormat<TRaw>()`, protected `Mpv`, and a `ParseValue` that coerces raw-to-typed and yields null on an empty string.
- Every read wrapper is NULLABLE by construction — `MpvPropertyRead<T,TRaw> : MpvProperty<T?,TRaw> where T : struct` and `MpvPropertyReadRef<T,TRaw> : MpvProperty<T?,TRaw> where T : class` — so `Get()`/`GetAsync()` answer `T?` and a property the core does not yet hold reads as null rather than as a default.
- `MpvOptionWith<T> : MpvOption<T,string>` is the sentinel base; `MpvOptionWithAuto<T>` adds `SetAuto`/`SetAutoAsync`/`GetAuto`/`GetAutoAsync` and `MpvOptionWithAutoNo<T>` adds `SetNo`/`SetNoAsync`/`GetNo`/`GetNoAsync` over it, so `auto` and `no` are member calls rather than magic strings.
- `MpvOptionString : MpvOptionRef<string,string> : MpvPropertyWriteRef<string,string>`, so a string option carries the same `Set`/`SetAsync`/`Add`/`Multiply`/`Cycle` surface a value option does.

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

| [INDEX] | [SYMBOL]                              | [TYPE_FAMILY]   | [CAPABILITY]                      |
| :-----: | :------------------------------------ | :-------------- | :-------------------------------- |
|  [01]   | `MpvPropertyEventArgs`                | property event  | observed property change          |
|  [02]   | `MpvLogMessageEventArgs`              | log event       | libmpv log line and level         |
|  [03]   | `MpvStartFileEventArgs`               | lifecycle event | playback start                    |
|  [04]   | `MpvEndFileEventArgs`                 | lifecycle event | playback end and reason           |
|  [05]   | `MpvCommandReplyEventArgs`            | command event   | async command completion          |
|  [06]   | `EndReason`                           | reason enum     | end-of-file classifier            |
|  [07]   | `MpvEventLoop`                        | event-loop enum | `Default` (simple) / `Thread`     |
|  [08]   | `MpvValueChangedEventArgs<T,TRaw>`    | typed change    | per-wrapper value change (struct) |
|  [09]   | `MpvValueChangedEventArgsRef<T,TRaw>` | typed change    | per-wrapper value change (class)  |

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

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: playback commands on `MpvContext` — `MpvCommand`-projected instance methods.

| [INDEX] | [SURFACE]                             | [CAPABILITY]             |
| :-----: | :------------------------------------ | :----------------------- |
|  [01]   | `LoadFile(path, append, appendPlay)`  | open media into playlist |
|  [02]   | `LoadPlaylist(path, append)`          | open a playlist file     |
|  [03]   | `Seek(units, SeekOption)`             | relative/absolute seek   |
|  [04]   | `RevertSeek(bool mark)`               | mark, or revert to mark  |
|  [05]   | `FrameStep` / `FrameBackStep`         | single-frame advance     |
|  [06]   | `Stop`                                | halt playback and unload |
|  [07]   | `PlaylistNext` / `PlaylistPrev`       | move playlist position   |
|  [08]   | `SubAdd` / `SubRemove` / `SubReload`  | external subtitle tracks |
|  [09]   | `AudioAdd` / `AudioRemove`            | external audio tracks    |
|  [10]   | `VideoAdd` / `VideoRemove`            | external video tracks    |
|  [11]   | `Screenshot(ScreenshotOptions)`       | frame capture to config  |
|  [12]   | `ScreenshotToFile(path, options)`     | frame capture to a path  |
|  [13]   | `PlaylistClear` / `…Remove` / `…Move` | playlist mutation        |
|  [14]   | `PlaylistShuffle` / `…Unshuffle`      | playlist ordering        |
|  [15]   | `Add` / `Cycle` / `Multiply`          | relative property change |
|  [16]   | `Quit(exitCode)`                      | terminate the player     |

- `SubAdd(string path, LoadOption option = Select, string? title = null, string? lang = null)` and `AudioAdd` share that arity; `LoadOption` = `Select` · `Auto` · `Cached`.
- `RevertSeek(true)` MARKS the current position and `RevertSeek(false)` returns to the last mark, so a scrub-and-cancel is two calls on the player's own memory rather than a caller-held snapshot.
- `ScreenshotOptions` is `[Flags]`: `None` · `Subtitles` · `Video` · `Window` · `EachFrame`; `SeekOption` is `[Flags]`: `None` · `Relative` · `Absolute` · `AbsolutePercent` · `RelativePercent` · `Keyframes` · `Exact`.

[ENTRYPOINT_SCOPE]: typed properties on `MpvContext`; the backing wrapper carries the read/write capability.

| [INDEX] | [SURFACE]                                                    | [CAPABILITY]                  |
| :-----: | :----------------------------------------------------------- | :---------------------------- |
|  [01]   | `Pause -> MpvOption<bool>`                                   | play/pause state              |
|  [02]   | `Speed -> MpvOption<double>`                                 | playback rate                 |
|  [03]   | `Volume -> MpvOption<double>`                                | audio level                   |
|  [04]   | `Mute -> MpvOption<bool>`                                    | audio mute                    |
|  [05]   | `AudioDelay -> MpvOption<double>`                            | audio sync offset             |
|  [06]   | `TimePos -> MpvPropertyWrite<double>`                        | absolute time position        |
|  [07]   | `PercentPos -> MpvPropertyWrite<double>`                     | percentage position           |
|  [08]   | `PlaybackTime -> MpvPropertyWrite<double>`                   | playback clock                |
|  [09]   | `Duration -> MpvPropertyRead<double>`                        | media length                  |
|  [10]   | `TimeRemaining -> MpvPropertyRead<double>`                   | remaining time                |
|  [11]   | `EofReached -> MpvPropertyRead<bool>`                        | end-of-file flag              |
|  [12]   | `Seeking -> MpvPropertyRead<bool>`                           | seek-in-progress flag         |
|  [13]   | `AudioId` / `SubId` / `VideoId -> MpvOptionWithAutoNo<int>`  | active track selection        |
|  [14]   | `LoopFile` / `LoopPlaylist -> MpvOptionString`               | file and playlist loop policy |
|  [15]   | `AbLoopA` / `AbLoopB` / `AbLoopCount -> MpvOptionString`     | A-B section loop policy       |
|  [16]   | `Start -> MpvOptionString`                                   | pre-load entry position       |
|  [17]   | `DemuxerCacheTime` / `…Duration -> MpvPropertyRead<double>`  | buffered extent and span      |
|  [18]   | `DemuxerCacheState -> MpvPropertyReadRef<DemuxerCacheState>` | structured cache state        |
|  [19]   | `SubText -> MpvPropertyReadString`                           | active subtitle cue text      |
|  [20]   | `SubStart` / `SubEnd -> MpvPropertyRead<float>`              | active cue bounds             |
|  [21]   | `SubDelay` / `SubVisibility` / `SubPos` / `SubScale`         | subtitle presentation options |
|  [22]   | `PlaylistPosition -> MpvPropertyWrite<int>`                  | current playlist index        |
|  [23]   | `PlaylistCount -> MpvPropertyRead<int>`                      | playlist length               |
|  [24]   | `TrackListCount -> MpvPropertyRead<int>`                     | enumerable track count        |
|  [25]   | `Duration` / `TimeRemaining` / `PlaytimeRemaining`           | length and remaining reads    |
|  [26]   | `MediaTitle` / `FileName` / `Path -> MpvPropertyReadString`  | source identity reads         |

- `LoopFile` / `LoopPlaylist` admit `"inf"`, `"no"`, or a count string; `AbLoopA` / `AbLoopB` / `AbLoopCount` carry the section bounds and repetition count.
- `Start` is an OPTION (`start`), not a property: it sets the entry position BEFORE a load, where `TimePos` does not exist because it is a property of a loaded file — a pre-load `TimePos` write is silently inert.
- `DemuxerCacheTime` is the absolute playback time the demuxer cache reaches, which is the buffered extent a scrub track shades; `DemuxerCacheDuration` is the span it holds ahead of the playhead.
- `SubText` / `SubStart` / `SubEnd` are the ACTIVE cue and its bounds as mpv itself decoded and timed them, so a caption band renders the player's own segment with no second cue parse.

[ENTRYPOINT_SCOPE]: indexed list properties — `MpvPropertyIndexRead<TIndex,T,TRaw>` exposes `this[TIndex] -> MpvPropertyRead<T,TRaw>` by formatting its `{0}` template, so each element is an ordinary typed read wrapper with its own `Changed` event.

| [INDEX] | [SURFACE]                                                          | [CAPABILITY]                |
| :-----: | :----------------------------------------------------------------- | :-------------------------- |
|  [01]   | `TrackListId` / `TrackListSrcId` / `TrackListFfIndex`              | per-track identity          |
|  [02]   | `TrackListType` (`"audio"`/`"video"`/`"sub"`)                      | per-track lane token        |
|  [03]   | `TrackListLanguage` / `TrackListCodec` / `TrackListDecoderDesc`    | per-track description       |
|  [04]   | `TrackListIsDefault` / `…IsForced` / `…IsSelected` / `…IsExternal` | per-track flags             |
|  [05]   | `TrackListDemuxWidth` / `…Height` / `…Fps` / `…Bitrate`            | per-track stream facts      |
|  [06]   | `PlaylistFileName` / `PlaylistTitle`                               | per-entry playlist identity |
|  [07]   | `PlaylistIsCurrent` / `PlaylistIsPlaying`                          | per-entry playlist state    |
|  [08]   | `ChapterListTitle` / `ChapterListTime`                             | per-chapter identity        |
|  [09]   | `EditionListId` / `EditionListTitle` / `EditionListDefault`        | per-edition identity        |

- `track-list/{0}/type` spells the subtitle lane `"sub"` while its option spells it `sid`, so a lane vocabulary spanning both states the correspondence once.

[ENTRYPOINT_SCOPE]: per-wrapper observation — the OBSERVE seam a UI feed subscribes.

| [INDEX] | [SURFACE]                                                         | [CAPABILITY]                           |
| :-----: | :---------------------------------------------------------------- | :------------------------------------- |
|  [01]   | `MpvPropertyRead<T,TRaw>.Changed`                                 | self-registering typed change event    |
|  [02]   | `MpvPropertyReadRef<T,TRaw>.Changed`                              | self-registering reference-typed event |
|  [03]   | `MpvValueChangedEventArgs<T,TRaw>.PropertyName` / `NewValue : T?` | changed name and nullable typed value  |
|  [04]   | `MpvValueChangedEventArgs<T,TRaw>.NewValueRaw : TRaw`             | the unparsed payload                   |
|  [05]   | `MpvPropertyEventArgs.Format` / `Name` / `Data` / `RequestId`     | raw client-event payload               |

- `Changed` OWNS its registration: a first subscription mints a unique request id, calls `ObserveProperty(id, PropertyName, Format)`, and hooks the context's `PropertyChanged`; a last unsubscribe unhooks and calls `UnobserveProperty(id)`. Its handler filters by that request id and parses `MpvPropertyEventArgs.Data` through the wrapper's own `ParseValue`, so a subscriber spells no property name, tracks no request id, and receives a genuinely optional value.

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

| [INDEX] | [SURFACE]                             | [CAPABILITY]             |
| :-----: | :------------------------------------ | :----------------------- |
|  [01]   | `MpvPropertyRead.Get / GetAsync`      | read property value      |
|  [02]   | `MpvPropertyWrite.Set / SetAsync`     | write property value     |
|  [03]   | `MpvPropertyWrite.Add / AddAsync`     | relative property change |
|  [04]   | `MpvPropertyWrite.Multiply / …Async`  | scale a property value   |
|  [05]   | `MpvPropertyWrite.Cycle / CycleAsync` | step a bounded property  |
|  [06]   | `MpvOption.Get / Set`                 | startup option access    |
|  [07]   | `MpvOptionWithAuto.SetAuto / GetAuto` | the `auto` sentinel      |
|  [08]   | `MpvOptionWithAutoNo.SetNo / GetNo`   | the `no` sentinel        |
|  [09]   | `MpvOptionList.Add / AddAsync`        | append list entry        |
|  [10]   | `MpvOptionList.Remove / RemoveAsync`  | drop list entry          |
|  [11]   | `MpvOptionList.Toggle / ToggleAsync`  | flip list membership     |
|  [12]   | `MpvOptionList.Clear / ClearAsync`    | empty the list option    |
|  [13]   | `MpvCommand.Invoke / InvokeAsync`     | dispatch a built command |

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

## [03]-[IMPLEMENTATION_LAW]

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
- `api-silk-webgpu-wgpu`(`libs/dotnet/.api/api-silk-webgpu-wgpu.md`): media materialization fires `AppUiFact.Media` after `LoadFile` settles, the decode-side peer of wgpu's `PfnLogCallback` `ViewportFault` stream; `LogMessage` and `EndFile` remain typed native observables.
- `api-reactiveui.md`(`.api/api-reactiveui.md`): observed `MpvPropertyRead` members marshalled onto `Dispatcher.UIThread` drive ReactiveUI transport bindings with no cross-thread hop.
- MediaSurface owner: `MpvView` with `Renderer` set to `VideoRenderer.OpenGl`; playback flows through the bound `IVideoView`'s `MpvContext` — `LoadFile` intake, `Pause` / `Speed` / `Volume` / `Mute` transport, `TimePos` / `PercentPos` seek, observed `MpvPropertyRead` and `PropertyChanged` state, `IVideoView.Dispose` teardown.

[LOCAL_ADMISSION]:
- Native libmpv provisions at the app-host distribution layer and binds at load; these assemblies ship no native binary.
- `MpvEventLoop.Default` (the `MpvSimpleEventLoop`) is the Avalonia path: events marshal onto the `Dispatcher.UIThread` the `MpvView` lives on, so `PropertyChanged` updates bindings without a cross-thread hop; `MpvEventLoop.Thread` serves a headless host where no dispatcher pumps.
- `LogMessage` (gated by `RequestLogMessages(minLevel)`) and `EndFile` (`MpvEndFileEventArgs.Reason` : `EndReason`) remain available for playback diagnostics; the media owner publishes the settled `LoadFile` outcome once through `AppUiFact.Media`.
- Every `MpvContext`, view, and overlay releases through `IVideoView.Dispose` at teardown to free the render context.

[MEDIA_BOUNDARY]:
- Neither assembly exposes a decoded-audio (PCM) tap: every render entry is a video path (`StartOpenGlRendering`, `StartSoftwareRendering`, `StartNativeRendering`) and the audio surface is device output selection (`AudioOutput`, `AudioDevice`, `AudioChannels`, `AudioSpdif`). A consumer needing samples reads the source independently; the media-to-caption route back in is the SIDECAR pair `SubAdd`/`AudioAdd`, after which `SubText`/`SubStart`/`SubEnd` carry the player's own timing.

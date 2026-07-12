# [RASM_RHINO_API_RHINOCOMMON_RENDER]

The `Rhino.Render` boundary owns renderer lifecycle over the display pipeline: `RenderPipeline` drives a batch render with pause/resume control into a `RenderWindow`, `RealtimeDisplayMode` publishes the framebuffer and middleground hooks and pass-count state that let a realtime engine draw inside the interactive viewport, `RenderWindow` exposes the channel and per-pixel color access plus post-effect execution control, and `RenderTexture` yields an evaluator and a simulated-texture bake. The host resolves against the current WIP `RhinoCommon` assembly; the realtime path composes `DisplayPipeline` and `DisplayPipelineAttributes` from `api-rhinocommon-display.md` at the framebuffer seam, consumes `ViewInfo`/`ViewportInfo` from `api-rhinocommon-document.md`, and writes `Color4f` pixels the display and capture surfaces read back.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon` — `Rhino.Render`

- package: `RhinoCommon`
- license: proprietary McNeel SDK (host-provided, not centrally pinned)
- assembly: `RhinoCommon.dll` (current WIP host framework, resolved from the installed Rhino application)
- namespace: `Rhino.Render` (`RenderPipeline`, `RealtimeDisplayMode`, `RenderWindow`, `RenderTexture`)
- namespace: `Rhino.Render.PostEffects` (`PostEffectExecutionControl`)
- asset: host-resolved managed reference; the boundary composes it, the manifest never pins it
- rail: host-boundary render

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: renderer lifecycle and realtime display

- rail: host-boundary render

| [INDEX] | [SYMBOL]              | [KIND]             | [CAPABILITY]                                                       |
| :-----: | :-------------------- | :----------------- | :----------------------------------------------------------------- |
|  [01]   | `RenderPipeline`      | batch renderer     | `IDisposable` session with construction, `Render`, pause, resume   |
|  [02]   | `RealtimeDisplayMode` | realtime engine    | viewport hooks, pass count, and post-effect toggle                 |
|  [03]   | `RenderWindow`        | render target      | channel pixel buffer with view binding, color writes, post-effects |
|  [04]   | `RenderTexture`       | procedural texture | evaluator and simulated-texture bake                               |

[PUBLIC_TYPE_SCOPE]: channel, evaluation, and post-effect carriers

- rail: host-boundary render

`RenderWindow.StandardChannels` opens the RGBA, depth, normal, and object-ID framebuffer channels for read/write. The realtime event handles carry `Attributes` and `Pipeline`.

| [INDEX] | [SYMBOL]                                  | [KIND]           | [CAPABILITY]                        |
| :-----: | :---------------------------------------- | :--------------- | :---------------------------------- |
|  [01]   | `RenderWindow.StandardChannels`           | channel enum     | standard framebuffer channel        |
|  [02]   | `RenderTexture.TextureEvaluatorFlags`     | evaluator flags  | `CreateEvaluator` behavior          |
|  [03]   | `RenderTexture.TextureGeneration`         | generation mode  | `SimulateTexture` allow/disallow    |
|  [04]   | `SimulatedTexture`                        | baked texture    | live-evaluation fallback simulation |
|  [05]   | `PostEffects.PostEffectExecutionControl`  | post-effect gate | per-render post-effect execution    |
|  [06]   | `DisplayPipelineSettingsChangedEventArgs` | realtime args    | settings-change event               |
|  [07]   | `InitFramebufferEventArgs`                | realtime args    | framebuffer-initialization event    |
|  [08]   | `DrawMiddlegroundEventArgs`               | realtime args    | middleground-draw event             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `RenderPipeline` — batch render session

- rail: host-boundary render

Construction through `new RenderPipeline(RhinoDoc, RunMode, PlugIn, Size, string, RenderWindow.StandardChannels, bool, bool)` binds the document, plug-in, size, and channel set. `Render()` and `RenderWindow(RhinoView view, Rectangle rect, bool inWindow)` execute full-frame and view-region renders; both return `RenderReturnCode`, and `Ok` marks success. The `GetRenderWindow` overloads resolve the whole session window or scope it by wireframe/source, viewport, and region; `PauseRendering` and `ResumeRendering` control an in-flight render.

| [INDEX] | [SURFACE]                                                                                                | [CALL_SHAPE] |
| :-----: | :------------------------------------------------------------------------------------------------------- | :----------- |
|  [01]   | `new RenderPipeline(RhinoDoc, RunMode, PlugIn, Size, string, RenderWindow.StandardChannels, bool, bool)` | construct    |
|  [02]   | `Render()`                                                                                               | run          |
|  [03]   | `RenderWindow(RhinoView view, Rectangle rect, bool inWindow)`                                            | run          |
|  [04]   | `GetRenderWindow()`                                                                                      | window       |
|  [05]   | `GetRenderWindow(bool withWireframeChannel[, bool fromRenderViewSource])`                                | window       |
|  [06]   | `GetRenderWindow(ViewportInfo viewportInfo, bool fromRenderViewSource[, Rectangle region])`              | window       |
|  [07]   | `PauseRendering()`                                                                                       | control      |
|  [08]   | `ResumeRendering()`                                                                                      | control      |

[ENTRYPOINT_SCOPE]: `RealtimeDisplayMode` — in-viewport realtime hooks

- rail: host-boundary render

The abstract lifecycle every realtime engine implements covers render size, startup, shutdown, started/completed state, and framebuffer availability. The events initialize the framebuffer against `Pipeline`, draw the middleground pass, and react to active-pipeline `DisplayPipelineAttributes` changes; control and pass state select OpenGL, post-effects, and the progressive-refinement ceiling.

| [INDEX] | [SURFACE]                                                                                    | [CALL_SHAPE] |
| :-----: | :------------------------------------------------------------------------------------------- | :----------- |
|  [01]   | `GetRenderSize(out int, out int)`                                                            | abstract     |
|  [02]   | `StartRenderer(int, int, RhinoDoc, ViewInfo, ViewportInfo, bool, RenderWindow)`              | abstract     |
|  [03]   | `ShutdownRenderer()`                                                                         | abstract     |
|  [04]   | `IsRendererStarted()`                                                                        | abstract     |
|  [05]   | `IsCompleted()`                                                                              | abstract     |
|  [06]   | `IsFrameBufferAvailable(ViewInfo)`                                                           | abstract     |
|  [07]   | `OnInitFramebuffer` (`EventHandler<InitFramebufferEventArgs>`)                               | event        |
|  [08]   | `OnDrawMiddleground` (`EventHandler<DrawMiddlegroundEventArgs>`)                             | event        |
|  [09]   | `OnDisplayPipelineSettingsChanged` (`EventHandler<DisplayPipelineSettingsChangedEventArgs>`) | event        |
|  [10]   | `SetUseDrawOpenGl(bool)`                                                                     | control      |
|  [11]   | `PostEffectsOn`                                                                              | pass state   |
|  [12]   | `MaxPasses`                                                                                  | pass state   |

[ENTRYPOINT_SCOPE]: `RenderWindow` and `RenderTexture` — channels and evaluation

- rail: host-boundary render

`RenderWindow.Create` allocates a render window, `SetView` binds its view, `OpenChannel` opens a standard channel for per-pixel read/write, and `SetRGBAChannelColors` writes a `Color4f` block into the RGBA channel over a rectangle. `RegisterPostEffectExecutionControl` registers the post-effect gate. `CreateEvaluator` builds a per-point texture evaluator, while `SimulateTexture` fills a bitmap-backed simulation for a texture that cannot be evaluated live.

| [INDEX] | [SURFACE]                                                                                                | [CALL_SHAPE] |
| :-----: | :------------------------------------------------------------------------------------------------------- | :----------- |
|  [01]   | `RenderWindow.Create(Size)`                                                                              | build        |
|  [02]   | `RenderWindow.SetView(ViewInfo)`                                                                         | build        |
|  [03]   | `RenderWindow.OpenChannel(RenderWindow.StandardChannels)`                                                | channel      |
|  [04]   | `RenderWindow.SetRGBAChannelColors(Rectangle, Color4f[])`                                                | write        |
|  [05]   | `RenderWindow.RegisterPostEffectExecutionControl(PostEffects.PostEffectExecutionControl)`                | post-effect  |
|  [06]   | `RenderTexture.CreateEvaluator(RenderTexture.TextureEvaluatorFlags)`                                     | evaluate     |
|  [07]   | `RenderTexture.SimulateTexture(out SimulatedTexture, RenderTexture.TextureGeneration, int, RhinoObject)` | bake         |

## [04]-[IMPLEMENTATION_LAW]

[RENDER_TOPOLOGY]:

- `RenderPipeline` is the batch owner: one session binds document, plug-in, size, and channels at construction, `Render` runs it into a `RenderWindow`, and `PauseRendering`/`ResumeRendering` gate an in-flight run — the session is `IDisposable`, so the render window and channels release deterministically.
- `RealtimeDisplayMode` is the interactive owner: it does not run a batch, it publishes framebuffer and middleground callbacks that draw into the active `DisplayPipeline`, reads its own `MaxPasses`/`PostEffectsOn` progressive state, and reacts to `DisplayPipelineAttributes` changes. Batch and realtime never merge — one produces a finished window, the other participates per frame.
- Channel access is explicit: `OpenChannel` selects a `StandardChannels` value, `SetRGBAChannelColors` writes a `Color4f` block over a rectangle, and the opened channel is the only per-pixel path — a raw buffer pointer beside it is the deleted form.
- Texture evaluation splits live and baked: `CreateEvaluator` yields the per-point evaluator, and `SimulateTexture` bakes a `SimulatedTexture` only where live evaluation is refused, gated by the `TextureGeneration` mode.

[STACKING]:

- `api-rhinocommon-display.md`: the realtime path is the render→display seam — `RealtimeDisplayMode` framebuffer and middleground events carry the `DisplayPipeline` and its `DisplayPipelineAttributes`, so a realtime engine draws through the display pipeline's state stacks rather than a private surface.
- `api-languageext.md`(`../../.api/api-languageext.md`): a render session, an opened channel, and a texture evaluator are trapped onto the rail — `Try.lift(() => RenderWindow.Create(size)).Run()` and a `using` over the `IDisposable` `RenderPipeline` bounded by an `Eff`; a failed `Render` or refused channel crosses as `Fin<A>`.
- `api-thinktecture-runtime-extensions.md`(`../../.api/api-thinktecture-runtime-extensions.md`): `StandardChannels`, `TextureEvaluatorFlags`, and `TextureGeneration` map at the edge to `[SmartEnum]`/flag owners; the domain composes the bounded channel/mode vocabulary, never the raw host enum.
- `api-rhinocommon-document.md`: `RenderWindow.SetView`/`GetRenderWindow` consume `ViewInfo`/`ViewportInfo`, and the constructed `RenderPipeline` binds a `RhinoDoc` — the document catalog owns those carriers.

[LOCAL_ADMISSION]:

- The `Rhino.Render` types are host handles trapped and mapped at the boundary; a `RenderPipeline`, `RenderWindow`, or `RenderTexture` never appears in a domain signature — the domain sees a `Fin<A>`, a bounded owner, or a canonical shape.
- `RenderPipeline` is the single batch-render owner and `RealtimeDisplayMode` the single realtime owner; a parallel render loop beside either is the collapsed form.

[RAIL_LAW]:

- Package: `RhinoCommon` (`Rhino.Render`)
- Owns: renderer lifecycle (`RenderPipeline` render/pause/resume), realtime display hooks and pass state (`RealtimeDisplayMode`), render-window channel and per-pixel color access, post-effect execution control, and `RenderTexture` evaluation and simulation
- Accept: a batch render session run into a `RenderWindow`; a realtime mode drawing through the display pipeline at the framebuffer seam; explicit `OpenChannel`/`SetRGBAChannelColors` per-pixel access; host handles trapped through `Try.lift(...).Run()` and channel/mode enums mapped to bounded owners at the edge
- Reject: a raw framebuffer pointer beside an opened channel, a private realtime draw surface bypassing the display pipeline, a batch and realtime path merged into one owner, a `RenderPipeline`/`RenderWindow`/`RenderTexture` escaping into a domain signature, and a re-derived color blend where the kernel color rail is composed

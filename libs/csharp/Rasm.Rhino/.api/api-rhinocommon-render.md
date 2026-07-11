# [RASM_RHINO_API_RHINOCOMMON_RENDER]

The `Rhino.Render` boundary owns renderer lifecycle over the display pipeline: `RenderPipeline` drives a batch render with pause/resume control into a `RenderWindow`, `RealtimeDisplayMode` publishes the framebuffer and middleground hooks and pass-count state that let a realtime engine draw inside the interactive viewport, `RenderWindow` exposes the channel and per-pixel color access plus post-effect execution control, and `RenderTexture` yields an evaluator and a simulated-texture bake. The host resolves against the current WIP `RhinoCommon` assembly; the realtime path composes `DisplayPipeline` and `DisplayPipelineAttributes` from `api-rhinocommon-display.md` at the framebuffer seam, consumes `ViewInfo`/`ViewportInfo` from `api-rhinocommon-document.md`, and writes `Color4f` pixels the display and capture surfaces read back.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon` (`Rhino.Render`)
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

| [INDEX] | [SYMBOL]              | [KIND]             | [CAPABILITY]                                                                                        |
| :-----: | :-------------------- | :----------------- | :-------------------------------------------------------------------------------------------------- |
|  [01]   | `RenderPipeline`      | batch renderer     | a `IDisposable` render session: construct, `Render`, and pause/resume over a `RenderWindow`         |
|  [02]   | `RealtimeDisplayMode` | realtime engine    | the in-viewport realtime hook — framebuffer/middleground callbacks, pass count, post-effect toggle  |
|  [03]   | `RenderWindow`        | render target      | the channel-backed pixel buffer: create, view-bind, open channels, set colors, control post-effects |
|  [04]   | `RenderTexture`       | procedural texture | a render-content texture yielding an evaluator and a simulated-texture bake                         |

[PUBLIC_TYPE_SCOPE]: channel, evaluation, and post-effect carriers
- rail: host-boundary render

| [INDEX] | [SYMBOL]                                                                                             | [KIND]           | [CAPABILITY]                                                                                       |
| :-----: | :--------------------------------------------------------------------------------------------------- | :--------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | `RenderWindow.StandardChannels`                                                                      | channel enum     | the standard framebuffer channels (RGBA, depth, normal, object-id) opened for read/write           |
|  [02]   | `RenderTexture.TextureEvaluatorFlags`                                                                | evaluator flags  | the flags selecting evaluator behavior for `CreateEvaluator`                                       |
|  [03]   | `RenderTexture.TextureGeneration`                                                                    | generation mode  | the allow/disallow procedural-generation mode passed to `SimulateTexture`                          |
|  [04]   | `SimulatedTexture`                                                                                   | baked texture    | the bitmap-backed simulation `SimulateTexture` fills for a texture the engine cannot evaluate live |
|  [05]   | `PostEffects.PostEffectExecutionControl`                                                             | post-effect gate | the registered control deciding whether a post-effect executes for a given render                  |
|  [06]   | `DisplayPipelineSettingsChangedEventArgs` / `InitFramebufferEventArgs` / `DrawMiddlegroundEventArgs` | realtime args    | the settings/framebuffer/middleground event handles carrying `Attributes` and the `Pipeline`       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `RenderPipeline` — batch render session
- rail: host-boundary render

| [INDEX] | [SURFACE]                                                                                                                                                                                     | [CALL_SHAPE] | [CAPABILITY]                                                              |
| :-----: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :----------- | :------------------------------------------------------------------------ |
|  [01]   | `new RenderPipeline(RhinoDoc, RunMode, PlugIn, Size, string, RenderWindow.StandardChannels, bool, bool)`                                                                                      | construct    | open a render session bound to a document, plug-in, size, and channel set |
|  [02]   | `Render()` / `RenderWindow(RhinoView view, Rectangle rect, bool inWindow)` — both return `RenderReturnCode`                                                                                   | run          | execute a full-frame or view-region render; `Ok` is the success code      |
|  [03]   | `GetRenderWindow()` / `GetRenderWindow(bool withWireframeChannel[, bool fromRenderViewSource])` / `GetRenderWindow(ViewportInfo viewportInfo, bool fromRenderViewSource[, Rectangle region])` | window       | resolve the session's `RenderWindow`, whole or for a viewport/region      |
|  [04]   | `PauseRendering()` / `ResumeRendering()`                                                                                                                                                      | control      | suspend and resume an in-flight render                                    |

[ENTRYPOINT_SCOPE]: `RealtimeDisplayMode` — in-viewport realtime hooks
- rail: host-boundary render

| [INDEX] | [SURFACE]                                                                                                                                                                                                                 | [CALL_SHAPE] | [CAPABILITY]                                                        |
| :-----: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | :----------- | :------------------------------------------------------------------ |
|  [01]   | `GetRenderSize(out int, out int)` / `StartRenderer(int, int, RhinoDoc, ViewInfo, ViewportInfo, bool, RenderWindow)` / `ShutdownRenderer()` / `IsRendererStarted()` / `IsCompleted()` / `IsFrameBufferAvailable(ViewInfo)` | abstract     | the renderer-lifecycle surface every realtime engine implements     |
|  [02]   | `OnInitFramebuffer` (`EventHandler<InitFramebufferEventArgs>`)                                                                                                                                                            | event        | initialize the framebuffer against the event's `Pipeline`           |
|  [03]   | `OnDrawMiddleground` (`EventHandler<DrawMiddlegroundEventArgs>`)                                                                                                                                                          | event        | draw the realtime middleground pass into the pipeline               |
|  [04]   | `OnDisplayPipelineSettingsChanged` (`EventHandler<DisplayPipelineSettingsChangedEventArgs>`)                                                                                                                              | event        | react to changed `DisplayPipelineAttributes` on the active pipeline |
|  [05]   | `SetUseDrawOpenGl(bool)`                                                                                                                                                                                                  | control      | select the OpenGL draw path for the realtime mode                   |
|  [06]   | `PostEffectsOn` / `MaxPasses`                                                                                                                                                                                             | pass state   | the post-effect toggle and the progressive-refinement pass ceiling  |

[ENTRYPOINT_SCOPE]: `RenderWindow` and `RenderTexture` — channels and evaluation
- rail: host-boundary render

| [INDEX] | [SURFACE]                                                                                                | [CALL_SHAPE] | [CAPABILITY]                                                                |
| :-----: | :------------------------------------------------------------------------------------------------------- | :----------- | :-------------------------------------------------------------------------- |
|  [01]   | `RenderWindow.Create(Size)` / `SetView(ViewInfo)`                                                        | build        | allocate a render window and bind the view it renders                       |
|  [02]   | `RenderWindow.OpenChannel(RenderWindow.StandardChannels)`                                                | channel      | open a standard channel for per-pixel read/write                            |
|  [03]   | `RenderWindow.SetRGBAChannelColors(Rectangle, Color4f[])`                                                | write        | blit a `Color4f` block into the RGBA channel over a rectangle               |
|  [04]   | `RenderWindow.RegisterPostEffectExecutionControl(PostEffects.PostEffectExecutionControl)`                | post-effect  | register the control gating post-effect execution                           |
|  [05]   | `RenderTexture.CreateEvaluator(RenderTexture.TextureEvaluatorFlags)`                                     | evaluate     | build a per-point texture evaluator                                         |
|  [06]   | `RenderTexture.SimulateTexture(out SimulatedTexture, RenderTexture.TextureGeneration, int, RhinoObject)` | bake         | fill a bitmap-backed simulation for a texture that cannot be evaluated live |

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

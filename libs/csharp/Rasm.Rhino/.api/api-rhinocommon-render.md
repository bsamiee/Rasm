# [RASM_RHINO_API_RHINOCOMMON_RENDER]

`Rhino.Render` owns the batch-renderer lifecycle and its framebuffer: `RenderPipeline` drives an abstract batch-render session over a `RhinoDoc`, populating a scene, running full-frame or view-region renders, and gating an in-flight run with pause/resume; `RenderWindow` is the render target, exposing standard framebuffer channels (`Channel`/`ChannelGPU` per-pixel read/write, image adjustment, wireframe channel, post-effect execution registration); `RenderTexture` yields a per-point `TextureEvaluator` or a simulated-texture bake for a texture that cannot evaluate live; and the `Rhino.Render.PostEffects` family (`PostEffect`, `PostEffectState`, `PostEffectUI`, `CustomPostEffectAttribute`, `PostEffectUuids`, `PostEffectExecutionControl`) registers and gates the post-processing effects the framebuffer runs. `api-rhinocommon-render-realtime.md` owns the disjoint realtime slice — `ChangeQueue` scene-change delivery, `RealtimeDisplayMode`, `AsyncRenderContext`, and `LightManagerSupport` — so this catalog draws the line at batch lifecycle, framebuffer channels, texture evaluation, and post-effects, never live scene-change delivery. `api-rhinocommon-rendercontent.md` owns the `RenderTexture` content authoring and `SimulatedTexture` bake carrier this catalog evaluates; `api-rhinocommon-rendersettings.md` owns the `RenderSettings` a render consumes and the `PostEffectCollection` the effects populate; and `api-rhinocommon-document.md` owns the `RhinoDoc`/`ViewInfo`/`ViewportInfo` carriers the pipeline and window bind.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon batch-render surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Render`, `Rhino.Render.PostEffects`
- kernel: `Rasm` (host-agnostic color and size owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: batch-render boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: batch renderer, framebuffer, and texture evaluation
- rail: batch-render boundary

`RenderPipeline` is the abstract batch session a render engine subclasses; `RenderWindow` is the render target with its nested per-channel accessors; `RenderTexture` (evaluation slice) and `TextureEvaluator` yield live per-point color.

| [INDEX] | [SYMBOL]                   | [KIND]             | [CAPABILITY]                                                       |
| :-----: | :------------------------- | :----------------- | :----------------------------------------------------------------- |
|  [01]   | `RenderPipeline`           | batch renderer     | `IDisposable` session: construct, populate scene, render, pause    |
|  [02]   | `RenderWindow`             | render target      | framebuffer channels, view binding, image adjust, post-effect gate |
|  [03]   | `RenderWindow.Channel`     | pixel channel      | per-pixel float/`Color4f` read/write over a channel buffer         |
|  [04]   | `RenderWindow.ChannelGPU`  | GPU channel        | GPU texture-handle channel with OpenGL/Metal handles               |
|  [05]   | `RenderWindow.ImageAdjust` | image adjust       | gamma and dither post-adjustment of the framebuffer                |
|  [06]   | `RenderTexture`            | procedural texture | live per-point evaluator and simulated-texture bake                |
|  [07]   | `TextureEvaluator`         | per-point eval     | `Color4f` evaluation at a UVW with derivative filtering            |

[PUBLIC_TYPE_SCOPE]: post-effect family
- rail: batch-render boundary

`PostEffect` is the abstract base a custom effect derives and `[CustomPostEffect]` registers; `PostEffectState`/`PostEffectUI` are host-handed state and UI collectors; `PostEffectExecutionControl` gates per-render execution.

| [INDEX] | [SYMBOL]                     | [KIND]            | [CAPABILITY]                                                  |
| :-----: | :--------------------------- | :---------------- | :------------------------------------------------------------ |
|  [01]   | `PostEffect`                 | abstract effect   | execute/param/state hooks, channel requirements, change scope |
|  [02]   | `PostEffectState`            | state bag         | typed `TryGetValue<T>`/`SetValue<T>` effect state             |
|  [03]   | `PostEffectUI`               | UI collector      | `ICollapsibleSection` section registration for an effect      |
|  [04]   | `CustomPostEffectAttribute`  | registration attr | type/styles/name/execute-timing/help/delay of a custom effect |
|  [05]   | `PostEffectUuids`            | static id vocab   | built-in post-effect factory ids                              |
|  [06]   | `PostEffectExecutionControl` | execution gate    | per-render `ReadyToExecutePostEffect` decision                |

[ENUM_ROSTERS]:
- `public enum Rhino.Render.RenderPipeline.RenderReturnCode` — `Ok`, `EmptyScene`, `Cancel`, `NoActiveView`, `OnPreCreateWindow`, `NoFrameWndPointer`, `ErrorCreatingWindow`, `ErrorStartingRender`, `EnterModalLoop`, `ExitModalLoop`, `ExitRhino`, `InternalError`; `Render`/`RenderWindow` return it, `Ok` marks success.
- `public enum Rhino.Render.RenderWindow.RenderSuccessCode` — `Completed`, `Failed`; the `EndAsyncRender` argument.
- `public enum Rhino.Render.RenderWindow.StandardChannels` — `None`, `Red`, `Green`, `Blue`, `Alpha`, `RGBA`, `RGB`, `DistanceFromCamera`, `NormalX`, `NormalY`, `NormalZ`, `NormalXYZ`, `LuminanceRed`, `LuminanceGreen`, `LuminanceBlue`, `BackgroundLuminanceRed`, `BackgroundLuminanceGreen`, `BackgroundLuminanceBlue`, `MaterialIds`, `ObjectIds`, `Wireframe`, `AlbedoRed`, `AlbedoGreen`, `AlbedoBlue`, `AlbedoRGB`, `WireframePointsRGBA`, `WireframeIsocurvesRGBA`, `WireframeCurvesRGBA`, `WireframeAnnotationsRGBA`; `ChannelId(StandardChannels)` maps each to its `Guid`.
- `public enum Rhino.Render.RenderTexture.TextureEvaluatorFlags` — `Normal = 0`, `DisableFiltering = 1`, `DisableLocalMapping = 2`, `DisableAdjustment = 4`, `DisableProjectionChange = 8`; the composable `CreateEvaluator` behavior axes.
- `public enum Rhino.Render.RenderTexture.TextureGeneration` — `Allow = 1`, `Disallow = 2`, `Skip = 3`; the `SimulateTexture` generation mode.
- `public enum Rhino.Render.PostEffects.PostEffectType` — `Early`, `ToneMapping`, `Late`; the post-effect execution stage.
- `[Flags] public enum Rhino.Render.PostEffects.PostEffectStyles` — `ExecuteForProductionRendering = 1`, `ExecuteForRealtimeRendering = 2`, `ExecuteForViewportDisplay = 4`, `Fixed = 0x100`, `DefaultShown = 0x200`, `DefaultOn = 0x400`.
- `public enum Rhino.Render.PostEffects.PostEffectExecuteWhileRenderingOptions` — `Never = 0`, `Always = 1`, `UseDelay = 2`, `UseExecutionControl = 3`; `UseExecutionControl` (8.14) hands the per-frame decision to a registered `PostEffectExecutionControl`.

## [03]-[ENTRYPOINTS]

[BATCH_PIPELINE]:
- `Rhino.Render.RenderPipeline(RhinoDoc doc, RunMode mode, PlugIn plugin, Size sizeRendering, string caption, RenderWindow.StandardChannels channels, bool reuseRenderWindow, bool clearLastRendering)` / `RenderPipeline(RhinoDoc, RunMode, PlugIn, Size, string, RenderWindow.StandardChannels, bool, bool, ref AsyncRenderContext aRC)` — protected constructors binding document, plug-in, size, and channel set; the `AsyncRenderContext` overload wires an async engine (`api-rhinocommon-render-realtime.md`).
- `Rhino.Render.RenderPipeline.Render() : RenderReturnCode` / `RenderWindow(RhinoView view, Rectangle rect, bool inWindow) : RenderReturnCode` — full-frame and view-region renders; `LocalizeRenderReturnCode(RenderReturnCode rc) : string` names the code.
- `Rhino.Render.RenderPipeline.GetRenderWindow() : RenderWindow` / `GetRenderWindow(bool withWireframeChannel[, bool fromRenderViewSource]) : RenderWindow` / `GetRenderWindow(ViewportInfo viewportInfo, bool fromRenderViewSource[, Rectangle region]) : RenderWindow` / `GetRenderWindowFromRenderViewSource(bool fromRenderViewSource) : RenderWindow` — mint a fresh caller-owned window wrapper the pipeline never retains.
- `Rhino.Render.RenderPipeline.SaveImage(string fileName, bool saveAlpha) : bool` / `CloseWindow() : bool` / `CommandResult() : Result` / `RenderSessionId : Guid` / `ConfirmationSeconds : int` / `PlugIn : PlugIn` — output, window teardown, command result, and session identity.
- `Rhino.Render.RenderPipeline.RenderSize() : Size` / `RenderSize(RhinoDoc doc) : Size` / `RenderSize(RhinoDoc doc, bool fromRenderSources) : Size` (static) / `SetAsyncRenderContext(ref AsyncRenderContext aRC) : void` — resolved render size and async-context binding.
- `Rhino.Render.RenderPipeline.SupportsPause() : bool` / `PauseRendering() : void` / `ResumeRendering() : void` — virtual pause control gating an in-flight render.
- `Rhino.Render.RenderPipeline.OnRenderBegin() : bool` / `OnRenderWindowBegin(RhinoView view, Rectangle rectangle) : bool` / `OnRenderEnd(RenderEndEventArgs e) : void` / `ContinueModal() : bool` — the four abstract worker hooks a subclass implements; `OnRenderBeginQuiet(Size imageSize) : bool` is the virtual quiet-start variant.
- `Rhino.Render.RenderPipeline.NeedToProcessGeometryTable() : bool` / `NeedToProcessLightTable() : bool` / `RenderSceneWithNoMeshes() : bool` / `IgnoreRhinoObject(RhinoObject obj) : bool` / `AddRenderMeshToScene(RhinoObject obj, Material material, Mesh mesh) : bool` / `AddLightToScene(LightObject light) : bool` — the virtual scene-population hooks that feed the render engine from the document tables.

[RENDER_WINDOW]:
- `Rhino.Render.RenderWindow.Create(Size szSize) : RenderWindow` (static) / `FromSessionId(Guid sessionId) : RenderWindow` (static) / `SessionId : Guid` / `Size() : Size` / `SetSize(Size size) : void` / `SetView(ViewInfo view) : void` — window construction, resolution, and view binding.
- `Rhino.Render.RenderWindow.OpenChannel(StandardChannels id) : RenderWindow.Channel` / `AddChannel(StandardChannels channel) : bool` / `AddWireframeChannel(RhinoDoc doc, ViewportInfo viewport, Size size, Rectangle region) : bool` / `IsChannelAvailable(Guid id) : bool` / `IsChannelShown(Guid id) : bool` — channel open and inventory; `GetRequestedRenderChannels() : Guid[]` / `GetRequestedRenderChannelsAsStandardChannels() : StandardChannels[]` enumerate the requested set.
- `Rhino.Render.RenderWindow.SetRGBAChannelColors(Size size, Color4f[] colors) : void` / `SetRGBAChannelColors(Rectangle rectangle, Color4f[] colors) : void` / `GetBitmap() : Bitmap` — bulk RGBA write and framebuffer read-back.
- `Rhino.Render.RenderWindow.Channel.SetValue(int x, int y, Color4f value) : void` / `SetValue(int x, int y, float value) : void` / `AddValue(int x, int y, Color4f value) : void` / `GetValue(int x, int y, ComponentOrders componentOrder, ref float[] values) : void` / `SetValues(Rectangle rectangle, Size bufferResolution, PixelBuffer colorBuffer) : void` / `GetMinMaxValues(out float min, out float max) : void` / `Clone() : Channel` — the per-pixel channel accessor; `Channel : IDisposable`.
- `Rhino.Render.RenderWindow.ChannelGPU.TextureHandleOpenGL() : uint` / `TextureHandleMetal() : nint` / `DisplayTechnology : DisplayTechnology` / `CopyTo(Channel channel) : void` / `Clone() : ChannelGPU` — the GPU-channel handles and copy-down; `ChannelGPU : IDisposable`.
- `Rhino.Render.RenderWindow.GetAdjust() : ImageAdjust` / `SetAdjust(ImageAdjust imageAdjust) : void` / `ImageAdjust.Gamma : float` / `ImageAdjust.Dither : Dithering.Methods` — the framebuffer gamma/dither adjustment.
- `Rhino.Render.RenderWindow.SetProgress(string text, float progress) : void` / `EndAsyncRender(RenderSuccessCode successCode) : void` / `SetIsRendering(bool is_rendering) : void` / `Invalidate() : void` / `InvalidateArea(Rectangle rect) : void` / `SetRenderOutputRect(Rectangle rect) : void` — progress, async completion, and invalidation.
- `Rhino.Render.RenderWindow.SaveDibAsBitmap(string filename) : void` / `SaveRenderImageAs(string filename, bool saveAlpha) : void` / `SaveRenderImageAs(string filename, Guid renderEngineGuid, bool saveAlpha) : void` — framebuffer egress.
- `Rhino.Render.RenderWindow.RegisterPostEffectExecutionControl(PostEffectExecutionControl ec) : void` — register the per-render post-effect gate; `RenderWindow : IDisposable`, and `Cloned : EventHandler<RenderWindowClonedEventArgs>` is the static clone broadcast.

[TEXTURE_EVALUATION]:
- `Rhino.Render.RenderTexture.CreateEvaluator(RenderTexture.TextureEvaluatorFlags evaluatorFlags) : TextureEvaluator` — virtual per-point evaluator construction over the disable-axis flags; the parameterless overload is obsolete.
- `Rhino.Render.RenderTexture.SimulateTexture(ref SimulatedTexture simulation, TextureGeneration tg, int size = -1, RhinoObject obj = null) : void` / `SimulatedTexture(TextureGeneration tg, int size = -1, RhinoObject obj = null) : SimulatedTexture` / `GenerateTextureSimulation(ref Bitmap bitmap, TextureEvaluatorFlags ef) : bool` — bake a `SimulatedTexture` for a texture that cannot evaluate live; the `bool isForDataOnly` simulate overload is obsolete.
- `Rhino.Render.TextureEvaluator.Initialize() : bool` / `GetColor(Point3d uvw, Vector3d duvwdx, Vector3d duvwdy) : Color4f` / `GetColor(Point3d uvw, Vector3d duvwdx, Vector3d duvwdy, ref Color4f color) : bool` — per-point color evaluation with UVW derivatives; `WriteToByteArray(int width, int height) : SimpleArrayByte` / `WriteToFloatArray(int width, int height) : SimpleArrayFloat` write a rasterized block; `TextureEvaluator : IDisposable`.

[POST_EFFECTS]:
- `Rhino.Render.PostEffects.PostEffect.RegisterPostEffect(PlugIn plugin) : Type[]` / `RegisterPostEffect(Assembly assembly, Guid pluginId) : Type[]` (static) — register a plug-in's `[CustomPostEffect]` effect classes; the base constructor self-reads its attribute.
- `Rhino.Render.PostEffects.PostEffect.Id : Guid` / `PostEffectType : PostEffectType` / `Styles : PostEffectStyles` / `LocalName : string` / `On : bool` / `Shown : bool` / `IsSelected : bool` / `CanDisplayHelp : bool` / `ExecuteWhileRenderingOption : PostEffectExecuteWhileRenderingOptions` / `RequiredChannels : Guid[]` — effect identity, display state, and channel requirements.
- `Rhino.Render.PostEffects.PostEffect.Execute(PostEffectPipeline pipeline, Rectangle rect) : bool` / `CanExecute(PostEffectPipeline pipeline) : bool` / `GetParam(string param, ref object v) : bool` / `SetParam(string param, object v) : bool` / `ResetToFactoryDefaults() : void` / `DisplayHelp() : bool` — the abstract execute, param, and reset hooks a custom effect implements.
- `Rhino.Render.PostEffects.PostEffect.ReadState(PostEffectState state) : bool` / `WriteState(ref PostEffectState state) : bool` / `AddUISections(PostEffectUI ui) : void` / `BeginChange(RenderContent.ChangeContexts changeContext) : void` / `EndChange() : bool` / `Changed() : void` — state IO, UI-section injection, and the change-notification scope mirroring the content protocol.
- `Rhino.Render.PostEffects.PostEffectState.TryGetValue<T>(string name, out T vValue) : bool` / `SetValue<T>(string name, T vValue) : bool` — the typed state bag handed to `ReadState`/`WriteState`; the constructor is internal, so an effect receives it, never mints it.
- `Rhino.Render.PostEffects.PostEffectUI.AddSection(Rhino.UI.Controls.ICollapsibleSection section) : void` — seat an editor section for the effect; host-handed, internal constructor.
- `Rhino.Render.PostEffects.CustomPostEffectAttribute(PostEffectType postEffectType, string name, PostEffectStyles styles = ExecuteForProductionRendering | ExecuteForRealtimeRendering | DefaultOn, PostEffectExecuteWhileRenderingOptions executeWhileRenderingOption = Always, bool canDisplayHelp = false, int executeWhileRenderingDelay = 0)` — `[AttributeUsage(AttributeTargets.Class)]` registration attribute; `PostEffectType`/`Styles`/`ExecuteWhileRenderingOption`/`Name`/`CanDisplayHelp`/`ExecuteWhileRenderingDelay` are settable at the attribute site.
- `Rhino.Render.PostEffects.PostEffectUuids` — static get-only `Guid` properties for the built-in effects: `Glare`, `Bloom`, `Glow`, `Fog`, `DepthOfField`, `Multiplier`, `Noise`, `GaussianBlur`, `WireframePointsRGBA`, `WireframeCurvesRGBA`, `WireframeIsocurvesRGBA`, `WireframeAnnotationsRGBA`, `ToneMapper_Clamp`, `ToneMapper_BlackWhitePoint`, `ToneMapper_Logarithmic`, `ToneMapper_FalseColor`, `ToneMapper_Filmic`, `Gamma`, `Dithering`, `Watermark`, `HueSatLum`, `BriCon`.
- `Rhino.Render.PostEffects.PostEffectExecutionControl.ReadyToExecutePostEffect(Guid pep_id) : bool` / `Detach() : nint` — the abstract per-render gate a subclass implements and `RenderWindow.RegisterPostEffectExecutionControl` registers; `PostEffectExecutionControl : IDisposable`.

## [04]-[IMPLEMENTATION_LAW]

[RENDER_TOPOLOGY]:
- `RenderPipeline` is the single batch owner: one abstract session subclass binds document, plug-in, size, and channels at protected construction, implements the four worker hooks (`OnRenderBegin`/`OnRenderWindowBegin` start the worker, `ContinueModal` polls it, `OnRenderEnd` settles it) and the scene-population virtuals, and `Render` runs it into a `RenderWindow` — the session is `IDisposable` and releases the native render session, while every `GetRenderWindow*` wrapper is caller-owned and disposes at its borrow.
- Channel access is explicit and typed: `OpenChannel` selects a `StandardChannels` value, `Channel.SetValue`/`AddValue`/`SetValues` write per pixel or per block, `SetRGBAChannelColors` writes a `Color4f` block, and the GPU path is `ChannelGPU` with OpenGL/Metal texture handles — a raw framebuffer pointer beside an opened channel is the deleted form.
- Texture evaluation splits live and baked: `CreateEvaluator` yields the per-point `TextureEvaluator` and `SimulateTexture` bakes a `SimulatedTexture` only where live evaluation is refused, gated by the `TextureGeneration` mode; the content authoring the texture graph carries is `api-rhinocommon-rendercontent.md`.
- Post-effects are one registered family: a custom effect derives `PostEffect`, `[CustomPostEffect]` registers its type/styles/timing, `PostEffect.RegisterPostEffect` installs it, and per-render execution rides `PostEffectExecutionControl.ReadyToExecutePostEffect` registered on the `RenderWindow` — a private post-processing pass beside the registered effect is rejected, and the document effect collection is `RenderSettings.PostEffects` (`api-rhinocommon-rendersettings.md`).
- Batch and realtime never merge: this catalog produces a finished window and runs post-effects, while `api-rhinocommon-render-realtime.md` participates per frame and streams scene changes.

[STACKING]:
- `LanguageExt.Core`(`../../.api/api-languageext.md`): a `RenderPipeline`/`RenderWindow`/`RenderWindow.Channel`/`TextureEvaluator`/`PostEffectExecutionControl` `IDisposable` rides a `using` bounded by `Eff`; `Render` returning `RenderReturnCode` projects to `Fin<Unit>` with `Ok` as success; `RenderWindow.Create`/`GetBitmap` traps through `Try.lift(...).Run()` and a null crosses as `Fin<A>`; a channel or effect outcome projects onto the rail.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): `RenderReturnCode`, `StandardChannels`, `RenderSuccessCode`, `TextureEvaluatorFlags`, `TextureGeneration`, `PostEffectType`, `PostEffectStyles`, and `PostEffectExecuteWhileRenderingOptions` map at the edge to `[SmartEnum]`/flag owners; a `RenderReturnCode` result folds into a `[Union]` outcome the domain matches, and the domain composes the bounded channel/effect vocabulary, never the raw host enum.
- `api-rhinocommon-render-realtime.md`: `RenderPipeline`'s `AsyncRenderContext` overload and a realtime engine's `StartRenderer` consume the `RenderWindow` this catalog owns — the realtime catalog participates through the window, this catalog owns the window and its channels.
- `api-rhinocommon-rendercontent.md`: `RenderTexture.CreateEvaluator`/`SimulateTexture` evaluate the content the content catalog authors, and `SimulatedTexture` is its bake carrier; the change scope `PostEffect.BeginChange(RenderContent.ChangeContexts)` names is the content change protocol.
- `api-rhinocommon-rendersettings.md`: a render consumes a `RenderSettings`, and the post-effects populate the `RenderSettings.PostEffects : PostEffectCollection` container the settings catalog owns.
- `api-rhinocommon-document.md`: `RenderPipeline` binds a `RhinoDoc` and `AddRenderMeshToScene`/`AddLightToScene` read the document geometry/light tables, and `RenderWindow.SetView`/`AddWireframeChannel` consume `ViewInfo`/`ViewportInfo` — the document catalog owns those carriers.

[LOCAL_ADMISSION]:
- `Rhino.Render` batch types are host handles trapped and mapped at the boundary; a `RenderPipeline`, `RenderWindow`, `TextureEvaluator`, or `PostEffect` never appears in a domain signature — the domain sees a `Fin<A>`, a bounded owner, or a canonical shape.
- `RenderPipeline` is the single batch-render owner, `RenderWindow` the single framebuffer owner, and `PostEffect` the single effect owner; a parallel render loop, raw framebuffer, or private post-processing pass beside them is the collapsed form.

[RAIL_LAW]:
- Surface: `Rhino.Render` batch slice + `Rhino.Render.PostEffects`
- Owns: the `RenderPipeline` batch session (construct, populate scene, render/pause/resume, save), the `RenderWindow` framebuffer (standard channels, `Channel`/`ChannelGPU` per-pixel access, image adjust, wireframe, post-effect registration), `RenderTexture` evaluation and simulation with `TextureEvaluator`, and the `PostEffect`/`PostEffectState`/`PostEffectUI`/`CustomPostEffectAttribute`/`PostEffectUuids`/`PostEffectExecutionControl` family.
- Accept: a batch render session run into a `RenderWindow`; explicit `OpenChannel`/`Channel.SetValue`/`SetRGBAChannelColors` per-pixel access; live `CreateEvaluator` or gated `SimulateTexture` evaluation; a registered `PostEffect` gated by `PostEffectExecutionControl`; host handles trapped through `Try.lift(...).Run()` and channel/mode/effect enums mapped to bounded owners at the edge.
- Reject: a raw framebuffer pointer beside an opened channel, a batch and realtime path merged into one owner, a private post-processing pass beside a registered `PostEffect`, a re-derived color blend where the kernel color rail is composed, and a `RenderPipeline`/`RenderWindow`/`TextureEvaluator`/`PostEffect` escaping into a domain signature.

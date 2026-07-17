# [RASM_RHINO_API_RHINOCOMMON_RENDER_REALTIME]

`Rhino.Render` owns the realtime-render seam that lets a render engine track a live document and draw inside the interactive viewport: `ChangeQueue` snapshots the scene and streams typed immutable change records (`Mesh`, `MeshInstance`, `Light`, `Material`, `Environment`, `GroundPlane`, `Skylight`, `ClippingPlane`, `DynamicObjectTransform`, `DisplayRenderSettings`, `MappingChannel`) into subclass `Apply*` hooks; `RealtimeDisplayMode` (with its `RealtimeDisplayModeClassInfo` registration descriptor) publishes the framebuffer, middleground, and HUD hooks a realtime engine draws through; `AsyncRenderContext` owns the render-thread lifecycle an asynchronous engine spins; and `LightManagerSupport` registers a custom light source of truth over the document light table. `api-rhinocommon-render.md` owns the disjoint batch slice — `RenderPipeline`, `RenderWindow`, `RenderTexture` evaluation, and the `PostEffects` family — so this catalog draws the line at live scene-change delivery and realtime participation, never batch lifecycle or framebuffer channels. `api-rhinocommon-display.md` owns the `DisplayPipeline`/`DisplayPipelineAttributes` the realtime hooks draw into; `api-rhinocommon-rendercontent.md` owns the `RenderMaterial`/`RenderTexture`/`RenderEnvironment` content the queue resolves by id; `api-rhinocommon-rendersettings.md` owns the `RenderSettings`/`LinearWorkflow` a change carries; and `api-rhinocommon-document.md` owns the `ViewInfo`/`ViewportInfo`/`RhinoDoc` carriers the queue and engine bind.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: RhinoCommon realtime-render surface
- host: `RhinoCommon` (Rhino host runtime, in-process)
- assembly: `RhinoCommon`
- namespaces: `Rhino.Render.ChangeQueue`, `Rhino.Render`
- kernel: `Rasm` (host-agnostic color, vector, and transform owners composed, never re-derived)
- substrate: `LanguageExt.Core`, `Thinktecture.Runtime.Extensions`
- rail: realtime-render boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scene-change queue and payload family
- rail: realtime-render boundary

`ChangeQueue` is the abstract owner; a render engine subclasses it and overrides only the `Apply*` hooks it needs. Every payload is a native-backed immutable snapshot minted internally and delivered to a hook — none carry a public constructor, and each exposes get-only reads of the scene state at snapshot time.

| [INDEX] | [SYMBOL]                    | [KIND]              | [CAPABILITY]                                                          |
| :-----: | :-------------------------- | :------------------ | :------------------------------------------------------------------- |
|  [01]   | `ChangeQueue`               | abstract queue      | scene snapshot, change streaming, content/id resolution, bake policy |
|  [02]   | `Mesh`                      | geometry snapshot   | single mesh, mapping channels, OCS transform, original-object access |
|  [03]   | `MeshInstance`              | instance snapshot   | instance/root/parent ids, material, transform, block ancestry, decals |
|  [04]   | `Light`                     | light snapshot      | id, CRC, free-floating `Geometry.Light` data, change-type            |
|  [05]   | `Material`                  | material snapshot   | mesh-instance and material CRC ids                                    |
|  [06]   | `Environment`              | environment snapshot | frame-buffer fill-mode vocabulary carrier                            |
|  [07]   | `Skylight`                  | skylight snapshot   | enable, custom-environment flag, shadow intensity                    |
|  [08]   | `GroundPlane`               | ground snapshot     | altitude, shadow-only, material CRC, texture scale/offset/rotation   |
|  [09]   | `ClippingPlane`             | clip snapshot       | plane, enable, id, per-viewport clip scope                           |
|  [10]   | `DynamicObjectTransform`    | dynamic-xform       | mesh-instance id and live drag transform                             |
|  [11]   | `DisplayRenderSettings`     | display-render flags | cull-backfaces, flat-shading, scene-lighting display flags           |
|  [12]   | `MappingChannel`            | mapping snapshot    | channel index, local transform, `TextureMapping`                     |
|  [13]   | `MappingChannelCollection`  | mapping set         | indexable channel container with single-mapping collapse             |

[PUBLIC_TYPE_SCOPE]: realtime display engine, async context, and light manager
- rail: realtime-render boundary

`RealtimeDisplayMode` is the realtime engine host a subclass implements; `RealtimeDisplayModeClassInfo` registers it; `AsyncRenderContext` owns an async render thread; `LightManagerSupport` is the abstract custom-light-manager base. `LightArray` and `LightMangerSupportCustomEvent` (host misspelling preserved) are the light-manager collaborators.

| [INDEX] | [SYMBOL]                       | [KIND]             | [CAPABILITY]                                                       |
| :-----: | :----------------------------- | :----------------- | :----------------------------------------------------------------- |
|  [01]   | `RealtimeDisplayMode`          | realtime engine    | render-size/start/shutdown lifecycle, framebuffer + HUD hooks      |
|  [02]   | `RealtimeDisplayModeClassInfo` | registration descr | name, GUID, OpenGL flag, type, required display technology         |
|  [03]   | `AsyncRenderContext`           | async engine base  | render-thread start/join, cancel-gated stop, disposal              |
|  [04]   | `LightManagerSupport`          | light-manager base | custom light CRUD, solo storage, custom-event dispatch             |
|  [05]   | `LightArray`                   | light container    | `ref`-passed light set the manager CRUD reads and writes           |

[ENUM_ROSTERS]:
- `[Flags] [CLSCompliant(false)] public enum Rhino.Render.ChangeQueue.ChangeQueue.BakingFunctions : uint` — `None = 0`, `Decals = 1`, `ProceduralTextures = 2`, `CustomObjectMappings = 4`, `WcsBasedMappings = 8`, `MultipleMappingChannels = 0x10`, `NoRepeatTextures = 0x20`, `All = uint.MaxValue`; the `BakeFor` override return.
- `public enum Rhino.Render.ChangeQueue.Light.Event` — `Added`, `Deleted`, `Undeleted`, `Modified`, `Sorted`; the `Light.ChangeType` payload.
- `[CLSCompliant(false)] public enum Rhino.Render.ChangeQueue.Environment.FrameBufferFillMode : uint` — `None = 0`, `DefaultColor = 1`, `SolidColor = 2`, `Gradient2Color = 3`, `Gradient4Color = 4`, `Bitmap = 5`, `Renderer = 6`, `Transparent = 7`, `Force32Bit = uint.MaxValue`.
- `public enum Rhino.Render.LightMangerSupportCustomEvent` — `light_added`, `light_deleted`, `light_undeleted`, `light_modified`, `light_sorted`; the `LightManagerSupport.OnCustomLightEvent` selector, name misspelled `Manger` in the host API.
- `RealtimeDisplayModeClassInfo.RequiredDisplayTechnology : Rhino.Display.DisplayTechnology` — `api-rhinocommon-display.md` owns the display-technology vocabulary.

## [03]-[ENTRYPOINTS]

[CHANGEQUEUE_LIFECYCLE]:
- `Rhino.Render.ChangeQueue.ChangeQueue(Guid pluginId, uint docRuntimeSerialNumber, ViewInfo viewinfo, DisplayPipelineAttributes attributes, bool bRespectDisplayPipelineAttributes, bool bNotifyChanges)` / `ChangeQueue(Guid pluginId, CreatePreviewEventArgs createPreviewEventArgs)` — protected document-driven and preview-driven constructors; the preview form leaves `IsPreview` true and `ViewId` `Guid.Empty`.
- `Rhino.Render.ChangeQueue.ChangeQueue.OneShot() : void` / `CreateWorld() : void` / `CreateWorld(bool bFlushWhenReady) : void` / `Flush() : void` / `RefreshMaterials() : void` — build the initial world, refresh materials, and drain the queue; `OneShot` builds and tears down a single snapshot.
- `Rhino.Render.ChangeQueue.ChangeQueue.IsPreview : bool` / `ViewId : Guid` / `DisplayPipelineAttributes : DisplayPipelineAttributes` — queue mode, sampled view id, and the attribute snapshot (may be null).
- `Rhino.Render.ChangeQueue.ChangeQueue.GetQueueView() : ViewInfo` / `GetQueueRenderSettings() : RenderSettings` / `GetQueueSkylight() : Skylight` / `GetQueueGroundPlane() : GroundPlane` / `GetQueueSun() : Rhino.Geometry.Light` / `GetQueueSceneBoundingBox() : BoundingBox` — pull the current scene state directly rather than waiting for a change.
- `Rhino.Render.ChangeQueue.ChangeQueue.AreViewsEqual(ViewInfo aView, ViewInfo bView) : bool` — virtual view-equality test a subclass overrides to widen or narrow redraw triggers.
- `Rhino.Render.ChangeQueue.ChangeQueue.CrcFromGuid(Guid guid) : uint` (static) / `ConvertCameraBasedLightToWorld(ChangeQueue changequeue, Light light, ViewInfo vp) : void` (static) — id-to-CRC hashing and camera-space light projection.

[CHANGEQUEUE_CONTENT_RESOLUTION]:
- `Rhino.Render.ChangeQueue.ChangeQueue.MaterialFromId(uint crc) : RenderMaterial` / `MaterialFromOriginalInstanceId(Guid id) : RenderMaterial` / `TextureFromOriginalInstanceId(Guid id) : RenderTexture` / `EnvironmentFromOriginalInstanceId(Guid id) : RenderEnvironment` — resolve the live `RenderContent` a payload CRC or original-instance id names.
- `Rhino.Render.ChangeQueue.ChangeQueue.EnvironmentForid(uint crc) : RenderEnvironment` / `EnvironmentIdForUsage(RenderEnvironment.Usage usage) : uint` / `TextureForId(uint crc) : RenderTexture` — CRC-keyed environment and texture resolution; `EnvironmentIdForUsage` maps a background/reflection/lighting usage to its content CRC.
- `Rhino.Render.ChangeQueue.ChangeQueue.OriginalInstanceIdsFromMaterialId(uint crc) : Guid[]` / `OriginalInstanceIdsFromTextureId(uint crc) : Guid[]` / `OriginalInstanceIdsFromEnvironmentId(uint crc) : Guid[]` — reverse CRC-to-original-instance lookup.

[CHANGEQUEUE_APPLY_HOOKS]:
- `Rhino.Render.ChangeQueue.ChangeQueue.ApplyViewChange(ViewInfo viewInfo) : void` / `ApplyMeshChanges(Guid[] deleted, List<Mesh> added) : void` / `ApplyMeshInstanceChanges(List<uint> deleted, List<MeshInstance> addedOrChanged) : void` / `ApplyDynamicObjectTransforms(List<DynamicObjectTransform> dynamicObjectTransforms) : void` — protected virtual geometry and view hooks a subclass overrides.
- `Rhino.Render.ChangeQueue.ChangeQueue.ApplyLightChanges(List<Light> lightChanges) : void` / `ApplyDynamicLightChanges(List<Rhino.Geometry.Light> dynamicLightChanges) : void` / `ApplySunChanges(Rhino.Geometry.Light sun) : void` / `ApplyMaterialChanges(List<Material> mats) : void` — light, dynamic-light, sun, and material hooks.
- `Rhino.Render.ChangeQueue.ChangeQueue.ApplyRenderSettingsChanges(RenderSettings rs) : void` / `ApplyRenderSettingsChanges(DisplayRenderSettings settings) : void` — the two settings hooks; the `DisplayRenderSettings` overload delivers the display-render flags payload.
- `Rhino.Render.ChangeQueue.ChangeQueue.ApplyEnvironmentChanges(RenderEnvironment.Usage usage) : void` / `ApplySkylightChanges(Skylight skylight) : void` / `ApplyGroundPlaneChanges(GroundPlane gp) : void` / `ApplyLinearWorkflowChanges(LinearWorkflow lw) : void` — environment, skylight, ground-plane, and linear-workflow hooks.
- `Rhino.Render.ChangeQueue.ChangeQueue.ApplyClippingPlaneChanges(Guid[] deleted, List<ClippingPlane> addedOrModified) : void` / `ApplyDynamicClippingPlaneChanges(List<ClippingPlane> changed) : void` / `ApplyDisplayPipelineAttributesChanges(DisplayPipelineAttributes displayPipelineAttributes) : void` — clip, dynamic-clip, and display-attribute hooks.
- `Rhino.Render.ChangeQueue.ChangeQueue.NotifyBeginUpdates() : void` / `NotifyEndUpdates() : void` / `NotifyDynamicUpdatesAreAvailable() : void` — the batched-update bracket a subclass overrides to lock its scene during a flush.
- `Rhino.Render.ChangeQueue.ChangeQueue.ProvideOriginalObject() : bool` / `BakeFor() : BakingFunctions` / `BakingSize(RhinoObject ro, RenderMaterial material, TextureType type) : int` / `ContentRenderHash(RenderContent content, CrcRenderHashFlags flags, string excludeParameterNames, LinearWorkflow lw) : uint` — the policy overrides: enable original-object access on `Mesh`/`MeshInstance`, select bake functions, size a bake, and compute a content render hash.

[CHANGEQUEUE_PAYLOADS]:
- `Rhino.Render.ChangeQueue.Mesh.SingleMesh : Rhino.Geometry.Mesh` / `GetMeshes() : Rhino.Geometry.Mesh[]` / `Id() : Guid` / `Mapping : MappingChannelCollection` / `Mappings : MappingChannel[]` / `OcsTransform : Transform` / `Object : RhinoObject` / `Attributes : ObjectAttributes` — the mesh snapshot; `Object`/`Attributes` read null unless `ProvideOriginalObject` returns true, and `Object` rides a `using`.
- `Rhino.Render.ChangeQueue.MeshInstance.InstanceId : uint` / `RootId : Guid` / `ParentId : Guid` / `MeshId : Guid` / `MaterialId : uint` / `RenderMaterial : RenderMaterial` / `Transform : Transform` / `ObjectAttributes : ObjectAttributes` / `Decals : Decals` / `Ancestry : MeshInstance.AncestryRecord[]` — the instance snapshot; `AncestryRecord : IDisposable` carries per-block-level `ReferenceId`/`DefinitionId`/`ReferenceAttributes`/`Transform`.
- `Rhino.Render.ChangeQueue.Light.Id : Guid` / `IdCrc : uint` / `Data : Rhino.Geometry.Light` / `MaterialId : uint` / `ChangeType : Light.Event` — the light snapshot; `Data` is a free-floating geometry light.
- `Rhino.Render.ChangeQueue.Material.Id : uint` / `MeshInstanceId : uint` — the material snapshot; the live material resolves through `ChangeQueue.MaterialFromId(Id)`.
- `Rhino.Render.ChangeQueue.GroundPlane.Enabled : bool` / `IsShadowOnly : bool` / `ShowUnderside : bool` / `Altitude : double` / `MaterialId : uint` / `TextureScale : Vector2d` / `TextureOffset : Vector2d` / `TextureRotation : double` / `Crc : uint` — the ground-plane snapshot.
- `Rhino.Render.ChangeQueue.Skylight.Enabled : bool` / `UsesCustomEnvironment : bool` / `ShadowIntensity : double` — the skylight snapshot.
- `Rhino.Render.ChangeQueue.ClippingPlane.IsEnabled : bool` / `Plane : Plane` / `Id : Guid` / `Attributes : ObjectAttributes` / `ViewIds : List<Guid>` / `ClipViewports : RhinoViewport[]` — the clip snapshot; `ViewIds`/`ClipViewports` scope the clip to viewports.
- `Rhino.Render.ChangeQueue.DynamicObjectTransform.MeshInstanceId : uint` / `Transform : Transform` — the live-drag transform snapshot.
- `Rhino.Render.ChangeQueue.DisplayRenderSettings.CullBackFaces : bool` / `ForceFlatShading : bool` / `SceneLightingOn : bool` — the display-render flag snapshot; the getters throw `NotImplementedException` in the current host build, so the surface is bound but unresolved.
- `Rhino.Render.ChangeQueue.MappingChannel.Channel : int` / `Local : Transform` / `Mapping : TextureMapping` — one mapping channel; `MappingChannelCollection.Count : int` / `this[int i] : MappingChannel` / `SingleMapping : MappingChannel` / `Channels : IEnumerable<MappingChannel>` cursors the set.

[REALTIME_DISPLAY_ENGINE]:
- `Rhino.Render.RealtimeDisplayMode.GetRenderSize(out int width, out int height) : void` / `StartRenderer(int w, int h, RhinoDoc doc, ViewInfo view, ViewportInfo viewportInfo, bool forCapture, RenderWindow renderWindow) : bool` / `ShutdownRenderer() : void` / `IsRendererStarted() : bool` / `IsCompleted() : bool` / `IsFrameBufferAvailable(ViewInfo view) : bool` — the abstract renderer lifecycle every engine implements.
- `Rhino.Render.RealtimeDisplayMode.CreateWorld(RhinoDoc doc, ViewInfo viewInfo, DisplayPipelineAttributes displayPipelineAttributes) : void` / `PostConstruct() : void` / `OnRenderSizeChanged(int width, int height) : bool` / `LastRenderedPass() : int` / `ShowCaptureProgress() : bool` / `CaptureProgress() : double` — virtual world-build, resize, and capture-progress hooks.
- `Rhino.Render.RealtimeDisplayMode.DrawOpenGl() : bool` / `UseFastDraw() : bool` / `SetUseDrawOpenGl(bool use) : void` / `OpenGlVersion() : int` — OpenGL participation control.
- `Rhino.Render.RealtimeDisplayMode.SignalRedraw() : void` / `SetView(ViewInfo view) : void` / `GetView() : ViewInfo` / `ComputeViewportCrc(ViewInfo view) : uint` — redraw signalling and view binding.
- `Rhino.Render.RealtimeDisplayMode.Paused : bool` / `Locked : bool` / `PostEffectsOn : bool` / `MaxPasses : int` / `LinearWorkflow : LinearWorkflow` — progressive-refinement state.
- `Rhino.Render.RealtimeDisplayMode.OnInitFramebuffer : EventHandler<InitFramebufferEventArgs>` / `OnDrawMiddleground : EventHandler<DrawMiddlegroundEventArgs>` / `OnDisplayPipelineSettingsChanged : EventHandler<DisplayPipelineSettingsChangedEventArgs>` / `MaxPassesChanged : EventHandler<HudMaxPassesChangedEventArgs>` — the four framebuffer/pipeline events, whose nested args carry `Pipeline : DisplayPipeline`, `Attributes : DisplayPipelineAttributes`, and `MaxPasses : int`.
- `Rhino.Render.RealtimeDisplayMode.Hud{Play,Pause,Lock,Unlock,ProductName,StatusText,Time}Button{Left,Right,Double}Clicked` and `HudPostEffects{On,Off}Button{Left,Right,Double}Clicked : EventHandler` — the HUD click roster; the `Hud*` virtual predicates (`HudShow`, `HudShowControls`, `HudShowPasses`, `HudShowMaxPasses`, `HudAllowEditMaxPasses`, `HudShowCustomStatusText`, `HudRendererPaused`, `HudRendererLocked`, `HudMaximumPasses`, `HudLastRenderedPass`, `HudProductName`, `HudCustomStatusText`, `HudStartTime`) configure the HUD.
- `Rhino.Render.RealtimeDisplayMode.RegisterDisplayModes(PlugIn plugin) : RealtimeDisplayModeClassInfo[]` / `RegisterDisplayModes(Assembly assembly, Guid pluginId) : RealtimeDisplayModeClassInfo[]` / `UnregisterDisplayModes(PlugIn plugin) : void` / `GetRealtimeViewport(nint realtimeViewport[, bool create]) : RealtimeDisplayMode` / `RemoveRealtimeViewport(nint realtimeViewport) : void` — the static registration and viewport-resolution surface.
- `Rhino.Render.RealtimeDisplayModeClassInfo.Name : string` / `GUID : Guid` / `DrawOpenGl : bool` / `RealtimeDisplayModeType : Type` / `DontRegisterAttributesOnStart : bool` / `RequiredDisplayTechnology : DisplayTechnology` — the abstract registration descriptor; `DrawOpenGl` true skips `RenderWindow` use.

[ASYNC_CONTEXT]:
- `Rhino.Render.AsyncRenderContext.RenderWindow : RenderWindow` / `RenderThread : Thread` — the render-window handle and the spawned render thread (private setter, set through `StartRenderThread`).
- `Rhino.Render.AsyncRenderContext.StartRenderThread(ThreadStart threadStart, string threadName) : bool` / `JoinRenderThread() : void` / `StopRendering() : void` — thread start, join, and cancel; `StopRendering` sets the protected `Cancel` flag, and an override calls base.
- `Rhino.Render.AsyncRenderContext.DeleteThis() : void` / `Dispose() : void` — protected native-teardown hook and disposal; `AsyncRenderContext : IDisposable`.

[LIGHT_MANAGER]:
- `Rhino.Render.LightManagerSupport.PluginId() : Guid` / `RenderEngineId() : Guid` — the abstract identity pair a manager supplies.
- `Rhino.Render.LightManagerSupport.GetLights(RhinoDoc doc, ref LightArray light_array) : void` / `LightFromId(RhinoDoc doc, Guid uuid, ref Light light) : bool` / `ModifyLight(RhinoDoc doc, Light light) : void` / `DeleteLight(RhinoDoc doc, Light light, bool bUndelete) : bool` / `ObjectSerialNumberFromLight(RhinoDoc doc, ref Light light) : int` / `LightDescription(RhinoDoc doc, ref Light light) : string` — the abstract light-CRUD surface over `Rhino.Geometry.Light`.
- `Rhino.Render.LightManagerSupport.OnEditLight(RhinoDoc doc, ref LightArray light_array) : bool` / `GroupLights(RhinoDoc doc, ref LightArray light_array) : void` / `UnGroup(RhinoDoc doc, ref LightArray light_array) : void` — edit and grouping over the `LightArray` set.
- `Rhino.Render.LightManagerSupport.SetLightSolo(RhinoDoc doc, Guid uuid_light, bool bSolo) : bool` / `GetLightSolo(RhinoDoc doc, Guid uuid_light) : bool` / `LightsInSoloStorage(RhinoDoc doc) : int` — virtual solo-storage support.
- `Rhino.Render.LightManagerSupport.OnCustomLightEvent(RhinoDoc doc, LightMangerSupportCustomEvent le, ref Light light) : void` — custom-event dispatch over the bounded event selector.
- `Rhino.Render.LightManagerSupport.RegisterLightManager(PlugIn plugin) : void` / `RegisterProviders(Assembly assembly, Guid pluginId) : void` — static registration by plug-in or assembly.

## [04]-[IMPLEMENTATION_LAW]

[REALTIME_TOPOLOGY]:
- `ChangeQueue` is the single scene-delivery owner: a subclass constructs it document-bound or preview-bound, overrides the `Apply*` hooks it consumes, and drives it with `CreateWorld`/`Flush`/`OneShot` — a private scene diff beside the queue is the deleted form. Each `Apply*` hook receives an immutable native-backed payload record; a payload never mutates and never carries a public constructor, so the queue is the sole payload source.
- Content is resolved, never re-carried: a payload names a CRC or original-instance id, and `ChangeQueue.MaterialFromId`/`TextureFromOriginalInstanceId`/`EnvironmentForid` resolve the live `RenderContent` — a payload never embeds the content graph, and `api-rhinocommon-rendercontent.md` owns the resolved content.
- `RealtimeDisplayMode` is the interactive engine owner: it implements the abstract render-size/start/shutdown lifecycle, draws through the `DisplayPipeline` its framebuffer and middleground events carry, and reads its own `MaxPasses`/`PostEffectsOn` progressive state — it does not run a batch and never produces a finished window. Batch lifecycle is `api-rhinocommon-render.md`; the two never merge.
- `AsyncRenderContext` owns the render thread: `StartRenderThread` spins it, `Cancel` gates it, `StopRendering` sets the flag, and `JoinRenderThread` settles it — an engine never spawns a raw thread beside this context.
- `LightManagerSupport` is the custom light source of truth: it registers a manager that answers the document light table over `LightArray`, and solo state rides its virtual solo-storage hooks — a parallel light registry beside the registered manager is rejected.

[STACKING]:
- `LanguageExt.Core`(`../../.api/api-languageext.md`): a `ChangeQueue`/`AsyncRenderContext`/`LightArray` `IDisposable` rides a `using` bounded by `Eff`; a nullable content resolution (`MaterialFromId`, `TextureFromOriginalInstanceId`) lifts to `Option<RenderContent>`; a `StartRenderThread` or `DeleteLight` `bool` projects to `Fin<Unit>`; and the `Apply*` hook payload lists cross as `Seq<A>`.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): `BakingFunctions`, `Light.Event`, `Environment.FrameBufferFillMode`, and `LightMangerSupportCustomEvent` map at the edge to `[SmartEnum]`/flag owners; the `ChangeType`/`ChangeReason` discriminant a payload carries collapses to one `[Union]` change owner keyed on the payload kind the `Apply*` hook already dispatches.
- `api-rhinocommon-render.md`: `RealtimeDisplayMode.StartRenderer` and `AsyncRenderContext.RenderWindow` consume the `RenderWindow` the batch catalog owns, and the realtime engine writes `Color4f` pixels through its channels — render.md owns the window, this catalog owns the realtime participation.
- `api-rhinocommon-display.md`: framebuffer and middleground events carry a `DisplayPipeline` and its `DisplayPipelineAttributes`, and `RealtimeDisplayModeClassInfo.RequiredDisplayTechnology` names a `DisplayTechnology` — a realtime engine draws through the display pipeline's state stacks, never a private surface.
- `api-rhinocommon-rendercontent.md`: `MaterialFromId`/`EnvironmentForid`/`TextureForId` resolve `RenderMaterial`/`RenderEnvironment`/`RenderTexture`, and `MeshInstance.Decals`/`RenderMaterial` and `MappingChannel.Mapping : TextureMapping` are content the queue reads; the content catalog owns them.
- `api-rhinocommon-document.md` and `api-rhinocommon-geometry.md`: `ViewInfo`/`ViewportInfo`/`RhinoDoc` bind the queue and engine, and `Rhino.Geometry.Mesh`/`Light`/`Transform`/`Plane`/`Vector2d`/`BoundingBox` are the geometry carriers a payload exposes — the document and geometry catalogs own the carriers, this catalog owns the snapshot.

[LOCAL_ADMISSION]:
- A change enters through the subclass `Apply*` hook or a `GetQueue*` pull; a payload stays inside the hook grant, and downstream code receives a detached value record, a resolved content reference, or a projected receipt — never a live payload escaping the hook.
- `ChangeQueue` is the sole scene-diff owner; content resolves through its CRC/id resolvers; the realtime engine is `RealtimeDisplayMode`; the async thread is `AsyncRenderContext`; the light manager is `LightManagerSupport`. A hand-rolled scene tracker, thread loop, or light registry beside these owners is the collapsed form.

[RAIL_LAW]:
- Surface: `Rhino.Render.ChangeQueue` + `Rhino.Render` realtime slice
- Owns: the `ChangeQueue` scene-delivery owner (construction, world build, flush, content/id resolution, bake policy, the `Apply*`/`Notify*` hook surface), the twelve immutable payload records, the `RealtimeDisplayMode`/`RealtimeDisplayModeClassInfo` realtime engine and its framebuffer/middleground/HUD hooks, `AsyncRenderContext` render-thread lifecycle, and `LightManagerSupport` custom light management.
- Accept: a subclass overriding the `Apply*` hooks it consumes, resolving content through the queue's CRC/id resolvers, and drawing through the display pipeline at the framebuffer seam; a registered realtime mode or light manager; host handles trapped through `Try.lift(...).Run()` and change/event enums mapped to bounded owners at the edge.
- Reject: a private scene diff beside `ChangeQueue`, a payload mutated or re-carried with its content graph, a batch render path merged into the realtime engine, a raw render thread beside `AsyncRenderContext`, a parallel light registry beside the registered manager, and a `ChangeQueue`/payload/`RealtimeDisplayMode` escaping into a domain signature.

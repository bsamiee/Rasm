# [RASM_RHINO_API_RHINOCOMMON_RENDER_REALTIME]

`Rhino.Render` owns the realtime-render seam a live engine draws through: a subclass receives typed immutable scene-change snapshots through `ChangeQueue`'s `Apply*` hooks, draws the interactive viewport through `RealtimeDisplayMode`'s framebuffer/middleground/HUD hooks, spins its render thread through `AsyncRenderContext`, and answers the document light table through `LightManagerSupport`. Content resolves by CRC or id through the queue rather than riding the payload; batch lifecycle and framebuffer channels are the disjoint `api-rhinocommon-render.md` slice this catalog never crosses.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `RhinoCommon`
- package: `RhinoCommon` (proprietary McNeel SDK)
- assembly: `RhinoCommon.dll` (host-resolved from the installed Rhino app, never NuGet-pinned)
- namespace: `Rhino.Render.ChangeQueue` (scene-change queue and immutable payload family)
- namespace: `Rhino.Render` (realtime engine, async context, light manager)
- rail: realtime-render boundary

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: scene-change queue and payload family

`ChangeQueue` is the abstract owner a render engine subclasses, overriding only the `Apply*` hooks it consumes; no payload carries a public constructor, and each exposes get-only reads of the scene at snapshot time.

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]        | [CAPABILITY]                                                          |
| :-----: | :------------------------- | :------------------- | :-------------------------------------------------------------------- |
|  [01]   | `ChangeQueue`              | abstract queue       | scene snapshot, change streaming, content/id resolution, bake policy  |
|  [02]   | `Mesh`                     | geometry snapshot    | single mesh, mapping channels, OCS transform, original-object access  |
|  [03]   | `MeshInstance`             | instance snapshot    | instance/root/parent ids, material, transform, block ancestry, decals |
|  [04]   | `Light`                    | light snapshot       | id, CRC, free-floating `Geometry.Light` data, change-type             |
|  [05]   | `Material`                 | material snapshot    | mesh-instance and material CRC ids                                    |
|  [06]   | `Environment`              | environment snapshot | frame-buffer fill-mode vocabulary carrier                             |
|  [07]   | `Skylight`                 | skylight snapshot    | enable, custom-environment flag, shadow intensity                     |
|  [08]   | `GroundPlane`              | ground snapshot      | altitude, shadow-only, material CRC, texture scale/offset/rotation    |
|  [09]   | `ClippingPlane`            | clip snapshot        | plane, enable, id, per-viewport clip scope                            |
|  [10]   | `DynamicObjectTransform`   | dynamic-xform        | mesh-instance id and live drag transform                              |
|  [11]   | `DisplayRenderSettings`    | display-render flags | cull-backfaces, flat-shading, scene-lighting display flags            |
|  [12]   | `MappingChannel`           | mapping snapshot     | channel index, local transform, `TextureMapping`                      |
|  [13]   | `MappingChannelCollection` | mapping set          | indexable channel container with single-mapping collapse              |

[PUBLIC_TYPE_SCOPE]: realtime display engine, async context, and light manager

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [CAPABILITY]                                                  |
| :-----: | :----------------------------- | :----------------- | :------------------------------------------------------------ |
|  [01]   | `RealtimeDisplayMode`          | realtime engine    | render-size/start/shutdown lifecycle, framebuffer + HUD hooks |
|  [02]   | `RealtimeDisplayModeClassInfo` | registration descr | name, GUID, OpenGL flag, type, required display technology    |
|  [03]   | `AsyncRenderContext`           | async engine base  | render-thread start/join, cancel-gated stop, disposal         |
|  [04]   | `LightManagerSupport`          | light-manager base | custom light CRUD, solo storage, custom-event dispatch        |
|  [05]   | `LightArray`                   | light container    | `ref`-passed light set the manager CRUD reads and writes      |

[ENUM_ROSTERS]:
- `[Flags] [CLSCompliant(false)] public enum Rhino.Render.ChangeQueue.ChangeQueue.BakingFunctions : uint` — `None = 0`, `Decals = 1`, `ProceduralTextures = 2`, `CustomObjectMappings = 4`, `WcsBasedMappings = 8`, `MultipleMappingChannels = 0x10`, `NoRepeatTextures = 0x20`, `All = uint.MaxValue`; the `BakeFor` override return.
- `public enum Rhino.Render.ChangeQueue.Light.Event` — `Added`, `Deleted`, `Undeleted`, `Modified`, `Sorted`; the `Light.ChangeType` payload.
- `[CLSCompliant(false)] public enum Rhino.Render.ChangeQueue.Environment.FrameBufferFillMode : uint` — `None = 0`, `DefaultColor = 1`, `SolidColor = 2`, `Gradient2Color = 3`, `Gradient4Color = 4`, `Bitmap = 5`, `Renderer = 6`, `Transparent = 7`, `Force32Bit = uint.MaxValue`.
- `public enum Rhino.Render.LightMangerSupportCustomEvent` — `light_added`, `light_deleted`, `light_undeleted`, `light_modified`, `light_sorted`; the `LightManagerSupport.OnCustomLightEvent` selector, `Manger` misspelled in the host API.
- `RealtimeDisplayModeClassInfo.RequiredDisplayTechnology : Rhino.Display.DisplayTechnology` — `api-rhinocommon-display.md` owns the display-technology vocabulary.

## [03]-[ENTRYPOINTS]

[CHANGEQUEUE_LIFECYCLE]:
- `ChangeQueue(Guid, uint, ViewInfo, DisplayPipelineAttributes, bool, bool)` / `ChangeQueue(Guid, CreatePreviewEventArgs)` — protected document- and preview-driven ctors; the preview form leaves `IsPreview` true and `ViewId` `Guid.Empty`.
- `ChangeQueue.OneShot()` / `CreateWorld()` / `CreateWorld(bool)` / `Flush()` / `RefreshMaterials()` — build the initial world, refresh materials, drain the queue; `OneShot` builds and tears down one snapshot.
- `ChangeQueue.IsPreview` / `ViewId` / `DisplayPipelineAttributes` — queue mode, sampled view id, and the attribute snapshot, null when unset.
- `ChangeQueue.GetQueueView() -> ViewInfo` / `GetQueueRenderSettings() -> RenderSettings` / `GetQueueSkylight() -> Skylight` / `GetQueueGroundPlane() -> GroundPlane` / `GetQueueSun() -> Light` / `GetQueueSceneBoundingBox() -> BoundingBox` — pull current scene state directly, no change wait.
- `ChangeQueue.AreViewsEqual(ViewInfo, ViewInfo) -> bool` — virtual redraw-trigger equality test a subclass widens or narrows.
- `ChangeQueue.CrcFromGuid(Guid) -> uint` / `ConvertCameraBasedLightToWorld(ChangeQueue, Light, ViewInfo)` — static id-to-CRC hash and camera-space light projection.

[CHANGEQUEUE_CONTENT_RESOLUTION]:
- `ChangeQueue.MaterialFromId(uint) -> RenderMaterial` / `MaterialFromOriginalInstanceId(Guid) -> RenderMaterial` / `TextureFromOriginalInstanceId(Guid) -> RenderTexture` / `EnvironmentFromOriginalInstanceId(Guid) -> RenderEnvironment` — resolve the live `RenderContent` a CRC or original-instance id names.
- `ChangeQueue.EnvironmentForid(uint) -> RenderEnvironment` / `TextureForId(uint) -> RenderTexture` / `EnvironmentIdForUsage(RenderEnvironment.Usage) -> uint` — CRC-keyed environment and texture; `EnvironmentIdForUsage` maps a usage to its content CRC.
- `ChangeQueue.OriginalInstanceIdsFromMaterialId(uint)` / `OriginalInstanceIdsFromTextureId(uint)` / `OriginalInstanceIdsFromEnvironmentId(uint) -> Guid[]` — reverse CRC-to-original-instance lookup.

[CHANGEQUEUE_APPLY_HOOKS]:
- `ChangeQueue.ApplyViewChange(ViewInfo)` / `ApplyMeshChanges(Guid[], List<Mesh>)` / `ApplyMeshInstanceChanges(List<uint>, List<MeshInstance>)` / `ApplyDynamicObjectTransforms(List<DynamicObjectTransform>)` — protected virtual geometry and view hooks.
- `ChangeQueue.ApplyLightChanges(List<Light>)` / `ApplyDynamicLightChanges(List<Light>)` / `ApplySunChanges(Light)` / `ApplyMaterialChanges(List<Material>)` — light, dynamic-light, sun, and material hooks.
- `ChangeQueue.ApplyRenderSettingsChanges(RenderSettings)` / `ApplyRenderSettingsChanges(DisplayRenderSettings)` — the two settings hooks; the `DisplayRenderSettings` overload delivers the display-render flags.
- `ChangeQueue.ApplyEnvironmentChanges(RenderEnvironment.Usage)` / `ApplySkylightChanges(Skylight)` / `ApplyGroundPlaneChanges(GroundPlane)` / `ApplyLinearWorkflowChanges(LinearWorkflow)` — environment, skylight, ground-plane, and linear-workflow hooks.
- `ChangeQueue.ApplyClippingPlaneChanges(Guid[], List<ClippingPlane>)` / `ApplyDynamicClippingPlaneChanges(List<ClippingPlane>)` / `ApplyDisplayPipelineAttributesChanges(DisplayPipelineAttributes)` — clip, dynamic-clip, and display-attribute hooks.
- `ChangeQueue.NotifyBeginUpdates()` / `NotifyEndUpdates()` / `NotifyDynamicUpdatesAreAvailable()` — the batched-update bracket a subclass locks its scene across during a flush.
- `ChangeQueue.ProvideOriginalObject() -> bool` / `BakeFor() -> BakingFunctions` / `BakingSize(RhinoObject, RenderMaterial, TextureType) -> int` / `ContentRenderHash(RenderContent, CrcRenderHashFlags, string, LinearWorkflow) -> uint` — policy overrides: enable original-object access, select bake functions, size a bake, hash content.

[CHANGEQUEUE_PAYLOADS]:
- `Mesh`: `SingleMesh` `GetMeshes()` `Id()` `Mapping` `Mappings` `OcsTransform` `Object` `Attributes` — `Object`/`Attributes` read null unless `ProvideOriginalObject` is true, and `Object` rides a `using`.
- `MeshInstance`: `InstanceId` `RootId` `ParentId` `MeshId` `MaterialId` `RenderMaterial` `Transform` `ObjectAttributes` `Decals` `Ancestry` — `Ancestry` is `AncestryRecord[] : IDisposable` carrying per-block `ReferenceId`/`DefinitionId`/`ReferenceAttributes`/`Transform`.
- `Light`: `Id` `IdCrc` `Data` `MaterialId` `ChangeType` — `Data` is a free-floating `Rhino.Geometry.Light`.
- `Material`: `Id` `MeshInstanceId` — the live material resolves through `MaterialFromId(Id)`.
- `GroundPlane`: `Enabled` `IsShadowOnly` `ShowUnderside` `Altitude` `MaterialId` `TextureScale` `TextureOffset` `TextureRotation` `Crc`.
- `Skylight`: `Enabled` `UsesCustomEnvironment` `ShadowIntensity`.
- `ClippingPlane`: `IsEnabled` `Plane` `Id` `Attributes` `ViewIds` `ClipViewports` — `ViewIds`/`ClipViewports` scope the clip to viewports.
- `DynamicObjectTransform`: `MeshInstanceId` `Transform`.
- `DisplayRenderSettings`: `CullBackFaces` `ForceFlatShading` `SceneLightingOn` — the getters throw `NotImplementedException`, so the payload is delivered but its reads unresolved.
- `MappingChannel`: `Channel` `Local` `Mapping`; `MappingChannelCollection`: `Count` `this[int]` `SingleMapping` `Channels`.

[REALTIME_DISPLAY_ENGINE]:
- `RealtimeDisplayMode.GetRenderSize(out int, out int)` / `StartRenderer(int, int, RhinoDoc, ViewInfo, ViewportInfo, bool, RenderWindow) -> bool` / `ShutdownRenderer()` / `IsRendererStarted() -> bool` / `IsCompleted() -> bool` / `IsFrameBufferAvailable(ViewInfo) -> bool` — the abstract renderer lifecycle every engine implements.
- `RealtimeDisplayMode.CreateWorld(RhinoDoc, ViewInfo, DisplayPipelineAttributes)` / `PostConstruct()` / `OnRenderSizeChanged(int, int) -> bool` / `LastRenderedPass() -> int` / `ShowCaptureProgress() -> bool` / `CaptureProgress() -> double` — virtual world-build, resize, and capture-progress hooks.
- `RealtimeDisplayMode.DrawOpenGl() -> bool` / `UseFastDraw() -> bool` / `SetUseDrawOpenGl(bool)` / `OpenGlVersion() -> int` — OpenGL participation control.
- `RealtimeDisplayMode.SignalRedraw()` / `SetView(ViewInfo)` / `GetView() -> ViewInfo` / `ComputeViewportCrc(ViewInfo) -> uint` — redraw signalling and view binding.
- `RealtimeDisplayMode.Paused` / `Locked` / `PostEffectsOn` / `MaxPasses` / `LinearWorkflow` — progressive-refinement state.
- `RealtimeDisplayMode.OnInitFramebuffer` / `OnDrawMiddleground` / `OnDisplayPipelineSettingsChanged` / `MaxPassesChanged` — the four framebuffer/pipeline events, nested args carrying `Pipeline : DisplayPipeline`, `Attributes : DisplayPipelineAttributes`, `MaxPasses : int`.
- `RealtimeDisplayMode.Hud{Play,Pause,Lock,Unlock,ProductName,StatusText,Time}Button{Left,Right,Double}Clicked` / `HudPostEffects{On,Off}Button{Left,Right,Double}Clicked` — the HUD click roster; the `Hud*` virtual predicates (`HudShow`, `HudShowControls`, `HudShowPasses`, `HudShowMaxPasses`, `HudAllowEditMaxPasses`, `HudShowCustomStatusText`, `HudRendererPaused`, `HudRendererLocked`, `HudMaximumPasses`, `HudLastRenderedPass`, `HudProductName`, `HudCustomStatusText`, `HudStartTime`) configure the HUD.
- `RealtimeDisplayMode.RegisterDisplayModes(PlugIn) -> RealtimeDisplayModeClassInfo[]` / `RegisterDisplayModes(Assembly, Guid) -> RealtimeDisplayModeClassInfo[]` / `UnregisterDisplayModes(PlugIn)` / `GetRealtimeViewport(nint[, bool]) -> RealtimeDisplayMode` / `RemoveRealtimeViewport(nint)` — static registration and viewport resolution.
- `RealtimeDisplayModeClassInfo`: `Name` `GUID` `DrawOpenGl` `RealtimeDisplayModeType` `DontRegisterAttributesOnStart` `RequiredDisplayTechnology` — the abstract registration descriptor; `DrawOpenGl` true skips `RenderWindow` use.

[ASYNC_CONTEXT]:
- `AsyncRenderContext.RenderWindow` / `RenderThread` — the render-window handle and spawned render thread, private setter set through `StartRenderThread`.
- `AsyncRenderContext.StartRenderThread(ThreadStart, string) -> bool` / `JoinRenderThread()` / `StopRendering()` — thread start, join, and cancel; `StopRendering` sets the protected `Cancel` flag, an override calling base.
- `AsyncRenderContext.DeleteThis()` / `Dispose()` — protected native-teardown hook and disposal; `AsyncRenderContext : IDisposable`.

[LIGHT_MANAGER]:
- `LightManagerSupport.PluginId() -> Guid` / `RenderEngineId() -> Guid` — the abstract identity pair a manager supplies.
- `LightManagerSupport.GetLights(RhinoDoc, ref LightArray)` / `LightFromId(RhinoDoc, Guid, ref Light) -> bool` / `ModifyLight(RhinoDoc, Light)` / `DeleteLight(RhinoDoc, Light, bool) -> bool` / `ObjectSerialNumberFromLight(RhinoDoc, ref Light) -> int` / `LightDescription(RhinoDoc, ref Light) -> string` — the abstract light-CRUD surface over `Rhino.Geometry.Light`.
- `LightManagerSupport.OnEditLight(RhinoDoc, ref LightArray) -> bool` / `GroupLights(RhinoDoc, ref LightArray)` / `UnGroup(RhinoDoc, ref LightArray)` — edit and grouping over the `LightArray` set.
- `LightManagerSupport.SetLightSolo(RhinoDoc, Guid, bool) -> bool` / `GetLightSolo(RhinoDoc, Guid) -> bool` / `LightsInSoloStorage(RhinoDoc) -> int` — virtual solo-storage support.
- `LightManagerSupport.OnCustomLightEvent(RhinoDoc, LightMangerSupportCustomEvent, ref Light)` — custom-event dispatch over the bounded event selector.
- `LightManagerSupport.RegisterLightManager(PlugIn)` / `RegisterProviders(Assembly, Guid)` — static registration by plug-in or assembly.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ChangeQueue` is the single scene-delivery owner: a subclass constructs it document- or preview-bound, overrides the `Apply*` hooks it consumes, and drives it with `CreateWorld`/`Flush`/`OneShot`; each hook receives an immutable native-backed payload the queue alone mints.
- Content resolves through the queue: a payload names a CRC or original-instance id, and `MaterialFromId`/`TextureFromOriginalInstanceId`/`EnvironmentForid` return the live `RenderContent`, never a content graph embedded in the payload.
- `RealtimeDisplayMode` is the interactive engine owner: it implements the render-size/start/shutdown lifecycle, draws through the `DisplayPipeline` its framebuffer and middleground events carry, and reads its own `MaxPasses`/`PostEffectsOn` progressive state.
- `AsyncRenderContext` owns the render thread: `StartRenderThread` spins it, `Cancel` gates it, `StopRendering` sets the flag, `JoinRenderThread` settles it.
- `LightManagerSupport` is the custom light source of truth over the document light table, answering it through `LightArray` with solo state riding its virtual solo-storage hooks.

[STACKING]:
- `LanguageExt.Core`(`../../.api/api-languageext.md`): a `ChangeQueue`/`AsyncRenderContext`/`LightArray` `IDisposable` rides a `using` bounded by `Eff`; a nullable content resolution lifts to `Option<RenderContent>`; a `StartRenderThread` or `DeleteLight` `bool` projects to `Fin<Unit>`; an `Apply*` hook payload list crosses as `Seq<A>`.
- `Thinktecture.Runtime.Extensions`(`../../.api/api-thinktecture-runtime-extensions.md`): `BakingFunctions`, `Light.Event`, `Environment.FrameBufferFillMode`, and `LightMangerSupportCustomEvent` map at the edge to `[SmartEnum]`/flag owners; the payload change discriminant collapses to one `[Union]` change owner keyed on the payload kind the `Apply*` hook dispatches.
- `api-rhinocommon-render.md`: `RealtimeDisplayMode.StartRenderer` and `AsyncRenderContext.RenderWindow` consume the `RenderWindow` the batch catalog owns, the realtime engine writing `Color4f` pixels through its channels.
- `api-rhinocommon-display.md`: framebuffer and middleground events carry a `DisplayPipeline` and its `DisplayPipelineAttributes`, and `RealtimeDisplayModeClassInfo.RequiredDisplayTechnology` names a `DisplayTechnology`; the engine draws through the display pipeline's state stacks.
- `api-rhinocommon-rendercontent.md`: `MaterialFromId`/`EnvironmentForid`/`TextureForId` resolve `RenderMaterial`/`RenderEnvironment`/`RenderTexture`, and `MeshInstance.Decals`/`RenderMaterial` and `MappingChannel.Mapping : TextureMapping` are content the content catalog owns.
- `api-rhinocommon-document.md` and `api-rhinocommon-geometry.md`: `ViewInfo`/`ViewportInfo`/`RhinoDoc` bind the queue and engine, and `Rhino.Geometry.Mesh`/`Light`/`Transform`/`Plane`/`Vector2d`/`BoundingBox` are the geometry carriers a payload exposes.

[LOCAL_ADMISSION]:
- A change enters through the subclass `Apply*` hook or a `GetQueue*` pull; a payload stays inside the hook grant, and downstream code receives a detached value record, a resolved content reference, or a projected receipt, never a live payload escaping the hook.

[RAIL_LAW]:
- Surface: `Rhino.Render.ChangeQueue` + `Rhino.Render` realtime slice
- Owns: the `ChangeQueue` scene-delivery owner and its immutable payload records, the `RealtimeDisplayMode`/`RealtimeDisplayModeClassInfo` interactive engine with its framebuffer/middleground/HUD hooks, `AsyncRenderContext` render-thread lifecycle, and `LightManagerSupport` custom light management.
- Accept: a subclass overriding the `Apply*` hooks it consumes, resolving content through the queue's CRC/id resolvers, and drawing through the display pipeline at the framebuffer seam; a registered realtime mode or light manager; host handles trapped through `Try.lift(...).Run()` and change/event enums mapped to bounded owners at the edge.
- Reject: a private scene diff beside `ChangeQueue`, a payload mutated or re-carried with its content graph, a batch render path merged into the realtime engine, a raw render thread beside `AsyncRenderContext`, a parallel light registry beside the registered manager, and a `ChangeQueue`/payload/`RealtimeDisplayMode` escaping into a domain signature.

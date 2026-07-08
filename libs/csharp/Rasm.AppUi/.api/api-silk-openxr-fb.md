# [RASM_APPUI_API_SILK_OPENXR_FB]

`Silk.NET.OpenXR.Extensions.FB` is the Meta/Facebook vendor-extension binding layered over the canonical `Silk.NET.OpenXR` core: the upstream `openxr.h` standard surface carries the cross-vendor `KHR`/`EXT` extensions, and the `FB_*` extensions — passthrough environment-blend, spatial-entity world-locked anchors, scene understanding, hand/body/face/eye tracking, foveation, color space, display refresh rate, render models, triangle mesh, and the composition-layer alpha/secure-content surfaces — live in this companion binding. Each `FB*` class (`FBPassthrough`, `FBSpatialEntity`, `FBScene`, `FBHandTrackingMesh`, ...) is a generated function-table root of instance methods returning `Result`; the descriptor structs, native handles, and flag/purpose enums they pointer-pass (`PassthroughCreateInfoFB`, `PassthroughLayerFB`, `SpaceComponentTypeFB`, `RoomLayoutFB`, `PassthroughFlagsFB`, ...) are declared in `Silk.NET.OpenXR` core, not in this assembly — the FB assembly carries only the entrypoint roots. The extension is enabled at instance create against the same `XR.GetApi()` instance `api-silk-openxr.md` owns and shares the host-installed OpenXR loader, so the env-blend passthrough composites on the very `Session`/`Swapchain` the `Wgpu` `GpuBackend` family already presents from — one GPU lifetime and one session across the rendered scene, the passthrough layer, and the world-locked anchor set.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.OpenXR.Extensions.FB`
- package: `Silk.NET.OpenXR.Extensions.FB`
- license: MIT (expression)
- assembly: `Silk.NET.OpenXR.Extensions.FB`
- namespace: `Silk.NET.OpenXR.Extensions.FB` (entrypoint roots only)
- struct/enum home: `Silk.NET.OpenXR` (all `*FB` descriptors, handles, flags, purpose/component enums)
- target: `lib/net5.0` is the highest TFM shipped; the `net10.0` consumer binds it (Silk.NET 2.x tops out at `net5.0`/`netstandard2.1`, no `net10.0` asset)
- asset: managed binding over the host-installed OpenXR loader (`libopenxr_loader`), no bundled native runtime
- depends: `Silk.NET.OpenXR`, `Silk.NET.Core`
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: extension function-table roots — every member is an instance method returning `Result`; each root has a paired `*Overloads` static-extension class and a private `_B` slot table
- rail: viewport

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                                          |
| :-----: | :----------------------------- | :----------------- | :---------------------------------------------- |
|  [01]   | `FBPassthrough`                | extension API root | `XR_FB_passthrough` + geometry-instance project |
|  [02]   | `FBSpatialEntity`              | extension API root | `XR_FB_spatial_entity` anchor create/component  |
|  [03]   | `FBSpatialEntityQuery`         | extension API root | persisted-anchor query/retrieve                 |
|  [04]   | `FBSpatialEntityStorage`       | extension API root | anchor save/erase to local/cloud store          |
|  [05]   | `FBSpatialEntityStorageBatch`  | extension API root | batched anchor save                             |
|  [06]   | `FBSpatialEntitySharing`       | extension API root | cross-user anchor share                         |
|  [07]   | `FBSpatialEntityContainer`     | extension API root | anchor-container membership                      |
|  [08]   | `FBSpatialEntityUser`          | extension API root | share-target user handle                         |
|  [09]   | `FBScene`                      | extension API root | room layout / semantic labels / bounds          |
|  [10]   | `FBSceneCapture`               | extension API root | trigger guardian scene capture                  |
|  [11]   | `FBHandTrackingMesh`           | extension API root | `XR_FB_hand_tracking_mesh`                       |
|  [12]   | `FBBodyTracking`               | extension API root | skeletal body-joint tracking                     |
|  [13]   | `FBFaceTracking` / `…2`        | extension API root | blendshape face-expression weights              |
|  [14]   | `FBEyeTrackingSocial`          | extension API root | social eye-gaze direction                        |
|  [15]   | `FBFoveation`                  | extension API root | fixed/eye-tracked foveation profile             |
|  [16]   | `FBColorSpace`                 | extension API root | output color-space enumerate/set                |
|  [17]   | `FBDisplayRefreshRate`         | extension API root | refresh-rate enumerate/get/request              |
|  [18]   | `FBRenderModel`                | extension API root | controller/device glTF render models            |
|  [19]   | `FBTriangleMesh`               | extension API root | mutable collision/passthrough mesh              |
|  [20]   | `FBSwapchainUpdateState`       | extension API root | per-frame swapchain state update                |
|  [21]   | `FBHapticPcm`                  | extension API root | PCM haptic device sample rate                    |
|  [22]   | `FBKeyboardTracking`           | extension API root | tracked physical-keyboard space                 |
|  [23]   | `FBPassthroughKeyboardHands`   | extension API root | hand-masked passthrough over keyboard           |

[PUBLIC_TYPE_SCOPE]: passthrough native handles, descriptor carriers, flags — all declared in `Silk.NET.OpenXR` core, pointer-passed into the `FBPassthrough` root
- rail: viewport

| [INDEX] | [SYMBOL]                          | [KIND]        | [RAIL]                                  |
| :-----: | :-------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `PassthroughFB`                   | native handle | passthrough feature handle              |
|  [02]   | `PassthroughLayerFB`              | native handle | passthrough composition layer           |
|  [03]   | `GeometryInstanceFB`              | native handle | passthrough-on-mesh projection instance |
|  [04]   | `PassthroughCreateInfoFB`         | descriptor    | passthrough create (flags)              |
|  [05]   | `PassthroughLayerCreateInfoFB`    | descriptor    | layer create (purpose, flags)           |
|  [06]   | `GeometryInstanceCreateInfoFB`    | descriptor    | mesh + base-space projection            |
|  [07]   | `GeometryInstanceTransformFB`     | descriptor    | per-frame instance pose/scale           |
|  [08]   | `CompositionLayerPassthroughFB`   | struct        | composition layer the frame submits     |
|  [09]   | `PassthroughStyleFB`              | descriptor    | edge color / texture-opacity style      |
|  [10]   | `PassthroughFlagsFB`              | enum (flags)  | `IsRunningAtCreationBitFB`              |
|  [11]   | `PassthroughLayerPurposeFB`       | enum          | `ReconstructionFB` / `ProjectedFB`      |

[PUBLIC_TYPE_SCOPE]: spatial-entity + scene carriers — core-declared, the anchor/scene-understanding surface the on-site review reads
- rail: viewport

| [INDEX] | [SYMBOL]                            | [KIND]     | [RAIL]                                |
| :-----: | :---------------------------------- | :--------- | :------------------------------------ |
|  [01]   | `SpatialAnchorCreateInfoFB`         | descriptor | anchor create at a `Posef` in a space |
|  [02]   | `SpaceComponentTypeFB`              | enum       | locatable/storable/sharable component |
|  [03]   | `SpaceComponentStatusFB`            | struct     | enabled/change-pending component state |
|  [04]   | `SpaceQueryInfoFB` / `…ResultFB`    | descriptor | persisted-anchor filter + result row  |
|  [05]   | `SpaceSaveInfoFB` / `SpaceEraseInfoFB` | descriptor | local/cloud persistence location   |
|  [06]   | `RoomLayoutFB`                      | struct     | floor/ceiling/wall anchor set         |
|  [07]   | `SemanticLabelsFB`                  | struct     | per-surface semantic label string     |
|  [08]   | `Rect2Df` / `Boundary2DFB`          | struct     | 2D surface boundary polygon           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: passthrough lifecycle, layer transport, style, and geometry-instance projection — every surface returns `Result`; Silk.NET ships 4 ptr/ref overloads of each pointer arg
- rail: viewport

| [INDEX] | [SURFACE]                                                                                       | [SURFACE_ROOT]  | [RAIL]              |
| :-----: | :---------------------------------------------------------------------------------------------- | :-------------- | :------------------ |
|  [01]   | `Result CreatePassthroughFB(Session, PassthroughCreateInfoFB*, PassthroughFB*)`                 | `FBPassthrough` | passthrough create  |
|  [02]   | `Result CreatePassthroughLayerFB(Session, PassthroughLayerCreateInfoFB*, PassthroughLayerFB*)`  | `FBPassthrough` | layer create        |
|  [03]   | `Result PassthroughStartFB(PassthroughFB)` / `PassthroughPauseFB(PassthroughFB)`                | `FBPassthrough` | feature start/pause |
|  [04]   | `Result PassthroughLayerResumeFB(PassthroughLayerFB)` / `PassthroughLayerPauseFB(…)`            | `FBPassthrough` | per-layer toggle    |
|  [05]   | `Result PassthroughLayerSetStyleFB(PassthroughLayerFB, PassthroughStyleFB*)`                    | `FBPassthrough` | edge/opacity style  |
|  [06]   | `Result CreateGeometryInstanceFB(Session, GeometryInstanceCreateInfoFB*, GeometryInstanceFB*)`  | `FBPassthrough` | passthrough-on-mesh |
|  [07]   | `Result GeometryInstanceSetTransformFB(GeometryInstanceFB, GeometryInstanceTransformFB*)`       | `FBPassthrough` | per-frame mesh pose |
|  [08]   | `Result DestroyPassthroughFB / DestroyPassthroughLayerFB / DestroyGeometryInstanceFB(handle)`   | `FBPassthrough` | native release      |

[ENTRYPOINT_SCOPE]: spatial-entity anchors and scene understanding — the persistent world-lock and real-world geometry the BIM on-site review consumes
- rail: viewport

| [INDEX] | [SURFACE]                                                                          | [SURFACE_ROOT]           | [RAIL]                       |
| :-----: | :--------------------------------------------------------------------------------- | :----------------------- | :--------------------------- |
|  [01]   | `Result CreateSpatialAnchorFB(Session, SpatialAnchorCreateInfoFB*, ulong* requestId)` | `FBSpatialEntity`    | mint world-locked anchor     |
|  [02]   | `Result GetSpaceUuidFB(Space, UuidEXT*)`                                            | `FBSpatialEntity`        | persistent anchor identity   |
|  [03]   | `Result EnumerateSpaceSupportedComponentsFB / GetSpaceComponentStatusFB / SetSpaceComponentStatusFB` | `FBSpatialEntity` | component enable/probe |
|  [04]   | `Result SaveSpaceFB(Session, SpaceSaveInfoFB*, ulong* requestId)` / `EraseSpaceFB(…)` | `FBSpatialEntityStorage` | persist/forget anchor      |
|  [05]   | `Result QuerySpacesFB(Session, SpaceQueryInfoBaseHeaderFB*, ulong* requestId)` / `RetrieveSpaceQueryResultsFB(…)` | `FBSpatialEntityQuery` | reload anchors next session |
|  [06]   | `Result ShareSpacesFB(Session, SpaceShareInfoFB*, ulong* requestId)`               | `FBSpatialEntitySharing` | co-review anchor share       |
|  [07]   | `Result GetSpaceRoomLayoutFB / GetSpaceSemanticLabelsFB / GetSpaceBoundingBox3Dfb / GetSpaceBoundary2Dfb` | `FBScene` | real-world geometry read |
|  [08]   | `Result RequestSceneCaptureFB(Session, SceneCaptureRequestInfoFB*, ulong* requestId)` | `FBSceneCapture`      | trigger room scan            |

[ENTRYPOINT_SCOPE]: render-comfort and device surfaces — refresh rate, foveation, color space, render models, hand/body tracking
- rail: viewport

| [INDEX] | [SURFACE]                                                                              | [SURFACE_ROOT]         | [RAIL]                      |
| :-----: | :------------------------------------------------------------------------------------- | :--------------------- | :-------------------------- |
|  [01]   | `Result EnumerateDisplayRefreshRatesFB / GetDisplayRefreshRateFB / RequestDisplayRefreshRateFB` | `FBDisplayRefreshRate` | adaptive refresh    |
|  [02]   | `Result CreateFoveationProfileFB(Session, FoveationProfileCreateInfoFB*, FoveationProfileFB*)` / `DestroyFoveationProfileFB(…)` | `FBFoveation` | fill-rate save |
|  [03]   | `Result EnumerateColorSpacesFB / SetColorSpaceFB(Session, ColorSpaceFB)`                | `FBColorSpace`         | output color management     |
|  [04]   | `Result EnumerateRenderModelPathsFB / GetRenderModelPropertiesFB / LoadRenderModelFB`   | `FBRenderModel`        | controller glTF             |
|  [05]   | `Result GetHandMeshFB(HandTrackerEXT, HandTrackingMeshFB*)`                              | `FBHandTrackingMesh`   | hand skinned mesh           |
|  [06]   | `Result CreateBodyTrackerFB / LocateBodyJointsFB / GetBodySkeletonFB / DestroyBodyTrackerFB` | `FBBodyTracking`   | skeletal pose               |
|  [07]   | `Result CreateTriangleMeshFB / TriangleMeshGetVertexBufferFB / TriangleMeshBeginUpdateFB / …EndUpdateFB` | `FBTriangleMesh` | mutable mesh upload  |

## [04]-[IMPLEMENTATION_LAW]

[FB_PASSTHROUGH_TOPOLOGY]:
- Each `FB*` root loads through the Silk.NET `TryGetExtension`/constructor convention against the live `XR` core instance — `new FBPassthrough(xr.Context)` (or `xr.TryGetInstanceExtension(out FBPassthrough fb)` against the created `Instance`) yields the function-table root, so every `FB_*` call is an instance method on that root returning `Result` and taking raw pointers to the core-declared `*FB` descriptor structs, never a managed wrapper object. The `*Overloads` companion classes are the Silk.NET extension-method projections (`Span<T>`/`ref` overloads) over the same entrypoints.
- Every `FB_*` extension is enabled at instance create by listing its name in the `InstanceCreateInfo.EnabledExtensionNames` chain exactly as the `KHR`/`EXT` extensions enable; the host runtime must advertise the extension (`xrEnumerateInstanceExtensionProperties`), so the immersive session probes per-extension and folds each missing capability to its degraded form — `XR_FB_passthrough` absent folds the env-blend case to the opaque `XR_ENVIRONMENT_BLEND_MODE_OPAQUE` flat composite, `XR_FB_spatial_entity` absent disables persistent world-lock and the anchors live only for the session.
- The passthrough lifecycle is `CreatePassthroughFB` (against the session, `IsRunningAtCreationBitFB` auto-starting the feed) -> `CreatePassthroughLayerFB` (`ReconstructionFB` for full-screen passthrough, `ProjectedFB` for surface-projected) -> the per-frame `CompositionLayerPassthroughFB` chained into the `EndFrame` layer array beneath the projection layer so the rendered BIM scene composites over the camera feed. `PassthroughLayerPauseFB`/`PassthroughLayerResumeFB` gate one layer without tearing the feature; `PassthroughLayerSetStyleFB` carries the `PassthroughStyleFB` edge-color and texture-opacity as a per-frame fold the session re-applies, never a re-created layer.
- `CreateGeometryInstanceFB` projects passthrough onto a `FBTriangleMesh` surface in a base space (the rejected form is a full-screen reconstruction layer where a windowed projected surface suffices), and `GeometryInstanceSetTransformFB` re-poses that instance each frame — the "passthrough porthole" cut into the virtual model is a geometry-instance, not a second layer.

[FB_SPATIAL_TOPOLOGY]:
- Spatial-entity anchors are the persistent world-lock the BIM site review pins the model to: `CreateSpatialAnchorFB` mints an anchor at a `Posef` in a reference space, `SetSpaceComponentStatusFB` enables the `Locatable`/`Storable`/`Sharable` components of `SpaceComponentTypeFB`, `SaveSpaceFB` persists it to local or cloud storage keyed by the `UuidEXT` from `GetSpaceUuidFB`, and the next session reloads it through `QuerySpacesFB`/`RetrieveSpaceQueryResultsFB` so the model returns to the exact physical position — the rejected form is re-registering the model origin by hand each visit. All anchor operations are async: each returns a `ulong` request id (Silk.NET's mapping of `XrAsyncRequestIdFB`) that the matching `EventDataSpace*CompleteFB` structure carries back on the event poll, never a blocking call.
- Scene understanding (`FBScene`) reads the runtime's guardian/room model: `GetSpaceRoomLayoutFB` yields the floor/ceiling/wall anchor set, `GetSpaceSemanticLabelsFB` the per-surface label, and `GetSpaceBoundingBox3Dfb`/`GetSpaceBoundary2Dfb` the real-world geometry the renderer occludes the virtual model against and the navigation tool clamps the user to — `RequestSceneCaptureFB` triggers a fresh room scan when none exists.
- `FBSpatialEntitySharing` + `FBSpatialEntityUser` is the co-review seam: a shared anchor uuid lets a second headset on the same site load the identical world-lock, so two reviewers see the model in one registered position.

[LOCAL_ADMISSION]:
- All native handles (`PassthroughFB`, `PassthroughLayerFB`, `GeometryInstanceFB`, `FoveationProfileFB`, anchor `Space`s, `BodyTrackerFB`) are released through their matching `DestroyXxxFB` native call, not `IDisposable` — the owning boundary capsule pairs create-and-destroy in a scoped fold exactly as the `api-silk-openxr.md` session/swapchain handles do.
- The package carries no bundled native runtime; it binds the host-installed OpenXR loader the `Silk.NET.OpenXR` core already loads, so the `Silk.NET.Core`/`Silk.NET.Maths` runtime is shared with the `Silk.NET.WebGPU` and `Silk.NET.OpenXR` families and no second native asset restores. The `FB_*` extensions are Meta-Quest-class capabilities the host runtime advertises only on Quest/Quest-Link hardware, so on a SteamVR/Varjo/Monado host the unsupported extensions fold to their degraded forms and on macOS the whole immersive surface folds to the flat viewport (no Apple OpenXR loader ships).
- The package ships in lockstep with the core: Silk.NET generates and publishes its entire package set — core plus every `KHR`/`EXT`/`FB`/`WGPU` extension — from one monorepo release, so `Silk.NET.OpenXR.Extensions.FB` rides the same `2.23.0` line as `Silk.NET.OpenXR`/`Silk.NET.WebGPU` over the shared `Silk.NET.Core` floor with no version split and no pin. The `net5.0` asset is the bound one for the `net10.0` consumer (Silk.NET 2.x ships no `net10.0`/`net8.0` lib).

[RAIL_LAW]:
- Package: `Silk.NET.OpenXR.Extensions.FB`
- Owns: the Meta/FB OpenXR vendor-extension binding — passthrough environment-blend (feature/layer/geometry-instance/style), spatial-entity world-locked anchors with local/cloud persistence and cross-user sharing, scene understanding (room layout, semantic labels, real-world bounds), foveation, color space, display-refresh-rate, render models, and body/hand/face/eye tracking — layered over the `api-silk-openxr.md` core session.
- Accept: `Result`-returning raw-pointer `*FB` calls on the extension function-table roots with the descriptor structs resolved from `Silk.NET.OpenXR` core; native-handle scoped create-and-destroy pairs at the boundary capsule; async anchor/scene operations retired through the `ulong` request id on the `EventDataSpace*CompleteFB` event poll; per-extension capability probe folding each missing `FB_*` to its degraded form.
- Reject: a managed convenience wrapper renaming the native surface; a second OpenXR session or instance for passthrough or anchors — every FB feature is created against the one session the `api-silk-openxr.md` core owns and the `Wgpu` `GpuBackend` device presents from, the FB layer chained into the same `EndFrame` layer array; a blocking wait on an anchor save/query where the async request id retires on the event poll; hand-registering the model origin each visit where a persisted spatial anchor world-locks it.

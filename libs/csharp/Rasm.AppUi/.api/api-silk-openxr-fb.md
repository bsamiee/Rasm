# [RASM_APPUI_API_SILK_OPENXR_FB]

`Silk.NET.OpenXR.Extensions.FB` layers Meta vendor entrypoints over canonical `Silk.NET.OpenXR`: generated `FB*` function-table roots return `Result` while pointer-passing descriptors, handles, flags, and purpose enums remain core declarations. Instance creation enables each extension against the same `XR.GetApi()` root and host OpenXR loader, so passthrough, spatial anchors, scene understanding, tracking, foveation, color, refresh, render-model, mesh, and composition surfaces share one `Session`/`Swapchain` with the `Wgpu` `GpuBackend` presentation rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.OpenXR.Extensions.FB`
- package: `Silk.NET.OpenXR.Extensions.FB` (MIT)
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

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]      | [RAIL]                                          |
| :-----: | :---------------------------- | :----------------- | :---------------------------------------------- |
|  [01]   | `FBPassthrough`               | extension API root | `XR_FB_passthrough` + geometry-instance project |
|  [02]   | `FBSpatialEntity`             | extension API root | `XR_FB_spatial_entity` anchor create/component  |
|  [03]   | `FBSpatialEntityQuery`        | extension API root | persisted-anchor query/retrieve                 |
|  [04]   | `FBSpatialEntityStorage`      | extension API root | anchor save/erase to local/cloud store          |
|  [05]   | `FBSpatialEntityStorageBatch` | extension API root | batched anchor save                             |
|  [06]   | `FBSpatialEntitySharing`      | extension API root | cross-user anchor share                         |
|  [07]   | `FBSpatialEntityContainer`    | extension API root | anchor-container membership                     |
|  [08]   | `FBSpatialEntityUser`         | extension API root | share-target user handle                        |
|  [09]   | `FBScene`                     | extension API root | room layout / semantic labels / bounds          |
|  [10]   | `FBSceneCapture`              | extension API root | trigger guardian scene capture                  |
|  [11]   | `FBHandTrackingMesh`          | extension API root | `XR_FB_hand_tracking_mesh`                      |
|  [12]   | `FBBodyTracking`              | extension API root | skeletal body-joint tracking                    |
|  [13]   | `FBFaceTracking` / `…2`       | extension API root | blendshape face-expression weights              |
|  [14]   | `FBEyeTrackingSocial`         | extension API root | social eye-gaze direction                       |
|  [15]   | `FBFoveation`                 | extension API root | fixed/eye-tracked foveation profile             |
|  [16]   | `FBColorSpace`                | extension API root | output color-space enumerate/set                |
|  [17]   | `FBDisplayRefreshRate`        | extension API root | refresh-rate enumerate/get/request              |
|  [18]   | `FBRenderModel`               | extension API root | controller/device glTF render models            |
|  [19]   | `FBTriangleMesh`              | extension API root | mutable collision/passthrough mesh              |
|  [20]   | `FBSwapchainUpdateState`      | extension API root | per-frame swapchain state update                |
|  [21]   | `FBHapticPcm`                 | extension API root | PCM haptic device sample rate                   |
|  [22]   | `FBKeyboardTracking`          | extension API root | tracked physical-keyboard space                 |
|  [23]   | `FBPassthroughKeyboardHands`  | extension API root | hand-masked passthrough over keyboard           |

[PUBLIC_TYPE_SCOPE]: passthrough native handles, descriptor carriers, flags — all declared in `Silk.NET.OpenXR` core, pointer-passed into the `FBPassthrough` root
- rail: viewport

| [INDEX] | [SYMBOL]                        | [KIND]        | [RAIL]                                  |
| :-----: | :------------------------------ | :------------ | :-------------------------------------- |
|  [01]   | `PassthroughFB`                 | native handle | passthrough feature handle              |
|  [02]   | `PassthroughLayerFB`            | native handle | passthrough composition layer           |
|  [03]   | `GeometryInstanceFB`            | native handle | passthrough-on-mesh projection instance |
|  [04]   | `PassthroughCreateInfoFB`       | descriptor    | passthrough create (flags)              |
|  [05]   | `PassthroughLayerCreateInfoFB`  | descriptor    | layer create (purpose, flags)           |
|  [06]   | `GeometryInstanceCreateInfoFB`  | descriptor    | mesh + base-space projection            |
|  [07]   | `GeometryInstanceTransformFB`   | descriptor    | per-frame instance pose/scale           |
|  [08]   | `CompositionLayerPassthroughFB` | struct        | composition layer the frame submits     |
|  [09]   | `PassthroughStyleFB`            | descriptor    | edge color / texture-opacity style      |
|  [10]   | `PassthroughFlagsFB`            | enum (flags)  | `IsRunningAtCreationBitFB`              |
|  [11]   | `PassthroughLayerPurposeFB`     | enum          | `ReconstructionFB` / `ProjectedFB`      |

[PUBLIC_TYPE_SCOPE]: spatial-entity + scene carriers — core-declared, the anchor/scene-understanding surface the on-site review reads
- rail: viewport

| [INDEX] | [SYMBOL]                               | [KIND]     | [RAIL]                                 |
| :-----: | :------------------------------------- | :--------- | :------------------------------------- |
|  [01]   | `SpatialAnchorCreateInfoFB`            | descriptor | anchor create at a `Posef` in a space  |
|  [02]   | `SpaceComponentTypeFB`                 | enum       | locatable/storable/sharable component  |
|  [03]   | `SpaceComponentStatusFB`               | struct     | enabled/change-pending component state |
|  [04]   | `SpaceQueryInfoFB` / `…ResultFB`       | descriptor | persisted-anchor filter + result row   |
|  [05]   | `SpaceSaveInfoFB` / `SpaceEraseInfoFB` | descriptor | local/cloud persistence location       |
|  [06]   | `RoomLayoutFB`                         | struct     | floor/ceiling/wall anchor set          |
|  [07]   | `SemanticLabelsFB`                     | struct     | per-surface semantic label string      |
|  [08]   | `Rect2Df` / `Boundary2DFB`             | struct     | 2D surface boundary polygon            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: passthrough lifecycle, layer transport, style, and geometry-instance projection — every surface returns `Result`; Silk.NET ships 4 ptr/ref overloads of each pointer arg
- rail: viewport

| [INDEX] | [SURFACE]                                                                                      | [SURFACE_ROOT]  | [RAIL]              |
| :-----: | :--------------------------------------------------------------------------------------------- | :-------------- | :------------------ |
|  [01]   | `Result CreatePassthroughFB(Session, PassthroughCreateInfoFB*, PassthroughFB*)`                | `FBPassthrough` | passthrough create  |
|  [02]   | `Result CreatePassthroughLayerFB(Session, PassthroughLayerCreateInfoFB*, PassthroughLayerFB*)` | `FBPassthrough` | layer create        |
|  [03]   | `Result PassthroughStartFB(PassthroughFB)` / `PassthroughPauseFB(PassthroughFB)`               | `FBPassthrough` | feature start/pause |
|  [04]   | `Result PassthroughLayerResumeFB(PassthroughLayerFB)` / `PassthroughLayerPauseFB(…)`           | `FBPassthrough` | per-layer toggle    |
|  [05]   | `Result PassthroughLayerSetStyleFB(PassthroughLayerFB, PassthroughStyleFB*)`                   | `FBPassthrough` | edge/opacity style  |
|  [06]   | `Result CreateGeometryInstanceFB(Session, GeometryInstanceCreateInfoFB*, GeometryInstanceFB*)` | `FBPassthrough` | passthrough-on-mesh |
|  [07]   | `Result GeometryInstanceSetTransformFB(GeometryInstanceFB, GeometryInstanceTransformFB*)`      | `FBPassthrough` | per-frame mesh pose |
|  [08]   | `Result DestroyPassthroughFB / DestroyPassthroughLayerFB / DestroyGeometryInstanceFB(handle)`  | `FBPassthrough` | native release      |

[ENTRYPOINT_SCOPE]: spatial-entity anchors and scene understanding — the persistent world-lock and real-world geometry the BIM on-site review consumes
- rail: viewport

| [INDEX] | [SURFACE]                             | [SURFACE_ROOT]           | [OPERATION]                 |
| :-----: | :------------------------------------ | :----------------------- | :-------------------------- |
|  [01]   | `CreateSpatialAnchorFB`               | `FBSpatialEntity`        | mint world-locked anchor    |
|  [02]   | `GetSpaceUuidFB`                      | `FBSpatialEntity`        | read persistent identity    |
|  [03]   | `EnumerateSpaceSupportedComponentsFB` | `FBSpatialEntity`        | enumerate components        |
|  [04]   | `GetSpaceComponentStatusFB`           | `FBSpatialEntity`        | read component state        |
|  [05]   | `SetSpaceComponentStatusFB`           | `FBSpatialEntity`        | change component state      |
|  [06]   | `SaveSpaceFB`                         | `FBSpatialEntityStorage` | persist anchor              |
|  [07]   | `EraseSpaceFB`                        | `FBSpatialEntityStorage` | forget anchor               |
|  [08]   | `QuerySpacesFB`                       | `FBSpatialEntityQuery`   | query persisted anchors     |
|  [09]   | `RetrieveSpaceQueryResultsFB`         | `FBSpatialEntityQuery`   | retrieve query results      |
|  [10]   | `ShareSpacesFB`                       | `FBSpatialEntitySharing` | share co-review anchor      |
|  [11]   | `GetSpaceRoomLayoutFB`                | `FBScene`                | read room layout            |
|  [12]   | `GetSpaceSemanticLabelsFB`            | `FBScene`                | read semantic labels        |
|  [13]   | `GetSpaceBoundingBox3Dfb`             | `FBScene`                | read three-dimensional box  |
|  [14]   | `GetSpaceBoundary2Dfb`                | `FBScene`                | read two-dimensional bounds |
|  [15]   | `RequestSceneCaptureFB`               | `FBSceneCapture`         | trigger room scan           |

[SIGNATURES]:
- `CreateSpatialAnchorFB`: `Result CreateSpatialAnchorFB(Session, SpatialAnchorCreateInfoFB*, ulong* requestId)`
- `GetSpaceUuidFB`: `Result GetSpaceUuidFB(Space, UuidEXT*)`
- `SaveSpaceFB`: `Result SaveSpaceFB(Session, SpaceSaveInfoFB*, ulong* requestId)`
- `EraseSpaceFB`: `Result EraseSpaceFB(…)`
- `QuerySpacesFB`: `Result QuerySpacesFB(Session, SpaceQueryInfoBaseHeaderFB*, ulong* requestId)`
- `RetrieveSpaceQueryResultsFB`: `Result RetrieveSpaceQueryResultsFB(…)`
- `ShareSpacesFB`: `Result ShareSpacesFB(Session, SpaceShareInfoFB*, ulong* requestId)`
- `RequestSceneCaptureFB`: `Result RequestSceneCaptureFB(Session, SceneCaptureRequestInfoFB*, ulong* requestId)`

[ENTRYPOINT_SCOPE]: render-comfort and device surfaces — refresh rate, foveation, color space, render models, hand/body tracking
- rail: viewport

| [INDEX] | [SURFACE]                        | [SURFACE_ROOT]         | [OPERATION]               |
| :-----: | :------------------------------- | :--------------------- | :------------------------ |
|  [01]   | `EnumerateDisplayRefreshRatesFB` | `FBDisplayRefreshRate` | enumerate refresh rates   |
|  [02]   | `GetDisplayRefreshRateFB`        | `FBDisplayRefreshRate` | read refresh rate         |
|  [03]   | `RequestDisplayRefreshRateFB`    | `FBDisplayRefreshRate` | request refresh rate      |
|  [04]   | `CreateFoveationProfileFB`       | `FBFoveation`          | create foveation profile  |
|  [05]   | `DestroyFoveationProfileFB`      | `FBFoveation`          | release foveation profile |
|  [06]   | `EnumerateColorSpacesFB`         | `FBColorSpace`         | enumerate color spaces    |
|  [07]   | `SetColorSpaceFB`                | `FBColorSpace`         | set output color space    |
|  [08]   | `EnumerateRenderModelPathsFB`    | `FBRenderModel`        | enumerate model paths     |
|  [09]   | `GetRenderModelPropertiesFB`     | `FBRenderModel`        | read model properties     |
|  [10]   | `LoadRenderModelFB`              | `FBRenderModel`        | load controller model     |
|  [11]   | `GetHandMeshFB`                  | `FBHandTrackingMesh`   | read skinned hand mesh    |
|  [12]   | `CreateBodyTrackerFB`            | `FBBodyTracking`       | create body tracker       |
|  [13]   | `LocateBodyJointsFB`             | `FBBodyTracking`       | locate body joints        |
|  [14]   | `GetBodySkeletonFB`              | `FBBodyTracking`       | read body skeleton        |
|  [15]   | `DestroyBodyTrackerFB`           | `FBBodyTracking`       | release body tracker      |
|  [16]   | `CreateTriangleMeshFB`           | `FBTriangleMesh`       | create mutable mesh       |
|  [17]   | `TriangleMeshGetVertexBufferFB`  | `FBTriangleMesh`       | access vertex buffer      |
|  [18]   | `TriangleMeshBeginUpdateFB`      | `FBTriangleMesh`       | begin mesh update         |
|  [19]   | `…EndUpdateFB`                   | `FBTriangleMesh`       | complete mesh update      |

[SIGNATURES]:
- `CreateFoveationProfileFB`: `Result CreateFoveationProfileFB(Session, FoveationProfileCreateInfoFB*, FoveationProfileFB*)`
- `DestroyFoveationProfileFB`: `Result DestroyFoveationProfileFB(…)`
- `SetColorSpaceFB`: `Result SetColorSpaceFB(Session, ColorSpaceFB)`
- `GetHandMeshFB`: `Result GetHandMeshFB(HandTrackerEXT, HandTrackingMeshFB*)`

## [04]-[IMPLEMENTATION_LAW]

[FB_PASSTHROUGH_TOPOLOGY]:
- Each `FB*` root loads against the live `XR` core instance through `new FBPassthrough(xr.Context)` or `xr.TryGetInstanceExtension(out FBPassthrough fb)` against the created `Instance`.
- Every `FB_*` call is an instance method on its function-table root, returns `Result`, and takes raw pointers to core-declared `*FB` descriptors. `*Overloads` classes project `Span<T>` and `ref` forms over the same entrypoints.
- Each `FB_*` extension name joins the `InstanceCreateInfo.EnabledExtensionNames` chain, and `xrEnumerateInstanceExtensionProperties` must advertise the extension.
- Session setup probes each extension independently. Missing `XR_FB_passthrough` folds environment blending to opaque `XR_ENVIRONMENT_BLEND_MODE_OPAQUE`, while missing `XR_FB_spatial_entity` keeps anchors session-local.
- Passthrough runs `CreatePassthroughFB` with optional `IsRunningAtCreationBitFB`, then `CreatePassthroughLayerFB` with `ReconstructionFB` or `ProjectedFB`, then chains `CompositionLayerPassthroughFB` beneath the projection layer in the `EndFrame` layer array.
- `PassthroughLayerPauseFB` and `PassthroughLayerResumeFB` toggle one layer without tearing down the feature. `PassthroughLayerSetStyleFB` reapplies the `PassthroughStyleFB` edge color and texture opacity without recreating the layer.
- `CreateGeometryInstanceFB` projects passthrough onto a `FBTriangleMesh` surface in a base space (the rejected form is a full-screen reconstruction layer where a windowed projected surface suffices), and `GeometryInstanceSetTransformFB` re-poses that instance each frame — the "passthrough porthole" cut into the virtual model is a geometry-instance, not a second layer.

[FB_SPATIAL_TOPOLOGY]:
- `CreateSpatialAnchorFB` mints the persistent world-lock at a `Posef` in a reference space, and `SetSpaceComponentStatusFB` activates its `Locatable`, `Storable`, and `Sharable` components.
- `SaveSpaceFB` persists the anchor by the `UuidEXT` from `GetSpaceUuidFB`; a later session restores physical registration through `QuerySpacesFB` and `RetrieveSpaceQueryResultsFB`.
- Anchor operations return a `ulong` request identifier mapped from `XrAsyncRequestIdFB`, and the matching `EventDataSpace*CompleteFB` event retires each request on the event poll.
- Scene understanding (`FBScene`) reads the runtime's guardian/room model: `GetSpaceRoomLayoutFB` yields the floor/ceiling/wall anchor set, `GetSpaceSemanticLabelsFB` the per-surface label, and `GetSpaceBoundingBox3Dfb`/`GetSpaceBoundary2Dfb` the real-world geometry the renderer occludes the virtual model against and the navigation tool clamps the user to — `RequestSceneCaptureFB` triggers a fresh room scan when none exists.
- `FBSpatialEntitySharing` + `FBSpatialEntityUser` is the co-review seam: a shared anchor uuid lets a second headset on the same site load the identical world-lock, so two reviewers see the model in one registered position.

[LOCAL_ADMISSION]:
- All native handles (`PassthroughFB`, `PassthroughLayerFB`, `GeometryInstanceFB`, `FoveationProfileFB`, anchor `Space`s, `BodyTrackerFB`) are released through their matching `DestroyXxxFB` native call, not `IDisposable` — the owning boundary capsule pairs create-and-destroy in a scoped fold exactly as the `api-silk-openxr.md` session/swapchain handles do.
- `Silk.NET.OpenXR.Extensions.FB` binds the host loader already loaded by `Silk.NET.OpenXR`; `Silk.NET.WebGPU` and `Silk.NET.OpenXR` share `Silk.NET.Core` and `Silk.NET.Maths` without restoring a second native asset.
- Quest and Quest Link runtimes advertise Meta `FB_*` capabilities. SteamVR, Varjo, and Monado fold unsupported extensions to degraded forms, while an absent macOS loader folds immersive output to the flat viewport.
- Silk.NET publishes core and every `KHR`/`EXT`/`FB`/`WGPU` extension in lockstep over one `Silk.NET.Core` floor; the consumer binds the available `net5.0` asset without a split package pin.

[RAIL_LAW]:
- Package: `Silk.NET.OpenXR.Extensions.FB`
- Owns: the Meta/FB OpenXR vendor-extension binding — passthrough environment-blend (feature/layer/geometry-instance/style), spatial-entity world-locked anchors with local/cloud persistence and cross-user sharing, scene understanding (room layout, semantic labels, real-world bounds), foveation, color space, display-refresh-rate, render models, and body/hand/face/eye tracking — layered over the `api-silk-openxr.md` core session.
- Accept: `Result`-returning raw-pointer `*FB` calls on the extension function-table roots with the descriptor structs resolved from `Silk.NET.OpenXR` core; native-handle scoped create-and-destroy pairs at the boundary capsule; async anchor/scene operations retired through the `ulong` request id on the `EventDataSpace*CompleteFB` event poll; per-extension capability probe folding each missing `FB_*` to its degraded form.
- Reject: a managed convenience wrapper renaming the native surface; a second OpenXR session or instance for passthrough or anchors — every FB feature is created against the one session the `api-silk-openxr.md` core owns and the `Wgpu` `GpuBackend` device presents from, the FB layer chained into the same `EndFrame` layer array; a blocking wait on an anchor save/query where the async request id retires on the event poll; hand-registering the model origin each visit where a persisted spatial anchor world-locks it.

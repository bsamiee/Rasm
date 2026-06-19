# [RASM_APPUI_API_SILK_OPENXR_FB]

`Silk.NET.OpenXR.Extensions.FB` is the Meta/Facebook vendor-extension binding layered over the canonical `Silk.NET.OpenXR` core: the upstream `openxr.h` standard surface carries the cross-vendor `KHR`/`EXT` extensions, and the `FB_*` extensions — passthrough environment-blend, hand-tracking mesh, spatial-entity anchors, and the composition-layer alpha/secure-content surfaces — live in this companion binding. The `FBPassthrough` class is the generated function-table root for the `XR_FB_passthrough` entrypoints the immersive on-site mixed-reality review negotiates: `CreatePassthroughFB` mints the passthrough feature against the session, `CreatePassthroughLayerFB` allocates the composition layer the env-blend pass composites under the rendered scene, and `PassthroughStartFB`/`PassthroughPauseFB` gate the camera feed. The extension is loaded against the same `XR.GetApi()` instance the `api-silk-openxr.md` core owns and shares the host-installed OpenXR loader, so the env-blend passthrough composites on the very `Session`/`Swapchain` the `Wgpu` `GpuBackend` family already presents from — one GPU lifetime and one session across the rendered scene and the passthrough layer.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.OpenXR.Extensions.FB`
- package: `Silk.NET.OpenXR.Extensions.FB`
- assembly: `Silk.NET.OpenXR.Extensions.FB`
- namespace: `Silk.NET.OpenXR.Extensions.FB`
- asset: managed binding over the host-installed OpenXR loader (`libopenxr_loader`), no bundled native runtime
- depends: `Silk.NET.OpenXR`, `Silk.NET.Core`
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: extension function-table roots
- rail: viewport

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]      | [RAIL]                          |
| :-----: | :----------------------------- | :----------------- | :------------------------------ |
|  [01]   | `FBPassthrough`                | extension API root | `XR_FB_passthrough` entrypoints |
|  [02]   | `FBHandTrackingMesh`           | extension API root | `XR_FB_hand_tracking_mesh`      |
|  [03]   | `FBSpatialEntity`              | extension API root | `XR_FB_spatial_entity` anchors  |
|  [04]   | `FBCompositionLayerAlphaBlend` | extension API root | composition-layer alpha blend   |

[PUBLIC_TYPE_SCOPE]: passthrough native handles and descriptor carriers
- rail: viewport

| [INDEX] | [SYMBOL]                        | [KIND]        | [RAIL]                              |
| :-----: | :------------------------------ | :------------ | :---------------------------------- |
|  [01]   | `PassthroughFB`                 | native handle | passthrough feature handle          |
|  [02]   | `PassthroughLayerFB`            | native handle | passthrough composition layer       |
|  [03]   | `PassthroughCreateInfoFB`       | descriptor    | passthrough create (flags)          |
|  [04]   | `PassthroughLayerCreateInfoFB`  | descriptor    | layer create (purpose, flags)       |
|  [05]   | `CompositionLayerPassthroughFB` | struct        | composition layer the frame submits |
|  [06]   | `PassthroughStyleFB`            | descriptor    | edge color / texture-opacity style  |
|  [07]   | `PassthroughFlagsFB`            | enum (flags)  | `IsRunningAtCreationBitFB`          |
|  [08]   | `PassthroughLayerPurposeFB`     | enum          | `ReconstructionFB` / `ProjectedFB`  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: passthrough lifecycle and composition
- rail: viewport

| [INDEX] | [SURFACE]                                                                               | [SURFACE_ROOT]  | [RAIL]              |
| :-----: | :-------------------------------------------------------------------------------------- | :-------------- | :------------------ |
|  [01]   | `CreatePassthroughFB(Session, PassthroughCreateInfoFB*, PassthroughFB*)`                | `FBPassthrough` | passthrough create  |
|  [02]   | `CreatePassthroughLayerFB(Session, PassthroughLayerCreateInfoFB*, PassthroughLayerFB*)` | `FBPassthrough` | layer create        |
|  [03]   | `PassthroughStartFB(PassthroughFB)`                                                     | `FBPassthrough` | start camera feed   |
|  [04]   | `PassthroughPauseFB(PassthroughFB)`                                                     | `FBPassthrough` | pause camera feed   |
|  [05]   | `PassthroughLayerSetStyleFB(PassthroughLayerFB, PassthroughStyleFB*)`                   | `FBPassthrough` | edge/opacity style  |
|  [06]   | `DestroyPassthroughFB(PassthroughFB)`                                                   | `FBPassthrough` | passthrough release |
|  [07]   | `DestroyPassthroughLayerFB(PassthroughLayerFB)`                                         | `FBPassthrough` | layer release       |

## [04]-[IMPLEMENTATION_LAW]

[FB_PASSTHROUGH_TOPOLOGY]:
- The extension loads through the Silk.NET `TryGetExtension`/constructor convention against the live `XR` core instance — `new FBPassthrough(xr.Context)` (or `xr.TryGetInstanceExtension(out FBPassthrough fb)` against the created `Instance`) yields the function-table root, so every `FB_*` call is an instance method on that root taking raw pointers to the `*FB` descriptor structs, never a managed wrapper object.
- `XR_FB_passthrough` is enabled at instance create by listing the extension name in the `InstanceCreateInfo.EnabledExtensionNames` chain exactly as the `KHR`/`EXT` extensions enable; the host runtime must advertise the extension (`xrEnumerateInstanceExtensionProperties`), so the immersive session probes for `XR_FB_passthrough` and folds the env-blend case to the opaque `XR_ENVIRONMENT_BLEND_MODE_OPAQUE` flat composite when the runtime lacks it.
- The passthrough lifecycle is `CreatePassthroughFB` (against the session, the `IsRunningAtCreationBitFB` flag auto-starting the feed) -> `CreatePassthroughLayerFB` (`ReconstructionFB` for full-screen passthrough, `ProjectedFB` for surface-projected) -> the per-frame `CompositionLayerPassthroughFB` chained into the `EndFrame` layer array beneath the projection layer so the rendered BIM scene composites over the camera feed.
- `PassthroughLayerSetStyleFB` carries the `PassthroughStyleFB` edge-color and texture-opacity so an on-site review tints or fades the real-world feed; the style update is a per-frame fold the immersive session re-applies, never a re-created layer.

[LOCAL_ADMISSION]:
- All native handles (`PassthroughFB`, `PassthroughLayerFB`) are released through their matching `DestroyPassthroughFB`/`DestroyPassthroughLayerFB` native call, not `IDisposable` — the owning boundary capsule pairs create-and-destroy in a scoped fold exactly as the `api-silk-openxr.md` session/swapchain handles do.
- The package carries no bundled native runtime; it binds the host-installed OpenXR loader the `Silk.NET.OpenXR` core already loads, so the `Silk.NET.Core`/`Silk.NET.Maths` runtime is shared with the `Silk.NET.WebGPU` and `Silk.NET.OpenXR` families and no second native asset restores.
- The package ships in lockstep with the core: Silk.NET generates and publishes its entire package set — core plus every `KHR`/`EXT`/`FB`/`WGPU` extension — from one monorepo release, so `Silk.NET.OpenXR.Extensions.FB` rides the same `2.23.0` line as `Silk.NET.OpenXR`/`Silk.NET.WebGPU` over the shared `Silk.NET.Core` floor with no version split and no pin.

[RAIL_LAW]:
- Package: `Silk.NET.OpenXR.Extensions.FB`
- Owns: the Meta/FB OpenXR vendor-extension binding — the passthrough environment-blend create/layer/start/style surface the immersive mixed-reality on-site review composites under the rendered scene, layered over the `api-silk-openxr.md` core session.
- Accept: raw-pointer `*FB` descriptor calls on the extension function-table root; native-handle scoped create-and-destroy pairs at the boundary capsule; the env-blend passthrough as one `immersive/xr-review.md` render-target case folding to the opaque flat composite when the runtime lacks the extension.
- Reject: a managed convenience wrapper renaming the native surface; a second OpenXR session or instance for passthrough — the passthrough feature is created against the one session the `api-silk-openxr.md` core owns and the `Wgpu` `GpuBackend` device presents from, the FB layer chained into the same `EndFrame` layer array.

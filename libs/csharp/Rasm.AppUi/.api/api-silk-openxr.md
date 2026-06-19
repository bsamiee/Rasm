# [RASM_APPUI_API_SILK_OPENXR]

`Silk.NET.OpenXR` is the managed OpenXR binding generated against the canonical `openxr.h` headers: `XR` is the static API entry exposing the global `CreateInstance`, an `Instance` enumerates a `SystemId`, a `Session` is created against a graphics-binding chain (the same `Device`/`Queue` the `Wgpu` `GpuBackend` family already owns, or a Vulkan/Metal/D3D12 binding), a `Swapchain` allocates the per-eye image array the stereo render targets bind, the frame loop runs `WaitFrame`/`BeginFrame`/`EndFrame` against a `CompositionLayerProjection`, and `xrLocateViews`/`xrLocateSpace` resolve the head and controller poses each frame. `Silk.NET.OpenXR.Extensions.KHR`/`EXT`/`FB` carry the vendor extension surface (`KHR_vulkan_enable2`, `KHR_composition_layer_depth`, `EXT_hand_tracking`, `FB_passthrough`) the immersive-review session negotiates at instance create. The binding shares the `Silk.NET.Core`/`Silk.NET.Maths` runtime the `Silk.NET.WebGPU` family already restores, so the `Wgpu` device the desktop viewport renders with is the same device the XR swapchain presents from — one GPU lifetime across the flat and immersive surfaces.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.OpenXR`
- package: `Silk.NET.OpenXR`
- package: `Silk.NET.OpenXR.Extensions.KHR`
- package: `Silk.NET.OpenXR.Extensions.EXT`
- assembly: `Silk.NET.OpenXR`
- namespace: `Silk.NET.OpenXR`
- namespace: `Silk.NET.OpenXR.Extensions.KHR`
- namespace: `Silk.NET.OpenXR.Extensions.EXT`
- asset: managed binding over the host-installed OpenXR loader (`libopenxr_loader`), no bundled native runtime
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: API root and session lifecycle owners
- rail: viewport

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY]   | [RAIL]                             |
| :-----: | :---------- | :-------------- | :--------------------------------- |
|  [01]   | `XR`        | static API root | global entry, function table       |
|  [02]   | `Instance`  | native handle   | runtime instance, extension query  |
|  [03]   | `Session`   | native handle   | graphics-bound XR session          |
|  [04]   | `Swapchain` | native handle   | per-eye swapchain image array      |
|  [05]   | `Space`     | native handle   | reference/action space for poses   |
|  [06]   | `ActionSet` | native handle   | input action collection            |
|  [07]   | `Action`    | native handle   | one bound input/pose/haptic action |

[PUBLIC_TYPE_SCOPE]: frame, view, and composition carriers
- rail: viewport

| [INDEX] | [SYMBOL]                         | [KIND]    | [RAIL]                                   |
| :-----: | :------------------------------- | :-------- | :--------------------------------------- |
|  [01]   | `SystemId`                       | id struct | resolved HMD system identity             |
|  [02]   | `View`                           | struct    | per-eye pose + field-of-view             |
|  [03]   | `ViewConfigurationType`          | enum      | stereo/mono/quad view config             |
|  [04]   | `Posef`                          | struct    | position quaternion + translation        |
|  [05]   | `Fovf`                           | struct    | asymmetric tangent field-of-view         |
|  [06]   | `FrameState`                     | struct    | predicted display time + should-render   |
|  [07]   | `CompositionLayerProjection`     | struct    | stereo projection layer                  |
|  [08]   | `CompositionLayerProjectionView` | struct    | per-eye layer view (swapchain sub-image) |
|  [09]   | `SwapchainImageVulkanKHR`        | struct    | imported swapchain image handle          |
|  [10]   | `EnvironmentBlendMode`           | enum      | opaque/additive/alpha-blend passthrough  |

[PUBLIC_TYPE_SCOPE]: descriptor and create-info carriers
- rail: viewport

| [INDEX] | [SYMBOL]                                            | [KIND]     | [RAIL]                                    |
| :-----: | :-------------------------------------------------- | :--------- | :---------------------------------------- |
|  [01]   | `InstanceCreateInfo`                                | descriptor | enabled extensions, app info              |
|  [02]   | `SystemGetInfo`                                     | descriptor | form-factor request                       |
|  [03]   | `SessionCreateInfo`                                 | descriptor | system id + graphics-binding `next` chain |
|  [04]   | `SwapchainCreateInfo`                               | descriptor | format, sample count, array size          |
|  [05]   | `ReferenceSpaceCreateInfo`                          | descriptor | stage/local/view reference space          |
|  [06]   | `ActionSetCreateInfo`                               | descriptor | action-set name + priority                |
|  [07]   | `ActionCreateInfo`                                  | descriptor | action name, type, subaction paths        |
|  [08]   | `FrameWaitInfo` / `FrameBeginInfo` / `FrameEndInfo` | descriptor | frame-loop carriers                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instance, system, and session creation
- rail: viewport

| [INDEX] | [SURFACE]                                                        | [SURFACE_ROOT] | [RAIL]                 |
| :-----: | :--------------------------------------------------------------- | :------------- | :--------------------- |
|  [01]   | `XR.GetApi()`                                                    | `XR`           | API root load          |
|  [02]   | `CreateInstance(InstanceCreateInfo*, Instance*)`                 | `XR`           | runtime instance       |
|  [03]   | `GetSystem(Instance, SystemGetInfo*, SystemId*)`                 | `XR`           | resolve HMD            |
|  [04]   | `EnumerateViewConfigurationViews(Instance, SystemId, type, ...)` | `XR`           | per-eye dimensions     |
|  [05]   | `CreateSession(Instance, SessionCreateInfo*, Session*)`          | `XR`           | graphics-bound session |
|  [06]   | `EnumerateInstanceExtensionProperties(layer, ...)`               | `XR`           | extension query        |

[ENTRYPOINT_SCOPE]: swapchain, space, and frame loop
- rail: viewport

| [INDEX] | [SURFACE]                                                                    | [SURFACE_ROOT] | [RAIL]                 |
| :-----: | :--------------------------------------------------------------------------- | :------------- | :--------------------- |
|  [01]   | `CreateSwapchain(Session, SwapchainCreateInfo*, Swapchain*)`                 | `XR`           | per-eye swapchain      |
|  [02]   | `EnumerateSwapchainImages(Swapchain, count, ..., SwapchainImageBaseHeader*)` | `XR`           | image array            |
|  [03]   | `AcquireSwapchainImage(Swapchain, SwapchainImageAcquireInfo*, index*)`       | `XR`           | acquire eye image      |
|  [04]   | `WaitSwapchainImage(Swapchain, SwapchainImageWaitInfo*)`                     | `XR`           | wait image ready       |
|  [05]   | `CreateReferenceSpace(Session, ReferenceSpaceCreateInfo*, Space*)`           | `XR`           | stage/local space      |
|  [06]   | `WaitFrame(Session, FrameWaitInfo*, FrameState*)`                            | `XR`           | predicted display time |
|  [07]   | `BeginFrame(Session, FrameBeginInfo*)`                                       | `XR`           | frame begin            |
|  [08]   | `LocateViews(Session, ViewLocateInfo*, ViewState*, count, ..., View*)`       | `XR`           | per-eye pose/fov       |
|  [09]   | `EndFrame(Session, FrameEndInfo*)`                                           | `XR`           | submit layers          |

[ENTRYPOINT_SCOPE]: input actions, poses, and haptics
- rail: viewport

| [INDEX] | [SURFACE]                                                            | [SURFACE_ROOT] | [RAIL]                  |
| :-----: | :------------------------------------------------------------------- | :------------- | :---------------------- |
|  [01]   | `CreateActionSet(Instance, ActionSetCreateInfo*, ActionSet*)`        | `XR`           | action set              |
|  [02]   | `CreateAction(ActionSet, ActionCreateInfo*, Action*)`                | `XR`           | bound action            |
|  [03]   | `SuggestInteractionProfileBindings(Instance, ...)`                   | `XR`           | controller profile bind |
|  [04]   | `SyncActions(Session, ActionsSyncInfo*)`                             | `XR`           | per-frame poll          |
|  [05]   | `GetActionStatePose / GetActionStateBoolean / GetActionStateFloat`   | `XR`           | controller state        |
|  [06]   | `LocateSpace(Space, baseSpace, time, SpaceLocation*)`                | `XR`           | controller pose         |
|  [07]   | `ApplyHapticFeedback(Session, HapticActionInfo*, HapticBaseHeader*)` | `XR`           | controller haptic       |

## [04]-[IMPLEMENTATION_LAW]

[OPENXR_TOPOLOGY]:
- `XR.GetApi()` returns the function-table root; every native call is an instance method on that `XR` object taking raw pointers to create-info structs — Silk.NET binds the C `openxr.h` surface directly, so a call site marshals `Span<T>`/`stackalloc` descriptor structs and passes pointers, never a managed wrapper object, exactly as the `Silk.NET.WebGPU` family does.
- The lifecycle is `Instance` (extensions enabled at create) -> `SystemId` (the resolved HMD) -> `Session` (created against the graphics-binding `next` chain) -> `Swapchain`s + reference `Space`s; the session runs the `WaitFrame`/`BeginFrame`/`LocateViews`/render-per-eye/`EndFrame` loop driven by the runtime-predicted display time, never a wall clock.
- The graphics binding is the seam to the `Wgpu` `GpuBackend` device: an OpenXR session created with the Vulkan binding (`KHR_vulkan_enable2`, `GraphicsBindingVulkanKHR`) shares the same physical device, queue family, and queue index the wgpu instance negotiated, so the meshlet/path-trace/splat passes render into the OpenXR swapchain images with the one device — a second GPU device for the immersive path is the cross-adapter copy penalty the shared binding avoids.
- `EndFrame` submits one `CompositionLayerProjection` per frame carrying two `CompositionLayerProjectionView` sub-images (left/right eye), each referencing a swapchain sub-image rectangle and the per-eye `Posef`+`Fovf` from `LocateViews`; `EnvironmentBlendMode` selects opaque VR, additive AR, or `FB_passthrough` mixed-reality compositing.
- Input is the action-set model: an `ActionSet` holds `Action`s bound to interaction-profile paths (`/user/hand/left/input/select/click`, `/user/hand/right/input/aim/pose`), `SyncActions` polls them per frame, and `GetActionStatePose`+`LocateSpace` resolves the controller pose the navigation and measurement tools read — a raw HID controller read is the rejected form because OpenXR owns the device abstraction.

[LOCAL_ADMISSION]:
- All native handles (`Instance`, `Session`, `Swapchain`, `Space`, `ActionSet`, `Action`) are released through their matching `DestroyXxx` native call, not `IDisposable` — the owning boundary capsule pairs create-and-destroy in a `using`-equivalent scoped fold exactly as the wgpu boundary pairs create-and-release.
- `Silk.NET.OpenXR` carries no bundled native runtime: it P/Invokes the host-installed OpenXR loader (`libopenxr_loader`, the runtime the headset vendor installs — SteamVR, Meta, Varjo, Monado), so the absence of an installed loader is the no-HMD floor that folds to the desktop flat viewport, never a hard fault — the immersive session is an optional surface the desktop path degrades from.
- The OpenXR loader is host-installed and macOS-absent today (no Apple OpenXR loader ships; visionOS uses ARKit/RealityKit, not OpenXR), so the immersive session activates on the Windows/Linux desktop hosts where the loader is present and folds to the flat viewport on macOS — the session create is a capability probe, not a launch precondition.

[RAIL_LAW]:
- Package: `Silk.NET.OpenXR` (+ `Silk.NET.OpenXR.Extensions.KHR`/`EXT`)
- Owns: the managed OpenXR binding — instance/system/session lifecycle, stereo swapchain allocation, reference-space and pose location, the predicted-display-time frame loop, the action-set input model, and environment-blend passthrough compositing for the immersive design-review surface.
- Accept: raw-pointer create-info calls on the `XR.GetApi()` function-table root; native-handle scoped create-and-destroy pairs; the shared graphics binding to the `Wgpu` device; the host-loader-absent fold to the flat viewport.
- Reject: a managed convenience wrapper renaming the native surface; a second GPU device for the immersive path; a raw HID controller read bypassing the action-set model; a wall-clock frame pace ignoring the runtime-predicted display time.

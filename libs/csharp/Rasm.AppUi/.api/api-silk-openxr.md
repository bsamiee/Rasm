# [RASM_APPUI_API_SILK_OPENXR]

`Silk.NET.OpenXR` binds the managed immersive-session lifecycle over the host-installed OpenXR loader — instance/session creation, stereo swapchain allocation, pose location, the runtime-paced frame loop, and the action-set input model — as pointer-passing calls on the `XR.GetApi()` function-table root. One `Wgpu` graphics-binding device backs both surfaces, and an absent loader folds the immersive session to the flat viewport, so the VR/AR review rail degrades to desktop rather than faults.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.OpenXR`
- package: `Silk.NET.OpenXR` + `Silk.NET.OpenXR.Extensions.KHR` + `Silk.NET.OpenXR.Extensions.EXT` (MIT)
- assembly: `Silk.NET.OpenXR`, `Silk.NET.OpenXR.Extensions.KHR`, `Silk.NET.OpenXR.Extensions.EXT`
- namespace: `Silk.NET.OpenXR`, `Silk.NET.OpenXR.Extensions.KHR`, `Silk.NET.OpenXR.Extensions.EXT`
- asset: managed binding over the host-installed OpenXR loader (`libopenxr_loader`), no bundled native runtime
- depends: `Silk.NET.Core`, `Silk.NET.Maths`
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: API root and session lifecycle owners

| [INDEX] | [SYMBOL]    | [TYPE_FAMILY]   | [CAPABILITY]                       |
| :-----: | :---------- | :-------------- | :--------------------------------- |
|  [01]   | `XR`        | static API root | global entry, function table       |
|  [02]   | `Instance`  | native handle   | runtime instance, extension query  |
|  [03]   | `Session`   | native handle   | graphics-bound XR session          |
|  [04]   | `Swapchain` | native handle   | per-eye swapchain image array      |
|  [05]   | `Space`     | native handle   | reference/action space for poses   |
|  [06]   | `ActionSet` | native handle   | input action collection            |
|  [07]   | `Action`    | native handle   | one bound input/pose/haptic action |

[PUBLIC_TYPE_SCOPE]: frame, view, and composition carriers

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                             |
| :-----: | :------------------------------- | :------------ | :--------------------------------------- |
|  [01]   | `SystemProperties`               | struct        | HMD name, vendor, graphics/tracking caps |
|  [02]   | `View`                           | struct        | per-eye pose + field-of-view             |
|  [03]   | `ViewConfigurationType`          | enum          | stereo/mono/quad view config             |
|  [04]   | `Posef`                          | struct        | position quaternion + translation        |
|  [05]   | `Fovf`                           | struct        | asymmetric tangent field-of-view         |
|  [06]   | `FrameState`                     | struct        | predicted display time + render flag     |
|  [07]   | `CompositionLayerProjection`     | struct        | stereo projection layer                  |
|  [08]   | `CompositionLayerProjectionView` | struct        | per-eye layer view (swapchain sub-image) |
|  [09]   | `SwapchainImageVulkanKHR`        | struct        | imported swapchain image handle          |
|  [10]   | `EnvironmentBlendMode`           | enum          | opaque/additive/alpha-blend passthrough  |

[PUBLIC_TYPE_SCOPE]: descriptor and create-info carriers

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------- | :------------ | :---------------------------------------- |
|  [01]   | `InstanceCreateInfo`                                | descriptor    | enabled extensions, app info              |
|  [02]   | `SystemGetInfo`                                     | descriptor    | form-factor request                       |
|  [03]   | `SessionCreateInfo`                                 | descriptor    | system id + graphics-binding `next` chain |
|  [04]   | `SwapchainCreateInfo`                               | descriptor    | format, sample count, array size          |
|  [05]   | `ReferenceSpaceCreateInfo`                          | descriptor    | stage/local/view reference space          |
|  [06]   | `ActionSetCreateInfo`                               | descriptor    | action-set name + priority                |
|  [07]   | `ActionCreateInfo`                                  | descriptor    | action name, type, subaction paths        |
|  [08]   | `FrameWaitInfo` / `FrameBeginInfo` / `FrameEndInfo` | descriptor    | frame-loop carriers                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instance, system, and session creation

Every surface is an `unsafe Result` instance method on the `XR.GetApi()` function-table root taking pointer create-info structs, each mirrored by `ref`/`ref readonly` managed overloads.

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]           |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `XR.GetApi()`                                                                        | static   | API root load          |
|  [02]   | `CreateInstance(InstanceCreateInfo) -> Instance`                                     | instance | runtime instance       |
|  [03]   | `GetSystem(Instance, SystemGetInfo) -> ulong`                                        | instance | resolve HMD system id  |
|  [04]   | `EnumerateViewConfigurationViews(Instance, ulong, type)`                             | instance | per-eye dimensions     |
|  [05]   | `CreateSession(Instance, SessionCreateInfo) -> Session`                              | instance | graphics-bound session |
|  [06]   | `EnumerateInstanceExtensionProperties(...)`                                          | instance | extension census       |
|  [07]   | `IsInstanceExtensionPresent(string, string) -> bool`                                 | instance | availability query     |
|  [08]   | `TryGetInstanceExtension<T : NativeExtension<XR>>(string?, Instance, out T) -> bool` | instance | typed extension load   |

[ENTRYPOINT_SCOPE]: swapchain, space, and frame loop

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]            |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `CreateSwapchain(Session, SwapchainCreateInfo) -> Swapchain`          | instance | per-eye swapchain       |
|  [02]   | `EnumerateSwapchainImages(Swapchain, SwapchainImageBaseHeader)`       | instance | image array             |
|  [03]   | `AcquireSwapchainImage(Swapchain, SwapchainImageAcquireInfo) -> uint` | instance | acquire eye image index |
|  [04]   | `WaitSwapchainImage(Swapchain, SwapchainImageWaitInfo)`               | instance | wait image ready        |
|  [05]   | `CreateReferenceSpace(Session, ReferenceSpaceCreateInfo) -> Space`    | instance | stage/local space       |
|  [06]   | `WaitFrame(Session, FrameWaitInfo) -> FrameState`                     | instance | predicted display time  |
|  [07]   | `BeginFrame(Session, FrameBeginInfo)`                                 | instance | frame begin             |
|  [08]   | `LocateViews(Session, ViewLocateInfo) -> View[]`                      | instance | per-eye pose/fov        |
|  [09]   | `EndFrame(Session, FrameEndInfo)`                                     | instance | submit layers           |

[ENTRYPOINT_SCOPE]: input actions, poses, and haptics

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]            |
| :-----: | :----------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `CreateActionSet(Instance, ActionSetCreateInfo) -> ActionSet`      | instance | action set              |
|  [02]   | `CreateAction(ActionSet, ActionCreateInfo) -> Action`              | instance | bound action            |
|  [03]   | `SuggestInteractionProfileBindings(Instance, ...)`                 | instance | controller profile bind |
|  [04]   | `SyncActions(Session, ActionsSyncInfo)`                            | instance | per-frame poll          |
|  [05]   | `GetActionStatePose / GetActionStateBoolean / GetActionStateFloat` | instance | controller state        |
|  [06]   | `LocateSpace(Space, Space, long) -> SpaceLocation`                 | instance | controller pose         |
|  [07]   | `ApplyHapticFeedback(Session, HapticActionInfo, HapticBaseHeader)` | instance | controller haptic       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every native call is a pointer-passing `unsafe Result` instance method on the `XR.GetApi()` root; a call site marshals `stackalloc`/`Span<T>` create-info structs and passes pointers, never a managed wrapper object.
- Lifecycle order binds: `Instance` (extensions enabled at create) -> system id -> `Session` (graphics-binding `next` chain) -> `Swapchain`s and reference `Space`s; the frame loop paces on the runtime-predicted `FrameState` display time, never a wall clock.
- `EndFrame` submits one `CompositionLayerProjection` of two `CompositionLayerProjectionView` sub-images, each carrying the per-eye `Posef` and `Fovf` from `LocateViews`; `EnvironmentBlendMode` selects opaque VR or additive AR.
- Input rides the action-set model: an `ActionSet` holds `Action`s bound to interaction-profile paths, `SyncActions` polls each frame, and `GetActionStatePose` with `LocateSpace` resolves the controller pose OpenXR owns the device abstraction for.
- Extension command sets load through `TryGetInstanceExtension<T>` once their `ExtensionName` is named in `InstanceCreateInfo.EnabledExtensionNames`; depth compositing chains as the `CompositionLayerDepthInfoKHR` struct, not a wrapped class.

[STACKING]:
- `api-silk-webgpu`(`.api/api-silk-webgpu.md`): the core `Device`/`Queue` is the physical device and queue family the Vulkan `GraphicsBindingVulkanKHR` binds into `SessionCreateInfo.next`, so the swapchain images present from the one wgpu device — a second immersive-path device is the cross-adapter copy penalty this seam avoids.
- `api-silk-webgpu-wgpu`(`.api/api-silk-webgpu-wgpu.md`): `Wgpu.InstanceEnumerateAdapters` supplies the adapter identity the OpenXR runtime LUID-matches, pinning session and compositor to one physical adapter.
- `api-silk-openxr-fb`(`.api/api-silk-openxr-fb.md`): the `FB*` passthrough and anchor roots load against this catalog's `XR.GetApi()` instance and composite their layer on the same `Session`/`Swapchain` the scene renders to.
- viewport boundary capsule: each native handle pairs its `Create`/`Destroy` in a scoped fold, and the session-create probe folds to the flat desktop viewport when the loader is absent.

[LOCAL_ADMISSION]:
- Native handles (`Instance`, `Session`, `Swapchain`, `Space`, `ActionSet`, `Action`) release through their matching `DestroyXxx` call, not `IDisposable`; the boundary capsule pairs create-and-destroy in a scoped fold.
- `Silk.NET.OpenXR` P/Invokes the host-installed OpenXR loader (`libopenxr_loader`, installed by the headset vendor runtime), so an absent loader is the no-HMD floor that folds to the flat desktop viewport and the session create is a capability probe, not a launch precondition.
- macOS ships no OpenXR loader (visionOS binds ARKit/RealityKit), so the immersive session activates on the Windows and Linux hosts and folds to the flat viewport on macOS.

[RAIL_LAW]:
- Package: `Silk.NET.OpenXR` (+ `Silk.NET.OpenXR.Extensions.KHR`/`EXT` command-sets)
- Owns: the managed OpenXR binding — instance/system/session lifecycle, stereo swapchain allocation, reference-space and pose location, the predicted-display-time frame loop, the action-set input model, and environment-blend passthrough compositing.
- Accept: raw-pointer create-info calls on the `XR.GetApi()` root; scoped native-handle create-and-destroy pairs; the shared graphics binding to the `Wgpu` device; the host-loader-absent fold to the flat viewport.
- Reject: a managed convenience wrapper renaming the native surface; a second GPU device for the immersive path; a raw HID controller read bypassing the action-set model; a wall-clock frame pace ignoring the runtime-predicted display time.

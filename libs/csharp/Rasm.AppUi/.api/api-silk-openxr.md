# [RASM_APPUI_API_SILK_OPENXR]

`Silk.NET.OpenXR` binds the managed immersive-session lifecycle over the host-installed OpenXR loader — instance/session creation, the runtime-driven session-state event queue, stereo swapchain allocation, pose location, the runtime-paced frame loop, and the action-set input model — as pointer-passing calls on the `XR.GetApi()` function-table root. One `Wgpu` graphics-binding device backs both surfaces, and an absent loader folds the immersive session to the flat viewport, so the VR/AR review rail degrades to desktop rather than faults.

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

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                                                        |
| :-----: | :------------------------------- | :------------ | :---------------------------------------------------------------------------------- |
|  [01]   | `SystemProperties`               | struct        | HMD name, vendor, graphics/tracking caps                                            |
|  [02]   | `View`                           | struct        | per-eye pose + field-of-view                                                        |
|  [03]   | `ViewConfigurationType`          | enum          | stereo/mono/quad view config                                                        |
|  [04]   | `ViewConfigurationView`          | struct        | `Recommended`/`Max` `ImageRect*` + `SwapchainSampleCount`                           |
|  [05]   | `ViewState`                      | struct        | `ViewStateFlags` orientation/position valid + tracked bits                          |
|  [06]   | `Posef`                          | struct        | position quaternion + translation                                                   |
|  [07]   | `Fovf`                           | struct        | asymmetric tangent field-of-view (`AngleLeft`/`Right`/`Up`/`Down`)                  |
|  [08]   | `FrameState`                     | struct        | `PredictedDisplayTime`/`PredictedDisplayPeriod`/`ShouldRender`                      |
|  [09]   | `CompositionLayerProjection`     | struct        | stereo projection layer (`LayerFlags`/`Space`/`ViewCount`/`Views`)                  |
|  [10]   | `CompositionLayerProjectionView` | struct        | per-eye layer view (`Pose`/`Fov`/`SubImage`)                                        |
|  [11]   | `SwapchainSubImage`              | struct        | `Swapchain` + `Rect2Di ImageRect` + `ImageArrayIndex`                               |
|  [12]   | `Rect2Di`                        | struct        | `Offset2Di` + `Extent2Di` image rectangle                                           |
|  [13]   | `CompositionLayerFlags`          | enum (flags)  | `BlendTextureSourceAlphaBit` / `UnpremultipliedAlphaBit`                            |
|  [14]   | `SwapchainImageVulkanKHR`        | struct        | imported swapchain image handle                                                     |
|  [15]   | `EnvironmentBlendMode`           | enum          | opaque/additive/alpha-blend passthrough                                             |
|  [16]   | `CompositionLayerQuad`           | struct        | world-anchored quad (`LayerFlags`/`Space`/`EyeVisibility`/`SubImage`/`Pose`/`Size`) |
|  [17]   | `Extent2Df`                      | struct        | `float Width` + `float Height`; the quad's metre extent                             |
|  [18]   | `EyeVisibility`                  | enum          | `Both` / `Left` / `Right` per-layer eye selection                                   |
|  [19]   | `ReferenceSpaceType`             | enum          | `View` / `Local` / `Stage` reference-space selection                                |

- `CompositionLayerQuad`: rides `StructureType.TypeCompositionLayerQuad`.

[PUBLIC_TYPE_SCOPE]: session-state and event-queue carriers — the runtime drives the session and the app answers on the event it dequeues

[SESSION_STATE]: `Unknown` `Idle` `Ready` `Synchronized` `Visible` `Focused` `Stopping` `LossPending` `Exiting`

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :----------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `SessionState`                       | enum          | session lifecycle the runtime drives                              |
|  [02]   | `EventDataBuffer`                    | struct        | 4000-byte `Varying` union the poll fills; `Type` selects the cast |
|  [03]   | `EventDataSessionStateChanged`       | struct        | `Session` + `SessionState State` + `long Time`                    |
|  [04]   | `EventDataInstanceLossPending`       | struct        | runtime revoking the instance                                     |
|  [05]   | `EventDataEventsLost`                | struct        | `uint LostEventCount` queue overflow                              |
|  [06]   | `EventDataInteractionProfileChanged` | struct        | bound interaction profile changed; actions re-resolve             |
|  [07]   | `StructureType`                      | enum          | `TypeEventData*` discriminant every event carries                 |

[PUBLIC_TYPE_SCOPE]: descriptor and create-info carriers

| [INDEX] | [SYMBOL]                                                        | [TYPE_FAMILY] | [CAPABILITY]                                          |
| :-----: | :-------------------------------------------------------------- | :------------ | :---------------------------------------------------- |
|  [01]   | `InstanceCreateInfo`                                            | descriptor    | enabled extensions, app info                          |
|  [02]   | `SystemGetInfo`                                                 | descriptor    | form-factor request                                   |
|  [03]   | `SessionCreateInfo`                                             | descriptor    | system id + graphics-binding `next` chain             |
|  [04]   | `SessionBeginInfo`                                              | descriptor    | `PrimaryViewConfigurationType` on the Ready answer    |
|  [05]   | `SwapchainCreateInfo`                                           | descriptor    | format, sample count, array size                      |
|  [06]   | `SwapchainImageAcquireInfo` / `…WaitInfo` / `…ReleaseInfo`      | descriptor    | per-image acquire/wait (`long Timeout`)/release       |
|  [07]   | `ReferenceSpaceCreateInfo`                                      | descriptor    | stage/local/view reference space                      |
|  [08]   | `ActionSetCreateInfo`                                           | descriptor    | `fixed byte ActionSetName[64]` + localized + priority |
|  [09]   | `ActionCreateInfo`                                              | descriptor    | `fixed byte ActionName[64]`, type, subaction paths    |
|  [10]   | `ActionSuggestedBinding` / `InteractionProfileSuggestedBinding` | descriptor    | one binding array PER interaction profile             |
|  [11]   | `SessionActionSetsAttachInfo`                                   | descriptor    | action sets sealed to the session before any sync     |
|  [12]   | `ActionSpaceCreateInfo` / `ActiveActionSet` / `ActionsSyncInfo` | descriptor    | pose space + per-frame sync set                       |
|  [13]   | `ViewLocateInfo`                                                | descriptor    | view config + display time + base space               |
|  [14]   | `FrameWaitInfo` / `FrameBeginInfo` / `FrameEndInfo`             | descriptor    | frame-loop carriers                                   |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: instance, system, and session creation

Every surface is an `unsafe Result` instance method on the `XR.GetApi()` function-table root taking pointer create-info structs, each mirrored by `ref`/`ref readonly` managed overloads. The generator DE-PLURALIZES a trailing plural in the native name, so `xrLocateViews` binds `LocateView`, `xrSyncActions` binds `SyncAction`, `xrSuggestInteractionProfileBindings` binds `SuggestInteractionProfileBinding`, `xrEnumerateViewConfigurationViews` binds `EnumerateViewConfigurationView`, and `xrGetActionStateVector2f` binds `GetActionStateVector2` — the native spelling never resolves and the singular is the only member.

| [INDEX] | [SURFACE]                                                                            | [SHAPE]  | [CAPABILITY]           |
| :-----: | :----------------------------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `XR.GetApi()`                                                                        | static   | API root load          |
|  [02]   | `CreateInstance(InstanceCreateInfo*, Instance*)`                                     | instance | runtime instance       |
|  [03]   | `GetSystem(Instance, SystemGetInfo*, ulong* systemId)`                               | instance | resolve HMD system id  |
|  [04]   | `EnumerateViewConfigurationView(Instance, ulong, type, ...)`                         | instance | per-eye dimensions     |
|  [05]   | `EnumerateEnvironmentBlendModes(Instance, ulong, type, ...)`                         | instance | advertised blend set   |
|  [06]   | `CreateSession(Instance, SessionCreateInfo*, Session*)`                              | instance | graphics-bound session |
|  [07]   | `EnumerateInstanceExtensionProperties(...)`                                          | instance | extension census       |
|  [08]   | `IsInstanceExtensionPresent(string?, string) -> bool`                                | instance | availability query     |
|  [09]   | `TryGetInstanceExtension<T : NativeExtension<XR>>(string?, Instance, out T) -> bool` | instance | typed extension load   |

[ENTRYPOINT_SCOPE]: session state and the event queue — the runtime drives `SessionState` and the app answers here; no other surface reaches a rendering state

| [INDEX] | [SURFACE]                                               | [SHAPE]  | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------ | :------- | :------------------------------------------------------ |
|  [01]   | `PollEvent(Instance, EventDataBuffer* eventData)`       | instance | dequeue one event; `Result.EventUnavailable` ends drain |
|  [02]   | `BeginSession(Session, SessionBeginInfo*)`              | instance | the app's answer to `SessionState.Ready`                |
|  [03]   | `EndSession(Session)`                                   | instance | the app's answer to `SessionState.Stopping`             |
|  [04]   | `RequestExitSession(Session)`                           | instance | app-initiated exit; runtime drives to `Exiting`         |
|  [05]   | `DestroySession(Session)` / `DestroyInstance(Instance)` | instance | terminal native release                                 |

[ENTRYPOINT_SCOPE]: swapchain, space, and frame loop

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]              |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :------------------------ |
|  [01]   | `CreateSwapchain(Session, SwapchainCreateInfo*, Swapchain*)`                          | instance | per-eye swapchain         |
|  [02]   | `EnumerateSwapchainImages(Swapchain, SwapchainImageBaseHeader*)`                      | instance | image array               |
|  [03]   | `AcquireSwapchainImage(Swapchain, SwapchainImageAcquireInfo*, uint* index)`           | instance | acquire eye image index   |
|  [04]   | `WaitSwapchainImage(Swapchain, SwapchainImageWaitInfo*)`                              | instance | wait image ready          |
|  [05]   | `ReleaseSwapchainImage(Swapchain, SwapchainImageReleaseInfo*)`                        | instance | return the acquired image |
|  [06]   | `CreateReferenceSpace(Session, ReferenceSpaceCreateInfo*, Space*)`                    | instance | stage/local space         |
|  [07]   | `WaitFrame(Session, FrameWaitInfo*, FrameState*)`                                     | instance | predicted display time    |
|  [08]   | `BeginFrame(Session, FrameBeginInfo*)`                                                | instance | frame begin               |
|  [09]   | `LocateView(Session, ViewLocateInfo*, ViewState*, uint capacity, uint* count, View*)` | instance | per-eye pose/fov          |
|  [10]   | `EndFrame(Session, FrameEndInfo*)`                                                    | instance | submit layers             |

[ENTRYPOINT_SCOPE]: input actions, poses, and haptics

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `CreateActionSet(Instance, ActionSetCreateInfo*, ActionSet*)`                     | instance | action set                           |
|  [02]   | `CreateAction(ActionSet, ActionCreateInfo*, Action*)`                             | instance | bound action                         |
|  [03]   | `StringToPath(Instance, byte* pathString, ulong* path)`                           | instance | interaction-profile + component path |
|  [04]   | `SuggestInteractionProfileBinding(Instance, InteractionProfileSuggestedBinding*)` | instance | one binding array per profile        |
|  [05]   | `AttachSessionActionSets(Session, SessionActionSetsAttachInfo*)`                  | instance | seal sets to session before sync     |
|  [06]   | `CreateActionSpace(Session, ActionSpaceCreateInfo*, Space*)`                      | instance | pose action space                    |
|  [07]   | `SyncAction(Session, ActionsSyncInfo*)`                                           | instance | per-frame poll                       |
|  [08]   | `GetActionState{Pose, Boolean, Float, Vector2}`                                   | instance | controller state                     |
|  [09]   | `LocateSpace(Space, Space baseSpace, long time, SpaceLocation*)`                  | instance | controller pose                      |
|  [10]   | `LocateSpaces(Session, SpacesLocateInfo*, SpaceLocations*)`                       | instance | batched pose location                |
|  [11]   | `ApplyHapticFeedback(Session, HapticActionInfo*, HapticBaseHeader*)`              | instance | controller haptic                    |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every native call is a pointer-passing `unsafe Result` instance method on the `XR.GetApi()` root; a call site marshals `stackalloc`/`Span<T>` create-info structs and passes pointers, never a managed wrapper object.
- Lifecycle order binds: `Instance` (extensions enabled at create) -> system id (a bare `ulong`, no wrapper type) -> `Session` (graphics-binding `next` chain) -> `Swapchain`s and reference `Space`s; the frame loop paces on the runtime-predicted `FrameState` display time, never a wall clock.
- The session is a RUNTIME-DRIVEN state machine and the event queue is its only channel: `PollEvent` drains until `Result.EventUnavailable`, an `EventDataSessionStateChanged` carrying `SessionState.Ready` obliges `BeginSession`, `Stopping` obliges `EndSession`, and `LossPending`/`Exiting` are terminal. `BeginFrame` is refused until the app has answered `Ready`, so a frame loop with no drain leaves a constructed session permanently unrenderable.
- `Result.ErrorInstanceLost` and `Result.ErrorSessionLost` are the TERMINAL loss pair a retriability posture reads — neither ever recovers on the same handle, so `XrSurface.Posture` classifies both `Terminal` while throttle-class refusals (`ErrorLimitReached`-family) ride `Transient`.
- `FrameState.ShouldRender` gates the eye passes, not the frame pair: a zero still runs `BeginFrame`/`EndFrame` with a zero-length layer array so the runtime keeps pacing.
- Each eye image brackets `AcquireSwapchainImage` -> `WaitSwapchainImage` -> render -> `ReleaseSwapchainImage`; an acquired image never released strands that swapchain index.
- `EndFrame` submits one `CompositionLayerProjection` of two `CompositionLayerProjectionView` sub-images, each carrying the per-eye `Posef` and `Fovf` from `LocateView`; `EnvironmentBlendMode` selects opaque VR or additive AR, and a non-opaque blend needs `CompositionLayerFlags.BlendTextureSourceAlphaBit` on the layer for its alpha to reach the compositor.
- `Fovf` is four asymmetric tangent angles, so a single-scalar field-of-view camera cannot carry an eye frustum; the projection consumes the four angles directly.
- Input rides the action-set model: an `ActionSet` holds `Action`s whose component paths are suggested PER interaction profile through one `InteractionProfileSuggestedBinding` array, `AttachSessionActionSets` seals the set to the session before any sync, `SyncAction` polls each frame, and `GetActionStatePose` with `LocateSpace` resolves the controller pose OpenXR owns the device abstraction for.
- Extension command sets load through `TryGetInstanceExtension<T>` once their `ExtensionName` is named in `InstanceCreateInfo.EnabledExtensionNames`; depth compositing chains as the `CompositionLayerDepthInfoKHR` struct, not a wrapped class.

[STACKING]:
- `api-silk-webgpu`(`libs/csharp/.api/api-silk-webgpu.md`): the core `Device`/`Queue` is the physical device and queue family the Vulkan `GraphicsBindingVulkanKHR` binds into `SessionCreateInfo.next`, so the swapchain images present from the one wgpu device — a second immersive-path device is the cross-adapter copy penalty this seam avoids.
- `api-silk-webgpu-wgpu`(`libs/csharp/.api/api-silk-webgpu-wgpu.md`): `Wgpu.InstanceEnumerateAdapters` supplies the adapter identity the OpenXR runtime LUID-matches, pinning session and compositor to one physical adapter.
- `api-silk-openxr-fb`(`.api/api-silk-openxr-fb.md`): the `FB*` passthrough and anchor roots load against this catalog's `XR.GetApi()` instance and composite their layer on the same `Session`/`Swapchain` the scene renders to; this catalog's `PollEvent` drain is the ONE point at which the FB `ulong` async request ledger retires, so the anchor and scene surfaces cannot land without the session-state machine above them.
- viewport boundary capsule: each native handle pairs its `Create`/`Destroy` in a scoped fold, the per-frame event drain publishes the session state the frame loop gates on, and the session-create probe folds to the flat desktop viewport when the loader is absent.

[LOCAL_ADMISSION]:
- Native handles (`Instance`, `Session`, `Swapchain`, `Space`, `ActionSet`, `Action`) release through their matching `DestroyXxx` call, not `IDisposable`; the boundary capsule pairs create-and-destroy in a scoped fold. Handles acquired AFTER session create — action sets, actions, action spaces, FB features and profiles — need a ledger the capsule can append to, not a construction-time column.
- `Silk.NET.OpenXR` P/Invokes the host-installed OpenXR loader (`libopenxr_loader`, installed by the headset vendor runtime), so an absent loader is the no-HMD floor that folds to the flat desktop viewport and the session create is a capability probe, not a launch precondition.
- macOS ships no OpenXR loader (visionOS binds ARKit/RealityKit), so the immersive session activates on the Windows and Linux hosts and folds to the flat viewport on macOS.

[RAIL_LAW]:
- Package: `Silk.NET.OpenXR` (+ `Silk.NET.OpenXR.Extensions.KHR`/`EXT` command-sets)
- Owns: the managed OpenXR binding — instance/system/session lifecycle, the runtime-driven session-state event queue, stereo swapchain allocation, reference-space and pose location, the predicted-display-time frame loop, the action-set input model, and environment-blend passthrough compositing.
- Accept: raw-pointer create-info calls on the `XR.GetApi()` root; scoped native-handle create-and-destroy pairs against an appendable ledger; a per-frame `PollEvent` drain ahead of `WaitFrame`; the shared graphics binding to the `Wgpu` device; the host-loader-absent fold to the flat viewport.
- Reject: a managed convenience wrapper renaming the native surface; the native plural spelling where the generator emits the singular; a second GPU device for the immersive path; a raw HID controller read bypassing the action-set model; a suggested binding never attached through `AttachSessionActionSets`; a symmetric single-angle camera standing in for the asymmetric `Fovf`; a wall-clock frame pace ignoring the runtime-predicted display time; a frame loop with no event drain, which never reaches a rendering state.

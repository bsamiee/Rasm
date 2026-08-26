# [RASM_APPUI_API_AVALONIA_GPU_INTEROP]

`Avalonia`'s compositor owns the render-thread scene: `ICompositionGpuInterop` imports an externally-rendered GPU texture and its fence so a wgpu, D3D, Vulkan, or Metal backend composites without a second swapchain, `ElementComposition` attaches the surface visual to a control on the visuals pipeline, and the animation surface drives a visual's transform, opacity, and color off the UI thread through key-frame, expression, and implicit animations.

## [01]-[PUBLIC_TYPES]

[COMPOSITION_TYPES]: compositor, visual tree, and GPU-interop owners (`Avalonia.Rendering.Composition`)

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]  | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `Compositor`                                             | class          | composition-factory root                                   |
|  [02]   | `CompositionObject`                                      | abstract class | animation host + `ImplicitAnimations` slot                 |
|  [03]   | `CompositionVisual`                                      | abstract class | animatable transform/opacity node                          |
|  [04]   | `CompositionContainerVisual`                             | class          | child-visual host                                          |
|  [05]   | `CompositionSurfaceVisual`                               | class          | surface-backed visual node                                 |
|  [06]   | `CompositionSolidColorVisual`                            | class          | animatable `Color` fill node                               |
|  [07]   | `CompositionCustomVisual`                                | sealed class   | handler-driven visual + `SendHandlerMessage`               |
|  [08]   | `CompositionCustomVisualHandler`                         | abstract class | render-thread draw + animation-frame callback              |
|  [09]   | `ElementComposition`                                     | static class   | element-to-visual bridge                                   |
|  [10]   | `ICompositionGpuInterop`                                 | interface      | external GPU image/semaphore import                        |
|  [11]   | `CompositionDrawingSurface`                              | sealed class   | imported-image target surface                              |
|  [12]   | `CompositionSurface`                                     | abstract class | drawing-surface base                                       |
|  [13]   | `ICompositionGpuImportedObject`                          | interface      | imported-handle lifetime base                              |
|  [14]   | `ICompositionImportedGpuImage`                           | interface      | imported texture handle                                    |
|  [15]   | `ICompositionImportedGpuSemaphore`                       | interface      | imported fence handle                                      |
|  [16]   | `CompositionGpuImportedImageSynchronizationCapabilities` | flags enum     | `Semaphores`/`KeyedMutex`/`Automatic`/`TimelineSemaphores` |

[ANIMATION_TYPES]: the composition animation vocabulary (`Avalonia.Rendering.Composition.Animations`)

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY]  | [CAPABILITY]                                              |
| :-----: | :---------------------------- | :------------- | :-------------------------------------------------------- |
|  [01]   | `ICompositionAnimationBase`   | interface      | the start/stop argument both an animation and a group is  |
|  [02]   | `CompositionAnimation`        | abstract class | `Target` plus the typed parameter bag                     |
|  [03]   | `KeyFrameAnimation`           | abstract class | timing, direction, iteration, stop policy                 |
|  [04]   | `ExpressionAnimation`         | sealed class   | expression-driven continuous animation                    |
|  [05]   | `CompositionAnimationGroup`   | class          | animations started and stopped as one unit                |
|  [06]   | `ImplicitAnimationCollection` | sealed class   | property-name keyed trigger map                           |
|  [07]   | `AnimationDelayBehavior`      | enum           | `SetInitialValueAfterDelay`/`SetInitialValueBeforeDelay`  |
|  [08]   | `AnimationIterationBehavior`  | enum           | `Count`/`Forever`                                         |
|  [09]   | `AnimationStopBehavior`       | enum           | `LeaveCurrentValue`/`SetToInitialValue`/`SetToFinalValue` |
|  [10]   | `PlaybackDirection`           | enum           | `Normal`/`Reverse`/`Alternate`/`AlternateReverse`         |

[KEYFRAME_TYPES]: the ten typed `KeyFrameAnimation` subclasses, each with `InsertKeyFrame(float, T)` and an `IEasing` overload

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY] | [CAPABILITY]                        |
| :-----: | :--------------------------------------------------------- | :------------ | :---------------------------------- |
|  [01]   | `ScalarKeyFrameAnimation` / `DoubleKeyFrameAnimation`      | class         | `float` / `double` key frames       |
|  [02]   | `BooleanKeyFrameAnimation` / `ColorKeyFrameAnimation`      | class         | `bool` / `Color` key frames         |
|  [03]   | `VectorKeyFrameAnimation` / `Vector2KeyFrameAnimation`     | class         | `Vector` / `Vector2` key frames     |
|  [04]   | `Vector3KeyFrameAnimation` / `Vector3DKeyFrameAnimation`   | class         | `Vector3` / `Vector3D` key frames   |
|  [05]   | `Vector4KeyFrameAnimation` / `QuaternionKeyFrameAnimation` | class         | `Vector4` / `Quaternion` key frames |

[PLATFORM_TYPES]: external-image and handle vocabulary (`Avalonia.Platform`)

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------- | :------------ | :------------------------------------ |
|  [01]   | `IPlatformHandle`                                   | interface     | shared-resource handle carrier        |
|  [02]   | `PlatformGraphicsExternalImageProperties`           | record struct | external-image memory/format layout   |
|  [03]   | `PlatformGraphicsExternalImageFormat`               | enum          | `R8G8B8A8UNorm`/`B8G8R8A8UNorm` order |
|  [04]   | `KnownPlatformGraphicsExternalImageHandleTypes`     | static class  | image handle-type constants           |
|  [05]   | `KnownPlatformGraphicsExternalSemaphoreHandleTypes` | static class  | semaphore handle-type constants       |

## [02]-[ENTRYPOINTS]

[COMPOSITOR_ACCESS]: compositor acquisition, interop query, and the node factories

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :---------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `ElementComposition.GetElementVisual(Visual)`                           | static   | backing visual, null until in a render tree   |
|  [02]   | `ElementComposition.SetElementChildVisual(Visual, CompositionVisual?)`  | static   | attach a visual as the element's last child   |
|  [03]   | `ElementComposition.GetElementChildVisual(Visual)`                      | static   | previously-attached `CompositionVisual?`      |
|  [04]   | `CompositionVisual.Compositor`                                          | property | owning `Compositor`                           |
|  [05]   | `Compositor.TryGetDefaultCompositor()`                                  | static   | process `Compositor?`                         |
|  [06]   | `Compositor.TryGetCompositionGpuInterop()`                              | instance | `ValueTask<ICompositionGpuInterop?>` query    |
|  [07]   | `Compositor.CreateDrawingSurface()`                                     | factory  | `CompositionDrawingSurface` import target     |
|  [08]   | `Compositor.CreateSurfaceVisual()`                                      | factory  | `CompositionSurfaceVisual` surface node       |
|  [09]   | `Compositor.CreateContainerVisual()`                                    | factory  | `CompositionContainerVisual` transform parent |
|  [10]   | `Compositor.CreateSolidColorVisual()`                                   | factory  | `CompositionSolidColorVisual` fill node       |
|  [11]   | `Compositor.CreateCustomVisual(CompositionCustomVisualHandler)`         | factory  | handler-driven `CompositionCustomVisual`      |
|  [12]   | `Compositor.CreateCompositionVisualSnapshot(CompositionVisual, double)` | instance | `Task<Bitmap>` render of a subtree at scaling |
|  [13]   | `Compositor.RequestCompositionUpdate(Action)`                           | instance | pre-commit callback on the compositor loop    |
|  [14]   | `Compositor.RequestCommitAsync()`                                       | instance | `Task` completing on the next committed batch |

[VISUAL_STATE]: the animatable `CompositionVisual` slots — each name is also its animation and implicit-trigger key

| [INDEX] | [SURFACE]                             | [SHAPE]  | [CAPABILITY]                                           |
| :-----: | :------------------------------------ | :------- | :----------------------------------------------------- |
|  [01]   | `Visible` (`bool`)                    | property | subtree visibility                                     |
|  [02]   | `Opacity` (`float`)                   | property | node opacity folded into the subtree                   |
|  [03]   | `Offset` / `Translation` (`Vector3D`) | property | two summed translation slots                           |
|  [04]   | `Scale` (`Vector3D`)                  | property | scale about `CenterPoint`                              |
|  [05]   | `CenterPoint` (`Vector3D`)            | property | pivot for scale, rotation, and orientation             |
|  [06]   | `RotationAngle` (`float`)             | property | radians about Z, applied at `CenterPoint`              |
|  [07]   | `Orientation` (`Quaternion`)          | property | 3D rotation applied after `RotationAngle`              |
|  [08]   | `Size` (`Vector`)                     | property | the extent `AnchorPoint` and render bounds key on      |
|  [09]   | `AnchorPoint` (`Vector`)              | property | origin as a fraction of `Size`                         |
|  [10]   | `ClipToBounds` (`bool`)               | property | clip the subtree to `Size`                             |
|  [11]   | `OpacityMask` (`IBrush?`)             | property | brush-driven per-pixel opacity                         |
|  [12]   | `RenderOptions` / `TextOptions`       | property | subtree rendering and text-rendering policy            |
|  [13]   | `CompositionSurfaceVisual.Surface`    | property | `CompositionSurface?` slot the imported frame lands in |
|  [14]   | `CompositionSolidColorVisual.Color`   | property | `Color` fill, animatable and implicitly triggerable    |
|  [15]   | `CompositionContainerVisual.Children` | property | `CompositionVisualCollection` child list               |

- Composition-level `Clip`, `TransformMatrix`, `Effect`, and `CacheMode` are internal — the public surface animates transform, opacity, and color, and every visual effect renders inside a custom visual's own draw.
- Setting any slot cancels a running animation on that same slot before the implicit lookup, and the implicit trigger fires only when the assigned value differs from the current one.

[CUSTOM_VISUAL]: the render-thread handler a `CompositionCustomVisual` drives

| [INDEX] | [SURFACE]                               | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :-------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `OnRender(ImmediateDrawingContext)`     | instance | the one draw callback, abstract                       |
|  [02]   | `OnMessage(object)`                     | instance | receives `CompositionCustomVisual.SendHandlerMessage` |
|  [03]   | `OnAnimationFrameUpdate()`              | instance | per-frame tick after registration                     |
|  [04]   | `GetRenderBounds() -> Rect`             | instance | dirty bounds, defaulting to `EffectiveSize`           |
|  [05]   | `EffectiveSize` (`Vector`)              | property | the visual's `Size` as the compositor sees it         |
|  [06]   | `CompositionNow` (`TimeSpan`)           | property | the compositor's server clock for the frame           |
|  [07]   | `Invalidate()` / `Invalidate(Rect)`     | instance | mark the whole visual or one rect dirty               |
|  [08]   | `RegisterForNextAnimationFrameUpdate()` | instance | arm exactly one `OnAnimationFrameUpdate`              |
|  [09]   | `RenderClipContains(Point)`             | instance | point against the current transformed clip            |
|  [10]   | `RenderClipIntersectes(Rect)`           | instance | rect against the current transformed clip             |

- `EffectiveSize`, `CompositionNow`, `Invalidate`, and `RegisterForNextAnimationFrameUpdate` are `protected` and throw `InvalidOperationException` before the handler attaches to a compositor; the two `RenderClip*` probes throw outside `OnRender`.
- `RegisterForNextAnimationFrameUpdate` arms a single frame — a continuous motion re-arms it from inside `OnAnimationFrameUpdate`.

[ANIMATION_CONTROL]: starting and stopping an animation (`CompositionObject`)

| [INDEX] | [SURFACE]                                             | [SHAPE]  | [CAPABILITY]                                      |
| :-----: | :---------------------------------------------------- | :------- | :------------------------------------------------ |
|  [01]   | `StartAnimation(string, CompositionAnimation)`        | instance | bind an animation to one property name and run it |
|  [02]   | `StopAnimation(string)`                               | instance | detach the animation from that property           |
|  [03]   | `StartAnimationGroup(ICompositionAnimationBase)`      | instance | start every member against its own `Target`       |
|  [04]   | `StopAnimationGroup(ICompositionAnimationBase)`       | instance | stop every member against its own `Target`        |
|  [05]   | `ImplicitAnimations` (`ImplicitAnimationCollection?`) | property | trigger map every property setter consults        |

- `StartAnimation` throws `ArgumentException` on a property name the object does not declare, admitting the `[VISUAL_STATE]` names alone.
- Group members carry the property name in `CompositionAnimation.Target`, and a null `Target` throws `ArgumentException` at start and stop.

[IMPLICIT_TRIGGERS]: the trigger map and the group, both keyed on property names

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `ImplicitAnimationCollection.Insert(string, ICompositionAnimationBase)` | instance | UWP-shaped add beside `IDictionary.Add` |
|  [02]   | `ImplicitAnimationCollection.Lookup(string)`                            | instance | UWP-shaped read, null when absent       |
|  [03]   | `ImplicitAnimationCollection.HasKey(string)`                            | instance | UWP-shaped probe                        |
|  [04]   | `ImplicitAnimationCollection.Size` (`uint`)                             | property | UWP-shaped count beside `Count`         |
|  [05]   | `ImplicitAnimationCollection.GetView()`                                 | instance | read-only dictionary view of the map    |
|  [06]   | `CompositionAnimationGroup.Add` / `Remove` / `RemoveAll`                | instance | group membership                        |

- `ImplicitAnimationCollection` implements `IDictionary<string, ICompositionAnimationBase>` alongside the UWP-shaped members, and a triggering setter passes the newly-assigned value in as the animation's final value.

[ANIMATION_AUTHORING]: animation construction and timing

| [INDEX] | [SURFACE]                                                            | [SHAPE]  | [CAPABILITY]                               |
| :-----: | :------------------------------------------------------------------- | :------- | :----------------------------------------- |
|  [01]   | `Compositor.Create{Scalar,...,Quaternion}KeyFrameAnimation()`        | factory  | one factory per `[KEYFRAME_TYPES]` row     |
|  [02]   | `Compositor.CreateExpressionAnimation()` / `(string)`                | factory  | expression optional up front               |
|  [03]   | `Compositor.CreateAnimationGroup()`                                  | factory  | empty `CompositionAnimationGroup`          |
|  [04]   | `Compositor.CreateImplicitAnimationCollection()`                     | factory  | empty `ImplicitAnimationCollection`        |
|  [05]   | `CompositionAnimation.Target` (`string?`)                            | property | property name a group member drives        |
|  [06]   | `ExpressionAnimation.Expression` (`string?`)                         | property | expression evaluated every frame           |
|  [07]   | `InsertKeyFrame(float, T)` on every typed subclass                   | instance | value key frame at progress 0.0-1.0        |
|  [08]   | `InsertKeyFrame(float, T, IEasing)`                                  | instance | same, under an explicit easing             |
|  [09]   | `KeyFrameAnimation.InsertExpressionKeyFrame(float, string, Easing?)` | instance | expression-valued key frame                |
|  [10]   | `KeyFrameAnimation.Duration` (`TimeSpan`)                            | property | run length, 1ms floor                      |
|  [11]   | `KeyFrameAnimation.DelayTime` / `DelayBehavior`                      | property | start delay and its initial-value policy   |
|  [12]   | `KeyFrameAnimation.Direction`                                        | property | `PlaybackDirection` playback order         |
|  [13]   | `KeyFrameAnimation.IterationBehavior` / `IterationCount`             | property | loop policy and count, defaulting to one   |
|  [14]   | `KeyFrameAnimation.StopBehavior`                                     | property | value left behind when the animation stops |

[ANIMATION_PARAMETERS]: expression-parameter binding, every member on `CompositionAnimation`

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `Set{Scalar,Vector2,Vector3,Vector4}Parameter(string, T)`       | instance | numeric expression parameter           |
|  [02]   | `Set{Color,Quaternion,Matrix3x2,Matrix4x4}Parameter(string, T)` | instance | typed expression parameter             |
|  [03]   | `SetReferenceParameter(string, CompositionObject)`              | instance | bind a live object into the expression |
|  [04]   | `ClearParameter(string)` / `ClearAllParameters()`               | instance | drop bound parameters                  |

- `KeyFrameAnimation.Duration`: its setter validates the field it overwrites instead of the incoming value, so one `TimeSpan.Zero` assignment lands silently and the next assignment of any value throws `ArgumentException`; clamp every duration to at least 1ms and the trap never arms. Its upper bound is one day whatever the message claims.
- `ExpressionAnimation.Expression`: parses lazily on first start, so a malformed expression surfaces at `StartAnimation`, never at assignment.
- `Offset` and `Scale` are `Vector3D`, so `CreateVector3DKeyFrameAnimation` drives them — the `Vector3` variant targets neither.
- `InsertExpressionKeyFrame` takes a nullable `Easing` and falls back to the compositor default; the typed `InsertKeyFrame` overload takes a non-null `IEasing`.
- `InsertKeyFrame` is declared per typed subclass and NOT on `KeyFrameAnimation`, so a base-typed animation inserts value frames only after a downcast; `InsertExpressionKeyFrame` is the base's own member and is therefore the one type-agnostic frame.
- Namespace split: `CompositionAnimation`, `KeyFrameAnimation`, `ExpressionAnimation`, `CompositionAnimationGroup`, and `ImplicitAnimationCollection` live in `Avalonia.Rendering.Composition.Animations` while every typed `*KeyFrameAnimation` subclass lives in `Avalonia.Rendering.Composition` beside the visuals.
- Expression keywords: `this.StartingValue` is the animated property's value when the run starts and `this.FinalValue` the value it ends on — in an `ImplicitAnimationCollection` that is the value the triggering assignment carried in, and it falls back to the starting value when no final value is supplied. A trigger animation therefore authors both endpoints as expression frames and needs no literal value, which makes one body cover every slot.
- `ImplicitAnimationCollection` exposes a settable `this[string]` indexer beside `Insert`/`Lookup`/`HasKey`/`Size`, so the dictionary spelling and the UWP-shaped set address the same inner map.

[GPU_IMPORT]: external image/semaphore import and sync-capability query (`ICompositionGpuInterop`)

| [INDEX] | [SURFACE]                                                               | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `ImportImage(IPlatformHandle, PlatformGraphicsExternalImageProperties)` | instance | -> `ICompositionImportedGpuImage`           |
|  [02]   | `ImportImage(ICompositionImportableSharedGpuContextImage)`              | instance | same-context image import                   |
|  [03]   | `ImportSemaphore(IPlatformHandle)`                                      | instance | -> `ICompositionImportedGpuSemaphore`       |
|  [04]   | `ImportSemaphore(ICompositionImportableSharedGpuContextSemaphore)`      | instance | same-context semaphore import               |
|  [05]   | `GetSynchronizationCapabilities(string)`                                | instance | update-mode flags for one image handle type |

- `ImportSemaphore(ICompositionImportableSharedGpuContextSemaphore)`: returns `ICompositionImportedGpuImage`, not a semaphore handle.
- Both context-overloads: on import failure the caller retains ownership of the handle and must destroy it.
- `ICompositionGpuInterop` is `[NotClientImplementable]` — the compositor is its only implementer.

[SURFACE_UPDATE]: synchronization-discriminated per-frame refresh (`CompositionDrawingSurface`)

Every `UpdateWith*Async` takes the imported `ICompositionImportedGpuImage` first and returns a `Task` completing when the render thread releases the image; each row carries only its distinguishing synchronization argument.

| [INDEX] | [SURFACE]                                | [SHAPE]  | [CAPABILITY]                                             |
| :-----: | :--------------------------------------- | :------- | :------------------------------------------------------- |
|  [01]   | `UpdateWithKeyedMutexAsync`              | instance | `+ uint acquire, uint release` — D3D11 keyed mutex       |
|  [02]   | `UpdateWithSemaphoresAsync`              | instance | `+ wait, signal` binary semaphores — Vulkan/D3D12        |
|  [03]   | `UpdateWithTimelineSemaphoresAsync`      | instance | `+ wait/waitValue, signal/signalValue` — timeline        |
|  [04]   | `UpdateAsync`                            | instance | image only — platform-managed sync (Metal `IOSurface`)   |
|  [05]   | `Dispose` / `~CompositionDrawingSurface` | instance | teardown; the finalizer posts disposal to the dispatcher |

[INTEROP_LIFETIME]: capability properties and imported-handle lifetime (`ICompositionGpuInterop` / `ICompositionGpuImportedObject`)

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                               |
| :-----: | :---------------------------------------------- | :------- | :--------------------------------------------------------- |
|  [01]   | `DeviceLuid`                                    | property | `byte[]?` adapter LUID (D3D match), settable to pin import |
|  [02]   | `DeviceUuid`                                    | property | `byte[]?` adapter UUID (Vulkan match), equally settable    |
|  [03]   | `SupportedImageHandleTypes`                     | property | `IReadOnlyList<string>` importable image-handle kinds      |
|  [04]   | `SupportedSemaphoreTypes`                       | property | `IReadOnlyList<string>` importable semaphore-handle kinds  |
|  [05]   | `ICompositionGpuInterop.IsLost`                 | property | `bool` interop device-context loss                         |
|  [06]   | `ICompositionGpuImportedObject.ImportCompleted` | property | `Task` gating the free of a non-owning source              |
|  [07]   | `ICompositionGpuImportedObject.IsLost`          | property | `bool` per-imported-object device-context loss             |

[EXTERNAL_IMAGE_SHAPE]: imported-image memory window and format (`PlatformGraphicsExternalImageProperties`)

| [INDEX] | [SURFACE]         | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :---------------- | :------- | :-------------------------------------------------- |
|  [01]   | `Width`, `Height` | property | `int` imported texture extent                       |
|  [02]   | `Format`          | property | `PlatformGraphicsExternalImageFormat` channel order |
|  [03]   | `MemorySize`      | property | `ulong` external-memory allocation size (Vulkan)    |
|  [04]   | `MemoryOffset`    | property | `ulong` image offset within the allocation          |
|  [05]   | `TopLeftOrigin`   | property | `bool`; false flips the sampled frame               |

[HANDLE_TYPE_CONSTANTS]: importable handle identifiers; each is a `const string` equal to its own name, gated by `SupportedImageHandleTypes`/`SupportedSemaphoreTypes`.

| [INDEX] | [CONSTANT]                                       | [CAPABILITY]                                                    |
| :-----: | :----------------------------------------------- | :-------------------------------------------------------------- |
|  [01]   | `D3D11TextureGlobalSharedHandle`                 | image; DXGI `GetSharedHandle` global (non-owning) D3D11 texture |
|  [02]   | `D3D11TextureNtHandle`                           | image; DXGI `CreateSharedHandle` NT-handle D3D11 texture        |
|  [03]   | `VulkanOpaquePosixFileDescriptor`                | image + semaphore; Vulkan `OPAQUE_FD`                           |
|  [04]   | `VulkanOpaqueNtHandle` / `VulkanOpaqueKmtHandle` | image + semaphore; Vulkan `OPAQUE_WIN32` / KMT                  |
|  [05]   | `IOSurfaceRef`                                   | image; Metal `IOSurface` (automatic-sync)                       |
|  [06]   | `Direct3D12FenceNtHandle`                        | semaphore; D3D12/D3D11 fence shared NT handle (timeline)        |
|  [07]   | `MetalSharedEvent`                               | semaphore; `MTLSharedEvent` pointer (Metal timeline)            |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `ElementComposition.GetElementVisual(this)` yields the control's backing visual and its `Compositor`, returning null until the control enters a render tree, so import defers to the first composition update.
- `Compositor.TryGetCompositionGpuInterop()` yields the `ICompositionGpuInterop` capability; a null or `IsLost` result folds to the `Software` Skia-raster path.
- Adapter identity matches through `DeviceLuid`/`DeviceUuid`: the external device pins these byte arrays, and a mismatch forces a cross-adapter copy.
- Capsule construction mints the node once — `CreateDrawingSurface()` and `CreateSurfaceVisual()` — binds `CompositionSurfaceVisual.Surface` (change-tracked, render-thread serialized), and attaches through `SetElementChildVisual`, which throws across compositor instances.
- External backend rendering targets a shared texture (D3D11 keyed-mutex handle, Vulkan external `VkImage`, or Metal `IOSurface`) exported as one `SupportedImageHandleTypes` string; `ImportImage` binds it as `ICompositionImportedGpuImage` reading the memory window and channel order from `PlatformGraphicsExternalImageProperties`, and `ImportSemaphore` binds the matching fence.
- `GetSynchronizationCapabilities(imageHandleType)` selects the update member — `KeyedMutex` picks `UpdateWithKeyedMutexAsync`, `Semaphores` picks `UpdateWithSemaphoresAsync`, `TimelineSemaphores` picks `UpdateWithTimelineSemaphoresAsync`, `Automatic` picks `UpdateAsync` — and each `Task` completes when the render thread releases the image for reuse or disposal.
- `MatrixUtils.ComputeTransform` composes the visual transform in fixed order: anchor-point shift of `-Size * AnchorPoint`, then scale, then Z rotation, then the orientation quaternion — each pivoted on `CenterPoint` — and finally the `Offset + Translation` sum, so `Size` lands before `AnchorPoint` and `GetRenderBounds` carry meaning.
- One property name keys both animation surfaces: `StartAnimation("Offset", a)` and `ImplicitAnimations["Offset"]` address the same slot off one string vocabulary, so a typo silently no-ops the implicit path while throwing on the explicit one.
- Explicit and implicit animation compete for a slot and the last write wins: assigning the property drops the running animation, and starting an animation overrides the assigned value until it stops under its `StopBehavior`.
- Composition animation reaches transform, opacity, and color only; every visual effect, mask, backdrop, or shape animates by redrawing inside `CompositionCustomVisualHandler.OnRender`, ticking through `RegisterForNextAnimationFrameUpdate` against `CompositionNow`.
- `CompositionEffectBrush`, `CompositionBackdropBrush`, `CompositionMaskBrush`, `CompositionSpriteShape`, `SpriteVisual`, and `InsetClip` exist nowhere in this assembly — no brush, clip, or shape type hangs off `Compositor`.
- `Compositor.CreateCompositionVisualSnapshot(visual, scaling)` renders a live subtree and its children to a `Bitmap` through a posted server job, the one readback path out of the composition tree; it throws when the visual belongs to another compositor or sits outside a render tree.

[STACKING]:
- `api-silk-webgpu`(`libs/dotnet/.api/api-silk-webgpu.md`) / `api-silk-webgpu-wgpu`(`libs/dotnet/.api/api-silk-webgpu-wgpu.md`): the `Wgpu` backend renders through `WebGPU.GetApi()` -> instance/adapter/device with the adapter matched to this interop's `DeviceLuid`/`DeviceUuid`; the rendered `Texture` exports as the platform handle `ImportImage` binds, and its export-format synchronization primitive selects the matching `UpdateWith*Async` member.
- `api-avalonia-skia`(`api-avalonia-skia.md`) / `api-skiasharp`(`api-skiasharp.md`): `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` shares Avalonia's own `GRContext` for the Skia-Ganesh families, while this interop imports an independently-rendered texture for the `Wgpu` family; the two are mutually exclusive `GpuBackend` rows and both fold to `Software` when their query returns null. `CompositionCustomVisualHandler.OnRender` hands its `ImmediateDrawingContext` to `TryGetFeature<ISkiaSharpApiLeaseFeature>()` for a composition-thread Skia draw.
- within-lib: the AppUi viewport capsule mints and attaches the composition node once, pairs import-and-dispose per frame or resize, and threads tree mutations through `Compositor.RequestCompositionUpdate`; transitional motion rides an `ImplicitAnimationCollection` keyed on `Offset`/`Opacity`/`Size` so a layout assignment animates without a per-frame tick.

[LOCAL_ADMISSION]:
- Interop path admits an externally-rendered (`Wgpu`) texture; the `ISkiaSharpApiLease` lease path admits a Skia-Ganesh render sharing Avalonia's `GRContext`, and each `GpuBackend` row selects exactly one.
- Imported images and semaphores are lifetime-scoped `IAsyncDisposable` handles exposing `ImportCompleted`/`IsLost`; the capsule awaits `ImportCompleted` before freeing a non-owning source, pairs import-and-dispose per frame or resize, and drops every handle across an `IsLost` transition.
- All interop work runs on the compositor render thread: import returns immediately-usable handles and `Update*Async` completes on the compositor loop, so the capsule awaits the `ValueTask`/`Task` rather than blocking the UI thread.
- Transform, opacity, and color motion rides a composition animation with a duration of at least 1ms; per-frame custom drawing rides a custom visual handler.

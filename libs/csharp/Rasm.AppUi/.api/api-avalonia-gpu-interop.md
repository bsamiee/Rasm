# [RASM_APPUI_API_AVALONIA_GPU_INTEROP]

The Avalonia 12 compositor GPU-interop seam imports an externally-rendered GPU texture into the composition tree: a control reads its backing `CompositionVisual` through `ElementComposition.GetElementVisual(this)` and its `Compositor`, `Compositor.TryGetCompositionGpuInterop()` queries the platform render interface for the `ICompositionGpuInterop` capability, `ICompositionGpuInterop.ImportImage`/`ImportSemaphore` import a shared GPU handle and a synchronization primitive, a `Compositor.CreateDrawingSurface()` is set onto a `Compositor.CreateSurfaceVisual()` via `CompositionSurfaceVisual.Surface`, that visual is attached into the element tree through `ElementComposition.SetElementChildVisual`, and per frame the surface is refreshed through the `CompositionDrawingSurface.UpdateWith{KeyedMutex,Semaphores,TimelineSemaphores}Async`/`UpdateAsync` synchronization-discriminated family so a wgpu-rendered or D3D/Vulkan/Metal-rendered frame composites into the Avalonia scene without a second swapchain. This compositor seam replaces the bare `ISkiaSharpApiLease` shared-context path when an external GPU backend (the `Wgpu` family) owns the render.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia` 'compositor'

- package: `Avalonia`
- license: MIT
- floor: `net10.0` consumer; the GPU-interop surface resolves from the `ref/net10.0` reference assembly (`Avalonia.Base.dll`)
- assembly: `Avalonia.Base` (compositor + GPU-interop owners), `Avalonia.Base` also owns `Avalonia.Platform` handle-type and external-image vocabulary
- namespace: `Avalonia.Rendering.Composition` (compositor, surfaces, imported handles), `Avalonia.Platform` (`IPlatformHandle`, external-image properties, known handle-type constants)
- asset: runtime library (no native asset; the GPU backend supplies the shared handle)
- rail: visuals

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compositor and GPU-interop owners — `Avalonia.Rendering.Composition`

- rail: visuals

| [INDEX] | [SYMBOL]                                                 | [KIND]            | [RAIL]                |
| :-----: | :------------------------------------------------------- | :---------------- | :-------------------- |
|  [01]   | `Compositor`                                             | composition root  | composition factories |
|  [02]   | `ICompositionGpuInterop`                                 | interop interface | external GPU import   |
|  [03]   | `CompositionDrawingSurface`                              | sealed surface    | imported-image target |
|  [04]   | `CompositionSurface`                                     | abstract surface  | drawing-surface base  |
|  [05]   | `CompositionSurfaceVisual`                               | container visual  | surface-backed node   |
|  [06]   | `CompositionContainerVisual`                             | container visual  | child-visual host     |
|  [07]   | `ElementComposition`                                     | static accessor   | element-visual bridge |
|  [08]   | `ICompositionGpuImportedObject`                          | imported base     | import lifecycle      |
|  [09]   | `ICompositionImportedGpuImage`                           | imported handle   | texture lifetime      |
|  [10]   | `ICompositionImportedGpuSemaphore`                       | imported handle   | semaphore lifetime    |
|  [11]   | `CompositionGpuImportedImageSynchronizationCapabilities` | flags enum        | synchronization modes |

[PUBLIC_TYPE_SCOPE]: external-image and handle vocabulary — `Avalonia.Platform`

- rail: visuals

| [INDEX] | [SYMBOL]                                            | [KIND]             | [RAIL]                   |
| :-----: | :-------------------------------------------------- | :----------------- | :----------------------- |
|  [01]   | `IPlatformHandle`                                   | handle interface   | shared-resource handle   |
|  [02]   | `PlatformGraphicsExternalImageProperties`           | record struct      | external-image layout    |
|  [03]   | `PlatformGraphicsExternalImageFormat`               | enum               | channel order            |
|  [04]   | `KnownPlatformGraphicsExternalImageHandleTypes`     | static const class | image handle identifiers |
|  [05]   | `KnownPlatformGraphicsExternalSemaphoreHandleTypes` | static const class | semaphore identifiers    |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compositor acquisition, GPU-interop query, surface/visual factories — `Compositor` / `ElementComposition`

- rail: visuals

| [INDEX] | [SURFACE]                                                            | [SURFACE_ROOT]       | [RAIL]                 |
| :-----: | :------------------------------------------------------------------- | :------------------- | :--------------------- |
|  [01]   | `static GetElementVisual(Visual) : CompositionVisual?`               | `ElementComposition` | backing visual         |
|  [02]   | `static SetElementChildVisual(Visual, CompositionVisual?)`           | `ElementComposition` | child attach           |
|  [03]   | `static GetElementChildVisual(Visual) : CompositionVisual?`          | `ElementComposition` | attached child         |
|  [04]   | `CompositionVisual.Compositor : Compositor`                          | `CompositionVisual`  | visual compositor      |
|  [05]   | `static TryGetDefaultCompositor() : Compositor?`                     | `Compositor`         | process compositor     |
|  [06]   | `TryGetCompositionGpuInterop() : ValueTask<ICompositionGpuInterop?>` | `Compositor`         | nullable interop query |
|  [07]   | `CreateDrawingSurface() : CompositionDrawingSurface`                 | `Compositor`         | imported-image target  |
|  [08]   | `CreateSurfaceVisual() : CompositionSurfaceVisual`                   | `Compositor`         | surface node           |
|  [09]   | `CreateContainerVisual() : CompositionContainerVisual`               | `Compositor`         | transform parent       |
|  [10]   | `RequestCompositionUpdate(Action)`                                   | `Compositor`         | pre-commit callback    |

[ENTRYPOINT_SCOPE]: external image and semaphore import + sync-capability query — `ICompositionGpuInterop`

- rail: visuals

| [INDEX] | [MEMBER]                         | [FORM]                   | [RESULT]           |
| :-----: | :------------------------------- | :----------------------- | :----------------- |
|  [01]   | `ImportImage`                    | platform handle          | imported image     |
|  [02]   | `ImportImage`                    | shared-context image     | imported image     |
|  [03]   | `ImportSemaphore`                | platform handle          | imported semaphore |
|  [04]   | `ImportSemaphore`                | shared-context semaphore | imported image     |
|  [05]   | `GetSynchronizationCapabilities` | image handle type        | capability flags   |

[PLATFORM_IMAGE_IMPORT]: `ImportImage(IPlatformHandle, PlatformGraphicsExternalImageProperties) : ICompositionImportedGpuImage` returns an immediately usable image.

[CONTEXT_IMAGE_IMPORT]: `ImportImage(ICompositionImportableSharedGpuContextImage) : ICompositionImportedGpuImage` leaves the caller owning the handle on failure.

[PLATFORM_SEMAPHORE_IMPORT]: `ImportSemaphore(IPlatformHandle) : ICompositionImportedGpuSemaphore` returns an immediately usable semaphore.

[CONTEXT_SEMAPHORE_IMPORT]: `ImportSemaphore(ICompositionImportableSharedGpuContextSemaphore) : ICompositionImportedGpuImage` imports a same-context semaphore.

[SYNC_CAPABILITY_QUERY]: `GetSynchronizationCapabilities(string imageHandleType) : CompositionGpuImportedImageSynchronizationCapabilities` returns the update-mode flags for one handle type.

[ENTRYPOINT_SCOPE]: synchronization-discriminated surface update family — `CompositionDrawingSurface`

- rail: visuals

| [INDEX] | [MEMBER]                            | [SYNC_CAP]           | [PRIMITIVE]      |
| :-----: | :---------------------------------- | :------------------- | :--------------- |
|  [01]   | `UpdateWithKeyedMutexAsync`         | `KeyedMutex`         | key pair         |
|  [02]   | `UpdateWithSemaphoresAsync`         | `Semaphores`         | binary pair      |
|  [03]   | `UpdateWithTimelineSemaphoresAsync` | `TimelineSemaphores` | timeline values  |
|  [04]   | `UpdateAsync`                       | `Automatic`          | platform-managed |
|  [05]   | `Dispose` / finalizer               | —                    | surface teardown |

[KEYED_MUTEX_UPDATE]: `UpdateWithKeyedMutexAsync(ICompositionImportedGpuImage, uint acquireIndex, uint releaseIndex) : Task` carries the D3D11 acquire/release key pair.

[BINARY_SEMAPHORE_UPDATE]: `UpdateWithSemaphoresAsync(ICompositionImportedGpuImage, ICompositionImportedGpuSemaphore wait, ICompositionImportedGpuSemaphore signal) : Task` carries the Vulkan/D3D12 wait/signal pair.

[TIMELINE_SEMAPHORE_UPDATE]: `UpdateWithTimelineSemaphoresAsync(ICompositionImportedGpuImage, ICompositionImportedGpuSemaphore wait, ulong waitValue, ICompositionImportedGpuSemaphore signal, ulong signalValue) : Task` carries the timeline wait/signal values.

[AUTOMATIC_UPDATE]: `UpdateAsync(ICompositionImportedGpuImage) : Task` delegates synchronization to the platform for Metal `IOSurface` images.

[SURFACE_TEARDOWN]: `Dispose()` tears down the surface, and `~CompositionDrawingSurface()` posts finalizer disposal to the dispatcher.

[ENTRYPOINT_SCOPE]: GPU-interop capability properties + imported-handle lifetime — `ICompositionGpuInterop` / `ICompositionGpuImportedObject`

- rail: visuals

| [INDEX] | [SURFACE]                   | [TYPE]                  | [RAIL]                                                              |
| :-----: | :-------------------------- | :---------------------- | :------------------------------------------------------------------ |
|  [01]   | `DeviceLuid` (get/set)      | `byte[]?`               | adapter LUID (D3D adapter match); settable to pin the import target |
|  [02]   | `DeviceUuid` (get/set)      | `byte[]?`               | adapter UUID (Vulkan adapter match)                                 |
|  [03]   | `SupportedImageHandleTypes` | `IReadOnlyList<string>` | importable image-handle kinds (values from the known-types class)   |
|  [04]   | `SupportedSemaphoreTypes`   | `IReadOnlyList<string>` | importable semaphore-handle kinds                                   |
|  [05]   | `IsLost` (interop)          | `bool`                  | device-context availability (interop-level loss)                    |
|  [06]   | `ImportCompleted`           | `Task`                  | per-handle import completion; gates freeing a non-owning source     |
|  [07]   | `IsLost` (imported handle)  | `bool`                  | per-imported-object device-context loss                             |

[ENTRYPOINT_SCOPE]: imported-image shape — `PlatformGraphicsExternalImageProperties` (record struct)

- rail: visuals

| [INDEX] | [FIELD]         | [TYPE]                                | [RAIL]                                           |
| :-----: | :-------------- | :------------------------------------ | :----------------------------------------------- |
|  [01]   | `Width`         | `int`                                 | imported texture width                           |
|  [02]   | `Height`        | `int`                                 | imported texture height                          |
|  [03]   | `Format`        | `PlatformGraphicsExternalImageFormat` | `R8G8B8A8UNorm` / `B8G8R8A8UNorm` channel order  |
|  [04]   | `MemorySize`    | `ulong`                               | external-memory allocation size (Vulkan import)  |
|  [05]   | `MemoryOffset`  | `ulong`                               | offset of the image within the allocation        |
|  [06]   | `TopLeftOrigin` | `bool`                                | origin convention; false flips the sampled frame |

[ENTRYPOINT_SCOPE]: known handle-type constants — `KnownPlatformGraphicsExternalImageHandleTypes` / `…SemaphoreHandleTypes`

- rail: visuals

| [INDEX] | [CONSTANT]                                       | [OWNER]                | [RAIL]                                                   |
| :-----: | :----------------------------------------------- | :--------------------- | :------------------------------------------------------- |
|  [01]   | `D3D11TextureGlobalSharedHandle`                 | image handle types     | DXGI `GetSharedHandle` global (non-owning) D3D11 texture |
|  [02]   | `D3D11TextureNtHandle`                           | image handle types     | DXGI `CreateSharedHandle` NT-handle D3D11 texture        |
|  [03]   | `VulkanOpaquePosixFileDescriptor`                | image + semaphore      | Vulkan `OPAQUE_FD` external-memory / external-semaphore  |
|  [04]   | `VulkanOpaqueNtHandle` / `VulkanOpaqueKmtHandle` | image + semaphore      | Vulkan `OPAQUE_WIN32` / KMT external handle              |
|  [05]   | `IOSurfaceRef`                                   | image handle types     | Metal `IOSurface` reference (automatic-sync image)       |
|  [06]   | `Direct3D12FenceNtHandle`                        | semaphore handle types | D3D12 / D3D11 fence shared NT handle (timeline-capable)  |
|  [07]   | `MetalSharedEvent`                               | semaphore handle types | `MTLSharedEvent` pointer (Metal timeline equivalent)     |

## [04]-[IMPLEMENTATION_LAW]

[INTEROP_TOPOLOGY]:

- A control obtains its compositor through `ElementComposition.GetElementVisual(this)?.Compositor`. `GetElementVisual` returns `CompositionVisual?` and remains null until the control enters a render tree, so the capsule defers import to the first composition update.
- `Compositor.TryGetCompositionGpuInterop()` returns `ValueTask<ICompositionGpuInterop?>`; a null or `IsLost` result folds to the `Software` Skia-raster path.
- The settable `DeviceLuid` and `DeviceUuid` identify the compositor's graphics adapter. The external wgpu/D3D/Vulkan device matches these byte arrays through `Silk.NET.WebGPU` `InstanceRequestAdapter`, and a D3D import may set `DeviceLuid` to pin the target.
- Matching adapter identities avoids the cross-adapter copy imposed by a mismatch.
- The capsule constructs the composition node once: `CreateDrawingSurface()` mints the `CompositionDrawingSurface`, and `CreateSurfaceVisual()` mints the `CompositionSurfaceVisual`.
- `surfaceVisual.Surface = drawingSurface` binds the change-tracked `CompositionSurface?` slot for render-thread serialization. `ElementComposition.SetElementChildVisual(this, surfaceVisual)` attaches that visual as the element's last child.
- `SetElementChildVisual` enforces the same-`Compositor` invariant and throws across compositor instances, so a visual minted on the default compositor cannot attach to a control on another compositor.
- The external backend renders into a shared GPU texture: a D3D11 keyed-mutex shared handle, Vulkan external-memory `VkImage`, or Metal `IOSurface`. Its exported `IPlatformHandle.HandleDescriptor` is one of the `SupportedImageHandleTypes` strings.
- `ImportImage(IPlatformHandle, PlatformGraphicsExternalImageProperties)` imports the handle as `ICompositionImportedGpuImage`. The properties carry `Width`, `Height`, `Format`, `MemorySize`, `MemoryOffset`, and `TopLeftOrigin` so the compositor reads the exact memory window and channel order.
- `ImportSemaphore` imports the matching fence. Both imported handles expose `ImportCompleted`, which precedes freeing a non-owning source resource, and `IsLost`.
- `GetSynchronizationCapabilities(imageHandleType)` returns `CompositionGpuImportedImageSynchronizationCapabilities` flags that select the matching `CompositionDrawingSurface.Update*Async` member.
- `KeyedMutex` selects `UpdateWithKeyedMutexAsync(image, acquireIndex, releaseIndex)` for D3D11, `Semaphores` selects `UpdateWithSemaphoresAsync(image, wait, signal)` for Vulkan/D3D12 binary synchronization, `TimelineSemaphores` selects `UpdateWithTimelineSemaphoresAsync(image, wait, waitValue, signal, signalValue)`, and `Automatic` selects `UpdateAsync(image)` for Metal `IOSurface`.
- Each update returns a `Task` that completes when the render thread releases the image, so reuse or disposal follows the await.

[STACKING]:

- Stacks UNDER `api-silk-webgpu` / `api-silk-webgpu-wgpu`: the `Wgpu` `GpuBackend` `RenderTargetFactory` row enters through `WebGPU.GetApi()`, `CreateInstance`, and `InstanceRequestAdapter`, with the adapter matched to this interop's `DeviceLuid` or `DeviceUuid`.
- `AdapterRequestDevice` and `DeviceGetQueue` establish execution; `CommandEncoder`, `RenderPassEncoder`, and `ComputePassEncoder` record mesh-shader and compute passes; `QueueSubmit` executes them. The rendered wgpu `Texture` exports as the platform handle consumed by this catalog's `ImportImage`.
- The exported synchronization primitive selects the `UpdateWith*Async` method: a wgpu fence or timeline selects `UpdateWithTimelineSemaphoresAsync`, a D3D11 keyed mutex selects `UpdateWithKeyedMutexAsync`, and a Metal `IOSurface` selects `UpdateAsync`.
- Stacks BESIDE `api-avalonia-skia` / `api-skiasharp`: `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` shares Avalonia's `GRContext` for the `Metal`, `Vulkan`, and `OpenGl` `GpuFamily.SkiaGanesh` rows. This interop imports an independently-rendered external texture for the `Wgpu` `GpuFamily.Wgpu` row.
- The backend rows are mutually exclusive: a `GpuBackend` leases the shared `GRContext` for Skia or imports through `ICompositionGpuInterop` for wgpu. Both fold to the `Software` `GpuFamily.SkiaRaster` path when `TryGetCompositionGpuInterop` or `TryLeasePlatformGraphicsApi` returns null.
- Consumed by `.planning/Render/pipeline.md` `[WGPU_BACKEND]` SPIKE: that page's `RenderTargetFactory` `Wgpu` row binds the wgpu swapchain and presents through this seam; the page's prior `CompositionDrawingSurface.UpdateWithExternalImageAsync` spelling resolves to the real `UpdateWith{KeyedMutex,Semaphores,TimelineSemaphores}Async`/`UpdateAsync` family this catalog documents, the method chosen by `GetSynchronizationCapabilities` per the backend's export format.

[LOCAL_ADMISSION]:

- The GPU-interop path is the compositor seam superseding the bare `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` for an external-backend render: the lease path shares Avalonia's own `GRContext`, the interop path imports an independently-rendered external texture. The `Wgpu` `GpuBackend` family renders with its own wgpu device and presents through `ImportImage`; the SkiaSharp Ganesh families render through the leased `GRContext`.
- Imported images and semaphores are lifetime-scoped handles; both implement `IAsyncDisposable` and expose `ImportCompleted`/`IsLost` through `ICompositionGpuImportedObject`. The boundary capsule awaits `ImportCompleted` before freeing a non-owning source resource, pairs import-and-dispose per frame or per resize, and never leaks an imported handle across a device-lost (`IsLost`) transition on either the interop or the imported-object level.
- All GPU-interop work runs on the compositor's render thread; the import calls return immediately-usable handles and the `Update*Async` calls complete on the compositor loop, so the boundary capsule awaits the `ValueTask`/`Task` rather than blocking the UI thread, and schedules tree mutations through `Compositor.RequestCompositionUpdate` when it must run after layout.

[RAIL_LAW]:

- Package: `Avalonia` (`Avalonia.Base` compositor + `Avalonia.Platform` handle vocabulary)
- Owns: the GPU-interop seam — compositor acquisition, external GPU image/semaphore import, adapter identity match, the surface/visual factory + `SetElementChildVisual` attach, and the synchronization-discriminated `Update*Async` refresh for an externally-rendered (`Wgpu`) viewport frame.
- Accept: `TryGetCompositionGpuInterop` query, `DeviceLuid`/`DeviceUuid` adapter match, `ImportImage`/`ImportSemaphore` shared-handle import, `GetSynchronizationCapabilities`-driven selection of the matching `UpdateWith{KeyedMutex,Semaphores,TimelineSemaphores}Async`/`UpdateAsync`, the `Surface`-slot + `SetElementChildVisual` tree attach.
- Reject: a second swapchain or window composited beside the Avalonia scene; an unsynchronized imported image whose update method ignores the `GetSynchronizationCapabilities` mode (e.g. `UpdateAsync` on a keyed-mutex-only image); a cross-adapter import ignoring the `DeviceLuid`/`DeviceUuid` match; a phantom single `UpdateWithExternalImageAsync` bind in place of the discriminated family; freeing a source texture before `ImportCompleted`.

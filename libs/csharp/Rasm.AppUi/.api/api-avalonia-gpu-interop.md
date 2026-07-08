# [RASM_APPUI_API_AVALONIA_GPU_INTEROP]

The Avalonia 12 compositor GPU-interop seam imports an externally-rendered GPU texture into the composition tree: a control reads its backing `CompositionVisual` through `ElementComposition.GetElementVisual(this)` and its `Compositor`, `Compositor.TryGetCompositionGpuInterop()` queries the platform render interface for the `ICompositionGpuInterop` capability, `ICompositionGpuInterop.ImportImage`/`ImportSemaphore` import a shared GPU handle and a synchronization primitive, a `Compositor.CreateDrawingSurface()` is set onto a `Compositor.CreateSurfaceVisual()` via `CompositionSurfaceVisual.Surface`, that visual is attached into the element tree through `ElementComposition.SetElementChildVisual`, and per frame the surface is refreshed through the `CompositionDrawingSurface.UpdateWith{KeyedMutex,Semaphores,TimelineSemaphores}Async`/`UpdateAsync` synchronization-discriminated family so a wgpu-rendered or D3D/Vulkan/Metal-rendered frame composites into the Avalonia scene without a second swapchain. This is the modern replacement for the bare `ISkiaSharpApiLease` shared-context path when an external GPU backend (the `Wgpu` family) owns the render.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia` (compositor)
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

| [INDEX] | [SYMBOL]                                       | [KIND]              | [RAIL]                                                          |
| :-----: | :--------------------------------------------- | :------------------ | :------------------------------------------------------------- |
|  [01]   | `Compositor`                                   | composition root    | scene-graph owner, GPU-interop query, surface/visual factories |
|  [02]   | `ICompositionGpuInterop`                       | interop interface   | external GPU image/semaphore import, sync-capability query      |
|  [03]   | `CompositionDrawingSurface`                    | sealed surface      | imported-image render target, sync-discriminated update family  |
|  [04]   | `CompositionSurface`                           | abstract surface    | base of `CompositionDrawingSurface`; the `Surface` slot type    |
|  [05]   | `CompositionSurfaceVisual`                     | container visual    | surface-backed visual node (settable `Surface`)                |
|  [06]   | `CompositionContainerVisual`                   | container visual    | parent of `CompositionSurfaceVisual`; child-visual host         |
|  [07]   | `ElementComposition`                           | static accessor     | element-visual bridge + child-visual attach/detach              |
|  [08]   | `ICompositionGpuImportedObject`                | imported base       | `ImportCompleted` task + `IsLost`, `IAsyncDisposable`           |
|  [09]   | `ICompositionImportedGpuImage`                 | imported handle     | imported texture lifetime (`ICompositionGpuImportedObject`)     |
|  [10]   | `ICompositionImportedGpuSemaphore`             | imported handle     | imported semaphore lifetime (`ICompositionGpuImportedObject`)   |
|  [11]   | `CompositionGpuImportedImageSynchronizationCapabilities` | flags enum | `Semaphores`/`KeyedMutex`/`Automatic`/`TimelineSemaphores`      |

[PUBLIC_TYPE_SCOPE]: external-image and handle vocabulary — `Avalonia.Platform`
- rail: visuals

| [INDEX] | [SYMBOL]                                        | [KIND]              | [RAIL]                                                       |
| :-----: | :---------------------------------------------- | :------------------ | :----------------------------------------------------------- |
|  [01]   | `IPlatformHandle`                               | handle interface    | opaque shared-resource handle (`Handle`, `HandleDescriptor`) |
|  [02]   | `PlatformGraphicsExternalImageProperties`       | record struct       | imported-image shape (size, format, memory window, origin)   |
|  [03]   | `PlatformGraphicsExternalImageFormat`           | enum                | `R8G8B8A8UNorm` / `B8G8R8A8UNorm`                            |
|  [04]   | `KnownPlatformGraphicsExternalImageHandleTypes` | static const class  | importable image-handle-type string constants                |
|  [05]   | `KnownPlatformGraphicsExternalSemaphoreHandleTypes` | static const class | importable semaphore-handle-type string constants            |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compositor acquisition, GPU-interop query, surface/visual factories — `Compositor` / `ElementComposition`
- rail: visuals

| [INDEX] | [SURFACE]                                                    | [SURFACE_ROOT]       | [RAIL]                                                                    |
| :-----: | :----------------------------------------------------------- | :------------------- | :----------------------------------------------------------------------- |
|  [01]   | `static GetElementVisual(Visual) : CompositionVisual?`       | `ElementComposition` | element backing-visual handle (null until attached to a render tree)      |
|  [02]   | `static SetElementChildVisual(Visual, CompositionVisual?)`   | `ElementComposition` | attach custom composition visual as last child (same-compositor enforced) |
|  [03]   | `static GetElementChildVisual(Visual) : CompositionVisual?`  | `ElementComposition` | read the previously-attached child visual                                 |
|  [04]   | `CompositionVisual.Compositor : Compositor`                  | `CompositionVisual`  | compositor handle off any visual                                          |
|  [05]   | `static TryGetDefaultCompositor() : Compositor?`             | `Compositor`         | process default compositor (null before the first window is shown)        |
|  [06]   | `TryGetCompositionGpuInterop() : ValueTask<ICompositionGpuInterop?>` | `Compositor` | GPU-interop query (nullable result is the no-GPU floor)                   |
|  [07]   | `CreateDrawingSurface() : CompositionDrawingSurface`         | `Compositor`         | drawing-surface create (the imported-image target)                        |
|  [08]   | `CreateSurfaceVisual() : CompositionSurfaceVisual`           | `Compositor`         | surface-visual create (the tree node the surface binds to)                |
|  [09]   | `CreateContainerVisual() : CompositionContainerVisual`       | `Compositor`         | plain container visual (grouping / transform parent)                      |
|  [10]   | `RequestCompositionUpdate(Action)`                          | `Compositor`         | enqueue a pre-commit callback on the render loop                          |

[ENTRYPOINT_SCOPE]: external image and semaphore import + sync-capability query — `ICompositionGpuInterop`
- rail: visuals

| [INDEX] | [SURFACE]                                                                                     | [SURFACE_ROOT]           | [RAIL]                                                         |
| :-----: | :-------------------------------------------------------------------------------------------- | :----------------------- | :------------------------------------------------------------ |
|  [01]   | `ImportImage(IPlatformHandle, PlatformGraphicsExternalImageProperties) : ICompositionImportedGpuImage` | `ICompositionGpuInterop` | import shared-handle image (immediately usable)               |
|  [02]   | `ImportImage(ICompositionImportableSharedGpuContextImage) : ICompositionImportedGpuImage`     | `ICompositionGpuInterop` | import same-context image (caller owns handle on failure)     |
|  [03]   | `ImportSemaphore(IPlatformHandle) : ICompositionImportedGpuSemaphore`                          | `ICompositionGpuInterop` | import shared-handle semaphore (immediately usable)           |
|  [04]   | `ImportSemaphore(ICompositionImportableSharedGpuContextSemaphore) : ICompositionImportedGpuImage` | `ICompositionGpuInterop` | import same-context semaphore                                  |
|  [05]   | `GetSynchronizationCapabilities(string imageHandleType) : CompositionGpuImportedImageSynchronizationCapabilities` | `ICompositionGpuInterop` | per-handle-type sync mode flags selecting the update method   |

[ENTRYPOINT_SCOPE]: synchronization-discriminated surface update family — `CompositionDrawingSurface`
- rail: visuals

| [INDEX] | [SURFACE]                                                                                                            | [SYNC_CAP]          | [RAIL]                                                       |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :------------------ | :----------------------------------------------------------- |
|  [01]   | `UpdateWithKeyedMutexAsync(ICompositionImportedGpuImage, uint acquireIndex, uint releaseIndex) : Task`              | `KeyedMutex`        | D3D11 keyed-mutex acquire/release key pair                  |
|  [02]   | `UpdateWithSemaphoresAsync(ICompositionImportedGpuImage, ICompositionImportedGpuSemaphore wait, ICompositionImportedGpuSemaphore signal) : Task` | `Semaphores` | binary wait/signal semaphore pair (Vulkan/D3D12) |
|  [03]   | `UpdateWithTimelineSemaphoresAsync(ICompositionImportedGpuImage, ICompositionImportedGpuSemaphore wait, ulong waitValue, ICompositionImportedGpuSemaphore signal, ulong signalValue) : Task` | `TimelineSemaphores` | timeline-semaphore wait/signal value pair |
|  [04]   | `UpdateAsync(ICompositionImportedGpuImage) : Task`                                                                  | `Automatic`         | platform-managed synchronization (Metal `IOSurface`)        |
|  [05]   | `Dispose()` / `~CompositionDrawingSurface()`                                                                        | —                   | surface teardown (finalizer posts dispose to the dispatcher) |

[ENTRYPOINT_SCOPE]: GPU-interop capability properties + imported-handle lifetime — `ICompositionGpuInterop` / `ICompositionGpuImportedObject`
- rail: visuals

| [INDEX] | [SURFACE]                   | [TYPE]                  | [RAIL]                                                              |
| :-----: | :-------------------------- | :---------------------- | :----------------------------------------------------------------- |
|  [01]   | `DeviceLuid` (get/set)      | `byte[]?`               | adapter LUID (D3D adapter match); settable to pin the import target |
|  [02]   | `DeviceUuid` (get/set)      | `byte[]?`               | adapter UUID (Vulkan adapter match)                                 |
|  [03]   | `SupportedImageHandleTypes` | `IReadOnlyList<string>` | importable image-handle kinds (values from the known-types class)   |
|  [04]   | `SupportedSemaphoreTypes`   | `IReadOnlyList<string>` | importable semaphore-handle kinds                                   |
|  [05]   | `IsLost` (interop)          | `bool`                  | device-context availability (interop-level loss)                    |
|  [06]   | `ImportCompleted`           | `Task`                  | per-handle import completion; gates freeing a non-owning source     |
|  [07]   | `IsLost` (imported handle)  | `bool`                  | per-imported-object device-context loss                             |

[ENTRYPOINT_SCOPE]: imported-image shape — `PlatformGraphicsExternalImageProperties` (record struct)
- rail: visuals

| [INDEX] | [FIELD]          | [TYPE]                                  | [RAIL]                                                  |
| :-----: | :--------------- | :-------------------------------------- | :----------------------------------------------------- |
|  [01]   | `Width`          | `int`                                   | imported texture width                                 |
|  [02]   | `Height`         | `int`                                   | imported texture height                                |
|  [03]   | `Format`         | `PlatformGraphicsExternalImageFormat`   | `R8G8B8A8UNorm` / `B8G8R8A8UNorm` channel order         |
|  [04]   | `MemorySize`     | `ulong`                                 | external-memory allocation size (Vulkan import)         |
|  [05]   | `MemoryOffset`   | `ulong`                                 | offset of the image within the allocation              |
|  [06]   | `TopLeftOrigin`  | `bool`                                  | origin convention; false flips the sampled frame        |

[ENTRYPOINT_SCOPE]: known handle-type constants — `KnownPlatformGraphicsExternalImageHandleTypes` / `…SemaphoreHandleTypes`
- rail: visuals

| [INDEX] | [CONSTANT]                                          | [OWNER]              | [RAIL]                                                       |
| :-----: | :------------------------------------------------- | :------------------- | :----------------------------------------------------------- |
|  [01]   | `D3D11TextureGlobalSharedHandle`                   | image handle types   | DXGI `GetSharedHandle` global (non-owning) D3D11 texture     |
|  [02]   | `D3D11TextureNtHandle`                             | image handle types   | DXGI `CreateSharedHandle` NT-handle D3D11 texture            |
|  [03]   | `VulkanOpaquePosixFileDescriptor`                  | image + semaphore    | Vulkan `OPAQUE_FD` external-memory / external-semaphore       |
|  [04]   | `VulkanOpaqueNtHandle` / `VulkanOpaqueKmtHandle`   | image + semaphore    | Vulkan `OPAQUE_WIN32` / KMT external handle                  |
|  [05]   | `IOSurfaceRef`                                     | image handle types   | Metal `IOSurface` reference (automatic-sync image)           |
|  [06]   | `Direct3D12FenceNtHandle`                          | semaphore handle types | D3D12 / D3D11 fence shared NT handle (timeline-capable)     |
|  [07]   | `MetalSharedEvent`                                 | semaphore handle types | `MTLSharedEvent` pointer (Metal timeline equivalent)         |

## [04]-[IMPLEMENTATION_LAW]

[INTEROP_TOPOLOGY]:
- A control obtains its compositor by `ElementComposition.GetElementVisual(this)?.Compositor` — `GetElementVisual` returns `CompositionVisual?` and is null until the control is rooted in a render tree, so the capsule defers the import to the first composition update. `Compositor.TryGetCompositionGpuInterop()` returns a `ValueTask<ICompositionGpuInterop?>`; a null/`IsLost` result is the no-GPU floor that folds to the `Software` Skia-raster path.
- The `DeviceLuid`/`DeviceUuid` properties are settable and identify the compositor's graphics adapter so the external wgpu/D3D/Vulkan device is created on the SAME adapter; a mismatched adapter forces a cross-adapter copy the LUID/UUID match avoids. The external backend matches its adapter against these byte arrays (`Silk.NET.WebGPU` `InstanceRequestAdapter`), and a D3D import may set `DeviceLuid` to pin the import target.
- The capsule constructs the composition node once: `CreateDrawingSurface()` mints a `CompositionDrawingSurface`, `CreateSurfaceVisual()` mints a `CompositionSurfaceVisual`, `surfaceVisual.Surface = drawingSurface` binds the surface into the visual (`CompositionSurface?` slot, change-tracked and serialized to the render thread), and `ElementComposition.SetElementChildVisual(this, surfaceVisual)` attaches the visual as the element's last child — `SetElementChildVisual` enforces a same-`Compositor` invariant and throws across compositor instances, so a stray visual minted on the default compositor cannot be attached to a control on another.
- The external backend renders into a shared GPU texture (D3D11 keyed-mutex shared handle, Vulkan external-memory `VkImage`, Metal `IOSurface`), exports an `IPlatformHandle` whose `HandleDescriptor` is one of the `SupportedImageHandleTypes` strings, and `ImportImage(IPlatformHandle, PlatformGraphicsExternalImageProperties)` imports it as an `ICompositionImportedGpuImage` — the `PlatformGraphicsExternalImageProperties` carries `Width`/`Height`/`Format`/`MemorySize`/`MemoryOffset`/`TopLeftOrigin` so the compositor reads the exact memory window and channel order. `ImportSemaphore` imports the matching fence; both imported handles expose `ImportCompleted` (await before freeing a non-owning source resource) and `IsLost`.
- The per-frame refresh is the synchronization-discriminated update family, NOT a single bind call. `GetSynchronizationCapabilities(imageHandleType)` returns `CompositionGpuImportedImageSynchronizationCapabilities` flags, and the capsule selects the matching `CompositionDrawingSurface.Update*Async` exactly as the enum cases map: `KeyedMutex` -> `UpdateWithKeyedMutexAsync(image, acquireIndex, releaseIndex)` (D3D11), `Semaphores` -> `UpdateWithSemaphoresAsync(image, wait, signal)` (Vulkan/D3D12 binary), `TimelineSemaphores` -> `UpdateWithTimelineSemaphoresAsync(image, wait, waitValue, signal, signalValue)`, `Automatic` -> `UpdateAsync(image)` (Metal `IOSurface`). Each returns a `Task` that completes when the render thread is done with the image, so the caller is free to reuse or dispose the texture only after the await.

[STACKING]:
- Stacks UNDER `api-silk-webgpu` / `api-silk-webgpu-wgpu`: the `Wgpu` `GpuBackend` `RenderTargetFactory` row uses `WebGPU.GetApi()` -> `CreateInstance` -> `InstanceRequestAdapter` (adapter matched to `DeviceLuid`/`DeviceUuid` from this interop) -> `AdapterRequestDevice`+`DeviceGetQueue` -> records mesh-shader/compute passes through `CommandEncoder`/`RenderPassEncoder`/`ComputePassEncoder` -> `QueueSubmit`, then exports the rendered wgpu `Texture` as a platform handle that THIS catalog's `ImportImage` consumes. The sync primitive wgpu exposes selects the `UpdateWith*Async` method: wgpu's fence/timeline maps to `UpdateWithTimelineSemaphoresAsync`, a D3D11 keyed-mutex backing maps to `UpdateWithKeyedMutexAsync`, a Metal `IOSurface` maps to `UpdateAsync`.
- Stacks BESIDE `api-avalonia-skia` / `api-skiasharp`: the `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` lease path shares Avalonia's own `GRContext` for IN-Skia draw (the `Metal`/`Vulkan`/`OpenGl` `GpuFamily.SkiaGanesh` rows), while THIS interop path imports an INDEPENDENTLY-rendered external texture (the `Wgpu` `GpuFamily.Wgpu` row). The two are mutually exclusive per backend row: a `GpuBackend` either leases the shared `GRContext` (Skia present) or imports through `ICompositionGpuInterop` (wgpu present). The `Software` `GpuFamily.SkiaRaster` 2D-Skia fallback is the floor both degrade to when `TryGetCompositionGpuInterop`/`TryLeasePlatformGraphicsApi` yields null.
- Consumed by `.planning/Render/pipeline.md` `[WGPU_BACKEND]` SPIKE: that page's `RenderTargetFactory` `Wgpu` row binds the wgpu swapchain and presents through this seam; the page's prior `CompositionDrawingSurface.UpdateWithExternalImageAsync` spelling resolves to the real `UpdateWith{KeyedMutex,Semaphores,TimelineSemaphores}Async`/`UpdateAsync` family this catalog documents, the method chosen by `GetSynchronizationCapabilities` per the backend's export format.

[LOCAL_ADMISSION]:
- The GPU-interop path is the modern compositor seam superseding the bare `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` for an external-backend render: the lease path shares Avalonia's own `GRContext`, the interop path imports an independently-rendered external texture. The `Wgpu` `GpuBackend` family renders with its own wgpu device and presents through `ImportImage`; the SkiaSharp Ganesh families render through the leased `GRContext`.
- Imported images and semaphores are lifetime-scoped handles; both implement `IAsyncDisposable` and expose `ImportCompleted`/`IsLost` through `ICompositionGpuImportedObject`. The boundary capsule awaits `ImportCompleted` before freeing a non-owning source resource, pairs import-and-dispose per frame or per resize, and never leaks an imported handle across a device-lost (`IsLost`) transition on either the interop or the imported-object level.
- All GPU-interop work runs on the compositor's render thread; the import calls return immediately-usable handles and the `Update*Async` calls complete on the compositor loop, so the boundary capsule awaits the `ValueTask`/`Task` rather than blocking the UI thread, and schedules tree mutations through `Compositor.RequestCompositionUpdate` when it must run after layout.

[RAIL_LAW]:
- Package: `Avalonia` (`Avalonia.Base` compositor + `Avalonia.Platform` handle vocabulary)
- Owns: the GPU-interop seam — compositor acquisition, external GPU image/semaphore import, adapter identity match, the surface/visual factory + `SetElementChildVisual` attach, and the synchronization-discriminated `Update*Async` refresh for an externally-rendered (`Wgpu`) viewport frame.
- Accept: `TryGetCompositionGpuInterop` query, `DeviceLuid`/`DeviceUuid` adapter match, `ImportImage`/`ImportSemaphore` shared-handle import, `GetSynchronizationCapabilities`-driven selection of the matching `UpdateWith{KeyedMutex,Semaphores,TimelineSemaphores}Async`/`UpdateAsync`, the `Surface`-slot + `SetElementChildVisual` tree attach.
- Reject: a second swapchain or window composited beside the Avalonia scene; an unsynchronized imported image whose update method ignores the `GetSynchronizationCapabilities` mode (e.g. `UpdateAsync` on a keyed-mutex-only image); a cross-adapter import ignoring the `DeviceLuid`/`DeviceUuid` match; a phantom single `UpdateWithExternalImageAsync` bind in place of the discriminated family; freeing a source texture before `ImportCompleted`.

# [RASM_APPUI_API_AVALONIA_GPU_INTEROP]

`Avalonia`'s compositor GPU-interop seam imports an externally-rendered GPU texture into the composition tree, so a wgpu, D3D, Vulkan, or Metal backend composites into the Avalonia scene without a second swapchain. `ICompositionGpuInterop` is the boundary: it imports a shared GPU image and its synchronization fence, a `CompositionDrawingSurface` carries the imported frame, and `ElementComposition` attaches the surface visual to a control on the visuals rail. This seam supersedes the `ISkiaSharpApiLease` shared-`GRContext` path when an external backend owns the render.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia`
- package: `Avalonia` (MIT)
- assembly: `Avalonia.Base`
- namespace: `Avalonia.Rendering.Composition`, `Avalonia.Platform`
- asset: runtime library; the external GPU backend supplies the shared handle
- rail: visuals

## [02]-[PUBLIC_TYPES]

[COMPOSITION_TYPES]: compositor and GPU-interop owners (`Avalonia.Rendering.Composition`)

| [INDEX] | [SYMBOL]                                                 | [TYPE_FAMILY]  | [CAPABILITY]                                               |
| :-----: | :------------------------------------------------------- | :------------- | :--------------------------------------------------------- |
|  [01]   | `Compositor`                                             | class          | composition-factory root                                   |
|  [02]   | `ICompositionGpuInterop`                                 | interface      | external GPU image/semaphore import                        |
|  [03]   | `CompositionDrawingSurface`                              | sealed class   | imported-image target surface                              |
|  [04]   | `CompositionSurface`                                     | abstract class | drawing-surface base                                       |
|  [05]   | `CompositionSurfaceVisual`                               | class          | surface-backed visual node                                 |
|  [06]   | `CompositionContainerVisual`                             | class          | child-visual host                                          |
|  [07]   | `ElementComposition`                                     | static class   | element-to-visual bridge                                   |
|  [08]   | `ICompositionGpuImportedObject`                          | interface      | imported-handle lifetime base                              |
|  [09]   | `ICompositionImportedGpuImage`                           | interface      | imported texture handle                                    |
|  [10]   | `ICompositionImportedGpuSemaphore`                       | interface      | imported fence handle                                      |
|  [11]   | `CompositionGpuImportedImageSynchronizationCapabilities` | flags enum     | `Semaphores`/`KeyedMutex`/`Automatic`/`TimelineSemaphores` |

[PLATFORM_TYPES]: external-image and handle vocabulary (`Avalonia.Platform`)

| [INDEX] | [SYMBOL]                                            | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------- | :------------ | :------------------------------------ |
|  [01]   | `IPlatformHandle`                                   | interface     | shared-resource handle carrier        |
|  [02]   | `PlatformGraphicsExternalImageProperties`           | record struct | external-image memory/format layout   |
|  [03]   | `PlatformGraphicsExternalImageFormat`               | enum          | `R8G8B8A8UNorm`/`B8G8R8A8UNorm` order |
|  [04]   | `KnownPlatformGraphicsExternalImageHandleTypes`     | static class  | image handle-type constants           |
|  [05]   | `KnownPlatformGraphicsExternalSemaphoreHandleTypes` | static class  | semaphore handle-type constants       |

## [03]-[ENTRYPOINTS]

[COMPOSITOR_ACCESS]: compositor acquisition, interop query, and surface/visual factories

| [INDEX] | [SURFACE]                                                              | [SHAPE]  | [CAPABILITY]                                  |
| :-----: | :--------------------------------------------------------------------- | :------- | :-------------------------------------------- |
|  [01]   | `ElementComposition.GetElementVisual(Visual)`                          | static   | backing visual, null until in a render tree   |
|  [02]   | `ElementComposition.SetElementChildVisual(Visual, CompositionVisual?)` | static   | attach a visual as the element's last child   |
|  [03]   | `ElementComposition.GetElementChildVisual(Visual)`                     | static   | previously-attached `CompositionVisual?`      |
|  [04]   | `CompositionVisual.Compositor`                                         | property | owning `Compositor`                           |
|  [05]   | `Compositor.TryGetDefaultCompositor()`                                 | static   | process `Compositor?`                         |
|  [06]   | `Compositor.TryGetCompositionGpuInterop()`                             | instance | `ValueTask<ICompositionGpuInterop?>` query    |
|  [07]   | `Compositor.CreateDrawingSurface()`                                    | factory  | `CompositionDrawingSurface` import target     |
|  [08]   | `Compositor.CreateSurfaceVisual()`                                     | factory  | `CompositionSurfaceVisual` surface node       |
|  [09]   | `Compositor.CreateContainerVisual()`                                   | factory  | `CompositionContainerVisual` transform parent |
|  [10]   | `Compositor.RequestCompositionUpdate(Action)`                          | instance | pre-commit callback on the compositor loop    |

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

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                                                          |
| :-----: | :---------------------------------------------- | :------- | :-------------------------------------------------------------------- |
|  [01]   | `DeviceLuid`                                    | property | `byte[]?` adapter LUID (D3D match); settable to pin the import target |
|  [02]   | `DeviceUuid`                                    | property | `byte[]?` adapter UUID (Vulkan match)                                 |
|  [03]   | `SupportedImageHandleTypes`                     | property | `IReadOnlyList<string>` importable image-handle kinds                 |
|  [04]   | `SupportedSemaphoreTypes`                       | property | `IReadOnlyList<string>` importable semaphore-handle kinds             |
|  [05]   | `ICompositionGpuInterop.IsLost`                 | property | `bool` interop device-context loss                                    |
|  [06]   | `ICompositionGpuImportedObject.ImportCompleted` | property | `Task` gating the free of a non-owning source                         |
|  [07]   | `ICompositionGpuImportedObject.IsLost`          | property | `bool` per-imported-object device-context loss                        |

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

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A control reads its backing visual through `ElementComposition.GetElementVisual(this)` and its `Compositor`; `GetElementVisual` returns null until the control enters a render tree, so import defers to the first composition update.
- `Compositor.TryGetCompositionGpuInterop()` yields the `ICompositionGpuInterop` capability; a null or `IsLost` result folds to the `Software` Skia-raster path.
- Adapter identity matches through `DeviceLuid`/`DeviceUuid`: the external device pins these byte arrays, and a mismatch forces a cross-adapter copy.
- Capsule construction mints the node once — `CreateDrawingSurface()` and `CreateSurfaceVisual()` — binds `CompositionSurfaceVisual.Surface` (change-tracked, render-thread serialized), and attaches through `SetElementChildVisual`, which throws across compositor instances.
- External backend rendering targets a shared texture (D3D11 keyed-mutex handle, Vulkan external `VkImage`, or Metal `IOSurface`) exported as one `SupportedImageHandleTypes` string; `ImportImage` binds it as `ICompositionImportedGpuImage` reading the memory window and channel order from `PlatformGraphicsExternalImageProperties`, and `ImportSemaphore` binds the matching fence.
- `GetSynchronizationCapabilities(imageHandleType)` selects the update member — `KeyedMutex` picks `UpdateWithKeyedMutexAsync`, `Semaphores` picks `UpdateWithSemaphoresAsync`, `TimelineSemaphores` picks `UpdateWithTimelineSemaphoresAsync`, `Automatic` picks `UpdateAsync` — and each `Task` completes when the render thread releases the image for reuse or disposal.

[STACKING]:
- `api-silk-webgpu`(`api-silk-webgpu.md`) / `api-silk-webgpu-wgpu`(`api-silk-webgpu-wgpu.md`): the `Wgpu` backend renders through `WebGPU.GetApi()` -> instance/adapter/device with the adapter matched to this interop's `DeviceLuid`/`DeviceUuid`; the rendered `Texture` exports as the platform handle `ImportImage` binds, and its export-format synchronization primitive selects the matching `UpdateWith*Async` member.
- `api-avalonia-skia`(`api-avalonia-skia.md`) / `api-skiasharp`(`api-skiasharp.md`): `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` shares Avalonia's own `GRContext` for the Skia-Ganesh families, while this interop imports an independently-rendered texture for the `Wgpu` family; the two are mutually exclusive `GpuBackend` rows and both fold to `Software` when their query returns null.
- within-lib: the AppUi viewport capsule mints and attaches the composition node once, pairs import-and-dispose per frame or resize, and threads tree mutations through `Compositor.RequestCompositionUpdate`.

[LOCAL_ADMISSION]:
- Interop path admits an externally-rendered (`Wgpu`) texture; the `ISkiaSharpApiLease` lease path admits a Skia-Ganesh render sharing Avalonia's `GRContext`, and each `GpuBackend` row selects exactly one.
- Imported images and semaphores are lifetime-scoped `IAsyncDisposable` handles exposing `ImportCompleted`/`IsLost`; the capsule awaits `ImportCompleted` before freeing a non-owning source, pairs import-and-dispose per frame or resize, and drops every handle across an `IsLost` transition.
- All interop work runs on the compositor render thread: import returns immediately-usable handles and `Update*Async` completes on the compositor loop, so the capsule awaits the `ValueTask`/`Task` rather than blocking the UI thread.

[RAIL_LAW]:
- Package: `Avalonia`
- Owns: the compositor GPU-interop seam — interop acquisition, external image/semaphore import, adapter-identity match, the surface/visual factory with its `SetElementChildVisual` attach, and the synchronization-discriminated `Update*Async` refresh for an externally-rendered viewport frame.
- Accept: `TryGetCompositionGpuInterop` query, `DeviceLuid`/`DeviceUuid` match, `ImportImage`/`ImportSemaphore` shared-handle import, `GetSynchronizationCapabilities`-selected `Update*Async`, the `Surface`-slot with its `SetElementChildVisual` attach.
- Reject: a second swapchain composited beside the Avalonia scene; an `Update*Async` member ignoring the `GetSynchronizationCapabilities` mode; a cross-adapter import ignoring the `DeviceLuid`/`DeviceUuid` match; freeing a source texture before `ImportCompleted`.

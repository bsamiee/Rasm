# [RASM_APPUI_API_AVALONIA_GPU_INTEROP]

The Avalonia 12 compositor GPU-interop seam imports an externally-rendered GPU texture into the composition tree: `Compositor.TryGetCompositionGpuInterop()` queries the platform render interface for the `ICompositionGpuInterop` capability, `ICompositionGpuInterop.ImportImage`/`ImportSemaphore` import a shared GPU handle and a synchronization primitive, and `CompositionDrawingSurface.UpdateWithExternalImage`/the imported-image update path binds the imported texture into an `ElementComposition.GetElementVisual` surface so a wgpu-rendered or D3D/Vulkan/Metal-rendered frame composites into the Avalonia scene without a second swapchain. This is the modern replacement for the bare `ISkiaSharpApiLease` shared-context path when an external GPU backend (the `Wgpu` family) owns the render.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Avalonia` (compositor)
- package: `Avalonia`
- assembly: `Avalonia.Base`
- namespace: `Avalonia.Rendering.Composition`
- asset: runtime library
- rail: visuals

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: compositor and GPU-interop owners
- rail: visuals

| [INDEX] | [SYMBOL]                                  | [KIND]            | [RAIL]                              |
| :-----: | :---------------------------------------- | :---------------- | :---------------------------------- |
|   [1]   | `Compositor`                              | composition root  | scene-graph owner, GPU query        |
|   [2]   | `ICompositionGpuInterop`                  | interop interface | external GPU image/semaphore import |
|   [3]   | `CompositionDrawingSurface`               | surface visual    | imported-image render target        |
|   [4]   | `CompositionSurfaceVisual`                | surface visual    | surface-backed visual node          |
|   [5]   | `ElementComposition`                      | static accessor   | element-to-visual bridge            |
|   [6]   | `ICompositionImportedGpuImage`            | imported handle   | imported texture lifetime           |
|   [7]   | `ICompositionImportedGpuSemaphore`        | imported handle   | imported semaphore lifetime         |
|   [8]   | `PlatformGraphicsExternalImageProperties` | struct            | imported image shape                |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: compositor acquisition and GPU-interop query
- rail: visuals

| [INDEX] | [SURFACE]                                     | [SURFACE_ROOT]       | [RAIL]                                                  |
| :-----: | :-------------------------------------------- | :------------------- | :------------------------------------------------------ |
|   [1]   | `ElementComposition.GetElementVisual(Visual)` | `ElementComposition` | element visual handle                                   |
|   [2]   | `CompositionVisual.Compositor`                | `CompositionVisual`  | compositor handle                                       |
|   [3]   | `Compositor.TryGetCompositionGpuInterop()`    | `Compositor`         | GPU-interop query (`ValueTask<ICompositionGpuInterop>`) |
|   [4]   | `Compositor.CreateDrawingSurface()`           | `Compositor`         | drawing surface create                                  |
|   [5]   | `Compositor.CreateSurfaceVisual()`            | `Compositor`         | surface visual create                                   |

[ENTRYPOINT_SCOPE]: external image and semaphore import
- rail: visuals

| [INDEX] | [SURFACE]                                                                              | [SURFACE_ROOT]              | [RAIL]                  |
| :-----: | :------------------------------------------------------------------------------------- | :-------------------------- | :---------------------- |
|   [1]   | `ImportImage(IPlatformHandle, PlatformGraphicsExternalImageProperties)`                | `ICompositionGpuInterop`    | import shared image     |
|   [2]   | `ImportImage(ICompositionImportableSharedGpuContextImage)`                             | `ICompositionGpuInterop`    | import context image    |
|   [3]   | `ImportSemaphore(IPlatformHandle)`                                                     | `ICompositionGpuInterop`    | import shared semaphore |
|   [4]   | `GetSynchronizationCapabilities(string imageHandleType)`                               | `ICompositionGpuInterop`    | sync capability query   |
|   [5]   | `CompositionDrawingSurface.UpdateWithExternalImageAsync(ICompositionImportedGpuImage)` | `CompositionDrawingSurface` | bind imported image     |

[ENTRYPOINT_SCOPE]: GPU-interop capability properties
- rail: visuals

| [INDEX] | [SURFACE]                   | [TYPE]                  | [RAIL]                      |
| :-----: | :-------------------------- | :---------------------- | :-------------------------- |
|   [1]   | `DeviceLuid`                | `byte[]?`               | adapter LUID (D3D match)    |
|   [2]   | `DeviceUuid`                | `byte[]?`               | adapter UUID (Vulkan match) |
|   [3]   | `SupportedImageHandleTypes` | `IReadOnlyList<string>` | importable handle kinds     |
|   [4]   | `SupportedSemaphoreTypes`   | `IReadOnlyList<string>` | importable semaphore kinds  |
|   [5]   | `IsLost`                    | `bool`                  | device-context availability |

## [4]-[IMPLEMENTATION_LAW]

[INTEROP_TOPOLOGY]:
- A control obtains its compositor by `ElementComposition.GetElementVisual(this)` then reading `.Compositor`; `Compositor.TryGetCompositionGpuInterop()` returns a `ValueTask<ICompositionGpuInterop>` — a null/lost result is the no-GPU floor that folds to the `Software` raster path.
- The `DeviceLuid`/`DeviceUuid` properties identify the compositor's graphics adapter so an external wgpu/D3D/Vulkan device is created on the SAME adapter — a mismatched adapter is the cross-adapter copy penalty the LUID/UUID match avoids.
- The external backend renders into a shared GPU texture (D3D11 keyed-mutex shared handle, Vulkan external-memory `VkImage`, Metal `IOSurface`), exports a platform handle, and `ImportImage(IPlatformHandle, PlatformGraphicsExternalImageProperties)` imports it as an `ICompositionImportedGpuImage`; `ImportSemaphore` imports the matching fence so the compositor waits for the external render to finish before it composites the imported image.
- `CompositionDrawingSurface.UpdateWithExternalImageAsync(importedImage)` binds the imported texture into a `CompositionSurfaceVisual` placed in the element tree, so the wgpu-rendered viewport frame appears in the Avalonia scene with one synchronized texture, never a second window or a copy through `SKImage`.
- `SupportedImageHandleTypes`/`SupportedSemaphoreTypes` enumerate the platform's importable handle kinds (e.g. `D3D11TextureGlobalSharedHandle`, `VulkanOpaquePosixFileDescriptor`, `IOSurfaceRef`) so the external backend selects an export format the compositor accepts.

[LOCAL_ADMISSION]:
- The GPU-interop path is the modern compositor seam superseding bare `ISkiaSharpApiLease.TryLeasePlatformGraphicsApi` for an external-backend render: the lease path shares Avalonia's own `GRContext` for in-Skia draw, while the interop path imports an independently-rendered external texture — the `Wgpu` `GpuBackend` family renders with its own wgpu device and presents through `ImportImage`, the SkiaSharp Ganesh families render through the leased `GRContext`.
- Imported images and semaphores are lifetime-scoped handles released through their `DisposeAsync`; the boundary capsule pairs import-and-dispose per frame or per resize, never leaking an imported handle across a device-lost (`IsLost`) transition.
- All GPU-interop work runs on the compositor's render thread; the import calls are async and complete on the compositor loop, so the boundary capsule awaits the `ValueTask` rather than blocking the UI thread.

[RAIL_LAW]:
- Package: `Avalonia` (`Avalonia.Base` compositor)
- Owns: the GPU-interop seam — compositor acquisition, external GPU image/semaphore import, adapter identity match, and the imported-image-to-surface-visual bind for an externally-rendered (`Wgpu`) viewport frame.
- Accept: `TryGetCompositionGpuInterop` query, LUID/UUID adapter match, `ImportImage`/`ImportSemaphore` shared-handle import, `UpdateWithExternalImageAsync` surface bind.
- Reject: a second swapchain or window composited beside the Avalonia scene; an unsynchronized imported image without a matching imported semaphore; a cross-adapter import ignoring the `DeviceLuid`/`DeviceUuid` match.

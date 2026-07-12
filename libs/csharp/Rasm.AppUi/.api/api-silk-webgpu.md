# [RASM_APPUI_API_SILK_WEBGPU]

`Silk.NET.WebGPU` is the managed wgpu/Dawn binding generated against the canonical `webgpu.h` headers: `WebGPU` is the static API entry exposing the global `CreateInstance`, `Instance` requests an `Adapter`, an `Adapter` requests a `Device`, a `Device` mints `Queue`/`ShaderModule`/`Buffer`/`Texture`/`RenderPipeline`/`ComputePipeline`/`BindGroup` resources, a `Surface` configures a swapchain and yields the per-frame `SurfaceTexture`, and `CommandEncoder` records `RenderPassEncoder`/`ComputePassEncoder` passes the `Queue` submits. `Silk.NET.WebGPU.Native.WGPU` carries the native `wgpu_native` (Dawn-compatible) runtime for the win/linux/osx RIDs the binding P/Invokes into.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.WebGPU`
- package: `Silk.NET.WebGPU`
- package: `Silk.NET.WebGPU.Native.WGPU` (native `wgpu_native` runtime, RID-fanned)
- license: MIT (expression)
- assembly: `Silk.NET.WebGPU`
- namespace: `Silk.NET.WebGPU` (function-table root, descriptors, enums, callback delegates, platform-source structs)
- namespace: `Silk.NET.WebGPU.Platforms.MacOS` (`NSWindow`/`ObjectiveCRuntime` Metal-layer plumbing for `SurfaceDescriptorFromMetalLayer`)
- target: `lib/net5.0` is the highest TFM shipped; the `net10.0` consumer binds it (Silk.NET 2.x tops out at `net5.0`/`netstandard2.1`)
- asset: runtime library + native wgpu binaries
- depends: `Silk.NET.Core`, `Silk.NET.Maths`
- rail: visuals
- note: there is no `Silk.NET.WebGPU.Extensions.Dawn` package or namespace in this binding — the admitted vendor extension is `Silk.NET.WebGPU.Extensions.WGPU` (`api-silk-webgpu-wgpu.md`), which carries `DevicePoll`, native log callbacks, full-adapter enumeration, indirect multi-draw, and push-constants. The native backend is `wgpu_native` (`Silk.NET.WebGPU.Native.WGPU`); `BackendType` then auto-negotiates D3D12/Vulkan/Metal under it.

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: API root and device lifecycle owners
- rail: visuals

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [RAIL]                        |
| :-----: | :--------- | :-------------- | :---------------------------- |
|  [01]   | `WebGPU`   | static API root | global entry, function table  |
|  [02]   | `Instance` | native handle   | adapter request root          |
|  [03]   | `Adapter`  | native handle   | device request, feature query |
|  [04]   | `Device`   | native handle   | resource factory              |
|  [05]   | `Queue`    | native handle   | command submission            |
|  [06]   | `Surface`  | native handle   | swapchain present target      |

[PUBLIC_TYPE_SCOPE]: resource and pipeline owners
- rail: visuals

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                    |
| :-----: | :------------------- | :------------ | :------------------------ |
|  [01]   | `Buffer`             | native handle | GPU buffer allocation     |
|  [02]   | `Texture`            | native handle | GPU texture allocation    |
|  [03]   | `TextureView`        | native handle | texture view              |
|  [04]   | `Sampler`            | native handle | texture sampler           |
|  [05]   | `ShaderModule`       | native handle | WGSL/SPIR-V shader        |
|  [06]   | `BindGroup`          | native handle | bound resource group      |
|  [07]   | `BindGroupLayout`    | native handle | binding layout            |
|  [08]   | `PipelineLayout`     | native handle | pipeline layout           |
|  [09]   | `RenderPipeline`     | native handle | raster pipeline           |
|  [10]   | `ComputePipeline`    | native handle | compute pipeline          |
|  [11]   | `CommandEncoder`     | native handle | command recorder          |
|  [12]   | `RenderPassEncoder`  | native handle | raster pass recorder      |
|  [13]   | `ComputePassEncoder` | native handle | compute pass recorder     |
|  [14]   | `CommandBuffer`      | native handle | finished command buffer   |
|  [15]   | `SurfaceTexture`     | struct        | per-frame swapchain image |

[PUBLIC_TYPE_SCOPE]: descriptor and enum value carriers
- rail: visuals

| [INDEX] | [SYMBOL]                                   | [KIND]       | [RAIL]                    |
| :-----: | :----------------------------------------- | :----------- | :------------------------ |
|  [01]   | `InstanceDescriptor`                       | descriptor   | instance options          |
|  [02]   | `RequestAdapterOptions`                    | descriptor   | adapter request           |
|  [03]   | `DeviceDescriptor`                         | descriptor   | device request            |
|  [04]   | `SurfaceConfiguration`                     | descriptor   | swapchain configuration   |
|  [05]   | `SurfaceCapabilities`                      | struct       | surface capabilities      |
|  [06]   | `TextureDescriptor`                        | descriptor   | texture allocation        |
|  [07]   | `TextureViewDescriptor`                    | descriptor   | texture-view allocation   |
|  [08]   | `BufferDescriptor`                         | descriptor   | buffer allocation         |
|  [09]   | `RenderPipelineDescriptor`                 | descriptor   | raster-pipeline creation  |
|  [10]   | `ComputePipelineDescriptor`                | descriptor   | compute-pipeline creation |
|  [11]   | `RenderPassDescriptor`                     | descriptor   | render-pass creation      |
|  [12]   | `QuerySetDescriptor`                       | descriptor   | query-set creation        |
|  [13]   | `SurfaceDescriptor`                        | descriptor   | surface source chain      |
|  [14]   | `SurfaceDescriptorFromMetalLayer`          | descriptor   | Metal surface source      |
|  [15]   | `SurfaceDescriptorFromWindowsHWND`         | descriptor   | Windows surface source    |
|  [16]   | `SurfaceDescriptorFromXlibWindow`          | descriptor   | Xlib surface source       |
|  [17]   | `SurfaceDescriptorFromWaylandSurface`      | descriptor   | Wayland surface source    |
|  [18]   | `SurfaceDescriptorFromXcbWindow`           | descriptor   | XCB surface source        |
|  [19]   | `SurfaceDescriptorFromAndroidNativeWindow` | descriptor   | Android surface source    |
|  [20]   | `ChainedStruct`                            | struct       | input extension chain     |
|  [21]   | `ChainedStructOut`                         | struct       | output extension chain    |
|  [22]   | `PfnRequestAdapterCallback`                | delegate ptr | adapter result            |
|  [23]   | `PfnRequestDeviceCallback`                 | delegate ptr | device result             |
|  [24]   | `PfnBufferMapCallback`                     | delegate ptr | buffer-map result         |
|  [25]   | `PfnQueueWorkDoneCallback`                 | delegate ptr | queue-work result         |
|  [26]   | `PfnErrorCallback`                         | delegate ptr | validation error          |
|  [27]   | `PfnDeviceLostCallback`                    | delegate ptr | device-lost result        |
|  [28]   | `TextureFormat`                            | enum         | pixel format              |
|  [29]   | `PresentMode`                              | enum         | present mode              |
|  [30]   | `BackendType`                              | enum         | graphics backend          |
|  [31]   | `FeatureName`                              | enum         | device feature            |
|  [32]   | `WGSLFeatureName`                          | enum         | WGSL feature              |
|  [33]   | `QueryType`                                | enum         | query family              |

[DESCRIPTOR_FIELDS]:
- `InstanceDescriptor`: instance creation options.
- `RequestAdapterOptions`: power preference and compatible surface.
- `DeviceDescriptor`: requested limits and features.
- `SurfaceConfiguration`: format and present mode.
- `SurfaceCapabilities`: supported formats and present modes.
- `QuerySetDescriptor`: timestamp and pipeline-statistics query sets.

[ENUM_VALUES]:
- `TextureFormat`: surface and texture pixel formats.
- `PresentMode`: swapchain present modes.
- `BackendType`: D3D12, Vulkan, Metal, OpenGL, and WGPU.
- `FeatureName` / `WGSLFeatureName`: device and WGSL feature flags.
- `QueryType`: `Timestamp` and `Occlusion`.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: API root and instance creation
- rail: visuals

| [INDEX] | [SURFACE]                                                                                  | [SURFACE_ROOT] | [RAIL]                 |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :--------------------- |
|  [01]   | `WebGPU.GetApi()`                                                                          | `WebGPU`       | API root load          |
|  [02]   | `CreateInstance(InstanceDescriptor*)`                                                      | `WebGPU`       | instance create        |
|  [03]   | `InstanceRequestAdapter(Instance*, RequestAdapterOptions*, PfnRequestAdapterCallback, ud)` | `WebGPU`       | adapter request        |
|  [04]   | `AdapterRequestDevice(Adapter*, DeviceDescriptor*, PfnRequestDeviceCallback, ud)`          | `WebGPU`       | device request         |
|  [05]   | `DeviceGetQueue(Device*)`                                                                  | `WebGPU`       | queue handle           |
|  [06]   | `AdapterEnumerateFeatures / AdapterHasFeature / AdapterGetLimits`                          | `WebGPU`       | adapter capability     |
|  [07]   | `DeviceEnumerateFeatures / DeviceHasFeature / DeviceGetLimits`                             | `WebGPU`       | device capability      |
|  [08]   | `InstanceProcessEvents(Instance*)`                                                         | `WebGPU`       | standard event pump    |
|  [09]   | `DeviceSetUncapturedErrorCallback / DevicePushErrorScope / DevicePopErrorScope`            | `WebGPU`       | validation error scope |

[ENTRYPOINT_SCOPE]: surface, swapchain, and present
- rail: visuals

| [INDEX] | [SURFACE]                                                          | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :----------------------------------------------------------------- | :------------- | :------------------ |
|  [01]   | `InstanceCreateSurface(Instance*, SurfaceDescriptor*)`             | `WebGPU`       | surface from window |
|  [02]   | `SurfaceConfigure(Surface*, SurfaceConfiguration*)`                | `WebGPU`       | swapchain configure |
|  [03]   | `SurfaceGetCurrentTexture(Surface*, SurfaceTexture*)`              | `WebGPU`       | acquire frame image |
|  [04]   | `TextureCreateView(Texture*, TextureViewDescriptor*)`              | `WebGPU`       | frame view          |
|  [05]   | `SurfacePresent(Surface*)`                                         | `WebGPU`       | present frame       |
|  [06]   | `SurfaceGetCapabilities(Surface*, Adapter*, SurfaceCapabilities*)` | `WebGPU`       | format/mode query   |

[ENTRYPOINT_SCOPE]: command recording and submission
- rail: visuals

| [INDEX] | [SURFACE]                                                                 | [SURFACE_ROOT] | [RAIL]                |
| :-----: | :------------------------------------------------------------------------ | :------------- | :-------------------- |
|  [01]   | `DeviceCreateCommandEncoder(Device*, CommandEncoderDescriptor*)`          | `WebGPU`       | encoder create        |
|  [02]   | `CommandEncoderBeginRenderPass(CommandEncoder*, RenderPassDescriptor*)`   | `WebGPU`       | render pass begin     |
|  [03]   | `CommandEncoderBeginComputePass(CommandEncoder*, ComputePassDescriptor*)` | `WebGPU`       | compute pass begin    |
|  [04]   | `RenderPassEncoderSetPipeline / SetBindGroup / Draw / DrawIndexed`        | `WebGPU`       | raster draw           |
|  [05]   | `ComputePassEncoderSetPipeline / DispatchWorkgroups`                      | `WebGPU`       | compute dispatch      |
|  [06]   | `CommandEncoderFinish(CommandEncoder*, CommandBufferDescriptor*)`         | `WebGPU`       | finish command buffer |
|  [07]   | `QueueSubmit(Queue*, nuint, CommandBuffer**)`                             | `WebGPU`       | submit to GPU         |
|  [08]   | `QueueWriteBuffer / QueueWriteTexture`                                    | `WebGPU`       | host-to-GPU upload    |

[ENTRYPOINT_SCOPE]: resource and pipeline creation
- rail: visuals

| [INDEX] | [SURFACE]                                                          | [SURFACE_ROOT] | [RAIL]                |
| :-----: | :----------------------------------------------------------------- | :------------- | :-------------------- |
|  [01]   | `DeviceCreateBuffer(Device*, BufferDescriptor*)`                   | `WebGPU`       | buffer alloc          |
|  [02]   | `DeviceCreateTexture(Device*, TextureDescriptor*)`                 | `WebGPU`       | texture alloc         |
|  [03]   | `DeviceCreateShaderModule(Device*, ShaderModuleDescriptor*)`       | `WebGPU`       | shader compile (WGSL) |
|  [04]   | `DeviceCreateRenderPipeline(Device*, RenderPipelineDescriptor*)`   | `WebGPU`       | raster pipeline       |
|  [05]   | `DeviceCreateComputePipeline(Device*, ComputePipelineDescriptor*)` | `WebGPU`       | compute pipeline      |
|  [06]   | `DeviceCreateBindGroup(Device*, BindGroupDescriptor*)`             | `WebGPU`       | bind group            |

[ENTRYPOINT_SCOPE]: timing and pipeline-statistics queries
- rail: evidence

The core query set admits `QueryType.Timestamp` and `QueryType.Occlusion`.

| [INDEX] | [SURFACE]                                                                                  | [SURFACE_ROOT] | [RAIL]                 |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :--------------------- |
|  [01]   | `DeviceCreateQuerySet(Device*, QuerySetDescriptor*)`                                       | `WebGPU`       | query-set allocation   |
|  [02]   | `CommandEncoderWriteTimestamp(CommandEncoder*, QuerySet*, queryIndex)`                     | `WebGPU`       | per-pass timestamp     |
|  [03]   | `RenderPassTimestampWrites`                                                                | `WebGPU`       | render-pass timestamp  |
|  [04]   | `ComputePassTimestampWrites`                                                               | `WebGPU`       | compute-pass timestamp |
|  [05]   | `CommandEncoderResolveQuerySet(CommandEncoder*, QuerySet*, first, count, Buffer*, offset)` | `WebGPU`       | resolve to read buffer |
|  [06]   | `BufferMapAsync(Buffer*, MapMode, offset, size, PfnBufferMapCallback, ud)`                 | `WebGPU`       | async readback map     |
|  [07]   | `BufferGetMappedRange(Buffer*, offset, size)`                                              | `WebGPU`       | resolved-tick access   |
|  [08]   | `BufferUnmap(Buffer*)`                                                                     | `WebGPU`       | mapped-range release   |
|  [09]   | `QuerySetRelease(QuerySet*)`                                                               | `WebGPU`       | query-set release      |

## [04]-[IMPLEMENTATION_LAW]

[WEBGPU_TOPOLOGY]:
- `WebGPU.GetApi()` returns the function-table root; every native call is an instance method on that `WebGPU` object taking raw pointers to descriptor structs — Silk.NET binds the C `webgpu.h` surface directly, so a call site marshals `Span<T>`/`stackalloc` descriptor structs and passes pointers, never a managed wrapper object.
- The lifecycle is `Instance` -> `Adapter` (async request through a callback) -> `Device` + `Queue` -> resources; the adapter/device requests complete through native callbacks that Silk.NET surfaces as `PfnRequestAdapterCallback`/`PfnRequestDeviceCallback` delegate pointers.
- `Surface` is created by `InstanceCreateSurface(instance, surfaceDescriptor)` where the `SurfaceDescriptor.NextInChain` carries the platform-source struct for the window handle — `SurfaceDescriptorFromMetalLayer` (a `CAMetalLayer` from `Platforms.MacOS.NSWindow`), `SurfaceDescriptorFromWindowsHWND`, `SurfaceDescriptorFromXlibWindow`/`…FromWaylandSurface`/`…FromXcbWindow`, or `…FromAndroidNativeWindow` — then configured with a `SurfaceConfiguration` (format, present mode, usage, width, height); each frame `SurfaceGetCurrentTexture` yields a `SurfaceTexture` whose `Texture` is viewed, rendered into through a `RenderPassEncoder`, and presented through `SurfacePresent`. The Avalonia path imports the rendered texture through `ICompositionGpuInterop.ImportImage` (`api-avalonia-gpu-interop.md`) rather than presenting a swapchain beside Avalonia's.
- `BackendType` auto-negotiates D3D12 (Windows), Metal (macOS), Vulkan (Linux), or the in-browser WebGPU target over the `wgpu_native` runtime — the same wgpu surface the archived VelloSharp wrapped, here owned at the binding tier with a live pinnable identity. `AdapterEnumerateFeatures`/`AdapterHasFeature`/`AdapterGetLimits` negotiate the feature/limit set (gating `timestamp-query`) before `AdapterRequestDevice` requests them in `DeviceDescriptor`.
- WGSL is the shader source for `DeviceCreateShaderModule`; compute shaders dispatch through `ComputePassEncoderDispatchWorkgroups` and raster pipelines draw through `RenderPassEncoderDraw`/`DrawIndexed`, so the meshlet mesh-shader and path-trace compute passes bind to one wgpu pipeline family.
- Validation rides the error-scope rail: `DeviceSetUncapturedErrorCallback` installs the global `PfnErrorCallback`, and `DevicePushErrorScope`/`DevicePopErrorScope` bracket a suspect operation so a validation/out-of-memory error is a counted `ViewportFault` on the telemetry spine, not a swallowed native abort — the wgpu-native `SetLogCallback` (`api-silk-webgpu-wgpu.md`) routes the lower-level adapter/device-lost stream beside it.

[TimestampQuery]:

- Admit: `QueryType.Timestamp` requires `timestamp-query`; core `QueryType` also exposes `Occlusion`, while pipeline statistics remains wgpu-native-only.
- Record: `CommandEncoderWriteTimestamp` or the `RenderPassTimestampWrites`/`ComputePassTimestampWrites` begin-descriptor entries record GPU wall time at pass boundaries.
- Resolve: `CommandEncoderResolveQuerySet` copies the resolved nanosecond ticks into a mappable read buffer.
- Read: `BufferMapAsync` -> `BufferGetMappedRange` -> `BufferUnmap` reads the resolved ticks.
- Retire: the wgpu-native `DevicePoll` (`api-silk-webgpu-wgpu.md`) or the standard `InstanceProcessEvents` retires the map callback.
- Measure: the resolved per-pass GPU duration is the measured timeline beside the encoder-projected CPU duration, never a busy-wait fence.

[LOCAL_ADMISSION]:
- All native handles (`Instance`, `Adapter`, `Device`, `Queue`, `Surface`, `Buffer`, `Texture`, pipelines, encoders) are released through their matching `XxxRelease`/`XxxDestroy` native call, not `IDisposable` — the owning boundary capsule pairs create-and-release in a `using`-equivalent scoped fold.
- `Silk.NET.WebGPU.Native.WGPU` supplies the native `wgpu_native` runtime for win-x64/win-arm64/linux-x64/linux-arm64/osx-x64/osx-arm64 — the one native asset this binding P/Invokes. There is no admitted Dawn package; `BackendType` selects D3D12/Vulkan/Metal under `wgpu_native`, and the wgpu-native-only entrypoints (poll, log, indirect multi-draw, push-constants) come from `Silk.NET.WebGPU.Extensions.WGPU` (`api-silk-webgpu-wgpu.md`), a second function-table view over the same runtime.
- The WebGPU surface integrates with Avalonia 12 through the compositor GPU-interop seam (`api-avalonia-gpu-interop.md`): the wgpu-rendered texture imports into a `CompositionDrawingSurface` through `ICompositionGpuInterop.ImportImage`, never a second swapchain composited beside Avalonia's.

[RAIL_LAW]:
- Package: `Silk.NET.WebGPU` (+ `Silk.NET.WebGPU.Native.WGPU`, with the `Silk.NET.WebGPU.Extensions.WGPU` vendor surface as the second function-table view)
- Owns: the managed `wgpu_native` binding — instance/adapter/device lifecycle, feature/limit negotiation, surface-from-window-handle and swapchain, buffer/texture/sampler allocation, WGSL shader compile, render/compute pipeline create, command recording, queue submission, error-scope validation, and timestamp query timing for the `Wgpu` `GpuBackend` family.
- Accept: raw-pointer descriptor calls on the `WebGPU.GetApi()` function-table root; `InstanceCreateSurface` with a platform-source `next` chain; feature/limit query before device request; error-scope-bracketed validation into the receipt sink; native-handle scoped create-and-release pairs at the boundary capsule; texture import into the Avalonia compositor via `ICompositionGpuInterop.ImportImage`.
- Reject: a managed convenience wrapper renaming the native surface; a `Silk.NET.WebGPU.Extensions.Dawn` reference (not admitted); a `QueryType.PipelineStatistics` core value (wgpu-native-only); mixing SkiaSharp `GRContext` Ganesh calls into the wgpu pipeline — the SkiaSharp Ganesh families (`Metal`/`Vulkan`/`OpenGl`/`Software`) and the `Wgpu` family are parallel backend rows the `RenderTargetFactory` column dispatches between.

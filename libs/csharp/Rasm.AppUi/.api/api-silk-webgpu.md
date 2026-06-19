# [RASM_APPUI_API_SILK_WEBGPU]

`Silk.NET.WebGPU` is the managed wgpu/Dawn binding generated against the canonical `webgpu.h` headers: `WebGPU` is the static API entry exposing the global `CreateInstance`, `Instance` requests an `Adapter`, an `Adapter` requests a `Device`, a `Device` mints `Queue`/`ShaderModule`/`Buffer`/`Texture`/`RenderPipeline`/`ComputePipeline`/`BindGroup` resources, a `Surface` configures a swapchain and yields the per-frame `SurfaceTexture`, and `CommandEncoder` records `RenderPassEncoder`/`ComputePassEncoder` passes the `Queue` submits. `Silk.NET.WebGPU.Native.WGPU` carries the native `wgpu_native` (Dawn-compatible) runtime for the win/linux/osx RIDs the binding P/Invokes into.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.WebGPU`
- package: `Silk.NET.WebGPU`
- package: `Silk.NET.WebGPU.Native.WGPU`
- assembly: `Silk.NET.WebGPU`
- namespace: `Silk.NET.WebGPU`
- namespace: `Silk.NET.WebGPU.Extensions.Dawn`
- asset: runtime library + native wgpu binaries
- rail: visuals

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

| [INDEX] | [SYMBOL]                    | [KIND]     | [RAIL]                           |
| :-----: | :-------------------------- | :--------- | :------------------------------- |
|  [01]   | `InstanceDescriptor`        | descriptor | instance create options          |
|  [02]   | `RequestAdapterOptions`     | descriptor | adapter request (power, surface) |
|  [03]   | `DeviceDescriptor`          | descriptor | device create (limits, features) |
|  [04]   | `SurfaceConfiguration`      | descriptor | swapchain config (format, mode)  |
|  [05]   | `TextureDescriptor`         | descriptor | texture allocation               |
|  [06]   | `BufferDescriptor`          | descriptor | buffer allocation                |
|  [07]   | `RenderPipelineDescriptor`  | descriptor | raster pipeline create           |
|  [08]   | `ComputePipelineDescriptor` | descriptor | compute pipeline create          |
|  [09]   | `RenderPassDescriptor`      | descriptor | render pass begin                |
|  [10]   | `TextureFormat`             | enum       | surface/texture pixel format     |
|  [11]   | `PresentMode`               | enum       | swapchain present mode           |
|  [12]   | `BackendType`               | enum       | D3D12/Vulkan/Metal/OpenGL/WGPU   |
|  [13]   | `FeatureName`               | enum       | device feature flags             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: API root and instance creation
- rail: visuals

| [INDEX] | [SURFACE]                                                                       | [SURFACE_ROOT] | [RAIL]          |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :-------------- |
|  [01]   | `WebGPU.GetApi()`                                                               | `WebGPU`       | API root load   |
|  [02]   | `CreateInstance(InstanceDescriptor*)`                                           | `WebGPU`       | instance create |
|  [03]   | `InstanceRequestAdapter(Instance*, RequestAdapterOptions*, callback, userdata)` | `WebGPU`       | adapter request |
|  [04]   | `AdapterRequestDevice(Adapter*, DeviceDescriptor*, callback, userdata)`         | `WebGPU`       | device request  |
|  [05]   | `DeviceGetQueue(Device*)`                                                       | `WebGPU`       | queue handle    |

[ENTRYPOINT_SCOPE]: surface, swapchain, and present
- rail: visuals

| [INDEX] | [SURFACE]                                                          | [SURFACE_ROOT] | [RAIL]              |
| :-----: | :----------------------------------------------------------------- | :------------- | :------------------ |
|  [01]   | `SurfaceConfigure(Surface*, SurfaceConfiguration*)`                | `WebGPU`       | swapchain configure |
|  [02]   | `SurfaceGetCurrentTexture(Surface*, SurfaceTexture*)`              | `WebGPU`       | acquire frame image |
|  [03]   | `TextureCreateView(Texture*, TextureViewDescriptor*)`              | `WebGPU`       | frame view          |
|  [04]   | `SurfacePresent(Surface*)`                                         | `WebGPU`       | present frame       |
|  [05]   | `SurfaceGetCapabilities(Surface*, Adapter*, SurfaceCapabilities*)` | `WebGPU`       | format/mode query   |

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

| [INDEX] | [SURFACE]                                                                                  | [SURFACE_ROOT] | [RAIL]                   |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :----------------------- |
|  [01]   | `DeviceCreateQuerySet(Device*, QuerySetDescriptor*)`                                       | `WebGPU`       | query-set alloc          |
|  [02]   | `CommandEncoderWriteTimestamp(CommandEncoder*, QuerySet*, queryIndex)`                     | `WebGPU`       | per-pass timestamp write |
|  [03]   | `CommandEncoderResolveQuerySet(CommandEncoder*, QuerySet*, first, count, Buffer*, offset)` | `WebGPU`       | resolve to read buffer   |
|  [04]   | `QuerySetDescriptor` (`QueryType.Timestamp` / `QueryType.PipelineStatistics`)              | `WebGPU`       | query-set descriptor     |
|  [05]   | `QuerySetRelease(QuerySet*)`                                                               | `WebGPU`       | query-set release        |

## [04]-[IMPLEMENTATION_LAW]

[WEBGPU_TOPOLOGY]:
- `WebGPU.GetApi()` returns the function-table root; every native call is an instance method on that `WebGPU` object taking raw pointers to descriptor structs — Silk.NET binds the C `webgpu.h` surface directly, so a call site marshals `Span<T>`/`stackalloc` descriptor structs and passes pointers, never a managed wrapper object.
- The lifecycle is `Instance` -> `Adapter` (async request through a callback) -> `Device` + `Queue` -> resources; the adapter/device requests complete through native callbacks that Silk.NET surfaces as `PfnRequestAdapterCallback`/`PfnRequestDeviceCallback` delegate pointers.
- `Surface` is created from a platform window handle (the raw `IntPtr`/`ICompositionGpuInterop` shared handle), configured with a `SurfaceConfiguration` (format, present mode, usage, width, height), and each frame `SurfaceGetCurrentTexture` yields a `SurfaceTexture` whose `Texture` is viewed, rendered into through a `RenderPassEncoder`, and presented through `SurfacePresent`.
- `BackendType` auto-negotiates D3D12 (Windows), Metal (macOS), Vulkan (Linux), or the in-browser WebGPU target — the same wgpu surface the archived VelloSharp wrapped, here owned at the binding tier with a live pinnable identity.
- WGSL is the shader source for `DeviceCreateShaderModule`; compute shaders dispatch through `ComputePassEncoderDispatchWorkgroups` and raster pipelines draw through `RenderPassEncoderDraw`/`DrawIndexed`, so the meshlet mesh-shader and path-trace compute passes bind to one wgpu pipeline family.
- A `QuerySet` of `QueryType.Timestamp` records GPU-side wall time at pass boundaries through `CommandEncoderWriteTimestamp` (or the `RenderPassTimestampWrites`/`ComputePassTimestampWrites` begin-descriptor entries on `timestamp-query` feature support), and `CommandEncoderResolveQuerySet` copies the resolved nanosecond ticks into a mappable read buffer the `DevicePoll` non-blocking advance retires — the resolved per-pass GPU duration is the measured timeline beside the encoder-projected CPU duration, never a busy-wait fence; a `QueryType.PipelineStatistics` set captures vertices-shaded, primitives-culled, and fragment-invocations for per-pass bottleneck attribution.

[LOCAL_ADMISSION]:
- All native handles (`Instance`, `Adapter`, `Device`, `Queue`, `Surface`, `Buffer`, `Texture`, pipelines, encoders) are released through their matching `XxxRelease`/`XxxDestroy` native call, not `IDisposable` — the owning boundary capsule pairs create-and-release in a `using`-equivalent scoped fold.
- `Silk.NET.WebGPU.Native.WGPU` supplies the native `wgpu_native` runtime for win-x64/win-arm64/linux-x64/linux-arm64/osx-x64/osx-arm64; the `Silk.NET.WebGPU.Extensions.Dawn` package swaps the native runtime to Google's Dawn for D3D12/Vulkan parity where wgpu-native lags.
- The WebGPU surface integrates with Avalonia 12 through the compositor GPU-interop seam (`api-avalonia-gpu-interop.md`): the wgpu-rendered texture imports into a `CompositionDrawingSurface` through `ICompositionGpuInterop.ImportImage`, never a second swapchain composited beside Avalonia's.

[RAIL_LAW]:
- Package: `Silk.NET.WebGPU` (+ `Silk.NET.WebGPU.Native.WGPU`)
- Owns: the managed wgpu/Dawn binding — instance/adapter/device lifecycle, surface swapchain, buffer/texture/sampler allocation, WGSL shader compile, render/compute pipeline create, command recording, and queue submission for the `Wgpu` `GpuBackend` family.
- Accept: raw-pointer descriptor calls on the `WebGPU.GetApi()` function-table root; native-handle scoped create-and-release pairs at the boundary capsule.
- Reject: a managed convenience wrapper renaming the native surface; mixing SkiaSharp `GRContext` Ganesh calls into the wgpu pipeline — the SkiaSharp Ganesh families (`Metal`/`Vulkan`/`OpenGl`/`Software`) and the `Wgpu` family are parallel backend rows the `RenderTargetFactory` column dispatches between.

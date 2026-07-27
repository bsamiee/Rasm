# [RASM_APPUI_API_SILK_WEBGPU]

`Silk.NET.WebGPU` binds the native `wgpu_native` runtime against the canonical `webgpu.h` surface: `WebGPU.GetApi()` roots a function table whose instance methods marshal raw-pointer descriptor structs across the instance/adapter/device/queue lifecycle onto the visuals rail. `SurfaceGetCurrentTexture` output imports into the Avalonia compositor as an external texture, never a swapchain composited beside Avalonia's.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.WebGPU`
- package: `Silk.NET.WebGPU` (MIT)
- package: `Silk.NET.WebGPU.Native.WGPU` (MIT)
- assembly: `Silk.NET.WebGPU`
- namespace: `Silk.NET.WebGPU` (function-table root, descriptors, enums, callback delegates, platform-source structs)
- namespace: `Silk.NET.WebGPU.Platforms.MacOS` (`NSWindow`/`ObjectiveCRuntime` Metal-layer plumbing for `SurfaceDescriptorFromMetalLayer`)
- asset: consumer binds the runtime library; `Silk.NET.WebGPU.Native.WGPU` fans the native `wgpu_native` binaries across the win/linux/osx RIDs
- depends: `Silk.NET.Core`, `Silk.NET.Maths`
- rail: visuals

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: API root and device-lifecycle owners

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [CAPABILITY]                  |
| :-----: | :--------- | :-------------- | :---------------------------- |
|  [01]   | `WebGPU`   | static API root | global entry, function table  |
|  [02]   | `Instance` | native handle   | adapter request root          |
|  [03]   | `Adapter`  | native handle   | device request, feature query |
|  [04]   | `Device`   | native handle   | resource factory              |
|  [05]   | `Queue`    | native handle   | command submission            |
|  [06]   | `Surface`  | native handle   | swapchain present target      |

[PUBLIC_TYPE_SCOPE]: resource and pipeline owners

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]              |
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

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY] | [CAPABILITY]                 |
| :-----: | :----------------------------------------- | :------------ | :--------------------------- |
|  [01]   | `InstanceDescriptor`                       | descriptor    | instance options             |
|  [02]   | `RequestAdapterOptions`                    | descriptor    | adapter request              |
|  [03]   | `DeviceDescriptor`                         | descriptor    | device request               |
|  [04]   | `SurfaceConfiguration`                     | descriptor    | swapchain configuration      |
|  [05]   | `SurfaceCapabilities`                      | struct        | surface capabilities         |
|  [06]   | `TextureDescriptor`                        | descriptor    | texture allocation           |
|  [07]   | `TextureViewDescriptor`                    | descriptor    | texture-view allocation      |
|  [08]   | `BufferDescriptor`                         | descriptor    | buffer allocation            |
|  [09]   | `RenderPipelineDescriptor`                 | descriptor    | raster-pipeline creation     |
|  [10]   | `ComputePipelineDescriptor`                | descriptor    | compute-pipeline creation    |
|  [11]   | `RenderPassDescriptor`                     | descriptor    | render-pass creation         |
|  [12]   | `QuerySetDescriptor`                       | descriptor    | query-set creation           |
|  [13]   | `RenderPassTimestampWrites`                | descriptor    | render-pass timestamp slots  |
|  [14]   | `ComputePassTimestampWrites`               | descriptor    | compute-pass timestamp slots |
|  [15]   | `ImageCopyTexture`                         | descriptor    | texture upload destination   |
|  [16]   | `TextureDataLayout`                        | descriptor    | texture upload row pitch     |
|  [17]   | `Extent3D`                                 | struct        | texture upload extent        |
|  [18]   | `SurfaceDescriptor`                        | descriptor    | surface source chain         |
|  [19]   | `SurfaceDescriptorFromMetalLayer`          | descriptor    | Metal surface source         |
|  [20]   | `SurfaceDescriptorFromWindowsHWND`         | descriptor    | Windows surface source       |
|  [21]   | `SurfaceDescriptorFromXlibWindow`          | descriptor    | Xlib surface source          |
|  [22]   | `SurfaceDescriptorFromWaylandSurface`      | descriptor    | Wayland surface source       |
|  [23]   | `SurfaceDescriptorFromXcbWindow`           | descriptor    | XCB surface source           |
|  [24]   | `SurfaceDescriptorFromAndroidNativeWindow` | descriptor    | Android surface source       |
|  [25]   | `ChainedStruct`                            | struct        | input extension chain        |
|  [26]   | `ChainedStructOut`                         | struct        | output extension chain       |
|  [27]   | `PfnRequestAdapterCallback`                | delegate ptr  | adapter result               |
|  [28]   | `PfnRequestDeviceCallback`                 | delegate ptr  | device result                |
|  [29]   | `PfnBufferMapCallback`                     | delegate ptr  | buffer-map result            |
|  [30]   | `PfnQueueWorkDoneCallback`                 | delegate ptr  | queue-work result            |
|  [31]   | `PfnErrorCallback`                         | delegate ptr  | validation error             |
|  [32]   | `PfnDeviceLostCallback`                    | delegate ptr  | device-lost result           |
|  [33]   | `TextureFormat`                            | enum          | pixel format                 |
|  [34]   | `PresentMode`                              | enum          | present mode                 |
|  [35]   | `BackendType`                              | enum          | graphics backend             |
|  [36]   | `FeatureName`                              | enum          | device feature               |
|  [37]   | `WGSLFeatureName`                          | enum          | WGSL feature                 |
|  [38]   | `ErrorFilter`                              | enum          | capture-scope selector       |
|  [39]   | `MapMode`                                  | enum          | readback map direction       |
|  [40]   | `QueryType`                                | enum          | query family                 |

## [03]-[ENTRYPOINTS]

Every surface below is an instance method on the `WebGPU.GetApi()` function-table root taking raw pointers to descriptor structs, each descriptor-taking call shipping a paired `ref readonly <Descriptor>` `in`-reference overload that binds the same native call. `callback, userdata` abbreviates the member's own `Pfn…Callback` delegate beside its `void*` state slot.

[ENTRYPOINT_SCOPE]: API root, instance, and device lifecycle

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]        |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------ |
|  [01]   | `WebGPU.GetApi()`                                                               | static   | API root load       |
|  [02]   | `CreateInstance(InstanceDescriptor*)`                                           | instance | instance create     |
|  [03]   | `InstanceRequestAdapter(Instance*, RequestAdapterOptions*, callback, userdata)` | instance | adapter request     |
|  [04]   | `AdapterRequestDevice(Adapter*, DeviceDescriptor*, callback, userdata)`         | instance | device request      |
|  [05]   | `DeviceGetQueue(Device*)`                                                       | instance | queue handle        |
|  [06]   | `InstanceProcessEvents(Instance*)`                                              | instance | standard event pump |

[ENTRYPOINT_SCOPE]: feature, limit, and extension negotiation

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]          |
| :-----: | :---------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `AdapterEnumerateFeatures(Adapter*, FeatureName*) -> nuint` | instance | adapter feature list  |
|  [02]   | `AdapterHasFeature(Adapter*, FeatureName) -> Bool32`        | instance | adapter feature probe |
|  [03]   | `AdapterGetLimits(Adapter*, SupportedLimits*) -> Bool32`    | instance | adapter limits        |
|  [04]   | `DeviceEnumerateFeatures(Device*, FeatureName*) -> nuint`   | instance | device feature list   |
|  [05]   | `DeviceHasFeature(Device*, FeatureName) -> Bool32`          | instance | device feature probe  |
|  [06]   | `DeviceGetLimits(Device*, SupportedLimits*) -> Bool32`      | instance | device limits         |
|  [07]   | `IsDeviceExtensionPresent(Device*, string) -> bool`         | instance | wgpu-native probe     |
|  [08]   | `TryGetDeviceExtension<Wgpu>(Device*, out Wgpu) -> bool`    | instance | extension table load  |

[ENTRYPOINT_SCOPE]: validation error scope

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]         |
| :-----: | :-------------------------------------------------------------- | :------- | :------------------- |
|  [01]   | `DeviceSetUncapturedErrorCallback(Device*, callback, userdata)` | instance | unscoped error sink  |
|  [02]   | `DevicePushErrorScope(Device*, ErrorFilter)`                    | instance | open a capture scope |
|  [03]   | `DevicePopErrorScope(Device*, callback, userdata)`              | instance | drain the open scope |

[ENTRYPOINT_SCOPE]: surface, swapchain, and present

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]        |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------ |
|  [01]   | `InstanceCreateSurface(Instance*, SurfaceDescriptor*)`             | instance | surface from window |
|  [02]   | `SurfaceGetCapabilities(Surface*, Adapter*, SurfaceCapabilities*)` | instance | format/mode query   |
|  [03]   | `SurfaceConfigure(Surface*, SurfaceConfiguration*)`                | instance | swapchain configure |
|  [04]   | `SurfaceGetCurrentTexture(Surface*, SurfaceTexture*)`              | instance | acquire frame image |
|  [05]   | `TextureCreateView(Texture*, TextureViewDescriptor*)`              | instance | frame view          |
|  [06]   | `SurfacePresent(Surface*)`                                         | instance | present frame       |

[ENTRYPOINT_SCOPE]: resource and pipeline creation

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]          |
| :-----: | :----------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `DeviceCreateBuffer(Device*, BufferDescriptor*)`                   | instance | buffer alloc          |
|  [02]   | `DeviceCreateTexture(Device*, TextureDescriptor*)`                 | instance | texture alloc         |
|  [03]   | `DeviceCreateShaderModule(Device*, ShaderModuleDescriptor*)`       | instance | shader compile (WGSL) |
|  [04]   | `DeviceCreateRenderPipeline(Device*, RenderPipelineDescriptor*)`   | instance | raster pipeline       |
|  [05]   | `DeviceCreateComputePipeline(Device*, ComputePipelineDescriptor*)` | instance | compute pipeline      |
|  [06]   | `DeviceCreateBindGroup(Device*, BindGroupDescriptor*)`             | instance | bind group            |

[ENTRYPOINT_SCOPE]: command recording and submission

| [INDEX] | [SURFACE]                                                                                   | [SHAPE]  | [CAPABILITY]          |
| :-----: | :------------------------------------------------------------------------------------------ | :------- | :-------------------- |
|  [01]   | `DeviceCreateCommandEncoder(Device*, CommandEncoderDescriptor*)`                            | instance | encoder create        |
|  [02]   | `CommandEncoderBeginRenderPass(CommandEncoder*, RenderPassDescriptor*)`                     | instance | render pass begin     |
|  [03]   | `CommandEncoderBeginComputePass(CommandEncoder*, ComputePassDescriptor*)`                   | instance | compute pass begin    |
|  [04]   | `RenderPassEncoderSetPipeline(RenderPassEncoder*, RenderPipeline*)`                         | instance | bind raster pipeline  |
|  [05]   | `RenderPassEncoderSetBindGroup(RenderPassEncoder*, uint, BindGroup*, nuint, uint*)`         | instance | bind resource group   |
|  [06]   | `RenderPassEncoderDraw(RenderPassEncoder*, uint, uint, uint, uint)`                         | instance | non-indexed draw      |
|  [07]   | `RenderPassEncoderDrawIndexed(RenderPassEncoder*, uint, uint, uint, int, uint)`             | instance | indexed draw          |
|  [08]   | `ComputePassEncoderSetPipeline(ComputePassEncoder*, ComputePipeline*)`                      | instance | bind compute pipeline |
|  [09]   | `ComputePassEncoderDispatchWorkgroups(ComputePassEncoder*, uint, uint, uint)`               | instance | compute dispatch      |
|  [10]   | `CommandEncoderFinish(CommandEncoder*, CommandBufferDescriptor*)`                           | instance | finish command buffer |
|  [11]   | `QueueSubmit(Queue*, nuint, CommandBuffer**)`                                               | instance | submit to GPU         |
|  [12]   | `QueueWriteBuffer(Queue*, Buffer*, ulong, void*, nuint)`                                    | instance | buffer upload         |
|  [13]   | `QueueWriteTexture(Queue*, ImageCopyTexture*, void*, nuint, TextureDataLayout*, Extent3D*)` | instance | texture upload        |

[ENTRYPOINT_SCOPE]: timestamp queries and readback

| [INDEX] | [SURFACE]                                                                               | [SHAPE]  | [CAPABILITY]           |
| :-----: | :-------------------------------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `DeviceCreateQuerySet(Device*, QuerySetDescriptor*)`                                    | instance | query-set allocation   |
|  [02]   | `CommandEncoderWriteTimestamp(CommandEncoder*, QuerySet*, uint)`                        | instance | per-pass timestamp     |
|  [03]   | `CommandEncoderResolveQuerySet(CommandEncoder*, QuerySet*, uint, uint, Buffer*, ulong)` | instance | resolve to read buffer |
|  [04]   | `BufferMapAsync(Buffer*, MapMode, nuint, nuint, callback, userdata)`                    | instance | async readback map     |
|  [05]   | `BufferGetMappedRange(Buffer*, nuint, nuint) -> void*`                                  | instance | resolved-tick access   |
|  [06]   | `BufferUnmap(Buffer*)`                                                                  | instance | mapped-range release   |
|  [07]   | `QuerySetRelease(QuerySet*)`                                                            | instance | query-set release      |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `WebGPU.GetApi()` returns the function-table root; a call site marshals `Span<T>`/`stackalloc` descriptor structs and passes raw pointers, never a managed wrapper object, since Silk.NET binds the C `webgpu.h` surface directly.
- Lifecycle threads `Instance` -> `Adapter` -> `Device` + `Queue` -> resources; the adapter and device requests complete through native callbacks Silk.NET surfaces as `PfnRequestAdapterCallback`/`PfnRequestDeviceCallback` delegate pointers.
- `InstanceCreateSurface` reads the window handle from `SurfaceDescriptor.NextInChain` — `SurfaceDescriptorFromMetalLayer` (a `CAMetalLayer` from `Platforms.MacOS.NSWindow`), `…FromWindowsHWND`, `…FromXlibWindow`/`…FromWaylandSurface`/`…FromXcbWindow`, or `…FromAndroidNativeWindow`; `SurfaceConfigure` sets format/present-mode/usage/extent, and each frame `SurfaceGetCurrentTexture` yields a `SurfaceTexture` viewed, rendered through a `RenderPassEncoder`, and presented through `SurfacePresent`.
- `BackendType` auto-negotiates D3D12 (Windows), Metal (macOS), Vulkan (Linux), or the in-browser WebGPU target over the `wgpu_native` runtime; `AdapterEnumerateFeatures`/`AdapterHasFeature`/`AdapterGetLimits` negotiate the feature and limit set (gating `timestamp-query`) before `AdapterRequestDevice` requests them in `DeviceDescriptor`.
- WGSL is the shader source for `DeviceCreateShaderModule`; compute shaders dispatch through `ComputePassEncoderDispatchWorkgroups` and raster pipelines draw through `RenderPassEncoderDraw`/`RenderPassEncoderDrawIndexed`, so the meshlet mesh-shader and path-trace compute passes bind to one wgpu pipeline family.
- Validation rides the error-scope rail: `DeviceSetUncapturedErrorCallback` installs the global `PfnErrorCallback` and `DevicePushErrorScope`/`DevicePopErrorScope` bracket a suspect operation, so a validation or out-of-memory error is a counted `ViewportFault` on the telemetry spine, never a swallowed native abort.
- Timestamp timing folds through the core query surface: `DeviceCreateQuerySet` mints a `QueryType.Timestamp` set, `CommandEncoderWriteTimestamp` or the `RenderPassTimestampWrites`/`ComputePassTimestampWrites` begin-descriptor entries record GPU wall time at pass boundaries, and `CommandEncoderResolveQuerySet` copies the nanosecond ticks into a mappable buffer.
- Readback threads `BufferMapAsync` -> `BufferGetMappedRange` -> `BufferUnmap` once the map callback retires under `InstanceProcessEvents`; the resolved per-pass GPU duration is the measured timeline beside the encoder-projected CPU duration, never a busy-wait fence.

[STACKING]:
- `api-silk-webgpu-wgpu`(`api-silk-webgpu-wgpu.md`): `TryGetDeviceExtension<Wgpu>` gates on `IsDeviceExtensionPresent` and loads `Silk.NET.WebGPU.Extensions.WGPU` as a second function-table view over the one `wgpu_native` runtime — `DevicePoll` retires the async map and submission callbacks non-blocking, `SetLogCallback` routes the native adapter/device-lost diagnostic stream into the receipt sink beside the error-scope rail, `InstanceEnumerateAdapters` supplies the compositor-LUID adapter match, and `RenderPassEncoderMultiDraw*`/`SetPushConstants` and pipeline-statistics queries extend the meshlet passes.
- `api-avalonia-gpu-interop`(`api-avalonia-gpu-interop.md`): the rendered wgpu `Texture` exports as the platform handle `ICompositionGpuInterop.ImportImage` binds into a `CompositionDrawingSurface`, the export-format synchronization primitive selecting the `UpdateWith*Async` refresh, never a second swapchain composited beside Avalonia's.
- `api-skiasharp`(`api-skiasharp.md`): the SkiaSharp Ganesh `GRContext` backends (`GRMtlBackendContext`/`GRVkBackendContext`/`GRD3DBackendContext`) and the `Wgpu` family are mutually exclusive `GpuBackend` rows the target-factory dispatches between; a `GRContext` Ganesh call never enters the wgpu pipeline.
- within-lib: the AppUi viewport boundary capsule folds each native-handle create-and-release pair in a scoped fold and projects validation faults and resolved timestamp durations onto the telemetry spine as `ViewportFault` and duration receipt rows.

[LOCAL_ADMISSION]:
- Every native handle (`Instance`, `Adapter`, `Device`, `Queue`, `Surface`, `Buffer`, `Texture`, pipelines, encoders) releases through its matching `XxxRelease`/`XxxDestroy` native call rather than `IDisposable`, the owning boundary capsule pairing create-and-release in a `using`-equivalent scoped fold.
- `Silk.NET.WebGPU.Native.WGPU` supplies the one native `wgpu_native` asset this binding P/Invokes across the win/linux/osx RIDs.

[RAIL_LAW]:
- Package: `Silk.NET.WebGPU`, `Silk.NET.WebGPU.Native.WGPU` native runtime
- Owns: the managed `wgpu_native` binding for the `Wgpu` `GpuBackend` family — instance/adapter/device lifecycle, feature/limit negotiation, surface-from-window-handle and swapchain, buffer/texture/sampler allocation, WGSL shader compile, render/compute pipeline create, command recording, queue submission, error-scope validation, and timestamp-query timing.
- Accept: raw-pointer descriptor calls on the `WebGPU.GetApi()` function-table root; `InstanceCreateSurface` with a platform-source `next` chain; feature/limit query before device request; error-scope-bracketed validation into the receipt sink; native-handle scoped create-and-release pairs at the boundary capsule; texture import into the Avalonia compositor through `ICompositionGpuInterop.ImportImage`.
- Reject: a managed convenience wrapper renaming the native surface; a `QueryType.PipelineStatistics` core value (wgpu-native-only); SkiaSharp `GRContext` Ganesh calls mixed into the wgpu pipeline.

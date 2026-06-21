# [RASM_COMPUTE_API_SILK_WEBGPU]

`Silk.NET.WebGPU` is the managed wgpu/Dawn binding generated against the canonical `webgpu.h` headers; the Compute lane consumes its GPGPU half — `WebGPU.GetApi()` is the static function-table root, a `Device` mints `Buffer`/`ShaderModule`/`BindGroupLayout`/`BindGroup`/`ComputePipeline`/`CommandEncoder`/`QuerySet`, a `CommandEncoder` records a `ComputePassEncoder` that binds a pipeline and `DispatchWorkgroups`, and the `Queue` submits the finished `CommandBuffer`. The Compute substrate holds no second device: the `ONE_WGPU_DEVICE` shared branch is the same `Device`/`Queue` the AppUi renderer owns (Metal-backed on macOS), here driven for compute-shader dispatch rather than the swapchain.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.WebGPU`
- package: `Silk.NET.WebGPU` (`2.23.0`)
- package: `Silk.NET.WebGPU.Native.WGPU` (native wgpu runtime)
- assembly: `Silk.NET.WebGPU` (`netstandard2.1`)
- namespace: `Silk.NET.WebGPU`
- asset: runtime library + native wgpu binaries
- rail: compute
- roster: 184 types across 4 namespaces

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: device lifecycle and submission owners
- rail: compute

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [RAIL]                       |
| :-----: | :--------- | :-------------- | :--------------------------- |
|  [01]   | `WebGPU`   | static API root | global entry, function table |
|  [02]   | `Instance` | native handle   | adapter request root         |
|  [03]   | `Adapter`  | native handle   | device request, limits query |
|  [04]   | `Device`   | native handle   | resource factory             |
|  [05]   | `Queue`    | native handle   | command submission, upload   |

[PUBLIC_TYPE_SCOPE]: compute resource and dispatch owners
- rail: compute

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :------------------- | :------------ | :---------------------------- |
|  [01]   | `Buffer`             | native handle | storage/uniform/staging alloc |
|  [02]   | `ShaderModule`       | native handle | WGSL/SPIR-V compute shader    |
|  [03]   | `BindGroupLayout`    | native handle | binding layout                |
|  [04]   | `BindGroup`          | native handle | bound resource group          |
|  [05]   | `PipelineLayout`     | native handle | pipeline layout               |
|  [06]   | `ComputePipeline`    | native handle | compute pipeline              |
|  [07]   | `CommandEncoder`     | native handle | command recorder              |
|  [08]   | `ComputePassEncoder` | native handle | compute pass recorder         |
|  [09]   | `CommandBuffer`      | native handle | finished command buffer       |
|  [10]   | `QuerySet`           | native handle | timestamp query pool          |

[PUBLIC_TYPE_SCOPE]: descriptor and enum value carriers
- rail: compute

| [INDEX] | [SYMBOL]                      | [KIND]     | [RAIL]                            |
| :-----: | :---------------------------- | :--------- | :-------------------------------- |
|  [01]   | `BufferDescriptor`            | descriptor | buffer alloc (usage, size, map)   |
|  [02]   | `ShaderModuleDescriptor`      | descriptor | shader module create              |
|  [03]   | `ShaderModuleWGSLDescriptor`  | descriptor | WGSL source chain                 |
|  [04]   | `ShaderModuleSPIRVDescriptor` | descriptor | SPIR-V source chain               |
|  [05]   | `BindGroupLayoutDescriptor`   | descriptor | layout entries                    |
|  [06]   | `BindGroupLayoutEntry`        | descriptor | one binding slot                  |
|  [07]   | `BufferBindingLayout`         | descriptor | storage/uniform binding kind      |
|  [08]   | `BindGroupDescriptor`         | descriptor | bound entries                     |
|  [09]   | `BindGroupEntry`              | descriptor | one bound resource                |
|  [10]   | `PipelineLayoutDescriptor`    | descriptor | layout set                        |
|  [11]   | `ComputePipelineDescriptor`   | descriptor | compute pipeline create           |
|  [12]   | `ProgrammableStageDescriptor` | descriptor | entry point, constants            |
|  [13]   | `QuerySetDescriptor`          | descriptor | query-set alloc (type, count)     |
|  [14]   | `BufferUsage`                 | enum       | storage/uniform/copy/map/indirect |
|  [15]   | `MapMode`                     | enum       | readback/upload map direction     |
|  [16]   | `ShaderStage`                 | enum       | `Compute` visibility flag         |
|  [17]   | `BufferBindingType`           | enum       | uniform/storage binding kind      |
|  [18]   | `BufferMapState`              | enum       | unmapped/pending/mapped poll      |
|  [19]   | `QueryType`                   | enum       | timestamp/occlusion query kind    |

## [03]-[ENTRYPOINTS]

All entrypoints are instance methods on the `WebGPU.GetApi()` function-table root taking raw pointers to descriptor structs; a call site marshals `Span<T>`/`stackalloc` descriptors and passes pointers, never a managed wrapper.

[ENTRYPOINT_SCOPE]: device and queue acquisition
- rail: compute

| [INDEX] | [SURFACE]                                                                       | [SURFACE_ROOT] | [RAIL]           |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :--------------- |
|  [01]   | `WebGPU.GetApi()`                                                               | `WebGPU`       | API root load    |
|  [02]   | `CreateInstance(InstanceDescriptor*)`                                           | `WebGPU`       | instance create  |
|  [03]   | `InstanceRequestAdapter(Instance*, RequestAdapterOptions*, callback, userdata)` | `WebGPU`       | adapter request  |
|  [04]   | `AdapterRequestDevice(Adapter*, DeviceDescriptor*, callback, userdata)`         | `WebGPU`       | device request   |
|  [05]   | `DeviceGetQueue(Device*)`                                                       | `WebGPU`       | queue handle     |
|  [06]   | `DeviceGetLimits(Device*, SupportedLimits*)`                                    | `WebGPU`       | workgroup limits |

[ENTRYPOINT_SCOPE]: compute resource and pipeline creation
- rail: compute

| [INDEX] | [SURFACE]                                                          | [SURFACE_ROOT] | [RAIL]                 |
| :-----: | :----------------------------------------------------------------- | :------------- | :--------------------- |
|  [01]   | `DeviceCreateBuffer(Device*, BufferDescriptor*)`                   | `WebGPU`       | storage/staging alloc  |
|  [02]   | `DeviceCreateShaderModule(Device*, ShaderModuleDescriptor*)`       | `WebGPU`       | compute shader compile |
|  [03]   | `DeviceCreateBindGroupLayout(Device*, BindGroupLayoutDescriptor*)` | `WebGPU`       | binding layout         |
|  [04]   | `DeviceCreateBindGroup(Device*, BindGroupDescriptor*)`             | `WebGPU`       | bound resource group   |
|  [05]   | `DeviceCreatePipelineLayout(Device*, PipelineLayoutDescriptor*)`   | `WebGPU`       | pipeline layout        |
|  [06]   | `DeviceCreateComputePipeline(Device*, ComputePipelineDescriptor*)` | `WebGPU`       | compute pipeline       |
|  [07]   | `DeviceCreateCommandEncoder(Device*, CommandEncoderDescriptor*)`   | `WebGPU`       | encoder create         |
|  [08]   | `DeviceCreateQuerySet(Device*, QuerySetDescriptor*)`               | `WebGPU`       | timestamp query-set    |

[ENTRYPOINT_SCOPE]: compute pass recording and submission
- rail: compute

| [INDEX] | [SURFACE]                                                                                                | [SURFACE_ROOT] | [RAIL]                |
| :-----: | :------------------------------------------------------------------------------------------------------- | :------------- | :-------------------- |
|  [01]   | `CommandEncoderBeginComputePass(CommandEncoder*, ComputePassDescriptor*)`                                | `WebGPU`       | compute pass begin    |
|  [02]   | `ComputePassEncoderSetPipeline(ComputePassEncoder*, ComputePipeline*)`                                   | `WebGPU`       | bind compute pipeline |
|  [03]   | `ComputePassEncoderSetBindGroup(ComputePassEncoder*, groupIndex, BindGroup*, dynamicOffsetCount, uint*)` | `WebGPU`       | bind resource group   |
|  [04]   | `ComputePassEncoderDispatchWorkgroups(ComputePassEncoder*, countX, countY, countZ)`                      | `WebGPU`       | workgroup dispatch    |
|  [05]   | `ComputePassEncoderDispatchWorkgroupsIndirect(ComputePassEncoder*, Buffer*, offset)`                     | `WebGPU`       | indirect dispatch     |
|  [06]   | `ComputePassEncoderEnd(ComputePassEncoder*)`                                                             | `WebGPU`       | close compute pass    |
|  [07]   | `CommandEncoderFinish(CommandEncoder*, CommandBufferDescriptor*)`                                        | `WebGPU`       | finish command buffer |
|  [08]   | `QueueSubmit(Queue*, nuint, CommandBuffer**)`                                                            | `WebGPU`       | submit to GPU         |

[ENTRYPOINT_SCOPE]: host transfer, readback, and timing
- rail: compute

| [INDEX] | [SURFACE]                                                                                  | [SURFACE_ROOT] | [RAIL]                  |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :---------------------- |
|  [01]   | `QueueWriteBuffer(Queue*, Buffer*, bufferOffset, void* data, nuint size)`                  | `WebGPU`       | host-to-GPU upload      |
|  [02]   | `CommandEncoderCopyBufferToBuffer(CommandEncoder*, src, srcOffset, dst, dstOffset, size)`  | `WebGPU`       | device-side copy        |
|  [03]   | `BufferMapAsync(Buffer*, MapMode, offset, size, PfnBufferMapCallback, void* userdata)`     | `WebGPU`       | request mapping         |
|  [04]   | `BufferGetMappedRange(Buffer*, nuint offset, nuint size)`                                  | `WebGPU`       | mapped readback pointer |
|  [05]   | `BufferGetMapState(Buffer*)`                                                               | `WebGPU`       | poll `BufferMapState`   |
|  [06]   | `BufferUnmap(Buffer*)`                                                                     | `WebGPU`       | release mapping         |
|  [07]   | `QueueOnSubmittedWorkDone(Queue*, PfnQueueWorkDoneCallback, void* userdata)`               | `WebGPU`       | submission completion   |
|  [08]   | `CommandEncoderWriteTimestamp(CommandEncoder*, QuerySet*, queryIndex)`                     | `WebGPU`       | per-pass timestamp      |
|  [09]   | `CommandEncoderResolveQuerySet(CommandEncoder*, QuerySet*, first, count, Buffer*, offset)` | `WebGPU`       | resolve to read buffer  |

## [04]-[IMPLEMENTATION_LAW]

[COMPUTE_TOPOLOGY]:
- `WebGPU.GetApi()` returns the function-table root; every native call is an instance method on that `WebGPU` object taking raw pointers to descriptor structs, so a call site marshals `stackalloc`/`Span<T>` descriptors and passes pointers, never a managed wrapper object.
- The compute lifecycle is `Device` -> `ShaderModule` (`DeviceCreateShaderModule` with a `ShaderModuleWGSLDescriptor` WGSL chain or `ShaderModuleSPIRVDescriptor` SPIR-V chain) -> `BindGroupLayout` + `PipelineLayout` -> `ComputePipeline` (`ComputePipelineDescriptor.Compute` is a `ProgrammableStageDescriptor` naming the WGSL entry point and `ConstantEntry` overrides) -> `BindGroup` binding the storage/uniform `Buffer` resources.
- `BufferDescriptor.Usage` is a `BufferUsage` flag set: `Storage` for read/write shader buffers, `Uniform` for constant blocks, `CopySrc`/`CopyDst` for device-side `CommandEncoderCopyBufferToBuffer`, `MapRead`/`MapWrite` for host mapping, `Indirect` for `DispatchWorkgroupsIndirect`, and `QueryResolve` for the resolved timestamp destination — a storage buffer carries `Storage | CopySrc`, a readback staging buffer carries `MapRead | CopyDst`.
- `BindGroupLayoutEntry.Visibility` carries `ShaderStage.Compute` for compute bindings, and `BufferBindingLayout.Type` selects `BufferBindingType.Storage` / `ReadOnlyStorage` / `Uniform`; the `BindGroupEntry` binds the concrete `Buffer` with offset and size at the matching `Binding` slot.
- A pass records `CommandEncoderBeginComputePass` -> `ComputePassEncoderSetPipeline` -> `ComputePassEncoderSetBindGroup` -> `ComputePassEncoderDispatchWorkgroups(x, y, z)` (or `DispatchWorkgroupsIndirect` reading the workgroup count from an `Indirect` buffer) -> `ComputePassEncoderEnd`; the encoder closes through `CommandEncoderFinish` into a `CommandBuffer` the `Queue` retires through `QueueSubmit`.
- Readback is two-phase: `CommandEncoderCopyBufferToBuffer` copies the `Storage` result into a `MapRead` staging buffer, then after submission `BufferMapAsync(MapMode.Read, ...)` requests the mapping, `BufferGetMapState` polls `BufferMapState.Mapped`, `BufferGetMappedRange` yields the host pointer, and `BufferUnmap` releases it; `QueueOnSubmittedWorkDone` is the submission-completion callback that gates the map request.
- A `QuerySet` of `QueryType.Timestamp` records GPU-side wall time at pass boundaries through `CommandEncoderWriteTimestamp` (or the `ComputePassTimestampWrites` begin-descriptor entries on `timestamp-query` feature support), and `CommandEncoderResolveQuerySet` copies the resolved nanosecond ticks into a `QueryResolve`/`CopySrc` buffer the readback path retires — the resolved per-pass GPU duration is the measured dispatch timeline, never a busy-wait fence.

[LOCAL_ADMISSION]:
- The Compute lane holds no second device: `ONE_WGPU_DEVICE` is the same `Device`/`Queue` the AppUi renderer owns, so the GPGPU substrate composes the renderer's already-acquired handles instead of `CreateInstance`/`AdapterRequestDevice` a second time; the adapter/device request entrypoints stay in the AppUi boundary capsule that mints the shared device.
- `TensorOpFamily` rows lower to compute-shader dispatch: each op compiles one WGSL `ShaderModule`, builds a `ComputePipeline` plus `BindGroupLayout`, and the dispatch executor binds the tensor storage `Buffer`s and issues `DispatchWorkgroups` over the tile/workgroup decomposition the op's launch geometry declares.
- All native handles (`Device`, `Queue`, `Buffer`, `ShaderModule`, `BindGroupLayout`, `BindGroup`, `PipelineLayout`, `ComputePipeline`, `CommandEncoder`, `ComputePassEncoder`, `QuerySet`) release through their matching `XxxRelease`/`XxxDestroy` native call, not `IDisposable` — the owning boundary capsule pairs create-and-release in a `using`-equivalent scoped fold; the shared `Device`/`Queue` are released by the AppUi owner, not the Compute lane.
- `DevicePoll`/device-tick advance is a `wgpu_native` extension surface absent from the base binding; the Compute readback path drives `QueueOnSubmittedWorkDone` plus `BufferGetMapState` polling, with the native non-blocking advance owned by `Silk.NET.WebGPU.Native.WGPU`.

[RAIL_LAW]:
- Package: `Silk.NET.WebGPU` (+ `Silk.NET.WebGPU.Native.WGPU`)
- Owns: the GPGPU half of the managed wgpu/Dawn binding — storage/uniform/staging buffer allocation, WGSL/SPIR-V compute shader compile, bind-group layout and pipeline create, compute pass recording, workgroup dispatch, queue submission, host transfer/readback mapping, and timestamp query resolution for the `ONE_WGPU_DEVICE` branch.
- Accept: raw-pointer descriptor calls on the `WebGPU.GetApi()` function-table root over the shared `Device`/`Queue` the AppUi renderer holds; native-handle scoped create-and-release pairs for compute-only resources.
- Reject: a second `Device`/`Queue` acquisition inside the Compute lane; a managed convenience wrapper renaming the native surface; CPU-side fallback math where a `TensorOpFamily` row lowers to a compute-shader dispatch.

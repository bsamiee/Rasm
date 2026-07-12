# [RASM_COMPUTE_API_SILK_WEBGPU]

`Silk.NET.WebGPU` is the managed wgpu/Dawn binding generated against the canonical `webgpu.h` headers; the Compute lane consumes its GPGPU half — `WebGPU.GetApi()` is the static function-table root, a `Device` mints `Buffer`/`ShaderModule`/`BindGroupLayout`/`BindGroup`/`ComputePipeline`/`CommandEncoder`/`QuerySet`, a `CommandEncoder` records a `ComputePassEncoder` that binds a pipeline and `DispatchWorkgroups`, and the `Queue` submits the finished `CommandBuffer`. The Compute substrate holds no second device: the `ONE_WGPU_DEVICE` shared branch is the same `Device`/`Queue` the AppUi renderer owns (Metal-backed on macOS), here driven for compute-shader dispatch rather than the swapchain. The base binding carries only the standard `webgpu.h` surface; the wgpu-native-only compute extensions (`DevicePoll`, `QueueSubmitForIndex`, compute-pass pipeline-statistics queries, synchronous adapter enumeration, push constants) live in the companion `Silk.NET.WebGPU.Extensions.WGPU` `Wgpu` function table, also admitted in the central manifest — `Silk.NET.WebGPU.Native.WGPU` carries ONLY the native `libwgpu` binaries, never a managed surface, so the device-tick/readback-advance entrypoints are reached through `Wgpu`, not the native-runtime package.

The whole surface is `unsafe`-pointer native methods generated from `webgpu.h`; Silk.NET emits a managed call form too — every descriptor-taking method has a paired `ref readonly <Descriptor>` overload beside the raw `<Descriptor>*` overload, so a call site passes a `Span<T>`/`stackalloc` descriptor by `in` reference rather than always pinning a pointer. The string-label overloads add a marshalled `string` form beside the `byte*` form. The Compute capsule prefers the `ref readonly` overload for descriptors (no manual pin) and the `byte*` form for hot WGSL/label paths it already owns as fixed buffers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.WebGPU`
- package: `Silk.NET.WebGPU` (`2.23.0`)
- package: `Silk.NET.WebGPU.Native.WGPU` (`2.23.0`) — native `libwgpu` runtime binaries only (no managed surface)
- assembly: `Silk.NET.WebGPU`
- license: MIT
- namespace: `Silk.NET.WebGPU`
- asset: ships `net5.0`/`netcoreapp3.1`/`netstandard2.0`/`netstandard2.1`; the `net10.0` consumer binds the `net5.0` asset (the public surface is code-generated and TFM-invariant across these, so the decompile is authoritative regardless of which fallback resolves) + native wgpu binaries
- rail: compute
- roster: generated surface across 4 namespaces
- abi: every entrypoint is an `unsafe` instance method on the `WebGPU : NativeAPI` function table; native handles (`Device`/`Queue`/`Buffer`/...) are pointer-wrapped structs released through `XxxRelease`/`XxxDestroy`, never `IDisposable`

[PACKAGE_SURFACE]: `Silk.NET.WebGPU.Extensions.WGPU`
- package: `Silk.NET.WebGPU.Extensions.WGPU` (`2.23.0`)
- assembly: `Silk.NET.WebGPU.Extensions.WGPU`
- license: MIT
- namespace: `Silk.NET.WebGPU.Extensions.WGPU`
- asset: ships `net5.0`/`netcoreapp3.1`/`netstandard2.0`/`netstandard2.1` (consumer binds `net5.0`, surface TFM-invariant); wgpu-native function extensions over the base binding
- rail: compute
- roster: wgpu-native extension surface across 3 namespaces
- root: `Wgpu` — the wgpu-native function table acquired through `GetApi()` over the same shared device; carries the device-tick, submission-index, pipeline-statistics, push-constant, and synchronous-adapter-enumeration entrypoints absent from `webgpu.h`

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

| [INDEX] | [SYMBOL]                      | [KIND]     | [RAIL]                                                         |
| :-----: | :---------------------------- | :--------- | :------------------------------------------------------------- |
|  [01]   | `BufferDescriptor`            | descriptor | buffer alloc (`Usage`, `Size`, `MappedAtCreation`)             |
|  [02]   | `ShaderModuleDescriptor`      | descriptor | shader module create (`NextInChain` source chain)              |
|  [03]   | `ShaderModuleWGSLDescriptor`  | descriptor | WGSL source chain (`SType.ShaderModuleWgslDescriptor`)         |
|  [04]   | `ShaderModuleSPIRVDescriptor` | descriptor | SPIR-V source chain (`Code`, `CodeSize`)                       |
|  [05]   | `BindGroupLayoutDescriptor`   | descriptor | layout entries (`Entries`, `EntryCount`)                       |
|  [06]   | `BindGroupLayoutEntry`        | descriptor | one binding slot (`Binding`, `Visibility`, `Buffer`)           |
|  [07]   | `BufferBindingLayout`         | descriptor | binding kind (`Type`, `HasDynamicOffset`, `MinBindingSize`)    |
|  [08]   | `BindGroupDescriptor`         | descriptor | bound entries (`Layout`, `Entries`)                            |
|  [09]   | `BindGroupEntry`              | descriptor | one bound resource (`Binding`, `Buffer`, `Offset`, `Size`)     |
|  [10]   | `PipelineLayoutDescriptor`    | descriptor | layout set (`BindGroupLayouts`)                                |
|  [11]   | `ComputePipelineDescriptor`   | descriptor | compute pipeline (`Layout`, `Compute` stage)                   |
|  [12]   | `ProgrammableStageDescriptor` | descriptor | `Module`, `EntryPoint`, `ConstantCount`, `Constants`           |
|  [13]   | `ConstantEntry`               | descriptor | one WGSL `override` constant (`Key`, `Value` double)           |
|  [14]   | `QuerySetDescriptor`          | descriptor | query-set alloc (`Type`, `Count`)                              |
|  [15]   | `ComputePassDescriptor`       | descriptor | pass begin (`Label`, `TimestampWrites`)                        |
|  [16]   | `ComputePassTimestampWrites`  | descriptor | `QuerySet`, `BeginningOfPassWriteIndex`, `EndOfPassWriteIndex` |
|  [17]   | `SupportedLimits` / `Limits`  | struct     | `DeviceGetLimits` out-struct; `maxComputeWorkgroupSize*` etc.  |
|  [18]   | `BufferUsage`                 | enum       | storage/uniform/copy/map/indirect/queryresolve                 |
|  [19]   | `MapMode`                     | enum       | readback/upload map direction                                  |
|  [20]   | `ShaderStage`                 | enum       | `Compute` visibility flag                                      |
|  [21]   | `BufferBindingType`           | enum       | `Uniform`/`Storage`/`ReadOnlyStorage` binding kind             |
|  [22]   | `BufferMapState`              | enum       | unmapped/pending/mapped poll                                   |
|  [23]   | `QueryType`                   | enum       | timestamp/occlusion query kind                                 |

[PUBLIC_TYPE_SCOPE]: wgpu-native extension owners (`Silk.NET.WebGPU.Extensions.WGPU`)
- rail: compute

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]   | [RAIL]                                                            |
| :-----: | :--------------------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `Wgpu`                       | static API root | `GetApi()` function table for the wgpu-native extension calls     |
|  [02]   | `WrappedSubmissionIndex`     | struct          | `Queue` + submission `ulong` index pairing for `DevicePoll` wait  |
|  [03]   | `QuerySetDescriptorExtras`   | descriptor      | `PipelineStatistics` query-set chain (`SType` extras)             |
|  [04]   | `PipelineStatisticName`      | enum            | vertex/clipper/primitive/fragment/compute-invocation statistic    |
|  [05]   | `NativeFeature`              | enum            | `PushConstants`/`PipelineStatisticsQuery`/multi-draw feature bits |
|  [06]   | `NativeLimits`               | struct          | wgpu-native limit extras (`MaxPushConstantSize`)                  |
|  [07]   | `InstanceBackend`            | enum            | Vulkan/Metal/DX12/GL backend selection for `InstanceExtras`       |
|  [08]   | `ShaderModuleGLSLDescriptor` | descriptor      | GLSL compute source chain (wgpu-native only)                      |

## [03]-[ENTRYPOINTS]

All entrypoints are `unsafe` instance methods on the `WebGPU.GetApi()` function-table root. Each descriptor-taking method ships two overloads — a raw `<Descriptor>*` form and a `ref readonly <Descriptor>` form — so a call site either pins a `Span<T>`/`stackalloc` descriptor and passes a pointer, or passes the `stackalloc` descriptor by `in` reference; both bind the same native call, neither is a managed convenience object wrapping the handle. The generic `<T0>(... ref T0 userdata)` overloads thread typed `unmanaged` userdata into the callback-bearing async entrypoints without a `void*` cast.

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

| [INDEX] | [SURFACE]                                                              | [SURFACE_ROOT] | [RAIL]                 |
| :-----: | :--------------------------------------------------------------------- | :------------- | :--------------------- |
|  [01]   | `DeviceCreateBuffer(Device*, BufferDescriptor*)`                       | `WebGPU`       | storage/staging alloc  |
|  [02]   | `DeviceCreateShaderModule(Device*, ShaderModuleDescriptor*)`           | `WebGPU`       | compute shader compile |
|  [03]   | `DeviceCreateBindGroupLayout(Device*, BindGroupLayoutDescriptor*)`     | `WebGPU`       | binding layout         |
|  [04]   | `DeviceCreateBindGroup(Device*, BindGroupDescriptor*)`                 | `WebGPU`       | bound resource group   |
|  [05]   | `DeviceCreatePipelineLayout(Device*, PipelineLayoutDescriptor*)`       | `WebGPU`       | pipeline layout        |
|  [06]   | `DeviceCreateComputePipeline(Device*, ComputePipelineDescriptor*)`     | `WebGPU`       | compute pipeline       |
|  [07]   | `DeviceCreateComputePipelineAsync(Device*, ..., callback, userdata)`   | `WebGPU`       | async pipeline compile |
|  [08]   | `ComputePipelineGetBindGroupLayout(ComputePipeline*, uint groupIndex)` | `WebGPU`       | auto-derive layout     |
|  [09]   | `DeviceCreateCommandEncoder(Device*, CommandEncoderDescriptor*)`       | `WebGPU`       | encoder create         |
|  [10]   | `DeviceCreateQuerySet(Device*, QuerySetDescriptor*)`                   | `WebGPU`       | timestamp query-set    |

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
|  [10]   | `QuerySetGetCount(QuerySet*)` / `QuerySetGetType(QuerySet*)`                               | `WebGPU`       | query-set introspection |

[ENTRYPOINT_SCOPE]: pass observability and debug grouping
- rail: compute

| [INDEX] | [SURFACE]                                                                     | [SURFACE_ROOT] | [RAIL]                 |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :--------------------- |
|  [01]   | `ComputePassEncoderSetLabel(ComputePassEncoder*, byte* label)`                | `WebGPU`       | pass label for capture |
|  [02]   | `ComputePassEncoderInsertDebugMarker(ComputePassEncoder*, byte* markerLabel)` | `WebGPU`       | inline debug marker    |
|  [03]   | `ComputePassEncoderPushDebugGroup(ComputePassEncoder*, byte* groupLabel)`     | `WebGPU`       | open debug group       |
|  [04]   | `ComputePassEncoderPopDebugGroup(ComputePassEncoder*)`                        | `WebGPU`       | close debug group      |
|  [05]   | `ComputePipelineSetLabel(ComputePipeline*, byte* label)`                      | `WebGPU`       | pipeline label         |

[ENTRYPOINT_SCOPE]: wgpu-native extension calls (`Wgpu.GetApi()`)
- rail: compute

| [INDEX] | [SURFACE]                                                                                    | [SURFACE_ROOT] | [RAIL]                     |
| :-----: | :------------------------------------------------------------------------------------------- | :------------- | :------------------------- |
|  [01]   | `DevicePoll(Device*, Bool32 wait, WrappedSubmissionIndex* index)`                            | `Wgpu`         | non-blocking device-tick   |
|  [02]   | `QueueSubmitForIndex(Queue*, nuint count, CommandBuffer** commands)` → `ulong`               | `Wgpu`         | submit + return wait index |
|  [03]   | `ComputePassEncoderBeginPipelineStatisticsQuery(ComputePassEncoder*, QuerySet*, queryIndex)` | `Wgpu`         | begin pipeline-statistics  |
|  [04]   | `ComputePassEncoderEndPipelineStatisticsQuery(ComputePassEncoder*)`                          | `Wgpu`         | end pipeline-statistics    |
|  [05]   | `InstanceEnumerateAdapters(Instance*, InstanceEnumerateAdapterOptions*, Adapter** out)`      | `Wgpu`         | synchronous adapter list   |

## [04]-[IMPLEMENTATION_LAW]

[COMPUTE_TOPOLOGY]:
- `WebGPU.GetApi()` returns the function-table root; every native call is an instance method on that `WebGPU` object taking raw pointers to descriptor structs, so a call site marshals `stackalloc`/`Span<T>` descriptors and passes pointers, never a managed wrapper object.
- The compute lifecycle is `Device` -> `ShaderModule` (`DeviceCreateShaderModule` with a `ShaderModuleWGSLDescriptor` WGSL chain or `ShaderModuleSPIRVDescriptor` SPIR-V chain) -> `BindGroupLayout` + `PipelineLayout` -> `ComputePipeline` (`ComputePipelineDescriptor.Compute` is a `ProgrammableStageDescriptor` naming the WGSL `EntryPoint` and a `ConstantCount`/`ConstantEntry` `override`-constant block) -> `BindGroup` binding the storage/uniform `Buffer` resources. The explicit-layout path (`DeviceCreatePipelineLayout` from `BindGroupLayoutDescriptor` rows) and the auto-layout path (a null `Layout`, then `ComputePipelineGetBindGroupLayout(pipeline, groupIndex)` reads the layout the WGSL `@group` declarations imply) are both available; the auto-layout path collapses the layout-descriptor authoring for a single-kernel op whose bindings the WGSL already declares.
- `BufferDescriptor.Usage` is a `BufferUsage` flag set: `Storage` for read/write shader buffers, `Uniform` for constant blocks, `CopySrc`/`CopyDst` for device-side `CommandEncoderCopyBufferToBuffer`, `MapRead`/`MapWrite` for host mapping, `Indirect` for `DispatchWorkgroupsIndirect`, and `QueryResolve` for the resolved timestamp destination — a storage buffer carries `Storage | CopySrc`, a readback staging buffer carries `MapRead | CopyDst`.
- `BindGroupLayoutEntry.Visibility` carries `ShaderStage.Compute` for compute bindings, and `BufferBindingLayout.Type` selects `BufferBindingType.Storage` / `ReadOnlyStorage` / `Uniform`; the `BindGroupEntry` binds the concrete `Buffer` with offset and size at the matching `Binding` slot.
- A pass records `CommandEncoderBeginComputePass` -> `ComputePassEncoderSetPipeline` -> `ComputePassEncoderSetBindGroup` -> `ComputePassEncoderDispatchWorkgroups(x, y, z)` (or `DispatchWorkgroupsIndirect` reading the workgroup count from an `Indirect` buffer) -> `ComputePassEncoderEnd`; the encoder closes through `CommandEncoderFinish` into a `CommandBuffer` the `Queue` retires through `QueueSubmit`.
- Readback is two-phase: `CommandEncoderCopyBufferToBuffer` copies the `Storage` result into a `MapRead` staging buffer, then after submission `BufferMapAsync(MapMode.Read, ...)` requests the mapping, `BufferGetMapState` polls `BufferMapState.Mapped`, `BufferGetMappedRange` yields the host pointer, and `BufferUnmap` releases it; `QueueOnSubmittedWorkDone` is the submission-completion callback that gates the map request.
- A `QuerySet` of `QueryType.Timestamp` records GPU-side wall time at pass boundaries through `CommandEncoderWriteTimestamp` (or the `ComputePassDescriptor.TimestampWrites` `ComputePassTimestampWrites` begin/end-of-pass entries on `timestamp-query` feature support), and `CommandEncoderResolveQuerySet` copies the resolved nanosecond ticks into a `QueryResolve`/`CopySrc` buffer the readback path retires — the resolved per-pass GPU duration is the measured dispatch timeline, never a busy-wait fence; it stamps the `Tensor/dispatch#DEVICE_KERNELS` `ComputeReceipt.TensorRun` GPU-nanosecond duration and the winning `BenchmarkRow` device-route claim.
- The wgpu-native `Wgpu` extension table adds the measured-execution depth the base binding lacks: `ComputePassEncoderBeginPipelineStatisticsQuery`/`EndPipelineStatisticsQuery` over a `QuerySetDescriptorExtras` `PipelineStatistics` query-set record per-pass `PipelineStatisticName` counters (compute-shader-invocation count and clipper/primitive statistics) that ride the same resolve-to-`QueryResolve` readback as the timestamp set, so a compute benchmark carries invocation counts beside wall time; `QueueSubmitForIndex` returns the `ulong` submission index that `DevicePoll(device, wait: true, WrappedSubmissionIndex)` blocks on for a deterministic non-busy-wait completion, and `DevicePoll(device, wait: false, …)` is the non-blocking device-tick that advances the readback map without `QueueOnSubmittedWorkDone` polling latency.

[LOCAL_ADMISSION]:
- The Compute lane holds no second device: `ONE_WGPU_DEVICE` is the same `Device`/`Queue` the AppUi renderer owns, so the GPGPU substrate composes the renderer's already-acquired handles instead of `CreateInstance`/`AdapterRequestDevice` a second time; the adapter/device request entrypoints stay in the AppUi boundary capsule that mints the shared device.
- `TensorOpFamily` rows lower to compute-shader dispatch: each op compiles one WGSL `ShaderModule`, builds a `ComputePipeline` plus `BindGroupLayout`, and the dispatch executor binds the tensor storage `Buffer`s and issues `DispatchWorkgroups` over the tile/workgroup decomposition the op's launch geometry declares.
- All native handles (`Device`, `Queue`, `Buffer`, `ShaderModule`, `BindGroupLayout`, `BindGroup`, `PipelineLayout`, `ComputePipeline`, `CommandEncoder`, `ComputePassEncoder`, `QuerySet`) release through their matching `XxxRelease`/`XxxDestroy` native call, not `IDisposable` — the owning boundary capsule pairs create-and-release in a `using`-equivalent scoped fold; the shared `Device`/`Queue` are released by the AppUi owner, not the Compute lane.
- `DevicePoll`/device-tick advance is a wgpu-native extension absent from the base `webgpu.h` binding but PRESENT as `Wgpu.DevicePoll(Device*, Bool32 wait, WrappedSubmissionIndex*)` in the admitted `Silk.NET.WebGPU.Extensions.WGPU` package (NOT `Silk.NET.WebGPU.Native.WGPU`, which carries only native binaries) — the Compute readback path drives `DevicePoll` for the non-blocking map advance and `QueueSubmitForIndex` + `DevicePoll(wait: true, WrappedSubmissionIndex)` for a deterministic submission-completion wait, falling back to `QueueOnSubmittedWorkDone` + `BufferGetMapState` polling only where the extension table is not acquired.
- The tensor storage `Buffer` and the model-lane ORT device value resolve ONE residency through `Tensor/residency#ORT_BRIDGE` `OrtResidency.DeviceResident` — a WGPU `Buffer` admits to an `OrtValue` through `OrtValue.CreateTensorValueWithData(OrtMemoryInfo, TensorElementType, long[], nint, long)` over the buffer's mapped pointer and an ORT device output binds back through `OrtIoBinding.BindOutputToDevice(string, OrtMemoryInfo)`, so the WebGPU compute kernel and the ONNX device inference share one physical allocation with no host round-trip; device-ness is the residency discriminant, never a second tensor owner. These ORT C-data residency members (`CreateTensorValueWithData`, `CreateAllocatedTensorValue`, `CreateTensorValueFromSystemNumericsTensorObject`, `BindOutputToDevice`) are owned by `api-onnxruntime.md`, not this binding — this is a forward cross-catalog dependency, so a rebuild of `api-onnxruntime.md` that renames them re-aligns this note. A device GEMM output feeding the render lane crosses the AppUi `ResidencyManifest.Mint` seam as the same physical `Buffer`.

[RAIL_LAW]:
- Package: `Silk.NET.WebGPU` (+ `Silk.NET.WebGPU.Native.WGPU` native binaries) and `Silk.NET.WebGPU.Extensions.WGPU` (`Wgpu` extension table)
- License: MIT (all three packages)
- Owns: the GPGPU half of the managed wgpu/Dawn binding — storage/uniform/staging buffer allocation, WGSL/SPIR-V compute shader compile, bind-group layout (explicit and `ComputePipelineGetBindGroupLayout` auto-derived) and pipeline create, compute pass recording, workgroup dispatch, queue submission, host transfer/readback mapping, timestamp and pipeline-statistics query resolution, and the `Wgpu.DevicePoll`/`QueueSubmitForIndex` device-tick/submission-wait for the `ONE_WGPU_DEVICE` branch.
- Accept: `unsafe` descriptor calls on the `WebGPU.GetApi()` / `Wgpu.GetApi()` function-table roots — either `<Descriptor>*` raw-pointer or `ref readonly <Descriptor>` `in`-reference form — over the shared `Device`/`Queue` the AppUi renderer holds; native-handle scoped create-and-release pairs for compute-only resources; the `Wgpu` extension table for device-tick, submission-index, and pipeline-statistics calls.
- Reject: a second `Device`/`Queue` acquisition inside the Compute lane; a managed convenience class wrapping the native handle into a renamed surface; attributing the device-tick to `Silk.NET.WebGPU.Native.WGPU` (it lives in `Silk.NET.WebGPU.Extensions.WGPU`); CPU-side fallback math where a `TensorOpFamily` row lowers to a compute-shader dispatch.

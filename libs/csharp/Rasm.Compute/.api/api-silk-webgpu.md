# [RASM_COMPUTE_API_SILK_WEBGPU]

`Silk.NET.WebGPU` binds the managed wgpu/Dawn surface over canonical `webgpu.h`, and the Compute lane drives its GPGPU half. `WebGPU.GetApi()` roots the `unsafe` function table where a `Device` mints compute resources and a `CommandEncoder` records the `ComputePassEncoder` whose `CommandBuffer` a `Queue` retires; `Silk.NET.WebGPU.Extensions.WGPU` layers the wgpu-native compute depth the base binding lacks.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.WebGPU`
- package: `Silk.NET.WebGPU` (MIT)
- package: `Silk.NET.WebGPU.Native.WGPU` (MIT)
- assembly: `Silk.NET.WebGPU`
- namespace: `Silk.NET.WebGPU`
- asset: consumer binds the `net5.0` asset; `Silk.NET.WebGPU.Native.WGPU` ships the native `libwgpu` runtime binaries alone and carries no managed surface
- abi: every entrypoint is an `unsafe` instance method on the `WebGPU : NativeAPI` function table; native handles (`Device`/`Queue`/`Buffer`/...) are pointer-wrapped structs released through `XxxRelease`/`XxxDestroy`, never `IDisposable`
- rail: compute

[PACKAGE_SURFACE]: `Silk.NET.WebGPU.Extensions.WGPU`
- package: `Silk.NET.WebGPU.Extensions.WGPU` (MIT)
- assembly: `Silk.NET.WebGPU.Extensions.WGPU`
- namespace: `Silk.NET.WebGPU.Extensions.WGPU`
- asset: consumer binds the `net5.0` asset; wgpu-native function extensions over the base binding
- abi: `Wgpu : NativeExtension<WebGPU>` loads through `WebGPU.TryGetDeviceExtension<Wgpu>` or `new Wgpu(webgpu.Context)` as a second function-table view over the one runtime, carrying the device-tick, submission-index, pipeline-statistics, push-constant, and synchronous-adapter-enumeration entrypoints `webgpu.h` omits
- rail: compute

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: device lifecycle and submission owners

| [INDEX] | [SYMBOL]   | [TYPE_FAMILY]   | [CAPABILITY]                 |
| :-----: | :--------- | :-------------- | :--------------------------- |
|  [01]   | `WebGPU`   | static API root | global entry, function table |
|  [02]   | `Instance` | native handle   | adapter request root         |
|  [03]   | `Adapter`  | native handle   | device request, limits query |
|  [04]   | `Device`   | native handle   | resource factory             |
|  [05]   | `Queue`    | native handle   | command submission, upload   |

[PUBLIC_TYPE_SCOPE]: compute resource and dispatch owners

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [CAPABILITY]                  |
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

| [INDEX] | [SYMBOL]                      | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :---------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `BufferDescriptor`            | descriptor    | buffer alloc (`Usage`, `Size`, `MappedAtCreation`)                |
|  [02]   | `ShaderModuleDescriptor`      | descriptor    | shader module create (`NextInChain` source chain)                 |
|  [03]   | `ShaderModuleWGSLDescriptor`  | descriptor    | WGSL source chain (`SType.ShaderModuleWgslDescriptor`)            |
|  [04]   | `ShaderModuleSPIRVDescriptor` | descriptor    | SPIR-V source chain (`Code`, `CodeSize`)                          |
|  [05]   | `BindGroupLayoutDescriptor`   | descriptor    | layout entries (`Entries`, `EntryCount`)                          |
|  [06]   | `BindGroupLayoutEntry`        | descriptor    | one binding slot (`Binding`, `Visibility`, `Buffer`)              |
|  [07]   | `BufferBindingLayout`         | descriptor    | binding kind (`Type`, `HasDynamicOffset`, `MinBindingSize`)       |
|  [08]   | `BindGroupDescriptor`         | descriptor    | bound entries (`Layout`, `Entries`)                               |
|  [09]   | `BindGroupEntry`              | descriptor    | one bound resource (`Binding`, `Buffer`, `Offset`, `Size`)        |
|  [10]   | `PipelineLayoutDescriptor`    | descriptor    | layout set (`BindGroupLayouts`)                                   |
|  [11]   | `ComputePipelineDescriptor`   | descriptor    | compute pipeline (`Layout`, `Compute` stage)                      |
|  [12]   | `ProgrammableStageDescriptor` | descriptor    | `Module`, `EntryPoint`, `ConstantCount`, `Constants`              |
|  [13]   | `ConstantEntry`               | descriptor    | one WGSL `override` constant (`Key`, `Value` double)              |
|  [14]   | `QuerySetDescriptor`          | descriptor    | query-set alloc (`Type`, `Count`)                                 |
|  [15]   | `ComputePassDescriptor`       | descriptor    | pass begin (`Label`, `TimestampWrites`)                           |
|  [16]   | `ComputePassTimestampWrites`  | descriptor    | `QuerySet`, `BeginningOfPassWriteIndex`, `EndOfPassWriteIndex`    |
|  [17]   | `SupportedLimits` / `Limits`  | struct        | `DeviceGetLimits` out-struct; `maxComputeWorkgroupSize*` etc.     |
|  [18]   | `BufferUsage`                 | enum          | storage/uniform/copy/map/indirect/queryresolve                    |
|  [19]   | `MapMode`                     | enum          | readback/upload map direction                                     |
|  [20]   | `ShaderStage`                 | enum          | `Compute` visibility flag                                         |
|  [21]   | `BufferBindingType`           | enum          | `Uniform`/`Storage`/`ReadOnlyStorage` binding kind                |
|  [22]   | `BufferMapState`              | enum          | unmapped/pending/mapped poll                                      |
|  [23]   | `QueryType`                   | enum          | timestamp/occlusion query kind                                    |
|  [24]   | `ErrorFilter`                 | enum          | `Validation`/`OutOfMemory`/`Internal` capture-scope selector      |
|  [25]   | `ErrorType`                   | enum          | NoError/Validation/OutOfMemory/Internal/Unknown/DeviceLost result |

[PUBLIC_TYPE_SCOPE]: wgpu-native extension owners (`Silk.NET.WebGPU.Extensions.WGPU`)

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]    | [CAPABILITY]                                                      |
| :-----: | :-------------------------------- | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `Wgpu`                            | native extension | loaded wgpu-native function table over the one runtime            |
|  [02]   | `WrappedSubmissionIndex`          | struct           | `Queue` + submission `ulong` index pairing for `DevicePoll` wait  |
|  [03]   | `QuerySetDescriptorExtras`        | descriptor       | `PipelineStatistics` query-set chain (`SType` extras)             |
|  [04]   | `PipelineStatisticName`           | enum             | vertex/clipper/primitive/fragment/compute-invocation statistic    |
|  [05]   | `NativeFeature`                   | enum             | `PushConstants`/`PipelineStatisticsQuery`/multi-draw feature bits |
|  [06]   | `NativeLimits`                    | struct           | wgpu-native limit extras (`MaxPushConstantSize`)                  |
|  [07]   | `InstanceBackend`                 | enum             | Vulkan/Metal/DX12/GL backend selection for `InstanceExtras`       |
|  [08]   | `InstanceEnumerateAdapterOptions` | descriptor       | backend mask for the synchronous adapter enumeration              |
|  [09]   | `ShaderModuleGLSLDescriptor`      | descriptor       | GLSL compute source chain (wgpu-native only)                      |

## [03]-[ENTRYPOINTS]

Each descriptor-taking call ships paired overloads — a raw `<Descriptor>*` form and a `ref readonly <Descriptor>` `in`-reference form binding the same native call. `callback, userdata` abbreviates the member's own `Pfn…Callback` delegate beside its `void*` state slot, and `<T0>(… ref T0)` generic overloads thread typed `unmanaged` userdata without a `void*` cast. Surfaces in the terminal scope bind the loaded `Wgpu` table; every other surface binds the `WebGPU` root.

[ENTRYPOINT_SCOPE]: device and queue acquisition

| [INDEX] | [SURFACE]                                                                       | [SHAPE]  | [CAPABILITY]         |
| :-----: | :------------------------------------------------------------------------------ | :------- | :------------------- |
|  [01]   | `WebGPU.GetApi()`                                                               | static   | API root load        |
|  [02]   | `CreateInstance(InstanceDescriptor*)`                                           | instance | instance create      |
|  [03]   | `InstanceRequestAdapter(Instance*, RequestAdapterOptions*, callback, userdata)` | instance | adapter request      |
|  [04]   | `AdapterRequestDevice(Adapter*, DeviceDescriptor*, callback, userdata)`         | instance | device request       |
|  [05]   | `DeviceGetQueue(Device*)`                                                       | instance | queue handle         |
|  [06]   | `DeviceGetLimits(Device*, SupportedLimits*) -> Bool32`                          | instance | workgroup limits     |
|  [07]   | `IsDeviceExtensionPresent(Device*, string) -> bool`                             | instance | wgpu-native probe    |
|  [08]   | `TryGetDeviceExtension<Wgpu>(Device*, out Wgpu) -> bool`                        | instance | extension table load |

[ENTRYPOINT_SCOPE]: compute resource and pipeline creation

| [INDEX] | [SURFACE]                                                                                   | [SHAPE]  | [CAPABILITY]           |
| :-----: | :------------------------------------------------------------------------------------------ | :------- | :--------------------- |
|  [01]   | `DeviceCreateBuffer(Device*, BufferDescriptor*)`                                            | instance | storage/staging alloc  |
|  [02]   | `DeviceCreateShaderModule(Device*, ShaderModuleDescriptor*)`                                | instance | compute shader compile |
|  [03]   | `DeviceCreateBindGroupLayout(Device*, BindGroupLayoutDescriptor*)`                          | instance | binding layout         |
|  [04]   | `DeviceCreateBindGroup(Device*, BindGroupDescriptor*)`                                      | instance | bound resource group   |
|  [05]   | `DeviceCreatePipelineLayout(Device*, PipelineLayoutDescriptor*)`                            | instance | pipeline layout        |
|  [06]   | `DeviceCreateComputePipeline(Device*, ComputePipelineDescriptor*)`                          | instance | compute pipeline       |
|  [07]   | `DeviceCreateComputePipelineAsync(Device*, ComputePipelineDescriptor*, callback, userdata)` | instance | async pipeline compile |
|  [08]   | `ComputePipelineGetBindGroupLayout(ComputePipeline*, uint) -> BindGroupLayout*`             | instance | auto-derive layout     |
|  [09]   | `DeviceCreateCommandEncoder(Device*, CommandEncoderDescriptor*)`                            | instance | encoder create         |
|  [10]   | `DeviceCreateQuerySet(Device*, QuerySetDescriptor*)`                                        | instance | timestamp query-set    |

[ENTRYPOINT_SCOPE]: compute-handle release

| [INDEX] | [SURFACE]                                        | [SHAPE]  | [CAPABILITY]      |
| :-----: | :----------------------------------------------- | :------- | :---------------- |
|  [01]   | `ShaderModuleRelease(ShaderModule*)`             | instance | shader release    |
|  [02]   | `BindGroupLayoutRelease(BindGroupLayout*)`       | instance | layout release    |
|  [03]   | `BindGroupRelease(BindGroup*)`                   | instance | group release     |
|  [04]   | `ComputePipelineRelease(ComputePipeline*)`       | instance | pipeline release  |
|  [05]   | `ComputePassEncoderRelease(ComputePassEncoder*)` | instance | pass release      |
|  [06]   | `CommandEncoderRelease(CommandEncoder*)`         | instance | encoder release   |
|  [07]   | `CommandBufferRelease(CommandBuffer*)`           | instance | commands release  |
|  [08]   | `QuerySetRelease(QuerySet*)`                     | instance | query-set release |
|  [09]   | `BufferRelease(Buffer*)`                         | instance | buffer release    |

[ENTRYPOINT_SCOPE]: compute pass recording and submission

| [INDEX] | [SURFACE]                                                                             | [SHAPE]  | [CAPABILITY]          |
| :-----: | :------------------------------------------------------------------------------------ | :------- | :-------------------- |
|  [01]   | `CommandEncoderBeginComputePass(CommandEncoder*, ComputePassDescriptor*)`             | instance | compute pass begin    |
|  [02]   | `ComputePassEncoderSetPipeline(ComputePassEncoder*, ComputePipeline*)`                | instance | bind compute pipeline |
|  [03]   | `ComputePassEncoderSetBindGroup(ComputePassEncoder*, uint, BindGroup*, nuint, uint*)` | instance | bind resource group   |
|  [04]   | `ComputePassEncoderDispatchWorkgroups(ComputePassEncoder*, uint, uint, uint)`         | instance | workgroup dispatch    |
|  [05]   | `ComputePassEncoderDispatchWorkgroupsIndirect(ComputePassEncoder*, Buffer*, ulong)`   | instance | indirect dispatch     |
|  [06]   | `ComputePassEncoderEnd(ComputePassEncoder*)`                                          | instance | close compute pass    |
|  [07]   | `CommandEncoderFinish(CommandEncoder*, CommandBufferDescriptor*)`                     | instance | finish command buffer |
|  [08]   | `QueueSubmit(Queue*, nuint, CommandBuffer**)`                                         | instance | submit to GPU         |

[ENTRYPOINT_SCOPE]: host transfer, readback, and timing

| [INDEX] | [SURFACE]                                                                                  | [SHAPE]  | [CAPABILITY]            |
| :-----: | :----------------------------------------------------------------------------------------- | :------- | :---------------------- |
|  [01]   | `QueueWriteBuffer(Queue*, Buffer*, ulong, void*, nuint)`                                   | instance | host-to-GPU upload      |
|  [02]   | `CommandEncoderCopyBufferToBuffer(CommandEncoder*, Buffer*, ulong, Buffer*, ulong, ulong)` | instance | device-side copy        |
|  [03]   | `BufferMapAsync(Buffer*, MapMode, nuint, nuint, callback, userdata)`                       | instance | request mapping         |
|  [04]   | `BufferGetMappedRange(Buffer*, nuint, nuint) -> void*`                                     | instance | mapped readback pointer |
|  [05]   | `BufferGetMapState(Buffer*) -> BufferMapState`                                             | instance | poll the map state      |
|  [06]   | `BufferUnmap(Buffer*)`                                                                     | instance | release mapping         |
|  [07]   | `QueueOnSubmittedWorkDone(Queue*, callback, userdata)`                                     | instance | submission completion   |
|  [08]   | `CommandEncoderWriteTimestamp(CommandEncoder*, QuerySet*, uint)`                           | instance | per-pass timestamp      |
|  [09]   | `CommandEncoderResolveQuerySet(CommandEncoder*, QuerySet*, uint, uint, Buffer*, ulong)`    | instance | resolve to read buffer  |
|  [10]   | `QuerySetGetCount(QuerySet*) -> uint`                                                      | instance | resolved query count    |
|  [11]   | `QuerySetGetType(QuerySet*) -> QueryType`                                                  | instance | query-set kind          |

[ENTRYPOINT_SCOPE]: pass observability and debug grouping

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]           |
| :-----: | :---------------------------------------------------------------- | :------- | :--------------------- |
|  [01]   | `ComputePassEncoderSetLabel(ComputePassEncoder*, byte*)`          | instance | pass label for capture |
|  [02]   | `ComputePassEncoderInsertDebugMarker(ComputePassEncoder*, byte*)` | instance | inline debug marker    |
|  [03]   | `ComputePassEncoderPushDebugGroup(ComputePassEncoder*, byte*)`    | instance | open debug group       |
|  [04]   | `ComputePassEncoderPopDebugGroup(ComputePassEncoder*)`            | instance | close debug group      |
|  [05]   | `ComputePipelineSetLabel(ComputePipeline*, byte*)`                | instance | pipeline label         |

[ENTRYPOINT_SCOPE]: device error capture

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]         |
| :-----: | :-------------------------------------------------------------- | :------- | :------------------- |
|  [01]   | `DevicePushErrorScope(Device*, ErrorFilter)`                    | instance | open a capture scope |
|  [02]   | `DevicePopErrorScope(Device*, callback, userdata)`              | instance | drain the open scope |
|  [03]   | `DeviceSetUncapturedErrorCallback(Device*, callback, userdata)` | instance | unscoped error sink  |

[ENTRYPOINT_SCOPE]: wgpu-native extension calls on the loaded `Wgpu` table

| [INDEX] | [SURFACE]                                                                                    | [SHAPE]  | [CAPABILITY]               |
| :-----: | :------------------------------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `DevicePoll(Device*, Bool32, WrappedSubmissionIndex*) -> Bool32`                             | instance | non-blocking device-tick   |
|  [02]   | `QueueSubmitForIndex(Queue*, nuint, CommandBuffer**) -> ulong`                               | instance | submit + return wait index |
|  [03]   | `ComputePassEncoderBeginPipelineStatisticsQuery(ComputePassEncoder*, QuerySet*, uint)`       | instance | begin pipeline-statistics  |
|  [04]   | `ComputePassEncoderEndPipelineStatisticsQuery(ComputePassEncoder*)`                          | instance | end pipeline-statistics    |
|  [05]   | `InstanceEnumerateAdapters(Instance*, InstanceEnumerateAdapterOptions*, Adapter**) -> nuint` | instance | synchronous adapter list   |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `WebGPU.GetApi()` roots the function table; every op marshals `stackalloc`/`Span<T>` descriptors by pointer, never a managed wrapper — the Compute capsule binds the `ref readonly` descriptor overload (no manual pin) and reserves the `byte*` form for hot WGSL/label paths it owns as fixed buffers.
- Compute lifecycle: `Device` -> `ShaderModule` (WGSL `ShaderModuleWGSLDescriptor` or SPIR-V `ShaderModuleSPIRVDescriptor` chain) -> `BindGroupLayout` + `PipelineLayout` -> `ComputePipeline` (`ComputePipelineDescriptor.Compute` names the WGSL `EntryPoint` and a `ConstantEntry` `override`-constant block) -> `BindGroup` over storage/uniform `Buffer`s. Explicit layout (`DeviceCreatePipelineLayout`) and auto layout (null `Layout`, then `ComputePipelineGetBindGroupLayout` reads what the WGSL `@group` declarations imply) both bind; auto-layout collapses layout authoring for a single-kernel op.
- `BufferDescriptor.Usage` maps role to flags: `Storage` read/write buffers, `Uniform` constant blocks, `CopySrc`/`CopyDst` device copy, `MapRead`/`MapWrite` host mapping, `Indirect` for `DispatchWorkgroupsIndirect`, `QueryResolve` for resolved timestamps — a storage buffer carries `Storage | CopySrc`, a readback staging buffer `MapRead | CopyDst`. `BindGroupLayoutEntry.Visibility` carries `ShaderStage.Compute` and `BufferBindingLayout.Type` selects `Storage`/`ReadOnlyStorage`/`Uniform` at the matching `Binding` slot.
- Pass record: `CommandEncoderBeginComputePass` -> `SetPipeline` -> `SetBindGroup` -> `DispatchWorkgroups(x, y, z)` (or `DispatchWorkgroupsIndirect` reading the count from an `Indirect` buffer) -> `End`; `CommandEncoderFinish` seals a `CommandBuffer` the `Queue` retires through `QueueSubmit`.
- Readback is two-phase: `CommandEncoderCopyBufferToBuffer` copies the `Storage` result into a `MapRead` staging buffer, then `BufferMapAsync(MapMode.Read)` requests the map, `BufferGetMapState` polls `BufferMapState.Mapped`, `BufferGetMappedRange` yields the host pointer, `BufferUnmap` releases it; `QueueOnSubmittedWorkDone` gates the map request.
- `QueryType.Timestamp` `QuerySet` stamps GPU wall time at pass boundaries (`CommandEncoderWriteTimestamp` or `ComputePassDescriptor.TimestampWrites`), and `CommandEncoderResolveQuerySet` copies the nanosecond ticks into a `QueryResolve`/`CopySrc` buffer — the resolved per-pass duration stamps the `ComputeReceipt.TensorRun` GPU-nanosecond field and the winning `BenchmarkRow` device-route claim.
- `Wgpu` adds pipeline-statistics (`ComputePassEncoderBeginPipelineStatisticsQuery`/`EndPipelineStatisticsQuery` over a `QuerySetDescriptorExtras` set resolving `PipelineStatisticName` invocation counts beside wall time) and deterministic completion: `QueueSubmitForIndex` returns the `ulong` index `DevicePoll(wait: true, WrappedSubmissionIndex)` blocks on, and `DevicePoll(wait: false)` is the non-blocking device-tick advancing the readback map without `QueueOnSubmittedWorkDone` latency.

[STACKING]:
- `api-onnxruntime.md`(`.api/api-onnxruntime.md`): a WGPU storage `Buffer` and the model-lane ORT device value resolve ONE residency — the buffer's mapped pointer admits to an `OrtValue` through `OrtValue.CreateTensorValueWithData(OrtMemoryInfo, TensorElementType, long[], nint, long)` and an ORT device output binds back through `OrtIoBinding.BindOutputToDevice(string, OrtMemoryInfo)`, so the compute kernel and ONNX device inference share one physical allocation with no host round-trip; those residency members are owned by `api-onnxruntime.md`.
- `api-bcl-interop.md`(`libs/csharp/.api/api-bcl-interop.md`): `ShaderModuleWGSLDescriptor.Code` and `ProgrammableStageDescriptor.EntryPoint` bind a native UTF-8 `byte*`, so `Marshal.StringToCoTaskMemUTF8` mints the WGSL source and entry-point block, `Marshal.FreeCoTaskMem` retires both in the compile fold's `finally`, and `Marshal.PtrToStringUTF8` drains the `PfnErrorCallback` message — one interop owner marshals both directions across this boundary.
- AppUi `ONE_WGPU_DEVICE`: the GPGPU substrate composes the renderer's already-acquired `Device`/`Queue` shared handle, and a device GEMM output feeding the render lane crosses the AppUi `ResidencyManifest.Mint` seam as the same physical `Buffer`.
- within-lib: `TensorOpFamily` rows lower to compute dispatch — each op compiles one WGSL `ShaderModule`, builds a `ComputePipeline` + `BindGroupLayout`, and the dispatch executor binds the tensor storage `Buffer`s and issues `DispatchWorkgroups` over the op's declared launch geometry.

[LOCAL_ADMISSION]:
- Compute acquires no device: `ONE_WGPU_DEVICE` is the renderer's `Device`/`Queue`, so the adapter/device-request entrypoints stay in the AppUi boundary capsule that mints the shared device.
- Native handles release through their matching `XxxRelease`/`XxxDestroy` in a scoped create-and-release fold, never `IDisposable`; the shared `Device`/`Queue` release at the AppUi owner, not the Compute lane.
- `DevicePoll` and the device-tick live in `Silk.NET.WebGPU.Extensions.WGPU` (`Wgpu.DevicePoll`), not `Silk.NET.WebGPU.Native.WGPU` (native binaries only); the readback path drives `DevicePoll` for the non-blocking map advance and `QueueSubmitForIndex` + `DevicePoll(wait: true)` for deterministic completion, falling back to `QueueOnSubmittedWorkDone` + `BufferGetMapState` only where the extension table is not acquired.
- `Wgpu` arrives injected beside the `WebGPU` root: the AppUi capsule minting the shared device also loads the one extension view through `TryGetDeviceExtension<Wgpu>` and hands both tables to the Compute capsule, so the compute lane probes availability off the injected handle rather than reloading a second view.

[RAIL_LAW]:
- Package: `Silk.NET.WebGPU` + `Silk.NET.WebGPU.Native.WGPU` (native binaries) + `Silk.NET.WebGPU.Extensions.WGPU` (`Wgpu` table)
- Owns: the GPGPU half of the managed wgpu/Dawn binding — buffer allocation, WGSL/SPIR-V compute compile, bind-group layout (explicit and `ComputePipelineGetBindGroupLayout` auto-derived) and pipeline create, pass recording, workgroup dispatch, queue submission, host readback mapping, timestamp and pipeline-statistics resolution, and the `Wgpu.DevicePoll`/`QueueSubmitForIndex` device-tick for the `ONE_WGPU_DEVICE` branch.
- Accept: `unsafe` descriptor calls on the `WebGPU` root and the loaded `Wgpu` table — `<Descriptor>*` raw-pointer or `ref readonly <Descriptor>` `in`-reference — over the shared `Device`/`Queue`; scoped create-and-release pairs for compute-only resources; the `Wgpu` table for device-tick, submission-index, and pipeline-statistics.
- Reject: a second `Device`/`Queue` in the Compute lane; a managed class wrapping the native handle into a renamed surface; a `GetApi()` static on the extension table where `TryGetDeviceExtension<Wgpu>` loads it; attributing the device-tick to `Silk.NET.WebGPU.Native.WGPU`; CPU fallback math where a `TensorOpFamily` row lowers to dispatch.

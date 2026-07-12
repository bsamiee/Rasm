# [RASM_APPUI_API_SILK_WEBGPU_WGPU]

`Silk.NET.WebGPU.Extensions.WGPU` is the wgpu-native (`wgpu.h`) vendor-extension binding layered over the canonical `Silk.NET.WebGPU` core (`webgpu.h`): the upstream standard surface omits the non-blocking submission-completion poll, the native log/error callbacks, full-adapter enumeration, GPU-driven indirect multi-draw, push constants, and the bindless descriptor-array `next`-chain a desktop render loop demands, and `wgpu_native` (the runtime `Silk.NET.WebGPU.Native.WGPU` already carries) adds them. `Wgpu : NativeExtension<WebGPU>` is the generated function-table root for those native-only entrypoints — `DevicePoll` advances the device queue without a blocking fence, `SetLogCallback`/`SetLogLevel` route the native diagnostic stream into the receipt sink, `InstanceEnumerateAdapters` returns every physical adapter for the compositor-LUID match, `RenderPassEncoderMultiDraw*`/`SetPushConstants` drive the meshlet cluster pass, and `QueueSubmitForIndex` mints the `WrappedSubmissionIndex` `DevicePoll` waits on. The extension is loaded against the existing core, not separately constructed: `new Wgpu(webgpu.Context)` (or `webgpu.TryGetExtension(out Wgpu wgpu)`) yields a second function-table view over the one loaded `wgpu_native` runtime, so a call site holds one `WebGPU` core and one `Wgpu` extension and never a second binding.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.WebGPU.Extensions.WGPU`

- package: `Silk.NET.WebGPU.Extensions.WGPU`
- assembly: `Silk.NET.WebGPU.Extensions.WGPU`
- namespace: `Silk.NET.WebGPU.Extensions.WGPU`
- asset: managed binding only — P/Invokes the `wgpu_native` binaries `Silk.NET.WebGPU.Native.WGPU` restores; zero additional native asset
- tfm: `net5.0` (consumer-bound by NuGet precedence; the `Wgpu` public surface is identical across the package's `net5.0`/`netcoreapp3.1`/`netstandard2.1` assets, so the resolved-vs-bound TFM diff is empty here)
- license: `MIT`
- rail: viewport

## [02]-[PUBLIC_TYPES]

[EXTENSION_ROOT]: function-table root and its Span/managed overload mirror

- rail: viewport

`Wgpu` takes the core context, and `WgpuOverloads` supplies the `Span<T>` and managed-argument mirrors.

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]                | [CAPABILITY]               |
| :-----: | :-------------- | :--------------------------- | :------------------------- |
|  [01]   | `Wgpu`          | `NativeExtension<WebGPU>`    | wgpu-native function table |
|  [02]   | `WgpuOverloads` | static `this Wgpu` extension | managed overload mirror    |

[ENUMS]: backend/log/chain-type/compiler vocabularies

- rail: viewport

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]        |
| :-----: | :---------------------- | :------------ | :------------------ |
|  [01]   | `InstanceBackend`       | flags enum    | backend mask        |
|  [02]   | `InstanceFlag`          | flags enum    | instance debug mask |
|  [03]   | `LogLevel`              | enum          | log verbosity       |
|  [04]   | `NativeSType`           | enum          | chain type tag      |
|  [05]   | `Dx12Compiler`          | enum          | D3D12 compiler      |
|  [06]   | `Gles3MinorVersion`     | enum          | GLES minor version  |
|  [07]   | `NativeFeature`         | enum          | native feature      |
|  [08]   | `NativeQueryType`       | enum          | native query kind   |
|  [09]   | `PipelineStatisticName` | enum          | statistic selector  |

[ENUM_MEMBERS]:

- `InstanceBackend`: `All`/`Vulkan`/`GL`/`Metal`/`DX12`/`DX11`/`BrowserWebGpu`/`Primary`/`Secondary`
- `InstanceFlag`: `Default`/`Debug`/`Validation`/`DiscardHalLabels`
- `LogLevel`: `Off`/`Error`/`Warn`/`Info`/`Debug`/`Trace`
- `NativeSType`: wgpu-native `next`-chain tags such as `STypeInstanceExtras`
- `Dx12Compiler`: `Undefined`/`Fxc`/`Dxc`

`NativeFeature` extends the standard `FeatureName`, `NativeQueryType` admits pipeline-statistics query sets, `Gles3MinorVersion` pins the GLES 3.x minor version for the GL backend, and `PipelineStatisticName` selects each reported statistic.

[CALLBACKS]: native log delegate and its function-pointer wrapper

- rail: viewport

`PfnLogCallback` wraps the native function pointer, constructs through `From(LogCallback)` or implicit `nint`, and is the type `SetLogCallback` binds.

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]                   | [CAPABILITY]             |
| :-----: | :--------------- | :------------------------------ | :----------------------- |
|  [01]   | `LogCallback`    | `unsafe delegate`               | managed sink             |
|  [02]   | `PfnLogCallback` | `readonly struct : IDisposable` | function-pointer wrapper |

[LOG_CALLBACK_SIGNATURE]: `void(LogLevel, byte* message, void* userdata)`

[NEXT_CHAIN_STRUCTS]: `ChainedStruct`-headed descriptor extensions threaded through the standard descriptor `NextInChain`

- rail: viewport

| [INDEX] | [SYMBOL]                     | [CAPABILITY]            |
| :-----: | :--------------------------- | :---------------------- |
|  [01]   | `InstanceExtras`             | instance creation       |
|  [02]   | `DeviceExtras`               | trace capture           |
|  [03]   | `RequiredLimitsExtras`       | required native limits  |
|  [04]   | `SupportedLimitsExtras`      | supported native limits |
|  [05]   | `SurfaceConfigurationExtras` | present latency         |
|  [06]   | `PipelineLayoutExtras`       | push-constant layout    |
|  [07]   | `PushConstantRange`          | stage byte range        |
|  [08]   | `BindGroupEntryExtras`       | descriptor arrays       |
|  [09]   | `BindGroupLayoutEntryExtras` | array binding count     |
|  [10]   | `QuerySetDescriptorExtras`   | pipeline statistics     |
|  [11]   | `ShaderModuleGLSLDescriptor` | GLSL compilation        |
|  [12]   | `ShaderDefine`               | compile definition      |

[NEXT_CHAIN_FIELDS]:

- `InstanceExtras`: `Chain`, `Backends:InstanceBackend`, `Flags:uint`, `DxilPath`, `DxcPath`
- `DeviceExtras`: `Chain`, `TracePath:byte*`
- `RequiredLimitsExtras`: `Chain`, `Limits:NativeLimits`
- `SupportedLimitsExtras`: `Chain`, `Limits:NativeLimits`
- `SurfaceConfigurationExtras`: `Chain`, `DesiredMaximumFrameLatency:Bool32`
- `PipelineLayoutExtras`: `Chain`, `PushConstantRangeCount`, `PushConstantRanges:PushConstantRange*`
- `PushConstantRange`: `Stages:ShaderStage`, `Start:uint`, `End:uint`
- `BindGroupEntryExtras`: `Chain`, `Buffers:Buffer**`, `Samplers:Sampler**`, `TextureViews:TextureView**`, and their counts
- `BindGroupLayoutEntryExtras`: `Chain`, `Count:uint`
- `QuerySetDescriptorExtras`: `Chain` and pipeline-statistic selectors
- `ShaderModuleGLSLDescriptor`: `Chain`, GLSL source, and `ShaderDefine` entries for the GL backend
- `ShaderDefine`: name and value

[NATIVE_VALUE_STRUCTS]: submission index, enumeration options, and the memory/registry report tree

- rail: viewport

| [INDEX] | [SYMBOL]                          | [CAPABILITY]        |
| :-----: | :-------------------------------- | :------------------ |
|  [01]   | `WrappedSubmissionIndex`          | submission tracking |
|  [02]   | `InstanceEnumerateAdapterOptions` | backend filtering   |
|  [03]   | `GlobalReport`                    | memory snapshot     |
|  [04]   | `HubReport`                       | backend report hub  |
|  [05]   | `RegistryReport`                  | allocation census   |
|  [06]   | `NativeLimits`                    | native limit values |

[NATIVE_VALUE_FIELDS]:

- `WrappedSubmissionIndex`: `Queue:Queue*`, `SubmissionIndex:ulong`
- `InstanceEnumerateAdapterOptions`: `NextInChain:ChainedStruct*`, `Backends:InstanceBackend`
- `GlobalReport`: top-level `GenerateReport` snapshot with per-backend `HubReport` values plus surface and instance counts
- `HubReport`: per-resource-kind `RegistryReport` values
- `RegistryReport`: `NumAllocated`, `NumKeptFromUser`, `NumReleasedFromUser`, `NumError`, `ElementSize`
- `NativeLimits`: `MaxPushConstantSize`, `MaxNonSamplerBindings`

`QueueSubmitForIndex` mints `WrappedSubmissionIndex`, and `DevicePoll` waits on it.

## [03]-[ENTRYPOINTS]

[LIFECYCLE_AND_DIAGNOSTICS]: load, queue poll, native log, version, and the memory report

- rail: viewport

Construction binds the core `WebGPU`; every remaining lifecycle and diagnostic entrypoint belongs to `Wgpu`.

| [INDEX] | [SURFACE]            | [CAPABILITY]             |
| :-----: | :------------------- | :----------------------- |
|  [01]   | `Wgpu`               | extension load           |
|  [02]   | `TryGetExtension`    | extension lookup         |
|  [03]   | `DevicePoll`         | queue completion advance |
|  [04]   | `SetLogCallback`     | native log routing       |
|  [05]   | `SetLogCallback<T0>` | typed userdata           |
|  [06]   | `SetLogLevel`        | verbosity floor          |
|  [07]   | `GetVersion`         | runtime version          |
|  [08]   | `GenerateReport`     | allocation snapshot      |

[LIFECYCLE_SIGNATURES]:

- `new Wgpu(webgpu.Context)`
- `webgpu.TryGetExtension(out Wgpu)`
- `DevicePoll(Device*, Bool32 wait, ref/WrappedSubmissionIndex*) -> Bool32`
- `SetLogCallback(PfnLogCallback callback, void* userdata)`
- `SetLogCallback<T0>(PfnLogCallback, ref T0 userdata) where T0:unmanaged`
- `SetLogLevel(LogLevel level)`
- `GetVersion() -> uint`
- `GenerateReport(Instance*, ref/GlobalReport)`

[ADAPTER_AND_SUBMISSION]: full-adapter enumeration and the submission-index handshake

- rail: viewport

Both entrypoints belong to `Wgpu`.

| [INDEX] | [SURFACE]                   | [CAPABILITY]        |
| :-----: | :-------------------------- | :------------------ |
|  [01]   | `InstanceEnumerateAdapters` | adapter enumeration |
|  [02]   | `QueueSubmitForIndex`       | indexed submission  |

[ADAPTER_AND_SUBMISSION_SIGNATURES]:

- `InstanceEnumerateAdapters(Instance*, ref/InstanceEnumerateAdapterOptions*, ref/Adapter*) -> nuint`
- `QueueSubmitForIndex(Queue*, nuint count, ref/CommandBuffer**) -> ulong`

[GPU_DRIVEN_RENDERING]: indirect multi-draw, count-buffer culling, and push constants

- rail: viewport

Every GPU-driven entrypoint belongs to `Wgpu`.

| [INDEX] | [SURFACE]                                        | [CAPABILITY]         |
| :-----: | :----------------------------------------------- | :------------------- |
|  [01]   | `RenderPassEncoderMultiDrawIndirect`             | indirect multi-draw  |
|  [02]   | `RenderPassEncoderMultiDrawIndexedIndirect`      | indexed multi-draw   |
|  [03]   | `RenderPassEncoderMultiDrawIndirectCount`        | buffered draw count  |
|  [04]   | `RenderPassEncoderMultiDrawIndexedIndirectCount` | indexed draw count   |
|  [05]   | `RenderPassEncoderSetPushConstants`              | push constants       |
|  [06]   | `RenderPassEncoderSetPushConstants<T0>`          | typed push constants |

[GPU_DRIVEN_SIGNATURES]:

- `RenderPassEncoderMultiDrawIndirect(RenderPassEncoder*, Buffer*, ulong offset, uint count)`
- `RenderPassEncoderMultiDrawIndexedIndirect(RenderPassEncoder*, Buffer*, ulong offset, uint count)`
- `RenderPassEncoderMultiDrawIndirectCount(encoder, buffer, offset, Buffer* countBuffer, ulong countOffset, uint maxCount)`
- `RenderPassEncoderMultiDrawIndexedIndirectCount(encoder, buffer, offset, countBuffer, countOffset, maxCount)`
- `RenderPassEncoderSetPushConstants(RenderPassEncoder*, ShaderStage stages, uint offset, uint sizeBytes, void* data)`
- `RenderPassEncoderSetPushConstants<T0>(encoder, stages, offset, sizeBytes, ref readonly T0) where T0:unmanaged`

[PIPELINE_STATISTICS]: query-driven pass profiling

- rail: viewport

Every pipeline-statistics entrypoint belongs to `Wgpu`.

| [INDEX] | [SURFACE]                                        | [PASS]  | [ACTION] |
| :-----: | :----------------------------------------------- | :------ | :------- |
|  [01]   | `RenderPassEncoderBeginPipelineStatisticsQuery`  | render  | begin    |
|  [02]   | `RenderPassEncoderEndPipelineStatisticsQuery`    | render  | end      |
|  [03]   | `ComputePassEncoderBeginPipelineStatisticsQuery` | compute | begin    |
|  [04]   | `ComputePassEncoderEndPipelineStatisticsQuery`   | compute | end      |

[PIPELINE_STATISTICS_SIGNATURES]:

- `RenderPassEncoderBeginPipelineStatisticsQuery(RenderPassEncoder*, QuerySet*, uint index)`
- `RenderPassEncoderEndPipelineStatisticsQuery(RenderPassEncoder*)`
- `ComputePassEncoderBeginPipelineStatisticsQuery(ComputePassEncoder*, QuerySet*, uint index)`
- `ComputePassEncoderEndPipelineStatisticsQuery(ComputePassEncoder*)`

## [04]-[IMPLEMENTATION_LAW]

[WGPU_EXTENSION_TOPOLOGY]:

- The extension loads through the Silk.NET `NativeExtension<WebGPU>` convention against the live core: `new Wgpu(webgpu.Context)` (or `webgpu.TryGetExtension(out Wgpu wgpu)`) — never `Wgpu.GetApi(...)`, which is the core `WebGPU.GetApi()` static, not an extension entrypoint. The result is a second function-table view over the one loaded `wgpu_native` runtime; a call site holds one `WebGPU` core and one `Wgpu` extension.
- `DevicePoll(device, wait:false, submissionIndex)` advances submission completion once per frame, retires command buffers, and runs mapped-buffer callbacks without blocking the UI thread. `wait:true` drains during teardown, while `InstanceProcessEvents` supplies the standard blocking pump.
- Core `WebGPU` records `CommandEncoderWriteTimestamp` or `RenderPassTimestampWrites` into a `QueryType.Timestamp` `QuerySet`, then `CommandEncoderResolveQuerySet` copies the ticks into a mappable buffer.
- The `BufferMapAsync` callback fires under `DevicePoll`, so the core owns query creation, writes, resolution, and mapping while this extension owns non-blocking callback retirement.
- `SetLogCallback` binds a `PfnLogCallback` (a `readonly struct` over a `delegate* unmanaged[Cdecl]<LogLevel, byte*, void*, void>`), constructed from a managed `LogCallback` via `PfnLogCallback.From(callback)` or the implicit conversion. The native wgpu diagnostic stream (adapter selection, validation errors, device-lost) routes into the AppUi receipt sink as evidence rows, so a wgpu validation error is a counted `ViewportFault` on the telemetry spine, not a swallowed native print.
- `InstanceEnumerateAdapters` returns every physical adapter (not the single power-preference adapter the standard `RequestAdapter` callback yields), so the compositor-adapter-LUID/UUID match the `ICompositionGpuInterop` seam demands selects the exact adapter the Avalonia compositor renders on. A power-preference single adapter ignoring the compositor LUID is the cross-adapter copy penalty.

[GPU_DRIVEN_LAW]:

- `MultiDrawIndirect`/`MultiDrawIndexedIndirect` issue one indirect multi-draw over the visible meshlet set rather than N per-meshlet draws; the `*Count` variants read the live draw count from a GPU buffer, so a compute culling pass writes the surviving-cluster count and the draw consumes it with zero CPU round-trip — this is the GPU-driven culling the `GEOMETRY_VIRTUAL` cluster owns.
- `BindGroupEntryExtras`/`BindGroupLayoutEntryExtras` carry the bindless descriptor arrays (`Buffer**`/`Sampler**`/`TextureView**` plus `Count`) the meshlet bindless residency binds — one bind group references the whole resident texture/buffer table, indexed in-shader, so a per-meshlet rebind is the rejected form.
- `SetPushConstants` feeds per-draw constants (view/cluster offsets) declared by `PipelineLayoutExtras.PushConstantRanges`, avoiding a per-draw uniform-buffer rebind; `QueueSubmitForIndex` returns the `WrappedSubmissionIndex` `DevicePoll` waits on, closing the submit->poll->retire loop.
- `GenerateReport` snapshots per-registry allocation counts (`RegistryReport.NumAllocated`/`ElementSize`), so GPU-resource residency and leaks are a counted evidence row, not a guess — it stacks with the residency-budget rail as the device-side truth.

[LOCAL_ADMISSION]:

- The extension carries no native runtime of its own — it P/Invokes the `wgpu_native` binaries `Silk.NET.WebGPU.Native.WGPU` already restores, so admitting the extension adds the vendor function table at zero additional native-asset cost.
- `InstanceExtras` selects the wgpu backend through `Backends`, the D3D12 compiler through `Dx12Compiler.Dxc` and the `DxcPath`/`DxilPath` fields, and validation through `InstanceFlag.Validation` or `Debug` packed into `Flags`.
- `DeviceExtras` carries the device trace path, and `SurfaceConfigurationExtras` carries the present latency.
- Every extra rides the standard `InstanceDescriptor.NextInChain`, `DeviceDescriptor.NextInChain`, or `SurfaceConfiguration.NextInChain` pointer.

[RAIL_LAW]:

- Package: `Silk.NET.WebGPU.Extensions.WGPU`
- Owns: the wgpu-native vendor surface over the standard binding — non-blocking `DevicePoll`, native log routing through `PfnLogCallback`, full-adapter enumeration for the compositor-LUID match, indirect/count-buffer multi-draw and push constants for GPU-driven meshlet rendering, bindless descriptor arrays via the `Extras` chain, pipeline-statistics profiling, the `GenerateReport` residency snapshot, and the submission-index handshake.
- Accept: `new Wgpu(webgpu.Context)`/`TryGetExtension` second function-table view over the one core; `DevicePoll` per-frame non-blocking advance; native log via `PfnLogCallback.From` into the receipt sink; `InstanceEnumerateAdapters` LUID-matched select; the `Extras` native `next`-chain on the standard descriptors; count-buffer GPU-driven draw counts.
- Reject: a second wgpu binding beside the core `WebGPU`; `Wgpu.GetApi` (the core static, not an extension load); `SetLogCallback(LogCallback, …)` (the wrong argument type — it binds `PfnLogCallback`); a blocking fence busy-spin where `DevicePoll` advances; a power-preference single-adapter select ignoring the compositor LUID; N per-meshlet draws where one indirect multi-draw issues; per-meshlet bind where a bindless array binds once. `BufferDestroy`/`TextureDestroy` are not members of this extension — explicit resource release lives on the core `WebGPU` binding.

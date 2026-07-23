# [RASM_APPUI_API_SILK_WEBGPU_WGPU]

`Wgpu : NativeExtension<WebGPU>` mints the wgpu-native (`wgpu.h`) function table layered over the portable `Silk.NET.WebGPU` core (`webgpu.h`) — the native-only entrypoints a desktop render loop demands past the WebGPU standard. `new Wgpu(webgpu.Context)` or `webgpu.TryGetExtension(out Wgpu)` loads it against the live core as a second function-table view over the one loaded `wgpu_native` runtime, so a call site holds one `WebGPU` core and one `Wgpu` extension and never a second binding. It feeds the viewport rail.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Silk.NET.WebGPU.Extensions.WGPU`
- package: `Silk.NET.WebGPU.Extensions.WGPU` (MIT)
- assembly: `Silk.NET.WebGPU.Extensions.WGPU`
- namespace: `Silk.NET.WebGPU.Extensions.WGPU`
- asset: managed binding only — P/Invokes the `wgpu_native` binaries `Silk.NET.WebGPU.Native.WGPU` restores
- rail: viewport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: function-table root and its Span/managed overload mirror

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]                | [CAPABILITY]               |
| :-----: | :-------------- | :--------------------------- | :------------------------- |
|  [01]   | `Wgpu`          | `NativeExtension<WebGPU>`    | wgpu-native function table |
|  [02]   | `WgpuOverloads` | static `this Wgpu` extension | `Span<T>`/managed mirror   |

[PUBLIC_TYPE_SCOPE]: backend, log, chain-type, and compiler enum vocabularies

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                     |
| :-----: | :---------------------- | :------------ | :------------------------------- |
|  [01]   | `InstanceBackend`       | flags enum    | backend mask                     |
|  [02]   | `InstanceFlag`          | flags enum    | instance debug mask              |
|  [03]   | `LogLevel`              | enum          | log verbosity                    |
|  [04]   | `NativeSType`           | enum          | `next`-chain tag per Extras kind |
|  [05]   | `Dx12Compiler`          | enum          | D3D12 shader compiler            |
|  [06]   | `Gles3MinorVersion`     | enum          | GLES 3.x minor version           |
|  [07]   | `NativeFeature`         | enum          | extends core `FeatureName`       |
|  [08]   | `NativeQueryType`       | enum          | pipeline-statistics query kind   |
|  [09]   | `PipelineStatisticName` | enum          | reported-statistic selector      |

[InstanceBackend]: `All` `Vulkan` `GL` `Metal` `DX12` `DX11` `BrowserWebGpu` `Primary` `Secondary`
[InstanceFlag]: `Default` `Debug` `Validation` `DiscardHalLabels`
[LogLevel]: `Off` `Error` `Warn` `Info` `Debug` `Trace`
[Dx12Compiler]: `Undefined` `Fxc` `Dxc`

[PUBLIC_TYPE_SCOPE]: native log callback and its function-pointer wrapper

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]                   | [CAPABILITY]             |
| :-----: | :--------------- | :------------------------------ | :----------------------- |
|  [01]   | `LogCallback`    | `unsafe delegate`               | managed log sink         |
|  [02]   | `PfnLogCallback` | `readonly struct : IDisposable` | function-pointer wrapper |

`PfnLogCallback` wraps `delegate* unmanaged[Cdecl]<LogLevel, byte*, void*, void>`, constructs via `PfnLogCallback.From(LogCallback)` or implicit `nint`, and is the type `SetLogCallback` binds.

[PUBLIC_TYPE_SCOPE]: `next`-chain descriptor-extension structs threaded through the standard descriptor `NextInChain`

Every `*Extras` opens with a `Chain:ChainedStruct` header.

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]               |
| :-----: | :--------------------------- | :------------ | :------------------------- |
|  [01]   | `InstanceExtras`             | chain struct  | instance creation          |
|  [02]   | `DeviceExtras`               | chain struct  | trace capture              |
|  [03]   | `RequiredLimitsExtras`       | chain struct  | required native limits     |
|  [04]   | `SupportedLimitsExtras`      | chain struct  | supported native limits    |
|  [05]   | `SurfaceConfigurationExtras` | chain struct  | present latency            |
|  [06]   | `PipelineLayoutExtras`       | chain struct  | push-constant layout       |
|  [07]   | `PushConstantRange`          | value struct  | stage byte range           |
|  [08]   | `BindGroupEntryExtras`       | chain struct  | bindless descriptor arrays |
|  [09]   | `BindGroupLayoutEntryExtras` | chain struct  | array binding count        |
|  [10]   | `QuerySetDescriptorExtras`   | chain struct  | pipeline statistics        |
|  [11]   | `ShaderModuleGLSLDescriptor` | chain struct  | GLSL compilation           |
|  [12]   | `ShaderDefine`               | value struct  | GLSL compile definition    |

- `InstanceExtras`: `Backends:InstanceBackend` `Flags:uint` `Dx12ShaderCompiler:Dx12Compiler` `Gles3MinorVersion` `DxilPath:byte*` `DxcPath:byte*`
- `DeviceExtras`: `TracePath:byte*`
- `RequiredLimitsExtras` / `SupportedLimitsExtras`: `Limits:NativeLimits`
- `SurfaceConfigurationExtras`: `DesiredMaximumFrameLatency:Bool32`
- `PipelineLayoutExtras`: `PushConstantRanges:PushConstantRange*` `PushConstantRangeCount:nuint`
- `PushConstantRange`: `Stages:ShaderStage` `Start:uint` `End:uint`
- `BindGroupEntryExtras`: `Buffers:Buffer**` `Samplers:Sampler**` `TextureViews:TextureView**` and their `nuint` counts
- `BindGroupLayoutEntryExtras`: `Count:uint`
- `QuerySetDescriptorExtras`: `PipelineStatistics:PipelineStatisticName*` `PipelineStatisticCount:nuint`
- `ShaderModuleGLSLDescriptor`: `Stage:ShaderStage` `Code:byte*` `Defines:ShaderDefine*` `DefineCount:uint`
- `ShaderDefine`: `Name:byte*` `Value:byte*`

[PUBLIC_TYPE_SCOPE]: native value structs — submission index, enumeration options, and the allocation report tree

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]        |
| :-----: | :-------------------------------- | :------------ | :------------------ |
|  [01]   | `WrappedSubmissionIndex`          | value struct  | submission tracking |
|  [02]   | `InstanceEnumerateAdapterOptions` | value struct  | backend filtering   |
|  [03]   | `GlobalReport`                    | value struct  | memory snapshot     |
|  [04]   | `HubReport`                       | value struct  | per-backend report  |
|  [05]   | `RegistryReport`                  | value struct  | allocation census   |
|  [06]   | `NativeLimits`                    | value struct  | native limit values |

- `WrappedSubmissionIndex`: `Queue:Queue*` `SubmissionIndex:ulong` — `QueueSubmitForIndex` mints it, `DevicePoll` waits on it
- `InstanceEnumerateAdapterOptions`: `NextInChain:ChainedStruct*` `Backends:InstanceBackend`
- `GlobalReport`: `Surfaces:RegistryReport` `BackendType` and a `HubReport` per backend (`Vulkan` `Metal` `Dx12` `Gl`)
- `HubReport`: a `RegistryReport` per resource kind (`Adapters` `Devices` `Queues` `Buffers` `Textures` and eleven more)
- `RegistryReport`: `NumAllocated` `NumKeptFromUser` `NumReleasedFromUser` `NumError` `ElementSize` — all `nuint`
- `NativeLimits`: `MaxPushConstantSize:uint` `MaxNonSamplerBindings:uint`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: extension load, non-blocking poll, diagnostics, adapter enumeration, GPU-driven draw, and pipeline-statistics queries

Construction binds the core `WebGPU`; every other entrypoint is an instance method on the loaded `Wgpu` table, and each pass-encoder op takes its `RenderPassEncoder*`/`ComputePassEncoder*` receiver as its first argument.

| [INDEX] | [SURFACE]                                                                                    | [SHAPE]  | [CAPABILITY]               |
| :-----: | :------------------------------------------------------------------------------------------- | :------- | :------------------------- |
|  [01]   | `Wgpu(WebGPU)`                                                                               | ctor     | load over live core        |
|  [02]   | `TryGetExtension(out Wgpu)`                                                                  | factory  | lookup and load            |
|  [03]   | `DevicePoll(Device*, Bool32, WrappedSubmissionIndex*) -> Bool32`                             | instance | non-blocking queue advance |
|  [04]   | `SetLogCallback(PfnLogCallback, void*)`                                                      | instance | native log routing         |
|  [05]   | `SetLogCallback<T0>(PfnLogCallback, ref T0)`                                                 | instance | typed-userdata log route   |
|  [06]   | `SetLogLevel(LogLevel)`                                                                      | instance | verbosity floor            |
|  [07]   | `GetVersion() -> uint`                                                                       | instance | runtime version            |
|  [08]   | `GenerateReport(Instance*, ref GlobalReport)`                                                | instance | allocation snapshot        |
|  [09]   | `InstanceEnumerateAdapters(Instance*, InstanceEnumerateAdapterOptions*, Adapter**) -> nuint` | instance | all physical adapters      |
|  [10]   | `QueueSubmitForIndex(Queue*, nuint, CommandBuffer**) -> ulong`                               | instance | submit minting the index   |
|  [11]   | `RenderPassEncoderMultiDrawIndirect(Buffer*, ulong, uint)`                                   | instance | indirect multi-draw        |
|  [12]   | `RenderPassEncoderMultiDrawIndexedIndirect(Buffer*, ulong, uint)`                            | instance | indexed multi-draw         |
|  [13]   | `RenderPassEncoderMultiDrawIndirectCount(Buffer*, ulong, Buffer*, ulong, uint)`              | instance | count-buffer draw          |
|  [14]   | `RenderPassEncoderMultiDrawIndexedIndirectCount(Buffer*, ulong, Buffer*, ulong, uint)`       | instance | indexed count-buffer draw  |
|  [15]   | `RenderPassEncoderSetPushConstants(ShaderStage, uint, uint, void*)`                          | instance | push constants             |
|  [16]   | `RenderPassEncoderSetPushConstants<T0>(ShaderStage, uint, uint, ref readonly T0)`            | instance | typed push constants       |
|  [17]   | `RenderPassEncoderBeginPipelineStatisticsQuery(QuerySet*, uint)`                             | instance | render stats begin         |
|  [18]   | `RenderPassEncoderEndPipelineStatisticsQuery()`                                              | instance | render stats end           |
|  [19]   | `ComputePassEncoderBeginPipelineStatisticsQuery(QuerySet*, uint)`                            | instance | compute stats begin        |
|  [20]   | `ComputePassEncoderEndPipelineStatisticsQuery()`                                             | instance | compute stats end          |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Wgpu` loads through the `NativeExtension<WebGPU>` convention against the live core — `new Wgpu(webgpu.Context)` or `webgpu.TryGetExtension(out Wgpu)` yields a second function-table view over the one loaded `wgpu_native` runtime.
- `DevicePoll(device, wait:false, submissionIndex)` advances submission completion once per frame, retires command buffers, and runs mapped-buffer callbacks off the UI thread; `wait:true` drains at teardown.
- `SetLogCallback` binds a `PfnLogCallback` over the native diagnostic stream — adapter selection, validation errors, device-lost — so a wgpu validation error is a counted evidence row, not a swallowed native print.
- `MultiDrawIndirect`/`MultiDrawIndexedIndirect` issue one indirect multi-draw over the visible meshlet set; the `*Count` variants read the live draw count a GPU compute-culling pass wrote, so the surviving-cluster count drives the draw with zero CPU round-trip.
- `BindGroupEntryExtras`/`BindGroupLayoutEntryExtras` bind the bindless descriptor arrays (`Buffer**`/`Sampler**`/`TextureView**` with `Count`) once, indexed in-shader; `SetPushConstants` feeds per-draw view/cluster offsets declared by `PipelineLayoutExtras.PushConstantRanges` without a per-draw uniform rebind.
- `QueueSubmitForIndex` returns the `WrappedSubmissionIndex` `DevicePoll` waits on, closing the submit -> poll -> retire loop.

[STACKING]:
- `api-silk-webgpu`(`.api/api-silk-webgpu.md`): the core this extends over one loaded `wgpu_native` runtime — the core owns instance/adapter/device lifecycle, surface, pipeline, and timestamp query create/write/resolve/map, and this extension's `DevicePoll` retires the core's `BufferMapAsync` callback that `InstanceProcessEvents` otherwise pumps blocking.
- `api-avalonia-gpu-interop`(`.api/api-avalonia-gpu-interop.md`): `InstanceEnumerateAdapters` returns every physical adapter for the compositor-adapter `DeviceLuid`/`DeviceUuid` match, so the wgpu device renders on the exact adapter the Avalonia compositor composites; the power-preference single adapter is the cross-adapter copy penalty.
- Within-lib viewport owner: `SetLogCallback` routes the native diagnostic stream into the AppUi receipt sink as counted `ViewportFault` rows; `GenerateReport` feeds the residency-budget rail as device-side `RegistryReport` truth; the multi-draw, bindless-array, and push-constant ops drive the `GEOMETRY_VIRTUAL` meshlet cluster pass.

[LOCAL_ADMISSION]:
- `Wgpu` carries no native runtime of its own, P/Invoking the `wgpu_native` binaries `Silk.NET.WebGPU.Native.WGPU` already restores, so admission adds the vendor function table at zero native-asset cost.
- Every `*Extras` rides the owning standard descriptor's `NextInChain`: `InstanceExtras` selects backend, D3D12 compiler, and validation on `InstanceDescriptor`; `DeviceExtras` carries the trace path; `SurfaceConfigurationExtras` carries present latency.

[RAIL_LAW]:
- Package: `Silk.NET.WebGPU.Extensions.WGPU`
- Owns: the wgpu-native vendor surface over the standard binding — non-blocking `DevicePoll`, native log routing through `PfnLogCallback`, full-adapter enumeration, indirect/count-buffer multi-draw and push constants, bindless descriptor arrays via the `Extras` chain, pipeline-statistics profiling, the `GenerateReport` residency snapshot, and the submission-index handshake.
- Accept: `new Wgpu(webgpu.Context)`/`TryGetExtension` second function-table view; `DevicePoll` per-frame non-blocking advance; native log via `PfnLogCallback.From` into the receipt sink; `InstanceEnumerateAdapters` LUID-matched select; the `Extras` native `next`-chain on the standard descriptors; count-buffer GPU-driven draw counts.
- Reject: a second wgpu binding beside the core; `Wgpu.GetApi` (the core static, not an extension load); `SetLogCallback` bound to a bare `LogCallback` in place of `PfnLogCallback`; a blocking fence busy-spin where `DevicePoll` advances; a power-preference single-adapter select ignoring the compositor LUID; N per-meshlet draws where one indirect multi-draw issues; a per-meshlet rebind where a bindless array binds once. `BufferDestroy`/`TextureDestroy` release lives on the core binding.

# [RASM_APPUI_API_SILK_WEBGPU_WGPU]

`Silk.NET.WebGPU.Extensions.WGPU` is the wgpu-native (`wgpu.h`) vendor-extension binding layered over the canonical `Silk.NET.WebGPU` core (`webgpu.h`): the upstream standard surface omits the non-blocking submission-completion poll, the native log/error callbacks, full-adapter enumeration, GPU-driven indirect multi-draw, push constants, and the bindless descriptor-array `next`-chain a desktop render loop demands, and `wgpu_native` (the runtime `Silk.NET.WebGPU.Native.WGPU` already carries) adds them. `Wgpu : NativeExtension<WebGPU>` is the generated function-table root for those native-only entrypoints — `DevicePoll` advances the device queue without a blocking fence, `SetLogCallback`/`SetLogLevel` route the native diagnostic stream into the receipt sink, `InstanceEnumerateAdapters` returns every physical adapter for the compositor-LUID match, `RenderPassEncoderMultiDraw*`/`SetPushConstants` drive the meshlet cluster pass, and `QueueSubmitForIndex` mints the `WrappedSubmissionIndex` `DevicePoll` waits on. The extension is **loaded against the existing core**, not separately constructed: `new Wgpu(webgpu.Context)` (or `webgpu.TryGetExtension(out Wgpu wgpu)`) yields a second function-table view over the one loaded `wgpu_native` runtime, so a call site holds one `WebGPU` core and one `Wgpu` extension and never a second binding.

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

| [INDEX] | [SYMBOL]        | [TYPE_FAMILY]     | [RAIL]                                                  |
| :-----: | :-------------- | :---------------- | :----------------------------------------------------- |
|  [01]   | `Wgpu`          | `NativeExtension<WebGPU>` | wgpu-native function table; ctor takes the core context |
|  [02]   | `WgpuOverloads` | static `this Wgpu` extension | `Span<T>`/managed-argument overload mirrors        |

[ENUMS]: backend/log/chain-type/compiler vocabularies
- rail: viewport

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY]  | [RAIL]                                                                 |
| :-----: | :------------------ | :------------- | :-------------------------------------------------------------------- |
|  [01]   | `InstanceBackend`   | flags enum     | `All`/`Vulkan`/`GL`/`Metal`/`DX12`/`DX11`/`BrowserWebGpu`/`Primary`/`Secondary` |
|  [02]   | `InstanceFlag`      | flags enum     | `Default`/`Debug`/`Validation`/`DiscardHalLabels` instance debug mask  |
|  [03]   | `LogLevel`          | enum           | `Off`/`Error`/`Warn`/`Info`/`Debug`/`Trace`                           |
|  [04]   | `NativeSType`       | enum           | wgpu-native `next`-chain struct-type tags (`STypeInstanceExtras`, …)  |
|  [05]   | `Dx12Compiler`      | enum           | `Undefined`/`Fxc`/`Dxc` — D3D12 shader compiler selection             |
|  [06]   | `Gles3MinorVersion` | enum           | GLES 3.x minor-version pin for the GL backend                         |
|  [07]   | `NativeFeature`     | enum           | wgpu-native feature flags beyond the standard `FeatureName`           |
|  [08]   | `NativeQueryType`   | enum           | wgpu-native query-set kinds (pipeline statistics)                     |
|  [09]   | `PipelineStatisticName` | enum       | the per-statistic selector for pipeline-statistics queries            |

[CALLBACKS]: native log delegate and its function-pointer wrapper
- rail: viewport

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]            | [RAIL]                                                            |
| :-----: | :---------------- | :----------------------- | :--------------------------------------------------------------- |
|  [01]   | `LogCallback`     | `unsafe delegate`        | `void(LogLevel, byte* message, void* userdata)` managed sink     |
|  [02]   | `PfnLogCallback`  | `readonly struct : IDisposable` | function-pointer wrapper; `From(LogCallback)` / implicit `nint`; **the type `SetLogCallback` actually binds** |

[NEXT_CHAIN_STRUCTS]: `ChainedStruct`-headed descriptor extensions threaded through the standard descriptor `NextInChain`
- rail: viewport

| [INDEX] | [SYMBOL]                       | [FIELDS]                                                            | [RAIL]                                            |
| :-----: | :----------------------------- | :----------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `InstanceExtras`               | `Chain`, `Backends:InstanceBackend`, `Flags:uint`, `DxilPath`, `DxcPath` | backend mask + DXC/DXIL compiler paths at instance create |
|  [02]   | `DeviceExtras`                 | `Chain`, `TracePath:byte*`                                          | wgpu device trace-capture path at device create   |
|  [03]   | `RequiredLimitsExtras`         | `Chain`, `Limits:NativeLimits`                                      | wgpu-native limit extensions on `RequiredLimits`  |
|  [04]   | `SupportedLimitsExtras`        | `Chain`, `Limits:NativeLimits`                                      | wgpu-native limits reported on `SupportedLimits`  |
|  [05]   | `SurfaceConfigurationExtras`   | `Chain`, `DesiredMaximumFrameLatency:Bool32`                       | swapchain present-latency control                 |
|  [06]   | `PipelineLayoutExtras`         | `Chain`, `PushConstantRangeCount`, `PushConstantRanges:PushConstantRange*` | declares push-constant ranges on the layout |
|  [07]   | `PushConstantRange`            | `Stages:ShaderStage`, `Start:uint`, `End:uint`                     | one push-constant byte range for a stage mask     |
|  [08]   | `BindGroupEntryExtras`         | `Chain`, `Buffers:Buffer**`/`Samplers:Sampler**`/`TextureViews:TextureView**` + counts | **bindless descriptor arrays** in a bind group |
|  [09]   | `BindGroupLayoutEntryExtras`   | `Chain`, `Count:uint`                                              | bindless array binding-count on the layout entry  |
|  [10]   | `QuerySetDescriptorExtras`     | `Chain`, pipeline-statistic selectors                              | pipeline-statistics query-set creation            |
|  [11]   | `ShaderModuleGLSLDescriptor`   | `Chain`, GLSL source + `ShaderDefine` defines                     | compile a GLSL shader module (GL backend)         |
|  [12]   | `ShaderDefine`                 | name/value pair                                                    | a `#define` for the GLSL descriptor               |

[NATIVE_VALUE_STRUCTS]: submission index, enumeration options, and the memory/registry report tree
- rail: viewport

| [INDEX] | [SYMBOL]                          | [FIELDS]                                                  | [RAIL]                                          |
| :-----: | :-------------------------------- | :------------------------------------------------------- | :--------------------------------------------- |
|  [01]   | `WrappedSubmissionIndex`          | `Queue:Queue*`, `SubmissionIndex:ulong`                  | the index `QueueSubmitForIndex` mints / `DevicePoll` waits on |
|  [02]   | `InstanceEnumerateAdapterOptions` | `NextInChain:ChainedStruct*`, `Backends:InstanceBackend` | backend filter for `InstanceEnumerateAdapters` |
|  [03]   | `GlobalReport`                    | per-backend `HubReport` + surface/instance counts        | top of the `GenerateReport` memory snapshot    |
|  [04]   | `HubReport`                       | per-resource-kind `RegistryReport` set                   | per-backend hub of the report tree             |
|  [05]   | `RegistryReport`                  | `NumAllocated`/`NumKeptFromUser`/`NumReleasedFromUser`/`NumError`/`ElementSize` | one resource registry's allocation census |
|  [06]   | `NativeLimits`                    | `MaxPushConstantSize`, `MaxNonSamplerBindings`           | wgpu-native limit values                       |

## [03]-[ENTRYPOINTS]

[LIFECYCLE_AND_DIAGNOSTICS]: load, queue poll, native log, version, and the memory report
- rail: viewport

| [INDEX] | [SURFACE]                                                                       | [SURFACE_ROOT] | [RAIL]                                          |
| :-----: | :------------------------------------------------------------------------------ | :------------- | :---------------------------------------------- |
|  [01]   | `new Wgpu(webgpu.Context)` / `webgpu.TryGetExtension(out Wgpu)`                  | core `WebGPU`  | load the extension over the one core instance   |
|  [02]   | `DevicePoll(Device*, Bool32 wait, ref/WrappedSubmissionIndex*) -> Bool32`        | `Wgpu`         | non-blocking (`wait:false`) queue-completion advance |
|  [03]   | `SetLogCallback(PfnLogCallback callback, void* userdata)`                        | `Wgpu`         | route native log into the receipt sink          |
|  [04]   | `SetLogCallback<T0>(PfnLogCallback, ref T0 userdata) where T0:unmanaged`         | `Wgpu`         | typed-userdata log-callback mirror              |
|  [05]   | `SetLogLevel(LogLevel level)`                                                    | `Wgpu`         | native log verbosity floor                      |
|  [06]   | `GetVersion() -> uint`                                                           | `Wgpu`         | wgpu-native runtime version                     |
|  [07]   | `GenerateReport(Instance*, ref/GlobalReport)`                                    | `Wgpu`         | per-registry allocation snapshot — leak/residency diagnostics |

[ADAPTER_AND_SUBMISSION]: full-adapter enumeration and the submission-index handshake
- rail: viewport

| [INDEX] | [SURFACE]                                                                                     | [SURFACE_ROOT] | [RAIL]                                       |
| :-----: | :-------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `InstanceEnumerateAdapters(Instance*, ref/InstanceEnumerateAdapterOptions*, ref/Adapter*) -> nuint` | `Wgpu`   | enumerate every physical adapter (LUID match)|
|  [02]   | `QueueSubmitForIndex(Queue*, nuint count, ref/CommandBuffer**) -> ulong`                       | `Wgpu`         | submit + return the submission index          |

[GPU_DRIVEN_RENDERING]: indirect multi-draw, count-buffer culling, and push constants
- rail: viewport

| [INDEX] | [SURFACE]                                                                                              | [SURFACE_ROOT] | [RAIL]                                          |
| :-----: | :----------------------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `RenderPassEncoderMultiDrawIndirect(RenderPassEncoder*, Buffer*, ulong offset, uint count)`            | `Wgpu`         | one indirect multi-draw over the visible set    |
|  [02]   | `RenderPassEncoderMultiDrawIndexedIndirect(RenderPassEncoder*, Buffer*, ulong offset, uint count)`     | `Wgpu`         | indexed indirect multi-draw                     |
|  [03]   | `RenderPassEncoderMultiDrawIndirectCount(encoder, buffer, offset, Buffer* countBuffer, ulong countOffset, uint maxCount)` | `Wgpu` | GPU-culled draw count read from a buffer    |
|  [04]   | `RenderPassEncoderMultiDrawIndexedIndirectCount(encoder, buffer, offset, countBuffer, countOffset, maxCount)` | `Wgpu`  | indexed GPU-culled draw-count multi-draw      |
|  [05]   | `RenderPassEncoderSetPushConstants(RenderPassEncoder*, ShaderStage stages, uint offset, uint sizeBytes, void* data)` | `Wgpu` | set push-constant bytes for a stage mask   |
|  [06]   | `RenderPassEncoderSetPushConstants<T0>(encoder, stages, offset, sizeBytes, ref readonly T0) where T0:unmanaged` | `Wgpu` | typed push-constant mirror                    |

[PIPELINE_STATISTICS]: query-driven pass profiling
- rail: viewport

| [INDEX] | [SURFACE]                                                                                  | [SURFACE_ROOT] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `RenderPassEncoderBeginPipelineStatisticsQuery(RenderPassEncoder*, QuerySet*, uint index)` | `Wgpu`         | begin a render-pass statistics query         |
|  [02]   | `RenderPassEncoderEndPipelineStatisticsQuery(RenderPassEncoder*)`                           | `Wgpu`         | end the render-pass statistics query         |
|  [03]   | `ComputePassEncoderBeginPipelineStatisticsQuery(ComputePassEncoder*, QuerySet*, uint index)`| `Wgpu`        | begin a compute-pass statistics query        |
|  [04]   | `ComputePassEncoderEndPipelineStatisticsQuery(ComputePassEncoder*)`                         | `Wgpu`         | end the compute-pass statistics query        |

## [04]-[IMPLEMENTATION_LAW]

[WGPU_EXTENSION_TOPOLOGY]:
- The extension loads through the Silk.NET `NativeExtension<WebGPU>` convention against the live core: `new Wgpu(webgpu.Context)` (or `webgpu.TryGetExtension(out Wgpu wgpu)`) — never `Wgpu.GetApi(...)`, which is the **core** `WebGPU.GetApi()` static, not an extension entrypoint. The result is a second function-table view over the one loaded `wgpu_native` runtime; a call site holds one `WebGPU` core and one `Wgpu` extension.
- `DevicePoll(device, wait:false, submissionIndex)` is the non-blocking submission-completion advance the standard `webgpu.h` lacks: the render loop polls the queue once per frame to retire completed command buffers and run mapped-buffer callbacks without blocking the UI thread on a fence. `wait:true` is the blocking drain the teardown capsule uses; a busy-spin on a standard fence is the rejected form. The mapped-buffer callback `DevicePoll` retires is the core timestamp-readback rail's: core `WebGPU` (`api-silk-webgpu.md`) records `CommandEncoderWriteTimestamp`/`RenderPassTimestampWrites` into a `QueryType.Timestamp` `QuerySet`, `CommandEncoderResolveQuerySet` copies the ticks into a mappable buffer, and the `BufferMapAsync` map callback fires here under `DevicePoll` — so GPU timing is one rail read across both catalogs (core owns query-set create/write/resolve/map, this extension owns the non-blocking poll that retires it), with the standard `InstanceProcessEvents` the blocking-pump alternative.
- `SetLogCallback` binds a `PfnLogCallback` (a `readonly struct` over a `delegate* unmanaged[Cdecl]<LogLevel, byte*, void*, void>`), constructed from a managed `LogCallback` via `PfnLogCallback.From(callback)` or the implicit conversion. The native wgpu diagnostic stream (adapter selection, validation errors, device-lost) routes into the AppUi receipt sink as evidence rows, so a wgpu validation error is a counted `ViewportFault` on the telemetry spine, not a swallowed native print.
- `InstanceEnumerateAdapters` returns every physical adapter (not the single power-preference adapter the standard `RequestAdapter` callback yields), so the compositor-adapter-LUID/UUID match the `ICompositionGpuInterop` seam demands selects the exact adapter the Avalonia compositor renders on. A power-preference single adapter ignoring the compositor LUID is the cross-adapter copy penalty.

[GPU_DRIVEN_LAW]:
- `MultiDrawIndirect`/`MultiDrawIndexedIndirect` issue one indirect multi-draw over the visible meshlet set rather than N per-meshlet draws; the `*Count` variants read the live draw count from a GPU buffer, so a compute culling pass writes the surviving-cluster count and the draw consumes it with zero CPU round-trip — this is the GPU-driven culling the `GEOMETRY_VIRTUAL` cluster owns.
- `BindGroupEntryExtras`/`BindGroupLayoutEntryExtras` carry the bindless descriptor arrays (`Buffer**`/`Sampler**`/`TextureView**` plus `Count`) the meshlet bindless residency binds — one bind group references the whole resident texture/buffer table, indexed in-shader, so a per-meshlet rebind is the rejected form.
- `SetPushConstants` feeds per-draw constants (view/cluster offsets) declared by `PipelineLayoutExtras.PushConstantRanges`, avoiding a per-draw uniform-buffer rebind; `QueueSubmitForIndex` returns the `WrappedSubmissionIndex` `DevicePoll` waits on, closing the submit->poll->retire loop.
- `GenerateReport` snapshots per-registry allocation counts (`RegistryReport.NumAllocated`/`ElementSize`), so GPU-resource residency and leaks are a counted evidence row, not a guess — it stacks with the residency-budget rail as the device-side truth.

[LOCAL_ADMISSION]:
- The extension carries no native runtime of its own — it P/Invokes the `wgpu_native` binaries `Silk.NET.WebGPU.Native.WGPU` already restores, so admitting the extension adds the vendor function table at zero additional native-asset cost.
- The `Extras` native `next`-chain structs select the wgpu backend (`InstanceExtras.Backends = InstanceBackend.Vulkan`/`Metal`/`DX12`), the D3D12 compiler (`Dx12Compiler.Dxc` via `DxcPath`/`DxilPath`), the validation level (`InstanceFlag.Validation`/`Debug` packed into `Flags`), the device trace path, and the present latency — all threaded through the standard `InstanceDescriptor.NextInChain`/`DeviceDescriptor.NextInChain`/`SurfaceConfiguration.NextInChain` pointers. A parallel non-standard create path is the rejected form; the extras ride the standard descriptor `next` chain.

[RAIL_LAW]:
- Package: `Silk.NET.WebGPU.Extensions.WGPU`
- Owns: the wgpu-native vendor surface over the standard binding — non-blocking `DevicePoll`, native log routing through `PfnLogCallback`, full-adapter enumeration for the compositor-LUID match, indirect/count-buffer multi-draw and push constants for GPU-driven meshlet rendering, bindless descriptor arrays via the `Extras` chain, pipeline-statistics profiling, the `GenerateReport` residency snapshot, and the submission-index handshake.
- Accept: `new Wgpu(webgpu.Context)`/`TryGetExtension` second function-table view over the one core; `DevicePoll` per-frame non-blocking advance; native log via `PfnLogCallback.From` into the receipt sink; `InstanceEnumerateAdapters` LUID-matched select; the `Extras` native `next`-chain on the standard descriptors; count-buffer GPU-driven draw counts.
- Reject: a second wgpu binding beside the core `WebGPU`; `Wgpu.GetApi` (the core static, not an extension load); `SetLogCallback(LogCallback, …)` (the wrong argument type — it binds `PfnLogCallback`); a blocking fence busy-spin where `DevicePoll` advances; a power-preference single-adapter select ignoring the compositor LUID; N per-meshlet draws where one indirect multi-draw issues; per-meshlet bind where a bindless array binds once. `BufferDestroy`/`TextureDestroy` are not members of this extension — explicit resource release lives on the core `WebGPU` binding.

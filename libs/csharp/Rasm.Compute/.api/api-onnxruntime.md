# [RASM_COMPUTE_API_ONNXRUNTIME]

`Microsoft.ML.OnnxRuntime` supplies model session execution, native runtime assets, tensor value binding, run options, metadata inspection, custom operators, EP-context model compilation, autoEP hardware-device discovery, and execution-provider selection for Compute model rails. `Model/*` owners compose the managed surface as one rail: `Boot` folds `OrtThreadingOptions` into `EnvironmentCreationOptions`; `Open` folds `SessionOptions` config-entry keys and `OrtModelCompilationOptions` EP-context compilation into the resident-session map; `RunOps` folds `OrtValue` carriers and `OrtIoBinding` loops under a `RunOptions.Terminate` deadline latch into a `Fin` projection; the autoEP `OrtEnv.GetEpDevices`/`GetModelCompatibilityForEpDevices`/`CreateSharedAllocator` trio drives provider ranking and shared-arena leases. Every native handle is `SafeHandle`/`IDisposable` and releases deterministically.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime`
- package: `Microsoft.ML.OnnxRuntime` (meta-package: native runtimes + props/targets + headers; declares dependency `Microsoft.ML.OnnxRuntime.Managed`)
- assembly: `Microsoft.ML.OnnxRuntime.Managed` — multi-targets `net8.0` / `netstandard2.0` / `net9.0-{android35,ios18,maccatalyst18}`; a `net10.0` non-mobile consumer binds the `lib/net8.0/Microsoft.ML.OnnxRuntime.dll` asset (no plain `net9.0` lib ships, and the `net9.0-*` assets are mobile-platform TFMs), so the surface below is decompile-verified against the consumer-bound `net8.0` assembly
- license: MIT (`LICENSE` shipped in package root)
- namespace: `Microsoft.ML.OnnxRuntime` (+ nested `Microsoft.ML.OnnxRuntime.CompileApi`, `Microsoft.ML.OnnxRuntime.Tensors` for `DenseTensor<T>`/`Tensor<T>`)
- asset: managed runtime library + native runtime DLLs (the base package ships only `runtimes/win-{x64,arm64}/native/onnxruntime.dll` + `onnxruntime_providers_shared.dll`; macOS/Linux base runtimes resolve through the platform-specific feed asset the build copies, and accelerated providers ride the sibling `.Gpu`/`.DirectML` packages)
- rail: model
- training surface: `CheckpointState`, `TrainingSession`, `TrainingUtils`, `OrtTrainingApi` are present but REJECTED by `[RAIL_LAW]` — the Compute rail accepts inference only and the on-device-training surface never enters a design owner

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: session and run contracts
- rail: model

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]       | [CAPABILITY]             |
| :-----: | :------------------------- | :------------------- | :----------------------- |
|  [01]   | `InferenceSession`         | session root         | executes model runs      |
|  [02]   | `SessionOptions`           | session policy       | configures session       |
|  [03]   | `RunOptions`               | run policy           | configures inference run |
|  [04]   | `OrtEnv`                   | runtime environment  | owns runtime scope       |
|  [05]   | `OrtMemoryInfo`            | memory descriptor    | describes tensor memory  |
|  [06]   | `OrtValue`                 | native value         | carries model values     |
|  [07]   | `NamedOnnxValue`           | named value          | binds named input        |
|  [08]   | `DisposableNamedOnnxValue` | named output         | owns named output        |
|  [09]   | `TensorElementType`        | element classifier   | classifies tensor data   |
|  [10]   | `OrtAllocatorType`         | allocator classifier | classifies allocators    |

[PUBLIC_TYPE_SCOPE]: metadata and tensor contracts
- rail: model

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]   | [MEMBERS] |
| :-----: | :-------------------------- | :--------------- | :-------- |
|  [01]   | `ModelMetadata`             | model metadata   | `[01]`    |
|  [02]   | `NodeMetadata`              | node metadata    | `[02]`    |
|  [03]   | `OrtTensorTypeAndShapeInfo` | tensor metadata  | `[03]`    |
|  [04]   | `DenseTensor<T>`            | tensor value     | `[04]`    |
|  [05]   | `OrtIoBinding`              | binding root     | `[05]`    |
|  [06]   | `PrePackedWeightsContainer` | weights cache    | `[06]`    |
|  [07]   | `FixedBufferOnnxValue`      | buffer value     | `[07]`    |
|  [08]   | `OrtLoraAdapter`            | adapter handle   | `[08]`    |
|  [09]   | `OrtMemoryAllocation`       | device buffer    | `[09]`    |
|  [10]   | `OrtAllocatorType`          | allocator enum   | `[10]`    |
|  [11]   | `OnnxValueType`             | value-kind enum  | `[11]`    |
|  [12]   | `IReadOnlyOrtValue`         | read view iface  | `[12]`    |
|  [13]   | `OrtTypeInfo`               | type descriptor  | `[13]`    |
|  [14]   | `SessionOptionsContainer`   | config registry  | `[14]`    |
|  [15]   | `SequenceMetadata`          | recursive schema | `[15]`    |
|  [16]   | `OptionalMetadata`          | recursive schema | `[16]`    |
|  [17]   | `MapMetadata`               | recursive schema | `[17]`    |

- [01]: `long Version`, `string Description`, `Domain`, `GraphName`, `GraphDescription`, `ProducerName`, `Dictionary<string,string> CustomMetadataMap` — the map feeds model-identity fingerprinting.
- [02]: `ElementDataType` (`TensorElementType`), `int[] Dimensions`, `string[] SymbolicDimensions`, `OnnxValueType`, `bool IsTensor`/`IsString`, `Type ElementType`, `AsMapMetadata()`/`AsSequenceMetadata()`/`AsOptionalMetadata()`.
- [03]: `ElementCount`/element type/shape; sizes output buffers, never re-multiplied dims.
- [04]: `Microsoft.ML.OnnxRuntime.Tensors` dense carrier; `ArrayTensorExtensions`/`ShapeUtils` helpers.
- [05]: binds inputs and outputs (see IO-binding entrypoints).
- [06]: `SafeHandle`; shares prepacked weights across sessions of the same model (`InferenceSession` ctor 3rd arg).
- [07]: binds fixed managed memory — superseded by `OrtValue` in the design (`OrtValue`-only law).
- [08]: `static Create(string adapterPath, OrtAllocator)` / `Create(byte[] bytes, OrtAllocator)`; `SafeHandle` — both overloads REQUIRE an `OrtAllocator`.
- [09]: `SafeHandle`; owns a device allocation from `OrtAllocator.Allocate(uint)` and binds via the device `BindInput/BindOutput` overload.
- [10]: `DeviceAllocator`, `ArenaAllocator`.
- [11]: `ONNX_TYPE_TENSOR`/`_SEQUENCE`/`_MAP`/`_OPTIONAL`/`_SPARSETENSOR` — the kind `OrtValue.OnnxType` and `NodeMetadata.OnnxValueType` report.
- [12]: read-only `OrtValue` projection consumed by `GetInitializerLocationDelegate`.
- [13]: `OrtValue.GetTypeInfo()` — tensor/sequence/map/optional shape introspection on a live value.
- [14]: static named-configuration registry: `Register(Action<SessionOptions>)` / `Register(name, handler)`, `Create(name)`, `ApplyConfiguration(name)`, `Reset()` — reusable session-config profiles.
- [15]: `NodeMetadata ElementMeta`; returned by `NodeMetadata.AsSequenceMetadata()`.
- [16]: `NodeMetadata ElementMeta`; returned by `NodeMetadata.AsOptionalMetadata()`.
- [17]: `TensorElementType KeyDataType`, `NodeMetadata ValueMetadata`; returned by `NodeMetadata.AsMapMetadata()`.

[PUBLIC_TYPE_SCOPE]: environment, threading, and provider-policy contracts
- rail: model

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]        | [MEMBERS] |
| :-----: | :---------------------------------- | :-------------------- | :-------- |
|  [01]   | `EnvironmentCreationOptions`        | boot options struct   | `[01]`    |
|  [02]   | `OrtThreadingOptions`               | global thread pool    | `[02]`    |
|  [03]   | `ExecutionProviderDevicePolicy`     | device-policy enum    | `[03]`    |
|  [04]   | `GraphOptimizationLevel`            | optimization enum     | `[04]`    |
|  [05]   | `OrtLoggingLevel`                   | logging enum          | `[05]`    |
|  [06]   | `OrtAllocator`                      | allocator handle      | `[06]`    |
|  [07]   | `OrtArenaCfg`                       | arena-config handle   | `[07]`    |
|  [08]   | `OrtExternalAllocation`             | external-buffer value | `[08]`    |
|  [09]   | `IDisposableReadOnlyCollection<T>`  | result collection     | `[09]`    |
|  [10]   | `BFloat16` / `Float16`              | readonly structs      | `[10]`    |
|  [11]   | `OrtEpDevice`                       | EP device descriptor  | `[11]`    |
|  [12]   | `OrtHardwareDevice`                 | hardware descriptor   | `[12]`    |
|  [13]   | `OrtHardwareDeviceType`             | device-type enum      | `[13]`    |
|  [14]   | `OrtMemoryInfoDeviceType`           | mem-device-type enum  | `[14]`    |
|  [15]   | `OrtKeyValuePairs`                  | kv-pairs handle       | `[15]`    |
|  [16]   | `OrtDeviceMemoryType`               | device-memory enum    | `[16]`    |
|  [17]   | `OrtMemType`                        | mem-class enum        | `[17]`    |
|  [18]   | `OrtSyncStream`                     | sync-stream handle    | `[18]`    |
|  [19]   | `OrtCompiledModelCompatibility`     | compat-verdict ENUM   | `[19]`    |
|  [20]   | `OrtDeviceEpIncompatibilityDetails` | incompat diagnostic   | `[20]`    |
|  [21]   | `OrtDeviceEpIncompatibilityReason`  | incompat reason flags | `[21]`    |
|  [22]   | `CoreMLFlags`                       | CoreML flags enum     | `[22]`    |
|  [23]   | `NnapiFlags`                        | NNAPI flags enum      | `[23]`    |
|  [24]   | `OrtCUDAProviderOptions`            | CUDA options handle   | `[24]`    |
|  [25]   | `OrtTensorRTProviderOptions`        | TensorRT options      | `[25]`    |

- [01]: `string logId`, `OrtLoggingLevel? logLevel` (nullable), `OrtThreadingOptions threadOptions`, `nint? loggingParam`, `DOrtLoggingFunction loggingFunction` — passed `ref` to `OrtEnv.CreateInstanceWithOptions`.
- [02]: `int GlobalIntraOpNumThreads`, `int GlobalInterOpNumThreads`, `bool GlobalSpinControl` (full read/write props), `SetGlobalDenormalAsZero()`; `SafeHandle`, parameterless ctor.
- [03]: `DEFAULT`, `PREFER_CPU`, `PREFER_NPU`, `PREFER_GPU`, `MAX_PERFORMANCE`, `MAX_EFFICIENCY`, `MIN_OVERALL_POWER`.
- [04]: `ORT_DISABLE_ALL`=0, `ORT_ENABLE_BASIC`=1, `ORT_ENABLE_EXTENDED`=2, `ORT_ENABLE_LAYOUT`=3, `ORT_ENABLE_ALL`=99.
- [05]: boot log severity.
- [06]: `static DefaultInstance`, `OrtMemoryInfo Info`, `OrtMemoryAllocation Allocate(uint size)`, ctor `(InferenceSession, OrtMemoryInfo)`; `SafeHandle`.
- [07]: ctor `(uint maxMemory, int arenaExtendStrategy, int initialChunkSizeBytes, int maxDeadBytesPerChunk)` — tunes the BFC arena for `OrtEnv.CreateAndRegisterAllocator`.
- [08]: wraps a caller-owned device buffer for `OrtIoBinding.BindInput/BindOutput(string, OrtExternalAllocation)`.
- [09]: `IReadOnlyList<T>`+`IDisposable` native result set returned by `Run`/`GetOutputValues`.
- [10]: CLR carriers for the bfloat16 / float16 element types; `IComparable`/`IEquatable`.
- [11]: `EpName`, `EpVendor`, `EpMetadata`, `EpOptions` (all `OrtKeyValuePairs`), `HardwareDevice`, `GetMemoryInfo(OrtDeviceMemoryType)`, `CreateSyncStream(IReadOnlyDictionary<string,string>)`.
- [12]: `Type` (`OrtHardwareDeviceType`), `VendorId` (uint), `Vendor` (string), `DeviceId` (uint), `Metadata` (`OrtKeyValuePairs`).
- [13]: `CPU`, `GPU`, `NPU`.
- [14]: `CPU`, `GPU`, `FPGA`, `NPU` — the device-type axis of the extended `OrtMemoryInfo` ctor.
- [15]: `Entries` (`IReadOnlyDictionary<string,string>`), `Add`, `Remove`, `Refresh`; `SafeHandle`, ctor `()` / `(IReadOnlyDictionary<string,string>)`.
- [16]: `DEFAULT` (0), `HOST_ACCESSIBLE` (5).
- [17]: `CpuInput` (-2), `CpuOutput`/`Cpu` (-1), `Default` (0).
- [18]: `nint GetHandle()` — ties device stream lifetime to `OrtEpDevice.CreateSyncStream`.
- [19]: `EP_NOT_APPLICABLE`, `EP_SUPPORTED_OPTIMAL`, `EP_SUPPORTED_PREFER_RECOMPILATION`, `EP_UNSUPPORTED` — the warm-start branch reads THIS enum, never a `"Incompatible"` substring.
- [20]: `OrtDeviceEpIncompatibilityReason ReasonsBitmask`, `string Notes`, `int ErrorCode`; `IDisposable` — paired with `OrtEnv.GetHardwareDeviceEpIncompatibilityDetails`.
- [21]: `[Flags] uint`: `None`=0, `DriverIncompatible`=1, `DeviceIncompatible`=2, `MissingDependency`=4, `Unknown`=0x80000000.
- [22]: `[Flags] uint`: `COREML_FLAG_USE_NONE`=0, `_USE_CPU_ONLY`=1, `_ENABLE_ON_SUBGRAPH`=2, `_ONLY_ENABLE_DEVICE_WITH_ANE`=4, `_ONLY_ALLOW_STATIC_INPUT_SHAPES`=8, `_CREATE_MLPROGRAM`=0x10, `_USE_CPU_AND_GPU`=0x20.
- [23]: `NNAPI_FLAG_USE_NONE`=0, `_USE_FP16`=1, `_USE_NCHW`=2, `_CPU_DISABLED`=4, `_CPU_ONLY`=8.
- [24]: `UpdateOptions(Dictionary<string,string>)`, `GetOptions()` string; `static ProviderOptionsValueHelper.StringToDict(s, dict)` parses a serialized options string.
- [25]: `UpdateOptions(Dictionary<string,string>)`, `GetOptions()`, `GetDeviceId()`.

[PUBLIC_TYPE_SCOPE]: ROCm, execution-mode, and provider-options contracts
- rail: model

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]         | [CAPABILITY]                                                        |
| :-----: | :--------------------------- | :--------------------- | :------------------------------------------------------------------ |
|  [01]   | `OrtROCMProviderOptions`     | ROCm options handle    | `UpdateOptions(Dictionary<string,string>)`                          |
|  [02]   | `ExecutionMode`              | execution-mode enum    | `ORT_SEQUENTIAL`, `ORT_PARALLEL`                                    |
|  [03]   | `OrtModelCompilationOptions` | compile options handle | drives the EP-context model compile pipeline                        |
|  [04]   | `OrtCompileApiFlags`         | compile flags enum     | `NONE`, `ERROR_IF_NO_NODES_COMPILED`, `ERROR_IF_OUTPUT_FILE_EXISTS` |

[PUBLIC_TYPE_SCOPE]: package assets
- rail: model

| [INDEX] | [SYMBOL]                                                           | [PACKAGE_ROLE] | [CAPABILITY]                                     |
| :-----: | :----------------------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `build/native/Microsoft.ML.OnnxRuntime.props`                      | MSBuild import | declares native copy                             |
|  [02]   | `build/native/Microsoft.ML.OnnxRuntime.targets`                    | MSBuild import | copies native assets                             |
|  [03]   | `runtimes/win-x64/native/onnxruntime.dll`                          | native asset   | base runtime (win-x64), shipped inline           |
|  [04]   | `runtimes/win-arm64/native/onnxruntime.dll`                        | native asset   | base runtime (win-arm64), shipped inline         |
|  [05]   | `runtimes/win-{x64,arm64}/native/onnxruntime_providers_shared.dll` | native asset   | shared-provider bridge for win append calls      |
|  [06]   | `libonnxruntime.dylib` / `libonnxruntime.so`                       | native asset   | macOS/Linux base runtime; feed asset supplies it |
|  [07]   | `build/native/include/*.h`                                         | native headers | `[07]`                                           |

- [07]: `onnxruntime_c_api.h`, `onnxruntime_cxx_api.h`, `onnxruntime_ep_c_api.h`, `onnxruntime_lite_custom_op.h`, plus the three config-key headers (section 4).

[PACKAGE_ASSET_SCOPE]: per-RID native ABI matrix (the canonical owner the genai lane composes)
- rail: model

[NATIVE_PAYLOAD_OWNER]:
`NATIVE_PAYLOAD_OWNER` is the owning per-RID native-payload matrix for the ONNX runtime floor. `api-onnxruntimegenai` `native-rids`/`native-floor`, `api-onnxruntime-gpu`, and `api-onnxruntime-directml` declare their managed-floor pin and defer RID/ABI facts here.

Base meta-package assets ship only the `win-{x64,arm64}` payloads inline; every other RID's `libonnxruntime.{dylib,so}` resolves through the platform/feed asset the build copies. Accelerated providers ride sibling packages that supersede the base runtime on their RID: `.Gpu` adds CUDA + TensorRT native on `win-x64`/`linux-x64`; `.DirectML` is a RID-gated managed-floor hold shipping a DML-enabled `onnxruntime.dll` plus `Microsoft.AI.DirectML`'s `DirectML.dll` on `win-x64`/`win-arm64`. Genai native assets co-locate per RID beside this runtime, so a run without a matching base payload faults at native initialization.

`[ACCEL]` keys each per-RID accelerated-provider native below; `.Gpu.Windows`/`.Gpu.Linux` follow the central pin, and `.DirectML` retains its managed-floor hold.

| [INDEX] | [RID]           | [BASE_RUNTIME_NATIVE]                                             | [ACCEL] | [SHIPPED_BY]                   |
| :-----: | :-------------- | :---------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `win-x64`       | `onnxruntime.dll` + `onnxruntime_providers_shared.dll` (+ `.lib`) | `[01]`  | base inline; providers sibling |
|  [02]   | `win-arm64`     | `onnxruntime.dll` + `onnxruntime_providers_shared.dll` (+ `.lib`) | `[02]`  | base inline; DML sibling       |
|  [03]   | `linux-x64`     | `libonnxruntime.so` + `libonnxruntime_providers_shared.so`        | `[03]`  | base feed; providers sibling   |
|  [04]   | `linux-arm64`   | `libonnxruntime.so` + `libonnxruntime_providers_shared.so`        | none    | base feed                      |
|  [05]   | `osx-arm64`     | `libonnxruntime.dylib` ONLY — NO separate provider native         | `[05]`  | base feed                      |
|  [06]   | `android`/`ios` | `onnxruntime.aar` / `onnxruntime.xcframework.zip` archive form    | `[06]`  | base feed                      |

- [01]: `onnxruntime_providers_cuda.dll` / `onnxruntime_providers_tensorrt.dll` (`.Gpu.Windows`); DML-enabled `onnxruntime.dll` + `DirectML.dll` (`.DirectML`).
- [02]: no CUDA/TensorRT (x64-only); DML-enabled `onnxruntime.dll` + `DirectML.dll` (`.DirectML`).
- [03]: `libonnxruntime_providers_cuda.so` / `libonnxruntime_providers_tensorrt.so` (`.Gpu.Linux`).
- [05]: none; the CoreML EP is built INTO the base dylib (the verified host asset).
- [06]: none (mobile EPs — NNAPI/CoreML — built into the archive).

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: inference operations
- rail: model

| [INDEX] | [SURFACE]                                                         | [CALL_SHAPE]   | [CAPABILITY] |
| :-----: | :---------------------------------------------------------------- | :------------- | :----------- |
|  [01]   | `InferenceSession.Run`                                            | run call       | `[01]`       |
|  [02]   | `InferenceSession.RunAsync`                                       | async run call | `[02]`       |
|  [03]   | `RunWithBinding`                                                  | bound-run call | `[03]`       |
|  [04]   | `RunWithBindingAndNames`                                          | bound-run call | `[04]`       |
|  [05]   | `RunWithBoundResults`                                             | bound-run call | `[05]`       |
|  [06]   | `EndProfiling`                                                    | session call   | `[06]`       |
|  [07]   | `ProfilingStartTimeNs`                                            | session prop   | `[07]`       |
|  [08]   | `CreateIoBinding`                                                 | factory call   | `[08]`       |
|  [09]   | `InputMetadata`/`OutputMetadata`/`OverridableInitializerMetadata` | session read   | `[09]`       |
|  [10]   | `InputNames`/`OutputNames`                                        | session read   | `[10]`       |
|  [11]   | `ModelMetadata`                                                   | session read   | `[11]`       |
|  [12]   | `GetEpDeviceForInputs`                                            | session read   | `[12]`       |
|  [13]   | `GetMemoryInfosForInputs`/`GetMemoryInfosForOutputs`              | session read   | `[13]`       |
|  [14]   | `Dispose`                                                         | lifetime call  | `[14]`       |

- [01]: `IDisposableReadOnlyCollection<OrtValue> Run(RunOptions, IReadOnlyCollection<string> inputNames, IReadOnlyCollection<OrtValue> inputValues, IReadOnlyCollection<string> outputNames)` — the OrtValue-only overload the `RunOps.Infer` fold drives (plus a `IReadOnlyDictionary<string,OrtValue>` input mirror).
- [02]: `Task<IReadOnlyCollection<OrtValue>> RunAsync(RunOptions, inputNames, inputValues, outputNames, outputValues)` — REQUIRES caller-PRE-ALLOCATED output `OrtValue`s and completes on a native callback off the lane scope; the design routes async through the lane seam instead.
- [03]: `void RunWithBinding(RunOptions, OrtIoBinding)` — executes a pre-bound inference; outputs read back via `OrtIoBinding.GetOutputValues`.
- [04]: `IDisposableReadOnlyCollection<DisposableNamedOnnxValue> RunWithBindingAndNames(RunOptions, OrtIoBinding, string[] names = null)` — returns the FORBIDDEN `DisposableNamedOnnxValue` collection; the design instead zips `GetOutputNames()` against `GetOutputValues()`.
- [05]: `IDisposableReadOnlyCollection<OrtValue> RunWithBoundResults(RunOptions, OrtIoBinding)` — OrtValue-only bound results in one call.
- [06]: `string EndProfiling()` closes the chrome-trace and returns its path; pairs with `ProfilingStartTimeNs`.
- [07]: `ulong` profiling epoch — receipt-relative trace start.
- [08]: `OrtIoBinding CreateIoBinding()`.
- [09]: `IReadOnlyDictionary<string,NodeMetadata>` — model I/O shape/dtype introspection (sizes bound buffers, validates free-dim overrides).
- [10]: `IReadOnlyList<string>` ordered I/O names for binding/zip.
- [11]: `ModelMetadata` — version/producer/`CustomMetadataMap` for identity.
- [12]: `IReadOnlyList<OrtEpDevice>` — the autoEP device chosen per input slot, closing the autoEP loop after `AppendExecutionProvider(env, devices, …)`.
- [13]: `IDisposableReadOnlyCollection<OrtMemoryInfo>` — where each I/O buffer must live, driving device-residency binding decisions.
- [14]: releases native handles.

[ENTRYPOINT_SCOPE]: execution-provider append operations
- rail: model
- note: all are instance `option call`s on `SessionOptions`; `[06]-[REGISTRATION_MEMBERS]` owns the verified full signatures of `[14]`/`[15]`

| [INDEX] | [SURFACE]                                                      | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------- | :-------------------------------------------- |
|  [01]   | `AppendExecutionProvider_CPU(int useArena = 1)`                | CPU EP; `useArena` 1 enables memory arena     |
|  [02]   | `AppendExecutionProvider_CUDA(int deviceId = 0)`               | CUDA EP by device index                       |
|  [03]   | `AppendExecutionProvider_CUDA(OrtCUDAProviderOptions)`         | CUDA EP with full provider-options struct     |
|  [04]   | `AppendExecutionProvider_DML(int deviceId = 0)`                | DirectML EP by device index                   |
|  [05]   | `AppendExecutionProvider_Tensorrt(int deviceId = 0)`           | TensorRT EP by device index                   |
|  [06]   | `AppendExecutionProvider_Tensorrt(OrtTensorRTProviderOptions)` | TensorRT EP with full provider-options struct |
|  [07]   | `AppendExecutionProvider_ROCm(int deviceId = 0)`               | ROCm EP by device index                       |
|  [08]   | `AppendExecutionProvider_ROCm(OrtROCMProviderOptions)`         | ROCm EP with full provider-options struct     |
|  [09]   | `AppendExecutionProvider_CoreML(CoreMLFlags coremlFlags)`      | CoreML EP with compute-unit flags             |
|  [10]   | `AppendExecutionProvider_OpenVINO(string deviceId = "")`       | OpenVINO EP by device string                  |
|  [11]   | `AppendExecutionProvider_MIGraphX(int deviceId = 0)`           | MIGraphX EP by device index                   |
|  [12]   | `AppendExecutionProvider_Nnapi(NnapiFlags nnapiFlags)`         | NNAPI EP with accelerator-mode flags          |
|  [13]   | `AppendExecutionProvider_Dnnl(int useArena = 1)`               | DNNL EP; `useArena` mirrors CPU EP semantics  |
|  [14]   | `AppendExecutionProvider(providerName, providerOptions)`       | generic EP by name and key-value options      |
|  [15]   | `AppendExecutionProvider(env, epDevices, epOptions)`           | EP from autoEP device list and options        |

[ENTRYPOINT_SCOPE]: value and provider operations
- rail: model

| [INDEX] | [SURFACE]                                                                          | [CALL_SHAPE]    | [CAPABILITY] |
| :-----: | :--------------------------------------------------------------------------------- | :-------------- | :----------- |
|  [01]   | `OrtValue.CreateTensorValueFromMemory<T>(T[], long[])`                             | factory call    | `[01]`       |
|  [02]   | `OrtValue.CreateTensorValueFromMemory<T>(OrtMemoryInfo, Memory<T>, long[])`        | factory call    | `[02]`       |
|  [03]   | `OrtValue.CreateTensorValueFromSystemNumericsTensorObject<T>(Tensor<T>)`           | factory call    | `[03]`       |
|  [04]   | `OrtValue.CreateFromStringTensor(Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>)` | factory call    | `[04]`       |
|  [05]   | `NamedOnnxValue.CreateFromTensor<T>(string, Tensor<T>)`                            | factory call    | `[05]`       |
|  [06]   | `RegisterCustomOpLibrary`                                                          | option call     | `[06]`       |
|  [07]   | `RegisterCustomOpLibraryV2`                                                        | option call     | `[07]`       |
|  [08]   | `RegisterOrtExtensions`                                                            | option call     | `[08]`       |
|  [09]   | `SessionOptions.EnableMemoryPattern` / `EnableProfiling` / `EnableCpuMemArena`     | option property | `[09]`       |
|  [10]   | `RunOptions.AddRunConfigEntry`                                                     | run call        | `[10]`       |

- [01]: binds managed array as tensor.
- [02]: binds device-pinned memory as tensor.
- [03]: binds `System.Numerics.Tensors.Tensor<T>`.
- [04]: binds the ONNX-owned `Tensor<string>` — distinct from `[03]`'s `System.Numerics.Tensors.Tensor<T>`.
- [05]: creates named value from tensor (superseded path — OrtValue-only law).
- [06]: `void RegisterCustomOpLibrary(string)` loads custom operators (no handle — ORT-managed lifetime, the leak-free spelling).
- [07]: `void RegisterCustomOpLibraryV2(string, out nint)` — legacy caller-owns-handle path; a discarded `out _` handle leaks the library, so prefer `[06]`.
- [08]: loads extension ops.
- [09]: `bool` toggles (memory reuse / profiling / CPU arena) — assigned, not called.
- [10]: sets run config entry.

[ENTRYPOINT_SCOPE]: environment, session-policy, and run-policy operations
- rail: model

Provider names include `CoreMLExecutionProvider` and `CPUExecutionProvider`; threading options carry typed global thread and spin settings.

[ENVIRONMENT_OPERATIONS]:

| [INDEX] | [SURFACE]                                                 | [CALL_SHAPE]    | [CAPABILITY] |
| :-----: | :-------------------------------------------------------- | :-------------- | :----------- |
|  [01]   | `OrtEnv.IsCreated`                                        | static property | `[01]`       |
|  [02]   | `OrtEnv.Instance()`                                       | static call     | `[02]`       |
|  [03]   | `OrtEnv.CreateInstanceWithOptions(ref opts)`              | static call     | `[03]`       |
|  [04]   | `OrtEnv.DisableTelemetryEvents` / `EnableTelemetryEvents` | instance call   | `[04]`       |
|  [05]   | `OrtEnv.GetVersionString`                                 | instance call   | `[05]`       |
|  [06]   | `OrtEnv.GetAvailableProviders`                            | instance call   | `[06]`       |
|  [07]   | `OrtEnv.GetEpDevices`                                     | instance call   | `[07]`       |
|  [08]   | `OrtEnv.GetHardwareDevices` / `GetNumHardwareDevices`     | instance call   | `[08]`       |
|  [09]   | `OrtEnv.GetCompatibilityInfoFromModel`                    | instance call   | `[09]`       |
|  [10]   | `OrtEnv.GetModelCompatibilityForEpDevices`                | instance call   | `[10]`       |
|  [11]   | `OrtEnv.GetHardwareDeviceEpIncompatibilityDetails`        | instance call   | `[11]`       |
|  [12]   | `OrtEnv.CreateSharedAllocator`                            | instance call   | `[12]`       |
|  [13]   | `OrtEnv.GetSharedAllocator`                               | instance call   | `[13]`       |
|  [14]   | `OrtEnv.ReleaseSharedAllocator`                           | instance call   | `[14]`       |
|  [15]   | `OrtEnv.CreateAndRegisterAllocator`                       | instance call   | `[15]`       |
|  [16]   | `OrtEnv.RegisterExecutionProviderLibrary`                 | instance call   | `[16]`       |
|  [17]   | `OrtEnv.CopyTensors`                                      | instance call   | `[17]`       |
|  [18]   | `OrtEnv.EnvLogLevel`                                      | instance prop   | `[18]`       |

- [01]: reports environment creation.
- [02]: returns the singleton environment.
- [03]: boots with `EnvironmentCreationOptions` (passed `ref`).
- [04]: silences / re-enables runtime telemetry.
- [05]: reports the native runtime version (stamps the deterministic result-cache key).
- [06]: `string[]` registered provider names.
- [07]: `IReadOnlyList<OrtEpDevice>` — enumerates autoEP-available devices.
- [08]: `IReadOnlyList<OrtHardwareDevice>` / `int` raw hardware enumeration beneath the EP devices.
- [09]: `string GetCompatibilityInfoFromModel(string modelPath, string epType)` — produces the compat-info STRING a later `GetModelCompatibilityForEpDevices` consumes; `GetCompatibilityInfoFromModelBytes(byte[], epType)` is the in-memory mirror. Compat info is embedded by EP-context compilation: probe the COMPILED artifact — an uncompiled source model carries none and the verdict lands `EP_NOT_APPLICABLE`.
- [10]: `OrtCompiledModelCompatibility GetModelCompatibilityForEpDevices(IReadOnlyList<OrtEpDevice> epDevices, string compatibilityInfo)` — the 2nd arg is the compat-info STRING from `GetCompatibilityInfoFromModel`, NOT a model path; returns the `EP_*` ENUM the warm-start branch reads.
- [11]: `OrtDeviceEpIncompatibilityDetails GetHardwareDeviceEpIncompatibilityDetails(string epName, OrtHardwareDevice)` — driver/device/dependency reason bitmask + notes when a device is rejected.
- [12]: `(OrtEpDevice, OrtDeviceMemoryType, OrtAllocatorType, IReadOnlyDictionary<string,string> allocatorOptions)` returns `OrtAllocator`.
- [13]: `OrtAllocator GetSharedAllocator(OrtMemoryInfo)` — reads back the shared allocator for a memory descriptor.
- [14]: releases a shared allocator for a device and memory type.
- [15]: `(OrtMemoryInfo, OrtArenaCfg)` / `(string providerType, OrtMemoryInfo, OrtArenaCfg, IReadOnlyDictionary<string,string> provider_options)` registers a BFC-arena allocator with `UnregisterAllocator(OrtMemoryInfo)` the retract arm.
- [16]: `(string registrationName, string libraryPath)` registers an out-of-tree EP shared library for autoEP discovery; `UnregisterExecutionProviderLibrary(name)` the retract arm.
- [17]: `(IReadOnlyList<OrtValue> src, IReadOnlyList<OrtValue> dst, OrtSyncStream)` device-to-device tensor copy over a sync stream.
- [18]: live env log-severity floor.

[SESSION_POLICY_OPERATIONS]:

| [INDEX] | [SURFACE]                                                                      | [CALL_SHAPE]    | [CAPABILITY] |
| :-----: | :----------------------------------------------------------------------------- | :-------------- | :----------- |
|  [01]   | `SessionOptions.AddSessionConfigEntry`                                         | option call     | `[01]`       |
|  [02]   | `SessionOptions.AddFreeDimensionOverrideByName`                                | option call     | `[02]`       |
|  [03]   | `SessionOptions.AddFreeDimensionOverride`                                      | option call     | `[03]`       |
|  [04]   | `SessionOptions.AddInitializer`                                                | option call     | `[04]`       |
|  [05]   | `SessionOptions.DisablePerSessionThreads`                                      | option call     | `[05]`       |
|  [06]   | `SessionOptions.SetEpSelectionPolicy`                                          | option call     | `[06]`       |
|  [07]   | `SessionOptions.SetEpSelectionPolicyDelegate`                                  | option call     | `[07]`       |
|  [08]   | `SessionOptions.EnableProfiling`                                               | option property | `[08]`       |
|  [09]   | `SessionOptions.ProfileOutputPathPrefix`                                       | option property | `[09]`       |
|  [10]   | `SessionOptions.GraphOptimizationLevel`                                        | option property | `[10]`       |
|  [11]   | `SessionOptions.OptimizedModelFilePath`                                        | option property | `[11]`       |
|  [12]   | `SessionOptions.ExecutionMode`                                                 | option property | `[12]`       |
|  [13]   | `SessionOptions.EnableCpuMemArena`                                             | option property | `[13]`       |
|  [14]   | `SessionOptions.IntraOpNumThreads`                                             | option property | `[14]`       |
|  [15]   | `SessionOptions.InterOpNumThreads`                                             | option property | `[15]`       |
|  [16]   | `SessionOptions.LogId` / `LogSeverityLevel` / `LogVerbosityLevel`              | option property | `[16]`       |
|  [17]   | `SessionOptions.SetLoadCancellationFlag`                                       | option call     | `[17]`       |
|  [18]   | `SessionOptions.MakeSessionOptionWithCudaProvider`                             | static factory  | `[18]`       |
|  [19]   | `SessionOptions.MakeSessionOptionWithTensorrtProvider`                         | static factory  | `[19]`       |
|  [20]   | `SessionOptions.MakeSessionOptionWithRocmProvider`                             | static factory  | `[20]`       |
|  [21]   | `SessionOptions.EpSelectionDelegate`                                           | delegate type   | `[21]`       |
|  [22]   | `SessionOptionsContainer.Register` / `Create` / `ApplyConfiguration` / `Reset` | static rail     | `[22]`       |

- [01]: sets string config entry.
- [02]: binds symbolic dimension value.
- [03]: binds dimension by denotation string.
- [04]: `(string, OrtValue)` injects a pre-loaded initializer.
- [05]: routes sessions onto the global pool.
- [06]: applies `ExecutionProviderDevicePolicy` enum to session.
- [07]: `(EpSelectionDelegate)` applies a custom device-rank fn.
- [08]: enables chrome-trace profiling.
- [09]: sets profile artifact prefix.
- [10]: sets graph optimization level.
- [11]: writes the post-optimization graph to disk — a managed warm-start path beside the `ep.context_*` EP-context blob.
- [12]: `ORT_SEQUENTIAL` or `ORT_PARALLEL` execution.
- [13]: toggles the CPU BFC arena (mirrors the `AppendExecutionProvider_CPU(useArena)` switch at session scope).
- [14]: per-session intra-op thread count.
- [15]: per-session inter-op thread count.
- [16]: per-session log id, severity floor, and verbosity.
- [17]: `(bool)` — cooperatively aborts an in-flight model load, the load-time counterpart to `RunOptions.Terminate`, so a deadline-bound `Open` cancels a slow compile/load rather than blocking.
- [18]: `(int deviceId)` or `(OrtCUDAProviderOptions)` shorthand.
- [19]: `(int)` or `(OrtTensorRTProviderOptions)` shorthand.
- [20]: `(int)` or `(OrtROCMProviderOptions)` shorthand.
- [21]: `(IReadOnlyList<OrtEpDevice>, OrtKeyValuePairs, OrtKeyValuePairs, uint) -> List<OrtEpDevice>`.
- [22]: named-configuration registry — `Register(name, Action<SessionOptions>)` stores a reusable profile, `Create(name)` / `options.ApplyConfiguration(name)` applies it; collapses repeated session-config construction into one named row rather than re-folding the config keys per open.

[VALUE_RUN_OPERATIONS]:

| [INDEX] | [SURFACE]                                                             | [CALL_SHAPE]  | [CAPABILITY] |
| :-----: | :-------------------------------------------------------------------- | :------------ | :----------- |
|  [01]   | `OrtValue.CreateTensorWithEmptyStrings`                               | factory call  | `[01]`       |
|  [02]   | `OrtValue.GetStringElement(index)`                                    | egress read   | `[02]`       |
|  [03]   | `OrtValue.GetStringTensorAsArray()`                                   | egress read   | `[03]`       |
|  [04]   | `OrtValue.StringTensorSetElementAt`                                   | ingress write | `[04]`       |
|  [05]   | `OrtValue.CreateSequence` / `CreateMap*`                              | factory call  | `[05]`       |
|  [06]   | `OrtValue.ProcessSequence` / `ProcessMap`                             | visitor call  | `[06]`       |
|  [07]   | `OrtValue.OnnxType` / `IsTensor` / `IsSparseTensor` / `GetTypeInfo()` | value read    | `[07]`       |
|  [08]   | `RunOptions.Terminate`                                                | run property  | `[08]`       |
|  [09]   | `RunOptions.AddActiveLoraAdapter`                                     | run call      | `[09]`       |
|  [10]   | `OrtLoraAdapter.Create`                                               | factory call  | `[10]`       |

- [01]: `(OrtAllocator, long[] shape)` allocates string output slots.
- [02]: `string` reads one string-tensor element; `GetStringElementAsSpan`/`GetStringElementAsMemory` are the zero-copy mirrors.
- [03]: `string[]` bulk string-tensor read — one call instead of element-wise iteration.
- [04]: `(ReadOnlySpan<char>/<byte>/ReadOnlyMemory<char>, int index)` writes one element into an empty-string slot.
- [05]: `CreateSequence(ICollection<OrtValue>)`, `CreateMap`/`CreateMapWithStringKeys`/`CreateMapWithStringValues` build sequence/ZipMap outputs.
- [06]: `(visitor, OrtAllocator)` folds a sequence/map output (e.g. classifier ZipMap) without materializing a managed collection; `GetValue(int, OrtAllocator)` / `GetValueCount()` index it.
- [07]: discriminates the `OnnxValueType` of a live output before projection.
- [08]: one-way cancellation latch.
- [09]: attaches `OrtLoraAdapter` for a run.
- [10]: `Create(string adapterPath, OrtAllocator)` / `Create(byte[] bytes, OrtAllocator)` — BOTH overloads require an `OrtAllocator` (e.g. `OrtAllocator.DefaultInstance`).

[METADATA_THREADING_OPERATIONS]:

| [INDEX] | [SURFACE]                                                                                 | [CALL_SHAPE]       | [CAPABILITY] |
| :-----: | :---------------------------------------------------------------------------------------- | :----------------- | :----------- |
|  [01]   | `ModelMetadata.{Version, ProducerName, Domain, GraphName, GraphDescription, Description}` | metadata prop      | `[01]`       |
|  [02]   | `ModelMetadata.CustomMetadataMap`                                                         | metadata prop      | `[02]`       |
|  [03]   | `NodeMetadata.ElementDataType`                                                            | metadata prop      | `[03]`       |
|  [04]   | `NodeMetadata.Dimensions`                                                                 | metadata prop      | `[04]`       |
|  [05]   | `NodeMetadata.SymbolicDimensions`                                                         | metadata prop      | `[05]`       |
|  [06]   | `NodeMetadata.OnnxValueType` / `IsTensor` / `IsString` / `ElementType`                    | metadata prop      | `[06]`       |
|  [07]   | `NodeMetadata.AsSequenceMetadata` / `AsMapMetadata` / `AsOptionalMetadata`                | metadata call      | `[07]`       |
|  [08]   | `OrtThreadingOptions.GlobalIntraOpNumThreads`                                             | rw property        | `[08]`       |
|  [09]   | `OrtThreadingOptions.GlobalInterOpNumThreads`                                             | rw property        | `[09]`       |
|  [10]   | `OrtThreadingOptions.GlobalSpinControl`                                                   | rw property        | `[10]`       |
|  [11]   | `OrtThreadingOptions.SetGlobalDenormalAsZero`                                             | option call        | `[11]`       |
|  [12]   | `OrtThreadingOptions`                                                                     | parameterless ctor | `[12]`       |

- [01]: graph version + provenance strings.
- [02]: `Dictionary<string,string>` author-stamped key-values — folds into the model-identity fingerprint.
- [03]: tensor slot element type (`TensorElementType`).
- [04]: `int[]` fixed dimensions (-1 = symbolic).
- [05]: `string[]` free-dimension names — pair with `AddFreeDimensionOverrideByName`.
- [06]: value kind + CLR element type of the slot.
- [07]: descends into sequence/map/optional slot shapes.
- [08]: global intra-op thread count.
- [09]: global inter-op thread count.
- [10]: spin versus low-CPU policy.
- [11]: flushes subnormal floats to zero on the global pool — latency/throughput knob.
- [12]: thread-pool options handle (`SafeHandle`).

[ENTRYPOINT_SCOPE]: IO-binding bound-inference operations
- rail: model

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]    | [CAPABILITY] |
| :-----: | :--------------------------------------------- | :-------------- | :----------- |
|  [01]   | `InferenceSession.CreateIoBinding`             | factory call    | `[01]`       |
|  [02]   | `InferenceSession.RunWithBinding`              | bound-run call  | `[02]`       |
|  [03]   | `OrtIoBinding.BindInput`                       | bind call       | `[03]`       |
|  [04]   | `OrtIoBinding.BindInput`                       | bind call       | `[04]`       |
|  [05]   | `OrtIoBinding.BindInput`                       | bind call       | `[05]`       |
|  [06]   | `OrtIoBinding.BindOutput`                      | bind call       | `[06]`       |
|  [07]   | `OrtIoBinding.BindOutput`                      | bind call       | `[07]`       |
|  [08]   | `OrtIoBinding.BindOutput`                      | bind call       | `[08]`       |
|  [09]   | `OrtIoBinding.BindOutputToDevice`              | bind call       | `[09]`       |
|  [10]   | `OrtIoBinding.SynchronizeBoundInputs`          | sync call       | `[10]`       |
|  [11]   | `OrtIoBinding.SynchronizeBoundOutputs`         | sync call       | `[11]`       |
|  [12]   | `OrtIoBinding.GetOutputNames`                  | binding read    | `[12]`       |
|  [13]   | `OrtIoBinding.GetOutputValues`                 | binding read    | `[13]`       |
|  [14]   | `OrtIoBinding.ClearBoundInputs`                | reset call      | `[14]`       |
|  [15]   | `OrtIoBinding.ClearBoundOutputs`               | reset call      | `[15]`       |
|  [16]   | `OrtValue.CreateTensorValueWithData`           | factory call    | `[16]`       |
|  [17]   | `OrtValue.CreateAllocatedTensorValue`          | factory call    | `[17]`       |
|  [18]   | `OrtValue.GetTensorDataAsSpan<T>`              | span read       | `[18]`       |
|  [19]   | `OrtValue.GetTensorMutableDataAsSpan<T>`       | span write      | `[19]`       |
|  [20]   | `OrtValue.GetTensorDataAsTensorSpan<T>`        | span read       | `[20]`       |
|  [21]   | `OrtValue.GetTensorMutableDataAsTensorSpan<T>` | span write      | `[21]`       |
|  [22]   | `OrtValue.GetTensorMutableRawData`             | raw read        | `[22]`       |
|  [23]   | `OrtValue.GetTensorSizeInBytes`                | metadata read   | `[23]`       |
|  [24]   | `OrtValue.GetTensorTypeAndShape`               | metadata read   | `[24]`       |
|  [25]   | `OrtValue.GetTensorMemoryInfo`                 | metadata read   | `[25]`       |
|  [26]   | `OrtMemoryInfo.DefaultInstance`                | static property | `[26]`       |
|  [27]   | `OrtMemoryInfo`                                | ctor            | `[27]`       |
|  [28]   | `OrtMemoryInfo`                                | ctor            | `[28]`       |
|  [29]   | `OrtMemoryInfo.Name`                           | accessor read   | `[29]`       |
|  [30]   | `OrtMemoryInfo.Id`                             | accessor read   | `[30]`       |
|  [31]   | `OrtMemoryInfo.GetAllocatorType`               | accessor read   | `[31]`       |
|  [32]   | `OrtMemoryInfo.GetMemoryType`                  | accessor read   | `[32]`       |
|  [33]   | `OrtMemoryInfo.GetDeviceMemoryType`            | accessor read   | `[33]`       |
|  [34]   | `OrtMemoryInfo.GetVendorId`                    | accessor read   | `[34]`       |

- [01]: returns an `OrtIoBinding` bound to the session.
- [02]: `(RunOptions, OrtIoBinding)` executes a pre-bound inference.
- [03]: `(string, OrtValue)` binds a named input value.
- [04]: `(string, TensorElementType, long[], OrtMemoryAllocation)` binds device input.
- [05]: `(string, OrtExternalAllocation)` binds a caller-owned external device buffer.
- [06]: `(string, OrtValue)` binds a named output value.
- [07]: `(string, TensorElementType, long[], OrtMemoryAllocation)` binds device output.
- [08]: `(string, OrtExternalAllocation)` binds a caller-owned external output buffer.
- [09]: `(string, OrtMemoryInfo)` binds an output to a device allocator.
- [10]: flushes pending bound input transfers.
- [11]: flushes pending bound output transfers.
- [12]: `string[]` returns bound output names.
- [13]: `IDisposableReadOnlyCollection<OrtValue>` returns bound output values.
- [14]: clears all bound inputs.
- [15]: clears all bound outputs.
- [16]: `(OrtMemoryInfo, TensorElementType, long[], nint, long)` binds device memory.
- [17]: `(OrtAllocator, TensorElementType, long[])` allocates an output tensor.
- [18]: `ReadOnlySpan<T>` reads bound tensor data.
- [19]: `Span<T>` writes bound tensor data.
- [20]: `ReadOnlyTensorSpan<T>` reads tensor data as `System.Numerics.Tensors` span.
- [21]: `TensorSpan<T>` writes tensor data as `System.Numerics.Tensors` span.
- [22]: `Span<byte>` reads raw tensor bytes.
- [23]: returns total byte count for the tensor buffer.
- [24]: returns `OrtTensorTypeAndShapeInfo` describing element type and shape.
- [25]: returns `OrtMemoryInfo` for where the tensor buffer lives.
- [26]: shared CPU `OrtMemoryInfo` for default-device binding.
- [27]: `(string, OrtAllocatorType, int, OrtMemType)` builds a device descriptor.
- [28]: `(string, OrtMemoryInfoDeviceType, uint vendorId, int deviceId, OrtDeviceMemoryType, ulong alignment, OrtAllocatorType)` extended device descriptor.
- [29]: `string Name { get; }` — allocator/device name; the `GetTensorMemoryInfo().Name` arena name the `ModelRun` receipt stamps as `ArenaAllocator`.
- [30]: `int Id { get; }` — device id the descriptor was created against.
- [31]: `OrtAllocatorType GetAllocatorType()` — arena/device allocator classifier of the descriptor.
- [32]: `OrtMemType GetMemoryType()` — the legacy CPU/output mem-type axis (distinct from the V2 device-memory class).
- [33]: `OrtDeviceMemoryType GetDeviceMemoryType()` — the V2 `DEFAULT`/`HOST_ACCESSIBLE` device-memory class.
- [34]: `uint GetVendorId()` — device vendor id; the read-side mirror of the [26] extended ctor.

## [04]-[CONFIG_KEYS]

[CONFIG_KEY_SCOPE]: session-options config-entry keys (decompile/header-verified, set through `SessionOptions.AddSessionConfigEntry`)
- rail: model
- source: `build/native/include/onnxruntime_session_options_config_keys.h`

EP-context (warm-start / fleet-compiled context) family — the `Open`/`Compile` folds drive these:

| [INDEX] | [KEY_STRING]                                         | [VALUE_DOMAIN]         | [ROLE]                                                  |
| :-----: | :--------------------------------------------------- | :--------------------- | :------------------------------------------------------ |
|  [01]   | `ep.context_enable`                                  | `0` / `1`              | reads the precompiled EP-context graph on load          |
|  [02]   | `ep.context_file_path`                               | filesystem path string | the context blob location                               |
|  [03]   | `ep.context_embed_mode`                              | `0` / `1`              | embed the EP-context binary in the model vs. side-file  |
|  [04]   | `ep.context_node_name_prefix`                        | prefix string          | namespaces EP-context node names across fleet blobs     |
|  [05]   | `ep.context_model_external_initializers_file_name`   | filename string        | spills EP-context initializers to a side file           |
|  [06]   | `ep.enable_weightless_ep_context_nodes`              | `0` / `1`              | emits weightless context nodes (weights stay external)  |
|  [07]   | `ep.share_ep_contexts` / `ep.stop_share_ep_contexts` | `0` / `1`              | shares/halts one EP-context across co-resident sessions |

Base session / arena / quantization keys the design composes:

| [INDEX] | [KEY_STRING]                                                            | [VALUE_DOMAIN]         | [ROLE] |
| :-----: | :---------------------------------------------------------------------- | :--------------------- | :----- |
|  [01]   | `session.disable_cpu_ep_fallback`                                       | `0` / `1`              | `[01]` |
|  [02]   | `session.use_env_allocators`                                            | `0` / `1`              | `[02]` |
|  [03]   | `session.use_device_allocator_for_initializers`                         | `0` / `1`              | `[03]` |
|  [04]   | `session.set_denormal_as_zero`                                          | `0` / `1`              | `[04]` |
|  [05]   | `session.disable_prepacking`                                            | `0` / `1`              | `[05]` |
|  [06]   | `session.intra_op.allow_spinning` / `session.intra_op.spin_duration_us` | `0`/`1` / microseconds | `[06]` |
|  [07]   | `session.qdq_matmulnbits_accuracy_level`                                | accuracy-level int     | `[07]` |
|  [08]   | `session.qdq_matmulnbits_block_size`                                    | block-size int         | `[08]` |
|  [09]   | `session.enable_dq_matmulnbits_fusion`                                  | `0` / `1`              | `[09]` |
|  [10]   | `mlas.enable_gemm_fastmath_arm64_bfloat16`                              | `0` / `1`              | `[10]` |

- [01]: forbids silent CPU fallback so an accelerated EP that cannot claim a node FAILS rather than degrading unobserved.
- [02]: routes the session onto the env-registered shared allocator (`CreateAndRegisterAllocator`/`CreateSharedAllocator`).
- [03]: loads initializers straight into device memory.
- [04]: per-session subnormal flush (the `OrtThreadingOptions.SetGlobalDenormalAsZero` per-session twin).
- [05]: disables weight prepacking (paired with `PrePackedWeightsContainer`).
- [06]: fine-grained intra-op spin policy beneath `GlobalSpinControl`.
- [07]: int4/int8 `MatMulNBits` accuracy floor — the runtime knob the `ModelPrecision` int4/int8 rows fold against.
- [08]: NBits quantization block size.
- [09]: fuses dequant + NBits matmul for quantized graphs.
- [10]: arm64 bf16 GEMM fast-math — Apple-silicon CPU throughput knob.

[CONFIG_KEY_SCOPE]: run-options config-entry keys (set through `RunOptions.AddRunConfigEntry`)
- rail: model
- source: `build/native/include/onnxruntime_run_options_config_keys.h`

| [INDEX] | [KEY_STRING]                              | [VALUE_DOMAIN]          | [ROLE]                                                       |
| :-----: | :---------------------------------------- | :---------------------- | :----------------------------------------------------------- |
|  [01]   | `memory.enable_memory_arena_shrinkage`    | allocator device string | shrinks the arena after a run (the `RunConfig.Bulk` posture) |
|  [02]   | `disable_synchronize_execution_providers` | `0` / `1`               | skips inter-EP sync for single-EP runs (latency knob)        |
|  [03]   | `gpu_graph_id`                            | graph-id string         | CUDA-graph capture/replay annotation for fixed-shape runs    |

[CONFIG_KEY_SCOPE]: env config keys (`onnxruntime_env_config_keys.h`)
- rail: model

| [INDEX] | [KEY_STRING]            | [VALUE_DOMAIN] | [ROLE]                                                  |
| :-----: | :---------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `allow_virtual_devices` | `0` / `1`      | admits virtual hardware devices into autoEP enumeration |

[CONFIG_KEY_SCOPE]: CoreML provider-option keys
- rail: model

| [INDEX] | [KEY_STRING]                         | [VALUE_DOMAIN]                                      |
| :-----: | :----------------------------------- | :-------------------------------------------------- |
|  [01]   | `ModelFormat`                        | `MLProgram`, `NeuralNetwork`                        |
|  [02]   | `MLComputeUnits`                     | `ALL`, `CPUOnly`, `CPUAndGPU`, `CPUAndNeuralEngine` |
|  [03]   | `RequireStaticInputShapes`           | `0`, `1`                                            |
|  [04]   | `EnableOnSubgraphs`                  | `0`, `1`                                            |
|  [05]   | `SpecializationStrategy`             | `Default`, `FastPrediction`                         |
|  [06]   | `ProfileComputePlan`                 | `0`, `1`                                            |
|  [07]   | `AllowLowPrecisionAccumulationOnGPU` | `0`, `1`                                            |
|  [08]   | `ModelCacheDirectory`                | cache-directory path string                         |

## [05]-[IMPLEMENTATION_LAW]

[MODEL_SESSION]:
- namespace: `Microsoft.ML.OnnxRuntime`
- session root: `InferenceSession`
- policy root: `SessionOptions`
- run root: `RunOptions`
- metadata root: model, node, tensor, and element classifiers
- lifetime: native handles release through deterministic disposal

[EXECUTION_PROVIDER_SELECTION]:
- explicit EP: call `AppendExecutionProvider_CPU/CUDA/DML/Tensorrt/ROCm/CoreML/OpenVINO/MIGraphX/Nnapi/Dnnl` before session construction; order determines fallback priority
- generic EP: `AppendExecutionProvider(string, Dictionary<string,string>)` selects by provider-name string for any registered provider (a bare `"CoreML"` faults `InvalidArgument`; the registered name is `"CoreMLExecutionProvider"`)
- autoEP device-list EP: `AppendExecutionProvider(OrtEnv, IReadOnlyList<OrtEpDevice>, IReadOnlyDictionary<string,string>)` binds directly to devices from `OrtEnv.GetEpDevices()`; close the loop with `InferenceSession.GetEpDeviceForInputs()` to read back which device claimed each input
- policy EP: `SetEpSelectionPolicy(ExecutionProviderDevicePolicy)` sets an enum-driven auto-selection strategy; `SetEpSelectionPolicyDelegate(EpSelectionDelegate)` replaces it with a managed callback that receives `(IReadOnlyList<OrtEpDevice>, OrtKeyValuePairs modelMetadata, OrtKeyValuePairs runtimeMetadata, uint maxSelections)` and returns a ranked `List<OrtEpDevice>`
- device enumeration: `OrtEnv.GetEpDevices()` returns `IReadOnlyList<OrtEpDevice>`, each carrying `EpName`, `EpVendor`, `HardwareDevice` (`Type`, `VendorId`, `DeviceId`, `Vendor`, `Metadata`), `EpMetadata`, and `EpOptions`; `GetHardwareDevices()`/`GetNumHardwareDevices()` expose the raw hardware layer beneath
- out-of-tree EP: `OrtEnv.RegisterExecutionProviderLibrary(registrationName, libraryPath)` registers a provider shared library so its devices enter `GetEpDevices()` enumeration; `UnregisterExecutionProviderLibrary` retracts it

[COMPATIBILITY_PROBE]: the warm-start admissibility decision is a TWO-STEP enum contract, not a string match —
- step 1: `string info = OrtEnv.Instance().GetCompatibilityInfoFromModel(modelPath, epType)` (or `GetCompatibilityInfoFromModelBytes(bytes, epType)`) produces the model's per-EP compatibility-info STRING; `epType` is the provider name (e.g. `"CoreMLExecutionProvider"`)
- step 2: `OrtCompiledModelCompatibility verdict = OrtEnv.Instance().GetModelCompatibilityForEpDevices(devices, info)` consumes THAT string and returns the `OrtCompiledModelCompatibility` ENUM
- branch on the enum, never on `verdict.ToString().Contains("Incompatible")` (no enum name contains that substring): `EP_UNSUPPORTED` and `EP_SUPPORTED_PREFER_RECOMPILATION` mean fresh-compile (clear the `ep.context_*` keys); `EP_SUPPORTED_OPTIMAL` keeps the warm-start read; `EP_NOT_APPLICABLE` means the device is not EP-context-aware so warm-start is moot
- on a rejected device, `OrtEnv.GetHardwareDeviceEpIncompatibilityDetails(epName, hardwareDevice)` returns `OrtDeviceEpIncompatibilityDetails` whose `ReasonsBitmask` (`DriverIncompatible`/`DeviceIncompatible`/`MissingDependency`) + `Notes` + `ErrorCode` populate a precise degradation receipt

[IO_BINDING]:
- binding root: `OrtIoBinding` from `InferenceSession.CreateIoBinding`
- bound run: `InferenceSession.RunWithBinding(RunOptions, OrtIoBinding)` executes against pre-bound input and output slots
- input binding: `BindInput` accepts an `OrtValue` or a device `(TensorElementType, long[], OrtMemoryAllocation)` tuple
- output binding: `BindOutput` mirrors input binding; `BindOutputToDevice(string, OrtMemoryInfo)` routes outputs to a device allocator
- synchronization: `SynchronizeBoundInputs` and `SynchronizeBoundOutputs` flush pending device transfers around the bound run
- output projection: `GetOutputNames` and `GetOutputValues` read bound results; `ClearBoundInputs` and `ClearBoundOutputs` reset the binding between runs
- steady state: binding amortizes input and output allocation across repeated runs and is the measured hot-loop path

[NATIVE_RUNTIME]:
- OWNERSHIP: this page owns both the EP/device selection matrix (sections 2-3 `ExecutionProvider*`/`AppendExecutionProvider_*`/`OrtEpDevice` plus `[EXECUTION_PROVIDER_SELECTION]`) and the per-RID native ABI matrix. Genai, `.Gpu`, and `.DirectML` catalogs declare their managed-floor pin and compose these facts without restating the EP roster or RID payload set.
- package asset: native runtime libraries; base meta-package `runtimes/` ships only `win-x64` and `win-arm64` payloads inline. macOS arm64 and Linux x64/arm64 runtimes load from the platform/feed-resolved `libonnxruntime.{dylib,so}` copied by the build. CoreML is built into the macOS dylib, while CUDA/TensorRT and DirectML ride sibling packages on matching RIDs.
- build assets: `build/native/*.props`/`*.targets` copy native runtime libraries by RID
- provider policy: execution-provider selection is explicit model-rail policy; append order is fallback priority — the genai `Config.AppendProvider`/`SetProviderOption`/`SetDecoderProviderOptionsHardware*` surface selects FROM this EP roster and binds to devices this page's `OrtEpDevice`/`OrtHardwareDevice` discovery enumerates

[LOCAL_ADMISSION]:
- Compute model execution enters through ONNX Runtime sessions and typed value binding.
- Model load, input binding, run policy, output projection, and disposal each emit receipts.
- Provider selection is policy data and cannot hide inside model-call helpers.
- Custom operators enter only through declared session options and asset evidence.

[STACKING]: the managed surface is internalized into single dense `Model/*` rails alongside the sibling admitted libs, never wrapped one-to-one —
- boot rail: `OrtThreadingOptions` (`GlobalIntraOp/InterOpNumThreads`, `GlobalSpinControl`) reads the AppHost `CpuBudget`, folds into `EnvironmentCreationOptions`, and `OrtEnv.CreateInstanceWithOptions(ref)` boots once behind `OrtEnv.IsCreated`; `DisableTelemetryEvents` runs at boot because the telemetry spine owns signals
- session rail: `SessionOptions` config-entry keys + EP `Register` + `OrtModelCompilationOptions` EP-context compile fold into ONE `Open`, the resident map keys on `ResidentKey(ModelIdentity.Checksum, ModelFingerprint.Of(SessionPolicy.SessionRows(ep)))`, and the compiled context blob is content-addressed through the device-aware `ContextKey` over `OrtEpDevice` `EpName`/`VendorId`/`DeviceId`/`HardwareDevice.Type` then crosses to the Persistence blob lane as an `ArtifactIndexRow` — one artifact owner, not a second EP cache
- provider rail: the `ExecutionProvider` `[SmartEnum<string>]` (Thinktecture) carries each EP's option-table/`ExecutionProviderDevicePolicy`/`OrtHardwareDeviceType`-affinity columns; `GetEpDevices()` → device-rank → `AppendExecutionProvider(env, devices, …)` is ONE polymorphic `Register`, and the two-step `GetCompatibilityInfoFromModel` → `GetModelCompatibilityForEpDevices` enum verdict is read once and CONSUMED into the warm-start branch
- run rail: `OrtValue` carriers admit through a `[Union]` `RunInput`, `OrtIoBinding` amortizes the steady-state loop over a `CreateSharedAllocator` arena, `RunOptions.Terminate` is latched off the AppHost `CancelScope`, `System.Numerics.Tensors.TensorPrimitives` owns the reductions, every projection lands in a LanguageExt `Fin<T>` inside a native-disposal bracket, and the deterministic result keys through `Microsoft.Extensions.Caching.Hybrid` stamped with `GetVersionString()` so cross-version numerical drift never serves a stale hit
- time rail: every receipt carries `NodaTime` `Instant`/`Duration`; profiling chrome-trace (`EndProfiling` + `ProfilingStartTimeNs`) lands as an `ArtifactIndexRow`

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime`
- Owns: ONNX session execution, EP-context compilation, autoEP device discovery, and native runtime assets
- Accept: measured model inference and EP-context warm-start/fleet-compile
- Reject: the on-device-training surface (`TrainingSession`, `CheckpointState`, `TrainingUtils`, `OrtTrainingApi`, `LRScheduler`); ML.NET training pipeline; `NamedOnnxValue`/`DisposableNamedOnnxValue`/`FixedBufferOnnxValue` value carriers (OrtValue-only law)

## [06]-[REGISTRATION_MEMBERS]

[ENTRYPOINT_SCOPE]: custom-op, extension, and EP registration decompile-verified signatures
- rail: model (session-options registration; consumed by `Model/extension#EXTENSION_OPS` and `Model/providers#EP_AXIS`)

| [INDEX] | [MEMBER]                                  | [SIGNATURE] |
| :-----: | :---------------------------------------- | :---------- |
|  [01]   | `RegisterCustomOpLibrary`                 | `[01]`      |
|  [02]   | `RegisterCustomOpLibraryV2`               | `[02]`      |
|  [03]   | `RegisterOrtExtensions`                   | `[03]`      |
|  [04]   | `SetEpSelectionPolicy`                    | `[04]`      |
|  [05]   | `SetEpSelectionPolicyDelegate`            | `[05]`      |
|  [06]   | `AppendExecutionProvider_CPU`             | `[06]`      |
|  [07]   | `AppendExecutionProvider_CUDA`            | `[07]`      |
|  [08]   | `AppendExecutionProvider_DML`             | `[08]`      |
|  [09]   | `AppendExecutionProvider_Tensorrt`        | `[09]`      |
|  [10]   | `AppendExecutionProvider_ROCm`            | `[10]`      |
|  [11]   | `AppendExecutionProvider_CoreML`          | `[11]`      |
|  [12]   | `AppendExecutionProvider`                 | `[12]`      |
|  [13]   | `AppendExecutionProvider`                 | `[13]`      |
|  [14]   | `OrtEnv.GetEpDevices`                     | `[14]`      |
|  [15]   | `MakeSessionOptionWithCudaProvider`       | `[15]`      |
|  [16]   | `MakeSessionOptionWithTensorrtProvider`   | `[16]`      |
|  [17]   | `MakeSessionOptionWithRocmProvider`       | `[17]`      |
|  [18]   | `OrtEnv.RegisterExecutionProviderLibrary` | `[18]`      |

- [01]: `void RegisterCustomOpLibrary(string libraryPath)`.
- [02]: `void RegisterCustomOpLibraryV2(string libraryPath, out nint libraryHandle)`.
- [03]: `void RegisterOrtExtensions()` — calls `OrtExtensionsNativeMethods.RegisterCustomOps`; throws `OnnxRuntimeException(ErrorCode.NoSuchFile)` if `Microsoft.ML.OnnxRuntime.Extensions` native asset is absent.
- [04]: `void SetEpSelectionPolicy(ExecutionProviderDevicePolicy policy)`.
- [05]: `void SetEpSelectionPolicyDelegate(EpSelectionDelegate selectionDelegate = null)`.
- [06]: `void AppendExecutionProvider_CPU(int useArena = 1)`.
- [07]: `void AppendExecutionProvider_CUDA(int deviceId = 0)` / `void AppendExecutionProvider_CUDA(OrtCUDAProviderOptions)`.
- [08]: `void AppendExecutionProvider_DML(int deviceId = 0)`.
- [09]: `void AppendExecutionProvider_Tensorrt(int deviceId = 0)` / `void AppendExecutionProvider_Tensorrt(OrtTensorRTProviderOptions)`.
- [10]: `void AppendExecutionProvider_ROCm(int deviceId = 0)` / `void AppendExecutionProvider_ROCm(OrtROCMProviderOptions)`.
- [11]: `void AppendExecutionProvider_CoreML(CoreMLFlags coremlFlags = CoreMLFlags.COREML_FLAG_USE_NONE)`.
- [12]: `void AppendExecutionProvider(string providerName, Dictionary<string, string> providerOptions = null)`.
- [13]: `void AppendExecutionProvider(OrtEnv env, IReadOnlyList<OrtEpDevice> epDevices, IReadOnlyDictionary<string, string> epOptions)`.
- [14]: `IReadOnlyList<OrtEpDevice> GetEpDevices()`.
- [15]: `static SessionOptions MakeSessionOptionWithCudaProvider(int deviceId = 0)` / `(OrtCUDAProviderOptions)` — one-call session+EP shorthand.
- [16]: `static SessionOptions MakeSessionOptionWithTensorrtProvider(int deviceId = 0)` / `(OrtTensorRTProviderOptions)`.
- [17]: `static SessionOptions MakeSessionOptionWithRocmProvider(int deviceId = 0)` / `(OrtROCMProviderOptions)`.
- [18]: `void RegisterExecutionProviderLibrary(string registrationName, string libraryPath)` — autoEP out-of-tree provider registration; `UnregisterExecutionProviderLibrary(string)` retracts.

[REGISTRATION_LAW]:
- `RegisterCustomOpLibrary(path)` maps to `OrtRegisterCustomOpsLibrary_V2` in the C API (load and register from path, no handle returned) — ONNX Runtime owns the library lifetime and frees it when the `SessionOptions` and every session built from them release, so this is the canonical leak-free registration spelling that tracks no caller handle.
- `RegisterCustomOpLibraryV2(path, out nint)` maps to `OrtRegisterCustomOpsLibrary` (load and register, returning a native handle the CALLER then owns) — the legacy caller-must-free path: a discarded `out _` handle never unloads and leaks the library, so it is the rejected spelling.
- `RegisterOrtExtensions()` is a convenience wrapper that loads the `libortextensions` native asset shipped by `Microsoft.ML.OnnxRuntime.Extensions`; there is no separate public `OrtExtensions` class — `OrtExtensionsNativeMethods` is internal.
- `OrtExtensions.RegisterCustomOps` does not exist as a public API in either the ORT managed assembly or the Extensions package; the correct entry point is `SessionOptions.RegisterOrtExtensions()`.
- `UseModel` does not exist on `SessionOptions` or `InferenceSession` in; it is not part of this package's public surface.
- `DisposableNamedOnnxValue.CreateFromOrtValue` is internal; it is not a callable public factory — callers consume `DisposableNamedOnnxValue` from `InferenceSession.Run` result collections only.

## [07]-[COMPILE_API]

`OrtModelCompilationOptions` (namespace `Microsoft.ML.OnnxRuntime.CompileApi`) drives the EP-context compile pipeline: it builds a compiled model whose execution-provider partitions are embedded or written to disk so a later session loads the precompiled graph instead of recompiling. Section 4's `ep.context_*` session-config keys activate the EP-context path; `Model/sessions#SESSION_CAPSULE` `Compile` drives the fleet-shared device-keyed context.

[ENTRYPOINT_SCOPE]: compile-API delegate contracts (nested inside `OrtModelCompilationOptions`, not standalone)
- rail: model

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]       | [CAPABILITY]                                                |
| :-----: | :--------------------------------- | :------------------- | :---------------------------------------------------------- |
|  [01]   | `WriteBufferToDestinationDelegate` | write delegate       | streams the compiled output buffer to a caller sink         |
|  [02]   | `GetInitializerLocationDelegate`   | initializer delegate | maps an initializer to `OrtExternalInitializerInfo` storage |

[ENTRYPOINT_SCOPE]: `OrtModelCompilationOptions` decompile-verified signatures
- source: `Microsoft.ML.OnnxRuntime.Managed` (`net8.0` consumer-bound asset) decompile
- rail: model (consumed by `Model/sessions#SESSION_CAPSULE` `Compile`)

| [INDEX] | [MEMBER]                                       | [SIGNATURE] |
| :-----: | :--------------------------------------------- | :---------- |
|  [01]   | `ctor`                                         | `[01]`      |
|  [02]   | `CompileModel`                                 | `[02]`      |
|  [03]   | `SetInputModelPath`                            | `[03]`      |
|  [04]   | `SetInputModelFromBuffer`                      | `[04]`      |
|  [05]   | `SetOutputModelPath`                           | `[05]`      |
|  [06]   | `SetOutputModelExternalInitializersFile`       | `[06]`      |
|  [07]   | `SetEpContextEmbedMode`                        | `[07]`      |
|  [08]   | `SetFlags`                                     | `[08]`      |
|  [09]   | `SetEpContextBinaryInformation`                | `[09]`      |
|  [10]   | `SetGraphOptimizationLevel`                    | `[10]`      |
|  [11]   | `SetOutputModelWriteDelegate`                  | `[11]`      |
|  [12]   | `SetOutputModelGetInitializerLocationDelegate` | `[12]`      |
|  [13]   | `Dispose`                                      | `[13]`      |
|  [14]   | `WriteBufferToDestinationDelegate`             | `[14]`      |
|  [15]   | `GetInitializerLocationDelegate`               | `[15]`      |
|  [16]   | `OrtCompileApiFlags`                           | `[16]`      |

- [01]: `OrtModelCompilationOptions(SessionOptions sessionOptions)`.
- [02]: `void CompileModel()`.
- [03]: `void SetInputModelPath(string path)`.
- [04]: `void SetInputModelFromBuffer(byte[] buffer)`.
- [05]: `void SetOutputModelPath(string path)`.
- [06]: `void SetOutputModelExternalInitializersFile(string filePath, ulong threshold)`.
- [07]: `void SetEpContextEmbedMode(bool embed)`.
- [08]: `void SetFlags(OrtCompileApiFlags flags)`.
- [09]: `void SetEpContextBinaryInformation(string outputDirectory, string modelName)`.
- [10]: `void SetGraphOptimizationLevel(GraphOptimizationLevel graphOptimizationLevel)`.
- [11]: `void SetOutputModelWriteDelegate(WriteBufferToDestinationDelegate writeBufferDelegate)`.
- [12]: `void SetOutputModelGetInitializerLocationDelegate(GetInitializerLocationDelegate getInitializerLocationDelegate)`.
- [13]: `void Dispose()`.
- [14]: `delegate void WriteBufferToDestinationDelegate(ReadOnlySpan<byte> buffer)`.
- [15]: `delegate OrtExternalInitializerInfo GetInitializerLocationDelegate(string initializerName, IReadOnlyOrtValue initializerValue, IReadOnlyExternalInitializerInfo originalInitializerLocation)`.
- [16]: `enum OrtCompileApiFlags : uint { NONE = 0, ERROR_IF_NO_NODES_COMPILED = 1, ERROR_IF_OUTPUT_FILE_EXISTS = 2 }`.

[COMPILE_LAW]:
- `OrtModelCompilationOptions(SessionOptions)` binds the compile to a configured session whose EP append order determines which provider claims each subgraph; `CompileModel()` runs the partition and write.
- input source is exclusive: `SetInputModelPath(string)` reads from disk and `SetInputModelFromBuffer(byte[])` reads from memory.
- output destination is exclusive: `SetOutputModelPath(string)` writes to disk, `SetOutputModelWriteDelegate` streams the buffer to a caller sink, and `SetOutputModelExternalInitializersFile(string, ulong)` spills initializers over the byte threshold to a side file.
- `SetEpContextEmbedMode(bool)` embeds the EP-context binary inside the compiled model when `true`, or writes it beside the model when `false`; `SetEpContextBinaryInformation(string, string)` names the output directory and model for the external EP-context binary.
- `SetFlags(OrtCompileApiFlags)` controls compile strictness: `ERROR_IF_NO_NODES_COMPILED` fails when no subgraph is EP-claimed, and `ERROR_IF_OUTPUT_FILE_EXISTS` fails rather than overwriting.
- `SetGraphOptimizationLevel(GraphOptimizationLevel)` applies graph optimization before partitioning; `SetOutputModelGetInitializerLocationDelegate` routes each initializer to an external storage location.
- Compile handle is native; `Dispose()` releases it and registered delegate state deterministically.

# [RASM_COMPUTE_API_ONNXRUNTIME]

`Microsoft.ML.OnnxRuntime` owns measured ONNX model-session execution for the Compute model rail: session construction, typed `OrtValue` binding, run-policy deadlines, execution-provider selection, EP-context warm-start compilation, autoEP hardware-device discovery, and the native runtime assets they bind. Every native handle is `SafeHandle`/`IDisposable` and releases deterministically. `Model/*` internalizes the managed surface into dense boot, session, provider, and run rails rather than one-to-one wrappers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime`
- package: `Microsoft.ML.OnnxRuntime` (MIT)
- assembly: `Microsoft.ML.OnnxRuntime.Managed` — a `net10.0` non-mobile consumer binds the `lib/net8.0` asset; the `net9.0-{android,ios,maccatalyst}` assets are mobile TFMs, so the surface decompiles against the consumer-bound `net8.0` assembly
- namespace: `Microsoft.ML.OnnxRuntime`, `Microsoft.ML.OnnxRuntime.CompileApi`, `Microsoft.ML.OnnxRuntime.Tensors`
- asset: managed runtime library and per-RID native runtime DLLs; accelerated providers ride sibling `.Gpu`/`.DirectML` packages
- rail: model

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: session and run contracts

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :------------------------- | :------------ | :--------------------------------- |
|  [01]   | `InferenceSession`         | class         | model-run executor                 |
|  [02]   | `SessionOptions`           | class         | session policy config              |
|  [03]   | `RunOptions`               | class         | inference-run policy               |
|  [04]   | `OrtEnv`                   | class         | runtime environment scope          |
|  [05]   | `OrtMemoryInfo`            | class         | tensor memory descriptor           |
|  [06]   | `OrtValue`                 | class         | native model value                 |
|  [07]   | `NamedOnnxValue`           | class         | named input binder                 |
|  [08]   | `DisposableNamedOnnxValue` | class         | named output owner                 |
|  [09]   | `TensorElementType`        | enum          | tensor-data classifier             |
|  [10]   | `OrtAllocatorType`         | enum          | `DeviceAllocator`/`ArenaAllocator` |

[PUBLIC_TYPE_SCOPE]: metadata and tensor contracts

| [INDEX] | [SYMBOL]                    | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :-------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `ModelMetadata`             | class         | model-identity metadata                                           |
|  [02]   | `NodeMetadata`              | class         | I/O slot metadata                                                 |
|  [03]   | `OrtTensorTypeAndShapeInfo` | class         | tensor type/shape info                                            |
|  [04]   | `DenseTensor<T>`            | class         | dense tensor carrier                                              |
|  [05]   | `OrtIoBinding`              | class         | bound-run binder                                                  |
|  [06]   | `PrePackedWeightsContainer` | class         | cross-session weight cache                                        |
|  [07]   | `FixedBufferOnnxValue`      | class         | fixed managed-buffer value                                        |
|  [08]   | `OrtLoraAdapter`            | class         | LoRA adapter handle                                               |
|  [09]   | `OrtMemoryAllocation`       | class         | device buffer                                                     |
|  [10]   | `OnnxValueType`             | enum          | `ONNX_TYPE_TENSOR`/`_SEQUENCE`/`_MAP`/`_OPTIONAL`/`_SPARSETENSOR` |
|  [11]   | `IReadOnlyOrtValue`         | interface     | read-only `OrtValue` view                                         |
|  [12]   | `OrtTypeInfo`               | class         | live-value shape introspection                                    |
|  [13]   | `SessionOptionsContainer`   | class         | named session-config registry                                     |
|  [14]   | `SequenceMetadata`          | class         | recursive sequence schema                                         |
|  [15]   | `OptionalMetadata`          | class         | recursive optional schema                                         |
|  [16]   | `MapMetadata`               | class         | recursive map schema                                              |

- `MapMetadata` exposes `KeyDataType`/`ValueMetadata` and `SequenceMetadata`/`OptionalMetadata` expose `ElementMeta`, returned by the `NodeMetadata.As*Metadata` descent; `ModelMetadata.CustomMetadataMap` feeds model-identity fingerprinting.
- `PrePackedWeightsContainer` shares prepacked weights across same-model sessions through the `InferenceSession` third ctor argument.

[PUBLIC_TYPE_SCOPE]: environment, threading, and provider-policy contracts

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :---------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `EnvironmentCreationOptions`        | struct        | env boot options                       |
|  [02]   | `OrtThreadingOptions`               | class         | global thread-pool policy              |
|  [03]   | `ExecutionProviderDevicePolicy`     | enum          | device-selection policy                |
|  [04]   | `GraphOptimizationLevel`            | enum          | graph-optimization level               |
|  [05]   | `OrtLoggingLevel`                   | enum          | boot log severity                      |
|  [06]   | `OrtAllocator`                      | class         | allocator handle                       |
|  [07]   | `OrtArenaCfg`                       | class         | BFC-arena config                       |
|  [08]   | `OrtExternalAllocation`             | class         | caller-owned device buffer             |
|  [09]   | `IDisposableReadOnlyCollection<T>`  | interface     | native result set                      |
|  [10]   | `BFloat16` / `Float16`              | struct        | bf16/fp16 CLR carriers                 |
|  [11]   | `OrtEpDevice`                       | class         | EP device descriptor                   |
|  [12]   | `OrtHardwareDevice`                 | class         | hardware descriptor                    |
|  [13]   | `OrtHardwareDeviceType`             | enum          | `CPU`/`GPU`/`NPU`                      |
|  [14]   | `OrtMemoryInfoDeviceType`           | enum          | `CPU`/`GPU`/`FPGA`/`NPU`               |
|  [15]   | `OrtKeyValuePairs`                  | class         | kv-pairs handle                        |
|  [16]   | `OrtDeviceMemoryType`               | enum          | `DEFAULT`/`HOST_ACCESSIBLE`            |
|  [17]   | `OrtMemType`                        | enum          | `CpuInput`/`CpuOutput`/`Cpu`/`Default` |
|  [18]   | `OrtSyncStream`                     | class         | device sync-stream handle              |
|  [19]   | `OrtCompiledModelCompatibility`     | enum          | warm-start compatibility verdict       |
|  [20]   | `OrtDeviceEpIncompatibilityDetails` | class         | rejection diagnostic                   |
|  [21]   | `OrtDeviceEpIncompatibilityReason`  | enum          | `[Flags]` incompatibility reasons      |
|  [22]   | `CoreMLFlags`                       | enum          | `[Flags]` CoreML compute-unit set      |
|  [23]   | `NnapiFlags`                        | enum          | `[Flags]` NNAPI accelerator set        |
|  [24]   | `OrtCUDAProviderOptions`            | class         | CUDA provider options                  |
|  [25]   | `OrtTensorRTProviderOptions`        | class         | TensorRT provider options              |

- `ExecutionProviderDevicePolicy`: `DEFAULT` `PREFER_CPU` `PREFER_NPU` `PREFER_GPU` `MAX_PERFORMANCE` `MAX_EFFICIENCY` `MIN_OVERALL_POWER`.
- `GraphOptimizationLevel`: `ORT_DISABLE_ALL` `ORT_ENABLE_BASIC` `ORT_ENABLE_EXTENDED` `ORT_ENABLE_LAYOUT` `ORT_ENABLE_ALL`.
- `OrtCompiledModelCompatibility`: `EP_NOT_APPLICABLE` `EP_SUPPORTED_OPTIMAL` `EP_SUPPORTED_PREFER_RECOMPILATION` `EP_UNSUPPORTED`.
- `OrtDeviceEpIncompatibilityReason`: `None` `DriverIncompatible` `DeviceIncompatible` `MissingDependency` `Unknown`.
- `EnvironmentCreationOptions` carries `logId`, `logLevel` (nullable), `threadOptions`, `loggingParam` (nullable), `loggingFunction`, passed `ref` to `OrtEnv.CreateInstanceWithOptions`.
- `OrtEpDevice` carries `EpName`, `EpVendor`, `EpMetadata`/`EpOptions` (`OrtKeyValuePairs`), `HardwareDevice`, `GetMemoryInfo(OrtDeviceMemoryType)`, `CreateSyncStream(IReadOnlyDictionary<string,string>)`; `OrtHardwareDevice` carries `Type`, `VendorId`, `Vendor`, `DeviceId`, `Metadata`.
- `OrtDeviceEpIncompatibilityDetails` carries `ReasonsBitmask`, `Notes`, `ErrorCode` (`IDisposable`), paired with `OrtEnv.GetHardwareDeviceEpIncompatibilityDetails`.
- `OrtCUDAProviderOptions`/`OrtTensorRTProviderOptions`/`OrtROCMProviderOptions` carry `UpdateOptions(Dictionary<string,string>)` and `GetOptions()`; `ProviderOptionsValueHelper.StringToDict` parses a serialized options string.

[PUBLIC_TYPE_SCOPE]: execution-mode, ROCm, and compile-API contracts

| [INDEX] | [SYMBOL]                           | [TYPE_FAMILY] | [CAPABILITY]                                                      |
| :-----: | :--------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `ExecutionMode`                    | enum          | `ORT_SEQUENTIAL`/`ORT_PARALLEL`                                   |
|  [02]   | `OrtROCMProviderOptions`           | class         | ROCm provider options                                             |
|  [03]   | `OrtModelCompilationOptions`       | class         | EP-context compile pipeline                                       |
|  [04]   | `OrtCompileApiFlags`               | enum          | `NONE`/`ERROR_IF_NO_NODES_COMPILED`/`ERROR_IF_OUTPUT_FILE_EXISTS` |
|  [05]   | `WriteBufferToDestinationDelegate` | delegate      | streams compiled output to a caller sink                          |
|  [06]   | `GetInitializerLocationDelegate`   | delegate      | maps an initializer to `OrtExternalInitializerInfo` storage       |

[PACKAGE_ASSET_SCOPE]: MSBuild imports and native headers

| [INDEX] | [SYMBOL]                                        | [TYPE_FAMILY]  | [CAPABILITY]                |
| :-----: | :---------------------------------------------- | :------------- | :-------------------------- |
|  [01]   | `build/native/Microsoft.ML.OnnxRuntime.props`   | MSBuild import | declares native copy        |
|  [02]   | `build/native/Microsoft.ML.OnnxRuntime.targets` | MSBuild import | copies native assets by RID |
|  [03]   | `build/native/include/*.h`                      | native headers | C/C++/EP/config-key headers |

- headers ship `onnxruntime_c_api.h`, `onnxruntime_cxx_api.h`, `onnxruntime_ep_c_api.h`, `onnxruntime_lite_custom_op.h`, and the three config-key headers section 4 mines.

[PACKAGE_ASSET_SCOPE]: per-RID native ABI matrix

Base assets ship the `win-{x64,arm64}` payloads inline; every other RID resolves `libonnxruntime.{dylib,so}` through the platform/feed asset the build copies. `.Gpu` adds CUDA + TensorRT native on `win-x64`/`linux-x64`; `.DirectML` ships a DML-enabled `onnxruntime.dll` and `DirectML.dll` on `win-{x64,arm64}`. Genai native co-locates per RID beside this runtime, so a run without a matching base payload faults at native initialization.

| [INDEX] | [RID]           | [BASE_RUNTIME_NATIVE]                                                 | [ACCEL] | [SHIPPED_BY]                   |
| :-----: | :-------------- | :-------------------------------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `win-x64`       | `onnxruntime.dll` + `onnxruntime_providers_shared.dll` (+ `.lib`)     | `[01]`  | base inline; providers sibling |
|  [02]   | `win-arm64`     | `onnxruntime.dll` + `onnxruntime_providers_shared.dll` (+ `.lib`)     | `[02]`  | base inline; DML sibling       |
|  [03]   | `linux-x64`     | `libonnxruntime.so` + `libonnxruntime_providers_shared.so`            | `[03]`  | base feed; providers sibling   |
|  [04]   | `linux-arm64`   | `libonnxruntime.so` + `libonnxruntime_providers_shared.so`            | none    | base feed                      |
|  [05]   | `osx-arm64`     | `libonnxruntime.dylib` — no separate provider native, CoreML built in | `[05]`  | base feed                      |
|  [06]   | `android`/`ios` | `onnxruntime.aar` / `onnxruntime.xcframework.zip` archive             | `[06]`  | base feed                      |

- [01]: `onnxruntime_providers_cuda.dll` / `onnxruntime_providers_tensorrt.dll` (`.Gpu`); DML-enabled `onnxruntime.dll` + `DirectML.dll` (`.DirectML`).
- [02]: DML-enabled `onnxruntime.dll` + `DirectML.dll` (`.DirectML`); no CUDA/TensorRT.
- [03]: `libonnxruntime_providers_cuda.so` / `libonnxruntime_providers_tensorrt.so` (`.Gpu`).
- [05]: CoreML EP built into the base dylib.
- [06]: mobile EPs (NNAPI/CoreML) built into the archive.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: inference operations on `InferenceSession`

| [INDEX] | [SURFACE]                                                          | [SHAPE]  | [CAPABILITY]                           |
| :-----: | :----------------------------------------------------------------- | :------- | :------------------------------------- |
|  [01]   | `Run(RunOptions, string[], OrtValue[], string[])`                  | instance | OrtValue-only run; `RunOps.Infer` fold |
|  [02]   | `RunAsync(RunOptions, string[], OrtValue[], string[], OrtValue[])` | instance | async run                              |
|  [03]   | `RunWithBinding(RunOptions, OrtIoBinding)`                         | instance | pre-bound run                          |
|  [04]   | `RunWithBoundResults(RunOptions, OrtIoBinding)`                    | instance | OrtValue-only bound results            |
|  [05]   | `CreateIoBinding() -> OrtIoBinding`                                | factory  | binding rooted to the session          |
|  [06]   | `EndProfiling() -> string`                                         | instance | closes chrome-trace, returns path      |
|  [07]   | `ProfilingStartTimeNs`                                             | property | trace epoch (receipt-relative)         |
|  [08]   | `{InputMetadata, OutputMetadata, OverridableInitializerMetadata}`  | property | I/O shape/dtype introspection          |
|  [09]   | `{InputNames, OutputNames}`                                        | property | ordered I/O names for binding/zip      |
|  [10]   | `ModelMetadata`                                                    | property | version/producer/`CustomMetadataMap`   |
|  [11]   | `GetEpDeviceForInputs() -> IReadOnlyList<OrtEpDevice>`             | instance | autoEP device chosen per input         |
|  [12]   | `{GetMemoryInfosForInputs, GetMemoryInfosForOutputs}`              | instance | per-I/O residency for device binding   |
|  [13]   | `Dispose()`                                                        | instance | releases native handles                |

- `Run` returns `IDisposableReadOnlyCollection<OrtValue>` and also accepts an `IReadOnlyDictionary<string,OrtValue>` input mirror; `RunWithBoundResults` returns the same, read back from binding via `OrtIoBinding.GetOutputValues`.
- `RunAsync` requires caller-pre-allocated output `OrtValue`s and completes on a native callback off the lane scope; the design routes async through the lane seam.

[ENTRYPOINT_SCOPE]: `SessionOptions.AppendExecutionProvider*` — instance EP append; append order is fallback priority

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :------------------------------------------------------------ | :------- | :---------------------------------------- |
|  [01]   | `_CPU(int useArena = 1)`                                      | instance | CPU EP; `useArena` 1 enables memory arena |
|  [02]   | `_CUDA(int deviceId = 0)`                                     | instance | CUDA EP by device index                   |
|  [03]   | `_CUDA(OrtCUDAProviderOptions)`                               | instance | CUDA EP with provider-options struct      |
|  [04]   | `_DML(int deviceId = 0)`                                      | instance | DirectML EP by device index               |
|  [05]   | `_Tensorrt(int deviceId = 0)`                                 | instance | TensorRT EP by device index               |
|  [06]   | `_Tensorrt(OrtTensorRTProviderOptions)`                       | instance | TensorRT EP with provider-options struct  |
|  [07]   | `_ROCm(int deviceId = 0)`                                     | instance | ROCm EP by device index                   |
|  [08]   | `_ROCm(OrtROCMProviderOptions)`                               | instance | ROCm EP with provider-options struct      |
|  [09]   | `_CoreML(CoreMLFlags = COREML_FLAG_USE_NONE)`                 | instance | CoreML EP with compute-unit flags         |
|  [10]   | `_OpenVINO(string deviceId = "")`                             | instance | OpenVINO EP by device string              |
|  [11]   | `_MIGraphX(int deviceId = 0)`                                 | instance | MIGraphX EP by device index               |
|  [12]   | `_Nnapi(NnapiFlags)`                                          | instance | NNAPI EP with accelerator-mode flags      |
|  [13]   | `_Dnnl(int useArena = 1)`                                     | instance | DNNL EP; `useArena` mirrors CPU EP        |
|  [14]   | `(string, Dictionary<string,string> = null)`                  | instance | generic EP by name and key-value options  |
|  [15]   | `(OrtEnv, OrtEpDevice[], IReadOnlyDictionary<string,string>)` | instance | EP from an autoEP device list             |

[ENTRYPOINT_SCOPE]: value construction, custom-op registration, and run config

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                         |
| :-----: | :-------------------------------------------------------------------------- | :------- | :----------------------------------- |
|  [01]   | `OrtValue.CreateTensorValueFromMemory<T>(T[], long[])`                      | factory  | binds managed array as tensor        |
|  [02]   | `OrtValue.CreateTensorValueFromMemory<T>(OrtMemoryInfo, Memory<T>, long[])` | factory  | binds device-pinned memory           |
|  [03]   | `OrtValue.CreateTensorValueFromSystemNumericsTensorObject<T>(Tensor<T>)`    | factory  | binds `System.Numerics.Tensors`      |
|  [04]   | `OrtValue.CreateFromStringTensor(Tensors.Tensor<string>)`                   | factory  | binds ONNX-owned string tensor       |
|  [05]   | `NamedOnnxValue.CreateFromTensor<T>(string, Tensor<T>)`                     | factory  | named value from tensor              |
|  [06]   | `SessionOptions.RegisterCustomOpLibrary(string)`                            | instance | loads custom operators               |
|  [07]   | `SessionOptions.RegisterCustomOpLibraryV2(string, out nint)`                | instance | custom ops, caller-owned handle      |
|  [08]   | `SessionOptions.RegisterOrtExtensions()`                                    | instance | loads extension ops                  |
|  [09]   | `SessionOptions.{EnableMemoryPattern, EnableProfiling, EnableCpuMemArena}`  | property | `bool` reuse/profiling/arena toggles |
|  [10]   | `RunOptions.AddRunConfigEntry(string, string)`                              | instance | sets a run config entry              |

- `RegisterCustomOpLibrary` maps to `OrtRegisterCustomOpsLibrary_V2`: ONNX Runtime owns the library lifetime and frees it when the options and every session release — the leak-free spelling that tracks no caller handle. `RegisterCustomOpLibraryV2` maps to `OrtRegisterCustomOpsLibrary` returning a caller-owned handle: a discarded `out _` never unloads and leaks the library.
- `RegisterOrtExtensions` loads the `libortextensions` native asset shipped by `Microsoft.ML.OnnxRuntime.Extensions`; an absent asset throws `OnnxRuntimeException(ErrorCode.NoSuchFile)`.

[ENTRYPOINT_SCOPE]: environment operations on `OrtEnv`

| [INDEX] | [SURFACE]                                                                        | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------------- | :------- | :--------------------------------------- |
|  [01]   | `IsCreated`                                                                      | static   | reports environment creation             |
|  [02]   | `Instance()`                                                                     | static   | returns the singleton environment        |
|  [03]   | `CreateInstanceWithOptions(ref EnvironmentCreationOptions)`                      | static   | boots the environment                    |
|  [04]   | `{DisableTelemetryEvents, EnableTelemetryEvents}`                                | instance | toggles runtime telemetry                |
|  [05]   | `GetVersionString()`                                                             | instance | native runtime version; result-cache key |
|  [06]   | `GetAvailableProviders() -> string[]`                                            | instance | registered provider names                |
|  [07]   | `GetEpDevices() -> IReadOnlyList<OrtEpDevice>`                                   | instance | autoEP-available device enumeration      |
|  [08]   | `{GetHardwareDevices, GetNumHardwareDevices}`                                    | instance | raw hardware layer beneath EP devices    |
|  [09]   | `GetCompatibilityInfoFromModel(string, string) -> string`                        | instance | per-EP compat-info string                |
|  [10]   | `GetModelCompatibilityForEpDevices(OrtEpDevice[], string)`                       | instance | warm-start enum verdict                  |
|  [11]   | `GetHardwareDeviceEpIncompatibilityDetails(string, OrtHardwareDevice)`           | instance | rejection reason bitmask                 |
|  [12]   | `CreateSharedAllocator(OrtEpDevice, OrtDeviceMemoryType, OrtAllocatorType, ...)` | instance | shared arena lease                       |
|  [13]   | `GetSharedAllocator(OrtMemoryInfo) -> OrtAllocator`                              | instance | reads back a shared allocator            |
|  [14]   | `ReleaseSharedAllocator`                                                         | instance | releases a shared allocator              |
|  [15]   | `CreateAndRegisterAllocator(OrtMemoryInfo, OrtArenaCfg)`                         | instance | registers a BFC-arena allocator          |
|  [16]   | `RegisterExecutionProviderLibrary(string, string)`                               | instance | out-of-tree EP registration              |
|  [17]   | `CopyTensors(OrtValue[], OrtValue[], OrtSyncStream)`                             | instance | device-to-device tensor copy             |
|  [18]   | `EnvLogLevel`                                                                    | property | live env log-severity floor              |

- `GetModelCompatibilityForEpDevices` takes the compat-info string from `GetCompatibilityInfoFromModel` (a `GetCompatibilityInfoFromModelBytes(byte[], string)` in-memory mirror exists), not a model path, and returns `OrtCompiledModelCompatibility`; branch on the `EP_*` enum, never a `ToString().Contains(...)` substring.
- `GetHardwareDeviceEpIncompatibilityDetails` returns `OrtDeviceEpIncompatibilityDetails`; `CreateSharedAllocator` takes a trailing `IReadOnlyDictionary<string,string>` allocator-options arg and returns `OrtAllocator`.
- Compat info is embedded by EP-context compilation: probe the compiled artifact, since an uncompiled source model carries none and the verdict lands `EP_NOT_APPLICABLE`.
- `CreateAndRegisterAllocator` also takes `(string providerType, OrtMemoryInfo, OrtArenaCfg, IReadOnlyDictionary<string,string>)`; `UnregisterAllocator(OrtMemoryInfo)` and `UnregisterExecutionProviderLibrary(string)` are the retract arms.

[ENTRYPOINT_SCOPE]: session-policy operations on `SessionOptions`

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                            |
| :-----: | :---------------------------------------------------------------------------- | :------- | :-------------------------------------- |
|  [01]   | `AddSessionConfigEntry(string, string)`                                       | instance | sets a string config entry              |
|  [02]   | `AddFreeDimensionOverrideByName(string, long)`                                | instance | binds a symbolic dimension value        |
|  [03]   | `AddFreeDimensionOverride(string, long)`                                      | instance | binds a dimension by denotation         |
|  [04]   | `AddInitializer(string, OrtValue)`                                            | instance | injects a pre-loaded initializer        |
|  [05]   | `DisablePerSessionThreads()`                                                  | instance | routes the session onto the global pool |
|  [06]   | `SetEpSelectionPolicy(ExecutionProviderDevicePolicy)`                         | instance | enum-driven auto EP selection           |
|  [07]   | `SetEpSelectionPolicyDelegate(EpSelectionDelegate = null)`                    | instance | custom device-rank callback             |
|  [08]   | `EnableProfiling`                                                             | property | chrome-trace profiling toggle           |
|  [09]   | `ProfileOutputPathPrefix`                                                     | property | profile artifact prefix                 |
|  [10]   | `GraphOptimizationLevel`                                                      | property | graph optimization level                |
|  [11]   | `OptimizedModelFilePath`                                                      | property | post-optimization graph to disk         |
|  [12]   | `ExecutionMode`                                                               | property | `ORT_SEQUENTIAL`/`ORT_PARALLEL`         |
|  [13]   | `EnableCpuMemArena`                                                           | property | CPU BFC-arena toggle                    |
|  [14]   | `IntraOpNumThreads`                                                           | property | per-session intra-op thread count       |
|  [15]   | `InterOpNumThreads`                                                           | property | per-session inter-op thread count       |
|  [16]   | `{LogId, LogSeverityLevel, LogVerbosityLevel}`                                | property | per-session log id/severity/verbosity   |
|  [17]   | `SetLoadCancellationFlag(bool)`                                               | instance | cooperatively aborts an in-flight load  |
|  [18]   | `MakeSessionOptionWithCudaProvider(int = 0 / OrtCUDAProviderOptions)`         | static   | one-call session+CUDA EP shorthand      |
|  [19]   | `MakeSessionOptionWithTensorrtProvider(int = 0 / OrtTensorRTProviderOptions)` | static   | one-call session+TensorRT shorthand     |
|  [20]   | `MakeSessionOptionWithRocmProvider(int = 0 / OrtROCMProviderOptions)`         | static   | one-call session+ROCm shorthand         |
|  [21]   | `EpSelectionDelegate`                                                         | delegate | custom device-rank callback type        |
|  [22]   | `SessionOptionsContainer.{Register, Create, ApplyConfiguration, Reset}`       | static   | reusable named session-config profiles  |

- `OptimizedModelFilePath` is a managed warm-start path beside the `ep.context_*` EP-context blob.
- `SetLoadCancellationFlag` is the load-time counterpart to `RunOptions.Terminate`, so a deadline-bound `Open` cancels a slow compile/load rather than blocking. `SetEpSelectionPolicyDelegate` binds an `EpSelectionDelegate` of shape `(IReadOnlyList<OrtEpDevice>, OrtKeyValuePairs, OrtKeyValuePairs, uint) -> List<OrtEpDevice>`.
- `SessionOptionsContainer` collapses repeated session-config construction into one named row: `Register(name, Action<SessionOptions>)` stores a profile, `Create(name)`/`ApplyConfiguration(name)` applies it.

[ENTRYPOINT_SCOPE]: value, string-tensor, and run-policy operations

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------ | :------- | :--------------------------------------- |
|  [01]   | `OrtValue.CreateTensorWithEmptyStrings(OrtAllocator, long[])`       | factory  | allocates string output slots            |
|  [02]   | `OrtValue.GetStringElement(int) -> string`                          | instance | one string-tensor element                |
|  [03]   | `OrtValue.GetStringTensorAsArray() -> string[]`                     | instance | bulk string-tensor read                  |
|  [04]   | `OrtValue.StringTensorSetElementAt(ReadOnlySpan<char>/<byte>, int)` | instance | writes one string slot                   |
|  [05]   | `OrtValue.CreateSequence / CreateMap*`                              | factory  | sequence/ZipMap outputs                  |
|  [06]   | `OrtValue.{ProcessSequence, ProcessMap}(visitor, OrtAllocator)`     | fold     | folds sequence/map without materializing |
|  [07]   | `OrtValue.{OnnxType, IsTensor, IsSparseTensor, GetTypeInfo()}`      | instance | discriminates live-value kind            |
|  [08]   | `RunOptions.Terminate`                                              | property | one-way cancellation latch               |
|  [09]   | `RunOptions.AddActiveLoraAdapter(OrtLoraAdapter)`                   | instance | attaches a LoRA adapter for a run        |
|  [10]   | `OrtLoraAdapter.Create(string/byte[], OrtAllocator)`                | factory  | builds a LoRA adapter                    |

- `CreateSequence(ICollection<OrtValue>)` and `CreateMap`/`CreateMapWithStringKeys`/`CreateMapWithStringValues` build sequence/ZipMap outputs; `OrtLoraAdapter.Create` both overloads require an `OrtAllocator`.
- `GetStringElement` has zero-copy `GetStringElementAsSpan`/`GetStringElementAsMemory` mirrors; `ProcessSequence`/`ProcessMap` index through `GetValue(int, OrtAllocator)`/`GetValueCount()`.

[ENTRYPOINT_SCOPE]: metadata and global-threading operations

| [INDEX] | [SURFACE]                                                                                 | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `ModelMetadata.{Version, ProducerName, Domain, GraphName, GraphDescription, Description}` | property | graph version + provenance       |
|  [02]   | `ModelMetadata.CustomMetadataMap`                                                         | property | author-stamped key-values        |
|  [03]   | `NodeMetadata.ElementDataType`                                                            | property | tensor slot element type         |
|  [04]   | `NodeMetadata.Dimensions`                                                                 | property | `int[]` fixed dims (-1 symbolic) |
|  [05]   | `NodeMetadata.SymbolicDimensions`                                                         | property | free-dimension names             |
|  [06]   | `NodeMetadata.{OnnxValueType, IsTensor, IsString, ElementType}`                           | property | value kind + CLR element type    |
|  [07]   | `NodeMetadata.{AsSequenceMetadata, AsMapMetadata, AsOptionalMetadata}`                    | instance | descends recursive slot shapes   |
|  [08]   | `OrtThreadingOptions.GlobalIntraOpNumThreads`                                             | property | global intra-op thread count     |
|  [09]   | `OrtThreadingOptions.GlobalInterOpNumThreads`                                             | property | global inter-op thread count     |
|  [10]   | `OrtThreadingOptions.GlobalSpinControl`                                                   | property | spin versus low-CPU policy       |
|  [11]   | `OrtThreadingOptions.SetGlobalDenormalAsZero()`                                           | instance | flushes subnormals on the pool   |
|  [12]   | `OrtThreadingOptions()`                                                                   | ctor     | thread-pool options handle       |

- `SymbolicDimensions` pairs with `AddFreeDimensionOverrideByName`; `GetTypeInfo()` introspects a live value where `NodeMetadata` describes a slot statically.

[ENTRYPOINT_SCOPE]: IO-binding bound-inference operations

| [INDEX] | [SURFACE]                                                                           | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :---------------------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `OrtIoBinding.BindInput(string, OrtValue)`                                          | instance | binds a named input value           |
|  [02]   | `OrtIoBinding.BindInput(string, TensorElementType, long[], OrtMemoryAllocation)`    | instance | binds device input                  |
|  [03]   | `OrtIoBinding.BindInput(string, OrtExternalAllocation)`                             | instance | binds a caller-owned buffer         |
|  [04]   | `OrtIoBinding.BindOutput(string, OrtValue)`                                         | instance | binds a named output value          |
|  [05]   | `OrtIoBinding.BindOutput(string, TensorElementType, long[], OrtMemoryAllocation)`   | instance | binds device output                 |
|  [06]   | `OrtIoBinding.BindOutput(string, OrtExternalAllocation)`                            | instance | binds a caller-owned output         |
|  [07]   | `OrtIoBinding.BindOutputToDevice(string, OrtMemoryInfo)`                            | instance | routes output to a device allocator |
|  [08]   | `OrtIoBinding.{SynchronizeBoundInputs, SynchronizeBoundOutputs}`                    | instance | flushes pending device transfers    |
|  [09]   | `OrtIoBinding.GetOutputNames() -> string[]`                                         | instance | bound output names                  |
|  [10]   | `OrtIoBinding.GetOutputValues() -> IDisposableReadOnlyCollection<OrtValue>`         | instance | bound output values                 |
|  [11]   | `OrtIoBinding.{ClearBoundInputs, ClearBoundOutputs}`                                | instance | resets the binding between runs     |
|  [12]   | `OrtValue.CreateTensorValueWithData(OrtMemoryInfo, TensorElementType, long[], ...)` | factory  | binds device memory                 |
|  [13]   | `OrtValue.CreateAllocatedTensorValue(OrtAllocator, TensorElementType, long[])`      | factory  | allocates an output tensor          |
|  [14]   | `OrtValue.GetTensorDataAsSpan<T>() -> ReadOnlySpan<T>`                              | instance | reads bound tensor data             |
|  [15]   | `OrtValue.GetTensorMutableDataAsSpan<T>() -> Span<T>`                               | instance | writes bound tensor data            |
|  [16]   | `OrtValue.GetTensorDataAsTensorSpan<T>() -> ReadOnlyTensorSpan<T>`                  | instance | reads as a numerics tensor span     |
|  [17]   | `OrtValue.GetTensorMutableDataAsTensorSpan<T>() -> TensorSpan<T>`                   | instance | writes as a numerics tensor span    |
|  [18]   | `OrtValue.GetTensorMutableRawData() -> Span<byte>`                                  | instance | reads raw tensor bytes              |
|  [19]   | `OrtValue.GetTensorSizeInBytes()`                                                   | instance | tensor buffer byte count            |
|  [20]   | `OrtValue.GetTensorTypeAndShape() -> OrtTensorTypeAndShapeInfo`                     | instance | element type and shape              |
|  [21]   | `OrtValue.GetTensorMemoryInfo() -> OrtMemoryInfo`                                   | instance | where the buffer lives              |
|  [22]   | `OrtMemoryInfo.DefaultInstance`                                                     | static   | shared CPU descriptor               |
|  [23]   | `OrtMemoryInfo(string, OrtAllocatorType, int, OrtMemType)`                          | ctor     | device descriptor                   |
|  [24]   | `OrtMemoryInfo(string, OrtMemoryInfoDeviceType, ...)`                               | ctor     | extended device descriptor          |
|  [25]   | `OrtMemoryInfo.{Name, Id}`                                                          | instance | allocator/device name and id        |
|  [26]   | `OrtMemoryInfo.GetAllocatorType() -> OrtAllocatorType`                              | instance | arena/device allocator classifier   |
|  [27]   | `OrtMemoryInfo.GetMemoryType() -> OrtMemType`                                       | instance | legacy CPU/output mem-type axis     |
|  [28]   | `OrtMemoryInfo.GetDeviceMemoryType() -> OrtDeviceMemoryType`                        | instance | V2 device-memory class              |
|  [29]   | `OrtMemoryInfo.GetVendorId() -> uint`                                               | instance | device vendor id                    |

- `CreateTensorValueWithData` takes a trailing `(nint dataPtr, long sizeBytes)`; the extended `OrtMemoryInfo` ctor takes `(string, OrtMemoryInfoDeviceType, uint vendorId, int deviceId, OrtDeviceMemoryType, ulong alignment, OrtAllocatorType)`.
- `GetTensorMemoryInfo().Name` is the arena name the `ModelRun` receipt stamps as `ArenaAllocator`; binding amortizes I/O allocation across repeated runs and is the measured hot-loop path.

[ENTRYPOINT_SCOPE]: EP-context model compilation on `OrtModelCompilationOptions`

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :----------------------------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `OrtModelCompilationOptions(SessionOptions)`                                   | ctor     | binds compile to a configured session       |
|  [02]   | `CompileModel()`                                                               | instance | runs the partition and write                |
|  [03]   | `SetInputModelPath(string)`                                                    | instance | reads the source from disk                  |
|  [04]   | `SetInputModelFromBuffer(byte[])`                                              | instance | reads the source from memory                |
|  [05]   | `SetOutputModelPath(string)`                                                   | instance | writes the compiled model to disk           |
|  [06]   | `SetOutputModelWriteDelegate(WriteBufferToDestinationDelegate)`                | instance | streams output to a caller sink             |
|  [07]   | `SetOutputModelExternalInitializersFile(string, ulong)`                        | instance | spills initializers over a byte threshold   |
|  [08]   | `SetOutputModelGetInitializerLocationDelegate(GetInitializerLocationDelegate)` | instance | routes each initializer to external storage |
|  [09]   | `SetEpContextEmbedMode(bool)`                                                  | instance | embeds the EP-context binary vs. side-file  |
|  [10]   | `SetEpContextBinaryInformation(string, string)`                                | instance | names the external EP-context output        |
|  [11]   | `SetGraphOptimizationLevel(GraphOptimizationLevel)`                            | instance | optimizes before partitioning               |
|  [12]   | `SetFlags(OrtCompileApiFlags)`                                                 | instance | compile strictness                          |
|  [13]   | `Dispose()`                                                                    | instance | releases handle and delegate state          |

- input source is exclusive (`[03]` disk / `[04]` memory) and output destination is exclusive (`[05]` disk / `[06]` sink); the EP append order on the bound session decides which provider claims each subgraph.
- `ERROR_IF_NO_NODES_COMPILED` fails when no subgraph is EP-claimed; `ERROR_IF_OUTPUT_FILE_EXISTS` fails rather than overwriting.
- delegates: `WriteBufferToDestinationDelegate(ReadOnlySpan<byte>)`; `GetInitializerLocationDelegate(string, IReadOnlyOrtValue, IReadOnlyExternalInitializerInfo) -> OrtExternalInitializerInfo`.

## [04]-[CONFIG_KEYS]

[CONFIG_KEY_SCOPE]: EP-context session keys through `SessionOptions.AddSessionConfigEntry` (`onnxruntime_session_options_config_keys.h`)

| [INDEX] | [KEY_STRING]                                         | [VALUE_DOMAIN]         | [ROLE]                                                  |
| :-----: | :--------------------------------------------------- | :--------------------- | :------------------------------------------------------ |
|  [01]   | `ep.context_enable`                                  | `0` / `1`              | reads the precompiled EP-context graph on load          |
|  [02]   | `ep.context_file_path`                               | filesystem path string | context blob location                                   |
|  [03]   | `ep.context_embed_mode`                              | `0` / `1`              | embed the EP-context binary vs. side-file               |
|  [04]   | `ep.context_node_name_prefix`                        | prefix string          | namespaces EP-context node names across fleet blobs     |
|  [05]   | `ep.context_model_external_initializers_file_name`   | filename string        | spills EP-context initializers to a side file           |
|  [06]   | `ep.enable_weightless_ep_context_nodes`              | `0` / `1`              | emits weightless context nodes                          |
|  [07]   | `ep.share_ep_contexts` / `ep.stop_share_ep_contexts` | `0` / `1`              | shares/halts one EP-context across co-resident sessions |

[CONFIG_KEY_SCOPE]: base session, arena, and quantization keys

| [INDEX] | [KEY_STRING]                                    | [VALUE_DOMAIN]     | [ROLE]                                               |
| :-----: | :---------------------------------------------- | :----------------- | :--------------------------------------------------- |
|  [01]   | `session.disable_cpu_ep_fallback`               | `0` / `1`          | forbids silent CPU fallback for an unclaimed EP node |
|  [02]   | `session.use_env_allocators`                    | `0` / `1`          | routes onto the env-registered shared allocator      |
|  [03]   | `session.use_device_allocator_for_initializers` | `0` / `1`          | loads initializers into device memory                |
|  [04]   | `session.set_denormal_as_zero`                  | `0` / `1`          | per-session subnormal flush                          |
|  [05]   | `session.disable_prepacking`                    | `0` / `1`          | disables weight prepacking                           |
|  [06]   | `session.intra_op.allow_spinning`               | `0` / `1`          | intra-op spin enable                                 |
|  [07]   | `session.intra_op.spin_duration_us`             | microseconds       | intra-op spin duration                               |
|  [08]   | `session.qdq_matmulnbits_accuracy_level`        | accuracy-level int | int4/int8 `MatMulNBits` accuracy floor               |
|  [09]   | `session.qdq_matmulnbits_block_size`            | block-size int     | NBits quantization block size                        |
|  [10]   | `session.enable_dq_matmulnbits_fusion`          | `0` / `1`          | fuses dequant + NBits matmul                         |
|  [11]   | `mlas.enable_gemm_fastmath_arm64_bfloat16`      | `0` / `1`          | arm64 bf16 GEMM fast-math                            |

[CONFIG_KEY_SCOPE]: run-options keys through `RunOptions.AddRunConfigEntry` (`onnxruntime_run_options_config_keys.h`)

| [INDEX] | [KEY_STRING]                              | [VALUE_DOMAIN]          | [ROLE]                                                    |
| :-----: | :---------------------------------------- | :---------------------- | :-------------------------------------------------------- |
|  [01]   | `memory.enable_memory_arena_shrinkage`    | allocator device string | shrinks the arena after a run (`RunConfig.Bulk` posture)  |
|  [02]   | `disable_synchronize_execution_providers` | `0` / `1`               | skips inter-EP sync for single-EP runs                    |
|  [03]   | `gpu_graph_id`                            | graph-id string         | CUDA-graph capture/replay annotation for fixed-shape runs |

[CONFIG_KEY_SCOPE]: env keys (`onnxruntime_env_config_keys.h`)

| [INDEX] | [KEY_STRING]            | [VALUE_DOMAIN] | [ROLE]                                                  |
| :-----: | :---------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `allow_virtual_devices` | `0` / `1`      | admits virtual hardware devices into autoEP enumeration |

[CONFIG_KEY_SCOPE]: CoreML provider-option keys

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

[TOPOLOGY]:
- session/policy/run roots are `InferenceSession`/`SessionOptions`/`RunOptions`; model, node, tensor, and element classifiers own metadata; native handles release through deterministic disposal.
- EP append order is fallback priority; the autoEP loop is `OrtEnv.GetEpDevices()` → device rank → `AppendExecutionProvider(env, devices, …)` → `InferenceSession.GetEpDeviceForInputs()` reading back the device per input. `SetEpSelectionPolicy`/`SetEpSelectionPolicyDelegate` drive enum or callback ranking; `OrtEnv.RegisterExecutionProviderLibrary` admits an out-of-tree EP.
- a bare `"CoreML"` faults `InvalidArgument`; the registered provider name is `"CoreMLExecutionProvider"`.
- warm-start admissibility is a two-step enum contract: `GetCompatibilityInfoFromModel(modelPath, epType) -> string`, then `GetModelCompatibilityForEpDevices(devices, info) -> OrtCompiledModelCompatibility`; branch on the enum, never a substring.
- `EP_UNSUPPORTED`/`EP_SUPPORTED_PREFER_RECOMPILATION` force a fresh compile clearing the `ep.context_*` keys, `EP_SUPPORTED_OPTIMAL` keeps the warm-start read, `EP_NOT_APPLICABLE` means the device is not EP-context-aware; a rejected device populates a receipt through `GetHardwareDeviceEpIncompatibilityDetails`.
- IO binding amortizes input and output allocation across repeated runs and is the measured hot-loop path; `SynchronizeBoundInputs`/`SynchronizeBoundOutputs` flush device transfers around the bound run.
- EP-context compilation embeds or side-files a compiled model so a later session loads the precompiled graph, activated through the `ep.context_*` session keys.

[STACKING]:
- `api-onnxruntimegenai`(`.api/api-onnxruntimegenai.md`): the genai `Config.AppendProvider`/`SetProviderOption`/`SetDecoderProviderOptionsHardware*` surface selects from this EP roster and binds to devices this page's `OrtEpDevice`/`OrtHardwareDevice` discovery enumerates; genai native co-locates per RID beside this runtime.
- boot rail: `OrtThreadingOptions` reads the AppHost `CpuBudget`, folds into `EnvironmentCreationOptions`, and `OrtEnv.CreateInstanceWithOptions(ref)` boots once behind `OrtEnv.IsCreated`; `DisableTelemetryEvents` runs at boot because the telemetry spine owns signals.
- session rail: `SessionOptions` config keys, EP register, and `OrtModelCompilationOptions` compile fold into one `Open` keyed on `ResidentKey(ModelIdentity.Checksum, ModelFingerprint.Of(SessionPolicy.SessionRows(ep)))`; the compiled context blob content-addresses through a device-aware `ContextKey` over `OrtEpDevice` `EpName`/`VendorId`/`DeviceId`/`HardwareDevice.Type`, crossing to the Persistence blob lane as one `ArtifactIndexRow`.
- provider rail: the `ExecutionProvider` `[SmartEnum<string>]` (Thinktecture) carries each EP's option-table/`ExecutionProviderDevicePolicy`/`OrtHardwareDeviceType`-affinity columns as one polymorphic `Register`, and the two-step compatibility enum verdict is read once and consumed into the warm-start branch.
- run rail: `OrtValue` carriers admit through a `[Union]` `RunInput`, `OrtIoBinding` amortizes the loop over a `CreateSharedAllocator` arena, `RunOptions.Terminate` latches off the AppHost `CancelScope`, `System.Numerics.Tensors.TensorPrimitives` owns reductions, projections land in a LanguageExt `Fin<T>` inside a native-disposal bracket, and the deterministic result keys through `Microsoft.Extensions.Caching.Hybrid` stamped with `GetVersionString()`.
- time rail: every receipt carries `NodaTime` `Instant`/`Duration`; profiling chrome-trace (`EndProfiling` + `ProfilingStartTimeNs`) lands as an `ArtifactIndexRow`.

[LOCAL_ADMISSION]:
- Compute model execution enters through ONNX Runtime sessions and typed value binding.
- Model load, input binding, run policy, output projection, and disposal each emit receipts.
- Provider selection is policy data and never hides inside model-call helpers.
- Custom operators enter through declared session options and asset evidence.

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime`
- Owns: ONNX session execution, EP-context compilation, autoEP device discovery, and native runtime assets
- Accept: measured model inference and EP-context warm-start/fleet-compile
- Reject: the on-device-training surface (`TrainingSession`, `CheckpointState`, `TrainingUtils`, `OrtTrainingApi`, `LRScheduler`); the ML.NET training pipeline; the `NamedOnnxValue`/`DisposableNamedOnnxValue`/`FixedBufferOnnxValue` value carriers, which the OrtValue-only law supersedes

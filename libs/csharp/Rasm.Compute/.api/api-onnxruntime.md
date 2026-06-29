# [RASM_COMPUTE_API_ONNXRUNTIME]

`Microsoft.ML.OnnxRuntime` supplies model session execution, native runtime
assets, tensor value binding, run options, metadata inspection, custom operators,
EP-context model compilation, autoEP hardware-device discovery, and
execution-provider selection for Compute model rails. The managed surface stacks
into the `Model/*` design owners as ONE rail: `Boot` folds `OrtThreadingOptions`
into `EnvironmentCreationOptions`, `Open` folds `SessionOptions` config-entry keys
+ `OrtModelCompilationOptions` EP-context compile into the resident-session map,
`RunOps` folds `OrtValue` carriers + `OrtIoBinding` bound loops under a
`RunOptions.Terminate` deadline latch into a `Fin` projection, and the autoEP
`OrtEnv.GetEpDevices`/`GetModelCompatibilityForEpDevices`/`CreateSharedAllocator`
trio drives the `ExecutionProvider` device-rank fold and shared-arena lease — every
native handle is `SafeHandle`/`IDisposable` and releases through deterministic
disposal, so the design composes capability rather than re-marshalling the C API.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime`
- package: `Microsoft.ML.OnnxRuntime` 1.27.0 (meta-package: native runtimes + props/targets + headers; declares dependency `Microsoft.ML.OnnxRuntime.Managed 1.27.0`)
- assembly: `Microsoft.ML.OnnxRuntime.Managed` 1.27.0 — multi-targets `net8.0` / `netstandard2.0` / `net9.0-{android35,ios18,maccatalyst18}`; a `net10.0` non-mobile consumer binds the `lib/net8.0/Microsoft.ML.OnnxRuntime.dll` asset (no plain `net9.0` lib ships, and the `net9.0-*` assets are mobile-platform TFMs), so the surface below is decompile-verified against the consumer-bound `net8.0` assembly
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

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]  | [CAPABILITY]                              |
| :-----: | :-------------------------- | :-------------- | :---------------------------------------- |
|  [01]   | `ModelMetadata`             | model metadata  | `long Version`, `string Description`, `Domain`, `GraphName`, `GraphDescription`, `ProducerName`, `Dictionary<string,string> CustomMetadataMap` — `CustomMetadataMap` feeds model-identity fingerprinting |
|  [02]   | `NodeMetadata`              | node metadata   | `ElementDataType` (`TensorElementType`), `int[] Dimensions`, `string[] SymbolicDimensions`, `OnnxValueType`, `bool IsTensor`/`IsString`, `Type ElementType`, `AsMapMetadata()`/`AsSequenceMetadata()`/`AsOptionalMetadata()` |
|  [03]   | `OrtTensorTypeAndShapeInfo` | tensor metadata | `ElementCount`/element type/shape; sizes output buffers, never re-multiplied dims |
|  [04]   | `DenseTensor<T>`            | tensor value    | `Microsoft.ML.OnnxRuntime.Tensors` dense carrier; `ArrayTensorExtensions`/`ShapeUtils` helpers |
|  [05]   | `OrtIoBinding`              | binding root    | binds inputs and outputs (see IO-binding entrypoints)                  |
|  [06]   | `PrePackedWeightsContainer` | weights cache   | `SafeHandle`; shares prepacked weights across sessions of the same model (`InferenceSession` ctor 3rd arg) |
|  [07]   | `FixedBufferOnnxValue`      | buffer value    | binds fixed managed memory — superseded by `OrtValue` in the design (`OrtValue`-only law) |
|  [08]   | `OrtLoraAdapter`            | adapter handle  | `static Create(string adapterPath, OrtAllocator)` / `Create(byte[] bytes, OrtAllocator)`; `SafeHandle` — both overloads REQUIRE an `OrtAllocator` |
|  [09]   | `OrtMemoryAllocation`       | device buffer   | `SafeHandle`; owns a device allocation from `OrtAllocator.Allocate(uint)` and binds via the device `BindInput/BindOutput` overload |
|  [10]   | `OrtAllocatorType`          | allocator enum  | `DeviceAllocator`, `ArenaAllocator`       |
|  [11]   | `OnnxValueType`             | value-kind enum | `ONNX_TYPE_TENSOR`/`_SEQUENCE`/`_MAP`/`_OPTIONAL`/`_SPARSETENSOR` — the kind `OrtValue.OnnxType` and `NodeMetadata.OnnxValueType` report |
|  [12]   | `IReadOnlyOrtValue`         | read view iface | read-only `OrtValue` projection consumed by `GetInitializerLocationDelegate` |
|  [13]   | `OrtTypeInfo`               | type descriptor | `OrtValue.GetTypeInfo()` — tensor/sequence/map/optional shape introspection on a live value |
|  [14]   | `SessionOptionsContainer`   | config registry | static named-configuration registry: `Register(Action<SessionOptions>)` / `Register(name, handler)`, `Create(name)`, `ApplyConfiguration(name)`, `Reset()` — reusable session-config profiles |

[PUBLIC_TYPE_SCOPE]: environment, threading, and provider-policy contracts
- rail: model

| [INDEX] | [SYMBOL]                          | [PACKAGE_ROLE]        | [CAPABILITY]                                                                                                                                                                                                                                 |
| :-----: | :-------------------------------- | :-------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `EnvironmentCreationOptions`      | boot options struct   | `string logId`, `OrtLoggingLevel? logLevel` (nullable), `OrtThreadingOptions threadOptions`, `nint? loggingParam`, `DOrtLoggingFunction loggingFunction` — passed `ref` to `OrtEnv.CreateInstanceWithOptions`                                |
|  [02]   | `OrtThreadingOptions`             | global thread pool    | `int GlobalIntraOpNumThreads`, `int GlobalInterOpNumThreads`, `bool GlobalSpinControl` (full read/write props), `SetGlobalDenormalAsZero()`; `SafeHandle`, parameterless ctor                                                                |
|  [03]   | `ExecutionProviderDevicePolicy`   | device-policy enum    | `DEFAULT`, `PREFER_CPU`, `PREFER_NPU`, `PREFER_GPU`, `MAX_PERFORMANCE`, `MAX_EFFICIENCY`, `MIN_OVERALL_POWER`                                                                                                                                |
|  [04]   | `GraphOptimizationLevel`          | optimization enum     | `ORT_DISABLE_ALL`=0, `ORT_ENABLE_BASIC`=1, `ORT_ENABLE_EXTENDED`=2, `ORT_ENABLE_LAYOUT`=3, `ORT_ENABLE_ALL`=99                                                                                                                               |
|  [05]   | `OrtLoggingLevel`                 | logging enum          | boot log severity                                                                                                                                                                                                                            |
|  [06]   | `OrtAllocator`                    | allocator handle      | `static DefaultInstance`, `OrtMemoryInfo Info`, `OrtMemoryAllocation Allocate(uint size)`, ctor `(InferenceSession, OrtMemoryInfo)`; `SafeHandle`                                                                                            |
|  [07]   | `OrtArenaCfg`                     | arena-config handle   | ctor `(uint maxMemory, int arenaExtendStrategy, int initialChunkSizeBytes, int maxDeadBytesPerChunk)` — tunes the BFC arena for `OrtEnv.CreateAndRegisterAllocator`                                                                          |
|  [08]   | `OrtExternalAllocation`           | external-buffer value | wraps a caller-owned device buffer for `OrtIoBinding.BindInput/BindOutput(string, OrtExternalAllocation)`                                                                                                                                     |
|  [09]   | `IDisposableReadOnlyCollection<T>`| result collection      | `IReadOnlyList<T>`+`IDisposable` native result set returned by `Run`/`GetOutputValues`                                                                                                                                                       |
|  [10]   | `BFloat16` / `Float16`            | readonly structs      | CLR carriers for the bfloat16 / float16 element types; `IComparable`/`IEquatable`                                                                                                                                                            |
|  [11]   | `OrtEpDevice`                     | EP device descriptor  | `EpName`, `EpVendor`, `EpMetadata`, `EpOptions` (all `OrtKeyValuePairs`), `HardwareDevice`, `GetMemoryInfo(OrtDeviceMemoryType)`, `CreateSyncStream(IReadOnlyDictionary<string,string>)`                                                      |
|  [12]   | `OrtHardwareDevice`               | hardware descriptor   | `Type` (`OrtHardwareDeviceType`), `VendorId` (uint), `Vendor` (string), `DeviceId` (uint), `Metadata` (`OrtKeyValuePairs`)                                                                                                                   |
|  [13]   | `OrtHardwareDeviceType`           | device-type enum      | `CPU`, `GPU`, `NPU`                                                                                                                                                                                                                          |
|  [14]   | `OrtMemoryInfoDeviceType`         | mem-device-type enum  | `CPU`, `GPU`, `FPGA`, `NPU` — the device-type axis of the extended `OrtMemoryInfo` ctor                                                                                                                                                      |
|  [15]   | `OrtKeyValuePairs`                | kv-pairs handle       | `Entries` (`IReadOnlyDictionary<string,string>`), `Add`, `Remove`, `Refresh`; `SafeHandle`, ctor `()` / `(IReadOnlyDictionary<string,string>)`                                                                                              |
|  [16]   | `OrtDeviceMemoryType`             | device-memory enum    | `DEFAULT` (0), `HOST_ACCESSIBLE` (5)                                                                                                                                                                                                         |
|  [17]   | `OrtMemType`                      | mem-class enum        | `CpuInput` (-2), `CpuOutput`/`Cpu` (-1), `Default` (0)                                                                                                                                                                                       |
|  [18]   | `OrtSyncStream`                   | sync-stream handle    | `nint GetHandle()` — ties device stream lifetime to `OrtEpDevice.CreateSyncStream`                                                                                                                                                           |
|  [19]   | `OrtCompiledModelCompatibility`   | compat-verdict ENUM   | `EP_NOT_APPLICABLE`, `EP_SUPPORTED_OPTIMAL`, `EP_SUPPORTED_PREFER_RECOMPILATION`, `EP_UNSUPPORTED` — the warm-start branch reads THIS enum, never a `"Incompatible"` substring                                                              |
|  [20]   | `OrtDeviceEpIncompatibilityDetails`| incompat diagnostic   | `OrtDeviceEpIncompatibilityReason ReasonsBitmask`, `string Notes`, `int ErrorCode`; `IDisposable` — paired with `OrtEnv.GetHardwareDeviceEpIncompatibilityDetails`                                                                          |
|  [21]   | `OrtDeviceEpIncompatibilityReason`| incompat reason flags | `[Flags] uint`: `None`=0, `DriverIncompatible`=1, `DeviceIncompatible`=2, `MissingDependency`=4, `Unknown`=0x80000000                                                                                                                        |
|  [22]   | `CoreMLFlags`                     | CoreML flags enum     | `[Flags] uint`: `COREML_FLAG_USE_NONE`=0, `_USE_CPU_ONLY`=1, `_ENABLE_ON_SUBGRAPH`=2, `_ONLY_ENABLE_DEVICE_WITH_ANE`=4, `_ONLY_ALLOW_STATIC_INPUT_SHAPES`=8, `_CREATE_MLPROGRAM`=0x10, `_USE_CPU_AND_GPU`=0x20                              |
|  [23]   | `NnapiFlags`                      | NNAPI flags enum      | `NNAPI_FLAG_USE_NONE`=0, `_USE_FP16`=1, `_USE_NCHW`=2, `_CPU_DISABLED`=4, `_CPU_ONLY`=8                                                                                                                                                      |
|  [24]   | `OrtCUDAProviderOptions`          | CUDA options handle   | `UpdateOptions(Dictionary<string,string>)`, `GetOptions()` string; `static ProviderOptionsValueHelper.StringToDict(s, dict)` parses a serialized options string                                                                              |
|  [25]   | `OrtTensorRTProviderOptions`      | TensorRT options      | `UpdateOptions(Dictionary<string,string>)`, `GetOptions()`, `GetDeviceId()`                                                                                                                                                                  |

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

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE] | [CAPABILITY]            |
| :-----: | :------------------------------------------------ | :------------- | :---------------------- |
|  [01]   | `build/native/Microsoft.ML.OnnxRuntime.props`     | MSBuild import | declares native copy    |
|  [02]   | `build/native/Microsoft.ML.OnnxRuntime.targets`   | MSBuild import | copies native assets    |
|  [03]   | `runtimes/win-x64/native/onnxruntime.dll`         | native asset   | base runtime (win-x64) — shipped by THIS package |
|  [04]   | `runtimes/win-arm64/native/onnxruntime.dll`       | native asset   | base runtime (win-arm64) — shipped by THIS package |
|  [05]   | `runtimes/win-{x64,arm64}/native/onnxruntime_providers_shared.dll` | native asset   | shared-provider bridge for win append calls |
|  [06]   | `libonnxruntime.dylib` / `libonnxruntime.so`      | native asset   | macOS/Linux base runtime — NOT in this package's win-only `runtimes/`; supplied by the platform/feed asset the build resolves, with the CoreML EP built into the macOS dylib |
|  [07]   | `build/native/include/*.h`                        | native headers | `onnxruntime_c_api.h`, `onnxruntime_cxx_api.h`, `onnxruntime_ep_c_api.h`, `onnxruntime_lite_custom_op.h`, plus the three config-key headers (section 4) |

[PACKAGE_ASSET_SCOPE]: per-RID native ABI matrix (the canonical owner the genai lane composes)
- rail: model
- note: this is the OWNING per-RID native-payload matrix for the whole ONNX runtime native floor; `api-onnxruntimegenai` `native-rids`/`native-floor`, `api-onnxruntime-gpu`, and `api-onnxruntime-directml` restate NOTHING — they declare their own managed-floor pin and defer the RID/ABI facts here. The base meta-package ships only the `win-{x64,arm64}` payloads inline; every other RID's `libonnxruntime.{dylib,so}` resolves through the platform/feed asset the build copies. The accelerated providers ride sibling packages that supersede the base runtime on their RID: `.Gpu` (its `Gpu.Windows`/`Gpu.Linux` sub-packages on the 1.27.0 line) adds CUDA + TensorRT native on `win-x64`/`linux-x64`; `.DirectML` is a RID-GATED HOLD pinned to `.Managed 1.24.4` (only one `Microsoft.ML.OnnxRuntime.dll` binds, so DML is not co-resident on the 1.27.0 line) shipping a DML-enabled `onnxruntime.dll` + `Microsoft.AI.DirectML`'s `DirectML.dll` on `win-x64`/`win-arm64`. The genai native (`onnxruntime-genai.{dll,dylib,so}` + `.aar`/`.xcframework.zip` mobile forms) co-locates per-RID beside this base runtime, so a genai model run with no matching base-runtime RID payload faults at native init.

| [INDEX] | [RID]        | [BASE_RUNTIME_NATIVE]                                                  | [ACCELERATED_PROVIDER_NATIVE]                                          | [SHIPPED_BY]                  |
| :-----: | :----------- | :-------------------------------------------------------------------- | :-------------------------------------------------------------------- | :---------------------------- |
|  [01]   | `win-x64`    | `onnxruntime.dll` + `onnxruntime_providers_shared.dll` (+ `.lib`)     | `onnxruntime_providers_cuda.dll` / `onnxruntime_providers_tensorrt.dll` (`.Gpu.Windows`, 1.27.0); DML-enabled `onnxruntime.dll` + `DirectML.dll` (`.DirectML`, 1.24.4 HOLD) | base inline; providers sibling |
|  [02]   | `win-arm64`  | `onnxruntime.dll` + `onnxruntime_providers_shared.dll` (+ `.lib`)     | no CUDA/TensorRT (x64-only); DML-enabled `onnxruntime.dll` + `DirectML.dll` (`.DirectML`, 1.24.4 HOLD) | base inline; DML sibling      |
|  [03]   | `linux-x64`  | `libonnxruntime.so` + `libonnxruntime_providers_shared.so`            | `libonnxruntime_providers_cuda.so` / `libonnxruntime_providers_tensorrt.so` (`.Gpu.Linux`, 1.27.0) | base feed; providers sibling   |
|  [04]   | `linux-arm64`| `libonnxruntime.so` + `libonnxruntime_providers_shared.so`           | none                                                                  | base feed                     |
|  [05]   | `osx-arm64`  | `libonnxruntime.dylib` ONLY — NO separate provider native             | none; the CoreML EP is built INTO the base dylib (the verified host asset) | base feed                     |
|  [06]   | `android`/`ios` | `onnxruntime.aar` / `onnxruntime.xcframework.zip` archive form     | none (mobile EPs — NNAPI/CoreML — built into the archive)             | base feed                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: inference operations
- rail: model

| [INDEX] | [SURFACE]                       | [CALL_SHAPE]     | [CAPABILITY]             |
| :-----: | :------------------------------ | :--------------- | :----------------------- |
|  [01]   | `InferenceSession.Run`          | run call         | `IDisposableReadOnlyCollection<OrtValue> Run(RunOptions, IReadOnlyCollection<string> inputNames, IReadOnlyCollection<OrtValue> inputValues, IReadOnlyCollection<string> outputNames)` — the OrtValue-only overload the `RunOps.Infer` fold drives (plus a `IReadOnlyDictionary<string,OrtValue>` input mirror) |
|  [02]   | `InferenceSession.RunAsync`     | async run call   | `Task<IReadOnlyCollection<OrtValue>> RunAsync(RunOptions, inputNames, inputValues, outputNames, outputValues)` — REQUIRES caller-PRE-ALLOCATED output `OrtValue`s and completes on a native callback off the lane scope; the design routes async through the lane seam instead |
|  [03]   | `RunWithBinding`                | bound-run call   | `void RunWithBinding(RunOptions, OrtIoBinding)` — executes a pre-bound inference; outputs read back via `OrtIoBinding.GetOutputValues` |
|  [04]   | `RunWithBindingAndNames`        | bound-run call   | `IDisposableReadOnlyCollection<DisposableNamedOnnxValue> RunWithBindingAndNames(RunOptions, OrtIoBinding, string[] names = null)` — returns the FORBIDDEN `DisposableNamedOnnxValue` collection; the design instead zips `GetOutputNames()` against `GetOutputValues()` |
|  [05]   | `RunWithBoundResults`           | bound-run call   | `IDisposableReadOnlyCollection<OrtValue> RunWithBoundResults(RunOptions, OrtIoBinding)` — OrtValue-only bound results in one call |
|  [06]   | `EndProfiling`                  | session call     | `string EndProfiling()` closes the chrome-trace and returns its path; pairs with `ProfilingStartTimeNs` |
|  [07]   | `ProfilingStartTimeNs`          | session prop     | `ulong` profiling epoch — receipt-relative trace start                   |
|  [08]   | `CreateIoBinding`               | factory call     | `OrtIoBinding CreateIoBinding()`                       |
|  [09]   | `InputMetadata`/`OutputMetadata`/`OverridableInitializerMetadata` | session read | `IReadOnlyDictionary<string,NodeMetadata>` — model I/O shape/dtype introspection (sizes bound buffers, validates free-dim overrides) |
|  [10]   | `InputNames`/`OutputNames`      | session read     | `IReadOnlyList<string>` ordered I/O names for binding/zip                 |
|  [11]   | `ModelMetadata`                 | session read     | `ModelMetadata` — version/producer/`CustomMetadataMap` for identity      |
|  [12]   | `GetEpDeviceForInputs`          | session read     | `IReadOnlyList<OrtEpDevice>` — the autoEP device chosen per input slot, closing the autoEP loop after `AppendExecutionProvider(env, devices, …)` |
|  [13]   | `GetMemoryInfosForInputs`/`GetMemoryInfosForOutputs` | session read | `IDisposableReadOnlyCollection<OrtMemoryInfo>` — where each I/O buffer must live, driving device-residency binding decisions |
|  [14]   | `Dispose`                       | lifetime call    | releases native handles  |

[ENTRYPOINT_SCOPE]: execution-provider append operations
- rail: model
- note: all are instance methods on `SessionOptions`

| [INDEX] | [SURFACE]                                                                                                                  | [CALL_SHAPE] | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------------------------------------------------------------- | :----------- | :-------------------------------------------- |
|  [01]   | `AppendExecutionProvider_CPU(int useArena = 1)`                                                                            | option call  | CPU EP; `useArena` 1 enables memory arena     |
|  [02]   | `AppendExecutionProvider_CUDA(int deviceId = 0)`                                                                           | option call  | CUDA EP by device index                       |
|  [03]   | `AppendExecutionProvider_CUDA(OrtCUDAProviderOptions)`                                                                     | option call  | CUDA EP with full provider-options struct     |
|  [04]   | `AppendExecutionProvider_DML(int deviceId = 0)`                                                                            | option call  | DirectML EP by device index                   |
|  [05]   | `AppendExecutionProvider_Tensorrt(int deviceId = 0)`                                                                       | option call  | TensorRT EP by device index                   |
|  [06]   | `AppendExecutionProvider_Tensorrt(OrtTensorRTProviderOptions)`                                                             | option call  | TensorRT EP with full provider-options struct |
|  [07]   | `AppendExecutionProvider_ROCm(int deviceId = 0)`                                                                           | option call  | ROCm EP by device index                       |
|  [08]   | `AppendExecutionProvider_ROCm(OrtROCMProviderOptions)`                                                                     | option call  | ROCm EP with full provider-options struct     |
|  [09]   | `AppendExecutionProvider_CoreML(CoreMLFlags coremlFlags = CoreMLFlags.COREML_FLAG_USE_NONE)`                               | option call  | CoreML EP with compute-unit flags             |
|  [10]   | `AppendExecutionProvider_OpenVINO(string deviceId = "")`                                                                   | option call  | OpenVINO EP by device string                  |
|  [11]   | `AppendExecutionProvider_MIGraphX(int deviceId = 0)`                                                                       | option call  | MIGraphX EP by device index                   |
|  [12]   | `AppendExecutionProvider_Nnapi(NnapiFlags nnapiFlags = NnapiFlags.NNAPI_FLAG_USE_NONE)`                                    | option call  | NNAPI EP with accelerator-mode flags          |
|  [13]   | `AppendExecutionProvider_Dnnl(int useArena = 1)`                                                                           | option call  | DNNL EP; `useArena` mirrors CPU EP semantics  |
|  [14]   | `AppendExecutionProvider(string providerName, Dictionary<string, string> providerOptions = null)`                          | option call  | generic EP by name and key-value options      |
|  [15]   | `AppendExecutionProvider(OrtEnv env, IReadOnlyList<OrtEpDevice> epDevices, IReadOnlyDictionary<string, string> epOptions)` | option call  | EP from autoEP device list and options        |

[ENTRYPOINT_SCOPE]: value and provider operations
- rail: model

| [INDEX] | [SURFACE]                                                                   | [CALL_SHAPE] | [CAPABILITY]                              |
| :-----: | :-------------------------------------------------------------------------- | :----------- | :---------------------------------------- |
|  [01]   | `OrtValue.CreateTensorValueFromMemory<T>(T[], long[])`                      | factory call | binds managed array as tensor             |
|  [02]   | `OrtValue.CreateTensorValueFromMemory<T>(OrtMemoryInfo, Memory<T>, long[])` | factory call | binds device-pinned memory as tensor      |
|  [03]   | `OrtValue.CreateTensorValueFromSystemNumericsTensorObject<T>(Tensor<T>)`    | factory call | binds `System.Numerics.Tensors.Tensor<T>` |
|  [04]   | `OrtValue.CreateFromStringTensor(Microsoft.ML.OnnxRuntime.Tensors.Tensor<string>)` | factory call | binds the ONNX-owned `Tensor<string>` — distinct from `[03]`'s `System.Numerics.Tensors.Tensor<T>` |
|  [05]   | `NamedOnnxValue.CreateFromTensor<T>(string, Tensor<T>)`                     | factory call | creates named value from tensor (superseded path — OrtValue-only law) |
|  [06]   | `RegisterCustomOpLibrary`                                                   | option call  | `void RegisterCustomOpLibrary(string)` loads custom operators (no handle — ORT-managed lifetime, the leak-free spelling) |
|  [07]   | `RegisterCustomOpLibraryV2`                                                 | option call  | `void RegisterCustomOpLibraryV2(string, out nint)` — legacy caller-owns-handle path; a discarded `out _` handle leaks the library, so prefer `[06]` |
|  [08]   | `RegisterOrtExtensions`                                                     | option call  | loads extension ops                       |
|  [09]   | `SessionOptions.EnableMemoryPattern` / `EnableProfiling` / `EnableCpuMemArena` | option property | `bool` toggles (memory reuse / profiling / CPU arena) — assigned, not called |
|  [10]   | `RunOptions.AddRunConfigEntry`                                              | run call     | sets run config entry                     |

[ENTRYPOINT_SCOPE]: environment, session-policy, and run-policy operations
- rail: model

Provider names include `CoreMLExecutionProvider` and `CPUExecutionProvider`; threading options carry typed global thread and spin settings.

[ENVIRONMENT_OPERATIONS]:

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]    | [CAPABILITY]                                                                           |
| :-----: | :---------------------------------------------- | :-------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `OrtEnv.IsCreated`                              | static property | reports environment creation                                                           |
|  [02]   | `OrtEnv.Instance()`                             | static call     | returns the singleton environment                                                      |
|  [03]   | `OrtEnv.CreateInstanceWithOptions(ref opts)`    | static call     | boots with `EnvironmentCreationOptions` (passed `ref`)                                  |
|  [04]   | `OrtEnv.DisableTelemetryEvents` / `EnableTelemetryEvents` | instance call   | silences / re-enables runtime telemetry                                                |
|  [05]   | `OrtEnv.GetVersionString`                       | instance call   | reports the native runtime version (stamps the deterministic result-cache key)         |
|  [06]   | `OrtEnv.GetAvailableProviders`                  | instance call   | `string[]` registered provider names                                                  |
|  [07]   | `OrtEnv.GetEpDevices`                           | instance call   | `IReadOnlyList<OrtEpDevice>` — enumerates autoEP-available devices                     |
|  [08]   | `OrtEnv.GetHardwareDevices` / `GetNumHardwareDevices` | instance call   | `IReadOnlyList<OrtHardwareDevice>` / `int` raw hardware enumeration beneath the EP devices |
|  [09]   | `OrtEnv.GetCompatibilityInfoFromModel`          | instance call   | `string GetCompatibilityInfoFromModel(string modelPath, string epType)` — produces the compat-info STRING a later `GetModelCompatibilityForEpDevices` consumes; `GetCompatibilityInfoFromModelBytes(byte[], epType)` is the in-memory mirror |
|  [10]   | `OrtEnv.GetModelCompatibilityForEpDevices`      | instance call   | `OrtCompiledModelCompatibility GetModelCompatibilityForEpDevices(IReadOnlyList<OrtEpDevice> epDevices, string compatibilityInfo)` — the 2nd arg is the compat-info STRING from `GetCompatibilityInfoFromModel`, NOT a model path; returns the `EP_*` ENUM the warm-start branch reads |
|  [11]   | `OrtEnv.GetHardwareDeviceEpIncompatibilityDetails` | instance call   | `OrtDeviceEpIncompatibilityDetails GetHardwareDeviceEpIncompatibilityDetails(string epName, OrtHardwareDevice)` — driver/device/dependency reason bitmask + notes when a device is rejected |
|  [12]   | `OrtEnv.CreateSharedAllocator`                  | instance call   | `(OrtEpDevice, OrtDeviceMemoryType, OrtAllocatorType, IReadOnlyDictionary<string,string> allocatorOptions)` returns `OrtAllocator` |
|  [13]   | `OrtEnv.GetSharedAllocator`                     | instance call   | `OrtAllocator GetSharedAllocator(OrtMemoryInfo)` — reads back the shared allocator for a memory descriptor |
|  [14]   | `OrtEnv.ReleaseSharedAllocator`                 | instance call   | releases a shared allocator for a device and memory type                               |
|  [15]   | `OrtEnv.CreateAndRegisterAllocator`             | instance call   | `(OrtMemoryInfo, OrtArenaCfg)` / `(string providerType, OrtMemoryInfo, OrtArenaCfg, IReadOnlyDictionary<string,string> provider_options)` registers a BFC-arena allocator with `UnregisterAllocator(OrtMemoryInfo)` the retract arm |
|  [16]   | `OrtEnv.RegisterExecutionProviderLibrary`       | instance call   | `(string registrationName, string libraryPath)` registers an out-of-tree EP shared library for autoEP discovery; `UnregisterExecutionProviderLibrary(name)` the retract arm |
|  [17]   | `OrtEnv.CopyTensors`                            | instance call   | `(IReadOnlyList<OrtValue> src, IReadOnlyList<OrtValue> dst, OrtSyncStream)` device-to-device tensor copy over a sync stream |
|  [18]   | `OrtEnv.EnvLogLevel`                            | instance prop   | live env log-severity floor                                                            |

[SESSION_POLICY_OPERATIONS]:

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]    | [CAPABILITY]                                                                                  |
| :-----: | :----------------------------------------------------- | :-------------- | :-------------------------------------------------------------------------------------------- |
|  [01]   | `SessionOptions.AddSessionConfigEntry`                 | option call     | sets string config entry                                                                      |
|  [02]   | `SessionOptions.AddFreeDimensionOverrideByName`        | option call     | binds symbolic dimension value                                                                |
|  [03]   | `SessionOptions.AddFreeDimensionOverride`              | option call     | binds dimension by denotation string                                                          |
|  [04]   | `SessionOptions.AddInitializer`                        | option call     | `(string, OrtValue)` injects a pre-loaded initializer                                         |
|  [05]   | `SessionOptions.DisablePerSessionThreads`              | option call     | routes sessions onto the global pool                                                          |
|  [06]   | `SessionOptions.SetEpSelectionPolicy`                  | option call     | applies `ExecutionProviderDevicePolicy` enum to session                                       |
|  [07]   | `SessionOptions.SetEpSelectionPolicyDelegate`          | option call     | `(EpSelectionDelegate)` applies a custom device-rank fn                                       |
|  [08]   | `SessionOptions.EnableProfiling`                       | option property | enables chrome-trace profiling                                                                |
|  [09]   | `SessionOptions.ProfileOutputPathPrefix`               | option property | sets profile artifact prefix                                                                  |
|  [10]   | `SessionOptions.GraphOptimizationLevel`                | option property | sets graph optimization level                                                                 |
|  [11]   | `SessionOptions.OptimizedModelFilePath`                | option property | writes the post-optimization graph to disk — a managed warm-start path beside the `ep.context_*` EP-context blob |
|  [12]   | `SessionOptions.ExecutionMode`                         | option property | `ORT_SEQUENTIAL` or `ORT_PARALLEL` execution                                                  |
|  [13]   | `SessionOptions.EnableCpuMemArena`                     | option property | toggles the CPU BFC arena (mirrors the `AppendExecutionProvider_CPU(useArena)` switch at session scope) |
|  [14]   | `SessionOptions.IntraOpNumThreads`                     | option property | per-session intra-op thread count                                                             |
|  [15]   | `SessionOptions.InterOpNumThreads`                     | option property | per-session inter-op thread count                                                             |
|  [16]   | `SessionOptions.LogId` / `LogSeverityLevel` / `LogVerbosityLevel` | option property | per-session log id, severity floor, and verbosity                                  |
|  [17]   | `SessionOptions.SetLoadCancellationFlag`               | option call     | `(bool)` — cooperatively aborts an in-flight model load, the load-time counterpart to `RunOptions.Terminate`, so a deadline-bound `Open` cancels a slow compile/load rather than blocking |
|  [18]   | `SessionOptions.MakeSessionOptionWithCudaProvider`     | static factory  | `(int deviceId)` or `(OrtCUDAProviderOptions)` shorthand                                      |
|  [19]   | `SessionOptions.MakeSessionOptionWithTensorrtProvider` | static factory  | `(int)` or `(OrtTensorRTProviderOptions)` shorthand                                           |
|  [20]   | `SessionOptions.MakeSessionOptionWithRocmProvider`     | static factory  | `(int)` or `(OrtROCMProviderOptions)` shorthand                                               |
|  [21]   | `SessionOptions.EpSelectionDelegate`                   | delegate type   | `(IReadOnlyList<OrtEpDevice>, OrtKeyValuePairs, OrtKeyValuePairs, uint) -> List<OrtEpDevice>` |
|  [22]   | `SessionOptionsContainer.Register` / `Create` / `ApplyConfiguration` / `Reset` | static rail | named-configuration registry — `Register(name, Action<SessionOptions>)` stores a reusable profile, `Create(name)` / `options.ApplyConfiguration(name)` applies it; collapses repeated session-config construction into one named row rather than re-folding the config keys per open |

[VALUE_RUN_OPERATIONS]:

| [INDEX] | [SURFACE]                               | [CALL_SHAPE] | [CAPABILITY]                     |
| :-----: | :-------------------------------------- | :----------- | :------------------------------- |
|  [01]   | `OrtValue.CreateTensorWithEmptyStrings` | factory call | `(OrtAllocator, long[] shape)` allocates string output slots         |
|  [02]   | `OrtValue.GetStringElement(index)`      | egress read  | `string` reads one string-tensor element; `GetStringElementAsSpan`/`GetStringElementAsMemory` are the zero-copy mirrors |
|  [03]   | `OrtValue.GetStringTensorAsArray()`     | egress read  | `string[]` bulk string-tensor read — one call instead of element-wise iteration |
|  [04]   | `OrtValue.StringTensorSetElementAt`     | ingress write| `(ReadOnlySpan<char>/<byte>/ReadOnlyMemory<char>, int index)` writes one element into an empty-string slot |
|  [05]   | `OrtValue.CreateSequence` / `CreateMap*`| factory call | `CreateSequence(ICollection<OrtValue>)`, `CreateMap`/`CreateMapWithStringKeys`/`CreateMapWithStringValues` build sequence/ZipMap outputs |
|  [06]   | `OrtValue.ProcessSequence` / `ProcessMap` | visitor call | `(visitor, OrtAllocator)` folds a sequence/map output (e.g. classifier ZipMap) without materializing a managed collection; `GetValue(int, OrtAllocator)` / `GetValueCount()` index it |
|  [07]   | `OrtValue.OnnxType` / `IsTensor` / `IsSparseTensor` / `GetTypeInfo()` | value read | discriminates the `OnnxValueType` of a live output before projection |
|  [08]   | `RunOptions.Terminate`                  | run property | one-way cancellation latch       |
|  [09]   | `RunOptions.AddActiveLoraAdapter`       | run call     | attaches `OrtLoraAdapter` for a run |
|  [10]   | `OrtLoraAdapter.Create`                 | factory call | `Create(string adapterPath, OrtAllocator)` / `Create(byte[] bytes, OrtAllocator)` — BOTH overloads require an `OrtAllocator` (e.g. `OrtAllocator.DefaultInstance`) |

[METADATA_THREADING_OPERATIONS]:

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE]       | [CAPABILITY]                 |
| :-----: | :-------------------------------------------- | :----------------- | :--------------------------- |
|  [01]   | `ModelMetadata.Version` / `ProducerName` / `Domain` / `GraphName` / `GraphDescription` / `Description` | metadata property | graph version + provenance strings |
|  [02]   | `ModelMetadata.CustomMetadataMap`             | metadata property  | `Dictionary<string,string>` author-stamped key-values — folds into the model-identity fingerprint |
|  [03]   | `NodeMetadata.ElementDataType`                | metadata property  | tensor slot element type (`TensorElementType`) |
|  [04]   | `NodeMetadata.Dimensions`                     | metadata property  | `int[]` fixed dimensions (-1 = symbolic) |
|  [05]   | `NodeMetadata.SymbolicDimensions`             | metadata property  | `string[]` free-dimension names — pair with `AddFreeDimensionOverrideByName` |
|  [06]   | `NodeMetadata.OnnxValueType` / `IsTensor` / `IsString` / `ElementType` | metadata property | value kind + CLR element type of the slot |
|  [07]   | `NodeMetadata.AsSequenceMetadata` / `AsMapMetadata` / `AsOptionalMetadata` | metadata call | descends into sequence/map/optional slot shapes |
|  [08]   | `OrtThreadingOptions.GlobalIntraOpNumThreads` | read/write property | global intra-op thread count |
|  [09]   | `OrtThreadingOptions.GlobalInterOpNumThreads` | read/write property | global inter-op thread count |
|  [10]   | `OrtThreadingOptions.GlobalSpinControl`       | read/write property | spin versus low-CPU policy   |
|  [11]   | `OrtThreadingOptions.SetGlobalDenormalAsZero` | option call        | flushes subnormal floats to zero on the global pool — latency/throughput knob |
|  [12]   | `OrtThreadingOptions`                         | parameterless ctor | thread-pool options handle (`SafeHandle`) |

[ENTRYPOINT_SCOPE]: IO-binding bound-inference operations
- rail: model

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]    | [CAPABILITY]                                                                                                                                        |
| :-----: | :--------------------------------------------- | :-------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `InferenceSession.CreateIoBinding`             | factory call    | returns an `OrtIoBinding` bound to the session                                                                                                      |
|  [02]   | `InferenceSession.RunWithBinding`              | bound-run call  | `(RunOptions, OrtIoBinding)` executes a pre-bound inference                                                                                         |
|  [03]   | `OrtIoBinding.BindInput`                       | bind call       | `(string, OrtValue)` binds a named input value                                                                                                      |
|  [04]   | `OrtIoBinding.BindInput`                       | bind call       | `(string, TensorElementType, long[], OrtMemoryAllocation)` binds device input                                                                       |
|  [05]   | `OrtIoBinding.BindInput`                       | bind call       | `(string, OrtExternalAllocation)` binds a caller-owned external device buffer                                                                       |
|  [06]   | `OrtIoBinding.BindOutput`                      | bind call       | `(string, OrtValue)` binds a named output value                                                                                                     |
|  [07]   | `OrtIoBinding.BindOutput`                      | bind call       | `(string, TensorElementType, long[], OrtMemoryAllocation)` binds device output                                                                      |
|  [08]   | `OrtIoBinding.BindOutput`                      | bind call       | `(string, OrtExternalAllocation)` binds a caller-owned external output buffer                                                                       |
|  [09]   | `OrtIoBinding.BindOutputToDevice`              | bind call       | `(string, OrtMemoryInfo)` binds an output to a device allocator                                                                                     |
|  [08]   | `OrtIoBinding.SynchronizeBoundInputs`          | sync call       | flushes pending bound input transfers                                                                                                               |
|  [09]   | `OrtIoBinding.SynchronizeBoundOutputs`         | sync call       | flushes pending bound output transfers                                                                                                              |
|  [10]   | `OrtIoBinding.GetOutputNames`                  | binding read    | `string[]` returns bound output names                                                                                                               |
|  [11]   | `OrtIoBinding.GetOutputValues`                 | binding read    | `IDisposableReadOnlyCollection<OrtValue>` returns bound output values                                                                               |
|  [12]   | `OrtIoBinding.ClearBoundInputs`                | reset call      | clears all bound inputs                                                                                                                             |
|  [13]   | `OrtIoBinding.ClearBoundOutputs`               | reset call      | clears all bound outputs                                                                                                                            |
|  [14]   | `OrtValue.CreateTensorValueWithData`           | factory call    | `(OrtMemoryInfo, TensorElementType, long[], nint, long)` binds device memory                                                                        |
|  [15]   | `OrtValue.CreateAllocatedTensorValue`          | factory call    | `(OrtAllocator, TensorElementType, long[])` allocates an output tensor                                                                              |
|  [16]   | `OrtValue.GetTensorDataAsSpan<T>`              | span read       | `ReadOnlySpan<T>` reads bound tensor data                                                                                                           |
|  [17]   | `OrtValue.GetTensorMutableDataAsSpan<T>`       | span write      | `Span<T>` writes bound tensor data                                                                                                                  |
|  [18]   | `OrtValue.GetTensorDataAsTensorSpan<T>`        | span read       | `ReadOnlyTensorSpan<T>` reads tensor data as `System.Numerics.Tensors` span                                                                         |
|  [19]   | `OrtValue.GetTensorMutableDataAsTensorSpan<T>` | span write      | `TensorSpan<T>` writes tensor data as `System.Numerics.Tensors` span                                                                                |
|  [20]   | `OrtValue.GetTensorMutableRawData`             | raw read        | `Span<byte>` reads raw tensor bytes                                                                                                                 |
|  [21]   | `OrtValue.GetTensorSizeInBytes`                | metadata read   | returns total byte count for the tensor buffer                                                                                                      |
|  [22]   | `OrtValue.GetTensorTypeAndShape`               | metadata read   | returns `OrtTensorTypeAndShapeInfo` describing element type and shape                                                                               |
|  [23]   | `OrtValue.GetTensorMemoryInfo`                 | metadata read   | returns `OrtMemoryInfo` for where the tensor buffer lives                                                                                           |
|  [24]   | `OrtMemoryInfo.DefaultInstance`                | static property | shared CPU `OrtMemoryInfo` for default-device binding                                                                                               |
|  [25]   | `OrtMemoryInfo`                                | ctor            | `(string, OrtAllocatorType, int, OrtMemType)` builds a device descriptor                                                                            |
|  [26]   | `OrtMemoryInfo`                                | ctor            | `(string, OrtMemoryInfoDeviceType, uint vendorId, int deviceId, OrtDeviceMemoryType, ulong alignment, OrtAllocatorType)` extended device descriptor |
|  [27]   | `OrtMemoryInfo.Name`                           | accessor read   | `string Name { get; }` — allocator/device name; the `GetTensorMemoryInfo().Name` arena name the `ModelRun` receipt stamps as `ArenaAllocator`        |
|  [28]   | `OrtMemoryInfo.Id`                             | accessor read   | `int Id { get; }` — device id the descriptor was created against                                                                                   |
|  [29]   | `OrtMemoryInfo.GetAllocatorType`               | accessor read   | `OrtAllocatorType GetAllocatorType()` — arena/device allocator classifier of the descriptor                                                        |
|  [30]   | `OrtMemoryInfo.GetMemoryType`                  | accessor read   | `OrtMemType GetMemoryType()` — the legacy CPU/output mem-type axis (distinct from the V2 device-memory class)                                       |
|  [31]   | `OrtMemoryInfo.GetDeviceMemoryType`            | accessor read   | `OrtDeviceMemoryType GetDeviceMemoryType()` — the V2 `DEFAULT`/`HOST_ACCESSIBLE` device-memory class                                                |
|  [32]   | `OrtMemoryInfo.GetVendorId`                    | accessor read   | `uint GetVendorId()` — device vendor id; the read-side mirror of the [26] extended ctor                                                            |

## [04]-[CONFIG_KEYS]

[CONFIG_KEY_SCOPE]: session-options config-entry keys (decompile/header-verified, set through `SessionOptions.AddSessionConfigEntry`)
- rail: model
- source: `build/native/include/onnxruntime_session_options_config_keys.h` (1.27.0)

EP-context (warm-start / fleet-compiled context) family — the `Open`/`Compile` folds drive these:

| [INDEX] | [KEY_STRING]                                      | [VALUE_DOMAIN]                | [ROLE]                                               |
| :-----: | :------------------------------------------------ | :--------------------------- | :--------------------------------------------------- |
|  [01]   | `ep.context_enable`                               | `0` / `1`                    | reads the precompiled EP-context graph on load        |
|  [02]   | `ep.context_file_path`                            | filesystem path string       | the context blob location                            |
|  [03]   | `ep.context_embed_mode`                           | `0` / `1`                    | embed the EP-context binary in the model vs. side-file (mirrors `OrtModelCompilationOptions.SetEpContextEmbedMode`) |
|  [04]   | `ep.context_node_name_prefix`                     | prefix string                | namespaces EP-context node names across fleet blobs   |
|  [05]   | `ep.context_model_external_initializers_file_name`| filename string              | spills EP-context initializers to a side file         |
|  [06]   | `ep.enable_weightless_ep_context_nodes`           | `0` / `1`                    | emits weightless context nodes (weights stay external)|
|  [07]   | `ep.share_ep_contexts` / `ep.stop_share_ep_contexts` | `0` / `1`                 | shares (or halts sharing) one EP-context across co-resident sessions |

Base session / arena / quantization keys the design composes:

| [INDEX] | [KEY_STRING]                                | [VALUE_DOMAIN]               | [ROLE]                                               |
| :-----: | :------------------------------------------ | :--------------------------- | :--------------------------------------------------- |
|  [01]   | `session.disable_cpu_ep_fallback`           | `0` / `1`                    | forbids silent CPU fallback so an accelerated EP that cannot claim a node FAILS rather than degrading unobserved |
|  [02]   | `session.use_env_allocators`                | `0` / `1`                    | routes the session onto the env-registered shared allocator (`CreateAndRegisterAllocator`/`CreateSharedAllocator`) |
|  [03]   | `session.use_device_allocator_for_initializers` | `0` / `1`                | loads initializers straight into device memory         |
|  [04]   | `session.set_denormal_as_zero`              | `0` / `1`                    | per-session subnormal flush (the `OrtThreadingOptions.SetGlobalDenormalAsZero` per-session twin) |
|  [05]   | `session.disable_prepacking`                | `0` / `1`                    | disables weight prepacking (paired with `PrePackedWeightsContainer`) |
|  [06]   | `session.intra_op.allow_spinning` / `session.intra_op.spin_duration_us` | `0`/`1` / microseconds | fine-grained intra-op spin policy beneath `GlobalSpinControl` |
|  [07]   | `session.qdq_matmulnbits_accuracy_level`    | accuracy-level int           | int4/int8 `MatMulNBits` accuracy floor — the runtime knob the `ModelPrecision` int4/int8 rows fold against |
|  [08]   | `session.qdq_matmulnbits_block_size`        | block-size int               | NBits quantization block size                          |
|  [09]   | `session.enable_dq_matmulnbits_fusion`      | `0` / `1`                    | fuses dequant + NBits matmul for quantized graphs      |
|  [10]   | `mlas.enable_gemm_fastmath_arm64_bfloat16`  | `0` / `1`                    | arm64 bf16 GEMM fast-math — Apple-silicon CPU throughput knob |

[CONFIG_KEY_SCOPE]: run-options config-entry keys (set through `RunOptions.AddRunConfigEntry`)
- rail: model
- source: `build/native/include/onnxruntime_run_options_config_keys.h` (1.27.0)

| [INDEX] | [KEY_STRING]                                  | [VALUE_DOMAIN]          | [ROLE]                                               |
| :-----: | :-------------------------------------------- | :---------------------- | :--------------------------------------------------- |
|  [01]   | `memory.enable_memory_arena_shrinkage`        | allocator device string | shrinks the arena after a run (the `RunConfig.Bulk` posture) |
|  [02]   | `disable_synchronize_execution_providers`     | `0` / `1`               | skips inter-EP sync for single-EP runs (latency knob) |
|  [03]   | `gpu_graph_id`                                | graph-id string         | CUDA-graph capture/replay annotation for fixed-shape runs |

[CONFIG_KEY_SCOPE]: env config keys (`onnxruntime_env_config_keys.h`)
- rail: model

| [INDEX] | [KEY_STRING]              | [VALUE_DOMAIN] | [ROLE]                                          |
| :-----: | :----------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `allow_virtual_devices`  | `0` / `1`      | admits virtual hardware devices into autoEP enumeration |

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
- OWNERSHIP: this page is the canonical owner of BOTH the EP/device selection matrix (sections 2-3 `ExecutionProvider*`/`AppendExecutionProvider_*`/`OrtEpDevice` + the `[EXECUTION_PROVIDER_SELECTION]` law) AND the per-RID native ABI matrix (section 2 `[PACKAGE_ASSET_SCOPE]: per-RID native ABI matrix`). The genai lane (`api-onnxruntimegenai`), the `.Gpu` lane, and the `.DirectML` lane declare ONLY their managed-floor pin and compose these facts here — neither restates the EP roster nor the RID payload set
- package asset: native runtime libraries; the BASE meta-package `runtimes/` ships only `win-x64` and `win-arm64` payloads inline (`onnxruntime.dll` + `onnxruntime_providers_shared.dll`). macOS arm64 and Linux x64/arm64 base runtimes load from the platform/feed-resolved `libonnxruntime.{dylib,so}` the build copies — the CoreML EP is built into the macOS dylib (no separate provider DLL), and accelerated CUDA/TensorRT (`linux-x64`/`win-x64` only) and DirectML (`win-x64`/`win-arm64`) providers ride the sibling `.Gpu`/`.DirectML` packages, additive onto the matching RID (the DirectML line is the 1.24.4 HOLD, RID-gated rather than co-resident on the 1.27.0 base)
- build assets: `build/native/*.props`/`*.targets` copy native runtime libraries by RID
- provider policy: execution-provider selection is explicit model-rail policy; append order is fallback priority — the genai `Config.AppendProvider`/`SetProviderOption`/`SetDecoderProviderOptionsHardware*` surface selects FROM this EP roster and binds to devices this page's `OrtEpDevice`/`OrtHardwareDevice` discovery enumerates

[LOCAL_ADMISSION]:
- Compute model execution enters through ONNX Runtime sessions and typed value binding.
- Model load, input binding, run policy, output projection, and disposal each emit receipts.
- Provider selection is policy data and cannot hide inside model-call helpers.
- Custom operators enter only through declared session options and asset evidence.

[STACKING]: the managed surface is internalized into single dense `Model/*` rails alongside the sibling admitted libs, never wrapped one-to-one —
- boot rail: `OrtThreadingOptions` (`GlobalIntraOp/InterOpNumThreads`, `GlobalSpinControl`) reads the AppHost `CpuBudget`, folds into `EnvironmentCreationOptions`, and `OrtEnv.CreateInstanceWithOptions(ref)` boots once behind `OrtEnv.IsCreated`; `DisableTelemetryEvents` runs at boot because the telemetry spine owns signals
- session rail: `SessionOptions` config-entry keys + EP `Register` + `OrtModelCompilationOptions` EP-context compile fold into ONE `Open`, the resident map keys on the model checksum (`UInt128`), and the compiled context blob is content-addressed via `System.IO.Hashing.XxHash3` over `OrtEpDevice` `EpName`/`VendorId`/`DeviceId`/`HardwareDevice.Type` then crosses to the Persistence blob lane as an `ArtifactIndexRow` — one artifact owner, not a second EP cache
- provider rail: the `ExecutionProvider` `[SmartEnum<string>]` (Thinktecture) carries each EP's `CoreMLFlags`/option-table/`ExecutionProviderDevicePolicy`/`OrtHardwareDeviceType`-affinity columns; `GetEpDevices()` → device-rank → `AppendExecutionProvider(env, devices, …)` is ONE polymorphic `Register`, and the two-step `GetCompatibilityInfoFromModel` → `GetModelCompatibilityForEpDevices` enum verdict is read once and CONSUMED into the warm-start branch
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

| [INDEX] | [MEMBER]                           | [SIGNATURE]                                                                                                                                                                                                |
| :-----: | :--------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `RegisterCustomOpLibrary`          | `void RegisterCustomOpLibrary(string libraryPath)`                                                                                                                                                         |
|  [02]   | `RegisterCustomOpLibraryV2`        | `void RegisterCustomOpLibraryV2(string libraryPath, out nint libraryHandle)`                                                                                                                               |
|  [03]   | `RegisterOrtExtensions`            | `void RegisterOrtExtensions()` — calls `OrtExtensionsNativeMethods.RegisterCustomOps`; throws `OnnxRuntimeException(ErrorCode.NoSuchFile)` if `Microsoft.ML.OnnxRuntime.Extensions` native asset is absent |
|  [04]   | `SetEpSelectionPolicy`             | `void SetEpSelectionPolicy(ExecutionProviderDevicePolicy policy)`                                                                                                                                          |
|  [05]   | `SetEpSelectionPolicyDelegate`     | `void SetEpSelectionPolicyDelegate(EpSelectionDelegate selectionDelegate = null)`                                                                                                                          |
|  [06]   | `AppendExecutionProvider_CPU`      | `void AppendExecutionProvider_CPU(int useArena = 1)`                                                                                                                                                       |
|  [07]   | `AppendExecutionProvider_CUDA`     | `void AppendExecutionProvider_CUDA(int deviceId = 0)` / `void AppendExecutionProvider_CUDA(OrtCUDAProviderOptions)`                                                                                        |
|  [08]   | `AppendExecutionProvider_DML`      | `void AppendExecutionProvider_DML(int deviceId = 0)`                                                                                                                                                       |
|  [09]   | `AppendExecutionProvider_Tensorrt` | `void AppendExecutionProvider_Tensorrt(int deviceId = 0)` / `void AppendExecutionProvider_Tensorrt(OrtTensorRTProviderOptions)`                                                                            |
|  [10]   | `AppendExecutionProvider_ROCm`     | `void AppendExecutionProvider_ROCm(int deviceId = 0)` / `void AppendExecutionProvider_ROCm(OrtROCMProviderOptions)`                                                                                        |
|  [11]   | `AppendExecutionProvider_CoreML`   | `void AppendExecutionProvider_CoreML(CoreMLFlags coremlFlags = CoreMLFlags.COREML_FLAG_USE_NONE)`                                                                                                          |
|  [12]   | `AppendExecutionProvider`          | `void AppendExecutionProvider(string providerName, Dictionary<string, string> providerOptions = null)`                                                                                                     |
|  [13]   | `AppendExecutionProvider`          | `void AppendExecutionProvider(OrtEnv env, IReadOnlyList<OrtEpDevice> epDevices, IReadOnlyDictionary<string, string> epOptions)`                                                                            |
|  [14]   | `OrtEnv.GetEpDevices`              | `IReadOnlyList<OrtEpDevice> GetEpDevices()`                                                                                                                                                                |
|  [15]   | `MakeSessionOptionWithCudaProvider`| `static SessionOptions MakeSessionOptionWithCudaProvider(int deviceId = 0)` / `(OrtCUDAProviderOptions)` — one-call session+EP shorthand                                                                    |
|  [16]   | `MakeSessionOptionWithTensorrtProvider` | `static SessionOptions MakeSessionOptionWithTensorrtProvider(int deviceId = 0)` / `(OrtTensorRTProviderOptions)`                                                                                       |
|  [17]   | `MakeSessionOptionWithRocmProvider`| `static SessionOptions MakeSessionOptionWithRocmProvider(int deviceId = 0)` / `(OrtROCMProviderOptions)`                                                                                                    |
|  [18]   | `OrtEnv.RegisterExecutionProviderLibrary` | `void RegisterExecutionProviderLibrary(string registrationName, string libraryPath)` — autoEP out-of-tree provider registration; `UnregisterExecutionProviderLibrary(string)` retracts               |

[REGISTRATION_LAW]:
- `RegisterCustomOpLibrary(path)` maps to `OrtRegisterCustomOpsLibrary_V2` in the C API (load and register from path, no handle returned) — ONNX Runtime owns the library lifetime and frees it when the `SessionOptions` and every session built from them release, so this is the canonical leak-free registration spelling that tracks no caller handle.
- `RegisterCustomOpLibraryV2(path, out nint)` maps to `OrtRegisterCustomOpsLibrary` (load and register, returning a native handle the CALLER then owns) — the legacy caller-must-free path: a discarded `out _` handle never unloads and leaks the library, so it is the rejected spelling.
- `RegisterOrtExtensions()` is a convenience wrapper that loads the `libortextensions` native asset shipped by `Microsoft.ML.OnnxRuntime.Extensions`; there is no separate public `OrtExtensions` class — `OrtExtensionsNativeMethods` is internal.
- `OrtExtensions.RegisterCustomOps` does not exist as a public API in either the ORT managed assembly or the Extensions package; the correct entry point is `SessionOptions.RegisterOrtExtensions()`.
- `UseModel` does not exist on `SessionOptions` or `InferenceSession` in 1.27.0; it is not part of this package's public surface.
- `DisposableNamedOnnxValue.CreateFromOrtValue` is internal; it is not a callable public factory — callers consume `DisposableNamedOnnxValue` from `InferenceSession.Run` result collections only.

## [07]-[COMPILE_API]

`OrtModelCompilationOptions` (namespace `Microsoft.ML.OnnxRuntime.CompileApi`) drives the EP-context compile pipeline: it builds a compiled model whose execution-provider partitions are embedded or written to disk so a later session loads the precompiled graph instead of recompiling. The `ep.context_*` session-config keys in section 4 enable the EP-context path; this type configures and runs the compile. The `Model/sessions#SESSION_CAPSULE` `Compile` member drives it for the fleet-shared device-keyed context.

[ENTRYPOINT_SCOPE]: compile-API delegate contracts (nested inside `OrtModelCompilationOptions`, not standalone)
- rail: model

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]       | [CAPABILITY]                                           |
| :-----: | :--------------------------------- | :------------------- | :----------------------------------------------------- |
|  [01]   | `OrtModelCompilationOptions.WriteBufferToDestinationDelegate` | write delegate       | streams compiled output buffer to a caller destination (the in-memory output sink) |
|  [02]   | `OrtModelCompilationOptions.GetInitializerLocationDelegate`   | initializer delegate | maps an initializer to an external storage location (returns `OrtExternalInitializerInfo`) |

[ENTRYPOINT_SCOPE]: `OrtModelCompilationOptions` decompile-verified signatures
- source: `Microsoft.ML.OnnxRuntime.Managed` 1.27.0 (`net8.0` consumer-bound asset) decompile
- rail: model (consumed by `Model/sessions#SESSION_CAPSULE` `Compile`)

| [INDEX] | [MEMBER]                                                                  | [SIGNATURE]                                                                                                                                                                                    |
| :-----: | :------------------------------------------------------------------------ | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `OrtModelCompilationOptions.ctor`                                         | `OrtModelCompilationOptions(SessionOptions sessionOptions)`                                                                                                                                    |
|  [02]   | `OrtModelCompilationOptions.CompileModel`                                 | `void CompileModel()`                                                                                                                                                                          |
|  [03]   | `OrtModelCompilationOptions.SetInputModelPath`                            | `void SetInputModelPath(string path)`                                                                                                                                                          |
|  [04]   | `OrtModelCompilationOptions.SetInputModelFromBuffer`                      | `void SetInputModelFromBuffer(byte[] buffer)`                                                                                                                                                  |
|  [05]   | `OrtModelCompilationOptions.SetOutputModelPath`                           | `void SetOutputModelPath(string path)`                                                                                                                                                         |
|  [06]   | `OrtModelCompilationOptions.SetOutputModelExternalInitializersFile`       | `void SetOutputModelExternalInitializersFile(string filePath, ulong threshold)`                                                                                                                |
|  [07]   | `OrtModelCompilationOptions.SetEpContextEmbedMode`                        | `void SetEpContextEmbedMode(bool embed)`                                                                                                                                                       |
|  [08]   | `OrtModelCompilationOptions.SetFlags`                                     | `void SetFlags(OrtCompileApiFlags flags)`                                                                                                                                                      |
|  [09]   | `OrtModelCompilationOptions.SetEpContextBinaryInformation`                | `void SetEpContextBinaryInformation(string outputDirectory, string modelName)`                                                                                                                 |
|  [10]   | `OrtModelCompilationOptions.SetGraphOptimizationLevel`                    | `void SetGraphOptimizationLevel(GraphOptimizationLevel graphOptimizationLevel)`                                                                                                                |
|  [11]   | `OrtModelCompilationOptions.SetOutputModelWriteDelegate`                  | `void SetOutputModelWriteDelegate(WriteBufferToDestinationDelegate writeBufferDelegate)`                                                                                                       |
|  [12]   | `OrtModelCompilationOptions.SetOutputModelGetInitializerLocationDelegate` | `void SetOutputModelGetInitializerLocationDelegate(GetInitializerLocationDelegate getInitializerLocationDelegate)`                                                                             |
|  [13]   | `OrtModelCompilationOptions.Dispose`                                      | `void Dispose()`                                                                                                                                                                               |
|  [14]   | `WriteBufferToDestinationDelegate`                                        | `delegate void WriteBufferToDestinationDelegate(ReadOnlySpan<byte> buffer)`                                                                                                                    |
|  [15]   | `GetInitializerLocationDelegate`                                          | `delegate OrtExternalInitializerInfo GetInitializerLocationDelegate(string initializerName, IReadOnlyOrtValue initializerValue, IReadOnlyExternalInitializerInfo originalInitializerLocation)` |
|  [16]   | `OrtCompileApiFlags`                                                      | `enum OrtCompileApiFlags : uint { NONE = 0, ERROR_IF_NO_NODES_COMPILED = 1, ERROR_IF_OUTPUT_FILE_EXISTS = 2 }`                                                                                 |

[COMPILE_LAW]:
- `OrtModelCompilationOptions(SessionOptions)` binds the compile to a configured session whose EP append order determines which provider claims each subgraph; `CompileModel()` runs the partition and write.
- input source is exclusive: `SetInputModelPath(string)` reads from disk and `SetInputModelFromBuffer(byte[])` reads from memory.
- output destination is exclusive: `SetOutputModelPath(string)` writes to disk, `SetOutputModelWriteDelegate` streams the buffer to a caller sink, and `SetOutputModelExternalInitializersFile(string, ulong)` spills initializers over the byte threshold to a side file.
- `SetEpContextEmbedMode(bool)` embeds the EP-context binary inside the compiled model when `true`, or writes it beside the model when `false`; `SetEpContextBinaryInformation(string, string)` names the output directory and model for the external EP-context binary.
- `SetFlags(OrtCompileApiFlags)` controls compile strictness: `ERROR_IF_NO_NODES_COMPILED` fails when no subgraph is EP-claimed, and `ERROR_IF_OUTPUT_FILE_EXISTS` fails rather than overwriting.
- `SetGraphOptimizationLevel(GraphOptimizationLevel)` applies graph optimization before partitioning; `SetOutputModelGetInitializerLocationDelegate` routes each initializer to an external storage location.
- the compile handle is native; `Dispose()` releases it and the registered delegate state deterministically.

# [RASM_COMPUTE_API_ONNXRUNTIME]

`Microsoft.ML.OnnxRuntime` supplies model session execution, native runtime
assets, tensor value binding, run options, metadata inspection, custom operators,
and execution-provider selection for Compute model rails.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime`
- package: `Microsoft.ML.OnnxRuntime`
- assembly: `Microsoft.ML.OnnxRuntime` (managed: `Microsoft.ML.OnnxRuntime.Managed` 1.27.0)
- namespace: `Microsoft.ML.OnnxRuntime`
- asset: runtime library and native assets
- rail: model

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: session and run contracts
- rail: model

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]       | [CAPABILITY]             |
| :-----: | :------------------------- | :------------------- | :----------------------- |
|   [1]   | `InferenceSession`         | session root         | executes model runs      |
|   [2]   | `SessionOptions`           | session policy       | configures session       |
|   [3]   | `RunOptions`               | run policy           | configures inference run |
|   [4]   | `OrtEnv`                   | runtime environment  | owns runtime scope       |
|   [5]   | `OrtMemoryInfo`            | memory descriptor    | describes tensor memory  |
|   [6]   | `OrtValue`                 | native value         | carries model values     |
|   [7]   | `NamedOnnxValue`           | named value          | binds named input        |
|   [8]   | `DisposableNamedOnnxValue` | named output         | owns named output        |
|   [9]   | `TensorElementType`        | element classifier   | classifies tensor data   |
|  [10]   | `OrtAllocatorType`         | allocator classifier | classifies allocators    |

[PUBLIC_TYPE_SCOPE]: metadata and tensor contracts
- rail: model

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]  | [CAPABILITY]                              |
| :-----: | :-------------------------- | :-------------- | :---------------------------------------- |
|   [1]   | `ModelMetadata`             | model metadata  | describes model                           |
|   [2]   | `NodeMetadata`              | node metadata   | describes model nodes                     |
|   [3]   | `OrtTensorTypeAndShapeInfo` | tensor metadata | describes tensor shape                    |
|   [4]   | `DenseTensor<T>`            | tensor value    | carries typed tensors                     |
|   [5]   | `OrtIoBinding`              | binding root    | binds inputs and outputs                  |
|   [6]   | `PrePackedWeightsContainer` | weights cache   | shares packed weights                     |
|   [7]   | `FixedBufferOnnxValue`      | buffer value    | binds fixed memory                        |
|   [8]   | `OrtLoraAdapter`            | adapter value   | binds LoRA adapter                        |
|   [9]   | `OrtMemoryAllocation`       | device buffer   | owns a device allocation                  |
|  [10]   | `OrtMemType`                | memory enum     | `Default`, `Cpu`, `CpuInput`, `CpuOutput` |
|  [11]   | `OrtAllocatorType`          | allocator enum  | `DeviceAllocator`, `ArenaAllocator`       |

[PUBLIC_TYPE_SCOPE]: environment, threading, and provider-policy contracts
- rail: model

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]       | [CAPABILITY]                                                                                                                                                                                                                                 |
| :-----: | :------------------------------ | :------------------- | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `EnvironmentCreationOptions`    | boot options struct  | fields `logId`, `logLevel`, `threadOptions`                                                                                                                                                                                                  |
|   [2]   | `OrtThreadingOptions`           | global thread pool   | `GlobalIntraOpNumThreads`, `GlobalInterOpNumThreads`, `GlobalSpinControl`                                                                                                                                                                    |
|   [3]   | `ExecutionProviderDevicePolicy` | device-policy enum   | `DEFAULT`, `PREFER_CPU`, `PREFER_NPU`, `PREFER_GPU`, `MAX_PERFORMANCE`, `MAX_EFFICIENCY`, `MIN_OVERALL_POWER`                                                                                                                                |
|   [4]   | `GraphOptimizationLevel`        | optimization enum    | session graph optimization incl. `ORT_ENABLE_ALL`                                                                                                                                                                                            |
|   [5]   | `OrtLoggingLevel`               | logging enum         | boot log severity                                                                                                                                                                                                                            |
|   [6]   | `OrtAllocator`                  | allocator handle     | owns native allocation scope                                                                                                                                                                                                                 |
|   [7]   | `IDisposableReadOnlyCollection` | result collection    | disposable native result set                                                                                                                                                                                                                 |
|   [8]   | `BFloat16`                      | readonly struct      | CLR carrier for the bfloat16 element type                                                                                                                                                                                                    |
|   [9]   | `OrtEpDevice`                   | EP device descriptor | `EpName`, `EpVendor`, `EpMetadata`, `EpOptions`, `HardwareDevice`, `GetMemoryInfo`, `CreateSyncStream`                                                                                                                                       |
|  [10]   | `OrtHardwareDevice`             | hardware descriptor  | `Type` (`OrtHardwareDeviceType`), `VendorId`, `Vendor`, `DeviceId`, `Metadata`                                                                                                                                                               |
|  [11]   | `OrtHardwareDeviceType`         | device-type enum     | `CPU`, `GPU`, `NPU`                                                                                                                                                                                                                          |
|  [12]   | `OrtKeyValuePairs`              | kv-pairs handle      | `Entries`, `Add`, `Remove`, `Refresh`; ctor from `IReadOnlyDictionary<string,string>`                                                                                                                                                        |
|  [13]   | `OrtDeviceMemoryType`           | device-memory enum   | `DEFAULT` (0), `HOST_ACCESSIBLE` (5)                                                                                                                                                                                                         |
|  [14]   | `OrtSyncStream`                 | sync-stream handle   | `GetHandle()` — ties device stream lifetime to `OrtEpDevice.CreateSyncStream`                                                                                                                                                                |
|  [15]   | `CoreMLFlags`                   | CoreML flags enum    | `COREML_FLAG_USE_NONE`, `COREML_FLAG_USE_CPU_ONLY`, `COREML_FLAG_ENABLE_ON_SUBGRAPH`, `COREML_FLAG_ONLY_ENABLE_DEVICE_WITH_ANE`, `COREML_FLAG_ONLY_ALLOW_STATIC_INPUT_SHAPES`, `COREML_FLAG_CREATE_MLPROGRAM`, `COREML_FLAG_USE_CPU_AND_GPU` |
|  [16]   | `NnapiFlags`                    | NNAPI flags enum     | `NNAPI_FLAG_USE_NONE` and accelerator-mode bits                                                                                                                                                                                              |
|  [17]   | `OrtCUDAProviderOptions`        | CUDA options handle  | `UpdateOptions(Dictionary<string,string>)`, `GetOptions()` string                                                                                                                                                                            |
|  [18]   | `OrtTensorRTProviderOptions`    | TensorRT options     | `UpdateOptions(Dictionary<string,string>)`, `GetDeviceId()`                                                                                                                                                                                  |

[PUBLIC_TYPE_SCOPE]: ROCm, execution-mode, and provider-options contracts
- rail: model

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]         | [CAPABILITY]                                                        |
| :-----: | :--------------------------- | :--------------------- | :------------------------------------------------------------------ |
|   [1]   | `OrtROCMProviderOptions`     | ROCm options handle    | `UpdateOptions(Dictionary<string,string>)`                          |
|   [2]   | `ExecutionMode`              | execution-mode enum    | `ORT_SEQUENTIAL`, `ORT_PARALLEL`                                    |
|   [3]   | `OrtModelCompilationOptions` | compile options handle | drives the EP-context model compile pipeline                        |
|   [4]   | `OrtCompileApiFlags`         | compile flags enum     | `NONE`, `ERROR_IF_NO_NODES_COMPILED`, `ERROR_IF_OUTPUT_FILE_EXISTS` |

[PUBLIC_TYPE_SCOPE]: package assets
- rail: model

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE] | [CAPABILITY]            |
| :-----: | :--------------------------------- | :------------- | :---------------------- |
|   [1]   | `Microsoft.ML.OnnxRuntime.props`   | MSBuild import | declares native copy    |
|   [2]   | `Microsoft.ML.OnnxRuntime.targets` | MSBuild import | copies native assets    |
|   [3]   | `libonnxruntime.dylib`             | native asset   | executes native runtime |
|   [4]   | `libonnxruntime.so`                | native asset   | executes native runtime |
|   [5]   | `onnxruntime.dll`                  | native asset   | executes native runtime |
|   [6]   | `onnxruntime_c_api.h`              | native header  | describes C API         |
|   [7]   | `onnxruntime_cxx_api.h`            | native header  | describes C++ API       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: inference operations
- rail: model

| [INDEX] | [SURFACE]                | [CALL_SHAPE]     | [CAPABILITY]             |
| :-----: | :----------------------- | :--------------- | :----------------------- |
|   [1]   | `InferenceSession.Run`   | run call         | executes inference       |
|   [2]   | `RunAsync`               | async run call   | executes inference       |
|   [3]   | `RunWithBindingAndNames` | binding run call | executes bound inference |
|   [4]   | `RunWithBoundResults`    | binding run call | returns bound outputs    |
|   [5]   | `EndProfiling`           | session call     | closes profiling output  |
|   [6]   | `CreateIoBinding`        | factory call     | creates IO binding       |
|   [7]   | `Dispose`                | lifetime call    | releases native handles  |

[ENTRYPOINT_SCOPE]: execution-provider append operations
- rail: model
- note: all are instance methods on `SessionOptions`

| [INDEX] | [SURFACE]                                                                                                                  | [CALL_SHAPE] | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------------------------------------------------------------------- | :----------- | :-------------------------------------------- |
|   [1]   | `AppendExecutionProvider_CPU(int useArena = 1)`                                                                            | option call  | CPU EP; `useArena` 1 enables memory arena     |
|   [2]   | `AppendExecutionProvider_CUDA(int deviceId = 0)`                                                                           | option call  | CUDA EP by device index                       |
|   [3]   | `AppendExecutionProvider_CUDA(OrtCUDAProviderOptions)`                                                                     | option call  | CUDA EP with full provider-options struct     |
|   [4]   | `AppendExecutionProvider_DML(int deviceId = 0)`                                                                            | option call  | DirectML EP by device index                   |
|   [5]   | `AppendExecutionProvider_Tensorrt(int deviceId = 0)`                                                                       | option call  | TensorRT EP by device index                   |
|   [6]   | `AppendExecutionProvider_Tensorrt(OrtTensorRTProviderOptions)`                                                             | option call  | TensorRT EP with full provider-options struct |
|   [7]   | `AppendExecutionProvider_ROCm(int deviceId = 0)`                                                                           | option call  | ROCm EP by device index                       |
|   [8]   | `AppendExecutionProvider_ROCm(OrtROCMProviderOptions)`                                                                     | option call  | ROCm EP with full provider-options struct     |
|   [9]   | `AppendExecutionProvider_CoreML(CoreMLFlags coremlFlags = CoreMLFlags.COREML_FLAG_USE_NONE)`                               | option call  | CoreML EP with compute-unit flags             |
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
|   [1]   | `OrtValue.CreateTensorValueFromMemory<T>(T[], long[])`                      | factory call | binds managed array as tensor             |
|   [2]   | `OrtValue.CreateTensorValueFromMemory<T>(OrtMemoryInfo, Memory<T>, long[])` | factory call | binds device-pinned memory as tensor      |
|   [3]   | `OrtValue.CreateTensorValueFromSystemNumericsTensorObject<T>(Tensor<T>)`    | factory call | binds `System.Numerics.Tensors.Tensor<T>` |
|   [4]   | `OrtValue.CreateFromStringTensor(Tensor<string>)`                           | factory call | binds string tensor input                 |
|   [5]   | `NamedOnnxValue.CreateFromTensor<T>(string, Tensor<T>)`                     | factory call | creates named value from tensor           |
|   [6]   | `RegisterCustomOpLibrary`                                                   | option call  | loads custom operators                    |
|   [7]   | `RegisterCustomOpLibraryV2`                                                 | option call  | loads custom operators                    |
|   [8]   | `RegisterOrtExtensions`                                                     | option call  | loads extension ops                       |
|   [9]   | `EnableMemoryPattern`                                                       | option call  | enables memory reuse                      |
|  [10]   | `RunOptions.AddRunConfigEntry`                                              | option call  | sets run config entry                     |

[ENTRYPOINT_SCOPE]: environment, session-policy, and run-policy operations
- rail: model

Provider names include `CoreMLExecutionProvider` and `CPUExecutionProvider`; threading options carry typed global thread and spin settings.

[ENVIRONMENT_OPERATIONS]:

| [INDEX] | [SURFACE]                                  | [CALL_SHAPE]    | [CAPABILITY]                                                                           |
| :-----: | :----------------------------------------- | :-------------- | :------------------------------------------------------------------------------------- |
|   [1]   | `OrtEnv.IsCreated`                         | static property | reports environment creation                                                           |
|   [2]   | `OrtEnv.Instance`                          | static call     | returns the singleton environment                                                      |
|   [3]   | `OrtEnv.CreateInstanceWithOptions`         | static call     | boots with environment options                                                         |
|   [4]   | `OrtEnv.DisableTelemetryEvents`            | instance call   | silences runtime telemetry                                                             |
|   [5]   | `OrtEnv.GetVersionString`                  | instance call   | reports the native runtime version                                                     |
|   [6]   | `OrtEnv.GetAvailableProviders`             | instance call   | returns provider names                                                                 |
|   [7]   | `OrtEnv.GetEpDevices`                      | instance call   | `IReadOnlyList<OrtEpDevice>` — enumerates autoEP-available devices                     |
|   [8]   | `OrtEnv.GetModelCompatibilityForEpDevices` | instance call   | `OrtCompiledModelCompatibility` for a device list and model path                       |
|   [9]   | `OrtEnv.CreateSharedAllocator`             | instance call   | `(OrtEpDevice, OrtDeviceMemoryType, OrtAllocatorType, options)` returns `OrtAllocator` |
|  [10]   | `OrtEnv.ReleaseSharedAllocator`            | instance call   | releases a shared allocator for a device and memory type                               |

[SESSION_POLICY_OPERATIONS]:

| [INDEX] | [SURFACE]                                              | [CALL_SHAPE]    | [CAPABILITY]                                                                                  |
| :-----: | :----------------------------------------------------- | :-------------- | :-------------------------------------------------------------------------------------------- |
|   [1]   | `SessionOptions.AddSessionConfigEntry`                 | option call     | sets string config entry                                                                      |
|   [2]   | `SessionOptions.AddFreeDimensionOverrideByName`        | option call     | binds symbolic dimension value                                                                |
|   [3]   | `SessionOptions.AddFreeDimensionOverride`              | option call     | binds dimension by denotation string                                                          |
|   [4]   | `SessionOptions.AddInitializer`                        | option call     | `(string, OrtValue)` injects a pre-loaded initializer                                         |
|   [5]   | `SessionOptions.DisablePerSessionThreads`              | option call     | routes sessions onto the global pool                                                          |
|   [6]   | `SessionOptions.SetEpSelectionPolicy`                  | option call     | applies `ExecutionProviderDevicePolicy` enum to session                                       |
|   [7]   | `SessionOptions.SetEpSelectionPolicyDelegate`          | option call     | `(EpSelectionDelegate)` applies a custom device-rank fn                                       |
|   [8]   | `SessionOptions.EnableProfiling`                       | option property | enables chrome-trace profiling                                                                |
|   [9]   | `SessionOptions.ProfileOutputPathPrefix`               | option property | sets profile artifact prefix                                                                  |
|  [10]   | `SessionOptions.GraphOptimizationLevel`                | option property | sets graph optimization level                                                                 |
|  [11]   | `SessionOptions.ExecutionMode`                         | option property | `ORT_SEQUENTIAL` or `ORT_PARALLEL` execution                                                  |
|  [12]   | `SessionOptions.IntraOpNumThreads`                     | option property | per-session intra-op thread count                                                             |
|  [13]   | `SessionOptions.InterOpNumThreads`                     | option property | per-session inter-op thread count                                                             |
|  [14]   | `SessionOptions.MakeSessionOptionWithCudaProvider`     | static factory  | `(int deviceId)` or `(OrtCUDAProviderOptions)` shorthand                                      |
|  [15]   | `SessionOptions.MakeSessionOptionWithTensorrtProvider` | static factory  | `(int)` or `(OrtTensorRTProviderOptions)` shorthand                                           |
|  [16]   | `SessionOptions.MakeSessionOptionWithRocmProvider`     | static factory  | `(int)` or `(OrtROCMProviderOptions)` shorthand                                               |
|  [17]   | `SessionOptions.EpSelectionDelegate`                   | delegate type   | `(IReadOnlyList<OrtEpDevice>, OrtKeyValuePairs, OrtKeyValuePairs, uint) -> List<OrtEpDevice>` |

[VALUE_RUN_OPERATIONS]:

| [INDEX] | [SURFACE]                               | [CALL_SHAPE] | [CAPABILITY]                     |
| :-----: | :-------------------------------------- | :----------- | :------------------------------- |
|   [1]   | `OrtValue.CreateTensorWithEmptyStrings` | factory call | allocates string output slots    |
|   [2]   | `RunOptions.Terminate`                  | run property | one-way cancellation latch       |
|   [3]   | `RunOptions.AddActiveLoraAdapter`       | run call     | attaches `OrtLoraAdapter`        |
|   [4]   | `OrtLoraAdapter.Create`                 | factory call | loads adapter from path or bytes |

[METADATA_THREADING_OPERATIONS]:

| [INDEX] | [SURFACE]                                     | [CALL_SHAPE]       | [CAPABILITY]                 |
| :-----: | :-------------------------------------------- | :----------------- | :--------------------------- |
|   [1]   | `ModelMetadata.Version`                       | metadata property  | graph version                |
|   [2]   | `NodeMetadata.ElementDataType`                | metadata property  | tensor slot element type     |
|   [3]   | `NodeMetadata.Dimensions`                     | metadata property  | fixed dimensions             |
|   [4]   | `NodeMetadata.SymbolicDimensions`             | metadata property  | free-dimension names         |
|   [5]   | `OrtThreadingOptions.GlobalIntraOpNumThreads` | set-only property  | global intra-op thread count |
|   [6]   | `OrtThreadingOptions.GlobalInterOpNumThreads` | set-only property  | global inter-op thread count |
|   [7]   | `OrtThreadingOptions.GlobalSpinControl`       | set-only property  | spin versus low-CPU policy   |
|   [8]   | `OrtThreadingOptions`                         | parameterless ctor | thread-pool options handle   |

[ENTRYPOINT_SCOPE]: IO-binding bound-inference operations
- rail: model

| [INDEX] | [SURFACE]                                      | [CALL_SHAPE]    | [CAPABILITY]                                                                                                                                        |
| :-----: | :--------------------------------------------- | :-------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `InferenceSession.CreateIoBinding`             | factory call    | returns an `OrtIoBinding` bound to the session                                                                                                      |
|   [2]   | `InferenceSession.RunWithBinding`              | bound-run call  | `(RunOptions, OrtIoBinding)` executes a pre-bound inference                                                                                         |
|   [3]   | `OrtIoBinding.BindInput`                       | bind call       | `(string, OrtValue)` binds a named input value                                                                                                      |
|   [4]   | `OrtIoBinding.BindInput`                       | bind call       | `(string, TensorElementType, long[], OrtMemoryAllocation)` binds device input                                                                       |
|   [5]   | `OrtIoBinding.BindOutput`                      | bind call       | `(string, OrtValue)` binds a named output value                                                                                                     |
|   [6]   | `OrtIoBinding.BindOutput`                      | bind call       | `(string, TensorElementType, long[], OrtMemoryAllocation)` binds device output                                                                      |
|   [7]   | `OrtIoBinding.BindOutputToDevice`              | bind call       | `(string, OrtMemoryInfo)` binds an output to a device allocator                                                                                     |
|   [8]   | `OrtIoBinding.SynchronizeBoundInputs`          | sync call       | flushes pending bound input transfers                                                                                                               |
|   [9]   | `OrtIoBinding.SynchronizeBoundOutputs`         | sync call       | flushes pending bound output transfers                                                                                                              |
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

## [4]-[CONFIG_KEYS]

[CONFIG_KEY_SCOPE]: session-options config-entry keys
- rail: model

| [INDEX] | [KEY_STRING]           | [SETTER]                               | [VALUE_DOMAIN]         |
| :-----: | :--------------------- | :------------------------------------- | :--------------------- |
|   [1]   | `ep.context_enable`    | `SessionOptions.AddSessionConfigEntry` | `0` / `1`              |
|   [2]   | `ep.context_file_path` | `SessionOptions.AddSessionConfigEntry` | filesystem path string |
|   [3]   | `ep.share_ep_contexts` | `SessionOptions.AddSessionConfigEntry` | `0` / `1`              |

[CONFIG_KEY_SCOPE]: run-options config-entry keys
- rail: model

| [INDEX] | [KEY_STRING]                           | [SETTER]                       | [VALUE_DOMAIN]          |
| :-----: | :------------------------------------- | :----------------------------- | :---------------------- |
|   [1]   | `memory.enable_memory_arena_shrinkage` | `RunOptions.AddRunConfigEntry` | allocator device string |

[CONFIG_KEY_SCOPE]: CoreML provider-option keys
- rail: model

| [INDEX] | [KEY_STRING]                         | [VALUE_DOMAIN]                                      |
| :-----: | :----------------------------------- | :-------------------------------------------------- |
|   [1]   | `ModelFormat`                        | `MLProgram`, `NeuralNetwork`                        |
|   [2]   | `MLComputeUnits`                     | `ALL`, `CPUOnly`, `CPUAndGPU`, `CPUAndNeuralEngine` |
|   [3]   | `RequireStaticInputShapes`           | `0`, `1`                                            |
|   [4]   | `EnableOnSubgraphs`                  | `0`, `1`                                            |
|   [5]   | `SpecializationStrategy`             | `Default`, `FastPrediction`                         |
|   [6]   | `ProfileComputePlan`                 | `0`, `1`                                            |
|   [7]   | `AllowLowPrecisionAccumulationOnGPU` | `0`, `1`                                            |
|   [8]   | `ModelCacheDirectory`                | cache-directory path string                         |

## [5]-[IMPLEMENTATION_LAW]

[MODEL_SESSION]:
- namespace: `Microsoft.ML.OnnxRuntime`
- session root: `InferenceSession`
- policy root: `SessionOptions`
- run root: `RunOptions`
- metadata root: model, node, tensor, and element classifiers
- lifetime: native handles release through deterministic disposal

[EXECUTION_PROVIDER_SELECTION]:
- explicit EP: call `AppendExecutionProvider_CPU/CUDA/DML/Tensorrt/ROCm/CoreML/OpenVINO/MIGraphX/Nnapi/Dnnl` before session construction; order determines fallback priority
- generic EP: `AppendExecutionProvider(string, Dictionary<string,string>)` selects by provider-name string for any registered provider
- autoEP device-list EP: `AppendExecutionProvider(OrtEnv, IReadOnlyList<OrtEpDevice>, IReadOnlyDictionary<string,string>)` binds directly to devices from `OrtEnv.GetEpDevices()`
- policy EP: `SetEpSelectionPolicy(ExecutionProviderDevicePolicy)` sets an enum-driven auto-selection strategy; `SetEpSelectionPolicyDelegate(EpSelectionDelegate)` replaces it with a managed callback that receives `IReadOnlyList<OrtEpDevice>` and returns a ranked selection
- device enumeration: `OrtEnv.GetEpDevices()` returns `IReadOnlyList<OrtEpDevice>`, each carrying `EpName`, `EpVendor`, `HardwareDevice` (`Type`, `VendorId`, `DeviceId`), `EpMetadata`, and `EpOptions`

[IO_BINDING]:
- binding root: `OrtIoBinding` from `InferenceSession.CreateIoBinding`
- bound run: `InferenceSession.RunWithBinding(RunOptions, OrtIoBinding)` executes against pre-bound input and output slots
- input binding: `BindInput` accepts an `OrtValue` or a device `(TensorElementType, long[], OrtMemoryAllocation)` tuple
- output binding: `BindOutput` mirrors input binding; `BindOutputToDevice(string, OrtMemoryInfo)` routes outputs to a device allocator
- synchronization: `SynchronizeBoundInputs` and `SynchronizeBoundOutputs` flush pending device transfers around the bound run
- output projection: `GetOutputNames` and `GetOutputValues` read bound results; `ClearBoundInputs` and `ClearBoundOutputs` reset the binding between runs
- steady state: binding amortizes input and output allocation across repeated runs and is the measured hot-loop path

[NATIVE_RUNTIME]:
- package asset: runtime-native libraries
- runtime identifiers: macOS arm64, Linux arm64, Linux x64, Windows arm64, Windows x64
- build assets: props and targets copy native runtime libraries
- provider policy: execution-provider selection is explicit model-rail policy

[LOCAL_ADMISSION]:
- Compute model execution enters through ONNX Runtime sessions and typed value binding.
- Model load, input binding, run policy, output projection, and disposal each emit receipts.
- Provider selection is policy data and cannot hide inside model-call helpers.
- Custom operators enter only through declared session options and asset evidence.

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime`
- Owns: ONNX session execution and native runtime assets
- Accept: measured model inference
- Reject: ML.NET training pipeline

## [6]-[REGISTRATION_MEMBERS]

[ENTRYPOINT_SCOPE]: custom-op and extension registration decompile-verified signatures
- rail: model-lane#GENERATIVE_RUN

| [INDEX] | [MEMBER]                           | [SIGNATURE]                                                                                                                                                                                                |
| :-----: | :--------------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `RegisterCustomOpLibrary`          | `void RegisterCustomOpLibrary(string libraryPath)`                                                                                                                                                         |
|   [2]   | `RegisterCustomOpLibraryV2`        | `void RegisterCustomOpLibraryV2(string libraryPath, out nint libraryHandle)`                                                                                                                               |
|   [3]   | `RegisterOrtExtensions`            | `void RegisterOrtExtensions()` — calls `OrtExtensionsNativeMethods.RegisterCustomOps`; throws `OnnxRuntimeException(ErrorCode.NoSuchFile)` if `Microsoft.ML.OnnxRuntime.Extensions` native asset is absent |
|   [4]   | `SetEpSelectionPolicy`             | `void SetEpSelectionPolicy(ExecutionProviderDevicePolicy policy)`                                                                                                                                          |
|   [5]   | `SetEpSelectionPolicyDelegate`     | `void SetEpSelectionPolicyDelegate(EpSelectionDelegate selectionDelegate = null)`                                                                                                                          |
|   [6]   | `AppendExecutionProvider_CPU`      | `void AppendExecutionProvider_CPU(int useArena = 1)`                                                                                                                                                       |
|   [7]   | `AppendExecutionProvider_CUDA`     | `void AppendExecutionProvider_CUDA(int deviceId = 0)` / `void AppendExecutionProvider_CUDA(OrtCUDAProviderOptions)`                                                                                        |
|   [8]   | `AppendExecutionProvider_DML`      | `void AppendExecutionProvider_DML(int deviceId = 0)`                                                                                                                                                       |
|   [9]   | `AppendExecutionProvider_Tensorrt` | `void AppendExecutionProvider_Tensorrt(int deviceId = 0)` / `void AppendExecutionProvider_Tensorrt(OrtTensorRTProviderOptions)`                                                                            |
|  [10]   | `AppendExecutionProvider_ROCm`     | `void AppendExecutionProvider_ROCm(int deviceId = 0)` / `void AppendExecutionProvider_ROCm(OrtROCMProviderOptions)`                                                                                        |
|  [11]   | `AppendExecutionProvider_CoreML`   | `void AppendExecutionProvider_CoreML(CoreMLFlags coremlFlags = CoreMLFlags.COREML_FLAG_USE_NONE)`                                                                                                          |
|  [12]   | `AppendExecutionProvider`          | `void AppendExecutionProvider(string providerName, Dictionary<string, string> providerOptions = null)`                                                                                                     |
|  [13]   | `AppendExecutionProvider`          | `void AppendExecutionProvider(OrtEnv env, IReadOnlyList<OrtEpDevice> epDevices, IReadOnlyDictionary<string, string> epOptions)`                                                                            |
|  [14]   | `OrtEnv.GetEpDevices`              | `IReadOnlyList<OrtEpDevice> GetEpDevices()`                                                                                                                                                                |

[REGISTRATION_LAW]:
- `RegisterCustomOpLibrary(path)` maps to `OrtRegisterCustomOpsLibrary_V2` in the C API (load and register from path, no handle returned).
- `RegisterCustomOpLibraryV2(path, out nint)` maps to `OrtRegisterCustomOpsLibrary` (load and register, returning a native handle for explicit unloading).
- `RegisterOrtExtensions()` is a convenience wrapper that loads the `libortextensions` native asset shipped by `Microsoft.ML.OnnxRuntime.Extensions`; there is no separate public `OrtExtensions` class — `OrtExtensionsNativeMethods` is internal.
- `OrtExtensions.RegisterCustomOps` does not exist as a public API in either the ORT managed assembly or the Extensions package; the correct entry point is `SessionOptions.RegisterOrtExtensions()`.
- `UseModel` does not exist on `SessionOptions` or `InferenceSession` in 1.27.0; it is not part of this package's public surface.
- `DisposableNamedOnnxValue.CreateFromOrtValue` is internal; it is not a callable public factory — callers consume `DisposableNamedOnnxValue` from `InferenceSession.Run` result collections only.

## [7]-[COMPILE_API]

`OrtModelCompilationOptions` drives the EP-context compile pipeline: it builds a compiled model whose execution-provider partitions are embedded or written to disk so a later session loads the precompiled graph instead of recompiling. The `ep.context_*` session-config keys in section 4 enable the EP-context path; this type configures and runs the compile.

[ENTRYPOINT_SCOPE]: compile-API delegate contracts
- rail: model

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]       | [CAPABILITY]                                           |
| :-----: | :--------------------------------- | :------------------- | :----------------------------------------------------- |
|   [1]   | `WriteBufferToDestinationDelegate` | write delegate       | streams compiled output buffer to a caller destination |
|   [2]   | `GetInitializerLocationDelegate`   | initializer delegate | maps an initializer to an external storage location    |

[ENTRYPOINT_SCOPE]: `OrtModelCompilationOptions` decompile-verified signatures
- source: `Microsoft.ML.OnnxRuntime.Managed` 1.27.0 decompile
- rail: model-lane#COMPILED_EP_CONTEXT
- consumer: `model-lane#COMPILED_EP_CONTEXT`

| [INDEX] | [MEMBER]                                                                  | [SIGNATURE]                                                                                                                                                                                    |
| :-----: | :------------------------------------------------------------------------ | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `OrtModelCompilationOptions.ctor`                                         | `OrtModelCompilationOptions(SessionOptions sessionOptions)`                                                                                                                                    |
|   [2]   | `OrtModelCompilationOptions.CompileModel`                                 | `void CompileModel()`                                                                                                                                                                          |
|   [3]   | `OrtModelCompilationOptions.SetInputModelPath`                            | `void SetInputModelPath(string path)`                                                                                                                                                          |
|   [4]   | `OrtModelCompilationOptions.SetInputModelFromBuffer`                      | `void SetInputModelFromBuffer(byte[] buffer)`                                                                                                                                                  |
|   [5]   | `OrtModelCompilationOptions.SetOutputModelPath`                           | `void SetOutputModelPath(string path)`                                                                                                                                                         |
|   [6]   | `OrtModelCompilationOptions.SetOutputModelExternalInitializersFile`       | `void SetOutputModelExternalInitializersFile(string filePath, ulong threshold)`                                                                                                                |
|   [7]   | `OrtModelCompilationOptions.SetEpContextEmbedMode`                        | `void SetEpContextEmbedMode(bool embed)`                                                                                                                                                       |
|   [8]   | `OrtModelCompilationOptions.SetFlags`                                     | `void SetFlags(OrtCompileApiFlags flags)`                                                                                                                                                      |
|   [9]   | `OrtModelCompilationOptions.SetEpContextBinaryInformation`                | `void SetEpContextBinaryInformation(string outputDirectory, string modelName)`                                                                                                                 |
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

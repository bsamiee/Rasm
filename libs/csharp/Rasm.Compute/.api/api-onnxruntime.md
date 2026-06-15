# [RASM_COMPUTE_API_ONNXRUNTIME]

`Microsoft.ML.OnnxRuntime` supplies model session execution, native runtime
assets, tensor value binding, run options, metadata inspection, custom operators,
and execution-provider selection for Compute model rails.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime`
- package: `Microsoft.ML.OnnxRuntime`
- assembly: `Microsoft.ML.OnnxRuntime`
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

| [INDEX] | [SYMBOL]                    | [PACKAGE_ROLE]  | [CAPABILITY]             |
| :-----: | :-------------------------- | :-------------- | :----------------------- |
|   [1]   | `ModelMetadata`             | model metadata  | describes model          |
|   [2]   | `NodeMetadata`              | node metadata   | describes model nodes    |
|   [3]   | `OrtTensorTypeAndShapeInfo` | tensor metadata | describes tensor shape   |
|   [4]   | `DenseTensor<T>`            | tensor value    | carries typed tensors    |
|   [5]   | `OrtIoBinding`              | binding root    | binds inputs and outputs |
|   [6]   | `PrePackedWeightsContainer` | weights cache   | shares packed weights    |
|   [7]   | `FixedBufferOnnxValue`      | buffer value    | binds fixed memory       |
|   [8]   | `OrtLoraAdapter`            | adapter value   | binds LoRA adapter       |

[PUBLIC_TYPE_SCOPE]: environment, threading, and provider-policy contracts
- rail: model

| [INDEX] | [SYMBOL]                        | [PACKAGE_ROLE]      | [CAPABILITY]                                                                                                  |
| :-----: | :------------------------------ | :------------------ | :------------------------------------------------------------------------------------------------------------ |
|   [1]   | `EnvironmentCreationOptions`    | boot options struct | fields `logId`, `logLevel`, `threadOptions`                                                                   |
|   [2]   | `OrtThreadingOptions`           | global thread pool  | `GlobalIntraOpNumThreads`, `GlobalInterOpNumThreads`, `GlobalSpinControl`                                     |
|   [3]   | `ExecutionProviderDevicePolicy` | device-policy enum  | `DEFAULT`, `PREFER_CPU`, `PREFER_NPU`, `PREFER_GPU`, `MAX_PERFORMANCE`, `MAX_EFFICIENCY`, `MIN_OVERALL_POWER` |
|   [4]   | `GraphOptimizationLevel`        | optimization enum   | session graph optimization incl. `ORT_ENABLE_ALL`                                                             |
|   [5]   | `OrtLoggingLevel`               | logging enum        | boot log severity                                                                                             |
|   [6]   | `OrtAllocator`                  | allocator handle    | owns native allocation scope                                                                                  |
|   [7]   | `IDisposableReadOnlyCollection` | result collection   | disposable native result set                                                                                  |
|   [8]   | `BFloat16`                      | readonly struct     | CLR carrier for the bfloat16 element type                                                                     |

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

[ENTRYPOINT_SCOPE]: value and provider operations
- rail: model

| [INDEX] | [SURFACE]                                         | [CALL_SHAPE] | [CAPABILITY]           |
| :-----: | :------------------------------------------------ | :----------- | :--------------------- |
|   [1]   | `OrtValue.CreateTensorValueFromMemory`            | factory call | binds tensor memory    |
|   [2]   | `CreateTensorValueFromSystemNumericsTensorObject` | factory call | binds tensor object    |
|   [3]   | `CreateFromTensor`                                | factory call | creates named value    |
|   [4]   | `CreateFromOrtValue`                              | factory call | wraps native value     |
|   [5]   | `AppendExecutionProvider`                         | option call  | selects provider       |
|   [6]   | `AppendExecutionProvider_ROCm`                    | option call  | selects provider       |
|   [7]   | `RegisterCustomOpLibrary`                         | option call  | loads custom operators |
|   [8]   | `RegisterCustomOpLibraryV2`                       | option call  | loads custom operators |
|   [9]   | `RegisterOrtExtensions`                           | option call  | loads extension ops    |
|  [10]   | `EnableMemoryPattern`                             | option call  | enables memory reuse   |
|  [11]   | `RunOptions.AddRunConfigEntry`                    | option call  | sets run config entry  |

[ENTRYPOINT_SCOPE]: environment, session-policy, and run-policy operations
- rail: model

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]      | [CAPABILITY]                                                                              |
| :-----: | :---------------------------------------------- | :---------------- | :---------------------------------------------------------------------------------------- |
|   [1]   | `OrtEnv.IsCreated`                              | static property   | reports environment creation                                                              |
|   [2]   | `OrtEnv.Instance`                               | static call       | returns the singleton environment                                                         |
|   [3]   | `OrtEnv.CreateInstanceWithOptions`              | static call       | boots with `ref EnvironmentCreationOptions`                                               |
|   [4]   | `OrtEnv.DisableTelemetryEvents`                 | instance call     | silences runtime telemetry                                                                |
|   [5]   | `OrtEnv.GetVersionString`                       | instance call     | reports the native runtime version                                                        |
|   [6]   | `OrtEnv.GetAvailableProviders`                  | instance call     | returns `string[]` provider names incl. `CoreMLExecutionProvider`, `CPUExecutionProvider` |
|   [7]   | `SessionOptions.AddSessionConfigEntry`          | option call       | sets string config entry                                                                  |
|   [8]   | `SessionOptions.AddFreeDimensionOverrideByName` | option call       | binds symbolic dimension value                                                            |
|   [9]   | `SessionOptions.DisablePerSessionThreads`       | option call       | routes sessions onto the global pool                                                      |
|  [10]   | `SessionOptions.SetEpSelectionPolicy`           | option call       | applies `ExecutionProviderDevicePolicy`                                                   |
|  [11]   | `SessionOptions.EnableProfiling`                | option property   | enables chrome-trace profiling                                                            |
|  [12]   | `SessionOptions.ProfileOutputPathPrefix`        | option property   | sets profile artifact prefix                                                              |
|  [13]   | `SessionOptions.GraphOptimizationLevel`         | option property   | sets graph optimization level                                                             |
|  [14]   | `OrtValue.CreateFromStringTensor`               | factory call      | binds `Tensor<string>` boundary input                                                     |
|  [15]   | `OrtValue.CreateTensorWithEmptyStrings`         | factory call      | allocates string output slots                                                             |
|  [16]   | `RunOptions.Terminate`                          | run property      | one-way cancellation latch                                                                |
|  [17]   | `RunOptions.AddActiveLoraAdapter`               | run call          | attaches `OrtLoraAdapter`                                                                 |
|  [18]   | `OrtLoraAdapter.Create`                         | factory call      | loads adapter from path or bytes                                                          |
|  [19]   | `ModelMetadata.Version`                         | metadata property | `long` graph version                                                                      |
|  [20]   | `NodeMetadata.ElementDataType`                  | metadata property | `TensorElementType` of the slot                                                           |
|  [21]   | `NodeMetadata.Dimensions`                       | metadata property | `int[]` fixed dimensions                                                                  |
|  [22]   | `NodeMetadata.SymbolicDimensions`               | metadata property | `string[]` free-dimension names                                                           |
|  [23]   | `OrtThreadingOptions.GlobalIntraOpNumThreads`   | set-only property  | `int` global intra-op thread count                                                       |
|  [24]   | `OrtThreadingOptions.GlobalInterOpNumThreads`   | set-only property  | `int` global inter-op thread count                                                       |
|  [25]   | `OrtThreadingOptions.GlobalSpinControl`         | set-only property  | `bool` spin flag, `false` no-spin/low-CPU, `true` spin/low-latency                       |
|  [26]   | `OrtThreadingOptions`                           | parameterless ctor | constructs a global thread-pool options handle                                           |

## [4]-[CONFIG_KEYS]

[CONFIG_KEY_SCOPE]: session-options config-entry keys
- source: `build/native/include/onnxruntime_session_options_config_keys.h`
- rail: model

| [INDEX] | [KEY_STRING]            | [SETTER]                          | [VALUE_DOMAIN]                            |
| :-----: | :---------------------- | :-------------------------------- | :---------------------------------------- |
|   [1]   | `ep.context_enable`     | `SessionOptions.AddSessionConfigEntry` | `0` / `1`                            |
|   [2]   | `ep.context_file_path`  | `SessionOptions.AddSessionConfigEntry` | filesystem path string               |
|   [3]   | `ep.share_ep_contexts`  | `SessionOptions.AddSessionConfigEntry` | `0` / `1`                            |

[CONFIG_KEY_SCOPE]: run-options config-entry keys
- source: `build/native/include/onnxruntime_run_options_config_keys.h`
- rail: model

| [INDEX] | [KEY_STRING]                            | [SETTER]                       | [VALUE_DOMAIN]            |
| :-----: | :-------------------------------------- | :----------------------------- | :----------------------- |
|   [1]   | `memory.enable_memory_arena_shrinkage`  | `RunOptions.AddRunConfigEntry` | allocator device string  |

[CONFIG_KEY_SCOPE]: CoreML provider-option keys
- source: CoreML execution-provider option surface
- rail: model

| [INDEX] | [KEY_STRING]                         | [VALUE_DOMAIN]                                            |
| :-----: | :----------------------------------- | :-------------------------------------------------------- |
|   [1]   | `ModelFormat`                        | `MLProgram`, `NeuralNetwork`                              |
|   [2]   | `MLComputeUnits`                     | `ALL`, `CPUOnly`, `CPUAndGPU`, `CPUAndNeuralEngine`       |
|   [3]   | `RequireStaticInputShapes`           | `0`, `1`                                                  |
|   [4]   | `EnableOnSubgraphs`                  | `0`, `1`                                                  |
|   [5]   | `SpecializationStrategy`             | `Default`, `FastPrediction`                              |
|   [6]   | `ProfileComputePlan`                 | `0`, `1`                                                  |
|   [7]   | `AllowLowPrecisionAccumulationOnGPU` | `0`, `1`                                                  |
|   [8]   | `ModelCacheDirectory`                | cache-directory path string                              |

## [5]-[IMPLEMENTATION_LAW]

[MODEL_SESSION]:
- namespace: `Microsoft.ML.OnnxRuntime`
- session root: `InferenceSession`
- policy root: `SessionOptions`
- run root: `RunOptions`
- metadata root: model, node, tensor, and element classifiers
- lifetime: native handles release through deterministic disposal

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

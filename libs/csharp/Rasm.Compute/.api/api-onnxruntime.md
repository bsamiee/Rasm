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

Provider names include `CoreMLExecutionProvider` and `CPUExecutionProvider`; threading options carry typed global thread and spin settings.

[ENVIRONMENT_OPERATIONS]:
| [INDEX] | [SURFACE]                          | [CALL_SHAPE]    | [CAPABILITY]                       |
| :-----: | :--------------------------------- | :-------------- | :--------------------------------- |
|   [1]   | `OrtEnv.IsCreated`                 | static property | reports environment creation       |
|   [2]   | `OrtEnv.Instance`                  | static call     | returns the singleton environment  |
|   [3]   | `OrtEnv.CreateInstanceWithOptions` | static call     | boots with environment options     |
|   [4]   | `OrtEnv.DisableTelemetryEvents`    | instance call   | silences runtime telemetry         |
|   [5]   | `OrtEnv.GetVersionString`          | instance call   | reports the native runtime version |
|   [6]   | `OrtEnv.GetAvailableProviders`     | instance call   | returns provider names             |

[SESSION_POLICY_OPERATIONS]:
| [INDEX] | [SURFACE]                                       | [CALL_SHAPE]    | [CAPABILITY]                         |
| :-----: | :---------------------------------------------- | :-------------- | :----------------------------------- |
|   [1]   | `SessionOptions.AddSessionConfigEntry`          | option call     | sets string config entry             |
|   [2]   | `SessionOptions.AddFreeDimensionOverrideByName` | option call     | binds symbolic dimension value       |
|   [3]   | `SessionOptions.DisablePerSessionThreads`       | option call     | routes sessions onto the global pool |
|   [4]   | `SessionOptions.SetEpSelectionPolicy`           | option call     | applies provider device policy       |
|   [5]   | `SessionOptions.EnableProfiling`                | option property | enables chrome-trace profiling       |
|   [6]   | `SessionOptions.ProfileOutputPathPrefix`        | option property | sets profile artifact prefix         |
|   [7]   | `SessionOptions.GraphOptimizationLevel`         | option property | sets graph optimization level        |

[VALUE_RUN_OPERATIONS]:
| [INDEX] | [SURFACE]                               | [CALL_SHAPE] | [CAPABILITY]                     |
| :-----: | :-------------------------------------- | :----------- | :------------------------------- |
|   [1]   | `OrtValue.CreateFromStringTensor`       | factory call | binds string tensor input        |
|   [2]   | `OrtValue.CreateTensorWithEmptyStrings` | factory call | allocates string output slots    |
|   [3]   | `RunOptions.Terminate`                  | run property | one-way cancellation latch       |
|   [4]   | `RunOptions.AddActiveLoraAdapter`       | run call     | attaches `OrtLoraAdapter`        |
|   [5]   | `OrtLoraAdapter.Create`                 | factory call | loads adapter from path or bytes |

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

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]    | [CAPABILITY]                                                                   |
| :-----: | :--------------------------------------- | :-------------- | :----------------------------------------------------------------------------- |
|   [1]   | `InferenceSession.CreateIoBinding`       | factory call    | returns an `OrtIoBinding` bound to the session                                 |
|   [2]   | `InferenceSession.RunWithBinding`        | bound-run call  | `(RunOptions, OrtIoBinding)` executes a pre-bound inference                    |
|   [3]   | `OrtIoBinding.BindInput`                 | bind call       | `(string, OrtValue)` binds a named input value                                 |
|   [4]   | `OrtIoBinding.BindInput`                 | bind call       | `(string, TensorElementType, long[], OrtMemoryAllocation)` binds device input  |
|   [5]   | `OrtIoBinding.BindOutput`                | bind call       | `(string, OrtValue)` binds a named output value                                |
|   [6]   | `OrtIoBinding.BindOutput`                | bind call       | `(string, TensorElementType, long[], OrtMemoryAllocation)` binds device output |
|   [7]   | `OrtIoBinding.BindOutputToDevice`        | bind call       | `(string, OrtMemoryInfo)` binds an output to a device allocator                |
|   [8]   | `OrtIoBinding.SynchronizeBoundInputs`    | sync call       | flushes pending bound input transfers                                          |
|   [9]   | `OrtIoBinding.SynchronizeBoundOutputs`   | sync call       | flushes pending bound output transfers                                         |
|  [10]   | `OrtIoBinding.GetOutputNames`            | binding read    | `string[]` returns bound output names                                          |
|  [11]   | `OrtIoBinding.GetOutputValues`           | binding read    | `IDisposableReadOnlyCollection<OrtValue>` returns bound output values          |
|  [12]   | `OrtIoBinding.ClearBoundInputs`          | reset call      | clears all bound inputs                                                        |
|  [13]   | `OrtIoBinding.ClearBoundOutputs`         | reset call      | clears all bound outputs                                                       |
|  [14]   | `OrtValue.CreateTensorValueWithData`     | factory call    | `(OrtMemoryInfo, TensorElementType, long[], nint, long)` binds device memory   |
|  [15]   | `OrtValue.CreateAllocatedTensorValue`    | factory call    | `(OrtAllocator, TensorElementType, long[])` allocates an output tensor         |
|  [16]   | `OrtValue.GetTensorDataAsSpan<T>`        | span read       | `ReadOnlySpan<T>` reads bound tensor data                                      |
|  [17]   | `OrtValue.GetTensorMutableDataAsSpan<T>` | span write      | `Span<T>` writes bound tensor data                                             |
|  [18]   | `OrtMemoryInfo.DefaultInstance`          | static property | shared CPU `OrtMemoryInfo` for default-device binding                          |
|  [19]   | `OrtMemoryInfo`                          | ctor            | `(string, OrtAllocatorType, int, OrtMemType)` builds a device descriptor       |

## [4]-[CONFIG_KEYS]

[CONFIG_KEY_SCOPE]: session-options config-entry keys
- source: `build/native/include/onnxruntime_session_options_config_keys.h`
- rail: model

| [INDEX] | [KEY_STRING]           | [SETTER]                               | [VALUE_DOMAIN]         |
| :-----: | :--------------------- | :------------------------------------- | :--------------------- |
|   [1]   | `ep.context_enable`    | `SessionOptions.AddSessionConfigEntry` | `0` / `1`              |
|   [2]   | `ep.context_file_path` | `SessionOptions.AddSessionConfigEntry` | filesystem path string |
|   [3]   | `ep.share_ep_contexts` | `SessionOptions.AddSessionConfigEntry` | `0` / `1`              |

[CONFIG_KEY_SCOPE]: run-options config-entry keys
- source: `build/native/include/onnxruntime_run_options_config_keys.h`
- rail: model

| [INDEX] | [KEY_STRING]                           | [SETTER]                       | [VALUE_DOMAIN]          |
| :-----: | :------------------------------------- | :----------------------------- | :---------------------- |
|   [1]   | `memory.enable_memory_arena_shrinkage` | `RunOptions.AddRunConfigEntry` | allocator device string |

[CONFIG_KEY_SCOPE]: CoreML provider-option keys
- source: CoreML execution-provider option surface
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
- source: `Microsoft.ML.OnnxRuntime` 1.26.0 managed assembly — `SessionOptions` decompile
- rail: model-lane#GENERATIVE_RUN

| [INDEX] | [MEMBER]                    | [SIGNATURE]                                                                                                                                                                                                | [USED_BY]                 | [EVIDENCE]       |
| :-----: | :-------------------------- | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | :------------------------ | :--------------- |
|   [1]   | `RegisterCustomOpLibrary`   | `void RegisterCustomOpLibrary(string libraryPath)`                                                                                                                                                         | model-lane#GENERATIVE_RUN | decompile 1.26.0 |
|   [2]   | `RegisterCustomOpLibraryV2` | `void RegisterCustomOpLibraryV2(string libraryPath, out nint libraryHandle)`                                                                                                                               | model-lane#GENERATIVE_RUN | decompile 1.26.0 |
|   [3]   | `RegisterOrtExtensions`     | `void RegisterOrtExtensions()` — calls `OrtExtensionsNativeMethods.RegisterCustomOps`; throws `OnnxRuntimeException(ErrorCode.NoSuchFile)` if `Microsoft.ML.OnnxRuntime.Extensions` native asset is absent | model-lane#GENERATIVE_RUN | decompile 1.26.0 |

[REGISTRATION_LAW]:
- `RegisterCustomOpLibrary(path)` maps to `OrtRegisterCustomOpsLibrary_V2` in the C API (load and register from path, no handle returned).
- `RegisterCustomOpLibraryV2(path, out nint)` maps to `OrtRegisterCustomOpsLibrary` (load and register, returning a native handle for explicit unloading).
- `RegisterOrtExtensions()` is a convenience wrapper that loads the `libortextensions` native asset shipped by `Microsoft.ML.OnnxRuntime.Extensions`; there is no separate public `OrtExtensions` class — `OrtExtensionsNativeMethods` is internal.
- `OrtExtensions.RegisterCustomOps` does not exist as a public API in either the ORT managed assembly or the Extensions package; the correct entry point is `SessionOptions.RegisterOrtExtensions()`.

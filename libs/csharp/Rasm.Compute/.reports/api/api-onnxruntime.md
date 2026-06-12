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
|   [3]   | `TensorTypeAndShapeInfo`    | tensor metadata | describes tensor shape   |
|   [4]   | `OrtValueTensor<T>`         | tensor value    | carries typed tensors    |
|   [5]   | `OrtIoBinding`              | binding root    | binds inputs and outputs |
|   [6]   | `PrePackedWeightsContainer` | weights cache   | shares packed weights    |
|   [7]   | `FixedBufferOnnxValue`      | buffer value    | binds fixed memory       |
|   [8]   | `OrtLoraAdapter`            | adapter value   | binds LoRA adapter       |

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
|  [11]   | `RunOptionsSetSyncStream`                         | option call  | binds sync stream      |

## [4]-[IMPLEMENTATION_LAW]

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

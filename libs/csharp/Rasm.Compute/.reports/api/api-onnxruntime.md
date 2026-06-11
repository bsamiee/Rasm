# [RASM_COMPUTE_API_ONNXRUNTIME]

`Microsoft.ML.OnnxRuntime` supplies model sessions, tensor inputs, run options, Ort values, and inference execution.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime`
- package: `Microsoft.ML.OnnxRuntime`
- assembly: `Microsoft.ML.OnnxRuntime`
- namespace: `Microsoft.ML.OnnxRuntime`
- asset: runtime library
- rail: model

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: ONNX family
- rail: model

| [INDEX] | [SYMBOL]                   | [PACKAGE_ROLE]      | [CAPABILITY]         |
| :-----: | :------------------------- | :------------------ | :------------------- |
|   [1]   | `InferenceSession`         | model session       | executes model run   |
|   [2]   | `SessionOptions`           | policy object       | carries policy input |
|   [3]   | `RunOptions`               | policy object       | carries policy input |
|   [4]   | `OrtValue`                 | native tensor value | executes model run   |
|   [5]   | `NamedOnnxValue`           | named input value   | executes model run   |
|   [6]   | `Tensor<T>`                | memory shape        | bounds payload shape |
|   [7]   | `DenseTensor<T>`           | memory shape        | bounds payload shape |
|   [8]   | `DisposableNamedOnnxValue` | owned output value  | executes model run   |
|   [9]   | `OrtMemoryInfo`            | memory shape        | bounds payload shape |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: inference operations
- rail: model

| [INDEX] | [SURFACE]                                         | [CALL_SHAPE]    | [CAPABILITY]              |
| :-----: | :------------------------------------------------ | :-------------- | :------------------------ |
|   [1]   | `Run`                                             | operation call  | executes operation        |
|   [2]   | `RunAsync`                                        | async operation | executes async work       |
|   [3]   | `CreateTensorValueFromMemory`                     | factory call    | creates configured handle |
|   [4]   | `CreateTensorValueFromSystemNumericsTensorObject` | factory call    | creates configured handle |
|   [5]   | `AppendExecutionProvider`                         | provider option | selects provider          |
|   [6]   | `RegisterCustomOpLibrary`                         | session option  | loads custom ops          |
|   [7]   | `RegisterCustomOpLibraryV2`                       | session option  | loads custom ops          |
|   [8]   | `RegisterOrtExtensions`                           | session option  | loads custom ops          |
|   [9]   | `EnableMemoryPattern`                             | memory option   | enables memory reuse      |
|  [10]   | `Dispose`                                         | operation call  | executes operation        |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime`
- Owns: model inference
- Accept: model lanes emit load and inference receipts
- Reject: ML.NET training pipeline

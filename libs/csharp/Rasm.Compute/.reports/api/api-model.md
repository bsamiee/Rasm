# [RASM_COMPUTE_API_MODEL]

Model APIs supply ONNX Runtime inference, extension operations, model identity, provider options, native-load receipts, and baseline comparison.

## [1]-[SURFACES]

This table is a lookup by model package.

| [INDEX] | [PACKAGE]                          | [ASSEMBLY]                          | [LOCAL_RAIL] |
| :-----: | :--------------------------------- | :---------------------------------- | :----------- |
|   [1]   | `Microsoft.ML.OnnxRuntime`         | `Microsoft.ML.OnnxRuntime`          | model        |
|   [2]   | `Microsoft.ML.OnnxRuntime.Extensions` | `Microsoft.ML.OnnxRuntime.Extensions` | model     |

## [2]-[API_LOCATORS]

This table is a lookup by assembly.

| [INDEX] | [ASSEMBLY]                          | [NAMESPACE]                       | [USING]                            | [API_LOCATOR] |
| :-----: | :---------------------------------- | :-------------------------------- | :--------------------------------- | :------------ |
|   [1]   | `Microsoft.ML.OnnxRuntime`          | `Microsoft.ML.OnnxRuntime`        | `Microsoft.ML.OnnxRuntime`         | `.cache/nuget/packages/microsoft.ml.onnxruntime/` |
|   [2]   | `Microsoft.ML.OnnxRuntime.Extensions` | `Microsoft.ML.OnnxRuntime.Extensions` | `Microsoft.ML.OnnxRuntime.Extensions` | `.cache/nuget/packages/microsoft.ml.onnxruntime.extensions/` |

## [3]-[CAPABILITIES]

This table is a lookup by type family.

| [INDEX] | [TYPE_FAMILY]       | [ENTRY_SURFACE]             | [LOCAL_RAIL] |
| :-----: | :------------------ | :-------------------------- | :----------- |
|   [1]   | `InferenceSession`  | model execution             | model        |
|   [2]   | `SessionOptions`    | provider and thread policy  | model        |
|   [3]   | tensor value APIs   | model input and output      | model        |
|   [4]   | extension operators | custom pre/post operations  | model        |
|   [5]   | native library load | runtime identity receipt    | model        |

## [4]-[REJECTED]

This table is a lookup by rejected package.

| [INDEX] | [REJECT]          | [LOCAL_RAIL] | [REASON]                 |
| :-----: | :---------------- | :----------- | :----------------------- |
|   [1]   | ML.NET `MLContext` | model       | training pipeline owner  |
|   [2]   | TorchSharp        | model        | second model stack       |

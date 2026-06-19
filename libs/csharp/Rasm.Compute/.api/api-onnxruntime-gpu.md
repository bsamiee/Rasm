# [RASM_COMPUTE_API_ONNXRUNTIME_GPU]

`Microsoft.ML.OnnxRuntime.Gpu` supplies the CUDA and TensorRT execution-provider native runtime libraries for the ONNX model lane. The package carries no managed assembly: it aggregates `Microsoft.ML.OnnxRuntime.Managed` plus the platform native sub-packages `Microsoft.ML.OnnxRuntime.Gpu.Windows` and `Microsoft.ML.OnnxRuntime.Gpu.Linux`, and the managed `AppendExecutionProvider_CUDA` and `AppendExecutionProvider_Tensorrt` entrypoints bind these native providers from `Microsoft.ML.OnnxRuntime`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime.Gpu`
- package: `Microsoft.ML.OnnxRuntime.Gpu`
- assembly: native/build assets only — no managed public assembly in 1.27.0
- namespace: package assets
- asset: native CUDA and TensorRT execution-provider runtimes
- rail: model

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: aggregator dependencies
- rail: model

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]                       |
| :-----: | :---------------------------------------- | :----------------- | :--------------------------------- |
|   [1]   | `Microsoft.ML.OnnxRuntime.Managed` 1.27.0 | managed dependency | supplies the managed append API    |
|   [2]   | `Microsoft.ML.OnnxRuntime.Gpu.Windows`    | native dependency  | supplies win-x64 GPU EP runtimes   |
|   [3]   | `Microsoft.ML.OnnxRuntime.Gpu.Linux`      | native dependency  | supplies linux-x64 GPU EP runtimes |

[PACKAGE_ASSET_SCOPE]: native execution-provider runtimes
- rail: model

| [INDEX] | [SYMBOL]                                           | [PACKAGE_ROLE] | [CAPABILITY]                  |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------- |
|   [1]   | `onnxruntime_providers_cuda.dll` (win-x64)         | native asset   | loads the CUDA EP             |
|   [2]   | `onnxruntime_providers_tensorrt.dll` (win-x64)     | native asset   | loads the TensorRT EP         |
|   [3]   | `onnxruntime_providers_shared.dll` (win-x64)       | native asset   | bridges shared provider entry |
|   [4]   | `libonnxruntime_providers_cuda.so` (linux-x64)     | native asset   | loads the CUDA EP             |
|   [5]   | `libonnxruntime_providers_tensorrt.so` (linux-x64) | native asset   | loads the TensorRT EP         |
|   [6]   | `libonnxruntime_providers_shared.so` (linux-x64)   | native asset   | bridges shared provider entry |
|   [7]   | `buildTransitive/native/*.props`                   | MSBuild import | declares native copy          |
|   [8]   | `buildTransitive/native/*.targets`                 | MSBuild import | copies native assets          |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: GPU execution-provider append operations
- rail: model
- note: all are instance methods on `SessionOptions` in `Microsoft.ML.OnnxRuntime`

| [INDEX] | [SURFACE]                                                      | [CALL_SHAPE] | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------- | :----------- | :-------------------------------------------- |
|   [1]   | `AppendExecutionProvider_CUDA(int deviceId = 0)`               | option call  | CUDA EP by device index                       |
|   [2]   | `AppendExecutionProvider_CUDA(OrtCUDAProviderOptions)`         | option call  | CUDA EP with full provider-options struct     |
|   [3]   | `AppendExecutionProvider_Tensorrt(int deviceId = 0)`           | option call  | TensorRT EP by device index                   |
|   [4]   | `AppendExecutionProvider_Tensorrt(OrtTensorRTProviderOptions)` | option call  | TensorRT EP with full provider-options struct |

[ENTRYPOINT_SCOPE]: provider-options structs
- rail: model

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]      | [CAPABILITY]                            |
| :-----: | :--------------------------- | :------------------ | :-------------------------------------- |
|   [1]   | `OrtCUDAProviderOptions`     | CUDA options handle | configures CUDA provider key-values     |
|   [2]   | `OrtTensorRTProviderOptions` | TensorRT options    | configures TensorRT provider key-values |

[ENTRYPOINT_SCOPE]: decompile-verified GPU append and provider-options signatures
- source: `Microsoft.ML.OnnxRuntime.Managed` 1.27.0 decompile
- rail: model-lane#GENERATIVE_RUN

| [INDEX] | [MEMBER]                                   | [SIGNATURE]                                                                            |
| :-----: | :----------------------------------------- | :------------------------------------------------------------------------------------- |
|   [1]   | `AppendExecutionProvider_CUDA`             | `void AppendExecutionProvider_CUDA(int deviceId = 0)`                                  |
|   [2]   | `AppendExecutionProvider_CUDA`             | `void AppendExecutionProvider_CUDA(OrtCUDAProviderOptions cudaProviderOptions)`        |
|   [3]   | `AppendExecutionProvider_Tensorrt`         | `void AppendExecutionProvider_Tensorrt(int deviceId = 0)`                              |
|   [4]   | `AppendExecutionProvider_Tensorrt`         | `void AppendExecutionProvider_Tensorrt(OrtTensorRTProviderOptions trtProviderOptions)` |
|   [5]   | `OrtCUDAProviderOptions.ctor`              | `OrtCUDAProviderOptions()`                                                             |
|   [6]   | `OrtCUDAProviderOptions.GetOptions`        | `string GetOptions()`                                                                  |
|   [7]   | `OrtCUDAProviderOptions.UpdateOptions`     | `void UpdateOptions(Dictionary<string, string> providerOptions)`                       |
|   [8]   | `OrtTensorRTProviderOptions.ctor`          | `OrtTensorRTProviderOptions()`                                                         |
|   [9]   | `OrtTensorRTProviderOptions.GetOptions`    | `string GetOptions()`                                                                  |
|  [10]   | `OrtTensorRTProviderOptions.UpdateOptions` | `void UpdateOptions(Dictionary<string, string> providerOptions)`                       |
|  [11]   | `OrtTensorRTProviderOptions.GetDeviceId`   | `int GetDeviceId()`                                                                    |

## [4]-[IMPLEMENTATION_LAW]

[GPU_PROVIDER_ASSETS]:
- package role: CUDA and TensorRT native execution-provider bundle
- managed entry: `SessionOptions.AppendExecutionProvider_CUDA` and `AppendExecutionProvider_Tensorrt` (defined in `Microsoft.ML.OnnxRuntime`, not in this package)
- native root: `onnxruntime_providers_cuda` and `onnxruntime_providers_tensorrt` — placed by the platform sub-packages' MSBuild props/targets
- provider bridge: `onnxruntime_providers_shared` loads the GPU provider into the base runtime

[PROVIDER_OPTIONS]:
- `OrtCUDAProviderOptions` and `OrtTensorRTProviderOptions` are `SafeHandle` wrappers over native option blobs
- `UpdateOptions(Dictionary<string, string>)` writes provider key-values; `GetOptions()` reads them back as a string
- `OrtTensorRTProviderOptions.GetDeviceId()` reports the bound device index

[PLATFORM_COVERAGE]:
- runtime identifiers: win-x64 and linux-x64 only; the package ships no osx-arm64 GPU EP runtime
- osx-arm64 never loads the CUDA or TensorRT EP — the native provider libraries do not exist for that RID, so GPU append calls fall back to CPU or CoreML on macOS
- GPU model execution is admitted only on the Windows and Linux x64 substrate; macOS routes accelerated inference through the CoreML EP in `Microsoft.ML.OnnxRuntime`

[LOCAL_ADMISSION]:
- GPU execution enters through ONNX Runtime session provider append calls before session construction.
- Provider selection is model-rail policy data; CUDA and TensorRT append order determines fallback priority.
- Native GPU provider asset presence is part of model evidence before GPU model execution is admitted.
- The osx-arm64 design record means macOS model lanes never assume GPU EP availability.

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime.Gpu`
- Owns: CUDA and TensorRT native execution-provider assets
- Accept: win-x64 and linux-x64 accelerated model inference
- Reject: osx-arm64 GPU EP load; managed-surface duplication of the `Microsoft.ML.OnnxRuntime` append API

# [RASM_COMPUTE_API_ONNXRUNTIME_GPU]

`Microsoft.ML.OnnxRuntime.Gpu` supplies the CUDA and TensorRT execution-provider native runtime libraries for the ONNX model lane. The package carries no managed assembly: it aggregates `Microsoft.ML.OnnxRuntime.Managed` plus the platform native sub-packages `Microsoft.ML.OnnxRuntime.Gpu.Windows` and `Microsoft.ML.OnnxRuntime.Gpu.Linux`, and the managed `AppendExecutionProvider_CUDA` and `AppendExecutionProvider_Tensorrt` entrypoints bind these native providers from `Microsoft.ML.OnnxRuntime`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime.Gpu`
- package: `Microsoft.ML.OnnxRuntime.Gpu`
- assembly: native/build assets only — no managed public assembly in 1.27.0
- namespace: package assets
- asset: native CUDA and TensorRT execution-provider runtimes
- rail: model

## [02]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: aggregator dependencies
- rail: model

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]                       |
| :-----: | :---------------------------------------- | :----------------- | :--------------------------------- |
|  [01]   | `Microsoft.ML.OnnxRuntime.Managed` 1.27.0 | managed dependency | supplies the managed append API    |
|  [02]   | `Microsoft.ML.OnnxRuntime.Gpu.Windows`    | native dependency  | supplies win-x64 GPU EP runtimes   |
|  [03]   | `Microsoft.ML.OnnxRuntime.Gpu.Linux`      | native dependency  | supplies linux-x64 GPU EP runtimes |

[PACKAGE_ASSET_SCOPE]: native execution-provider runtimes
- rail: model

| [INDEX] | [SYMBOL]                                           | [PACKAGE_ROLE] | [CAPABILITY]                  |
| :-----: | :------------------------------------------------- | :------------- | :---------------------------- |
|  [01]   | `onnxruntime_providers_cuda.dll` (win-x64)         | native asset   | loads the CUDA EP             |
|  [02]   | `onnxruntime_providers_tensorrt.dll` (win-x64)     | native asset   | loads the TensorRT EP         |
|  [03]   | `onnxruntime_providers_shared.dll` (win-x64)       | native asset   | bridges shared provider entry |
|  [04]   | `libonnxruntime_providers_cuda.so` (linux-x64)     | native asset   | loads the CUDA EP             |
|  [05]   | `libonnxruntime_providers_tensorrt.so` (linux-x64) | native asset   | loads the TensorRT EP         |
|  [06]   | `libonnxruntime_providers_shared.so` (linux-x64)   | native asset   | bridges shared provider entry |
|  [07]   | `buildTransitive/native/*.props`                   | MSBuild import | declares native copy          |
|  [08]   | `buildTransitive/native/*.targets`                 | MSBuild import | copies native assets          |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: GPU execution-provider append operations
- rail: model
- note: all are instance methods on `SessionOptions` in `Microsoft.ML.OnnxRuntime`

| [INDEX] | [SURFACE]                                                      | [CALL_SHAPE] | [CAPABILITY]                                  |
| :-----: | :------------------------------------------------------------- | :----------- | :-------------------------------------------- |
|  [01]   | `AppendExecutionProvider_CUDA(int deviceId = 0)`               | option call  | CUDA EP by device index                       |
|  [02]   | `AppendExecutionProvider_CUDA(OrtCUDAProviderOptions)`         | option call  | CUDA EP with full provider-options struct     |
|  [03]   | `AppendExecutionProvider_Tensorrt(int deviceId = 0)`           | option call  | TensorRT EP by device index                   |
|  [04]   | `AppendExecutionProvider_Tensorrt(OrtTensorRTProviderOptions)` | option call  | TensorRT EP with full provider-options struct |

[ENTRYPOINT_SCOPE]: provider-options structs
- rail: model

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]      | [CAPABILITY]                            |
| :-----: | :--------------------------- | :------------------ | :-------------------------------------- |
|  [01]   | `OrtCUDAProviderOptions`     | CUDA options handle | configures CUDA provider key-values     |
|  [02]   | `OrtTensorRTProviderOptions` | TensorRT options    | configures TensorRT provider key-values |

[ENTRYPOINT_SCOPE]: decompile-verified GPU append and provider-options signatures
- source: `Microsoft.ML.OnnxRuntime.Managed` 1.27.0 decompile
- rail: model-lane#GENERATIVE_RUN

| [INDEX] | [MEMBER]                                   | [SIGNATURE]                                                                            |
| :-----: | :----------------------------------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `AppendExecutionProvider_CUDA`             | `void AppendExecutionProvider_CUDA(int deviceId = 0)`                                  |
|  [02]   | `AppendExecutionProvider_CUDA`             | `void AppendExecutionProvider_CUDA(OrtCUDAProviderOptions cudaProviderOptions)`        |
|  [03]   | `AppendExecutionProvider_Tensorrt`         | `void AppendExecutionProvider_Tensorrt(int deviceId = 0)`                              |
|  [04]   | `AppendExecutionProvider_Tensorrt`         | `void AppendExecutionProvider_Tensorrt(OrtTensorRTProviderOptions trtProviderOptions)` |
|  [05]   | `OrtCUDAProviderOptions.ctor`              | `OrtCUDAProviderOptions()`                                                             |
|  [06]   | `OrtCUDAProviderOptions.GetOptions`        | `string GetOptions()`                                                                  |
|  [07]   | `OrtCUDAProviderOptions.UpdateOptions`     | `void UpdateOptions(Dictionary<string, string> providerOptions)`                       |
|  [08]   | `OrtTensorRTProviderOptions.ctor`          | `OrtTensorRTProviderOptions()`                                                         |
|  [09]   | `OrtTensorRTProviderOptions.GetOptions`    | `string GetOptions()`                                                                  |
|  [10]   | `OrtTensorRTProviderOptions.UpdateOptions` | `void UpdateOptions(Dictionary<string, string> providerOptions)`                       |
|  [11]   | `OrtTensorRTProviderOptions.GetDeviceId`   | `int GetDeviceId()`                                                                    |

## [04]-[IMPLEMENTATION_LAW]

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

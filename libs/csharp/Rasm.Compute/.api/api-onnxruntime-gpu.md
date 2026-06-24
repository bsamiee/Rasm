# [RASM_COMPUTE_API_ONNXRUNTIME_GPU]

`Microsoft.ML.OnnxRuntime.Gpu` supplies the CUDA and TensorRT execution-provider native runtime libraries for the ONNX model lane. The package carries no managed assembly and ships NO native DLLs of its own: it is a meta/aggregator that depends on `Microsoft.ML.OnnxRuntime.Managed 1.27.0` plus the platform native sub-packages `Microsoft.ML.OnnxRuntime.Gpu.Windows 1.27.0` and `Microsoft.ML.OnnxRuntime.Gpu.Linux 1.27.0` (the native binaries live in the SUB-packages), and the managed `AppendExecutionProvider_CUDA`/`AppendExecutionProvider_Tensorrt` entrypoints plus the `OrtCUDAProviderOptions`/`OrtTensorRTProviderOptions` option handles bind these native providers from `Microsoft.ML.OnnxRuntime`. The catalog GUIDES the `Model/providers#EP_AXIS` `[EP_EXECUTION]` design record: the GPU rows re-enter the `ExecutionProvider` `[SmartEnum<string>]` axis only behind a win-x64/linux-x64 RID that carries the sub-package asset.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime.Gpu`
- package: `Microsoft.ML.OnnxRuntime.Gpu` 1.27.0 (meta/aggregator)
- assembly: native/build assets only — no managed public assembly; the aggregator's own asset tree contains ONLY `buildTransitive/native/*.props`/`*.targets` + `buildTransitive/netstandard2.{0,1}/*.props`/`*.targets` (asset-flow plumbing), with every native binary delegated to the platform sub-packages
- license: MIT
- namespace: package assets (managed surface is `Microsoft.ML.OnnxRuntime`)
- asset: native CUDA and TensorRT execution-provider runtimes (via sub-packages)
- runtime requirement: the CUDA/TensorRT providers require a SYSTEM CUDA toolkit + cuDNN + TensorRT runtime on `PATH`/`LD_LIBRARY_PATH` — the sub-package nuspecs declare NO CUDA/cuDNN/TensorRT NuGet dependency, so those native libraries are a host provisioning concern, not a restored asset
- rail: model

## [02]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: aggregator dependency graph (decompile/nuspec-verified)
- rail: model

| [INDEX] | [SYMBOL]                                          | [PACKAGE_ROLE]     | [CAPABILITY]                       |
| :-----: | :------------------------------------------------ | :----------------- | :--------------------------------- |
|  [01]   | `Microsoft.ML.OnnxRuntime.Managed` 1.27.0         | managed dependency | supplies the managed append API + provider-options handles |
|  [02]   | `Microsoft.ML.OnnxRuntime.Gpu.Windows` 1.27.0     | native dependency  | ships the win-x64 GPU EP runtimes  |
|  [03]   | `Microsoft.ML.OnnxRuntime.Gpu.Linux` 1.27.0       | native dependency  | ships the linux-x64 GPU EP runtimes |
|  [04]   | `buildTransitive/native/*.props` / `*.targets`    | MSBuild import     | flows the sub-package native assets into the consumer output |

[PACKAGE_ASSET_SCOPE]: native execution-provider runtimes (shipped by the SUB-packages, not the aggregator)
- rail: model
- note: `Microsoft.ML.OnnxRuntime.Gpu.Windows` ships the win-x64 set; `Microsoft.ML.OnnxRuntime.Gpu.Linux` ships the linux-x64 set; each sub-package ALSO ships its own base runtime (`onnxruntime.dll`/`libonnxruntime.so`), so the GPU package supersedes the base `Microsoft.ML.OnnxRuntime` win runtime on that RID

| [INDEX] | [SYMBOL]                                           | [SUB-PACKAGE]                          | [CAPABILITY]                  |
| :-----: | :------------------------------------------------- | :------------------------------------- | :---------------------------- |
|  [01]   | `onnxruntime.dll` (win-x64)                        | `Gpu.Windows`                          | GPU-enabled base runtime       |
|  [02]   | `onnxruntime_providers_cuda.dll` (win-x64)         | `Gpu.Windows`                          | loads the CUDA EP             |
|  [03]   | `onnxruntime_providers_tensorrt.dll` (win-x64)     | `Gpu.Windows`                          | loads the TensorRT EP         |
|  [04]   | `onnxruntime_providers_shared.dll` (win-x64)       | `Gpu.Windows`                          | bridges shared provider entry |
|  [05]   | `libonnxruntime.so` (linux-x64)                    | `Gpu.Linux`                            | GPU-enabled base runtime       |
|  [06]   | `libonnxruntime_providers_cuda.so` (linux-x64)     | `Gpu.Linux`                            | loads the CUDA EP             |
|  [07]   | `libonnxruntime_providers_tensorrt.so` (linux-x64) | `Gpu.Linux`                            | loads the TensorRT EP         |
|  [08]   | `libonnxruntime_providers_shared.so` (linux-x64)   | `Gpu.Linux`                            | bridges shared provider entry |

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

[ENTRYPOINT_SCOPE]: provider-options structs (defined in `Microsoft.ML.OnnxRuntime`)
- rail: model

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]      | [CAPABILITY]                            |
| :-----: | :--------------------------- | :------------------ | :-------------------------------------- |
|  [01]   | `OrtCUDAProviderOptions`     | CUDA options handle | `SafeHandle` over a native option blob; `UpdateOptions(Dictionary<string,string>)` / `GetOptions()` round-trips CUDA keys (`device_id`, `gpu_mem_limit`, `cudnn_conv_algo_search`, `arena_extend_strategy`, …) |
|  [02]   | `OrtTensorRTProviderOptions` | TensorRT options    | `SafeHandle`; `UpdateOptions` / `GetOptions()` / `GetDeviceId()` round-trips TensorRT keys (`trt_fp16_enable`, `trt_engine_cache_enable`, `trt_engine_cache_path`, `trt_max_workspace_size`, …) |
|  [03]   | `ProviderOptionsValueHelper` | options parser      | `static StringToDict(string s, Dictionary<string,string>)` parses a serialized `GetOptions()` string back into a key-value dictionary |
|  [04]   | `SessionOptions.MakeSessionOptionWith{Cuda,Tensorrt}Provider` | static factory | one-call session+EP shorthand: `(int deviceId)` or `(OrtCUDAProviderOptions)` / `(OrtTensorRTProviderOptions)` |

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

[STACKING]: this package adds NO managed surface — it is consumed entirely through `Microsoft.ML.OnnxRuntime` —
- the GPU rows re-enter the `Model/providers#EP_AXIS` `ExecutionProvider` `[SmartEnum<string>]` as one row each (`Cuda` → `AppendExecutionProvider_CUDA(0)` or `OrtCUDAProviderOptions`, `TensorRt` → `AppendExecutionProvider_Tensorrt(0)` or `OrtTensorRTProviderOptions`) behind a win-x64/linux-x64 RID gate, with the option blob round-tripped through `UpdateOptions`/`GetOptions` and the option-table hash folded into the `Model/identity#MODEL_IDENTITY` fingerprint exactly as the CPU/CoreML rows do
- `OrtTensorRTProviderOptions` `trt_engine_cache_path` aligns with the `Model/sessions#SESSION_CAPSULE` blob-lane artifact directory so a built TensorRT engine is catalogued inventory, not a stray temp file, mirroring the CoreML `ModelCacheDirectory` binding
- the autoEP path applies unchanged: `OrtEnv.GetEpDevices()` surfaces the GPU device as an `OrtEpDevice` with `HardwareDevice.Type == GPU`, so the device-rank fold and the `GetCompatibilityInfoFromModel`→`GetModelCompatibilityForEpDevices` enum verdict drive GPU warm-start identically to the CPU/CoreML rows

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime.Gpu`
- Owns: CUDA and TensorRT native execution-provider assets (via the Windows/Linux sub-packages)
- Accept: win-x64 and linux-x64 accelerated model inference
- Reject: osx-arm64 GPU EP load; managed-surface duplication of the `Microsoft.ML.OnnxRuntime` append API; a system-CUDA-toolkit NuGet shim (the CUDA/cuDNN/TensorRT native libraries are host provisioning, not a package asset)

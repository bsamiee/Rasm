# [RASM_COMPUTE_API_ONNXRUNTIME_DIRECTML]

`Microsoft.ML.OnnxRuntime.DirectML` supplies the DirectML execution-provider native runtime for the ONNX model lane. The package carries no managed assembly: it depends on `Microsoft.ML.OnnxRuntime.Managed` plus `Microsoft.AI.DirectML`, ships Windows-only native runtimes, and the managed `AppendExecutionProvider_DML` entrypoint binds the DirectML provider from `Microsoft.ML.OnnxRuntime`.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime.DirectML`
- package: `Microsoft.ML.OnnxRuntime.DirectML`
- assembly: native/build assets only — no managed public assembly in 1.24.4
- namespace: package assets
- asset: native DirectML execution-provider runtime
- rail: model

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: package dependencies
- rail: model

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]                    |
| :-----: | :---------------------------------------- | :----------------- | :------------------------------ |
|   [1]   | `Microsoft.ML.OnnxRuntime.Managed` 1.24.4 | managed dependency | supplies the managed append API |
|   [2]   | `Microsoft.AI.DirectML` 1.15.4            | native dependency  | supplies the DirectML runtime   |

[PACKAGE_ASSET_SCOPE]: native execution-provider runtimes
- rail: model

| [INDEX] | [SYMBOL]                                       | [PACKAGE_ROLE] | [CAPABILITY]                     |
| :-----: | :--------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `onnxruntime.dll` (win-x64)                    | native asset   | executes the DML-enabled runtime |
|   [2]   | `onnxruntime_providers_shared.dll` (win-x64)   | native asset   | bridges shared provider entry    |
|   [3]   | `onnxruntime.dll` (win-arm64)                  | native asset   | executes the DML-enabled runtime |
|   [4]   | `onnxruntime_providers_shared.dll` (win-arm64) | native asset   | bridges shared provider entry    |
|   [5]   | `dml_provider_factory.h`                       | native header  | describes the DML provider C API |
|   [6]   | `build/native/*.props`                         | MSBuild import | declares native copy             |
|   [7]   | `build/native/*.targets`                       | MSBuild import | copies native assets             |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DirectML execution-provider append operation
- rail: model
- note: instance method on `SessionOptions` in `Microsoft.ML.OnnxRuntime`

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE] | [CAPABILITY]                |
| :-----: | :---------------------------------------------- | :----------- | :-------------------------- |
|   [1]   | `AppendExecutionProvider_DML(int deviceId = 0)` | option call  | DirectML EP by device index |

[ENTRYPOINT_SCOPE]: decompile-verified DirectML append signature
- source: `Microsoft.ML.OnnxRuntime.Managed` 1.24.4 decompile
- rail: model-lane#GENERATIVE_RUN

| [INDEX] | [MEMBER]                                       | [SIGNATURE]                                                                                                                               |
| :-----: | :--------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------- |
|   [1]   | `AppendExecutionProvider_DML`                  | `void AppendExecutionProvider_DML(int deviceId = 0)`                                                                                      |
|   [2]   | `OrtSessionOptionsAppendExecutionProvider_DML` | `static extern nint OrtSessionOptionsAppendExecutionProvider_DML(nint options, int device_id)` — native P/Invoke backing the managed call |

## [4]-[IMPLEMENTATION_LAW]

[DIRECTML_PROVIDER_ASSETS]:
- package role: DirectML native execution-provider runtime
- managed entry: `SessionOptions.AppendExecutionProvider_DML(int)` (defined in `Microsoft.ML.OnnxRuntime`, not in this package)
- native root: the DML provider is built into `onnxruntime.dll`; `onnxruntime_providers_shared` bridges the provider entry
- backing dependency: `Microsoft.AI.DirectML` supplies the DirectML runtime the EP binds against

[OPTION_SHAPE]:
- the DirectML EP takes only a `deviceId` integer; there is no managed `OrtDmlProviderOptions` struct or dictionary overload in 1.24.4
- device selection and provider configuration beyond the device index route through `SessionOptions` config entries

[PLATFORM_COVERAGE]:
- runtime identifiers: win-x64 and win-arm64 only; the package ships no osx-arm64 or linux runtime
- DirectML is Windows-only; osx-arm64 never loads the DML EP — the native runtime does not exist for that RID
- macOS model lanes route accelerated inference through the CoreML EP in `Microsoft.ML.OnnxRuntime`; DML append calls are admitted only on the Windows substrate

[LOCAL_ADMISSION]:
- DirectML execution enters through ONNX Runtime session provider append calls before session construction.
- Provider selection is model-rail policy data; the DML append device index is explicit at session composition.
- Native DML runtime presence is part of model evidence before DirectML model execution is admitted.
- The Windows-only design record means non-Windows model lanes never assume DML EP availability.

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime.DirectML`
- Owns: DirectML native execution-provider assets
- Accept: win-x64 and win-arm64 accelerated model inference
- Reject: osx-arm64 and linux DML EP load; managed-surface duplication of the `Microsoft.ML.OnnxRuntime` append API

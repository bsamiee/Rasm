# [RASM_COMPUTE_API_ONNXRUNTIME_DIRECTML]

`Microsoft.ML.OnnxRuntime.DirectML` supplies the DirectML execution-provider native runtime for the ONNX model lane. The package carries no managed assembly: it depends on `Microsoft.ML.OnnxRuntime.Managed 1.24.4` plus `Microsoft.AI.DirectML 1.15.4`, ships Windows-only native runtimes (win-x64, win-arm64), and the managed `AppendExecutionProvider_DML` entrypoint binds the DirectML provider from `Microsoft.ML.OnnxRuntime`. The catalog GUIDES the `Model/providers#EP_AXIS` `[EP_EXECUTION]` design record: the `DirectMl` row re-enters the `ExecutionProvider` `[SmartEnum<string>]` axis only behind a win-x64/win-arm64 RID.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime.DirectML`
- package: `Microsoft.ML.OnnxRuntime.DirectML` 1.24.4
- assembly: native/build assets only — no managed public assembly (the managed surface is `Microsoft.ML.OnnxRuntime.Managed`)
- license: MIT
- namespace: package assets (managed surface is `Microsoft.ML.OnnxRuntime`)
- asset: native DirectML execution-provider runtime
- version ceiling: the DirectML NuGet line tops at 1.24.4 while `Microsoft.ML.OnnxRuntime`/`.Gpu` ride 1.27.0 — this is the central manifest's documented HOLD, so the DirectML package pins `.Managed 1.24.4`; a project that combines DirectML with the 1.27.0 base must reconcile the two managed assets at the build (only one `Microsoft.ML.OnnxRuntime.dll` binds), which is why DML is a RID-gated record, not a co-resident provider on the 1.27.0 line
- rail: model

## [02]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: package dependencies (nuspec-verified)
- rail: model

| [INDEX] | [SYMBOL]                                  | [PACKAGE_ROLE]     | [CAPABILITY]                    |
| :-----: | :---------------------------------------- | :----------------- | :------------------------------ |
|  [01]   | `Microsoft.ML.OnnxRuntime.Managed` 1.24.4 | managed dependency | supplies the managed `AppendExecutionProvider_DML` API |
|  [02]   | `Microsoft.AI.DirectML` 1.15.4            | native dependency  | supplies the `DirectML.dll` runtime the EP binds against |

[PACKAGE_ASSET_SCOPE]: native execution-provider runtimes (resolve-verified)
- rail: model

| [INDEX] | [SYMBOL]                                       | [PACKAGE_ROLE] | [CAPABILITY]                     |
| :-----: | :--------------------------------------------- | :------------- | :------------------------------- |
|  [01]   | `runtimes/win-x64/native/onnxruntime.dll`      | native asset   | executes the DML-enabled runtime |
|  [02]   | `runtimes/win-x64/native/onnxruntime.lib`      | native asset   | win-x64 import lib for native linking |
|  [03]   | `runtimes/win-x64/native/onnxruntime_providers_shared.dll` | native asset   | bridges shared provider entry    |
|  [04]   | `runtimes/win-arm64/native/onnxruntime.dll`    | native asset   | executes the DML-enabled runtime |
|  [05]   | `runtimes/win-arm64/native/onnxruntime.lib`    | native asset   | win-arm64 import lib for native linking |
|  [06]   | `runtimes/win-arm64/native/onnxruntime_providers_shared.dll` | native asset   | bridges shared provider entry    |
|  [07]   | `build/native/include/dml_provider_factory.h`  | native header  | describes the DML provider C API (`OrtSessionOptionsAppendExecutionProvider_DML`) |
|  [08]   | `build/native/*.props` / `*.targets`           | MSBuild import | declares + copies native assets by RID |
|  [09]   | `build/netstandard2.0/*` / `build/netstandard2.1/*` | MSBuild import | TFM-specific props/targets variants flowing the native assets to managed consumers |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: DirectML execution-provider append operation
- rail: model
- note: instance method on `SessionOptions` in `Microsoft.ML.OnnxRuntime`

| [INDEX] | [SURFACE]                                       | [CALL_SHAPE] | [CAPABILITY]                |
| :-----: | :---------------------------------------------- | :----------- | :-------------------------- |
|  [01]   | `AppendExecutionProvider_DML(int deviceId = 0)` | option call  | DirectML EP by device index |

[ENTRYPOINT_SCOPE]: decompile-verified DirectML append signature
- source: `Microsoft.ML.OnnxRuntime.Managed` 1.24.4 decompile
- rail: model-lane#GENERATIVE_RUN

| [INDEX] | [MEMBER]                                       | [SIGNATURE]                                                                                                                               |
| :-----: | :--------------------------------------------- | :---------------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `AppendExecutionProvider_DML`                  | `void AppendExecutionProvider_DML(int deviceId = 0)`                                                                                      |
|  [02]   | `OrtSessionOptionsAppendExecutionProvider_DML` | `static extern nint OrtSessionOptionsAppendExecutionProvider_DML(nint options, int device_id)` — native P/Invoke backing the managed call |

## [04]-[IMPLEMENTATION_LAW]

[DIRECTML_PROVIDER_ASSETS]:
- package role: DirectML native execution-provider runtime
- managed entry: `SessionOptions.AppendExecutionProvider_DML(int)` (defined in `Microsoft.ML.OnnxRuntime`, not in this package)
- native root: the DML provider is built into `onnxruntime.dll`; `onnxruntime_providers_shared` bridges the provider entry
- backing dependency: `Microsoft.AI.DirectML` supplies the DirectML runtime the EP binds against

[OPTION_SHAPE]:
- decompile-verified: the DirectML EP takes only a `deviceId` integer (`AppendExecutionProvider_DML(int deviceId = 0)` → native `OrtSessionOptionsAppendExecutionProvider_DML(handle, deviceId)`); there is NO managed `OrtDmlProviderOptions` type, struct, or dictionary overload in the 1.24.4 managed assembly
- device selection and provider configuration beyond the device index route through `SessionOptions.AddSessionConfigEntry`; the DML provider also requires the session arena disabled (`AppendExecutionProvider_DML` internally rejects the per-session memory arena), so a DML session sets the CPU-arena/memory-pattern posture accordingly rather than minting a provider-options blob

[STACKING]: the DML row composes the SAME `Model/*` session capsule as the CPU/CoreML/GPU rows —
- the 1.24.4 managed line also carries `OrtModelCompilationOptions`, `OrtEnv.GetEpDevices`/`GetModelCompatibilityForEpDevices`, and `CreateSharedAllocator`, so a `DirectMl` `ExecutionProvider` row drives the identical EP-context warm-start, autoEP device-rank (`HardwareDevice.Type == GPU`), and shared-arena fold the other rows use — DML adds zero new session machinery
- the row registers via `static (sessionOptions, _, _) => sessionOptions.AppendExecutionProvider_DML(0)` with no option table, and the empty option table folds into the `Model/identity#MODEL_IDENTITY` fingerprint as the zero-key case, keying a DML run distinctly in the result cache without call-site hashing

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

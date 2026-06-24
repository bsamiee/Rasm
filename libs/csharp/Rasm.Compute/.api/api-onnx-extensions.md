# [RASM_COMPUTE_API_ONNX_EXTENSIONS]

`Microsoft.ML.OnnxRuntime.Extensions` supplies native extension-operator assets,
runtime copy targets, tokenizer and pre/post-processing operator libraries, and
session-registration material for ONNX execution lanes. It carries NO managed
public assembly (no `.Managed` companion at 0.14.0) — the sole managed entry is
`SessionOptions.RegisterOrtExtensions()`, defined in `Microsoft.ML.OnnxRuntime`,
which P/Invokes the `ortextensions` native asset this package ships. The catalog
GUIDES the `Model/extension#EXTENSION_OPS` `CustomOps` fold: asset presence is
guarded before registration, and the string-tensor round-trip (`Tensor<string>`
ingress + `CreateTensorWithEmptyStrings`/`GetStringElement` egress) is how a
tokenizer/detokenizer custom-op model crosses the managed boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime.Extensions`
- package: `Microsoft.ML.OnnxRuntime.Extensions` 0.14.0
- assembly: native/build assets only — no managed public assembly and no `lib/` directory at 0.14.0 (resolve-verified: only `runtimes/` + `build/`/`buildTransitive/`)
- license: MIT
- namespace: package assets (managed entry is `Microsoft.ML.OnnxRuntime.SessionOptions.RegisterOrtExtensions`)
- asset: native custom-operator runtime assets (`libortextensions`/`ortextensions`) + per-TFM build targets
- consumer-bound build TFM: the package ships `build/` and `buildTransitive/` targets for `netstandard1.1`/`netstandard2.0` plus mobile TFMs (`net6.0-android31.0`, `net6.0-ios15.4`, `net6.0-macos12.3`, `monoandroid11.0`, `xamarinios10`) — a `net10.0` non-mobile consumer binds the `netstandard2.0` targets (no `net8.0`/`net9.0` build folder ships)
- rail: model

## [02]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: native runtime assets
- rail: model

| [INDEX] | [SYMBOL]                                                  | [PACKAGE_ROLE]      | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------- | :------------------ | :-------------------- |
|  [01]   | `runtimes/osx.10.14-arm64/native/libortextensions.dylib` | native asset        | loads extension ops (macOS arm64) |
|  [02]   | `runtimes/osx.10.14-x64/native/libortextensions.dylib`   | native asset        | loads extension ops (macOS x64) |
|  [03]   | `runtimes/linux-arm64/native/libortextensions.so`        | native asset        | loads extension ops (linux arm64) |
|  [04]   | `runtimes/linux-x64/native/libortextensions.so`          | native asset        | loads extension ops (linux x64) |
|  [05]   | `runtimes/win-arm64/native/ortextensions.dll`            | native asset        | loads extension ops (win arm64) |
|  [06]   | `runtimes/win-x64/native/ortextensions.dll`              | native asset        | loads extension ops (win x64) |
|  [07]   | `runtimes/win-x86/native/ortextensions.dll`              | native asset        | loads extension ops (win x86) |
|  [08]   | `runtimes/android/native/onnxruntime-extensions.aar`     | native asset        | loads Android ops     |
|  [09]   | `runtimes/ios/native/onnxruntime_extensions.xcframework.zip` | native asset     | loads Apple ops       |

[PACKAGE_ASSET_SCOPE]: build assets
- rail: model
- note: there is no plain `build/native/` props/targets — the targets are per-TFM under `build/<tfm>/` and `buildTransitive/<tfm>/`

| [INDEX] | [SYMBOL]                                                         | [PACKAGE_ROLE] | [CAPABILITY]         |
| :-----: | :--------------------------------------------------------------- | :------------- | :------------------- |
|  [01]   | `build/netstandard2.0/Microsoft.ML.OnnxRuntime.Extensions.props` / `.targets` | MSBuild import | declares + copies native assets for the `netstandard2.0`-bound consumer (the `net10.0` selection) |
|  [02]   | `buildTransitive/netstandard2.0/...targets`                     | MSBuild import | flows native assets transitively through a referencing project |
|  [03]   | `build/{net6.0-android31.0,net6.0-ios15.4,net6.0-macos12.3,monoandroid11.0,xamarinios10}/...targets` | MSBuild import | mobile/legacy TFM target variants |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: session registration
- rail: model

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]     | [CAPABILITY]                              |
| :-----: | :------------------------------------- | :--------------- | :---------------------------------------- |
|  [01]   | `SessionOptions.RegisterOrtExtensions` | session option   | registers extension ops via managed entry |
|  [02]   | extension native library               | runtime asset    | supplies custom op implementations        |
|  [03]   | tokenizer operations                   | native op family | executes tokenization                     |
|  [04]   | preprocessing operations               | native op family | prepares model inputs                     |
|  [05]   | postprocessing operations              | native op family | projects model outputs                    |
|  [06]   | native asset copy target               | build target     | places runtime assets                     |

[ENTRYPOINT_SCOPE]: string-tensor boundary (the round-trip a tokenizer/detokenizer op model needs; `OrtValue` members in `Microsoft.ML.OnnxRuntime`)
- rail: model (consumed by `Model/extension#EXTENSION_OPS` `StringSlots`/`StringEgress` and `Model/inference#INFERENCE_MODES` `RunInput.Strings`)

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]   | [CAPABILITY]                              |
| :-----: | :------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `OrtValue.CreateFromStringTensor(Tensor<string>)` | ingress factory | binds a `System.Numerics.Tensors.Tensor<string>` token input (the `RunInput.Strings` admission case) |
|  [02]   | `OrtValue.CreateTensorWithEmptyStrings(OrtAllocator, long[])` | egress factory | allocates the empty string-output slots a tokenizer/detokenizer op fills |
|  [03]   | `OrtValue.GetStringElement(int)` / `GetStringTensorAsArray()` | egress read | reads decoded string elements out (element-wise or bulk) |

[ENTRYPOINT_SCOPE]: decompile-verified registration facts
- rail: model (session-options registration; consumed by `Model/extension#EXTENSION_OPS`)

| [INDEX] | [MEMBER]                                       | [SIGNATURE]                                                                                |
| :-----: | :--------------------------------------------- | :----------------------------------------------------------------------------------------- |
|  [01]   | `SessionOptions.RegisterOrtExtensions`         | `void RegisterOrtExtensions()` — defined on `SessionOptions` in `Microsoft.ML.OnnxRuntime`; calls `OrtExtensionsNativeMethods.RegisterCustomOps(handle, ref OrtApiBase)`, catches the native `DllNotFoundException`, and re-throws `OnnxRuntimeException(ErrorCode.NoSuchFile, ...)` when the `ortextensions` asset is absent |
|  [02]   | `OrtExtensionsNativeMethods.RegisterCustomOps` | `internal static extern nint RegisterCustomOps(nint sessionOptions, ref OrtApiBase)` `[DllImport("ortextensions")]` — invoked by `RegisterOrtExtensions()`; not a public API surface |

## [04]-[IMPLEMENTATION_LAW]

[EXTENSION_ASSETS]:
- package role: native custom-operator bundle
- managed entry: `SessionOptions.RegisterOrtExtensions()` (defined in `Microsoft.ML.OnnxRuntime`, not in this package)
- binary root: `libortextensions` and `ortextensions` — placed by this package's MSBuild props/targets
- build root: props, targets, and build-transitive asset copy

[PHANTOM_CORRECTIONS]:
- `OrtExtensions.RegisterCustomOps` — PHANTOM. No public `OrtExtensions` class exists in `Microsoft.ML.OnnxRuntime.Extensions` 0.14.0. The package contains only native assets and MSBuild targets; there is no managed public assembly. The correct entry point is `SessionOptions.RegisterOrtExtensions()` defined in `Microsoft.ML.OnnxRuntime`.
- `OrtOperators` — PHANTOM. No `OrtOperators` type exists in `Microsoft.ML.OnnxRuntime` 1.27.0 or in `Microsoft.ML.OnnxRuntime.Extensions` 0.14.0. Extension op registration has no CLR type owner; it is fully native and entered through `SessionOptions.RegisterOrtExtensions()`.
- `RegisterCustomOpLibraryV2` on `SessionOptions` — is a separate method for loading arbitrary custom op libraries by path; it is distinct from the Extensions package registration path.

[LOCAL_ADMISSION]:
- Extension operators enter through ONNX Runtime session registration via `SessionOptions.RegisterOrtExtensions()`.
- Tokenizer, preprocessing, and postprocessing operators stay model-rail assets.
- Native asset presence is part of model evidence before model execution is admitted.
- Extension packages do not create a separate preprocessing service family.

[STACKING]: the package contributes ONE registration step and one boundary, folded into the existing owners —
- `RegisterOrtExtensions()` is one arm of the `Model/extension#EXTENSION_OPS` `CustomOps.Register` fold (gated on `SessionPolicy.OrtExtensions`), sequenced beside the per-path `RegisterCustomOpLibraryV2(path, out _)` arm and the `File.Exists` asset guard that faults `ExtensionAssetMissing` before any registration runs — so a missing `ortextensions` asset is caught at the managed guard, not at the native `DllNotFoundException`
- the string-tensor round-trip stacks the `RunInput.Strings` ingress (`CreateFromStringTensor`) with the `StringSlots`/`StringEgress` egress (`CreateTensorWithEmptyStrings` + `GetStringElement`) over the SAME `OrtValue` carrier the tensor pipeline uses; the `String` dtype is a model-boundary-only row and never enters the interior tensor vocabulary — a duplicate string-input factory or a tokenizer service is the rejected form
- there is no managed op-discovery surface to mine: the tokenizer/pre/post operators are entirely native and entered through the single registration call, so the catalog's job is the asset RID table + the boundary members, not an op roster

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime.Extensions`
- Owns: ONNX extension native assets
- Accept: declared custom-operator sessions
- Reject: opaque model preprocessor services; phantom `OrtExtensions.RegisterCustomOps` call site; phantom `OrtOperators` type

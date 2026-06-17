# [RASM_COMPUTE_API_ONNX_EXTENSIONS]

`Microsoft.ML.OnnxRuntime.Extensions` supplies native extension-operator assets,
runtime copy targets, tokenizer and pre/post-processing operator libraries, and
session-registration material for ONNX execution lanes.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime.Extensions`
- package: `Microsoft.ML.OnnxRuntime.Extensions`
- assembly: native/build assets only — no managed public assembly in 0.14.0
- namespace: package assets
- asset: native runtime assets
- rail: model

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: native runtime assets
- rail: model

| [INDEX] | [SYMBOL]                                   | [PACKAGE_ROLE]      | [CAPABILITY]          |
| :-----: | :----------------------------------------- | :------------------ | :-------------------- |
|   [1]   | `libortextensions.dylib` (osx.10.14-arm64) | native asset        | loads extension ops   |
|   [2]   | `libortextensions.dylib` (osx.10.14-x64)   | native asset        | loads extension ops   |
|   [3]   | `libortextensions.so` (linux-arm64)        | native asset        | loads extension ops   |
|   [4]   | `libortextensions.so` (linux-x64)          | native asset        | loads extension ops   |
|   [5]   | `ortextensions.dll` (win-arm64)            | native asset        | loads extension ops   |
|   [6]   | `ortextensions.dll` (win-x64)              | native asset        | loads extension ops   |
|   [7]   | `ortextensions.dll` (win-x86)              | native asset        | loads extension ops   |
|   [8]   | `onnxruntime-extensions.aar`               | native asset        | loads Android ops     |
|   [9]   | `onnxruntime_extensions.xcframework.zip`   | native asset        | loads Apple ops       |
|  [10]   | `runtimes/*/native`                        | runtime asset group | selects native binary |

[PACKAGE_ASSET_SCOPE]: build assets
- rail: model

| [INDEX] | [SYMBOL]                                      | [PACKAGE_ROLE] | [CAPABILITY]         |
| :-----: | :-------------------------------------------- | :------------- | :------------------- |
|   [1]   | `Microsoft.ML.OnnxRuntime.Extensions.props`   | MSBuild import | declares native copy |
|   [2]   | `Microsoft.ML.OnnxRuntime.Extensions.targets` | MSBuild import | copies native assets |
|   [3]   | `buildTransitive` targets                     | MSBuild import | flows native assets  |
|   [4]   | platform target folders                       | asset selector | selects RID behavior |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: session registration
- rail: model

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]     | [CAPABILITY]                              |
| :-----: | :------------------------------------- | :--------------- | :---------------------------------------- |
|   [1]   | `SessionOptions.RegisterOrtExtensions` | session option   | registers extension ops via managed entry |
|   [2]   | extension native library               | runtime asset    | supplies custom op implementations        |
|   [3]   | tokenizer operations                   | native op family | executes tokenization                     |
|   [4]   | preprocessing operations               | native op family | prepares model inputs                     |
|   [5]   | postprocessing operations              | native op family | projects model outputs                    |
|   [6]   | native asset copy target               | build target     | places runtime assets                     |

[ENTRYPOINT_SCOPE]: decompile-verified registration facts
- source: `Microsoft.ML.OnnxRuntime` 1.26.0 managed assembly decompile; `Microsoft.ML.OnnxRuntime.Extensions` 0.14.0 package inspection
- rail: model-lane#GENERATIVE_RUN

| [INDEX] | [MEMBER]                                       | [SIGNATURE]                                                                                | [USED_BY]                 | [EVIDENCE]           |
| :-----: | :--------------------------------------------- | :----------------------------------------------------------------------------------------- | :------------------------ | :------------------- |
|   [1]   | `SessionOptions.RegisterOrtExtensions`         | `void RegisterOrtExtensions()` — defined on `SessionOptions` in `Microsoft.ML.OnnxRuntime` | model-lane#GENERATIVE_RUN | decompile ORT 1.26.0 |
|   [2]   | `OrtExtensionsNativeMethods.RegisterCustomOps` | internal — invoked by `RegisterOrtExtensions()`; not a public API surface                  | model-lane#GENERATIVE_RUN | decompile ORT 1.26.0 |

## [4]-[IMPLEMENTATION_LAW]

[EXTENSION_ASSETS]:
- package role: native custom-operator bundle
- managed entry: `SessionOptions.RegisterOrtExtensions()` (defined in `Microsoft.ML.OnnxRuntime`, not in this package)
- binary root: `libortextensions` and `ortextensions` — placed by this package's MSBuild props/targets
- build root: props, targets, and build-transitive asset copy

[PHANTOM_CORRECTIONS]:
- `OrtExtensions.RegisterCustomOps` — PHANTOM. No public `OrtExtensions` class exists in `Microsoft.ML.OnnxRuntime.Extensions` 0.14.0. The package contains only native assets and MSBuild targets; there is no managed public assembly. The correct entry point is `SessionOptions.RegisterOrtExtensions()` defined in `Microsoft.ML.OnnxRuntime`.
- `OrtOperators` — PHANTOM. No `OrtOperators` type exists in `Microsoft.ML.OnnxRuntime` 1.26.0 or in `Microsoft.ML.OnnxRuntime.Extensions` 0.14.0. Extension op registration has no CLR type owner; it is fully native and entered through `SessionOptions.RegisterOrtExtensions()`.
- `RegisterCustomOpLibraryV2` on `SessionOptions` — is a separate method for loading arbitrary custom op libraries by path; it is distinct from the Extensions package registration path.

[LOCAL_ADMISSION]:
- Extension operators enter through ONNX Runtime session registration via `SessionOptions.RegisterOrtExtensions()`.
- Tokenizer, preprocessing, and postprocessing operators stay model-rail assets.
- Native asset presence is part of model evidence before model execution is admitted.
- Extension packages do not create a separate preprocessing service family.

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime.Extensions`
- Owns: ONNX extension native assets
- Accept: declared custom-operator sessions
- Reject: opaque model preprocessor services; phantom `OrtExtensions.RegisterCustomOps` call site; phantom `OrtOperators` type

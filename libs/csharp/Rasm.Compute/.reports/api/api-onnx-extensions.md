# [RASM_COMPUTE_API_ONNX_EXTENSIONS]

`Microsoft.ML.OnnxRuntime.Extensions` supplies native extension-operator assets,
runtime copy targets, tokenizer and pre/post-processing operator libraries, and
session-registration material for ONNX execution lanes.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime.Extensions`
- package: `Microsoft.ML.OnnxRuntime.Extensions`
- assembly: native/build assets
- namespace: package assets
- asset: native runtime assets
- rail: model

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: native runtime assets
- rail: model

| [INDEX] | [SYMBOL]                                 | [PACKAGE_ROLE]      | [CAPABILITY]          |
| :-----: | :--------------------------------------- | :------------------ | :-------------------- |
|   [1]   | `libortextensions.dylib`                 | native asset        | loads extension ops   |
|   [2]   | `libortextensions.so`                    | native asset        | loads extension ops   |
|   [3]   | `ortextensions.dll`                      | native asset        | loads extension ops   |
|   [4]   | `onnxruntime-extensions.aar`             | native asset        | loads Android ops     |
|   [5]   | `onnxruntime_extensions.xcframework.zip` | native asset        | loads Apple ops       |
|   [6]   | `runtimes/*/native`                      | runtime asset group | selects native binary |

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

| [INDEX] | [SURFACE]                 | [CALL_SHAPE]     | [CAPABILITY]            |
| :-----: | :------------------------ | :--------------- | :---------------------- |
|   [1]   | `RegisterOrtExtensions`   | session option   | registers extension ops |
|   [2]   | extension native library  | runtime asset    | supplies custom ops     |
|   [3]   | tokenizer operations      | native op family | executes tokenization   |
|   [4]   | preprocessing operations  | native op family | prepares model inputs   |
|   [5]   | postprocessing operations | native op family | projects model outputs  |
|   [6]   | native asset copy target  | build target     | places runtime assets   |

## [4]-[IMPLEMENTATION_LAW]

[EXTENSION_ASSETS]:
- package role: native custom-operator bundle
- managed entry: `RegisterOrtExtensions`
- binary root: `libortextensions` and `ortextensions`
- build root: props, targets, and build-transitive asset copy

[LOCAL_ADMISSION]:
- Extension operators enter through ONNX Runtime session registration.
- Tokenizer, preprocessing, and postprocessing operators stay model-rail assets.
- Native asset presence is part of model evidence before model execution is admitted.
- Extension packages do not create a separate preprocessing service family.

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime.Extensions`
- Owns: ONNX extension native assets
- Accept: declared custom-operator sessions
- Reject: opaque model preprocessor services

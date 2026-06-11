# [RASM_COMPUTE_API_ONNX_EXTENSIONS]

`Microsoft.ML.OnnxRuntime.Extensions` supplies native extension-op assets for ONNX execution lanes.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Microsoft.ML.OnnxRuntime.Extensions`
- package: `Microsoft.ML.OnnxRuntime.Extensions`
- assembly: native/build assets
- namespace: package assets
- asset: native runtime assets
- rail: model

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: extension asset family
- rail: model

| [INDEX] | [ASSET]                      | [PACKAGE_ROLE]      | [CAPABILITY]          |
| :-----: | :--------------------------- | :------------------ | :-------------------- |
|   [1]   | extension native library     | native library      | loads custom ops      |
|   [2]   | `runtimes/*/native`          | runtime asset group | selects native binary |
|   [3]   | `buildTransitive` assets     | MSBuild import      | declares build input  |
|   [4]   | tokenizer operation assets   | native op asset     | loads custom ops      |
|   [5]   | pre/post processor op assets | native op asset     | loads custom ops      |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: extension assets
- rail: model

| [INDEX] | [SURFACE]               | [CALL_SHAPE]    | [CAPABILITY]          |
| :-----: | :---------------------- | :-------------- | :-------------------- |
|   [1]   | native runtime asset    | package asset   | selects native binary |
|   [2]   | build-transitive import | MSBuild asset   | declares build input  |
|   [3]   | tokenizer op asset      | native op asset | loads custom ops      |
|   [4]   | preprocessing op asset  | native op asset | loads custom ops      |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Microsoft.ML.OnnxRuntime.Extensions`
- Owns: ONNX extension native assets
- Accept: extension assets enter the ONNX Runtime session options rail
- Reject: opaque model preprocessor services

# [RASM_COMPUTE_API_GRPC_TOOLS]

`Grpc.Tools` supplies build-time protocol generation, MSBuild targets, `.proto` item metadata, and generated client seams.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Tools`
- package: `Grpc.Tools`
- assembly: build assets
- namespace: MSBuild/protoc assets
- asset: build assets
- rail: remote-contracts

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: build asset family
- rail: remote-contracts

| [INDEX] | [ASSET]                  | [PACKAGE_ROLE]       | [CAPABILITY]               |
| :-----: | :----------------------- | :------------------- | :------------------------- |
|   [1]   | `Grpc.Tools.props`       | MSBuild import       | declares build input       |
|   [2]   | `Grpc.Tools.targets`     | MSBuild target       | drives protocol generation |
|   [3]   | `Protobuf.MSBuild.dll`   | generator task asset | executes generator task    |
|   [4]   | `protoc`                 | compiler binary      | emits message contracts    |
|   [5]   | `grpc_csharp_plugin`     | compiler plugin      | emits client contracts     |
|   [6]   | bundled `.proto` imports | import asset         | resolves proto imports     |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build surfaces
- rail: remote-contracts

| [INDEX] | [SURFACE]               | [CALL_SHAPE]     | [CAPABILITY]              |
| :-----: | :---------------------- | :--------------- | :------------------------ |
|   [1]   | `Protobuf` item         | MSBuild item     | declares proto input      |
|   [2]   | `GrpcServices=Client`   | item metadata    | selects generated surface |
|   [3]   | `ProtoRoot`             | item metadata    | scopes imports            |
|   [4]   | `AdditionalImportDirs`  | item metadata    | resolves proto imports    |
|   [5]   | generated C# messages   | generated output | emits message contracts   |
|   [6]   | generated gRPC clients  | generated output | emits client contracts    |
|   [7]   | `CompileOutputs`        | build switch     | adds compile items        |
|   [8]   | `GeneratorResponseFile` | response file    | passes generator args     |

## [4]-[IMPLEMENTATION_LAW]

[RAIL_LAW]:
- Package: `Grpc.Tools`
- Owns: protocol generation
- Accept: generated contracts enter remote rail
- Reject: runtime dependency on generator package

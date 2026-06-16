# [RASM_COMPUTE_API_GRPC_TOOLS]

`Grpc.Tools` supplies build-time protocol generation, MSBuild targets, `.proto`
item metadata, compiler binaries, import protos, and generated client seams.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Tools`
- package: `Grpc.Tools`
- assembly: build assets
- namespace: MSBuild/protoc assets
- asset: build tool package
- rail: remote-contracts

## [2]-[PACKAGE_ASSETS]

[PACKAGE_ASSET_SCOPE]: MSBuild assets
- rail: remote-contracts

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE] | [CAPABILITY]             |
| :-----: | :---------------------------------------------- | :------------- | :----------------------- |
|   [1]   | `build/Grpc.Tools.props`                        | MSBuild import | declares generator props |
|   [2]   | `build/Grpc.Tools.targets`                      | MSBuild import | runs generator targets   |
|   [3]   | `build/_grpc/_Grpc.Tools.props`                 | MSBuild import | declares gRPC props      |
|   [4]   | `build/_grpc/_Grpc.Tools.targets`               | MSBuild import | runs gRPC target steps   |
|   [5]   | `build/_protobuf/Google.Protobuf.Tools.props`   | MSBuild import | declares proto props     |
|   [6]   | `build/_protobuf/Google.Protobuf.Tools.targets` | MSBuild import | runs proto target steps  |
|   [7]   | `Protobuf.MSBuild.dll`                          | task assembly  | executes generator tasks |
|   [8]   | `Grpc.CSharp.xml`                               | task metadata  | describes gRPC task      |
|   [9]   | `Protobuf.CSharp.xml`                           | task metadata  | describes proto task     |

[PACKAGE_ASSET_SCOPE]: compiler and import assets
- rail: remote-contracts

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]  | [CAPABILITY]             |
| :-----: | :--------------------------------- | :-------------- | :----------------------- |
|   [1]   | `protoc`                           | compiler binary | emits message code       |
|   [2]   | `grpc_csharp_plugin`               | compiler plugin | emits client code        |
|   [3]   | `google/protobuf/any.proto`        | import proto    | supplies well-known type |
|   [4]   | `google/protobuf/timestamp.proto`  | import proto    | supplies time type       |
|   [5]   | `google/protobuf/duration.proto`   | import proto    | supplies duration type   |
|   [6]   | `google/protobuf/empty.proto`      | import proto    | supplies empty type      |
|   [7]   | `google/protobuf/field_mask.proto` | import proto    | supplies field mask type |
|   [8]   | `google/protobuf/wrappers.proto`   | import proto    | supplies scalar wrappers |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: project inputs
- rail: remote-contracts

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]  | [CAPABILITY]                                             |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------- |
|   [1]   | `Protobuf` item             | MSBuild item  | declares proto input                                     |
|   [2]   | `GrpcServices`              | item metadata | `Both`/`Client`/`Server`/`None` stub generation selector |
|   [3]   | `ProtoRoot`                 | item metadata | scopes import roots                                      |
|   [4]   | `AdditionalImportDirs`      | item metadata | resolves additional import paths                         |
|   [5]   | `Access`                    | item metadata | `Public`/`Internal` class access modifier                |
|   [6]   | `OutputDir`                 | item metadata | sets generated output directory                          |
|   [7]   | `CompileOutputs`            | item metadata | `True`/`False` — includes generated code in compilation  |
|   [8]   | `AdditionalProtocArguments` | item metadata | passes extra arguments to protoc                         |
|   [9]   | `ProtoCompile`              | item metadata | `True`/`False` — compiles or import-only                 |

[ENTRYPOINT_SCOPE]: `Protobuf` item metadata decompile-verified rows
- source: `Grpc.Tools` 2.81.1 — `Grpc.CSharp.xml` / `Protobuf.CSharp.xml` / `Google.Protobuf.Tools.targets` source
- rail: remote-contracts

| [INDEX] | [MEMBER]                    | [SIGNATURE]                                                  | [USED_BY]              | [EVIDENCE]                 |
| :-----: | :-------------------------- | :----------------------------------------------------------- | :--------------------- | :------------------------- |
|   [1]   | `GrpcServices`              | enum: `Both` (default), `Client`, `Server`, `None`           | remote-lane#CALL_SPINE | Grpc.CSharp.xml 2.81.1     |
|   [2]   | `Access`                    | enum: `Public` (default), `Internal`                         | remote-lane#CALL_SPINE | Protobuf.CSharp.xml 2.81.1 |
|   [3]   | `ProtoCompile`              | bool: `true` (default) — compile vs import-only              | remote-lane#CALL_SPINE | Protobuf.CSharp.xml 2.81.1 |
|   [4]   | `ProtoRoot`                 | string — import root path; defaults to item relative dir     | remote-lane#CALL_SPINE | targets 2.81.1             |
|   [5]   | `AdditionalImportDirs`      | string — semicolon-separated extra import directories        | remote-lane#CALL_SPINE | targets 2.81.1             |
|   [6]   | `OutputDir`                 | string — generated file output directory                     | remote-lane#CALL_SPINE | targets 2.81.1             |
|   [7]   | `CompileOutputs`            | bool: `True` (default) — adds generated files to compilation | remote-lane#CALL_SPINE | targets 2.81.1             |
|   [8]   | `AdditionalProtocArguments` | string — extra arguments passed verbatim to `protoc`         | remote-lane#CALL_SPINE | targets 2.81.1             |

[ENTRYPOINT_SCOPE]: generated outputs
- rail: remote-contracts

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]   | [CAPABILITY]            |
| :-----: | :--------------------------- | :------------- | :---------------------- |
|   [1]   | generated message types      | generated code | carries remote payloads |
|   [2]   | generated parser members     | generated code | parses remote payloads  |
|   [3]   | generated descriptor members | generated code | describes remote schema |
|   [4]   | generated client types       | generated code | calls remote services   |
|   [5]   | generated async methods      | generated code | calls async operations  |
|   [6]   | generated streaming methods  | generated code | calls stream operations |

## [4]-[IMPLEMENTATION_LAW]

[GENERATOR_ADMISSION]:
- package role: build-only protocol generation
- dependency role: private tool asset
- runtime rule: generated code is admitted, generator package is not a runtime surface
- source rule: `.proto` files are boundary contracts and require owner-local folder placement

[ITEM_METADATA]:
- `GrpcServices=Client` is the Compute-admitted value; `Server` and `Both` are server-hosting values rejected by `[LOCAL_ADMISSION]`
- `Access=Internal` is preferred for Compute-internal generated types; `Public` only when the contract crosses package boundaries
- `OutputOptions` does not exist as a named metadata property — access modifier and service shape are controlled by `Access` and `GrpcServices` respectively

[REMOTE_CONTRACT_OUTPUT]:
- message output: generated `IMessage<T>` contracts
- client output: generated client stubs over `Grpc.Net.Client`
- descriptor output: generated static descriptors for contract inspection
- stream output: generated unary, client-stream, server-stream, and duplex calls

[LOCAL_ADMISSION]:
- Compute remote contract source uses `Protobuf` MSBuild items.
- Generator outputs must land in the remote-contract rail before implementation source uses them.
- Generated clients are adapters and cannot own Compute execution policy.
- Tool assets remain `PrivateAssets=all`.

[RAIL_LAW]:
- Package: `Grpc.Tools`
- Owns: protocol generation
- Accept: generated remote contracts
- Reject: runtime dependency on generator assets

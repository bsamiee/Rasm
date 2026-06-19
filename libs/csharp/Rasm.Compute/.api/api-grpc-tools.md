# [RASM_COMPUTE_API_GRPC_TOOLS]

`Grpc.Tools` supplies build-time protocol generation, MSBuild targets, `.proto`
item metadata, compiler binaries, import protos, and generated client seams.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Tools`
- package: `Grpc.Tools`
- assembly: build assets
- namespace: MSBuild/protoc assets
- asset: build tool package
- rail: remote-contracts

## [02]-[PUBLIC_TYPES]

[PACKAGE_ASSET_SCOPE]: MSBuild assets
- rail: remote-contracts

| [INDEX] | [SYMBOL]                                        | [PACKAGE_ROLE] | [CAPABILITY]             |
| :-----: | :---------------------------------------------- | :------------- | :----------------------- |
|  [01]   | `build/Grpc.Tools.props`                        | MSBuild import | declares generator props |
|  [02]   | `build/Grpc.Tools.targets`                      | MSBuild import | runs generator targets   |
|  [03]   | `build/_grpc/_Grpc.Tools.props`                 | MSBuild import | declares gRPC props      |
|  [04]   | `build/_grpc/_Grpc.Tools.targets`               | MSBuild import | runs gRPC target steps   |
|  [05]   | `build/_protobuf/Google.Protobuf.Tools.props`   | MSBuild import | declares proto props     |
|  [06]   | `build/_protobuf/Google.Protobuf.Tools.targets` | MSBuild import | runs proto target steps  |
|  [07]   | `Protobuf.MSBuild.dll`                          | task assembly  | executes generator tasks |
|  [08]   | `Grpc.CSharp.xml`                               | task metadata  | describes gRPC task      |
|  [09]   | `Protobuf.CSharp.xml`                           | task metadata  | describes proto task     |

[PACKAGE_ASSET_SCOPE]: compiler and import assets
- rail: remote-contracts

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]  | [CAPABILITY]             |
| :-----: | :--------------------------------- | :-------------- | :----------------------- |
|  [01]   | `protoc`                           | compiler binary | emits message code       |
|  [02]   | `grpc_csharp_plugin`               | compiler plugin | emits client code        |
|  [03]   | `google/protobuf/any.proto`        | import proto    | supplies well-known type |
|  [04]   | `google/protobuf/timestamp.proto`  | import proto    | supplies time type       |
|  [05]   | `google/protobuf/duration.proto`   | import proto    | supplies duration type   |
|  [06]   | `google/protobuf/empty.proto`      | import proto    | supplies empty type      |
|  [07]   | `google/protobuf/field_mask.proto` | import proto    | supplies field mask type |
|  [08]   | `google/protobuf/wrappers.proto`   | import proto    | supplies scalar wrappers |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: project inputs
- rail: remote-contracts

| [INDEX] | [SURFACE]                   | [CALL_SHAPE]  | [CAPABILITY]                                             |
| :-----: | :-------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Protobuf` item             | MSBuild item  | declares proto input                                     |
|  [02]   | `GrpcServices`              | item metadata | `Both`/`Client`/`Server`/`None` stub generation selector |
|  [03]   | `ProtoRoot`                 | item metadata | scopes import roots                                      |
|  [04]   | `AdditionalImportDirs`      | item metadata | resolves additional import paths                         |
|  [05]   | `Access`                    | item metadata | `Public`/`Internal` class access modifier                |
|  [06]   | `OutputDir`                 | item metadata | sets generated output directory                          |
|  [07]   | `CompileOutputs`            | item metadata | `True`/`False` — includes generated code in compilation  |
|  [08]   | `AdditionalProtocArguments` | item metadata | passes extra arguments to protoc                         |
|  [09]   | `ProtoCompile`              | item metadata | `True`/`False` — compiles or import-only                 |

[ENTRYPOINT_SCOPE]: `Protobuf` item metadata decompile-verified rows
- source: `Grpc.Tools` 2.81.1 — `Grpc.CSharp.xml` / `Protobuf.CSharp.xml` / `Google.Protobuf.Tools.targets`
- rail: remote-contracts
- consumer: `remote-lane#CALL_SPINE`

| [INDEX] | [MEMBER]                    | [SIGNATURE]                                                  |
| :-----: | :-------------------------- | :----------------------------------------------------------- |
|  [01]   | `GrpcServices`              | enum: `Both` (default), `Client`, `Server`, `None`           |
|  [02]   | `Access`                    | enum: `Public` (default), `Internal`                         |
|  [03]   | `ProtoCompile`              | bool: `true` (default) — compile vs import-only              |
|  [04]   | `ProtoRoot`                 | string — import root path; defaults to item relative dir     |
|  [05]   | `AdditionalImportDirs`      | string — semicolon-separated extra import directories        |
|  [06]   | `OutputDir`                 | string — generated file output directory                     |
|  [07]   | `CompileOutputs`            | bool: `True` (default) — adds generated files to compilation |
|  [08]   | `AdditionalProtocArguments` | string — extra arguments passed verbatim to `protoc`         |

[ENTRYPOINT_SCOPE]: generated outputs
- rail: remote-contracts

| [INDEX] | [SURFACE]                    | [CALL_SHAPE]   | [CAPABILITY]            |
| :-----: | :--------------------------- | :------------- | :---------------------- |
|  [01]   | generated message types      | generated code | carries remote payloads |
|  [02]   | generated parser members     | generated code | parses remote payloads  |
|  [03]   | generated descriptor members | generated code | describes remote schema |
|  [04]   | generated client types       | generated code | calls remote services   |
|  [05]   | generated async methods      | generated code | calls async operations  |
|  [06]   | generated streaming methods  | generated code | calls stream operations |

## [04]-[IMPLEMENTATION_LAW]

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

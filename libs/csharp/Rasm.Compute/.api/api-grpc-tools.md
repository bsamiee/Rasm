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

| [INDEX] | [SYMBOL]                                            | [PACKAGE_ROLE] | [CAPABILITY]                          |
| :-----: | :-------------------------------------------------- | :------------- | :------------------------------------ |
|  [01]   | `build/Grpc.Tools.props`                            | MSBuild import | top-level prop import                 |
|  [02]   | `build/Grpc.Tools.targets`                          | MSBuild import | top-level target import               |
|  [03]   | `build/_grpc/_Grpc.Tools.props`                     | MSBuild import | gRPC-stub props                       |
|  [04]   | `build/_grpc/_Grpc.Tools.targets`                   | MSBuild import | gRPC-stub `Protobuf_Compile` transform |
|  [05]   | `build/_protobuf/Google.Protobuf.Tools.props`       | MSBuild import | message props + `Protobuf_StandardImportsPath` |
|  [06]   | `build/_protobuf/Google.Protobuf.Tools.targets`     | MSBuild import | `ProtoCompile` task wiring            |
|  [07]   | `build/_protobuf/{net45,netstandard1.3}/Protobuf.MSBuild.dll` | task assembly | `ProtoCompile` MSBuild task implementation |
|  [08]   | `build/_grpc/Grpc.CSharp.xml`                       | IDE schema     | VS property-page schema (`GrpcServices`) |
|  [09]   | `build/_protobuf/Protobuf.CSharp.xml`               | IDE schema     | VS property-page schema (`Access`, `ProtoCompile`) |

[PACKAGE_ASSET_SCOPE]: compiler and import assets
- rail: remote-contracts

Compiler binaries ship per-RID under `tools/<rid>/`: `linux_x64`, `linux_arm64`, `linux_x86`, `macosx_x64`, `windows_x64`, `windows_x86`. There is NO `macosx_arm64` asset — Apple-silicon hosts run the `macosx_x64` `protoc`/`grpc_csharp_plugin` under Rosetta 2. The well-known protos live under `build/native/include/google/protobuf/` (resolved by `Protobuf_StandardImportsPath`).

| [INDEX] | [SYMBOL]                            | [PACKAGE_ROLE]  | [CAPABILITY]                          |
| :-----: | :---------------------------------- | :-------------- | :------------------------------------ |
|  [01]   | `tools/<rid>/protoc`                | compiler binary | emits message code (per-RID)          |
|  [02]   | `tools/<rid>/grpc_csharp_plugin`    | compiler plugin | emits client/server stub code (per-RID) |
|  [03]   | `google/protobuf/any.proto`         | import proto    | `Any` type-erased message envelope    |
|  [04]   | `google/protobuf/timestamp.proto`   | import proto    | `Timestamp` (NodaTime-bridged)        |
|  [05]   | `google/protobuf/duration.proto`    | import proto    | `Duration` (NodaTime-bridged)         |
|  [06]   | `google/protobuf/empty.proto`       | import proto    | `Empty` no-payload message            |
|  [07]   | `google/protobuf/field_mask.proto`  | import proto    | `FieldMask` partial-update selector   |
|  [08]   | `google/protobuf/wrappers.proto`    | import proto    | scalar nullable wrappers              |
|  [09]   | `google/protobuf/struct.proto`      | import proto    | `Struct`/`Value`/`ListValue` dynamic JSON-like payloads |
|  [10]   | `google/protobuf/descriptor.proto`  | import proto    | `FileDescriptorProto` self-description (descriptor-diff source) |
|  [11]   | `google/protobuf/api.proto`         | import proto    | `Api`/`Method` service reflection types |
|  [12]   | `google/protobuf/type.proto`        | import proto    | `Type`/`Field`/`Enum` reflection types |
|  [13]   | `google/protobuf/source_context.proto` | import proto | `SourceContext` reflection origin     |

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
|  [06]   | `OutputDir`                 | item metadata | message-code output directory                            |
|  [07]   | `GrpcOutputDir`             | item metadata | gRPC-stub output directory (defaults to `OutputDir`)     |
|  [08]   | `CompileOutputs`            | item metadata | `True`/`False` — includes generated code in compilation  |
|  [09]   | `AdditionalProtocArguments` | item metadata | passes extra arguments to protoc                         |
|  [10]   | `OutputOptions`             | item metadata | raw protoc message-codegen options escape hatch          |
|  [11]   | `GrpcOutputOptions`         | item metadata | raw `grpc_csharp_plugin` options escape hatch            |
|  [12]   | `ProtoCompile`              | item metadata | `True`/`False` — compiles or import-only                 |

[ENTRYPOINT_SCOPE]: `Protobuf` item metadata verified rows
- source: `Grpc.Tools` 2.81.1 — `build/_protobuf/Google.Protobuf.Tools.targets` + `build/_grpc/_Grpc.Tools.targets` `Protobuf_Compile` transform (the authoritative metadata; the `*.CSharp.xml` files are VS IDE property-page schemas only)
- rail: remote-contracts
- consumer: `remote-lane#CALL_SPINE`

| [INDEX] | [MEMBER]                    | [SIGNATURE]                                                  |
| :-----: | :-------------------------- | :----------------------------------------------------------- |
|  [01]   | `GrpcServices`              | enum: `Both` (default), `Client`, `Server`, `None`           |
|  [02]   | `Access`                    | enum: `Public` (default), `Internal` — sugar that appends `internal_access` to `OutputOptions`/`GrpcOutputOptions` |
|  [03]   | `ProtoCompile`              | bool: `true` (default) — compile vs import-only              |
|  [04]   | `ProtoRoot`                 | string — import root path; defaults to item relative dir     |
|  [05]   | `AdditionalImportDirs`      | string — semicolon-separated extra import dirs; the protoc `ProtoPath` is `AdditionalImportDirs` + `Protobuf_StandardImportsPath` + `ProtoRoot` |
|  [06]   | `OutputDir`                 | string — message-code output directory                       |
|  [07]   | `GrpcOutputDir`             | string — gRPC-stub output directory; falls back to `OutputDir` when empty |
|  [08]   | `CompileOutputs`            | bool: `True` (default) — adds generated files to compilation |
|  [09]   | `AdditionalProtocArguments` | string — extra arguments passed verbatim to `protoc`         |
|  [10]   | `OutputOptions`             | string — raw protoc message-codegen options (`GrpcServices=None` appends `no_server`/`no_client`) |
|  [11]   | `GrpcOutputOptions`         | string — raw `grpc_csharp_plugin` options                    |
|  [12]   | `Generator`                 | string — design-time build generator hook; defaults to `MSBuild:Compile` (the `$(Protobuf_Generator)` property is `CSharp`) |

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
- `Access`/`GrpcServices` are sugar the `Protobuf_Compile` transform lowers into the protoc/plugin option strings: `Access=Internal` appends `internal_access`, `GrpcServices=None` appends `no_server`+`no_client`. The lowered options compose with the user-facing `OutputOptions` (message codegen) and `GrpcOutputOptions` (gRPC stubs) escape-hatch metadata — both are real `Protobuf` item metadata, so a raw protoc flag the named metadata does not cover is reachable without a custom target.
- `OutputDir` and `GrpcOutputDir` are distinct sinks (message code vs gRPC stubs); `GrpcOutputDir` defaults to `OutputDir`.

[REMOTE_CONTRACT_OUTPUT]:
- message output: generated `IMessage<T>` contracts
- client output: generated client stubs over `Grpc.Net.Client`
- descriptor output: generated static descriptors for contract inspection
- stream output: generated unary, client-stream, server-stream, and duplex calls

[STACK_INTEGRATION]:
- Single wire rail: `Grpc.Tools` emits the `IMessage<T>`/client-stub seam that the rest of the wire stack composes against — generated clients invoke over `Grpc.Net.Client`'s `GrpcChannel`, generated messages parse zero-alloc through `Google.Protobuf`'s `IBufferMessage`/`CodedInputStream` over a `RecyclableMemoryStream`, `Timestamp`/`Duration` round-trip through `NodaTime.Serialization.Protobuf`, and `FieldMask` (from `field_mask.proto`) drives the partial-update APPLY leg.
- Build-once, compile-twice: the same `.proto` files compile `GrpcServices=Client` here and `GrpcServices=Server` at the AppHost root; the AppHost emits the descriptor set (via `descriptor.proto` self-description) that feeds the contract-evolution checksum gate and the browser connect-es codegen.
- `Access=Internal` keeps generated types package-private so the `WireDocument` parity owner — not the raw stub — is the public seam; `Access=Public` only where the contract literally crosses a package boundary.

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

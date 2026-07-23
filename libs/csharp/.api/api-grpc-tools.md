# [RASM_API_GRPC_TOOLS]

`Grpc.Tools` mints the C# wire seam from `.proto` at build time, driving `protoc` and `grpc_csharp_plugin` from `Protobuf` MSBuild items, folding the emitted files into `Compile`, and rooting the well-known import set every contract resolves against. Generated messages and stubs are the runtime surface; the generator never enters the runtime graph.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Grpc.Tools`
- package: `Grpc.Tools` (Apache-2.0)
- assembly: `Protobuf.MSBuild.dll`, selected by `$(MSBuildRuntimeType)` between the `netstandard1.3` and `net45` copies
- namespace: `Grpc.Tools`
- role: development dependency — `build/Grpc.Tools.props` and `build/Grpc.Tools.targets` are the import entry points, `tools/<os>_<cpu>/` the compiler root, `build/native/include/` the import root
- rail: remote-contracts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: MSBuild tasks `Protobuf.MSBuild.dll` registers through `UsingTask`

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY] | [CAPABILITY]                                     |
| :-----: | :---------------------- | :------------ | :----------------------------------------------- |
|  [01]   | `ProtoCompile`          | msbuild-task  | invokes `protoc` with the lowered option set     |
|  [02]   | `ProtoToolsPlatform`    | msbuild-task  | resolves host OS and CPU to the compiler RID     |
|  [03]   | `ProtoCompilerOutputs`  | msbuild-task  | infers expected outputs for the up-to-date check |
|  [04]   | `ProtoReadDependencies` | msbuild-task  | reads `.protodep` files from prior compiles      |

[PUBLIC_TYPE_SCOPE]: well-known imports `Protobuf_StandardImportsPath` resolves; a `.proto` imports the file and `protoc` mints the C# types.
- `any.proto`: `Any` — type-erased message envelope.
- `timestamp.proto` `duration.proto`: `Timestamp` `Duration` — the NodaTime-bridged temporal pair.
- `empty.proto`: `Empty` — no-payload request or reply.
- `field_mask.proto`: `FieldMask` — partial-update path selector.
- `struct.proto`: `Struct` `Value` `ListValue` `NullValue` — dynamic JSON-shaped payload.
- `wrappers.proto`: `BoolValue` `BytesValue` `DoubleValue` `FloatValue` `Int32Value` `Int64Value` `StringValue` `UInt32Value` `UInt64Value` — nullable scalar boxes.
- `descriptor.proto`: `FileDescriptorProto` — self-description, the descriptor-set diff source.
- `api.proto`: `Api` `Method` `Mixin` — service reflection.
- `type.proto`: `Type` `Field` `Enum` `EnumValue` `Option` `Syntax` — type reflection.
- `source_context.proto`: `SourceContext` — reflection origin path.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `Protobuf` item metadata — `[SHAPE]` pairs the value kind with its `ItemDefinitionGroup` default, `—` marking a metadatum left unset.

| [INDEX] | [SURFACE]                   | [SHAPE]                      | [CAPABILITY]                                      |
| :-----: | :-------------------------- | :--------------------------- | :------------------------------------------------ |
|  [01]   | `GrpcServices`              | `enum = Both`                | `Both`/`Client`/`Server`/`None` stub selector     |
|  [02]   | `Access`                    | `enum = Public`              | `Public`/`Internal` generated-type visibility     |
|  [03]   | `ProtoCompile`              | `bool = True`                | compiles the file or admits it import-only        |
|  [04]   | `CompileOutputs`            | `bool = True`                | folds generated files into `Compile`              |
|  [05]   | `Generator`                 | `MSBuild:Compile`            | design-time single-file generator hook            |
|  [06]   | `ProtoRoot`                 | `path = project dir`         | roots the import namespace                        |
|  [07]   | `OutputDir`                 | `path = Protobuf_OutputPath` | message-code sink                                 |
|  [08]   | `GrpcOutputDir`             | `path = OutputDir`           | stub-code sink                                    |
|  [09]   | `GrpcPluginExe`             | `path = packaged plugin`     | overrides the plugin binary per item              |
|  [10]   | `AdditionalImportDirs`      | `paths = —`                  | prepends import roots ahead of the well-known set |
|  [11]   | `OutputOptions`             | `options = —`                | raw `protoc` message-codegen options              |
|  [12]   | `GrpcOutputOptions`         | `options = —`                | raw `grpc_csharp_plugin` options                  |
|  [13]   | `AdditionalProtocArguments` | `args = —`                   | raw `protoc` command-line arguments               |

[ENTRYPOINT_SCOPE]: project properties, environment overrides, and extension targets

| [INDEX] | [SURFACE]                        | [SHAPE]  | [CAPABILITY]                                       |
| :-----: | :------------------------------- | :------- | :------------------------------------------------- |
|  [01]   | `Protobuf_ProtocFullPath`        | property | binds a host-provided `protoc`                     |
|  [02]   | `gRPC_PluginFullPath`            | property | binds a host-provided `grpc_csharp_plugin`         |
|  [03]   | `Protobuf_ToolsOs`               | property | forces the packaged-compiler OS segment            |
|  [04]   | `Protobuf_ToolsCpu`              | property | forces the packaged-compiler CPU segment           |
|  [05]   | `Protobuf_OutputPath`            | property | project-wide message-sink root                     |
|  [06]   | `Protobuf_DepFilesPath`          | property | `.protodep` dependency-file store                  |
|  [07]   | `Protobuf_StandardImportsPath`   | property | well-known-proto import root                       |
|  [08]   | `EnableDefaultProtobufItems`     | property | globs `**/*.proto` into `Protobuf`, off by default |
|  [09]   | `DisableProtobufDesignTimeBuild` | property | suppresses codegen and platform resolution         |
|  [10]   | `Protobuf_NoOrphanWarning`       | property | silences the no-known-outputs warning              |
|  [11]   | `Protobuf_TouchMissingExpected`  | property | creates missing outputs outside `obj/`             |
|  [12]   | `Protobuf_NoWarnMissingExpected` | property | silences the missing-output warning                |
|  [13]   | `PROTOBUF_PROTOC`                | env      | binds a host-provided `protoc`                     |
|  [14]   | `GRPC_PROTOC_PLUGIN`             | env      | binds a host-provided plugin                       |
|  [15]   | `PROTOBUF_TOOLS_OS`              | env      | forces the packaged-compiler OS segment            |
|  [16]   | `PROTOBUF_TOOLS_CPU`             | env      | forces the packaged-compiler CPU segment           |
|  [17]   | `Protobuf_Compile`               | target   | runs codegen outside the C# `BeforeCompile` hook   |
|  [18]   | `Protobuf_Clean`                 | target   | deletes expected outputs and `.protodep` files     |
|  [19]   | `Protobuf_BeforeCompile`         | target   | pre-codegen extension point                        |
|  [20]   | `Protobuf_AfterCompile`          | target   | post-codegen extension point                       |
|  [21]   | `Protobuf_PrepareCompileOptions` | target   | metadata-to-option lowering extension point        |
|  [22]   | `Protobuf_ResolvePlatform`       | target   | compiler-RID resolution extension point            |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Protobuf` items fold through one chain: selection roots each entry, `Protobuf_PrepareCompileOptions` lowers metadata to option strings, `ProtoCompile` runs the compiler, and reconciliation settles the expected outputs.
- `Access=Internal` appends `internal_access` to both the message and the stub option list; `GrpcServices=Client` appends `no_server`, `Server` appends `no_client`, and `None` skips the plugin leg entirely.
- `OutputOptions` and `GrpcOutputOptions` pass ahead of the lowered options, so a raw flag the named metadata omits reaches the compiler without a custom target.
- `ProtoPath` concatenates `AdditionalImportDirs`, `Protobuf_StandardImportsPath`, then `ProtoRoot`.
- Message code and stub code land in separate sinks; `CompileOutputs` decides whether either joins the C# compilation.
- Each message emits an `IMessage<T>`/`IBufferMessage` class carrying a static `Parser` and `Descriptor`; each service with a stub leg emits `<Service>.<Service>Client` over a `CallInvoker` and `<Service>.<Service>Base` for hosting.
- Up-to-date checking rides inferred `Protobuf_ExpectedOutputs` against `.protodep` files, so a proto with no inferable output recompiles every build.
- Apple-silicon hosts run the `macosx_x64` compiler pair under Rosetta 2, no `macosx_arm64` asset shipping; `PROTOBUF_PROTOC` and `GRPC_PROTOC_PLUGIN` bind a native pair instead.

[STACKING]:
- `Google.Protobuf`(`.api/api-protobuf.md`): generated messages implement `IMessage<T>` and `IBufferMessage`, exposing the static `MessageParser<T>` and `MessageDescriptor` for decode and reflection; `field_mask.proto` lands the `FieldMask` the partial-update apply leg selects on.
- `Grpc.Net.Client`(`.api/api-grpc-client.md`): a generated `<Service>Client` binds `GrpcChannel.CreateCallInvoker`, so `CallInvokerExtensions.Intercept` layers policy under the stub, and each method resolves to `AsyncUnaryCall<T>` or one of the three streaming call shapes.
- `NodaTime.Serialization.Protobuf`(`.api/api-nodatime-protobuf.md`): `Timestamp` and `Duration`, landed by this import root, project inward through `ToInstant`/`ToNodaDuration` and outward through `ToTimestamp`/`ToProtobufDuration` at the seam.
- `Grpc.AspNetCore`(`.api/api-grpc-aspnetcore.md`): `GrpcServices=Server` emits the `<Service>Base` class `MapGrpcService<TService>` routes, and `Access` fixes that type's visibility.
- `Microsoft.IO.RecyclableMemoryStream`(`Rasm.Compute/.api/api-recyclable-stream.md`): generated messages write into and parse out of the pooled `IBufferWriter<byte>`/`ReadOnlySequence<byte>` staging buffer without an intermediate array.
- `Riok.Mapperly`(`.api/api-mapperly.md`): a `[Mapper]` transcribes each generated message to its domain owner per case, so a generated type never crosses the package seam.
- within-lib: one `.proto` set compiles per consumer — `GrpcServices=None` for message-only contracts, `Client` in Compute, `Server` at the app root, which emits the descriptor set feeding the contract checksum gate and the browser codegen.

[LOCAL_ADMISSION]:
- `.proto` files live beside the owner minting their contract and enter the build as that project's `Protobuf` items.
- Generated code is the runtime surface and the generator package is not.
- Generated clients are adapters and never own execution policy.
- `Access=Internal` is the admitted default, leaving the domain parity owner the public seam; `Public` binds where the contract crosses a package boundary.

[RAIL_LAW]:
- Package: `Grpc.Tools`
- Owns: build-time `.proto` compilation to C# messages and stubs, and the well-known import root every contract resolves against
- Accept: `Protobuf` items whose metadata selects stub kind, type visibility, and output sinks, with the option metadata carrying any raw compiler flag
- Reject: a hand-written wire DTO, a bespoke compiler-invocation target, and checked-in generated source

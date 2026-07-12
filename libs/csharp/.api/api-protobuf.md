# [RASM_API_PROTOBUF]

`Google.Protobuf` owns generated wire contracts and their binary, buffered, reflected, and JSON projections for the Compute remote lane. Its `IBufferMessage` path writes into `IBufferWriter<byte>`, while the same bytes carry content identity. Browser clients decode the wire through `protobufjs` or `@bufbuild` at the `Rasm.AppUi` seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.Protobuf`

- package: `Google.Protobuf` (version, direct pin)
- license: BSD-3-Clause (`protocolbuffers/protobuf`)
- assembly: `Google.Protobuf`
- managed-tfm: multi-target (`net45`, `net5.0`, `netstandard1.1`, `netstandard2.0`); the `net10.0` consumer binds `lib/net5.0`, whose surface carries the `ReadOnlySpan<byte>`, `ReadOnlySequence<byte>`, and `IBufferWriter<byte>` overloads absent from `lib/netstandard2.0`
- namespaces: `Google.Protobuf`, `Google.Protobuf.Collections`, `Google.Protobuf.Reflection`, `Google.Protobuf.WellKnownTypes`
- asset: managed runtime library; `Google.Protobuf.Tools` (`protoc`) and the gRPC `.proto` compile are separate package owners
- transitive floor: bound transitively by `Google.OrTools` (`MPModelProto` carriers) and `NodaTime.Serialization.Protobuf`; the central pin wins the version-conflict resolution
- rail: remote-contracts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message, codec, and buffer-context contracts

- rail: remote-contracts

`IBufferMessage` exposes `InternalMergeFrom(ref ParseContext)` and `InternalWriteTo(ref WriteContext)`, while `FieldCodec<T>` is the reusable read, write, and size unit behind each generated accessor. `WireFormat.MakeTag(int, WireFormat.WireType)` builds a field tag, while `GetTagWireType` and `GetTagFieldNumber` project its number and wire type.

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]      | [CAPABILITY]                      |
| :-----: | :------------------------------- | :------------------ | :-------------------------------- |
|  [01]   | `IMessage`                       | message contract    | descriptor with binary operations |
|  [02]   | `IMessage<T>`                    | message contract    | typed equality and cloning        |
|  [03]   | `IBufferMessage`                 | buffer contract     | generated buffer entry            |
|  [04]   | `IDeepCloneable<T>`              | clone contract      | structural deep copy              |
|  [05]   | `MessageParser<T>`               | parser              | typed message decoding            |
|  [06]   | `CodedInputStream`               | binary reader       | bounded binary decoding           |
|  [07]   | `CodedOutputStream`              | binary writer       | binary encoding with size costing |
|  [08]   | `ParseContext`                   | ref parse context   | generated parse cursor            |
|  [09]   | `WriteContext`                   | ref write context   | generated write cursor            |
|  [10]   | `FieldCodec<T>`                  | per-field codec     | generated accessor codec          |
|  [11]   | `ByteString`                     | immutable bytes     | immutable wire-byte ownership     |
|  [12]   | `JsonFormatter`                  | JSON codec          | message projection                |
|  [13]   | `JsonParser`                     | JSON codec          | message reconstruction            |
|  [14]   | `InvalidProtocolBufferException` | protocol fault rail | binary parse failure              |
|  [15]   | `InvalidJsonException`           | JSON fault rail     | JSON parse failure                |
|  [16]   | `WireFormat`                     | wire tag helper     | tag construction and projection   |

`ParseContext` and `WriteContext` are `ref struct` cursors driven only by generated code. `InvalidProtocolBufferException` and `InvalidJsonException` inherit `IOException`.

`WireFormat.WireType` is the closed wire-tag vocabulary.

| [INDEX] | [VARIANT]         |
| :-----: | :---------------- |
|  [01]   | `Varint`          |
|  [02]   | `Fixed64`         |
|  [03]   | `LengthDelimited` |
|  [04]   | `StartGroup`      |
|  [05]   | `EndGroup`        |
|  [06]   | `Fixed32`         |

[PUBLIC_TYPE_SCOPE]: collection and reflection contracts

- rail: remote-contracts

| [INDEX] | [SYMBOL]                     | [PACKAGE_ROLE]      | [CAPABILITY]                   |
| :-----: | :--------------------------- | :------------------ | :----------------------------- |
|  [01]   | `RepeatedField<T>`           | repeated collection | codec-driven repeated fields   |
|  [02]   | `MapField<TKey, TValue>`     | map collection      | codec-driven map fields        |
|  [03]   | `UnknownFieldSet`            | unknown field store | unknown-field preservation     |
|  [04]   | `ExtensionRegistry`          | extension registry  | proto2 extension resolution    |
|  [05]   | `Extension`                  | extension base      | field-number identity          |
|  [06]   | `Extension<TTarget, TValue>` | extension handle    | typed extension field          |
|  [07]   | `FileDescriptor`             | reflection metadata | file-level descriptor graph    |
|  [08]   | `MessageDescriptor`          | reflection metadata | message-level descriptor graph |
|  [09]   | `FieldDescriptor`            | reflection metadata | field-level descriptor graph   |
|  [10]   | `EnumDescriptor`             | reflection metadata | enum-value descriptor graph    |
|  [11]   | `ServiceDescriptor`          | reflection metadata | RPC service descriptor graph   |
|  [12]   | `MethodDescriptor`           | reflection metadata | RPC method descriptor graph    |
|  [13]   | `TypeRegistry`               | type registry       | typed payload resolution       |
|  [14]   | `GeneratedClrTypeInfo`       | generated binding   | generated CLR metadata binding |
|  [15]   | `OriginalNameAttribute`      | generated attribute | protocol-name preservation     |

[PUBLIC_TYPE_SCOPE]: well-known contracts

- rail: remote-contracts
- note: namespace `Google.Protobuf.WellKnownTypes`; each row is a generated message, while temporal rows add CLR conversion helpers

| [INDEX] | [SYMBOL]    | [PACKAGE_ROLE]  | [CAPABILITY]             |
| :-----: | :---------- | :-------------- | :----------------------- |
|  [01]   | `Any`       | typed envelope  | typed payload packing    |
|  [02]   | `Timestamp` | instant message | timestamp conversion     |
|  [03]   | `Duration`  | span message    | duration conversion      |
|  [04]   | `FieldMask` | field selector  | path selection and merge |
|  [05]   | `Struct`    | dynamic object  | string-keyed values      |
|  [06]   | `Value`     | dynamic value   | dynamic value union      |
|  [07]   | `ListValue` | dynamic list    | ordered dynamic values   |
|  [08]   | `Empty`     | empty message   | no-payload RPC marker    |

Every scalar wrapper is a generated message over one CLR primitive.

| [INDEX] | [SYMBOL]      | [PRIMITIVE]  |
| :-----: | :------------ | :----------- |
|  [01]   | `DoubleValue` | `double`     |
|  [02]   | `FloatValue`  | `float`      |
|  [03]   | `Int64Value`  | `long`       |
|  [04]   | `UInt64Value` | `ulong`      |
|  [05]   | `Int32Value`  | `int`        |
|  [06]   | `UInt32Value` | `uint`       |
|  [07]   | `BoolValue`   | `bool`       |
|  [08]   | `StringValue` | `string`     |
|  [09]   | `BytesValue`  | `ByteString` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stream and copy message operations

- rail: remote-contracts
- note: parser operations decode one typed message, parser policies drop unknown fields or bind an extension registry, and copy operations materialize owned bytes.

| [INDEX] | [SURFACE]                                   | [INPUT]             | [RESULT]           | [CAPABILITY]                 |
| :-----: | :------------------------------------------ | :------------------ | :----------------- | :--------------------------- |
|  [01]   | `MessageParser<T>.ParseFrom`                | `byte[]`            | `T`                | typed message parse          |
|  [02]   | `MessageParser<T>.ParseFrom`                | `ByteString`        | `T`                | typed message parse          |
|  [03]   | `MessageParser<T>.ParseFrom`                | `Stream`            | `T`                | typed message parse          |
|  [04]   | `MessageParser<T>.ParseFrom`                | `CodedInputStream`  | `T`                | typed message parse          |
|  [05]   | `MessageParser<T>.ParseDelimitedFrom`       | `Stream`            | `T`                | length-prefixed parse        |
|  [06]   | `MessageParser<T>.ParseJson`                | `string`            | `T`                | typed JSON parse             |
|  [07]   | `MessageParser<T>.WithDiscardUnknownFields` | `bool`              | `MessageParser<T>` | unknown-field policy         |
|  [08]   | `MessageParser<T>.WithExtensionRegistry`    | `ExtensionRegistry` | `MessageParser<T>` | extension-aware parser       |
|  [09]   | `IMessage.MergeFrom`                        | `CodedInputStream`  | `void`             | in-place message merge       |
|  [10]   | `IMessage.WriteTo`                          | `CodedOutputStream` | `void`             | binary stream write          |
|  [11]   | `IMessage.CalculateSize`                    | —                   | `int`              | exact serialized size        |
|  [12]   | `MessageExtensions.ToByteArray`             | `IMessage`          | `byte[]`           | managed byte copy            |
|  [13]   | `MessageExtensions.ToByteString`            | `IMessage`          | `ByteString`       | immutable byte copy          |
|  [14]   | `MessageExtensions.WriteTo`                 | `IMessage, Stream`  | `void`             | stream write                 |
|  [15]   | `MessageExtensions.WriteDelimitedTo`        | `IMessage, Stream`  | `void`             | length-prefixed stream write |
|  [16]   | `IDeepCloneable<T>.Clone`                   | —                   | `T`                | structural deep copy         |

[ENTRYPOINT_SCOPE]: buffer fast-path parse and write operations

- rail: remote-contracts#ARTIFACT_SYNC
- note: `ParseFrom` returns `T` without stream allocation. The extension operations bind `this IMessage`; `MergeFrom` mutates it, write operations emit it into pooled or pre-sized targets, and `WriteLengthPrefixedTo` adds a varint length prefix. The `lib/net5.0` asset carries these overloads, while `lib/netstandard2.0` does not.

| [INDEX] | [SURFACE]                                 | [BUFFER]                 | [RESULT] |
| :-----: | :---------------------------------------- | :----------------------- | :------- |
|  [01]   | `MessageParser<T>.ParseFrom`              | `ReadOnlySpan<byte>`     | `T`      |
|  [02]   | `MessageParser<T>.ParseFrom`              | `ReadOnlySequence<byte>` | `T`      |
|  [03]   | `MessageExtensions.MergeFrom`             | `ReadOnlySpan<byte>`     | `void`   |
|  [04]   | `MessageExtensions.MergeFrom`             | `ReadOnlySequence<byte>` | `void`   |
|  [05]   | `MessageExtensions.WriteTo`               | `IBufferWriter<byte>`    | `void`   |
|  [06]   | `MessageExtensions.WriteTo`               | `Span<byte>`             | `void`   |
|  [07]   | `MessageExtensions.WriteLengthPrefixedTo` | `IBufferWriter<byte>`    | `void`   |

[ENTRYPOINT_SCOPE]: `ByteString` and no-copy buffer ownership

- rail: remote-contracts

`ByteString` is the immutable wire-byte carrier. Copy factories own their bytes, `Span` and `Memory` expose zero-copy views, and `UnsafeByteOperations.UnsafeWrap` adopts caller-owned memory whose contents remain unchanged for the `ByteString` lifetime.

| [INDEX] | [SURFACE]                         | [INPUT]                     | [RESULT]               |
| :-----: | :-------------------------------- | :-------------------------- | :--------------------- |
|  [01]   | `ByteString.CopyFrom`             | `ReadOnlySpan<byte>`        | `ByteString`           |
|  [02]   | `ByteString.CopyFrom`             | `byte[]`                    | `ByteString`           |
|  [03]   | `ByteString.CopyFrom`             | `byte[], int, int`          | `ByteString`           |
|  [04]   | `ByteString.CopyFromUtf8`         | `string`                    | `ByteString`           |
|  [05]   | `ByteString.FromStream`           | `Stream`                    | `ByteString`           |
|  [06]   | `ByteString.FromStreamAsync`      | `Stream, CancellationToken` | `Task<ByteString>`     |
|  [07]   | `ByteString.FromBase64`           | `string`                    | `ByteString`           |
|  [08]   | `ByteString.ToBase64`             | —                           | `string`               |
|  [09]   | `ByteString.ToStringUtf8`         | —                           | `string`               |
|  [10]   | `ByteString.CreateCodedInput`     | —                           | `CodedInputStream`     |
|  [11]   | `ByteString.Span`                 | —                           | `ReadOnlySpan<byte>`   |
|  [12]   | `ByteString.Memory`               | —                           | `ReadOnlyMemory<byte>` |
|  [13]   | `UnsafeByteOperations.UnsafeWrap` | `ReadOnlyMemory<byte>`      | `ByteString`           |

[ENTRYPOINT_SCOPE]: any-envelope, field-mask, and well-known operations

- rail: remote-contracts
- note: `Any.Unpack<T>` throws on a type mismatch, while `TryUnpack<T>` returns a boolean verdict. Typed `FieldMask.FromString<T>` validates the parsed comma-path mask, `Normalize` sorts, deduplicates, and prunes redundant subpaths, and `Merge` copies masked paths from source to destination with an optional policy.

| [INDEX] | [SURFACE]                       | [INPUT]                                                       | [RESULT]         |
| :-----: | :------------------------------ | :------------------------------------------------------------ | :--------------- |
|  [01]   | `Any.Pack`                      | `IMessage`                                                    | `Any`            |
|  [02]   | `Any.Pack`                      | `IMessage, string typeUrlPrefix`                              | `Any`            |
|  [03]   | `Any.Unpack<T>`                 | —                                                             | `T`              |
|  [04]   | `Any.TryUnpack<T>`              | `out T`                                                       | `bool`           |
|  [05]   | `Any.Unpack`                    | `TypeRegistry`                                                | `IMessage`       |
|  [06]   | `Any.Is`                        | `MessageDescriptor`                                           | `bool`           |
|  [07]   | `Any.GetTypeName`               | `string typeUrl`                                              | `string`         |
|  [08]   | `FieldMask.FromString`          | `string`                                                      | `FieldMask`      |
|  [09]   | `FieldMask.FromString<T>`       | `string`                                                      | `FieldMask`      |
|  [10]   | `FieldMask.FromFieldNumbers<T>` | `IEnumerable<int>`                                            | `FieldMask`      |
|  [11]   | `FieldMask.IsValid`             | `MessageDescriptor, FieldMask`                                | `bool`           |
|  [12]   | `FieldMask.IsValid`             | `MessageDescriptor, string`                                   | `bool`           |
|  [13]   | `FieldMask.Normalize`           | —                                                             | `FieldMask`      |
|  [14]   | `Timestamp.FromDateTime`        | `DateTime`                                                    | `Timestamp`      |
|  [15]   | `Timestamp.ToDateTime`          | —                                                             | `DateTime`       |
|  [16]   | `Timestamp.FromDateTimeOffset`  | `DateTimeOffset`                                              | `Timestamp`      |
|  [17]   | `Timestamp.ToDateTimeOffset`    | —                                                             | `DateTimeOffset` |
|  [18]   | `Duration.FromTimeSpan`         | `TimeSpan`                                                    | `Duration`       |
|  [19]   | `Duration.ToTimeSpan`           | —                                                             | `TimeSpan`       |
|  [20]   | `Value.ForString`               | `string`                                                      | `Value`          |
|  [21]   | `Value.ForNumber`               | `double`                                                      | `Value`          |
|  [22]   | `Value.ForBool`                 | `bool`                                                        | `Value`          |
|  [23]   | `Value.ForStruct`               | `Struct`                                                      | `Value`          |
|  [24]   | `Value.ForList`                 | `params Value[]`                                              | `Value`          |
|  [25]   | `Value.ForNull`                 | —                                                             | `Value`          |
|  [26]   | `FieldMask.Union`               | `params FieldMask[] otherMasks`                               | `FieldMask`      |
|  [27]   | `FieldMask.Merge`               | `IMessage source, IMessage destination`                       | `void`           |
|  [28]   | `FieldMask.Merge`               | `IMessage source, IMessage destination, MergeOptions options` | `void`           |

`FieldMask.MergeOptions` is sealed. Its defaults merge messages, append repeated values, and copy source primitive values; replacement flags discard existing message or repeated content and preserve source presence semantics for primitives.

| [INDEX] | [PROPERTY]               | [TYPE] |
| :-----: | :----------------------- | :----- |
|  [01]   | `ReplaceMessageFields`   | `bool` |
|  [02]   | `ReplaceRepeatedFields`  | `bool` |
|  [03]   | `ReplacePrimitiveFields` | `bool` |

[ENTRYPOINT_SCOPE]: JSON projection

- rail: remote-contracts
- note: `JsonFormatter`/`JsonParser` carry an immutable `Settings` builder; reuse one configured formatter, never per-call construction.

Formatting targets a string or `TextWriter`; parser overloads target a generic message or a runtime `MessageDescriptor`. Formatter policy controls default values, type resolution, enum projection, and proto-name preservation, while parser policy controls recursion, type resolution, and unknown fields. `ToDiagnosticString` emits best-effort diagnostic JSON.

| [INDEX] | [SURFACE]                                            | [INPUT]                     | [RESULT]   |
| :-----: | :--------------------------------------------------- | :-------------------------- | :--------- |
|  [01]   | `JsonFormatter.Format`                               | `IMessage`                  | `string`   |
|  [02]   | `JsonFormatter.Format`                               | `IMessage, TextWriter`      | `void`     |
|  [03]   | `JsonFormatter.ToDiagnosticString`                   | `IMessage`                  | `string`   |
|  [04]   | `JsonFormatter.Settings.WithFormatDefaultValues`     | `bool`                      | `Settings` |
|  [05]   | `JsonFormatter.Settings.WithTypeRegistry`            | `TypeRegistry`              | `Settings` |
|  [06]   | `JsonFormatter.Settings.WithFormatEnumsAsIntegers`   | `bool`                      | `Settings` |
|  [07]   | `JsonFormatter.Settings.WithPreserveProtoFieldNames` | `bool`                      | `Settings` |
|  [08]   | `JsonParser.Parse<T>`                                | `string`                    | `T`        |
|  [09]   | `JsonParser.Parse`                                   | `string, MessageDescriptor` | `IMessage` |
|  [10]   | `new JsonParser.Settings`                            | `int, TypeRegistry`         | `Settings` |
|  [11]   | `JsonParser.Settings.WithIgnoreUnknownFields`        | `bool`                      | `Settings` |

[ENTRYPOINT_SCOPE]: reflection descriptor graph

- rail: remote-contracts#CONTRACT_EVOLUTION
- note: the descriptor-diff contract-evolution law walks this surface to fold its `XxHash128` projection checksum.
- note: `DescriptorBase` gives every descriptor `Name` and `FullName`; surface folds key on those properties

`FileDescriptor.BuildFromByteStrings(IEnumerable<ByteString>)` returns `IReadOnlyList<FileDescriptor>` from serialized `FileDescriptorProto` payloads; its second overload binds an `ExtensionRegistry`. `MessageDescriptor.Fields` orders through `InDeclarationOrder()` or `InFieldNumberOrder()`. `MessageDescriptor.ToProto()` returns `DescriptorProto`, whose end-exclusive `ReservedRange` entries carry `int Start` and `int End` for removed-then-reclaimed detection.

| [INDEX] | [SURFACE]                             | [TYPE]                          | [CAPABILITY]          |
| :-----: | :------------------------------------ | :------------------------------ | :-------------------- |
|  [01]   | `FileDescriptor.BuildFromByteStrings` | `IReadOnlyList<FileDescriptor>` | descriptor restore    |
|  [02]   | `FileDescriptor.MessageTypes`         | `IList<MessageDescriptor>`      | message walk          |
|  [03]   | `FileDescriptor.Services`             | `IList<ServiceDescriptor>`      | service walk          |
|  [04]   | `FileDescriptor.EnumTypes`            | `IList<EnumDescriptor>`         | enum walk             |
|  [05]   | `FileDescriptor.Dependencies`         | `IList<FileDescriptor>`         | dependency walk       |
|  [06]   | `MessageDescriptor.Fields`            | `FieldCollection`               | ordered field walk    |
|  [07]   | `MessageDescriptor.FindFieldByNumber` | `FieldDescriptor?`              | wire-number lookup    |
|  [08]   | `MessageDescriptor.FindFieldByName`   | `FieldDescriptor?`              | name lookup           |
|  [09]   | `MessageDescriptor.Parser`            | `MessageParser?`                | message parser        |
|  [10]   | `MessageDescriptor.ClrType`           | `Type?`                         | CLR type              |
|  [11]   | `MessageDescriptor.Oneofs`            | `IList<OneofDescriptor>`        | oneof walk            |
|  [12]   | `FieldDescriptor.FieldNumber`         | `int`                           | wire number           |
|  [13]   | `FieldDescriptor.FieldType`           | `FieldType`                     | wire type             |
|  [14]   | `FieldDescriptor.JsonName`            | `string`                        | JSON name             |
|  [15]   | `FieldDescriptor.IsRepeated`          | `bool`                          | repeated predicate    |
|  [16]   | `FieldDescriptor.IsMap`               | `bool`                          | map predicate         |
|  [17]   | `FieldDescriptor.IsPacked`            | `bool`                          | packed predicate      |
|  [18]   | `FieldDescriptor.Accessor`            | `IFieldAccessor`                | reflective access     |
|  [19]   | `FieldDescriptor.ContainingOneof`     | `OneofDescriptor?`              | oneof membership      |
|  [20]   | `ServiceDescriptor.Methods`           | `IList<MethodDescriptor>`       | RPC walk              |
|  [21]   | `TypeRegistry.FromFiles`              | `TypeRegistry`                  | file registry         |
|  [22]   | `TypeRegistry.FromMessages`           | `TypeRegistry`                  | message registry      |
|  [23]   | `MessageDescriptor.NestedTypes`       | `IList<MessageDescriptor>`      | nested-message walk   |
|  [24]   | `MessageDescriptor.EnumTypes`         | `IList<EnumDescriptor>`         | nested-enum walk      |
|  [25]   | `MessageDescriptor.ToProto`           | `DescriptorProto`               | reserved-range access |
|  [26]   | `EnumDescriptor.Values`               | `IList<EnumValueDescriptor>`    | enum-value walk       |
|  [27]   | `EnumValueDescriptor.Name`            | `string`                        | enum name             |
|  [28]   | `EnumValueDescriptor.Number`          | `int`                           | enum number           |
|  [29]   | `OneofDescriptor.Fields`              | `IList<FieldDescriptor>`        | oneof field walk      |
|  [30]   | `MethodDescriptor.InputType`          | `MessageDescriptor`             | RPC input type        |
|  [31]   | `MethodDescriptor.OutputType`         | `MessageDescriptor`             | RPC output type       |
|  [32]   | `MethodDescriptor.IsClientStreaming`  | `bool`                          | client-streaming flag |
|  [33]   | `MethodDescriptor.IsServerStreaming`  | `bool`                          | server-streaming flag |

[ENTRYPOINT_SCOPE]: scalar `FieldCodec` factories

- rail: remote-contracts
- note: `uint tag` selects the wire field; each factory has a tag-only overload and a default-value overload of its `[VALUE]` type

| [INDEX] | [SURFACE]                | [VALUE]      | [RESULT]                 |
| :-----: | :----------------------- | :----------- | :----------------------- |
|  [01]   | `FieldCodec.ForString`   | `string`     | `FieldCodec<string>`     |
|  [02]   | `FieldCodec.ForBytes`    | `ByteString` | `FieldCodec<ByteString>` |
|  [03]   | `FieldCodec.ForBool`     | `bool`       | `FieldCodec<bool>`       |
|  [04]   | `FieldCodec.ForInt32`    | `int`        | `FieldCodec<int>`        |
|  [05]   | `FieldCodec.ForSInt32`   | `int`        | `FieldCodec<int>`        |
|  [06]   | `FieldCodec.ForFixed32`  | `uint`       | `FieldCodec<uint>`       |
|  [07]   | `FieldCodec.ForSFixed32` | `int`        | `FieldCodec<int>`        |
|  [08]   | `FieldCodec.ForUInt32`   | `uint`       | `FieldCodec<uint>`       |
|  [09]   | `FieldCodec.ForInt64`    | `long`       | `FieldCodec<long>`       |
|  [10]   | `FieldCodec.ForSInt64`   | `long`       | `FieldCodec<long>`       |
|  [11]   | `FieldCodec.ForFixed64`  | `ulong`      | `FieldCodec<ulong>`      |
|  [12]   | `FieldCodec.ForSFixed64` | `long`       | `FieldCodec<long>`       |
|  [13]   | `FieldCodec.ForUInt64`   | `ulong`      | `FieldCodec<ulong>`      |
|  [14]   | `FieldCodec.ForFloat`    | `float`      | `FieldCodec<float>`      |
|  [15]   | `FieldCodec.ForDouble`   | `double`     | `FieldCodec<double>`     |

[ENTRYPOINT_SCOPE]: generic and collection field-codec operations

- rail: remote-contracts
- note: generated accessors drive codecs by `ref` context, and repeated or map collections consume them for bulk wire flow. `CalculateSizeWithTag` includes the field tag.

| [INDEX] | [SURFACE]                            | [INPUT]                                             | [RESULT]                       |
| :-----: | :----------------------------------- | :-------------------------------------------------- | :----------------------------- |
|  [01]   | `FieldCodec.ForMessage<T>`           | `uint tag, MessageParser<T>`                        | `FieldCodec<T>`                |
|  [02]   | `FieldCodec.ForEnum<T>`              | `uint tag, Func<T, int>, Func<int, T>`              | `FieldCodec<T>`                |
|  [03]   | `FieldCodec<T>.Read`                 | `ref ParseContext`                                  | `T`                            |
|  [04]   | `FieldCodec<T>.WriteTagAndValue`     | `ref WriteContext, T`                               | `void`                         |
|  [05]   | `FieldCodec<T>.CalculateSizeWithTag` | `T`                                                 | `int`                          |
|  [06]   | `RepeatedField<T>.AddEntriesFrom`    | `ref ParseContext, FieldCodec<T>`                   | `void`                         |
|  [07]   | `RepeatedField<T>.WriteTo`           | `ref WriteContext, FieldCodec<T>`                   | `void`                         |
|  [08]   | `new MapField<TKey, TValue>.Codec`   | `FieldCodec<TKey>, FieldCodec<TValue>, uint mapTag` | `MapField<TKey, TValue>.Codec` |
|  [09]   | `WireFormat.MakeTag`                 | `int fieldNumber, WireFormat.WireType`              | `uint`                         |

[ENTRYPOINT_SCOPE]: `CodedInputStream` / `CodedOutputStream` limits and sizing

- rail: remote-contracts
- note: stream codecs back the non-buffer path. `CreateWithLimits` bounds payload depth and size, the constructor's `leaveOpen` flag retains the backing stream, and each static size member costs its matching wire value. `IMessage.CalculateSize()` sizes a direct `WriteTo(Span<byte>)`, while `SpaceLeft` reports the remaining fixed-buffer capacity.

| [INDEX] | [SURFACE]                                  | [INPUT]                                     | [RESULT]           |
| :-----: | :----------------------------------------- | :------------------------------------------ | :----------------- |
|  [01]   | `CodedInputStream.CreateWithLimits`        | `Stream, int sizeLimit, int recursionLimit` | `CodedInputStream` |
|  [02]   | `CodedInputStream.SizeLimit`               | —                                           | `int`              |
|  [03]   | `CodedInputStream.RecursionLimit`          | —                                           | `int`              |
|  [04]   | `new CodedInputStream`                     | `Stream, bool leaveOpen`                    | `CodedInputStream` |
|  [05]   | `CodedOutputStream.SpaceLeft`              | —                                           | `int`              |
|  [06]   | `CodedOutputStream.ComputeDoubleSize`      | `double`                                    | `int`              |
|  [07]   | `CodedOutputStream.ComputeFloatSize`       | `float`                                     | `int`              |
|  [08]   | `CodedOutputStream.ComputeUInt64Size`      | `ulong`                                     | `int`              |
|  [09]   | `CodedOutputStream.ComputeInt64Size`       | `long`                                      | `int`              |
|  [10]   | `CodedOutputStream.ComputeInt32Size`       | `int`                                       | `int`              |
|  [11]   | `CodedOutputStream.ComputeFixed64Size`     | `ulong`                                     | `int`              |
|  [12]   | `CodedOutputStream.ComputeFixed32Size`     | `uint`                                      | `int`              |
|  [13]   | `CodedOutputStream.ComputeBoolSize`        | `bool`                                      | `int`              |
|  [14]   | `CodedOutputStream.ComputeStringSize`      | `string`                                    | `int`              |
|  [15]   | `CodedOutputStream.ComputeGroupSize`       | `IMessage`                                  | `int`              |
|  [16]   | `CodedOutputStream.ComputeMessageSize`     | `IMessage`                                  | `int`              |
|  [17]   | `CodedOutputStream.ComputeBytesSize`       | `ByteString`                                | `int`              |
|  [18]   | `CodedOutputStream.ComputeUInt32Size`      | `uint`                                      | `int`              |
|  [19]   | `CodedOutputStream.ComputeEnumSize`        | `int`                                       | `int`              |
|  [20]   | `CodedOutputStream.ComputeSFixed32Size`    | `int`                                       | `int`              |
|  [21]   | `CodedOutputStream.ComputeSFixed64Size`    | `long`                                      | `int`              |
|  [22]   | `CodedOutputStream.ComputeSInt32Size`      | `int`                                       | `int`              |
|  [23]   | `CodedOutputStream.ComputeSInt64Size`      | `long`                                      | `int`              |
|  [24]   | `CodedOutputStream.ComputeLengthSize`      | `int length`                                | `int`              |
|  [25]   | `CodedOutputStream.ComputeRawVarint32Size` | `uint`                                      | `int`              |
|  [26]   | `CodedOutputStream.ComputeRawVarint64Size` | `ulong`                                     | `int`              |
|  [27]   | `CodedOutputStream.ComputeTagSize`         | `int fieldNumber`                           | `int`              |

## [04]-[IMPLEMENTATION_LAW]

[WIRE_CONTRACTS]:

- namespace: `Google.Protobuf`; generated messages implement `IMessage<T>` and expose a static `Parser` (`MessageParser<T>`) and `Descriptor` (`MessageDescriptor`).
- codec root: `MessageParser<T>`, `CodedInputStream`/`CodedOutputStream`, and `FieldCodec<T>` own binary payload flow; JSON is an edge format, never a replacement for binary remote-contract ownership.
- collection root: `RepeatedField<T>` and `MapField<TKey, TValue>` are generated-contract members, while their `AddEntriesFrom`, `WriteTo`, and `CalculateSize` operations and `MapField<TKey, TValue>.Codec` are the wire primitives a descriptor-driven walker composes
- consumer TFM: the `net10.0` consumer binds `lib/net5.0`, which carries `ParseFrom(ReadOnlySpan<byte>)`, `MergeFrom(ReadOnlySequence<byte>)`, `WriteTo(IBufferWriter<byte>)`, and `WriteLengthPrefixedTo(IBufferWriter<byte>)`; `lib/netstandard2.0` does not

[BUFFER_FAST_PATH]:

- read entry: `MessageParser<T>.ParseFrom(ReadOnlySpan<byte>)` and `MessageParser<T>.ParseFrom(ReadOnlySequence<byte>)` parse pooled buffers without a stream allocation; `MessageExtensions.MergeFrom(ReadOnlySpan<byte>)` and `MessageExtensions.MergeFrom(ReadOnlySequence<byte>)` extend a live message in place
- write entry: `MessageExtensions.WriteTo(IBufferWriter<byte>)`, `MessageExtensions.WriteTo(Span<byte>)`, and `MessageExtensions.WriteLengthPrefixedTo(IBufferWriter<byte>)` emit into pooled or pre-sized buffers; `IMessage.CalculateSize()` pre-sizes a direct `Span<byte>` write
- no-copy aliasing: `UnsafeByteOperations.UnsafeWrap(ReadOnlyMemory<byte>)` adopts caller-owned memory into a `ByteString`; the caller owns buffer lifetime past the message's read window.
- context boundary: `ParseContext`/`WriteContext` `Initialize` factories are package-internal; generated code drives them through `IBufferMessage.InternalMergeFrom(ref ParseContext)` / `InternalWriteTo(ref WriteContext)`, and Compute reaches the fast path only through the public parser, message-extension, and field-codec surfaces — never by constructing a `ref` context.
- field codec: `FieldCodec<T>.Read(ref ParseContext)`, `WriteTagAndValue(ref WriteContext, T)`, and `CalculateSizeWithTag(T)` are the per-field ref-context operations behind generated accessors.

[REFLECTION_CONTRACTS]:

- namespace: `Google.Protobuf.Reflection`; the descriptor graph drives contract inspection
- generated metadata: `GeneratedClrTypeInfo` binds generated CLR types, parsers, and oneof/property names to descriptors; `OriginalNameAttribute` preserves protocol names where C# names diverge.
- contract evolution: `FileDescriptor.BuildFromByteStrings` reconstructs serialized descriptor payloads; `MessageDescriptor.Fields`, `FieldDescriptor.FieldNumber`, `FieldDescriptor.FieldType`, `FieldDescriptor.IsRepeated`, `FieldDescriptor.IsMap`, `FieldDescriptor.IsPacked`, and `ServiceDescriptor.Methods` feed the descriptor-diff `XxHash128` projection checksum

[INTEGRATION_STACK]:

- artifact frame: `IMessage.CalculateSize()` sizes the message, and `MessageExtensions.WriteLengthPrefixedTo(IBufferWriter<byte>)` writes its prefixed body into a `Microsoft.IO.RecyclableMemoryStream` sink. Protobuf owns the message body, while the frame envelope and hash remain outside its boundary.
- frame integrity: `Crc32.HashToUInt32(ReadOnlySpan<byte>)` hashes the contiguous `ByteString` or segment span through `System.IO.Hashing`
- artifact identity: `XxHash128` hashes the pooled `GetReadOnlySequence()` view through `HashToUInt128(ReadOnlySpan<byte>, long seed)` for one segment or incremental `Append` plus `GetCurrentHashAsUInt128` for several segments.
- decode mirror: a received frame's `RecyclableMemoryStream.GetReadOnlySequence()` feeds `MessageParser<T>.ParseFrom(ReadOnlySequence<byte>)` so a fragmented pooled payload decodes without a contiguous copy; `IBufferMessage.InternalMergeFrom` is the generated zero-alloc leg.
- transport: transport services carry `Google.Protobuf` message contracts through `Grpc.Net.Client`; typed `FaultDetail` and `Status` values and `Any`-wrapped payloads decode through `Any.TryUnpack<T>` or `Any.Unpack(TypeRegistry)`
- partial update: a `FieldMask.Paths` set drives partial updates; `FieldMask.IsValid(MessageDescriptor, FieldMask)` gates the paths and `FieldMask.Normalize()` deduplicates them before the descriptor accessor applies the merge
- clock seam: `NodaTime.Serialization.Protobuf` maps `Timestamp` and `Duration` through `Instant.ToTimestamp()`, `Duration.ToProtobufDuration()`, `Timestamp.ToInstant()`, and `Duration.ToNodaDuration()`
- calendar seam: the same owner maps Google common date values through `ToDate()`, `ToTimeOfDay()`, `ToProtobufDayOfWeek()`, `ToLocalDate()`, `ToLocalTime()`, and `ToIsoDayOfWeek()`; these extension surfaces own temporal conversion

[LOCAL_ADMISSION]:

- Remote Compute contracts enter through generated `IMessage<T>` surfaces, and the protobuf codec stack exclusively owns their binary payloads.
- JSON projection remains an edge format for diagnostics and browser-adjacent consumers; one reused `JsonFormatter` or `JsonParser` carries explicit `Settings` and `TypeRegistry` policy.
- Reflection descriptors serve diagnostics, contract evolution, and read-only runtime dispatch.

[RAIL_LAW]:

- Package: `Google.Protobuf`
- Owns: generated wire contracts, the codec stack, the buffer fast path, `FieldCodec<T>`, the reflection descriptor graph, the well-known type family, and JSON edge projection
- Accept: generated message contracts over the Span/Sequence/`IBufferWriter` fast path stacked onto the `RecyclableMemoryStream` writer face and the `XxHash128`/`Crc32` content-identity law
- Reject: any parallel owner that bypasses generated messages, public buffer entries, admitted temporal or hash owners, or the selected target-framework asset

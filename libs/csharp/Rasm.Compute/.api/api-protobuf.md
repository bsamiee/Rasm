# [RASM_COMPUTE_API_PROTOBUF]

`Google.Protobuf` supplies generated message contracts, parsers, codecs,
reflection, well-known types, repeated fields, maps, and JSON projection.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.Protobuf`
- package: `Google.Protobuf`
- assembly: `Google.Protobuf`
- namespace: `Google.Protobuf`
- asset: runtime library
- rail: remote-contracts

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message and codec contracts
- rail: remote-contracts

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]     | [CAPABILITY]           |
| :-----: | :------------------------------- | :----------------- | :--------------------- |
|   [1]   | `IMessage`                       | message contract   | defines wire payload   |
|   [2]   | `IMessage<T>`                    | message contract   | defines typed payload  |
|   [3]   | `IBufferMessage`                 | message contract   | enables parse context  |
|   [4]   | `IDeepCloneable<T>`              | clone contract     | duplicates messages    |
|   [5]   | `MessageParser<T>`               | parser             | parses typed payloads  |
|   [6]   | `CodedInputStream`               | binary reader      | reads wire payloads    |
|   [7]   | `CodedOutputStream`              | binary writer      | writes wire payloads   |
|   [8]   | `ParseContext`                   | parse context      | parses buffer payloads |
|   [9]   | `WriteContext`                   | write context      | writes buffer payloads |
|  [10]   | `FieldCodec<T>`                  | field codec        | encodes field values   |
|  [11]   | `ByteString`                     | immutable bytes    | carries binary values  |
|  [12]   | `JsonFormatter`                  | JSON codec         | formats JSON payloads  |
|  [13]   | `JsonParser`                     | JSON codec         | parses JSON payloads   |
|  [14]   | `InvalidProtocolBufferException` | protocol exception | reports parse failures |

[PUBLIC_TYPE_SCOPE]: collection and reflection contracts
- rail: remote-contracts

| [INDEX] | [SYMBOL]                | [PACKAGE_ROLE]      | [CAPABILITY]             |
| :-----: | :---------------------- | :------------------ | :----------------------- |
|   [1]   | `RepeatedField<T>`      | repeated collection | carries repeated fields  |
|   [2]   | `MapField<TKey,TValue>` | map collection      | carries map fields       |
|   [3]   | `UnknownFieldSet`       | unknown field store | preserves unknown fields |
|   [4]   | `ExtensionRegistry`     | extension registry  | resolves extensions      |
|   [5]   | `FileDescriptor`        | reflection metadata | describes proto files    |
|   [6]   | `MessageDescriptor`     | reflection metadata | describes messages       |
|   [7]   | `FieldDescriptor`       | reflection metadata | describes fields         |
|   [8]   | `EnumDescriptor`        | reflection metadata | describes enums          |
|   [9]   | `ServiceDescriptor`     | reflection metadata | describes services       |
|  [10]   | `TypeRegistry`          | type registry       | resolves JSON type names |
|  [11]   | `OriginalNameAttribute` | generated attribute | preserves proto names    |

[PUBLIC_TYPE_SCOPE]: well-known contracts
- rail: remote-contracts

| [INDEX] | [SYMBOL]       | [PACKAGE_ROLE]  | [CAPABILITY]            |
| :-----: | :------------- | :-------------- | :---------------------- |
|   [1]   | `Any`          | wrapper message | carries typed messages  |
|   [2]   | `Timestamp`    | time message    | carries instants        |
|   [3]   | `Duration`     | time message    | carries durations       |
|   [4]   | `FieldMask`    | field selector  | carries field paths     |
|   [5]   | `Struct`       | dynamic object  | carries object values   |
|   [6]   | `Value`        | dynamic value   | carries scalar values   |
|   [7]   | `ListValue`    | dynamic list    | carries list values     |
|   [8]   | `Empty`        | empty message   | marks empty payloads    |
|   [9]   | wrapper values | scalar wrappers | carries nullable values |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message operations
- rail: remote-contracts

| [INDEX] | [SURFACE]              | [CALL_SHAPE]   | [CAPABILITY]            |
| :-----: | :--------------------- | :------------- | :---------------------- |
|   [1]   | `ParseFrom`            | parser call    | parses binary payloads  |
|   [2]   | `MergeFrom`            | merge call     | merges wire payloads    |
|   [3]   | `WriteTo`              | writer call    | writes binary payloads  |
|   [4]   | `CalculateSize`        | size call      | measures wire size      |
|   [5]   | `Clone`                | copy call      | duplicates messages     |
|   [6]   | `ToByteArray`          | extension call | emits byte payloads     |
|   [7]   | `ToByteString`         | extension call | emits immutable bytes   |
|   [8]   | `ToBase64`             | byte call      | emits text bytes        |
|   [9]   | `UnsafeWrap`           | byte call      | wraps binary payload    |
|  [10]   | `UnsafeByteOperations` | helper surface | controls byte ownership |

[ENTRYPOINT_SCOPE]: JSON and reflection operations
- rail: remote-contracts

| [INDEX] | [SURFACE]                             | [CALL_SHAPE]    | [CAPABILITY]         |
| :-----: | :------------------------------------ | :-------------- | :------------------- |
|   [1]   | `JsonFormatter.Format`                | formatter call  | writes JSON payloads |
|   [2]   | `JsonParser.Parse`                    | parser call     | reads JSON payloads  |
|   [3]   | `WithFormatDefaultValues`             | settings call   | sets JSON policy     |
|   [4]   | `WithTypeRegistry`                    | settings call   | sets type registry   |
|   [5]   | `FileDescriptor.BuildFromByteStrings` | descriptor call | builds descriptors   |
|   [6]   | `FieldMask.FromString`                | factory call    | parses field masks   |
|   [7]   | `Timestamp.FromDateTime`              | factory call    | creates timestamp    |
|   [8]   | `Duration.FromTimeSpan`               | factory call    | creates duration     |

[ENTRYPOINT_SCOPE]: any-envelope, descriptor-surface, and parse-bound operations
- rail: remote-contracts

| [INDEX] | [SURFACE]                           | [CALL_SHAPE]     | [CAPABILITY]                                                            |
| :-----: | :---------------------------------- | :--------------- | :---------------------------------------------------------------------- |
|   [1]   | `Any.Pack`                          | factory call     | wraps a typed message                                                   |
|   [2]   | `Any.Unpack`                        | envelope call    | extracts a typed message                                                |
|   [3]   | `Any.Is`                            | envelope call    | tests against a `MessageDescriptor`                                     |
|   [4]   | `ByteString.Span`                   | byte property    | exposes `ReadOnlySpan<byte>`                                            |
|   [5]   | `FileDescriptor.MessageTypes`       | descriptor walk  | lists `MessageDescriptor` rows                                          |
|   [6]   | `FileDescriptor.Services`           | descriptor walk  | lists `ServiceDescriptor` rows                                          |
|   [7]   | `MessageDescriptor.Fields`          | descriptor walk  | field collection with `InDeclarationOrder`                              |
|   [8]   | `FieldDescriptor.FieldNumber`       | descriptor value | wire field number                                                       |
|   [9]   | `FieldDescriptor.FieldType`         | descriptor value | wire field type                                                         |
|  [10]   | `ServiceDescriptor.Methods`         | descriptor walk  | lists `MethodDescriptor` rows                                           |
|  [11]   | `MessageParser<T>.ParseFrom`        | parser call      | accepts `ReadOnlySequence<byte>` fragmented payloads                    |
|  [12]   | `CodedInputStream.CreateWithLimits` | factory call     | binds size and recursion limits; package default recursion limit is 100 |

[ENTRYPOINT_SCOPE]: buffer fast-path parse and write operations
- rail: remote-contracts

| [INDEX] | [SURFACE]                                 | [CALL_SHAPE] | [CAPABILITY]                                                                   |
| :-----: | :---------------------------------------- | :----------- | :----------------------------------------------------------------------------- |
|   [1]   | `MessageParser.ParseFrom`                 | parser call  | `(ReadOnlySpan<byte>)` parses a contiguous buffer with no stream allocation    |
|   [2]   | `MessageParser.ParseFrom`                 | parser call  | `(ReadOnlySequence<byte>)` parses a fragmented buffer                          |
|   [3]   | `MessageExtensions.MergeFrom`             | merge call   | `(this IMessage, ReadOnlySpan<byte>)` merges a contiguous buffer               |
|   [4]   | `MessageExtensions.MergeFrom`             | merge call   | `(this IMessage, ReadOnlySequence<byte>)` merges a fragmented buffer           |
|   [5]   | `MessageExtensions.WriteTo`               | writer call  | `(this IMessage, IBufferWriter<byte>)` writes into a pooled writer             |
|   [6]   | `MessageExtensions.WriteTo`               | writer call  | `(this IMessage, Span<byte>)` writes into a pre-sized buffer                   |
|   [7]   | `MessageExtensions.WriteLengthPrefixedTo` | writer call  | `(this IMessage, IBufferWriter<byte>)` writes a varint-length-prefixed payload |
|   [8]   | `UnsafeByteOperations.UnsafeWrap`         | byte call    | `(ReadOnlyMemory<byte>)` aliases backing memory into a `ByteString` no-copy    |

[ENTRYPOINT_SCOPE]: buffer-message and field-codec context operations
- rail: remote-contracts

Generated buffer-message methods operate by `ref ParseContext` and `ref WriteContext`; field-codec rows keep exact parameter shapes out of cells.

[BUFFER_MESSAGE_OPERATIONS]:
| [INDEX] | [SURFACE]                          | [CALL_SHAPE] | [CAPABILITY]                        |
| :-----: | :--------------------------------- | :----------- | :---------------------------------- |
|   [1]   | `IBufferMessage.InternalMergeFrom` | parse call   | generated buffer-driven merge entry |
|   [2]   | `IBufferMessage.InternalWriteTo`   | write call   | generated buffer-driven write entry |

[PARSE_WRITE_CONTEXT_OPERATIONS]:
| [INDEX] | [SURFACE]                                                          | [CALL_SHAPE] | [CAPABILITY]                             |
| :-----: | :----------------------------------------------------------------- | :----------- | :--------------------------------------- |
|   [1]   | `ParseContext.ReadTag`                                             | read call    | reads the next field tag                 |
|   [2]   | `ParseContext.ReadMessage`                                         | read call    | reads a nested length-delimited message  |
|   [3]   | `ParseContext.ReadString` / `ReadBytes`                            | read call    | reads string or `ByteString` fields      |
|   [4]   | `ParseContext.ReadInt32` / `ReadInt64` / `ReadDouble` / `ReadBool` | read call    | reads scalar fields by wire type         |
|   [5]   | `WriteContext.WriteTag`                                            | write call   | writes a field tag                       |
|   [6]   | `WriteContext.WriteMessage`                                        | write call   | writes a nested length-delimited message |
|   [7]   | `WriteContext.WriteString` / `WriteBytes`                          | write call   | writes string or `ByteString` fields     |
|   [8]   | `WriteContext.WriteRawTag`                                         | write call   | emits precomputed tag bytes              |

[FIELD_CODEC_OPERATIONS]:
| [INDEX] | [SURFACE]                            | [CALL_SHAPE] | [CAPABILITY]                        |
| :-----: | :----------------------------------- | :----------- | :---------------------------------- |
|   [1]   | `FieldCodec.For*`                    | factory call | builds scalar field codecs          |
|   [2]   | `FieldCodec.ForMessage<T>`           | factory call | builds a message-field codec        |
|   [3]   | `FieldCodec.ForEnum<T>`              | factory call | builds an enum-field codec          |
|   [4]   | `FieldCodec<T>.Read`                 | read call    | reads a typed field value           |
|   [5]   | `FieldCodec<T>.WriteTagAndValue`     | write call   | writes tag plus value               |
|   [6]   | `FieldCodec<T>.CalculateSizeWithTag` | size call    | measures encoded size including tag |

## [4]-[IMPLEMENTATION_LAW]

[WIRE_CONTRACTS]:
- namespace: `Google.Protobuf`
- contract root: generated messages implement `IMessage<T>`
- codec root: parsers, coded streams, and field codecs own binary payload flow
- collection root: repeated fields and map fields remain generated-contract internals

[BUFFER_FAST_PATH]:
- read entry: `MessageParser.ParseFrom(ReadOnlySpan<byte>)` and `ParseFrom(ReadOnlySequence<byte>)` parse pooled buffers without a stream allocation
- merge entry: `MessageExtensions.MergeFrom(ReadOnlySpan<byte>)` and `MergeFrom(ReadOnlySequence<byte>)` extend a live message from a buffer
- write entry: `MessageExtensions.WriteTo(IBufferWriter<byte>)`, `WriteTo(Span<byte>)`, and `WriteLengthPrefixedTo(IBufferWriter<byte>)` emit into pooled or pre-sized buffers
- no-copy aliasing: `UnsafeByteOperations.UnsafeWrap(ReadOnlyMemory<byte>)` adopts backing memory into a `ByteString` and requires the caller to own buffer lifetime
- context boundary: `ParseContext` and `WriteContext` `Initialize` factories are package-internal; generated code drives them through `IBufferMessage.InternalMergeFrom(ref ParseContext)` and `InternalWriteTo(ref WriteContext)`, and Compute reaches the fast path only through the public parser, message-extension, and field-codec surfaces
- field codec: `FieldCodec<T>.Read(ref ParseContext)`, `WriteTagAndValue(ref WriteContext, T)`, and `CalculateSizeWithTag(T)` are the ref-context per-field operations behind generated accessors

[REFLECTION_CONTRACTS]:
- namespace: `Google.Protobuf.Reflection`
- descriptors: file, message, field, enum, method, and service descriptors drive contract inspection
- generated metadata: `GeneratedClrTypeInfo` binds generated CLR types to descriptors
- naming: `OriginalNameAttribute` preserves protocol names where C# names diverge

[LOCAL_ADMISSION]:
- Remote Compute contracts enter source through generated message surfaces.
- Binary payloads use protobuf codecs and never handwritten byte DTOs.
- JSON projection is an edge format and cannot replace binary remote-contract ownership.
- Reflection descriptors are diagnostics material before they become runtime dispatch material.

[RAIL_LAW]:
- Package: `Google.Protobuf`
- Owns: generated wire contracts and codecs
- Accept: generated message contracts
- Reject: handwritten binary DTOs

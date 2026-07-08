# [RASM_API_PROTOBUF]

`Google.Protobuf` supplies the generated wire-contract runtime the Compute remote
lane projects: `IMessage<T>` message contracts, the `MessageParser<T>` /
`CodedInputStream` / `CodedOutputStream` codec stack, the `IBufferMessage` +
`MessageExtensions` zero-allocation Span/Sequence/`IBufferWriter` fast path, the
`FieldCodec<T>` per-payload encoding primitive, the reflection descriptor graph,
the well-known type family (`Any`, `Timestamp`, `Duration`, `FieldMask`, `Struct`),
and JSON projection at the edge. It is the wire codec under the
`Runtime/wire#PROTO_VOCABULARY` five-service gRPC vocabulary and the
`Runtime/transport#ARTIFACT_FRAMES` `FrameEdge` fold; the `IBufferMessage` fast path
writes into a `Microsoft.IO.RecyclableMemoryStream` `IBufferWriter<byte>` face
(`api-recyclable-stream`) and the same buffer is content-keyed by the suite
`XxHash128` law (`api-hashing` / `Runtime/codecs#CONTENT_ADDRESSING`). This page is
HOST-LOCAL and carries no TS_PROJECTION; the browser projects the wire through its
own `protobufjs`/`@bufbuild` decode at the `Rasm.AppUi` seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.Protobuf`
- package: `Google.Protobuf` (version, direct pin)
- license: BSD-3-Clause (`protocolbuffers/protobuf`)
- assembly: `Google.Protobuf`
- managed-tfm: multi-target (`net45`, `net5.0`, `netstandard1.1`, `netstandard2.0`); the `net10.0` consumer binds `lib/net5.0` — the asset that carries the `ReadOnlySpan`/`ReadOnlySequence`/`IBufferWriter` fast-path overloads. `lib/netstandard2.0` (the `api resolve` default pick) is the lower-API fallback and is NOT the bound surface; verify member presence against `lib/net5.0`.
- namespaces: `Google.Protobuf`, `Google.Protobuf.Collections`, `Google.Protobuf.Reflection`, `Google.Protobuf.WellKnownTypes`
- asset: managed runtime library; `Google.Protobuf.Tools` (`protoc`) and the gRPC `.proto` compile are separate package owners
- transitive floor: bound transitively by `Google.OrTools` (`MPModelProto` carriers) and `NodaTime.Serialization.Protobuf`; the central pin wins the version-conflict resolution
- rail: remote-contracts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message, codec, and buffer-context contracts
- rail: remote-contracts

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:------------------------------- |:------------------ |:----------------------------------------------------------------- |
| [01] | `IMessage` | message contract | untyped wire payload (`MergeFrom`/`WriteTo`/`CalculateSize`) |
| [02] | `IMessage<T>` | message contract | typed payload; carries the static `Parser`/`Descriptor` |
| [03] | `IBufferMessage` | buffer contract | `InternalMergeFrom(ref ParseContext)` / `InternalWriteTo(ref WriteContext)` zero-alloc entry |
| [04] | `IDeepCloneable<T>` | clone contract | structural `Clone()` deep copy |
| [05] | `MessageParser<T>` | parser | typed parse over byte[]/Span/Sequence/Stream/Coded + JSON + config |
| [06] | `CodedInputStream` | binary reader | streaming wire reader with size/recursion limits |
| [07] | `CodedOutputStream` | binary writer | streaming wire writer + static `Compute*Size` cost family |
| [08] | `ParseContext` | ref parse context | `ref struct` buffer parse cursor driven by generated code |
| [09] | `WriteContext` | ref write context | `ref struct` buffer write cursor driven by generated code |
| [10] | `FieldCodec<T>` | per-field codec | the reusable read/write/size unit behind every generated accessor |
| [11] | `ByteString` | immutable bytes | zero-copy `Span`/`Memory` view, base64/utf8/stream factories |
| [12] | `JsonFormatter` | JSON codec | message → JSON with a `Settings` builder |
| [13] | `JsonParser` | JSON codec | JSON → message with a `Settings` builder |
| [14] | `InvalidProtocolBufferException` | protocol fault rail | the binary-codec parse-failure exception (`: IOException`) |
| [15] | `InvalidJsonException` | JSON fault rail | the JSON parse-failure exception (`: IOException`, sibling rail) |
| [16] | `WireFormat` | wire tag helper | `static uint MakeTag(int fieldNumber, WireType)` + `GetTagWireType`/`GetTagFieldNumber` over the `WireType` enum (`Varint`/`Fixed64`/`LengthDelimited`/`StartGroup`/`EndGroup`/`Fixed32`) — builds the tag a `FieldCodec.For*` consumes |

[PUBLIC_TYPE_SCOPE]: collection and reflection contracts
- rail: remote-contracts

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:--------------------------- |:------------------ |:----------------------------------------------------------------- |
| [01] | `RepeatedField<T>` | repeated collection | typed list with codec-driven `AddEntriesFrom`/`WriteTo`/`CalculateSize` |
| [02] | `MapField<TKey,TValue>` | map collection | typed dictionary with a nested `MapField.Codec` map-entry codec |
| [03] | `UnknownFieldSet` | unknown field store | round-trips fields absent from the local schema |
| [04] | `ExtensionRegistry` | extension registry | resolves proto2 extensions during parse |
| [05] | `Extension` / `Extension<T,V>` | extension handle | a single registered extension field |
| [06] | `FileDescriptor` | reflection metadata | message/enum/service/dependency graph + `BuildFromByteStrings` |
| [07] | `MessageDescriptor` | reflection metadata | `Fields`/`Oneofs`/`Parser`/`ClrType` + `FindFieldBy*` |
| [08] | `FieldDescriptor` | reflection metadata | `FieldNumber`/`FieldType`/`Accessor`/`IsRepeated`/`IsMap`/`JsonName` |
| [09] | `EnumDescriptor` | reflection metadata | enum value set |
| [10] | `ServiceDescriptor` | reflection metadata | `Methods` rpc set |
| [11] | `MethodDescriptor` | reflection metadata | input/output type + client/server-streaming flags |
| [12] | `TypeRegistry` | type registry | name→`MessageDescriptor` map for `Any`/JSON resolution |
| [13] | `GeneratedClrTypeInfo` | generated binding | binds generated CLR types, parsers, oneof/property names |
| [14] | `OriginalNameAttribute` | generated attribute | preserves the proto name where the C# name diverges |

[PUBLIC_TYPE_SCOPE]: well-known contracts
- rail: remote-contracts
- note: namespace `Google.Protobuf.WellKnownTypes`; each is a generated message with C#-native conversion helpers.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:-------------------------------------- |:--------------- |:--------------------------------------------------------- |
| [01] | `Any` | typed envelope | `Pack`/`Unpack<T>`/`TryUnpack<T>`/`Is` over a type-URL |
| [02] | `Timestamp` | instant message | `From`/`ToDateTime` + `From`/`ToDateTimeOffset` |
| [03] | `Duration` | span message | `FromTimeSpan` / `ToTimeSpan` |
| [04] | `FieldMask` | field selector | `Paths` `RepeatedField<string>` + `FromString`/`IsValid`/`Normalize` |
| [05] | `Struct` | dynamic object | string→`Value` `MapField` |
| [06] | `Value` | dynamic value | `ForString`/`ForNumber`/`ForBool`/`ForStruct`/`ForList`/`ForNull` |
| [07] | `ListValue` | dynamic list | ordered `Value` sequence |
| [08] | `Empty` | empty message | the no-payload rpc marker |
| [09] | `*Value` wrappers | scalar wrappers | `Int32Value`/`StringValue`/`BoolValue`/… nullable scalars |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: stream and copy message operations
- rail: remote-contracts
- note: the generated-message instance/extension surface for the stream-bound and copy paths; the buffer fast path is the next section.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:------------------------------- |:------------------------------------------------- |:---------------------------------------- |
| [01] | `MessageParser<T>.ParseFrom` | `T ParseFrom(byte[]\| ByteString\| Stream\| CodedInputStream)` | typed parse from a stream-bound source |
| [02] | `MessageParser<T>.ParseDelimitedFrom` | `T ParseDelimitedFrom(Stream)` | parses one length-prefixed message off a stream |
| [03] | `MessageParser<T>.ParseJson` | `T ParseJson(string)` | typed JSON parse via the parser |
| [04] | `MessageParser<T>.WithDiscardUnknownFields` | `MessageParser<T> WithDiscardUnknownFields(bool)` | returns a parser that drops unknown fields |
| [05] | `MessageParser<T>.WithExtensionRegistry` | `MessageParser<T> WithExtensionRegistry(ExtensionRegistry)` | returns an extension-aware parser |
| [06] | `IMessage.MergeFrom` | `void MergeFrom(CodedInputStream)` | extends a live message from the wire |
| [07] | `IMessage.WriteTo` | `void WriteTo(CodedOutputStream)` | writes the message to the wire |
| [08] | `IMessage.CalculateSize` | `int CalculateSize()` | exact serialized byte length |
| [09] | `MessageExtensions.ToByteArray` | `byte[] ToByteArray(this IMessage)` | edge copy to a managed array |
| [10] | `MessageExtensions.ToByteString` | `ByteString ToByteString(this IMessage)` | edge copy to an immutable `ByteString` |
| [11] | `MessageExtensions.WriteTo` | `void WriteTo(this IMessage, Stream)` | streaming write |
| [12] | `MessageExtensions.WriteDelimitedTo` | `void WriteDelimitedTo(this IMessage, Stream)` | length-prefixed streaming write |
| [13] | `IDeepCloneable<T>.Clone` | `T Clone()` | structural deep copy |

[ENTRYPOINT_SCOPE]: buffer fast-path parse and write operations
- rail: remote-contracts#ARTIFACT_SYNC
- note: this is the zero-allocation path the `FrameEdge` fold and the `RecyclableMemoryStream` writer face compose; every overload is present in `lib/net5.0`, absent in `lib/netstandard2.0`.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------------- |:------------------------------------------------------- |:----------------------------------------------------------------------- |
| [01] | `MessageParser<T>.ParseFrom` | `T ParseFrom(ReadOnlySpan<byte>)` | parses a contiguous buffer with no stream allocation |
| [02] | `MessageParser<T>.ParseFrom` | `T ParseFrom(ReadOnlySequence<byte>)` | parses a fragmented buffer — accepts a `RecyclableMemoryStream.GetReadOnlySequence()` |
| [03] | `MessageExtensions.MergeFrom` | `void MergeFrom(this IMessage, ReadOnlySpan<byte>)` | merges a contiguous buffer into a live message |
| [04] | `MessageExtensions.MergeFrom` | `void MergeFrom(this IMessage, ReadOnlySequence<byte>)` | merges a fragmented buffer |
| [05] | `MessageExtensions.WriteTo` | `void WriteTo(this IMessage, IBufferWriter<byte>)` | writes into a pooled writer — the `RecyclableMemoryStream` sink |
| [06] | `MessageExtensions.WriteTo` | `void WriteTo(this IMessage, Span<byte>)` | writes into a pre-sized (`CalculateSize`) buffer |
| [07] | `MessageExtensions.WriteLengthPrefixedTo` | `void WriteLengthPrefixedTo(this IMessage, IBufferWriter<byte>)` | writes a varint-length-prefixed payload — the `FrameEdge` frame body |
| [08] | `ByteString.Span` / `Memory` | `ReadOnlySpan<byte> Span` / `ReadOnlyMemory<byte> Memory` | zero-copy view; `UnsafeByteOperations.UnsafeWrap` aliases without copying |

[ENTRYPOINT_SCOPE]: `ByteString` and no-copy buffer ownership
- rail: remote-contracts

`ByteString` is the immutable wire-byte carrier; `UnsafeByteOperations.UnsafeWrap` adopts caller-owned memory and shifts buffer-lifetime ownership to the caller.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------------- |:-------------------------------------------------- |:-------------------------------------------- |
| [01] | `ByteString.CopyFrom` | `static ByteString CopyFrom(ReadOnlySpan<byte>\| byte[]\| byte[],int,int)` | copies bytes into an immutable `ByteString` |
| [02] | `ByteString.CopyFromUtf8` | `static ByteString CopyFromUtf8(string)` | utf8-encodes a string |
| [03] | `ByteString.FromStream` | `static ByteString FromStream(Stream)` | drains a stream into a `ByteString` |
| [04] | `ByteString.FromStreamAsync` | `static Task<ByteString> FromStreamAsync(Stream, CancellationToken)` | async stream drain |
| [05] | `ByteString.FromBase64` / `ToBase64` | `static ByteString FromBase64(string)` / `string ToBase64()` | base64 edge transcode |
| [06] | `ByteString.ToStringUtf8` | `string ToStringUtf8()` | utf8 decode |
| [07] | `ByteString.CreateCodedInput` | `CodedInputStream CreateCodedInput()` | reads the bytes as a coded stream |
| [08] | `UnsafeByteOperations.UnsafeWrap` | `static ByteString UnsafeWrap(ReadOnlyMemory<byte>)` | aliases backing memory into a `ByteString` no-copy (caller owns lifetime) |

[ENTRYPOINT_SCOPE]: any-envelope, field-mask, and well-known operations
- rail: remote-contracts

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------- |:------------------------------------------------------- |:----------------------------------------------------- |
| [01] | `Any.Pack` | `static Any Pack(IMessage)` / `Pack(IMessage, string typeUrlPrefix)` | wraps a typed message under a type-URL |
| [02] | `Any.Unpack<T>` / `TryUnpack<T>` | `T Unpack<T>()` / `bool TryUnpack<T>(out T)` | extracts a typed message (throwing / Try variant) |
| [03] | `Any.Unpack` | `IMessage Unpack(TypeRegistry)` | resolves the payload type by registry |
| [04] | `Any.Is` | `bool Is(MessageDescriptor)` | tests the type-URL against a descriptor |
| [05] | `Any.GetTypeName` | `static string GetTypeName(string typeUrl)` | strips the prefix to a bare full name |
| [06] | `FieldMask.FromString` | `static FieldMask FromString(string)` / `FromString<T>(string)` | parses a comma-path mask (typed validates against `T`) |
| [07] | `FieldMask.FromFieldNumbers<T>` | `static FieldMask FromFieldNumbers<T>(IEnumerable<int>)` | builds a mask from field numbers |
| [08] | `FieldMask.IsValid` | `static bool IsValid(MessageDescriptor, FieldMask\| string)` | validates mask paths against a descriptor |
| [09] | `FieldMask.Normalize` | `FieldMask Normalize()` | sorts/dedups/prunes redundant sub-paths |
| [10] | `Timestamp.FromDateTimeOffset` | `static Timestamp FromDateTimeOffset(DateTimeOffset)` | offset-aware instant construction |
| [11] | `Duration.FromTimeSpan` / `ToTimeSpan` | `static Duration FromTimeSpan(TimeSpan)` / `TimeSpan ToTimeSpan()` | span transcode |
| [12] | `Value.For*` | `static Value ForString\| ForNumber\| ForBool\| ForStruct\| ForList\| ForNull(...)` | dynamic-value construction for `Struct` graphs |
| [13] | `FieldMask.Union` | `FieldMask Union(params FieldMask[] otherMasks)` | merges two or more masks into one (the multi-tile viewport union) |
| [14] | `FieldMask.Merge` | `void Merge(IMessage source, IMessage destination, MergeOptions options)` / `Merge(IMessage source, IMessage destination)` | the partial-update APPLY leg copies masked paths source→destination |
| [15] | `FieldMask.MergeOptions` | `sealed class { bool ReplaceMessageFields; bool ReplaceRepeatedFields; bool ReplacePrimitiveFields }` | per-field-kind merge policy: replace vs append/merge (default merges) |

[ENTRYPOINT_SCOPE]: JSON projection
- rail: remote-contracts
- note: `JsonFormatter`/`JsonParser` carry an immutable `Settings` builder; reuse one configured formatter, never per-call construction.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------------- |:----------------------------------------------------- |:------------------------------------ |
| [01] | `JsonFormatter.Format` | `string Format(IMessage)` / `void Format(IMessage, TextWriter)` | message → JSON string or writer |
| [02] | `JsonFormatter.ToDiagnosticString` | `static string ToDiagnosticString(IMessage)` | best-effort JSON for logs/diagnostics |
| [03] | `JsonFormatter.Settings.WithFormatDefaultValues` | `Settings WithFormatDefaultValues(bool)` | emit default-valued fields |
| [04] | `JsonFormatter.Settings.WithTypeRegistry` | `Settings WithTypeRegistry(TypeRegistry)` | resolve `Any`/well-known payloads |
| [05] | `JsonFormatter.Settings.WithFormatEnumsAsIntegers` | `Settings WithFormatEnumsAsIntegers(bool)` | numeric vs named enum projection |
| [06] | `JsonFormatter.Settings.WithPreserveProtoFieldNames` | `Settings WithPreserveProtoFieldNames(bool)` | proto snake_case vs JSON camelCase |
| [07] | `JsonParser.Parse` | `T Parse<T>(string)` / `IMessage Parse(string, MessageDescriptor)` | JSON → message |
| [08] | `JsonParser.Settings` | `Settings(int recursionLimit, TypeRegistry)` + `WithIgnoreUnknownFields` | recursion/type/unknown JSON policy |

[ENTRYPOINT_SCOPE]: reflection descriptor graph
- rail: remote-contracts#CONTRACT_EVOLUTION
- note: the descriptor surface the `Runtime/wire` descriptor-diff contract-evolution law walks to fold its `XxHash128` projection-checksum.
- note: every descriptor (`File`/`Message`/`Field`/`Enum`/`EnumValue`/`Oneof`/`Service`/`Method`) exposes `Name` and `FullName` (`DescriptorBase`); the surface-fold keys read these.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:---------------------------------------- |:----------------------------------------------------- |:------------------------------------------------- |
| [01] | `FileDescriptor.BuildFromByteStrings` | `static IReadOnlyList<FileDescriptor> BuildFromByteStrings(IEnumerable<ByteString>, ExtensionRegistry?)` | reconstructs descriptors from a serialized `FileDescriptorSet` |
| [02] | `FileDescriptor.MessageTypes` / `Services` / `EnumTypes` / `Dependencies` | descriptor-graph walk | enumerates the contract surface for diffing |
| [03] | `MessageDescriptor.Fields` | `FieldCollection Fields { InDeclarationOrder()\| InFieldNumberOrder() }` | ordered field walk |
| [04] | `MessageDescriptor.FindFieldByNumber` / `FindFieldByName` | `FieldDescriptor FindFieldBy*(...)` | field lookup by wire number or name |
| [05] | `MessageDescriptor.Parser` / `ClrType` / `Oneofs` | descriptor values | the message parser, CLR type, and oneof set |
| [06] | `FieldDescriptor.FieldNumber` / `FieldType` / `JsonName` | descriptor values | wire number, wire type, JSON name |
| [07] | `FieldDescriptor.IsRepeated` / `IsMap` / `IsPacked` | descriptor flags | shape predicates for the diff law |
| [08] | `FieldDescriptor.Accessor` / `ContainingOneof` | `IFieldAccessor Accessor` / `OneofDescriptor` | reflective get/set + oneof membership |
| [09] | `ServiceDescriptor.Methods` | `MethodDescriptor` walk (`IsClientStreaming`/`IsServerStreaming`) | rpc-shape diff inputs |
| [10] | `TypeRegistry.FromFiles` / `FromMessages` | `static TypeRegistry From*(...)` | builds the `Any`/JSON resolution registry |
| [11] | `MessageDescriptor.NestedTypes` / `EnumTypes` | `IList<MessageDescriptor> NestedTypes` / `IList<EnumDescriptor> EnumTypes` | nested message/enum recursion for the surface-fold |
| [12] | `MessageDescriptor.ToProto` | `DescriptorProto ToProto()` — `.ReservedRange` carries `int Start` / `int End` (end-exclusive) | reserved-number ranges for the removed-then-reclaimed Breaking gate |
| [13] | `EnumDescriptor.Values` / `EnumValueDescriptor` | `IList<EnumValueDescriptor> Values`; `EnumValueDescriptor.Name` / `int Number` | enum value-set walk |
| [14] | `OneofDescriptor.Fields` / `MethodDescriptor.InputType` / `OutputType` | `IList<FieldDescriptor> Fields`; `MethodDescriptor.InputType` / `OutputType` (`MessageDescriptor`) | oneof membership + rpc input/output type for the rpc-shape diff |

[ENTRYPOINT_SCOPE]: `FieldCodec<T>` per-field codec
- rail: remote-contracts
- note: `FieldCodec.For*` builds the reusable codec a generated accessor drives by `ref` context; `RepeatedField<T>`/`MapField<TKey,TValue>` consume a codec for bulk wire flow.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------------- |:----------------------------------------------------- |:-------------------------------------------- |
| [01] | `FieldCodec.For*` | `static FieldCodec<T> ForBool\| ForInt\| ForFixed\| ForDouble\| ForFloat\| ForString\| ForBytes\| ForSInt\| ForSFixed(uint tag,...)` | scalar field codecs by wire tag |
| [02] | `FieldCodec.ForMessage<T>` | `static FieldCodec<T> ForMessage<T>(uint tag, MessageParser<T>)` | message-field codec |
| [03] | `FieldCodec.ForEnum<T>` | `static FieldCodec<T> ForEnum<T>(uint tag, Func<T,int>, Func<int,T>)` | enum-field codec |
| [04] | `FieldCodec<T>.Read` | `T Read(ref ParseContext)` | reads a typed field value |
| [05] | `FieldCodec<T>.WriteTagAndValue` | `void WriteTagAndValue(ref WriteContext, T)` | writes tag plus value |
| [06] | `FieldCodec<T>.CalculateSizeWithTag` | `int CalculateSizeWithTag(T)` | encoded size including the tag |
| [07] | `RepeatedField<T>.AddEntriesFrom` / `WriteTo` | `void AddEntriesFrom(ref ParseContext, FieldCodec<T>)` / `WriteTo(ref WriteContext, FieldCodec<T>)` | bulk repeated read/write through a codec |
| [08] | `MapField<TKey,TValue>.Codec` | `new MapField.Codec(FieldCodec<TKey>, FieldCodec<TValue>, uint mapTag)` | map-entry codec for the map fast path |
| [09] | `WireFormat.MakeTag` | `static uint MakeTag(int fieldNumber, WireFormat.WireType)` over the `WireType` enum (`…/LengthDelimited/…`) | builds the `uint tag` a `FieldCodec.For*` row consumes — `ForBytes(MakeTag(field, LengthDelimited))` |

[ENTRYPOINT_SCOPE]: `CodedInputStream` / `CodedOutputStream` limits and sizing
- rail: remote-contracts
- note: the stream codecs back the non-buffer path; the `Compute*Size` family is the static cost oracle for pre-sizing a `WriteTo(Span<byte>)`.

| [INDEX] | [SURFACE] | [CALL_SHAPE] | [CAPABILITY] |
|:-----: |:----------------------------------- |:----------------------------------------------------- |:-------------------------------------------- |
| [01] | `CodedInputStream.CreateWithLimits` | `static CodedInputStream CreateWithLimits(Stream, int sizeLimit, int recursionLimit)` | bounds untrusted-payload depth and size |
| [02] | `CodedInputStream.SizeLimit` / `RecursionLimit` | `int SizeLimit` / `int RecursionLimit` | reads back the active limits (package default recursion 100) |
| [03] | `CodedInputStream(Stream, bool leaveOpen)` | `CodedInputStream(Stream input, bool leaveOpen)` | leaves the backing stream open on dispose |
| [04] | `CodedOutputStream.Compute*Size` | `static int ComputeInt32Size\| ComputeInt64Size\| ComputeBoolSize\| ComputeDoubleSize\| ComputeStringSize(...)` | per-wire-type size oracle |
| [05] | `CodedOutputStream.SpaceLeft` | `int SpaceLeft` | remaining capacity in a fixed buffer |

## [04]-[IMPLEMENTATION_LAW]

[WIRE_CONTRACTS]:
- namespace: `Google.Protobuf`; generated messages implement `IMessage<T>` and expose a static `Parser` (`MessageParser<T>`) and `Descriptor` (`MessageDescriptor`).
- codec root: `MessageParser<T>`, `CodedInputStream`/`CodedOutputStream`, and `FieldCodec<T>` own binary payload flow; JSON is an edge format, never a replacement for binary remote-contract ownership.
- collection root: `RepeatedField<T>`/`MapField<TKey,TValue>` are generated-contract members, but their `AddEntriesFrom`/`WriteTo`/`CalculateSize` codec methods and `MapField.Codec` are the wire primitives a descriptor-driven dynamic walker composes.
- consumer TFM: the `net10.0` consumer binds `lib/net5.0`. The Span/Sequence/`IBufferWriter` fast-path overloads (`ParseFrom(ReadOnlySpan)`, `MergeFrom(ReadOnlySequence)`, `WriteTo(IBufferWriter)`, `WriteLengthPrefixedTo`) exist in `net5.0` and are ABSENT from `netstandard2.0`; an `api resolve` that previews `netstandard2.0` is not proof of the bound surface.

[BUFFER_FAST_PATH]:
- read entry: `MessageParser<T>.ParseFrom(ReadOnlySpan<byte>)` and `ParseFrom(ReadOnlySequence<byte>)` parse pooled buffers without a stream allocation; `MergeFrom(ReadOnlySpan/ReadOnlySequence)` extends a live message in place.
- write entry: `MessageExtensions.WriteTo(IBufferWriter<byte>)`, `WriteTo(Span<byte>)`, and `WriteLengthPrefixedTo(IBufferWriter<byte>)` emit into pooled or pre-sized buffers; pre-size a `Span` write through `IMessage.CalculateSize()` / the `CodedOutputStream.Compute*Size` family.
- no-copy aliasing: `UnsafeByteOperations.UnsafeWrap(ReadOnlyMemory<byte>)` adopts caller-owned memory into a `ByteString`; the caller owns buffer lifetime past the message's read window.
- context boundary: `ParseContext`/`WriteContext` `Initialize` factories are package-internal; generated code drives them through `IBufferMessage.InternalMergeFrom(ref ParseContext)` / `InternalWriteTo(ref WriteContext)`, and Compute reaches the fast path only through the public parser, message-extension, and field-codec surfaces — never by constructing a `ref` context.
- field codec: `FieldCodec<T>.Read(ref ParseContext)`, `WriteTagAndValue(ref WriteContext, T)`, and `CalculateSizeWithTag(T)` are the per-field ref-context operations behind generated accessors.

[REFLECTION_CONTRACTS]:
- namespace: `Google.Protobuf.Reflection`; the file/message/field/enum/method/service descriptor graph drives contract inspection.
- generated metadata: `GeneratedClrTypeInfo` binds generated CLR types, parsers, and oneof/property names to descriptors; `OriginalNameAttribute` preserves protocol names where C# names diverge.
- contract evolution: `FileDescriptor.BuildFromByteStrings` reconstructs a serialized `FileDescriptorSet`, and the `MessageDescriptor.Fields` / `FieldDescriptor.{FieldNumber,FieldType,IsRepeated,IsMap,IsPacked}` / `ServiceDescriptor.Methods` walk feeds the `Runtime/wire#CONTRACT_EVOLUTION` descriptor-diff `XxHash128` projection-checksum.

[INTEGRATION_STACK]:
- artifact-sync frame: `IMessage.CalculateSize` sizes the `Runtime/transport#ARTIFACT_FRAMES` 64 KiB `FrameEdge`; `MessageExtensions.WriteLengthPrefixedTo(IBufferWriter<byte>)` writes the frame body into a `Microsoft.IO.RecyclableMemoryStream` `IBufferWriter<byte>` sink (`api-recyclable-stream`); the per-frame integrity is `Crc32.HashToUInt32(ReadOnlySpan<byte>)` over the contiguous `ByteString`/segment span and the whole-artifact identity is `XxHash128` over the pooled `GetReadOnlySequence()` view (`HashToUInt128(ReadOnlySpan<byte>, long seed)` for a single segment, else incremental `Append`+`GetCurrentHashAsUInt128`) — both ride `System.IO.Hashing` (`api-hashing`) over the same pooled bytes; protobuf owns the message body, never the frame envelope or the hash.
- decode mirror: a received frame's `RecyclableMemoryStream.GetReadOnlySequence()` feeds `MessageParser<T>.ParseFrom(ReadOnlySequence<byte>)` so a fragmented pooled payload decodes without a contiguous copy; `IBufferMessage.InternalMergeFrom` is the generated zero-alloc leg.
- transport: the five `Runtime/wire#PROTO_VOCABULARY` services are `Google.Protobuf` message contracts carried by `Grpc.Net.Client` (`api-grpc-client`); the typed `FaultDetail`/`Status` rail and the `Any`-wrapped in-band conflict payload decode through `Any.TryUnpack<T>`/`Unpack(TypeRegistry)`.
- partial update: a `FieldMask` `Paths` set drives the `Runtime/transport` partial-update APPLY leg — `FieldMask.IsValid(descriptor,...)` gates the paths against the target `MessageDescriptor` and `Normalize` dedups before the field-level merge over the descriptor `Accessor`.
- clock seam: `Timestamp`/`Duration` cross to NodaTime through `NodaTime.Serialization.Protobuf` (`api-nodatime-protobuf`) — `Instant.ToTimestamp()` / `Duration.ToProtobufDuration()` outward and `Timestamp.ToInstant()` / `Duration.ToNodaDuration()` inward (the `Runtime/wire` `FrameEdge.Transaction` realized form writes `hlc.ToTimestamp()` onto the `TransactionRequest` HLC field); calendar-bearing fields cross as `Google.Type` commons through `ToDate`/`ToTimeOfDay`/`ToProtobufDayOfWeek` ↔ `ToLocalDate`/`ToLocalTime`/`ToIsoDayOfWeek`. A hand-rolled `DateTime`↔`Timestamp` bridge beside that adapter is the rejected form.

[LOCAL_ADMISSION]:
- Remote Compute contracts enter source through generated `IMessage<T>` surfaces; binary payloads use the protobuf codec stack and never handwritten byte DTOs.
- JSON projection is an edge format (diagnostics, debug, browser-adjacent), gated through one reused `JsonFormatter`/`JsonParser` with an explicit `Settings`/`TypeRegistry`; it cannot replace binary remote-contract ownership.
- Reflection descriptors are diagnostics and contract-evolution material before they become runtime dispatch material; the descriptor-diff law consumes them read-only.

[RAIL_LAW]:
- Package: `Google.Protobuf`
- Owns: generated wire contracts, the codec stack, the buffer fast path, `FieldCodec<T>`, the reflection descriptor graph, the well-known type family, and JSON edge projection
- Accept: generated message contracts over the Span/Sequence/`IBufferWriter` fast path stacked onto the `RecyclableMemoryStream` writer face and the `XxHash128`/`Crc32` content-identity law
- Reject: handwritten binary DTOs; per-call `JsonFormatter`/`CodedInputStream` construction; a managed `ParseContext`/`WriteContext` driver; a second clock or hash bridge beside the NodaTime/`System.IO.Hashing` owners; trusting a `netstandard2.0` `api resolve` preview as the bound surface

# [RASM_API_PROTOBUF]

`Google.Protobuf` owns generated wire contracts for the Compute remote lane and every projection off them: binary through the coded streams, the span and sequence buffer fast path, the extension and unknown-field algebra, the reflection descriptor graph, and JSON at the diagnostic edge. Generated messages implement `IBufferMessage`, so one message body crosses a pooled transport with no intermediate array. Frame envelopes, content digests, and transport policy stop outside this boundary.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `Google.Protobuf`
- package: `Google.Protobuf` (BSD-3-Clause)
- assembly: `Google.Protobuf`, binding the asset carrying the `ReadOnlySpan<byte>`, `ReadOnlySequence<byte>`, and `IBufferWriter<byte>` overloads
- namespace: `Google.Protobuf`, `Google.Protobuf.Collections`, `Google.Protobuf.Reflection`, `Google.Protobuf.WellKnownTypes`
- depends: `Grpc.Tools` owns the `.proto` compile; this package ships the managed runtime alone
- rail: remote-contracts

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message, codec, and buffer-cursor contracts

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :------------------------------- | :------------ | :------------------------------------- |
|  [01]   | `IMessage`                       | interface     | descriptor with binary read and write  |
|  [02]   | `IMessage<T>`                    | interface     | typed merge, equality, and cloning     |
|  [03]   | `IBufferMessage`                 | interface     | generated `ref`-cursor entry           |
|  [04]   | `IDeepCloneable<T>`              | interface     | structural deep copy                   |
|  [05]   | `IExtendableMessage<T>`          | interface     | proto2 extension field access          |
|  [06]   | `ICustomDiagnosticMessage`       | interface     | message-owned diagnostic rendering     |
|  [07]   | `MessageParser`                  | class         | untyped decode off a descriptor        |
|  [08]   | `MessageParser<T>`               | class         | typed decode with parse policy         |
|  [09]   | `MessageExtensions`              | static class  | serialize, merge, and buffer entries   |
|  [10]   | `CodedInputStream`               | sealed class  | bounded binary decoding                |
|  [11]   | `CodedOutputStream`              | sealed class  | binary encoding with size costing      |
|  [12]   | `ParseContext`                   | ref struct    | generated parse cursor over a span     |
|  [13]   | `WriteContext`                   | ref struct    | generated write cursor over a span     |
|  [14]   | `FieldCodec`                     | static class  | per-field codec factories              |
|  [15]   | `FieldCodec<T>`                  | sealed class  | one field's read, write, and size unit |
|  [16]   | `WireFormat`                     | static class  | tag construction and projection        |
|  [17]   | `WireFormat.WireType`            | enum          | closed wire-tag vocabulary             |
|  [18]   | `ByteString`                     | sealed class  | immutable wire-byte ownership          |
|  [19]   | `UnsafeByteOperations`           | static class  | caller-memory adoption without a copy  |
|  [20]   | `JsonFormatter`                  | sealed class  | message projection to JSON             |
|  [21]   | `JsonParser`                     | sealed class  | message reconstruction from JSON       |
|  [22]   | `InvalidProtocolBufferException` | sealed class  | binary parse failure                   |
|  [23]   | `InvalidJsonException`           | sealed class  | JSON parse failure                     |

[WIRE_TYPE]: `Varint` `Fixed64` `LengthDelimited` `StartGroup` `EndGroup` `Fixed32`

[PUBLIC_TYPE_SCOPE]: collection, extension, and unknown-field contracts

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY] | [CAPABILITY]                            |
| :-----: | :----------------------------------- | :------------ | :-------------------------------------- |
|  [01]   | `RepeatedField<T>`                   | sealed class  | codec-driven repeated field             |
|  [02]   | `MapField<TKey, TValue>`             | sealed class  | codec-driven map field                  |
|  [03]   | `MapField<TKey, TValue>.Codec`       | sealed class  | key, value, and map-tag codec triple    |
|  [04]   | `RepeatedFieldExtensions`            | static class  | span-sourced bulk append                |
|  [05]   | `UnsafeCollectionOperations`         | static class  | zero-copy backing-array views           |
|  [06]   | `UnknownFieldSet`                    | sealed class  | unknown-field preservation store        |
|  [07]   | `UnknownField`                       | sealed class  | one preserved field's raw values        |
|  [08]   | `ExtensionRegistry`                  | sealed class  | parse-time extension resolution         |
|  [09]   | `ExtensionSet`                       | static class  | `ref`-threaded extension-set operations |
|  [10]   | `ExtensionSet<TTarget>`              | sealed class  | one message's extension storage         |
|  [11]   | `Extension`                          | class         | field-number identity                   |
|  [12]   | `Extension<TTarget, TValue>`         | class         | typed singular extension handle         |
|  [13]   | `RepeatedExtension<TTarget, TValue>` | class         | typed repeated extension handle         |

[PUBLIC_TYPE_SCOPE]: reflection descriptor graph

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY]  | [CAPABILITY]                               |
| :-----: | :---------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `DescriptorBase`                    | abstract class | index, full name, file, and declaration    |
|  [02]   | `FileDescriptor`                    | sealed class   | file-level descriptor graph root           |
|  [03]   | `MessageDescriptor`                 | sealed class   | message-level descriptor graph             |
|  [04]   | `MessageDescriptor.FieldCollection` | sealed class   | ordered and keyed field views              |
|  [05]   | `FieldDescriptor`                   | sealed class   | field-level descriptor graph               |
|  [06]   | `EnumDescriptor`                    | sealed class   | enum-value descriptor graph                |
|  [07]   | `EnumValueDescriptor`               | sealed class   | one enum value's name and number           |
|  [08]   | `OneofDescriptor`                   | sealed class   | oneof membership and case access           |
|  [09]   | `ServiceDescriptor`                 | sealed class   | RPC service descriptor graph               |
|  [10]   | `MethodDescriptor`                  | sealed class   | RPC method descriptor graph                |
|  [11]   | `ExtensionCollection`               | sealed class   | declaration- and number-ordered extensions |
|  [12]   | `DescriptorDeclaration`             | sealed class   | source span and comments for a descriptor  |
|  [13]   | `IFieldAccessor`                    | interface      | reflective get, set, clear, and presence   |
|  [14]   | `OneofAccessor`                     | sealed class   | active oneof case resolution               |
|  [15]   | `FieldType`                         | enum           | closed field-type vocabulary               |
|  [16]   | `TypeRegistry`                      | sealed class   | full-name to message resolution            |
|  [17]   | `GeneratedClrTypeInfo`              | sealed class   | generated CLR metadata binding             |
|  [18]   | `OriginalNameAttribute`             | attribute      | protocol-name preservation                 |
|  [19]   | `DescriptorValidationException`     | sealed class   | descriptor build failure                   |
|  [20]   | `FileDescriptorProto`               | sealed class   | file descriptor as a wire message          |
|  [21]   | `DescriptorProto`                   | sealed class   | message descriptor as a wire message       |
|  [22]   | `FileDescriptorSet`                 | sealed class   | descriptor-set payload carrier             |

[PUBLIC_TYPE_SCOPE]: `Google.Protobuf.WellKnownTypes` generated messages

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                           |
| :-----: | :----------------------- | :------------ | :------------------------------------- |
|  [01]   | `Any`                    | sealed class  | type-URL-keyed payload envelope        |
|  [02]   | `Timestamp`              | sealed class  | instant with comparison and arithmetic |
|  [03]   | `Duration`               | sealed class  | span with negation and arithmetic      |
|  [04]   | `FieldMask`              | sealed class  | path selection, set algebra, and merge |
|  [05]   | `FieldMask.MergeOptions` | sealed class  | replace-instead-of-merge policy        |
|  [06]   | `Struct`                 | sealed class  | string-keyed dynamic map               |
|  [07]   | `Value`                  | sealed class  | dynamic value over a `oneof` kind      |
|  [08]   | `Value.KindOneofCase`    | enum          | active dynamic-value discriminant      |
|  [09]   | `ListValue`              | sealed class  | ordered dynamic values                 |
|  [10]   | `NullValue`              | enum          | single-cased null marker               |
|  [11]   | `Empty`                  | sealed class  | no-payload RPC marker                  |
|  [12]   | `TimeExtensions`         | static class  | BCL temporal projection onto the wire  |

Each wrapper message carries one presence-bearing field over its named CLR primitive; `BytesValue` carries `ByteString`.

[WRAPPER]: `DoubleValue` `FloatValue` `Int32Value` `UInt32Value` `Int64Value` `UInt64Value` `BoolValue` `StringValue` `BytesValue`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: typed decode through `MessageParser<T>`

`MessageParser<T>` is the static `Parser` every generated message exposes; each parse form returns a fresh `T` and each `With*` form returns a reconfigured parser.

| [INDEX] | [SURFACE]                                                      | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `ParseFrom(byte[]) -> T`                                       | instance | array decode                     |
|  [02]   | `ParseFrom(byte[], int, int) -> T`                             | instance | array-slice decode               |
|  [03]   | `ParseFrom(ByteString) -> T`                                   | instance | immutable-bytes decode           |
|  [04]   | `ParseFrom(Stream) -> T`                                       | instance | stream decode                    |
|  [05]   | `ParseFrom(ReadOnlySpan<byte>) -> T`                           | instance | pooled decode, no stream         |
|  [06]   | `ParseFrom(ReadOnlySequence<byte>) -> T`                       | instance | fragmented decode, no contiguity |
|  [07]   | `ParseFrom(CodedInputStream) -> T`                             | instance | decode under caller-set limits   |
|  [08]   | `ParseDelimitedFrom(Stream) -> T`                              | instance | length-prefixed decode           |
|  [09]   | `ParseJson(string) -> T`                                       | instance | JSON decode                      |
|  [10]   | `WithDiscardUnknownFields(bool) -> MessageParser<T>`           | instance | unknown-field drop policy        |
|  [11]   | `WithExtensionRegistry(ExtensionRegistry) -> MessageParser<T>` | instance | extension-aware parser           |

- `MessageParser<T>.ParseJson`: routes through the default `JsonParser`, so `WithDiscardUnknownFields` never reaches it — an unknown-field-tolerant JSON decode binds `JsonParser.Settings.WithIgnoreUnknownFields`.

[ENTRYPOINT_SCOPE]: serialize and merge through `MessageExtensions`

Every member binds `this IMessage`; merge forms mutate the receiver in place, and write forms emit into a stream, a pooled writer, or a pre-sized span.

| [INDEX] | [SURFACE]                                    | [SHAPE] | [CAPABILITY]                        |
| :-----: | :------------------------------------------- | :------ | :---------------------------------- |
|  [01]   | `MergeFrom(byte[])`                          | static  | array merge                         |
|  [02]   | `MergeFrom(byte[], int, int)`                | static  | array-slice merge                   |
|  [03]   | `MergeFrom(ByteString)`                      | static  | immutable-bytes merge               |
|  [04]   | `MergeFrom(Stream)`                          | static  | stream merge                        |
|  [05]   | `MergeFrom(ReadOnlySpan<byte>)`              | static  | pooled merge, no stream             |
|  [06]   | `MergeFrom(ReadOnlySequence<byte>)`          | static  | fragmented merge, no contiguity     |
|  [07]   | `MergeDelimitedFrom(Stream)`                 | static  | length-prefixed merge               |
|  [08]   | `ToByteArray() -> byte[]`                    | static  | managed byte copy                   |
|  [09]   | `ToByteString() -> ByteString`               | static  | immutable byte copy                 |
|  [10]   | `WriteTo(Stream)`                            | static  | stream write                        |
|  [11]   | `WriteTo(IBufferWriter<byte>)`               | static  | pooled write, no intermediate array |
|  [12]   | `WriteTo(Span<byte>)`                        | static  | write into a pre-sized span         |
|  [13]   | `WriteDelimitedTo(Stream)`                   | static  | length-prefixed stream write        |
|  [14]   | `WriteLengthPrefixedTo(IBufferWriter<byte>)` | static  | varint-prefixed pooled write        |
|  [15]   | `IsInitialized() -> bool`                    | static  | required-field completeness verdict |

- `MessageExtensions.WriteTo(Span<byte>)`: requires a destination sized exactly to `IMessage.CalculateSize()`; any other length throws.

[ENTRYPOINT_SCOPE]: generated message contract members

| [INDEX] | [SURFACE]                                                 | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :-------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `IMessage.Descriptor -> MessageDescriptor`                | property | reflection entry off the instance |
|  [02]   | `IMessage.CalculateSize() -> int`                         | instance | exact serialized size             |
|  [03]   | `IMessage.MergeFrom(CodedInputStream)`                    | instance | bounded stream merge              |
|  [04]   | `IMessage.WriteTo(CodedOutputStream)`                     | instance | bounded stream write              |
|  [05]   | `IMessage<T>.MergeFrom(T)`                                | instance | typed message-into-message merge  |
|  [06]   | `IDeepCloneable<T>.Clone() -> T`                          | instance | structural deep copy              |
|  [07]   | `IBufferMessage.InternalMergeFrom(ref ParseContext)`      | instance | generated zero-alloc read leg     |
|  [08]   | `IBufferMessage.InternalWriteTo(ref WriteContext)`        | instance | generated zero-alloc write leg    |
|  [09]   | `ICustomDiagnosticMessage.ToDiagnosticString() -> string` | instance | message-owned diagnostic form     |

[ENTRYPOINT_SCOPE]: `ByteString` bytes ownership

Copy factories own their bytes; `Span` and `Memory` expose zero-copy views over them.

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                  |
| :-----: | :-------------------------------------------------------------------------- | :------- | :---------------------------- |
|  [01]   | `ByteString.Empty -> ByteString`                                            | property | shared zero-length instance   |
|  [02]   | `ByteString.Length -> int`                                                  | property | byte count                    |
|  [03]   | `ByteString.IsEmpty -> bool`                                                | property | emptiness verdict             |
|  [04]   | `ByteString.Span -> ReadOnlySpan<byte>`                                     | property | zero-copy span view           |
|  [05]   | `ByteString.Memory -> ReadOnlyMemory<byte>`                                 | property | zero-copy memory view         |
|  [06]   | `ByteString[int] -> byte`                                                   | property | single-byte index             |
|  [07]   | `ByteString.CopyFrom(ReadOnlySpan<byte>)`                                   | factory  | span copy                     |
|  [08]   | `ByteString.CopyFrom(byte[])`                                               | factory  | array copy                    |
|  [09]   | `ByteString.CopyFrom(byte[], int, int)`                                     | factory  | array-slice copy              |
|  [10]   | `ByteString.CopyFrom(string, Encoding)`                                     | factory  | encoded-text copy             |
|  [11]   | `ByteString.CopyFromUtf8(string)`                                           | factory  | UTF-8 text copy               |
|  [12]   | `ByteString.FromBase64(string)`                                             | factory  | base64 decode                 |
|  [13]   | `ByteString.FromStream(Stream)`                                             | factory  | synchronous stream drain      |
|  [14]   | `ByteString.FromStreamAsync(Stream, CancellationToken) -> Task<ByteString>` | factory  | asynchronous stream drain     |
|  [15]   | `ByteString.ToByteArray() -> byte[]`                                        | instance | managed array copy            |
|  [16]   | `ByteString.ToBase64() -> string`                                           | instance | base64 encode                 |
|  [17]   | `ByteString.ToStringUtf8() -> string`                                       | instance | UTF-8 decode                  |
|  [18]   | `ByteString.ToString(Encoding) -> string`                                   | instance | encoded decode                |
|  [19]   | `ByteString.CopyTo(byte[], int)`                                            | instance | copy into a caller array      |
|  [20]   | `ByteString.WriteTo(Stream)`                                                | instance | copy into a stream            |
|  [21]   | `ByteString.CreateCodedInput() -> CodedInputStream`                         | instance | bounded reader over the bytes |
|  [22]   | `ByteString == ByteString -> bool`                                          | operator | structural byte equality      |
|  [23]   | `UnsafeByteOperations.UnsafeWrap(ReadOnlyMemory<byte>)`                     | static   | adopt caller memory, no copy  |

- `UnsafeByteOperations.UnsafeWrap`: aliases the caller's memory, so that memory stays unchanged for the whole `ByteString` lifetime.

[ENTRYPOINT_SCOPE]: repeated and map collections with zero-copy views

Wire operations take a codec and a `ref` cursor; `UnsafeCollectionOperations` hands back the live backing array.

| [INDEX] | [SURFACE]                                                                      | [SHAPE]  | [CAPABILITY]                |
| :-----: | :----------------------------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `RepeatedField<T>.AddEntriesFrom(ref ParseContext, FieldCodec<T>)`             | instance | bulk decode into the field  |
|  [02]   | `RepeatedField<T>.AddEntriesFrom(CodedInputStream, FieldCodec<T>)`             | instance | bulk stream decode          |
|  [03]   | `RepeatedField<T>.WriteTo(ref WriteContext, FieldCodec<T>)`                    | instance | bulk encode from the field  |
|  [04]   | `RepeatedField<T>.WriteTo(CodedOutputStream, FieldCodec<T>)`                   | instance | bulk stream encode          |
|  [05]   | `RepeatedField<T>.CalculateSize(FieldCodec<T>) -> int`                         | instance | exact repeated-field size   |
|  [06]   | `RepeatedField<T>.Capacity -> int`                                             | property | pre-size the backing array  |
|  [07]   | `RepeatedField<T>.AddRange(IEnumerable<T>)`                                    | instance | bulk append                 |
|  [08]   | `RepeatedField<T>.Clone() -> RepeatedField<T>`                                 | instance | structural deep copy        |
|  [09]   | `RepeatedFieldExtensions.AddRange(ReadOnlySpan<T>)`                            | static   | span-sourced bulk append    |
|  [10]   | `UnsafeCollectionOperations.AsSpan(RepeatedField<T>) -> Span<T>`               | static   | live backing-array window   |
|  [11]   | `UnsafeCollectionOperations.SetCount(RepeatedField<T>, int)`                   | static   | resize without per-item add |
|  [12]   | `MapField<TKey, TValue>.AddEntriesFrom(ref ParseContext, Codec)`               | instance | bulk decode into the map    |
|  [13]   | `MapField<TKey, TValue>.WriteTo(ref WriteContext, Codec)`                      | instance | bulk encode from the map    |
|  [14]   | `MapField<TKey, TValue>.CalculateSize(Codec) -> int`                           | instance | exact map-field size        |
|  [15]   | `MapField<TKey, TValue>.MergeFrom(IDictionary<TKey, TValue>)`                  | instance | dictionary merge            |
|  [16]   | `MapField<TKey, TValue>.Clone() -> MapField<TKey, TValue>`                     | instance | structural deep copy        |
|  [17]   | `new MapField<TKey, TValue>.Codec(FieldCodec<TKey>, FieldCodec<TValue>, uint)` | ctor     | key, value, and map tag     |

- `UnsafeCollectionOperations.AsSpan`: writes through to the field's storage, and a `null` element written into a message-typed span corrupts the later encode; `SetCount` grows with default-initialized slots.

[ENTRYPOINT_SCOPE]: extension handles and registry admission

An extension is one handle over a field number and a codec; `ExtensionRegistry` admits handles to a parse and binds to a parser or a `CodedInputStream`.

| [INDEX] | [SURFACE]                                                         | [SHAPE]  | [CAPABILITY]                   |
| :-----: | :---------------------------------------------------------------- | :------- | :----------------------------- |
|  [01]   | `new Extension<TTarget, TValue>(int, FieldCodec<TValue>)`         | ctor     | singular extension handle      |
|  [02]   | `new RepeatedExtension<TTarget, TValue>(int, FieldCodec<TValue>)` | ctor     | repeated extension handle      |
|  [03]   | `Extension.FieldNumber -> int`                                    | property | extension field identity       |
|  [04]   | `ExtensionRegistry.Add(Extension)`                                | instance | admit one extension to parsing |
|  [05]   | `ExtensionRegistry.AddRange(IEnumerable<Extension>)`              | instance | admit an extension set         |
|  [06]   | `ExtensionRegistry.Contains(Extension) -> bool`                   | instance | admission verdict              |
|  [07]   | `ExtensionRegistry.Clone() -> ExtensionRegistry`                  | instance | fork the registry              |

[ENTRYPOINT_SCOPE]: extension access through `IExtendableMessage<T>`

Every member takes the handle alone; a repeated read returns the live `RepeatedField<TValue>`.

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]          |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :-------------------- |
|  [01]   | `GetExtension(Extension<T, TValue>) -> TValue`                                    | instance | singular read         |
|  [02]   | `GetExtension(RepeatedExtension<T, TValue>) -> RepeatedField<TValue>`             | instance | repeated read         |
|  [03]   | `GetOrInitializeExtension(RepeatedExtension<T, TValue>) -> RepeatedField<TValue>` | instance | repeated mint-on-read |
|  [04]   | `SetExtension(Extension<T, TValue>, TValue)`                                      | instance | singular write        |
|  [05]   | `HasExtension(Extension<T, TValue>) -> bool`                                      | instance | presence verdict      |
|  [06]   | `ClearExtension(Extension<T, TValue>)`                                            | instance | singular clear        |
|  [07]   | `ClearExtension(RepeatedExtension<T, TValue>)`                                    | instance | repeated clear        |

[ENTRYPOINT_SCOPE]: extension storage through `ExtensionSet`

Every operation is static over a `ref ExtensionSet<T>`, so an unextended message stores no set at all.

| [INDEX] | [SURFACE]                                                                                     | [SHAPE] | [CAPABILITY]               |
| :-----: | :-------------------------------------------------------------------------------------------- | :------ | :------------------------- |
|  [01]   | `Get(ref ExtensionSet<T>, Extension<T, TValue>) -> TValue`                                    | static  | singular read              |
|  [02]   | `GetOrInitialize(ref ExtensionSet<T>, RepeatedExtension<T, TValue>) -> RepeatedField<TValue>` | static  | repeated mint-on-read      |
|  [03]   | `Set(ref ExtensionSet<T>, Extension<T, TValue>, TValue)`                                      | static  | singular write             |
|  [04]   | `Has(ref ExtensionSet<T>, Extension<T, TValue>) -> bool`                                      | static  | presence verdict           |
|  [05]   | `Clear(ref ExtensionSet<T>, Extension<T, TValue>)`                                            | static  | singular clear             |
|  [06]   | `TryMergeFieldFrom(ref ExtensionSet<T>, ref ParseContext) -> bool`                            | static  | decode one extension field |
|  [07]   | `MergeFrom(ref ExtensionSet<T>, ExtensionSet<T>)`                                             | static  | fold one set into another  |
|  [08]   | `Clone(ExtensionSet<T>) -> ExtensionSet<T>`                                                   | static  | fork the set               |

[ENTRYPOINT_SCOPE]: unknown-field preservation

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                |
| :-----: | :--------------------------------------------------------- | :------- | :-------------------------- |
|  [01]   | `UnknownFieldSet.HasField(int) -> bool`                    | instance | preserved-field presence    |
|  [02]   | `UnknownFieldSet.MergeFieldFrom(ref ParseContext) -> bool` | instance | capture one unclaimed field |
|  [03]   | `UnknownFieldSet.AddOrReplaceField(int, UnknownField)`     | instance | seat a field by number      |
|  [04]   | `UnknownFieldSet.WriteTo(ref WriteContext)`                | instance | re-emit preserved fields    |
|  [05]   | `UnknownFieldSet.CalculateSize() -> int`                   | instance | preserved-field size        |

Each `UnknownField` staging member takes one value of its named wire kind.

[STAGE]: `AddVarint` `AddFixed32` `AddFixed64` `AddLengthDelimited`
- `MessageParser<T>.WithDiscardUnknownFields(true)`: drops the `UnknownFieldSet` capture, so a re-emitted message loses every field the current descriptor does not claim.

[ENTRYPOINT_SCOPE]: `FieldCodec` construction and per-field wire operations

Each scalar factory has a tag-only form and a default-value form, and `uint tag` comes from `WireFormat.MakeTag`. Encoding class selects the factory:

[VARINT]: `ForInt32` `ForInt64` `ForUInt32` `ForUInt64` `ForBool` `ForEnum<T>`
[ZIGZAG]: `ForSInt32` `ForSInt64`
[FIXED32]: `ForFixed32` `ForSFixed32` `ForFloat`
[FIXED64]: `ForFixed64` `ForSFixed64` `ForDouble`
[LENGTH_DELIMITED]: `ForString` `ForBytes` `ForMessage<T>` `ForClassWrapper<T>` `ForStructWrapper<T>`

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                       |
| :-----: | :---------------------------------------------------------- | :------- | :--------------------------------- |
|  [01]   | `FieldCodec.ForEnum<T>(uint, Func<T, int>, Func<int, T>)`   | static   | enum codec over both maps          |
|  [02]   | `FieldCodec.ForMessage<T>(uint, MessageParser<T>)`          | static   | nested-message codec               |
|  [03]   | `FieldCodec.ForGroup<T>(uint, uint, MessageParser<T>)`      | static   | start- and end-tagged group        |
|  [04]   | `FieldCodec.ForClassWrapper<T>(uint)`                       | static   | wrapper codec, reference value     |
|  [05]   | `FieldCodec.ForStructWrapper<T>(uint) -> FieldCodec<T?>`    | static   | wrapper codec, nullable value      |
|  [06]   | `FieldCodec<T>.Read(ref ParseContext) -> T`                 | instance | cursor read                        |
|  [07]   | `FieldCodec<T>.Read(CodedInputStream) -> T`                 | instance | stream read                        |
|  [08]   | `FieldCodec<T>.WriteTagAndValue(ref WriteContext, T)`       | instance | cursor tag-and-value write         |
|  [09]   | `FieldCodec<T>.WriteTagAndValue(CodedOutputStream, T)`      | instance | stream tag-and-value write         |
|  [10]   | `FieldCodec<T>.CalculateSizeWithTag(T) -> int`              | instance | size, default-valued fields elided |
|  [11]   | `FieldCodec<T>.CalculateUnconditionalSizeWithTag(T) -> int` | instance | size, defaults always counted      |
|  [12]   | `FieldCodec<T>.Tag -> uint`                                 | property | this field's start tag             |
|  [13]   | `FieldCodec<T>.EndTag -> uint`                              | property | group end tag                      |
|  [14]   | `FieldCodec<T>.DefaultValue -> T`                           | property | value the codec elides             |
|  [15]   | `FieldCodec<T>.FixedSize -> int`                            | property | non-zero on fixed-width codecs     |
|  [16]   | `WireFormat.MakeTag(int, WireFormat.WireType) -> uint`      | static   | build a field tag                  |
|  [17]   | `WireFormat.GetTagFieldNumber(uint) -> int`                 | static   | project a tag's field number       |
|  [18]   | `WireFormat.GetTagWireType(uint) -> WireFormat.WireType`    | static   | project a tag's wire type          |

- `FieldCodec<T>.CalculateSizeWithTag`: includes the tag and returns zero for the codec's default value; the unconditional form counts a default-valued field a presence-bearing writer must still emit.

[ENTRYPOINT_SCOPE]: typed envelopes and field masks

| [INDEX] | [SURFACE]                                                             | [SHAPE]  | [CAPABILITY]                        |
| :-----: | :-------------------------------------------------------------------- | :------- | :---------------------------------- |
|  [01]   | `Any.Pack(IMessage) -> Any`                                           | static   | wrap under the default type prefix  |
|  [02]   | `Any.Pack(IMessage, string) -> Any`                                   | static   | wrap under a caller type prefix     |
|  [03]   | `Any.Unpack<T>() -> T`                                                | instance | typed unwrap                        |
|  [04]   | `Any.TryUnpack<T>(out T) -> bool`                                     | instance | typed unwrap with a verdict         |
|  [05]   | `Any.Unpack(TypeRegistry) -> IMessage`                                | instance | registry-resolved unwrap            |
|  [06]   | `Any.Is(MessageDescriptor) -> bool`                                   | instance | payload-type verdict                |
|  [07]   | `Any.GetTypeName(string) -> string`                                   | static   | full name out of a type URL         |
|  [08]   | `FieldMask.FromString(string) -> FieldMask`                           | static   | comma-path parse                    |
|  [09]   | `FieldMask.FromString<T>(string) -> FieldMask`                        | static   | parse validated against `T`         |
|  [10]   | `FieldMask.FromStringEnumerable<T>(IEnumerable<string>) -> FieldMask` | static   | validated multi-path parse          |
|  [11]   | `FieldMask.FromFieldNumbers<T>(IEnumerable<int>) -> FieldMask`        | static   | mask from wire numbers              |
|  [12]   | `FieldMask.IsValid(MessageDescriptor, FieldMask) -> bool`             | static   | descriptor-checked mask verdict     |
|  [13]   | `FieldMask.IsValid<T>(FieldMask) -> bool`                             | static   | type-checked mask verdict           |
|  [14]   | `FieldMask.IsPathValid(string) -> bool`                               | static   | path-syntax verdict                 |
|  [15]   | `FieldMask.Normalize() -> FieldMask`                                  | instance | sort, dedupe, prune subpaths        |
|  [16]   | `FieldMask.Union(params FieldMask[]) -> FieldMask`                    | instance | path-set union                      |
|  [17]   | `FieldMask.Intersection(FieldMask) -> FieldMask`                      | instance | path-set intersection               |
|  [18]   | `FieldMask.Merge(IMessage, IMessage)`                                 | instance | copy masked paths, source to target |
|  [19]   | `FieldMask.Merge(IMessage, IMessage, MergeOptions)`                   | instance | masked copy under a replace policy  |
|  [20]   | `FieldMask.Paths -> RepeatedField<string>`                            | property | the selected path set               |

- `Any.Unpack<T>`: throws on a payload-type mismatch; `TryUnpack<T>` returns the verdict instead.
- `FieldMask.MergeOptions`: `ReplaceMessageFields`, `ReplaceRepeatedFields`, and `ReplacePrimitiveFields` each discard existing target content instead of merging into it.

[ENTRYPOINT_SCOPE]: temporal and dynamic well-known values

`Timestamp` and `Duration` carry an operator algebra, so a wire instant subtracts, offsets, and compares without leaving the message type.

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                      |
| :-----: | :---------------------------------------------------------- | :------- | :-------------------------------- |
|  [01]   | `Timestamp.FromDateTime(DateTime) -> Timestamp`             | static   | UTC instant in                    |
|  [02]   | `Timestamp.FromDateTimeOffset(DateTimeOffset) -> Timestamp` | static   | offset instant in                 |
|  [03]   | `Timestamp.ToDateTime() -> DateTime`                        | instance | UTC instant out                   |
|  [04]   | `Timestamp.ToDateTimeOffset() -> DateTimeOffset`            | instance | offset instant out                |
|  [05]   | `Timestamp - Timestamp -> Duration`                         | operator | elapsed span between instants     |
|  [06]   | `Timestamp + Duration -> Timestamp`                         | operator | instant offset forward            |
|  [07]   | `Timestamp - Duration -> Timestamp`                         | operator | instant offset backward           |
|  [08]   | `Timestamp.CompareTo(Timestamp) -> int`                     | instance | sort key over instants            |
|  [09]   | `Duration.FromTimeSpan(TimeSpan) -> Duration`               | static   | span in                           |
|  [10]   | `Duration.ToTimeSpan() -> TimeSpan`                         | instance | span out                          |
|  [11]   | `-Duration -> Duration`                                     | operator | sign inversion                    |
|  [12]   | `Duration + Duration -> Duration`                           | operator | span accumulation                 |
|  [13]   | `Duration.CompareTo(Duration) -> int`                       | instance | sort key over spans               |
|  [14]   | `Duration.MaxSeconds` / `MinSeconds`                        | static   | representable second bounds       |
|  [15]   | `TimeExtensions.ToTimestamp(DateTime) -> Timestamp`         | static   | extension-form instant projection |
|  [16]   | `TimeExtensions.ToTimestamp(DateTimeOffset) -> Timestamp`   | static   | extension-form offset projection  |
|  [17]   | `TimeExtensions.ToDuration(TimeSpan) -> Duration`           | static   | extension-form span projection    |
|  [18]   | `Value.KindCase -> Value.KindOneofCase`                     | property | active case discriminant          |
|  [19]   | `Value.ClearKind()`                                         | instance | clear whichever case is set       |
|  [20]   | `Struct.Fields -> MapField<string, Value>`                  | property | string-keyed dynamic members      |
|  [21]   | `ListValue.Values -> RepeatedField<Value>`                  | property | ordered dynamic members           |

Each `Value` factory is static and returns a `Value` over one named case; `ForList` is variadic and `ForNull` takes none.

[VALUE_CASE]: `ForString` `ForNumber` `ForBool` `ForStruct` `ForList` `ForNull`
- `Timestamp.FromDateTime`: faults on any `DateTime` whose `Kind` is not `Utc`, rather than silently shifting it.
- `Timestamp`: declares `<`, `>`, `<=`, `>=`, `==`, and `!=` over instants; `Duration` orders through `CompareTo` alone.

[ENTRYPOINT_SCOPE]: reflection descriptor graph

`DescriptorBase` gives every descriptor `Index`, `Name`, `FullName`, `File`, and `Declaration`; a descriptor's own options resolve through its `GetOptions()`, and both `BuildFromByteStrings` forms return `IReadOnlyList<FileDescriptor>`.

| [INDEX] | [SURFACE]                                                                         | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :-------------------------------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `FileDescriptor.BuildFromByteStrings(IEnumerable<ByteString>)`                    | static   | rebuild files from payloads      |
|  [02]   | `FileDescriptor.BuildFromByteStrings(IEnumerable<ByteString>, ExtensionRegistry)` | static   | rebuild with extensions bound    |
|  [03]   | `FileDescriptor.SerializedData -> ByteString`                                     | property | the payload a rebuild consumes   |
|  [04]   | `FileDescriptor.ToProto() -> FileDescriptorProto`                                 | instance | file descriptor as a message     |
|  [05]   | `FileDescriptor.MessageTypes` / `EnumTypes` / `Services`                          | property | top-level declaration walks      |
|  [06]   | `FileDescriptor.Dependencies` / `PublicDependencies`                              | property | import graph walk                |
|  [07]   | `FileDescriptor.Extensions -> ExtensionCollection`                                | property | file-level extension walk        |
|  [08]   | `FileDescriptor.FindTypeByName<T>(string) -> T`                                   | instance | typed lookup by full name        |
|  [09]   | `MessageDescriptor.Fields -> FieldCollection`                                     | property | field view root                  |
|  [10]   | `MessageDescriptor.Fields.InDeclarationOrder()`                                   | instance | declaration-ordered fields       |
|  [11]   | `MessageDescriptor.Fields.InFieldNumberOrder()`                                   | instance | wire-number-ordered fields       |
|  [12]   | `MessageDescriptor.FindFieldByNumber(int) -> FieldDescriptor`                     | instance | wire-number lookup               |
|  [13]   | `MessageDescriptor.FindFieldByName(string) -> FieldDescriptor`                    | instance | proto-name lookup                |
|  [14]   | `MessageDescriptor.FindDescriptor<T>(string) -> T`                                | instance | typed nested lookup              |
|  [15]   | `MessageDescriptor.NestedTypes` / `EnumTypes` / `Oneofs`                          | property | nested declaration walks         |
|  [16]   | `MessageDescriptor.RealOneofCount -> int`                                         | property | oneofs excluding synthetic       |
|  [17]   | `MessageDescriptor.IsMapEntry -> bool`                                            | property | generated map-entry verdict      |
|  [18]   | `MessageDescriptor.ClrType -> Type`                                               | property | bound CLR type                   |
|  [19]   | `MessageDescriptor.Parser -> MessageParser`                                       | property | untyped parser for this message  |
|  [20]   | `MessageDescriptor.ToProto() -> DescriptorProto`                                  | instance | message descriptor as a message  |
|  [21]   | `FieldDescriptor.FieldNumber -> int`                                              | property | wire number                      |
|  [22]   | `FieldDescriptor.FieldType -> FieldType`                                          | property | wire type                        |
|  [23]   | `FieldDescriptor.JsonName` / `PropertyName`                                       | property | JSON and CLR spellings           |
|  [24]   | `FieldDescriptor.HasPresence -> bool`                                             | property | explicit-presence verdict        |
|  [25]   | `FieldDescriptor.IsRepeated` / `IsMap` / `IsPacked`                               | property | cardinality and packing          |
|  [26]   | `FieldDescriptor.IsRequired` / `IsExtension`                                      | property | legacy-required and extension    |
|  [27]   | `FieldDescriptor.ContainingOneof` / `RealContainingOneof`                         | property | oneof membership                 |
|  [28]   | `FieldDescriptor.MessageType` / `EnumType` / `ExtendeeType`                       | property | referenced descriptors           |
|  [29]   | `FieldDescriptor.Accessor -> IFieldAccessor`                                      | property | reflective value access          |
|  [30]   | `FieldDescriptor.CompareTo(FieldDescriptor) -> int`                               | instance | declaration-order sort key       |
|  [31]   | `IFieldAccessor.GetValue(IMessage) -> object`                                     | instance | reflective read                  |
|  [32]   | `IFieldAccessor.SetValue(IMessage, object)`                                       | instance | reflective write                 |
|  [33]   | `IFieldAccessor.HasValue(IMessage) -> bool`                                       | instance | reflective presence              |
|  [34]   | `IFieldAccessor.Clear(IMessage)`                                                  | instance | reflective clear                 |
|  [35]   | `OneofDescriptor.Accessor -> OneofAccessor`                                       | property | oneof case access                |
|  [36]   | `OneofDescriptor.IsSynthetic -> bool`                                             | property | proto3-optional synthetic case   |
|  [37]   | `OneofAccessor.GetCaseFieldDescriptor(IMessage) -> FieldDescriptor`               | instance | active case resolution           |
|  [38]   | `EnumDescriptor.Values -> IList<EnumValueDescriptor>`                             | property | enum-value walk                  |
|  [39]   | `EnumDescriptor.FindValueByNumber(int) -> EnumValueDescriptor`                    | instance | enum lookup by number            |
|  [40]   | `EnumDescriptor.FindValueByName(string) -> EnumValueDescriptor`                   | instance | enum lookup by name              |
|  [41]   | `ServiceDescriptor.Methods -> IList<MethodDescriptor>`                            | property | RPC walk                         |
|  [42]   | `ServiceDescriptor.FindMethodByName(string) -> MethodDescriptor`                  | instance | RPC lookup                       |
|  [43]   | `MethodDescriptor.InputType` / `OutputType`                                       | property | RPC message types                |
|  [44]   | `MethodDescriptor.IsClientStreaming` / `IsServerStreaming`                        | property | RPC streaming shape              |
|  [45]   | `ExtensionCollection.GetExtensionsInNumberOrder(MessageDescriptor)`               | instance | extensions by wire number        |
|  [46]   | `DescriptorDeclaration.LeadingComments` / `TrailingComments`                      | property | source comments for a descriptor |
|  [47]   | `TypeRegistry.FromFiles(IEnumerable<FileDescriptor>) -> TypeRegistry`             | static   | registry over whole files        |
|  [48]   | `TypeRegistry.FromMessages(IEnumerable<MessageDescriptor>) -> TypeRegistry`       | static   | registry over messages           |
|  [49]   | `TypeRegistry.Find(string) -> MessageDescriptor`                                  | instance | full-name resolution             |
|  [50]   | `DescriptorProto.Types.ReservedRange.Start` / `End`                               | property | end-exclusive reserved band      |

- `MessageDescriptor.Parser`: returns the untyped `MessageParser`, so a typed decode goes through the generated static `Parser` instead.

[ENTRYPOINT_SCOPE]: JSON projection

`JsonFormatter` and `JsonParser` each carry an immutable `Settings` builder; one configured instance serves every call it governs.

| [INDEX] | [SURFACE]                                                  | [SHAPE]  | [CAPABILITY]                     |
| :-----: | :--------------------------------------------------------- | :------- | :------------------------------- |
|  [01]   | `new JsonFormatter(Settings)`                              | ctor     | formatter under explicit policy  |
|  [02]   | `JsonFormatter.Format(IMessage) -> string`                 | instance | project to a string              |
|  [03]   | `JsonFormatter.Format(IMessage, int) -> string`            | instance | project at an indentation level  |
|  [04]   | `JsonFormatter.Format(IMessage, TextWriter)`               | instance | project into a writer            |
|  [05]   | `JsonFormatter.WriteValue(TextWriter, object)`             | instance | project one field value          |
|  [06]   | `JsonFormatter.ToDiagnosticString(IMessage) -> string`     | static   | best-effort diagnostic render    |
|  [07]   | `JsonFormatter.Settings.WithFormatDefaultValues(bool)`     | instance | emit default-valued fields       |
|  [08]   | `JsonFormatter.Settings.WithFormatEnumsAsIntegers(bool)`   | instance | emit enums as numbers            |
|  [09]   | `JsonFormatter.Settings.WithPreserveProtoFieldNames(bool)` | instance | emit proto names over JSON names |
|  [10]   | `JsonFormatter.Settings.WithIndentation(string)`           | instance | set the indent unit              |
|  [11]   | `JsonFormatter.Settings.WithTypeRegistry(TypeRegistry)`    | instance | resolve `Any` payload types      |
|  [12]   | `new JsonParser(Settings)`                                 | ctor     | parser under explicit policy     |
|  [13]   | `JsonParser.Parse<T>(string) -> T`                         | instance | typed parse from a string        |
|  [14]   | `JsonParser.Parse<T>(TextReader) -> T`                     | instance | typed parse from a reader        |
|  [15]   | `JsonParser.Parse(string, MessageDescriptor) -> IMessage`  | instance | descriptor-driven parse          |
|  [16]   | `new JsonParser.Settings(int, TypeRegistry)`               | ctor     | recursion limit and registry     |
|  [17]   | `JsonParser.Settings.WithIgnoreUnknownFields(bool)`        | instance | tolerate unclaimed JSON members  |
|  [18]   | `JsonParser.Settings.WithRecursionLimit(int)`              | instance | bound nesting depth              |
|  [19]   | `JsonParser.Settings.WithTypeRegistry(TypeRegistry)`       | instance | resolve `Any` payload types      |

- `JsonFormatter.Settings.WithIndentation`: a non-null indentation inserts `Environment.NewLine` breaks, so a writer already emitting `\r\n` doubles them; `null` disables indentation and keeps the projection single-line.

[ENTRYPOINT_SCOPE]: coded-stream limits, determinism, and size costing

Each `CodedOutputStream.Compute<Kind>Size(value) -> int` costs one wire value, `<Kind>` drawn from the same vocabulary the `FieldCodec` factories carry.

[SIZE_KIND]: `Double` `Float` `Int32` `Int64` `UInt32` `UInt64` `SInt32` `SInt64` `Fixed32` `Fixed64` `SFixed32` `SFixed64` `Bool` `String` `Bytes` `Enum`

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `CodedInputStream.CreateWithLimits(Stream, int, int)`    | static   | bound payload size and depth    |
|  [02]   | `new CodedInputStream(Stream, bool)`                     | ctor     | `leaveOpen` retains the stream  |
|  [03]   | `CodedInputStream.SizeLimit` / `RecursionLimit`          | property | the bounds in force             |
|  [04]   | `CodedInputStream.ReadTag() -> uint`                     | instance | advance to the next field       |
|  [05]   | `CodedInputStream.PeekTag() -> uint`                     | instance | look ahead without advancing    |
|  [06]   | `CodedInputStream.SkipLastField()`                       | instance | discard the current field       |
|  [07]   | `CodedInputStream.IsAtEnd` / `ReachedLimit`              | property | end and scope-end verdicts      |
|  [08]   | `CodedInputStream.Position` / `LastTag`                  | property | cursor state                    |
|  [09]   | `CodedInputStream.DiscardUnknownFields`                  | property | unknown-field drop policy       |
|  [10]   | `CodedInputStream.ExtensionRegistry`                     | property | extensions bound to this read   |
|  [11]   | `CodedOutputStream.Deterministic`                        | property | fix map ordering across writes  |
|  [12]   | `CodedOutputStream.SpaceLeft -> int`                     | property | remaining fixed-buffer capacity |
|  [13]   | `CodedOutputStream.CheckNoSpaceLeft()`                   | instance | fault on an under-filled buffer |
|  [14]   | `CodedOutputStream.WriteTag(int, WireFormat.WireType)`   | instance | emit a field tag                |
|  [15]   | `CodedOutputStream.ComputeMessageSize(IMessage) -> int`  | static   | nested-message cost with length |
|  [16]   | `CodedOutputStream.ComputeGroupSize(IMessage) -> int`    | static   | group cost between its tags     |
|  [17]   | `CodedOutputStream.ComputeTagSize(int) -> int`           | static   | tag cost for a field number     |
|  [18]   | `CodedOutputStream.ComputeLengthSize(int) -> int`        | static   | length-prefix cost              |
|  [19]   | `CodedOutputStream.ComputeRawVarint32Size(uint) -> int`  | static   | raw 32-bit varint cost          |
|  [20]   | `CodedOutputStream.ComputeRawVarint64Size(ulong) -> int` | static   | raw 64-bit varint cost          |

- `CodedOutputStream.Deterministic`: fixes map-field ordering so equal messages serialize to equal bytes within one binary; unknown fields carried across a schema change break that equality, so a persisted canonical form pins its own field order.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Generated messages implement `IMessage<T>` and `IBufferMessage` and expose a static `Parser` and `Descriptor`, so codec and metadata both reach off the type alone.
- `ParseContext` and `WriteContext` are `ref struct` cursors whose `Initialize` factories are package-internal: generated code drives them through `InternalMergeFrom` and `InternalWriteTo`, and a caller reaches the fast path only through the parser, message-extension, and field-codec surfaces.
- Span and sequence entries decode without a stream allocation, and `IMessage.CalculateSize()` pre-sizes a direct `Span<byte>` write, so a pooled buffer round-trips with no intermediate array.
- `UnsafeByteOperations.UnsafeWrap` and `UnsafeCollectionOperations.AsSpan` alias caller storage rather than copying it, so the caller holds that storage unchanged for the message's read window.
- `FieldCodec<T>` is the one read, write, and size unit behind every generated accessor, and `RepeatedField<T>` and `MapField<TKey, TValue>` consume it for bulk wire flow.
- Extension state threads as `ref ExtensionSet<TTarget>`, so an unextended message stores no set, and `UnknownFieldSet` preserves every field the current descriptor does not claim.
- Descriptor identity folds on `DescriptorBase.FullName` with `Index`, and `FileDescriptor.SerializedData` with `ToProto()` returns the payload a contract diff walks.
- `InvalidProtocolBufferException` and `InvalidJsonException` both derive from `IOException`, so one catch shape covers both decode rails.

[STACKING]:
- `Microsoft.IO.RecyclableMemoryStream`(`Rasm.Compute/.api/api-recyclable-stream.md`): a rented stream is the `IBufferWriter<byte>` that `WriteLengthPrefixedTo` writes into, and its `GetReadOnlySequence()` is the fragmented source `ParseFrom(ReadOnlySequence<byte>)` decodes from, so a frame stages and decodes with no contiguous copy.
- `System.IO.Hashing`(`.api/api-hashing.md`): a `Deterministic` write feeds `XxHash128.HashToUInt128` for content identity beside `Crc32.HashToUInt32` for frame integrity, both reading the same staged span.
- `CommunityToolkit.HighPerformance`(`.api/api-highperformance.md`): `MemoryOwner<byte>.DangerousGetArray` hands its rented segment to `UnsafeByteOperations.UnsafeWrap`, and `ArrayPoolBufferWriter<byte>` is the `WriteTo(IBufferWriter<byte>)` target the owner releases after the send.
- `Grpc.Net.Client`(`.api/api-grpc-client.md`): `IMessage<T>` payloads serialize on the call path under the channel's message-size bounds, and a typed fault detail crosses as an `Any` that `TryUnpack<T>` or `Unpack(TypeRegistry)` resolves.
- `Grpc.AspNetCore`(`.api/api-grpc-aspnetcore.md`): the same generated contracts are the server-edge request and response payloads.
- `Grpc.Tools`(`.api/api-grpc-tools.md`): `protoc` emits the `IMessage<T>` classes, their static `Parser` and `Descriptor`, and `field_mask.proto`'s `FieldMask` this surface then operates on.
- `NodaTime.Serialization.Protobuf`(`.api/api-nodatime-protobuf.md`): `Timestamp` and `Duration` fields project through its paired inverses, so the wire clock and interior NodaTime values share one vocabulary and this package's operator algebra stays on the wire side of that seam.
- `Riok.Mapperly`(`.api/api-mapperly.md`): generated mappers transcribe message fields to domain shapes, filling get-only `RepeatedField<T>` and `MapField<TKey, TValue>` members through an existing-target mapping method.
- Within-library: one message crosses as one pooled pass — `CalculateSize()` sizes the frame, `WriteLengthPrefixedTo(IBufferWriter<byte>)` stages it into the rented stream, the digest reads that span, and the receiving leg folds `GetReadOnlySequence()` straight into `ParseFrom(ReadOnlySequence<byte>)`; a partial update rides `FieldMask.Normalize()` then `IsValid` then `Merge` under `MergeOptions`, and a descriptor-diff walker folds `Fields.InFieldNumberOrder()`, `FieldDescriptor.FieldType`, `HasPresence`, and `DescriptorProto.Types.ReservedRange` into the contract checksum.

[LOCAL_ADMISSION]:
- Remote Compute contracts enter through generated `IMessage<T>` surfaces, and this codec stack owns their binary payloads end to end.
- JSON projection serves diagnostics, through one configured `JsonFormatter` or `JsonParser` carrying explicit `Settings` and `TypeRegistry`.
- Reflection descriptors serve diagnostics, contract evolution, and read-only runtime dispatch.
- A proto2 extension or an unknown field enters through the extension and unknown-field surfaces, keeping forward-compatible payloads intact across a re-emit.

[RAIL_LAW]:
- Package: `Google.Protobuf`
- Owns: generated wire contracts, the codec stack, the buffer fast path, `FieldCodec<T>`, the extension and unknown-field algebra, the reflection descriptor graph, the well-known type family, and JSON edge projection
- Accept: generated contracts driven over the span, sequence, and `IBufferWriter<byte>` entries, stacked onto the pooled stream sink and the content-identity digest
- Reject: a per-message serializer written against raw streams beside the generated `IBufferMessage` path

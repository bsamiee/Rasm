# [RASM_PERSISTENCE_API_MESSAGEPACK]

`MessagePack` owns the schemaless binary wire on the snapshot axis: `ref struct` reader and writer tokens over `ReadOnlySequence<byte>`, an attribute-declared contract whose formatters and resolver a source generator emits, and one immutable `MessagePackSerializerOptions` folding the resolver chain, the security ceilings, and in-codec LZ4 framing into a single profile value.

Only an attribute-carrying wire type enters this row; an attribute-free seam graph rides its own codec and a content-stable blob rides canonical CBOR. Content identity keys the uncompressed companion encoding.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePack`
- package: `MessagePack` (MIT)
- assembly: `MessagePack`, `MessagePack.Annotations`
- namespace: `MessagePack`, `MessagePack.Formatters`, `MessagePack.Resolvers`
- target: the `net10.0` consumer binds `lib/net9.0`
- depends: `MessagePack.Annotations` (contract attributes), `MessagePackAnalyzer` (source generator)
- rail: snapshot-codec

## [02]-[PUBLIC_TYPES]

[CODEC_TYPES]: token, profile, and framing surfaces

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]                                    |
| :-----: | :---------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `MessagePackSerializer`             | class         | static codec root: typed, by-`Type`, JSON       |
|  [02]   | `MessagePackSerializer.Typeless`    | class         | nested root writing a CLR type header           |
|  [03]   | `MessagePackReader`                 | ref struct    | pull decoder over `ReadOnlySequence<byte>`      |
|  [04]   | `MessagePackWriter`                 | ref struct    | push encoder over `IBufferWriter<byte>`         |
|  [05]   | `MessagePackStreamReader`           | class         | length-delimited frame reader over a `Stream`   |
|  [06]   | `MessagePackSerializerOptions`      | class         | immutable profile and the typeless type gate    |
|  [07]   | `MessagePackSecurity`               | class         | depth, decompressed-size, and hashing ceilings  |
|  [08]   | `MessagePackCompression`            | enum          | `None`, `Lz4Block` ext99, `Lz4BlockArray` ext98 |
|  [09]   | `MessagePackSerializationException` | class         | contract and decode failure                     |

- value types: `MessagePackType` the next-token discriminant, `Nil`, `ExtensionHeader` a `(sbyte TypeCode, uint Length)` pair, and `ExtensionResult` that code with its payload sequence

[RESOLVER_TYPES]: formatter-lookup families a profile chain composes
- contract: `StandardResolver` `ContractlessStandardResolver` `TypelessContractlessStandardResolver` `StandardResolverAllowPrivate` `ContractlessStandardResolverAllowPrivate`
- composition: `IFormatterResolver` `IMessagePackFormatter<T>` `CompositeResolver` `StaticCompositeResolver` `FormatterResolverExtensions`
- generated: `SourceGeneratedFormatterResolver` `AttributeFormatterResolver` `BuiltinResolver` `PrimitiveObjectResolver`
- encoding: `NativeDateTimeResolver` `NativeGuidResolver` `NativeDecimalResolver` `DynamicEnumAsStringResolver` `DynamicEnumAsStringIgnoreCaseResolver`

[ANNOTATION_TYPES]: contract and generator attributes
- contract: `MessagePackObjectAttribute` `KeyAttribute` `IgnoreMemberAttribute` `UnionAttribute` `SerializationConstructorAttribute` `MessagePackFormatterAttribute`
- generator: `GeneratedMessagePackResolverAttribute` `CompositeResolverAttribute` `MessagePackKnownFormatterAttribute` `MessagePackAssumedFormattableAttribute` `ExcludeFormatterFromSourceGeneratedResolverAttribute`

- `StandardResolver` falls through to runtime reflection emit, so a published-AOT profile composes the generated resolver alone.
- `Native*Resolver` rows emit .NET-native `DateTime`, `Guid`, and `decimal` encodings a non-.NET peer cannot read; a cross-runtime wire keeps the portable formatters.
- `KeyAttribute(int)` encodes an array slot and `KeyAttribute(string)` a map key; `MessagePackObjectAttribute.KeyAsPropertyName` selects map mode wholesale and `AllowPrivate` admits non-public members.
- `UnionAttribute(int, Type)` tags one polymorphic case on the base declaration, and the tag sequence is the wire contract.
- `CompositeResolverAttribute(params Type[])` generates a resolver from resolver TYPES; a standalone `IMessagePackFormatter<T>` instance seats through `CompositeResolver.Create` instead.
- `MessagePackKnownFormatterAttribute` admits a hand-written formatter into the generated resolver, `ExcludeFormatterFromSourceGeneratedResolverAttribute` withholds one for `[MessagePackFormatter]` selection, and `MessagePackAssumedFormattableAttribute` declares a type the program registers itself.

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: `MessagePackSerializer` codec root; every op takes a trailing `MessagePackSerializerOptions?` and `CancellationToken`
- typed write: `Serialize<T>(IBufferWriter<byte>, T)` `Serialize<T>(ref MessagePackWriter, T)` `Serialize<T>(T) -> byte[]` `Serialize<T>(Stream, T)` `SerializeAsync<T>(Stream, T) -> Task`
- typed read: `Deserialize<T>(in ReadOnlySequence<byte>) -> T` `Deserialize<T>(ref MessagePackReader) -> T` `Deserialize<T>(ReadOnlyMemory<byte>, out int) -> T` `Deserialize<T>(Stream) -> T` `DeserializeAsync<T>(Stream) -> ValueTask<T>`
- runtime-typed: `Serialize(Type, IBufferWriter<byte>, object?)` `Deserialize(Type, ReadOnlyMemory<byte>) -> object?` `Typeless.Serialize(object?) -> byte[]` `Typeless.Deserialize(ReadOnlyMemory<byte>) -> object?`
- json projection: `SerializeToJson<T>(TextWriter, T)` `SerializeToJson<T>(T) -> string` `ConvertToJson(in ReadOnlySequence<byte>) -> string` `ConvertFromJson(string) -> byte[]`

- `Deserialize<T>(ReadOnlyMemory<byte>, out int)`: `bytesRead` reports the consumed prefix, so concatenated payloads walk inside one buffer.
- `DefaultOptions` on the root and on `Typeless` are settable process-wide statics, the latter seeded from `TypelessContractlessStandardResolver.Options`; a profile passes its own options per call.

[ENTRYPOINT_SCOPE]: profile, security, and resolver composition; every `With*` returns a new immutable value

| [INDEX] | [SURFACE]                                                       | [SHAPE]  | [CAPABILITY]                                   |
| :-----: | :-------------------------------------------------------------- | :------- | :--------------------------------------------- |
|  [01]   | `MessagePackSerializerOptions.Standard`                         | property | base profile every `With*` chain starts from   |
|  [02]   | `MessagePackSerializerOptions(IFormatterResolver)`              | ctor     | profile bound to one resolver                  |
|  [03]   | `LoadType(string) -> Type?`                                     | instance | typeless type-header resolution seam           |
|  [04]   | `ThrowIfDeserializingTypeIsDisallowed(Type)`                    | instance | typeless deserialization allowlist seam        |
|  [05]   | `MessagePackSecurity.TrustedData`                               | static   | preset without collision-resistant hashing     |
|  [06]   | `MessagePackSecurity.UntrustedData`                             | static   | collision-resistant preset, 64 MiB inflate cap |
|  [07]   | `MessagePackSecurity.DepthStep(ref MessagePackReader)`          | instance | per-edge depth guard a custom formatter calls  |
|  [08]   | `MessagePackSecurity.GetEqualityComparer<T>()`                  | instance | comparer an untrusted hash collection keys on  |
|  [09]   | `CompositeResolver.Create(params IFormatterResolver[])`         | factory  | chains resolvers into one lookup               |
|  [10]   | `StaticCompositeResolver.Register(params IFormatterResolver[])` | instance | mutates the process singleton                  |
|  [11]   | `FormatterResolverExtensions.GetFormatterWithVerify<T>()`       | static   | resolves or throws where lookup returns null   |

- profile mutators: `WithResolver(IFormatterResolver)` `WithSecurity(MessagePackSecurity)` `WithCompression(MessagePackCompression)` `WithCompressionMinLength(int)` `WithSuggestedContiguousMemorySize(int)` `WithOmitAssemblyVersion(bool)` `WithPool(SequencePool)`
- security mutators: `WithMaximumObjectGraphDepth(int)` `WithMaximumDecompressedSize(int)` `WithHashCollisionResistant(bool)`
- resolver statics: `StandardResolver.Instance` `StandardResolver.Options` `ContractlessStandardResolver.Options` `TypelessContractlessStandardResolver.Options` `SourceGeneratedFormatterResolver.Instance` `AttributeFormatterResolver.Instance` `StaticCompositeResolver.Instance`

- `CompositeResolver.Create`: its `(IReadOnlyList<IMessagePackFormatter>, IReadOnlyList<IFormatterResolver>)` overload seats formatter instances ahead of the resolver chain, the one path a standalone `IMessagePackFormatter<T>` takes.
- `MessagePackSecurity`: both presets cap the object graph at 500 frames; `MaximumDecompressedSize` is `int.MaxValue` on `TrustedData` against 64 MiB on `UntrustedData`, and an untrusted profile tightens both. `GetHashCollisionResistantEqualityComparer<T>()` is the protected override seat for a comparer the preset lacks.
- `MessagePackSerializerOptions`: `CompressionMinLength` defaults to 64 bytes, `SuggestedContiguousMemorySize` to 1 MiB, and `SequencePool` to `SequencePool.Shared`; a subclass overrides `Clone()` so the `With*` chain reproduces its own type.

[ENTRYPOINT_SCOPE]: `MessagePackWriter` and `MessagePackReader` tokens a custom `IMessagePackFormatter<T>` composes
- writer framing: `WriteArrayHeader(int)` `WriteMapHeader(int)` `WriteNil()` `WriteRaw(ReadOnlySpan<byte>)` `WriteExtensionFormatHeader(ExtensionHeader)` `WriteExtensionFormat(ExtensionResult)` `Flush()`
- writer primitives: `Write(long)` `Write(int)` `Write(ulong)` `Write(bool)` `Write(char)` `Write(double)` `Write(DateTime)` `Write(string?)` `Write(ReadOnlySpan<byte>)` `Write(ReadOnlySpan<char>)` `Write(in ReadOnlySequence<byte>)`
- writer buffer: `GetSpan(int)` `Advance(int)` `Clone(IBufferWriter<byte>)` `GetEncodedLength(long)` `CancellationToken`
- reader navigation: `NextMessagePackType` `NextCode` `IsNil` `Depth` `Position` `Consumed` `End` `CreatePeekReader()` `Skip()` `Clone(in ReadOnlySequence<byte>)`
- reader framing: `ReadArrayHeader()` `TryReadArrayHeader(out int)` `ReadMapHeader()` `TryReadMapHeader(out int)` `TryReadNil()` `ReadRaw(long)` `ReadExtensionFormatHeader()` `ReadExtensionFormat()`
- reader primitives: `ReadInt64()` `ReadInt32()` `ReadBoolean()` `ReadDouble()` `ReadString()` `ReadDateTime()` `ReadBytes()` `ReadStringSequence()` `TryReadStringSpan(out ReadOnlySpan<byte>)`

- `ReadBytes` and `ReadStringSequence` return `ReadOnlySequence<byte>?` slices over the source buffer; `TryReadStringSpan` yields a zero-copy span only where the string is contiguous.
- `WriteUInt8`, `WriteInt8`, `WriteUInt16`, `WriteInt32`, and `WriteInt64` force a fixed width where the `Write` family emits the shortest encoding.
- `Depth` pairs with `MessagePackSecurity.DepthStep`: a formatter steps before recursing into a child edge and decrements on exit.

[ENTRYPOINT_SCOPE]: `MessagePackStreamReader` framed ingest

| [INDEX] | [SURFACE]                                                   | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :---------------------------------------------------------- | :------- | :------------------------------------------ |
|  [01]   | `MessagePackStreamReader(Stream, bool, SequencePool)`       | ctor     | the flag leaves the stream open on dispose  |
|  [02]   | `ReadAsync(CancellationToken)`                              | instance | next whole frame, `null` at end of stream   |
|  [03]   | `ReadArrayAsync(CancellationToken)`                         | instance | one element per array member                |
|  [04]   | `ReadArrayHeaderAsync(CancellationToken) -> ValueTask<int>` | instance | array length, throwing at end of stream     |
|  [05]   | `ReadMapHeaderAsync(CancellationToken) -> ValueTask<int>`   | instance | map pair count, throwing at end of stream   |
|  [06]   | `RemainingBytes`                                            | property | bytes buffered past the last complete frame |
|  [07]   | `DiscardBufferedData()`                                     | instance | drops the buffer after a stream reposition  |

- `ReadAsync` returns `ValueTask<ReadOnlySequence<byte>?>` and `ReadArrayAsync` an `IAsyncEnumerable<ReadOnlySequence<byte>>`; each result stays valid until `Dispose` or the next read on the same reader.
- `MessagePackStreamReader` is not thread-safe: one call completes, including its asynchronous tail, before the next begins.
- A null `SequencePool` argument throws `ArgumentNullException`; `SequencePool.Shared` is the default rental.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One `MessagePackSerializerOptions` value carries the resolver chain, the security ceilings, and the compression row; a profile builds it once and threads it through every op.
- Resolution is first-hit down the composed chain: a formatter instance seated at the head outranks every resolver behind it, and the first resolver returning non-null owns the type.
- Compression rides inside the codec, `Lz4BlockArray` framing chunk-wise across the payload, so an outer compressor over the same bytes double-frames them.
- `MessagePackReader` and `MessagePackWriter` are `ref struct` cursors threaded by `ref` through formatter calls, which confines them to the synchronous stack frame.
- Untrusted decode composes three ceilings: `UntrustedData` collision-resistant hashing, `WithMaximumObjectGraphDepth` against a recursion bomb, and `WithMaximumDecompressedSize` against an LZ4 expansion bomb.
- A typeless payload carries a CLR type header, so `LoadType` and `ThrowIfDeserializingTypeIsDisallowed` override to a profile-local allowlist before an untrusted header names a type.

[STACKING]:
- `api-messagepack-analyzer`: its generator emits the `[GeneratedMessagePackResolver]` partial and its `MsgPack###` diagnostics reject an unattributed, unkeyed, or key-colliding contract at build, so `[Key]` drift breaks the compile rather than the decode.
- `api-thinktecture-serialization`: `ThinktectureMessageFormatterResolver.Instance` sits in the composed chain and derives one formatter per generated `[ValueObject]`, `[SmartEnum]`, and `[Union]` owner, so a `NodeId` or `ContentAddress` crosses as its bare key with no hand-written codec.
- `api-hashing`: `Version/commits#CRDT_WIRE` `CrdtWire.ContentKey` hashes the `None`-compression companion encoding through the kernel `ContentHash.Of` entry, so the at-rest `Lz4BlockArray` framing stays out of the key and the Python and TypeScript replicas reproduce it byte-for-byte.
- `api-cbor` and `api-chr-avro`: codec-selection peers — MessagePack owns the schemaless evolving record, CBOR the content-stable self-describing blob, Avro the schema-governed leg.
- `api-objectstore`: `MessagePackStreamReader.ReadArrayAsync` is the framed-ingest seam over a length-delimited multi-snapshot blob body, yielding one sequence per element under the codec's own `SequencePool`.
- within-lib: `Element/codec#CODEC_AXIS` `SnapshotCodec` builds one profile from `MessagePackSerializerOptions.Standard` — `CompositeResolver.Create` seating the `InstantFormatter` head ahead of the Thinktecture, generated, and reflection resolvers, `UntrustedData` at a 256-frame depth, and `Lz4BlockArray` — and the published-AOT profile swaps that chain for the single generated resolver behind the same formatter head.
- within-lib: `Query/cache#L2_CONTRIBUTION` reads that same profile through `Deserialize<T>(in ReadOnlySequence<byte>)` and `Serialize<T>(IBufferWriter<byte>, T)`, so durable cache bytes and snapshot bytes share one codec.

[LOCAL_ADMISSION]:
- MessagePack is a codec inside snapshot profiles, never public Persistence vocabulary; contract keys, union tags, resolver composition, compression, and security are profile data.
- Only a type carrying `[MessagePackObject]` or `[MessagePack.Union]` enters the row — negotiation tests the attribute before admission, so a seam graph without it routes to its own codec.
- A stored binary payload carries receipt projection for codec, schema, compression, and redaction class.
- JSON projection is diagnostic output and never owns a snapshot payload.

[RAIL_LAW]:
- Package: `MessagePack`
- Owns: schemaless binary snapshot, cache, and sync wire with in-codec LZ4 framing
- Accept: profile-declared serialization of attribute-carrying wire types, bounded untrusted decode, generated-resolver AOT composition
- Reject: hand-rolled msgpack framing, an outer compressor over an in-codec-compressed payload, a serializer-branded type on a public Persistence surface

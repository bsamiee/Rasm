# [RASM_PERSISTENCE_API_MESSAGEPACK]

`MessagePack` supplies compact binary snapshot codecs, readers, writers,
formatters, resolvers, security policy, compression hooks, extension headers,
and annotation contracts.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePack`
- package: `MessagePack`
- assembly: `MessagePack`
- namespace: `MessagePack`
- asset: runtime library
- rail: snapshot-codec

## [02]-[PUBLIC_TYPES]

[CODEC_TYPES]: reader, writer, and serializer surfaces
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                         | [PACKAGE_ROLE]     | [CAPABILITY]                           |
| :-----: | :------------------------------- | :----------------- | :------------------------------------- |
|  [01]   | `MessagePackSerializer`          | codec root         | serializes snapshots (typed+typeless)  |
|  [02]   | `MessagePackSerializer.Typeless` | typeless codec     | serializes object? with type header    |
|  [03]   | `MessagePackReader`              | reader             | reads encoded payloads                 |
|  [04]   | `MessagePackWriter`              | writer             | writes encoded payloads                |
|  [05]   | `MessagePackStreamReader`        | stream reader      | reads length-delimited streamed frames |
|  [06]   | `MessagePackSerializerOptions`   | codec policy       | configures serialization               |
|  [07]   | `MessagePackSecurity`            | security policy    | controls reader security               |
|  [08]   | `MessagePackCompression`         | compression policy | classifies compression                 |
|  [09]   | `ExtensionHeader`                | extension header   | carries extension metadata             |
|  [10]   | `ExtensionResult`                | extension payload  | carries extension data                 |
|  [11]   | `Nil`                            | nil marker         | carries nil value                      |

`MessagePackSerializer.Typeless` static class: `DefaultOptions` (readable/settable, defaults to `TypelessContractlessStandardResolver.Options`), `Serialize` overloads (`ref MessagePackWriter`, `IBufferWriter<byte>`, `byte[]`, `Stream`), `SerializeAsync(Stream, ...)`, `Deserialize` overloads (`ref MessagePackReader`, `ReadOnlySequence<byte>`, `Stream`, `Memory<byte>`, `ReadOnlyMemory<byte>`), `DeserializeAsync(Stream, ...)`.

`MessagePackCompression` enum cases: `None`, `Lz4Block` (ext type 99, single-block), `Lz4BlockArray` (ext type 98, chunked).
`MessagePackSecurity` presets: `TrustedData` (no hash-collision resistance, unlimited depth), `UntrustedData` (hash-collision resistant). With* mutators: `WithMaximumObjectGraphDepth`, `WithMaximumDecompressedSize`, `WithHashCollisionResistant`.

[RESOLVER_TYPES]: resolver and formatter surfaces
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE]     | [CAPABILITY]                         |
| :-----: | :------------------------------------- | :----------------- | :----------------------------------- |
|  [01]   | `IFormatterResolver`                   | resolver contract  | resolves formatters                  |
|  [02]   | `IMessagePackFormatter<T>`             | formatter contract | formats typed values                 |
|  [03]   | `CompositeResolver`                    | resolver           | composes formatters (factory)        |
|  [04]   | `StandardResolver`                     | resolver           | resolves standard contract types     |
|  [05]   | `ContractlessStandardResolver`         | resolver           | resolves contractless types          |
|  [06]   | `TypelessContractlessStandardResolver` | resolver           | resolves typeless object? payloads   |
|  [07]   | `SourceGeneratedFormatterResolver`     | resolver           | resolves source-generated formatters |
|  [08]   | `AttributeFormatterResolver`           | resolver           | resolves `[MessagePackFormatter]`    |
|  [09]   | `StaticCompositeResolver`              | resolver           | mutable singleton composite          |

`CompositeResolver.Create` overloads: `(IReadOnlyList<IMessagePackFormatter>, IReadOnlyList<IFormatterResolver>)`, `(params IFormatterResolver[])`, `(params IMessagePackFormatter[])`.
`StaticCompositeResolver` singleton `Instance`; `Register(params IMessagePackFormatter[])`, `Register(params IFormatterResolver[])`, `Register(IReadOnlyList<IMessagePackFormatter>, IReadOnlyList<IFormatterResolver>)`.
`StandardResolver.Instance`, `StandardResolver.Options`; same pattern on `ContractlessStandardResolver` and `TypelessContractlessStandardResolver`.

[ANNOTATION_TYPES]: contract attributes
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]        | [CAPABILITY]             |
| :-----: | :-------------------------------------- | :-------------------- | :----------------------- |
|  [01]   | `MessagePackObjectAttribute`            | contract attribute    | declares object contract |
|  [02]   | `KeyAttribute`                          | contract attribute    | assigns member key       |
|  [03]   | `IgnoreMemberAttribute`                 | contract attribute    | excludes member          |
|  [04]   | `UnionAttribute`                        | contract attribute    | declares union case      |
|  [05]   | `MessagePackFormatterAttribute`         | formatter attribute   | selects formatter        |
|  [06]   | `SerializationConstructorAttribute`     | constructor attribute | selects constructor      |
|  [07]   | `GeneratedMessagePackResolverAttribute` | resolver attribute    | marks generated resolver |
|  [08]   | `CompositeResolverAttribute`            | resolver attribute    | declares resolver input  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: codec operations
- rail: snapshot-codec

`Serialize<T>` overloads: `(IBufferWriter<byte>, T, ...)`, `(ref MessagePackWriter, T, ...)`, `(T, ...) → byte[]`, `(Stream, T, ...)`.
`Deserialize<T>` overloads: `(in ReadOnlySequence<byte>, ...)`, `(ref MessagePackReader, ...)`, `(ReadOnlyMemory<byte>, ...)`, `(ReadOnlyMemory<byte>, out int bytesRead, ...)`, `(ReadOnlyMemory<byte>, options, out int bytesRead, ...)`, `(Stream, ...)`.
Typeless overloads (taking `Type` first): `Serialize(Type, ...)`, `Deserialize(Type, ...)`, `SerializeAsync(Type, ...)`, `DeserializeAsync(Type, ...)`.

| [INDEX] | [SURFACE]          | [CALL_SHAPE]     | [CAPABILITY]                                       |
| :-----: | :----------------- | :--------------- | :------------------------------------------------- |
|  [01]   | `Serialize`        | codec call       | writes typed or typeless snapshot payload          |
|  [02]   | `Deserialize`      | codec call       | reads typed or typeless snapshot payload           |
|  [03]   | `SerializeAsync`   | async codec call | writes streamed payload                            |
|  [04]   | `DeserializeAsync` | async codec call | reads streamed payload                             |
|  [05]   | `SerializeToJson`  | projection call  | serializes directly to JSON string or `TextWriter` |
|  [06]   | `ConvertToJson`    | projection call  | converts existing msgpack bytes to JSON            |
|  [07]   | `ConvertFromJson`  | ingestion call   | converts JSON string/TextReader to msgpack         |
|  [08]   | `DefaultOptions`   | static property  | process-wide default options (gettable/settable)   |
|  [09]   | `WriteArrayHeader` | writer call      | writes array header                                |
|  [10]   | `WriteMapHeader`   | writer call      | writes map header                                  |
|  [11]   | `ReadArrayHeader`  | reader call      | reads array header                                 |
|  [12]   | `ReadMapHeader`    | reader call      | reads map header                                   |

`SerializeToJson<T>` overloads: `(TextWriter, T, ...)` and `(T, ...) → string`.
`ConvertToJson` overloads: `(ReadOnlyMemory<byte>, ...)`, `(in ReadOnlySequence<byte>, ...)`, `(ref MessagePackReader, TextWriter, ...)`.
`ConvertFromJson` overloads: `(string, ref MessagePackWriter, ...)`, `(string, ...) → byte[]`, `(TextReader, ref MessagePackWriter, ...)`.

[ENTRYPOINT_SCOPE]: resolver and policy operations
- rail: snapshot-codec

| [INDEX] | [SURFACE]                                | [CALL_SHAPE]    | [CAPABILITY]                                |
| :-----: | :--------------------------------------- | :-------------- | :------------------------------------------ |
|  [01]   | `WithResolver`                           | option call     | sets resolver policy                        |
|  [02]   | `WithSecurity`                           | option call     | sets security policy                        |
|  [03]   | `WithCompression`                        | option call     | sets compression policy                     |
|  [04]   | `WithCompressionMinLength`               | option call     | sets minimum byte length for compression    |
|  [05]   | `WithOldSpec`                            | option call     | forces MessagePack v1 encoding              |
|  [06]   | `WithOmitAssemblyVersion`                | option call     | omits assembly version on typeless          |
|  [07]   | `WithAllowAssemblyVersionMismatch`       | option call     | tolerates version mismatches on typeless    |
|  [08]   | `WithPool`                               | option call     | substitutes `SequencePool`                  |
|  [09]   | `WithSuggestedContiguousMemorySize`      | option call     | hints contiguous buffer cap (default 1MB)   |
|  [10]   | `GetFormatter<T>`                        | resolver call   | resolves formatter                          |
|  [11]   | `CompositeResolver.Create`               | factory call    | composes resolver from formatters/resolvers |
|  [12]   | `MessagePackSerializerOptions.Standard`  | static preset   | default standard options                    |
|  [13]   | `MessagePackSecurity.TrustedData`        | security preset | trusted-data (default) reading              |
|  [14]   | `MessagePackSecurity.UntrustedData`      | security preset | hardens untrusted reading                   |
|  [15]   | `MessagePackCompression.Lz4BlockArray`   | compression row | chunked LZ4 framing (ext98)                 |
|  [16]   | `MessagePackCompression.Lz4Block`        | compression row | single-block LZ4 framing (ext99)            |
|  [17]   | `MessagePackStreamReader.ReadAsync`      | segment read    | next length-delimited frame                 |
|  [18]   | `MessagePackStreamReader.ReadArrayAsync` | enumerable read | streamed array elements                     |

`ReadAsync` returns `ValueTask<ReadOnlySequence<byte>?>`; null signals end of stream.
`MessagePackSerializerOptions` properties: `Resolver`, `Compression`, `CompressionMinLength` (default 64), `SuggestedContiguousMemorySize` (default 1MB), `OldSpec`, `OmitAssemblyVersion`, `AllowAssemblyVersionMismatch`, `Security` (default `TrustedData`), `SequencePool`.

## [04]-[IMPLEMENTATION_LAW]

[SNAPSHOT_CODEC]:
- namespace: `MessagePack`
- codec root: `MessagePackSerializer`
- policy root: `MessagePackSerializerOptions`
- resolver root: formatter resolvers and generated resolvers
- contract root: object, key, union, ignore, formatter, and constructor attributes

[LOCAL_ADMISSION]:
- MessagePack is a codec inside snapshot profiles, not public Persistence vocabulary.
- Contract keys, union tags, resolver composition, compression, and security are profile data.
- Binary payloads require receipt projection for codec, schema, compression, and redaction class.
- JSON projection is diagnostic output and cannot replace snapshot payload ownership.

[RAIL_LAW]:
- Package: `MessagePack`
- Owns: binary snapshot codec
- Accept: profile-declared snapshot serialization
- Reject: serializer-branded public APIs

[STREAM_READER_CONTRACT]: `public class MessagePackStreamReader : IDisposable` — not thread-safe; complete each async call before issuing the next

```csharp
public MessagePackStreamReader(Stream stream)
public MessagePackStreamReader(Stream stream, bool leaveOpen)
public MessagePackStreamReader(Stream stream, bool leaveOpen, SequencePool pool)
public ReadOnlySequence<byte> RemainingBytes { get; }
public ValueTask<ReadOnlySequence<byte>?> ReadAsync(CancellationToken cancellationToken)
public IAsyncEnumerable<ReadOnlySequence<byte>> ReadArrayAsync(CancellationToken cancellationToken)
public ValueTask<int> ReadArrayHeaderAsync(CancellationToken cancellationToken)
public ValueTask<int> ReadMapHeaderAsync(CancellationToken cancellationToken)
public void DiscardBufferedData()
public void Dispose()
```

- `pool` is required when supplied; `null` throws `ArgumentNullException`.
- `RemainingBytes` is valid until the next read.
- `ReadAsync` returns `null` at end of stream; array and map header reads throw on end-of-stream; `ReadArrayAsync` throws on truncation.
- `ReadAsync` result sequence is valid until `Dispose` or the next `ReadAsync` call, whichever is first.
- `ReadArrayAsync` result sequences carry the same validity window per element.
- `DiscardBufferedData` is required after seeking the underlying stream between reads.
- `SequencePool.Shared` is the default pool; inject a custom pool only for memory-budget profiling.

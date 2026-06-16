# [RASM_PERSISTENCE_API_MESSAGEPACK]

`MessagePack` supplies compact binary snapshot codecs, readers, writers,
formatters, resolvers, security policy, compression hooks, extension headers,
and annotation contracts.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePack`
- package: `MessagePack`
- assembly: `MessagePack`
- namespace: `MessagePack`
- asset: runtime library
- rail: snapshot-codec

## [2]-[PUBLIC_TYPES]

[CODEC_TYPES]: reader, writer, and serializer surfaces
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                       | [PACKAGE_ROLE]     | [CAPABILITY]               |
| :-----: | :----------------------------- | :----------------- | :------------------------- |
|   [1]   | `MessagePackSerializer`        | codec root         | serializes snapshots       |
|   [2]   | `MessagePackReader`            | reader             | reads encoded payloads     |
|   [3]   | `MessagePackWriter`            | writer             | writes encoded payloads    |
|   [4]   | `MessagePackStreamReader`      | stream reader      | reads streamed payloads    |
|   [5]   | `MessagePackSerializerOptions` | codec policy       | configures serialization   |
|   [6]   | `MessagePackSecurity`          | security policy    | controls reader security   |
|   [7]   | `MessagePackCompression`       | compression policy | classifies compression     |
|   [8]   | `ExtensionHeader`              | extension header   | carries extension metadata |
|   [9]   | `ExtensionResult`              | extension payload  | carries extension data     |
|  [10]   | `Nil`                          | nil marker         | carries nil value          |

[RESOLVER_TYPES]: resolver and formatter surfaces
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                           | [PACKAGE_ROLE]     | [CAPABILITY]                   |
| :-----: | :--------------------------------- | :----------------- | :----------------------------- |
|   [1]   | `IFormatterResolver`               | resolver contract  | resolves formatters            |
|   [2]   | `IMessagePackFormatter<T>`         | formatter contract | formats typed values           |
|   [3]   | `CompositeResolver`                | resolver           | composes formatters            |
|   [4]   | `StandardResolver`                 | resolver           | resolves standard types        |
|   [5]   | `ContractlessStandardResolver`     | resolver           | resolves contractless values   |
|   [6]   | `SourceGeneratedFormatterResolver` | resolver           | resolves generated formatters  |
|   [7]   | `AttributeFormatterResolver`       | resolver           | resolves attributed formatters |
|   [8]   | `StaticCompositeResolver`          | resolver           | composes static formatters     |

[ANNOTATION_TYPES]: contract attributes
- rail: snapshot-codec

| [INDEX] | [SYMBOL]                                | [PACKAGE_ROLE]        | [CAPABILITY]             |
| :-----: | :-------------------------------------- | :-------------------- | :----------------------- |
|   [1]   | `MessagePackObjectAttribute`            | contract attribute    | declares object contract |
|   [2]   | `KeyAttribute`                          | contract attribute    | assigns member key       |
|   [3]   | `IgnoreMemberAttribute`                 | contract attribute    | excludes member          |
|   [4]   | `UnionAttribute`                        | contract attribute    | declares union case      |
|   [5]   | `MessagePackFormatterAttribute`         | formatter attribute   | selects formatter        |
|   [6]   | `SerializationConstructorAttribute`     | constructor attribute | selects constructor      |
|   [7]   | `GeneratedMessagePackResolverAttribute` | resolver attribute    | marks generated resolver |
|   [8]   | `CompositeResolverAttribute`            | resolver attribute    | declares resolver input  |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: codec operations
- rail: snapshot-codec

| [INDEX] | [SURFACE]          | [CALL_SHAPE]     | [CAPABILITY]            |
| :-----: | :----------------- | :--------------- | :---------------------- |
|   [1]   | `Serialize`        | codec call       | writes snapshot payload |
|   [2]   | `Deserialize`      | codec call       | reads snapshot payload  |
|   [3]   | `SerializeAsync`   | async codec call | writes streamed payload |
|   [4]   | `DeserializeAsync` | async codec call | reads streamed payload  |
|   [5]   | `ConvertToJson`    | projection call  | projects debug JSON     |
|   [6]   | `WriteArrayHeader` | writer call      | writes array header     |
|   [7]   | `WriteMapHeader`   | writer call      | writes map header       |
|   [8]   | `ReadArrayHeader`  | reader call      | reads array header      |
|   [9]   | `ReadMapHeader`    | reader call      | reads map header        |

[ENTRYPOINT_SCOPE]: resolver and policy operations
- rail: snapshot-codec

| [INDEX] | [SURFACE]                              | [CALL_SHAPE]    | [CAPABILITY]               |
| :-----: | :------------------------------------- | :-------------- | :------------------------- |
|   [1]   | `WithResolver`                         | option call     | sets resolver policy       |
|   [2]   | `WithSecurity`                         | option call     | sets security policy       |
|   [3]   | `WithCompression`                      | option call     | sets compression policy    |
|   [4]   | `GetFormatter<T>`                      | resolver call   | resolves formatter         |
|   [5]   | `CompositeResolver.Create`             | factory call    | composes resolver          |
|   [6]   | `MessagePackSecurity.UntrustedData`    | security preset | hardens untrusted reading  |
|   [7]   | `MessagePackCompression.Lz4BlockArray` | compression row | block-array LZ4 framing    |
|   [8]   | `MessagePackCompression.Lz4Block`      | compression row | block LZ4 framing          |
|   [9]   | `MessagePackStreamReader.ReadAsync`    | segment read    | length-delimited frames    |
|  [10]   | `ConvertToJson`                        | projection call | diagnostic JSON projection |

`ReadAsync` returns `ValueTask<ReadOnlySequence<byte>?>`; `ConvertToJson` accepts payload memory, serializer options, and cancellation.

## [4]-[IMPLEMENTATION_LAW]

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

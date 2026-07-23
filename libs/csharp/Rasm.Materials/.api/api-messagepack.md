# [RASM_MATERIALS_API_MESSAGEPACK]

`MessagePack` (neuecc / MessagePack-CSharp) mints the compact binary appearance-interchange wire: the Materials appearance/material model serialized through a source-generated, IL-emit-free resolver over `[MessagePackObject]`/`[Key]`/`[Union]` modeling with LZ4 compression. It is the binary peer of the human-readable JSON leg; `ConvertToJson` stays a diagnostic bridge, never the JSON system of record.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePack`
- package: `MessagePack` (MIT)
- assembly: `MessagePack` (engine) + `MessagePack.Annotations` (attribute markers, pure-managed)
- namespace: `MessagePack`, `MessagePack.Resolvers`, `MessagePack.Formatters`
- asset: multi-target `net9.0`/`net8.0`/`netstandard2.1`/`netstandard2.0`/`net472`; the `net10.0` consumer binds `lib/net9.0/MessagePack.dll`, `MessagePack.Annotations` binds `lib/netstandard2.0`
- owner: DIRECT in `Rasm.Materials` (engine + `MessagePackAnalyzer`); the `interchange` `WireCodec` composes `MessagePackSerializer` and the modeling attributes directly
- rail: appearance-interchange (binary wire)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serializer facade, options, and zero-copy primitives — `MessagePack`

| [INDEX] | [SYMBOL]                            | [TYPE_FAMILY] | [CAPABILITY]              |
| :-----: | :---------------------------------- | :------------ | :------------------------ |
|  [01]   | `MessagePackSerializer`             | static class  | serialization facade      |
|  [02]   | `MessagePackSerializerOptions`      | class         | `Standard` and `With*`    |
|  [03]   | `MessagePackReader`                 | ref struct    | typed wire reads          |
|  [04]   | `MessagePackWriter`                 | ref struct    | typed wire writes         |
|  [05]   | `MessagePackCompression`            | enum          | compression policy        |
|  [06]   | `MessagePackSecurity`               | class         | input hardening           |
|  [07]   | `MessagePackStreamReader`           | class         | top-level message framing |
|  [08]   | `MessagePackType`                   | enum          | wire format family        |
|  [09]   | `MessagePackCode`                   | static class  | wire byte codes           |
|  [10]   | `ExtensionHeader`                   | struct        | custom embedding          |
|  [11]   | `ExtensionResult`                   | struct        | custom embedding          |
|  [12]   | `Nil`                               | struct        | `Nil.Default` singleton   |
|  [13]   | `MessagePackSerializationException` | exception     | serialization failure     |
|  [14]   | `FormatterNotRegisteredException`   | exception     | missing formatter         |

[PUBLIC_TYPE_SCOPE]: formatter and resolver contracts — `MessagePack.Formatters`

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]            |
| :-----: | :------------------------------------------ | :------------ | :---------------------- |
|  [01]   | `IFormatterResolver`                        | interface     | formatter lookup        |
|  [02]   | `IMessagePackFormatter<T>`                  | interface     | reader-writer codec     |
|  [03]   | `IMessagePackSerializationCallbackReceiver` | interface     | serialization callbacks |

- `IMessagePackSerializationCallbackReceiver`: `OnBeforeSerialize`/`OnAfterDeserialize` fire around the serialized type.

[PUBLIC_TYPE_SCOPE]: resolver catalog — `MessagePack.Resolvers`

| [INDEX] | [SYMBOL]                               | [TYPE_FAMILY]    | [CAPABILITY]          |
| :-----: | :------------------------------------- | :--------------- | :-------------------- |
|  [01]   | `StandardResolver`                     | default resolver | built-in chain        |
|  [02]   | `ContractlessStandardResolver`         | contractless     | property-name keys    |
|  [03]   | `CompositeResolver`                    | chain builder    | `Create` overloads    |
|  [04]   | `BuiltinResolver`                      | leaf resolver    | primitives            |
|  [05]   | `PrimitiveObjectResolver`              | leaf resolver    | `object` graph        |
|  [06]   | `DynamicObjectResolver`                | runtime IL emit  | formatter generation  |
|  [07]   | `DynamicObjectResolverAllowPrivate`    | runtime IL emit  | private-member access |
|  [08]   | `DynamicEnumResolver`                  | enum resolver    | numeric encoding      |
|  [09]   | `DynamicEnumAsStringResolver`          | enum resolver    | string encoding       |
|  [10]   | `TypelessObjectResolver`               | typeless         | CLR type names        |
|  [11]   | `TypelessContractlessStandardResolver` | typeless         | `DefaultOptions`      |
|  [12]   | `NativeGuidResolver`                   | native           | `Guid` encoding       |
|  [13]   | `NativeDateTimeResolver`               | native           | `DateTime` encoding   |
|  [14]   | `NativeDecimalResolver`                | native           | `decimal` encoding    |

[PUBLIC_TYPE_SCOPE]: modeling attributes — `MessagePack.Annotations`

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]     | [CAPABILITY]            |
| :-----: | :-------------------------------------- | :---------------- | :---------------------- |
|  [01]   | `MessagePackObjectAttribute`            | type marker       | object modeling         |
|  [02]   | `KeyAttribute`                          | member key        | positional or map key   |
|  [03]   | `IgnoreMemberAttribute`                 | member skip       | serialization exclusion |
|  [04]   | `UnionAttribute`                        | polymorphism      | union dispatch          |
|  [05]   | `MessagePackFormatterAttribute`         | custom codec      | formatter binding       |
|  [06]   | `SerializationConstructorAttribute`     | ctor selector     | deserialization ctor    |
|  [07]   | `GeneratedMessagePackResolverAttribute` | source-gen marker | AOT resolver            |
|  [08]   | `CompositeResolverAttribute`            | source-gen marker | composite resolver      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serialize and deserialize — `MessagePackSerializer`

| [INDEX] | [SURFACE]                                                  | [SHAPE]         | [CAPABILITY]               |
| :-----: | :--------------------------------------------------------- | :-------------- | :------------------------- |
|  [01]   | `Serialize<T>(T, options?, ct?) -> byte[]`                 | static          | fresh array                |
|  [02]   | `Serialize<T>(IBufferWriter<byte>, T, options?, ct?)`      | static          | pooled-buffer write        |
|  [03]   | `Serialize<T>(ref MessagePackWriter, T, options?)`         | static          | caller-writer composition  |
|  [04]   | `Serialize<T>(Stream, T, options?, ct?)`                   | static          | stream write               |
|  [05]   | `SerializeAsync<T>(Stream, T, options?, ct?) -> Task`      | static          | async stream write         |
|  [06]   | `Deserialize<T>(ReadOnlyMemory<byte>, options?, ct?) -> T` | static          | memory-block read          |
|  [07]   | `Deserialize<T>(in ReadOnlySequence<byte>, options?) -> T` | static          | multi-segment zero-copy    |
|  [08]   | `Deserialize<T>(ref MessagePackReader, options?) -> T`     | static          | caller-reader composition  |
|  [09]   | `Deserialize<T>(ReadOnlyMemory<byte>, options?, out int)`  | static          | framed bytes-consumed read |
|  [10]   | `Deserialize<T>(Stream, options?, ct?) -> T`               | static          | stream read                |
|  [11]   | `DeserializeAsync<T>(Stream, …) -> ValueTask<T>`           | static          | async stream read          |
|  [12]   | `MessagePackSerializer.DefaultOptions`                     | static property | process default            |

[ENTRYPOINT_SCOPE]: JSON bridge and typeless — `MessagePackSerializer`

| [INDEX] | [SURFACE]                                                      | [SHAPE] | [CAPABILITY]                 |
| :-----: | :------------------------------------------------------------- | :------ | :--------------------------- |
|  [01]   | `ConvertToJson(ReadOnlyMemory<byte>, options?, ct?) -> string` | static  | bytes to JSON text           |
|  [02]   | `ConvertToJson(ref MessagePackReader, TextWriter, options?)`   | static  | streamed JSON projection     |
|  [03]   | `ConvertFromJson(string, options?, ct?) -> byte[]`             | static  | JSON text to bytes           |
|  [04]   | `SerializeToJson<T>(T, options?, ct?) -> string`               | static  | model to JSON text           |
|  [05]   | `MessagePackSerializer.Typeless.Serialize`                     | static  | type-embedded serialization  |
|  [06]   | `MessagePackSerializer.Typeless.Deserialize`                   | static  | heterogeneous object graph   |
|  [07]   | `Serialize(Type, object?, options?, ct?)`                      | static  | runtime-type serialization   |
|  [08]   | `Deserialize(Type, …) -> object?`                              | static  | runtime-type deserialization |

[ENTRYPOINT_SCOPE]: options builder — `MessagePackSerializerOptions`

| [INDEX] | [SURFACE]                                 | [SHAPE]         | [CAPABILITY]             |
| :-----: | :---------------------------------------- | :-------------- | :----------------------- |
|  [01]   | `MessagePackSerializerOptions.Standard`   | static property | standard-resolver base   |
|  [02]   | `WithResolver(IFormatterResolver)`        | instance        | resolution-chain swap    |
|  [03]   | `WithCompression(MessagePackCompression)` | instance        | LZ4 payload compression  |
|  [04]   | `WithCompressionMinLength(int)`           | instance        | threshold (default `64`) |
|  [05]   | `WithSecurity(MessagePackSecurity)`       | instance        | untrusted-input policy   |
|  [06]   | `WithPool(SequencePool)`                  | instance        | custom buffer pool       |
|  [07]   | `WithOldSpec(bool?)`                      | instance        | spec interop             |
|  [08]   | `WithOmitAssemblyVersion(bool)`           | instance        | typeless metadata        |
|  [09]   | `WithAllowAssemblyVersionMismatch(bool)`  | instance        | typeless metadata        |

[ENTRYPOINT_SCOPE]: resolver composition and hardening — `Resolvers` / `MessagePackSecurity`

| [INDEX] | [SURFACE]                                               | [SHAPE]         | [CAPABILITY]              |
| :-----: | :------------------------------------------------------ | :-------------- | :------------------------ |
|  [01]   | `CompositeResolver.Create(params IFormatterResolver[])` | static          | priority resolver chain   |
|  [02]   | `StandardResolver.Instance`                             | static property | default fallback          |
|  [03]   | `StandardResolver.Options`                              | static property | pre-built options         |
|  [04]   | `MessagePackSecurity.UntrustedData`                     | static property | hardened base policy      |
|  [05]   | `MessagePackSecurity.TrustedData`                       | static property | trusted base policy       |
|  [06]   | `WithMaximumObjectGraphDepth(int)`                      | instance        | depth cap (default `500`) |
|  [07]   | `WithMaximumDecompressedSize(int)`                      | instance        | LZ4 decompression cap     |
|  [08]   | `WithHashCollisionResistant(bool)`                      | instance        | resistant map comparers   |
|  [09]   | `IFormatterResolver.GetFormatter<T>()`                  | instance        | formatter lookup          |
|  [10]   | `IMessagePackFormatter<T>.Serialize`                    | instance        | custom serialization      |
|  [11]   | `IMessagePackFormatter<T>.Deserialize`                  | instance        | custom deserialization    |

- `CompositeResolver.Create`: a second overload takes paired `IReadOnlyList<IMessagePackFormatter>` and `IReadOnlyList<IFormatterResolver>`, formatters first.

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `MessagePackSerializer` is the one entry; the generic `<T>` family is the default, and the `Type`-keyed and `Typeless` forms mirror it only at a heterogeneous `object` boundary.
- `MessagePackSerializerOptions` is immutable — one instance per wire profile (resolver, compression, security), reused, each `With*` returning a fresh instance, never per-call construction. `DefaultOptions` is the process default and equals `TypelessContractlessStandardResolver.Options`.
- Zero-copy is the path: deserialize off a multi-segment `ReadOnlySequence<byte>`, serialize into an `IBufferWriter<byte>`, and the `ref MessagePackReader`/`ref MessagePackWriter` overloads fold a custom `IMessagePackFormatter<T>` into the parent graph with no `byte[]` round-trip.
- `MessagePackCompression.Lz4BlockArray` is the streaming array form for large graphs and `Lz4Block` the single-block form; `WithCompressionMinLength` (default `64`) gates small payloads out of compression.
- Records model through `[MessagePackObject]` + `[Key(int)]` (compact positional array) with `[SerializationConstructor]` selecting the immutable ctor; `[Key(string)]` (map form) is the forward-compatible schema. `[Union(int key, Type subType)]` on the base type dispatches the polymorphic `BsdfLobe`/appearance hierarchy, each arm a stable integer key.
- `[GeneratedMessagePackResolver]` source-generates the host-neutral resolver partial (AOT-safe, IL-emit-free, exposing `UseMapMode`), never the runtime-reflection `DynamicObjectResolver`; `MessagePackAnalyzer` enforces `[Key]` coverage and union completeness at compile time.
- External wire applies `MessagePackSecurity.UntrustedData` (depth cap `500`, decompression-size cap, collision-resistant comparers) via `WithSecurity`, and `TrustedData` serves in-process self-produced bytes only; the typed `MessagePackSerializationException`/`FormatterNotRegisteredException` rail lowers into the folder's `LanguageExt` `Fin`/`Validation` boundary.

[STACKING]:
- `Thinktecture.Runtime.Extensions.MessagePack`(`.api/api-thinktecture-messagepack.md`): `ThinktectureMessageFormatterResolver.Instance` composes ahead of `StandardResolver.Instance` through `CompositeResolver.Create(...)`, serializing the folder's `[SmartEnum]`/`[ValueObject]`/`[Union]` identity types by their key value with no hand-written formatter.
- within-lib: the `interchange` `WireCodec` composes `MessagePackSerializer` and the `[MessagePackObject]`/`[Key]`/`[GeneratedMessagePackResolver]` modeling beyond the bridge-resolved value objects, one shared immutable `MessagePackSerializerOptions` carrying resolver, compression, and security for the whole appearance-interchange profile.

[LOCAL_ADMISSION]:
- `MessagePack` owns the compact binary wire only; the human-readable JSON peer is `Thinktecture.Runtime.Extensions.Json` + `UnitsNet.Serialization.JsonNet`, and `ConvertToJson`/`SerializeToJson` are diagnostic bridges, not the JSON system of record.
- This is the neuecc `MessagePack-CSharp` engine; the PolyType-based `Nerdbank.MessagePack` is a different package — its attributes, resolvers, and formatters never mix with this one's.
- `UnitsNet` quantities and `Wacton.Unicolour` colors serialize as member values through the standard resolver or a small `IMessagePackFormatter<T>`, never a re-minted quantity/color codec the standard chain already covers.

[RAIL_LAW]:
- Package: `MessagePack` (MessagePack-CSharp / neuecc)
- Owns: the compact binary appearance/material interchange wire — `MessagePackSerializer` over `[MessagePackObject]`/`[Key]`/`[Union]`-modelled records, resolver composition, LZ4 compression, and untrusted-input hardening
- Accept: the generic `Serialize`/`Deserialize<T>` family (`byte[]`/`Stream`/`IBufferWriter`/`ReadOnlySequence`/`ref Reader`/`ref Writer`, async); one shared immutable `MessagePackSerializerOptions`; `CompositeResolver.Create(ThinktectureMessageFormatterResolver.Instance, StandardResolver.Instance)`; `[GeneratedMessagePackResolver]` source-gen; `MessagePackSecurity.UntrustedData` on external wire; the typed exception rail
- Reject: the runtime-reflection `DynamicObjectResolver` as the AOT path; per-call options construction; `ConvertToJson` as the JSON system of record; `TrustedData` on external input; a hand-rolled quantity/color codec the standard resolver provides

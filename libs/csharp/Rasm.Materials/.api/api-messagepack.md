# [RASM_MATERIALS_API_MESSAGEPACK]

`MessagePack` (neuecc / MessagePack-CSharp) is the COMPACT BINARY interchange wire owner under the Materials `Appearance/interchange` concept — the binary peer of the human-readable JSON leg (`Thinktecture.Runtime.Extensions.Json` + `UnitsNet.Serialization.JsonNet`). It serializes the appearance/material model — the `MaterialGraph` node tree, the `BsdfLobe` `[Union]` lobe family, the `MaterialLibrary` rows, and the OpenPBR/MaterialX wire vocabulary — into LZ4-compressible MessagePack, modelled through `[MessagePackObject]`/`[Key]` on records, `[Union]` for the polymorphic lobe/appearance hierarchy, and a `[GeneratedMessagePackResolver]` source-generated (AOT-safe, IL-emit-free) resolver. Its decisive integration is the `Thinktecture.Runtime.Extensions.MessagePack` bridge: `ThinktectureMessageFormatterResolver.Instance` composed ahead of `StandardResolver.Instance` via `CompositeResolver.Create(...)` serializes the folder's Thinktecture `[SmartEnum]`/`[ValueObject]`/`[Union]` identity types natively, with no hand-written formatter. `Rasm.Materials` carries the DIRECT `MessagePack` + `MessagePackAnalyzer` references (versions central): the `Appearance/interchange` `WireCodec` composes `MessagePackSerializer` and the `[MessagePackObject]`/`[Key]`/`[GeneratedMessagePackResolver]` modeling surface directly, beyond the bridge-resolved value-objects; the same central pins also own `Rasm.Persistence` and the StreamJsonRpc bridge closure. This is the neuecc `MessagePack-CSharp` engine — distinct from the PolyType-based `Nerdbank.MessagePack` that rides the bridge closure; do not conflate the two.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `MessagePack`

- package: `MessagePack` (MessagePack-CSharp / neuecc)
- license: `MIT`
- assemblies: `MessagePack` (engine) + `MessagePack.Annotations` (the attribute marker set)
- namespace roots: `MessagePack`, `MessagePack.Resolvers`, `MessagePack.Formatters`
- asset: multi-target `net9.0` / `net8.0` / `netstandard2.1` / `netstandard2.0` / `net472`; the `net10.0` consumer binds `lib/net9.0/MessagePack.dll`; `MessagePack.Annotations` binds `lib/netstandard2.0` (pure-managed)
- ownership: DIRECT in `Rasm.Materials` (`MessagePack` + analyzer-style `MessagePackAnalyzer`, version-less against the central pins) — the interchange design composes the facade and modeling attributes directly; the same pins own `Rasm.Persistence` + the bridge closure
- rail: appearance-interchange (binary wire)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: serializer facade, options, and zero-copy primitives — `MessagePack`

- rail: appearance-interchange

`MessagePackSerializer` spans byte arrays, streams, buffers, sequences, asynchronous operations, and the JSON bridge. `MessagePackReader`, a `ref struct` over `ReadOnlySequence<byte>`, owns typed reads, skipping, and peeking; `MessagePackWriter`, a `ref struct` over `IBufferWriter<byte>`, owns typed writes, headers, and raw payloads. `MessagePackCompression` admits `None`, `Lz4Block`, and `Lz4BlockArray`; `MessagePackSecurity` admits `TrustedData` and `UntrustedData` with depth and decompression caps. `MessagePackStreamReader` reads consecutive top-level messages from a `Stream`, and extension types carry custom binary embeddings as type-code and payload pairs.

| [INDEX] | [SYMBOL]                            | [KIND]               | [CAPABILITY]              |
| :-----: | :---------------------------------- | :------------------- | :------------------------ |
|  [01]   | `MessagePackSerializer`             | static facade        | serialization             |
|  [02]   | `MessagePackSerializerOptions`      | immutable options    | `Standard` and `With*`    |
|  [03]   | `MessagePackReader`                 | zero-copy reader     | typed wire reads          |
|  [04]   | `MessagePackWriter`                 | zero-copy writer     | typed wire writes         |
|  [05]   | `MessagePackCompression`            | compression enum     | compression policy        |
|  [06]   | `MessagePackSecurity`               | hardening policy     | input hardening           |
|  [07]   | `MessagePackStreamReader`           | framed stream reader | top-level message framing |
|  [08]   | `MessagePackType`                   | wire vocabulary      | format-family enum        |
|  [09]   | `MessagePackCode`                   | wire vocabulary      | byte-code table           |
|  [10]   | `ExtensionHeader`                   | extension type       | custom embedding          |
|  [11]   | `ExtensionResult`                   | extension type       | custom embedding          |
|  [12]   | `Nil`                               | nil value            | `Nil.Default` singleton   |
|  [13]   | `MessagePackSerializationException` | error rail           | serialization failure     |
|  [14]   | `FormatterNotRegisteredException`   | error rail           | missing formatter         |

[PUBLIC_TYPE_SCOPE]: formatter + resolver contracts — `MessagePack` / `MessagePack.Formatters`

- rail: appearance-interchange

`IFormatterResolver.GetFormatter<T>()` maps types to formatters. `IMessagePackFormatter<T>` owns `Serialize(ref Writer, T, options)` and `Deserialize(ref Reader, options)`, while `IMessagePackSerializationCallbackReceiver` binds `OnBeforeSerialize` and `OnAfterDeserialize` to the serialized type.

| [INDEX] | [SYMBOL]                                    | [KIND]            | [CAPABILITY]            |
| :-----: | :------------------------------------------ | :---------------- | :---------------------- |
|  [01]   | `IFormatterResolver`                        | resolver contract | formatter lookup        |
|  [02]   | `IMessagePackFormatter<T>`                  | codec contract    | reader-writer codec     |
|  [03]   | `IMessagePackSerializationCallbackReceiver` | lifecycle hook    | serialization callbacks |

[PUBLIC_TYPE_SCOPE]: resolver catalog — `MessagePack.Resolvers`

- rail: appearance-interchange
- Resolvers are composed in priority order; `CompositeResolver.Create` is the chain builder and the seam where the Thinktecture bridge is stacked ahead of the standard fallback.

`StandardResolver.Instance` and `.Options` expose the built-in, attribute, and dynamic-object chain. `ContractlessStandardResolver` serializes POCOs without `[MessagePackObject]` through property-name keys. Typeless resolvers embed CLR type names, and `TypelessContractlessStandardResolver.Options` is the `DefaultOptions` default. Native resolvers retain platform-native primitive encodings.

| [INDEX] | [SYMBOL]                               | [PACKAGE_ROLE]   | [CAPABILITY]          |
| :-----: | :------------------------------------- | :--------------- | :-------------------- |
|  [01]   | `StandardResolver`                     | default resolver | built-in chain        |
|  [02]   | `ContractlessStandardResolver`         | contractless     | property-name keys    |
|  [03]   | `CompositeResolver`                    | chain builder    | `Create` overloads    |
|  [04]   | `BuiltinResolver`                      | leaf resolver    | primitives            |
|  [05]   | `PrimitiveObjectResolver`              | leaf resolver    | `object` graph        |
|  [06]   | `DynamicObjectResolver`                | runtime IL emit  | formatter generation  |
|  [07]   | `…AllowPrivate`                        | runtime IL emit  | private-member access |
|  [08]   | `DynamicEnumResolver`                  | enum resolver    | numeric encoding      |
|  [09]   | `DynamicEnumAsStringResolver`          | enum resolver    | string encoding       |
|  [10]   | `TypelessObjectResolver`               | typeless         | CLR type names        |
|  [11]   | `TypelessContractlessStandardResolver` | typeless         | `DefaultOptions`      |
|  [12]   | `NativeGuidResolver`                   | native           | `Guid` encoding       |
|  [13]   | `NativeDateTimeResolver`               | native           | `DateTime` encoding   |
|  [14]   | `NativeDecimalResolver`                | native           | `decimal` encoding    |

[PUBLIC_TYPE_SCOPE]: modeling attributes — `MessagePack` (`MessagePack.Annotations`)

- rail: appearance-interchange

`MessagePackObjectAttribute(bool keyAsPropertyName = false)` exposes `AllowPrivate` and `SuppressSourceGeneration`. `KeyAttribute` accepts compact positional `int` keys or map-form `string` keys. `UnionAttribute` accepts `(int key, Type subType)` or `(int key, string subType)` for base-type or interface dispatch. `MessagePackFormatterAttribute(Type formatterType, params object?[]? arguments)` binds an `IMessagePackFormatter<T>` to a type or member. `SerializationConstructorAttribute` selects the deserialization constructor for immutable records. `GeneratedMessagePackResolverAttribute` marks the `[GeneratedMessagePackResolver]` partial-class resolver, which is AOT-safe and exposes `UseMapMode`.

| [INDEX] | [SYMBOL]                                | [KIND]            | [CAPABILITY]            |
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

[ENTRYPOINT_SCOPE]: serialize / deserialize — `MessagePackSerializer`

- rail: appearance-interchange
- The generic `<T>` family is the default; every form takes an optional `MessagePackSerializerOptions`. The `ref MessagePackWriter`/`ref MessagePackReader` and `IBufferWriter<byte>`/`ReadOnlySequence<byte>` overloads are the zero-copy path off a pooled buffer — never a `byte[]` round-trip when a sequence is in hand.

`DefaultOptions` is the process default and equals `TypelessContractlessStandardResolver.Options`.

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [CAPABILITY]               |
| :-----: | :----------------------------------------------------------------------- | :------------- | :------------------------- |
|  [01]   | `Serialize<T>(T value, options?, ct?)` → `byte[]`                        | serialize      | fresh array                |
|  [02]   | `Serialize<T>(IBufferWriter<byte>, T, options?, ct?)`                    | serialize      | pooled-buffer write        |
|  [03]   | `Serialize<T>(ref MessagePackWriter, T, options?)`                       | serialize      | caller-writer composition  |
|  [04]   | `Serialize<T>(Stream, T, options?, ct?)`                                 | serialize      | synchronous stream write   |
|  [05]   | `SerializeAsync<T>(Stream, T, options?, ct?)` → `Task`                   | serialize      | asynchronous stream write  |
|  [06]   | `Deserialize<T>(ReadOnlyMemory<byte>, options?, ct?)` → `T`              | deserialize    | memory-block read          |
|  [07]   | `Deserialize<T>(in ReadOnlySequence<byte>, options?, ct?)` → `T`         | deserialize    | multi-segment zero-copy    |
|  [08]   | `Deserialize<T>(ref MessagePackReader, options?)` → `T`                  | deserialize    | caller-reader composition  |
|  [09]   | `Deserialize<T>(ReadOnlyMemory<byte>, options?, out int bytesRead, ct?)` | deserialize    | framed bytes-consumed read |
|  [10]   | `Deserialize<T>(Stream, options?, ct?)` → `T`                            | deserialize    | synchronous stream read    |
|  [11]   | `DeserializeAsync<T>(Stream, …)` → `ValueTask<T>`                        | deserialize    | asynchronous stream read   |
|  [12]   | `DefaultOptions { get; set; }`                                           | global config  | process default            |

[ENTRYPOINT_SCOPE]: JSON bridge + typeless — `MessagePackSerializer`

- rail: appearance-interchange

`ConvertToJson` and `SerializeToJson` return `string`, while `ConvertFromJson` returns `byte[]`. The typeless pair embeds type data for heterogeneous `object` graphs, and the runtime-`Type` pair mirrors the generic family.

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY] | [CAPABILITY]                 |
| :-----: | :----------------------------------------------------------- | :------------- | :--------------------------- |
|  [01]   | `ConvertToJson(ReadOnlyMemory<byte>, options?, ct?)`         | diagnostics    | bytes to JSON text           |
|  [02]   | `ConvertToJson(ref MessagePackReader, TextWriter, options?)` | diagnostics    | streamed JSON projection     |
|  [03]   | `ConvertFromJson(string, options?, ct?)`                     | bridge         | JSON text to bytes           |
|  [04]   | `SerializeToJson<T>(T, options?, ct?)`                       | diagnostics    | model to JSON text           |
|  [05]   | `MessagePackSerializer.Typeless.Serialize`                   | typeless       | type-embedded serialization  |
|  [06]   | `MessagePackSerializer.Typeless.Deserialize`                 | typeless       | heterogeneous object graph   |
|  [07]   | `Serialize(Type, object?, options?, ct?)`                    | non-generic    | runtime-type serialization   |
|  [08]   | `Deserialize(Type, …)`                                       | non-generic    | runtime-type deserialization |

[ENTRYPOINT_SCOPE]: options builder — `MessagePackSerializerOptions`

- rail: appearance-interchange
- The options object is IMMUTABLE; each `With*` returns a new instance. `Standard` is the base; layer the resolver, compression, and security in one fluent chain.

`WithOldSpec`, `WithOmitAssemblyVersion`, and `WithAllowAssemblyVersionMismatch` own spec and typeless-metadata interoperability.

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [CAPABILITY]                |
| :-----: | :---------------------------------------- | :------------- | :-------------------------- |
|  [01]   | `MessagePackSerializerOptions.Standard`   | base           | standard-resolver options   |
|  [02]   | `WithResolver(IFormatterResolver)`        | builder        | resolution-chain swap       |
|  [03]   | `WithCompression(MessagePackCompression)` | builder        | LZ4 payload compression     |
|  [04]   | `WithCompressionMinLength(int)`           | builder        | threshold with default `64` |
|  [05]   | `WithSecurity(MessagePackSecurity)`       | builder        | untrusted-input policy      |
|  [06]   | `WithPool(SequencePool)`                  | builder        | custom buffer pool          |
|  [07]   | `WithOldSpec(bool?)`                      | builder        | interop knob                |
|  [08]   | `WithOmitAssemblyVersion(bool)`           | builder        | typeless metadata           |
|  [09]   | `WithAllowAssemblyVersionMismatch(bool)`  | builder        | typeless metadata           |

[ENTRYPOINT_SCOPE]: resolver composition + hardening — `Resolvers` / `MessagePackSecurity`

- rail: appearance-interchange

`CompositeResolver.Create` accepts resolver params or paired `IReadOnlyList<IMessagePackFormatter>` and `IReadOnlyList<IFormatterResolver>` inputs; custom formatters precede resolver fallbacks. `IFormatterResolver.GetFormatter<T>()` selects the formatter, and `IMessagePackFormatter<T>.Serialize` and `.Deserialize` own the custom codec for a non-attributed type.

| [INDEX] | [SURFACE]                                         | [ROLE]       | [CAPABILITY]                 |
| :-----: | :------------------------------------------------ | :----------- | :--------------------------- |
|  [01]   | `CompositeResolver.Create(resolvers)`             | compose      | priority resolver chain      |
|  [02]   | `CompositeResolver.Create(formatters, resolvers)` | compose      | formatter-first chain        |
|  [03]   | `StandardResolver.Instance`                       | leaf         | default fallback             |
|  [04]   | `StandardResolver.Options`                        | leaf         | pre-built options            |
|  [05]   | `MessagePackSecurity.UntrustedData`               | policy       | hardened base policy         |
|  [06]   | `MessagePackSecurity.TrustedData`                 | policy       | trusted base policy          |
|  [07]   | `WithMaximumObjectGraphDepth(int)`                | policy       | depth cap with default `500` |
|  [08]   | `WithMaximumDecompressedSize(int)`                | policy       | LZ4 decompression cap        |
|  [09]   | `WithHashCollisionResistant(bool)`                | policy       | resistant map comparers      |
|  [10]   | `IFormatterResolver.GetFormatter<T>()`            | custom codec | formatter lookup             |
|  [11]   | `IMessagePackFormatter<T>.Serialize`              | custom codec | custom serialization         |
|  [12]   | `IMessagePackFormatter<T>.Deserialize`            | custom codec | custom deserialization       |

## [04]-[IMPLEMENTATION_LAW]

[WIRE_PROFILE]:

- facade: `MessagePackSerializer` is the ONE entry; the generic `<T>` family is the default, `Type`-keyed and `Typeless` mirrors only at a heterogeneous boundary.
- options: `MessagePackSerializerOptions` is immutable — build ONE options instance per wire profile (resolver + compression + security) and reuse it; never construct options per call.
- compression: `MessagePackCompression.Lz4BlockArray` is the streaming-friendly array form (preferred for large graphs); `Lz4Block` is the single-block form. `CompressionMinLength` (default 64) gates small payloads out of compression.
- zero-copy: deserialize off a multi-segment `ReadOnlySequence<byte>` and serialize into an `IBufferWriter<byte>`; the `ref MessagePackReader`/`ref MessagePackWriter` overloads compose a custom `IMessagePackFormatter<T>` into the parent graph without a `byte[]` round-trip.

[THINKTECTURE_BRIDGE]:

- the decisive integration: `Thinktecture.Runtime.Extensions.MessagePack` ships `ThinktectureMessageFormatterResolver` (an `IFormatterResolver`) plus the `ThinktectureMessagePackFormatter`/`ThinktectureStructMessagePackFormatter` codecs that serialize Thinktecture `[SmartEnum]`/`[ValueObject]`/`[Union]` types by their key value.
- compose it AHEAD of the standard fallback: `CompositeResolver.Create(ThinktectureMessageFormatterResolver.Instance, StandardResolver.Instance)`, then `MessagePackSerializerOptions.Standard.WithResolver(that).WithCompression(MessagePackCompression.Lz4BlockArray).WithSecurity(MessagePackSecurity.UntrustedData)`. The folder's material/appearance identity value-objects then serialize with no hand-written formatter — the bridge resolver is the seam, `CompositeResolver` the chain.
- the bridge resolver serializes the generated identity types; the folder's DIRECT `MessagePack` reference is earned by the `interchange#MATERIAL_WIRE` design composing `MessagePackSerializer`, the `[MessagePackObject]`/`[Key]` modeling, and the `[GeneratedMessagePackResolver]` source-gen resolver beyond the bridge-resolved value-objects.

[MODELING_AND_AOT]:

- model appearance/material records with `[MessagePackObject]` + `[Key(int)]` (positional, the compact array form) and `[SerializationConstructor]` for immutable record constructors; reserve `[Key(string)]` (map form) for forward-compatible schemas.
- the polymorphic `BsdfLobe`/appearance hierarchy serializes through `[Union(key, subType)]` on the base type/interface — the MessagePack discriminated-union dispatch that mirrors the Thinktecture `[Union]` lobe family; each arm gets a stable integer key.
- prefer the SOURCE-GENERATED resolver: a `[GeneratedMessagePackResolver]` partial class is filled by the MessagePack source generator at compile time (AOT-safe, IL-emit-free), the host-neutral posture's resolver — NOT the runtime-reflection `DynamicObjectResolver`. The `MessagePackAnalyzer` (pinned beside the engine) enforces `[Key]` coverage and union completeness at compile time.

[SECURITY_AND_FAILURE]:

- any external/untrusted wire applies `MessagePackSecurity.UntrustedData` (depth cap 500, decompression-size cap, collision-resistant comparers) via `WithSecurity`; the default `TrustedData` is for in-process self-produced bytes only.
- failure is the typed `MessagePackSerializationException` / `FormatterNotRegisteredException` rail; lower it into the folder's `LanguageExt` `Fin`/`Validation` boundary rather than letting it escape.

[LOCAL_ADMISSION]:

- MessagePack owns the COMPACT BINARY wire only; the human-readable JSON wire is the `Thinktecture.Runtime.Extensions.Json` + `UnitsNet.Serialization.JsonNet` peer leg. `ConvertToJson`/`SerializeToJson` are DIAGNOSTIC bridges (debug/inspection), not the JSON system of record.
- this is the neuecc `MessagePack-CSharp` engine; `Nerdbank.MessagePack` (PolyType-based, in the bridge closure via StreamJsonRpc) is a DIFFERENT package — never mix their attributes, resolvers, or formatters.
- build ONE shared `MessagePackSerializerOptions` for the appearance-interchange profile; resolver/compression/security are wire-profile policy, not per-call arguments.
- `UnitsNet` quantities and `Wacton.Unicolour` colors serialize as member values through the standard resolver or a small `IMessagePackFormatter<T>`; do not re-mint a quantity/color codec the standard chain already covers.

[RAIL_LAW]:

- Package: `MessagePack` (MessagePack-CSharp / neuecc)
- Owns: the compact binary appearance/material interchange wire — `MessagePackSerializer` over `[MessagePackObject]`/`[Key]`/`[Union]`-modelled records, resolver composition, LZ4 compression, and untrusted-input hardening
- Accept: the generic `MessagePackSerializer.Serialize/Deserialize<T>` family (byte[]/`Stream`/`IBufferWriter`/`ReadOnlySequence`/`ref Reader`/`ref Writer` + async); one shared immutable `MessagePackSerializerOptions`; `CompositeResolver.Create(ThinktectureMessageFormatterResolver.Instance, StandardResolver.Instance)` for the Thinktecture identity bridge; `[GeneratedMessagePackResolver]` AOT source-gen; `MessagePackSecurity.UntrustedData` on external wire; the typed `MessagePackSerializationException` rail
- Reject: conflation with `Nerdbank.MessagePack`; the runtime-reflection `DynamicObjectResolver` as the host-neutral/AOT path (use source-gen); per-call options construction; `ConvertToJson` as the JSON system of record (it is a diagnostic bridge — JSON is the Thinktecture.Json/UnitsNet.JsonNet leg); `TrustedData` on external input; a hand-rolled quantity/color codec the standard resolver already provides

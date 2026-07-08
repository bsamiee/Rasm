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

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:----------------------------- |:------------------ |:---------------------------------------------------------------------- |
| [01] | `MessagePackSerializer` | static facade | `Serialize`/`Deserialize` over byte[]/`Stream`/buffer/sequence + async + JSON bridge |
| [02] | `MessagePackSerializerOptions` | immutable options | `Standard` base + `With*` resolver/compression/security/pool builder |
| [03] | `MessagePackReader` | zero-copy reader | `ref struct` over `ReadOnlySequence<byte>`; typed read + skip + peek |
| [04] | `MessagePackWriter` | zero-copy writer | `ref struct` over `IBufferWriter<byte>`; typed write + headers + raw |
| [05] | `MessagePackCompression` | compression enum | `None` / `Lz4Block` / `Lz4BlockArray` |
| [06] | `MessagePackSecurity` | hardening policy | `TrustedData`/`UntrustedData`; depth + decompression caps |
| [07] | `MessagePackStreamReader` | framed stream reader | reads consecutive top-level MessagePack messages off a `Stream` |
| [08] | `MessagePackType` / `MessagePackCode` | wire vocabulary | the MessagePack format-family enum and byte-code table |
| [09] | `ExtensionHeader` / `ExtensionResult` | extension type | the `ext` (type-code + payload) carrier for custom binary embeddings |
| [10] | `Nil` | nil value | the MessagePack `nil` singleton (`Nil.Default`) |
| [11] | `MessagePackSerializationException` / `FormatterNotRegisteredException` | error rail | serialization failure + missing-formatter failure |

[PUBLIC_TYPE_SCOPE]: formatter + resolver contracts — `MessagePack` / `MessagePack.Formatters`
- rail: appearance-interchange

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:----------------------------- |:----------------- |:---------------------------------------------------------------------- |
| [01] | `IFormatterResolver` | resolver contract | `IMessagePackFormatter<T>? GetFormatter<T>()` — the type→formatter lookup |
| [02] | `IMessagePackFormatter<T>` | codec contract | `Serialize(ref Writer, T, options)` / `Deserialize(ref Reader, options)` |
| [03] | `IMessagePackSerializationCallbackReceiver` | lifecycle hook | `OnBeforeSerialize`/`OnAfterDeserialize` on the serialized type |

[PUBLIC_TYPE_SCOPE]: resolver catalog — `MessagePack.Resolvers`
- rail: appearance-interchange
- Resolvers are composed in priority order; `CompositeResolver.Create` is the chain builder and the seam where the Thinktecture bridge is stacked ahead of the standard fallback.

| [INDEX] | [SYMBOL] | [PACKAGE_ROLE] | [CAPABILITY] |
|:-----: |:------------------------------------ |:------------------ |:------------------------------------------------------------------ |
| [01] | `StandardResolver` | default resolver | built-in + attribute + dynamic-object chain (`Instance`, `Options`) |
| [02] | `ContractlessStandardResolver` | contractless | serializes POCOs with no `[MessagePackObject]` (property-name keys) |
| [03] | `CompositeResolver` | chain builder | `Create(params IFormatterResolver[])` / `Create(formatters, resolvers)` |
| [04] | `BuiltinResolver` / `PrimitiveObjectResolver` | leaf resolvers | primitives and the `object`-graph fallback |
| [05] | `DynamicObjectResolver` / `…AllowPrivate` | runtime IL emit | reflection-emit formatter generation (the non-AOT path) |
| [06] | `DynamicEnumResolver` / `DynamicEnumAsStringResolver` | enum resolvers | numeric vs string enum encoding |
| [07] | `TypelessObjectResolver` / `TypelessContractlessStandardResolver` | typeless | embeds the CLR type name (the `DefaultOptions` default) |
| [08] | `NativeGuidResolver` / `NativeDateTimeResolver` / `NativeDecimalResolver` | native | platform-native primitive encodings |

[PUBLIC_TYPE_SCOPE]: modeling attributes — `MessagePack` (`MessagePack.Annotations`)
- rail: appearance-interchange

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------ |:-------------- |:------------------------------------------------------------------ |
| [01] | `MessagePackObjectAttribute` | type marker | `(bool keyAsPropertyName=false)` + `AllowPrivate`/`SuppressSourceGeneration` |
| [02] | `KeyAttribute` | member key | `(int)` positional array key (compact) or `(string)` map key |
| [03] | `IgnoreMemberAttribute` | member skip | excludes a member from serialization |
| [04] | `UnionAttribute` | polymorphism | `(int key, Type subType)` / `(int key, string subType)` — discriminated-union dispatch over a base type/interface |
| [05] | `MessagePackFormatterAttribute` | custom codec | `(Type formatterType, params object?[]? arguments)` — bind an `IMessagePackFormatter<T>` to a type/member |
| [06] | `SerializationConstructorAttribute` | ctor selector | picks the deserialization constructor (immutable record support) |
| [07] | `GeneratedMessagePackResolverAttribute` | source-gen marker | `[GeneratedMessagePackResolver]` partial-class resolver (AOT; `UseMapMode`) |
| [08] | `CompositeResolverAttribute` | source-gen marker | source-generated composite resolver declaration |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: serialize / deserialize — `MessagePackSerializer`
- rail: appearance-interchange
- The generic `<T>` family is the default; every form takes an optional `MessagePackSerializerOptions`. The `ref MessagePackWriter`/`ref MessagePackReader` and `IBufferWriter<byte>`/`ReadOnlySequence<byte>` overloads are the zero-copy path off a pooled buffer — never a `byte[]` round-trip when a sequence is in hand.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:----------------------------------------------------------------- |:------------- |:----------------------------------------------------- |
| [01] | `Serialize<T>(T value, options?, ct?)` → `byte[]` | serialize | allocating digest to a fresh array |
| [02] | `Serialize<T>(IBufferWriter<byte>, T, options?, ct?)` | serialize | zero-copy write into a pooled buffer writer |
| [03] | `Serialize<T>(ref MessagePackWriter, T, options?)` | serialize | direct write into a caller's writer (composite codecs) |
| [04] | `Serialize<T>(Stream, T, options?, ct?)` | serialize | synchronous stream write |
| [05] | `SerializeAsync<T>(Stream, T, options?, ct?)` → `Task` | serialize | async stream write |
| [06] | `Deserialize<T>(ReadOnlyMemory<byte>, options?, ct?)` → `T` | deserialize | from a memory block |
| [07] | `Deserialize<T>(in ReadOnlySequence<byte>, options?, ct?)` → `T` | deserialize | from a multi-segment pooled sequence (zero-copy) |
| [08] | `Deserialize<T>(ref MessagePackReader, options?)` → `T` | deserialize | direct read from a caller's reader (composite codecs) |
| [09] | `Deserialize<T>(ReadOnlyMemory<byte>, options?, out int bytesRead, ct?)` | deserialize | reports bytes consumed (framed streams) |
| [10] | `Deserialize<T>(Stream, options?, ct?)` / `DeserializeAsync<T>(Stream, …)` → `T`/`ValueTask<T>` | deserialize | sync / async stream read |
| [11] | `DefaultOptions { get; set; }` | global config | process default (= `TypelessContractlessStandardResolver.Options`) |

[ENTRYPOINT_SCOPE]: JSON bridge + typeless — `MessagePackSerializer`
- rail: appearance-interchange

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------------------------------- |:------------- |:----------------------------------------------------- |
| [01] | `ConvertToJson(ReadOnlyMemory<byte>, options?, ct?)` → `string` | diagnostics | MessagePack bytes → JSON text (debug/inspection) |
| [02] | `ConvertToJson(ref MessagePackReader, TextWriter, options?)` | diagnostics | streamed MessagePack → JSON |
| [03] | `ConvertFromJson(string, options?, ct?)` → `byte[]` | bridge | JSON text → MessagePack bytes |
| [04] | `SerializeToJson<T>(T, options?, ct?)` → `string` | diagnostics | value → JSON (via MessagePack model) |
| [05] | `MessagePackSerializer.Typeless.{Serialize,Deserialize}` | typeless | type-embedded serialize for heterogeneous `object` graphs |
| [06] | `Serialize(Type, object?, options?, ct?)` / `Deserialize(Type, …)` | non-generic | runtime-`Type`-keyed mirror of the generic family |

[ENTRYPOINT_SCOPE]: options builder — `MessagePackSerializerOptions`
- rail: appearance-interchange
- The options object is IMMUTABLE; each `With*` returns a new instance. `Standard` is the base; layer the resolver, compression, and security in one fluent chain.

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `MessagePackSerializerOptions.Standard` | base | the standard-resolver default options |
| [02] | `WithResolver(IFormatterResolver)` | builder | swap the formatter-resolution chain (the bridge seam) |
| [03] | `WithCompression(MessagePackCompression)` | builder | enable `Lz4Block` / `Lz4BlockArray` payload compression |
| [04] | `WithCompressionMinLength(int)` | builder | min payload size before compression engages (default 64) |
| [05] | `WithSecurity(MessagePackSecurity)` | builder | apply the untrusted-input hardening policy |
| [06] | `WithPool(SequencePool)` | builder | inject a custom buffer pool |
| [07] | `WithOldSpec(bool?)` / `WithOmitAssemblyVersion(bool)` / `WithAllowAssemblyVersionMismatch(bool)` | builder | spec/typeless-metadata interop knobs |

[ENTRYPOINT_SCOPE]: resolver composition + hardening — `Resolvers` / `MessagePackSecurity`
- rail: appearance-interchange

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CAPABILITY] |
|:-----: |:---------------------------------------------------------------- |:------------- |:----------------------------------------------------------- |
| [01] | `CompositeResolver.Create(params IFormatterResolver[])` | compose | priority chain of resolvers |
| [02] | `CompositeResolver.Create(IReadOnlyList<IMessagePackFormatter>, IReadOnlyList<IFormatterResolver>)` | compose | custom formatters first, then resolver fallbacks |
| [03] | `StandardResolver.Instance` / `.Options` | leaf | the default fallback resolver and its pre-built options |
| [04] | `MessagePackSecurity.UntrustedData` / `.TrustedData` | policy | the hardened vs trusted base policy |
| [05] | `WithMaximumObjectGraphDepth(int)` | policy | recursion-depth cap (default 500) |
| [06] | `WithMaximumDecompressedSize(int)` / `WithHashCollisionResistant(bool)` | policy | LZ4 bomb cap + collision-resistant map comparers |
| [07] | `IFormatterResolver.GetFormatter<T>()` / `IMessagePackFormatter<T>.{Serialize,Deserialize}` | custom codec | hand-written formatter for a non-attributed type |

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

# [RASM_MATERIALS_API_MESSAGEPACK]

`Rasm.Persistence` owns the `MessagePack` codec surface for this branch at `libs/csharp/Rasm.Persistence/.api/api-messagepack.md` — the serializer root, the `ref struct` reader and writer tokens, the immutable options profile, the security ceilings, the resolver families, and the contract and generator attributes — so Materials registers that surface rather than re-tabling it. This partition holds the appearance-interchange PROFILE alone: the source-generated, IL-emit-free resolver chain the Materials appearance and material model serializes through, and the domain formatter stacking that chain composes. It is the binary peer of the human-readable JSON leg; `ConvertToJson` stays a diagnostic bridge, never the JSON system of record.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: Materials appearance-wire partition of `MessagePack`
- package: `MessagePack` (MIT, direct `PackageReference` beside `MessagePackAnalyzer`)
- assembly: `MessagePack` (engine) + `MessagePack.Annotations` (attribute markers, pure-managed)
- namespace: `MessagePack`, `MessagePack.Resolvers`, `MessagePack.Formatters`
- asset: multi-target `net9.0`/`net8.0`/`netstandard2.1`/`netstandard2.0`/`net472`; the `net10.0` consumer binds `lib/net9.0/MessagePack.dll`, `MessagePack.Annotations` binds `lib/netstandard2.0`
- rail: appearance-interchange (binary wire)

- Registers the codec surface(`libs/csharp/Rasm.Persistence/.api/api-messagepack.md`): `MessagePackSerializer` and its `Typeless` root, `MessagePackReader`/`MessagePackWriter`/`MessagePackStreamReader`, `MessagePackSerializerOptions` with every `With*` mutator, `MessagePackSecurity` and its presets, `MessagePackCompression`, the resolver families (contract, composition, generated, encoding, reflection-emit), `IMessagePackSerializationCallbackReceiver`, the contract and generator attributes, and the `MessagePackSerializationException`/`FormatterNotRegisteredException` rail all resolve there — a member verified against that catalogue is verified for this profile, and re-tabling one here forks the branch's codec truth.

## [02]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- One immutable `MessagePackSerializerOptions` carries the whole appearance-interchange profile — resolver chain, `Lz4BlockArray` compression, and `MessagePackSecurity.UntrustedData` on external wire — reused across every call; per-call construction and a mutated `MessagePackSerializer.DefaultOptions` are both the deleted form.
- Zero-copy is the path: deserialize off a multi-segment `ReadOnlySequence<byte>`, serialize into an `IBufferWriter<byte>`, and the `ref MessagePackReader`/`ref MessagePackWriter` overloads fold a domain `IMessagePackFormatter<T>` into the parent graph with no `byte[]` round-trip.
- Records model through `[MessagePackObject]` + `[Key(int)]` (compact positional array) with `[SerializationConstructor]` selecting the immutable ctor; `[Key(string)]` (map form) is the forward-compatible schema. `[Union(int key, Type subType)]` on the base type dispatches the polymorphic `BsdfLobe`/appearance hierarchy, each arm a stable integer key.
- `[GeneratedMessagePackResolver]` source-generates the host-neutral resolver partial (AOT-safe, IL-emit-free, exposing `UseMapMode`), never a runtime reflection-emit resolver; `MessagePackAnalyzer` enforces `[Key]` coverage and union completeness at compile time.
- The typed `MessagePackSerializationException`/`FormatterNotRegisteredException` rail lowers into the folder's `LanguageExt` `Fin`/`Validation` boundary at the codec edge.

[STACKING]:
- `Thinktecture.Runtime.Extensions.MessagePack`(`.api/api-thinktecture-messagepack.md`): `ThinktectureMessageFormatterResolver.Instance` composes ahead of `StandardResolver.Instance` through `CompositeResolver.Create(...)`, serializing the folder's `[SmartEnum]`/`[ValueObject]`/`[Union]` identity types by their key value with no hand-written formatter.
- `api-messagepack`(`libs/csharp/Rasm.Persistence/.api/api-messagepack.md`): the peer partition of the same distribution carries the snapshot axis — the attribute-declared wire types, the framed `MessagePackStreamReader` ingest, and the content-identity encoding — so the two profiles differ in resolver chain and security preset alone and never in codec spelling.
- within-lib: the `interchange` `WireCodec` composes the serializer and the `[MessagePackObject]`/`[Key]`/`[GeneratedMessagePackResolver]` modeling beyond the bridge-resolved value objects, under that one shared profile.

[LOCAL_ADMISSION]:
- `MessagePack` owns the compact binary wire only; the human-readable JSON peer is `Thinktecture.Runtime.Extensions.Json` + `UnitsNet.Serialization.JsonNet`, and `ConvertToJson`/`SerializeToJson` are diagnostic bridges, not the JSON system of record.
- This is the neuecc `MessagePack-CSharp` engine; the PolyType-based `Nerdbank.MessagePack` is a different package — its attributes, resolvers, and formatters never mix with this one's.
- `UnitsNet` quantities and `Wacton.Unicolour` colors serialize as member values through the standard resolver or a small `IMessagePackFormatter<T>`, never a re-minted quantity or color codec the standard chain already covers.

[RAIL_LAW]:
- Package: `MessagePack` (MessagePack-CSharp / neuecc)
- Owns: the compact binary appearance and material interchange PROFILE — the source-generated resolver chain, the `[MessagePackObject]`/`[Key]`/`[Union]` modeling of the appearance hierarchy, the LZ4 posture, and the untrusted-input hardening
- Accept: one shared immutable options value, `CompositeResolver.Create(ThinktectureMessageFormatterResolver.Instance, StandardResolver.Instance)`, `[GeneratedMessagePackResolver]` source-gen, `MessagePackSecurity.UntrustedData` on external wire, and the typed exception rail
- Reject: a member roster for the package here, a runtime reflection-emit resolver as the AOT path, per-call options construction, `ConvertToJson` as the JSON system of record, `TrustedData` on external input, and a hand-rolled quantity or color codec the standard resolver provides

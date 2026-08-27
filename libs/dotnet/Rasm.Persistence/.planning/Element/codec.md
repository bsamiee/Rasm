# [PERSISTENCE_ELEMENT_CODEC]

Rasm.Persistence encodes every durable `ElementGraph`, `GraphDelta`, and geometry blob through one `SnapshotCodec` axis paired with the `CompressionPolicy` and `HashPolicy` axes, derives the one `ContentAddress` by composing the kernel `Rasm.Domain` `ContentHash.Of` seed-zero entry over PLAINTEXT canonical bytes, seals every file-resident artifact under one fixed-offset `SnapshotHeader` that is the artifact's entire trust boundary, verifies every read through one ordered `[FaultCase]` ladder that rejects before any decoder with attack surface runs, and splits opaque payload bytes into FastCDC content-defined chunks for cross-snapshot dedup. Every codec, compression, and seal transform returns a typed `Fin` — the transform family raises no exception, so a malformed shape, a smuggled CBOR suffix, an unresolvable dictionary era, and an over-ceiling stored length all reach the caller as `CodecFault` cases on the `Fin` the write fold already runs.

`SnapshotHeader` carries TWO digests because they answer two questions: `ContentHash` addresses the PLAINTEXT (the catalog key, per RULINGS `[02]` "Content keys cover PLAINTEXT") and `StoredDigest` proves the STORED bytes intact (the integrity word `Verify` checks before any decoder binds, which must read the bytes on disk). One field serving both keyed the artifact by its framing, so re-packing unchanged content under a newer `CompressionPolicy` row minted a fresh address, breaking chunk dedup and chaining `Lineage` through an edit that never happened. The header also records the observed seal alone — compression as applied, both plain and stored length, the schema fingerprint, the retention epoch, and its own checksum — so a torn or foreign artifact self-rejects by offset before the codec machinery binds.

Codec, compression, and hash variance are delegate rows on string-keyed smart enums, and codec binding is fixed at write: the canonical CBOR row's deterministic map-key order makes bytes content-stable for the `ContentAddress` a schemaless MessagePack body cannot guarantee, the MessagePack row is the Marten-event and cache wire, JSON-STJ the inspector/web wire, file-raw the geometry-blob passthrough. `ContentAddress` (the upstream `Projection/address#CONTENT_ADDRESS` `[ValueObject<UInt128>]` hasher over the `Projection/address#IMPLEMENTATION_LAW` codec), `ElementGraph`, `GraphDelta`, and `Node` arrive settled from `Rasm.Element`; `ContentHash.Of` from the `Rasm` kernel; the clock, correlation, tenant, and instrument slots ride the Persistence-owned `Element/graph#STORE_HOOKS` `ProjectionContext` frame the composition root fills; `DataClassification` stays a wire input at the boundary.

## [01]-[INDEX]

- [02]-[CODEC_AXIS]: five codec rows, the package wire context, generated converter admission, the `GeoJsonProjection` dual service, and the AOT resolver landmark.
- [03]-[CONTENT_ADDRESS]: `ContentAddress` composed off the kernel seed-zero entry, the canonical-byte projection, the precomputed-digest wrap, the upstream `GraphMembers` incremental accumulator, and the streaming digest leg.
- [04]-[COMPRESSION_HASHING]: compression rows, the two-row hash axis, framing routes, and identity values.
- [05]-[SNAPSHOT_SPINE]: fixed-offset header trust boundary, the tier rejection ladder, the single-pass atomic write fold, and the orphan sweep.
- [06]-[CONTENT_CHUNKING]: FastCDC content-defined chunk boundaries and per-chunk content-key dedup.

## [02]-[CODEC_AXIS]

- Owner: `SnapshotCodec` `[SmartEnum<string>]` under the `ComparerAccessors.StringOrdinal` accessor; `ElementJson` the package `JsonSerializerContext` partial joining the suite STJ merge; `InstantFormatter` the one primitive-mapped NodaTime MessagePack formatter; `WireSurface` the wire-surface vocabulary each codec admits through its frozen `Membership` set, so content negotiation is the codec rows a surface admits, never a parallel format enum; `GeoJsonProjection` the one `GeoJsonConverterFactory` admission; `PersistenceResolver` the AOT MessagePack resolver landmark.
- Cases: codec rows `json-stj`/`messagepack`/`file-raw`/`cbor`/`proto-binary`; wire surfaces `snapshot`/`cache`/`sync`/`web`.
- Entry: `Serialize(Type, object?)` and `Deserialize(Type, ReadOnlyMemory<byte>)` are the row transforms, both on `Fin` because the codec takes an UNTYPED `object?`/byte window at a boundary its `Admits` predicate gates only when a caller consulted it — a `throw` inside a row delegate escapes the one `Fin` every consumer of this page already runs; `Negotiate(WireSurface, Type, Seq<string>)` filters by surface and verified shape admission before ranking accepted rows, so attribute-free domain graphs cannot enter MessagePack through negotiation.
- Auto: registering `ThinktectureJsonConverterFactory` and `ThinktectureMessageFormatterResolver.Instance` once derives every `[ValueObject]`/`[SmartEnum]`/`[Union]` converter and formatter, so a `NodeId`/`ContentAddress`/`Discipline` crosses both the Marten-event STJ wire and the MessagePack cache wire as its bare key with zero hand codec; `GeoJsonProjection` admits one `GeoJsonConverterFactory` deriving the GeoJSON projection of every `NetTopologySuite` geometry, feature, and attribute table the `Coverage`/`GeoReference` nodes carry; `Marten.UseSystemTextJsonForSerialization(ElementJson.Options)` binds the `json-stj` options as the event-store serializer so a stored `GraphEvent` and an inspector projection share one converter set; registering the kernel `LanguageExtJsonConverterFactory` (`Rasm/Domain/results#CARRIER_CODEC`) once gives every `Seq`/`Set`/`Option`/`HashMap` member both legs — LanguageExt ships no STJ support, so without it the read leg fails on every carrier member while the write leg succeeds.
- Packages: Rasm (`Rasm.Domain` `LanguageExtJsonConverterFactory` — the kernel carrier codec), MessagePack, MessagePackAnalyzer, System.Formats.Cbor, Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, Thinktecture.Runtime.Extensions.MessagePack, NetTopologySuite.IO.GeoJSON4STJ, NodaTime, NodaTime.Serialization.SystemTextJson, BCL inbox.
- Boundary: artifact-kind-to-codec binding is fixed at write — a second codec on one kind is a conflict, not a fallback; the DOMAIN graph types (`GraphDelta`/`Header`/`Node`/`Relationship` carrying LanguageExt `Seq`/`Option` and Thinktecture `[Union]`/`[SmartEnum]`/`[ValueObject]` members, with NO `[MessagePackObject]` because the domain stays library-neutral) ride the `json-stj` row ONLY — source-gen-registered on `ElementJson` (`GraphEvent`/`GraphProjection`/`GraphDelta` roots, the rest reachable transitively), whose STJ set handles `Seq`/`Option`/`[Union]`/NodaTime — because the `messagepack` row's `GeneratedMessagePackResolver` finds only `[MessagePackObject]` owners and its `StandardResolver` rejects an attribute-free `Seq<Node>`, so MessagePack on the domain graph is the deleted phantom; the `messagepack` row remains the Marten cache/sync codec for Persistence-owned positional records and pairs with the `none` compression row because `Lz4BlockArray` owns compression in-codec (double framing is the deleted pattern). The `proto-binary` row admits generated `IMessage` values on `sync` alone and carries the `Version/commits#CRDT_WIRE` payload; descriptor validation and domain admission stay at that wire owner, while this row only invokes the generated runtime. The `cbor` row is the self-describing IETF blob codec whose `CborConformanceMode.Canonical` map-key order makes the bytes content-stable for the `ContentAddress` and whose `Strict` reader over a FIXED `ReadOnlyMemory` rejects a length bomb and smuggled suffix; the `json-stj` row is the inspector/web wire and Marten event-store serializer; the `file-raw` row is the geometry-blob passthrough. Every `messagepack` decode uses `MessagePackSecurity.UntrustedData.WithMaximumObjectGraphDepth(256)`, and the `#SNAPSHOT_SPINE` `Snapshots.Verify` ladder adds header, length, schema, epoch, checksum, and content-address admission before decode; MemoryPack and protobuf SNAPSHOT encodings stay rejected — proto-binary is a sync payload codec, never a snapshot encoding.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
using Google.Protobuf;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WireSurface {
    public static readonly WireSurface Snapshot = new("snapshot");
    public static readonly WireSurface Cache = new("cache");
    public static readonly WireSurface Sync = new("sync");
    public static readonly WireSurface Web = new("web");
}

// --- [SERVICES] ------------------------------------------------------------------------
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(GraphEvent))]
[JsonSerializable(typeof(GraphProjection))]
[JsonSerializable(typeof(GraphDelta))]
[JsonSerializable(typeof(SnapshotCatalogRow))]
[JsonSerializable(typeof(SnapshotHeader))]
[JsonSerializable(typeof(ElementIdentity))]
public partial class ElementJson : JsonSerializerContext {
    public static readonly JsonSerializerOptions Options =
        new JsonSerializerOptions(JsonSerializerOptions.Strict) {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = Default,
            Converters = { new ThinktectureJsonConverterFactory(), new LanguageExtJsonConverterFactory(), GeoJsonProjection.Default.Factory },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
}

public sealed class InstantFormatter : IMessagePackFormatter<Instant> {
    public static readonly InstantFormatter Instance = new();
    public void Serialize(ref MessagePackWriter writer, Instant value, MessagePackSerializerOptions options) => writer.Write(value.ToUnixTimeTicks());
    public Instant Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) => Instant.FromUnixTimeTicks(reader.ReadInt64());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SnapshotCodec {
    public static readonly SnapshotCodec JsonStj = new("json-stj", headerId: 1, negotiationRank: 1, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot, WireSurface.Web]),
        admits: static shape => ElementJson.Default.GetTypeInfo(shape) is not null,
        serialize: static (shape, value) => Fin.Succ(JsonSerializer.SerializeToUtf8Bytes(value, shape, ElementJson.Options)),
        deserialize: static (shape, payload) => Fin.Succ(JsonSerializer.Deserialize(payload.Span, shape, ElementJson.Options)));
    public static readonly SnapshotCodec MessagePackBinary = new("messagepack", headerId: 2, negotiationRank: 3, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot, WireSurface.Cache, WireSurface.Sync, WireSurface.Web]),
        admits: static shape => shape.IsDefined(typeof(MessagePackObjectAttribute), inherit: false) || shape.IsDefined(typeof(MessagePack.UnionAttribute), inherit: false),
        serialize: static (shape, value) => Fin.Succ(MessagePackSerializer.Serialize(shape, value, Binary)),
        deserialize: static (shape, payload) => Fin.Succ(MessagePackSerializer.Deserialize(shape, payload, Binary)));
    public static readonly SnapshotCodec FileRaw = new("file-raw", headerId: 3, negotiationRank: 0, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot]),
        admits: static shape => shape == typeof(byte[]) || shape == typeof(ReadOnlyMemory<byte>),
        serialize: static (_, value) => Opaque(value).Map(static bytes => bytes.ToArray()),
        deserialize: static (_, payload) => Fin.Succ<object?>(payload.ToArray()));
    public static readonly SnapshotCodec Cbor = new("cbor", headerId: 4, negotiationRank: 2, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot, WireSurface.Sync, WireSurface.Web]),
        admits: static shape => shape == typeof(byte[]) || shape == typeof(ReadOnlyMemory<byte>),
        serialize: static (_, value) => Opaque(value).Map(CborBlob.Encode),
        deserialize: static (_, payload) => CborBlob.Decode(payload).Map(static bytes => (object?)bytes));
    public static readonly SnapshotCodec ProtoBinary = new("proto-binary", headerId: 5, negotiationRank: 4, membership: FrozenSet.ToFrozenSet([WireSurface.Sync]),
        admits: static shape => typeof(IMessage).IsAssignableFrom(shape),
        serialize: ProtoEncode,
        deserialize: ProtoDecode);

    public int HeaderId { get; }
    public int NegotiationRank { get; }
    public FrozenSet<WireSurface> Membership { get; }

    static Fin<ReadOnlyMemory<byte>> Opaque(object? value) => value switch {
        byte[] bytes => Fin.Succ<ReadOnlyMemory<byte>>(bytes),
        ReadOnlyMemory<byte> memory => Fin.Succ(memory),
        _ => Fin.Fail<ReadOnlyMemory<byte>>(new CodecFault.ShapeRefused(value?.GetType().Name ?? "null")),
    };

    static Fin<byte[]> ProtoEncode(Type shape, object? value) => value is IMessage message && shape.IsInstanceOfType(message)
        ? Fin.Succ(message.ToByteArray())
        : Fin.Fail<byte[]>(new CodecFault.ShapeRefused(shape.Name));

    static Fin<object?> ProtoDecode(Type shape, ReadOnlyMemory<byte> payload) =>
        Try.lift(() => Activator.CreateInstance(shape) is IMessage message
            ? (message.MergeFrom(payload.Span), Fin.Succ<object?>(message)).Item2
            : Fin.Fail<object?>(new CodecFault.ShapeRefused(shape.Name))).Run().Bind(static inner => inner);

    public static Fin<SnapshotCodec> ByHeaderId(int headerId) =>
        FactoryBridge.Row<int, string, SnapshotCodec>(candidate: headerId, column: static row => row.HeaderId, match: None);
    public bool Serves(WireSurface surface) => Membership.Contains(surface);

    public static Fin<SnapshotCodec> Negotiate(WireSurface surface, Type shape, Seq<string> accepted) =>
        toSeq(toSeq(Items).Filter(c => c.Serves(surface) && c.Admits(shape)).OrderByDescending(static c => c.NegotiationRank))
            .Find(c => accepted.Contains(c.Key))
            .ToFin(new CodecFault.NoMutualCodec(surface.Key));

    [UseDelegateFromConstructor] public partial bool Admits(Type shape);
    [UseDelegateFromConstructor] public partial Fin<byte[]> Serialize(Type shape, object? value);
    [UseDelegateFromConstructor] public partial Fin<object?> Deserialize(Type shape, ReadOnlyMemory<byte> payload);

    public static readonly MessagePackSerializerOptions Binary = BuildBinary(
        ThinktectureMessageFormatterResolver.Instance, GeneratedMessagePackResolver.Instance, StandardResolver.Instance);
    public static readonly MessagePackSerializerOptions Aot = BuildBinary(PersistenceResolver.Instance);

    static MessagePackSerializerOptions BuildBinary(params IFormatterResolver[] resolvers) =>
        MessagePackSerializerOptions.Standard
            .WithResolver(CompositeResolver.Create([InstantFormatter.Instance], resolvers))
            .WithSecurity(MessagePackSecurity.UntrustedData.WithMaximumObjectGraphDepth(256))
            .WithCompression(MessagePackCompression.Lz4BlockArray);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CborBlob {
    public static byte[] Encode(ReadOnlyMemory<byte> payload) {
        CborWriter writer = new(CborConformanceMode.Canonical);
        writer.WriteTag(CborTag.SelfDescribeCbor);
        writer.WriteByteString(payload.Span);
        return writer.Encode();
    }
    public static Fin<byte[]> Decode(ReadOnlyMemory<byte> payload) {
        return Try.lift(() => {
            CborReader reader = new(payload, CborConformanceMode.Strict);
            if (reader.PeekState() == CborReaderState.Tag && reader.PeekTag() == CborTag.SelfDescribeCbor) reader.ReadTag();
            byte[] bytes = reader.ReadByteString();
            return reader.BytesRemaining == 0
                ? Fin.Succ(bytes)
                : Fin.Fail<byte[]>(new CodecFault.FrameRejected($"cbor-trailing:{reader.BytesRemaining}"));
        }).Run().Bind(static inner => inner);
    }
}

public sealed record GeoJsonProjection(GeometryFactory Geometry, string IdProperty = GeoJsonConverterFactory.DefaultIdPropertyName) {
    public static readonly GeoJsonProjection Default = new(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));
    public GeoJsonConverterFactory Factory =>
        new(Geometry, writeGeometryBBox: true, IdProperty, RingOrientationOption.EnforceRfc9746, allowModifiedAttributes: false);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[GeneratedMessagePackResolver] public partial class GeneratedMessagePackResolver;
[CompositeResolver(typeof(ThinktectureMessageFormatterResolver), typeof(GeneratedMessagePackResolver))] public partial class PersistenceResolver;
```

## [03]-[CONTENT_ADDRESS]

- Owner: `ContentAddress` the upstream `Rasm.Element/Projection/address#CONTENT_ADDRESS` `[ValueObject<UInt128>]` content key every snapshot identity, dedup probe, diff, and AS-OF cut reads, COMPOSED here directly — Persistence mints NO node/graph/edge hash owner of its own: the node content key is the upstream `ContentAddress.Of(node, tolerance)` a `Version/merge#STRUCTURAL_DIFF` `GraphNode` composes inline, the EDGE key the upstream `ContentAddress.Of(edge, tolerance)` streaming arm (upstream retired `Relationship.ToCanonicalBytes`, so no materialized edge-byte array exists to hash beside it), the graph address the upstream `ContentAddress.OfGraph(graph)` a `Query/topology` memo key reads inline, and a precomputed framing/chunk/snapshot digest the generated `ContentAddress.Create(UInt128)` wraps without re-hashing — so a Persistence-local `NodeHash`/`GraphHash` forwarding owner over those one-hop upstream entries is the deleted form (the ONE byte projection and the ONE order-independent fold already live upstream, never re-spelled).
- Cases: a graph content address IS the upstream `Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph` (the semantic header folded first, then sorted node addresses, then sorted edge addresses through `ContentAddress.Of(edge, tolerance)`); a node content address is the upstream `ContentAddress.Of(node, tolerance)` over the node's `ToCanonicalBytes` projection through the kernel `CanonicalWriter` (`Rasm/Domain/identity#CONTENT_KEY`; fixed IEEE-754 LE bits with `-0.0→0.0` and `NaN→canonical`, measure quantization to the header tolerance, explicit attribute-order canon); a precomputed framing/chunk/snapshot digest wraps through generated `ContentAddress.Create(UInt128)` (no re-hash); delta keying rides `delta.Address(tolerance)` — the `Version/commits#CRDT_WIRE` delta content key the `Version/ingress` event-dedup probe reads — never a second delta hasher.
- Entry: the upstream owns every minting entry — `ContentAddress.Of(ReadOnlySpan<byte>)` hashes the framing/chunk preimage, generated `ContentAddress.Create(UInt128)` wraps a precomputed snapshot/chunk digest, `ContentAddress.Of(Node, tolerance)` is the id-INCLUSIVE graph-dedup key, `ContentAddress.Of(Relationship, tolerance)` the edge key that streams its preimage into the caller's writer rather than materializing it, `ContentAddress.OfGraph(ElementGraph)` the order-independent snapshot identity, `ContentAddress.OfGraph(GraphMembers)` that same identity off the accumulated member sets, and `ContentAddress.Verify(...)` the re-hash gate; the kernel `ContentHash.Of<TState>(TState, Action<TState, CanonicalWriter>)` streaming leg is the entry a payload no `ReadOnlySpan<byte>` spans folds through — its callback receives the kernel `CanonicalWriter`, so a chunk appends through `Raw` and a framed field through `Ordinal`/`String`/`Rows`, and NO call site touches an `XxHash128` accumulator directly — seed-zero and CHUNK-ORDER-canonical so it addresses into the same identity space the one-shot does; this page composes those entries at the snapshot catalog (`Snapshots.Write` wraps the sealed `SnapshotHeader.ContentHash` `UInt128` through `Create(UInt128)` into the `SnapshotCatalogRow`), the chunk fold (`ContentChunker.Chunk` folds its cut spans through the streaming leg and wraps through `Create(UInt128)`), and the reassembly verify, never a Persistence-local re-derivation.
- Auto: the content address is the kernel's ONE algorithm — the `Rasm.Domain` `ContentHash.Of` seed-zero `XxHash128` entry, the same digest the upstream's `ContentAddress` value-object wraps and the `Rasm` kernel mints for geometry by content-hash — so a snapshot, a chunk, a diff, and a federation key all read one 128-bit address and a second hasher is the deleted form; this page's opaque framing and chunk preimages (the sealed-bytes digest in `SnapshotHeader.Seal`, the per-chunk and whole-payload digests in `ContentChunker`) compose `ContentHash.Of` DIRECTLY — a per-call-site `XxHash128.HashToUInt128` invocation is the deleted spelling (value-identical, so the re-anchor is pure call-path collapse, never an identity re-mint) — and wrap every result through generated `ContentAddress.Create(UInt128)`, while the node/graph content keys compose the upstream's `ToCanonicalBytes`-backed entries verbatim so the float-bearing parity corpus (`Version/commits#CRDT_WIRE`) pins the layout cross-runtime; measure quantization to `Header.Tolerance` happens once inside the kernel `CanonicalWriter` before hashing so two geometrically-equal nodes within tolerance share one address.
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of` — the ONE hasher entry every digest mint composes — plus `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter`), Rasm.Element (`Projection/address#CONTENT_ADDRESS` `ContentAddress.Of`/`ContentAddress.OfGraph`/`ContentAddress.Verify` + `Projection/address#INCREMENTAL_ADDRESS` `GraphMembers.Of`/`Advance`/`GraphMemberStep.Resolve` + `Node.ToCanonicalBytes` + `ElementGraph`), BCL inbox.
- Growth: the delta-composable graph address is the upstream `Rasm.Element/Projection/address#INCREMENTAL_ADDRESS` `GraphMembers` accumulator composed here — `GraphMembers.Of(graph)` seeds the member sets a snapshot address sorts, `members.Advance(delta, key)` steps one delta behind the `NormalForm()` gate, and only its `GraphMemberStep.Refold` outcome requests a full-state fold after a tolerance reheader; `ContentAddress.OfGraph(members)` re-enters the SAME private sorted fold the graph entry calls, so an incremental address and a recompute are byte-identical by construction and a `Version/timetravel` `Scrub` reel or `Bisect` probe pays one member step per event instead of one whole-graph fold. `ToCanonicalBytes(tolerance)` folds the parametric-form digest inside the `Digest(EncodeForm)` parametric arm it re-enters, NEVER beside it as a standalone `Of(UInt128)` sibling key — a sibling key leaves `OfGraph` blind to a parametric-body edit. `HashPolicy` widens the content address by one row under an epoch-gated identity re-mint; a new canonical-byte rule is one clause on the kernel `CanonicalWriter`; zero new surface — a second hasher, a `GetHashCode`-based address, a Persistence-local `NodeHash`/`GraphHash` forwarding owner, or a per-surface key respelling is the deleted form.
- Boundary: the `ContentAddress` is non-cryptographic identity — a tamper or security claim on it is the named defect (the `Version/provenance#ATTESTED_LEDGER` `AttestedEntry` owns tamper-evidence); the canonical byte projection is the ONE `Projection/address#IMPLEMENTATION_LAW` codec shared between the `NodeId` content hash and the diff `ContentBytes` so a node that did not change is byte-identical and the structural diff prunes it; the kernel seed convention (seed-zero content, `ContentHash.Of` the verbatim contract) is ground truth and the literal digest values stamp on the host-validation pass, never an un-run asserted value; the graph address IS the upstream `ContentAddress.OfGraph` order-independent fold, composed once so the topology memo key (`Query/topology`) and the snapshot graph identity never fork into two Persistence-local orderings, and snapshot/chunk identities wrap their precomputed digest through generated `ContentAddress.Create(UInt128)` without re-hashing.

| [INDEX] | [POLICY]            | [VALUE]                                        | [BINDING]                                                     |
| :-----: | :------------------ | :--------------------------------------------- | :------------------------------------------------------------ |
|  [01]   | content algorithm   | kernel `ContentHash.Of` (seed-zero)            | one hasher entry; a direct `XxHash128` call site is deleted   |
|  [02]   | node/graph key      | upstream `ContentAddress.Of`/`OfGraph`         | composed in one hop; no Persistence-local hash owner          |
|  [03]   | precomputed wrap    | generated `ContentAddress.Create(UInt128)`     | snapshot/chunk digest wrapped without re-hashing              |
|  [04]   | incremental address | upstream `Advance` + `OfGraph(members)`        | byte-identical to the full-state fold; a reheader refuses     |
|  [05]   | streaming identity  | kernel `ContentHash.Of<TState>(state, chunks)` | chunk order IS canonical; the one-shot spans one `int` window |
|  [06]   | identity claim      | non-cryptographic                              | tamper-evidence is `Version/provenance#ATTESTED_LEDGER`       |
|  [07]   | addressed bytes     | PLAINTEXT, never the framed form               | a re-pack under a new compression row keeps one address       |
|  [08]   | streaming callback  | kernel `CanonicalWriter` (`Raw`/`Ordinal`)     | no call site holds an `XxHash128` accumulator                 |
|  [09]   | edge key            | upstream `ContentAddress.Of(edge, tolerance)`  | streams its preimage; no materialized edge-byte array         |

## [04]-[COMPRESSION_HASHING]

- Owner: `CompressionPolicy` and `HashPolicy` `[SmartEnum<string>]` row families under the `ComparerAccessors.StringOrdinal` accessor.
- Cases: 6 compression rows — `none`, `lz4-fast`, `lz4-high`, `zstd`, `zstd-high`, `zstd-dict` (the trained-dictionary regime whose decode resolves the blob by the frame's own dict id); 2 hash rows — `Identity` (the kernel `ContentHash.Of` 128-bit content address) and `Content` (the `XxHash3` 64-bit short tag).
- Entry: `public partial byte[] Pack(ReadOnlyMemory<byte> payload)` is the pure byte transform; `public partial UInt128 Compute(ReadOnlyMemory<byte> payload)` is the row's hash.
- Packages: K4os.Compression.LZ4, ZstdSharp.Port, System.IO.Hashing (`XxHash3` short tag + the `Crc32` frame checksum — direct structural calls, never policy rows), Rasm (`ContentHash.Of`), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: one compression level is one row carrying its own `HeaderId` so every prior archive stays readable across the swap; a trained-dictionary row carries its dict blob, never a per-call branch; a WIDER content address is one `HashPolicy` row with a fresh `DomainId` under an epoch-gated identity re-mint — the `HashDomain` header byte is the forward-compatibility law that makes the row addition non-breaking; zero new surface.
- Boundary: both hash rows are non-cryptographic identity — a security claim on either is the named defect; `Identity` pins the kernel `ContentHash.Of` as the one `ContentAddress` algorithm every snapshot identity, chunk key, and diff reads, so a 64-bit hash standing in for the content address is the deleted form; `Content` is the `XxHash3` 64-bit short tag stamped on every chunk as a bloom/sketch pre-filter ahead of the authoritative 128-bit compare — a chunk-fold datum, never a header hash domain; frame checksums are STRUCTURAL frame facts, not policy rows — the `SnapshotHeader` checksum is a direct `Crc32.HashToUInt32` over the header prefix and a compression frame's own integrity word (`ZstdSharp` `checksumFlag`) belongs to its frame, so the former five-row ladder (`Frame`/`Wide`/`FrameWide`) is the deleted enumeration of call-site facts as vocabulary; the `HashDomain` law: the header byte records `Identity.DomainId`, `Verify` hard-rejects any other domain as `SnapshotTier.HashDomainGap` TODAY, and a future wider address lands as one row whose new `DomainId` the ladder resolves through `ByDomainId` under its epoch gate — never a second ladder arm and never a per-artifact algorithm negotiation; the `messagepack` codec pairs with `none` because `Lz4BlockArray` owns in-codec compression (double framing is the deleted pattern), and a `Cbor`/`JsonStj` blob whose body already rode Arrow-IPC `Zstd` block compression likewise pairs with `none`; `ZstdSharp.Port`'s self-describing frame (`contentSizeFlag`/`checksumFlag`, long-distance matching, `btultra2`) is the higher-ratio path and `LZ4Pickler` the lowest-latency self-describing frame, the policy row selecting one so a payload frames exactly once and the frame checksum complements the snapshot spine's own content hash rather than replacing it; a payload outgrowing the one-shot span rides the zstd rows' `ZstdFrame.PackStream`/`UnpackStream` `CompressionStream`/`DecompressionStream` adapters — the ONE streaming path, so whole-payload materialization is never the price of a large artifact and an LZ4 streaming sibling is the deleted parallel — and both legs read the row's own `ZstdTuning` rather than a `(level, archival)` pair each re-states, so one policy row cannot frame two ways and a row carrying no tuning has no streaming leg by construction; every codec, compression, and seal transform answers on `Fin`, so `Known`-style membership probes delete with the `Option` they were compensating for — a caller asking whether a header id is rostered takes the resolve that already knows.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CompressionPolicy {
    public static readonly CompressionPolicy None = new("none", headerId: 0, tuning: None, pack: static p => Fin.Succ(p.ToArray()), unpack: static f => Fin.Succ(f.ToArray()));
    public static readonly CompressionPolicy Lz4Fast = new("lz4-fast", headerId: 1, tuning: None, pack: static p => Fin.Succ(LZ4Pickler.Pickle(p.Span, LZ4Level.L00_FAST)), unpack: static f => Fin.Succ(LZ4Pickler.Unpickle(f.Span)));
    public static readonly CompressionPolicy Lz4High = new("lz4-high", headerId: 2, tuning: None, pack: static p => Fin.Succ(LZ4Pickler.Pickle(p.Span, LZ4Level.L09_HC)), unpack: static f => Fin.Succ(LZ4Pickler.Unpickle(f.Span)));
    public static readonly CompressionPolicy Zstd = new("zstd", headerId: 3, tuning: Some(ZstdTuning.Fast), pack: static p => Fin.Succ(ZstdFrame.Pack(p.Span, ZstdTuning.Fast)), unpack: static f => Fin.Succ(ZstdFrame.Unpack(f.Span)));
    public static readonly CompressionPolicy ZstdHigh = new("zstd-high", headerId: 4, tuning: Some(ZstdTuning.Archival), pack: static p => Fin.Succ(ZstdFrame.Pack(p.Span, ZstdTuning.Archival)), unpack: static f => Fin.Succ(ZstdFrame.Unpack(f.Span)));
    public static readonly CompressionPolicy ZstdDict = new("zstd-dict", headerId: 5, tuning: Some(ZstdTuning.Fast), pack: static p => ZstdFrame.PackDict(p.Span), unpack: static f => ZstdFrame.UnpackDict(f.Span));

    public int HeaderId { get; }
    public Option<ZstdTuning> Tuning { get; }
    public static Fin<CompressionPolicy> ByHeaderId(int headerId) =>
        FactoryBridge.Row<int, string, CompressionPolicy>(candidate: headerId, column: static row => row.HeaderId, match: None);
    [UseDelegateFromConstructor] public partial Fin<byte[]> Pack(ReadOnlyMemory<byte> payload);
    [UseDelegateFromConstructor] public partial Fin<byte[]> Unpack(ReadOnlyMemory<byte> framed);
}

public readonly record struct ZstdTuning(int Level, Seq<(ZSTD_cParameter Key, int Value)> Extra) {
    public static readonly ZstdTuning Fast = new(Level: 3, Extra: Seq<(ZSTD_cParameter, int)>());
    public static readonly ZstdTuning Archival = new(Level: 19, Extra: Seq(
        (ZSTD_cParameter.ZSTD_c_enableLongDistanceMatching, 1),
        (ZSTD_cParameter.ZSTD_c_strategy, (int)ZSTD_strategy.ZSTD_btultra2)));
}

public static class ZstdFrame {
    static Compressor Tuned(ZstdTuning tuning) {
        Compressor compressor = new(tuning.Level);
        compressor.SetParameter(ZSTD_cParameter.ZSTD_c_contentSizeFlag, 1);
        compressor.SetParameter(ZSTD_cParameter.ZSTD_c_checksumFlag, 1);
        tuning.Extra.Iter(row => compressor.SetParameter(row.Key, row.Value));
        return compressor;
    }

    public static byte[] Pack(ReadOnlySpan<byte> payload, ZstdTuning tuning) {
        using Compressor compressor = Tuned(tuning);
        return compressor.Wrap(payload).ToArray();
    }
    public static byte[] Unpack(ReadOnlySpan<byte> framed) { using Decompressor decompressor = new(); return decompressor.Unwrap(framed).ToArray(); }

    static readonly ConcurrentDictionary<uint, byte[]> Trained = new();
    static volatile uint Active;
    public static unsafe uint Train(IEnumerable<byte[]> samples, int capacity = DictBuilder.DefaultDictCapacity) {
        byte[] dictionary = DictBuilder.TrainFromBuffer(samples, capacity);
        fixed (byte* blob = dictionary) {
            uint id = Methods.ZSTD_getDictID_fromDict(blob, (nuint)dictionary.Length);
            Trained[id] = dictionary;
            Active = id;
            return id;
        }
    }
    public static Fin<byte[]> PackDict(ReadOnlySpan<byte> payload) {
        using Compressor compressor = Tuned(ZstdTuning.Fast);
        compressor.LoadDictionary(Trained[Active]);
        return Fin.Succ(compressor.Wrap(payload).ToArray());
    }
    public static unsafe Fin<byte[]> UnpackDict(ReadOnlySpan<byte> framed) {
        uint id;
        fixed (byte* frame = framed) { id = Methods.ZSTD_getDictID_fromFrame(frame, (nuint)framed.Length); }
        if (!Trained.TryGetValue(id, out byte[]? dictionary)) { return Fin.Fail<byte[]>(new CodecFault.FrameRejected($"zstd-dict:{id}")); }
        using Decompressor decompressor = new();
        decompressor.LoadDictionary(dictionary);
        return Fin.Succ(decompressor.Unwrap(framed).ToArray());
    }

    public static long PackStream(Stream source, Stream sink, ZstdTuning tuning, Option<long> pledged) {
        using CompressionStream stream = new(sink, Tuned(tuning), bufferSize: 0, leaveOpen: true, preserveCompressor: false);
        pledged.Iter(length => stream.SetPledgedSrcSize((ulong)length));
        source.CopyTo(stream);
        return sink.Length;
    }
    public static long UnpackStream(Stream framed, Stream sink) {
        using DecompressionStream stream = new(framed, new Decompressor(), bufferSize: 0, leaveOpen: true, preserveDecompressor: false);
        stream.CopyTo(sink);
        return sink.Length;
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HashPolicy {
    public static readonly HashPolicy Identity = new("xxhash128", domainId: 2, bits: 128, hexFormat: "x32", compute: static p => ContentHash.Of(p.Span));
    public static readonly HashPolicy Content = new("xxhash3", domainId: 1, bits: 64, hexFormat: "x16", compute: static p => XxHash3.HashToUInt64(p.Span));

    public byte DomainId { get; }
    public int Bits { get; }
    public string HexFormat { get; }
    [UseDelegateFromConstructor] public partial UInt128 Compute(ReadOnlyMemory<byte> payload);
    public static Fin<HashPolicy> ByDomainId(byte domainId) =>
        FactoryBridge.Row<byte, string, HashPolicy>(candidate: domainId, column: static row => row.DomainId, match: None);
}
```

## [05]-[SNAPSHOT_SPINE]

- Owner: `SnapshotHeader` is the fixed prologue and trust boundary carrying BOTH the plaintext `ContentHash` and the stored-bytes `StoredDigest`; `SnapshotAdmission` carries schema, epoch, and allocation ceilings; `SnapshotFormat` pairs codec with compression; `SnapshotRoute` owns artifact placement, retention, lineage, size, and format policy; `[FaultCase]` is the family's generated identity roster, while the nine `SnapshotTier` rows are the ordered verify ladder; `CodecFault`, `SnapshotCatalogRow`, and `Snapshots` own rejection, evidence, catalog, seal, verify, and sweep.
- Entry: `Write<T>(ProjectionContext, SnapshotRoute, T, Func<SnapshotCatalogRow,IO<Unit>>)` validates shape/format compatibility before encoding and consumes one route carrier, both size ceilings answering on the SAME `Fin` so a payload's failure shape does not depend on which bound it crossed. `Verify(ReadOnlySpan<byte>, SnapshotAdmission)` runs three raw tiers to the checksum, materializes the header ONCE, then rejects reserved-bit drift, hash-domain gaps, negative or mismatched lengths, allocation ceilings, stored-digest corruption, capability gaps, epoch, and schema off typed fields before decoding. `Sweep(ProjectionContext, SnapshotRoute, Seq<SnapshotCatalogRow>)` derives orphan age and directory from the route.
- Auto: the write fold derives the codec/compression ids, the schema fingerprint, the retention epoch, both lengths, BOTH kernel `ContentHash.Of` digests (plaintext for identity, stored for integrity), the `Crc32` header checksum, the HLC stamp, the classification, and the content-lineage rank into the catalog row; the sealed `ContentHash` IS the `ContentAddress` every secondary surface derives from, so a snapshot is catalog-addressable on the artifact-blob index without a parallel key, an unchanged artifact re-packed under a newer compression row keeps that key, and `Lineage` chains the prior edition's content address so the newest-`Count`-editions retention bound (`Version/retention`) prunes by lineage depth off the catalog row.
- Output: `SnapshotCatalogRow` — the durable catalog row whose `StoredLength`/`PlainLength` are the artifact's own sealed length fields the retention sweep reads, never a later filesystem stat, and whose HLC pair is the frame's `Clock.Stamp` at seal.
- Packages: Rasm (`ContentHash.Of`), System.IO.Hashing (`Crc32` — the structural header checksum), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new header capability is one flag-bit row; a new rejection cause is one `[FaultCase]` ladder row at the next free offset, breaking every ladder arm (it lands through the SAME `CodecFault.SnapshotRejected` case — the band wraps the ladder, never grows a parallel case per rung); a new codec-fault class is one `CodecFault` case; one artifact kind is one catalog row value; zero new surface.
- Law: the 88-byte `SnapshotHeader` is the ONE remaining fixed-width hand frame in this folder — every field is a constant-width word at a declared offset and needs no framing, while its two 16-byte digests persist through the kernel `ContentHash.Wire`/`Admit` correspondence (big-endian) and every other word is a `BinaryPrimitives` little-endian read; no corpus family replaces it because no peer decodes the prologue and the CRC must reject before any parser binds.
- Boundary: the single-pass seal `Clear`s the stack-allocated prefix buffer before both writes so the placeholder header is genuinely zeroed (terminally invalid magic) and no uninitialized padding byte ever persists into the reserved-gap offsets — the CRC is then computed over the same zeroed-gap layout it verifies against, deterministic across runtimes — and both 128-bit digests flow un-truncated to the header (a 64-bit truncation collides distinct contents and is the deleted form); the seal writes the zeroed placeholder header, the stored bytes, then seeks to zero and writes the final header, `Flush(flushToDisk: true)` before `File.Move` does the atomic rename, so a crash leaves the temp swept rather than a torn final; the header is the artifact's ENTIRE trust boundary and `Verify` runs the ordered ladder — magic/identity, the one live layout, header checksum, then, on the now-self-consistent header, hash-domain capability (the `HashDomain` byte must equal `HashPolicy.Identity.DomainId` — any other value is `SnapshotTier.HashDomainGap` until a future row's epoch gate admits it through `ByDomainId`, whose `Fin` refusal carries the offending byte the tier reports), stored-length truncation, the `StoredDigest` over stored bytes through the kernel entry, codec/compression capability, then the epoch-then-fingerprint ratchet — each tier verifying before the next so corrupted or foreign input rejects before any decoder with attack surface binds; the ladder verifies `StoredDigest` and NEVER the plaintext `ContentHash`, because the plaintext does not exist until a decompressor has already run and a ladder claiming to prove it would be asserting a digest it never took — the unpack leg proves that one; epoch and fingerprint are one-way ratchets while the layout byte admits by EQUALITY alone, so any artifact carrying another layout is `SnapshotTier.Foreign` before a field reads at the wrong offset, and a content-keyed snapshot rebuilds from its truth; temp residue and catalog-orphaned payloads leave only through the age-gated `Sweep` (a final artifact lands on disk before its catalog `persist` `Bind` commits, so the sweep reaps only residue older than the grace window) and the swept count is the crash-loop signal.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SnapshotTier {
    public static readonly SnapshotTier Foreign = new("foreign");
    public static readonly SnapshotTier HeaderCorrupt = new("header-corrupt");
    public static readonly SnapshotTier HashDomainGap = new("hash-domain-gap");
    public static readonly SnapshotTier Truncated = new("truncated");
    public static readonly SnapshotTier SizeExceeded = new("size-exceeded");
    public static readonly SnapshotTier PayloadCorrupt = new("payload-corrupt");
    public static readonly SnapshotTier CapabilityGap = new("capability-gap");
    public static readonly SnapshotTier VersionAhead = new("version-ahead");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CodecFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Codec;
    private CodecFault() { }

    [FaultCase(0)] public sealed partial record NoMutualCodec(string Surface) : CodecFault;
    [FaultCase(1)] public sealed partial record ShapeRefused(string Shape) : CodecFault;
    [FaultCase(2)] public sealed partial record FrameRejected(string Detail) : CodecFault;
    [FaultCase(3)] public sealed partial record SnapshotRejected(SnapshotTier Tier, string Evidence) : CodecFault;
    [FaultCase(4)] public sealed partial record ReassemblyDrift(UInt128 Expected, UInt128 Actual) : CodecFault;
    [FaultCase(5)] public sealed partial record ChunkManifestRejected(string Detail) : CodecFault;

    public override string Message => Switch(
        noMutualCodec:         static c => $"<codec-mutual:{c.Surface}>",
        shapeRefused:          static c => $"<codec-shape:{c.Shape}>",
        frameRejected:         static c => $"<codec-frame:{c.Detail}>",
        snapshotRejected:      static c => $"<snapshot:{c.Tier.Key}:{c.Evidence}>",
        reassemblyDrift:       static c => $"<codec-reassembly:{c.Expected:x32}!={c.Actual:x32}>",
        chunkManifestRejected: static c => $"<codec-manifest:{c.Detail}>");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SnapshotHeader(
    uint Magic, byte Layout, byte HashDomain, int CodecId, int CompressionId,
    ulong SchemaFingerprint, ulong Epoch, long PlainLength, long StoredLength, UInt128 ContentHash, UInt128 StoredDigest, uint Checksum) {
    public const int Size = 88;
    public const int ChecksumOffset = Size - 4;
    public const int StoredOffset = ChecksumOffset - 16;
    public const int ContentOffset = StoredOffset - 16;
    public const uint MagicValue = 0x504E5352;
    public const byte Layout = 3;

    public static SnapshotHeader Seal(SnapshotCodec codec, CompressionPolicy compression, ulong schemaFingerprint, ulong epoch, ReadOnlySpan<byte> plain, ReadOnlySpan<byte> stored) {
        Span<byte> prefix = stackalloc byte[Size];
        prefix.Clear();
        SnapshotHeader draft = new(MagicValue, Layout, HashPolicy.Identity.DomainId, codec.HeaderId, compression.HeaderId,
            schemaFingerprint, epoch, plain.Length, stored.Length,
            ContentHash: Rasm.Domain.ContentHash.Of(plain), StoredDigest: Rasm.Domain.ContentHash.Of(stored), Checksum: 0u);
        draft.WriteFields(prefix);
        return draft with { Checksum = Crc32.HashToUInt32(prefix[..ChecksumOffset]) };
    }

    public void Write(Span<byte> destination) { WriteFields(destination); BinaryPrimitives.WriteUInt32LittleEndian(destination[ChecksumOffset..], Checksum); }

    private void WriteFields(Span<byte> d) {
        BinaryPrimitives.WriteUInt32LittleEndian(d, Magic);
        (d[4], d[5]) = (Layout, HashDomain);
        BinaryPrimitives.WriteInt32LittleEndian(d[8..], CodecId);
        BinaryPrimitives.WriteInt32LittleEndian(d[12..], CompressionId);
        BinaryPrimitives.WriteUInt64LittleEndian(d[20..], SchemaFingerprint);
        BinaryPrimitives.WriteUInt64LittleEndian(d[28..], Epoch);
        BinaryPrimitives.WriteInt64LittleEndian(d[36..], PlainLength);
        BinaryPrimitives.WriteInt64LittleEndian(d[44..], StoredLength);
        Rasm.Domain.ContentHash.Wire(ContentHash).Span.CopyTo(d[ContentOffset..StoredOffset]);
        Rasm.Domain.ContentHash.Wire(StoredDigest).Span.CopyTo(d[StoredOffset..ChecksumOffset]);
    }
}

public sealed record SnapshotCatalogRow(
    Guid Id, string Kind, SnapshotCodec Codec, CompressionPolicy Compression, ContentAddress Hash,
    long PlainLength, long StoredLength, ulong SchemaFingerprint, ulong Epoch, Option<ContentAddress> Lineage,
    string RetentionClass, DataClassification Classification, Instant HlcPhysical, ulong HlcLogical) {
    public Instant WrittenAt => HlcPhysical;
}

public sealed record SnapshotFormat(SnapshotCodec Codec, CompressionPolicy Compression) {
    public bool Admits(Type shape) => Codec.Admits(shape) && (Codec != SnapshotCodec.MessagePackBinary || Compression == CompressionPolicy.None);
}

public sealed record SnapshotRoute(
    string Directory,
    string Kind,
    SnapshotFormat Format,
    ulong SchemaFingerprint,
    ulong Epoch,
    long MaxPlainLength,
    long MaxStoredLength,
    Duration OrphanAge,
    DataClassification Classification,
    string RetentionClass,
    Option<ContentAddress> Lineage);

public readonly record struct SnapshotAdmission(ulong SchemaFingerprint, ulong Epoch, long MaxPlainLength, long MaxStoredLength);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Snapshots {
    public const string Suffix = ".rsnp";

    public static Fin<SnapshotHeader> Verify(ReadOnlySpan<byte> artifact, SnapshotAdmission admission) =>
        artifact.Length < SnapshotHeader.Size || BinaryPrimitives.ReadUInt32LittleEndian(artifact) != SnapshotHeader.MagicValue
            ? new CodecFault.SnapshotRejected(SnapshotTier.Foreign, artifact.Length < SnapshotHeader.Size ? "headerless" : "magic")
        : artifact[4] != SnapshotHeader.Layout ? new CodecFault.SnapshotRejected(SnapshotTier.Foreign, $"layout:{artifact[4]}")
        : BinaryPrimitives.ReadUInt32LittleEndian(artifact[SnapshotHeader.ChecksumOffset..]) != Crc32.HashToUInt32(artifact[..SnapshotHeader.ChecksumOffset]) ? new CodecFault.SnapshotRejected(SnapshotTier.HeaderCorrupt, "crc")
        : Read(artifact).Bind(header => Admit(header, artifact, admission));

    static Fin<SnapshotHeader> Admit(SnapshotHeader header, ReadOnlySpan<byte> artifact, SnapshotAdmission admission) =>
        artifact[6] != 0 || artifact[7] != 0 || BinaryPrimitives.ReadUInt32LittleEndian(artifact[16..]) != 0 ? new CodecFault.SnapshotRejected(SnapshotTier.HeaderCorrupt, "reserved")
        : header.HashDomain != HashPolicy.Identity.DomainId ? new CodecFault.SnapshotRejected(SnapshotTier.HashDomainGap, $"domain:{header.HashDomain}")
        : header.PlainLength < 0 || header.StoredLength < 0 || header.StoredLength != artifact.Length - SnapshotHeader.Size ? new CodecFault.SnapshotRejected(SnapshotTier.Truncated, $"{artifact.Length - SnapshotHeader.Size}")
        : header.PlainLength > admission.MaxPlainLength || header.StoredLength > admission.MaxStoredLength ? new CodecFault.SnapshotRejected(SnapshotTier.SizeExceeded, $"{header.PlainLength}/{header.StoredLength}")
        : Rasm.Domain.ContentHash.Of(artifact[SnapshotHeader.Size..]) != header.StoredDigest ? new CodecFault.SnapshotRejected(SnapshotTier.PayloadCorrupt, "stored-digest")
        : SnapshotCodec.ByHeaderId(header.CodecId).IsFail || CompressionPolicy.ByHeaderId(header.CompressionId).IsFail ? new CodecFault.SnapshotRejected(SnapshotTier.CapabilityGap, $"codec:{header.CodecId}/{header.CompressionId}")
        : header.Epoch > admission.Epoch ? new CodecFault.SnapshotRejected(SnapshotTier.VersionAhead, $"epoch:{header.Epoch}")
        : header.SchemaFingerprint != admission.SchemaFingerprint ? new CodecFault.SnapshotRejected(SnapshotTier.VersionAhead, "fingerprint")
        : Fin.Succ(header);

    public static IO<SnapshotCatalogRow> Write<T>(ProjectionContext frame, SnapshotRoute route, T value, Func<SnapshotCatalogRow, IO<Unit>> persist) =>
        !route.Format.Admits(typeof(T))
            ? IO.fail<SnapshotCatalogRow>(new CodecFault.NoMutualCodec($"snapshot:{typeof(T).FullName}"))
            : IO.lift(() => route.Format.Codec.Serialize(typeof(T), value))
            .Bind(encoded => encoded.LongLength > route.MaxPlainLength
                ? IO.fail<(Guid Id, SnapshotHeader Header)>(new CodecFault.SnapshotRejected(SnapshotTier.SizeExceeded, $"plain:{encoded.LongLength}>{route.MaxPlainLength}"))
                : IO.lift(() => Seal(route, Guid.CreateVersion7(), encoded)))
            .Map(file => (File: file, Stamp: frame.Clock.Stamp(frame.Now())))
            .Map(stamped => new SnapshotCatalogRow(stamped.File.Id, route.Kind, route.Format.Codec, route.Format.Compression, ContentAddress.Create(stamped.File.Header.ContentHash), stamped.File.Header.PlainLength, stamped.File.Header.StoredLength, route.SchemaFingerprint, route.Epoch, route.Lineage, route.RetentionClass, route.Classification, stamped.Stamp.Physical, stamped.Stamp.Logical))
            .Bind(row => persist(row).Map(_ => row));

    public static IO<Seq<string>> Sweep(ProjectionContext frame, SnapshotRoute route, Seq<SnapshotCatalogRow> catalog) =>
        IO.lift(() => Try.lift(() => Fin.Succ((Now: frame.Now(), Files: toSeq(Directory.EnumerateFiles(route.Directory))))).Run().Bind(static inner => inner))
            .Map(scan => scan.Files.Filter(file => !catalog.Exists(row => string.Equals(Path.GetFileName(file), $"{row.Id}{Suffix}", StringComparison.Ordinal)) && scan.Now - Instant.FromDateTimeUtc(File.GetLastWriteTimeUtc(file)) >= route.OrphanAge))
            .Bind(static orphans => orphans.TraverseM(static file =>
                IO.lift(() => Try.lift(() => { File.Delete(file); return Fin<string>.Succ(file); }).Run().Bind(static inner => inner))).As());

    static Fin<(Guid Id, SnapshotHeader Header)> Seal(SnapshotRoute route, Guid id, byte[] encoded) =>
        route.Format.Compression.Pack(encoded).Bind(packed =>
            packed.LongLength > route.MaxStoredLength
                ? Fin.Fail<(Guid, SnapshotHeader)>(new CodecFault.SnapshotRejected(SnapshotTier.SizeExceeded, $"stored:{packed.LongLength}>{route.MaxStoredLength}"))
                : Fin.Succ((id, Land(route, id, SnapshotHeader.Seal(
                    route.Format.Codec, route.Format.Compression, route.SchemaFingerprint, route.Epoch, encoded, packed), packed))));

    static SnapshotHeader Land(SnapshotRoute route, Guid id, SnapshotHeader header, byte[] packed) {
        Span<byte> prefix = stackalloc byte[SnapshotHeader.Size];
        prefix.Clear();
        string final = Path.Combine(route.Directory, $"{id}{Suffix}");
        string temp = $"{final}.tmp";
        using (FileStream stream = new(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
            stream.Write(prefix);
            stream.Write(packed);
            header.Write(prefix);
            stream.Position = 0;
            stream.Write(prefix);
            stream.Flush(flushToDisk: true);
        }
        File.Move(temp, final, overwrite: false);
        return header;
    }

    static Fin<SnapshotHeader> Read(ReadOnlySpan<byte> a) {
        (uint magic, byte version, byte domain, int codec, int compression) = (BinaryPrimitives.ReadUInt32LittleEndian(a), a[4], a[5], BinaryPrimitives.ReadInt32LittleEndian(a[8..]), BinaryPrimitives.ReadInt32LittleEndian(a[12..]));
        (ulong fingerprint, ulong epoch, long plain, long stored) = (BinaryPrimitives.ReadUInt64LittleEndian(a[20..]), BinaryPrimitives.ReadUInt64LittleEndian(a[28..]), BinaryPrimitives.ReadInt64LittleEndian(a[36..]), BinaryPrimitives.ReadInt64LittleEndian(a[44..]));
        uint checksum = BinaryPrimitives.ReadUInt32LittleEndian(a[SnapshotHeader.ChecksumOffset..]);
        return (Rasm.Domain.ContentHash.Admit(a[SnapshotHeader.ContentOffset..SnapshotHeader.StoredOffset], key),
                Rasm.Domain.ContentHash.Admit(a[SnapshotHeader.StoredOffset..SnapshotHeader.ChecksumOffset], key))
            .Apply((content, storedDigest) => new SnapshotHeader(magic, version, domain, codec, compression, fingerprint, epoch, plain, stored, content, storedDigest, checksum))
            .As();
    }
}
```

## [06]-[CONTENT_CHUNKING]

- Owner: `ChunkPolicy` the FastCDC min/avg/max size axis; `ContentChunk` the content-keyed chunk record carrying its 128-bit content address, source offset, length, and the `XxHash3` short tag; `ChunkManifest` the per-payload ordered chunk-key sequence; `ContentChunker` the static surface owning the FastCDC cut, the per-chunk content-key derivation, the manifest fold, and the cross-payload dedup projection.
- Entry: `Chunk` cuts and addresses a segmented window through ONE `Emit` pass the interior fold and the terminal seal both take, so terminality is a FastCDC argument at one boundary rather than a flag threaded through the segment carrier, the fold, and the cut signature; `Novel` probes local or peer chunk indexes only; `Reassemble` proves contiguous offsets, positive chunk lengths, fetched length, and each chunk address as it drains into the caller's sink, then proves the whole-artifact address from the kernel `CanonicalWriter` it folded on the way.
- Law: the artifact ceiling is LIFTED — the window is `ReadOnlySequence<byte>` and the identity folds through the kernel streaming leg, so the `> int.MaxValue` reassembly refusal that stood as the solution's hard 2 GiB artifact bound was never a policy and deletes with the contiguity demand that produced it; a payload larger than one `byte[]` is now representable end to end and the FastCDC per-segment `byte[]` bound is a chunker fact the segment walk absorbs, never a payload bound.
- Auto: the content-defined boundary is the FastCDC normalized gear-hash cut so an insertion that shifts every fixed-window boundary leaves the content-defined boundaries stable past the edit and a small change to a large artifact re-stores only the changed chunks; each chunk's content key is the kernel `ContentHash.Of` (the `HashPolicy.Identity` row) so an identical chunk across two snapshots or two peers dedups; the short tag is `HashPolicy.Content` (`XxHash3`) so `Novel` probes `mayHold(ShortTag)` before `holds(ContentKey)` on a hot re-store path; a multi-segment payload cuts segment by segment with the sub-minimum tail carried forward, so the cut set matches the one contiguous bytes produce and no manifest depends on how the caller happened to buffer.
- Output: `ChunkAssembly` carries the verified address, the drained length, and the chunk tally.
- Packages: FastCDC.Net, Rasm (`ContentHash.Of` — both the span and the streaming legs — plus the `Rasm/Domain/identity#CONTENT_KEY` `CanonicalWriter` the streaming callback hands each fold, whose `Raw` is the one chunk-append spelling), System.IO.Hashing (`XxHash3` short tag only — the 128-bit accumulator is the kernel's and no call site holds one), LanguageExt.Core, BCL inbox (`ReadOnlySequence<byte>`, `IBufferWriter<byte>`).
- Growth: a new chunk-size profile is one `ChunkPolicy` row; zero new surface — a fixed-window framing, a per-edit full re-store, a second content-defined chunker, a whole-payload materialization ahead of the cut, or a length refusal standing in for a capability bound is the deleted form.
- Boundary: chunk membership proves only local or peer chunk placement. Remote object-store placement is the provider's exact-object conditional seal; no chunk index can skip or synthesize provider objects. Multipart windows may preserve whole FastCDC cuts as part boundaries without treating their membership as object evidence.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Rasm.Domain;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChunkPolicy {
    public static readonly ChunkPolicy Artifact = new("artifact", min: 16 * 1024u, avg: 64 * 1024u, max: 256 * 1024u);
    public static readonly ChunkPolicy Small = new("small", min: 2 * 1024u, avg: 8 * 1024u, max: 32 * 1024u);

    public uint Min { get; }
    public uint Avg { get; }
    public uint Max { get; }
    private ChunkPolicy(string key, uint min, uint avg, uint max) : this(key) => (Min, Avg, Max) = (min, avg, max);

    public FastCdc Over(byte[] segment, bool eof) => new(segment, Min, Avg, Max, eof);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ContentChunk(UInt128 ContentKey, ulong ShortTag, long Offset, int Length);

public readonly record struct ChunkManifest(ContentAddress WholeArtifact, long Length, Seq<ContentChunk> Chunks);

public readonly record struct ChunkAssembly(ContentAddress WholeArtifact, long Length, int Chunks);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentChunker {
    public static ChunkManifest Chunk(ChunkPolicy policy, ReadOnlySequence<byte> payload) {
        ChunkWalk walk = Emit(policy,
            Segments(payload).Fold(ChunkWalk.Empty, (state, segment) => Emit(policy, state, segment, eof: false)),
            ReadOnlyMemory<byte>.Empty, eof: true);
        return new ChunkManifest(
            ContentAddress.Create(ContentHash.Of(walk.Cuts, static (cuts, writer) => cuts.Iter(cut => writer.Raw(cut.Span.Span)))),
            payload.Length,
            walk.Cuts.Map(static cut => cut.Chunk));
    }

    public static Seq<ContentChunk> Novel(ChunkManifest manifest, Func<ulong, bool> mayHold, Func<UInt128, bool> holds) =>
        manifest.Chunks.Filter(chunk => !mayHold(chunk.ShortTag) || !holds(chunk.ContentKey));

    public static Fin<ChunkAssembly> Reassemble(ChunkManifest manifest, Func<UInt128, ReadOnlyMemory<byte>> fetch, IBufferWriter<byte> sink) {
        ChunkDrain drain = new(manifest, fetch, sink);
        UInt128 whole = ContentHash.Of(drain, static (state, hash) => state.Take(hash));
        return drain.Refusal.Match(
            Some: Fin<ChunkAssembly>.Fail,
            None: () => drain.Offset != manifest.Length
                ? Fin<ChunkAssembly>.Fail(new CodecFault.ChunkManifestRejected($"terminal:{drain.Offset}!={manifest.Length}"))
                : whole == manifest.WholeArtifact.Value
                    ? Fin<ChunkAssembly>.Succ(new ChunkAssembly(manifest.WholeArtifact, drain.Offset, manifest.Chunks.Count))
                    : Fin<ChunkAssembly>.Fail(new CodecFault.ReassemblyDrift(manifest.WholeArtifact.Value, whole)));
    }

    static Seq<ReadOnlyMemory<byte>> Segments(ReadOnlySequence<byte> payload) {
        SequencePosition position = payload.Start;
        List<ReadOnlyMemory<byte>> held = [];
        while (payload.TryGet(ref position, out ReadOnlyMemory<byte> segment)) { held.Add(segment); }
        return toSeq(held);
    }

    static ChunkWalk Emit(ChunkPolicy policy, ChunkWalk state, ReadOnlyMemory<byte> segment, bool eof) {
        byte[] source = state.Carry.IsEmpty ? segment.ToArray() : [.. state.Carry.Span, .. segment.Span];
        Seq<(ContentChunk Chunk, ReadOnlyMemory<byte> Span)> cuts = toSeq(policy.Over(source, eof).GetChunks()
            .Select(cut => {
                ReadOnlyMemory<byte> span = source.AsMemory((int)cut.Offset, (int)cut.Length);
                return (new ContentChunk(ContentHash.Of(span.Span), XxHash3.HashToUInt64(span.Span), state.Base + cut.Offset, (int)cut.Length), span);
            }));
        long consumed = cuts.Fold(0L, static (sum, cut) => sum + cut.Chunk.Length);
        return new ChunkWalk(state.Cuts + cuts, source.AsMemory((int)consumed), state.Base + consumed);
    }
}

public readonly record struct ChunkWalk(Seq<(ContentChunk Chunk, ReadOnlyMemory<byte> Span)> Cuts, ReadOnlyMemory<byte> Carry, long Base) {
    public static readonly ChunkWalk Empty = new(Seq<(ContentChunk, ReadOnlyMemory<byte>)>(), ReadOnlyMemory<byte>.Empty, 0L);
}

file sealed class ChunkDrain(ChunkManifest manifest, Func<UInt128, ReadOnlyMemory<byte>> fetch, IBufferWriter<byte> sink) {
    public long Offset { get; private set; }
    public Option<CodecFault> Refusal { get; private set; }

    public void Take(CanonicalWriter writer) {
        foreach (ContentChunk chunk in manifest.Chunks) {
            if (Refusal.IsSome) { return; }
            if (chunk.Offset != Offset || chunk.Length <= 0 || Offset + chunk.Length > manifest.Length) {
                Refusal = Some<CodecFault>(new CodecFault.ChunkManifestRejected($"span:{chunk.Offset}+{chunk.Length}@{Offset}/{manifest.Length}"));
                return;
            }
            ReadOnlyMemory<byte> payload = fetch(chunk.ContentKey);
            if (payload.Length != chunk.Length || ContentHash.Of(payload.Span) != chunk.ContentKey) {
                Refusal = Some<CodecFault>(new CodecFault.ChunkManifestRejected($"chunk:{chunk.ContentKey:X32}/{payload.Length}"));
                return;
            }
            writer.Raw(payload.Span);
            sink.Write(payload.Span);
            Offset += chunk.Length;
        }
    }
}
```

| [INDEX] | [POLICY]         | [VALUE]                             | [BINDING]                                                    |
| :-----: | :--------------- | :---------------------------------- | :----------------------------------------------------------- |
|  [01]   | chunk boundary   | FastCDC normalized gear-hash cut    | insertion-stable; small change re-stores only changed chunks |
|  [02]   | chunk identity   | kernel `ContentHash.Of` content key | dedup across snapshots/peers; never the gear-hash cut        |
|  [03]   | dedup pre-filter | `XxHash3` 64-bit short tag          | `Novel` probes `mayHold` before `holds`                      |
|  [04]   | reassembly guard | whole-artifact content hash         | torn/reordered manifest faults, never silent wrong bytes     |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

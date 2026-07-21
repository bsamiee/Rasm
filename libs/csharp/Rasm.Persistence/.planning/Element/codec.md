# [PERSISTENCE_ELEMENT_CODEC]

Rasm.Persistence encodes every durable `ElementGraph`, `GraphDelta`, and geometry blob through one `SnapshotCodec` axis paired with the `CompressionPolicy` and `HashPolicy` axes, derives the one `ContentAddress` by composing the kernel `Rasm.Domain` `ContentHash.Of` seed-zero entry over canonical bytes, seals every file-resident artifact under one fixed-offset `SnapshotHeader` that is the artifact's entire trust boundary, verifies every read through one ordered `RejectTier` ladder that rejects before any decoder with attack surface runs, and splits opaque payload bytes into FastCDC content-defined chunks for cross-snapshot dedup. A `SnapshotHeader` records what WAS done — observed compression, both plain and stored length, the content hash over stored bytes, the schema fingerprint, the retention epoch, and its own checksum — never what was requested, so a torn or foreign artifact self-rejects by offset before the codec machinery binds.

Codec, compression, and hash variance are delegate rows on string-keyed smart enums, and codec residence is fixed at write: the canonical CBOR row's deterministic map-key order makes bytes content-stable for the `ContentAddress` a schemaless MessagePack body cannot guarantee, the MessagePack row is the Marten-event and cache wire, JSON-STJ the inspector/web wire, file-raw the geometry-blob passthrough. `ContentAddress` (the seam `Projection/address#CONTENT_ADDRESS` `[ValueObject<UInt128>]` hasher over the `Projection/address#CANONICAL_WRITER` codec), `ElementGraph`, `GraphDelta`, and `Node` arrive settled from `Rasm.Element`; `ContentHash.Of` from the `Rasm` kernel; the clock/correlation/tenant slots ride the Persistence-owned `Element/graph#STORE_RAIL` `ProjectionContext` frame the composition root fills; `ReceiptSinkPort` and `DataClassification` stay port and wire inputs at the seam.

## [01]-[INDEX]

- [02]-[CODEC_AXIS]: four codec rows, the package wire context, generated converter admission, the `GeoJsonProjection` dual service, and the AOT resolver landmark.
- [03]-[CONTENT_ADDRESS]: the kernel-composed content address, the canonical-byte projection, the precomputed-digest wrap, and the incremental/streaming consumer contracts.
- [04]-[COMPRESSION_HASHING]: compression rows, the two-row hash axis, framing routes, and identity values.
- [05]-[SNAPSHOT_SPINE]: fixed-offset header trust boundary, the tier rejection ladder, the single-pass atomic write fold, and the orphan sweep.
- [06]-[CONTENT_CHUNKING]: FastCDC content-defined chunk boundaries and per-chunk content-key dedup.

## [02]-[CODEC_AXIS]

- Owner: `SnapshotCodec` `[SmartEnum<string>]` under the `ComparerAccessors.StringOrdinal` accessor; `ElementJson` the package `JsonSerializerContext` partial joining the suite STJ merge; `InstantFormatter` the one primitive-mapped NodaTime MessagePack formatter; `WireSurface` the wire-surface vocabulary each codec admits through its frozen `Membership` set, so content negotiation is the codec rows a surface admits, never a parallel format enum; `GeoJsonProjection` the one `GeoJsonConverterFactory` admission; `PersistenceResolver` the AOT MessagePack resolver landmark.
- Cases: codec rows `json-stj`/`messagepack`/`file-raw`/`cbor`; wire surfaces `snapshot`/`cache`/`sync`/`web`.
- Entry: `Serialize(Type, object?)` is the row transform; `Negotiate(WireSurface, Type, Seq<string>)` filters by surface and verified shape admission before ranking accepted rows, so attribute-free seam graphs cannot enter MessagePack through negotiation.
- Auto: registering `ThinktectureJsonConverterFactory` and `ThinktectureMessageFormatterResolver.Instance` once derives every `[ValueObject]`/`[SmartEnum]`/`[Union]` converter and formatter, so a `NodeId`/`ContentAddress`/`Discipline` crosses both the Marten-event STJ wire and the MessagePack cache wire as its bare key with zero hand codec; `GeoJsonProjection` admits one `GeoJsonConverterFactory` deriving the GeoJSON projection of every `NetTopologySuite` geometry, feature, and attribute table the `Coverage`/`GeoReference` nodes carry; `Marten.UseSystemTextJsonForSerialization(ElementJson.Options)` binds the `json-stj` options as the event-store serializer so a stored `GraphEvent` and an inspector projection share one converter set.
- Receipt: codec negotiation rides `store.codec.negotiate` carrying the surface and the resolved codec; the AOT resolver swap rides no receipt (a build-time selection).
- Packages: MessagePack, MessagePackAnalyzer, System.Formats.Cbor, Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, Thinktecture.Runtime.Extensions.MessagePack, NetTopologySuite.IO.GeoJSON4STJ, NodaTime, NodaTime.Serialization.SystemTextJson, BCL inbox.
- Growth: one codec is one row; a new wire record is one `[JsonSerializable]` row on `ElementJson` plus one MessagePack union tag when polymorphic; a new GeoJSON consumer composes the SAME `GeoJsonProjection.Default.Factory` — `Ingest/geospatial` reifies feature `properties` through `IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>` under its own open-resolver `GeoWire.Options` carrying THIS factory (two options views, one converter/factory owner), never a second `GeoJsonConverterFactory`; the AOT landmark swaps the runtime resolver LIST for the single source-generated `PersistenceResolver` (Thinktecture + generated, dropping the reflection `StandardResolver` AOT cannot trim), the custom `InstantFormatter` staying the prepended formatter head in BOTH chains because a `[CompositeResolver]` composes resolver TYPES and cannot carry a standalone `IMessagePackFormatter<Instant>` instance; zero new surface.
- Boundary: artifact-kind-to-codec residence is fixed at write — a second codec on one kind is a conflict, not a fallback; the SEAM graph types (`GraphDelta`/`Header`/`Node`/`Relationship` carrying LanguageExt `Seq`/`Option` and Thinktecture `[Union]`/`[SmartEnum]`/`[ValueObject]` members, with NO `[MessagePackObject]` because the seam stays library-neutral) ride the `json-stj` row ONLY — source-gen-registered on `ElementJson` (`GraphEvent`/`GraphProjection`/`GraphDelta` roots, the rest reachable transitively), whose STJ set handles `Seq`/`Option`/`[Union]`/NodaTime — because the `messagepack` row's `GeneratedMessagePackResolver` finds only `[MessagePackObject]` owners and its `StandardResolver` rejects an attribute-free `Seq<Node>`, so MessagePack on the seam graph is the deleted phantom and the `messagepack` row covers ONLY the `[MessagePackObject]`/`[MessagePack.Union]`-attributed Persistence-owned wire types (`Version/commits#CRDT_WIRE` `CrdtOpWire`); the `messagepack` row is the Marten cache/sync wire and pairs with the `none` compression row because `Lz4BlockArray` owns compression in-codec (double framing is the deleted pattern); the `cbor` row is the self-describing IETF blob codec whose `CborConformanceMode.Canonical` map-key order makes the bytes content-stable for the `ContentAddress` (the guarantee a schemaless MessagePack body cannot give across insertion order) and whose `Strict` reader over a FIXED `ReadOnlyMemory` is the untrusted-egress guard — the buffer length bounds an over-declared byte-string (a `CborContentException` on a length bomb, never an over-read) and the trailing-bytes check rejects a smuggled suffix — so a structured self-describing blob routes through `Cbor` while an evolving typed record stays `messagepack`; the `json-stj` row is the inspector/web wire and the Marten event-store serializer; the `file-raw` row is the geometry-blob passthrough that never re-frames; every `messagepack` decode uses `MessagePackSecurity.UntrustedData.WithMaximumObjectGraphDepth(256)`, and the `#SNAPSHOT_SPINE` `Snapshots.Verify` ladder adds header, length, schema, epoch, checksum, and content-address admission before decode; MemoryPack and protobuf snapshot encodings stay rejected — proto owns RPC payloads only.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WireSurface {
    public static readonly WireSurface Snapshot = new("snapshot");
    public static readonly WireSurface Cache = new("cache");
    public static readonly WireSurface Sync = new("sync");
    public static readonly WireSurface Web = new("web");
}

// --- [SERVICES] ---------------------------------------------------------------------------
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
            Converters = { new ThinktectureJsonConverterFactory(), GeoJsonProjection.Default.Factory },
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
        serialize: static (shape, value) => JsonSerializer.SerializeToUtf8Bytes(value, shape, ElementJson.Options),
        deserialize: static (shape, payload) => JsonSerializer.Deserialize(payload.Span, shape, ElementJson.Options));
    public static readonly SnapshotCodec MessagePackBinary = new("messagepack", headerId: 2, negotiationRank: 3, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot, WireSurface.Cache, WireSurface.Sync, WireSurface.Web]),
        admits: static shape => shape.IsDefined(typeof(MessagePackObjectAttribute), inherit: false) || shape.IsDefined(typeof(MessagePack.UnionAttribute), inherit: false),
        serialize: static (shape, value) => MessagePackSerializer.Serialize(shape, value, Binary),
        deserialize: static (shape, payload) => MessagePackSerializer.Deserialize(shape, payload, Binary));
    public static readonly SnapshotCodec FileRaw = new("file-raw", headerId: 3, negotiationRank: 0, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot]),
        admits: static shape => shape == typeof(byte[]) || shape == typeof(ReadOnlyMemory<byte>),
        serialize: static (_, value) => value switch { byte[] bytes => bytes, ReadOnlyMemory<byte> memory => memory.ToArray(), _ => throw new InvalidOperationException("<raw-shape>") },
        deserialize: static (_, payload) => payload.ToArray());
    public static readonly SnapshotCodec Cbor = new("cbor", headerId: 4, negotiationRank: 2, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot, WireSurface.Sync, WireSurface.Web]),
        admits: static shape => shape == typeof(byte[]) || shape == typeof(ReadOnlyMemory<byte>),
        serialize: static (_, value) => CborBlob.Encode(value switch { byte[] bytes => bytes, ReadOnlyMemory<byte> memory => memory, _ => throw new InvalidOperationException("<cbor-shape>") }),
        deserialize: static (_, payload) => CborBlob.Decode(payload));

    public int HeaderId { get; }
    public int NegotiationRank { get; }
    public FrozenSet<WireSurface> Membership { get; }

    public static bool Known(int headerId) => Items.Any(c => c.HeaderId == headerId);
    public static Option<SnapshotCodec> ByHeaderId(int headerId) => toSeq(Items).Find(c => c.HeaderId == headerId);
    public bool Serves(WireSurface surface) => Membership.Contains(surface);

    public static Fin<SnapshotCodec> Negotiate(WireSurface surface, Type shape, Seq<string> accepted) =>
        toSeq(Items).Filter(c => c.Serves(surface) && c.Admits(shape)).OrderByDescending(static c => c.NegotiationRank).ToSeq()
            .Find(c => accepted.Contains(c.Key))
            .Match(Some: Fin.Succ, None: () => Fin.Fail<SnapshotCodec>(new CodecFault.NoMutualCodec(surface.Key)));

    [UseDelegateFromConstructor] public partial bool Admits(Type shape);
    [UseDelegateFromConstructor] public partial byte[] Serialize(Type shape, object? value);
    [UseDelegateFromConstructor] public partial object? Deserialize(Type shape, ReadOnlyMemory<byte> payload);

    // The runtime resolver list. The custom `InstantFormatter` is the prepended formatter HEAD (a standalone
    // `IMessagePackFormatter<Instant>` a generated `[CompositeResolver]` cannot carry), then the Thinktecture
    // generated-owner resolver, the source-generated `GeneratedMessagePackResolver`, and the reflection
    // `StandardResolver` BCL fallback. The published-AOT build selects `Aot` — the single generated
    // `PersistenceResolver` (Thinktecture + generated, no reflection fallback) — keeping the SAME `InstantFormatter`
    // head, so the AOT path never drops the `Instant` formatting the snapshot/cache `GraphEvent`/`SnapshotCatalogRow` carry.
    public static readonly MessagePackSerializerOptions Binary = BuildBinary(
        ThinktectureMessageFormatterResolver.Instance, GeneratedMessagePackResolver.Instance, StandardResolver.Instance);
    public static readonly MessagePackSerializerOptions Aot = BuildBinary(PersistenceResolver.Instance);

    static MessagePackSerializerOptions BuildBinary(params IFormatterResolver[] resolvers) =>
        MessagePackSerializerOptions.Standard
            .WithResolver(CompositeResolver.Create([InstantFormatter.Instance], resolvers))
            .WithSecurity(MessagePackSecurity.UntrustedData.WithMaximumObjectGraphDepth(256))
            .WithCompression(MessagePackCompression.Lz4BlockArray);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Canonical CBOR blob — `Canonical` reorders map keys + emits shortest-form integers so the bytes are
// content-stable for the `ContentAddress`. The frame is one tagged byte string (a flat data item, no nesting),
// so the untrusted-egress guard is structural, not a depth count: the `Strict` reader over the FIXED
// `payload` buffer rejects a malformed/indefinite-length frame and an over-declared byte-string length raises
// `CborContentException` rather than over-reading the buffer (a `CurrentDepth` cap would be inert — a byte
// string never nests), and the trailing-bytes check rejects a smuggled suffix so a torn frame self-faults.
public static class CborBlob {
    public static byte[] Encode(ReadOnlyMemory<byte> payload) {
        CborWriter writer = new(CborConformanceMode.Canonical);
        writer.WriteTag(CborTag.SelfDescribeCbor);
        writer.WriteByteString(payload.Span);
        return writer.Encode();
    }
    public static byte[] Decode(ReadOnlyMemory<byte> payload) {
        CborReader reader = new(payload, CborConformanceMode.Strict);
        if (reader.PeekState() == CborReaderState.Tag && reader.PeekTag() == CborTag.SelfDescribeCbor) reader.ReadTag();
        byte[] bytes = reader.ReadByteString();
        return reader.BytesRemaining == 0 ? bytes : throw new CborContentException($"<cbor-trailing:{reader.BytesRemaining}>");
    }
}

// The ONE GeoJSON converter FACTORY — the codec's closed source-generated options and the `Ingest/geospatial`
// open-resolver `GeoWire.Options` are two options VIEWS over this single factory (feature reification needs an
// open resolver the closed `ElementJson` resolver cannot serve), so geometry conversion and the WGS84 factory
// never fork while each options set keeps its own resolver posture.
public sealed record GeoJsonProjection(GeometryFactory Geometry, bool WriteBoundingBox = true, string IdProperty = GeoJsonConverterFactory.DefaultIdPropertyName, bool AllowModifyingAttributes = false) {
    // GeoJSON is FIXED WGS84 (RFC 7946), so the shared factory carries SRID 4326 — the geospatial admission
    // rejects a factory whose SRID disagrees with the canonical CRS, and a bare GeometryFactory.Default (SRID 0)
    // cannot express that law.
    public static readonly GeoJsonProjection Default = new(NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326));
    public GeoJsonConverterFactory Factory => new(Geometry, WriteBoundingBox, IdProperty, RingOrientationOption.EnforceRfc9746, AllowModifyingAttributes);
}

// --- [COMPOSITION] ------------------------------------------------------------------------
// The source-generated resolver pair. `GeneratedMessagePackResolver` discovers every `[MessagePackObject]`
// owner at compile time; `PersistenceResolver` is the AOT landmark composing the Thinktecture + generated
// resolvers (no reflection `StandardResolver`) — `Aot` prepends `InstantFormatter` to it so the custom NodaTime
// `Instant` formatter survives the runtime→AOT swap. A `[CompositeResolver]` composes resolver TYPES, never a
// standalone `IMessagePackFormatter`, so the `Instant` formatter rides the `BuildBinary` head in both chains.
[GeneratedMessagePackResolver] public partial class GeneratedMessagePackResolver;
[CompositeResolver(typeof(ThinktectureMessageFormatterResolver), typeof(GeneratedMessagePackResolver))] public partial class PersistenceResolver;
```

## [03]-[CONTENT_ADDRESS]

- Owner: `ContentAddress` the seam `Rasm.Element/Projection/address#CONTENT_ADDRESS` `[ValueObject<UInt128>]` content key every snapshot identity, dedup probe, diff, and AS-OF cut reads, COMPOSED here directly — Persistence mints NO node/graph hash owner of its own: the node content key is the seam `ContentAddress.Of(node.ToCanonicalBytes(tolerance).Span)` a `Version/merge#STRUCTURAL_DIFF` `GraphNode` composes inline, the graph address the seam `ContentAddress.OfGraph(graph)` a `Query/topology` memo key reads inline, and a precomputed framing/chunk/snapshot digest the seam `ContentAddress.Of(UInt128)` wraps without re-hashing — so a Persistence-local `NodeHash`/`GraphHash` forwarding owner over those one-hop seam entries is the deleted form (the ONE byte projection and the ONE order-independent fold already live on the seam, never re-spelled).
- Cases: a graph content address IS the seam `Projection/address#CONTENT_ADDRESS` `ContentAddress.OfGraph` (the semantic header folded first, then sorted node addresses, then sorted edge canonical bytes); a node content address is the seam `ContentAddress.Of` over the node's `Projection/address#CANONICAL_WRITER` `ToCanonicalBytes()` projection (fixed IEEE-754 LE bits with `-0.0→0.0` and `NaN→canonical`, measure quantization to the header tolerance, explicit attribute-order canon); a precomputed framing/chunk/snapshot digest wraps through the seam `ContentAddress.Of(UInt128)` carrier (no re-hash); delta keying rides the `Version/commits#CRDT_WIRE` `CrdtWire.ContentKey`, never a second delta hasher.
- Entry: the seam owns every minting entry — `ContentAddress.Of(ReadOnlySpan<byte>)` hashes the framing/chunk preimage, `ContentAddress.Of(UInt128)` wraps a precomputed snapshot/chunk digest, `ContentAddress.Of(Node, tolerance)` is the id-INCLUSIVE graph-dedup key, `ContentAddress.OfGraph(ElementGraph)` the order-independent snapshot identity, and `ContentAddress.Verify(...)` the re-hash gate; this page composes those entries at the snapshot catalog (`Snapshots.Write` wraps the sealed `SnapshotHeader.ContentHash` `UInt128` through `Of(UInt128)` into the `SnapshotCatalogRow`) and the chunk fold (`ContentChunker.Chunk` wraps the whole-payload digest through `Of(UInt128)`), never a Persistence-local re-derivation.
- Auto: the content address is the kernel's ONE algorithm — the `Rasm.Domain` `ContentHash.Of` seed-zero `XxHash128` entry, the same digest the seam's `ContentAddress` value-object wraps and the `Rasm` kernel mints for geometry by content-hash — so a snapshot, a chunk, a diff, and a federation key all read one 128-bit address and a second hasher is the deleted form; this page's opaque framing and chunk preimages (the sealed-bytes digest in `SnapshotHeader.Seal`, the per-chunk and whole-payload digests in `ContentChunker`) compose `ContentHash.Of` DIRECTLY — a per-call-site `XxHash128.HashToUInt128` invocation is the deleted spelling (value-identical, so the re-anchor is pure call-path collapse, never an identity migration) — and wrap every result through the seam `ContentAddress.Of(UInt128)` carrier, while the node/graph content keys compose the seam's `ToCanonicalBytes`-backed entries verbatim so the float-bearing parity corpus (`Version/commits#CRDT_WIRE`) pins the layout cross-runtime; measure quantization to `Header.Tolerance` happens once inside the seam `CanonicalWriter` before hashing so two geometrically-equal nodes within tolerance share one address.
- Receipt: content addressing rides no standalone receipt (it folds into the snapshot/diff/dedup receipts that carry the address).
- Packages: Rasm (`Rasm.Domain` `ContentHash.Of` — the ONE hasher entry every digest mint composes), Rasm.Element (`Projection/address#CONTENT_ADDRESS` `ContentAddress.Of`/`ContentAddress.OfGraph`/`ContentAddress.Verify` + `Projection/address#CANONICAL_WRITER` `Node.ToCanonicalBytes` + `ElementGraph`), BCL inbox.
- Growth: TWO consumer contracts are recorded on this section, both provided by the seam owner on landing. INCREMENTAL `OfGraph(prior, delta)` — the delta-composable graph address (`Version/timetravel` `Scrub`/`Bisect` re-shape onto it) is `Rasm.Element/Projection/address`'s member; until it lands the documented interim is the whole-graph `OfGraph` recompute, and the parametric-form digest is a COMPONENT of `ToCanonicalBytes(tolerance)` (the `ParametricStream` clause), NEVER a standalone `Of(UInt128)` sibling key — a sibling key would leave `OfGraph` blind to a parametric-body edit. STREAMING identity — the kernel `ContentHash` Growth row's incremental lifecycle (`XxHash128` `Append` + `GetCurrentHashAsUInt128`, seed zero) lands as one kernel member when a consumer demands it, and Persistence IS that demanding consumer for the blobstore multipart and chunk folds whose payloads outgrow a one-shot span; until it lands the documented interim is the one-shot `ContentHash.Of` over the in-memory payload plus the `ContentAddress.Of(UInt128)` wrap of a precomputed digest. A wider content address is one `HashPolicy` row plus an epoch-gated identity migration; a new canonical-byte rule is one clause on the seam `CanonicalWriter`; zero new surface — a second hasher, a `GetHashCode`-based address, a Persistence-local `NodeHash`/`GraphHash` forwarding owner, or a per-surface key respelling is the deleted form.
- Boundary: the `ContentAddress` is non-cryptographic identity — a tamper or security claim on it is the named defect (the `Version/provenance#ATTESTED_LEDGER` `AttestedEntry` owns tamper-evidence); the canonical byte projection is the ONE seam `Projection/address#CANONICAL_WRITER` codec shared between the `NodeId` content hash and the diff `ContentBytes` so a node that did not change is byte-identical and the structural diff prunes it; the kernel seed convention (seed-zero content, `ContentHash.Of` the verbatim contract) is ground truth and the literal digest values stamp on the host-validation pass, never an un-run asserted value; the graph address IS the seam `ContentAddress.OfGraph` order-independent fold, composed once so the topology memo key (`Query/topology`) and the snapshot graph identity never fork into two Persistence-local orderings, and the snapshot/chunk identities wrap their precomputed digest through `ContentAddress.Of(UInt128)` rather than the bare Thinktecture `Create`, so the seam's wrap verb is the one Persistence reads.

| [INDEX] | [POLICY]            | [VALUE]                                            | [BINDING]                                                   |
| :-----: | :------------------ | :------------------------------------------------- | :---------------------------------------------------------- |
|  [01]   | content algorithm   | kernel `ContentHash.Of` (seed-zero)                | one hasher entry; a direct `XxHash128` call site is deleted |
|  [02]   | node/graph key      | seam `ContentAddress.Of`/`OfGraph`                 | composed in one hop; no Persistence-local hash owner        |
|  [03]   | precomputed wrap    | seam `ContentAddress.Of(UInt128)`                  | snapshot/chunk digest wrapped, never raw `Create`           |
|  [04]   | incremental address | `OfGraph(prior, delta)` seam contract              | whole-graph recompute is the documented interim             |
|  [05]   | streaming identity  | kernel `Append`/`GetCurrentHashAsUInt128` contract | one-shot `ContentHash.Of` is the documented interim         |
|  [06]   | identity claim      | non-cryptographic                                  | tamper-evidence is `Version/provenance#ATTESTED_LEDGER`     |

## [04]-[COMPRESSION_HASHING]

- Owner: `CompressionPolicy` and `HashPolicy` `[SmartEnum<string>]` row families under the `ComparerAccessors.StringOrdinal` accessor.
- Cases: 5 compression rows — `none`, `lz4-fast`, `lz4-high`, `zstd`, `zstd-high`; 2 hash rows — `Identity` (the kernel `ContentHash.Of` 128-bit content address) and `Content` (the `XxHash3` 64-bit short tag).
- Entry: `public partial byte[] Pack(ReadOnlyMemory<byte> payload)` is the pure byte transform; `public partial UInt128 Compute(ReadOnlyMemory<byte> payload)` is the row's hash.
- Packages: K4os.Compression.LZ4, ZstdSharp.Port, System.IO.Hashing (`XxHash3` short tag + the `Crc32` frame checksum — direct structural calls, never policy rows), Rasm (`ContentHash.Of`), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: one compression level is one row carrying its own `HeaderId` so every prior archive stays readable across the swap; a trained-dictionary row carries its dict blob, never a per-call branch; a WIDER content address is one `HashPolicy` row with a fresh `DomainId` plus an epoch-gated identity migration — the `HashDomain` header byte is the forward-compatibility law that makes the row addition non-breaking; zero new surface.
- Boundary: both hash rows are non-cryptographic identity — a security claim on either is the named defect; `Identity` pins the kernel `ContentHash.Of` as the one `ContentAddress` algorithm every snapshot identity, chunk key, and diff reads, so a 64-bit hash standing in for the content address is the deleted form; `Content` is the `XxHash3` 64-bit short tag stamped on every chunk as a bloom/sketch pre-filter ahead of the authoritative 128-bit compare — a chunk-fold datum, never a header hash domain; frame checksums are STRUCTURAL frame facts, not policy rows — the `SnapshotHeader` checksum is a direct `Crc32.HashToUInt32` over the header prefix and a compression frame's own integrity word (`ZstdSharp` `checksumFlag`) belongs to its frame, so the former five-row ladder (`Frame`/`Wide`/`FrameWide`) is the deleted enumeration of call-site facts as vocabulary; the `HashDomain` law: the header byte records `Identity.DomainId`, `Verify` hard-rejects any other domain as `RejectTier.HashDomainGap` TODAY, and a future wider address lands as one row whose new `DomainId` the ladder resolves through `ByDomainId` under its epoch gate — never a second ladder arm and never a per-artifact algorithm negotiation; the `messagepack` codec pairs with `none` because `Lz4BlockArray` owns in-codec compression (double framing is the deleted pattern), and a `Cbor`/`JsonStj` blob whose body already rode Arrow-IPC `Zstd` block compression likewise pairs with `none`; `ZstdSharp.Port`'s self-describing frame (`contentSizeFlag`/`checksumFlag`, long-distance matching, `btultra2`) is the higher-ratio path and `LZ4Pickler` the lowest-latency self-describing frame, the policy row selecting one so a payload frames exactly once and the frame checksum complements the snapshot rail's own content hash rather than replacing it; a payload outgrowing the one-shot span rides the zstd rows' `ZstdFrame.PackStream`/`UnpackStream` `CompressionStream`/`DecompressionStream` adapters — the ONE streaming residence, so whole-payload materialization is never the price of a large artifact and an LZ4 streaming sibling is the deleted parallel.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;                                 // ContentHash — the ONE kernel digest entry

// --- [TYPES] ------------------------------------------------------------------------------
// Two sibling policy vocabularies — `CompressionPolicy` and `HashPolicy` — with `ZstdFrame` co-located as
// `CompressionPolicy`'s frame pack/unpack helper (referenced only by its `zstd`/`zstd-high` rows).
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CompressionPolicy {
    public static readonly CompressionPolicy None = new("none", headerId: 0, pack: static p => p.ToArray(), unpack: static f => f.ToArray());
    public static readonly CompressionPolicy Lz4Fast = new("lz4-fast", headerId: 1, pack: static p => LZ4Pickler.Pickle(p.Span, LZ4Level.L00_FAST), unpack: static f => LZ4Pickler.Unpickle(f.Span));
    public static readonly CompressionPolicy Lz4High = new("lz4-high", headerId: 2, pack: static p => LZ4Pickler.Pickle(p.Span, LZ4Level.L09_HC), unpack: static f => LZ4Pickler.Unpickle(f.Span));
    public static readonly CompressionPolicy Zstd = new("zstd", headerId: 3, pack: static p => ZstdFrame.Pack(p.Span, level: 3, archival: false), unpack: static f => ZstdFrame.Unpack(f.Span));
    public static readonly CompressionPolicy ZstdHigh = new("zstd-high", headerId: 4, pack: static p => ZstdFrame.Pack(p.Span, level: 19, archival: true), unpack: static f => ZstdFrame.Unpack(f.Span));

    public int HeaderId { get; }
    public static bool Known(int headerId) => Items.Any(r => r.HeaderId == headerId);
    public static Option<CompressionPolicy> ByHeaderId(int headerId) => toSeq(Items).Find(r => r.HeaderId == headerId);
    [UseDelegateFromConstructor] public partial byte[] Pack(ReadOnlyMemory<byte> payload);
    [UseDelegateFromConstructor] public partial byte[] Unpack(ReadOnlyMemory<byte> framed);
}

public static class ZstdFrame {
    public static byte[] Pack(ReadOnlySpan<byte> payload, int level, bool archival) {
        using Compressor compressor = new(level);
        compressor.SetParameter(ZSTD_cParameter.ZSTD_c_contentSizeFlag, 1);
        compressor.SetParameter(ZSTD_cParameter.ZSTD_c_checksumFlag, 1);
        if (archival) {
            compressor.SetParameter(ZSTD_cParameter.ZSTD_c_enableLongDistanceMatching, 1);
            compressor.SetParameter(ZSTD_cParameter.ZSTD_c_strategy, (int)ZSTD_strategy.ZSTD_btultra2);
        }
        return compressor.Wrap(payload).ToArray();
    }
    public static byte[] Unpack(ReadOnlySpan<byte> framed) { using Decompressor decompressor = new(); return decompressor.Unwrap(framed).ToArray(); }

    // The streaming pair for payloads outgrowing the one-shot span (the partitioned-upstream folds): the
    // `CompressionStream`/`DecompressionStream` adapters carry the same frame semantics as `Wrap`/`Unwrap`,
    // so a large artifact zstd-frames without whole-payload materialization — the zstd rows are the ONE
    // streaming residence (an LZ4-row streaming sibling is the deleted parallel; the lz4 rows stay the
    // low-latency small-payload pickle).
    public static long PackStream(Stream source, Stream sink, int level) {
        using CompressionStream stream = new(sink, level);
        source.CopyTo(stream);
        return sink.Length;
    }
    public static long UnpackStream(Stream framed, Stream sink) {
        using DecompressionStream stream = new(framed);
        stream.CopyTo(sink);
        return sink.Length;
    }
}

// The two-row hash axis: `Identity` IS the kernel content address (the header `HashDomain` byte records its
// `DomainId`); `Content` is the chunk short-tag pre-filter. Frame checksums (header `Crc32`, zstd frame word)
// are direct structural calls at their frames, never rows — the deleted `Frame`/`Wide`/`FrameWide` rows
// enumerated call-site facts as vocabulary. A wider address is one new row + `DomainId` under an epoch gate.
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
    public static Option<HashPolicy> ByDomainId(byte domainId) => toSeq(Items).Find(r => r.DomainId == domainId);
}
```

## [05]-[SNAPSHOT_SPINE]

- Owner: `SnapshotHeader` is the fixed prologue and trust boundary; `SnapshotAdmission` carries schema, epoch, and allocation ceilings; `SnapshotFormat` pairs codec with compression; `SnapshotRoute` owns artifact residence, retention, lineage, size, and format policy; `RejectTier`, `CodecFault`, `SnapshotCatalogRow`, and `Snapshots` own rejection, evidence, catalog, seal, verify, and sweep.
- Entry: `Write<T>(ReceiptSinkPort, ProjectionContext, SnapshotRoute, T, Func<SnapshotCatalogRow,IO<Unit>>)` validates shape/format compatibility before encoding and consumes one route carrier. `Verify(ReadOnlySpan<byte>, SnapshotAdmission)` rejects reserved-bit drift, negative or mismatched lengths, allocation ceilings, capability gaps, epoch, and schema before decoding. `Sweep(ProjectionContext, SnapshotRoute, Seq<SnapshotCatalogRow>)` derives orphan age and directory from the route.
- Auto: the write fold derives the codec/compression ids, the schema fingerprint, the retention epoch, both lengths, the kernel `ContentHash.Of` content hash over the stored bytes, the `Crc32` header checksum, the HLC stamp, the classification, and the content-lineage rank into the catalog row; the sealed `Hash` IS the `ContentAddress` every secondary surface derives from, so a snapshot is catalog-addressable on the artifact-blob index without a parallel key and `Lineage` chains the prior edition's content address so the newest-`Count`-editions retention bound (`Version/retention`) prunes by lineage depth off the catalog row.
- Receipt: `SnapshotCatalogRow` is the durable evidence; `StoredLength`/`PlainLength` are the artifact's own sealed length fields the retention sweep reads, never a later filesystem stat.
- Packages: Rasm (`ContentHash.Of`), System.IO.Hashing (`Crc32` — the structural header checksum), NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new header capability is one flag-bit row; a new rejection cause is one `RejectTier` row breaking every ladder arm (it rails through the SAME `CodecFault.SnapshotRejected` band — the band wraps the tier, never grows a parallel case per rank); a new codec-fault class is one `CodecFault` case; one artifact kind is one catalog row value; zero new surface.
- Boundary: the single-pass seal `Clear`s the stack-allocated prefix buffer before both writes so the placeholder header is genuinely zeroed (terminally invalid magic) and no uninitialized padding byte ever persists into the reserved-gap offsets — the CRC is then computed over the same zeroed-gap layout it verifies against, deterministic across runtimes — and the full 128-bit content hash flows un-truncated through the seal tuple to the catalog `ContentAddress` (a 64-bit truncation is the deleted form that would collide distinct contents); the seal writes the zeroed placeholder header, the stored bytes, then seeks to zero and writes the final header, `Flush(flushToDisk: true)` before `File.Move` does the atomic rename, so a crash leaves the temp swept rather than a torn final; the header is the artifact's ENTIRE trust boundary and `Verify` runs the ordered ladder on raw bytes — magic/identity, layout-version ratchet, header checksum, hash-domain capability (the `HashDomain` byte must equal `HashPolicy.Identity.DomainId` — any other value is `RejectTier.HashDomainGap` until a future row's epoch gate admits it through `ByDomainId`), stored-length truncation, content hash over stored bytes through the kernel entry, codec/compression capability, then the epoch-then-fingerprint ratchet — each tier verifying before the next so corrupted or foreign input rejects before any decoder with attack surface binds, and layout-version/epoch/fingerprint are one-way ratchets so a future-layout artifact is deployment evidence rejected by one process and restored by a newer sibling; temp residue and catalog-orphaned payloads leave only through the age-gated `Sweep` (a final artifact lands on disk before its catalog `persist` `Bind` commits, so the sweep reaps only residue older than the grace window) and the swept count is the crash-loop signal.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
// The fault band derives from the KERNEL Expected (the federation base), aliased so the bare `Expected`
// names `Rasm.Domain.Expected` and never the `LanguageExt.Common.Expected` whose `(string,int,Option)`
// ctor is the deleted form. `ContentHash` is the kernel digest entry — no direct `XxHash128` call site.
using Rasm.Domain;
using Expected = Rasm.Domain.Expected;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RejectTier {
    public static readonly RejectTier Foreign = new("foreign", rank: 1);
    public static readonly RejectTier FutureLayout = new("future-layout", rank: 2);
    public static readonly RejectTier HeaderCorrupt = new("header-corrupt", rank: 3);
    public static readonly RejectTier HashDomainGap = new("hash-domain-gap", rank: 4);
    public static readonly RejectTier Truncated = new("truncated", rank: 5);
    public static readonly RejectTier SizeExceeded = new("size-exceeded", rank: 6);
    public static readonly RejectTier PayloadCorrupt = new("payload-corrupt", rank: 7);
    public static readonly RejectTier CapabilityGap = new("capability-gap", rank: 8);
    public static readonly RejectTier VersionAhead = new("version-ahead", rank: 9);

    public int Rank { get; }
    // The tier rejects through the typed CodecFault.SnapshotRejected band carrying THIS tier, not a bare Error.New —
    // a recovery reads error.IsType<CodecFault.SnapshotRejected>() and the tier off the case, never a code substring.
    public CodecFault Reject(string evidence) => new CodecFault.SnapshotRejected(this, evidence);
}

// --- [ERRORS] -----------------------------------------------------------------------------
// The codec fault band: a closed [Union] over the KERNEL `Rasm.Domain.Expected` (parameterless protected ctor;
// `Category` virtual; `Code`/`Message` inherited from `Error`). Band membership derives through the
// `Element/graph#FAULT_TABLES` registry row — `Code => FaultBand.Codec + n`, `SnapshotRejected` projecting
// `+10 + Tier.Rank` off the carried `RejectTier` (the band WRAPS the tier, never re-enumerates the tier
// ranks as union cases) — so the typed case lifts BARE onto `Fin<T>`/`Validation<Error,T>` with no `.ToError()` hop
// and a recovery reads `error.IsType<CodecFault.SnapshotRejected>()` / `error.HasCode(8326)` /
// `error.Category()`, never the bare `Error.New(8310/8330)` a codec/reassembly miss would otherwise spell.
// No `[GenerateUnionOps]` — the kernel union-ops generator is strictly opt-in.
[Union]
public abstract partial record CodecFault : Expected, IValidationError<CodecFault> {
    private CodecFault() : base() { }
    public sealed record NoMutualCodec(string Surface) : CodecFault;
    public sealed record SnapshotRejected(RejectTier Tier, string Evidence) : CodecFault;
    public sealed record ReassemblyDrift(UInt128 Expected, UInt128 Actual) : CodecFault;
    public sealed record ChunkManifestRejected(string Detail) : CodecFault;

    public override int Code => FaultBand.Codec + Switch(
        noMutualCodec:    static _ => 0,
        snapshotRejected: static c => 10 + c.Tier.Rank,
        reassemblyDrift:  static _ => 20,
        chunkManifestRejected: static _ => 21);

    public override string Message => Switch(
        noMutualCodec:    static c => $"<codec-no-mutual:{c.Surface}>",
        snapshotRejected: static c => $"<snapshot-{c.Tier.Key}:{c.Evidence}>",
        reassemblyDrift:  static c => $"<chunk-reassembly-drift:{c.Expected:X32}!={c.Actual:X32}>",
        chunkManifestRejected: static c => $"<chunk-manifest:{c.Detail}>");

    public override string Category => Switch(
        noMutualCodec:    static _ => "Negotiate",
        snapshotRejected: static _ => "Snapshot",
        reassemblyDrift:  static _ => "Reassembly",
        chunkManifestRejected: static _ => "Reassembly");

    public static CodecFault Create(string message) => new NoMutualCodec(message);
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct SnapshotHeader(
    uint Magic, byte Version, byte HashDomain, int CodecId, int CompressionId,
    ulong SchemaFingerprint, ulong Epoch, long PlainLength, long StoredLength, UInt128 ContentHash, uint Checksum) {
    public const int Size = 72;
    public const int HashOffset = Size - 20;
    public const int ChecksumOffset = Size - 4;
    public const uint MagicValue = 0x504E5352;     // RSNP little-endian
    public const byte LayoutVersion = 1;

    public static SnapshotHeader Seal(SnapshotCodec codec, CompressionPolicy compression, ulong schemaFingerprint, ulong epoch, long plainLength, ReadOnlySpan<byte> stored) {
        Span<byte> prefix = stackalloc byte[Size];
        prefix.Clear();
        SnapshotHeader draft = new(MagicValue, LayoutVersion, HashPolicy.Identity.DomainId, codec.HeaderId, compression.HeaderId,
            schemaFingerprint, epoch, plainLength, stored.Length, Rasm.Domain.ContentHash.Of(stored), Checksum: 0u);
        draft.WriteFields(prefix);
        return draft with { Checksum = Crc32.HashToUInt32(prefix[..ChecksumOffset]) };
    }

    public void Write(Span<byte> destination) { WriteFields(destination); BinaryPrimitives.WriteUInt32LittleEndian(destination[ChecksumOffset..], Checksum); }

    private void WriteFields(Span<byte> d) {
        BinaryPrimitives.WriteUInt32LittleEndian(d, Magic);
        (d[4], d[5]) = (Version, HashDomain);
        BinaryPrimitives.WriteInt32LittleEndian(d[8..], CodecId);
        BinaryPrimitives.WriteInt32LittleEndian(d[12..], CompressionId);
        BinaryPrimitives.WriteUInt64LittleEndian(d[20..], SchemaFingerprint);
        BinaryPrimitives.WriteUInt64LittleEndian(d[28..], Epoch);
        BinaryPrimitives.WriteInt64LittleEndian(d[36..], PlainLength);
        BinaryPrimitives.WriteInt64LittleEndian(d[44..], StoredLength);
        BinaryPrimitives.WriteUInt128LittleEndian(d[HashOffset..ChecksumOffset], ContentHash);
    }
}

public sealed record SnapshotCatalogRow(
    Guid Id, string Kind, SnapshotCodec Codec, CompressionPolicy Compression, ContentAddress Hash,
    long PlainLength, long StoredLength, ulong SchemaFingerprint, ulong Epoch, Option<ContentAddress> Lineage,
    string RetentionClass, DataClassification Classification, Instant HlcPhysical, ulong HlcLogical, Guid Correlation) {
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

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Snapshots {
    public const string Suffix = ".rsnp";

    public static Fin<SnapshotHeader> Verify(ReadOnlySpan<byte> artifact, SnapshotAdmission admission) =>
        artifact.Length < SnapshotHeader.Size || BinaryPrimitives.ReadUInt32LittleEndian(artifact) != SnapshotHeader.MagicValue
            ? RejectTier.Foreign.Reject(artifact.Length < SnapshotHeader.Size ? "headerless" : "magic")
        : artifact[4] > SnapshotHeader.LayoutVersion ? RejectTier.FutureLayout.Reject($"{artifact[4]}>{SnapshotHeader.LayoutVersion}")
        : BinaryPrimitives.ReadUInt32LittleEndian(artifact[SnapshotHeader.ChecksumOffset..]) != Crc32.HashToUInt32(artifact[..SnapshotHeader.ChecksumOffset]) ? RejectTier.HeaderCorrupt.Reject("crc")
        : artifact[6] != 0 || artifact[7] != 0 || BinaryPrimitives.ReadUInt32LittleEndian(artifact[16..]) != 0 ? RejectTier.HeaderCorrupt.Reject("reserved")
        : artifact[5] != HashPolicy.Identity.DomainId ? RejectTier.HashDomainGap.Reject($"domain:{artifact[5]}")
        : BinaryPrimitives.ReadInt64LittleEndian(artifact[36..]) < 0 || BinaryPrimitives.ReadInt64LittleEndian(artifact[44..]) < 0 || BinaryPrimitives.ReadInt64LittleEndian(artifact[44..]) != artifact.Length - SnapshotHeader.Size ? RejectTier.Truncated.Reject($"{artifact.Length - SnapshotHeader.Size}")
        : BinaryPrimitives.ReadInt64LittleEndian(artifact[36..]) > admission.MaxPlainLength || BinaryPrimitives.ReadInt64LittleEndian(artifact[44..]) > admission.MaxStoredLength ? RejectTier.SizeExceeded.Reject($"{BinaryPrimitives.ReadInt64LittleEndian(artifact[36..])}/{BinaryPrimitives.ReadInt64LittleEndian(artifact[44..])}")
        : Rasm.Domain.ContentHash.Of(artifact[SnapshotHeader.Size..]) != BinaryPrimitives.ReadUInt128LittleEndian(artifact[SnapshotHeader.HashOffset..SnapshotHeader.ChecksumOffset]) ? RejectTier.PayloadCorrupt.Reject("content-hash")
        : !SnapshotCodec.Known(BinaryPrimitives.ReadInt32LittleEndian(artifact[8..])) || !CompressionPolicy.Known(BinaryPrimitives.ReadInt32LittleEndian(artifact[12..])) ? RejectTier.CapabilityGap.Reject("codec")
        : BinaryPrimitives.ReadUInt64LittleEndian(artifact[28..]) > admission.Epoch ? RejectTier.VersionAhead.Reject($"epoch:{BinaryPrimitives.ReadUInt64LittleEndian(artifact[28..])}")
        : BinaryPrimitives.ReadUInt64LittleEndian(artifact[20..]) != admission.SchemaFingerprint ? RejectTier.VersionAhead.Reject("fingerprint")
        : Fin.Succ(Read(artifact));

    // The sink payload is the sealed TYPED `SnapshotHeader` (source-gen-registered on `ElementJson`) —
    // a bare tuple cannot cross the strict source-gen resolver, so the header IS the seal evidence wire shape.
    public static IO<SnapshotCatalogRow> Write<T>(ReceiptSinkPort sink, ProjectionContext frame, SnapshotRoute route, T value, Func<SnapshotCatalogRow, IO<Unit>> persist) =>
        !route.Format.Admits(typeof(T))
            ? IO.fail<SnapshotCatalogRow>(new CodecFault.NoMutualCodec($"snapshot:{typeof(T).FullName}"))
            : IO.lift(() => route.Format.Codec.Serialize(typeof(T), value))
            .Bind(encoded => encoded.LongLength > route.MaxPlainLength
                ? IO.fail<(Guid Id, SnapshotHeader Header)>(RejectTier.SizeExceeded.Reject($"plain:{encoded.LongLength}>{route.MaxPlainLength}"))
                : IO.lift(() => Seal(route, Guid.CreateVersion7(), encoded)))
            // Port edge conversion: ReceiptSinkPort.Send requires CorrelationId and TenantContext — the raw
            // Guid/UInt128 frame values convert HERE exactly as the timetravel Folded edge does.
            .Bind(file => sink.Send(CorrelationId.Create(frame.Correlation), TenantContext.Current, "Rasm.Persistence", route.Kind, JsonSerializer.SerializeToElement(file.Header, ElementJson.Options))
                .Map(envelope => new SnapshotCatalogRow(file.Id, route.Kind, route.Format.Codec, route.Format.Compression, ContentAddress.Of(file.Header.ContentHash), file.Header.PlainLength, file.Header.StoredLength, route.SchemaFingerprint, route.Epoch, route.Lineage, route.RetentionClass, route.Classification, envelope.Physical, envelope.Logical, frame.Correlation)))
            .Bind(row => persist(row).Map(_ => row));

    public static IO<Seq<string>> Sweep(ProjectionContext frame, SnapshotRoute route, Seq<SnapshotCatalogRow> catalog) =>
        IO.lift(() => (Now: frame.Now(), Files: toSeq(Directory.EnumerateFiles(route.Directory))))
            .Map(scan => scan.Files.Filter(file => !catalog.Exists(row => string.Equals(Path.GetFileName(file), $"{row.Id}{Suffix}", StringComparison.Ordinal)) && scan.Now - Instant.FromDateTimeUtc(File.GetLastWriteTimeUtc(file)) >= route.OrphanAge))
            .Bind(static orphans => orphans.TraverseM(static file => IO.lift(() => { File.Delete(file); return file; })).As());

    static (Guid Id, SnapshotHeader Header) Seal(SnapshotRoute route, Guid id, byte[] encoded) {
        byte[] packed = route.Format.Compression.Pack(encoded);
        if (packed.LongLength > route.MaxStoredLength) { throw RejectTier.SizeExceeded.Reject($"stored:{packed.LongLength}>{route.MaxStoredLength}").ToException(); }
        SnapshotHeader header = SnapshotHeader.Seal(route.Format.Codec, route.Format.Compression, route.SchemaFingerprint, route.Epoch, encoded.LongLength, packed);
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
        return (id, header);
    }

    static SnapshotHeader Read(ReadOnlySpan<byte> a) =>
        new(BinaryPrimitives.ReadUInt32LittleEndian(a), a[4], a[5], BinaryPrimitives.ReadInt32LittleEndian(a[8..]), BinaryPrimitives.ReadInt32LittleEndian(a[12..]),
            BinaryPrimitives.ReadUInt64LittleEndian(a[20..]), BinaryPrimitives.ReadUInt64LittleEndian(a[28..]), BinaryPrimitives.ReadInt64LittleEndian(a[36..]), BinaryPrimitives.ReadInt64LittleEndian(a[44..]),
            BinaryPrimitives.ReadUInt128LittleEndian(a[SnapshotHeader.HashOffset..SnapshotHeader.ChecksumOffset]), BinaryPrimitives.ReadUInt32LittleEndian(a[SnapshotHeader.ChecksumOffset..]));
}
```

## [06]-[CONTENT_CHUNKING]

- Owner: `ChunkPolicy` the FastCDC min/avg/max size axis; `ContentChunk` the content-keyed chunk record carrying its 128-bit content address, source offset, length, and the `XxHash3` short tag; `ChunkManifest` the per-payload ordered chunk-key sequence; `ContentChunker` the static surface owning the FastCDC cut, the per-chunk content-key derivation, the manifest fold, and the cross-payload dedup projection.
- Entry: `Chunk` cuts and addresses bytes; `Novel` probes local or peer chunk indexes only; `Reassemble` first proves bounded total length, contiguous offsets, positive chunk lengths, fetched length, and each chunk address, then proves the whole-artifact address.
- Auto: the content-defined boundary is the FastCDC normalized gear-hash cut so an insertion that shifts every fixed-window boundary leaves the content-defined boundaries stable past the edit and a small change to a large geometry blob re-stores only the changed chunks; each chunk's content key is the kernel `ContentHash.Of` (the `HashPolicy.Identity` row) so an identical chunk across two snapshots or two peers dedups; the short tag is `HashPolicy.Content` (`XxHash3`) so `Novel` probes `mayHold(ShortTag)` before `holds(ContentKey)` on a hot re-store path.
- Receipt: a chunked store rides `store.chunk.split` carrying chunk and novel-chunk counts; a dedup hit rides `store.chunk.dedup` carrying reused-chunk and reused-byte counts.
- Packages: FastCDC.Net, Rasm (`ContentHash.Of`), System.IO.Hashing (`XxHash3` short tag), LanguageExt.Core, BCL inbox.
- Growth: a new chunk-size profile is one `ChunkPolicy` row; a payload that outgrows the one-shot in-memory span composes the kernel streaming-identity member (`XxHash128` `Append` + `GetCurrentHashAsUInt128`, seed zero — the `#CONTENT_ADDRESS` consumer contract) the moment it lands, replacing only the whole-payload digest line; zero new surface — a fixed-window framing, a per-edit full re-store, or a second content-defined chunker is the deleted form.
- Boundary: chunk membership proves only local or peer chunk residence. Remote object-store residence is the provider's exact-object conditional seal; no chunk index can skip or synthesize provider objects. Multipart windows may preserve whole FastCDC cuts as part boundaries without treating their membership as object evidence.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;                                 // ContentHash — the ONE kernel digest entry

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ChunkPolicy {
    public static readonly ChunkPolicy Artifact = new("artifact", min: 16 * 1024u, avg: 64 * 1024u, max: 256 * 1024u);
    public static readonly ChunkPolicy Small = new("small", min: 2 * 1024u, avg: 8 * 1024u, max: 32 * 1024u);

    public uint Min { get; }
    public uint Avg { get; }
    public uint Max { get; }
    private ChunkPolicy(string key, uint min, uint avg, uint max) : this(key) => (Min, Avg, Max) = (min, avg, max);

    // The one chunker mint: a FastCdc instance is stateful one-shot, so every (re-)chunk mints fresh HERE —
    // the policy row owns the ctor spelling and a call-site `new FastCdc(...)` is the deleted scatter.
    public FastCdc Over(byte[] source) => new(source, Min, Avg, Max, eof: true);
}

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct ContentChunk(UInt128 ContentKey, ulong ShortTag, uint Offset, int Length);

public readonly record struct ChunkManifest(ContentAddress WholeArtifact, long Length, Seq<ContentChunk> Chunks);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class ContentChunker {
    public static ChunkManifest Chunk(ChunkPolicy policy, ReadOnlyMemory<byte> payload) {
        byte[] source = payload.ToArray();
        // FastCdc `Chunk` exposes `Offset`/`Length` as `uint`; the span slice and the `int`-shaped `ContentChunk.Length`
        // take the explicit `int` cast (a >2-GiB payload partitions upstream, so the narrowing never truncates a live chunk).
        Seq<ContentChunk> chunks = toSeq(policy.Over(source).GetChunks()
            .Select(cut => {
                ReadOnlySpan<byte> span = source.AsSpan((int)cut.Offset, (int)cut.Length);
                return new ContentChunk(ContentHash.Of(span), XxHash3.HashToUInt64(span), cut.Offset, (int)cut.Length);
            }));
        return new ChunkManifest(ContentAddress.Of(ContentHash.Of(source)), source.LongLength, chunks);
    }

    public static Seq<ContentChunk> Novel(ChunkManifest manifest, Func<ulong, bool> mayHold, Func<UInt128, bool> holds) =>
        manifest.Chunks.Filter(chunk => !mayHold(chunk.ShortTag) || !holds(chunk.ContentKey));

    public static Fin<ReadOnlyMemory<byte>> Reassemble(ChunkManifest manifest, Func<UInt128, ReadOnlyMemory<byte>> fetch) {
        if (manifest.Length is < 0 or > int.MaxValue) { return Fin.Fail<ReadOnlyMemory<byte>>(new CodecFault.ChunkManifestRejected($"length:{manifest.Length}")); }
        ArrayBufferWriter<byte> buffer = new((int)manifest.Length);
        Fin<long> filled = manifest.Chunks.Fold(
            Fin.Succ(0L),
            (state, chunk) => state.Bind(offset => {
                if (chunk.Offset != offset || chunk.Length <= 0 || offset + chunk.Length > manifest.Length) {
                    return Fin.Fail<long>(new CodecFault.ChunkManifestRejected($"span:{chunk.Offset}+{chunk.Length}@{offset}/{manifest.Length}"));
                }
                ReadOnlyMemory<byte> payload = fetch(chunk.ContentKey);
                if (payload.Length != chunk.Length || ContentHash.Of(payload.Span) != chunk.ContentKey) {
                    return Fin.Fail<long>(new CodecFault.ChunkManifestRejected($"chunk:{chunk.ContentKey:X32}/{payload.Length}"));
                }
                buffer.Write(payload.Span);
                return Fin.Succ(offset + chunk.Length);
            }));
        return filled.Bind(length => {
            if (length != manifest.Length) { return Fin.Fail<ReadOnlyMemory<byte>>(new CodecFault.ChunkManifestRejected($"terminal:{length}!={manifest.Length}")); }
            UInt128 actual = ContentHash.Of(buffer.WrittenSpan);
            return actual == manifest.WholeArtifact.Value
                ? Fin.Succ((ReadOnlyMemory<byte>)buffer.WrittenMemory)
                : Fin.Fail<ReadOnlyMemory<byte>>(new CodecFault.ReassemblyDrift(manifest.WholeArtifact.Value, actual));
        });
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
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

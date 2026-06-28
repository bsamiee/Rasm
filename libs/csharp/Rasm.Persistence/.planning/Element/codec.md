# [PERSISTENCE_ELEMENT_CODEC]

Rasm.Persistence encodes every durable `ElementGraph`, `GraphDelta`, and geometry blob through one `SnapshotCodec` axis paired with the `CompressionPolicy` and `HashPolicy` axes, derives the one `ContentAddress` by composing the kernel seed-zero `XxHash128` over canonical bytes (no second hasher), seals every file-resident artifact under one fixed-offset `SnapshotHeader` that is the artifact's entire trust boundary, verifies every read through one ordered `RejectTier` ladder that rejects before any decoder with attack surface runs, and splits opaque payload bytes into FastCDC content-defined chunks for cross-snapshot dedup. Codec, compression, and hash variance are delegate rows on string-keyed smart enums; the canonical CBOR row's deterministic map-key order makes the encoded bytes content-stable for the `ContentAddress` a schemaless MessagePack body cannot guarantee; the MessagePack row is the Marten-event and cache wire; JSON-STJ is the inspector/web wire; file-raw is the geometry-blob passthrough. The header records what WAS done — observed compression, both plain and stored length, the content hash over stored bytes, the schema fingerprint, the retention epoch, and its own checksum — never what was requested, so a torn or foreign artifact self-rejects by offset before the codec machinery binds. `ContentAddress` (the seam `[ValueObject<UInt128>]`), `ElementGraph`, `GraphDelta`, and `Node` arrive settled from `Rasm.Element`; `ClockPolicy`, `ReceiptSinkPort`, `DataClassification`, and `CorrelationId` arrive from `Rasm.AppHost`.

## [01]-[INDEX]

- [01]-[CODEC_AXIS]: four codec rows, the package wire context, generated converter admission, and the AOT resolver landmark.
- [02]-[CONTENT_ADDRESS]: the kernel-composed `XxHash128` content address, canonical-byte projection, and the measure-quantized node hash.
- [03]-[COMPRESSION_HASHING]: compression rows, hash rows, framing routes, and identity values.
- [04]-[SNAPSHOT_SPINE]: fixed-offset header trust boundary, the tier rejection ladder, the single-pass atomic write fold, and the orphan sweep.
- [05]-[CONTENT_CHUNKING]: FastCDC content-defined chunk boundaries and per-chunk content-key dedup.

## [02]-[CODEC_AXIS]

- Owner: `SnapshotCodec` `[SmartEnum<string>]` under the `CodecKeyPolicy` ordinal accessor; `ElementJson` the package `JsonSerializerContext` partial joining the suite STJ merge; `InstantFormatter` the one primitive-mapped NodaTime MessagePack formatter; `WireSurface` the wire-surface vocabulary each codec admits through its frozen `Membership` set so content negotiation is the codec rows a surface admits, never a parallel format enum; `GeoJsonProjection` the one `GeoJsonConverterFactory` admission; `PersistenceResolver` the AOT MessagePack resolver landmark.
- Cases: 4 codec rows — `json-stj`, `messagepack`, `file-raw`, `cbor`; 4 wire surfaces — `snapshot`, `cache`, `sync`, `web`.
- Entry: `public partial byte[] Serialize(Type shape, object? value)` is the pure byte transform dispatching shape-discriminated through source-generated metadata; `public static Fin<SnapshotCodec> Negotiate(WireSurface surface, Seq<string> accepted)` resolves the highest mutually-supported codec a consumer admits.
- Auto: registering `ThinktectureJsonConverterFactory` and `ThinktectureMessageFormatterResolver.Instance` once derives every `[ValueObject]`/`[SmartEnum]`/`[Union]` converter and formatter, so a `NodeId`/`ContentAddress`/`Discipline` crosses both the Marten-event STJ wire and the MessagePack cache wire as its bare key with zero hand-written codec; `GeoJsonProjection` admits one `GeoJsonConverterFactory` deriving the GeoJSON projection of every `NetTopologySuite` geometry, feature, and attribute table the `Coverage`/`GeoReference` nodes carry; `Marten.UseSystemTextJsonForSerialization(ElementJson.Options)` binds the `json-stj` row's options as the event-store serializer so a stored `GraphEvent` and an inspector projection share one converter set.
- Receipt: codec negotiation rides `store.codec.negotiate` carrying the surface and the resolved codec; the AOT resolver swap rides no receipt (it is a build-time selection).
- Packages: MessagePack, MessagePackAnalyzer, System.Formats.Cbor, Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, Thinktecture.Runtime.Extensions.MessagePack, NetTopologySuite.IO.GeoJSON4STJ, NodaTime, NodaTime.Serialization.SystemTextJson, BCL inbox.
- Growth: one codec is one row; a new wire record is one `[JsonSerializable]` row on `ElementJson` plus one MessagePack union tag when polymorphic; the AOT landmark swaps the runtime `CompositeResolver.Create` chain for the generated `PersistenceResolver` under a published-AOT build as one `Binary`/`Foreign` field selection, never a second codec; zero new surface.
- Boundary: artifact-kind-to-codec residence is fixed at write — a second codec on one kind is a conflict, not a fallback; the `messagepack` row is the Marten cache/sync wire and pairs with the `none` compression row because `Lz4BlockArray` owns compression in-codec (double framing is the deleted pattern); the `cbor` row is the self-describing IETF blob codec whose `CborConformanceMode.Canonical` deterministic map-key order makes the bytes content-stable for the `ContentAddress` (the guarantee a schemaless MessagePack body cannot give across insertion order) and whose `Strict` bounded reader guards an untrusted egress frame against a depth/length bomb, so a structured self-describing blob routes through `Cbor` while an evolving typed record stays `messagepack`; the `json-stj` row is the inspector/web wire and the Marten event-store serializer; the `file-raw` row is the geometry-blob passthrough that never re-frames; the restore lane reads MessagePack under `MessagePackSecurity.UntrustedData` because a stored blob crossed a rest boundary, the write lane keeps the trusted default; MemoryPack and protobuf snapshot encodings stay rejected — proto owns RPC payloads only.

```csharp signature
public sealed class CodecKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;
    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<CodecKeyPolicy, string>]
[KeyMemberComparer<CodecKeyPolicy, string>]
public sealed partial class WireSurface {
    public static readonly WireSurface Snapshot = new("snapshot");
    public static readonly WireSurface Cache = new("cache");
    public static readonly WireSurface Sync = new("sync");
    public static readonly WireSurface Web = new("web");
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    RespectNullableAnnotations = true,
    RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(GraphEvent))]
[JsonSerializable(typeof(SnapshotCatalogRow))]
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
[KeyMemberEqualityComparer<CodecKeyPolicy, string>]
[KeyMemberComparer<CodecKeyPolicy, string>]
public sealed partial class SnapshotCodec {
    public static readonly SnapshotCodec JsonStj = new("json-stj", headerId: 1, negotiationRank: 1, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot, WireSurface.Web]),
        serialize: static (shape, value) => JsonSerializer.SerializeToUtf8Bytes(value, shape, ElementJson.Options),
        deserialize: static (shape, payload) => JsonSerializer.Deserialize(payload.Span, shape, ElementJson.Options));
    public static readonly SnapshotCodec MessagePackBinary = new("messagepack", headerId: 2, negotiationRank: 3, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot, WireSurface.Cache, WireSurface.Sync, WireSurface.Web]),
        serialize: static (shape, value) => MessagePackSerializer.Serialize(shape, value, Binary),
        deserialize: static (shape, payload) => MessagePackSerializer.Deserialize(shape, payload, Binary));
    public static readonly SnapshotCodec FileRaw = new("file-raw", headerId: 3, negotiationRank: 0, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot]),
        serialize: static (_, value) => (byte[])value!,
        deserialize: static (_, payload) => payload.ToArray());
    public static readonly SnapshotCodec Cbor = new("cbor", headerId: 4, negotiationRank: 2, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot, WireSurface.Sync, WireSurface.Web]),
        serialize: static (_, value) => CborBlob.Encode((ReadOnlyMemory<byte>)value!),
        deserialize: static (_, payload) => CborBlob.Decode(payload));

    public int HeaderId { get; }
    public int NegotiationRank { get; }
    public FrozenSet<WireSurface> Membership { get; }

    public static bool Known(int headerId) => Items.Any(c => c.HeaderId == headerId);
    public static Option<SnapshotCodec> ByHeaderId(int headerId) => Items.Find(c => c.HeaderId == headerId).ToOption();
    public bool Serves(WireSurface surface) => Membership.Contains(surface);

    public static Fin<SnapshotCodec> Negotiate(WireSurface surface, Seq<string> accepted) =>
        Items.Filter(c => c.Serves(surface)).OrderByDescending(static c => c.NegotiationRank).ToSeq()
            .Find(c => accepted.Contains(c.Key))
            .Match(Some: Fin.Succ, None: () => Fin.Fail<SnapshotCodec>(Error.New(8310, $"<codec-no-mutual:{surface.Key}>")));

    [UseDelegateFromConstructor] public partial byte[] Serialize(Type shape, object? value);
    [UseDelegateFromConstructor] public partial object? Deserialize(Type shape, ReadOnlyMemory<byte> payload);

    public static readonly MessagePackSerializerOptions Binary = MessagePackSerializerOptions.Standard
        .WithResolver(CompositeResolver.Create([InstantFormatter.Instance], [ThinktectureMessageFormatterResolver.Instance, GeneratedMessagePackResolver.Instance, StandardResolver.Instance]))
        .WithCompression(MessagePackCompression.Lz4BlockArray);
    public static readonly MessagePackSerializerOptions Foreign = Binary.WithSecurity(MessagePackSecurity.UntrustedData.WithMaximumObjectGraphDepth(64));
}

// Canonical CBOR blob — `Canonical` reorders map keys + emits shortest-form integers so the bytes are
// content-stable for the `ContentAddress`; `Strict` bounds an untrusted egress frame against a depth bomb,
// and `Decode` rejects any trailing bytes after the byte string so a smuggled-suffix frame self-faults.
public static class CborBlob {
    public const int MaxDepth = 64;
    public static byte[] Encode(ReadOnlyMemory<byte> payload) {
        var writer = new CborWriter(CborConformanceMode.Canonical);
        writer.WriteTag(CborTag.SelfDescribeCbor);
        writer.WriteByteString(payload.Span);
        return writer.Encode();
    }
    public static byte[] Decode(ReadOnlyMemory<byte> payload) {
        var reader = new CborReader(payload, CborConformanceMode.Strict);
        if (reader.PeekState() == CborReaderState.Tag && reader.PeekTag() == CborTag.SelfDescribeCbor) reader.ReadTag();
        if (reader.CurrentDepth > MaxDepth) throw new CborContentException($"<cbor-depth:{reader.CurrentDepth}>");
        var bytes = reader.ReadByteString();
        return reader.BytesRemaining == 0 ? bytes : throw new CborContentException($"<cbor-trailing:{reader.BytesRemaining}>");
    }
}

public sealed record GeoJsonProjection(GeometryFactory Geometry, bool WriteBoundingBox = true, string IdProperty = GeoJsonConverterFactory.DefaultIdPropertyName, bool AllowModifyingAttributes = false) {
    public static readonly GeoJsonProjection Default = new(GeometryFactory.Default);
    public GeoJsonConverterFactory Factory => new(Geometry, WriteBoundingBox, IdProperty, RingOrientationOption.EnforceRfc9746, AllowModifyingAttributes);
}

[GeneratedMessagePackResolver] public partial class GeneratedMessagePackResolver;
[CompositeResolver(typeof(ThinktectureMessageFormatterResolver), typeof(GeneratedMessagePackResolver))] public partial class PersistenceResolver;
```

## [03]-[CONTENT_ADDRESS]

- Owner: `ContentAddress` the seam `[ValueObject<UInt128>]` content key every snapshot identity, dedup probe, diff, and AS-OF cut reads; `NodeHash` the thin Persistence composition over the seam `ContentAddress` owner — the id-EXCLUDED node content key (`OfNode`, the diff/dedup key) and the order-independent graph address (`OfGraph`, delegated to the seam `ContentAddress.OfGraph`), so the diff `ContentBytes` and the `NodeId` content key share the ONE seam byte projection and no second graph-address ordering exists.
- Cases: a graph content address IS the seam `ContentAddress.OfGraph` (sorted node addresses plus sorted edge canonical bytes); a node content address is the seam `ContentAddress.Of` over the node's `ToCanonicalBytes()` (fixed IEEE-754 LE bits with `-0.0→0.0` and `NaN→canonical`, measure quantization to the header tolerance, explicit attribute-order canon); delta keying rides the `Version/commits#CRDT_WIRE` `CrdtWire.ContentKey`, never a second delta hasher.
- Entry: `public static ContentAddress OfNode(Node node, double tolerance)` composes the seam `ContentAddress.Of` over the node's tolerance-quantized canonical bytes (id-excluded, so two occurrences of identical content collide for dedup); `public static ContentAddress OfGraph(ElementGraph graph)` delegates to the seam `ContentAddress.OfGraph` so the topology memo key and the snapshot graph identity are ONE algorithm.
- Auto: the content address is the kernel's ONE algorithm — `System.IO.Hashing.XxHash128` seeded zero, the same digest the seam's `ContentAddress` value-object wraps and the `Rasm` kernel mints for geometry by content-hash — so a snapshot, a chunk, a diff, and a federation key all read one 128-bit address and a second hasher is the deleted form; the canonical-byte projection rides the seam `Node.ToCanonicalBytes()` instance member (the mint stays static-abstract on the union, verification/re-hash go through the instance) so the float-bearing parity corpus (`Version/commits#CRDT_WIRE`) pins the layout cross-runtime; measure quantization to `Header.Tolerance` happens once before hashing so two geometrically-equal nodes within tolerance share one address.
- Receipt: content addressing rides no standalone receipt (it folds into the snapshot/diff/dedup receipts that carry the address).
- Packages: System.IO.Hashing (`XxHash128.HashToUInt128`), Rasm.Element (`ContentAddress.Of`/`ContentAddress.OfGraph`/`Node.ToCanonicalBytes`/`ElementGraph`), System.Buffers.Binary, BCL inbox.
- Growth: a wider content address is one `HashPolicy` row plus an epoch-gated identity migration; a new canonical-byte rule is one clause on the seam `Node.ToCanonicalBytes()` (seam-owned, this page composes); zero new surface — a second hasher, a `GetHashCode`-based address, or a per-surface key respelling is the deleted form because the kernel owns the one seed-zero `XxHash128` and every durable identity composes it.
- Boundary: the `ContentAddress` is non-cryptographic identity — a tamper or security claim on it is the named defect (the `Version/provenance#ATTESTED_LEDGER` `AttestedEntry` owns tamper-evidence); the canonical byte projection is shared between the `NodeId` content hash and the diff `ContentBytes` so a node that did not change is byte-identical and the structural diff prunes it; the kernel seed convention (`SeedOrigin = Guid.Empty`, seed-zero content) is ground truth and the literal digest values stamp on the host-validation pass, never an un-run asserted value; the graph address IS the seam `ContentAddress.OfGraph` order-independent fold (sorted node addresses + sorted edge canonical bytes), composed once so the topology memo key and the snapshot graph identity never fork into two Persistence-local orderings.

```csharp signature
public static class NodeHash {
    // The id-EXCLUDED node content key — two occurrences with identical content share it (the diff/dedup
    // key), distinct from the seam `ContentAddress.Of(node, tolerance)` which prepends the id for occurrence
    // identity. Composes the seam `ContentAddress.Of(bytes)` over the node's tolerance-quantized canonical
    // bytes; no second hasher.
    public static ContentAddress OfNode(Node node, double tolerance) =>
        ContentAddress.Of(node.ToCanonicalBytes(tolerance).Span);

    // The order-INDEPENDENT graph content address IS the seam owner's one fold (sorted node addresses +
    // sorted edge canonical bytes). Composed, never a second Persistence-local node/edge ordering — a
    // divergent fold would address one graph two ways and fork the topology memo from the snapshot identity.
    public static ContentAddress OfGraph(ElementGraph graph) => ContentAddress.OfGraph(graph);
}
```

| [INDEX] | [POLICY]              | [VALUE]                                | [BINDING]                                                  |
| :-----: | :-------------------- | :------------------------------------- | :-------------------------------------------------------- |
|  [01]   | content algorithm     | kernel seed-zero `XxHash128`           | one hasher; snapshot/chunk/diff/federation share it       |
|  [02]   | canonical bytes       | seam `Node.ToCanonicalBytes(tolerance)`| float canon + measure quantization; shared with diff      |
|  [03]   | measure quantization  | `Header.Tolerance` before hashing      | two within-tolerance nodes share one address              |
|  [04]   | identity claim        | non-cryptographic                      | tamper-evidence is `Version/provenance#ATTESTED_LEDGER`   |

## [04]-[COMPRESSION_HASHING]

- Owner: `CompressionPolicy` and `HashPolicy` `[SmartEnum<string>]` row families under the `CodecKeyPolicy` ordinal accessor.
- Cases: 5 compression rows — `none`, `lz4-fast`, `lz4-high`, `zstd`, `zstd-high`; 5 hash rows — `Content` (`XxHash3`), `Identity` (`XxHash128`), `Frame` (`Crc32`), `Wide` (`XxHash64`), `FrameWide` (`Crc64`).
- Entry: `public partial byte[] Pack(ReadOnlyMemory<byte> payload)` is the pure byte transform; `public partial UInt128 Compute(ReadOnlyMemory<byte> payload)` is the row's hash.
- Packages: K4os.Compression.LZ4, ZstdSharp.Port, System.IO.Hashing, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: one compression level or hash algorithm is one row carrying its own `HeaderId` so every prior archive stays readable across the swap; a trained-dictionary row carries its dict blob, never a per-call branch; zero new surface.
- Boundary: every hash row is non-cryptographic identity — a security claim on any row is the named defect; `Identity` pins `XxHash128` as the one `ContentAddress` algorithm every snapshot identity, chunk key, and diff reads, so a 64-bit hash standing in for the content address is the deleted form; `Content` is the `XxHash3` 64-bit short tag stamped on every chunk as a bloom/sketch pre-filter ahead of the authoritative 128-bit compare; the `messagepack` codec pairs with `none` because `Lz4BlockArray` owns in-codec compression (double framing is the deleted pattern), and a `Cbor`/`JsonStj` blob whose body already rode Arrow-IPC `Zstd` block compression likewise pairs with `none`; `ZstdSharp.Port`'s self-describing frame (`contentSizeFlag`/`checksumFlag`, long-distance matching, `btultra2`) is the higher-ratio path and `LZ4Pickler` the lowest-latency self-describing frame, the policy row selecting one so a payload frames exactly once and the frame checksum complements the snapshot rail's own `XxHash128` rather than replacing it; the header `HashDomain` byte records `Identity.DomainId` so the content-hash algorithm self-describes and `Verify` resolves it through `ByDomainId`, a future wider address being one row plus an epoch-gated migration, never a second ladder arm.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<CodecKeyPolicy, string>]
[KeyMemberComparer<CodecKeyPolicy, string>]
public sealed partial class CompressionPolicy {
    public static readonly CompressionPolicy None = new("none", headerId: 0, pack: static p => p.ToArray(), unpack: static f => f.ToArray());
    public static readonly CompressionPolicy Lz4Fast = new("lz4-fast", headerId: 1, pack: static p => LZ4Pickler.Pickle(p.Span, LZ4Level.L00_FAST), unpack: static f => LZ4Pickler.Unpickle(f.Span));
    public static readonly CompressionPolicy Lz4High = new("lz4-high", headerId: 2, pack: static p => LZ4Pickler.Pickle(p.Span, LZ4Level.L09_HC), unpack: static f => LZ4Pickler.Unpickle(f.Span));
    public static readonly CompressionPolicy Zstd = new("zstd", headerId: 3, pack: static p => ZstdFrame.Pack(p.Span, level: 3, archival: false), unpack: static f => ZstdFrame.Unpack(f.Span));
    public static readonly CompressionPolicy ZstdHigh = new("zstd-high", headerId: 4, pack: static p => ZstdFrame.Pack(p.Span, level: 19, archival: true), unpack: static f => ZstdFrame.Unpack(f.Span));

    public int HeaderId { get; }
    public static bool Known(int headerId) => Items.Any(r => r.HeaderId == headerId);
    public static Option<CompressionPolicy> ByHeaderId(int headerId) => Items.Find(r => r.HeaderId == headerId).ToOption();
    [UseDelegateFromConstructor] public partial byte[] Pack(ReadOnlyMemory<byte> payload);
    [UseDelegateFromConstructor] public partial byte[] Unpack(ReadOnlyMemory<byte> framed);
}

public static class ZstdFrame {
    public static byte[] Pack(ReadOnlySpan<byte> payload, int level, bool archival) {
        using var compressor = new Compressor(level);
        compressor.SetParameter(ZSTD_cParameter.ZSTD_c_contentSizeFlag, 1);
        compressor.SetParameter(ZSTD_cParameter.ZSTD_c_checksumFlag, 1);
        if (archival) {
            compressor.SetParameter(ZSTD_cParameter.ZSTD_c_enableLongDistanceMatching, 1);
            compressor.SetParameter(ZSTD_cParameter.ZSTD_c_strategy, (int)ZSTD_strategy.ZSTD_btultra2);
        }
        return compressor.Wrap(payload).ToArray();
    }
    public static byte[] Unpack(ReadOnlySpan<byte> framed) { using var d = new Decompressor(); return d.Unwrap(framed).ToArray(); }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<CodecKeyPolicy, string>]
[KeyMemberComparer<CodecKeyPolicy, string>]
public sealed partial class HashPolicy {
    public static readonly HashPolicy Content = new("xxhash3", domainId: 1, bits: 64, hexFormat: "x16", compute: static p => XxHash3.HashToUInt64(p.Span));
    public static readonly HashPolicy Identity = new("xxhash128", domainId: 2, bits: 128, hexFormat: "x32", compute: static p => XxHash128.HashToUInt128(p.Span));
    public static readonly HashPolicy Frame = new("crc32", domainId: 3, bits: 32, hexFormat: "x8", compute: static p => Crc32.HashToUInt32(p.Span));
    public static readonly HashPolicy Wide = new("xxhash64", domainId: 4, bits: 64, hexFormat: "x16", compute: static p => XxHash64.HashToUInt64(p.Span));
    public static readonly HashPolicy FrameWide = new("crc64", domainId: 5, bits: 64, hexFormat: "x16", compute: static p => Crc64.HashToUInt64(p.Span));

    public byte DomainId { get; }
    public int Bits { get; }
    public string HexFormat { get; }
    [UseDelegateFromConstructor] public partial UInt128 Compute(ReadOnlyMemory<byte> payload);
    public static Option<HashPolicy> ByDomainId(byte domainId) => Items.Find(r => r.DomainId == domainId).ToOption();
}
```

## [05]-[SNAPSHOT_SPINE]

- Owner: `SnapshotHeader` the fixed-72-byte little-endian prologue that is the artifact's entire trust boundary; `RejectTier` the ordered rejection-ladder rank carrying its evidence shape; `SnapshotCatalogRow` the content-lineage catalog edition; `Snapshots` the static surface owning the single-pass seal, the tier ladder, and the orphan sweep.
- Entry: `public static IO<SnapshotCatalogRow> Write<T>(ReceiptSinkPort sink, CorrelationId correlation, string directory, string kind, SnapshotCodec codec, CompressionPolicy compression, ulong schemaFingerprint, ulong epoch, DataClassification classification, string retentionClass, Option<ContentAddress> lineage, T value, Func<SnapshotCatalogRow, IO<Unit>> persist)` encodes, packs, hashes, seals, stamps, and persists one artifact; `public static Fin<SnapshotHeader> Verify(ReadOnlySpan<byte> artifact, ulong schemaFingerprint, ulong epoch)` is the pure tier ladder on raw bytes with zero decoding.
- Auto: the write fold derives the codec/compression ids, the schema fingerprint, the retention epoch, both lengths, the `XxHash128` content hash over the stored bytes, the `Crc32` header checksum, the HLC stamp, the classification, and the content-lineage rank into the catalog row; the sealed `Hash` IS the `ContentAddress` every secondary surface derives from, so a snapshot is catalog-addressable on the artifact-blob index without a parallel key and `Lineage` chains the prior edition's content address so the newest-`Count`-editions retention bound (`Version/retention`) prunes by lineage depth off the catalog row.
- Receipt: `SnapshotCatalogRow` is the durable evidence; `StoredLength`/`PlainLength` are the artifact's own sealed length fields the retention sweep reads, never a later filesystem stat.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new header capability is one flag-bit row; a new rejection cause is one `RejectTier` row breaking every ladder arm; one artifact kind is one catalog row value; zero new surface.
- Boundary: the single-pass seal `Clear`s the stack-allocated prefix buffer before both writes so the placeholder header is genuinely zeroed (terminally invalid magic) and no uninitialized padding byte ever persists into the reserved-gap offsets — the CRC is then computed over the same zeroed-gap layout it verifies against, deterministic across runtimes — and the full 128-bit `XxHash128` flows un-truncated through the seal tuple to the catalog `ContentAddress` (a 64-bit truncation is the deleted form that would collide distinct contents); the seal writes the zeroed placeholder header, the stored bytes, then seeks to zero and writes the final header, `Flush(flushToDisk: true)` before `File.Move` does the atomic rename, so a crash leaves the temp swept rather than a torn final; the header is the artifact's ENTIRE trust boundary and `Verify` runs the ordered ladder on raw bytes — magic/identity, layout-version ratchet, header checksum, hash-domain capability, stored-length truncation, content hash over stored bytes through the resolved `HashPolicy.Compute`, codec/compression capability, then the epoch-then-fingerprint ratchet — each tier verifying before the next so corrupted or foreign input rejects before any decoder with attack surface binds, and layout-version/epoch/fingerprint are one-way ratchets so a future-layout artifact is deployment evidence rejected by one process and restored by a newer sibling; temp residue and catalog-orphaned payloads leave only through the age-gated `Sweep` (a final artifact lands on disk before its catalog `persist` `Bind` commits, so the sweep reaps only residue older than the grace window) and the swept count is the crash-loop signal.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<CodecKeyPolicy, string>]
public sealed partial class RejectTier {
    public static readonly RejectTier Foreign = new("foreign", rank: 1);
    public static readonly RejectTier FutureLayout = new("future-layout", rank: 2);
    public static readonly RejectTier HeaderCorrupt = new("header-corrupt", rank: 3);
    public static readonly RejectTier HashDomainGap = new("hash-domain-gap", rank: 4);
    public static readonly RejectTier Truncated = new("truncated", rank: 5);
    public static readonly RejectTier PayloadCorrupt = new("payload-corrupt", rank: 6);
    public static readonly RejectTier CapabilityGap = new("capability-gap", rank: 7);
    public static readonly RejectTier VersionAhead = new("version-ahead", rank: 8);

    public int Rank { get; }
    public Error Reject(string evidence) => Error.New(8320 + Rank, $"<snapshot-{Key}:{evidence}>");
}

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
        var draft = new SnapshotHeader(MagicValue, LayoutVersion, HashPolicy.Identity.DomainId, codec.HeaderId, compression.HeaderId,
            schemaFingerprint, epoch, plainLength, stored.Length, XxHash128.HashToUInt128(stored), Checksum: 0u);
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
    string RetentionClass, DataClassification Classification, Instant HlcPhysical, ulong HlcLogical, CorrelationId Correlation) {
    public Instant WrittenAt => HlcPhysical;
}

public static class Snapshots {
    public const string Suffix = ".rsnp";

    public static Fin<SnapshotHeader> Verify(ReadOnlySpan<byte> artifact, ulong schemaFingerprint, ulong epoch) =>
        artifact.Length < SnapshotHeader.Size || BinaryPrimitives.ReadUInt32LittleEndian(artifact) != SnapshotHeader.MagicValue
            ? RejectTier.Foreign.Reject(artifact.Length < SnapshotHeader.Size ? "headerless" : "magic")
        : artifact[4] > SnapshotHeader.LayoutVersion ? RejectTier.FutureLayout.Reject($"{artifact[4]}>{SnapshotHeader.LayoutVersion}")
        : BinaryPrimitives.ReadUInt32LittleEndian(artifact[SnapshotHeader.ChecksumOffset..]) != Crc32.HashToUInt32(artifact[..SnapshotHeader.ChecksumOffset]) ? RejectTier.HeaderCorrupt.Reject("crc")
        : artifact[5] != HashPolicy.Identity.DomainId ? RejectTier.HashDomainGap.Reject($"domain:{artifact[5]}")
        : BinaryPrimitives.ReadInt64LittleEndian(artifact[44..]) != artifact.Length - SnapshotHeader.Size ? RejectTier.Truncated.Reject($"{artifact.Length - SnapshotHeader.Size}")
        : XxHash128.HashToUInt128(artifact[SnapshotHeader.Size..]) != BinaryPrimitives.ReadUInt128LittleEndian(artifact[SnapshotHeader.HashOffset..SnapshotHeader.ChecksumOffset]) ? RejectTier.PayloadCorrupt.Reject("xxhash128")
        : !SnapshotCodec.Known(BinaryPrimitives.ReadInt32LittleEndian(artifact[8..])) || !CompressionPolicy.Known(BinaryPrimitives.ReadInt32LittleEndian(artifact[12..])) ? RejectTier.CapabilityGap.Reject("codec")
        : BinaryPrimitives.ReadUInt64LittleEndian(artifact[28..]) > epoch ? RejectTier.VersionAhead.Reject($"epoch:{BinaryPrimitives.ReadUInt64LittleEndian(artifact[28..])}")
        : BinaryPrimitives.ReadUInt64LittleEndian(artifact[20..]) != schemaFingerprint ? RejectTier.VersionAhead.Reject("fingerprint")
        : Fin.Succ(Read(artifact));

    public static IO<SnapshotCatalogRow> Write<T>(ReceiptSinkPort sink, CorrelationId correlation, string directory, string kind, SnapshotCodec codec, CompressionPolicy compression, ulong schemaFingerprint, ulong epoch, DataClassification classification, string retentionClass, Option<ContentAddress> lineage, T value, Func<SnapshotCatalogRow, IO<Unit>> persist) =>
        IO.lift(() => Seal(directory, Guid.CreateVersion7(), codec, compression, schemaFingerprint, epoch, codec.Serialize(typeof(T), value)))
            .Bind(file => sink.Send(correlation, TenantContext.Current, "Rasm.Persistence", kind, JsonSerializer.SerializeToElement(file, ElementJson.Options))
                .Map(envelope => new SnapshotCatalogRow(file.Id, kind, codec, compression, ContentAddress.Create(file.ContentHash), file.PlainLength, file.StoredLength, schemaFingerprint, epoch, lineage, retentionClass, classification, envelope.Physical, envelope.Logical, correlation)))
            .Bind(row => persist(row).Map(_ => row));

    public static IO<Seq<string>> Sweep(ClockPolicy clocks, Duration grace, string directory, Seq<SnapshotCatalogRow> catalog) =>
        IO.lift(() => (Now: clocks.Now, Files: toSeq(Directory.EnumerateFiles(directory))))
            .Map(scan => scan.Files.Filter(file => !catalog.Exists(row => string.Equals(Path.GetFileName(file), $"{row.Id}{Suffix}", StringComparison.Ordinal)) && scan.Now - Instant.FromDateTimeUtc(File.GetLastWriteTimeUtc(file)) >= grace))
            .Bind(static orphans => orphans.TraverseM(static file => IO.lift(() => { File.Delete(file); return file; })).As());

    static (Guid Id, UInt128 ContentHash, long PlainLength, long StoredLength) Seal(string directory, Guid id, SnapshotCodec codec, CompressionPolicy compression, ulong schemaFingerprint, ulong epoch, byte[] encoded) {
        var packed = compression.Pack(encoded);
        var header = SnapshotHeader.Seal(codec, compression, schemaFingerprint, epoch, encoded.LongLength, packed);
        Span<byte> prefix = stackalloc byte[SnapshotHeader.Size];
        prefix.Clear();
        var final = Path.Combine(directory, $"{id}{Suffix}");
        var temp = $"{final}.tmp";
        using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
            stream.Write(prefix);
            stream.Write(packed);
            header.Write(prefix);
            stream.Position = 0;
            stream.Write(prefix);
            stream.Flush(flushToDisk: true);
        }
        File.Move(temp, final, overwrite: false);
        return (id, header.ContentHash, encoded.LongLength, packed.LongLength);
    }

    static SnapshotHeader Read(ReadOnlySpan<byte> a) =>
        new(BinaryPrimitives.ReadUInt32LittleEndian(a), a[4], a[5], BinaryPrimitives.ReadInt32LittleEndian(a[8..]), BinaryPrimitives.ReadInt32LittleEndian(a[12..]),
            BinaryPrimitives.ReadUInt64LittleEndian(a[20..]), BinaryPrimitives.ReadUInt64LittleEndian(a[28..]), BinaryPrimitives.ReadInt64LittleEndian(a[36..]), BinaryPrimitives.ReadInt64LittleEndian(a[44..]),
            BinaryPrimitives.ReadUInt128LittleEndian(a[SnapshotHeader.HashOffset..SnapshotHeader.ChecksumOffset]), BinaryPrimitives.ReadUInt32LittleEndian(a[SnapshotHeader.ChecksumOffset..]));
}
```

## [06]-[CONTENT_CHUNKING]

- Owner: `ChunkPolicy` the FastCDC min/avg/max size axis; `ContentChunk` the content-keyed chunk record carrying its `XxHash128` address, source offset, length, and the `XxHash3` short tag; `ChunkManifest` the per-payload ordered chunk-key sequence; `ContentChunker` the static surface owning the FastCDC cut, the per-chunk content-key derivation, the manifest fold, and the cross-payload dedup projection.
- Entry: `public static ChunkManifest Chunk(ChunkPolicy policy, ReadOnlyMemory<byte> payload)` cuts the payload into content-defined chunks through `FastCdc.GetChunks`, keys each by `XxHash128`, and stamps each with the 64-bit `XxHash3` short tag; `public static Seq<ContentChunk> Novel(ChunkManifest manifest, Func<ulong, bool> mayHold, Func<UInt128, bool> holds)` projects the chunks a peer or the artifact-blob index lacks, probing the cheap 64-bit tag before the authoritative 128-bit compare; `public static Fin<ReadOnlyMemory<byte>> Reassemble(ChunkManifest manifest, Func<UInt128, ReadOnlyMemory<byte>> fetch)` rebuilds and verifies against the manifest's whole-artifact content hash.
- Auto: the content-defined boundary is the FastCDC normalized gear-hash cut so an insertion that shifts every fixed-window boundary leaves the content-defined boundaries stable past the edit and a small change to a large geometry blob re-stores only the changed chunks; each chunk's content key is `XxHash128` (the `HashPolicy.Identity` row) so an identical chunk across two snapshots or two peers dedups; the short tag is `HashPolicy.Content` (`XxHash3`) so `Novel` probes `mayHold(ShortTag)` before `holds(ContentKey)` on a hot re-store path.
- Receipt: a chunked store rides `store.chunk.split` carrying chunk and novel-chunk counts; a dedup hit rides `store.chunk.dedup` carrying reused-chunk and reused-byte counts.
- Packages: FastCDC.Net, System.IO.Hashing, LanguageExt.Core, BCL inbox.
- Growth: a new chunk-size profile is one `ChunkPolicy` row; zero new surface — a fixed-window framing, a per-edit full re-store, or a second content-defined chunker is the deleted form.
- Boundary: this owner is the opaque-byte chunker for snapshot frames and the geometry-blob multipart window (`Store/blobstore#MULTIPART_TRANSFER`); the chunk content key is `XxHash128`, never the FastCDC gear-hash cut value (a boundary marker, not an identity); the chunker is one-shot over an in-memory `byte[]` so a payload above the 4-GiB `uint`-offset window partitions upstream; the `Snapshots.Seal` fold chunks the packed payload so a sealed snapshot's chunks dedup against the artifact-blob index, and the blob multipart window consumes whole content-defined chunks rather than a fixed slice so a re-uploaded artifact skips the chunks the index already holds.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<CodecKeyPolicy, string>]
public sealed partial class ChunkPolicy {
    public static readonly ChunkPolicy Artifact = new("artifact", min: 16 * 1024u, avg: 64 * 1024u, max: 256 * 1024u);
    public static readonly ChunkPolicy Small = new("small", min: 2 * 1024u, avg: 8 * 1024u, max: 32 * 1024u);

    public uint Min { get; }
    public uint Avg { get; }
    public uint Max { get; }
    private ChunkPolicy(string key, uint min, uint avg, uint max) : this(key) => (Min, Avg, Max) = (min, avg, max);
}

public readonly record struct ContentChunk(UInt128 ContentKey, ulong ShortTag, uint Offset, int Length);

public readonly record struct ChunkManifest(ContentAddress WholeArtifact, long Length, Seq<ContentChunk> Chunks);

public static class ContentChunker {
    public static ChunkManifest Chunk(ChunkPolicy policy, ReadOnlyMemory<byte> payload) {
        var source = payload.ToArray();
        var chunks = toSeq(new FastCdc(source, policy.Min, policy.Avg, policy.Max, eof: true).GetChunks()
            .Select(cut => {
                var span = source.AsSpan(cut.Offset, cut.Length);
                return new ContentChunk(XxHash128.HashToUInt128(span), XxHash3.HashToUInt64(span), (uint)cut.Offset, cut.Length);
            }));
        return new ChunkManifest(ContentAddress.Create(XxHash128.HashToUInt128(source)), source.LongLength, chunks);
    }

    public static Seq<ContentChunk> Novel(ChunkManifest manifest, Func<ulong, bool> mayHold, Func<UInt128, bool> holds) =>
        manifest.Chunks.Filter(chunk => !mayHold(chunk.ShortTag) || !holds(chunk.ContentKey));

    public static Fin<ReadOnlyMemory<byte>> Reassemble(ChunkManifest manifest, Func<UInt128, ReadOnlyMemory<byte>> fetch) {
        var buffer = new ArrayBufferWriter<byte>((int)manifest.Length);
        foreach (var chunk in manifest.Chunks) buffer.Write(fetch(chunk.ContentKey).Span);
        return XxHash128.HashToUInt128(buffer.WrittenSpan) == manifest.WholeArtifact.Value
            ? Fin.Succ((ReadOnlyMemory<byte>)buffer.WrittenMemory)
            : Fin.Fail<ReadOnlyMemory<byte>>(Error.New(8330, "<chunk-reassembly-drift>"));
    }
}
```

| [INDEX] | [POLICY]            | [VALUE]                              | [BINDING]                                                  |
| :-----: | :------------------ | :----------------------------------- | :-------------------------------------------------------- |
|  [01]   | chunk boundary      | FastCDC normalized gear-hash cut     | insertion-stable; small change re-stores only changed chunks |
|  [02]   | chunk identity      | `XxHash128` content key              | dedup across snapshots/peers; never the gear-hash cut     |
|  [03]   | dedup pre-filter    | `XxHash3` 64-bit short tag           | `Novel` probes `mayHold` before `holds`                   |
|  [04]   | reassembly guard    | whole-artifact `XxHash128`           | torn/reordered manifest faults, never silent wrong bytes  |

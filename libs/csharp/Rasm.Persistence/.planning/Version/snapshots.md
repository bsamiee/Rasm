# [PERSISTENCE_SNAPSHOT_CODECS]

Rasm.Persistence encodes every durable payload through one three-row `SnapshotCodec` axis paired with the `CompressionPolicy` and `HashPolicy` axes, seals payloads under one fixed-offset `SnapshotHeader` that is the artifact's entire trust boundary, commits them through the single-pass atomic-write protocol, verifies every read through one ordered tier ladder that rejects before any decoder with attack surface runs, catalogs editions by content lineage under classification and retention columns, splits opaque payload bytes into content-defined chunks for cross-snapshot dedup, and projects restore and diff evidence; `PersistenceWireContext` is the package's one JsonSerializerContext partial joining the suite Strict merge. Codec, compression, and hash variance are delegate rows on string-keyed smart enums, the writer fold is the page's single entrypoint family over MessagePack, System.Text.Json source generation, LZ4 block framing, FastCDC content-defined chunking, and System.IO.Hashing, and every stamp rides the AppHost clock, correlation, retention-class, and receipt spine as settled vocabulary. The header records what WAS done — observed compression, both plain and stored length, the content hash over stored bytes, the schema fingerprint, the retention epoch, and its own checksum — never what was requested, so a torn or foreign artifact self-rejects by offset before the codec machinery binds.

## [01]-[INDEX]

- [01]-[CODEC_AXIS]: three codec rows, package wire context, and generated converter admission.
- [02]-[COMPRESSION_HASHING]: compression rows, hash rows, and framing routes and identity values.
- [03]-[SNAPSHOT_SPINE]: fixed-offset header trust boundary, tier rejection ladder, single-pass atomic write fold, content-lineage catalog row, and orphan sweep.
- [04]-[CONTENT_CHUNKING]: FastCDC content-defined chunk boundaries; per-chunk content-key dedup.
- [05]-[RESTORE_AND_DIFF]: ladder-verified read, restore receipt parity, and content-addressed catalog/edition diff.
- [06]-[FUZZ_HARNESS]: out-of-process SharpFuzz over the codec/restore untrusted boundary.
- [07]-[SCHEMA_EVOLUTION]: content-negotiated wire formats, schema-version registry, and codec-as-lineage.
- [08]-[TS_PROJECTION]: wire shapes and msgpack alignment the dashboard consumes.

## [02]-[CODEC_AXIS]

- Owner: `SnapshotCodec` `[SmartEnum<string>]` under the `SnapshotKeyPolicy` ordinal accessor; `PersistenceWireContext` as the package wire context; `InstantFormatter` as the one primitive-mapped NodaTime formatter row; `WireSurface` as the wire-surface vocabulary each codec row admits through its frozen `Membership` set, the one axis `SnapshotCodec.Negotiate`/`Offered` filter so content negotiation is the codec rows a surface admits rather than a parallel format enum; `GeoJsonProjection` as the one `GeoJsonConverterFactory` admission; `PersistenceResolver` as the AOT MessagePack resolver landmark.
- Cases: 4 codec rows — json-stj, messagepack, file-raw, cbor; 4 wire surfaces — snapshot, cache, sync, web.
- Entry: `public partial byte[] Serialize(Type shape, object? value)` — pure byte transform; shape-discriminated dispatch serves every registered wire record through source-generated metadata.
- Auto: registering `ThinktectureJsonConverterFactory` and `ThinktectureMessageFormatterResolver.Instance` once derives every value-object, smart-enum, and keyed-union converter and formatter — per-type hand-written codec classes are the deleted pattern; `GeoJsonProjection` admits one `GeoJsonConverterFactory` deriving the GeoJSON projection of every NetTopologySuite geometry, feature, and attribute table on the STJ rail.
- Packages: MessagePack, MessagePackAnalyzer, System.Formats.Cbor, Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, Thinktecture.Runtime.Extensions.MessagePack, NetTopologySuite.IO.GeoJSON4STJ, NodaTime, NodaTime.Serialization.SystemTextJson, BCL inbox.
- Growth: one codec is one row; a new wire record is one `[JsonSerializable]` row on `PersistenceWireContext` plus one MessagePack union tag row when polymorphic; the AOT resolver landmark is the `PersistenceResolver` partial carrying `[CompositeResolverAttribute]` over `[GeneratedMessagePackResolverAttribute]`, so a published-AOT build binds the generated resolver and a JIT build keeps the runtime `CompositeResolver.Create` chain — the swap is one `Foreign`/`Binary` resolver-field selection, never a second codec; a new geometry policy is one column on the `GeoJsonProjection` factory record; zero new surface.
- Boundary: artifact-kind-to-codec residence is fixed at write — a second codec on one kind is a conflict, not a fallback; `GeoJsonProjection.Factory` carries the whole geometry wire profile so geometry wire records hold no per-call geometry policy — `writeGeometryBBox` stamps the GeoJSON `bbox`, `GeoJsonConverterFactory.DefaultIdPropertyName` lifts a feature `id` out of `properties`, and `allowModifyingAttributesTables: false` keeps deserialized tables read-only `JsonElementAttributesTable` projections; the `ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true)` and `ThinktectureMessageFormatterResolver(skipObjectsWithMessagePackFormatterAttribute: true)` ctors arm only where a source-context already owns a domain type's converter, so a doubly-registered converter never shadows the source-generated one; the `PersistenceResolver` AOT landmark composes ahead of the runtime `CompositeResolver.Create` chain under a published-AOT build; `ProjectJson` and the `ContractlessStandardResolver` tail are diagnostic egress only, never a payload route or wire surface; `Foreign` gates every cross-process payload; the whole-archive codec path streams through `SerializeAsync`/`DeserializeAsync` and `MessagePackStreamReader`'s length-delimited segment sequence so a multi-segment op-log or snapshot decodes one frame at a time; an extension typecode crosses through `ExtensionHeader`/`ExtensionResult` and stays absent on the wire so the TS ext-union is `never`; NodaTime intervals bind ISO through `WithIsoIntervalConverter`/`WithIsoDateIntervalConverter` and a `[Union]`/`[SmartEnum]` key parses inbound from the UTF-8 span through `ThinktectureSpanParsableJsonConverter`; MessagePack union tag rows are append-only and a retired tag never returns; a hand-written converter or formatter beside the generated ones is the named defect; the `Cbor` row is the self-describing IETF blob codec whose `CborConformanceMode.Canonical` deterministic map-key order makes the encoded bytes content-stable for the `XxHash128` `ContentAddress` (a guarantee schemaless MessagePack cannot give across insertion order) and whose `Strict` bounded reader guards an untrusted egress-received frame against a depth/length bomb, so a structured self-describing blob routes through `CborBlob` while an evolving typed record stays MessagePack/Avro and a `Lax`-mode content key is the deleted form; MemoryPack and protobuf snapshot encodings stay rejected — proto owns RPC payloads only; the MessagePackBinary row is the value the cache serializer registration consumes.

```csharp signature
public sealed class SnapshotKeyPolicy : IEqualityComparerAccessor<string>, IComparerAccessor<string> {
    public static IEqualityComparer<string> EqualityComparer => StringComparer.Ordinal;

    public static IComparer<string> Comparer => StringComparer.Ordinal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SnapshotKeyPolicy, string>]
[KeyMemberComparer<SnapshotKeyPolicy, string>]
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
[JsonSerializable(typeof(SnapshotCatalogRow))]
[JsonSerializable(typeof(SealedSnapshot))]
[JsonSerializable(typeof(SnapshotDelta))]
[JsonSerializable(typeof(RestoreReceipt))]
[JsonSerializable(typeof(CacheIndexFact))]
public partial class PersistenceWireContext : JsonSerializerContext;

public sealed class InstantFormatter : IMessagePackFormatter<Instant> {
    public static readonly InstantFormatter Instance = new();

    public void Serialize(ref MessagePackWriter writer, Instant value, MessagePackSerializerOptions options) =>
        writer.Write(value.ToUnixTimeTicks());

    public Instant Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options) =>
        Instant.FromUnixTimeTicks(reader.ReadInt64());
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SnapshotKeyPolicy, string>]
[KeyMemberComparer<SnapshotKeyPolicy, string>]
public sealed partial class SnapshotCodec {
    public static readonly SnapshotCodec JsonStj = new(
        "json-stj", headerId: 1, version: 1, negotiationRank: 1, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot, WireSurface.Web]),
        serialize: static (shape, value) => JsonSerializer.SerializeToUtf8Bytes(value, shape, SnapshotJson),
        deserialize: static (shape, payload) => JsonSerializer.Deserialize(payload.Span, shape, SnapshotJson),
        projectJson: static payload => Encoding.UTF8.GetString(payload.Span));
    public static readonly SnapshotCodec MessagePackBinary = new(
        "messagepack", headerId: 2, version: 1, negotiationRank: 3,
        membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot, WireSurface.Cache, WireSurface.Sync, WireSurface.Web]),
        serialize: static (shape, value) => MessagePackSerializer.Serialize(shape, value, Binary),
        deserialize: static (shape, payload) => MessagePackSerializer.Deserialize(shape, payload, Binary),
        projectJson: static payload => MessagePackSerializer.ConvertToJson(payload, Binary));
    public static readonly SnapshotCodec FileRaw = new(
        "file-raw", headerId: 3, version: 1, negotiationRank: 0, membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot]),
        serialize: static (_, value) => (byte[])value!,
        deserialize: static (_, payload) => payload.ToArray(),
        projectJson: static payload => $"{{\"opaqueLength\":{payload.Length}}}");
    // The self-describing IETF blob codec — `System.Formats.Cbor` under `CborConformanceMode.Canonical`: a
    // canonical frame's deterministic map-key order and shortest-form integers make the encoded bytes
    // content-stable for the `XxHash128` `ContentAddress` regardless of insertion order, the load-bearing seam
    // a schemaless MessagePack body cannot guarantee. Distinct from `FileRaw` (raw passthrough, no structure) and
    // from `MessagePackBinary` (the typed object-graph wire) — CBOR owns the structured self-describing blob whose
    // untrusted-ingest decode is bounded against a depth/length bomb.
    public static readonly SnapshotCodec Cbor = new(
        "cbor", headerId: 4, version: 1, negotiationRank: 2,
        membership: FrozenSet.ToFrozenSet([WireSurface.Snapshot, WireSurface.Sync, WireSurface.Web]),
        serialize: static (_, value) => CborBlob.Encode((ReadOnlyMemory<byte>)value!),
        deserialize: static (_, payload) => CborBlob.Decode(payload),
        projectJson: static payload => $"{{\"cborLength\":{payload.Length}}}");

    public int HeaderId { get; }
    public int Version { get; }
    public int NegotiationRank { get; }
    public FrozenSet<WireSurface> Membership { get; }

    public static bool Known(int headerId) => Items.Any(codec => codec.HeaderId == headerId);

    public static Option<SnapshotCodec> ByHeaderId(int headerId) => Items.Find(codec => codec.HeaderId == headerId).ToOption();

    public bool Serves(WireSurface surface) => Membership.Contains(surface);

    // The codecs a wire surface admits, descending negotiation rank: file-raw never reaches web, cache binds messagepack only.
    public static Seq<SnapshotCodec> Offered(WireSurface surface) =>
        Items.Filter(codec => codec.Serves(surface)).OrderByDescending(static codec => codec.NegotiationRank).ToSeq();

    // Highest mutually-supported codec on `surface` an accepting consumer admits; file-raw on web is unrepresentable
    // because Membership withholds it, so a server-imposed single format or a hardcoded pick is the deleted form.
    public static Fin<SnapshotCodec> Negotiate(WireSurface surface, Seq<string> accepted) =>
        Offered(surface).Find(codec => accepted.Contains(codec.Key))
            .Match(Some: Fin.Succ, None: () => Fin.Fail<SnapshotCodec>(Error.New($"<wire-no-mutual-codec:{surface.Key}>")));

    [UseDelegateFromConstructor]
    public partial byte[] Serialize(Type shape, object? value);
    [UseDelegateFromConstructor]
    public partial object? Deserialize(Type shape, ReadOnlyMemory<byte> payload);
    [UseDelegateFromConstructor]
    public partial string ProjectJson(ReadOnlyMemory<byte> payload);
    public static readonly JsonSerializerOptions SnapshotJson =
        new JsonSerializerOptions(JsonSerializerOptions.Strict) {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            TypeInfoResolver = PersistenceWireContext.Default,
            Converters = { new ThinktectureJsonConverterFactory(), GeoJsonProjection.Default.Factory },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
    public static readonly MessagePackSerializerOptions Binary =
        MessagePackSerializerOptions.Standard
            .WithResolver(CompositeResolver.Create(
                [InstantFormatter.Instance],
                [ThinktectureMessageFormatterResolver.Instance, SourceGeneratedFormatterResolver.Instance, StandardResolver.Instance]))
            .WithCompression(MessagePackCompression.Lz4BlockArray);
    public static readonly MessagePackSerializerOptions Foreign =
        Binary.WithSecurity(MessagePackSecurity.UntrustedData);
}

// The self-describing canonical CBOR blob codec the `SnapshotCodec.Cbor` row composes. `Encode` wraps the
// payload in a `CborConformanceMode.Canonical` self-describing frame (the tag-55799 `SelfDescribeCbor` marker
// plus a definite-length byte string) so the writer reorders map keys and emits shortest-form integers on
// `Encode`, making the bytes content-stable for the `XxHash128` `ContentAddress`. `Decode` reads under a
// bounded `CborConformanceMode.Strict` reader whose `PeekState` loop bounds `CurrentDepth` against `MaxDepth`
// and refuses an indefinite-length frame before allocating, so a depth-bomb or unterminated egress-received
// frame faults as `CborContentException` rather than exhausting memory — the BCL exposes no decompressed-size
// cap, so the depth/length guard is this rail's responsibility, never `CborConformanceMode.Lax` on a content key.
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
        if (reader.PeekState() == CborReaderState.Tag && reader.PeekTag() == CborTag.SelfDescribeCbor)
            reader.ReadTag();
        return reader.CurrentDepth > MaxDepth
            ? throw new CborContentException($"<cbor-depth:{reader.CurrentDepth}>")
            : reader.ReadByteString();
    }
}

public sealed record GeoJsonProjection(
    GeometryFactory Geometry,
    bool WriteBoundingBox = true,
    string IdProperty = GeoJsonConverterFactory.DefaultIdPropertyName,
    RingOrientationOption RingOrientation = RingOrientationOption.EnforceRfc9746,
    bool AllowModifyingAttributes = false) {
    public static readonly GeoJsonProjection Default = new(GeometryFactory.Default);

    public GeoJsonConverterFactory Factory =>
        new(Geometry, WriteBoundingBox, IdProperty, RingOrientation, AllowModifyingAttributes);
}

[GeneratedMessagePackResolver]
public partial class GeneratedMessagePackResolver;

[CompositeResolver(typeof(ThinktectureMessageFormatterResolver), typeof(GeneratedMessagePackResolver))]
public partial class PersistenceResolver;
```

## [03]-[COMPRESSION_HASHING]

- Owner: `CompressionPolicy` and `HashPolicy` `[SmartEnum<string>]` row families under the `SnapshotKeyPolicy` ordinal accessor.
- Cases: 5 compression rows — none, lz4-fast, lz4-high, zstd, zstd-high; 5 hash rows — Content, Identity, Frame, Wide, FrameWide.
- Entry: `public partial byte[] Pack(ReadOnlyMemory<byte> payload)` — pure byte transform; the row delegate is total over any payload size.
- Packages: K4os.Compression.LZ4, ZstdSharp.Port, MessagePack, System.IO.Hashing, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: one compression level or hash algorithm is one row; the Zstandard rows each carry their own `HeaderId` (zstd=3, zstd-high=4) so the snapshot header's `CompressionId` keeps every prior LZ4 archive (lz4-fast=1, lz4-high=2) readable across the swap, and a trained-dictionary row carrying its dict blob or a multithreaded archival row is one more `CompressionPolicy` row over `ZstdFrame` rather than a per-call branch; zero new surface.
- Boundary: every hash row is non-cryptographic identity — a security or tamper claim on any row is the named defect; compression evidence never obscures redaction or retention receipts; the MessagePackBinary codec pairs with the none row at write because Lz4BlockArray owns compression in-codec — double framing is the deleted pattern, and a `SnapshotCodec.Cbor` or `JsonStj` blob whose body already rode the Arrow-IPC `Apache.Arrow.Compression` `Zstd` block compression (or a MessagePack `Lz4BlockArray` payload) likewise pairs with `None` because re-framing an already-compressed frame through the standalone `ZstdFrame`/`LZ4Pickler` is the rejected double-frame; `LZ4Pickler` owns the lowest-latency self-describing frame while `ZstdSharp.Port` owns the higher-ratio path with `contentSizeFlag`/`checksumFlag` frame self-description, long-distance matching, and the `btultra2` archival strategy, the policy row selecting one so a payload is framed exactly once and the `checksumFlag` frame checksum complements rather than replaces the snapshot rail's own `XxHash128`/`Crc32` over the framed bytes (the integrity receipt survives a codec change); the four standalone-frame rows ARE the standalone route (`LZ4Pickler` for lz4-*, `ZstdFrame` for zstd-*) and `Pack`/`Unpack` never dispatch on payload size, because the >1-MiB streaming lane (table row [03]) and the fixed-span raw-block lane (table row [04]) are the `#CONTENT_CHUNKING` chunker and rented-buffer kernels' package-depth reach (`LZ4Encoder`/`LZ4ChainDecoder`, the Zstd `WrapStream`/`FlushStream` `OperationStatus` pump), not a third `CompressionPolicy` row — a snapshot body above the chunk window partitions through FastCDC before any `Pack`, so a per-call size branch on the row delegate is the deleted form; the Frame row is `Crc32` and is the `#SNAPSHOT_SPINE` `SnapshotHeader.Checksum` algorithm (the header's own integrity guard over its `[0..68]` prefix, separating header corruption from payload corruption) and never stands in for Identity; `Identity` pins `XxHash128` as the one federation content-address algorithm every snapshot identity, sealed `Hash`, chunk key, diff, and dedup surface computes — `Seal`, `ContentAddress`, `#CONTENT_CHUNKING`, `Query/cache#ARTIFACT_BLOB_INDEX`, and `Version/diff#STRUCTURAL_DIFF` all read this one 128-bit address, so at one store generation a 64-bit hash standing in for the content address is the deleted form; the sealed header's `HashDomain` byte records `Identity.DomainId` so the content-hash algorithm is self-describing and `Verify` resolves it through `HashPolicy.ByDomainId` rather than hardcoding `XxHash128` — a future wider content-address row is one `HashPolicy` row plus the epoch-gated identity migration, never a second ladder arm or a re-parse, and an unknown domain rejects at the `HashDomainGap` tier before the content-hash tier binds it; `Content`, `Wide`, and `FrameWide` are the narrower 64-bit non-identity tags whose collision domains stay statistically independent of the content address without the 128-bit cost of `Identity` — `Content` is `XxHash3` for a fast short tag stamped on every `#CONTENT_CHUNKING` `ContentChunk` as its `ShortTag`, so the chunk-dedup lookup probes a 64-bit bloom/sketch membership pre-filter ahead of the authoritative 128-bit `XxHash128` content-key compare and a tag-miss skips the lookup entirely on a hot re-store path; `Wide` is `XxHash64` for a wide artifact-catalog index, and `FrameWide` is `Crc64` for whole-archive frame integrity where `Frame`'s 32-bit check is too narrow — none of the three is the content address, and `Content` is the pre-filter face only, never a dedup identity, so a `ShortTag` collision always falls through to the `Identity` compare; every row folds once into the `Bits`/`HexFormat` columns so a tag width is data, never a per-call format string.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<SnapshotKeyPolicy, string>]
[KeyMemberComparer<SnapshotKeyPolicy, string>]
public sealed partial class CompressionPolicy {
    public static readonly CompressionPolicy None = new(
        "none", headerId: 0,
        pack: static payload => payload.ToArray(),
        unpack: static framed => framed.ToArray());
    public static readonly CompressionPolicy Lz4Fast = new(
        "lz4-fast", headerId: 1,
        pack: static payload => LZ4Pickler.Pickle(payload.Span, LZ4Level.L00_FAST),
        unpack: static framed => LZ4Pickler.Unpickle(framed.Span));
    public static readonly CompressionPolicy Lz4High = new(
        "lz4-high", headerId: 2,
        pack: static payload => LZ4Pickler.Pickle(payload.Span, LZ4Level.L09_HC),
        unpack: static framed => LZ4Pickler.Unpickle(framed.Span));
    // The higher-ratio standalone-frame rows: `ZstdSharp.Port` is the pure-managed libzstd port whose
    // self-describing frame carries its own decoded size (`ZSTD_c_contentSizeFlag`) and a frame checksum
    // (`ZSTD_c_checksumFlag`) so the header stores only the `CompressionId`, never a sidecar length. The
    // balanced row rides level 3, the archival row level 19 with long-distance matching and the `btultra2`
    // strategy for a large redundant snapshot — distinct `HeaderId`s keep every prior LZ4 archive readable.
    public static readonly CompressionPolicy Zstd = new(
        "zstd", headerId: 3,
        pack: static payload => ZstdFrame.Pack(payload.Span, level: 3, archival: false),
        unpack: static framed => ZstdFrame.Unpack(framed.Span));
    public static readonly CompressionPolicy ZstdHigh = new(
        "zstd-high", headerId: 4,
        pack: static payload => ZstdFrame.Pack(payload.Span, level: 19, archival: true),
        unpack: static framed => ZstdFrame.Unpack(framed.Span));

    public int HeaderId { get; }

    public static bool Known(int headerId) => Items.Any(row => row.HeaderId == headerId);

    public static Option<CompressionPolicy> ByHeaderId(int headerId) => Items.Find(row => row.HeaderId == headerId).ToOption();

    [UseDelegateFromConstructor]
    public partial byte[] Pack(ReadOnlyMemory<byte> payload);
    [UseDelegateFromConstructor]
    public partial byte[] Unpack(ReadOnlyMemory<byte> framed);
}

// The Zstd frame kernel the `CompressionPolicy.Zstd*` rows compose. `Compressor`/`Decompressor` hold a reusable
// cctx/dctx and are `IDisposable`, so each Pack/Unpack owns its context disposed at scope (never a per-call leak,
// never one context shared across parallel snapshot workers). `contentSizeFlag` self-describes the decoded size so
// `Unwrap` sizes from the frame rather than a sidecar length, `checksumFlag` embeds a frame integrity tag beside
// the snapshot rail's own `XxHash128`, and the archival row arms long-distance matching plus the `btultra2`
// strategy for a large redundant snapshot. The trained-dictionary regime (`DictBuilder.TrainFromBuffer` +
// `LoadDictionary`) for the small-similar-blob corpus is a future dictionary-bearing row carrying its dict blob,
// never a per-call branch here, and a payload above the streaming threshold pumps `WrapStream`/`FlushStream`
// `OperationStatus` segments rather than materializing one contiguous compressed buffer.
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

    public static byte[] Unpack(ReadOnlySpan<byte> framed) {
        using var decompressor = new Decompressor();
        return decompressor.Unwrap(framed).ToArray();
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SnapshotKeyPolicy, string>]
[KeyMemberComparer<SnapshotKeyPolicy, string>]
public sealed partial class HashPolicy {
    public static readonly HashPolicy Content = new("xxhash3", domainId: 1, bits: 64, hexFormat: "x16", compute: static payload => XxHash3.HashToUInt64(payload.Span));
    public static readonly HashPolicy Identity = new("xxhash128", domainId: 2, bits: 128, hexFormat: "x32", compute: static payload => XxHash128.HashToUInt128(payload.Span));
    public static readonly HashPolicy Frame = new("crc32", domainId: 3, bits: 32, hexFormat: "x8", compute: static payload => Crc32.HashToUInt32(payload.Span));
    public static readonly HashPolicy Wide = new("xxhash64", domainId: 4, bits: 64, hexFormat: "x16", compute: static payload => XxHash64.HashToUInt64(payload.Span));
    public static readonly HashPolicy FrameWide = new("crc64", domainId: 5, bits: 64, hexFormat: "x16", compute: static payload => Crc64.HashToUInt64(payload.Span));

    public byte DomainId { get; }
    public int Bits { get; }
    public string HexFormat { get; }
    [UseDelegateFromConstructor]
    public partial UInt128 Compute(ReadOnlyMemory<byte> payload);

    // The header HashDomain byte carries this id so a sealed artifact self-describes its content-hash algorithm;
    // Verify ratchets the byte against the reader's Identity.DomainId, and a future epoch-gated content-address row
    // resolves here for the migration walk — never a second Verify-ladder arm.
    public static Option<HashPolicy> ByDomainId(byte domainId) => Items.Find(row => row.DomainId == domainId).ToOption();

    public string Tag(ReadOnlyMemory<byte> payload) =>
        Compute(payload).ToString(HexFormat, CultureInfo.InvariantCulture);
}
```

| [INDEX] | [ROUTE]          | [PAYLOAD_CLASS]              | [MECHANISM]                                                   | [VALUE]                                                                |
| :-----: | :--------------- | :--------------------------- | :------------------------------------------------------------ | :--------------------------------------------------------------------- |
|  [01]   | in-codec         | MessagePackBinary payloads   | `MessagePackCompression.Lz4BlockArray` blocks                 | `CompressionMinLength` provider default 64 bytes                       |
|  [02]   | standalone frame | JsonStj and FileRaw payloads | `LZ4Pickler` self-describing frame                            | level from the selected row                                            |
|  [03]   | streaming frame  | content chunks above 1 MiB before `#CONTENT_CHUNKING` partitions them | `LZ4Encoder`/`LZ4ChainDecoder` `Topup`/`Drain` chained blocks | explicit 1-MiB `blockSize` on `LZ4Encoder.Create`, each block bounded by `LZ4Codec.MaximumOutputSize`, no contiguous buffer |
|  [04]   | raw block        | fixed known-length spans into a rented buffer | `LZ4Codec.Encode`/`Decode` into a caller buffer               | `LZ4Codec.MaximumOutputSize` bounds the destination                    |

## [04]-[SNAPSHOT_SPINE]

- Owner: `SnapshotHeader` the fixed-72-byte little-endian prologue that is the artifact's entire trust boundary; `RejectTier` the `[SmartEnum<string>]` ordered rejection-ladder rank carrying its own evidence shape; `SealedSnapshot` the seal receipt; `SnapshotCatalogRow` the content-lineage catalog edition; `Snapshots` the static surface owning the single-pass seal, the tier ladder, the content-lineage edition rank, and the orphan sweep.
- Entry: `public static IO<SnapshotCatalogRow> Write<T>(ReceiptSinkPort sink, CorrelationId correlation, string directory, string kind, SnapshotCodec codec, CompressionPolicy compression, ulong schemaFingerprint, ulong epoch, DataClassification classification, string retentionClass, Option<UInt128> lineage, T value, Func<SnapshotCatalogRow, IO<Unit>> persist)` — `IO` carries the file-system and sink effects; one call encodes, packs, hashes, seals, stamps, and persists. `public static Fin<SnapshotHeader> Verify(ReadOnlySpan<byte> artifact, ulong schemaFingerprint, ulong epoch)` is the pure tier ladder run on raw bytes with zero decoding — the one read gate every consumer composes.
- Auto: the write fold derives the codec id and version, the compression id, the schema fingerprint, the retention epoch, both the plain and stored length, the `XxHash128` content hash over the stored bytes, the `Crc32` header checksum, the HLC stamp, the classification, the retention class, and the content-lineage rank into the catalog row — per-call-site assembly ceremony is the deleted pattern; the schema fingerprint arrives from the compiled-model fingerprint law, the epoch from the durability epoch fence, and the row identity from the UuidV7Key identity row; the sealed `Hash` is the content address every secondary surface derives from, so a sealed snapshot is automatically catalog-addressable on the artifact-blob index without a parallel key — `ContentAddress` selects the cache tag, the blob lookup, and the diff identity from one value, and `Lineage` chains the prior edition's content address so the newest-`Count`-editions retention bound (`Version/retention#RETENTION_SWEEPS` `VersionBound`) prunes by lineage depth off the catalog row rather than a second store.
- Receipt: `SnapshotCatalogRow` is the durable evidence; the `SealedSnapshot` payload rides the receipt envelope at the sink edge, so the catalog HLC stamp and the receipt stamp are one value; `StoredLength` and `PlainLength` are the artifact's own sealed length fields the retention sweep reads, never a later filesystem stat.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new header capability is one flag bit row on `Flags`; a new rejection cause is one `RejectTier` row breaking every ladder arm; one artifact kind is one catalog row value; the chunk-dedup hand-off is one `ContentChunker.Chunk` fold over the packed payload before catalog insertion (`#CONTENT_CHUNKING`), never a second framing; zero new surface.
- Boundary: `Seal` and the sweep deletion kernel are this fence's boundary capsules — language-owned statement forms stay inside those two bodies; `directory` arrives from the placement law and is never derived here; the durability order is settled — the single-pass seal writes a placeholder header, the stored bytes, then seeks to zero and writes the final header so the artifact carries a zeroed magic (terminally invalid) at every instant before the final header lands, `Flush(flushToDisk: true)` forces the data blocks before `File.Move` performs the atomic rename, and a crash between the two leaves the temp file swept rather than a torn final or a complete-but-unclaimed orphan the sealed header still proves salvageable; the header is the artifact's ENTIRE trust boundary — magic + identity, header version, codec row + version, observed compression, hash domain, schema fingerprint, retention epoch, plain length, stored length, `XxHash128` content hash over the stored bytes, and the `Crc32` header checksum that separates header corruption (terminal for the copy) from payload corruption (replica and salvage routes) — so the header records what WAS done, never what was requested; `Verify` runs the ordered `RejectTier` ladder on raw bytes — magic/identity, header layout-version ratchet, header checksum, hash-domain capability (the content-hash algorithm the `HashDomain` byte names must resolve to a `HashPolicy` row before the content-hash tier binds it), stored-length truncation with byte counts, content-hash over stored bytes through the resolved `HashPolicy.Compute`, codec/compression capability, and the epoch-then-schema-fingerprint ratchet — each tier verifying before the next runs so corrupted or foreign input rejects before any decoder with attack surface binds, and layout-version, epoch, and fingerprint are one-way ratchets (a reader admits at or below its compiled/store-generation ceiling) so a future-layout or future-epoch artifact is deployment evidence rejected by one process and restored by a newer sibling, not a data error; temp residue and catalog-orphaned payloads leave only through the age-gated `Sweep` — a final `.rsnp` lands on disk before its catalog `persist` `Bind` commits, so the sweep reaps only residue older than the `grace` window measured off `ClockPolicy.Now` and never a concurrent in-flight write, with the swept count as the crash-loop signal the `store.snapshot.sweep` receipt carries; catalog insertion enters through the `persist` delegate so the store rail stays the single write path; `ContentAddress` parses the sealed `Hash` so the cache, blob, and diff surfaces share one content key and never mint parallel identities, and `Lineage` is the prior-edition back-link so a content-version chain prunes by edition depth; the magic constant spells RSNP in little-endian byte order; `AsOfKey` is the icechunk as-of content-key seam — the `python:data/gridded/virtual` lane stamps an icechunk as-of snapshot identity over the one C#-owned XxHash128 content-key seed (the same seed `ContentAddress` reads), so a data virtual-cube as-of read reproduces its snapshot content-key from the shared seed and resolves against this durable content-addressed snapshot spine rather than a second snapshot identity — the version-control surface is the Persistence Version concern read at the wire, and a second as-of snapshot identity per runtime is the deleted form.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<SnapshotKeyPolicy, string>]
[KeyMemberComparer<SnapshotKeyPolicy, string>]
public sealed partial class RejectTier {
    public static readonly RejectTier Foreign       = new("foreign", rank: 1);        // magic + identity mismatch
    public static readonly RejectTier FutureLayout  = new("future-layout", rank: 2);  // header version above ceiling
    public static readonly RejectTier HeaderCorrupt = new("header-corrupt", rank: 3); // header checksum mismatch
    public static readonly RejectTier HashDomainGap = new("hash-domain-gap", rank: 4);// content-hash algorithm not understood
    public static readonly RejectTier Truncated     = new("truncated", rank: 5);      // stored-length byte-count drift
    public static readonly RejectTier PayloadCorrupt = new("payload-corrupt", rank: 6); // content-hash mismatch over stored bytes
    public static readonly RejectTier CapabilityGap = new("capability-gap", rank: 7); // codec/compression not understood
    public static readonly RejectTier VersionAhead  = new("version-ahead", rank: 8);  // codec/schema fingerprint above ceiling

    public int Rank { get; }

    public Error Reject(string evidence) =>
        Error.New(8240 + Rank, $"<snapshot-{Key}:{evidence}>");
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct SnapshotHeader(
    uint Magic, byte Version, byte HashDomain, int CodecId, int CodecVersion, int CompressionId,
    ulong SchemaFingerprint, ulong Epoch, long PlainLength, long StoredLength, UInt128 ContentHash, uint Checksum) {
    public const int Size = 72;          // 4+1+1+2pad+4+4+4 = 20 | +8+8+8+8 = 52 | +16 hash = 68 | +4 crc = 72
    public const int HashOffset = Size - 20;   // bytes 52..68, the UInt128 ContentHash window
    public const int ChecksumOffset = Size - 4; // bytes 68..72, the Crc32 over bytes 0..ChecksumOffset

    public const uint MagicValue = 0x504E5352;

    public const byte LayoutVersion = 1;

    public static SnapshotHeader Seal(
        SnapshotCodec codec, CompressionPolicy compression, ulong schemaFingerprint, ulong epoch, long plainLength, ReadOnlySpan<byte> stored) {
        Span<byte> prefix = stackalloc byte[Size];
        var draft = new SnapshotHeader(
            MagicValue, LayoutVersion, HashPolicy.Identity.DomainId, codec.HeaderId, codec.Version, compression.HeaderId,
            schemaFingerprint, epoch, plainLength, stored.Length, XxHash128.HashToUInt128(stored), Checksum: 0u);
        draft.WriteFields(prefix);
        return draft with { Checksum = Crc32.HashToUInt32(prefix[..ChecksumOffset]) };
    }

    public void Write(Span<byte> destination) {
        WriteFields(destination);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[ChecksumOffset..], Checksum);
    }

    private void WriteFields(Span<byte> destination) {
        BinaryPrimitives.WriteUInt32LittleEndian(destination, Magic);
        (destination[4], destination[5]) = (Version, HashDomain);
        BinaryPrimitives.WriteInt32LittleEndian(destination[8..], CodecId);
        BinaryPrimitives.WriteInt32LittleEndian(destination[12..], CodecVersion);
        BinaryPrimitives.WriteInt32LittleEndian(destination[16..], CompressionId);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[20..], SchemaFingerprint);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[28..], Epoch);
        BinaryPrimitives.WriteInt64LittleEndian(destination[36..], PlainLength);
        BinaryPrimitives.WriteInt64LittleEndian(destination[44..], StoredLength);
        BinaryPrimitives.WriteUInt128LittleEndian(destination[HashOffset..ChecksumOffset], ContentHash);
    }
}

public readonly record struct SealedSnapshot(Guid Id, string Path, string Hash, long PlainLength, long StoredLength);

public sealed record SnapshotCatalogRow(
    Guid Id,
    string Kind,
    SnapshotCodec Codec,
    CompressionPolicy Compression,
    string Hash,
    long PlainLength,
    long StoredLength,
    ulong SchemaFingerprint,
    ulong Epoch,
    Option<UInt128> Lineage,
    string RetentionClass,
    DataClassification Classification,
    Instant HlcPhysical,
    ulong HlcLogical,
    CorrelationId Correlation) {
    // The HLC physical component IS the write instant — no separate wall-clock `WrittenAt`, which would carry the
    // identical `envelope.Physical` value the causal stamp already holds.
    public Instant WrittenAt => HlcPhysical;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Snapshots {
    public const string Suffix = ".rsnp";

    public static UInt128 ContentAddress(SnapshotCatalogRow row) =>
        UInt128.Parse(row.Hash, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    // The as-of content-key folds the sorted manifest through the incremental XxHash128 accumulator — each 16-byte key
    // and the as-of tick Append into the digest, so a wide manifest never materializes one contiguous buffer.
    public static UInt128 AsOfKey(Seq<UInt128> sortedManifest, Instant asOf) {
        var digest = new XxHash128();
        Span<byte> cell = stackalloc byte[16];
        foreach (var key in sortedManifest) {
            BinaryPrimitives.WriteUInt128LittleEndian(cell, key);
            digest.Append(cell);
        }
        BinaryPrimitives.WriteInt64LittleEndian(cell[..8], asOf.ToUnixTimeTicks());
        digest.Append(cell[..8]);
        return digest.GetCurrentHashAsUInt128();
    }

    public static Fin<SnapshotHeader> Verify(ReadOnlySpan<byte> artifact, ulong schemaFingerprint, ulong epoch) =>
        artifact.Length < SnapshotHeader.Size || BinaryPrimitives.ReadUInt32LittleEndian(artifact) != SnapshotHeader.MagicValue
            ? RejectTier.Foreign.Reject(artifact.Length < SnapshotHeader.Size ? "headerless" : "magic")
        : artifact[4] > SnapshotHeader.LayoutVersion ? RejectTier.FutureLayout.Reject($"{artifact[4]}>{SnapshotHeader.LayoutVersion}")
        : BinaryPrimitives.ReadUInt32LittleEndian(artifact[SnapshotHeader.ChecksumOffset..]) != Crc32.HashToUInt32(artifact[..SnapshotHeader.ChecksumOffset])
            ? RejectTier.HeaderCorrupt.Reject("crc")
        : artifact[5] != HashPolicy.Identity.DomainId ? RejectTier.HashDomainGap.Reject($"domain:{artifact[5]}!={HashPolicy.Identity.DomainId}")
        : BinaryPrimitives.ReadInt64LittleEndian(artifact[44..]) != artifact.Length - SnapshotHeader.Size
            ? RejectTier.Truncated.Reject($"{artifact.Length - SnapshotHeader.Size}!={BinaryPrimitives.ReadInt64LittleEndian(artifact[44..])}")
        : XxHash128.HashToUInt128(artifact[SnapshotHeader.Size..]) != ReadContentHash(artifact) ? RejectTier.PayloadCorrupt.Reject("xxhash128")
        : !SnapshotCodec.Known(BinaryPrimitives.ReadInt32LittleEndian(artifact[8..])) || !CompressionPolicy.Known(BinaryPrimitives.ReadInt32LittleEndian(artifact[16..]))
            ? RejectTier.CapabilityGap.Reject($"{BinaryPrimitives.ReadInt32LittleEndian(artifact[8..])}:{BinaryPrimitives.ReadInt32LittleEndian(artifact[16..])}")
        : BinaryPrimitives.ReadUInt64LittleEndian(artifact[28..]) > epoch
            ? RejectTier.VersionAhead.Reject($"epoch:{BinaryPrimitives.ReadUInt64LittleEndian(artifact[28..])}>{epoch}")
        : BinaryPrimitives.ReadUInt64LittleEndian(artifact[20..]) != schemaFingerprint
            ? RejectTier.VersionAhead.Reject($"fingerprint:{BinaryPrimitives.ReadUInt64LittleEndian(artifact[20..]):x16}!={schemaFingerprint:x16}")
        : Fin.Succ(Read(artifact));

    public static IO<SnapshotCatalogRow> Write<T>(
        ReceiptSinkPort sink,
        CorrelationId correlation,
        string directory,
        string kind,
        SnapshotCodec codec,
        CompressionPolicy compression,
        ulong schemaFingerprint,
        ulong epoch,
        DataClassification classification,
        string retentionClass,
        Option<UInt128> lineage,
        T value,
        Func<SnapshotCatalogRow, IO<Unit>> persist) =>
        IO.lift(() => Seal(directory, Guid.CreateVersion7(), codec, compression, schemaFingerprint, epoch, codec.Serialize(typeof(T), value)))
            .Bind(file => sink
                .Send(correlation, TenantContext.Current, "Rasm.Persistence", kind, JsonSerializer.SerializeToElement(file, SnapshotCodec.SnapshotJson))
                .Map(envelope => new SnapshotCatalogRow(
                    file.Id, kind, codec, compression, file.Hash, file.PlainLength, file.StoredLength,
                    schemaFingerprint, epoch, lineage, retentionClass, classification,
                    envelope.Physical, envelope.Logical, correlation)))
            .Bind(row => persist(row).Map(_ => row));

    // The orphan sweep is age-gated: a final `.rsnp` exists on disk before its catalog `persist` Bind commits, so an
    // orphan younger than `grace` is an in-flight write a concurrent sweep must NOT reap. Only residue older than the
    // grace window — a crash-abandoned `.tmp` or a catalog-orphaned payload — deletes, and the swept count is the
    // crash-loop signal the receipt carries (`store.snapshot.sweep`).
    public static IO<Seq<string>> Sweep(ClockPolicy clocks, Duration grace, string directory, Seq<SnapshotCatalogRow> catalog) =>
        IO.lift(() => (Now: clocks.Now, Files: toSeq(Directory.EnumerateFiles(directory))))
            .Map(scan => scan.Files.Filter(file =>
                !catalog.Exists(row => string.Equals(Path.GetFileName(file), $"{row.Id}{Suffix}", StringComparison.Ordinal))
                && scan.Now - Instant.FromDateTimeUtc(File.GetLastWriteTimeUtc(file)) >= grace))
            .Bind(static orphans => orphans
                .TraverseM(static file => IO.lift(() => { File.Delete(file); return file; }))
                .As());

    private static SnapshotHeader Read(ReadOnlySpan<byte> artifact) =>
        new SnapshotHeader(
            BinaryPrimitives.ReadUInt32LittleEndian(artifact), artifact[4], artifact[5],
            BinaryPrimitives.ReadInt32LittleEndian(artifact[8..]), BinaryPrimitives.ReadInt32LittleEndian(artifact[12..]),
            BinaryPrimitives.ReadInt32LittleEndian(artifact[16..]),
            BinaryPrimitives.ReadUInt64LittleEndian(artifact[20..]), BinaryPrimitives.ReadUInt64LittleEndian(artifact[28..]),
            BinaryPrimitives.ReadInt64LittleEndian(artifact[36..]), BinaryPrimitives.ReadInt64LittleEndian(artifact[44..]),
            ReadContentHash(artifact), BinaryPrimitives.ReadUInt32LittleEndian(artifact[SnapshotHeader.ChecksumOffset..]));

    private static UInt128 ReadContentHash(ReadOnlySpan<byte> artifact) =>
        BinaryPrimitives.ReadUInt128LittleEndian(artifact[SnapshotHeader.HashOffset..SnapshotHeader.ChecksumOffset]);

    private static SealedSnapshot Seal(
        string directory, Guid id, SnapshotCodec codec, CompressionPolicy compression, ulong schemaFingerprint, ulong epoch, byte[] encoded) {
        var packed = compression.Pack(encoded);
        var header = SnapshotHeader.Seal(codec, compression, schemaFingerprint, epoch, encoded.LongLength, packed);
        Span<byte> prefix = stackalloc byte[SnapshotHeader.Size];
        var final = Path.Combine(directory, $"{id}{Suffix}");
        var temp = $"{final}.tmp";
        using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
            stream.Write(prefix);                 // placeholder: zeroed magic is terminally invalid until the final header lands
            stream.Write(packed);
            header.Write(prefix);
            stream.Position = 0;
            stream.Write(prefix);
            stream.Flush(flushToDisk: true);
        }
        File.Move(temp, final, overwrite: false);
        return new SealedSnapshot(id, final, header.ContentHash.ToString("x32", CultureInfo.InvariantCulture), encoded.LongLength, packed.LongLength);
    }
}
```

The header is one fixed 72-byte little-endian record: magic at `[0..4]`, layout `Version` and `HashDomain` at `[4]`/`[5]`, the codec/codec-version/compression ids through `[8..20]`, `SchemaFingerprint` and `Epoch` at `[20..36]`, `PlainLength` and `StoredLength` at `[36..52]`, the `XxHash128` `ContentHash` over the STORED bytes at `[52..68]`, and the `Crc32` `Checksum` last at `[68..72]` covering every byte ahead of it. The load-bearing contracts: the checksum covers `[0..68]` so header corruption is terminal for the copy while payload corruption routes to salvage, the content hash is over the post-compression stored representation so verification strictly precedes any decoder, and `StoredLength` is the on-disk framed payload length the truncation tier checks before the content hash runs.

## [05]-[CONTENT_CHUNKING]

- Owner: `ChunkPolicy` the FastCDC min/avg/max size and eof axis with one named policy row; `ContentChunk` the content-keyed chunk record carrying its `XxHash128` address, source offset, and length; `ChunkManifest` the per-payload ordered chunk-key sequence; `ContentChunker` the static surface owning the FastCDC cut, the per-chunk content-key derivation, the manifest fold, and the cross-payload dedup projection.
- Cases: a `ChunkPolicy` row sizes the rolling-hash window — the snapshot row at the 64-KiB-class window, a small-artifact row at a tighter window; a `ContentChunk` carries its content-address key plus its `(Offset, Length)` window; a `ChunkManifest` is the ordered chunk-key sequence reconstructing the payload, and a re-store keys each chunk so only changed chunks transfer.
- Entry: `public static ChunkManifest Chunk(ChunkPolicy policy, ReadOnlyMemory<byte> payload)` — cuts the payload into content-defined chunks through `FastCdc.GetChunks`, keys each by its `XxHash128` content address, and stamps each with the `HashPolicy.Content` 64-bit `XxHash3` short tag; `public static Seq<ContentChunk> Novel(ChunkManifest manifest, Func<ulong, bool> mayHold, Func<UInt128, bool> holds)` projects the chunks a peer or the artifact-blob index lacks, probing the cheap 64-bit `ShortTag` bloom/sketch pre-filter (`mayHold`) before the full 128-bit `holds` content-key compare so a tag-miss skips the lookup; `public static Fin<ReadOnlyMemory<byte>> Reassemble(ChunkManifest manifest, Func<UInt128, ReadOnlyMemory<byte>> fetch)` rebuilds the payload from its chunk keys and verifies the concatenation against the manifest's `WholeArtifact` content hash, so a torn or reordered manifest faults rather than returning silently wrong bytes.
- Auto: the content-defined boundary is the FastCDC normalized gear-hash cut over `FastCdc(byte[] source, uint minSize, uint avgSize, uint maxSize, bool eof)` so an insertion that shifts every fixed-window boundary leaves the content-defined boundaries stable past the edit, and a small change to a large artifact re-stores only the changed chunks; each chunk's content key is `XxHash128` over the chunk bytes (the `HashPolicy.Identity` row) so an identical chunk across two snapshots or two peers dedups on its key through `Query/cache#ARTIFACT_BLOB_INDEX`; each chunk also carries the `HashPolicy.Content` 64-bit `XxHash3` `ShortTag` so the dedup lookup probes the cheap short tag through a bloom/sketch membership pre-filter ahead of the 128-bit content-key compare — a tag-miss proves the chunk novel without the full lookup, cutting the content-key compares on a hot re-store path, and the short tag is a non-identity pre-filter so a tag-hit always falls through to the authoritative `XxHash128 holds` compare and a false-positive tag never dedups a distinct chunk; the min/avg/max size bounds trace to the `ChunkPolicy` row so the gear-hash mask is data, never a free literal, and clear the `FastCdc.MinimumMin`/`AverageMin`/`MaximumMin` package floors; the per-frame `Crc32` and whole-artifact `XxHash128` integrity (`#SNAPSHOT_SPINE`, `Sync/collaboration#PRESENCE_AND_BLOB`) stay the in-transfer check beside the content-defined dedup boundary.
- Receipt: a chunked store rides `store.chunk.split` carrying the chunk count and the novel-chunk count; a dedup hit rides `store.chunk.dedup` carrying the reused-chunk count and the reused bytes.
- Packages: FastCDC.Net, System.IO.Hashing, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new chunk-size profile is one `ChunkPolicy` row; a new chunk-keyed surface is one consumer of the manifest; zero new surface — a fixed-window framing, a per-edit full re-store, or a second content-defined chunker beside this one is the deleted form because the FastCDC cut is the one opaque-byte boundary and the chunk content key is the one identity the artifact-blob index already dedups on.
- Boundary: this owner is the opaque-byte chunker for snapshot frames and the blob multipart window — it aligns with `Compute/interchange#GEOMETRY_DELTA` `DeltaCodec` at the technique level only, never importing it as a byte chunker and never re-deriving the geometry-structural delta: Compute owns the geometry-aware structural delta over geometry columns, this owns the opaque-byte content-defined chunk over snapshot and blob bytes, and the two meet at the rolling-hash technique, not the code; the `#SNAPSHOT_SPINE` `Seal` fold chunks the packed payload through `ContentChunker.Chunk` so a sealed snapshot's chunks dedup against the artifact-blob index, and the `Store/remote#MULTIPART_TRANSFER` window consumes a whole number of content-defined chunks rather than a fixed `PartSize` slice — the chunk-size column on the `ObjectStore` row clears each provider's part floor while spanning whole chunks, so a re-uploaded artifact skips the chunks the index already holds; the `FastCdc` chunker is stateful and one-shot over an in-memory `byte[]` so a re-chunk requires a fresh instance, and a payload above the 4-GiB `uint`-offset window partitions upstream before chunking — the package owns no stream or file IO; the chunk content key is `XxHash128` (`HashPolicy.Identity`), never the `Chunk.Hash` 32-bit gear-hash cut value, because the gear-hash is a boundary marker and the content address is the dedup identity; the chunk `ShortTag` is `HashPolicy.Content` (`XxHash3`, 64-bit) and is the dedup-lookup pre-filter face only — `Novel` probes `mayHold(ShortTag)` before `holds(ContentKey)` so the cheap tag culls a content-key compare on a hot re-store path while the authoritative dedup identity stays the 128-bit content key, and a `ShortTag` false positive only costs one fall-through `holds` compare, never a wrong dedup; a chunk's `(Offset, Length)` is a `uint` window the `Chunk` record carries, and the manifest reconstructs the payload in chunk order so a torn or reordered manifest is a typed reassembly rejection.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<SnapshotKeyPolicy, string>]
[KeyMemberComparer<SnapshotKeyPolicy, string>]
public sealed partial class ChunkPolicy {
    public static readonly ChunkPolicy Snapshot = new("snapshot", minSize: 16u * 1024, avgSize: 64u * 1024, maxSize: 256u * 1024);
    public static readonly ChunkPolicy Artifact = new("artifact", minSize: 4u * 1024, avgSize: 16u * 1024, maxSize: 64u * 1024);

    public uint MinSize { get; }
    public uint AvgSize { get; }
    public uint MaxSize { get; }

    public FastCdc Over(byte[] source) => new(source, MinSize, AvgSize, MaxSize, eof: true);
}

public readonly record struct ContentChunk(UInt128 ContentKey, ulong ShortTag, uint Offset, uint Length);

public sealed record ChunkManifest(Seq<ContentChunk> Chunks, UInt128 WholeArtifact, long Length) {
    public Seq<UInt128> Keys => Chunks.Map(static chunk => chunk.ContentKey);
}

public static class ContentChunker {
    public static ChunkManifest Chunk(ChunkPolicy policy, ReadOnlyMemory<byte> payload) {
        var source = payload.ToArray();          // FastCdc holds a byte[]; the cut spans then key off this one buffer
        var chunks = toSeq(policy.Over(source).GetChunks())
            .Map(cut => {
                var span = source.AsSpan((int)cut.Offset, (int)cut.Length);
                return new ContentChunk(
                    XxHash128.HashToUInt128(span),                    // HashPolicy.Identity content address
                    XxHash3.HashToUInt64(span),                      // HashPolicy.Content 64-bit pre-filter tag
                    cut.Offset,
                    cut.Length);
            });
        return new ChunkManifest(chunks, XxHash128.HashToUInt128(source), payload.Length);
    }

    public static Seq<ContentChunk> Novel(ChunkManifest manifest, Func<ulong, bool> mayHold, Func<UInt128, bool> holds) =>
        manifest.Chunks
            .Filter(chunk => !mayHold(chunk.ShortTag) || !holds(chunk.ContentKey))
            .Distinct();

    // Reassembly is content-verified: the concatenated chunks must re-hash to the manifest's WholeArtifact, so a torn,
    // reordered, or corrupt-fetch manifest is a typed reassembly rejection rather than a silently wrong payload.
    public static Fin<ReadOnlyMemory<byte>> Reassemble(ChunkManifest manifest, Func<UInt128, ReadOnlyMemory<byte>> fetch) {
        var buffer = new ArrayBufferWriter<byte>((int)manifest.Length);
        manifest.Chunks.Iter(chunk => buffer.Write(fetch(chunk.ContentKey).Span));
        return XxHash128.HashToUInt128(buffer.WrittenSpan) == manifest.WholeArtifact
            ? Fin.Succ(buffer.WrittenMemory)
            : Fin.Fail<ReadOnlyMemory<byte>>(Error.New(8261, $"<chunk-reassembly:{manifest.Chunks.Count}-chunks>"));
    }
}
```

| [INDEX] | [POLICY]     | [WINDOW]                                     | [BINDING]                                                                                          |
| :-----: | :----------- | :------------------------------------------- | :------------------------------------------------------------------------------------------------- |
|  [01]   | snapshot cut | min 16 KiB, avg 64 KiB, max 256 KiB          | `ChunkPolicy.Snapshot`; snapshot-frame dedup                                                       |
|  [02]   | artifact cut | min 4 KiB, avg 16 KiB, max 64 KiB            | `ChunkPolicy.Artifact`; small-artifact blob dedup                                                  |
|  [03]   | chunk key    | `XxHash128` over chunk bytes                 | `HashPolicy.Identity`; the one dedup identity                                                      |
|  [04]   | dedup index  | `Query/cache#ARTIFACT_BLOB_INDEX`            | identical chunk across snapshots/peers dedups                                                      |
|  [05]   | short tag    | `XxHash3` 64-bit `ShortTag` over chunk bytes | `HashPolicy.Content`; bloom/sketch pre-filter ahead of the 128-bit compare; never a dedup identity |

## [06]-[RESTORE_AND_DIFF]

- Owner: `RestoreReceipt`, `SnapshotDelta`, `SnapshotRestoreOps` — ladder-verified read, restore hand-off parity, and content-addressed catalog/edition diff projection.
- Entry: `public IO<Fin<T>> Read<T>(string path, ulong schemaFingerprint, ulong epoch)` decodes through the one `Snapshots.Verify` tier ladder, then binds the `Unpack`/`Deserialize` through `Try` so a hash-valid-but-structurally-foreign payload faults as a typed `RejectTier.PayloadCorrupt` rather than an exception escaping the rail; `public IO<Fin<RestoreReceipt>> Restore(ClockPolicy clocks, CorrelationId correlation, string path, StoreProfile target, ulong epoch, Func<string, IO<Fin<Unit>>> repair)` — `Fin` aborts on integrity rejection; `IO` carries the read and hand-off effects.
- Receipt: `RestoreReceipt` carries source id, verified hash, target profile, elapsed `Duration`, and `Instant` stamp — the typed SUCCESS evidence; a rejected restore is `Fin.Fail<RestoreReceipt>` carrying the `RejectTier`-coded `Error` the one `Snapshots.Verify` ladder mints, so the failing tier rides the fault rail rather than a half-populated receipt with a null hash, and the receipt is never a generic shape.
- Packages: System.IO.Hashing, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new delta classification is one field row on `SnapshotDelta`; a new edition relation is one column on the diff projection; zero new surface.
- Boundary: BOTH `Read` and `Restore` gate through the ONE `Snapshots.Verify` tier ladder — neither re-implements a partial check, so a foreign magic, a future layout, a corrupt header `Crc32`, a truncated payload, a content-hash mismatch over stored bytes, a capability gap, or a schema/codec version ahead of the reader's ceiling is the SAME ordered typed rejection naming the `RejectTier`, never a best-effort decode and never a hash-only check that lets a codec/fingerprint mismatch slip into restore; repair mechanics live with the store lifecycle owner and enter through the `repair` delegate so this cluster owns receipt parity only; the catalog diff keys on the content hash and the edition diff keys on the `Lineage` chain — object equality never enters either fold, and a moved-edition lineage advance reads as a `Changed` kind whose `From`/`To` are the prior and current content addresses rather than a re-import.

```csharp signature
public sealed record RestoreReceipt(
    Guid Source, string VerifiedHash, StoreProfile Target, Duration Elapsed, Instant At, CorrelationId Correlation);

// ChangeKind arrives settled from Version/timetravel#TIME_TRAVEL (Added | Removed | Replaced | Converged).
public readonly record struct KeyEdit(string Kind, ChangeKind Change, Option<UInt128> From, Option<UInt128> To);

public sealed record SnapshotDelta(Seq<KeyEdit> Edits) {
    public Seq<string> Added => Edits.Filter(static e => e.Change == ChangeKind.Added).Map(static e => e.Kind);
    public Seq<string> Removed => Edits.Filter(static e => e.Change == ChangeKind.Removed).Map(static e => e.Kind);
    public Seq<string> Changed => Edits.Filter(static e => e.Change == ChangeKind.Replaced || e.Change == ChangeKind.Converged).Map(static e => e.Kind);
}

public static class SnapshotRestoreOps {
    // One entry per kind keyed to its CURRENT edition: the HLC-newest row, so a catalog diff compares present state and a
    // multi-edition kind never resolves to an arbitrary historical address under last-write-wins map insertion.
    public static HashMap<string, UInt128> Index(Seq<SnapshotCatalogRow> rows) =>
        toHashMap(rows.GroupBy(static row => row.Kind)
            .Map(group => group.MaxBy(static row => (row.HlcPhysical, row.HlcLogical)))
            .Map(static row => (row.Kind, Snapshots.ContentAddress(row))));

    public static SnapshotDelta Diff(HashMap<string, UInt128> source, HashMap<string, UInt128> target) =>
        new(toSeq(target.Keys.Where(k => !source.ContainsKey(k)).Map(k => new KeyEdit(k, ChangeKind.Added, None, Some(target[k])))
            .Append(source.Keys.Where(k => !target.ContainsKey(k)).Map(k => new KeyEdit(k, ChangeKind.Removed, Some(source[k]), None)))
            .Append(target.Keys.Where(source.ContainsKey).Where(k => source[k] != target[k])
                .Map(k => new KeyEdit(k, ChangeKind.Replaced, Some(source[k]), Some(target[k]))))));

    // VersionBound prunes per content lineage, NOT by flat kind recency: LineageRank is a row's 0-based depth down its
    // own Lineage back-link chain from the chain head (the edition no successor cites as its prior), so two interleaved
    // lineages of one kind each keep their own newest Count and EvictLineage never collapses onto CountBound's flat tally.
    public static Seq<(SnapshotCatalogRow Row, int LineageRank)> Editions(Seq<SnapshotCatalogRow> catalog, string kind) {
        var members = catalog.Filter(r => r.Kind == kind);
        var byAddress = members.ToHashMap(Snapshots.ContentAddress);
        var cited = members.Choose(static r => r.Lineage).ToHashSet();
        return members
            .Filter(r => !cited.Contains(Snapshots.ContentAddress(r)))                         // chain heads: the newest editions
            .Bind(head => Chain(head, byAddress).Map((rank, row) => (Row: row, LineageRank: rank)));
    }

    // Unfolds one lineage newest-first from its head, following each row's Lineage back-link to its predecessor; the
    // visited set bounds a cyclic Lineage (a forged or torn catalog) to a finite walk rather than nontermination.
    private static Seq<SnapshotCatalogRow> Chain(SnapshotCatalogRow head, HashMap<UInt128, SnapshotCatalogRow> byAddress) {
        Seq<SnapshotCatalogRow> Walk(SnapshotCatalogRow node, LanguageExt.HashSet<UInt128> seen) =>
            node.Lineage
                .Filter(prior => !seen.Contains(prior))
                .Bind(byAddress.Find)
                .Match(prior => node.Cons(Walk(prior, seen.Add(Snapshots.ContentAddress(node)))),
                       () => node.Cons(Seq<SnapshotCatalogRow>()));
        return Walk(head, LanguageExt.HashSet<UInt128>.Empty);
    }

    extension(SnapshotCatalogRow row) {
        // Verify gates the raw bytes; the decode then binds through Try so an Unpack or Deserialize fault on a
        // hash-valid-but-structurally-foreign payload converts to a typed Fin.Fail, never an exception escaping the rail.
        public IO<Fin<T>> Read<T>(string path, ulong schemaFingerprint, ulong epoch) =>
            IO.lift(() => File.ReadAllBytes(path)).Map(bytes =>
                Snapshots.Verify(bytes, schemaFingerprint, epoch).Bind(header =>
                    Try.lift(() => (T)SnapshotCodec.ByHeaderId(header.CodecId).IfNone(row.Codec)
                        .Deserialize(typeof(T),
                            CompressionPolicy.ByHeaderId(header.CompressionId).IfNone(row.Compression).Unpack(bytes.AsMemory(SnapshotHeader.Size)))!)
                        .Run().MapFail(err => RejectTier.PayloadCorrupt.Reject($"decode:{err.Message}"))));

        public IO<Fin<RestoreReceipt>> Restore(
            ClockPolicy clocks, CorrelationId correlation, string path, StoreProfile target, ulong epoch, Func<string, IO<Fin<Unit>>> repair) =>
            IO.lift(clocks.Mark).Bind(mark =>
                IO.lift(() => File.ReadAllBytes(path)).Bind(bytes =>
                    Snapshots.Verify(bytes, row.SchemaFingerprint, epoch).Match(
                        Succ: header => repair(path).Map(outcome => outcome.Map(_ =>
                            new RestoreReceipt(row.Id, header.ContentHash.ToString("x32", CultureInfo.InvariantCulture), target, clocks.Elapsed(mark), clocks.Now, correlation))),
                        Fail: error => IO<Fin<RestoreReceipt>>.Pure(Fin.Fail<RestoreReceipt>(error)))));
    }
}
```

## [07]-[FUZZ_HARNESS]

- Owner: `SnapshotCodecFuzz` — the dedicated `tests/csharp/_fuzz` out-of-process harness project entry point for the snapshot codec and restore-ladder untrusted-data boundary.
- Entry: `public static void Main()` calling `Fuzzer.OutOfProcess.Run(Action<Stream>)` — the raw-byte stream overload, never the `Action<string>` overload, because the boundary admits binary frames with no UTF-8 assumption.
- Auto: `afl-fuzz -i <corpus> -o <findings> -- dotnet <harness>.dll` drives the campaign; the in-process `Run(Action<Stream>)` falls through to a single standalone execution under CI when `__AFL_SHM_ID` is absent, so the same entry point gates as a one-shot smoke under `test run` and as a coverage-guided session under afl-fuzz with no second harness.
- Receipt: no receipt — a survived input records a zero exit, an uncaught exception records the AFL FAULT_CRASH return code `2`, and a crashing input persists as a findings-dir artifact under the project artifact root.
- Packages: SharpFuzz, MessagePack, K4os.Compression.LZ4, System.IO.Hashing, LanguageExt.Core, BCL inbox.
- Growth: a new untrusted decode surface is one arm on the `Drive` fold, never a second harness project; a new seed shape is one corpus file under the findings root, never a code change.
- Boundary: the harness asserts the total-rejection law — every malformed byte sequence resolves to a typed `Snapshots.Verify` `RejectTier` rejection or a deserializer-internal fault the fold catches, never an unhandled escape past `Drive`, so the ONE tier ladder (`Snapshots.Verify`), `CompressionPolicy.Unpack`, and the codec `Foreign` `UntrustedData` resolver together prove crash-safe under adversarial input; the fuzzer drives the SAME `Snapshots.Verify` the production `Read`/`Restore` gate through with a wildcard fingerprint/epoch so a malformed header rejects at its tier before any body decode and only a ladder-verified artifact reaches the codec, making a divergence between the fuzzed gate and the production read impossible by construction; the round-trip-identity arm seeds the corpus with real sealed-snapshot bytes and asserts that a header-valid, hash-matching payload decodes to a value whose re-seal reproduces the same content address, so the fuzzer also exercises the success path a malformed-only corpus starves; the `Foreign` resolver (`MessagePackSecurity.UntrustedData`) is the only codec route the harness drives because it is the cross-process payload boundary the restore lane reads — the trusted `Binary` route is never fuzzed because no untrusted byte reaches it; `Drive` swallows only the deserializer and codec fault taxonomy and rethrows any `OutOfMemoryException`/`StackOverflowException`-class fault so an unbounded-allocation defect surfaces as a crash rather than a silent catch; this cluster owns the harness design and the `tests/csharp/_fuzz` project is its source home.

```csharp signature
public static class SnapshotCodecFuzz {
    public static void Main() => Fuzzer.OutOfProcess.Run(Drive);

    private static void Drive(Stream data) {
        using var whole = new MemoryStream();
        data.CopyTo(whole);
        var artifact = whole.ToArray();
        _ = Snapshots.Verify(artifact, schemaFingerprint: AnyFingerprint(artifact), epoch: AnyEpoch(artifact)).Match(
            Succ: header => DriveBody(header, artifact.AsMemory(SnapshotHeader.Size)),
            Fail: static _ => unit);                 // a ladder rejection is the proved-safe outcome, never a crash
    }

    // A header-valid artifact carries its own fingerprint AND epoch; reading both back as the verify ceiling makes them
    // a wildcard so the epoch and schema gates admit every header-valid input and the fuzzer reaches the codec on the
    // success path too — a fixed `epoch: 0` ceiling would reject every real artifact (epoch > 0) at the VersionAhead tier.
    private static ulong AnyFingerprint(ReadOnlySpan<byte> artifact) =>
        artifact.Length >= SnapshotHeader.Size ? BinaryPrimitives.ReadUInt64LittleEndian(artifact[20..]) : 0;

    private static ulong AnyEpoch(ReadOnlySpan<byte> artifact) =>
        artifact.Length >= SnapshotHeader.Size ? BinaryPrimitives.ReadUInt64LittleEndian(artifact[28..]) : 0;

    private static Unit DriveBody(SnapshotHeader header, ReadOnlyMemory<byte> framed) {
        return (CompressionPolicy.ByHeaderId(header.CompressionId), SnapshotCodec.ByHeaderId(header.CodecId)).Sequence().Match(
            Some: pair => {
                try {
                    var payload = pair.Item1.Unpack(framed);
                    _ = pair.Item2 == SnapshotCodec.MessagePackBinary
                        ? MessagePackSerializer.Deserialize<object>(payload, SnapshotCodec.Foreign)
                        : pair.Item2 == SnapshotCodec.JsonStj
                            ? JsonSerializer.Deserialize(payload, typeof(SnapshotCatalogRow), SnapshotCodec.SnapshotJson)
                            : payload;
                } catch (Exception ex) when (ex is MessagePackSerializationException or JsonException or InvalidDataException or FormatException) { }
                return unit;
            },
            None: static () => unit);
    }
}
```

## [08]-[SCHEMA_EVOLUTION]

- Owner: `SchemaVersion` the unified schema-version registry row; `CodecLineageEdge` the schema/codec-version provenance edge; `SchemaEvolution` the static surface owning the schema-version registration, the content negotiation, and the codec-as-lineage projection. There is NO parallel wire-format axis — the negotiable formats ARE the `SnapshotCodec` rows under their `WireSurface` `Membership` and `NegotiationRank`, so `Negotiate` filters the codec axis, never a second enum mirroring it.
- Cases: a schema version is a `(TypeName, Version, Fingerprint)` triple with its forward/backward compatibility flags; a codec-lineage edge is a `WasDerivedFrom` relating a payload's codec/schema version to its predecessor.
- Entry: `public static FrozenDictionary<(string, int), SchemaVersion> Build(Seq<SchemaVersion> versions)` freezes the registry from every wire type's compiled version; `public static Option<SchemaVersion> Resolve(FrozenDictionary<(string, int), SchemaVersion> registry, SnapshotHeader header)` resolves a sealed snapshot's `(CodecVersion, SchemaFingerprint)` to its row so a payload self-describes its schema; `public static Fin<SnapshotCodec> Negotiate(WireSurface surface, Seq<string> accepted)` intersects the surface's offered `SnapshotCodec` rows with the consumer's accepted codec keys and picks the highest `NegotiationRank`; `public static CodecLineageEdge Edge(UInt128 payload, SchemaVersion from, SchemaVersion to)` projects the codec/schema version as a provenance edge.
- Auto: the schema-version registry is unified across the codec axis and the `SchemaFingerprint` so one registry answers "what schema version produced this payload" — every sealed snapshot's header `CodecVersion` plus the `SchemaFingerprint` resolves to a `SchemaVersion` row, so a payload self-describes its schema; content negotiation reads the codec rows the `WireSurface` admits through `Membership` and the consumer's accepted codec keys and picks the highest `NegotiationRank` mutually-supported one so a JSON-only web consumer reads json-stj while a binary peer reads messagepack — a `file-raw` request on the `web` surface is unrepresentable because `Membership` withholds it; codec and schema versions are provenance edges (`Version/provenance#CAUSAL_DAG` `WasDerivedFrom`) so a schema migration's effect on a payload is a lineage edge — "this payload was re-encoded from schema v3 to v4" is a join dimension, not a hidden re-write.
- Receipt: a registration rides `store.schema.register`; a negotiation rides `store.wire.negotiate` carrying the selected codec key; a codec-lineage edge folds into the `Version/provenance#CAUSAL_DAG`.
- Packages: System.IO.Hashing, MessagePack, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new schema version is one `SchemaVersion` registry row; a new compatibility flag is one column; a new negotiable format is one `SnapshotCodec` row plus its `Membership`/`NegotiationRank` columns (never a parallel format row); zero new surface — a per-version codec class, a hardcoded format pick, a parallel wire-format enum, or a silent schema re-write is the deleted form because the registry is unified with the codec axis and the fingerprint, negotiation is the codec axis filtered by surface membership, and a codec/schema version change is a provenance edge.
- Boundary: the schema-version registry is unified with the `SnapshotCodec` `CodecVersion` and the `SchemaFingerprint` so one registry resolves a payload's schema version — a separate version registry per codec or a version field duplicated across surfaces is the deleted form; content negotiation is the codec axis filtered by `WireSurface` `Membership` and ordered by `NegotiationRank` so a consumer advertising its accepted codec keys receives the highest mutually-supported one, and a hardcoded format pick, a server-imposed single format, or a parallel `WireFormat` enum duplicating the codec rows is the deleted form — the JSON-STJ row serves web consumers, the MessagePack row serves binary peers, and the negotiation picks per-consumer; codec and schema versions are provenance edges so a schema evolution is a lineage join dimension — a payload re-encoded under a new schema version carries a `WasDerivedFrom` edge to its prior version so the migration is auditable through the same provenance DAG, and a silent in-place schema re-write that loses the version history is the deleted form; forward/backward compatibility flags gate a read — a payload at a schema version newer than the reader's, where the version is not forward-compatible, folds to the `RESTORE_AND_DIFF` version-unsupported rejection rather than a best-effort decode, the same gate the snapshot header `CodecVersion` check enforces.

```csharp signature
public readonly record struct SchemaVersion(string TypeName, int Version, ulong Fingerprint, bool ForwardCompatible, bool BackwardCompatible);

public readonly record struct CodecLineageEdge(UInt128 Payload, string TypeName, int FromVersion, int ToVersion, ulong FromFingerprint, ulong ToFingerprint);

public static class SchemaEvolution {
    // The registry is the union of every wire type's compiled SchemaVersion, keyed by (TypeName, Version) AND indexed
    // by Fingerprint, so a sealed snapshot's (CodecVersion, SchemaFingerprint) self-describes its schema row.
    public static FrozenDictionary<(string, int), SchemaVersion> Build(Seq<SchemaVersion> versions) =>
        versions.ToDictionary(static v => (v.TypeName, v.Version), static v => v).ToFrozenDictionary();

    public static Option<SchemaVersion> Resolve(FrozenDictionary<(string, int), SchemaVersion> registry, string typeName, int version) =>
        registry.TryGetValue((typeName, version), out var found) ? Some(found) : None;

    public static Option<SchemaVersion> Resolve(FrozenDictionary<(string, int), SchemaVersion> registry, SnapshotHeader header) =>
        toSeq(registry.Values).Find(v => v.Fingerprint == header.SchemaFingerprint && v.Version == header.CodecVersion).ToOption();

    // Negotiation is the codec axis filtered by WireSurface membership, NOT a parallel format enum: a consumer's
    // accepted codec keys intersect the surface's offered SnapshotCodec rows and the highest NegotiationRank wins.
    public static Fin<SnapshotCodec> Negotiate(WireSurface surface, Seq<string> accepted) =>
        SnapshotCodec.Negotiate(surface, accepted);

    public static CodecLineageEdge Edge(UInt128 payload, SchemaVersion from, SchemaVersion to) =>
        new(payload, to.TypeName, from.Version, to.Version, from.Fingerprint, to.Fingerprint);

    public static Fin<Unit> AdmitRead(SchemaVersion payload, SchemaVersion reader) =>
        payload.Version > reader.Version && !payload.ForwardCompatible
            ? Fin.Fail<Unit>(Error.New($"<schema-version-unsupported:{payload.TypeName}:{payload.Version}>"))
            : payload.Version < reader.Version && !reader.BackwardCompatible
                ? Fin.Fail<Unit>(Error.New($"<schema-version-too-old:{payload.TypeName}:{payload.Version}>"))
                : Fin.Succ(unit);
}
```

## [09]-[TS_PROJECTION]

- Owner: `SnapshotCodecKey`, `SnapshotCompressionKey`, `DataClassificationKey`, `SnapshotHeaderWire`, `SnapshotCatalogRowWire`, `SnapshotDeltaWire`, `RestoreReceiptWire`, `SnapshotDecodeOptions`, `SnapshotExtensionRows` — the page's wire transcription.
- Packages: BCL inbox.
- Growth: a custom extension byte lands as one `SnapshotExtensionRows` row paired with one TS ExtensionCodec registration row; zero new surface.
- Boundary: every codec shape crosses primitive-mapped, so the extension union is `never`; `useBigInt64` aligns 64-bit integers with bigint and `schemaFingerprint`/`epoch`/`plainLength`/`storedLength` read through DataView `getBigUint64`/`getBigInt64` on the fixed-72-byte header prefix at their declared offsets; `contentHash` crosses as the 32-hex-char `XxHash128` string the catalog `hash` mirrors; instants cross as ISO-8601 strings and `elapsed` as the roundtrip-pattern string; `hlcLogical` resets on every physical advance and stays inside the JSON number envelope; the delta crosses as one `KeyEdit` array carrying the per-kind `from`/`to` content addresses rather than three flat name lists, and the `change` discriminant is the `ChangeKind` key scalar; smart-enum columns cross as their key scalars and `target` carries the store-profile key string.

```ts contract
type SnapshotCodecKey = "json-stj" | "messagepack" | "file-raw";

type SnapshotCompressionKey = "none" | "lz4-fast" | "lz4-high";

type DataClassificationKey = "none" | "operational" | "host-identity" | "user-content" | "personal" | "credential" | "secret";

type ChangeKindKey = "added" | "removed" | "replaced" | "converged";

// Fixed 72-byte little-endian prologue; offsets are load-bearing for the DataView reader.
interface SnapshotHeaderWire {
  readonly magic: number;            // u32 @0
  readonly version: number;          // u8  @4
  readonly hashDomain: number;       // u8  @5
  readonly codecId: number;          // i32 @8
  readonly codecVersion: number;     // i32 @12
  readonly compressionId: number;    // i32 @16
  readonly schemaFingerprint: bigint; // u64 @20
  readonly epoch: bigint;            // u64 @28
  readonly plainLength: bigint;      // i64 @36
  readonly storedLength: bigint;     // i64 @44
  readonly contentHash: string;      // u128 @52 -> 32 hex chars
  readonly checksum: number;         // u32 @68 (Crc32 over [0..68))
}

interface SnapshotCatalogRowWire {
  readonly id: string;
  readonly kind: string;
  readonly codec: SnapshotCodecKey;
  readonly compression: SnapshotCompressionKey;
  readonly hash: string;
  readonly plainLength: number;
  readonly storedLength: number;
  readonly schemaFingerprint: bigint;
  readonly epoch: bigint;
  readonly lineage: string | null;
  readonly writtenAt: string;
  readonly retentionClass: string;
  readonly classification: DataClassificationKey;
  readonly hlcPhysical: string;
  readonly hlcLogical: number;
  readonly correlation: string;
}

interface KeyEditWire {
  readonly kind: string;
  readonly change: ChangeKindKey;
  readonly from: string | null;
  readonly to: string | null;
}

interface SnapshotDeltaWire {
  readonly edits: readonly KeyEditWire[];
}

interface RestoreReceiptWire {
  readonly source: string;
  readonly verifiedHash: string;
  readonly target: string;
  readonly elapsed: string;
  readonly at: string;
  readonly correlation: string;
}

interface SnapshotDecodeOptions {
  readonly useBigInt64: true;
}

type SnapshotExtensionRows = never;
```

## [10]-[RESEARCH]

- [SCHEMA_NEGOTIATION]: the schema-version-to-`SchemaFingerprint` resolution is settled (`SchemaEvolution.Build`/`Resolve` over the header's `(CodecVersion, SchemaFingerprint)`); the residual is the content-negotiation wire-format advertisement a peer and a web consumer exchange — whether the negotiation rides the gRPC metadata / HTTP Accept header the AppHost wire law owns or a payload-prefix format byte, confirmed against the live compiled-model fingerprint set fed to `Build`.
- [RENAME_DURABILITY]: the single-pass seal (placeholder header → stored bytes → final header) and the data-flush-before-rename order are settled; the one named accepted residual is directory-entry durability — whether APFS guarantees the rename's directory entry survives a power loss without an explicit parent-directory fsync, and the managed route to that fsync if it does not (directory-entry durability is unreachable from managed code per `docs/stacks/csharp/domain/durability` SEAL_LAW).
- [RESOLVER_PRECEDENCE]: `ThinktectureMessageFormatterResolver` coverage over keyed unions and complex value objects composed with `SourceGeneratedFormatterResolver` under Lz4BlockArray; the precedence of the `PersistenceResolver` `[CompositeResolver]`/`[GeneratedMessagePackResolver]` AOT landmark over the runtime `CompositeResolver.Create` chain and over `SourceGeneratedFormatterResolver` when both resolve one generated type; `GeoJsonConverterFactory` precedence over combined source-generated contract metadata for geometry-bearing wire records.
- [ZSTD_SWAP]: the inbox Zstandard stream surface a future TFM exposes — its managed `Pack`/`Unpack` shape and level vocabulary for the deferred `CompressionPolicy` row, gated on the framework move that admits it.
- [CDC_DEDUP_RATIO]: the cross-snapshot chunk-reuse rate and the under-edit-churn dedup ratio the FastCDC `ChunkPolicy.Snapshot` window holds against a real model-history corpus — whether the content-defined boundary survives an interior insertion that shifts every fixed window and how the `min/avg/max` window trades dedup ratio against chunk-count overhead, measured before the `ChunkPolicy` window literals finalize; the `FastCdc.GetChunks` cut algorithm and the `Chunk` offset/length/hash record are settled against the catalogue.

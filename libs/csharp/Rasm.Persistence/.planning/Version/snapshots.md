# [PERSISTENCE_SNAPSHOT_CODECS]

Rasm.Persistence encodes every durable payload through one three-row `SnapshotCodec` axis paired with the `CompressionPolicy` and `HashPolicy` axes, seals payloads under the snapshot header and atomic-write protocol, catalogs them with classification and retention columns, splits opaque payload bytes into content-defined chunks for cross-snapshot dedup, and projects restore and diff evidence; `PersistenceWireContext` is the package's one JsonSerializerContext partial joining the suite Strict merge. Codec, compression, and hash variance are delegate rows on string-keyed smart enums, the writer fold is the page's single entrypoint family over MessagePack, System.Text.Json source generation, LZ4 block framing, FastCDC content-defined chunking, and System.IO.Hashing, and every stamp rides the AppHost clock, correlation, and receipt spine as settled vocabulary.

## [01]-[INDEX]

- [01]-[CODEC_AXIS]: three codec rows, package wire context, and generated converter admission.
- [02]-[COMPRESSION_HASHING]: compression rows, hash rows, and framing routes and identity values.
- [03]-[SNAPSHOT_PROTOCOL]: header law, atomic write fold, catalog row, and orphan sweep.
- [04]-[CONTENT_CHUNKING]: FastCDC content-defined chunk boundaries; per-chunk content-key dedup.
- [05]-[RESTORE_AND_DIFF]: verified read, restore receipt parity, and content-addressed diff.
- [06]-[FUZZ_HARNESS]: out-of-process SharpFuzz over the codec/restore untrusted boundary.
- [07]-[SCHEMA_EVOLUTION]: content-negotiated wire formats, schema-version registry, and codec-as-lineage.
- [08]-[TS_PROJECTION]: wire shapes and msgpack alignment the dashboard consumes.

## [02]-[CODEC_AXIS]

- Owner: `SnapshotCodec` `[SmartEnum<string>]` under the `SnapshotKeyPolicy` ordinal accessor; `PersistenceWireContext` as the package wire context; `InstantFormatter` as the one primitive-mapped NodaTime formatter row; `WireSurface` as the wire-surface vocabulary carried per codec row as a frozen membership set; `GeoJsonProjection` as the one `GeoJsonConverterFactory` admission; `PersistenceResolver` as the AOT MessagePack resolver landmark.
- Cases: 3 codec rows — json-stj, messagepack, file-raw; 4 wire surfaces — snapshot, cache, sync, web.
- Entry: `public partial byte[] Serialize(Type shape, object? value)` — pure byte transform; shape-discriminated dispatch serves every registered wire record through source-generated metadata.
- Auto: registering `ThinktectureJsonConverterFactory` and `ThinktectureMessageFormatterResolver.Instance` once derives every value-object, smart-enum, and keyed-union converter and formatter — per-type hand-written codec classes are the deleted pattern; `GeoJsonProjection` admits one `GeoJsonConverterFactory` deriving the GeoJSON projection of every NetTopologySuite geometry, feature, and attribute table on the STJ rail.
- Packages: MessagePack, MessagePackAnalyzer, Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, Thinktecture.Runtime.Extensions.MessagePack, NetTopologySuite.IO.GeoJSON4STJ, NodaTime, NodaTime.Serialization.SystemTextJson, BCL inbox.
- Growth: one codec is one row; a new wire record is one `[JsonSerializable]` row on `PersistenceWireContext` plus one MessagePack union tag row when polymorphic; the AOT resolver landmark is the `PersistenceResolver` partial carrying `[CompositeResolverAttribute]` over `[GeneratedMessagePackResolverAttribute]`, so a published-AOT build binds the generated resolver and a JIT build keeps the runtime `CompositeResolver.Create` chain — the swap is one `Foreign`/`Binary` resolver-field selection, never a second codec; a new geometry policy is one column on the `GeoJsonProjection` factory record; zero new surface.
- Boundary: artifact-kind-to-codec residence is fixed at write — a second codec on one kind is a conflict, not a fallback; `GeoJsonProjection.Factory` carries the whole geometry wire profile so geometry wire records hold no per-call geometry policy — `writeGeometryBBox` stamps the GeoJSON `bbox`, `GeoJsonConverterFactory.DefaultIdPropertyName` lifts a feature `id` out of `properties`, and `allowModifyingAttributesTables: false` keeps deserialized tables read-only `JsonElementAttributesTable` projections; the `ThinktectureJsonConverterFactory(skipObjectsWithJsonConverterAttribute: true)` and `ThinktectureMessageFormatterResolver(skipObjectsWithMessagePackFormatterAttribute: true)` ctors arm only where a source-context already owns a domain type's converter, so a doubly-registered converter never shadows the source-generated one; the `PersistenceResolver` AOT landmark composes ahead of the runtime `CompositeResolver.Create` chain under a published-AOT build; `ProjectJson` and the `ContractlessStandardResolver` tail are diagnostic egress only, never a payload route or wire surface; `Foreign` gates every cross-process payload; the whole-archive codec path streams through `SerializeAsync`/`DeserializeAsync` and `MessagePackStreamReader`'s length-delimited segment sequence so a multi-segment op-log or snapshot decodes one frame at a time; an extension typecode crosses through `ExtensionHeader`/`ExtensionResult` and stays absent on the wire so the TS ext-union is `never`; NodaTime intervals bind ISO through `WithIsoIntervalConverter`/`WithIsoDateIntervalConverter` and a `[Union]`/`[SmartEnum]` key parses inbound from the UTF-8 span through `ThinktectureSpanParsableJsonConverter`; MessagePack union tag rows are append-only and a retired tag never returns; a hand-written converter or formatter beside the generated ones is the named defect; MemoryPack, CBOR, and protobuf snapshot encodings stay rejected — proto owns RPC payloads only; the MessagePackBinary row is the value the cache serializer registration consumes.

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
        "json-stj", headerId: 1, version: 1, membership: new[] { WireSurface.Snapshot, WireSurface.Web }.ToFrozenSet(),
        serialize: static (shape, value) => JsonSerializer.SerializeToUtf8Bytes(value, shape, SnapshotJson),
        deserialize: static (shape, payload) => JsonSerializer.Deserialize(payload.Span, shape, SnapshotJson),
        projectJson: static payload => Encoding.UTF8.GetString(payload.Span));
    public static readonly SnapshotCodec MessagePackBinary = new(
        "messagepack", headerId: 2, version: 1,
        membership: new[] { WireSurface.Snapshot, WireSurface.Cache, WireSurface.Sync, WireSurface.Web }.ToFrozenSet(),
        serialize: static (shape, value) => MessagePackSerializer.Serialize(shape, value, Binary),
        deserialize: static (shape, payload) => MessagePackSerializer.Deserialize(shape, payload, Binary),
        projectJson: static payload => MessagePackSerializer.ConvertToJson(payload, Binary));
    public static readonly SnapshotCodec FileRaw = new(
        "file-raw", headerId: 3, version: 1, membership: new[] { WireSurface.Snapshot }.ToFrozenSet(),
        serialize: static (_, value) => (byte[])value!,
        deserialize: static (_, payload) => payload.ToArray(),
        projectJson: static payload => $"{{\"opaqueLength\":{payload.Length}}}");

    public int HeaderId { get; }
    public int Version { get; }
    public FrozenSet<WireSurface> Membership { get; }
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
- Cases: 3 compression rows — none, lz4-fast, lz4-high; 5 hash rows — Content, Identity, Frame, Wide, FrameWide.
- Entry: `public partial byte[] Pack(ReadOnlyMemory<byte> payload)` — pure byte transform; the row delegate is total over any payload size.
- Packages: K4os.Compression.LZ4, MessagePack, System.IO.Hashing, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: one compression level or hash algorithm is one row; a Zstandard level lands as one `CompressionPolicy` delegate row carrying its own `HeaderId` so the snapshot header's `CompressionId` keeps every prior LZ4 archive readable across the swap; zero new surface.
- Boundary: every hash row is non-cryptographic identity — a security or tamper claim on any row is the named defect; compression evidence never obscures redaction or retention receipts; the MessagePackBinary codec pairs with the none row at write because Lz4BlockArray owns compression in-codec — double framing is the deleted pattern; the Frame row belongs to artifact frame checks and never stands in for Identity; `Identity` pins `XxHash128` as the one federation content-address algorithm every snapshot identity, sealed `Hash`, chunk key, diff, and dedup surface computes — `Seal`, `ContentAddress`, `#CONTENT_CHUNKING`, `Query/cache#ARTIFACT_BLOB_INDEX`, and `Version/diff#STRUCTURAL_DIFF` all read this one 128-bit address, so its algorithm cannot vary and a 64-bit hash standing in for the content address is the deleted form; `Content`, `Wide`, and `FrameWide` are the narrower 64-bit non-identity tags whose collision domains stay statistically independent of the content address without the 128-bit cost of `Identity` — `Content` is `XxHash3` for a fast short tag stamped on every `#CONTENT_CHUNKING` `ContentChunk` as its `ShortTag`, so the chunk-dedup lookup probes a 64-bit bloom/sketch membership pre-filter ahead of the authoritative 128-bit `XxHash128` content-key compare and a tag-miss skips the lookup entirely on a hot re-store path; `Wide` is `XxHash64` for a wide artifact-catalog index, and `FrameWide` is `Crc64` for whole-archive frame integrity where `Frame`'s 32-bit check is too narrow — none of the three is the content address, and `Content` is the pre-filter face only, never a dedup identity, so a `ShortTag` collision always falls through to the `Identity` compare; every row folds once into the `Bits`/`HexFormat` columns so a tag width is data, never a per-call format string.

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

    public int HeaderId { get; }
    [UseDelegateFromConstructor]
    public partial byte[] Pack(ReadOnlyMemory<byte> payload);
    [UseDelegateFromConstructor]
    public partial byte[] Unpack(ReadOnlyMemory<byte> framed);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<SnapshotKeyPolicy, string>]
[KeyMemberComparer<SnapshotKeyPolicy, string>]
public sealed partial class HashPolicy {
    public static readonly HashPolicy Content = new("xxhash3", bits: 64, hexFormat: "x16", compute: static payload => XxHash3.HashToUInt64(payload.Span));
    public static readonly HashPolicy Identity = new("xxhash128", bits: 128, hexFormat: "x32", compute: static payload => XxHash128.HashToUInt128(payload.Span));
    public static readonly HashPolicy Frame = new("crc32", bits: 32, hexFormat: "x8", compute: static payload => Crc32.HashToUInt32(payload.Span));
    public static readonly HashPolicy Wide = new("xxhash64", bits: 64, hexFormat: "x16", compute: static payload => XxHash64.HashToUInt64(payload.Span));
    public static readonly HashPolicy FrameWide = new("crc64", bits: 64, hexFormat: "x16", compute: static payload => Crc64.HashToUInt64(payload.Span));

    public int Bits { get; }
    public string HexFormat { get; }
    [UseDelegateFromConstructor]
    public partial UInt128 Compute(ReadOnlyMemory<byte> payload);

    public string Tag(ReadOnlyMemory<byte> payload) =>
        Compute(payload).ToString(HexFormat, CultureInfo.InvariantCulture);
}
```

| [INDEX] | [ROUTE]          | [PAYLOAD_CLASS]              | [MECHANISM]                                                   | [VALUE]                                                                |
| :-----: | :--------------- | :--------------------------- | :------------------------------------------------------------ | :--------------------------------------------------------------------- |
|  [01]   | in-codec         | MessagePackBinary payloads   | `MessagePackCompression.Lz4BlockArray` blocks                 | `CompressionMinLength` provider default 64 bytes                       |
|  [02]   | standalone frame | JsonStj and FileRaw payloads | `LZ4Pickler` self-describing frame                            | level from the selected row                                            |
|  [03]   | streaming frame  | payloads above 1 MiB         | `LZ4Encoder`/`LZ4ChainDecoder` `Topup`/`Drain` chained blocks | `SuggestedContiguousMemorySize` 1 MiB segmenting, no contiguous buffer |
|  [04]   | raw block        | fixed known-length spans     | `LZ4Codec.Encode`/`Decode` into a caller buffer               | `LZ4Codec.MaximumOutputSize` bounds the destination                    |

## [04]-[SNAPSHOT_PROTOCOL]

- Owner: `SnapshotHeader`, `SealedSnapshot`, `SnapshotCatalogRow`, `Snapshots` — the header law, the atomic write fold, and the orphan sweep.
- Entry: `public static IO<SnapshotCatalogRow> Write<T>(ReceiptSinkPort sink, CorrelationId correlation, string directory, string kind, SnapshotCodec codec, CompressionPolicy compression, ulong schemaFingerprint, DataClassification classification, string retentionClass, T value, Func<SnapshotCatalogRow, IO<Unit>> persist)` — `IO` carries the file-system and sink effects; one call encodes, packs, hashes, seals, stamps, and persists.
- Auto: the write fold derives codec id, compression id, schema fingerprint, identity hash, HLC stamp, classification, and retention class into the catalog row — per-call-site assembly ceremony is the deleted pattern; the schema fingerprint value arrives from the compiled-model fingerprint law and the row identity from the UuidV7Key identity row; the sealed `Hash` is the content address every secondary surface derives from, so a sealed snapshot is automatically catalog-addressable on the artifact-blob index without a parallel key — `ContentAddress` selects the cache tag, the blob lookup, and the diff identity from one value.
- Receipt: `SnapshotCatalogRow` is the durable evidence; the `SealedSnapshot` payload rides the receipt envelope at the sink edge, so the catalog HLC stamp and the receipt stamp are one value.
- Packages: System.IO.Hashing, NodaTime, LanguageExt.Core, BCL inbox.
- Growth: a new header capability is one flag bit row on `Flags`; one artifact kind is one catalog row value; the chunk-dedup hand-off is one `ContentChunker.Chunk` fold over the packed payload before catalog insertion (`#CONTENT_CHUNKING`), never a second framing; zero new surface.
- Boundary: `Seal` and the sweep deletion kernel are this fence's boundary capsules — language-owned statement forms stay inside those two bodies; `directory` arrives from the placement law and is never derived here; the durability order is settled — `Flush(flushToDisk: true)` forces the data blocks before `File.Move` performs the atomic rename, so a crash between the two leaves the temp file swept rather than a torn final; temp residue and catalog-orphaned payloads leave only through `Sweep`; catalog insertion enters through the `persist` delegate so the store rail stays the single write path; `ContentAddress` parses the sealed `Hash` so the cache, blob, and diff surfaces share one content key and never mint parallel identities; the magic constant spells RSNP in little-endian byte order.

```csharp signature
public readonly record struct SnapshotHeader(
    uint Magic, int CodecId, int CodecVersion, ulong SchemaFingerprint, int CompressionId, uint Flags)
{
    public const int Size = 28;

    public const uint MagicValue = 0x504E5352;

    public static Fin<SnapshotHeader> Parse(ReadOnlySpan<byte> prefix) =>
        prefix.Length < Size ? Fin.Fail<SnapshotHeader>(Error.New("<snapshot-header-truncated>"))
        : BinaryPrimitives.ReadUInt32LittleEndian(prefix) != MagicValue ? Fin.Fail<SnapshotHeader>(Error.New("<snapshot-magic-mismatch>"))
        : Fin.Succ(new SnapshotHeader(
            BinaryPrimitives.ReadUInt32LittleEndian(prefix),
            BinaryPrimitives.ReadInt32LittleEndian(prefix[4..]),
            BinaryPrimitives.ReadInt32LittleEndian(prefix[8..]),
            BinaryPrimitives.ReadUInt64LittleEndian(prefix[12..]),
            BinaryPrimitives.ReadInt32LittleEndian(prefix[20..]),
            BinaryPrimitives.ReadUInt32LittleEndian(prefix[24..])));

    public void Write(Span<byte> destination) {
        BinaryPrimitives.WriteUInt32LittleEndian(destination, Magic);
        BinaryPrimitives.WriteInt32LittleEndian(destination[4..], CodecId);
        BinaryPrimitives.WriteInt32LittleEndian(destination[8..], CodecVersion);
        BinaryPrimitives.WriteUInt64LittleEndian(destination[12..], SchemaFingerprint);
        BinaryPrimitives.WriteInt32LittleEndian(destination[20..], CompressionId);
        BinaryPrimitives.WriteUInt32LittleEndian(destination[24..], Flags);
    }
}

public readonly record struct SealedSnapshot(Guid Id, string Path, string Hash, long Length);

public sealed record SnapshotCatalogRow(
    Guid Id,
    string Kind,
    SnapshotCodec Codec,
    CompressionPolicy Compression,
    string Hash,
    long Length,
    Instant WrittenAt,
    string RetentionClass,
    DataClassification Classification,
    Instant HlcPhysical,
    ulong HlcLogical,
    CorrelationId Correlation);

public static class Snapshots {
    public const string Suffix = ".rsnp";

    public static UInt128 ContentAddress(SnapshotCatalogRow row) =>
        UInt128.Parse(row.Hash, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    public static IO<SnapshotCatalogRow> Write<T>(
        ReceiptSinkPort sink,
        CorrelationId correlation,
        string directory,
        string kind,
        SnapshotCodec codec,
        CompressionPolicy compression,
        ulong schemaFingerprint,
        DataClassification classification,
        string retentionClass,
        T value,
        Func<SnapshotCatalogRow, IO<Unit>> persist) =>
        IO.lift(() => Seal(directory, Guid.CreateVersion7(), codec, compression, schemaFingerprint, codec.Serialize(typeof(T), value)))
            .Bind(file => sink
                .Send(correlation, TenantContext.Current, "Rasm.Persistence", kind, JsonSerializer.SerializeToElement(file, SnapshotCodec.SnapshotJson))
                .Map(envelope => new SnapshotCatalogRow(
                    file.Id, kind, codec, compression, file.Hash, file.Length,
                    envelope.Physical, retentionClass, classification,
                    envelope.Physical, envelope.Logical, correlation)))
            .Bind(row => persist(row).Map(_ => row));

    public static IO<Seq<string>> Sweep(string directory, Seq<SnapshotCatalogRow> catalog) =>
        IO.lift(() => toSeq(Directory.EnumerateFiles(directory))
                .Filter(file => !catalog.Exists(row => string.Equals(Path.GetFileName(file), $"{row.Id}{Suffix}", StringComparison.Ordinal))))
            .Bind(static orphans => orphans
                .TraverseM(static file => IO.lift(() => { File.Delete(file); return file; }))
                .As());

    private static SealedSnapshot Seal(
        string directory, Guid id, SnapshotCodec codec, CompressionPolicy compression, ulong schemaFingerprint, byte[] encoded) {
        var packed = compression.Pack(encoded);
        var header = new SnapshotHeader(SnapshotHeader.MagicValue, codec.HeaderId, codec.Version, schemaFingerprint, compression.HeaderId, 0u);
        Span<byte> prefix = stackalloc byte[SnapshotHeader.Size];
        header.Write(prefix);
        var final = Path.Combine(directory, $"{id}{Suffix}");
        var temp = $"{final}.tmp";
        using (var stream = new FileStream(temp, FileMode.CreateNew, FileAccess.Write, FileShare.None)) {
            stream.Write(prefix);
            stream.Write(packed);
            stream.Flush(flushToDisk: true);
        }
        File.Move(temp, final, overwrite: false);
        return new SealedSnapshot(id, final, HashPolicy.Identity.Tag(packed), prefix.Length + packed.LongLength);
    }
}
```

## [05]-[CONTENT_CHUNKING]

- Owner: `ChunkPolicy` the FastCDC min/avg/max size and eof axis with one named policy row; `ContentChunk` the content-keyed chunk record carrying its `XxHash128` address, source offset, and length; `ChunkManifest` the per-payload ordered chunk-key sequence; `ContentChunker` the static surface owning the FastCDC cut, the per-chunk content-key derivation, the manifest fold, and the cross-payload dedup projection.
- Cases: a `ChunkPolicy` row sizes the rolling-hash window — the snapshot row at the 64-KiB-class window, a small-artifact row at a tighter window; a `ContentChunk` carries its content-address key plus its `(Offset, Length)` window; a `ChunkManifest` is the ordered chunk-key sequence reconstructing the payload, and a re-store keys each chunk so only changed chunks transfer.
- Entry: `public static ChunkManifest Chunk(ChunkPolicy policy, ReadOnlyMemory<byte> payload)` — cuts the payload into content-defined chunks through `FastCdc.GetChunks`, keys each by its `XxHash128` content address, and stamps each with the `HashPolicy.Content` 64-bit `XxHash3` short tag; `public static Seq<ContentChunk> Novel(ChunkManifest manifest, Func<ulong, bool> mayHold, Func<UInt128, bool> holds)` projects the chunks a peer or the artifact-blob index lacks, probing the cheap 64-bit `ShortTag` bloom/sketch pre-filter (`mayHold`) before the full 128-bit `holds` content-key compare so a tag-miss skips the lookup; `public static ReadOnlyMemory<byte> Reassemble(ChunkManifest manifest, Func<UInt128, ReadOnlyMemory<byte>> fetch)` rebuilds the payload from its chunk keys.
- Auto: the content-defined boundary is the FastCDC normalized gear-hash cut over `FastCdc(byte[] source, uint minSize, uint avgSize, uint maxSize, bool eof)` so an insertion that shifts every fixed-window boundary leaves the content-defined boundaries stable past the edit, and a small change to a large artifact re-stores only the changed chunks; each chunk's content key is `XxHash128` over the chunk bytes (the `HashPolicy.Identity` row) so an identical chunk across two snapshots or two peers dedups on its key through `Query/cache#ARTIFACT_BLOB_INDEX`; each chunk also carries the `HashPolicy.Content` 64-bit `XxHash3` `ShortTag` so the dedup lookup probes the cheap short tag through a bloom/sketch membership pre-filter ahead of the 128-bit content-key compare — a tag-miss proves the chunk novel without the full lookup, cutting the content-key compares on a hot re-store path, and the short tag is a non-identity pre-filter so a tag-hit always falls through to the authoritative `XxHash128 holds` compare and a false-positive tag never dedups a distinct chunk; the min/avg/max size bounds trace to the `ChunkPolicy` row so the gear-hash mask is data, never a free literal, and clear the `FastCdc.MinimumMin`/`AverageMin`/`MaximumMin` package floors; the per-frame `Crc32` and whole-artifact `XxHash128` integrity (`#SNAPSHOT_PROTOCOL`, `Sync/collaboration#PRESENCE_AND_BLOB`) stay the in-transfer check beside the content-defined dedup boundary.
- Receipt: a chunked store rides `store.chunk.split` carrying the chunk count and the novel-chunk count; a dedup hit rides `store.chunk.dedup` carrying the reused-chunk count and the reused bytes.
- Packages: FastCDC.Net, System.IO.Hashing, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new chunk-size profile is one `ChunkPolicy` row; a new chunk-keyed surface is one consumer of the manifest; zero new surface — a fixed-window framing, a per-edit full re-store, or a second content-defined chunker beside this one is the deleted form because the FastCDC cut is the one opaque-byte boundary and the chunk content key is the one identity the artifact-blob index already dedups on.
- Boundary: this owner is the opaque-byte chunker for snapshot frames and the blob multipart window — it aligns with `Compute/interchange#GEOMETRY_DELTA` `DeltaCodec` at the technique level only, never importing it as a byte chunker and never re-deriving the geometry-structural delta: Compute owns the geometry-aware structural delta over geometry columns, this owns the opaque-byte content-defined chunk over snapshot and blob bytes, and the two meet at the rolling-hash technique, not the code; the `#SNAPSHOT_PROTOCOL` `Seal` fold chunks the packed payload through `ContentChunker.Chunk` so a sealed snapshot's chunks dedup against the artifact-blob index, and the `Store/remote#MULTIPART_TRANSFER` window consumes a whole number of content-defined chunks rather than a fixed `PartSize` slice — the chunk-size column on the `ObjectStore` row clears each provider's part floor while spanning whole chunks, so a re-uploaded artifact skips the chunks the index already holds; the `FastCdc` chunker is stateful and one-shot over an in-memory `byte[]` so a re-chunk requires a fresh instance, and a payload above the 4-GiB `uint`-offset window partitions upstream before chunking — the package owns no stream or file IO; the chunk content key is `XxHash128` (`HashPolicy.Identity`), never the `Chunk.Hash` 32-bit gear-hash cut value, because the gear-hash is a boundary marker and the content address is the dedup identity; the chunk `ShortTag` is `HashPolicy.Content` (`XxHash3`, 64-bit) and is the dedup-lookup pre-filter face only — `Novel` probes `mayHold(ShortTag)` before `holds(ContentKey)` so the cheap tag culls a content-key compare on a hot re-store path while the authoritative dedup identity stays the 128-bit content key, and a `ShortTag` false positive only costs one fall-through `holds` compare, never a wrong dedup; a chunk's `(Offset, Length)` is a `uint` window the `Chunk` record carries, and the manifest reconstructs the payload in chunk order so a torn or reordered manifest is a typed reassembly rejection.

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
        var source = payload.ToArray();
        var chunks = toSeq(policy.Over(source).GetChunks())
            .Map(cut => {
                var span = source.AsSpan((int)cut.Offset, (int)cut.Length);
                return new ContentChunk(
                    XxHash128.HashToUInt128(span),
                    (ulong)HashPolicy.Content.Compute(new ReadOnlyMemory<byte>(source, (int)cut.Offset, (int)cut.Length)),
                    cut.Offset,
                    cut.Length);
            });
        return new ChunkManifest(chunks, XxHash128.HashToUInt128(payload.Span), payload.Length);
    }

    public static Seq<ContentChunk> Novel(ChunkManifest manifest, Func<ulong, bool> mayHold, Func<UInt128, bool> holds) =>
        manifest.Chunks
            .Filter(chunk => !mayHold(chunk.ShortTag) || !holds(chunk.ContentKey))
            .Distinct();

    public static ReadOnlyMemory<byte> Reassemble(ChunkManifest manifest, Func<UInt128, ReadOnlyMemory<byte>> fetch) {
        var buffer = new ArrayBufferWriter<byte>((int)manifest.Length);
        manifest.Chunks.Iter(chunk => buffer.Write(fetch(chunk.ContentKey).Span));
        return buffer.WrittenMemory;
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

- Owner: `RestoreReceipt`, `SnapshotDelta`, `SnapshotRestoreOps` — verified read, restore hand-off parity, and content-addressed diff projection.
- Entry: `public IO<Fin<RestoreReceipt>> Restore(ClockPolicy clocks, CorrelationId correlation, string path, StoreProfile target, Func<string, IO<Fin<Unit>>> repair)` — `Fin` aborts on integrity rejection; `IO` carries the read and hand-off effects.
- Receipt: `RestoreReceipt` carries source id, verified hash, target profile, elapsed `Duration`, and `Instant` stamp — typed restore evidence, never a generic receipt shape.
- Packages: LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new delta classification is one field row on `SnapshotDelta`; zero new surface.
- Boundary: repair mechanics live with the store lifecycle owner and enter through the `repair` delegate — this cluster owns receipt parity only; a forward-incompatible header, codec mismatch, fingerprint mismatch, or hash mismatch is a typed rejection, never a best-effort decode; diff identity is the content hash and object equality never enters the fold.

```csharp signature
public sealed record RestoreReceipt(
    Guid Source, string VerifiedHash, StoreProfile Target, Duration Elapsed, Instant At, CorrelationId Correlation);

public sealed record SnapshotDelta(
    ImmutableArray<string> Added, ImmutableArray<string> Removed, ImmutableArray<string> Changed);

public static class SnapshotRestoreOps {
    public static HashMap<string, string> Index(Seq<SnapshotCatalogRow> rows) =>
        toHashMap(rows.Map(static row => (row.Kind, row.Hash)));

    public static SnapshotDelta Diff(HashMap<string, string> source, HashMap<string, string> target) =>
        new(
            Added: [.. target.Keys.Where(kind => !source.ContainsKey(kind))],
            Removed: [.. source.Keys.Where(kind => !target.ContainsKey(kind))],
            Changed: [.. target.Keys.Where(kind => source.Find(kind).Map(hash => hash != target[kind]).IfNone(false))]);

    extension(SnapshotCatalogRow row) {
        public IO<Fin<T>> Read<T>(string path, ulong schemaFingerprint) =>
            IO.lift(() => File.ReadAllBytes(path)).Map(bytes =>
                SnapshotHeader.Parse(bytes.AsSpan(0, SnapshotHeader.Size)).Bind(header =>
                    header.CodecId != row.Codec.HeaderId ? Fin.Fail<T>(Error.New("<snapshot-codec-mismatch>"))
                    : header.CodecVersion > row.Codec.Version ? Fin.Fail<T>(Error.New("<snapshot-version-unsupported>"))
                    : header.SchemaFingerprint != schemaFingerprint ? Fin.Fail<T>(Error.New("<snapshot-fingerprint-mismatch>"))
                    : HashPolicy.Identity.Tag(bytes.AsMemory(SnapshotHeader.Size)) != row.Hash ? Fin.Fail<T>(Error.New("<snapshot-hash-mismatch>"))
                    : Fin.Succ((T)row.Codec.Deserialize(typeof(T), row.Compression.Unpack(bytes.AsMemory(SnapshotHeader.Size)))!)));

        public IO<Fin<RestoreReceipt>> Restore(
            ClockPolicy clocks, CorrelationId correlation, string path, StoreProfile target, Func<string, IO<Fin<Unit>>> repair) =>
            IO.lift(clocks.Mark).Bind(mark =>
                IO.lift(() => File.ReadAllBytes(path)).Bind(bytes =>
                    HashPolicy.Identity.Tag(bytes.AsMemory(SnapshotHeader.Size)) != row.Hash
                        ? IO<Fin<RestoreReceipt>>.Pure(Fin.Fail<RestoreReceipt>(Error.New("<snapshot-hash-mismatch>")))
                        : repair(path).Map(outcome => outcome.Map(_ =>
                            new RestoreReceipt(row.Id, row.Hash, target, clocks.Elapsed(mark), clocks.Now, correlation)))));
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
- Boundary: the harness asserts the total-rejection law — every malformed byte sequence resolves to a typed `Fin` rejection or a deserializer-internal fault the fold catches, never an unhandled escape past `Drive`, so the codec `Foreign` `UntrustedData` resolver, `CompressionPolicy.Unpack`, `SnapshotHeader.Parse`, and the `HashPolicy.Identity` verify together prove crash-safe under adversarial input; the round-trip-identity arm seeds the corpus with real sealed-snapshot bytes and asserts that a header-valid, hash-matching payload decodes to a value whose re-seal reproduces the same content address, so the fuzzer also exercises the success path a malformed-only corpus starves; the `Foreign` resolver (`MessagePackSecurity.UntrustedData`) is the only codec route the harness drives because it is the cross-process payload boundary the restore lane reads — the trusted `Binary` route is never fuzzed because no untrusted byte reaches it; `Drive` swallows only the deserializer and codec fault taxonomy and rethrows any `OutOfMemoryException`/`StackOverflowException`-class fault so an unbounded-allocation defect surfaces as a crash rather than a silent catch; the harness references the package codec owners directly and declares no parallel decode path, so a divergence between the fuzzed decode and the production `Restore` read is impossible by construction; this cluster owns the harness design and the `tests/csharp/_fuzz` project is its source home.

```csharp signature
public static class SnapshotCodecFuzz {
    public static void Main() => Fuzzer.OutOfProcess.Run(Drive);

    private static void Drive(Stream data) {
        Span<byte> prefix = stackalloc byte[SnapshotHeader.Size];
        var read = data.ReadAtLeast(prefix, SnapshotHeader.Size, throwOnEndOfStream: false);
        _ = SnapshotHeader.Parse(prefix[..read]).Match(
            Succ: header => DriveBody(header, data),
            Fail: static _ => unit);
    }

    private static Unit DriveBody(SnapshotHeader header, Stream data) {
        using var rest = new MemoryStream();
        data.CopyTo(rest);
        var framed = rest.ToArray();
        var compression = Seq(CompressionPolicy.None, CompressionPolicy.Lz4Fast, CompressionPolicy.Lz4High)
            .Find(row => row.HeaderId == header.CompressionId);
        var codec = Seq(SnapshotCodec.JsonStj, SnapshotCodec.MessagePackBinary, SnapshotCodec.FileRaw)
            .Find(row => row.HeaderId == header.CodecId);
        return (compression, codec).Sequence().Match(
            Some: pair => {
                try {
                    var payload = pair.Item1.Unpack(framed);
                    _ = HashPolicy.Identity.Tag(payload);
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

- Owner: `SchemaVersion` the unified schema-version registry row; `WireFormat` the content-negotiable wire-format axis; `CodecLineageEdge` the schema/codec-version provenance edge; `SchemaEvolution` the static surface owning the schema-version registration, the content negotiation, and the codec-as-lineage projection.
- Cases: a schema version registers a `(TypeName, Version, Fingerprint)` triple with its forward/backward compatibility flags; `WireFormat` carries the negotiable formats (json-stj, messagepack, file-raw — the `SnapshotCodec` rows) so a content-negotiation picks the best mutually-supported format; a codec-lineage edge is a `WasDerivedFrom` relating a payload's codec/schema version to its predecessor.
- Entry: `public static SchemaVersion Register(string typeName, int version, ulong fingerprint, bool forwardCompatible, bool backwardCompatible)` — registers a schema version; `public static Fin<WireFormat> Negotiate(Seq<WireFormat> offered, Seq<WireFormat> accepted)` picks the highest mutually-supported wire format; `public static CodecLineageEdge Edge(UInt128 payload, SchemaVersion from, SchemaVersion to)` projects the codec/schema version as a provenance edge.
- Auto: the schema-version registry is unified across the codec axis and the `SchemaFingerprint` so one registry answers "what schema version produced this payload" — every sealed snapshot's header `CodecVersion` plus the `SchemaFingerprint` resolves to a `SchemaVersion` row, so a payload self-describes its schema; content negotiation reads the offered and accepted `WireFormat` sets (a peer or a web consumer advertises its accepted formats, the codec axis offers its supported formats) and picks the highest mutually-supported one so a JSON-only web consumer reads json-stj while a binary peer reads messagepack off the same payload; codec and schema versions are provenance edges (`Version/provenance#CAUSAL_DAG` `WasDerivedFrom`) so a schema migration's effect on a payload is a lineage edge — "this payload was re-encoded from schema v3 to v4" is a join dimension, not a hidden re-write.
- Receipt: a registration rides `store.schema.register`; a negotiation rides `store.wire.negotiate` carrying the selected format; a codec-lineage edge folds into the `Version/provenance#CAUSAL_DAG`.
- Packages: System.IO.Hashing, MessagePack, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new wire format is one `WireFormat` row (a `SnapshotCodec` row's negotiable face); a new schema version is one `SchemaVersion` registry row; a new compatibility flag is one column; zero new surface — a per-version codec class, a hardcoded format pick, or a silent schema re-write is the deleted form because the registry is unified with the codec axis and the fingerprint, negotiation is a set-intersection over the `WireFormat` axis, and a codec/schema version change is a provenance edge.
- Boundary: the schema-version registry is unified with the `SnapshotCodec` `CodecVersion` and the `SchemaFingerprint` so one registry resolves a payload's schema version — a separate version registry per codec or a version field duplicated across surfaces is the deleted form; content negotiation is a set-intersection over the `WireFormat` axis (the negotiable face of the `SnapshotCodec` rows) so a consumer advertising its accepted formats receives the highest mutually-supported one, and a hardcoded format pick or a server-imposed single format is the deleted form — the JSON-STJ row serves web consumers, the MessagePack row serves binary peers, and the negotiation picks per-consumer; codec and schema versions are provenance edges so a schema evolution is a lineage join dimension — a payload re-encoded under a new schema version carries a `WasDerivedFrom` edge to its prior version so the migration is auditable through the same provenance DAG, and a silent in-place schema re-write that loses the version history is the deleted form; forward/backward compatibility flags gate a read — a payload at a schema version newer than the reader's, where the version is not forward-compatible, folds to the `RESTORE_AND_DIFF` version-unsupported rejection rather than a best-effort decode, the same gate the snapshot header `CodecVersion` check enforces.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<SnapshotKeyPolicy, string>]
public sealed partial class WireFormat {
    public static readonly WireFormat JsonStj = new("json-stj", rank: 1);
    public static readonly WireFormat MessagePack = new("messagepack", rank: 3);
    public static readonly WireFormat FileRaw = new("file-raw", rank: 0);

    public int Rank { get; }
}

public readonly record struct SchemaVersion(string TypeName, int Version, ulong Fingerprint, bool ForwardCompatible, bool BackwardCompatible);

public readonly record struct CodecLineageEdge(UInt128 Payload, string TypeName, int FromVersion, int ToVersion, ulong FromFingerprint, ulong ToFingerprint);

public static class SchemaEvolution {
    public static readonly FrozenDictionary<(string, int), SchemaVersion> Registry = FrozenDictionary<(string, int), SchemaVersion>.Empty;

    public static SchemaVersion Register(string typeName, int version, ulong fingerprint, bool forwardCompatible, bool backwardCompatible) =>
        new(typeName, version, fingerprint, forwardCompatible, backwardCompatible);

    public static Fin<WireFormat> Negotiate(Seq<WireFormat> offered, Seq<WireFormat> accepted) =>
        toSeq(offered.Filter(accepted.Contains).OrderByDescending(static format => format.Rank)).HeadOrNone()
            .Match(
                Some: Fin.Succ,
                None: () => Fin.Fail<WireFormat>(Error.New("<wire-format-no-mutual-support>")));

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
- Boundary: every codec shape crosses primitive-mapped, so the extension union is `never`; `useBigInt64` aligns 64-bit integers with bigint and `schemaFingerprint` reads through DataView getBigUint64 on the header prefix; instants cross as ISO-8601 strings and `elapsed` as the roundtrip-pattern string; `hlcLogical` resets on every physical advance and stays inside the JSON number envelope; smart-enum columns cross as their key scalars and `target` carries the store-profile key string.

```ts contract
type SnapshotCodecKey = "json-stj" | "messagepack" | "file-raw";

type SnapshotCompressionKey = "none" | "lz4-fast" | "lz4-high";

type DataClassificationKey = "none" | "operational" | "host-identity" | "user-content" | "personal" | "credential" | "secret";

interface SnapshotHeaderWire {
  readonly magic: number;
  readonly codecId: number;
  readonly codecVersion: number;
  readonly schemaFingerprint: bigint;
  readonly compressionId: number;
  readonly flags: number;
}

interface SnapshotCatalogRowWire {
  readonly id: string;
  readonly kind: string;
  readonly codec: SnapshotCodecKey;
  readonly compression: SnapshotCompressionKey;
  readonly hash: string;
  readonly length: number;
  readonly writtenAt: string;
  readonly retentionClass: string;
  readonly classification: DataClassificationKey;
  readonly hlcPhysical: string;
  readonly hlcLogical: number;
  readonly correlation: string;
}

interface SnapshotDeltaWire {
  readonly added: readonly string[];
  readonly removed: readonly string[];
  readonly changed: readonly string[];
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

- [SCHEMA_NEGOTIATION]: the content-negotiation wire-format advertisement a peer and a web consumer exchange — whether the negotiation rides the gRPC metadata / HTTP Accept header the AppHost wire law owns or a payload-prefix format byte, and the schema-version-to-`SchemaFingerprint` resolution confirming a payload self-describes its schema version against the live compiled-model fingerprint.
- [RENAME_DURABILITY]: the data-flush-before-rename order is settled; the residual is directory-entry durability — whether APFS guarantees the rename's directory entry survives a power loss without an explicit parent-directory fsync, and the managed route to that fsync if it does not.
- [RESOLVER_PRECEDENCE]: `ThinktectureMessageFormatterResolver` coverage over keyed unions and complex value objects composed with `SourceGeneratedFormatterResolver` under Lz4BlockArray; the precedence of the `PersistenceResolver` `[CompositeResolver]`/`[GeneratedMessagePackResolver]` AOT landmark over the runtime `CompositeResolver.Create` chain and over `SourceGeneratedFormatterResolver` when both resolve one generated type; `GeoJsonConverterFactory` precedence over combined source-generated contract metadata for geometry-bearing wire records.
- [ZSTD_SWAP]: the inbox Zstandard stream surface a future TFM exposes — its managed `Pack`/`Unpack` shape and level vocabulary for the deferred `CompressionPolicy` row, gated on the framework move that admits it.
- [CDC_DEDUP_RATIO]: the cross-snapshot chunk-reuse rate and the under-edit-churn dedup ratio the FastCDC `ChunkPolicy.Snapshot` window holds against a real model-history corpus — whether the content-defined boundary survives an interior insertion that shifts every fixed window and how the `min/avg/max` window trades dedup ratio against chunk-count overhead, measured before the `ChunkPolicy` window literals finalize; the `FastCdc.GetChunks` cut algorithm and the `Chunk` offset/length/hash record are settled against the catalogue.

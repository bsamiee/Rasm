# [PERSISTENCE_INGEST_POINTCLOUD]

Rasm.Persistence ingests reality-capture scans through ONE `ScanSource` owner over the E57/LAS/LAZ codec pair — the `[A.4]` Ingest growth row ("the next foreign-file codec into the record path lands as a page HERE") made real: a `ScanFormat` `[SmartEnum<string>]` crosses the three capture wires and settles each from the file magics alone (`ASTM-E57` at byte 0, `LASF` with the offset-104 compressed high bit), `Aardvark.Data.E57` decoding the E57 leg and `Unofficial.laszip.netstandard` the ONE engine for BOTH `.las` and `.laz` because `open_reader_stream` reports compression and a single forward loop decodes either, so a per-compression reader family is the deleted form. This owner is decode-FOR-STORAGE only — registration solving, segmentation, and scan-to-BIM semantics stay the `Rasm` kernel and the `Rasm.Bim` `Exchange/reconstruct#LAS_INGEST` owners — and it keeps the durable half those owners cannot: the `ScanHeader` row, the into-project-frame `ScanRegistration` row, the chunked blob placement over the RAW capture bytes, and the per-region `ScanRegion` H3 cells a windowed read selects on.

`Origin` arrives from `Ingest/tabular#TABULAR_SOURCE`; `ProjectionContext` from `Element/graph#STORE_HOOKS`; `ContentAddress`, `ChunkManifest`, `ChunkPolicy`, and `ContentChunker` from `Element/codec#CONTENT_CHUNKING`; `H3Cell` and its `IdentityStore.Cell` mint from `Element/identity#ELEMENT_IDENTITY`; `MultipartTransfer.Upload` from `Store/blobstore#MULTIPART_TRANSFER`, bound behind the `ScanStore` port the composition root fills; `ICapability`/`CapabilitySet` from `Rasm/Domain/validation#CAPABILITY`, carrying the per-opened-source decode reach; `FaultBand` from the `Rasm/Domain/results#FAULT_BAND` roster, where `ScanFault` occupies the `Scan` decade and facts ride `store.scan.*`.

## [01]-[INDEX]

- [02]-[SCAN_SOURCE]: three-format capability axis with its magic-byte sniff, the `ScanSpec` descriptor and its storage port, the closed ingest/probe/window op family, the ONE chunk-and-cell fold, the codec-native window paths, and the typed fact stream.
- [03]-[SCAN_STORAGE]: four durable rows — extent, header, registration, region — their derivation law, and the retention and policy table binding them to the raw blob.

## [02]-[SCAN_SOURCE]

- Owner: `ScanFormat` is the capture-wire axis owning `Sniff`; `ScanCapability` is the per-opened-source decode reach; `ScanStore` is the two-arrow blob port; `ScanSpec` fixes format, origin, region resolution, and that port; `ScanOp`/`ScanYield` close dispatch; `ScanBatch` is the ONE streaming point currency both codecs fold into; `[FaultCase]` closes the fault roster with `ScanFault` the accumulating `Fault` family above it; `ScanSource` owns `Run`; `E57Codec`/`LasCodec` are the two decode legs.
- Cases: `ScanOp.Ingest` chunks the raw bytes, sweeps the point stream, and yields `Landed`; `ScanOp.Probe` reads metadata and yields `Probed`; `ScanOp.Window` fetches the resident blob and yields `Points`. `ScanFault` is `CodecReject | CrsUnsupported | RegionUnresolvable | WindowMiss`; independent failures accumulate as `Error.Many`.
- Entry: `Run(ScanOp, ProjectionContext)` is the ONE polymorphic entry; blob read and write remain on `ScanSpec.Blob`, so provider selection stays at the composition root.
- Auto: ingest streams the codec's points through ONE fold that simultaneously cuts `ChunkPolicy.Artifact` FastCDC chunks over the RAW bytes and folds each point's cell at `ScanSpec.RegionResolution` into a `HashMap` accumulating `(count, zmin, zmax)` per cell — a streaming fold with no materialized point set at any instant, the manifest handed to `ScanStore.Land` and thence `MultipartTransfer.Upload`. Extent and region rows derive from the DECODED positions rather than the declared header extrema, because a writer's extrema can disagree with its own points and a region row keyed on that disagreement strands every windowed read; the header's point count stays the codec's declared total, which is exactly what a probe answers without a stream. Both legs read position, class, and colour alone — the LAS leg masks the arithmetic decoder through `decompress_selective`, the E57 leg excludes the unread semantics at `StreamPointsFull` so the bit-unpacker skips them. `LasCodec` settles ONE `CapabilitySet<ScanCapability>` at open — colour lanes, extended classes, a usable spatial index — and every step reads that set; E57 channel presence is per-`Data3D` chunk rather than per file, so its lanes settle at the batch and it declares no per-open reach. `Window` folds the codec's OWN spatial access: the laszip `.lax` `inside_rectangle`/`read_inside_point` path when `has_spatial_index` reports one and the full filtered stream otherwise, and the E57 leg prefilters per-`Data3D` cartesian bounds before streaming a setup at all.
- Packages: Aardvark.Data.E57, Unofficial.laszip.netstandard, Rasm (`FaultBand`, `ICapability`/`CapabilitySet`), Rasm.Persistence (`ContentAddress`/`ChunkPolicy`/`ChunkManifest`/`ContentChunker`, `H3Cell`/`IdentityStore.Cell`, `ProjectionContext`, `MultipartTransfer`, `Origin`), pocketken.H3, NetTopologySuite, LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, CommunityToolkit.HighPerformance, BCL inbox.
- Growth: a new capture format is one `ScanFormat` row and one decode leg on the one fold; a new decode reach is one `ScanCapability` row with the derivation that holds it; a new per-point channel is one lane on `ScanBatch` both legs fill; a new op modality is one `ScanOp` case breaking `Run` at compile time; a new fault class is one case inside the registry decade; zero new surface — a per-compression reader family beside the one `is_compressed` report, a hand-rolled E57 XML-plus-binary parser, a second point model beside `ScanBatch`, a materialized point set behind the region fold, a post-decode bbox filter where the codec owns a spatial index, or a scan-to-BIM semantic inside this codec is the deleted form.
- Boundary: decode-for-storage only — registration solving, segmentation, and element semantics stay the kernel and `Rasm.Bim/Exchange/reconstruct#LAS_INGEST` owners, and this page never fits, classifies, or projects. Points NEVER persist decoded: the raw blob is the system of record and every durable row is derived, so a re-derivation from those bytes reproduces the rows exactly and a stale row is a re-sweep, never an in-place rewrite. That blob admits as `Version/retention#RETENTION_CLASSES` `ArtifactKind.Scan` deriving `RetentionClass.Blob` — capture bytes are an observation no fold reproduces. Identity is the `ChunkManifest.WholeArtifact` `ContentAddress` the kernel `ContentHash` mints over the RAW bytes — the same value the Bim reconstruct lineage mints — so the two owners join on ONE identity with no shared carrier and no cross-package reference. `E57Data3D.Pose` is consumed INSIDE the E57 decode (`StreamPointsFull` already maps every position into the file-level frame), so it is never a `ScanRegistration` row and re-applying it double-transforms the cloud; `ScanRegistration` carries the separate into-project-frame transform an upstream survey or ICP solve admits. `→ Element/identity#ELEMENT_IDENTITY` (the cell mint, leg-1 downward), `← Element/codec#CONTENT_CHUNKING` (chunking and identity), `← Store/blobstore#MULTIPART_TRANSFER` (storage, through the port).

```csharp
using Rasm.Domain;
using Rasm.Persistence.Element;

namespace Rasm.Persistence.Ingest;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScanCapability : ICapability<ScanCapability> {
    public static readonly ScanCapability Color = new("color");
    public static readonly ScanCapability ExtendedClass = new("extended-class");
    public static readonly ScanCapability SpatialIndex = new("spatial-index");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ScanFormat {
    public static readonly ScanFormat E57 = new("e57");
    public static readonly ScanFormat Las = new("las");
    public static readonly ScanFormat Laz = new("laz");

    const int CompressionMarker = 104;
    const byte CompressedBit = 0x80;
    static ReadOnlySpan<byte> E57Magic => "ASTM-E57"u8;
    static ReadOnlySpan<byte> LasMagic => "LASF"u8;

    public static Option<ScanFormat> Sniff(ReadOnlySpan<byte> prefix) =>
        prefix.Length >= E57Magic.Length && prefix[..E57Magic.Length].SequenceEqual(E57Magic)
            ? Some(E57)
            : prefix.Length > CompressionMarker && prefix[..LasMagic.Length].SequenceEqual(LasMagic)
                ? Some((prefix[CompressionMarker] & CompressedBit) != 0 ? Laz : Las)
                : None;
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ScanStore(
    Func<ChunkManifest, ReadOnlySequence<byte>, IO<Unit>> Land,
    Func<ContentAddress, IO<Option<ReadOnlySequence<byte>>>> Fetch);

[ComplexValueObject]
public sealed partial class ScanSpec {
    public ScanFormat Format { get; }
    public Origin Origin { get; }
    public int RegionResolution { get; }
    public ScanStore Blob { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref ScanFormat format, ref Origin origin,
        ref int regionResolution, ref ScanStore blob) {
        if (origin is Origin.FromPath { Path: string path } && string.IsNullOrWhiteSpace(path)) {
            validationError = ValidationError.Create("<scan-spec-path>");
        } else if (regionResolution is < 0 or > 15) {
            validationError = ValidationError.Create("<scan-spec-resolution>");
        }
    }
}

public readonly record struct ScanBatch(
    int Count,
    ReadOnlyMemory<double> Positions,
    Option<ReadOnlyMemory<byte>> Classes,
    Option<ReadOnlyMemory<ushort>> Colors);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScanOp {
    private ScanOp() { }
    public sealed record Ingest(ScanSpec Spec, ReadOnlySequence<byte> Bytes) : ScanOp;
    public sealed record Probe(ScanSpec Spec, ReadOnlyMemory<byte> Header) : ScanOp;
    public sealed record Window(ScanSpec Spec, ContentAddress Scan, Seq<H3Cell> Cells) : ScanOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScanYield {
    private ScanYield() { }
    public sealed record Landed(ScanHeader Header, ContentAddress Scan, int Chunks, Seq<ScanRegion> Regions) : ScanYield;
    public sealed record Probed(ScanHeader Header) : ScanYield;
    public sealed record Points(Seq<ScanBatch> Batches) : ScanYield;
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScanFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Scan;
    private ScanFault() { }
    [FaultCase(0)]
    public sealed partial record CodecReject(string Detail) : ScanFault();
    [FaultCase(1)]
    public sealed partial record CrsUnsupported(string Detail) : ScanFault();
    [FaultCase(2)]
    public sealed partial record RegionUnresolvable(string Detail) : ScanFault();
    [FaultCase(3)]
    public sealed partial record WindowMiss(ContentAddress Scan) : ScanFault();


    public override string Message => Switch(
        codecReject:        static c => $"<scan-codec-reject:{c.Detail}>",
        crsUnsupported:     static c => $"<scan-crs-unsupported:{c.Detail}>",
        regionUnresolvable: static c => $"<scan-region-unresolvable:{c.Detail}>",
        windowMiss:         static c => $"<scan-window-miss:{c.Scan}>");
}

public sealed class ScanRefusal(ScanFault fault) : Exception(fault.Message) { public ScanFault Fault { get; } = fault; }

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class ScanSource {
    public static IO<Validation<Error, ScanYield>> Run(ScanOp op, ProjectionContext frame) =>
        op.Switch(
            frame,
            ingest: static (f, i) => Landed(i.Spec, i.Bytes, f),
            probe:  static (f, p) => Probed(p.Spec, p.Header, f),
            window: static (f, w) => Windowed(w.Spec, w.Scan, w.Cells));

    static IO<Validation<Error, ScanYield>> Landed(ScanSpec spec, ReadOnlySequence<byte> bytes, ProjectionContext frame) {
        ChunkManifest manifest = ContentChunker.Chunk(ChunkPolicy.Artifact, bytes);
        return from now in IO.lift(frame.Now)
               from swept in IO.lift(() => Capture(() => Sweep(spec, bytes, manifest.WholeArtifact, now)).Bind(static inner => inner))
               from held in swept.Match(
                   Succ: s => spec.Blob.Land(manifest, bytes).Map(_ => (Validation<Error, ScanYield>)
                       new ScanYield.Landed(s.Header, manifest.WholeArtifact, manifest.Chunks.Count, s.Regions)),
                   Fail: fault => IO.pure((Validation<Error, ScanYield>)fault))
               select held;
    }

    static Validation<Error, (ScanHeader Header, Seq<ScanRegion> Regions)> Sweep(ScanSpec spec, ReadOnlySequence<byte> bytes, ContentAddress scan, Instant now) {
        (long Points, Option<string> Wkt, Option<Instant> Captured, Option<string> Sensor) meta = Meta(spec, bytes);
        HashMap<H3Cell, (long Count, double ZMin, double ZMax)> regions = HashMap<H3Cell, (long, double, double)>();
        ScanExtent extent = ScanExtent.Empty;
        foreach (ScanBatch batch in Batches(spec, bytes, None)) {
            ReadOnlySpan<double> xyz = batch.Positions.Span;
            for (int at = 0; at < batch.Count; at++) {
                (double x, double y, double z) = (xyz[at * 3], xyz[(at * 3) + 1], xyz[(at * 3) + 2]);
                extent = extent.Extend(x, y, z);
                Fin<H3Cell> minted = IdentityStore.Cell(new Envelope(x, x, y, y), spec.RegionResolution);
                if (minted.IsFail) { return new ScanFault.RegionUnresolvable($"<cell:{x:R},{y:R}@{spec.RegionResolution}>"); }
                regions = minted.Match(
                    Succ: cell => regions.AddOrUpdate(cell,
                        Some: held => (held.Count + 1, Math.Min(held.ZMin, z), Math.Max(held.ZMax, z)),
                        None: () => (1L, z, z)),
                    Fail: _ => regions);
            }
        }
        return (
            new ScanHeader(scan, spec.Format, meta.Points, meta.Wkt, extent, meta.Captured, meta.Sensor, now),
            regions.AsIterable().ToSeq().Map(row => new ScanRegion(scan, row.Key, row.Value.Count, row.Value.ZMin, row.Value.ZMax)));
    }

    static IO<Validation<Error, ScanYield>> Probed(ScanSpec spec, ReadOnlyMemory<byte> header, ProjectionContext frame) =>
        from now in IO.lift(frame.Now)
        from read in IO.lift(() => Sniffed(spec, header.Span).Bind(_ => Capture(() => {
            ReadOnlySequence<byte> payload = Source(spec.Origin);
            (long Points, Option<string> Wkt, Option<Instant> Captured, Option<string> Sensor) meta = Meta(spec, payload);
            return (ScanYield)new ScanYield.Probed(
                new ScanHeader(ContentChunker.Chunk(ChunkPolicy.Artifact, payload).WholeArtifact, spec.Format,
                    meta.Points, meta.Wkt, ScanExtent.Empty, meta.Captured, meta.Sensor, now));
        })))
        select read;

    static IO<Validation<Error, ScanYield>> Windowed(ScanSpec spec, ContentAddress scan, Seq<H3Cell> cells) =>
        from fetched in spec.Blob.Fetch(scan)
        from read in IO.lift(() => fetched.Match(
            Some: payload => Capture(() => (ScanYield)new ScanYield.Points(toSeq(Batches(spec, payload, Some(Bounds(cells))))
                .Map(batch => Selected(batch, spec.RegionResolution, toHashSet(cells)))
                .Filter(static batch => batch.Count > 0))),
            None: () => (Validation<Error, ScanYield>)new ScanFault.WindowMiss(scan)))
        select read;

    static Envelope Bounds(Seq<H3Cell> cells) =>
        cells.Fold(new Envelope(), static (bbox, cell) => {
            bbox.ExpandToInclude(cell.Live.GetCellBoundary(null).EnvelopeInternal);
            return bbox;
        });

    static ScanBatch Selected(ScanBatch batch, int resolution, HashSet<H3Cell> cells) {
        ReadOnlySpan<double> xyz = batch.Positions.Span;
        List<int> kept = [];
        for (int at = 0; at < batch.Count; at++) {
            double x = xyz[at * 3];
            double y = xyz[(at * 3) + 1];
            if (IdentityStore.Cell(new Envelope(x, x, y, y), resolution).Match(Succ: cells.Contains, Fail: static _ => false)) { kept.Add(at); }
        }
        return Gathered(batch, kept);
    }

    static ScanBatch Gathered(ScanBatch batch, List<int> kept) {
        double[] xyz = new double[kept.Count * 3];
        byte[]? classes = batch.Classes.Match<byte[]?>(Some: _ => new byte[kept.Count], None: static () => null);
        ushort[]? colors = batch.Colors.Match<ushort[]?>(Some: _ => new ushort[kept.Count * 3], None: static () => null);
        for (int slot = 0; slot < kept.Count; slot++) {
            int at = kept[slot];
            batch.Positions.Span.Slice(at * 3, 3).CopyTo(xyz.AsSpan(slot * 3, 3));
            if (classes is not null) { classes[slot] = batch.Classes.Map(lane => lane.Span[at]).IfNone((byte)0); }
            if (colors is not null) { batch.Colors.Map(lane => lane.Slice(at * 3, 3)).IfNone(default).Span.CopyTo(colors.AsSpan(slot * 3, 3)); }
        }
        return new ScanBatch(kept.Count, xyz, Optional(classes).Map(static lane => (ReadOnlyMemory<byte>)lane), Optional(colors).Map(static lane => (ReadOnlyMemory<ushort>)lane));
    }

    static Validation<Error, Unit> Sniffed(ScanSpec spec, ReadOnlySpan<byte> prefix) =>
        ScanFormat.Sniff(prefix).Match(
            Some: found => found == spec.Format
                ? (Validation<Error, Unit>)unit
                : new ScanFault.CodecReject($"<sniff:{found.Key}!={spec.Format.Key}>"),
            None: static () => new ScanFault.CodecReject("<no-capture-magic>"));

    static (long Points, Option<string> Wkt, Option<Instant> Captured, Option<string> Sensor) Meta(ScanSpec spec, ReadOnlySequence<byte> payload) =>
        spec.Format.Switch(
            payload,
            e57: static p => E57Codec.Meta(p),
            las: static p => LasCodec.Meta(p),
            laz: static p => LasCodec.Meta(p));

    static IEnumerable<ScanBatch> Batches(ScanSpec spec, ReadOnlySequence<byte> payload, Option<Envelope> window) =>
        spec.Format.Switch(
            (payload, window),
            e57: static s => E57Codec.Batches(s.payload, s.window),
            las: static s => LasCodec.Batches(s.payload, s.window),
            laz: static s => LasCodec.Batches(s.payload, s.window));

    static ReadOnlySequence<byte> Source(Origin origin) => new(origin.Read(
        path: File.ReadAllBytes,
        stream: static s => { using MemoryStream buffered = new(); s.CopyTo(buffered); return buffered.ToArray(); }));

    internal static Stream Seekable(ReadOnlySequence<byte> payload) =>
        (payload.IsSingleSegment ? payload.First : new ReadOnlyMemory<byte>(payload.ToArray())).AsStream();

    internal static Validation<Error, TValue> Capture<TValue>(Func<TValue> codec) =>
        Op.Of().Catch(() => Fin.Succ(codec()))
            .MapFail(static e => e.Exception.Case is ScanRefusal refusal ? refusal.Fault : e)
            .ToValidation();
}

// --- [BOUNDARIES] ----------------------------------------------------------------------

static class E57Codec {
    const int ChunkPoints = 1 << 20;

    static readonly ImmutableHashSet<PointPropertySemantics> Unread = ImmutableHashSet.Create(
        PointPropertySemantics.RowIndex, PointPropertySemantics.ColumnIndex,
        PointPropertySemantics.ReturnCount, PointPropertySemantics.ReturnIndex,
        PointPropertySemantics.TimeStamp, PointPropertySemantics.Intensity,
        PointPropertySemantics.IsTimeStampInvalid, PointPropertySemantics.IsIntensityInvalid,
        PointPropertySemantics.IsColorInvalid, PointPropertySemantics.SphericalInvalidState,
        PointPropertySemantics.NormalX, PointPropertySemantics.NormalY, PointPropertySemantics.NormalZ,
        PointPropertySemantics.Reflectance, PointPropertySemantics.Amplitude);

    static Seq<ASTM_E57.E57Data3D> Setups(ASTM_E57.E57Root root) =>
        Optional(root.Data3D).Match(Some: toSeq, None: Seq<ASTM_E57.E57Data3D>);

    public static (long Points, Option<string> Wkt, Option<Instant> Captured, Option<string> Sensor) Meta(ReadOnlySequence<byte> payload) {
        using Stream source = ScanSource.Seekable(payload);
        ASTM_E57.E57Root root = ASTM_E57.E57FileHeader.Parse(source, payload.Length, verbose: false).E57Root;
        Seq<ASTM_E57.E57Data3D> setups = Setups(root);
        return (
            setups.Sum(static setup => setup.Points.RecordCount),
            Optional(root.CoordinateMetadata),
            setups.Choose(static setup => Optional(setup.AcquisitonStart)).Head.Map(static stamp => Instant.FromDateTimeOffset(stamp.DateTime)),
            setups.Choose(static setup => Optional(setup.SensorModel)).Head);
    }

    public static IEnumerable<ScanBatch> Batches(ReadOnlySequence<byte> payload, Option<Envelope> window) {
        using Stream source = ScanSource.Seekable(payload);
        ASTM_E57.E57FileHeader header = ASTM_E57.E57FileHeader.Parse(source, payload.Length, verbose: false);
        foreach (ASTM_E57.E57Data3D setup in Setups(header.E57Root)) {
            if (window.Map(bbox => !Touches(setup, bbox)).IfNone(false)) { continue; }
            foreach ((V3d[] positions, ImmutableDictionary<PointPropertySemantics, Array> channels) in setup.StreamPointsFull(ChunkPoints, verbose: false, Unread)) {
                yield return Batch(positions, channels);
            }
        }
    }

    static bool Touches(ASTM_E57.E57Data3D setup, Envelope window) =>
        setup.CartesianBounds is not { } bounds
        || window.Intersects(new Envelope(bounds.Bounds.Min.X, bounds.Bounds.Max.X, bounds.Bounds.Min.Y, bounds.Bounds.Max.Y));

    static ScanBatch Batch(V3d[] positions, ImmutableDictionary<PointPropertySemantics, Array> channels) {
        bool[] meaningful = Meaningful(channels, positions.Length);
        int count = meaningful.Count(static ok => ok);
        int[] classes = channels.TryGetValue(PointPropertySemantics.Classification, out Array? lane) && lane is int[] widened ? widened : [];
        byte[] red = Channel(channels, PointPropertySemantics.ColorRed);
        byte[] green = Channel(channels, PointPropertySemantics.ColorGreen);
        byte[] blue = Channel(channels, PointPropertySemantics.ColorBlue);
        double[] xyz = new double[count * 3];
        byte[]? held = classes.Length > 0 ? new byte[count] : null;
        ushort[]? colors = red.Length > 0 ? new ushort[count * 3] : null;
        int cursor = 0;
        for (int at = 0; at < positions.Length; at++) {
            if (!meaningful[at]) { continue; }
            (xyz[cursor * 3], xyz[(cursor * 3) + 1], xyz[(cursor * 3) + 2]) = (positions[at].X, positions[at].Y, positions[at].Z);
            if (held is not null) { held[cursor] = (byte)classes[at]; }
            if (colors is not null) { (colors[cursor * 3], colors[(cursor * 3) + 1], colors[(cursor * 3) + 2]) = (red[at], green[at], blue[at]); }
            cursor++;
        }
        return new ScanBatch(count, xyz, Optional(held).Map(static lane => (ReadOnlyMemory<byte>)lane), Optional(colors).Map(static lane => (ReadOnlyMemory<ushort>)lane));
    }

    static bool[] Meaningful(ImmutableDictionary<PointPropertySemantics, Array> channels, int count) =>
        channels.TryGetValue(PointPropertySemantics.CartesianInvalidState, out Array? lane)
            ? lane switch {
                byte[] states => [.. states.Select(static state => state == 0)],
                int[] widened => [.. widened.Select(static state => state == 0)],
                _ => [.. Enumerable.Repeat(true, count)],
            }
            : [.. Enumerable.Repeat(true, count)];

    static byte[] Channel(ImmutableDictionary<PointPropertySemantics, Array> channels, PointPropertySemantics semantic) =>
        channels.TryGetValue(semantic, out Array? lane) && lane is byte[] bytes ? bytes : [];
}

static class LasCodec {
    const int BatchPoints = 1 << 16;
    const ushort CrsRecord = 2112;
    const byte ExtendedFormat = 6;

    const LASZIP_DECOMPRESS_SELECTIVE Read =
        LASZIP_DECOMPRESS_SELECTIVE.CHANNEL_RETURNS_XY | LASZIP_DECOMPRESS_SELECTIVE.Z
        | LASZIP_DECOMPRESS_SELECTIVE.CLASSIFICATION | LASZIP_DECOMPRESS_SELECTIVE.RGB;

    public static (long Points, Option<string> Wkt, Option<Instant> Captured, Option<string> Sensor) Meta(ReadOnlySequence<byte> payload) {
        using Stream source = ScanSource.Seekable(payload);
        laszip codec = Opened(source);
        try {
            _ = Checked(codec, codec.get_number_of_point(out long points));
            return (points, Crs(codec.get_header_pointer()), None, None);
        } finally { _ = codec.close_reader(); }
    }

    public static IEnumerable<ScanBatch> Batches(ReadOnlySequence<byte> payload, Option<Envelope> window) {
        using Stream source = ScanSource.Seekable(payload);
        laszip codec = Opened(source);
        try {
            laszip_header header = codec.get_header_pointer();
            laszip_point point = codec.get_point_pointer();
            CapabilitySet<ScanCapability> reach = Reach(codec, header, window);
            double[] coordinates = new double[3];
            List<double> xyz = new(BatchPoints * 3);
            List<byte> classes = new(BatchPoints);
            List<ushort> colors = new(BatchPoints * 3);
            while (Next(codec, reach)) {
                _ = Checked(codec, codec.get_coordinates(coordinates));
                xyz.AddRange(coordinates);
                classes.Add(reach.Admits(ScanCapability.ExtendedClass) ? point.extended_classification : point.classification);
                if (reach.Admits(ScanCapability.Color)) { colors.AddRange([point.rgb[0], point.rgb[1], point.rgb[2]]); }
                if (classes.Count == BatchPoints) { yield return Batch(xyz, classes, colors, reach); }
            }
            if (classes.Count > 0) { yield return Batch(xyz, classes, colors, reach); }
        } finally { _ = codec.close_reader(); }
    }

    static CapabilitySet<ScanCapability> Reach(laszip codec, laszip_header header, Option<Envelope> window) =>
        CapabilitySet<ScanCapability>.Of([
            .. (Colored(header.point_data_format) ? Seq(ScanCapability.Color) : Seq<ScanCapability>()),
            .. (header.point_data_format >= ExtendedFormat ? Seq(ScanCapability.ExtendedClass) : Seq<ScanCapability>()),
            .. (Indexed(codec, window) ? Seq(ScanCapability.SpatialIndex) : Seq<ScanCapability>()),
        ]);

    static bool Indexed(laszip codec, Option<Envelope> window) =>
        window.IsSome
        && Checked(codec, codec.has_spatial_index(out bool present, out _)) is 0 && present
        && Window(codec, window);

    static bool Window(laszip codec, Option<Envelope> window) => window.Match(
        Some: bbox => Checked(codec, codec.inside_rectangle(bbox.MinX, bbox.MinY, bbox.MaxX, bbox.MaxY, out bool empty)) is 0
            && !empty
            && Checked(codec, codec.exploit_spatial_index(true)) is 0,
        None: static () => false);

    static bool Next(laszip codec, CapabilitySet<ScanCapability> reach) {
        if (reach.Admits(ScanCapability.SpatialIndex)) {
            _ = Checked(codec, codec.read_inside_point(out bool done));
            return !done;
        }
        _ = Checked(codec, codec.read_point());
        return true;
    }

    static ScanBatch Batch(List<double> xyz, List<byte> classes, List<ushort> colors, CapabilitySet<ScanCapability> reach) {
        ScanBatch batch = new(classes.Count, xyz.ToArray(), (ReadOnlyMemory<byte>)classes.ToArray(),
            reach.Admits(ScanCapability.Color) ? Some((ReadOnlyMemory<ushort>)colors.ToArray()) : None);
        xyz.Clear();
        classes.Clear();
        colors.Clear();
        return batch;
    }

    static laszip Opened(Stream source) {
        laszip codec = laszip.create();
        _ = Checked(codec, codec.decompress_selective(Read));
        _ = Checked(codec, codec.open_reader_stream(source, out _, leaveOpen: true));
        return codec;
    }

    static Option<string> Crs(laszip_header header) =>
        toSeq(header.vlrs).Find(static record => record.record_id == CrsRecord)
            .Map(static record => Encoding.UTF8.GetString(record.data).TrimEnd('\0'));

    static bool Colored(byte format) => format is 2 or 3 or 5 or 7 or 8 or 10;

    static int Checked(laszip codec, int status) => status is 0
        ? status
        : throw new ScanRefusal(new ScanFault.CodecReject(codec.get_error()));
}
```

| [INDEX] | [POLICY]           | [VALUE]                                             | [BINDING]                                                  |
| :-----: | :----------------- | :-------------------------------------------------- | :--------------------------------------------------------- |
|  [01]   | one scan owner     | `ScanSource.Run` over `ScanOp`                      | ingest/probe/window are cases of ONE dispatch              |
|  [02]   | format settlement  | `ScanFormat.Sniff` over the file magics             | `ASTM-E57`, `LASF` + offset-104 bit; no extension branch   |
|  [03]   | one LAS engine     | `open_reader_stream` reports `is_compressed`        | `.las` and `.laz` are one leg; no per-compression reader   |
|  [04]   | one point currency | `ScanBatch`, xyz-interleaved                        | 8-bit E57 colour widens INTO the 16-bit lane, never a fork |
|  [05]   | streaming fold     | chunk cut and cell fold in ONE pass                 | peak residency is one batch plus the manifest              |
|  [06]   | measured rows      | extent and regions from DECODED positions           | declared extrema can disagree with their own points        |
|  [07]   | channel masking    | `decompress_selective` / `StreamPointsFull` exclude | position, class, colour alone reach the unpacker           |
|  [08]   | window push-down   | `.lax` index else per-`Data3D` bounds               | codec-native access; a full-decode filter is the fallback  |
|  [09]   | storage port       | `ScanStore` two arrows                              | provider selection stays at the composition root           |
|  [10]   | scan identity      | `ChunkManifest.WholeArtifact` over raw bytes        | equals the Bim reconstruct lineage; two owners, one key    |
|  [11]   | decode reach       | `CapabilitySet<ScanCapability>` per opened source   | colour, extended class, index; never three loose flags     |
|  [12]   | fault band         | `[FaultCase]` ordinals on `Fault`                   | `8520`-`8523`; contiguous case-grain identity              |

## [03]-[SCAN_STORAGE]

- Owner: `ScanExtent` the measured axis-aligned bounding envelope with its `Empty` seed and `Extend` fold; `ScanHeader` the per-scan durable row; `ScanRegistration` the into-project-frame transform row; `ScanRegion` the per-cell occupancy row — four rows, every one DERIVED from the raw blob the `[02]` ingest landed.
- Cases: a scan with no admitted CRS carries `Wkt` absent rather than a fabricated default; a scan whose writer declared no acquisition stamp carries `Captured` absent, because the per-point timestamp epoch is a guess no durable row inherits; a region row exists exactly where a decoded point landed, so an empty cell is an absent row rather than a zero-count one.
- Entry: rows are VALUES the `[02]` ops yield — `ScanYield.Landed` hands the header and the region set, and a `ScanRegistration` arrives from the upstream solve that computed it; durable landing is the app composition root's through `Element/graph#STORE_HOOKS` for row storage and `Store/blobstore#MULTIPART_TRANSFER` for the capture bytes, the same row-shape law every Ingest sibling obeys.
- Auto: `ScanExtent.Extend` is the one fold both codecs feed, seeded at `Empty` whose bounds are inverted so the first point sets all six faces without a first-point special case. `ScanRegion.Cell` keys at the spec's `RegionResolution` and the `ZMin`/`ZMax` band is the vertical extent WITHIN that cell, so a two-storey capture answers a storey-banded window from one row set rather than a re-decode. `ScanRegistration.Transform` is 16 row-major doubles and `Frame` names the target frame, admitted verbatim — this page never solves, composes, or inverts it.
- Packages: covered by `[02]`.
- Growth: a new durable scan axis is one field on `ScanHeader`; a new occupancy statistic is one column on `ScanRegion`; a new frame target is one `ScanRegistration` row; zero new surface — a stored decoded point set, a per-format row family, a stored extent contradicting the derivable one, or a solved registration is the deleted form.
- Boundary: rows are DERIVED and the blob is authoritative, so a row set rebuilt from the same bytes is byte-identical and a schema widening re-sweeps rather than rewriting in place. `ScanRegistration` is admitted, never solved — the kernel `Rasm/Processing/register#REGISTRATION` cloud-ICP owner computes it and the app hands the result here — and the E57 `Data3D` pose is NOT that transform: the decode already applied it. Region cells are the `Element/identity#ELEMENT_IDENTITY` `bigint` vocabulary, so a `ScanRegion` joins the element identity tier's cell column and the in-database `h3-pg` index directly. Capture bytes never enter a relational row; the object plane is their durable home.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct ScanExtent(double MinX, double MinY, double MinZ, double MaxX, double MaxY, double MaxZ) {
    public static readonly ScanExtent Empty = new(
        double.PositiveInfinity, double.PositiveInfinity, double.PositiveInfinity,
        double.NegativeInfinity, double.NegativeInfinity, double.NegativeInfinity);

    public ScanExtent Extend(double x, double y, double z) => new(
        Math.Min(MinX, x), Math.Min(MinY, y), Math.Min(MinZ, z),
        Math.Max(MaxX, x), Math.Max(MaxY, y), Math.Max(MaxZ, z));
}

public readonly record struct ScanHeader(
    ContentAddress Scan,
    ScanFormat Format,
    long Points,
    Option<string> Wkt,
    ScanExtent Extent,
    Option<Instant> Captured,
    Option<string> Sensor,
    Instant At);

public readonly record struct ScanRegistration(ContentAddress Scan, ReadOnlyMemory<double> Transform, string Frame, Instant At);

public readonly record struct ScanRegion(ContentAddress Scan, H3Cell Cell, long Points, double ZMin, double ZMax);
```

| [INDEX] | [POLICY]          | [VALUE]                                     | [BINDING]                                                     |
| :-----: | :---------------- | :------------------------------------------ | :------------------------------------------------------------ |
|  [01]   | chunk policy      | `ChunkPolicy.Artifact`                      | 16/64/256 KiB window over the raw capture bytes               |
|  [02]   | region resolution | `ScanSpec.RegionResolution`, `0..15`        | spec VALUE, never a codec constant                            |
|  [03]   | window path       | `.lax` when `has_spatial_index` reports one | else the full stream filtered at the cell test                |
|  [04]   | retention         | `ArtifactKind.Scan` → `RetentionClass.Blob` | capture bytes are unreproducible; every row derives from them |
|  [05]   | registration      | admitted transform, never solved here       | the kernel registration owner computes it                     |
|  [06]   | vertical band     | `ZMin`/`ZMax` per cell                      | storey-selective windows read rows, not the blob              |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

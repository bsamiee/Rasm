# [PERSISTENCE_INGEST_GEOSPATIAL]

Rasm.Persistence ingests and emits geospatial features through ONE `GeoSource` owner over the NTS-IO codec family — the `[A.4]` Ingest growth row ("the next foreign-file codec into the record rail lands as a page HERE") made real: a `GeoFormat` `[SmartEnum<string>]` crosses the four wire projections (`GeoPackage` the GPB-header-plus-WKB blob over the already-admitted `Microsoft.Data.Sqlite` container, `GeoJson` the RFC-7946 feature text, `Wkb`/`Wkt` the core-NTS binary/text pair) and every format decodes into ONE interior currency — the NTS `Geometry` under ONE shared `GeometryFactory` — so a per-codec ad-hoc factory, a coordinate DTO fork, or a second geometry model is the deleted form. A `GeoSpec` fixes a read once — format, `Origin` source, the `CrsPolicy` admissible-SRID set, the ordinate cap, the H3 cell resolution, and an `Option<string>` layer selector — and the owner discriminates ingest, egress, and probe on the closed `GeoOp` `[Union]`, never a `ReadGpkg`/`ReadGeoJson`/`WriteWkb` name family.

Every ingested feature lands as ONE `GeoFeatureRow`: the decoded `Geometry`, its canonical WKB interchange bytes (`WKBWriter` with EWKB SRID embedding — the storage-codec-independent byte form), the content key `ContentAddress.Of(wkb.Span)` minted through the ONE kernel seed-zero entry (`Element/codec#CONTENT_ADDRESS` — a GeoPackage blob, a PostGIS column, and a GeoJSON text of the same geometry share one content key), the `H3Cell` bucket set derived at ingest (`H3Index.FromPoint` for points, `Geometry.Fill` polyfill for regions and lines at the spec resolution — bit-identical to the `h3-pg` `h3_latlng_to_cell` server-side mint, the `Element/identity#SPATIAL_CELL` cell-parity law), and the deferred `GeoProperties` carrier the app reifies TYPED through ONE `Bind<T>` — a GeoJSON feature's `properties` through `IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>` under the SAME `ElementJson.Options` graph that carries `GeoJsonProjection.Default.Factory` (`Element/codec#CODEC_AXIS` dual service — geometry and typed properties NEVER deserialize under two disjoint converter sets), a GeoPackage attribute-column bag through the same STJ wire round-trip `Ingest/tabular#TABULAR_SOURCE` binds cells with. The codec NEVER knows the element graph: rows project to `Rasm.Element` nodes at the app composition root (ARCH:61 mirrored), `← Rasm.Bim/Semantics/geospatial` consumes the feature ingress (BIM:81 sibling row). The three typed faults are REACHED, never decorative — `CrsUnsupported` from the GPB header `SrsId`, the EWKB SRID, and the container spine `srs_id` probed against `CrsPolicy`; `GeometryInvalid` from the strict-mode parse (`WKBReader.IsStrict`/`WKTReader.IsStrict`, `RepairRings` off — byte-identity and repair are mutually exclusive) plus the NTS `Geometry.IsValid` gate; `CodecReject` from every remaining codec throw — with `Code => FaultBand.GeoIngest + n` (band 8440) and facts riding `store.geo.*`. `Origin` arrives from `Ingest/tabular#TABULAR_SOURCE`; `ProjectionContext` from `Element/graph#STORE_RAIL` ([A.1] frame); `ContentAddress` from `Element/codec`; `H3Cell` from `Element/identity`; `FaultBand` from `Element/graph#FAULT_TABLES`.

## [01]-[INDEX]

- [01]-[GEO_SOURCE]: the four-format axis under one shared factory, the `GeoSpec` descriptor, the closed ingest/egress/probe op family, the CRS and validity gates, the H3 bucket derivation, and the typed fact stream.
- [02]-[FEATURE_ROWS]: the `GeoFeatureRow` currency — canonical WKB + content key + cell set + deferred properties — the one `Bind<T>` reify seam, and the GeoPackage container spine read.

## [02]-[GEO_SOURCE]

- Owner: `GeoFormat` the `[SmartEnum<string>]` wire-projection axis (`gpkg`/`geojson`/`wkb`/`wkt`), each row carrying its `CarriesProperties` column (GeoPackage and GeoJson deliver attributed features; Wkb/Wkt deliver bare geometry); `GeoAdmission` the ONE codec-policy record minting the shared `GeometryFactory` (precision + SRID off `GeoJsonProjection.Default.Geometry` — the codec-owned projection, never a second factory) and the four policy-frozen codec instances; `CrsPolicy` the admissible-SRID vocabulary (`Canonical` = WGS84-only, a projected deployment widens the set as DATA); `GeoSpec` the `[ComplexValueObject]` read descriptor; `GeoOp`/`GeoYield` the closed op/result families; `GeoIngestFault` the closed fault band; `GeoFact` the receipt record; `GeoSource` the static surface owning the one `Run` dispatch.
- Cases: `GeoOp.Ingest(GeoSpec)` decodes the source into `Seq<GeoFeatureRow>`; `GeoOp.Egress(GeoSpec, Seq<GeoPayload>)` writes features out at the spec's format — GeoJson as one `FeatureCollection` under `ElementJson.Options` (ring orientation `EnforceRfc9746` rides the codec factory, write-only by law, so a sign-reading kernel normalizes at admission), GeoPackage as GPB blobs through the policy-frozen writer, Wkb/Wkt through the core writers with the SRID embedded/`OutputOrdinates` capped; `GeoOp.Probe(GeoSpec)` yields the layer roster (GeoPackage spine rows, GeoJSON feature census, Wkb/Wkt geometry kind) without projecting rows; `GeoIngestFault` is `CodecReject | CrsUnsupported | GeometryInvalid` (8441-8443); `GeoFact` kinds are `ingest | egress | probe`.
- Entry: `public static IO<Validation<GeoIngestFault, GeoYield>> Run(GeoOp op, ProjectionContext frame, Func<GeoFact, IO<Unit>> sink)` — ONE polymorphic entry over the closed op union through the generated total `Switch`; the typed-properties reify is NOT a second entry — it is the `GeoFeatureRow.Bind<T>` member on the yielded row.
- Auto: the four codecs bind ONE factory — `GeoAdmission.Canonical` freezes `WKBReader { IsStrict = true, HandleSRID = true }`, `WKBWriter(ByteOrder.LittleEndian, handleSRID: true)` (EWKB SRID embedding so the canonical bytes carry their CRS), `WKTReader { IsStrict = true }`, `WKTWriter { OutputOrdinates = cap }`, `GeoPackageGeoReader { HandleSRID = true, RepairRings = false, HandleOrdinates = cap }`, `GeoPackageGeoWriter { HandleOrdinates = cap }` (the writer's ordinate policy derives body dimensionality AND header envelope kind, so the two cannot disagree) — `RepairRings` stays OFF because a repaired blob re-emits different bytes and byte-identity content keys and repair are mutually exclusive; the CRS gate runs BEFORE geometry admission — GeoPackage reads the GPB header (`GeoPackageBinaryHeader.Read` — magic `GP`, `SrsId`, `Ordinates`, `Extent`) and probes `SrsId` against the policy, EWKB probes the stamped SRID post-read, the container spine probes `gpkg_geometry_columns.srs_id` once per layer, GeoJSON is fixed WGS84 by format (no member to probe — an out-of-policy deployment refuses the FORMAT row, never a per-feature scan); the validity gate follows — a strict-parse throw or `!geometry.IsValid` rails `GeometryInvalid` with the NTS reason; the H3 derivation dispatches on the decoded shape — a `Point` through `H3Index.FromPoint(point, resolution)`, a polygonal or lineal shape through the `Fill` polyfill (the antimeridian probe `Geometry.IsTransMeridian` gates the fill, a crossing region filling per hemisphere-split half via NTS `Intersection` because a raw transmeridian polyfill walks the wrong lobe), `H3Index.Invalid` projecting to an EMPTY cell contribution never a stored zero cell — the cells persist as `H3Cell` `long`s (`H3Cell.Of`), uniform-resolution so the in-PG `h3-pg` equality join holds bit-for-bit (`CompactCells` is the region-key densifier a spatial-bucket STORE may apply downstream, never the durable row form).
- Receipt: every op rides a `GeoFact` under `store.geo.*` — an `ingest` fact carrying the format key, feature count, and derived-cell total; an `egress` fact carrying the format and feature count; a `probe` fact carrying the layer count — one kind-discriminated stream stamped `frame.Now()`.
- Packages: NetTopologySuite.IO.GeoPackage (`GeoPackageGeoReader`/`GeoPackageGeoWriter`/`GeoPackageBinaryHeader`), NetTopologySuite.IO.GeoJSON4STJ (`GeoJsonConverterFactory` via `GeoJsonProjection.Default.Factory`, `IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>`, `NetTopologySuite.Features.Feature`/`FeatureCollection`/`AttributesTable`), NetTopologySuite (`WKBReader`/`WKBWriter`/`WKTReader`/`WKTWriter`/`Geometry.IsValid`/`Geometry.Intersection`/`PrecisionModel`/`Ordinates`), pocketken.H3 (`H3Index.FromPoint`, `Geometry.Fill`, `LineString.Fill`, `Geometry.IsTransMeridian`, `H3Index.Invalid`, the `ulong` durable form), Microsoft.Data.Sqlite (the GeoPackage container spine read — already admitted), Rasm.Persistence (`Element/codec` `ContentAddress`/`ElementJson.Options`/`GeoJsonProjection`, `Element/identity` `H3Cell`, `Element/graph` `FaultBand`/`ProjectionContext`, `Ingest/tabular` `Origin`), LanguageExt.Core, Thinktecture.Runtime.Extensions, NodaTime, BCL inbox.
- Growth: a new wire projection is one `GeoFormat` row plus its codec arms in the format `Switch` (broken loudly at compile time); a new CRS stance is one `CrsPolicy` value (DATA, zero code); a new ordinate posture is the `GeoAdmission` cap; a new fault class is one case inside the registry decade; zero new surface — a per-codec `GeometryFactory`, a raw-WKB read of a GPB blob (the header is unparseable to a raw reader), a `RepairRings`-on row beside content addressing, a WKT `string.Split` parse, a hand-spelled GeoJSON shaper, a second H3 coordinate model, or a geo→element map inside this codec is the deleted form.
- Boundary: NTS `Geometry` is the SINGLE interior vocabulary and a store-to-feed flow is decode-blob → interior → encode-text, never a direct transcode; WKB is the canonical interchange binary — the content key hashes the WKB bytes, so identity is storage-codec-independent; the GeoJSON id convention rides the ONE `GeoJsonProjection` row (two partner id conventions would be two projection rows on two options instances, never post-read patching); precision is admission-side (the reader's `PrecisionModel` applies as coordinates parse; writers emit stored doubles raw), so emitted-text hash stability comes from constructing under the fixed factory BEFORE serialization; XYM/XYZM degrade silently on the GeoJSON text wire, so measure-bearing data routes through the blob projection — a format-capability fact on the `GeoFormat` row, not a runtime surprise; `→ Element/identity#SPATIAL_CELL` (cell derivation, leg-3→leg-1 downward), `← Element/codec#GeoJsonProjection` (converter graph), `→ Rasm.Element` (row shape only), `← Rasm.Bim/Semantics/geospatial` (feature ingress, BIM:81); the GDAL/OGR GeoParquet COLUMNAR lane is `Query/columnar`'s (BIM:81 first half) — this page owns feature-file codecs, never a columnar reader.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using Rasm.Persistence.Element;
using Expected = Rasm.Domain.Expected;

// --- [TYPES] ----------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeoFormat {
    public static readonly GeoFormat GeoPackage = new("gpkg", carriesProperties: true);
    public static readonly GeoFormat GeoJson = new("geojson", carriesProperties: true);
    public static readonly GeoFormat Wkb = new("wkb", carriesProperties: false);
    public static readonly GeoFormat Wkt = new("wkt", carriesProperties: false);
    public bool CarriesProperties { get; }
    private GeoFormat(string key, bool carriesProperties) : this(key) => CarriesProperties = carriesProperties;
}

// The admissible-SRID vocabulary: the CRS gate is a set-membership probe against DATA, so a projected
// deployment widens the set with zero gate edits; 4326 is the canonical interior frame.
public readonly record struct CrsPolicy(int Canonical, FrozenSet<int> Admissible) {
    public static readonly CrsPolicy Wgs84 = new(4326, FrozenSet.ToFrozenSet([4326]));
    public bool Admits(int srid) => srid == 0 || Admissible.Contains(srid);
}

// --- [MODELS] ---------------------------------------------------------------------------

// ONE factory, four policy-frozen codecs: precision + SRID come off the codec-owned `GeoJsonProjection`
// (`Element/codec#CODEC_AXIS`), so a GeoJSON→WKB→GeoPackage round trip keeps one precision grid and a
// per-codec ad-hoc `new GeometryFactory(...)` is the rejected form. `RepairRings` stays OFF: a repaired
// blob re-emits different bytes, and content-key byte identity and ring repair are mutually exclusive.
public sealed record GeoAdmission(GeometryFactory Factory, Ordinates Cap) {
    public static readonly GeoAdmission Canonical = new(GeoJsonProjection.Default.Geometry, Ordinates.XYZ);

    public WKBReader WkbIn => new() { IsStrict = true, HandleSRID = true, HandleOrdinates = Cap };
    public WKBWriter WkbOut => new(ByteOrder.LittleEndian, handleSRID: true);
    public WKTReader WktIn => new(Factory) { IsStrict = true };
    public WKTWriter WktOut => new() { OutputOrdinates = Cap };
    public GeoPackageGeoReader GpkgIn => new() { HandleSRID = true, RepairRings = false, HandleOrdinates = Cap };
    public GeoPackageGeoWriter GpkgOut => new() { HandleOrdinates = Cap };
}

[ComplexValueObject]
public sealed partial class GeoSpec {
    public GeoFormat Format { get; }
    public Origin Source { get; }
    public CrsPolicy Crs { get; }
    public int CellResolution { get; }
    public Option<string> Layer { get; }

    static Validation<ValidationError, GeoSpec> Validate(GeoFormat format, Origin source, CrsPolicy crs, int cellResolution, Option<string> layer) =>
        cellResolution is >= 0 and <= 15
            ? Validation<ValidationError, GeoSpec>.Success(new GeoSpec(format, source, crs, cellResolution, layer))
            : Validation<ValidationError, GeoSpec>.Fail(ValidationError.Create("<geo-spec-resolution>"));
}

// The deferred-properties carrier: a GeoJSON feature keeps its element-backed attribute table (reified
// typed ONLY through the one options graph), a GeoPackage feature keeps its column bag, a bare geometry
// carries nothing — one closed family, one `Bind<T>` dispatch, never a loose `IAttributesTable` walk.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeoProperties {
    private GeoProperties() { }
    public sealed record Deferred(IPartiallyDeserializedAttributesTable Table) : GeoProperties;
    public sealed record Columns(HashMap<string, object?> Bag) : GeoProperties;
    public sealed record Bare : GeoProperties;
}

public readonly record struct GeoFeatureRow(Geometry Shape, ReadOnlyMemory<byte> Wkb, ContentAddress Content, Seq<H3Cell> Cells, GeoProperties Properties);

public readonly record struct GeoPayload(Geometry Shape, HashMap<string, object?> Properties);

public readonly record struct GeoLayer(string Name, int Srid, string Kind, long Features);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeoOp {
    private GeoOp() { }
    public sealed record Ingest(GeoSpec Spec) : GeoOp;
    public sealed record Egress(GeoSpec Spec, Seq<GeoPayload> Features) : GeoOp;
    public sealed record Probe(GeoSpec Spec) : GeoOp;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeoYield {
    private GeoYield() { }
    public sealed record Features(Seq<GeoFeatureRow> Rows) : GeoYield;
    public sealed record Written(int Count) : GeoYield;
    public sealed record Roster(Seq<GeoLayer> Layers) : GeoYield;
}

// --- [ERRORS] ---------------------------------------------------------------------------
// Band 8440 (`FaultBand.GeoIngest`): every case is REACHED — CrsUnsupported from the GPB `SrsId`/EWKB
// SRID/spine `srs_id` probes, GeometryInvalid from strict parse + `Geometry.IsValid`, CodecReject from
// the remaining codec throws — never a decorative roster.
[Union]
public abstract partial record GeoIngestFault : Expected, IValidationError<GeoIngestFault> {
    private GeoIngestFault() : base() { }
    public sealed record CodecReject(string Format, string Detail) : GeoIngestFault;
    public sealed record CrsUnsupported(int Srid) : GeoIngestFault;
    public sealed record GeometryInvalid(string Reason) : GeoIngestFault;

    public override int Code => FaultBand.GeoIngest + Switch(
        codecReject:     static _ => 1,
        crsUnsupported:  static _ => 2,
        geometryInvalid: static _ => 3);

    public override string Message => Switch(
        codecReject:     static c => $"<geo-codec-reject:{c.Format}:{c.Detail}>",
        crsUnsupported:  static c => $"<geo-crs-unsupported:{c.Srid}>",
        geometryInvalid: static c => $"<geo-geometry-invalid:{c.Reason}>");

    public override string Category => Switch(
        codecReject:     static _ => "Codec",
        crsUnsupported:  static _ => "Crs",
        geometryInvalid: static _ => "Geometry");

    public static GeoIngestFault Create(string message) => new CodecReject("wire", message);
    public static GeoIngestFault Lift(GeoFormat format, Exception boundary) => boundary switch {
        JsonException wire => new CodecReject(format.Key, $"{wire.Path}:{wire.Message}"),
        ArgumentException typeLiteral => new CodecReject(format.Key, typeLiteral.Message),
        ParseException malformed => new GeometryInvalid(malformed.Message),
        _ => new CodecReject(format.Key, boundary.Message),
    };
}

public readonly record struct GeoFact(string Kind, string Format, long Features, long Cells, Instant At);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class GeoSource {
    public static IO<Validation<GeoIngestFault, GeoYield>> Run(GeoOp op, ProjectionContext frame, Func<GeoFact, IO<Unit>> sink) =>
        op.Switch(
            state: (frame, sink),
            ingest: static (s, i) => Ingested(i.Spec, s.frame, s.sink),
            egress: static (s, e) => Emitted(e.Spec, e.Features, s.frame, s.sink),
            probe:  static (s, p) => Probed(p.Spec, s.frame, s.sink));

    static IO<Validation<GeoIngestFault, GeoYield>> Ingested(GeoSpec spec, ProjectionContext frame, Func<GeoFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Capture(spec.Format, () => spec.Format.Switch(
                state: spec,
                geoPackage: static s => GeoContainer.Features(s, GeoAdmission.Canonical),
                geoJson:    static s => GeoWire.Features(s),
                wkb:        static s => Seq(GeoAdmission.Canonical.WkbIn.Read(Bytes(s.Source))),
                wkt:        static s => Seq(GeoAdmission.Canonical.WktIn.Read(Text(s.Source)))
                    .Map(static g => (g, (GeoProperties)new GeoProperties.Bare()))))
            .Bind(features => features.Traverse(f => Row(spec, f)).As()))
        from _ in rows.Match(
            Succ: y => sink(new GeoFact("ingest", spec.Format.Key, y.Count, y.Sum(static r => (long)r.Cells.Count), frame.Now())),
            Fail: _ => IO.pure(unit))
        select rows.Map(static y => (GeoYield)new GeoYield.Features(y));

    // ONE row mint for every format: CRS gate → strict validity gate → canonical WKB → content key →
    // H3 bucket set → deferred properties. The gates run in cheapest-first order so rejection cost is
    // proportional to how wrong the feature is.
    static Validation<GeoIngestFault, GeoFeatureRow> Row(GeoSpec spec, (Geometry Shape, GeoProperties Properties) feature) =>
        !spec.Crs.Admits(feature.Shape.SRID)
            ? Validation<GeoIngestFault, GeoFeatureRow>.Fail(new GeoIngestFault.CrsUnsupported(feature.Shape.SRID))
            : !feature.Shape.IsValid
                ? Validation<GeoIngestFault, GeoFeatureRow>.Fail(new GeoIngestFault.GeometryInvalid(feature.Shape.GeometryType))
                : Validation<GeoIngestFault, GeoFeatureRow>.Success(Minted(spec, feature.Shape, feature.Properties));

    static GeoFeatureRow Minted(GeoSpec spec, Geometry shape, GeoProperties properties) {
        byte[] wkb = GeoAdmission.Canonical.WkbOut.Write(shape);
        return new GeoFeatureRow(shape, wkb, ContentAddress.Of(wkb.AsSpan()), Cells(shape, spec.CellResolution), properties);
    }

    // The SPATIAL_CELL derivation: a point mints one cell, a region/line polyfills — bit-identical to the
    // `h3-pg` server-side mint (the cell-parity law), `H3Index.Invalid` contributing NOTHING (never a stored
    // zero cell), and a transmeridian region filling per hemisphere-split half because a raw fill walks the
    // wrong lobe across the antimeridian.
    static Seq<H3Cell> Cells(Geometry shape, int resolution) => shape switch {
        Point point => new H3Index(H3Index.FromPoint(point, resolution)) is var cell && cell != H3Index.Invalid
            ? Seq(H3Cell.Of(cell)) : Seq<H3Cell>(),
        LineString line => toSeq(line.Fill(resolution)).Filter(static c => c != H3Index.Invalid).Map(H3Cell.Of),
        _ when shape.IsTransMeridian() => toSeq(Hemispheres(shape).SelectMany(half => half.Fill(resolution)))
            .Filter(static c => c != H3Index.Invalid).Distinct().Map(H3Cell.Of),
        _ => toSeq(shape.Fill(resolution)).Filter(static c => c != H3Index.Invalid).Map(H3Cell.Of),
    };

    static IEnumerable<Geometry> Hemispheres(Geometry shape) {
        GeometryFactory factory = GeoAdmission.Canonical.Factory;
        yield return shape.Intersection(factory.ToGeometry(new Envelope(-180d, 0d, -90d, 90d)));
        yield return shape.Intersection(factory.ToGeometry(new Envelope(0d, 180d, -90d, 90d)));
    }

    static IO<Validation<GeoIngestFault, GeoYield>> Emitted(GeoSpec spec, Seq<GeoPayload> features, ProjectionContext frame, Func<GeoFact, IO<Unit>> sink) =>
        from done in IO.lift(() => Capture(spec.Format, () => spec.Format.Switch(
            state: (spec, features),
            geoPackage: static s => GeoContainer.Write(s.spec, s.features, GeoAdmission.Canonical),
            geoJson:    static s => GeoWire.Write(s.spec, s.features),
            wkb:        static s => Binary(s.spec, s.features, static g => GeoAdmission.Canonical.WkbOut.Write(g)),
            wkt:        static s => Binary(s.spec, s.features, static g => Encoding.UTF8.GetBytes(GeoAdmission.Canonical.WktOut.Write(g))))))
        from _ in done.Match(Succ: _ => sink(new GeoFact("egress", spec.Format.Key, features.Count, 0L, frame.Now())), Fail: _ => IO.pure(unit))
        select done.Map(_ => (GeoYield)new GeoYield.Written(features.Count));

    static IO<Validation<GeoIngestFault, GeoYield>> Probed(GeoSpec spec, ProjectionContext frame, Func<GeoFact, IO<Unit>> sink) =>
        from roster in IO.lift(() => Capture(spec.Format, () => spec.Format.Switch(
            state: spec,
            geoPackage: static s => GeoContainer.Spine(s),
            geoJson:    static s => GeoWire.Census(s),
            wkb:        static s => Seq(new GeoLayer("wkb", GeoAdmission.Canonical.WkbIn.Read(Bytes(s.Source)).SRID, "geometry", 1L)),
            wkt:        static s => Seq(new GeoLayer("wkt", 0, GeoAdmission.Canonical.WktIn.Read(Text(s.Source)).GeometryType, 1L)))))
        from _ in roster.Match(Succ: y => sink(new GeoFact("probe", spec.Format.Key, y.Count, 0L, frame.Now())), Fail: _ => IO.pure(unit))
        select roster.Map(static y => (GeoYield)new GeoYield.Roster(y));

    static Unit Binary(GeoSpec spec, Seq<GeoPayload> features, Func<Geometry, byte[]> encode) =>
        spec.Source.Read(
            path:   p => { File.WriteAllBytes(p, features.Map(f => encode(f.Shape)).Fold(Array.Empty<byte>(), static (a, b) => [.. a, .. b])); return unit; },
            stream: s => { features.Iter(f => s.Write(encode(f.Shape))); return unit; });

    static byte[] Bytes(Origin source) => source.Read(path: File.ReadAllBytes, stream: static s => { using MemoryStream buffered = new(); s.CopyTo(buffered); return buffered.ToArray(); });
    static string Text(Origin source) => source.Read(path: File.ReadAllText, stream: static s => new StreamReader(s).ReadToEnd());

    internal static Validation<GeoIngestFault, TValue> Capture<TValue>(GeoFormat format, Func<TValue> codec) =>
        Try.lift(codec).Run().Match(
            Succ: Validation<GeoIngestFault, TValue>.Success,
            Fail: e => e.ToException() is var ex && ex is GeoRefusal refusal
                ? Validation<GeoIngestFault, TValue>.Fail(refusal.Fault)
                : Validation<GeoIngestFault, TValue>.Fail(GeoIngestFault.Lift(format, ex)));
}

// The typed in-codec refusal carrier: a spine/header gate deep inside a codec fold surfaces its ALREADY-typed
// fault through the one Capture funnel instead of flattening to a CodecReject message.
public sealed class GeoRefusal(GeoIngestFault fault) : Exception(fault.Message) { public GeoIngestFault Fault { get; } = fault; }
```

| [INDEX] | [POLICY]            | [VALUE]                                          | [BINDING]                                                          |
| :-----: | :------------------ | :----------------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | one geo owner       | `GeoSource.Run` over `GeoOp`                     | ingest/egress/probe are cases of ONE dispatch                      |
|  [02]   | one factory         | `GeoAdmission.Canonical` off `GeoJsonProjection` | four codecs, one precision grid; per-codec factory deleted         |
|  [03]   | interior vocabulary | NTS `Geometry` only                              | decode → interior → encode; no direct transcode, no DTO fork       |
|  [04]   | canonical bytes     | EWKB via `WKBWriter(handleSRID: true)`           | content key = `ContentAddress.Of(wkb)`; codec-independent identity |
|  [05]   | CRS gate            | `CrsPolicy` set membership, cheapest-first       | GPB `SrsId`/EWKB SRID/spine `srs_id`; GeoJSON fixed WGS84          |
|  [06]   | validity gate       | strict parse + `Geometry.IsValid`                | `RepairRings` off — byte identity and repair are exclusive         |
|  [07]   | H3 buckets          | `FromPoint`/`Fill` at spec resolution            | `h3-pg` bit parity; `Invalid` contributes nothing                  |
|  [08]   | fault band          | `Code => FaultBand.GeoIngest + n`                | 8441-8443 off the `graph#FAULT_TABLES` registry                    |
|  [09]   | receipt             | one `GeoFact` stream `store.geo.*`               | kind-discriminated; never parallel records                         |
|  [10]   | element projection  | per-app geo→element map                          | row shape only (ARCH:61 mirrored); BIM:81 the feature consumer     |

## [03]-[FEATURE_ROWS]

- Owner: `GeoFeatureRow` the one feature currency (`Shape` + canonical `Wkb` + `Content` key + `Cells` + `Properties`); `GeoProperties` the closed deferred-properties family with its one `Bind<T>` reify; `GeoWire` the GeoJSON text seam over `ElementJson.Options`; `GeoContainer` the GeoPackage container seam over the admitted `Microsoft.Data.Sqlite` — the three-table metadata spine (`gpkg_contents`/`gpkg_geometry_columns`) binding each feature table to exactly one geometry column and SRID.
- Cases: `GeoProperties.Deferred` holds the GeoJSON element-backed table — reified typed ONLY through `TryDeserializeJsonObject<T>(ElementJson.Options, out …)` so a feature's geometry and its typed properties resolve under ONE converter graph (a false return is absence, never a throw); `GeoProperties.Columns` holds the GeoPackage attribute-column bag — bound through the same STJ wire round-trip tabular cells mint through; `GeoProperties.Bare` is the Wkb/Wkt geometry-only row.
- Entry: `public Option<T> Bind<T>(this GeoFeatureRow row)` — ONE reify member dispatching the properties union; walking the loose `IAttributesTable` in domain code is the rejected form (the shard law: properties stay element-backed until projected).
- Auto: the container read gates cheapest-first — spine presence (`gpkg_geometry_columns` join `gpkg_contents WHERE data_type = 'features'`), per-layer `srs_id` against `CrsPolicy` (railed through the typed `GeoRefusal` so the deep gate surfaces `CrsUnsupported`, never a flattened message), then per-row GPB blob decode through the policy-frozen reader with every non-geometry column landing in the row bag; the layer selector (`spec.Layer`) narrows the spine sweep to one feature table, `None` reading every layer; the egress writes GPB blobs through the policy-frozen writer — the feature rows, the `rtree_{layer}_geom` maintenance, and the `gpkg_contents` extent expansion ride ONE transaction per layer write onto an already-registered layer (embedded-store law: a stale denormalized extent misleads every discovery consumer; the extent only ever EXPANDS at write, a shrink is a re-registration concern).
- Receipt: rides `[02]`'s facts — the container read contributes its per-layer feature counts to the one `ingest` fact.
- Packages: covered by `[02]`.
- Growth: a new properties source is one `GeoProperties` case plus one `Bind<T>` arm (compile-broken); a new spine gate is one probe row in the cheapest-first ladder; zero new surface — a second reify path beside `Bind<T>`, a per-format row type, or a raw-WKB read of a GPB column is the deleted form.
- Boundary: the GPB header, never the WKB body, is the single SRID authority (the reader stamps it onto the decoded geometry — `HandleSRID = true`); NaN-coded empty points remap by flag at read so a consumer never sees NaN coordinates; the container's embedded-store mechanics (WAL sidecars, STRICT tables, open ritual) are `Store/provisioning#EMBEDDED_FLOOR` law composed as settled material — this page opens a plain read-only `SqliteConnection` over the `.gpkg` artifact, never a second embedded engine.

```csharp signature
// --- [OPERATIONS] -----------------------------------------------------------------------

public static class GeoRows {
    // ONE reify seam for every properties source: Deferred re-runs the SAME options graph the geometry
    // decoded under; Columns round-trips the bag through the one STJ wire (the tabular `Bind<T>` law);
    // Bare is absence. A `false` TryDeserializeJsonObject is absence, never a throw.
    extension(GeoFeatureRow row) {
        public Option<T> Bind<T>() => row.Properties.Switch(
            deferred: d => d.Table.TryDeserializeJsonObject<T>(ElementJson.Options, out T? typed) ? Optional(typed) : None,
            columns:  c => Optional(JsonSerializer.Deserialize<T>(
                JsonSerializer.SerializeToUtf8Bytes(c.Bag.ToDictionary(), ElementJson.Options), ElementJson.Options)),
            bare:     static _ => Option<T>.None);
    }
}

// The GeoJSON text seam: ONE options graph (`ElementJson.Options` carrying `GeoJsonProjection.Default.Factory`)
// for the whole conversion family — geometry, features, collections, attribute tables. JSON null is null
// geometry (the rail's one null) projected to absence at the seam; ring orientation enforces on WRITE only.
public static class GeoWire {
    public static Seq<(Geometry Shape, GeoProperties Properties)> Features(GeoSpec spec) {
        FeatureCollection collection = spec.Source.Read(
            path:   static p => JsonSerializer.Deserialize<FeatureCollection>(File.ReadAllBytes(p), ElementJson.Options),
            stream: static s => JsonSerializer.Deserialize<FeatureCollection>(s, ElementJson.Options)) ?? [];
        return toSeq(collection).Map(feature => (
            feature.Geometry,
            feature.Attributes is IPartiallyDeserializedAttributesTable table
                ? (GeoProperties)new GeoProperties.Deferred(table)
                : new GeoProperties.Bare()));
    }

    public static Unit Write(GeoSpec spec, Seq<GeoPayload> features) {
        FeatureCollection collection = [];
        features.Iter(f => collection.Add(new Feature(f.Shape, new AttributesTable(f.Properties.ToDictionary()))));
        return spec.Source.Read(
            path:   p => { File.WriteAllBytes(p, JsonSerializer.SerializeToUtf8Bytes(collection, ElementJson.Options)); return unit; },
            stream: s => { JsonSerializer.Serialize(s, collection, ElementJson.Options); return unit; });
    }

    public static Seq<GeoLayer> Census(GeoSpec spec) =>
        Seq(new GeoLayer("features", 4326, nameof(FeatureCollection), Features(spec).Count));
}

// The GeoPackage container seam over the admitted Microsoft.Data.Sqlite: spine-first (the three-table
// metadata spine binds each feature table to ONE geometry column + SRID), per-layer CRS gate through the
// typed GeoRefusal, per-row GPB decode + column bag. Read-only mount — the container's own store mechanics
// are the embedded-floor owner's, never re-derived here.
public static class GeoContainer {
    public static Seq<(Geometry Shape, GeoProperties Properties)> Features(GeoSpec spec, GeoAdmission admission) =>
        spec.Source.Read(path: p => Read(p, spec, admission), stream: _ => throw new GeoRefusal(new GeoIngestFault.CodecReject("gpkg", "<container-needs-a-path>")));

    static Seq<(Geometry, GeoProperties)> Read(string path, GeoSpec spec, GeoAdmission admission) {
        using SqliteConnection container = new($"Data Source={path};Mode=ReadOnly");
        container.Open();
        List<(Geometry, GeoProperties)> rows = [];
        foreach (GeoLayer layer in Layers(container, spec.Layer)) {                     // Exemption: ADO container read fills a seam-local list frozen once on return
            if (!spec.Crs.Admits(layer.Srid)) { throw new GeoRefusal(new GeoIngestFault.CrsUnsupported(layer.Srid)); }
            using SqliteCommand features = container.CreateCommand();
            features.CommandText = $"SELECT * FROM \"{layer.Name}\"";
            using SqliteDataReader reader = features.ExecuteReader();
            int geometryAt = reader.GetOrdinal(layer.Kind);
            while (reader.Read()) {
                Geometry shape = admission.GpkgIn.Read((byte[])reader.GetValue(geometryAt));
                HashMap<string, object?> bag = toHashMap(Enumerable.Range(0, reader.FieldCount)
                    .Filter(i => i != geometryAt)
                    .Map(i => (reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i))));
                rows.Add((shape, new GeoProperties.Columns(bag)));
            }
        }
        return toSeq(rows);
    }

    public static Seq<GeoLayer> Spine(GeoSpec spec) =>
        spec.Source.Read(path: p => {
            using SqliteConnection container = new($"Data Source={p};Mode=ReadOnly");
            container.Open();
            return Layers(container, spec.Layer);
        }, stream: _ => throw new GeoRefusal(new GeoIngestFault.CodecReject("gpkg", "<container-needs-a-path>")));

    // The metadata spine: `gpkg_geometry_columns` names the ONE geometry column per feature table,
    // `gpkg_contents` the layer roster — `Kind` carries the geometry COLUMN name the row decode reads.
    static Seq<GeoLayer> Layers(SqliteConnection container, Option<string> selected) {
        using SqliteCommand spine = container.CreateCommand();
        spine.CommandText = """
            SELECT c.table_name, g.srs_id, g.column_name
            FROM gpkg_contents c JOIN gpkg_geometry_columns g ON g.table_name = c.table_name
            WHERE c.data_type = 'features'
            """;
        using SqliteDataReader reader = spine.ExecuteReader();
        List<GeoLayer> layers = [];
        while (reader.Read()) { layers.Add(new GeoLayer(reader.GetString(0), reader.GetInt32(1), reader.GetString(2), 0L)); }   // Exemption: ADO spine read, frozen once on return
        return selected.Match(Some: name => toSeq(layers).Filter(l => l.Name == name), None: () => toSeq(layers));
    }

    // A layer write is ONE transaction over feature rows + R-tree rows + contents extent — a stale extent
    // misleads every discovery consumer, so the write maintains ALL THREE in the same commit: each insert
    // RETURNs its rowid and mirrors into `rtree_{layer}_geom` (idempotent beside the spec trigger set — an
    // OR-REPLACE on the same id), and the `gpkg_contents` envelope EXPANDS with the written union, never shrinks.
    public static Unit Write(GeoSpec spec, Seq<GeoPayload> features, GeoAdmission admission) =>
        spec.Source.Read(path: p => {
            using SqliteConnection container = new($"Data Source={p};Mode=ReadWriteCreate");
            container.Open();
            using SqliteTransaction commit = container.BeginTransaction();
            string layer = spec.Layer.IfNone("features");
            Envelope written = new();
            features.Iter(f => {
                using SqliteCommand insert = container.CreateCommand();
                insert.Transaction = commit;
                insert.CommandText = $"INSERT INTO \"{layer}\" (geom) VALUES ($blob) RETURNING rowid";
                _ = insert.Parameters.AddWithValue("$blob", admission.GpkgOut.Write(f.Shape));
                long rowid = (long)insert.ExecuteScalar()!;
                Envelope bound = f.Shape.EnvelopeInternal;
                written.ExpandToInclude(bound);
                using SqliteCommand index = container.CreateCommand();
                index.Transaction = commit;
                index.CommandText = $"INSERT OR REPLACE INTO \"rtree_{layer}_geom\" (id, minx, maxx, miny, maxy) VALUES ($id, $minx, $maxx, $miny, $maxy)";
                _ = index.Parameters.AddWithValue("$id", rowid);
                _ = index.Parameters.AddWithValue("$minx", bound.MinX);
                _ = index.Parameters.AddWithValue("$maxx", bound.MaxX);
                _ = index.Parameters.AddWithValue("$miny", bound.MinY);
                _ = index.Parameters.AddWithValue("$maxy", bound.MaxY);
                _ = index.ExecuteNonQuery();
            });
            if (!written.IsNull) {                                                                       // an empty payload never shrinks or nulls the stored extent
                using SqliteCommand extent = container.CreateCommand();
                extent.Transaction = commit;
                extent.CommandText = """
                    UPDATE gpkg_contents SET
                        min_x = MIN(COALESCE(min_x, $minx), $minx), min_y = MIN(COALESCE(min_y, $miny), $miny),
                        max_x = MAX(COALESCE(max_x, $maxx), $maxx), max_y = MAX(COALESCE(max_y, $maxy), $maxy),
                        last_change = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')
                    WHERE table_name = $layer
                    """;
                _ = extent.Parameters.AddWithValue("$minx", written.MinX);
                _ = extent.Parameters.AddWithValue("$miny", written.MinY);
                _ = extent.Parameters.AddWithValue("$maxx", written.MaxX);
                _ = extent.Parameters.AddWithValue("$maxy", written.MaxY);
                _ = extent.Parameters.AddWithValue("$layer", layer);
                _ = extent.ExecuteNonQuery();
            }
            commit.Commit();
            return unit;
        }, stream: _ => throw new GeoRefusal(new GeoIngestFault.CodecReject("gpkg", "<container-needs-a-path>")));
}
```

| [INDEX] | [POLICY]            | [VALUE]                                   | [BINDING]                                                          |
| :-----: | :------------------ | :---------------------------------------- | :----------------------------------------------------------------- |
|  [01]   | deferred properties | `GeoProperties` union + one `Bind<T>`     | element-backed until projected; loose table walks rejected         |
|  [02]   | one converter graph | `ElementJson.Options`                     | geometry and typed properties never split converter sets           |
|  [03]   | spine authority     | `gpkg_geometry_columns` + `gpkg_contents` | one geometry column + SRID per layer; header is the SRID authority |
|  [04]   | deep-gate faults    | `GeoRefusal` typed carrier                | a spine CRS refusal surfaces typed, never a flattened message      |
|  [05]   | container mechanics | read-only `Microsoft.Data.Sqlite` mount   | embedded-floor law composed, never a second engine                 |
|  [06]   | layer write         | one transaction: rows + rtree + extent    | a stale extent misleads discovery; maintained in the same commit   |

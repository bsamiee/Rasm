# [PERSISTENCE_INGEST_GEOSPATIAL]

Rasm.Persistence ingests and emits geospatial features through ONE `GeoSource` owner over the NTS-IO codec family — the `[A.4]` Ingest growth row ("the next foreign-file codec into the record rail lands as a page HERE") made real: a `GeoFormat` `[SmartEnum<string>]` crosses the four wire projections (`GeoPackage` the GPB-header-plus-WKB blob over the already-admitted `Microsoft.Data.Sqlite` container, `GeoJson` the RFC-7946 feature text, `Wkb`/`Wkt` the core-NTS binary/text pair), each row carrying its `CarriesProperties` and `CarriesMeasures` capability columns (XYM/XYZM degrade silently on the GeoJSON text wire, so measure-bearing data routes by the ROW, not a runtime surprise), and every format decodes into ONE interior currency — the NTS `Geometry` under ONE shared `GeometryFactory` — so a per-codec ad-hoc factory, a coordinate DTO fork, or a second geometry model is the deleted form. A `GeoSpec` fixes a read once — format, `Origin` source, the `CrsPolicy` admissible-SRID set, the `GeoAdmission` factory-and-ordinate posture, the H3 cell resolution, and an `Option<string>` layer selector — and the owner discriminates ingest, egress, and probe on the closed `GeoOp` `[Union]`, never a `ReadGpkg`/`ReadGeoJson`/`WriteWkb` name family.

Every ingested feature lands as ONE `GeoFeatureRow`: decoded `Geometry`, canonical EWKB, `ContentAddress`, derived `H3Cell` buckets, and deferred `GeoProperties`. `GeoRows.Bind<T>` reifies GeoJSON properties through `IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>` and GeoPackage columns through the one `GeoWire.Options` wire — the SAME `GeoJsonProjection.Default.Factory` geometry row plus the Thinktecture/NodaTime converter families over an OPEN resolver, because the `ElementJson` source-gen resolver resolves only registered graph types and a consumer POCO is not one — returning `Validation<GeoIngestFault, Option<T>>` so malformed properties never escape the rail. Rows project to `Rasm.Element` only at the app composition root. `GeoIngestFault` reaches codec, CRS admission, CRS mismatch, geometry validity, capability-loss, and missing-layer cases; its `Semigroup` aggregates per-feature refusals. Codes occupy the `FaultBand.GeoIngest` decade, and facts ride `store.geo.*`. `Origin` arrives from `Ingest/tabular#TABULAR_SOURCE`; `ProjectionContext` from `Element/graph#STORE_RAIL`; `ContentAddress` from `Element/codec`; `H3Cell` from `Element/identity`; `FaultBand` from `Element/graph#FAULT_TABLES`.

## [01]-[INDEX]

- [01]-[GEO_SOURCE]: the four-format capability axis under one shared factory, the `GeoSpec` descriptor carrying the admission posture, the closed ingest/egress/probe op family with arity-honest binary egress, the CRS and validity gates, the H3 bucket derivation, and the typed fact stream.
- [02]-[FEATURE_ROWS]: the `GeoFeatureRow` currency — canonical WKB + content key + cell set + deferred properties — the one `Bind<T>` reify seam, and the GeoPackage container spine read and attributed write.

## [02]-[GEO_SOURCE]

- Owner: `GeoFormat` carries property/measure capability; `GeoAdmission` carries the shared factory, ordinate cap, codec instances, plural fold, and `ToCellFrame` projection; `CrsPolicy` carries admitted payload SRIDs; `GeoSpec` fixes format, source, CRS, admission, H3 resolution, and layer; `GeoOp`/`GeoYield` close dispatch; `GeoIngestFault` closes the accumulating fault band; `GeoFactKind`/`GeoFact` close receipts; `GeoSource` owns `Run`.
- Cases: `GeoOp.Ingest(GeoSpec)` decodes into `Seq<GeoFeatureRow>`; `GeoOp.Egress(GeoSpec, Seq<GeoPayload>)` writes GeoJSON as one `FeatureCollection`, GeoPackage as GPB plus attributes, and WKB/WKT as one arity-honest `GeometryCollection`; `GeoOp.Probe(GeoSpec)` yields layer identity, CRS, geometry column/type, Z/M rules, spatial-index presence, and count. `GeoIngestFault` is `CodecReject | CrsUnsupported | CrsMismatch | GeometryInvalid | CapabilityLoss | LayerMissing | Aggregate` (`8441`-`8447`). `GeoFactKind` closes `ingest | egress | probe`.
- Entry: `public static IO<Validation<GeoIngestFault, GeoYield>> Run(GeoOp op, ProjectionContext frame, Func<GeoFact, IO<Unit>> sink)` — ONE polymorphic entry over the closed op union through the generated total `Switch`; the typed-properties reify is NOT a second entry — it is the `GeoFeatureRow.Bind<T>` member on the yielded row.
- Auto: all codecs bind one factory and ordinate cap. GeoPackage gates `GeoPackageBinaryHeader.SrsId` against both `CrsPolicy` and the registered spine before decoding. Strict parse and `Geometry.IsValid` precede minting. `ToCellFrame` preserves payload geometry while projecting a WGS84 indexing copy; non-`4326` output refuses before H3. Cell derivation covers points, multipoints, lines, multilines, polygons, multipolygons, and recursive collections — `Fill` itself splits an antimeridian-crossing polygon (`IsTransMeridian` gating its internal lon±360 `SplitGeometry`), so no caller-side hemisphere split exists; an unsupported collection member, invalid/uncovered cell set, or mixed resolution refuses without partial indexing. Egress validates every payload SRID, measure capability, property capability, and selected GeoPackage layer SRID before mutation.
- Receipt: every op rides a `GeoFact` under `store.geo.*` — an `ingest` fact carrying the format key, feature count, and derived-cell total; an `egress` fact carrying the format and feature count; a `probe` fact carrying the layer count — one kind-discriminated stream stamped `frame.Now()`.
- Packages: NetTopologySuite.IO.GeoPackage (`GeoPackageGeoReader`/`GeoPackageGeoWriter`/`GeoPackageBinaryHeader`), NetTopologySuite.IO.GeoJSON4STJ (`GeoJsonConverterFactory` via `GeoJsonProjection.Default.Factory`, `IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>`, `NetTopologySuite.Features.Feature`/`FeatureCollection`/`AttributesTable`), NetTopologySuite (`WKBReader`/`WKBWriter`/`WKTReader`/`WKTWriter`/`NtsGeometryServices`/`GeometryFactory.CreateGeometryCollection`/`Geometry.IsValid`/`PrecisionModel`/`Ordinates`), pocketken.H3 (`H3Index.FromPoint`, `Geometry.Fill` — antimeridian split internal, `LineString.Fill`, `H3Index.Invalid`, the `ulong` durable form), Microsoft.Data.Sqlite (the GeoPackage container spine read — already admitted), Rasm.Persistence (`Element/codec` `ContentAddress`/`GeoJsonProjection`, `Element/identity` `H3Cell`, `Element/graph` `FaultBand`/`ProjectionContext`, `Ingest/tabular` `Origin`), LanguageExt.Core, Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, NodaTime, NodaTime.Serialization.SystemTextJson, BCL inbox.
- Growth: a new wire projection is one `GeoFormat` row plus its codec arms in the format `Switch` (broken loudly at compile time); a new CRS stance is one `CrsPolicy` value (DATA, zero code); a new ordinate posture is one `GeoAdmission` value on the spec; a new fault class is one case inside the registry decade; zero new surface — a per-codec `GeometryFactory`, a raw-WKB read of a GPB blob (the header is unparseable to a raw reader), a `RepairRings`-on row beside content addressing, a WKT `string.Split` parse, a hand-spelled GeoJSON shaper, an unframed plural byte egress, a second H3 coordinate model, or a geo→element map inside this codec is the deleted form.
- Boundary: NTS `Geometry` is the SINGLE interior vocabulary and a store-to-feed flow is decode-blob → interior → encode-text, never a direct transcode; WKB is the canonical interchange binary — the content key hashes the WKB bytes, so identity is storage-codec-independent; the GeoJSON id convention rides the ONE `GeoJsonProjection` row (two partner id conventions are two projection rows on two options instances, never post-read patching); precision is admission-side (the reader's `PrecisionModel` applies as coordinates parse; writers emit stored doubles raw), so emitted-text hash stability comes from constructing under the fixed factory BEFORE serialization; XYM/XYZM degrade silently on the GeoJSON text wire, so measure-bearing data routes through a `CarriesMeasures` row — the capability is the format row's column; `→ Element/identity#ELEMENT_IDENTITY` (cell derivation — the `H3Cell`/`Cell(Envelope,int)` owner, leg-3→leg-1 downward), `← Element/codec#GeoJsonProjection` (converter graph), `→ Rasm.Element` (row shape only), `← Rasm.Bim/Semantics/geospatial` (feature ingress over the `GeoFeatureWkb` wire); the GDAL/OGR GeoParquet COLUMNAR lane is `Query/columnar`'s — this page owns feature-file codecs, never a columnar reader.

```csharp signature
using Rasm.Persistence.Element;
using Expected = Rasm.Domain.Expected;

// --- [TYPES] ----------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeoFormat {
    public static readonly GeoFormat GeoPackage = new("gpkg", carriesProperties: true, carriesMeasures: true);
    public static readonly GeoFormat GeoJson = new("geojson", carriesProperties: true, carriesMeasures: false);
    public static readonly GeoFormat Wkb = new("wkb", carriesProperties: false, carriesMeasures: true);
    public static readonly GeoFormat Wkt = new("wkt", carriesProperties: false, carriesMeasures: true);
    public bool CarriesProperties { get; }
    public bool CarriesMeasures { get; }
    private GeoFormat(string key, bool carriesProperties, bool carriesMeasures) : this(key) =>
        (CarriesProperties, CarriesMeasures) = (carriesProperties, carriesMeasures);
}

public readonly record struct CrsPolicy(int Canonical, FrozenSet<int> Admissible) {
    public static readonly CrsPolicy Wgs84 = new(4326, FrozenSet.ToFrozenSet([4326]));
    public bool Admits(int srid) => Admissible.Contains(srid);
}

[SmartEnum<int>]
public sealed partial class GeoOrdinateRule {
    public static readonly GeoOrdinateRule Forbidden = new(0);
    public static readonly GeoOrdinateRule Required = new(1);
    public static readonly GeoOrdinateRule Optional = new(2);

    static readonly Lazy<FrozenDictionary<int, GeoOrdinateRule>> ByWire =
        new(static () => Items.ToFrozenDictionary(static rule => rule.Key));

    public static GeoOrdinateRule Of(long wire) => ByWire.Value[(int)wire];
}

// --- [MODELS] ---------------------------------------------------------------------------

public sealed record GeoAdmission(GeometryFactory Factory, Ordinates Cap, Func<Geometry, Geometry> ToCellFrame) {
    public static readonly GeoAdmission Canonical = new(GeoJsonProjection.Default.Geometry, Ordinates.XYZ, static shape => shape);

    public WKBReader WkbIn => new(new NtsGeometryServices(Factory.PrecisionModel, Factory.SRID)) { IsStrict = true, HandleSRID = true, HandleOrdinates = Cap };
    public WKBWriter WkbOut => new(
        ByteOrder.LittleEndian,
        handleSRID: true,
        emitZ: (Cap & Ordinates.Z) != 0,
        emitM: (Cap & Ordinates.M) != 0);
    public WKTReader WktIn => new(Factory) { IsStrict = true };
    public WKTWriter WktOut => new() { OutputOrdinates = Cap };
    public GeoPackageGeoReader GpkgIn => new() { HandleSRID = true, RepairRings = false, HandleOrdinates = Cap };
    public GeoPackageGeoWriter GpkgOut => new() { HandleOrdinates = Cap };

    public Geometry Collected(Seq<GeoPayload> features) =>
        features is [GeoPayload only] ? only.Shape : Factory.CreateGeometryCollection([.. features.Map(static f => f.Shape)]);

    public Seq<Geometry> Expanded(Geometry shape) => shape.OgcGeometryType == OgcGeometryType.GeometryCollection
        ? toSeq(Enumerable.Range(0, shape.NumGeometries)).Map(shape.GetGeometryN)
        : Seq(shape);

    public Geometry Empty() => Factory.CreateGeometryCollection();
}

[ComplexValueObject]
public sealed partial class GeoSpec {
    public GeoFormat Format { get; }
    public Origin Source { get; }
    public CrsPolicy Crs { get; }
    public GeoAdmission Admission { get; }
    public int CellResolution { get; }
    public Option<string> Layer { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref GeoFormat format, ref Origin source, ref CrsPolicy crs,
        ref GeoAdmission admission, ref int cellResolution, ref Option<string> layer) {
        if (source is Origin.FromPath { Path: string path } && string.IsNullOrWhiteSpace(path)) {
            validationError = ValidationError.Create("<geo-spec-path>");
        } else if (cellResolution is < 0 or > 15) {
            validationError = ValidationError.Create("<geo-spec-resolution>");
        } else if (!crs.Admissible.Contains(crs.Canonical) || admission.Factory.SRID != crs.Canonical) {
            validationError = ValidationError.Create("<geo-spec-crs-factory>");
        } else if (format == GeoFormat.GeoJson && crs.Canonical != 4326) {
            validationError = ValidationError.Create("<geo-spec-geojson-crs>");
        } else if (layer.Map(string.IsNullOrWhiteSpace).IfNone(false) || (layer.IsSome && format != GeoFormat.GeoPackage)) {
            validationError = ValidationError.Create("<geo-spec-layer>");
        } else if (format == GeoFormat.GeoPackage && source is Origin.FromStream) {
            validationError = ValidationError.Create("<geo-spec-container-path>");
        } else if (!format.CarriesMeasures && (admission.Cap & Ordinates.M) != 0) {
            validationError = ValidationError.Create("<geo-spec-measure-loss>");
        }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeoProperties {
    private GeoProperties() { }
    public sealed record Deferred(IPartiallyDeserializedAttributesTable Table) : GeoProperties;
    public sealed record Columns(HashMap<string, object?> Bag) : GeoProperties;
    public sealed record Bare : GeoProperties;
}

public readonly record struct GeoFeatureRow(Geometry Shape, ReadOnlyMemory<byte> Wkb, ContentAddress Content, Seq<H3Cell> Cells, GeoProperties Properties);

public readonly record struct GeoPayload(Geometry Shape, HashMap<string, object?> Properties);

public readonly record struct GeoLayer(
    string Name,
    int Srid,
    string GeometryColumn,
    string GeometryType,
    GeoOrdinateRule Z,
    GeoOrdinateRule M,
    bool Indexed,
    long Features);

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
[Union]
public abstract partial record GeoIngestFault : Expected, IValidationError<GeoIngestFault>, Semigroup<GeoIngestFault> {
    private GeoIngestFault() : base() { }
    public sealed record CodecReject(string Format, string Detail) : GeoIngestFault;
    public sealed record CrsUnsupported(int Srid) : GeoIngestFault;
    public sealed record CrsMismatch(int Registered, int Payload) : GeoIngestFault;
    public sealed record GeometryInvalid(string Reason) : GeoIngestFault;
    public sealed record CapabilityLoss(string Format, string Capability) : GeoIngestFault;
    public sealed record LayerMissing(string Layer) : GeoIngestFault;
    public sealed record Aggregate(Seq<GeoIngestFault> Faults) : GeoIngestFault;

    public override int Code => FaultBand.GeoIngest + Switch(
        codecReject:     static _ => 1,
        crsUnsupported:  static _ => 2,
        crsMismatch:     static _ => 3,
        geometryInvalid: static _ => 4,
        capabilityLoss:  static _ => 5,
        layerMissing:    static _ => 6,
        aggregate:       static _ => 7);

    public override string Message => Switch(
        codecReject:     static c => $"<geo-codec-reject:{c.Format}:{c.Detail}>",
        crsUnsupported:  static c => $"<geo-crs-unsupported:{c.Srid}>",
        crsMismatch:     static c => $"<geo-crs-mismatch:{c.Registered}:{c.Payload}>",
        geometryInvalid: static c => $"<geo-geometry-invalid:{c.Reason}>",
        capabilityLoss:  static c => $"<geo-capability-loss:{c.Format}:{c.Capability}>",
        layerMissing:    static c => $"<geo-layer-missing:{c.Layer}>",
        aggregate:       static c => $"<geo-aggregate:{c.Faults.Count}>");

    public override string Category => Switch(
        codecReject:     static _ => "Codec",
        crsUnsupported:  static _ => "Crs",
        crsMismatch:     static _ => "Crs",
        geometryInvalid: static _ => "Geometry",
        capabilityLoss:  static _ => "Capability",
        layerMissing:    static _ => "Layer",
        aggregate:       static _ => "Aggregate");

    public static GeoIngestFault Create(string message) => new CodecReject("wire", message);

    public GeoIngestFault Combine(GeoIngestFault rhs) => (this, rhs) switch {
        (Aggregate l, Aggregate r) => new Aggregate(l.Faults + r.Faults),
        (Aggregate l, _) => new Aggregate(l.Faults.Add(rhs)),
        (_, Aggregate r) => new Aggregate(this.Cons(r.Faults)),
        _ => new Aggregate(Seq(this, rhs)),
    };

    public static GeoIngestFault Lift(GeoFormat format, Exception boundary) => boundary switch {
        JsonException wire => new CodecReject(format.Key, $"{wire.Path}:{wire.Message}"),
        ArgumentException typeLiteral => new CodecReject(format.Key, typeLiteral.Message),
        ParseException malformed => new GeometryInvalid(malformed.Message),
        _ => new CodecReject(format.Key, boundary.Message),
    };
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeoFactKind {
    public static readonly GeoFactKind Ingest = new("ingest");
    public static readonly GeoFactKind Egress = new("egress");
    public static readonly GeoFactKind Probe = new("probe");
}

public readonly record struct GeoFact(GeoFactKind Kind, string Format, long Features, long Cells, Instant At);

// --- [OPERATIONS] -----------------------------------------------------------------------

public static class GeoSource {
    // The registry-mounted census derives from the kind vocabulary; the `store.geo.` prefix declares once here.
    public static readonly Seq<StoreSlot> Slots =
        toSeq(GeoFactKind.Items).Map(static kind => StoreSlot.Create($"store.geo.{kind.Key}"));

    public static IO<Validation<GeoIngestFault, GeoYield>> Run(GeoOp op, ProjectionContext frame, Func<GeoFact, IO<Unit>> sink) =>
        op.Switch(
            (frame, sink),
            ingest: static (s, i) => Ingested(i.Spec, s.frame, s.sink),
            egress: static (s, e) => Emitted(e.Spec, e.Features, s.frame, s.sink),
            probe:  static (s, p) => Probed(p.Spec, s.frame, s.sink));

    static IO<Validation<GeoIngestFault, GeoYield>> Ingested(GeoSpec spec, ProjectionContext frame, Func<GeoFact, IO<Unit>> sink) =>
        from rows in IO.lift(() => Capture(spec.Format, () => spec.Format.Switch(
                spec,
                geoPackage: static s => GeoContainer.Features(s),
                geoJson:    static s => GeoWire.Features(s),
                wkb:        static s => s.Admission.Expanded(s.Admission.WkbIn.Read(Bytes(s.Source)))
                    .Map(static shape => (shape, (GeoProperties)new GeoProperties.Bare())),
                wkt:        static s => s.Admission.Expanded(s.Admission.WktIn.Read(Text(s.Source)))
                    .Map(static shape => (shape, (GeoProperties)new GeoProperties.Bare()))))
            .Bind(features => features.Traverse(f => Row(spec, f)).As()))
        from _ in rows.Match(
            Succ: y => sink(new GeoFact(GeoFactKind.Ingest, spec.Format.Key, y.Count, y.Sum(static r => (long)r.Cells.Count), frame.Now())),
            Fail: _ => IO.pure(unit))
        select rows.Map(static y => (GeoYield)new GeoYield.Features(y));

    static Validation<GeoIngestFault, GeoFeatureRow> Row(GeoSpec spec, (Geometry Shape, GeoProperties Properties) feature) =>
        !spec.Crs.Admits(feature.Shape.SRID)
            ? new GeoIngestFault.CrsUnsupported(feature.Shape.SRID)
            : !feature.Shape.IsValid
                ? new GeoIngestFault.GeometryInvalid(feature.Shape.GeometryType)
                : Minted(spec, feature.Shape, feature.Properties);

    static Validation<GeoIngestFault, GeoFeatureRow> Minted(GeoSpec spec, Geometry shape, GeoProperties properties) {
        byte[] wkb = spec.Admission.WkbOut.Write(shape);
        Geometry indexed = spec.Admission.ToCellFrame(shape);
        if (indexed.SRID != 4326) { return new GeoIngestFault.CrsUnsupported(indexed.SRID); }
        if (!CellShape(indexed)) { return new GeoIngestFault.GeometryInvalid($"<h3-unsupported:{indexed.GeometryType}>"); }
        Seq<H3Index> cells = Cells(indexed, spec.CellResolution);
        return (indexed.IsEmpty, cells.IsEmpty, cells.AreOfSameResolution()) switch {
            (false, true, _) => new GeoIngestFault.GeometryInvalid($"<h3-uncovered:{indexed.GeometryType}>"),
            (_, _, false) => new GeoIngestFault.GeometryInvalid("<mixed-cell-resolution>"),
            _ => new GeoFeatureRow(shape, wkb, ContentAddress.Of(wkb.AsSpan()), cells.Map(H3Cell.Of), properties),
        };
    }

    static Seq<H3Index> Cells(Geometry shape, int resolution) => shape switch {
        { IsEmpty: true } => Seq<H3Index>(),
        Point point => Cell(H3Index.FromPoint(point, resolution)),
        MultiPoint points => Parts(points).Choose(part => part is Point point
            ? Some(H3Index.FromPoint(point, resolution))
            : None).Filter(static cell => cell != H3Index.Invalid),
        LineString line => toSeq(line.Fill(resolution)).Filter(static c => c != H3Index.Invalid),
        MultiLineString lines => Parts(lines).Bind(part => part is LineString line
            ? toSeq(line.Fill(resolution))
            : Seq<H3Index>()).Filter(static cell => cell != H3Index.Invalid).Distinct(),
        // Fill splits an antimeridian-crossing polygon internally (IsTransMeridian -> lon±360 SplitGeometry) — a caller-side split re-derives the package.
        Polygon or MultiPolygon => toSeq(shape.Fill(resolution)).Filter(static c => c != H3Index.Invalid),
        GeometryCollection collection => Parts(collection).Bind(part => Cells(part, resolution)).Distinct(),
        _ => Seq<H3Index>(),
    };

    static Seq<Geometry> Parts(Geometry collection) =>
        toSeq(Enumerable.Range(0, collection.NumGeometries)).Map(collection.GetGeometryN);

    static bool CellShape(Geometry shape) => shape switch {
        { IsEmpty: true } => true,
        Point or LineString or Polygon => true,
        MultiPoint or MultiLineString or MultiPolygon or GeometryCollection => Parts(shape).ForAll(CellShape),
        _ => false,
    };

    static Seq<H3Index> Cell(H3Index cell) =>
        cell == H3Index.Invalid ? Seq<H3Index>() : Seq(cell);

    static IO<Validation<GeoIngestFault, GeoYield>> Emitted(GeoSpec spec, Seq<GeoPayload> features, ProjectionContext frame, Func<GeoFact, IO<Unit>> sink) =>
        Payload(spec, features).Match(
            Fail: fault => IO.pure((Validation<GeoIngestFault, GeoYield>)fault),
            Succ: _ => Emit(spec, features, frame, sink));

    static IO<Validation<GeoIngestFault, GeoYield>> Emit(GeoSpec spec, Seq<GeoPayload> features, ProjectionContext frame, Func<GeoFact, IO<Unit>> sink) =>
        from done in IO.lift(() => Capture(spec.Format, () => spec.Format.Switch(
            (spec, features, frame.Now()),
            geoPackage: static s => GeoContainer.Write(s.spec, s.features, s.Item3),
            geoJson:    static s => GeoWire.Write(s.spec, s.features),
            wkb:        static s => Binary(s.spec, s.spec.Admission.WkbOut.Write(s.spec.Admission.Collected(s.features))),
            wkt:        static s => Binary(s.spec, Encoding.UTF8.GetBytes(s.spec.Admission.WktOut.Write(s.spec.Admission.Collected(s.features)))))))
        from _ in done.Match(Succ: _ => sink(new GeoFact(GeoFactKind.Egress, spec.Format.Key, features.Count, 0L, frame.Now())), Fail: _ => IO.pure(unit))
        select done.Map(_ => (GeoYield)new GeoYield.Written(features.Count));

    static Validation<GeoIngestFault, Unit> Payload(GeoSpec spec, Seq<GeoPayload> features) {
        // ONE validity policy for ingest AND egress: every feature passes the strict Geometry.IsValid gate before
        // any writer runs, so invalid topology can never serialize — the shared fold that keeps the no-repair
        // byte-identity posture honest on the egress side too.
        Seq<GeoPayload> invalid = features.Filter(static feature => !feature.Shape.IsValid);
        if (!invalid.IsEmpty) { return new GeoIngestFault.GeometryInvalid(invalid[0].Shape.GeometryType); }
        Seq<GeoPayload> unadmitted = features.Filter(feature => !spec.Crs.Admits(feature.Shape.SRID));
        if (!unadmitted.IsEmpty) { return new GeoIngestFault.CrsUnsupported(unadmitted[0].Shape.SRID); }
        if (!spec.Format.CarriesProperties && features.Exists(static feature => !feature.Properties.IsEmpty)) {
            return new GeoIngestFault.CapabilityLoss(spec.Format.Key, "properties");
        }
        if (!spec.Format.CarriesMeasures && features.Exists(static feature => feature.Shape.Coordinates.Any(static coordinate => !double.IsNaN(coordinate.M)))) {
            return new GeoIngestFault.CapabilityLoss(spec.Format.Key, "measures");
        }
        return unit;
    }

    static IO<Validation<GeoIngestFault, GeoYield>> Probed(GeoSpec spec, ProjectionContext frame, Func<GeoFact, IO<Unit>> sink) =>
        from roster in IO.lift(() => Capture(spec.Format, () => spec.Format.Switch(
            spec,
            geoPackage: static s => GeoContainer.Spine(s),
            geoJson:    static s => GeoWire.Census(s),
            wkb: static s => Layer("wkb", s.Admission.WkbIn.Read(Bytes(s.Source)), s.Admission.Cap),
            wkt: static s => Layer("wkt", s.Admission.WktIn.Read(Text(s.Source)), s.Admission.Cap))))
        from _ in roster.Match(Succ: y => sink(new GeoFact(GeoFactKind.Probe, spec.Format.Key, y.Count, 0L, frame.Now())), Fail: _ => IO.pure(unit))
        select roster.Map(static y => (GeoYield)new GeoYield.Roster(y));

    static Seq<GeoLayer> Layer(string name, Geometry shape, Ordinates cap) => Seq(new GeoLayer(
        name,
        shape.SRID,
        "geometry",
        shape.GeometryType,
        (cap & Ordinates.Z) != 0 ? GeoOrdinateRule.Required : GeoOrdinateRule.Forbidden,
        (cap & Ordinates.M) != 0 ? GeoOrdinateRule.Required : GeoOrdinateRule.Forbidden,
        false,
        1L));

    static Unit Binary(GeoSpec spec, byte[] payload) =>
        spec.Source.Read(
            path:   p => { File.WriteAllBytes(p, payload); return unit; },
            stream: s => { s.Write(payload); return unit; });

    static byte[] Bytes(Origin source) => source.Read(path: File.ReadAllBytes, stream: static s => { using MemoryStream buffered = new(); s.CopyTo(buffered); return buffered.ToArray(); });
    static string Text(Origin source) => source.Read(path: File.ReadAllText, stream: static s => new StreamReader(s).ReadToEnd());

    internal static Validation<GeoIngestFault, TValue> Capture<TValue>(GeoFormat format, Func<TValue> codec) =>
        Try.lift(codec).Run().Match(
            Succ: static value => (Validation<GeoIngestFault, TValue>)value,
            Fail: e => e.ToException() is GeoRefusal refusal
                ? (Validation<GeoIngestFault, TValue>)refusal.Fault
                : GeoIngestFault.Lift(format, e.ToException()));
}

public sealed class GeoRefusal(GeoIngestFault fault) : Exception(fault.Message) { public GeoIngestFault Fault { get; } = fault; }
```

| [INDEX] | [POLICY]            | [VALUE]                                          | [BINDING]                                                       |
| :-----: | :------------------ | :----------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | one geo owner       | `GeoSource.Run` over `GeoOp`                     | ingest/egress/probe are cases of ONE dispatch                    |
|  [02]   | one factory         | `GeoAdmission` off `GeoJsonProjection`           | four codecs, one precision grid; readers factory-bound           |
|  [03]   | interior vocabulary | NTS `Geometry` only                              | decode → interior → encode; no direct transcode, no DTO fork     |
|  [04]   | canonical bytes     | EWKB via `WKBWriter(handleSRID: true)`           | `ContentAddress.Of(wkb)` key; codec-independent                  |
|  [05]   | CRS gate            | `CrsPolicy` set membership, cheapest-first       | GPB `SrsId`/EWKB SRID/spine `srs_id`; GeoJSON fixed WGS84        |
|  [06]   | validity gate       | strict parse + `Geometry.IsValid`                | `RepairRings` off — byte identity and repair are exclusive       |
|  [07]   | fault accumulation  | `Semigroup` + `Aggregate` through `Traverse`     | a bulk ingest reports every refused feature, not the first       |
|  [08]   | plural binary wire  | `Collected` → one `GeometryCollection`           | egress arity is the value's shape; concatenation deleted         |
|  [09]   | H3 buckets          | `FromPoint`/`Fill` at spec resolution            | `h3-pg` bit parity; `Invalid` and empty shapes contribute nothing |
|  [10]   | fault band          | `Code => FaultBand.GeoIngest + n`                | `8441`-`8447` off the `graph#FAULT_TABLES` registry              |
|  [11]   | receipt             | one `GeoFact` stream `store.geo.*`               | kind-discriminated; never parallel records                       |
|  [12]   | element projection  | per-app geo→element map                          | `[02]-[SEAMS]` `Ingest → Rasm.Element` wire; Bim consumes features |

## [03]-[FEATURE_ROWS]

- Owner: `GeoFeatureRow` the one feature currency (`Shape` + canonical `Wkb` + `Content` key + `Cells` + `Properties`); `GeoProperties` the closed deferred-properties family with its one `Bind<T>` reify; `GeoWire` the GeoJSON text seam over its open-resolver `Options` carrying the shared `GeoJsonProjection.Default.Factory` row; `GeoContainer` the GeoPackage container seam over the admitted `Microsoft.Data.Sqlite` — the three-table metadata spine (`gpkg_contents`/`gpkg_geometry_columns`) binding each feature table to exactly one geometry column and SRID.
- Cases: `GeoProperties.Deferred` holds the GeoJSON element-backed table — reified typed ONLY through `TryDeserializeJsonObject<T>(GeoWire.Options, out …)` so a feature's geometry and its typed properties resolve under ONE geometry converter row (a false return is absence, never a throw); `GeoProperties.Columns` holds the GeoPackage attribute-column bag — bound through the same STJ wire round-trip tabular cells mint through; `GeoProperties.Bare` is the Wkb/Wkt geometry-only row.
- Entry: `GeoRows` `extension(GeoFeatureRow row)` member `public Validation<GeoIngestFault, Option<T>> Bind<T>()` dispatches the properties union through the typed codec rail; loose `IAttributesTable` walks are rejected.
- Auto: container reads derive layer name, geometry column/type, SRID, Z/M rules, spatial-index presence, and count from `gpkg_geometry_columns`, `gpkg_contents`, and `sqlite_master`. Selected-layer absence reaches `LayerMissing`; GPB/header-to-spine disagreement reaches `CrsMismatch`. Egress admits only registered attribute columns, quotes every identifier, binds typed `SqliteParameter` values, and writes rows plus an extant R-tree plus the `gpkg_contents` extent in one immediate transaction stamped from `ProjectionContext`.
- Receipt: rides `[02]`'s facts — the container read contributes its per-layer feature counts to the one `ingest` fact.
- Packages: covered by `[02]`.
- Growth: a new properties source is one `GeoProperties` case plus one `Bind<T>` arm (compile-broken); a new spine gate is one probe row in the cheapest-first ladder; zero new surface — a second reify path beside `Bind<T>`, a per-format row type, a geometry-only attributed write, or a raw-WKB read of a GPB column is the deleted form.
- Boundary: GPB headers own payload SRID and must equal the registered spine SRID; `HandleSRID` stamps the admitted value onto geometry. GeoJSON null geometry projects to the empty collection so properties survive without an interior null. `Store/provisioning#EMBEDDED_FLOOR` owns database lifecycle; this page mounts an existing `.gpkg` read-only for ingest/probe or read-write for an attributed layer transaction.

```csharp signature
// --- [OPERATIONS] -----------------------------------------------------------------------

public static class GeoRows {
    extension(GeoFeatureRow row) {
        public Validation<GeoIngestFault, Option<T>> Bind<T>() => row.Properties.Switch(
            deferred: static d => GeoSource.Capture(GeoFormat.GeoJson, () =>
                d.Table.TryDeserializeJsonObject<T>(GeoWire.Options, out T? typed) ? Optional(typed) : None),
            columns: static c => GeoSource.Capture(GeoFormat.GeoPackage, () => Optional(JsonSerializer.Deserialize<T>(
                JsonSerializer.SerializeToUtf8Bytes(c.Bag.ToDictionary(), GeoWire.Options), GeoWire.Options))),
            bare: static _ => (Validation<GeoIngestFault, Option<T>>)Option<T>.None);
    }
}

public static class GeoWire {
    public static readonly JsonSerializerOptions Options =
        new JsonSerializerOptions(JsonSerializerOptions.Default) {
            Converters = { new ThinktectureJsonConverterFactory(), GeoJsonProjection.Default.Factory },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    public static Seq<(Geometry Shape, GeoProperties Properties)> Features(GeoSpec spec) {
        FeatureCollection collection = spec.Source.Read(
            path:   static p => JsonSerializer.Deserialize<FeatureCollection>(File.ReadAllBytes(p), Options),
            stream: static s => JsonSerializer.Deserialize<FeatureCollection>(s, Options)) ?? [];
        return toSeq(collection).Map(feature => (
            feature.Geometry ?? spec.Admission.Empty(),
            feature.Attributes is IPartiallyDeserializedAttributesTable table
                ? (GeoProperties)new GeoProperties.Deferred(table)
                : new GeoProperties.Bare()));
    }

    public static Unit Write(GeoSpec spec, Seq<GeoPayload> features) {
        FeatureCollection collection = [];
        features.Iter(f => collection.Add(new Feature(f.Shape, new AttributesTable(f.Properties.ToDictionary()))));
        return spec.Source.Read(
            path:   p => { File.WriteAllBytes(p, JsonSerializer.SerializeToUtf8Bytes(collection, Options)); return unit; },
            stream: s => { JsonSerializer.Serialize(s, collection, Options); return unit; });
    }

    public static Seq<GeoLayer> Census(GeoSpec spec) =>
        Seq(new GeoLayer("features", 4326, "geometry", nameof(FeatureCollection), GeoOrdinateRule.Optional, GeoOrdinateRule.Forbidden, false, Features(spec).Count));
}

public static class GeoContainer {
    public static Seq<(Geometry Shape, GeoProperties Properties)> Features(GeoSpec spec) =>
        spec.Source.Read(path: p => Read(p, spec), stream: _ => throw new GeoRefusal(new GeoIngestFault.CodecReject("gpkg", "<container-needs-a-path>")));

    static Seq<(Geometry, GeoProperties)> Read(string path, GeoSpec spec) {
        using SqliteConnection container = new($"Data Source={path};Mode=ReadOnly");
        container.Open();
        List<(Geometry, GeoProperties)> rows = [];
        foreach (GeoLayer layer in Layers(container, spec.Layer)) {
            if (!spec.Crs.Admits(layer.Srid)) { throw new GeoRefusal(new GeoIngestFault.CrsUnsupported(layer.Srid)); }
            using SqliteCommand features = container.CreateCommand();
            features.CommandText = $"SELECT * FROM {Quote(layer.Name)}";
            using SqliteDataReader reader = features.ExecuteReader();
            int geometryAt = reader.GetOrdinal(layer.GeometryColumn);
            while (reader.Read()) {
                byte[] payload = (byte[])reader.GetValue(geometryAt);
                using BinaryReader headerWire = new(new MemoryStream(payload));
                GeoPackageBinaryHeader header = GeoPackageBinaryHeader.Read(headerWire);
                if (!spec.Crs.Admits(header.SrsId)) { throw new GeoRefusal(new GeoIngestFault.CrsUnsupported(header.SrsId)); }
                if (header.SrsId != layer.Srid) { throw new GeoRefusal(new GeoIngestFault.CrsMismatch(layer.Srid, header.SrsId)); }
                Geometry shape = spec.Admission.GpkgIn.Read(payload);
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

    static Seq<GeoLayer> Layers(SqliteConnection container, Option<string> selected) {
        List<GeoLayer> layers = [];
        using (SqliteCommand spine = container.CreateCommand()) {
            spine.CommandText = """
                SELECT c.table_name, g.srs_id, g.column_name, g.geometry_type_name, g.z, g.m
                FROM gpkg_contents c JOIN gpkg_geometry_columns g ON g.table_name = c.table_name
                WHERE c.data_type = 'features'
                """;
            using SqliteDataReader reader = spine.ExecuteReader();
            while (reader.Read()) {
                layers.Add(new GeoLayer(
                    reader.GetString(0),
                    reader.GetInt32(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    GeoOrdinateRule.Of(reader.GetInt64(4)),
                    GeoOrdinateRule.Of(reader.GetInt64(5)),
                    false,
                    0L));
            }
        }
        Seq<GeoLayer> roster = toSeq(layers).Map(layer => layer with {
            Indexed = Exists(container, $"rtree_{layer.Name}_{layer.GeometryColumn}"),
            Features = Count(container, layer.Name),
        });
        return selected.Match(
            Some: name => roster.Find(layer => StringComparer.Ordinal.Equals(layer.Name, name)).Match(
                Some: static layer => Seq(layer),
                None: () => throw new GeoRefusal(new GeoIngestFault.LayerMissing(name))),
            None: () => roster);
    }

    static bool Exists(SqliteConnection container, string table) {
        using SqliteCommand exists = container.CreateCommand();
        exists.CommandText = "SELECT EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $table)";
        _ = exists.Parameters.Add(new SqliteParameter("$table", SqliteType.Text) { Value = table });
        return exists.ExecuteScalar() is 1L;
    }

    static long Count(SqliteConnection container, string table) {
        using SqliteCommand count = container.CreateCommand();
        count.CommandText = $"SELECT COUNT(*) FROM {Quote(table)}";
        return count.ExecuteScalar() is long total
            ? total
            : throw new GeoRefusal(new GeoIngestFault.CodecReject("gpkg", $"<layer-count:{table}>"));
    }

    static FrozenSet<string> Columns(SqliteConnection container, string table) {
        using SqliteCommand schema = container.CreateCommand();
        schema.CommandText = $"PRAGMA table_info({Quote(table)})";
        using SqliteDataReader reader = schema.ExecuteReader();
        List<string> columns = [];
        while (reader.Read()) { columns.Add(reader.GetString(1)); }
        return columns.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    public static Unit Write(GeoSpec spec, Seq<GeoPayload> features, Instant at) =>
        spec.Source.Read(path: p => {
            using SqliteConnection container = new($"Data Source={p};Mode=ReadWrite");
            container.Open();
            using SqliteTransaction commit = container.BeginTransaction(deferred: false);
            GeoLayer layer = Layers(container, Some(spec.Layer.IfNone("features")))[0];
            Seq<int> mismatched = features.Map(static feature => feature.Shape.SRID).Filter(srid => srid != layer.Srid);
            if (!mismatched.IsEmpty) { throw new GeoRefusal(new GeoIngestFault.CrsMismatch(layer.Srid, mismatched[0])); }
            FrozenSet<string> schema = Columns(container, layer.Name);
            Envelope written = new();
            features.Iter(f => {
                string[] columns = [.. f.Properties.Keys];
                Option<string> missing = toSeq(columns).Find(column => !schema.Contains(column) || StringComparer.OrdinalIgnoreCase.Equals(column, layer.GeometryColumn));
                missing.IfSome(column => throw new GeoRefusal(new GeoIngestFault.CapabilityLoss("gpkg", $"column:{column}")));
                using SqliteCommand insert = container.CreateCommand();
                insert.Transaction = commit;
                insert.CommandText =
                    $"INSERT INTO {Quote(layer.Name)} ({Quote(layer.GeometryColumn)}{string.Concat(columns.Select(c => $", {Quote(c)}"))}) " +
                    $"VALUES ($blob{string.Concat(columns.Select(static (_, i) => $", $p{i}"))}) RETURNING rowid";
                _ = insert.Parameters.Add(new SqliteParameter("$blob", SqliteType.Blob) { Value = spec.Admission.GpkgOut.Write(f.Shape) });
                for (int i = 0; i < columns.Length; i++) {
                    _ = insert.Parameters.Add(Parameter($"$p{i}", f.Properties[columns[i]]));
                }
                long rowid = insert.ExecuteScalar() is long id
                    ? id
                    : throw new GeoRefusal(new GeoIngestFault.CodecReject("gpkg", "<insert-rowid>"));
                Envelope bound = f.Shape.EnvelopeInternal;
                written.ExpandToInclude(bound);
                if (layer.Indexed) {
                    using SqliteCommand index = container.CreateCommand();
                    index.Transaction = commit;
                    index.CommandText = $"INSERT OR REPLACE INTO {Quote($"rtree_{layer.Name}_{layer.GeometryColumn}")} (id, minx, maxx, miny, maxy) VALUES ($id, $minx, $maxx, $miny, $maxy)";
                    _ = index.Parameters.Add(new SqliteParameter("$id", SqliteType.Integer) { Value = rowid });
                    _ = index.Parameters.Add(new SqliteParameter("$minx", SqliteType.Real) { Value = bound.MinX });
                    _ = index.Parameters.Add(new SqliteParameter("$maxx", SqliteType.Real) { Value = bound.MaxX });
                    _ = index.Parameters.Add(new SqliteParameter("$miny", SqliteType.Real) { Value = bound.MinY });
                    _ = index.Parameters.Add(new SqliteParameter("$maxy", SqliteType.Real) { Value = bound.MaxY });
                    _ = index.ExecuteNonQuery();
                }
            });
            if (!written.IsNull) {
                using SqliteCommand extent = container.CreateCommand();
                extent.Transaction = commit;
                extent.CommandText = """
                    UPDATE gpkg_contents SET
                        min_x = MIN(COALESCE(min_x, $minx), $minx), min_y = MIN(COALESCE(min_y, $miny), $miny),
                        max_x = MAX(COALESCE(max_x, $maxx), $maxx), max_y = MAX(COALESCE(max_y, $maxy), $maxy),
                        last_change = $changed
                    WHERE table_name = $layer
                    """;
                _ = extent.Parameters.Add(new SqliteParameter("$minx", SqliteType.Real) { Value = written.MinX });
                _ = extent.Parameters.Add(new SqliteParameter("$miny", SqliteType.Real) { Value = written.MinY });
                _ = extent.Parameters.Add(new SqliteParameter("$maxx", SqliteType.Real) { Value = written.MaxX });
                _ = extent.Parameters.Add(new SqliteParameter("$maxy", SqliteType.Real) { Value = written.MaxY });
                _ = extent.Parameters.Add(new SqliteParameter("$changed", SqliteType.Text) { Value = InstantPattern.ExtendedIso.Format(at) });
                _ = extent.Parameters.Add(new SqliteParameter("$layer", SqliteType.Text) { Value = layer.Name });
                _ = extent.ExecuteNonQuery();
            }
            commit.Commit();
            return unit;
        }, stream: _ => throw new GeoRefusal(new GeoIngestFault.CodecReject("gpkg", "<container-needs-a-path>")));

    static SqliteParameter Parameter(string name, object? value) => value switch {
        null => new SqliteParameter(name, SqliteType.Text) { Value = DBNull.Value },
        byte[] bytes => new SqliteParameter(name, SqliteType.Blob) { Value = bytes },
        bool flag => new SqliteParameter(name, SqliteType.Integer) { Value = flag ? 1L : 0L },
        sbyte or byte or short or ushort or int or uint or long => new SqliteParameter(name, SqliteType.Integer) { Value = value },
        float or double or decimal => new SqliteParameter(name, SqliteType.Real) { Value = value },
        _ => new SqliteParameter(name, SqliteType.Text) { Value = value },
    };

    static string Quote(string identifier) => $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
```

| [INDEX] | [POLICY]            | [VALUE]                                   | [BINDING]                                                          |
| :-----: | :------------------ | :---------------------------------------- | :------------------------------------------------------------------ |
|  [01]   | deferred properties | `GeoProperties` union + one `Bind<T>`     | element-backed until projected; loose table walks rejected          |
|  [02]   | one converter graph | `GeoJsonProjection.Default.Factory` row   | one geometry converter; `GeoWire.Options` open resolver binds POCOs |
|  [03]   | spine authority     | `gpkg_geometry_columns` + `gpkg_contents` | one geometry column + SRID per layer; header is the SRID authority  |
|  [04]   | deep-gate faults    | `GeoRefusal` typed carrier                | a spine CRS refusal surfaces typed, never a flattened message       |
|  [05]   | null geometry       | empty collection under the one factory    | unlocated features keep properties; no null crosses inward          |
|  [06]   | container mechanics | existing `Microsoft.Data.Sqlite` mount    | read-only ingest; immediate attributed write transaction            |
|  [07]   | layer write         | one txn: rows + columns + rtree + extent  | attributed features persist their bag; a stale extent misleads      |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

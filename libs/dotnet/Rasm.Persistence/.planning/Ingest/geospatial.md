# [PERSISTENCE_INGEST_GEOSPATIAL]

Rasm.Persistence ingests and emits geospatial features through ONE `GeoSource` owner over the NTS-IO codec family — the `[A.4]` Ingest growth row ("the next foreign-file codec into the record pipeline lands as a page HERE") made real: a `GeoFormat` `[SmartEnum<string>]` crosses the four wire projections (`GeoPackage` the GPB-header-plus-WKB blob over the already-admitted `Microsoft.Data.Sqlite` container, `GeoJson` the RFC-7946 feature text, `Wkb`/`Wkt` the core-NTS binary/text pair), each row carrying ONE `CapabilitySet<GeoCapability>` column over the kernel capability floor — properties, measures, layers, a free CRS, a streamable source — so what a wire can carry is set membership a new format extends by one row, and `GeoFormat.Law` bars the corner a layered container cannot occupy. Every format decodes into ONE interior currency — the NTS `Geometry` under ONE shared `GeometryFactory` — so a per-codec ad-hoc factory, a coordinate DTO fork, or a second geometry model is the deleted form. `GeoSpec` fixes a read once — format, `Origin` source, the `CrsPolicy` admissible-SRID set, the `GeoAdmission` factory-and-ordinate posture, the H3 cell resolution, and an `Option<string>` layer selector — and its admission runs ONE rule table that reports every violated rule at once, narrowing the format's declared capabilities by the ordinate cap the posture reads; the owner then discriminates ingest, egress, and probe on the closed `GeoOp` `[Union]`, never a `ReadGpkg`/`ReadGeoJson`/`WriteWkb` name family.

Every ingested feature lands as ONE `GeoFeatureRow`: decoded `Geometry`, canonical EWKB, `ContentAddress`, derived `H3Cell` buckets, and deferred `GeoProperties`. Admission is APPLICATIVE end to end — `GeoFeatureRow.Of` accumulates the payload-CRS, topology, cell-frame, and cell-shape lifts before the dependent bucket derivation sequences, the GeoPackage container accumulates every row whose GPB header refuses and every layer column the spine never registered, and the attributed write admits every feature BEFORE the transaction opens — so a malformed feature, container, or write batch reports every offending column at once instead of the first. `Validation<Error, …>` is the only carrier, `Error.Many` is its plural error shape, and `GeoSource.Capture` is the one funnel preserving genuine SDK causes. `GeoRows.Bind<T>` reifies GeoJSON properties through `IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>` and GeoPackage columns through the one `GeoWire.Options` wire — the SAME `GeoJsonProjection.Default.Factory` geometry row with the Thinktecture/NodaTime converter families over an OPEN resolver, because the `ElementJson` source-gen resolver resolves only registered graph types and a consumer POCO is not one — returning `Validation<Error, Option<T>>` so malformed properties never escape the carrier. Rows project to `Rasm.Element` only at the app composition root. `GeoIngestFault` reaches codec, CRS admission, CRS mismatch, geometry validity, capability-loss, missing-layer, and unregistered-column cases. Facts ride `store.geo.*`. `Origin` arrives from `Ingest/tabular#TABULAR_SOURCE`; `ProjectionContext` from `Element/graph#STORE_HOOKS`; `ContentAddress` from `Element/codec`; `H3Cell` from `Element/identity`; `ICapability`/`CapabilitySet`/`CapabilityLaw` from `Rasm/Domain/validation#CAPABILITY`; `FaultBand` from the `Rasm/Domain/results#FAULT_BAND` roster.

## [01]-[INDEX]

- [02]-[GEO_SOURCE]: `GeoCapability` vocabulary and the `GeoFormat` capability column under one shared factory, the `GeoSpec` descriptor carrying the admission posture and its one rule table, the closed ingest/egress/probe op family with arity-honest binary egress, the accumulating CRS and validity gates, the H3 bucket derivation, and the typed fact stream.
- [03]-[FEATURE_ROWS]: `GeoFeatureRow` currency — canonical WKB + content key + cell set + deferred properties — the one `Bind<T>` reify entry, the generated GeoJSON feature correspondence, and the GeoPackage container spine read and attributed write.

## [02]-[GEO_SOURCE]

- Owner: `GeoCapability` closes the wire-capability vocabulary over the kernel `ICapability` floor; `GeoFormat` carries one `CapabilitySet` column with the `Law` barring the illegal corner; `GeoAdmission` carries the shared factory, ordinate cap, codec instances, plural fold, `ToCellFrame` projection, and the GeoJSON mapper bound to that factory; `CrsPolicy` carries admitted payload SRIDs; `GeoSpec` fixes format, source, CRS, admission, H3 resolution, and layer, and derives the effective capability set; `GeoOp`/`GeoYield` close dispatch; `[FaultCase]` closes the case-grain fault roster; `GeoIngestFault` closes the accumulating family above it; `GeoSource` owns `Run`.
- Cases: `GeoOp.Ingest` decodes into `Seq<GeoFeatureRow>`; `GeoOp.Egress` writes the selected container; `GeoHost.Probe` yields layer metadata. `GeoCapability` is `properties | measures | layers | crs-free | streamable`. `GeoIngestFault` is `CodecReject | CrsUnsupported | CrsMismatch | GeometryInvalid | CapabilityLoss | LayerMissing | ColumnUnknown`; independent cases accumulate as `Error.Many`.
- Entry: `Run(GeoOp, ProjectionContext)` is the ONE polymorphic entry; typed-property reification remains `GeoFeatureRow.Bind<T>` on the yielded row.
- Auto: all codecs bind one factory and ordinate cap. `GeoSpec` admission folds ONE rule table — path, resolution, CRS-factory agreement, CRS pinning, layer selection, stream posture, measure loss, and the `CapabilityLaw` corner — reporting every violated rule in one error rather than the first. GeoPackage gates `GeoPackageBinaryHeader.SrsId` against both `CrsPolicy` and the registered spine before decoding, accumulating BOTH refusals per row. Strict parse and `Geometry.IsValid` precede minting. `ToCellFrame` preserves payload geometry while projecting a WGS84 indexing copy; non-`4326` output refuses before H3. Cell derivation covers points, multipoints, lines, multilines, polygons, multipolygons, and recursive collections — `Fill` itself splits an antimeridian-crossing polygon (`IsTransMeridian` gating its internal lon±360 `SplitGeometry`), so no caller-side hemisphere split exists; an unsupported collection member, invalid/uncovered cell set, or mixed resolution refuses without partial indexing. Egress derives the payload's DEMANDED capability set from the values themselves and diffs it against the spec's effective set, so one refusal names every capability the wire drops.
- Packages: NetTopologySuite.IO.GeoPackage (`GeoPackageGeoReader`/`GeoPackageGeoWriter`/`GeoPackageBinaryHeader`), NetTopologySuite.IO.GeoJSON4STJ (`GeoJsonConverterFactory` via `GeoJsonProjection.Default.Factory`, `IPartiallyDeserializedAttributesTable.TryDeserializeJsonObject<T>`, `NetTopologySuite.Features.Feature`/`FeatureCollection`/`AttributesTable`), NetTopologySuite (`WKBReader`/`WKBWriter`/`WKTReader`/`WKTWriter`/`NtsGeometryServices`/`GeometryFactory.CreateGeometryCollection`/`Geometry.IsValid`/`PrecisionModel`/`Ordinates`), pocketken.H3 (`H3Index.FromPoint`, `Geometry.Fill` — antimeridian split internal, `LineString.Fill`, `H3Index.Invalid`, the `ulong` durable form), Microsoft.Data.Sqlite (the GeoPackage container spine read — already admitted), Riok.Mapperly (the GeoJSON feature correspondence), Rasm (`Rasm/Domain/validation#CAPABILITY` `ICapability`/`CapabilitySet`/`CapabilityLaw`, `Rasm/Domain/results#FAULT_BAND` `FaultBand`), Rasm.Persistence (`Element/codec` `ContentAddress`/`GeoJsonProjection`, `Element/identity` `H3Cell`, `Element/graph#STORE_HOOKS` `ProjectionContext`, `Ingest/tabular#TABULAR_SOURCE` `Origin`), LanguageExt.Core, Thinktecture.Runtime.Extensions, Thinktecture.Runtime.Extensions.Json, NodaTime, NodaTime.Serialization.SystemTextJson, BCL inbox.
- Growth: a new wire projection is one `GeoFormat` row with its capability set and its codec arms in the format `Switch` (broken loudly at compile time); a new carriable trait is one `GeoCapability` row with the memberships that hold it and, where a corner is illegal, one `Law` row; a new CRS stance is one `CrsPolicy` value (DATA, zero code); a new ordinate posture is one `GeoAdmission` value on the spec; a new fault class is one case inside the registry decade; zero new surface — a per-codec `GeometryFactory`, a `bool` capability column beside the set, a raw-WKB read of a GPB blob (the header is unparseable to a raw reader), a `RepairRings`-on row beside content addressing, a WKT `string.Split` parse, a hand-spelled GeoJSON shaper, an unframed plural byte egress, a second H3 coordinate model, or a geo→element map inside this codec is the deleted form.
- Boundary: NTS `Geometry` is the SINGLE interior vocabulary and a store-to-feed flow is decode-blob → interior → encode-text, never a direct transcode; WKB is the canonical interchange binary — the content key hashes the WKB bytes, so identity is storage-codec-independent; the GeoJSON id convention rides the ONE `GeoJsonProjection` row (two partner id conventions are two projection rows on two options instances, never post-read patching); precision is admission-side (the reader's `PrecisionModel` applies as coordinates parse; writers emit stored doubles raw), so emitted-text hash stability comes from constructing under the fixed factory BEFORE serialization; XYM/XYZM degrade silently on the GeoJSON text wire, so measure-bearing data routes through the `Measures` capability the format row holds or withholds; a stream source reaches only a format holding `Streamable`, so the container legs never re-gate it; `→ Element/identity#ELEMENT_IDENTITY` (cell derivation — the `H3Cell`/`Cell(Envelope,int)` owner, leg-3→leg-1 downward), `← Element/codec#CODEC_AXIS` (converter graph), `→ Rasm.Element` (row shape only), `← Rasm.Bim/Semantics/feature` (feature ingress over the `GeoWire` wire — the in-branch pair both `[03]-[CONTRACTS]` maps register whole); the GDAL/OGR GeoParquet COLUMNAR lane is `Query/columnar`'s — this page owns feature-file codecs, never a columnar reader.

```csharp
using Rasm.Domain;
using Rasm.Persistence.Element;

// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeoCapability : ICapability<GeoCapability> {
    public static readonly GeoCapability Properties = new("properties");
    public static readonly GeoCapability Measures = new("measures");
    public static readonly GeoCapability Layers = new("layers");
    public static readonly GeoCapability CrsFree = new("crs-free");
    public static readonly GeoCapability Streamable = new("streamable");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GeoFormat {
    public static readonly GeoFormat GeoPackage = new("gpkg", CapabilitySet<GeoCapability>.Of(
        GeoCapability.Properties, GeoCapability.Measures, GeoCapability.Layers, GeoCapability.CrsFree));
    public static readonly GeoFormat GeoJson = new("geojson", CapabilitySet<GeoCapability>.Of(
        GeoCapability.Properties, GeoCapability.Streamable));
    public static readonly GeoFormat Wkb = new("wkb", CapabilitySet<GeoCapability>.Of(
        GeoCapability.Measures, GeoCapability.CrsFree, GeoCapability.Streamable));
    public static readonly GeoFormat Wkt = new("wkt", CapabilitySet<GeoCapability>.Of(
        GeoCapability.Measures, GeoCapability.CrsFree, GeoCapability.Streamable));

    public static readonly CapabilityLaw<GeoCapability> Law = CapabilityLaw<GeoCapability>.Forbidden(
        Seq(CapabilitySet<GeoCapability>.Of(GeoCapability.Layers, GeoCapability.Streamable)));

    public CapabilitySet<GeoCapability> Capabilities { get; }
    private GeoFormat(string key, CapabilitySet<GeoCapability> capabilities) : this(key) => Capabilities = capabilities;
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
}

// --- [MODELS] --------------------------------------------------------------------------

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
    public GeoWire Wire => new(this);

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

    public CapabilitySet<GeoCapability> Capabilities => Effective(Format, Admission);

    static CapabilitySet<GeoCapability> Effective(GeoFormat format, GeoAdmission admission) =>
        (admission.Cap & Ordinates.M) != 0 ? format.Capabilities : format.Capabilities.Without(GeoCapability.Measures);

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref GeoFormat format, ref Origin source, ref CrsPolicy crs,
        ref GeoAdmission admission, ref int cellResolution, ref Option<string> layer) {
        CapabilitySet<GeoCapability> held = Effective(format, admission);
        Seq<string> broken = Seq<(bool Broken, string Token)>(
            (source is Origin.FromPath { Path: string path } && string.IsNullOrWhiteSpace(path), "path"),
            (cellResolution is < 0 or > 15, "resolution"),
            (!crs.Admissible.Contains(crs.Canonical) || admission.Factory.SRID != crs.Canonical, "crs-factory"),
            (crs.Canonical != 4326 && !held.Admits(GeoCapability.CrsFree), "crs-pinned"),
            (layer.Map(string.IsNullOrWhiteSpace).IfNone(false), "layer-blank"),
            (layer.IsSome && !held.Admits(GeoCapability.Layers), "layer-unsupported"),
            (source is Origin.FromStream && !held.Admits(GeoCapability.Streamable), "stream"),
            (!held.Admits(GeoCapability.Measures) && (admission.Cap & Ordinates.M) != 0, "measure-loss"),
            (GeoFormat.Law.Admit(held).IsFail, "capability-corner"))
            .Filter(static rule => rule.Broken).Map(static rule => rule.Token);
        if (!broken.IsEmpty) { validationError = ValidationError.Create($"<geo-spec:{string.Join(',', broken)}>"); }
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeoProperties {
    private GeoProperties() { }
    public sealed record Deferred(IPartiallyDeserializedAttributesTable Table) : GeoProperties;
    public sealed record Columns(HashMap<string, object?> Bag) : GeoProperties;
    public sealed record Bare : GeoProperties;
}

public readonly record struct GeoDecoded(Geometry Shape, GeoProperties Properties);

public readonly record struct GeoFeatureRow(Geometry Shape, ReadOnlyMemory<byte> Wkb, ContentAddress Content, Seq<H3Cell> Cells, GeoProperties Properties) {
    public static Validation<Error, GeoFeatureRow> Of(GeoSpec spec, GeoDecoded feature) =>
        Error.New(spec.Format.Message, spec.Format).Bind(indexed =>
            AdmissionSlots.Accumulate(Seq(
                AdmissionSlots.Gate(spec.Crs.Admits(feature.Shape.SRID), new GeoIngestFault.CrsUnsupported(feature.Shape.SRID)),
                AdmissionSlots.Gate(feature.Shape.IsValid, new GeoIngestFault.GeometryInvalid(feature.Shape.GeometryType)),
                AdmissionSlots.Gate(indexed.SRID == 4326, new GeoIngestFault.CrsUnsupported(indexed.SRID)),
                AdmissionSlots.Gate(GeoCells.Shaped(indexed), new GeoIngestFault.GeometryInvalid($"<h3-unsupported:{indexed.GeometryType}>"))))
            .Bind(_ => Bucketed(spec, feature, indexed)));

    static Validation<Error, GeoFeatureRow> Bucketed(GeoSpec spec, GeoDecoded feature, Geometry indexed) {
        Seq<H3Index> cells = GeoCells.Of(indexed, spec.CellResolution);
        return AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(indexed.IsEmpty || !cells.IsEmpty, new GeoIngestFault.GeometryInvalid($"<h3-uncovered:{indexed.GeometryType}>")),
            AdmissionSlots.Gate(cells.AreOfSameResolution(), new GeoIngestFault.GeometryInvalid("<mixed-cell-resolution>"))))
        .Bind(_ => Error.New(spec.Format.Message, spec.Format)
            .Map(wkb => new GeoFeatureRow(feature.Shape, wkb, ContentAddress.Of(wkb.AsSpan()), cells.Map(H3Cell.Of), feature.Properties)));
    }
}

public readonly record struct GeoPayload(Geometry Shape, HashMap<string, object?> Properties);

public readonly record struct GeoInsert(GeoPayload Feature, Seq<string> Columns);

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

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GeoIngestFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.GeoIngest;
    private GeoIngestFault() { }
    [FaultCase(0)]
    public sealed partial record PayloadRejected(string Format, string Detail) : GeoIngestFault();
    [FaultCase(1)]
    public sealed partial record CodecReject(string Format, Error Cause) : GeoIngestFault(), ICausedFault;
    [FaultCase(2)]
    public sealed partial record CrsUnsupported(int Srid) : GeoIngestFault();
    [FaultCase(3)]
    public sealed partial record CrsMismatch(int Registered, int Payload) : GeoIngestFault();
    [FaultCase(4)]
    public sealed partial record GeometryInvalid(string Reason) : GeoIngestFault();
    [FaultCase(5)]
    public sealed partial record CapabilityLoss(string Format, CapabilitySet<GeoCapability> Missing) : GeoIngestFault();
    [FaultCase(6)]
    public sealed partial record LayerMissing(string Layer) : GeoIngestFault();
    [FaultCase(7)]
    public sealed partial record ColumnUnknown(string Layer, Seq<string> Columns) : GeoIngestFault();


    public override string Message => Switch(
        payloadRejected: static c => $"<geo-codec-reject:{c.Format}:{c.Detail}>",
        codecReject:     static c => $"<geo-codec-reject:{c.Format}:{c.Cause.Message}>",
        crsUnsupported:  static c => $"<geo-crs-unsupported:{c.Srid}>",
        crsMismatch:     static c => $"<geo-crs-mismatch:{c.Registered}:{c.Payload}>",
        geometryInvalid: static c => $"<geo-geometry-invalid:{c.Reason}>",
        capabilityLoss:  static c => $"<geo-capability-loss:{c.Format}:{c.Missing.Wire}>",
        layerMissing:    static c => $"<geo-layer-missing:{c.Layer}>",
        columnUnknown:   static c => $"<geo-column-unknown:{c.Layer}:{string.Join(',', c.Columns)}>");

    public static Error Lift(GeoFormat format, Error boundary) => boundary switch {
        Fault => boundary,
        { Exception.Case: JsonException or SqliteException or NetTopologySuite.IO.ParseException } =>
            new CodecReject(format.Key, boundary),
        _ => boundary,
    };
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class GeoSource {
    static readonly Seq<(GeoCapability Capability, Func<GeoPayload, bool> Evidence)> Demands = Seq(
        (GeoCapability.Properties, (Func<GeoPayload, bool>)(static f => !f.Properties.IsEmpty)),
        (GeoCapability.Measures, static f => f.Shape.Coordinates.Any(static c => !double.IsNaN(c.M))));

    public static IO<Validation<Error, GeoYield>> Run(GeoOp op, ProjectionContext frame) =>
        op.Switch(
            frame,
            ingest: static (f, i) => Ingested(i.Spec),
            egress: static (f, e) => Emitted(e.Spec, e.Features, f),
            probe:  static (f, p) => Probed(p.Spec));

    static IO<Validation<Error, GeoYield>> Ingested(GeoSpec spec) => IO.lift(() =>
        Decoded(spec).Bind(features => features.Traverse(feature => GeoFeatureRow.Of(spec, feature)).As())
            .Map(static rows => (GeoYield)new GeoYield.Features(rows)));

    static Validation<Error, Seq<GeoDecoded>> Decoded(GeoSpec spec) => spec.Format.Switch(
        spec,
        geoPackage: static s => GeoContainer.Features(s),
        geoJson:    static s => s.Admission.Wire.Features(s),
        wkb:        static s => Capture(s.Format, () => s.Admission.Expanded(s.Admission.WkbIn.Read(Bytes(s.Source)))).Map(Bared),
        wkt:        static s => Capture(s.Format, () => s.Admission.Expanded(s.Admission.WktIn.Read(Text(s.Source)))).Map(Bared));

    static Seq<GeoDecoded> Bared(Seq<Geometry> shapes) =>
        shapes.Map(static shape => new GeoDecoded(shape, new GeoProperties.Bare()));

    static IO<Validation<Error, GeoYield>> Emitted(GeoSpec spec, Seq<GeoPayload> features, ProjectionContext frame) =>
        from at in IO.lift(frame.Now)
        from done in IO.lift(() => Payload(spec, features).Bind(admitted => Written(spec, admitted, at)))
        select done.Map(_ => (GeoYield)new GeoYield.Written(features.Count));

    static Validation<Error, Seq<GeoPayload>> Payload(GeoSpec spec, Seq<GeoPayload> features) =>
        (features.Traverse(feature => Feature(spec, feature)).As(), Capable(spec, features))
            .Apply(static (admitted, _) => admitted).As();

    static Validation<Error, GeoPayload> Feature(GeoSpec spec, GeoPayload feature) =>
        AdmissionSlots.Accumulate(Seq(
            AdmissionSlots.Gate(spec.Crs.Admits(feature.Shape.SRID), new GeoIngestFault.CrsUnsupported(feature.Shape.SRID)),
            AdmissionSlots.Gate(feature.Shape.IsValid, new GeoIngestFault.GeometryInvalid(feature.Shape.GeometryType))))
        .Map(_ => feature);

    static Validation<Error, Unit> Capable(GeoSpec spec, Seq<GeoPayload> features) {
        CapabilitySet<GeoCapability> demanded = CapabilitySet<GeoCapability>.Of(
            [.. Demands.Filter(row => features.Exists(row.Evidence)).Map(static row => row.Capability)]);
        return spec.Capabilities.AdmitsAll(demanded)
            ? unit
            : new GeoIngestFault.CapabilityLoss(spec.Format.Key, spec.Capabilities.Missing(demanded));
    }

    static Validation<Error, Unit> Written(GeoSpec spec, Seq<GeoPayload> features, Instant at) => spec.Format.Switch(
        (spec, features, at),
        geoPackage: static s => GeoContainer.Write(s.spec, s.features, s.at),
        geoJson:    static s => s.spec.Admission.Wire.Write(s.spec, s.features),
        wkb:        static s => Capture(s.spec.Format, () => Binary(s.spec, s.spec.Admission.WkbOut.Write(s.spec.Admission.Collected(s.features)))),
        wkt:        static s => Capture(s.spec.Format, () => Binary(s.spec, Encoding.UTF8.GetBytes(s.spec.Admission.WktOut.Write(s.spec.Admission.Collected(s.features))))));

    static IO<Validation<Error, GeoYield>> Probed(GeoSpec spec) =>
        IO.lift(() => spec.Format.Switch(
            spec,
            geoPackage: static s => GeoContainer.Spine(s),
            geoJson:    static s => s.Admission.Wire.Census(s),
            wkb:        static s => Capture(s.Format, () => s.Admission.WkbIn.Read(Bytes(s.Source))).Map(shape => Layer("wkb", shape, s.Admission.Cap)),
            wkt:        static s => Capture(s.Format, () => s.Admission.WktIn.Read(Text(s.Source))).Map(shape => Layer("wkt", shape, s.Admission.Cap)))
            .Map(static rows => (GeoYield)new GeoYield.Roster(rows)));

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

    internal static Validation<Error, TValue> Capture<TValue>(GeoFormat format, Func<TValue> codec) =>
        Try.lift(() => Fin.Succ(codec())).Run().Bind(static inner => inner).MapFail(e => GeoIngestFault.Lift(format, e)).ToValidation();
}

public static class GeoCells {
    public static Seq<H3Index> Of(Geometry shape, int resolution) => shape switch {
        { IsEmpty: true } => Seq<H3Index>(),
        Point point => Cell(H3Index.FromPoint(point, resolution)),
        MultiPoint points => Parts(points).Choose(part => part is Point point
            ? Some(H3Index.FromPoint(point, resolution))
            : None).Filter(static cell => cell != H3Index.Invalid),
        LineString line => toSeq(line.Fill(resolution)).Filter(static c => c != H3Index.Invalid),
        MultiLineString lines => Parts(lines).Bind(part => part is LineString line
            ? toSeq(line.Fill(resolution))
            : Seq<H3Index>()).Filter(static cell => cell != H3Index.Invalid).Distinct(),
        Polygon or MultiPolygon => toSeq(shape.Fill(resolution)).Filter(static c => c != H3Index.Invalid),
        GeometryCollection collection => Parts(collection).Bind(part => Of(part, resolution)).Distinct(),
        _ => Seq<H3Index>(),
    };

    public static bool Shaped(Geometry shape) => shape switch {
        { IsEmpty: true } => true,
        Point or LineString or Polygon => true,
        MultiPoint or MultiLineString or MultiPolygon or GeometryCollection => Parts(shape).ForAll(Shaped),
        _ => false,
    };

    internal static Seq<Geometry> Parts(Geometry collection) =>
        toSeq(Enumerable.Range(0, collection.NumGeometries)).Map(collection.GetGeometryN);

    static Seq<H3Index> Cell(H3Index cell) => cell == H3Index.Invalid ? Seq<H3Index>() : Seq(cell);
}
```

| [INDEX] | [POLICY]            | [VALUE]                                       | [BINDING]                                                         |
| :-----: | :------------------ | :-------------------------------------------- | :---------------------------------------------------------------- |
|  [01]   | one geo owner       | `GeoSource.Run` over `GeoOp`                  | ingest/egress/probe are cases of ONE dispatch                     |
|  [02]   | one factory         | `GeoAdmission` off `GeoJsonProjection`        | four codecs, one precision grid; readers factory-bound            |
|  [03]   | wire capability     | `CapabilitySet<GeoCapability>` per format row | one column, one `Law`; a bool product cannot state the corner     |
|  [04]   | interior vocabulary | NTS `Geometry` only                           | decode → interior → encode; no direct transcode, no DTO fork      |
|  [05]   | canonical bytes     | EWKB via `WKBWriter(handleSRID: true)`        | `ContentAddress.Of(wkb)` key; codec-independent                   |
|  [06]   | CRS gate            | `CrsPolicy` set membership                    | GPB `SrsId`/EWKB SRID/spine `srs_id`; GeoJSON fixed WGS84         |
|  [07]   | validity gate       | strict parse + `Geometry.IsValid`             | `RepairRings` off — byte identity and repair are exclusive        |
|  [08]   | one carrier         | `Validation<Error, …>` everywhere             | `Capture` funnels SDK throws; no authored refusal throws          |
|  [09]   | fault accumulation  | applicative accumulation + `Error.Many`       | feature, container, or batch reports EVERY defect, not the first  |
|  [10]   | plural binary wire  | `Collected` → one `GeometryCollection`        | egress arity is the value's shape; concatenation deleted          |
|  [11]   | H3 buckets          | `FromPoint`/`Fill` at spec resolution         | `h3-pg` bit parity; `Invalid` and empty shapes contribute nothing |
|  [12]   | fault band          | `[FaultCase]` ordinals on `Fault`             | `8440`-`8447`; contiguous case-grain identity                     |
|  [13]   | element projection  | per-app geo→element map                       | `[03]-[CONTRACTS]` `Ingest → Rasm.Element`; Bim consumes features |

## [03]-[FEATURE_ROWS]

- Owner: `GeoFeatureRow` the one feature currency (`Shape` + canonical `Wkb` + `Content` key + `Cells` + `Properties`) with its applicative `Of` admission; `GeoProperties` the closed deferred-properties family with its one `Bind<T>` reify; `GeoWire` the GeoJSON mapper — a `[Mapper]` bound to the read's own `GeoAdmission`, owning the generated feature correspondence, the open-resolver `Options` carrying the shared `GeoJsonProjection.Default.Factory` row, and the three composed STJ legs; `GeoContainer` the GeoPackage container adapter over the admitted `Microsoft.Data.Sqlite` — the three-table metadata spine (`gpkg_contents`/`gpkg_geometry_columns`) binding each feature table to exactly one geometry column and SRID.
- Cases: `GeoProperties.Deferred` holds the GeoJSON element-backed table — reified typed ONLY through `TryDeserializeJsonObject<T>(GeoWire.Options, out …)` so a feature's geometry and its typed properties resolve under ONE geometry converter row (a false return is absence, never a throw); `GeoProperties.Columns` holds the GeoPackage attribute-column bag — bound through the same STJ wire round-trip tabular cells mint through; `GeoProperties.Bare` is the Wkb/Wkt geometry-only row.
- Entry: `GeoRows` `extension(GeoFeatureRow row)` member `public Validation<Error, Option<T>> Bind<T>()` dispatches the properties union through the typed codec path; loose `IAttributesTable` walks are rejected.
- Auto: container reads derive layer name, geometry column/type, SRID, Z/M rules, spatial-index presence, and count from `gpkg_geometry_columns`, `gpkg_contents`, and `sqlite_master` — the spine reader CLOSES before the per-layer probes run, and each layer mints once with its two ordinate-rule admissions and its count accumulated. Selected-layer absence reaches `LayerMissing`; a `z`/`m` column no rule names reaches `CodecReject`; GPB/header-to-spine disagreement reaches `CrsMismatch` per row, accumulated with the header's own SRID admission. Egress ADMITS before it mutates: every feature's SRID and every property column resolve against the registered schema first, so the transaction opens over admitted material and a refusal costs one rollback rather than a partial write; the write then binds typed `SqliteParameter` values, quotes every identifier, and writes rows, an extant R-tree, and the `gpkg_contents` extent in one immediate transaction stamped from `ProjectionContext`.
- Packages: covered by `[02]`.
- Growth: a new properties source is one `GeoProperties` case with one `Bind<T>` arm (compile-broken); a new spine gate is one lift inside the layer mint; zero new surface — a second reify path beside `Bind<T>`, a per-format row type, a hand-built `Feature`/`AttributesTable` beside the mapper, a geometry-only attributed write, or a raw-WKB read of a GPB column is the deleted form.
- Boundary: GPB headers own payload SRID and must equal the registered spine SRID; `HandleSRID` stamps the admitted value onto geometry. `GeoWire` absorbs a null GeoJSON geometry into the empty collection under the one factory, so properties survive without an interior null, while a null DOCUMENT refuses typed rather than reading as an empty collection. `Store/provisioning#EMBEDDED_FLOOR` owns database lifecycle; this page mounts an existing `.gpkg` read-only for ingest/probe or read-write for an attributed layer transaction. `data-interchange`'s reader carve keeps the ordinal `SqliteDataReader` read hand-bound — a reader is not a mappable source — while the GeoJSON feature model, an object pair, generates.

```csharp
// --- [OPERATIONS] ----------------------------------------------------------------------

public static class GeoRows {
    extension(GeoFeatureRow row) {
        public Validation<Error, Option<T>> Bind<T>() => row.Properties.Switch(
            deferred: static d => Error.New(GeoFormat.GeoJson.Message, GeoFormat.GeoJson),
            columns: static c => Error.New(GeoFormat.GeoPackage.Message, GeoFormat.GeoPackage),
            bare: static _ => (Validation<Error, Option<T>>)Option<T>.None);
    }
}

// --- [BOUNDARIES] ----------------------------------------------------------------------

[Mapper]
[MapperRequiredMapping(RequiredMappingStrategy.Both)]
public sealed partial class GeoWire(GeoAdmission admission) {
    public static readonly JsonSerializerOptions Options =
        new JsonSerializerOptions(JsonSerializerOptions.Default) {
            Converters = { new ThinktectureJsonConverterFactory(), GeoJsonProjection.Default.Factory },
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    [MapperIgnoreSource(nameof(Feature.BoundingBox))]
    [MapProperty(nameof(Feature.Geometry), nameof(GeoDecoded.Shape))]
    [MapProperty(nameof(Feature.Attributes), nameof(GeoDecoded.Properties))]
    public partial GeoDecoded Decoded(Feature feature);

    [MapperIgnoreTarget(nameof(Feature.BoundingBox))]
    [MapProperty(nameof(GeoPayload.Shape), nameof(Feature.Geometry))]
    [MapProperty(nameof(GeoPayload.Properties), nameof(Feature.Attributes))]
    public partial Feature Featured(GeoPayload payload);

    [UserMapping]
    Geometry Located(Geometry? shape) => shape ?? admission.Empty();

    [UserMapping]
    static GeoProperties Held(IAttributesTable? table) =>
        table is IPartiallyDeserializedAttributesTable deferred ? new GeoProperties.Deferred(deferred) : new GeoProperties.Bare();

    [UserMapping]
    static IAttributesTable Tabled(HashMap<string, object?> bag) =>
        new AttributesTable(bag.ToDictionary(static kv => kv.Key, static kv => kv.Value));

    public Validation<Error, Seq<GeoDecoded>> Features(GeoSpec spec) =>
        Collection(spec).Map(collection => toSeq(collection).Map(Decoded));

    public Validation<Error, Unit> Write(GeoSpec spec, Seq<GeoPayload> features) =>
        Error.New(GeoFormat.GeoJson.Message, GeoFormat.GeoJson);

    public Validation<Error, Seq<GeoLayer>> Census(GeoSpec spec) =>
        Features(spec).Map(static rows => Seq(new GeoLayer(
            "features", 4326, "geometry", nameof(FeatureCollection), GeoOrdinateRule.Optional, GeoOrdinateRule.Forbidden, false, rows.Count)));

    static Validation<Error, FeatureCollection> Collection(GeoSpec spec) =>
        Error.New(GeoFormat.GeoJson.Message, GeoFormat.GeoJson)
        .Bind(static held => held.ToValidation(
            (Error)new GeoIngestFault.PayloadRejected("geojson", "<null-document>")));
}

public static class GeoContainer {
    public static Validation<Error, Seq<GeoDecoded>> Features(GeoSpec spec) =>
        Pathed(spec, path => {
            using SqliteConnection container = new($"Data Source={path};Mode=ReadOnly");
            container.Open();
            return Selected(container, spec.Layer).Bind(layers => layers
                .Traverse(layer => Admitted(spec, layer.Srid).Bind(_ => Payloads(container, spec, layer))).As()
                .Map(static grouped => grouped.Bind(static rows => rows)));
        });

    public static Validation<Error, Seq<GeoLayer>> Spine(GeoSpec spec) =>
        Pathed(spec, path => {
            using SqliteConnection container = new($"Data Source={path};Mode=ReadOnly");
            container.Open();
            return Selected(container, spec.Layer);
        });

    public static Validation<Error, Unit> Write(GeoSpec spec, Seq<GeoPayload> features, Instant at) =>
        Pathed(spec, path => {
            using SqliteConnection container = new($"Data Source={path};Mode=ReadWrite");
            container.Open();
            return Selected(container, Some(spec.Layer.IfNone("features")))
                .Bind(roster => roster.Traverse(layer => Bound(container, spec, layer, features, at)).As())
                .Map(static _ => unit);
        });

    static Validation<Error, TValue> Pathed<TValue>(GeoSpec spec, Func<string, Validation<Error, TValue>> read) =>
        Error.New(spec.Format.Message, spec.Format)
        .Bind(static inner => inner);

    static Validation<Error, Unit> Admitted(GeoSpec spec, int srid) =>
        AdmissionSlots.Gate(spec.Crs.Admits(srid), new GeoIngestFault.CrsUnsupported(srid));

    static Validation<Error, Seq<GeoLayer>> Selected(SqliteConnection container, Option<string> selected) =>
        Roster(container).Bind(roster => selected.Match(
            Some: name => roster.Find(layer => StringComparer.Ordinal.Equals(layer.Name, name)).Match(
                Some: static layer => (Validation<Error, Seq<GeoLayer>>)Seq(layer),
                None: () => new GeoIngestFault.LayerMissing(name)),
            None: () => roster));

    static Validation<Error, Seq<GeoLayer>> Roster(SqliteConnection container) {
        List<(string Name, int Srid, string Column, string Type, long Z, long M)> declared = [];
        using (SqliteCommand spine = container.CreateCommand()) {
            spine.CommandText = """
                SELECT c.table_name, g.srs_id, g.column_name, g.geometry_type_name, g.z, g.m
                FROM gpkg_contents c JOIN gpkg_geometry_columns g ON g.table_name = c.table_name
                WHERE c.data_type = 'features'
                """;
            using SqliteDataReader reader = spine.ExecuteReader();
            while (reader.Read()) {
                declared.Add((reader.GetString(0), reader.GetInt32(1), reader.GetString(2), reader.GetString(3), reader.GetInt64(4), reader.GetInt64(5)));
            }
        }
        return toSeq(declared).Traverse(row => Minted(container, row)).As();
    }

    static Validation<Error, GeoLayer> Minted(SqliteConnection container, (string Name, int Srid, string Column, string Type, long Z, long M) row) =>
        (Ordinate(row.Name, "z", row.Z), Ordinate(row.Name, "m", row.M), Count(container, row.Name))
            .Apply((z, m, features) => new GeoLayer(
                row.Name, row.Srid, row.Column, row.Type, z, m, Exists(container, $"rtree_{row.Name}_{row.Column}"), features))
            .As();

    static Validation<Error, GeoOrdinateRule> Ordinate(string layer, string column, long wire) =>
        GeoOrdinateRule.TryGet((int)wire, out GeoOrdinateRule? rule) && rule is not null
            ? rule
            : new GeoIngestFault.PayloadRejected("gpkg", $"<ordinate-rule:{layer}.{column}:{wire}>");

    static Validation<Error, Seq<GeoDecoded>> Payloads(SqliteConnection container, GeoSpec spec, GeoLayer layer) {
        using SqliteCommand features = container.CreateCommand();
        features.CommandText = $"SELECT * FROM {Quote(layer.Name)}";
        using SqliteDataReader reader = features.ExecuteReader();
        int geometryAt = reader.GetOrdinal(layer.GeometryColumn);
        List<Validation<Error, GeoDecoded>> rows = [];
        while (reader.Read()) { rows.Add(Decoded(reader, geometryAt, spec, layer)); }
        return toSeq(rows).Traverse(static row => row).As();
    }

    static Validation<Error, GeoDecoded> Decoded(SqliteDataReader reader, int geometryAt, GeoSpec spec, GeoLayer layer) {
        byte[] payload = (byte[])reader.GetValue(geometryAt);
        HashMap<string, object?> bag = toHashMap(toSeq(Enumerable.Range(0, reader.FieldCount))
            .Filter(i => i != geometryAt)
            .Map(i => (reader.GetName(i), reader.IsDBNull(i) ? null : reader.GetValue(i))));
        return Header(spec.Format, payload).Bind(header =>
            AdmissionSlots.Accumulate(Seq(Admitted(spec, header.SrsId), Registered(layer.Srid, header.SrsId)))
                .Bind(_ => Error.New(spec.Format.Message, spec.Format)
                    .Map(shape => new GeoDecoded(shape, new GeoProperties.Columns(bag)))));
    }

    static Validation<Error, GeoPackageBinaryHeader> Header(GeoFormat format, byte[] payload) =>
        Error.New(format.Message, format);

    static Validation<Error, Unit> Registered(int registered, int payload) =>
        AdmissionSlots.Gate(registered == payload, new GeoIngestFault.CrsMismatch(registered, payload));

    static Validation<Error, Unit> Bound(SqliteConnection container, GeoSpec spec, GeoLayer layer, Seq<GeoPayload> features, Instant at) {
        FrozenSet<string> schema = Columns(container, layer.Name);
        return features.Traverse(feature => Admissible(layer, schema, feature)).As()
            .Bind(admitted => Committed(container, spec, layer, admitted, at));
    }

    static Validation<Error, GeoInsert> Admissible(GeoLayer layer, FrozenSet<string> schema, GeoPayload feature) =>
        (Registered(layer.Srid, feature.Shape.SRID), Named(layer, schema, feature))
            .Apply((_, columns) => new GeoInsert(feature, columns)).As();

    static Validation<Error, Seq<string>> Named(GeoLayer layer, FrozenSet<string> schema, GeoPayload feature) {
        Seq<string> columns = toSeq(feature.Properties.Keys);
        Seq<string> unknown = columns.Filter(column =>
            !schema.Contains(column) || StringComparer.OrdinalIgnoreCase.Equals(column, layer.GeometryColumn));
        return unknown.IsEmpty ? columns : new GeoIngestFault.ColumnUnknown(layer.Name, unknown);
    }

    static Validation<Error, Unit> Committed(SqliteConnection container, GeoSpec spec, GeoLayer layer, Seq<GeoInsert> admitted, Instant at) {
        using SqliteTransaction commit = container.BeginTransaction(deferred: false);
        return admitted.Traverse(row => Inserted(container, commit, spec, layer, row)).As().Map(bounds => {
            Envelope written = bounds.Fold(new Envelope(), static (extent, bound) => { extent.ExpandToInclude(bound); return extent; });
            if (!written.IsNull) { Stamped(container, commit, layer, written, at); }
            commit.Commit();
            return unit;
        });
    }

    static Validation<Error, Envelope> Inserted(SqliteConnection container, SqliteTransaction commit, GeoSpec spec, GeoLayer layer, GeoInsert row) {
        using SqliteCommand insert = container.CreateCommand();
        insert.Transaction = commit;
        insert.CommandText =
            $"INSERT INTO {Quote(layer.Name)} ({Quote(layer.GeometryColumn)}{string.Concat(row.Columns.Map(c => $", {Quote(c)}"))}) " +
            $"VALUES ($blob{string.Concat(row.Columns.Map(static (_, i) => $", $p{i}"))}) RETURNING rowid";
        _ = insert.Parameters.Add(new SqliteParameter("$blob", SqliteType.Blob) { Value = spec.Admission.GpkgOut.Write(row.Feature.Shape) });
        row.Columns.Iter((column, i) => insert.Parameters.Add(Parameter($"$p{i}", row.Feature.Properties[column])));
        if (insert.ExecuteScalar() is not long rowid) { return new GeoIngestFault.PayloadRejected("gpkg", "<insert-rowid>"); }
        Envelope bound = row.Feature.Shape.EnvelopeInternal;
        if (layer.Indexed) { Rtree(container, commit, layer, rowid, bound); }
        return bound;
    }

    static Unit Rtree(SqliteConnection container, SqliteTransaction commit, GeoLayer layer, long rowid, Envelope bound) {
        using SqliteCommand index = container.CreateCommand();
        index.Transaction = commit;
        index.CommandText = $"INSERT OR REPLACE INTO {Quote($"rtree_{layer.Name}_{layer.GeometryColumn}")} (id, minx, maxx, miny, maxy) VALUES ($id, $minx, $maxx, $miny, $maxy)";
        _ = index.Parameters.Add(new SqliteParameter("$id", SqliteType.Integer) { Value = rowid });
        _ = index.Parameters.Add(new SqliteParameter("$minx", SqliteType.Real) { Value = bound.MinX });
        _ = index.Parameters.Add(new SqliteParameter("$maxx", SqliteType.Real) { Value = bound.MaxX });
        _ = index.Parameters.Add(new SqliteParameter("$miny", SqliteType.Real) { Value = bound.MinY });
        _ = index.Parameters.Add(new SqliteParameter("$maxy", SqliteType.Real) { Value = bound.MaxY });
        _ = index.ExecuteNonQuery();
        return unit;
    }

    static Unit Stamped(SqliteConnection container, SqliteTransaction commit, GeoLayer layer, Envelope written, Instant at) {
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
        return unit;
    }

    static bool Exists(SqliteConnection container, string table) {
        using SqliteCommand exists = container.CreateCommand();
        exists.CommandText = "SELECT EXISTS(SELECT 1 FROM sqlite_master WHERE type = 'table' AND name = $table)";
        _ = exists.Parameters.Add(new SqliteParameter("$table", SqliteType.Text) { Value = table });
        return exists.ExecuteScalar() is 1L;
    }

    static Validation<Error, long> Count(SqliteConnection container, string table) {
        using SqliteCommand count = container.CreateCommand();
        count.CommandText = $"SELECT COUNT(*) FROM {Quote(table)}";
        return count.ExecuteScalar() is long total ? total : new GeoIngestFault.PayloadRejected("gpkg", $"<layer-count:{table}>");
    }

    static FrozenSet<string> Columns(SqliteConnection container, string table) {
        using SqliteCommand schema = container.CreateCommand();
        schema.CommandText = $"PRAGMA table_info({Quote(table)})";
        using SqliteDataReader reader = schema.ExecuteReader();
        List<string> columns = [];
        while (reader.Read()) { columns.Add(reader.GetString(1)); }
        return columns.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

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

| [INDEX] | [POLICY]            | [VALUE]                                     | [BINDING]                                                           |
| :-----: | :------------------ | :------------------------------------------ | :------------------------------------------------------------------ |
|  [01]   | deferred properties | `GeoProperties` union + one `Bind<T>`       | element-backed until projected; loose table walks rejected          |
|  [02]   | generated wire      | `[Mapper] GeoWire` over the NTS feature     | admission-bound; hand `Feature`/`AttributesTable` mints deleted     |
|  [03]   | one converter graph | `GeoJsonProjection.Default.Factory` row     | one geometry converter; `GeoWire.Options` open resolver binds POCOs |
|  [04]   | spine authority     | `gpkg_geometry_columns` + `gpkg_contents`   | one geometry column + SRID per layer; header is the SRID authority  |
|  [05]   | deep-gate faults    | `Validation` from every container leg       | a spine refusal surfaces typed and accumulated, never as a throw    |
|  [06]   | null geometry       | mapper-absorbed empty under the one factory | unlocated features keep properties; a null DOCUMENT refuses typed   |
|  [07]   | container mechanics | existing `Microsoft.Data.Sqlite` mount      | read-only ingest; immediate attributed write transaction            |
|  [08]   | admit-then-mutate   | column + SRID admission before `Begin`      | the write opens over admitted rows; a refusal costs one rollback    |
|  [09]   | layer write         | one txn: rows + columns + rtree + extent    | attributed features persist their bag; a stale extent misleads      |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

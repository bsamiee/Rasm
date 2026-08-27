# [BIM_FEATURE]

`GeoFeature` is the host-neutral geospatial ROW every admitted vector and raster source lands on and every shared projection reads: an OGC Simple-Features `NetTopologySuite` `Geometry`, its `IAttributesTable`, and its shared `ProjectedCrs`, carrying the typed `IsValidOp` verdict, the guaranteed-on-shape `Anchor`, the H3 DGGS `Cell`, the ONE `ProjNET` datum leg, and the `ToObject` lowering that mints a shared `Object` occurrence with its `Pset_SiteContext` bag through a `GraphDelta` the `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` fold composes. Host-neutrality is the binding law: NTS owns the 2D planar geometry, the kernel `Rasm` owns the 3D solid geometry, the contract owns the node vocabulary, and the three meet only at the in-process WKB/`CoordinateSequence` kernel wire and the content-keyed graph node — a RhinoCommon binding on a geospatial owner is the named contract violation.

`GeoServices` pins the ONE `NtsGeometryServices.Instance` precision/SRID root every reader resolves factories from; `GeoSchema` names the source's own identity/label/taxonomy columns and `GeoClassifier` resolves the `(source, kind, tag)` ladder onto the true IFC4.3 class the shared `Classification` carries, publishing its match as `Evidence` so a mapped row, a catch-all fallback, and a feature carrying no taxonomy column at all read as three states rather than one. `GeoCorridor` pairs an alignment identity with its centreline and `GeoCorridors` indexes that roster once per projection so the chainage stamp resolves its nearest alignment through an `STRtree` under a declared reach budget. `GeoWire` projects the row onto its two wire forms, `GeoWkb` bridges OGR and NTS, and `GeoGdal` owns the once-per-process GDAL bootstrap and the ONE acquire-use-release bracket every `/vsimem` leg on `Semantics/vector#VECTOR_FOLD` and `Semantics/raster#RASTER_INGEST` runs inside.

## [01]-[INDEX]

- [02]-[GEO_FEATURE]: `GeoFeature` the row, `GeoServices` its precision root, the kernel `Evidence<T>` carrier composed, the datum leg, the corridor roster, the H3 cover key, the source schema, and the classifier ladder.
- [03]-[GEO_BOUNDARY]: `GeoWire` the two wire projections, `GeoWkb` the OGR↔NTS bridge, `GeoGdal` the bootstrap and the one GDAL bracket every derive leg composes.

## [02]-[GEO_FEATURE]

- Owner: `GeoFeature` the host-neutral geospatial row — planar `Geometry`, `IAttributesTable`, `Option<ProjectedCrs>` source CRS, its `Anchor` the guaranteed-on-shape interior point every keying and marker leg reads, its `Cell` the H3 DGGS keyer over the 4326-reprojected anchor, its `Defect`/`Repair` the typed `IsValidOp` verdict, its `Relation`/`Reduced`/`Refined`/`Mapped` DE-9IM-matrix, precision-snap, densify and frameless-affine reads, and `ToObject` the graph-node projection; `GeoServices` the process-wide `NtsGeometryServices` root — robust `GeometryOverlay.NG`, dense `PackedCoordinateSequenceFactory`, the `Wgs84` anchor every 4326-frame leg reprojects against; `OrdinateReproject` the whole-sequence datum walk; `GeoCorridor` the alignment-and-centreline carrier and `GeoCorridors` its indexed roster; `GeoCover` the canonical-order H3 region key; `GeoSchema` the composition-supplied source column policy; `GeoClassifier` the frozen `(source-or-any, kind-or-any, tag)` → IFC-class table.
- Cases: every geospatial measurement lands on the kernel `Evidence<T>` carrier (`Rasm/Domain/validation#VERDICT_CARRIERS`, E-B14 seated up) — an `Option` collapses `Refused` and `Absent`, so a statistics scan GDAL defeated and a band nothing scanned read alike: the `FORGED_ZERO` boundary at measurement grain is WHY the three-state carrier composes here.
- Law: `GeoClassifier` NEVER faults on an unrecognized tag — `Fallback` absorbs it and the `Match` evidence records that it did, so one unmapped feature never aborts a site import while the census still states how many features the table reached; only an EMPTY geometry faults.
- Entry: `GeoServices.Configure()` seats the instance once through `Lazy`; `GeoFeature.Repair()` returns the repaired row beside the defect the fixer resolved; `Attr<T>` is the ONE typed attribute read over both table shapes; `Reproject(target, key)` composes the `Semantics/georeference#GEODETIC_TRANSFORM` leg over the raw packed ordinate store; `Cell(resolution, key)` mints the DGGS cell; `ToObject(reference, schema, source, ctx, corridors)` accumulates classification and reprojection APPLICATIVELY and lands one `GraphDelta`; `GeoCorridors.Of(corridors, reach)` indexes the roster and `Nearest(feature)` answers the alignment inside the reach budget; `GeoCover.Of(canonical, key)` admits a region key in canonical order; `GeoClassifier.Classify(feature, schema, source, key)` resolves the ladder most-specific first.
- Auto: `GeoServices.Configure` sets `NtsGeometryServices.Instance` behind `Lazy`'s own execution-and-publication gate so every reader resolves cached factories at one `PrecisionModel`/`SRID`; every 4326-frame leg (H3 cell, MVT cut, KML emit) reprojects through the ONE datum leg onto `Wgs84` before any cell mint or tile cut; `ToObject` content-keys the reprojected footprint WKB through the kernel seed-zero `ContentHash.Of` and rides the GeoJSON footprint on one `Pset_SiteContext` `PropertyValue.Text` so the cross-runtime `shapely`/`turf` peers decode it; the chainage stamp lands DIMENSIONED through the shared `MeasureValue` gate.
- Output: `GeoFeature` is the typed planar evidence a site clash or a parcel-boundary setback reads; `Defect` names WHICH invariant broke and WHERE; `GeoClass.Match` is the classification census a site import states — a mapped tag, a tag the table did not reach, or no taxonomy column at all; the projected `Object` node carries the same generic `Classification` an imported element carries, so the shared `Bake` and the `Review/validation#IDS_FACETS` audit read a site-context model with no second selection surface; its chainage rows land DIMENSIONED, so "every element between 2+400 and 3+100" and "everything within 8 m of centreline" select through the standing `Model/query#ELEMENT_SET` `ByProperty` range facet with ZERO query edits.
- Packages: `NetTopologySuite`, `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage`, `pocketken.H3`, `MaxRev.Gdal.Core`, `ProjNET`, `NodaTime`, `Rasm.Element`, `Rasm`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new attribute CLR type is one `Typed` arm over the shared `PropertyValue`/`TemporalValue` family; a new LOD posture is a `Refined`/`Simplified` tolerance argument; a new DGGS resolution or ring radius is an argument, never a member; a new site-context class mapping is one `GeoClassifier` row keyed on `(source-or-any, kind-or-any, tag)` — `AnySource`/`Any` for the agnostic mapping every landed row is, `Some(source)`/`Some(kind)` only where the ingest family or the geometry kind genuinely discriminates; a new corridor facility class is one `GeoClassifier` row and its `CorridorClasses` membership; a source naming its columns differently is one `GeoSchema` value, never a reader edit; a new chainage row is one declared static beside its `PropertyCategory.Neutral.Row`; a new probe whose absence and refusal differ is one `Evidence<T>` column; never a per-feature-kind `GeoFeature` subtype, never a parallel `Feature`/`GeoElement` node beside the shared `Object`, and never a second precision/SRID configuration beside `NtsGeometryServices.Instance`.
- Boundary: `NetTopologySuite` owns the planar Simple-Features algebra, and a hand-rolled planar intersection or a second R-tree is the deleted form; the DGGS cell algebra is `pocketken.H3`'s under the v4-canonical spellings (`FromPoint`/`Fill`/`GridDiskDistances`/`CompactCells` — the superseded `GetKRing`/`Compact` aliases rejected because the cell vocabulary must match the `h3-pg` function names one-to-one), a live mutable `H3Index` never stores (the `(ulong)` conversion is the durable form) and `H3Index.Invalid` projects to `None`, never a stored zero cell; `NtsGeometryServices.Instance` is the single precision/SRID owner configured once and a per-call factory the rejected form; validity repair enters through `GeometryFixer.Fix` before any overlay or write, and a bare `IsValid` bool at a repair site is the deleted form that discards the answer the same op already computed; the geodetic reprojection composes the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg over the shared `GeoReference` and a `NetTopologySuite`-side datum shift is the named contract violation, the from-frame minting through the shared `GeoReference.Admit` off the feature's four `ProjectedCrs` identity strings because that record's constructor is PRIVATE and its members GET-ONLY — a `GeoReference.Identity with { Crs = … }` re-open is the unrepresentable form; the 3D solid geometry stays the kernel `Rasm`'s and a geospatial owner carrying a RhinoCommon `Brep`/`Mesh` is the host-bound defect — NTS 2D planar geometry crosses to the kernel ONLY as a `CoordinateSequence` ordinate buffer (or its WKB form) the kernel constrained-Delaunay pass triangulates into the content-keyed geometry the `Object` node references, distinct from the cross-runtime GeoJSON peer wire; the site-context projection mints a shared `Object` node and a parallel `GeoElement`/`SiteElement` record beside it is the deleted form; the `FootPrint` token is declared ONCE and read at BOTH the bag row and the `Representations` slot, and the `Pset_SiteContext` set name is one declared static, its rows minting through the owner-blessed `PropertyCategory.Neutral.Row` EMPTY-prefix producer category because the set ROUND-TRIPS to IFC as a Pset and a producer prefix lands inside the emitted property name — the same custody that keeps the chainage vocabulary declared here rather than read off the Bim-prefixed `Model/spatial#LINEAR_POSITIONING` `PositioningRows` roster; the chainage stamp is `Semantics/model#GEO_ALGEBRA` `Along`'s OWN answer and a second linear-referencing pass at the projection is the deleted form, `SegmentIndex`/`SegmentFraction` are the DURABLE identity while `Station`/`Offset` are DERIVED reads the stamp refreshes, and both measures land through the shared `MeasureValue` gate because the query algebra's range restriction decides over a dimensioned `Measure`; the classification ladder is a frozen data table keyed on `(source-or-any, kind-or-any, tag)`, never enumerated `switch` arms, and a per-kind or per-source copy of an agnostic mapping is the deleted form; the source's attribute COLUMN NAMES are the composition-supplied `GeoSchema` value — the identity column the projection stamps onto `ExternalId`/`Tag` IS the column `Semantics/model#TILE_PYRAMID` `TilePolicy.IdAttributeName` reads, so two independent literals naming one column is the deleted form; ingested `IfcClass`/`PredefinedType` tokens admit BARE and prove at the `Emit` gate per `[PREDEFINED_TOKEN_RULING]`, so the classifier carries the true IFC4.3 entity-type string and never an `IfcClass` row.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using System.Text.Json;
using H3;
using H3.Algorithms;
using H3.Extensions;
using LanguageExt;
using NetTopologySuite;
using NetTopologySuite.Densify;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Converters;
using NetTopologySuite.Operation.Valid;
using NetTopologySuite.Precision;
using NetTopologySuite.Simplify;
using NodaTime;
using Rasm;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Geospatial;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using OgcDimension = NetTopologySuite.Geometries.Dimension;

namespace Rasm.Bim;

// --- [TYPES] ---------------------------------------------------------------------------

public sealed record GeoSchema(string Identity, string Label, Seq<string> Tags) {
    public static readonly GeoSchema Default = new("id", "name", Seq("type", "class"));
}

public sealed record GeoClass(string Class, string Predefined, Evidence<string> Match);

// --- [SERVICES] ------------------------------------------------------------------------
public static class GeoServices {
    static readonly Lazy<NtsGeometryServices> Root = new(static () => {
        NtsGeometryServices.Instance = new NtsGeometryServices(
            PackedCoordinateSequenceFactory.DoubleFactory,
            new PrecisionModel(PrecisionModels.Floating),
            Srid,
            GeometryOverlay.NG,
            new CoordinateEqualityComparer());
        return NtsGeometryServices.Instance;
    });

    public const int Srid = 4326;

    public static NtsGeometryServices Configure() => Root.Value;

    public static GeometryFactory Factory => Configure().CreateGeometryFactory(Srid);

    public static readonly Fin<GeoReference> Wgs84 = GeoReference.Admit(
        0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 1.0, "WGS84", "", "EPSG:4326", "", "", "");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record GeoFeature(
    Geometry Geometry,
    IAttributesTable Attributes,
    Option<ProjectedCrs> SourceCrs) {
    public OgcGeometryType Kind => Geometry.OgcGeometryType;
    public Envelope Bounds => Geometry.EnvelopeInternal;

    public Option<TopologyValidationError> Defect =>
        new IsValidOp(Geometry).ValidationError is { } error ? Some(error) : None;

    public (GeoFeature Feature, Option<TopologyValidationError> Defect) Repair() =>
        Defect.Match(
            Some: defect => (this with { Geometry = GeometryFixer.Fix(Geometry) }, Some(defect)),
            None: () => (this, Option<TopologyValidationError>.None));

    public Option<object> Attr(string name) => Attributes.Exists(name) ? Optional(Attributes[name]) : None;

    public Option<T> Attr<T>(string name) =>
        Attributes is IPartiallyDeserializedAttributesTable lazyTable
            ? lazyTable.TryGetJsonObjectPropertyValue<T>(name, GeoWire.Json, out T? pulled) ? Optional(pulled) : None
            : Attr(name).Bind(static value => value is T typed ? Some(typed) : None);

    public Option<string> Text(string name) =>
        Attr(name).Bind(static value => value?.ToString() is { Length: > 0 } text ? Some(text) : None);

    public GeoFeature Refined(double maxSegment) => this with { Geometry = Densifier.Densify(Geometry, maxSegment) };

    public GeoFeature Simplified(double tolerance) => this with { Geometry = TopologyPreservingSimplifier.Simplify(Geometry, tolerance) };

    public Point Anchor => Geometry.InteriorPoint;

    public IntersectionMatrix Relation(Geometry probe) => Geometry.Relate(probe);

    public GeoFeature Reduced(PrecisionModel grid) =>
        this with { Geometry = GeometryPrecisionReducer.ReduceKeepCollapsed(Geometry, grid) };

    public GeoFeature Mapped(AffineTransformation map) => this with { Geometry = map.Transform(Geometry) };

    public Fin<Option<ulong>> Cell(int resolution) =>
        GeoServices.Wgs84.Bind(frame => Reproject(frame, key)).Map(wgs => {
            H3Index cell = H3Index.FromPoint(wgs.Anchor, resolution);
            return cell.IsValidCell ? Some((ulong)cell) : Option<ulong>.None;
        });

    // --- [SITE_CONTEXT_ROWS]
    internal const string FootPrint = nameof(FootPrint);
    internal const string SiteContextSet = "Pset_SiteContext";
    static readonly PropertyName FootPrintRow = PropertyCategory.Neutral.Row(FootPrint);
    static readonly PropertyName ClassifiedRow = PropertyCategory.Neutral.Row("Classified");

    // --- [CORRIDOR_ROWS]
    internal const string Alignment = nameof(Alignment);
    internal const string Station = nameof(Station);
    internal const string Offset = nameof(Offset);
    internal const string SegmentIndex = nameof(SegmentIndex);
    internal const string SegmentFraction = nameof(SegmentFraction);
    static readonly PropertyName AlignmentRow = PropertyCategory.Neutral.Row(Alignment);
    static readonly PropertyName StationRow = PropertyCategory.Neutral.Row(Station);
    static readonly PropertyName OffsetRow = PropertyCategory.Neutral.Row(Offset);
    static readonly PropertyName SegmentIndexRow = PropertyCategory.Neutral.Row(SegmentIndex);
    static readonly PropertyName SegmentFractionRow = PropertyCategory.Neutral.Row(SegmentFraction);

    public Fin<GraphDelta> ToObject(GeoReference reference, GeoSchema schema, Option<GeoVectorSource> source, ProjectionContext ctx, GeoCorridors corridors) =>
        ((
                (GeoClassifier.Classify(this, schema, source, ctx.Key)).ToValidation(),
                (Reproject(reference, ctx.Key)).ToValidation())
            .Apply(static (row, footprint) => (Row: row, Footprint: footprint)).As()).ToFin()
        .Bind(admitted => Classification.Of(ClassificationSystem.IfcSystem.Key, admitted.Row.Class, ctx.Key)
        .Bind(classification => admitted.Footprint.Chainage(corridors, ctx.Key).Map(chainage => {
            NodeId objectId = NodeId.Of(new NodeSeed.Placement());
            Map<PropertyName, PropertyValue> attributes = admitted.Footprint.Attributes.GetNames().AsIterable()
                .Filter(static name => name.Length > 0)
                .Fold(Map<PropertyName, PropertyValue>(), (bag, name) =>
                    bag.AddOrUpdate(PropertyName.Create(name), Typed(admitted.Footprint.Attributes[name])))
                .AddOrUpdate(FootPrintRow, new PropertyValue.Text(GeoWire.ToGeoJson(admitted.Footprint)))
                .AddOrUpdate(ClassifiedRow, new PropertyValue.Text(admitted.Row.Match.Switch(
                    measured: static _ => "mapped", refused: static _ => "fallback", absent: static _ => "untagged")));
            Map<PropertyName, PropertyValue> values = chainage.Fold(attributes,
                static (bag, stamp) => bag.AddOrUpdate(stamp.Name, stamp.Value));
            var pset = new Node.PropertySet(NodeId.Of(new NodeSeed.Placement()), new PropertyBag(SiteContextSet, values, InheritanceMode.OccurrenceWins, EvidenceGrade.Import));
            var obj = new Node.Object(
                Id:              objectId,
                Kind:            ObjectKind.Occurrence,
                ExternalId:      admitted.Footprint.Text(schema.Identity),
                Classification:  classification,
                PredefinedType:  PredefinedType.Create(admitted.Row.Predefined),
                ObjectType:      Option<string>.None,
                Name:            admitted.Footprint.Text(schema.Label).IfNone(admitted.Row.Class),
                Tag:             admitted.Footprint.Text(schema.Identity).IfNone(""),
                Representations: RepresentationContentHash.Empty.With(FootPrint, ContentHash.Of(GeoWkb.FromNts(admitted.Footprint.Geometry))),
                History:         Option<OwnerHistory>.None,
                Span:            SchemaSpan.From(ReleaseVersion.Ifc4X3Add2));
            return GraphDelta.Empty
                .Put(obj).Put(pset)
                .Link(new Relationship.Assign(objectId, pset.Id, AssignKind.PropertyDefinition));
        })));

    Fin<Seq<(PropertyName Name, PropertyValue Value)>> Chainage(GeoCorridors corridors) =>
        corridors.Nearest(this).Match(
            Some: corridor => GeoModel.Along(corridor.Centreline, new LinearProbe.Locate(Anchor.Coordinate))
                .Bind(answer => answer is LinearAnswer.At hit
                    ? (MeasureValue.OfSi(Dimension.LengthDim, hit.Station, key), MeasureValue.OfSi(Dimension.LengthDim, hit.Offset, key))
                    .Apply((station, offset) => Seq(
                        (AlignmentRow, (PropertyValue)new PropertyValue.Text(corridor.Alignment)),
                        (StationRow, new PropertyValue.Measure(station)),
                        (OffsetRow, new PropertyValue.Measure(offset)),
                        (SegmentIndexRow, new PropertyValue.Integer(new System.Numerics.BigInteger(hit.Durable.SegmentIndex))),
                        (SegmentFractionRow, new PropertyValue.Number(hit.Durable.SegmentFraction)))).As()
                    : Fin.Succ(Seq<(PropertyName, PropertyValue)>())),
            None: static () => Fin.Succ(Seq<(PropertyName, PropertyValue)>()));

    internal static PropertyValue Typed(object? raw) => raw switch {
        null                  => new PropertyValue.Text(""),
        bool flag             => new PropertyValue.Boolean(flag),
        double d              => new PropertyValue.Number(d),
        float f               => new PropertyValue.Number(f),
        decimal m             => new PropertyValue.Number((double)m),
        sbyte or byte or short or ushort or int or uint or long or ulong
                              => new PropertyValue.Integer(new System.Numerics.BigInteger(Convert.ToDecimal(raw, CultureInfo.InvariantCulture))),
        LocalDate date        => new PropertyValue.Temporal(new TemporalValue.Date(date)),
        LocalTime clock       => new PropertyValue.Temporal(new TemporalValue.Time(clock)),
        LocalDateTime moment  => new PropertyValue.Temporal(new TemporalValue.Moment(moment)),
        Period span           => new PropertyValue.Temporal(new TemporalValue.Span(span)),
        Instant instant       => new PropertyValue.Temporal(new TemporalValue.Stamp(instant)),
        DateTimeOffset offset => new PropertyValue.Temporal(new TemporalValue.Stamp(Instant.FromDateTimeOffset(offset))),
        DateTime local        => new PropertyValue.Temporal(new TemporalValue.Moment(LocalDateTime.FromDateTime(local))),
        var other             => new PropertyValue.Text(other.ToString() ?? ""),
    };

    public Fin<GeoFeature> Reproject(GeoReference target) =>
        SourceFrame().Bind(source => {
            Geometry shifted = Geometry.Copy();
            var walk = new OrdinateReproject(source, target, key);
            shifted.Apply(walk);
            return walk.Verdict.Map(_ => {
                shifted.SRID = target.Epsg.IfNone(0);
                return this with {
                    Geometry = shifted,
                    SourceCrs = target.Epsg == Some(4326) ? Option<ProjectedCrs>.None : target.Crs,
                };
            });
        });

    Fin<GeoReference> SourceFrame() =>
        SourceCrs.Match(
            Some: crs => GeoReference.Admit(0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 1.0, "", "",
                crs.Name, crs.Wkt, crs.MapProjection, crs.MapZone, key),
            None: static () => Fin.Succ(GeoReference.Identity));

    sealed class OrdinateReproject(GeoReference source, GeoReference target) : IEntireCoordinateSequenceFilter {
        Fin<Unit> verdict = Fin.Succ(unit);
        public Fin<Unit> Verdict => verdict;
        public bool Done => verdict.IsFail;
        public bool GeometryChanged => true;

        public void Filter(CoordinateSequence seq) =>
            verdict = seq switch {
                PackedDoubleCoordinateSequence packed =>
                    GeoTransform.Reproject(source, target, packed.GetRawCoordinates().AsSpan(), stride: seq.Dimension, key),
                _ => Copied(seq),
            };

        Fin<Unit> Copied(CoordinateSequence seq) {
            int stride = seq.Dimension;
            var ordinates = new double[seq.Count * stride];
            for (int i = 0; i < seq.Count; i++) {
                for (int ordinate = 0; ordinate < stride; ordinate++) {
                    ordinates[(i * stride) + ordinate] = seq.GetOrdinate(i, ordinate);
                }
            }
            return GeoTransform.Reproject(source, target, ordinates.AsSpan(), stride: stride, key).Map(_ => {
                for (int i = 0; i < seq.Count; i++) {
                    for (int ordinate = 0; ordinate < stride; ordinate++) {
                        seq.SetOrdinate(i, ordinate, ordinates[(i * stride) + ordinate]);
                    }
                }
                return unit;
            });
        }
    }
}

public sealed record GeoCorridor(string Alignment, GeoFeature Centreline);

public sealed class GeoCorridors {
    readonly Seq<GeoCorridor> roster;
    readonly STRtree<GeoCorridor> index;
    readonly double reach;

    GeoCorridors(Seq<GeoCorridor> corridors, double budget) {
        roster = corridors;
        reach = budget;
        index = new STRtree<GeoCorridor>();
        corridors.Iter(c => index.Insert(c.Centreline.Bounds, c));
    }

    public static readonly GeoCorridors Empty = new(Seq<GeoCorridor>(), 0.0);

    public Seq<GeoCorridor> Roster => roster;

    public static GeoCorridors Of(Seq<GeoCorridor> corridors, double reach) =>
        corridors.IsEmpty ? Empty : new GeoCorridors(corridors, Math.Abs(reach));

    public Option<GeoCorridor> Nearest(GeoFeature feature) =>
        roster.IsEmpty
            ? None
            : toSeq(index.NearestNeighbour(feature.Bounds, new GeoCorridor("", feature), GeoDistance.Centrelines, 1))
                .Head
                .Filter(c => c.Centreline.Geometry.IsWithinDistance(feature.Geometry, reach));
}

public sealed record GeoCover {
    GeoCover(IReadOnlyList<H3Index> canonical) => Canonical = canonical;

    public IReadOnlyList<H3Index> Canonical { get; }

    public static Fin<GeoCover> Of(IReadOnlyList<H3Index> canonical) =>
        canonical.IsCanonicalCells()
            ? Fin.Succ(new GeoCover(canonical))
            : Fin.Fail<GeoCover>(new BimFault.Refused(BimScope.Semantics, BimReason.Rejected, string.Join(':', new object?[] { "geo-cover-rejected", "uncanonical", canonical.Count.ToString(CultureInfo.InvariantCulture) })));

    public FrozenSet<ulong> Cells => Canonical.Select(static cell => (ulong)cell).ToFrozenSet();

    public bool Contains(ulong cell) => Canonical.CanonicalCellsContain((H3Index)cell);

    public double AreaSquareMetres => Canonical.Sum(static cell => cell.CellAreaInMSquared());

    public MultiPolygon Outline => Canonical.CellsToMultiPolygon();
}

public sealed class GeoDistance<T>(Func<T, Geometry> shape) : IItemDistance<Envelope, T> {
    public double Distance(IBoundable<Envelope, T> a, IBoundable<Envelope, T> b) => shape(a.Item).Distance(shape(b.Item));
}

public static class GeoDistance {
    public static readonly GeoDistance<GeoCorridor> Centrelines = new(static c => c.Centreline.Geometry);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class GeoClassifier {
    static readonly Option<OgcGeometryType> Any = None;

    static readonly Option<GeoVectorSource> AnySource = None;

    static readonly (string Class, string Predefined) Fallback = ("IfcGeographicElement", "NOTDEFINED");

    static readonly Map<(Option<GeoVectorSource> Source, Option<OgcGeometryType> Kind, string Tag), (string Class, string Predefined)> Table =
        Map(
            ((AnySource, Any, "building"),                 ("IfcBuilding",          "NOTDEFINED")),
            ((AnySource, Any, "buildingpart"),             ("IfcBuilding",          "NOTDEFINED")),
            ((AnySource, Any, "parcel"),                   ("IfcSite",              "NOTDEFINED")),
            ((AnySource, Any, "landuse"),                  ("IfcSite",              "NOTDEFINED")),
            ((AnySource, Any, "relief"),                   ("IfcGeographicElement", "TERRAIN")),
            ((AnySource, Any, "tinrelief"),                ("IfcGeographicElement", "TERRAIN")),
            ((AnySource, Any, "contour"),                  ("IfcGeographicElement", "TERRAIN")),
            ((AnySource, Any, "road"),                     ("IfcRoad",              "NOTDEFINED")),
            ((AnySource, Any, "transportationsquare"),     ("IfcRoad",              "NOTDEFINED")),
            ((AnySource, Any, "rail"),                     ("IfcRailway",           "NOTDEFINED")),
            ((AnySource, Any, "railway"),                  ("IfcRailway",           "NOTDEFINED")),
            ((AnySource, Any, "bridge"),                   ("IfcBridge",            "NOTDEFINED")),
            ((AnySource, Any, "bridgepart"),               ("IfcBridge",            "NOTDEFINED")),
            ((AnySource, Any, "tunnel"),                   ("IfcTunnel",            "NOTDEFINED")),
            ((AnySource, Any, "tunnelpart"),               ("IfcTunnel",            "NOTDEFINED")),
            ((AnySource, Any, "tree"),                     ("IfcGeographicElement", "VEGETATION")),
            ((AnySource, Any, "vegetation"),               ("IfcGeographicElement", "VEGETATION")),
            ((AnySource, Any, "plantcover"),               ("IfcGeographicElement", "VEGETATION")),
            ((AnySource, Any, "solitaryvegetationobject"), ("IfcGeographicElement", "VEGETATION")),
            ((AnySource, Any, "waterway"),                 ("IfcMarineFacility",    "WATERWAY")),
            ((AnySource, Any, "waterbody"),                ("IfcMarineFacility",    "NOTDEFINED")),
            ((AnySource, Any, "cityfurniture"),            ("IfcGeographicElement", "NOTDEFINED")));

    internal static readonly FrozenSet<string> CorridorClasses =
        Table.Values.Map(static row => row.Class)
            .Filter(static cls => cls is "IfcRoad" or "IfcRailway" or "IfcBridge" or "IfcMarineFacility")
            .Distinct()
            .ToFrozenSet(StringComparer.Ordinal);

    public static Fin<GeoClass> Classify(GeoFeature feature, GeoSchema schema, Option<GeoVectorSource> source) {
        if (feature.Geometry.IsEmpty) {
            return Fin.Fail<GeoClass>(new BimFault.Refused(BimScope.Semantics, BimReason.Unmapped, string.Join(':', new object?[] { "geo-feature-miss", "empty", feature.Kind.ToString() })));
        }
        Option<string> tag = schema.Tags
            .Choose(column => feature.Text(column))
            .Head
            .Map(static t => t.ToLowerInvariant());
        return Fin.Succ(tag.Match(
            None: static () => new GeoClass(Fallback.Class, Fallback.Predefined, new Evidence<string>.Absent()),
            Some: read => (Table.Find((source, Some(feature.Kind), read))
                    | Table.Find((source, Any, read))
                    | Table.Find((AnySource, Some(feature.Kind), read))
                    | Table.Find((AnySource, Any, read)))
                .Match(
                    Some: row => new GeoClass(row.Class, row.Predefined, new Evidence<string>.Measured(read)),
                    None: () => new GeoClass(Fallback.Class, Fallback.Predefined,
                        new Evidence<string>.Refused(new BimFault.Refused(BimScope.Semantics, BimReason.Unmapped, string.Join(':', new object?[] { "geo-feature-miss", "tag", read })))))));
    }
}
```

## [03]-[GEO_BOUNDARY]

- Owner: `GeoWire` the `GeoFeature`'s two canonical wire projections per `docs/stacks/csharp/domain/data-interchange#GEO_INTERCHANGE` — the GeoJSON text and the GeoPackage binary blob; `GeoWkb` the ONE bidirectional OGR↔NTS bridge every GDAL leg and the GeoParquet geo-column cross; `GeoGdal` the once-per-process `MaxRev.Gdal.Core` bootstrap AND the ONE acquire-use-release bracket every `/vsimem` leg in this folder runs inside; `GdalSink` the closed sink vocabulary a derive leg writes through.
- Cases: `GdalSink` arms `Memory` (a `/vsimem` path the same process unlinks) and `Temp` (a real temp file the managed read-back recovers, because this GDAL SWIG build exposes only `VSIFWriteL(string, …)` and NO `byte[]` `VSIFReadL`).
- Law: `GdalBase.ConfigureAll()` MUST run before any `OSGeo.*` call, and a second bootstrap owner is the deleted form — every GDAL-touching page composes `GeoGdal.Bootstrap`; the `IsConfigured` read stays INSIDE the lazy factory so a foreign caller that already configured the process is not re-configured.
- Entry: `GeoGdal.Bootstrap()` seats the drivers, PROJ paths, and the thrown SWIG error model; `GeoGdal.Derive(bytes, sink, suffix, run, lane, key)` acquires the `/vsimem` source and the sink path, opens the dataset, hands `run` the open dataset and the sink path, and releases BOTH on every arm; `GeoWire.ToGeoJson`/`ToGpkgBlob`/`FromGpkgBlob` project the row; `GeoWkb.ToNts`/`FromNts`/`ToOgr` cross the bridge.
- Auto: `Derive` rides `IO.Bracket`, so release brackets the ACQUISITION rather than the outcome — the three hand `try`/`finally` pairs it replaces each spelled one acquire/release policy three times and each released only what its own body acquired.
- Output: a derive leg returns its own `Fin`, the lane naming which derivation refused; the bracket adds no value of its own because a released resource is not evidence.
- Packages: `NetTopologySuite`, `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage`, `MaxRev.Gdal.Core`, `MaxRev.Gdal.MacosRuntime.Minimal.arm64`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new sink medium is one `GdalSink` row carrying its mint and release columns; a new GDAL derivation is one `Derive` call over the existing bracket, never a fourth hand `try`/`finally`.
- Boundary: GeoJSON text and the GeoPackage blob are the ONLY two wire forms of a `GeoFeature`, and `GeoWkb` is an interior bridge that never becomes a shared wire; the writer preserves Z (`Geometry.AsBinary`'s 2D default drops a terrain footprint's elevations — the deleted second inline spelling), so no other WKB spelling exists in the folder; both codec owners hold SETTINGS only and every read/write state is call-local; a failed GDAL open lowers onto `Model/faults#FAULT_BAND` `BimFault.Refused` with `BimReason.Codec`, never `Refused/BimReason.Capability` — that band is the `Semantics/georeference#GEODETIC_TRANSFORM` leg's; a publish without the matching RID runtime faults at first call on the same band.

```csharp
// --- [BOUNDARIES] ----------------------------------------------------------------------
public static class GeoWire {
    public static readonly JsonSerializerOptions Json = Compose();

    static JsonSerializerOptions Compose() {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new GeoJsonConverterFactory(
            GeoServices.Factory, writeGeometryBBox: true, idPropertyName: "id",
            RingOrientationOption.EnforceRfc9746, allowModifyingAttributesTables: false));
        options.MakeReadOnly();
        return options;
    }

    public static string ToGeoJson(GeoFeature feature) => JsonSerializer.Serialize(feature.Geometry, Json);

    static readonly GeoPackageGeoReader BlobReader = new() { HandleSRID = true, HandleOrdinates = Ordinates.XYZ };
    static readonly GeoPackageGeoWriter BlobWriter = new() { HandleOrdinates = Ordinates.XYZ };

    public static byte[] ToGpkgBlob(GeoFeature feature) => BlobWriter.Write(feature.Geometry);
    public static Geometry FromGpkgBlob(byte[] blob) => BlobReader.Read(blob);
}

public static class GeoWkb {
    static readonly WKBReader Reader = new(GeoServices.Configure());
    static readonly WKBWriter Writer = new() { HandleOrdinates = Ordinates.XYZ };

    public static Geometry ToNts(byte[] wkb) => Reader.Read(wkb);

    public static Geometry ToNts(OSGeo.OGR.Geometry ogr) {
        var wkb = new byte[ogr.WkbSize()];
        ogr.ExportToWkb(wkb, OSGeo.OGR.wkbByteOrder.wkbNDR);
        return ToNts(wkb);
    }

    public static byte[] FromNts(Geometry geometry) => Writer.Write(geometry);

    public static OSGeo.OGR.Geometry ToOgr(Geometry geometry) => OSGeo.OGR.Geometry.CreateFromWkb(FromNts(geometry));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class GdalSink {
    public static readonly GdalSink Memory = new("memory",
        mint:    static suffix => $"/vsimem/{Guid.NewGuid():N}{suffix}",
        release: static path => { OSGeo.GDAL.Gdal.Unlink(path); return unit; });
    public static readonly GdalSink Temp = new("temp",
        mint:    static suffix => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{suffix}"),
        release: static path => { File.Delete(path); return unit; });

    [UseDelegateFromConstructor]
    public partial string Mint(string suffix);

    [UseDelegateFromConstructor]
    public partial Unit Release(string path);
}

public static class GeoGdal {
    static readonly Lazy<Unit> Boot = new(static () => {
        if (!MaxRev.Gdal.Core.GdalBase.IsConfigured) { MaxRev.Gdal.Core.GdalBase.ConfigureAll(); }
        OSGeo.GDAL.Gdal.UseExceptions();
        OSGeo.OGR.Ogr.UseExceptions();
        OSGeo.OSR.Osr.UseExceptions();
        return unit;
    });

    public static Unit Bootstrap() => Boot.Value;

    public static Fin<A> Derive<A>(
        ReadOnlyMemory<byte> bytes, GdalSink sink, string suffix,
        Func<OSGeo.GDAL.Dataset, string, Fin<A>> run, string lane) =>
        IO.lift(() => Try.lift(() => {
                Bootstrap();
                string source = GdalSink.Memory.Mint(".tif");
                OSGeo.GDAL.Gdal.FileFromMemBuffer(source, bytes.ToArray());
                return Fin.Succ((Source: source, Sink: sink.Mint(suffix)));
            }).Run().Bind(static inner => inner))
            .Bracket(
                Use: scope => IO.lift(() => Try.lift(() => {
                    using var dataset = OSGeo.GDAL.Gdal.Open(scope.Source, OSGeo.GDAL.Access.GA_ReadOnly);
                    return run(dataset, scope.Sink);
                }).Run().Bind(static inner => inner)),
                Fin: scope => IO.lift(() => Try.lift(() => {
                    GdalSink.Memory.Release(scope.Source);
                    return Fin.Succ(sink.Release(scope.Sink));
                }).Run().Bind(static inner => inner)))
            .Try().runFin.As().Run()
            ;

    public static Fin<A> Raster<A>(ReadOnlyMemory<byte> bytes, string suffix, Func<OSGeo.GDAL.Dataset, Fin<A>> run, string lane) =>
        IO.lift(() => Try.lift(() => {
                Bootstrap();
                string source = GdalSink.Memory.Mint(suffix);
                OSGeo.GDAL.Gdal.FileFromMemBuffer(source, bytes.ToArray());
                return Fin.Succ(source);
            }).Run().Bind(static inner => inner))
            .Bracket(
                Use: source => IO.lift(() => Try.lift(() => {
                    using var dataset = OSGeo.GDAL.Gdal.Open(source, OSGeo.GDAL.Access.GA_ReadOnly);
                    return run(dataset);
                }).Run().Bind(static inner => inner)),
                Fin: source => IO.lift(() => Try.lift(() => Fin.Succ(GdalSink.Memory.Release(source))).Run().Bind(static inner => inner)))
            .Try().runFin.As().Run()
            ;

    public static Fin<A> Vector<A>(ReadOnlyMemory<byte> bytes, Func<OSGeo.OGR.DataSource, Fin<A>> run, string lane) =>
        IO.lift(() => Try.lift(() => {
                Bootstrap();
                string source = GdalSink.Memory.Mint("");
                OSGeo.GDAL.Gdal.FileFromMemBuffer(source, bytes.ToArray());
                return Fin.Succ(source);
            }).Run().Bind(static inner => inner))
            .Bracket(
                Use: source => IO.lift(() => Try.lift(() => {
                    using var data = OSGeo.OGR.Ogr.Open(source, 0);
                    return run(data);
                }).Run().Bind(static inner => inner)),
                Fin: source => IO.lift(() => Try.lift(() => Fin.Succ(GdalSink.Memory.Release(source))).Run().Bind(static inner => inner)))
            .Try().runFin.As().Run()
            ;

    public static Fin<A> Author<A>(GdalSink sink, string suffix, Func<string, Fin<A>> run, string lane) =>
        IO.lift(() => Try.lift(() => { Bootstrap(); return Fin.Succ(sink.Mint(suffix)); }).Run().Bind(static inner => inner))
            .Bracket(
                Use: path => IO.lift(() => Try.lift(() => run(path)).Run().Bind(static inner => inner)),
                Fin: path => IO.lift(() => Try.lift(() => Fin.Succ(sink.Release(path))).Run().Bind(static inner => inner)))
            .Try().runFin.As().Run()
            ;
}
```

## [04]-[RESEARCH]

(none)

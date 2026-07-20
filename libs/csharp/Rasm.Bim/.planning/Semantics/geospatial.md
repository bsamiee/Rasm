# [BIM_GEOSPATIAL]

Georeferenced site context projects onto the `Rasm.Element` seam through one host-neutral `GeoFeature` carrier: `GeoVector`/`GeoRaster` ingest folds every admitted vector and raster source onto that row, and `GeoFeature.ToObject`/`GeoRaster.ToCoverage` lower it onto a seam `Object` node and a raster onto a seam `Coverage` node through a `GraphDelta` the `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` fold composes. This geospatial source registers as its own projector at the app composition root beside the IFC `Projection/semantic#SEMANTIC_PROJECTOR`, never folded BY it — that projector captures only a GeometryGym `DatabaseIfc`, never a GIS source. Host-neutrality is the binding law: NTS owns the 2D planar geometry, the kernel `Rasm` owns the 3D solid geometry, the seam owns the node vocabulary, and the three meet only at the in-process WKB/`CoordinateSequence` kernel wire and the content-keyed seam node — a RhinoCommon binding on a geospatial owner is the named seam violation.

One `GeoFeature` carries the OGC Simple-Features `NetTopologySuite` `Geometry`, its `AttributesTable`, and its seam `ProjectedCrs`; `GeoModel` holds the feature set under one `NtsGeometryServices.Instance` precision/SRID root with TWO spatial indexes — the `STRtree` envelope broad-phase and the `pocketken.H3` DGGS cell bucket, bit-for-bit the same 64-bit v4 cell the `Rasm.Persistence` `h3-pg` server index computes — over the `GeoPredicate`-parameterized DE-9IM join, the k-NN/setback/dissolve planar algebra, and the `GeoModel.ToTiles` MVT LOD pyramid the `csharp:Rasm.AppUi/Charts` Mapsui overlays consume. A vector feature RIDES an `Object` occurrence discriminated by the generic `Classification("ifc", code)` like any imported element, never a parallel `Feature`/`GeoElement` family; a raster lands a `Coverage` node (a `CoverageGrid` by-ref, bands, and the seam `GeoReference` CRS), never a stored pixel blob on the element. Vector ingest (shapefile/GeoJSON/CityJSON/FlatGeobuf/GeoParquet/KML managed codecs and the GDAL/OGR universal long-tail) and raster ingest (GeoTIFF/COG/DEM windowed `ReadRaster<T>`) are Bim's NTS/GDAL capability the projector composes; KML rides a MANAGED `SharpKml.Core` styled-presentation arm, a remote `.fgb` escalates to `PackedRTree.StreamSearch` range reads, and the OGR↔NTS bridge is one `GeoWkb` owner.

## [01]-[INDEX]

- [01]-[GEOSPATIAL_SEAM]: `GeoFeature` row, `GeoModel` spatial index, DE-9IM/k-NN/overlay algebra, MVT pyramid, and the vector→`Object` projection.
- [02]-[VECTOR_INGEST]: `GeoVector` fold admitting every managed and OGR source onto `GeoFeature`, the `GeoWkb` bridge, and remote-`.fgb` streaming.
- [03]-[RASTER_INGEST]: `GeoRaster` GDAL ingest — band read, typed schema, overview pyramid, DEM/contour vectors, and raster→`Coverage` projection.

## [02]-[GEOSPATIAL_SEAM]

- Owner: `GeoFeature` the host-neutral geospatial row — planar `Geometry`, `IAttributesTable`, `Option<ProjectedCrs>` source CRS, its `Cell` the H3 DGGS keyer over the 4326-reprojected centroid; `GeoModel` the feature set under one `NtsGeometryServices.Instance` precision/SRID root carrying the lazily-built `STRtree` broad-phase, the `GeoPredicate`-parameterized `SpatialJoin`, the k-NN/`Setback`/`Dissolve` planar algebra, the H3 `Bucket`/`Cover` coarse DGGS index, the `ToTiles` MVT LOD pyramid, and the `ToObject`-folding `Project`; `GeoPredicate` the closed DE-9IM `[SmartEnum<string>]` delegate table dispatching the `IPreparedGeometry` narrow phase; `GeoServices` the process-wide `NtsGeometryServices` root — robust `GeometryOverlay.NG`, dense `PackedCoordinateSequenceFactory`, the `Wgs84` anchor every 4326-frame leg reprojects against; `GeoTiles` the MVT byte codec + TileJSON catalog; `GeoClassifier` the frozen `(OgcGeometryType, tag)`→`(IFC class, predefined)` table carrying the true IFC4.3 class string the seam `Classification` takes, never an `IfcClass` row (the Bim `Emit` gate validates against the roster).
- Entry: `GeoModel.Of` indexes the features into the `STRtree` once; `SpatialJoin(probe, GeoPredicate)` runs the broad-then-narrow rail over the closed DE-9IM vocabulary and `SpatialJoin(probe, de9im)` its open 9-char-mask tail through `Geometry.Relate`; `Nearest` stamps each k-NN hit with its `DistanceOp.NearestPoints` clash-gap witness; `Setback` carves the buildable region from the repaired parcel minus the dissolved context; `Bucket` and `Cover` build the coarse H3 index, `Cover` returning the `FrozenSet<ulong>` region key the Persistence `h3_cell = ANY(@cells)` prefilter tests verbatim; `ToTiles` reprojects onto `Wgs84`, routes each feature to its `(zoom, layer)` slots through a per-zoom LOD simplify, and folds through `VectorTileTree.Add`; `GeoTiles.Encode`/`Decode`/`Catalog` stream the `.mvt` bytes and emit the TileJSON descriptor; `GeoFeature.ToObject` projects one feature onto a seam `Object` occurrence — a `GeoClassifier`-resolved generic `Classification`, a bare `PredefinedType`, the reprojected footprint content-keyed into `Representations` under `FootPrint` (the analytical surface a `Rasm.Compute` consumer resolves one-hop from the blob store, never an inline coordinate field), the attributes a `Pset_SiteContext` node linked by a neutral `Assign(PropertyDefinition)` edge — faulting only on an EMPTY geometry, every recognized feature classifying to the `IfcGeographicElement` catch-all rather than aborting the import; `GeoModel.Project` folds the set into one header-less `GraphDelta` the seam `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` composes.
- Auto: `GeoServices.Configure` sets `NtsGeometryServices.Instance` once behind an idempotency guard so every reader resolves cached factories at one `PrecisionModel`/`SRID`; `GeoModel.Of` is the single admission running `GeometryFixer.Fix` exactly once, so no downstream leg re-scans validity; every 4326-frame leg (H3 cell, MVT cut, KML emit) reprojects through the ONE datum leg onto `Wgs84` before any cell mint or tile cut (H3 and the WebMercator grid both take SRID-4326 input); `ToObject` content-keys the reprojected footprint WKB through the kernel seed-zero `ContentHash.Of` and rides the GeoJSON footprint on one `Pset_SiteContext` `PropertyValue.Text` so the cross-runtime `shapely`/`turf` peers decode it.
- Receipt: `GeoFeature` is the typed planar evidence a site clash or a parcel-boundary setback reads (`Setback` the composed carve, `Nearest` the k-NN gap witness); the `STRtree` broad-phase and the H3 `Bucket`/`Cover` coarse bucket key the same server-side cell, so an in-process membership test and the `h3-pg` SQL prefilter agree; the `ToTiles` `VectorTileTree` with its `Catalog` TileJSON is the `{z}/{x}/{y}.mvt` delivery the `csharp:Rasm.AppUi/Charts` Mapsui overlay fetches; the projected `Object` node carries the same generic `Classification` an imported element carries, so the seam `Bake` and the `Review/validation#IDS_FACETS` audit read a site-context model with no second selection surface; the raster `Coverage` node carries the field by-ref + bands + CRS the terrain consumer reads.
- Packages: `NetTopologySuite`, `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage`, `NetTopologySuite.IO.VectorTiles`, `NetTopologySuite.IO.VectorTiles.Mapbox`, `pocketken.H3`, `ProjNET`, `Rasm.Element`, `Rasm`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new planar predicate is one `GeoPredicate` row over the existing `IPreparedGeometry` delegate column, and an ad-hoc DE-9IM relation is a 9-char mask on the `SpatialJoin` pattern overload — never a new row per experiment; a new overlay op is one `Geometry` instance method on the existing algebra; a new LOD posture is a `Refined`/`Simplified` tolerance argument; a new spatial index is the `STRtree`/`Quadtree` swap the `GeoModel` carrier owns; a new DGGS resolution or ring radius is an argument, never a member; a new tile LOD/layer policy is one route delegate value, never a second pyramid builder; a new site-context class mapping is one `GeoClassifier` table row keyed on `(OgcGeometryType, tag)`; a new attribute projection is one `Pset_SiteContext` `PropertyValue`; never a parallel planar geometry world beside NTS, never a per-feature-kind `GeoFeature` subtype, never a parallel `Feature`/`GeoElement` node beside the seam `Object`, and never a second precision/SRID configuration beside `NtsGeometryServices.Instance`.
- Boundary: `NetTopologySuite` owns the planar Simple-Features algebra, and a hand-rolled planar intersection or a second R-tree is the deleted form; the DGGS cell algebra is `pocketken.H3`'s under the v4-canonical spellings (`FromPoint`/`Fill`/`GridDiskDistances`/`CompactCells` — the legacy `GetKRing`/`Compact` aliases rejected because the cell vocabulary must match the `h3-pg` function names one-to-one), a live mutable `H3Index` never stores (the `(ulong)` conversion is the durable form) and `H3Index.Invalid` projects to `None`, never a stored zero cell; the MVT object model and protobuf are `NetTopologySuite.IO.VectorTiles`'s — geometry enters the tile cut ALREADY 4326 (the datum leg runs before tiling, never inside the codec), the 2D `.mvt` pyramid stays orthogonal to the 3D-Tiles glTF stack, and a hand-spelled MVT protobuf is the deleted form; `NtsGeometryServices.Instance` is the single precision/SRID owner configured once and a per-call factory the rejected form; validity repair enters through `GeometryFixer.Fix` before any overlay or write; the geodetic reprojection composes the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg over the seam `GeoReference` and a `NetTopologySuite`-side datum shift is the named seam violation; the 3D solid geometry stays the kernel `Rasm`'s and a geospatial owner carrying a RhinoCommon `Brep`/`Mesh` is the host-bound defect — NTS 2D planar geometry crosses to the kernel ONLY as a `CoordinateSequence` ordinate buffer (or its WKB form) the kernel constrained-Delaunay pass triangulates into the content-keyed geometry the `Object` node references, distinct from the cross-runtime GeoJSON peer wire; the site-context projection mints a seam `Object` node and a parallel `GeoElement`/`SiteElement` record beside it is the deleted form; a raster coverage lands a seam `Coverage` node and a stored pixel blob on the element node is the deleted form; the `GeoClassifier` is a frozen data table keyed on `(OgcGeometryType, tag)`, never enumerated `switch` arms.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Data;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using CommunityToolkit.HighPerformance;
using FlatGeobuf.Index;
using GISBlox.IO.GeoParquet.Extensions;
using H3;
using H3.Algorithms;
using H3.Extensions;
using LanguageExt;
using NetTopologySuite;
using NetTopologySuite.Densify;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Converters;
using NetTopologySuite.IO.VectorTiles;
using NetTopologySuite.IO.VectorTiles.Mapbox;
using NetTopologySuite.IO.VectorTiles.Tiles;
using NetTopologySuite.Operation.Distance;
using NetTopologySuite.Operation.Union;
using NetTopologySuite.Simplify;
using Rasm;
using Rasm.Domain;
using Rasm.Element.Classification;
using Rasm.Element.Geospatial;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using SharpKml.Base;
using SharpKml.Dom.GX;
using SharpKml.Engine;
using Thinktecture;
using static LanguageExt.Prelude;
using KmlDom = SharpKml.Dom;   // aliased — SharpKml.Dom.Feature/Geometry/Point/Polygon would shadow the NTS vocabulary

namespace Rasm.Bim;

// --- [TYPES] --------------------------------------------------------------------------------
// GeoPredicate rows form the closed DE-9IM join vocabulary as a delegate-column policy table: SpatialJoin's narrow phase dispatches the
// row, so a new spatial relation is one row — never a per-predicate join family, never a call-site bool. Within
// is the one relation the prepared surface lacks; it reads the probe-side inverse off IPreparedGeometry.Geometry.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class GeoPredicate {
    public static readonly GeoPredicate Intersects       = new("intersects",       static (p, g) => p.Intersects(g));
    public static readonly GeoPredicate Contains         = new("contains",         static (p, g) => p.Contains(g));
    public static readonly GeoPredicate ContainsProperly = new("containsproperly", static (p, g) => p.ContainsProperly(g));
    public static readonly GeoPredicate Covers           = new("covers",           static (p, g) => p.Covers(g));
    public static readonly GeoPredicate CoveredBy        = new("coveredby",        static (p, g) => p.CoveredBy(g));
    public static readonly GeoPredicate Crosses          = new("crosses",          static (p, g) => p.Crosses(g));
    public static readonly GeoPredicate Overlaps         = new("overlaps",         static (p, g) => p.Overlaps(g));
    public static readonly GeoPredicate Touches          = new("touches",          static (p, g) => p.Touches(g));
    public static readonly GeoPredicate Within           = new("within",           static (p, g) => p.Geometry.Within(g));

    [UseDelegateFromConstructor]
    public partial bool Holds(IPreparedGeometry prepared, Geometry candidate);
}

// --- [SERVICES] ---------------------------------------------------------------------------
// GeoServices is the single planar-geometry configuration root: NtsGeometryServices.Instance is the one global
// PrecisionModel/SRID/overlay owner, configured once at module init with the robust OverlayNG engine and a
// dense PackedCoordinateSequenceFactory so every reader resolves cached factories carrying one precision.
public static class GeoServices {
    static readonly Lock Gate = new();
    static bool configured;

    public static GeometryFactory Factory => Configure().CreateGeometryFactory(Srid);

    public const int Srid = 4326;

    // Wgs84 is the one WGS84 to-frame every 4326-consuming leg (H3 cell mint, MVT tile cut, KML emit) reprojects onto —
    // a georeference.md FromSite spelling with zero placement (pure datum frame, no map-conversion offset); the
    // literal EPSG:4326 admits structurally, so the Fin binds without an unreachable fallback.
    public static readonly Fin<GeoReference> Wgs84 = GeoReference.Admit(
        0.0, 0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 1.0, "WGS84", "", "EPSG:4326", "", "", "", Op.Of(name: nameof(GeoServices)));

    public static NtsGeometryServices Configure() {
        lock (Gate) {
            if (!configured) {
                NtsGeometryServices.Instance = new NtsGeometryServices(
                    PackedCoordinateSequenceFactory.DoubleFactory,
                    new PrecisionModel(PrecisionModels.Floating),
                    Srid,
                    GeometryOverlay.NG,
                    new CoordinateEqualityComparer());
                configured = true;
            }
            return NtsGeometryServices.Instance;
        }
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record GeoFeature(
    Geometry Geometry,
    IAttributesTable Attributes,
    Option<ProjectedCrs> SourceCrs) {
    public OgcGeometryType Kind => Geometry.OgcGeometryType;
    public Envelope Bounds => Geometry.EnvelopeInternal;

    public GeoFeature Repaired => Geometry.IsValid ? this : this with { Geometry = GeometryFixer.Fix(Geometry) };

    // IAttributesTable carries no GetOptionalValue (that member is the concrete AttributesTable's) — the optional read
    // is the Exists-guarded indexer so an absent attribute yields None rather than a KeyNotFound throw.
    public Option<object> Attr(string name) => Attributes.Exists(name) ? Optional(Attributes[name]) : None;

    // Densifier.Densify vertex insertion so no segment exceeds maxSegment — the pre-reprojection step a long survey
    // edge takes so the curve tracks a non-linear datum transform instead of chording across it.
    public GeoFeature Refined(double maxSegment) => this with { Geometry = Densifier.Densify(Geometry, maxSegment) };

    // TopologyPreservingSimplifier vertex shedding with no ring self-cross — the coarse site-abstraction inverse of
    // Refined; DouglasPeuckerSimplifier is the rejected form on ring-bearing features (it corner-cuts topology).
    public GeoFeature Simplified(double tolerance) => this with { Geometry = TopologyPreservingSimplifier.Simplify(Geometry, tolerance) };

    // Cell is the DGGS site-context keyer: reproject onto the Wgs84 frame through the ONE datum leg, then the v4-canonical
    // H3Index.FromPoint over the centroid — the SAME 64-bit cell h3-pg computes server-side, so the in-process bucket
    // and the persisted h3_cell column agree bit-for-bit. H3Index.Invalid projects to None, never a stored zero cell.
    public Fin<Option<ulong>> Cell(int resolution, Op key) =>
        GeoServices.Wgs84.Bind(frame => Reproject(frame, key)).Map(wgs => {
            H3Index cell = H3Index.FromPoint(wgs.Geometry.Centroid, resolution);
            return cell.IsValidCell ? Some((ulong)cell) : Option<ulong>.None;
        });

    // ToObject is the seam-node projection: a vector feature RIDES an Object node [§4B] carrying the generic Classification
    // ("ifc", true IFC4.3 class string the GeoClassifier carries), the first-class PredefinedType token admitted BARE
    // (validity is the Bim Emit egress gate over the IfcClass valid-set, never an ingress invariant [C6]), the footprint
    // content-keyed into the Representations map ("FootPrint" -> kernel seed-zero ContentHash over the reprojected WKB)
    // [M2] so a Rasm.Compute consumer RESOLVES the analytical surface one-hop by content key from the blob store, NEVER
    // an inline coordinate field on the seam node (no Object.BoundaryPolygon/Axis member exists — the deleted §4-RT-M2
    // violation). The attribute bag is a Pset_SiteContext PropertySet node linked by a neutral Assign(PropertyDefinition)
    // edge (Subject=object, Definition=pset — NOT an Associate, which the seam reserves for a material/appearance/coverage
    // resource). The GeoJSON footprint rides one PropertyValue.Text so the cross-runtime shapely/turf peers decode it,
    // and the content-keyed WKB is the canonical analytical surface the structural/energy disciplines resolve. The delta
    // carries no Header — the seam Assemble fold composes it onto the graph.
    public Fin<GraphDelta> ToObject(GeoReference reference, ProjectionContext ctx) =>
        GeoClassifier.Classify(this, ctx.Key).Bind(row =>
        Reproject(reference, ctx.Key).Map(footprint => {
            NodeId objectId = NodeId.Rooted();
            // Blank-name skip: a blank OGR/DBF field name would throw PropertyName.Create INSIDE the Map, escaping the rail.
            Map<PropertyName, PropertyValue> values = footprint.Attributes.GetNames().AsIterable()
                .Filter(static name => name.Length > 0)
                .Fold(Map<PropertyName, PropertyValue>(), (bag, name) =>
                    bag.AddOrUpdate(PropertyName.Create(name), new PropertyValue.Text(footprint.Attributes[name]?.ToString() ?? "")))
                .AddOrUpdate(PropertyName.Create("Footprint"), new PropertyValue.Text(GeoWire.ToGeoJson(footprint)));
            var pset = new Node.PropertySet(NodeId.Rooted(), new PropertyBag("Pset_SiteContext", values, InheritanceMode.OccurrenceWins, PropertySource.Import));
            var obj = new Node.Object(
                Id:              objectId,
                Kind:            ObjectKind.Occurrence,
                ExternalId:      footprint.Attr("id").Bind(static v => v.ToString() is { Length: > 0 } id ? Some(id) : Option<string>.None),
                Classification:  Classification.Create("ifc", row.Class, "", None, None, None),
                PredefinedType:  PredefinedType.Create(row.Predefined),
                Name:            footprint.Attr("name").Map(static v => v.ToString() ?? "").Filter(static n => n.Length > 0).IfNone(row.Class),
                Tag:             footprint.Attr("id").Map(static v => v.ToString() ?? "").IfNone(""),
                Representations: RepresentationContentHash.Empty.With("FootPrint", ContentHash.Of(GeoWkb.FromNts(footprint.Geometry))),
                History:         Option<OwnerHistory>.None,
                Span:            SchemaSpan.From(ReleaseVersion.Ifc4X3Add2));
            return GraphDelta.Empty
                .Put(obj).Put(pset)
                .Link(new Relationship.Assign(objectId, pset.Id, AssignKind.PropertyDefinition));
        }));

    // Reproject composes the Semantics/georeference#GEODETIC_TRANSFORM GeoTransform.Reproject leg — the
    // ONE ProjNET/OSR datum owner over the seam GeoReference, reprojecting a DOUBLE-precision ordinate Span<double> IN
    // PLACE (survey eastings never narrow to float). SourceCrs is the from-frame, the target the to-frame (the project
    // reference at ToObject, GeoServices.Wgs84 on the H3/MVT/KML legs); the leg is additive (an Unreferenced endpoint
    // or an equal CRS leaves the ordinates untouched so a single-datum site never blocks — a WKT-resolved frame with
    // no EPSG still transforms) and faults bare off key only when both engines defeat a present, differing pair. Geometry.Coordinates is a DETACHED snapshot, so the
    // projector flattens it into the buffer, hands the buffer to the owner, and writes the reprojected ordinates back
    // through Geometry.Apply (the .api/api-nettopologysuite reprojection seam — the in-place ordinate visitor); a
    // NetTopologySuite-side datum shift (a filter that COMPUTES the transform) is the named seam violation, distinct
    // from this write-back visitor that COMPUTES nothing and only marshals the owner's output.
    public Fin<GeoFeature> Reproject(GeoReference target, Op key) {
        GeoReference source = GeoReference.Identity with { Crs = SourceCrs };
        Coordinate[] coordinates = Geometry.Coordinates;
        var ordinates = new double[coordinates.Length * 3];
        for (int i = 0; i < coordinates.Length; i++) {
            (ordinates[i * 3], ordinates[i * 3 + 1], ordinates[i * 3 + 2]) =
                (coordinates[i].X, coordinates[i].Y, double.IsNaN(coordinates[i].Z) ? 0.0 : coordinates[i].Z);
        }
        return GeoTransform.Reproject(source, target, ordinates.AsSpan(), stride: 3, key).Map(_ => {
            Geometry shifted = Geometry.Copy();
            shifted.Apply(new OrdinateWriteback(ordinates));
            // Result carries the frame it now HOLDS, never the stale from-frame: geometry SRID stamps the target's
            // EPSG (0 when the target resolves by WKT only) and SourceCrs re-stamps to the target's projected CRS —
            // clearing to None on the geodetic EPSG:4326 anchor — so a consumer ingress gate (the AppUi basemap
            // admits SourceCrs.IsNone && SRID == 4326) admits a reprojected feature on the feature's own evidence.
            shifted.SRID = target.Epsg.IfNone(0);
            return this with {
                Geometry = shifted,
                SourceCrs = target.Epsg == Some(4326) ? Option<ProjectedCrs>.None : target.Crs,
            };
        });
    }

    // OrdinateWriteback is the write-back visitor the reprojection seam rides: it COMPUTES no transform (the datum
    // shift is GeoTransform's), it is pure index-aligned marshalling, so it is NOT the deleted NTS-side-datum-shift
    // filter. Geometry.Apply walks the components in the SAME order Geometry.Coordinates enumerated, so the running
    // cursor aligns the flat buffer with each visited ordinate; GeometryChanged invalidates the cached envelope after the walk.
    sealed class OrdinateWriteback(double[] ordinates) : ICoordinateSequenceFilter {
        int cursor;
        public bool Done => false;
        public bool GeometryChanged => true;
        public void Filter(CoordinateSequence seq, int i) {
            seq.SetX(i, ordinates[cursor * 3]);
            seq.SetY(i, ordinates[cursor * 3 + 1]);
            if (seq.HasZ) { seq.SetZ(i, ordinates[cursor * 3 + 2]); }
            cursor++;
        }
    }
}

public sealed record GeoModel {
    // Non-positional record: Of is the ONE admission (GeometryFixer repair runs exactly once, un-bypassable) — a public
    // positional ctor would readmit unrepaired geometry past the validity gate.
    GeoModel(Seq<GeoFeature> features) { Features = features; index = Build(features); }

    public Seq<GeoFeature> Features { get; }

    readonly STRtree<GeoFeature> index;

    // A `with` copy would alias the built-once STRtree against a different Features set, surfacing stale broad-phase
    // candidates from the wrong feature set (the Graph/element#ELEMENT_GRAPH frozen-snapshot guard, applied here) — so a
    // `with` copy is forbidden; only Of mints a fresh indexed model and the index rebuilds with it.
    private GeoModel(GeoModel original) =>
        throw new InvalidOperationException("GeoModel carries a built-once STRtree index and must not be copied via `with`; build one through GeoModel.Of.");

    public static GeoModel Of(Seq<GeoFeature> features) => new(features.Map(static f => f.Repaired));

    // Default STR node capacity (10): the ctor arg is per-NODE fan-out, not item count — seeding it with the feature
    // count flattens the tree to one node and degrades every Query to a linear scan (the deleted form).
    static STRtree<GeoFeature> Build(Seq<GeoFeature> features) {
        var tree = new STRtree<GeoFeature>();
        features.Iter(f => tree.Insert(f.Bounds, f));
        return tree;
    }

    // Canonical broad-then-narrow spatial join: STRtree.Query envelope candidates, then the GeoPredicate row's
    // IPreparedGeometry delegate per candidate — the DE-9IM relation is a policy value, never a sibling join method.
    public Seq<GeoFeature> SpatialJoin(Geometry probe, GeoPredicate predicate) {
        var prepared = PreparedGeometryFactory.Prepare(probe);
        return index.Query(probe.EnvelopeInternal).AsIterable().Filter(f => predicate.Holds(prepared, f.Geometry)).ToSeq();
    }

    // Open DE-9IM tail: an ad-hoc 9-char mask joins without minting a vocabulary row — the GeoPredicate table
    // stays the closed named set, Geometry.Relate(g, pattern) the generator over the full relation space.
    public Seq<GeoFeature> SpatialJoin(Geometry probe, string de9im) =>
        index.Query(probe.EnvelopeInternal).AsIterable().Filter(f => probe.Relate(f.Geometry, de9im)).ToSeq();

    // STRtree k-NN over true geometry separation, each hit stamped with its DistanceOp.NearestPoints closest-pair
    // clash-gap witness a site-proximity report reads (Witness[0] on the probe, Witness[1] on the feature). An
    // empty-model guard is load-bearing: NearestNeighbour on an empty STRtree throws, never returns empty.
    public Seq<(GeoFeature Feature, double Distance, Coordinate[] Witness)> Nearest(Geometry probe, int k) =>
        Features.IsEmpty
            ? Seq<(GeoFeature, double, Coordinate[])>()
            : index.NearestNeighbour(probe.EnvelopeInternal, new GeoFeature(probe, new AttributesTable(), Option<ProjectedCrs>.None), GeoDistance.Instance, k)
                .AsIterable()
                .Map(f => { Coordinate[] witness = DistanceOp.NearestPoints(probe, f.Geometry); return (f, witness[0].Distance(witness[1]), witness); })
                .ToSeq();

    // Parcel-setback carve: the repaired parcel shrunk by the setback (negative Buffer), minus the dissolved
    // context footprints — the buildable-region evidence a zoning check reads, one composed overlay, no fault rail
    // (a fully-consumed parcel yields the valid empty polygon, never an error).
    public Geometry Setback(Geometry parcel, double distance) =>
        GeometryFixer.Fix(parcel).Buffer(-Math.Abs(distance)).Difference(Dissolve());

    // Features were repaired once at Of — re-running the per-call IsValid scan here is the deleted double-admission.
    // Empty-set arm mints the valid empty polygon: OverlayNGRobust.Union of zero geometries returns NULL (the
    // NTS empty-union convention), which would NPE the Setback Difference — the guard keeps the algebra total.
    public Geometry Dissolve() =>
        Features.IsEmpty
            ? GeoServices.Factory.CreatePolygon()
            : OverlayNGRobust.Union(Features.Map(static f => f.Geometry).ToArray());

    // Coarse DGGS join map beside the STRtree: every feature bucketed by its centroid H3 cell at one resolution —
    // a regional query is a cell-membership read, never a per-feature scan; a cell-less feature (invalid centroid) drops.
    public Fin<HashMap<ulong, Seq<GeoFeature>>> Bucket(int resolution, Op key) =>
        Features.Traverse(f => f.Cell(resolution, key).Map(cell => (Cell: cell, Feature: f))).As()
            .Map(static pairs => pairs.Fold(
                HashMap<ulong, Seq<GeoFeature>>(),
                static (acc, pair) => pair.Cell.Match(
                    Some: id => acc.AddOrUpdate(id, Some: s => s.Add(pair.Feature), None: () => Seq(pair.Feature)),
                    None: () => acc)));

    // A probe region lowered to its compacted H3 cover: Geometry.Fill polyfill at the resolution, GridDiskDistances
    // ring expansion (ring 0 = the fill alone), CompactCells to the minimal mixed-resolution region key — the
    // FrozenSet<ulong> the Persistence h3-pg `h3_cell = ANY(@cells)` prefilter tests against the SAME cell ids.
    public static Fin<FrozenSet<ulong>> Cover(Geometry probe, Option<ProjectedCrs> crs, int resolution, int ring, Op key) =>
        GeoServices.Wgs84
            .Bind(frame => new GeoFeature(probe, new AttributesTable(), crs).Reproject(frame, key))
            .Map(wgs => {
                var fill = wgs.Geometry.Fill(resolution).ToSeq();
                var expanded = ring > 0 ? fill.Bind(cell => cell.GridDiskDistances(ring).AsIterable().Map(static r => r.Index).ToSeq()) : fill;
                return expanded.Distinct().CompactCells().AsIterable().Map(static cell => (ulong)cell).ToFrozenSet();
            });

    // MVT LOD pyramid: the set reprojects onto Wgs84 through the ONE datum leg, the route delegate assigns each
    // feature its (zoom, layer) slots (the per-feature LOD/layer policy — one feature may land at many zooms), each
    // slot's geometry passes the zoom-matched TopologyPreservingSimplifier (one MVT integer-grid cell in degrees, so
    // low-zoom tiles shed sub-pixel vertices without ring self-cross), and the tupled stream folds through the ONE
    // VectorTileTree.Add. GeoTiles owns the bytes; this owner never touches the protobuf.
    public Fin<VectorTileTree> ToTiles(Func<GeoFeature, Seq<(int Zoom, string Layer)>> route, Op key) =>
        GeoServices.Wgs84
            .Bind(frame => Features.Traverse(f => f.Reproject(frame, key)).As())
            .Map(wgs => {
                var tree = new VectorTileTree();
                tree.Add(wgs.Bind(f => route(f).Map(slot => (Lod(f, slot.Zoom), slot.Zoom, slot.Layer))));
                return tree;
            });

    static IFeature Lod(GeoFeature feature, int zoom) =>
        new Feature(TopologyPreservingSimplifier.Simplify(feature.Geometry, 360.0 / (4096.0 * (1L << zoom))), feature.Attributes);

    // Whole site-context feature set folds into ONE header-less GraphDelta the seam Assemble fold composes — each
    // feature's Object + PropertySet nodes and the Assign(PropertyDefinition) edge accumulate onto the graph in one apply.
    public Fin<GraphDelta> Project(GeoReference reference, ProjectionContext ctx) =>
        Features.Traverse(f => f.ToObject(reference, ctx)).As()
            .Map(static deltas => deltas.Fold(GraphDelta.Empty, static (acc, delta) => acc.Merge(delta)));

    // True-separation item distance for the STRtree k-NN leg — envelope order alone would rank a large far feature
    // above a small near one, so the metric reads the geometries.
    sealed class GeoDistance : IItemDistance<Envelope, GeoFeature> {
        public static readonly GeoDistance Instance = new();
        public double Distance(IBoundable<Envelope, GeoFeature> a, IBoundable<Envelope, GeoFeature> b) =>
            a.Item.Geometry.Distance(b.Item.Geometry);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoClassifier {
    // Frozen (geometry-kind, tag) -> (true IFC entity-type string, predefined token) site-context table — a data
    // table, never enumerated switch arms. Class is the TRUE IFC4.3 entity-type the seam Classification carries as
    // a library-neutral (system, code) pair, NOT a Model/elements#IFC_CLASS IfcClass row: the seam never validates the
    // class against the roster, and resolving IfcGeographicElement/IfcSite/IfcBuilding through IfcClass.TryGet would
    // collapse them to the Proxy fallback. Bim's Emit egress gate resolves the IfcClass row from this code and admits
    // its predefined token [C6], so a class the roster has not yet rostered round-trips to IFC only once it is added.
    // Each "" tag row is the per-kind generic fallback toward the IfcGeographicElement geographic-context catch-all.
    static readonly Map<(OgcGeometryType Kind, string Tag), (string Class, string Predefined)> Table =
        Map(
            ((OgcGeometryType.Polygon,      "building"),  ("IfcBuilding",          "NOTDEFINED")),
            ((OgcGeometryType.MultiPolygon, "building"),  ("IfcBuilding",          "NOTDEFINED")),
            ((OgcGeometryType.Polygon,      "parcel"),    ("IfcSite",              "NOTDEFINED")),
            ((OgcGeometryType.Polygon,      "landuse"),   ("IfcSite",              "NOTDEFINED")),
            ((OgcGeometryType.Polygon,      "relief"),    ("IfcGeographicElement", "TERRAIN")),
            ((OgcGeometryType.MultiPolygon, "relief"),    ("IfcGeographicElement", "TERRAIN")),
            ((OgcGeometryType.LineString,   "road"),      ("IfcRoad",              "NOTDEFINED")),   // the IFC4.3 FACILITY — IfcCourse/IfcRail are construction-product classes, wrong for a GIS corridor
            ((OgcGeometryType.Polygon,      "road"),      ("IfcRoad",              "NOTDEFINED")),
            ((OgcGeometryType.LineString,   "rail"),      ("IfcRailway",           "NOTDEFINED")),
            ((OgcGeometryType.LineString,   "bridge"),    ("IfcBridge",            "NOTDEFINED")),
            ((OgcGeometryType.Polygon,      "bridge"),    ("IfcBridge",            "NOTDEFINED")),
            ((OgcGeometryType.LineString,   "contour"),   ("IfcGeographicElement", "TERRAIN")),
            ((OgcGeometryType.Point,        "tree"),      ("IfcGeographicElement", "VEGETATION")),
            ((OgcGeometryType.Polygon,      "vegetation"), ("IfcGeographicElement", "VEGETATION")),   // landcover polygon — the IFC4.3 VEGETATION token, not the NOTDEFINED fallback
            ((OgcGeometryType.MultiPolygon, "vegetation"), ("IfcGeographicElement", "VEGETATION")),
            ((OgcGeometryType.LineString,   "waterway"),  ("IfcMarineFacility",    "WATERWAY")),   // the IFC4.3 marine FACILITY — the navigable-corridor peer of the road/rail rows
            ((OgcGeometryType.Polygon,      ""),          ("IfcGeographicElement", "NOTDEFINED")),
            ((OgcGeometryType.MultiPolygon, ""),          ("IfcGeographicElement", "NOTDEFINED")),
            ((OgcGeometryType.LineString,   ""),          ("IfcGeographicElement", "NOTDEFINED")),
            ((OgcGeometryType.Point,        ""),          ("IfcGeographicElement", "NOTDEFINED")));

    // Resilient ingress: a non-empty feature ALWAYS classifies (the IfcGeographicElement catch-all absorbs an unmapped
    // (kind, tag)) so one unrecognized feature never aborts a whole site import; only an EMPTY geometry (no footprint)
    // faults Model/faults#FAULT_BAND BimFault.UnmappedClass, the typed case lifted BARE off ctx.Key (band 2600 IS the
    // Expected Code, no .ToError() hop). The tag reads the feature "type"/"class" attribute.
    public static Fin<(string Class, string Predefined)> Classify(GeoFeature feature, Op key) {
        if (feature.Geometry.IsEmpty) {
            return Fin.Fail<(string Class, string Predefined)>(new BimFault.UnmappedClass(key, $"geo-feature-miss:empty:{feature.Kind}"));
        }
        string tag = feature.Attr("type").Map(static v => v.ToString() ?? "")
            .IfNone(() => feature.Attr("class").Map(static v => v.ToString() ?? "").IfNone(""))
            .ToLowerInvariant();
        return Fin.Succ((Table.Find((feature.Kind, tag)) | Table.Find((feature.Kind, "")))   // the verified Option `|` alternative — `.OrElse` is a phantom member
            .IfNone(("IfcGeographicElement", "NOTDEFINED")));
    }
}

// --- [BOUNDARIES] ---------------------------------------------------------------------------
// MVT byte codec + TileJSON catalog over the NetTopologySuite.IO.VectorTiles.Mapbox pair: Encode streams each
// populated tile through the per-tile VectorTile.Write (the object-store/tile-endpoint form — tree.Write(path, …)
// filesystem pyramids are the rejected form for a store-backed host), Decode re-anchors stored bytes against their
// Tile(x, y, zoom) definition (MVT bytes carry only tile-local integer coordinates, never the geographic anchor),
// and Catalog emits the TileJSON 2.0.0 descriptor off GetExtents so bounds/zoom-span never hand-author beside the
// pyramid. The sub-pixel cull rides the writer's own DefaultMinLinealExtent/DefaultMinPolygonalExtent thresholds.
public static class GeoTiles {
    public static Fin<Seq<(ulong TileId, byte[] Bytes)>> Encode(VectorTileTree tree, Op key) =>
        Try.lift(() => tree.GetTileIds()
            .Select(id => {
                using var buffer = new MemoryStream();
                tree[id].Write(buffer, MapboxTileWriter.DefaultMinLinealExtent, MapboxTileWriter.DefaultMinPolygonalExtent);
                return (TileId: id, Bytes: buffer.ToArray());
            })
            .ToArray().ToSeq()).Run()
            .MapFail(error => new BimFault.CodecReject(key, $"geo-mvt-encode:{error.Message}"));

    // Single world-tile emit the GeoVectorSource.Mvt encode column binds: the set reprojects onto the ONE
    // Wgs84 frame (MVT quantizes lon/lat), lands one z0 (0,0,0) tile through the same VectorTile.Write path, and
    // THROWS into the row's Try rail on a reprojection reject — the pyramid emit stays Encode/Catalog.
    internal static byte[] EncodeWorldTile(Seq<GeoFeature> features) {
        Op key = Op.Of(name: nameof(EncodeWorldTile));
        Seq<GeoFeature> wgs = GeoServices.Wgs84
            .Bind(frame => features.Traverse(f => f.Reproject(frame, key)).As())
            .Match(Succ: static s => s, Fail: static error => throw new InvalidDataException(error.Message));
        var tile = new VectorTile { TileId = new NetTopologySuite.IO.VectorTiles.Tiles.Tile(0, 0, 0).Id };
        var layer = new Layer { Name = "features" };
        wgs.Iter(f => layer.Features.Add(new NetTopologySuite.Features.Feature(f.Geometry, f.Attributes)));
        tile.Layers.Add(layer);
        using var buffer = new MemoryStream();
        tile.Write(buffer, MapboxTileWriter.DefaultMinLinealExtent, MapboxTileWriter.DefaultMinPolygonalExtent);
        return buffer.ToArray();
    }

    // Decode threads the layer name beside each row — the renderer's style key is load-bearing round-trip
    // evidence; the reader de-quantizes against the canonical GeoServices.Factory precision.
    public static Fin<Seq<(string Layer, GeoFeature Feature)>> Decode(ReadOnlyMemory<byte> bytes, int x, int y, int zoom, Op key) =>
        Try.lift(() => {
            using var stream = new MemoryStream(bytes.ToArray());
            VectorTile tile = new MapboxTileReader(GeoServices.Factory)
                .Read(stream, new NetTopologySuite.IO.VectorTiles.Tiles.Tile(x, y, zoom), MapboxTileWriter.DefaultIdAttributeName);
            return tile.Layers.AsIterable()
                .Bind(layer => layer.Features.AsIterable()
                    .Map(f => (layer.Name, new GeoFeature(f.Geometry, f.Attributes, Option<ProjectedCrs>.None))))
                .ToSeq();
        }).Run().MapFail(error => new BimFault.CodecReject(key, $"geo-mvt-decode:{error.Message}"));

    // TileJSON 2.0.0 source descriptor a MapLibre/Mapsui renderer discovers the pyramid through: tiles is the
    // {z}/{x}/{y}.mvt URL template, bounds/minzoom/maxzoom read off the populated tree (GetExtents), vector_layers
    // carry the per-layer zoom advertisement; format "pbf"/tilejson "2.0.0" ride the DTO defaults, field names the
    // wire snake_case the DTO declares.
    public static string Catalog(VectorTileTree tree, string name, string urlTemplate, Seq<VectorLayer> layers) {
        tree.GetExtents(out double[] bounds, out int minZoom, out int maxZoom);
        return JsonSerializer.Serialize(new VectorTileSource {
            id = name,
            name = name,
            tiles = [urlTemplate],
            bounds = bounds,
            minzoom = minZoom,
            maxzoom = maxZoom,
            vector_layers = layers.ToArray(),
        });
    }
}
```

## [03]-[VECTOR_INGEST]

- Owner: `GeoVector` the universal vector ingest-and-egress fold over `GeoVectorSource`, the `[SmartEnum<string>]` source table whose rows carry the `decode`/`encode` codec-pair columns — dedicated managed codecs (shapefile, GeoJSON, CityJSON ingest-only by row law, FlatGeobuf, GeoParquet, KML/KMZ) and the `MaxRev.Gdal.Core` OGR universal reader for the long-tail, every arm producing the canonical `GeoFeature`; `GeoWire` the `GeoFeature`'s two canonical wire projections per `data-interchange#GEO_INTERCHANGE` (GeoJSON text the cross-runtime `shapely`/`turf` peers decode, the GeoPackage binary blob the `csharp:Rasm.Persistence/Store` geo-store persists); `GeoWkb` the ONE bidirectional OGR↔NTS bridge every GDAL leg and the GeoParquet geo-column cross; `GeoKml` the managed KML/KMZ codec (the GDAL OGR `KML` driver the rejected style-and-extended-data-losing form) and the `Site` styled-KMZ presentation emit.
- Entry: `GeoVector.Read(source, bytes, clip, key)` dispatches the row's `Decode` column onto `Seq<GeoFeature>`, the `clip` driving a server-side window push-down (shapefile `MbrFilter`, FlatGeobuf's Packed-Hilbert-R-tree, OGR `SetSpatialFilterRect`) and a corrupt container faulting `Model/faults#FAULT_BAND` `BimFault.CodecReject` off `key`; `GeoVector.Stream(fetch, window, key)` is the remote-`.fgb` range-read escalation, answering a window query over a continental `.fgb` in a handful of byte-range reads rather than a whole-file pull; `GeoVector.Write(source, features, crs, key)` dispatches the row's `Encode` column symmetrically, the OGR universal egress writing its driver output to a real temp file (this GDAL SWIG build exposes no `byte[]` `VSIFReadL`).
- Auto: the `GeoVectorSource` row's delegate columns route decode AND encode with no call-site branch, each managed arm pushing the `clip` down through its own spatial index and each OGR arm through `Layer.SetSpatialFilterRect`; a remote `.fgb` escalates to `GeoVector.Stream`; every produced `GeoFeature` is `GeometryFixer.Fix`-repaired at admission.
- Receipt: the `GeoVector.Read` `Seq<GeoFeature>` is the universal vector ingest evidence the `GEOSPATIAL_SEAM` `GeoModel` indexes and the `GeoFeature.ToObject`/`GeoModel.Project` projection lowers onto seam `Object` nodes; the `GeoVectorSource` row records which codec decoded so the reader is one table read.
- Packages: `NetTopologySuite`, `NetTopologySuite.IO.Esri.Shapefile`, `bertt.CityJSON`, `FlatGeobuf`, `GISBlox.IO.GeoParquet`, `SharpKml.Core`, `MaxRev.Gdal.Core`, `NodaTime`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new vector format is one `GeoVectorSource` row carrying its `decode`/`encode` codec pair — `managed:true` for a dedicated codec, `managed:false` closing over its OGR driver token — with zero entry-point edits; a new attribute push-down is one `Layer.SetAttributeFilter`/`ReadColumns` argument; a new KML symbology is one `Site` styles row and routing; a new remote transport is one `PackedRTree.ReadNode` delegate value; never a per-format importer family, never a hand-rolled binary record, and never a boolean op on the OGR side.
- Boundary: the managed shapefile/FlatGeobuf/GeoParquet/KML codecs are the pure-managed defaults and admitting GDAL for a format a managed codec reads is the rejected form — the OGR `KML` driver specifically the style-and-extended-data-losing form `GeoKml` deletes; the managed codec output IS the canonical `NetTopologySuite.Features.Feature`; the shapefile byte form is the zipped `.shp`/`.shx`/`.dbf`/`.prj` quartet BOTH directions — a `Stream.Null` dbf read that drops every attribute and a bare-`.shp` egress that strands the offset index and attribute table are the deleted fragment forms; the OGR↔NTS bridge is the ONE `GeoWkb` owner and a second inline WKB spelling is the deleted form, running planar boolean ops on the OGR side fragmenting the one topology owner; SharpKml carries its OWN geographic geometry (`Vector` is `(lat, lon[, alt])`, the inverse of an NTS `Coordinate(X=lon, Y=lat)`) so the bridge swaps ordinates both directions and a cast is unrepresentable; the `GdalBase.ConfigureAll()` bootstrap runs once per process behind the `IsConfigured` guard and `Gdal.UseExceptions()` flips the SWIG error model so a failed open lowers onto `BimFault.CodecReject`; the CityJSON quantization is lossless (integer indices into `Vertices`, recovered through `Transform`) and tessellating it in the codec is the deleted form; `CityJSON.*`/`OSGeo.*`/`FlatGeobuf.*`/`GISBlox.*`/`SharpKml.*` types never leak past this fold; the `Site` KMZ and the MVT pyramid are DELIVERY projections, never the cross-runtime peer wire — GeoJSON text and the GeoPackage blob stay the only two wire forms.

```csharp signature
// Vector source table with DECODE/ENCODE delegate columns: Read/Write dispatch the row, so a new format is one
// row carrying its codec pair — never a call-site if-ladder, never a per-format importer family. Managed marks the
// pure-managed codecs the boundary law protects from OGR regression; the OGR long-tail rows close over their own
// driver token (Ogr.Open auto-detects on read, GetDriverByName pins the write). CityJson is ingest-only by row law
// (its encode column throws into the Read/Write Try rail); Kml is MANAGED both ways through SharpKml — the GDAL KML
// driver is the rejected style-and-extended-data-losing form.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class GeoVectorSource {
    public static readonly GeoVectorSource Shapefile  = new("shapefile",  managed: true,
        decode: static (bytes, clip) => GeoVector.Shapefile(bytes, clip),
        encode: static (features, crs) => GeoVector.WriteShapefile(features, crs));
    public static readonly GeoVectorSource GeoJson    = new("geojson",    managed: true,
        decode: static (bytes, clip) => GeoVector.GeoJson(bytes, clip),
        encode: static (features, _) => GeoVector.WriteGeoJson(features));
    public static readonly GeoVectorSource CityJson   = new("cityjson",   managed: true,
        decode: static (bytes, _) => GeoVector.CityJson(bytes),
        encode: static (_, _) => throw new NotSupportedException("cityjson-egress-unsupported:ingest-only"));
    public static readonly GeoVectorSource FlatGeobuf = new("flatgeobuf", managed: true,
        decode: static (bytes, clip) => GeoVector.FlatGeobuf(bytes, clip),
        encode: static (features, _) => GeoVector.WriteFlatGeobuf(features));
    public static readonly GeoVectorSource GeoParquet = new("geoparquet", managed: true,
        decode: static (bytes, clip) => GeoVector.GeoParquet(bytes, clip),
        encode: static (features, _) => GeoVector.WriteGeoParquet(features));
    public static readonly GeoVectorSource Kml        = new("kml",        managed: true,
        decode: static (bytes, clip) => GeoKml.Read(bytes, clip),
        encode: static (features, _) => GeoKml.Write(features));
    // KMZ shares the zip-discriminating GeoKml.Read (the kmz magic routes KmzFile.Open internally); its encode is
    // a symmetric bare-KMZ zip of the Write document — format#FORMAT_AXIS Kmz capability columns are arm-backed here.
    public static readonly GeoVectorSource Kmz        = new("kmz",        managed: true,
        decode: static (bytes, clip) => GeoKml.Read(bytes, clip),
        encode: static (features, _) => GeoKml.WriteKmz(features));
    // Single-tile MVT byte codec anchored at the world tile (0,0,0) — MVT bytes carry only tile-local integer
    // coordinates, so the bare-bytes row pins the one self-describing anchor; the multi-tile pyramid + TileJSON
    // catalog stay GeoTiles.Encode/Catalog. format#FORMAT_AXIS Mvt capability columns are arm-backed here.
    public static readonly GeoVectorSource Mvt        = new("mvt",        managed: true,
        decode: static (bytes, _) => GeoTiles.Decode(bytes, 0, 0, 0, Op.Of(name: "mvt-row")).Match(
            Succ: static rows => rows.Map(static r => r.Feature),
            Fail: static error => throw new InvalidDataException(error.Message)),
        encode: static (features, _) => GeoTiles.EncodeWorldTile(features));
    public static readonly GeoVectorSource GeoPackage = new("geopackage", managed: false,
        decode: static (bytes, clip) => GeoVector.Universal(bytes, clip),
        encode: static (features, crs) => GeoVector.WriteUniversal("GPKG", features, crs));
    public static readonly GeoVectorSource Gml        = new("gml",        managed: false,
        decode: static (bytes, clip) => GeoVector.Universal(bytes, clip),
        encode: static (features, crs) => GeoVector.WriteUniversal("GML", features, crs));
    public static readonly GeoVectorSource FileGdb    = new("filegdb",    managed: false,
        decode: static (bytes, clip) => GeoVector.Universal(bytes, clip),
        encode: static (features, crs) => GeoVector.WriteUniversal("OpenFileGDB", features, crs));

    [UseDelegateFromConstructor]
    public partial Seq<GeoFeature> Decode(ReadOnlyMemory<byte> bytes, Option<Envelope> clip);

    [UseDelegateFromConstructor]
    public partial byte[] Encode(Seq<GeoFeature> features, Option<ProjectedCrs> crs);

    public bool Managed { get; }
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
// GeoWire owns the GeoFeature's two canonical wire projections per data-interchange#GEO_INTERCHANGE: NetTopologySuite is the
// SINGLE interior geo vocabulary and GeoJSON text with the GeoPackage binary blob are its ONLY two wire forms.
// GeoJSON text is the cross-runtime geometry wire the Python shapely.from_geojson and TS turf peers decode; the
// GeoPackage blob is the Rasm.Persistence/Store geo-store-blob projection.
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

// ONE OGR↔NTS bridge: every GDAL leg (universal read/write, contour), the GeoParquet geo-column cell, AND the
// FootPrint content-key WKB cross here — the reader resolves the canonical GeoServices precision and the writer
// preserves Z (Geometry.AsBinary's 2D default drops a terrain footprint's elevations — the deleted second inline
// spelling), so no other WKB spelling exists in the fold. Both owners hold settings only; Read/Write state is call-local.
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

// MANAGED KML/KMZ codec + styled-presentation emit over SharpKml.Core — the full OGC-KML object model the GDAL
// OGR KML driver cannot reach (it drops Style/ExtendedData/Tour). SharpKml carries its OWN geographic geometry
// (Vector is (lat, lon[, alt]) — the INVERSE of an NTS Coordinate(X=lon, Y=lat)), so the bridge swaps ordinates in
// both directions and never casts. Read/Write are the raw codec arms the GeoVectorSource.Kml row binds (throw-on-
// corrupt inside the Read/Write Try rail, geometry 4326 by KML law, tagged EPSG:4326 so a project-CRS projection
// datum-shifts correctly); Site is the composed Fin-railed presentation entry that owns its Wgs84 reprojection.
public static class GeoKml {
    const string OrthoEntry = "files/ortho.png";

    // KMZ discriminates on the zip magic; KmzFile.GetDefaultKmlFile resolves the doc.kml. Containers walk
    // recursively, each Placemark lowering to one GeoFeature: geometry through the lat/lon-swapped bridge,
    // name/id/description with the ExtendedData Data rows and SchemaData SimpleData rows onto the attribute bag.
    // KML is geographic by definition, so every row tags the structurally-admitting EPSG:4326 source CRS.
    internal static Seq<GeoFeature> Read(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        using var stream = new MemoryStream(bytes.ToArray());
        KmlFile file;
        if (bytes.Span is [0x50, 0x4B, ..]) { using var kmz = KmzFile.Open(stream); file = kmz.GetDefaultKmlFile(); }
        else { file = KmlFile.Load(stream); }
        Option<ProjectedCrs> crs = ProjectedCrs.Of("EPSG:4326", "", "", "", Op.Of(name: nameof(GeoKml)))
            .Match(Succ: static c => Some(c), Fail: static _ => Option<ProjectedCrs>.None);
        KmlDom.Feature? root = file.Root switch { KmlDom.Kml k => k.Feature, KmlDom.Feature f => f, _ => null };
        var features = root is null ? Seq<GeoFeature>() : Walk(root, crs);
        return clip.Match(None: () => features, Some: env => features.Filter(f => f.Bounds.Intersects(env)));
    }

    static Seq<GeoFeature> Walk(KmlDom.Feature feature, Option<ProjectedCrs> crs) => feature switch {
        KmlDom.Container container => container.Features.AsIterable().Bind(child => Walk(child, crs)).ToSeq(),
        KmlDom.Placemark mark when Lower(mark.Geometry) is { } geometry => Seq(new GeoFeature(geometry, Attributes(mark), crs)),
        _ => Seq<GeoFeature>(),
    };

    // KML DOM -> NTS: Point/LineString/LinearRing/Polygon(+holes)/MultipleGeometry, each Vector swapped onto
    // Coordinate(X=Longitude, Y=Latitude); an open KML ring closes before CreateLinearRing admits it.
    static Geometry? Lower(KmlDom.Geometry? geometry) => geometry switch {
        KmlDom.Point p when p.Coordinate is { } v => GeoServices.Factory.CreatePoint(Coord(v)),
        KmlDom.LineString l => GeoServices.Factory.CreateLineString(l.Coordinates.AsIterable().Map(Coord).ToArray()),
        KmlDom.LinearRing r => GeoServices.Factory.CreatePolygon(Ring(r)),
        KmlDom.Polygon poly when poly.OuterBoundary?.LinearRing is { } shell =>
            GeoServices.Factory.CreatePolygon(
                Ring(shell),
                poly.InnerBoundary.AsIterable().Map(static h => h.LinearRing).Somes().Map(Ring).ToArray()),
        KmlDom.MultipleGeometry multi =>
            GeoServices.Factory.CreateGeometryCollection(multi.Geometry.AsIterable().Map(Lower).Somes().ToArray()),
        _ => null,
    };

    static Coordinate Coord(Vector v) => new(v.Longitude, v.Latitude, v.Altitude ?? double.NaN);

    static LinearRing Ring(KmlDom.LinearRing ring) {
        Coordinate[] shell = ring.Coordinates.AsIterable().Map(Coord).ToArray();
        return GeoServices.Factory.CreateLinearRing(
            shell.Length >= 3 && !shell[0].Equals2D(shell[^1]) ? [.. shell, shell[0]] : shell);
    }

    // Indexer sets, never Add: a KML Data row legitimately named "name"/"id" must overwrite, not throw.
    static IAttributesTable Attributes(KmlDom.Placemark mark) {
        var table = new AttributesTable { ["name"] = mark.Name ?? "", ["id"] = mark.Id ?? "" };
        if (mark.Description?.Text is { Length: > 0 } text) { table["description"] = text; }
        mark.ExtendedData?.Data.AsIterable().Iter(d => { if (d.Name is { Length: > 0 } name) { table[name] = d.Value ?? ""; } });
        mark.ExtendedData?.SchemaData.AsIterable()
            .Bind(static s => s.SimpleData.AsIterable())
            .Iter(d => { if (d.Name is { Length: > 0 } name) { table[name] = d.Text ?? ""; } });
        return table;
    }

    // Symmetric bare-KML egress the GeoVectorSource.Kml encode column binds: one Document of unstyled
    // Placemarks with ExtendedData — the styled/overlay/tour presentation is Site's, never re-spelled here.
    internal static byte[] Write(Seq<GeoFeature> features) {
        var document = new KmlDom.Document { Name = "features" };
        features.Iter(f => document.AddFeature(Mark(f, Option<string>.None)));
        using var output = new MemoryStream();
        KmlFile.Create(new KmlDom.Kml { Feature = document }, duplicates: false).Save(output);
        return output.ToArray();
    }

    // Bare-KMZ egress the GeoVectorSource.Kmz encode column binds: the Write document zipped through
    // KmzFile.Create/Save — the styled ortho/tour KMZ stays Site's, never re-spelled here.
    internal static byte[] WriteKmz(Seq<GeoFeature> features) {
        var document = new KmlDom.Document { Name = "features" };
        features.Iter(f => document.AddFeature(Mark(f, Option<string>.None)));
        using var kmz = KmzFile.Create(KmlFile.Create(new KmlDom.Kml { Feature = document }, duplicates: false));
        using var output = new MemoryStream();
        kmz.Save(output);
        return output.ToArray();
    }

    // Styled KMZ site emit: the feature set reprojects onto the Wgs84 frame through the ONE datum leg (KML is
    // always geographic), styles land ONCE as shared Document styles the placemarks reference by #id StyleUrl, the
    // attribute bag rides ExtendedData, the ortho drapes as a GroundOverlay over its 4326 LatLonBox with the image
    // archived beside doc.kml (KmzFile.AddFile), and the ordered tour stops become a gx:Tour Playlist of FlyTo
    // primitives framed by each stop's CalculateLookAt. styleOf routes a feature to its style row — a new symbology
    // is one styles row + routing, never a second emit path.
    public static Fin<byte[]> Site(
        Seq<GeoFeature> features,
        Func<GeoFeature, string> styleOf,
        Map<string, (Color32 Line, double WidthPx, Color32 Fill)> styles,
        Option<(byte[] Png, Envelope Bounds)> ortho,
        Seq<GeoFeature> tour,
        Op key) =>
        GeoServices.Wgs84
            .Bind(frame => features.Traverse(f => f.Reproject(frame, key)).As()
                .Bind(wgs => tour.Traverse(t => t.Reproject(frame, key)).As().Map(route => (Wgs: wgs, Route: route))))
            .Bind(site => Try.lift(() => {
                var document = new KmlDom.Document { Name = "site-context" };
                styles.Iter((id, row) => document.AddStyle(new KmlDom.Style {
                    Id = id,
                    Line = new KmlDom.LineStyle { Color = row.Line, Width = row.WidthPx },
                    Polygon = new KmlDom.PolygonStyle { Color = row.Fill, Fill = true, Outline = true },
                }));
                site.Wgs.Iter(f => document.AddFeature(Mark(f, Some(styleOf(f)))));
                ortho.Iter(o => document.AddFeature(new KmlDom.GroundOverlay {
                    Icon = new KmlDom.Icon { Href = new Uri(OrthoEntry, UriKind.Relative) },
                    Bounds = new KmlDom.LatLonBox { North = o.Bounds.MaxY, South = o.Bounds.MinY, East = o.Bounds.MaxX, West = o.Bounds.MinX },
                    DrawOrder = -1,
                }));
                if (!site.Route.IsEmpty) { document.AddFeature(TourOf(site.Route)); }
                var kml = new KmlDom.Kml { Feature = document };
                kml.AddNamespacePrefix("gx", "http://www.google.com/kml/ext/2.2");
                using var kmz = KmzFile.Create(KmlFile.Create(kml, duplicates: false));
                ortho.Iter(o => kmz.AddFile(OrthoEntry, o.Png));
                using var output = new MemoryStream();
                kmz.Save(output);
                return output.ToArray();
            }).Run().MapFail(error => new BimFault.CodecReject(key, $"geo-kml-site:{error.Message}")));

    static KmlDom.Placemark Mark(GeoFeature feature, Option<string> styleId) {
        var mark = new KmlDom.Placemark {
            Name = feature.Attr("name").Map(static v => v.ToString() ?? "").IfNone(""),
            Geometry = Raise(feature.Geometry),
            StyleUrl = styleId.Map(static id => new Uri($"#{id}", UriKind.Relative)).IfNoneUnsafe((Uri?)null),
        };
        var data = new KmlDom.ExtendedData();
        feature.Attributes.GetNames().AsIterable()
            .Iter(name => data.AddData(new KmlDom.Data { Name = name, Value = feature.Attributes[name]?.ToString() ?? "" }));
        mark.ExtendedData = data;
        return mark;
    }

    // NTS -> KML DOM: the inverse bridge (Coordinate(X,Y) -> Vector(lat: Y, lon: X)); a polygon carries its holes
    // as InnerBoundary rings, a collection recurses through MultipleGeometry.
    static KmlDom.Geometry Raise(Geometry geometry) => geometry switch {
        Point p => new KmlDom.Point { Coordinate = new Vector(p.Y, p.X) },
        LineString l => new KmlDom.LineString { Coordinates = Vectors(l.Coordinates) },
        Polygon poly => RaisePolygon(poly),
        GeometryCollection collection => RaiseCollection(collection),
        var other => new KmlDom.Point { Coordinate = new Vector(other.Centroid.Y, other.Centroid.X) },
    };

    static KmlDom.Polygon RaisePolygon(Polygon polygon) {
        var raised = new KmlDom.Polygon {
            Tessellate = true,
            OuterBoundary = new KmlDom.OuterBoundary { LinearRing = new KmlDom.LinearRing { Coordinates = Vectors(polygon.ExteriorRing.Coordinates) } },
        };
        for (int i = 0; i < polygon.NumInteriorRings; i++) {
            raised.AddInnerBoundary(new KmlDom.InnerBoundary { LinearRing = new KmlDom.LinearRing { Coordinates = Vectors(polygon.GetInteriorRingN(i).Coordinates) } });
        }
        return raised;
    }

    static KmlDom.MultipleGeometry RaiseCollection(GeometryCollection collection) {
        var multi = new KmlDom.MultipleGeometry();
        for (int i = 0; i < collection.NumGeometries; i++) { multi.AddGeometry(Raise(collection.GetGeometryN(i))); }
        return multi;
    }

    static KmlDom.CoordinateCollection Vectors(Coordinate[] coordinates) =>
        new(coordinates.Select(static c => new Vector(c.Y, c.X)));

    // Each stop frames itself: CalculateLookAt derives the camera from the stop's own bounds, so the fly-through
    // needs no hand-authored viewpoints; the 3-second dwell is the FlyTo Duration.
    static Tour TourOf(Seq<GeoFeature> route) {
        var playlist = new Playlist();
        route.Iter(stop => playlist.AddTourPrimitive(new FlyTo { Duration = 3.0, View = Mark(stop, Option<string>.None).CalculateLookAt() }));
        return new Tour { Name = "site-tour", Playlist = playlist };
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoVector {
    // ONE decode entry over the source table's delegate column — zero call-site branch; the arm throws on a corrupt
    // container and the rail lowers it onto the typed codec fault, the source key riding the detail.
    public static Fin<Seq<GeoFeature>> Read(GeoVectorSource source, ReadOnlyMemory<byte> bytes, Option<Envelope> clip, Op key) =>
        Try.lift(() => source.Decode(bytes, clip)).Run()
            .MapFail(error => new BimFault.CodecReject(key, $"geo-vector:{source.Key}:{error.Message}"));

    // Managed FlatGeobuf arm pushes the clip DOWN through the Packed-Hilbert-R-tree bbox index so a
    // continental .fgb decodes only the overlapping feature runs — the managed equivalent of the GDAL OGR
    // Layer.SetSpatialFilterRect, never an Ogr.Open over /vsimem. A remote .fgb escalates to Stream below.
    internal static Seq<GeoFeature> FlatGeobuf(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        using var fgb = new MemoryStream(bytes.ToArray());
        var rect = clip.MatchUnsafe(env => env, () => null);
        return global::FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(fgb, rect).AsIterable()
            .Map(f => new GeoFeature(f.Geometry, f.Attributes, Option<ProjectedCrs>.None)).ToSeq();
    }

    // Remote-.fgb range-read escalation: fetch is the consumer's byte-range read (HTTP Range / object-store
    // seek). Helpers.ReadHeader consumes the 8-byte magic + 4-byte size prefix + headerSize blob off the range
    // stream, so the index seats at 12 + headerSize (the codec's own Deserialize seeks exactly that);
    // PackedRTree.StreamSearch walks ONLY the index nodes it needs (its ReadNode offsets are index-relative,
    // re-based here), CalcSize seats the feature body, and each hit reads its 4-byte record length then fetches
    // that record body alone — FromByteBuffer lowers the PREFIX-STRIPPED buffer (GetRootAsFeature reads the root
    // offset at the buffer position) against the canonical factory and the packed sequence layout. A continental
    // remote .fgb thus answers a window query in a handful of range reads, never a whole-file pull.
    public static Fin<Seq<GeoFeature>> Stream(PackedRTree.ReadNode fetch, Envelope window, Op key) =>
        Try.lift(() => {
            using var head = fetch(0, HeaderProbeBytes);
            var header = global::FlatGeobuf.Helpers.ReadHeader(head, out int headerSize);
            var schema = header.UnPack();
            ulong indexOrigin = 12uL + (ulong)headerSize;
            ulong bodyOrigin = indexOrigin + PackedRTree.CalcSize(header.FeaturesCount, header.IndexNodeSize);
            var sequences = new global::FlatGeobuf.NTS.FlatGeobufCoordinateSequenceFactory();
            var hits = PackedRTree.StreamSearch(header.FeaturesCount, header.IndexNodeSize, window,
                (offset, length) => fetch(indexOrigin + offset, length));
            var features = Seq<GeoFeature>();
            Span<byte> prefix = stackalloc byte[4];
            foreach ((ulong offset, ulong _) in hits) {
                using (var sized = fetch(bodyOrigin + offset, 4)) { sized.ReadExactly(prefix); }
                uint length = BinaryPrimitives.ReadUInt32LittleEndian(prefix);
                var bytes = new byte[length];
                using (var record = fetch(bodyOrigin + offset + 4, length)) { record.ReadExactly(bytes); }
                IFeature feature = global::FlatGeobuf.NTS.FeatureConversions.FromByteBuffer(
                    GeoServices.Factory, sequences, new global::Google.FlatBuffers.ByteBuffer(bytes), schema);
                features = features.Add(new GeoFeature(feature.Geometry, feature.Attributes, Option<ProjectedCrs>.None));
            }
            return features;
        }).Run().MapFail(error => new BimFault.CodecReject(key, $"geo-fgb-stream:{error.Message}"));

    // One range read covers magic + the size-prefixed header for any real-world schema; ReadHeader stops at the
    // header end regardless of the over-fetch.
    const ulong HeaderProbeBytes = 1UL << 16;

    // Managed GeoParquet COLUMNAR arm: ReadColumns is the COLUMN push-down (the columnar analog of the FGB
    // bbox ROW push-down), the geo column holds WKB the GeoWkb bridge crosses — the SAME bridge the OGR universal
    // arm crosses. GeoParquet carries no server-side bbox filter, so the clip filters client-side.
    internal static Seq<GeoFeature> GeoParquet(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.parquet");
        File.WriteAllBytes(path, bytes.ToArray());
        try {
            var meta = GISBlox.IO.GeoParquet.GeoParquetReader.ReadGeoMetadata(path);
            var primary = meta?.Primary_column ?? "geometry";
            var table = GISBlox.IO.GeoParquet.GeoParquetReader.ReadAll(path, GISBlox.IO.GeoParquet.Common.GeometryFormat.WKB);
            var features = table.AsEnumerable().AsIterable()
                .Map(row => new GeoFeature(GeoWkb.ToNts((byte[])row[primary]), RowAttributes(table, row, primary), Option<ProjectedCrs>.None))
                .ToSeq();
            return clip.Match(None: () => features, Some: env => features.Filter(f => f.Bounds.Intersects(env)));
        } finally { File.Delete(path); }
    }

    static IAttributesTable RowAttributes(System.Data.DataTable table, System.Data.DataRow row, string primary) {
        var attributes = new AttributesTable();
        foreach (System.Data.DataColumn column in table.Columns) {
            if (column.ColumnName != primary) {
                attributes.Add(column.ColumnName, row.IsNull(column) ? "" : row[column]);
            }
        }
        return attributes;
    }

    internal static Seq<GeoFeature> GeoJson(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        var collection = JsonSerializer.Deserialize<FeatureCollection>(bytes.Span, GeoWire.Json) ?? new FeatureCollection();
        var features = collection.ToSeq().Map(f => new GeoFeature(f.Geometry, f.Attributes, Option<ProjectedCrs>.None));
        return clip.Match(None: () => features, Some: env => features.Filter(f => f.Bounds.Intersects(env)));
    }

    // A shapefile is the .shp/.shx/.dbf/.prj QUARTET — one byte buffer carries it as the standard zip transport,
    // discriminated on the zip magic exactly like the KMZ arm; a bare .shp buffer degrades to geometry-only. The
    // .dbf stream feeds OpenRead so attributes survive (a Stream.Null dbf silently drops every attribute — the
    // deleted form), and the .prj WKT tags the seam SourceCrs so a projected shapefile datum-shifts at projection
    // instead of silently landing raw ordinates on the model frame.
    internal static Seq<GeoFeature> Shapefile(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        var options = new ShapefileReaderOptions {
            Factory = GeoServices.Factory,
            MbrFilter = clip.MatchUnsafe(env => env, () => null),
            GeometryBuilderMode = GeometryBuilderMode.FixInvalidShapes,
        };
        var (shpBytes, dbfBytes, prjText) = bytes.Span is [0x50, 0x4B, ..] ? UnzipQuartet(bytes) : (bytes.ToArray(), (byte[]?)null, "");
        Option<ProjectedCrs> crs = prjText.Length == 0
            ? Option<ProjectedCrs>.None
            : ProjectedCrs.Of("", "", "", prjText, Op.Of(name: nameof(GeoVector)))
                .Match(Succ: static c => Some(c), Fail: static _ => Option<ProjectedCrs>.None);
        using var shp = new MemoryStream(shpBytes);
        using Stream dbf = dbfBytes is null ? Stream.Null : new MemoryStream(dbfBytes);
        using var reader = NetTopologySuite.IO.Esri.Shapefile.OpenRead(shp, dbf, options);
        return reader.AsIterable().Map(feature => new GeoFeature(feature.Geometry, feature.Attributes, crs)).ToSeq();
    }

    // Zip-quartet split: entries resolve by extension (any base name); a zip with no .shp is corrupt and throws
    // into the Read Try rail; absent .dbf/.prj degrade (geometry-only / no CRS), never fault.
    static (byte[] Shp, byte[]? Dbf, string Prj) UnzipQuartet(ReadOnlyMemory<byte> bytes) {
        using var archive = new ZipArchive(new MemoryStream(bytes.ToArray()), ZipArchiveMode.Read);
        byte[]? Entry(string extension) {
            if (archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(extension, StringComparison.OrdinalIgnoreCase)) is not { } entry) { return null; }
            using var source = entry.Open();
            using var buffer = new MemoryStream();
            source.CopyTo(buffer);
            return buffer.ToArray();
        }
        return (Entry(".shp") ?? throw new InvalidDataException("shapefile-zip-missing-shp"),
                Entry(".dbf"),
                Entry(".prj") is { } prj ? Encoding.UTF8.GetString(prj) : "");
    }

    internal static Seq<GeoFeature> CityJson(ReadOnlyMemory<byte> bytes) {
        var document = Newtonsoft.Json.JsonConvert.DeserializeObject<CityJSON.CityJsonDocument>(Encoding.UTF8.GetString(bytes.Span))!;
        // CityJSON metadata.referenceSystem is an OGC URN (urn:ogc:def:crs:EPSG::25832) the seam ProjectedCrs.Of
        // admits on its Name carrier (the three-state [ComplexValueObject] takes name/mapProjection/mapZone/wkt, never a
        // single-string overload — that spelling is the deleted phantom). A blank referenceSystem yields the no-CRS None
        // (Of would fault the all-blank product, so the blank case never enters admission); a present URN structurally
        // admits, the per-feature reprojection deriving its EPSG through the seam Resolution. The Op is the keyless
        // ingest re-key the codec arm carries.
        var system = document.Metadata?.ReferenceSystem ?? "";
        Option<ProjectedCrs> crs = system.Length == 0
            ? Option<ProjectedCrs>.None
            : ProjectedCrs.Of(system, "", "", "", Op.Of(name: nameof(GeoVector)))
                .Match(Succ: static c => Some(c), Fail: static _ => Option<ProjectedCrs>.None);
        return document.CityObjects.ToSeq()
            .Map(pair => new GeoFeature(Boundary(document, pair.Value), Attributes(pair.Key, pair.Value), crs));
    }

    // Per-CityObject planar footprint: the highest-LoD geometry's boundary vertex indices dereference into the
    // document Vertices pool and dequantize through Transform, then fold into the planar convex hull. A
    // metadata-only object or a degenerate sub-3-point hull falls back to the document AABB.
    static Geometry Boundary(CityJSON.CityJsonDocument document, CityJSON.CityObject city) {
        var scale = document.Transform.ScaleVector3();
        var translate = document.Transform.TranslateVector3();
        var hull = city.Geometry
            .OrderByDescending(static g => g.Lod ?? "")
            .AsIterable()
            .Head
            .Map(geometry => LeafIndices(geometry)
                .Map(i => new Coordinate(
                    document.Vertices[i].X * scale.X + translate.X,
                    document.Vertices[i].Y * scale.Y + translate.Y,
                    document.Vertices[i].Z * scale.Z + translate.Z))
                .ToArray());
        return hull.Filter(static cs => cs.Length >= 3)
            .Map(static cs => GeoServices.Factory.CreateMultiPointFromCoords(cs).ConvexHull())
            .IfNone(() => { var (envelope, _, _) = document.GetVerticesEnvelope(); return GeoServices.Factory.ToGeometry(envelope); });
    }

    static Seq<int> LeafIndices(CityJSON.Geometry.Geometry geometry) {
        System.Array? boundaries = geometry switch {
            CityJSON.Geometry.SolidGeometry s            => s.Boundaries,
            CityJSON.Geometry.MultiSurfaceGeometry m     => m.Boundaries,
            CityJSON.Geometry.CompositeSurfaceGeometry c => c.Boundaries,
            CityJSON.Geometry.MultiSolidGeometry m       => m.Boundaries,
            CityJSON.Geometry.CompositeSolidGeometry c   => c.Boundaries,
            _                                            => null,
        };
        return boundaries is null ? Seq<int>() : Flatten(boundaries).ToSeq();
    }

    static IEnumerable<int> Flatten(System.Array array) {
        foreach (var item in array) {
            if (item is int leaf) { yield return leaf; }
            else if (item is System.Array nested) { foreach (int x in Flatten(nested)) { yield return x; } }
        }
    }

    static IAttributesTable Attributes(string id, CityJSON.CityObject city) =>
        new AttributesTable(city.Attributes ?? new Dictionary<string, object>()) { ["id"] = id, ["type"] = city.Type.ToString() };

    // Ogr.Open auto-detects the container, so the universal decode carries no driver token; every geometry crosses the
    // ONE GeoWkb bridge.
    internal static Seq<GeoFeature> Universal(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        GeoGdal.Bootstrap();
        string path = $"/vsimem/{Guid.NewGuid():N}";
        OSGeo.GDAL.Gdal.FileFromMemBuffer(path, bytes.ToArray());
        try {
            using var data = OSGeo.OGR.Ogr.Open(path, 0);
            var features = Seq<GeoFeature>();
            for (int l = 0; l < data.GetLayerCount(); l++) {
                var layer = data.GetLayerByIndex(l);
                clip.Iter(env => layer.SetSpatialFilterRect(env.MinX, env.MinY, env.MaxX, env.MaxY));
                layer.ResetReading();
                for (var feature = layer.GetNextFeature(); feature is not null; feature = layer.GetNextFeature()) {
                    features = features.Add(new GeoFeature(GeoWkb.ToNts(feature.GetGeometryRef()), AttributesOf(feature), Option<ProjectedCrs>.None));
                }
            }
            return features;
        } finally { OSGeo.GDAL.Gdal.Unlink(path); }
    }

    static IAttributesTable AttributesOf(OSGeo.OGR.Feature feature) {
        var table = new AttributesTable();
        for (int f = 0; f < feature.GetFieldCount(); f++) {
            var defn = feature.GetFieldDefnRef(f);
            table.Add(defn.GetName(), feature.IsFieldSet(f) ? feature.GetFieldAsString(f) : "");
        }
        return table;
    }

    // Symmetric egress dispatches the SAME row's ENCODE delegate column Read's decode rides — the managed
    // codecs and the styled SharpKml arm emit directly, the OGR rows pin their driver; CityJson's encode column
    // throws by row law (ingest-only — a planar GeoFeature set cannot re-emit a 3D city model).
    public static Fin<byte[]> Write(GeoVectorSource source, Seq<GeoFeature> features, Option<ProjectedCrs> crs, Op key) =>
        Try.lift(() => source.Encode(features, crs)).Run()
            .MapFail(error => new BimFault.CodecReject(key, $"geo-vector-write:{source.Key}:{error.Message}"));

    internal static byte[] WriteFlatGeobuf(Seq<GeoFeature> features) {
        using var output = new MemoryStream();
        var kind = features.Head
            .Map(static f => global::FlatGeobuf.NTS.GeometryConversions.ToGeometryType(f.Geometry))
            .IfNone(global::FlatGeobuf.GeometryType.Unknown);
        global::FlatGeobuf.NTS.FeatureCollectionConversions.Serialize(
            output, features.Map(static f => (IFeature)new Feature(f.Geometry, f.Attributes)), kind, dimensions: 3, columns: null);
        return output.ToArray();
    }

    internal static byte[] WriteGeoParquet(Seq<GeoFeature> features) {
        const string geoColumn = "geometry";
        var table = new System.Data.DataTable();
        table.AddGeoColumn(geoColumn, 0, GISBlox.IO.GeoParquet.Common.GeometryFormat.WKB);
        table.Columns[geoColumn]!.SetAsPrimaryGeoColumn();
        var names = features.Bind(static f => f.Attributes.GetNames().ToSeq()).Distinct().ToSeq();
        names.Iter(name => table.Columns.Add(name, typeof(string)));
        table.AddGeoProcessingMetadata([geoColumn], geoColumn);
        features.Iter(f => {
            var row = table.NewRow();
            row[geoColumn] = GeoWkb.FromNts(f.Geometry);
            names.Iter(name => row[name] = f.Attr(name).Map(static v => v.ToString() ?? "").IfNone(""));
            table.Rows.Add(row);
        });
        using var output = new MemoryStream();
        GISBlox.IO.GeoParquet.GeoParquetWriter.Write(output, table, geoColumn);
        return output.ToArray();
    }

    // Shapefile egress is the QUARTET zipped into ONE archive buffer (the transport the read arm discriminates):
    // returning the bare .shp alone strands the .shx offset index and the .dbf attributes — an unreadable fragment,
    // a deleted form. .prj carries the seam CRS: the inline Wkt when a GIS-origin CRS defined it, else the
    // authority Name (an EPSG/URN form ProjNET/OSR re-resolve to WKT on read).
    internal static byte[] WriteShapefile(Seq<GeoFeature> features, Option<ProjectedCrs> crs) {
        using var shp = new MemoryStream();
        using var shx = new MemoryStream();
        using var dbf = new MemoryStream();
        using var prj = new MemoryStream();
        NetTopologySuite.IO.Esri.Shapefile.WriteAllFeatures(
            features.Map(static f => (IFeature)new Feature(f.Geometry, f.Attributes)),
            shp, shx, dbf, prj, crs.Map(static c => c.Wkt.Length > 0 ? c.Wkt : c.Name).IfNone(""), null);
        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true)) {
            foreach (var (name, part) in new[] { ("features.shp", shp), ("features.shx", shx), ("features.dbf", dbf), ("features.prj", prj) }) {
                if (part.Length == 0) { continue; }
                using var entry = archive.CreateEntry(name).Open();
                entry.Write(part.GetBuffer().AsSpan(0, (int)part.Length));
            }
        }
        return output.ToArray();
    }

    internal static byte[] WriteGeoJson(Seq<GeoFeature> features) {
        var collection = new FeatureCollection();
        features.Iter(f => collection.Add(new Feature(f.Geometry, f.Attributes)));
        return JsonSerializer.SerializeToUtf8Bytes(collection, GeoWire.Json);
    }

    // GDAL OGR universal egress (GeoPackage/GML/FileGDB) writes the row's pinned driver to a REAL temp file
    // then File.ReadAllBytes — this GDAL SWIG build exposes only VSIFWriteL(string, ...) and NO byte[] VSIFReadL,
    // so a /vsimem byte read-back has no handle-level primitive; the managed temp-file read-back is the correct
    // egress (the SAME temp-file pattern the GeoParquet/CityJSON arms use).
    internal static byte[] WriteUniversal(string ogrDriver, Seq<GeoFeature> features, Option<ProjectedCrs> crs) {
        GeoGdal.Bootstrap();
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}");
        using (var driver = OSGeo.OGR.Ogr.GetDriverByName(ogrDriver))
        using (var data = driver.CreateDataSource(path, [])) {
            using var srs = crs.Match(Some: SpatialRef, None: () => (OSGeo.OSR.SpatialReference?)null);
            using var layer = data.CreateLayer("features", srs, OSGeo.OGR.wkbGeometryType.wkbUnknown, []);
            using var defn = layer.GetLayerDefn();
            features.Iter(f => {
                using var feature = new OSGeo.OGR.Feature(defn);
                using var geom = GeoWkb.ToOgr(f.Geometry);
                feature.SetGeometry(geom);
                layer.CreateFeature(feature);
            });
        }
        try { return File.ReadAllBytes(path); } finally { File.Delete(path); }
    }

    static OSGeo.OSR.SpatialReference SpatialRef(ProjectedCrs crs) {
        var srs = new OSGeo.OSR.SpatialReference("");
        // SetFromUserInput is OSR's universal CRS parser (an EPSG/URN authority code OR an inline WKT); the seam
        // three-state ProjectedCrs carries the inline Wkt when present, else the authority Name (no single-string
        // .Value member exists on the [ComplexValueObject] — that spelling is the deleted phantom).
        srs.SetFromUserInput(crs.Wkt.Length > 0 ? crs.Wkt : crs.Name);
        srs.SetAxisMappingStrategy(OSGeo.OSR.AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
        return srs;
    }
}
```

## [04]-[RASTER_INGEST]

- Owner: `GeoRaster` the GDAL raster ingest owner over `MaxRev.Gdal.Core` — a windowed multi-band `Dataset.ReadRaster<T>` stack placed in georeferenced space by the six-coefficient affine RE-ANCHORED to the pixel window and resample ratio (the tile's own affine, its NTS extent folded off that affine's corners; stamping the source affine on a windowed/resampled buffer mislocates the tile, and the SWIG `Dataset.GetExtent` takes the OGR `Envelope`, never the NTS type), with the `Contour`/`Cog`/`DemProcess` DEM-to-vector and hillshade/slope/aspect legs; `RasterTile` the windowed pixel carrier — the polymorphic `RasterBand` `[Union]` typed by the source `Band.DataType`, the full six-coefficient geo-transform, the NTS extent, the per-band self-describing `RasterBandInfo` schema, the base tile dims, and the `RasterOverview` pyramid; `GeoRaster.ToCoverage` the seam `Coverage`-node projection lowering a placed MULTI-RESOLUTION raster by content-key reference, never a stored pixel blob on the node and never a single-resolution descriptor that strands a COG/DEM pyramid.
- Entry: `GeoRaster.Read(bytes, window, targetWidth, targetHeight, key)` opens the raster through `Gdal.Open` over a `/vsimem/` buffer and reads the windowed band STACK and every per-band schema (value envelope, palette legend, overview pyramid) into a `RasterTile` ONCE at ingest, faulting `BimFault.CodecReject` off `key` on an open/read fault; `GeoRaster.ToCoverage(tile, reference, field, overviewKey, ctx)` wraps the placed MULTI-RESOLUTION raster into a `Geospatial/coverage#COVERAGE_NODE` `CoverageGrid` and lands a CONTENT-hashed `Node.Coverage`, `CoverageGrid.Of` railing `ElementFault.ValueRejected` on any degenerate/duplicate/non-coarsening/hollow-palette/unknown-token grid; `GeoRaster.Contour` vectorizes the DEM, `GeoRaster.Cog` transcodes to a Cloud-Optimized GeoTIFF, and `GeoRaster.DemProcess(demBytes, mode, key)` derives hillshade/slope/aspect, each carrying its `Op key`.
- Auto: `GeoRaster.Read` runs `GeoGdal.Bootstrap` once, re-anchors the `GetGeoTransform` affine to the pixel window and resample ratio, folds the NTS extent off its four corners, and lowers each band's full schema (the value envelope in priority order — the stored `GetMinimum`/`GetMaximum` flag, else `ComputeRasterMinMax` — and a palette band's clamped colour-and-category legend); `ToCoverage` maps the `GridDescriptor` POSITIONALLY off the six-coefficient geo-transform (the two rotation terms first-class and the SIGNED pixel-height preserved, so a north-up raster's negative `CellSizeY` is valid and degeneracy is the zero-determinant test, never a sign check), content-keys each overview level by `overviewKey`, and lands a NON-ROOTED `Node.Coverage` whose `NodeId` is CONTENT-hashed over its own canonical bytes (the diff/dedup projection — pyramid, CRS, and per-band decode/range/palette all folded into the identity), never a rooted mint and never an inlined pixel buffer; the placement composes the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg, escalating to OSR only for the exotic datum-grid transforms `ProjNET` cannot express.
- Receipt: the `RasterTile` is the placed pixel evidence a terrain-mesh tessellation reads; the seam `Coverage` node is the by-reference field the terrain consumer and the `Exchange/export` 3D-Tiles terrain leg read — its `OverviewLevel` pyramid letting a `Rasm.Compute` working-resolution route pick a level by `LevelFor`, size the fetch by `ByteLength(level)`, and read that decimated level's bytes by its own `RasterKey` rather than the full base raster; its per-band `Range` the display-normalization envelope read from metadata alone and its `Palette` the indexed-band colour-and-category legend `CoverageBand.Decode` resolves; the contour `GeoFeature` lines are the vectorized terrain the site model indexes.
- Packages: `MaxRev.Gdal.Core`, `MaxRev.Gdal.MacosRuntime.Minimal.arm64`, `NetTopologySuite`, `ProjNET`, `CommunityToolkit.HighPerformance`, `Rasm.Element`, `Rasm`, `LanguageExt.Core`
- Growth: a new raster format is enumerable through the one `Gdal.Open` universal driver path with zero new code; a new DEM derivation is one `wrapper_GDALDEMProcessing` mode; a new resample kernel is one `RasterIOExtraArg`; a new resolution tier is one `RasterOverview` row off the existing `GetOverviewCount` fold lowered to one `OverviewLevel`; a new band attribute (a statistic, a histogram bin) is one `RasterBandInfo` column lowered to one `CoverageBand` column; the seam projection is one `ToCoverage` op; never a per-format raster reader, never an inlined pixel blob on the node, never a single-resolution tile that strands the pyramid, and never a `Palette`-role band with no colour table behind it.
- Boundary: the raster ingest is `MaxRev.Gdal.Core`'s — `GdalBase.ConfigureAll()` MUST run once before any `OSGeo.*` call and a publish without the matching RID runtime faults at first call onto `BimFault.CodecReject` (the `Model/faults#FAULT_BAND` band the `geo-raster`/`geo-vector`/`geo-contour` details route, never `CapabilityMiss` — that band is the `Semantics/georeference#GEODETIC_TRANSFORM` leg's); pixels move through `Dataset.ReadRaster<T>` into a managed `T[]` matching the `Band.DataType` and a hand-rolled raster decoder is the deleted form; reprojection inside a GDAL pipeline uses OSR while managed-geometry reprojection stays the `ProjNET` leg, OSR escalating only the exotic datum-grid transforms; the seam `Coverage` node references the field by content key and an inlined pixel blob on the node is the deleted form; a coverage is MULTI-RESOLUTION so `ToCoverage` reads the pyramid and content-keys each level — a single-resolution descriptor that drops the COG/DEM overview set and forces a full-base fetch is the deleted form, and the coarser-than-base + strictly-monotone gate is `CoverageGrid.Of`'s the projector cannot violate without `ElementFault.ValueRejected`; a band is FULLY self-describing so an envelope-less band and a `Palette`-role band with an EMPTY colour table (the hollow channel) are the deleted forms the seam `CoverageBand` contract forbids; every `ColorTable`/`RasterAttributeTable`/`ColorEntry` SWIG handle is read under `using` and only the lowered `ColorBin` rows cross onto the seam, never a live GDAL handle; the tile-pyramid PARTITIONING (building new overview levels) stays at `Rasm.Compute` — `Rasm.Bim` AUTHORS the COG/contour and READS the existing GDAL overview pyramid, the 3D-Tiles terrain leg crossing the seam, never re-deriving the pyramid.

```csharp signature
// --- [COMPOSITION] ------------------------------------------------------------------------
// GeoGdal runs the mandatory once-per-process GDAL bootstrap: ConfigureAll registers every GDAL+OGR driver and
// resolves the gdal-data/PROJ paths from the RID runtime package; UseExceptions flips the SWIG error model to thrown.
public static class GeoGdal {
    static readonly Lock Gate = new();

    public static void Bootstrap() {
        lock (Gate) {
            if (!MaxRev.Gdal.Core.GdalBase.IsConfigured) {
                MaxRev.Gdal.Core.GdalBase.ConfigureAll();
                OSGeo.GDAL.Gdal.UseExceptions();
                OSGeo.OGR.Ogr.UseExceptions();
                OSGeo.OSR.Osr.UseExceptions();
            }
        }
    }
}

// --- [MODELS] -----------------------------------------------------------------------------
// RasterBand types the band buffer by the source Band.DataType — GDT_Byte->byte[] (ortho), GDT_Float16/32->float[] (DEM),
// GDT_Float64->double[] (survey-grade DEM — narrowing it to float is the same precision-loss defect the datum leg
// forbids on eastings), the integer widths->int[] (classification rasters) — so a multi-band read carries its
// true pixel type. SampleAt is the erased float convenience; a precision-true read rides Plane's typed continuation.
[Union]
public partial record RasterBand {
    partial record Floats(float[] Samples);
    partial record Doubles(double[] Samples);
    partial record Bytes(byte[] Samples);
    partial record Ints(int[] Samples);

    public int Length => Switch(
        floats:  static b => b.Samples.Length,
        doubles: static b => b.Samples.Length,
        bytes:   static b => b.Samples.Length,
        ints:    static b => b.Samples.Length);

    public float SampleAt(int i) => Switch<float>(
        floats:  b => b.Samples[i],
        doubles: b => (float)b.Samples[i],
        bytes:   b => b.Samples[i],
        ints:    b => b.Samples[i]);
}

// Placed windowed raster: the typed pixel band-stack, the FULL six-coefficient geo-transform + NTS extent, the
// per-band self-describing schema, the base-raster Band.GetBlockSize tile dims (lowered onto CoverageGrid.BaseBlockX/Y
// so a tiled-COG base read aligns the same way an overview read does), and the GDAL overview PYRAMID a COG/tiled DEM
// carries (each level its own decimated read off Band.GetOverview(i)) — so ToCoverage lowers a MULTI-RESOLUTION
// coverage, never the single-resolution descriptor that strands a COG/DEM pyramid and forces a full-base fetch.
public sealed record RasterTile(
    RasterBand Band,
    int Width,
    int Height,
    double[] GeoTransform,
    Envelope Extent,
    Seq<RasterBandInfo> Bands,
    Seq<RasterOverview> Overviews,
    int BaseBlockX,
    int BaseBlockY) {

    // Zero-copy per-band plane over the band-sequential stack (CommunityToolkit.HighPerformance AsMemory2D at the
    // band offset): a DEM sampler or display-normalization pass addresses [row, col] at the TRUE pixel type —
    // continuation-per-case shape IS the union dispatch, so no erased-to-float copy and no per-type member spam.
    public T Plane<T>(int band, Func<ReadOnlyMemory2D<float>, T> floats, Func<ReadOnlyMemory2D<double>, T> doubles, Func<ReadOnlyMemory2D<byte>, T> bytes, Func<ReadOnlyMemory2D<int>, T> ints) =>
        Band.Switch(
            floats:  s => floats(s.Samples.AsMemory().AsMemory2D(band * Width * Height, Height, Width, 0)),
            doubles: s => doubles(s.Samples.AsMemory().AsMemory2D(band * Width * Height, Height, Width, 0)),
            bytes:   s => bytes(s.Samples.AsMemory().AsMemory2D(band * Width * Height, Height, Width, 0)),
            ints:    s => ints(s.Samples.AsMemory().AsMemory2D(band * Width * Height, Height, Width, 0)));
}

// One GDAL overview level a multi-resolution raster carries (the band-1 Band.GetOverview(i) the COG/tiled DEM holds):
// GetOverview index Level (the intrinsic level key the caller's overviewKey resolves to the persisted level blob's
// content key), the level dimensions (strictly coarser than the base — CoverageGrid.Of enforces), the decimated cell
// size (the base CellSize scaled by the Width-decimation ratio, the scalar LevelFor compares a target resolution
// against), and the level's Band.GetBlockSize tile dims (0 = untiled/strip). The per-level pixel bytes are
// content-keyed at ToCoverage, this carrier holding only the level SCHEMA the seam OverviewLevel takes by-reference.
public sealed record RasterOverview(
    int Level,
    int Width,
    int Height,
    double CellSize,
    int BlockX,
    int BlockY);

// RasterBandInfo captures the per-band GDAL schema once at ingest so ToCoverage lowers a TYPED, FULLY self-describing
// CoverageBand without re-opening the dataset: pixel DataType (-> RasterSampleType token), ColorInterp (-> BandRole
// token), OPTIONAL NoData sentinel (GDAL hasval flag lowered to Option<double>, never a NaN sentinel), unit string,
// Offset/Scale linear decode (GDAL GetOffset/GetScale, identity 0.0/1.0 when unset), OPTIONAL (Min,Max) value envelope
// (GetMinimum/GetMaximum's stored flag, else ComputeRasterMinMax — the Range a display-normalization consumer reads
// from metadata alone, never an envelope-less band), and the ColorBin Palette legend a GCI_PaletteIndex band carries
// (GetRasterColorTable ColorEntry quads paired with the GetDefaultRAT GFU_Name category or GetCategoryNames — never a
// hollow Palette role with no table behind it). OSGeo.GDAL.* types stay confined to this carrier the GeoRaster owner
// reads and ToCoverage lowers — they never cross to the seam node.
public sealed record RasterBandInfo(
    int Index,
    OSGeo.GDAL.DataType DataType,
    OSGeo.GDAL.ColorInterp ColorInterp,
    Option<double> NoData,
    string Units,
    double Offset,
    double Scale,
    Option<(double Min, double Max)> Range,
    Seq<ColorBin> Palette);

// Bounded DEM-derivation vocabulary the GeoRaster.DemProcess leg lowers to the gdal_dem mode token (ToString
// lowercased: "hillshade"/"slope"/"aspect") — a bounded enum, never a free mode string, so a new derivation is one row.
public enum DemMode : byte { Hillshade = 0, Slope = 1, Aspect = 2 }

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoRaster {
    public static Fin<RasterTile> Read(ReadOnlyMemory<byte> bytes, Option<Envelope> window, int targetWidth, int targetHeight, Op key) =>
        Try.lift(() => {
            GeoGdal.Bootstrap();
            string path = $"/vsimem/{Guid.NewGuid():N}.tif";
            OSGeo.GDAL.Gdal.FileFromMemBuffer(path, bytes.ToArray());
            try {
                using var dataset = OSGeo.GDAL.Gdal.Open(path, OSGeo.GDAL.Access.GA_ReadOnly);
                var transform = new double[6];
                dataset.GetGeoTransform(transform);
                var (xOff, yOff, xSize, ySize) = Pixels(window, transform, dataset.RasterXSize, dataset.RasterYSize);
                // Tile's OWN affine: origin re-anchored to the pixel window, cell scaled by the read-time resample
                // ratio (identity for a full native read) — stamping the SOURCE affine on a windowed/resampled buffer
                // silently mislocates the tile downstream. The NTS extent folds off THIS affine's four corners
                // (rotation honored); the SWIG Dataset.GetExtent takes the OGR Envelope, never the NTS type.
                var (rx, ry) = ((double)xSize / targetWidth, (double)ySize / targetHeight);
                double[] gt = [
                    transform[0] + (xOff * transform[1]) + (yOff * transform[2]), transform[1] * rx, transform[2] * ry,
                    transform[3] + (xOff * transform[4]) + (yOff * transform[5]), transform[4] * rx, transform[5] * ry];
                (double X, double Y) Corner(double c, double r) => (gt[0] + (c * gt[1]) + (r * gt[2]), gt[3] + (c * gt[4]) + (r * gt[5]));
                var extent = new Envelope();
                Span<(double X, double Y)> corners = [Corner(0, 0), Corner(targetWidth, 0), Corner(0, targetHeight), Corner(targetWidth, targetHeight)];
                foreach (var (cx, cy) in corners) { extent.ExpandToInclude(cx, cy); }
                int bands = dataset.RasterCount;
                var bandMap = Enumerable.Range(1, bands).ToArray();
                using var first = dataset.GetRasterBand(1);
                var dataType = first.DataType;
                var band = Materialize(dataset, dataType, xOff, yOff, xSize, ySize, targetWidth, targetHeight, bands, bandMap);
                Seq<RasterBandInfo> schema = Enumerable.Range(1, bands).AsIterable().Map(b => BandInfo(dataset.GetRasterBand(b), b - 1)).ToSeq();
                // Base-raster tile dims (GetBlockSize) lower onto CoverageGrid.BaseBlockX/Y so the full-resolution
                // base read aligns to tiles the same way an overview read does; the band-1 overview PYRAMID
                // (GetOverviewCount/GetOverview(i), decreasing-resolution by GDAL contract) folds into the RasterOverview
                // set the seam OverviewLevel takes by-reference — a COG/tiled DEM yields its decimated levels here.
                first.GetBlockSize(out int baseBlockX, out int baseBlockY);
                // Overview cell sizes decimate the SOURCE raster, so baseCell reads the source affine, not the tile's.
                double baseCell = Math.Sqrt(Math.Abs((transform[1] * transform[5]) - (transform[2] * transform[4])));
                Seq<RasterOverview> overviews = Overviews(first, dataset.RasterXSize, baseCell);
                return new RasterTile(band, targetWidth, targetHeight, gt, extent, schema, overviews, baseBlockX, baseBlockY);
            } finally { OSGeo.GDAL.Gdal.Unlink(path); }
        }).Run().MapFail(error => new BimFault.CodecReject(key, $"geo-raster:{error.Message}"));

    // ToCoverage [M1] projects the seam Coverage: the placed MULTI-RESOLUTION raster lands a Node.Coverage wrapping a CoverageGrid
    // that holds the field BY REFERENCE (the base RasterKey content key to the pixel buffer in the object store, NEVER
    // inlined pixels), the FULL six-coefficient affine GridDescriptor mapped POSITIONALLY off the GDAL geo-transform
    // [originX, pxW, rowRot, originY, colRot, pxH] (the two rotation terms first-class, the SIGNED pxH preserved — a
    // north-up raster's negative CellSizeY is valid; degeneracy is the zero-determinant test CoverageGrid.Of enforces,
    // never a sign check), the OverviewLevel PYRAMID each level its own overviewKey(o.Level) content key + GetBlockSize
    // tile dims (so a working-resolution consumer fetches a decimated level by key, never the full base), the base
    // BaseBlockX/Y tile dims (symmetric with each overview's), and one TYPED, FULLY self-describing CoverageBand per band
    // (RasterSampleType.Parse over the DataType token, BandRole over the ColorInterp, the optional GDAL NoData, the unit
    // string, the Offset/Scale decode, the (Min,Max) Range envelope, and the ColorBin Palette legend for a palette band).
    // CoverageGrid.Of rails ElementFault.ValueRejected on a degenerate grid, an empty band set, a duplicate band index, a
    // decode-degenerate band, an overview level set not strictly coarsening (wider-than-base dims, a cell size not
    // exceeding the base cell, or a non-monotone order), or a Palette band whose legend is empty (the hollow-palette gate) or carries
    // duplicate indices, and an unknown pixel token rails through Parse; the field UInt128 is the content key of the
    // persisted base pixel buffer (one XxHash128 seed).
    // Coverage is a NON-ROOTED resource node, so its NodeId is CONTENT-hashed over the node's OWN canonical bytes
    // (the H7 projection the diff/dedup shares, the id excluded — the pyramid, the CRS, and each band's decode/range/
    // palette folded into the identity), never a rooted mint — two models sharing the same raster+affine+pyramid+CRS dedup
    // to one node, the element.md non-rooted-resource policy this folder owns no second hasher for.
    public static Fin<Node.Coverage> ToCoverage(RasterTile tile, GeoReference reference, UInt128 field, Func<int, UInt128> overviewKey, ProjectionContext ctx) =>
        tile.Bands
            .Traverse(info => RasterSampleType.Parse(SampleToken(info.DataType), ctx.Key).Map(sample =>
                new CoverageBand(info.Index, $"band{info.Index}", sample, Role(info.ColorInterp, info.Palette), info.NoData, info.Units, info.Offset, info.Scale, info.Range, info.Palette)))
            .As()
            .Bind(bands => CoverageGrid.Of(
                kind:       CoverageKind.Raster,
                rasterKey:  field,
                grid:       new GridDescriptor(
                                tile.GeoTransform[0], tile.GeoTransform[1], tile.GeoTransform[2],
                                tile.GeoTransform[3], tile.GeoTransform[4], tile.GeoTransform[5],
                                tile.Width, tile.Height),
                bands:      bands,
                crs:        reference,
                key:        ctx.Key,
                // Each RasterOverview level mints its OWN content key (overviewKey(o.Level) = the persisted level blob's
                // RasterKey keyed by the GetOverview index), so a working-resolution consumer fetches a decimated level
                // by key rather than the base raster; CoverageGrid.Of enforces the coarser-than-base + strictly-monotone
                // pyramid invariants (a non-monotone or upsampled level set is unrepresentable, not merely rejected late).
                overviews:  tile.Overviews.Map(o => new OverviewLevel(o.Width, o.Height, o.CellSize, overviewKey(o.Level), o.BlockX, o.BlockY)),
                baseBlockX: tile.BaseBlockX,
                baseBlockY: tile.BaseBlockY))
            .Map(grid => {
                Node.Coverage draft = new(NodeId.Content(default), grid);
                return draft with { Id = NodeId.Content(draft.ToCanonicalBytes(ctx.Header.Tolerance).Span) };
            });

    // GDAL pixel-type lowering the typed CoverageBand schema rides: the DataType token strips the GDT_ prefix to the
    // RasterSampleType key (an unknown pixel type rails ValueRejected through Parse) — never a stringly DataType on the seam.
    static string SampleToken(OSGeo.GDAL.DataType dataType) =>
        dataType.ToString().Replace("GDT_", "").ToLowerInvariant();

    static readonly Map<OSGeo.GDAL.ColorInterp, string> RoleToken = Map(
        (OSGeo.GDAL.ColorInterp.GCI_GrayIndex,    BandRole.Gray.Key),
        (OSGeo.GDAL.ColorInterp.GCI_PaletteIndex, BandRole.Palette.Key),
        (OSGeo.GDAL.ColorInterp.GCI_RedBand,      BandRole.Red.Key),
        (OSGeo.GDAL.ColorInterp.GCI_GreenBand,    BandRole.Green.Key),
        (OSGeo.GDAL.ColorInterp.GCI_BlueBand,     BandRole.Blue.Key),
        (OSGeo.GDAL.ColorInterp.GCI_AlphaBand,    BandRole.Alpha.Key));

    // GDAL ColorInterp -> BandRole lowering: resolve through the generated TryGet over the frozen role table, defaulting
    // Undefined for the HSL/CMYK/YCbCr channels a coverage consumer does not read. A GCI_PaletteIndex band whose
    // GetRasterColorTable is null lowers with an EMPTY palette, which would land a HOLLOW Palette role the seam
    // <coverage-palette-empty> gate rejects — so an empty-legend palette band DOWNGRADES to Undefined (its raw indices
    // still read through Real), keeping "a Palette role never crosses the seam without its legend" an ENFORCED invariant
    // rather than a claim; a genuine palette band (non-empty table) keeps its Palette role and its ColorBin legend.
    static BandRole Role(OSGeo.GDAL.ColorInterp colorInterp, Seq<ColorBin> palette) {
        BandRole role = BandRole.TryGet(RoleToken.Find(colorInterp).IfNone(BandRole.Undefined.Key), out BandRole? resolved) && resolved is { } r ? r : BandRole.Undefined;
        return role == BandRole.Palette && palette.IsEmpty ? BandRole.Undefined : role;
    }

    // FULL 2x2 affine inverse (rotation terms honored): all four window corners map through it and the pixel
    // window is their min/max hull — an axis-only `(X-gt0)/gt1` division silently misreads a rotated raster while the
    // GridDescriptor downstream celebrates the preserved rotation. A zero-determinant affine is degenerate and
    // reads the full raster (CoverageGrid.Of rejects it at projection).
    static (int XOff, int YOff, int XSize, int YSize) Pixels(Option<Envelope> window, double[] gt, int rasterX, int rasterY) =>
        window.Filter(_ => (gt[1] * gt[5]) - (gt[2] * gt[4]) != 0.0).Match(
            None: () => (0, 0, rasterX, rasterY),
            Some: env => {
                double det = (gt[1] * gt[5]) - (gt[2] * gt[4]);
                (double Col, double Row) Invert(double x, double y) =>
                    (((gt[5] * (x - gt[0])) - (gt[2] * (y - gt[3]))) / det,
                     ((gt[1] * (y - gt[3])) - (gt[4] * (x - gt[0]))) / det);
                Span<(double Col, double Row)> corners =
                    [Invert(env.MinX, env.MinY), Invert(env.MinX, env.MaxY), Invert(env.MaxX, env.MinY), Invert(env.MaxX, env.MaxY)];
                var (c0, c1, r0, r1) = (double.MaxValue, double.MinValue, double.MaxValue, double.MinValue);
                foreach (var (col, row) in corners) {
                    (c0, c1, r0, r1) = (Math.Min(c0, col), Math.Max(c1, col), Math.Min(r0, row), Math.Max(r1, row));
                }
                int x0 = Math.Clamp((int)Math.Floor(c0), 0, rasterX);
                int y0 = Math.Clamp((int)Math.Floor(r0), 0, rasterY);
                int x1 = Math.Clamp((int)Math.Ceiling(c1), 0, rasterX);
                int y1 = Math.Clamp((int)Math.Ceiling(r1), 0, rasterY);
                return (x0, y0, Math.Max(1, x1 - x0), Math.Max(1, y1 - y0));
            });

    static RasterBand Materialize(
        OSGeo.GDAL.Dataset dataset, OSGeo.GDAL.DataType dataType,
        int xOff, int yOff, int xSize, int ySize, int width, int height, int bands, int[] bandMap) =>
        dataType switch {
            OSGeo.GDAL.DataType.GDT_Byte =>
                new RasterBand.Bytes(ReadStack<byte>(dataset, xOff, yOff, xSize, ySize, width, height, bands, bandMap)),
            OSGeo.GDAL.DataType.GDT_Int16 or OSGeo.GDAL.DataType.GDT_UInt16
                or OSGeo.GDAL.DataType.GDT_Int32 or OSGeo.GDAL.DataType.GDT_UInt32 =>
                new RasterBand.Ints(ReadStack<int>(dataset, xOff, yOff, xSize, ySize, width, height, bands, bandMap)),
            OSGeo.GDAL.DataType.GDT_Float64 =>
                new RasterBand.Doubles(ReadStack<double>(dataset, xOff, yOff, xSize, ySize, width, height, bands, bandMap)),
            _ =>
                new RasterBand.Floats(ReadStack<float>(dataset, xOff, yOff, xSize, ySize, width, height, bands, bandMap)),
        };

    static T[] ReadStack<T>(
        OSGeo.GDAL.Dataset dataset, int xOff, int yOff, int xSize, int ySize, int width, int height, int bands, int[] bandMap)
        where T : struct {
        var buffer = new T[width * height * bands];
        dataset.ReadRaster(xOff, yOff, xSize, ySize, buffer, width, height, bands, bandMap, 0, 0, 0);
        return buffer;
    }

    // BandInfo lowers the full per-band GDAL schema to a RasterBandInfo at read time (index 0-based): pixel DataType and
    // ColorInterp host enums, the optional NoData (the GDAL out-int hasval flag lowered to Option<double>, never a
    // NaN sentinel), the unit string, the Offset/Scale linear decode (GetOffset/GetScale return the 0.0/1.0 identity
    // when unset), the optional (Min,Max) value envelope (EnvelopeOf reads the stored flag, else computes), and the
    // ColorBin Palette legend a GCI_PaletteIndex band carries (PaletteOf, empty for a non-palette band). ToCoverage
    // lowers each row to a typed self-describing seam CoverageBand, so the GDAL surface stops here.
    static RasterBandInfo BandInfo(OSGeo.GDAL.Band band, int index) {
        band.GetNoDataValue(out double noData, out int hasNoData);
        band.GetOffset(out double offset, out int _);
        band.GetScale(out double scale, out int _);
        return new RasterBandInfo(
            Index:       index,
            DataType:    band.DataType,
            ColorInterp: band.GetColorInterpretation(),
            NoData:      hasNoData != 0 ? Some(noData) : Option<double>.None,
            Units:       band.GetUnitType() ?? "",
            Offset:      offset,
            Scale:       scale,
            Range:       EnvelopeOf(band),
            Palette:     PaletteOf(band));
    }

    // Per-band value envelope -> CoverageBand.Range: GetMinimum/GetMaximum read the STORED metadata flag first
    // (hasval==1 ⇒ the source carries the envelope) and fall to ComputeRasterMinMax (approxOk sampling a decimated
    // overview, not the full base raster) only when the stored flag is clear, so a display-normalization consumer reads the
    // envelope from metadata alone — never an envelope-less band the consumer must scan pixels to normalize. A
    // compute that defeats GDAL (an empty band) leaves None rather than faulting the whole read.
    static Option<(double Min, double Max)> EnvelopeOf(OSGeo.GDAL.Band band) {
        band.GetMinimum(out double storedMin, out int hasMin);
        band.GetMaximum(out double storedMax, out int hasMax);
        if (hasMin != 0 && hasMax != 0) { return Some((storedMin, storedMax)); }
        var minMax = new double[2];
        return Try.lift(() => { band.ComputeRasterMinMax(minMax, approx_ok: 1); return Some((minMax[0], minMax[1])); })
            .Run().IfFail(Option<(double, double)>.None);
    }

    // A GCI_PaletteIndex band's colour-and-category legend -> CoverageBand.Palette: iterate the GetRasterColorTable
    // ColorEntry quads (null table ⇒ no palette ⇒ empty Seq, the non-Palette band's None-equivalent), clamp each
    // short c1..c4 into the 0-255 byte domain, and pair each index with its class label — the GetDefaultRAT GFU_Name
    // column row (the richer RAT legend) when the band ships one, else the lighter GetCategoryNames[index] array, else
    // blank. Every ColorTable/RasterAttributeTable/ColorEntry SWIG handle frees under `using` at this boundary so only the
    // lowered ColorBin rows cross onto the seam, never a live GDAL handle; an empty result (a null table) is the
    // non-palette None-equivalent the Role lowering downgrades to BandRole.Undefined, so no hollow Palette reaches the seam.
    static Seq<ColorBin> PaletteOf(OSGeo.GDAL.Band band) {
        using var table = band.GetRasterColorTable();
        if (table is null) { return Seq<ColorBin>(); }
        using var rat = band.GetDefaultRAT();
        int nameColumn = rat is not null ? rat.GetColOfUsage(OSGeo.GDAL.RATFieldUsage.GFU_Name) : -1;
        string[] categories = band.GetCategoryNames() ?? [];
        int count = table.GetCount();
        return Enumerable.Range(0, count).AsIterable().Map(i => {
            using OSGeo.GDAL.ColorEntry entry = table.GetColorEntry(i);
            string category =
                rat is not null && nameColumn >= 0 && rat.GetRowOfValue(i) is int row and >= 0 ? rat.GetValueAsString(row, nameColumn)
                : i < categories.Length                                                       ? categories[i]
                : "";
            return new ColorBin(i, Clamp(entry.c1), Clamp(entry.c2), Clamp(entry.c3), Clamp(entry.c4), category);
        }).ToSeq();
    }

    // Band-1 GDAL overview PYRAMID -> the RasterOverview set (the seam OverviewLevel takes it by-reference): the
    // GetOverviewCount levels, each GetOverview(i) carrying its dimensions, its decimated CellSize (the base CellSize
    // scaled by the Width-decimation ratio, the rotation-aware scalar LevelFor compares against), and its GetBlockSize
    // tile dims; GDAL's GetOverview(i) is decreasing-resolution by contract, so the natural index order satisfies the
    // CoverageGrid.Of strictly-monotone gate. The per-level pixel bytes are content-keyed at ToCoverage by Level.
    static Seq<RasterOverview> Overviews(OSGeo.GDAL.Band band, int baseWidth, double baseCell) =>
        Enumerable.Range(0, band.GetOverviewCount()).AsIterable().Map(i => {
            using OSGeo.GDAL.Band level = band.GetOverview(i);
            level.GetBlockSize(out int blockX, out int blockY);
            double scale = level.XSize > 0 ? (double)baseWidth / level.XSize : 1.0;
            return new RasterOverview(i, level.XSize, level.YSize, baseCell * scale, blockX, blockY);
        }).ToSeq();

    static byte Clamp(short channel) => (byte)Math.Clamp((int)channel, 0, 255);

    public static Fin<Seq<GeoFeature>> Contour(ReadOnlyMemory<byte> demBytes, double interval, Op key) =>
        Try.lift(() => {
            GeoGdal.Bootstrap();
            string source = $"/vsimem/{Guid.NewGuid():N}.tif";
            string sink = $"/vsimem/{Guid.NewGuid():N}.shp";
            OSGeo.GDAL.Gdal.FileFromMemBuffer(source, demBytes.ToArray());
            try {
                using var dem = OSGeo.GDAL.Gdal.Open(source, OSGeo.GDAL.Access.GA_ReadOnly);
                var options = new OSGeo.GDAL.GDALContourOptions(["-i", interval.ToString(CultureInfo.InvariantCulture), "-a", "elev"]);
                using var contoured = OSGeo.GDAL.Gdal.wrapper_GDALContourDestName(sink, dem, options, null, null);
                var features = Seq<GeoFeature>();
                for (int l = 0; l < contoured.GetLayerCount(); l++) {
                    var layer = contoured.GetLayerByIndex(l);
                    layer.ResetReading();
                    for (var feature = layer.GetNextFeature(); feature is not null; feature = layer.GetNextFeature()) {
                        features = features.Add(new GeoFeature(GeoWkb.ToNts(feature.GetGeometryRef()),
                            new AttributesTable { ["type"] = "contour", ["elev"] = feature.GetFieldAsDouble("elev") }, Option<ProjectedCrs>.None));
                    }
                }
                return features;
            } finally { OSGeo.GDAL.Gdal.Unlink(source); OSGeo.GDAL.Gdal.Unlink(sink); }
        }).Run().MapFail(error => new BimFault.CodecReject(key, $"geo-contour:{error.Message}"));

    // GeoTIFF -> Cloud-Optimized GeoTIFF transcode (the gdal_translate COG driver): the input rides /vsimem, the
    // tiled+overviewed COG output a REAL temp file File.ReadAllBytes recovers (no byte[] VSIFReadL in this SWIG build);
    // COG output feeds the Exchange/export 3D-Tiles terrain leg and a cloud raster store.
    public static Fin<byte[]> Cog(ReadOnlyMemory<byte> bytes, Op key) =>
        Try.lift(() => {
            GeoGdal.Bootstrap();
            string source = $"/vsimem/{Guid.NewGuid():N}.tif";
            string sink = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tif");
            OSGeo.GDAL.Gdal.FileFromMemBuffer(source, bytes.ToArray());
            try {
                using var src = OSGeo.GDAL.Gdal.Open(source, OSGeo.GDAL.Access.GA_ReadOnly);
                var options = new OSGeo.GDAL.GDALTranslateOptions(["-of", "COG", "-co", "COMPRESS=DEFLATE", "-co", "OVERVIEWS=AUTO"]);
                using (OSGeo.GDAL.Gdal.wrapper_GDALTranslate(sink, src, options, null, null)) { }
                return File.ReadAllBytes(sink);
            } finally { OSGeo.GDAL.Gdal.Unlink(source); File.Delete(sink); }
        }).Run().MapFail(error => new BimFault.CodecReject(key, $"geo-cog:{error.Message}"));

    // DEM derivation leg (the gdal_dem hillshade/slope/aspect algorithm): the bounded DemMode lowers to the GDAL
    // mode token, the input rides /vsimem, the derived single-band GeoTIFF a REAL temp file File.ReadAllBytes recovers;
    // a new derivation is one DemMode row, and the result re-enters Read/ToCoverage as a content-keyed field.
    public static Fin<byte[]> DemProcess(ReadOnlyMemory<byte> demBytes, DemMode mode, Op key) =>
        Try.lift(() => {
            GeoGdal.Bootstrap();
            string source = $"/vsimem/{Guid.NewGuid():N}.tif";
            string sink = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.tif");
            OSGeo.GDAL.Gdal.FileFromMemBuffer(source, demBytes.ToArray());
            try {
                using var dem = OSGeo.GDAL.Gdal.Open(source, OSGeo.GDAL.Access.GA_ReadOnly);
                var options = new OSGeo.GDAL.GDALDEMProcessingOptions(["-of", "GTiff", "-co", "COMPRESS=DEFLATE"]);
                using (OSGeo.GDAL.Gdal.wrapper_GDALDEMProcessing(sink, dem, mode.ToString().ToLowerInvariant(), null, options, null, null)) { }
                return File.ReadAllBytes(sink);
            } finally { OSGeo.GDAL.Gdal.Unlink(source); File.Delete(sink); }
        }).Run().MapFail(error => new BimFault.CodecReject(key, $"geo-dem:{error.Message}"));
}
```

## [05]-[RESEARCH]

(none)

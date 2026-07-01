# [BIM_GEOSPATIAL]

The georeferenced-BIM site-context PROJECTOR over the `Rasm.Element` seam: one `GeoFeature` canonical row (the OGC Simple-Features `NetTopologySuite` `Geometry` plus its `AttributesTable` and seam `ProjectedCrs`) the whole geospatial seam materializes, a `GeoModel` carrying the feature set under one `NtsGeometryServices.Instance` precision/SRID configuration and the `STRtree` broad-phase 2D index, the `GeoVector`/`GeoRaster` ingest folds admitting every admitted vector and raster source onto that one carrier, and the `GeoFeature.ToObject`/`GeoRaster.ToCoverage` projection that lowers a vector feature onto a seam `Object` node and a raster coverage onto a seam `Coverage` node [M1] through a `GraphDelta` the `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` fold composes — the geospatial source a projector registered at the app composition root [§6] alongside the IFC `Projection/semantic#SEMANTIC_PROJECTOR`, never folded BY it (that projector captures only a GeometryGym `DatabaseIfc`, never a GIS source). A vector feature RIDES an `Object` node — a parcel, a terrain TIN, or a city building lands as an `Object` occurrence discriminated by the generic `Classification("ifc", code)` like any imported element, never a parallel `Feature`/`GeoElement` family [§4B]. A raster coverage lands a `Coverage` node (a `CoverageGrid` by-ref + bands + the seam `GeoReference` CRS) [M1], never a stored pixel blob on the element. The vector ingest (shapefile/GeoJSON/CityJSON/FlatGeobuf/GeoParquet managed codecs plus the GDAL/OGR universal long-tail) and the raster ingest (GeoTIFF/COG/DEM windowed `ReadRaster<T>`) are Bim's NTS/GDAL capability the projector composes. The page is HOST-NEUTRAL: NTS owns the 2D planar geometry, the kernel `Rasm` owns the 3D solid geometry, the seam owns the node vocabulary, and the three meet only at the in-process WKB/`CoordinateSequence` kernel wire and the content-keyed seam node — a RhinoCommon binding on a geospatial owner is the named seam violation.

## [01]-[INDEX]

- [01]-[GEOSPATIAL_SEAM]: `GeoFeature` canonical row, `GeoModel` feature set + `STRtree` broad-phase, the `NtsGeometryServices.Instance` precision/SRID root, the planar predicate/overlay/spatial-join surface, the `GeoClassifier` `(OgcGeometryType, tag)`→`(IFC class, predefined)` table, and the `GeoFeature.ToObject`/`GeoModel.Project` site-context seam `Object`-node projection through a `GraphDelta`.
- [02]-[VECTOR_INGEST]: `GeoVector` fold over the `GeoVectorSource` `[SmartEnum]` — the managed shapefile/GeoJSON/CityJSON + FlatGeobuf (row-oriented, bbox push-down) + GeoParquet (columnar, column push-down) arms and the GeoPackage/OGR-universal arm producing `GeoFeature` rows, the OGR↔NTS WKB bridge, the server-side push-down, and the symmetric write side.
- [03]-[RASTER_INGEST]: `GeoRaster` GDAL raster ingest (GeoTIFF/COG/DEM windowed `ReadRaster<T>`), the geo-transform/extent placement, the per-band self-describing schema (`Range` value-envelope + `ColorBin` `Palette` legend) and the multi-resolution `RasterOverview` pyramid read at ingest, the DEM contour/hillshade vectorization to `GeoFeature`, the COG transcode, and the `GeoRaster.ToCoverage` seam `Coverage`-node projection [M1] lowering the typed bands + overview pyramid + base tile dims onto the seam `CoverageGrid`.

## [02]-[GEOSPATIAL_SEAM]

- Owner: `GeoFeature` the one host-neutral geospatial row carrying its `NetTopologySuite.Geometries.Geometry` planar geometry, its `IAttributesTable` keyed property bag, and its `Option<ProjectedCrs>` seam source CRS, plus the `Attr` optional-read; `GeoModel` the feature set under one `GeometryFactory` resolved from `NtsGeometryServices.Instance`, carrying the lazily-built `STRtree<GeoFeature>` broad-phase 2D index and the `GeoFeature.ToObject`-folding `Project`; `GeoServices` the process-wide `NtsGeometryServices` configuration root set once with the robust `GeometryOverlay.NG` engine and the dense `PackedCoordinateSequenceFactory`; `GeoClassifier` the frozen `(OgcGeometryType, tag)`→`(IFC class, predefined)` table the site-context projection keys on, carrying the true IFC4.3 class string the seam `Classification` takes (never an `IfcClass` row — the Bim `Emit` gate validates against the roster).
- Entry: `GeoModel.Of(Seq<GeoFeature> features)` indexes the features into the `STRtree` once; `GeoModel.SpatialJoin(Geometry probe)` runs the canonical broad-then-narrow rail (`STRtree.Query` candidates, then `PreparedGeometryFactory.Prepare(probe).Intersects` per candidate); `GeoFeature.ToObject(GeoReference reference, ProjectionContext ctx)` projects one feature onto a seam `Object` occurrence node with a `GeoClassifier`-resolved `Classification.Create("ifc", classKey, "", "", None, None)` (the seam SIX-member `(System, Code, Edition, Source, EditionDate, Title)` factory — a synthetic entity-class mint carries no resolved edition/publisher/title) and a bare `PredefinedType` token, reprojecting the geometry into the project CRS through the `Semantics/georeference#GEODETIC_TRANSFORM` `GeoTransform.Reproject` leg, content-keying the reprojected footprint into the `Representations` map under the `FootPrint` key (the analytical surface the `Rasm.Compute` disciplines resolve one-hop by content key from the blob store, never an inline coordinate field on the node [M2]), and threading the `AttributesTable` onto a seam `Pset_SiteContext` `PropertySet` node linked by a neutral `Assign(PropertyDefinition)` edge — `Fin<GraphDelta>` aborts only on an EMPTY geometry (no footprint, `Model/faults#FAULT_BAND` `BimFault.UnmappedClass` lifted BARE off `ctx.Key`, never a `.ToError()` hop), every recognized feature classifying to the `IfcGeographicElement` catch-all rather than aborting the import; `GeoModel.Project(GeoReference, ProjectionContext)` folds the whole feature set into one header-less `GraphDelta` the seam `Rasm.Element/Projection/projection#PROJECTION_CONTRACT` `Assemble` fold composes onto the model graph (the geospatial source registered as its own projector, never the IFC `SemanticProjector`'s concern).
- Auto: `GeoServices.Configure` sets `NtsGeometryServices.Instance` once at module init behind an idempotency guard with `GeometryOverlay.NG` and a `PackedCoordinateSequenceFactory` so every reader resolves cached factories carrying the one canonical `PrecisionModel`/`SRID`; `GeoModel.Of` bulk-inserts each feature envelope into the `STRtree` (read-only after the first `Query`, lazily built); `GeoModel.Dissolve` folds a footprint set through `OverlayNGRobust.Union` after a `GeometryFixer.Fix` validity gate; `GeoFeature.ToObject` reads the `GeoClassifier` table for the IFC class string, reprojects the geometry double-precision `CoordinateSequence` through the `Semantics/georeference#GEODETIC_TRANSFORM` `GeoTransform.Reproject` leg into the project CRS, content-keys the reprojected footprint WKB through the kernel seed-zero `ContentHash.Of` for the `Object` node `Representations` map, and folds the `IAttributesTable` `GetNames` onto a seam `Pset_SiteContext` `PropertySet` node (the GeoJSON footprint riding one `PropertyValue.Text` so the cross-runtime `shapely`/`turf` peers decode it), linking the two by a neutral `Assign(PropertyDefinition)` edge (`Subject`=object, `Definition`=pset).
- Receipt: the `GeoFeature` is the typed planar-geometry evidence a site clash or a parcel-boundary setback reads; the `GeoModel.STRtree` index is the broad-phase candidate generator; the projected seam `Object` node is discriminated by the same generic `Classification` an imported element carries, so the seam `Bake` and the `Review/validation#IDS_FACETS` audit read a site-context model with no second selection surface; the raster `Coverage` node carries the field by-ref + bands + CRS the terrain consumer reads.
- Packages: `NetTopologySuite`, `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage`, `ProjNET`, `Rasm.Element`, `Rasm`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new planar predicate is one `Geometry` instance method on the existing algebra; a new spatial index is the `STRtree`/`Quadtree` swap the `GeoModel` carrier owns; a new site-context class mapping is one `GeoClassifier` table row keyed on `(OgcGeometryType, tag)`; a new attribute projection is one `Pset_SiteContext` `PropertyValue`; never a parallel planar geometry world beside NTS, never a per-feature-kind `GeoFeature` subtype, never a parallel `Feature`/`GeoElement` node beside the seam `Object`, and never a second precision/SRID configuration beside `NtsGeometryServices.Instance`.
- Boundary: the planar Simple-Features algebra is `NetTopologySuite`'s — the `Geometry` hierarchy, the DE-9IM predicates, the `OverlayNG` robust boolean, the `STRtree` index, and `PreparedGeometry` acceleration are the package's, and a hand-rolled planar intersection or a second R-tree is the deleted form; the `NtsGeometryServices.Instance` global is the single precision/SRID owner configured once and a per-call factory is the rejected form; validity repair enters through `GeometryFixer.Fix` before any overlay or write; the geodetic reprojection composes the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg over the seam `GeoReference` and a `NetTopologySuite`-side datum shift is the named seam violation; the 3D solid geometry stays the kernel `Rasm`'s and a geospatial owner carrying a RhinoCommon `Brep`/`Mesh` is the host-bound defect — NTS 2D planar geometry crosses to the kernel ONLY as a `CoordinateSequence` ordinate buffer (or its WKB form) the kernel constrained-Delaunay realize pass triangulates into the content-keyed geometry the `Object` node references, distinct from the cross-runtime GeoJSON peer wire; the site-context projection mints a seam `Object` node and a parallel `GeoElement`/`SiteElement` record beside it is the deleted form [§4B]; a raster coverage lands a seam `Coverage` node [M1] and a stored pixel blob on the element node is the deleted form; the `GeoClassifier` is a frozen data table keyed on `(OgcGeometryType, tag)`, never enumerated `switch` arms.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using GISBlox.IO.GeoParquet.Extensions;
using LanguageExt;
using NetTopologySuite;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.IO.Converters;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Operation.Union;
using Rasm;
using Rasm.Domain;
using Rasm.Element;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [SERVICES] ---------------------------------------------------------------------------
// The single planar-geometry configuration root: NtsGeometryServices.Instance is the one global
// PrecisionModel/SRID/overlay owner, configured once at module init with the robust OverlayNG engine and
// the dense PackedCoordinateSequenceFactory so every reader resolves cached factories carrying one precision.
public static class GeoServices {
    static readonly Lock Gate = new();
    static bool configured;

    public static GeometryFactory Factory => Configure().CreateGeometryFactory(Srid);

    public const int Srid = 4326;

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

    // The seam-node projection: a vector feature RIDES an Object node [§4B] carrying the generic Classification
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
            Map<PropertyName, PropertyValue> values = footprint.Attributes.GetNames().AsIterable()
                .Fold(Map<PropertyName, PropertyValue>(), (bag, name) =>
                    bag.AddOrUpdate(PropertyName.Create(name), new PropertyValue.Text(footprint.Attributes[name]?.ToString() ?? "")))
                .AddOrUpdate(PropertyName.Create("Footprint"), new PropertyValue.Text(GeoWire.ToGeoJson(footprint)));
            var pset = new Node.PropertySet(NodeId.Rooted(), new PropertyBag("Pset_SiteContext", values, InheritanceMode.OccurrenceWins));
            var obj = new Node.Object(
                Id:              objectId,
                Kind:            ObjectKind.Occurrence,
                ExternalId:      footprint.Attr("id").Bind(static v => v.ToString() is { Length: > 0 } id ? Some(id) : Option<string>.None),
                Classification:  Classification.Create("ifc", row.Class, "", "", None, None),
                PredefinedType:  PredefinedType.Create(row.Predefined),
                Name:            footprint.Attr("name").Map(static v => v.ToString() ?? "").Filter(static n => n.Length > 0).IfNone(row.Class),
                Tag:             footprint.Attr("id").Map(static v => v.ToString() ?? "").IfNone(""),
                Representations: RepresentationContentHash.Empty.With("FootPrint", ContentHash.Of(footprint.Geometry.AsBinary())),
                History:         Option<OwnerHistory>.None,
                Span:            SchemaSpan.From(ReleaseVersion.Ifc4X3Add2));
            return GraphDelta.Empty
                .Put(obj).Put(pset)
                .Link(new Relationship.Assign(objectId, pset.Id, AssignKind.PropertyDefinition));
        }));

    // The geodetic reprojection composes the Semantics/georeference#GEODETIC_TRANSFORM GeoTransform.Reproject leg — the
    // ONE ProjNET/OSR datum owner over the seam GeoReference, reprojecting a DOUBLE-precision ordinate Span<double> IN
    // PLACE (survey eastings never narrow to float). SourceCrs is the from-frame, the project reference the to-frame; the
    // leg is additive (an absent/equal source/target EPSG leaves the ordinates untouched so a single-datum site never
    // blocks) and faults bare off key only when both engines defeat a present, differing pair. Geometry.Coordinates is a
    // DETACHED snapshot, so the projector flattens it into the buffer, hands the buffer to the owner, and writes the
    // reprojected ordinates back through Geometry.Apply (the .api/api-nettopologysuite reprojection seam — the in-place
    // ordinate visitor); a NetTopologySuite-side datum shift (a filter that COMPUTES the transform) is the named seam
    // violation, distinct from this write-back visitor that COMPUTES nothing and only marshals the owner's output.
    Fin<GeoFeature> Reproject(GeoReference target, Op key) {
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
            return this with { Geometry = shifted };
        });
    }

    // The write-back visitor the reprojection seam rides: it COMPUTES no transform (the datum shift is GeoTransform's),
    // it is pure index-aligned marshalling, so it is NOT the deleted NTS-side-datum-shift filter. Geometry.Apply walks
    // the components in the SAME order Geometry.Coordinates enumerated, so the running cursor aligns the flat buffer with
    // each visited ordinate; GeometryChanged invalidates the cached envelope after the walk.
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

public sealed record GeoModel(Seq<GeoFeature> Features) {
    readonly STRtree<GeoFeature> index = Build(Features);

    // A `with` copy would alias the built-once STRtree against a different Features set, surfacing stale broad-phase
    // candidates from the wrong feature set (the Graph/element#ELEMENT_GRAPH frozen-snapshot guard, applied here) — so a
    // `with` copy is forbidden; only Of mints a fresh indexed model and the index rebuilds with it.
    private GeoModel(GeoModel original) =>
        throw new InvalidOperationException("GeoModel carries a built-once STRtree index and must not be copied via `with`; build one through GeoModel.Of.");

    public static GeoModel Of(Seq<GeoFeature> features) => new(features.Map(static f => f.Repaired));

    static STRtree<GeoFeature> Build(Seq<GeoFeature> features) {
        var tree = new STRtree<GeoFeature>(Math.Max(2, features.Count));
        features.Iter(f => tree.Insert(f.Bounds, f));
        return tree;
    }

    public Seq<GeoFeature> SpatialJoin(Geometry probe) {
        var prepared = PreparedGeometryFactory.Prepare(probe);
        return index.Query(probe.EnvelopeInternal).AsIterable().Filter(f => prepared.Intersects(f.Geometry)).ToSeq();
    }

    public Geometry Dissolve() =>
        OverlayNGRobust.Union(Features.Map(static f => f.Repaired.Geometry).ToArray());

    // The whole site-context feature set folds into ONE header-less GraphDelta the seam Assemble fold composes — each
    // feature's Object + PropertySet nodes and the Assign(PropertyDefinition) edge accumulate onto the graph in one apply.
    public Fin<GraphDelta> Project(GeoReference reference, ProjectionContext ctx) =>
        Features.Map(f => f.ToObject(reference, ctx)).Sequence()
            .Map(static deltas => deltas.Fold(GraphDelta.Empty, static (acc, delta) => acc.Merge(delta)));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoClassifier {
    // The frozen (geometry-kind, tag) -> (true IFC entity-type string, predefined token) site-context table — a data
    // table, never enumerated switch arms. The class is the TRUE IFC4.3 entity-type the seam Classification carries as
    // a library-neutral (system, code) pair, NOT a Model/elements#IFC_CLASS IfcClass row: the seam never validates the
    // class against the roster, and resolving IfcGeographicElement/IfcSite/IfcBuilding through IfcClass.TryGet would
    // collapse them to the Proxy fallback. The Bim Emit egress gate resolves the IfcClass row from this code and admits
    // the predefined token [C6], so a class the roster has not yet rostered round-trips to IFC only once it is added.
    // The "" tag rows are the per-kind generic fallback toward the IfcGeographicElement geographic-context catch-all.
    static readonly Map<(OgcGeometryType Kind, string Tag), (string Class, string Predefined)> Table =
        Map(
            ((OgcGeometryType.Polygon,      "building"),  ("IfcBuilding",          "NOTDEFINED")),
            ((OgcGeometryType.MultiPolygon, "building"),  ("IfcBuilding",          "NOTDEFINED")),
            ((OgcGeometryType.Polygon,      "parcel"),    ("IfcSite",              "NOTDEFINED")),
            ((OgcGeometryType.Polygon,      "landuse"),   ("IfcSite",              "NOTDEFINED")),
            ((OgcGeometryType.Polygon,      "relief"),    ("IfcGeographicElement", "TERRAIN")),
            ((OgcGeometryType.MultiPolygon, "relief"),    ("IfcGeographicElement", "TERRAIN")),
            ((OgcGeometryType.LineString,   "road"),      ("IfcCourse",            "PAVEMENT")),
            ((OgcGeometryType.LineString,   "rail"),      ("IfcRail",              "RAIL")),
            ((OgcGeometryType.LineString,   "contour"),   ("IfcGeographicElement", "TERRAIN")),
            ((OgcGeometryType.Point,        "tree"),      ("IfcGeographicElement", "VEGETATION")),
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
        return Fin.Succ(Table.Find((feature.Kind, tag))
            .OrElse(() => Table.Find((feature.Kind, "")))
            .IfNone(("IfcGeographicElement", "NOTDEFINED")));
    }
}
```

## [03]-[VECTOR_INGEST]

- Owner: `GeoVector` the universal vector ingest-and-egress fold over `GeoVectorSource`, the `[SmartEnum<string>]` source table whose rows carry the managed-versus-GDAL reader discriminant — `Shapefile` (`NetTopologySuite.IO.Esri.Shapefile`), `GeoJson` (`NetTopologySuite.IO.GeoJSON4STJ`), `CityJson` (`bertt.CityJSON`, ingest-only), `FlatGeobuf` (`FlatGeobuf.NTS`, the cloud-optimized row-oriented codec with the Packed-Hilbert-R-tree bbox push-down), and `GeoParquet` (`GISBlox.IO.GeoParquet` over `ParquetSharp`, the columnar `DataTable`↔WKB arm) decode through dedicated managed codecs, `GeoPackage` and the long-tail OGR-only drivers through the one `MaxRev.Gdal.Core` OGR universal reader — every arm producing the canonical `GeoFeature` row; `GeoWire` the `GeoFeature`'s two canonical wire projections per `data-interchange#GEO_INTERCHANGE` (GeoJSON text the cross-runtime `shapely`/`turf` peers decode + the GeoPackage binary blob the `csharp:Rasm.Persistence/Store` geo-store-blob persists); `GeoWkb` the bidirectional OGR↔NTS bridge the universal driver path crosses on both legs — the SAME `WKBReader.Read` the GeoParquet geo-column cell crosses.
- Entry: `GeoVector.Read(GeoVectorSource source, ReadOnlyMemory<byte> bytes, Option<Envelope> clip, Op key)` decodes a vector source onto `Seq<GeoFeature>` — the `Shapefile` arm through `Shapefile.OpenRead` (the `clip` driving `ShapefileReaderOptions.MbrFilter` server-side window push-down), the `CityJson` arm through a `Transform`-dequantized boundary fold per `CityObject`, the `FlatGeobuf` arm through `FeatureCollectionConversions.Deserialize(stream, rect)` (the `clip` pushed DOWN through the Packed-Hilbert-R-tree bbox index), the `GeoParquet` arm through `GeoParquetReader.ReadAll`/`ReadColumns` then a per-row `WKBReader.Read`, the universal arm through `Ogr.Open` over a `/vsimem/` buffer then `Layer.SetSpatialFilterRect`/`GetNextFeature`/`Feature.GetGeometryRef.ExportToWkb` → `WKBReader.Read` — `Fin<T>` aborts on a corrupt container (`Model/faults#FAULT_BAND` `BimFault.CodecReject` lifted BARE off `key`, never a `.ToError()` hop) at the boundary; `GeoVector.Write(GeoVectorSource source, Seq<GeoFeature> features, Option<ProjectedCrs> crs, Op key)` emits the symmetric byte payload, the universal egress writing the driver to a real temp file then `File.ReadAllBytes` (this GDAL SWIG build exposes no `byte[]` `VSIFReadL`, so a `/vsimem` byte read-back has no handle-level primitive).
- Auto: the `GeoVectorSource` row discriminant routes the decode without a call-site branch — the `Shapefile.OpenRead` `MbrFilter` skips records outside the `clip`; the `CityJson` arm reads `CityObject.Geometry[i]` discriminating `GeometryType` and recovering coordinates as `vertex × Transform.ScaleVector3() + Transform.TranslateVector3()`; the `FlatGeobuf` arm pushes the `clip` through the Packed-Hilbert-R-tree, a remote `.fgb` escalating to `PackedRTree.StreamSearch`; the `GeoParquet` arm `ReadGeoMetadata` reads the OGC `geo` header, `ReadColumns` projects only the needed columns server-side, each WKB cell bridging through `WKBReader.Read`; the universal arm runs `GeoGdal.Bootstrap` once, opens through `Gdal.FileFromMemBuffer` + `Ogr.Open`, pushes the `clip` through `Layer.SetSpatialFilterRect`, and bridges each `ExportToWkb` buffer through `WKBReader.Read`; every produced `GeoFeature` is `GeometryFixer.Fix`-repaired at admission.
- Receipt: the `GeoVector.Read` `Seq<GeoFeature>` is the universal vector ingest evidence the `GEOSPATIAL_SEAM` `GeoModel` indexes and the `GeoFeature.ToObject`/`GeoModel.Project` projection lowers onto seam `Object` nodes; the `GeoVectorSource` row records which codec decoded so the reader is one table read.
- Packages: `NetTopologySuite`, `NetTopologySuite.IO.Esri.Shapefile`, `bertt.CityJSON`, `FlatGeobuf`, `GISBlox.IO.GeoParquet`, `MaxRev.Gdal.Core`, `NodaTime`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new managed vector codec is one `GeoVectorSource` `managed:true` row; a new OGR-only long-tail format is enumerable through the existing universal `Ogr.Open` arm with zero new row; a new attribute push-down is one `Layer.SetAttributeFilter`/`ReadColumns` argument; never a per-format importer family, never a hand-rolled binary record, and never a boolean op on the OGR side.
- Boundary: the shapefile/FlatGeobuf/GeoParquet codecs are the pure-managed defaults and admitting GDAL for a format a managed codec reads is the rejected form; the managed codec output IS the canonical `NetTopologySuite.Features.Feature`; the OGR↔NTS bridge is the `ExportToWkb`→`WKBReader.Read` / `WKBWriter.Write`→`CreateFromWkb` wire — running planar boolean ops on the OGR side fragments the one topology owner; the `GdalBase.ConfigureAll()` bootstrap runs once per process behind the `IsConfigured` guard; `Gdal.UseExceptions()` flips the SWIG error model so a failed open lowers onto `BimFault.CodecReject`; the CityJSON quantization is lossless (integer indices into `Vertices`, recovered through `Transform`) and tessellating it in the codec is the deleted form; `CityJSON.*`/`OSGeo.*`/`FlatGeobuf.*`/`GISBlox.*` types never leak past this fold.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class GeoVectorSource {
    public static readonly GeoVectorSource Shapefile  = new("shapefile",  managed: true,  ogrDriver: "ESRI Shapefile");
    public static readonly GeoVectorSource GeoJson    = new("geojson",    managed: true,  ogrDriver: "GeoJSON");
    public static readonly GeoVectorSource CityJson   = new("cityjson",   managed: true,  ogrDriver: "CityJSON");
    public static readonly GeoVectorSource FlatGeobuf = new("flatgeobuf", managed: true,  ogrDriver: "FlatGeobuf");
    public static readonly GeoVectorSource GeoParquet = new("geoparquet", managed: true,  ogrDriver: "Parquet");
    public static readonly GeoVectorSource GeoPackage = new("geopackage", managed: false, ogrDriver: "GPKG");
    public static readonly GeoVectorSource Kml        = new("kml",        managed: false, ogrDriver: "KML");
    public static readonly GeoVectorSource Gml        = new("gml",        managed: false, ogrDriver: "GML");
    public static readonly GeoVectorSource FileGdb    = new("filegdb",    managed: false, ogrDriver: "OpenFileGDB");

    public bool Managed { get; }
    public string OgrDriver { get; }
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
// The GeoFeature's two canonical wire projections per data-interchange#GEO_INTERCHANGE: NetTopologySuite is the
// SINGLE interior geo vocabulary and GeoJSON text plus the GeoPackage binary blob are its ONLY two wire forms.
// The GeoJSON text wire is the cross-runtime geometry wire the Python shapely.from_geojson and TS turf peers
// decode; the GeoPackage blob is the Rasm.Persistence/Store geo-store-blob projection.
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

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoVector {
    public static Fin<Seq<GeoFeature>> Read(GeoVectorSource source, ReadOnlyMemory<byte> bytes, Option<Envelope> clip, Op key) =>
        Try.lift(() => source == GeoVectorSource.Shapefile  ? Shapefile(bytes, clip)
                     : source == GeoVectorSource.GeoJson    ? GeoJson(bytes, clip)
                     : source == GeoVectorSource.CityJson   ? CityJson(bytes)
                     : source == GeoVectorSource.FlatGeobuf ? FlatGeobuf(bytes, clip)
                     : source == GeoVectorSource.GeoParquet ? GeoParquet(bytes, clip)
                     : Universal(source, bytes, clip)).Run()
            .MapFail(error => new BimFault.CodecReject(key, $"geo-vector:{error.Message}"));

    // The managed FlatGeobuf arm pushes the clip DOWN through the Packed-Hilbert-R-tree bbox index so a
    // continental .fgb decodes only the overlapping feature runs — the managed equivalent of the GDAL OGR
    // Layer.SetSpatialFilterRect, never an Ogr.Open over /vsimem. A remote .fgb escalates to PackedRTree.StreamSearch.
    static Seq<GeoFeature> FlatGeobuf(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        using var fgb = new MemoryStream(bytes.ToArray());
        var rect = clip.MatchUnsafe(env => env, () => null);
        return global::FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(fgb, rect).AsIterable()
            .Map(f => new GeoFeature(f.Geometry, f.Attributes, Option<ProjectedCrs>.None)).ToSeq();
    }

    // The managed GeoParquet COLUMNAR arm: ReadColumns is the COLUMN push-down (the columnar analog of the FGB
    // bbox ROW push-down), the geo column holds WKB the canonical NTS WKBReader.Read bridges — the SAME bridge
    // the OGR universal arm crosses. GeoParquet carries no server-side bbox filter, so the clip filters client-side.
    static Seq<GeoFeature> GeoParquet(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.parquet");
        File.WriteAllBytes(path, bytes.ToArray());
        try {
            var meta = GISBlox.IO.GeoParquet.GeoParquetReader.ReadGeoMetadata(path);
            var primary = meta?.Primary_column ?? "geometry";
            var table = GISBlox.IO.GeoParquet.GeoParquetReader.ReadAll(path, GISBlox.IO.GeoParquet.Common.GeometryFormat.WKB);
            var reader = new WKBReader(GeoServices.Configure());
            var features = table.AsEnumerable().AsIterable()
                .Map(row => new GeoFeature(reader.Read((byte[])row[primary]), RowAttributes(table, row, primary), Option<ProjectedCrs>.None))
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

    static Seq<GeoFeature> GeoJson(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        var collection = JsonSerializer.Deserialize<FeatureCollection>(bytes.Span, GeoWire.Json) ?? new FeatureCollection();
        var features = collection.ToSeq().Map(f => new GeoFeature(f.Geometry, f.Attributes, Option<ProjectedCrs>.None));
        return clip.Match(None: () => features, Some: env => features.Filter(f => f.Bounds.Intersects(env)));
    }

    static Seq<GeoFeature> Shapefile(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        var options = new ShapefileReaderOptions {
            Factory = GeoServices.Factory,
            MbrFilter = clip.MatchUnsafe(env => env, () => null),
            GeometryBuilderMode = GeometryBuilderMode.FixInvalidShapes,
        };
        using var shp = new MemoryStream(bytes.ToArray());
        using var reader = NetTopologySuite.IO.Esri.Shapefile.OpenRead(shp, Stream.Null, options);
        return reader.AsIterable().Map(feature => new GeoFeature(feature.Geometry, feature.Attributes, Option<ProjectedCrs>.None)).ToSeq();
    }

    static Seq<GeoFeature> CityJson(ReadOnlyMemory<byte> bytes) {
        var document = Newtonsoft.Json.JsonConvert.DeserializeObject<CityJSON.CityJsonDocument>(Encoding.UTF8.GetString(bytes.Span))!;
        // The CityJSON metadata.referenceSystem is an OGC URN (urn:ogc:def:crs:EPSG::25832) the seam ProjectedCrs.Of
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

    // The per-CityObject planar footprint: the highest-LoD geometry's boundary vertex indices dereference into
    // the document Vertices pool and dequantize through Transform, then fold into the planar convex hull. A
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

    static Seq<GeoFeature> Universal(GeoVectorSource source, ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        GeoGdal.Bootstrap();
        string path = $"/vsimem/{Guid.NewGuid():N}";
        OSGeo.GDAL.Gdal.FileFromMemBuffer(path, bytes.ToArray());
        try {
            using var data = OSGeo.OGR.Ogr.Open(path, 0);
            var reader = new WKBReader(GeoServices.Configure());
            var features = Seq<GeoFeature>();
            for (int l = 0; l < data.GetLayerCount(); l++) {
                var layer = data.GetLayerByIndex(l);
                clip.Iter(env => layer.SetSpatialFilterRect(env.MinX, env.MinY, env.MaxX, env.MaxY));
                layer.ResetReading();
                for (var feature = layer.GetNextFeature(); feature is not null; feature = layer.GetNextFeature()) {
                    var ogr = feature.GetGeometryRef();
                    var wkb = new byte[ogr.WkbSize()];
                    ogr.ExportToWkb(wkb, OSGeo.OGR.wkbByteOrder.wkbNDR);
                    features = features.Add(new GeoFeature(reader.Read(wkb), AttributesOf(feature), Option<ProjectedCrs>.None));
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

    // The symmetric egress dispatches on the SAME GeoVectorSource row Read decodes: the managed shapefile/GeoJSON/
    // FGB/GeoParquet codecs emit directly, the GDAL OGR universal arm emits the long-tail container formats.
    // CityJSON carries NO write arm — it is an ingest-only 3D-city source a planar GeoFeature set cannot re-emit.
    public static Fin<byte[]> Write(GeoVectorSource source, Seq<GeoFeature> features, Option<ProjectedCrs> crs, Op key) =>
        Try.lift(() =>
            source == GeoVectorSource.Shapefile  ? WriteShapefile(features, crs)
          : source == GeoVectorSource.GeoJson    ? WriteGeoJson(features)
          : source == GeoVectorSource.FlatGeobuf ? WriteFlatGeobuf(features)
          : source == GeoVectorSource.GeoParquet ? WriteGeoParquet(features)
          : source == GeoVectorSource.CityJson   ? throw new NotSupportedException("cityjson-egress-unsupported:ingest-only")
          : WriteUniversal(source, features, crs)).Run()
            .MapFail(error => new BimFault.CodecReject(key, $"geo-vector-write:{error.Message}"));

    static byte[] WriteFlatGeobuf(Seq<GeoFeature> features) {
        using var output = new MemoryStream();
        var kind = features.Head
            .Map(static f => global::FlatGeobuf.NTS.GeometryConversions.ToGeometryType(f.Geometry))
            .IfNone(global::FlatGeobuf.GeometryType.Unknown);
        global::FlatGeobuf.NTS.FeatureCollectionConversions.Serialize(
            output, features.Map(static f => (IFeature)new Feature(f.Geometry, f.Attributes)), kind, dimensions: 3, columns: null);
        return output.ToArray();
    }

    static byte[] WriteGeoParquet(Seq<GeoFeature> features) {
        const string geoColumn = "geometry";
        var writer = new WKBWriter();
        var table = new System.Data.DataTable();
        table.AddGeoColumn(geoColumn, 0, GISBlox.IO.GeoParquet.Common.GeometryFormat.WKB);
        table.Columns[geoColumn]!.SetAsPrimaryGeoColumn();
        var names = features.Bind(static f => f.Attributes.GetNames().ToSeq()).Distinct().ToSeq();
        names.Iter(name => table.Columns.Add(name, typeof(string)));
        table.AddGeoProcessingMetadata([geoColumn], geoColumn);
        features.Iter(f => {
            var row = table.NewRow();
            row[geoColumn] = writer.Write(f.Geometry);
            names.Iter(name => row[name] = f.Attr(name).Map(static v => v.ToString() ?? "").IfNone(""));
            table.Rows.Add(row);
        });
        using var output = new MemoryStream();
        GISBlox.IO.GeoParquet.GeoParquetWriter.Write(output, table, geoColumn);
        return output.ToArray();
    }

    static byte[] WriteShapefile(Seq<GeoFeature> features, Option<ProjectedCrs> crs) {
        using var shp = new MemoryStream();
        using var shx = new MemoryStream();
        using var dbf = new MemoryStream();
        using var prj = new MemoryStream();
        // The .prj sidecar is a WKT projection string; the seam three-state ProjectedCrs carries the inline Wkt when a
        // GIS-origin CRS defined it, else the authority Name (an EPSG/URN form ProjNET/OSR re-resolve to WKT on read).
        NetTopologySuite.IO.Esri.Shapefile.WriteAllFeatures(
            features.Map(static f => (IFeature)new Feature(f.Geometry, f.Attributes)),
            shp, shx, dbf, prj, crs.Map(static c => c.Wkt.Length > 0 ? c.Wkt : c.Name).IfNone(""), null);
        return shp.ToArray();
    }

    static byte[] WriteGeoJson(Seq<GeoFeature> features) {
        var collection = new FeatureCollection();
        features.Iter(f => collection.Add(new Feature(f.Geometry, f.Attributes)));
        return JsonSerializer.SerializeToUtf8Bytes(collection, GeoWire.Json);
    }

    // The GDAL OGR universal egress (GeoPackage/KML/GML) writes the explicit driver to a REAL temp file then
    // File.ReadAllBytes — this GDAL SWIG build exposes only VSIFWriteL(string, ...) and NO byte[] VSIFReadL, so a
    // /vsimem byte read-back has no handle-level primitive; the managed temp-file read-back is the correct egress
    // (the SAME temp-file pattern the GeoParquet/CityJSON arms use). The driver is driver-pinned via GetDriverByName.
    static byte[] WriteUniversal(GeoVectorSource source, Seq<GeoFeature> features, Option<ProjectedCrs> crs) {
        GeoGdal.Bootstrap();
        string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}");
        var writer = new WKBWriter();
        using (var driver = OSGeo.OGR.Ogr.GetDriverByName(source.OgrDriver))
        using (var data = driver.CreateDataSource(path, [])) {
            using var srs = crs.Match(Some: SpatialRef, None: () => (OSGeo.OSR.SpatialReference?)null);
            using var layer = data.CreateLayer("features", srs, OSGeo.OGR.wkbGeometryType.wkbUnknown, []);
            using var defn = layer.GetLayerDefn();
            features.Iter(f => {
                using var feature = new OSGeo.OGR.Feature(defn);
                using var geom = OSGeo.OGR.Geometry.CreateFromWkb(writer.Write(f.Geometry));
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

- Owner: `GeoRaster` the GDAL raster ingest owner over `MaxRev.Gdal.Core` — a windowed multi-band `Dataset.ReadRaster<T>` band-stack read placed in georeferenced space by the 6-coefficient `GetGeoTransform` affine and the `GetSpatialRef`/`GetExtent` CRS extent (terrain elevation, an ortho basemap, a slope/aspect surface); `GeoRaster.Contour`/`DemProcess` the DEM-to-vector and hillshade/slope/aspect legs; `RasterTile` the windowed pixel carrier (the polymorphic `RasterBand` `[Union]` pixel buffer typed by the source `Band.DataType`, the FULL six-coefficient geo-transform with its rotation terms, the NTS `Envelope` extent, the per-band `RasterBandInfo` schema — `DataType`/`ColorInterp`/optional `NoData`/`Units`/`Offset`/`Scale`/optional `(Min,Max)` `Range`/the `ColorBin` `Palette` legend, the base-raster `Band.GetBlockSize` `BaseBlockX`/`BaseBlockY` tile dimensions, and the `RasterOverview` set — each pyramid level its dimensions, decimated cell size, own content-keyed `RasterKey`, and `Band.GetBlockSize` tile dims read off `Band.GetOverview(i)`); `GeoRaster.ToCoverage` the seam `Coverage`-node projection [M1] lowering a placed MULTI-RESOLUTION raster onto the seam graph by content-key reference (the field by-ref + per-band self-describing descriptors + the overview pyramid + the base tile dims + the seam `GeoReference`), never a stored pixel blob on the node and never a single-resolution descriptor that strands a COG/DEM pyramid.
- Entry: `GeoRaster.Read(ReadOnlyMemory<byte> bytes, Option<Envelope> window, int targetWidth, int targetHeight, Op key)` opens the raster through `Gdal.Open` over a `/vsimem/` buffer and reads the windowed band STACK into a `RasterTile` — the `window` mapping to a pixel sub-window through the inverse geo-transform, every band read in one `Dataset.ReadRaster<T>` call typed by the source `Band.DataType`, the target size triggering GDAL on-read resampling, the per-band value envelope (`Band.GetMinimum`/`GetMaximum`, else `ComputeRasterMinMax` when the stored flag is clear), the per-band `Palette`-role colour legend (`Band.GetRasterColorTable` `ColorEntry` quads paired with the `GetDefaultRAT` `GFU_Name` category or the lighter `GetCategoryNames`), and the band-1 overview pyramid (`Band.GetOverviewCount`/`GetOverview(i)`, each level's dimensions + decimated cell size + `Band.GetBlockSize` tile dims, the caller content-keying each level's bytes) all read once at ingest — `Fin<T>` aborts on an open/read fault (`BimFault.CodecReject` lifted BARE off `key`); `GeoRaster.ToCoverage(RasterTile tile, GeoReference reference, UInt128 field, Func<int, UInt128> overviewKey, ProjectionContext ctx)` wraps the placed raster into a `Geospatial/coverage#COVERAGE_NODE` `CoverageGrid` (the full six-coefficient affine `GridDescriptor` mapped POSITIONALLY off the geo-transform with its rotation terms preserved, the per-band TYPED self-describing `CoverageBand` schema carrying `Range` + `Palette`, the base `RasterKey` content key, the `OverviewLevel` pyramid each level its own `overviewKey(i)` content key, the base `BaseBlockX`/`BaseBlockY` tile dims, the seam `GeoReference`) and lands a CONTENT-hashed `Node.Coverage`, `Fin<Node.Coverage>` railing `ElementFault.ValueRejected` on a degenerate grid, an empty band set, a duplicate band index, an overview coarser-than-base / non-monotone level set, a `Palette` band with duplicate colour-bin indices, or an unknown pixel token; `GeoRaster.Contour(...)` vectorizes the DEM, `GeoRaster.Cog(...)` transcodes to a Cloud-Optimized GeoTIFF, and `GeoRaster.DemProcess(..., DemMode mode, ...)` derives hillshade/slope/aspect, each carrying its `Op key`.
- Auto: `GeoRaster.Read` runs `GeoGdal.Bootstrap` once, opens the bytes, reads the `GetGeoTransform` affine and the `GetExtent` NTS `Envelope` directly in the target CRS, reads the windowed pixels through `Dataset.ReadRaster<T>` into a managed `T[]` matching the `Band.DataType`, lowers each band's full schema (the value envelope read in priority order — `GetMinimum`/`GetMaximum`'s stored flag, else `ComputeRasterMinMax` — into the optional `(Min,Max)` `Range`, and a `GCI_PaletteIndex` band's `GetRasterColorTable` `ColorEntry` quads clamped `short`→`byte` and paired with the `GetDefaultRAT` `GFU_Name` category or `GetCategoryNames[index]` into the `ColorBin` `Palette`), and folds the band-1 `GetOverviewCount` overview set into a `RasterOverview` row per level (`GetOverview(i)` dimensions, the base `CellSize` scaled by the decimation ratio, the `GetBlockSize` tile dims); `ToCoverage` lowers the placed raster through `CoverageGrid.Of` — the `GridDescriptor` mapped POSITIONALLY off the full six-coefficient geo-transform (the two rotation terms first-class and the SIGNED pixel-height preserved, so a north-up raster's negative `CellSizeY` is valid and degeneracy is the zero-determinant test, never a sign check on a cell size), one TYPED `CoverageBand` per band (`RasterSampleType.Parse` over the `DataType` token, `BandRole` over the `ColorInterp` via the generated `TryGet`+`Undefined` fallback, the optional GDAL `NoData`, the `Units`, the `Offset`/`Scale` decode, the `Range` envelope, the `Palette` legend), the caller-supplied base `RasterKey` `UInt128` referencing the pixel buffer in the object store, the `OverviewLevel` set each level content-keyed by `overviewKey(i)`, the base `BaseBlockX`/`BaseBlockY` tile dims, and the seam `GeoReference` so the coverage carries its CRS [M1]; the resulting `Node.Coverage` is NON-ROOTED, so its `NodeId` is CONTENT-hashed over the node's own canonical bytes (the diff/dedup projection — the pyramid, the CRS, and the per-band decode/range/palette all folded into the identity), never a rooted mint, the pixel buffer never inlines onto the node, and a degenerate grid, an empty band set, a duplicate band index, an overview coarser-than-base / non-monotone level set, a `Palette` band carrying duplicate colour-bin indices, or an unknown pixel token rails `ElementFault.ValueRejected`; `GeoRaster.Contour` runs `wrapper_GDALContourDestName` over the DEM band producing contour `GeoFeature` lines tagged `"contour"`, `Cog` runs `wrapper_GDALTranslate` (the COG driver), `DemProcess` runs `wrapper_GDALDEMProcessing` (the `DemMode` lowered to the gdal_dem mode), each reading its derived raster back from a real temp file; the raster placement composes the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg and escalates to OSR only for the exotic datum-grid transforms `ProjNET` cannot express.
- Receipt: the `RasterTile` is the placed pixel evidence a terrain-mesh tessellation reads; the seam `Coverage` node is the by-reference field the terrain consumer and the `Exchange/export` 3D-Tiles terrain leg read — its `OverviewLevel` pyramid letting a `Rasm.Compute` working-resolution route pick a level by `LevelFor`, size the fetch by `ByteLength(level)`, and read that decimated level's bytes by its own `RasterKey` rather than the full base raster; its per-band `Range` the display-normalization envelope read from metadata alone and its `Palette` the indexed-band colour-and-category legend `CoverageBand.Decode` resolves; the contour `GeoFeature` lines are the vectorized terrain the site model indexes.
- Packages: `MaxRev.Gdal.Core`, `MaxRev.Gdal.MacosRuntime.Minimal.arm64`, `NetTopologySuite`, `ProjNET`, `Rasm.Element`, `Rasm`, `LanguageExt.Core`
- Growth: a new raster format is enumerable through the one `Gdal.Open` universal driver path with zero new code; a new DEM derivation is one `wrapper_GDALDEMProcessing` mode; a new resample kernel is one `RasterIOExtraArg`; a new resolution tier is one `RasterOverview` row off the existing `GetOverviewCount` fold lowered to one `OverviewLevel`; a new band attribute (a statistic, a histogram bin) is one `RasterBandInfo` column lowered to one `CoverageBand` column; the seam projection is one `ToCoverage` op; never a per-format raster reader, never an inlined pixel blob on the node, never a single-resolution tile that strands the pyramid, and never a `Palette`-role band with no colour table behind it.
- Boundary: the raster ingest is `MaxRev.Gdal.Core`'s — `GdalBase.ConfigureAll()` MUST run once before any `OSGeo.*` call and a publish without the matching RID runtime faults at first call onto `BimFault.CodecReject` (the `Model/faults#FAULT_BAND` band the `geo-raster`/`geo-vector`/`geo-contour` ingest details route, never `CapabilityMiss` — that band is the `Semantics/georeference#GEODETIC_TRANSFORM` reprojection leg's); pixels move through `Dataset.ReadRaster<T>` into a managed `T[]` matching the `Band.DataType` and a hand-rolled raster decoder is the deleted form; reprojection inside a GDAL pipeline uses OSR while managed-geometry reprojection stays the `ProjNET` leg, OSR escalating only the exotic datum-grid transforms; the seam `Coverage` node references the field by content key [M1] and an inlined pixel blob on the node is the deleted form; a coverage is MULTI-RESOLUTION so `ToCoverage` reads the band-1 `GetOverviewCount`/`GetOverview(i)` pyramid and content-keys each level — a single-resolution `RasterTile`/`CoverageGrid` that drops the COG/DEM overview set and forces a full-base fetch is the deleted form, and the `OverviewLevel` coarser-than-base + strictly-monotone gate is `CoverageGrid.Of`'s the projector cannot violate without `ElementFault.ValueRejected`; a band is FULLY self-describing so `RasterBandInfo`/`ToCoverage` read the `GetMinimum`/`GetMaximum`-else-`ComputeRasterMinMax` value envelope into the band `Range` and the `GetRasterColorTable`/`GetDefaultRAT`/`GetCategoryNames` legend into the `Palette` — an envelope-less band a consumer must scan pixels to normalize and a `Palette`-role band with an EMPTY colour table (the hollow channel) are the deleted forms the seam `CoverageBand` contract forbids; every `ColorTable`/`RasterAttributeTable`/`ColorEntry` SWIG handle is read under `using` and only the lowered `ColorBin` rows cross onto the seam, never a live GDAL handle; the tile-pyramid PARTITIONING (building new overview levels) stays at `Rasm.Compute` consumed at the seam — `Rasm.Bim` AUTHORS the COG/contour and READS the existing GDAL overview pyramid a COG/DEM already carries, the 3D-Tiles terrain leg crossing the seam, never re-deriving the pyramid.

```csharp signature
// --- [COMPOSITION] ------------------------------------------------------------------------
// The mandatory once-per-process GDAL bootstrap: ConfigureAll registers every GDAL+OGR driver and resolves
// the gdal-data/PROJ paths from the RID runtime package; UseExceptions flips the SWIG error model to thrown.
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
// The band buffer types by the source Band.DataType — GDT_Byte->byte[] (ortho), GDT_Float*->float[] (DEM),
// the integer widths->int[] (classification rasters) — so a multi-band read carries its true pixel type.
[Union]
public partial record RasterBand {
    partial record Floats(float[] Samples);
    partial record Bytes(byte[] Samples);
    partial record Ints(int[] Samples);

    public int Length => Switch(
        floats: static b => b.Samples.Length,
        bytes:  static b => b.Samples.Length,
        ints:   static b => b.Samples.Length);

    public float SampleAt(int i) => Switch<float>(
        floats: b => b.Samples[i],
        bytes:  b => b.Samples[i],
        ints:   b => b.Samples[i]);
}

// The placed windowed raster: the typed pixel band-stack, the FULL six-coefficient geo-transform + NTS extent, the
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
    int BaseBlockY);

// One GDAL overview level a multi-resolution raster carries (the band-1 Band.GetOverview(i) the COG/tiled DEM holds):
// the GetOverview index Level (the intrinsic level key the caller's overviewKey resolves to the persisted level blob's
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

// The per-band GDAL schema read once at ingest so ToCoverage lowers a TYPED, FULLY self-describing CoverageBand without
// re-opening the dataset: the pixel DataType (-> the RasterSampleType token), the ColorInterp (-> the BandRole token),
// the OPTIONAL NoData sentinel (the GDAL hasval flag lowered to Option<double>, never a NaN sentinel), the unit string,
// the Offset/Scale linear decode (GDAL GetOffset/GetScale, identity 0.0/1.0 when unset), the OPTIONAL (Min,Max) value
// envelope (GetMinimum/GetMaximum's stored flag, else ComputeRasterMinMax — the Range a display-normalization consumer
// reads from metadata alone, never an envelope-less band), and the ColorBin Palette legend a GCI_PaletteIndex band
// carries (GetRasterColorTable ColorEntry quads paired with the GetDefaultRAT GFU_Name category or GetCategoryNames —
// never a hollow Palette role with no table behind it). The OSGeo.GDAL.* types stay confined to this carrier the
// GeoRaster owner reads and ToCoverage lowers — they never cross to the seam node.
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

// The bounded DEM-derivation vocabulary the GeoRaster.DemProcess leg lowers to the gdal_dem mode token (ToString
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
                var extent = new Envelope();
                using var srs = dataset.GetSpatialRef();
                dataset.GetExtent(extent, srs);
                var (xOff, yOff, xSize, ySize) = Pixels(window, transform, dataset.RasterXSize, dataset.RasterYSize);
                int bands = dataset.RasterCount;
                var bandMap = Enumerable.Range(1, bands).ToArray();
                using var first = dataset.GetRasterBand(1);
                var dataType = first.DataType;
                var band = Materialize(dataset, dataType, xOff, yOff, xSize, ySize, targetWidth, targetHeight, bands, bandMap);
                Seq<RasterBandInfo> schema = Enumerable.Range(1, bands).AsIterable().Map(b => BandInfo(dataset.GetRasterBand(b), b - 1)).ToSeq();
                // The base-raster tile dims (GetBlockSize) lower onto CoverageGrid.BaseBlockX/Y so the full-resolution
                // base read aligns to tiles the same way an overview read does; the band-1 overview PYRAMID
                // (GetOverviewCount/GetOverview(i), decreasing-resolution by GDAL contract) folds into the RasterOverview
                // set the seam OverviewLevel takes by-reference — a COG/tiled DEM yields its decimated levels here.
                first.GetBlockSize(out int baseBlockX, out int baseBlockY);
                double baseCell = Math.Sqrt(Math.Abs((transform[1] * transform[5]) - (transform[2] * transform[4])));
                Seq<RasterOverview> overviews = Overviews(first, dataset.RasterXSize, baseCell);
                return new RasterTile(band, targetWidth, targetHeight, transform, extent, schema, overviews, baseBlockX, baseBlockY);
            } finally { OSGeo.GDAL.Gdal.Unlink(path); }
        }).Run().MapFail(error => new BimFault.CodecReject(key, $"geo-raster:{error.Message}"));

    // The seam Coverage projection [M1]: the placed MULTI-RESOLUTION raster lands a Node.Coverage wrapping a CoverageGrid
    // that holds the field BY REFERENCE (the base RasterKey content key to the pixel buffer in the object store, NEVER
    // inlined pixels), the FULL six-coefficient affine GridDescriptor mapped POSITIONALLY off the GDAL geo-transform
    // [originX, pxW, rowRot, originY, colRot, pxH] (the two rotation terms first-class, the SIGNED pxH preserved — a
    // north-up raster's negative CellSizeY is valid; degeneracy is the zero-determinant test CoverageGrid.Of enforces,
    // never a sign check), the OverviewLevel PYRAMID each level its own overviewKey(o.Level) content key + GetBlockSize
    // tile dims (so a working-resolution consumer fetches a decimated level by key, never the full base), the base
    // BaseBlockX/Y tile dims (symmetric with each overview's), and one TYPED, FULLY self-describing CoverageBand per band
    // (RasterSampleType.Parse over the DataType token, BandRole over the ColorInterp, the optional GDAL NoData, the unit
    // string, the Offset/Scale decode, the (Min,Max) Range envelope, and the ColorBin Palette legend for a palette band).
    // CoverageGrid.Of rails ElementFault.ValueRejected on a degenerate grid, an empty band set, a duplicate band index,
    // an overview coarser-than-base / non-monotone level set, or a duplicate-index Palette, and an unknown pixel token
    // rails through Parse; the field UInt128 is the content key of the persisted base pixel buffer (one XxHash128 seed).
    // The Coverage is a NON-ROOTED resource node, so its NodeId is CONTENT-hashed over the node's OWN canonical bytes
    // (the H7 projection the diff/dedup shares, the id excluded — the pyramid, the CRS, and each band's decode/range/
    // palette folded into the identity), never a rooted mint — two models sharing the same raster+affine+pyramid+CRS dedup
    // to one node, the element.md non-rooted-resource policy this folder owns no second hasher for.
    public static Fin<Node.Coverage> ToCoverage(RasterTile tile, GeoReference reference, UInt128 field, Func<int, UInt128> overviewKey, ProjectionContext ctx) =>
        tile.Bands
            .Map(info => RasterSampleType.Parse(SampleToken(info.DataType), ctx.Key).Map(sample =>
                new CoverageBand(info.Index, $"band{info.Index}", sample, Role(info.ColorInterp), info.NoData, info.Units, info.Offset, info.Scale, info.Range, info.Palette)))
            .Sequence()
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

    // The GDAL pixel-type and color-interpretation lowering the typed CoverageBand schema rides: the DataType token
    // strips the GDT_ prefix to the RasterSampleType key (an unknown pixel type rails ValueRejected through Parse), and
    // the ColorInterp resolves a BandRole through the generated TryGet over the frozen role table, defaulting Undefined
    // for the HSL/CMYK/YCbCr channels a coverage consumer does not read — never a stringly DataType on the seam.
    static string SampleToken(OSGeo.GDAL.DataType dataType) =>
        dataType.ToString().Replace("GDT_", "").ToLowerInvariant();

    static readonly Map<OSGeo.GDAL.ColorInterp, string> RoleToken = Map(
        (OSGeo.GDAL.ColorInterp.GCI_GrayIndex,    BandRole.Gray.Key),
        (OSGeo.GDAL.ColorInterp.GCI_PaletteIndex, BandRole.Palette.Key),
        (OSGeo.GDAL.ColorInterp.GCI_RedBand,      BandRole.Red.Key),
        (OSGeo.GDAL.ColorInterp.GCI_GreenBand,    BandRole.Green.Key),
        (OSGeo.GDAL.ColorInterp.GCI_BlueBand,     BandRole.Blue.Key),
        (OSGeo.GDAL.ColorInterp.GCI_AlphaBand,    BandRole.Alpha.Key));

    static BandRole Role(OSGeo.GDAL.ColorInterp colorInterp) =>
        BandRole.TryGet(RoleToken.Find(colorInterp).IfNone(BandRole.Undefined.Key), out BandRole? role) && role is { } resolved
            ? resolved
            : BandRole.Undefined;

    static (int XOff, int YOff, int XSize, int YSize) Pixels(Option<Envelope> window, double[] gt, int rasterX, int rasterY) =>
        window.Match(
            None: () => (0, 0, rasterX, rasterY),
            Some: env => {
                int x0 = Math.Clamp((int)Math.Floor((env.MinX - gt[0]) / gt[1]), 0, rasterX);
                int y0 = Math.Clamp((int)Math.Floor((env.MaxY - gt[3]) / gt[5]), 0, rasterY);
                int x1 = Math.Clamp((int)Math.Ceiling((env.MaxX - gt[0]) / gt[1]), 0, rasterX);
                int y1 = Math.Clamp((int)Math.Ceiling((env.MinY - gt[3]) / gt[5]), 0, rasterY);
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

    // The full per-band GDAL schema lowered to a RasterBandInfo at read time (index 0-based): the pixel DataType and
    // the ColorInterp host enums, the optional NoData (the GDAL out-int hasval flag lowered to Option<double>, never a
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

    // The per-band value envelope -> CoverageBand.Range: GetMinimum/GetMaximum read the STORED metadata flag first
    // (hasval==1 ⇒ the source carries the envelope) and fall to ComputeRasterMinMax (approxOk sampling a decimated
    // overview, not the full base raster) only when the stored flag is clear, so a display-normalization consumer reads
    // the envelope from metadata alone — never an envelope-less band the consumer must scan pixels to normalize. A
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
    // blank. Every ColorTable/RasterAttributeTable/ColorEntry SWIG handle frees under `using` at this boundary so only
    // the lowered ColorBin rows cross onto the seam, never a live GDAL handle — and a Palette role never lands hollow.
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

    // The band-1 GDAL overview PYRAMID -> the RasterOverview set (the seam OverviewLevel takes it by-reference): the
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
                var reader = new WKBReader(GeoServices.Configure());
                var features = Seq<GeoFeature>();
                for (int l = 0; l < contoured.GetLayerCount(); l++) {
                    var layer = contoured.GetLayerByIndex(l);
                    layer.ResetReading();
                    for (var feature = layer.GetNextFeature(); feature is not null; feature = layer.GetNextFeature()) {
                        var wkb = new byte[feature.GetGeometryRef().WkbSize()];
                        feature.GetGeometryRef().ExportToWkb(wkb, OSGeo.OGR.wkbByteOrder.wkbNDR);
                        features = features.Add(new GeoFeature(reader.Read(wkb),
                            new AttributesTable { ["type"] = "contour", ["elev"] = feature.GetFieldAsDouble("elev") }, Option<ProjectedCrs>.None));
                    }
                }
                return features;
            } finally { OSGeo.GDAL.Gdal.Unlink(source); OSGeo.GDAL.Gdal.Unlink(sink); }
        }).Run().MapFail(error => new BimFault.CodecReject(key, $"geo-contour:{error.Message}"));

    // The GeoTIFF -> Cloud-Optimized GeoTIFF transcode (the gdal_translate COG driver): the input rides /vsimem, the
    // tiled+overviewed COG output a REAL temp file File.ReadAllBytes recovers (no byte[] VSIFReadL in this SWIG build);
    // the COG is the format the Exchange/export 3D-Tiles terrain leg and a cloud raster store consume.
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

    // The DEM derivation leg (the gdal_dem hillshade/slope/aspect algorithm): the bounded DemMode lowers to the GDAL
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

- [NTS_PLANAR_ALGEBRA]: the `NetTopologySuite` member spellings the `GEOSPATIAL_SEAM` composes are decompile-verified — `NtsGeometryServices.Instance`, `GeometryOverlay.NG`, `PackedCoordinateSequenceFactory.DoubleFactory`, `STRtree<T>.Insert`/`Query`, `PreparedGeometryFactory.Prepare` + `IPreparedGeometry.Intersects`, `GeometryFixer.Fix`, `OverlayNGRobust.Union`, and `WKBReader.Read`/`WKBWriter.Write` — confirmed against `.api/api-nettopologysuite`; the canonical broad-then-narrow spatial-join rail stacks `STRtree.Query` then `PreparedGeometry.Intersects`.
- [GDAL_UNIVERSAL_INGEST]: the `MaxRev.Gdal.Core` member spellings the `VECTOR_INGEST` universal arm and the `RASTER_INGEST` owner compose are decompile-verified at `.api/api-maxrev-gdal` — `GdalBase.ConfigureAll()`/`IsConfigured`, `Gdal.FileFromMemBuffer`/`Unlink`, `Ogr.Open`/`Ogr.GetDriverByName`/`Layer.SetSpatialFilterRect`/`GetNextFeature`/`Feature.GetGeometryRef`/`Geometry.CreateFromWkb`, `Gdal.Open`/`Dataset.ReadRaster<T>`/`GetGeoTransform`/`GetSpatialRef`/`GetExtent`/`Band.DataType`, the FULL self-describing band-metadata surface the `CoverageBand` schema internalizes — `Band.GetNoDataValue`/`GetOffset`/`GetScale`/`GetUnitType`/`GetColorInterpretation` plus the value envelope `Band.GetMinimum`/`GetMaximum`(stored flag)/`ComputeRasterMinMax(double[2], approx_ok)` (→ the `Range` `Option<(Min,Max)>`) and the `GCI_PaletteIndex` legend `Band.GetRasterColorTable() -> ColorTable.GetCount()`/`GetColorEntry(int) -> ColorEntry.c1..c4`, `Band.GetDefaultRAT() -> RasterAttributeTable.GetColOfUsage(RATFieldUsage.GFU_Name)`/`GetRowOfValue(int)`/`GetValueAsString(row,col)`, and `Band.GetCategoryNames() -> string[]` (→ the `ColorBin` `Palette`), the multi-resolution pyramid `Band.GetOverviewCount()`/`GetOverview(int) -> Band` with each level's `XSize`/`YSize` and `Band.GetBlockSize(out,out)` tile dims (→ the `OverviewLevel` set + the base `BaseBlockX`/`BaseBlockY`), `wrapper_GDALContourDestName`/`GDALContourOptions`, `wrapper_GDALTranslate`/`GDALTranslateOptions` (the COG transcode), `wrapper_GDALDEMProcessing`/`GDALDEMProcessingOptions` (the hillshade/slope/aspect leg), and the OSR `SpatialReference`/`SetFromUserInput`/`SetAxisMappingStrategy`/`CoordinateTransformation` escalation; every `ColorTable`/`RasterAttributeTable`/`ColorEntry`/overview `Band` SWIG handle frees under `using` so only the lowered value rows cross onto the seam, never a live GDAL handle; the universal EGRESS reads its driver output back from a REAL temp file (this SWIG build exposes `VSIFWriteL(string, …)` but NO `byte[]` `VSIFReadL`, so a `/vsimem` byte read-back has no handle-level primitive, the catalog's named phantom); the RID runtime pin `MaxRev.Gdal.MacosRuntime.Minimal.arm64` stages the `gdal-data`/PROJ resources, the hard platform constraint the boundary names.
- [SEAM_PROJECTION]: the `GeoFeature.ToObject`/`GeoModel.Project` vector→seam-`Object` and `GeoRaster.ToCoverage` raster→seam-`Coverage` projections ground against the seam node contract — a vector feature rides an `Object` node with no parallel `Feature` family, the `Coverage` node a `Geospatial/coverage#COVERAGE_NODE` `CoverageGrid` by-ref + bands + pyramid + CRS, the `Object` geometry a `Graph/element#NODE_MODEL` `RepresentationContentHash`-keyed map [M1][M2], the geospatial source its own projector registered at the composition root [§6]; a vector feature lands an `Object` occurrence carrying the generic `Classification.Create("ifc", classKey, "", "", None, None)` (the seam six-member factory, edition-unspecified for a synthetic entity-class mint) whose `classKey` is the TRUE IFC4.3 entity-type string the `GeoClassifier` table holds (the seam `Classification` is a library-neutral `(system, code)` pair, NOT a `Model/elements#IFC_CLASS` `IfcClass` row — the Bim `Emit` egress gate resolves the `IfcClass` row from the code and runs `AdmitPredefined`, so an IfcGeographicElement/IfcSite/IfcBuilding the roster has not yet rostered round-trips to IFC only once it is added), its bare `PredefinedType` token validated at `Emit` not ingest [C6], its attributes a `Pset_SiteContext` `PropertySet` node linked by a neutral `Rasm.Element/Properties/property#PROPERTY_BAG`-bound `Assign(PropertyDefinition)` edge (`Subject`=object, `Definition`=pset — `Associate` is the seam's material/appearance/coverage binding, never a property attachment), its footprint content-keyed into the `Representations` map under the `FootPrint` key through the kernel seed-zero `ContentHash.Of` [M2] — the analytical surface the `Rasm.Compute` disciplines resolve one-hop by content key from the blob store, the IFC leg leaving the same key empty (the analytical surface it cannot evaluate), with NO inline coordinate field on the seam node (the deleted §4-RT-M2 violation); a raster lands a MULTI-RESOLUTION `Coverage` node wrapping a `CoverageGrid` whose base `RasterKey` `UInt128` references the field in the object store, whose `OverviewLevel` pyramid (the band-1 `GetOverviewCount`/`GetOverview(i)` levels, each content-keyed by its own `RasterKey` + `GetBlockSize` tile dims) lets a `Rasm.Compute` working-resolution route `LevelFor`/`ByteLength`/fetch a decimated level rather than the full base, whose per-band `Range` (`GetMinimum`/`GetMaximum` else `ComputeRasterMinMax`) is the display-normalization envelope and `Palette` (`GetRasterColorTable`/`GetDefaultRAT`/`GetCategoryNames`) the indexed-band colour-and-category legend `CoverageBand.Decode` resolves, and which carries the seam `GeoReference` [M1] — the pixel buffer never inlined, the single-resolution descriptor that strands a COG/DEM pyramid and the hollow `Palette`-role band with no table the deleted forms `CoverageGrid.Of` rejects — so a site-context model is a seam graph like any imported model and the seam `Bake` reads it with no second selection surface.
- [REPROJECTION_SEAM]: the geodetic reprojection composes the `Semantics/georeference#GEODETIC_TRANSFORM` `GeoTransform.Reproject` `ProjNET` leg over the seam `GeoReference` — the ONE datum owner reprojecting a DOUBLE-precision ordinate `Span<double>` IN PLACE, so a migration-source `ICoordinateSequenceFilter` that COMPUTES the datum shift is the deleted NTS-side form; the projector flattens the geometry's `CoordinateSequence` into the double buffer, hands it to the owner, and writes the reprojected ordinates back through `Geometry.Apply` (the `.api/api-nettopologysuite` reprojection seam — `Geometry.Coordinates` is a detached copy, so the write-back rides the in-place ordinate visitor that COMPUTES nothing, never the array), never narrowing survey coordinates to `float`; the `GeoFeature.SourceCrs` is the from-frame and the project `GeoReference` the to-frame, the two EPSG codes driving the SRID lookup, an absent source/target EPSG or a matching EPSG leaving the geometry unchanged (the additive owner contract) so a single-datum site never blocks ingest, with `MaxRev.Gdal.Core` OSR the escalation for the exotic datum-grid/dynamic-datum transforms `ProjNET` cannot express, the owner faulting `BimFault.CapabilityMiss` bare only when both engines defeat a present, differing pair.

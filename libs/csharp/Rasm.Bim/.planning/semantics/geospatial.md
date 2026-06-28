# [BIM_GEOSPATIAL]

The georeferenced-BIM site-context PROJECTOR over the `Rasm.Element` seam: one `GeoFeature` canonical row (the OGC Simple-Features `NetTopologySuite` `Geometry` plus its `AttributesTable` and seam `ProjectedCrs`) the whole geospatial seam materializes, a `GeoModel` carrying the feature set under one `NtsGeometryServices.Instance` precision/SRID configuration and the `STRtree` broad-phase 2D index, a `GeoIngest` fold admitting every admitted vector and raster source onto that one carrier, and the `GeoProjection` that lowers a vector feature onto a seam `Object` node and a raster coverage onto a seam `Coverage` node [M1] through a `GraphDelta` the `Projection/semantic#SEMANTIC_PROJECTOR` projector folds. A vector feature RIDES an `Object` node — a parcel, a terrain TIN, or a city building lands as an `Object` occurrence discriminated by the generic `Classification("ifc", code)` like any imported element, never a parallel `Feature`/`GeoElement` family [§4B]. A raster coverage lands a `Coverage` node (raster/field by-ref + bands + the seam `GeoReference` CRS) [M1], never a stored pixel blob on the element. The vector ingest (shapefile/GeoJSON/CityJSON/FlatGeobuf/GeoParquet managed codecs plus the GDAL/OGR universal long-tail) and the raster ingest (GeoTIFF/COG/DEM windowed `ReadRaster<T>`) are Bim's NTS/GDAL capability the projector composes. The page is HOST-NEUTRAL: NTS owns the 2D planar geometry, the kernel `Rasm` owns the 3D solid geometry, the seam owns the node vocabulary, and the three meet only at the in-process WKB/`CoordinateSequence` kernel wire and the content-keyed seam node — a RhinoCommon binding on a geospatial owner is the named seam violation.

## [01]-[INDEX]

- [01]-[GEOSPATIAL_SEAM]: `GeoFeature` canonical row, `GeoModel` feature set + `STRtree` broad-phase, the `NtsGeometryServices.Instance` precision/SRID root, the planar predicate/overlay/spatial-join surface, the `GeoClassifier` `OgcGeometryType`→`IfcClass` table, and the `GeoFeature.ToObject`/`GeoModel.Project` site-context seam `Object`-node projection through a `GraphDelta`.
- [02]-[VECTOR_INGEST]: `GeoVector` fold over the `GeoVectorSource` `[SmartEnum]` — the managed shapefile/GeoJSON/CityJSON + FlatGeobuf (row-oriented, bbox push-down) + GeoParquet (columnar, column push-down) arms and the GeoPackage/OGR-universal arm producing `GeoFeature` rows, the OGR↔NTS WKB bridge, the server-side push-down, and the symmetric write side.
- [03]-[RASTER_INGEST]: `GeoRaster` GDAL raster ingest (GeoTIFF/COG/DEM windowed `ReadRaster<T>`), the geo-transform/extent placement, the DEM contour/hillshade vectorization to `GeoFeature`, the COG transcode, and the `GeoRaster.ToCoverage` seam `Coverage`-node projection [M1].

## [02]-[GEOSPATIAL_SEAM]

- Owner: `GeoFeature` the one host-neutral geospatial row carrying its `NetTopologySuite.Geometries.Geometry` planar geometry, its `IAttributesTable` keyed property bag, and its `Option<ProjectedCrs>` seam source CRS; `GeoModel` the feature set under one `GeometryFactory` resolved from `NtsGeometryServices.Instance`, carrying the lazily-built `STRtree<GeoFeature>` broad-phase 2D index; `GeoServices` the process-wide `NtsGeometryServices` configuration root set once with the robust `GeometryOverlay.NG` engine and the dense `PackedCoordinateSequenceFactory`; `GeoClassifier` the frozen `(OgcGeometryType, tag)`→`IfcClass` table the site-context projection keys on; `GeoProjection` the seam-node projector lowering a feature onto an `Object` node and its attributes onto a `PropertySet` node through a `GraphDelta`.
- Entry: `GeoModel.Of(Seq<GeoFeature> features)` indexes the features into the `STRtree` once; `GeoModel.SpatialJoin(Geometry probe)` runs the canonical broad-then-narrow rail (`STRtree.Query` candidates, then `PreparedGeometryFactory.Prepare(probe).Intersects` per candidate); `GeoFeature.ToObject(GeoReference reference, ProjectionContext ctx)` projects one feature onto a seam `Object` occurrence node with a `GeoClassifier`-resolved `Classification("ifc", classKey)`, reprojecting the geometry into the canonical kernel frame through `GeoTransform.Reproject`, content-keying the footprint, and threading the `AttributesTable` onto a seam `Pset_SiteContext` `PropertySet` node linked by a neutral `Associate` edge — `Fin<GraphDelta>` aborts on a feature whose `(OgcGeometryType, tag)` the classifier does not carry (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) or an invalid geometry `GeometryFixer.Fix` cannot repair (`BimFault.ModelRejected`), each lowered with `.ToError()`; `GeoModel.Project(GeoReference, ProjectionContext)` folds the whole feature set into one `GraphDelta`.
- Auto: `GeoServices.Configure` sets `NtsGeometryServices.Instance` once at module init behind an idempotency guard with `GeometryOverlay.NG` and a `PackedCoordinateSequenceFactory` so every reader resolves cached factories carrying the one canonical `PrecisionModel`/`SRID`; `GeoModel.Of` bulk-inserts each feature envelope into the `STRtree` (read-only after the first `Query`, lazily built); `GeoModel.Dissolve` folds a footprint set through `OverlayNGRobust.Union` after a `GeometryFixer.Fix` validity gate; `GeoFeature.ToObject` reads the `GeoClassifier` table for the IFC class, reprojects the geometry ordinates through the `Semantics/georeference#GEODETIC_TRANSFORM` `GeoTransform.Reproject` into the project CRS, content-keys the reprojected footprint WKB through the kernel seed-zero `XxHash128` for the `Object` node `RepresentationContentHash`, and folds the `IAttributesTable` `GetNames`/`GetValues` onto a seam `Pset_SiteContext` `PropertySet` node (the GeoJSON footprint riding one `PropertyValue` so the cross-runtime `shapely`/`turf` peers decode it), linking the two by an `IfcRelDefinesByProperties` neutral `Associate` edge.
- Receipt: the `GeoFeature` is the typed planar-geometry evidence a site clash or a parcel-boundary setback reads; the `GeoModel.STRtree` index is the broad-phase candidate generator; the projected seam `Object` node is discriminated by the same generic `Classification` an imported element carries, so the seam `Bake` and the `Review/validation#IDS_FACETS` audit read a site-context model with no second selection surface; the raster `Coverage` node carries the field by-ref + bands + CRS the terrain consumer reads.
- Packages: `NetTopologySuite`, `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage`, `ProjNET`, `Rasm.Element`, `Rasm`, `GeometryGymIFC_Core`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`, `NodaTime`
- Growth: a new planar predicate is one `Geometry` instance method on the existing algebra; a new spatial index is the `STRtree`/`Quadtree` swap the `GeoModel` carrier owns; a new site-context class mapping is one `GeoClassifier` table row keyed on `(OgcGeometryType, tag)`; a new attribute projection is one `Pset_SiteContext` `PropertyValue`; never a parallel planar geometry world beside NTS, never a per-feature-kind `GeoFeature` subtype, never a parallel `Feature`/`GeoElement` node beside the seam `Object`, and never a second precision/SRID configuration beside `NtsGeometryServices.Instance`.
- Boundary: the planar Simple-Features algebra is `NetTopologySuite`'s — the `Geometry` hierarchy, the DE-9IM predicates, the `OverlayNG` robust boolean, the `STRtree` index, and `PreparedGeometry` acceleration are the package's, and a hand-rolled planar intersection or a second R-tree is the deleted form; the `NtsGeometryServices.Instance` global is the single precision/SRID owner configured once and a per-call factory is the rejected form; validity repair enters through `GeometryFixer.Fix` before any overlay or write; the geodetic reprojection composes the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg over the seam `GeoReference` and a `NetTopologySuite`-side datum shift is the named seam violation; the 3D solid geometry stays the kernel `Rasm`'s and a geospatial owner carrying a RhinoCommon `Brep`/`Mesh` is the host-bound defect — NTS 2D planar geometry crosses to the kernel ONLY as a `CoordinateSequence` ordinate buffer (or its WKB form) the kernel constrained-Delaunay realize pass triangulates into the content-keyed geometry the `Object` node references, distinct from the cross-runtime GeoJSON peer wire; the site-context projection mints a seam `Object` node and a parallel `GeoElement`/`SiteElement` record beside it is the deleted form [§4B]; a raster coverage lands a seam `Coverage` node [M1] and a stored pixel blob on the element node is the deleted form; the `GeoClassifier` is a frozen data table keyed on `(OgcGeometryType, tag)`, never enumerated `switch` arms.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.Json;
using GeometryGym.Ifc;
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

    // The seam-node projection: a vector feature RIDES an Object node [§4B] carrying the generic Classification,
    // its footprint content-keyed for the Object RepresentationContentHash, its attributes a Pset_SiteContext
    // PropertySet node linked by a neutral Associate(DefinesByProperties) edge. The GeoJSON footprint rides one
    // PropertyValue so the cross-runtime shapely/turf peers decode it. Geospatial classes target IFC4.3.
    public Fin<GraphDelta> ToObject(GeoReference reference, ProjectionContext ctx) =>
        GeoClassifier.Classify(this)
            .Bind(row => IfcClass.Resolve(row.Class.Key)
                .Bind(cls => cls.AdmitPredefined(row.Predefined, row.Predefined, ReleaseVersion.IFC4X3_ADD2)
                    .Map(predefined => (Class: cls, Predefined: predefined))))
            .Map(row => {
                var footprint = Reproject(reference);
                var objectId = ctx.Rooted();
                var pset = new Node.PropertySet(
                    Id:         ctx.Rooted(),
                    ExternalId: None,
                    Name:       "Pset_SiteContext",
                    Mode:       InheritanceMode.OccurrenceWins,
                    Values:     footprint.Attributes.GetNames()
                                    .ToMap(name => new PropertyName(name), name => PropertyValue.Of(footprint.Attributes[name]?.ToString() ?? "", "IfcText"))
                                    .AddOrUpdate(new PropertyName("Footprint"), PropertyValue.Of(GeoWire.ToGeoJson(footprint), "IfcText")));
                var obj = new Node.Object(
                    Id:         objectId,
                    Mode:       ObjectMode.Occurrence,
                    ExternalId: footprint.Attributes.GetOptionalValue("id")?.ToString() is { Length: > 0 } id ? Some(id) : None,
                    Class:      new Classification("ifc", row.Class.Key),
                    Predefined: row.Predefined,
                    Geometry:   RepresentationContentHash.Empty.With("FootPrint", InterchangeIdentity.Key("geo-footprint", footprint.Geometry.ToBinary(), 1e-6, 1e-6, 1e-9)),
                    Owner:      None,
                    Name:       (footprint.Attributes.GetOptionalValue("name") ?? row.Class.Key).ToString()!,
                    Tag:        (footprint.Attributes.GetOptionalValue("id") ?? "").ToString()!);
                return GraphDelta.Empty
                    .Put(obj).Put(pset)
                    .Link(IfcRelKind.DefinesByProperties.Edge(pset.Id, objectId, Map<PropertyName, PropertyValue>()));
            });

    // The packed PackedCoordinateSequence materializes a detached Coordinate[] for Geometry.Coordinates, so the
    // reprojected ordinates persist back through an ICoordinateSequenceFilter (Geometry.Apply visits the live
    // sequences in Coordinates order) — a write into the detached Coordinate[] would silently no-op.
    GeoFeature Reproject(GeoReference reference) {
        if (!reference.IsGeoreferenced) {
            return this;
        }
        var coords = Geometry.Coordinates;
        var span = new float[coords.Length * 3];
        for (int i = 0; i < coords.Length; i++) {
            (span[i * 3], span[i * 3 + 1], span[i * 3 + 2]) = ((float)coords[i].X, (float)coords[i].Y, (float)(double.IsNaN(coords[i].Z) ? 0.0 : coords[i].Z));
        }
        GeoTransform.Reproject(reference, span, stride: 3);
        Geometry.Apply(new SequenceReprojection(span));
        return this;
    }
}

// Writes the reprojected ordinate columns back into the live CoordinateSequence in Geometry.Apply visitation
// order (the same canonical order Geometry.Coordinates produced span from), so the cursor and the span index
// align across multi-sequence geometries (polygon holes, multi-part collections).
sealed class SequenceReprojection(float[] reprojected) : ICoordinateSequenceFilter {
    int cursor;
    public bool Done => false;
    public bool GeometryChanged => true;

    public void Filter(CoordinateSequence sequence, int index) {
        int o = cursor++ * 3;
        sequence.SetX(index, reprojected[o]);
        sequence.SetY(index, reprojected[o + 1]);
        if (sequence.HasZ) {
            sequence.SetZ(index, reprojected[o + 2]);
        }
    }
}

public sealed record GeoModel(Seq<GeoFeature> Features) {
    readonly STRtree<GeoFeature> index = Build(Features);

    public static GeoModel Of(Seq<GeoFeature> features) => new(features.Map(static f => f.Repaired));

    static STRtree<GeoFeature> Build(Seq<GeoFeature> features) {
        var tree = new STRtree<GeoFeature>(Math.Max(2, features.Count));
        features.Iter(f => tree.Insert(f.Bounds, f));
        return tree;
    }

    public Seq<GeoFeature> SpatialJoin(Geometry probe) {
        var prepared = PreparedGeometryFactory.Prepare(probe);
        return index.Query(probe.EnvelopeInternal).Filter(f => prepared.Intersects(f.Geometry)).ToSeq();
    }

    public Geometry Dissolve() =>
        OverlayNGRobust.Union(Features.Map(static f => f.Repaired.Geometry).ToArray());

    // The whole site-context feature set folds into ONE GraphDelta the SemanticProjector merges — each feature's
    // Object + PropertySet nodes and the Associate edge accumulate onto the seam graph in one apply.
    public Fin<GraphDelta> Project(GeoReference reference, ProjectionContext ctx) =>
        Features.Map(f => f.ToObject(reference, ctx)).Sequence()
            .Map(static deltas => deltas.Fold(GraphDelta.Empty, static (acc, delta) => acc.Merge(delta)));
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoClassifier {
    static readonly Map<(OgcGeometryType Kind, string Tag), (IfcClass Class, string Predefined)> Table =
        Map(
            ((OgcGeometryType.Polygon,      "building"),  (Resolve("IfcBuilding"),          "ELEMENT")),
            ((OgcGeometryType.MultiPolygon, "building"),  (Resolve("IfcBuilding"),          "ELEMENT")),
            ((OgcGeometryType.Polygon,      "parcel"),    (Resolve("IfcSite"),              "ELEMENT")),
            ((OgcGeometryType.Polygon,      "landuse"),   (Resolve("IfcSite"),              "ELEMENT")),
            ((OgcGeometryType.Polygon,      "relief"),    (Resolve("IfcGeographicElement"), "TERRAIN")),
            ((OgcGeometryType.MultiPolygon, "relief"),    (Resolve("IfcGeographicElement"), "TERRAIN")),
            ((OgcGeometryType.LineString,   "road"),      (Resolve("IfcCourse"),            "PAVEMENT")),
            ((OgcGeometryType.LineString,   "rail"),      (Resolve("IfcRail"),              "NOTDEFINED")),
            ((OgcGeometryType.LineString,   "contour"),   (Resolve("IfcGeographicElement"), "TERRAIN")),
            ((OgcGeometryType.Point,        "furniture"), (Resolve("IfcGeographicElement"), "VEGETATION")),
            ((OgcGeometryType.Polygon,      ""),          (Resolve("IfcGeographicElement"), "NOTDEFINED")),
            ((OgcGeometryType.LineString,   ""),          (Resolve("IfcGeographicElement"), "NOTDEFINED")));

    static IfcClass Resolve(string ifcType) => IfcClass.TryGet(ifcType).IfNone(IfcClass.Proxy);

    public static Fin<(IfcClass Class, string Predefined)> Classify(GeoFeature feature) {
        var tag = (feature.Attributes.GetOptionalValue("type") ?? feature.Attributes.GetOptionalValue("class") ?? "").ToString()!.ToLowerInvariant();
        return Table.Find((feature.Kind, tag))
            .OrElse(() => Table.Find((feature.Kind, "")))
            .ToFin(new BimFault.UnmappedClass($"geo-feature-miss:{feature.Kind}:{tag}").ToError());
    }
}
```

## [03]-[VECTOR_INGEST]

- Owner: `GeoVector` the universal vector ingest-and-egress fold over `GeoVectorSource`, the `[SmartEnum<string>]` source table whose rows carry the managed-versus-GDAL reader discriminant — `Shapefile` (`NetTopologySuite.IO.Esri.Shapefile`), `GeoJson` (`NetTopologySuite.IO.GeoJSON4STJ`), `CityJson` (`bertt.CityJSON`, ingest-only), `FlatGeobuf` (`FlatGeobuf.NTS`, the cloud-optimized row-oriented codec with the Packed-Hilbert-R-tree bbox push-down), and `GeoParquet` (`GISBlox.IO.GeoParquet` over `ParquetSharp`, the columnar `DataTable`↔WKB arm) decode through dedicated managed codecs, `GeoPackage` and the long-tail OGR-only drivers through the one `MaxRev.Gdal.Core` OGR universal reader — every arm producing the canonical `GeoFeature` row; `GeoWire` the `GeoFeature`'s two canonical wire projections per `data-interchange#GEO_INTERCHANGE` (GeoJSON text the cross-runtime `shapely`/`turf` peers decode + the GeoPackage binary blob the `csharp:Rasm.Persistence/Store` geo-store-blob persists); `GeoWkb` the bidirectional OGR↔NTS bridge the universal driver path crosses on both legs — the SAME `WKBReader.Read` the GeoParquet geo-column cell crosses.
- Entry: `GeoVector.Read(GeoVectorSource source, ReadOnlyMemory<byte> bytes, Option<Envelope> clip)` decodes a vector source onto `Seq<GeoFeature>` — the `Shapefile` arm through `Shapefile.OpenRead` (the `clip` driving `ShapefileReaderOptions.MbrFilter` server-side window push-down), the `CityJson` arm through a `Transform`-dequantized boundary fold per `CityObject`, the `FlatGeobuf` arm through `FeatureCollectionConversions.Deserialize(stream, rect)` (the `clip` pushed DOWN through the Packed-Hilbert-R-tree bbox index), the `GeoParquet` arm through `GeoParquetReader.ReadAll`/`ReadColumns` then a per-row `WKBReader.Read`, the universal arm through `Ogr.Open` over a `/vsimem/` buffer then `Layer.SetSpatialFilterRect`/`GetNextFeature`/`Feature.GetGeometryRef.ExportToWkb` → `WKBReader.Read` — `Fin<T>` aborts on a corrupt container (`Model/faults#FAULT_BAND` `BimFault.CodecReject`) lowered with `.ToError()` at the boundary; `GeoVector.Write(GeoVectorSource source, Seq<GeoFeature> features, Option<ProjectedCrs> crs)` emits the symmetric byte payload.
- Auto: the `GeoVectorSource` row discriminant routes the decode without a call-site branch — the `Shapefile.OpenRead` `MbrFilter` skips records outside the `clip`; the `CityJson` arm reads `CityObject.Geometry[i]` discriminating `GeometryType` and recovering coordinates as `vertex × Transform.ScaleVector3() + Transform.TranslateVector3()`; the `FlatGeobuf` arm pushes the `clip` through the Packed-Hilbert-R-tree, a remote `.fgb` escalating to `PackedRTree.StreamSearch`; the `GeoParquet` arm `ReadGeoMetadata` reads the OGC `geo` header, `ReadColumns` projects only the needed columns server-side, each WKB cell bridging through `WKBReader.Read`; the universal arm runs `GeoGdal.Bootstrap` once, opens through `Gdal.FileFromMemBuffer` + `Ogr.Open`, pushes the `clip` through `Layer.SetSpatialFilterRect`, and bridges each `ExportToWkb` buffer through `WKBReader.Read`; every produced `GeoFeature` is `GeometryFixer.Fix`-repaired at admission.
- Receipt: the `GeoVector.Read` `Seq<GeoFeature>` is the universal vector ingest evidence the `GEOSPATIAL_SEAM` `GeoModel` indexes and the `GeoProjection` lowers onto seam `Object` nodes; the `GeoVectorSource` row records which codec decoded so the reader is one table read.
- Packages: `NetTopologySuite`, `NetTopologySuite.IO.Esri.Shapefile`, `bertt.CityJSON`, `FlatGeobuf`, `GISBlox.IO.GeoParquet`, `MaxRev.Gdal.Core`, `NodaTime`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new managed vector codec is one `GeoVectorSource` `managed:true` row; a new OGR-only long-tail format is enumerable through the existing universal `Ogr.Open` arm with zero new row; a new attribute push-down is one `Layer.SetAttributeFilter`/`ReadColumns` argument; never a per-format importer family, never a hand-rolled binary record, and never a boolean op on the OGR side.
- Boundary: the shapefile/FlatGeobuf/GeoParquet codecs are the pure-managed defaults and admitting GDAL for a format a managed codec reads is the rejected form; the managed codec output IS the canonical `NetTopologySuite.Features.Feature`; the OGR↔NTS bridge is the `ExportToWkb`→`WKBReader.Read` / `WKBWriter.Write`→`CreateFromWkb` wire — running planar boolean ops on the OGR side fragments the one topology owner; the `GdalBase.ConfigureAll()` bootstrap runs once per process behind the `IsConfigured` guard; `Gdal.UseExceptions()` flips the SWIG error model so a failed open lowers onto `BimFault.CodecReject`; the CityJSON quantization is lossless (integer indices into `Vertices`, recovered through `Transform`) and tessellating it in the codec is the deleted form; `CityJSON.*`/`OSGeo.*`/`FlatGeobuf.*`/`GISBlox.*` types never leak past this fold.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
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
    public static Fin<Seq<GeoFeature>> Read(GeoVectorSource source, ReadOnlyMemory<byte> bytes, Option<Envelope> clip) =>
        Try.lift(() => source == GeoVectorSource.Shapefile  ? Shapefile(bytes, clip)
                     : source == GeoVectorSource.GeoJson    ? GeoJson(bytes, clip)
                     : source == GeoVectorSource.CityJson   ? CityJson(bytes)
                     : source == GeoVectorSource.FlatGeobuf ? FlatGeobuf(bytes, clip)
                     : source == GeoVectorSource.GeoParquet ? GeoParquet(bytes, clip)
                     : Universal(source, bytes, clip)).Run()
            .MapFail(static error => new BimFault.CodecReject($"geo-vector:{error.Message}").ToError());

    // The managed FlatGeobuf arm pushes the clip DOWN through the Packed-Hilbert-R-tree bbox index so a
    // continental .fgb decodes only the overlapping feature runs — the managed equivalent of the GDAL OGR
    // Layer.SetSpatialFilterRect, never an Ogr.Open over /vsimem. A remote .fgb escalates to PackedRTree.StreamSearch.
    static Seq<GeoFeature> FlatGeobuf(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        using var fgb = new MemoryStream(bytes.ToArray());
        var rect = clip.MatchUnsafe(env => env, () => null);
        return global::FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(fgb, rect)
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
            var features = table.AsEnumerable()
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
        return reader.Map(feature => new GeoFeature(feature.Geometry, feature.Attributes, Option<ProjectedCrs>.None)).ToSeq();
    }

    static Seq<GeoFeature> CityJson(ReadOnlyMemory<byte> bytes) {
        var document = Newtonsoft.Json.JsonConvert.DeserializeObject<CityJSON.CityJsonDocument>(Encoding.UTF8.GetString(bytes.Span))!;
        var crs = ProjectedCrs.TryCreate(document.Metadata?.ReferenceSystem ?? "").ToOption();
        return document.CityObjects.ToSeq()
            .Map(pair => new GeoFeature(Boundary(document, pair.Value), Attributes(pair.Key, pair.Value), crs));
    }

    // The per-CityObject planar footprint: the highest-LoD geometry's boundary vertex indices dereference into
    // the document Vertices pool and dequantize through Transform, then fold into the planar convex hull. A
    // metadata-only object or a degenerate sub-3-point hull falls back to the document AABB.
    static Geometry Boundary(CityJSON.CityJsonDocument document, CityJSON.CityObject city) {
        var scale = document.Transform.ScaleVector3();
        var translate = document.Transform.TranslateVector3();
        var hull = city.Geometry.AsIterable()
            .OrderByDescending(static g => g.Lod ?? "")
            .HeadOrNone()
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
    public static Fin<byte[]> Write(GeoVectorSource source, Seq<GeoFeature> features, Option<ProjectedCrs> crs) =>
        Try.lift(() =>
            source == GeoVectorSource.Shapefile  ? WriteShapefile(features, crs)
          : source == GeoVectorSource.GeoJson    ? WriteGeoJson(features)
          : source == GeoVectorSource.FlatGeobuf ? WriteFlatGeobuf(features)
          : source == GeoVectorSource.GeoParquet ? WriteGeoParquet(features)
          : source == GeoVectorSource.CityJson   ? throw new NotSupportedException("cityjson-egress-unsupported:ingest-only")
          : WriteUniversal(source, features, crs)).Run()
            .MapFail(static error => new BimFault.CodecReject($"geo-vector-write:{error.Message}").ToError());

    static byte[] WriteFlatGeobuf(Seq<GeoFeature> features) {
        using var output = new MemoryStream();
        var kind = features.HeadOrNone()
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
            names.Iter(name => row[name] = f.Attributes.GetOptionalValue(name)?.ToString() ?? "");
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
        NetTopologySuite.IO.Esri.Shapefile.WriteAllFeatures(
            features.Map(static f => (IFeature)new Feature(f.Geometry, f.Attributes)),
            shp, shx, dbf, prj, crs.Map(static c => c.Value).IfNone(""), null);
        return shp.ToArray();
    }

    static byte[] WriteGeoJson(Seq<GeoFeature> features) {
        var collection = new FeatureCollection();
        features.Iter(f => collection.Add(new Feature(f.Geometry, f.Attributes)));
        return JsonSerializer.SerializeToUtf8Bytes(collection, GeoWire.Json);
    }

    static byte[] WriteUniversal(GeoVectorSource source, Seq<GeoFeature> features, Option<ProjectedCrs> crs) {
        GeoGdal.Bootstrap();
        string path = $"/vsimem/{Guid.NewGuid():N}";
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
        try { return ReadVsimem(path); } finally { OSGeo.GDAL.Gdal.Unlink(path); }
    }

    static byte[] ReadVsimem(string path) {
        var handle = OSGeo.GDAL.Gdal.VSIFOpenL(path, "rb");
        OSGeo.GDAL.Gdal.VSIFSeekL(handle, 0, 2);
        var length = (int)OSGeo.GDAL.Gdal.VSIFTellL(handle);
        OSGeo.GDAL.Gdal.VSIFSeekL(handle, 0, 0);
        var buffer = new byte[length];
        OSGeo.GDAL.Gdal.VSIFReadL(buffer, 1, length, handle);
        OSGeo.GDAL.Gdal.VSIFCloseL(handle);
        return buffer;
    }

    static OSGeo.OSR.SpatialReference SpatialRef(ProjectedCrs crs) {
        var srs = new OSGeo.OSR.SpatialReference("");
        srs.SetFromUserInput(crs.Value);
        srs.SetAxisMappingStrategy(OSGeo.OSR.AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
        return srs;
    }
}
```

## [04]-[RASTER_INGEST]

- Owner: `GeoRaster` the GDAL raster ingest owner over `MaxRev.Gdal.Core` — a windowed multi-band `Dataset.ReadRaster<T>` band-stack read placed in georeferenced space by the 6-coefficient `GetGeoTransform` affine and the `GetSpatialRef`/`GetExtent` CRS extent (terrain elevation, an ortho basemap, a slope/aspect surface); `GeoRaster.Contour`/`DemProcess` the DEM-to-vector and hillshade/slope/aspect legs; `RasterTile` the windowed pixel carrier (the polymorphic `RasterBand` `[Union]` typed by the source `Band.DataType`, the band count, the geo-transform, and the NTS `Envelope` extent); `GeoRaster.ToCoverage` the seam `Coverage`-node projection [M1] lowering a placed raster onto the seam graph by content-key reference (the field by-ref + band descriptors + the seam `GeoReference`), never a stored pixel blob on the node.
- Entry: `GeoRaster.Read(ReadOnlyMemory<byte> bytes, Option<Envelope> window, int targetWidth, int targetHeight)` opens the raster through `Gdal.Open` over a `/vsimem/` buffer and reads the windowed band STACK into a `RasterTile` — the `window` mapping to a pixel sub-window through the inverse geo-transform, every band read in one `Dataset.ReadRaster<T>` call typed by the source `Band.DataType`, the target size triggering GDAL on-read resampling — `Fin<T>` aborts on an open/read fault (`BimFault.CodecReject`) lowered with `.ToError()`; `GeoRaster.ToCoverage(RasterTile tile, GeoReference reference, ContentAddress field, ProjectionContext ctx)` projects the placed raster onto a seam `Coverage` node carrying the field content key, the band descriptors, and the seam `GeoReference`; `GeoRaster.Contour(...)`/`GeoRaster.Cog(...)` vectorize and transcode.
- Auto: `GeoRaster.Read` runs `GeoGdal.Bootstrap` once, opens the bytes, reads the `GetGeoTransform` affine and the `GetExtent` NTS `Envelope` directly in the target CRS, and reads the windowed pixels through `Dataset.ReadRaster<T>` into a managed `T[]` matching the `Band.DataType`; `ToCoverage` content-addresses the raster field bytes (the placed pixel buffer persisted to the object store, referenced by `ContentAddress`), folds the band count and `DataType` into the `Coverage` node band descriptors, and stamps the seam `GeoReference` so the coverage carries its CRS [M1] — the pixel buffer never inlines onto the node; `GeoRaster.Contour` runs `wrapper_GDALContourDestName` over the DEM band producing contour `GeoFeature` lines tagged `"contour"`; the raster placement composes the `Semantics/georeference#GEODETIC_TRANSFORM` `ProjNET` leg and escalates to OSR only for the exotic datum-grid transforms `ProjNET` cannot express.
- Receipt: the `RasterTile` is the placed pixel evidence a terrain-mesh tessellation reads; the seam `Coverage` node is the by-reference field the terrain consumer and the `Exchange/export` 3D-Tiles terrain leg read; the contour `GeoFeature` lines are the vectorized terrain the site model indexes.
- Packages: `MaxRev.Gdal.Core`, `MaxRev.Gdal.MacosRuntime.Minimal.arm64`, `NetTopologySuite`, `ProjNET`, `Rasm.Element`, `Rasm`, `LanguageExt.Core`
- Growth: a new raster format is enumerable through the one `Gdal.Open` universal driver path with zero new code; a new DEM derivation is one `wrapper_GDALDEMProcessing` mode; a new resample kernel is one `RasterIOExtraArg`; the seam projection is one `ToCoverage` op; never a per-format raster reader and never an inlined pixel blob on the node.
- Boundary: the raster ingest is `MaxRev.Gdal.Core`'s — `GdalBase.ConfigureAll()` MUST run once before any `OSGeo.*` call and a publish without the matching RID runtime faults at first call (`BimFault.CapabilityMiss`); pixels move through `Dataset.ReadRaster<T>` into a managed `T[]` matching the `Band.DataType` and a hand-rolled raster decoder is the deleted form; reprojection inside a GDAL pipeline uses OSR while managed-geometry reprojection stays the `ProjNET` leg, OSR escalating only the exotic datum-grid transforms; the seam `Coverage` node references the field by content key [M1] and an inlined pixel blob on the node is the deleted form; the tile-pyramid partitioning stays at `Rasm.Compute` consumed at the seam — `Rasm.Bim` authors the COG/contour and the 3D-Tiles terrain leg crosses the seam, never the pyramid.

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

public sealed record RasterTile(
    RasterBand Band,
    int Width,
    int Height,
    int BandCount,
    double[] GeoTransform,
    Envelope Extent,
    OSGeo.GDAL.DataType DataType);

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoRaster {
    public static Fin<RasterTile> Read(ReadOnlyMemory<byte> bytes, Option<Envelope> window, int targetWidth, int targetHeight) =>
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
                var dataType = dataset.GetRasterBand(1).DataType;
                var band = Materialize(dataset, dataType, xOff, yOff, xSize, ySize, targetWidth, targetHeight, bands, bandMap);
                return new RasterTile(band, targetWidth, targetHeight, bands, transform, extent, dataType);
            } finally { OSGeo.GDAL.Gdal.Unlink(path); }
        }).Run().MapFail(static error => new BimFault.CodecReject($"geo-raster:{error.Message}").ToError());

    // The seam Coverage projection [M1]: the placed raster lands a Coverage node referencing the field by
    // content key (the pixel buffer persisted to the object store, NOT inlined), carrying the band descriptors
    // and the seam GeoReference so the terrain consumer reads the field + CRS without a stored blob on the node.
    public static Node.Coverage ToCoverage(RasterTile tile, GeoReference reference, ContentAddress field, ProjectionContext ctx) =>
        new Node.Coverage(
            Id:        ctx.Rooted(),
            Field:     field,
            Bands:     tile.BandCount,
            Width:     tile.Width,
            Height:    tile.Height,
            Reference: reference,
            Sample:    tile.DataType.ToString());

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

    public static Fin<Seq<GeoFeature>> Contour(ReadOnlyMemory<byte> demBytes, double interval) =>
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
        }).Run().MapFail(static error => new BimFault.CodecReject($"geo-contour:{error.Message}").ToError());
}
```

## [05]-[RESEARCH]

- [NTS_PLANAR_ALGEBRA]: the `NetTopologySuite` member spellings the `GEOSPATIAL_SEAM` composes are decompile-verified — `NtsGeometryServices.Instance`, `GeometryOverlay.NG`, `PackedCoordinateSequenceFactory.DoubleFactory`, `STRtree<T>.Insert`/`Query`, `PreparedGeometryFactory.Prepare` + `IPreparedGeometry.Intersects`, `GeometryFixer.Fix`, `OverlayNGRobust.Union`, and `WKBReader.Read`/`WKBWriter.Write` — confirmed against `.api/api-nettopologysuite`; the canonical broad-then-narrow spatial-join rail stacks `STRtree.Query` then `PreparedGeometry.Intersects`.
- [GDAL_UNIVERSAL_INGEST]: the `MaxRev.Gdal.Core` member spellings the `VECTOR_INGEST` universal arm and the `RASTER_INGEST` owner compose are decompile-verified at `.api/api-maxrev-gdal` — `GdalBase.ConfigureAll()`/`IsConfigured`, `Gdal.FileFromMemBuffer`/`Unlink`, `Ogr.Open`/`Layer.SetSpatialFilterRect`/`GetNextFeature`/`Feature.GetGeometryRef`, `Gdal.Open`/`Dataset.ReadRaster<T>`/`GetGeoTransform`/`GetExtent`/`Band.DataType`, `wrapper_GDALContourDestName`/`wrapper_GDALTranslate`/`Warp`, and the OSR `SpatialReference`/`CoordinateTransformation` escalation; the RID runtime pin `MaxRev.Gdal.MacosRuntime.Minimal.arm64` stages the `gdal-data`/PROJ resources, the hard platform constraint the boundary names.
- [SEAM_PROJECTION]: the `GeoFeature.ToObject`/`GeoModel.Project` vector→seam-`Object` and `GeoRaster.ToCoverage` raster→seam-`Coverage` projections ground against `ELEMENT-REBUILD-PLAN.md` §4B (the `Coverage` node raster/field by-ref+bands+CRS; vector features ride `Object`, no parallel Feature family) and §6 (the geospatial projector → Object/Coverage nodes); a vector feature lands an `Object` occurrence discriminated by the generic `Classification("ifc", code)` the `GeoClassifier` resolves through `Model/elements#ELEMENT_MODEL` `IfcClass`, its attributes a `Pset_SiteContext` `PropertySet` node linked by a neutral `Associate(IfcRelDefinesByProperties)` edge, its footprint content-keyed for the `RepresentationContentHash`; a raster lands a `Coverage` node referencing the field by `ContentAddress` and carrying the seam `GeoReference` [M1], the pixel buffer persisted to the object store, never inlined — so a site-context model is a seam graph like any imported model and the seam `Bake` reads it with no second selection surface.
- [REPROJECTION_SEAM]: the geodetic reprojection is the `Semantics/georeference#GEODETIC_TRANSFORM` `GeoTransform.Reproject` `ProjNET` leg over the seam `GeoReference` by default, with `MaxRev.Gdal.Core` OSR the escalation for the exotic datum-grid/dynamic-datum transforms `ProjNET` cannot express; the `GeoFeature.SourceCrs` and the project CRS drive the SRID lookup, an unresolvable CRS leaving the geometry unreprojected rather than faulting so a single-datum site never blocks ingest.

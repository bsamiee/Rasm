# [BIM_GEOSPATIAL]

The georeferenced-BIM site-context owner: one `GeoFeature` canonical row (the OGC Simple-Features `NetTopologySuite` `Geometry` plus its `AttributesTable` and source CRS) the whole geospatial seam materializes and emits, a `GeoModel` carrying the feature set under one `NtsGeometryServices.Instance` precision/SRID configuration and the `STRtree` broad-phase 2D index, and a `GeoIngest` fold admitting every admitted vector and raster source — shapefile (`NetTopologySuite.IO.Esri.Shapefile`, managed), GeoJSON (`NetTopologySuite.IO.GeoJSON4STJ`, the managed `System.Text.Json` codec), CityJSON (`bertt.CityJSON`, managed, 3D-city ingest-only), FlatGeobuf (`FlatGeobuf.NTS`, managed, the NTS-native cloud-optimized row-oriented codec with the Packed-Hilbert-R-tree bbox push-down), GeoParquet (`GISBlox.IO.GeoParquet`, managed, the columnar `DataTable`↔WKB arm over the admitted `ParquetSharp` native engine), and the universal GDAL/OGR driver set covering the GeoPackage SQLite container / KML / GML long-tail plus GeoTIFF/COG/DEM raster (`MaxRev.Gdal.Core`) — onto that one carrier. The planar Simple-Features algebra is `NetTopologySuite`'s; the `GeoFeature`'s two wire projections are the managed GeoJSON text (`NetTopologySuite.IO.GeoJSON4STJ`) the cross-runtime Python `shapely`/TS `turf` peers decode and the GeoPackage binary blob (`NetTopologySuite.IO.GeoPackage`) the `csharp:Rasm.Persistence/Store` geo-store-blob persists — the only two wire forms the `data-interchange#GEO_INTERCHANGE` law admits over the one NTS interior vocabulary; the geodetic reprojection composes the `georeference#GEODETIC_TRANSFORM` `ProjNET` leg (escalating to `MaxRev.Gdal.Core` OSR for the exotic datum-grid transforms `ProjNET` cannot express); the site-context `BimElement` projection reuses the `Model/elements#ELEMENT_MODEL` vocabulary so a parcel, a terrain TIN, or a city building lands as a `BimElement` like any imported element. The page is HOST-NEUTRAL: NTS owns the 2D planar geometry, the kernel `Rasm` owns the 3D solid geometry, and the two meet only at the in-process WKB/`CoordinateSequence`-buffer kernel wire (distinct from the cross-runtime GeoJSON peer wire) — a RhinoCommon binding on a geospatial owner is the named seam violation.

## [01]-[INDEX]

- [01]-[GEOSPATIAL_SEAM]: `GeoFeature` canonical row, `GeoModel` feature set + `STRtree` broad-phase, the `NtsGeometryServices.Instance` precision/SRID configuration root, the planar predicate/overlay/spatial-join surface, and the `GeoFeature.ToElement` site-context `BimElement` projection.
- [02]-[VECTOR_INGEST]: `GeoVector` fold over the `GeoVectorSource` `[SmartEnum]` — the managed shapefile/GeoJSON/CityJSON + FlatGeobuf (row-oriented, bbox push-down) + GeoParquet (columnar, column push-down) arms and the GeoPackage/OGR-universal arm producing `GeoFeature` rows, the OGR↔NTS WKB bridge the GeoParquet geo-column cell shares, the MBR/bbox/column/attribute server-side push-down, and the symmetric write side.
- [03]-[RASTER_INGEST]: `GeoRaster` GDAL raster ingest (GeoTIFF/COG/DEM windowed `ReadRaster<T>`), the geo-transform/extent placement, the DEM contour/hillshade vectorization to `GeoFeature`, and the COG transcode.

## [02]-[GEOSPATIAL_SEAM]

- Owner: `GeoFeature` the one host-neutral geospatial row carrying its `NetTopologySuite.Geometries.Geometry` planar geometry, its `IAttributesTable` keyed property bag, and its `Option<ProjectedCrs>` source CRS (the `georeference#GEO_REFERENCE` value object); `GeoModel` the feature set under one `GeometryFactory` resolved from `NtsGeometryServices.Instance`, carrying the lazily-built `STRtree<GeoFeature>` broad-phase 2D index keyed by `Geometry.EnvelopeInternal`; `GeoServices` the process-wide `NtsGeometryServices` configuration root set once with the robust `GeometryOverlay.NG` engine and the dense `PackedCoordinateSequenceFactory`; `GeoClassifier` the frozen `OgcGeometryType`/`CityObjectType`→IFC-class table the site-context projection keys on.
- Entry: `GeoModel.Of(Seq<GeoFeature> features)` indexes the features into the `STRtree` once; `GeoModel.SpatialJoin(Geometry probe)` runs the canonical broad-then-narrow rail — `STRtree.Query(probe.EnvelopeInternal)` for the envelope-overlap candidate set, then `PreparedGeometryFactory.Prepare(probe).Intersects` for the exact DE-9IM predicate per candidate — returning the matched `GeoFeature` set without an O(N) scan; `GeoFeature.ToElement(GeoReference reference, ClockPolicy clocks)` projects one feature onto a `Model/elements#ELEMENT_MODEL` `BimElement` row with a `GeoClassifier`-resolved `IfcClass`, reprojecting the geometry into the canonical kernel frame through `reference.Reproject` and threading the `AttributesTable` onto the element `PropertyBinding` set — `Fin<T>` aborts on a feature whose `OgcGeometryType` the classifier table does not carry (`Model/faults#FAULT_BAND` `BimFault.UnmappedClass`) or an invalid geometry `GeometryFixer.Fix` cannot repair (`BimFault.ModelRejected`), each lowered with `.ToError()`.
- Auto: `GeoServices.Configure` sets `NtsGeometryServices.Instance` exactly once at module init behind an idempotency guard with `GeometryOverlay.NG` (the robust `OverlayNG` boolean engine) and a `PackedCoordinateSequenceFactory` (the struct-of-arrays `double[]` layout a kernel buffer maps onto without per-point `Coordinate` boxing), so every reader, the shapefile codec, and the OGR ingest resolve cached factories carrying the one canonical `PrecisionModel`/`SRID` and a per-call `new GeometryFactory()` is the rejected precision-fragmenting form; `GeoModel.Of` bulk-`Insert`s each feature's `Geometry.EnvelopeInternal` envelope into the `STRtree` (read-only after the first `Query`, lazily built), so a point-in-zone classification of many elements against one footprint or a clash narrow-phase candidate set is one broad-then-narrow pass mirroring the `Model/systems#INTERFERENCE` 3D BVH projected to 2D; `GeoModel.Dissolve` folds a parcel/footprint set through `OverlayNGRobust.Union` (the noded robust dissolve) after a `GeometryFixer.Fix` validity gate so a malformed OGR/shapefile ring never poisons the boolean engine; `GeoFeature.ToElement` reads the `GeoClassifier` table for the IFC class, reprojects the geometry ordinates through the `georeference#GEODETIC_TRANSFORM` `ProjNET` `MathTransform` into the project CRS, and folds the `IAttributesTable` `GetNames`/`GetValues` onto the `Pset_SiteContext` `PropertyBinding` set so the geospatial attributes ride the typed property store the rest of Bim reads.
- Receipt: the `GeoFeature` is the typed planar-geometry evidence a site clash, a parcel-boundary setback check, or an infrastructure placement reads; the `GeoModel.STRtree` index is the broad-phase candidate generator the spatial-join and overlay folds consume; the projected `BimElement` is a `Model/elements#ELEMENT_MODEL` row discriminated by the same `IfcClass`/`PredefinedType` vocabulary an imported element carries, so the `Model/query#ELEMENT_SET` algebra and the `Review/validation#IDS_FACETS` audit read a site-context model with no second selection surface.
- Packages: `NetTopologySuite`, `NetTopologySuite.IO.GeoJSON4STJ`, `NetTopologySuite.IO.GeoPackage`, `ProjNET`, `GeometryGymIFC_Core`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`, `NodaTime`, `Rasm`
- Growth: a new planar predicate is one `Geometry` instance-method call on the existing algebra; a new spatial index is the `STRtree`/`Quadtree` swap the `GeoModel` carrier owns; a new site-context class mapping is one `GeoClassifier` table row keyed on `(OgcGeometryType, CityObjectType)`; a new attribute projection is one `Pset_SiteContext` `PropertyBinding` column; never a parallel planar geometry world beside NTS, never a per-feature-kind `GeoFeature` subtype, and never a second precision/SRID configuration beside `NtsGeometryServices.Instance`.
- Boundary: the planar Simple-Features algebra is `NetTopologySuite`'s — the `Geometry` type hierarchy, the DE-9IM `Intersects`/`Contains`/`Relate` predicates, the `OverlayNG` robust boolean, the `STRtree` index, and `PreparedGeometry` repeated-query acceleration are the package's, and a hand-rolled planar intersection or a second R-tree is the deleted form; the `NtsGeometryServices.Instance` global is the single precision/SRID owner configured once and a per-call factory is the rejected form; validity repair enters through `GeometryFixer.Fix` before any overlay or write and trusting a raw OGR/shapefile ring into the boolean engine is the deleted form; the geodetic reprojection composes the `georeference#GEODETIC_TRANSFORM` `ProjNET` leg and a `NetTopologySuite`-side datum shift is the named seam violation (NTS owns the planar algebra on both sides of the transform, never the transform); the 3D solid geometry stays the kernel `Rasm`'s and a geospatial owner carrying a RhinoCommon `Brep`/`Mesh` is the host-bound defect — NTS 2D planar geometry crosses to the kernel ONLY as a `CoordinateSequence` ordinate buffer (or its `Geometry.ToBinary()` WKB form) the kernel constrained-Delaunay realize pass `csharp:Rasm/Geometry/meshing/delaunay#TESSELLATION` triangulates into the 3D `GeometryHandle`, so a parcel/terrain footprint realizes its handle through the kernel planar-triangulation arm rather than staying permanently `Pending`, and this in-process kernel wire is DISTINCT from the cross-runtime GeoJSON peer wire (the footprint's `Pset_SiteContext` GeoJSON property the `shapely`/`turf` peers decode) — emitting a WKB-hex string on the peer wire is the rejected form `data-interchange#GEO_INTERCHANGE` deletes; the site-context projection mints a `Model/elements#ELEMENT_MODEL` `BimElement` and a parallel `GeoElement`/`SiteElement` record beside `BimElement` is the deleted form; the `GeoClassifier` is a frozen data table keyed on `(OgcGeometryType, CityObjectType)`, never enumerated `switch` arms.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Data;
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
using NodaTime;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Bim;

// --- [SERVICES] ---------------------------------------------------------------------------
// The single planar-geometry configuration root: NtsGeometryServices.Instance is the one global
// PrecisionModel/SRID/overlay owner. Configured once at module init with the robust OverlayNG
// engine and the dense PackedCoordinateSequenceFactory so every reader, codec, and the OGR ingest
// resolve cached factories carrying one precision — a per-call `new GeometryFactory()` fragments it.
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

    public Fin<BimElement> ToElement(GeoReference reference, ClockPolicy clocks) =>
        GeoClassifier.Classify(this)
            .Bind(row => row.Class.AdmitPredefined(row.Predefined, row.Predefined)
                .Map(predefined => (Class: row.Class, Predefined: predefined)))
            .Map(row => {
                // The planar footprint reprojects in place onto the project CRS, then crosses TWO distinct wires.
                // (1) The cross-runtime peer wire: the geometry rides a GeoJSON Pset_SiteContext property the
                // Python `shapely.from_geojson` and TS `turf`/`GeoJsonWire` (RFC 7946) peers decode — the one
                // canonical geometry wire `data-interchange#GEO_INTERCHANGE` admits (a WKB-hex string is the
                // rejected wire form; raw WKB is the in-process kernel wire only). (2) The kernel wire: the
                // GeometryHandle stays Pending(globalId) until the kernel constrained-Delaunay realize pass
                // (`csharp:Rasm/Geometry/meshing/delaunay#TESSELLATION`) triangulates the reprojected footprint
                // boundary (its outer ring a `Constraint.Segment` loop, each hole a void loop) over the
                // CoordinateSequence ordinate buffer into the 3D handle — exactly as every BimElement defers
                // geometry realization through `Exchange/tessellation`, the parcel/terrain footprint differing
                // only in taking the kernel planar-triangulation arm rather than the IFC companion. The handle
                // is keyed by the deterministic GlobalId so the realize pass binds it back by reference.
                var footprint = Reproject(reference);
                var globalId = ParserIfc.HashGlobalID($"geo:{row.Class.Key}:{footprint.Attributes.GetOptionalValue("id") ?? footprint.Geometry.ToText()}");
                return new BimElement(
                    GlobalId:           globalId,
                    Class:              row.Class,
                    Predefined:         row.Predefined,
                    Name:               (footprint.Attributes.GetOptionalValue("name") ?? row.Class.Key).ToString()!,
                    Tag:                (footprint.Attributes.GetOptionalValue("id") ?? "").ToString()!,
                    Geometry:           GeometryHandle.Pending(globalId),
                    Properties:         footprint.Attributes.GetNames()
                                            .Map(name => new BimElement.PropertyBinding("Pset_SiteContext", name, footprint.Attributes[name]?.ToString() ?? ""))
                                            .ToSeq()
                                            .Add(new BimElement.PropertyBinding("Pset_SiteContext", "Footprint", GeoWire.ToGeoJson(footprint))),
                    Quantities:         Seq<BimElement.QuantityBinding>(),
                    Materials:          Seq<BimMaterial>(),
                    Classifications:    Seq<ClassificationRef>(),
                    TypeGlobalId:       Option<string>.None,
                    SpatialContainerId: Option<string>.None);
            });

    // The packed PackedCoordinateSequence materializes a detached Coordinate[] for Geometry.Coordinates, so
    // the reprojected ordinates persist back through an ICoordinateSequenceFilter (Geometry.Apply visits the
    // live sequences in Coordinates order) — a write into the detached Coordinate[] would silently no-op.
    GeoFeature Reproject(GeoReference reference) {
        if (!reference.IsGeoreferenced) {
            return this;
        }
        var coords = Geometry.Coordinates;
        var span = new float[coords.Length * 3];
        for (int i = 0; i < coords.Length; i++) {
            (span[i * 3], span[i * 3 + 1], span[i * 3 + 2]) = ((float)coords[i].X, (float)coords[i].Y, (float)(double.IsNaN(coords[i].Z) ? 0.0 : coords[i].Z));
        }
        reference.Reproject(span, stride: 3);
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

    public Fin<Seq<BimElement>> Project(GeoReference reference, ClockPolicy clocks) =>
        Features.TraverseM(f => f.ToElement(reference, clocks)).As();
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

- Owner: `GeoVector` the universal vector ingest-and-egress fold over `GeoVectorSource`, the `[SmartEnum<string>]` source table whose rows carry the managed-versus-GDAL reader discriminant — `Shapefile` (`NetTopologySuite.IO.Esri.Shapefile`), `GeoJson` (`NetTopologySuite.IO.GeoJSON4STJ`, the managed `System.Text.Json` converter-factory), `CityJson` (`bertt.CityJSON`, ingest-only), `FlatGeobuf` (`FlatGeobuf.NTS`, the NTS-native cloud-optimized row-oriented codec with the Packed-Hilbert-R-tree bbox push-down), and `GeoParquet` (`GISBlox.IO.GeoParquet` over the admitted `ParquetSharp` native engine, the columnar `DataTable`↔WKB arm) decode through their dedicated managed codecs, `GeoPackage` and the long-tail OGR-only drivers (KML, GML, FileGDB, PostGIS, …) through the one `MaxRev.Gdal.Core` OGR universal reader — every arm producing the canonical `GeoFeature` row; `GeoWire` the `GeoFeature`'s two canonical wire projections per `data-interchange#GEO_INTERCHANGE` (GeoJSON text the cross-runtime `shapely`/`turf` peers decode + the GeoPackage binary blob the Persistence geo-store-blob persists); `GeoWkb` the bidirectional OGR↔NTS bridge (`OSGeo.OGR.Geometry.ExportToWkb` → `NetTopologySuite.IO.WKBReader.Read`, and `WKBWriter.Write` → `OSGeo.OGR.Geometry.CreateFromWkb`) the universal driver path crosses at the wire on both the read and the write leg — the SAME WKB bridge `NetTopologySuite.IO.WKBReader.Read` the GeoParquet geo-column cell crosses, so an OGR feature, a GeoParquet `DataTable` cell, and an FGB `FlatGeobufCoordinateSequence` land the identical NTS geometry.
- Entry: `GeoVector.Read(GeoVectorSource source, ReadOnlyMemory<byte> bytes, Option<Envelope> clip)` decodes a vector source onto `Seq<GeoFeature>` — the `Shapefile` arm through `NetTopologySuite.IO.Esri.Shapefile.Shapefile.OpenRead` (streaming, the `clip` envelope driving `ShapefileReaderOptions.MbrFilter` server-side window push-down and `Factory` seeded from `NtsGeometryServices.Instance`), the `CityJson` arm through Newtonsoft deserialization into `CityJsonDocument` then a `Transform`-dequantized boundary fold per `CityObject`, the `FlatGeobuf` arm through `FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(stream, rect)` (the `clip` envelope pushed DOWN through the Packed-Hilbert-R-tree bbox index so only the overlapping feature runs decode — the managed equivalent of `Layer.SetSpatialFilterRect`, never an `Ogr.Open`), the `GeoParquet` arm through `GISBlox.IO.GeoParquet.GeoParquetReader.ReadAll`/`ReadColumns` over the `ParquetSharp` native engine then a per-row `WKBReader.Read` of the geo-column WKB cell (the columnar `DataTable`↔`GeoFeature` bridge), the universal arm through `Ogr.Open` over a `/vsimem/` in-memory buffer then `Layer.SetSpatialFilterRect(clip)`/`Layer.GetNextFeature`/`Feature.GetGeometryRef.ExportToWkb` → `WKBReader.Read` — `Fin<T>` aborts on a corrupt container (`Model/faults#FAULT_BAND` `BimFault.CodecReject`) lowered with `.ToError()` at the boundary, projecting the `ShapefileException`/`GeometryException`/OGR `ApplicationException` onto the rail so domain code never sees a driver exception; `GeoVector.Write(GeoVectorSource source, Seq<GeoFeature> features, Option<ProjectedCrs> crs)` emits the symmetric byte payload through `Shapefile.WriteAllFeatures` (the `DbfField` schema inferred from the first feature's `AttributesTable`), `FeatureCollectionConversions.Serialize` (the FGB Hilbert-sorted body), or `GeoParquetWriter.Write` (the columnar `DataTable` with the OGC `geo` metadata).
- Auto: the `GeoVectorSource` row discriminant routes the decode without a call-site branch — a `Shapefile.OpenRead` yields `NetTopologySuite.Features.Feature` directly (the codec output IS the canonical NTS feature shape, no intermediate shapefile record), its `MbrFilter` skipping records outside the `clip` envelope before decoding their geometry so a continental basemap clips to the project extent cheaply; the `CityJson` arm reads `CityObject.Geometry[i]` discriminating on `GeometryType` and recovering real coordinates as `vertex × Transform.ScaleVector3() + Transform.TranslateVector3()`, mapping each `CityObjectType` onto the `GeoClassifier` tag so a `Building`/`Bridge`/`Relief` urban feature lands its `IfcClass`, and reading `Metadata.ReferenceSystem` (the source CRS URN) onto the `GeoFeature.SourceCrs`; the `FlatGeobuf` arm `Deserialize(stream, rect)` pushes the `clip` envelope DOWN through the file's Packed-Hilbert-R-tree index so only the bbox-overlapping feature runs decode (the row-oriented managed push-down, the `FlatGeobufCoordinateSequence` landing the canonical `GeoServices` precision), and a remote `.fgb` escalates to `PackedRTree.StreamSearch` over an `ObjectStore` byte-range `ReadNode`; the `GeoParquet` arm `ReadGeoMetadata` reads the OGC `geo` header (primary column + CRS + bbox) before rows, `ReadColumns` projects only the geometry + needed attribute columns server-side (the COLUMN push-down, the columnar analog of the FGB bbox ROW push-down), and each geo-column WKB cell `WKBReader.Read`-bridges to the `GeoFeature` `Geometry` over the `ParquetSharp` native engine; the universal arm runs the `GeoGdal.Bootstrap` `GdalBase.ConfigureAll()` once, opens the bytes through `Gdal.FileFromMemBuffer("/vsimem/in", …)` + `Ogr.Open`, pushes the `clip` down through `Layer.SetSpatialFilterRect` and any attribute filter through `Layer.SetAttributeFilter` so the driver evaluates the filter server-side, then bridges each `Feature.GetGeometryRef.ExportToWkb` buffer through `WKBReader.Read` into an NTS `Geometry` (GEOS stays inside the GDAL native boundary — the managed `NetTopologySuite` is the one planar algebra and a boolean op on the OGR side is the rejected form); every produced `GeoFeature` is `GeometryFixer.Fix`-repaired at admission so a degenerate ring never reaches the index.
- Receipt: the `GeoVector.Read` `Seq<GeoFeature>` is the universal vector ingest evidence the `GEOSPATIAL_SEAM` `GeoModel` indexes; the `GeoVectorSource` row records which codec decoded (the managed codec for shapefile/GeoJSON/CityJSON and the FlatGeobuf row-oriented + GeoParquet columnar managed arms, the GDAL OGR universal path for GeoPackage and the OGR-only long-tail formats) so a non-conforming source is enumerable and the reader is one table read.
- Packages: `NetTopologySuite`, `NetTopologySuite.IO.Esri.Shapefile`, `bertt.CityJSON`, `FlatGeobuf`, `GISBlox.IO.GeoParquet`, `MaxRev.Gdal.Core`, `NodaTime`, `Thinktecture.Runtime.Extensions`, `LanguageExt.Core`
- Growth: a new managed vector codec is one `GeoVectorSource` `managed:true` row carrying its reader discriminant (the FlatGeobuf row-oriented and GeoParquet columnar arms are the pattern); a new OGR-only long-tail vector format is enumerable through the existing universal `Ogr.Open` arm with zero new row (GDAL covers it); a new attribute push-down is one `Layer.SetAttributeFilter` (OGR) or `ReadColumns` (GeoParquet) argument; never a per-format `ShapefileImporter`/`GeoJsonImporter` service family, never a hand-rolled `.shp`/`.dbf`/`.fgb` binary record, and never a boolean op on the OGR/GEOS side.
- Boundary: the shapefile codec is the pure-managed `NetTopologySuite.IO.Esri.Shapefile` default (no native dependency), FlatGeobuf is the pure-managed `FlatGeobuf.NTS` NTS-native codec, and GeoParquet is the managed `GISBlox.IO.GeoParquet` columnar codec over the already-admitted `ParquetSharp` native engine — the OGR `"ESRI Shapefile"`/`"FlatGeobuf"`/`"Parquet"` drivers are reserved for formats only GDAL covers and admitting GDAL for a format a managed codec reads is the rejected form (FGB/GeoParquet route through their managed arms, NEVER `Ogr.Open`); the managed codec output IS the canonical `NetTopologySuite.Features.Feature` (FGB exchanges `IFeature` directly; GeoParquet exchanges a `DataTable` whose geo column WKB-bridges to the `GeoFeature` via `WKBReader`/`WKBWriter`) and a translation through a shapefile-specific record type is the deleted form; the OGR↔NTS bridge is the `ExportToWkb`→`WKBReader.Read` / `WKBWriter.Write`→`CreateFromWkb` wire — the SAME `WKBReader.Read`/`WKBWriter.Write` the GeoParquet geo-column cell crosses — and running planar boolean ops on the OGR side fragments the one topology owner; the `GdalBase.ConfigureAll()` bootstrap runs once per process behind the `GdalBase.IsConfigured` guard and a per-open configure is the rejected form; `Gdal.UseExceptions()` flips the SWIG error model to thrown so a failed open lowers onto `BimFault.CodecReject` at the boundary rather than a domain branch on a raw `CPLErr`; the CityJSON quantization is lossless (`Geometry.Boundaries` are integer indices into `CityJsonDocument.Vertices`, recovered through `Transform`) and tessellating it in the codec is the deleted form; `CityJSON.*`/`OSGeo.*`/`FlatGeobuf.*`/`GISBlox.*`/shapefile reader types never leak past this fold — internal code holds the canonical `GeoFeature` per the boundary-mapping law.

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
// The GeoFeature's two canonical wire projections per data-interchange#GEO_INTERCHANGE: NetTopologySuite is
// the SINGLE interior geo vocabulary and GeoJSON text plus the GeoPackage binary blob are its ONLY two wire
// forms (a coordinate DTO, a raw WKB-hex string, or a vendor geometry type crossing the wire is the rejected
// shape). The GeoJSON text wire (NetTopologySuite.IO.GeoJSON4STJ converter-factory) is the cross-runtime
// geometry wire the Python shapely.from_geojson and TS turf/GeoJsonWire (RFC 7946) peers decode and the
// managed GeoJson ingest reads; the GeoPackage binary blob (NetTopologySuite.IO.GeoPackage, the GP-magic
// header + WKB body) is the csharp:Rasm.Persistence/Store geo-store-blob projection. CRS is fixed by the
// GeoJSON format to WGS84 lon/lat so reprojection stays interior-only (the georeference#GEODETIC_TRANSFORM
// leg) and the GeoServices.Factory PrecisionModel is admitted as coordinates parse — the one precision owner.
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

    // The GeoPackage binary-blob projection: GeoPackageGeoWriter emits the GP-header+WKB blob the Persistence
    // geo-store-blob column persists, GeoPackageGeoReader admits it back — the second wire form, never the
    // .gpkg SQLite CONTAINER (that rides the GDAL universal arm because Bim admits no embedded-store engine).
    static readonly GeoPackageGeoReader BlobReader = new() { HandleSRID = true, HandleOrdinates = Ordinates.XYZ };
    static readonly GeoPackageGeoWriter BlobWriter = new() { HandleOrdinates = Ordinates.XYZ };

    public static byte[] ToGpkgBlob(GeoFeature feature) => BlobWriter.Write(feature.Geometry);
    public static Geometry FromGpkgBlob(byte[] blob) => BlobReader.Read(blob);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class GeoVector {
    // The dataset bytes arrive from the csharp:Rasm.Persistence/Store ObjectStore.Fetch transport at the
    // Semantics/geospatial <- Rasm.Persistence/Store [TRANSPORT] seam (consumed as settled wire vocabulary, no
    // upward project reference) — this owner is source-agnostic on ReadOnlyMemory<byte> and the GDAL /vsimem
    // open is Bim's, never a Persistence GDAL reference.
    public static Fin<Seq<GeoFeature>> Read(GeoVectorSource source, ReadOnlyMemory<byte> bytes, Option<Envelope> clip) =>
        Try.lift(() => source == GeoVectorSource.Shapefile  ? Shapefile(bytes, clip)
                     : source == GeoVectorSource.GeoJson    ? GeoJson(bytes, clip)
                     : source == GeoVectorSource.CityJson   ? CityJson(bytes)
                     : source == GeoVectorSource.FlatGeobuf ? FlatGeobuf(bytes, clip)
                     : source == GeoVectorSource.GeoParquet ? GeoParquet(bytes, clip)
                     : Universal(source, bytes, clip)).Run()
            .MapFail(static error => new BimFault.CodecReject($"geo-vector:{error.Message}").ToError());

    // The managed FlatGeobuf arm: NetTopologySuite.IO.GeoJSON4STJ's row-oriented cloud-optimized peer — the
    // FGB is NTS-native end to end (FlatGeobuf.NTS.FeatureCollectionConversions exchanges IFeature directly, no
    // vendor row), and `Deserialize(stream, rect)` pushes the clip envelope DOWN through the Packed-Hilbert-R-tree
    // bbox index so a continental .fgb decodes only the overlapping feature runs — the managed equivalent of the
    // GDAL OGR Layer.SetSpatialFilterRect, never an Ogr.Open over /vsimem (api-flatgeobuf). The FlatGeobufCoordinate-
    // Sequence is a dense NTS CoordinateSequence so the geometry lands the canonical GeoServices precision. A remote
    // .fgb too large to fetch whole escalates to FlatGeobuf.Index.PackedRTree.StreamSearch over an ObjectStore
    // byte-range ReadNode (the streaming counterpart of /vsicurl) at the Persistence transport seam.
    static Seq<GeoFeature> FlatGeobuf(ReadOnlyMemory<byte> bytes, Option<Envelope> clip) {
        using var fgb = new MemoryStream(bytes.ToArray());
        var rect = clip.MatchUnsafe(env => env, () => null);
        return global::FlatGeobuf.NTS.FeatureCollectionConversions.Deserialize(fgb, rect)
            .Map(f => new GeoFeature(f.Geometry, f.Attributes, Option<ProjectedCrs>.None)).ToSeq();
    }

    // The managed GeoParquet COLUMNAR arm: GISBlox.IO.GeoParquet over the admitted ParquetSharp native engine
    // (csharp:Rasm.Persistence#api-parquetsharp), distinct from the row-oriented FGB/shapefile codecs — the no-new-
    // native-runtime columnar leg for a web-published parcel/building set. ReadColumns is the COLUMN push-down (read
    // only the geometry + needed attribute columns of a wide dataset, the columnar analog of the FGB bbox row push-
    // down), the geo column holds WKB byte[] the canonical NTS WKBReader.Read bridges to the GeoFeature Geometry —
    // the SAME WKB bridge the OGR universal arm crosses (api-gisblox-geoparquet) — and the non-geo columns fold onto
    // the AttributesTable. GeoParquet carries no server-side bbox filter, so the clip filters client-side after the
    // columnar projection; routing it through the GDAL OGR "Parquet" driver is the rejected form the managed codec owns.
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

    // The non-geometry DataTable columns fold onto the canonical AttributesTable (the columnar peer of the OGR
    // AttributesOf field walk and the shapefile DbfField row), skipping the WKB-bearing primary geo column.
    static IAttributesTable RowAttributes(System.Data.DataTable table, System.Data.DataRow row, string primary) {
        var attributes = new AttributesTable();
        foreach (System.Data.DataColumn column in table.Columns) {
            if (column.ColumnName != primary) {
                attributes.Add(column.ColumnName, row.IsNull(column) ? "" : row[column]);
            }
        }
        return attributes;
    }

    // The managed GeoJSON arm: NetTopologySuite.IO.GeoJSON4STJ deserializes the whole FeatureCollection through
    // the one GeoWire converter-factory profile (the canonical text wire). GeoJSON carries no server-side
    // push-down, so the clip envelope filters client-side after decode; routing GeoJSON through GDAL OGR is the
    // rejected form — the managed codec IS the wire projection the standard mandates.
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
    // the document Vertices pool and dequantize through Transform (real = index-vertex × Scale + Translate),
    // then fold into the planar convex hull — a per-object footprint, never the whole-document envelope. A
    // metadata-only object (no geometry) or a degenerate sub-3-point hull falls back to the document AABB.
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

    // The boundary-index flatten over the closed CityJSON geometry-dimensionality union — surface (int[][][]),
    // solid (int[][][][]), and the multi/composite-solid rank one above each carry an int-leaf nest, so one
    // recursive System.Array walk yields the leaf vertex indices for any rank.
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

    // The symmetric egress dispatches on the SAME GeoVectorSource row Read decodes: the managed shapefile and
    // GeoJSON codecs emit directly, the GDAL OGR universal arm (Driver.CreateDataSource over /vsimem) emits the
    // long-tail OGR-only container formats (GeoPackage/KML/GML/FileGDB). CityJSON carries NO write arm — it is
    // an ingest-only 3D-city source whose dequantized solids a planar GeoFeature set cannot losslessly re-emit.
    public static Fin<byte[]> Write(GeoVectorSource source, Seq<GeoFeature> features, Option<ProjectedCrs> crs) =>
        Try.lift(() =>
            source == GeoVectorSource.Shapefile  ? WriteShapefile(features, crs)
          : source == GeoVectorSource.GeoJson    ? WriteGeoJson(features)
          : source == GeoVectorSource.FlatGeobuf ? WriteFlatGeobuf(features)
          : source == GeoVectorSource.GeoParquet ? WriteGeoParquet(features)
          : source == GeoVectorSource.CityJson   ? throw new NotSupportedException("cityjson-egress-unsupported:ingest-only")
          : WriteUniversal(source, features, crs)).Run()
            .MapFail(static error => new BimFault.CodecReject($"geo-vector-write:{error.Message}").ToError());

    // The managed FlatGeobuf egress: FeatureCollectionConversions.Serialize streams the header (bbox + CRS + column
    // schema), builds the Packed-Hilbert-R-tree over the feature envelopes, and writes the Hilbert-sorted body —
    // the symmetric counterpart of the FlatGeobuf read arm, the homogeneous-layer GeometryType resolved from the
    // first feature through GeometryConversions.ToGeometryType (api-flatgeobuf); never a GDAL transcode for the
    // NTS-native format the managed codec owns.
    static byte[] WriteFlatGeobuf(Seq<GeoFeature> features) {
        using var output = new MemoryStream();
        var kind = features.HeadOrNone()
            .Map(static f => global::FlatGeobuf.NTS.GeometryConversions.ToGeometryType(f.Geometry))
            .IfNone(global::FlatGeobuf.GeometryType.Unknown);
        global::FlatGeobuf.NTS.FeatureCollectionConversions.Serialize(
            output, features.Map(static f => (IFeature)new Feature(f.Geometry, f.Attributes)), kind, dimensions: 3, columns: null);
        return output.ToArray();
    }

    // The managed GeoParquet COLUMNAR egress: the Seq<GeoFeature> projects to a DataTable (geometry -> WKB byte[]
    // cell via WKBWriter, attributes -> typed columns), the geo column tagged through the GISBlox Extensions schema
    // surface (AddGeoColumn/SetAsPrimaryGeoColumn/AddGeoProcessingMetadata embedding the OGC `geo` file metadata),
    // and GeoParquetWriter.Write(stream, …) emits the columnar payload over the ParquetSharp native engine — the
    // columnar counterpart of the row-oriented FGB/shapefile egress, with no new native runtime beyond ParquetSharp's.
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

    // The managed GeoJSON egress: the whole FeatureCollection serializes through the one GeoWire converter-factory
    // profile (RFC 9746 ring orientation enforced at emission, the GeoServices.Factory precision), the symmetric
    // counterpart of the GeoJson read arm — never a GDAL transcode for the format the managed codec owns.
    static byte[] WriteGeoJson(Seq<GeoFeature> features) {
        var collection = new FeatureCollection();
        features.Iter(f => collection.Add(new Feature(f.Geometry, f.Attributes)));
        return JsonSerializer.SerializeToUtf8Bytes(collection, GeoWire.Json);
    }

    // The GDAL OGR universal egress: the source row's driver creates a /vsimem dataset, each GeoFeature bridges
    // NTS -> OGR through WKBWriter.Write -> Geometry.CreateFromWkb -> Feature.SetGeometry (the same wire the
    // ingest universal arm crosses in reverse), and the written container reads back through the GDAL VSI file
    // API (the inverse of Gdal.FileFromMemBuffer). The one driver path covers the OGR-only GeoPackage/KML/GML/
    // FileGDB write with zero per-format writer; running a boolean op on the OGR side stays the rejected form.
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

    // The GDAL virtual-filesystem byte read-back: the VSI*L file API reads the /vsimem container the OGR driver
    // wrote (seek-to-end sizes the buffer, one bulk read drains it), the inverse of the FileFromMemBuffer the
    // ingest arm pushes bytes in with — so a write never touches a real filesystem path.
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

    // The OGR write SRS: the project CRS user-input string resolved to a SpatialReference under the GDAL-3
    // traditional GIS axis order so the written ordinates never axis-swap against the GeoServices SRID.
    static OSGeo.OSR.SpatialReference SpatialRef(ProjectedCrs crs) {
        var srs = new OSGeo.OSR.SpatialReference("");
        srs.SetFromUserInput(crs.Value);
        srs.SetAxisMappingStrategy(OSGeo.OSR.AxisMappingStrategy.OAMS_TRADITIONAL_GIS_ORDER);
        return srs;
    }
}
```

## [04]-[RASTER_INGEST]

- Owner: `GeoRaster` the GDAL raster ingest owner over `MaxRev.Gdal.Core` — a windowed multi-band `Dataset.ReadRaster<T>` band-stack read placed in georeferenced space by the 6-coefficient `GetGeoTransform` affine and the `GetSpatialRef`/`GetExtent` CRS extent, the GeoTIFF/COG/DEM source the site model reads (terrain elevation, an ortho basemap, a slope/aspect surface); `GeoRaster.Contour` the DEM-to-vector leg through `Gdal.wrapper_GDALContourDestName` projecting iso-contour `GeoFeature` lines into the site model, and `GeoRaster.DemProcess` the `Gdal.wrapper_GDALDEMProcessing` hillshade/slope/aspect derivation; `RasterTile` the windowed pixel carrier (the polymorphic `RasterBand` `[Union]` typed by the source `Band.DataType` — `Floats`/`Bytes`/`Ints` so a DEM lands `float[]` and an ortho `byte[]` — the band count, the geo-transform, and the NTS `Envelope` extent).
- Entry: `GeoRaster.Read(ReadOnlyMemory<byte> bytes, Option<Envelope> window, int targetWidth, int targetHeight)` opens the raster through `Gdal.Open` over a `/vsimem/` buffer and reads the windowed band STACK into a `RasterTile` — the `window` envelope mapping to a pixel sub-window through the inverse geo-transform (clamped to the raster bounds, `None` reading the full coverage), every band read in one `Dataset.ReadRaster<T>` call typed by the source `Band.DataType` through the `Materialize` dispatch, and the `targetWidth`/`targetHeight` differing from the window size triggering GDAL on-read resampling (the kernel selected by `RasterIOExtraArg`) so a DEM downsamples to a working resolution in one call — `Fin<T>` aborts on an open/read fault (`Model/faults#FAULT_BAND` `BimFault.CodecReject`) lowered with `.ToError()`; `GeoRaster.Contour(ReadOnlyMemory<byte> demBytes, double interval)` vectorizes the DEM band into iso-contour `GeoFeature` lines the `GEOSPATIAL_SEAM` `GeoModel` indexes; `GeoRaster.Cog(ReadOnlyMemory<byte> bytes)` transcodes a GeoTIFF to a Cloud-Optimized GeoTIFF through `Gdal.wrapper_GDALTranslate` with the `"COG"` driver and `BuildOverviews` pyramid for the 3D-Tiles terrain delivery the `Exchange/export` leg streams.
- Auto: `GeoRaster.Read` runs the `GeoGdal.Bootstrap` `GdalBase.ConfigureAll()` once, opens the bytes through `Gdal.FileFromMemBuffer` + `Gdal.Open(path, Access.GA_ReadOnly)`, reads the `Dataset.GetGeoTransform` 6-coefficient affine and the `Dataset.GetExtent(env, srs)` NTS `Envelope` directly in the target CRS, and reads the windowed pixels through `Dataset.ReadRaster<T>` into a managed `T[]` whose element type matches the `Band.DataType` (`GDT_Float32`→`float[]` for a DEM, `GDT_Byte`→`byte[]` for an ortho), the `bufXSize`/`bufYSize` resample arguments downsampling on read; `GeoRaster.Contour` runs `Gdal.wrapper_GDALContourDestName` over the DEM band producing an OGR contour `Layer`, then bridges each contour `Feature.GetGeometryRef.ExportToWkb` through `WKBReader.Read` into the `GeoFeature` `LineString` rows tagged `"contour"` the `GeoClassifier` maps onto `IfcGeographicElement`; the raster placement composes the `georeference#GEODETIC_TRANSFORM` `ProjNET` leg for the managed reprojection of the extent and escalates to `MaxRev.Gdal.Core` OSR (a one-shot `Gdal.Warp` with `-t_srs` or a `SpatialReference`/`CoordinateTransformation` over the same ordinates, always `SetAxisMappingStrategy(OAMS_TRADITIONAL_GIS_ORDER)` against the GDAL-3 axis swap) only for the exotic datum-grid transforms `ProjNET` cannot express.
- Receipt: the `RasterTile` is the placed pixel evidence (the typed band buffer, the geo-transform, and the NTS extent) a terrain-mesh tessellation or an ortho draping reads; the contour `GeoFeature` lines are the vectorized terrain the site model indexes; the COG transcode is the web-delivery pyramid the `Exchange/export` 3D-Tiles terrain leg streams — each carrying the source `DataType` and resolution as receipt facts.
- Packages: `MaxRev.Gdal.Core`, `MaxRev.Gdal.MacosRuntime.Minimal.arm64`, `NetTopologySuite`, `ProjNET`, `LanguageExt.Core`
- Growth: a new raster format is enumerable through the one `Gdal.Open` universal driver path with zero new code (GDAL covers GeoTIFF/COG/DEM/JPEG2000/…); a new DEM derivation is one `wrapper_GDALDEMProcessing` mode (`"hillshade"`/`"slope"`/`"aspect"`); a new resample kernel is one `RasterIOExtraArg`; never a per-format raster reader, never a hand-rolled GeoTIFF decoder, and never an OSR transform for what `ProjNET` already covers.
- Boundary: the raster ingest is `MaxRev.Gdal.Core`'s — the bindings DLL is managed AnyCPU but P/Invokes the native `libgdal`/`libgeos`/`libproj` shipped by the RID-keyed `MaxRev.Gdal.MacosRuntime.Minimal.arm64` runtime, so `GdalBase.ConfigureAll()` MUST run once before any `OSGeo.*` call and a publish without the matching RID runtime faults at first call (`BimFault.CapabilityMiss`); the `gdal-data`/PROJ resource set the runtime stages is required for CRS resolution even when the native libraries load; pixels move through `Dataset.ReadRaster<T>` into a managed `T[]` matching the `Band.DataType` and a hand-rolled raster decoder is the deleted form; reprojection inside a GDAL pipeline (`Gdal.Warp`, `Geometry.TransformTo`, `Dataset.GetExtent(env, srs)`) uses OSR while managed-geometry reprojection stays the `georeference#GEODETIC_TRANSFORM` `ProjNET` leg — OSR escalates only the exotic datum-grid/dynamic-datum transforms `ProjNET` cannot reach, and using OSR for a transform `ProjNET` covers is the rejected form; the `/vsimem/`/`/vsizip/`/`/vsicurl/` virtual filesystem opens a dataset from an in-memory buffer or a remote/zipped source without a filesystem path; the tile-pyramid partitioning and streaming stay at `Rasm.Compute/Runtime/codecs#TILE_PARTITION` consumed at the seam — `Rasm.Bim` authors the COG/contour and the 3D-Tiles terrain leg crosses the seam, never the pyramid.

```csharp signature
// --- [COMPOSITION] ------------------------------------------------------------------------
// The mandatory once-per-process GDAL bootstrap: ConfigureAll registers every GDAL+OGR driver and
// resolves the gdal-data/PROJ paths from the RID runtime package; UseExceptions flips the SWIG error
// model to thrown so a failed open lowers onto BimFault.CodecReject. A per-open configure is rejected.
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
// The band buffer types by the source Band.DataType per the api-maxrev-gdal [RASTER_IO] law — GDT_Byte→
// byte[] (ortho/basemap imagery), GDT_Float32/Float64→float[] (DEM elevation, slope/aspect), the integer
// widths→int[] (classification/index rasters) — so a multi-band read carries its true pixel type rather
// than a forced float; SampleAt reads any arm into the continuous float domain the DEM/contour leg consumes.
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

    // The window envelope maps to a pixel window through the inverse north-up geo-transform (gt[1] px width,
    // gt[5] negative px height); None reads the full raster. The window clamps to the raster bounds so an
    // overhanging clip never over-reads, and a differing target size triggers GDAL's on-read resampling.
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

    // The band stack types by the source Band.DataType and reads every band in one Dataset.ReadRaster<T>
    // call (the bandCount + interleaved bandMap), so a single-band DEM and a multi-band ortho both land
    // their true pixel type — GDT_Byte→byte[], the integer widths→int[], GDT_Float*→float[].
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

- [NTS_PLANAR_ALGEBRA]: the `NetTopologySuite` 2.6.0 member spellings the `GEOSPATIAL_SEAM` composes are decompile-verified — `NtsGeometryServices` is the global factory/precision/SRID cache (`Instance` the canonical singleton, the `(CoordinateSequenceFactory, PrecisionModel, int srid, GeometryOverlay, CoordinateEqualityComparer)` ctor), `GeometryOverlay.NG` the robust `OverlayNG` boolean engine, `PackedCoordinateSequenceFactory.DoubleFactory` the dense struct-of-arrays layout, `STRtree<T>.Insert(Envelope, item)`/`Query(Envelope)` the bulk-loaded broad-phase, `PreparedGeometryFactory.Prepare(Geometry)` + `IPreparedGeometry.Intersects` the narrow-phase acceleration, `GeometryFixer.Fix(Geometry)` the validity repair, `OverlayNGRobust.Union(IEnumerable<Geometry>)` the noded dissolve, and `WKBReader.Read`/`WKBWriter.Write` the OGR bridge — confirmed against the `.api/api-nettopologysuite` catalog (the topology engine is pure managed C#, no native GEOS, distinct from the GDAL native boundary); the canonical broad-then-narrow spatial-join rail stacks `STRtree.Query` then `PreparedGeometry.Intersects`, the same two-tier pattern the `Model/systems#INTERFERENCE` 3D BVH runs projected to 2D.
- [GDAL_UNIVERSAL_INGEST]: the `MaxRev.Gdal.Core` 3.13.1 member spellings the `VECTOR_INGEST` universal arm and the `RASTER_INGEST` owner compose are decompile-verified at `.api/api-maxrev-gdal` — `GdalBase.ConfigureAll()`/`IsConfigured` the mandatory once-per-process bootstrap, `Gdal.UseExceptions`/`Ogr.UseExceptions`/`Osr.UseExceptions` the error-model flip, `Gdal.FileFromMemBuffer`/`Unlink` the `/vsimem/` virtual filesystem, `Ogr.Open`/`DataSource.GetLayerByIndex`/`Layer.SetSpatialFilterRect`/`SetAttributeFilter`/`GetNextFeature`/`Feature.GetGeometryRef`/`OSGeo.OGR.Geometry.ExportToWkb` the vector iteration, `Gdal.Open`/`Dataset.ReadRaster<T>`/`GetGeoTransform`/`GetExtent`/`GetSpatialRef`/`Band.DataType` the raster read, `Gdal.wrapper_GDALContourDestName`/`wrapper_GDALDEMProcessing`/`wrapper_GDALTranslate`/`Warp` the algorithms, and `SpatialReference.ImportFromEPSG`/`SetFromUserInput`/`SetAxisMappingStrategy(OAMS_TRADITIONAL_GIS_ORDER)`/`CoordinateTransformation` the OSR escalation; the RID runtime pin `MaxRev.Gdal.MacosRuntime.Minimal.arm64` tracks the same `3.13.1.534` version and stages the `gdal-data`/PROJ resources `ConfigureAll` resolves, the hard platform constraint the boundary names.
- [CODEC_SEAM]: the shapefile codec is `NetTopologySuite.IO.Esri.Shapefile` 1.2.0 (`Shapefile.OpenRead`/`OpenWrite`/`WriteAllFeatures`, `ShapefileReaderOptions.MbrFilter`/`Factory`/`GeometryBuilderMode.FixInvalidShapes`, the `DbfField` typed schema) verified at `.api/api-nts-esri-shapefile` — the pure-managed default for shapefile I/O, its output the canonical NTS `Feature`; the CityJSON codec is `bertt.CityJSON` 2.5.0 (`CityJsonDocument`/`CityObject`/`CityObjectType`/`Transform.ScaleVector3`/`TranslateVector3`/`GetVerticesEnvelope`/`CityJsonWriter.Write`/`CityJsonSeqReader.ReadCityJsonSeq`, Newtonsoft-serialized, transitively floored on `NetTopologySuite.Features`) verified at `.api/api-cityjson` — the lossless transform-quantized 3D-city encoding; the FlatGeobuf codec is `FlatGeobuf` 3.26.0 (`FlatGeobuf.NTS.FeatureCollectionConversions.Serialize`/`Deserialize(stream, Envelope rect)` the bbox-push-down read/write over NTS `IFeature` directly, `GeometryConversions.ToGeometryType` the homogeneous-layer kind, the `FlatGeobufCoordinateSequence` packed NTS sequence, and `FlatGeobuf.Index.PackedRTree.StreamSearch(numItems, nodeSize, rect, ReadNode)` the remote byte-range query, pure-managed `lib/netstandard2.1` AnyCPU) verified at `.api/api-flatgeobuf` — the NTS-native cloud-optimized row-oriented codec, the managed peer of shapefile with no GDAL/OGR dependency; the GeoParquet codec is `GISBlox.IO.GeoParquet` 1.1.1 (`GeoParquetReader.ReadAll`/`ReadColumns`/`ReadGeoMetadata` the columnar projection + `geo`-metadata-first read, `GeoParquetWriter.Write(stream, DataTable, geoColumn, batchSize)` the columnar emit, the `GeoFileMetadata`/`GeoColumnMetadata`/`GeometryFormat.WKB` model, and the `DataTable.AddGeoColumn`/`SetAsPrimaryGeoColumn`/`AddGeoProcessingMetadata` schema-tagging extensions, the geo column WKB-bridging via the NTS `WKBReader.Read`/`WKBWriter.Write` over the admitted `ParquetSharp` 23.0.0.2 native engine) verified at `.api/api-gisblox-geoparquet` — the managed columnar arm exchanging a `DataTable` whose geo cell is the SAME WKB the OGR universal bridge crosses; all four managed codecs land the same `NetTopologySuite.Features.Feature`/`Geometry`/`AttributesTable` shape the GDAL OGR bridge produces (shapefile/CityJSON/FGB exchange `IFeature`, GeoParquet bridges a `DataTable` geo cell through WKB), so they are NTS-geometry source rows, not parallel geometry worlds, and FlatGeobuf + GeoParquet are admitted as `managed:true` `GeoVectorSource` arms NOT routed through `Ogr.Open`.
- [REPROJECTION_SEAM]: the geodetic reprojection is the `georeference#GEODETIC_TRANSFORM` `ProjNET` 2.1.0 leg by default (`CoordinateSystemServices.CreateTransformation`/`MathTransform.Transform`, the managed datum/projection owner), with `MaxRev.Gdal.Core` OSR the escalation counterpart for the exotic datum-grid/dynamic-datum transforms `ProjNET` cannot express (`SpatialReference`/`CoordinateTransformation` PROJ-backed, `IsDynamic`/`GetCoordinateEpoch` plate-motion); the seam reads identically from both sides per `.api/api-projnet` `[OSR_VS_PROJNET]` — `ProjNET` owns every transform that stays in the managed planar algebra and OSR escalates only what it cannot reach, with `Densifier.Densify` inserting vertices so a long edge tracks a non-linear datum transform; the `GeoFeature.SourceCrs` and the project CRS drive the SRID lookup, an unresolvable CRS leaving the geometry unreprojected rather than faulting so a single-datum site never blocks ingest.
- [SITE_CONTEXT_PROJECTION]: the `GeoFeature.ToElement` projection mints a `Model/elements#ELEMENT_MODEL` `BimElement` discriminated by the `GeoClassifier`-resolved `IfcClass` (the IFC4.3 geospatial entities `IfcSite`/`IfcBuilding`/`IfcBridge`/`IfcGeographicElement`/`IfcCourse`/`IfcRail` resolved by name through `IfcClass.TryGet` against the `Model/elements#ELEMENT_MODEL` vocabulary, defaulting to `IfcGeographicElement`/`Proxy`), threading the `IAttributesTable` onto the `Semantics/properties#PROPERTY_SETS` `Pset_SiteContext` `PropertyBinding` set and the deterministic GlobalId off `ParserIfc.HashGlobalID` so a re-ingest dedups against the prior pass through the `Review/diff#MODEL_DIFF` federation; a site-context model is a `BimModel` like any imported model so the `Model/query#ELEMENT_SET` algebra reads it with no second selection surface, and the CityJSON dequantized solid/surface geometry can tessellate into the `Exchange/export` glTF/3D-Tiles delivery sharing the projected coordinate frame this seam establishes.

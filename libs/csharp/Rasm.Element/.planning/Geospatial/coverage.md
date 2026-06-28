# [ELEMENT_COVERAGE]

The host-neutral raster/field coverage owner: one `CoverageGrid` the `Graph/element#NODE_MODEL` `Node.Coverage` case wraps, carrying the gridded data BY REFERENCE (a content key to the raster/field bytes, never inlined pixels), a per-band descriptor set, the affine `GridDescriptor`, and the `Geospatial/reference#GEO_REFERENCE` `GeoReference` CRS. A coverage is the continuous-field counterpart to the discrete `Object` graph: a digital elevation model, a solar-irradiance field, a noise-contour raster, a soil-stratum grid — gridded data a site-context or environmental consumer samples. The bytes are CONTENT-KEYED and held by the same object store the geometry blobs use (one `XxHash128` seed), so a coverage node carries the grid metadata and the band schema while the heavy raster lives in the blob store addressed by its `RasterKey` — the seam never inlines a pixel buffer. Vector features ride the `Object` node (a georeferenced object with a `GeoReference`-projected placement), so the seam carries NO parallel `Feature` family — the `NetTopologySuite` Simple-Features algebra, the GDAL/OGR raster+vector ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON codecs all live in `Rasm.Bim`, lowering a raster onto a content-keyed `CoverageGrid` and a vector feature onto an `Object` node. The page composes `Geospatial/reference#GEO_REFERENCE` for the CRS and rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` on a degenerate grid or a band-count mismatch.

## [01]-[INDEX]

- [01]-[COVERAGE_NODE]: the `CoverageGrid` by-ref raster/field descriptor, the `CoverageKind` discriminant, the `CoverageBand` per-band schema, the affine `GridDescriptor`, and the `Of` admission.

## [02]-[COVERAGE_NODE]

- Owner: `CoverageGrid` the host-neutral coverage descriptor the `Node.Coverage` case wraps; `CoverageKind` the `Raster`/`Field` discriminant; `CoverageBand` the per-band schema row (`Index` + `Name` + `DataType` + `NoData` + `Units`); `GridDescriptor` the affine grid (origin + cell size + dimensions); the `RasterKey` content key to the bytes by-reference.
- Entry: `CoverageGrid.Of(kind, rasterKey, grid, bands, crs, key)` admits a coverage — `Fin<T>` railing `ElementFault.ValueRejected` on a degenerate grid (non-positive width/height/cell-size) or an empty band set; `SampleAddress(col, row)` projects a grid cell onto its real-world coordinate through the `GridDescriptor` affine and the `GeoReference` so a consumer reads a cell's location without the raster bytes; `BandAt(index)` resolves a band descriptor.
- Auto: `Of` validates the `GridDescriptor` dimensions positive and the band set non-empty, the `RasterKey` being the content key the blob store resolves; `SampleAddress` applies the affine (`OriginX + col·CellSizeX`, `OriginY + row·CellSizeY`) so a cell's planar coordinate is derived from the grid metadata alone; the per-band `NoData` and `DataType` carry the sampling schema a consumer reads before fetching the bytes.
- Receipt: the `CoverageGrid` is the gridded-field evidence a site-context or environmental consumer reads — the grid metadata and band schema in the node, the heavy raster in the content-keyed blob store addressed by `RasterKey`; a `Bake`-derived `Element` whose object is a site context carries its `Coverage` nodes through the graph so a solar/noise/elevation field samples at the element's placement, the `Rasm.Compute` environmental route reading the bytes by `RasterKey`.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum]`), LanguageExt.Core (`Seq`/`Fin`/`Option`), `Rasm` (the kernel `Op` op-key + the content-key seed the `RasterKey` shares).
- Growth: a new band attribute is one `CoverageBand` column; a new coverage kind is one `CoverageKind` row (a `PointCloud`/`Tin` field grid); a new sampling projection is one method on `CoverageGrid`; never a per-raster-format coverage type and never inlined pixels.
- Boundary: `CoverageGrid` holds the bytes BY REFERENCE — the `RasterKey` content key addresses the raster/field in the same blob store the geometry uses (one `XxHash128` seed), and an inlined pixel buffer or a host raster type on the seam is the named defect; the band schema and the affine grid live in the node so a consumer reads the sampling schema before fetching bytes; vector features ride the `Object` node (a georeferenced object), so a parallel `Feature`/`GeoFeature` family on the seam is the deleted form — the `NetTopologySuite` Simple-Features algebra, the GDAL/OGR raster+universal-vector ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf codecs all live in `Rasm.Bim`, lowering a raster onto a content-keyed `CoverageGrid` and a vector feature onto an `Object` node; the `GeoReference` CRS rides the coverage (and the `Header`), DROPPED from the `Object` node, so a coverage carries its own georeference for a multi-CRS site context.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CoverageKind {
 public static readonly CoverageKind Raster = new("raster"); // a discretized cell grid (DEM, orthophoto, contour raster)
 public static readonly CoverageKind Field = new("field"); // a continuous sampled scalar/vector field (irradiance, noise, wind)
}

public readonly record struct CoverageBand(int Index, string Name, string DataType, double NoData, string Units);

public readonly record struct GridDescriptor(double OriginX, double OriginY, double CellSizeX, double CellSizeY, int Width, int Height) {
 public bool IsDegenerate => CellSizeX <= 0.0 || CellSizeY <= 0.0 || Width <= 0 || Height <= 0;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record CoverageGrid(
 CoverageKind Kind,
 UInt128 RasterKey,
 GridDescriptor Grid,
 Seq<CoverageBand> Bands,
 GeoReference Crs) {

 public static Fin<CoverageGrid> Of(CoverageKind kind, UInt128 rasterKey, GridDescriptor grid, Seq<CoverageBand> bands, GeoReference crs, Op key) =>
 grid.IsDegenerate
 ? ElementFault.ValueRejected(key, $"<coverage-grid-degenerate:{grid.Width}x{grid.Height}>")
 : bands.IsEmpty
 ? ElementFault.ValueRejected(key, "<coverage-bands-empty>")
 : Fin.Succ(new CoverageGrid(kind, rasterKey, grid, bands, crs));

 public Option<CoverageBand> BandAt(int index) => Bands.Find(b => b.Index == index);

 // The affine cell→planar projection: a consumer reads a cell's real-world coordinate from the
 // grid metadata alone, never the raster bytes.
 public (double X, double Y) SampleAddress(int col, int row) =>
 (Grid.OriginX + col * Grid.CellSizeX, Grid.OriginY + row * Grid.CellSizeY);
}
```

## [03]-[RESEARCH]

- [COVERAGE_BY_REFERENCE]: the coverage keys the gridded data by a content hash (the `RasterKey`) rather than inlining pixels — the raster/field bytes live in the same content-addressed object store the geometry blobs use (one `XxHash128` seed shared with `Projection/address#CONTENT_ADDRESS`), so a coverage node carries the grid metadata and band schema while the heavy raster is fetched by key; the neutral `RasterKey` name carries no IFC or GDAL format leak, the `Rasm.Bim` GDAL/OGR ingest writing the raster to the blob store and lowering its grid metadata onto this descriptor.
- [VECTOR_ON_OBJECT]: vector features ride the `Object` node — a georeferenced object whose placement projects through the model `GeoReference` — so the seam carries NO parallel `Feature` family; the `Rasm.Bim` `Semantics/geospatial` owner holds the `NetTopologySuite` Simple-Features planar algebra, the `STRtree` broad-phase, the GDAL/OGR universal vector+raster ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf/KML/MVT codecs, lowering a vector feature onto an `Object` node (with its `Classification` and properties) and a raster onto a content-keyed `CoverageGrid`; the seam thus unifies the discrete BIM graph and the continuous geospatial field under one node model without a second geometry stack.

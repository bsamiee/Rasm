# [ELEMENT_COVERAGE]

The host-neutral raster/field coverage owner: one `CoverageGrid` the `Graph/element#NODE_MODEL` `Node.Coverage` case wraps, holding the gridded data BY CONTENT KEY (a `RasterKey` into the same seed-zero `XxHash128` object store the geometry blobs use — never an inlined pixel buffer), the full six-coefficient affine `GridDescriptor` (origin, per-axis cell size, AND the two rotation terms — the GDAL geotransform in full, so a rotated or skewed grid is exact rather than truncated to the north-up special case), a typed `CoverageBand` schema per band, and the `Geospatial/reference#GEO_REFERENCE` `GeoReference` CRS. The grid descriptor is INVERTIBLE: `Project` maps a fractional cell onto its planar world coordinate and `Locate` inverts a world coordinate back to a fractional cell, so a site-context or environmental consumer samples a field at an element's placement from the grid metadata alone — `Locate` for the continuous-field interpolation cell, `CellAt` for the containing in-bounds discrete cell — never the raster bytes.

A band is typed, not stringly. `RasterSampleType` is the full pixel-storage vocabulary (`Byte`/`Int16`/`Float32`/`Float64`/the complex pair), each row carrying its `Bytes` width and a `Complex` flag so a consumer sizes a blob fetch and reads a complex pair without touching a host enum; `BandRole` is the display channel (`Gray`/`Red`/`Green`/`Blue`/`Alpha`/`Palette`) so a multi-band orthophoto is self-describing; `NoData` is an `Option<double>` (a band may carry no sentinel at all, mirroring the optional GDAL nodata flag); `Units` and the `Offset`/`Scale` linear decode (`Real(raw) = Offset + Scale·raw`) let a scaled-integer DEM read in real units. The seam INTERNALIZES the raster pixel/channel/scaling vocabulary so a downstream coverage consumer never re-learns the GDAL `DataType`/`ColorInterp`/`GetOffset` surface.

A coverage is the continuous-field counterpart to the discrete `Object` graph — a digital elevation model, a solar-irradiance field, a noise-contour raster, a soil-stratum grid. Vector features ride the `Object` node (a georeferenced object with a `Classification` and properties), so the seam carries NO parallel `Feature` family: the `NetTopologySuite` Simple-Features algebra, the `STRtree` broad phase, the GDAL/OGR raster+vector ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf codecs all live in `Rasm.Bim`, which writes the raster bytes to the content-keyed blob store and lowers only the grid metadata + band schema onto this `CoverageGrid`, lowering a vector feature onto an `Object` node — the seam unifying the discrete BIM graph and the continuous geospatial field under one node model without a second geometry stack.

`CoverageGrid.CanonicalBytes` is the coverage's content identity — the `Graph/element#NODE_MODEL` `Node.Coverage` arm delegates to it, so a non-rooted coverage node's `NodeId` derives from its kind, the full affine, dimensions, `RasterKey`, the CRS (`Geospatial/reference#GEO_REFERENCE` `GeoReference.CanonicalBytes`), and the index-ordered per-band schema — IEEE-754-canonical and band-order-stable across the C#/Python/TypeScript runtimes that share the one `XxHash128` seed (`Projection/address#CONTENT_ADDRESS`). `Of` rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` on a non-invertible (zero-determinant) or zero-sized grid, an empty band set, or duplicate band indices.

## [01]-[INDEX]

- [01]-[COVERAGE_NODE]: the `CoverageGrid` by-reference raster/field descriptor; the `CoverageKind` `Raster`/`Field` discriminant carrying the `Interpolates` sampling policy; the `RasterSampleType` pixel-storage vocabulary with its `Bytes` width; the `BandRole` display channel; the `CoverageBand` per-band schema (typed sample type, role, optional `NoData`, units, `Offset`/`Scale` decode); the six-coefficient invertible affine `GridDescriptor` (`Project`/`Locate`/`Determinant`/`Contains`); and the `Of` admission, `CellAt` discrete sample, `ByteLength` fetch sizing, and `CanonicalBytes` content projection.

## [02]-[COVERAGE_NODE]

- Owner: `CoverageGrid` the host-neutral coverage descriptor the `Graph/element#NODE_MODEL` `Node.Coverage` case wraps; `CoverageKind` the `Raster`/`Field` `[SmartEnum<string>]` carrying the `Interpolates` sampling-policy column; `RasterSampleType` the `[SmartEnum<string>]` pixel-storage vocabulary (the GDAL `DataType` set) with its `Bytes` width and `Complex` flag; `BandRole` the `[SmartEnum<string>]` display channel (the GDAL `ColorInterp` set, reduced to the roles a coverage consumer reads); `CoverageBand` the per-band schema row; `GridDescriptor` the six-coefficient affine geotransform; the `RasterKey` content key to the bytes by-reference; the `GeoReference` CRS the coverage carries.
- Entry: `CoverageGrid.Of(kind, rasterKey, grid, bands, crs, key)` admits a coverage on the `Fin<T>` rail — `ElementFault.ValueRejected` on a degenerate (zero-determinant or zero-sized) grid, an empty band set, or duplicate band indices; `Grid.Project(col, row)` maps a fractional cell onto its planar world coordinate through the full affine (rotation-aware), `Grid.Locate(x, y)` inverts a world coordinate back to a fractional cell (the continuous-field sample), `CellAt(x, y)` floors-and-bounds-checks that inverse into the containing in-bounds discrete cell (`None` outside the coverage), `BandAt(index)` resolves a band descriptor, `ByteLength` sums the uncompressed raster size across bands, and `CanonicalBytes(writer)` projects the coverage's content into the shared canonical bytes.
- Auto: `Of` validates the affine invertible (`Determinant != 0`) and the dimensions positive, the band set non-empty, and the band indices distinct, the `RasterKey` being the content key the blob store resolves; `Grid.Project`/`Grid.Locate` are the forward/inverse of one `[[CellSizeX, RowRotation],[ColumnRotation, CellSizeY]]` affine over the `(OriginX, OriginY)` translation, so the inverse is exact for any admitted grid and a consumer reads a cell's planar coordinate — or a coordinate's cell — from the metadata alone; the per-band `RasterSampleType.Bytes` sizes a fetch, `CoverageBand.Real(raw)` applies the `Offset`/`Scale` decode, and `CoverageBand.IsNoData(raw)` tests the optional sentinel (NaN-safe via `double.Equals`); `CanonicalBytes` writes the kind, the six affine coefficients and dimensions, the `RasterKey`, the CRS (delegating to `GeoReference.CanonicalBytes`), and each band in `Index` order, every `double` through the `Projection/address#CONTENT_ADDRESS` IEEE-754 canon so identity is byte-stable cross-runtime.
- Receipt: the `CoverageGrid` is the gridded-field evidence a site-context or environmental consumer reads — the kind, the invertible affine, and the typed band schema in the node, the heavy raster in the content-keyed blob store addressed by `RasterKey`; a `Graph/element#ELEMENT_GRAPH` `Bake`-derived `Element` whose object is a site context carries its `Coverage` nodes flat in `element.Coverages`, so a `Rasm.Compute` environmental route resolves an element's placement to a cell (`Locate`/`CellAt`), sizes the fetch (`ByteLength`), reads the bytes by `RasterKey`, and decodes them through `CoverageBand.Real` — the seam delivering the full sampling schema, the consumer never re-deriving the geotransform or re-learning the GDAL band surface.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + the generated `Key`/`TryGet` resolvers the `RasterSampleType.Parse` rail and the `BandRole` projector lookup use), LanguageExt.Core (`Seq`/`Fin`/`Option` + the `Expected`→`Fin` lift the bare `ElementFault` case rides), `Projection/address#CONTENT_ADDRESS` (`CanonicalWriter` the `CanonicalBytes` projection writes through), `Geospatial/reference#GEO_REFERENCE` (`GeoReference` + its `CanonicalBytes`), `Rasm` (the kernel `Op` op-key + the `Domain.ContentHash` seed-zero content-key entry the `RasterKey` shares with `Projection/address#CONTENT_ADDRESS`).
- Growth: a new pixel storage type is one `RasterSampleType` row carrying its `Bytes`/`Complex`; a new display channel is one `BandRole` row; a new band attribute is one `CoverageBand` column written into `CanonicalBytes`; a new affine parameter is impossible (six coefficients are the complete 2D geotransform); a new sampling policy is one `CoverageKind` column; never a per-raster-format coverage type, never a stringly `DataType`, never an inlined pixel buffer, and never an irregular point-cloud/TIN coverage on this grid owner — an irregular geometry rides an `Object` node by content hash, not this regular-grid descriptor.
- Boundary: `CoverageGrid` holds the bytes BY REFERENCE — the `RasterKey` content key addresses the raster/field in the same seed-zero `XxHash128` blob store the geometry uses, and an inlined pixel buffer, a host raster handle, or a second hasher on the seam is the named defect; the `GridDescriptor` is the FULL six-coefficient affine (the two rotation terms are first-class, `Determinant != 0` is the invertibility law `Of` enforces), so an axis-aligned-only descriptor, a `CellSizeY > 0` north-up assumption, or a forward-only map with no inverse is the deleted form; a band is typed (`RasterSampleType`/`BandRole`/`Option<double>` nodata/`Offset`/`Scale`), so a `string` data type, a sentinel-double nodata, or a raw-undecoded band is the deleted form; vector features ride the `Object` node, so a parallel `Feature`/`GeoFeature` family on the seam is the deleted form — the `NetTopologySuite` algebra, the `STRtree` index, the GDAL/OGR raster+universal-vector ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf codecs all live in `Rasm.Bim`; the `GeoReference` CRS rides the coverage (and the `Header`), DROPPED from the `Object` node, so a coverage carries its own georeference for a multi-CRS site context; `CanonicalBytes` is the coverage's only content projection (id-free, IEEE-754-canonical, band-order-stable), and a per-coverage ad-hoc serialization is the named defect.

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
 public static readonly CoverageKind Raster = new("raster", interpolates: false); // discrete cells; a sample reads the containing cell (orthophoto, land-cover classification, contour raster)
 public static readonly CoverageKind Field = new("field", interpolates: true);   // a continuous phenomenon; a sample interpolates between cells (elevation surface, irradiance, noise, wind)

 // The sampling-policy column a Rasm.Compute environmental route reads to pick the resampling: a continuous Field
 // interpolates between cells (use the fractional Locate), a discrete Raster takes the containing cell (use CellAt) —
 // read as a column, never re-branched per consumer.
 public bool Interpolates { get; }
 private CoverageKind(string key, bool interpolates) : this(key) => Interpolates = interpolates;
}

// The complete pixel-storage vocabulary (the GDAL DataType set), internalized so a coverage consumer never touches
// the host enum: each row carries its per-pixel Bytes width (a consumer sizes a blob fetch) and a Complex flag (a
// real scalar versus a complex pair, e.g. SAR/InSAR). GDT_Unknown is the deleted form — a coverage declares its type.
[SmartEnum<string>]
public sealed partial class RasterSampleType {
 public static readonly RasterSampleType Byte = new("byte", bytes: 1, complex: false);
 public static readonly RasterSampleType Int8 = new("int8", bytes: 1, complex: false);
 public static readonly RasterSampleType UInt16 = new("uint16", bytes: 2, complex: false);
 public static readonly RasterSampleType Int16 = new("int16", bytes: 2, complex: false);
 public static readonly RasterSampleType UInt32 = new("uint32", bytes: 4, complex: false);
 public static readonly RasterSampleType Int32 = new("int32", bytes: 4, complex: false);
 public static readonly RasterSampleType UInt64 = new("uint64", bytes: 8, complex: false);
 public static readonly RasterSampleType Int64 = new("int64", bytes: 8, complex: false);
 public static readonly RasterSampleType Float16 = new("float16", bytes: 2, complex: false);
 public static readonly RasterSampleType Float32 = new("float32", bytes: 4, complex: false);
 public static readonly RasterSampleType Float64 = new("float64", bytes: 8, complex: false);
 public static readonly RasterSampleType CInt16 = new("cint16", bytes: 4, complex: true);
 public static readonly RasterSampleType CInt32 = new("cint32", bytes: 8, complex: true);
 public static readonly RasterSampleType CFloat16 = new("cfloat16", bytes: 4, complex: true);
 public static readonly RasterSampleType CFloat32 = new("cfloat32", bytes: 8, complex: true);
 public static readonly RasterSampleType CFloat64 = new("cfloat64", bytes: 16, complex: true);

 public int Bytes { get; }
 public bool Complex { get; }
 private RasterSampleType(string key, int bytes, bool complex) : this(key) => (Bytes, Complex) = (bytes, complex);

 // The Rasm.Bim GDAL projector maps Band.DataType onto a token and admits it here — an unknown pixel type rails
 // ElementFault.ValueRejected (bytes are uninterpretable without it), the Discipline.Parse-consistent admission.
 public static Fin<RasterSampleType> Parse(string token, Op key) =>
  TryGet(token, out RasterSampleType? type) && type is { } row ? Fin.Succ(row) : ElementFault.ValueRejected(key, $"<raster-sample-type-unknown:{token}>");
}

// The display channel a band carries (the GDAL ColorInterp set, reduced to the roles a coverage consumer reads):
// a single-band DEM/field is Gray or Undefined, a multi-band orthophoto is Red/Green/Blue/Alpha, an indexed land-cover
// raster is Palette. The Rasm.Bim projector maps the full GDAL ColorInterp enum (CMYK/HSL/YCbCr) onto these via the
// generated TryGet, defaulting Undefined; an exotic channel is one row, never a parallel band-role family.
[SmartEnum<string>]
public sealed partial class BandRole {
 public static readonly BandRole Undefined = new("undefined");
 public static readonly BandRole Gray = new("gray");
 public static readonly BandRole Palette = new("palette");
 public static readonly BandRole Red = new("red");
 public static readonly BandRole Green = new("green");
 public static readonly BandRole Blue = new("blue");
 public static readonly BandRole Alpha = new("alpha");
}

// The six-coefficient affine geotransform mirroring GDAL Dataset.GetGeoTransform [originX, pxW, rowRot, originY, colRot,
// pxH] (field order matches the array so the Rasm.Bim projector maps it positionally): the two rotation terms are
// first-class (a rotated/skewed grid is exact), and pxH (CellSizeY) is signed — a north-up raster carries a NEGATIVE
// CellSizeY, so degeneracy is the non-invertible (zero-determinant) test, NOT a sign check on a cell size.
//   Xworld = OriginX + col·CellSizeX + row·RowRotation
//   Yworld = OriginY + col·ColumnRotation + row·CellSizeY
public readonly record struct GridDescriptor(
 double OriginX, double CellSizeX, double RowRotation,
 double OriginY, double ColumnRotation, double CellSizeY,
 int Width, int Height) {

 public double Determinant => CellSizeX * CellSizeY - RowRotation * ColumnRotation;
 public bool IsDegenerate => Width <= 0 || Height <= 0 || Determinant == 0.0;
 public bool Contains(int col, int row) => col >= 0 && col < Width && row >= 0 && row < Height;

 // Forward affine: a fractional cell (col+0.5, row+0.5 for cell centre; col, row for the cell's top-left corner)
 // onto its planar world coordinate, rotation included.
 public (double X, double Y) Project(double col, double row) =>
  (OriginX + (col * CellSizeX) + (row * RowRotation), OriginY + (col * ColumnRotation) + (row * CellSizeY));

 // Inverse affine (the GDALInvGeoTransform analogue): a planar world coordinate back to a fractional cell. Exact for
 // any admitted grid — Of guarantees Determinant != 0, so the consumer samples a field at an element's placement.
 public (double Col, double Row) Locate(double x, double y) {
  double det = Determinant;
  double dx = x - OriginX, dy = y - OriginY;
  return (((CellSizeY * dx) - (RowRotation * dy)) / det, ((CellSizeX * dy) - (ColumnRotation * dx)) / det);
 }
}

public readonly record struct CoverageBand(
 int Index,
 string Name,
 RasterSampleType SampleType,
 BandRole Role,
 Option<double> NoData = default,
 string Units = "",
 double Offset = 0.0,
 double Scale = 1.0) {

 // The linear decode a scaled-integer band carries (mirroring GDAL Band.GetOffset/GetScale): the real-world value is
 // Offset + Scale·raw, so a UInt16 DEM stored at scale 0.01 reads in metres without the consumer hand-applying it.
 public double Real(double raw) => Offset + (Scale * raw);

 // The optional nodata sentinel test (NaN-safe via double.Equals — a NaN sentinel matches a NaN cell, which == would
 // miss); a band with no sentinel (None, the optional GDAL nodata flag clear) never reports a value as absent.
 public bool IsNoData(double raw) => NoData.Match(Some: raw.Equals, None: static () => false);
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
   ? ElementFault.ValueRejected(key, $"<coverage-grid-degenerate:{grid.Width}x{grid.Height}/det:{grid.Determinant}>")
   : bands.IsEmpty
    ? ElementFault.ValueRejected(key, "<coverage-bands-empty>")
    : bands.Map(static b => b.Index).Distinct().Count() != bands.Count
     ? ElementFault.ValueRejected(key, "<coverage-band-index-duplicate>")
     : Fin.Succ(new CoverageGrid(kind, rasterKey, grid, bands, crs));

 public Option<CoverageBand> BandAt(int index) => Bands.Find(b => b.Index == index);

 // The CONTAINING in-bounds discrete cell for a planar world coordinate (the Raster sample): Locate returns a
 // CORNER-based fractional cell (an integer is a cell's top-left corner, per Project), so cell (c,r) spans the
 // fractional half-open box [c,c+1)x[r,r+1) and the containing cell is FLOOR(Locate), NOT Round — a point in a
 // cell's second half rounds UP to the next cell and a negative fractional rounds toward zero into the grid, both
 // mis-assigning the cell (the GDAL pixel-containment idiom is floor of the inverse geotransform). None when the
 // coordinate falls outside the coverage. A continuous Field consumer reads the fractional Grid.Locate instead
 // (Kind.Interpolates selects which), never re-deriving the inverse here.
 public Option<(int Col, int Row)> CellAt(double x, double y) {
  (double col, double row) = Grid.Locate(x, y);
  int c = (int)Math.Floor(col), r = (int)Math.Floor(row);
  return Grid.Contains(c, r) ? Some((c, r)) : None;
 }

 // The total uncompressed raster byte size summed across bands (each Width·Height·SampleType.Bytes) — a consumer sizes
 // a blob fetch from the metadata alone, no host raster opened.
 public long ByteLength => Bands.Fold(0L, (acc, b) => acc + ((long)Grid.Width * Grid.Height * b.SampleType.Bytes));

 // The coverage's content projection the Graph/element#NODE_MODEL Node.Coverage arm delegates to: kind, the six affine
 // coefficients and dimensions, the RasterKey, the CRS (Geospatial/reference#GEO_REFERENCE GeoReference.CanonicalBytes),
 // and each band in Index order — every double through the shared CanonicalWriter IEEE-754 canon so the coverage's
 // content identity (its content-hashed NodeId and its Projection/address#CONTENT_ADDRESS diff/dedup key) is
 // byte-stable across the runtimes sharing the one XxHash128 seed.
 public void CanonicalBytes(CanonicalWriter w) {
  w.String(Kind.Key);
  w.Double(Grid.OriginX).Double(Grid.CellSizeX).Double(Grid.RowRotation)
   .Double(Grid.OriginY).Double(Grid.ColumnRotation).Double(Grid.CellSizeY)
   .Ordinal(Grid.Width).Ordinal(Grid.Height)
   .U128(RasterKey);
  Crs.CanonicalBytes(w);
  w.Ordinal(Bands.Count);
  foreach (CoverageBand b in Bands.OrderBy(static x => x.Index)) {
   w.Ordinal(b.Index).String(b.Name).String(b.SampleType.Key).String(b.Role.Key).Bool(b.NoData.IsSome);
   b.NoData.IfSome(nd => w.Double(nd));
   w.String(b.Units).Double(b.Offset).Double(b.Scale);
  }
 }
}
```

## [03]-[RESEARCH]

- [INVERTIBLE_GEOTRANSFORM]: the `GridDescriptor` is the FULL six-coefficient GDAL geotransform (`[OriginX, CellSizeX, RowRotation, OriginY, ColumnRotation, CellSizeY]`), not the north-up axis-aligned special case — the two rotation terms are first-class, so a rotated DEM or an obliquely-gridded field is exact rather than silently re-projected; `Project` is the forward cell→world affine and `Locate` its exact inverse (the `GDALInvGeoTransform` analogue, `Determinant != 0` guaranteed at admission), and `CellAt` floors-and-bounds-checks the inverse into the containing discrete cell (`Locate`'s integer is a cell corner, so the containing cell is the floor, not the round) — so the receipt's "sample a field at the element's placement" is a real metadata-only projection (forward AND inverse), not a forward-only promise the body cannot keep; degeneracy is the non-invertible (`Determinant == 0`) or zero-sized test, because a standard north-up raster carries a NEGATIVE `CellSizeY` (`pxH`) that a sign check would wrongly reject.
- [TYPED_BAND_VOCABULARY]: the seam internalizes the GDAL raster vocabulary so a downstream consumer never re-learns the host enums — `RasterSampleType` is the complete `DataType` pixel set (`Byte`…`Float64` + the complex pair) with each row's `Bytes` width (fetch sizing) and `Complex` flag, `BandRole` the `ColorInterp` display channel reduced to the roles a coverage reads (the projector maps the full CMYK/HSL/YCbCr set onto these or `Undefined`), `NoData` an `Option<double>` (the optional GDAL nodata flag, NaN-safe via `double.Equals` in `IsNoData`), and `Offset`/`Scale` the linear decode (`Real(raw) = Offset + Scale·raw`, the `Band.GetOffset`/`GetScale` analogue) so a scaled-integer DEM reads in real units — a `string` data type, a sentinel-double nodata, and a raw-undecoded band are the deleted naive forms.
- [BY_REFERENCE_CONTENT_KEY]: the coverage keys the gridded data by a content hash (the `RasterKey`) rather than inlining pixels — the raster/field bytes live in the same seed-zero `XxHash128` object store the geometry blobs use (the one seed shared with `Projection/address#CONTENT_ADDRESS`), so the node carries the grid metadata and band schema while the heavy raster is fetched by key (sized by `ByteLength`), the neutral `RasterKey` name carrying no IFC or GDAL format leak; `CoverageGrid.CanonicalBytes` is the coverage's content identity (the `Node.Coverage` arm delegates to it), projecting kind + affine + dimensions + `RasterKey` + CRS (`GeoReference.CanonicalBytes`) + index-ordered bands through the shared `CanonicalWriter`, so two coverages with the same raster bytes but a different affine or CRS address as the distinct coverages they are, and a coverage hashes identically across the C#/Python/TypeScript runtimes.
- [VECTOR_ON_OBJECT]: vector features ride the `Object` node — a georeferenced object whose placement projects through the model `GeoReference` — so the seam carries NO parallel `Feature` family; the `Rasm.Bim` `Semantics/geospatial` owner holds the `NetTopologySuite` Simple-Features planar algebra, the `STRtree` broad-phase, the GDAL/OGR universal vector+raster ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf/KML/MVT codecs, writing the raster bytes to the content-keyed blob store and lowering only the grid metadata + band schema onto a `CoverageGrid` and a vector feature onto an `Object` node — the seam unifying the discrete BIM graph and the continuous geospatial field under one node model without a second geometry stack.

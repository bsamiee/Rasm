# [ELEMENT_COVERAGE]

The host-neutral raster/field coverage owner: one `CoverageGrid` the `Graph/element#NODE_MODEL` `Node.Coverage` case wraps, holding the gridded data BY CONTENT KEY (a `RasterKey` into the same seed-zero `XxHash128` object store the geometry blobs use — never an inlined pixel buffer), the full six-coefficient affine `GridDescriptor` (origin, per-axis cell size, AND the two rotation terms — the GDAL geotransform in full, so a rotated or skewed grid is exact rather than truncated to the north-up special case), an `OverviewLevel` pyramid (the coverage is MULTI-RESOLUTION — a working-resolution consumer picks a level by target cell size and fetches that level's bytes, never the full base raster), a typed `CoverageBand` schema per band, and the `Geospatial/reference#GEO_REFERENCE` `GeoReference` CRS. The grid descriptor is INVERTIBLE: `Project` maps a fractional cell onto its planar world coordinate and `Locate` inverts a world coordinate back to a fractional cell, so a site-context or environmental consumer reads ONE `Sample` whose discrete-cell-vs-continuous-fraction shape the `CoverageKind.Interpolates` policy column selects — a continuous `Field` yields the fractional `Locate` for interpolation, a discrete `Raster` the containing in-bounds cell — never re-branching the resampling per consumer and never touching the raster bytes for the geometry.

A band is typed, not stringly, and FULLY self-describing. `RasterSampleType` is the complete pixel-storage vocabulary (`Byte`/`Int16`/`Float32`/`Float64`/the complex pair), each row carrying its `Bytes` width and a `Complex` flag so a consumer sizes a blob fetch and reads a complex pair without touching a host enum; `BandRole` is the display channel (`Gray`/`Red`/`Green`/`Blue`/`Alpha`/`Palette`) so a multi-band orthophoto is self-describing; `NoData` is an `Option<double>` (a band may carry no sentinel at all, mirroring the optional GDAL nodata flag); `Units` and the `Offset`/`Scale` linear decode (`Real(raw) = Offset + Scale·raw`) let a scaled-integer DEM read in real units; `Range` is the optional `(Min, Max)` value envelope a consumer reads for display-normalization from the metadata alone (the optional GDAL min/max flag); and a `Palette` band carries its `ColorBin` index→colour-and-category legend, so an indexed land-cover or soil-stratum raster decodes a cell value to an RGBA colour and a category label WITHOUT a parallel sidecar — the `Palette` role is never a hollow channel with no table behind it. The seam INTERNALIZES the raster pixel/channel/scaling/legend vocabulary so a downstream coverage consumer never re-learns the GDAL `DataType`/`ColorInterp`/`GetOffset`/`GetMinimum`/`GetRasterColorTable`/`GetDefaultRAT` surface.

A coverage is the continuous-field counterpart to the discrete `Object` graph — a digital elevation model, a solar-irradiance field, a noise-contour raster, a soil-stratum grid. Vector features ride the `Object` node (a georeferenced object with a `Classification` and properties), so the seam carries NO parallel `Feature` family: the `NetTopologySuite` Simple-Features algebra, the `STRtree` broad phase, the GDAL/OGR raster+vector ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf codecs all live in `Rasm.Bim`, which writes the raster bytes (each pyramid level its own content-keyed blob) to the object store and lowers only the grid metadata + overview + band schema onto this `CoverageGrid`, lowering a vector feature onto an `Object` node — the seam unifying the discrete BIM graph and the continuous geospatial field under one node model without a second geometry stack.

`CoverageGrid.CanonicalBytes` is the coverage's content identity — the `Graph/element#NODE_MODEL` `Node.Coverage` arm delegates to it, so a non-rooted coverage node's `NodeId` derives from its kind, the full affine, dimensions, base `RasterKey`, the index-ordered overview levels, the CRS (`Geospatial/reference#GEO_REFERENCE` `GeoReference.CanonicalBytes`), and the index-ordered per-band schema (sample type, role, nodata, units, decode, range, palette) — IEEE-754-canonical and order-stable across the C#/Python/TypeScript runtimes that share the one `XxHash128` seed (`Projection/address#CONTENT_ADDRESS`). `Of` rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` on a non-invertible (zero-determinant) or zero-sized grid, an empty band set, duplicate band indices, an overview coarser-than-base or non-monotone level set, or a `Palette` band whose colour bins carry duplicate indices.

## [01]-[INDEX]

- [01]-[COVERAGE_NODE]: the `CoverageGrid` by-reference raster/field descriptor; the `CoverageKind` `Raster`/`Field` discriminant carrying the `Interpolates` sampling policy; the `RasterSampleType` pixel-storage vocabulary with its `Bytes` width; the `BandRole` display channel; the `ColorBin` palette legend entry and the `CoverageSample` discrete-or-fractional sample result; the six-coefficient invertible affine `GridDescriptor` (`Project`/`Locate`/`Determinant`/`Contains`); the `OverviewLevel` pyramid row with its `TileOf` block-window accessor; the `CoverageBand` per-band schema (typed sample type, role, optional `NoData`, units, `Offset`/`Scale` decode, optional `Range`, optional palette); and the `Of` admission, `Sample`/`CellAt`/`CellCenter` projections, `LevelFor` resolution selector, `TileAt` base-tile window, `ByteLength` fetch sizing, and `CanonicalBytes` content projection.

## [02]-[COVERAGE_NODE]

- Owner: `CoverageGrid` the host-neutral coverage descriptor the `Graph/element#NODE_MODEL` `Node.Coverage` case wraps; `CoverageKind` the `Raster`/`Field` `[SmartEnum<string>]` carrying the `Interpolates` sampling-policy column; `RasterSampleType` the `[SmartEnum<string>]` pixel-storage vocabulary (the GDAL `DataType` set) with its `Bytes` width and `Complex` flag; `BandRole` the `[SmartEnum<string>]` display channel (the GDAL `ColorInterp` set, reduced to the roles a coverage consumer reads); `ColorBin` the palette index→RGBA-and-category legend entry (the GDAL `ColorTable`/`RasterAttributeTable` lowered); `CoverageSample` the `[Union]` discrete-cell-or-fractional sample result the `Kind.Interpolates` column selects; `OverviewLevel` the pyramid-level row (dimensions + cell size + per-level `RasterKey` + tile `BlockX`/`BlockY`) carrying the `TileOf` block-window accessor; `CoverageBand` the per-band schema row; `GridDescriptor` the six-coefficient affine geotransform; the `RasterKey` content key to the bytes by-reference plus the base raster's `BaseBlockX`/`BaseBlockY` tile dimensions (symmetric with each overview's block, so the full-resolution base read aligns to tiles the same way an overview read does); the `GeoReference` CRS the coverage carries.
- Entry: `CoverageGrid.Of(kind, rasterKey, grid, bands, crs, key, overviews)` admits a coverage on the `Fin<T>` rail — `ElementFault.ValueRejected` on a degenerate (zero-determinant or zero-sized) grid, an empty band set, duplicate band indices, an overview level wider-than-base, an overview level set whose stored order is not strictly coarsening (a `Zip`-adjacent `CellSize` non-increase), or a `Palette` band carrying duplicate colour-bin indices; `Grid.Project(col, row)` maps a fractional cell onto its planar world coordinate through the full affine (rotation-aware), `Grid.Locate(x, y)` inverts a world coordinate back to a fractional cell, `Sample(x, y)` returns the `CoverageKind.Interpolates`-selected `CoverageSample` (the `Field` fractional `Locate` or the `Raster` containing cell), `CellAt(x, y)` floors-and-bounds-checks the inverse into the containing in-bounds discrete cell (`None` outside the coverage), `CellCenter(col, row)` projects a cell's centre world point, `LevelFor(targetCellSize)` resolves the coarsest pyramid level still finer than a target resolution, `OverviewLevel.TileOf(col, row)`/`CoverageGrid.TileAt(col, row)` resolve the GDAL block window a windowed fetch aligns to (the containing tile column/row plus its bounds-clipped cell-extent — an overview level and the base raster sharing the ONE tiling-arithmetic body), `BandAt(index)` resolves a band descriptor, `ByteLength(level)` sums the uncompressed raster size across bands at a chosen level, and `CanonicalBytes(writer)` projects the coverage's content into the shared canonical bytes.
- Auto: `Of` validates the affine invertible (`Determinant != 0`) and the dimensions positive, the band set non-empty, the band indices distinct, the `RasterKey` being the content key the blob store resolves, each `OverviewLevel` strictly coarser than the base AND the stored level set strictly monotone (a `Zip` over `(levels, levels.Tail)` rejecting any adjacent pair whose successor `CellSize` does not strictly exceed its predecessor — so a non-monotone or upsampled pyramid is unrepresentable, not merely each-coarser-than-base; a `Rasm.Bim` GDAL projector reads `Dataset.GetOverviewCount`/`GetOverview` index-ordered into the level set and content-keys each level's blob, the `GetOverview(i)` decreasing-resolution contract preserved as the seam invariant), and each `Palette`-role band's `ColorBin` indices distinct; `Grid.Project`/`Grid.Locate` are the forward/inverse of one `[[CellSizeX, RowRotation],[ColumnRotation, CellSizeY]]` affine over the `(OriginX, OriginY)` translation, so the inverse is exact for any admitted grid and a consumer reads a cell's planar coordinate — or a coordinate's cell — from the metadata alone; `Sample` reads the `Kind.Interpolates` policy column and yields `CoverageSample.Fraction` (the raw fractional cell for a `Field`) or `CoverageSample.Cell` (the floored in-bounds cell for a `Raster`, `None` outside), so a consumer never re-decides the resampling; `LevelFor` folds the overview set picking the coarsest level whose cell size still resolves a target, defaulting the base when no overview is fine enough; the per-band `RasterSampleType.Bytes` sizes a fetch, `CoverageBand.Real(raw)` applies the `Offset`/`Scale` decode, `CoverageBand.IsNoData(raw)` tests the optional sentinel (NaN-safe via `double.Equals`), and `CoverageBand.Decode(raw)` resolves a `Palette` band's raw index to its `ColorBin` legend entry; `TileOf`/`TileAt` floor a cell to its containing tile through the carried `BlockX`/`BlockY` (an untiled/strip level reads as one full-width row band, an out-of-bounds cell `None`), so a windowed fetch aligns to the GDAL block grid from the metadata alone; `CanonicalBytes` writes the kind, the six affine coefficients and dimensions, the base `RasterKey` and the base `BaseBlockX`/`BaseBlockY` tiling, the index-ordered overview levels (each folding its own block size), the CRS (delegating to `GeoReference.CanonicalBytes`), and each band in `Index` order with its full schema, every `double` through the `Projection/address#CONTENT_ADDRESS` IEEE-754 canon so identity is byte-stable cross-runtime and a re-tile forks the base identity the same way it forks an overview's.
- Receipt: the `CoverageGrid` is the gridded-field evidence a site-context or environmental consumer reads — the kind, the invertible affine, the resolution pyramid, and the typed band schema (decode + range + palette) in the node, the heavy raster in the content-keyed blob store addressed per-level by `RasterKey`; a `Graph/element#ELEMENT_GRAPH` `Bake`-derived `Element` whose object is a site context carries its `Coverage` nodes flat in `element.Coverages`, so a `Rasm.Compute` environmental route resolves an element's placement to a sample (`Sample`), picks a working level (`LevelFor`), sizes the fetch (`ByteLength(level)`), reads that level's bytes by `RasterKey`, decodes a scaled band through `CoverageBand.Real` or an indexed band through `CoverageBand.Decode`, and reads the value envelope from `CoverageBand.Range` — the seam delivering the full sampling schema, the consumer never re-deriving the geotransform, the pyramid selection, or the GDAL band/legend surface.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + the generated `Key`/`TryGet` resolvers the `RasterSampleType.Parse` rail and the `BandRole` projector lookup use; `[Union]` the `CoverageSample` discrete-or-fractional result), LanguageExt.Core (`Seq`/`Fin`/`Option` + the `Expected`→`Fin` lift the bare `ElementFault` case rides), Generator.Equals (`[Equatable]` the `CoverageGrid` structural equality + member diff the `Graph/element#ELEMENT_GRAPH` snapshot drills into, `[UnorderedEquality]` the index-keyed `Bands`/`Overviews`/`Palette` sets — order-independent equality matching the `Index`/resolution-keyed canonical sort), `Projection/address#CONTENT_ADDRESS` (`CanonicalWriter` the `CanonicalBytes` projection writes through), `Geospatial/reference#GEO_REFERENCE` (`GeoReference` + its `CanonicalBytes`), `Rasm` (the kernel `Op` op-key + the `Domain.ContentHash` seed-zero content-key entry the `RasterKey` shares with `Projection/address#CONTENT_ADDRESS`).
- Growth: a new pixel storage type is one `RasterSampleType` row carrying its `Bytes`/`Complex`; a new display channel is one `BandRole` row; a new band attribute (a colour-map, a category label, a statistic) is one `CoverageBand` column or one `ColorBin` column written into `CanonicalBytes`; a new resolution tier is one `OverviewLevel` row; a new sampling policy is one `CoverageKind` column the `Sample` dispatch reads; a new fetch-alignment query composes the one `TileOf` body (base and overview share it), never a per-level re-spelling of the `col/BlockX` arithmetic; a new affine parameter is impossible (six coefficients are the complete 2D geotransform); never a per-raster-format coverage type, never a stringly `DataType`, never an inlined pixel buffer, never a sidecar palette/legend table beside the band, never a carried tile size with no tiling accessor (the decorative-data thin slice), and never an irregular point-cloud/TIN coverage on this grid owner — an irregular geometry rides an `Object` node by content hash, not this regular-grid descriptor.
- Boundary: `CoverageGrid` holds the bytes BY REFERENCE — the per-level `RasterKey` content key addresses the raster/field in the same seed-zero `XxHash128` blob store the geometry uses, and an inlined pixel buffer, a host raster handle, or a second hasher on the seam is the named defect; the `GridDescriptor` is the FULL six-coefficient affine (the two rotation terms are first-class, `Determinant != 0` is the invertibility law `Of` enforces), so an axis-aligned-only descriptor, a `CellSizeY > 0` north-up assumption, or a forward-only map with no inverse is the deleted form; a coverage is MULTI-RESOLUTION (the `OverviewLevel` pyramid + `LevelFor` selection + the level-keyed `ByteLength`/`RasterKey`), so a single-resolution descriptor that strands a COG/DEM pyramid and forces a full-base fetch is the deleted form; sampling is ONE policy-driven `Sample` (the `Kind.Interpolates` column selecting `Fraction` vs `Cell`), so a consumer hand-branching `Locate`-vs-`CellAt` per call is the deleted form; a band is typed AND fully self-describing (`RasterSampleType`/`BandRole`/`Option<double>` nodata/`Offset`/`Scale`/`Option<(Min,Max)>` range/`Seq<ColorBin>` palette), so a `string` data type, a sentinel-double nodata, a raw-undecoded band, an envelope-less band a consumer must scan pixels to normalize, or a `Palette` role with no colour table behind it (the hollow channel) is the deleted form; vector features ride the `Object` node, so a parallel `Feature`/`GeoFeature` family on the seam is the deleted form — the `NetTopologySuite` algebra, the `STRtree` index, the GDAL/OGR raster+universal-vector ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf codecs all live in `Rasm.Bim`; the `GeoReference` CRS rides the coverage (and the `Header`), DROPPED from the `Object` node, so a coverage carries its own georeference for a multi-CRS site context; `CoverageGrid` is a record carrying `[Equatable]` so the `Rasm.Persistence` `StructuralMerge` drills a changed band/level/colour-bin to `Coverage.Grid.Bands[i].<column>` rather than replacing the whole coverage; `CanonicalBytes` is the coverage's only content projection (id-free, IEEE-754-canonical, band-and-level-order-stable), and a per-coverage ad-hoc serialization is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Generator.Equals;
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

 // The sampling-policy column the Sample dispatch reads to pick the resampling: a continuous Field yields the
 // fractional Locate (interpolate between cells), a discrete Raster the containing cell — read as a column on Sample,
 // never re-branched per consumer (POLICY_VALUES: behavior rides the vocabulary row, not a caller-side if).
 public bool Interpolates { get; }
 private CoverageKind(string key, bool interpolates) : this(key) => Interpolates = interpolates;
}

// The complete pixel-storage vocabulary (the GDAL DataType set, GDT_Byte=1 … GDT_CFloat64=11, GDT_UInt64=12,
// GDT_Int64=13, GDT_Int8=14, GDT_Float16=15, GDT_CFloat16=16), internalized so a coverage consumer never touches
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
// raster is Palette (and carries its CoverageBand.Palette legend). The Rasm.Bim projector maps the full GDAL
// ColorInterp enum (GCI_HueBand/GCI_CyanBand/GCI_YCbCr_*) onto these via the generated TryGet, defaulting Undefined;
// an exotic channel is one row, never a parallel band-role family.
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

// One palette legend entry — the GDAL ColorTable.GetColorEntry(int)->ColorEntry(c1..c4) RGBA quad PLUS the
// RasterAttributeTable category label (GetValueAsString over the GFU_Name column) — lowered onto the seam so an
// indexed Palette band decodes a raw cell value to a colour AND a category WITHOUT a parallel sidecar table. The
// RGBA channels are byte-domain (0-255, the ColorEntry short range a projector clamps), the Category the optional
// class name a land-cover/soil-stratum legend carries (empty when the source has no RAT). Index is the raw cell
// value the band stores; a Palette band's bins are index-distinct (Of enforces).
[Equatable]
public readonly partial record struct ColorBin(int Index, byte R, byte G, byte B, byte A, string Category = "") {
 public void CanonicalBytes(CanonicalWriter w) =>
  w.Ordinal(Index).Ordinal(R).Ordinal(G).Ordinal(B).Ordinal(A).String(Category);
}

// One pyramid level a multi-resolution coverage (a COG/tiled DEM) carries — the GDAL Dataset.GetOverview(i) level
// the Rasm.Bim projector reads (GetOverviewCount levels) and content-keys its own blob into the object store: the
// level dimensions (always strictly coarser than the base — Of enforces), the level cell size (the base CellSize
// scaled by the decimation factor, the value LevelFor compares a target resolution against), the per-level RasterKey
// the consumer fetches, and the GDAL Band.GetBlockSize tile dimensions a windowed fetch aligns to (0 = untiled/strip).
[Equatable]
public readonly partial record struct OverviewLevel(int Width, int Height, double CellSize, UInt128 RasterKey, int BlockX, int BlockY) {
 public bool IsCoarserThan(int baseWidth, int baseHeight) => Width > 0 && Height > 0 && Width <= baseWidth && Height <= baseHeight && (Width < baseWidth || Height < baseHeight);

 // The GDAL `GetBlockSize` tile window a windowed fetch ALIGNS to — the operation the carried BlockX/BlockY exist to
 // serve (a stored tile size with no tiling accessor is the decorative-data thin slice, the consumer otherwise
 // re-deriving col/BlockX by hand). Given a cell, resolve its containing tile column/row AND the tile's cell-extent
 // clipped to the level bounds (an edge tile is partial), so a Rasm.Compute windowed read fetches exactly the tile a
 // cell falls in. An untiled (strip) level — BlockX/BlockY 0 — is one full-width row band: TileCol 0, BlockX the level
 // Width, the row band BlockY high. None for an out-of-bounds cell (the level resolves no tile outside its grid).
 public Option<(int TileCol, int TileRow, int OriginCol, int OriginRow, int SpanCol, int SpanRow)> TileOf(int col, int row) {
  if (col < 0 || col >= Width || row < 0 || row >= Height) { return None; }
  int bx = BlockX > 0 ? BlockX : Width, by = BlockY > 0 ? BlockY : 1;
  int tileCol = col / bx, tileRow = row / by, originCol = tileCol * bx, originRow = tileRow * by;
  return Some((tileCol, tileRow, originCol, originRow, Math.Min(bx, Width - originCol), Math.Min(by, Height - originRow)));
 }

 public void CanonicalBytes(CanonicalWriter w) =>
  w.Ordinal(Width).Ordinal(Height).Double(CellSize).U128(RasterKey).Ordinal(BlockX).Ordinal(BlockY);
}

// The result of one Sample — the discrete-cell-or-continuous-fraction discriminant the CoverageKind.Interpolates
// policy column selects, so a consumer reads ONE shape rather than choosing Locate-vs-CellAt itself: a Field yields
// Fraction (the raw fractional cell for an interpolating read; Inside flags whether it lands within the grid bounds),
// a Raster yields Cell (the containing in-bounds integer cell, or Cell with Inside=false carrying the floored cell
// outside bounds). ONE polymorphic sample result, never a parallel SampleFractional/SampleDiscrete pair.
[Union]
public abstract partial record CoverageSample {
 public sealed record Fraction(double Col, double Row, bool Inside) : CoverageSample;
 public sealed record Cell(int Col, int Row, bool Inside) : CoverageSample;
}

// The six-coefficient affine geotransform mirroring GDAL Dataset.GetGeoTransform(double[]) [originX, pxW, rowRot,
// originY, colRot, pxH] (field order matches the array so the Rasm.Bim projector maps it positionally): the two
// rotation terms are first-class (a rotated/skewed grid is exact), and pxH (CellSizeY) is signed — a north-up raster
// carries a NEGATIVE CellSizeY, so degeneracy is the non-invertible (zero-determinant) test, NOT a sign check.
//   Xworld = OriginX + col·CellSizeX + row·RowRotation
//   Yworld = OriginY + col·ColumnRotation + row·CellSizeY
public readonly record struct GridDescriptor(
 double OriginX, double CellSizeX, double RowRotation,
 double OriginY, double ColumnRotation, double CellSizeY,
 int Width, int Height) {

 public double Determinant => CellSizeX * CellSizeY - RowRotation * ColumnRotation;
 public bool IsDegenerate => Width <= 0 || Height <= 0 || Determinant == 0.0;
 public bool Contains(int col, int row) => col >= 0 && col < Width && row >= 0 && row < Height;

 // The base-level ground resolution magnitude — the geometric mean of the two axis cell extents, rotation included
 // (sqrt of the absolute determinant: |det| is the cell's planar area for any admitted affine), so LevelFor compares
 // a target resolution against the base AND every coarser OverviewLevel.CellSize on one scalar.
 public double CellSize => Math.Sqrt(Math.Abs(Determinant));

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

[Equatable]
public readonly partial record struct CoverageBand(
 int Index,
 string Name,
 RasterSampleType SampleType,
 BandRole Role,
 Option<double> NoData = default,
 string Units = "",
 double Offset = 0.0,
 double Scale = 1.0,
 Option<(double Min, double Max)> Range = default,
 [property: UnorderedEquality] Seq<ColorBin> Palette = default) {

 // The linear decode a scaled-integer band carries (mirroring GDAL Band.GetOffset/GetScale): the real-world value is
 // Offset + Scale·raw, so a UInt16 DEM stored at scale 0.01 reads in metres without the consumer hand-applying it.
 public double Real(double raw) => Offset + (Scale * raw);

 // The optional nodata sentinel test (NaN-safe via double.Equals — a NaN sentinel matches a NaN cell, which == would
 // miss); a band with no sentinel (None, the optional GDAL nodata flag clear) never reports a value as absent.
 public bool IsNoData(double raw) => NoData.Match(Some: raw.Equals, None: static () => false);

 // Resolve a Palette band's raw cell value to its ColorBin legend entry (the GDAL ColorTable/RAT lookup): an indexed
 // land-cover/soil raster decodes a cell to its RGBA colour and category. None when the band carries no palette (a
 // non-Palette band) or the raw index is not in the legend — the consumer rails its own missing-legend handling
 // rather than defaulting a colour. The raw value floors to the integer index the legend keys on.
 public Option<ColorBin> Decode(double raw) => Palette.Find(b => b.Index == (int)Math.Floor(raw));
}

// --- [MODELS] -----------------------------------------------------------------------------
[Equatable]
public sealed partial record CoverageGrid(
 CoverageKind Kind,
 UInt128 RasterKey,
 GridDescriptor Grid,
 [property: UnorderedEquality] Seq<CoverageBand> Bands,
 GeoReference Crs,
 [property: UnorderedEquality] Seq<OverviewLevel> Overviews = default,
 int BaseBlockX = 0,
 int BaseBlockY = 0) {

 public static Fin<CoverageGrid> Of(
  CoverageKind kind, UInt128 rasterKey, GridDescriptor grid, Seq<CoverageBand> bands, GeoReference crs, Op key,
  Seq<OverviewLevel> overviews = default, int baseBlockX = 0, int baseBlockY = 0) =>
  grid.IsDegenerate
   ? ElementFault.ValueRejected(key, $"<coverage-grid-degenerate:{grid.Width}x{grid.Height}/det:{grid.Determinant}>")
   : bands.IsEmpty
    ? ElementFault.ValueRejected(key, "<coverage-bands-empty>")
    : bands.Map(static b => b.Index).Distinct().Count() != bands.Count
     ? ElementFault.ValueRejected(key, "<coverage-band-index-duplicate>")
     : overviews.Exists(o => !o.IsCoarserThan(grid.Width, grid.Height))
      ? ElementFault.ValueRejected(key, "<coverage-overview-not-coarser>")
      : overviews.Zip(overviews.Tail).Exists(static p => p.Item2.CellSize <= p.Item1.CellSize)
       ? ElementFault.ValueRejected(key, "<coverage-overview-non-monotone>")
       : bands.Exists(b => b.Role == BandRole.Palette && b.Palette.Map(static c => c.Index).Distinct().Count() != b.Palette.Count)
        ? ElementFault.ValueRejected(key, "<coverage-palette-index-duplicate>")
        : Fin.Succ(new CoverageGrid(kind, rasterKey, grid, bands, crs, overviews, baseBlockX, baseBlockY));

 public Option<CoverageBand> BandAt(int index) => Bands.Find(b => b.Index == index);

 // The base-raster tile window a windowed fetch ALIGNS to — the dual of OverviewLevel.TileOf for the FULL-resolution
 // base raster (whose tile size the COG carries on BaseBlockX/BaseBlockY, symmetric with each overview's BlockX/BlockY,
 // so the base is never the un-tiled level that strands a tiled-COG base read). One synthetic base OverviewLevel
 // (the base dimensions + base CellSize + base RasterKey + base block) reuses the ONE TileOf body, so the tile
 // arithmetic is owned once across base and overviews, never re-spelled per level.
 public Option<(int TileCol, int TileRow, int OriginCol, int OriginRow, int SpanCol, int SpanRow)> TileAt(int col, int row) =>
  new OverviewLevel(Grid.Width, Grid.Height, Grid.CellSize, RasterKey, BaseBlockX, BaseBlockY).TileOf(col, row);

 // The ONE policy-driven sample: read the CoverageKind.Interpolates column and yield the matching CoverageSample —
 // a continuous Field yields the raw fractional Locate (the interpolation cell, Inside flagging in-bounds), a discrete
 // Raster yields the FLOORED containing cell (Inside flagging in-bounds). A consumer reads one result shape and never
 // re-decides the resampling: a Field consumer reads Fraction.Col/Row, a Raster consumer Cell.Col/Row — the discriminant
 // is recoverable from the value, never a caller-side if on Kind. Floor (not Round) is the GDAL pixel-containment idiom:
 // Locate's integer is a cell's top-left corner (per Project), so cell (c,r) spans [c,c+1)×[r,r+1) and the containing
 // cell is FLOOR — a point in a cell's second half rounds UP to the wrong cell, a negative fractional rounds toward zero.
 public CoverageSample Sample(double x, double y) {
  (double col, double row) = Grid.Locate(x, y);
  (int c, int r, bool inside) = Discrete(col, row);
  return Kind.Interpolates
   ? new CoverageSample.Fraction(col, row, inside)
   : new CoverageSample.Cell(c, r, inside);
 }

 // The CONTAINING in-bounds discrete cell for a planar world coordinate (the explicit Raster read): FLOOR(Locate)
 // bounds-checked, None outside the coverage. The discrete projection of Sample a caller takes when it knows it wants
 // a cell regardless of Kind (a Field consumer still snapping to a cell for a nearest-neighbour read); Sample is the
 // Kind-driven entry, CellAt the explicit-discrete one — both compose Grid.Locate AND the ONE Discrete floor-and-bound
 // owner below, so the FLOOR(Locate)+Contains containment rule is spelled once (the deleted form is each re-flooring inline).
 public Option<(int Col, int Row)> CellAt(double x, double y) {
  (double col, double row) = Grid.Locate(x, y);
  return Discrete(col, row) is (int c, int r, true) ? Some((c, r)) : None;
 }

 // The ONE floor-and-bound containment rule both Sample and CellAt compose: a fractional cell floors to its containing
 // integer cell (the GDAL top-left-corner pixel-containment idiom) and the bounds check flags in-grid — the single owner
 // of the cell-containment law, so the FLOOR(col)/FLOOR(row)/Contains triple is never re-spelled per read.
 (int Col, int Row, bool Inside) Discrete(double col, double row) {
  int c = (int)Math.Floor(col), r = (int)Math.Floor(row);
  return (c, r, Grid.Contains(c, r));
 }

 // The planar world coordinate of a discrete cell's CENTRE (Project at col+0.5, row+0.5) — the dual of CellAt a
 // consumer reads to place a per-cell value (a sampled irradiance, a contour vertex) at the cell's georeferenced
 // centroid rather than its corner, rotation included.
 public (double X, double Y) CellCenter(int col, int row) => Grid.Project(col + 0.5, row + 0.5);

 // Resolve the coarsest pyramid level whose cell size still resolves a target ground resolution — the working-level
 // selection a Rasm.Compute environmental route makes before a fetch so it reads a decimated overview rather than the
 // full base raster (the GDAL overview-selection idiom). Folds the overview set keeping the coarsest level still
 // finer-or-equal than the target; the base (None) is the floor when no overview is fine enough (or none exist), so a
 // consumer always gets a level. A target finer than the base returns the base — the source resolves no finer.
 public Option<OverviewLevel> LevelFor(double targetCellSize) =>
  Overviews
   .Filter(o => o.CellSize <= targetCellSize)
   .Fold(Option<OverviewLevel>.None, (best, o) => best.Match(Some: b => o.CellSize > b.CellSize ? Some(o) : best, None: () => Some(o)));

 // The total uncompressed raster byte size summed across bands at a chosen level (each levelWidth·levelHeight·
 // SampleType.Bytes), the base when no level is given — a consumer sizes a blob fetch from the metadata alone, no host
 // raster opened. A complex band's SampleType.Bytes already counts both components, so the sum is exact for SAR/InSAR.
 public long ByteLength(Option<OverviewLevel> level = default) {
  (int w, int h) = level.Match(Some: o => (o.Width, o.Height), None: () => (Grid.Width, Grid.Height));
  return Bands.Fold(0L, (acc, b) => acc + ((long)w * h * b.SampleType.Bytes));
 }

 // The coverage's content projection the Graph/element#NODE_MODEL Node.Coverage arm delegates to: kind, the six affine
 // coefficients and dimensions, the base RasterKey, the index-ordered overview levels, the CRS (Geospatial/reference#
 // GEO_REFERENCE GeoReference.CanonicalBytes), and each band in Index order with its full schema (sample type, role,
 // nodata, units, decode, range, palette) — every double through the shared CanonicalWriter IEEE-754 canon so the
 // coverage's content identity (its content-hashed NodeId and its Projection/address#CONTENT_ADDRESS diff/dedup key) is
 // byte-stable across the runtimes sharing the one XxHash128 seed. Two coverages with the same base raster but a
 // different pyramid, CRS, palette, or affine address as the distinct coverages they are.
 public void CanonicalBytes(CanonicalWriter w) {
  w.String(Kind.Key);
  w.Double(Grid.OriginX).Double(Grid.CellSizeX).Double(Grid.RowRotation)
   .Double(Grid.OriginY).Double(Grid.ColumnRotation).Double(Grid.CellSizeY)
   .Ordinal(Grid.Width).Ordinal(Grid.Height)
   .U128(RasterKey).Ordinal(BaseBlockX).Ordinal(BaseBlockY);
  w.Ordinal(Overviews.Count);
  foreach (OverviewLevel o in Overviews.OrderByDescending(static x => x.Width).ThenByDescending(static x => x.Height)) { o.CanonicalBytes(w); }
  Crs.CanonicalBytes(w);
  w.Ordinal(Bands.Count);
  foreach (CoverageBand b in Bands.OrderBy(static x => x.Index)) {
   w.Ordinal(b.Index).String(b.Name).String(b.SampleType.Key).String(b.Role.Key).Bool(b.NoData.IsSome);
   b.NoData.IfSome(nd => w.Double(nd));
   w.String(b.Units).Double(b.Offset).Double(b.Scale).Bool(b.Range.IsSome);
   b.Range.IfSome(r => w.Double(r.Min).Double(r.Max));
   w.Ordinal(b.Palette.Count);
   foreach (ColorBin c in b.Palette.OrderBy(static x => x.Index)) { c.CanonicalBytes(w); }
  }
 }
}
```

## [03]-[RESEARCH]

- [INVERTIBLE_GEOTRANSFORM]: the `GridDescriptor` is the FULL six-coefficient GDAL geotransform (`[OriginX, CellSizeX, RowRotation, OriginY, ColumnRotation, CellSizeY]`, the `Dataset.GetGeoTransform(double[])` array), not the north-up axis-aligned special case — the two rotation terms are first-class, so a rotated DEM or an obliquely-gridded field is exact rather than silently re-projected; `Project` is the forward cell→world affine and `Locate` its exact inverse (the `GDALInvGeoTransform` analogue, `Determinant != 0` guaranteed at admission), `Sample` reads the `Kind.Interpolates` policy column to yield the fractional (`Field`) or floored-cell (`Raster`) `CoverageSample` — so the receipt's "sample a field at the element's placement" is a real metadata-only projection (forward AND inverse, policy-selected), not a forward-only promise the body cannot keep; `CellAt`/`CellCenter` are the explicit discrete-corner / cell-centre projections, `CellSize = sqrt(|Determinant|)` the single rotation-aware resolution scalar `LevelFor` compares against; degeneracy is the non-invertible (`Determinant == 0`) or zero-sized test, because a standard north-up raster carries a NEGATIVE `CellSizeY` (`pxH`) that a sign check would wrongly reject.
- [MULTI_RESOLUTION_PYRAMID]: a coverage is MULTI-RESOLUTION — the `OverviewLevel` set is the GDAL overview pyramid (`Dataset.GetOverviewCount`/`GetOverview(i)`, `Band.GetOverviewCount`/`GetOverview(i)`, built by `BuildOverviews`) the `Rasm.Bim` projector reads, each level content-keyed into the same blob store as its own `RasterKey` and carrying its dimensions, decimated `CellSize`, and `Band.GetBlockSize` tile extents; `LevelFor(targetCellSize)` resolves the coarsest level still finer than a working resolution (the GDAL overview-selection idiom), so a `Rasm.Compute` route downsamples a DEM by FETCHING a decimated level rather than reading the full base raster and resampling in-process, `ByteLength(level)` sizes that level's fetch from the metadata alone, and `OverviewLevel.TileOf`/`CoverageGrid.TileAt` align that fetch to the `Band.GetBlockSize` tile grid (the base carrying its own `BaseBlockX`/`BaseBlockY` symmetric with each overview's, so a tiled-COG base read aligns the same way) — the carried block size driving a real windowed-read accessor rather than sitting as decorative metadata; `Of` enforces TWO independent pyramid invariants — each level strictly coarser than the base (`OverviewLevel.IsCoarserThan`) AND the stored level set strictly monotone (the `Zip`-adjacent `CellSize` strict-increase, the `GetOverview(i)` decreasing-resolution contract) — so an upsampled level, a wider-than-base level, OR a non-monotone (out-of-order or duplicate-resolution) level set is unrepresentable, the each-coarser-than-base check alone being insufficient because it would admit a `[base/2, base/8, base/4]` shuffled set the monotone gate rejects; a single-resolution descriptor that strands the pyramid and forces a full-base fetch is the deleted naive form.
- [TYPED_BAND_VOCABULARY]: the seam internalizes the FULL GDAL raster vocabulary so a downstream consumer never re-learns the host enums — `RasterSampleType` is the complete `DataType` pixel set (`GDT_Byte`…`GDT_Float64` + `GDT_Int8`/`GDT_UInt64`/`GDT_Int64`/`GDT_Float16` + the `GDT_CInt16`…`GDT_CFloat64` complex pair) with each row's `Bytes` width (fetch sizing) and `Complex` flag, `BandRole` the `ColorInterp` display channel reduced to the roles a coverage reads (the projector maps the full `GCI_HueBand`/`GCI_CyanBand`/`GCI_YCbCr_*` set onto these or `Undefined`), `NoData` an `Option<double>` (the optional `Band.GetNoDataValue(out, out hasval)` flag, NaN-safe via `double.Equals` in `IsNoData`), `Offset`/`Scale` the linear decode (`Real(raw) = Offset + Scale·raw`, the `Band.GetOffset`/`GetScale` analogue) so a scaled-integer DEM reads in real units, `Units` the `Band.GetUnitType` string, `Range` the optional `(Min, Max)` envelope (`Band.GetMinimum`/`GetMaximum`/`ComputeRasterMinMax`) a consumer reads for display-normalization without scanning pixels, and `Palette` the `ColorBin` index→RGBA-and-category legend (`ColorTable.GetColorEntry(int)`→`ColorEntry(c1..c4)` plus the `RasterAttributeTable` category column) so an indexed `Palette` band is decodable via `Decode(raw)` — a `string` data type, a sentinel-double nodata, a raw-undecoded band, an envelope-less band, and a `Palette` role with no colour table behind it (the hollow channel) are the deleted naive forms.
- [BY_REFERENCE_CONTENT_KEY]: the coverage keys the gridded data by a content hash (the base `RasterKey` plus a per-`OverviewLevel` `RasterKey`) rather than inlining pixels — the raster/field bytes live in the same seed-zero `XxHash128` object store the geometry blobs use (the one seed shared with `Projection/address#CONTENT_ADDRESS`), so the node carries the grid metadata, the pyramid, and the band schema while the heavy raster of a chosen level is fetched by key (sized by `ByteLength(level)`), the neutral `RasterKey` name carrying no IFC or GDAL format leak; `CoverageGrid.CanonicalBytes` is the coverage's content identity (the `Node.Coverage` arm delegates to it), projecting kind + affine + dimensions + base `RasterKey` + index-ordered overview levels + CRS (`GeoReference.CanonicalBytes`) + index-ordered bands (with decode, range, and palette) through the shared `CanonicalWriter`, so two coverages with the same base raster bytes but a different pyramid, affine, CRS, or palette address as the distinct coverages they are, and a coverage hashes identically across the C#/Python/TypeScript runtimes; `CoverageGrid` carries `[Equatable]` (the `[UnorderedEquality]` index-keyed `Bands`/`Overviews`/`Palette` sets) so the `Rasm.Persistence` 3-way `StructuralMerge` drills a changed band column, overview level, or colour bin to `Coverage.Grid.Bands[i].<column>` rather than replacing the whole coverage.
- [VECTOR_ON_OBJECT]: vector features ride the `Object` node — a georeferenced object whose placement projects through the model `GeoReference` — so the seam carries NO parallel `Feature` family; the `Rasm.Bim` `Semantics/geospatial` owner holds the `NetTopologySuite` Simple-Features planar algebra, the `STRtree` broad-phase, the GDAL/OGR universal vector+raster ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf/KML/MVT codecs, writing each raster pyramid level to the content-keyed blob store and lowering only the grid metadata + overview + band schema onto a `CoverageGrid` and a vector feature onto an `Object` node — the seam unifying the discrete BIM graph and the continuous geospatial field under one node model without a second geometry stack.

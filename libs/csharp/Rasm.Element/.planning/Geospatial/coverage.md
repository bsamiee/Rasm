# [ELEMENT_COVERAGE]

`CoverageGrid` owns the host-neutral raster/field coverage the `Graph/element#NODE_MODEL` `Node.Coverage` case wraps, holding the gridded data BY CONTENT KEY (a `RasterKey` into the same seed-zero `XxHash128` object store the geometry blobs use — never an inlined pixel buffer), the kernel `Rasm.Numerics` `CellLattice` placement (the branch's ONE bounded rectangular cell lattice — an index-to-world affine, a per-axis census, and one budget ceiling admitted together, so a rotated, skewed, or layered grid is exact rather than truncated to the north-up planar special case a six-coefficient geotransform carries), an `OverviewLevel` pyramid derived as that lattice's `Coarsen` chain (the coverage is MULTI-RESOLUTION — a working-resolution consumer picks a level by target resolution and fetches that level's bytes, never the full base raster), a typed `CoverageBand` schema per band, and the `Geospatial/reference#GEO_REFERENCE` `GeoReference` CRS. `CellLattice` arrives INVERTIBLE BY ADMISSION and carries NO host coordinate onto this seam: the kernel publishes its placement as neutral doubles, so `LatticeGeodesy` projects the forward affine as `CentreOf`/`Origin`/`Span` onto the seam-neutral `Vector3` and the stored inverse as `Fractional`'s bare `(Column, Row)` pair, and a site-context or environmental consumer reads ONE `Sample` whose discrete-cell-vs-continuous-fraction shape the `CoverageKind.Interpolates` policy column selects — a continuous `Field` yields the fractional coordinate for interpolation, a discrete `Raster` the containing in-bounds cell — never re-branching the resampling per consumer and never touching the raster bytes for the geometry.

`CoverageBand` types every band and describes it FULLY, never stringly. Pixel storage is the kernel `Rasm.Drawing` `ChannelDtype` roster verbatim — the sixteen storage rows each carrying their `Width` and a `Complex` column, so a coverage sizes a blob fetch and reads a complex pair on the SAME vocabulary the kernel encode/decode arena packs through and a raster band, a texture plane, and a packed geometry channel never spell one storage type three ways; `BandRole` is the display channel (`Gray`/`Red`/`Green`/`Blue`/`Alpha`/`Palette`) so a multi-band orthophoto is self-describing; `NoData` is an `Option<double>` (a band may carry no sentinel at all, mirroring the optional GDAL nodata flag); `Units` and the `Offset`/`Scale` linear decode (`Real(raw) = Offset + Scale·raw`) let a scaled-integer DEM read in real units; `Range` carries the optional `(Min, Max)` a consumer reads for display-normalization from the metadata alone (the optional GDAL min/max flag); and a `Palette` band carries its `ColorBin` index→colour-and-category legend whose colour is an admitted kernel `PerceptualColor`, so an indexed land-cover or soil-stratum raster decodes a cell value to a mixable, contrast-testable colour and a category label WITHOUT a parallel sidecar — the `Palette` role is never a hollow channel with no table behind it, and a continuous field reads `Shade` for the perceptual interpolation between bracketing bins no display-byte quad expresses. `CoverageBand` INTERNALIZES the raster channel/scaling/legend vocabulary, so a downstream coverage consumer never re-learns the GDAL `DataType`/`ColorInterp`/`GetOffset`/`GetMinimum`/`GetRasterColorTable`/`GetDefaultRAT` surface.

`CoverageGrid` is the continuous-field counterpart to the discrete `Object` graph — a digital elevation model, a solar-irradiance field, a noise-contour raster, a soil-stratum grid. Vector features ride the `Object` node (a georeferenced object with a `Classification` and properties), so the seam carries NO parallel `Feature` family: the `NetTopologySuite` Simple-Features algebra, the `STRtree` broad phase, the GDAL/OGR raster+vector ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf codecs all live in `Rasm.Bim`, which writes the raster bytes (each pyramid level its own content-keyed blob) to the object store and lowers only the grid metadata + overview + band schema onto this `CoverageGrid`, lowering a vector feature onto an `Object` node — the seam unifying the discrete BIM graph and the continuous geospatial field under one node model without a second geometry stack.

`CoverageGrid` stays TIME-AWARE where its field is time-varying: the `TimeSlice` run is the NetCDF/time-enabled-COG temporal axis lowered onto the seam — every slice shares the ONE grid, pyramid, and band schema and differs only by its anchor `Instant` and its own content-keyed `RasterKey`, so an hourly irradiance stack, a seasonal flood-level series, or a climate-projection scenario is ONE `Coverage` node whose timeline is metadata-selected (`SliceAt` the latest-at-or-before step-function read, `SliceWindow` the interval clip) exactly as the pyramid is (`LevelFor`), never a slice-per-node spray a consumer re-correlates by naming convention; an atemporal coverage carries an empty slice run and the base `RasterKey` alone.

`CoverageGrid.CanonicalBytes` is the coverage's content identity — the `Graph/element#NODE_MODEL` `Node.Coverage` arm delegates to it, so a non-rooted coverage node's `NodeId` derives from its kind, the lattice's twelve index-to-world affine coefficients and three-axis census, base `RasterKey`, the resolution-ordered overview levels, the instant-ordered time slices, the CRS (`Geospatial/reference#GEO_REFERENCE` `GeoReference.CanonicalBytes`), and the index-ordered per-band schema (storage row, role, nodata, units, decode, range, palette) — IEEE-754-canonical and order-stable across the C#/Python/TypeScript runtimes that share the one `XxHash128` seed (`Projection/address#CONTENT_ADDRESS`). Twelve coefficients spell the whole affine (the fourth row is the invariant `[0 0 0 1]`), so a rotated or layered placement — which a six-coefficient planar geotransform silently collapses onto one key — keys distinctly. `Of` is the ONE public admission (the record constructor is PRIVATE, so unadmitted grid state is unrepresentable) and ACCUMULATES its independent admission invariants (each one `Validation<Error,_>` slot, collapsed once to the `Fin<T>` rail), railing `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` on an empty band set, duplicate band indices, a band whose `Offset`/`Scale` decode or `(Min, Max)` range is non-finite or inverted, an overview run that is not the base lattice's `Coarsen` chain, a time-slice run whose stored instants are not strictly increasing, or a `Palette` band whose colour-bin legend is absent or carries duplicate indices — every violation reported in one `Fin.Fail`, never first-fault-wins. Grid degeneracy is NOT a gate here: a `CellLattice` is its own admission evidence (its private constructor is reachable only through `CellLattice.Of`, whose invertibility and cell-budget gates already ran), so the seam admits a placement it cannot re-doubt.

## [01]-[INDEX]

- [02]-[COVERAGE_NODE]: `CoverageGrid` describes the raster/field by reference over the kernel `CellLattice` placement — `CoverageKind` carrying the `Interpolates` sampling policy, `BandRole` the display channel, `ColorBin` the palette legend over an admitted `PerceptualColor`, `CoverageSample` the discrete-or-fractional read result, `LatticeGeodesy` the geospatial `Resolution` reading and lattice canonical projection, `OverviewLevel` the pyramid row with `TileOf`, `TimeSlice` the temporal row, and `CoverageBand` the per-band schema; `Of` admits, `Sample`/`CellAt`/`CellCenter` project, `Shade` reads the legend, `LevelFor`/`SliceAt`/`SliceWindow`/`Window`/`TileAt` select, `ByteLength` sizes a fetch, and `CanonicalBytes` projects content.

## [02]-[COVERAGE_NODE]

- Owner: `CoverageGrid` the host-neutral coverage descriptor the `Graph/element#NODE_MODEL` `Node.Coverage` case wraps; `CoverageKind` the `Raster`/`Field` `[SmartEnum<string>]` carrying the `Interpolates` sampling-policy column; `BandRole` the `[SmartEnum<string>]` display channel (the GDAL `ColorInterp` set, reduced to the roles a coverage consumer reads); `ColorBin` the palette index→`PerceptualColor`-and-category legend entry (the GDAL `ColorTable`/`RasterAttributeTable` lowered onto the kernel colour owner); `CoverageSample` the `[Union]` discrete-cell-or-fractional sample result the `Kind.Interpolates` column selects; `LatticeGeodesy` the `extension(CellLattice)` block carrying the geospatial `Resolution` reading and the lattice `CanonicalBytes` projection the base grid and every level both compose; `OverviewLevel` the pyramid-level row (the level's own `CellLattice` + per-level `RasterKey` + tile `BlockX`/`BlockY`) carrying the `TileOf` block-window accessor; `TimeSlice` the temporal row (`Instant` anchor + per-slice `RasterKey`) a time-varying coverage carries beside the pyramid; `CoverageBand` the per-band schema row over the kernel `ChannelDtype` storage roster; the `RasterKey` content key to the bytes by-reference, the base raster's `BaseBlockX`/`BaseBlockY` tile dimensions (symmetric with each overview's block, so the full-resolution base read aligns to tiles the same way an overview read does); the `GeoReference` CRS the coverage carries.
- Exemption: `CoverageSample` is the one RECORD-root `[Union]` on this page, carved from the class-root + `[Equatable]` `[GRAPH_FAMILY]` form every stored owner here takes, on a genuinely distinct payload-timing-and-consumer discriminant: it is a transient read result that never seats on a node, never contributes to `CanonicalBytes`, and never enters the `Rasm.Persistence` `StructuralMerge`, so the record root's generated structural equality IS the whole requirement and a member drill has nothing below it to reach. Its closure still rides the private root constructor, exactly as the class-root owners' does.
- Entry: `CoverageGrid.Of(kind, rasterKey, grid, bands, crs, key, overviews, slices)` admits a coverage on the `Fin<T>` rail — the ONE public admission over the PRIVATE record constructor, so every instance carries the gates — the independent invariants ACCUMULATING through the shared `Projection/fault#ADMISSION_SLOTS` algebra (each one `Gate` slot, the `Accumulate` fold unioning every miss so a malformed lowering reports them all at once): `ElementFault.ValueRejected` on an empty band set, duplicate band indices, a decode-degenerate band (non-finite `Offset`/`Scale`, or a non-finite or inverted `Range`), a negative base or overview block extent (`0` is the admitted untiled sentinel), an overview run that is not the base lattice's `Coarsen` chain, a `TimeSlice` run whose stored instants are not strictly increasing, or a `Palette` band whose legend is empty or carries duplicate colour-bin indices; the `CellLattice` placement arrives ADMITTED, so grid degeneracy is unrepresentable rather than gated. `Grid.CentreOf(col, row)` maps a cell onto its world point through the full affine (rotation- and layer-aware) and `Grid.Fractional(x, y)` inverts a world coordinate back to a fractional cell, both answering seam-neutral values off the `LatticeGeodesy` projection block, the railed `Sample(x, y, key)` returns the `CoverageKind.Interpolates`-selected `CoverageSample` (the `Field` fractional inverse or the `Raster` containing cell), `CellAt(x, y, key)` floors-and-bounds-checks the finite inverse into the containing in-bounds discrete cell (`None` outside the coverage), `CellCenter(col, row)` projects a cell's centre world point as the seam-neutral `Vector3`, `Shade(index, raw)` reads the band legend under the same `Interpolates` policy (the exact bin's colour for a `Raster`, the perceptual mix between bracketing bins for a `Field`), `LevelFor(targetResolution, key)` resolves the coarsest pyramid level still finer than a finite positive target resolution, `SliceAt(instant)` resolves the governing time slice (the latest at-or-before, the step-function read) and `SliceWindow(from, to, key)` rails a reversed interval before clipping it, `Window(x0, y0, x1, y1, key, level)` rails non-finite or reversed bounds before clipping a world rect onto the chosen level's OWN lattice (`None` off-coverage), `OverviewLevel.TileOf(col, row)`/`CoverageGrid.TileAt(col, row)` resolve the GDAL block window a windowed fetch aligns to (the containing tile column/row and its bounds-clipped cell-extent — an overview level and the base raster sharing the ONE tiling-arithmetic body), `BandAt(index)` resolves a band descriptor, `ByteLength(level)` sums the uncompressed raster size across bands at a chosen level, and `CanonicalBytes(writer)` projects the coverage's content into the shared canonical bytes.
- Auto: `Of` validates each band decode-sound (`CoverageBand.IsDegenerate` — finite `Offset`/`Scale`, a finite ordered `Range`; `NoData` deliberately ungated because a NaN/±∞ sentinel is legal GDAL float nodata), the band set non-empty, the band indices distinct, every base/overview block extent non-negative (`0` alone means untiled), the `RasterKey` being the content key the blob store resolves, the overview run being the base lattice's `Coarsen` CHAIN (one fold threads the expected lattice — base, then each successor — and dies to `None` at the first level whose own lattice is not its predecessor coarsened once, so the wider-than-base, cell-not-coarser, non-finite-cell-size, and non-monotone gates collapse into this ONE proof and the pyramid's level ordinal IS its position in the run), each `Palette`-role band's legend non-empty with distinct `ColorBin` indices, and the `TimeSlice` run's stored instants strictly increasing (the `Zip`-adjacent monotone law, so `SliceAt`'s latest-at-or-before fold is total over stored order); `Grid.CentreOf`/`Grid.Origin`/`Grid.Fractional` project the kernel lattices own forward and inverse affine onto seam-neutral values, so the inverse is exact for any admitted grid and a consumer reads a cell's world point — or a point's cell — from the metadata alone; `Sample` reads the `Kind.Interpolates` policy column and yields `CoverageSample.Fraction` (the raw fractional cell for a `Field`) or `CoverageSample.Cell` (the floored in-bounds cell for a `Raster`, `Inside` false outside), so a consumer never re-decides the resampling; `LevelFor` folds the overview run picking the coarsest level whose `Resolution` still resolves a target, defaulting the base when no overview is fine enough; `Window` projects the four world-rect corners through `Fractional` on the CHOSEN LEVEL'S OWN lattice (rotation-exact), floors the bounding envelope to containing cells, and clips to that lattice's census (`None` when disjoint) — each level carrying its own affine, so no base-to-level ratio scaling exists to get wrong; the per-band `SampleType.Width` sizes a fetch, `CoverageBand.Real(raw)` applies the `Offset`/`Scale` decode, `CoverageBand.IsNoData(raw)` tests the optional sentinel (NaN-safe via `double.Equals`), `CoverageBand.Decode(raw)` resolves a `Palette` band's raw index to its `ColorBin` legend entry, and `CoverageGrid.Shade(index, raw)` reads that legend under the `Interpolates` policy so a discrete raster answers the exact bin's colour and a continuous field the perceptual `Mix` between the bracketing bins; `TileOf`/`TileAt` floor a cell to its containing tile through the carried `BlockX`/`BlockY` (an untiled/strip level reads as one full-width row band, an out-of-bounds cell `None`), so a windowed fetch aligns to the GDAL block grid from the metadata alone; `CanonicalBytes` writes the kind, the base lattice's twelve affine coefficients and three-axis census, the base `RasterKey` and the base `BaseBlockX`/`BaseBlockY` tiling, the resolution-ordered overview levels (each folding its own lattice and block size), the CRS (delegating to `GeoReference.CanonicalBytes`), and each band in `Index` order with its full schema, every `double` through the `Projection/address#CONTENT_ADDRESS` IEEE-754 canon so identity is byte-stable cross-runtime and a re-tile forks the base identity the same way it forks an overview's.
- Receipt: the `CoverageGrid` is the gridded-field evidence a site-context or environmental consumer reads — the kind, the admitted lattice, the resolution pyramid, and the typed band schema (decode + range + palette) in the node, the heavy raster in the content-keyed blob store addressed per-level by `RasterKey`; a `Graph/element#ELEMENT_GRAPH` `Bake`-derived `Element` whose object is a site context carries its `Coverage` nodes flat in `element.Coverages`, so a `Rasm.Compute` environmental route resolves an element's placement to a sample (`Sample`), picks a working level (`LevelFor`), clips the site region onto that level's cell window (`Window`), sizes the fetch (`ByteLength(level)`), reads that level's bytes by `RasterKey`, decodes a scaled band through `CoverageBand.Real` or an indexed band through `Shade`, and reads the declared bounds from `CoverageBand.Range` — the seam delivering the full sampling schema, the consumer never re-deriving the placement affine, the pyramid selection, or the GDAL band/legend surface.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + the generated `Key`/`TryGet` resolvers the `BandRole` and `CoverageKind` projector lookups use; `[Union]` the `CoverageSample` discrete-or-fractional result), LanguageExt.Core (`Seq`/`Fin`/`Option` + `Validation<Error,_>` the accumulating admission gates fold through `Apply`/`Error.Combine` and collapse `.ToFin()` once + the `Expected`→`Fin` lift the bare `ElementFault` case rides), `Projection/fault#ADMISSION_SLOTS` (the `Gate` slot and `Accumulate` fold the `Of` admission runs on), Generator.Equals (`[Equatable]` the `CoverageGrid` structural equality + member diff the `Graph/element#ELEMENT_GRAPH` snapshot drills into, `[UnorderedEquality]` the index-keyed `Bands`/`Palette` sets — order-independent equality matching the `Index`-keyed canonical sort; `Overviews` and `Slices` keep ORDERED equality because each run's POSITION is load-bearing — the pyramid ordinal under the `Coarsen`-chain law and the strictly-increasing timeline `SliceAt` folds over — so an unordered comparer there forks equality from the content address), `Rasm` (the kernel `Numerics.CellLattice` placement and its `Of`/`Coarsen`/`Affine`/`Inverse`/`Contains`/`CellMeasure`/`Rank` surface, `Numerics.PerceptualColor`/`BlendPath`/`UnitInterval` the legend colour axis, `Drawing.ChannelDtype` the storage roster, the `Op` op-key, and the `Domain.ContentHash` seed-zero content-key entry the `RasterKey` shares with `Projection/address#CONTENT_ADDRESS`), `Projection/address#CONTENT_ADDRESS` (`CanonicalWriter` the `CanonicalBytes` projection writes through), `Geospatial/reference#GEO_REFERENCE` (`GeoReference` + its `CanonicalBytes`).
- Growth: a new pixel storage type is one kernel `ChannelDtype` row (the coverage inherits it with no edit here, and its pack/unpack arms land at the kernel owner); a new display channel is one `BandRole` row; a new band attribute (a category label, a statistic) is one `CoverageBand` column or one `ColorBin` column written into `CanonicalBytes`; a new resolution tier is one `OverviewLevel` row appended to the `Coarsen` chain; a new instant is one `TimeSlice` row (a scenario axis, a slice window, or a per-slice statistic is one `TimeSlice` column); a new sampling policy is one `CoverageKind` column the `Sample` and `Shade` dispatches read; a new fetch-alignment query composes the one `TileOf` body (base and overview share it), never a per-level re-spelling of the `col/BlockX` arithmetic; a new placement parameter is impossible (the lattice's affine and census are the complete placement, layers included); never a per-raster-format coverage type, never a package-local storage-type roster beside the kernel's, never an inlined pixel buffer, never a sidecar palette/legend table beside the band, never a display-byte colour quad where the kernel colour owner is in scope, never a carried tile size with no tiling accessor (the decorative-data thin slice), and never an irregular point-cloud/TIN coverage on this grid owner — an irregular geometry rides an `Object` node by content hash, not this regular-grid descriptor, and a scattered survey, borehole, or sensor set BECOMES a coverage upstream through the kernel `Spatial/fields#SCALAR_FIELD` `ScalarField.SibsonCase` evaluator (natural-neighbour weights off the `Spatial/cloud` Voronoi dual) sampled onto a `CellLattice`, which is why the descriptor needs no irregular case: the whole pyramid, band, `LevelFor`, `Window`, and `TileOf` surface already describes what that evaluator produces.
- Boundary: no host type crosses this seam — the kernel `CellLattice` publishes its placement as neutral doubles (`Affine`/`Inverse` twelve row-major coefficients each, the census and cell measure beside them), so `LatticeGeodesy` composes arithmetic and never a host type, `Origin`/`Span`/`CentreOf` answering the seam-owned `Graph/element` `Vector3` and `Fractional` a bare `(Column, Row)` pair; no `CoverageGrid` member, tabulation, or fence on this page names `Point3d`/`Vector3d`/`Transform` or opens `Rhino.Geometry`, and a package-local `OriginX`/`CellSizeX` quadruple beside the affine is the axis-aligned fiction a rotated lattice silently breaks.
- Boundary: `CoverageGrid` holds the bytes BY REFERENCE — the per-level `RasterKey` content key addresses the raster/field in the same seed-zero `XxHash128` blob store the geometry uses, and an inlined pixel buffer, a host raster handle, or a second hasher on the seam is the named defect; the placement is the kernel `CellLattice` and nothing else, so a package-local geotransform record, an axis-aligned-only descriptor, a north-up sign assumption, or a forward-only map with no inverse is the deleted form — the lattice's private constructor makes a degenerate placement unrepresentable, so this seam gates the placement NOWHERE and a re-doubting `IsDegenerate` check here is a second admission authority beside the kernel's own; the pyramid is the lattice's own `Coarsen` CHAIN, so an arbitrary-factor level set is refused at admission and a source pyramid whose factors are not successive halvings normalizes at the `Rasm.Bim` projector (which re-decimates onto the chain) rather than diverging the level affine from the base — the reward is that `LevelFor`, `Window`, and `TileOf` all read one level's own lattice with no base-to-level ratio anywhere; a coverage is MULTI-RESOLUTION (the `OverviewLevel` pyramid + `LevelFor` selection + the level-keyed `ByteLength`/`RasterKey`), so a single-resolution descriptor that strands a COG/DEM pyramid and forces a full-base fetch is the deleted form; a time-varying field is ONE coverage whose `TimeSlice` run shares the grid, pyramid, and band schema (`SliceAt`/`SliceWindow` the metadata-selected temporal fetch), so a slice-per-node spray a consumer re-correlates by naming convention is the deleted form and the record constructor is PRIVATE so every instance crosses the `Of` gates; sampling is ONE policy-driven `Sample` and legend reading ONE policy-driven `Shade` (the same `Kind.Interpolates` column selecting `Fraction` vs `Cell` and mixed vs exact colour), so a consumer hand-branching `Fractional`-vs-`CellAt` or lerping display bytes per call is the deleted form; a region read is the ONE `Window` world-rect→cell-window projection, so a consumer re-deriving the corner/floor/clamp arithmetic per fetch is the deleted form; a band is typed AND fully self-describing (kernel `ChannelDtype` storage/`BandRole`/`Option<double>` nodata/`Offset`/`Scale`/`Option<(Min,Max)>` range/`Seq<ColorBin>` palette), so a `string` data type, a package-local storage roster, a sentinel-double nodata, a raw-undecoded band, a range-less band a consumer must scan pixels to normalize, or a `Palette` role with no colour table behind it (the hollow channel) is the deleted form; a legend colour is an admitted `PerceptualColor` and the display-byte quadruple exists only inside `CanonicalBytes` (through the kernel's ONE `ToRgb` quantizer, so the content key stays byte-stable cross-runtime because one quantizer owns it), so a stored RGBA quad on the seam is the deleted form; vector features ride the `Object` node, so a parallel `Feature`/`GeoFeature` family on the seam is the deleted form — the `NetTopologySuite` algebra, the `STRtree` index, the GDAL/OGR raster+universal-vector ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf codecs all live in the `Rasm.Bim` `Semantics/geospatial` owner; the `GeoReference` CRS rides the coverage (and the `Header`), DROPPED from the `Object` node, so a coverage carries its own georeference for a multi-CRS site context; `CoverageGrid` is a record carrying `[Equatable]` so the `Rasm.Persistence` `StructuralMerge` drills a changed band/level/colour-bin to `Coverage.Grid.Bands[i].<column>` rather than replacing the whole coverage; `CanonicalBytes` is the coverage's only content projection (id-free, IEEE-754-canonical, band-and-level-order-stable), and a per-coverage ad-hoc serialization is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Generator.Equals;
using LanguageExt;
using LanguageExt.Traits;
using NodaTime;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Element.Projection.AdmissionSlots;

namespace Rasm.Element.Geospatial;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CoverageKind {
 public static readonly CoverageKind Raster = new("raster", interpolates: false); // discrete cells; a sample reads the containing cell (orthophoto, land-cover classification, contour raster)
 public static readonly CoverageKind Field = new("field", interpolates: true);   // a continuous phenomenon; a sample interpolates between cells (elevation surface, irradiance, noise, wind)

 // Interpolates carries the sampling policy the Sample dispatch reads to pick the resampling: a continuous Field yields the
 // fractional inverse (interpolate between cells), a discrete Raster the containing cell — read as a column on Sample,
 // never re-branched per consumer (POLICY_VALUES: behavior rides the vocabulary row, not a caller-side if).
 public bool Interpolates { get; }
}

// LatticeGeodesy reads an admitted kernel lattice geospatially, attached to the receiver this package does not own. The
// kernel publishes the placement HOST-NEUTRALLY — `Affine` the twelve index-to-world coefficients in row-major 3×4
// order (the fourth matrix row is the invariant [0 0 0 1], so twelve IS the whole affine), `Inverse` its stored
// counterpart, and the census/measure reads beside them — so this block projects those neutral doubles onto the
// seam-owned `Graph/element` `Vector3` and answers a fractional cell as a bare pair, with NO host type in scope.
// Both coefficient runs are the kernel's OWN ImmutableArray<double> stores, which carry NO implicit conversion to a
// span: every read spells .AsSpan() at the call site, so the projection stays zero-copy over the kernel's storage
// and never materializes a defensive copy per sample.
// Resolution is the one comparable ground-resolution scalar LevelFor ranks a target against (the cell measure taken
// to its rank root, so a rotated planar lattice and a layered one both answer one magnitude); Origin is the world
// corner of cell (0,0,0) — the geotransform anchor a tabulation or a foreign encoder writes; Span the per-axis world
// extent of one cell, read off the affine columns so a rotated or anisotropic lattice reports true where an
// OriginX/CellSizeX quadruple could only report an axis-aligned fiction. One block serves the base grid and every
// pyramid level, so neither projection is spelled twice.
public static class LatticeGeodesy {
 extension(CellLattice lattice) {
  public double Resolution => lattice.Rank is 2 ? Math.Sqrt(lattice.CellMeasure) : Math.Cbrt(lattice.CellMeasure);

  public Vector3 Origin => Placed(lattice.Affine.AsSpan(), 0.0, 0.0, 0.0);

  public Vector3 Span => Extent(lattice.Affine.AsSpan());

  public Vector3 CentreOf(int column, int row, int layer = 0) =>
   Placed(lattice.Affine.AsSpan(), column + 0.5, row + 0.5, lattice.Rank is 3 ? layer + 0.5 : 0.0);

  // Fractional maps a planar world sample onto its fractional cell through the lattice's own stored inverse — invertibility
  // is admission evidence (CellLattice.Of gated it), so this read is total and no second inversion exists on the seam.
  public (double Column, double Row) Fractional(double x, double y) =>
   Placed(lattice.Inverse.AsSpan(), x, y, 0.0) switch { var local => (local.X, local.Y) };

  // CanonicalBytes projects the placement: the twelve coefficients then the three-axis census.
  public void CanonicalBytes(CanonicalWriter w) {
   foreach (double coefficient in lattice.Affine.AsSpan()) { w.Double(coefficient); }
   w.Ordinal(lattice.Columns.Value).Ordinal(lattice.Rows.Value).Ordinal(lattice.Layers.Value);
  }
 }

 // Row-major 3×4 application; the omitted fourth row is the invariant [0 0 0 1].
 static Vector3 Placed(ReadOnlySpan<double> a, double x, double y, double z) =>
  new(a[0] * x + a[1] * y + a[2] * z + a[3],
      a[4] * x + a[5] * y + a[6] * z + a[7],
      a[8] * x + a[9] * y + a[10] * z + a[11]);

 // Per-axis cell extent is the column norm of the linear block — the anisotropic, rotated, and sheared cases all
 // report their true world size where a stored per-axis size could only mirror an axis-aligned placement.
 static Vector3 Extent(ReadOnlySpan<double> a) =>
  new(Math.Sqrt(a[0] * a[0] + a[4] * a[4] + a[8] * a[8]),
      Math.Sqrt(a[1] * a[1] + a[5] * a[5] + a[9] * a[9]),
      Math.Sqrt(a[2] * a[2] + a[6] * a[6] + a[10] * a[10]));
}

// BandRole names the display channel a band carries (the GDAL ColorInterp set, reduced to the roles a consumer reads):
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

// One palette legend entry — the GDAL ColorTable.GetColorEntry(int)->ColorEntry(c1..c4) quad PLUS the
// RasterAttributeTable category label (GetValueAsString over the GFU_Name column) — lowered onto the seam so an
// indexed Palette band decodes a raw cell value to a colour AND a category WITHOUT a parallel sidecar table. The
// colour is an ADMITTED kernel PerceptualColor: the Rasm.Bim projector clamps the ColorEntry short range and admits
// through PerceptualColor.OfRgb at the host edge, so the legend arrives on the branch's one colour axis and gains
// Mix, Contrast, Difference, and Simulate reach a display-byte quad could not carry — a discrete legend is now
// contrast-testable against its own basemap and a continuous field interpolates perceptually between bins. The
// Category is the optional class name a land-cover/soil-stratum legend carries (empty when the source has no RAT);
// Index is the raw cell value the band stores, and a Palette band's bins are index-distinct (Of enforces).
[Equatable]
public readonly partial record struct ColorBin(int Index, PerceptualColor Colour, string Category = "") {
 // ColorBin keys its content on the display-byte quadruple, projected through the kernel's ONE quantizer: identity is
 // byte-stable across runtimes because every runtime shares that quantizer, not because a raw quad was stored.
 // Quantization stays CONDITION-FREE by kernel law: viewing conditions seat on the BlendPath and DeltaMetric
 // appearance-case payloads alone, never on ToRgb, so no observer or surround reaches this call.
 // Routing one here — even as a gamut row keyed off a display — forks the content key the moment two runtimes
 // adapt under different surrounds, and the fork is invisible because both keys are internally consistent.
 // Perceptual reach on the legend is the DISPLAY axis (Shade, Blend, Contrast, Difference), never this projection.
 public void CanonicalBytes(CanonicalWriter w) {
  (byte r, byte g, byte b, byte a) = Colour.ToRgb();
  w.Ordinal(Index).Ordinal(r).Ordinal(g).Ordinal(b).Ordinal(a).String(Category);
 }
}

// One pyramid level a multi-resolution coverage (a COG/tiled DEM) carries — the GDAL Dataset.GetOverview(i) level the
// Rasm.Bim projector reads (GetOverviewCount levels) and content-keys its own blob into the object store. The
// level carries its OWN CellLattice, so its census, cell size, and world affine are one admitted value rather than
// three loose columns a consumer re-correlates against the base: Width/Height/CellSize/IsCoarserThan all collapse
// onto Grid, and Window reads this lattice directly instead of scaling a base-relative ratio. Of proves the run is the
// base lattice's Coarsen chain, so a level's ordinal IS its position and its affine can never drift from the base.
[Equatable]
public readonly partial record struct OverviewLevel(CellLattice Grid, UInt128 RasterKey, int BlockX, int BlockY) {
 // TileOf resolves the GDAL `GetBlockSize` tile window a windowed fetch ALIGNS to — the operation BlockX/BlockY exist to
 // serve (a stored tile size with no tiling accessor is the decorative-data thin slice, the consumer otherwise
 // re-deriving col/BlockX by hand). Given a cell, resolve its containing tile column/row AND the tile's cell-extent
 // clipped to the level bounds (an edge tile is partial), so a Rasm.Compute windowed read fetches exactly the tile a
 // cell falls in. An untiled (strip) level — BlockX/BlockY 0 — is one full-width row band: TileCol 0, BlockX the level
 // width, the row band BlockY high. None for an out-of-bounds cell (the level resolves no tile outside its grid).
 public Option<(int TileCol, int TileRow, int OriginCol, int OriginRow, int SpanCol, int SpanRow)> TileOf(int col, int row) {
  int width = Grid.Columns.Value, height = Grid.Rows.Value;
  if (!Grid.Contains(col, row)) { return None; }
  int bx = BlockX > 0 ? BlockX : width, by = BlockY > 0 ? BlockY : 1;
  int tileCol = col / bx, tileRow = row / by, originCol = tileCol * bx, originRow = tileRow * by;
  return Some((tileCol, tileRow, originCol, originRow, Math.Min(bx, width - originCol), Math.Min(by, height - originRow)));
 }

 public void CanonicalBytes(CanonicalWriter w) {
  Grid.CanonicalBytes(w);
  w.U128(RasterKey).Ordinal(BlockX).Ordinal(BlockY);
 }
}

// One temporal slice a time-VARYING coverage carries (an hourly irradiance stack, a seasonal groundwater/flood-level
// series, a diurnal noise contour, a climate-projection scenario — the NetCDF/time-enabled-COG time axis lowered onto the
// seam): every slice shares the ONE grid, pyramid, and band schema and differs only by its anchor instant and its
// own content-keyed blob — the slice-per-Coverage-node spray a consumer re-correlates by naming convention is the
// deleted form. At is the slice anchor (the dataset's band timestamp); RasterKey that slice's blob in the same
// seed-zero store. The OverviewLevel row shape, mirrored onto the time axis — but the ordering law is only
// monotone-adjacent, never the pyramid's Coarsen-chain proof: successive anchors derive nothing from one another.
[Equatable]
public readonly partial record struct TimeSlice(Instant At, UInt128 RasterKey) {
 public void CanonicalBytes(CanonicalWriter w) =>
  w.I64(At.ToUnixTimeTicks()).U128(RasterKey);
}

// CoverageSample carries one Sample result — the discrete-cell-or-continuous-fraction discriminant CoverageKind.Interpolates
// policy column selects, so a consumer reads ONE shape rather than choosing Fractional-vs-CellAt itself: a Field yields
// Fraction (the raw fractional cell for an interpolating read; Inside flags whether it lands within the grid bounds),
// a Raster yields Cell (the containing in-bounds integer cell, or Cell with Inside=false carrying the floored cell
// outside bounds). ONE polymorphic sample result, never a parallel SampleFractional/SampleDiscrete pair.
// This family roots on a RECORD union (the PropertyValue/TableRow/GraphMutation form), not the class-root [Equatable]
// [GRAPH_FAMILY] form CoverageGrid itself takes: it is a transient READ RESULT with no stored member, no content-key
// contribution, and no diff to drill, so Thinktecture's record-generated structural equality is the whole equality
// requirement and stacking Generator.Equals here buys a member drill nothing descends into. The private root
// ctor is what CLOSES the family — case discovery is constructor reachability, so without it a foreign shape could
// derive and fall to the default arm as a phantom case.
[Union]
public abstract partial record CoverageSample {
 private CoverageSample() { }

 public sealed record Fraction(double Col, double Row, bool Inside) : CoverageSample;
 public sealed record Cell(int Col, int Row, bool Inside) : CoverageSample;
}

[Equatable]
public readonly partial record struct CoverageBand(
 int Index,
 string Name,
 ChannelDtype SampleType,
 BandRole Role,
 Option<double> NoData = default,
 string Units = "",
 double Offset = 0.0,
 double Scale = 1.0,
 Option<(double Min, double Max)> Range = default,
 [property: UnorderedEquality] Seq<ColorBin> Palette = default) {

 // Real applies the linear decode a scaled-integer band carries (mirroring GDAL Band.GetOffset/GetScale): the value is
 // Offset + Scale·raw, so a UInt16 DEM stored at scale 0.01 reads in metres without the consumer hand-applying it.
 public double Real(double raw) => Offset + (Scale * raw);

 // IsDegenerate flags a band whose decode or range arithmetic cannot run: a non-finite Offset/Scale poisons every Real
 // read, and a Range that is non-finite or inverted (Min > Max, NaN either side) is malformed — Of refuses both so a
 // consumer's decode/normalization is total. NoData stays UNGATED: a NaN/±∞ sentinel is a legal GDAL float nodata the
 // NaN-safe IsNoData exists to match.
 public bool IsDegenerate =>
  !double.IsFinite(Offset) || !double.IsFinite(Scale)
  || Range.Exists(static r => !(r.Min <= r.Max) || !double.IsFinite(r.Min) || !double.IsFinite(r.Max));

 // IsNoData tests the optional sentinel (NaN-safe via double.Equals — a NaN sentinel matches a NaN cell, which == misses);
 // a band with no sentinel (None, the optional GDAL nodata flag clear) never reports a value as absent.
 public bool IsNoData(double raw) => NoData is { IsSome: true, Case: double noData } && raw.Equals(noData);

 // Resolve a Palette band's raw cell value to its ColorBin legend entry (the GDAL ColorTable/RAT lookup): an indexed
 // land-cover/soil raster decodes a cell to its colour AND its class name. None when the band carries no palette (a
 // non-Palette band) or the raw index is not in the legend — the consumer rails its own missing-legend handling
 // rather than defaulting a colour. The raw value floors to the integer index the legend keys on.
 public Option<ColorBin> Decode(double raw) => Palette.Find(b => b.Index == (int)Math.Floor(raw));

 // Bracket resolves the legend bins for a raw value — the greatest bin at-or-below and the least strictly above — folded
 // in ONE pass over an unordered legend, so no sort materializes and the read is total over any stored order.
 private (Option<ColorBin> Lower, Option<ColorBin> Upper) Bracket(double raw) =>
  Palette.Fold((Lower: Option<ColorBin>.None, Upper: Option<ColorBin>.None), (best, bin) =>
   bin.Index <= raw
    ? (best.Lower.Exists(held => held.Index >= bin.Index) ? best.Lower : Some(bin), best.Upper)
    : (best.Lower, best.Upper.Exists(held => held.Index <= bin.Index) ? best.Upper : Some(bin)));

 // Blend interpolates PERCEPTUALLY between bracketing bins for a continuous Field — the capability the display-byte
 // quad could not carry, because a channel-wise byte lerp travels the sRGB diagonal rather than the perceptual axis
 // and a land-cover ramp read that way bands and shifts hue. Below the first bin and above the last the terminal
 // bin's colour holds (a legend states no extrapolation), and a one-bin legend is that bin. Bins are index-distinct
 // at admission, so the bracket span is strictly positive and the unit quotient is total.
 public Option<PerceptualColor> Blend(double raw) => Bracket(raw) switch {
  ({ IsSome: true, Case: ColorBin lo }, { IsSome: true, Case: ColorBin hi }) =>
   Some(lo.Colour.Mix(hi.Colour, UnitInterval.Create(value: (raw - lo.Index) / (hi.Index - lo.Index)))),
  ({ IsSome: true, Case: ColorBin lo }, _) => Some(lo.Colour),
  (_, { IsSome: true, Case: ColorBin hi }) => Some(hi.Colour),
  _ => None,
 };
}

// --- [MODELS] -----------------------------------------------------------------------------
[Equatable]
public sealed partial record CoverageGrid {
 public CoverageKind Kind { get; }
 public UInt128 RasterKey { get; }
 public CellLattice Grid { get; }
 [UnorderedEquality] public Seq<CoverageBand> Bands { get; }
 public GeoReference Crs { get; }
 // ORDERED: the run's position IS the pyramid ordinal under the Coarsen-chain law, so a reordered run is a
 // different pyramid — the unordered comparer the index-keyed Bands and Palette sets take would erase that.
 public Seq<OverviewLevel> Overviews { get; }
 // ORDERED for the same reason: admission proves the stored instants strictly increasing and `SliceAt`'s
 // latest-at-or-before fold, `SliceWindow`'s clip, and `CanonicalBytes` all read that stored order, so an unordered
 // comparer would rule two coverages equal whose timelines — and therefore whose content addresses — differ.
 public Seq<TimeSlice> Slices { get; }
 public int BaseBlockX { get; }
 public int BaseBlockY { get; }

 // PRIVATE ctor + GET-ONLY members (the AssessmentPayload shape): Of is the ONLY public admission, so a
 // decode-degenerate band, a hollow palette, an off-chain pyramid, or a non-monotone timeline is UNREPRESENTABLE —
 // a positional public ctor beside the gates is the bypass that lets unadmitted state reach Sample/Window/LevelFor;
 // a wire or persistence decoder re-admits through the SAME railed Of (the ContentAddress.Verify distrust posture),
 // and no init/set survives for a `with`/object-initializer to re-open an invariant.
 private CoverageGrid(
  CoverageKind kind, UInt128 rasterKey, CellLattice grid, Seq<CoverageBand> bands, GeoReference crs,
  Seq<OverviewLevel> overviews, Seq<TimeSlice> slices, int baseBlockX, int baseBlockY) =>
  (Kind, RasterKey, Grid, Bands, Crs, Overviews, Slices, BaseBlockX, BaseBlockY) =
   (kind, rasterKey, grid, bands, crs, overviews, slices, baseBlockX, baseBlockY);

 // EVERY admission invariant is INDEPENDENT of its siblings — no gate reads another's result — so the gates
 // ACCUMULATE through the shared Projection/fault#ADMISSION_SLOTS algebra (VALIDATION_MONOID): each Gate is one
 // Validation slot, Accumulate folds the run, and .ToFin() collapses ONCE at the seam return, so a malformed
 // GDAL lowering reports EVERY violated invariant in one Fin.Fail, never first-fault-wins (the early-abort guard
 // chain is the rejected form); the public rail stays Fin<CoverageGrid>. The grid arrives ADMITTED — CellLattice.Of
 // already ran the invertibility and cell-budget gates behind a private ctor — so re-doubting the placement here
 // would mint a second admission authority for one fact. The slice gate enforces the stored timeline strictly
 // increasing, so SliceAt's latest-at-or-before fold is total over stored order.
 public static Fin<CoverageGrid> Of(
  CoverageKind kind, UInt128 rasterKey, CellLattice grid, Seq<CoverageBand> bands, GeoReference crs, Op key,
  Seq<OverviewLevel> overviews = default, Seq<TimeSlice> slices = default, int baseBlockX = 0, int baseBlockY = 0) =>
  Accumulate(Seq(
      Gate(!bands.IsEmpty, key, "<coverage-bands-empty>"),
      Gate(bands.Map(static b => b.Index).Distinct().Count == bands.Count, key, "<coverage-band-index-duplicate>"),
      Gate(!bands.Exists(static b => b.IsDegenerate), key, "<coverage-band-decode-degenerate>"),
      Gate(baseBlockX >= 0 && baseBlockY >= 0, key, "<coverage-base-block-negative>"),
      Gate(!overviews.Exists(static o => o.BlockX < 0 || o.BlockY < 0), key, "<coverage-overview-block-negative>"),
      Gate(Coarsens(grid, overviews, key), key, "<coverage-overview-off-coarsen-chain>"),
      Gate(!slices.Zip(slices.Tail).Exists(static p => p.Item2.At <= p.Item1.At), key, "<coverage-slice-non-monotone>"),
      Gate(!bands.Exists(static b => b.Role == BandRole.Palette && b.Palette.IsEmpty), key, "<coverage-palette-empty>"),
      Gate(!bands.Exists(static b => b.Role == BandRole.Palette && b.Palette.Map(static c => c.Index).Distinct().Count != b.Palette.Count), key, "<coverage-palette-index-duplicate>")))
   .Map(_ => new CoverageGrid(kind, rasterKey, grid, bands, crs, overviews, slices, baseBlockX, baseBlockY))
   .ToFin();

 // Coarsens proves the pyramid IS the base lattice's Coarsen chain: one fold threads the expected lattice — base, then each
 // successor — and dies to None at the first level whose own lattice is not its predecessor coarsened exactly once.
 // That ONE proof subsumes the four gates a descriptor-less level set needed (coarser-than-base dims, a cell size
 // finite and exceeding the base's, and a strictly-monotone stored order), and it buys what those never could: every
 // level's world affine is DERIVED from the base, so a level can never drift its origin or rotation, its ordinal is
 // its position in the run, and Window/TileOf read the level's own lattice with no base-relative ratio. A source
 // pyramid whose decimation factors are not successive halvings normalizes at the Rasm.Bim projector, which
 // re-decimates onto the chain rather than lowering an affine the base cannot reproduce.
 private static bool Coarsens(CellLattice basis, Seq<OverviewLevel> levels, Op key) =>
  levels.Fold(Some(basis), (expected, level) =>
   expected.Bind(prior => prior.Coarsen(key).ToOption()).Filter(next => next == level.Grid)).IsSome;

 public Option<CoverageBand> BandAt(int index) => Bands.Find(b => b.Index == index);

 // TileAt resolves the base-raster tile window a windowed fetch ALIGNS to — the dual of OverviewLevel.TileOf for the
 // FULL-resolution base raster (whose tile size the COG carries on BaseBlockX/BaseBlockY, symmetric with each overview's BlockX/BlockY,
 // so the base is never the un-tiled level that strands a tiled-COG base read). One synthetic base OverviewLevel
 // (the base lattice + base RasterKey + base block) reuses the ONE TileOf body, so the tile arithmetic is owned
 // once across base and overviews, never re-spelled per level.
 public Option<(int TileCol, int TileRow, int OriginCol, int OriginRow, int SpanCol, int SpanRow)> TileAt(int col, int row) =>
  new OverviewLevel(Grid, RasterKey, BaseBlockX, BaseBlockY).TileOf(col, row);

 // Shade is the ONE policy-driven legend read, the colour dual of Sample: the SAME CoverageKind.Interpolates column that
 // picks a fractional or discrete sample picks an interpolated or exact colour, so a discrete land-cover raster
 // answers its bin's declared colour while a continuous field answers the perceptual mix between bracketing bins.
 // None for a band that carries no legend or a raw value outside it — the consumer rails a missing legend rather
 // than receiving a default colour. A caller lerping display bytes, or branching on Kind itself, is the deleted form.
 public Option<PerceptualColor> Shade(int index, double raw) =>
  BandAt(index).Bind(band => Kind.Interpolates ? band.Blend(raw) : band.Decode(raw).Map(static bin => bin.Colour));

 // Sample is the ONE policy-driven read: it takes the CoverageKind.Interpolates column and yields the matching CoverageSample —
 // a continuous Field yields the raw fractional inverse (the interpolation cell, Inside flagging in-bounds), a discrete
 // Raster yields the FLOORED containing cell (Inside flagging in-bounds). A consumer reads one result shape and never
 // re-decides the resampling: a Field consumer reads Fraction.Col/Row, a Raster consumer Cell.Col/Row — the discriminant
 // is recoverable from the value, never a caller-side if on Kind. Floor (not Round) is the GDAL pixel-containment idiom: the
 // fractional integer part is a cell's top-left corner, so cell (c,r) spans [c,c+1)×[r,r+1) and the containing
 // cell is FLOOR — a point in a cell's second half rounds UP to the wrong cell, a negative fractional rounds toward zero.
 public Fin<CoverageSample> Sample(double x, double y, Op key) =>
  double.IsFinite(x) && double.IsFinite(y)
   ? Fin.Succ(SampleAdmitted(x, y))
   : ElementFault.ValueRejected(key, $"<coverage-sample-coordinate-non-finite:{x:R}:{y:R}>");

 internal CoverageSample SampleAdmitted(double x, double y) {
  (double col, double row) = Grid.Fractional(x, y);
  (int c, int r, bool inside) = Discrete(col, row);
  return Kind.Interpolates
   ? new CoverageSample.Fraction(col, row, inside)
   : new CoverageSample.Cell(c, r, inside);
 }

 // CellAt resolves the CONTAINING in-bounds discrete cell for a planar world coordinate (the explicit Raster read):
 // FLOOR(Fractional) bounds-checked, None outside the coverage. The discrete projection of Sample a caller takes when it wants
 // a cell regardless of Kind (a Field consumer still snapping to a cell for a nearest-neighbour read); Sample is the
 // Kind-driven entry, CellAt the explicit-discrete one — both compose Grid.Fractional AND the ONE Discrete floor-and-bound
 // owner below, so the FLOOR(Fractional)+Contains containment rule is spelled once (the deleted form is each re-flooring inline).
 public Fin<Option<(int Col, int Row)>> CellAt(double x, double y, Op key) =>
  double.IsFinite(x) && double.IsFinite(y)
   ? Fin.Succ(CellAtAdmitted(x, y))
   : ElementFault.ValueRejected(key, $"<coverage-cell-coordinate-non-finite:{x:R}:{y:R}>");

 internal Option<(int Col, int Row)> CellAtAdmitted(double x, double y) {
  (double col, double row) = Grid.Fractional(x, y);
  return Discrete(col, row) is (int c, int r, true) ? Some((c, r)) : None;
 }

 // Discrete owns the ONE floor-and-bound containment rule both Sample and CellAt compose: a fractional cell floors to its
 // containing integer cell (the GDAL top-left-corner pixel-containment idiom) and the bounds check flags in-grid — the sole owner
 // of the cell-containment law, so the FLOOR(col)/FLOOR(row)/Contains triple is never re-spelled per read.
 private (int Col, int Row, bool Inside) Discrete(double col, double row) {
  int c = (int)Math.Floor(col), r = (int)Math.Floor(row);
  return (c, r, Grid.Contains(c, r));
 }

 // CellCenter projects a discrete cell's CENTRE world point — the dual of CellAt a consumer reads to place a value (a
 // sampled irradiance, a contour vertex) at the cell's georeferenced centroid rather than its corner. The lattice
 // owns the half-cell offset, so this seam never re-spells the col+0.5 idiom the kernel already carries.
 public Vector3 CellCenter(int col, int row) => Grid.CentreOf(col, row);

 // Resolve the coarsest pyramid level whose resolution still resolves a target ground resolution — the working-level
 // selection a Rasm.Compute environmental route makes before a fetch so it reads a decimated overview rather than the
 // full base raster (the GDAL overview-selection idiom). Folds the overview run keeping the coarsest level still
 // finer-or-equal than the target; the base (None) is the floor when no overview is fine enough (or none exist), so a
 // consumer always gets a level. A target finer than the base returns the base — the source resolves no finer.
 public Fin<Option<OverviewLevel>> LevelFor(double targetResolution, Op key) =>
  double.IsFinite(targetResolution) && targetResolution > 0.0
   ? Fin.Succ(Overviews
      .Filter(o => o.Grid.Resolution <= targetResolution)
      .Fold(Option<OverviewLevel>.None, (best, o) =>
       best is { IsSome: true, Case: OverviewLevel current } && current.Grid.Resolution >= o.Grid.Resolution ? best : Some(o)))
   : ElementFault.ValueRejected(key, $"<coverage-target-resolution-non-finite-or-non-positive:{targetResolution:R}>");

 // SliceAt is the TEMPORAL dual of LevelFor: the slice governing an instant is the latest at-or-before it — the
 // step-function read a time-varying field takes (an hourly irradiance value holds until its successor lands).
 // Stored order is admitted strictly increasing, so the fold's last accepted slice IS the governing one;
 // None before the first slice or on an atemporal coverage (empty Slices — the base RasterKey is then the one blob).
 public Option<TimeSlice> SliceAt(Instant at) =>
  Slices.Fold(Option<TimeSlice>.None, (best, s) => s.At <= at ? Some(s) : best);

 // SliceWindow takes the interval form: every slice anchored inside [from, to] — the range fetch a seasonal analysis composes
 // (SliceWindow -> per-slice RasterKey -> ByteLength-sized fetches), stored monotone order preserved.
 public Fin<Seq<TimeSlice>> SliceWindow(Instant from, Instant to, Op key) =>
  from <= to
   ? Fin.Succ(Slices.Filter(s => s.At >= from && s.At <= to))
   : ElementFault.ValueRejected(key, $"<coverage-slice-window-reversed:{from}:{to}>");

 // Window reads a world rect a Rasm.Compute region fetch composes (LevelFor -> Window -> ByteLength -> TileOf): the
 // four corners invert onto the CHOSEN LEVEL'S OWN lattice (rotation-exact — an axis-aligned world rect is a
 // cell-space parallelogram), take the bounding envelope, floor to containing cells, clip to that lattice's census; None when the rect
 // misses the coverage — the ONE inverse-affine+FLOOR owner serving interval reads, so a consumer never re-spells the
 // corner/floor/clamp arithmetic per fetch. Each level carries its own affine (the Coarsen-chain law), so there is
 // no base-to-level ratio and no second scaling step to get wrong.
 public Fin<Option<(int Col, int Row, int SpanCol, int SpanRow)>> Window(double x0, double y0, double x1, double y1, Op key, Option<OverviewLevel> level = default) =>
  double.IsFinite(x0) && double.IsFinite(y0) && double.IsFinite(x1) && double.IsFinite(y1) && x0 <= x1 && y0 <= y1
   ? Fin.Succ(WindowAdmitted(x0, y0, x1, y1, level))
   : ElementFault.ValueRejected(key, $"<coverage-window-non-finite-or-reversed:{x0:R}:{y0:R}:{x1:R}:{y1:R}>");

 internal Option<(int Col, int Row, int SpanCol, int SpanRow)> WindowAdmitted(double x0, double y0, double x1, double y1, Option<OverviewLevel> level = default) {
  CellLattice lattice = LatticeOf(level);
  (int w, int h) = (lattice.Columns.Value, lattice.Rows.Value);
  (double MinC, double MinR, double MaxC, double MaxR) e =
   Seq(lattice.Fractional(x0, y0), lattice.Fractional(x1, y0), lattice.Fractional(x0, y1), lattice.Fractional(x1, y1))
   .Fold((double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity),
    static (env, c) => (Math.Min(env.Item1, c.Column), Math.Min(env.Item2, c.Row), Math.Max(env.Item3, c.Column), Math.Max(env.Item4, c.Row)));
  (int c0, int r0) = (Math.Max(0, (int)Math.Floor(e.MinC)), Math.Max(0, (int)Math.Floor(e.MinR)));
  (int c1, int r1) = (Math.Min(w - 1, (int)Math.Floor(e.MaxC)), Math.Min(h - 1, (int)Math.Floor(e.MaxR)));
  return c1 < c0 || r1 < r0 ? None : Some((c0, r0, c1 - c0 + 1, r1 - r0 + 1));
 }

 // ByteLength sums the uncompressed raster bytes across bands at a chosen level (each level census · storage
 // Width), the base when no level is given — a consumer sizes a blob fetch from the metadata alone, no host raster
 // opened. A complex row's Width already counts both components, so the sum is exact for SAR/InSAR.
 // Every band spans the SAME census, so the per-cell stride folds once and the census multiplies once — the
 // per-band width·census product the loop form re-multiplied is the arithmetic this factoring deletes.
 public long ByteLength(Option<OverviewLevel> level = default) =>
  LatticeOf(level).CellCount * Bands.Fold(0L, static (stride, b) => stride + b.SampleType.Width);

 // LatticeOf is the ONE level-or-base resolution Window and ByteLength both read, so the "no level means the base"
 // default is stated once rather than re-branched at every level-taking member.
 private CellLattice LatticeOf(Option<OverviewLevel> level) =>
  level is { IsSome: true, Case: OverviewLevel selected } ? selected.Grid : Grid;

 // CanonicalBytes projects the coverage content the Graph/element#NODE_MODEL Node.Coverage arm delegates to: kind, the lattice's
 // TWELVE affine coefficients and three-axis census, the base RasterKey, the ordered overview run, the CRS
 // (Geospatial/reference#GEO_REFERENCE GeoReference.CanonicalBytes), and each band in Index order with its full schema
 // (storage row, role, nodata, units, decode, range, palette) — every double through the shared CanonicalWriter
 // IEEE-754 canon so the coverage's content identity (its content-hashed NodeId and its Projection/address#
 // CONTENT_ADDRESS diff/dedup key) is byte-stable across the runtimes sharing the one XxHash128 seed. Twelve
 // coefficients ARE the whole affine, so a rotated or layered placement the six-coefficient planar geotransform
 // collapsed onto one key now keys distinctly. Two coverages with the same base raster but a different pyramid, CRS,
 // palette, or placement address as the distinct coverages they are.
 public void CanonicalBytes(CanonicalWriter w) {
  w.String(Kind.Key);
  Grid.CanonicalBytes(w);
  w.U128(RasterKey).Ordinal(BaseBlockX).Ordinal(BaseBlockY);
  w.Ordinal(Overviews.Count);
  foreach (OverviewLevel o in Overviews) { o.CanonicalBytes(w); }   // stored order IS the admitted Coarsen chain, so equality and canonical bytes share it
  w.Ordinal(Slices.Count);
  foreach (TimeSlice s in Slices) { s.CanonicalBytes(w); }   // stored order IS the admitted strictly-increasing timeline
  Crs.CanonicalBytes(w);
  w.Ordinal(Bands.Count);
  foreach (CoverageBand b in Bands.OrderBy(static x => x.Index)) {
   w.Ordinal(b.Index).String(b.Name).Ordinal(b.SampleType.Key).String(b.Role.Key).Bool(b.NoData.IsSome);
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

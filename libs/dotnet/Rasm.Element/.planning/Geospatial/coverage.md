# [ELEMENT_COVERAGE]

`CoverageGrid` owns the host-neutral raster/field coverage the `Graph/element#NODE_MODEL` `Node.Coverage` case wraps, holding the gridded data BY CONTENT KEY (a `BlobKey` into the same seed-zero `XxHash128` object store the geometry blobs use — never an inlined pixel buffer), the kernel `Rasm.Numerics` `CellLattice` placement (the branch's ONE bounded rectangular cell grid — an index-to-world affine, a per-axis census, and one budget ceiling admitted together, so a rotated, skewed, or layered grid is exact rather than truncated to the north-up planar special case a six-coefficient geotransform carries), the resolution pyramid as ONE `OverviewLevel` run whose HEAD IS THE BASE and whose successors are that grid's `Coarsen` chain (the coverage is MULTI-RESOLUTION — a working-resolution consumer picks a level by target resolution and fetches that level's bytes, never the full base raster), a typed `CoverageBand` schema per band, and the `Geospatial/reference#GEO_REFERENCE` `GeoReference` CRS. `CellLattice` arrives INVERTIBLE BY ADMISSION and publishes its placement as neutral doubles, so `LatticeGeodesy` projects the forward affine onto the contract-neutral `Vector3` and the stored inverse onto `Fractional`'s bare `(Column, Row)` pair, and a site-context or environmental consumer reads ONE `Sample` whose discrete-cell-vs-continuous-fraction shape the `CoverageKind.Interpolates` policy column selects — never re-branching the resampling per consumer and never touching the raster bytes for the geometry.

`CoverageBand` types every band and describes it FULLY, never stringly. Pixel storage is the kernel `Rasm.Drawing` `ChannelDtype` roster verbatim — the sixteen storage rows each carrying their `Width` and a `Complex` column, so a coverage sizes a blob fetch and reads a complex pair on the SAME vocabulary the kernel encode/decode arena packs through; `BandRole` is the display channel so a multi-band orthophoto is self-describing; `NoData` is an `Option<double>` (a band may carry no sentinel at all, mirroring the optional GDAL nodata flag); `Units` and the `Offset`/`Scale` linear decode (`Real(raw) = Offset + Scale·raw`) let a scaled-integer DEM read in real units; `Range` carries the optional `(Min, Max)` a consumer reads for display-normalization from the metadata alone; and a `Palette` band carries its `ColorBin` index→colour-and-category legend whose colour is an admitted kernel `PerceptualColor`, so an indexed land-cover or soil-stratum raster decodes a cell value to a mixable, contrast-testable colour and a category label WITHOUT a parallel sidecar. `CoverageBand.Of` is the band's OWN result-returning admission — a decode-degenerate band or a hollow palette is unrepresentable at construction, so the grid's gate set carries only what no single row can prove. `CoverageBand` INTERNALIZES the raster channel/scaling/legend vocabulary, so a downstream coverage consumer never re-learns the GDAL `DataType`/`ColorInterp`/`GetOffset`/`GetMinimum`/`GetRasterColorTable`/`GetDefaultRAT` surface.

`CoverageGrid` is the continuous-field counterpart to the discrete `Object` graph — a digital elevation model, a solar-irradiance field, a noise-contour raster, a soil-stratum grid. Vector features ride the `Object` node, so the contract carries NO parallel `Feature` family: the `NetTopologySuite` Simple-Features algebra, the `STRtree` broad phase, the GDAL/OGR raster+vector ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf codecs all live in `Rasm.Bim`, which writes the raster bytes (each pyramid level its own content-keyed blob) to the object store and lowers only the level run + band schema onto this `CoverageGrid` — the contract unifying the discrete BIM graph and the continuous geospatial field under one node model without a second geometry stack.

`CoverageGrid.CanonicalBytes` owns coverage content identity, and `Of` accumulates independent invariants through the kernel admission algebra while preserving every `Coarsen` refusal.

## [01]-[INDEX]

- [02]-[COVERAGE_NODE]: `CoverageGrid` describes the raster/field by reference over the kernel `CellLattice` placement — `CoverageKind` carrying the `Interpolates` sampling policy, `BandRole` the display channel, `ColorBin` the palette legend over an admitted `PerceptualColor`, `CoverageSample` the discrete-or-fractional read result, `LatticeGeodesy` the geospatial `Resolution` reading and grid canonical projection, `OverviewLevel` the level row (the base is the run's head) with `TileOf`, and `CoverageBand` the construction-gated per-band schema; `Of` admits, `Sample`/`CellAt`/`CellCenter` project, `Shade` reads the legend, `LevelFor`/`Window` select, `ByteLength` sizes a fetch, and `CanonicalBytes` projects content.

## [02]-[COVERAGE_NODE]

- Owner: `CoverageGrid` the host-neutral coverage descriptor the `Graph/element#NODE_MODEL` `Node.Coverage` case wraps; `CoverageKind` the `Raster`/`Field` `[SmartEnum<string>]` carrying the `Interpolates` sampling-policy column; `BandRole` the `[SmartEnum<string>]` display channel (the GDAL `ColorInterp` set, reduced to the roles a coverage consumer reads); `ColorBin` the palette index→`PerceptualColor`-and-category legend entry; `CoverageSample` the `[Union]` discrete-cell-or-fractional sample result with `Inside` on the ROOT (one bounds column, two payload cases); `LatticeGeodesy` the `extension(CellLattice)` block carrying the geospatial `Resolution` reading and the grid `CanonicalBytes` projection every level composes; `OverviewLevel` the level row — its OWN `CellLattice`, its per-level `BlobKey`, and an `Option<(int X, int Y)> Block` tile size (absence IS untiled, no zero sentinel) — carrying the `TileOf` block-window accessor, the run's HEAD being the full-resolution base so base and overview share one row shape and one tiling body; `CoverageBand` the construction-gated per-band schema over the kernel `ChannelDtype` storage roster; the `GeoReference` CRS the coverage carries.
- Exemption: `CoverageSample` is the one RECORD-root `[Union]` on this page, carved from the class-root + `[Equatable]` `[GRAPH_FAMILY]` form every stored owner here takes, on a genuinely distinct payload-timing-and-consumer discriminant: it is a transient read result that never seats on a node, never contributes to `CanonicalBytes`, and never enters the `Rasm.Persistence` `StructuralMerge`, so the record root's generated structural equality IS the whole requirement. Its closure still rides the private root constructor.
- Entry: `CoverageBand.Of(index, name, sampleType, role, key, …)` is the band's own result-returning admission — accumulating `<coverage-band-decode-non-finite>` on a non-finite `Offset`/`Scale`, `<coverage-band-range-degenerate>` on a non-finite or inverted `(Min, Max)`, `<coverage-band-palette-empty>` on a `Palette` role with no legend, and `<coverage-band-palette-index-duplicate>` on a colliding legend — and NORMALIZES the legend to index order at intake, so every downstream read walks one regime. `CoverageGrid.Of(kind, levels, bands, crs)` admits a coverage on the `Fin<T>` result — the ONE public admission over the PRIVATE record constructor — accumulating through `Rasm/Domain/validation#ADMISSION_SLOTS`: `<coverage-levels-empty>` on an empty level run, `<coverage-level-block-non-positive>` on a declared tile block with a non-positive extent, the `Coarsens` slot (the KERNEL `Coarsen` refusal carried verbatim, or `<coverage-level-off-coarsen-chain>` where a level's grid is not its predecessor coarsened once), `<coverage-bands-empty>`, and `<coverage-band-index-duplicate>`; bands normalize to index order at intake, so stored order, structural equality, and `CanonicalBytes` all read ONE order. `Sample(x, y, key)` gates through the shared `Finite` slot and returns the `CoverageKind.Interpolates`-selected `CoverageSample`; `CellAt(x, y, key)` floors-and-bounds-checks the finite inverse into the containing in-bounds discrete cell (`None` outside); `CellCenter(col, row)` projects a cell's centre world point as the contract-neutral `Vector3`; `Shade(index, raw)` reads the band legend under the same `Interpolates` policy; `LevelFor(targetResolution)` resolves the coarsest level still finer-or-equal than a finite positive target off the ADMITTED chain order (total — the base is the floor); `Window(x0, y0, x1, y1, level)` gates finiteness and ordering, then clips a world rect onto the CHOSEN level's OWN grid (`None` off-coverage) folding the corner envelope through the one `MeasureBand.Envelope` owner; `OverviewLevel.TileOf(col, row)` resolves the GDAL block window a windowed fetch aligns to; `BandAt(index)` resolves a band descriptor; `ByteLength(level)` sizes the uncompressed fetch; `CanonicalBytes(writer)` projects the coverage's content through the kernel writer's `Doubles`/`Rows`/`Optional` composers.
- Auto: the level run is proven the base's `Coarsen` CHAIN — one fold threads the expected grid (head, then each successor) and the slot carries the kernel's own refusal on the failing step, so every level's world affine is DERIVED from the base, a level can never drift its origin or rotation, its ordinal is its position, and `Window`/`TileOf` read the level's own grid with no base-relative ratio anywhere; `LevelFor` reads that same admitted order — resolutions strictly coarsen along the run, so "coarsest still finer than target" is one fold over stored order with the base as floor, and no extremum machinery re-derives what admission proved; `Sample` reads the `Kind.Interpolates` policy column and yields `CoverageSample.Fraction` or `CoverageSample.Cell`, both carrying the root `Inside`; the per-band `SampleType.Width` sizes a fetch, `CoverageBand.Real(raw)` applies the `Offset`/`Scale` decode, `IsNoData(raw)` tests the optional sentinel (NaN-safe via `double.Equals`), `Decode(raw)` resolves a `Palette` band's raw index to its legend entry, and `Shade` answers the exact bin's colour for a `Raster` and the perceptual `Blend` between bracketing bins for a `Field`; `TileOf` floors a cell to its containing tile through the optional `Block` (an untiled level reads as one full-width row band, an out-of-bounds cell `None`).
- Output: the `CoverageGrid` is the gridded field a site-context or environmental consumer reads — the kind, the admitted level run, and the typed band schema in the node, the heavy raster in the content-keyed blob store addressed per-level by `BlobKey`; a `Rasm.Compute` environmental route resolves an element's placement to a sample (`Sample`), picks a working level (`LevelFor`), clips the site region onto that level's cell window (`Window`), sizes the fetch (`ByteLength`), reads that level's bytes by `BlobKey`, and decodes through `Real`/`Shade`/`Range` — the contract delivering the full sampling schema, the consumer never re-deriving the placement affine, the pyramid selection, or the GDAL band/legend surface.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[Union]`), LanguageExt.Core (`Seq`/`Fin`/`Option`/`Validation`), Generator.Equals (`[Equatable]` structural equality + member diff the `Graph/element#ELEMENT_GRAPH` snapshot drills into), `Rasm` (`Rasm/Domain/validation#ADMISSION_SLOTS`, `Numerics.CellLattice`, `Numerics.PerceptualColor`/`UnitInterval`, `Drawing.ChannelDtype`, `Op`), `Properties/quantity#MEASURE_ALGEBRA` (`MeasureBand.Envelope`), `Projection/address#CONTENT_ADDRESS` (`BlobKey`, `CanonicalWriter`), `Geospatial/reference#GEO_REFERENCE` (`GeoReference`).
- Growth: a new pixel storage type is one kernel `ChannelDtype` row; a new display channel is one `BandRole` row; a new band attribute is one `CoverageBand` column written into its canonical arm; a new resolution tier is one `OverviewLevel` row appended to the `Coarsen` chain; a new sampling policy is one `CoverageKind` column the `Sample` and `Shade` dispatches read; a new fetch-alignment query composes the one `TileOf` body; a temporal axis returns as one level-run-shaped slice run WITH its producer and reader in the same pass (the deleted `TimeSlice` module is the precedent: declared axes with no producer do not ride); never a per-raster-format coverage type, never a package-local storage-type roster beside the kernel's, never an inlined pixel buffer, never a sidecar palette/legend table beside the band, and never an irregular point-cloud/TIN coverage on this grid owner — a scattered survey, borehole, or sensor set BECOMES a coverage upstream through the kernel `Spatial/fields#SCALAR_FIELD` `ScalarField.SibsonCase` evaluator sampled onto a `CellLattice`.
- Boundary: no host type crosses this boundary — the kernel `CellLattice` publishes its placement as neutral doubles (`Affine`/`Inverse` twelve row-major coefficients each), so `LatticeGeodesy` composes arithmetic and never a host type, `CentreOf` answering the contract-owned `Graph/element` `Vector3` and `Fractional` a bare `(Column, Row)` pair — `LatticeGeodesy` OWNS the half-cell centring and the column-norm extent over those neutral coefficients, the one place either is spelled; no member on this page names `Point3d`/`Vector3d`/`Transform` or opens `Rhino.Geometry`. `CoverageGrid` holds the bytes BY REFERENCE — the per-level `BlobKey` addresses the raster in the same seed-zero store the geometry uses, and an inlined pixel buffer, a host raster handle, or a second hasher on the contract is the named defect; the placement is the kernel `CellLattice` and nothing else, so a package-local geotransform record, an axis-aligned-only descriptor, or a forward-only map with no inverse is the deleted form, and a re-doubting placement check here is a second admission authority beside the kernel's own. The base IS the level run's head — a base-beside-the-pyramid column pair re-derives the level shape it already owns, and a source pyramid off the `Coarsen` chain — odd axes ceiling-half, terminal axes remain unchanged, and a three-dimensional chain retains at least two layers — normalizes at the `Rasm.Bim` projector (which re-decimates onto the chain) rather than diverging the level affine from the base. Sampling is ONE policy-driven `Sample` and legend reading ONE policy-driven `Shade`, so a consumer hand-branching `Fractional`-vs-`CellAt` or lerping display bytes per call is the deleted form; a region read is the ONE `Window` projection; a band is typed AND fully self-describing, gated at ITS OWN construction, so a `string` data type, a sentinel-double nodata, a raw-undecoded band, or a `Palette` role with no colour table behind it is unrepresentable; a legend colour is an admitted `PerceptualColor` and the display-byte quadruple exists only inside `CanonicalBytes` (through the kernel's ONE `ToRgb` quantizer, condition-free by kernel law, so the content key stays byte-stable cross-runtime); vector features ride the `Object` node, so a parallel `Feature` family on the contract is the deleted form; the `GeoReference` CRS rides the coverage (and the `Header`), so a coverage carries its own georeference for a multi-CRS site context; `CanonicalBytes` is the coverage's only content projection, and a per-coverage ad-hoc serialization is the named defect.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Generator.Equals;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Properties;
using Rasm.Numerics;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Geospatial;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CoverageKind {
 public static readonly CoverageKind Raster = new("raster", interpolates: false);
 public static readonly CoverageKind Field = new("field", interpolates: true);

 public bool Interpolates { get; }
}

public static class LatticeGeodesy {
 extension(CellLattice grid) {
  public double Resolution => grid.Rank is 2 ? Math.Sqrt(grid.CellMeasure) : Math.Cbrt(grid.CellMeasure);

  public Vector3 CentreOf(int column, int row, int layer = 0) =>
   Placed(grid.Affine.AsSpan(), column + 0.5, row + 0.5, grid.Rank is 3 ? layer + 0.5 : 0.0);

  public (double Column, double Row) Fractional(double x, double y) =>
   Placed(grid.Inverse.AsSpan(), x, y, 0.0) switch { var local => (local.X, local.Y) };

  public void CanonicalBytes(CanonicalWriter w) =>
   w.Doubles(grid.Affine.AsSpan())
    .Ordinal(grid.Columns.Value).Ordinal(grid.Rows.Value).Ordinal(grid.Layers.Value);
 }

 static Vector3 Placed(ReadOnlySpan<double> a, double x, double y, double z) =>
  new(a[0] * x + a[1] * y + a[2] * z + a[3],
      a[4] * x + a[5] * y + a[6] * z + a[7],
      a[8] * x + a[9] * y + a[10] * z + a[11]);
}

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

[Equatable]
public readonly partial record struct ColorBin(int Index, PerceptualColor Colour, string Category = "") {
 public void CanonicalBytes(CanonicalWriter w) {
  (byte r, byte g, byte b, byte a) = Colour.ToRgb();
  w.Ordinal(Index).Ordinal(r).Ordinal(g).Ordinal(b).Ordinal(a).String(Category);
 }
}

[Equatable]
public readonly partial record struct OverviewLevel(CellLattice Grid, ArtifactContent Raster, Option<(int X, int Y)> Block = default) {
 public Option<(int TileCol, int TileRow, int OriginCol, int OriginRow, int SpanCol, int SpanRow)> TileOf(int col, int row) {
  int width = Grid.Columns.Value, height = Grid.Rows.Value;
  if (!Grid.Contains(col, row)) { return None; }
  (int bx, int by) = Block.IfNone((width, 1));
  int tileCol = col / bx, tileRow = row / by, originCol = tileCol * bx, originRow = tileRow * by;
  return Some((tileCol, tileRow, originCol, originRow, Math.Min(bx, width - originCol), Math.Min(by, height - originRow)));
 }

 public void CanonicalBytes(CanonicalWriter w) {
  Grid.CanonicalBytes(w);
  w.String(Raster.Sha256).I64(checked((long)Raster.Bytes))
   .Optional(Block, static (block, wr) => wr.Ordinal(block.X).Ordinal(block.Y));
 }
}

[Union]
public abstract partial record CoverageSample {
 private CoverageSample(bool inside) { Inside = inside; }

 public bool Inside { get; }

 public sealed record Fraction(double Col, double Row, bool Inside) : CoverageSample(Inside);
 public sealed record Cell(int Col, int Row, bool Inside) : CoverageSample(Inside);
}

[Equatable]
public sealed partial record CoverageBand {
 public int Index { get; }
 public string Name { get; }
 public ChannelDtype SampleType { get; }
 public BandRole Role { get; }
 public Option<double> NoData { get; }
 public string Units { get; }
 public double Offset { get; }
 public double Scale { get; }
 public Option<(double Min, double Max)> Range { get; }
 public Seq<ColorBin> Palette { get; }

 private CoverageBand(
  int index, string name, ChannelDtype sampleType, BandRole role, Option<double> noData, string units,
  double offset, double scale, Option<(double Min, double Max)> range, Seq<ColorBin> palette) =>
  (Index, Name, SampleType, Role, NoData, Units, Offset, Scale, Range, Palette) =
   (index, name, sampleType, role, noData, units, offset, scale, range, palette);

 public static Fin<CoverageBand> Of(
  int index, string name, ChannelDtype sampleType, BandRole role,
  Option<double> noData = default, string units = "", double offset = 0.0, double scale = 1.0,
  Option<(double Min, double Max)> range = default, Seq<ColorBin> palette = default) =>
  Accumulate(Seq(
    Finite(($"coverage-band[{index}].offset", offset), ($"coverage-band[{index}].scale", scale)),
    AdmittedRange(range, index),
    Gate(role != BandRole.Palette || !palette.IsEmpty, $"<coverage-band-palette-empty:{index}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(palette.Map(static c => c.Index).Distinct().Count == palette.Count, $"<coverage-band-palette-index-duplicate:{index}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))))
   .Map(_ => new CoverageBand(index, name, sampleType, role, noData, units, offset, scale, range,
   toSeq(palette.OrderBy(static c => c.Index)).Strict()))
   .ToFin();

 private static Validation<Error, Unit> AdmittedRange(Option<(double Min, double Max)> range, int index) =>
  range.Traverse(bounds => (Finite(($"coverage-band[{index}].range.min", bounds.Min), ($"coverage-band[{index}].range.max", bounds.Max)),
    Gate(bounds.Min <= bounds.Max, $"<coverage-band-range-inverted:{index}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)))
    .Apply(static (_, _) => unit).As()).As().Map(static _ => unit);

 public double Real(double raw) => Offset + (Scale * raw);

 public bool IsNoData(double raw) => NoData is { IsSome: true, Case: double noData } && raw.Equals(noData);

 public Option<ColorBin> Decode(double raw) => Palette.Find(b => b.Index == (int)Math.Floor(raw));

 private (Option<ColorBin> Lower, Option<ColorBin> Upper) Bracket(double raw) =>
  Palette.Fold((Lower: Option<ColorBin>.None, Upper: Option<ColorBin>.None), (best, bin) =>
   bin.Index <= raw ? (Some(bin), best.Upper) : (best.Lower, best.Upper.IsSome ? best.Upper : Some(bin)));

 public Option<PerceptualColor> Blend(double raw) => Bracket(raw) switch {
  ({ IsSome: true, Case: ColorBin lo }, { IsSome: true, Case: ColorBin hi }) =>
   Some(lo.Colour.Mix(hi.Colour, UnitInterval.Create(value: (raw - lo.Index) / (hi.Index - lo.Index)))),
  ({ IsSome: true, Case: ColorBin lo }, _) => Some(lo.Colour),
  (_, { IsSome: true, Case: ColorBin hi }) => Some(hi.Colour),
  _ => None,
 };

 public void CanonicalBytes(CanonicalWriter w) {
  w.Ordinal(Index).String(Name).Ordinal(SampleType.Key).String(Role.Key)
   .Optional(NoData, static (nd, wr) => wr.Double(nd))
   .String(Units).Double(Offset).Double(Scale)
   .Optional(Range, static (r, wr) => wr.Double(r.Min).Double(r.Max))
   .Rows(Palette, static (c, wr) => c.CanonicalBytes(wr));
 }
}

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record CoverageGrid {
 public CoverageKind Kind { get; }
 [property: OrderedEquality] public Seq<OverviewLevel> Levels { get; }
 [property: OrderedEquality] public Seq<CoverageBand> Bands { get; }
 public GeoReference Crs { get; }

 public OverviewLevel Base => Levels[0];
 public CellLattice Grid => Base.Grid;
 public ArtifactContent Raster => Base.Raster;

 private CoverageGrid(CoverageKind kind, Seq<OverviewLevel> levels, Seq<CoverageBand> bands, GeoReference crs) =>
  (Kind, Levels, Bands, Crs) = (kind, levels, bands, crs);

 public static Fin<CoverageGrid> Of(
  CoverageKind kind, Seq<OverviewLevel> levels, Seq<CoverageBand> bands, GeoReference crs) =>
  Accumulate(Seq(
    Gate(!levels.IsEmpty, "<coverage-levels-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    AdmittedBlocks(levels),
    Coarsens(levels),
    Gate(!bands.IsEmpty, "<coverage-bands-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(bands.Map(static b => b.Index).Distinct().Count == bands.Count, "<coverage-band-index-duplicate>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))))
   .Map(_ => new CoverageGrid(kind, levels, toSeq(bands.OrderBy(static b => b.Index)).Strict(), crs))
   .ToFin();

 private static Validation<Error, Unit> AdmittedBlocks(Seq<OverviewLevel> levels) =>
  Accumulate(levels.Map((level, index) => level.Block.Traverse(block =>
   (In(block.X, Band.Positive, $"coverage-level[{index}].block.x"),
    In(block.Y, Band.Positive, $"coverage-level[{index}].block.y")).Apply(static (_, _) => unit).As())
   .As().Map(static _ => unit)).Strict());

 private static Validation<Error, Unit> Coarsens(Seq<OverviewLevel> levels) =>
  Accumulate(levels.Zip(levels.Tail).Map(pair =>
   pair.Item1.Grid.Coarsen().ToValidation().Bind(next =>
    Gate(next == pair.Item2.Grid, "<coverage-level-off-coarsen-chain>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)))).Strict());

 public Option<CoverageBand> BandAt(int index) => Bands.Find(b => b.Index == index);

 public Option<PerceptualColor> Shade(int index, double raw) =>
  BandAt(index).Bind(band => Kind.Interpolates ? band.Blend(raw) : band.Decode(raw).Map(static bin => bin.Colour));

 public Fin<CoverageSample> Sample(double x, double y) =>
  Finite(("coverage-sample-x", x), ("coverage-sample-y", y))
   .Map(_ => SampleAdmitted(x, y)).ToFin();

 internal CoverageSample SampleAdmitted(double x, double y) {
  (double col, double row) = Grid.Fractional(x, y);
  (int c, int r, bool inside) = Discrete(col, row);
  return Kind.Interpolates
   ? new CoverageSample.Fraction(col, row, inside)
   : new CoverageSample.Cell(c, r, inside);
 }

 public Fin<Option<(int Col, int Row)>> CellAt(double x, double y) =>
  Finite(("coverage-cell-x", x), ("coverage-cell-y", y))
   .Map(_ => CellAtAdmitted(x, y)).ToFin();

 internal Option<(int Col, int Row)> CellAtAdmitted(double x, double y) {
  (double col, double row) = Grid.Fractional(x, y);
  return Discrete(col, row) is (int c, int r, true) ? Some((c, r)) : None;
 }

 private (int Col, int Row, bool Inside) Discrete(double col, double row) {
  int c = (int)Math.Floor(col), r = (int)Math.Floor(row);
  return (c, r, Grid.Contains(c, r));
 }

 public Vector3 CellCenter(int col, int row) => Grid.CentreOf(col, row);

 public Fin<OverviewLevel> LevelFor(double targetResolution) =>
  In(targetResolution, Band.Positive, "coverage-target-resolution")
   .Map(resolution => Levels.Fold(Base, (best, level) => level.Grid.Resolution <= resolution ? level : best))
   .ToFin();

 public Fin<Option<(int Col, int Row, int SpanCol, int SpanRow)>> Window(
  double x0, double y0, double x1, double y1, OverviewLevel level) =>
  Accumulate(Seq(
    Finite(("coverage-window-x0", x0), ("coverage-window-y0", y0), ("coverage-window-x1", x1), ("coverage-window-y1", y1)),
    Gate(x0 <= x1 && y0 <= y1, $"<coverage-window-reversed:{x0:R}:{y0:R}:{x1:R}:{y1:R}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))))
   .Map(_ => WindowAdmitted(x0, y0, x1, y1, level)).ToFin();

 internal Option<(int Col, int Row, int SpanCol, int SpanRow)> WindowAdmitted(
  double x0, double y0, double x1, double y1, OverviewLevel level) {
  CellLattice grid = level.Grid;
  (int w, int h) = (grid.Columns.Value, grid.Rows.Value);
  ((double cA, double rA), (double cB, double rB), (double cC, double rC), (double cD, double rD)) =
   (grid.Fractional(x0, y0), grid.Fractional(x1, y0), grid.Fractional(x0, y1), grid.Fractional(x1, y1));
  (double minC, double maxC, _) = MeasureBand.Envelope(cA, cB, cC, cD);
  (double minR, double maxR, _) = MeasureBand.Envelope(rA, rB, rC, rD);
  (int c0, int r0) = (Math.Max(0, (int)Math.Floor(minC)), Math.Max(0, (int)Math.Floor(minR)));
  (int c1, int r1) = (Math.Min(w - 1, (int)Math.Floor(maxC)), Math.Min(h - 1, (int)Math.Floor(maxR)));
  return c1 < c0 || r1 < r0 ? None : Some((c0, r0, c1 - c0 + 1, r1 - r0 + 1));
 }

 public long ByteLength(OverviewLevel level) =>
  level.Grid.CellCount * Bands.Fold(0L, static (stride, b) => stride + b.SampleType.Width);

 public void CanonicalBytes(CanonicalWriter w) {
  w.String(Kind.Key)
   .Rows(Levels, static (level, wr) => level.CanonicalBytes(wr));
  Crs.CanonicalBytes(w);
  w.Rows(Bands, static (band, wr) => band.CanonicalBytes(wr));
 }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

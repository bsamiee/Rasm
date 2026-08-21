# [ELEMENT_COVERAGE]

`CoverageGrid` owns the host-neutral raster/field coverage the `Graph/element#NODE_MODEL` `Node.Coverage` case wraps, holding the gridded data BY CONTENT KEY (a `BlobKey` into the same seed-zero `XxHash128` object store the geometry blobs use — never an inlined pixel buffer), the kernel `Rasm.Numerics` `CellLattice` placement (the branch's ONE bounded rectangular cell lattice — an index-to-world affine, a per-axis census, and one budget ceiling admitted together, so a rotated, skewed, or layered grid is exact rather than truncated to the north-up planar special case a six-coefficient geotransform carries), the resolution pyramid as ONE `OverviewLevel` run whose HEAD IS THE BASE and whose successors are that lattice's `Coarsen` chain (the coverage is MULTI-RESOLUTION — a working-resolution consumer picks a level by target resolution and fetches that level's bytes, never the full base raster), a typed `CoverageBand` schema per band, and the `Geospatial/reference#GEO_REFERENCE` `GeoReference` CRS. `CellLattice` arrives INVERTIBLE BY ADMISSION and publishes its placement as neutral doubles, so `LatticeGeodesy` projects the forward affine onto the seam-neutral `Vector3` and the stored inverse onto `Fractional`'s bare `(Column, Row)` pair, and a site-context or environmental consumer reads ONE `Sample` whose discrete-cell-vs-continuous-fraction shape the `CoverageKind.Interpolates` policy column selects — never re-branching the resampling per consumer and never touching the raster bytes for the geometry.

`CoverageBand` types every band and describes it FULLY, never stringly. Pixel storage is the kernel `Rasm.Drawing` `ChannelDtype` roster verbatim — the sixteen storage rows each carrying their `Width` and a `Complex` column, so a coverage sizes a blob fetch and reads a complex pair on the SAME vocabulary the kernel encode/decode arena packs through; `BandRole` is the display channel so a multi-band orthophoto is self-describing; `NoData` is an `Option<double>` (a band may carry no sentinel at all, mirroring the optional GDAL nodata flag); `Units` and the `Offset`/`Scale` linear decode (`Real(raw) = Offset + Scale·raw`) let a scaled-integer DEM read in real units; `Range` carries the optional `(Min, Max)` a consumer reads for display-normalization from the metadata alone; and a `Palette` band carries its `ColorBin` index→colour-and-category legend whose colour is an admitted kernel `PerceptualColor`, so an indexed land-cover or soil-stratum raster decodes a cell value to a mixable, contrast-testable colour and a category label WITHOUT a parallel sidecar. `CoverageBand.Of` is the band's OWN railed admission — a decode-degenerate band or a hollow palette is unrepresentable at construction, so the grid's gate set carries only what no single row can prove. `CoverageBand` INTERNALIZES the raster channel/scaling/legend vocabulary, so a downstream coverage consumer never re-learns the GDAL `DataType`/`ColorInterp`/`GetOffset`/`GetMinimum`/`GetRasterColorTable`/`GetDefaultRAT` surface.

`CoverageGrid` is the continuous-field counterpart to the discrete `Object` graph — a digital elevation model, a solar-irradiance field, a noise-contour raster, a soil-stratum grid. Vector features ride the `Object` node, so the seam carries NO parallel `Feature` family: the `NetTopologySuite` Simple-Features algebra, the `STRtree` broad phase, the GDAL/OGR raster+vector ingest, and the shapefile/GeoPackage/GeoJSON/CityJSON/FlatGeobuf codecs all live in `Rasm.Bim`, which writes the raster bytes (each pyramid level its own content-keyed blob) to the object store and lowers only the level run + band schema onto this `CoverageGrid` — the seam unifying the discrete BIM graph and the continuous geospatial field under one node model without a second geometry stack.

`CoverageGrid.CanonicalBytes` owns coverage content identity, and `Of` accumulates independent invariants through the kernel admission algebra while preserving every `Coarsen` refusal.

## [01]-[INDEX]

- [02]-[COVERAGE_NODE]: `CoverageGrid` describes the raster/field by reference over the kernel `CellLattice` placement — `CoverageKind` carrying the `Interpolates` sampling policy, `BandRole` the display channel, `ColorBin` the palette legend over an admitted `PerceptualColor`, `CoverageSample` the discrete-or-fractional read result, `LatticeGeodesy` the geospatial `Resolution` reading and lattice canonical projection, `OverviewLevel` the level row (the base is the run's head) with `TileOf`, and `CoverageBand` the construction-gated per-band schema; `Of` admits, `Sample`/`CellAt`/`CellCenter` project, `Shade` reads the legend, `LevelFor`/`Window` select, `ByteLength` sizes a fetch, and `CanonicalBytes` projects content.

## [02]-[COVERAGE_NODE]

- Owner: `CoverageGrid` the host-neutral coverage descriptor the `Graph/element#NODE_MODEL` `Node.Coverage` case wraps; `CoverageKind` the `Raster`/`Field` `[SmartEnum<string>]` carrying the `Interpolates` sampling-policy column; `BandRole` the `[SmartEnum<string>]` display channel (the GDAL `ColorInterp` set, reduced to the roles a coverage consumer reads); `ColorBin` the palette index→`PerceptualColor`-and-category legend entry; `CoverageSample` the `[Union]` discrete-cell-or-fractional sample result with `Inside` on the ROOT (one bounds column, two payload cases); `LatticeGeodesy` the `extension(CellLattice)` block carrying the geospatial `Resolution` reading and the lattice `CanonicalBytes` projection every level composes; `OverviewLevel` the level row — its OWN `CellLattice`, its per-level `BlobKey`, and an `Option<(int X, int Y)> Block` tile size (absence IS untiled, no zero sentinel) — carrying the `TileOf` block-window accessor, the run's HEAD being the full-resolution base so base and overview share one row shape and one tiling body; `CoverageBand` the construction-gated per-band schema over the kernel `ChannelDtype` storage roster; the `GeoReference` CRS the coverage carries.
- Exemption: `CoverageSample` is the one RECORD-root `[Union]` on this page, carved from the class-root + `[Equatable]` `[GRAPH_FAMILY]` form every stored owner here takes, on a genuinely distinct payload-timing-and-consumer discriminant: it is a transient read result that never seats on a node, never contributes to `CanonicalBytes`, and never enters the `Rasm.Persistence` `StructuralMerge`, so the record root's generated structural equality IS the whole requirement. Its closure still rides the private root constructor.
- Entry: `CoverageBand.Of(index, name, sampleType, role, key, …)` is the band's own railed admission — accumulating `<coverage-band-decode-non-finite>` on a non-finite `Offset`/`Scale`, `<coverage-band-range-degenerate>` on a non-finite or inverted `(Min, Max)`, `<coverage-band-palette-empty>` on a `Palette` role with no legend, and `<coverage-band-palette-index-duplicate>` on a colliding legend — and NORMALIZES the legend to index order at intake, so every downstream read walks one regime. `CoverageGrid.Of(kind, levels, bands, crs, key)` admits a coverage on the `Fin<T>` rail — the ONE public admission over the PRIVATE record constructor — accumulating through `Rasm/Domain/validation#ADMISSION_SLOTS`: `<coverage-levels-empty>` on an empty level run, `<coverage-level-block-non-positive>` on a declared tile block with a non-positive extent, the `Coarsens` slot (the KERNEL `Coarsen` refusal carried verbatim, or `<coverage-level-off-coarsen-chain>` where a level's lattice is not its predecessor coarsened once), `<coverage-bands-empty>`, and `<coverage-band-index-duplicate>`; bands normalize to index order at intake, so stored order, structural equality, and `CanonicalBytes` all read ONE order. `Sample(x, y, key)` gates through the shared `Finite` slot and returns the `CoverageKind.Interpolates`-selected `CoverageSample`; `CellAt(x, y, key)` floors-and-bounds-checks the finite inverse into the containing in-bounds discrete cell (`None` outside); `CellCenter(col, row)` projects a cell's centre world point as the seam-neutral `Vector3`; `Shade(index, raw)` reads the band legend under the same `Interpolates` policy; `LevelFor(targetResolution, key)` resolves the coarsest level still finer-or-equal than a finite positive target off the ADMITTED chain order (total — the base is the floor); `Window(x0, y0, x1, y1, level, key)` gates finiteness and ordering, then clips a world rect onto the CHOSEN level's OWN lattice (`None` off-coverage) folding the corner envelope through the one `MeasureBand.Envelope` owner; `OverviewLevel.TileOf(col, row)` resolves the GDAL block window a windowed fetch aligns to; `BandAt(index)` resolves a band descriptor; `ByteLength(level)` sizes the uncompressed fetch; `CanonicalBytes(writer)` projects the coverage's content through the kernel writer's `Doubles`/`Rows`/`Optional` composers.
- Auto: the level run is proven the base's `Coarsen` CHAIN — one fold threads the expected lattice (head, then each successor) and the slot carries the kernel's own refusal on the failing step, so every level's world affine is DERIVED from the base, a level can never drift its origin or rotation, its ordinal is its position, and `Window`/`TileOf` read the level's own lattice with no base-relative ratio anywhere; `LevelFor` reads that same admitted order — resolutions strictly coarsen along the run, so "coarsest still finer than target" is one fold over stored order with the base as floor, and no extremum machinery re-derives what admission proved; `Sample` reads the `Kind.Interpolates` policy column and yields `CoverageSample.Fraction` or `CoverageSample.Cell`, both carrying the root `Inside`; the per-band `SampleType.Width` sizes a fetch, `CoverageBand.Real(raw)` applies the `Offset`/`Scale` decode, `IsNoData(raw)` tests the optional sentinel (NaN-safe via `double.Equals`), `Decode(raw)` resolves a `Palette` band's raw index to its legend entry, and `Shade` answers the exact bin's colour for a `Raster` and the perceptual `Blend` between bracketing bins for a `Field`; `TileOf` floors a cell to its containing tile through the optional `Block` (an untiled level reads as one full-width row band, an out-of-bounds cell `None`).
- Receipt: the `CoverageGrid` is the gridded-field evidence a site-context or environmental consumer reads — the kind, the admitted level run, and the typed band schema in the node, the heavy raster in the content-keyed blob store addressed per-level by `BlobKey`; a `Rasm.Compute` environmental route resolves an element's placement to a sample (`Sample`), picks a working level (`LevelFor`), clips the site region onto that level's cell window (`Window`), sizes the fetch (`ByteLength`), reads that level's bytes by `BlobKey`, and decodes through `Real`/`Shade`/`Range` — the seam delivering the full sampling schema, the consumer never re-deriving the placement affine, the pyramid selection, or the GDAL band/legend surface.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<string>]` + `[Union]`), LanguageExt.Core (`Seq`/`Fin`/`Option`/`Validation`), Generator.Equals (`[Equatable]` structural equality + member diff the `Graph/element#ELEMENT_GRAPH` snapshot drills into), `Rasm` (`Rasm/Domain/validation#ADMISSION_SLOTS`, `Numerics.CellLattice`, `Numerics.PerceptualColor`/`UnitInterval`, `Drawing.ChannelDtype`, `Op`), `Properties/quantity#MEASURE_ALGEBRA` (`MeasureBand.Envelope`), `Projection/address#CONTENT_ADDRESS` (`BlobKey`, `CanonicalWriter`), `Geospatial/reference#GEO_REFERENCE` (`GeoReference`).
- Growth: a new pixel storage type is one kernel `ChannelDtype` row; a new display channel is one `BandRole` row; a new band attribute is one `CoverageBand` column written into its canonical arm; a new resolution tier is one `OverviewLevel` row appended to the `Coarsen` chain; a new sampling policy is one `CoverageKind` column the `Sample` and `Shade` dispatches read; a new fetch-alignment query composes the one `TileOf` body; a temporal axis returns as one level-run-shaped slice run WITH its producer and reader in the same pass (the deleted `TimeSlice` estate is the precedent: declared axes with no producer do not ride); never a per-raster-format coverage type, never a package-local storage-type roster beside the kernel's, never an inlined pixel buffer, never a sidecar palette/legend table beside the band, and never an irregular point-cloud/TIN coverage on this grid owner — a scattered survey, borehole, or sensor set BECOMES a coverage upstream through the kernel `Spatial/fields#SCALAR_FIELD` `ScalarField.SibsonCase` evaluator sampled onto a `CellLattice`.
- Boundary: no host type crosses this seam — the kernel `CellLattice` publishes its placement as neutral doubles (`Affine`/`Inverse` twelve row-major coefficients each), so `LatticeGeodesy` composes arithmetic and never a host type, `CentreOf` answering the seam-owned `Graph/element` `Vector3` and `Fractional` a bare `(Column, Row)` pair — `LatticeGeodesy` OWNS the half-cell centring and the column-norm extent over those neutral coefficients, the one place either is spelled; no member on this page names `Point3d`/`Vector3d`/`Transform` or opens `Rhino.Geometry`. `CoverageGrid` holds the bytes BY REFERENCE — the per-level `BlobKey` addresses the raster in the same seed-zero store the geometry uses, and an inlined pixel buffer, a host raster handle, or a second hasher on the seam is the named defect; the placement is the kernel `CellLattice` and nothing else, so a package-local geotransform record, an axis-aligned-only descriptor, or a forward-only map with no inverse is the deleted form, and a re-doubting placement check here is a second admission authority beside the kernel's own. The base IS the level run's head — a base-beside-the-pyramid column pair re-derives the level shape it already owns, and a source pyramid whose factors are not successive halvings normalizes at the `Rasm.Bim` projector (which re-decimates onto the chain) rather than diverging the level affine from the base. Sampling is ONE policy-driven `Sample` and legend reading ONE policy-driven `Shade`, so a consumer hand-branching `Fractional`-vs-`CellAt` or lerping display bytes per call is the deleted form; a region read is the ONE `Window` projection; a band is typed AND fully self-describing, gated at ITS OWN construction, so a `string` data type, a sentinel-double nodata, a raw-undecoded band, or a `Palette` role with no colour table behind it is unrepresentable; a legend colour is an admitted `PerceptualColor` and the display-byte quadruple exists only inside `CanonicalBytes` (through the kernel's ONE `ToRgb` quantizer, condition-free by kernel law, so the content key stays byte-stable cross-runtime); vector features ride the `Object` node, so a parallel `Feature` family on the seam is the deleted form; the `GeoReference` CRS rides the coverage (and the `Header`), so a coverage carries its own georeference for a multi-CRS site context; `CanonicalBytes` is the coverage's only content projection, and a per-coverage ad-hoc serialization is the named defect.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
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

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class CoverageKind {
 public static readonly CoverageKind Raster = new("raster", interpolates: false); // discrete cells (orthophoto, land cover)
 public static readonly CoverageKind Field = new("field", interpolates: true);   // continuous phenomenon (elevation, irradiance)

 // The sampling policy Sample and Shade both read: a Field interpolates (fractional inverse, perceptual blend), a
 // Raster contains (floored cell, exact bin) — a column on the row, never a caller-side branch.
 public bool Interpolates { get; }
}

// LatticeGeodesy reads an admitted kernel lattice geospatially. The kernel publishes the placement HOST-NEUTRALLY —
// `Affine`/`Inverse` twelve row-major 3×4 coefficients each (the omitted fourth row is the invariant [0 0 0 1]) —
// so this block OWNS the half-cell centring and the column-norm arithmetic over those neutral doubles and answers
// the seam-owned Vector3 / bare (Column, Row) pair with no host type in scope. Both coefficient runs are the
// kernel's own ImmutableArray<double> stores; every read spells .AsSpan() so the projection stays zero-copy.
public static class LatticeGeodesy {
 extension(CellLattice lattice) {
  // The one comparable ground-resolution scalar LevelFor ranks against — the cell measure at its rank root, so a
  // rotated planar lattice and a layered one both answer one magnitude.
  public double Resolution => lattice.Rank is 2 ? Math.Sqrt(lattice.CellMeasure) : Math.Cbrt(lattice.CellMeasure);

  public Vector3 CentreOf(int column, int row, int layer = 0) =>
   Placed(lattice.Affine.AsSpan(), column + 0.5, row + 0.5, lattice.Rank is 3 ? layer + 0.5 : 0.0);

  // Invertibility is admission evidence (CellLattice.Of gated it), so this read is total.
  public (double Column, double Row) Fractional(double x, double y) =>
   Placed(lattice.Inverse.AsSpan(), x, y, 0.0) switch { var local => (local.X, local.Y) };

  public void CanonicalBytes(CanonicalWriter w) =>
   w.Doubles(lattice.Affine.AsSpan())
    .Ordinal(lattice.Columns.Value).Ordinal(lattice.Rows.Value).Ordinal(lattice.Layers.Value);
 }

 static Vector3 Placed(ReadOnlySpan<double> a, double x, double y, double z) =>
  new(a[0] * x + a[1] * y + a[2] * z + a[3],
      a[4] * x + a[5] * y + a[6] * z + a[7],
      a[8] * x + a[9] * y + a[10] * z + a[11]);
}

// The GDAL ColorInterp set reduced to the roles a consumer reads; the Rasm.Bim projector maps the full enum
// (GCI_HueBand/GCI_CyanBand/GCI_YCbCr_*) onto these via the generated TryGet, defaulting Undefined.
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

// One palette legend entry — the GDAL ColorTable quad PLUS the RasterAttributeTable category label — lowered onto
// the seam. The colour is an ADMITTED kernel PerceptualColor (the Bim projector admits through PerceptualColor.OfRgb
// at the host edge), so the legend gains Mix/Contrast/Difference reach a display-byte quad could not carry.
[Equatable]
public readonly partial record struct ColorBin(int Index, PerceptualColor Colour, string Category = "") {
 // Content keys on the display-byte quadruple through the kernel's ONE quantizer — condition-free by kernel law
 // (viewing conditions seat on BlendPath/DeltaMetric payloads, never ToRgb), so the key cannot fork across runtimes
 // adapting under different surrounds. Perceptual reach stays on the DISPLAY axis, never this projection.
 public void CanonicalBytes(CanonicalWriter w) {
  (byte r, byte g, byte b, byte a) = Colour.ToRgb();
  w.Ordinal(Index).Ordinal(r).Ordinal(g).Ordinal(b).Ordinal(a).String(Category);
 }
}

// One pyramid level — the run's HEAD is the full-resolution base, so base and overview share one row shape, one
// blob-key column, and one tiling body. The level carries its OWN CellLattice (census, cell size, and world affine
// one admitted value) and Of proves the run is the head's Coarsen chain, so a level's ordinal IS its position and
// its affine can never drift from the base. Block is the GDAL tile size; ABSENCE is untiled (no zero sentinel).
[Equatable]
public readonly partial record struct OverviewLevel(CellLattice Grid, BlobKey RasterKey, Option<(int X, int Y)> Block = default) {
 // TileOf resolves the GDAL GetBlockSize tile window a windowed fetch ALIGNS to: the containing tile column/row and
 // its bounds-clipped cell-extent (an edge tile is partial). An untiled level is one full-width row band. None for
 // an out-of-bounds cell.
 public Option<(int TileCol, int TileRow, int OriginCol, int OriginRow, int SpanCol, int SpanRow)> TileOf(int col, int row) {
  int width = Grid.Columns.Value, height = Grid.Rows.Value;
  if (!Grid.Contains(col, row)) { return None; }
  (int bx, int by) = Block.IfNone((width, 1));
  int tileCol = col / bx, tileRow = row / by, originCol = tileCol * bx, originRow = tileRow * by;
  return Some((tileCol, tileRow, originCol, originRow, Math.Min(bx, width - originCol), Math.Min(by, height - originRow)));
 }

 public void CanonicalBytes(CanonicalWriter w) {
  Grid.CanonicalBytes(w);
  w.U128(RasterKey.Value).Optional(Block, static (block, wr) => wr.Ordinal(block.X).Ordinal(block.Y));
 }
}

// One Sample result — the discrete-cell-or-continuous-fraction discriminant the CoverageKind.Interpolates policy
// column selects. Inside rides the ROOT: one bounds column, two payload cases, so neither case re-declares it.
// RECORD union (transient read result — see the [02] Exemption card); the private root ctor closes the family.
[Union]
public abstract partial record CoverageSample {
 private CoverageSample(bool inside) { Inside = inside; }

 public bool Inside { get; }

 public sealed record Fraction(double Col, double Row, bool Inside) : CoverageSample(Inside);
 public sealed record Cell(int Col, int Row, bool Inside) : CoverageSample(Inside);
}

// Construction-gated band schema: Of is the ONLY admission, so a non-finite decode, a degenerate range, or a hollow
// or colliding palette is UNREPRESENTABLE and the grid's own gates never re-doubt a row. The legend normalizes to
// index order AT INTAKE, so Bracket, CanonicalBytes, and structural equality all read one stored regime.
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

 // NoData stays UNGATED: a NaN/±∞ sentinel is legal GDAL float nodata the NaN-safe IsNoData exists to match.
 public static Fin<CoverageBand> Of(
  int index, string name, ChannelDtype sampleType, BandRole role, Op key,
  Option<double> noData = default, string units = "", double offset = 0.0, double scale = 1.0,
  Option<(double Min, double Max)> range = default, Seq<ColorBin> palette = default) =>
  Accumulate(Seq(
    Finite(key, ($"coverage-band[{index}].offset", offset), ($"coverage-band[{index}].scale", scale)),
    AdmittedRange(range, index, key),
    Gate(role != BandRole.Palette || !palette.IsEmpty, key, $"<coverage-band-palette-empty:{index}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(palette.Map(static c => c.Index).Distinct().Count == palette.Count, key, $"<coverage-band-palette-index-duplicate:{index}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))))
   .Map(_ => new CoverageBand(index, name, sampleType, role, noData, units, offset, scale, range,
   toSeq(palette.OrderBy(static c => c.Index)).Strict()))
   .ToFin();

 private static Validation<Error, Unit> AdmittedRange(Option<(double Min, double Max)> range, int index, Op key) =>
  range.Match(
   Some: bounds => (Finite(key, ($"coverage-band[{index}].range.min", bounds.Min), ($"coverage-band[{index}].range.max", bounds.Max)),
    Gate(bounds.Min <= bounds.Max, key, $"<coverage-band-range-inverted:{index}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)))
    .Apply(static (_, _) => unit).As(),
   None: () => Success<Error, Unit>(unit));

 // The linear decode a scaled-integer band carries (GDAL GetOffset/GetScale): a UInt16 DEM at scale 0.01 reads in
 // metres without the consumer hand-applying it.
 public double Real(double raw) => Offset + (Scale * raw);

 // NaN-safe via double.Equals — a NaN sentinel matches a NaN cell, which == misses.
 public bool IsNoData(double raw) => NoData is { IsSome: true, Case: double noData } && raw.Equals(noData);

 // Resolve a Palette band's raw cell value to its legend entry; None when the band carries no palette or the index
 // is not in the legend — the consumer rails its own missing-legend handling rather than defaulting a colour.
 public Option<ColorBin> Decode(double raw) => Palette.Find(b => b.Index == (int)Math.Floor(raw));

 // Greatest bin at-or-below and least strictly above, one pass over the admitted index order.
 private (Option<ColorBin> Lower, Option<ColorBin> Upper) Bracket(double raw) =>
  Palette.Fold((Lower: Option<ColorBin>.None, Upper: Option<ColorBin>.None), (best, bin) =>
   bin.Index <= raw ? (Some(bin), best.Upper) : (best.Lower, best.Upper.IsSome ? best.Upper : Some(bin)));

 // Perceptual interpolation between bracketing bins for a continuous Field — a channel-wise byte lerp travels the
 // sRGB diagonal and bands/shifts hue. Beyond the terminal bins the terminal colour holds (a legend states no
 // extrapolation); bins are index-distinct at admission, so the unit quotient is total.
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

// --- [MODELS] -----------------------------------------------------------------------------
[Equatable]
public sealed partial record CoverageGrid {
 public CoverageKind Kind { get; }
 // ORDERED runs both: Levels' position IS the pyramid ordinal under the Coarsen-chain law, Bands normalize to
 // index order at intake — stored order, structural equality, and CanonicalBytes read ONE regime.
 [property: OrderedEquality] public Seq<OverviewLevel> Levels { get; }
 [property: OrderedEquality] public Seq<CoverageBand> Bands { get; }
 public GeoReference Crs { get; }

 // One-hop base reads: the run's head IS the full-resolution base (non-empty by admission, so the index is total).
 public OverviewLevel Base => Levels[0];
 public CellLattice Grid => Base.Grid;
 public BlobKey RasterKey => Base.RasterKey;

 // PRIVATE ctor + GET-ONLY members: Of is the ONLY public admission, so an off-chain pyramid or a malformed band
 // set is UNREPRESENTABLE; a wire or persistence decoder re-admits through the SAME railed Of (the
 // ContentAddress.Verify distrust posture).
 private CoverageGrid(CoverageKind kind, Seq<OverviewLevel> levels, Seq<CoverageBand> bands, GeoReference crs) =>
  (Kind, Levels, Bands, Crs) = (kind, levels, bands, crs);

 // Independent invariants ACCUMULATE through the shared ADMISSION_SLOTS algebra; band-local invariants already
 // proved at CoverageBand.Of never re-run here. The Coarsens slot carries the KERNEL's own Coarsen refusal — a
 // boolean gate swallowed it. The lattice arrives ADMITTED (CellLattice.Of gated invertibility and budget), so
 // placement degeneracy is unrepresentable rather than gated.
 public static Fin<CoverageGrid> Of(
  CoverageKind kind, Seq<OverviewLevel> levels, Seq<CoverageBand> bands, GeoReference crs, Op key) =>
  Accumulate(Seq(
    Gate(!levels.IsEmpty, key, "<coverage-levels-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    AdmittedBlocks(levels, key),
    Coarsens(levels, key),
    Gate(!bands.IsEmpty, key, "<coverage-bands-empty>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Gate(bands.Map(static b => b.Index).Distinct().Count == bands.Count, key, "<coverage-band-index-duplicate>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))))
   .Map(_ => new CoverageGrid(kind, levels, toSeq(bands.OrderBy(static b => b.Index)).Strict(), crs))
   .ToFin();

 private static Validation<Error, Unit> AdmittedBlocks(Seq<OverviewLevel> levels, Op key) =>
  Accumulate(levels.Map((level, index) => level.Block.Match(
   Some: block => (In(block.X, Band.Positive, $"coverage-level[{index}].block.x", key),
    In(block.Y, Band.Positive, $"coverage-level[{index}].block.y", key)).Apply(static (_, _) => unit).As(),
   None: () => Success<Error, Unit>(unit))).Strict());

 // Each level is its predecessor coarsened EXACTLY once — the adjacent-pair form of the chain proof, accumulating,
 // so every off-chain step reports (a threaded fold stopped at the first). The slot carries the KERNEL's own
 // Coarsen refusal where coarsening itself refuses, the chain token where a level's lattice diverges. This ONE
 // proof subsumes the coarser-than-base, cell-monotone, and ordering gates a descriptor-less level set needed.
 private static Validation<Error, Unit> Coarsens(Seq<OverviewLevel> levels, Op key) =>
  Accumulate(levels.Zip(levels.Tail).Map(pair =>
   pair.Item1.Grid.Coarsen(key).Match(
    Succ: next => Gate(next == pair.Item2.Grid, key, "<coverage-level-off-coarsen-chain>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
    Fail: refusal => Gate(false, refusal))).Strict());

 public Option<CoverageBand> BandAt(int index) => Bands.Find(b => b.Index == index);

 // The ONE policy-driven legend read, the colour dual of Sample: the SAME Interpolates column that picks a
 // fractional or discrete sample picks an interpolated or exact colour. None for a band without a legend or a raw
 // value outside it. A caller lerping display bytes, or branching on Kind itself, is the deleted form.
 public Option<PerceptualColor> Shade(int index, double raw) =>
  BandAt(index).Bind(band => Kind.Interpolates ? band.Blend(raw) : band.Decode(raw).Map(static bin => bin.Colour));

 // The ONE policy-driven read: a Field yields the raw fractional inverse, a Raster the FLOORED containing cell.
 // Floor (not Round) is the GDAL pixel-containment idiom — cell (c,r) spans [c,c+1)×[r,r+1).
 public Fin<CoverageSample> Sample(double x, double y, Op key) =>
  Finite(key, ("coverage-sample-x", x), ("coverage-sample-y", y))
   .Map(_ => SampleAdmitted(x, y)).ToFin();

 internal CoverageSample SampleAdmitted(double x, double y) {
  (double col, double row) = Grid.Fractional(x, y);
  (int c, int r, bool inside) = Discrete(col, row);
  return Kind.Interpolates
   ? new CoverageSample.Fraction(col, row, inside)
   : new CoverageSample.Cell(c, r, inside);
 }

 // The explicit-discrete projection a caller takes when it wants a cell regardless of Kind; None outside.
 public Fin<Option<(int Col, int Row)>> CellAt(double x, double y, Op key) =>
  Finite(key, ("coverage-cell-x", x), ("coverage-cell-y", y))
   .Map(_ => CellAtAdmitted(x, y)).ToFin();

 internal Option<(int Col, int Row)> CellAtAdmitted(double x, double y) {
  (double col, double row) = Grid.Fractional(x, y);
  return Discrete(col, row) is (int c, int r, true) ? Some((c, r)) : None;
 }

 // The ONE floor-and-bound containment rule Sample and CellAt both compose.
 private (int Col, int Row, bool Inside) Discrete(double col, double row) {
  int c = (int)Math.Floor(col), r = (int)Math.Floor(row);
  return (c, r, Grid.Contains(c, r));
 }

 // A discrete cell's CENTRE world point — LatticeGeodesy owns the half-cell offset over the neutral affine.
 public Vector3 CellCenter(int col, int row) => Grid.CentreOf(col, row);

 // The coarsest level whose resolution still resolves the target — TOTAL over the admitted chain: resolutions
 // strictly coarsen along the run, so one fold over stored order answers, the base the floor when no overview is
 // fine enough or the target is finer than the base. No extremum machinery re-derives what admission proved.
 public Fin<OverviewLevel> LevelFor(double targetResolution, Op key) =>
  In(targetResolution, Band.Positive, "coverage-target-resolution", key)
   .Map(resolution => Levels.Fold(Base, (best, level) => level.Grid.Resolution <= resolution ? level : best))
   .ToFin();

 // A world-rect read a Rasm.Compute region fetch composes (LevelFor -> Window -> ByteLength -> TileOf): the four
 // corners invert onto the CHOSEN level's OWN lattice (rotation-exact — an axis-aligned world rect is a cell-space
 // parallelogram), the corner envelope folds through the one MeasureBand.Envelope owner per axis, floors to
 // containing cells, clips to that lattice's census; None when the rect misses the coverage. Each level carries its
 // own affine (the Coarsen-chain law), so no base-to-level ratio exists to get wrong.
 public Fin<Option<(int Col, int Row, int SpanCol, int SpanRow)>> Window(
  double x0, double y0, double x1, double y1, OverviewLevel level, Op key) =>
  Accumulate(Seq(
    Finite(key, ("coverage-window-x0", x0), ("coverage-window-y0", y0), ("coverage-window-x1", x1), ("coverage-window-y1", y1)),
    Gate(x0 <= x1 && y0 <= y1, key, $"<coverage-window-reversed:{x0:R}:{y0:R}:{x1:R}:{y1:R}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))))
   .Map(_ => WindowAdmitted(x0, y0, x1, y1, level)).ToFin();

 internal Option<(int Col, int Row, int SpanCol, int SpanRow)> WindowAdmitted(
  double x0, double y0, double x1, double y1, OverviewLevel level) {
  CellLattice lattice = level.Grid;
  (int w, int h) = (lattice.Columns.Value, lattice.Rows.Value);
  ((double cA, double rA), (double cB, double rB), (double cC, double rC), (double cD, double rD)) =
   (lattice.Fractional(x0, y0), lattice.Fractional(x1, y0), lattice.Fractional(x0, y1), lattice.Fractional(x1, y1));
  (double minC, double maxC, _) = MeasureBand.Envelope(cA, cB, cC, cD);
  (double minR, double maxR, _) = MeasureBand.Envelope(rA, rB, rC, rD);
  (int c0, int r0) = (Math.Max(0, (int)Math.Floor(minC)), Math.Max(0, (int)Math.Floor(minR)));
  (int c1, int r1) = (Math.Min(w - 1, (int)Math.Floor(maxC)), Math.Min(h - 1, (int)Math.Floor(maxR)));
  return c1 < c0 || r1 < r0 ? None : Some((c0, r0, c1 - c0 + 1, r1 - r0 + 1));
 }

 // Uncompressed raster bytes across bands at a chosen level (census · per-cell stride) — a consumer sizes a blob
 // fetch from the metadata alone. A complex row's Width already counts both components, so the sum is exact for
 // SAR/InSAR; every band spans the same census, so the stride folds once and the census multiplies once.
 public long ByteLength(OverviewLevel level) =>
  level.Grid.CellCount * Bands.Fold(0L, static (stride, b) => stride + b.SampleType.Width);

 // The Graph/element#NODE_MODEL Node.Coverage arm delegates here: kind, the count-framed level run, the CRS, and
 // each band in stored (index) order — every double through the shared IEEE-754 canon so the content identity is
 // byte-stable across the runtimes sharing the one XxHash128 seed. Twelve coefficients ARE the whole affine, so a
 // rotated or layered placement keys distinctly; two coverages sharing a base raster but differing in pyramid,
 // CRS, palette, or placement address as the distinct coverages they are.
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
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

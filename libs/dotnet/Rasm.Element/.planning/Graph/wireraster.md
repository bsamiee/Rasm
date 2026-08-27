# [ELEMENT_WIRE_RASTER]

`WireCodec`'s coverage plane: the `CoverageGrid` codec rebuilding the base-as-level-0 run from the wire's flat base columns and the overview list (the wire keeps its frozen face; the contract maps at the edge), the `CoverageBand` codec re-admitting through the band's own result-returning `Of` with the palette crossing through the ONE `ToRgb` quantizer the content key shares, the `CellLattice` placement re-admitting through the kernel's own gate, and the `GeoReference` codec re-entering the compound `Admit` (the flattened vertical pair riding the frozen `vertical_datum`/`vertical_epsg` columns).

## [01]-[INDEX]

- [02]-[RASTER_CODEC]: coverage, band, grid, and georeference codecs.

## [02]-[RASTER_CODEC]

- Cases: `CoverageWire`/`GeoReferenceWire` flat payloads; `CoverageSample` stays native and absent from the `Graph/wire#NODE_CODEC` census.
- Law: this page is one partial part of the `Graph/wire#NODE_CODEC` mapper family and composes its shared identity, presence, interval, and optional-value gates.
- Law: every decoded value re-crosses its OWNER's admission gate — the decoder constructs no case directly and trusts no carried invariant (the `ContentAddress.Verify` distrust posture); every optional column crosses by EXPLICIT presence, never a defaulted zero, blank, or sentinel.
- Packages: Google.Protobuf, Mapperly, NodaTime.Serialization.Protobuf, LanguageExt, and Thinktecture compose the generated support closure coordinated at `Graph/wire#NODE_CODEC`.
- Growth: a new column is one append-only corpus field and one transcription member; a new seated union case also updates the owning parity census.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using Google.Protobuf;
using LanguageExt;
using LanguageExt.Common;
// Contracts are retired from this logic.
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Geospatial;
using Rasm.Element.Projection;
using Riok.Mapperly.Abstractions;
using static LanguageExt.Prelude;
using static Rasm.Element.Graph.BoundaryConverters;
using CellLattice = Rasm.Numerics.CellLattice;
using LatticeAxis = Rasm.Numerics.Dimension;
using PerceptualColor = Rasm.Numerics.PerceptualColor;

namespace Rasm.Element.Graph;

// --- [SERVICES] ------------------------------------------------------------------------
internal static partial class WireCodec {
 internal static partial CoverageWire ToWire(CoverageGrid grid);

 [UserMapping] internal static GeoReferenceWire ToWire(GeoReference geo) {
  GeoReferenceWire w = new() {
   Eastings = geo.Eastings, Northings = geo.Northings, OrthogonalHeight = geo.OrthogonalHeight,
   XAxisAbscissa = geo.XAxisAbscissa, XAxisOrdinate = geo.XAxisOrdinate,
   ScaleX = geo.ScaleX, ScaleY = geo.ScaleY, ScaleZ = geo.ScaleZ,
   GeodeticDatum = geo.GeodeticDatum,
  };
  geo.Vertical.IfSome(v => { w.VerticalDatum = v.Name; v.Epsg.IfSome(e => w.VerticalEpsg = e); });
  geo.Crs.IfSome(c => {
   ProjectedCrsWire p = new() { Name = c.Name, Resolution = c.Resolution.Key };
   c.Epsg.IfSome(e => p.Epsg = e);
   if (c.Wkt.Length > 0) { p.Wkt = c.Wkt; }
   if (c.MapProjection.Length > 0) { p.MapProjection = c.MapProjection; }
   if (c.MapZone.Length > 0) { p.MapZone = c.MapZone; }
   w.Crs = p;
  });
  geo.Epoch.IfSome(epoch => w.Epoch = epoch); return w;
 }

 [UserMapping] internal static CoverageBandWire ToWire(CoverageBand band) {
  CoverageBandWire w = new() { Index = band.Index, Name = band.Name, SampleType = band.SampleType.Key, Role = band.Role.Key, Units = band.Units, Offset = band.Offset, Scale = band.Scale };
  band.NoData.IfSome(v => w.NoData = v);
  band.Range.IfSome(r => { w.RangeMin = r.Min; w.RangeMax = r.Max; });
  w.Palette.AddRange(band.Palette.Map(static c => {
   (byte r, byte g, byte b, byte a) = c.Colour.ToRgb();
   return new ColorBinWire { Index = c.Index, R = r, G = g, B = b, A = a, Category = c.Category };
  }));
  return w;
 }

 [UserMapping] internal static CellLatticeWire ToWire(CellLattice grid) {
  CellLatticeWire w = new() { Columns = grid.Columns.Value, Rows = grid.Rows.Value, Layers = grid.Layers.Value, Ceiling = grid.Ceiling };
  w.Affine.AddRange(grid.Affine); return w;
 }

 static Fin<CoverageGrid> ToCoverage(CoverageWire w) =>
  from kind in FactoryBridge.Row<string, CoverageKind>(w.Kind)
  from geo in Present(w.Crs, "coverage.crs")
  from crs in ToGeoReference(geo)
  from bands in toSeq(w.Bands).TraverseM(band => ToBand(band)).As()
  from grid in ToLattice(w.Grid)
  from overviews in toSeq(w.Overviews).TraverseM(overview =>
   from grid in ToLattice(overview.Grid)
   from raster in ToArtifactContent(overview.RasterArtifact, "coverage.overview.raster_artifact")
   select new OverviewLevel(grid, raster, Blocked(overview.BlockX, overview.BlockY))).As()
  from raster in ToArtifactContent(w.RasterArtifact, "coverage.raster_artifact")
  from coverage in CoverageGrid.Of(
   kind,
   new OverviewLevel(grid, raster, Blocked(w.BaseBlockX, w.BaseBlockY)).Cons(overviews),
   bands, crs, key)
  select coverage;

 static Option<(int X, int Y)> Blocked(int x, int y) => x > 0 && y > 0 ? Some((x, y)) : None;

 static Fin<CellLattice> ToLattice(CellLatticeWire? w) =>
  w is { Affine.Count: 12 } wire
   ? from columns in FactoryBridge.Accept<LatticeAxis>(candidate: wire.Columns)
     from rows in FactoryBridge.Accept<LatticeAxis>(candidate: wire.Rows)
     from layers in FactoryBridge.Accept<LatticeAxis>(candidate: wire.Layers)
     from grid in CellLattice.Of([.. wire.Affine], columns, rows, layers, wire.Ceiling)
     select grid
   : new KernelFault.InvalidValue("element-wire.grid.affine", $"carry 12 coefficients; actual={w?.Affine.Count ?? 0}");

 static Fin<CoverageBand> ToBand(CoverageBandWire w) =>
  (FactoryBridge.Row<int, ChannelDtype>(w.SampleType),
   FactoryBridge.Row<string, BandRole>(w.Role))
   .Apply(static (sampleType, role) => (sampleType, role)).As()
   .Bind(t => BothOrNeither(w.HasRangeMin, w.HasRangeMax, "band-range").Bind(_ =>
    !w.Palette.All(static p => (p.R | p.G | p.B | p.A) <= 255u) ? new KernelFault.InvalidValue("element-wire.band.palette", "channels must fit byte range")
    : toSeq(w.Palette).TraverseM(bin => PerceptualColor
       .OfRgb((byte)bin.R, (byte)bin.G, (byte)bin.B, alpha: bin.A / 255.0)
       .Map(colour => new ColorBin(bin.Index, colour, bin.Category))).As()
      .Bind(palette => CoverageBand.Of(w.Index, w.Name, t.sampleType, t.role,
       Opt(w.HasNoData, w.NoData), w.Units, w.Offset, w.Scale, Opt(w.HasRangeMin, (w.RangeMin, w.RangeMax)), palette))));

 static Fin<GeoReference> ToGeoReference(GeoReferenceWire w) => GeoReference.Admit(
  w.Eastings, w.Northings, w.OrthogonalHeight,
  w.XAxisAbscissa, w.XAxisOrdinate, w.ScaleX, w.ScaleY, w.ScaleZ,
  w.GeodeticDatum, w.VerticalDatum,
  w.Crs?.Name ?? "", w.Crs?.Wkt ?? "", w.Crs?.MapProjection ?? "", w.Crs?.MapZone ?? "",
  Opt(w.HasEpoch, w.Epoch), Opt(w.HasVerticalEpsg, w.VerticalEpsg));
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

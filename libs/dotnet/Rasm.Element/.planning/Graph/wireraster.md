# [ELEMENT_WIRE_RASTER]

`WireCodec`'s coverage plane: the `CoverageGrid` codec rebuilding the base-as-level-0 run from the wire's flat base columns and the overview list (the wire keeps its frozen face; the seam maps at the edge), the `CoverageBand` codec re-admitting through the band's own railed `Of` with the palette crossing through the ONE `ToRgb` quantizer the content key shares, the `CellLattice` placement re-admitting through the kernel's own gate, and the `GeoReference` codec re-entering the compound `Admit` (the flattened vertical pair riding the frozen `vertical_datum`/`vertical_epsg` columns).

## [01]-[INDEX]

- [02]-[RASTER_CODEC]: coverage, band, lattice, and georeference codecs.

## [02]-[RASTER_CODEC]

- Cases: `CoverageWire`/`GeoReferenceWire` flat payloads; `CoverageSample` stays native and absent from the `Graph/wire#NODE_CODEC` census.
- Law: this page is one partial part of the `Graph/wire#NODE_CODEC` mapper family and composes its shared identity, presence, interval, and optional-value gates.
- Law: every decoded value re-crosses its OWNER's admission gate — the decoder constructs no case directly and trusts no carried invariant (the `ContentAddress.Verify` distrust posture); every optional column crosses by EXPLICIT presence, never a defaulted zero, blank, or sentinel.
- Packages: Google.Protobuf, Mapperly, NodaTime.Serialization.Protobuf, LanguageExt, and Thinktecture compose the generated support closure coordinated at `Graph/wire#NODE_CODEC`.
- Growth: a new column is one append-only corpus field and one transcription member; a new seated union case also updates the owning parity census.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using Google.Protobuf;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Contracts.Element;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Element.Geospatial;
using Rasm.Element.Projection;
using Riok.Mapperly.Abstractions;
using static LanguageExt.Prelude;
using static Rasm.Element.Graph.SeamConverters;
using CellLattice = Rasm.Numerics.CellLattice;
using LatticeAxis = Rasm.Numerics.Dimension;
using PerceptualColor = Rasm.Numerics.PerceptualColor;

namespace Rasm.Element.Graph;

// --- [SERVICES] ---------------------------------------------------------------------------
// One partial part of the ONE `[Mapper]` WireCodec family — the attribute, the parity census, the key codecs, and
// the shared decode gates ride `Graph/wire#NODE_CODEC`; this part owns the coverage, lattice, and georeference transcriptions.
internal static partial class WireCodec {
 // The envelope-side generated transcription of the whole coverage descriptor; the level/band internals below
 // own every hand crossing.
 internal static partial CoverageWire ToWire(CoverageGrid grid);

 // Wire epsg/resolution columns are peer-informative derivations; blank ProjectedCrs strings stay unset.
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
  // The legend colour crosses through the SAME ToRgb quantizer CanonicalBytes takes, so the wire quadruple and the
  // content key are one projection — a second quantization here would let two runtimes agree on the key and disagree
  // on the swatch. The decoder re-admits through PerceptualColor.OfRgb, never a stored perceptual triple, because the
  // display quadruple is the only form both the key and every host palette surface already speak. Both calls stay
  // CONDITION-FREE for the same reason coverage#COVERAGE_NODE CanonicalBytes does: the kernel seats a viewing
  // condition on appearance-case payloads and never on ToRgb, and a gamut or observer argument admitted at either
  // end alone splits the wire from the key it is defined to agree with.
  w.Palette.AddRange(band.Palette.Map(static c => {
   (byte r, byte g, byte b, byte a) = c.Colour.ToRgb();
   return new ColorBinWire { Index = c.Index, R = r, G = g, B = b, A = a, Category = c.Category };
  }));
  return w;
 }

 // The kernel placement crosses as its twelve index-to-world coefficients plus the census and ceiling the decoder
 // re-admits with — the fourth matrix row is the invariant [0 0 0 1] and carries no information, so twelve IS the
 // whole affine and a thirteenth column would be a value the receiver already knows. The body stays hand because
 // the source is the KERNEL owner: its axis columns lower through .Value reads and its derived affine surface would
 // demote a generated partial to an ignore-roster inventory over a foreign package's members.
 [UserMapping] internal static CellLatticeWire ToWire(CellLattice lattice) {
  CellLatticeWire w = new() { Columns = lattice.Columns.Value, Rows = lattice.Rows.Value, Layers = lattice.Layers.Value, Ceiling = lattice.Ceiling };
  w.Affine.AddRange(lattice.Affine); return w;
 }

 static Fin<CoverageGrid> ToCoverage(CoverageWire w, Op key) =>
  from kind in key.Row<string, CoverageKind>(w.Kind)
  from geo in Present(w.Crs, "coverage.crs", key)
  from crs in ToGeoReference(geo, key)
  from bands in toSeq(w.Bands).TraverseM(band => ToBand(band, key)).As()
  from grid in ToLattice(w.Grid, key)
  from overviews in toSeq(w.Overviews).TraverseM(overview =>
   from lattice in ToLattice(overview.Grid, key)
   from raster in ToArtifactContent(overview.RasterArtifact, "coverage.overview.raster_artifact", key)
   select new OverviewLevel(lattice, raster, Blocked(overview.BlockX, overview.BlockY))).As()
  from raster in ToArtifactContent(w.RasterArtifact, "coverage.raster_artifact", key)
  from coverage in CoverageGrid.Of(
   kind,
   new OverviewLevel(grid, raster, Blocked(w.BaseBlockX, w.BaseBlockY)).Cons(overviews),
   bands, crs, key)
  select coverage;

 // The wire keeps zero as its untiled sentinel (frozen sint32 columns); the seam carries absence as absence — the
 // boundary maps once, in both directions, and neither regime leaks into the other.
 static Option<(int X, int Y)> Blocked(int x, int y) => x > 0 && y > 0 ? Some((x, y)) : None;

 // The placement RE-ADMITS through the kernel's own gate rather than crossing as trusted state: a wire whose affine
 // is non-invertible or whose census breaches the ceiling rails here, so a foreign encoder cannot hand this runtime
 // a lattice its own CellLattice.Of would refuse. The arity gate is the wire's, because a repeated field carries no
 // fixed length and a short affine would otherwise index past its own array; the census crosses the SAME rail through
 // AcceptValidated, because the generated Create THROWS on a non-positive axis and a foreign encoder owns that int.
 static Fin<CellLattice> ToLattice(CellLatticeWire? w, Op key) =>
  w is { Affine.Count: 12 } wire
   ? from columns in key.AcceptValidated<LatticeAxis>(candidate: wire.Columns)
     from rows in key.AcceptValidated<LatticeAxis>(candidate: wire.Rows)
     from layers in key.AcceptValidated<LatticeAxis>(candidate: wire.Layers)
     from lattice in CellLattice.Of([.. wire.Affine], columns, rows, layers, wire.Ceiling, key)
     select lattice
   : new KernelFault.InvalidValue("element-wire.lattice.affine", $"carry 12 coefficients; actual={w?.Affine.Count ?? 0}", Some(key));

 // The two token gates are INDEPENDENT and accumulate applicatively; the half-open range and palette-overflow
 // gates then read the proved pair.
 static Fin<CoverageBand> ToBand(CoverageBandWire w, Op key) =>
  (key.Row<int, ChannelDtype>(w.SampleType),
   key.Row<string, BandRole>(w.Role))
   .Apply(static (sampleType, role) => (sampleType, role)).As()
   .Bind(t => BothOrNeither(w.HasRangeMin, w.HasRangeMax, "band-range", key).Bind(_ =>
    !w.Palette.All(static p => (p.R | p.G | p.B | p.A) <= 255u) ? new KernelFault.InvalidValue("element-wire.band.palette", "channels must fit byte range", Some(key))
    : toSeq(w.Palette).TraverseM(bin => PerceptualColor
       .OfRgb((byte)bin.R, (byte)bin.G, (byte)bin.B, alpha: bin.A / 255.0, key: key)
       .Map(colour => new ColorBin(bin.Index, colour, bin.Category))).As()
      // Re-admission through the band's OWN railed Of (the decode distrust posture) — the wire's gates prove the
      // wire columns, the owner's gates prove the band.
      .Bind(palette => CoverageBand.Of(w.Index, w.Name, t.sampleType, t.role, key,
       Opt(w.HasNoData, w.NoData), w.Units, w.Offset, w.Scale, Opt(w.HasRangeMin, (w.RangeMin, w.RangeMax)), palette))));

 // A seam GeoReference is Identity (no CRS) or Admit-resolved (Some CRS) — the wire mirrors the closed pair: an
 // absent crs decodes ONLY to the exact Identity tuple (junk columns rail), a present crs re-admits in full; the
 // wire's derived epsg/resolution columns are peer-informative — the seam re-derives both through Admit.
 static Fin<GeoReference> ToGeoReference(GeoReferenceWire w, Op key) => GeoReference.Admit(
  w.Eastings, w.Northings, w.OrthogonalHeight,
  w.XAxisAbscissa, w.XAxisOrdinate, w.ScaleX, w.ScaleY, w.ScaleZ,
  w.GeodeticDatum, w.VerticalDatum,
  w.Crs?.Name ?? "", w.Crs?.Wkt ?? "", w.Crs?.MapProjection ?? "", w.Crs?.MapZone ?? "", key,
  Opt(w.HasEpoch, w.Epoch), Opt(w.HasVerticalEpsg, w.VerticalEpsg));
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

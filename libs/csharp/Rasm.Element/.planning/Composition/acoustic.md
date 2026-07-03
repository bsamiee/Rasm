# [ELEMENT_ACOUSTIC]

The intrinsic single-material acoustic owner: the `AcousticBand` `[SmartEnum<int>]` ONE-THIRD-OCTAVE centre vocabulary, the banded `Acoustic` carrier holding the per-band `AbsorptionSpectrum` and `SoundReductionIndexDb` vectors over the eighteen `100`-to-`5000` Hz one-third-octave centres PLUS the three material-INTRINSIC `Option<double>` acoustic constants — `DynamicStiffnessMNPerM3` (EN 29052-1 `s′`, the resilient-layer property the EN 12354-2 floating-floor impact improvement `ΔL_w` reads), `FlowResistivityPaSPerM2` (ISO 9053 `σ`, the Delany-Bazley/Miki porous-absorber model input), and `LossFactor` (the small-strain internal `η` the ISO 12354-1 structural-reverberation corrections read) — the `RatingContour` `[SmartEnum<string>]` contour-fit family (`Stc`/`Rw`) whose rows differ only in data and SHARE one `Fit` kernel, the `AbsorptionClass` `[SmartEnum<string>]` ISO 11654 A-E class vocabulary whose αw floors are the row policy column, and the PURE projection folds — `Nrc`/`Saa` (the ASTM C423 absorption averages), `AlphaW`/`AlphaWShape`/`AbsorptionClass` (the ISO 11654 weighted absorption over the derived octave `αp`, its `L`/`M`/`H` shape flags, and the A-E class — the ASTM pair's ISO sibling), `StcWeighted` (ASTM E413) and `Rw` (ISO 717-1) over the shared contour fit, and `C`/`Ctr` (the ISO 717-1 spectrum adaptation terms). The folds are EXPRESSION-BODIED projections over the band carriers, never stored ratings that could drift from the spectrum: a material's NRC, αw, STC, Rw, and adaptation terms are computed from its absorption/sound-reduction vectors on read, so the one source of truth is the banded data. The vocabulary is the standards' MEASUREMENT resolution — ASTM E413 and ISO 717-1 are defined over the sixteen one-third-octave bands `125`-`4000` Hz and `100`-`3150` Hz, so a six-octave-band rating is the DELETED approximation, not ASTM E413; NRC reads the four octave-coincident bands (`250`/`500`/`1k`/`2k`), SAA the twelve bands `200`-`2500`, and the ISO 11654 octave `αp` the contiguous third-octave triplets `200`-`5000` directly off the same one-third-octave vector — the `5000` Hz row carried for the `4000`-octave triplet (ISO 354 measures the full `100`-`5000` span). This owner is the seam home the migration source's `Rasm.Materials` acoustic case relocates to so the rating algebra sits at the lowest stratum, and the `RatingContour` `Fit` kernel is PUBLIC so the `Rasm.Compute` ISO 12354 layered-assembly fold feeds its per-band layered sound-reduction vector through the SAME contour fit the per-material rating uses — one contour-fit owner, never a second STC/Rw algorithm, and the impact rating (IIC ASTM E989 / Ln,w ISO 717-2) lands as ONE more `RatingContour` row when its assembly spectrum is carried in `Rasm.Compute` — the `DynamicStiffnessMNPerM3` carrier member is that row's UNBLOCKING material input, the `Rasm.Compute` floor fold deriving the floating-floor `ΔL_w` from `s′` and the floating slab's areal mass before the impact spectrum reaches the shared `Fit`. The page composes the `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet.Acoustic` case (which wraps the `Acoustic` carrier and forwards its reads) and the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter` (the `Acoustic` spectra contribute to the `Material` node's content hash), and rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` on a non-finite sound-reduction band or an out-of-`[0,1]` absorption band. The page is HOST-NEUTRAL and PURE — no geometry, no units coercion (the bands are dimensionless ratios and decibels, not dimensioned `MeasureValue`s), no external acoustics library (only the universal Thinktecture / LanguageExt / Generator.Equals substrate the whole seam rides).

## [01]-[INDEX]

- [01]-[ACOUSTIC_FOLDS]: the `AcousticBand` one-third-octave vocabulary, the banded `Acoustic` carrier (the two spectra plus the `DynamicStiffnessMNPerM3`/`FlowResistivityPaSPerM2`/`LossFactor` intrinsic constants) and its `Of` admission, the `RatingContour` `[SmartEnum<string>]` contour-fit family sharing one `Fit` kernel, the `AbsorptionClass` ISO 11654 A-E vocabulary, the `Nrc`/`Saa`/`AlphaW`/`AlphaWShape`/`StcWeighted`/`Rw`/`C`/`Ctr` projection folds, and the `CanonicalBytes` content contribution.

## [02]-[ACOUSTIC_FOLDS]

- Owner: `AcousticBand` the `[SmartEnum<int>]` eighteen-row one-third-octave-centre vocabulary keyed on the band index, each carrying its centre frequency; `RatingContour` the `[SmartEnum<string>]` contour-fit family (`Stc`/`Rw`), each row carrying its reference contour, its first-band index, its single-band deficiency cap, its slide-sense (`Ascending`) and its `RatingOffset`, all sharing the `DeficitBudget` and the one `Fit` kernel; `AbsorptionClass` the `[SmartEnum<string>]` ISO 11654 class vocabulary (`A`-`E` plus `Unclassified`), each row carrying its αw `Floor` policy column the one `Of` resolver reads; `Acoustic` the `[Equatable]` banded carrier holding the fixed-length eighteen-band `AbsorptionSpectrum` and `SoundReductionIndexDb` `[OrderedEquality]` `ImmutableArray<double>` vectors, the three material-intrinsic `Option<double>` constants (`DynamicStiffnessMNPerM3` the EN 29052-1 dynamic stiffness per unit area `s′`, `FlowResistivityPaSPerM2` the ISO 9053 airflow resistivity `σ`, `LossFactor` the small-strain internal loss factor `η` — each an atomic equality leaf the `[Equatable]` drill compares by `Option<double>` value), the `Nrc`/`Saa`/`AlphaW`/`AlphaWShape`/`AbsorptionClass`/`StcWeighted`/`Rw`/`C`/`Ctr` projection folds, and the `CanonicalBytes` projection.
- Entry: `Acoustic.Of(absorption, sri, key, dynamicStiffness, flowResistivity, lossFactor)` admits the two eighteen-band vectors AND the three optional intrinsic constants once at construction — each absorption band in `[0,1]` (a relational pattern that also rejects a non-finite band), each sound-reduction band finite, both lengths equal to the eighteen `AcousticBand` centres, each `Some`-carried constant finite and strictly positive (the trailing `Option<double>` parameters default `None`, so a spectra-only datasheet calls the three-argument form unchanged) — `Fin<T>` railing `ElementFault.ValueRejected` on a band-arity mismatch, an out-of-unit absorption, a non-finite sound-reduction, or a non-positive intrinsic constant; `At(band)`/`SriAt(band)` read a band by its `AcousticBand` row; `Nrc`/`Saa`/`AlphaW`/`AlphaWShape`/`AbsorptionClass`/`StcWeighted`/`Rw`/`C`/`Ctr` are the expression-bodied projection folds; `RatingContour.Stc.Fit(sri)`/`Rw.Fit(sri)` is the static single-number contour fit the `Rasm.Compute` assembly layered fold shares; `CanonicalBytes(writer)` contributes the spectra to the node content hash.
- Auto: `Nrc` averages the four octave-coincident bands (`250`/`500`/`1k`/`2k`, the `NrcBands` index span) and rounds to the nearest `0.05` through `RoundTo` (ASTM C423); `Saa` averages the twelve bands `200`-`2500` (the `SaaBands` span) and rounds to the nearest `0.01` (ASTM C423); `AlphaW` runs the ISO 11654 slide in INTEGER `0.05` ticks — the octave `αp` is each contiguous third-octave triplet mean from the `200` Hz band rounded to `0.05` and capped at `1.00`, the `AlphaWReferenceTicks` contour (`0.80`/`1.00`/`1.00`/`1.00`/`0.90`) slides down until the summed unfavourable deviations fit the `0.10` (two-tick) budget, αw is the slid `500` Hz value, `AlphaWShape` flags an octave exceeding the slid contour by `0.25` (`L` `250`, `M` `500`/`1k`, `H` `2k`/`4k`), and `AbsorptionClass.Of(αw)` resolves the A-E row off the `Floor` column; `StcWeighted` and `Rw` run the SHARED `RatingContour.Fit` — the contour slides from a spectrum-derived ceiling until the summed unfavourable deviations are within `DeficitBudget` (`32` dB) and (for `Stc`) no single-band deviation exceeds its `8` dB cap, `RatingOffset + the highest passing shift` the rating (the airborne rows are `Ascending` with offset `0`, so the rating IS the shift at `500` Hz); `C`/`Ctr` evaluate the ISO 717-1 adaptation `XAj − Rw` over the `SpectrumNo1`/`SpectrumNo2` reference spectra; the folds read the band spans directly so a rating never drifts from the spectrum.
- Receipt: the `Acoustic` carrier is the acoustic evidence the `MaterialPropertySet.Acoustic` case wraps and the `Bake`-derived `Element` reads flat; `StcWeighted`/`Rw`/`Nrc`/`Saa`/`AlphaW`/`AlphaWShape`/`AbsorptionClass`/`C`/`Ctr` are derived reads, never stored, so editing one band re-derives every rating (a finish schedule reads `a.AbsorptionClass` and `a.AlphaWShape` beside `a.Nrc` off one carrier); the `CanonicalBytes` projection writes both spectra AND the three intrinsic constants (each `Bool`-prefixed, so `None` and `Some(v)` address distinctly) through the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter` so the `Material` node's content address covers the acoustic data — editing a band or an intrinsic constant changes the node identity AND every rating in lockstep; the `RatingContour` family is the shared single-number owner the `Rasm.Compute` ISO 12354 layered fold feeds its per-band layered sound-reduction vector through, so the assembly STC/Rw and the material STC/Rw share one contour fit.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<int>]`/`[SmartEnum<string>]`), LanguageExt.Core (`Fin` the admission rail, `Option<double>` the intrinsic-constant absence carrier), Generator.Equals (`[Equatable]`/`[OrderedEquality]` the band-wise structural diff the `Acoustic` carrier carries so the `Node.Material` merge drills into it), `Rasm.Element` (the `CanonicalWriter` codec), `Rasm` (the kernel `Op` op-key), BCL inbox (`ImmutableArray<double>` the stored content-equality spectra, `ReadOnlyMemory<double>` the `Of` admission shape, `ReadOnlySpan<double>` the `Fit` transient input, `Math`).
- Growth: a new octave/one-third-octave band is one `AcousticBand` row (the band carriers widen by data and the folds re-read the new index); a new AIRBORNE contour-fit rating is ONE `RatingContour` row carrying its contour, first-band index, deficiency cap, `Ascending: true`, and `RatingOffset: 0` — STC and Rw collapse to the one slide `Fit` by ROW DATA ALONE (no kernel code); the impact rating (IIC ASTM E989 / Ln,w ISO 717-2) is the DESCENDING sibling the kernel ALREADY admits by data — one `RatingContour` row carrying its contour, `Ascending: false` (the deviation inverts to spectrum-above-contour, both spectrum-derived scan bounds following the sense — the row's feasible shifts are NEGATIVE, which a zero scan floor would silently miss), and `RatingOffset: 110` (IIC `= 110 + shift`), landing the moment `Rasm.Compute` carries the assembly normalized-impact spectrum (a single MATERIAL has no impact spectrum — impact is a floor-assembly property; the carrier's `DynamicStiffnessMNPerM3` is the resilient-layer input that assembly-side `ΔL_w` fold reads, so the impact chain's material leg is ALREADY carried here), so the row is pure data with no kernel edit, never a parallel STC/Rw/impact method triplet; a new absorption average is one expression-bodied fold over a band-index span and a `RoundTo` step; a new absorption-class boundary is one `AbsorptionClass` row carrying its αw `Floor` (the resolver reads the column, never a threshold chain); a new material-intrinsic acoustic constant is one `Option<double>` member + one `NonPositive` admission arm + one `Scalar` canon write; the bands grow by data, the airborne and impact ratings by row, the classes by row, the averages by fold, the constants by member — never a stored scalar RATING column and never a second contour algorithm.
- Boundary: `Acoustic` is a BANDED spectrum NOT a scalar — the `AbsorptionSpectrum` and `SoundReductionIndexDb` are fixed-length eighteen-band `[OrderedEquality]` `ImmutableArray<double>` vectors over the `AcousticBand` one-third-octave centres (NOT `ReadOnlyMemory<double>` members — the carrier is the `MaterialPropertySet.Acoustic` case's `Spectrum`, and the `Node.Material` `Generator.Equals` diff drills into a nested value only when it is `[Equatable]`, so a plain-record / `ReadOnlyMemory<double>` carrier would take REFERENCE equality and be an opaque whole-spectrum diff leaf — the deleted form per the `Graph/element#NODE_MODEL` `[STRUCTURAL_EQUALITY]` mandate; the `ImmutableArray<double>` is `IEnumerable<double>` so `[OrderedEquality]` gives band-wise content equality aligned with the order-sensitive content key), and a single-number absorption or STC field is the deleted form; the one-third-octave resolution is load-bearing — ASTM E413 and ISO 717-1 are defined over one-third-octave bands, so a six-octave-band STC is the deleted approximation that yields a different number than the standard, and the migration source's octave-band carrier is the rebuilt form's deleted predecessor; `StcWeighted`/`Rw`/`Nrc`/`Saa`/`AlphaW`/`C`/`Ctr` are expression-bodied projection folds over the carriers, never stored ratings that drift from the spectrum; `AlphaW` derives its octave `αp` from the stored one-third-octave vector and slides in INTEGER `0.05` ticks (the two-tick `0.10` budget and five-tick `0.25` shape excess are exact integer tests, never accumulated-double comparisons), the `AbsorptionClass` thresholds are `Floor` ROW DATA on the vocabulary (a relational-pattern chain restating the A-E table is the deleted form), and the ISO pair (`AlphaW`/`AbsorptionClass`) COMPLEMENTS the ASTM pair (`Nrc`/`Saa`) — the dual-standard law `StcWeighted`/`Rw` already hold — never a replacement; `StcWeighted` and `Rw` are ONE contour-slide algorithm differing only in the `RatingContour` row data (the `Rw` curve adds the `100` Hz band and drops the `8` dB cap), so a per-rating contour method is the deleted form; the `RatingContour.Fit` kernel is the ONE contour-fit owner — the `Rasm.Compute` assembly layered fold feeds its computed per-band layered sound-reduction vector through this same kernel, a second STC/Rw algorithm being the named defect, and the assembly aggregation (the mass-law layered spectrum) lives in `Rasm.Compute`, this owner contributing only the contour family and the per-material banded carrier; an out-of-`[0,1]` absorption band, a non-finite sound-reduction band, or a non-positive `Some`-carried intrinsic constant rails `ElementFault.ValueRejected` at `Of`, never a clamped sentinel, and an unmeasured constant is `None` — never a `0.0` sentinel the impact fold would read as a real stiffness; the intrinsic constants are `Option<double>` with the declared unit in the NAME (`MNPerM3`, `PaSPerM2`), NOT `MeasureValue` — `s′` (MN/m³) and `σ` (Pa·s/m², the rayl/m) are declared-unit standards scalars and `η` is dimensionless, so per the page's no-units-coercion charter they stay off the quantity owner the way the sibling `Composition/material` `ThermalExpansionPerK` raw double does; the carrier's `LossFactor` is the SMALL-strain internal `η` (ISO 12354-1 transmission input) and is INDEPENDENT of the `Composition/material#MATERIAL_PROPERTY` `Damping` case's large-strain design `DampingRatio` `ζ` (EN 1998 response-spectrum damping) — the amplitude regimes differ by orders of magnitude, so the two columns are independent measured data, never a `η = 2ζ` derivation pair; `CanonicalBytes` writes the bands and constants through `CanonicalWriter.Double` (exact IEEE-754 bits, sign/NaN canon) NOT `Measure` (tolerance-quantized), because the bands are dimensionless ratios and decibels not dimensioned `MeasureValue`s — the seam `Properties/quantity#MEASURE_VALUE` `SoundPressureLevel` measure is the dimensioned Pset scalar, distinct from these raw rating-input vectors; the page is PURE — no geometry, no units coercion, no external acoustics library (only the universal Thinktecture / LanguageExt / Generator.Equals substrate).

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Collections.Immutable;
using Generator.Equals;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
// The IEC 61260 one-third-octave preferred-centre vocabulary 100-5000 Hz — the union of every carried rating
// window (ASTM E413 125-4000, ISO 717-1 100-3150, ISO 11654 octave triplets 200-5000; ISO 354 measures the full
// span). NRC reads the four octave-coincident bands; SAA the twelve 200-2500 Hz bands; STC bands 1..16; Rw bands
// 0..15; the ISO 11654 octave αp_j reads the contiguous triplet 3+3j — each rating selects its window by index.
[SmartEnum<int>]
public sealed partial class AcousticBand {
 public static readonly AcousticBand Hz100  = new(0,  centerHz: 100);
 public static readonly AcousticBand Hz125  = new(1,  centerHz: 125);
 public static readonly AcousticBand Hz160  = new(2,  centerHz: 160);
 public static readonly AcousticBand Hz200  = new(3,  centerHz: 200);
 public static readonly AcousticBand Hz250  = new(4,  centerHz: 250);
 public static readonly AcousticBand Hz315  = new(5,  centerHz: 315);
 public static readonly AcousticBand Hz400  = new(6,  centerHz: 400);
 public static readonly AcousticBand Hz500  = new(7,  centerHz: 500);
 public static readonly AcousticBand Hz630  = new(8,  centerHz: 630);
 public static readonly AcousticBand Hz800  = new(9,  centerHz: 800);
 public static readonly AcousticBand Hz1000 = new(10, centerHz: 1000);
 public static readonly AcousticBand Hz1250 = new(11, centerHz: 1250);
 public static readonly AcousticBand Hz1600 = new(12, centerHz: 1600);
 public static readonly AcousticBand Hz2000 = new(13, centerHz: 2000);
 public static readonly AcousticBand Hz2500 = new(14, centerHz: 2500);
 public static readonly AcousticBand Hz3150 = new(15, centerHz: 3150);
 public static readonly AcousticBand Hz4000 = new(16, centerHz: 4000);
 public static readonly AcousticBand Hz5000 = new(17, centerHz: 5000);

 public int CenterHz { get; }
 // Accessor, never an eager static initializer: the Items read supplies the materialization edge.
 public static int Count => Items.Count;
}

// The contour-fit single-number rating family: STC (ASTM E413), Rw (ISO 717-1), and the deferred impact rating
// (Ln,w ISO 717-2 / IIC ASTM E989) are ONE contour-slide algorithm differing only in ROW DATA — the reference
// contour, the participating band window, the single-band deficiency cap, the slide SENSE, and the rating
// OFFSET — so the family is rows + one Fit kernel, never a method per rating. A row's Contour is relative to its
// 500 Hz value (offset 0 at 500 Hz), so the rating is RatingOffset ± the feasible shift at 500 Hz.
[SmartEnum<string>]
public sealed partial class RatingContour {
 // ASTM E413 STC: bands 125-4000 Hz (FirstIndex 1), an 8 dB single-band cap; airborne (the contour slides UP toward
 // the spectrum, the unfavourable deviation is spectrum-below-contour) with a 0 rating offset (rating = the shift).
 public static readonly RatingContour Stc = new("stc", firstIndex: 1, maxDeficiency: 8.0, ascending: true, ratingOffset: 0,
  contour: [-16.0, -13.0, -10.0, -7.0, -4.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 4.0, 4.0, 4.0, 4.0, 4.0]);
 // ISO 717-1 Rw: bands 100-3150 Hz (FirstIndex 0), same mid shape extended down to 100 Hz at -19 dB, NO cap (the
 // cap is the row's PositiveInfinity, so the worst-band test never trips), airborne, 0 offset — the ONLY structural
 // differences from STC are the added 100 Hz band and the dropped cap, both ROW DATA, never a second algorithm.
 public static readonly RatingContour Rw = new("rw", firstIndex: 0, maxDeficiency: double.PositiveInfinity, ascending: true, ratingOffset: 0,
  contour: [-19.0, -16.0, -13.0, -10.0, -7.0, -4.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 4.0, 4.0, 4.0, 4.0]);
 // The impact rating (Ln,w ISO 717-2 / IIC = 110 − Ln,w ASTM E989) is the DESCENDING sibling: the contour slides
 // DOWN, the unfavourable deviation is spectrum-ABOVE-contour, RatingOffset 110 for IIC. It lands as one more row
 // when Rasm.Compute carries the assembly normalized-impact spectrum (Analysis/physics Ln,w growth note) — not
 // declared here because a single MATERIAL has no impact spectrum (a floor-assembly property); the carrier's
 // DynamicStiffnessMNPerM3 is that chain's material leg (the EN 12354-2 floating-floor ΔL_w input), and the
 // sign-agnostic Fit scan admits the row's NEGATIVE feasible shifts by data alone.

 public int FirstIndex { get; }
 public double MaxDeficiency { get; }
 // The slide sense: true is the airborne upward slide (deviation = contour above spectrum), false the impact downward
 // slide (deviation = spectrum above contour). The two senses are ONE monotone search, the sense column flipping the
 // deviation and the rating direction so the impact rating is a row, never a parallel descending kernel.
 public bool Ascending { get; }
 // The rating offset the 500 Hz contour ordinate reads against: 0 for the airborne ratings (rating = the shift), 110
 // for IIC (IIC = 110 − Ln,w, and the favourable downward Ln,w shift makes IIC = 110 + shift). In BOTH senses a larger
 // feasible shift is a better rating, so the rating is RatingOffset + shift — the sense flips only the DEVIATION
 // orientation and the ceiling direction, never the monotone "more shift is better" the one return reads.
 public int RatingOffset { get; }
 public ImmutableArray<double> Contour { get; }

 const double DeficitBudget = 32.0;   // ASTM E413 / ISO 717-1 / ISO 717-2 summed-deviation limit over 16 bands

 // The shared contour slide: the unfavourable-deviation sum (and the row-capped worst band) is monotone in the
 // shift, so the downward scan returns the FIRST (highest) feasible shift — RatingOffset + shift IS the rating
 // (a larger feasible shift is a better STC AND a better IIC). Sense flips only the deviation orientation, so
 // STC/Rw (ascending, offset 0 → rating = shift) and the impact row (descending, offset 110 → IIC = 110 + shift)
 // are ONE kernel; a span loop is the named EXPRESSION_SPINE kernel exemption. ReadOnlySpan input: every caller
 // (a stored ImmutableArray spectrum, Compute's freshly-built mass-law double[]) yields one with zero copy; the
 // kernel reads and never stores, so it earns no content-equality ImmutableArray.
 public int Fit(ReadOnlySpan<double> s) {
  ReadOnlySpan<double> contour = Contour.AsSpan();
  double sense = Ascending ? 1.0 : -1.0;
  double top = double.NegativeInfinity, bottom = double.PositiveInfinity;
  for (int k = 0; k < contour.Length; k++) {
   double clear = sense * (s[FirstIndex + k] - contour[k]);
   if (clear > top) { top = clear; }
   if (clear < bottom) { bottom = clear; }
  }
  // BOTH scan bounds are spectrum-derived, so the shift domain is SIGN-AGNOSTIC — the descending impact row's
  // feasible shifts are NEGATIVE (Ln,w 60 fits at shift -60, IIC = 110 - 60), where a zero floor would exit
  // before testing a single one. Ceiling: at shift = top + budget the best-clearance band's lone deviation IS
  // the whole budget (a spectrum parallel to the contour is feasible right up to it — a `top + 1` start
  // under-reports by up to the budget), so ceil(top) + budget + 1 strictly bounds above; the MaxDeficiency cap
  // only tightens it. Floor: at shift = floor(bottom) every deviation is zero, so the downward scan ALWAYS
  // returns at or above the floor and the tail return is the unreachable totality anchor.
  int ceiling = (int)Math.Ceiling(top) + (int)DeficitBudget + 1;
  int floor = (int)Math.Floor(bottom);
  for (int shift = ceiling; shift >= floor; shift--) {
   double deficit = 0.0, worst = 0.0;
   for (int k = 0; k < contour.Length; k++) {
    double d = Math.Max(0.0, sense * (contour[k] - s[FirstIndex + k]) + shift);
    deficit += d;
    if (d > worst) { worst = d; }
   }
   if (deficit <= DeficitBudget && worst <= MaxDeficiency) { return RatingOffset + shift; }
  }
  return RatingOffset + floor;
 }
}

// The ISO 11654 sound-absorption class — the αw band A (best) through E plus the sub-0.15 Unclassified tail,
// each row carrying its αw FLOOR as the policy column so the class boundaries are ROW DATA one resolver reads,
// never a relational-pattern chain restating the table. Rows declare best-first; Of resolves the first floor
// at-or-under the rating (the half-tick 0.025 guard absorbs binary-float noise on the 0.05-gridded αw).
[SmartEnum<string>]
public sealed partial class AbsorptionClass {
 public static readonly AbsorptionClass A = new("a", floor: 0.90);
 public static readonly AbsorptionClass B = new("b", floor: 0.80);
 public static readonly AbsorptionClass C = new("c", floor: 0.60);
 public static readonly AbsorptionClass D = new("d", floor: 0.30);
 public static readonly AbsorptionClass E = new("e", floor: 0.15);
 public static readonly AbsorptionClass Unclassified = new("unclassified", floor: 0.0);

 public double Floor { get; }

 public static AbsorptionClass Of(double alphaW) =>
  toSeq(Items).Find(row => alphaW >= row.Floor - 0.025).IfNone(Unclassified);
}

// --- [MODELS] -----------------------------------------------------------------------------
// [Equatable] is LOAD-BEARING ([STRUCTURAL_EQUALITY]): the Node.Material diff drills into the MaterialPropertySet.Acoustic
// case's Spectrum member only when it is itself [Equatable] — a plain record is an opaque leaf the StructuralMerge would
// key WHOLE-spectrum. ImmutableArray (IEnumerable<double>) under [OrderedEquality] gives band-wise CONTENT equality where
// a ReadOnlyMemory member takes reference equality (two content-identical spectra off different backing arrays would
// mis-compare); the order is the AcousticBand index order CanonicalBytes also writes, so the structural diff stays
// aligned with the order-sensitive content key.
[Equatable]
public sealed partial record Acoustic {
 [property: OrderedEquality] public ImmutableArray<double> AbsorptionSpectrum { get; }
 [property: OrderedEquality] public ImmutableArray<double> SoundReductionIndexDb { get; }
 // Material-INTRINSIC constants, Option-carried (each measured only for its material class: s′ EN 29052-1 resilient
 // interlayers, σ ISO 9053 porous absorbers, η damped panels); None is absence, never a 0.0 sentinel a fold would
 // read as real. Units ride the NAME (declared-unit standards scalars, the no-units-coercion charter); each is an
 // atomic Option<double> equality leaf under the [Equatable] drill — no attribute needed.
 public Option<double> DynamicStiffnessMNPerM3 { get; }
 public Option<double> FlowResistivityPaSPerM2 { get; }
 public Option<double> LossFactor { get; }

 private Acoustic(ImmutableArray<double> absorption, ImmutableArray<double> sri,
  Option<double> dynamicStiffness, Option<double> flowResistivity, Option<double> lossFactor) =>
  (AbsorptionSpectrum, SoundReductionIndexDb, DynamicStiffnessMNPerM3, FlowResistivityPaSPerM2, LossFactor) =
   (absorption, sri, dynamicStiffness, flowResistivity, lossFactor);

 // The ONE admission (BOUNDARY_ADMISSION): both lengths the AcousticBand count, each absorption band in [0,1] (the
 // relational pattern also rejects non-finite — NaN satisfies neither bound), each SRI band finite, each Some-carried
 // constant finite and strictly positive — railing the typed offending value on the first miss; the interior (the
 // Fit windows, the ΔL_w consumer, the Double canon) never re-guards. The Option parameters default None so a
 // spectra-only datasheet calls the three-argument form. No re-hydration escape exists: the private ctor's only
 // public reach is Of, so a persisted spectrum re-admits through this same band gate, never a raw-array bypass.
 public static Fin<Acoustic> Of(ReadOnlyMemory<double> absorption, ReadOnlyMemory<double> sri, Op key,
  Option<double> dynamicStiffness = default, Option<double> flowResistivity = default, Option<double> lossFactor = default) =>
  absorption.Length != AcousticBand.Count || sri.Length != AcousticBand.Count
   ? ElementFault.ValueRejected(key, $"<acoustic-band-arity:absorption={absorption.Length}:sri={sri.Length}:expected={AcousticBand.Count}>")
   : FirstOutOfUnit(absorption.Span) is { } badAbs
    ? ElementFault.ValueRejected(key, $"<acoustic-absorption-out-of-unit:{badAbs:R}>")
    : FirstNonFinite(sri.Span) is { } badSri
     ? ElementFault.ValueRejected(key, $"<acoustic-sri-non-finite:{badSri:R}>")
     : NonPositive(dynamicStiffness) is { } badStiffness
      ? ElementFault.ValueRejected(key, $"<acoustic-dynamic-stiffness-non-positive:{badStiffness:R}>")
      : NonPositive(flowResistivity) is { } badResistivity
       ? ElementFault.ValueRejected(key, $"<acoustic-flow-resistivity-non-positive:{badResistivity:R}>")
       : NonPositive(lossFactor) is { } badLoss
        ? ElementFault.ValueRejected(key, $"<acoustic-loss-factor-non-positive:{badLoss:R}>")
        : Fin.Succ(new Acoustic([.. absorption.Span], [.. sri.Span], dynamicStiffness, flowResistivity, lossFactor));   // admission copies the transient memory into the content-equality ImmutableArray

 public double At(AcousticBand band) => AbsorptionSpectrum[band.Key];
 public double SriAt(AcousticBand band) => SoundReductionIndexDb[band.Key];

 // ASTM C423 absorption averages: the band-index window is the policy, the RoundTo step the precision.
 public double Nrc => RoundTo(Mean(NrcBands), 0.05);
 public double Saa => RoundTo(Mean(SaaBands), 0.01);

 // ISO 11654 weighted absorption over the DERIVED octave αp — the ASTM pair's ISO sibling (the same
 // dual-standard law StcWeighted/Rw hold): αw the slid-contour 500 Hz value, AlphaWShape the L/M/H
 // datasheet flags (an octave αp exceeding the slid contour by >= 0.25), AbsorptionClass the A-E read.
 public double AlphaW => AlphaWSlide().AlphaWTicks * 0.05;
 public AbsorptionClass AbsorptionClass => AbsorptionClass.Of(AlphaW);
 public (bool Low, bool Mid, bool High) AlphaWShape =>
  AlphaWSlide().Excess switch {
   var excess => ((excess & 0b00001) != 0, (excess & 0b00110) != 0, (excess & 0b11000) != 0),
  };

 // The shared RatingContour fit — StcWeighted / Rw differ ONLY by the row, never a second algorithm. The stored
 // ImmutableArray spectrum yields the Fit span with no copy through AsSpan().
 public int StcWeighted => RatingContour.Stc.Fit(SoundReductionIndexDb.AsSpan());
 public int Rw => RatingContour.Rw.Fit(SoundReductionIndexDb.AsSpan());

 // ISO 717-1 spectrum adaptation terms: Cj = XAj - Rw over the airborne band window (100-3150 Hz),
 // C against the No.1 (pink-noise) spectrum, Ctr against the No.2 (urban-traffic) spectrum.
 public int C => SpectrumAdaptation(SpectrumNo1);
 public int Ctr => SpectrumAdaptation(SpectrumNo2);

 // The canonical content contribution: both spectra then the three intrinsic constants write through
 // CanonicalWriter.Double (exact IEEE-754 bits, sign/NaN canon) so the Material node's ContentAddress covers the
 // acoustic data — bands and constants are NOT measures, so they take the Double canon not the tolerance-quantized
 // Measure canon; each Option is Bool-prefixed (self-delimiting, None and Some(v) address distinctly).
 public void CanonicalBytes(CanonicalWriter w) {
  ReadOnlySpan<double> abs = AbsorptionSpectrum.AsSpan();
  ReadOnlySpan<double> sri = SoundReductionIndexDb.AsSpan();
  w.Ordinal(abs.Length);
  for (int i = 0; i < abs.Length; i++) { w.Double(abs[i]); }
  for (int i = 0; i < sri.Length; i++) { w.Double(sri[i]); }
  Scalar(w, DynamicStiffnessMNPerM3);
  Scalar(w, FlowResistivityPaSPerM2);
  Scalar(w, LossFactor);
 }

 // --- [ACOUSTIC_TABLES] ----------------------------------------------------------------
 // The rating band windows and the ISO 717-1 reference spectra (No.1 for C, No.2 for Ctr), A-weighted and
 // normalized so the overall A-weighted level is 0 dB. Constant primitive spans promote to the data segment.
 static ReadOnlySpan<int> NrcBands => [4, 7, 10, 13];                                // 250/500/1000/2000 Hz
 static ReadOnlySpan<int> SaaBands => [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14];     // 200-2500 Hz (12 bands)
 static ReadOnlySpan<int> AlphaWReferenceTicks => [16, 20, 20, 20, 18];              // ISO 11654 contour (0.80/1.00/1.00/1.00/0.90) in 0.05 ticks, octaves 250-4000
 static ReadOnlySpan<double> SpectrumNo1 => [-29.0, -26.0, -23.0, -21.0, -19.0, -17.0, -15.0, -13.0, -12.0, -11.0, -10.0, -9.0, -9.0, -9.0, -9.0, -9.0];
 static ReadOnlySpan<double> SpectrumNo2 => [-20.0, -20.0, -18.0, -16.0, -15.0, -14.0, -13.0, -12.0, -11.0, -9.0, -8.0, -9.0, -10.0, -11.0, -13.0, -15.0];

 double Mean(ReadOnlySpan<int> indices) {
  ReadOnlySpan<double> abs = AbsorptionSpectrum.AsSpan();
  double sum = 0.0;
  foreach (int i in indices) { sum += abs[i]; }
  return sum / indices.Length;
 }

 // The ISO 11654 slide runs in INTEGER 0.05 TICKS (αp rounds to 0.05, the contour slides in 0.05 steps), so the
 // 0.10 budget is exactly 2 ticks and the 0.25 shape excess exactly 5 — integer-exact, never an accumulated-double
 // epsilon (the Fit kernel's int-dB discipline in the absorption domain). Octave αp_j is the contiguous
 // third-octave triplet mean from the 200 Hz band (first = 3+3j), capped at 1.00 (20 ticks); the contour slides
 // DOWN from zero shift, the FIRST feasible (smallest) shift is the rating, and shift 20 zeroes every deviation
 // so the scan is total. Excess is the per-octave >= 5-tick exceedance bitmask AlphaWShape folds to L/M/H.
 // The span loop is the named EXPRESSION_SPINE kernel exemption the Fit kernel already declares.
 (int AlphaWTicks, int Excess) AlphaWSlide() {
  ReadOnlySpan<int> reference = AlphaWReferenceTicks;
  ReadOnlySpan<double> abs = AbsorptionSpectrum.AsSpan();
  Span<int> ap = stackalloc int[reference.Length];
  for (int j = 0; j < ap.Length; j++) {
   int first = 3 + 3 * j;
   ap[j] = Math.Min(20, (int)Math.Round((abs[first] + abs[first + 1] + abs[first + 2]) / 0.15, MidpointRounding.AwayFromZero));
  }
  for (int shift = 0; shift <= 20; shift++) {
   int deficit = 0;
   for (int j = 0; j < ap.Length; j++) { deficit += Math.Max(0, reference[j] - shift - ap[j]); }
   if (deficit <= 2) {
    int excess = 0;
    for (int j = 0; j < ap.Length; j++) { if (ap[j] - (reference[j] - shift) >= 5) { excess |= 1 << j; } }
    return (20 - shift, excess);
   }
  }
  return (0, 0);
 }

 // XAj = -10 log10 Σ 10^((Lij - Ri)/10) over the Rw window, rounded; the adaptation term is XAj - Rw.
 int SpectrumAdaptation(ReadOnlySpan<double> spectrum) {
  ReadOnlySpan<double> sri = SoundReductionIndexDb.AsSpan();
  int first = RatingContour.Rw.FirstIndex;
  double acc = 0.0;
  for (int k = 0; k < spectrum.Length; k++) { acc += Math.Pow(10.0, (spectrum[k] - sri[first + k]) / 10.0); }
  return (int)Math.Round(-10.0 * Math.Log10(acc), MidpointRounding.AwayFromZero) - Rw;
 }

 static double RoundTo(double value, double step) => Math.Round(value / step, MidpointRounding.AwayFromZero) * step;

 // The two admission band scans — direct ReadOnlySpan<double> folds (the sibling Composition/material AllFinite idiom,
 // never a Func<double,bool> delegate per band), each returning the FIRST offending band value so Of rails the precise
 // datum. The absorption relational pattern `is not (>= 0.0 and <= 1.0)` rejects out-of-unit AND non-finite in one
 // test (NaN satisfies neither bound); the sound-reduction scan rejects only non-finite (a negative dB SRI is physical).
 static double? FirstOutOfUnit(ReadOnlySpan<double> bands) {
  foreach (double b in bands) { if (b is not (>= 0.0 and <= 1.0)) { return b; } }
  return null;
 }

 static double? FirstNonFinite(ReadOnlySpan<double> bands) {
  foreach (double b in bands) { if (!double.IsFinite(b)) { return b; } }
  return null;
 }

 // The intrinsic-constant admission scan — the Option sibling of the band scans, returning the offending value when
 // the carried scalar is non-finite or non-positive so Of rails the precise datum; None passes (absence is legal).
 static double? NonPositive(Option<double> scalar) =>
  scalar is { IsSome: true, Case: double v } && (!double.IsFinite(v) || v <= 0.0) ? v : null;

 // The Option canon write: Bool(IsSome) then the Double when present — self-delimiting, matching the
 // Composition/material CaseBytes evidence discipline (void, the corpus-wide CanonicalBytes contribution shape).
 static void Scalar(CanonicalWriter w, Option<double> value) {
  w.Bool(value.IsSome);
  value.IfSome(v => w.Double(v));
 }
}
```

## [03]-[RESEARCH]

- [ACOUSTIC_RATING_ALGEBRA]: the single-material acoustic ratings are the ASTM C423 absorption averages (`Nrc`, the mean of the four octave-coincident one-third-octave bands `250`/`500`/`1k`/`2k` rounded to `0.05`; `Saa`, the mean of the twelve one-third-octave bands `200`-`2500` rounded to `0.01`), the ISO 11654 weighted absorption (`AlphaW`: the octave `αp` per octave `250`-`4000` is the contiguous third-octave triplet mean from `200` Hz rounded to `0.05` and capped at `1.00`; the reference contour `0.80`/`1.00`/`1.00`/`1.00`/`0.90` slides toward the measured curve in `0.05` steps until the summed unfavourable deviations are at most `0.10`; αw is the slid contour's `500` Hz value; `AlphaWShape` adds the `L`/`M`/`H` indicators where an octave `αp` exceeds the slid contour by `0.25` or more; `AbsorptionClass` bands αw into `A` `>= 0.90`, `B` `0.80`-`0.85`, `C` `0.60`-`0.75`, `D` `0.30`-`0.55`, `E` `0.15`-`0.25`, `Unclassified` below — the slide computed in integer `0.05` ticks so budget and excess tests are exact), the airborne sound-insulation contour ratings (`StcWeighted`, the ASTM E413 fit over the sixteen bands `125`-`4000` with the `32` dB deficiency budget and the `8` dB single-band cap; `Rw`, the ISO 717-1 fit over the sixteen bands `100`-`3150` with the same budget and NO single-band cap — the structural difference is the `100` Hz band and the dropped cap, not a second algorithm), and the ISO 717-1 spectrum adaptation terms (`C` against reference spectrum No.1, `Ctr` against the urban-traffic spectrum No.2, each `XAj − Rw`). The one-third-octave resolution is the standards' measurement resolution and is load-bearing: a six-octave-band STC is not ASTM E413 and yields a different number, so the migration source's octave-band carrier is this rebuild's deleted predecessor; the seam relocates these as expression-bodied folds on the carrier so the rating algebra sits at the lowest stratum the `MaterialPropertySet.Acoustic` case and the `Rasm.Compute` assembly fold both read.
- [CONTOUR_FAMILY_COLLAPSE]: STC, Rw, and the impact rating are ONE contour-slide algorithm — position a reference contour relative to its `500` Hz value, slide it toward the measured spectrum in `1` dB increments until the summed unfavourable deviations are within the `32` dB budget (and, where a standard imposes it, no single-band deviation exceeds the cap), and read `RatingOffset` plus the feasible shift — differing only in the contour shape, the participating band window, the deviation cap, the slide sense, and the rating offset, ALL `RatingContour` ROW DATA the one `Fit` kernel reads, never a method per rating, a per-rating `StcContourFit` method the deleted form. STC and Rw are the PROVEN collapse — one `Fit`, differing ONLY in the `Contour`/`FirstIndex`/`MaxDeficiency` row data (the `100` Hz band and the dropped cap), both `Ascending` with offset `0` so the airborne pair adds no kernel code. The impact rating (IIC ASTM E989 / Ln,w ISO 717-2) is the DESCENDING sibling the kernel ALREADY admits by data: the `Ascending` column flips the deviation orientation (spectrum-above-contour) and the scan direction, the `RatingOffset` column supplies the `110` (IIC `= 110 + shift`), and the `Fit` monotone search — its spectrum-derived ceiling AND floor covering the descending row's NEGATIVE feasible shifts (`Ln,w` `60` fits at shift `−60`) — and the `RatingOffset + shift` return are sense-agnostic, so the impact row lands as pure `RatingContour` data the moment `Rasm.Compute` carries the assembly normalized-impact spectrum — no kernel edit, never a parallel STC/Rw/impact triplet. A single MATERIAL carries no impact spectrum (impact is a floor-assembly property), so the impact ROW is not declared here while the kernel columns that admit it ARE realized — the ANTICIPATORY_COLLAPSE proof is the impact row's zero-surface diff.
- [LAYERED_RATING_SHARE]: the `RatingContour.Fit` kernel is the shared single-number owner the `Rasm.Compute` ISO 12354 layered-assembly fold feeds its per-band layered sound-reduction vector through — the assembly sound-reduction is the field-incidence mass law over the buildup's total areal mass `m' = Σ(ρ·t)` (`R(f) = 20·log₁₀(m'·f) − 47 dB`) evaluated at each `AcousticBand` one-third-octave centre, reading the plies' `Mechanical` density NOT a per-ply acoustic spectrum, that eighteen-band vector fed ONCE through `RatingContour.Stc.Fit`/`Rw.Fit` so the assembly rating and the material rating share one contour fit; the naive per-leaf dB sum (`Σ R_i` across plies) is the DELETED form — it treats rigidly-bonded layers as acoustically independent and over-predicts a real layer set's rating by tens of dB, and the seam carries no decoupling cavity (a `MaterialLayer` is `material + thickness` only) to justify the independent-leaf model, so the combined-mass estimate is the honest closed form. The assembly aggregation itself (the series-resistance U-value, the rule-of-mixtures density, the mass-law layered sound-reduction over the accumulated areal mass evaluated at the one-third-octave centres) lives in `Rasm.Compute` over the `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition` plies, this owner contributing only the contour family and the per-material banded carrier.
- [CONTENT_CONTRIBUTION]: the `CanonicalBytes` projection writes the band count then every absorption band then every sound-reduction band then the three `Bool`-prefixed intrinsic constants through the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter.Double` primitive (the exact IEEE-754 little-endian bits with the `-0.0`→`0.0` and `NaN`→one-pattern canon), so the `Material` node's `ToCanonicalBytes` covers the acoustic data and a `Material` differing only in its absorption vector, sound-reduction vector, or an intrinsic constant addresses distinctly — editing any of them re-derives the node identity AND every derived rating in lockstep; the bands and constants take the `Double` canon NOT the tolerance-quantized `Measure` canon because they are dimensionless ratios, decibel values, and declared-unit standards scalars, not dimensioned `Properties/quantity#MEASURE_VALUE` measures — the `SoundPressureLevel` dimensioned Pset scalar admits through `MeasureValue` on the quantity owner, distinct from these raw rating-input values which carry no SI-registry coercion.
- [INTRINSIC_CONSTANTS]: the three `Option<double>` carrier constants are the material-intrinsic acoustic data the spectra cannot encode — `DynamicStiffnessMNPerM3` (EN 29052-1 dynamic stiffness per unit area `s′`, the resilient-interlayer property the EN 12354-2 floating-floor impact improvement `ΔL_w` is derived from together with the floating slab's areal mass, the deferred impact `RatingContour` row's material-side unblock); `FlowResistivityPaSPerM2` (ISO 9053 airflow resistivity `σ`, the porous-absorber input the Delany-Bazley/Miki empirical impedance models PREDICT an absorption spectrum from — a measured `AbsorptionSpectrum` stays the stored truth, `σ` the model input a `Rasm.Compute` predictive route reads when no measured spectrum exists); and `LossFactor` (the small-strain internal loss factor `η` the ISO 12354-1 structural-reverberation-time corrections read — INDEPENDENT of the `Composition/material#MATERIAL_PROPERTY` `Damping` case's large-strain design `DampingRatio` `ζ`, because the two standards measure different amplitude regimes and a `η = 2ζ` equivalence holds only at resonance for linear viscous behaviour, so the columns are independent measured data, never a derivation pair). Each is `Option`-carried (absence is `None`, never a `0.0` sentinel), guarded finite-and-strictly-positive at `Of` through the `NonPositive` scan, written through the `Bool`+`Double` self-delimiting canon, and compared as an atomic `Option<double>` leaf under the `[Equatable]` drill; the declared unit rides the member NAME because `s′`/`σ` are declared-unit standards scalars outside the SI-registry coercion the page charter excludes.

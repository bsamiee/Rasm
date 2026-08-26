# [ELEMENT_ACOUSTIC]

`Acoustic` owns the intrinsic single-material acoustic evidence: the per-band `AbsorptionSpectrum` and `SoundReductionIndexDb` vectors over the eighteen `AcousticBand` `100`-to-`5000` Hz one-third-octave centres, and three material-INTRINSIC `Option<double>` constants — `DynamicStiffnessMNPerM3` (EN 29052-1 `s′`, the resilient-layer property the EN 12354-2 floating-floor `ΔL_w` reads), `FlowResistivityPaSPerM2` (ISO 9053 `σ`, the Delany-Bazley/Miki porous-absorber model input), and `LossFactor` (the small-strain internal `η` the ISO 12354-1 structural-reverberation corrections read).

`RatingContour` mints the `[SmartEnum<string>]` contour-fit family (`Stc`/`Rw`/`Iic`/`Lnw`) whose rows differ only in data and SHARE one `Fit` kernel; `AbsorptionClass` mints the ISO 11654 A-E class vocabulary whose αw floors ride the row policy column; the PURE projection folds — `Nrc`/`Saa` (ASTM C423), `AlphaW`/`AlphaWShape`/`AbsorptionClass` (the ISO 11654 weighted absorption over the derived octave `αp`, its `L`/`M`/`H` shape flags, and the A-E class), `StcWeighted` (ASTM E413) and `Rw` (ISO 717-1) over the shared contour fit, and `C`/`Ctr` (the ISO 717-1 spectrum adaptation terms) — compute on read.

Banded data is the ONE source of truth: every rating is an expression-bodied projection computed on read, never a drift-prone scalar stored beside the spectrum. `Acoustic` stays HOST-NEUTRAL and PURE — no geometry, no units coercion (the bands are dimensionless ratios and decibels, not dimensioned `MeasureValue`s), no external acoustics library.

`AcousticBand` fixes the standards' MEASUREMENT resolution — ASTM E413 and ISO 717-1 are defined over the sixteen one-third-octave bands `125`-`4000` Hz and `100`-`3150` Hz, so a six-octave-band rating is the DELETED approximation; NRC reads the four octave-coincident bands (`250`/`500`/`1k`/`2k`), SAA the twelve bands `200`-`2500`, and the ISO 11654 octave `αp` the contiguous third-octave triplets `200`-`5000` off the same vector — the `5000` Hz row carried for the `4000`-octave triplet (ISO 354 measures the full `100`-`5000` span).

`RatingContour.Fit` stays PUBLIC so the `Rasm.Compute` ISO 12354 layered-assembly fold feeds its per-band layered sound-reduction vector through the SAME contour fit the per-material rating uses — one contour-fit owner, never a second STC/Rw algorithm. `Iic` (ASTM E989) and `Lnw` (ISO 717-2) are the DESCENDING impact row pair rating the `Rasm.Compute` assembly normalized-impact spectrum — a single material has no impact spectrum, so the rows carry reference-contour data alone, and `DynamicStiffnessMNPerM3` carries the pair's material input (the `Rasm.Compute` floor fold derives `ΔL_w` from `s′` and the floating slab's areal mass before the impact spectrum reaches the shared `Fit`).

`Acoustic` composes the `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet.Acoustic` case, the kernel admission slots its `Of` and `Fit` admissions accumulate over, and the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter`; universal scalar refusals remain `KernelFault`, while spectral-shape refusals use `ElementFault.ValueRejected`.

## [01]-[INDEX]

- [02]-[ACOUSTIC_FOLDS]: `AcousticBand` one-third-octave vocabulary, `Acoustic` banded carrier (the two spectra and the `DynamicStiffnessMNPerM3`/`FlowResistivityPaSPerM2`/`LossFactor` intrinsic constants) with its `Of` admission, `RatingContour` `[SmartEnum<string>]` contour-fit family sharing one `Fit` kernel, `AbsorptionClass` ISO 11654 A-E vocabulary, `Nrc`/`Saa`/`AlphaW`/`AlphaWShape`/`StcWeighted`/`Rw`/`C`/`Ctr` projection folds, and the `CanonicalBytes` content contribution.

## [02]-[ACOUSTIC_FOLDS]

- Owner: `AcousticBand` the `[SmartEnum<int>]` eighteen-row one-third-octave-centre vocabulary keyed on the band index, each carrying its centre frequency; `RatingContour` the `[SmartEnum<string>]` contour-fit family (`Stc`/`Rw`/`Iic`/`Lnw`), each row carrying its reference contour, its first-band index, its single-band deficiency cap, its slide-sense (`Ascending`), its reported-figure orientation (`RatingSign`), and its `RatingOffset`, all sharing the `DeficitBudget` and the one `Fit` kernel; `AbsorptionClass` the `[SmartEnum<string>]` ISO 11654 class vocabulary (`A`-`E` and `Unclassified`), each row carrying its αw `Floor` policy column the one `Of` resolver reads; `Acoustic` the `[Equatable]` banded carrier holding the fixed-length eighteen-band `AbsorptionSpectrum` and `SoundReductionIndexDb` `[OrderedEquality]` `ImmutableArray<double>` vectors, the three material-intrinsic `Option<double>` constants (`DynamicStiffnessMNPerM3` the EN 29052-1 dynamic stiffness per unit area `s′`, `FlowResistivityPaSPerM2` the ISO 9053 airflow resistivity `σ`, `LossFactor` the small-strain internal loss factor `η` — each an atomic equality leaf the `[Equatable]` drill compares by `Option<double>` value), the `Nrc`/`Saa`/`AlphaW`/`AlphaWShape`/`AbsorptionClass`/`StcWeighted`/`Rw`/`C`/`Ctr` projection folds, and the `CanonicalBytes` projection.
- Entry: `Acoustic.Of(absorption, sri, key, dynamicStiffness, flowResistivity, lossFactor)` admits the two eighteen-band vectors AND the three optional intrinsic constants once — arity against the `AcousticBand` count, each absorption band in `[0,1]`, each sound-reduction band finite, each `Some`-carried constant on `Band.Positive` — the independent gates joining applicatively so `Fin<T>` carries every named fault in one failure; `SriAt(band)` reads a band by its row; `Average(AbsorptionAverage)` is the ONE absorption-average fold (`Nrc`/`Saa` its one-hop reads), `Adaptation(AdaptationSpectrum)` the ONE ISO 717-1 adaptation read (`C`/`Ctr` one-hop), `AlphaW`/`AlphaWShape`/`AbsorptionClass` the ISO 11654 projections (`AlphaWShape` a kernel `CapabilitySet<AlphaWShapeFlag>` whose rows own their octave masks), `StcWeighted`/`Rw` the shared `RatingContour` fit reads, and `AbsorptionClass.Of(alphaW, key)` the result-returning public class resolution; `RatingContour.<row>.Fit(span, key)` is the RESULT-RETURNING cross-assembly contour gate (`Stc`/`Rw` airborne, `Iic`/`Lnw` impact) entering the internal `FitAdmitted` kernel; `CanonicalBytes(writer)` contributes the spectra and constants through the kernel `Doubles`/`Optional` canon.
- Auto: the absorption averages read their row's window and step (`AbsorptionAverage.Nrc` 250-2k at `0.05`, `.Saa` 200-2500 at `0.01`, ASTM C423); `AlphaW` runs the ISO 11654 slide in INTEGER `0.05` ticks (octave `αp` triplet means capped at `1.00`, the reference contour sliding until the two-tick budget holds, the `≥ 5`-tick per-octave excess masked onto the `AlphaWShapeFlag` rows); `StcWeighted`/`Rw` run the SHARED `FitAdmitted` slide — the row carries contour, window, `Option` cap, sense, sign, and offset, so STC, Rw, IIC, and Ln,w are ONE algorithm differing only in ROW DATA and `Report(shift)` the one figure projection; `C`/`Ctr` evaluate `XAj − Rw` over their row's reference spectrum; the folds read the band spans directly so a rating never drifts from the spectrum.
- Output: the `Acoustic` carrier is what the `MaterialPropertySet.Acoustic` case wraps and the `Bake`-derived `Element` reads flat; `StcWeighted`/`Rw`/`Nrc`/`Saa`/`AlphaW`/`AlphaWShape`/`AbsorptionClass`/`C`/`Ctr` are derived reads, never stored, so editing one band re-derives every rating (a finish schedule reads `a.AbsorptionClass` and `a.AlphaWShape` beside `a.Nrc` off one carrier); the `CanonicalBytes` projection writes both spectra AND the three intrinsic constants (each `Bool`-prefixed, so `None` and `Some(v)` address distinctly) through the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter` so the `Material` node's content address covers the acoustic data — editing a band or an intrinsic constant changes the node identity AND every rating in lockstep; the `RatingContour` family is the shared single-number owner the `Rasm.Compute` ISO 12354 layered fold feeds its per-band layered sound-reduction vector through, so the assembly STC/Rw and the material STC/Rw share one contour fit — the impact pair likewise, over the assembly normalized-impact spectrum.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<int>]`/`[SmartEnum<string>]`), LanguageExt.Core (`Fin` the admission result, `Option<double>` the intrinsic-constant absence carrier), Generator.Equals (`[Equatable]`/`[OrderedEquality]` the band-wise structural diff the `Acoustic` carrier carries so the `Node.Material` merge drills into it), `Rasm.Element` (the `CanonicalWriter` codec), `Rasm` (the kernel `Op` and `Rasm/Domain/validation#ADMISSION_SLOTS`), BCL inbox (`ImmutableArray<double>` the stored content-equality spectra, `ReadOnlyMemory<double>` the `Of` admission shape, `ReadOnlySpan<double>` the `Fit` transient input, `Math`).
- Growth: a new band is one `AcousticBand` row; a new airborne or impact single-number rating is ONE `RatingContour` row (contour, window, cap, sense, sign, offset — the impact pair landed as the descending rows over the assembly spectrum only `Rasm.Compute` produces, E-E11); a new absorption average is one `AbsorptionAverage` row; a new adaptation term one `AdaptationSpectrum` row; a new class boundary one `AbsorptionClass` row; a new intrinsic constant is one `Option<double>` member + one `Optional(Band.Positive)` slot + one canon write — everything grows by row or column, never a second algorithm.
- Boundary: `Acoustic` is a BANDED spectrum NOT a scalar — the `AbsorptionSpectrum` and `SoundReductionIndexDb` are fixed-length eighteen-band `[OrderedEquality]` `ImmutableArray<double>` vectors over the `AcousticBand` one-third-octave centres (NOT `ReadOnlyMemory<double>` members — the carrier is the `MaterialPropertySet.Acoustic` case's `Spectrum`, and the `Node.Material` `Generator.Equals` diff drills into a nested value only when it is `[Equatable]`, so a plain-record / `ReadOnlyMemory<double>` carrier takes REFERENCE equality and be an opaque whole-spectrum diff leaf — the deleted form per the `Graph/element#NODE_MODEL` `[STRUCTURAL_EQUALITY]` mandate; the `ImmutableArray<double>` is `IEnumerable<double>` so `[OrderedEquality]` gives band-wise content equality aligned with the order-sensitive content key), and a single-number absorption or STC field is the deleted form; the one-third-octave resolution is load-bearing — ASTM E413 and ISO 717-1 are defined over one-third-octave bands, so a six-octave-band STC is the deleted approximation that yields a different number than the standard, and an octave-band carrier is the deleted form; `StcWeighted`/`Rw`/`Nrc`/`Saa`/`AlphaW`/`C`/`Ctr` are expression-bodied projection folds over the carriers, never stored ratings that drift from the spectrum; `AlphaW` derives its octave `αp` from the stored one-third-octave vector and slides in INTEGER `0.05` ticks (the two-tick `0.10` budget and five-tick `0.25` shape excess are exact integer tests, never accumulated-double comparisons), the `AbsorptionClass` thresholds are `Floor` ROW DATA on the vocabulary (a relational-pattern chain restating the A-E table is the deleted form), and the ISO pair (`AlphaW`/`AbsorptionClass`) COMPLEMENTS the ASTM pair (`Nrc`/`Saa`) — the dual-standard law `StcWeighted`/`Rw` already hold — never a replacement; `StcWeighted` and `Rw` are ONE contour-slide algorithm differing only in the `RatingContour` row data (the `Rw` curve adds the `100` Hz band and drops the `8` dB cap), so a per-rating contour method is the deleted form; the `RatingContour.Fit` kernel is the ONE contour-fit owner — the `Rasm.Compute` assembly layered fold feeds its computed per-band layered sound-reduction vector through the same result-returning `Fit(span, key)` gate (a raw cross-assembly span admits its contour window ONCE there, never a caller-only length convention and never an unchecked parallel entrypoint), a second STC/Rw algorithm being the named defect, and the assembly aggregation (the mass-law layered spectrum, the normalized-impact spectrum) lives in `Rasm.Compute`, this owner contributing only the contour family and the per-material banded carrier — the impact rows rate that assembly spectrum alone, so no per-material `Iic`/`Lnw` read lands on the carrier; an out-of-`[0,1]` absorption band, a non-finite sound-reduction band, or a non-positive `Some`-carried intrinsic constant returns `KernelFault.OutOfRange` at `Of`, never a clamped sentinel, and an unmeasured constant is `None` — never a `0.0` sentinel the impact fold reads as a real stiffness; the intrinsic constants are `Option<double>` with the declared unit in the NAME (`MNPerM3`, `PaSPerM2`), NOT `MeasureValue` — `s′` (MN/m³) and `σ` (Pa·s/m², the rayl/m) are declared-unit standards scalars and `η` is dimensionless, so per the page's no-units-coercion charter they stay off the quantity owner the way the sibling `Composition/material` `ThermalExpansionPerK` raw double does; the carrier's `LossFactor` is the SMALL-strain internal `η` (ISO 12354-1 transmission input) and is INDEPENDENT of the `Composition/material#MATERIAL_PROPERTY` `Damping` case's large-strain design `DampingRatio` `ζ` (EN 1998 response-spectrum damping) — the amplitude regimes differ by orders of magnitude, so the two columns are independent measured data, never a `η = 2ζ` derivation pair; `CanonicalBytes` writes the bands and constants through `CanonicalWriter.Double` (exact IEEE-754 bits, sign/NaN canon) NOT `Measure` (tolerance-quantized), because the bands are dimensionless ratios and decibels not dimensioned `MeasureValue`s — the shared `Properties/quantity#MEASURE_VALUE` `SoundPressureLevel` measure is the dimensioned Pset scalar, distinct from these raw rating-input vectors; the page is PURE — no geometry, no units coercion, no external acoustics library (only the universal Thinktecture / LanguageExt / Generator.Equals substrate).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Immutable;
using Generator.Equals;
using LanguageExt;
using LanguageExt.Traits;
using Rasm.Domain;
using Rasm.Element.Projection;
using Thinktecture;
using Band = Rasm.Numerics.Band;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Composition;

// --- [TYPES] ---------------------------------------------------------------------------
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
}

[SmartEnum<string>]
public sealed partial class RatingContour {
 public static readonly RatingContour Stc = new("stc", firstIndex: 1, maxDeficiency: Some(8.0), sense: 1, ratingSign: 1, ratingOffset: 0,
  contour: [-16.0, -13.0, -10.0, -7.0, -4.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 4.0, 4.0, 4.0, 4.0, 4.0]);
 public static readonly RatingContour Rw = new("rw", firstIndex: 0, maxDeficiency: None, sense: 1, ratingSign: 1, ratingOffset: 0,
  contour: [-19.0, -16.0, -13.0, -10.0, -7.0, -4.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 4.0, 4.0, 4.0, 4.0]);
 private static readonly ImmutableArray<double> ImpactContour = [2.0, 2.0, 2.0, 2.0, 2.0, 2.0, 1.0, 0.0, -1.0, -2.0, -3.0, -6.0, -9.0, -12.0, -15.0, -18.0];
 public static readonly RatingContour Iic = new("iic", firstIndex: 0, maxDeficiency: Some(8.0), sense: -1, ratingSign: 1, ratingOffset: 110,
  contour: ImpactContour);
 public static readonly RatingContour Lnw = new("lnw", firstIndex: 0, maxDeficiency: None, sense: -1, ratingSign: -1, ratingOffset: 0,
  contour: ImpactContour);

 public int FirstIndex { get; }
 public Option<double> MaxDeficiency { get; }
 public int Sense { get; }
 public int RatingSign { get; }
 public int RatingOffset { get; }
 public ImmutableArray<double> Contour { get; }

 private const double DeficitBudget = 32.0;

 public Fin<int> Fit(ReadOnlySpan<double> s, Op key) {
  if (s.Length < FirstIndex + Contour.Length) {
   return new ElementFault.ValueRejected(key, $"<contour-window-short:{s.Length}:expected>={FirstIndex + Contour.Length}>");
  }
  Fin<Unit> window = Indexed(s.Slice(FirstIndex, Contour.Length), double.IsFinite, key, "contour-band-non-finite").ToFin();
  return window.IsSucc ? Fin.Succ(FitAdmitted(s)) : window.Map(static _ => 0);
 }

 internal int FitAdmitted(ReadOnlySpan<double> s) {
  ReadOnlySpan<double> contour = Contour.AsSpan();
  double sense = Sense;
  double top = double.NegativeInfinity, bottom = double.PositiveInfinity;
  for (int k = 0; k < contour.Length; k++) {
   double clear = sense * (s[FirstIndex + k] - contour[k]);
   if (clear > top) { top = clear; }
   if (clear < bottom) { bottom = clear; }
  }
  int ceiling = (int)Math.Ceiling(top) + (int)DeficitBudget + 1;
  int floor = (int)Math.Floor(bottom);
  for (int shift = ceiling; shift >= floor; shift--) {
   double deficit = 0.0, worst = 0.0;
   for (int k = 0; k < contour.Length; k++) {
    double d = Math.Max(0.0, sense * (contour[k] - s[FirstIndex + k]) + shift);
    deficit += d;
    if (d > worst) { worst = d; }
   }
   if (deficit <= DeficitBudget && MaxDeficiency.ForAll(cap => worst <= cap)) { return Report(shift); }
  }
  return Report(floor);
 }

 int Report(int shift) => RatingOffset + (RatingSign * shift);
}

[SmartEnum<string>]
public sealed partial class AbsorptionClass {
 public static readonly AbsorptionClass A = new("a", floor: 0.90);
 public static readonly AbsorptionClass B = new("b", floor: 0.80);
 public static readonly AbsorptionClass C = new("c", floor: 0.60);
 public static readonly AbsorptionClass D = new("d", floor: 0.30);
 public static readonly AbsorptionClass E = new("e", floor: 0.15);
 public static readonly AbsorptionClass Unclassified = new("unclassified", floor: 0.0);

 public double Floor { get; }

 internal static AbsorptionClass OfAdmitted(double alphaW) =>
  toSeq(Items.OrderByDescending(static row => row.Floor)).Find(row => alphaW >= row.Floor - 0.025).IfNone(Unclassified);

 public static Fin<AbsorptionClass> Of(double alphaW, Op key) =>
  In(alphaW, Band.Unit, "absorption-class", key).Map(OfAdmitted).ToFin();
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AlphaWShapeFlag : ICapability<AlphaWShapeFlag> {
 public static readonly AlphaWShapeFlag L = new("l", mask: 0b00001);
 public static readonly AlphaWShapeFlag M = new("m", mask: 0b00110);
 public static readonly AlphaWShapeFlag H = new("h", mask: 0b11000);

 public int Mask { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AbsorptionAverage {
 public static readonly AbsorptionAverage Nrc = new("nrc", step: 0.05, bands: [4, 7, 10, 13]);
 public static readonly AbsorptionAverage Saa = new("saa", step: 0.01, bands: [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14]);

 public double Step { get; }
 public ImmutableArray<int> Bands { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class AdaptationSpectrum {
 public static readonly AdaptationSpectrum C = new("c",
  levels: [-29.0, -26.0, -23.0, -21.0, -19.0, -17.0, -15.0, -13.0, -12.0, -11.0, -10.0, -9.0, -9.0, -9.0, -9.0, -9.0]);
 public static readonly AdaptationSpectrum Ctr = new("ctr",
  levels: [-20.0, -20.0, -18.0, -16.0, -15.0, -14.0, -13.0, -12.0, -11.0, -9.0, -8.0, -9.0, -10.0, -11.0, -13.0, -15.0]);

 public ImmutableArray<double> Levels { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[Equatable]
public sealed partial record Acoustic {
 [property: OrderedEquality] public ImmutableArray<double> AbsorptionSpectrum { get; }
 [property: OrderedEquality] public ImmutableArray<double> SoundReductionIndexDb { get; }
 public Option<double> DynamicStiffnessMNPerM3 { get; }
 public Option<double> FlowResistivityPaSPerM2 { get; }
 public Option<double> LossFactor { get; }

 private Acoustic(ImmutableArray<double> absorption, ImmutableArray<double> sri,
  Option<double> dynamicStiffness, Option<double> flowResistivity, Option<double> lossFactor) =>
  (AbsorptionSpectrum, SoundReductionIndexDb, DynamicStiffnessMNPerM3, FlowResistivityPaSPerM2, LossFactor) =
   (absorption, sri, dynamicStiffness, flowResistivity, lossFactor);

 public static Fin<Acoustic> Of(ReadOnlyMemory<double> absorption, ReadOnlyMemory<double> sri, Op key,
  Option<double> dynamicStiffness = default, Option<double> flowResistivity = default, Option<double> lossFactor = default) =>
  (Gate(absorption.Length == AcousticBand.Items.Count && sri.Length == AcousticBand.Items.Count, key, $"<acoustic-band-arity:absorption={absorption.Length}:sri={sri.Length}:expected={AcousticBand.Items.Count}>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Indexed(absorption.Span, static band => band is >= 0.0 and <= 1.0, key, "acoustic-absorption-out-of-unit"),
   Indexed(sri.Span, double.IsFinite, key, "acoustic-sri-non-finite"),
   Optional(dynamicStiffness, Band.Positive, "acoustic-dynamic-stiffness", key),
   Optional(flowResistivity, Band.Positive, "acoustic-flow-resistivity", key),
   Optional(lossFactor, Band.Positive, "acoustic-loss-factor", key))
  .Apply((_, _, _, _, _, _) => new Acoustic([.. absorption.Span], [.. sri.Span], dynamicStiffness, flowResistivity, lossFactor))
  .As().ToFin();

 public double SriAt(AcousticBand band) => SoundReductionIndexDb[band.Key];

 public double Average(AbsorptionAverage row) => RoundTo(Mean(row.Bands.AsSpan()), row.Step);
 public double Nrc => Average(AbsorptionAverage.Nrc);
 public double Saa => Average(AbsorptionAverage.Saa);

 public double AlphaW => AlphaWSlide().AlphaWTicks * 0.05;
 public AbsorptionClass AbsorptionClass => AbsorptionClass.OfAdmitted(AlphaW);
 public CapabilitySet<AlphaWShapeFlag> AlphaWShape {
  get {
   int excess = AlphaWSlide().Excess;
   return toSeq(AlphaWShapeFlag.Items).Filter(flag => (excess & flag.Mask) != 0)
    .Fold(CapabilitySet<AlphaWShapeFlag>.None, static (held, flag) => held.With(flag));
  }
 }

 public int StcWeighted => RatingContour.Stc.FitAdmitted(SoundReductionIndexDb.AsSpan());
 public int Rw => RatingContour.Rw.FitAdmitted(SoundReductionIndexDb.AsSpan());

 public int Adaptation(AdaptationSpectrum row) => SpectrumAdaptation(row.Levels.AsSpan());
 public int C => Adaptation(AdaptationSpectrum.C);
 public int Ctr => Adaptation(AdaptationSpectrum.Ctr);

 public void CanonicalBytes(CanonicalWriter w) =>
  w.Doubles(AbsorptionSpectrum.AsSpan()).Doubles(SoundReductionIndexDb.AsSpan())
   .Optional(DynamicStiffnessMNPerM3, static (v, run) => run.Double(v))
   .Optional(FlowResistivityPaSPerM2, static (v, run) => run.Double(v))
   .Optional(LossFactor, static (v, run) => run.Double(v));

 // --- [ACOUSTIC_TABLES] ----------------------------------------------------------------
 private static ReadOnlySpan<int> AlphaWReferenceTicks => [16, 20, 20, 20, 18];

 double Mean(ReadOnlySpan<int> indices) {
  ReadOnlySpan<double> abs = AbsorptionSpectrum.AsSpan();
  double sum = 0.0;
  foreach (int i in indices) { sum += abs[i]; }
  return sum / indices.Length;
 }

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

 int SpectrumAdaptation(ReadOnlySpan<double> spectrum) {
  ReadOnlySpan<double> sri = SoundReductionIndexDb.AsSpan();
  int first = RatingContour.Rw.FirstIndex;
  double acc = 0.0;
  for (int k = 0; k < spectrum.Length; k++) { acc += Math.Pow(10.0, (spectrum[k] - sri[first + k]) / 10.0); }
  return (int)Math.Round(-10.0 * Math.Log10(acc), MidpointRounding.AwayFromZero) - Rw;
 }

 private static double RoundTo(double value, double step) => Math.Round(value / step, MidpointRounding.AwayFromZero) * step;

}
```

## [03]-[RESEARCH]

(none)

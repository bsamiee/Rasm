# [ELEMENT_ACOUSTIC]

The intrinsic single-material acoustic owner: the `AcousticBand` `[SmartEnum<int>]` ONE-THIRD-OCTAVE centre vocabulary, the banded `Acoustic` carrier holding the per-band `AbsorptionSpectrum` and `SoundReductionIndexDb` vectors over the seventeen `100`-to-`4000` Hz one-third-octave centres, the `RatingContour` `[SmartEnum<string>]` contour-fit family (`Stc`/`Rw`) whose rows differ only in data and SHARE one `Fit` kernel, and the PURE projection folds — `Nrc`/`Saa` (the ASTM C423 absorption averages), `StcWeighted` (ASTM E413) and `Rw` (ISO 717-1) over the shared contour fit, and `C`/`Ctr` (the ISO 717-1 spectrum adaptation terms). The folds are EXPRESSION-BODIED projections over the band carriers, never stored ratings that could drift from the spectrum: a material's NRC, STC, Rw, and adaptation terms are computed from its absorption/sound-reduction vectors on read, so the one source of truth is the banded data. The vocabulary is the standards' MEASUREMENT resolution — ASTM E413 and ISO 717-1 are defined over the sixteen one-third-octave bands `125`-`4000` Hz and `100`-`3150` Hz, so a six-octave-band rating is the DELETED approximation, not ASTM E413; NRC reads the four octave-coincident bands (`250`/`500`/`1k`/`2k`) and SAA the twelve bands `200`-`2500` directly off the same one-third-octave vector. This owner is the seam home the migration source's `Rasm.Materials` acoustic case relocates to so the rating algebra sits at the lowest stratum, and the `RatingContour` `Fit` kernel is PUBLIC so the `Rasm.Compute` ISO 12354 layered-assembly fold feeds its per-band layered sound-reduction vector through the SAME contour fit the per-material rating uses — one contour-fit owner, never a second STC/Rw algorithm, and the impact rating (IIC ASTM E989 / Ln,w ISO 717-2) lands as ONE more `RatingContour` row when its assembly spectrum is carried in `Rasm.Compute`. The page composes the `Composition/material#MATERIAL_PROPERTY` `MaterialPropertySet.Acoustic` case (which wraps the `Acoustic` carrier and forwards its reads) and the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter` (the `Acoustic` spectra contribute to the `Material` node's content hash), and rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` on a non-finite sound-reduction band or an out-of-`[0,1]` absorption band. The page is HOST-NEUTRAL and PURE — no geometry, no units coercion (the bands are dimensionless ratios and decibels, not dimensioned `MeasureValue`s), no external package.

## [01]-[INDEX]

- [01]-[ACOUSTIC_FOLDS]: the `AcousticBand` one-third-octave vocabulary, the banded `Acoustic` carrier and its `Of` admission, the `RatingContour` `[SmartEnum<string>]` contour-fit family sharing one `Fit` kernel, the `Nrc`/`Saa`/`StcWeighted`/`Rw`/`C`/`Ctr` projection folds, and the `CanonicalBytes` content contribution.

## [02]-[ACOUSTIC_FOLDS]

- Owner: `AcousticBand` the `[SmartEnum<int>]` seventeen-row one-third-octave-centre vocabulary keyed on the band index, each carrying its centre frequency; `RatingContour` the `[SmartEnum<string>]` contour-fit family (`Stc`/`Rw`), each row carrying its reference contour, its first-band index, and its single-band deficiency cap, all sharing the `DeficitBudget` and the one `Fit` kernel; `Acoustic` the banded carrier holding the fixed-length seventeen-band `AbsorptionSpectrum` and `SoundReductionIndexDb` `ReadOnlyMemory<double>` vectors, the `Nrc`/`Saa`/`StcWeighted`/`Rw`/`C`/`Ctr` projection folds, and the `CanonicalBytes` projection.
- Entry: `Acoustic.Of(absorption, sri, key)` admits the two seventeen-band vectors once at construction — each absorption band in `[0,1]` (a relational pattern that also rejects a non-finite band), each sound-reduction band finite, both lengths equal to the seventeen `AcousticBand` centres — `Fin<T>` railing `ElementFault.ValueRejected` on a band-arity mismatch, an out-of-unit absorption, or a non-finite sound-reduction; `At(band)`/`SriAt(band)` read a band by its `AcousticBand` row; `Nrc`/`Saa`/`StcWeighted`/`Rw`/`C`/`Ctr` are the expression-bodied projection folds; `RatingContour.Stc.Fit(sri)`/`Rw.Fit(sri)` is the static single-number contour fit the `Rasm.Compute` assembly layered fold shares; `CanonicalBytes(writer)` contributes the spectra to the node content hash.
- Auto: `Nrc` averages the four octave-coincident bands (`250`/`500`/`1k`/`2k`, the `NrcBands` index span) and rounds to the nearest `0.05` through `RoundTo` (ASTM C423); `Saa` averages the twelve bands `200`-`2500` (the `SaaBands` span) and rounds to the nearest `0.01` (ASTM C423); `StcWeighted` and `Rw` run the SHARED `RatingContour.Fit` — the contour slides up from a spectrum-derived ceiling until the summed deficiencies are within `DeficitBudget` (`32` dB) and (for `Stc`) no single-band deficiency exceeds its `8` dB cap, the highest passing contour value at `500` Hz the rating; `C`/`Ctr` evaluate the ISO 717-1 adaptation `XAj − Rw` over the `SpectrumNo1`/`SpectrumNo2` reference spectra; the folds read the band spans directly so a rating never drifts from the spectrum.
- Receipt: the `Acoustic` carrier is the acoustic evidence the `MaterialPropertySet.Acoustic` case wraps and the `Bake`-derived `Element` reads flat; `StcWeighted`/`Rw`/`Nrc`/`Saa`/`C`/`Ctr` are derived reads, never stored, so editing one band re-derives every rating; the `CanonicalBytes` projection writes both spectra through the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter` so the `Material` node's content address covers the acoustic data — editing a band changes the node identity AND every rating in lockstep; the `RatingContour` family is the shared single-number owner the `Rasm.Compute` ISO 12354 layered fold feeds its per-band layered sound-reduction vector through, so the assembly STC/Rw and the material STC/Rw share one contour fit.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<int>]`/`[SmartEnum<string>]`), LanguageExt.Core (`Fin`), `Rasm.Element` (the `CanonicalWriter` codec), `Rasm` (the kernel `Op` op-key), BCL inbox (`ReadOnlyMemory<double>`/`ReadOnlySpan<double>`/`ImmutableArray<double>`/`Math`).
- Growth: a new octave/one-third-octave band is one `AcousticBand` row (the band carriers widen by data and the folds re-read the new index); a new AIRBORNE contour-fit rating is ONE `RatingContour` row carrying its contour, first-band index, and deficiency cap — STC and Rw already collapse to the one upward-slide `Fit` by ROW DATA ALONE (no kernel code); the impact rating (IIC ASTM E989 / Ln,w ISO 717-2) keeps the ONE `Fit` owner by GROWING it with a slide-sense column (the deficiency inverts to spectrum-above-contour) plus a `110 − x` rating-offset column the row supplies, landing when `Rasm.Compute` carries the assembly normalized-impact spectrum — a bounded extension of the one kernel, never a parallel STC/Rw/impact method triplet; a new absorption average is one expression-bodied fold over a band-index span and a `RoundTo` step; the bands grow by data, the airborne ratings by row, the averages by fold — never a stored scalar column and never a second contour algorithm.
- Boundary: `Acoustic` is a BANDED spectrum NOT a scalar — the `AbsorptionSpectrum` and `SoundReductionIndexDb` are fixed-length seventeen-band vectors over the `AcousticBand` one-third-octave centres, and a single-number absorption or STC field is the deleted form; the one-third-octave resolution is load-bearing — ASTM E413 and ISO 717-1 are defined over one-third-octave bands, so a six-octave-band STC is the deleted approximation that yields a different number than the standard, and the migration source's octave-band carrier is the rebuilt form's deleted predecessor; `StcWeighted`/`Rw`/`Nrc`/`Saa`/`C`/`Ctr` are expression-bodied projection folds over the carriers, never stored ratings that drift from the spectrum; `StcWeighted` and `Rw` are ONE contour-slide algorithm differing only in the `RatingContour` row data (the `Rw` curve adds the `100` Hz band and drops the `8` dB cap), so a per-rating contour method is the deleted form; the `RatingContour.Fit` kernel is the ONE contour-fit owner — the `Rasm.Compute` assembly layered fold feeds its computed per-band layered sound-reduction vector through this same kernel, a second STC/Rw algorithm being the named defect, and the assembly aggregation (the mass-law layered spectrum) lives in `Rasm.Compute`, this owner contributing only the contour family and the per-material banded carrier; an out-of-`[0,1]` absorption band or a non-finite sound-reduction band rails `ElementFault.ValueRejected` at `Of`, never a clamped sentinel; `CanonicalBytes` writes the bands through `CanonicalWriter.Double` (exact IEEE-754 bits, sign/NaN canon) NOT `Measure` (tolerance-quantized), because the bands are dimensionless ratios and decibels not dimensioned `MeasureValue`s — the seam `Properties/quantity#MEASURE_VALUE` `SoundPressureLevel` measure is the dimensioned Pset scalar, distinct from these raw rating-input vectors; the page is PURE — no geometry, no units coercion, no external package.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
// The IEC 61260 one-third-octave preferred-centre vocabulary 100-4000 Hz — the resolution ASTM E413 / C423
// and ISO 717-1 are defined over. NRC reads the four octave-coincident bands; SAA the twelve 200-2500 Hz
// bands; STC bands 1..16 (125-4000); Rw bands 0..15 (100-3150) — each rating selects its window by index.
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

 public int CenterHz { get; }
 public int Index => Key;
 public static readonly int Count = Items.Count;
}

// The contour-fit single-number rating family: STC (ASTM E413) and Rw (ISO 717-1) are ONE upward-slide
// algorithm differing only in the reference contour, the participating band window, and the single-band
// deficiency cap — so the family is rows + one Fit kernel, never a method per rating. A row's Contour is
// relative to its 500 Hz value (offset 0 at 500 Hz), so the rating IS the feasible shift at 500 Hz.
[SmartEnum<string>]
public sealed partial class RatingContour {
 // ASTM E413 STC: bands 125-4000 Hz (FirstIndex 1), an 8 dB single-band deficiency cap.
 public static readonly RatingContour Stc = new("stc", firstIndex: 1, maxDeficiency: 8.0,
  contour: [-16.0, -13.0, -10.0, -7.0, -4.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 4.0, 4.0, 4.0, 4.0, 4.0]);
 // ISO 717-1 Rw: bands 100-3150 Hz (FirstIndex 0), same mid shape extended down to 100 Hz at -19 dB, NO cap
 // (the only structural difference from STC — the cap is the row's PositiveInfinity, so the test never trips).
 public static readonly RatingContour Rw = new("rw", firstIndex: 0, maxDeficiency: double.PositiveInfinity,
  contour: [-19.0, -16.0, -13.0, -10.0, -7.0, -4.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0, 4.0, 4.0, 4.0, 4.0]);

 public int FirstIndex { get; }
 public double MaxDeficiency { get; }
 public ImmutableArray<double> Contour { get; }

 const double DeficitBudget = 32.0;   // ASTM E413 / ISO 717-1 summed-deficiency limit over 16 bands

 // The shared upward contour slide: feasibility (sum of deficiencies within budget and worst within cap) is
 // monotone in the shift, so scan from the ceiling where the contour first clears the spectrum at the binding
 // band down to the highest feasible shift — that shift IS the rating (the contour value at 500 Hz). A span
 // loop is the named EXPRESSION_SPINE kernel exemption; the ceiling is spectrum-derived, never a magic cap.
 public int Fit(ReadOnlyMemory<double> spectrum) {
  ReadOnlySpan<double> s = spectrum.Span;
  ReadOnlySpan<double> contour = Contour.AsSpan();
  double top = double.NegativeInfinity;
  for (int k = 0; k < contour.Length; k++) { double clear = s[FirstIndex + k] - contour[k]; if (clear > top) { top = clear; } }
  for (int shift = (int)Math.Ceiling(top) + 1; shift >= 0; shift--) {
   double deficit = 0.0, worst = 0.0;
   for (int k = 0; k < contour.Length; k++) {
    double d = Math.Max(0.0, contour[k] + shift - s[FirstIndex + k]);
    deficit += d;
    if (d > worst) { worst = d; }
   }
   if (deficit <= DeficitBudget && worst <= MaxDeficiency) { return shift; }
  }
  return 0;
 }
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Acoustic {
 public ReadOnlyMemory<double> AbsorptionSpectrum { get; }
 public ReadOnlyMemory<double> SoundReductionIndexDb { get; }

 private Acoustic(ReadOnlyMemory<double> absorption, ReadOnlyMemory<double> sri) =>
  (AbsorptionSpectrum, SoundReductionIndexDb) = (absorption, sri);

 public static Fin<Acoustic> Of(ReadOnlyMemory<double> absorption, ReadOnlyMemory<double> sri, Op key) =>
  absorption.Length != AcousticBand.Count || sri.Length != AcousticBand.Count
   ? ElementFault.ValueRejected(key, $"<acoustic-band-arity:absorption={absorption.Length}:sri={sri.Length}:expected={AcousticBand.Count}>")
   : FirstFailing(absorption.Span, static a => a is >= 0.0 and <= 1.0) is { } badAbs
    ? ElementFault.ValueRejected(key, $"<acoustic-absorption-out-of-unit:{badAbs:R}>")
    : FirstFailing(sri.Span, static r => double.IsFinite(r)) is { } badSri
     ? ElementFault.ValueRejected(key, $"<acoustic-sri-non-finite:{badSri:R}>")
     : Fin.Succ(new Acoustic(absorption, sri));

 // Catalogue/test seeding of already-valid spectra — bypasses Of's admission for the Rasm.Materials curated rows.
 internal static Acoustic Seed(double[] absorption, double[] sri) => new(absorption, sri);

 public double At(AcousticBand band) => AbsorptionSpectrum.Span[band.Index];
 public double SriAt(AcousticBand band) => SoundReductionIndexDb.Span[band.Index];

 // ASTM C423 absorption averages: the band-index window is the policy, the RoundTo step the precision.
 public double Nrc => RoundTo(Mean(NrcBands), 0.05);
 public double Saa => RoundTo(Mean(SaaBands), 0.01);

 // The shared RatingContour fit — StcWeighted / Rw differ ONLY by the row, never a second algorithm.
 public int StcWeighted => RatingContour.Stc.Fit(SoundReductionIndexDb);
 public int Rw => RatingContour.Rw.Fit(SoundReductionIndexDb);

 // ISO 717-1 spectrum adaptation terms: Cj = XAj - Rw over the airborne band window (100-3150 Hz),
 // C against the No.1 (pink-noise) spectrum, Ctr against the No.2 (urban-traffic) spectrum.
 public int C => SpectrumAdaptation(SpectrumNo1);
 public int Ctr => SpectrumAdaptation(SpectrumNo2);

 // The canonical content contribution: both spectra write through CanonicalWriter.Double (exact IEEE-754 bits,
 // sign/NaN canon) so the Material node's ContentAddress covers the acoustic data — bands are NOT measures, so
 // they take the Double canon not the tolerance-quantized Measure canon.
 public CanonicalWriter CanonicalBytes(CanonicalWriter w) {
  ReadOnlySpan<double> abs = AbsorptionSpectrum.Span;
  ReadOnlySpan<double> sri = SoundReductionIndexDb.Span;
  w.Ordinal(abs.Length);
  for (int i = 0; i < abs.Length; i++) { w.Double(abs[i]); }
  for (int i = 0; i < sri.Length; i++) { w.Double(sri[i]); }
  return w;
 }

 // --- [ACOUSTIC_TABLES] ----------------------------------------------------------------
 // The rating band windows and the ISO 717-1 reference spectra (No.1 for C, No.2 for Ctr), A-weighted and
 // normalized so the overall A-weighted level is 0 dB. Constant primitive spans promote to the data segment.
 static ReadOnlySpan<int> NrcBands => [4, 7, 10, 13];                                // 250/500/1000/2000 Hz
 static ReadOnlySpan<int> SaaBands => [3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14];     // 200-2500 Hz (12 bands)
 static ReadOnlySpan<double> SpectrumNo1 => [-29.0, -26.0, -23.0, -21.0, -19.0, -17.0, -15.0, -13.0, -12.0, -11.0, -10.0, -9.0, -9.0, -9.0, -9.0, -9.0];
 static ReadOnlySpan<double> SpectrumNo2 => [-20.0, -20.0, -18.0, -16.0, -15.0, -14.0, -13.0, -12.0, -11.0, -9.0, -8.0, -9.0, -10.0, -11.0, -13.0, -15.0];

 double Mean(ReadOnlySpan<int> indices) {
  ReadOnlySpan<double> abs = AbsorptionSpectrum.Span;
  double sum = 0.0;
  foreach (int i in indices) { sum += abs[i]; }
  return sum / indices.Length;
 }

 // XAj = -10 log10 Σ 10^((Lij - Ri)/10) over the Rw window, rounded; the adaptation term is XAj - Rw.
 int SpectrumAdaptation(ReadOnlySpan<double> spectrum) {
  ReadOnlySpan<double> sri = SoundReductionIndexDb.Span;
  int first = RatingContour.Rw.FirstIndex;
  double acc = 0.0;
  for (int k = 0; k < spectrum.Length; k++) { acc += Math.Pow(10.0, (spectrum[k] - sri[first + k]) / 10.0); }
  return (int)Math.Round(-10.0 * Math.Log10(acc), MidpointRounding.AwayFromZero) - Rw;
 }

 static double RoundTo(double value, double step) => Math.Round(value / step, MidpointRounding.AwayFromZero) * step;

 static double? FirstFailing(ReadOnlySpan<double> bands, Func<double, bool> ok) {
  foreach (double b in bands) { if (!ok(b)) { return b; } }
  return null;
 }
}
```

## [03]-[RESEARCH]

- [ACOUSTIC_RATING_ALGEBRA]: the single-material acoustic ratings are the ASTM C423 absorption averages (`Nrc`, the mean of the four octave-coincident one-third-octave bands `250`/`500`/`1k`/`2k` rounded to `0.05`; `Saa`, the mean of the twelve one-third-octave bands `200`-`2500` rounded to `0.01`), the airborne sound-insulation contour ratings (`StcWeighted`, the ASTM E413 fit over the sixteen bands `125`-`4000` with the `32` dB deficiency budget and the `8` dB single-band cap; `Rw`, the ISO 717-1 fit over the sixteen bands `100`-`3150` with the same budget and NO single-band cap — the structural difference is the `100` Hz band and the dropped cap, not a second algorithm), and the ISO 717-1 spectrum adaptation terms (`C` against reference spectrum No.1, `Ctr` against the urban-traffic spectrum No.2, each `XAj − Rw`). The one-third-octave resolution is the standards' measurement resolution and is load-bearing: a six-octave-band STC is not ASTM E413 and yields a different number, so the migration source's octave-band carrier is this rebuild's deleted predecessor; the seam relocates these as expression-bodied folds on the carrier so the rating algebra sits at the lowest stratum the `MaterialPropertySet.Acoustic` case and the `Rasm.Compute` assembly fold both read.
- [CONTOUR_FAMILY_COLLAPSE]: STC, Rw, and the impact rating are ONE contour-slide algorithm — position a reference contour relative to its `500` Hz value, slide it toward the measured spectrum in `1` dB increments until the summed deficiencies are within the `32` dB budget (and, where a standard imposes it, no single-band deficiency exceeds the cap), and read the contour value at `500` Hz — differing only in the contour shape, the participating band window, the deficiency cap, and the slide direction; so the family is the `RatingContour` `[SmartEnum<string>]` rows plus one `Fit` kernel, never a method per rating, and a per-rating `StcContourFit` method is the deleted form. STC and Rw are the PROVEN collapse — one `Fit`, differing ONLY in the `Contour`/`FirstIndex`/`MaxDeficiency` row data (the `100` Hz band and the dropped cap), so the airborne pair adds no kernel code. The impact rating (IIC ASTM E989 / Ln,w ISO 717-2) keeps the ONE `Fit` owner by GROWING it with a slide-sense column (the deficiency inverts to spectrum-above-contour) plus a `110 − x` rating-offset column the row supplies, landing when `Rasm.Compute` carries the assembly normalized-impact spectrum — a bounded growth of the one kernel rather than a parallel STC/Rw/impact triplet, the collapse holding because the airborne ratings differ by data alone and the impact rating by data plus one shared kernel column.
- [LAYERED_RATING_SHARE]: the `RatingContour.Fit` kernel is the shared single-number owner the `Rasm.Compute` ISO 12354 layered-assembly fold feeds its per-band layered sound-reduction vector through — the assembly sound-reduction is the field-incidence mass law over the buildup's total areal mass `m' = Σ(ρ·t)` (`R(f) = 20·log₁₀(m'·f) − 47 dB`) evaluated at each `AcousticBand` one-third-octave centre, reading the plies' `Mechanical` density NOT a per-ply acoustic spectrum, that seventeen-band vector fed ONCE through `RatingContour.Stc.Fit`/`Rw.Fit` so the assembly rating and the material rating share one contour fit; the naive per-leaf dB sum (`Σ R_i` across plies) is the DELETED form — it treats rigidly-bonded layers as acoustically independent and over-predicts a real layer set's rating by tens of dB, and the seam carries no decoupling cavity (a `MaterialLayer` is `material + thickness` only) to justify the independent-leaf model, so the combined-mass estimate is the honest closed form. The assembly aggregation itself (the series-resistance U-value, the rule-of-mixtures density, the mass-law layered sound-reduction over the accumulated areal mass evaluated at the one-third-octave centres) lives in `Rasm.Compute` over the `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition` plies, this owner contributing only the contour family and the per-material banded carrier.
- [CONTENT_CONTRIBUTION]: the `CanonicalBytes` projection writes the band count then every absorption band then every sound-reduction band through the `Projection/address#CONTENT_ADDRESS` `CanonicalWriter.Double` primitive (the exact IEEE-754 little-endian bits with the `-0.0`→`0.0` and `NaN`→one-pattern canon), so the `Material` node's `ToCanonicalBytes` covers the acoustic spectra and a `Material` differing only in its absorption or sound-reduction vector addresses distinctly — editing a band re-derives the node identity AND every derived rating in lockstep; the bands take the `Double` canon NOT the tolerance-quantized `Measure` canon because they are dimensionless absorption ratios and decibel sound-reduction values, not dimensioned `Properties/quantity#MEASURE_VALUE` measures — the `SoundPressureLevel` dimensioned Pset scalar admits through `MeasureValue` on the quantity owner, distinct from these raw rating-input vectors which carry no SI dimension and never coerce through `UnitsNet`.

# [ELEMENT_ACOUSTIC]

The intrinsic single-material acoustic owner: the `AcousticBand` octave-centre vocabulary, the banded `Acoustic` carrier holding a per-band absorption spectrum and a per-band sound-reduction-index vector over the six `125`/`250`/`500`/`1k`/`2k`/`4k` Hz centres, and the PURE projection folds — `Nrc` (the four-band ASTM C423 mean), `Saa` (the ASTM C423 sound-absorption average), `StcWeighted` (the ASTM E413 single-number rating), and the shared `StcContourFit` kernel. The folds are EXPRESSION-BODIED projections over the band carriers, never stored ratings that could drift from the spectrum: a material's NRC and STC are computed from its absorption/SRI vectors on read, so the one source of truth is the banded data. This owner is the seam home the migration source's `Rasm.Materials` acoustic case relocates to so the rating algebra sits at the lowest stratum, the `StcContourFit` kernel PUBLIC so the `Rasm.Compute` layered-STC assembly fold (ISO 12354) feeds its per-band layered SRI vector through the SAME contour fit the per-material rating uses — one contour-fit owner, never a second STC algorithm. The page composes the `Composition/material#MATERIAL_NODE` `MaterialPropertySet.Acoustic` case (which wraps the `Acoustic` carrier) and rails `Projection/fault#FAULT_BAND` `ElementFault.ValueRejected` (band 2500) on a non-finite SRI band or an out-of-`[0,1]` absorption band. The page is HOST-NEUTRAL and PURE — no geometry, no units coercion (the bands are dimensionless ratios and dB), no external package.

## [01]-[INDEX]

- [01]-[ACOUSTIC_FOLDS]: the `AcousticBand` `[SmartEnum<int>]` octave-centre vocabulary, the banded `Acoustic` carrier and its `Of` admission, the `Nrc`/`Saa`/`StcWeighted` projection folds, and the shared `StcContourFit` single-number kernel.

## [02]-[ACOUSTIC_FOLDS]

- Owner: `AcousticBand` the `[SmartEnum<int>]` six-row octave-centre vocabulary keyed on the band index, each carrying its centre frequency; `Acoustic` the banded carrier holding the fixed-length six-band `AbsorptionSpectrum` and `SoundReductionIndexDb` `ReadOnlyMemory<double>` vectors; the `Nrc`/`Saa`/`StcWeighted` projection folds and the `StcContourFit` kernel.
- Entry: `Acoustic.Of(absorption, sri, key)` admits the two six-band vectors once at construction — each absorption band in `[0,1]`, each SRI band finite, both lengths equal to the six `AcousticBand` centres — `Fin<T>` railing `ElementFault.ValueRejected` on a band-arity mismatch, an out-of-unit absorption, or a non-finite SRI; `At(band)`/`SriAt(band)` read a band by its `AcousticBand` row; `Nrc`/`Saa`/`StcWeighted` are the expression-bodied projection folds; `StcContourFit(sri)` is the static single-number contour fit the assembly layered-STC fold shares.
- Auto: `Nrc` averages the four mid bands (`250`/`500`/`1k`/`2k`, the `IsNrcBand` predicate) and rounds to the nearest `0.05` (ASTM C423); `Saa` averages all six bands and rounds to the nearest `0.01`; `StcWeighted` runs `StcContourFit` over the SRI vector — the contour slides down from `80` dB until the deficiency sum is within `32` dB and no single band deficiency exceeds `8` dB (ASTM E413), the highest passing contour the rating; the folds read the band spans directly so a rating never drifts from the spectrum.
- Receipt: the `Acoustic` carrier is the acoustic evidence the `MaterialPropertySet.Acoustic` case wraps and the `Bake`-derived `Element` reads flat; `StcWeighted`/`Nrc`/`Saa` are derived reads, never stored, so editing one band re-derives every rating; the `StcContourFit` kernel is the shared single-number owner the `Rasm.Compute` ISO 12354 layered-STC assembly fold feeds its per-band layered SRI vector through, so the assembly STC and the material STC share one contour fit.
- Packages: Thinktecture.Runtime.Extensions (`[SmartEnum<int>]`), LanguageExt.Core (`Fin`/`guard`), `Rasm` (the kernel `Op` op-key), BCL inbox (`ReadOnlyMemory<double>`).
- Growth: a new octave band is one `AcousticBand` row (the band carrier widens by data and the folds re-read the new index); a new acoustic rating is one expression-bodied fold over the band carrier (a weighted-apparent-sound-reduction `Rw`, a `C`/`Ctr` spectrum adaptation), never a stored scalar column; the bands grow by data, the ratings by fold.
- Boundary: `Acoustic` is a BANDED spectrum NOT a scalar — the `AbsorptionSpectrum` and `SoundReductionIndexDb` are fixed-length six-band vectors over the `AcousticBand` octave centres, and a single-number absorption or STC field is the deleted form; `Nrc`/`Saa`/`StcWeighted` are expression-bodied projection folds over the carriers, never stored ratings that drift from the spectrum; an out-of-`[0,1]` absorption band or a non-finite SRI band rails `ElementFault.ValueRejected` at `Of`, never a clamped sentinel; the `StcContourFit` kernel is PUBLIC and the ONE contour-fit owner — the `Rasm.Compute` assembly layered-STC fold feeds its computed per-band layered SRI vector through this same kernel, a second STC algorithm being the named defect; the page is PURE — no geometry, no units coercion, no external package, the bands being dimensionless ratios and decibels.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class AcousticBand {
 public static readonly AcousticBand Hz125 = new(0, centerHz: 125);
 public static readonly AcousticBand Hz250 = new(1, centerHz: 250);
 public static readonly AcousticBand Hz500 = new(2, centerHz: 500);
 public static readonly AcousticBand Hz1000 = new(3, centerHz: 1000);
 public static readonly AcousticBand Hz2000 = new(4, centerHz: 2000);
 public static readonly AcousticBand Hz4000 = new(5, centerHz: 4000);

 public int CenterHz { get; }
 public int Index => Key;
 public static readonly int Count = Items.Count;
 public static bool IsNrcBand(int index) => index is >= 1 and <= 4;
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record Acoustic {
 public ReadOnlyMemory<double> AbsorptionSpectrum { get; }
 public ReadOnlyMemory<double> SoundReductionIndexDb { get; }

 private Acoustic(ReadOnlyMemory<double> absorption, ReadOnlyMemory<double> sri) =>
 (AbsorptionSpectrum, SoundReductionIndexDb) = (absorption, sri);

 public static Fin<Acoustic> Of(ReadOnlyMemory<double> absorption, ReadOnlyMemory<double> sri, Op key) =>
 guard(absorption.Length == AcousticBand.Count && sri.Length == AcousticBand.Count,
 ElementFault.ValueRejected(key, $"<acoustic-band-arity:absorption={absorption.Length}:sri={sri.Length}:expected={AcousticBand.Count}>"))
 .ToFin()
 .Bind(_ => OutOfUnit(absorption.Span) is { } badAbs
 ? Fin.Fail<Acoustic>(ElementFault.ValueRejected(key, $"<acoustic-absorption-out-of-unit:{badAbs:R}>"))
 : NonFinite(sri.Span) is { } badSri
 ? Fin.Fail<Acoustic>(ElementFault.ValueRejected(key, $"<acoustic-sri-non-finite:{badSri:R}>"))
 : Fin.Succ(new Acoustic(absorption, sri)));

 internal static Acoustic Seed(double[] absorption, double[] sri) => new(absorption, sri);

 public double At(AcousticBand band) => AbsorptionSpectrum.Span[band.Index];
 public double SriAt(AcousticBand band) => SoundReductionIndexDb.Span[band.Index];

 public double Nrc =>
 Math.Round(toSeq(System.Linq.Enumerable.Range(0, AcousticBand.Count))
 .Filter(AcousticBand.IsNrcBand)
 .Map(i => AbsorptionSpectrum.Span[i])
 .Average() * 20.0, MidpointRounding.AwayFromZero) / 20.0;

 public double Saa =>
 Math.Round(toSeq(System.Linq.Enumerable.Range(0, AcousticBand.Count))
 .Map(i => AbsorptionSpectrum.Span[i])
 .Average() * 100.0, MidpointRounding.AwayFromZero) / 100.0;

 public int StcWeighted => StcContourFit(SoundReductionIndexDb);

 static double? OutOfUnit(ReadOnlySpan<double> bands) {
 foreach (double b in bands) { if (b is < 0.0 or > 1.0 || !double.IsFinite(b)) { return b; } }
 return null;
 }

 static double? NonFinite(ReadOnlySpan<double> bands) {
 foreach (double b in bands) { if (!double.IsFinite(b)) { return b; } }
 return null;
 }

 // --- [STC_CONTOUR] --------------------------------------------------------------------
 // The ASTM E413 single-number contour fit, PUBLIC so the Rasm.Compute ISO 12354 layered-STC
 // assembly fold feeds its per-band layered SRI vector through the SAME kernel — one contour owner.
 public static int StcContourFit(ReadOnlyMemory<double> sri) {
 ReadOnlySpan<double> bands = sri.Span;
 int best = 0;
 for (int stc = 80; stc >= 0; stc--) {
 double deficiencySum = 0.0;
 bool maxOk = true;
 for (int i = 0; i < bands.Length; i++) {
 double contour = StcContourDb(stc, i);
 double deficiency = Math.Max(0.0, contour - bands[i]);
 deficiencySum += deficiency;
 if (deficiency > 8.0) { maxOk = false; }
 }
 if (deficiencySum <= 32.0 && maxOk) { best = stc; break; }
 }
 return best;
 }

 static double StcContourDb(int stc, int bandIndex) =>
 bandIndex switch {
 0 => stc - 16.0,
 1 => stc - 5.0,
 2 => stc + 0.0,
 3 => stc + 1.0,
 4 => stc + 4.0,
 _ => stc + 4.0,
 };
}
```

## [03]-[RESEARCH]

- [ACOUSTIC_RATING_ALGEBRA]: the `Nrc` (ASTM C423 four-band 250/500/1k/2k mean rounded to 0.05), `Saa` (ASTM C423 sound-absorption average over the third-octave bands approximated by the six-band mean rounded to 0.01), and `StcWeighted` (ASTM E413 contour fit: the highest 500 Hz contour value whose summed deficiencies are within 32 dB and whose worst single-band deficiency is within 8 dB) are the standard single-number ratings derived from the banded spectrum, never stored — the migration source carried these as expression-bodied folds on the `Rasm.Materials` `MaterialProperty.Acoustic` case and this owner relocates them to the seam so the rating algebra sits at the lowest stratum the `MaterialPropertySet.Acoustic` case and the Compute assembly fold both read.
- [LAYERED_STC_SHARE]: the `StcContourFit` kernel is the shared single-number owner the `Rasm.Compute` ISO 12354 simplified-composite layered-STC fold feeds its per-band layered SRI vector through — each leaf's transmission multiplies so the per-band SRI adds in dB, the resulting layered vector fed once through this kernel so the assembly STC and the material STC share one contour fit; the assembly aggregation itself (the series-resistance U-value, the rule-of-mixtures density, the mass-law/coincidence layered SRI) lives in `Rasm.Compute` over the `Composition/material#MATERIAL_NODE` `MaterialComposition` plies, this owner contributing only the contour-fit kernel and the per-material banded carrier.

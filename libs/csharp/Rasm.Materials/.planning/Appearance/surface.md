# [MATERIALS_SURFACE]

THE COLOR-SCIENCE LOWERING and THE OPENPBR-CONSTRUCTION HALF. The surface page owns the four lowering/grounding kernels the wire and the library drive — ONE `SpectralUpsample` RGB→SPD kernel feeding Unicolour's `Spd`→XYZ and the measured-illuminant reduction every base color grounds through, ONE `ToneMap` scene-to-display operator table (`aces` · `agx` · `pbr-neutral` · `reinhard` · `filmic` · `exposure`) egressing through the `DisplayEncoding` transfer-and-range rows, ONE `ConductorMetal` axis grounding every metal F0 from the measured complex-IOR carried on its rows, and ONE `SlabStack` OpenPBR Surface 1.1 stack-of-slabs the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` fold lowers from. The page owns the `ToneOperator`, `DisplayEncoding`, and `ConductorMetal` axes, the `Slab` `[Union]` closed family (its own discriminant — no parallel kind enum), and the spectral/tone/conductor/slab kernels; it COMPOSES the `bsdf#SHADING_FRAME` `SpectralBand` band vocabulary and `MaterialFault` (band 2450) declared once on the kernel page, the `bsdf#LOBE_FAMILY` `RgbSpectrum`/`ComplexIor` validated carriers and `BsdfLobe` closed set, and the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf.Of` weighted-lobe fold — the kernel page owns frame-local shading, this page owns the OpenPBR construction the `interchange#MATERIAL_WIRE` and the `weathering#WEATHERING` aging trajectory target.

This page is the lowering boundary: a `graph#MATERIAL_LIBRARY` `MaterialParameters` row lowers through the one `OpenPbrSurface.Of` column correspondence to the canonical OpenPBR vector (the SAME vector `interchange#MATERIAL_WIRE` projects, never re-minted at the wire), `SlabStack.Lower` derives the formal OpenPBR Surface 1.1 stack from it (fuzz over coat over emission over base), the base substrate grounds its conductor lobe from the `ConductorMetal` row's measured `Ior`, the base color upsamples to an SPD through `SpectralUpsample`, and `SlabStack.ToLayered` collapses the albedo-scaled stack to the one `LayeredBsdf` weighted fold the renderer shades — so the OpenPBR vector is the canonical lowering, the slab algebra the construction the row drives through, the lobe math single-sourced on the kernel page, and the tone-map the display-referred egress the raster path consumes. The split is BY CONCERN, not by size: the kernel page carries the per-sample shading math the path tracer drives, this page the color-science and OpenPBR-construction the wire and library drive, the two sharing the `MaterialFault` band declared once on the kernel.

## [01]-[INDEX]

- [01]-[SPECTRAL_UPSAMPLE]: the RGB→SPD coefficient kernel, the Unicolour `Spd`→XYZ composition, the measured-illuminant reduction, and scene-linear admission under a parameterized `GamutMap` strategy.
- [02]-[TONE_MAP]: the `ToneOperator` table (ACES-fit, AgX, PBR Neutral, Reinhard, filmic, exposure) over one triple-valued `Grade` column, and the `DisplayEncoding` transfer-and-range egress rows.
- [03]-[CONDUCTOR_IOR]: the `ConductorMetal` axis carrying the measured complex-IOR per RGB band on its rows, the wire `Resolve`, and the `Conductor` lobe grounding.
- [04]-[OPENPBR_SLAB]: the `Slab` `[Union]` closed family (its four cases the slab discriminant, no parallel kind enum), the `OpenPbrSurface` vector and its one `Of` `MaterialParameters`→OpenPBR lowering, the `SlabStack` outermost-to-base layering algebra, and the `ToLayered` collapse the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` fold consumes.

## [02]-[SPECTRAL_UPSAMPLE]

- Owner: `SpectralUpsample` author-kernel; composes the `bsdf#SHADING_FRAME` `SpectralBand` `[SmartEnum<string>]` band-centre vocabulary (declared once on the kernel page, read by the thin-film lobe and the spectral curve) and Wacton.Unicolour for every conversion Unicolour already owns.
- Entry: `public static Fin<Spd> ToSpd(RgbSpectrum rgb, Op key)` and `public static Fin<RgbSpectrum> SceneLinear(Unicolour colour, Op key, GamutMap strategy = GamutMap.OklchChromaReduction)` — RGB→SPD is the author-kernel Unicolour lacks (NOT_COVERED); SceneLinear COMPOSES the one `PortValue.SceneLinear` Acescg working space and Unicolour's `.RgbLinear` accessor, never re-deriving the linearization; the out-of-gamut pull-in is the parameterized `GamutMap` policy (the `bsdf#WHITE_FURNACE_HARNESS` passes `GamutMap.WxyPurityReduction` beside the default to price the two strategies); both entries carry the `Op key` the `MaterialFault` rail correlates.
- Packages: Wacton.Unicolour (composed — `Spd`, `Unicolour`, `Configuration`, `RgbConfiguration`, `ColourSpace`, `DeltaE`, `GamutMap`/`MapToRgbGamut`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured illuminant is one Unicolour `Spd` construction; a new working space is one `RgbConfiguration` preset selection; the upsampling table is the only author-kernel — a new spectral band is one `SpectralBand` row; zero new surface. A measured isotropic spectral BRDF (EPFL RGL goniophotometer, brdf-loader format) admits through one `Spd` construction per band, framed at `[06]-[RESEARCH]` and the `acquisition#ACQUISITION` import path.
- Boundary: RGB→SPD is the documented Unicolour NOT_COVERED concern, authored as the Smits (1999) seven-basis non-negative reflectance upsampling — the constant/cyan/magenta/yellow/red/green/blue basis SPDs combined so the round-trip `SPD→XYZ→RGB` reproduces the input chromaticity with a smooth, energy-bounded reflectance (the appearance-engine requirement Smits states); the resulting `Spd` feeds Unicolour's `new Unicolour(Configuration, Spd)` → internal `Xyz.FromSpd` for the measured-illuminant path; the scene-linear working space is the ONE `graph#MATERIAL_GRAPH` `PortValue.SceneLinear` Acescg `Configuration` instance (AP1 primaries; one instance means one Unicolour lazy-conversion cache identity — a second `new Configuration(RgbConfiguration.Acescg)` mint forks the cache and is the deleted form; `PortValue.SceneLinear` is its ONE canonical spelling corpus-wide — the `acquisition#ACQUISITION`/`finish#FINISH` grounding constructions read it directly, a local `SceneConfig` re-export alias is the deleted form), and read through Unicolour's `.RgbLinear` — Materials NEVER re-derives the sRGB/ACEScg transfer curve, it composes the preset; appearance MATCH between a measured target and a library row is the direct `Unicolour.Difference(reference, DeltaE.Ciede2000)` call, the industry-standard appearance ΔE — no local rename wrapper exists; the `IsInRgbGamut` check gates the boundary shade and a saturated upsampled primary that lands outside the gamut is perceptually pulled in through the composed `MapToRgbGamut(strategy)` rather than hard-faulted — `GamutMap.OklchChromaReduction` (reduce Oklch chroma until in gamut) the default policy, `GamutMap.WxyPurityReduction` (walk the dominant-wavelength excitation purity in) the admitted second strategy the white-furnace ΔE-cost comparison prices against it — so the white-furnace residual closes on a gamut-mapped in-gamut reflectance instead of rejecting the row, and the fault rail is reserved for a non-finite channel; `RgbSpectrum.Luminance` reads the AP1 weights consistent with the ACEScg working space; Wacton.Unicolour is consumed directly as the one scene-linear/spectral color owner and Materials never mints a second `ColourSpace` wrapper. Every base color, conductor, and FinishMix pigment grounds through this one owner — the `acquisition#ACQUISITION` `GroundSpectral` composes the `ToSpd`/`new Unicolour(Configuration, Spd)` RGB→SPD path AND the `SceneLinear` grounding, while `finish#FINISH` `FinishMix.Reflectance` composes ONLY `SceneLinear` over its Kubelka-Munk pigment-mix reflectance (the `new Unicolour(Configuration, Pigment[], double[])` ctor owns the mix, NOT the `Spd`/`ToSpd` upsample) — both fold through this one `SceneLinear` grounding owner, never N parallel inline spectral-construction sites.
- Law: the page's `[EXPRESSION_SPINE]` kernel exemptions are `SpectralUpsample` (`ToSpd`/`Acc`/`Resample5nm` fill fixed-length `double[]` buffers by index across the Smits ordered-combination branch and the 69-sample 5nm resample), the `ToneMap` curve kernels (`AcesFit`/`ReinhardExtended`/`FilmicCurve`/`AgxGrade`/`PbrNeutralGrade` — fixed-width channel math on the per-frame display path), and the `SlabStack` mix-chain bindings (`CoatLobes`/`LowerBase`), the admitted boundary-numeric-kernel carve-out from the immutable-fold law; every admission, dispatch, and egress surface on the page is expression-bodied.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                       // Seq, Option, Fin
using Rasm.Domain;                       // Op
using Rasm.Materials.Appearance.Bsdf;    // MaterialFault, RgbSpectrum, ComplexIor, BsdfLobe, LayeredBsdf, LobeWeight, Microfacet, MultiScatter, LocalVector, SpectralBand-consumers
using Rasm.Materials.Appearance.Graph;   // MaterialParameters, SubsurfaceRadius, ThinFilm (the OpenPbrSurface.Of source row + its scatter/film carriers), PortValue.SceneLinear (the one Acescg Configuration instance)
using Rasm.Vectors;                      // UnitInterval
using Thinktecture;                      // ComparerAccessors, [SmartEnum]/[Union]/[KeyMember*], ConversionOperatorsGeneration
using Wacton.Unicolour;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance.Surface;

// SpectralBand (the red/green/blue 610/550/465 nm band-centre vocabulary) is declared on bsdf#SHADING_FRAME and
// composed here — the kernel thin-film lobe and this spectral curve read the one band vocabulary, no second register.

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SpectralUpsample {
    private static readonly double[] White =   [1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0];
    private static readonly double[] Cyan =    [0.97, 0.97, 0.97, 0.93, 0.12, 0.04, 0.0, 0.0, 0.05, 0.05];
    private static readonly double[] Magenta = [0.99, 0.99, 0.84, 0.18, 0.03, 0.05, 0.40, 0.99, 0.99, 0.99];
    private static readonly double[] Yellow =  [0.0, 0.0, 0.10, 0.78, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0];
    private static readonly double[] RedB =    [0.10, 0.10, 0.0, 0.0, 0.0, 0.0, 0.84, 1.0, 1.0, 1.0];
    private static readonly double[] GreenB =  [0.0, 0.0, 0.03, 0.49, 1.0, 1.0, 0.46, 0.0, 0.0, 0.0];
    private static readonly double[] BlueB =   [1.0, 1.0, 0.89, 0.46, 0.06, 0.0, 0.0, 0.0, 0.0, 0.05];

    public static Fin<Spd> ToSpd(RgbSpectrum rgb, Op key) {
        double[] r = new double[10];
        double red = rgb.R, green = rgb.G, blue = rgb.B;
        if (red <= green && red <= blue) { Acc(r, White, red); if (green <= blue) { Acc(r, Cyan, green - red); Acc(r, BlueB, blue - green); } else { Acc(r, Cyan, blue - red); Acc(r, GreenB, green - blue); } }
        else if (green <= red && green <= blue) { Acc(r, White, green); if (red <= blue) { Acc(r, Magenta, red - green); Acc(r, BlueB, blue - red); } else { Acc(r, Magenta, blue - green); Acc(r, RedB, red - blue); } }
        else { Acc(r, White, blue); if (red <= green) { Acc(r, Yellow, red - blue); Acc(r, GreenB, green - red); } else { Acc(r, Yellow, green - blue); Acc(r, RedB, red - green); } }
        for (int i = 0; i < r.Length; i++) { r[i] = Math.Clamp(r[i], 0.0, 1.0); }
        Spd spd = new(380, 5, Resample5nm(r));
        return spd.IsValid ? Fin.Succ(spd) : Fin.Fail<Spd>(MaterialFault.Parameter(key, "<spd-interval-invalid>"));
    }
    private static void Acc(double[] dst, double[] basis, double w) { double c = Math.Max(0.0, w); for (int i = 0; i < dst.Length; i++) { dst[i] += basis[i] * c; } }
    private static double[] Resample5nm(double[] controls) {
        double[] o = new double[69];
        for (int i = 0; i < o.Length; i++) { double t = i / 68.0 * 9.0; int lo = (int)t; double f = t - lo; o[i] = lo >= 9 ? controls[9] : controls[lo] + (controls[lo + 1] - controls[lo]) * f; }
        return o;
    }

    // The gamut pull-in is a parameterized policy: OklchChromaReduction the default grounding, WxyPurityReduction the
    // second admitted strategy the white-furnace harness prices for ΔE cost on a saturated upsampled primary.
    public static Fin<RgbSpectrum> SceneLinear(Unicolour colour, Op key, GamutMap strategy = GamutMap.OklchChromaReduction) =>
        (colour.IsInRgbGamut ? colour : colour.MapToRgbGamut(strategy)).RgbLinear.Triplet switch {
            var lin => RgbSpectrum.TryCreate(Math.Max(0.0, lin.First), Math.Max(0.0, lin.Second), Math.Max(0.0, lin.Third), out RgbSpectrum rgb)
                ? Fin.Succ(rgb)
                : Fin.Fail<RgbSpectrum>(MaterialFault.Gamut(key, "<non-finite-linear-rgb>")),
        };
}
```

## [03]-[TONE_MAP]

- Owner: `ToneOperator` `[SmartEnum<string>]` (aces · agx · pbr-neutral · reinhard · filmic · exposure), each row carrying its full-triple `Grade` operator; `DisplayEncoding` `[SmartEnum<string>]` the transfer-and-range egress rows (srgb · display-p3 · rec2020 · rec2100-pq · rec2100-hlg); `ToneMap` the static kernel table.
- Entry: `public static RgbSpectrum Apply(ToneOperator op, RgbSpectrum sceneLinear, double exposure)` tone-maps the integrator's HDR radiance to display-linear [0,1], and `public static (double R, double G, double B) Encode(RgbSpectrum displayLinear, DisplayEncoding target)` rebases the AP1 triple onto the row's pre-built `Configuration` then reads its transfer — two steps, one policy row each, no per-call knob.
- Packages: Wacton.Unicolour (composed for the encode after tone-map — `RgbConfiguration` transfer presets paired with `DynamicRange` per row), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new operator is one `ToneOperator` row carrying its `Grade` delegate — `Apply` reads the row through `[UseDelegateFromConstructor]` and never branches; a new display target is one `DisplayEncoding` row pairing a composed `RgbConfiguration` transfer preset with its `DynamicRange` — never an author-kernel, never a loose transfer-without-range parameter; zero new surface.
- Boundary: the tone curves are the documented Unicolour NOT_COVERED concern — the Narkowicz (2016) ACES-fit approximation of the combined RRT+ODT (the rational `(x(ax+b))/(x(cx+d)+e)` curve matched to the reference ACES sRGB ODT), the Sobotka AgX display transform (the Blender-default view transform, realized as the Wrensch minimal fit: the 3×3 inset matrix into log2 space clamped to `[−12.47393, 4.026069]`, the 6th-order sigmoid polynomial, the 3×3 outset matrix, and the `2.2`-exponent linearization back to display-linear — AgX mixes channels through its matrices, which is WHY the row column is the triple `Grade(RgbSpectrum)` and not a scalar curve; the per-channel operators ride `Map`), the Khronos PBR Neutral commerce transform (toe offset by the min channel — `x − 6.25x²` under `0.08` — rational peak compression past the `0.76` shoulder toward `1 − d²/(peak + d − 0.76)`, then desaturation toward the compressed peak by `1 − 1/(0.15·(peak − newPeak) + 1)` so clipped highlights stay neutral while every value below the shoulder passes UNCHANGED — the base-color-preserving property that makes it the material-fidelity default; channel-coupled through min/max, so it rides the triple `Grade` column like AgX), the Reinhard extended `x(1+x/Lwhite²)/(1+x)` global operator, the Hejl-Burgess-Dawson filmic curve, and the plain exposure-then-clamp; exposure is applied as a multiplicative scale before the curve (`scene · 2^exposure`); the OETF/transfer after tone-mapping is COMPOSED through Unicolour with the transfer AND the dynamic range as ONE `DisplayEncoding` row — the Rec2100 PQ/HLG transfers scale by `DynamicRange.WhiteLuminance`, so the HDR rows declare `DynamicRange.High` (203-nit reference white, 1000-nit max) and the SDR rows `DynamicRange.Standard` (100-nit) as an explicit column, never the package `Configuration` default — the omitted-argument default is `DynamicRange.High`, so an undeclared SDR row would silently encode at the 203-nit HDR scale — and each `Configuration` mints once per row, never per encode; the `rec2020` row is the wide-gamut SDR target the `RgbConfiguration.Rec2020` preset already carries (the BT.2020 primaries under the standard range, distinct from the PQ/HLG rows that pair the same primaries with the HDR transfers); `Encode` REBASES the display-linear triple from the `PortValue.SceneLinear` AP1 working space onto the target row's configuration through `ConvertToConfiguration` (XYZ-preserving) BEFORE reading the row's `.Rgb` transfer — an AP1-linear triple relabelled target-linear was the deleted form, the primary mismatch hue-shifting every P3/Rec2100 encode (the same cross-space grounding law `weathering#WEATHERING` and `finish#FINISH` enforce inbound); the tone-mapped display-referred output is the result the app-platform raster path (`Rasm.AppUi/Charts/custom#COLOR_SPACE`) consumes downstream, never a surface Materials reaches into. CHARTERED DUPLICATION (declared, not accidental): the `Rasm.AppUi/Render/capture` `ToneMap` (a SkiaSharp per-channel `float` `SKColorFilter` LUT on the raster-encode path, its `rec2020`/HDR targets the `ColorPolicy` rows) is a DISTINCT app-platform owner — one tone-map owner per runtime, the appearance-domain `ToneOperator`/`DisplayEncoding` here grounding path-traced `RgbSpectrum` radiance through Unicolour (the `rec2020` SDR row covering capture's `Rec2020` target inbound), the capture-time Skia curve there grounding chart/document raster export; the shared Narkowicz/Reinhard coefficients are two runtimes implementing one published curve at the wire, never drift, and neither owner references the other.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// Grade is the full-triple operator column: the scalar curves ride Map per channel; AgX mixes channels through its
// inset/outset matrices, so the column is RgbSpectrum→RgbSpectrum, never double→double.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ToneOperator {
    public static readonly ToneOperator Aces       = new("aces",        static s => s.Map(ToneMap.AcesFit));
    public static readonly ToneOperator Agx        = new("agx",         ToneMap.AgxGrade);
    public static readonly ToneOperator PbrNeutral = new("pbr-neutral", ToneMap.PbrNeutralGrade);
    public static readonly ToneOperator Reinhard   = new("reinhard",    static s => s.Map(ToneMap.ReinhardExtended));
    public static readonly ToneOperator Filmic     = new("filmic",      static s => s.Map(ToneMap.FilmicCurve));
    public static readonly ToneOperator Exposure   = new("exposure",    static s => s.Map(static x => Math.Clamp(x, 0.0, 1.0)));

    [UseDelegateFromConstructor]
    public partial RgbSpectrum Grade(RgbSpectrum sceneLinear);
}

// The display egress policy: transfer AND range travel as ONE row — the Rec2100 PQ/HLG transfers scale by
// DynamicRange.WhiteLuminance, so the range is a declared column (never the package Configuration default) and each
// Configuration mints once per row, never per encode.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DisplayEncoding {
    public static readonly DisplayEncoding Srgb       = new("srgb",        new Configuration(RgbConfiguration.StandardRgb, dynamicRange: DynamicRange.Standard));
    public static readonly DisplayEncoding DisplayP3  = new("display-p3",  new Configuration(RgbConfiguration.DisplayP3,  dynamicRange: DynamicRange.Standard));
    public static readonly DisplayEncoding Rec2020    = new("rec2020",     new Configuration(RgbConfiguration.Rec2020,    dynamicRange: DynamicRange.Standard));
    public static readonly DisplayEncoding Rec2100Pq  = new("rec2100-pq",  new Configuration(RgbConfiguration.Rec2100Pq,  dynamicRange: DynamicRange.High));
    public static readonly DisplayEncoding Rec2100Hlg = new("rec2100-hlg", new Configuration(RgbConfiguration.Rec2100Hlg, dynamicRange: DynamicRange.High));

    public Configuration Config { get; }
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ToneMap {
    public static RgbSpectrum Apply(ToneOperator op, RgbSpectrum sceneLinear, double exposure) =>
        op.Grade(sceneLinear.Scale(Math.Pow(2.0, exposure)));

    // Rebase AP1→target primaries (ConvertToConfiguration, XYZ-preserving) BEFORE the transfer read — display-linear
    // means target-linear; constructing the AP1 triple AS target.Config was the hue-shifting deleted form.
    public static (double R, double G, double B) Encode(RgbSpectrum displayLinear, DisplayEncoding target) =>
        new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, displayLinear.R, displayLinear.G, displayLinear.B)
            .ConvertToConfiguration(target.Config).Rgb.Triplet switch { var rgb => (rgb.First, rgb.Second, rgb.Third) };

    internal static double AcesFit(double x) {
        const double a = 2.51, b = 0.03, c = 2.43, d = 0.59, ee = 0.14;
        return Math.Clamp(x * (a * x + b) / (x * (c * x + d) + ee), 0.0, 1.0);
    }
    internal static double ReinhardExtended(double x) {
        const double lWhite = 4.0;
        return Math.Clamp(x * (1.0 + x / (lWhite * lWhite)) / (1.0 + x), 0.0, 1.0);
    }
    internal static double FilmicCurve(double x) {
        double v = Math.Max(0.0, x - 0.004);
        return (v * (6.2 * v + 0.5)) / (v * (6.2 * v + 1.7) + 0.06);
    }

    // Sobotka AgX by the Wrensch minimal fit: inset 3×3 → log2 [−12.47393, 4.026069] normalize → 6th-order sigmoid →
    // outset 3×3 → 2.2-exponent linearize (display-linear out; the transfer stays on Encode). log2(0) clamps to the
    // floor and negatives clamp before the exponent, so the chain is total on any HDR radiance.
    internal static RgbSpectrum AgxGrade(RgbSpectrum s) {
        double r = 0.842479062253094 * s.R + 0.0784335999999992 * s.G + 0.0792237451477643 * s.B;
        double g = 0.0423282422610123 * s.R + 0.878468636469772 * s.G + 0.0791661274605434 * s.B;
        double b = 0.0423756549057051 * s.R + 0.0784336 * s.G + 0.879142973793104 * s.B;
        (r, g, b) = (Sigmoid(r), Sigmoid(g), Sigmoid(b));
        return RgbSpectrum.Create(
            Math.Pow(Math.Clamp(1.19687900512017 * r - 0.0980208811401368 * g - 0.0990297440797205 * b, 0.0, 1.0), 2.2),
            Math.Pow(Math.Clamp(-0.0528968517574562 * r + 1.15190312990417 * g - 0.0989611768448433 * b, 0.0, 1.0), 2.2),
            Math.Pow(Math.Clamp(-0.0529716355144438 * r - 0.0980434501171241 * g + 1.15107367264116 * b, 0.0, 1.0), 2.2));

        static double Sigmoid(double v) {
            const double minEv = -12.47393, maxEv = 4.026069;
            double x = Math.Clamp((Math.Log2(Math.Max(v, double.Epsilon)) - minEv) / (maxEv - minEv), 0.0, 1.0);
            double x2 = x * x, x4 = x2 * x2;
            return 15.5 * x4 * x2 - 40.14 * x4 * x + 31.96 * x4 - 6.868 * x2 * x + 0.4298 * x2 + 0.1191 * x - 0.00232;
        }
    }

    // Khronos PBR Neutral: min-channel toe offset, rational shoulder past 0.76, desaturation toward the compressed
    // peak — values below the shoulder pass UNCHANGED (the base-color-preserving law). Channel-coupled via min/max.
    internal static RgbSpectrum PbrNeutralGrade(RgbSpectrum s) {
        const double startCompression = 0.8 - 0.04, desaturation = 0.15;
        double x = Math.Min(s.R, Math.Min(s.G, s.B));
        double offset = x < 0.08 ? x - 6.25 * x * x : 0.04;
        (double r, double g, double b) = (s.R - offset, s.G - offset, s.B - offset);
        double peak = Math.Max(r, Math.Max(g, b));
        if (peak < startCompression) { return RgbSpectrum.Create(Math.Max(0.0, r), Math.Max(0.0, g), Math.Max(0.0, b)); }
        const double d = 1.0 - startCompression;
        double newPeak = 1.0 - d * d / (peak + d - startCompression);
        double scale = newPeak / peak, mix = 1.0 - 1.0 / (desaturation * (peak - newPeak) + 1.0);
        return RgbSpectrum.Create(
            Math.Clamp(r * scale + (newPeak - r * scale) * mix, 0.0, 1.0),
            Math.Clamp(g * scale + (newPeak - g * scale) * mix, 0.0, 1.0),
            Math.Clamp(b * scale + (newPeak - b * scale) * mix, 0.0, 1.0));
    }
}
```

## [04]-[CONDUCTOR_IOR]

- Owner: `ConductorMetal` `[SmartEnum<string>]` the measured-metal axis whose rows CARRY the per-band `ComplexIor` (the data lives with the vocabulary that selects it — a parallel metal→IOR dictionary beside the axis is the deleted form, and a parallel `ConductorIor` operations class forwarding `.Ior` reads is the deleted form too: the row IS the grounding surface).
- Entry: the row's `Ior` property reads the measured complex refractive index per RGB band as one `ComplexIor` carrier (its `Eta`/`K` two validated `RgbSpectrum` bands) — `SlabStack.LowerBase` constructs the grounded `bsdf#LOBE_FAMILY` `Conductor` lobe as `new BsdfLobe.Conductor(metal.Ior, alphaX, alphaY)` directly; the metal F0 is the measured `ComplexIor` Fresnel, NEVER a hand-authored RGB albedo scaled to a guess; the normal-incidence Fresnel is the carrier's own `ComplexIor.FresnelNormal`, never a local re-derivation; `public static Option<ConductorMetal> Resolve(string family, string name)` is the one static operation on the axis — the wire-boundary resolve `interchange#MATERIAL_WIRE` feeds with the split library id.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new measured metal is one `ConductorMetal` row carrying its three-band `(η, k)` measured pair from the published refractive-index table; the row set is the INTERNAL leg of `graph#MEASURED_SPECTRAL_LIBRARY` — the conductor rows ground here rather than carrying a hand-authored Acescg albedo. A spectral 195-wavelength conductor curve (EPFL RGL goniophotometer or a full `refractiveindex.info` n/k spectrum) is the [UPSTREAM-BLOCKED] extension that admits through `[02]-[SPECTRAL_UPSAMPLE]` `ToSpd` per band once a managed `.bsdf`/spectral reader lands at `acquisition#EPFL_RGL_BRDF_LOADER` and the decoded conductor curve arrives over the host-free-peer / host-edge wire (NOT a Rasm.Compute project reference — the acyclic strata forbids the AEC→app-platform edge); zero new surface.
- Boundary: the complex refractive index `(η, k)` per RGB band is the physically-correct conductor F0 carried as one `ComplexIor` `[ComplexValueObject]` band — the `bsdf#MICROFACET_KERNEL` `Microfacet.FresnelConductor(cosI, ComplexIor ior)` overload reads it directly, so a metal's edge tint and grazing-angle hue shift emerge from the measured dispersion rather than an artist's base-color triple; the three-band `Eta`/`K` values transcribe the Johnson-Christy / `refractiveindex.info` measured dataset at the RGB band centres `SpectralBand.Red`/`Green`/`Blue` carry (610/550/465 nm sampled against the published 630/532/465 nm anchors); the `graph#MATERIAL_LIBRARY` conductor rows carry a measured `BaseColor` for the diffuse-substitute preview path AND name a `ConductorMetal` so the `bsdf#LOBE_FAMILY` `Conductor` lobe grounds from the named row's `Ior`, the base color the perceptual seed and the `(η, k)` the shading truth; the smart-enum KEYS align to the `graph#MATERIAL_LIBRARY` register's `metal.<name>` name column — `"chrome"` (never `"chromium"`) so `Resolve("metal", "chrome")` grounds the register's `metal.chrome` row instead of silently falling to the interchange `Iron` default; a metal absent from the rows falls back to the `graph#MATERIAL_LIBRARY` base-color-as-F0 dielectric-Schlick approximation rather than faulting — the register's `metal.steel` alloy row has no published `(η, k)` dataset and shades through exactly that fallback — so the rows ground the eight named metals and a ninth admits without a rebuild; the conductor F0 round-trips in-gamut through the `bsdf#WHITE_FURNACE_HARNESS` lossless-conductor furnace (F≡1 reflects unit energy) so a measured metal conserves energy under the Kulla-Conty multi-scatter term.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The measured complex refractive index RIDES the metal row — three-band (η, k) transcribed from the Johnson-Christy /
// refractiveindex.info datasets at the SpectralBand 610/550/465 nm centres. Keys match the graph#MATERIAL_LIBRARY
// register name column ("chrome", never "chromium"): a register/key drift silently downgrades the interchange resolve
// to the Iron default, so the register spelling is the law here.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConductorMetal {
    public static readonly ConductorMetal Gold     = new("gold",     ComplexIor.Create(RgbSpectrum.Create(0.183, 0.421, 1.373), RgbSpectrum.Create(3.424, 2.346, 1.770)));
    public static readonly ConductorMetal Copper   = new("copper",   ComplexIor.Create(RgbSpectrum.Create(0.271, 0.677, 1.316), RgbSpectrum.Create(3.609, 2.625, 2.292)));
    public static readonly ConductorMetal Aluminum = new("aluminum", ComplexIor.Create(RgbSpectrum.Create(1.346, 0.965, 0.617), RgbSpectrum.Create(7.475, 6.400, 5.303)));
    public static readonly ConductorMetal Silver   = new("silver",   ComplexIor.Create(RgbSpectrum.Create(0.159, 0.145, 0.135), RgbSpectrum.Create(3.929, 3.190, 2.381)));
    public static readonly ConductorMetal Iron     = new("iron",     ComplexIor.Create(RgbSpectrum.Create(2.911, 2.950, 2.585), RgbSpectrum.Create(3.089, 2.932, 2.767)));
    public static readonly ConductorMetal Chrome   = new("chrome",   ComplexIor.Create(RgbSpectrum.Create(2.020, 2.790, 2.020), RgbSpectrum.Create(3.860, 4.200, 3.860)));
    public static readonly ConductorMetal Titanium = new("titanium", ComplexIor.Create(RgbSpectrum.Create(2.741, 2.542, 2.267), RgbSpectrum.Create(3.814, 3.435, 3.039)));
    public static readonly ConductorMetal Brass    = new("brass",    ComplexIor.Create(RgbSpectrum.Create(0.444, 0.527, 1.094), RgbSpectrum.Create(3.695, 2.765, 1.829)));

    public ComplexIor Ior { get; }

    // The wire-boundary resolve interchange#MATERIAL_WIRE feeds with the split library id ("metal.chrome" → family
    // "metal", name "chrome"); the family token is the graph register's metal.* prefix. A miss shades through the
    // graph#MATERIAL_LIBRARY base-color-as-F0 dielectric fallback (metal.steel carries no published (η, k) dataset).
    public static Option<ConductorMetal> Resolve(string family, string name) =>
        family == "metal" && TryGet(name, out ConductorMetal? metal) ? Optional(metal) : Option<ConductorMetal>.None;
}
```

## [05]-[OPENPBR_SLAB]

- Owner: `Slab` `[Union]` the closed slab family (fuzz · coat · emission · base — its four cases ARE the slab discriminant, no parallel kind enum re-describing them); `SlabStack` the outermost-to-base layering algebra; `OpenPbrSurface` the OpenPBR parameter vector AND the one `MaterialParameters`→OpenPBR lowering (`OpenPbrSurface.Of`).
- Entry: `public static OpenPbrSurface Of(MaterialParameters p, ConductorMetal conductor)` is the SINGLE `MaterialParameters`→OpenPBR column correspondence — the standard-column vector both this stack and the `interchange#MATERIAL_WIRE` generated `WireMap.ToWire` transcription read, so the mapping is declared once and never re-minted at the wire; `public static SlabStack Lower(OpenPbrSurface surface)` derives the formal OpenPBR Surface 1.1 stack from that vector and `public static SlabStack Lower(MaterialParameters p, ConductorMetal conductor)` is the convenience overload composing `Of` then `Lower` (the row names its `ConductorMetal` through `[04]-[CONDUCTOR_IOR]`); `public Fin<LayeredBsdf> ToLayered(Op key)` collapses the stack to the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` weighted-lobe fold the integrator shades — the vector IS the canonical lowering, the stack the composition law, the weighted fold its energy-preserving collapse, so the renderer reads one `LayeredBsdf`.
- Packages: Rasm (project — `UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new layering modifier is one `Slab` case carrying its albedo-scaling operator (the fuzz slab is the new closed lobe case the seven-lobe set lacked, realized as the `bsdf#LOBE_FAMILY` `Sheen` lobe at the fuzz position; the iridescent topcoat is the realized `Coat`→`bsdf#LOBE_FAMILY` `ThinFilm` lowering when the coat carries the row's `ThinFilm` film, NOT a parallel owner); a new OpenPBR parameter is one `OpenPbrSurface` column `Of` populates and `Lower` reads — the standard column set (`base`, `specular`, `transmission`, `subsurface`, `coat`, `fuzz`, `thin_film`, `emission`) the `graph#MATERIAL_LIBRARY` `MaterialParameters` aligns to — the `geometry` group (opacity, normal, tangent) stays on the `graph#MATERIAL_GRAPH` node fold, never vector columns; zero new surface. The `interchange#MATERIAL_WIRE` `MaterialWire` is the OpenPBR-vector wire projection this stack's `OpenPbrSurface` defines and the TS/Py consumers decode — `interchange` composes `OpenPbrSurface.Of`, never a second `MaterialParameters`→OpenPBR mint.
- Law: the slab stack is the formal OpenPBR Surface 1.1 layering order outermost-to-base — `fuzz` over `coat` over `emission` over the `base` substrate — composed by albedo-scaling layering operators, NOT the additive convex-combination weight fold `bsdf#LAYERED_COMPOSITION` predated: `RemainingEnergy` cascades `pass ← pass · (1 − w · E(slab))` over the placed outer slabs, where `E(slab)` is that slab's lobe-specific normal-incidence directional albedo from `bsdf#LOBE_FAMILY` `MultiScatter.DirectionalAlbedo` at the lobe's OWN GGX alpha (`LobeAlpha` reads `AlphaX`/`AlphaY` per case), so a rough coat occludes more of the base than a smooth one and the energy split is a function of the real lobe, never a fixed constant; the base substrate is the FULL OpenPBR mixing chain lowered to fold rows — metalness splits conductor (grounded from the `[04]-[CONDUCTOR_IOR]` rows) vs dielectric, `transmission` splits the dielectric into the transmissive interface vs the opaque body, `subsurface` splits the body into Burley diffusion vs glossy-diffuse, and glossy-diffuse is the reflect-only parameterized-IOR specular over the diffuse floor at the F0-scaled multi-scatter albedo split — every fractional weight is rows of the one fold with the 0/1 ends collapsing, never a winner-take-all gate.
- Boundary: `OpenPbrSurface.Of` is the ONE `MaterialParameters`→OpenPBR construction and `SlabStack.Lower` the ONE vector→slab lowering — a per-material slab builder or a second wire-side OpenPBR mint is the deleted form; the fuzz slab lowers to a `Sheen` lobe weighted by `fuzz_weight` and colored by the `fuzz_color` column `Of` mints as the white→base lerp at `MaterialParameters.SheenTint` (the Disney sheen-tint law: white at tint 0 — the OpenPBR `fuzz_color` default — the base hue at tint 1, so the velvet/silk/denim rows' authored tints reach the slab and the wire instead of being dropped at the lowering), the coat slab to its `thin_film_weight` MIX — the plain `Clearcoat` dielectric and the `bsdf#LOBE_FAMILY` `ThinFilm` interference lobe (film thickness/IOR from the coat's validated `ThinFilm` carrier, the coat IOR lifted into a real `ComplexIor` base — the Belcour-Barla spectral interference the `finish#COAT_STACK_THIN_FILM` pearlescent topcoat drives) split by the film weight as two rows of the one weighted fold, the 0/1 ends collapsing to a single lobe; the film columns SOURCE from `MaterialParameters.Film` through `Of` (a hardcoded zero triple was the deleted form — it dead-ended this lowering and shipped permanent zeros on the `interchange#MATERIAL_WIRE` `thin_film` columns), the emission slab to the `graph#MATERIAL_GRAPH` emission carrier (energy-additive, never occluding), and the base substrate to the metalness → transmission → subsurface mixing chain — a `Conductor` lobe (grounded from the `ConductorMetal` the row names through `[04]-[CONDUCTOR_IOR]`) against the dielectric rows, the transmissive `Dielectric` against the opaque body, the `Subsurface` diffusion against the glossy-diffuse pair (the reflect-only `Dielectric` specular at `SpecularIor` over the `Diffuse` floor), each fractional weight two rows of the one fold — the `Subsurface` lobe reading the validated three-band `SubsurfaceRadius` `[ComplexValueObject]` carrier's `Magnitude` for the Burley diffusion radius (the carrier declared on `graph#MATERIAL_LIBRARY` `MaterialParameters`, gating a negative or non-finite millimetre mean-free-path once at `Create` so no `Vector3d` scatter vector threads the slab signatures); `ToLayered` collapses the albedo-scaled slab weights into the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf.Of` normalized lobe list so the integrator shades one `LayeredBsdf` and never re-derives the slab nesting per sample — the per-lobe albedo-scaling is computed once at lowering, the energy each outer slab leaves for the layer below (`1 − w · E(slab)` at the lobe's own alpha) baked into the lobe weight; the OpenPBR z-up local-frame convention matches the `bsdf#SHADING_FRAME` `LocalVector` basis so no slab re-derives `cosθ`; slab-weight admission is TOTAL — `Of` clamps the OpenPBR columns and the `Weight` helper collapses every `w · pass` into `[0,1]` before `UnitInterval.Create` through a comparison-ordered floor (`Math.Clamp` propagates NaN, so a non-finite consumer weight lands at zero rather than throwing), so no `Slab` weight throws the value-object guard mid-fold, and the one fault site is `ToLayered`→`LayeredBsdf.Of` railing `MaterialFault.Parameter` when every lobe weight filters to zero (a degenerate empty stack), never a propagated energy gain; the `weathering#WEATHERING` aging operator targets the slab columns directly once lowered (chalking raises `coat_roughness`, soiling raises `fuzz_weight`, patina greens the `Slab.Base` color and drops its `Metalness` toward zero — the conductor corrodes to a dielectric verdigris, never a metal-to-metal `ConductorMetal` swap the 8-member smart-enum cannot represent) through the `SlabColumnDelta` carried on each `WeatheringEffect` policy row, the slab columns the shared aging target.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The closed slab family IS the slab discriminant — the four cases ARE the axis, so SlabStack.Lower/LowerSlab/ToLayered
// dispatch on the Slab Switch arms directly. A parallel SlabKind [SmartEnum] re-describing this same closed set with no
// independent row data and no reader is the rejected parallel-discriminant form (the discriminant is recoverable from
// the value itself); the slab order is carried structurally by SlabStack.Lower's outermost-to-base Add sequence.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Slab {
    private Slab() { }

    public sealed record Fuzz(double Weight, RgbSpectrum Color, double Roughness) : Slab;
    public sealed record Coat(double Weight, RgbSpectrum Color, double Roughness, double Ior, ThinFilm Film) : Slab;   // Film is the graph#MATERIAL_LIBRARY validated thin_film carrier; ThinFilm.None the film-free zero — never a loose (Option<double>, double) pair re-describing it
    public sealed record Emission(RgbSpectrum Radiance, double Luminance) : Slab;
    public sealed record Base(double Metalness, ConductorMetal Conductor, RgbSpectrum BaseColor, double Roughness, double SpecularIor, double Anisotropy, double Transmission, double TransmissionRoughness, double Subsurface, SubsurfaceRadius SubsurfaceRadius) : Slab;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The OpenPBR Surface 1.1 parameter vector: the ONE MaterialParameters->OpenPBR correspondence (Of) every consumer
// reads. SlabStack.Lower derives the slab family FROM this vector and interchange#MATERIAL_WIRE projects OpenPbrGroupsWire
// FROM it, so the column mapping is declared once here and never re-minted at the wire (the DERIVED_LOGIC primary).
public readonly record struct OpenPbrSurface(
    double BaseWeight, RgbSpectrum BaseColor, double BaseMetalness, double BaseDiffuseRoughness, double BaseSpecularTint,
    double SpecularWeight, double SpecularRoughness, double SpecularIor, double SpecularAnisotropy,
    double TransmissionWeight, double TransmissionRoughness,
    double SubsurfaceWeight, SubsurfaceRadius SubsurfaceRadius,
    double CoatWeight, double CoatRoughness, double CoatIor,
    double FuzzWeight, double FuzzRoughness, RgbSpectrum FuzzColor,
    double ThinFilmWeight, double ThinFilmThickness, double ThinFilmIor,
    RgbSpectrum EmissionColor, double EmissionLuminance,
    ConductorMetal Conductor) {

    // BaseSpecularTint reads MaterialParameters.SpecularTint — the Disney specular-tint column feeding the frozen
    // interchange#MATERIAL_WIRE wire column; MaterialX 1.39 open_pbr_surface has NO base_specular_tint input, so the
    // .mtlx egress lowers this tint INTO specular_color (the interchange TintedSpecular specular→base lerp).
    // FuzzColor mints from MaterialParameters.SheenTint as the white→base Disney sheen-tint lerp (the OpenPBR
    // fuzz_color: white at tint 0, the base hue at tint 1) — both authored tints reach the vector and the
    // interchange#MATERIAL_WIRE wire. The thin_film group reads the row's validated ThinFilm carrier (p.Film, the
    // graph#MATERIAL_LIBRARY init-defaulted column, ThinFilm.None on a film-free row) — a hardcoded zero triple here
    // would dead-end the Coat film lowering AND ship permanent zeros on the wire's Key(23..25) columns. SpecularColor/
    // CoatColor stay synthesized White (the OpenPBR neutral-tint baseline) because MaterialParameters carries no
    // specular/coat colour source — a column with no source is speculative; the tint scalars and the film HAVE sources.
    // CoatIor synthesizes the OpenPBR coat_ior default 1.6 (no MaterialParameters coat-IOR column; 1.5 was the Disney
    // clearcoat default mislabelled as the OpenPBR one).
    public static OpenPbrSurface Of(MaterialParameters p, ConductorMetal conductor) =>
        new(BaseWeight: 1.0, AcescgRgb(p.BaseColor), p.Metalness, p.Roughness, Math.Clamp(p.SpecularTint, 0.0, 1.0),
            SpecularWeight: 1.0, p.Roughness, p.Ior, p.Anisotropy,
            p.Transmission, p.TransmissionRoughness,
            p.Subsurface, p.SubsurfaceRadius,
            p.Clearcoat, p.ClearcoatRoughness, CoatIor: 1.6,
            p.Sheen, FuzzRoughness: Math.Max(1e-3, p.Roughness),
            FuzzColor: RgbSpectrum.White.Lerp(AcescgRgb(p.BaseColor), Math.Clamp(p.SheenTint, 0.0, 1.0)),
            ThinFilmWeight: p.Film.Weight, ThinFilmThickness: p.Film.ThicknessNm, ThinFilmIor: p.Film.Ior,
            AcescgRgb(p.Emission), p.EmissionLuminance,
            conductor);

    internal static RgbSpectrum AcescgRgb(Unicolour colour) =>
        colour.RgbLinear switch { var lin => RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)) };
}

public sealed record SlabStack(Seq<Slab> Slabs) {
    // The convenience entry the library/finish drive: lower the row to the canonical OpenPbrSurface vector, then to slabs.
    public static SlabStack Lower(MaterialParameters p, ConductorMetal conductor) => Lower(OpenPbrSurface.Of(p, conductor));

    // The ONE OpenPBR construction: the outermost-to-base slab order derived from the vector columns. fuzz over coat over
    // emission over base; the coat re-mints the vector's thin_film columns into the validated ThinFilm carrier (the
    // clamps keep Lower total on a consumer-built vector — the same admission posture as Weight) so an iridescent
    // topcoat rides the existing Coat slab rather than a parallel owner; the base names its ConductorMetal for [04].
    public static SlabStack Lower(OpenPbrSurface s) =>
        new(Seq<Slab>()
            .Add(new Slab.Fuzz(s.FuzzWeight, s.FuzzColor, s.FuzzRoughness))
            .Add(new Slab.Coat(s.CoatWeight, RgbSpectrum.White, s.CoatRoughness, s.CoatIor, ThinFilm.Create(Math.Clamp(s.ThinFilmWeight, 0.0, 1.0), Math.Max(0.0, s.ThinFilmThickness), Math.Max(1.0, s.ThinFilmIor))))
            .Add(new Slab.Emission(s.EmissionColor, s.EmissionLuminance))
            .Add(new Slab.Base(s.BaseMetalness, s.Conductor, s.BaseColor, s.SpecularRoughness, s.SpecularIor, s.SpecularAnisotropy, s.TransmissionWeight, s.TransmissionRoughness, s.SubsurfaceWeight, s.SubsurfaceRadius)));

    // Energy-preserving collapse to the bsdf#LAYERED_COMPOSITION fold: each slab transmits 1-E(slab) of the energy below
    // it (E the slab's own directional albedo from bsdf#LOBE_FAMILY MultiScatter.DirectionalAlbedo at the slab's roughness),
    // so the outermost fuzz/coat attenuate the layers below ONCE at lowering and the integrator shades one LayeredBsdf.
    public Fin<LayeredBsdf> ToLayered(Op key) =>
        LayeredBsdf.Of(Slabs.Fold(Seq<LobeWeight>(), static (acc, slab) => acc + LowerSlab(slab, RemainingEnergy(acc))).Filter(static l => l.Weight.Value > 0.0), key);

    // The energy the placed outer slabs leave for the layer below: each lobe attenuates by its OWN directional albedo at
    // the lobe's roughness (the slab's alpha), not a fixed constant, so a rough coat occludes more than a smooth one.
    private static double RemainingEnergy(Seq<LobeWeight> placed) =>
        placed.Fold(1.0, static (pass, lw) => Math.Clamp(pass * (1.0 - lw.Weight.Value * MultiScatter.DirectionalAlbedo(LobeAlpha(lw.Lobe), 1.0)), 0.0, 1.0));

    // The single-scatter alpha a lobe occludes at: the GGX alpha of the lobe's roughness, the diffuse/subsurface full-rough.
    // Clearcoat/ThinFilm bind the case's own static projections as bare method groups (a `static` lambda modifier on a
    // method group is not C#; method groups capture nothing, so the closure-free law holds without it).
    private static double LobeAlpha(BsdfLobe lobe) => lobe.Switch(
        diffuse:   static _ => 1.0,
        conductor: static c => Math.Max(c.AlphaX, c.AlphaY),
        dielectric: static g => Math.Max(g.AlphaX, g.AlphaY),
        sheen:     static s => Microfacet.AlphaOf(s.Roughness),
        clearcoat: BsdfLobe.Clearcoat.Alpha,
        subsurface: static _ => 1.0,
        thinFilm:  BsdfLobe.ThinFilm.AlphaX);

    // The fuzz lowers to a Sheen lobe, the coat to its film-weight mix; both attenuated by the energy the outer layers leave.
    private static Seq<LobeWeight> LowerSlab(Slab slab, double pass) => slab.Switch(
        fuzz:     f => f.Weight > 0.0 ? Seq(new LobeWeight(new BsdfLobe.Sheen(f.Color, f.Roughness), Weight(f.Weight, pass))) : Seq<LobeWeight>(),
        coat:     c => c.Weight > 0.0 ? CoatLobes(c, pass) : Seq<LobeWeight>(),
        emission: static _ => Seq<LobeWeight>(),
        @base:    b => LowerBase(b, pass));

    // OpenPBR thin_film_weight is a MIX weight (film coverage over the coat interface), never a >0 gate: a fractional
    // film splits the coat between the plain Clearcoat dielectric and the Belcour-Barla ThinFilm interference lobe
    // (the coat IOR lifted into a real ComplexIor base) as two rows of the one weighted fold; 0 and 1 collapse to the
    // single-lobe ends, so the mix costs nothing at the common extremes.
    private static Seq<LobeWeight> CoatLobes(Slab.Coat c, double pass) {
        double fw = c.Film.Weight;
        BsdfLobe clear = new BsdfLobe.Clearcoat(c.Weight, c.Roughness);
        BsdfLobe film = new BsdfLobe.ThinFilm(c.Film.ThicknessNm, c.Film.Ior, c.Roughness, ComplexIor.Create(RgbSpectrum.Create(c.Ior, c.Ior, c.Ior), RgbSpectrum.Black));
        return fw <= 0.0 ? Seq(new LobeWeight(clear, Weight(c.Weight, pass)))
             : fw >= 1.0 ? Seq(new LobeWeight(film, Weight(c.Weight, pass)))
             : Seq(new LobeWeight(clear, Weight(c.Weight * (1.0 - fw), pass)), new LobeWeight(film, Weight(c.Weight * fw, pass)));
    }

    // The OpenPBR base MIXING CHAIN as fold rows: metalness splits conductor vs dielectric; transmission_weight splits
    // the dielectric into the transmissive interface vs the opaque body; subsurface_weight splits the body into Burley
    // diffusion vs glossy-diffuse; glossy-diffuse is the reflect-only dielectric specular (Transmittance Black — the
    // transmit arm evaluates to zero, so the parameterized-IOR reflection needs no bespoke lobe case) over the diffuse
    // floor, split by the F0-scaled multi-scatter albedo — the same 1−E law the outer slabs cascade. Every fractional
    // weight is rows of the ONE fold with the 0/1 ends collapsing (the CoatLobes mix law): jade at subsurface 0.6 and
    // oil at transmission 0.9 lower to the authored mixes, and plastics keep their Fresnel specular instead of
    // rendering matte — the winner-take-all >0 gates that dropped the SpecularIor/SpecularRoughness columns on every
    // opaque dielectric were the deleted form.
    private static Seq<LobeWeight> LowerBase(Slab.Base b, double pass) {
        double alpha = Microfacet.AlphaOf(b.Roughness), transmissionAlpha = Microfacet.AlphaOf(b.TransmissionRoughness);
        double metalness = Math.Clamp(b.Metalness, 0.0, 1.0);
        double transmission = Math.Clamp(b.Transmission, 0.0, 1.0), subsurface = Math.Clamp(b.Subsurface, 0.0, 1.0);
        double f0 = Math.Pow((b.SpecularIor - 1.0) / (b.SpecularIor + 1.0), 2.0);
        double specularShare = Math.Clamp(f0 * MultiScatter.DirectionalAlbedo(alpha, 1.0), 0.0, 1.0);
        double opaque = (1.0 - metalness) * (1.0 - transmission);
        Seq<LobeWeight> Row(BsdfLobe lobe, double w) => w > 0.0 ? Seq(new LobeWeight(lobe, Weight(w, pass))) : Seq<LobeWeight>();
        return Row(new BsdfLobe.Conductor(b.Conductor.Ior, alpha, alpha), metalness)
             + Row(new BsdfLobe.Dielectric(b.SpecularIor, transmissionAlpha, transmissionAlpha, b.BaseColor), (1.0 - metalness) * transmission)
             + Row(new BsdfLobe.Subsurface(b.BaseColor, b.SubsurfaceRadius.Magnitude), opaque * subsurface)
             + Row(new BsdfLobe.Dielectric(b.SpecularIor, alpha, alpha, RgbSpectrum.Black), opaque * (1.0 - subsurface) * specularShare)
             + Row(new BsdfLobe.Diffuse(b.BaseColor, b.Roughness), opaque * (1.0 - subsurface) * (1.0 - specularShare));
    }

    // The one weight-admission helper: every slab weight collapses into [0,1] before UnitInterval.Create so a consumer-built
    // Slab carrying an out-of-band weight never throws the ValueObject guard mid-fold. Comparison-ordered, not Math.Clamp —
    // Clamp propagates NaN, so a non-finite weight (NaN roughness poisoning the albedo cascade) collapses to zero instead.
    private static UnitInterval Weight(double w, double pass) =>
        UnitInterval.Create(w * pass is var scaled && scaled >= 0.0 ? Math.Min(scaled, 1.0) : 0.0);
}
```

## [06]-[RESEARCH]

- [SPECTRAL_GROUNDING]: the RGB→SPD Smits seven-basis kernel + the Unicolour `Spd`→XYZ composition + the measured-illuminant reduction is the one spectral grounding owner every base color, conductor, and `finish#FINISH` pigment grounds through — the `acquisition#ACQUISITION` `GroundSpectral` composes this owner's `ToSpd` + `new Unicolour(Configuration, Spd)` RGB→SPD path AND `SceneLinear`, while `finish#FINISH` `FinishMix.Reflectance` composes ONLY `SceneLinear` over its Kubelka-Munk pigment-mix reflectance (the `new Unicolour(Configuration, Pigment[], double[])` ctor owns the mix, never the `Spd`/`ToSpd` upsample) — both fold through this one `SceneLinear` grounding owner rather than N parallel inline spectral-construction sites. The growth leg is the full Unicolour `Spd`/`SpectralCoefficients`/`Observer` (Degree2/Degree10) / `Illuminant` (the A/C/D50-D75/E/F-series statics) measured-illuminant reduction so a base color round-trips through a witnessed reflectance SPD under a named illuminant and clears the `graph#MATERIAL_LIBRARY` `SpectralAdmit` MacAdam spectral-locus bound, framed at `graph#MEASURED_SPECTRAL_LIBRARY`.
- [WHITE_FURNACE_HARNESS] (tier-2): the energy-conservation harness lives on the kernel page (`bsdf#WHITE_FURNACE_HARNESS`) since it samples `bsdf#LOBE_FAMILY` `BsdfLobe.Sample` and bounds the furnace residual against the lobe set; the surface page's `ConductorMetal` rows and `SlabStack.ToLayered` lowering feed that harness — a measured conductor F0 round-trips in-gamut through the lossless-conductor furnace and a lowered `SlabStack` collapses to an energy-bounded `LayeredBsdf`; the ΔE-cost residual resolves by running `SceneLinear` under BOTH admitted strategies on a saturated upsampled primary — `GamutMap.OklchChromaReduction` against `GamutMap.WxyPurityReduction` — and comparing each mapped reflectance's `Difference(reference, DeltaE.Ciede2000)`, the strategy parameter existing exactly so the comparison needs no second grounding entry.
- [CONDUCTOR_SPECTRAL_CURVE]: the per-band `(η, k)` row set is the realized INTERNAL leg; the full per-wavelength conductor curve is the [UPSTREAM-BLOCKED] extension awaiting the host-free-peer / host-edge spectral-reconstruction inference run delivering the decoded curve over the wire (NOT a Rasm.Compute project reference — the acyclic strata forbids the AEC-domain Materials a reference to the app-platform Compute ONNX owner), the decoded curve admitting through `[02]-[SPECTRAL_UPSAMPLE]` `ToSpd` per band. Until that lands the rows ground the three measured RGB-band anchors, the spectral inference the upstream-gated leg over a realized three-band fast path; framed at `graph#MEASURED_SPECTRAL_LIBRARY`.

# [MATERIALS_SURFACE]

THE COLOR-SCIENCE LOWERING and THE OPENPBR-CONSTRUCTION HALF. The surface page owns the four lowering/grounding kernels the wire and the library drive — ONE `SpectralUpsample` RGB→SPD kernel feeding Unicolour's `Spd`→XYZ and the measured-illuminant reduction every base color grounds through, ONE `ToneMap` ACES RRT/ODT + scene-referred operator table, ONE `ConductorIor` measured complex-IOR table grounding every metal F0 per band, and ONE `SlabStack` OpenPBR Surface 1.1 stack-of-slabs the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` fold lowers from. The page owns the `SpectralBand`, `ToneOperator`, and `ConductorMetal` axes, the `Slab` `[Union]` closed family (its own discriminant — no parallel kind enum), and the spectral/tone/conductor/slab kernels; it COMPOSES the `bsdf#SHADING_FRAME` `MaterialFault` (band 2450) declared once on the kernel page, the `bsdf#LOBE_FAMILY` `RgbSpectrum`/`ComplexIor` validated carriers and `BsdfLobe` closed set, and the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf.Of` weighted-lobe fold — the kernel page owns frame-local shading, this page owns the OpenPBR construction the `interchange#MATERIAL_WIRE` and the `weathering#WEATHERING` aging trajectory target.

This page is the lowering boundary: a `graph#MATERIAL_LIBRARY` `MaterialParameters` row lowers through the one `OpenPbrSurface.Of` column correspondence to the canonical OpenPBR vector (the SAME vector `interchange#MATERIAL_WIRE` projects, never re-minted at the wire), `SlabStack.Lower` derives the formal OpenPBR Surface 1.1 stack from it (fuzz over coat over emission over base), the base substrate grounds its conductor lobe from `ConductorIor`, the base color upsamples to an SPD through `SpectralUpsample`, and `SlabStack.ToLayered` collapses the albedo-scaled stack to the one `LayeredBsdf` weighted fold the renderer shades — so the OpenPBR vector is the canonical lowering, the slab algebra the construction the row drives through, the lobe math single-sourced on the kernel page, and the tone-map the display-referred egress the raster path consumes. The split is BY CONCERN, not by size: the kernel page carries the per-sample shading math the path tracer drives, this page the color-science and OpenPBR-construction the wire and library drive, the two sharing the `MaterialFault` band declared once on the kernel.

## [01]-[INDEX]

- [01]-[SPECTRAL_UPSAMPLE]: the RGB→SPD coefficient kernel, the Unicolour `Spd`→XYZ composition, the measured-illuminant reduction, and scene-linear admission.
- [02]-[TONE_MAP]: the ACES RRT/ODT author-kernel, the scene-referred filmic/Reinhard/exposure operators, and the `ToneOperator` table.
- [03]-[CONDUCTOR_IOR]: the `ConductorMetal` axis, the `ConductorIor` measured complex-IOR table per RGB band, and the `Conductor` lobe grounding.
- [04]-[OPENPBR_SLAB]: the `Slab` `[Union]` closed family (its four cases the slab discriminant, no parallel kind enum), the `OpenPbrSurface` vector and its one `Of` `MaterialParameters`→OpenPBR lowering, the `SlabStack` outermost-to-base layering algebra, and the `ToLayered` collapse the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` fold consumes.

## [02]-[SPECTRAL_UPSAMPLE]

- Owner: `SpectralUpsample` author-kernel; composes the `bsdf#SHADING_FRAME` `SpectralBand` `[SmartEnum<string>]` band-centre vocabulary (declared once on the kernel page, read by the thin-film lobe and the spectral curve) and Wacton.Unicolour for every conversion Unicolour already owns.
- Entry: `public static Fin<Spd> ToSpd(RgbSpectrum rgb, Op key)` and `public static Fin<RgbSpectrum> SceneLinear(Unicolour colour, Op key)` — RGB→SPD is the author-kernel Unicolour lacks (NOT_COVERED); SceneLinear COMPOSES `RgbConfiguration.Acescg` and Unicolour's `.RgbLinear` accessor, never re-deriving the linearization; both carry the `Op key` the `MaterialFault` rail correlates.
- Packages: Wacton.Unicolour (composed — `Spd`, `Unicolour`, `Configuration`, `RgbConfiguration`, `ColourSpace`, `DeltaE`, `GamutMap`/`MapToRgbGamut`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured illuminant is one Unicolour `Spd` construction; a new working space is one `RgbConfiguration` preset selection; the upsampling table is the only author-kernel — a new spectral band is one `SpectralBand` row; zero new surface. A measured isotropic spectral BRDF (EPFL RGL goniophotometer, brdf-loader format) admits through one `Spd` construction per band, framed at `[05]-[RESEARCH]` and the `acquisition#ACQUISITION` import path.
- Boundary: RGB→SPD is the documented Unicolour NOT_COVERED concern, authored as the Smits (1999) seven-basis non-negative reflectance upsampling — the constant/cyan/magenta/yellow/red/green/blue basis SPDs combined so the round-trip `SPD→XYZ→RGB` reproduces the input chromaticity with a smooth, energy-bounded reflectance (the appearance-engine requirement Smits states); the resulting `Spd` feeds Unicolour's `new Unicolour(Configuration, Spd)` → internal `Xyz.FromSpd` for the measured-illuminant path; the scene-linear working space is `RgbConfiguration.Acescg` (AP1 primaries) or `Aces20651` for the ACES scene-referred path, set in a `Configuration` and read through Unicolour's `.RgbLinear` — Materials NEVER re-derives the sRGB/ACEScg transfer curve, it composes the preset; appearance MATCH between a measured target and a library row is `Unicolour.Difference(reference, DeltaE.Ciede2000)`, the industry-standard appearance ΔE, composed not re-minted; the `IsInRgbGamut` check gates the boundary shade and a saturated upsampled primary that lands outside the gamut is perceptually pulled in through the composed `MapToRgbGamut(GamutMap.OklchChromaReduction)` (reduce Oklch chroma until in gamut) rather than hard-faulted, so the white-furnace residual closes on a chroma-reduced in-gamut reflectance instead of rejecting the row — the fault rail is reserved for a non-finite channel; `RgbSpectrum.Luminance` reads the AP1 weights consistent with the ACEScg working space; Wacton.Unicolour is consumed directly as the one scene-linear/spectral color owner and Materials never mints a second `ColourSpace` wrapper. Every base color, conductor, and FinishMix pigment grounds through this one owner — the `acquisition#ACQUISITION` `GroundSpectral` composes the `ToSpd`/`new Unicolour(Configuration, Spd)` RGB→SPD path AND the `SceneLinear` grounding, while `finish#FINISH` `FinishMix.Reflectance` composes ONLY `SceneLinear` over its Kubelka-Munk pigment-mix reflectance (the `new Unicolour(Configuration, Pigment[], double[])` ctor owns the mix, NOT the `Spd`/`ToSpd` upsample) — both fold through this one `SceneLinear` grounding owner, never N parallel inline spectral-construction sites.
- Law: `SpectralUpsample` is the page's one `[EXPRESSION_SPINE]` kernel exemption — `ToSpd`/`Acc`/`Resample5nm` fill fixed-length `double[]` buffers by index across the Smits ordered-combination branch and the 69-sample 5nm resample, the admitted boundary-numeric-kernel carve-out from the immutable-fold law for the per-shade hot path; every other kernel on the page is expression-bodied.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Collections.Frozen;
using LanguageExt;                       // Seq, Option, Fin
using Rasm;                              // UnitInterval
using Rasm.Domain;                       // Op
using Rasm.Materials.Appearance.Bsdf;    // MaterialFault, RgbSpectrum, ComplexIor, BsdfLobe, LayeredBsdf, LobeWeight, Microfacet, MultiScatter, LocalVector, SpectralBand-consumers
using Rasm.Materials.Appearance.Graph;   // MaterialParameters, SubsurfaceRadius (the OpenPbrSurface.Of source row + its scatter carrier)
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

    public static Fin<RgbSpectrum> SceneLinear(Unicolour colour, Op key) {
        Unicolour mapped = colour.IsInRgbGamut ? colour : colour.MapToRgbGamut(GamutMap.OklchChromaReduction);
        ColourTriplet lin = mapped.RgbLinear.Triplet;
        return RgbSpectrum.TryCreate(Math.Max(0.0, lin.First), Math.Max(0.0, lin.Second), Math.Max(0.0, lin.Third), out RgbSpectrum rgb)
            ? Fin.Succ(rgb)
            : Fin.Fail<RgbSpectrum>(MaterialFault.Gamut(key, "<non-finite-linear-rgb>"));
    }
    public static readonly Configuration SceneConfig = new(RgbConfiguration.Acescg);
    public static RgbSpectrum FromAcescg(double r, double g, double b, Op key) {
        Unicolour c = new(SceneConfig, ColourSpace.RgbLinear, r, g, b);
        return SceneLinear(c, key).IfFail(RgbSpectrum.Black);
    }
    public static double AppearanceDelta(Unicolour candidate, Unicolour reference) => candidate.Difference(reference, DeltaE.Ciede2000);
}
```

## [03]-[TONE_MAP]

- Owner: `ToneOperator` `[SmartEnum<string>]` (aces · reinhard · filmic · exposure); `ToneMap` the static operator table.
- Entry: `public static RgbSpectrum Apply(ToneOperator op, RgbSpectrum sceneLinear, double exposure)` — one entry, four operators by row; the scene-linear input is the integrator's HDR radiance, the output is display-referred [0,1] for the gamut check.
- Packages: Wacton.Unicolour (composed for the encode after tone-map — `RgbConfiguration.StandardRgb`/`Rec2100Pq` transfer), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new operator is one `ToneOperator` row carrying its per-channel `Curve` delegate — `Apply` reads the row's curve through `[UseDelegateFromConstructor]` and never branches; the encode/transfer after tone-mapping is a composed `RgbConfiguration` preset, never an author-kernel; zero new surface.
- Boundary: ACES RRT/ODT is the documented Unicolour NOT_COVERED concern — authored as the Narkowicz (2016) ACES-fit approximation of the combined RRT+ODT (the rational `(x(ax+b))/(x(cx+d)+e)` curve matched to the reference ACES sRGB ODT), exact in the fence; the scene-referred operators are the Reinhard extended `x(1+x/Lwhite²)/(1+x)` global operator, the Hejl-Burgess-Dawson filmic curve, and the plain exposure-then-clamp; exposure is applied as a multiplicative scale before the curve (`scene · 2^exposure`); the OETF/transfer after tone-mapping (sRGB gamma, or PQ for HDR via `RgbConfiguration.Rec2100Pq`) is COMPOSED through Unicolour — the consumer constructs a `Unicolour(config, ColourSpace.RgbLinear, r, g, b)` and reads `.Rgb` for the encoded display value, so Materials authors ONLY the tone CURVE the library lacks and never re-derives a transfer function; the tone-mapped display-referred output is the result the app-platform raster path (`Rasm.AppUi/Charts/custom#COLOR_SPACE`) consumes downstream, never a surface Materials reaches into.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ToneOperator {
    public static readonly ToneOperator Aces = new("aces", ToneMap.AcesFit);
    public static readonly ToneOperator Reinhard = new("reinhard", ToneMap.ReinhardExtended);
    public static readonly ToneOperator Filmic = new("filmic", ToneMap.FilmicCurve);
    public static readonly ToneOperator Exposure = new("exposure", static x => Math.Clamp(x, 0.0, 1.0));

    [UseDelegateFromConstructor]
    public partial double Curve(double sceneLinearChannel);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ToneMap {
    public static RgbSpectrum Apply(ToneOperator op, RgbSpectrum sceneLinear, double exposure) =>
        sceneLinear.Scale(Math.Pow(2.0, exposure)).Map(op.Curve);

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

    public static (double R, double G, double B) Encode(RgbSpectrum displayLinear, RgbConfiguration target) {
        Unicolour c = new(new Configuration(target), ColourSpace.RgbLinear, displayLinear.R, displayLinear.G, displayLinear.B);
        ColourTriplet rgb = c.Rgb.Triplet;
        return (rgb.First, rgb.Second, rgb.Third);
    }
}
```

## [04]-[CONDUCTOR_IOR]

- Owner: `ConductorMetal` `[SmartEnum<string>]` the measured-metal axis; `ConductorIor` the per-band `ComplexIor` table; the `Conductor` lobe grounding.
- Entry: `public static ComplexIor Of(ConductorMetal metal)` reads the measured complex refractive index per RGB band as one `ComplexIor` carrier (its `Eta`/`K` two validated `RgbSpectrum` bands), and `public static BsdfLobe.Conductor Lobe(ConductorMetal metal, double alphaX, double alphaY)` constructs the grounded `bsdf#LOBE_FAMILY` `Conductor` lobe from it — the metal F0 is the measured `ComplexIor` Fresnel, NEVER a hand-authored RGB albedo scaled to a guess.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new measured metal is one `ConductorMetal` row carrying its three-band `(η, k)` measured pair from the published refractive-index table; the table is the INTERNAL leg of `graph#MEASURED_SPECTRAL_LIBRARY` — the conductor rows ground here rather than carrying a hand-authored Acescg albedo. A spectral 195-wavelength conductor curve (EPFL RGL goniophotometer or a full `refractiveindex.info` n/k spectrum) is the [UPSTREAM-BLOCKED] extension that admits through `[02]-[SPECTRAL_UPSAMPLE]` `ToSpd` per band once a managed `.bsdf`/spectral reader lands at `acquisition#EPFL_RGL_BRDF_LOADER` and the decoded conductor curve arrives over the host-free-peer / host-edge wire (NOT a Rasm.Compute project reference — the acyclic strata forbids the AEC→app-platform edge); zero new surface.
- Boundary: the complex refractive index `(η, k)` per RGB band is the physically-correct conductor F0 carried as one `ComplexIor` `[ComplexValueObject]` band — the `bsdf#MICROFACET_KERNEL` `Microfacet.FresnelConductor(cosI, ComplexIor ior)` overload reads it directly, so a metal's edge tint and grazing-angle hue shift emerge from the measured dispersion rather than an artist's base-color triple; the three-band `Eta`/`K` values transcribe the Johnson-Christy / `refractiveindex.info` measured dataset at the RGB band centres `SpectralBand.Red`/`Green`/`Blue` carry (610/550/465 nm sampled against the published 630/532/465 nm anchors); the `graph#MATERIAL_LIBRARY` conductor rows carry a measured `BaseColor` for the diffuse-substitute preview path AND name a `ConductorMetal` so the `bsdf#LOBE_FAMILY` `Conductor` lobe grounds from `ConductorIor.Of`, the base color the perceptual seed and the `(η, k)` the shading truth; a metal absent from the table falls back to the `graph#MATERIAL_LIBRARY` base-color-as-F0 dielectric-Schlick approximation rather than faulting, so the table grounds the eight named metals and a ninth row admits without a rebuild; the conductor F0 round-trips in-gamut through the `bsdf#WHITE_FURNACE_HARNESS` lossless-conductor furnace (F≡1 reflects unit energy) so a measured metal conserves energy under the Kulla-Conty multi-scatter term.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConductorMetal {
    public static readonly ConductorMetal Gold     = new("gold");
    public static readonly ConductorMetal Copper   = new("copper");
    public static readonly ConductorMetal Aluminum = new("aluminum");
    public static readonly ConductorMetal Silver   = new("silver");
    public static readonly ConductorMetal Iron     = new("iron");
    public static readonly ConductorMetal Chromium = new("chromium");
    public static readonly ConductorMetal Titanium = new("titanium");
    public static readonly ConductorMetal Brass    = new("brass");
}

// --- [TABLES] ------------------------------------------------------------------------------
public static class ConductorIor {
    private static readonly FrozenDictionary<ConductorMetal, ComplexIor> Table =
        new (ConductorMetal Metal, ComplexIor Ior)[] {
            (ConductorMetal.Gold,     ComplexIor.Create(RgbSpectrum.Create(0.183, 0.421, 1.373), RgbSpectrum.Create(3.424, 2.346, 1.770))),
            (ConductorMetal.Copper,   ComplexIor.Create(RgbSpectrum.Create(0.271, 0.677, 1.316), RgbSpectrum.Create(3.609, 2.625, 2.292))),
            (ConductorMetal.Aluminum, ComplexIor.Create(RgbSpectrum.Create(1.346, 0.965, 0.617), RgbSpectrum.Create(7.475, 6.400, 5.303))),
            (ConductorMetal.Silver,   ComplexIor.Create(RgbSpectrum.Create(0.159, 0.145, 0.135), RgbSpectrum.Create(3.929, 3.190, 2.381))),
            (ConductorMetal.Iron,     ComplexIor.Create(RgbSpectrum.Create(2.911, 2.950, 2.585), RgbSpectrum.Create(3.089, 2.932, 2.767))),
            (ConductorMetal.Chromium, ComplexIor.Create(RgbSpectrum.Create(2.020, 2.790, 2.020), RgbSpectrum.Create(3.860, 4.200, 3.860))),
            (ConductorMetal.Titanium, ComplexIor.Create(RgbSpectrum.Create(2.741, 2.542, 2.267), RgbSpectrum.Create(3.814, 3.435, 3.039))),
            (ConductorMetal.Brass,    ComplexIor.Create(RgbSpectrum.Create(0.444, 0.527, 1.094), RgbSpectrum.Create(3.695, 2.765, 1.829))),
        }.ToFrozenDictionary(static r => r.Metal, static r => r.Ior);

    public static ComplexIor Of(ConductorMetal metal) => Table[metal];

    public static BsdfLobe.Conductor Lobe(ConductorMetal metal, double alphaX, double alphaY) =>
        new(Of(metal), alphaX, alphaY);

    public static Option<ConductorMetal> Resolve(string family, string name) =>
        family == "metal" && ConductorMetal.TryGet(name, out ConductorMetal? metal) ? Optional(metal) : Option<ConductorMetal>.None;

    public static RgbSpectrum FresnelNormal(ConductorMetal metal) => Of(metal).FresnelNormal;
}
```

## [05]-[OPENPBR_SLAB]

- Owner: `Slab` `[Union]` the closed slab family (fuzz · coat · emission · base — its four cases ARE the slab discriminant, no parallel kind enum re-describing them); `SlabStack` the outermost-to-base layering algebra; `OpenPbrSurface` the OpenPBR parameter vector AND the one `MaterialParameters`→OpenPBR lowering (`OpenPbrSurface.Of`).
- Entry: `public static OpenPbrSurface Of(MaterialParameters p, ConductorMetal conductor)` is the SINGLE `MaterialParameters`→OpenPBR column correspondence — the standard-column vector both this stack and `interchange#MATERIAL_WIRE` `OpenPbrGroupsWire.Of` read, so the mapping is declared once and never re-minted at the wire; `public static SlabStack Lower(OpenPbrSurface surface)` derives the formal OpenPBR Surface 1.1 stack from that vector and `public static SlabStack Lower(MaterialParameters p, ConductorMetal conductor)` is the convenience overload composing `Of` then `Lower` (the row names its `ConductorMetal` through `[04]-[CONDUCTOR_IOR]`); `public Fin<LayeredBsdf> ToLayered(Op key)` collapses the stack to the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` weighted-lobe fold the integrator shades — the vector IS the canonical lowering, the stack the composition law, the weighted fold its energy-preserving collapse, so the renderer reads one `LayeredBsdf`.
- Packages: Rasm (project — `UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new layering modifier is one `Slab` case carrying its albedo-scaling operator (the fuzz slab is the new closed lobe case the seven-lobe set lacked, realized as the `bsdf#LOBE_FAMILY` `Sheen` lobe at the fuzz position; the iridescent topcoat is the realized `Coat`→`bsdf#LOBE_FAMILY` `ThinFilm` lowering when the coat carries a film thickness, NOT a parallel owner); a new OpenPBR parameter is one `OpenPbrSurface` column `Of` populates and `Lower` reads — the standard column set (`base`, `specular`, `transmission`, `subsurface`, `coat`, `fuzz`, `thin_film`, `emission`, `geometry`) the `graph#MATERIAL_LIBRARY` `MaterialParameters` aligns to; zero new surface. The `interchange#MATERIAL_WIRE` `MaterialWire` is the OpenPBR-vector wire projection this stack's `OpenPbrSurface` defines and the TS/Py consumers decode — `interchange` composes `OpenPbrSurface.Of`, never a second `MaterialParameters`→OpenPBR mint.
- Law: the slab stack is the formal OpenPBR Surface 1.1 layering order outermost-to-base — `fuzz` over `coat` over `emission` over the `base` substrate — composed by albedo-scaling layering operators, NOT the additive convex-combination weight fold `bsdf#LAYERED_COMPOSITION` predated: `RemainingEnergy` cascades `pass ← pass · (1 − w · E(slab))` over the placed outer slabs, where `E(slab)` is that slab's lobe-specific normal-incidence directional albedo from `bsdf#LOBE_FAMILY` `MultiScatter.DirectionalAlbedo` at the lobe's OWN GGX alpha (`LobeAlpha` reads `AlphaX`/`AlphaY` per case), so a rough coat occludes more of the base than a smooth one and the energy split is a function of the real lobe, never a fixed constant; the base substrate is the metalness-mixed conductor-vs-dielectric the `[04]-[CONDUCTOR_IOR]` table grounds, the dielectric arm carrying the opaque glossy-diffuse-plus-subsurface or the translucent transmission per the `transmission` weight.
- Boundary: `OpenPbrSurface.Of` is the ONE `MaterialParameters`→OpenPBR construction and `SlabStack.Lower` the ONE vector→slab lowering — a per-material slab builder or a second wire-side OpenPBR mint is the deleted form; the fuzz slab lowers to a `Sheen` lobe weighted by `fuzz_weight`, the coat slab to a `Clearcoat` lobe weighted by `coat_weight` OR — when its `ThinFilmThickness` is `Some` (the `thin_film_weight > 0` admission) — to a `bsdf#LOBE_FAMILY` `ThinFilm` lobe carrying the film thickness/IOR and the coat IOR lifted into a real `ComplexIor` base (the Belcour-Barla spectral interference the `finish#COAT_STACK_THIN_FILM` pearlescent topcoat drives), the emission slab to the `graph#MATERIAL_GRAPH` emission carrier (energy-additive, never occluding), and the base substrate to the metalness lerp between a `Conductor` lobe (grounded from the `ConductorMetal` the row names through `[04]-[CONDUCTOR_IOR]`) and a `Dielectric`/`Diffuse`/`Subsurface` mix per `transmission`/`subsurface`, the `Subsurface` lobe reading the validated three-band `SubsurfaceRadius` `[ComplexValueObject]` carrier's `Magnitude` for the Burley diffusion radius (the carrier declared on `graph#MATERIAL_LIBRARY` `MaterialParameters`, gating a negative or non-finite millimetre mean-free-path once at `Create` so no `Vector3d` scatter vector threads the slab signatures); `ToLayered` collapses the albedo-scaled slab weights into the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf.Of` normalized lobe list so the integrator shades one `LayeredBsdf` and never re-derives the slab nesting per sample — the per-lobe albedo-scaling is computed once at lowering, the energy each outer slab leaves for the layer below (`1 − w · E(slab)` at the lobe's own alpha) baked into the lobe weight; the OpenPBR z-up local-frame convention matches the `bsdf#SHADING_FRAME` `LocalVector` basis so no slab re-derives `cosθ`; slab-weight admission is TOTAL — `Of` clamps the OpenPBR columns and the `Weight` helper clamps every `w · pass` into `[0,1]` before `UnitInterval.Create`, so no `Slab` weight throws the value-object guard mid-fold, and the one fault site is `ToLayered`→`LayeredBsdf.Of` railing `MaterialFault.Parameter` when every lobe weight filters to zero (a degenerate empty stack), never a propagated energy gain; the `weathering#WEATHERING` aging operator targets the slab columns directly once lowered (chalking raises `coat_roughness`, soiling raises `fuzz_weight`, patina greens the `Slab.Base` color and drops its `Metalness` toward zero — the conductor corrodes to a dielectric verdigris, never a metal-to-metal `ConductorMetal` swap the 8-member smart-enum cannot represent) through the `SlabColumnDelta` trajectory, the slab columns the shared aging target.

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
    public sealed record Coat(double Weight, RgbSpectrum Color, double Roughness, double Ior, Option<double> ThinFilmThickness, double ThinFilmIor) : Slab;
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
    double FuzzWeight, double FuzzRoughness,
    double ThinFilmWeight, double ThinFilmThickness, double ThinFilmIor,
    RgbSpectrum EmissionColor, double EmissionLuminance,
    ConductorMetal Conductor) {

    // BaseSpecularTint reads the existing MaterialParameters.SpecularTint column (the OpenPBR base_specular_tint, the
    // facing-vs-edge specular colour weight) — NOT hardcoded, so the authored tint a row carries reaches the OpenPBR
    // vector and the interchange#MATERIAL_WIRE wire rather than being dropped at the lowering. SpecularColor/CoatColor/
    // FuzzColor stay synthesized White/BaseColor (the OpenPBR neutral-tint baseline) because MaterialParameters carries
    // no specular/coat/fuzz colour source — a column with no source would be speculative; the tint scalar HAS a source.
    public static OpenPbrSurface Of(MaterialParameters p, ConductorMetal conductor) =>
        new(BaseWeight: 1.0, AcescgRgb(p.BaseColor), p.Metalness, p.Roughness, Math.Clamp(p.SpecularTint, 0.0, 1.0),
            SpecularWeight: 1.0, p.Roughness, p.Ior, p.Anisotropy,
            p.Transmission, p.TransmissionRoughness,
            p.Subsurface, p.SubsurfaceRadius,
            p.Clearcoat, p.ClearcoatRoughness, CoatIor: 1.5,
            p.Sheen, FuzzRoughness: Math.Max(1e-3, p.Roughness),
            ThinFilmWeight: 0.0, ThinFilmThickness: 0.0, ThinFilmIor: 1.5,
            AcescgRgb(p.Emission), p.EmissionLuminance,
            conductor);

    internal static RgbSpectrum AcescgRgb(Unicolour colour) { var lin = colour.RgbLinear; return RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)); }
}

public sealed record SlabStack(Seq<Slab> Slabs) {
    // The convenience entry the library/finish drive: lower the row to the canonical OpenPbrSurface vector, then to slabs.
    public static SlabStack Lower(MaterialParameters p, ConductorMetal conductor) => Lower(OpenPbrSurface.Of(p, conductor));

    // The ONE OpenPBR construction: the outermost-to-base slab order derived from the vector columns. fuzz over coat over
    // emission over base; the coat carries the thin_film modifier when ThinFilmWeight>0 so an iridescent topcoat rides
    // the existing Coat slab rather than a parallel owner; the base names its ConductorMetal for the [04] grounding.
    public static SlabStack Lower(OpenPbrSurface s) =>
        new(Seq<Slab>()
            .Add(new Slab.Fuzz(s.FuzzWeight, s.BaseColor, s.FuzzRoughness))
            .Add(new Slab.Coat(s.CoatWeight, RgbSpectrum.White, s.CoatRoughness, s.CoatIor, s.ThinFilmWeight > 0.0 ? Some(s.ThinFilmThickness) : Option<double>.None, s.ThinFilmIor))
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

    // The single-scatter alpha a lobe occludes at: the GGX alpha of the lobe's roughness, the diffuse/sheen full-rough.
    private static double LobeAlpha(BsdfLobe lobe) => lobe.Switch(
        diffuse:   static _ => 1.0,
        conductor: static c => Math.Max(c.AlphaX, c.AlphaY),
        dielectric: static g => Math.Max(g.AlphaX, g.AlphaY),
        sheen:     static s => Microfacet.AlphaOf(s.Roughness),
        clearcoat: static BsdfLobe.Clearcoat.Alpha,
        subsurface: static _ => 1.0,
        thinFilm:  static BsdfLobe.ThinFilm.AlphaX);

    // The coat lowers to a ThinFilm lobe (the Belcour-Barla spectral interference term) when a film thickness is carried,
    // else a plain Clearcoat dielectric layer; the fuzz to a Sheen lobe; both attenuated by the energy the base leaves.
    private static Seq<LobeWeight> LowerSlab(Slab slab, double pass) => slab.Switch(
        fuzz:     f => f.Weight > 0.0 ? Seq(new LobeWeight(new BsdfLobe.Sheen(f.Color, f.Roughness), Weight(f.Weight, pass))) : Seq<LobeWeight>(),
        coat:     c => c.Weight > 0.0 ? Seq(new LobeWeight(CoatLobe(c), Weight(c.Weight, pass))) : Seq<LobeWeight>(),
        emission: static _ => Seq<LobeWeight>(),
        @base:    b => LowerBase(b, pass));

    private static BsdfLobe CoatLobe(Slab.Coat c) => c.ThinFilmThickness.Match(
        Some: thickness => new BsdfLobe.ThinFilm(thickness, c.ThinFilmIor, c.Roughness, ComplexIor.Create(RgbSpectrum.Create(c.Ior, c.Ior, c.Ior), RgbSpectrum.Black)),
        None: () => new BsdfLobe.Clearcoat(c.Weight, c.Roughness));

    private static Seq<LobeWeight> LowerBase(Slab.Base b, double pass) {
        double alpha = Microfacet.AlphaOf(b.Roughness);
        BsdfLobe conductor = new BsdfLobe.Conductor(ConductorIor.Of(b.Conductor), alpha, alpha);
        BsdfLobe dielectric = b.Transmission > 0.0
            ? new BsdfLobe.Dielectric(b.SpecularIor, Microfacet.AlphaOf(b.TransmissionRoughness), Microfacet.AlphaOf(b.TransmissionRoughness), b.BaseColor)
            : b.Subsurface > 0.0
                ? new BsdfLobe.Subsurface(b.BaseColor, b.SubsurfaceRadius.Magnitude)
                : new BsdfLobe.Diffuse(b.BaseColor, b.Roughness);
        double metalness = Math.Clamp(b.Metalness, 0.0, 1.0);
        return Seq(new LobeWeight(conductor, Weight(metalness, pass)), new LobeWeight(dielectric, Weight(1.0 - metalness, pass)));
    }

    // The one weight-admission helper: every slab weight is clamped to [0,1] before UnitInterval.Create so a consumer-built
    // Slab carrying an out-of-band weight never throws the ValueObject guard mid-fold — the admission stays total here.
    private static UnitInterval Weight(double w, double pass) => UnitInterval.Create(Math.Clamp(w * pass, 0.0, 1.0));
}
```

## [06]-[RESEARCH]

- [SPECTRAL_GROUNDING]: the RGB→SPD Smits seven-basis kernel + the Unicolour `Spd`→XYZ composition + the measured-illuminant reduction is the one spectral grounding owner every base color, conductor, and `finish#FINISH` pigment grounds through — the `acquisition#ACQUISITION` `GroundSpectral` composes this owner's `ToSpd` + `new Unicolour(Configuration, Spd)` RGB→SPD path AND `SceneLinear`, while `finish#FINISH` `FinishMix.Reflectance` composes ONLY `SceneLinear` over its Kubelka-Munk pigment-mix reflectance (the `new Unicolour(Configuration, Pigment[], double[])` ctor owns the mix, never the `Spd`/`ToSpd` upsample) — both fold through this one `SceneLinear` grounding owner rather than N parallel inline spectral-construction sites. The growth leg is the full Unicolour `Spd`/`SpectralCoefficients`/`Observer` (Degree2/Degree10) / `Illuminant` (the A/C/D50-D75/E/F-series statics) measured-illuminant reduction so a base color round-trips through a witnessed reflectance SPD under a named illuminant and clears the `graph#MATERIAL_LIBRARY` `SpectralAdmit` MacAdam spectral-locus bound, framed at `graph#MEASURED_SPECTRAL_LIBRARY`.
- [WHITE_FURNACE_HARNESS] (tier-2): the energy-conservation harness lives on the kernel page (`bsdf#WHITE_FURNACE_HARNESS`) since it samples `bsdf#LOBE_FAMILY` `BsdfLobe.Sample` and bounds the furnace residual against the lobe set; the surface page's `ConductorIor` table and `SlabStack.ToLayered` lowering feed that harness — a measured conductor F0 round-trips in-gamut through the lossless-conductor furnace and a lowered `SlabStack` collapses to an energy-bounded `LayeredBsdf`, the residual the harness measures the perceptual ΔE the `SpectralUpsample` `MapToRgbGamut(GamutMap.OklchChromaReduction)` chroma reduction costs on a saturated upsampled primary.
- [CONDUCTOR_SPECTRAL_CURVE]: the per-band `(η, k)` table is the realized INTERNAL leg; the full per-wavelength conductor curve is the [UPSTREAM-BLOCKED] extension awaiting the host-free-peer / host-edge spectral-reconstruction inference run delivering the decoded curve over the wire (NOT a Rasm.Compute project reference — the acyclic strata forbids the AEC-domain Materials a reference to the app-platform Compute ONNX owner), the decoded curve admitting through `[02]-[SPECTRAL_UPSAMPLE]` `ToSpd` per band. Until that lands the table grounds the three measured RGB-band anchors, the spectral inference the upstream-gated leg over a realized three-band fast path; framed at `graph#MEASURED_SPECTRAL_LIBRARY`.

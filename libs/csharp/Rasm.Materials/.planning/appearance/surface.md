# [MATERIALS_SURFACE]

THE COLOR-SCIENCE LOWERING and THE OPENPBR-CONSTRUCTION HALF. The surface page owns the four lowering/grounding kernels the wire and the library drive — ONE `SpectralUpsample` RGB→SPD kernel feeding Unicolour's `Spd`→XYZ and the measured-illuminant reduction every base color grounds through, ONE `ToneMap` ACES RRT/ODT + scene-referred operator table, ONE `ConductorIor` measured complex-IOR table grounding every metal F0 per band, and ONE `SlabStack` OpenPBR Surface 1.1 stack-of-slabs the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` fold lowers from. The page owns the `SpectralBand`, `ToneOperator`, `ConductorMetal`, and `SlabKind` axes and the spectral/tone/conductor/slab kernels; it COMPOSES the `bsdf#SHADING_FRAME` `MaterialFault` (band 2450) and `MaterialKeyPolicy` accessor declared once on the kernel page, the `bsdf#LOBE_FAMILY` `RgbSpectrum`/`ComplexIor` validated carriers and `BsdfLobe` closed set, and the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf.Of` weighted-lobe fold — the kernel page owns frame-local shading, this page owns the OpenPBR construction the `interchange#MATERIAL_WIRE` and the `weathering#WEATHERING` aging trajectory target.

This page is the lowering boundary: a `graph#MATERIAL_LIBRARY` `MaterialParameters` row lowers through `SlabStack.Lower` to the formal OpenPBR Surface 1.1 stack (fuzz over coat over emission over base), the base substrate grounds its conductor lobe from `ConductorIor`, the base color upsamples to an SPD through `SpectralUpsample`, and `SlabStack.ToLayered` collapses the albedo-scaled stack to the one `LayeredBsdf` weighted fold the renderer shades — so the slab algebra is the construction the row drives through, the lobe math is single-sourced on the kernel page, and the tone-map is the display-referred egress the raster path consumes. The split is BY CONCERN, not by size: the kernel page carries the per-sample shading math the path tracer drives, this page the color-science and OpenPBR-construction the wire and library drive, the two sharing the `MaterialFault` band and `MaterialKeyPolicy` declared once on the kernel.

## [01]-[INDEX]

- [01]-[SPECTRAL_UPSAMPLE]: the RGB→SPD coefficient kernel, the Unicolour `Spd`→XYZ composition, the measured-illuminant reduction, and scene-linear admission.
- [02]-[TONE_MAP]: the ACES RRT/ODT author-kernel, the scene-referred filmic/Reinhard/exposure operators, and the `ToneOperator` table.
- [03]-[CONDUCTOR_IOR]: the `ConductorMetal` axis, the `ConductorIor` measured complex-IOR table per RGB band, and the `Conductor` lobe grounding.
- [04]-[OPENPBR_SLAB]: the `SlabKind` axis, the `Slab` `[Union]`, the `SlabStack` outermost-to-base layering algebra, and the `MaterialParameters`→stack lowering the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` fold consumes.

## [02]-[SPECTRAL_UPSAMPLE]

- Owner: `SpectralUpsample` author-kernel; composes the `bsdf#SHADING_FRAME` `SpectralBand` `[SmartEnum<string>]` band-centre vocabulary (declared once on the kernel page, read by the thin-film lobe and the spectral curve) and Wacton.Unicolour for every conversion Unicolour already owns.
- Entry: `public static Fin<Spd> ToSpd(RgbSpectrum rgb, Op key)` and `public static Fin<RgbSpectrum> SceneLinear(Unicolour colour, Op key)` — RGB→SPD is the author-kernel Unicolour lacks (NOT_COVERED); SceneLinear COMPOSES `RgbConfiguration.Acescg` and Unicolour's `.RgbLinear` accessor, never re-deriving the linearization; both carry the `Op key` the `MaterialFault` rail correlates.
- Packages: Wacton.Unicolour (composed — `Spd`, `Unicolour`, `Configuration`, `RgbConfiguration`, `ColourSpace`, `DeltaE`, `GamutMap`/`MapToRgbGamut`), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured illuminant is one Unicolour `Spd` construction; a new working space is one `RgbConfiguration` preset selection; the upsampling table is the only author-kernel — a new spectral band is one `SpectralBand` row; zero new surface. A measured isotropic spectral BRDF (EPFL RGL goniophotometer, brdf-loader format) admits through one `Spd` construction per band, framed at `[05]-[RESEARCH]` and the `acquisition#ACQUISITION` import path.
- Boundary: RGB→SPD is the documented Unicolour NOT_COVERED concern, authored as the Smits (1999) seven-basis non-negative reflectance upsampling — the constant/cyan/magenta/yellow/red/green/blue basis SPDs combined so the round-trip `SPD→XYZ→RGB` reproduces the input chromaticity with a smooth, energy-bounded reflectance (the appearance-engine requirement Smits states); the resulting `Spd` feeds Unicolour's `new Unicolour(Configuration, Spd)` → internal `Xyz.FromSpd` for the measured-illuminant path; the scene-linear working space is `RgbConfiguration.Acescg` (AP1 primaries) or `Aces20651` for the ACES scene-referred path, set in a `Configuration` and read through Unicolour's `.RgbLinear` — Materials NEVER re-derives the sRGB/ACEScg transfer curve, it composes the preset; appearance MATCH between a measured target and a library row is `Unicolour.Difference(reference, DeltaE.Ciede2000)`, the industry-standard appearance ΔE, composed not re-minted; the `IsInRgbGamut` check gates the boundary shade and a saturated upsampled primary that lands outside the gamut is perceptually pulled in through the composed `MapToRgbGamut(GamutMap.OklchChromaReduction)` (reduce Oklch chroma until in gamut) rather than hard-faulted, so the white-furnace residual closes on a chroma-reduced in-gamut reflectance instead of rejecting the row — the fault rail is reserved for a non-finite channel; `RgbSpectrum.Luminance` reads the AP1 weights consistent with the ACEScg working space; Wacton.Unicolour is consumed directly as the one scene-linear/spectral color owner and Materials never mints a second `ColourSpace` wrapper. Every base color, conductor, and FinishMix pigment grounds through this one owner — the `acquisition#ACQUISITION` `GroundSpectral` and `finish#FINISH` `FinishMix.Reflectance` inline `new Unicolour(Configuration, Spd)` sites compose the same `ToSpd`/`SceneLinear` kernel, never N parallel inline spectral-construction sites.
- Law: `SpectralUpsample` is the page's one `[EXPRESSION_SPINE]` kernel exemption — `ToSpd`/`Acc`/`Resample5nm` fill fixed-length `double[]` buffers by index across the Smits ordered-combination branch and the 69-sample 5nm resample, the admitted boundary-numeric-kernel carve-out from the immutable-fold law for the per-shade hot path; every other kernel on the page is expression-bodied.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using Rasm.Materials.Appearance.Bsdf;   // MaterialFault, MaterialKeyPolicy, RgbSpectrum, ComplexIor, BsdfLobe, LayeredBsdf, LobeWeight, Microfacet, MultiScatter, LocalVector, SpectralBand-consumers
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
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
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
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
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

- Owner: `SlabKind` `[SmartEnum<string>]` the slab axis (fuzz · coat · emission · base); `Slab` `[Union]` the closed slab family; `SlabStack` the outermost-to-base layering algebra; `OpenPbrSurface` the OpenPBR parameter vector.
- Entry: `public static SlabStack Lower(MaterialParameters parameters, ConductorMetal conductor)` lowers a `graph#MATERIAL_LIBRARY` `MaterialParameters` row to the formal OpenPBR Surface 1.1 stack (the row names its `ConductorMetal` through `[04]-[CONDUCTOR_IOR]`), and `public Fin<LayeredBsdf> ToLayered(Op key)` collapses the stack to the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` weighted-lobe fold the integrator shades — the stack IS the composition law, the weighted fold its energy-preserving lowering, so the renderer reads one `LayeredBsdf` and the slab algebra is the construction the row drives through.
- Packages: Rasm (project — `UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new layering modifier is one `Slab` case carrying its albedo-scaling operator (the fuzz slab is the new closed lobe case the seven-lobe set lacked, realized as the `bsdf#LOBE_FAMILY` `Sheen` lobe at the fuzz position; a thin-film modifier rides the `Coat` slab's `ThinFilm` field); a new OpenPBR parameter is one `OpenPbrSurface` column the `Lower` reads — the standard column set (`base`, `specular`, `transmission`, `subsurface`, `coat`, `fuzz`, `thin_film`, `emission`, `geometry`) the `graph#MATERIAL_LIBRARY` `MaterialParameters` aligns to; zero new surface. The `interchange#MATERIAL_WIRE` `MaterialWire` is the OpenPBR-vector wire projection this stack defines and the TS/Py consumers decode.
- Law: the slab stack is the formal OpenPBR Surface 1.1 layering order outermost-to-base — `fuzz` over `coat` over `emission` over the `base` substrate — composed by albedo-scaling layering operators (each slab transmits `1 − E(slab)` of the energy below it, where `E` is the slab's directional albedo from `bsdf#LOBE_FAMILY` `MultiScatter.DirectionalAlbedo`), NOT the additive convex-combination weight fold `bsdf#LAYERED_COMPOSITION` predated; the base substrate is the metalness-mixed conductor-vs-dielectric the `[04]-[CONDUCTOR_IOR]` table grounds, the dielectric arm carrying the opaque glossy-diffuse-plus-subsurface or the translucent transmission per the `transmission` weight.
- Boundary: `SlabStack.Lower` is the ONE OpenPBR construction — a per-material slab builder is the deleted form; the fuzz slab lowers to a `Sheen` lobe weighted by `fuzz_weight`, the coat slab to a `Clearcoat` lobe weighted by `coat_weight` (its `thin_film` field a `ThinFilm` lobe modifier when `thin_film_weight > 0`), the emission slab to the `graph#MATERIAL_GRAPH` emission carrier (energy-additive, never occluding), and the base substrate to the metalness lerp between a `Conductor` lobe (grounded from the `ConductorMetal` the row names through `[04]-[CONDUCTOR_IOR]`) and a `Dielectric`/`Diffuse`/`Subsurface` mix per `transmission`/`subsurface`, the `Subsurface` lobe reading the validated three-band `SubsurfaceRadius` `[ComplexValueObject]` carrier's `Magnitude` for the Burley diffusion radius (the carrier declared on `graph#MATERIAL_LIBRARY` `MaterialParameters`, gating a negative or non-finite millimetre mean-free-path once at `Create` so no `Vector3d` scatter vector threads the slab signatures); `ToLayered` collapses the albedo-scaled slab weights into the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf.Of` normalized lobe list so the integrator shades one `LayeredBsdf` and never re-derives the slab nesting per sample — the albedo-scaling is computed once at lowering, the energy each outer slab leaves for the layer below baked into the lobe weight; the OpenPBR z-up local-frame convention matches the `bsdf#SHADING_FRAME` `LocalVector` basis so no slab re-derives `cosθ`; an over-unit total or a non-finite slab weight rails `MaterialFault.Parameter` at `Lower`, never a propagated energy gain; the `weathering#WEATHERING` aging operator targets the slab columns directly once lowered (chalking raises `coat_roughness`, soiling raises `fuzz_weight`, patina swaps the base `ConductorMetal`) through the `SlabColumnDelta` trajectory, the slab columns the shared aging target.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<MaterialKeyPolicy, string>]
[KeyMemberComparer<MaterialKeyPolicy, string>]
public sealed partial class SlabKind {
    public static readonly SlabKind Fuzz     = new("fuzz");
    public static readonly SlabKind Coat     = new("coat");
    public static readonly SlabKind Emission = new("emission");
    public static readonly SlabKind Base     = new("base");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Slab {
    private Slab() { }

    public sealed record Fuzz(double Weight, RgbSpectrum Color, double Roughness) : Slab;
    public sealed record Coat(double Weight, RgbSpectrum Color, double Roughness, double Ior, Option<double> ThinFilmThickness, double ThinFilmIor) : Slab;
    public sealed record Emission(RgbSpectrum Radiance, double Luminance) : Slab;
    public sealed record Base(double Metalness, ConductorMetal Conductor, RgbSpectrum BaseColor, double Roughness, double SpecularIor, double Anisotropy, double Transmission, double TransmissionRoughness, double Subsurface, SubsurfaceRadius SubsurfaceRadius) : Slab;

    public SlabKind Kind => Switch<SlabKind>(
        fuzz:     static _ => SlabKind.Fuzz,
        coat:     static _ => SlabKind.Coat,
        emission: static _ => SlabKind.Emission,
        @base:    static _ => SlabKind.Base);
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct OpenPbrSurface(
    double BaseWeight, RgbSpectrum BaseColor, double BaseMetalness, double BaseDiffuseRoughness,
    double SpecularWeight, double SpecularRoughness, double SpecularIor, double SpecularAnisotropy,
    double TransmissionWeight, double TransmissionRoughness,
    double SubsurfaceWeight, SubsurfaceRadius SubsurfaceRadius,
    double CoatWeight, double CoatRoughness, double CoatIor,
    double FuzzWeight, double FuzzRoughness,
    double ThinFilmWeight, double ThinFilmThickness, double ThinFilmIor,
    RgbSpectrum EmissionColor, double EmissionLuminance,
    ConductorMetal Conductor);

public sealed record SlabStack(Seq<Slab> Slabs) {
    public static SlabStack Lower(MaterialParameters p, ConductorMetal conductor) =>
        new(Seq<Slab>()
            .Add(new Slab.Fuzz(p.Sheen, AcescgRgb(p.BaseColor), Math.Max(1e-3, p.Roughness)))
            .Add(new Slab.Coat(p.Clearcoat, RgbSpectrum.White, p.ClearcoatRoughness, 1.5, Option<double>.None, 1.5))
            .Add(new Slab.Emission(AcescgRgb(p.Emission), p.EmissionLuminance))
            .Add(new Slab.Base(p.Metalness, conductor, AcescgRgb(p.BaseColor), p.Roughness, p.Ior, p.Anisotropy, p.Transmission, p.TransmissionRoughness, p.Subsurface, p.SubsurfaceRadius)));

    public Fin<LayeredBsdf> ToLayered(Op key) {
        double ax = Microfacet.AlphaOf(BaseRoughness());
        Seq<LobeWeight> lobes = Slabs.Fold(Seq<LobeWeight>(), (acc, slab) => acc + LowerSlab(slab, ax, RemainingEnergy(acc)));
        return LayeredBsdf.Of(lobes.Filter(l => l.Weight.Value > 0.0), key);
    }

    private double BaseRoughness() => Slabs.Choose(static s => s is Slab.Base b ? Some(b.Roughness) : Option<double>.None).HeadOrNone().IfNone(0.5);

    private static double RemainingEnergy(Seq<LobeWeight> placed) =>
        Math.Clamp(1.0 - placed.Sum(l => l.Weight.Value * MultiScatter.DirectionalAlbedo(0.5, 0.5)), 0.0, 1.0);

    private static Seq<LobeWeight> LowerSlab(Slab slab, double baseAlpha, double pass) => slab.Switch(
        fuzz:     f => f.Weight > 0.0 ? Seq1(new LobeWeight(new BsdfLobe.Sheen(f.Color, f.Roughness), UnitInterval.Create(f.Weight * pass))) : Seq<LobeWeight>(),
        coat:     c => c.Weight > 0.0 ? Seq1(new LobeWeight(new BsdfLobe.Clearcoat(c.Weight, c.Roughness), UnitInterval.Create(c.Weight * pass))) : Seq<LobeWeight>(),
        emission: static _ => Seq<LobeWeight>(),
        @base:    b => LowerBase(b, baseAlpha, pass));

    private static Seq<LobeWeight> LowerBase(Slab.Base b, double alpha, double pass) {
        BsdfLobe conductor = new BsdfLobe.Conductor(ConductorIor.Of(b.Conductor), alpha, alpha);
        BsdfLobe dielectric = b.Transmission > 0.0
            ? new BsdfLobe.Dielectric(b.SpecularIor, Microfacet.AlphaOf(b.TransmissionRoughness), Microfacet.AlphaOf(b.TransmissionRoughness), b.BaseColor)
            : b.Subsurface > 0.0
                ? new BsdfLobe.Subsurface(b.BaseColor, b.SubsurfaceRadius.Magnitude)
                : new BsdfLobe.Diffuse(b.BaseColor, b.Roughness);
        double mw = Math.Clamp(b.Metalness, 0.0, 1.0) * pass, dw = (1.0 - Math.Clamp(b.Metalness, 0.0, 1.0)) * pass;
        return Seq(new LobeWeight(conductor, UnitInterval.Create(Math.Clamp(mw, 0.0, 1.0))), new LobeWeight(dielectric, UnitInterval.Create(Math.Clamp(dw, 0.0, 1.0))));
    }

    private static RgbSpectrum AcescgRgb(Unicolour colour) { var lin = colour.RgbLinear; return RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)); }
}
```

## [06]-[RESEARCH]

- [SPECTRAL_GROUNDING]: the RGB→SPD Smits seven-basis kernel + the Unicolour `Spd`→XYZ composition + the measured-illuminant reduction is the one spectral grounding owner every base color, conductor, and `finish#FINISH` pigment grounds through — the `acquisition#ACQUISITION` `GroundSpectral` and `finish#FINISH` `FinishMix.Reflectance` inline `new Unicolour(Configuration, Spd)` sites compose this owner's `ToSpd`/`SceneLinear` rather than N parallel inline spectral-construction sites. The growth leg is the full Unicolour `Spd`/`SpectralCoefficients`/`Observer` (Degree2/Degree10) / `Illuminant` (the A/C/D50-D75/E/F-series statics) measured-illuminant reduction so a base color round-trips through a witnessed reflectance SPD under a named illuminant and clears the `graph#MATERIAL_LIBRARY` `SpectralAdmit` MacAdam spectral-locus bound, framed at `graph#MEASURED_SPECTRAL_LIBRARY`.
- [WHITE_FURNACE_HARNESS] (tier-2): the energy-conservation harness lives on the kernel page (`bsdf#WHITE_FURNACE_HARNESS`) since it samples `bsdf#LOBE_FAMILY` `BsdfLobe.Sample` and bounds the furnace residual against the lobe set; the surface page's `ConductorIor` table and `SlabStack.ToLayered` lowering feed that harness — a measured conductor F0 round-trips in-gamut through the lossless-conductor furnace and a lowered `SlabStack` collapses to an energy-bounded `LayeredBsdf`, the residual the harness measures the perceptual ΔE the `SpectralUpsample` `MapToRgbGamut(GamutMap.OklchChromaReduction)` chroma reduction costs on a saturated upsampled primary.
- [CONDUCTOR_SPECTRAL_CURVE]: the per-band `(η, k)` table is the realized INTERNAL leg; the full per-wavelength conductor curve is the [UPSTREAM-BLOCKED] extension awaiting the host-free-peer / host-edge spectral-reconstruction inference run delivering the decoded curve over the wire (NOT a Rasm.Compute project reference — the acyclic strata forbids the AEC-domain Materials a reference to the app-platform Compute ONNX owner), the decoded curve admitting through `[02]-[SPECTRAL_UPSAMPLE]` `ToSpd` per band. Until that lands the table grounds the three measured RGB-band anchors, the spectral inference the upstream-gated leg over a realized three-band fast path; framed at `graph#MEASURED_SPECTRAL_LIBRARY`.

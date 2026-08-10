# [MATERIALS_SURFACE]

THE COLOR-SCIENCE LOWERING and THE OPENPBR-CONSTRUCTION HALF. The surface page owns the four lowering/grounding kernels the wire and the library drive — ONE `SpectralUpsample` RGB→SPD kernel feeding Unicolour's `Spd`→XYZ and the measured-illuminant reduction every base color grounds through, ONE `ToneMap` scene-to-display operator table (`aces` · `agx` · `pbr-neutral` · `reinhard` · `filmic` · `exposure`) egressing through the `DisplayEncoding` transfer-and-range rows, ONE `ConductorMetal` axis grounding every metal F0 from the measured complex-IOR carried on its rows, and ONE `SlabStack` OpenPBR Surface 1.1 stack-of-slabs the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` fold lowers from. The page owns the `ToneOperator`, `DisplayEncoding`, and `ConductorMetal` axes, the `Slab` `[Union]` closed family (its own discriminant — no parallel kind enum), and the spectral/tone/conductor/slab kernels; it COMPOSES the `bsdf#SHADING_FRAME` `SpectralBand` band vocabulary and `MaterialFault` (band 2450) declared once on the kernel page, the `bsdf#LOBE_FAMILY` `RgbSpectrum`/`ComplexIor` validated carriers and `BsdfLobe` closed set, and the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf.Of` weighted-lobe fold — the kernel page owns frame-local shading, this page owns the OpenPBR construction the `interchange#MATERIAL_WIRE` and the `weathering#WEATHERING` aging trajectory target.

This page is the lowering boundary: a `graph#MATERIAL_LIBRARY` `MaterialParameters` row lowers through the one `OpenPbrSurface.Of` column correspondence to the canonical OpenPBR vector (the SAME vector `interchange#MATERIAL_WIRE` projects, never re-minted at the wire), `SlabStack.Lower` derives the formal OpenPBR Surface 1.1 stack from it (fuzz over coat over emission over base), the base substrate grounds its conductor lobe from the `ConductorMetal` row's measured `Ior`, the base color upsamples to an SPD through `SpectralUpsample`, and `SlabStack.ToLayered` collapses the albedo-scaled stack to the one `LayeredBsdf` weighted fold the renderer shades — so the OpenPBR vector is the canonical lowering, the slab algebra the construction the row drives through, the lobe math single-sourced on the kernel page, and the tone-map the display-referred egress the raster path consumes. The split is BY CONCERN, not by size: the kernel page carries the per-sample shading math the path tracer drives, this page the color-science and OpenPBR-construction the wire and library drive, the two sharing the `MaterialFault` band declared once on the kernel.

## [01]-[INDEX]

- [02]-[SPECTRAL_UPSAMPLE]: the RGB→SPD coefficient kernel, the Unicolour `Spd`→XYZ composition, the measured-illuminant reduction, and scene-linear admission under a kernel `GamutPolicy` row.
- [03]-[TONE_MAP]: the `ToneOperator` table (ACES, AgX, PBR Neutral, Reinhard, Hable filmic, exposure) over the one `ToneCurve` custody case, and the `DisplayEncoding` transfer-and-range egress rows.
- [04]-[CONDUCTOR_IOR]: the `ConductorMetal` axis carrying the measured complex-IOR per RGB band and its `MeasuredSource` provenance class on its rows, the wire `Resolve`, and the `Conductor` lobe grounding.
- [05]-[OPENPBR_SLAB]: the `Slab` `[Union]` closed family (its four cases the slab discriminant, no parallel kind enum), the `OpenPbrSurface` vector and its one `Of` `MaterialParameters`→OpenPBR lowering, the `SlabStack` outermost-to-base layering algebra, and the `ToLayered` collapse the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` fold consumes.

## [02]-[SPECTRAL_UPSAMPLE]

- Owner: `SpectralUpsample` author-kernel; composes the `bsdf#SHADING_FRAME` `SpectralBand` `[SmartEnum<string>]` band-centre vocabulary (declared once on the kernel page, read by the thin-film lobe and the spectral curve) and Wacton.Unicolour for every conversion Unicolour already owns.
- Entry: `public static Fin<ReadOnlyMemory<double>> ToCurve(RgbSpectrum rgb, Op key)`, its `public static Fin<Spd> ToSpd(RgbSpectrum rgb, Op key)` Unicolour projection, and `public static Fin<RgbSpectrum> SceneLinear(Unicolour colour, Op key, GamutPolicy? bound = null)` — RGB→SPD is the author-kernel Unicolour lacks (NOT_COVERED), and the SAMPLES leave through `ToCurve` because `Spd` is a one-way XYZ intake that republishes no grid, so a consumer needing the reflectance itself reads the kernel's own output at the page's declared `SampleStart`/`SampleStep`/`SampleCount` extent rather than re-running the basis combination; SceneLinear COMPOSES the one `PortValue.SceneLinear` Acescg working space and Unicolour's `.RgbLinear` accessor, never re-deriving the linearization; the out-of-gamut pull-in is the kernel `GamutPolicy` row whose `Contains`/`Bound` pair arrives together (the `[06]` `[SPECTRAL_ROUND_TRIP_DELTA_E]` probe passes `GamutPolicy.Spectral` beside the default to price the two strategies); both entries carry the `Op key` the `MaterialFault` rail correlates.
- Packages: Wacton.Unicolour (composed — `Spd`, `Unicolour`, `ColourSpace`, `DeltaE`), MathNet.Numerics (composed — `Interpolate.CubicSplineMonotone`, the PCHIP reconstruction each Smits basis resamples through once at type init), Rasm (project — `RgbProfile` the working-space roster, `GamutPolicy` the gamut rows), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured illuminant is one Unicolour `Spd` construction; a new working space is one kernel `RgbProfile` row; the upsampling table is the only author-kernel — a new spectral band is one `SpectralBand` row; zero new surface. Measured-illuminant reduction is the growth leg — Unicolour `Spd`/`SpectralCoefficients` under a named `Illuminant` static and the policy-selected `Observer` — round-tripping a base color through a witnessed reflectance SPD and clearing the `graph#MATERIAL_LIBRARY` `SpectralAdmit` spectral-locus bound, and a measured isotropic spectral BRDF (EPFL RGL goniophotometer, brdf-loader format) admits through one `Spd` construction per band; both cross the `acquisition#ACQUISITION` import path, whose `[EPFL_RGL_BRDF_LOADER]` row holds the reader question for every consumer of that format.
- Boundary: RGB→SPD is the documented Unicolour NOT_COVERED concern, authored as the Smits (1999) seven-basis non-negative reflectance upsampling — the constant/cyan/magenta/yellow/red/green/blue basis SPDs combined so the round-trip `SPD→XYZ→RGB` reproduces the input chromaticity with a smooth, energy-bounded reflectance (the appearance-engine requirement Smits states); the resulting `Spd` feeds Unicolour's `new Unicolour(Configuration, Spd)` → internal `Xyz.FromSpd` for the measured-illuminant path; the scene-linear working space is ONE Acescg `Configuration` instance PER CIE OBSERVER, named on the graph page — `PortValue.SceneLinear` reading the kernel `RgbProfile.Acescg.Configuration` instance (`Observer.Degree2`, the default every reflectance path reads) and `PortValue.SceneLinearDegree10` minted there (`Observer.Degree10`, the large-field readout `photometric#PHOTOMETRIC` selects for its SPD and Planckian integration) — because a distinct standard observer IS a distinct tristimulus integration and a single instance cannot carry both; any further Acescg `Configuration` mint forks the Unicolour lazy-conversion cache and pays a chromatic adaptation per crossing, so it is the deleted form, the two names being the canonical spellings corpus-wide (the `acquisition#ACQUISITION`/`finish#FINISH` grounding constructions read `PortValue.SceneLinear` directly, a local `SceneConfig` re-export alias is the deleted form), and read through Unicolour's `.RgbLinear` — Materials NEVER re-derives the sRGB/ACEScg transfer curve, it composes the preset; appearance MATCH between a measured target and a library row is the direct `Unicolour.Difference(reference, DeltaE.Ciede2000)` call, the industry-standard appearance ΔE — no local rename wrapper exists; the row's `Contains` gates the boundary shade and a saturated upsampled primary that lands outside the gamut is perceptually pulled in through the same row's `Bound` rather than hard-faulted — `GamutPolicy.Perceptual` (reduce Oklch chroma until in gamut) the default policy, `GamutPolicy.Spectral` (walk the dominant-wavelength excitation purity in) the admitted second strategy the white-furnace ΔE-cost comparison prices against it — so the white-furnace residual closes on a gamut-mapped in-gamut reflectance instead of rejecting the row, and the fault rail is reserved for a non-finite channel; `RgbSpectrum.Luminance` reads the `bsdf#LOBE_FAMILY` `LuminanceWeights` triple DERIVED at type init from this working space's own AP1 `Chromaticities`, so the weights follow a working-space change instead of being re-typed per reader; Wacton.Unicolour is consumed directly as the one scene-linear/spectral color owner and Materials never mints a second `ColourSpace` wrapper. Every base color, conductor, and FinishMix pigment grounds through this one owner — the `acquisition#ACQUISITION` `GroundSpectral` composes the `ToSpd`/`new Unicolour(Configuration, Spd)` RGB→SPD path AND the `SceneLinear` grounding, while `environment#SKY_MODEL` reads `ToCurve` for the per-band ground albedo its spectral fit indexes, while `finish#FINISH` `FinishMix.Reflectance` composes ONLY `SceneLinear` over its Kubelka-Munk pigment-mix reflectance (the `new Unicolour(Configuration, Pigment[], double[])` ctor owns the mix, NOT the `Spd`/`ToSpd` upsample) — both fold through this one `SceneLinear` grounding owner, never N parallel inline spectral-construction sites.
- Law: the page's `[EXPRESSION_SPINE]` kernel exemptions are `SpectralUpsample` (`ToCurve`/`Acc` fill one fixed-length `double[]` buffer by index across the Smits ordered-combination branch), the `ToneMap` authored curve kernels (`AgxGrade`/`PbrNeutralGrade` — fixed-width channel math on the per-frame display path) beside the `ToneMap.Package` and `ToneMap.Encode` span forms (a `Span<T>` crosses no lambda, so neither the package seam nor the plane-row egress has an expression form), and the `SlabStack` mix-chain bindings (`CoatLobes`/`LowerBase`), the admitted boundary-numeric-kernel carve-out from the immutable-fold law; every admission, dispatch, and egress surface on the page is expression-bodied.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Linq;                       // the type-init basis resample and the display-row folds
using LanguageExt;                       // Seq, Option, Fin
using MathNet.Numerics;                  // Interpolate — the PCHIP reconstruction each Smits basis resamples through at type init
using Rasm.Domain;                       // Op
using Rasm.Materials.Appearance.Bsdf;    // MaterialFault, RgbSpectrum, ComplexIor, BsdfLobe, LayeredBsdf, LobeWeight, Microfacet<T>, MultiScatter, LocalVector<T>, SpectralBand-consumers
using Rasm.Materials.Appearance.Graph;   // MaterialParameters, SubsurfaceRadius, ThinFilm (the OpenPbrSurface.Of source row + its scatter/film carriers), PortValue.SceneLinear/.SceneLinearDegree10 (the two per-observer Acescg Configuration instances)
using Rasm.Materials.Appearance.Texture; // ShadeVec4 — the ONE field register the display egress lands on
using Rasm.Materials.Raster;             // PlaneTransfer — the storage row each DisplayEncoding names
using Rasm.Numerics;                      // UnitInterval, RgbProfile (the kernel working-space roster each DisplayEncoding row names)
using Thinktecture;                      // ComparerAccessors, [SmartEnum]/[Union]/[KeyMember*], ConversionOperatorsGeneration
using TinyEXR.V3;                        // ImageProcessing.ToneMap + ToneMapOperator — the span tone-map fold the Lowered rows cross
using Wacton.Unicolour;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance.Surface;

// SpectralBand (the red/green/blue 610/550/465 nm band-centre vocabulary) is declared on bsdf#SHADING_FRAME and
// composed here — the kernel thin-film lobe and this spectral curve read the one band vocabulary, no second register.

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SpectralUpsample {
    // Declaration order is load-bearing: the extents and the two wavelength grids initialize BEFORE every Sampled
    // basis reads them, and a reversed order seats a null node array inside the first interpolant at type init.
    // Both grids DERIVE from the extents — 69 samples at 5 nm from 380 nm is the Spd this page constructs, and the
    // Smits basis spreads its 10 control points evenly across that same span — so no wavelength row is transcribed.
    // INTERNAL rather than private: the sampling grid is the page's declared extent and every consumer of ToCurve
    // reads its own wavelengths off these anchors rather than re-deriving a grid the samples would then not sit on.
    internal const int SampleStart = 380, SampleStep = 5, SampleCount = 69;
    private const int ControlCount = 10;

    private static readonly double[] SampleNm =
        Enumerable.Range(0, SampleCount).Select(static i => (double)(SampleStart + (i * SampleStep))).ToArray();
    private static readonly double[] ControlNm =
        Enumerable.Range(0, ControlCount)
            .Select(static k => SampleStart + (k * (SampleCount - 1.0) * SampleStep / (ControlCount - 1))).ToArray();

    // Each basis reconstructs ONCE at type init and the accumulation then runs directly in SAMPLE space, so the
    // per-colour resample is gone entirely rather than merely improved. PCHIP is the reconstruction this basis
    // DEMANDS: monotone cubic introduces no overshoot between control points, so a basis bounded in [0,1] cannot
    // acquire a negative lobe at its sharp cyan and yellow edges — and a negative reflectance is not a spectrum,
    // which is exactly what a natural spline would manufacture there. The prior piecewise-linear walk both re-ran
    // the reconstruction per colour and kinked every basis at each control point, and those corners are where the
    // round-trip chromaticity error accumulated.
    private static readonly double[] White =   Sampled([1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0]);
    private static readonly double[] Cyan =    Sampled([0.97, 0.97, 0.97, 0.93, 0.12, 0.04, 0.0, 0.0, 0.05, 0.05]);
    private static readonly double[] Magenta = Sampled([0.99, 0.99, 0.84, 0.18, 0.03, 0.05, 0.40, 0.99, 0.99, 0.99]);
    private static readonly double[] Yellow =  Sampled([0.0, 0.0, 0.10, 0.78, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0]);
    private static readonly double[] RedB =    Sampled([0.10, 0.10, 0.0, 0.0, 0.0, 0.0, 0.84, 1.0, 1.0, 1.0]);
    private static readonly double[] GreenB =  Sampled([0.0, 0.0, 0.03, 0.49, 1.0, 1.0, 0.46, 0.0, 0.0, 0.0]);
    private static readonly double[] BlueB =   Sampled([1.0, 1.0, 0.89, 0.46, 0.06, 0.0, 0.0, 0.0, 0.0, 0.05]);

    // ToCurve is the SAMPLES the upsample produces and ToSpd their Unicolour projection — one kernel, two egresses.
    // Wacton's Spd is a one-way XYZ intake that republishes no grid, so a consumer needing the samples themselves
    // (the environment#SKY_MODEL per-band ground albedo, a durable measured curve) reads them here instead of
    // re-running the basis combination against a package that will not give them back. The wavelength of sample `i`
    // is SampleStart + i·SampleStep, so a consumer indexes the page's own declared extent and never assumes a grid.
    public static Fin<ReadOnlyMemory<double>> ToCurve(RgbSpectrum rgb, Op key) {
        double[] r = new double[SampleCount];
        double red = rgb.R, green = rgb.G, blue = rgb.B;
        if (red <= green && red <= blue) { Acc(r, White, red); if (green <= blue) { Acc(r, Cyan, green - red); Acc(r, BlueB, blue - green); } else { Acc(r, Cyan, blue - red); Acc(r, GreenB, green - blue); } }
        else if (green <= red && green <= blue) { Acc(r, White, green); if (red <= blue) { Acc(r, Magenta, red - green); Acc(r, BlueB, blue - red); } else { Acc(r, Magenta, blue - green); Acc(r, RedB, red - blue); } }
        else { Acc(r, White, blue); if (red <= green) { Acc(r, Yellow, red - blue); Acc(r, GreenB, green - red); } else { Acc(r, Yellow, green - blue); Acc(r, RedB, red - green); } }
        for (int i = 0; i < r.Length; i++) { r[i] = Math.Clamp(r[i], 0.0, 1.0); }
        return r.All(double.IsFinite)
            ? Fin.Succ((ReadOnlyMemory<double>)r)
            : Fin.Fail<ReadOnlyMemory<double>>(MaterialFault.Parameter(key, "<spectral-upsample-non-finite>"));
    }

    public static Fin<Spd> ToSpd(RgbSpectrum rgb, Op key) =>
        ToCurve(rgb, key).Bind(curve => new Spd(SampleStart, SampleStep, curve.ToArray()) switch {
            var spd => spd.IsValid ? Fin.Succ(spd) : Fin.Fail<Spd>(MaterialFault.Parameter(key, "<spd-interval-invalid>")),
        });
    private static void Acc(double[] dst, double[] basis, double w) { double c = Math.Max(0.0, w); for (int i = 0; i < dst.Length; i++) { dst[i] += basis[i] * c; } }

    private static double[] Sampled(double[] controls) =>
        Interpolate.CubicSplineMonotone(ControlNm, controls) switch {
            var curve => SampleNm.Select(nm => Math.Clamp(curve.Interpolate(nm), 0.0, 1.0)).ToArray(),
        };

    // The gamut pull-in is a kernel GamutPolicy ROW, so the check and the projection arrive paired: Perceptual the
    // default grounding, Spectral the second admitted strategy the white-furnace harness prices for ΔE cost on a
    // saturated upsampled primary, and a raw GamutMap argument beside a hand-written check-then-map is the deleted form.
    public static Fin<RgbSpectrum> SceneLinear(Unicolour colour, Op key, GamutPolicy? bound = null) =>
        (bound ?? GamutPolicy.Perceptual) switch {
            var policy => (policy.Contains(colour) ? colour : policy.Bound(colour)).RgbLinear.Triplet,
        } switch {
            var lin => RgbSpectrum.TryCreate(Math.Max(0.0, lin.First), Math.Max(0.0, lin.Second), Math.Max(0.0, lin.Third), out RgbSpectrum rgb)
                ? Fin.Succ(rgb)
                : Fin.Fail<RgbSpectrum>(MaterialFault.Gamut(key, "<non-finite-linear-rgb>")),
        };
}
```

## [03]-[TONE_MAP]

- Owner: `ToneOperator` `[SmartEnum<string>]` (aces · agx · pbr-neutral · reinhard · filmic · exposure), each row carrying its `ToneCurve` custody case — `Lowered` naming a TinyEXR operator, `Authored` carrying a curve the package does not express; `DisplayEncoding` `[SmartEnum<string>]` the transfer-and-range egress rows (srgb · display-p3 · rec2020 · rec2100-pq · rec2100-hlg); `ToneMap` the static kernel table.
- Entry: `public static RgbSpectrum Apply(ToneOperator op, RgbSpectrum sceneLinear, double exposure)` tone-maps the integrator's HDR radiance to display-linear [0,1], and `Encode` rebases the AP1 triple onto the row's pre-built `Configuration` then reads its transfer over one arity — `public static ShadeVec4 Encode(RgbSpectrum displayLinear, DisplayEncoding target)` for a single shade and `public static void Encode(ReadOnlySpan<ShadeVec4> displayLinear, DisplayEncoding target, Span<ShadeVec4> destination)` for a whole plane row in one pass — two steps, one policy row each, no per-call knob.
- Packages: Rasm (project — the `Numerics/atoms#SCALAR_FLOOR` `RgbProfile` working-space roster each display row names, its `Configuration` the one instance per space), TinyEXR.NET (composed — `ImageProcessing.ToneMap(source, destination, channels, ToneMapOperator, …)` the span fold every `Lowered` row crosses and `ToneMapOperator` its `Reinhard`/`ReinhardExtended`/`Aces`/`Hable` roster), Wacton.Unicolour (composed for the encode after tone-map — `ConvertToConfiguration` the XYZ-preserving rebase, `.Rgb` the row's transfer read), `texture#TEXTURE_UV` (composed — `ShadeVec4`, the corpus' one four-lane field register the egress lands on), `Raster/plane#PLANE_VOCABULARY` (composed — the `PlaneTransfer` row each display row names as its storage encode), Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new operator is one `ToneOperator` row carrying its `ToneCurve` case — `Lowered` where the package roster already carries the curve, `Authored` only where it does not, so admitting a curve the package gained becomes a one-case edit that DELETES a body rather than leaving two; a new display target is one `DisplayEncoding` row naming a kernel `RgbProfile` row and its `PlaneTransfer` storage row — never an author-kernel, never a loose transfer-without-range parameter; zero new surface.
- Boundary: the tone curves are the documented Unicolour NOT_COVERED concern, and the ones TinyEXR's own span fold already lowers are COMPOSED rather than re-derived — `aces`, `reinhard`, and `filmic` name `ToneMapOperator.Aces`, `ReinhardExtended`, and `Hable`, so the estate authors only what no admitted package expresses and the catalogue's RAIL_LAW Reject against a hand-rolled tone-map fold is held structurally by the `ToneCurve` split rather than by a reviewer noticing; the `filmic` row IS Hable and names that curve rather than a Hejl-Burgess-Dawson body it would otherwise have to keep beside an equivalent the package ships. What stays AUTHORED is the pair the roster lacks — the Sobotka AgX display transform (the Blender-default view transform, realized as the Wrensch minimal fit: the 3×3 inset matrix into log2 space clamped to `[−12.47393, 4.026069]`, the 6th-order sigmoid polynomial, the 3×3 outset matrix, and the `2.2`-exponent linearization back to display-linear — AgX mixes channels through its matrices, which is WHY the row column is the triple `Grade(RgbSpectrum)` and not a scalar curve; the per-channel operators ride `Map`), the Khronos PBR Neutral commerce transform (toe offset by the min channel — `x − 6.25x²` under `0.08` — rational peak compression past the `0.76` shoulder toward `1 − d²/(peak + d − 0.76)`, then desaturation toward the compressed peak by `1 − 1/(0.15·(peak − newPeak) + 1)` so clipped highlights stay neutral while every value below the shoulder passes UNCHANGED — the base-color-preserving property that makes it the material-fidelity default; channel-coupled through min/max, so it rides the triple `Grade` column like AgX), and the plain exposure-then-clamp, which is a bound rather than a tone map and therefore authors no curve at all; exposure is applied as a multiplicative scale before either custody arm reads the shade (`scene · 2^exposure`); the OETF/transfer after tone-mapping is COMPOSED through Unicolour with the transfer AND the dynamic range as ONE `DisplayEncoding` row — the Rec2100 PQ/HLG transfers scale by `DynamicRange.WhiteLuminance`, so the range rides the named `RgbProfile` row rather than this page — the HDR rows resolve `DynamicRange.High` (203-nit reference white, 1000-nit max) and the SDR rows `DynamicRange.Standard` (100-nit) as the kernel row's explicit column, never the package default, which is `High` and would silently encode an undeclared SDR row at the 203-nit HDR scale — and the row's `Configuration` is the kernel's single instance per space, so a page-local mint (the deleted form) can no longer give one working space two identities and pay a chromatic adaptation per crossing; the `rec2020` row is the wide-gamut SDR target the `RgbProfile.Rec2020` row already carries (the BT.2020 primaries under the standard range, distinct from the PQ/HLG rows that pair the same primaries with the HDR transfers); `Encode` REBASES the display-linear triple from the `PortValue.SceneLinear` AP1 working space onto the target row's configuration through `ConvertToConfiguration` (XYZ-preserving) BEFORE reading the row's `.Rgb` transfer — an AP1-linear triple relabelled target-linear was the deleted form, the primary mismatch hue-shifting every P3/Rec2100 encode (the same cross-space grounding law `weathering#WEATHERING` and `finish#FINISH` enforce inbound); the tone-mapped display-referred output is the result the app-platform raster path (`Rasm.AppUi/Charts/custom#COLOR_SPACE`) consumes downstream, never a surface Materials reaches into. The IN-FOLDER split is BY CONCERN and neither owner is the other's twin: `DisplayEncoding` owns COLORIMETRIC egress — primary rebase, transfer, and reference white — for a scene-referred `RgbSpectrum` radiance, while `Raster/plane#PLANE_FORMAT` `PlaneTransfer` owns STORAGE encode/decode of an already-display-referred plane and asserts nothing about reference white; each `DisplayEncoding` row therefore NAMES its storage transfer as a column (`Srgb`/`DisplayP3`/`Rec2020 → PlaneTransfer.Srgb`, `Rec2100Pq → PlaneTransfer.Pq`, `Rec2100Hlg → PlaneTransfer.Hlg`) so an encode reads the pairing off the colorimetric row instead of a caller pairing the two by hand, and a plane carrying `pq` or `hlg` with no `DisplayEncoding` provenance is UNANCHORED — the 203-nit reference white those transfers scale by is this page's declaration, not the plane's. The ONE consumer path is likewise stated rather than left as convention: a scene-referred `Rgba32F` plane bound for an SDR container passes `ToneMap.Apply` then `ToneMap.Encode` BEFORE `Raster/plane#TEXTURE_PLANE` `Write` narrows it, and `Encode` lands `ShadeVec4` precisely so the corpus' one quantizer at `Write` is the only narrowing site — a bare `(double, double, double)` tuple bypassed it and made the tone-map an egress with no reachable consumer. A `PlaneQuantity.Light` plane written to an integer-depth `PlaneFormat` with no display binding set is the case that must REFUSE at the press seam rather than clip silently through the sRGB OETF. CHARTERED DUPLICATION (declared, not accidental): the `Rasm.AppUi/Render/capture` `ToneMap` (a SkiaSharp per-channel `float` `SKColorFilter` LUT on the raster-encode path, its `rec2020`/HDR targets the `ColorPolicy` rows) is a DISTINCT app-platform owner — one tone-map owner per runtime, the appearance-domain `ToneOperator`/`DisplayEncoding` here grounding path-traced `RgbSpectrum` radiance through Unicolour (the `rec2020` SDR row covering capture's `Rec2020` target inbound), the capture-time Skia curve there grounding chart/document raster export; the shared Narkowicz/Reinhard coefficients are two runtimes implementing one published curve at the wire, never drift, and neither owner references the other.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// ToneCurve is the CUSTODY of a curve, and the two cases are two genuinely different evidence shapes rather than one
// wearing a flag: Lowered names an operator TinyEXR's own span fold already implements, and Authored carries a curve
// the package does not express. The split is what makes the composition law checkable — a Lowered row cannot carry a
// hand-rolled body and an Authored row cannot silently shadow a package operator, so the catalogue's RAIL_LAW
// (a hand-rolled tone-map fold this surface already lowers is a Reject) is structural rather than a review note.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ToneCurve {
    private ToneCurve() { }
    public sealed record Lowered(ToneMapOperator Operator) : ToneCurve;
    public sealed record Authored(Func<RgbSpectrum, RgbSpectrum> Grade) : ToneCurve;
}

// The curve column is the row's own custody. AgX and PBR Neutral stay AUTHORED because the package's roster carries
// neither, and both mix channels through their own matrices, so the authored face is RgbSpectrum→RgbSpectrum rather
// than double→double; the exposure row is a clamp and not a tone map at all, which is why it authors nothing but the
// bound. The `filmic` row IS Hable — the package's filmic operator — so the row names the curve it actually runs.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ToneOperator {
    public static readonly ToneOperator Aces       = new("aces",        new ToneCurve.Lowered(ToneMapOperator.Aces));
    public static readonly ToneOperator Agx        = new("agx",         new ToneCurve.Authored(ToneMap.AgxGrade));
    public static readonly ToneOperator PbrNeutral = new("pbr-neutral", new ToneCurve.Authored(ToneMap.PbrNeutralGrade));
    public static readonly ToneOperator Reinhard   = new("reinhard",    new ToneCurve.Lowered(ToneMapOperator.ReinhardExtended));
    public static readonly ToneOperator Filmic     = new("filmic",      new ToneCurve.Lowered(ToneMapOperator.Hable));
    public static readonly ToneOperator Exposure   = new("exposure",    new ToneCurve.Authored(static s => s.Map(static x => Math.Clamp(x, 0.0, 1.0))));

    public ToneCurve Curve { get; }
}

// Transfer, range, AND the storage row travel as ONE display-egress row — the Rec2100 PQ/HLG transfers
// scale by DynamicRange.WhiteLuminance, so the range is a declared column (never the package Configuration default)
// and each Configuration mints once per row, never per encode. Storage is the Raster/plane#PLANE_FORMAT PlaneTransfer
// this colorimetric row lowers ONTO, declared here because the reference white the PQ/HLG curves scale by is this
// page's fact and a plane carrying the transfer alone cannot state it — a caller pairing the two by hand is the
// deleted form, and the three SDR rows sharing PlaneTransfer.Srgb is the honest reading: primaries differ, the
// storage curve does not.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class DisplayEncoding {
    public static readonly DisplayEncoding Srgb       = new("srgb",        RgbProfile.Srgb,       PlaneTransfer.Srgb);
    public static readonly DisplayEncoding DisplayP3  = new("display-p3",  RgbProfile.DisplayP3,  PlaneTransfer.Srgb);
    public static readonly DisplayEncoding Rec2020    = new("rec2020",     RgbProfile.Rec2020,    PlaneTransfer.Srgb);
    public static readonly DisplayEncoding Rec2100Pq  = new("rec2100-pq",  RgbProfile.Rec2100Pq,  PlaneTransfer.Pq);
    public static readonly DisplayEncoding Rec2100Hlg = new("rec2100-hlg", RgbProfile.Rec2100Hlg, PlaneTransfer.Hlg);

    // The working space is the kernel row's, so the target and every perceptual value the estate rebases through it
    // share ONE Configuration instance — a page-local mint gave the same primaries a second identity and cost a
    // chromatic-adaptation round trip on each crossing, while the row's DynamicRange arrives already explicit.
    public RgbProfile Profile { get; }
    public Configuration Config => Profile.Configuration;
    public PlaneTransfer Storage { get; }
    private DisplayEncoding(string key, RgbProfile profile, PlaneTransfer storage) : this(key) =>
        (Profile, Storage) = (profile, storage);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class ToneMap {
    // ONE dispatch over the curve's custody: a lowered row crosses the package's own span fold over a three-lane
    // stack window, an authored row runs the estate's curve. Exposure applies BEFORE either, so the scale is one
    // multiply the row never sees.
    public static RgbSpectrum Apply(ToneOperator op, RgbSpectrum sceneLinear, double exposure) =>
        sceneLinear.Scale(Math.Pow(2.0, exposure)) switch {
            var exposed => op.Curve.Switch(
                state: exposed,
                lowered:  static (s, curve) => Package(s, curve.Operator),
                authored: static (s, curve) => curve.Grade(s)),
        };

    // Exemption: the package fold takes spans, so the three-lane window is a stack buffer the call writes through —
    // a Span crosses no lambda and the shade has no expression form on this side of the seam.
    static RgbSpectrum Package(RgbSpectrum sceneLinear, ToneMapOperator op) {
        Span<float> lanes = [(float)sceneLinear.R, (float)sceneLinear.G, (float)sceneLinear.B];
        Span<float> graded = stackalloc float[3];
        ImageProcessing.ToneMap(lanes, graded, channels: 3, op);
        return RgbSpectrum.Create(Math.Max(0.0, graded[0]), Math.Max(0.0, graded[1]), Math.Max(0.0, graded[2]));
    }

    // Rebase AP1→target primaries (ConvertToConfiguration, XYZ-preserving) BEFORE the transfer read — display-linear
    // means target-linear; constructing the AP1 triple AS target.Config was the hue-shifting deleted form. The result
    // lands on ShadeVec4, the corpus' one field register, so the egress feeds plane#TEXTURE_PLANE Write — the single
    // quantizer — rather than a bare tuple that bypassed it and left the whole tone-map with no reachable consumer.
    // Coverage rides through unchanged: a tone map grades radiance, never alpha.
    public static ShadeVec4 Encode(RgbSpectrum displayLinear, DisplayEncoding target) =>
        new Unicolour(PortValue.SceneLinear, ColourSpace.RgbLinear, displayLinear.R, displayLinear.G, displayLinear.B)
            .ConvertToConfiguration(target.Config).Rgb.Triplet switch { var rgb => new ShadeVec4(rgb.First, rgb.Second, rgb.Third, 1.0) };

    // Span arity serves a whole plane row in one pass, so a 4k row egresses without a per-texel Unicolour
    // round trip at the call site, and the destination is the caller's because the press already owns the staging
    // arena. A short destination is a caller defect the span slice raises, never a silent truncation.
    public static void Encode(ReadOnlySpan<ShadeVec4> displayLinear, DisplayEncoding target, Span<ShadeVec4> destination) {
        for (int i = 0; i < displayLinear.Length; i++) {   // Exemption: the [EXPRESSION_SPINE] span-kernel carve — a Span crosses no lambda, so the fold has no expression form
            ShadeVec4 texel = displayLinear[i];
            destination[i] = Encode(RgbSpectrum.Create(Math.Max(0.0, texel.X), Math.Max(0.0, texel.Y), Math.Max(0.0, texel.Z)), target) with { W = texel.W };
        }
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

- Owner: `ConductorMetal` `[SmartEnum<string>]` the measured-metal axis whose rows CARRY the per-band `ComplexIor` and their `MeasuredSource` provenance class (the data lives with the vocabulary that selects it — a parallel metal→IOR dictionary beside the axis is the deleted form, and a parallel `ConductorIor` operations class forwarding `.Ior` reads is the deleted form too: the row IS the grounding surface).
- Entry: the row's `Ior` property reads the measured complex refractive index per RGB band as one `ComplexIor` carrier (its `Eta`/`K` two validated `RgbSpectrum` bands) — `SlabStack.LowerBase` constructs the grounded `bsdf#LOBE_FAMILY` `Conductor` lobe from the row's `Ior` where the axis carries one and from `SlabStack.DielectricF0(BaseColor)` where it does not, so `Slab.Base.Conductor` is `Option<ConductorMetal>` and an unrostered alloy crosses as absence rather than as a rostered neighbour's dispersion; the metal F0 is the measured `ComplexIor` Fresnel, NEVER a hand-authored RGB albedo scaled to a guess; the normal-incidence Fresnel is the carrier's own `ComplexIor.FresnelNormal`, never a local re-derivation; `public static Option<ConductorMetal> Resolve(string family, string name)` is the one static operation on the axis — the wire-boundary resolve `interchange#MATERIAL_WIRE` feeds with the split library id.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new measured metal is one `ConductorMetal` row carrying its three-band `(η, k)` measured pair and the `MeasuredSource` class that pair came under; a new custody class is one `MeasuredSource` row; the row set is the INTERNAL leg of the measured-spectral library — the conductor rows ground here rather than carrying a hand-authored Acescg albedo, and the EXTERNAL leg is the measured `.bsdf` ingest whose reader blocks at `acquisition` `[EPFL_RGL_BRDF_LOADER]`. A full per-wavelength `refractiveindex.info` n/k spectrum is one `Curve` column on the SAME row — an `Option<Spd>` init-defaulted absent, so every existing row binds unchanged and a transcribed spectrum is data rather than a second conductor surface; a curve arriving over the `neural#MODEL_REGISTRY` spectral-reflectance stage for an UNROSTERED metal crosses as the `interchange#TEXTURE_EGRESS` `StageRequestWire`/`StageResultWire` pair — the acyclic strata forbid the AEC-domain Materials a project reference to the app-platform Compute ONNX owner, so an inferred curve arrives over the wire and never over an assembly edge — then admits through `[02]-[SPECTRAL_UPSAMPLE]` `ToSpd` per `SpectralBand` and reaches the lobe as an `Option<Spd>` the caller carries beside its `ConductorMetal`; zero new surface.
- Boundary: the complex refractive index `(η, k)` per RGB band is the physically-correct conductor F0 carried as one `ComplexIor` `[ComplexValueObject]` band — the carrier's own `ComplexIor.Fresnel(cosI)` per-band read answers from it directly, so a metal's edge tint and grazing-angle hue shift emerge from the measured dispersion rather than an artist's base-color triple; the three-band `Eta`/`K` values transcribe a measured dataset at the RGB band centres `SpectralBand.Red`/`Green`/`Blue` carry (610/550/465 nm sampled against the published 630/532/465 nm anchors) and every row declares WHICH custody class that dataset came under through its `MeasuredSource` column — the SEED_ROW_LAW per-column provenance at this page's grain, `Elemental` for a primary optical-constants transcription and `Aggregated` for a CC0 public-domain database reading that redistributes unconditionally, so admissibility is read off the row rather than inferred from a table nothing annotates; the `graph#MATERIAL_LIBRARY` conductor rows carry a measured `BaseColor` for the diffuse-substitute preview path AND name a `ConductorMetal` so the `bsdf#LOBE_FAMILY` `Conductor` lobe grounds from the named row's `Ior`, the base color the perceptual seed and the `(η, k)` the shading truth; the smart-enum KEYS align to the `graph#MATERIAL_LIBRARY` register's `metal.<name>` name column — `"chrome"` (never `"chromium"`) so `Resolve("metal", "chrome")` grounds the register's `metal.chrome` row instead of silently falling to the interchange `Iron` default; a metal absent from the rows falls back to the `graph#MATERIAL_LIBRARY` base-color-as-F0 dielectric-Schlick approximation rather than faulting — the register's `metal.steel` alloy row has no published `(η, k)` dataset and shades through exactly that fallback — so the rows ground the eight named metals and a ninth admits without a rebuild; the conductor F0 round-trips in-gamut through the `bsdf#BSDF_GOLDEN` lossless-conductor furnace row (F≡1 reflects unit energy) so a measured metal conserves energy under the Kulla-Conty multi-scatter term; the `Curve` column is the per-wavelength EXTENSION of the same measurement, the three-band `Ior` staying the fast path every lobe reads and the curve the high-fidelity path a spectral integrator resolves per `SpectralBand` through `[02]-[SPECTRAL_UPSAMPLE]` — a row carrying a curve and a row carrying only its bands differ by a column, never by a second conductor family.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// The measured complex refractive index RIDES the metal row — three-band (η, k) transcribed from the Johnson-Christy /
// refractiveindex.info datasets at the SpectralBand 610/550/465 nm centres. Keys match the graph#MATERIAL_LIBRARY
// register name column ("chrome", never "chromium"): a register/key drift silently downgrades the interchange resolve
// to the Iron default, so the register spelling is the law here.
// MeasuredSource is the SEED_ROW_LAW provenance column at this page's grain: a transcribed optical constant is only
// as reusable as its custody, so the class the value came under rides the row rather than a sentence beside the
// table. Elemental IS an elemental-metal measurement transcribed from the primary optical-constants literature;
// Aggregated IS a CC0 public-domain aggregate reading (the refractiveindex.info database class), which redistributes
// without condition and is therefore admissible verbatim; Alloy IS an engineering alloy with NO published complex
// index, whose row carries the honest absence the dielectric fallback then grounds. The column decides admissibility
// the way the neural registry's LicenseClass does — read off the source, never inferred by a reader.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MeasuredSource {
    public static readonly MeasuredSource Elemental = new("elemental");
    public static readonly MeasuredSource Aggregated = new("aggregated");
    public static readonly MeasuredSource Alloy = new("alloy");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConductorMetal {
    public static readonly ConductorMetal Gold     = new("gold",     MeasuredSource.Elemental,  ComplexIor.Create(RgbSpectrum.Create(0.183, 0.421, 1.373), RgbSpectrum.Create(3.424, 2.346, 1.770)));
    public static readonly ConductorMetal Copper   = new("copper",   MeasuredSource.Elemental,  ComplexIor.Create(RgbSpectrum.Create(0.271, 0.677, 1.316), RgbSpectrum.Create(3.609, 2.625, 2.292)));
    public static readonly ConductorMetal Aluminum = new("aluminum", MeasuredSource.Elemental,  ComplexIor.Create(RgbSpectrum.Create(1.346, 0.965, 0.617), RgbSpectrum.Create(7.475, 6.400, 5.303)));
    public static readonly ConductorMetal Silver   = new("silver",   MeasuredSource.Elemental,  ComplexIor.Create(RgbSpectrum.Create(0.159, 0.145, 0.135), RgbSpectrum.Create(3.929, 3.190, 2.381)));
    public static readonly ConductorMetal Iron     = new("iron",     MeasuredSource.Elemental,  ComplexIor.Create(RgbSpectrum.Create(2.911, 2.950, 2.585), RgbSpectrum.Create(3.089, 2.932, 2.767)));
    public static readonly ConductorMetal Chrome   = new("chrome",   MeasuredSource.Elemental,  ComplexIor.Create(RgbSpectrum.Create(2.020, 2.790, 2.020), RgbSpectrum.Create(3.860, 4.200, 3.860)));
    public static readonly ConductorMetal Titanium = new("titanium", MeasuredSource.Elemental,  ComplexIor.Create(RgbSpectrum.Create(2.741, 2.542, 2.267), RgbSpectrum.Create(3.814, 3.435, 3.039)));
    public static readonly ConductorMetal Brass    = new("brass",    MeasuredSource.Aggregated, ComplexIor.Create(RgbSpectrum.Create(0.444, 0.527, 1.094), RgbSpectrum.Create(3.695, 2.765, 1.829)));

    public MeasuredSource Source { get; }
    public ComplexIor Ior { get; }

    // Curve carries the per-wavelength measurement the three-band Ior samples: init-defaulted absent so every row binds
    // unchanged and a transcribed refractiveindex.info spectrum is one column on its own row, never a parallel
    // spectral-conductor family. Absence is TYPED — a row with no published spectrum and a row whose spectrum is
    // flat are different facts, and a zero-filled Spd would forge the second.
    public Option<Spd> Curve { get; init; }

    // The wire-boundary resolve interchange#MATERIAL_WIRE feeds with the split library id ("metal.chrome" → family
    // "metal", name "chrome"); the family token is the graph register's metal.* prefix. A miss shades through the
    // graph#MATERIAL_LIBRARY base-color-as-F0 dielectric fallback (metal.steel carries no published (η, k) dataset).
    public static Option<ConductorMetal> Resolve(string family, string name) =>
        family == "metal" && TryGet(name, out ConductorMetal? metal) ? Optional(metal) : Option<ConductorMetal>.None;
}
```

## [05]-[OPENPBR_SLAB]

- Owner: `Slab` `[Union]` the closed slab family (fuzz · coat · emission · base — its four cases ARE the slab discriminant, no parallel kind enum re-describing them); `SlabStack` the outermost-to-base layering algebra; `OpenPbrSurface` the OpenPBR parameter vector AND the one `MaterialParameters`→OpenPBR lowering (`OpenPbrSurface.Of`).
- Entry: `public static OpenPbrSurface Of(MaterialParameters p, ConductorMetal conductor)` is the SINGLE `MaterialParameters`→OpenPBR column correspondence — the standard-column vector both this stack and the `interchange#MATERIAL_WIRE` generated `WireMap.ToWire` transcription read, so the mapping is declared once and never re-minted at the wire; `public static SlabStack Lower(OpenPbrSurface surface)` derives the formal OpenPBR Surface 1.1 stack from that vector and `public static SlabStack Lower(MaterialParameters p, ConductorMetal conductor)` is the convenience overload composing `Of` then `Lower` (the row names its `ConductorMetal` through `[04]-[CONDUCTOR_IOR]`); `public Fin<(LayeredBsdf Bsdf, RgbSpectrum Emission)> ToLayered(Op key)` collapses the stack to the PAIR the integrator reads — the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf` weighted-lobe fold it shades, and the accumulated emission it adds as radiance outside that fold — the vector IS the canonical lowering, the stack the composition law, the weighted fold its energy-preserving collapse, so the renderer reads one `LayeredBsdf` and one emission term rather than losing the second at the collapse.
- Packages: Rasm (project — `UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new layering modifier is one `Slab` case carrying its albedo-scaling operator (the fuzz slab is the new closed lobe case the seven-lobe set lacked, realized as the `bsdf#LOBE_FAMILY` `Sheen` lobe at the fuzz position; the iridescent topcoat is the realized `Coat`→`bsdf#LOBE_FAMILY` `ThinFilm` lowering when the coat carries the row's `ThinFilm` film, NOT a parallel owner); a new OpenPBR parameter is one `OpenPbrSurface` column `Of` populates and `Lower` reads — the standard column set (`base`, `specular`, `transmission`, `subsurface`, `coat` — its roughness, anisotropy, and rotation alike — `fuzz`, `thin_film`, `emission`) the `graph#MATERIAL_LIBRARY` `MaterialParameters` aligns to — the TEXTURABLE `geometry` inputs (opacity, normal, tangent) stay on the `graph#MATERIAL_GRAPH` node fold, never vector columns, while `geometry_thin_walled` — a set-level boolean no texture port can carry — is the one geometry member riding the vector as `GeometryThinWalled`, reaching `Slab.Base.ThinWalled` and there indexing the transmissive lobe at unity so a shell passes light straight instead of refracting through a volume it does not enclose; zero new surface. The `interchange#MATERIAL_WIRE` `MaterialWire` is the OpenPBR-vector wire projection this stack's `OpenPbrSurface` defines and the TS/Py consumers decode — `interchange` composes `OpenPbrSurface.Of`, never a second `MaterialParameters`→OpenPBR mint.
- Law: the slab stack is the formal OpenPBR Surface 1.1 layering order outermost-to-base — `fuzz` over `coat` over `emission` over the `base` substrate — composed by albedo-scaling layering operators, NOT the additive convex-combination weight fold `bsdf#LAYERED_COMPOSITION` predated: `RemainingEnergy` cascades `pass ← pass · (1 − w · E(slab))` over the placed outer slabs, where `E(slab)` is that slab's lobe-specific normal-incidence directional albedo from `bsdf#LOBE_FAMILY` `MultiScatter.DirectionalAlbedo` at the lobe's OWN GGX alpha (`LobeAlpha` reads `AlphaX`/`AlphaY` per case), so a rough coat occludes more of the base than a smooth one and the energy split is a function of the real lobe, never a fixed constant; the base substrate is the FULL OpenPBR mixing chain lowered to fold rows — metalness splits conductor (grounded from the `[04]-[CONDUCTOR_IOR]` rows) vs dielectric, `transmission` splits the dielectric into the transmissive interface vs the opaque body, `subsurface` splits the body into Burley diffusion vs glossy-diffuse, and glossy-diffuse is the reflect-only parameterized-IOR specular over the diffuse floor at the F0-scaled multi-scatter albedo split — every fractional weight is rows of the one fold with the 0/1 ends collapsing, never a winner-take-all gate.
- Law: `Anisotropy` is the Disney ASPECT RATIO, not an axis assignment — `alphaX = alpha/aspect` is the TANGENT-aligned roughness and `alphaY = alpha·aspect` the bitangent-aligned one, both read in the `bsdf#SHADING_FRAME` `ShadingFrame` basis, so the frame's X axis IS the anisotropy reference and a consumer supplying an arbitrary tangent renders an arbitrary highlight. `SpecularRotation` is what closes that: it turns the reference within the frame by a declared angle rather than leaving it to whatever tangent the geometry happened to carry, so a brushed grain is a material fact the row states and the frame stays the geometric basis every isotropic lobe shares. It travels UNIT on the vector and the wire (the OpenPBR/`.mtlx` convention where 1 is a half turn), converts to radians exactly once at `Lower`, and rides the lobe rather than the frame — a rotation baked into `ShadingFrame` would turn every isotropic sibling with it.
- Law: the COAT carries that same pair and reads that same remap. `CoatAnisotropy` and `CoatRotation` are the OpenPBR 1.1 `coat_roughness_anisotropy` group, and `SlabStack.Aspect` is the ONE owner both the coat lowering and the substrate lowering call — a second remap spelling would let a brushed lacquer and the brushed substrate under it disagree about what one anisotropy column means, and `LowerBase`'s reflect and transmit alphas both run through it too, so three consumers share one derivation. The grain DIRECTION reaching shading as `CoatRotation` IS the `geometry_coat_tangent` channel's Rasm-side consumer: a tangent-VECTOR plane averages two opposed tangents to zero under `MipPolicy.Box` where the rotation scalar averages meaningfully, so the tangent port stays the `.mtlx` egress and the frame evidence a peer supplies, exactly as `geometry_tangent` does. The coat's pair threads BOTH rows of the film mix, so an iridescent brushed topcoat keeps its highlight at every `thin_film_weight` rather than losing it the moment the interference lobe takes the weight.
- Law: a ROTATION is the OpenPBR-canonical carrier for anisotropy direction and a tangent VECTOR plane is not: a scalar rotation plane mips correctly under `MipPolicy.Box` where averaging two opposed tangent vectors cancels to zero, so `geometry_tangent` stays what it honestly is — the `.mtlx` egress port and the frame evidence a peer consumer supplies — and never a Rasm-side shading input.
- Boundary: `OpenPbrSurface.Of` is the ONE `MaterialParameters`→OpenPBR construction and `SlabStack.Lower` the ONE vector→slab lowering — a per-material slab builder or a second wire-side OpenPBR mint is the deleted form; the fuzz slab lowers to a `Sheen` lobe weighted by `fuzz_weight` and colored by the `fuzz_color` column `Of` mints as the authored `MaterialParameters.FuzzColor` under the Disney `SheenTint` base-hue bias (identity at tint 0, so an authored three-band tint — a `weathering#WEATHERING` `FuzzColorTo` terminal, an ingested `fuzz_color` plane's bound mean, or a library row that names one — reaches the slab and the wire at full precision where the prior white→base lerp collapsed it to one luminance scalar; a row leaving the column at its `PortValue.White` neutral takes that lerp as the DEGENERATE case of the same algebra rather than as a second path), the coat slab to its `thin_film_weight` MIX — the plain `Clearcoat` dielectric and the `bsdf#LOBE_FAMILY` `ThinFilm` interference lobe (film thickness/IOR from the coat's validated `ThinFilm` carrier, the coat IOR lifted into a real `ComplexIor` base — the Belcour-Barla spectral interference the `finish#FINISH` pearlescent topcoat drives) split by the film weight as two rows of the one weighted fold, the 0/1 ends collapsing to a single lobe, BOTH rows built on the coat's own `Aspect`-remapped alpha pair and rotation so the grain survives the mix; the film columns SOURCE from `MaterialParameters.Film` through `Of` (a hardcoded zero triple was the deleted form — it dead-ended this lowering and shipped permanent zeros on the `interchange#MATERIAL_WIRE` `thin_film` columns), the coat's `coat_color` reaching shading as the `Clearcoat` lobe's `Tint` — a coloured lacquer ABSORBS on the way through, so the tint rides the lobe's `Transmitted` throughput onto every layer beneath it and never into its own reflected specular, which stays achromatic as a dielectric interface must; the emission slab lowering to NO lobe and instead accumulating `Radiance · Luminance` under the pass tint on the collapse's own emission field, energy-additive and never occluding, which the integrator adds ONCE per shading point outside the BSDF estimator so it is never multiplied by a cosine or divided by a pdf — the `graph#MATERIAL_GRAPH` `BsdfOutput` sink's `Emission` port and the `SurfaceShade.EmissionLinear` column are the row-side terminal and this pair the slab-side one; and the base substrate lowering to the metalness → transmission → subsurface mixing chain — a `Conductor` lobe (grounded from the `ConductorMetal` the row names through `[04]-[CONDUCTOR_IOR]`) against the dielectric rows, the transmissive `Dielectric` against the opaque body (indexed at unity where the row is THIN-WALLED, because a shell's two interfaces sit one wall apart and the substrate's index would bend a ray through a volume the geometry does not enclose — the reflected specular keeps `SpecularIor`, so the flag splits the two arms and never re-indexes the substrate), the `Subsurface` diffusion against the glossy-diffuse pair (the reflect-only `Dielectric` specular at `SpecularIor` over the `Diffuse` floor), each fractional weight two rows of the one fold — the `Subsurface` lobe reading the validated three-band `SubsurfaceRadius` `[ComplexValueObject]` carrier's `Magnitude` for the Burley diffusion radius (the carrier declared on `graph#MATERIAL_LIBRARY` `MaterialParameters`, gating a negative or non-finite millimetre mean-free-path once at `Create` so no `Vector3d` scatter vector threads the slab signatures); `ToLayered` collapses the albedo-scaled slab weights into the `bsdf#LAYERED_COMPOSITION` `LayeredBsdf.Of` normalized lobe list so the integrator shades one `LayeredBsdf` and never re-derives the slab nesting per sample — the per-lobe albedo-scaling is computed once at lowering, the energy each outer slab leaves for the layer below (`1 − w · E(slab)` at the lobe's own alpha) baked into the lobe weight and the SPECTRAL tint it leaves carried on the row's own `Throughput` column, the two axes kept apart because a hue folded into a `UnitInterval` weight would be re-normalized away as though it were energy the stack redistributed; the OpenPBR z-up local-frame convention matches the `bsdf#SHADING_FRAME` `LocalVector<T>` basis so no slab re-derives `cosθ`; slab-weight admission is TOTAL — `Of` clamps the OpenPBR columns and the `Weight` helper collapses every `w · pass` into `[0,1]` before `UnitInterval.Create` through a comparison-ordered floor (`Math.Clamp` propagates NaN, so a non-finite consumer weight lands at zero rather than throwing), so no `Slab` weight throws the value-object guard mid-fold, and the one fault site is `ToLayered`→`LayeredBsdf.Of` railing `MaterialFault.Parameter` when every lobe weight filters to zero (a degenerate empty stack), never a propagated energy gain; the `weathering#WEATHERING` aging operator targets the slab columns directly once lowered (chalking raises `coat_roughness`, soiling raises `fuzz_weight`, patina greens the `Slab.Base` color and drops its `Metalness` toward zero — the conductor corrodes to a dielectric verdigris, never a metal-to-metal `ConductorMetal` swap the 8-member smart-enum cannot represent) through the `SurfaceDelta` carried on each `WeatheringEffect` policy row — whose `SurfaceColumn` roster DERIVES from this vector's own constructor, so the coat anisotropy and rotation columns are targetable the moment they land here and neither page edits for the other.

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
    // Anisotropy and Rotation are the coat's OWN grain, the same pair the base carries and remapped through the same
    // Aspect owner: OpenPBR 1.1 states coat_roughness_anisotropy as a first-class input, and a brushed lacquer over a
    // brushed substrate is a real finish whose two layers must read one meaning of the column. Rotation is RADIANS,
    // converted once at Lower from the vector's unit column exactly as the base's is.
    // Film is the graph#MATERIAL_LIBRARY validated thin_film carrier; ThinFilm.None the film-free zero — never a
    // loose (Option<double>, double) pair re-describing it.
    public sealed record Coat(
        double Weight, RgbSpectrum Color, double Roughness, double Anisotropy, double Rotation, double Ior, ThinFilm Film) : Slab;
    public sealed record Emission(RgbSpectrum Radiance, double Luminance) : Slab;
    // Weight, DiffuseRoughness, SpecularWeight, and SpecularTint are the four OpenPBR base columns the vector always
    // carried and the lowering once dropped — base_weight dims the whole substrate, base_diffuse_roughness IS the
    // Oren-Nayar term (distinct from the SPECULAR roughness the same row also carries), specular_weight dims the
    // Fresnel lobe, and SpecularTint is the resolved specular_color the dielectric interface reflects through.
    // Rotation is the anisotropy reference azimuth in RADIANS, converted once here from the vector's unit column.
    public sealed record Base(
        double Weight, double Metalness, Option<ConductorMetal> Conductor, RgbSpectrum BaseColor,
        double DiffuseRoughness, double SpecularWeight, RgbSpectrum SpecularTint,
        double Roughness, double SpecularIor, double Anisotropy, double Rotation,
        double Transmission, double TransmissionRoughness,
        double Subsurface, SubsurfaceRadius SubsurfaceRadius,
        // ThinWalled carries the vector's geometry_thin_walled flag to the ONE arm that reads it. Thin shells hold
        // no interior to refract through — both interfaces sit one wall apart — so a transmissive lobe passes
        // straight while SpecularIor still governs the reflected specular. Leaving the flag on the wire and out of
        // this slab shipped every leaf, curtain, and lampshade to every peer as a solid whose back face bends light
        // through a volume that does not exist, while Rasm's own integrator read that same row as solid glass.
        bool ThinWalled) : Slab;
}

// --- [MODELS] ------------------------------------------------------------------------------
// The OpenPBR Surface 1.1 parameter vector: the ONE MaterialParameters->OpenPBR correspondence (Of) every consumer
// reads. SlabStack.Lower derives the slab family FROM this vector and interchange#MATERIAL_WIRE projects OpenPbrGroupsWire
// FROM it, so the column mapping is declared once here and never re-minted at the wire (the DERIVED_LOGIC primary).
public readonly record struct OpenPbrSurface(
    double BaseWeight, RgbSpectrum BaseColor, double BaseMetalness, double BaseDiffuseRoughness, double BaseSpecularTint,
    double SpecularWeight, RgbSpectrum SpecularColor, double SpecularRoughness, double SpecularIor,
    double SpecularAnisotropy, double SpecularRotation,
    double TransmissionWeight, double TransmissionRoughness,
    double SubsurfaceWeight, SubsurfaceRadius SubsurfaceRadius,
    double CoatWeight, double CoatRoughness, double CoatAnisotropy, double CoatRotation, double CoatIor, RgbSpectrum CoatColor,
    double FuzzWeight, double FuzzRoughness, RgbSpectrum FuzzColor,
    double ThinFilmWeight, double ThinFilmThickness, double ThinFilmIor,
    RgbSpectrum EmissionColor, double EmissionLuminance,
    bool GeometryThinWalled,
    Option<ConductorMetal> Conductor) {

    // Both COLOUR-plus-SCALAR tint pairs lower by ONE algebra, stated once: the vector's colour column is the
    // authored OpenPBR tint and the Disney scalar biases it toward the base hue, so SpecularColor · lerp(White,
    // BaseColor, BaseSpecularTint) and FuzzColor · lerp(White, BaseColor, SheenTint) are the same expression over two
    // groups. MaterialX 1.39 open_pbr_surface has NO base_specular_tint input, so the .mtlx egress ships the RESOLVED
    // SpecularColor column verbatim — the product is already in it and a second bias at the egress applied the tint
    // twice — and the wire keeps both columns because a consumer re-deriving the product from one of them cannot.
    // Every colour column has a real MaterialParameters source: the weathering CoatColorTo/FuzzColorTo trajectories
    // reach coat_color and fuzz_color, and the bakeable coat_color/specular_color/fuzz_color planes bind all three
    // through Raster/set#SET_BIND — so the prior synthesized
    // White, and the SheenTint white→base lerp that stood in for a fuzz colour and collapsed an ingested three-band
    // tint to one luminance scalar at every bind, are the deleted forms. The thin_film group reads the row's
    // validated ThinFilm carrier (p.Film, ThinFilm.None on a film-free row); CoatIor synthesizes the OpenPBR
    // coat_ior default 1.6 (no MaterialParameters coat-IOR column; 1.5 was the Disney clearcoat default mislabelled
    // as the OpenPBR one). SpecularRotation and CoatRotation both stay the UNIT column the wire and the .mtlx ports
    // carry — Lower converts each to radians once, so a frame-facing angle exists at exactly one hop on either group,
    // and the coat's anisotropy rides beside its rotation so the pair reaches the lobe together or not at all. GeometryThinWalled reads the row's
    // ThinWalled shell flag — the ONE geometry-group member that IS a vector column, because it is a set-level
    // boolean no texture port can carry; the texturable geometry inputs (opacity, normal, tangent) stay on the
    // graph#MATERIAL_GRAPH node fold.
    public static OpenPbrSurface Of(MaterialParameters p, Option<ConductorMetal> conductor) =>
        new(BaseWeight: 1.0, AcescgRgb(p.BaseColor), p.Metalness, p.BaseDiffuseRoughness, Math.Clamp(p.SpecularTint, 0.0, 1.0),
            SpecularWeight: 1.0, Tinted(AcescgRgb(p.SpecularColor), AcescgRgb(p.BaseColor), p.SpecularTint),
            p.Roughness, p.Ior,
            p.Anisotropy, Math.Clamp(p.AnisotropyRotation, 0.0, 1.0),
            p.Transmission, p.TransmissionRoughness,
            p.Subsurface, p.SubsurfaceRadius,
            p.Clearcoat, p.ClearcoatRoughness, p.ClearcoatAnisotropy, Math.Clamp(p.ClearcoatAnisotropyRotation, 0.0, 1.0), CoatIor: 1.6, AcescgRgb(p.CoatColor),
            p.Sheen, FuzzRoughness: Math.Max(1e-3, p.Roughness),
            Tinted(AcescgRgb(p.FuzzColor), AcescgRgb(p.BaseColor), p.SheenTint),
            ThinFilmWeight: p.Film.Weight, ThinFilmThickness: p.Film.ThicknessNm, ThinFilmIor: p.Film.Ior,
            AcescgRgb(p.Emission), p.EmissionLuminance,
            GeometryThinWalled: p.ThinWalled,
            conductor);

    // Tinted IS that algebra for both groups — the authored colour attenuated by the Disney scalar's white→base bias,
    // identity at tint 0 so an untinted row lowers its authored colour unchanged.
    internal static RgbSpectrum Tinted(RgbSpectrum authored, RgbSpectrum baseHue, double tint) =>
        authored.Mul(RgbSpectrum.White.Lerp(baseHue, Math.Clamp(tint, 0.0, 1.0)));

    internal static RgbSpectrum AcescgRgb(Unicolour colour) =>
        colour.RgbLinear switch { var lin => RgbSpectrum.Create(Math.Max(0.0, lin.R), Math.Max(0.0, lin.G), Math.Max(0.0, lin.B)) };
}

public sealed record SlabStack(Seq<Slab> Slabs) {
    // The convenience entry the library/finish drive: lower the row to the canonical OpenPbrSurface vector, then to slabs.
    public static SlabStack Lower(MaterialParameters p, Option<ConductorMetal> conductor) => Lower(OpenPbrSurface.Of(p, conductor));

    // The ONE OpenPBR construction: the outermost-to-base slab order derived from the vector columns. fuzz over coat over
    // emission over base; the coat re-mints the vector's thin_film columns into the validated ThinFilm carrier (the
    // clamps keep Lower total on a consumer-built vector — the same admission posture as Weight) so an iridescent
    // topcoat rides the existing Coat slab rather than a parallel owner; the base names its ConductorMetal for [04].
    public static SlabStack Lower(OpenPbrSurface s) =>
        new(Seq<Slab>()
            .Add(new Slab.Fuzz(s.FuzzWeight, s.FuzzColor, s.FuzzRoughness))
            .Add(new Slab.Coat(s.CoatWeight, s.CoatColor, s.CoatRoughness, s.CoatAnisotropy, s.CoatRotation * Math.PI, s.CoatIor,
                ThinFilm.Create(Math.Clamp(s.ThinFilmWeight, 0.0, 1.0), Math.Max(0.0, s.ThinFilmThickness), Math.Max(1.0, s.ThinFilmIor))))
            .Add(new Slab.Emission(s.EmissionColor, s.EmissionLuminance))
            .Add(new Slab.Base(s.BaseWeight, s.BaseMetalness, s.Conductor, s.BaseColor,
                s.BaseDiffuseRoughness, s.SpecularWeight, s.SpecularColor,
                s.SpecularRoughness, s.SpecularIor, s.SpecularAnisotropy, s.SpecularRotation * Math.PI,
                s.TransmissionWeight, s.TransmissionRoughness, s.SubsurfaceWeight, s.SubsurfaceRadius,
                s.GeometryThinWalled)));

    // Energy-preserving collapse to the bsdf#LAYERED_COMPOSITION fold, returning the TWO terminal quantities the
    // integrator needs: each slab transmits 1-E(slab) of the ENERGY below it (E the slab's own directional albedo
    // from bsdf#LOBE_FAMILY MultiScatter.DirectionalAlbedo at the slab's roughness) and its own spectral Transmitted
    // TINT, so the outermost fuzz/coat attenuate the layers below ONCE at lowering; the emission slab accumulates
    // BESIDE the lobe list under the same pass tint (a coloured lacquer filters what glows under it exactly as it
    // filters what reflects under it) because self-emission is energy the surface ADDS, not energy the normalized
    // convex fold redistributes — a lobe carrying it would be divided by a pdf and cosine that describe scattering.
    public Fin<(LayeredBsdf Bsdf, RgbSpectrum Emission)> ToLayered(Op key) =>
        Slabs.Fold((Lobes: Seq<LobeWeight>(), Emission: RgbSpectrum.Black), static (acc, slab) =>
            LowerSlab(slab, RemainingPass(acc.Lobes)) switch {
                var lowered => (acc.Lobes + lowered.Lobes, acc.Emission.Add(lowered.Emission)),
            }) switch {
            var folded => LayeredBsdf.Of(folded.Lobes.Filter(static l => l.Weight.Value > 0.0), key)
                .Map(bsdf => (Bsdf: bsdf, Emission: folded.Emission)),
        };

    // What the placed outer slabs leave for the layer below, on TWO independent axes: each lobe attenuates energy by
    // its OWN directional albedo at the lobe's roughness (the slab's alpha), not a fixed constant, so a rough coat
    // occludes more than a smooth one; and each lobe multiplies its own Transmitted tint, so a coloured coat filters
    // the layers beneath by hue without that hue ever entering the scalar weight the normalization owns.
    private static (double Energy, RgbSpectrum Tint) RemainingPass(Seq<LobeWeight> placed) =>
        placed.Fold((Energy: 1.0, Tint: RgbSpectrum.White), static (pass, lw) => (
            Math.Clamp(pass.Energy * (1.0 - lw.Weight.Value * MultiScatter.DirectionalAlbedo(LobeAlpha(lw.Lobe), 1.0)), 0.0, 1.0),
            pass.Tint.Mul(lw.Lobe.Transmitted)));

    // The single-scatter alpha a lobe occludes at: the ROUGHER axis of the lobe's own GGX pair, the diffuse and
    // subsurface arms full-rough. All four glossy-reflect lobes now carry that pair, so the five arms read one shape
    // and no case reaches through a static projection its neighbour does not have.
    private static double LobeAlpha(BsdfLobe lobe) => lobe.Switch(
        diffuse:    static _ => 1.0,
        conductor:  static c => Math.Max(c.AlphaX, c.AlphaY),
        dielectric: static g => Math.Max(g.AlphaX, g.AlphaY),
        sheen:      static s => Microfacet<double>.AlphaOf(s.Roughness),
        clearcoat:  static c => Math.Max(c.AlphaX, c.AlphaY),
        subsurface: static _ => 1.0,
        thinFilm:   static f => Math.Max(f.AlphaX, f.AlphaY));

    // The ONE Disney aspect remap, read by the coat AND the substrate: alphaX is the tangent-aligned roughness and
    // alphaY the bitangent-aligned one, the EXACT inverse of the acquisition#ACQUISITION projection
    // Anisotropy = (1 − min/max)/0.9, so a goniophotometer capture round-trips through the lowering it once died at.
    // The aspect floors at sqrt(0.1) ≈ 0.3162 — never zero — so no divide guard exists, and the graph#MATERIAL_LIBRARY
    // unit gate already forbids an out-of-band Anisotropy reaching here. A second remap spelling at the coat would
    // let a brushed lacquer and the brushed substrate under it disagree about what one anisotropy column means.
    internal static (double X, double Y) Aspect(double alpha, double anisotropy) =>
        Math.Sqrt(1.0 - (0.9 * Math.Clamp(anisotropy, 0.0, 1.0))) switch { var a => (alpha / a, alpha * a) };

    // ONE switch owns both lowering products: the lobes a slab contributes and the radiance it ADDS. The fuzz lowers
    // to a Sheen lobe and the coat to its film-weight mix, both attenuated by the pass the outer layers leave; the
    // emission slab contributes NO lobe and instead folds Radiance·Luminance under the pass tint — the same
    // EmissionLuminance-carries-the-energy law graph#MATERIAL_GRAPH states — so the emission is a fold accumulator
    // rather than an eighth lobe the closed set does not carry.
    private static (Seq<LobeWeight> Lobes, RgbSpectrum Emission) LowerSlab(Slab slab, (double Energy, RgbSpectrum Tint) pass) => slab.Switch(
        state: pass,
        fuzz:     static (p, f) => (f.Weight > 0.0 ? Seq(new LobeWeight(new BsdfLobe.Sheen(f.Color, f.Roughness), Weight(f.Weight, p.Energy), p.Tint)) : Seq<LobeWeight>(), RgbSpectrum.Black),
        coat:     static (p, c) => (c.Weight > 0.0 ? CoatLobes(c, p) : Seq<LobeWeight>(), RgbSpectrum.Black),
        emission: static (p, e) => (Seq<LobeWeight>(), e.Radiance.Scale(Math.Max(0.0, e.Luminance)).Mul(p.Tint)),
        @base:    static (p, b) => (LowerBase(b, p), RgbSpectrum.Black));

    // OpenPBR thin_film_weight is a MIX weight (film coverage over the coat interface), never a >0 gate: a fractional
    // film splits the coat between the plain Clearcoat dielectric and the Belcour-Barla ThinFilm interference lobe
    // (the coat IOR lifted into a real ComplexIor base) as two rows of the one weighted fold; 0 and 1 collapse to the
    // single-lobe ends, so the mix costs nothing at the common extremes.
    // The coat's grain threads BOTH rows: the alpha pair comes off the shared Aspect owner at the coat's own
    // anisotropy and the rotation rides each lobe, so a brushed lacquer keeps its highlight whether the film weight
    // lands it on the plain dielectric, on the interference lobe, or split across both.
    private static Seq<LobeWeight> CoatLobes(Slab.Coat c, (double Energy, RgbSpectrum Tint) pass) {
        double fw = c.Film.Weight;
        (double ax, double ay) = Aspect(Microfacet<double>.AlphaOf(c.Roughness), c.Anisotropy);
        BsdfLobe clear = new BsdfLobe.Clearcoat(c.Weight, ax, ay, c.Rotation, c.Color);
        BsdfLobe film = new BsdfLobe.ThinFilm(c.Film.ThicknessNm, c.Film.Ior, ax, ay, c.Rotation,
            ComplexIor.Create(RgbSpectrum.Create(c.Ior, c.Ior, c.Ior), RgbSpectrum.Black));
        return fw <= 0.0 ? Seq(new LobeWeight(clear, Weight(c.Weight, pass.Energy), pass.Tint))
             : fw >= 1.0 ? Seq(new LobeWeight(film, Weight(c.Weight, pass.Energy), pass.Tint))
             : Seq(new LobeWeight(clear, Weight(c.Weight * (1.0 - fw), pass.Energy), pass.Tint), new LobeWeight(film, Weight(c.Weight * fw, pass.Energy), pass.Tint));
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
    private static Seq<LobeWeight> LowerBase(Slab.Base b, (double Energy, RgbSpectrum Tint) pass) {
        double alpha = Microfacet<double>.AlphaOf(b.Roughness), transmissionAlpha = Microfacet<double>.AlphaOf(b.TransmissionRoughness);
        (double alphaX, double alphaY) = Aspect(alpha, b.Anisotropy);
        (double transAlphaX, double transAlphaY) = Aspect(transmissionAlpha, b.Anisotropy);
        double weight = Math.Clamp(b.Weight, 0.0, 1.0);
        double metalness = Math.Clamp(b.Metalness, 0.0, 1.0);
        double transmission = Math.Clamp(b.Transmission, 0.0, 1.0), subsurface = Math.Clamp(b.Subsurface, 0.0, 1.0);
        double f0 = Math.Pow((b.SpecularIor - 1.0) / (b.SpecularIor + 1.0), 2.0);
        // specular_weight DIMS the Fresnel lobe rather than gating it: the share the specular takes from the diffuse
        // floor scales by the column, so the energy it releases lands on the diffuse arm and the split stays a
        // partition. A >0 gate here made specular_weight a bake-only column no shade could read.
        double specularShare = Math.Clamp(f0 * MultiScatter.DirectionalAlbedo(alpha, 1.0) * Math.Clamp(b.SpecularWeight, 0.0, 1.0), 0.0, 1.0);
        double opaque = (1.0 - metalness) * (1.0 - transmission);
        // base_weight scales every emitted row, so the whole substrate dims as one and the layering cascade above
        // reads the dimmed energy rather than a substrate that ignored its own weight.
        Seq<LobeWeight> Row(BsdfLobe lobe, double w) => w > 0.0 ? Seq(new LobeWeight(lobe, Weight(weight * w, pass.Energy), pass.Tint)) : Seq<LobeWeight>();
        // Thin shells transmit at unit index: front and back interfaces sit one wall apart, so a ray leaves
        // parallel to how it entered and the substrate's own index would bend it through geometry that carries no
        // such volume. Reflection below keeps b.SpecularIor, since a shell's front face IS a real dielectric
        // interface — so the flag splits both arms rather than re-indexing the whole substrate.
        double transmissionIor = b.ThinWalled ? 1.0 : b.SpecularIor;
        // The conductor arm is TYPED-ABSENT rather than defaulted: an alloy the eight measured rows do not carry has
        // no published (eta, k), and grounding it on a rostered neighbour ships that neighbour's dispersion as
        // though it were measured. Absence therefore routes to the DIELECTRIC grounding the base colour already
        // states — the same lobe, the same Fresnel kernel, an index inverted from the authored F0 — so the fallback
        // is one arm of the same fold and every unrostered row shades by the law this page declares.
        ComplexIor conductorIor = b.Conductor.Match(Some: static metal => metal.Ior, None: () => DielectricF0(b.BaseColor));
        return Row(new BsdfLobe.Conductor(conductorIor, alphaX, alphaY, b.Rotation), metalness)
             + Row(new BsdfLobe.Dielectric(transmissionIor, transAlphaX, transAlphaY, b.Rotation, b.SpecularTint, b.BaseColor), (1.0 - metalness) * transmission)
             + Row(new BsdfLobe.Subsurface(b.BaseColor, b.SubsurfaceRadius.Magnitude), opaque * subsurface)
             + Row(new BsdfLobe.Dielectric(b.SpecularIor, alphaX, alphaY, b.Rotation, b.SpecularTint, RgbSpectrum.Black), opaque * (1.0 - subsurface) * specularShare)
             + Row(new BsdfLobe.Diffuse(b.BaseColor, b.DiffuseRoughness), opaque * (1.0 - subsurface) * (1.0 - specularShare));
    }

    // DielectricF0 grounds an UNROSTERED conductor as the real dielectric whose normal-incidence Fresnel IS the
    // authored base colour: inverting Schlick's F0 = ((eta − 1)/(eta + 1))² per band gives eta = (1 + √F0)/(1 − √F0)
    // at k = 0, so the fallback rides the SAME Conductor lobe and the SAME Fresnel kernel the measured rows do rather
    // than a parallel approximation arm the multi-scatter cascade would have to special-case. Each band clamps below
    // unity because a reflectance of exactly one is a perfect mirror carrying no finite index — the alloy rows this
    // serves sit far below it, and the clamp is the degenerate guard rather than a tuned floor.
    internal static ComplexIor DielectricF0(RgbSpectrum baseColor) =>
        ComplexIor.Create(RgbSpectrum.Create(Eta(baseColor.R), Eta(baseColor.G), Eta(baseColor.B)), RgbSpectrum.Black);

    static double Eta(double reflectance) =>
        Math.Sqrt(Math.Clamp(reflectance, 0.0, 1.0 - 1e-6)) switch { var root => (1.0 + root) / (1.0 - root) };

    // The one weight-admission helper: every slab weight collapses into [0,1] before UnitInterval.Create so a consumer-built
    // Slab carrying an out-of-band weight never throws the ValueObject guard mid-fold. Comparison-ordered, not Math.Clamp —
    // Clamp propagates NaN, so a non-finite weight (NaN roughness poisoning the albedo cascade) collapses to zero instead.
    private static UnitInterval Weight(double w, double pass) =>
        UnitInterval.Create(w * pass is var scaled && scaled >= 0.0 ? Math.Min(scaled, 1.0) : 0.0);
}
```

## [06]-[RESEARCH]

(none)

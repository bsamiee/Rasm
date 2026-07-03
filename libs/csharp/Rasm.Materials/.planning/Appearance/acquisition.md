# [MATERIALS_ACQUISITION]

THE MEASURED-MATERIAL IMPORT PATH. One `Acquisition` static fold over the closed `CaptureSource` `[Union]` (measured-brdf · svbrdf-map · spectral-reflectance) lands an acquired real-world material as a `graph#MATERIAL_LIBRARY` `MaterialParameters` row carrying a measured `Provenance` receipt, so the appearance engine shades real captured materials rather than authored approximations. An acquired material is NEVER a second material owner: `Acquisition.Import` takes the capture data and returns a `MaterialParameters` row the SAME `graph#MATERIAL_LIBRARY` registers and the SAME `bsdf#LOBE_FAMILY` shades — a goniophotometer BRDF, a neural SVBRDF field, and a spectrophotometer reflectance curve all produce one `MaterialParameters` row, never a `MeasuredMaterial`/`AcquiredBrdf` type. The import is a data-import concern distinct from the measured-spectral grounding of existing rows: it admits the capture, fits the closed parameter vector by the `csharp:Rasm.Compute/blas#DENSE_ALGEBRA` dense-route doctrine shape, grounds the base color through the `surface#SPECTRAL_UPSAMPLE` `Spd` construction, and gates the round-trip in-gamut through the white-furnace harness. The fit's forward model is NOT a re-minted lobe — it COMPOSES the `bsdf#MICROFACET_KERNEL` `Microfacet.Ndf`/`MaskingShadowing`/`FresnelDielectric`/`FresnelConductor` over the REAL `bsdf#SHADING_FRAME` `LocalVector` incident/outgoing directions reconstructed from each `BrdfSample` `(θi, θo, Δφ)`, so the GGX/Smith/Fresnel math is single-sourced on the kernel and the design matrix carries the true microfacet half-vector, never a `cos((θi−θo)/2)` specular-plane proxy that ignores the azimuth; the fit target is the ANISOTROPIC `(αx, αy)` lobe under a conductor/dielectric Fresnel discriminant (`Option<ComplexIor>` on the capture — `Some` a measured complex IOR routing `FresnelConductor`, `None` the dielectric fit over the IOR seed), so a goniophotometer capture of brushed metal fits to the same lobe space `bsdf#LOBE_FAMILY` shades, never a rough dielectric with a hardcoded metalness. The page composes `surface#SPECTRAL_UPSAMPLE` for the `Spd`→scene-linear color, `graph#MATERIAL_LIBRARY` `MaterialParameters` for the produced row, `bsdf#MICROFACET_KERNEL`/`bsdf#SHADING_FRAME` for the single-sourced forward model, Wacton.Unicolour directly for the spectral→XYZ→Acescg conversion, and the `MaterialFault` (`FaultBand.Material`) rail for a malformed or out-of-gamut capture. The `Provenance` receipt this page OWNS — its `CaptureMethod` discriminant, the angular-sample/spectral-band counts, the `FitResidual` beside the `Svd`-witnessed `FitConditionNumber`/`FitRank` conditioning evidence, and the grounded-color chromaticity readout (`DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv`) — is the measured evidence the `interchange#MATERIAL_WIRE` `WireProvenance` carries and the `finish#FINISH` Kubelka-Munk pigment-mix receipt reuses, so every measured-vs-authored distinction reads one receipt shape.

## [01]-[INDEX]

- [01]-[ACQUISITION]: the `CaptureSource` `[Union]` capture family, the `BrdfSample` angular-reflectance record, the `Provenance` measured-capture receipt over the `CaptureMethod` `[SmartEnum]` instrument discriminant (counts, conditioning, and chromaticity columns beside the residual), the `Microfacet`-composed anisotropic conductor/dielectric forward model over reconstructed `LocalVector` directions, the thin-`QR` Gauss-Newton `Acquisition.Import` fit-and-ground fold over the bounded `(αx, αy[, η])` parameter vector producing a `MaterialParameters` row, and the per-texel SVBRDF field average.

## [02]-[ACQUISITION]

- Owner: `Acquisition` static import fold; `CaptureSource` `[Union]` (measured-brdf · svbrdf-map · spectral-reflectance); `BrdfSample` the goniophotometer angular-reflectance record over `(θi, θo, Δφ)`; `CaptureMethod` `[SmartEnum<string>]` the measurement-instrument discriminant (goniophotometer · spectrophotometer · neural-svbrdf · authored); `Provenance` the measured-capture receipt this page owns, keyed by `CaptureMethod` and carrying the angular-sample and spectral-band COUNTS, the `FitConditionNumber`/`FitRank` conditioning witness, and the grounded-color `DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv` chromaticity readout beside the `FitResidual`.
- Cases: capture {`MeasuredBrdf` (an angular `Seq<BrdfSample>` over incident/outgoing zenith and relative azimuth, the dielectric IOR seed, and the conductor/dielectric Fresnel discriminant — `Option<ComplexIor>` whose `Some` carries a measured complex IOR fixing `FresnelConductor` so only `(αx, αy)` are fit, whose `None` fits the dielectric `(αx, αy, η)`), `SvbrdfMap` (a per-texel `Seq<MaterialParameters>` field from a neural fit), `SpectralReflectance` (a `Spd` reflectance curve plus its metalness/roughness)} — the closed capture family; a capture is a `CaptureSource` case, never a capture subtype. A measurement instrument is a `CaptureMethod` ROW (the goniophotometer, the spectrophotometer, the neural-SVBRDF inference, the `finish#FINISH` Kubelka-Munk pigment-mix, the authored sentinel), never a per-instrument capture type.
- Entry: `public static Fin<(MaterialParameters Row, Provenance Provenance)> Import(CaptureSource source, Provenance provenance, Op key)` — the import fold pairing the fitted closed `MaterialParameters` vector with its measured `Provenance` (the `MeasuredBrdf` arm runs the realized `csharp:Rasm.Compute/blas#DENSE_ALGEBRA`-shaped overdetermined thin-`QR` Gauss-Newton solve fitting the anisotropic GGX `(αx, αy)` — plus the dielectric IOR when no conductor IOR is supplied — to the angular samples, projects the fitted alphas to the Disney `Roughness`/`Anisotropy` row columns, stamps the `goniophotometer` `CaptureMethod` and the sample count onto the receipt, and writes the witnessed `FitResidual` beside the final-Jacobian `FitConditionNumber`/`FitRank`; the `SpectralReflectance` arm grounds the base color through `surface#SPECTRAL_UPSAMPLE` and stamps the `spectrophotometer` method; the `SvbrdfMap` arm AVERAGES the per-texel field — the scene-linear base-color mean, the per-column scalar mean, and the per-band `SubsurfaceRadius` mean — into one row and stamps the `neural-svbrdf` method with the texel count), `Fin<T>` aborting on an underdetermined capture (`<measured-brdf-underdetermined>`, `m ≤ n` against the arm's unknown count), a non-finite Jacobian or diverged fit (`MaterialFault.Parameter`), an out-of-gamut grounded color (`MaterialFault.Gamut`), or an empty capture; the produced row re-admits through `MaterialParameters.Of`, the ONE re-admission site also stamping the grounded base color's chromaticity readout (`DominantWavelength`/`ExcitationPurity`/`Temperature` → `DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv`) onto the receipt for EVERY arm, and the residual-bearing provenance pairs alongside the way `finish#FINISH` `Resolve` returns its `(Row, Provenance)`.
- Packages: Wacton.Unicolour (composed — `new Unicolour(PortValue.SceneLinear, Spd)`→`Xyz`→scene-linear `Acescg` for the spectral-reflectance grounding, the `RgbLinear.Triplet` channel read for the SVBRDF base-color mean, `IsInRgbGamut` for the fit gate, and the `DominantWavelength`/`ExcitationPurity`/`Temperature` chromaticity readout stamped onto the receipt), MathNet.Numerics (composed — `Matrix<double>`/`Vector<double>` dense carriers, `Matrix<double>.Build.Dense`/`Vector<double>.Build.Dense` the design/residual build, `Matrix<double>.QR(QRMethod.Thin)` + `QR<double>.Solve` for the overdetermined GGX/Smith least-squares Gauss-Newton step, `Svd(true)` the rank-deficient fallback, `Svd(false)` the S-only conditioning handle whose `ConditionNumber`/`Rank` witness the converged Jacobian onto the receipt, `Vector<double>.L2Norm` the residual witness, `Control.UseManaged` the osx-arm64 provider — the direct AEC-domain pin, catalogued in `.api/api-mathnet-numerics.md`), Rasm (project), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new capture modality is one `CaptureSource` case carrying its fit arm — never a per-capture material or a second import owner; a new measurement instrument is one `CaptureMethod` row the receipt stamps, never a per-device receipt type; a new fit parameter is one column of the bounded parameter vector plus its `(Lo, Hi)` bounds row projected onto a column the existing `graph#MATERIAL_LIBRARY` `MaterialParameters` already carries — the solver is column-generic over `p` and the central-difference Jacobian derives per column, so widening the unknown set is data, never a solver edit; a new provenance field is one init-defaulted column on the `Provenance` record (every existing construction binds unchanged), and the generated `interchange#MATERIAL_WIRE` `WireMap.ToWire(Provenance)` runs `RequiredMappingStrategy.Both`, so the new column compels a `WireProvenance` mirror or an explicit ignore row at the wire build — a compile-forced decision, never a silent drop. The measured-BRDF fit shares the `bsdf#WHITE_FURNACE_HARNESS` energy-conservation gate so an acquired material round-trips in-gamut; the spectral grounding shares the `surface#SPECTRAL_UPSAMPLE` `Spd` construction with the `graph#MEASURED_SPECTRAL_LIBRARY` conductor rows, one upsampling owner.
- Law: `SolveGgx` is the page's one `[EXPRESSION_SPINE]` kernel exemption — the 12-iteration Gauss-Newton loop fills fixed-length `double[]` log-residual buffers and threads the mutable bounded parameter vector (`αx, αy[, η]`, each column clamped to its `(Lo, Hi)` bounds row) and `residual` scalar by index across the bounded iteration, the in-place perturb-restore central-difference `Jacobian` and the per-cell `Reflectance` forward-model evaluation riding the same carve-out — the admitted boundary-numeric-kernel exemption from the immutable-fold law for the dense least-squares solve (the same carve-out `surface#SPECTRAL_UPSAMPLE` `ToSpd` and `bsdf#LAYERED_COMPOSITION` `LayeredBsdf.Of` name); every other operation on the page — `Import`, `FitBrdf`, `AverageField`, `GroundSpectral`, the `FieldSum` fold — is expression-bodied and rail-threaded.
- Boundary: `Acquisition.Import` is the ONE import path — an acquired-material type is the deleted form; the `MeasuredBrdf` arm fits the closed parameter vector through the REALIZED `csharp:Rasm.Compute/blas#DENSE_ALGEBRA`-shaped overdetermined route (the `m` angular reflectance samples are the design-matrix rows, the anisotropic GGX `(αx, αy)` — plus the dielectric IOR on the `None`-conductor arm — the `n ∈ {2, 3}` unknowns, a thin-`QR` Gauss-Newton step `Δp = QR(Thin).Solve(−r)` over the log-residual Jacobian iterated to convergence with an `Svd(true)` truncated-pseudo-inverse conditioning fallback when the thin-`QR` step goes non-finite, the `bsdf#MICROFACET_KERNEL` GGX/Smith/Fresnel the forward model `D·G·F/(4·cosθi·cosθo)`) and never hand-rolls a Levenberg-Marquardt loop; the forward model is SINGLE-SOURCED on the kernel — `Acquisition.Reflectance` reconstructs the incident `wi = (sinθi, 0, cosθi)` and outgoing `wo = (sinθo·cosΔφ, sinθo·sinΔφ, cosθo)` `bsdf#SHADING_FRAME` `LocalVector` directions from the sample's `(θi, θo, Δφ)`, forms the true half-vector `h = (wi+wo).Normalize()`, and reads `Microfacet.Ndf(h, αx, αy)` · `Microfacet.MaskingShadowing(wo, wi, αx, αy)` · the discriminant-routed Fresnel (`Microfacet.FresnelConductor(|wo·h|, ComplexIor).Luminance` when the capture carries a measured conductor IOR, `Microfacet.FresnelDielectric(wo·h, η)` otherwise) / `(4·|cosθi|·|cosθo|)`, so the design matrix carries the genuine microfacet response at each measured geometry — a brushed-metal or anisotropic capture fits `(αx, αy)` against the conductor Fresnel and lands `Metalness = 1.0` with the fitted `Anisotropy`, never a rough dielectric with a hardcoded metalness — and the page NEVER re-mints the NDF, the Smith masking, or the Fresnel term as a private kernel (the prior hand-rolled `GgxModel`/`SmithG1`/inline-`D` with a `cos((θi−θo)/2)` half-angle that dropped the azimuth is the deleted form); the fitted alphas project to the row as `Roughness = (αx·αy)^¼` and `Anisotropy = (1 − min/max)/0.9` (the inverse Disney aspect remap); the fit is witnessed by the recomputed true relative residual `‖r‖/‖logMeasured‖` against the original samples (the one correctness signal surviving the iteration) written onto `Provenance.FitResidual` and gated through the `bsdf#WHITE_FURNACE_HARNESS`, and by the converged-Jacobian `Svd(false)` S-only conditioning readout written onto `FitConditionNumber`/`FitRank` — an in-plane capture whose constant azimuth cannot observe `αy` reads as `FitRank < n` with `+Inf` `ConditionNumber` (the `Svd` rank-deficient contract), so under-observation is receipt evidence, never a silent mis-fit — the managed provider selected once through `Control.UseManaged` (osx-arm64 has no native MKL/OpenBLAS — a per-call-site `TryUseNativeMKL` is the named defect); the spectral grounding composes the `surface#SPECTRAL_UPSAMPLE` `Spd`→scene-linear `Acescg` through the ONE `PortValue.SceneLinear` working space (the `graph#MATERIAL_GRAPH` Acescg `Configuration` instance — one instance, one Unicolour lazy-conversion cache identity) so a measured reflectance curve becomes a base color the SAME way a library row grounds, never a re-minted inline `new Configuration(RgbConfiguration.Acescg)` and never a local `SceneConfig` re-export alias; the `SvbrdfMap` arm computes a REAL field average — the base color averaged in scene-linear through `RgbLinear`, every Disney scalar column averaged, and the `SubsurfaceRadius` averaged per band — not a `texels.Head` first-row read (the prior placeholder the prose mislabelled as an average is the deleted form), so a 256×256 neural SVBRDF field collapses to the one representative row the renderer shades; the produced `MaterialParameters` re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of` so an acquired row passes the same gamut/unit/IOR gate a registered row passes, the ONE re-admission site stamping the admitted row's grounded base-color chromaticity — `DominantWavelength`/`ExcitationPurity`/`Temperature` onto `DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv`, the `photometric#PHOTOMETRIC` `EmissionInput` Wxy-projection precedent applied uniformly to all three arms (`CctDuv` carries the off-locus distance, so a reader gates `CctKelvin` on `|Duv| ≤ 0.05` before trusting the correlated temperature) — and the `Provenance` receipt carries the `CaptureMethod` instrument, the angular sample or spectral band count, the fit residual, the conditioning witness, and the chromaticity readout as the measured evidence a `MaterialParameters` authored guess lacks — the generated `interchange#MATERIAL_WIRE` `WireMap.ToWire(Provenance)` mirrors `Device`/`WavelengthCount`/`FitResidual`/`Method`/`AngularSamples` by member name and derives `Measured` structurally as `p != Provenance.Authored`, its `RequiredMappingStrategy.Both` diagnostics forcing every receipt column onto a wire mirror or an explicit ignore row; the binary capture decode (the EPFL RGL `brdf-loader` `.bsdf` format, the neural-SVBRDF `.exr` map) is the host-edge import boundary the `Rasm.Bim`/app root owns, this owner consuming the decoded `Seq<BrdfSample>`/`Seq<MaterialParameters>` portable data, never the binary file format; a malformed, empty, or out-of-gamut capture rails `MaterialFault`, never a sentinel row.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                        // Seq, Option, Fin
using Rasm.Domain;                        // Op
using Rasm.Materials.Appearance.Bsdf;    // Microfacet (GGX/Smith/Fresnel), LocalVector (the reconstructed directions), RgbSpectrum, ComplexIor, MaterialFault
using Rasm.Materials.Appearance.Surface; // SpectralUpsample (ToSpd + the SceneLinear grounding gate)
using Rasm.Materials.Appearance.Graph;   // MaterialParameters, SubsurfaceRadius, PortValue (the SceneLinear Acescg Configuration)
using MathNet.Numerics;                   // Control.UseManaged
using MathNet.Numerics.LinearAlgebra;     // Matrix<double>, Vector<double>
using MathNet.Numerics.LinearAlgebra.Factorization; // QRMethod, Svd<T> (the S-only conditioning handle)
using Wacton.Unicolour;                   // Unicolour, Spd, ColourSpace, ColourTriplet
using Thinktecture;                       // [SmartEnum]/[Union]/[KeyMember*] + ComparerAccessors.StringOrdinal (the Thinktecture ordinal-key policy every folder table pins)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;      // folder-root, beside graph#MATERIAL_LIBRARY MaterialParameters and finish#FINISH

// --- [TYPES] -------------------------------------------------------------------------------
// The measurement-instrument discriminant the Provenance receipt is keyed by: a goniophotometer angular BRDF, a
// spectrophotometer reflectance curve, a neural-SVBRDF inference, the finish#FINISH Kubelka-Munk pigment-mix, and the
// authored sentinel — so a consumer reads HOW a row was measured, not just THAT it was. A new instrument is one row,
// never a per-device receipt type; a Kubelka-Munk pigment mix is a MEASURED-pigment finish (its pigments carry measured
// K/S reflectance), so it stamps PigmentMix and reads as measured (!= Authored), never the authored sentinel.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CaptureMethod {
    public static readonly CaptureMethod Goniophotometer = new("goniophotometer");
    public static readonly CaptureMethod Spectrophotometer = new("spectrophotometer");
    public static readonly CaptureMethod NeuralSvbrdf = new("neural-svbrdf");
    public static readonly CaptureMethod PigmentMix = new("pigment-mix");   // the finish#FINISH Kubelka-Munk pigment-mix receipt — a MEASURED-pigment finish, not authored
    public static readonly CaptureMethod Authored = new("authored");
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct BrdfSample(double IncidentZenith, double OutgoingZenith, double AzimuthDelta, RgbSpectrum Reflectance);

// The measured-capture receipt this page OWNS — a sealed record, never a struct: Method is a reference-typed row and a
// struct `default` would ghost it null past every initializer. The generated interchange#MATERIAL_WIRE
// WireMap.ToWire(Provenance) mirrors Device/WavelengthCount/FitResidual/Method/AngularSamples and the six evidence columns below by member name and derives
// Measured structurally (p != Provenance.Authored); every later column is init-defaulted enrichment (every existing
// 3-arg construction binds, the authored sentinel and a Kubelka-Munk mix both defaulting Method to Authored):
// FitConditionNumber/FitRank carry the converged-Jacobian Svd conditioning (FitRank < n with +Inf condition reads
// a rank-deficient fit — e.g. an in-plane capture that cannot observe alphaY); DominantWavelengthNm/ExcitationPurity/
// CctKelvin/CctDuv carry the grounded base-color chromaticity Import stamps at the one re-admission site (CctDuv gates
// CctKelvin validity at |Duv| <= 0.05). A measured row is structurally `!= Authored`, the measured-vs-authored signal.
public sealed record Provenance(string Device, int WavelengthCount, double FitResidual) {
    public CaptureMethod Method { get; init; } = CaptureMethod.Authored;
    public int AngularSamples { get; init; }
    public double FitConditionNumber { get; init; }
    public int FitRank { get; init; }
    public double DominantWavelengthNm { get; init; }
    public double ExcitationPurity { get; init; }
    public double CctKelvin { get; init; }
    public double CctDuv { get; init; }

    public static readonly Provenance Authored = new("authored", 0, 0.0);

    // The instrument-stamped construction the acquisition arms use: a measured device name, its spectral band or angular
    // sample count, the fit residual, and the CaptureMethod instrument — one factory so no arm hand-spells the receipt.
    public static Provenance Of(CaptureMethod method, string device, int wavelengthCount, int angularSamples, double fitResidual) =>
        new(device, wavelengthCount, fitResidual) { Method = method, AngularSamples = angularSamples };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSource {
    private CaptureSource() { }

    // Conductor is the Fresnel discriminant: Some carries the measured complex IOR (FresnelConductor, alphas-only fit,
    // Metalness 1.0); None fits the dielectric (alphaX, alphaY, eta) over the Ior seed.
    public sealed record MeasuredBrdf(Seq<BrdfSample> Samples, double Ior, Option<ComplexIor> Conductor) : CaptureSource;
    public sealed record SvbrdfMap(Seq<MaterialParameters> Texels) : CaptureSource;
    public sealed record SpectralReflectance(Spd Reflectance, double Metalness, double Roughness) : CaptureSource;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Acquisition {
    static Acquisition() => Control.UseManaged();   // provider selected ONCE — osx-arm64 rides managed (native MKL/OpenBLAS are x64-only; the per-call-site TryUseNativeMKL is the named defect)

    // One import entry pairing the produced row with its measured Provenance (the finish#FINISH Resolve shape): the
    // MeasuredBrdf arm runs the overdetermined anisotropic conductor/dielectric fit and stamps the goniophotometer
    // receipt with the witnessed residual + conditioning, the SpectralReflectance arm grounds and stamps the
    // spectrophotometer method, the SvbrdfMap arm averages the per-texel field and stamps the neural texel count; every
    // row re-admits through MaterialParameters.Of, and the ONE re-admission site stamps the admitted row's grounded
    // base-color chromaticity onto the receipt uniformly (the photometric#PHOTOMETRIC EmissionInput Wxy precedent).
    public static Fin<(MaterialParameters Row, Provenance Provenance)> Import(CaptureSource source, Provenance provenance, Op key) =>
        source.Switch(
            state: (provenance, key),
            measuredBrdf:        static (s, c) => FitBrdf(c.Samples, c.Ior, c.Conductor, s.provenance, s.key),
            svbrdfMap:           static (s, c) => AverageField(c.Texels, s.provenance, s.key),
            spectralReflectance: static (s, c) => GroundSpectral(c.Reflectance, c.Metalness, c.Roughness, s.key)
                                                    .Map(row => (Row: row, Provenance: s.provenance with { Method = CaptureMethod.Spectrophotometer })))
        .Bind(pair => MaterialParameters.Of(pair.Row, key).Map(row => (row, pair.Provenance with {
            DominantWavelengthNm = row.BaseColor.DominantWavelength,
            ExcitationPurity = row.BaseColor.ExcitationPurity,
            CctKelvin = row.BaseColor.Temperature.Cct,
            CctDuv = row.BaseColor.Temperature.Duv })));

    // The overdetermined GGX/Smith/Fresnel least-squares fit: a thin-QR Gauss-Newton iteration over the angular
    // reflectance design matrix (the m measured BrdfSample geometries the rows, the bounded parameter vector the
    // n ∈ {2,3} unknowns — (alphaX, alphaY) under a supplied conductor IOR, (alphaX, alphaY, eta) for the dielectric —
    // Acquisition.Reflectance the bsdf#MICROFACET_KERNEL forward model) — the Rasm.Compute/blas#DENSE_ALGEBRA
    // dense-overdetermined route shape, never a hand-rolled Levenberg-Marquardt loop. The fitted alphas project to the
    // Disney Roughness/Anisotropy row columns; the recomputed true residual and the converged-Jacobian conditioning
    // ride the goniophotometer-stamped receipt the bsdf#WHITE_FURNACE_HARNESS gates and interchange#MATERIAL_WIRE
    // WireProvenance carries, real measured evidence vs an authored guess. The Provenance rides ALONGSIDE the row
    // (the finish#FINISH Resolve precedent), never a phantom column on the closed sixteen-positional-column MaterialParameters (its seventeenth field the init-defaulted Film).
    static Fin<(MaterialParameters Row, Provenance Provenance)> FitBrdf(Seq<BrdfSample> samples, double ior, Option<ComplexIor> conductor, Provenance provenance, Op key) =>
        guard(samples.Count > (conductor.IsSome ? 2 : 3), MaterialFault.Parameter(key, $"<measured-brdf-underdetermined:{samples.Count}>")).ToFin()
            .Bind(_ => SolveGgx(samples, ior, conductor, key))
            .Bind(fit => SpectralUpsample.ToSpd(
                    samples.Map(static s => s.Reflectance).Fold(RgbSpectrum.Black, static (acc, r) => acc.Add(r)).Scale(1.0 / samples.Count), key)
                .Bind(spd => GroundSpectral(spd, metalness: conductor.IsSome ? 1.0 : 0.0, fit.Roughness, key))
                .Map(row => (row with { Ior = fit.Eta, Anisotropy = fit.Anisotropy },
                             Provenance.Of(CaptureMethod.Goniophotometer, provenance.Device, provenance.WavelengthCount, samples.Count, fit.Residual)
                                 with { FitConditionNumber = fit.ConditionNumber, FitRank = fit.Rank })));

    // The fitted Disney projection + the witnessed relative residual + the Svd conditioning the receipt carries.
    readonly record struct BrdfFit(double Roughness, double Anisotropy, double Eta, double Residual, double ConditionNumber, int Rank);

    static Fin<BrdfFit> SolveGgx(Seq<BrdfSample> samples, double ior0, Option<ComplexIor> conductor, Op key) {
        int m = samples.Count;
        int n = conductor.IsSome ? 2 : 3;   // columns: [alphaX, alphaY(, eta)] — the conductor Fresnel is measured, not fit
        double[] p = n == 2
            ? [Microfacet.AlphaOf(0.3), Microfacet.AlphaOf(0.3)]
            : [Microfacet.AlphaOf(0.3), Microfacet.AlphaOf(0.3), Math.Clamp(ior0, 1.0, 2.5)];   // seed alpha = roughness²
        (double Lo, double Hi)[] bounds = n == 2
            ? [(1e-4, 1.0), (1e-4, 1.0)]
            : [(1e-4, 1.0), (1e-4, 1.0), (1.0, 2.5)];
        double[] logMeasured = samples.Map(static s => Math.Log(Math.Max(1e-6, s.Reflectance.Luminance))).ToArray();
        Vector<double> measured = Vector<double>.Build.DenseOfArray(logMeasured);
        double residual = double.MaxValue;
        for (int iter = 0; iter < 12; iter++) {
            Vector<double> r = Vector<double>.Build.Dense(m, i => logMeasured[i] - Math.Log(Model(samples[i], p, conductor)));
            Matrix<double> j = Matrix<double>.Build.Dense(m, n, (i, c) => Jacobian(samples[i], p, conductor, c));
            if (!j.Enumerate().All(double.IsFinite) || !r.Enumerate().All(double.IsFinite)) { return MaterialFault.Parameter(key, "<ggx-fit-non-finite-jacobian>"); }
            Vector<double> delta = j.QR(QRMethod.Thin).Solve(-r);                     // Gauss-Newton descent step Δp = -(JᵀJ)⁻¹Jᵀr; J=∂r/∂p so the RHS is -r, never +r (the ascent sign)
            // The library refuses its own gate: a near-rank-deficient Jacobian fills the QR solution with NaN while
            // IsFullRank stays true (the .api/api-mathnet-numerics.md ADMISSION law), so probe the solution all-finite
            // and fall back to the Svd(true) truncated pseudo-inverse — the ROUTE_SPINE RankRevealing conditioning fallback.
            if (!delta.Enumerate().All(double.IsFinite)) { delta = j.Svd(true).Solve(-r); }
            for (int c = 0; c < n; c++) { p[c] = Math.Clamp(p[c] + 0.5 * delta[c], bounds[c].Lo, bounds[c].Hi); }
            residual = r.L2Norm() / Math.Max(1e-6, measured.L2Norm());
            if (residual < 1e-4) { break; }
        }
        // The witnessed residual is RECOMPUTED at the CONVERGED p against the original samples (the in-loop value is
        // stale by one update step) — the true relative residual the receipt reports, never a factor-reconstructed proxy.
        residual = Vector<double>.Build.Dense(m, i => logMeasured[i] - Math.Log(Model(samples[i], p, conductor))).L2Norm() / Math.Max(1e-6, measured.L2Norm());
        // The conditioning witness at the CONVERGED p: an S-only Svd handle (no U/VT) reads ConditionNumber + Rank —
        // rank-deficiency (an unobservable column) stamps FitRank < n with +Inf condition, receipt evidence, never silent.
        Svd<double> conditioning = Matrix<double>.Build.Dense(m, n, (i, c) => Jacobian(samples[i], p, conductor, c)).Svd(false);
        return double.IsFinite(residual)
            ? Fin.Succ(new BrdfFit(
                Roughness: Math.Sqrt(Math.Sqrt(p[0] * p[1])),                                  // alpha_geo = √(αx·αy) = roughness²
                Anisotropy: Math.Clamp((1.0 - Math.Min(p[0], p[1]) / Math.Max(p[0], p[1])) / 0.9, 0.0, 1.0),   // inverse Disney aspect² = min/max
                Eta: n == 3 ? p[2] : 1.5,                                                      // conductor rows keep the coat-side Disney default — Ior is unread at Metalness 1
                Residual: residual, ConditionNumber: conditioning.ConditionNumber, Rank: conditioning.Rank))
            : MaterialFault.Parameter(key, "<ggx-fit-diverged>");
    }

    // The microfacet forward model SINGLE-SOURCED on bsdf#MICROFACET_KERNEL: reconstruct the real incident/outgoing
    // LocalVector directions from a sample's (θi, θo, Δφ), form the true half-vector, and read the kernel's anisotropic
    // GGX NDF + Smith height-correlated masking + the discriminant-routed Fresnel — the measured-conductor luminance over
    // ComplexIor, or the dielectric term over eta — NEVER a re-minted D/G/F with a cos((θi−θo)/2) specular-plane
    // half-angle that drops the azimuth (Δφ is what makes alphaX ≠ alphaY observable). alphas are GGX alphas directly
    // (roughness² per the Disney remap); the per-cell evaluation rides the SolveGgx kernel carve-out.
    static double Reflectance(BrdfSample s, double alphaX, double alphaY, double eta, Option<ComplexIor> conductor) {
        double si = Math.Sin(s.IncidentZenith), so = Math.Sin(s.OutgoingZenith);
        LocalVector wi = new LocalVector(si, 0.0, Math.Cos(s.IncidentZenith)).Normalize();
        LocalVector wo = new LocalVector(so * Math.Cos(s.AzimuthDelta), so * Math.Sin(s.AzimuthDelta), Math.Cos(s.OutgoingZenith)).Normalize();
        if (!wi.SameHemisphere(wo)) { return 1e-6; }
        LocalVector h = wi.Add(wo).Normalize();
        double d = Microfacet.Ndf(h, alphaX, alphaY);
        double g = Microfacet.MaskingShadowing(wo, wi, alphaX, alphaY);
        double f = conductor.Match(
            Some: ior => Microfacet.FresnelConductor(Math.Abs(wo.Dot(h)), ior).Luminance,
            None: () => Microfacet.FresnelDielectric(wo.Dot(h), eta));
        double denom = 4.0 * Math.Max(1e-4, Math.Abs(wi.CosTheta)) * Math.Max(1e-4, Math.Abs(wo.CosTheta));
        return Math.Max(1e-6, d * g * f / denom);
    }

    // The vector-view of the forward model the residual and Jacobian builds share; eta rides p[2] only on the dielectric arm.
    static double Model(BrdfSample s, double[] p, Option<ComplexIor> conductor) =>
        Reflectance(s, p[0], p[1], p.Length == 3 ? p[2] : 0.0, conductor);

    // Column-generic central-difference of the log-residual Jacobian over the kernel-composed forward model: perturb the
    // column in place and restore — allocation-free inside the SolveGgx kernel exemption, total over any column count.
    static double Jacobian(BrdfSample s, double[] p, Option<ComplexIor> conductor, int col) {
        const double h = 1e-4;
        double keep = p[col];
        p[col] = keep + h; double up = Math.Log(Model(s, p, conductor));
        p[col] = keep - h; double dn = Math.Log(Model(s, p, conductor));
        p[col] = keep;
        return -(up - dn) / (2.0 * h);
    }

    // The REAL per-texel SVBRDF field average — the neural fit's spatially-varying field collapsed to ONE representative
    // row in a single fold pass (never a texels.Head first-row read, never 17 LINQ traversals): FieldSum accumulates the
    // scene-linear base/emission channels and every Disney scalar + SubsurfaceRadius band, then Mean divides once. The
    // base/emission average runs in RgbLinear (the shading-truth channel) so two texels' colors blend physically, not in
    // a display-encoded space. The averaged row re-admits through MaterialParameters.Of (the Import Bind), so a degenerate
    // mean rails the row gate rather than passing a bad scatter band; the neural-svbrdf method + texel count stamp the receipt.
    static Fin<(MaterialParameters Row, Provenance Provenance)> AverageField(Seq<MaterialParameters> texels, Provenance provenance, Op key) =>
        texels.IsEmpty
            ? MaterialFault.Parameter(key, "<svbrdf-map-empty>")
            : Fin.Succ((texels.Fold(FieldSum.Zero, static (acc, t) => acc.Add(t)).Mean(texels.Count),
                        Provenance.Of(CaptureMethod.NeuralSvbrdf, provenance.Device, provenance.WavelengthCount, texels.Count, provenance.FitResidual)));

    // The single-pass field accumulator: the six scene-linear base/emission channels (read once through RgbLinear), the
    // twelve Disney scalars plus EmissionLuminance, and the 3 SubsurfaceRadius bands, summed in one fold and divided in
    // Mean — the fold algebra the immutable-accumulation law mandates over a mutable running total. Film is NOT averaged:
    // a neural field carries no interference column, so the mean row keeps the init-defaulted ThinFilm.None.
    readonly record struct FieldSum(
        double BaseR, double BaseG, double BaseB, double EmitR, double EmitG, double EmitB,
        double Metalness, double Roughness, double SpecularTint, double Anisotropy, double Ior,
        double Transmission, double TransmissionRoughness, double Sheen, double SheenTint, double Clearcoat, double ClearcoatRoughness,
        double Subsurface, double EmissionLuminance, double RadiusR, double RadiusG, double RadiusB) {
        public static readonly FieldSum Zero = default;
        public FieldSum Add(MaterialParameters t) {
            ColourTriplet b = t.BaseColor.RgbLinear.Triplet, e = t.Emission.RgbLinear.Triplet;
            return new FieldSum(BaseR + b.First, BaseG + b.Second, BaseB + b.Third, EmitR + e.First, EmitG + e.Second, EmitB + e.Third,
                Metalness + t.Metalness, Roughness + t.Roughness, SpecularTint + t.SpecularTint, Anisotropy + t.Anisotropy, Ior + t.Ior,
                Transmission + t.Transmission, TransmissionRoughness + t.TransmissionRoughness, Sheen + t.Sheen, SheenTint + t.SheenTint, Clearcoat + t.Clearcoat, ClearcoatRoughness + t.ClearcoatRoughness,
                Subsurface + t.Subsurface, EmissionLuminance + t.EmissionLuminance, RadiusR + t.SubsurfaceRadius.R, RadiusG + t.SubsurfaceRadius.G, RadiusB + t.SubsurfaceRadius.B);
        }
        public MaterialParameters Mean(int count) {
            double n = Math.Max(1, count);
            return new MaterialParameters(
                BaseColor: Linear(BaseR / n, BaseG / n, BaseB / n), Metalness: Metalness / n, Roughness: Roughness / n, SpecularTint: SpecularTint / n,
                Anisotropy: Anisotropy / n, Ior: Ior / n, Transmission: Transmission / n, TransmissionRoughness: TransmissionRoughness / n,
                Sheen: Sheen / n, SheenTint: SheenTint / n, Clearcoat: Clearcoat / n, ClearcoatRoughness: ClearcoatRoughness / n,
                Subsurface: Subsurface / n, SubsurfaceRadius: SubsurfaceRadius.Create(RadiusR / n, RadiusG / n, RadiusB / n),
                Emission: Linear(EmitR / n, EmitG / n, EmitB / n), EmissionLuminance: EmissionLuminance / n);
        }
    }

    // The spectral-reflectance grounding: a measured Spd resolved to a scene-linear Acescg base color through the ONE
    // PortValue.SceneLinear working space (a re-minted inline Configuration forks the Unicolour lazy-conversion cache).
    // The gamut gate rejects FIRST (an out-of-gamut capture faults, never a silent chroma reduction), then the SceneLinear
    // grounding is CONSUMED — the finite-gated grounded triple IS the row base color, never a discarded validation binding.
    // Named-arg construction over the closed sixteen-positional-column MaterialParameters (plus the init-defaulted Film) so a column reorder breaks loudly at compile time.
    static Fin<MaterialParameters> GroundSpectral(Spd reflectance, double metalness, double roughness, Op key) =>
        from color in Fin.Succ(new Unicolour(PortValue.SceneLinear, reflectance))
        from _ in guard(color.IsInRgbGamut, MaterialFault.Gamut(key, "<acquired-color-out-of-gamut>"))
        from grounded in SpectralUpsample.SceneLinear(color, key)
        select new MaterialParameters(
            BaseColor: Linear(grounded.R, grounded.G, grounded.B), Metalness: metalness, Roughness: roughness, SpecularTint: 0.0, Anisotropy: 0.0, Ior: 1.5,
            Transmission: 0.0, TransmissionRoughness: 0.0, Sheen: 0.0, SheenTint: 0.0, Clearcoat: 0.0, ClearcoatRoughness: 0.0,
            Subsurface: 0.0, SubsurfaceRadius: SubsurfaceRadius.None, Emission: Linear(0.0, 0.0, 0.0), EmissionLuminance: 0.0);

    // The one scene-linear Unicolour constructor the page composes — graph#MATERIAL_GRAPH PortValue.SceneLinear (the one
    // Acescg working space), so every base/emission color this page mints reads the same scene-linear channel basis the
    // library rows do, never a second ColourSpace wrapper or a re-minted Configuration.
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
}
```

## [03]-[RESEARCH]

- [EPFL_RGL_BRDF_LOADER]: the EPFL RGL measured-BRDF program publishes isotropic spectral BRDFs (195 wavelengths, the `brdf-loader` `.bsdf` binary format with a header, theta/phi parameterization, and a Rough-Quadtree spectral payload); the binary decode is an UPSTREAM host-edge concern — no managed `.bsdf` reader is vendored, so the decode lands at the `Rasm.Bim`/app-root import boundary and feeds this owner the decoded `Seq<BrdfSample>`. The `FitBrdf` arm consumes the decoded samples; until a managed `.bsdf` reader is admitted (or the C++ `brdf-loader` is bound), the binary-format read stays [UPSTREAM-BLOCKED] on the missing reader, the angular-sample import the realized internal path.
- [SVBRDF_NEURAL_FIT]: REALIZED — single-image and multi-image neural SVBRDF acquisition (the deep-material-capture shift) produces a per-texel parameter field as an `.exr`/tensor map; the `SvbrdfMap` arm computes a GENUINE single-pass field average through the `FieldSum` fold accumulator — the scene-linear base/emission channels (read once through `RgbLinear`), the twelve Disney scalars plus `EmissionLuminance`, and the three `SubsurfaceRadius` bands summed in one `Seq.Fold` and divided in `Mean` — never the `texels.Head` first-row read the prior page shipped under an "averages the field to one row" comment. The averaged row re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of` so a degenerate field-mean (a non-finite scatter band) rails the row gate. The per-texel field driving a `texture#TEXTURE_UV` `Image` source (a spatially-varying material rather than one averaged row) is the growth path — the field becomes a texture the `graph#MATERIAL_GRAPH` `Texture` node samples, riding the existing graph fold, never a second material owner. The neural inference itself is an UPSTREAM model-runtime concern outside the design.
- [BRDF_PARAMETER_FIT]: REALIZED — `SolveGgx` composes `MathNet.Numerics` at full depth: `Matrix<double>.Build.Dense`/`Vector<double>.Build.Dense` the design/residual build, `Matrix<double>.QR(QRMethod.Thin).Solve` over the log-residual Jacobian, `Svd(true)` the rank-deficient fallback, `Svd(false)` the S-only conditioning readout, `Vector<double>.L2Norm` the residual witness, `Control.UseManaged` selected once in the `Acquisition` static constructor — the `m` measured `BrdfSample` geometries the design-matrix rows, `(αx, αy[, η])` the `n ∈ {2, 3}` unknowns selected by the `Option<ComplexIor>` conductor discriminant, the step `p ← clamp(p + 0.5·Δp)` damped per-column against the bounds rows and iterated 12× to a `1e-4` residual (the fit mechanics and the kernel-composed forward model are the `[02]`-Boundary law). DELETED forms: the hand-rolled `GgxModel`/`SmithG1`/inline-`D` with a `cos((θi−θo)/2)` half-angle that dropped the azimuth, and the isotropic-dielectric-only fit whose hardcoded `Metalness: 0.0` mis-fit every measured metal to a rough dielectric. The strata blocker is RESOLVED: `MathNet.Numerics` is a DIRECT Materials NuGet pin (central in `Directory.Packages.props`), never a `Rasm.Compute` project reference — the acyclic strata forbids the AEC→app-platform edge and "MathNet transitive via Compute" is impossible (transitivity flows from Compute's references to its consumers, not the reverse); `csharp:Rasm.Compute/blas#DENSE_ALGEBRA` (`FactorRoute.Orthonormal` + `RankRevealing` fallback, `LevenbergMarquardt` the nonlinear precedent) is the DOCTRINE reference the fit shape follows, not a project edge. Realizes the `BRDF_FIT_COMPUTE_SEAM` task; the `csharp:Rasm.Compute/blas` `[DENSE_ALGEBRA]` counterpart is a doctrine citation only (no Compute edge lands).
- [CAPTURE_PROVENANCE]: REALIZED — the `Provenance` receipt this page OWNS records HOW a material was measured, not merely THAT it was. The `CaptureMethod` `[SmartEnum<string>]` instrument discriminant (goniophotometer · spectrophotometer · neural-svbrdf · pigment-mix · authored) stamps the import arm's measurement source, the `AngularSamples` column carries the goniophotometer sample count or the neural texel count, the `FitConditionNumber`/`FitRank` columns carry the converged-Jacobian `Svd(false)` conditioning witness, the `DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv` columns carry the grounded base-color chromaticity `Import` stamps at the one re-admission site (the `photometric#PHOTOMETRIC` `EmissionInput` Wxy-projection precedent — `CctDuv` gates the correlated temperature's validity at `|Duv| ≤ 0.05`), and the generated `interchange#MATERIAL_WIRE` `WireMap.ToWire(Provenance)` mirrors `Device`/`WavelengthCount`/`FitResidual`/`Method` (`CaptureMethod.Key`)/`AngularSamples` and the six evidence columns by member name and derives `Measured` from (`p != Provenance.Authored`). Every enrichment column is init-defaulted so the `Authored` sentinel and every 3-arg construction bind unchanged, and the `finish#FINISH` `MixProvenance` stamps `PigmentMix` through the frozen 5-arg `Provenance.Of` — a measured-capture instrument is a `CaptureMethod` row, never a per-device receipt type. RESOLVED wire counterpart: the six conditioning/chromaticity columns (`FitConditionNumber`/`FitRank`/`DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv`) now mirror as `WireProvenance` `[Key(6..11)]` columns at `interchange#MATERIAL_WIRE`, auto-mapped by NAME under the mapper's `RequiredMappingStrategy.Both` — `FitConditionNumber`'s `+Inf` (a rank-deficient fit) crosses the JSON leg as the named literal and the MessagePack leg as the native double, decoded by the TS/Py peers, so the measured quality survives to every consumer, never a silent drop.

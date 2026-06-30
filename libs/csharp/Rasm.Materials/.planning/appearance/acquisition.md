# [MATERIALS_ACQUISITION]

THE MEASURED-MATERIAL IMPORT PATH. One `Acquisition` static fold over the closed `CaptureSource` `[Union]` (measured-brdf · svbrdf-map · spectral-reflectance) lands an acquired real-world material as a `graph#MATERIAL_LIBRARY` `MaterialParameters` row carrying a measured `Provenance` receipt, so the appearance engine shades real captured materials rather than authored approximations. An acquired material is NEVER a second material owner: `Acquisition.Import` takes the capture data and returns a `MaterialParameters` row the SAME `graph#MATERIAL_LIBRARY` registers and the SAME `bsdf#LOBE_FAMILY` shades — a goniophotometer BRDF, a neural SVBRDF field, and a spectrophotometer reflectance curve all produce one `MaterialParameters` row, never a `MeasuredMaterial`/`AcquiredBrdf` type. The import is a data-import concern distinct from the measured-spectral grounding of existing rows: it admits the capture, fits the closed parameter vector by the `csharp:Rasm.Compute/blas#DENSE_ALGEBRA` dense-route doctrine shape, grounds the base color through the `surface#SPECTRAL_UPSAMPLE` `Spd` construction, and gates the round-trip in-gamut through the white-furnace harness. The fit's forward model is NOT a re-minted lobe — it COMPOSES the `bsdf#MICROFACET_KERNEL` `Microfacet.Ndf`/`MaskingShadowing`/`FresnelDielectric` over the REAL `bsdf#SHADING_FRAME` `LocalVector` incident/outgoing directions reconstructed from each `BrdfSample` `(θi, θo, Δφ)`, so the GGX/Smith/Fresnel math is single-sourced on the kernel and the design matrix carries the true microfacet half-vector, never a `cos((θi−θo)/2)` specular-plane proxy that ignores the azimuth. The page composes `surface#SPECTRAL_UPSAMPLE` for the `Spd`→scene-linear color, `graph#MATERIAL_LIBRARY` `MaterialParameters` for the produced row, `bsdf#MICROFACET_KERNEL`/`bsdf#SHADING_FRAME` for the single-sourced forward model, Wacton.Unicolour directly for the spectral→XYZ→Acescg conversion, and the `MaterialFault` band-2450 rail for a malformed or out-of-gamut capture. The `Provenance` receipt this page OWNS — its `CaptureMethod` discriminant, angular/spectral resolution, and `FitResidual` — is the measured evidence the `interchange#MATERIAL_WIRE` `WireProvenance` carries and the `finish#FINISH` Kubelka-Munk pigment-mix receipt reuses, so every measured-vs-authored distinction reads one receipt shape.

## [01]-[INDEX]

- [01]-[ACQUISITION]: the `CaptureSource` `[Union]` capture family, the `BrdfSample` angular-reflectance record, the `Provenance` measured-capture receipt over the `CaptureMethod` `[SmartEnum]` instrument discriminant, the `Microfacet`-composed GGX/Smith forward model over reconstructed `LocalVector` directions, the thin-`QR` Gauss-Newton `Acquisition.Import` fit-and-ground fold producing a `MaterialParameters` row, and the per-texel SVBRDF field average.

## [02]-[ACQUISITION]

- Owner: `Acquisition` static import fold; `CaptureSource` `[Union]` (measured-brdf · svbrdf-map · spectral-reflectance); `BrdfSample` the goniophotometer angular-reflectance record over `(θi, θo, Δφ)`; `CaptureMethod` `[SmartEnum<string>]` the measurement-instrument discriminant (goniophotometer · spectrophotometer · neural-svbrdf · authored); `Provenance` the measured-capture receipt this page owns, keyed by `CaptureMethod` and carrying the angular/spectral resolution beside the `FitResidual`.
- Cases: capture {`MeasuredBrdf` (an angular `Seq<BrdfSample>` over incident/outgoing zenith and relative azimuth, the dielectric IOR seed, and the `CaptureMethod` instrument), `SvbrdfMap` (a per-texel `Seq<MaterialParameters>` field from a neural fit), `SpectralReflectance` (a `Spd` reflectance curve plus its metalness/roughness)} — the closed capture family; a capture is a `CaptureSource` case, never a capture subtype. A measurement instrument is a `CaptureMethod` ROW (the goniophotometer, the spectrophotometer, the neural-SVBRDF inference, the `finish#FINISH` Kubelka-Munk pigment-mix, the authored sentinel), never a per-instrument capture type.
- Entry: `public static Fin<(MaterialParameters Row, Provenance Provenance)> Import(CaptureSource source, Provenance provenance, Op key)` — the import fold pairing the fitted closed `MaterialParameters` vector with its measured `Provenance` (the `MeasuredBrdf` arm runs the realized `csharp:Rasm.Compute/blas#DENSE_ALGEBRA`-shaped overdetermined thin-`QR` Gauss-Newton solve fitting the GGX roughness + dielectric IOR to the angular samples, stamps the `goniophotometer` `CaptureMethod` and the sample count onto the receipt, and writes the witnessed `FitResidual`; the `SpectralReflectance` arm grounds the base color through `surface#SPECTRAL_UPSAMPLE` and stamps the `spectrophotometer` method with the `Spd` band count; the `SvbrdfMap` arm AVERAGES the per-texel field — the scene-linear base-color mean, the per-column scalar mean, and the per-band `SubsurfaceRadius` mean — into one row and stamps the `neural-svbrdf` method with the texel count), `Fin<T>` aborting on an underdetermined capture (`<measured-brdf-underdetermined>`, fewer than 3 samples), a non-finite Jacobian or diverged fit (`MaterialFault.Parameter`), an out-of-gamut grounded color (`MaterialFault.Gamut`), or an empty capture; the produced row re-admits through `MaterialParameters.Of` and the residual-bearing provenance pairs alongside the way `finish#FINISH` `Resolve` returns its `(Row, Provenance)`.
- Packages: Wacton.Unicolour (composed — `new Unicolour(SpectralUpsample.SceneConfig, Spd)`→`Xyz`→scene-linear `Acescg` for the spectral-reflectance grounding, the `RgbLinear.Triplet` channel read for the SVBRDF base-color mean, and `IsInRgbGamut` for the fit gate), MathNet.Numerics (composed — `Matrix<double>`/`Vector<double>` dense carriers, `Matrix<double>.Build.Dense`/`Vector<double>.Build.Dense` the design/residual build, `Matrix<double>.QR(QRMethod.Thin)` + `QR<double>.Solve` for the overdetermined GGX/Smith least-squares Gauss-Newton step, `Svd(true)` the rank-deficient fallback, `Vector<double>.L2Norm` the residual witness, `Control.UseManaged` the osx-arm64 provider — the direct AEC-domain pin, catalogued in `.api/api-mathnet-numerics.md`), Rasm (project), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new capture modality is one `CaptureSource` case carrying its fit arm — never a per-capture material or a second import owner; a new measurement instrument is one `CaptureMethod` row the receipt stamps, never a per-device receipt type; a new fit parameter is one column the existing `graph#MATERIAL_LIBRARY` `MaterialParameters` already carries, the fit targeting the closed Disney/OpenPBR vector; a new provenance field is one defaulted column on the `Provenance` record (existing `finish#FINISH` and `interchange#MATERIAL_WIRE` construction sites bind positionally on the leading three, the new column riding the wire when `interchange` adds its `WireProvenance` mirror). The measured-BRDF fit shares the `bsdf#WHITE_FURNACE_HARNESS` energy-conservation gate so an acquired material round-trips in-gamut; the spectral grounding shares the `surface#SPECTRAL_UPSAMPLE` `Spd` construction with the `graph#MEASURED_SPECTRAL_LIBRARY` conductor rows, one upsampling owner.
- Law: `SolveGgx` is the page's one `[EXPRESSION_SPINE]` kernel exemption — the 12-iteration Gauss-Newton loop fills fixed-length `double[]` log-residual buffers and threads the mutable `(α, η)` parameter pair and `residual` scalar by index across the bounded iteration, the admitted boundary-numeric-kernel carve-out from the immutable-fold law for the dense least-squares solve (the same carve-out `surface#SPECTRAL_UPSAMPLE` `ToSpd` and `bsdf#LAYERED_COMPOSITION` `LayeredBsdf.Of` name); every other operation on the page — `Import`, `FitBrdf`, `AverageField`, `GroundSpectral`, the `Microfacet`-composed forward model — is expression-bodied and rail-threaded.
- Boundary: `Acquisition.Import` is the ONE import path — an acquired-material type is the deleted form; the `MeasuredBrdf` arm fits the closed parameter vector through the REALIZED `csharp:Rasm.Compute/blas#DENSE_ALGEBRA`-shaped overdetermined route (the `m` angular reflectance samples are the design-matrix rows, the GGX roughness + dielectric IOR the `n=2` unknowns, a thin-`QR` Gauss-Newton step `Δp = QR(Thin).Solve(−r)` over the log-residual Jacobian iterated to convergence with an `Svd(true)` truncated-pseudo-inverse conditioning fallback when the thin-`QR` step goes non-finite, the `bsdf#MICROFACET_KERNEL` GGX/Smith/Fresnel the forward model `D·G·F/(4·cosθi·cosθo)`) and never hand-rolls a Levenberg-Marquardt loop; the forward model is SINGLE-SOURCED on the kernel — `BrdfModel.Reflectance` reconstructs the incident `wi = (sinθi, 0, cosθi)` and outgoing `wo = (sinθo·cosΔφ, sinθo·sinΔφ, cosθo)` `bsdf#SHADING_FRAME` `LocalVector` directions from the sample's `(θi, θo, Δφ)`, forms the true half-vector `h = (wi+wo).Normalize()`, and reads `Microfacet.Ndf(h, α, α)` · `Microfacet.MaskingShadowing(wo, wi, α, α)` · `Microfacet.FresnelDielectric(wo.Dot(h), η)` / `(4·|cosθi|·|cosθo|)`, so the design matrix carries the genuine microfacet response at each measured geometry and the page NEVER re-mints the NDF, the Smith masking, or the Fresnel term as a private kernel (the prior hand-rolled `GgxModel`/`SmithG1`/inline-`D` with a `cos((θi−θo)/2)` half-angle that dropped the azimuth is the deleted form); the fit is witnessed by the recomputed true relative residual `‖r‖/‖logMeasured‖` against the original samples (the one correctness signal surviving the iteration) written onto `Provenance.FitResidual` and gated through the `bsdf#WHITE_FURNACE_HARNESS`, the managed provider selected once through `Control.UseManaged` (osx-arm64 has no native MKL/OpenBLAS — a per-call-site `TryUseNativeMKL` is the named defect); the spectral grounding composes the `surface#SPECTRAL_UPSAMPLE` `Spd`→scene-linear `Acescg` through the SHARED `SpectralUpsample.SceneConfig` working space so a measured reflectance curve becomes a base color the SAME way a library row grounds, never a re-minted inline `new Configuration(RgbConfiguration.Acescg)`; the `SvbrdfMap` arm computes a REAL field average — the base color averaged in scene-linear through `RgbLinear`, every Disney scalar column averaged, and the `SubsurfaceRadius` averaged per band — not a `texels.Head` first-row read (the prior placeholder the prose mislabelled as an average is the deleted form), so a 256×256 neural SVBRDF field collapses to the one representative row the renderer shades; the produced `MaterialParameters` re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of` so an acquired row passes the same gamut/unit/IOR gate a registered row passes, and the `Provenance` receipt carries the `CaptureMethod` instrument, the angular sample or spectral band count, and the fit residual as the measured evidence a `MaterialParameters` authored guess lacks — `interchange#MATERIAL_WIRE` `WireProvenance.Of` reads the leading `Device`/`WavelengthCount`/`FitResidual` and derives `Measured` as `p != Provenance.Authored`, so the receipt growth never breaks the wire mirror; the binary capture decode (the EPFL RGL `brdf-loader` `.bsdf` format, the neural-SVBRDF `.exr` map) is the host-edge import boundary the `Rasm.Bim`/app root owns, this owner consuming the decoded `Seq<BrdfSample>`/`Seq<MaterialParameters>` portable data, never the binary file format; a malformed, empty, or out-of-gamut capture rails `MaterialFault`, never a sentinel row.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using LanguageExt;                        // Seq, Option, Fin
using Rasm.Domain;                        // Op
using Rasm.Materials.Appearance.Bsdf;    // Microfacet (GGX/Smith/Fresnel), LocalVector (the reconstructed directions), RgbSpectrum, MaterialFault
using Rasm.Materials.Appearance.Surface; // SpectralUpsample (Spd grounding + the shared SceneConfig working space)
using Rasm.Materials.Appearance.Graph;   // MaterialParameters, SubsurfaceRadius, PortValue (the SceneLinear Acescg Configuration)
using MathNet.Numerics;                   // Control.UseManaged
using MathNet.Numerics.LinearAlgebra;     // Matrix<double>, Vector<double>
using MathNet.Numerics.LinearAlgebra.Factorization; // QRMethod
using Wacton.Unicolour;                   // Unicolour, Spd, ColourSpace, ColourTriplet
using Thinktecture;                       // ComparerAccessors (the CaptureMethod key comparer, owned by bsdf#SHADING_FRAME)
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

// The measured-capture receipt this page OWNS. The leading Device/WavelengthCount/FitResidual triple is the wire-stable
// shape interchange#MATERIAL_WIRE WireProvenance.Of(p) reads positionally and finish#FINISH MixProvenance constructs;
// the Method/AngularSamples columns are init-defaulted enrichment (every existing 3-arg construction binds, the
// authored sentinel and a Kubelka-Munk mix both defaulting Method to Authored) carrying the instrument and the angular
// sample count beside the residual. A measured row is structurally `!= Authored`, the measured-vs-authored signal.
public readonly record struct Provenance(string Device, int WavelengthCount, double FitResidual) {
    public CaptureMethod Method { get; init; } = CaptureMethod.Authored;
    public int AngularSamples { get; init; }

    public static readonly Provenance Authored = new("authored", 0, 0.0);

    // The instrument-stamped construction the acquisition arms use: a measured device name, its spectral band or angular
    // sample count, the fit residual, and the CaptureMethod instrument — one factory so no arm hand-spells the receipt.
    public static Provenance Of(CaptureMethod method, string device, int wavelengthCount, int angularSamples, double fitResidual) =>
        new(device, wavelengthCount, fitResidual) { Method = method, AngularSamples = angularSamples };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSource {
    private CaptureSource() { }

    public sealed record MeasuredBrdf(Seq<BrdfSample> Samples, double Ior) : CaptureSource;
    public sealed record SvbrdfMap(Seq<MaterialParameters> Texels) : CaptureSource;
    public sealed record SpectralReflectance(Spd Reflectance, double Metalness, double Roughness) : CaptureSource;

    public CaptureMethod Method => Switch<CaptureMethod>(
        measuredBrdf:        static _ => CaptureMethod.Goniophotometer,
        svbrdfMap:           static _ => CaptureMethod.NeuralSvbrdf,
        spectralReflectance: static _ => CaptureMethod.Spectrophotometer);
}

// --- [OPERATIONS] --------------------------------------------------------------------------
// The microfacet forward model SINGLE-SOURCED on bsdf#MICROFACET_KERNEL: reconstruct the real incident/outgoing
// LocalVector directions from a sample's (θi, θo, Δφ), form the true half-vector, and read the kernel's GGX NDF +
// Smith height-correlated masking + dielectric Fresnel — NEVER a re-minted D/G/F with a cos((θi−θo)/2) specular-plane
// half-angle that drops the azimuth. alpha is the GGX alpha directly (roughness² per the Disney remap), eta the
// dielectric IOR. The page fits this model; the lobe math lives on the kernel and is composed, not re-derived.
public static class BrdfModel {
    public static double Reflectance(BrdfSample s, double alpha, double eta) {
        double si = Math.Sin(s.IncidentZenith), so = Math.Sin(s.OutgoingZenith);
        LocalVector wi = new LocalVector(si, 0.0, Math.Cos(s.IncidentZenith)).Normalize();
        LocalVector wo = new LocalVector(so * Math.Cos(s.AzimuthDelta), so * Math.Sin(s.AzimuthDelta), Math.Cos(s.OutgoingZenith)).Normalize();
        if (!wi.SameHemisphere(wo)) { return 1e-6; }
        LocalVector h = wi.Add(wo).Normalize();
        double d = Microfacet.Ndf(h, alpha, alpha);
        double g = Microfacet.MaskingShadowing(wo, wi, alpha, alpha);
        double f = Microfacet.FresnelDielectric(wo.Dot(h), eta);
        double denom = 4.0 * Math.Max(1e-4, Math.Abs(wi.CosTheta)) * Math.Max(1e-4, Math.Abs(wo.CosTheta));
        return Math.Max(1e-6, d * g * f / denom);
    }
}

public static class Acquisition {
    // One import entry pairing the produced row with its measured Provenance (the finish#FINISH Resolve shape): the
    // MeasuredBrdf arm runs the overdetermined GGX/Smith fit and stamps the goniophotometer receipt with the witnessed
    // residual, the SpectralReflectance arm grounds and stamps the spectrophotometer band count, the SvbrdfMap arm
    // averages the per-texel field and stamps the neural texel count; every row re-admits through MaterialParameters.Of.
    public static Fin<(MaterialParameters Row, Provenance Provenance)> Import(CaptureSource source, Provenance provenance, Op key) =>
        source.Switch(
            state: (provenance, key),
            measuredBrdf:        static (s, c) => FitBrdf(c.Samples, c.Ior, s.provenance, s.key),
            svbrdfMap:           static (s, c) => AverageField(c.Texels, s.provenance, s.key),
            spectralReflectance: static (s, c) => GroundSpectral(c.Reflectance, c.Metalness, c.Roughness, s.key)
                                                    .Map(row => (row, s.provenance with { Method = CaptureMethod.Spectrophotometer })))
        .Bind(pair => MaterialParameters.Of(pair.Row, key).Map(row => (row, pair.Provenance)));

    // The overdetermined GGX/Smith/Fresnel least-squares fit: a thin-QR Gauss-Newton iteration over the angular
    // reflectance design matrix (the m measured BrdfSample geometries the rows, the (alpha, eta) the n=2 unknowns,
    // BrdfModel.Reflectance the bsdf#MICROFACET_KERNEL forward model) — the Rasm.Compute/blas#DENSE_ALGEBRA
    // dense-overdetermined route shape, never a hand-rolled Levenberg-Marquardt loop. The fitted parameters seed the
    // row, the recomputed true residual rides the goniophotometer-stamped Provenance.FitResidual receipt the
    // bsdf#WHITE_FURNACE_HARNESS gates and interchange#MATERIAL_WIRE WireProvenance carries, real measured evidence
    // vs an authored guess. The Provenance rides ALONGSIDE the row (the finish#FINISH Resolve precedent), never a
    // phantom column on the closed 16-column MaterialParameters.
    static Fin<(MaterialParameters Row, Provenance Provenance)> FitBrdf(Seq<BrdfSample> samples, double ior, Provenance provenance, Op key) =>
        guard(samples.Count >= 3, MaterialFault.Parameter(key, $"<measured-brdf-underdetermined:{samples.Count}<3>")).ToFin()
            .Bind(_ => SolveGgx(samples, ior, key))
            .Bind(fit => samples.Map(static s => s.Reflectance).Fold(RgbSpectrum.Black, static (acc, r) => acc.Add(r)).Scale(1.0 / samples.Count) is var meanAlbedo
                ? SpectralUpsample.ToSpd(meanAlbedo, key)
                    .Bind(spd => GroundSpectral(spd, Metalness: 0.0, fit.Roughness, key))
                    .Map(row => (row with { Ior = fit.Ior },
                                 Provenance.Of(CaptureMethod.Goniophotometer, provenance.Device, provenance.WavelengthCount, samples.Count, fit.Residual)))
                : MaterialFault.Parameter(key, "<measured-brdf-mean-degenerate>"));

    // The fitted GGX alpha + dielectric IOR + the witnessed relative residual the Provenance receipt carries.
    readonly record struct BrdfFit(double Roughness, double Ior, double Residual);

    static Fin<BrdfFit> SolveGgx(Seq<BrdfSample> samples, double ior0, Op key) {
        Control.UseManaged();   // selected once; osx-arm64 rides the managed provider (native MKL/OpenBLAS are x64-only)
        int m = samples.Count;
        double[] logMeasured = samples.Map(static s => Math.Log(Math.Max(1e-6, s.Reflectance.Luminance))).ToArray();
        Vector<double> measured = Vector<double>.Build.DenseOfArray(logMeasured);
        (double alpha, double eta) p = (Microfacet.AlphaOf(0.3), Math.Clamp(ior0, 1.0, 2.5));   // seed alpha = roughness²
        double residual = double.MaxValue;
        for (int iter = 0; iter < 12; iter++) {
            Vector<double> r = Vector<double>.Build.Dense(m, i => logMeasured[i] - Math.Log(BrdfModel.Reflectance(samples[i], p.alpha, p.eta)));
            Matrix<double> j = Matrix<double>.Build.Dense(m, 2, (i, c) => Jacobian(samples[i], p, c));
            if (!j.Enumerate().All(double.IsFinite) || !r.Enumerate().All(double.IsFinite)) { return MaterialFault.Parameter(key, "<ggx-fit-non-finite-jacobian>"); }
            Vector<double> delta = j.QR(QRMethod.Thin).Solve(-r);                     // Gauss-Newton descent step Δp = -(JᵀJ)⁻¹Jᵀr; J=∂r/∂p so the RHS is -r, never +r (the ascent sign)
            // The library refuses its own gate: a near-rank-deficient Jacobian fills the QR solution with NaN while
            // IsFullRank stays true (the .api/api-mathnet-numerics.md ADMISSION law), so probe the solution all-finite
            // and fall back to the Svd(true) truncated pseudo-inverse — the ROUTE_SPINE RankRevealing conditioning fallback.
            if (!delta.Enumerate().All(double.IsFinite)) { delta = j.Svd(true).Solve(-r); }
            p = (Math.Clamp(p.alpha + 0.5 * delta[0], 1e-4, 1.0), Math.Clamp(p.eta + 0.5 * delta[1], 1.0, 2.5));
            residual = r.L2Norm() / Math.Max(1e-6, measured.L2Norm());
            if (residual < 1e-4) { break; }
        }
        return double.IsFinite(residual) ? Fin.Succ(new BrdfFit(Math.Sqrt(p.alpha), p.eta, residual)) : MaterialFault.Parameter(key, "<ggx-fit-diverged>");
    }

    // Central-difference column of the log-residual Jacobian w.r.t. (alpha, eta) over the kernel-composed forward model.
    static double Jacobian(BrdfSample s, (double alpha, double eta) p, int col) {
        const double h = 1e-4;
        (double a, double e) plus = col == 0 ? (p.alpha + h, p.eta) : (p.alpha, p.eta + h);
        (double a, double e) minus = col == 0 ? (p.alpha - h, p.eta) : (p.alpha, p.eta - h);
        return -(Math.Log(BrdfModel.Reflectance(s, plus.a, plus.e)) - Math.Log(BrdfModel.Reflectance(s, minus.a, minus.e))) / (2.0 * h);
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

    // The single-pass field accumulator: scene-linear color channels (read once through RgbLinear) plus the 12 Disney
    // scalars plus the 3 SubsurfaceRadius bands, summed in one fold and divided in Mean — the fold algebra the immutable
    // accumulation law mandates over a 17-field-per-texel mutable running total.
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

    // The spectral-reflectance grounding: a measured Spd resolved to a scene-linear Acescg base color through the SHARED
    // SpectralUpsample.SceneConfig working space (never a re-minted inline new Configuration(RgbConfiguration.Acescg)),
    // the gamut gate the same one a library row passes. Named-arg construction over the closed 16-column MaterialParameters
    // so a column reorder breaks loudly at compile time rather than silently mis-seeding a positional field.
    static Fin<MaterialParameters> GroundSpectral(Spd reflectance, double metalness, double roughness, Op key) {
        Unicolour color = new(SpectralUpsample.SceneConfig, reflectance);
        return SpectralUpsample.SceneLinear(color, key)
            .Bind(_ => color.IsInRgbGamut
                ? Fin.Succ(new MaterialParameters(
                    BaseColor: color, Metalness: metalness, Roughness: roughness, SpecularTint: 0.0, Anisotropy: 0.0, Ior: 1.5,
                    Transmission: 0.0, TransmissionRoughness: 0.0, Sheen: 0.0, SheenTint: 0.0, Clearcoat: 0.0, ClearcoatRoughness: 0.0,
                    Subsurface: 0.0, SubsurfaceRadius: SubsurfaceRadius.None, Emission: Linear(0.0, 0.0, 0.0), EmissionLuminance: 0.0))
                : MaterialFault.Gamut(key, "<acquired-color-out-of-gamut>"));
    }

    // The one scene-linear Unicolour constructor the page composes — graph#MATERIAL_GRAPH PortValue.SceneLinear (the one
    // Acescg working space), so every base/emission color this page mints reads the same scene-linear channel basis the
    // library rows do, never a second ColourSpace wrapper or a re-minted Configuration.
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
}
```

## [03]-[RESEARCH]

- [EPFL_RGL_BRDF_LOADER]: the EPFL RGL measured-BRDF program publishes isotropic spectral BRDFs (195 wavelengths, the `brdf-loader` `.bsdf` binary format with a header, theta/phi parameterization, and a Rough-Quadtree spectral payload); the binary decode is an UPSTREAM host-edge concern — no managed `.bsdf` reader is vendored, so the decode lands at the `Rasm.Bim`/app-root import boundary and feeds this owner the decoded `Seq<BrdfSample>`. The `FitBrdf` arm consumes the decoded samples; until a managed `.bsdf` reader is admitted (or the C++ `brdf-loader` is bound), the binary-format read stays [UPSTREAM-BLOCKED] on the missing reader, the angular-sample import the realized internal path.
- [SVBRDF_NEURAL_FIT]: REALIZED — single-image and multi-image neural SVBRDF acquisition (the deep-material-capture shift) produces a per-texel parameter field as an `.exr`/tensor map; the `SvbrdfMap` arm computes a GENUINE single-pass field average through the `FieldSum` fold accumulator — the scene-linear base/emission channels (read once through `RgbLinear`), the twelve Disney scalars, and the three `SubsurfaceRadius` bands summed in one `Seq.Fold` and divided in `Mean` — never the `texels.Head` first-row read the prior page shipped under an "averages the field to one row" comment. The averaged row re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of` so a degenerate field-mean (a non-finite scatter band) rails the row gate. The per-texel field driving a `texture#TEXTURE_UV` `Image` source (a spatially-varying material rather than one averaged row) is the growth path — the field becomes a texture the `graph#MATERIAL_GRAPH` `Texture` node samples, riding the existing graph fold, never a second material owner. The neural inference itself is an UPSTREAM model-runtime concern outside the design.
- [BRDF_PARAMETER_FIT]: REALIZED — the `FitBrdf` arm runs the overdetermined GGX/Smith least-squares fit through `SolveGgx`, a thin-`QR` Gauss-Newton iteration composing `MathNet.Numerics` (`Matrix<double>.Build.Dense`/`Vector<double>.Build.Dense` the design/residual build, `Matrix<double>.QR(QRMethod.Thin).Solve` over the log-residual Jacobian, `Svd(true)` the rank-deficient conditioning fallback, `Vector<double>.L2Norm` the residual witness, `Control.UseManaged` the osx-arm64 provider) — the `m` measured `BrdfSample` geometries the design-matrix rows, the GGX alpha + dielectric IOR the `n=2` unknowns, `BrdfModel.Reflectance` the forward model, the step `p ← clamp(p + 0.5·Δp)` damped and iterated 12× to a `1e-4` residual. The forward model is SINGLE-SOURCED on `bsdf#MICROFACET_KERNEL` — `BrdfModel.Reflectance` reconstructs the incident/outgoing `bsdf#SHADING_FRAME` `LocalVector` directions from each sample's `(θi, θo, Δφ)`, forms the true half-vector, and reads `Microfacet.Ndf` · `Microfacet.MaskingShadowing` · `Microfacet.FresnelDielectric` / `(4·|cosθi|·|cosθo|)`, so the design matrix carries the genuine microfacet response at the measured geometry; the prior hand-rolled `GgxModel`/`SmithG1`/inline-`D` with a `cos((θi−θo)/2)` half-angle that dropped the azimuth — a re-minted lobe the page's own boundary forbade — is DELETED. The recomputed true relative residual `‖r‖/‖logMeasured‖` is the witnessed evidence written onto `Provenance.FitResidual` the `bsdf#WHITE_FURNACE_HARNESS` gates and the `interchange#MATERIAL_WIRE` `WireProvenance` carries, so a fitted material distinguishes from an authored guess on the wire. The strata blocker is RESOLVED: `MathNet.Numerics` is a DIRECT Materials NuGet pin (the AEC-domain folder's own dependency, the version already central in `Directory.Packages.props`), never a `Rasm.Compute` project reference — the acyclic strata forbids the AEC→app-platform edge and "MathNet transitive via Compute" is impossible (transitivity flows from Compute's references to its consumers, not the reverse). The `csharp:Rasm.Compute/blas#DENSE_ALGEBRA` thin-`QR` is the DOCTRINE reference the fit shape follows (the `FactorRoute.Orthonormal` overdetermined route with the `RankRevealing` `Svd(true)` conditioning fallback, the `LevenbergMarquardt` damped-Gauss-Newton owner the nonlinear precedent), not a project edge. Realizes the `BRDF_FIT_COMPUTE_SEAM` task; ripple counterpart `csharp:Rasm.Compute/blas` `[DENSE_ALGEBRA]` is a DOCTRINE citation only, not a realized seam (no Compute edge lands).
- [CAPTURE_PROVENANCE]: REALIZED — the `Provenance` receipt this page OWNS records HOW a material was measured, not merely THAT it was. The `CaptureMethod` `[SmartEnum<string>]` instrument discriminant (goniophotometer · spectrophotometer · neural-svbrdf · pigment-mix · authored) stamps the import arm's measurement source, the `AngularSamples` column carries the goniophotometer sample count or the neural texel count, and the leading `Device`/`WavelengthCount`/`FitResidual` triple stays the wire-stable shape `interchange#MATERIAL_WIRE` `WireProvenance.Of(p)` reads positionally and derives `Measured` from (`p != Provenance.Authored`). The `CaptureMethod`/`AngularSamples` columns are init-defaulted so the `Authored` sentinel binds unchanged, and the `finish#FINISH` `MixProvenance` stamps `PigmentMix` through `Provenance.Of` — a measured-capture instrument is a `CaptureMethod` row, never a per-device receipt type. The cross-file wire carry is REALIZED: `interchange#MATERIAL_WIRE` `WireProvenance` carries the `Method` (`CaptureMethod.Key`) and `AngularSamples` mirror columns (defaulted on its TS/Py decode rows) reading `p.Method.Key`/`p.AngularSamples` so the instrument crosses the wire to the host-free peers.

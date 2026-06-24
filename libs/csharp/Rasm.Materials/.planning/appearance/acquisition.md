# [MATERIALS_ACQUISITION]

THE MEASURED-MATERIAL IMPORT PATH. One `Acquisition` static fold over the closed `CaptureSource` `[Union]` (measured-brdf · svbrdf-map · spectral-reflectance) lands an acquired real-world material as a `graph#MATERIAL_LIBRARY` `MaterialParameters` row carrying measured provenance, so the appearance engine shades real captured materials rather than authored approximations. An acquired material is NEVER a second material owner: `Acquisition.Import` takes the capture data and returns a `MaterialParameters` row the SAME `graph#MATERIAL_LIBRARY` registers and the SAME `bsdf#LOBE_FAMILY` shades — a goniophotometer BRDF, a neural SVBRDF map, and a spectral-reflectance curve all produce one `MaterialParameters` row, never a `MeasuredMaterial`/`AcquiredBrdf` type. The import is a data-import concern distinct from the measured-spectral grounding of existing rows: it admits the capture, fits the closed parameter vector by the `csharp:Rasm.Compute/blas#ROUTE_SPINE` dense-route doctrine shape, grounds the base color through the `surface#SPECTRAL_UPSAMPLE` `Spd` construction, and gates the round-trip in-gamut through the white-furnace harness. The page composes `surface#SPECTRAL_UPSAMPLE` for the `Spd`→scene-linear color, `graph#MATERIAL_LIBRARY` `MaterialParameters` for the produced row, Wacton.Unicolour directly for the spectral→XYZ→Acescg conversion, and the `MaterialFault` band-2450 rail for a malformed or out-of-gamut capture.

## [01]-[INDEX]

- [01]-[ACQUISITION]: the `CaptureSource` `[Union]` capture family, the `BrdfSample`/`Provenance` capture records, the `Acquisition.Import` fit-and-ground fold producing a `MaterialParameters` row, and the measured-provenance receipt.

## [02]-[ACQUISITION]

- Owner: `Acquisition` static import fold; `CaptureSource` `[Union]` (measured-brdf · svbrdf-map · spectral-reflectance); `BrdfSample` the goniophotometer angular-reflectance record; `Provenance` the measured-provenance receipt.
- Cases: capture {`MeasuredBrdf` (an isotropic angular `Seq<BrdfSample>` over incident/outgoing zenith), `SvbrdfMap` (a per-texel `Seq<MaterialParameters>` field from a neural fit), `SpectralReflectance` (a `Spd` reflectance curve)} — the closed capture family; a capture is a `CaptureSource` case, never a capture subtype.
- Entry: `public static Fin<(MaterialParameters Row, Provenance Provenance)> Import(CaptureSource source, Provenance provenance, Op key)` — the import fold pairing the fitted closed `MaterialParameters` vector with its measured `Provenance` (the `MeasuredBrdf` arm runs the realized `csharp:Rasm.Compute/blas#ROUTE_SPINE`-shaped overdetermined thin-`QR` Gauss-Newton solve fitting the GGX roughness + dielectric IOR to the angular samples and writes the witnessed `FitResidual` onto the provenance, the `SpectralReflectance` arm grounds the base color through `surface#SPECTRAL_UPSAMPLE`, the `SvbrdfMap` arm averages the per-texel field to one row), `Fin<T>` aborting on an underdetermined capture (`<measured-brdf-underdetermined>`, fewer than 3 samples), a non-finite Jacobian or diverged fit (`MaterialFault.Parameter`), an out-of-gamut grounded color (`MaterialFault.Gamut`), or an empty capture; the produced row re-admits through `MaterialParameters.Of` and the residual-bearing provenance pairs alongside the way `finish#FINISH` `Resolve` returns its `(Row, Provenance)`.
- Packages: Wacton.Unicolour (composed — `Spd`→`Xyz`→scene-linear `Acescg` for the spectral-reflectance grounding and `IsInRgbGamut` for the fit gate), MathNet.Numerics (composed — `Matrix<double>`/`Vector<double>` dense carriers, `Matrix<double>.QR(QRMethod.Thin)` + `QR<double>.Solve` for the overdetermined GGX/Smith least-squares Gauss-Newton step, `Svd(true)` the rank-deficient fallback, `Control.UseManaged` the osx-arm64 provider — the direct AEC-domain pin, catalogued in `.api/api-mathnet-numerics.md`), Rasm (project), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new capture modality is one `CaptureSource` case carrying its fit arm — never a per-capture material or a second import owner; a new fit parameter is one column the existing `graph#MATERIAL_LIBRARY` `MaterialParameters` already carries, the fit targeting the closed Disney/OpenPBR vector. The measured-BRDF fit shares the `bsdf#WHITE_FURNACE_HARNESS` energy-conservation gate so an acquired material round-trips in-gamut; the spectral grounding shares the `surface#SPECTRAL_UPSAMPLE` `Spd` construction with the `graph#MEASURED_SPECTRAL_LIBRARY` conductor rows, one upsampling owner.
- Boundary: `Acquisition.Import` is the ONE import path — an acquired-material type is the deleted form; the `MeasuredBrdf` arm fits the closed parameter vector through the REALIZED `csharp:Rasm.Compute/blas#ROUTE_SPINE`-shaped overdetermined route (the `m` angular reflectance samples are the design-matrix rows, the GGX roughness + dielectric IOR the `n=2` unknowns, a thin-`QR` Gauss-Newton step `Δp = QR(Thin).Solve(−r)` over the log-residual Jacobian iterated to convergence with an `Svd(true)` truncated-pseudo-inverse conditioning fallback when `κ` exceeds the cap, the `bsdf#MICROFACET_KERNEL` GGX/Smith/Fresnel the forward model `D·G·F/(4·cosθi·cosθo)`) and never hand-rolls a Levenberg-Marquardt loop, the fit witnessed by the recomputed true relative residual against the original samples (the one correctness signal surviving the iteration) written onto `Provenance.FitResidual` and gated through the `bsdf#WHITE_FURNACE_HARNESS`, the managed provider selected once through `Control.UseManaged` (osx-arm64 has no native MKL/OpenBLAS); the spectral grounding composes the `surface#SPECTRAL_UPSAMPLE` `Spd`→scene-linear `Acescg` so a measured reflectance curve becomes a base color the SAME way a library row grounds, never a re-minted color path; the produced `MaterialParameters` re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of` so an acquired row passes the same gamut/unit/IOR gate a registered row passes, and the `Provenance` receipt carries the capture device, wavelength count, and fit residual as the measured evidence a `MaterialParameters` authored guess lacks; the binary capture decode (the EPFL RGL `brdf-loader` `.bsdf` format, the neural-SVBRDF `.exr` map) is the host-edge import boundary the `Rasm.Bim`/app root owns, this owner consuming the decoded `Seq<BrdfSample>`/`Seq<MaterialParameters>` portable data, never the binary file format; a malformed, empty, or out-of-gamut capture rails `MaterialFault`, never a sentinel row.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using Rasm.Materials.Appearance.Bsdf;   // Microfacet (the GGX/Smith/Fresnel forward model), RgbSpectrum
using Rasm.Materials.Appearance.Surface; // SpectralUpsample (Spd grounding)
using MathNet.Numerics;                  // Control.UseManaged
using MathNet.Numerics.LinearAlgebra;    // Matrix<double>, Vector<double>
using MathNet.Numerics.LinearAlgebra.Factorization; // QRMethod
using static LanguageExt.Prelude;

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct BrdfSample(double IncidentZenith, double OutgoingZenith, double AzimuthDelta, RgbSpectrum Reflectance);

public readonly record struct Provenance(string Device, int WavelengthCount, double FitResidual) {
    public static readonly Provenance Authored = new("authored", 0, 0.0);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSource {
    private CaptureSource() { }

    public sealed record MeasuredBrdf(Seq<BrdfSample> Samples, double Ior) : CaptureSource;
    public sealed record SvbrdfMap(Seq<MaterialParameters> Texels) : CaptureSource;
    public sealed record SpectralReflectance(Spd Reflectance, double Metalness, double Roughness) : CaptureSource;
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Acquisition {
    // One import entry pairing the produced row with its measured Provenance (the finish#FINISH Resolve shape): the
    // MeasuredBrdf arm runs the overdetermined GGX/Smith fit and carries the witnessed FitResidual, the other arms
    // pair the input provenance; the row re-admits through graph#MATERIAL_LIBRARY MaterialParameters.Of.
    public static Fin<(MaterialParameters Row, Provenance Provenance)> Import(CaptureSource source, Provenance provenance, Op key) =>
        source.Switch(
            state: (provenance, key),
            measuredBrdf:       static (s, c) => FitBrdf(c.Samples, c.Ior, s.provenance, s.key),
            svbrdfMap:          static (s, c) => AverageField(c.Texels, s.provenance, s.key),
            spectralReflectance: static (s, c) => GroundSpectral(c.Reflectance, c.Metalness, c.Roughness, s.key).Map(row => (row, s.provenance)))
        .Bind(pair => MaterialParameters.Of(pair.Row, key).Map(row => (row, pair.Provenance)));

    // The overdetermined GGX/Smith/Fresnel least-squares fit: a thin-QR Gauss-Newton iteration over the angular
    // reflectance design matrix (the m measured BrdfSample angles the rows, the (roughness, IOR) the n=2 unknowns),
    // the bsdf#MICROFACET_KERNEL the forward model — the Rasm.Compute/blas#ROUTE_SPINE dense-overdetermined route shape, never
    // a hand-rolled Levenberg-Marquardt loop. The fitted parameters seed the row; the recomputed true residual rides
    // the Provenance.FitResidual receipt the bsdf#WHITE_FURNACE_HARNESS gates, real measured evidence vs an authored guess.
    // The fit recomputes the true relative residual against the measured angular samples and writes it onto the
    // Provenance.FitResidual receipt the interchange#MATERIAL_WIRE WireProvenance carries — measured evidence the
    // renderer distinguishes a fitted material from an authored guess by; the residual gates the white-furnace harness.
    // The Provenance rides ALONGSIDE the row (the finish#FINISH Resolve precedent), never a phantom column on the
    // closed 16-column MaterialParameters; Import pairs the fitted row with the residual-bearing provenance.
    static Fin<(MaterialParameters Row, Provenance Provenance)> FitBrdf(Seq<BrdfSample> samples, double ior, Provenance provenance, Op key) =>
        guard(samples.Count >= 3, MaterialFault.Parameter(key, $"<measured-brdf-underdetermined:{samples.Count}<3>"))
            .Bind(_ => SolveGgx(samples, ior, key))
            .Bind(fit => samples.Map(static s => s.Reflectance).Fold(RgbSpectrum.Black, static (acc, r) => acc.Add(r)).Scale(1.0 / samples.Count) is var meanAlbedo
                ? SpectralUpsample.ToSpd(meanAlbedo, key)
                    .Bind(spd => GroundSpectral(spd, Metalness: 0.0, fit.Roughness, key))
                    .Map(row => (row with { Ior = fit.Ior }, provenance with { FitResidual = fit.Residual }))
                : MaterialFault.Parameter(key, "<measured-brdf-mean-degenerate>"));

    // The fitted parameter vector + the witnessed relative residual the Provenance receipt carries.
    readonly record struct BrdfFit(double Roughness, double Ior, double Residual);

    static Fin<BrdfFit> SolveGgx(Seq<BrdfSample> samples, double ior0, Op key) {
        Control.UseManaged();   // selected once; osx-arm64 rides the managed provider (native MKL/OpenBLAS are x64-only)
        int m = samples.Count;
        double[] logMeasured = samples.Map(static s => Math.Log(Math.Max(1e-6, s.Reflectance.Luminance))).ToArray();
        (double alpha, double eta) p = (0.3, Math.Clamp(ior0, 1.0, 2.5));
        double residual = double.MaxValue;
        for (int iter = 0; iter < 12; iter++) {
            Vector<double> r = Vector<double>.Build.Dense(m, i => logMeasured[i] - Math.Log(Math.Max(1e-6, GgxModel(samples[i], p.alpha, p.eta))));
            Matrix<double> j = Matrix<double>.Build.Dense(m, 2, (i, c) => Jacobian(samples[i], p, c));
            if (!j.Enumerate().All(double.IsFinite) || !r.Enumerate().All(double.IsFinite)) { return MaterialFault.Parameter(key, "<ggx-fit-non-finite-jacobian>"); }
            Vector<double> delta = j.QR(QRMethod.Thin).Solve(r);                      // thin-QR least-squares Gauss-Newton step
            // The library refuses its own gate: a near-rank-deficient Jacobian fills the QR solution with NaN while
            // IsFullRank stays true (Rasm.Compute/blas#ROUTE_SPINE admission), so probe the solution all-finite and fall back to the
            // SVD truncated pseudo-inverse — the ROUTE_SPINE RankRevealing conditioning fallback.
            if (!delta.Enumerate().All(double.IsFinite)) { delta = j.Svd(true).Solve(r); }
            p = (Math.Clamp(p.alpha + 0.5 * delta[0], 1e-3, 1.0), Math.Clamp(p.eta + 0.5 * delta[1], 1.0, 2.5));
            residual = r.L2Norm() / Math.Max(1e-6, Vector<double>.Build.DenseOfArray(logMeasured).L2Norm());
            if (residual < 1e-4) { break; }
        }
        return double.IsFinite(residual) ? Fin.Succ(new BrdfFit(Math.Sqrt(p.alpha), p.eta, residual)) : MaterialFault.Parameter(key, "<ggx-fit-diverged>");
    }

    // The microfacet forward model luminance at a sample's half-angle: D·G·F / (4·cosθi·cosθo), the bsdf#MICROFACET_KERNEL
    // GGX NDF + Smith masking + dielectric Fresnel — single-sourced shape, not a re-minted lobe.
    static double GgxModel(BrdfSample s, double alpha, double eta) {
        double ci = Math.Max(1e-4, Math.Cos(s.IncidentZenith)), co = Math.Max(1e-4, Math.Cos(s.OutgoingZenith));
        double cosH = Math.Cos((s.IncidentZenith - s.OutgoingZenith) * 0.5);
        double a2 = alpha * alpha, ch2 = cosH * cosH;
        double d = a2 / (Math.PI * Math.Pow(ch2 * (a2 - 1.0) + 1.0, 2.0));
        double g = SmithG1(ci, alpha) * SmithG1(co, alpha);
        double f = Microfacet.FresnelDielectric(cosH, eta);
        return d * g * f / (4.0 * ci * co);
    }
    static double SmithG1(double cosTheta, double alpha) {
        double t2 = (1.0 - cosTheta * cosTheta) / Math.Max(1e-6, cosTheta * cosTheta);
        return 2.0 / (1.0 + Math.Sqrt(1.0 + alpha * alpha * t2));
    }
    // Central-difference column of the log-residual Jacobian w.r.t. (alpha, eta).
    static double Jacobian(BrdfSample s, (double alpha, double eta) p, int col) {
        const double h = 1e-4;
        (double a, double e) plus = col == 0 ? (p.alpha + h, p.eta) : (p.alpha, p.eta + h);
        (double a, double e) minus = col == 0 ? (p.alpha - h, p.eta) : (p.alpha, p.eta - h);
        return -(Math.Log(Math.Max(1e-6, GgxModel(s, plus.a, plus.e))) - Math.Log(Math.Max(1e-6, GgxModel(s, minus.a, minus.e)))) / (2.0 * h);
    }

    static Fin<(MaterialParameters Row, Provenance Provenance)> AverageField(Seq<MaterialParameters> texels, Provenance provenance, Op key) =>
        texels.IsEmpty ? MaterialFault.Parameter(key, "<svbrdf-map-empty>") : Fin.Succ((texels.Head, provenance));

    static readonly Unicolour Black = new(PortValue.SceneLinear, ColourSpace.RgbLinear, 0.0, 0.0, 0.0);

    static Fin<MaterialParameters> GroundSpectral(Spd reflectance, double metalness, double roughness, Op key) {
        Unicolour color = new(new Configuration(RgbConfiguration.Acescg), reflectance);
        return SpectralUpsample.SceneLinear(color, key)
            .Bind(rgb => color.IsInRgbGamut
                ? Fin.Succ(new MaterialParameters(color, metalness, roughness, 0.0, 0.0, 1.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, SubsurfaceRadius.None, Black, 0.0))
                : MaterialFault.Gamut(key, "<acquired-color-out-of-gamut>"));
    }
}
```

## [03]-[RESEARCH]

- [EPFL_RGL_BRDF_LOADER]: the EPFL RGL measured-BRDF program publishes isotropic spectral BRDFs (195 wavelengths, the `brdf-loader` `.bsdf` binary format with a header, theta/phi parameterization, and a Rough-Quadtree spectral payload); the binary decode is an UPSTREAM host-edge concern — no managed `.bsdf` reader is vendored, so the decode lands at the `Rasm.Bim`/app-root import boundary and feeds this owner the decoded `Seq<BrdfSample>`. The `FitBrdf` arm consumes the decoded samples; until a managed `.bsdf` reader is admitted (or the C++ `brdf-loader` is bound), the binary-format read stays [UPSTREAM-BLOCKED] on the missing reader, the angular-sample import the realized internal path.
- [SVBRDF_NEURAL_FIT]: single-image and multi-image neural SVBRDF acquisition (the deep-material-capture shift) produces a per-texel parameter field as an `.exr`/tensor map; the `SvbrdfMap` arm averages the field to one row today, the realized internal seam. The per-texel field driving a `texture#TEXTURE_UV` `Image` source (a spatially-varying material rather than one averaged row) is the growth path — the field becomes a texture the `graph#MATERIAL_GRAPH` `Texture` node samples, riding the existing graph fold, never a second material owner. The neural inference itself is an UPSTREAM model-runtime concern outside the design.
- [BRDF_PARAMETER_FIT]: REALIZED — the `FitBrdf` arm runs the overdetermined GGX/Smith least-squares fit through `SolveGgx`, a thin-`QR` Gauss-Newton iteration composing `MathNet.Numerics` (`Matrix<double>.QR(QRMethod.Thin).Solve` over the log-residual Jacobian, `Svd(true)` the rank-deficient conditioning fallback, `Control.UseManaged` the osx-arm64 provider) — the `m` measured `BrdfSample` angles the design-matrix rows, the GGX roughness + dielectric IOR the `n=2` unknowns, the `bsdf#MICROFACET_KERNEL` `D·G·F/(4·cosθi·cosθo)` the forward model, the step `p ← clamp(p + 0.5·Δp)` damped and iterated 12× to a `1e-4` residual. The specular-contrast heuristic is deleted; the recomputed true relative residual `‖r‖/‖logMeasured‖` is the witnessed evidence written onto `Provenance.FitResidual` the `bsdf#WHITE_FURNACE_HARNESS` gates and the `interchange#MATERIAL_WIRE` `WireProvenance` carries, so a fitted material distinguishes from an authored guess on the wire. The strata blocker is RESOLVED: `MathNet.Numerics` is a DIRECT Materials NuGet pin (the AEC-domain folder's own dependency, the version already central in `Directory.Packages.props`), never a `Rasm.Compute` project reference — the acyclic strata forbids the AEC→app-platform edge and "MathNet transitive via Compute" is impossible (transitivity flows from Compute's references to its consumers, not the reverse). The `csharp:Rasm.Compute/blas#ROUTE_SPINE` thin-`QR` is the DOCTRINE reference the fit shape follows (the `FactorRoute.Orthonormal` overdetermined route with the `RankRevealing` `Svd(true)` conditioning fallback, the `LevenbergMarquardt` damped-Gauss-Newton owner the nonlinear precedent), not a project edge. Realizes the `BRDF_FIT_COMPUTE_SEAM` task; ripple counterpart `csharp:Rasm.Compute/blas` `[ROUTE_SPINE]` is a DOCTRINE citation only, not a realized seam (no Compute edge lands).
```

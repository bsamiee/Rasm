# [MATERIALS_ACQUISITION]

`Acquisition.Import` folds the closed measured-BRDF, SVBRDF-map, spectral-reflectance, and neural-plane `CaptureSource` family into the `AcquiredMaterial` product — the `MaterialParameters` row the material library registers and the lobe family shades, its measured `Provenance`, and, for a plane-bearing capture, the admitted `Raster/set#TEXTURE_SET` `TextureSet` itself, so a photo-to-PBR import returns a spatially-varying shadeable material rather than only the mean the seam key carries. Capture fitting composes the GGX/Smith/Fresnel kernel over reconstructed incident and outgoing directions, fits the anisotropic conductor-or-dielectric parameter vector, grounds spectral color, and gates the admitted row through the white-furnace and material-fault rails. `Provenance` carries capture method, sample counts, residual, conditioning, and grounded-color chromaticity as the one measured-evidence receipt shared by wire and pigment-mix consumers. Per-capture acquired-material types and private BRDF kernels are deleted forms.

## [01]-[INDEX]

- [02]-[ACQUISITION]: `CaptureSource` closes the `[Union]` capture family, `BrdfSample` records angular reflectance, `AcquiredMaterial` carries the import product, and `Provenance` carries the measured-capture receipt over the `CaptureMethod` `[SmartEnum]` instrument discriminant (counts, conditioning, chromaticity, and model-attribution columns beside the residual); `Acquisition.Import` composes the `Microfacet`-composed anisotropic conductor/dielectric forward model over reconstructed `LocalVector` directions, the thin-`QR` Gauss-Newton fit-and-ground fold over the bounded `(αx, αy, φ[, η])` parameter vector, the per-texel SVBRDF field average, the neural-plane arm's `SetBind` summary over an admitted set, the chart-solved colour calibration every arm crosses, and the `Freeze`/`Thaw` spectral-EXR round trip a measured curve persists through.

## [02]-[ACQUISITION]

- Owner: `Acquisition` static import fold; `CaptureSource` `[Union]` (measured-brdf · svbrdf-map · spectral-reflectance · neural-planes); `AcquiredMaterial` the one import product; `BrdfSample` the goniophotometer angular-reflectance record over `(θi, θo, Δφ)`; `CaptureMethod` `[SmartEnum<string>]` the measurement-instrument discriminant (goniophotometer · spectrophotometer · neural-svbrdf · neural-planes · pigment-mix · authored); `Provenance` the measured-capture receipt this page owns, keyed by `CaptureMethod` and carrying the angular-sample and spectral-band COUNTS, the `FitConditionNumber`/`FitRank` conditioning witness, the grounded-color `DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv` chromaticity readout beside the `FitResidual`, the `Option<ModelCardId>`/`Option<LicenseClass>`/`Option<ContentAddress>` model attribution a neural capture fills and every other capture leaves absent, and the `Calibrated`/`CalibrationDeltaE` chart witness; `SpectralCurve` the durable sampled-spectrum carrier and `CaptureCalibration` the twenty-four measured chart patches.
- Cases: capture {`MeasuredBrdf` (an angular `Seq<BrdfSample>` over incident/outgoing zenith and relative azimuth, the dielectric IOR seed, and the conductor/dielectric Fresnel discriminant — `Option<ComplexIor>` whose `Some` carries a measured complex IOR fixing `FresnelConductor` so only `(αx, αy, φ)` are fit, whose `None` fits the dielectric `(αx, αy, φ, η)`), `SvbrdfMap` (a per-texel `Seq<MaterialParameters>` field from a neural fit, collapsed to one row and discarded), `SpectralReflectance` (a `SpectralCurve` reflectance grid with its metalness/roughness — the carrier that persists, where an `Spd` cannot answer what grid built it), `NeuralPlanes` (an admitted `TextureSet` assembled from the `neural#MODEL_REGISTRY` stage plan's outputs, its `Seq<StageResult>` evidence, and the caller's fallback row — the planes SURVIVE the import)} — the closed capture family: `CaptureSource` admits a capture as one case, never a capture subtype, and `CaptureMethod` admits every measurement instrument as one ROW (the goniophotometer, the spectrophotometer, the neural-SVBRDF field, the neural-plane set, the `finish#FINISH` Kubelka-Munk pigment-mix, the authored sentinel), never a per-instrument capture type.
- Entry: `public static Fin<AcquiredMaterial> Import(CaptureSource source, Provenance provenance, Op key, Option<CaptureCalibration> calibration = default)` — the import fold, its optional chart context the one axis no capture value carries pairing the fitted closed `MaterialParameters` vector with its measured `Provenance` and, on the plane-bearing arm, the admitted set (the `MeasuredBrdf` arm runs the realized `csharp:Rasm.Compute/Tensor/blas#DENSE_ALGEBRA`-shaped overdetermined thin-`QR` Gauss-Newton solve fitting the anisotropic GGX `(αx, αy)`, the grain azimuth, and the dielectric IOR when no conductor IOR is supplied to the angular samples, projects the fit to the Disney `Roughness`/`Anisotropy`/`AnisotropyRotation` row columns, stamps the `goniophotometer` `CaptureMethod` and the sample count onto the receipt, and writes the witnessed `FitResidual` beside the final-Jacobian `FitConditionNumber`/`FitRank`; the `SpectralReflectance` arm grounds the base color through `surface#SPECTRAL_UPSAMPLE` and stamps the `spectrophotometer` method; the `SvbrdfMap` arm AVERAGES the per-texel field — the scene-linear base-color mean, the per-column scalar mean, and the per-band `SubsurfaceRadius` mean — into one row and stamps the `neural-svbrdf` method with the texel count; the `NeuralPlanes` arm binds the admitted set through the ONE `Raster/set#SET_BIND` `BindTarget.Summary` measured fold, returns the set alongside the summary row, and stamps the `neural-planes` method with the set digest as its device, the summed tiles inferred as its sample count, the WORST stage golden delta as its residual, and the MOST RESTRICTIVE contributing `ModelCard`/`LicenseClass` as its attribution), `Fin<T>` aborting on an underdetermined capture (`<measured-brdf-underdetermined>`, `m ≤ n` against the arm's own widened unknown count), a non-finite Jacobian or diverged fit (`MaterialFault.Parameter`), an out-of-gamut grounded color (`MaterialFault.Gamut`), or an empty capture; the produced row re-admits through `MaterialParameters.Of`, the ONE re-admission site also stamping the grounded base color's chromaticity readout (`DominantWavelength`/`ExcitationPurity`/`Temperature` → `DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv`) onto the receipt for EVERY arm, and the residual-bearing provenance rides the product beside the row the way `finish#FINISH` `Resolve` returns its `(Row, Provenance)`; `SyntheticGrid(int seed, int count, Op key)` rails the deterministic stratified synthetic capture the benchmark corpus pins — a non-positive count refuses on the same `Fin` rail every sibling entry holds — geometry, ground-truth alphas, and a ground-truth grain azimuth derived wholly from the seed through the KERNEL's one lane-keyed draw, reflectance the kernel-composed dielectric forward model at those parameters so the fit workload has a known answer in direction as well as magnitude. `public static Fin<ReadOnlyMemory<byte>> Freeze(SpectralCurve curve, Op key)` and `public static Fin<SpectralCurve> Thaw(ReadOnlyMemory<byte> payload, int wavelengthCount, Op key)` are the durable pair a measured reflectance persists through, the receipt's own `WavelengthCount` the round-trip witness the thaw proves against.
- Packages: Wacton.Unicolour (composed — `new Unicolour(PortValue.SceneLinear, Spd)`→`Xyz`→scene-linear `Acescg` for the spectral-reflectance grounding, the `RgbLinear.Triplet` channel read for the SVBRDF base-color mean, `IsInRgbGamut` for the fit gate, and the `DominantWavelength`/`ExcitationPurity`/`Temperature` chromaticity readout stamped onto the receipt), MathNet.Numerics (composed — `Matrix<double>`/`Vector<double>` dense carriers, `Matrix<double>.Build.Dense`/`Vector<double>.Build.Dense` the design/residual build, `Matrix<double>.QR(QRMethod.Thin)` + `QR<double>.Solve` for the overdetermined GGX/Smith least-squares Gauss-Newton step, `Svd(true)` the rank-deficient fallback, `Svd(false)` the S-only conditioning handle whose `ConditionNumber`/`Rank` witness the converged Jacobian onto the receipt, `Vector<double>.L2Norm` the residual witness, `Control.UseManaged` the osx-arm64 provider — the direct AEC-domain pin, catalogued in `libs/csharp/.api/api-mathnet-numerics.md`), `Rasm.Materials.Raster` (composed — `TextureSet` the acquired plane bundle, `SetBind.Bind` under `BindTarget.Summary` the ONE measured summary fold, `TextureSet.Digest` the receipt's set identity), `neural#MODEL_REGISTRY` (composed — `StageResult` the stage evidence, `ModelRegistry.Rows` the card lookup the attribution folds, `ModelCardId`/`LicenseClass`/`ModelCard.Artefact` the receipt columns), TinyEXR.NET (composed — `Spectral.CreateReflectivePart`/`GetWavelengths`/`GetChannelName`/`IsSpectral` the wavelength-sampled part family, `ExrFile.SaveToMemory`/`LoadFromMemory` over `WriterResult<byte[]>`/`ReaderResult<Image>`, `Compression.ZIP` the durable row — the ONE container a measured curve persists through), Wacton.Unicolour.Datasets (composed — `Macbeth.All`, the twenty-four-patch reference the chart solve targets), Rasm (project — `Op`, `Deterministic` the one splitmix64 draw owner, `Rasm.Element.Projection` `ContentAddress`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new capture modality is one `CaptureSource` case carrying its fit arm and one `AcquiredMaterial` construction — never a per-capture material, a second import owner, or a `ImportPlanes`/`ImportBrdf` entry pair; a new measurement instrument is one `CaptureMethod` row the receipt stamps, never a per-device receipt type; a new fit parameter is one column of the bounded parameter vector with its `(Lo, Hi)` bounds row projected onto a column the existing `graph#MATERIAL_LIBRARY` `MaterialParameters` already carries — the solver is column-generic over `p` and the central-difference Jacobian derives per column, so widening the unknown set is data, never a solver edit; a new receipt fact is one init-defaulted column on the `Provenance` record (every existing construction binds unchanged), and the generated `interchange#MATERIAL_WIRE` `WireMap.ToWire(Provenance)` runs `RequiredMappingStrategy.Both`, so the new column compels a `WireProvenance` mirror or an explicit ignore row at the wire build — a compile-forced decision, never a silent drop. Measured-BRDF fitting shares the `bsdf#BSDF_GOLDEN` energy-conservation rows so an acquired material round-trips in-gamut; spectral grounding shares the `surface#SPECTRAL_UPSAMPLE` `Spd` construction with the `surface#CONDUCTOR_IOR` conductor rows, one upsampling owner.
- Law: `SolveGgx` is the page's one `[EXPRESSION_SPINE]` kernel exemption — the 12-iteration Gauss-Newton loop fills fixed-length `double[]` log-residual buffers and threads the mutable bounded parameter vector (`αx, αy, φ[, η]`, each column clamped to its `(Lo, Hi)` bounds row — `φ` over the half turn an ellipse's own symmetry makes its whole observable domain) and `residual` scalar by index across the bounded iteration, the in-place perturb-restore central-difference `Jacobian` and the per-cell `Reflectance` forward-model evaluation riding the same carve-out — the admitted boundary-numeric-kernel exemption from the immutable-fold law for the dense least-squares solve (the same carve-out `surface#SPECTRAL_UPSAMPLE` `ToSpd` and `bsdf#LAYERED_COMPOSITION` `LayeredBsdf.Of` name); the `Solve` chart least-squares and the `Freeze`/`Thaw` container legs take the same carve for their design-matrix build and their `WriterResult`/`ReaderResult` probes; every other operation on the page — `Import`, `Calibrate`, `FitBrdf`, `AverageField`, `GroundSpectral`, the `FieldSum` fold — is expression-bodied and rail-threaded.
- Boundary: `Acquisition.Import` is the ONE import path and `AcquiredMaterial` its ONE product — a per-capture acquired-material type is the deleted form; the `MeasuredBrdf` arm fits the closed parameter vector through the REALIZED `csharp:Rasm.Compute/Tensor/blas#DENSE_ALGEBRA`-shaped overdetermined route (the `m` angular reflectance samples are the design-matrix rows, the anisotropic GGX `(αx, αy)`, the grain azimuth `φ`, and the dielectric IOR on the `None`-conductor arm are the `n ∈ {3, 4}` unknowns, a thin-`QR` Gauss-Newton step `Δp = QR(Thin).Solve(−r)` over the log-residual Jacobian iterated to convergence with an `Svd(true)` truncated-pseudo-inverse conditioning fallback when the thin-`QR` step goes non-finite, the `bsdf#MICROFACET_KERNEL` GGX/Smith/Fresnel the forward model `D·G·F/(4·cosθi·cosθo)`) and never hand-rolls a Levenberg-Marquardt loop; the forward model is SINGLE-SOURCED on the kernel — `Acquisition.Reflectance` reconstructs the incident `wi = (sinθi, 0, cosθi)` and outgoing `wo = (sinθo·cosΔφ, sinθo·sinΔφ, cosθo)` `bsdf#SHADING_FRAME` `LocalVector` directions from the sample's `(θi, θo, Δφ)`, rotates both by `−φ` about local Z so the fitted grain axis IS the basis the anisotropic terms read (the same rotate-the-lobe-not-the-frame form `bsdf#LOBE_FAMILY` shades through, so fit and shade share one convention), forms the true half-vector `h = (wi+wo).Normalize()`, and reads `Microfacet.Ndf(h, αx, αy)` · `Microfacet.MaskingShadowing(wo, wi, αx, αy)` · the discriminant-routed Fresnel (`Microfacet.FresnelConductor(|wo·h|, ComplexIor).Luminance` when the capture carries a measured conductor IOR, `Microfacet.FresnelDielectric(wo·h, η)` otherwise) / `(4·|cosθi|·|cosθo|)`, so the design matrix carries the genuine microfacet response at each measured geometry — a brushed-metal or anisotropic capture fits `(αx, αy)` against the conductor Fresnel and lands `Metalness = 1.0` with the fitted `Anisotropy`, never a rough dielectric with a hardcoded metalness — and the page NEVER re-mints the NDF, the Smith masking, or the Fresnel term as a private kernel (the prior hand-rolled `GgxModel`/`SmithG1`/inline-`D` with a `cos((θi−θo)/2)` half-angle that dropped the azimuth is the deleted form); the fitted parameters project to the row as `Roughness = (αx·αy)^¼`, `Anisotropy = (1 − min/max)/0.9`, and `AnisotropyRotation` the fitted azimuth of the `αx` axis on the row's unit convention — the FULL inverse of the Disney aspect remap, so a brushed-metal capture round-trips its grain DIRECTION as well as its magnitude; the azimuth was always IN the residual surface (the design matrix carries `(θi, θo, Δφ)` per sample and the forward model reconstructs `wo` from all three) and only the projection threw it away, while an in-plane capture whose constant azimuth cannot observe `αy` still reads `FitRank < n` with `+Inf` `ConditionNumber`, so its rotation is receipt-flagged unobserved rather than fabricated; the fit is witnessed by the recomputed true relative residual `‖r‖/‖logMeasured‖` against the original samples (the one correctness signal surviving the iteration) written onto `Provenance.FitResidual` and gated through `bsdf#BSDF_GOLDEN`, and by the converged-Jacobian `Svd(false)` S-only conditioning readout written onto `FitConditionNumber`/`FitRank` — an in-plane capture whose constant azimuth cannot observe `αy` reads as `FitRank < n` with `+Inf` `ConditionNumber` (the `Svd` rank-deficient contract), so under-observation is receipt evidence, never a silent mis-fit — the managed provider selected once through `Control.UseManaged` (osx-arm64 has no native MKL/OpenBLAS — a per-call-site `TryUseNativeMKL` is the named defect); the spectral grounding composes the `surface#SPECTRAL_UPSAMPLE` `Spd`→scene-linear `Acescg` through the ONE `PortValue.SceneLinear` working space (the kernel `RgbProfile.Acescg.Configuration` instance the graph page names — one instance, one Unicolour lazy-conversion cache identity) so a measured reflectance curve becomes a base color the SAME way a library row grounds, never a re-minted inline `new Configuration(RgbConfiguration.Acescg)` at any tier and never a local `SceneConfig` re-export alias; the `SvbrdfMap` arm computes a REAL field average — the base color averaged in scene-linear through `RgbLinear`, every Disney scalar column averaged, and the `SubsurfaceRadius` averaged per band — not a `texels.Head` first-row read (the prior placeholder the prose mislabelled as an average is the deleted form), so a 256×256 neural SVBRDF field collapses to the one representative row the renderer shades; the produced `MaterialParameters` re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of` so an acquired row passes the same gamut/unit/IOR gate a registered row passes, the ONE re-admission site stamping the admitted row's grounded base-color chromaticity — `DominantWavelength`/`ExcitationPurity`/`Temperature` onto `DominantWavelengthNm`/`ExcitationPurity`/`CctKelvin`/`CctDuv`, the `photometric#PHOTOMETRIC` `EmissionInput` Wxy-projection precedent applied uniformly to every arm (`CctDuv` carries the off-locus distance, so a reader gates `CctKelvin` on `|Duv| ≤ 0.05` before trusting the correlated temperature) — and the `Provenance` receipt carries the `CaptureMethod` instrument, the angular sample or spectral band count, the fit residual, the conditioning witness, and the chromaticity readout as the measured evidence a `MaterialParameters` authored guess lacks — the generated `interchange#MATERIAL_WIRE` `WireMap.ToWire(Provenance)` mirrors `Device`/`WavelengthCount`/`FitResidual`/`Method`/`AngularSamples` by member name and derives `Measured` structurally as `p != Provenance.Authored`, its `RequiredMappingStrategy.Both` diagnostics forcing every receipt column onto a wire mirror or an explicit ignore row; the `NeuralPlanes` arm consumes an ALREADY-ADMITTED `TextureSet` and never assembles one — `Rasm.Compute` executes the stage plan, the app root resolves each `StageResult.Planes` channel output — the frozen prior-drop projection, so a `delit` or `depth` prior reaches no set — through `Raster/codec#RASTER_CODEC` and admits the bundle through `Raster/set#TEXTURE_SET` `TextureSet.Of`, and this owner binds what admission already proved, so an inference product reaches the same extent, transfer, convention, and payload gates a pressed set passes; the averaged row comes from the ONE `SetBind` `Average` measured fold rather than a second mean spelled here, and the set rides back so the caller keeps a spatially-varying material where the `SvbrdfMap` arm deliberately discards its field; the binary capture decode (the EPFL RGL `brdf-loader` `.bsdf` format, the neural-SVBRDF `.exr` map) is the host-edge import boundary the `Rasm.Bim`/app root owns, this owner consuming the decoded `Seq<BrdfSample>`/`Seq<MaterialParameters>` portable data, never the binary file format; a malformed, empty, or out-of-gamut capture rails `MaterialFault`, never a sentinel row. COLOUR CALIBRATION is the measurement rigour the receipt's own vocabulary claims: `CaptureMethod.NeuralPlanes` and `NeuralSvbrdf` both stamp a receipt reading MEASURED (`p != Provenance.Authored`) on a base colour derived from a photograph whose white balance and exposure nothing verified, so an optional `CaptureCalibration` of the twenty-four ColorChecker patches solves one 3x3 through the SAME thin-`QR` route `SolveGgx` composes and stamps `Calibrated` beside the mean CIEDE2000 residual it left — a chart-solved matrix with its own delta-E is what separates a measured reflectance from a camera's guess; the solve runs in AP1-linear with `Macbeth`'s sRGB/D50 references rebased through `ConvertToConfiguration` FIRST (the colorimetric boundary `graph#MATERIAL_LIBRARY` `Named` enforces), so the matrix corrects WITHIN one space rather than silently absorbing a chromatic adaptation, and it applies at the one pre-admission site every arm crosses rather than inside a single arm a fifth modality would then miss. A MEASURED CURVE PERSISTS: `SpectralCurve` is the durable sampled-spectrum carrier because Wacton's `Spd` is a one-way XYZ intake that republishes no grid, and `Freeze`/`Thaw` round-trip it through a wavelength-sampled EXR part whose channel names ARE the sampling grid — one texel wide, `Compression.ZIP`, `MaximumWavelengthCount` bounding a 195-wavelength goniophotometer grid with decades of headroom; a `codec#RASTER_FORMAT` row is the WRONG home and stays unminted, because a wavelength grid is not a texture plane and admitting it as one would put a spectrum through extent, transfer, and mip gates that describe pixels. Without the pair a grounded triple survived an import and the curve did not, so a re-ground under a different working space was impossible; `Provenance.WavelengthCount` is the round-trip WITNESS the thaw proves its decoded grid against rather than a bare count nothing checks.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Runtime.InteropServices;     // MemoryMarshal — the spectral channel's float window
using LanguageExt;                        // Seq, Option, Fin
using Rasm.Domain;                        // Op, Deterministic (the ONE splitmix64 draw owner)
using Rasm.Materials.Appearance.Bsdf;    // Microfacet (GGX/Smith/Fresnel), LocalVector (the reconstructed directions), RgbSpectrum, ComplexIor, MaterialFault
using Rasm.Materials.Appearance.Surface; // SpectralUpsample (ToSpd + the SceneLinear grounding gate)
using Rasm.Materials.Appearance.Graph;   // MaterialParameters, SubsurfaceRadius, PortValue (the SceneLinear Acescg Configuration)
using Rasm.Element.Projection;            // ContentAddress — the seam address spelling the artefact column carries
using Rasm.Materials.Raster;              // TextureSet, SetBind, BindTarget, SetBinding — the acquired plane set and its ONE measured summary fold
using Rasm.Numerics;                      // GamutPolicy — the kernel working-gamut row the grounding gate checks through
using MathNet.Numerics;                   // Control.UseManaged
using MathNet.Numerics.LinearAlgebra;     // Matrix<double>, Vector<double>
using MathNet.Numerics.LinearAlgebra.Factorization; // QRMethod, Svd<T> (the S-only conditioning handle)
using TinyEXR.V3;                         // ExrFile, Image, Part, Spectral, Compression — the durable spectral-curve container
using Wacton.Unicolour;                   // Unicolour, Spd, ColourSpace, ColourTriplet, DeltaE
using Wacton.Unicolour.Datasets;          // Macbeth — the 24-patch ColorChecker reference the calibration solve targets
using Thinktecture;                       // [SmartEnum]/[Union]/[KeyMember*] + ComparerAccessors.StringOrdinal (the Thinktecture ordinal-key policy every folder table pins)
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;      // folder-root, beside graph#MATERIAL_LIBRARY MaterialParameters and finish#FINISH

// --- [TYPES] -------------------------------------------------------------------------------
// CaptureMethod keys Provenance by measurement instrument: a goniophotometer angular BRDF, a
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
    public static readonly CaptureMethod NeuralPlanes = new("neural-planes");   // the neural#MODEL_REGISTRY stage plan's PLANE product — a whole set, distinct from the SvbrdfMap per-texel field that collapses to one row
    public static readonly CaptureMethod PigmentMix = new("pigment-mix");   // the finish#FINISH Kubelka-Munk pigment-mix receipt — a MEASURED-pigment finish, not authored
    public static readonly CaptureMethod Authored = new("authored");
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct BrdfSample(double IncidentZenith, double OutgoingZenith, double AzimuthDelta, RgbSpectrum Reflectance);

// SpectralCurve carries a durable sampled spectrum — start wavelength, sample interval, and the non-negative
// coefficient grid, admitted ONCE at Of so the interior never re-checks and Spd never sees a rejected grid.
// Wacton's Spd is a ONE-WAY XYZ intake: the package publishes no reader for the grid a construction handed it, so a
// measured curve that must persist, round-trip, or be re-ground under a different working space cannot live as an
// Spd and lives here instead, crossing at the point of integration through ToSpd. The photometric#PHOTOMETRIC
// emission SPD reads this SAME carrier, so one sampled-spectrum shape spans reflectance and emission rather than
// two column triples that drift apart the first time either grows a column.
public readonly record struct SpectralCurve(int StartNm, int IntervalNm, ReadOnlyMemory<double> Coefficients) {
    public static Fin<SpectralCurve> Of(int startNm, int intervalNm, ReadOnlyMemory<double> coefficients, Op key) =>
        intervalNm is not (0 or 1 or 5)                                                    // the Spd.IsValid interval domain, proven BEFORE construction
            ? MaterialFault.Parameter(key, $"<spectral-curve-interval:{intervalNm}>")
            : startNm <= 0 || coefficients.Length < 2
                ? MaterialFault.Parameter(key, $"<spectral-curve-extent:{startNm}@{coefficients.Length}>")
                : coefficients.Span.ContainsAnyInRange(double.NegativeInfinity, -double.Epsilon)
                    ? MaterialFault.Parameter(key, "<spectral-curve-negative-coefficient>")
                    : Fin.Succ(new SpectralCurve(startNm, intervalNm, coefficients));

    public Spd ToSpd() => new(StartNm, IntervalNm, Coefficients.ToArray());
    public double WavelengthAt(int index) => StartNm + (index * IntervalNm);
}

// CaptureCalibration holds the twenty-four patch triples a ColorChecker photograph yields, in the SAME order Macbeth.All
// publishes — order IS the correspondence, so a shuffled roster solves a matrix onto the wrong references and no
// residual can detect it, which is why the arity gate names the count rather than trusting a caller's zip.
public readonly record struct CaptureCalibration(Seq<Unicolour> Patches) {
    public static Fin<CaptureCalibration> Of(Seq<Unicolour> patches, Op key) =>
        patches.Count == toSeq(Macbeth.All).Count
            ? Fin.Succ(new CaptureCalibration(patches))
            : MaterialFault.Parameter(key, $"<capture-calibration-arity:{patches.Count}>");
}

// Provenance is the measured-capture receipt — a sealed record, never a struct: Method is a reference-typed row and a
// struct `default` would ghost it null past every initializer. Generated interchange#MATERIAL_WIRE
// WireMap.ToWire(Provenance) mirrors Device/WavelengthCount/FitResidual/Method/AngularSamples and the six evidence
// columns below by member name, derives Measured structurally (p != Provenance.Authored), and lowers the Option-typed
// attribution pair through its own readers onto the WireProvenance ModelCard/License keys — RequiredMappingStrategy.Both
// makes an unmirrored column a BUILD BREAK, so a receipt field never reaches a peer as silence;
// every later column is init-defaulted enrichment (every existing
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

    // Calibrated says a chart-solved matrix corrected this capture and CalibrationDeltaE reports
    // the mean CIEDE2000 residual it left over the twenty-four patches. Both matter because a capture that reads
    // MEASURED on the strength of a photograph nothing white-balanced is an authored guess wearing a receipt, and a
    // solved matrix with a large residual is a chart photographed under conditions the linear model cannot correct.
    // Explicit presence rides the residual: an uncalibrated capture measures no residual, and a zero on a required
    // slot reads to a grader as a perfect chart solve. Its python peer already declares this column
    // `float | None` for exactly that reason, so a bare double here emits a proto zero that peer's absent arm can
    // never see, and both sides then disagree on what an uncalibrated capture reports.
    public Option<double> CalibrationDeltaE { get; init; }
    public bool Calibrated { get; init; }

    // ModelCard/License carry the attribution a neural capture holds and every other capture genuinely lacks — TYPED ABSENCE,
    // never an empty-string sentinel: a goniophotometer row has no model card, and Option collapses the reader's "no model" vs
    // "a model whose id happened to be blank" distinction. Both columns travel together because a licence is meaningless
    // without the artefact it governs, and the wire flattens the pair to their keys.
    public Option<ModelCardId> ModelCard { get; init; }
    public Option<LicenseClass> License { get; init; }

    // ModelArtefact addresses the BYTES the governing card ran: a card names a model and this
    // names the weights, so a receipt can answer which artefact produced a plane rather than only which row
    // authorized one. Optional at BOTH ends by the same reason the neural registry gives — caller-supplied weights
    // are a deployment fact the registry never addresses — so a mandatory column here would forge an address for
    // exactly the rows that have none.
    public Option<ContentAddress> ModelArtefact { get; init; }

    public static readonly Provenance Authored = new("authored", 0, 0.0);

    // Provenance.Of stamps the acquisition arms with a measured device name, its spectral band or angular
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
    // Reflectance carries a curve rather than a bare Spd: a measured reflectance the estate can PERSIST and re-ground is the whole
    // point of a spectrophotometer capture, and an Spd cannot answer what grid it was built from.
    public sealed record SpectralReflectance(SpectralCurve Reflectance, double Metalness, double Roughness) : CaptureSource;

    // NeuralPlanes carries the photo-to-PBR PLANE product — an admitted Raster/set#TEXTURE_SET assembled from the
    // neural#MODEL_REGISTRY stage plan's outputs, beside the Seq<StageResult> evidence that produced it — and what SURVIVES
    // separates it from SvbrdfMap: SvbrdfMap collapses a per-texel parameter field to one row and discards it, while this arm
    // keeps the planes, so the acquired material stays spatially varying and the averaged row serves as the SUMMARY the seam
    // AppearanceSummary and the LOD fallback read, not the whole result. Fallback supplies the caller's own base row for every
    // column the set does not carry, so the arm names no library key and hardcodes no neutral.
    public sealed record NeuralPlanes(TextureSet Planes, Seq<StageResult> Stages, MaterialParameters Fallback) : CaptureSource;
}

// What an import PRODUCES: the parameter row every arm yields, its measured receipt, and — for a plane-bearing
// capture — the admitted set itself. The set is Option-typed rather than a fourth arm on a widened tuple because
// three of the four capture arms genuinely produce no planes, and a caller reading Planes.IsSome learns exactly
// whether a shadeable spatially-varying material came back or only its summary row.
public sealed record AcquiredMaterial(MaterialParameters Row, Provenance Provenance, Option<TextureSet> Planes) {
    public static AcquiredMaterial Summary(MaterialParameters row, Provenance provenance) => new(row, provenance, None);
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
    public static Fin<AcquiredMaterial> Import(CaptureSource source, Provenance provenance, Op key, Option<CaptureCalibration> calibration = default) =>
        source.Switch(
            state: (provenance, key),
            measuredBrdf:        static (s, c) => FitBrdf(c.Samples, c.Ior, c.Conductor, s.provenance, s.key),
            svbrdfMap:           static (s, c) => AverageField(c.Texels, s.provenance, s.key),
            spectralReflectance: static (s, c) => GroundSpectral(c.Reflectance.ToSpd(), c.Metalness, c.Roughness, s.key)
                                                    .Map(row => AcquiredMaterial.Summary(row, s.provenance with { Method = CaptureMethod.Spectrophotometer })),
            neuralPlanes:        static (s, c) => ImportPlanes(c.Planes, c.Stages, c.Fallback, s.provenance, s.key))
        .Bind(acquired => Calibrate(acquired, calibration, key))
        .Bind(acquired => MaterialParameters.Of(acquired.Row, key).Map(row => acquired with { Row = row, Provenance = acquired.Provenance with {
            DominantWavelengthNm = row.BaseColor.DominantWavelength,
            ExcitationPurity = row.BaseColor.ExcitationPurity,
            CctKelvin = row.BaseColor.Temperature.Cct,
            CctDuv = row.BaseColor.Temperature.Duv } }));

    // Calibrate sits at the ONE pre-admission site every arm crosses rather than inside the neural arm
    // alone: a photograph feeds the SVBRDF field and the plane set alike, a goniophotometer capture simply passes
    // None, and a per-arm copy would drift the moment a fifth capture modality lands. The solve runs in AP1-linear —
    // Macbeth publishes under sRGB/D50, so ConvertToConfiguration rebases BOTH sides onto the scene-linear working
    // space FIRST (the same colorimetric boundary graph#MATERIAL_LIBRARY Named enforces) and the matrix is then a
    // correction within one space rather than a fit that silently absorbs a chromatic adaptation.
    static Fin<AcquiredMaterial> Calibrate(AcquiredMaterial acquired, Option<CaptureCalibration> calibration, Op key) =>
        calibration.Match(
            None: () => Fin.Succ(acquired),
            Some: chart => Solve(chart, key).Map(fit => acquired with {
                Row = acquired.Row with { BaseColor = Correct(acquired.Row.BaseColor, fit.Matrix) },
                Provenance = acquired.Provenance with { Calibrated = true, CalibrationDeltaE = Some(fit.DeltaE) } }));

    // Least squares over the twenty-four patch rows: the measured AP1 triples are the design matrix, the references
    // the multi-right-hand-side, and one thin-QR solve returns the 3x3 the whole capture is multiplied by — the SAME
    // dense-overdetermined route SolveGgx composes, never a second numeric owner. The residual is the mean CIEDE2000
    // the corrected patches still carry, which is the number that separates a usable chart shot from a hopeful one.
    static Fin<(Matrix<double> Matrix, double DeltaE)> Solve(CaptureCalibration chart, Op key) {
        Seq<Unicolour> reference = toSeq(Macbeth.All).Map(static patch => patch.ConvertToConfiguration(PortValue.SceneLinear));
        Matrix<double> measured = Matrix<double>.Build.Dense(chart.Patches.Count, 3, (row, band) => Band(chart.Patches[row], band));
        Matrix<double> target = Matrix<double>.Build.Dense(reference.Count, 3, (row, band) => Band(reference[row], band));
        Matrix<double> fit = measured.QR(QRMethod.Thin).Solve(target);
        return fit.Enumerate().All(double.IsFinite)
            ? Fin.Succ((fit, chart.Patches
                .Map((patch, index) => Correct(patch, fit).Difference(reference[index], DeltaE.Ciede2000))
                .Fold(0.0, static (sum, delta) => sum + delta) / chart.Patches.Count))
            : MaterialFault.Parameter(key, "<capture-calibration-degenerate>");
    }

    static double Band(Unicolour colour, int index) =>
        colour.RgbLinear.Triplet switch { var t => index is 0 ? t.First : index is 1 ? t.Second : t.Third };

    static Unicolour Correct(Unicolour colour, Matrix<double> fit) =>
        colour.RgbLinear.Triplet switch {
            var t => Linear(
                (t.First * fit[0, 0]) + (t.Second * fit[1, 0]) + (t.Third * fit[2, 0]),
                (t.First * fit[0, 1]) + (t.Second * fit[1, 1]) + (t.Third * fit[2, 1]),
                (t.First * fit[0, 2]) + (t.Second * fit[1, 2]) + (t.Third * fit[2, 2])),
        };

    // Freeze/Thaw give the ONE measured product this estate produces and could not persist a durable container: a
    // wavelength-sampled EXR part, one texel wide, whose channel names ARE the sampling grid. A RasterFormat row is
    // the WRONG home — a wavelength grid is not a texture plane, and admitting it as one would put a spectrum through
    // extent, transfer, and mip gates that describe pixels. Provenance.WavelengthCount becomes the round-trip
    // WITNESS rather than a bare count: a thaw whose decoded grid disagrees with the receipt refuses.
    public static Fin<ReadOnlyMemory<byte>> Freeze(SpectralCurve curve, Op key) {
        float[] wavelengths = [.. Enumerable.Range(0, curve.Coefficients.Length).Select(i => (float)curve.WavelengthAt(i))];
        float[] samples = [.. curve.Coefficients.ToArray().Select(static v => (float)v)];
        WriterResult<byte[]> written = ExrFile.SaveToMemory(
            new TinyEXR.V3.Image([Spectral.CreateReflectivePart(width: 1, height: 1, wavelengths, samples, units: "nm")]),
            Compression.ZIP, options: null);
        return written is { IsSuccess: true, Value: { } bytes }
            ? Fin.Succ((ReadOnlyMemory<byte>)bytes)
            : MaterialFault.Parameter(key, $"<spectral-freeze:{written.Status}>");
    }

    public static Fin<SpectralCurve> Thaw(ReadOnlyMemory<byte> payload, int wavelengthCount, Op key) {
        ReaderResult<TinyEXR.V3.Image> read = ExrFile.LoadFromMemory(payload, options: null);
        return read is { IsSuccess: true, Value: { } file } && toSeq(file.Parts).Head.Case is Part part && Spectral.IsSpectral(part.Header)
            ? Spectral.GetWavelengths(part.Header) is { Length: > 1 } grid && grid.Length == wavelengthCount
                ? SpectralCurve.Of((int)grid[0], (int)Math.Round(grid[1] - grid[0]), Samples(part, grid), key)
                : MaterialFault.Parameter(key, $"<spectral-thaw-grid:{wavelengthCount}>")
            : MaterialFault.Parameter(key, $"<spectral-thaw:{read.Status}>");
    }

    // One channel per wavelength, each holding this one-texel part's single sample — the planar layout
    // CreateReflectivePart wrote, read back through the channel name the same helper minted.
    static ReadOnlyMemory<double> Samples(Part part, float[] grid) =>
        part.GetLevel(0, 0) switch {
            var level => (ReadOnlyMemory<double>)grid
                .Select(nm => (double)MemoryMarshal.Cast<byte, float>(level.GetChannel(Spectral.GetChannelName(SpectrumType.Reflective, nm, 0)).Data.Span)[0])
                .ToArray(),
        };

    // ImportPlanes closes the round trip photo → stages → planes → SHADEABLE MATERIAL: the admitted set binds through the ONE
    // Raster/set#SET_BIND Average target, whose measured per-channel fold over each plane's base level is the summary the seam
    // key carries — never a pyramid tail, never a re-derivation here — and the set rides back beside it so the caller keeps the
    // spatially-varying material rather than only its mean.
    static Fin<AcquiredMaterial> ImportPlanes(TextureSet planes, Seq<StageResult> stages, MaterialParameters fallback, Provenance provenance, Op key) =>
        stages.IsEmpty
            ? MaterialFault.Parameter(key, "<neural-planes-no-stage-evidence>")
            : from binding in SetBind.Bind(planes, fallback, BindTarget.Summary, key)
              from row in binding is SetBinding.Row bound
                  ? Fin.Succ(bound.Parameters)
                  : Fin.Fail<MaterialParameters>(MaterialFault.Graph(key, "<neural-planes-summary-not-a-row>"))
              select new AcquiredMaterial(row, Attributed(provenance, planes, stages), Some(planes));

    // Attribution over a MULTI-MODEL plan: the receipt carries the MOST RESTRICTIVE card in the chain, never the
    // last or the largest, because the grant a consumer must honour is the strictest one any contributing model
    // imposes — and the residual is likewise the WORST golden delta, since a set is only as trustworthy as its
    // weakest stage. Tiles inferred fill the sample count the goniophotometer arm fills with angular samples: both
    // are the count of measurement units the receipt's own instrument produced. A stage whose card left the
    // registry contributes its result without an attribution rather than blocking the import — the plan already
    // gated the grant at request construction, and this column is evidence rather than a second gate. The device
    // column carries the SET key, so a receipt names the exact planes it summarizes without a second identity field.
    static Provenance Attributed(Provenance provenance, TextureSet planes, Seq<StageResult> stages) {
        Option<ModelCard> governing = stages
            .Choose(static result => ModelRegistry.Rows.TryGetValue(result.ModelCardId, out ModelCard? card) ? Some(card!) : Option<ModelCard>.None)
            .Fold(Option<ModelCard>.None, static (strictest, card) => strictest.Filter(held => held.License.Rank >= card.License.Rank).IsSome ? strictest : Some(card));
        return Provenance.Of(CaptureMethod.NeuralPlanes, planes.Digest.ToValue(), provenance.WavelengthCount,
                angularSamples: stages.Fold(0, static (tiles, result) => tiles + result.TilesEmitted),
                fitResidual: stages.Fold(0.0, static (worst, result) => Math.Max(worst, result.GoldenDelta)))
            with { ModelCard = governing.Map(static card => card.Id), License = governing.Map(static card => card.License),
                   ModelArtefact = governing.Bind(static card => card.Artefact) };
    }

    // SyntheticGrid mints the deterministic capture the benchmark corpus pins (BenchInput.Synthetic → GgxFit): geometry is a
    // ⌈√count⌉-lane stratified (θi, θo) grid with Δφ swept uniformly over [0, π) so alphaX ≠ alphaY stays observable,
    // each zenith jittered inside its cell by a SplitMix64 stream off the seed; the reflectance column is the
    // kernel-composed dielectric forward model at seed-derived ground-truth alphas, so the fit workload has a known
    // answer and a byte-stable capture — no fixture file, no RNG state outside (seed, count).
    public static Fin<Seq<BrdfSample>> SyntheticGrid(int seed, int count, Op key) {
        if (count < 1) { return MaterialFault.Parameter(key, $"<synthetic-grid-count:{count}>"); }
        var (alphaX, alphaY) = (0.1 + 0.4 * Draw(seed, 0), 0.1 + 0.4 * Draw(seed, 1));
        double rotation = Draw(seed, 2) * double.Pi;                                   // ground-truth grain azimuth the fit must recover
        // ⌈√count⌉ lanes: count ≤ lanes² makes index / lanes < lanes by construction, so the row index needs no
        // modulo wrap — the floor-lane form aliased the overflow row onto row 0 and double-filled one stratum.
        int lanes = int.Max((int)double.Ceiling(double.Sqrt(count)), 1);
        return Fin.Succ(toSeq(Enumerable.Range(0, count).Select(index => {
            double thetaI = (index / lanes + Draw(seed, 2L * index + 3L)) / lanes * (double.Pi / 2.0 - 0.02);
            double thetaO = (index % lanes + Draw(seed, 2L * index + 4L)) / lanes * (double.Pi / 2.0 - 0.02);
            double deltaPhi = (index + 0.5) / count * double.Pi;
            var sample = new BrdfSample(thetaI, thetaO, deltaPhi, RgbSpectrum.Black);
            return sample with { Reflectance = RgbSpectrum.White.Scale(Reflectance(sample, alphaX, alphaY, rotation, eta: 1.5, conductor: None)) };
        })));
    }

    // Draw defers to the KERNEL — Rasm.Domain Deterministic owns the one splitmix64 stream, and its lane fold
    // mixes each lane into the state where a local multiply-then-use folded them by one multiply and lost the
    // finalizer's avalanche. The seed rides the dedicated seed argument rather than a leading lane, so a full
    // 64-bit policy seed stays one value and two captures never collide by re-packing seed and lane into one span.
    static double Draw(int seed, long lane) => Deterministic.Unit(lanes: [lane], seed: seed);

    // SolveGgx fits overdetermined GGX/Smith/Fresnel least squares through a thin-QR Gauss-Newton iteration over the angular
    // reflectance design matrix (the m measured BrdfSample geometries the rows, the bounded parameter vector the
    // n ∈ {3,4} unknowns — (alphaX, alphaY, phi) under a supplied conductor IOR, (alphaX, alphaY, phi, eta) for the dielectric —
    // Acquisition.Reflectance the bsdf#MICROFACET_KERNEL forward model) — the Rasm.Compute/Tensor/blas#DENSE_ALGEBRA
    // dense-overdetermined route shape, never a hand-rolled Levenberg-Marquardt loop. Fitted alphas project to the
    // Disney Roughness/Anisotropy row columns; the recomputed true residual and the converged-Jacobian conditioning
    // ride the goniophotometer-stamped receipt bsdf#BSDF_GOLDEN gates and interchange#MATERIAL_WIRE
    // WireProvenance carries, real measured evidence vs an authored guess. Provenance rides ALONGSIDE the row
    // (the finish#FINISH Resolve precedent), never a phantom column on MaterialParameters, whose positional core is closed and whose every later axis is init-defaulted enrichment.
    static Fin<AcquiredMaterial> FitBrdf(Seq<BrdfSample> samples, double ior, Option<ComplexIor> conductor, Provenance provenance, Op key) =>
        guard(samples.Count > (conductor.IsSome ? 3 : 4), MaterialFault.Parameter(key, $"<measured-brdf-underdetermined:{samples.Count}>")).ToFin()
            .Bind(_ => SolveGgx(samples, ior, conductor, key))
            .Bind(fit => SpectralUpsample.ToSpd(
                    samples.Map(static s => s.Reflectance).Fold(RgbSpectrum.Black, static (acc, r) => acc.Add(r)).Scale(1.0 / samples.Count), key)
                .Bind(spd => GroundSpectral(spd, metalness: conductor.IsSome ? 1.0 : 0.0, fit.Roughness, key))
                .Map(row => AcquiredMaterial.Summary(
                             row with { Ior = fit.Eta, Anisotropy = fit.Anisotropy, AnisotropyRotation = fit.Rotation },
                             Provenance.Of(CaptureMethod.Goniophotometer, provenance.Device, provenance.WavelengthCount, samples.Count, fit.Residual)
                                 with { FitConditionNumber = fit.ConditionNumber, FitRank = fit.Rank })));

    // BrdfFit carries the fitted Disney projection — magnitude AND direction — the witnessed relative residual, and
    // the Svd conditioning that says whether the direction was observable at all.
    readonly record struct BrdfFit(double Roughness, double Anisotropy, double Rotation, double Eta, double Residual, double ConditionNumber, int Rank);

    static Fin<BrdfFit> SolveGgx(Seq<BrdfSample> samples, double ior0, Option<ComplexIor> conductor, Op key) {
        int m = samples.Count;
        int n = conductor.IsSome ? 3 : 4;   // columns: [alphaX, alphaY, phi(, eta)] — the conductor Fresnel is measured, not fit
        double[] p = n == 3
            ? [Microfacet.AlphaOf(0.3), Microfacet.AlphaOf(0.3), 0.0]
            : [Microfacet.AlphaOf(0.3), Microfacet.AlphaOf(0.3), 0.0, Math.Clamp(ior0, 1.0, 2.5)];   // seed alpha = roughness²
        // phi bounds span a HALF turn: an ellipse is symmetric under a half rotation, so [0, pi) is the whole
        // observable domain and a [0, 2pi) box would carry two minima the solver could oscillate between.
        (double Lo, double Hi)[] bounds = n == 3
            ? [(1e-4, 1.0), (1e-4, 1.0), (0.0, Math.PI)]
            : [(1e-4, 1.0), (1e-4, 1.0), (0.0, Math.PI), (1.0, 2.5)];
        double[] logMeasured = samples.Map(static s => Math.Log(Math.Max(1e-6, s.Reflectance.Luminance))).ToArray();
        Vector<double> measured = Vector<double>.Build.DenseOfArray(logMeasured);
        double residual = double.MaxValue;
        for (int iter = 0; iter < 12; iter++) {
            Vector<double> r = Vector<double>.Build.Dense(m, i => logMeasured[i] - Math.Log(Model(samples[i], p, conductor)));
            Matrix<double> j = Matrix<double>.Build.Dense(m, n, (i, c) => Jacobian(samples[i], p, conductor, c));
            if (!j.Enumerate().All(double.IsFinite) || !r.Enumerate().All(double.IsFinite)) { return MaterialFault.Parameter(key, "<ggx-fit-non-finite-jacobian>"); }
            Vector<double> delta = j.QR(QRMethod.Thin).Solve(-r);                     // Gauss-Newton descent step Δp = -(JᵀJ)⁻¹Jᵀr; J=∂r/∂p so the RHS is -r, never +r (the ascent sign)
            // MathNet can pass its rank gate while a near-rank-deficient Jacobian fills the QR solution with NaN;
            // IsFullRank stays true (the `libs/csharp/.api/api-mathnet-numerics.md` admission law), so probe the solution all-finite
            // and fall back to the Svd(true) truncated pseudo-inverse — the ROUTE_SPINE RankRevealing conditioning fallback.
            if (!delta.Enumerate().All(double.IsFinite)) { delta = j.Svd(true).Solve(-r); }
            for (int c = 0; c < n; c++) { p[c] = Math.Clamp(p[c] + 0.5 * delta[c], bounds[c].Lo, bounds[c].Hi); }
            residual = r.L2Norm() / Math.Max(1e-6, measured.L2Norm());
            if (residual < 1e-4) { break; }
        }
        // SolveGgx RECOMPUTES the witnessed residual at converged p against the original samples (the in-loop value is
        // stale by one update step) — the true relative residual the receipt reports, never a factor-reconstructed proxy.
        residual = Vector<double>.Build.Dense(m, i => logMeasured[i] - Math.Log(Model(samples[i], p, conductor))).L2Norm() / Math.Max(1e-6, measured.L2Norm());
        // Converged conditioning reads ConditionNumber and Rank from an S-only Svd handle (no U/VT) —
        // rank-deficiency (an unobservable column) stamps FitRank < n with +Inf condition, receipt evidence, never silent.
        Svd<double> conditioning = Matrix<double>.Build.Dense(m, n, (i, c) => Jacobian(samples[i], p, conductor, c)).Svd(false);
        return double.IsFinite(residual)
            ? Fin.Succ(new BrdfFit(
                Roughness: Math.Sqrt(Math.Sqrt(p[0] * p[1])),                                  // alpha_geo = √(αx·αy) = roughness²
                Anisotropy: Math.Clamp((1.0 - Math.Min(p[0], p[1]) / Math.Max(p[0], p[1])) / 0.9, 0.0, 1.0),   // inverse Disney aspect² = min/max
                // Rotation projects the fitted azimuth onto the row's UNIT column (1 is a half turn), taken from the
                // ALPHA-X axis: when the fit lands alphaY as the rougher of the pair the rougher axis is a quarter
                // turn away, so the projection adds that quarter rather than reporting the smooth axis as the grain.
                Rotation: Math.Clamp((p[2] + (p[0] >= p[1] ? 0.0 : Math.PI / 2.0)) / Math.PI % 1.0, 0.0, 1.0),
                Eta: n == 4 ? p[3] : 1.5,                                                      // conductor rows keep the coat-side Disney default — Ior is unread at Metalness 1
                Residual: residual, ConditionNumber: conditioning.ConditionNumber, Rank: conditioning.Rank))
            : MaterialFault.Parameter(key, "<ggx-fit-diverged>");
    }

    // Reflectance SINGLE-SOURCES the microfacet forward model on bsdf#MICROFACET_KERNEL: reconstruct the real incident/outgoing
    // LocalVector directions from a sample's (θi, θo, Δφ), form the true half-vector, and read the kernel's anisotropic
    // GGX NDF + Smith height-correlated masking + the discriminant-routed Fresnel — the measured-conductor luminance over
    // ComplexIor, or the dielectric term over eta — NEVER a re-minted D/G/F with a cos((θi−θo)/2) specular-plane
    // half-angle that drops the azimuth (Δφ is what makes alphaX ≠ alphaY observable). alphas are GGX alphas directly
    // (roughness² per the Disney remap); the per-cell evaluation rides the SolveGgx kernel carve-out.
    static double Reflectance(BrdfSample s, double alphaX, double alphaY, double rotation, double eta, Option<ComplexIor> conductor) {
        double si = Math.Sin(s.IncidentZenith), so = Math.Sin(s.OutgoingZenith);
        LocalVector wi = new LocalVector(si, 0.0, Math.Cos(s.IncidentZenith)).Normalize().RotateZ(-rotation);
        LocalVector wo = new LocalVector(so * Math.Cos(s.AzimuthDelta), so * Math.Sin(s.AzimuthDelta), Math.Cos(s.OutgoingZenith)).Normalize().RotateZ(-rotation);
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

    // Model is the vector view shared by residual and Jacobian builds; eta rides p[2] only on the dielectric arm. The
    // conductor arm never reads eta (its Fresnel is the measured ComplexIor), so the placeholder stays the physical
    // 1.5 rather than an impossible 0.0 a re-routed edit would silently integrate.
    static double Model(BrdfSample s, double[] p, Option<ComplexIor> conductor) =>
        Reflectance(s, p[0], p[1], p[2], p.Length == 4 ? p[3] : 1.5, conductor);

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

    // AverageField computes the REAL per-texel SVBRDF mean — the neural fit's spatially-varying field collapsed to ONE representative
    // row in a single fold pass (never a texels.Head first-row read, never 17 LINQ traversals): FieldSum accumulates the
    // scene-linear base/emission channels and every Disney scalar + SubsurfaceRadius band, then Mean divides once. The
    // base/emission average runs in RgbLinear (the shading-truth channel) so two texels' colors blend physically, not in
    // a display-encoded space. Averaged rows re-admit through MaterialParameters.Of (the Import Bind), so a degenerate
    // mean rails the row gate rather than passing a bad scatter band; the neural-svbrdf method + texel count stamp the receipt.
    static Fin<AcquiredMaterial> AverageField(Seq<MaterialParameters> texels, Provenance provenance, Op key) =>
        texels.IsEmpty
            ? MaterialFault.Parameter(key, "<svbrdf-map-empty>")
            : Fin.Succ(AcquiredMaterial.Summary(
                        texels.Fold(FieldSum.Zero, static (acc, t) => acc.Add(t)).Mean(texels.Count),
                        Provenance.Of(CaptureMethod.NeuralSvbrdf, provenance.Device, provenance.WavelengthCount, texels.Count, provenance.FitResidual)));

    // FieldSum accumulates in one pass: the six scene-linear base/emission channels (read once through RgbLinear), the
    // twelve Disney scalars with EmissionLuminance, and the 3 SubsurfaceRadius bands, summed in one fold and divided in
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

    // GroundSpectral resolves measured Spd to a scene-linear Acescg base color through the ONE
    // PortValue.SceneLinear working space (a re-minted inline Configuration forks the Unicolour lazy-conversion cache).
    // Gamut admission rejects FIRST (an out-of-gamut capture faults, never a silent chroma reduction), then the SceneLinear
    // grounding is CONSUMED — the finite-gated grounded triple IS the row base color, never a discarded validation binding.
    // Named-arg construction over the closed positional core makes a column reorder break loudly at compile time; every init-defaulted enrichment column takes its own default, which is what a spectrophotometer capture genuinely measures.
    static Fin<MaterialParameters> GroundSpectral(Spd reflectance, double metalness, double roughness, Op key) =>
        from color in Fin.Succ(new Unicolour(PortValue.SceneLinear, reflectance))
        from _ in guard(GamutPolicy.Perceptual.Contains(color), MaterialFault.Gamut(key, "<acquired-color-out-of-gamut>"))
        from grounded in SpectralUpsample.SceneLinear(color, key)
        select new MaterialParameters(
            BaseColor: Linear(grounded.R, grounded.G, grounded.B), Metalness: metalness, Roughness: roughness, SpecularTint: 0.0, Anisotropy: 0.0, Ior: 1.5,
            Transmission: 0.0, TransmissionRoughness: 0.0, Sheen: 0.0, SheenTint: 0.0, Clearcoat: 0.0, ClearcoatRoughness: 0.0,
            Subsurface: 0.0, SubsurfaceRadius: SubsurfaceRadius.None, Emission: Linear(0.0, 0.0, 0.0), EmissionLuminance: 0.0);

    // Linear is the one scene-linear Unicolour constructor — graph#MATERIAL_GRAPH PortValue.SceneLinear (the one
    // Acescg working space), so every base/emission color this page mints reads the same scene-linear channel basis the
    // library rows do, never a second ColourSpace wrapper or a re-minted Configuration.
    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
}
```

## [03]-[RESEARCH]

- [EPFL_RGL_BRDF_LOADER]-[BLOCKED]: Which managed reader admits the 195-wavelength EPFL RGL `.bsdf` header, theta/phi parameterization, and Rough-Quadtree spectral payload into the `Seq<BrdfSample>` the `MeasuredBrdf` arm fits and the per-band `SpectralCurve` the `SpectralReflectance` arm grounds through `surface#SPECTRAL_UPSAMPLE` `ToSpd` onto the `surface#CONDUCTOR_IOR` conductor and dielectric rows?; verify an admitted managed reader or bound `brdf-loader` at the app-root import boundary. `Acquisition.Import` is the one ingest path both consumers reach through, so this row holds the question for both. `SpectralCurve` already gives the decoded payload its durable home, carrying the wavelength grid `Freeze`/`Thaw` persist, so the READER alone remains blocking.

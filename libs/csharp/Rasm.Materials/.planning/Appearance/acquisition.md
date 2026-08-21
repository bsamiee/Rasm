# [MATERIALS_ACQUISITION]

`Acquisition.Import` folds the closed measured-BRDF, SVBRDF-map, spectral-reflectance, gloss-meter, and neural-plane `CaptureSource` family into the `AcquiredMaterial` product — the `MaterialParameters` row the material library registers and the lobe family shades, its `CaptureProvenance` receipt, and, for a plane-bearing capture, the admitted `Raster/set#TEXTURE_SET` `TextureSet` itself, so a photo-to-PBR import returns a spatially-varying shadeable material rather than only the mean the seam key carries. Capture fitting composes the `bsdf#MICROFACET_KERNEL` GGX/Smith/Fresnel kernel over reconstructed incident and outgoing directions, hands the anisotropic conductor-or-dielectric parameter vector to the kernel `Lm.Minimize` functor, grounds spectral colour through the one `surface#SPECTRAL_UPSAMPLE` owner, and gates the admitted row through the gamut and `MaterialFault` rails. `CaptureProvenance` is the MEASUREMENT receipt — instrument, counts, residual, conditioning, chromaticity, chart calibration, model attribution, third-party custody — distinct by construction from the seam `Rasm.Element` `PropertyEvidence` CITATION carrier, which models source, reference, expiry, and grade and holds no column any of those facts could land on. Per-capture acquired-material types and private BRDF kernels are deleted forms.

## [01]-[INDEX]

- [02]-[ACQUISITION]: `CaptureSource` closes the `[Union]` capture family, `BrdfSample` records angular reflectance, `AcquiredMaterial` carries the import product, `CaptureMethod` keys the instrument, and `CaptureProvenance` carries the measurement receipt; `Acquisition.Import` composes the generic forward model, the kernel `Lm.Minimize` fit, the per-texel SVBRDF average, the gloss-meter reading, the neural-plane `SetBind` summary, the chart-solved colour calibration every arm crosses, the `Freeze`/`Thaw` spectral-EXR round trip, and the `BrdfArchive` EPFL RGL tensor-container reader over its `CaptureField`/`TensorDtype` vocabularies.

## [02]-[ACQUISITION]

- Owner: `Acquisition` the static import fold; `CaptureSource` `[Union]` the closed capture family; `AcquiredMaterial` the one import product; `BrdfSample` the goniophotometer angular record over `(θi, θo, Δφ)`; `CaptureMethod` `[SmartEnum<string>]` the instrument discriminant; `CaptureProvenance` the measurement receipt; `SpectralCurve` the durable sampled-spectrum carrier with its scale-invariant `LuminousEfficacy` fold; `SpectralKind` the reflective/emissive claim binding its own container part factory; `GlossCurve` the caller-supplied GU-to-roughness calibration row; `BrdfArchive` the admitted EPFL RGL tensor container over its `CaptureField`/`TensorDtype` vocabularies; `CaptureCalibration` the twenty-four measured chart patches.
- Cases: capture {`MeasuredBrdf` (angular samples, the dielectric IOR seed, and the Fresnel discriminant — `Some` conductor fixes a measured `ComplexIor` so only `(αx, αy, φ)` fit, `None` fits the dielectric `(αx, αy, φ, η)`), `SvbrdfMap` (a per-texel field collapsed to one row and discarded), `SpectralReflectance` (a `SpectralCurve` with its metalness/roughness — the carrier that PERSISTS, where an `Spd` cannot answer what grid built it), `GlossMeter` (the tri-angle 20/60/85 GU triple over the caller's base row), `NeuralPlanes` (an admitted `TextureSet` with its `Seq<StageResult>` evidence and a fallback row — the planes SURVIVE)}. `CaptureMethod` admits every instrument as one ROW (goniophotometer, spectrophotometer, neural-SVBRDF, neural-planes, gloss-meter, the `finish#FINISH` Kubelka-Munk pigment mix, the authored sentinel), never a per-instrument capture type.
- Entry: `public static Fin<AcquiredMaterial> Import(CaptureSource source, CaptureProvenance provenance, Context context, Op key, Option<CaptureCalibration> calibration = default)` — the ONE import path. `Context` carries the SOLVER LADDER: `SolvePolicy.Of(context, key)` derives the damped Gauss-Newton residual, step, and iteration bands off the model context, so the fit's convergence gates are a project's to tighten and never this page's constants. `CaptureCalibration` carries the one axis no capture value holds, applied at the single pre-admission site every arm crosses.
- Entry: each arm produces `(row, receipt)`: `MeasuredBrdf` runs the kernel functor over `BrdfResidual`, projects onto the Disney `Roughness`/`Anisotropy`/`AnisotropyRotation` columns, and stamps the goniophotometer receipt with the witnessed residual and the observability witness; `SpectralReflectance` grounds through `surface#SPECTRAL_UPSAMPLE`; `SvbrdfMap` folds a REAL per-column mean; `GlossMeter` writes roughness ALONE; `NeuralPlanes` binds through the ONE `Raster/set#SET_BIND` `BindTarget.Summary` fold and returns the set beside the row, stamping the set digest as its device, the summed tiles as its sample count, the WORST stage golden delta as its residual, and the MOST RESTRICTIVE contributing card as its attribution.
- Entry: `SyntheticGrid(seed, count, key)` rails the deterministic stratified capture the benchmark corpus pins — geometry, ground-truth alphas, and a ground-truth grain azimuth all derived from the seed through the KERNEL's one lane-keyed draw, reflectance the kernel forward model at those parameters, so the fit workload has a known answer in direction as well as magnitude and no fixture file exists. `BrdfArchive.Of(payload, key)` admits an EPFL RGL container and `Lower` turns it into the `Seq<BrdfSample>` the fit consumes and the per-band `SpectralCurve` the grounding does. `Freeze(curve, kind, key)`/`Thaw(payload, wavelengthCount, key)` are the durable pair a measured spectrum persists through, the receipt's own `WavelengthCount` the round-trip witness.
- Packages: Wacton.Unicolour (composed — `new Unicolour(PortValue.SceneLinear, Spd)` grounding, `RgbLinear.Triplet` channel reads, `IsInRgbGamut`, and the `DominantWavelength`/`ExcitationPurity`/`Temperature` chromaticity readout the receipt stamps), Wacton.Unicolour.Datasets (`Macbeth.All`, the 24-patch reference the chart solve targets), MathNet.Numerics (composed for the LINEAR chart solve ALONE — `Matrix<double>.Build.Dense`, `QR(QRMethod.Thin)`, `QR<double>.Solve`, `Control.UseManaged` the osx-arm64 provider), Rasm (project — `Op`, `Context`, `Deterministic`, and `Rasm.Solving` `Lm.Minimize`/`IDualResidual`/`DualModel`/`Dual<T>`/`SolvePolicy`), Rasm.Element (`ContentAddress`), `Rasm.Materials.Raster` (`TextureSet`, `SetBind.Bind`, `IngestProvenance`), `neural#MODEL_REGISTRY` (`StageResult`, `ModelRegistry.Rows`, `ModelCard`), TinyEXR.NET (the durable spectral container), DoubleDouble, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new capture modality is one `CaptureSource` case carrying its fit arm — never a per-capture material, a second import owner, or an `ImportPlanes`/`ImportBrdf` entry pair; a new instrument is one `CaptureMethod` row; a new fit parameter is one `(Lo, Hi)` row on `BrdfResidual.Bounds` projected onto a column `MaterialParameters` already carries, since `Dof` reads the bounds length, the box map is column-generic, and the Jacobian derives itself; a new receipt fact is one init-defaulted `CaptureProvenance` column the generated `interchange#MATERIAL_WIRE` mapper then COMPELS onto a wire mirror or an explicit ignore at build time; a new tensor field is one `CaptureField` row carrying the RANK its payload must hold.
- Law: this page carves out FIVE measured kernels and nothing else — `Reflectance` (a fixed-width geometry evaluation), the chart least-squares, the `Freeze`/`Thaw` container legs, `SpectralCurve.LuminousEfficacy`, and the archive's extent walk — the same boundary-numeric carve `surface#SPECTRAL_UPSAMPLE` `ToCurve` names. Every other operation is expression-bodied and rail-threaded. Deleted whole: the hand Gauss-Newton loop, its half-step damping, its thin-`QR` step, its truncated-pseudo-inverse fallback, and its central-difference Jacobian — each re-derived what the kernel functor owns, and its differencing step size was a number this page had no basis to choose.
- Law: the forward model is SINGLE-SOURCED on `bsdf#MICROFACET_KERNEL` and GENERIC over its scalar, which is what makes the single-sourcing reachable — the synthetic capture evaluates the body at `double` and the residual row evaluates the SAME body at the solver's dual scalar, so the fit differentiates exactly what it predicts and no dual transcription exists to drift. `Reflectance` reconstructs `wi`/`wo` from `(θi, θo, Δφ)`, rotates both by `−φ` about local Z so the fitted grain axis IS the basis the anisotropic terms read (the rotate-the-lobe form `bsdf#LOBE_FAMILY` shades through), forms the true half-vector, and reads `Ndf` · `MaskingShadowing` · the discriminant-routed Fresnel over `4·|cosθi|·|cosθo|`. The deleted form is a private `D`/`G`/`F` with a `cos((θi−θo)/2)` half-angle that drops the azimuth — and Δφ is exactly what makes `αx ≠ αy` observable.
- Law: bounds ride a DIFFERENTIABLE box reparameterization `p = Lo + (Hi − Lo)·σ(u)`, never a per-iteration clamp: every iterate is interior by construction and the map is smooth everywhere, where a clamp has no derivative at its own boundary and reads a zero gradient exactly where the box binds — precisely where an in-plane capture's `αy` sits. `φ` spans a HALF turn, because an ellipse is symmetric under a half rotation and a `[0, 2π)` box carries two minima the ladder oscillates between.
- Law: OBSERVABILITY reads the CAPTURE GEOMETRY, never a second factorization of the converged Jacobian. A capture whose relative-azimuth spread is degenerate can measure neither the second alpha nor the grain lying along it, so it reports `FitRank < Dof` with `+Inf` conditioning and NAMES ITS OWN CAUSE, where a condition number could only say something was ill-posed. That witness costs one pass and survives the ladder's regularization — LM converges a deficient system to SOME answer, and a reader needs the reason that answer is not evidence.
- Law: the fitted parameters project as `Roughness = (αx·αy)^¼`, `Anisotropy = (1 − min/max)/0.9`, and `AnisotropyRotation` the fitted azimuth of the `αx` axis on the row's unit convention — the FULL inverse of the Disney aspect remap, so a brushed-metal capture round-trips its grain DIRECTION as well as its magnitude. The azimuth was always IN the residual surface; only the projection threw it away.
- Law: COLOUR CALIBRATION carries the rigour the receipt's own vocabulary claims. A neural arm stamps MEASURED on a base colour derived from a photograph nothing white-balanced, so an optional `CaptureCalibration` solves one 3x3 through a thin-`QR` LINEAR least squares — a linear correction is not a nonlinear fit, so it stays on the dense-algebra route and never reaches for the ladder — and stamps `Calibrated` beside the mean CIEDE2000 residual it left. `Macbeth`'s sRGB/D50 references rebase through `ConvertToConfiguration` FIRST so the solve runs in AP1-linear and corrects WITHIN one space rather than silently absorbing a chromatic adaptation, and it applies at the one pre-admission site every arm crosses rather than inside a single arm a sixth modality would then miss.
- Law: a MEASURED CURVE PERSISTS. Wacton's `Spd` is a one-way XYZ intake republishing no grid, so `SpectralCurve` carries the durable form and `Freeze`/`Thaw` round-trip it through a wavelength-sampled EXR part whose channel names ARE the sampling grid. `SpectralKind` rows the reflective and emissive part factories, so the leg spans BOTH claims and the `photometric#PHOTOMETRIC` measured emission SPD persists exactly as a reflectance does. A polarised part — four Stokes components per wavelength, not one sampled curve — takes NO row and refuses at the thaw. Re-ingest PARSES the container's own channel names rather than re-minting names this page would have written, and its Stokes half is not optional: parsing wavelengths alone flattens four polarisation components of one wavelength onto one channel.
- Boundary: `Acquisition.Import` is the ONE import path and `AcquiredMaterial` its ONE product. Every produced row re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of`, and that ONE re-admission site stamps the grounded base colour's chromaticity onto the receipt for EVERY arm (`CctDuv` carrying the off-locus distance a reader gates `CctKelvin` on). The `SvbrdfMap` arm computes a REAL field average in one fold — the base colour in scene-linear `RgbLinear`, every Disney scalar, and `SubsurfaceRadius` per band — never a `texels.Head` first-row read. `CaptureProvenance` rides ALONGSIDE the row (the `finish#FINISH` `Resolve` precedent), never a phantom column on `MaterialParameters`.
- Boundary: the `NeuralPlanes` arm consumes an ALREADY-ADMITTED `TextureSet` and never assembles one. `Rasm.Compute` executes the stage plan; the app root resolves each `StageResult.Planes` channel output — the FROZEN prior-drop projection `neural#STAGE_PLAN` owns, so a `delit` or `depth` `PriorField` reaches no set — through `Raster/codec#RASTER_CODEC` and admits the bundle through `TextureSet.Of`. This owner binds what admission already proved, so an inference product passes the same extent, transfer, convention, and payload gates a pressed set passes; the averaged row comes from the ONE `SetBind` `Average` fold and the set rides back, where the `SvbrdfMap` arm deliberately discards its field.
- Boundary: the EPFL RGL `.bsdf` decode is HAND-AUTHORED because no managed reader exists and the reference implementation's licence permits clean transcription. `BrdfArchive.Of` admits by magic word, gates each `CaptureField` row's payload against the RANK the ROW declares (never a positional roster a producer could reorder), and reads element widths off `TensorDtype` so an unadmitted dtype names itself at the field gate rather than at a cast. Two facts are DERIVED, never declared: ISOTROPY is `phi_i.Length <= 2`, since an isotropic measurement carries at most two incident-azimuth slices and a flag beside that would be a second truth; the WAVELENGTH COUNT reads off the `wavelengths` field, since published materials sample from a handful of bands to nearly two hundred and a constant admits one dataset while mis-striding every other.
- Boundary: the neural-SVBRDF `.exr` map stays the host-edge import boundary the app root owns, this owner consuming its decoded portable data; the managed MathNet provider is selected ONCE through `Control.UseManaged` (osx-arm64 has no native MKL/OpenBLAS, and a per-call-site `TryUseNativeMKL` is the named defect); the spectral grounding composes the ONE `PortValue.SceneLinear` working space, never a re-minted inline `Configuration` and never a local `SceneConfig` alias; a `codec#RASTER_FORMAT` row for a spectral part stays UNMINTED, because a wavelength grid is not a texture plane and admitting it as one puts a spectrum through extent, transfer, and mip gates that describe pixels; a malformed, empty, or out-of-gamut capture rails `MaterialFault`, never a sentinel row.
- Boundary: `CaptureProvenance` carries the MEASUREMENT half of custody and composes the seam shapes for the rest — third-party custody rides the `Raster/set#SET_INGEST` `IngestProvenance` shape ITSELF rather than a second source/licence/reference spelling, so a capture's custody and an ingested set's are ONE question read at two grains. It does NOT delete onto the seam `Rasm.Element` `PropertyEvidence`: that carrier models CITATION (source, reference, expiry, grade, attestation, run) and holds no column for an instrument, a sample count, a fit residual, a conditioning witness, a chromaticity readout, a chart delta, or a model attribution. Two carriers, two questions, one composed shape where they meet.

```csharp signature
// --- [RUNTIME_PRELUDE] ---------------------------------------------------------------------
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using DoubleDouble;
using LanguageExt;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Materials.Appearance.Bsdf;
using Rasm.Materials.Appearance.Graph;
using Rasm.Materials.Appearance.Surface;
using Rasm.Materials.Appearance.Texture;
using Rasm.Materials.Raster;
using Rasm.Numerics;
using Rasm.Solving;
using TinyEXR.V3;
using Thinktecture;
using Wacton.Unicolour;
using Wacton.Unicolour.Datasets;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Appearance;

// --- [TYPES] -------------------------------------------------------------------------------
// A Kubelka-Munk pigment mix is a MEASURED-pigment finish (its pigments carry measured K/S reflectance), so it
// stamps PigmentMix and reads as measured (!= Authored), never the authored sentinel.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CaptureMethod {
    public static readonly CaptureMethod Goniophotometer = new("goniophotometer");
    public static readonly CaptureMethod Spectrophotometer = new("spectrophotometer");
    public static readonly CaptureMethod NeuralSvbrdf = new("neural-svbrdf");
    public static readonly CaptureMethod NeuralPlanes = new("neural-planes");
    public static readonly CaptureMethod GlossMeter = new("gloss-meter");
    public static readonly CaptureMethod PigmentMix = new("pigment-mix");
    public static readonly CaptureMethod Authored = new("authored");
}

// A reflectance and an emission spectrum are the SAME sampled shape under different physical claims, and the
// container records that claim in its own channel-name prefix. A reflective-only Freeze left the estate's other
// measured spectrum with no durable form at all while the emissive part sat catalogued and unconsumed, which is the gap the second row closes.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpectralKind {
    public static readonly SpectralKind Reflectance = new("reflectance", SpectrumType.Reflective, Spectral.CreateReflectivePart);
    public static readonly SpectralKind Emission = new("emission", SpectrumType.Emissive, Spectral.CreateEmissivePart);

    public SpectrumType Container { get; }

    [UseDelegateFromConstructor]
    public partial Part Part(int width, int height, ReadOnlySpan<float> wavelengths, ReadOnlySpan<float> samples, string? units, Compression compression);

    public static Option<SpectralKind> Of(SpectrumType container) =>
        toSeq(Items).Find(row => row.Container == container);
}

// --- [MODELS] ------------------------------------------------------------------------------
public readonly record struct BrdfSample(double IncidentZenith, double OutgoingZenith, double AzimuthDelta, RgbSpectrum Reflectance);

// SpectralCurve carries a durable sampled spectrum — start wavelength, sample interval, and the non-negative
// coefficient grid, admitted ONCE at Of so the interior never re-checks and Spd never sees a rejected grid. It
// crosses to Wacton at the point of integration through ToSpd.
public readonly record struct SpectralCurve(int StartNm, int IntervalNm, ReadOnlyMemory<double> Coefficients) {
    public static Fin<SpectralCurve> Of(int startNm, int intervalNm, ReadOnlyMemory<double> coefficients, Op key) =>
        intervalNm is not (0 or 1 or 5)                                                    // the Spd.IsValid interval domain, proven BEFORE construction
            ? new MaterialFault.Parameter(key, $"<spectral-curve-interval:{intervalNm}>")
            : startNm <= 0 || coefficients.Length < 2
                ? new MaterialFault.Parameter(key, $"<spectral-curve-extent:{startNm}@{coefficients.Length}>")
                : coefficients.Span.ContainsAnyInRange(double.NegativeInfinity, -double.Epsilon)
                    ? new MaterialFault.Parameter(key, "<spectral-curve-negative-coefficient>")
                    : Fin.Succ(new SpectralCurve(startNm, intervalNm, coefficients));

    public Spd ToSpd() => new(StartNm, IntervalNm, Coefficients.ToArray());
    public double WavelengthAt(int index) => StartNm + (index * IntervalNm);

    // LuminousEfficacy folds the photopic-band radiant-power fraction the photometric#PHOTOMETRIC luminous divide
    // scales by: ∫V(λ)S(λ)dλ / ∫S(λ)dλ over this curve's OWN sampling grid, which is why it lives on the curve and
    // not on the gate that consumes it. The ratio is SCALE-INVARIANT — both integrals are linear in S, so a common
    // factor cancels — and therefore a RELATIVE captured spectrum answers it exactly; an absolute SPD is NOT
    // required. The absolute premise held only for a tristimulus readout, whose Y the package normalizes against the
    // reference white and which consequently cannot recover the fraction at any scale, and that is exactly why the
    // fold reads these samples rather than crossing a Unicolour conversion to reach them.
    // Exemption: a span fold crosses no lambda, so the accumulation has no expression form.
    public double LuminousEfficacy() {
        ReadOnlySpan<double> samples = Coefficients.Span;
        (double weighted, double total) = (0.0, 0.0);
        for (int i = 0; i < samples.Length; i++) {
            (weighted, total) = (weighted + (Photopic(WavelengthAt(i)) * samples[i]), total + samples[i]);
        }
        // The gate's own contract is (0,1] by physics, so a degenerate all-zero curve reports the declared unity
        // idealization rather than a zero that would divide the luminous coercion by nothing.
        return total > 0.0 ? Math.Clamp(weighted / total, double.Epsilon, 1.0) : 1.0;
    }

    // V(λ) is the CIE photopic luminosity function, DERIVED from its own two-lobe analytic form rather than carried
    // as a transcribed lattice: each lobe is a Gaussian whose width differs either side of its own peak — the
    // asymmetry is what lets two lobes carry a curve a symmetric sum cannot — so the whole weighting is four
    // constants per lobe a reader checks against the definition instead of against a table nothing regenerates.
    // Past the visible band both lobes have already decayed, so a sample outside it contributes nothing and the fold
    // needs no domain gate; the peak evaluates to unity at 555 nm, which is the anchor the 683 lm/W divide assumes.
    static double Photopic(double nm) =>
        (0.821 * Lobe(nm, peak: 568.8, lower: 46.9, upper: 40.5)) + (0.286 * Lobe(nm, peak: 530.9, lower: 16.3, upper: 31.1));

    static double Lobe(double nm, double peak, double lower, double upper) =>
        (nm - peak) / (nm < peak ? lower : upper) switch { var t => Math.Exp(-0.5 * t * t) };
}

// CaptureCalibration holds the twenty-four patch triples a ColorChecker photograph yields, in the SAME order Macbeth.All
// publishes — order IS the correspondence, so a shuffled roster solves a matrix onto the wrong references and no
// residual can detect it, which is why the arity gate names the count rather than trusting a caller's zip.
public readonly record struct CaptureCalibration(Seq<Unicolour> Patches) {
    public static Fin<CaptureCalibration> Of(Seq<Unicolour> patches, Op key) =>
        patches.Count == toSeq(Macbeth.All).Count
            ? Fin.Succ(new CaptureCalibration(patches))
            : new MaterialFault.Parameter(key, $"<capture-calibration-arity:{patches.Count}>");
}

// CaptureProvenance is the measured-capture receipt — a sealed record, never a struct: Method is a reference-typed row and a
// struct `default` would ghost it null past every initializer. Generated interchange#MATERIAL_WIRE
// AppearanceWireMap.ToWire(CaptureProvenance) mirrors Device/WavelengthCount/FitResidual/Method/AngularSamples and the six evidence
// columns below by member name, derives Measured off the CaptureMethod column, and lowers the Option-typed
// attribution pair through its own readers onto the WireProvenance ModelCard/License keys — RequiredMappingStrategy.Both
// makes an unmirrored column a BUILD BREAK, so a receipt field never reaches a peer as silence;
// every later column is init-defaulted enrichment (every existing
// 3-arg construction binds, the authored sentinel and a Kubelka-Munk mix both defaulting Method to Authored). CctDuv gates CctKelvin validity at |Duv| <= 0.05.
public sealed record CaptureProvenance(string Device, int WavelengthCount, double FitResidual) {
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

    // Typed absence is the honest default: an estate-produced goniophotometer capture has no third-party custody to
    // declare, and a row that filled the shape with its own device name would forge provenance out of a measurement it performed itself.
    public Option<IngestProvenance> Ingest { get; init; }

    // ModelArtefact addresses the BYTES the governing card ran: a card names a model and this
    // names the weights, so a receipt can answer which artefact produced a plane rather than only which row
    // authorized one. Optional at BOTH ends by the same reason the neural registry gives — caller-supplied weights
    // are a deployment fact the registry never addresses — so a mandatory column here would forge an address for exactly the rows that have none.
    public Option<ContentAddress> ModelArtefact { get; init; }

    // Measured reads the METHOD, the one column that says what took the measurement — never a whole-record compare
    // against the authored default. Record equality made every enrichment column part of the answer, so a genuine
    // goniophotometer capture whose device, counts, and residual happened to match the default read AUTHORED, and a
    // new receipt column silently changed the predicate. One discriminant, one reading, and the wire mirror composes
    // this member rather than restating the comparison.
    public bool Measured => Method != CaptureMethod.Authored;

    public static readonly CaptureProvenance Authored = new("authored", 0, 0.0);

    public static CaptureProvenance Of(CaptureMethod method, string device, int wavelengthCount, int angularSamples, double fitResidual) =>
        new(device, wavelengthCount, fitResidual) { Method = method, AngularSamples = angularSamples };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSource {
    private CaptureSource() { }

    public sealed record MeasuredBrdf(Seq<BrdfSample> Samples, double Ior, Option<ComplexIor> Conductor) : CaptureSource;
    public sealed record SvbrdfMap(Seq<MaterialParameters> Texels) : CaptureSource;
    public sealed record SpectralReflectance(SpectralCurve Reflectance, double Metalness, double Roughness) : CaptureSource;

    // Fallback supplies the caller's own base row for every column the set does not carry, so the arm names no library key and hardcodes no neutral.
    public sealed record NeuralPlanes(TextureSet Planes, Seq<StageResult> Stages, MaterialParameters Fallback) : CaptureSource;

    // GlossMeter carries the cheapest real measurement in the estate: a handheld tri-angle gloss meter reads
    // specular reflectance at 20, 60, and 85 degrees in GLOSS UNITS against a polished black-glass standard, and
    // those three numbers are genuine instrument evidence a specification already records for coatings, panels, and
    // finishes. The triple crosses whole rather than as one number because the ANGLES discriminate: 20 degrees
    // separates the high-gloss band the 60 reading saturates on, 85 separates the matte band it bottoms out on, and
    // a single-angle reading cannot tell a satin coating from a semi-gloss one. Base carries the caller's own row so
    // the arm answers roughness alone and invents no colour a gloss meter never measured.
    public sealed record GlossMeter(double Gu20, double Gu60, double Gu85, GlossCurve Curve, MaterialParameters Base) : CaptureSource;
}

// GlossCurve is the GU-to-roughness CALIBRATION as a caller-supplied policy row, not a table this page transcribes:
// the correspondence depends on the instrument's aperture, the standard it zeroes against, and the coating family
// being read, so an estate that ships one curve as law states a number nobody measured. The row carries the
// SELECTION band each angle governs and the monotone map itself as a delegate, so a lab that characterizes its own
// meter lands one row and every consumer of that lab's captures reads its calibration rather than a default.
// Angles are the instrument's, so the band edges are GU on the SAME scale the reading is.
public readonly record struct GlossCurve(double HighBand, double MatteBand, Func<double, double> Roughness) {
    public double Of(double gu20, double gu60, double gu85) =>
        Roughness(gu60 >= HighBand ? gu20 : gu60 <= MatteBand ? gu85 : gu60);
}

// What an import PRODUCES: the parameter row every arm yields, its measured receipt, and — for a plane-bearing
// capture — the admitted set itself. The set is Option-typed rather than a fourth arm on a widened tuple because
// four of the five capture arms genuinely produce no planes, and a caller reading Planes.IsSome learns exactly
// whether a shadeable spatially-varying material came back or only its summary row.
public sealed record AcquiredMaterial(MaterialParameters Row, CaptureProvenance Provenance, Option<TextureSet> Planes) {
    public static AcquiredMaterial Summary(MaterialParameters row, CaptureProvenance provenance) => new(row, provenance, None);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class CaptureField {
    public static readonly CaptureField Description = new("description", rank: 1);
    public static readonly CaptureField ThetaI = new("theta_i", rank: 1);
    public static readonly CaptureField PhiI = new("phi_i", rank: 1);
    public static readonly CaptureField Sigma = new("sigma", rank: 2);
    public static readonly CaptureField Ndf = new("ndf", rank: 2);
    public static readonly CaptureField Vndf = new("vndf", rank: 4);
    public static readonly CaptureField Luminance = new("luminance", rank: 4);
    public static readonly CaptureField Rgb = new("rgb", rank: 5);
    public static readonly CaptureField Spectra = new("spectra", rank: 5);
    public static readonly CaptureField Wavelengths = new("wavelengths", rank: 1);

    public int Rank { get; }
}

// TensorDtype closes the container's element vocabulary. Each row carries BOTH its stored width and the WIDENING that lifts its
// run to the ONE interior scalar, so the reader never switches on a dtype key and a container carrying an unadmitted
// dtype names itself at the field gate rather than at a cast that produced numbers. Coupling the two columns is what
// keeps a width and its reader from disagreeing: a row admitted at four bytes that decoded as a double read every second element and reported a plausible measurement.
[SmartEnum<byte>]
public sealed partial class TensorDtype {
    public static readonly TensorDtype UInt8 = new(key: 1, width: 1, widen: static run => Lift<byte>(run, static v => v));
    public static readonly TensorDtype Int32 = new(key: 2, width: 4, widen: static run => Lift<int>(run, static v => v));
    public static readonly TensorDtype UInt32 = new(key: 3, width: 4, widen: static run => Lift<uint>(run, static v => v));
    public static readonly TensorDtype Float16 = new(key: 4, width: 2, widen: static run => Lift<Half>(run, static v => (double)v));
    public static readonly TensorDtype Float32 = new(key: 5, width: 4, widen: static run => Lift<float>(run, static v => v));
    public static readonly TensorDtype Float64 = new(key: 6, width: 8, widen: static run => Lift<double>(run, static v => v));

    public int Width { get; }

    [UseDelegateFromConstructor]
    public partial ReadOnlyMemory<double> Widen(ReadOnlyMemory<byte> run);

    // ONE lift over the six rows: the stored element type is the type parameter and the row's own projection the
    // only per-row datum, so a seventh dtype is a row and never a second cast body.
    static ReadOnlyMemory<double> Lift<TStored>(ReadOnlyMemory<byte> run, Func<TStored, double> project) where TStored : struct =>
        (ReadOnlyMemory<double>)[.. MemoryMarshal.Cast<byte, TStored>(run.Span).ToArray().Select(project)];
}

// Both derived facts have their reader in Lower, so neither is a decorative accessor.
public sealed record BrdfArchive(HashMap<CaptureField, ReadOnlyMemory<double>> Fields, HashMap<CaptureField, Seq<int>> Extents) {
    // Magic spells the container's own leading word, "TLB_FSDF" little-endian.
    public const ulong Magic = 0x00465344425F4C54;

    public bool Isotropic => Fields.Find(CaptureField.PhiI).Map(static phi => phi.Length <= 2).IfNone(true);
    public int WavelengthCount => Fields.Find(CaptureField.Wavelengths).Map(static grid => grid.Length).IfNone(0);

    // Every payload widens to double at admission, so the interior holds ONE scalar type and no read re-decides what a field's bytes meant.
    // Exemption: the header walk is a boundary container kernel — a length-prefixed field run with no rail inside it.
    public static Fin<BrdfArchive> Of(ReadOnlyMemory<byte> payload, Op key) {
        if (payload.Length < 12 || BinaryPrimitives.ReadUInt64LittleEndian(payload.Span) != Magic) {
            return new MaterialFault.Parameter(key, $"<brdf-archive-magic:{payload.Length}>");
        }
        var fields = HashMap<CaptureField, ReadOnlyMemory<double>>();
        var extents = HashMap<CaptureField, Seq<int>>();
        for (int cursor = 12; cursor < payload.Length;) {
            Fin<(CaptureField Row, Seq<int> Shape, ReadOnlyMemory<double> Payload, int Next)> read = Field(payload, cursor, key);
            if (read.Case is not (CaptureField row, Seq<int> shape, ReadOnlyMemory<double> lanes, int next)) {
                return read.Map(static _ => default(BrdfArchive)!);
            }
            (fields, extents, cursor) = (fields.AddOrUpdate(row, lanes), extents.AddOrUpdate(row, shape), next);
        }
        // Two fields carry every lowering: the incident zenith grid the samples index and the measured triple
        // they carry. An archive missing either is not a partial capture to salvage — it is a container this reader
        // cannot lower at all, so it refuses at admission rather than at the first empty walk.
        return fields.Find(CaptureField.ThetaI).IsSome && fields.Find(CaptureField.Rgb).IsSome
            ? Fin.Succ(new BrdfArchive(fields, extents))
            : new MaterialFault.Parameter(key, "<brdf-archive-incomplete>");
    }

    // A field the roster does not carry is a typed refusal naming the offending key, never a silently skipped run,
    // and a rank the row contradicts refuses BY NAME rather than reading its neighbour's stride.
    static Fin<(CaptureField Row, Seq<int> Shape, ReadOnlyMemory<double> Payload, int Next)> Field(
        ReadOnlyMemory<byte> payload, int at, Op key) {
        ReadOnlySpan<byte> bytes = payload.Span;
        int nameLength = BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
        string name = Encoding.ASCII.GetString(bytes.Slice(at + 2, nameLength));
        int head = at + 2 + nameLength;
        (int rank, byte dtype) = (BinaryPrimitives.ReadUInt16LittleEndian(bytes[head..]), bytes[head + 2]);
        Seq<int> shape = toSeq(Enumerable.Range(0, rank).Select(axis => (int)BinaryPrimitives.ReadUInt32LittleEndian(bytes[(head + 3 + (axis * 4))..])));
        int at0 = head + 3 + (rank * 4);
        return !CaptureField.TryGet(name, out CaptureField? row)
            ? new MaterialFault.Parameter(key, $"<brdf-archive-field:{name}>")
            : !TensorDtype.TryGet(dtype, out TensorDtype? element)
                ? new MaterialFault.Parameter(key, $"<brdf-archive-dtype:{name}:{dtype}>")
                : row!.Rank != rank
                    ? new MaterialFault.Parameter(key, $"<brdf-archive-rank:{name}:{rank}!={row.Rank}>")
                    : shape.Fold(element!.Width, static (n, axis) => n * axis) switch {
                        var run when at0 + run <= bytes.Length =>
                            Fin.Succ((row, shape, element.Widen(payload.Slice(at0, run)), at0 + run)),
                        var run => new MaterialFault.Parameter(key, $"<brdf-archive-truncated:{name}:{run}>"),
                    };
    }

    // Lower turns the admitted archive into the two shapes the import arms consume, and it is where ISOTROPY earns
    // its keep: an isotropic measurement sweeps ONE incident azimuth, so the relative-azimuth column is the outgoing
    // azimuth alone and the grid carries no second alpha to fit — which is exactly the degenerate spread the fit's
    // own observability witness then reports rather than fabricating a grain. An anisotropic archive sweeps its
    // declared phi_i slices and the samples span the azimuth the anisotropy lives on.
    // Exemption: the lattice walk is a boundary container kernel — a declared-extent product with no rail inside it.
    public Fin<Seq<BrdfSample>> Lower(Op key) =>
        (Fields.Find(CaptureField.ThetaI), Fields.Find(CaptureField.PhiI), Fields.Find(CaptureField.Rgb)) switch {
            (var thetaI, var phiI, var rgb) when thetaI.IsSome && rgb.IsSome =>
                Fin.Succ(toSeq(Walk(thetaI.IfNone(default), phiI.IfNone(default), rgb.IfNone(default)))),
            _ => new MaterialFault.Parameter(key, "<brdf-archive-unlowerable>"),
        };

    IEnumerable<BrdfSample> Walk(ReadOnlyMemory<double> thetaI, ReadOnlyMemory<double> phiI, ReadOnlyMemory<double> rgb) {
        int azimuths = Isotropic ? 1 : Math.Max(1, phiI.Length);
        int lanes = Math.Max(1, rgb.Length / Math.Max(1, thetaI.Length * azimuths * 3));
        for (int incident = 0; incident < thetaI.Length; incident++) {
            for (int azimuth = 0; azimuth < azimuths; azimuth++) {
                for (int outgoing = 0; outgoing < lanes; outgoing++) {
                    int lane = (((incident * azimuths) + azimuth) * lanes + outgoing) * 3;
                    yield return new BrdfSample(
                        thetaI.Span[incident],
                        (outgoing + 0.5) / lanes * (Math.PI / 2.0),
                        Isotropic ? 0.0 : phiI.Span[azimuth],
                        RgbSpectrum.Create(Math.Max(0.0, rgb.Span[lane]), Math.Max(0.0, rgb.Span[lane + 1]), Math.Max(0.0, rgb.Span[lane + 2])));
                }
            }
        }
    }

    // Spectra lowers the per-band half the SpectralReflectance arm grounds: the wavelength grid is the FILE's own,
    // so the curve's start and interval derive from the first two samples rather than from a constant this reader
    // would have to keep in step with a dataset it does not own.
    public Fin<SpectralCurve> Spectra(Op key) =>
        (Fields.Find(CaptureField.Wavelengths), Fields.Find(CaptureField.Spectra)) switch {
            (var grid, var spectra) when grid.IsSome && spectra.IsSome && WavelengthCount > 1 =>
                grid.IfNone(default) switch {
                    var nm => SpectralCurve.Of((int)nm.Span[0], (int)Math.Round(nm.Span[1] - nm.Span[0]), spectra.IfNone(default)[..WavelengthCount], key),
                },
            _ => new MaterialFault.Parameter(key, $"<brdf-archive-no-spectra:{WavelengthCount}>"),
        };
}

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Acquisition {
    static Acquisition() => Control.UseManaged();

    // Every non-fitting arm carries the Context inertly, so widening a second arm onto the solver ladder is a body edit and never a signature one.
    public static Fin<AcquiredMaterial> Import(
        CaptureSource source, CaptureProvenance provenance, Context context, Op key, Option<CaptureCalibration> calibration = default) =>
        source.Switch(
            state: (provenance, context, key),
            measuredBrdf:        static (s, c) => FitBrdf(c.Samples, c.Ior, c.Conductor, s.provenance, s.context, s.key),
            svbrdfMap:           static (s, c) => AverageField(c.Texels, s.provenance, s.key),
            spectralReflectance: static (s, c) => GroundSpectral(c.Reflectance.ToSpd(), c.Metalness, c.Roughness, s.key)
                                                    .Map(row => AcquiredMaterial.Summary(row, s.provenance with { Method = CaptureMethod.Spectrophotometer })),
            neuralPlanes:        static (s, c) => ImportPlanes(c.Planes, c.Stages, c.Fallback, s.provenance, s.key),
            glossMeter:          static (s, c) => FitGloss(c, s.provenance, s.key))
        .Bind(acquired => Calibrate(acquired, calibration, key))
        .Bind(acquired => MaterialParameters.Of(acquired.Row, key).Map(row => acquired with { Row = row, Provenance = acquired.Provenance with {
            DominantWavelengthNm = row.BaseColor.DominantWavelength,
            ExcitationPurity = row.BaseColor.ExcitationPurity,
            CctKelvin = row.BaseColor.Temperature.Cct,
            CctDuv = row.BaseColor.Temperature.Duv } }));

    // Writing a colour, a metalness, or an anisotropy here would fabricate the columns the instrument is silent
    // about. The mapped roughness carries NO residual, because a calibrated single-point reading has no fit to leave
    // one — a zero residual on this arm is the honest statement that nothing was solved, not a perfect solve.
    static Fin<AcquiredMaterial> FitGloss(CaptureSource.GlossMeter c, CaptureProvenance provenance, Op key) =>
        double.IsFinite(c.Gu20) && double.IsFinite(c.Gu60) && double.IsFinite(c.Gu85)
        && c.Gu20 >= 0.0 && c.Gu60 >= 0.0 && c.Gu85 >= 0.0
            ? c.Curve.Of(c.Gu20, c.Gu60, c.Gu85) switch {
                var roughness when double.IsFinite(roughness) && roughness is >= 0.0 and <= 1.0 =>
                    Fin.Succ(AcquiredMaterial.Summary(
                        c.Base with { Roughness = roughness },
                        CaptureProvenance.Of(CaptureMethod.GlossMeter, provenance.Device, provenance.WavelengthCount,
                            angularSamples: 3, fitResidual: 0.0))),
                var roughness => new MaterialFault.Parameter(key, $"<gloss-curve-out-of-unit:{roughness:R}>"),
            }
            : new MaterialFault.Parameter(key, $"<gloss-reading-out-of-range:{c.Gu20:R},{c.Gu60:R},{c.Gu85:R}>");

    // A goniophotometer capture simply passes None.
    static Fin<AcquiredMaterial> Calibrate(AcquiredMaterial acquired, Option<CaptureCalibration> calibration, Op key) =>
        calibration
            .Map(chart => Solve(chart, key).Map(fit => acquired with {
                Row = acquired.Row with { BaseColor = Correct(acquired.Row.BaseColor, fit.Matrix) },
                Provenance = acquired.Provenance with { Calibrated = true, CalibrationDeltaE = Some(fit.DeltaE) } }))
            .IfNone(Fin.Succ(acquired));

    // The residual is the mean CIEDE2000 the corrected patches still carry, which is the number that separates a usable chart shot from a hopeful one.
    static Fin<(Matrix<double> Matrix, double DeltaE)> Solve(CaptureCalibration chart, Op key) {
        Seq<Unicolour> reference = toSeq(Macbeth.All).Map(static patch => patch.ConvertToConfiguration(PortValue.SceneLinear));
        Matrix<double> measured = Matrix<double>.Build.Dense(chart.Patches.Count, 3, (row, band) => Band(chart.Patches[row], band));
        Matrix<double> target = Matrix<double>.Build.Dense(reference.Count, 3, (row, band) => Band(reference[row], band));
        Matrix<double> fit = measured.QR(QRMethod.Thin).Solve(target);
        return fit.Enumerate().All(double.IsFinite)
            ? Fin.Succ((fit, chart.Patches
                .Map((patch, index) => Correct(patch, fit).Difference(reference[index], DeltaE.Ciede2000))
                .Fold(0.0, static (sum, delta) => sum + delta) / chart.Patches.Count))
            : new MaterialFault.Parameter(key, "<capture-calibration-degenerate>");
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

    // The durable form is a wavelength-sampled EXR part, one texel wide, whose channel names ARE the sampling grid.
    // CaptureProvenance.WavelengthCount becomes the round-trip WITNESS rather than a bare count: a thaw whose decoded grid disagrees with the receipt refuses.
    public static Fin<ReadOnlyMemory<byte>> Freeze(SpectralCurve curve, SpectralKind kind, Op key) {
        float[] wavelengths = [.. Enumerable.Range(0, curve.Coefficients.Length).Select(i => (float)curve.WavelengthAt(i))];
        float[] samples = [.. curve.Coefficients.ToArray().Select(static v => (float)v)];
        WriterResult<byte[]> written = ExrFile.SaveToMemory(
            new TinyEXR.V3.Image([kind.Part(width: 1, height: 1, wavelengths, samples, units: "nm", Compression.ZIP)]),
            Compression.ZIP, options: null);
        return written is { IsSuccess: true, Value: { } bytes }
            ? Fin.Succ((ReadOnlyMemory<byte>)bytes)
            : new MaterialFault.Parameter(key, $"<spectral-freeze:{written.Status}>");
    }

    // Thaw returns the KIND beside the curve, so the round trip preserves the physical claim rather than the claim
    // surviving only in whichever caller happened to know it.
    public static Fin<(SpectralCurve Curve, SpectralKind Kind)> Thaw(ReadOnlyMemory<byte> payload, int wavelengthCount, Op key) {
        ReaderResult<TinyEXR.V3.Image> read = ExrFile.LoadFromMemory(payload, options: null);
        return read is { IsSuccess: true, Value: { } file } && toSeq(file.Parts).Head.Case is Part part && Spectral.IsSpectral(part.Header)
            ? SpectralKind.Of(Spectral.GetSpectrumType(part.Header)).Case is SpectralKind kind
                ? Spectral.GetWavelengths(part.Header) is { Length: > 1 } grid && grid.Length == wavelengthCount
                    ? Samples(part, key).Bind(samples =>
                          SpectralCurve.Of((int)grid[0], (int)Math.Round(grid[1] - grid[0]), samples, key).Map(curve => (curve, kind)))
                    : new MaterialFault.Parameter(key, $"<spectral-thaw-grid:{wavelengthCount}>")
                : new MaterialFault.Parameter(key, $"<spectral-thaw-kind:{Spectral.GetSpectrumType(part.Header)}>")
            : new MaterialFault.Parameter(key, $"<spectral-thaw:{read.Status}>");
    }

    // A non-zero Stokes component is DROPPED rather than summed into an unpolarised sample it is not. Ordering is by
    // parsed wavelength, so the grid the header declares and the samples this returns index the same spectrum by construction.
    static Fin<ReadOnlyMemory<double>> Samples(Part part, Op key) =>
        part.GetLevel(0, 0) switch {
            var level => toSeq(level.Channels)
                .Filter(Spectral.IsSpectralChannel)
                .Choose(name => Spectral.TryParseChannelWavelength(name, out float nm)
                             && Spectral.TryGetStokesComponent(name, out int stokes) && stokes == 0
                                ? Some((Nm: nm, Name: name))
                                : Option<(float Nm, string Name)>.None)
                .OrderBy(static channel => channel.Nm)
                .Map(channel => (double)MemoryMarshal.Cast<byte, float>(level.GetChannel(channel.Name).Data.Span)[0]) switch {
                var samples => samples.IsEmpty
                    ? Fin.Fail<ReadOnlyMemory<double>>(new MaterialFault.Parameter(key, "<spectral-thaw-no-channels>"))
                    : Fin.Succ((ReadOnlyMemory<double>)samples.ToArray()),
            },
        };

    // The bind target's measured per-channel fold runs over each plane's BASE level — never a pyramid tail, never a re-derivation here.
    static Fin<AcquiredMaterial> ImportPlanes(TextureSet planes, Seq<StageResult> stages, MaterialParameters fallback, CaptureProvenance provenance, Op key) =>
        stages.IsEmpty
            ? new MaterialFault.Parameter(key, "<neural-planes-no-stage-evidence>")
            : from binding in SetBind.Bind(planes, fallback, BindTarget.Summary, SamplerState.Default, key)
              from row in binding is SetBinding.Row bound
                  ? Fin.Succ(bound.Parameters)
                  : Fin.Fail<MaterialParameters>(new MaterialFault.Graph(key, "<neural-planes-summary-not-a-row>"))
              select new AcquiredMaterial(row, Attributed(provenance, planes, stages), Some(planes));

    // The grant a consumer must honour is the strictest one any contributing model imposes, and a set is only as
    // trustworthy as its weakest stage. A stage whose card left the registry contributes its result without an
    // attribution rather than blocking the import — the plan already gated the grant at request construction, and
    // this column is evidence rather than a second gate.
    static CaptureProvenance Attributed(CaptureProvenance provenance, TextureSet planes, Seq<StageResult> stages) {
        Option<ModelCard> governing = stages
            .Choose(static result => ModelRegistry.Rows.TryGetValue(result.ModelCardId, out ModelCard? card) ? Some(card!) : Option<ModelCard>.None)
            .Fold(Option<ModelCard>.None, static (strictest, card) => strictest.Filter(held => held.License.Rank >= card.License.Rank).IsSome ? strictest : Some(card));
        return CaptureProvenance.Of(CaptureMethod.NeuralPlanes, planes.Digest.ToValue(), provenance.WavelengthCount,
                angularSamples: stages.Fold(0, static (tiles, result) => tiles + result.TilesEmitted),
                fitResidual: stages.Fold(0.0, static (worst, result) => Math.Max(worst, result.GoldenDelta)))
            with { ModelCard = governing.Map(static card => card.Id), License = governing.Map(static card => card.License),
                   ModelArtefact = governing.Bind(static card => card.Artefact) };
    }

    // Geometry is a ⌈√count⌉-lane stratified (θi, θo) grid with Δφ swept uniformly over [0, π) so alphaX ≠ alphaY
    // stays observable, each zenith jittered inside its cell by a SplitMix64 stream off the seed.
    public static Fin<Seq<BrdfSample>> SyntheticGrid(int seed, int count, Op key) {
        if (count < 1) { return new MaterialFault.Parameter(key, $"<synthetic-grid-count:{count}>"); }
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

    // The underdetermined gate reads the ARM's own widened unknown count, so a conductor capture with four samples fits and a dielectric one with four refuses.
    static Fin<AcquiredMaterial> FitBrdf(
        Seq<BrdfSample> samples, double ior, Option<ComplexIor> conductor, CaptureProvenance provenance, Context context, Op key) =>
        guard(samples.Count > (conductor.IsSome ? 3 : 4), new MaterialFault.Parameter(key, $"<measured-brdf-underdetermined:{samples.Count}>")).ToFin()
            .Bind(_ => SolveGgx(samples, ior, conductor, context, key))
            .Bind(fit => SpectralUpsample.ToSpd(
                    samples.Map(static s => s.Reflectance).Fold(RgbSpectrum.Black, static (acc, r) => acc.Add(r)).Scale(1.0 / samples.Count), key)
                .Bind(spd => GroundSpectral(spd, metalness: conductor.IsSome ? 1.0 : 0.0, fit.Roughness, key))
                .Map(row => AcquiredMaterial.Summary(
                             row with { Ior = fit.Eta, Anisotropy = fit.Anisotropy, AnisotropyRotation = fit.Rotation },
                             CaptureProvenance.Of(CaptureMethod.Goniophotometer, provenance.Device, provenance.WavelengthCount, samples.Count, fit.Residual)
                                 with { FitConditionNumber = fit.ConditionNumber, FitRank = fit.Rank })));

    readonly record struct BrdfFit(double Roughness, double Anisotropy, double Rotation, double Eta, double Residual, double ConditionNumber, int Rank);

    // Every iteration mechanic belongs to Rasm/Solving/solver and none of it is respelled here.
    static Fin<BrdfFit> SolveGgx(Seq<BrdfSample> samples, double ior0, Option<ComplexIor> conductor, Context context, Op key) =>
        new BrdfResidual(samples, ior0, conductor) switch {
            var residual => SolvePolicy.Of(context: context, key: key)
                .Bind(ladder => Lm.Minimize(new DualModel(residual), ladder, key))
                .Bind(result => residual.Project(result, key)),
        };

    // Stating the residual ROW AT A TIME in dual arithmetic lets DualModel derive the exact Jacobian — the
    // differencing step this page used to choose is gone, and with it the question of what step size an anisotropic
    // GGX surface tolerates. The box map is p = Lo + (Hi − Lo)·σ(u) with σ the logistic, so the solver optimizes an UNCONSTRAINED u.
    sealed class BrdfResidual(Seq<BrdfSample> samples, double ior0, Option<ComplexIor> conductor) : IDualResidual {
        static readonly (double Lo, double Hi)[] AlphaPhi = [(1e-4, 1.0), (1e-4, 1.0), (0.0, Math.PI)];
        static readonly (double Lo, double Hi)[] AlphaPhiEta = [.. AlphaPhi, (1.0, 2.5)];

        internal (double Lo, double Hi)[] Bounds => conductor.IsSome ? AlphaPhi : AlphaPhiEta;
        public int Dof => Bounds.Length;             // [alphaX, alphaY, phi(, eta)] — the conductor Fresnel is measured, not fit
        public int Rows => samples.Count;

        // The seed is the BOUNDED parameter inverted through the reparameterization, so the solver starts from an
        // unconstrained coordinate whose image is the physical seed rather than from a raw value the box would clip.
        public double[] Seed => [.. Bounds.Zip(
            conductor.IsSome
                ? [Microfacet<double>.AlphaOf(0.3), Microfacet<double>.AlphaOf(0.3), 0.0]
                : [Microfacet<double>.AlphaOf(0.3), Microfacet<double>.AlphaOf(0.3), 0.0, Math.Clamp(ior0, 1.0, 2.5)],
            static (box, value) => Free(value, box))];

        // Each row is the LOG residual: reflectance spans decades across a capture, so a linear residual lets the
        // near-specular geometries dominate every step and the grazing tail contributes nothing to the fit.
        public Dual<ddouble> Row(int index, ReadOnlySpan<Dual<ddouble>> parameters) =>
            Dual<ddouble>.Of(ddouble.Log(ddouble.Max(Floor, (ddouble)samples[index].Reflectance.Luminance)))
            - Dual<ddouble>.Log(Model(samples[index], parameters));

        // The bounded parameter vector, reconstructed from the solver's unconstrained coordinates inside the dual
        // arithmetic so the box map itself differentiates rather than sitting outside the derivative chain.
        Dual<ddouble> Model(BrdfSample sample, ReadOnlySpan<Dual<ddouble>> u) =>
            Reflectance(sample,
                alphaX:   Bound(u[0], Bounds[0]), alphaY: Bound(u[1], Bounds[1]), rotation: Bound(u[2], Bounds[2]),
                eta:      u.Length == 4 ? Bound(u[3], Bounds[3]) : Dual<ddouble>.Of((ddouble)1.5),
                conductor: conductor);

        // Project reads the ladder's receipt onto the row: the parameters bound back through the same map, and the
        // witnessed relative residual off the functor's OWN 106-bit objective rather than a norm recomputed here.
        internal Fin<BrdfFit> Project(LmResult result, Op key) =>
            result.IsValid
                ? [.. Bounds.Zip(result.Parameters, static (box, free) => Bound(free, box))] switch {
                    var p => Fin.Succ(new BrdfFit(
                        Roughness: Math.Sqrt(Math.Sqrt(p[0] * p[1])),  // alpha_geo = √(αx·αy) = roughness²
                        Anisotropy: Math.Clamp((1.0 - (Math.Min(p[0], p[1]) / Math.Max(p[0], p[1]))) / 0.9, 0.0, 1.0),  // inverse Disney aspect² = min/max
                        // Rotation projects the fitted azimuth onto the row's UNIT column (1 is a half turn), taken
                        // from the ALPHA-X axis: when the fit lands alphaY as the rougher of the pair the rougher
                        // axis is a quarter turn away, so the projection adds that quarter rather than reporting the smooth axis as the grain.
                        Rotation: Math.Clamp(((p[2] + (p[0] >= p[1] ? 0.0 : Math.PI / 2.0)) / Math.PI) % 1.0, 0.0, 1.0),
                        Eta: p.Length == 4 ? p[3] : 1.5,   // conductor rows keep the coat-side Disney default — Ior is unread at Metalness 1
                        Residual: result.Norm / Math.Max(1e-6, Scale),
                        ConditionNumber: Observable ? 1.0 / Math.Max(1e-12, AzimuthSpread) : double.PositiveInfinity,
                        Rank: Observable ? Dof : Dof - 2))
                }
                : new MaterialFault.Parameter(key, "<ggx-fit-diverged>");

        // Deriving the witness from the capture is what lets the kernel functor own the numerics whole: the ladder
        // already regularizes a deficient system into SOME answer, and what a reader needs is the reason that answer is not evidence.
        double AzimuthSpread => samples.Fold((Lo: double.MaxValue, Hi: double.MinValue),
            static (span, s) => (Math.Min(span.Lo, s.AzimuthDelta), Math.Max(span.Hi, s.AzimuthDelta))) switch {
            var span => span.Hi - span.Lo,
        };
        bool Observable => AzimuthSpread > 1e-3;

        // The log-measured magnitude the relative residual normalizes against — the capture's own scale, folded once.
        double Scale => Math.Sqrt(samples.Fold(0.0, static (sum, s) =>
            Math.Log(Math.Max(1e-6, s.Reflectance.Luminance)) switch { var l => sum + (l * l) }));

        static readonly ddouble Floor = (ddouble)1e-6;

        // The logistic box and its inverse: one pair, so a bound and a seed can never disagree about the same box.
        static Dual<ddouble> Bound(Dual<ddouble> free, (double Lo, double Hi) box) =>
            Dual<ddouble>.Of((ddouble)box.Lo)
            + (Dual<ddouble>.Of((ddouble)(box.Hi - box.Lo))
               / (Dual<ddouble>.Of(ddouble.One) + Dual<ddouble>.Exp(-free)));
        static double Bound(double free, (double Lo, double Hi) box) => box.Lo + ((box.Hi - box.Lo) / (1.0 + Math.Exp(-free)));
        static double Free(double bounded, (double Lo, double Hi) box) =>
            Math.Clamp((bounded - box.Lo) / (box.Hi - box.Lo), 1e-9, 1.0 - 1e-9) switch { var t => Math.Log(t / (1.0 - t)) };
    }

    // alphas are GGX alphas directly (roughness² per the Disney remap); the per-cell evaluation rides the SolveGgx
    // kernel carve-out. The deleted form was a re-minted D/G/F with a cos((θi−θo)/2) specular-plane half-angle that
    // drops the azimuth, and Δφ is what makes alphaX ≠ alphaY observable. The conductor arm lifts the measured bands
    // into the scalar and folds them through the kernel's own luminance weights, so the Fresnel discriminant stays
    // the capture's own fact rather than a branch the solver has to see through.
    static T Reflectance<T>(BrdfSample s, T alphaX, T alphaY, T rotation, T eta, Option<ComplexIor> conductor)
        where T : INumber<T>, IRootFunctions<T>, IPowerFunctions<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>, ITrigonometricFunctions<T> {
        (T zi, T zo, T dphi) = (T.CreateChecked(s.IncidentZenith), T.CreateChecked(s.OutgoingZenith), T.CreateChecked(s.AzimuthDelta));
        LocalVector<T> wi = new LocalVector<T>(T.Sin(zi), T.Zero, T.Cos(zi)).Normalize().RotateZ(-rotation);
        LocalVector<T> wo = new LocalVector<T>(T.Sin(zo) * T.Cos(dphi), T.Sin(zo) * T.Sin(dphi), T.Cos(zo)).Normalize().RotateZ(-rotation);
        T floor = T.CreateChecked(1e-6);
        if (!wi.SameHemisphere(wo)) { return floor; }
        LocalVector<T> h = wi.Add(wo).Normalize();
        T d = Microfacet<T>.Ndf(h, alphaX, alphaY);
        T g = Microfacet<T>.MaskingShadowing(wo, wi, alphaX, alphaY);
        T f = conductor.Match(
            Some: ior => Luminance<T>(ior, T.Abs(wo.Dot(h))),
            None: () => Microfacet<T>.FresnelDielectric(wo.Dot(h), eta));
        T guard = T.CreateChecked(1e-4);
        T denom = T.CreateChecked(4.0) * T.Max(guard, T.Abs(wi.CosTheta)) * T.Max(guard, T.Abs(wo.CosTheta));
        return T.Max(floor, d * g * f / denom);
    }

    // The measured conductor's per-band Fresnel reduced to the ONE luminance the log residual reads, folded through
    // the bsdf#LOBE_FAMILY luminance-weight owner so the reduction follows a working-space change rather than pinning a triple this page would have to maintain.
    static T Luminance<T>(ComplexIor ior, T cosI)
        where T : INumber<T>, IRootFunctions<T>, IPowerFunctions<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>, ITrigonometricFunctions<T> =>
        (T.CreateChecked(RgbSpectrum.LuminanceWeights.R) * Microfacet<T>.FresnelConductor(cosI, T.CreateChecked(ior.Eta.R), T.CreateChecked(ior.K.R)))
        + (T.CreateChecked(RgbSpectrum.LuminanceWeights.G) * Microfacet<T>.FresnelConductor(cosI, T.CreateChecked(ior.Eta.G), T.CreateChecked(ior.K.G)))
        + (T.CreateChecked(RgbSpectrum.LuminanceWeights.B) * Microfacet<T>.FresnelConductor(cosI, T.CreateChecked(ior.Eta.B), T.CreateChecked(ior.K.B)));

    // The base/emission average runs in RgbLinear (the shading-truth channel) so two texels' colors blend
    // physically, not in a display-encoded space. Averaged rows re-admit through MaterialParameters.Of (the Import
    // Bind), so a degenerate mean rails the row gate rather than passing a bad scatter band.
    static Fin<AcquiredMaterial> AverageField(Seq<MaterialParameters> texels, CaptureProvenance provenance, Op key) =>
        texels.IsEmpty
            ? new MaterialFault.Parameter(key, "<svbrdf-map-empty>")
            : Fin.Succ(AcquiredMaterial.Summary(
                        texels.Fold(FieldSum.Zero, static (acc, t) => acc.Add(t)).Mean(texels.Count),
                        CaptureProvenance.Of(CaptureMethod.NeuralSvbrdf, provenance.Device, provenance.WavelengthCount, texels.Count, provenance.FitResidual)));

    // Film is NOT averaged: a neural field carries no interference column, so the mean row keeps the init-defaulted ThinFilm.None.
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
        from _ in guard(GamutPolicy.Perceptual.Contains(color), new MaterialFault.Gamut(key, "<acquired-color-out-of-gamut>"))
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

(none)

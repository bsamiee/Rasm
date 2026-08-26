# [MATERIALS_ACQUISITION]

`Acquisition.Import` folds the closed measured-BRDF, SVBRDF-map, spectral-reflectance, gloss-meter, and neural-plane `CaptureSource` family into the `AcquiredMaterial` product — the `MaterialParameters` row the material library registers and the lobe family shades, its `CaptureProvenance`, and, for a plane-bearing capture, the admitted `Raster/set#TEXTURE_SET` `TextureSet` itself. Capture fitting composes the `bsdf#MICROFACET_KERNEL` GGX/Smith/Fresnel kernel over reconstructed incident and outgoing directions, hands the anisotropic conductor-or-dielectric parameter vector to `Lm.Minimize`, grounds spectral colour through `surface#SPECTRAL_UPSAMPLE`, and gates the admitted row through the gamut and `MaterialFault` channels. `CaptureProvenance` carries present-only sampling, the closed fit-or-inference `CaptureAssessment`, independently present chromaticity readouts, chart calibration, model attribution, and third-party custody; no absent measurement is encoded as zero or infinity. It remains distinct from the contract `Rasm.Element` `PropertyEvidence` citation carrier. Per-capture acquired-material types and private BRDF kernels are deleted forms.

## [01]-[INDEX]

- [02]-[ACQUISITION]: `CaptureSource` closes the `[Union]` capture family, `BrdfSample` records angular reflectance, `AcquiredMaterial` carries the import product, `CaptureMethod` keys the instrument, and `CaptureProvenance` carries the measurement record; `Acquisition.Import` composes the generic forward model, the kernel `Lm.Minimize` fit, the per-texel SVBRDF average, the gloss-meter reading, the neural-plane `SetBind` summary, the chart-solved colour calibration every arm crosses, the `Freeze`/`Thaw` spectral-EXR round trip, and the `BrdfArchive` EPFL RGL tensor-container reader over its `CaptureField`/`TensorDtype` vocabularies.

## [02]-[ACQUISITION]

- Owner: `Acquisition` the static import fold; `CaptureSource` `[Union]` the closed capture family; `AcquiredMaterial` the one import product; `BrdfSample` the goniophotometer angular record over `(θi, θo, Δφ)`; `CaptureMethod` `[SmartEnum<string>]` the instrument discriminant; `CaptureProvenance` the measurement record; `SpectralCurve` the durable sampled-spectrum carrier with its scale-invariant `LuminousEfficacy` fold; `SpectralKind` the reflective/emissive claim binding its own container part factory; `GlossCurve` the caller-supplied GU-to-roughness calibration row; `BrdfArchive` the admitted EPFL RGL tensor container over its `CaptureField`/`TensorDtype` vocabularies; `CaptureCalibration` the twenty-four measured chart patches.
- Cases: capture {`MeasuredBrdf` (angular samples, the dielectric IOR seed, and the Fresnel discriminant — `Some` conductor fixes a measured `ComplexIor` so only `(αx, αy, φ)` fit, `None` fits the dielectric `(αx, αy, φ, η)`), `SvbrdfMap` (a per-texel field collapsed to one row and discarded), `SpectralReflectance` (a `SpectralCurve` with its metalness/roughness — the carrier that PERSISTS, where an `Spd` cannot answer what grid built it), `GlossMeter` (the tri-angle 20/60/85 GU triple over the caller's base row), `NeuralPlanes` (an admitted `TextureSet` with its `Seq<StageResult>` evidence and a fallback row — the planes SURVIVE)}. `CaptureMethod` admits every instrument as one ROW (goniophotometer, spectrophotometer, neural-SVBRDF, neural-planes, gloss-meter, the `finish#FINISH` Kubelka-Munk pigment mix, the authored sentinel), never a per-instrument capture type.
- Entry: `public static Fin<AcquiredMaterial> Import(CaptureSource source, CaptureProvenance provenance, Context context, Op key, Option<CaptureCalibration> calibration = default)` — the ONE import path. `Context` carries the SOLVER LADDER: `SolvePolicy.Of(context, key)` derives the damped Gauss-Newton residual, step, and iteration bands off the model context, so the fit's convergence gates are a project's to tighten and never this page's constants. `CaptureCalibration` carries the one axis no capture value holds, applied at the single pre-admission site every arm crosses.
- Entry: each arm produces `(row, provenance)`: `MeasuredBrdf` runs the kernel functor over `BrdfResidual`, projects onto the Disney `Roughness`/`Anisotropy`/`AnisotropyRotation` columns, and stamps the goniophotometer provenance with the witnessed residual and the observability witness; `SpectralReflectance` grounds through `surface#SPECTRAL_UPSAMPLE`; `SvbrdfMap` folds a REAL per-column mean; `GlossMeter` writes roughness ALONE; `NeuralPlanes` binds through the ONE `Raster/set#SET_BIND` `BindTarget.Summary` fold and returns the set beside the row, stamping the set digest as its device, the summed tiles as its sample count, the WORST stage reference delta as its residual, and the MOST RESTRICTIVE contributing card as its attribution.
- Entry: `SyntheticGrid(seed, count, key)` mints the deterministic stratified capture the benchmark corpus pins — geometry, ground-truth alphas, and a ground-truth grain azimuth all derived from the seed through the KERNEL's one lane-keyed draw, reflectance the kernel forward model at those parameters, so the fit workload has a known answer in direction as well as magnitude and no fixture file exists. `BrdfArchive.Of(payload, key)` admits an EPFL RGL container and `Lower` turns it into the `Seq<BrdfSample>` the fit consumes and the per-band `SpectralCurve` the grounding does. `Freeze(curve, kind, key)`/`Thaw(payload, wavelengthCount, key)` are the durable pair a measured spectrum persists through, the provenance's own `WavelengthCount` the round-trip witness.
- Packages: Wacton.Unicolour (composed — `new Unicolour(PortValue.SceneLinear, Spd)` grounding, `RgbLinear.Triplet` channel reads, `IsInRgbGamut`, and the `DominantWavelength`/`ExcitationPurity`/`Temperature` chromaticity readout the provenance stamps), Wacton.Unicolour.Datasets (`Macbeth.All`, the 24-patch reference the chart solve targets), MathNet.Numerics (composed for the LINEAR chart solve ALONE — `Matrix<double>.Build.Dense`, `QR(QRMethod.Thin)`, `QR<double>.Solve`, `Control.UseManaged` the osx-arm64 provider), Rasm (project — `Op`, `Context`, `Deterministic`, and `Rasm.Solving` `Lm.Minimize`/`IDualResidual`/`DualModel`/`Dual<T>`/`SolvePolicy`), Rasm.Element (`ContentAddress`), `Rasm.Materials.Raster` (`TextureSet`, `SetBind.Bind`, `IngestProvenance`), `neural#MODEL_REGISTRY` (`StageResult`, `ModelRegistry.Rows`, `ModelCard`), TinyEXR.NET (the durable spectral container), DoubleDouble, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a new capture modality is one `CaptureSource` case — never a per-capture material or a second import owner; a new numerical result shape is one `CaptureAssessment` case, while optional facts stay typed absence on their owner. A new fit parameter is one `(Lo, Hi)` row on `BrdfResidual.Bounds`; `Dof`, the differentiable box map, rank evidence, and the Jacobian derive from that row. A new tensor field is one `CaptureField` row carrying the rank its payload must hold.
- Law: this page carves out FIVE measured kernels and nothing else — `Reflectance` (a fixed-width geometry evaluation), the chart least-squares, the `Freeze`/`Thaw` container legs, `SpectralCurve.LuminousEfficacy`, and the archive's extent walk — the same boundary-numeric carve `surface#SPECTRAL_UPSAMPLE` `ToCurve` names. Every other operation is expression-bodied and result-threaded. Deleted whole: the hand Gauss-Newton loop, its half-step damping, its thin-`QR` step, its truncated-pseudo-inverse fallback, and its central-difference Jacobian — each re-derived what the kernel functor owns, and its differencing step size was a number this page had no basis to choose.
- Law: the forward model is SINGLE-SOURCED on `bsdf#MICROFACET_KERNEL` and GENERIC over its scalar, which is what makes the single-sourcing reachable — the synthetic capture evaluates the body at `double` and the residual row evaluates the SAME body at the solver's dual scalar, so the fit differentiates exactly what it predicts and no dual transcription exists to drift. `Reflectance` reconstructs `wi`/`wo` from `(θi, θo, Δφ)`, rotates both by `−φ` about local Z so the fitted grain axis IS the basis the anisotropic terms read (the rotate-the-lobe form `bsdf#LOBE_FAMILY` shades through), forms the true half-vector, and reads `Ndf` · `MaskingShadowing` · the discriminant-routed Fresnel over `4·|cosθi|·|cosθo|`. The deleted form is a private `D`/`G`/`F` with a `cos((θi−θo)/2)` half-angle that drops the azimuth — and Δφ is exactly what makes `αx ≠ αy` observable.
- Law: bounds ride a DIFFERENTIABLE box reparameterization `p = Lo + (Hi − Lo)·σ(u)`, never a per-iteration clamp: every iterate is interior by construction and the map is smooth everywhere, where a clamp has no derivative at its own boundary and reads a zero gradient exactly where the box binds — precisely where an in-plane capture's `αy` sits. `φ` spans a HALF turn, because an ellipse is symmetric under a half rotation and a `[0, 2π)` box carries two minima the ladder oscillates between.
- Law: OBSERVABILITY reads the capture geometry, never a second factorization of the converged Jacobian. A capture whose relative-azimuth spread is degenerate can measure neither the second alpha nor its grain axis, so the fit reports `Rank < ParameterCount` and carries no condition number. Full-rank fits alone carry a finite condition number. The witness survives ladder regularization without a non-finite sentinel.
- Law: the fitted parameters project as `Roughness = (αx·αy)^¼`, `Anisotropy = (1 − min/max)/0.9`, and `AnisotropyRotation` the fitted azimuth of the `αx` axis on the row's unit convention — the FULL inverse of the Disney aspect remap, so a brushed-metal capture round-trips its grain DIRECTION as well as its magnitude. The azimuth was always IN the residual surface; only the projection threw it away.
- Law: COLOUR CALIBRATION carries the rigour the provenance's own vocabulary claims. A neural arm stamps MEASURED on a base colour derived from a photograph nothing white-balanced, so an optional `CaptureCalibration` solves one 3x3 through a thin-`QR` LINEAR least squares — a linear correction is not a nonlinear fit, so it stays on the dense-algebra route and never reaches for the ladder — and stamps `Calibrated` beside the mean CIEDE2000 residual it left. `Macbeth`'s sRGB/D50 references rebase through `ConvertToConfiguration` FIRST so the solve runs in AP1-linear and corrects WITHIN one space rather than silently absorbing a chromatic adaptation, and it applies at the one pre-admission site every arm crosses rather than inside a single arm a sixth modality would then miss.
- Law: a MEASURED CURVE PERSISTS. Wacton's `Spd` is a one-way XYZ intake republishing no grid, so `SpectralCurve` carries the durable form and `Freeze`/`Thaw` round-trip it through a wavelength-sampled EXR part whose channel names ARE the sampling grid. `SpectralKind` rows the reflective and emissive part factories, so the leg spans BOTH claims and the `photometric#PHOTOMETRIC` measured emission SPD persists exactly as a reflectance does. A polarised part — four Stokes components per wavelength, not one sampled curve — takes NO row and refuses at the thaw. Re-ingest PARSES the container's own channel names rather than re-minting names this page would have written, and its Stokes half is not optional: parsing wavelengths alone flattens four polarisation components of one wavelength onto one channel.
- Boundary: `Acquisition.Import` is the ONE import path and `AcquiredMaterial` its ONE product. Every produced row re-admits through `graph#MATERIAL_LIBRARY` `MaterialParameters.Of`, then projects dominance only when wavelength and non-zero purity are meaningful and temperature only when Unicolour marks CCT/Duv valid. The `SvbrdfMap` arm computes a real field average in one fold — base colour in scene-linear `RgbLinear`, every Disney scalar, and `SubsurfaceRadius` per band — never a `texels.Head` first-row read. `CaptureProvenance` rides alongside the row, never as a phantom `MaterialParameters` column.
- Boundary: the `NeuralPlanes` arm consumes an ALREADY-ADMITTED `TextureSet` and never assembles one. `Rasm.Compute` executes the stage plan; the app root resolves each `StageResult.Planes` channel output — the FROZEN prior-drop projection `neural#STAGE_PLAN` owns, so a `delit` or `depth` `PriorField` reaches no set — through `Raster/codec#RASTER_CODEC` and admits the bundle through `TextureSet.Of`. This owner binds what admission already proved, so an inference product passes the same extent, transfer, convention, and payload gates a pressed set passes; the averaged row comes from the ONE `SetBind` `Average` fold and the set rides back, where the `SvbrdfMap` arm deliberately discards its field.
- Boundary: the EPFL RGL `.bsdf` decode is HAND-AUTHORED because no managed reader exists and the reference implementation's licence permits clean transcription. `BrdfArchive.Of` admits by magic word, gates each `CaptureField` row's payload against the RANK the ROW declares (never a positional roster a producer could reorder), and reads element widths off `TensorDtype` so an unadmitted dtype names itself at the field gate rather than at a cast. Two facts are DERIVED, never declared: ISOTROPY is `phi_i.Length <= 2`, since an isotropic measurement carries at most two incident-azimuth slices and a flag beside that would be a second truth; the WAVELENGTH COUNT reads off the `wavelengths` field, since published materials sample from a handful of bands to nearly two hundred and a constant admits one dataset while mis-striding every other.
- Boundary: the neural-SVBRDF `.exr` map stays the host-edge import boundary the app root owns, this owner consuming its decoded portable data; the managed MathNet provider is selected ONCE through `Control.UseManaged` (osx-arm64 has no native MKL/OpenBLAS, and a per-call-site `TryUseNativeMKL` is the named defect); the spectral grounding composes the ONE `PortValue.SceneLinear` working space, never a re-minted inline `Configuration` and never a local `SceneConfig` alias; a `codec#RASTER_FORMAT` row for a spectral part stays UNMINTED, because a wavelength grid is not a texture plane and admitting it as one puts a spectrum through extent, transfer, and mip gates that describe pixels; a malformed, empty, or out-of-gamut capture fails `MaterialFault`, never a sentinel row.
- Boundary: `CaptureProvenance` carries the MEASUREMENT half of custody and composes the contract shapes for the rest — third-party custody rides the `Raster/set#SET_INGEST` `IngestProvenance` shape ITSELF rather than a second source/licence/reference spelling, so a capture's custody and an ingested set's are ONE question read at two grains. It does NOT delete onto the contract `Rasm.Element` `PropertyEvidence`: that carrier models CITATION (source, reference, expiry, grade, attestation, run) and holds no column for an instrument, a sample count, a fit residual, a conditioning witness, a chromaticity readout, a chart delta, or a model attribution. Two carriers, two questions, one composed shape where they meet.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
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

// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct BrdfSample(double IncidentZenith, double OutgoingZenith, double AzimuthDelta, RgbSpectrum Reflectance);

public readonly record struct SpectralCurve(int StartNm, int IntervalNm, ReadOnlyMemory<double> Coefficients) {
    public static Fin<SpectralCurve> Of(int startNm, int intervalNm, ReadOnlyMemory<double> coefficients, Op key) =>
        intervalNm is not (0 or 1 or 5)
            ? new MaterialFault.Parameter(key, $"<spectral-curve-interval:{intervalNm}>")
            : startNm <= 0 || coefficients.Length < 2
                ? new MaterialFault.Parameter(key, $"<spectral-curve-extent:{startNm}@{coefficients.Length}>")
                : coefficients.Span.ContainsAnyInRange(double.NegativeInfinity, -double.Epsilon)
                    ? new MaterialFault.Parameter(key, "<spectral-curve-negative-coefficient>")
                    : Fin.Succ(new SpectralCurve(startNm, intervalNm, coefficients));

    public Spd ToSpd() => new(StartNm, IntervalNm, Coefficients.ToArray());
    public double WavelengthAt(int index) => StartNm + (index * IntervalNm);

    public double LuminousEfficacy() {
        ReadOnlySpan<double> samples = Coefficients.Span;
        (double weighted, double total) = (0.0, 0.0);
        for (int i = 0; i < samples.Length; i++) {
            (weighted, total) = (weighted + (Photopic(WavelengthAt(i)) * samples[i]), total + samples[i]);
        }
        return total > 0.0 ? Math.Clamp(weighted / total, double.Epsilon, 1.0) : 1.0;
    }

    static double Photopic(double nm) =>
        (0.821 * Lobe(nm, peak: 568.8, lower: 46.9, upper: 40.5)) + (0.286 * Lobe(nm, peak: 530.9, lower: 16.3, upper: 31.1));

    static double Lobe(double nm, double peak, double lower, double upper) =>
        (nm - peak) / (nm < peak ? lower : upper) switch { var t => Math.Exp(-0.5 * t * t) };
}

public readonly record struct CaptureCalibration(Seq<Unicolour> Patches) {
    public static Fin<CaptureCalibration> Of(Seq<Unicolour> patches, Op key) =>
        patches.Count == toSeq(Macbeth.All).Count
            ? Fin.Succ(new CaptureCalibration(patches))
            : new MaterialFault.Parameter(key, $"<capture-calibration-arity:{patches.Count}>");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureAssessment {
    private CaptureAssessment() { }
    public sealed record Fit(double Residual, int Rank, int ParameterCount, Option<double> ConditionNumber) : CaptureAssessment;
    public sealed record Inference(int Tiles, double ReferenceDeltaMax) : CaptureAssessment;
}

public sealed record ChromaticityEvidence(
    Option<(double WavelengthNm, double Purity)> Dominance,
    Option<Temperature> Temperature) {
    public static Option<ChromaticityEvidence> Of(Unicolour colour) =>
        Of(colour.DominantWavelength, colour.ExcitationPurity, colour.Temperature);

    public static Option<ChromaticityEvidence> Of(double wavelengthNm, double purity, Temperature temperature) =>
        new ChromaticityEvidence(
            Optional((WavelengthNm: wavelengthNm, Purity: purity))
                .Filter(static observed => double.IsFinite(observed.WavelengthNm)
                    && observed.WavelengthNm is >= 360.0 and <= 700.0
                    && double.IsFinite(observed.Purity) && observed.Purity is > 0.0 and <= 1.0),
            Optional(temperature)
                .Filter(static observed => observed.IsValid && double.IsFinite(observed.Cct) && double.IsFinite(observed.Duv)))
        switch {
            { Dominance.IsSome: true } observed => Some(observed),
            { Temperature.IsSome: true } observed => Some(observed),
            _ => None,
        };
}

public sealed record CaptureProvenance(string Device, CaptureMethod Method) {
    public Option<int> WavelengthCount { get; init; }
    public Option<int> AngularSamples { get; init; }
    public Option<CaptureAssessment> Assessment { get; init; }
    public Option<ChromaticityEvidence> Chromaticity { get; init; }

    public Option<double> CalibrationDeltaE { get; init; }
    public bool Calibrated { get; init; }

    public Option<ModelCardId> ModelCard { get; init; }
    public Option<LicenseClass> License { get; init; }

    public Option<IngestProvenance> Ingest { get; init; }

    public bool Measured => Method != CaptureMethod.Authored;

    public static readonly CaptureProvenance Authored = new("authored", CaptureMethod.Authored);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CaptureSource {
    private CaptureSource() { }

    public sealed record MeasuredBrdf(Seq<BrdfSample> Samples, double Ior, Option<ComplexIor> Conductor) : CaptureSource;
    public sealed record SvbrdfMap(Seq<MaterialParameters> Texels) : CaptureSource;
    public sealed record SpectralReflectance(SpectralCurve Reflectance, double Metalness, double Roughness) : CaptureSource;

    public sealed record NeuralPlanes(TextureSet Planes, Seq<StageResult> Stages, MaterialParameters Fallback) : CaptureSource;

    public sealed record GlossMeter(double Gu20, double Gu60, double Gu85, GlossCurve Curve, MaterialParameters Base) : CaptureSource;
}

public readonly record struct GlossCurve(double HighBand, double MatteBand, Func<double, double> Roughness) {
    public double Of(double gu20, double gu60, double gu85) =>
        Roughness(gu60 >= HighBand ? gu20 : gu60 <= MatteBand ? gu85 : gu60);
}

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

    static ReadOnlyMemory<double> Lift<TStored>(ReadOnlyMemory<byte> run, Func<TStored, double> project) where TStored : struct =>
        (ReadOnlyMemory<double>)[.. MemoryMarshal.Cast<byte, TStored>(run.Span).ToArray().Select(project)];
}

public sealed record BrdfArchive(HashMap<CaptureField, ReadOnlyMemory<double>> Fields, HashMap<CaptureField, Seq<int>> Extents) {
    public const ulong Magic = 0x00465344425F4C54;

    public bool Isotropic => Fields.Find(CaptureField.PhiI).Map(static phi => phi.Length <= 2).IfNone(true);
    public int WavelengthCount => Fields.Find(CaptureField.Wavelengths).Map(static grid => grid.Length).IfNone(0);

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
        return fields.Find(CaptureField.ThetaI).IsSome && fields.Find(CaptureField.Rgb).IsSome
            ? Fin.Succ(new BrdfArchive(fields, extents))
            : new MaterialFault.Parameter(key, "<brdf-archive-incomplete>");
    }

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

    public Fin<SpectralCurve> Spectra(Op key) =>
        (Fields.Find(CaptureField.Wavelengths), Fields.Find(CaptureField.Spectra)) switch {
            (var grid, var spectra) when grid.IsSome && spectra.IsSome && WavelengthCount > 1 =>
                grid.IfNone(default) switch {
                    var nm => SpectralCurve.Of((int)nm.Span[0], (int)Math.Round(nm.Span[1] - nm.Span[0]), spectra.IfNone(default)[..WavelengthCount], key),
                },
            _ => new MaterialFault.Parameter(key, $"<brdf-archive-no-spectra:{WavelengthCount}>"),
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Acquisition {
    static Acquisition() => Control.UseManaged();

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
            Chromaticity = ChromaticityEvidence.Of(row.BaseColor) } }));

    static Fin<AcquiredMaterial> FitGloss(CaptureSource.GlossMeter c, CaptureProvenance provenance, Op key) =>
        double.IsFinite(c.Gu20) && double.IsFinite(c.Gu60) && double.IsFinite(c.Gu85)
        && c.Gu20 >= 0.0 && c.Gu60 >= 0.0 && c.Gu85 >= 0.0
            ? c.Curve.Of(c.Gu20, c.Gu60, c.Gu85) switch {
                var roughness when double.IsFinite(roughness) && roughness is >= 0.0 and <= 1.0 =>
                    Fin.Succ(AcquiredMaterial.Summary(
                        c.Base with { Roughness = roughness },
                        provenance with {
                            Method = CaptureMethod.GlossMeter,
                            AngularSamples = Some(3),
                            Assessment = None,
                        })),
                var roughness => new MaterialFault.Parameter(key, $"<gloss-curve-out-of-unit:{roughness:R}>"),
            }
            : new MaterialFault.Parameter(key, $"<gloss-reading-out-of-range:{c.Gu20:R},{c.Gu60:R},{c.Gu85:R}>");

    static Fin<AcquiredMaterial> Calibrate(AcquiredMaterial acquired, Option<CaptureCalibration> calibration, Op key) =>
        calibration
            .TraverseM(chart => Solve(chart, key).Map(fit => acquired with {
                Row = acquired.Row with { BaseColor = Correct(acquired.Row.BaseColor, fit.Matrix) },
                Provenance = acquired.Provenance with { Calibrated = true, CalibrationDeltaE = Some(fit.DeltaE) } })).As()
            .Map(result => result.IfNone(acquired));

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

    static Fin<ReadOnlyMemory<double>> Samples(Part part, Op key) =>
        part.GetLevel(0, 0) switch {
            var level => toSeq(toSeq(level.Channels)
                .Filter(Spectral.IsSpectralChannel)
                .Choose(name => Spectral.TryParseChannelWavelength(name, out float nm)
                             && Spectral.TryGetStokesComponent(name, out int stokes) && stokes == 0
                                ? Some((Nm: nm, Name: name))
                                : Option<(float Nm, string Name)>.None)
                .OrderBy(static channel => channel.Nm))
                .Map(channel => (double)MemoryMarshal.Cast<byte, float>(level.GetChannel(channel.Name).Data.Span)[0]) switch {
                var samples => samples.IsEmpty
                    ? Fin.Fail<ReadOnlyMemory<double>>(new MaterialFault.Parameter(key, "<spectral-thaw-no-channels>"))
                    : Fin.Succ((ReadOnlyMemory<double>)samples.ToArray()),
            },
        };

    static Fin<AcquiredMaterial> ImportPlanes(TextureSet planes, Seq<StageResult> stages, MaterialParameters fallback, CaptureProvenance provenance, Op key) =>
        stages.IsEmpty
            ? new MaterialFault.Parameter(key, "<neural-planes-no-stage-evidence>")
            : from binding in SetBind.Bind(planes, fallback, BindTarget.Summary, SamplerState.Default, key)
              from row in binding is SetBinding.Row bound
                  ? Fin.Succ(bound.Parameters)
                  : Fin.Fail<MaterialParameters>(new MaterialFault.Graph(key, "<neural-planes-summary-not-a-row>"))
              select new AcquiredMaterial(row, Attributed(provenance, planes, stages), Some(planes));

    static CaptureProvenance Attributed(CaptureProvenance provenance, TextureSet planes, Seq<StageResult> stages) {
        Option<ModelCard> governing = stages
            .Choose(static result => ModelRegistry.Rows.TryGetValue(result.ModelCardId, out ModelCard? card) ? Some(card!) : Option<ModelCard>.None)
            .Fold(Option<ModelCard>.None, static (strictest, card) => strictest.Filter(held => held.License.Rank >= card.License.Rank).IsSome ? strictest : Some(card));
        int tiles = stages.Fold(0, static (total, result) => total + result.TilesEmitted);
        double delta = stages.Fold(0.0, static (worst, result) => Math.Max(worst, result.ReferenceDelta));
        return provenance with {
            Device = planes.Digest.ToValue(),
            Method = CaptureMethod.NeuralPlanes,
            AngularSamples = None,
            Assessment = Some<CaptureAssessment>(new CaptureAssessment.Inference(tiles, delta)),
            ModelCard = governing.Map(static card => card.Id),
            License = governing.Map(static card => card.License),
        };
    }

    public static Fin<Seq<BrdfSample>> SyntheticGrid(int seed, int count, Op key) {
        if (count < 1) { return new MaterialFault.Parameter(key, $"<synthetic-grid-count:{count}>"); }
        var (alphaX, alphaY) = (0.1 + 0.4 * Draw(seed, 0), 0.1 + 0.4 * Draw(seed, 1));
        double rotation = Draw(seed, 2) * double.Pi;
        int lanes = int.Max((int)double.Ceiling(double.Sqrt(count)), 1);
        return Fin.Succ(toSeq(Enumerable.Range(0, count).Select(index => {
            double thetaI = (index / lanes + Draw(seed, 2L * index + 3L)) / lanes * (double.Pi / 2.0 - 0.02);
            double thetaO = (index % lanes + Draw(seed, 2L * index + 4L)) / lanes * (double.Pi / 2.0 - 0.02);
            double deltaPhi = (index + 0.5) / count * double.Pi;
            var sample = new BrdfSample(thetaI, thetaO, deltaPhi, RgbSpectrum.Black);
            return sample with { Reflectance = RgbSpectrum.White.Scale(Reflectance(sample, alphaX, alphaY, rotation, eta: 1.5, conductor: None)) };
        })));
    }

    static double Draw(int seed, long lane) => Deterministic.Unit(lanes: [lane], seed: seed);

    static Fin<AcquiredMaterial> FitBrdf(
        Seq<BrdfSample> samples, double ior, Option<ComplexIor> conductor, CaptureProvenance provenance, Context context, Op key) =>
        guard(samples.Count > (conductor.IsSome ? 3 : 4), new MaterialFault.Parameter(key, $"<measured-brdf-underdetermined:{samples.Count}>")).ToFin()
            .Bind(_ => SolveGgx(samples, ior, conductor, context, key))
            .Bind(fit => SpectralUpsample.ToSpd(
                    samples.Map(static s => s.Reflectance).Fold(RgbSpectrum.Black, static (acc, r) => acc.Add(r)).Scale(1.0 / samples.Count), key)
                .Bind(spd => GroundSpectral(spd, metalness: conductor.IsSome ? 1.0 : 0.0, fit.Roughness, key))
                .Map(row => AcquiredMaterial.Summary(
                             row with { Ior = fit.Eta, Anisotropy = fit.Anisotropy, AnisotropyRotation = fit.Rotation },
                             provenance with {
                                 Method = CaptureMethod.Goniophotometer,
                                 AngularSamples = Some(samples.Count),
                                 Assessment = Some<CaptureAssessment>(new CaptureAssessment.Fit(
                                     fit.Residual, fit.Rank, fit.ParameterCount, fit.ConditionNumber)),
                             })));

    readonly record struct BrdfFit(
        double Roughness, double Anisotropy, double Rotation, double Eta,
        double Residual, Option<double> ConditionNumber, int Rank, int ParameterCount);

    static Fin<BrdfFit> SolveGgx(Seq<BrdfSample> samples, double ior0, Option<ComplexIor> conductor, Context context, Op key) =>
        new BrdfResidual(samples, ior0, conductor) switch {
            var residual => SolvePolicy.Of(context: context, key: key)
                .Bind(ladder => Lm.Minimize(new DualModel(residual), ladder, key))
                .Bind(result => residual.Project(result, key)),
        };

    sealed class BrdfResidual(Seq<BrdfSample> samples, double ior0, Option<ComplexIor> conductor) : IDualResidual {
        static readonly (double Lo, double Hi)[] AlphaPhi = [(1e-4, 1.0), (1e-4, 1.0), (0.0, Math.PI)];
        static readonly (double Lo, double Hi)[] AlphaPhiEta = [.. AlphaPhi, (1.0, 2.5)];

        internal (double Lo, double Hi)[] Bounds => conductor.IsSome ? AlphaPhi : AlphaPhiEta;
        public int Dof => Bounds.Length;
        public int Rows => samples.Count;

        public double[] Seed => [.. Bounds.Zip(
            conductor.IsSome
                ? [Microfacet<double>.AlphaOf(0.3), Microfacet<double>.AlphaOf(0.3), 0.0]
                : [Microfacet<double>.AlphaOf(0.3), Microfacet<double>.AlphaOf(0.3), 0.0, Math.Clamp(ior0, 1.0, 2.5)],
            static (box, value) => Free(value, box))];

        public Dual<ddouble> Row(int index, ReadOnlySpan<Dual<ddouble>> parameters) =>
            Dual<ddouble>.Of(ddouble.Log(ddouble.Max(Floor, (ddouble)samples[index].Reflectance.Luminance)))
            - Dual<ddouble>.Log(Model(samples[index], parameters));

        Dual<ddouble> Model(BrdfSample sample, ReadOnlySpan<Dual<ddouble>> u) =>
            Reflectance(sample,
                alphaX:   Bound(u[0], Bounds[0]), alphaY: Bound(u[1], Bounds[1]), rotation: Bound(u[2], Bounds[2]),
                eta:      u.Length == 4 ? Bound(u[3], Bounds[3]) : Dual<ddouble>.Of((ddouble)1.5),
                conductor: conductor);

        internal Fin<BrdfFit> Project(LmResult result, Op key) =>
            result.IsValid
                ? [.. Bounds.Zip(result.Parameters, static (box, free) => Bound(free, box))] switch {
                    var p => Fin.Succ(new BrdfFit(
                        Roughness: Math.Sqrt(Math.Sqrt(p[0] * p[1])),
                        Anisotropy: Math.Clamp((1.0 - (Math.Min(p[0], p[1]) / Math.Max(p[0], p[1]))) / 0.9, 0.0, 1.0),
                        Rotation: Math.Clamp(((p[2] + (p[0] >= p[1] ? 0.0 : Math.PI / 2.0)) / Math.PI) % 1.0, 0.0, 1.0),
                        Eta: p.Length == 4 ? p[3] : 1.5,
                        Residual: result.Norm / Math.Max(1e-6, Scale),
                        ConditionNumber: Observable
                            ? Some(Math.Max(1.0, 1.0 / Math.Max(1e-12, AzimuthSpread)))
                            : None,
                        Rank: Observable ? Dof : Dof - 2,
                        ParameterCount: Dof))
                }
                : new MaterialFault.Parameter(key, "<ggx-fit-diverged>");

        double AzimuthSpread => samples.Fold((Lo: double.MaxValue, Hi: double.MinValue),
            static (span, s) => (Math.Min(span.Lo, s.AzimuthDelta), Math.Max(span.Hi, s.AzimuthDelta))) switch {
            var span => span.Hi - span.Lo,
        };
        bool Observable => AzimuthSpread > 1e-3;

        double Scale => Math.Sqrt(samples.Fold(0.0, static (sum, s) =>
            Math.Log(Math.Max(1e-6, s.Reflectance.Luminance)) switch { var l => sum + (l * l) }));

        static readonly ddouble Floor = (ddouble)1e-6;

        static Dual<ddouble> Bound(Dual<ddouble> free, (double Lo, double Hi) box) =>
            Dual<ddouble>.Of((ddouble)box.Lo)
            + (Dual<ddouble>.Of((ddouble)(box.Hi - box.Lo))
               / (Dual<ddouble>.Of(ddouble.One) + Dual<ddouble>.Exp(-free)));
        static double Bound(double free, (double Lo, double Hi) box) => box.Lo + ((box.Hi - box.Lo) / (1.0 + Math.Exp(-free)));
        static double Free(double bounded, (double Lo, double Hi) box) =>
            Math.Clamp((bounded - box.Lo) / (box.Hi - box.Lo), 1e-9, 1.0 - 1e-9) switch { var t => Math.Log(t / (1.0 - t)) };
    }

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

    static T Luminance<T>(ComplexIor ior, T cosI)
        where T : INumber<T>, IRootFunctions<T>, IPowerFunctions<T>, IExponentialFunctions<T>, ILogarithmicFunctions<T>, ITrigonometricFunctions<T> =>
        (T.CreateChecked(RgbSpectrum.LuminanceWeights.R) * Microfacet<T>.FresnelConductor(cosI, T.CreateChecked(ior.Eta.R), T.CreateChecked(ior.K.R)))
        + (T.CreateChecked(RgbSpectrum.LuminanceWeights.G) * Microfacet<T>.FresnelConductor(cosI, T.CreateChecked(ior.Eta.G), T.CreateChecked(ior.K.G)))
        + (T.CreateChecked(RgbSpectrum.LuminanceWeights.B) * Microfacet<T>.FresnelConductor(cosI, T.CreateChecked(ior.Eta.B), T.CreateChecked(ior.K.B)));

    static Fin<AcquiredMaterial> AverageField(Seq<MaterialParameters> texels, CaptureProvenance provenance, Op key) =>
        texels.IsEmpty
            ? new MaterialFault.Parameter(key, "<svbrdf-map-empty>")
            : Fin.Succ(AcquiredMaterial.Summary(
                        texels.Fold(FieldSum.Zero, static (acc, t) => acc.Add(t)).Mean(texels.Count),
                        provenance with { Method = CaptureMethod.NeuralSvbrdf, AngularSamples = Some(texels.Count) }));

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

    static Fin<MaterialParameters> GroundSpectral(Spd reflectance, double metalness, double roughness, Op key) =>
        from color in Fin.Succ(new Unicolour(PortValue.SceneLinear, reflectance))
        from _ in guard(GamutPolicy.Perceptual.Contains(color), new MaterialFault.Gamut(key, "<acquired-color-out-of-gamut>"))
        from grounded in SpectralUpsample.SceneLinear(color, key)
        select new MaterialParameters(
            BaseColor: Linear(grounded.R, grounded.G, grounded.B), Metalness: metalness, Roughness: roughness, SpecularTint: 0.0, Anisotropy: 0.0, Ior: 1.5,
            Transmission: 0.0, TransmissionRoughness: 0.0, Sheen: 0.0, SheenTint: 0.0, Clearcoat: 0.0, ClearcoatRoughness: 0.0,
            Subsurface: 0.0, SubsurfaceRadius: SubsurfaceRadius.None, Emission: Linear(0.0, 0.0, 0.0), EmissionLuminance: 0.0);

    static Unicolour Linear(double r, double g, double b) => new(PortValue.SceneLinear, ColourSpace.RgbLinear, r, g, b);
}
```

## [03]-[RESEARCH]

(none)

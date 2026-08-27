# [RASM_FABRICATION_IMPLICIT]

Implicit additive geometry admits one compatible operation set containing periodic fields, lattices, content-addressed voxel wires, or VDB sources; binds one operation-scoped PicoGK runtime; calibrates physical distance; consumes every native handle inside the lease; and projects only durable results. `FieldExpression` generates the periodic space and differentiates itself, `SpectralExpression` generates the frequency-domain space the same way, `FieldDefinition.Generated` carries per-occurrence programs without mutating `FieldKind.Items`, and `Sdf.Voxelize` scopes every native handle to one callback.

Wire posture is host-local: `Voxels`, `Lattice`, `Mesh`, `ScalarField`, `VectorField`, `OpenVdbFile`, and `Library.GlobalInstance` never cross a result boundary. Every provider selector this page admits is an OWNED `[SmartEnum<string>]` row carrying its PicoGK enum as a column, so `FabricationCanon.Discriminant` writes the row's own key into a preimage and a provider reordering its enum members re-keys nothing. Support geometry composes the ONE `SupportTopology` `support#SUPPORT_TOPOLOGY` publishes on `SupportPlan`; this page reconstructs no edge set. Refusals ride `FabricationFault` through `Admission.Admitted`, and every native failure carries the provider's own cause forward because the provider owns that taxonomy.

## [01]-[INDEX]

- [02]-[FIELD_PROGRAM]: `FieldKey`, `VoxelSample`, `FieldExpression`, `FieldKind`, `FieldDefinition`, `TpmsForm`, `ImplicitCell`, `CalibrationPolicy`, `CalibrationStats`, `FieldThreshold`.
- [03]-[SPECTRAL_ALGEBRA]: `SpectralShape`, `SpectralMetric`, `SpectralExpression`, `SpectralSymbol`.
- [04]-[MORPHOLOGY]: `VoxelMorphologyStep`, `VoxelBoolean`, the one `Apply` entrypoint, `SpectralMorphology`, `FilteredField`.
- [05]-[PROVIDER_VOCABULARY]: `SliceRender`, `SliceAxis`, `MaskSampling`, `CliFormat`, `ContourWinding`, `RasterFrame`.
- [06]-[OPERATION_SET]: `VdbSource`, `CliMode`, `ImplicitPolicy`, `VoxelWire`, `VoxelOperationKind`, `ImplicitOp`.
- [07]-[VOXEL_LEASE]: `VoxelRuntime`, `VoxelMetrics`, `VoxelScope`, `Rasterized`, `Sdf.Voxelize` and its build fold.
- [08]-[LAYER_EGRESS]: `CliImport`, `CliStack`, `Sdf.Cli` and the three egress lanes.
- [09]-[CANONICAL_BYTES]: `ImplicitCanonical`.

## [02]-[FIELD_PROGRAM]

- Owner: `FieldExpression` owns constant, wave, sum, product, minimum, maximum, and absolute program generation together with the closed-form gradient each case carries; `FieldKind` owns the common seed programs; `FieldDefinition` owns known and generated level-set programs; `ImplicitCell` owns the orthotropic period metric and the density, orientation, and scale drivers; `FieldCalibration` owns density quantiles and calibration evidence.
- Cases: `TpmsForm` splits solid from sheet, the sheet carrying the printable wall band its half-width clamps into.
- Law: `FieldExpression.At` returns level and gradient from ONE structural fold, so no sampling stencil, step size, or truncation error enters the distance law. Density grades wall thickness, axis grades orientation, and scale grades the period itself — three independent drivers a conformal lattice varies, so folding scale into density collapses two of them.
- Exemption: `FieldCalibration.Of` and `SampleAction.Invoke` are numeric kernels — pooled parallel partition fills over `ParallelHelper`, tensor reductions, and in-place sorts have no expression form.
- Entry: `FieldDefinition.Admit` is the one construction for both arms; `FieldCalibration.Of` is the one quantile pass.
- Auto: every generated owner refuses through `[ValidationError]`, so one `Admission.Admitted` read closes each admission and no hand ternary restates the `Validate` contract. `Resolution` derives the sample cube from the policy's own quantile-error target rather than a spelled grid.
- Packages: `System.Numerics.Tensors` (finite checks, extrema, moments, energy, subtraction, absolute transforms), `CommunityToolkit.HighPerformance` (`MemoryOwner<T>`, `SpanOwner<T>`, `Span2D<T>`, `ParallelHelper.For` over `struct IAction`), `UnitsNet`, LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a topology is `FieldExpression` data, a common topology one `FieldKind` seed row, a per-occurrence topology one `FieldDefinition.Generated`, and a spatial driver one `ImplicitCell` field column.
- Boundary: raw level equations never claim signed-distance semantics — the distance law divides the residual by the world gradient norm floored at the policy's own gradient floor, so a level set whose gradient vanishes reports a bounded distance rather than an infinity.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Numerics;
using System.Numerics.Tensors;
using System.Text;
using System.Threading;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using LanguageExt.Traits;
using LanguageExt.UnsafeValueAccess;
using PicoGK;
using QuikGraph;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Additive;

// --- [TYPES] ---------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
public readonly partial struct FieldKey {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        value = value.Trim();
        if (!Witness.Keyed(value))
            validationError = new ValidationError("field-key");
    }

    public static Fin<FieldKey> Admit(string value) => Admission.OfValue<FieldKey, string>(value);
}

public readonly record struct VoxelSample(float Level, Vector3 Gradient);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldExpression {
    private FieldExpression() { }

    public sealed record Constant(float Value) : FieldExpression;
    public sealed record Wave(float Amplitude, Vector3 Frequency, float Phase) : FieldExpression;
    public sealed record Sum(Arr<FieldExpression> Terms) : FieldExpression;
    public sealed record Product(Arr<FieldExpression> Factors) : FieldExpression;
    public sealed record Minimum(Arr<FieldExpression> Terms) : FieldExpression;
    public sealed record Maximum(Arr<FieldExpression> Terms) : FieldExpression;
    public sealed record Absolute(FieldExpression Term) : FieldExpression;

    public VoxelSample At(Vector3 phase) => Switch(
        state: phase,
        constant: static (_, expression) => new VoxelSample(expression.Value, Vector3.Zero),
        wave: static (value, expression) =>
            (Vector3.Dot(expression.Frequency, value) + expression.Phase) switch {
                var angle => new VoxelSample(
                    expression.Amplitude * MathF.Cos(angle),
                    -expression.Amplitude * MathF.Sin(angle) * expression.Frequency),
            },
        sum: static (value, expression) => expression.Terms.Fold(
            new VoxelSample(0.0f, Vector3.Zero),
            (total, term) => term.At(value) switch {
                var sample => new VoxelSample(total.Level + sample.Level, total.Gradient + sample.Gradient),
            }),
        product: static (value, expression) => expression.Factors.Fold(
            new VoxelSample(1.0f, Vector3.Zero),
            (total, term) => term.At(value) switch {
                var sample => new VoxelSample(
                    total.Level * sample.Level,
                    (total.Gradient * sample.Level) + (sample.Gradient * total.Level)),
            }),
        minimum: static (value, expression) => expression.Terms.Fold(
            new VoxelSample(float.PositiveInfinity, Vector3.Zero),
            (held, term) => term.At(value) switch {
                var sample => sample.Level < held.Level ? sample : held,
            }),
        maximum: static (value, expression) => expression.Terms.Fold(
            new VoxelSample(float.NegativeInfinity, Vector3.Zero),
            (held, term) => term.At(value) switch {
                var sample => sample.Level > held.Level ? sample : held,
            }),
        absolute: static (value, expression) => expression.Term.At(value) switch {
            var sample => new VoxelSample(
                MathF.Abs(sample.Level),
                sample.Level < 0.0f ? -sample.Gradient : sample.Gradient),
        });

    public bool Valid => Switch(
        constant: static expression => float.IsFinite(expression.Value),
        wave: static expression => float.IsFinite(expression.Amplitude)
            && float.IsFinite(expression.Frequency.X)
            && float.IsFinite(expression.Frequency.Y)
            && float.IsFinite(expression.Frequency.Z)
            && float.IsFinite(expression.Phase),
        sum: static expression => !expression.Terms.IsEmpty && expression.Terms.ForAll(static term => term.Valid),
        product: static expression => !expression.Factors.IsEmpty && expression.Factors.ForAll(static term => term.Valid),
        minimum: static expression => !expression.Terms.IsEmpty && expression.Terms.ForAll(static term => term.Valid),
        maximum: static expression => !expression.Terms.IsEmpty && expression.Terms.ForAll(static term => term.Valid),
        absolute: static expression => expression.Term.Valid);
}

[SmartEnum<string>]
public sealed partial class FieldKind {
    private const float LidinoidOffset = 0.15f;
    private const float QuarterPeriod = -0.5f * MathF.PI;

    private static readonly FieldExpression SinX = Sine(Vector3.UnitX);
    private static readonly FieldExpression SinY = Sine(Vector3.UnitY);
    private static readonly FieldExpression SinZ = Sine(Vector3.UnitZ);
    private static readonly FieldExpression CosX = Cosine(Vector3.UnitX);
    private static readonly FieldExpression CosY = Cosine(Vector3.UnitY);
    private static readonly FieldExpression CosZ = Cosine(Vector3.UnitZ);

    public static readonly FieldKind Gyroid = new("gyroid", Add(
        Multiply(SinX, CosY), Multiply(SinY, CosZ), Multiply(SinZ, CosX)));
    public static readonly FieldKind SchwarzP = new("schwarz-p", Add(CosX, CosY, CosZ));
    public static readonly FieldKind SchwarzD = new("schwarz-d", Add(
        Multiply(SinX, SinY, SinZ),
        Multiply(SinX, CosY, CosZ),
        Multiply(CosX, SinY, CosZ),
        Multiply(CosX, CosY, SinZ)));
    public static readonly FieldKind Neovius = new("neovius", Add(
        Scale(3.0f, Add(CosX, CosY, CosZ)),
        Scale(4.0f, Multiply(CosX, CosY, CosZ))));
    public static readonly FieldKind Lidinoid = new("lidinoid", Add(
        Scale(0.5f, Add(
            Multiply(Sine(2.0f * Vector3.UnitX), CosY, SinZ),
            Multiply(Sine(2.0f * Vector3.UnitY), CosZ, SinX),
            Multiply(Sine(2.0f * Vector3.UnitZ), CosX, SinY))),
        Scale(-0.5f, Add(
            Multiply(Cosine(2.0f * Vector3.UnitX), Cosine(2.0f * Vector3.UnitY)),
            Multiply(Cosine(2.0f * Vector3.UnitY), Cosine(2.0f * Vector3.UnitZ)),
            Multiply(Cosine(2.0f * Vector3.UnitZ), Cosine(2.0f * Vector3.UnitX)))),
        new FieldExpression.Constant(LidinoidOffset)));
    public static readonly FieldKind Cellular = new("cellular", Minimum(
        Absolute(Multiply(SinX, SinY)),
        Absolute(Multiply(SinY, SinZ)),
        Absolute(Multiply(SinZ, SinX))));

    public FieldExpression Program { get; }

    private static FieldExpression Sine(Vector3 frequency) => new FieldExpression.Wave(1.0f, frequency, QuarterPeriod);

    private static FieldExpression Cosine(Vector3 frequency) => new FieldExpression.Wave(1.0f, frequency, 0.0f);

    private static FieldExpression Add(params ReadOnlySpan<FieldExpression> terms) =>
        new FieldExpression.Sum(toArr(terms.ToArray()));

    private static FieldExpression Multiply(params ReadOnlySpan<FieldExpression> factors) =>
        new FieldExpression.Product(toArr(factors.ToArray()));

    private static FieldExpression Minimum(params ReadOnlySpan<FieldExpression> terms) =>
        new FieldExpression.Minimum(toArr(terms.ToArray()));

    private static FieldExpression Absolute(FieldExpression term) => new FieldExpression.Absolute(term);

    private static FieldExpression Scale(float factor, FieldExpression term) =>
        Multiply(new FieldExpression.Constant(factor), term);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FieldDefinition {
    private FieldDefinition() { }

    public sealed record Known(FieldKind Kind) : FieldDefinition;
    public sealed record Generated(FieldKey Key, FieldExpression Program) : FieldDefinition;

    public static Fin<FieldDefinition> Admit(string key) =>
        Admission.Of<FieldKind, string>(key).Map(static kind => (FieldDefinition)new Known(kind));

    public static Fin<FieldDefinition> Admit(FieldKey key, FieldExpression program) =>
        (AdmissionSlots.Gate(program.Valid, FabConcern.Additive, "implicit-field:generated-program", FabricationFault.Inadmissible),
         AdmissionSlots.Gate(!FieldKind.TryGet(key.Value, out _),
             FabConcern.Additive, "implicit-field:generated-shadows-seed", FabricationFault.Inadmissible))
            .Apply(static (_, _) => unit)
            .As()
            .ToFin()
            .Map(_ => (FieldDefinition)new Generated(key, program));

    public FieldKey Identity => Switch(
        known: static definition => FieldKey.Create(definition.Kind.Key),
        generated: static definition => definition.Key);

    public VoxelSample At(Vector3 phase) => Switch(
        state: phase,
        known: static (value, definition) => definition.Kind.Program.At(value),
        generated: static (value, definition) => definition.Program.At(value));

}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TpmsForm {
    private TpmsForm() { }

    public sealed record Solid : TpmsForm;
    public sealed record Sheet(Length MinimumWall, Length MaximumWall) : TpmsForm;

    public float Distance(float residual, float gradientNorm, FieldThreshold threshold) => Switch(
        state: (Residual: residual, GradientNorm: gradientNorm, Threshold: threshold),
        solid: static (state, _) => state.Residual / state.GradientNorm,
        sheet: static (state, form) => MathF.Abs(state.Residual / state.GradientNorm)
            - (float)Math.Clamp(
                state.Threshold.HalfWidth.Millimeters,
                0.5 * form.MinimumWall.Millimeters,
                0.5 * form.MaximumWall.Millimeters));

    public bool Valid => Switch(
        solid: static () => true,
        sheet: static form => double.IsFinite(form.MinimumWall.Millimeters)
            && double.IsFinite(form.MaximumWall.Millimeters)
            && form.MinimumWall.Millimeters > 0.0
            && form.MaximumWall.Millimeters >= form.MinimumWall.Millimeters);
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class CalibrationPolicy {
    private const int SampleFloor = 8;
    private const double DensityCeiling = 0.5;

    public int MinimumSamples { get; }
    public int MaximumSamples { get; }
    public Ratio QuantileError { get; }
    public Ratio DensityFloor { get; }
    public double GradientFloorPerMillimeter { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int minimumSamples,
        ref int maximumSamples,
        ref Ratio quantileError,
        ref Ratio densityFloor,
        ref double gradientFloorPerMillimeter) {
        if (minimumSamples < SampleFloor
            || maximumSamples < minimumSamples
            || quantileError.DecimalFractions is <= 0.0 or >= 1.0
            || densityFloor.DecimalFractions <= 0.0
            || densityFloor.DecimalFractions >= DensityCeiling
            || !ValidityClaim.Positive(gradientFloorPerMillimeter).Holds)
            validationError = new ValidationError("calibration-policy");
    }

    public static Fin<CalibrationPolicy> Admit(
        int minimumSamples,
        int maximumSamples,
        Ratio quantileError,
        Ratio densityFloor,
        double gradientFloorPerMillimeter) =>
        Validate(minimumSamples, maximumSamples, quantileError, densityFloor, gradientFloorPerMillimeter,
            out CalibrationPolicy policy).Admitted(policy);
}

[ComplexValueObject]
public sealed partial class ImplicitCell {
    public Length PeriodX { get; }
    public Length PeriodY { get; }
    public Length PeriodZ { get; }
    public Matrix4x4 WorldToCell { get; }
    public Ratio RelativeDensity { get; }
    public Ratio FrameTolerance { get; }
    public Ratio MinimumScale { get; }

    public Option<Func<Fin<ScalarField>>> DensityField { get; }
    public Option<Func<Fin<VectorField>>> AxisField { get; }
    public Option<Func<Fin<ScalarField>>> ScaleField { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref Length periodX,
        ref Length periodY,
        ref Length periodZ,
        ref Matrix4x4 worldToCell,
        ref Ratio relativeDensity,
        ref Ratio frameTolerance,
        ref Ratio minimumScale,
        ref Option<Func<Fin<ScalarField>>> densityField,
        ref Option<Func<Fin<VectorField>>> axisField,
        ref Option<Func<Fin<ScalarField>>> scaleField) {
        if (!(Arr(periodX, periodY, periodZ).ForAll(static period => ValidityClaim.Positive(period.Millimeters))
            && frameTolerance.DecimalFractions is > 0.0 and < 1.0
            && minimumScale.DecimalFractions is > 0.0 and <= 1.0
            && Math.Abs(worldToCell.GetDeterminant()) > frameTolerance.DecimalFractions
            && relativeDensity.DecimalFractions is > 0.0 and < 1.0))
            validationError = new ValidationError("implicit-cell");
    }

    public static Fin<ImplicitCell> Admit(
        Length periodX,
        Length periodY,
        Length periodZ,
        Matrix4x4 worldToCell,
        Ratio relativeDensity,
        Ratio frameTolerance,
        Ratio minimumScale,
        Option<Func<Fin<ScalarField>>> densityField = default,
        Option<Func<Fin<VectorField>>> axisField = default,
        Option<Func<Fin<ScalarField>>> scaleField = default) =>
        Validate(periodX, periodY, periodZ, worldToCell, relativeDensity, frameTolerance, minimumScale,
            densityField, axisField, scaleField, out ImplicitCell cell).Admitted(cell);

    public Vector3 Phase(Vector3 world, Option<Vector3> axis, Ratio scale) =>
        Vector3.Transform(world, Frame(axis)) switch {
            var local => Vector3.Multiply(local, Wavenumber(scale)),
        };

    public Vector3 WorldGradient(Vector3 phaseGradient, Option<Vector3> axis, Ratio scale) =>
        Vector3.Multiply(phaseGradient, Wavenumber(scale)) switch {
            var scaled => Vector3.TransformNormal(scaled, Matrix4x4.Transpose(Frame(axis))),
        };

    private Matrix4x4 Frame(Option<Vector3> axis) =>
        axis.Map(value => FieldMath.AxisFrame(value, (float)FrameTolerance.DecimalFractions))
            .IfNone(Matrix4x4.Identity) * WorldToCell;

    private Vector3 Wavenumber(Ratio scale) =>
        (float)Math.Max(scale.DecimalFractions, MinimumScale.DecimalFractions) switch {
            var factor => new Vector3(
                MathF.Tau / ((float)PeriodX.Millimeters * factor),
                MathF.Tau / ((float)PeriodY.Millimeters * factor),
                MathF.Tau / ((float)PeriodZ.Millimeters * factor)),
        };
}

public readonly record struct CalibrationStats(
    float Minimum,
    float Maximum,
    float Average,
    float StandardDeviation,
    float SumOfSquares,
    int MinimumIndex,
    int MaximumIndex,
    int Samples);

public readonly record struct FieldThreshold(float Iso, Length HalfWidth, CalibrationStats Stats);
```

## [03]-[SPECTRAL_ALGEBRA]

- Owner: `SpectralExpression` owns the frequency-domain program and its per-bin evaluation; `SpectralMetric` owns the three frequency measures a symbol reads; `SpectralSymbol` owns the named laws over those programs.
- Law: a spectral law is DATA on the same generative method `[02]-[FIELD_PROGRAM]` proves for the spatial domain — a closure carries no structure a preimage can digest and no term a sibling law can share, so a directional blur, a band-pass, and a derivative response are three PROGRAMS over one algebra rather than three hand bodies. A new frequency law is one `SpectralSymbol` row spelling an expression, and it needs no arm anywhere.
- Cases: `Gaussian` carries its metric and decay coefficient, `Derivative` its order, `Product` its factors, and `Constant` a fixed response; the metric absorbs the shape pair, so a decay coefficient stays a pure convention constant.
- Auto: the three landed rows reproduce their closed forms exactly — an anisotropic Gaussian is `Gaussian(Anisotropic, GaussianDecay)`, a radial band-pass is `Gaussian(Offset, UnitDecay)`, and a gradient-magnitude response is `Product(Derivative(1), Gaussian(Scaled, GaussianDecay))`.
- Packages: BCL inbox (`System.Numerics.Complex`), `Rhino.Geometry` value vectors, Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new frequency measure is one `SpectralMetric` row; a new response family is one `SpectralExpression` case, which breaks every symbol program at compile time.
- Boundary: a symbol reads the bin's per-axis cycles-per-millimetre off the transform result's own axes and never re-derives a spectrum axis beside the lattice that produced it; the shape pair is the step's anisotropy ratio and cut-off wavelength, the ONE pair every metric reads.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct SpectralShape(double Anisotropy, double Wavelength);

[SmartEnum<string>]
public sealed partial class SpectralMetric {
    public static readonly SpectralMetric Anisotropic = new("anisotropic", static (frequency, shape) =>
        Math.Sqrt(
            Square(frequency.X * shape.Wavelength * shape.Anisotropy)
            + Square(frequency.Y * shape.Wavelength)
            + Square(frequency.Z * shape.Wavelength)));
    public static readonly SpectralMetric Scaled = new("scaled", static (frequency, shape) =>
        Radius(frequency) * shape.Wavelength);
    public static readonly SpectralMetric Offset = new("offset", static (frequency, shape) =>
        (Radius(frequency) - (1.0 / shape.Wavelength)) * shape.Wavelength * shape.Anisotropy);

    public Func<Vector3d, SpectralShape, double> Measure { get; }

    internal static double Radius(Vector3d frequency) =>
        Math.Sqrt(Square(frequency.X) + Square(frequency.Y) + Square(frequency.Z));

    private static double Square(double value) => value * value;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SpectralExpression {
    private SpectralExpression() { }

    public sealed record Constant(Complex Value) : SpectralExpression;
    public sealed record Gaussian(SpectralMetric Metric, double Decay) : SpectralExpression;
    public sealed record Derivative(int Order) : SpectralExpression;
    public sealed record Product(Arr<SpectralExpression> Factors) : SpectralExpression;

    public Complex At(Vector3d frequency, SpectralShape shape) => Switch(
        state: (Frequency: frequency, Shape: shape),
        constant: static (_, term) => term.Value,
        gaussian: static (at, term) => term.Metric.Measure(at.Frequency, at.Shape) switch {
            var measure => new Complex(Math.Exp(-term.Decay * measure * measure), 0.0),
        },
        derivative: static (at, term) => Complex.Pow(
            new Complex(0.0, Math.Tau * SpectralMetric.Radius(at.Frequency)), term.Order),
        product: static (at, term) => term.Factors.Fold(
            Complex.One, (held, factor) => held * factor.At(at.Frequency, at.Shape)));

    public bool Valid => Switch(
        constant: static term => double.IsFinite(term.Value.Real) && double.IsFinite(term.Value.Imaginary),
        gaussian: static term => double.IsFinite(term.Decay) && term.Decay > 0.0,
        derivative: static term => term.Order > 0,
        product: static term => !term.Factors.IsEmpty && term.Factors.ForAll(static factor => factor.Valid));
}

[SmartEnum<string>]
public sealed partial class SpectralSymbol {
    private const double GaussianDecay = 2.0 * Math.PI * Math.PI;
    private const double UnitDecay = 1.0;

    public static readonly SpectralSymbol DirectionalBlur = new("directional-blur",
        new SpectralExpression.Gaussian(SpectralMetric.Anisotropic, GaussianDecay));
    public static readonly SpectralSymbol BandPass = new("band-pass",
        new SpectralExpression.Gaussian(SpectralMetric.Offset, UnitDecay));
    public static readonly SpectralSymbol GradientMagnitude = new("gradient-magnitude",
        new SpectralExpression.Product(Arr(
            (SpectralExpression)new SpectralExpression.Derivative(1),
            new SpectralExpression.Gaussian(SpectralMetric.Scaled, GaussianDecay))));

    public SpectralExpression Program { get; }

    public Complex Of(Vector3d frequency, SpectralShape shape) => Program.At(frequency, shape);
}
```

## [04]-[MORPHOLOGY]

- Owner: `VoxelMorphologyStep` owns the transform vocabulary and the ONE `Apply` entrypoint both its capsule classes answer on; `VoxelBoolean` owns the set operations over a rasterized set; `SpectralMorphology` owns the kernel-numeric boundary; `FilteredField` owns the reconstruction the filtered spectrum re-enters through.
- Cases: nine PicoGK rows are provider statement capsules; the `Spectral` row is a kernel-numeric-floor capsule that reaches no provider entry point.
- Law: `Spectral` crosses the boundary as SAMPLES on the budget lattice and returns as an `IImplicit` the provider rasterizes over the same bounds, so its failures are typed numeric refusals and never a native status. A spectral row lowered onto a PicoGK morphology call, or a page-local transform, window, frequency axis, or separability rule beside the kernel arena, is the deleted form.
- Exemption: `Apply`'s provider bodies, `SpectralMorphology.Rasterize`, and `FilteredField.fSignedDistance` are statement capsules — a native handle mutates or is replaced in place, and the reconstruction is a per-query lattice fold with no expression form.
- Entry: `VoxelMorphologyStep.Apply(Voxels, ImplicitPolicy)` is the one entrypoint, so the morphology fold never learns which capsule class a step belongs to.
- Auto: the provider bracket releases the held handle on the failure arm, so a mid-chain native throw never strands a lease; the reconstruction interpolates TRILINEARLY over the eight surrounding cell centres, so the filtered field is continuous and the smoothness the spectral law spends a transform pair to obtain survives its return.
- Packages: `PicoGK` (`Voxels` morphology entry points, `IImplicit`), `Rasm.Numerics` (`CellLattice` the ONE addressing owner both sides of the boundary read, `SpectralArena`, `Spectrum`, `SpectralSense`, `SpectralScaling`, `SignedAxis`, `PositiveMagnitude`), kernel `Rasm.Domain` (`Custody.Rollback` — the failure-arm release under every provider bracket), LanguageExt.Core.
- Growth: a native transform is one `VoxelMorphologyStep` case; a frequency-domain transform is one `SpectralSymbol` row under the single `Spectral` case.
- Boundary: a native failure carries the provider's own cause forward on the composed error, because the provider owns that taxonomy and the fabrication case names only the operation and its budget.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record VoxelMorphologyStep {
    private VoxelMorphologyStep() { }

    public sealed record Offset(Length Distance) : VoxelMorphologyStep;
    public sealed record Shell(Length Distance) : VoxelMorphologyStep;
    public sealed record OverOffset(Length First, Length FinalSurface) : VoxelMorphologyStep;
    public sealed record Smoothen(Length Distance) : VoxelMorphologyStep;
    public sealed record Fillet(Length Radius) : VoxelMorphologyStep;
    public sealed record DoubleOffset(Length First, Length Second) : VoxelMorphologyStep;
    public sealed record TripleOffset(Length Distance) : VoxelMorphologyStep;
    public sealed record Trim(BoundingBox Bounds) : VoxelMorphologyStep;
    public sealed record ProjectZ(Length Start, Length End) : VoxelMorphologyStep;
    public sealed record Spectral(SpectralSymbol Symbol, Ratio Anisotropy, Length Wavelength) : VoxelMorphologyStep;

    public bool Valid => Switch(
        offset: static value => double.IsFinite(value.Distance.Millimeters),
        shell: static value => double.IsFinite(value.Distance.Millimeters) && value.Distance.Millimeters != 0.0,
        overOffset: static value => double.IsFinite(value.First.Millimeters) && double.IsFinite(value.FinalSurface.Millimeters),
        smoothen: static value => ValidityClaim.Positive(value.Distance.Millimeters),
        fillet: static value => ValidityClaim.Positive(value.Radius.Millimeters),
        doubleOffset: static value => double.IsFinite(value.First.Millimeters) && double.IsFinite(value.Second.Millimeters),
        tripleOffset: static value => double.IsFinite(value.Distance.Millimeters),
        trim: static value => value.Bounds.IsValid,
        projectZ: static value => double.IsFinite(value.Start.Millimeters)
            && double.IsFinite(value.End.Millimeters)
            && value.Start.Millimeters < value.End.Millimeters,
        spectral: static value => ValidityClaim.All(
            value.Symbol.Program.Valid, ValidityClaim.Positive(value.Anisotropy.DecimalFractions),
            ValidityClaim.Positive(value.Wavelength.Millimeters)));

    public Fin<Voxels> Apply(Voxels voxels, ImplicitPolicy policy) => Switch(
        state: (Held: voxels, Policy: policy),
        offset: static (state, step) => Provider(state.Held, held => { held.Offset((float)step.Distance.Millimeters); return held; }),
        shell: static (state, step) => Provider(state.Held, held => {
            Voxels result = held.voxShell((float)step.Distance.Millimeters);
            held.Dispose();
            return result;
        }),
        overOffset: static (state, step) => Provider(state.Held, held => {
            held.OverOffset((float)step.First.Millimeters, (float)step.FinalSurface.Millimeters);
            return held;
        }),
        smoothen: static (state, step) => Provider(state.Held, held => { held.Smoothen((float)step.Distance.Millimeters); return held; }),
        fillet: static (state, step) => Provider(state.Held, held => { held.Fillet((float)step.Radius.Millimeters); return held; }),
        doubleOffset: static (state, step) => Provider(state.Held, held => {
            held.DoubleOffset((float)step.First.Millimeters, (float)step.Second.Millimeters);
            return held;
        }),
        tripleOffset: static (state, step) => Provider(state.Held, held => { held.TripleOffset((float)step.Distance.Millimeters); return held; }),
        trim: static (state, step) => Provider(state.Held, held => { held.Trim(FieldMath.Bounds(step.Bounds)); return held; }),
        projectZ: static (state, step) => Provider(state.Held, held => {
            held.ProjectZSlice((float)step.Start.Millimeters, (float)step.End.Millimeters);
            return held;
        }),
        spectral: static (state, step) => SpectralMorphology.Filter(state.Held, step, state.Policy));

    private static Fin<Voxels> Provider(Voxels held, Func<Voxels, Voxels> body) =>
        Op.Of(name: "implicit:morphology").Catch(() => Fin.Succ(body(held))).Rollback(held);
}

[SmartEnum]
public sealed partial class VoxelBoolean {
    public static readonly VoxelBoolean Union = new(
        static (head, tail) => { head.BoolAddAll(tail); return unit; });
    public static readonly VoxelBoolean Subtract = new(
        static (head, tail) => { head.BoolSubtractAll(tail); return unit; });
    public static readonly VoxelBoolean Intersect = new(
        static (head, tail) => tail.Iter(voxel => head.BoolIntersect(voxel)));

    [UseDelegateFromConstructor]
    public partial Unit Apply(Voxels head, IEnumerable<Voxels> tail);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
file static class SpectralMorphology {
    internal static Fin<Voxels> Filter(Voxels held, VoxelMorphologyStep.Spectral step, ImplicitPolicy policy) {
        Op key = Op.Of(name: nameof(Filter));
        SpectralShape shape = new(step.Anisotropy.DecimalFractions, step.Wavelength.Millimeters);
        return from cell in key.AcceptValidated<PositiveMagnitude>(candidate: policy.Budget.VoxelSizeMm)
               from lattice in CellLattice.Of(
                   bounds: policy.Budget.Bounds, cell: cell, ceiling: policy.Budget.VoxelCap, key: key)
               from sampled in key.Catch(() => Fin.Succ<SpectralArena>(new SpectralArena.Interleaved(
                       [.. Enumerable.Range(0, (int)lattice.CellCount)
                           .Select(index => lattice.Coordinate(index))
                           .Select(at => lattice.Center(at.Column, at.Row, at.Layer))
                           .Select(point => new Complex(
                               held.fSignedDistance(new Vector3((float)point.X, (float)point.Y, (float)point.Z)), 0.0))],
                       lattice)))
               from forward in sampled.Transform(SpectralSense.Forward, SpectralScaling.Symmetric, key)
               from axes in Seq(SignedAxis.PositiveX, SignedAxis.PositiveY, SignedAxis.PositiveZ)
                   .TraverseM(axis => forward.Frequencies(axis, key)).As()
               let symbol = Enumerable.Range(0, (int)forward.Cells)
                   .Select(bin => lattice.Coordinate(bin))
                   .Select(at => step.Symbol.Of(
                       new Vector3d(axes[0][at.Column], axes[1][at.Row], axes[2][at.Layer]), shape))
                   .ToArray()
               from modulated in forward.Modulate(symbol, key)
               from inverted in modulated.Arena.Transform(SpectralSense.Inverse, SpectralScaling.Symmetric, key)
               from rebuilt in Rasterize(held, inverted, lattice, policy)
               select rebuilt;
    }

    private static Fin<Voxels> Rasterize(Voxels held, Spectrum filtered, CellLattice lattice, ImplicitPolicy policy) =>
        Op.Of(name: "implicit:spectral-raster").Catch(() => {
            Voxels result = new(new FilteredField(held, filtered, lattice), FieldMath.Bounds(policy.Budget.Bounds));
            held.Dispose();
            return Fin.Succ(result);
        }).Rollback(held);
}

file sealed class FilteredField(Voxels source, Spectrum filtered, CellLattice lattice) : IImplicit {
    private readonly Complex[] values = filtered.Arena is SpectralArena.Interleaved grid ? grid.Values : [];

    public float fSignedDistance(in Vector3 point) {
        Point3d world = new(point.X, point.Y, point.Z);
        (int Column, int Row, int Layer) nearest = lattice.Nearest(world);
        if (values.Length != lattice.CellCount || !lattice.Contains(nearest.Column, nearest.Row, nearest.Layer))
            return source.fSignedDistance(point);

        Point3d local = lattice.Locate(world);
        (double X, double Y, double Z) origin = (local.X - 0.5, local.Y - 0.5, local.Z - 0.5);
        (int X, int Y, int Z) corner = ((int)Math.Floor(origin.X), (int)Math.Floor(origin.Y), (int)Math.Floor(origin.Z));
        (double X, double Y, double Z) fraction = (origin.X - corner.X, origin.Y - corner.Y, origin.Z - corner.Z);

        double total = 0.0;
        for (int step = 0; step < 8; step++) {
            (int X, int Y, int Z) offset = (step & 1, (step >> 1) & 1, (step >> 2) & 1);
            double weight =
                (offset.X == 0 ? 1.0 - fraction.X : fraction.X)
                * (offset.Y == 0 ? 1.0 - fraction.Y : fraction.Y)
                * (offset.Z == 0 ? 1.0 - fraction.Z : fraction.Z);
            long index = lattice.Linear(
                Math.Clamp(corner.X + offset.X, 0, lattice.Columns.Value - 1),
                Math.Clamp(corner.Y + offset.Y, 0, lattice.Rows.Value - 1),
                Math.Clamp(corner.Z + offset.Z, 0, lattice.Layers.Value - 1));
            total += weight * values[(int)index].Real;
        }
        return (float)total;
    }
}
```

## [05]-[PROVIDER_VOCABULARY]

- Owner: `SliceRender`, `SliceAxis`, `MaskSampling`, `CliFormat`, and `ContourWinding` own the five provider selectors this page admits; `RasterFrame` owns the grid frame a raster preimage reads.
- Law: a preimage NEVER carries a provider enum ordinal. Each row keys on this package's own vocabulary and carries the PicoGK member as a `Native` column, so `FabricationCanon.Discriminant` writes the row's key length-framed and a provider that reorders, inserts, or renumbers its enum re-keys nothing already minted. The provider member is read at the call site alone.
- Cases: `MaskSampling` earns real cases rather than wearing a boolean — each row carries BOTH the elevation law its lane derives and the slice fetch it performs, so the mask loop reads two columns off one admitted row instead of branching twice on the same discriminant.
- Entry: `ContourWinding.Of(PolyContour)` is the one winding read — `PolyContour.eDetectWinding` is a STATIC fold over a vertex list, and the instance `eWinding()` reports only the last detected value, so a preimage reading the instance accessor forks on contour construction order.
- Auto: a `RasterFrame` accompanies every raster preimage because a grayscale payload is addressable — and its content key reproducible across voxel sizes — only with the grid origin and pitch beside it.
- Packages: `PicoGK` (`Voxels.ESliceMode`, `Voxels.ESliceAxis`, `CliIo.EFormat`, `PolyContour.EWinding`), Thinktecture.Runtime.Extensions.
- Growth: a new provider selector is one `[SmartEnum<string>]` here carrying its native column; a new sampling lane is one `MaskSampling` row spelling its two columns.
- Boundary: the slice fetch takes its buffer by reference because PicoGK writes into one allocated `ImageGrayScale` across every slice, so the column is a declared delegate rather than a `Func`.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class SliceRender {
    public static readonly SliceRender SignedDistance = new("signed-distance", Voxels.ESliceMode.SignedDistance);
    public static readonly SliceRender BlackWhite = new("black-white", Voxels.ESliceMode.BlackWhite);
    public static readonly SliceRender Antialiased = new("antialiased", Voxels.ESliceMode.Antialiased);

    public Voxels.ESliceMode Native { get; }
}

[SmartEnum<string>]
public sealed partial class SliceAxis {
    public static readonly SliceAxis X = new("x", Voxels.ESliceAxis.X);
    public static readonly SliceAxis Y = new("y", Voxels.ESliceAxis.Y);
    public static readonly SliceAxis Z = new("z", Voxels.ESliceAxis.Z);

    public Voxels.ESliceAxis Native { get; }
}

[SmartEnum<string>]
public sealed partial class CliFormat {
    public static readonly CliFormat EmptyFirstLayer = new("empty-first-layer", CliIo.EFormat.UseEmptyFirstLayer);
    public static readonly CliFormat FirstLayerWithContent = new("first-layer-with-content", CliIo.EFormat.FirstLayerWithContent);

    public CliIo.EFormat Native { get; }
}

[SmartEnum<string>]
public sealed partial class ContourWinding {
    public static readonly ContourWinding Unknown = new("unknown", PolyContour.EWinding.UNKNOWN);
    public static readonly ContourWinding Clockwise = new("clockwise", PolyContour.EWinding.CLOCKWISE);
    public static readonly ContourWinding Counterclockwise = new("counterclockwise", PolyContour.EWinding.COUNTERCLOCKWISE);

    public PolyContour.EWinding Native { get; }

    private static readonly Lazy<FrozenDictionary<PolyContour.EWinding, ContourWinding>> Rows = new(
        static () => Items.ToFrozenDictionary(static row => row.Native),
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static ContourWinding Of(PolyContour contour) => Rows.Value[PolyContour.eDetectWinding(contour.oVertices())];
}

public delegate void SliceFetch(
    Voxels field, int index, Length elevation, ref ImageGrayScale image, SliceRender render, SliceAxis axis);

[SmartEnum<string>]
public sealed partial class MaskSampling {
    public static readonly MaskSampling VoxelGrid = new("voxel-grid",
        elevations: static (policy, sliceCount) => toSeq(Enumerable.Range(0, sliceCount)).Map(index =>
            Length.FromMillimeters(policy.Budget.Bounds.Min.Z + ((index + 0.5) * policy.Budget.VoxelSizeMm))),
        fetch: static (Voxels field, int index, Length _, ref ImageGrayScale image, SliceRender render, SliceAxis axis) =>
            field.GetVoxelSlice(index, ref image, render.Native, axis.Native));
    public static readonly MaskSampling Interpolated = new("interpolated",
        elevations: static (policy, _) => Sdf.Elevations(policy.Budget.Bounds, policy.LayerHeight),
        fetch: static (Voxels field, int _, Length at, ref ImageGrayScale image, SliceRender render, SliceAxis _axis) =>
            field.GetInterpolatedVoxelSlice((float)at.Millimeters, ref image, render.Native));

    public Func<ImplicitPolicy, int, Seq<Length>> Elevations { get; }
    public SliceFetch Fetch { get; }
}

public readonly record struct RasterFrame(float VoxelSizeMm, Point3d Origin, int Columns, int Rows, int Layers) {
    public static RasterFrame Of(Voxels field, int layer) {
        field.GetVoxelDimensions(out int columns, out int rows, out int layers);
        Vector3 origin = field.vecZSliceOrigin(layer);
        return new RasterFrame(field.fVoxelSize, new Point3d(origin.X, origin.Y, origin.Z), columns, rows, layers);
    }
}
```

## [06]-[OPERATION_SET]

- Owner: `ImplicitOp` owns the operation vocabulary and every column shared across it; `ImplicitPolicy` owns budget, layer height, egress mode, calibration, and the commit sink; `VoxelWire` owns the content-addressed round trip; `VdbSource` owns an external field's identity and metadata contract.
- Law: the columns EVERY case reads — policy, morphology, commit sink, nested inputs, and kind — ride the union ROOT, so `Policy`, `Morphology`, `Subject`, `Commit`, and `Expanded` are base reads rather than five parallel arm tables restating one correspondence per case.
- Cases: the commit sink is `Some` where the case owns a wire and `None` where the policy sink answers, so one expression closes every commit; `Nested` is empty on every leaf, so the compatibility walk needs no arm.
- Law: layer egress is REQUESTED rather than universal, so `ImplicitPolicy.Cli` carries `Option<CliMode>` and a voxelizing caller that posts no layer stack supplies nothing — the same absence carrier the run spine's own memo and progress columns take. A mode defaulted onto a caller that never asked for one is the deleted form, and the CLI pipeline reads presence rather than a sentinel encoding.
- Entry: `ImplicitPolicy.Admit` proves budget, layer height, and any requested egress mode ONCE, so the operation admission never re-checks a policy invariant and the budget's own cell ceiling is not re-tested downstream.
- Auto: `VoxelOperationKind` supplies the fault subject key, so a refusal names the operation through owned vocabulary rather than a member name captured at a call site.
- Packages: `PicoGK`, `Rasm.Fabrication.Process` (`VoxelBudget`, `ContentKey`, `EgressKind`, `FaultSubject`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new operation is one `ImplicitOp` case and one `VoxelOperationKind` row; a layer encoding is one `CliMode` case.
- Boundary: commit, wire read, wire write, and the three grading-field readers are the genuine EXTERNALS this page injects — each names a capability the caller owns — while every algorithm the page charters stays on the page.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class VoxelOperationKind {
    public static readonly VoxelOperationKind Field = new("field");
    public static readonly VoxelOperationKind LatticeSupport = new("lattice-support");
    public static readonly VoxelOperationKind Source = new("source");
    public static readonly VoxelOperationKind Vdb = new("vdb");
    public static readonly VoxelOperationKind Composite = new("composite");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CliMode {
    private CliMode() { }

    public sealed record Grayscale(SliceRender Render, MaskSampling Sampling, SliceAxis Axis) : CliMode;
    public sealed record CliVector(
        CliFormat Format,
        double UnitsInMillimeters,
        bool AbsoluteOrigin,
        Option<FileInfo> Target = default) : CliMode;
    public sealed record VdbCli(FileInfo Target) : CliMode;

    public bool Valid => Switch(
        grayscale: static _ => true,
        cliVector: static mode => ValidityClaim.Positive(mode.UnitsInMillimeters),
        vdbCli: static mode => mode.Target.Directory is { Exists: true });
}

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class VdbSource {
    public ContentKey Key { get; }
    public FileInfo Path { get; }
    public FieldKey Field { get; }
    public HashMap<string, string> RequiredMetadata { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ContentKey key,
        ref FileInfo path,
        ref FieldKey field,
        ref HashMap<string, string> requiredMetadata) {
        if (requiredMetadata.IsEmpty || !requiredMetadata.ForAll(static pair => Witness.Keyed(pair.Key)))
            validationError = new ValidationError("vdb-source");
    }

    public static Fin<VdbSource> Admit(
        ContentKey key, FileInfo path, FieldKey field, HashMap<string, string> requiredMetadata) =>
        Validate(key, path, field, requiredMetadata, out VdbSource source).Admitted(source);
}

[ComplexValueObject]
public sealed partial class ImplicitPolicy {
    public VoxelBudget Budget { get; }
    public Length LayerHeight { get; }
    public CalibrationPolicy Calibration { get; }
    public Func<Voxels, Fin<ContentKey>> Commit { get; }

    public Option<CliMode> Cli { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref VoxelBudget budget,
        ref Length layerHeight,
        ref CalibrationPolicy calibration,
        ref Func<Voxels, Fin<ContentKey>> commit,
        ref Option<CliMode> cli) {
        if (!ValidityClaim.Positive(layerHeight.Millimeters).Holds || !cli.ForAll(static mode => mode.Valid))
            validationError = new ValidationError("implicit-policy");
    }

    public static Fin<ImplicitPolicy> Admit(
        VoxelBudget budget,
        Length layerHeight,
        CalibrationPolicy calibration,
        Func<Voxels, Fin<ContentKey>> commit,
        Option<CliMode> cli = default) =>
        Validate(budget, layerHeight, calibration, commit, cli, out ImplicitPolicy policy).Admitted(policy);
}

public sealed record VoxelWire(ContentKey Key, Func<Fin<Voxels>> ToVoxels, Func<Voxels, Fin<ContentKey>> FromVoxels);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImplicitOp(
    ImplicitPolicy Policy,
    Seq<VoxelMorphologyStep> Morphology,
    Option<VoxelWire> Sink,
    Seq<ImplicitOp> Nested,
    VoxelOperationKind Kind) {
    public sealed record Field(
        ImplicitPolicy Policy,
        Seq<VoxelMorphologyStep> Morphology,
        FieldDefinition Definition,
        TpmsForm Form,
        ImplicitCell Cell,
        VoxelWire Envelope)
        : ImplicitOp(Policy, Morphology, Some(Envelope), Seq<ImplicitOp>(), VoxelOperationKind.Field);

    public sealed record LatticeSupport(
        ImplicitPolicy Policy,
        Seq<VoxelMorphologyStep> Morphology,
        SupportPlan Support,
        Option<VoxelWire> Part)
        : ImplicitOp(Policy, Morphology, Part, Seq<ImplicitOp>(), VoxelOperationKind.LatticeSupport);

    public sealed record Source(
        ImplicitPolicy Policy,
        Seq<VoxelMorphologyStep> Morphology,
        VoxelWire Wire)
        : ImplicitOp(Policy, Morphology, Some(Wire), Seq<ImplicitOp>(), VoxelOperationKind.Source);

    public sealed record Vdb(
        ImplicitPolicy Policy,
        Seq<VoxelMorphologyStep> Morphology,
        VdbSource Origin)
        : ImplicitOp(Policy, Morphology, Option<VoxelWire>.None, Seq<ImplicitOp>(), VoxelOperationKind.Vdb);

    public sealed record Composite(
        ImplicitPolicy Policy,
        Seq<VoxelMorphologyStep> Morphology,
        Seq<ImplicitOp> Inputs,
        VoxelBoolean Boolean)
        : ImplicitOp(Policy, Morphology, Option<VoxelWire>.None, Inputs, VoxelOperationKind.Composite);

    public FaultSubject.VoxelOperation Subject => new(Kind.Key);

    public Fin<ContentKey> Commit(Voxels voxels) =>
        Sink.Map(wire => wire.FromVoxels(voxels)).IfNone(() => Policy.Commit(voxels));

    public Seq<ImplicitOp> Expanded => Seq(this) + Nested.Bind(static row => row.Expanded);
}
```

## [07]-[VOXEL_LEASE]

- Owner: `VoxelRuntime` owns PicoGK ambient lifetime; `VoxelScope` owns metrics, mesh, ray, and VDB projection inside one lease; `Sdf.Voxelize` owns the materializing entrypoint and the build fold under it.
- Law: `VoxelRuntime.Use` serializes one `Library.GlobalInstance` per compatible operation set, so sequential sets may select distinct voxel sizes. A returned native handle is invalid egress; every handle allocated in the fold is consumed inside the callback and released on both arms.
- Exemption: `VoxelRuntime.Use`, `Combine`, `LatticeVoxels`, `Subtract`, `Measure`, and `Sealed` are lifetime and provider statement capsules — each guards its own native acquire or aggregate, which has no expression form. `Consume` holds no exemption: its rasters arrive already acquired and never transfer custody, so the both-arms release rides kernel `Custody.Bracket`.
- Entry: `Sdf.Voxelize<T>(Seq<ImplicitOp>, Func<Arr<VoxelScope>, Fin<T>>)` is the single materializing entrypoint for one or many compatible fields.
- Auto: admission accumulates every operation fault before native allocation. `Raster` intersects the field operation's own `Envelope` occupancy through `voxIntersectImplicit` rather than rasterizing the whole budget box, because a full-bounds construction allocates the entire budget before discarding almost all of it. `Occupied` rejects an empty rasterization before it posts an empty program. Lattice scaffolds read the ONE `SupportTopology` `support#SUPPORT_TOPOLOGY` publishes, so the support edge set is never reconstructed and a missing parent is impossible by the owner's own admission.
- Result: `VoxelMetrics` carries physical volume, queried bounds, native memory, committed field identity, and the `CalibrationStats` the quantile pass measured.
- Packages: `PicoGK` (implicit rasterization and intersection, lattice beams and nodes, metrics, ray-cast, mesh extraction, VDB read and write with field metadata), QuikGraph (`SEquatableEdge` endpoints off the published topology), kernel `Rasm.Domain` (`Custody.Rollback` the failure-arm release every acquire-chain fold composes, `Custody.Bracket` the both-arms release under `Consume` — disposer faults appended onto the primary), LanguageExt.Core.
- Growth: a materializing consumer is one `Voxelize` callback.
- Boundary: `PeriodicImplicit.fSignedDistance` copies the provider's by-reference callback value before use; VDB source identity travels with its field name and required metadata; the document container is what NAMES a field, so the direct single-field write cannot serve an egress whose import lane resolves by name.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record VoxelMetrics(
    Volume Volume,
    BoundingBox Bounds,
    long NativeBytes,
    ContentKey Field,
    Option<CalibrationStats> Calibration);

internal readonly record struct Rasterized(Voxels Voxels, Option<CalibrationStats> Calibration);

public sealed class VoxelScope {
    internal VoxelScope(Voxels native, VoxelMetrics metrics) => (Native, Metrics) = (native, metrics);

    internal Voxels Native { get; }
    public VoxelMetrics Metrics { get; }

    public Fin<ContentKey> Mesh(Func<PicoGK.Mesh, Fin<ContentKey>> project) {
        using PicoGK.Mesh mesh = Native.mshAsMesh();
        return project(mesh);
    }

    public Fin<Option<Point3d>> Raycast(Point3d origin, Vector3d direction) =>
        !origin.IsValid || !direction.IsValid || direction.IsZero
            ? Fin.Fail<Option<Point3d>>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "implicit-ray:invalid"))
            : Fin.Succ(Native.bRayCastToSurface(
                FieldMath.Vector(origin),
                new Vector3((float)direction.X, (float)direction.Y, (float)direction.Z),
                out Vector3 hit)
                    ? Some(new Point3d(hit.X, hit.Y, hit.Z))
                    : None);

    public Fin<ContentKey> Vdb(FileInfo target, FieldKey field, HashMap<string, string> provenance) =>
        target.Directory is not { Exists: true }
            ? Fin.Fail<ContentKey>(new KernelFault.InvalidValue("implicit", "implicit-vdb:export-target"))
            : Op.Of(name: "implicit-vdb:export").Catch(() => {
                    using OpenVdbFile file = new();
                    _ = file.nAdd(Native, field.Value);
                    using FieldMetadata metadata = Native.oMetaData();
                    provenance.Iter(pair => metadata.SetValue(pair.Key, pair.Value));
                    file.SaveToFile(target.FullName);
                    return Fin.Succ(Metrics.Field);
                });
}

// --- [RUNTIME] -------------------------------------------------------------------------
file static class VoxelRuntime {
    private static readonly Lock Gate = new();

    public static Fin<T> Use<T>(Seq<ImplicitOp> operations, Func<Fin<T>> run) {
        ImplicitOp operation = operations[0];
        lock (Gate) {
            return Op.Of(name: "implicit:runtime").Catch(() => {
                    using Library.GlobalInstance runtime = new((float)operation.Policy.Budget.VoxelSizeMm);
                    return run();
                }, cause => Provider(operation, cause));
        }
    }

    internal static Option<FabricationFault.VoxelFault> Provider(ImplicitOp operation, Error cause) =>
        cause.Exception.Bind(raised => raised is PicoGKAllocException or PicoGKLibraryMismatchException
            ? Some(new FabricationFault.VoxelFault(operation.Subject, operation.Policy.Budget, cause))
            : None);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class Sdf {
    public static Fin<T> Voxelize<T>(Seq<ImplicitOp> operations, Func<Arr<VoxelScope>, Fin<T>> consume) =>
        operations.IsEmpty
            ? Fin.Fail<T>(new KernelFault.InvalidValue("implicit", "implicit:empty-operation-set"))
            : from _ in operations.Traverse(operation => Admit(operation).ToValidation()).As().ToFin()
              from __ in AdmissionSlots.Gate(Compatible(operations),
                  FabConcern.Additive, "implicit:incompatible-operation-set", FabricationFault.Inadmissible).ToFin()
              from result in VoxelRuntime.Use(operations, () =>
                  from rasters in Build(operations)
                  from consumed in Consume(operations, rasters, consume)
                  select consumed)
              select result;

    private static Fin<T> Consume<T>(
        Seq<ImplicitOp> operations,
        Seq<Rasterized> rasters,
        Func<Arr<VoxelScope>, Fin<T>> consume) =>
        Custody.Bracket(
            () => from scopes in operations.Zip(rasters)
                      .Traverse(row =>
                          from _ in Occupied(row.Second.Voxels, row.First)
                          from field in row.First.Commit(row.Second.Voxels)
                          select new VoxelScope(row.Second.Voxels, Measure(row.Second, field)))
                      .As()
                  from result in consume(scopes.ToArr())
                  select result,
            [.. rasters.Map(static row => (IDisposable?)row.Voxels)]);

    private static Fin<Unit> Occupied(Voxels voxels, ImplicitOp operation) =>
        voxels.bIsEmpty() ? Fail<Unit>(operation) : Fin.Succ(unit);

    private static Fin<Seq<Rasterized>> Build(Seq<ImplicitOp> operations) =>
        operations.Fold(
            Fin.Succ(Seq<Rasterized>()),
            static (result, operation) =>
                from held in result
                from next in Build(operation).Rollback(held.Map(static row => (IDisposable?)row.Voxels).ToArray())
                select held.Add(next));

    private static Fin<Rasterized> Build(ImplicitOp operation) => operation.Switch(
        field: Field,
        latticeSupport: static row => Lattice(row.Support, row.Part, row.Morphology, row.Policy)
            .Map(static voxels => new Rasterized(voxels, None)),
        source: static row => row.Wire.ToVoxels()
            .Bind(voxels => Morph(voxels, row.Morphology, row.Policy))
            .Map(static voxels => new Rasterized(voxels, None)),
        vdb: static row => Vdb(row.Origin, row.Policy.Budget.VoxelSizeMm)
            .Bind(voxels => Morph(voxels, row.Morphology, row.Policy))
            .Map(static voxels => new Rasterized(voxels, None)),
        composite: static row =>
            from inputs in Build(row.Inputs)
            from combined in Combine(inputs.Map(static held => held.Voxels), row.Boolean)
            from morphed in Morph(combined, row.Morphology, row.Policy)
            select new Rasterized(morphed, inputs.Map(static held => held.Calibration).Somes().Head));

    private static Fin<Voxels> Combine(Seq<Voxels> inputs, VoxelBoolean operation) =>
        Op.Of(name: "implicit-composite").Catch(() => {
                Voxels result = inputs[0].voxDuplicate();
                try {
                    _ = operation.Apply(result, inputs.Skip(1));
                    return Fin.Succ(result);
                }
                catch {
                    result.Dispose();
                    throw;
                }
                finally {
                    inputs.Iter(static input => input.Dispose());
                }
            });

    private static Fin<Rasterized> Field(ImplicitOp.Field operation) =>
        from density in Acquire(operation.Cell.DensityField)
        from scale in Acquire(operation.Cell.ScaleField).Rollback(Lease(density))
        from axis in Acquire(operation.Cell.AxisField).Rollback(Lease(density), Lease(scale))
        from raster in Raster(operation, density, scale, axis)
            .Rollback(Lease(density), Lease(scale), Lease(axis))
        select Released(raster, density, scale, axis);

    private static Fin<Rasterized> Raster(
        ImplicitOp.Field operation,
        Option<ScalarField> density,
        Option<ScalarField> scale,
        Option<VectorField> axis) =>
        from envelope in operation.Envelope.ToVoxels()
        from calibration in FieldCalibration
            .Of(operation.Definition, operation.Cell, operation.Policy.Calibration)
            .Rollback(envelope)
        from intersected in Op.Of(name: "implicit:intersect").Catch(() => Fin.Succ(
            envelope.voxIntersectImplicit(new PeriodicImplicit(
                operation.Definition,
                operation.Form,
                operation.Cell,
                FieldMath.Bounds(operation.Policy.Budget.Bounds),
                density,
                scale,
                axis,
                calibration))), cause => VoxelRuntime.Provider(operation, cause))
            .Rollback(envelope, calibration)
        from morphed in Morph(intersected, operation.Morphology, operation.Policy)
            .Rollback(envelope, calibration)
        select Sealed(morphed, envelope, calibration);

    private static Rasterized Sealed(Voxels morphed, Voxels envelope, FieldCalibration calibration) {
        CalibrationStats stats = calibration.Stats;
        envelope.Dispose();
        calibration.Dispose();
        return new Rasterized(morphed, Some(stats));
    }

    private static Rasterized Released(
        Rasterized raster, Option<ScalarField> density, Option<ScalarField> scale, Option<VectorField> axis) {
        density.Iter(static field => field.Dispose());
        scale.Iter(static field => field.Dispose());
        axis.Iter(static field => field.Dispose());
        return raster;
    }

    private static IDisposable? Lease<T>(Option<T> held) where T : class, IDisposable => held.ValueUnsafe();

    private static Fin<Voxels> Lattice(
        SupportPlan support,
        Option<VoxelWire> part,
        Seq<VoxelMorphologyStep> morphology,
        ImplicitPolicy policy) =>
        from scaffold in LatticeVoxels(support.Topology)
        from result in part.Map(wire => Subtract(scaffold, wire)).IfNone(() => Fin.Succ(scaffold))
        from morphed in Morph(result, morphology, policy)
        select morphed;

    private static Fin<Voxels> LatticeVoxels(SupportTopology topology) =>
        from beams in toSeq(topology.Graph.Edges).Traverse(edge =>
                from parent in topology.Node(edge.Source).ToFin(Refusal("implicit-lattice:absent-node"))
                from child in topology.Node(edge.Target).ToFin(Refusal("implicit-lattice:absent-node"))
                select (Parent: parent, Child: child))
            .As()
        from voxels in Op.Of(name: "implicit-lattice").Catch(() => {
                using PicoGK.Lattice lattice = new();
                topology.Nodes.Iter(node => lattice.AddSphere(FieldMath.Vector(node.At), (float)node.Radius));
                beams.Iter(beam => lattice.AddBeam(
                    FieldMath.Vector(beam.Parent.At),
                    (float)beam.Parent.Radius,
                    FieldMath.Vector(beam.Child.At),
                    (float)beam.Child.Radius,
                    bRoundCap: true));
                return Fin.Succ(new Voxels(lattice));
            })
        select voxels;

    private static Fin<Voxels> Subtract(Voxels scaffold, VoxelWire wire) =>
        wire.ToVoxels().Map(model => {
            try {
                scaffold.BoolSubtract(model);
                return scaffold;
            }
            finally {
                model.Dispose();
            }
        }).Rollback(scaffold);

    private static Fin<Voxels> Vdb(VdbSource source, double voxelSizeMm) =>
        from _ in VdbMetadata(source, voxelSizeMm)
        from voxels in Op.Of(name: "implicit-vdb").Catch(() => {
                using OpenVdbFile file = new(source.Path.FullName);
                using Voxels field = file.voxGet(source.Field.Value);
                return Fin.Succ(field.voxDuplicate());
            })
        select voxels;

    private static Fin<Unit> VdbMetadata(VdbSource source, double voxelSizeMm) =>
        VdbIdentity(source).Bind(_ => Op.Of(name: "implicit-vdb:metadata").Catch(() => {
                using OpenVdbFile file = new(source.Path.FullName);
                if (!file.bIsPicoGKCompatible() || !file.fPicoGKVoxelSizeMM().Equals((float)voxelSizeMm))
                    return Fin.Fail<Unit>(new KernelFault.InvalidValue("implicit", "implicit-vdb:voxel-size"));

                using Voxels field = file.voxGet(source.Field.Value);
                using FieldMetadata metadata = field.oMetaData();
                return source.RequiredMetadata.ForAll(pair =>
                    metadata.bGetValueAt(pair.Key, out string actual)
                    && string.Equals(actual, pair.Value, StringComparison.Ordinal))
                        ? Fin.Succ(unit)
                        : Fin.Fail<Unit>(new KernelFault.InvalidValue("implicit", "implicit-vdb:metadata"));
            }));

    private static Fin<Unit> VdbIdentity(VdbSource source) =>
        Op.Of(name: "implicit-vdb:identity").Catch(() => {
                using FileStream payload = source.Path.OpenRead();
                long canonicalLength = sizeof(int) + Encoding.UTF8.GetByteCount(source.Field.Value) + payload.Length;
                if (canonicalLength > int.MaxValue)
                    return Fin.Succ(false);

                byte[] kind = Encoding.UTF8.GetBytes(source.Key.Kind.Key);
                byte[] field = Encoding.UTF8.GetBytes(source.Field.Value);
                return Fin.Succ(ContentHash.Of(
                    state: (Kind: kind, Field: field, Length: (int)canonicalLength, Payload: payload),
                    chunks: static (state, hash) => {
                        Span<byte> width = stackalloc byte[sizeof(int)];
                        BinaryPrimitives.WriteInt32LittleEndian(width, state.Kind.Length);
                        hash.Append(width);
                        hash.Append(state.Kind);
                        BinaryPrimitives.WriteInt32LittleEndian(width, state.Length);
                        hash.Append(width);
                        BinaryPrimitives.WriteInt32LittleEndian(width, state.Field.Length);
                        hash.Append(width);
                        hash.Append(state.Field);
                        hash.Append(state.Payload);
                    }) == source.Key.Digest);
            })
            .Bind(matches => matches
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new KernelFault.InvalidValue("implicit", "implicit-vdb:identity")));

    private static Fin<Voxels> Morph(Voxels voxels, Seq<VoxelMorphologyStep> steps, ImplicitPolicy policy) =>
        steps.Fold(Fin.Succ(voxels), (result, step) => result.Bind(held => step.Apply(held, policy)));

    private static Fin<Option<T>> Acquire<T>(Option<Func<Fin<T>>> source) where T : class, IDisposable =>
        Op.Of(name: "implicit-driver").Catch(() => source.TraverseM(static factory => factory()).As());

    private static VoxelMetrics Measure(Rasterized raster, ContentKey field) {
        raster.Voxels.CalculateProperties(out float cubicMillimeters, out BBox3 properties);
        BBox3 queried = raster.Voxels.oCalculateBoundingBox();
        return new VoxelMetrics(
            Volume.FromCubicMillimeters(cubicMillimeters),
            FieldMath.Box(properties.bIsEmpty() ? queried : properties),
            raster.Voxels.nMemUsage(),
            field,
            raster.Calibration);
    }

    private static Fin<Unit> Admit(ImplicitOp operation) =>
        AdmissionSlots.Accumulate(Seq(
                AdmissionSlots.Gate(operation.Morphology.ForAll(static step => step.Valid),
                    FabConcern.Additive, "implicit:morphology", FabricationFault.Inadmissible),
                operation.Switch(
                    field: static row => AdmissionSlots.Gate(row.Form.Valid,
                        FabConcern.Additive, "implicit:field-form", FabricationFault.Inadmissible),
                    latticeSupport: static row => AdmissionSlots.Gate(
                        !row.Support.Topology.Graph.IsVerticesEmpty, FabConcern.Additive, "implicit:lattice-support", FabricationFault.Inadmissible),
                    source: static _ => Nothing,
                    vdb: static row => AdmissionSlots.Gate(row.Origin.Path.Exists,
                        FabConcern.Additive, "implicit:vdb-path", FabricationFault.Inadmissible),
                    composite: static row => AdmissionSlots.Accumulate(
                        Seq(AdmissionSlots.Gate(!row.Inputs.IsEmpty, FabConcern.Additive, "implicit:composite-inputs", FabricationFault.Inadmissible))
                        + row.Inputs.Map(static input =>
                            (K<Validation<Error>, Unit>)Admit(input).ToValidation())))))
            .As()
            .ToFin()
            .Map(static _ => unit);

    private static K<Validation<Error>, Unit> Nothing =>
        AdmissionSlots.Accumulate(Seq<K<Validation<Error>, Unit>>());


    private static Error Refusal(string locus) => FabricationFault.Inadmissible(FabConcern.Additive, locus);

    private static bool Compatible(Seq<ImplicitOp> operations) =>
        operations.Bind(static operation => operation.Expanded)
            .Map(static operation => operation.Policy.Budget.VoxelSizeMm)
            .Distinct()
            .Count == 1;

    internal static Seq<Length> Elevations(BoundingBox bounds, Length layerHeight) =>
        toSeq(Enumerable.Range(
                0,
                Math.Max(1, (int)Math.Ceiling((bounds.Max.Z - bounds.Min.Z) / layerHeight.Millimeters))))
            .Map(index => Length.FromMillimeters(bounds.Min.Z + ((index + 0.5) * layerHeight.Millimeters)));

    private static Fin<T> Fail<T>(ImplicitOp operation) =>
        Fin.Fail<T>(FabricationFault.Inadmissible(FabConcern.Additive, "implicit:operation"));
}

// --- [FIELD] ---------------------------------------------------------------------------
file static class FieldMath {
    public static Matrix4x4 AxisFrame(Vector3 axis, float tolerance) =>
        Vector3.Normalize(axis) switch {
            var direction => Math.Clamp(Vector3.Dot(Vector3.UnitZ, direction), -1.0f, 1.0f) switch {
                var dot when dot > 1.0f - tolerance => Matrix4x4.Identity,
                var dot when dot < -1.0f + tolerance => Matrix4x4.CreateFromQuaternion(
                    Quaternion.Conjugate(Quaternion.CreateFromAxisAngle(Vector3.UnitX, MathF.PI))),
                var dot => Matrix4x4.CreateFromQuaternion(Quaternion.Conjugate(
                    Quaternion.CreateFromAxisAngle(
                        Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, direction)),
                        MathF.Acos(dot)))),
            },
        };

    public static Vector3 Vector(Point3d point) => new((float)point.X, (float)point.Y, (float)point.Z);
    public static BBox3 Bounds(BoundingBox bounds) => new(Vector(bounds.Min), Vector(bounds.Max));

    public static BoundingBox Box(BBox3 bounds) => new(
        new Point3d(bounds.vecMin.X, bounds.vecMin.Y, bounds.vecMin.Z),
        new Point3d(bounds.vecMax.X, bounds.vecMax.Y, bounds.vecMax.Z));
}

file sealed class PeriodicImplicit(
    FieldDefinition definition,
    TpmsForm form,
    ImplicitCell cell,
    BBox3 bounds,
    Option<ScalarField> densityField,
    Option<ScalarField> scaleField,
    Option<VectorField> axisField,
    FieldCalibration calibration) : IImplicit, IBoundedImplicit {

    public BBox3 oBounds => bounds;

    public float fSignedDistance(in Vector3 world) => Distance(world);

    private float Distance(Vector3 world) =>
        (
            Density: densityField.Bind(field => Scalar(field, world)).IfNone(cell.RelativeDensity),
            Scale: scaleField.Bind(field => Scalar(field, world)).IfNone(Ratio.FromDecimalFractions(1.0)),
            Axis: axisField.Bind(field => Axis(field, world))) switch {
                var drivers => (
                    Sample: definition.At(cell.Phase(world, drivers.Axis, drivers.Scale)),
                    Threshold: calibration.Threshold(form, drivers.Density)) switch {
                        var field => form.Distance(
                            field.Sample.Level - field.Threshold.Iso,
                            MathF.Max(
                                cell.WorldGradient(field.Sample.Gradient, drivers.Axis, drivers.Scale).Length(),
                                calibration.GradientFloorPerMillimeter),
                            field.Threshold),
                    },
            };

    private Option<Ratio> Scalar(ScalarField field, Vector3 at) =>
        field.bGetValue(at, out float value) && float.IsFinite(value)
            ? Some(Ratio.FromDecimalFractions(Math.Clamp(
                value,
                (float)calibration.DensityFloor.DecimalFractions,
                (float)(1.0 - calibration.DensityFloor.DecimalFractions))))
            : None;

    private Option<Vector3> Axis(VectorField field, Vector3 at) =>
        field.bGetValue(at, out Vector3 value)
        && float.IsFinite(value.X)
        && float.IsFinite(value.Y)
        && float.IsFinite(value.Z)
        && value.LengthSquared() > (float)Math.Pow(cell.FrameTolerance.DecimalFractions, 2.0)
            ? Some(value)
            : None;
}

file sealed class FieldCalibration : IDisposable {
    private readonly MemoryOwner<float> levels;
    private readonly MemoryOwner<float> distances;
    private readonly CalibrationStats stats;
    private readonly CalibrationPolicy policy;

    private FieldCalibration(
        MemoryOwner<float> levels,
        MemoryOwner<float> distances,
        CalibrationStats stats,
        CalibrationPolicy policy) =>
        (this.levels, this.distances, this.stats, this.policy) = (levels, distances, stats, policy);

    public float GradientFloorPerMillimeter => (float)policy.GradientFloorPerMillimeter;
    public Ratio DensityFloor => policy.DensityFloor;
    public CalibrationStats Stats => stats;

    public static Fin<FieldCalibration> Of(
        FieldDefinition definition,
        ImplicitCell cell,
        CalibrationPolicy policy) {
        int resolution = Resolution(policy);
        int count = checked(resolution * resolution * resolution);
        MemoryOwner<float> levels = MemoryOwner<float>.Allocate(count);
        MemoryOwner<float>? distances = null;
        bool transferred = false;
        try {
            distances = MemoryOwner<float>.Allocate(count);
            SampleAction action = new(
                levels.Memory,
                distances.Memory,
                definition,
                cell,
                resolution,
                (float)policy.GradientFloorPerMillimeter);
            ParallelHelper.For(0, count, in action);
            Span<float> levelSpan = levels.Span;
            Span<float> distanceSpan = distances.Span;
            if (!TensorPrimitives.IsFiniteAll(levelSpan) || !TensorPrimitives.IsFiniteAll(distanceSpan))
                return Fin.Fail<FieldCalibration>(
                    new GeometryFault.DegenerateInput(Kind.Mesh, None, "implicit-calibration:non-finite"));

            float average = TensorPrimitives.Average(distanceSpan);
            using SpanOwner<float> baseline = SpanOwner<float>.Allocate(count);
            using SpanOwner<float> centered = SpanOwner<float>.Allocate(count);
            baseline.Span.Fill(average);
            TensorPrimitives.Subtract(distanceSpan, baseline.Span, centered.Span);
            TensorPrimitives.Abs(centered.Span, centered.Span);
            CalibrationStats stats = new(
                TensorPrimitives.Min(distanceSpan),
                TensorPrimitives.Max(distanceSpan),
                average,
                TensorPrimitives.StdDev(distanceSpan),
                TensorPrimitives.SumOfSquares(centered.Span),
                TensorPrimitives.IndexOfMin(distanceSpan),
                TensorPrimitives.IndexOfMax(distanceSpan),
                count);
            levelSpan.Sort();
            distanceSpan.Sort();
            FieldCalibration calibration = new(levels, distances, stats, policy);
            transferred = true;
            return Fin.Succ(calibration);
        }
        finally {
            if (!transferred) {
                levels.Dispose();
                distances?.Dispose();
            }
        }
    }

    public FieldThreshold Threshold(TpmsForm form, Ratio density) =>
        Math.Clamp((int)Math.Round(density.DecimalFractions * (levels.Span.Length - 1)), 0, levels.Span.Length - 1) switch {
            var index => form.Switch(
                state: (Calibration: this, Index: index),
                solid: static (state, _) => new FieldThreshold(
                    state.Calibration.levels.Span[state.Index],
                    Length.Zero,
                    state.Calibration.stats),
                sheet: static (state, _) => new FieldThreshold(
                    0.0f,
                    Length.FromMillimeters(state.Calibration.distances.Span[state.Index]),
                    state.Calibration.stats)),
        };

    public void Dispose() {
        levels.Dispose();
        distances.Dispose();
    }

    private static int Resolution(CalibrationPolicy policy) =>
        (Minimum: (int)Math.Ceiling(Math.Cbrt(policy.MinimumSamples)), Policy: policy) switch {
            var bounds => Math.Clamp(
                (int)Math.Ceiling(Math.Cbrt(
                    bounds.Policy.MinimumSamples / Math.Pow(bounds.Policy.QuantileError.DecimalFractions, 2.0))),
                bounds.Minimum,
                Math.Max(bounds.Minimum, (int)Math.Floor(Math.Cbrt(bounds.Policy.MaximumSamples)))),
        };

    private readonly struct SampleAction(
        Memory<float> levels,
        Memory<float> distances,
        FieldDefinition definition,
        ImplicitCell cell,
        int resolution,
        float gradientFloorPerMillimeter) : IAction {

        public void Invoke(int index) {
            int x = index % resolution;
            int y = index / resolution % resolution;
            int z = index / (resolution * resolution);
            Vector3 phase = new(
                (x + 0.5f) * MathF.Tau / resolution,
                (y + 0.5f) * MathF.Tau / resolution,
                (z + 0.5f) * MathF.Tau / resolution);
            VoxelSample sample = definition.At(phase);
            float gradient = MathF.Max(
                cell.WorldGradient(sample.Gradient, None, Ratio.FromDecimalFractions(1.0)).Length(),
                gradientFloorPerMillimeter);
            levels.Span[index] = sample.Level;
            distances.Span[index] = MathF.Abs(sample.Level / gradient);
        }
    }
}
```

## [08]-[LAYER_EGRESS]

- Owner: `Sdf.Cli` owns the single layer-stack entrypoint and the three egress lanes under it; `CliStack` owns the settled layer result; `CliImport` owns the reader result a round trip produces.
- Law: the sink enters as the runtime's own `Option<IProgress<double>>` carrier, absent by default so a headless caller spells nothing, and reaches the provider's trailing parameter directly on all three egress entry points — a page-local reporter, a percentage tally, or a polling thread beside the native call is the deleted form. Vectorize and write are two reported phases of ONE egress and take the SAME sink, so a caller never sees the file phase stall silently.
- Exemption: `Vector`, `Grayscale`, and `Direct` are provider statement capsules — the grayscale loop in particular owns its statement form because PicoGK writes every slice into one mutable `ref ImageGrayScale` buffer.
- Entry: `Sdf.Cli(ImplicitOp, Option<IProgress<double>>)` is the one layer-stack entrypoint; `slicing#DEPOSITION` threads its implicit arm into this signature.
- Auto: the mask loop reads its elevation law and its slice fetch off ONE admitted `MaskSampling` row, so the lane branches nowhere; header date, unit scale, and reader warnings ride `CliImport`, because discarding them loses the only evidence that a round-tripped program degraded.
- Result: `CliStack` carries layers, canonical `.cli` identity, mask identities, committed field identities, optional `VoxelMetrics`, and the optional reader result.
- Packages: `PicoGK` (`oVectorize`, `CliIo`, `Vdb2Cli`, grayscale slice reads), LanguageExt.Core.
- Growth: a layer encoding is one `CliMode` case and one lane here.
- Boundary: the stack is INSPECTED between the field and the file — the layer census and the slices are the canonical identity preimage — so the vectorize-then-write staging earns its place; where a lane writes a single field and reads nothing back, the collapsed single-call write is the form and no container materializes.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record CliImport(int Layers, BoundingBox Bounds, string HeaderDate, Seq<string> Warnings);

public sealed record CliStack(
    int Layers,
    ContentKey Key,
    Seq<ContentKey> Masks,
    Seq<ContentKey> Fields,
    Option<VoxelMetrics> Metrics,
    Option<CliImport> Import);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static partial class Sdf {
    public static Fin<CliStack> Cli(ImplicitOp operation, Option<IProgress<double>> progress = default) =>
        from _ in Admit(operation)
        from mode in operation.Policy.Cli.ToFin(Refusal("implicit-cli:mode-absent"))
        from stack in mode.Switch(
            state: (Operation: operation, Progress: progress),
            grayscale: static (state, mode) => Voxelize(Seq(state.Operation), scopes => Grayscale(scopes[0], state.Operation, mode)),
            cliVector: static (state, mode) => Voxelize(Seq(state.Operation),
                scopes => Vector(scopes[0], state.Operation, mode, state.Progress)),
            vdbCli: static (state, mode) => Direct(state.Operation, mode, state.Progress))
        select stack;

    private static Fin<CliStack> Vector(
        VoxelScope scope, ImplicitOp operation, CliMode.CliVector mode, Option<IProgress<double>> progress) =>
        Op.Of(name: "implicit-cli:vector").Catch(() => {
                IProgress<double>? sink = progress.ValueUnsafe();
                PolySliceStack slices = scope.Native.oVectorize(
                    (float)operation.Policy.LayerHeight.Millimeters,
                    mode.AbsoluteOrigin,
                    sink);
                mode.Target.Iter(target => CliIo.WriteSlicesToCliFile(
                    slices,
                    target.FullName,
                    mode.Format.Native,
                    strDate: null,
                    mode.UnitsInMillimeters,
                    sink));
                return ImplicitCanonical
                    .Cli(slices, Seq(scope.Metrics.Field), operation.Policy, Some(mode), Op.Of(name: nameof(Vector)))
                    .Map(key => new CliStack(
                        slices.nCount(), key, Seq<ContentKey>(), Seq(scope.Metrics.Field), Some(scope.Metrics), None));
            }, cause => VoxelRuntime.Provider(operation, cause));

    private static Fin<CliStack> Grayscale(VoxelScope scope, ImplicitOp operation, CliMode.Grayscale mode) =>
        Op.Of(name: "implicit-cli:grayscale").Catch(() => {
                ImageGrayScale image = scope.Native.imgAllocateSlice(out int voxelSlices, mode.Axis.Native);
                Seq<Length> elevations = mode.Sampling.Elevations(operation.Policy, voxelSlices);
                Fin<Seq<ContentKey>> masks = Fin.Succ(Seq<ContentKey>());
                for (int index = 0; index < elevations.Count; index++) {
                    mode.Sampling.Fetch(scope.Native, index, elevations[index], ref image, mode.Render, mode.Axis);
                    Fin<ContentKey> mask = ImplicitCanonical.Image(
                        index,
                        elevations[index],
                        image,
                        RasterFrame.Of(scope.Native, index),
                        scope.Metrics.Field,
                        operation.Policy,
                        Op.Of(name: nameof(Grayscale)));
                    masks = masks.Bind(held => mask.Map(held.Add));
                }
                return masks.Bind(settled => ImplicitCanonical
                    .MaskIndex(settled, scope.Metrics.Field, operation.Policy, mode, Op.Of(name: nameof(Grayscale)))
                    .Map(key => new CliStack(
                        settled.Count, key, settled, Seq(scope.Metrics.Field), Some(scope.Metrics), None)));
            }, cause => VoxelRuntime.Provider(operation, cause));

    private static Fin<CliStack> Direct(ImplicitOp operation, CliMode.VdbCli mode, Option<IProgress<double>> progress) =>
        operation is not ImplicitOp.Vdb vdb || !operation.Morphology.IsEmpty || !vdb.Origin.Path.Exists
            ? Fail<CliStack>(operation)
            : VoxelRuntime.Use(Seq(operation), () =>
                from _ in VdbMetadata(vdb.Origin, vdb.Policy.Budget.VoxelSizeMm)
                from stack in Op.Of(name: "implicit-cli:direct").Catch(() => {
                        Vdb2Cli.Convert(
                            vdb.Origin.Path.FullName,
                            (float)vdb.Policy.LayerHeight.Millimeters,
                            mode.Target.FullName,
                            vdb.Origin.Field.Value,
                            progress.ValueUnsafe());
                        CliIo.Result imported = CliIo.oSlicesFromCliFile(mode.Target.FullName);
                        return ImplicitCanonical
                            .Cli(imported.oSlices, Seq(vdb.Origin.Key), vdb.Policy, Option<CliMode.CliVector>.None,
                                Op.Of(name: nameof(Direct)))
                            .Map(key => new CliStack(
                                imported.oSlices.nCount(),
                                key,
                                Seq<ContentKey>(),
                                Seq(vdb.Origin.Key),
                                None,
                                Some(new CliImport(
                                    (int)imported.nLayers,
                                    FieldMath.Box(imported.oBBoxFile),
                                    imported.strHeaderDate,
                                    Witness.Keyed(imported.strWarnings)
                                        ? Seq(imported.strWarnings)
                                        : Seq<string>()))));
                    }, cause => VoxelRuntime.Provider(operation, cause))
                select stack);
}
```

## [09]-[CANONICAL_BYTES]

- Owner: `ImplicitCanonical` owns every layer-stack, mask, and mask-index KEY this page mints; each rides the `FabricationCanon` keyed close whole, so no lane opens a writer, spells a mint, or discards the close's own refusal.
- Law: a preimage carries NO provider enum ordinal — every closed selector writes through `FabricationCanon.Discriminant`, which frames the owned row's own key, so a provider renumbering its enum re-keys nothing. The writer binds to the operation's own voxel size, so the declared quantization grid is a policy axis rather than a spelled constant.
- Auto: `FabricationCanon.Rows` writes the count before its rows, so every collection layout is self-delimiting and no length-free concatenation can forge equality; `ContourWinding.Of` computes the winding from the contour's own vertices rather than reading a value the last detection happened to leave behind.
- Result: a mask preimage carries its `RasterFrame` — pitch, origin, and census — because a grayscale payload is addressable only with the grid beside it, so the same raster at two voxel sizes cannot mint one key.
- Packages: `Rasm.Element` `CanonicalWriter` through the ONE `FabricationCanon` family, `PicoGK` slice and contour reads.
- Growth: a new preimage is one method here composing the same family.
- Boundary: canonical keys include every behavior-bearing policy value, emission setting, and field identity; a float raster writes through the double primitive, which canonicalizes signed zero and every NaN payload, so a bit pattern the provider happens to emit cannot fork a key.

```csharp
public static class ImplicitCanonical {
    public static Fin<ContentKey> Cli(
        PolySliceStack slices,
        Seq<ContentKey> fields,
        ImplicitPolicy policy,
        Option<CliMode.CliVector> mode,
        Op key) =>
        FabricationCanon.Keyed(EgressKind.Cli, policy.Budget.VoxelSizeMm, writer => writer
            .Double(policy.LayerHeight.Millimeters)
            .Maybe(mode, static (row, value) => row
                .Discriminant(value.Format)
                .Double(value.UnitsInMillimeters)
                .Bool(value.AbsoluteOrigin))
            .Rows(fields, static (row, field) => field.CanonicalBytes(row))
            .Rows(toSeq(Enumerable.Range(0, slices.nCount())), (row, layer) => Layer(row, slices.oSliceAt(layer))),
            key);

    public static Fin<ContentKey> Image(
        int layer,
        Length elevation,
        ImageGrayScale image,
        RasterFrame frame,
        ContentKey field,
        ImplicitPolicy policy,
        Op key) =>
        FabricationCanon.Keyed(EgressKind.Cli, policy.Budget.VoxelSizeMm, writer => field
            .CanonicalBytes(writer
                .Ordinal(layer).Double(elevation.Millimeters)
                .Double(frame.VoxelSizeMm).Coords(frame.Origin)
                .Ordinal(frame.Columns).Ordinal(frame.Rows).Ordinal(frame.Layers)
                .Ordinal(image.nWidth).Ordinal(image.nHeight))
            .Rows(toSeq(image.m_afValues), static (row, value) => row.Double(value)),
            key);

    public static Fin<ContentKey> MaskIndex(
        Seq<ContentKey> masks,
        ContentKey field,
        ImplicitPolicy policy,
        CliMode.Grayscale mode,
        Op key) =>
        FabricationCanon.Keyed(EgressKind.Cli, policy.Budget.VoxelSizeMm, writer => field
            .CanonicalBytes(writer
                .Double(policy.LayerHeight.Millimeters)
                .Discriminant(mode.Render)
                .Discriminant(mode.Axis)
                .Discriminant(mode.Sampling))
            .Rows(masks, static (row, mask) => mask.CanonicalBytes(row)),
            key);

    private static CanonicalWriter Layer(CanonicalWriter writer, PolySlice slice) =>
        writer.Double(slice.fZPos())
            .Rows(toSeq(Enumerable.Range(0, slice.nContours())),
                (row, index) => Contour(row, slice.oContourAt(index)));

    private static CanonicalWriter Contour(CanonicalWriter writer, PolyContour contour) =>
        writer.Discriminant(ContourWinding.Of(contour))
            .Rows(toSeq(Enumerable.Range(0, contour.nCount())),
                (row, index) => contour.vecVertex(index) switch {
                    var vertex => row.Double(vertex.X).Double(vertex.Y),
                });
}
```

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

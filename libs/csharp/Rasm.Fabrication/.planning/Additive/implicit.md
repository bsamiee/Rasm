# [RASM_FABRICATION_IMPLICIT]

Implicit additive geometry admits one compatible operation set containing periodic fields, lattices, content-addressed voxel wires, or VDB sources; binds one operation-scoped PicoGK runtime; calibrates physical distance; consumes every native handle inside the lease; and projects only durable receipts. `FieldExpression` generates the periodic space and differentiates itself, `SpectralExpression` generates the frequency-domain space the same way, `FieldDefinition.Generated` carries per-occurrence programs without mutating `FieldKind.Items`, and `Implicit.Voxelize` scopes every native handle to one callback.

Wire posture is host-local: `Voxels`, `Lattice`, `Mesh`, `ScalarField`, `VectorField`, `OpenVdbFile`, and `Library.GlobalInstance` never cross a result boundary. Every provider selector this page admits is an OWNED `[SmartEnum<string>]` row carrying its PicoGK enum as a column, so `FabricationCanon.Discriminant` writes the row's own key into a preimage and a provider reordering its enum members re-keys nothing. Support geometry composes the ONE `SupportTopology` `support#SUPPORT_TOPOLOGY` publishes on `SupportPlan`; this page reconstructs no edge set. Refusals ride `FabricationFault` through `Admission.Admitted`, and every native failure carries the provider's own cause forward because the provider owns that taxonomy.

## [01]-[INDEX]

- [02]-[FIELD_PROGRAM]: `FieldKey`, `FieldSample`, `FieldExpression`, `FieldKind`, `FieldDefinition`, `TpmsForm`, `ImplicitCell`, `CalibrationPolicy`, `CalibrationStats`, `FieldThreshold`.
- [03]-[SPECTRAL_ALGEBRA]: `SpectralShape`, `SpectralMetric`, `SpectralExpression`, `SpectralSymbol`.
- [04]-[MORPHOLOGY]: `VoxelMorphologyStep`, `VoxelBoolean`, the one `Apply` rail, `SpectralMorphology`, `FilteredField`.
- [05]-[PROVIDER_VOCABULARY]: `SliceRender`, `SliceAxis`, `MaskSampling`, `CliFormat`, `ContourWinding`, `RasterFrame`.
- [06]-[OPERATION_SET]: `VdbSource`, `CliMode`, `ImplicitPolicy`, `VoxelWire`, `VoxelOperationKind`, `ImplicitOp`.
- [07]-[VOXEL_LEASE]: `VoxelRuntime`, `VoxelMetrics`, `VoxelScope`, `Rasterized`, `Implicit.Voxelize` and its build fold.
- [08]-[LAYER_EGRESS]: `CliImport`, `CliStack`, `Implicit.Cli` and the three egress lanes.
- [09]-[CANONICAL_BYTES]: `ImplicitCanonical`.

## [02]-[FIELD_PROGRAM]

- Owner: `FieldExpression` owns constant, wave, sum, product, minimum, maximum, and absolute program generation together with the closed-form gradient each case carries; `FieldKind` owns the common seed programs; `FieldDefinition` owns known and generated level-set programs; `ImplicitCell` owns the orthotropic period metric and the density, orientation, and scale drivers; `FieldCalibration` owns density quantiles and calibration evidence.
- Cases: `TpmsForm` splits solid from sheet, the sheet carrying the printable wall band its half-width clamps into.
- Law: `FieldExpression.At` returns level and gradient from ONE structural fold, so no sampling stencil, step size, or truncation error enters the distance law. Density grades wall thickness, axis grades orientation, and scale grades the period itself — three independent drivers a conformal lattice varies, so folding scale into density collapses two of them.
- Exemption: `FieldCalibration.Of` and `SampleAction.Invoke` are numeric kernels — pooled parallel partition fills over `ParallelHelper`, tensor reductions, and in-place sorts have no expression form.
- Entry: `FieldDefinition.Admit` is the one construction for both arms; `FieldCalibration.Of` is the one quantile pass.
- Auto: every generated owner refuses through `[ValidationError<FabricationFault>]`, so one `Admission.Admitted` read closes each admission and no hand ternary restates the `Validate` contract. `Resolution` derives the sample cube from the policy's own quantile-error target rather than a spelled grid.
- Packages: `System.Numerics.Tensors` (finite checks, extrema, moments, energy, subtraction, absolute transforms), `CommunityToolkit.HighPerformance` (`MemoryOwner<T>`, `SpanOwner<T>`, `Span2D<T>`, `ParallelHelper.For` over `struct IAction`), `UnitsNet`, LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a topology is `FieldExpression` data, a common topology one `FieldKind` seed row, a per-occurrence topology one `FieldDefinition.Generated`, and a spatial driver one `ImplicitCell` field column.
- Boundary: raw level equations never claim signed-distance semantics — the distance law divides the residual by the world gradient norm floored at the policy's own gradient floor, so a level set whose gradient vanishes reports a bounded distance rather than an infinity.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
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

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[ValueObject<string>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ValidationError<FabricationFault>]
public readonly partial struct FieldKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref FabricationFault? validationError, ref string value) {
        value = value.Trim();
        if (!Witness.Keyed(value))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Additive, "field-key");
    }

    public static Fin<FieldKey> Admit(string value) => Admission.OfValue<FieldKey, string>(value);
}

public readonly record struct FieldSample(float Level, Vector3 Gradient);

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

    public FieldSample At(Vector3 phase) => Switch(
        state: phase,
        constant: static (_, expression) => new FieldSample(expression.Value, Vector3.Zero),
        // The switch operand binds tighter than `+`, so the phase-shifted dot product parenthesizes as ONE angle:
        // without the parens the governing expression is `Phase` alone and every field goes position-independent.
        wave: static (value, expression) =>
            (Vector3.Dot(expression.Frequency, value) + expression.Phase) switch {
                var angle => new FieldSample(
                    expression.Amplitude * MathF.Cos(angle),
                    -expression.Amplitude * MathF.Sin(angle) * expression.Frequency),
            },
        sum: static (value, expression) => expression.Terms.Fold(
            new FieldSample(0.0f, Vector3.Zero),
            (total, term) => term.At(value) switch {
                var sample => new FieldSample(total.Level + sample.Level, total.Gradient + sample.Gradient),
            }),
        product: static (value, expression) => expression.Factors.Fold(
            new FieldSample(1.0f, Vector3.Zero),
            (total, term) => term.At(value) switch {
                var sample => new FieldSample(
                    total.Level * sample.Level,
                    (total.Gradient * sample.Level) + (sample.Gradient * total.Level)),
            }),
        // The extremum seeds are the identity of the fold, not an absence carrier: an admitted case holds at least
        // one term, so the seed is replaced on the first step and never reaches a caller.
        minimum: static (value, expression) => expression.Terms.Fold(
            new FieldSample(float.PositiveInfinity, Vector3.Zero),
            (held, term) => term.At(value) switch {
                var sample => sample.Level < held.Level ? sample : held,
            }),
        maximum: static (value, expression) => expression.Terms.Fold(
            new FieldSample(float.NegativeInfinity, Vector3.Zero),
            (held, term) => term.At(value) switch {
                var sample => sample.Level > held.Level ? sample : held,
            }),
        absolute: static (value, expression) => expression.Term.At(value) switch {
            var sample => new FieldSample(
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
[ValidationError<FabricationFault>]
public sealed partial class FieldKind {
    // The Lidinoid's own level offset — the constant term its published level-set equation carries, not a tuning knob.
    private const float LidinoidOffset = 0.15f;
    // A sine is a cosine a quarter period behind, so ONE wave case spells both trigonometric seeds.
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

    // A generated program may not shadow a seed row: two definitions answering one key mint one content identity
    // for two different level sets.
    public static Fin<FieldDefinition> Admit(FieldKey key, FieldExpression program) =>
        (AdmissionSlots.Gate(program.Valid, Refusal("generated-program")),
         AdmissionSlots.Gate(!FieldKind.TryGet(key.Value, out _), Refusal("generated-shadows-seed")))
            .Apply(static (_, _) => unit)
            .As()
            .ToFin()
            .Map(_ => (FieldDefinition)new Generated(key, program));

    public FieldKey Identity => Switch(
        known: static definition => FieldKey.Create(definition.Kind.Key),
        generated: static definition => definition.Key);

    public FieldSample At(Vector3 phase) => Switch(
        state: phase,
        known: static (value, definition) => definition.Kind.Program.At(value),
        generated: static (value, definition) => definition.Program.At(value));

    private static Error Refusal(string locus) =>
        new FabricationFault.PolicyInadmissible(FabConcern.Additive, $"implicit-field:{locus}");
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TpmsForm {
    private TpmsForm() { }

    public sealed record Solid : TpmsForm;
    public sealed record Sheet(Length MinimumWall, Length MaximumWall) : TpmsForm;

    // Sheet half-width is the calibrated quantile clamped into the printable wall band, never a bare floor:
    // an unbounded upper wall lets a graded density close the cell into solid stock.
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

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class CalibrationPolicy {
    // A quantile over a cube needs at least two samples per axis to separate one bin from another, so the floor is
    // the smallest admissible cube rather than a chosen count.
    private const int SampleFloor = 8;
    // Relative density is the fraction of the cell the solid phase occupies; at or above one half the sheet's own
    // complement closes, so the driver band is open on both sides of the half.
    private const double DensityCeiling = 0.5;

    public int MinimumSamples { get; }
    public int MaximumSamples { get; }
    public Ratio QuantileError { get; }
    public Ratio DensityFloor { get; }
    public double GradientFloorPerMillimeter { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
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
            || !Witness.Positive(gradientFloorPerMillimeter))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Additive, "calibration-policy");
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
[ValidationError<FabricationFault>]
public sealed partial class ImplicitCell {
    public Length PeriodX { get; }
    public Length PeriodY { get; }
    public Length PeriodZ { get; }
    public Matrix4x4 WorldToCell { get; }
    public Ratio RelativeDensity { get; }
    public Ratio FrameTolerance { get; }
    public Ratio MinimumScale { get; }

    // The three drivers are genuine EXTERNALS — a graded field is read from a provider source the caller owns, so
    // the column carries the reader rather than a shape this page could construct. A caller factory is not
    // VDB-bound: `new ScalarField()` is public and mints an empty sparse field at the ambient `Library`'s voxel
    // size, filled through `SetValue(Vector3, float)` — so a natural-neighbour grading reader composes the kernel
    // Sibson surface over its own sample set and hands the filled field here.
    public Option<Func<Fin<ScalarField>>> DensityField { get; }
    public Option<Func<Fin<VectorField>>> AxisField { get; }
    public Option<Func<Fin<ScalarField>>> ScaleField { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
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
        if (!(Arr(periodX, periodY, periodZ).ForAll(static period => Witness.Positive(period.Millimeters))
            && frameTolerance.DecimalFractions is > 0.0 and < 1.0
            && minimumScale.DecimalFractions is > 0.0 and <= 1.0
            // The frame tolerance IS the singularity floor: a world-to-cell map whose determinant falls under it
            // has collapsed an axis, and the period metric it feeds stops being a metric.
            && Math.Abs(worldToCell.GetDeterminant()) > frameTolerance.DecimalFractions
            && relativeDensity.DecimalFractions is > 0.0 and < 1.0))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Additive, "implicit-cell");
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
- Boundary: a symbol reads the bin's per-axis cycles-per-millimetre off the transform receipt's own axes and never re-derives a spectrum axis beside the lattice that produced it; the shape pair is the step's anisotropy ratio and cut-off wavelength, the ONE pair every metric reads.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
// The one shape pair every metric reads: the step's anisotropy ratio and its cut-off wavelength in millimetres.
public readonly record struct SpectralShape(double Anisotropy, double Wavelength);

[SmartEnum<string>]
[ValidationError<FabricationFault>]
public sealed partial class SpectralMetric {
    // Per-axis scaled norm — the anisotropic standard deviations are the wavelength stretched along X by the ratio.
    public static readonly SpectralMetric Anisotropic = new("anisotropic", static (frequency, shape) =>
        Math.Sqrt(
            Square(frequency.X * shape.Wavelength * shape.Anisotropy)
            + Square(frequency.Y * shape.Wavelength)
            + Square(frequency.Z * shape.Wavelength)));
    // Radial norm in wavelength units.
    public static readonly SpectralMetric Scaled = new("scaled", static (frequency, shape) =>
        Radius(frequency) * shape.Wavelength);
    // Radial offset from the pass centre in half-band units: the centre is the reciprocal wavelength and the band
    // half-width is that centre divided by the ratio, so the offset carries both in one product.
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
        // The Fourier derivative operator: differentiation in space is multiplication by the imaginary angular
        // frequency, so an order rides as a power rather than a second case per order.
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
[ValidationError<FabricationFault>]
public sealed partial class SpectralSymbol {
    // The Fourier-pair exponent of a unit-variance Gaussian: a spatial Gaussian of width s transforms to
    // exp(-2*pi^2*(f*s)^2), so every Gaussian row over a length-scaled metric carries this one coefficient.
    private const double GaussianDecay = 2.0 * Math.PI * Math.PI;
    // The band-pass metric already normalizes by its own half-width, so its response needs no further decay.
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

- Owner: `VoxelMorphologyStep` owns the transform vocabulary and the ONE `Apply` rail both its capsule classes answer on; `VoxelBoolean` owns the set operations over a rasterized set; `SpectralMorphology` owns the kernel-numeric boundary; `FilteredField` owns the reconstruction the filtered spectrum re-enters through.
- Cases: nine PicoGK rows are provider statement capsules; the `Spectral` row is a kernel-numeric-floor capsule that reaches no provider entry point.
- Law: `Spectral` crosses the boundary as SAMPLES on the budget lattice and returns as an `IImplicit` the provider rasterizes over the same bounds, so its failures are typed numeric refusals and never a native status. A spectral row lowered onto a PicoGK morphology call, or a page-local transform, window, frequency axis, or separability rule beside the kernel arena, is the deleted form.
- Exemption: `Apply`'s provider bodies, `SpectralMorphology.Rasterize`, and `FilteredField.fSignedDistance` are statement capsules — a native handle mutates or is replaced in place, and the reconstruction is a per-query lattice fold with no expression form.
- Entry: `VoxelMorphologyStep.Apply(Voxels, ImplicitPolicy)` is the one rail, so the morphology fold never learns which capsule class a step belongs to.
- Auto: the provider bracket releases the held handle on the failure arm, so a mid-chain native throw never strands a lease; the reconstruction interpolates TRILINEARLY over the eight surrounding cell centres, so the filtered field is continuous and the smoothness the spectral law spends a transform pair to obtain survives its return.
- Packages: `PicoGK` (`Voxels` morphology entry points, `IImplicit`), `Rasm.Numerics` (`CellLattice` the ONE addressing owner both sides of the boundary read, `SpectralArena`, `SpectralReceipt`, `SpectralSense`, `SpectralScaling`, `SignedAxis`, `PositiveMagnitude`), LanguageExt.Core.
- Growth: a native transform is one `VoxelMorphologyStep` case; a frequency-domain transform is one `SpectralSymbol` row under the single `Spectral` case.
- Boundary: a native failure carries the provider's own cause forward on the composed error, because the provider owns that taxonomy and the fabrication case names only the operation and its budget.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
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
        smoothen: static value => Witness.Positive(value.Distance.Millimeters),
        fillet: static value => Witness.Positive(value.Radius.Millimeters),
        doubleOffset: static value => double.IsFinite(value.First.Millimeters) && double.IsFinite(value.Second.Millimeters),
        tripleOffset: static value => double.IsFinite(value.Distance.Millimeters),
        trim: static value => value.Bounds.IsValid,
        projectZ: static value => double.IsFinite(value.Start.Millimeters)
            && double.IsFinite(value.End.Millimeters)
            && value.Start.Millimeters < value.End.Millimeters,
        spectral: static value => value.Symbol.Program.Valid
            && Witness.Positive(value.Anisotropy.DecimalFractions)
            && Witness.Positive(value.Wavelength.Millimeters));

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

    // One `Try` per statement capsule, the held handle released on the failure arm. A row's own body decides
    // whether it mutates the handle in place or replaces it, and the bracket is indifferent to which.
    private static Fin<Voxels> Provider(Voxels held, Func<Voxels, Voxels> body) =>
        Try.lift(() => body(held)).Run().As().ToFin().Rollback(held);
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

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
// The PicoGK boundary for the kernel numeric floor. The field crosses as SAMPLES, never as a handle: the budget
// lattice is the one addressing owner on both sides, the signed distances read through the `IImplicit` contract
// PicoGK already publishes on `Voxels`, and the filtered field re-enters as an implicit the provider rasterizes
// over the same bounds. `SpectralScaling.Symmetric` is the round-trip convention that makes the inverse exact.
file static class SpectralMorphology {
    internal static Fin<Voxels> Filter(Voxels held, VoxelMorphologyStep.Spectral step, ImplicitPolicy policy) {
        Op key = Op.Of(name: nameof(Filter));
        SpectralShape shape = new(step.Anisotropy.DecimalFractions, step.Wavelength.Millimeters);
        return from cell in key.AcceptValidated<PositiveMagnitude>(candidate: policy.Budget.VoxelSizeMm)
               from lattice in CellLattice.Of(
                   bounds: policy.Budget.Bounds, cell: cell, ceiling: policy.Budget.VoxelCap, key: key)
               from sampled in Try.lift(() => new SpectralArena.Interleaved(
                       [.. Enumerable.Range(0, (int)lattice.CellCount)
                           .Select(index => lattice.Coordinate(index))
                           .Select(at => lattice.Center(at.Column, at.Row, at.Layer))
                           .Select(point => new Complex(
                               held.fSignedDistance(new Vector3((float)point.X, (float)point.Y, (float)point.Z)), 0.0))],
                       lattice))
                   .Run().As().ToFin()
               from forward in sampled.Transform(SpectralSense.Forward, SpectralScaling.Symmetric, key)
               from axes in Seq(SignedAxis.X, SignedAxis.Y, SignedAxis.Z).TraverseM(axis => forward.Axis(axis, key)).As()
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

    // The filtered spectrum returns to the provider as an implicit over the SAME lattice, so the sampling grain
    // that left is the grain that comes back and no second addressing owner appears. The source handle releases
    // only once the replacement exists, and it releases on the success arm alone — the rollback owns the failure.
    private static Fin<Voxels> Rasterize(Voxels held, SpectralReceipt filtered, CellLattice lattice, ImplicitPolicy policy) =>
        Try.lift(() => {
            Voxels result = new(new FilteredField(held, filtered, lattice), FieldMath.Bounds(policy.Budget.Bounds));
            held.Dispose();
            return result;
        }).Run().As().ToFin().Rollback(held);
}

// The reconstructed field. A query inside the lattice interpolates TRILINEARLY across the eight surrounding cell
// centres, so the filtered field is continuous and the smoothness the transform pair purchased is not thrown away
// by a staircase read; the corner census clamps at the border, which is the zero-normal-derivative mirror the
// symmetric round trip already assumes. A query the lattice does not contain reads the SOURCE field, because the
// transform is periodic and a bin past the grid extent carries the wrap rather than geometry. The real part alone
// is the distance: a symmetric round trip leaves the imaginary residue at numerical zero, and publishing it would
// claim a component the transform never carried. The arena pattern-matches because only the interleaved layout
// addresses a 3D lattice — a match failure means the modulation moved the field onto a layout this cannot address.
file sealed class FilteredField(Voxels source, SpectralReceipt filtered, CellLattice lattice) : IImplicit {
    private readonly Complex[] values = filtered.Arena is SpectralArena.Interleaved grid ? grid.Values : [];

    public float fSignedDistance(in Vector3 point) {
        Point3d world = new(point.X, point.Y, point.Z);
        (int Column, int Row, int Layer) nearest = lattice.Nearest(world);
        if (values.Length != lattice.CellCount || !lattice.Contains(nearest.Column, nearest.Row, nearest.Layer))
            return source.fSignedDistance(point);

        // Cell CENTRES carry the samples and sit at the half-offset, so the interpolation origin is the centre
        // lattice: the fractional cell coordinate shifts down by a half before the corner census reads it.
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

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<FabricationFault>]
public sealed partial class SliceRender {
    public static readonly SliceRender SignedDistance = new("signed-distance", Voxels.ESliceMode.SignedDistance);
    public static readonly SliceRender BlackWhite = new("black-white", Voxels.ESliceMode.BlackWhite);
    public static readonly SliceRender Antialiased = new("antialiased", Voxels.ESliceMode.Antialiased);

    public Voxels.ESliceMode Native { get; }
}

[SmartEnum<string>]
[ValidationError<FabricationFault>]
public sealed partial class SliceAxis {
    public static readonly SliceAxis X = new("x", Voxels.ESliceAxis.X);
    public static readonly SliceAxis Y = new("y", Voxels.ESliceAxis.Y);
    public static readonly SliceAxis Z = new("z", Voxels.ESliceAxis.Z);

    public Voxels.ESliceAxis Native { get; }
}

[SmartEnum<string>]
[ValidationError<FabricationFault>]
public sealed partial class CliFormat {
    public static readonly CliFormat EmptyFirstLayer = new("empty-first-layer", CliIo.EFormat.UseEmptyFirstLayer);
    public static readonly CliFormat FirstLayerWithContent = new("first-layer-with-content", CliIo.EFormat.FirstLayerWithContent);

    public CliIo.EFormat Native { get; }
}

[SmartEnum<string>]
[ValidationError<FabricationFault>]
public sealed partial class ContourWinding {
    public static readonly ContourWinding Unknown = new("unknown", PolyContour.EWinding.UNKNOWN);
    public static readonly ContourWinding Clockwise = new("clockwise", PolyContour.EWinding.CLOCKWISE);
    public static readonly ContourWinding Counterclockwise = new("counterclockwise", PolyContour.EWinding.COUNTERCLOCKWISE);

    public PolyContour.EWinding Native { get; }

    private static readonly Lazy<FrozenDictionary<PolyContour.EWinding, ContourWinding>> Rows = new(
        static () => Items.ToFrozenDictionary(static row => row.Native),
        LazyThreadSafetyMode.ExecutionAndPublication);

    // The static fold is PURE and reads the contour's own vertices; the instance accessor reports whatever the last
    // detection left behind, so a key minted off it depends on how the contour was built.
    public static ContourWinding Of(PolyContour contour) => Rows.Value[PolyContour.eDetectWinding(contour.oVertices())];
}

// One allocated grayscale buffer is written across every slice, so the fetch column takes it by reference.
public delegate void SliceFetch(
    Voxels field, int index, Length elevation, ref ImageGrayScale image, SliceRender render, SliceAxis axis);

[SmartEnum<string>]
[ValidationError<FabricationFault>]
public sealed partial class MaskSampling {
    // The voxel-grid lane exposes on the provider's own slice census at the voxel pitch; the interpolated lane
    // exposes on the policy's layer height and reads between grid planes. Each row owns BOTH halves of its lane.
    public static readonly MaskSampling VoxelGrid = new("voxel-grid",
        elevations: static (policy, sliceCount) => toSeq(Enumerable.Range(0, sliceCount)).Map(index =>
            Length.FromMillimeters(policy.Budget.Bounds.Min.Z + ((index + 0.5) * policy.Budget.VoxelSizeMm))),
        fetch: static (Voxels field, int index, Length _, ref ImageGrayScale image, SliceRender render, SliceAxis axis) =>
            field.GetVoxelSlice(index, ref image, render.Native, axis.Native));
    public static readonly MaskSampling Interpolated = new("interpolated",
        elevations: static (policy, _) => Implicit.Elevations(policy.Budget.Bounds, policy.LayerHeight),
        fetch: static (Voxels field, int _, Length at, ref ImageGrayScale image, SliceRender render, SliceAxis _axis) =>
            field.GetInterpolatedVoxelSlice((float)at.Millimeters, ref image, render.Native));

    public Func<ImplicitPolicy, int, Seq<Length>> Elevations { get; }
    public SliceFetch Fetch { get; }
}

// The grid FRAME a raster preimage carries: pitch and origin make an `ImageGrayScale` payload addressable, so the
// same mask at two voxel sizes cannot mint one key. Named for the raster, not the slice: the kernel's `SliceFrame` is
// the slicing datum-and-extent frame, and one name over two frames in one folder is a silent wrong bind.
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
- Law: layer egress is REQUESTED rather than universal, so `ImplicitPolicy.Cli` carries `Option<CliMode>` and a voxelizing caller that posts no layer stack supplies nothing — the same absence carrier the run spine's own memo and progress columns take. A mode defaulted onto a caller that never asked for one is the deleted form, and the CLI rail reads presence rather than a sentinel encoding.
- Entry: `ImplicitPolicy.Admit` proves budget, layer height, and any requested egress mode ONCE, so the operation admission never re-checks a policy invariant and the budget's own cell ceiling is not re-tested downstream.
- Auto: `VoxelOperationKind` supplies the fault subject key, so a refusal names the operation through owned vocabulary rather than a member name captured at a call site.
- Packages: `PicoGK`, `Rasm.Fabrication.Process` (`VoxelBudget`, `ContentKey`, `EgressKind`, `FaultSubject`), LanguageExt.Core, Thinktecture.Runtime.Extensions.
- Growth: a new operation is one `ImplicitOp` case and one `VoxelOperationKind` row; a layer encoding is one `CliMode` case.
- Boundary: commit, wire read, wire write, and the three grading-field readers are the genuine EXTERNALS this page injects — each names a capability the caller owns — while every algorithm the page charters stays on the page.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[SmartEnum<string>]
[ValidationError<FabricationFault>]
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
        cliVector: static mode => Witness.Positive(mode.UnitsInMillimeters),
        vdbCli: static mode => mode.Target.Directory is { Exists: true });
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class VdbSource {
    public ContentKey Key { get; }
    public FileInfo Path { get; }
    public FieldKey Field { get; }
    public HashMap<string, string> RequiredMetadata { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref ContentKey key,
        ref FileInfo path,
        ref FieldKey field,
        ref HashMap<string, string> requiredMetadata) {
        if (requiredMetadata.IsEmpty || !requiredMetadata.ForAll(static pair => Witness.Keyed(pair.Key)))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Additive, "vdb-source");
    }

    public static Fin<VdbSource> Admit(
        ContentKey key, FileInfo path, FieldKey field, HashMap<string, string> requiredMetadata) =>
        Validate(key, path, field, requiredMetadata, out VdbSource source).Admitted(source);
}

[ComplexValueObject]
[ValidationError<FabricationFault>]
public sealed partial class ImplicitPolicy {
    public VoxelBudget Budget { get; }
    public Length LayerHeight { get; }
    public CalibrationPolicy Calibration { get; }
    public Func<Voxels, Fin<ContentKey>> Commit { get; }

    // Layer egress is REQUESTED, not universal: a voxelizing caller that never posts a layer stack has no mode to
    // name, so absence IS the second state and no sentinel encoding stands in for it. The column seats last
    // because it is the only optional one, so a non-CLI caller spells nothing at all.
    public Option<CliMode> Cli { get; }

    // `VoxelBudget.Admit` already proved bounds, voxel size, cap, and required-cell fit, so this admission proves
    // only what the policy itself owns and no downstream gate re-tests the budget.
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref FabricationFault? validationError,
        ref VoxelBudget budget,
        ref Length layerHeight,
        ref CalibrationPolicy calibration,
        ref Func<Voxels, Fin<ContentKey>> commit,
        ref Option<CliMode> cli) {
        if (!Witness.Positive(layerHeight.Millimeters) || !cli.ForAll(static mode => mode.Valid))
            validationError = new FabricationFault.PolicyInadmissible(FabConcern.Additive, "implicit-policy");
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

// Root columns carry what EVERY case reads. A derived case repeating a base positional property passes it through
// rather than re-declaring it, so `Policy` and `Morphology` are one property each across the whole union.
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

    // One expression closes every commit: a case owning a wire seats it as the sink, and the policy sink answers
    // for the rest.
    public Fin<ContentKey> Commit(Voxels voxels) =>
        Sink.Map(wire => wire.FromVoxels(voxels)).IfNone(() => Policy.Commit(voxels));

    // The whole operation tree, the compatibility census reads. Leaves carry an empty `Nested`, so the walk needs
    // no arm and a new composing case joins it by seating its inputs on the root column.
    public Seq<ImplicitOp> Expanded => Seq(this) + Nested.Bind(static row => row.Expanded);
}
```

## [07]-[VOXEL_LEASE]

- Owner: `VoxelRuntime` owns PicoGK ambient lifetime; `VoxelScope` owns metrics, mesh, ray, and VDB projection inside one lease; `Implicit.Voxelize` owns the materializing rail and the build fold under it.
- Law: `VoxelRuntime.Use` serializes one `Library.GlobalInstance` per compatible operation set, so sequential sets may select distinct voxel sizes. A returned native handle is invalid egress; every handle allocated in the fold is consumed inside the callback and released on both arms.
- Exemption: `VoxelRuntime.Use`, `Consume`, `Combine`, `LatticeVoxels`, `Subtract`, `Measure`, and `Sealed` are lifetime and provider statement capsules — a lease, an ordered release, and a native aggregate have no expression form.
- Entry: `Implicit.Voxelize<T>(Seq<ImplicitOp>, Func<Arr<VoxelScope>, Fin<T>>)` is the single materializing rail for one or many compatible fields.
- Auto: admission accumulates every operation fault before native allocation. `Raster` intersects the envelope with the implicit through `voxIntersectImplicit` rather than rasterizing the whole budget box, because a full-bounds construction allocates the entire budget before discarding almost all of it. `Occupied` rejects an empty rasterization before it posts an empty program. Lattice scaffolds read the ONE `SupportTopology` `support#SUPPORT_TOPOLOGY` publishes, so the support edge set is never reconstructed and a missing parent is impossible by the owner's own admission.
- Receipt: `VoxelMetrics` carries physical volume, queried bounds, native memory, committed field identity, and the `CalibrationStats` the quantile pass measured.
- Packages: `PicoGK` (implicit rasterization and intersection, lattice beams and nodes, metrics, ray-cast, mesh extraction, VDB read and write with field metadata), QuikGraph (`SEquatableEdge` endpoints off the published topology), LanguageExt.Core.
- Growth: a materializing consumer is one `Voxelize` callback.
- Boundary: `PeriodicImplicit.fSignedDistance` copies the provider's by-reference callback value before use; VDB source identity travels with its field name and required metadata; the document container is what NAMES a field, so the direct single-field write cannot serve an egress whose import lane resolves by name.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
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
            ? Fin.Fail<Option<Point3d>>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "implicit-ray:invalid").ToError())
            : Fin.Succ(Native.bRayCastToSurface(
                FieldMath.Vector(origin),
                new Vector3((float)direction.X, (float)direction.Y, (float)direction.Z),
                out Vector3 hit)
                    ? Some(new Point3d(hit.X, hit.Y, hit.Z))
                    : None);

    // Export closes the VDB round trip the import lane opens; provenance travels as field metadata so a
    // re-imported field carries the identity its required-metadata gate compares.
    public Fin<ContentKey> Vdb(FileInfo target, FieldKey field, HashMap<string, string> provenance) =>
        target.Directory is not { Exists: true }
            ? Fin.Fail<ContentKey>(new FabricationFault.PolicyInadmissible(FabConcern.Additive, "implicit-vdb:export-target"))
            : Try.lift(() => {
                    using OpenVdbFile file = new();
                    _ = file.nAdd(Native, field.Value);
                    using FieldMetadata metadata = Native.oMetaData();
                    provenance.Iter(pair => metadata.SetValue(pair.Key, pair.Value));
                    file.SaveToFile(target.FullName);
                    return Metrics.Field;
                })
                .Run().As().ToFin()
                .MapFail(static error =>
                    new GeometryFault.DegenerateInput(Kind.Mesh, None, "implicit-vdb:export").ToError() + error);
}

// --- [RUNTIME] ------------------------------------------------------------------------------------------------------------------------------------
file static class VoxelRuntime {
    private static readonly Lock Gate = new();

    public static Fin<T> Use<T>(Seq<ImplicitOp> operations, Func<Fin<T>> run) {
        ImplicitOp operation = operations[0];
        lock (Gate) {
            return Try.lift<Fin<T>>(() => {
                    using Library.GlobalInstance runtime = new((float)operation.Policy.Budget.VoxelSizeMm);
                    return run();
                })
                .Run().As().ToFin()
                .MapFail(error => Native(operation, error))
                .Bind(static result => result);
        }
    }

    // The native cause travels WITH the fabrication case: the case names the operation and its budget, and the
    // provider owns the taxonomy of why its own call failed.
    internal static Error Native(ImplicitOp operation, Error cause) =>
        (Error)new FabricationFault.VoxelFault(operation.Subject, operation.Policy.Budget) + cause;
}

file static class VoxelRail {
    extension<T>(Fin<T> step) {
        public Fin<T> Rollback(params ReadOnlySpan<IDisposable?> held) {
            Seq<IDisposable?> captured = toSeq(held.ToArray());
            return step.MapFail(error => {
                _ = captured.Iter(static lease => lease?.Dispose());
                return error;
            });
        }
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static partial class Implicit {
    public static Fin<T> Voxelize<T>(Seq<ImplicitOp> operations, Func<Arr<VoxelScope>, Fin<T>> consume) =>
        operations.IsEmpty
            ? Fin.Fail<T>(new FabricationFault.PolicyInadmissible(FabConcern.Additive, "implicit:empty-operation-set"))
            : from _ in operations.Traverse(operation => Admit(operation).ToValidation()).As().ToFin()
              from __ in Gate(Compatible(operations), "implicit:incompatible-operation-set").ToFin()
              from result in VoxelRuntime.Use(operations, () =>
                  from rasters in Build(operations)
                  from consumed in Consume(operations, rasters, consume)
                  select consumed)
              select result;

    private static Fin<T> Consume<T>(
        Seq<ImplicitOp> operations,
        Seq<Rasterized> rasters,
        Func<Arr<VoxelScope>, Fin<T>> consume) {
        try {
            return from scopes in operations.Zip(rasters)
                       .Traverse(row =>
                           from _ in Occupied(row.Second.Voxels, row.First)
                           from field in row.First.Commit(row.Second.Voxels)
                           select new VoxelScope(row.Second.Voxels, Measure(row.Second, field)))
                       .As()
                   from result in consume(scopes.ToArr())
                   select result;
        }
        finally {
            rasters.Iter(static raster => raster.Voxels.Dispose());
        }
    }

    // Empty fields rasterize without fault and post an empty program; the gate turns that into evidence.
    private static Fin<Unit> Occupied(Voxels voxels, ImplicitOp operation) =>
        voxels.bIsEmpty() ? Fail<Unit>(operation) : Fin.Succ(unit);

    private static Fin<Seq<Rasterized>> Build(Seq<ImplicitOp> operations) =>
        operations.Fold(
            Fin.Succ(Seq<Rasterized>()),
            static (rail, operation) =>
                from held in rail
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
        Try.lift(() => {
                Voxels result = inputs[0].voxDuplicate();
                try {
                    _ = operation.Apply(result, inputs.Skip(1));
                    return result;
                }
                catch {
                    result.Dispose();
                    throw;
                }
                finally {
                    inputs.Iter(static input => input.Dispose());
                }
            })
            .Run().As().ToFin()
            .MapFail(static error =>
                new GeometryFault.DegenerateInput(Kind.Mesh, None, "implicit-composite").ToError() + error);

    private static Fin<Rasterized> Field(ImplicitOp.Field operation) =>
        from density in Acquire(operation.Cell.DensityField)
        from scale in Acquire(operation.Cell.ScaleField).Rollback(Lease(density))
        from axis in Acquire(operation.Cell.AxisField).Rollback(Lease(density), Lease(scale))
        from raster in Raster(operation, density, scale, axis)
            .Rollback(Lease(density), Lease(scale), Lease(axis))
        select Released(raster, density, scale, axis);

    // `voxIntersectImplicit` rasterizes the field only where the envelope already has occupancy; a full-bounds
    // construction allocates the whole budget box before discarding almost all of it.
    private static Fin<Rasterized> Raster(
        ImplicitOp.Field operation,
        Option<ScalarField> density,
        Option<ScalarField> scale,
        Option<VectorField> axis) =>
        from envelope in operation.Envelope.ToVoxels()
        from calibration in FieldCalibration
            .Of(operation.Definition, operation.Cell, operation.Policy.Calibration)
            .Rollback(envelope)
        from intersected in Try
            .lift(() => envelope.voxIntersectImplicit(new PeriodicImplicit(
                operation.Definition,
                operation.Form,
                operation.Cell,
                FieldMath.Bounds(operation.Policy.Budget.Bounds),
                density,
                scale,
                axis,
                calibration)))
            .Run().As().ToFin()
            .Rollback(envelope, calibration)
        from morphed in Morph(intersected, operation.Morphology, operation.Policy)
            .Rollback(envelope, calibration)
        select Sealed(morphed, envelope, calibration);

    // The calibration's evidence outlives its buffers, so the stats are read BEFORE the release and the seal is a
    // named capsule rather than an assignment smuggled into a query.
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

    // The support edge set has ONE owner. `SupportTopology` publishes the graph and its by-id index together, so
    // every endpoint read is total by the owner's own admission and no parent lookup can fail here.
    private static Fin<Voxels> Lattice(
        SupportPlan support,
        Option<VoxelWire> part,
        Seq<VoxelMorphologyStep> morphology,
        ImplicitPolicy policy) =>
        from scaffold in LatticeVoxels(support.Topology)
        from result in part.Map(wire => Subtract(scaffold, wire)).IfNone(() => Fin.Succ(scaffold))
        from morphed in Morph(result, morphology, policy)
        select morphed;

    // Endpoints resolve through the owner's TOTAL read, so an ordinal its census does not carry answers a typed
    // refusal rather than throwing out of an indexer this fold cannot guard. Edges point parent to child; a beam
    // carries a radius at each end and is unchanged under swapping both endpoint and radius together, so the
    // scaffold solid is direction-agnostic and only the naming records which end is which.
    private static Fin<Voxels> LatticeVoxels(SupportTopology topology) =>
        from beams in toSeq(topology.Graph.Edges).Traverse(edge =>
                from parent in topology.Node(edge.Source).ToFin(Refusal("implicit-lattice:absent-node"))
                from child in topology.Node(edge.Target).ToFin(Refusal("implicit-lattice:absent-node"))
                select (Parent: parent, Child: child))
            .As()
        from voxels in Try.lift(() => {
                using PicoGK.Lattice lattice = new();
                // `Nodes` is the owner's id-ordered census, so the scaffold builds in one deterministic order and
                // its content key does not depend on dictionary enumeration. `Radius` is the owner's own
                // millimetre projection — re-deriving it off the `Length` column forks the unit read.
                topology.Nodes.Iter(node => lattice.AddSphere(FieldMath.Vector(node.At), (float)node.Radius));
                beams.Iter(beam => lattice.AddBeam(
                    FieldMath.Vector(beam.Parent.At),
                    (float)beam.Parent.Radius,
                    FieldMath.Vector(beam.Child.At),
                    (float)beam.Child.Radius,
                    bRoundCap: true));
                return new Voxels(lattice);
            })
            .Run().As().ToFin()
            .MapFail(static error =>
                new GeometryFault.DegenerateInput(Kind.Mesh, None, "implicit-lattice").ToError() + error)
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
        from voxels in Try.lift(() => {
                using OpenVdbFile file = new(source.Path.FullName);
                using Voxels field = file.voxGet(source.Field.Value);
                return field.voxDuplicate();
            })
            .Run().As().ToFin()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, None, "implicit-vdb").ToError() + error)
        select voxels;

    private static Fin<Unit> VdbMetadata(VdbSource source, double voxelSizeMm) =>
        VdbIdentity(source).Bind(_ => Try.lift<Fin<Unit>>(() => {
                using OpenVdbFile file = new(source.Path.FullName);
                if (!file.bIsPicoGKCompatible() || !file.fPicoGKVoxelSizeMM().Equals((float)voxelSizeMm))
                    return Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Additive, "implicit-vdb:voxel-size"));

                using Voxels field = file.voxGet(source.Field.Value);
                using FieldMetadata metadata = field.oMetaData();
                return source.RequiredMetadata.ForAll(pair =>
                    metadata.bGetValueAt(pair.Key, out string actual)
                    && string.Equals(actual, pair.Value, StringComparison.Ordinal))
                        ? Fin.Succ(unit)
                        : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Additive, "implicit-vdb:metadata"));
            })
            .Run().As().ToFin()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, None, "implicit-vdb").ToError() + error)
            .Bind(static result => result));

    private static Fin<Unit> VdbIdentity(VdbSource source) =>
        Try.lift(() => {
                using FileStream payload = source.Path.OpenRead();
                long canonicalLength = sizeof(int) + Encoding.UTF8.GetByteCount(source.Field.Value) + payload.Length;
                if (canonicalLength > int.MaxValue)
                    return false;

                // Kernel `ContentHash` owns the accumulator AND its explicit seed zero, so a digest minted here
                // lands inside the one federation key space rather than beside it under an implicit default seed.
                byte[] kind = Encoding.UTF8.GetBytes(source.Key.Kind.Key);
                byte[] field = Encoding.UTF8.GetBytes(source.Field.Value);
                return ContentHash.Of(
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
                    }) == source.Key.Digest;
            })
            .Run().As().ToFin()
            .MapFail(static error =>
                new GeometryFault.DegenerateInput(Kind.Mesh, None, "implicit-vdb:identity").ToError() + error)
            .Bind(matches => matches
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new FabricationFault.PolicyInadmissible(FabConcern.Additive, "implicit-vdb:identity")));

    // Every step owns its own bracket — the provider rows release the held handle on a native throw and the
    // spectral row releases it on the kernel rail — so the fold is a plain `Fin` chain with no second try layer.
    private static Fin<Voxels> Morph(Voxels voxels, Seq<VoxelMorphologyStep> steps, ImplicitPolicy policy) =>
        steps.Fold(Fin.Succ(voxels), (rail, step) => rail.Bind(held => step.Apply(held, policy)));

    private static Fin<Option<T>> Acquire<T>(Option<Func<Fin<T>>> source) where T : class, IDisposable =>
        Try.lift<Fin<Option<T>>>(() => source.Match(
                None: static () => Fin.Succ(Option<T>.None),
                Some: static factory => factory().Map(Some)))
            .Run().As().ToFin()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, None, "implicit-driver").ToError() + error)
            .Bind(static result => result);

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

    // The operation admission proves what the OPERATION owns; `ImplicitPolicy.Admit` and `VoxelBudget.Admit`
    // already closed policy, budget, and egress mode, so nothing here re-tests them.
    // `FieldDefinition.Admit` already proved its program, so the field arm gates only the form — the one shape on
    // this union with no admission of its own.
    private static Fin<Unit> Admit(ImplicitOp operation) =>
        AdmissionSlots.Accumulate(Seq(
                Gate(operation.Morphology.ForAll(static step => step.Valid), "implicit:morphology"),
                operation.Switch(
                    field: static row => Gate(row.Form.Valid, "implicit:field-form"),
                    latticeSupport: static row => Gate(
                        !row.Support.Topology.Graph.IsVerticesEmpty, "implicit:lattice-support"),
                    source: static _ => Nothing,
                    vdb: static row => Gate(row.Origin.Path.Exists, "implicit:vdb-path"),
                    composite: static row => AdmissionSlots.Accumulate(
                        Seq(Gate(!row.Inputs.IsEmpty, "implicit:composite-inputs"))
                        + row.Inputs.Map(static input =>
                            (K<Validation<Error>, Unit>)Admit(input).ToValidation())))))
            .As()
            .ToFin()
            .Map(static _ => unit);

    // A source operation carries only externals the caller owns, so its arm proves nothing past the shared
    // morphology gate; the empty accumulation IS that statement, and a fabricated always-true gate is not.
    private static K<Validation<Error>, Unit> Nothing =>
        AdmissionSlots.Accumulate(Seq<K<Validation<Error>, Unit>>());

    private static K<Validation<Error>, Unit> Gate(bool valid, string locus) =>
        AdmissionSlots.Gate(valid, Refusal(locus));

    private static Error Refusal(string locus) => new FabricationFault.PolicyInadmissible(FabConcern.Additive, locus);

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
        Fin.Fail<T>(new FabricationFault.VoxelFault(operation.Subject, operation.Policy.Budget));
}

// --- [FIELD] --------------------------------------------------------------------------------------------------------------------------------------
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

    // The ONE provider-bounds lift. Three call sites read it, so a fourth cannot spell a different corner order.
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

    // The provider hands its sample point by reference; the value copies before it reaches the fold.
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
                    new GeometryFault.DegenerateInput(Kind.Mesh, None, "implicit-calibration:non-finite").ToError());

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
            // The quantile read is an ORDER statistic, so both channels sort once here and every threshold read
            // is an index rather than a scan.
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

    // The cube side that makes a quantile estimate meet the policy's error target, floored at the policy's own
    // minimum cube and ceilinged at its maximum.
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
            FieldSample sample = definition.At(phase);
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

- Owner: `Implicit.Cli` owns the single layer-stack rail and the three egress lanes under it; `CliStack` owns the settled layer receipt; `CliImport` owns the reader receipt a round trip produces.
- Law: the sink enters as the runtime's own `Option<IProgress<double>>` carrier, absent by default so a headless caller spells nothing, and reaches the provider's trailing parameter directly on all three egress entry points — a page-local reporter, a percentage tally, or a polling thread beside the native call is the deleted form. Vectorize and write are two reported phases of ONE egress and take the SAME sink, so a caller never sees the file phase stall silently.
- Exemption: `Vector`, `Grayscale`, and `Direct` are provider statement capsules — the grayscale loop in particular owns its statement form because PicoGK writes every slice into one mutable `ref ImageGrayScale` buffer.
- Entry: `Implicit.Cli(ImplicitOp, Option<IProgress<double>>)` is the one layer-stack rail; `slicing#DEPOSITION` threads its implicit arm into this signature.
- Auto: the mask loop reads its elevation law and its slice fetch off ONE admitted `MaskSampling` row, so the lane branches nowhere; header date, unit scale, and reader warnings ride `CliImport`, because discarding them loses the only evidence that a round-tripped program degraded.
- Receipt: `CliStack` carries layers, canonical `.cli` identity, mask identities, committed field identities, optional `VoxelMetrics`, and the optional reader receipt.
- Packages: `PicoGK` (`oVectorize`, `CliIo`, `Vdb2Cli`, grayscale slice reads), LanguageExt.Core.
- Growth: a layer encoding is one `CliMode` case and one lane here.
- Boundary: the stack is INSPECTED between the field and the file — the layer census and the slices are the canonical identity preimage — so the vectorize-then-write staging earns its place; where a lane writes a single field and reads nothing back, the collapsed single-call write is the form and no container materializes.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record CliImport(int Layers, BoundingBox Bounds, string HeaderDate, Seq<string> Warnings);

public sealed record CliStack(
    int Layers,
    ContentKey Key,
    Seq<ContentKey> Masks,
    Seq<ContentKey> Fields,
    Option<VoxelMetrics> Metrics,
    Option<CliImport> Import);

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static partial class Implicit {
    // The lane READS presence: a policy that requested no layer egress cannot answer this rail, so the absent
    // mode refuses here rather than being defaulted into an encoding the caller never asked for.
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
        Try.lift(() => {
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
                ContentKey key = ContentKey.Of(
                    EgressKind.Cli,
                    ImplicitCanonical.Cli(slices, Seq(scope.Metrics.Field), operation.Policy, Some(mode)));
                return new CliStack(
                    slices.nCount(), key, Seq<ContentKey>(), Seq(scope.Metrics.Field), Some(scope.Metrics), None);
            })
            .Run().As().ToFin()
            .MapFail(error => VoxelRuntime.Native(operation, error));

    private static Fin<CliStack> Grayscale(VoxelScope scope, ImplicitOp operation, CliMode.Grayscale mode) =>
        Try.lift(() => {
                ImageGrayScale image = scope.Native.imgAllocateSlice(out int voxelSlices, mode.Axis.Native);
                Seq<Length> elevations = mode.Sampling.Elevations(operation.Policy, voxelSlices);
                Seq<ContentKey> masks = Seq<ContentKey>();
                for (int index = 0; index < elevations.Count; index++) {
                    mode.Sampling.Fetch(scope.Native, index, elevations[index], ref image, mode.Render, mode.Axis);
                    masks = masks.Add(ContentKey.Of(
                        EgressKind.Cli,
                        ImplicitCanonical.Image(
                            index,
                            elevations[index],
                            image,
                            RasterFrame.Of(scope.Native, index),
                            scope.Metrics.Field,
                            operation.Policy)));
                }
                ContentKey key = ContentKey.Of(
                    EgressKind.Cli,
                    ImplicitCanonical.MaskIndex(masks, scope.Metrics.Field, operation.Policy, mode));
                return new CliStack(masks.Count, key, masks, Seq(scope.Metrics.Field), Some(scope.Metrics), None);
            })
            .Run().As().ToFin()
            .MapFail(error => VoxelRuntime.Native(operation, error));

    // The direct lane converts a VDB file straight to a layer program, so it admits only a morphology-free VDB
    // operation: a step the conversion never sees would leave the emitted program describing a different solid.
    private static Fin<CliStack> Direct(ImplicitOp operation, CliMode.VdbCli mode, Option<IProgress<double>> progress) =>
        operation is not ImplicitOp.Vdb vdb || !operation.Morphology.IsEmpty || !vdb.Origin.Path.Exists
            ? Fail<CliStack>(operation)
            : VoxelRuntime.Use(Seq(operation), () =>
                from _ in VdbMetadata(vdb.Origin, vdb.Policy.Budget.VoxelSizeMm)
                from stack in Try.lift(() => {
                        Vdb2Cli.Convert(
                            vdb.Origin.Path.FullName,
                            (float)vdb.Policy.LayerHeight.Millimeters,
                            mode.Target.FullName,
                            vdb.Origin.Field.Value,
                            progress.ValueUnsafe());
                        CliIo.Result imported = CliIo.oSlicesFromCliFile(mode.Target.FullName);
                        ContentKey key = ContentKey.Of(
                            EgressKind.Cli,
                            ImplicitCanonical.Cli(
                                imported.oSlices, Seq(vdb.Origin.Key), vdb.Policy, Option<CliMode.CliVector>.None));
                        return new CliStack(
                            imported.oSlices.nCount(),
                            key,
                            Seq<ContentKey>(),
                            Seq(vdb.Origin.Key),
                            None,
                            Some(new CliImport(
                                (int)imported.nLayers,
                                FieldMath.Box(imported.oBBoxFile),
                                imported.strHeaderDate,
                                Witness.Keyed(imported.strWarnings) ? Seq(imported.strWarnings) : Seq<string>())));
                    })
                    .Run().As().ToFin()
                    .MapFail(error => VoxelRuntime.Native(operation, error))
                select stack);
}
```

## [09]-[CANONICAL_BYTES]

- Owner: `ImplicitCanonical` owns every layer-stack, mask, and mask-index preimage this page mints.
- Law: a preimage carries NO provider enum ordinal — every closed selector writes through `FabricationCanon.Discriminant`, which frames the owned row's own key, so a provider renumbering its enum re-keys nothing. The writer binds to the operation's own voxel size, so the declared quantization grid is a policy axis rather than a spelled constant.
- Auto: `FabricationCanon.Rows` writes the count before its rows, so every collection layout is self-delimiting and no length-free concatenation can forge equality; `ContourWinding.Of` computes the winding from the contour's own vertices rather than reading a value the last detection happened to leave behind.
- Receipt: a mask preimage carries its `RasterFrame` — pitch, origin, and census — because a grayscale payload is addressable only with the grid beside it, so the same raster at two voxel sizes cannot mint one key.
- Packages: `Rasm.Element` `CanonicalWriter` through the ONE `FabricationCanon` family, `PicoGK` slice and contour reads.
- Growth: a new preimage is one method here composing the same family.
- Boundary: canonical keys include every behavior-bearing policy value, emission setting, and field identity; a float raster writes through the double primitive, which canonicalizes signed zero and every NaN payload, so a bit pattern the provider happens to emit cannot fork a key.

```csharp signature
public static class ImplicitCanonical {
    public static byte[] Cli(
        PolySliceStack slices,
        Seq<ContentKey> fields,
        ImplicitPolicy policy,
        Option<CliMode.CliVector> mode) {
        CanonicalWriter writer = new(policy.Budget.VoxelSizeMm);
        writer.Double(policy.LayerHeight.Millimeters)
            .Maybe(mode, static (row, value) => row
                .Discriminant(value.Format)
                .Double(value.UnitsInMillimeters)
                .Bool(value.AbsoluteOrigin))
            .Rows(fields, static (row, key) => key.CanonicalBytes(row))
            .Rows(toSeq(Enumerable.Range(0, slices.nCount())), (row, layer) => Layer(row, slices.oSliceAt(layer)));
        return writer.ToBytes().ToArray();
    }

    public static byte[] Image(
        int layer,
        Length elevation,
        ImageGrayScale image,
        RasterFrame frame,
        ContentKey field,
        ImplicitPolicy policy) {
        CanonicalWriter writer = new(policy.Budget.VoxelSizeMm);
        writer.Ordinal(layer).Double(elevation.Millimeters)
            .Double(frame.VoxelSizeMm).Coords(frame.Origin)
            .Ordinal(frame.Columns).Ordinal(frame.Rows).Ordinal(frame.Layers)
            .Ordinal(image.nWidth).Ordinal(image.nHeight);
        field.CanonicalBytes(writer)
            .Rows(toSeq(image.m_afValues), static (row, value) => row.Double(value));
        return writer.ToBytes().ToArray();
    }

    public static byte[] MaskIndex(
        Seq<ContentKey> masks,
        ContentKey field,
        ImplicitPolicy policy,
        CliMode.Grayscale mode) {
        CanonicalWriter writer = new(policy.Budget.VoxelSizeMm);
        writer.Double(policy.LayerHeight.Millimeters)
            .Discriminant(mode.Render)
            .Discriminant(mode.Axis)
            .Discriminant(mode.Sampling);
        field.CanonicalBytes(writer).Rows(masks, static (row, key) => key.CanonicalBytes(row));
        return writer.ToBytes().ToArray();
    }

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
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

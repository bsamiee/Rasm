# [RASM_FABRICATION_IMPLICIT]

Implicit additive geometry admits one compatible operation set containing periodic fields, lattices, content-addressed voxel wires, or VDB sources; binds one operation-scoped PicoGK runtime; calibrates physical distance; consumes every native handle inside the lease; and projects only durable receipts. `FieldExpression` generates the periodic space and differentiates itself, `FieldKind` retains common seed programs, `FieldDefinition.Generated` carries per-occurrence programs without mutating `FieldKind.Items`, and `Implicit.Voxelize` scopes every native handle to one callback.

Wire posture is host-local. `Voxels`, `Lattice`, `Mesh`, `ScalarField`, `VectorField`, `OpenVdbFile`, and `Library.GlobalInstance` never cross a result boundary.

## [01]-[INDEX]

- [01]-[IMPLICIT]: owns `FieldExpression`, `FieldDefinition`, `FieldKind`, `TpmsForm`, `ImplicitCell`, `FieldCalibration`, `VoxelMorphologyStep`, `CliMode`, `ImplicitPolicy`, `VoxelWire`, `VoxelScope`, `CliStack`, `CliImport`, `ImplicitOp`, `VoxelRuntime`, `PeriodicImplicit`, `ImplicitCanonical`, `Implicit.Voxelize`, and `Implicit.Cli`.

## [02]-[IMPLICIT]

- Owner: `FieldExpression` owns constant, wave, sum, product, minimum, maximum, and absolute program generation together with the closed-form gradient each case carries; `FieldDefinition` owns known and generated level-set programs; `ImplicitCell` owns the orthotropic period metric and the density, orientation, and scale drivers; `FieldCalibration` owns density quantiles and calibration evidence; `VoxelRuntime` owns PicoGK ambient lifetime; `VoxelScope` owns metrics, mesh, ray, and VDB projection inside one lease; `Implicit` owns voxel and `.cli` egress.
- Entry: `Implicit.Voxelize<T>(Seq<ImplicitOp>, Func<Arr<VoxelScope>, Fin<T>>)` is the single materializing rail for one or many compatible fields; `Implicit.Cli(ImplicitOp)` is the single layer-stack rail, and every mode enters the `VoxelRuntime.Use` lease through `Voxelize` or `Direct`.
- Auto: admission accumulates policy, field, morphology, VDB, and support faults before native allocation. `VoxelRuntime.Use` serializes one `Library.GlobalInstance` per compatible operation set, so sequential sets may select distinct voxel sizes. `FieldExpression.At` returns level and gradient from one structural fold, so no sampling stencil, step size, or truncation error enters the distance law. `Raster` intersects the envelope with the implicit through `voxIntersectImplicit` rather than rasterizing the whole budget box. `FieldCalibration` derives sample resolution from policy, fills pooled planes through `ParallelHelper`, composes tensor reductions, and keeps sheet calibration centered on the neutral isosurface. `Occupied` rejects an empty rasterization before it posts an empty program.
- Receipt: `CliStack` carries layers, canonical `.cli` identity, mask identities, committed field identities, optional `VoxelMetrics`, and the optional `CliImport` reader receipt. `VoxelMetrics` carries physical volume, queried bounds, native memory, committed field identity, and the `CalibrationStats` the quantile pass measured. Native handles remain callback-local.
- Packages: `PicoGK` supplies implicit rasterization and intersection, lattice beams and nodes, morphology, metrics, ray-cast, mesh extraction, interpolated grayscale slices, vector slices, VDB read and write with field metadata, and `.cli` emission and import; `System.Numerics.Tensors` supplies finite checks, extrema, moments, energy, subtraction, and absolute transforms; `CommunityToolkit.HighPerformance` supplies pooled owners, `Span2D`, parallel partitioning, and pooled canonical writers; `UnitsNet`, `LanguageExt`, and `Thinktecture` own physical values, typed rails, and closed generated shapes.
- Growth: a topology is `FieldExpression` data, a common topology is a `FieldKind` seed row, a per-occurrence topology is `FieldDefinition.Generated`, a native transform is a `VoxelMorphologyStep` case, a layer encoding is a `CliMode` case, a spatial driver is an `ImplicitCell` field column, and a materializing consumer is one `Voxelize` callback.
- Boundary: PicoGK remains sidecar-native; operation policy never binds process-lifetime resolution; `PeriodicImplicit.fSignedDistance` copies the provider's by-reference callback value into `Distance`; `VoxelMorphologyStep.Apply`, `VoxelScope.Mesh`, `VoxelScope.Vdb`, `VoxelRuntime.Use`, `Consume`, `Raster`, `Combine`, `LatticeVoxels`, `Subtract`, `Measure`, `Vector`, `Grayscale`, VDB I/O, and canonical writers are provider or lifetime statement capsules; `FieldCalibration.Of` and `SampleAction.Invoke` are numeric kernels; `Grayscale` owns the statement loop because PicoGK requires one mutable `ref ImageGrayScale` buffer across slices; raw level equations never claim signed-distance semantics; a returned native handle is invalid egress; VDB source identity travels with its field name and required metadata; `eDetectWinding` computes the winding a content key digests because `eWinding` reports only the last detected value; canonical keys include every behavior-bearing policy value, emission setting, and field identity.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Buffers.Binary;
using System.IO.Hashing;
using System.Numerics;
using System.Numerics.Tensors;
using System.Text;
using System.Threading;
using CommunityToolkit.HighPerformance;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance.Helpers;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using PicoGK;
using Rasm.Fabrication.Process;
using Rasm.Numerics;
using Rhino.Geometry;
using Thinktecture;
using UnitsNet;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Additive;

// --- [TYPES] --------------------------------------------------------------------------------------------------------------------------------------
[ValueObject<string>]
public readonly partial struct FieldKey {
    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) =>
        validationError = string.IsNullOrWhiteSpace(value)
            ? new ValidationError("<implicit-field-key-empty>")
            : null;
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

    // FieldExpression differentiates itself: every case has a closed-form derivative, so level and gradient
    // emerge from one structural fold and no finite-difference step, stencil, or step-size knob exists.
    public FieldSample At(Vector3 phase) => Switch(
        state: phase,
        constant: static (_, expression) => new FieldSample(expression.Value, Vector3.Zero),
        wave: static (value, expression) =>
            Vector3.Dot(expression.Frequency, value) + expression.Phase switch {
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
        absolute: static expression => expression.Term is not null && expression.Term.Valid);
}

[SmartEnum<string>]
public sealed partial class FieldKind {
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
        new FieldExpression.Constant(0.15f)));
    public static readonly FieldKind Cellular = new("cellular", Minimum(
        Absolute(Multiply(SinX, SinY)),
        Absolute(Multiply(SinY, SinZ)),
        Absolute(Multiply(SinZ, SinX))));

    public FieldExpression Program { get; }

    private static FieldExpression Sine(Vector3 frequency) =>
        new FieldExpression.Wave(1.0f, frequency, -0.5f * MathF.PI);

    private static FieldExpression Cosine(Vector3 frequency) =>
        new FieldExpression.Wave(1.0f, frequency, 0.0f);

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
        FieldKind.TryGet(key, out FieldKind? kind) && kind is not null
            ? Fin.Succ<FieldDefinition>(new Known(kind))
            : Fin.Fail<FieldDefinition>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"implicit-field:unknown:{key}").ToError());

    public static Fin<FieldDefinition> Admit(FieldKey key, FieldExpression program) =>
        program is not null
        && program.Valid
        && !FieldKind.Items.Exists(item => string.Equals(item.Key, key.Value, StringComparison.Ordinal))
            ? Fin.Succ<FieldDefinition>(new Generated(key, program))
            : Fin.Fail<FieldDefinition>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit-field:generated-invalid").ToError());

    public FieldKey Identity => Switch(
        known: static definition => FieldKey.Create(definition.Kind.Key),
        generated: static definition => definition.Key);

    public FieldSample At(Vector3 phase) => Switch(
        state: phase,
        known: static (value, definition) => definition.Kind.Program.At(value),
        generated: static (value, definition) => definition.Program.At(value));

    public bool Valid => Switch(
        known: static definition => definition.Kind is not null && definition.Kind.Program.Valid,
        generated: static definition => !string.IsNullOrWhiteSpace(definition.Key.Value)
            && definition.Program is not null
            && definition.Program.Valid
            && !FieldKind.Items.Exists(item => string.Equals(
                item.Key, definition.Key.Value, StringComparison.Ordinal)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TpmsForm {
    private TpmsForm() { }

    public sealed record Solid : TpmsForm;
    public sealed record Sheet(Length MinimumWall, Length MaximumWall) : TpmsForm;

    // Sheet half-width is the calibrated quantile clamped into the printable wall band, never a bare floor:
    // an unbounded upper wall lets a graded density close the cell into solid stock.
    public float Distance(float residual, float gradientNorm, FieldThreshold threshold) => Switch(
        state: (residual, gradientNorm, threshold),
        solid: static (state, _) => state.residual / state.gradientNorm,
        sheet: static (state, form) => MathF.Abs(state.residual / state.gradientNorm)
            - (float)Math.Clamp(
                state.threshold.HalfWidth.Millimeters,
                0.5 * form.MinimumWall.Millimeters,
                0.5 * form.MaximumWall.Millimeters));

    public bool Valid => Switch(
        solid: static () => true,
        sheet: static form => double.IsFinite(form.MinimumWall.Millimeters)
            && double.IsFinite(form.MaximumWall.Millimeters)
            && form.MinimumWall.Millimeters > 0.0
            && form.MaximumWall.Millimeters >= form.MinimumWall.Millimeters);
}

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

    public bool Valid => Switch(
        offset: static value => double.IsFinite(value.Distance.Millimeters),
        shell: static value => double.IsFinite(value.Distance.Millimeters) && value.Distance.Millimeters != 0.0,
        overOffset: static value => double.IsFinite(value.First.Millimeters) && double.IsFinite(value.FinalSurface.Millimeters),
        smoothen: static value => double.IsFinite(value.Distance.Millimeters) && value.Distance.Millimeters > 0.0,
        fillet: static value => double.IsFinite(value.Radius.Millimeters) && value.Radius.Millimeters > 0.0,
        doubleOffset: static value => double.IsFinite(value.First.Millimeters) && double.IsFinite(value.Second.Millimeters),
        tripleOffset: static value => double.IsFinite(value.Distance.Millimeters),
        trim: static value => value.Bounds.IsValid,
        projectZ: static value => double.IsFinite(value.Start.Millimeters)
            && double.IsFinite(value.End.Millimeters)
            && value.Start.Millimeters < value.End.Millimeters);

    public Voxels Apply(Voxels voxels) => Switch(
        state: voxels,
        offset: static (held, step) => { held.Offset((float)step.Distance.Millimeters); return held; },
        shell: static (held, step) => {
            Voxels result = held.voxShell((float)step.Distance.Millimeters);
            held.Dispose();
            return result;
        },
        overOffset: static (held, step) => {
            held.OverOffset((float)step.First.Millimeters, (float)step.FinalSurface.Millimeters);
            return held;
        },
        smoothen: static (held, step) => { held.Smoothen((float)step.Distance.Millimeters); return held; },
        fillet: static (held, step) => { held.Fillet((float)step.Radius.Millimeters); return held; },
        doubleOffset: static (held, step) => {
            held.DoubleOffset((float)step.First.Millimeters, (float)step.Second.Millimeters);
            return held;
        },
        tripleOffset: static (held, step) => { held.TripleOffset((float)step.Distance.Millimeters); return held; },
        trim: static (held, step) => { held.Trim(FieldMath.Bounds(step.Bounds)); return held; },
        projectZ: static (held, step) => {
            held.ProjectZSlice((float)step.Start.Millimeters, (float)step.End.Millimeters);
            return held;
        });
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

[SmartEnum]
public sealed partial class MaskSampling {
    public static readonly MaskSampling VoxelGrid = new();
    public static readonly MaskSampling Interpolated = new();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CliMode {
    private CliMode() { }

    public sealed record Grayscale(ESliceMode Mode, MaskSampling Sampling, ESliceAxis Axis) : CliMode;
    public sealed record CliVector(
        CliIo.EFormat Format,
        double UnitsInMillimeters,
        bool AbsoluteOrigin,
        Option<FileInfo> Target = default) : CliMode;
    public sealed record VdbCli(FileInfo Target) : CliMode;
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class CalibrationPolicy {
    public int MinimumSamples { get; }
    public int MaximumSamples { get; }
    public Ratio QuantileError { get; }
    public Ratio DensityFloor { get; }
    public double GradientFloorPerMillimeter { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref int minimumSamples,
        ref int maximumSamples,
        ref Ratio quantileError,
        ref Ratio densityFloor,
        ref double gradientFloorPerMillimeter) =>
        validationError = minimumSamples < 8 || maximumSamples < minimumSamples
            || quantileError.DecimalFractions is <= 0.0 or >= 1.0
            || densityFloor.DecimalFractions is <= 0.0 or >= 0.5
            || !double.IsFinite(gradientFloorPerMillimeter)
            || gradientFloorPerMillimeter <= 0.0
                ? new ValidationError("<implicit-calibration-policy-invalid>")
                : null;
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

    [BoundaryAdapter]
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
        ref Option<Func<Fin<ScalarField>>> scaleField) =>
        validationError = Arr(periodX, periodY, periodZ).ForAll(static period => period.Millimeters > 0.0)
            && frameTolerance.DecimalFractions is > 0.0 and < 1.0
            && minimumScale.DecimalFractions is > 0.0 and <= 1.0
            && Math.Abs(worldToCell.GetDeterminant()) > frameTolerance.DecimalFractions
            && relativeDensity.DecimalFractions is > 0.0 and < 1.0
            && Arr(densityField, scaleField).ForAll(static field => field.ForAll(static factory => factory is not null))
            && axisField.ForAll(static factory => factory is not null)
                ? null
                : new ValidationError("<implicit-cell-invalid>");

    // Density grades wall thickness, axis grades orientation, and scale grades the period itself — the three
    // independent drivers a conformal lattice varies; folding scale into density collapses two of them.
    public Vector3 Phase(Vector3 world, Option<Vector3> axis, Ratio scale) =>
        Vector3.Transform(
            world,
            axis.Map(value => FieldMath.AxisFrame(value, (float)FrameTolerance.DecimalFractions))
                .IfNone(Matrix4x4.Identity) * WorldToCell) switch {
                var local => Vector3.Multiply(local, Wavenumber(scale)),
            };

    public Vector3 WorldGradient(Vector3 phaseGradient, Option<Vector3> axis, Ratio scale) =>
        Vector3.Multiply(phaseGradient, Wavenumber(scale)) switch {
            var scaled => Vector3.TransformNormal(
                scaled,
                Matrix4x4.Transpose(
                    axis.Map(value => FieldMath.AxisFrame(value, (float)FrameTolerance.DecimalFractions))
                        .IfNone(Matrix4x4.Identity) * WorldToCell)),
        };

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

[ComplexValueObject]
public sealed partial class VdbSource {
    public ContentKey Key { get; }
    public FileInfo Path { get; }
    public FieldKey Field { get; }
    public HashMap<string, string> RequiredMetadata { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ContentKey key,
        ref FileInfo path,
        ref FieldKey field,
        ref HashMap<string, string> requiredMetadata) =>
        validationError = path is null
            || requiredMetadata.ForAll(static _ => false)
            || !requiredMetadata.ForAll(static pair => !string.IsNullOrWhiteSpace(pair.Key) && pair.Value is not null)
                ? new ValidationError("<implicit-vdb-source-invalid>")
                : null;
}

[ComplexValueObject]
public sealed partial class ImplicitPolicy {
    public VoxelBudget Budget { get; }
    public Length LayerHeight { get; }
    public CliMode Cli { get; }
    public CalibrationPolicy Calibration { get; }
    public Func<Voxels, Fin<ContentKey>> Commit { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref VoxelBudget budget,
        ref Length layerHeight,
        ref CliMode cli,
        ref CalibrationPolicy calibration,
        ref Func<Voxels, Fin<ContentKey>> commit) =>
        validationError = budget is null
            || !budget.Bounds.IsValid
            || !double.IsFinite(budget.VoxelSizeMm)
            || budget.VoxelSizeMm <= 0.0
            || budget.VoxelCap <= 0
            || !double.IsFinite(layerHeight.Millimeters)
            || layerHeight.Millimeters <= 0.0
            || cli is null
            || calibration is null
            || commit is null
                ? new ValidationError("<implicit-policy-invalid>")
                : null;
}

[ComplexValueObject]
public sealed partial class VoxelWire {
    public ContentKey Key { get; }
    public Func<Fin<Voxels>> ToVoxels { get; }
    public Func<Voxels, Fin<ContentKey>> FromVoxels { get; }

    [BoundaryAdapter]
    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ContentKey key,
        ref Func<Fin<Voxels>> toVoxels,
        ref Func<Voxels, Fin<ContentKey>> fromVoxels) =>
        validationError = toVoxels is null || fromVoxels is null
            ? new ValidationError("<implicit-voxel-wire-invalid>")
            : null;
}

public sealed record VoxelMetrics(
    Volume Volume,
    BoundingBox Bounds,
    long NativeBytes,
    ContentKey Field,
    Option<CalibrationStats> Calibration);

// Header date, unit scale, and reader warnings are the import receipt; discarding them loses the only
// evidence that a round-tripped program degraded.
public sealed record CliImport(int Layers, BoundingBox Bounds, string HeaderDate, Seq<string> Warnings);

public sealed record CliStack(
    int Layers,
    ContentKey Key,
    Seq<ContentKey> Masks,
    Seq<ContentKey> Fields,
    Option<VoxelMetrics> Metrics,
    Option<CliImport> Import);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ImplicitOp {
    private ImplicitOp() { }

    public sealed record Field(
        FieldDefinition Definition,
        TpmsForm Form,
        ImplicitCell Cell,
        VoxelWire Envelope,
        Seq<VoxelMorphologyStep> Morphology,
        ImplicitPolicy Policy) : ImplicitOp;
    public sealed record LatticeSupport(
        SupportPlan Support,
        Option<VoxelWire> Part,
        Seq<VoxelMorphologyStep> Morphology,
        ImplicitPolicy Policy) : ImplicitOp;
    public sealed record Source(VoxelWire Wire, Seq<VoxelMorphologyStep> Morphology, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record Vdb(VdbSource Source, Seq<VoxelMorphologyStep> Morphology, ImplicitPolicy Policy) : ImplicitOp;
    public sealed record Composite(
        Seq<ImplicitOp> Inputs,
        VoxelBoolean Boolean,
        Seq<VoxelMorphologyStep> Morphology,
        ImplicitPolicy Policy) : ImplicitOp;

    public ImplicitPolicy Policy => Switch(
        field: static operation => operation.Policy,
        latticeSupport: static operation => operation.Policy,
        source: static operation => operation.Policy,
        vdb: static operation => operation.Policy,
        composite: static operation => operation.Policy);

    public Seq<VoxelMorphologyStep> Morphology => Switch(
        field: static operation => operation.Morphology,
        latticeSupport: static operation => operation.Morphology,
        source: static operation => operation.Morphology,
        vdb: static operation => operation.Morphology,
        composite: static operation => operation.Morphology);

    public FaultSubject.VoxelOperation Subject => Switch(
        field: static _ => new FaultSubject.VoxelOperation(nameof(Field)),
        latticeSupport: static _ => new FaultSubject.VoxelOperation(nameof(LatticeSupport)),
        source: static _ => new FaultSubject.VoxelOperation(nameof(Source)),
        vdb: static _ => new FaultSubject.VoxelOperation(nameof(Vdb)),
        composite: static _ => new FaultSubject.VoxelOperation(nameof(Composite)));

    public Fin<ContentKey> Commit(Voxels voxels) => Switch(
        state: voxels,
        field: static (held, operation) => operation.Envelope.FromVoxels(held),
        latticeSupport: static (held, operation) => operation.Part
            .Map(wire => wire.FromVoxels(held))
            .IfNone(operation.Policy.Commit(held)),
        source: static (held, operation) => operation.Wire.FromVoxels(held),
        vdb: static (held, operation) => operation.Policy.Commit(held),
        composite: static (held, operation) => operation.Policy.Commit(held));
}

internal readonly record struct Rasterized(Voxels Voxels, Option<CalibrationStats> Calibration);

public sealed class VoxelScope {
    internal VoxelScope(Voxels native, VoxelMetrics metrics) => (Native, Metrics) = (native, metrics);

    internal Voxels Native { get; }
    public VoxelMetrics Metrics { get; }

    public Fin<ContentKey> Mesh(Func<PicoGK.Mesh, Fin<ContentKey>> project) {
        if (project is null) {
            return Fin.Fail<ContentKey>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit-mesh:projection-missing").ToError());
        }
        using PicoGK.Mesh mesh = Native.mshAsMesh();
        return project(mesh);
    }

    public Fin<Option<Point3d>> Raycast(Point3d origin, Vector3d direction) =>
        !origin.IsValid || !direction.IsValid || direction.IsZero
            ? Fin.Fail<Option<Point3d>>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit-ray:invalid").ToError())
            : Fin.Succ(Native.bRayCastToSurface(
                FieldMath.Vector(origin),
                new Vector3((float)direction.X, (float)direction.Y, (float)direction.Z),
                out Vector3 hit)
                    ? Some(new Point3d(hit.X, hit.Y, hit.Z))
                    : None);

    // Export closes the VDB round trip the import lane opens; provenance travels as field metadata so a
    // re-imported field carries the identity its required-metadata gate compares.
    public Fin<ContentKey> Vdb(FileInfo target, FieldKey field, HashMap<string, string> provenance) =>
        target?.Directory is not { Exists: true } || string.IsNullOrWhiteSpace(field.Value)
            ? Fin.Fail<ContentKey>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit-vdb:export-target").ToError())
            : Try.lift(() => {
                    using OpenVdbFile file = new();
                    _ = file.nAdd(Native, field.Value);
                    using FieldMetadata metadata = Native.oMetaData();
                    provenance.Iter(pair => metadata.SetValue(pair.Key, pair.Value));
                    file.SaveToFile(target.FullName);
                    return Metrics.Field;
                })
                .Run()
                .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"implicit-vdb:export:{error.Message}").ToError());
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
                .Run()
                .MapFail(_ => FabricationFault.VoxelFault(operation.Subject, operation.Policy.Budget).ToError())
                .Bind(static result => result);
        }
    }
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
public static class Implicit {
    public static Fin<T> Voxelize<T>(Seq<ImplicitOp> operations, Func<Arr<VoxelScope>, Fin<T>> consume) =>
        operations.IsEmpty
            ? Fin.Fail<T>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit:empty-operation-set").ToError())
        : consume is null
            ? Fin.Fail<T>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit:consumer-missing").ToError())
            : from _ in operations.Traverse(operation => Admit(operation).ToValidation()).As().ToFin()
              from __ in Gate(Compatible(operations), "implicit:incompatible-operation-set").ToFin()
              from result in VoxelRuntime.Use(operations, () =>
                  from rasters in Build(operations)
                  from consumed in Consume(operations, rasters, consume)
                  select consumed)
              select result;

    public static Fin<CliStack> Cli(ImplicitOp operation) =>
        from _ in Admit(operation)
        from stack in operation.Policy.Cli.Switch(
            state: operation,
            grayscale: static (op, mode) => Voxelize(Seq(op), scopes => Grayscale(scopes[0], op, mode)),
            cliVector: static (op, mode) => Voxelize(Seq(op), scopes => Vector(scopes[0], op, mode)),
            vdbCli: static (op, mode) => Direct(op, mode))
        select stack;

    private static Fin<T> Consume<T>(
        Seq<ImplicitOp> operations,
        Seq<Rasterized> rasters,
        Func<Arr<VoxelScope>, Fin<T>> consume) {
        try {
            return from scopes in operations.Zip(rasters)
                       .Map(row =>
                           from _ in WithinBudget(row.Item1)
                           from __ in Occupied(row.Item2.Voxels, row.Item1)
                           from field in row.Item1.Commit(row.Item2.Voxels)
                           select new VoxelScope(row.Item2.Voxels, Measure(row.Item2, field)))
                       .Sequence()
                   from result in consume(scopes.ToArr())
                   select result;
        }
        finally {
            rasters.Iter(static raster => raster.Voxels.Dispose());
        }
    }

    // An empty field rasterizes without fault and posts an empty program; the gate turns that into evidence.
    private static Fin<Unit> Occupied(Voxels voxels, ImplicitOp operation) =>
        voxels.bIsEmpty()
            ? Fail<Unit>(operation)
            : Fin.Succ(unit);

    private static Fin<Seq<Rasterized>> Build(Seq<ImplicitOp> operations) =>
        operations.Fold(
            Fin.Succ(Seq<Rasterized>()),
            static (rail, operation) =>
                from held in rail
                from next in Build(operation).Rollback(held.Map(static row => (IDisposable?)row.Voxels).ToArray())
                select held.Add(next));

    private static Fin<Rasterized> Build(ImplicitOp operation) => operation.Switch(
        field: Field,
        latticeSupport: static operation => Lattice(operation.Support, operation.Part, operation.Morphology)
            .Map(static voxels => new Rasterized(voxels, None)),
        source: static operation => operation.Wire.ToVoxels()
            .Bind(voxels => Morph(voxels, operation.Morphology))
            .Map(static voxels => new Rasterized(voxels, None)),
        vdb: static operation => Vdb(operation.Source, operation.Policy.Budget.VoxelSizeMm)
            .Bind(voxels => Morph(voxels, operation.Morphology))
            .Map(static voxels => new Rasterized(voxels, None)),
        composite: static operation =>
            from inputs in Build(operation.Inputs)
            from combined in Combine(inputs.Map(static row => row.Voxels), operation.Boolean)
            from morphed in Morph(combined, operation.Morphology)
            select new Rasterized(morphed, inputs.Map(static row => row.Calibration).Somes().HeadOrNone()));

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
            .Run()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"implicit-composite:{error.Message}").ToError());

    private static Fin<Rasterized> Field(ImplicitOp.Field operation) =>
        from density in Acquire(operation.Cell.DensityField)
        from scale in Acquire(operation.Cell.ScaleField)
            .Rollback(density.Match(
                None: static () => Array.Empty<IDisposable?>(),
                Some: static field => [(IDisposable?)field]))
        from axis in Acquire(operation.Cell.AxisField)
            .Rollback(Seq(density.Map(static field => (IDisposable?)field), scale.Map(static field => (IDisposable?)field))
                .Somes()
                .ToArray())
        from raster in Raster(operation, density, scale, axis)
        select raster;

    // `voxIntersectImplicit` rasterizes the field only where the envelope already has occupancy; a full-bounds
    // `new Voxels(field, bounds)` allocates the whole budget box before discarding almost all of it.
    private static Fin<Rasterized> Raster(
        ImplicitOp.Field operation,
        Option<ScalarField> density,
        Option<ScalarField> scale,
        Option<VectorField> axis) {
        Voxels? envelope = null;
        FieldCalibration? calibration = null;
        try {
            return from held in operation.Envelope.ToVoxels()
                   let captured = envelope = held
                   from admittedCalibration in FieldCalibration.Of(operation.Definition, operation.Cell, operation.Policy.Calibration)
                   let retained = calibration = admittedCalibration
                   let implicitField = new PeriodicImplicit(
                       operation.Definition,
                       operation.Form,
                       operation.Cell,
                       FieldMath.Bounds(operation.Policy.Budget.Bounds),
                       density,
                       scale,
                       axis,
                       retained)
                   from morphed in Morph(captured.voxIntersectImplicit(implicitField), operation.Morphology)
                   select new Rasterized(morphed, Some(retained.Stats));
        }
        finally {
            envelope?.Dispose();
            calibration?.Dispose();
            density.Iter(static field => field.Dispose());
            scale.Iter(static field => field.Dispose());
            axis.Iter(static field => field.Dispose());
        }
    }

    private static Fin<Voxels> Lattice(
        SupportPlan support,
        Option<VoxelWire> part,
        Seq<VoxelMorphologyStep> morphology) =>
        from edges in TreeEdges(support.TreeNodes)
        let scaffold = LatticeVoxels(support.TreeNodes, edges)
        from result in part.Map(wire => Subtract(scaffold, wire)).IfNone(Fin.Succ(scaffold))
        from morphed in Morph(result, morphology)
        select morphed;

    private static Fin<Seq<(TreeNode Node, TreeNode Parent)>> TreeEdges(Seq<TreeNode> nodes) =>
        toHashMap(nodes.Map(static node => (node.Id, node))) switch {
            var byId => nodes
                .Bind(node => node.Parents.Map(parent => (Node: node, Parent: parent)))
                .Map(edge => byId.Find(edge.Parent)
                    .ToFin(new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"implicit-lattice:missing-parent:{edge.Parent}").ToError())
                    .Map(parent => (edge.Node, parent)))
                .Sequence(),
        };

    private static Voxels LatticeVoxels(Seq<TreeNode> nodes, Seq<(TreeNode Node, TreeNode Parent)> edges) {
        using PicoGK.Lattice lattice = new();
        nodes.Iter(node => lattice.AddSphere(
            FieldMath.Vector(node.At),
            (float)node.PhysicalRadius.Millimeters));
        edges.Iter(edge => lattice.AddBeam(
            FieldMath.Vector(edge.Parent.At),
            (float)edge.Parent.PhysicalRadius.Millimeters,
            FieldMath.Vector(edge.Node.At),
            (float)edge.Node.PhysicalRadius.Millimeters,
            bRoundCap: true));
        return new Voxels(lattice);
    }

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
            .Run()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"implicit-vdb:{error.Message}").ToError())
        select voxels;

    private static Fin<Unit> VdbMetadata(VdbSource source, double voxelSizeMm) =>
        VdbIdentity(source).Bind(_ => Try.lift<Fin<Unit>>(() => {
                using OpenVdbFile file = new(source.Path.FullName);
                if (!file.bIsPicoGKCompatible()
                    || !file.fPicoGKVoxelSizeMM().Equals((float)voxelSizeMm)) {
                    return Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit-vdb:voxel-size").ToError());
                }
                using Voxels field = file.voxGet(source.Field.Value);
                using FieldMetadata metadata = field.oMetaData();
                return source.RequiredMetadata.ForAll(pair =>
                    metadata.bGetValueAt(pair.Key, out string actual)
                    && string.Equals(actual, pair.Value, StringComparison.Ordinal))
                        ? Fin.Succ(unit)
                        : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit-vdb:metadata").ToError());
            })
            .Run()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"implicit-vdb:{error.Message}").ToError())
            .Bind(static result => result));

    private static Fin<Unit> VdbIdentity(VdbSource source) =>
        Try.lift(() => {
                using FileStream payload = source.Path.OpenRead();
                long canonicalLength = sizeof(int) + Encoding.UTF8.GetByteCount(source.Field.Value) + payload.Length;
                if (canonicalLength > int.MaxValue)
                    return false;

                using XxHash128 hash = new();
                byte[] kind = Encoding.UTF8.GetBytes(source.Key.Kind.Key);
                byte[] field = Encoding.UTF8.GetBytes(source.Field.Value);
                AppendInt32(hash, kind.Length);
                hash.Append(kind);
                AppendInt32(hash, (int)canonicalLength);
                AppendInt32(hash, field.Length);
                hash.Append(field);
                hash.Append(payload);
                return hash.GetCurrentHashAsUInt128() == source.Key.Digest;
            })
            .Run()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"implicit-vdb:identity:{error.Message}").ToError())
            .Bind(matches => matches
                ? Fin.Succ(unit)
                : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit-vdb:identity").ToError()));

    private static void AppendInt32(XxHash128 hash, int value) {
        Span<byte> width = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(width, value);
        hash.Append(width);
    }

    private static Fin<Voxels> Morph(Voxels voxels, Seq<VoxelMorphologyStep> steps) =>
        steps.Fold(
            Fin.Succ(voxels),
            static (rail, step) =>
                from held in rail
                from next in Try.lift(() => step.Apply(held))
                    .Run()
                    .Rollback(held)
                select next);

    private static Fin<Option<T>> Acquire<T>(Option<Func<Fin<T>>> source) where T : class, IDisposable =>
        Try.lift<Fin<Option<T>>>(() => source.Match(
                None: static () => Fin.Succ(Option<T>.None),
                Some: static factory => factory().Map(Some)))
            .Run()
            .MapFail(static error => new GeometryFault.DegenerateInput(Kind.Mesh, -1, $"implicit-driver:{error.Message}").ToError())
            .Bind(static result => result);

    private static Fin<Unit> WithinBudget(ImplicitOp operation) =>
        operation.Policy.Budget.RequiredCells <= operation.Policy.Budget.VoxelCap
            ? Fin.Succ(unit)
            : Fail<Unit>(operation);

    private static VoxelMetrics Measure(Rasterized raster, ContentKey field) {
        raster.Voxels.CalculateProperties(out float cubicMillimeters, out BBox3 propertyBounds);
        BBox3 queried = raster.Voxels.oCalculateBoundingBox();
        BoundingBox bounds = new(
            new Point3d(queried.vecMin.X, queried.vecMin.Y, queried.vecMin.Z),
            new Point3d(queried.vecMax.X, queried.vecMax.Y, queried.vecMax.Z));
        return new VoxelMetrics(
            Volume.FromCubicMillimeters(cubicMillimeters),
            propertyBounds.bIsEmpty() ? bounds : new BoundingBox(
                new Point3d(propertyBounds.vecMin.X, propertyBounds.vecMin.Y, propertyBounds.vecMin.Z),
                new Point3d(propertyBounds.vecMax.X, propertyBounds.vecMax.Y, propertyBounds.vecMax.Z)),
            raster.Voxels.nMemUsage(),
            field,
            raster.Calibration);
    }

    private static Fin<CliStack> Vector(VoxelScope scope, ImplicitOp operation, CliMode.CliVector mode) =>
        Try.lift(() => {
                PolySliceStack slices = scope.Native.oVectorize(
                    (float)operation.Policy.LayerHeight.Millimeters,
                    mode.AbsoluteOrigin);
                mode.Target.Iter(target => CliIo.WriteSlicesToCliFile(
                    slices,
                    target.FullName,
                    mode.Format,
                    strDate: null,
                    mode.UnitsInMillimeters));
                ContentKey key = ContentKey.Of(
                    EgressKind.Cli,
                    ImplicitCanonical.Cli(slices, Seq(scope.Metrics.Field), operation.Policy, Some(mode)));
                return new CliStack(
                    slices.nCount(), key, Seq<ContentKey>(), Seq(scope.Metrics.Field), Some(scope.Metrics), None);
            })
            .Run()
            .MapFail(_ => FabricationFault.VoxelFault(operation.Subject, operation.Policy.Budget).ToError());

    private static Fin<CliStack> Grayscale(VoxelScope scope, ImplicitOp operation, CliMode.Grayscale mode) =>
        Try.lift(() => {
                ImageGrayScale image = scope.Native.imgAllocateSlice(out int voxelSlices, mode.Axis);
                Seq<Length> elevations = mode.Sampling == MaskSampling.VoxelGrid
                    ? toSeq(Enumerable.Range(0, voxelSlices)).Map(index => Length.FromMillimeters(
                        operation.Policy.Budget.Bounds.Min.Z
                        + (index + 0.5) * operation.Policy.Budget.VoxelSizeMm))
                    : Elevations(operation.Policy.Budget.Bounds, operation.Policy.LayerHeight);
                Seq<ContentKey> masks = Seq<ContentKey>();
                for (int index = 0; index < elevations.Count; index++) {
                    if (mode.Sampling == MaskSampling.VoxelGrid) {
                        scope.Native.GetVoxelSlice(index, ref image, mode.Mode, mode.Axis);
                    }
                    else {
                        scope.Native.GetInterpolatedVoxelSlice(
                            (float)elevations[index].Millimeters,
                            ref image,
                            mode.Mode);
                    }
                    masks = masks.Add(ContentKey.Of(
                        EgressKind.Cli,
                        ImplicitCanonical.Image(index, elevations[index], image, scope.Metrics.Field)));
                }
                ContentKey key = ContentKey.Of(
                    EgressKind.Cli,
                    ImplicitCanonical.MaskIndex(masks, scope.Metrics.Field, operation.Policy, mode));
                return new CliStack(masks.Count, key, masks, Seq(scope.Metrics.Field), Some(scope.Metrics), None);
            })
            .Run()
            .MapFail(_ => FabricationFault.VoxelFault(operation.Subject, operation.Policy.Budget).ToError());

    private static Fin<CliStack> Direct(ImplicitOp operation, CliMode.VdbCli mode) =>
        operation is not ImplicitOp.Vdb vdb
        || !operation.Morphology.IsEmpty
        || !vdb.Source.Path.Exists
        || mode.Target.Directory is not { Exists: true }
        || vdb.Policy.LayerHeight.Millimeters <= 0.0
            ? Fail<CliStack>(operation)
            : VoxelRuntime.Use(Seq(operation), () =>
                from _ in VdbMetadata(vdb.Source, vdb.Policy.Budget.VoxelSizeMm)
                from stack in Try.lift(() => {
                    Vdb2Cli.Convert(
                        vdb.Source.Path.FullName,
                        (float)vdb.Policy.LayerHeight.Millimeters,
                        mode.Target.FullName,
                        vdb.Source.Field.Value);
                    CliIo.Result imported = CliIo.oSlicesFromCliFile(mode.Target.FullName);
                    ContentKey key = ContentKey.Of(
                        EgressKind.Cli,
                        ImplicitCanonical.Cli(
                            imported.oSlices, Seq(vdb.Source.Key), vdb.Policy, Option<CliMode.CliVector>.None));
                    return new CliStack(
                        imported.oSlices.nCount(),
                        key,
                        Seq<ContentKey>(),
                        Seq(vdb.Source.Key),
                        None,
                        Some(new CliImport(
                            imported.nLayers,
                            new BoundingBox(
                                new Point3d(imported.oBBoxFile.vecMin.X, imported.oBBoxFile.vecMin.Y, imported.oBBoxFile.vecMin.Z),
                                new Point3d(imported.oBBoxFile.vecMax.X, imported.oBBoxFile.vecMax.Y, imported.oBBoxFile.vecMax.Z)),
                            imported.strHeaderDate,
                            string.IsNullOrWhiteSpace(imported.strWarnings)
                                ? Seq<string>()
                                : Seq(imported.strWarnings))));
                })
                .Run()
                .MapFail(_ => FabricationFault.VoxelFault(operation.Subject, operation.Policy.Budget).ToError())
                select stack);

    private static Seq<Length> Elevations(BoundingBox bounds, Length layerHeight) =>
        toSeq(Enumerable.Range(
                0,
                Math.Max(1, (int)Math.Ceiling((bounds.Max.Z - bounds.Min.Z) / layerHeight.Millimeters))))
            .Map(index => Length.FromMillimeters(bounds.Min.Z + (index + 0.5) * layerHeight.Millimeters));

    private static Fin<Unit> Admit(ImplicitOp? operation) =>
        operation is null
            ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit:operation-null").ToError())
            : operation.Policy is null
                ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit:policy").ToError())
                : Gates(
                    Gate(operation.Policy.Cli.Switch(
                        grayscale: static mode => mode.Sampling is not null,
                        cliVector: static mode => mode.UnitsInMillimeters > 0.0
                            && mode.Target.ForAll(static target => target is not null),
                        vdbCli: static mode => mode.Target is not null), "implicit:cli-mode"),
                    Gate(operation.Morphology.ForAll(static step => step.Valid), "implicit:morphology"),
                    operation.Switch(
                        field: static field => Gates(
                            Gate(field.Definition is not null && field.Definition.Valid, "implicit:field-definition"),
                            Gate(field.Cell is not null, "implicit:field-cell"),
                            Gate(field.Envelope is not null, "implicit:field-envelope"),
                            Gate(field.Form is not null && field.Form.Valid, "implicit:field-form")),
                        latticeSupport: static lattice => Gates(
                            Gate(lattice.Support is not null && !lattice.Support.TreeNodes.IsEmpty, "implicit:lattice-support"),
                            Gate(lattice.Part.ForAll(static wire => wire is not null), "implicit:lattice-part")),
                        source: static source => Gate(source.Wire is not null, "implicit:source-wire"),
                        vdb: static vdb => Gates(
                            Gate(vdb.Source is not null, "implicit:vdb-source"),
                            Gate(vdb.Source is not null && vdb.Source.Path.Exists, "implicit:vdb-path")),
                        composite: static composite => Gates(
                            Gate(!composite.Inputs.IsEmpty, "implicit:composite-inputs"),
                            Gate(composite.Boolean is not null, "implicit:composite-boolean"),
                            composite.Inputs.Traverse(input => Admit(input).ToValidation()).Map(static _ => unit))))
                    .ToFin()
                    .Map(static _ => unit);

    private static Validation<Error, Unit> Gates(params ReadOnlySpan<Validation<Error, Unit>> gates) =>
        toSeq(gates.ToArray()).Traverse(static gate => gate).Map(static _ => unit);

    private static Validation<Error, Unit> Gate(bool valid, string locus) =>
        (valid
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, locus).ToError()))
        .ToValidation();

    private static bool Compatible(Seq<ImplicitOp> operations) =>
        operations.Bind(Expand).Map(static operation => operation.Policy.Budget.VoxelSizeMm).Distinct().Count == 1;

    private static Seq<ImplicitOp> Expand(ImplicitOp operation) =>
        Seq(operation).Concat(operation.Switch(
            field: static _ => Seq<ImplicitOp>(),
            latticeSupport: static _ => Seq<ImplicitOp>(),
            source: static _ => Seq<ImplicitOp>(),
            vdb: static _ => Seq<ImplicitOp>(),
            composite: static composite => composite.Inputs.Bind(Expand)));

    private static Fin<T> Fail<T>(ImplicitOp operation) =>
        Fin.Fail<T>(FabricationFault.VoxelFault(operation.Subject, operation.Policy.Budget).ToError());
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
    private readonly MemoryOwner<float> _levels;
    private readonly MemoryOwner<float> _distances;
    private readonly CalibrationStats _stats;
    private readonly CalibrationPolicy _policy;

    private FieldCalibration(
        MemoryOwner<float> levels,
        MemoryOwner<float> distances,
        CalibrationStats stats,
        CalibrationPolicy policy) =>
        (_levels, _distances, _stats, _policy) = (levels, distances, stats, policy);

    public float GradientFloorPerMillimeter => (float)_policy.GradientFloorPerMillimeter;
    public Ratio DensityFloor => _policy.DensityFloor;
    public CalibrationStats Stats => _stats;

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
            Span2D<float> planes = levelSpan.AsSpan2D(resolution, resolution * resolution);
            if (planes.Height != resolution
                || !TensorPrimitives.IsFiniteAll(levelSpan)
                || !TensorPrimitives.IsFiniteAll(distanceSpan)) {
                return Fin.Fail<FieldCalibration>(new GeometryFault.DegenerateInput(Kind.Mesh, -1, "implicit-calibration:non-finite").ToError());
            }

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
        Math.Clamp(
            (int)Math.Round(density.DecimalFractions * (_levels.Span.Length - 1)),
            0,
            _levels.Span.Length - 1) switch {
                var index => form.Switch(
                    state: (Calibration: this, Index: index),
                    solid: static (state, _) => new FieldThreshold(
                        state.Calibration._levels.Span[state.Index],
                        Length.Zero,
                        state.Calibration._stats),
                    sheet: static (state, _) => new FieldThreshold(
                        0.0f,
                        Length.FromMillimeters(state.Calibration._distances.Span[state.Index]),
                        state.Calibration._stats)),
            };

    public void Dispose() {
        _levels.Dispose();
        _distances.Dispose();
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
            FieldSample sample = definition.At(phase);
            float gradient = MathF.Max(
                cell.WorldGradient(sample.Gradient, None, Ratio.FromDecimalFractions(1.0)).Length(),
                gradientFloorPerMillimeter);
            levels.Span[index] = sample.Level;
            distances.Span[index] = MathF.Abs(sample.Level / gradient);
        }
    }
}

// --- [CANONICAL_BYTES] ----------------------------------------------------------------------------------------------------------------------------
public static class ImplicitCanonical {
    // `eWinding` reports the last detected value, so an undetected contour would fork the key on read order;
    // `eDetectWinding` computes it. Emission policy joins the preimage because it shifts the produced bytes.
    public static byte[] Cli(
        PolySliceStack slices,
        Seq<ContentKey> fields,
        ImplicitPolicy policy,
        Option<CliMode.CliVector> mode) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Float64(writer, policy.LayerHeight.Millimeters);
        mode.Match(
            None: () => { Int32(writer, 0); },
            Some: value => {
                Int32(writer, 1);
                Int32(writer, (int)value.Format);
                Float64(writer, value.UnitsInMillimeters);
                Int32(writer, value.AbsoluteOrigin ? 1 : 0);
            });
        Keys(writer, fields);
        Int32(writer, slices.nCount());
        toSeq(Enumerable.Range(0, slices.nCount())).Iter(layer => {
            PolySlice slice = slices.oSliceAt(layer);
            Float64(writer, slice.fZPos());
            Int32(writer, slice.nContours());
            toSeq(Enumerable.Range(0, slice.nContours())).Iter(contourIndex => {
                PolyContour contour = slice.oContourAt(contourIndex);
                Int32(writer, contour.nCount());
                Int32(writer, (int)contour.eDetectWinding());
                toSeq(Enumerable.Range(0, contour.nCount())).Iter(vertexIndex => {
                    Vector2 vertex = contour.vecVertex(vertexIndex);
                    Float64(writer, vertex.X);
                    Float64(writer, vertex.Y);
                });
            });
        });
        return writer.WrittenSpan.ToArray();
    }

    public static byte[] Image(int layer, Length elevation, ImageGrayScale image, ContentKey field) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Int32(writer, layer);
        Float64(writer, elevation.Millimeters);
        UInt128Value(writer, field.Digest);
        Int32(writer, image.nWidth);
        Int32(writer, image.nHeight);
        image.m_afValues.Iter(value => Float32(writer, value));
        return writer.WrittenSpan.ToArray();
    }

    public static byte[] MaskIndex(
        Seq<ContentKey> masks,
        ContentKey field,
        ImplicitPolicy policy,
        CliMode.Grayscale mode) {
        using ArrayPoolBufferWriter<byte> writer = new();
        Float64(writer, policy.LayerHeight.Millimeters);
        Int32(writer, (int)mode.Mode);
        Int32(writer, (int)mode.Axis);
        Int32(writer, mode.Sampling == MaskSampling.VoxelGrid ? 0 : 1);
        UInt128Value(writer, field.Digest);
        Keys(writer, masks);
        return writer.WrittenSpan.ToArray();
    }

    private static void Keys(ArrayPoolBufferWriter<byte> writer, Seq<ContentKey> keys) {
        Int32(writer, keys.Count);
        keys.Iter(key => UInt128Value(writer, key.Digest));
    }

    private static void Int32(ArrayPoolBufferWriter<byte> writer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(sizeof(int)), value);
        writer.Advance(sizeof(int));
    }

    private static void Float64(ArrayPoolBufferWriter<byte> writer, double value) {
        BinaryPrimitives.WriteDoubleLittleEndian(writer.GetSpan(sizeof(double)), value);
        writer.Advance(sizeof(double));
    }

    private static void Float32(ArrayPoolBufferWriter<byte> writer, float value) {
        BinaryPrimitives.WriteSingleLittleEndian(writer.GetSpan(sizeof(float)), value);
        writer.Advance(sizeof(float));
    }

    private static void UInt128Value(ArrayPoolBufferWriter<byte> writer, UInt128 value) {
        BinaryPrimitives.WriteUInt128LittleEndian(writer.GetSpan(16), value);
        writer.Advance(16);
    }
}
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

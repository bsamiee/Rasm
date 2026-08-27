# 1. Move policy admission and stop semantics onto existing owners

## From

`libs/dotnet/Rasm/.planning/Processing/register.md:48-71`

```csharp
[SmartEnum<int>]
public sealed partial class AlignmentStopKind {
    public static readonly AlignmentStopKind Converged = new(key: 0);
    public static readonly AlignmentStopKind MaxIterationsExhausted = new(key: 1);
    public static readonly AlignmentStopKind OptimizerStopped = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class AlignmentOptimizerStopKind {
    public static readonly AlignmentOptimizerStopKind StepAccepted = new(key: 0);
    public static readonly AlignmentOptimizerStopKind StepBelowTolerance = new(key: 1);
    public static readonly AlignmentOptimizerStopKind BudgetExhausted = new(key: 2);
    public static readonly AlignmentOptimizerStopKind ModelRefused = new(key: 3);
}

internal readonly record struct AlignBands(double Neglect, double Convergence, double Residual, double Step, double Orientation, double Real, double Ridge) {
    internal static AlignBands Of(Context context, AlignmentPolicy policy) => new(
        Neglect: context.For(ToleranceLane.Neglect).Value,
        Convergence: context.For(policy.Convergence).Value,
        Residual: context.For(policy.Residual).Value,
        Step: context.For(policy.Step).Value,
        Orientation: context.For(ToleranceLane.Orientation).Value,
        Real: context.For(ToleranceLane.Real).Value,
        Ridge: context.For(policy.Ridge).Value);
}
```

`libs/dotnet/Rasm/.planning/Processing/register.md:114-139`

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct AlignmentPolicy(
    Dimension MaxIterations, ToleranceLane Convergence, ToleranceLane Residual,
    ToleranceLane Step, ToleranceLane Ridge, UnitInterval RobustScale, Dimension OptimizerBudget,
    PoseFit Fit, Option<UnitInterval> TrimFraction, Dimension CoarseLevels) {
    public static readonly AlignmentPolicy Default = new(
        MaxIterations: Dimension.Create(value: 30),
        Convergence: ToleranceLane.Convergence, Residual: ToleranceLane.Residual,
        Step: ToleranceLane.Step, Ridge: ToleranceLane.Kkt,
        RobustScale: UnitInterval.Create(value: 0.1), OptimizerBudget: Dimension.Create(value: 8),
        Fit: PoseFit.Rigid, TrimFraction: Option<UnitInterval>.None, CoarseLevels: Dimension.Create(value: 1));
    internal Fin<AlignmentPolicy> Admit(Op key) {
        AlignmentPolicy self = this;
        return guard(ValidityClaim.All(
                ValidityClaim.CountAtLeast(count: self.MaxIterations.Value, floor: 1),
                ValidityClaim.Positive(value: self.RobustScale.Value),
                ValidityClaim.CountAtLeast(count: self.OptimizerBudget.Value, floor: 1),
                self.TrimFraction.Map(static f => f.Value < 1.0).IfNone(noneValue: true),
                ValidityClaim.CountAtLeast(count: self.CoarseLevels.Value, floor: 1)), key.InvalidInput())
            .ToFin().Map(_ => self);
    }
    internal SolvePolicy Ladder(AlignBands bands) => SolvePolicy.Canonical with {
        ResidualTolerance = PositiveMagnitude.Create(value: bands.Residual),
        StepFloor = bands.Step, MaxIterations = OptimizerBudget,
    };
}
```

`libs/dotnet/Rasm/.planning/Processing/register.md:151-180`

```csharp
public readonly record struct GicpSolve(
    AlignmentOptimizerStopKind Stop, int Iterations, double InitialCost, double FinalCost, double StepNorm,

internal readonly record struct AlignmentStep(
    Transform Delta, Option<LinearSolution> Solve = default, Option<RobustWeights> Robust = default,
    Option<GicpSolve> Optimizer = default, Option<AlignmentStopKind> Stop = default,
    Option<double> Scale = default);

public readonly record struct Alignment(
    Transform Transform, AlignKind Kind, AlignmentStopKind Stop, int Iterations, Option<double> FinalDelta,
```

`libs/dotnet/Rasm/.planning/Processing/register.md:194-220`

```csharp
ProjectionRow.Of<Transform>(() => self.Stop.Equals(AlignmentStopKind.Converged)
    ? key.AcceptValue(value: self.Transform)
    : Fin.Fail<Transform>(key.InvalidResult())));

private readonly record struct IcpState(Transform Current, Option<double> FinalDelta, int Iterations, AlignmentStep Step, Option<AlignmentStopKind> Stop);

internal static Fin<Alignment> AlignClouds(AlignKind kind, VectorCloud source, VectorCloud target, AlignmentPolicy policy, Op key) =>
    from activePolicy in policy.Admit(key: key)
    from alignment in (source, target) switch {
        (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt) => IcpAlign(source: src, target: tgt, kind: kind, policy: activePolicy, key: key),
        _ => Fin.Fail<Alignment>(error: key.InvalidInput()),
    }
    select alignment;

private static Fin<Alignment> IcpAlign(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, AlignKind kind, AlignmentPolicy policy, Op key) =>
    from bands in Fin.Succ(AlignBands.Of(context: source.Tolerance, policy: policy))
    from neighborhoodPolicy in NeighborhoodPolicy.Of(context: source.Tolerance, key: key)
```

`libs/dotnet/Rasm/.planning/Processing/register.md:229-265`

```csharp
initialState: Fin.Succ(new IcpState(Current: Transform.Identity, FinalDelta: Option<double>.None, Iterations: 0, Step: new AlignmentStep(Delta: Transform.Identity), Stop: Option<AlignmentStopKind>.None)),

select new Alignment(Transform: final.Current, Kind: kind, Stop: final.Stop.IfNone(AlignmentStopKind.MaxIterationsExhausted), Iterations: final.Iterations, FinalDelta: final.FinalDelta,

Atom<Fin<IcpState>> cell = Atom(value: Fin.Succ(carried with { Stop = Option<AlignmentStopKind>.None }));

select new IcpState(Current: current, FinalDelta: Some(finalDelta), Iterations: state.Iterations + 1, Step: step,
    Stop: step.Stop.IsSome ? step.Stop : finalDelta < bands.Convergence ? Some(AlignmentStopKind.Converged) : Option<AlignmentStopKind>.None);
```

`libs/dotnet/Rasm/.planning/Processing/register.md:394-430`

```csharp
internal static Fin<AlignmentStep> SolveGeneralizedIcp(Seq<Point3d> source, AlignmentMatch match, Transform current, AlignmentPolicy policy, AlignBands bands, Op key) =>
    from sourcePca in match.SourcePca.ToFin(key.InvalidInput())
    from targetPca in match.TargetPca.ToFin(key.InvalidInput())
    from rows in AdmitAlignmentRows(source: source, target: match.Targets, weights: match.RowMass, minimum: ProcrustesFloor, key: key)
    from sourceRowCount in Admit.SameCount(expected: rows, key: key, counts: [match.SourceRows.Length])
    from sourceRows in guard(match.SourceRows.All(row => row >= 0 && row < sourcePca.Samples.Count), key.InvalidInput())
    from targetIndexCount in Admit.SameCount(expected: rows, key: key, counts: [match.TargetIndices.Length])
    from targetIndices in guard(match.TargetIndices.All(index => index >= 0 && index < targetPca.Samples.Count), key.InvalidInput())
    from seedField in PrecisionFieldOf(source: source, match: match, sourcePca: sourcePca, targetPca: targetPca, current: current, bands: bands, key: key)
    from initial in ObjectiveOf(source: source, match: match, precision: seedField, current: current, bands: bands, key: key)
    let model = new GicpModel(source: source, match: match, sourcePca: sourcePca, targetPca: targetPca, current: current, seedField: seedField, seedObjective: initial, bands: bands, key: key)
    from result in Lm.Minimize(model: model, policy: policy.Ladder(bands), key: key)
    select GicpStep(model: model, result: result, initial: initial, sourcePca: sourcePca, targetPca: targetPca, bands: bands);

private static AlignmentStep GicpStep(GicpModel model, LmResult result, GicpObjective initial, NeighborhoodPcaResult sourcePca, NeighborhoodPcaResult targetPca, AlignBands bands) {
    double stepNorm = Math.Sqrt(d: result.Parameters.Sum(static value => value * value));
    GicpObjective at = result.Iterations == 0 ? initial : model.Objective;
    bool converged = result.Status == SolveStatus.Converged;
    AlignmentOptimizerStopKind stop = converged
        ? result.Iterations == 0 ? AlignmentOptimizerStopKind.StepBelowTolerance : AlignmentOptimizerStopKind.StepAccepted
        : model.LastFault.IsSome ? AlignmentOptimizerStopKind.ModelRefused
        : AlignmentOptimizerStopKind.BudgetExhausted;
    GicpSolve gicp = new(Stop: stop, Iterations: result.Iterations, InitialCost: (double)initial.Cost, FinalCost: (double)at.Cost, StepNorm: stepNorm,
        TerminalLambda: result.Lambda, MeanMahalanobis: at.MeanMahalanobis, MaxMahalanobis: at.MaxMahalanobis,
        RegularizedCovarianceCount: at.RegularizedCount, CovarianceRidge: at.Ridge, SourcePca: sourcePca.Census, TargetPca: targetPca.Census);
    Transform delta = result.Iterations == 0 ? Transform.Identity : ComposeRigidTransform(
        omega: new Vector3d(x: result.Parameters[0], y: result.Parameters[1], z: result.Parameters[2]),
        translation: new Vector3d(x: result.Parameters[3], y: result.Parameters[4], z: result.Parameters[5]), bands: bands);
    return new AlignmentStep(Delta: delta, Optimizer: Some(gicp), Stop: !converged ? Some(AlignmentStopKind.OptimizerStopped)
        : result.Iterations == 0 ? Some(AlignmentStopKind.Converged) : Option<AlignmentStopKind>.None);
}
```

## To

```csharp
// AlignmentStopKind DELETED
// AlignmentOptimizerStopKind DELETED

internal readonly record struct AlignBands(
    double Neglect, double Convergence, double Residual, double Orientation,
    double Real, double Ridge, SolvePolicy Solver);

// AlignBands.Of DELETED
```

```csharp
[ComplexValueObject]
public sealed partial class AlignmentPolicy {
    public static AlignmentPolicy Default { get; } = Create(
        maxIterations: Dimension.Create(30),
        convergence: ToleranceLane.Convergence, residual: ToleranceLane.Residual,
        step: ToleranceLane.Step, ridge: ToleranceLane.Kkt,
        robustScale: UnitInterval.Create(0.1), optimizerBudget: Option<Dimension>.None,
        fit: PoseFit.Rigid, trimFraction: Option<UnitInterval>.None,
        coarseLevels: Dimension.Create(1));

    public Dimension MaxIterations { get; }
    public ToleranceLane Convergence { get; }
    public ToleranceLane Residual { get; }
    public ToleranceLane Step { get; }
    public ToleranceLane Ridge { get; }
    public UnitInterval RobustScale { get; }
    public Option<Dimension> OptimizerBudget { get; }
    public PoseFit Fit { get; }
    public Option<UnitInterval> TrimFraction { get; }
    public Dimension CoarseLevels { get; }

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError, ref Dimension maxIterations,
        ref ToleranceLane convergence, ref ToleranceLane residual, ref ToleranceLane step,
        ref ToleranceLane ridge, ref UnitInterval robustScale, ref Option<Dimension> optimizerBudget,
        ref PoseFit fit, ref Option<UnitInterval> trimFraction, ref Dimension coarseLevels) =>
        validationError = robustScale.Value > 0.0
            && trimFraction.Map(static value => value.Value < 1.0).IfNone(true)
            ? null
            : new ValidationError("AlignmentPolicy requires nonzero robust scale and trim fraction below one.");
}

// AlignmentPolicy.Admit DELETED
// AlignmentPolicy.Ladder DELETED
```

```csharp
public readonly record struct GicpSolve(
    SolveStatus Stop, int Iterations, double InitialCost, double FinalCost, double StepNorm,

internal readonly record struct AlignmentStep(
    Transform Delta, Option<LinearSolution> Solve = default, Option<RobustWeights> Robust = default,
    Option<GicpSolve> Optimizer = default, Option<SolveStatus> Stop = default, Option<double> Scale = default);

public readonly record struct Alignment(
    Transform Transform, AlignKind Kind, SolveStatus Stop, int Iterations, Option<double> FinalDelta,
```

```csharp
ProjectionRow.Of<Transform>(() => self.Stop == SolveStatus.Converged
    ? key.AcceptValue(self.Transform) : Fin.Fail<Transform>(key.InvalidResult())));

private readonly record struct IcpState(
    Transform Current, Option<double> FinalDelta, int Iterations, AlignmentStep Step, Option<SolveStatus> Stop);
```

```csharp
internal static Fin<Alignment> AlignClouds(AlignKind kind, VectorCloud source, VectorCloud target, AlignmentPolicy policy, Op key) =>
    (source, target) switch {
        (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt) => IcpAlign(src, tgt, kind, policy, key),
        _ => Fin.Fail<Alignment>(key.InvalidInput()),
    };

private static Fin<Alignment> IcpAlign(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, AlignKind kind, AlignmentPolicy policy, Op key) =>
    from solver in SolvePolicy.Of(source.Tolerance, key, policy.OptimizerBudget)
    from activeSolver in (solver with {
        ResidualTolerance = PositiveMagnitude.Create(source.Tolerance.For(policy.Residual).Value),
        StepFloor = source.Tolerance.For(policy.Step).Value,
    }).Admit(key)
    let bands = new AlignBands(
        source.Tolerance.For(ToleranceLane.Neglect).Value,
        source.Tolerance.For(policy.Convergence).Value,
        activeSolver.ResidualTolerance.Value,
        source.Tolerance.For(ToleranceLane.Orientation).Value,
        source.Tolerance.For(ToleranceLane.Real).Value,
        source.Tolerance.For(policy.Ridge).Value,
        activeSolver)
    from neighborhoodPolicy in NeighborhoodPolicy.Of(source.Tolerance, key)
```

```csharp
initialState: Fin.Succ(new IcpState(Transform.Identity, Option<double>.None, 0,
    new AlignmentStep(Transform.Identity), Option<SolveStatus>.None)),

select new Alignment(Transform: final.Current, Kind: kind,
    Stop: final.Stop.IfNone(SolveStatus.Exhausted), Iterations: final.Iterations, FinalDelta: final.FinalDelta,

Atom<Fin<IcpState>> cell = Atom(Fin.Succ(carried with { Stop = Option<SolveStatus>.None }));

select new IcpState(current, Some(finalDelta), state.Iterations + 1, step,
    step.Stop.IsSome ? step.Stop
        : finalDelta < bands.Convergence ? Some(SolveStatus.Converged) : Option<SolveStatus>.None);
```

```csharp
internal static Fin<AlignmentStep> SolveGeneralizedIcp(
    Seq<Point3d> source, AlignmentMatch match, Transform current, AlignBands bands, Op key) =>
    from sourcePca in match.SourcePca.ToFin(key.InvalidInput())
    from targetPca in match.TargetPca.ToFin(key.InvalidInput())
    from rows in AdmitAlignmentRows(source, match.Targets, match.RowMass, ProcrustesFloor, key)
    from sourceRowCount in Admit.SameCount(expected: rows, key: key, counts: [match.SourceRows.Length])
    from sourceRows in guard(match.SourceRows.All(row => row >= 0 && row < sourcePca.Samples.Count), key.InvalidInput())
    from targetIndexCount in Admit.SameCount(expected: rows, key: key, counts: [match.TargetIndices.Length])
    from targetIndices in guard(match.TargetIndices.All(index => index >= 0 && index < targetPca.Samples.Count), key.InvalidInput())
    from seedField in PrecisionFieldOf(source, match, sourcePca, targetPca, current, bands, key)
    from initial in ObjectiveOf(source, match, seedField, current, bands, key)
    let model = new GicpModel(source, match, sourcePca, targetPca, current, seedField, initial, bands, key)
    from result in Lm.Minimize(model, bands.Solver, key).MapFail(fault => model.Fault.IfNone(fault))
    from step in model.Fault.Match(
        Some: fault => Fin.Fail<AlignmentStep>(fault),
        None: () => {
            GicpObjective at = result.Iterations == 0 ? initial : model.Memo.Objective;
            GicpSolve gicp = new(result.Status, result.Iterations, (double)initial.Cost, (double)at.Cost,
                Math.Sqrt(result.Parameters.Sum(static value => value * value)), result.Lambda,
                at.MeanMahalanobis, at.MaxMahalanobis, at.RegularizedCount, at.Ridge, sourcePca.Census, targetPca.Census);
            Transform delta = result.Iterations == 0 ? Transform.Identity : ComposeRigidTransform(
                new(result.Parameters[0], result.Parameters[1], result.Parameters[2]),
                new(result.Parameters[3], result.Parameters[4], result.Parameters[5]), bands);
            return Fin.Succ(new AlignmentStep(delta, Optimizer: Some(gicp),
                Stop: result.Status != SolveStatus.Converged || result.Iterations == 0
                    ? Some(result.Status) : Option<SolveStatus>.None));
        })
    select step;

// GicpStep DELETED
```

## Why

`Dimension` already excludes counts below one, yet the raw record is revalidated by both `VectorIntent.Align` and `AlignDetailed`. `SolvePolicy.Canonical` does not exist, and the fixed eight-pass optimizer budget can violate `SolvePolicy.Admit` before the lambda ladder reaches its ceiling. Both local stop vocabularies duplicate `SolveStatus`, while `ModelRefused` converts the original retained `Error` into success-shaped evidence.

## Change

Make `AlignmentPolicy` a Thinktecture complex value object so generated construction admits it once; retain only the two refinements not already carried by `Dimension` and `UnitInterval`. Make the optimizer budget optional so absence delegates to `SolvePolicy.Of`. Resolve the solver policy once per alignment, reuse `SolveStatus`, return retained model errors on `Fin`, and inline the one-call `GicpStep` projection.

## Delta

Code-fence LOC: -20. Authored module-level types: -2. Authored module-level methods: -4. Public smart-enum row members: -7. Raw public policy constructors: -1. Net authored module-level symbols/members/types: -14.

## Ripples

Update `libs/dotnet/Rasm/.planning/Processing/register.md:13-16,23,701-707` to describe generated policy admission, `SolvePolicy`, `SolveStatus`, and model refusal on `Fin`. In `libs/dotnet/Rasm/.planning/Processing/intent.md:230-236`, delete the second `AlignmentPolicy.Admit` and store `policy.IfNone(AlignmentPolicy.Default)` directly. In `libs/dotnet/Rasm.Fabrication/.planning/Verify/probing.md:704-716`, encode `OptimizerBudget` with `.Maybe(policy.OptimizerBudget.Map(static budget => budget.Value), static (row, budget) => row.I64(budget))`.

# 2. Localize GICP transfer state and repair covariance fusion

## From

`libs/dotnet/Rasm/.planning/Processing/register.md:534-574`

```csharp
private static Fin<GicpPrecisionField> PrecisionFieldOf(Seq<Point3d> source, AlignmentMatch match, NeighborhoodPcaResult sourcePca, NeighborhoodPcaResult targetPca, Transform current, AlignBands bands, Op key) =>
    toSeq(Enumerable.Range(start: 0, count: source.Count)).Fold(
        initialState: Fin.Succ((Inverses: new SymmetricMatrix[source.Count], Regularized: 0, Ridge: 0.0)),
        f: (acc, i) => acc.Bind(state =>
            PrecisionOf(current: current, source: sourcePca.Samples[index: match.SourceRows[i]].Covariance, target: targetPca.Samples[index: match.TargetIndices[i]].Covariance, bands: bands, key: key)
                .Map(precision => { state.Inverses[i] = precision.Inverse; return (state.Inverses, state.Regularized + (precision.Regularized ? 1 : 0), Math.Max(val1: state.Ridge, val2: precision.Ridge)); })))
        .Map(state => new GicpPrecisionField(Inverses: state.Inverses, RegularizedCount: state.Regularized, Ridge: state.Ridge));

[StructLayout(LayoutKind.Auto)] private readonly record struct GicpPrecision(SymmetricMatrix Inverse, bool Regularized, double Ridge);

private static Fin<GicpPrecision> PrecisionOf(Transform current, SymmetricMatrix source, SymmetricMatrix target, AlignBands bands, Op key) {
    Span<double> rs = stackalloc double[9];
    for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) { double sum = 0.0; for (int k = 0; k < 3; k++) sum += current[i, k] * source.At(i: k, j: j); rs[(i * 3) + j] = sum; }
    double[] upper = new double[6]; int slot = 0; double trace = 0.0;
    for (int i = 0; i < 3; i++) for (int j = i; j < 3; j++) {
        double rrt = 0.0;
        for (int k = 0; k < 3; k++) rrt += rs[(i * 3) + k] * current[j, k];
        double value = target.At(i: i, j: j) + rrt;
        if (i == j) trace += value;
        upper[slot++] = value;
    }
    double floor = Math.Max(val1: bands.Residual, val2: bands.Ridge * Math.Max(val1: Math.Abs(value: trace / 3.0), val2: 1.0));
    return from fused in SymmetricMatrix.Of(dim: Dimension.Create(value: 3), upper: new Arr<double>(upper), key: key)
           from eigen in fused.DecomposeEigenDetailed(key: key)
           from inverse in SpectralInverseOf(pairs: eigen.Pairs, floor: floor, key: key)
           select new GicpPrecision(Inverse: inverse.Matrix, Regularized: inverse.Clamped > 0, Ridge: floor);
}

private static Fin<(SymmetricMatrix Matrix, int Clamped)> SpectralInverseOf(Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs, double floor, Op key) {
    if (pairs.Count != 3) return Fin.Fail<(SymmetricMatrix, int)>(key.InvalidResult());
    double[] upper = new double[6]; int clamped = 0;
    foreach ((double eigenvalue, Arr<double> vector) in pairs) {
        double lambda = Math.Max(val1: eigenvalue, val2: floor);
        if (eigenvalue < floor) clamped++;
        double inv = 1.0 / lambda; int slot = 0;
        for (int i = 0; i < 3; i++) for (int j = i; j < 3; j++) upper[slot++] += inv * vector[index: i] * vector[index: j];
    }
    return Admit.AllFinite(upper, key)
        .Bind(_ => SymmetricMatrix.Of(dim: Dimension.Create(value: 3), upper: new Arr<double>(upper), key: key))
        .Map(matrix => (matrix, clamped));
}
```

`libs/dotnet/Rasm/.planning/Processing/register.md:599-617`

```csharp
[StructLayout(LayoutKind.Auto)] private readonly record struct GicpMemo(double[] At, GicpPrecisionField Field, GicpObjective Objective);

private sealed class GicpModel(
    Seq<Point3d> source, AlignmentMatch match, NeighborhoodPcaResult sourcePca, NeighborhoodPcaResult targetPca,
    Transform current, GicpPrecisionField seedField, GicpObjective seedObjective, AlignBands bands, Op key) : ILmModel {
    private GicpMemo memo = new(At: new double[6], Field: seedField, Objective: seedObjective);
    private Option<Error> lastFault = None;

    public int Dof => 6;
    public double[] Seed { get; } = new double[6];
    internal GicpObjective Objective => memo.Objective;
    internal Option<Error> LastFault => lastFault;

    public ddouble Norm(ReadOnlySpan<double> parameters) {
        double[] at = parameters.ToArray();
        Transform trial = TrialOf(parameters: parameters);
        return (from field in PrecisionFieldOf(source: source, match: match, sourcePca: sourcePca, targetPca: targetPca, current: trial, bands: bands, key: key)
                from objective in ObjectiveOf(source: source, match: match, precision: field, current: trial, bands: bands, key: key)
                select new GicpMemo(At: at, Field: field, Objective: objective))
```

## To

```csharp
private static Fin<GicpPrecisionField> PrecisionFieldOf(
    Seq<Point3d> source, AlignmentMatch match, NeighborhoodPcaResult sourcePca,
    NeighborhoodPcaResult targetPca, Transform current, AlignBands bands, Op key) {
    return toSeq(Enumerable.Range(0, source.Count)).Fold(
        Fin.Succ((Inverses: new SymmetricMatrix[source.Count], Regularized: 0, Ridge: 0.0)),
        (acc, i) => acc.Bind(state => Precision(
            sourcePca.Samples[match.SourceRows[i]].Covariance,
            targetPca.Samples[match.TargetIndices[i]].Covariance)
            .Map(row => {
                state.Inverses[i] = row.Inverse;
                return (state.Inverses, state.Regularized + (row.Regularized ? 1 : 0), Math.Max(state.Ridge, row.Ridge));
            })))
        .Map(state => new GicpPrecisionField(state.Inverses, state.Regularized, state.Ridge));

    Fin<(SymmetricMatrix Inverse, bool Regularized, double Ridge)> Precision(SymmetricMatrix sourceCovariance, SymmetricMatrix targetCovariance) {
        Span<double> rs = stackalloc double[9];
        for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) {
            double sum = 0.0;
            for (int k = 0; k < 3; k++) sum += current[i, k] * sourceCovariance.At(k, j);
            rs[(i * 3) + j] = sum;
        }
        double[] upper = new double[6]; int slot = 0; double trace = 0.0;
        for (int i = 0; i < 3; i++) for (int j = i; j < 3; j++) {
            double rrt = 0.0;
            for (int k = 0; k < 3; k++) rrt += rs[(i * 3) + k] * current[j, k];
            double value = targetCovariance.At(i, j) + rrt;
            if (i == j) trace += value;
            upper[slot++] = value;
        }
        double floor = Math.Max(bands.Residual, bands.Ridge * Math.Max(Math.Abs(trace / 3.0), 1.0));
        return from fused in SymmetricMatrix.Of(Dimension.Create(3), new Arr<double>(upper), key)
               from eigen in fused.DecomposeEigenDetailed(key)
               from inverse in SpectralInverse(eigen.Pairs, floor)
               select (inverse.Matrix, inverse.Clamped > 0, floor);
    }

    Fin<(SymmetricMatrix Matrix, int Clamped)> SpectralInverse(Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs, double floor) {
        if (pairs.Count != 3) return Fin.Fail<(SymmetricMatrix, int)>(key.InvalidResult());
        double[] upper = new double[6]; int clamped = 0;
        foreach ((double eigenvalue, Arr<double> vector) in pairs) {
            double lambda = Math.Max(eigenvalue, floor);
            if (eigenvalue < floor) clamped++;
            double inverse = 1.0 / lambda; int slot = 0;
            for (int i = 0; i < 3; i++) for (int j = i; j < 3; j++) upper[slot++] += inverse * vector[i] * vector[j];
        }
        return Admit.AllFinite(upper, key)
            .Bind(_ => SymmetricMatrix.Of(Dimension.Create(3), new Arr<double>(upper), key))
            .Map(matrix => (matrix, clamped));
    }
}

// GicpPrecision DELETED
// PrecisionOf DELETED
// SpectralInverseOf DELETED
```

```csharp
// GicpMemo DELETED

private sealed class GicpModel(
    Seq<Point3d> source, AlignmentMatch match, NeighborhoodPcaResult sourcePca, NeighborhoodPcaResult targetPca,
    Transform current, GicpPrecisionField seedField, GicpObjective seedObjective, AlignBands bands, Op key) : ILmModel {
    internal (double[] At, GicpPrecisionField Field, GicpObjective Objective) Memo = (new double[6], seedField, seedObjective);
    internal Option<Error> Fault = None;

    public int Dof => 6;
    public double[] Seed { get; } = new double[6];

    public ddouble Norm(ReadOnlySpan<double> parameters) {
        double[] at = parameters.ToArray();
        Transform trial = TrialOf(parameters);
        return (from field in PrecisionFieldOf(source, match, sourcePca, targetPca, trial, bands, key)
                from objective in ObjectiveOf(source, match, field, trial, bands, key)
                select (At: at, Field: field, Objective: objective))
            .Match(
                Succ: held => { Memo = held; return ddouble.Sqrt(held.Objective.Cost); },
                Fail: fault => { Fault = Some(fault); return (ddouble)double.PositiveInfinity; });
    }
```

## Why

`GicpPrecision` and `GicpMemo` are single-producer transfer shapes with no independent invariant. Their two precision methods likewise serve only `PrecisionFieldOf`. The fused covariance also adds `target.At(i, i)` to every upper-triangle cell in row `i`; off-diagonal cells require `target.At(i, j)` for `R S Rᵀ + T`.

## Change

Localize both precision functions, replace both transfer records with named tuples, and remove the model's forwarding properties. Correct the target covariance index while preserving the spectral clamp, matrix admission, `Fin` flow, and persistent precision-field aggregate.

## Delta

Code-fence LOC: -5. Private nested types: -2. Module-level methods: -2. Nested forwarding properties: -2. Net module-level symbols/members/types: -6.

## Ripples

In `GicpModel.Linearize`, replace `memo` and `lastFault` with `Memo` and `Fault`. Task 1's final GICP projection already reads `model.Fault` and `model.Memo.Objective`. Update `libs/dotnet/Rasm/.planning/Processing/register.md:23` to state the corrected covariance fusion without naming deleted transfer records.

# 3. Inline forwarding solves and derive correspondence metadata

## From

`libs/dotnet/Rasm/.planning/Processing/register.md:84-102`

```csharp
public static readonly PoseFit Rigid = new(key: 0, AlignKernel.SeatRigid);
public static readonly PoseFit Similarity = new(key: 1, AlignKernel.SeatSimilarity);

public static readonly AlignKind Point = new(key: 0, needs: CapabilitySet<AlignNeed>.None,
    solveStep: static (source, match, current, policy, bands, key) => AlignKernel.SolvePointToPoint(source: source, target: match.Targets, rowMass: match.RowMass, current: current, policy: policy, bands: bands, key: key));
public static readonly AlignKind Plane = new(key: 1, needs: CapabilitySet<AlignNeed>.Of(AlignNeed.TargetNormals),
    solveStep: static (source, match, current, _, bands, key) => AlignKernel.SolvePointToPlane(source: source, target: match.Targets, normals: match.Normals, rowMass: match.RowMass, current: current, bands: bands, key: key));
public static readonly AlignKind Symmetric = new(key: 2, needs: CapabilitySet<AlignNeed>.Of(AlignNeed.TargetNormals, AlignNeed.SourceNormals),
    solveStep: static (source, match, current, _, bands, key) => AlignKernel.SolveSymmetric(source: source, target: match.Targets, normals: match.Normals, sourceNormals: match.SourceNormals, rowMass: match.RowMass, current: current, bands: bands, key: key));
public static readonly AlignKind NormalWeightedPointToPlane = new(key: 4, needs: CapabilitySet<AlignNeed>.Of(AlignNeed.TargetNormals, AlignNeed.SourceNormals),
    solveStep: static (source, match, current, _, bands, key) => AlignKernel.SolveNormalWeightedPointToPlane(source: source, target: match.Targets, targetNormals: match.Normals, sourceNormals: match.SourceNormals, rowMass: match.RowMass, current: current, bands: bands, key: key));
```

`libs/dotnet/Rasm/.planning/Processing/register.md:263-301`

```csharp
let finalDelta = DeltaMagnitude(delta: step.Delta)

private static double DeltaMagnitude(Transform delta) {
    double diff = 0.0;
    for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) diff += Math.Abs(value: delta[i, j] - (i == j ? 1.0 : 0.0));
    return diff;
}
```

`libs/dotnet/Rasm/.planning/Processing/register.md:287-294`

```csharp
return CorrespondenceSetOf(items: items, distances: distances, rowMass: rowMass, targetIndices: targetIndices, targetMass: targetMass, sourceCount: kept.Length, targetCount: match.Correspondences.TargetCount, bands: bands, key: key)
    .Bind(set => key.AcceptValue(value: (
        Source: toSeq(sourceRows.Select(row => source[index: row])),
        Match: match with {
            Correspondences = set,
            Targets = targets, Normals = normals, SourceNormals = rowSourceNormals, Distances = distances,
            RowMass = rowMass, TargetIndices = targetIndices, SourceRows = sourceRows,
        })));
```

`libs/dotnet/Rasm/.planning/Processing/register.md:332-351`

```csharp
return CorrespondenceSetOf(items: items, distances: distances, rowMass: rowMass, targetIndices: targetIndices, targetMass: targetMass, sourceCount: n, targetCount: target.Vertices.Count, bands: bands, key: key)
    .Map(set => new AlignmentMatch(Correspondences: set,
        Targets: targets, Normals: rowNormals, SourceNormals: rowSourceNormals, Distances: distances, RowMass: rowMass, TargetIndices: targetIndices, SourceRows: rows, SourcePca: sourcePca, TargetPca: targetPca));

private static Fin<CloudCorrespondenceSet> CorrespondenceSetOf(List<CloudCorrespondence> items, double[] distances, double[] rowMass, int[] targetIndices, Arr<double> targetMass, int sourceCount, int targetCount, AlignBands bands, Op key) {
    double totalMass = TensorPrimitives.Sum<double>(rowMass);
    if (distances.Length == 0 || totalMass <= bands.Neglect) { return Fin.Fail<CloudCorrespondenceSet>(key.InvalidResult()); }
    Seq<Scalar> rows = toSeq(distances).Map(static value => (Scalar)value);
    Seq<int> covered = toSeq(targetIndices).Distinct().Strict();
    return from spread in Distribution<Scalar>.Of(values: rows, percentiles: [90.0, 95.0], key: key, rule: Some(QuantileRule.NearestRank))
           from weighted in Stat<Scalar>.Of(values: rows, key: key, weights: Some(toSeq(rowMass)))
           from coupling in Stat<Scalar>.Of(values: toSeq(rowMass).Map(static value => (Scalar)value), key: key)
           select new CloudCorrespondenceSet(Items: toSeq(items), SourceCount: sourceCount, TargetCount: targetCount,
               Measurements: Some((Coupling: coupling, WeightedDistance: weighted, Distance: spread)),
               CoveredSourceCount: sourceCount, CoveredTargetCount: covered.Count,
               RetainedSourceMass: totalMass,
               RetainedTargetMass: covered.Fold(0.0, (held, index) => held + targetMass[index: index]));
}
```

`libs/dotnet/Rasm/.planning/Processing/register.md:355-392`

```csharp
internal static Fin<AlignmentStep> SolvePointToPoint(Seq<Point3d> source, Point3d[] target, double[] rowMass, Transform current, AlignmentPolicy policy, AlignBands bands, Op key) =>
    SolveProcrustes(source: source, target: target, weights: rowMass, current: current, fit: policy.Fit, bands: bands, key: key)
        .Map(static aligned => new AlignmentStep(Delta: aligned.Delta, Scale: aligned.Scale));

internal static Fin<AlignmentStep> SolvePointToPlane(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, double[] rowMass, Transform current, AlignBands bands, Op key) =>
    SolveLinearizedRows(source: source, target: target, normals: normals, rowMass: rowMass, current: current, bands: bands, key: key, rowNormal: static (_, normal) => (Normal: normal, Weight: 1.0));

internal static Fin<AlignmentStep> SolveSymmetric(Seq<Point3d> source, Point3d[] target, Vector3d[] normals, Vector3d[] sourceNormals, double[] rowMass, Transform current, AlignBands bands, Op key) =>
    Admit.SameCount(expected: source.Count, key: key, counts: [sourceNormals.Length]).Bind(_ => SolveLinearizedRows(
        source: source, target: target, normals: normals, rowMass: rowMass, current: current, bands: bands, key: key,
        rowNormal: (i, targetNormal) => {
            Vector3d rotated = current * sourceNormals[i];
            Vector3d sourceNormal = rotated * targetNormal < 0.0 ? -rotated : rotated;
            Vector3d combined = sourceNormal + targetNormal;
            _ = combined.Unitize();
            return (Normal: combined, Weight: 1.0);
        }));

internal static Fin<AlignmentStep> SolveNormalWeightedPointToPlane(Seq<Point3d> source, Point3d[] target, Vector3d[] targetNormals, Vector3d[] sourceNormals, double[] rowMass, Transform current, AlignBands bands, Op key) =>
    Admit.SameCount(expected: source.Count, key: key, counts: [sourceNormals.Length]).Bind(_ => SolveLinearizedRows(
        source: source, target: target, normals: targetNormals, rowMass: rowMass, current: current, bands: bands, key: key,
        rowNormal: (i, normal) => (Normal: normal, Weight: Math.Sqrt(d: Math.Max(val1: Math.Abs(value: (current * sourceNormals[i]) * normal), val2: bands.Real)))));
```

`libs/dotnet/Rasm/.planning/Processing/register.md:479-485`

```csharp
let rotation = RotationTransformOf(rotation: rot)
from scaled in fit.Seat(source: source, target: target, srcCentroid: srcCentroid, tgtCentroid: tgtCentroid, weights: weights, rotation: rotation, sourceSpread: sourceSpread, bands: bands, key: key)
select scaled;
}

internal static Fin<(Transform Delta, Option<double> Scale)> SeatRigid(Seq<Point3d> source, Seq<Point3d> target, Point3d srcCentroid, Point3d tgtCentroid, double[] weights, Transform rotation, double sourceSpread, AlignBands bands, Op key) =>
    key.AcceptValue(value: (Delta: WithTranslation(rotation: rotation, translation: tgtCentroid - (rotation * srcCentroid)), Scale: Option<double>.None));
```

`libs/dotnet/Rasm/.planning/Processing/register.md:632-670`

```csharp
double[] jacobian = JacobianOf(point: x);

private static double[] JacobianOf(Point3d point) => [
    0.0, -point.Z, point.Y, -1.0, 0.0, 0.0,
    point.Z, 0.0, -point.X, 0.0, -1.0, 0.0,
    -point.Y, point.X, 0.0, 0.0, 0.0, -1.0,
];

private static Transform RotationTransformOf(Matrix rotation) {
    Transform xform = Transform.Identity;
    for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) xform[i, j] = rotation.At(i: i, j: j);
    return xform;
}
```

## To

```csharp
public static readonly PoseFit Rigid = new(key: 0,
    seat: static (_, _, source, target, _, rotation, _, _, key) =>
        key.AcceptValue((AlignKernel.WithTranslation(rotation, target - (rotation * source)), Option<double>.None)));
public static readonly PoseFit Similarity = new(key: 1, AlignKernel.SeatSimilarity);
```

```csharp
public static readonly AlignKind Point = new(key: 0, needs: CapabilitySet<AlignNeed>.None,
    solveStep: static (source, match, current, policy, bands, key) =>
        AlignKernel.SolveProcrustes(source, match.Targets, match.RowMass, current, policy.Fit, bands, key)
            .Map(static fit => new AlignmentStep(fit.Delta, Scale: fit.Scale)));
public static readonly AlignKind Plane = new(key: 1, needs: CapabilitySet<AlignNeed>.Of(AlignNeed.TargetNormals),
    solveStep: static (source, match, current, _, bands, key) => AlignKernel.SolveLinearizedRows(
        source, match.Targets, match.Normals, match.RowMass, current, bands, key, static (_, normal) => (normal, 1.0)));
public static readonly AlignKind Symmetric = new(key: 2, needs: CapabilitySet<AlignNeed>.Of(AlignNeed.TargetNormals, AlignNeed.SourceNormals),
    solveStep: static (source, match, current, _, bands, key) =>
        Admit.SameCount(expected: source.Count, key: key, counts: [match.SourceNormals.Length]).Bind(_ =>
            AlignKernel.SolveLinearizedRows(source, match.Targets, match.Normals, match.RowMass, current, bands, key,
                (i, targetNormal) => {
                    Vector3d rotated = current * match.SourceNormals[i];
                    Vector3d normal = rotated * targetNormal < 0.0 ? -rotated : rotated;
                    normal += targetNormal;
                    _ = normal.Unitize();
                    return (normal, 1.0);
                })));
public static readonly AlignKind NormalWeightedPointToPlane = new(key: 4, needs: CapabilitySet<AlignNeed>.Of(AlignNeed.TargetNormals, AlignNeed.SourceNormals),
    solveStep: static (source, match, current, _, bands, key) =>
        Admit.SameCount(expected: source.Count, key: key, counts: [match.SourceNormals.Length]).Bind(_ =>
            AlignKernel.SolveLinearizedRows(source, match.Targets, match.Normals, match.RowMass, current, bands, key,
                (i, normal) => (normal, Math.Sqrt(Math.Max(Math.Abs((current * match.SourceNormals[i]) * normal), bands.Real))))));
```

```csharp
let finalDelta = Magnitude(step.Delta)

static double Magnitude(Transform delta) {
    double total = 0.0;
    for (int i = 0; i < 4; i++) for (int j = 0; j < 4; j++) total += Math.Abs(delta[i, j] - (i == j ? 1.0 : 0.0));
    return total;
}

// DeltaMagnitude DELETED
```

```csharp
return CorrespondenceSetOf(items, distances, rowMass, targetMass, bands, key)
    .Bind(set => key.AcceptValue((Source: toSeq(sourceRows.Select(row => source[row])), Match: match with {
        Correspondences = set, Targets = targets, Normals = normals, SourceNormals = rowSourceNormals,
        Distances = distances, RowMass = rowMass, TargetIndices = targetIndices, SourceRows = sourceRows,
    })));

return CorrespondenceSetOf(items, distances, rowMass, targetMass, bands, key)
    .Map(set => new AlignmentMatch(set, targets, rowNormals, rowSourceNormals,
        distances, rowMass, targetIndices, rows, sourcePca, targetPca));
```

```csharp
private static Fin<CloudCorrespondenceSet> CorrespondenceSetOf(
    List<CloudCorrespondence> items, double[] distances, double[] rowMass,
    Arr<double> targetMass, AlignBands bands, Op key) {
    double totalMass = TensorPrimitives.Sum<double>(rowMass);
    if (distances.Length == 0 || totalMass <= bands.Neglect) return Fin.Fail<CloudCorrespondenceSet>(key.InvalidResult());
    Seq<Scalar> rows = toSeq(distances).Map(static value => (Scalar)value);
    Seq<int> covered = toSeq(items.Select(static item => item.TargetIndex)).Distinct().Strict();
    return from spread in Distribution<Scalar>.Of(rows, [90.0, 95.0], key, Some(QuantileRule.NearestRank))
           from weighted in Stat<Scalar>.Of(rows, key, Some(toSeq(rowMass)))
           from coupling in Stat<Scalar>.Of(toSeq(rowMass).Map(static value => (Scalar)value), key)
           select new CloudCorrespondenceSet(Items: toSeq(items), SourceCount: items.Count, TargetCount: targetMass.Count,
               Measurements: Some((coupling, weighted, spread)), CoveredSourceCount: items.Count, CoveredTargetCount: covered.Count,
               RetainedSourceMass: totalMass, RetainedTargetMass: covered.Fold(0.0, (total, index) => total + targetMass[index]));
}
```

```csharp
// SolvePointToPoint DELETED
// SolvePointToPlane DELETED
// SolveSymmetric DELETED
// SolveNormalWeightedPointToPlane DELETED
// SeatRigid DELETED
// JacobianOf DELETED
// RotationTransformOf DELETED

internal static Fin<(Transform Delta, Option<double> Scale)> SolveProcrustes(
    Seq<Point3d> source, Point3d[] target, double[] weights, Transform current, PoseFit fit, AlignBands bands, Op key)

internal static Fin<AlignmentStep> SolveLinearizedRows(
    Seq<Point3d> source, Point3d[] target, Vector3d[] normals, double[] rowMass, Transform current,
    AlignBands bands, Op key, Func<int, Vector3d, (Vector3d Normal, double Weight)> rowNormal)

internal static Transform WithTranslation(Transform rotation, Vector3d translation)
```

```csharp
let rotation = Rotation(rot)

static Transform Rotation(Matrix source) {
    Transform result = Transform.Identity;
    for (int i = 0; i < 3; i++) for (int j = 0; j < 3; j++) result[i, j] = source.At(i, j);
    return result;
}

double[] jacobian = [
    0.0, -x.Z, x.Y, -1.0, 0.0, 0.0,
    x.Z, 0.0, -x.X, 0.0, -1.0, 0.0,
    -x.Y, x.X, 0.0, 0.0, 0.0, -1.0,
];
```

## Why

Four `Solve*` methods and `SeatRigid` each have one generated-row caller and only forward to a shared kernel. The delta norm, Jacobian literal, and matrix conversion are single-site calculation details. `CorrespondenceSetOf` also receives target indices and both counts even though `items` and `targetMass` already own them.

## Change

Bind the five one-call behaviors directly to their Thinktecture row delegates; keep only the shared Procrustes, linearized, robust, generalized, and similarity kernels. Move single-site calculations to locals or their loop. Derive coverage and counts inside `CorrespondenceSetOf`, removing three redundant parameters without changing evidence.

## Delta

Code-fence LOC: -9. Module-level methods: -8. Module-level method parameters: -3. Types and fields: 0. Net module-level symbols/members/types: -11.

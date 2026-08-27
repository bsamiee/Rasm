# 1. Close and collapse transport policy

### Location

- `transport.md:57-61`, anchor `public sealed partial class TransportEstimator`
- `transport.md:75-95`, anchors `CloudTransportPolicy`, `CloudTransportPolicy.Of`, `convergenceTolerance`, and `couplingCutoff`
- `transport.md:121-129`, anchors `policy.Estimator.Switch`, both self-solves, and `plan.Project<TOut>`
- `transport.md:230-231`, anchor `SinkhornPlan.Project<TOut>` bias parameters

### From

```csharp
[SmartEnum<int>]
public sealed partial class TransportEstimator {
    public static readonly TransportEstimator Entropic = new(key: 0);
    public static readonly TransportEstimator Debiased = new(key: 1);
}
```

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct CloudTransportPolicy(
    PositiveMagnitude Regularization, Dimension MaxIterations, TransportEstimator Estimator,
    Option<PositiveMagnitude> MassRelaxation, PositiveMagnitude ConvergenceTolerance, PositiveMagnitude CouplingCutoff) {
```

```csharp
public static Fin<CloudTransportPolicy> Of(double regularization, int maxIterations, Context context,
    Option<TransportEstimator> estimator = default, Option<double> massRelaxation = default,
    Option<double> convergenceTolerance = default, Option<double> couplingCutoff = default, Op? key = null) {
```

```csharp
from tolerance in op.AcceptValidated<PositiveMagnitude>(
    candidate: convergenceTolerance.IfNone(context.For(lane: ToleranceLane.Convergence).Value))
from cutoff in op.AcceptValidated<PositiveMagnitude>(
    candidate: couplingCutoff.IfNone(context.For(lane: ToleranceLane.Neglect).Value))
```

```csharp
from bias in policy.Estimator.Switch(
    entropic: () => Fin.Succ((Source: Option<double>.None, Target: Option<double>.None, Distance: plan.Distance)),
    debiased: () =>
```

```csharp
Option<double> sourceBias, Option<double> targetBias, CloudTransportPolicy policy, Op key) {
```

### To

```csharp
// TransportEstimator DELETED
// CloudTransportPolicy.Estimator DELETED
```

```csharp
[ComplexValueObject]
public sealed partial class CloudTransportPolicy {
    public PositiveMagnitude Regularization { get; }
    public Dimension MaxIterations { get; }
    public bool Debias { get; }
    public Option<PositiveMagnitude> MassRelaxation { get; }
    public PositiveMagnitude ConvergenceTolerance { get; }
    public PositiveMagnitude CouplingCutoff { get; }
```

```csharp
static partial void ValidateFactoryArguments(ref ValidationError? error,
    ref PositiveMagnitude regularization, ref Dimension maxIterations, ref bool debias,
    ref Option<PositiveMagnitude> massRelaxation, ref PositiveMagnitude convergenceTolerance,
    ref PositiveMagnitude couplingCutoff) =>
    error = regularization == default || maxIterations == default
        || massRelaxation.Case is PositiveMagnitude relaxation && relaxation == default
        || convergenceTolerance == default || couplingCutoff == default
            ? new ValidationError(message: "CloudTransportPolicy requires admitted positive values.") : null;
```

```csharp
public static Fin<CloudTransportPolicy> Of(double regularization, int maxIterations, Context context,
    bool debias = false, Option<double> massRelaxation = default, Op? key = null) {
    Op op = key.OrDefault();
    return from model in Admit.NotNull(value: context, key: op)
           from reg in op.AcceptValidated<PositiveMagnitude>(regularization)
           from cap in op.AcceptValidated<Dimension>(maxIterations)
           from relax in massRelaxation.TraverseM(value =>
               op.AcceptValidated<PositiveMagnitude>(value)).As()
```

```csharp
           from tolerance in op.AcceptValidated<PositiveMagnitude>(
               model.For(ToleranceLane.Convergence).Value)
           from cutoff in op.AcceptValidated<PositiveMagnitude>(
               model.For(ToleranceLane.Neglect).Value)
           from policy in op.AcceptValidated<CloudTransportPolicy>(
               Validate(reg, cap, debias, relax, tolerance, cutoff,
                   out CloudTransportPolicy? admitted), admitted)
           select policy;
```

```csharp
from bias in active.Debias
    ? from selfS in Solve(src.Vertices, src.Vertices, srcMass, srcMass, active, op)
      from selfT in Solve(tgt.Vertices, tgt.Vertices, tgtMass, tgtMass, active, op)
      from _ in guard(selfS.Stop.Converged && selfT.Stop.Converged,
          op.InvalidResult(detail: "sinkhorn-debias-unconverged")).ToFin()
      select (Evidence: Some((Raw: plan.Distance, Source: selfS.Distance, Target: selfT.Distance)),
          Distance: plan.Distance - (0.5 * selfS.Distance) - (0.5 * selfT.Distance))
    : Fin.Succ((Evidence: Option<(double Raw, double Source, double Target)>.None,
        Distance: plan.Distance))
```

```csharp
Option<(double Raw, double Source, double Target)> bias,
    CloudTransportPolicy policy, Op key) {
```

```csharp
bias: bias.Evidence, policy: active, key: op)
```

### Why

The positional constructor bypasses policy admission, its two tolerance overrides fork the authoritative `Context` lanes, and the payload-free estimator pair only encodes whether the self-transport leg runs. A generated complex value object closes construction, its hook rejects default nested values on every generated factory, and one bool retains the genuine mode while deleting the estimator type and generated roster. One optional bias product makes partial three-solve evidence unrepresentable, and the LanguageExt guard prevents an exhausted self-plan from changing a debiased answer.

### Ripples

- `libs/dotnet/Rasm/.planning/Processing/intent.md:240-245`: replace the value-type default comparison with `Admit.NotNull(value: policy, key: op)` and pass the admitted policy into `TransportCase`.

# 2. Admit cluster cases at the transport boundary

### Location

- `transport.md:114-132`, anchors `CloudTransport.Sinkhorn<TOut>` and the `(source, target) switch`

### From

```csharp
public static Fin<TOut> Sinkhorn<TOut>(VectorCloud source, VectorCloud target, CloudTransportPolicy policy, Op? key = null) {
    Op op = key.OrDefault();
    return (source, target) switch {
        (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt) =>
```

```csharp
        _ => Fin.Fail<TOut>(op.Unsupported(inputType: source.GetType(), outputType: typeof(TOut))),
    };
}
```

### To

```csharp
public static Fin<TOut> Sinkhorn<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target,
    CloudTransportPolicy policy, Op? key = null) {
    Op op = key.OrDefault();
    return from src in Admit.NotNull(value: source, key: op)
           from tgt in Admit.NotNull(value: target, key: op)
           from active in Admit.NotNull(value: policy, key: op)
```

```csharp
           from output in plan.Project<TOut>(source: src, target: tgt, distance: bias.Distance,
               bias: bias.Evidence, policy: active, key: op)
           select output;
}
```

### Why

The kernel solves weighted clusters only. Taking the generated cluster case makes that domain visible in the signature, deletes the catch-all and its unsafe `source.GetType()` null path, and leaves the operation total over admitted inputs. The policy is a generated reference owner after section 1, so its one null gate belongs beside the two cluster gates.

### Ripples

- `libs/dotnet/Rasm/.planning/Processing/intent.md:79,240-245`: make `TransportCase.Source`, `TransportCase.Target`, and `VectorIntent.Transport` take `VectorCloud.ClusterCase`; retain the three `Admit.NotNull` gates at that public construction boundary.

# 3. Replace mirrored status vocabularies with owning facts

### Location

- `transport.md:31-55`, anchors `SinkhornResidualKind`, `SinkhornStopKind`, and `SinkhornNumericStatus`
- `transport.md:94`, anchor `CloudTransportPolicy.ResidualKind`
- `transport.md:286`, anchor `SinkhornSummary.ResidualKind`

### From

```csharp
[SmartEnum<int>]
public sealed partial class SinkhornResidualKind {
    public static readonly SinkhornResidualKind MarginalMass = new(key: 0);
    public static readonly SinkhornResidualKind ScalingChange = new(key: 1);
}
```

```csharp
public SinkhornResidualKind Residual { get; }
public bool Converged { get; }
internal static SinkhornStopKind Of(SinkhornResidualKind residual, bool converged) => residual.Switch(
```

```csharp
[SmartEnum<int>]
public sealed partial class SinkhornNumericStatus {
    public static readonly SinkhornNumericStatus FiniteAccepted = new(key: 0);
    public static readonly SinkhornNumericStatus UnderflowFloored = new(key: 1);
}
```

### To

```csharp
// SinkhornResidualKind DELETED
// SinkhornStopKind DELETED
// SinkhornNumericStatus DELETED
// CloudTransportPolicy.ResidualKind DELETED
// SinkhornSummary.ResidualKind DELETED
```

### Why

`MassRelaxation.IsSome` already selects marginal versus scaling residuals, the terminal fold state owns convergence, and the coupling loop owns whether it floored underflow. The three generated vocabularies only mirror those facts: the stop roster is their Cartesian product and the numeric roster renames a boolean. Keeping `MassRelaxation`, `Converged`, and `UnderflowFloored` preserves the evidence while deleting three public types, eight rows, the `Of` wrapper, and their generated surfaces. No consumer outside the target names them.

# 4. Keep iteration state local

### Location

- `transport.md:63-72`, anchor `public abstract partial record SinkhornStep`
- `transport.md:149-165`, anchors local `Advance` and `Range(...).FoldUntil`

### From

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SinkhornStep {
    private SinkhornStep() { }
    public sealed record Advance(int Iterations, double SourceResidual, double TargetResidual) : SinkhornStep;
    public sealed record Settled(int Iterations, double SourceResidual, double TargetResidual) : SinkhornStep;
```

```csharp
SinkhornStep settled = Range(0, policy.MaxIterations.Value).FoldUntil(
    state: (SinkhornStep)new SinkhornStep.Advance(Iterations: 0, SourceResidual: double.PositiveInfinity, TargetResidual: double.PositiveInfinity),
    f: (_, _) => Advance(),
    stateP: static state => state is SinkhornStep.Settled);
```

### To

```csharp
// SinkhornStep DELETED
```

```csharp
(int Iterations, double Source, double Target, bool Converged) Advance(int iteration) {
```

```csharp
return (Iterations: iteration + 1, Source: s, Target: t,
    Converged: Math.Max(s, t) <= policy.ConvergenceTolerance.Value);
```

```csharp
(int Iterations, double Source, double Target, bool Converged) settled =
    Range(0, policy.MaxIterations.Value).FoldUntil(
    state: (Iterations: 0, Source: double.PositiveInfinity, Target: double.PositiveInfinity, Converged: false),
    f: (_, iteration) => Advance(iteration),
    stateP: static pair => pair.State.Converged);
```

### Why

Both cases carry the same payload and differ only by the predicate terminating this one fold. The local tuple exposes exhaustion as `Converged == false`, removes one public union, two cases, the `Reading` projection, one allocation per iteration, and the captured counter. `pair.State.Converged` also matches LanguageExt's catalogued pure `FoldUntil` predicate shape, `Func<(State, Value), bool>`; the prior one-argument state test does not.

# 5. Nest the plan as an opaque execution capsule

### Location

- `transport.md:135,181-186`, anchors `Fin<SinkhornPlan> Solve` and `Fin.Fail<SinkhornPlan>`
- `transport.md:182-185`, anchor `new SinkhornPlan`
- `transport.md:128-129`, anchor the `plan.Project<TOut>` call
- `transport.md:224-243`, anchors `internal sealed record SinkhornPlan`, `Plane`, and `Project<TOut>`

### From

```csharp
internal sealed record SinkhornPlan(
    double Distance, double[] Coupling, int Rows, int Columns,
    double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations, SinkhornStopKind Stop,
    double ConvergenceTolerance, double CouplingCutoff, bool UnderflowFloored) {
```

```csharp
internal Memory2D<double> Plane => Coupling.AsMemory2D(height: Rows, width: Columns);
SinkhornPlan self = this;
```

```csharp
private static Fin<SinkhornPlan> Solve(Seq<Point3d> source, Seq<Point3d> target, Arr<double> sourceMass, Arr<double> targetMass, CloudTransportPolicy policy, Op key) {
```

```csharp
internal Fin<TOut> Project<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double distance,
    Option<double> sourceBias, Option<double> targetBias, CloudTransportPolicy policy, Op key) {
```

```csharp
: Fin.Fail<SinkhornPlan>(key.InvalidResult());
```

### To

```csharp
// SinkhornPlan DELETED
```

```csharp
private static Fin<Plan> Solve(Seq<Point3d> source, Seq<Point3d> target, Arr<double> sourceMass,
    Arr<double> targetMass, CloudTransportPolicy policy, Op key) {
```

```csharp
private sealed class Plan(
    double distance, double[] coupling, Arr<double> sourceMass, Arr<double> targetMass,
    double sourceResidual, double targetResidual, int iterations,
    bool converged, bool underflowFloored, CloudTransportPolicy policy) {
    double Distance => distance;
    bool Converged => converged;
```

```csharp
return Fin.Succ(new Plan(distance, entries, sourceMass, targetMass,
    settled.Source, settled.Target, settled.Iterations, settled.Converged, floored, policy));
```

```csharp
: Fin.Fail<Plan>(key.InvalidResult());
```

```csharp
Memory2D<double> plane = coupling.AsMemory2D(
    height: sourceMass.Count, width: targetMass.Count);
return ResultProjection.Rows<Plan, TOut>(self: this, key: key, owner: typeof(VectorCloud),
```

```csharp
internal Fin<TOut> Project<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target,
    double distance, Option<(double Raw, double Source, double Target)> bias, Op key) {
```

```csharp
from output in plan.Project<TOut>(src, tgt, bias.Distance, bias.Evidence, op)
```

### Why

The plan is mutable-array-backed execution state, not a structural value. The positional record emits array-reference equality, cloning, deconstruction, and component properties nothing consumes. A private class nested under `CloudTransport` removes the module-level type, keeps its state opaque, retains the admitted policy and marginals once, derives plane dimensions from marginal counts, and deletes the copied rows, columns, cutoff, tolerance, `Plane` wrapper, and one-use `self` alias.

# 6. Measure scaling deltas during each update

### Location

- `transport.md:149-156`, anchors local `Advance`, `prevU`, and `prevV`
- `transport.md:203-206`, anchor `MaxDelta`

### From

```csharp
(double[] prevU, double[] prevV) = ([.. logU], [.. logV]);
for (int i = 0; i < m; i++) logU[i] = exponent * (logA[i] - LogSumExp(row: logK.Span.GetRowSpan(i), shift: logV, scratch: fold.AsSpan(0, n)));
for (int j = 0; j < n; j++) logV[j] = exponent * (logB[j] - LogSumExpColumn(logK: logK.Span, column: j, rows: m, shift: logU, gather: gather.AsSpan(0, m), fold: fold.AsSpan(0, m)));
```

```csharp
private static double MaxDelta(ReadOnlySpan<double> prev, ReadOnlySpan<double> next, Span<double> scratch) {
    TensorPrimitives.Subtract(x: next, y: prev, destination: scratch);
    return Math.Abs(value: TensorPrimitives.MaxMagnitude<double>(x: scratch));
}
```

```csharp
scalingChange: () => (MaxDelta(prev: prevU, next: logU, scratch: fold.AsSpan(0, m)),
    MaxDelta(prev: prevV, next: logV, scratch: fold.AsSpan(0, n))));
```

### To

```csharp
(double deltaU, double deltaV) = (0.0, 0.0);
for (int i = 0; i < m; i++) {
    double next = exponent * (logA[i] - LogSumExp(logK.Span.GetRowSpan(i), logV, fold.AsSpan(0, n)));
    deltaU = Math.Max(deltaU, Math.Abs(next - logU[i]));
    logU[i] = next;
}
```

```csharp
for (int j = 0; j < n; j++) {
    double next = exponent * (logB[j] - LogSumExpColumn(logK.Span, j, logU,
        gather.AsSpan(0, m), fold.AsSpan(0, m)));
    deltaV = Math.Max(deltaV, Math.Abs(next - logV[j]));
    logV[j] = next;
}
```

```csharp
scalingChange: () => (deltaU, deltaV));
```

```csharp
// CloudTransport.MaxDelta DELETED
```

### Why

Each snapshot exists only so `MaxDelta` can compare it with the array immediately after its in-place update. Measuring the same delta while the old element is still present removes two arrays and two full vector passes per iteration and deletes a one-call module member.

# 7. Localize the numeric kernel

### Location

- `transport.md:110-112`, anchors `MinPositiveNormal` and `LogUnderflowFloor`
- `transport.md:168-178`, anchors `rowFloored` and the coupling floor
- `transport.md:189-201`, anchors `LogSumExp` and `LogSumExpColumn`

### From

```csharp
internal static readonly double MinPositiveNormal = BitConverter.UInt64BitsToDouble(0x0010_0000_0000_0000UL);
internal static readonly double LogUnderflowFloor = Math.Log(d: MinPositiveNormal);
```

```csharp
bool rowFloored = TensorPrimitives.Min<double>(x: row) < LogUnderflowFloor;
TensorPrimitives.Exp<double>(x: row, destination: row);
if (rowFloored) {
    floored = true;
    for (int j = 0; j < n; j++) if (row[j] < MinPositiveNormal) row[j] = 0.0;
}
```

```csharp
private static double LogSumExp(ReadOnlySpan<double> row, ReadOnlySpan<double> shift, Span<double> scratch) {
    TensorPrimitives.Add(x: row, y: shift, destination: scratch);
    double max = TensorPrimitives.Max<double>(x: scratch);
    if (double.IsNegativeInfinity(d: max)) return double.NegativeInfinity;
    TensorPrimitives.Subtract(x: scratch, y: max, destination: scratch);
    TensorPrimitives.Exp<double>(x: scratch, destination: scratch);
    return max + Math.Log(d: TensorPrimitives.Sum<double>(x: scratch));
}
```

```csharp
private static double LogSumExpColumn(Span2D<double> logK, int column, int rows,
    ReadOnlySpan<double> shift, Span<double> gather, Span<double> fold) {
    for (int i = 0; i < rows; i++) gather[i] = logK.GetRowSpan(i)[column];
    return LogSumExp(row: gather, shift: shift, scratch: fold);
}
```

### To

```csharp
double minPositiveNormal = BitConverter.UInt64BitsToDouble(0x0010_0000_0000_0000UL);
double logUnderflowFloor = Math.Log(minPositiveNormal);
```

```csharp
bool rowFloored = TensorPrimitives.Min<double>(row) < logUnderflowFloor;
TensorPrimitives.Exp<double>(row, row);
if (rowFloored) {
    floored = true;
    for (int j = 0; j < n; j++) if (row[j] < minPositiveNormal) row[j] = 0.0;
}
```

```csharp
double LogSumExp(ReadOnlySpan<double> row, ReadOnlySpan<double> shift, Span<double> scratch) {
    TensorPrimitives.Add(row, shift, scratch);
    double max = TensorPrimitives.Max<double>(scratch);
    if (double.IsNegativeInfinity(max)) return double.NegativeInfinity;
    TensorPrimitives.Subtract(scratch, max, scratch);
    TensorPrimitives.Exp<double>(scratch, scratch);
    return max + Math.Log(TensorPrimitives.Sum<double>(scratch));
}
```

```csharp
double LogSumExpColumn(Span2D<double> kernel, int column,
    ReadOnlySpan<double> shift, Span<double> gather, Span<double> scratch) {
    kernel.GetColumn(column).CopyTo(gather);
    return LogSumExp(gather, shift, scratch);
}
```

```csharp
// CloudTransport.MinPositiveNormal DELETED
// CloudTransport.LogUnderflowFloor DELETED
// CloudTransport.LogSumExp DELETED
// CloudTransport.LogSumExpColumn DELETED
```

### Why

The two anchors and both LSE helpers are used only by `Solve`, so local functions and locals remove four module members without duplicating logic. `Span2D<T>.GetColumn(...).CopyTo(Span<T>)` is the catalogued strided gather, deleting the hand-written row walk and redundant `rows` parameter while preserving the contiguous bridge `TensorPrimitives` requires.

# 8. Inline the marginal residual fold

### Location

- `transport.md:154-156`, anchor the `marginalMass` residual arm
- `transport.md:208-220`, anchor `MarginalResiduals`

### From

```csharp
(double s, double t) = policy.ResidualKind.Switch<(double Source, double Target)>(
    marginalMass: () => MarginalResiduals(logK: logK, logU: logU, logV: logV,
        a: sourceMass, b: targetMass, m: m, n: n, gather: gather, fold: fold),
```

```csharp
private static (double Source, double Target) MarginalResiduals(
    Memory2D<double> logK, double[] logU, double[] logV, Arr<double> a, Arr<double> b,
    int m, int n, double[] gather, double[] fold) {
```

### To

```csharp
(double s, double t) = (deltaU, deltaV);
if (policy.MassRelaxation.IsNone) {
    (s, t) = (0.0, 0.0);
    for (int i = 0; i < m; i++) {
        double log = logU[i] + LogSumExp(logK.Span.GetRowSpan(i), logV, fold.AsSpan(0, n));
        s = Math.Max(s, Math.Abs((log < logUnderflowFloor ? 0.0 : Math.Exp(log)) - sourceMass[i]));
    }
```

```csharp
    for (int j = 0; j < n; j++) {
        double log = logV[j] + LogSumExpColumn(logK.Span, j, logU,
            gather.AsSpan(0, m), fold.AsSpan(0, m));
        t = Math.Max(t, Math.Abs((log < logUnderflowFloor ? 0.0 : Math.Exp(log)) - targetMass[j]));
    }
}
```

```csharp
// CloudTransport.MarginalResiduals DELETED
```

### Why

The helper has one caller, depends on nearly every local solve buffer, and owns no independent capability. Its two loops belong in `Advance`; absence of relaxation selects them directly, while relaxation keeps the deltas already measured during the update. This removes one module member and a nine-argument forwarding seam.

# 9. Mint correspondence sets on their owner

### Location

- `transport.md:239-240`, anchor the correspondence projection row
- `transport.md:252-253`, anchor summary correspondence construction
- `transport.md:344-387`, anchors `Correspondences.OfCoupling` and `Correspondences.Fold`

### From

```csharp
internal static class Correspondences {
    internal static Fin<CloudCorrespondenceSet> OfCoupling(VectorCloud.ClusterCase source,
        VectorCloud.ClusterCase target, Memory2D<double> coupling, double cutoff, Op key) =>
        from sourceMass in CloudKernel.MassOf(cluster: source, key: key)
        from targetMass in CloudKernel.MassOf(cluster: target, key: key)
```

```csharp
private static Fin<CloudCorrespondenceSet> Fold(VectorCloud.ClusterCase source,
    VectorCloud.ClusterCase target, Memory2D<double> coupling, double cutoff,
    Arr<double> a, Arr<double> b, Op key) {
    Seq<CloudCorrespondence> items = default;
```

```csharp
ProjectionRow.Of<CloudCorrespondenceSet>(() => Settled(() => Correspondences.OfCoupling(source: source, target: target,
    coupling: self.Plane, cutoff: self.CouplingCutoff, key: key))),
```

```csharp
from pairs in Correspondences.OfCoupling(source: source, target: target, coupling: Plane,
    cutoff: CouplingCutoff, key: key)
```

### To

```csharp
internal static Fin<CloudCorrespondenceSet> OfCoupling(VectorCloud.ClusterCase source,
    VectorCloud.ClusterCase target, Memory2D<double> coupling, double cutoff,
    Arr<double> sourceMass, Arr<double> targetMass, Op key) {
    (int rows, int columns) = (coupling.Height, coupling.Width);
    List<CloudCorrespondence> items = [];
```

```csharp
items.Add(new CloudCorrespondence(SourceIndex: i, TargetIndex: j,
    SourcePoint: sp, TargetPoint: tp, Residual: tp - sp,
    Distance: distance, SquaredDistance: squared,
    SourceMass: Some(sourceMass[i]), TargetMass: Some(targetMass[j]), CouplingMass: Some(pi),
    Confidence: denominator > cutoff
        ? Some(Math.Min(1.0, pi / denominator)) : Option<double>.None));
```

```csharp
ProjectionRow.Of<CloudCorrespondenceSet>(() => Settled(() => CloudCorrespondenceSet.OfCoupling(
    source, target, plane, policy.CouplingCutoff.Value, sourceMass, targetMass, key))),
```

```csharp
from pairs in CloudCorrespondenceSet.OfCoupling(source, target, plane,
    policy.CouplingCutoff.Value, sourceMass, targetMass, key)
```

```csharp
// Correspondences DELETED
// Correspondences.Fold DELETED
```

### Why

The helper type only constructs `CloudCorrespondenceSet`, and its private fold has exactly one caller. Seating that factory on the constructed owner removes a module type and a one-call member. Passing the plan's admitted marginals also deletes two repeated `CloudKernel.MassOf` normalizations per projection, while a local `List` materializes the public `Seq` once after the dense walk instead of replacing a persistent sequence per retained cell.

# 10. Remove absence from always-present masses

### Location

- `transport.md:319-323`, anchor `CloudCorrespondence`
- `transport.md:367-370`, anchor its coupling construction

### From

```csharp
Option<double> SourceMass, Option<double> TargetMass,
Option<double> CouplingMass, Option<double> Confidence);
```

```csharp
SourceMass: Some(a[i]), TargetMass: Some(b[j]), CouplingMass: Some(pi),
```

### To

```csharp
double SourceMass, double TargetMass, double CouplingMass, Option<double> Confidence);
```

```csharp
SourceMass: sourceMass[i], TargetMass: targetMass[j], CouplingMass: pi,
```

### Why

Both construction paths always supply all three masses, and no absence arm exists or is consumed. Bare admitted values remove three unconditional `Some` wrappers from every correspondence while retaining `Option<double> Confidence`, whose absence is genuine on nearest-neighbor registration rows.

### Ripples

- `libs/dotnet/Rasm/.planning/Processing/register.md:329-331`: pass `sourceMass[row]`, `targetMass[nearest]`, and `sourceMass[row]` directly; retain `Confidence: Option<double>.None`.

# 11. Delete derived correspondence geometry columns

### Location

- `transport.md:319-323`, anchor `CloudCorrespondence`
- `transport.md:364-370`, anchor correspondence construction

### From

```csharp
int SourceIndex, int TargetIndex, Point3d SourcePoint, Point3d TargetPoint, Vector3d Residual,
double Distance, double SquaredDistance,
```

```csharp
Residual: tp - sp, Distance: distance, SquaredDistance: squared,
```

### To

```csharp
public readonly record struct CloudCorrespondence(
    int SourceIndex, int TargetIndex, Point3d SourcePoint, Point3d TargetPoint,
    double SourceMass, double TargetMass, double CouplingMass, Option<double> Confidence);
```

```csharp
// CloudCorrespondence.Residual DELETED
// CloudCorrespondence.Distance DELETED
// CloudCorrespondence.SquaredDistance DELETED
```

### Why

Residual, squared distance, and distance are pure derivations of the two retained endpoints, not independent correspondence facts. Both producing folds already hold the local distance used by their statistics. Deleting the three convenience columns removes public symbols and contradictory constructor states without removing source or target geometry.

### Ripples

- `libs/dotnet/Rasm/.planning/Processing/register.md:323-331`: remove the three constructor arguments, replace the now-single-use `residual` local with `double squared = transformed[i].DistanceToSquared(other: targetPoint)`, and retain the `distances` array consumed by the registration solve.

# 12. Consolidate correspondence measurements

### Location

- `transport.md:325-340`, anchors `CloudCorrespondenceSet` and `IsValid`
- `transport.md:356-385`, anchors `total`, `weightedSquared`, and set construction

### From

```csharp
Seq<CloudCorrespondence> Items, int SourceCount, int TargetCount, int NonZeroCount,
double TotalMass, Option<double> Rmse, Option<Distribution<Scalar>> Distances,
```

```csharp
(double total, double weightedSquared) = (0.0, 0.0);
```

```csharp
(total, weightedSquared) = (total + pi, weightedSquared + (pi * squared));
```

```csharp
Rmse: spread.Map(census => total > cutoff ? Math.Sqrt(weightedSquared / total) : census.Summary.Rms),
```

### To

```csharp
Seq<CloudCorrespondence> Items, int SourceCount, int TargetCount,
Option<(Stat<Scalar> Coupling, Stat<Scalar> WeightedDistance,
    Distribution<Scalar> Distance)> Measurements,
```

```csharp
List<Scalar> distances = [];
List<double> weights = [];
```

```csharp
distances.Add((Scalar)distance);
weights.Add(pi);
// Correspondences.Fold.total DELETED
// Correspondences.Fold.weightedSquared DELETED
```

```csharp
Seq<double> mass = toSeq(weights); Seq<Scalar> samples = toSeq(distances);
Fin<Option<(Stat<Scalar> Coupling, Stat<Scalar> WeightedDistance,
    Distribution<Scalar> Distance)>> measured = samples.IsEmpty
    ? Fin.Succ(Option<(Stat<Scalar>, Stat<Scalar>, Distribution<Scalar>)>.None)
    : from coupling in Stat<Scalar>.Of(mass.Map(static value => (Scalar)value), key)
      from weighted in Stat<Scalar>.Of(samples, key, Some(mass))
      from spread in Distribution<Scalar>.Of(samples, Seq(90.0, 95.0), key,
          Some(QuantileRule.Interpolated))
      select Some((Coupling: coupling, WeightedDistance: weighted, Distance: spread));
```

```csharp
return from measurements in measured
       from set in key.AcceptValue(new CloudCorrespondenceSet(
           toSeq(items), rows, columns, measurements,
           coveredSource.Count, coveredTarget.Count,
           coveredSource.Fold(0.0, (held, i) => held + sourceMass[i]),
           coveredTarget.Fold(0.0, (held, j) => held + targetMass[j])))
       select set;
```

```csharp
public bool IsValid => ValidityClaim.All(
    Measurements.IsSome == !Items.IsEmpty,
    Measurements.Map(e => e.Coupling.Count == Items.Count
        && e.WeightedDistance.Count == Items.Count && e.Distance.Summary.Count == Items.Count
        && e.Coupling.IsValid && e.WeightedDistance.IsValid && e.Distance.IsValid).IfNone(true),
    CoveredSourceCount >= 0 && CoveredSourceCount <= SourceCount,
    CoveredTargetCount >= 0 && CoveredTargetCount <= TargetCount,
    ValidityClaim.Nonnegative(RetainedSourceMass), ValidityClaim.Nonnegative(RetainedTargetMass));
```

```csharp
// CloudCorrespondenceSet.NonZeroCount DELETED
// CloudCorrespondenceSet.TotalMass DELETED
// CloudCorrespondenceSet.Rmse DELETED
// CloudCorrespondenceSet.Distances DELETED
```

### Why

The current set flattens count, total coupling mass, and weighted RMS beside the statistics owners, while `SinkhornSummary` separately re-walks coupling values. One optional product makes empty versus measured correspondence evidence explicit: `Coupling` preserves the coupling-entry statistics, `WeightedDistance` owns total mass and weighted RMS, and `Distance` owns unweighted order statistics. This deletes four flattened fields plus the summary's duplicate statistics slot without turning a lawful all-pruned transport result into a failure.

### Ripples

- `libs/dotnet/Rasm/.planning/Processing/register.md:340-351`: retain the existing weighted `Stat<Scalar>` and `Distribution<Scalar>`, add `Stat<Scalar>.Of` over `rowMass` for the coupling column, wrap the three as `Some((coupling, weighted, spread))`, and remove the flattened constructor arguments.

# 13. Inline and compact the summary projection

### Location

- `transport.md:237-238`, anchor the `ProjectionRow.Of<SinkhornSummary>` row
- `transport.md:246-263`, anchor `SinkhornPlan.SummaryOf`
- `transport.md:279-305`, anchors `SinkhornSummary`, forwarding properties, and `IsValid`

### From

```csharp
double Distance, Option<double> RawDistance, Option<double> SourceBiasDistance, Option<double> TargetBiasDistance,
double Regularization, Option<double> MassRelaxation, double ConvergenceTolerance, double CouplingCutoff,
TransportEstimator Estimator, SinkhornNumericStatus NumericStatus,
```

```csharp
Option<Stat<Scalar>> Coupling, CloudCorrespondenceSet Correspondences) : IValidityEvidence {
    public int NonZeroCouplings => Coupling.Map(static census => census.Count).IfNone(0);
    public Option<double> CouplingMass => Coupling.Map(static census => census.Mean * census.Count);
```

```csharp
internal Fin<SinkhornSummary> SummaryOf(VectorCloud.ClusterCase source,
    VectorCloud.ClusterCase target, double distance,
    Option<double> sourceBias, Option<double> targetBias,
    CloudTransportPolicy policy, Op key) {
```

### To

```csharp
public readonly record struct SinkhornSummary(
    double Distance, Option<(double Raw, double Source, double Target)> Bias,
    CloudTransportPolicy Policy, bool Converged, bool UnderflowFloored,
    double SourceConvergenceResidual, double TargetConvergenceResidual, int Iterations,
    CloudCorrespondenceSet Correspondences) : IValidityEvidence {
```

```csharp
ProjectionRow.Of<SinkhornSummary>(() =>
    from pairs in CloudCorrespondenceSet.OfCoupling(source, target, plane,
        policy.CouplingCutoff.Value, sourceMass, targetMass, key)
    from summary in key.AcceptValue(new SinkhornSummary(distance, bias, policy,
        converged, underflowFloored, sourceResidual, targetResidual, iterations, pairs))
    select summary),
```

```csharp
public bool IsValid => Policy is { } policy && ValidityClaim.All(
    ValidityClaim.Finite(Distance),
    Bias.Map(static held => ValidityClaim.All(
        ValidityClaim.Nonnegative(held.Raw), ValidityClaim.Nonnegative(held.Source),
        ValidityClaim.Nonnegative(held.Target))).IfNone(!policy.Debias),
    policy.Debias == Bias.IsSome,
    ValidityClaim.Nonnegative(SourceConvergenceResidual),
    ValidityClaim.Nonnegative(TargetConvergenceResidual),
    Iterations >= 1 && Iterations <= policy.MaxIterations.Value,
    ValidityClaim.Evidence(Some(Correspondences)));
```

```csharp
// SinkhornPlan.SummaryOf DELETED
// SinkhornSummary.RawDistance DELETED
// SinkhornSummary.SourceBiasDistance DELETED
// SinkhornSummary.TargetBiasDistance DELETED
// SinkhornSummary.Regularization DELETED
// SinkhornSummary.MassRelaxation DELETED
// SinkhornSummary.ConvergenceTolerance DELETED
// SinkhornSummary.CouplingCutoff DELETED
// SinkhornSummary.Estimator DELETED
// SinkhornSummary.NumericStatus DELETED
// SinkhornSummary.Stop DELETED
// SinkhornSummary.Coupling DELETED
// SinkhornSummary.NonZeroCouplings DELETED
// SinkhornSummary.CouplingMass DELETED
```

### Why

The summary flattens one admitted policy, stores three independently optional values that are one bias product, and duplicates coupling statistics now owned by `Correspondences.Measurements`. Keeping the policy intact and carrying bias evidence as one option removes the primitive mirrors and forwarding members. Convergence remains the fold's evidence instead of being re-derived during validation, preserving the page's single-authority law. Inlining the one-call `SummaryOf` body into its lazy row also deletes a member and its forwarding seam.

# 14. Inline and materialize the barycentric projection

### Location

- `transport.md:243`, anchor the `ProjectionRow.Of<VectorCloud>` row
- `transport.md:265-275`, anchor `SinkhornPlan.BarycentricImage`

### From

```csharp
ProjectionRow.Of<VectorCloud>(() => Settled(() =>
    self.BarycentricImage(target: target, key: key))));
```

```csharp
internal Fin<VectorCloud> BarycentricImage(VectorCloud.ClusterCase target, Op key) {
    Seq<Point3d> image = default;
```

```csharp
if (mass > CouplingCutoff) image = image.Add(new Point3d(weighted / mass));
```

### To

```csharp
ProjectionRow.Of<VectorCloud>(() => Settled(() => {
    List<Point3d> image = [];
    for (int i = 0; i < sourceMass.Count; i++) {
        ReadOnlySpan<double> row = plane.Span.GetRowSpan(i);
```

```csharp
if (mass > policy.CouplingCutoff.Value) image.Add(new Point3d(weighted / mass));
```

```csharp
return VectorCloud.Cluster(points: toSeq(image), context: target.Tolerance, key: key);
})),
```

```csharp
// SinkhornPlan.BarycentricImage DELETED
```

### Why

`BarycentricImage` has one caller and exists only as one projection row. Moving its loop into that lazy row deletes a member without making the work eager. A local `List<Point3d>` materializes the public `Seq<Point3d>` once at the boundary instead of replacing a persistent sequence inside the dense row loop.

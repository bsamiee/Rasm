# 1. Give Runge–Kutta methods canonical keys

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:45`
```csharp
[SmartEnum<int>]
public sealed partial class RungeKuttaMethod {
    public static readonly RungeKuttaMethod Euler = Of(key: 0, order: 1, coupling: [[]], weights: [1.0]);
    public static readonly RungeKuttaMethod Heun = Of(key: 1, order: 2, coupling: [[], [1.0]], weights: [0.5, 0.5]);
```

To:
```csharp
[SmartEnum<string>]
public sealed partial class RungeKuttaMethod {
    public static readonly RungeKuttaMethod Euler = Of(key: "euler", order: 1, coupling: [[]], weights: [1.0]);
    public static readonly RungeKuttaMethod Heun = Of(key: "heun", order: 2, coupling: [[], [1.0]], weights: [0.5, 0.5]);
```

Why: `Key` is consumed as the method's textual identity, while arbitrary ordinals add a translation problem and `Of` currently discards its `key` argument instead of initializing the generated smart-enum key.

Change: Change the smart-enum key and `Of` parameter to `string`, assign canonical lower-kebab-case keys to all nine rows, and pass `key` to the generated constructor.

Delta: 0 LOC; 0 module-level symbols, members, or types; nine arbitrary ordinal identities and one discarded parameter removed.

# 2. Admit built-in tableaus at static construction

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:75`
```csharp
internal ButcherTableau Tableau { get; }
internal ButcherTableau.OrderConditions Conditions { get; }
internal DenseFormula Formula { get; }
private static RungeKuttaMethod Of(
    int key, int order, double[][] coupling, double[] weights,
    Option<(double[] Weights, int Order)> embedded = default) {
    ButcherTableau tableau = ButcherTableau.Of(
        toSeq(coupling.Select(static row => toSeq(row))), toSeq(weights),
        embedded.Map(static pair => (toSeq(pair.Weights), pair.Order)), order);
    ButcherTableau.OrderConditions conditions = tableau.ConditionsOf(tableau.Weights, order);
    return new(tableau: tableau, conditions: conditions,
        formula: DenseFormula.Identify(tableau));
}
```

To:
```csharp
internal ButcherTableau Tableau { get; }
private static RungeKuttaMethod Of(
    string key, int order, double[][] coupling, double[] weights,
    Option<(double[] Weights, int Order)> embedded = default) {
    ButcherTableau candidate = ButcherTableau.Of(
        toSeq(coupling.Select(static row => toSeq(row))), toSeq(weights),
        embedded.Map(static pair => (toSeq(pair.Weights), pair.Order)), order);
    ButcherTableau tableau = candidate
        .Admit(candidate.ConditionsOf(candidate.Weights, order))
        .ThrowIfFail();
    return new(key: key, tableau: tableau);
}
```

Why: These tableaus are static package data. Carrying their proof beside them and rerunning admission in both integrator factories duplicates work and permits the stored tableau and proof to diverge.

Change: Collapse invalid static data once in `RungeKuttaMethod.Of`, remove `RungeKuttaMethod.Conditions`, remove `ButcherTableau : IValidityEvidence` and `IsValid`, and delete both factory calls to `Tableau.Admit`.

Delta: -5 LOC; -2 module-level members; 0 types.

# 3. Preserve double-double tableau residuals through subtraction

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:155`
```csharp
foreach (RootedTree tree in RootedTree.OfOrder(p)) {
    ddouble[] phi = tree.Weight(aWide, StageCount);
    ddouble lhs = 0.0;
    for (int i = 0; i < StageCount; i++) lhs += b[i] * phi[i];
    double residual = Math.Abs((double)lhs - (1.0 / tree.Density));
```

To:
```csharp
foreach (RootedTree tree in RootedTree.OfOrder(p)) {
    ddouble[] phi = tree.Weight(aWide, StageCount);
    ddouble lhs = 0.0;
    for (int i = 0; i < StageCount; i++) lhs += b[i] * phi[i];
    double residual = (double)ddouble.Abs(lhs - ((ddouble)1.0 / tree.Density));
```

Why: Narrowing the elementary weight before subtraction discards the extra significand that the rooted-tree calculation uses.

Change: Form the target and residual in `ddouble`, then narrow only the nonnegative residual evidence.

Delta: 0 LOC; 0 module-level symbols, members, or types; one premature precision collapse removed.

# 4. Put continuous-extension data on its Runge–Kutta row

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:258`
```csharp
[SmartEnum]
internal sealed partial class DenseFormula {
    public static readonly DenseFormula GenericMomentFit = new(fixedDenseOrder: 0, published: None);
    public static readonly DenseFormula DormandPrinceShampine = new(fixedDenseOrder: 4, published: Some((Fingerprint: DormandPrinceAbscissae, Table: DormandPrinceTable)));
    public static readonly DenseFormula BogackiShampine = new(fixedDenseOrder: 3, published: Some((Fingerprint: BogackiShampineAbscissae, Table: BogackiShampineTable)));
    internal int FixedDenseOrder { get; }
    internal Option<(double[] Fingerprint, double[][] Table)> Published { get; }
```

To:
```csharp
// DenseFormula DELETED
```

Why: `RungeKuttaMethod` already owns method identity. A second smart enum, copied abscissa fingerprints, and a scan to rediscover that identity are duplicate discrimination; the generic row represents only absence of published coefficients.

Change: Add `Option<(int Order, double[][] Coefficients)> ContinuousExtension` to `RungeKuttaMethod`, place the Bogacki–Shampine and Dormand–Prince coefficient tables on their method rows, default the other rows to `None`, move Horner evaluation into `DenseOutput`, and delete `DenseFormula`, its rows, fingerprints, `Identify`, `WeightsAt`, and `DerivativeAt`.

Delta: -14 LOC; -9 net module-level members; -1 module-level type.

# 5. Solve all generic dense-output basis columns together

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:428`
```csharp
internal static Fin<DenseInterpolant> Interpolant(ButcherTableau tableau, DenseFormula family) {
    int order = DenseOrderFor(family: family, tableau: tableau);
    if (family.Published.IsSome) { return Fin.Succ(new DenseInterpolant(Order: order, Basis: [], Solve: None)); }
    int stages = tableau.StageCount;
    double[] design = MomentDesign(tableau: tableau, stages: stages, order: order);
    return Range(0, order).ToSeq()
        .TraverseM(moment => BasisColumn(tableau: tableau, design: design, stages: stages, order: order, moment: moment)).As()
        .Map(columns => new DenseInterpolant(
            Order: order,
            Basis: [.. columns.Map(static column => column.Correction)],
            Solve: columns.Last.Map(static column => column.Solve)));
}
```

To:
```csharp
internal static Fin<DenseInterpolant> Interpolant(
    ButcherTableau tableau,
    Option<(int Order, double[][] Coefficients)> extension) {
    int order = DenseOrderFor(tableau, extension);
    if (extension.IsSome) return Fin.Succ(new DenseInterpolant(order, []));
    return Try.lift(() => {
        int stages = tableau.StageCount;
        int[] anchors = [.. DistinctAnchors(tableau).Take(order)];
        var design = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(
            stages, order, (stage, power) => Math.Pow(tableau.Abscissae[stage], power));
        var vandermonde = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(
            order, order, (power, column) => Math.Pow(tableau.Abscissae[anchors[column]], power));
        var identity = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.DenseIdentity(order);
        var preimages = vandermonde.LU().Solve(identity);
        var samples = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(stages, order);
        for (int column = 0; column < order; column++)
            for (int row = 0; row < order; row++) samples[anchors[row], column] = preimages[row, column];
        var correction = design.Multiply(design.QR(
            MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Thin).Solve(samples));
        return new DenseInterpolant(order,
            [.. Enumerable.Range(0, order).Select(column => correction.Column(column).ToArray())]);
    }).Run();
}
```

Why: The current path factorizes the same Vandermonde and design matrices once per moment, adapts them through package-local matrix wrappers, hand-multiplies each result, and retains one arbitrary final `LinearSolution`. MathNet directly supports multi-right-hand-side LU, thin QR, and matrix multiplication.

Change: Build each matrix once, solve all unit right-hand sides together, solve all correction columns with one thin QR, retain only the basis columns, reduce `DenseInterpolant` to `(Order, Basis)`, and delete `BasisColumn`, `DesignProduct`, `MomentDesign`, and `MomentPreimage`.

Delta: -32 LOC; -5 module-level members; 0 types; the `LinearSolution`, `Dimension`, `Span2D`, tensor-dot, and per-column factorization paths leave this concern.

# 6. Correct the generic dense-output polynomial exponent

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:414`
```csharp
internal readonly record struct DenseInterpolant(int Order, ImmutableArray<double[]> Basis, Option<LinearSolution> Solve) {
    internal double[] At(double theta, int stages) {
        double endpointScale = theta * (1.0 - theta);
        double[] correction = new double[stages];
        double power = theta;
        for (int m = 0; m < Order; m++) {
            power *= theta;
            double coefficient = (power - theta) / ((m + 1.0) * endpointScale);
```

To:
```csharp
internal readonly record struct DenseInterpolant(int Order, ImmutableArray<double[]> Basis) {
    internal double[] At(double theta, int stages) {
        double endpointScale = theta * (1.0 - theta);
        double[] correction = new double[stages];
        double power = 1.0;
        for (int m = 0; m < Order; m++) {
            power *= theta;
            double coefficient = (power - theta) / ((m + 1.0) * endpointScale);
```

Why: The basis formula requires `theta^(m+1)`, but seeding `power` with `theta` and multiplying before use produces `theta^(m+2)`. Even Euler therefore yields a quarter-step weight at `theta = 0.5` instead of a half-step weight.

Change: Seed the recurrence with one so the first multiplication produces `theta`; remove the already-obsolete `Solve` field from the interpolant.

Delta: 0 LOC; -1 module-level member; 0 types; one off-by-one exponent removed.

# 7. Fold endpoint residuals at their sole consumer

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:305`
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct EndpointResiduals {
    internal EndpointResiduals(double valueLeft, double valueRight, Option<(double Left, double Right)> derivatives, double coefficient) =>
        (ValueLeft, ValueRight, Derivatives, Coefficient) =
            (Math.Abs(value: valueLeft), Math.Abs(value: valueRight),
             derivatives.Map(static pair => (Left: Math.Abs(value: pair.Left), Right: Math.Abs(value: pair.Right))), Math.Abs(value: coefficient));
    public double ValueLeft { get; }
    public double ValueRight { get; }
    public Option<(double Left, double Right)> Derivatives { get; }
    public double Coefficient { get; }
    public bool WithinTolerance(double tolerance) =>
        ValueLeft <= tolerance && ValueRight <= tolerance && Coefficient <= tolerance
        && Derivatives.Map(pair => pair.Left <= tolerance && pair.Right <= tolerance).IfNone(noneValue: true);
}
```

To:
```csharp
// EndpointResiduals DELETED
```

Why: Endpoint residuals have one producer and one immediate reduction. Naming and publishing every intermediate residual preserves no capability after the condition check.

Change: Make `EndpointEvidence` return only `(Failed, MaxResidual)`, take absolute values in that fold, and delete `EndpointResiduals` and `WithinTolerance`.

Delta: -10 LOC; -6 module-level members; -1 module-level type.

# 8. Keep only irreducible dense-output evidence

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:320`
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct DenseConditions(int StageCount, int MethodOrder, int DenseOrder, int CheckedConditionCount, int FailedConditionCount, double MaxResidual, EndpointResiduals Endpoints, Option<LinearSolution> CorrectionSolve = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        StageCount >= 1 && MethodOrder >= 1 && DenseOrder >= 0,
        DenseOrder <= MethodOrder,
        CheckedConditionCount >= 3,
        ValidityClaim.CountExactly(count: FailedConditionCount, expected: 0),
        ValidityClaim.Nonnegative(value: MaxResidual),
        MaxResidual <= ButcherTableau.CoefficientTolerance,
        Endpoints.WithinTolerance(ButcherTableau.CoefficientTolerance),
        Endpoints.Derivatives.IsSome ^ CorrectionSolve.IsSome,
        CorrectionSolve.Map(static solve => solve.IsValid).IfNone(noneValue: true));
}
```

To:
```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct DenseConditions(int DenseOrder, double MaxResidual) : IValidityEvidence {
    public bool IsValid => DenseOrder >= 0
        && double.IsFinite(MaxResidual)
        && MaxResidual is >= 0.0 and <= ButcherTableau.CoefficientTolerance;
}
```

Why: Stage count and method order are tableau facts, probe count is fixed by the algorithm, a successful result has no failed conditions, and factorization evidence is consumed before coefficients enter the interpolant. Storing those derivable facts permits contradictory evidence.

Change: Reduce `DenseConditions` to the derived dense order and maximum residual and keep failed-count rejection inside `Conditions` before construction.

Delta: -6 LOC; -6 module-level members; 0 types.

Ripples: `libs/dotnet/Rasm/.planning/Processing/flow.md` updates its `DenseConditions` validity check and trace payload to the reduced two-field evidence shape.

# 9. Accumulate independent dense-output probes applicatively

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:336`
```csharp
return ProbeAt(family: family, tableau: tableau, interpolant: interpolant, order: order, theta: 0.0).Bind(zero =>
    ProbeAt(family: family, tableau: tableau, interpolant: interpolant, order: order, theta: 0.5).Bind(mid =>
        ProbeAt(family: family, tableau: tableau, interpolant: interpolant, order: order, theta: 1.0).Bind(one =>
            EndpointEvidence(family: family, tableau: tableau, interpolant: interpolant, order: order).Bind(evidence => {
                DenseConditions conditions = new(
                    StageCount: tableau.StageCount, MethodOrder: tableau.MethodOrder, DenseOrder: order,
                    CheckedConditionCount: zero.CheckedConditionCount + mid.CheckedConditionCount + one.CheckedConditionCount,
                    FailedConditionCount: zero.FailedConditionCount + mid.FailedConditionCount + one.FailedConditionCount,
                    MaxResidual: Math.Max(val1: zero.MaxResidual, val2: Math.Max(val1: mid.MaxResidual, val2: one.MaxResidual)),
                    Endpoints: evidence, CorrectionSolve: mid.CorrectionSolve);
                return conditions.IsValid ? Fin.Succ(conditions) : Fin.Fail<DenseConditions>(new KernelFault.InvalidResult());
            }))));
```

To:
```csharp
return (
    ProbeAt(tableau, extension, interpolant, order, 0.0).ToValidation(),
    ProbeAt(tableau, extension, interpolant, order, 0.5).ToValidation(),
    ProbeAt(tableau, extension, interpolant, order, 1.0).ToValidation(),
    EndpointEvidence(tableau, extension).ToValidation())
    .Apply((zero, mid, one, endpoint) => (
        Failed: zero.Failed + mid.Failed + one.Failed + endpoint.Failed,
        MaxResidual: Math.Max(endpoint.MaxResidual,
            Math.Max(zero.MaxResidual, Math.Max(mid.MaxResidual, one.MaxResidual)))))
    .As().ToFin()
    .Bind(result => {
        DenseConditions conditions = new(order, result.MaxResidual);
        return result.Failed == 0 && conditions.IsValid
            ? Fin.Succ(conditions)
            : Fin.Fail<DenseConditions>(new KernelFault.InvalidResult());
    });
```

Why: The three theta probes and endpoint measurement consume the same admitted inputs but do not consume one another. Nested `Bind` invents dependency and discards later failures.

Change: Combine the four `Validation<Error, _>` values with `Apply`, reduce each probe to `(Failed, MaxResidual)`, collapse once to `Fin`, and delete `ThetaProbe`.

Delta: -5 LOC; -4 module-level members; -1 nested type.

# 10. Store dense output on the method and collapse duplicate integrator cases

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:621`
```csharp
[Union]
public abstract partial record RungeKuttaIntegrator {
    public sealed record FixedCase : RungeKuttaIntegrator {
        internal FixedCase(RungeKuttaMethod method, DenseConditions dense, DenseOutput.DenseInterpolant interp) { Method = method; Dense = dense; Interp = interp; }
        public override RungeKuttaMethod Method { get; }
        internal override DenseConditions Dense { get; }
        internal override DenseOutput.DenseInterpolant Interp { get; }
    }
    public sealed record AdaptiveCase : RungeKuttaIntegrator {
        internal AdaptiveCase(RungeKuttaMethod method, PositiveMagnitude tolerance, StepControl control, DenseConditions dense, DenseOutput.DenseInterpolant interp) {
            Method = method; Tolerance = tolerance; Control = control; Dense = dense; Interp = interp;
        }
```

To:
```csharp
public sealed record RungeKuttaIntegrator {
    private RungeKuttaIntegrator(
        RungeKuttaMethod method,
        Option<(PositiveMagnitude Tolerance, StepControl Control)> adaptive) =>
        (Method, Adaptive) = (method, adaptive);

    public RungeKuttaMethod Method { get; }
    private Option<(PositiveMagnitude Tolerance, StepControl Control)> Adaptive { get; }
```

Why: Fixed versus adaptive is already determined by the tableau's embedded-pair option. The union repeats that discriminant, duplicates method and dense-output storage, and duplicates identical stage and span work across two dispatch arms.

Change: Construct and collapse `DenseOutput.Interpolant` and `DenseOutput.Conditions` once in `RungeKuttaMethod.Of`, store them on the method row, replace the integrator union with one record carrying optional adaptive policy, compute stages and the primary combination once, and match only the adaptive policy for error control.

Delta: -20 LOC; -7 net module-level members; -2 module-level types.

# 11. Construct dense spans directly without dropping finite-stage admission

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:601`
```csharp
internal static Fin<DenseOutputSpan<TState, TDelta>> Of(IntegrationModule<TState, TDelta> module, TState start, double step, Seq<TDelta> stages, ButcherTableau tableau, DenseFormula family, DenseOutput.DenseInterpolant interpolant, DenseConditions conditions) =>
    DenseOutput.WeightsAt(tableau, family, interpolant, 1.0).Bind(weights => {
        TDelta reconstructed = module.Combine(coefficients: weights, deltas: stages);
        TDelta declared = module.Combine(coefficients: tableau.Weights, deltas: stages);
        double drift = module.Norm(arg: module.Sum(arg1: reconstructed, arg2: module.Scale(arg1: -1.0, arg2: declared)));
        double stageScale = stages.Fold(initialState: 0.0, f: (max, delta) => Math.Max(val1: max, val2: module.Norm(arg: delta)));
        return double.IsFinite(drift) && drift <= EpsilonPolicy.SqrtEpsilon * Math.Max(val1: 1.0, val2: stageScale)
            && stages.Count == tableau.StageCount && Math.Abs(value: step) > EpsilonPolicy.ZeroTolerance && conditions.IsValid
            ? Fin.Succ(new DenseOutputSpan<TState, TDelta>(start, step, stages, tableau, family, interpolant, conditions, module))
            : Fin.Fail<DenseOutputSpan<TState, TDelta>>(new KernelFault.InvalidResult());
    });
```

To:
```csharp
// DenseOutputSpan.Of DELETED
```

Why: Method construction already proves endpoint coefficients and continuous-extension conditions, and an admitted tableau makes the stage count structural. Reconstructing both endpoint combinations on every accepted step is duplicate hot-loop validation, but the current stage-norm finiteness check must remain at the point each stage is produced.

Change: Gate finite nonzero `h` at `Step` entry, bind every sampled stage through `double.IsFinite(module.Norm(k))` inside `Stages`, construct `DenseOutputSpan` directly after successful stages, store the method instead of tableau/formula/interpolant/conditions, derive `Conditions` from the method, inline weight evaluation in `PointAt`, and delete `DenseOutputSpan.Of` plus the one-call `DenseOutput.WeightsAt` forwarder.

Delta: -15 LOC; -4 module-level members; 0 types; endpoint reconstruction leaves the accepted-step hot path without weakening non-finite stage rejection.

# 12. Delete integrator admission forwarding methods

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:664`
```csharp
public static Fin<RungeKuttaIntegrator> Admit(RungeKuttaIntegrator value) =>
    Optional(value).ToFin(new KernelFault.InvalidInput());
public static Fin<RungeKuttaIntegrator> AdmitOrFixed(Option<RungeKuttaIntegrator> value) =>
    value.Match(Some: Fin.Succ, None: () => Fixed(method: RungeKuttaMethod.RK4));
```

To:
```csharp
// RungeKuttaIntegrator.Admit and RungeKuttaIntegrator.AdmitOrFixed DELETED
```

Why: Integrators are admitted only by `Fixed` and `Adaptive`; `Admit` merely null-checks an admitted value, while `AdmitOrFixed` hides one consumer's default behind a second hop.

Change: Delete both forwarding methods, use `Optional(integrator).ToFin(...)` at the required-input boundary, and spell the optional RK4 default at its sole consumer.

Delta: -4 LOC; -2 module-level members; 0 types.

Ripples: `libs/dotnet/Rasm/.planning/Parametric/projections.md` replaces `RungeKuttaIntegrator.Admit` in `SpringShape.Step`; `libs/dotnet/Rasm/.planning/Processing/extract.md` replaces `AdmitOrFixed` in `Extraction.StreamBundle`.

# 13. Refuse non-finite adaptive error before controller arithmetic

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:678`
```csharp
let primary = s.Module.Combine(coefficients: c.Method.Tableau.Weights, deltas: ks)
let secondary = s.Module.Combine(embedded.Weights, ks)
let err = Math.Abs(value: s.H) * s.Module.Norm(arg: s.Module.Sum(arg1: primary, arg2: s.Module.Scale(arg1: -1.0, arg2: secondary)))
let scale = c.Control.Rescale(s.History, err, c.Tolerance.Value, 1.0 / (embedded.Order + 1.0))
let next = s.Module.Add(s.State, s.H, primary)
from result in err <= c.Tolerance.Value
```

To:
```csharp
let primary = s.Module.Combine(coefficients: c.Method.Tableau.Weights, deltas: ks)
let secondary = s.Module.Combine(embedded.Weights, ks)
let err = Math.Abs(value: s.H) * s.Module.Norm(arg: s.Module.Sum(arg1: primary, arg2: s.Module.Scale(arg1: -1.0, arg2: secondary)))
from finite in guard(double.IsFinite(err), new KernelFault.InvalidResult())
let scale = c.Control.Rescale(s.History, err, c.Tolerance.Value, 1.0 / (embedded.Order + 1.0))
let next = s.Module.Add(s.State, s.H, primary)
from result in err <= c.Tolerance.Value
```

Why: For `NaN`, `error > ZeroTolerance` is false, so `Rescale` selects `MaxScale` and returns a rejection carrying a larger suggested step and non-finite evidence.

Change: Gate the measured error in the ambient `Fin` before rescaling or constructing either step outcome.

Delta: +1 LOC; 0 module-level symbols, members, or types; one invalid controller path removed.

# 14. Forward every quadrature sample coordinate

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:889`
```csharp
Counted(run: guard => Admit(outcome: l.Route.Evaluate(x => guard.Finite(l.F()), l.Bounds.Lower, l.Bounds.Upper, ctl), skipped: guard.Skipped, ctl: ctl))
Counted(run: guard => Admit(outcome: (Value: Integrate.OnRectangle(y => guard.Finite(r.F()), r.X.Lower, r.X.Upper, r.Y.Lower, r.Y.Upper, r.Order), Error: Option<double>.None, L1Norm: Option<double>.None), skipped: guard.Skipped, ctl: ctl))
Counted(run: guard => Admit(outcome: (Value: Integrate.OnCuboid(z => guard.Finite(c.F(z)), c.X.Lower, c.X.Upper, c.Y.Lower, c.Y.Upper, c.Z.Lower, c.Z.Upper, c.Order), Error: Option<double>.None, L1Norm: Option<double>.None), skipped: guard.Skipped, ctl: ctl))
Counted(run: guard => Admit(outcome: Smolyak.Integrate(f: x => guard.Finite(s.F()), bounds: s.Bounds, level: s.Level), skipped: guard.Skipped, ctl: ctl))
```

To:
```csharp
Counted(run: guard => Admit(outcome: l.Route.Evaluate(x => guard.Finite(l.F(x)), l.Bounds.Lower, l.Bounds.Upper, ctl), skipped: guard.Skipped, ctl: ctl))
Counted(run: guard => Admit(outcome: (Value: Integrate.OnRectangle((x, y) => guard.Finite(r.F(x, y)), r.X.Lower, r.X.Upper, r.Y.Lower, r.Y.Upper, r.Order), Error: Option<double>.None, L1Norm: Option<double>.None), skipped: guard.Skipped, ctl: ctl))
Counted(run: guard => Admit(outcome: (Value: Integrate.OnCuboid((x, y, z) => guard.Finite(c.F(x, y, z)), c.X.Lower, c.X.Upper, c.Y.Lower, c.Y.Upper, c.Z.Lower, c.Z.Upper, c.Order), Error: Option<double>.None, L1Norm: Option<double>.None), skipped: guard.Skipped, ctl: ctl))
Counted(run: guard => Admit(outcome: Smolyak.Integrate(f: x => guard.Finite(s.F(x)), bounds: s.Bounds, level: s.Level), skipped: guard.Skipped, ctl: ctl))
```

Why: The line and sparse-grid adapters discard the package coordinate, while the rectangle and cuboid adapters supply too few arguments to their domain delegates.

Change: Forward the actual sample coordinates to each domain integrand and pass only its returned scalar through the skip counter.

Delta: 0 LOC; 0 module-level symbols, members, or types; four invalid delegate adapters removed.

# 15. Fuse quadrature measurement and admission

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:910`
```csharp
private static Fin<QuadratureEvidence> Counted(Func<SkipCounter, Fin<QuadratureEvidence>> run) => run(arg: new SkipCounter());
```

To:
```csharp
private static Fin<QuadratureEvidence> Measure(
    Func<SkipCounter, (double Value, Option<double> Error, Option<double> L1Norm)> evaluate,
    QuadratureControl control) {
    SkipCounter counter = new();
    return Admit(evaluate(counter), counter.Skipped, control);
}
```

Why: `Counted` only allocates and forwards, while every caller repeats the same evaluation-to-admission plumbing. The boundary operation is measuring one guarded estimate and admitting it with the resulting skip count.

Change: Replace `Counted` with `Measure`, make every dispatch arm return only the private estimate tuple, and call `Admit` once inside `Measure`.

Delta: -4 LOC across the five dispatch arms; 0 net module-level members or types; five repeated admission calls and one forwarding method removed.

# 16. Couple quadrature error and cancellation evidence

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:873`
```csharp
public sealed record QuadratureEvidence(
    double Value, Option<double> Error, Option<double> L1Norm, Option<double> Ratio, int Skipped);
```

To:
```csharp
public sealed record QuadratureEvidence(
    double Value, Option<(double Error, double CancellationRatio)> Estimate, int Skipped);
```

Why: `L1Norm` is a private MathNet preimage used only to derive cancellation ratio. Four independent public options permit impossible combinations even though the adaptive kernel produces error and L1 norm together; the ODE route fabricates this quadrature type despite already publishing its error through `Convergence`.

Change: Keep L1 norm only in the private estimate tuple, construct one optional `(Error, CancellationRatio)` after both budget checks, remove the synthetic `QuadratureEvidence` assignment from the traced ODE route, and project the coupled option in quadrature summaries.

Delta: -4 LOC across the named ripples; -2 module-level members; 0 types; contradictory public evidence states removed.

Ripples: `libs/dotnet/Rasm.Compute/.planning/Tensor/quadrature.md` derives `Unwitnessed`, `ErrorBound`, and `Conditioning` from `row.Estimate`; `libs/dotnet/Rasm.Compute/.planning/Solver/route.md` deletes the traced route's `Evidence = Some(new QuadratureEvidence(...))` initializer.

# 17. Pass tetrahedral simplex coefficients as scalars

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:769`
```csharp
internal static readonly QuadratureRule Tet4 = new(2, [.. Simplex3(ab: [(5.0 + (3.0 * Math.Sqrt(5.0))) / 20.0, (5.0 - Math.Sqrt(5.0)) / 20.0])]);

private static IEnumerable<(double, double, double, double)> Simplex3(double[] ab) {
    (double a, double b) = (ab[0], ab[1]);
```

To:
```csharp
internal static readonly QuadratureRule Tet4 = new(2, [.. Simplex3(a: (5.0 + (3.0 * Math.Sqrt(5.0))) / 20.0, b: (5.0 - Math.Sqrt(5.0)) / 20.0)]);

private static IEnumerable<(double, double, double, double)> Simplex3(double a, double b) {
```

Why: The helper consumes exactly two coefficients but accepts an arbitrary array, indexes it without admission, and allocates it during static initialization.

Change: Pass the two tetrahedral simplex coefficients directly and delete the indexed array.

Delta: -1 LOC; 0 module-level symbols, members, or types; one allocation and one unchecked shape removed.

# 18. Preserve double-double dense moment residuals through subtraction

From: `libs/dotnet/Rasm/.planning/Numerics/integrate.md:485`
```csharp
Enumerable.Range(0, order)
    .Select(moment => {
        ddouble actual = weights.Zip(tableau.Abscissae).Fold((ddouble)0.0,
            (sum, pair) => sum + (pair.First * ddouble.Pow(pair.Second, moment)));
        return Math.Abs((double)actual - (Math.Pow(theta, moment + 1) / (moment + 1.0)));
    })
```

To:
```csharp
Enumerable.Range(0, order)
    .Select(moment => {
        ddouble actual = weights.Zip(tableau.Abscissae).Fold((ddouble)0.0,
            (sum, pair) => sum + (pair.First * ddouble.Pow(pair.Second, moment)));
        return (double)ddouble.Abs(actual - (ddouble.Pow(theta, moment + 1) / (moment + 1)));
    })
```

Why: The target is raised in `double` and the accumulated moment is narrowed before comparison, so the residual does not retain double-double precision.

Change: Raise the target and subtract in `ddouble`; narrow only the absolute residual returned to the evidence fold.

Delta: 0 LOC; 0 module-level symbols, members, or types; two premature precision collapses removed.

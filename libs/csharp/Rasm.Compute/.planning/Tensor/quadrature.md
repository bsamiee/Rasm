# [COMPUTE_QUADRATURE]

Rasm.Compute owned-build numeric lane for integration and spectral operators: an accuracy-routed quadrature owner, an embedded-pair adaptive integrator over an additive-module carrier, and a constant-coefficient periodic spectral operator. Each result leaves as typed evidence carrying its conditioning channel, terminal partition, or imaginary residual — never a bare `double`, a sampled trajectory, or a raw `Complex[]` — so a gate rejects on quality and a caller resumes a budget-exhausted run rather than reading best-so-far as convergence.

MathNet ships the kernels but no gate: no route inspects a return for non-finiteness, only `GaussKronrod` yields the `error`/`L1Norm` conditioning channel, only `DoubleExponential`/`GaussLegendre` substitute infinite bounds through a baked-in abscissa transform, `RungeKutta` is fixed-step scalar with no embedded pair or receipt, and `Fourier` exposes no operator-symbol algebra — so the finite guard, cancellation gate, per-route infinite-bound capability, the rooted-tree order proof over the `IModule<TSelf>` carrier, and the symbol vocabulary with parity-derived Nyquist handling are all owned here. Host-local, crossing no wire; the guarded-integrand skip counters, tableau stage folds, `RootedTree` order walk, and in-place `Fourier` transform on the spectral copy are its sanctioned statement-form numeric kernels.

## [01]-[INDEX]

- [02]-[QUADRATURE_ROUTE]: accuracy-routed `QuadratureRoute` `[SmartEnum<string>]` over the `IntegrationDomain` `[Union]` arity (line · rectangle · cuboid · Smolyak sparse-grid); the one finite-guard-then-admit combinator with the error-budget and `L1` cancellation gates.
- [03]-[INTEGRATOR_TABLEAU]: the embedded-pair `StepTableau` with the `RootedTree` uncapped Butcher-tree order proof, the `IModule<TSelf>` additive-module carrier, the frozen `AdaptiveControl` policy, and the `IntegratorTerminal` partition.
- [04]-[SPECTRAL_OPERATOR]: the `SpectralSymbol` `[SmartEnum<string>]` multiplier vocabulary composed through `Spectral`, applied pointwise with parity-derived Nyquist zeroing — the packed-real `ForwardReal`/`InverseReal` leg on even lengths, the complex leg with its imaginary-residual gate on odd.

## [02]-[QUADRATURE_ROUTE]

- Owner: `QuadratureRoute` the `[SmartEnum<string>]` accuracy axis carrying each kernel's `KernelOutcome` delegate and `InfiniteBounds` capability; `IntegrationDomain` the `[Union]` arity axis over genuine 1-D/2-D/3-D integrands; `IntervalSpec` the bound value-object whose values alone encode finite or infinite extent; `Quadrature.Integrate` the one entry whose `Admit` combinator reads the delegate column once.
- Cases: `QuadratureRoute` rows double-exponential, gauss-legendre, gauss-kronrod; `IntegrationDomain` cases `Line`, `Rectangle`, `Cuboid`, `SparseGrid` — full-tensor cubature riding `Rectangle`/`Cuboid` to d=3, the Smolyak `SparseGrid(F, Bounds, Level)` case answering the 5-20-dimensional projection-PCE and moment integrals a full tensor product cannot reach.
- Auto: each arm wraps its integrand in a skip-counting guard because no route inspects returns and a pole poisons the weighted sum silently; `QuadratureControl.MaxSkipped` makes that loss budget explicit and defaults to zero; one `Try.lift<Fin<QuadratureEvidence>>` captures integrand and MathNet exceptions onto the typed rail; a `Line` arm faults a route lacking `InfiniteBounds` against an infinite `IntervalSpec` rather than feeding infinity to a finite-only kernel; only `GaussKronrod` returns the `error`/`L1Norm` channel, so the error-budget and cancellation gates bind only where the channels are `Some`; `SparseGrid` folds the nested Clenshaw-Curtis combination formula through `SmolyakCubature.Integrate` under `MaxSparseLevel`.
- Receipt: `QuadratureEvidence` carries `Option<double>` error, L1, and ratio so a non-adaptive route reports honest absence, never a fabricated `NaN`; the skip count rides the receipt, never silently as coverage; the gate is three-tier — non-finite rejects, an adaptive error estimate over `max(AbsoluteError, RelativeError·|value|)` rejects as `<error-over-budget>` (the estimate is admission evidence, never receipt decoration), and a cancellation ratio breaching `floor` rejects — never a rejection on slow convergence alone.
- Packages: MathNet.Numerics, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new accuracy kernel is one `QuadratureRoute` row with its delegate and infinite-bound capability; a new arity is one `IntegrationDomain` case; a new sparse-grid 1-D rule family (nested Gauss-Patterson) or dimension-adaptive refinement is a policy row on `SmolyakCubature`, never a sibling integrator; zero new surface.
- Boundary — accuracy is the primary decision with order secondary: double-exponential, fixed Gauss-Legendre, and adaptive Gauss-Kronrod bind `Integrate.DoubleExponential`/`GaussLegendre`/`GaussKronrod` as `QuadratureRoute` rows, never three sibling factories, and the finite-guard-then-admit combinator applies once over the uniform `KernelOutcome` column, never re-spelled per kernel.
- Boundary — infinite bounds route only into `DoubleExponential`/`GaussLegendre`, whose MathNet entries substitute infinity through a baked-in abscissa transform; `GaussKronrod` and the cubature rules integrate the raw interval and fault on an infinite bound rather than yielding `NaN` weights, so `InfiniteBounds` is load-bearing. A 1-D delegate forced through a 2-D rule integrates `(b−a)·∫f` and is rejected.
- Boundary — `error`/`L1Norm`/`Ratio` are `Option<double>` because only the adaptive Kronrod row yields them; a `NaN` sentinel posing as a value is rejected. Cancellation ratio `|value/L1|` is the free conditioning diagnostic the short `GaussKronrod` overload discards, read here from the out-param overload so a gate rejects ill-conditioned cancellation, not a converged small result.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
public readonly record struct IntervalSpec(double Lower, double Upper) {
    public bool Infinite => double.IsInfinity(Lower) || double.IsInfinity(Upper);
}

public readonly record struct KernelOutcome(double Value, Option<double> Error, Option<double> L1Norm);

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class QuadratureRoute {
    // InfiniteBounds is true for DoubleExponential/GaussLegendre — the MathNet abscissa transform is baked in —
    // and false for GaussKronrod, which integrates the raw interval and is finite-only.
    public static readonly QuadratureRoute DoubleExponential = new("double-exponential", infiniteBounds: true,
        kernel: static (f, lo, hi, c) => new KernelOutcome(Integrate.DoubleExponential(f, lo, hi, c.AbsoluteError), None, None));
    public static readonly QuadratureRoute GaussLegendre = new("gauss-legendre", infiniteBounds: true,
        kernel: static (f, lo, hi, c) => new KernelOutcome(Integrate.GaussLegendre(f, lo, hi, c.LegendreOrder), None, None));
    public static readonly QuadratureRoute GaussKronrod = new("gauss-kronrod", infiniteBounds: false,
        kernel: static (f, lo, hi, c) => {
            double value = Integrate.GaussKronrod(f, lo, hi, out double error, out double l1Norm, c.RelativeError, c.MaximumDepth, c.KronrodPoints);
            return new KernelOutcome(value, Some(error), Some(l1Norm));
        });

    private readonly Func<Func<double, double>, double, double, QuadratureControl, KernelOutcome> kernel;

    public bool InfiniteBounds { get; }

    public KernelOutcome Run(Func<double, double> f, double lower, double upper, QuadratureControl control) => kernel(f, lower, upper, control);
}

[Union]
public abstract partial record IntegrationDomain {
    private IntegrationDomain() { }

    public sealed record Line(Func<double, double> F, IntervalSpec Bounds, QuadratureRoute Route) : IntegrationDomain;
    public sealed record Rectangle(Func<double, double, double> F, IntervalSpec X, IntervalSpec Y, int Order) : IntegrationDomain;
    public sealed record Cuboid(Func<double, double, double, double> F, IntervalSpec X, IntervalSpec Y, IntervalSpec Z, int Order) : IntegrationDomain;
    public sealed record SparseGrid(Func<double[], double> F, IntervalSpec[] Bounds, int Level) : IntegrationDomain;
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record QuadratureControl(double AbsoluteError, double RelativeError, double CancellationFloor, int MaxSkipped, int LegendreOrder, int KronrodPoints, int MaximumDepth, int MaxSparseLevel) {
    public static readonly QuadratureControl Default = new(AbsoluteError: 1e-8, RelativeError: 1e-8, CancellationFloor: 1e-10, MaxSkipped: 0, LegendreOrder: 128, KronrodPoints: 15, MaximumDepth: 15, MaxSparseLevel: 8);
}

public sealed record QuadratureEvidence(double Value, Option<double> Error, Option<double> L1Norm, Option<double> Ratio, int Skipped);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class Quadrature {
    public static Fin<QuadratureEvidence> Integrate(IntegrationDomain domain, QuadratureControl? control = null) {
        QuadratureControl ctl = control ?? QuadratureControl.Default;
        if (domain is null) {
            return Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected("<quadrature-domain-null>"));
        }

        if (!Valid(ctl)) {
            return Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected("<quadrature-control-invalid>"));
        }

        return Try.lift<Fin<QuadratureEvidence>>(() => domain.Switch(
            line: l => {
                if (!Ordered(l.Bounds)) {
                    return Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected("<quadrature-bounds-invalid>"));
                }

                if (l.Bounds.Infinite && !l.Route.InfiniteBounds) {
                    return Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected($"<infinite-bounds-unsupported:route={l.Route.Key}>"));
                }

                int skipped = 0;
                Func<double, double> guarded = x => l.F(x) is var y && double.IsFinite(y) ? y : (skipped++, 0.0).Item2;
                return Admit(l.Route.Run(guarded, l.Bounds.Lower, l.Bounds.Upper, ctl), skipped, ctl);
            },
            rectangle: r => {
                if (!FiniteOrdered(r.X) || !FiniteOrdered(r.Y) || r.Order <= 0) {
                    return Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected("<cubature-domain-invalid>"));
                }

                int skipped = 0;
                Func<double, double, double> guarded = (x, y) => r.F(x, y) is var z && double.IsFinite(z) ? z : (skipped++, 0.0).Item2;
                return Admit(new KernelOutcome(Integrate.OnRectangle(guarded, r.X.Lower, r.X.Upper, r.Y.Lower, r.Y.Upper, r.Order), None, None), skipped, ctl);
            },
            cuboid: c => {
                if (!FiniteOrdered(c.X) || !FiniteOrdered(c.Y) || !FiniteOrdered(c.Z) || c.Order <= 0) {
                    return Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected("<cubature-domain-invalid>"));
                }

                int skipped = 0;
                Func<double, double, double, double> guarded = (x, y, z) => c.F(x, y, z) is var w && double.IsFinite(w) ? w : (skipped++, 0.0).Item2;
                return Admit(new KernelOutcome(Integrate.OnCuboid(guarded, c.X.Lower, c.X.Upper, c.Y.Lower, c.Y.Upper, c.Z.Lower, c.Z.Upper, c.Order), None, None), skipped, ctl);
            },
            sparseGrid: s => {
                if (s.Bounds is null || s.Bounds.Length is < 2 or > 20 || s.Level <= 0 || s.Level > ctl.MaxSparseLevel || toSeq(s.Bounds).Exists(static b => !FiniteOrdered(b))) {
                    return Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected("<sparse-grid-domain-invalid>"));
                }

                int skipped = 0;
                Func<double[], double> guarded = x => s.F(x) is var y && double.IsFinite(y) ? y : (skipped++, 0.0).Item2;
                return Admit(SmolyakCubature.Integrate(guarded, s.Bounds, s.Level), skipped, ctl);
            }))
            .Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<quadrature-kernel:{error.Message}>"))
            .Bind(identity);
    }

    // Admission rejects non-finite value, adaptive error beyond `max(abs, rel·|value|)`, and cancellation below
    // floor where L1 exists; absent evidence channels remain `None`.
    static Fin<QuadratureEvidence> Admit(KernelOutcome outcome, int skipped, QuadratureControl ctl) =>
        !double.IsFinite(outcome.Value)
            ? Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected($"<quadrature-nonfinite:skipped={skipped}>"))
        : skipped > ctl.MaxSkipped
            ? Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected($"<integrand-loss:skipped={skipped}:budget={ctl.MaxSkipped}>"))
        : outcome.Error is { IsSome: true, Case: double estimate }
          && estimate > Math.Max(ctl.AbsoluteError, ctl.RelativeError * Math.Abs(outcome.Value))
            ? Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected($"<error-over-budget:error={estimate:e3}:budget={Math.Max(ctl.AbsoluteError, ctl.RelativeError * Math.Abs(outcome.Value)):e3}:skipped={skipped}>"))
        : outcome.L1Norm.Map(l1 => l1 == 0.0 && outcome.Value == 0.0 ? 1.0 : Math.Abs(outcome.Value / l1)).Match(
            Some: ratio => double.IsFinite(ratio) && ratio >= ctl.CancellationFloor
                ? Fin.Succ(new QuadratureEvidence(outcome.Value, outcome.Error, outcome.L1Norm, Some(ratio), skipped))
                : Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected($"<cancellation-breach:ratio={ratio:e3}:skipped={skipped}>")),
            None: () => Fin.Succ(new QuadratureEvidence(outcome.Value, outcome.Error, None, None, skipped)));

    static bool Valid(QuadratureControl control) =>
        double.IsFinite(control.AbsoluteError) && control.AbsoluteError > 0.0
        && double.IsFinite(control.RelativeError) && control.RelativeError > 0.0
        && double.IsFinite(control.CancellationFloor) && control.CancellationFloor is >= 0.0 and <= 1.0
        && control.MaxSkipped >= 0 && control.LegendreOrder > 0 && control.KronrodPoints > 0 && control.MaximumDepth > 0
        && control.MaxSparseLevel is > 0 and < 31;

    static bool Ordered(IntervalSpec bounds) => !double.IsNaN(bounds.Lower) && !double.IsNaN(bounds.Upper) && bounds.Lower < bounds.Upper;

    static bool FiniteOrdered(IntervalSpec bounds) => !bounds.Infinite && Ordered(bounds);
}

// Smolyak combines nested Clenshaw-Curtis rules across `q−d+1 ≤ |ℓ|₁ ≤ q` for 5–20-dimensional PCE and moments;
// nested Chebyshev extrema retain lower-level evaluations without a full tensor product.
public static class SmolyakCubature {
    public static KernelOutcome Integrate(Func<double[], double> f, IntervalSpec[] bounds, int level) {
        int d = bounds.Length;
        double sum = 0.0;
        foreach ((int[] multi, int coefficient) in CombinationLevels(d, level)) {
            double block = 0.0;
            foreach ((double[] node, double weight) in TensorNodes(multi, bounds)) { block += weight * f(node); }
            sum += coefficient * block;
        }
        return new KernelOutcome(sum, None, None);
    }

    // Multi-indices with q−d+1 ≤ |ℓ|₁ ≤ q and the Smolyak coefficient (−1)^(q−|ℓ|₁)·C(d−1, q−|ℓ|₁).
    static IEnumerable<(int[] Multi, int Coefficient)> CombinationLevels(int dimensions, int level) {
        int q = level + dimensions - 1;
        return Compositions(dimensions, q)
            .Where(multi => multi.All(static l => l >= 1) && multi.Sum() >= q - dimensions + 1)
            .Select(multi => (multi, (int)(Math.Pow(-1, q - multi.Sum()) * Binomial(dimensions - 1, q - multi.Sum()))));
    }

    static IEnumerable<(double[] Node, double Weight)> TensorNodes(int[] multi, IntervalSpec[] bounds) {
        (double[] Nodes, double[] Weights)[] axes = [.. multi.Select((l, k) => ClenshawCurtis(l, bounds[k]))];
        return Enumerable.Range(0, axes.Aggregate(1, static (acc, axis) => acc * axis.Nodes.Length)).Select(flat => {
            double[] node = new double[axes.Length];
            double weight = 1.0;
            int rest = flat;
            for (int k = axes.Length - 1; k >= 0; k--) {
                int i = rest % axes[k].Nodes.Length;
                rest /= axes[k].Nodes.Length;
                node[k] = axes[k].Nodes[i];
                weight *= axes[k].Weights[i];
            }
            return (node, weight);
        });
    }

    // Nested Chebyshev-extrema rule at level ℓ: n = 2^(ℓ−1)+1 points (ℓ=1 the midpoint), classic
    // Clenshaw-Curtis cosine-sum weights mapped onto [lower, upper].
    static (double[] Nodes, double[] Weights) ClenshawCurtis(int level, IntervalSpec bounds) {
        int n = level == 1 ? 1 : (1 << (level - 1)) + 1;
        double half = 0.5 * (bounds.Upper - bounds.Lower);
        double mid = 0.5 * (bounds.Upper + bounds.Lower);
        if (n == 1) { return ([mid], [2.0 * half]); }
        double[] nodes = new double[n];
        double[] weights = new double[n];
        for (int i = 0; i < n; i++) {
            nodes[i] = mid - half * Math.Cos(Math.PI * i / (n - 1));
            double w = 1.0;
            for (int j = 1; j <= (n - 1) / 2; j++) {
                double b = 2 * j == n - 1 ? 1.0 : 2.0;
                w -= b * Math.Cos(2.0 * j * Math.PI * i / (n - 1)) / (4.0 * j * j - 1.0);
            }
            weights[i] = half * 2.0 * w / (n - 1) * (i == 0 || i == n - 1 ? 0.5 : 1.0);
        }
        return (nodes, weights);
    }

    static IEnumerable<int[]> Compositions(int slots, int maxTotal) =>
        slots == 0
            ? [Array.Empty<int>()]
            : from head in Enumerable.Range(0, maxTotal + 1)
              from tail in Compositions(slots - 1, maxTotal - head)
              select tail.Prepend(head).ToArray();

    static double Binomial(int n, int k) =>
        k < 0 || k > n ? 0.0 : Enumerable.Range(0, k).Aggregate(1.0, (acc, i) => acc * (n - i) / (i + 1));
}
```

## [03]-[INTEGRATOR_TABLEAU]

- Owner: `StepTableau` the embedded-pair Butcher tableau deriving both propagating and embedded order at construction; `IModule<TSelf>` the additive-module carrier one step writes over; `RootedTree` the Butcher-tree algebra proving order uncapped; `AdaptiveControl` the frozen step-control policy; `IntegratorTerminal` the terminal partition; `IntegratorEvidence` the run receipt.
- Cases: `IModule<TSelf>` carriers `Scalar`, `ComplexState`, `VectorState` (scalar/complex/fixed-rank-vector and grid-slab unified, collapsing the scalar-versus-vector transcription class); `IntegratorTerminal` rows completed, budget-exhausted, step-underflow, non-finite carrying `Resolved`/`Retryable` columns.
- Auto: `Create` validates strict-lower-triangular `A`, row-sum consistency `Σⱼ A[i,j] = C[i]`, and `Σ B = Σ BHat = 1`, then derives order through the captured `RootedTree` walk; private `Step` shares the stage vector between `B` and `BHat`; `Integrate` captures RHS/carrier failures, grows on scaled error ≤ 1, shrinks otherwise, consumes the consecutive-reject budget, clamps the final step to `t1`, and partitions exhaustion as retryable best-so-far.
- Receipt: `IntegratorEvidence` records the terminal partition with achieved time and step/reject counts because convergence, budget, and underflow all return best-so-far indistinguishable without the marker; `Resolved`/`Retryable` let a caller relax tolerance or budget and resume rather than lose the partial integration.
- Packages: MathNet.Numerics, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new method is one `StepTableau.Create` call over its coefficient arrays — order derived, never transcribed; a new state shape is one `IModule<TSelf>` carrier; a new terminal is one `IntegratorTerminal` row; zero new surface.
- Boundary — the embedded pair `B`/`BHat` makes the local error the genuine scaled difference of two orders sharing one stage vector: a single `B` row with an `AdaptiveControl` whose `NextStep` never fires is fixed-step masquerading as adaptive, and the driver reads `control` every step.
- Boundary — order is derived as the largest p for which every rooted tree of order p satisfies `Σᵢ bᵢ Φᵢ(t) = 1/γ(t)`, bounded by the stage count (Butcher barrier); a hand-transcribed `switch` capped at a literal with a runtime `_ => false` misreports a 5(4) pair as order 4 and is rejected.
- Boundary — error norm is the carrier's static-abstract `ScaledError`, a per-component two-pass RMS dividing by `atol + rtol·max` before squaring so large-magnitude state never overflows the naive squared-sum-then-root; a default `=> 0.0` gating nothing is rejected.
- Boundary — every termination succeeds with its marker; only an inadmissible span (`t1 ≤ t0` or non-positive initial step) faults, because mapping budget-exhaustion to `Fin.Fail` destroys the caller's relaxed-criterion retry.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
public interface IModule<TSelf> where TSelf : IModule<TSelf> {
    static abstract TSelf operator +(TSelf left, TSelf right);
    static abstract TSelf operator *(double scalar, TSelf value);

    // Embedded-difference scaled error the driver gates on (target 1.0); each carrier owns the per-component
    // reduction — no instance default, no stub — so a carrier that cannot weigh its error cannot be integrated.
    static abstract double ScaledError(TSelf high, TSelf low, double absoluteTolerance, double relativeTolerance);
}

public readonly record struct Scalar(double Value) : IModule<Scalar> {
    public static Scalar operator +(Scalar left, Scalar right) => new(left.Value + right.Value);
    public static Scalar operator *(double scalar, Scalar value) => new(scalar * value.Value);

    public static double ScaledError(Scalar high, Scalar low, double atol, double rtol) =>
        Math.Abs(high.Value - low.Value) / (atol + rtol * Math.Max(Math.Abs(high.Value), Math.Abs(low.Value)));
}

public readonly record struct ComplexState(Complex Value) : IModule<ComplexState> {
    public static ComplexState operator +(ComplexState left, ComplexState right) => new(left.Value + right.Value);
    public static ComplexState operator *(double scalar, ComplexState value) => new(scalar * value.Value);

    public static double ScaledError(ComplexState high, ComplexState low, double atol, double rtol) =>
        (high.Value - low.Value).Magnitude / (atol + rtol * Math.Max(high.Value.Magnitude, low.Value.Magnitude));
}

public sealed record VectorState : IModule<VectorState> {
    private VectorState(ImmutableArray<double> values) => Values = values;

    public ImmutableArray<double> Values { get; }

    public static Fin<VectorState> Create(IEnumerable<double> values) =>
        values is null
            ? Fin.Fail<VectorState>(new ComputeFault.ModelRejected("<vector-state-invalid>"))
            : Try.lift<Fin<VectorState>>(() => {
                    ImmutableArray<double> materialized = [.. values];
                    return materialized.Length > 0 && materialized.All(double.IsFinite)
                        ? Fin.Succ(new VectorState(materialized))
                        : Fin.Fail<VectorState>(new ComputeFault.ModelRejected("<vector-state-invalid>"));
                })
                .Run()
                .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<vector-state-materialize:{error.Message}>"))
                .Bind(identity);

    public static VectorState operator +(VectorState left, VectorState right) {
        double[] sum = new double[left.Values.Length];
        for (int i = 0; i < sum.Length; i++) { sum[i] = left.Values[i] + right.Values[i]; }
        return new([.. sum]);
    }

    public static VectorState operator *(double scalar, VectorState value) {
        double[] scaled = new double[value.Values.Length];
        for (int i = 0; i < scaled.Length; i++) { scaled[i] = scalar * value.Values[i]; }
        return new([.. scaled]);
    }

    // Scaled two-pass RMS: divide by atol + rtol·max BEFORE squaring, so an infinity here is norm policy, not overflow.
    public static double ScaledError(VectorState high, VectorState low, double atol, double rtol) {
        double sum = 0.0;
        for (int i = 0; i < high.Values.Length; i++) {
            double scale = atol + rtol * Math.Max(Math.Abs(high.Values[i]), Math.Abs(low.Values[i]));
            double term = (high.Values[i] - low.Values[i]) / scale;
            sum += term * term;
        }

        return Math.Sqrt(sum / high.Values.Length);
    }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class IntegratorTerminal {
    public static readonly IntegratorTerminal Completed = new("completed", resolved: true, retryable: false);
    public static readonly IntegratorTerminal BudgetExhausted = new("budget-exhausted", resolved: false, retryable: true);
    public static readonly IntegratorTerminal StepUnderflow = new("step-underflow", resolved: false, retryable: true);
    public static readonly IntegratorTerminal NonFinite = new("non-finite", resolved: false, retryable: false);

    public bool Resolved { get; }
    public bool Retryable { get; }
}

// Butcher-tree algebra: a rooted tree is a multiset of subtrees, its order the node count, its density
// γ(t) = |t|·Π γ(children); the order conditions Σ_i b_i Φ_i(t) = 1/γ(t) over every tree of order ≤ p are the verified order.
public sealed record RootedTree(ImmutableArray<RootedTree> Children) {
    public static readonly RootedTree Leaf = new(ImmutableArray<RootedTree>.Empty);

    public int Order => 1 + Children.Sum(static child => child.Order);
    public double Density => Order * Children.Aggregate(1.0, static (acc, child) => acc * child.Density);

    public static int VerifiedOrder(double[,] a, double[] b) {
        int stages = b.Length;
        int order = 0;
        for (int p = 1; p <= stages; p++) {
            bool holds = OfOrder(p).All(tree => {
                double[] phi = tree.Weight(a, stages);
                double lhs = 0.0;
                for (int i = 0; i < stages; i++) { lhs += b[i] * phi[i]; }
                return Math.Abs(lhs - 1.0 / tree.Density) < 1e-10;
            });

            if (!holds) { break; }
            order = p;
        }

        return order;
    }

    // Elementary weight Φ_i(t): a single node weights 1 at every stage; a composite multiplies the stage-local
    // g_i = Σ_j a_ij Φ_j(child) over its child subtrees — so [τ] yields c_i and [[τ]] yields Σ_j a_ij c_j.
    public double[] Weight(double[,] a, int stages) {
        double[] phi = new double[stages];
        Array.Fill(phi, 1.0);
        foreach (RootedTree child in Children) {
            double[] childWeight = child.Weight(a, stages);
            for (int i = 0; i < stages; i++) {
                double g = 0.0;
                for (int j = 0; j < stages; j++) { g += a[i, j] * childWeight[j]; }
                phi[i] *= g;
            }
        }

        return phi;
    }

    // All rooted trees of exactly `order` nodes: a root over a multiset of subtrees whose orders sum to order − 1,
    // chosen in non-decreasing pool index so each multiset is emitted once.
    public static IEnumerable<RootedTree> OfOrder(int order) {
        if (order <= 1) { return [Leaf]; }
        ImmutableArray<RootedTree> pool = Enumerable.Range(1, order - 1).SelectMany(OfOrder).ToImmutableArray();
        return Forests(pool, order - 1, 0).Select(static forest => new RootedTree(forest));
    }

    static IEnumerable<ImmutableArray<RootedTree>> Forests(ImmutableArray<RootedTree> pool, int remaining, int start) {
        if (remaining == 0) { return [ImmutableArray<RootedTree>.Empty]; }
        return Enumerable.Range(start, pool.Length - start)
            .Where(idx => pool[idx].Order <= remaining)
            .SelectMany(idx => Forests(pool, remaining - pool[idx].Order, idx).Select(rest => rest.Insert(0, pool[idx])));
    }
}

// --- [MODELS] ------------------------------------------------------------------------------
public sealed record AdaptiveControl(double Safety, double MinRatio, double MaxRatio, int RejectBudget, double AbsoluteTolerance, double RelativeTolerance, double MinStep) {
    public static readonly AdaptiveControl Default = new(Safety: 0.9, MinRatio: 0.2, MaxRatio: 5.0, RejectBudget: 8, AbsoluteTolerance: 1e-9, RelativeTolerance: 1e-6, MinStep: 1e-12);

    // PI-free elementary controller: error < 1 grows, error > 1 shrinks, both clamped; the exponent uses the
    // lower (embedded) order + 1, the order governing the local error term.
    public double NextStep(double h, double error, int embeddedOrder) =>
        h * Math.Clamp(Safety * Math.Pow(Math.Max(error, double.Epsilon), -1.0 / (embeddedOrder + 1)), MinRatio, MaxRatio);
}

public readonly record struct StepOutcome<V>(V Proposal, double Error) where V : IModule<V>;

public sealed record IntegratorEvidence<V>(V State, IntegratorTerminal Terminal, double Achieved, int Steps, int Rejects, int RejectBudget) where V : IModule<V>;

public sealed record StepTableau(double[,] A, double[] B, double[] BHat, double[] C, int Order, int EmbeddedOrder) {
    public static Fin<StepTableau> Create(double[,] a, double[] b, double[] bHat, double[] c) {
        if (a is null || b is null || bHat is null || c is null) {
            return Fin.Fail<StepTableau>(new ComputeFault.ModelRejected("<tableau-null>"));
        }

        int stages = c.Length;
        bool shaped = stages > 0 && a.GetLength(0) == stages && a.GetLength(1) == stages && b.Length == stages && bHat.Length == stages;
        bool finite = shaped && a.Cast<double>().Concat(b).Concat(bHat).Concat(c).All(double.IsFinite);
        bool lower = shaped && toSeq(Enumerable.Range(0, stages)).ForAll(i => toSeq(Enumerable.Range(i, stages - i)).ForAll(j => a[i, j] == 0.0));
        bool rowSums = shaped && toSeq(Enumerable.Range(0, stages)).ForAll(i => Near(RowSum(a, i, stages), c[i]));
        bool consistent = Near(b.Sum(), 1.0) && Near(bHat.Sum(), 1.0);
        return Try.lift<Fin<StepTableau>>(() => {
                int order = finite && lower && rowSums && consistent ? RootedTree.VerifiedOrder(a, b) : 0;
                int embeddedOrder = finite && lower && rowSums && consistent ? RootedTree.VerifiedOrder(a, bHat) : 0;
                return order > 0 && embeddedOrder > 0 && order != embeddedOrder
                    ? Fin.Succ(new StepTableau(a, b, bHat, c, order, embeddedOrder))
                    : Fin.Fail<StepTableau>(new ComputeFault.ModelRejected($"<tableau-malformed:shaped={shaped}:finite={finite}:lower={lower}:rowsums={rowSums}:consistent={consistent}:order={order}:embedded={embeddedOrder}>"));
            })
            .Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<tableau-proof:{error.Message}>"))
            .Bind(identity);
    }

    StepOutcome<V> Step<V>(AdaptiveControl control, Func<double, V, V> rhs, double t, V state, double h) where V : IModule<V> {
        int stages = C.Length;
        V[] k = new V[stages];
        for (int i = 0; i < stages; i++) {
            V stage = state;
            for (int j = 0; j < i; j++) { stage += (h * A[i, j]) * k[j]; }
            k[i] = rhs(t + C[i] * h, stage);
        }

        V high = Combine(state, B, k, h);
        V low = Combine(state, BHat, k, h);
        return new StepOutcome<V>(high, V.ScaledError(high, low, control.AbsoluteTolerance, control.RelativeTolerance));
    }

    public Fin<IntegratorEvidence<V>> Integrate<V>(AdaptiveControl control, Func<double, V, V> rhs, double t0, V state0, double t1, double h0) where V : IModule<V> {
        bool validControl = control is not null && rhs is not null && state0 is not null
            && A is not null && B is not null && BHat is not null && C is not null
            && double.IsFinite(control.Safety) && control.Safety > 0.0
            && double.IsFinite(control.MinRatio) && control.MinRatio > 0.0
            && double.IsFinite(control.MaxRatio) && control.MaxRatio >= control.MinRatio
            && control.RejectBudget >= 0
            && double.IsFinite(control.AbsoluteTolerance) && control.AbsoluteTolerance > 0.0
            && double.IsFinite(control.RelativeTolerance) && control.RelativeTolerance > 0.0
            && double.IsFinite(control.MinStep) && control.MinStep > 0.0;
        if (!validControl || !double.IsFinite(t0) || !double.IsFinite(t1) || !double.IsFinite(h0) || !(t1 > t0) || !(h0 > 0.0)) {
            return Fin.Fail<IntegratorEvidence<V>>(new ComputeFault.ModelRejected($"<integrate-span:t0={t0}:t1={t1}:h0={h0}>"));
        }

        return Try.lift<Fin<IntegratorEvidence<V>>>(() => {
                (double t, double h, V state, int steps, int rejects, int streak) = (t0, h0, state0, 0, 0, 0);
                IntegratorTerminal terminal = IntegratorTerminal.Completed;
                while (t < t1) {
                    if (t + h > t1) { h = t1 - t; }
                    StepOutcome<V> outcome = Step(control, rhs, t, state, h);
                    if (!double.IsFinite(outcome.Error)) { terminal = IntegratorTerminal.NonFinite; break; }

                    if (outcome.Error <= 1.0) {
                        (t, state, steps, streak) = (t + h, outcome.Proposal, steps + 1, 0);
                    } else {
                        (rejects, streak) = (rejects + 1, streak + 1);
                        if (streak > control.RejectBudget) { terminal = IntegratorTerminal.BudgetExhausted; break; }
                    }

                    // Underflow only mid-interval — the final landing step clamped below MinStep is completion, not a stall.
                    h = control.NextStep(h, outcome.Error, EmbeddedOrder);
                    if (h < control.MinStep && t < t1) { terminal = IntegratorTerminal.StepUnderflow; break; }
                }

                return Fin.Succ(new IntegratorEvidence<V>(state, terminal, t, steps, rejects, control.RejectBudget));
            })
            .Run()
            .MapFail(static error => (Error)new ComputeFault.ModelRejected($"<integrator-kernel:{error.Message}>"))
            .Bind(identity);
    }

    static V Combine<V>(V state, double[] weights, V[] k, double h) where V : IModule<V> {
        V accumulated = state;
        for (int i = 0; i < k.Length; i++) { accumulated += (h * weights[i]) * k[i]; }
        return accumulated;
    }

    static double RowSum(double[,] a, int row, int stages) => toSeq(Enumerable.Range(0, stages)).Sum(col => a[row, col]);

    static bool Near(double value, double target) => Math.Abs(value - target) < 1e-12;
}
```

## [04]-[SPECTRAL_OPERATOR]

- Owner: `SpectralSymbol` the `[SmartEnum<string>]` Fourier-multiplier vocabulary carrying each operator's symbol delegate and parity; `SymbolParity` the composing parity policy; `Spectral` the pointwise-product composition carrier; `WaveAxis` the split-spectrum wavenumber owner; `SpectralOperator.Apply` the forward-multiply-inverse application with the imaginary-residual gate.
- Cases: `SpectralSymbol` rows derivative (`i·k`), laplacian (`−k²`), biharmonic (`k⁴`), hilbert (`−i·sgn k`), anti-derivative (`1/(i·k)`, zero mode killed); `SymbolParity` even, odd carrying `ZeroesNyquist`.
- Auto: `Spectral.At(k)` is the pointwise product of factor rows and `Spectral.Parity` the XOR-fold of factor parities, so the Nyquist bin zeroes exactly when the composition is odd; `Apply` dispatches on field length — even lengths ride the packed-real `Fourier.ForwardReal`/`InverseReal` pair (half the transform work, real output BY CONSTRUCTION, the symbol multiplying each nonnegative-k packed pair so Hermitian symmetry is structural), odd lengths ride the `Complex[]` pair whose imaginary-residual gate diagnoses broken symmetry — both over an internal spectral copy so the caller's field is never mutated; `WaveAxis.K()` derives the split-spectrum angular wavenumbers from `Fourier.FrequencyScale` scaled by 2π, never a hand-indexed bin walk.
- Receipt: `SpectralEvidence` carries the real result, the composed parity, and `Option<double>` imaginary residual — `Some` only on the complex leg where the gate read it, `None` on the packed-real leg whose realness is structural; excess residual fails the complex leg as broken Hermitian symmetry, never a usable result.
- Packages: MathNet.Numerics, Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: a new operator is one `SpectralSymbol` row with its symbol delegate and parity; a composite is a `Spectral.Then` chain; zero new code path.
- Boundary — every constant-coefficient periodic operator is one `SpectralSymbol` row applied pointwise to the forward transform; symbols compose by pointwise multiplication before a single inverse, and parity is row data the operator owns, never a `bool oddOrder` knob nor a bare `Func<double, Complex>` riding beside the call.
- Boundary — the split-spectrum wavenumber derives once at grid construction (ascending positives through Nyquist, then descending negatives, scaled by `2π/extent`) because hand-indexing the bin applies an aliased symbol past the half length silently; `Fourier.Forward`/`Inverse` pin `AsymmetricScaling` because the symmetric default cancels only on round trips, the most common silent error; excess imaginary residual is inadmissible because a real-symbol operator owes a machine-zero imaginary part and excess diagnoses broken Hermitian symmetry.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SymbolParity {
    public static readonly SymbolParity Even = new("even", zeroesNyquist: false);
    public static readonly SymbolParity Odd = new("odd", zeroesNyquist: true);

    public bool ZeroesNyquist { get; }

    // Parity composes by XOR (even∘even = even, odd∘odd = even, mixed = odd); an odd composite zeroes the Nyquist bin (odd symbols are discontinuous across ±Nyquist).
    public SymbolParity Compose(SymbolParity other) => this == other ? Even : Odd;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SpectralSymbol {
    // σ(k): the Fourier multiplier, Hermitian-conjugate symmetric (σ(−k) = conj σ(k)) so it maps real to real;
    // Antiderivatives kill the undetermined zero mode instead of dividing by zero.
    public static readonly SpectralSymbol Derivative = new("derivative", symbol: static k => new Complex(0.0, k), parity: SymbolParity.Odd);
    public static readonly SpectralSymbol Laplacian = new("laplacian", symbol: static k => new Complex(-k * k, 0.0), parity: SymbolParity.Even);
    public static readonly SpectralSymbol Biharmonic = new("biharmonic", symbol: static k => new Complex(k * k * k * k, 0.0), parity: SymbolParity.Even);
    public static readonly SpectralSymbol Hilbert = new("hilbert", symbol: static k => new Complex(0.0, -Math.Sign(k)), parity: SymbolParity.Odd);
    public static readonly SpectralSymbol AntiDerivative = new("anti-derivative", symbol: static k => k == 0.0 ? Complex.Zero : new Complex(0.0, -1.0 / k), parity: SymbolParity.Odd);

    private readonly Func<double, Complex> symbol;

    public SymbolParity Parity { get; }

    public Complex At(double wavenumber) => symbol(wavenumber);
}

public sealed class Spectral {
    private Spectral(Seq<SpectralSymbol> factors) => Factors = factors;

    public Seq<SpectralSymbol> Factors { get; }

    public static Spectral Of(SpectralSymbol symbol) => new(Seq(symbol));

    public Spectral Then(SpectralSymbol next) => new(Factors.Add(next));

    public SymbolParity Parity => Factors.Fold(SymbolParity.Even, static (acc, factor) => acc.Compose(factor.Parity));

    public Complex At(double wavenumber) => Factors.Fold(Complex.One, (acc, factor) => acc * factor.At(wavenumber));
}

public readonly record struct WaveAxis(int Length, double Extent) {
    // `FrequencyScale` supplies transform-order cycles per unit; scaling by `2π` yields angular wavenumbers
    // without a hand-indexed bin walk.
    public double[] K() =>
        [.. Fourier.FrequencyScale(Length, Length / Extent).Select(static cycles => 2.0 * Math.PI * cycles)];

    public int Nyquist => (Length & 1) == 0 ? Length >> 1 : -1;
}

// --- [MODELS] ------------------------------------------------------------------------------
// ImaginaryResidual is Some only on the complex (odd-length) leg — the packed-real leg is real by
// construction, so its Hermitian gate is structural and the channel reports honest absence.
public sealed record SpectralEvidence(double[] Field, Option<double> ImaginaryResidual, SymbolParity Parity);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class SpectralOperator {
    // Field length selects packed-real even transforms or complex odd transforms; the latter's imaginary
    // residual diagnoses broken Hermitian symmetry.
    public static Fin<SpectralEvidence> Apply(double[] field, WaveAxis axis, Spectral op) =>
        axis.Length < 2 || !double.IsFinite(axis.Extent) || axis.Extent <= 0.0 || field.Length != axis.Length || !toSeq(field).ForAll(double.IsFinite)
            ? Fin.Fail<SpectralEvidence>(new ComputeFault.ModelRejected($"<wave-axis-mismatch:field={field.Length}:axis={axis.Length}>"))
            : (field.Length & 1) == 0
                ? PackedReal(field, axis, op)
                : ComplexLeg(field, axis, op);

    // `ForwardReal` packs `n` samples into `n+2` interleaved bin components; the symbol multiplies each pair,
    // odd parity zeroes Nyquist, and `InverseReal` returns without complex materialization.
    static Fin<SpectralEvidence> PackedReal(double[] field, WaveAxis axis, Spectral op) {
        double[] k = axis.K();
        bool killNyquist = op.Parity.ZeroesNyquist;
        int half = axis.Length >> 1;
        double[] packed = new double[axis.Length + 2];
        field.CopyTo(packed, 0);
        Fourier.ForwardReal(packed, axis.Length, FourierOptions.AsymmetricScaling);
        for (int bin = 0; bin <= half; bin++) {
            Complex factor = killNyquist && bin == half ? Complex.Zero : op.At(k[bin]);
            Complex scaled = new Complex(packed[2 * bin], packed[2 * bin + 1]) * factor;
            (packed[2 * bin], packed[2 * bin + 1]) = (scaled.Real, scaled.Imaginary);
        }

        Fourier.InverseReal(packed, axis.Length, FourierOptions.AsymmetricScaling);
        return Fin.Succ(new SpectralEvidence(packed[..axis.Length], None, op.Parity));
    }

    static Fin<SpectralEvidence> ComplexLeg(double[] field, WaveAxis axis, Spectral op) {
        double[] k = axis.K();
        int nyquist = axis.Nyquist;
        bool killNyquist = op.Parity.ZeroesNyquist;
        Complex[] spectrum = new Complex[field.Length];
        for (int i = 0; i < spectrum.Length; i++) { spectrum[i] = new Complex(field[i], 0.0); }

        Fourier.Forward(spectrum, FourierOptions.AsymmetricScaling);
        for (int i = 0; i < spectrum.Length; i++) {
            spectrum[i] *= killNyquist && i == nyquist ? Complex.Zero : op.At(k[i]);
        }

        Fourier.Inverse(spectrum, FourierOptions.AsymmetricScaling);
        double[] result = new double[spectrum.Length];
        (double real, double imaginary) = (0.0, 0.0);
        for (int i = 0; i < spectrum.Length; i++) {
            result[i] = spectrum[i].Real;
            (real, imaginary) = (Math.Max(real, Math.Abs(spectrum[i].Real)), Math.Max(imaginary, Math.Abs(spectrum[i].Imaginary)));
        }

        return imaginary / Math.Max(real, double.Epsilon) is var residual && residual < 1e-8
            ? Fin.Succ(new SpectralEvidence(result, Some(residual), op.Parity))
            : Fin.Fail<SpectralEvidence>(new ComputeFault.ModelRejected($"<imaginary-residual:r={residual:e3}>"));
    }
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

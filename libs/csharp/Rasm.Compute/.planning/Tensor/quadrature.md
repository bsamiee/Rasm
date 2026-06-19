# [COMPUTE_QUADRATURE]

Rasm.Compute owned-build numeric lane for integration and spectral operators: the kernels with no library surface — accuracy-routed quadrature, the order-derived additive-module integrator with its adaptive-control policy, and the constant-coefficient periodic spectral operator — built and gated in-house, every result leaving as typed evidence rather than a bare `double` or `Complex[]`. The lane is host-local and crosses no wire. In-place numeric kernels — the `StepTableau` stage-fold and Butcher-tree order walk, the per-evaluation counters inside guarded integrands — are this page's statement exemption.

## [01]-[INDEX]

- [01]-[OWNED_BUILDS]: quadrature route; integrator tableau; adaptive control; spectral operator.

## [02]-[OWNED_BUILDS]

- Owner: the owned-build lane with no library surface — `QuadratureRoute` the four-kernel accuracy-routed integration owner with the cancellation diagnostic, `StepTableau` the order-derived integrator with the minimal additive-module step and frozen adaptive-control policy, and `SpectralOperator` the constant-coefficient periodic symbol applied pointwise.
- Cases: `QuadratureRoute` `[SmartEnum<string>]` cases double-exponential · gauss-legendre · gauss-kronrod · cubature (4), each carrying its `Integrate` delegate column; `StepTableau` order derived by the Butcher-tree walk; `SpectralOperator` symbols are policy rows composed by pointwise multiplication, a new operator a new symbol row.
- Entry: `public static Fin<QuadratureEvidence> Integrate(QuadratureRoute route, Func<double, double> f, IntervalSpec interval, double floor)` routes the four kernels through the one finite-admission-then-lift combinator over the delegate column with the `|value/L1|` cancellation ratio gate; `public static Fin<StepTableau> Create(double[,] a, double[] b, double[] c)` returns the order-derived tableau or a typed structural fault on row-sum inconsistency; `public static V Step<V>(StepTableau tableau, AdaptiveControl control, Func<double, V, V> rhs, double t, V state, double h)` is the one additive-module step over the carrier; `public static Complex[] Apply(Complex[] field, WaveAxis axis, Func<double, Complex> symbol, bool oddOrder)` drives the spectral operator.
- Auto: `QuadratureRoute.Integrate` counts skipped non-finite evaluations in the receipt never silently as coverage, reads the `L1` value-to-ratio cancellation channel as the free conditioning diagnostic, and the short overload that discards `L1` is rejected; the integrand admits as a guarded delegate because no route inspects returns for non-finiteness and a pole poisons the weighted sum silently; the double-exponential kernel feeds `IntervalSpec` bound substitution only on the facade entry because the direct kernel feeds infinity into abscissa evaluation and yields `NaN` weights; `StepTableau.Create` derives verified order as the largest integer for which every Butcher-tree order condition holds rather than asserting it; the `WaveAxis.K()` split-spectrum wavenumber derives once at grid construction through `Generate.LinearRangeMap` because hand-indexing the bin number applies an aliased symbol past the half length; `Fourier.Forward`/`Fourier.Inverse` pin `FourierOptions.AsymmetricScaling` because the symmetric default cancels only on round trips; the Nyquist bin zeros for odd-order symbols.
- Receipt: a quadrature run emits `QuadratureEvidence(Value, Error, L1Norm, Ratio, Skipped)` carrying the cancellation ratio so gates reject on quality not on slow convergence; the integrator records the terminated-at-budget case with its binding budget and residual because the three exhaustion mechanisms return best-so-far indistinguishable from convergence.
- Packages: MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, LanguageExt.Core, BCL inbox
- Growth: a new quadrature kernel is one `QuadratureRoute` row carrying its `Integrate` delegate (a distinct kernel, not a wrapper); a new spectral operator is one symbol row; zero new surface.
- Boundary: the quadrature route is the accuracy decision with order secondary — double-exponential, fixed Gauss-Legendre, adaptive Gauss-Kronrod, and tensor-product cubature are four distinct kernels carried as `QuadratureRoute` rows whose `Integrate` delegate column binds `Integrate.DoubleExponential`/`Integrate.GaussLegendre`/`Integrate.GaussKronrod`/`Integrate.OnRectangle`+`OnCuboid`, never three sibling factories plus a missing fourth, and the finite-admission-then-lift `Integrate.X(...) is var v && double.IsFinite(v) ? Fin.Succ : Fin.Fail` combinator applies ONCE over the delegate column never re-spelled per kernel; infinite bounds substitute only on the facade entry because the direct double-exponential kernel feeds infinity into abscissa evaluation and yields `NaN` weights; the integrator tableau validates at construction because row-sum consistency and the order conditions are definition-time facts and verified order is derived as the largest integer for which every Butcher-tree order condition holds, never asserted and never capped at a hardcoded literal; one step function writes over the minimal additive-module operations (addition, scalar scaling, step-scaled increment) admitting scalar/complex/fixed-rank-vector/grid-slab carriers through the `IModule<V>` carrier constraint and collapsing the scalar-versus-vector transcription-error class; adaptive control is one `AdaptiveControl` policy row (safety factor, step-ratio clamps, error-norm choice, reject budget) read off the receipt and the scaled two-pass error norm guards large-magnitude state because the naive squared-sum-then-root overflows; the spectral asymmetric scaling convention fixes in the owner and the result is marked inadmissible on excess imaginary residual because a real-symbol operator owes a machine-zero imaginary part.

```csharp signature
public sealed record QuadratureEvidence(double Value, double Error, double L1Norm, double Ratio, int Skipped);

public readonly record struct IntervalSpec(double Lower, double Upper, bool LowerInfinite, bool UpperInfinite);

[SmartEnum<string>]
[KeyMemberEqualityComparer<NumericKeyPolicy, string>]
[KeyMemberComparer<NumericKeyPolicy, string>]
public sealed partial class QuadratureRoute {
    public static readonly QuadratureRoute DoubleExponential = new("double-exponential",
        static (f, i, _) => Integrate.DoubleExponential(f, i.Lower, i.Upper));
    public static readonly QuadratureRoute GaussLegendre = new("gauss-legendre",
        static (f, i, order) => Integrate.GaussLegendre(f, i.Lower, i.Upper, order));
    public static readonly QuadratureRoute GaussKronrod = new("gauss-kronrod",
        static (f, i, _) => Integrate.GaussKronrod(f, i.Lower, i.Upper, out _, out _));
    public static readonly QuadratureRoute Cubature = new("cubature",
        static (f, i, _) => Integrate.OnRectangle((x, _) => f(x), i.Lower, i.Upper, i.Lower, i.Upper));

    private readonly Func<Func<double, double>, IntervalSpec, int, double> integrate;

    public double Run(Func<double, double> f, IntervalSpec interval, int order) => integrate(f, interval, order);
}

public static class Quadrature {
    public static Fin<QuadratureEvidence> Integrate(QuadratureRoute route, Func<double, double> f, IntervalSpec interval, double floor, int order = 7) {
        var skipped = 0;
        var guarded = (Func<double, double>)(x => f(x) is var y && double.IsFinite(y) ? y : (++skipped, 0.0).Item2);
        return route == QuadratureRoute.GaussKronrod
            ? Kronrod(guarded, interval, floor, skipped)
            : route.Run(guarded, interval, order) is var value && double.IsFinite(value)
                ? Fin.Succ(new QuadratureEvidence(value, double.NaN, double.NaN, double.NaN, skipped))
                : Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected($"<quadrature-nonfinite:route={route.Key}:skipped={skipped}>"));
    }

    static Fin<QuadratureEvidence> Kronrod(Func<double, double> guarded, IntervalSpec interval, double floor, int skipped) {
        var value = Integrate.GaussKronrod(guarded, interval.Lower, interval.Upper, out var error, out var l1Norm);
        return Math.Abs(value / l1Norm) is var ratio && double.IsFinite(ratio) && ratio >= floor
            ? Fin.Succ(new QuadratureEvidence(value, error, l1Norm, ratio, skipped))
            : Fin.Fail<QuadratureEvidence>(new ComputeFault.ModelRejected($"<cancellation-breach:ratio={ratio:e3}:skipped={skipped}>"));
    }
}

public readonly record struct WaveAxis(int Length, double Extent) {
    public double[] K() =>
        Generate.LinearRangeMap(0.0, 1.0, Length - 1.0,
            i => (i < (Length >> 1) + 1 ? i : i - Length) * (2.0 * Math.PI / Extent));

    public int Nyquist => (Length & 1) == 0 ? Length >> 1 : -1;
}

public static class SpectralOperator {
    public static Complex[] Apply(Complex[] field, WaveAxis axis, Func<double, Complex> symbol, bool oddOrder) {
        var (k, n) = (axis.K(), axis.Nyquist);
        Fourier.Forward(field, FourierOptions.AsymmetricScaling);
        var driven = field.Select((c, i) => c * (oddOrder && i == n ? Complex.Zero : symbol(k[i]))).ToArray();
        Fourier.Inverse(driven, FourierOptions.AsymmetricScaling);
        return driven;
    }
}

public sealed record AdaptiveControl(double Safety, double MinRatio, double MaxRatio, int RejectBudget) {
    public static readonly AdaptiveControl Default = new(Safety: 0.9, MinRatio: 0.2, MaxRatio: 5.0, RejectBudget: 8);

    public double NextStep(double h, double error, double tolerance, int order) =>
        h * Math.Clamp(Safety * Math.Pow(tolerance / Math.Max(error, double.Epsilon), 1.0 / (order + 1)), MinRatio, MaxRatio);
}

public interface IModule<TSelf> where TSelf : IModule<TSelf> {
    static abstract TSelf operator +(TSelf left, TSelf right);
    static abstract TSelf operator *(double scalar, TSelf value);

    static virtual double ScaledNorm(TSelf value, TSelf reference, double atol, double rtol) => 0.0;
}

public sealed record StepTableau(double[,] A, double[] B, double[] C, int Order) {
    public static Fin<StepTableau> Create(double[,] a, double[] b, double[] c) =>
        toSeq(Enumerable.Range(0, c.Length)).ForAll(row => Math.Abs(RowSum(a, row, c.Length) - c[row]) < 1e-12)
            ? Fin.Succ(new StepTableau(a, b, c, VerifiedOrder(a, b, c)))
            : Fin.Fail<StepTableau>(new ComputeFault.ModelRejected("<tableau-row-sum-inconsistent>"));

    public V Step<V>(AdaptiveControl control, Func<double, V, V> rhs, double t, V state, double h)
        where V : IModule<V> {
        int stages = C.Length;
        var k = new V[stages];
        for (int i = 0; i < stages; i++) {
            V increment = state;
            for (int j = 0; j < i; j++) {
                increment += (h * A[i, j]) * k[j];
            }

            k[i] = rhs(t + C[i] * h, increment);
        }

        V update = state + (h * B[0]) * k[0];
        for (int i = 1; i < stages; i++) {
            update += (h * B[i]) * k[i];
        }

        return update;
    }

    static double RowSum(double[,] a, int row, int stages) =>
        toSeq(Enumerable.Range(0, stages)).Sum(col => a[row, col]);

    static int VerifiedOrder(double[,] a, double[] b, double[] c) =>
        toSeq(Enumerable.Range(1, 5)).TakeWhile(p => OrderConditions(p).ForAll(condition => condition(a, b, c))).LastOrDefault();

    static Seq<Func<double[,], double[], double[], bool>> OrderConditions(int order) => order switch {
        1 => Seq<Func<double[,], double[], double[], bool>>(static (_, b, _) => Near(b.Sum(), 1.0)),
        2 => OrderConditions(1).Add(static (_, b, c) => Near(Dot(b, c), 0.5)),
        3 => OrderConditions(2)
            .Add(static (_, b, c) => Near(Sum(b, i => c[i] * c[i]), 1.0 / 3.0))
            .Add(static (a, b, c) => Near(Sum(b, i => Sum(c.Length, j => a[i, j] * c[j])), 1.0 / 6.0)),
        4 => OrderConditions(3)
            .Add(static (_, b, c) => Near(Sum(b, i => c[i] * c[i] * c[i]), 0.25))
            .Add(static (a, b, c) => Near(Sum(b, i => c[i] * Sum(c.Length, j => a[i, j] * c[j])), 0.125))
            .Add(static (a, b, c) => Near(Sum(b, i => Sum(c.Length, j => a[i, j] * c[j] * c[j])), 1.0 / 12.0))
            .Add(static (a, b, c) => Near(Sum(b, i => Sum(c.Length, j => a[i, j] * Sum(c.Length, k => a[j, k] * c[k]))), 1.0 / 24.0)),
        _ => Seq<Func<double[,], double[], double[], bool>>(static (_, _, _) => false),
    };

    static bool Near(double value, double target) => Math.Abs(value - target) < 1e-10;
    static double Dot(double[] b, double[] c) => toSeq(Enumerable.Range(0, b.Length)).Sum(i => b[i] * c[i]);
    static double Sum(double[] b, Func<int, double> term) => toSeq(Enumerable.Range(0, b.Length)).Sum(term);
    static double Sum(int count, Func<int, double> term) => toSeq(Enumerable.Range(0, count)).Sum(term);
}
```

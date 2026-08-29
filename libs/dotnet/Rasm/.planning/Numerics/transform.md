# [RASM_NUMERICS_TRANSFORM]

`Rasm.Numerics` owns the branch algorithms and admission boundaries that remain around MathNet's transform surface: Kaiser and Bohman tapers, finite-result interpolation, the separable `CellLattice` Fourier walk, and one-axis tap convolution. Package-owned windows and one-dimensional Fourier layouts stay direct at their consumers.

Rebuilds compose the `Rasm.Domain` types as the validity floor and `Numerics/atoms` as the addressing floor: `CellLattice` is the ONE linearization a lattice-backed plane addresses through, `Dimension`/`PositiveMagnitude`/`UnitInterval`/`SignedAxis` carry every extent, rate, fraction, and axis, `EpsilonPolicy` anchors the two floors this band reads. MathNet's managed provider is the pinned realization — the multidim entrypoints are unservable by construction and the separable row-column fold IS the platform-total N-dimensional transform.

## [01]-[INDEX]

- [02]-[WINDOW]: the two branch-owned taper algorithms MathNet omits.
- [03]-[INTERPOLATE]: one finite-result interpolant over MathNet's runtime capability flags.
- [04]-[SPECTRAL]: transform direction, the separable lattice transform, and admitted tap-series convolution.

## [02]-[WINDOW]

- Owner: `Taper` owns the Kaiser and Bohman coefficient arrays MathNet omits; every package-backed symmetric or periodic form stays at its `MathNet.Numerics.Window` factory.
- Entry: `Taper.Kaiser(width, beta)` and `Taper.Bohman(width)` take the exact parameters their formulas consume and return finite coefficient arrays on `Fin`.
- Auto: every sampled coefficient run gates whole through `TensorPrimitives.IsFiniteAll` before it leaves the entry, so an admitted but extreme Kaiser beta whose `I0` ratio overflows refuses typed instead of shipping a success-shaped `NaN`.
- Packages: MathNet.Numerics (`SpecialFunctions.BesselI0` the Kaiser evaluation), System.Numerics.Tensors (`TensorPrimitives.IsFiniteAll` the sampled-result gate), `Numerics/atoms` (`Dimension`, `PositiveMagnitude`), LanguageExt.Core, BCL inbox.
- Growth: a branch-owned taper is one direct operation carrying its own formula and parameters; package-backed tapers remain direct package calls.
- Boundary: these operations publish endpoint-aligned arrays. A continuous tap-grid window or FFT framing policy belongs to its consumer and does not widen this owner.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Linq;
using System.Numerics.Tensors;
using LanguageExt;
using MathNet.Numerics;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Taper {
    public static Fin<Arr<double>> Kaiser(Dimension width, PositiveMagnitude beta) =>
        Try.lift(() => width.Value is 1
                ? new Arr<double>([1.0])
                : new Arr<double>([.. Enumerable.Range(0, width.Value).Select(n => {
                    double span = width.Value - 1.0;
                    double x = (2.0 * n / span) - 1.0;
                    return SpecialFunctions.BesselI0(beta.Value * Math.Sqrt(Math.Max(0.0, 1.0 - (x * x))))
                        / SpecialFunctions.BesselI0(beta.Value);
                })]))
            .Run().Bind(Finite);

    public static Fin<Arr<double>> Bohman(Dimension width) =>
        Try.lift(() => width.Value is 1
                ? new Arr<double>([1.0])
                : new Arr<double>([.. Enumerable.Range(0, width.Value).Select(n => {
                    double x = Math.Abs((2.0 * n / (width.Value - 1.0)) - 1.0);
                    return ((1.0 - x) * Math.Cos(Math.PI * x)) + (Math.Sin(Math.PI * x) / Math.PI);
                })]))
            .Run().Bind(Finite);

    private static Fin<Arr<double>> Finite(Arr<double> samples) =>
        TensorPrimitives.IsFiniteAll<double>(samples.AsSpan())
            ? Fin.Succ(samples)
            : Fin.Fail<Arr<double>>(new KernelFault.InvalidResult());
}
```

## [03]-[INTERPOLATE]

- Owner: `Interpolant` is the fitted-curve capsule over `IInterpolation`, retaining the package curve privately and publishing finite-result reads.
- Entry: `Fit(points, values, factory)` admits aligned, finite, strictly ascending samples, captures the selected MathNet `*Sorted` factory, and returns `Fin<Interpolant>`; `Evaluate`, `Derivative`, `SecondDerivative`, and `Integrate` are the four reads.
- Auto: `Fit` accumulates every column and ordering refusal through `Admit.Claims`; each read passes the package result through `Acceptance.Value`, and calculus reads consult the orthogonal `SupportsDifferentiation` and `SupportsIntegration` flags before calling the package member.
- Law: the selected package algorithm is explicit at the caller as a method group. Algorithm-named forwarding factories, capability markers, and speculative special-case mints do not widen this owner.
- Packages: MathNet.Numerics (`IInterpolation` and the caller-selected `*Sorted` factories), System.Numerics.Tensors (`TensorPrimitives.IsFiniteAll`), `Numerics/atoms`, Rasm.Domain (`Admit.Claims`, `Acceptance.Value`), LanguageExt.Core, BCL inbox.
- Growth: a new scheme is a package method group passed to `Fit`; a genuinely different raw admission shape remains at the consumer that owns it.
- Boundary: `MathNet.Numerics.Interpolation` is one-dimensional whole, so bicubic or scattered-surface reconstruction belongs to its own route.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Linq;
using System.Numerics.Tensors;
using LanguageExt;
using MathNet.Numerics.Interpolation;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class Interpolant {
    private Interpolant(IInterpolation curve) => Curve = curve;
    private IInterpolation Curve { get; }

    public Fin<double> Evaluate(double t) => Acceptance.Value(Curve.Interpolate(t));
    public Fin<double> Derivative(double t) => Curve.SupportsDifferentiation
        ? Acceptance.Value(Curve.Differentiate(t))
        : Fin.Fail<double>(new KernelFault.Unsupported(InputType: Curve.GetType(), OutputType: typeof(double)));
    public Fin<double> SecondDerivative(double t) => Curve.SupportsDifferentiation
        ? Acceptance.Value(Curve.Differentiate2(t))
        : Fin.Fail<double>(new KernelFault.Unsupported(InputType: Curve.GetType(), OutputType: typeof(double)));
    public Fin<double> Integrate(double upper, Option<double> lower = default) => Curve.SupportsIntegration
        ? Acceptance.Value(lower.Match(Some: a => Curve.Integrate(a, upper), None: () => Curve.Integrate(upper)))
        : Fin.Fail<double>(new KernelFault.Unsupported(InputType: Curve.GetType(), OutputType: typeof(double)));

    public static Fin<Interpolant> Fit(
        Arr<double> points,
        Arr<double> values,
        Func<double[], double[], IInterpolation> factory) =>
        Admit.Claims((points.Count >= 2, "points-extent"),
                (points.Count == values.Count, "sample-arity"),
                (TensorPrimitives.IsFiniteAll<double>(points.AsSpan()), "points-finite"),
                (TensorPrimitives.IsFiniteAll<double>(values.AsSpan()), "values-finite"),
                (factory is not null, "factory"),
                (points.Count < 2 || Enumerable.Range(1, points.Count - 1).All(i => points[i - 1] < points[i]), "points-ascending"))
            .Bind(_ => Try.lift(() => new Interpolant(factory([.. points.AsIterable()], [.. values.AsIterable()]))).Run());

}
```

## [04]-[SPECTRAL]

- Owner: `TransformDirection` binds the two in-place complex Fourier operations; `Spectral` owns the separable `CellLattice` walk; `TapSeries` owns the admitted odd, finite, unit-sum tap array and its one-axis fold; `TapWindow` owns admitted staging geometry; `TapBorder` owns out-of-extent resolution.
- Entry: `Spectral.Transform(samples, lattice, direction, options)` transforms an admitted complex lattice in place; `TapSeries.Of(taps)` mints a normalized kernel, and `series.Convolve(source, folded, window, border)` folds one strided axis.
- Auto: the lattice transform gathers, transforms, and scatters each axis through the lattice's own extents and strides, then finite-gates the mutated array. The tap fold rejects non-finite source material, divides every output lane by its resolved-weight sum, and refuses a cancelled, overflowed, or non-finite result.
- Law: package-owned one-dimensional layouts, frequency axes, power projections, and pointwise multiplication remain direct at their consumers. This owner retains only the branch-owned separable lattice walk and sample-domain tap fold.
- Packages: MathNet.Numerics (`Fourier.Forward`/`Inverse`, `FourierOptions`), System.Numerics.Tensors (`TensorPrimitives.Multiply`/`MultiplyAdd`/`Divide`/`Sum`/`IsFiniteAll`), `Numerics/atoms` (`CellLattice`, `Dimension`, `PositiveMagnitude`, `EpsilonPolicy`), Rasm.Domain (`Admit.Claims`, `Admit.FiniteComplexSpan`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL (`System.Numerics.Complex`).
- Growth: a transform direction or border law is one smart-enum row; a new package buffer layout remains at the consumer that owns it rather than widening this band with a carrier union.
- Boundary: `Spectral.Transform` is the only multidimensional Fourier boundary because MathNet's managed multidimensional provider path is unavailable. Direct packed, split, and one-dimensional transforms stay at consumers. Tap generation remains consumer policy; this band admits and applies the resulting series without a speculative lattice-convolution shell.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Numerics;
using System.Numerics.Tensors;
using LanguageExt;
using MathNet.Numerics.IntegralTransforms;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class TransformDirection {
    public static readonly TransformDirection Forward = new(apply: Fourier.Forward);
    public static readonly TransformDirection Inverse = new(apply: Fourier.Inverse);

    [UseDelegateFromConstructor]
    internal partial void Apply(Complex[] samples, FourierOptions options);
}

[SmartEnum]
public sealed partial class TapBorder {
    public static readonly TapBorder Clamp = new(resolve: static (index, extent) => Some((int)Math.Clamp(value: index, min: 0L, max: (long)extent - 1)));
    public static readonly TapBorder Wrap = new(resolve: static (index, extent) => Some((int)(((index % extent) + extent) % extent)));
    public static readonly TapBorder Mirror = new(resolve: static (index, extent) => {
        long period = Math.Max(val1: 1L, val2: ((long)extent - 1) * 2);
        long folded = ((index % period) + period) % period;
        return Some((int)(folded < extent ? folded : period - folded));
    });
    public static readonly TapBorder Omit = new(resolve: static (_, _) => Option<int>.None);

    [UseDelegateFromConstructor] public partial Option<int> Resolve(long index, int extent);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record TapWindow {
    private TapWindow(int extent, int origin, int from, int run, int stride) =>
        (Extent, Origin, From, Run, Stride) = (extent, origin, from, run, stride);
    public int Extent { get; }
    public int Origin { get; }
    public int From { get; }
    public int Run { get; }
    public int Stride { get; }

    public static Fin<TapWindow> Of(Dimension extent, Dimension stride, int origin, int from, Dimension run) =>
        Admit.Claims((origin >= 0, "origin"),
                (from >= 0, "from"),
                (origin <= from, "origin-precedes-from"),
                (run.Value <= extent.Value - from, "run-within-extent"))
            .Map(_ => new TapWindow(extent.Value, origin, from, run.Value, stride.Value));
}

public sealed record TapSeries {
    private TapSeries(Arr<double> taps) => Taps = taps;

    public Arr<double> Taps { get; }
    public int Radius => Taps.Count / 2;

    public static Fin<TapSeries> Of(Arr<double> taps) {
        double sum = TensorPrimitives.Sum<double>(taps.AsSpan());
        if (taps.Count < 1 || int.IsEvenInteger(taps.Count) || !TensorPrimitives.IsFiniteAll<double>(taps.AsSpan()) || !double.IsFinite(sum) || Math.Abs(sum) <= EpsilonPolicy.ZeroTolerance) { return Fin.Fail<TapSeries>(new KernelFault.InvalidInput()); }
        double[] normalized = new double[taps.Count];
        TensorPrimitives.Divide<double>(taps.AsSpan(), sum, normalized);
        return TensorPrimitives.IsFiniteAll<double>(normalized) ? Fin.Succ(new TapSeries(new Arr<double>(normalized))) : Fin.Fail<TapSeries>(new KernelFault.InvalidResult());
    }

    public Fin<Unit> Convolve(ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border) {
        if (window is null) { return Fin.Fail<Unit>(new KernelFault.InvalidInput()); }
        int stride = window.Stride;
        int staged = source.Length / stride;
        bool whole = window.Origin == 0 && staged == window.Extent;
        Fin<Unit> admitted = Admit.Claims((TensorPrimitives.IsFiniteAll<double>(source), "source-finite"),
            (border is not null, "border"),
            (source.Length == staged * stride, "source-extent"),
            (folded.Length == (long)window.Run * stride, "folded-extent"),
            ((long)window.Origin <= Math.Max(0L, (long)window.From - Radius), "staging-head"),
            ((long)window.Origin + staged > Math.Min((long)window.Extent - 1, (long)window.From + window.Run - 1 + Radius), "staging-tail"),
            (whole || border == TapBorder.Omit, "partial-window-border"));
        if (admitted.IsFail) { return admitted; }
        return ConvolveCore(source, folded, window, border);
    }

    private Fin<Unit> ConvolveCore(ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border) {
        ReadOnlySpan<double> taps = Taps.AsSpan();
        int radius = Radius, stride = window.Stride;
        for (int at = 0; at < window.Run; at++) {
            int record = window.From + at, seat = at * stride;
            Span<double> lane = folded.Slice(seat, stride);
            lane.Clear();
            double admitted = 0.0;
            for (int tap = -radius; tap <= radius; tap++) {
                long logical = (long)record + tap;
                Option<int> resolved = logical >= 0L && logical < window.Extent
                    ? Some((int)logical)
                    : border.Resolve(index: logical, extent: window.Extent);
                if (resolved is not { IsSome: true, Case: int sample }) { continue; }
                double weight = taps[tap + radius];
                admitted += weight;
                int sourceOffset = (sample - window.Origin) * stride;
                TensorPrimitives.MultiplyAdd<double>(source.Slice(sourceOffset, stride), weight, lane, lane);
            }
            if (!double.IsFinite(admitted) || Math.Abs(value: admitted) <= EpsilonPolicy.ZeroTolerance) {
                return Fin.Fail<Unit>(new KernelFault.InvalidResult(Detail: Some($"resolved tap-weight sum invalid at record {record}")));
            }
            TensorPrimitives.Multiply<double>(lane, 1.0 / admitted, lane);
            if (!TensorPrimitives.IsFiniteAll<double>(lane)) { return Fin.Fail<Unit>(new KernelFault.InvalidResult()); }
        }
        return Fin.Succ(unit);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Spectral {
    public static Fin<Unit> Transform(Complex[] samples, CellLattice lattice, TransformDirection direction, FourierOptions options) =>
        Admit.Claims((samples is not null && Admit.FiniteComplexSpan(samples), "samples"),
                (lattice.CellCount >= 1L && samples is not null && samples.LongLength == lattice.CellCount, "lattice"),
                (direction is not null, "direction"))
            .Bind(_ => Try.lift(() => {
                int cells = samples.Length;
                for (int axis = 0; axis < lattice.Rank; axis++) {
                    int count = lattice.Extent(axis).Value;
                    int stride = lattice.Stride(axis);
                    Complex[] line = new Complex[count];
                    for (int origin = 0; origin < cells; origin++) {
                        if (origin / stride % count != 0) { continue; }
                        for (int k = 0; k < count; k++) { line[k] = samples[origin + (k * stride)]; }
                        direction.Apply(line, options);
                        for (int k = 0; k < count; k++) { samples[origin + (k * stride)] = line[k]; }
                    }
                }
                return unit;
            }).Run())
            .Bind(_ => Admit.FiniteComplexSpan(samples) ? Fin.Succ(unit) : Fin.Fail<Unit>(new KernelFault.InvalidResult()));
}
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

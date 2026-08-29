# 1. Delete the taper request unions

From: [transform.md:37](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:37)
```csharp
[Union]
public abstract partial record TaperSampling {
    private TaperSampling() { }
    public sealed record Symmetric(Option<TaperShape> Shape) : TaperSampling;
    public sealed record Periodic : TaperSampling;
}

[Union]
public abstract partial record TaperShape {
    private TaperShape() { }
    public sealed record Gaussian(PositiveMagnitude Sigma) : TaperShape;
    public sealed record Tukey(UnitInterval Fraction) : TaperShape;
    public sealed record Kaiser(PositiveMagnitude Beta) : TaperShape;
}
```
To:
```csharp
// TaperSampling and TaperShape DELETED
```
Why: The package-backed symmetric forms are direct `MathNet.Numerics.Window` calls, the only actual roster consumer requests periodic windows, and the two retained branch-owned tapers take their parameters directly. The unions therefore encode no shared invariant or case family that survives the roster collapse.

Change: Delete both unions and pass `Dimension`, `PositiveMagnitude`, and other algorithm parameters directly to the retained taper operations.

Delta: -13 LOC; -2 module-level types and -5 nested case types; 0 members.

Ripples: In `libs/dotnet/Rasm.Compute/.planning/Stats/signal.md`, remove every `TaperSampling.Periodic` construction and make the signal-owned periodic-window policy sample directly. Remove the deleted union vocabulary from `libs/dotnet/Rasm/ARCHITECTURE.md` and `libs/dotnet/Rasm/RULINGS.md`.

# 2. Delete package-backed window rows

From: [transform.md:57](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:57)
```csharp
public static readonly WindowTaper Hann = new(periodicDesign: Some<Func<int, double[]>>(Window.HannPeriodic), design: Unparameterized(Window.Hann));
public static readonly WindowTaper Hamming = new(periodicDesign: Some<Func<int, double[]>>(Window.HammingPeriodic), design: Unparameterized(Window.Hamming));
public static readonly WindowTaper Cosine = new(periodicDesign: Some<Func<int, double[]>>(Window.CosinePeriodic), design: Unparameterized(Window.Cosine));
public static readonly WindowTaper Lanczos = new(periodicDesign: Some<Func<int, double[]>>(Window.LanczosPeriodic), design: Unparameterized(Window.Lanczos));
public static readonly WindowTaper Blackman = new(periodicDesign: None, design: Unparameterized(Window.Blackman));
public static readonly WindowTaper BlackmanHarris = new(periodicDesign: None, design: Unparameterized(Window.BlackmanHarris));
public static readonly WindowTaper BlackmanNuttall = new(periodicDesign: None, design: Unparameterized(Window.BlackmanNuttall));
public static readonly WindowTaper Nuttall = new(periodicDesign: None, design: Unparameterized(Window.Nuttall));
public static readonly WindowTaper FlatTop = new(periodicDesign: None, design: Unparameterized(Window.FlatTop));
public static readonly WindowTaper Bartlett = new(periodicDesign: None, design: Unparameterized(Window.Bartlett));
public static readonly WindowTaper BartlettHann = new(periodicDesign: None, design: Unparameterized(Window.BartlettHann));
public static readonly WindowTaper Triangular = new(periodicDesign: None, design: Unparameterized(Window.Triangular));
public static readonly WindowTaper Dirichlet = new(periodicDesign: None, design: Unparameterized(Window.Dirichlet));
public static readonly WindowTaper Gauss = new(
    periodicDesign: None, design: Parameterized(Window.Gauss, static shape => shape is TaperShape.Gaussian gaussian ? Some(gaussian.Sigma.Value) : None));
public static readonly WindowTaper Tukey = new(
    periodicDesign: None, design: Parameterized(Window.Tukey, static shape => shape is TaperShape.Tukey tukey ? Some(tukey.Fraction.Value) : None));
```
To:
```csharp
// WindowTaper package-backed rows DELETED
```
Why: All fifteen rows rename one catalogued MathNet factory. The abstraction adds selection, rejection, and allocation layers without owning an algorithm; the four periodic choices are policy of their sole signal consumer.

Change: Delete the fifteen rows. Call symmetric and shaped `Window` factories directly. Move only `HannPeriodic`, `HammingPeriodic`, `CosinePeriodic`, and `LanczosPeriodic` into the signal policy that dynamically selects among them.

Delta: -16 LOC; -15 module-level members; 0 types.

Ripples: In `libs/dotnet/Rasm.Compute/.planning/Stats/signal.md`, replace `WindowTaper` with a four-row local periodic-window policy bound directly to the four `Window.*Periodic` methods. Update the transform stacking note in `libs/dotnet/.api/api-mathnet-numerics.md` and remove the package-backed roster claim from `libs/dotnet/Rasm/ARCHITECTURE.md` and `libs/dotnet/Rasm/RULINGS.md`.

# 3. Retain only the branch-owned taper algorithms

From: [transform.md:74](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:74)
```csharp
public static readonly WindowTaper Kaiser = new(periodicDesign: None, design: Parameterized(static (width, beta) => {
    if (width is 1) { return [1.0]; }
    double norm = SpecialFunctions.BesselI0(x: beta), span = Math.Max(val1: 1, val2: width - 1);
    return [.. Enumerable.Range(start: 0, count: width).Select(n => SpecialFunctions.BesselI0(
        x: beta * Math.Sqrt(d: Math.Max(val1: 0.0, val2: 1.0 - Math.Pow(x: (2.0 * n / span) - 1.0, y: 2.0)))) / norm)];
}, static shape => shape is TaperShape.Kaiser kaiser ? Some(kaiser.Beta.Value) : None));
public static readonly WindowTaper Bohman = new(periodicDesign: None, design: Unparameterized(static width => {
    if (width is 1) { return [1.0]; }
    double span = Math.Max(val1: 1, val2: width - 1);
    return [.. Enumerable.Range(start: 0, count: width).Select(n => {
        double x = Math.Abs(value: (2.0 * n / span) - 1.0);
        return ((1.0 - x) * Math.Cos(d: Math.PI * x)) + (Math.Sin(a: Math.PI * x) / Math.PI);
    })];
}));
```
To:
```csharp
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
Why: Kaiser and Bohman are the only algorithms MathNet does not own. Giving them direct parameterized operations preserves that genuine capability while deleting the smart-enum delegate columns, periodic option, shape extraction, and invalid three-parameter helper lambdas.

Change: Replace `WindowTaper` with `Taper`, move the existing formulas into the two named operations, retain the finite-result gate, and delete `Design`, `PeriodicDesign`, `Sample`, `Unparameterized`, and `Parameterized`.

Delta: -7 LOC; -5 module-level members; 0 types.

Ripples: In `libs/dotnet/Rasm.Materials/.planning/Raster/plane.md`, rename the endpoint-window references to `Taper.Kaiser`. Remove `WindowTaper` from `libs/dotnet/Rasm/ARCHITECTURE.md` and `libs/dotnet/Rasm/RULINGS.md`.

# 4. Collapse interpolation capability markers onto MathNet flags

From: [transform.md:131](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:131)
```csharp
public interface IEvaluable { }
public interface IDifferentiable : IEvaluable { }
public interface IIntegrable : IEvaluable { }
public interface ICalculus : IDifferentiable, IIntegrable { }

public sealed class Interpolant<TCapability> where TCapability : IEvaluable {
    internal Interpolant(IInterpolation curve) => Curve = curve;
    internal IInterpolation Curve { get; }

    public Fin<double> Evaluate(double t) => Interpolant.AdmitFinite(value: Curve.Interpolate(t: t));
}
```
To:
```csharp
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
}
```
Why: `IInterpolation` already owns the two orthogonal capability facts. Four empty marker interfaces and a phantom generic duplicate those facts, while the wrapper remains useful only as the carrier-typed finite-result boundary.

Change: Merge the generic and static `Interpolant` declarations, move the three calculus reads onto the sealed result, gate them with `SupportsDifferentiation` or `SupportsIntegration`, and return `Fin<Interpolant>` from the remaining fit entry.

Delta: -8 LOC; -5 module-level types; 0 members.

Ripples: In `libs/dotnet/Rasm.Element/.planning/Composition/material.md`, delete the `CalculusInterpolant` alias and use `Interpolant`. In `libs/dotnet/Rasm.Compute/.planning/Tensor/sampling.md`, replace the marker-diamond ownership narrative with the package-flag gate. In `libs/dotnet/Rasm/.planning/Parametric/curve.md`, remove the compile-time calculus-tier claim. Update `libs/dotnet/Rasm/ARCHITECTURE.md` and `libs/dotnet/Rasm/RULINGS.md`.

# 5. Replace algorithm-named interpolation factories with one fit boundary

From: [transform.md:146](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:146)
```csharp
public static Fin<Interpolant<ICalculus>> NaturalCubicSpline(Arr<double> points, Arr<double> values) =>
    Build<ICalculus>(points, values, static (p, v) => CubicSpline.InterpolateNaturalSorted(p, v));
public static Fin<Interpolant<ICalculus>> AkimaSpline(Arr<double> points, Arr<double> values) =>
    Build<ICalculus>(points, values, static (p, v) => CubicSpline.InterpolateAkimaSorted(p, v));
public static Fin<Interpolant<ICalculus>> PchipSpline(Arr<double> points, Arr<double> values) =>
    Build<ICalculus>(points, values, static (p, v) => CubicSpline.InterpolatePchipSorted(p, v));
public static Fin<Interpolant<ICalculus>> CubicHermiteSpline(Arr<double> points, Arr<double> values, Arr<double> slopes) =>
    Build<ICalculus>(points, values, (p, v) => CubicSpline.InterpolateHermiteSorted(p, v, [.. slopes.AsIterable()]), slopes: Some(slopes));
public static Fin<Interpolant<ICalculus>> LinearSpline(Arr<double> points, Arr<double> values) =>
    Build<ICalculus>(points, values, static (p, v) => MathNet.Numerics.Interpolation.LinearSpline.InterpolateSorted(p, v));
public static Fin<Interpolant<ICalculus>> StepInterpolation(Arr<double> points, Arr<double> values) =>
    Build<ICalculus>(points, values, static (p, v) => MathNet.Numerics.Interpolation.StepInterpolation.InterpolateSorted(p, v));
public static Fin<Interpolant<IDifferentiable>> NevillePolynomial(Arr<double> points, Arr<double> values) =>
    Build<IDifferentiable>(points, values, static (p, v) => NevillePolynomialInterpolation.InterpolateSorted(p, v));
public static Fin<Interpolant<IDifferentiable>> LogLinearSpline(Arr<double> points, Arr<double> values) =>
    Build<IDifferentiable>(points, values, static (p, v) => MathNet.Numerics.Interpolation.LogLinear.InterpolateSorted(p, v));
public static Fin<Interpolant<IEvaluable>> FloaterHormannRational(Arr<double> points, Arr<double> values) =>
    Build<IEvaluable>(points, values, static (p, v) => Barycentric.InterpolateRationalFloaterHormannSorted(p, v));
public static Fin<Interpolant<IEvaluable>> BulirschStoerRational(Arr<double> points, Arr<double> values) =>
    Build<IEvaluable>(points, values, static (p, v) => BulirschStoerRationalInterpolation.InterpolateSorted(p, v));
public static Fin<Interpolant<IEvaluable>> EquidistantBarycentricPolynomial(Arr<double> points, Arr<double> values) =>
    Build<IEvaluable>(points, values, static (p, v) => Barycentric.InterpolatePolynomialEquidistantSorted(p, v));
```
To:
```csharp
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
```
Why: The eleven members differ only by the MathNet method group they pass to the same admission and exception boundary. The selected package algorithm is already explicit at the caller.

Change: Replace the eleven factories with `Fit`, inline the ascending predicate, and pass the exact MathNet `*Sorted` method group from each consumer.

Delta: -11 LOC; -10 module-level members; 0 types.

Ripples: In `libs/dotnet/Rasm.Element/.planning/Composition/material.md`, replace `Interpolant.LinearSpline` with `Interpolant.Fit(..., LinearSpline.InterpolateSorted)` and add the direct `MathNet.Numerics` manifest and README touch points. In `libs/dotnet/Rasm/.planning/Parametric/curve.md`, replace `Interpolant.PchipSpline` with `Interpolant.Fit(..., CubicSpline.InterpolatePchipSorted)`. Replace the factory roster and witnesses in `libs/dotnet/Rasm.Compute/.planning/Tensor/sampling.md`.

# 6. Delete unconsumed interpolation special cases

From: [transform.md:170](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:170)
```csharp
public static Fin<Interpolant<ICalculus>> QuadraticSpline(Arr<double> knots, Arr<double> constant, Arr<double> linear, Arr<double> quadratic) {
    return Admit.Claims((knots.Count >= 2, "knots-extent"),
            (constant.Count == knots.Count - 1, "constant-extent"),
            (linear.Count == constant.Count, "linear-extent"),
            (quadratic.Count == constant.Count, "quadratic-extent"),
            (TensorPrimitives.IsFiniteAll<double>(knots.AsSpan()), "knots-finite"),
            (TensorPrimitives.IsFiniteAll<double>(constant.AsSpan()), "constant-finite"),
            (TensorPrimitives.IsFiniteAll<double>(linear.AsSpan()), "linear-finite"),
            (TensorPrimitives.IsFiniteAll<double>(quadratic.AsSpan()), "quadratic-finite"),
            (Ascending(knots), "knots-ascending"))
        .Bind(_ => Try.lift(() => new Interpolant<ICalculus>(new MathNet.Numerics.Interpolation.QuadraticSpline(
            x: [.. knots.AsIterable()], c0: [.. constant.AsIterable()], c1: [.. linear.AsIterable()], c2: [.. quadratic.AsIterable()]))).Run());
}
public static Fin<Interpolant<IEvaluable>> TransformedInterpolation(Func<double, double> transform, Func<double, double> inverse, Arr<double> points, Arr<double> values) =>
    Build<IEvaluable>(points, values, (p, v) => MathNet.Numerics.Interpolation.TransformedInterpolation.InterpolateSorted(transform: transform, transformInverse: inverse, x: p, y: v));
```
To:
```csharp
// QuadraticSpline and TransformedInterpolation DELETED
```
Why: Neither member has a consumer. Both algorithms remain available at their MathNet owners, and a future consumer can place its distinct coefficient or transform admission at the boundary that owns those raw values instead of expanding this module with speculative factories.

Change: Delete both factories and their prose roster entries; use `QuadraticSpline` or `TransformedInterpolation.InterpolateSorted` directly if a concrete consumer arrives.

Delta: -14 LOC; -2 module-level members; 0 types.

# 7. Delete obsolete interpolation helpers

From: [transform.md:186](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:186)
```csharp
private static Fin<Interpolant<TCapability>> Build<TCapability>(Arr<double> points, Arr<double> values, Func<double[], double[], IInterpolation> factory, Option<Arr<double>> slopes = default)
    where TCapability : IEvaluable =>
    Admit.Claims((points.Count >= 2, "points-extent"),
            (points.Count == values.Count, "sample-arity"),
            (TensorPrimitives.IsFiniteAll<double>(points.AsSpan()), "points-finite"),
            (TensorPrimitives.IsFiniteAll<double>(values.AsSpan()), "values-finite"),
            (slopes.Map(s => s.Count == points.Count).IfNone(true), "slopes-extent"),
            (slopes.Map(s => TensorPrimitives.IsFiniteAll<double>(s.AsSpan())).IfNone(true), "slopes-finite"),
            (Ascending(points), "points-ascending"))
        .Bind(_ => Try.lift(() => new Interpolant<TCapability>(factory(arg1: [.. points.AsIterable()], arg2: [.. values.AsIterable()]))).Run());
private static bool Ascending(Arr<double> points) =>
    points.Count < 2 || Enumerable.Range(start: 1, count: points.Count - 1).All(index => points[index - 1] < points[index]);
internal static Fin<double> AdmitFinite(double value) =>
    double.IsFinite(value) ? Acceptance.Value(value: value) : Fin.Fail<double>(new KernelFault.InvalidResult());
```
To:
```csharp
// Build, Ascending, and AdmitFinite DELETED
```
Why: `Fit` now owns the only shared two-column admission, its ascending claim is single-use and inlined, and `Acceptance.Value` already owns finite scalar admission.

Change: Delete all three helpers and call `Acceptance.Value` directly from the four result reads.

Delta: -13 LOC; -3 module-level members; 0 types.

# 8. Use MathNet normalization values directly

From: [transform.md:238](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:238)
```csharp
[SmartEnum]
public sealed partial class SpectralScaling {
    public static readonly SpectralScaling Symmetric = new(
        fourierConvention: FourierOptions.Default, hartleyConvention: HartleyOptions.Default, roundTrip: static _ => 1.0);
    public static readonly SpectralScaling AsymmetricInverse = new(
        fourierConvention: FourierOptions.AsymmetricScaling, hartleyConvention: HartleyOptions.AsymmetricScaling, roundTrip: static _ => 1.0);
    public static readonly SpectralScaling Unscaled = new(
        fourierConvention: FourierOptions.NoScaling, hartleyConvention: HartleyOptions.NoScaling, roundTrip: static cells => (double)cells);
    public FourierOptions FourierConvention { get; }
    public HartleyOptions HartleyConvention { get; }
    [UseDelegateFromConstructor] public partial double RoundTrip(long cells);
}
```
To:
```csharp
// SpectralScaling DELETED
```
Why: The three rows duplicate `FourierOptions` and `HartleyOptions` exactly. No retained kernel consumes Hartley options, and the sole no-scaling round-trip factor is directly the transformed cell count at its consumer.

Change: Delete `SpectralScaling`; accept `FourierOptions` on the retained lattice transform and derive the no-scaling inverse divisor where the intermediate spectrum is consumed.

Delta: -11 LOC; -6 module-level members; -1 module-level type.

Ripples: In `libs/dotnet/Rasm.Compute/.planning/Tensor/quadrature.md`, pass `FourierOptions.NoScaling` and divide the inverse by its sample count. In `libs/dotnet/Rasm.Materials/.planning/Raster/filter.md` and `libs/dotnet/Rasm.Materials/.planning/Raster/tile.md`, pass `FourierOptions.Default`. Apply the same replacement in `libs/dotnet/Rasm.Fabrication/.planning/Additive/implicit.md` and remove the duplicate convention narrative from `libs/dotnet/Rasm.Compute/.planning/Stats/signal.md`.

# 9. Reduce transform direction to its used delegate column

From: [transform.md:251](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:251)
```csharp
[SmartEnum]
public sealed partial class SpectralSense {
    public static readonly SpectralSense Forward = new(
        interleaved: Fourier.Forward, split: Fourier.Forward, packed: Fourier.ForwardReal, realValued: Hartley.NaiveForward);
    public static readonly SpectralSense Inverse = new(
        interleaved: Fourier.Inverse, split: Fourier.Inverse, packed: Fourier.InverseReal, realValued: Hartley.NaiveInverse);
    [UseDelegateFromConstructor] internal partial void Interleaved(Complex[] arena, FourierOptions options);
    [UseDelegateFromConstructor] internal partial void Split(double[] real, double[] imaginary, FourierOptions options);
    [UseDelegateFromConstructor] internal partial void Packed(double[] arena, int samples, FourierOptions options);
    [UseDelegateFromConstructor] internal partial double[] RealValued(double[] samples, HartleyOptions options);
}
```
To:
```csharp
[SmartEnum]
public sealed partial class TransformDirection {
    public static readonly TransformDirection Forward = new(apply: Fourier.Forward);
    public static readonly TransformDirection Inverse = new(apply: Fourier.Inverse);

    [UseDelegateFromConstructor]
    internal partial void Apply(Complex[] samples, FourierOptions options);
}
```
Why: Direction is a real two-case behavior family, but only the complex one-dimensional delegate survives inside the branch-owned separable lattice transform. The other three columns merely forward package entrypoints consumers can call directly.

Change: Rename the owner to the canonical direction term, retain one complex-array delegate, and delete the split, packed-real, and Hartley columns.

Delta: -4 LOC; -3 module-level members; 0 types.

Ripples: Replace `SpectralSense` with `TransformDirection` in `libs/dotnet/Rasm.Materials/.planning/Raster/filter.md`, `libs/dotnet/Rasm.Materials/.planning/Raster/tile.md`, and `libs/dotnet/Rasm.Fabrication/.planning/Additive/implicit.md`. `libs/dotnet/Rasm.Compute/.planning/Tensor/quadrature.md` calls the split and packed `Fourier` entrypoints directly.

# 10. Collapse spectral carriers onto the separable lattice transform

From: [transform.md:326](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:326)
```csharp
[Union]
public abstract partial record SpectralArena : IValidityEvidence {
    public sealed record Interleaved(Complex[] Values, CellLattice Lattice) : SpectralArena;
    public sealed record Split(double[] Real, double[] Imaginary, PositiveMagnitude Rate) : SpectralArena;
    public sealed record HalfSpectrum(double[] Values, Dimension Samples, PositiveMagnitude Rate) : SpectralArena;
    public sealed record RealValued(Arr<double> Samples, PositiveMagnitude Rate) : SpectralArena;
    public Fin<Spectrum> Transform(SpectralSense sense, SpectralScaling scaling) =>
        MatrixKernel.SpectralTransform(arena: this, sense: sense, scaling: scaling);
    public static int PackedLength(int samples) => int.IsEvenInteger(samples) ? samples + 2 : samples + 1;
}

public sealed class Spectrum : IValidityEvidence {
    public SpectralArena Arena { get; }
    public Fin<Arr<double>> Power() => MatrixKernel.SpectralPower(arena: Arena);
    public Fin<Arr<double>> Frequencies(SignedAxis axis) => MatrixKernel.SpectralFrequencies(arena: Arena, axis: axis);
    public Fin<Spectrum> Modulate(ReadOnlySpan<Complex> symbol) => MatrixKernel.SpectralModulate(spectrum: this, symbol: symbol);
}
```
To:
```csharp
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
Why: The union wraps four MathNet layouts, and `Spectrum` then wraps that wrapper with derived projections and one-call helpers. Only the separable lattice transform is branch-owned; split and packed one-dimensional transforms, power, frequency axes, and pointwise multiplication are direct MathNet, `Complex`, or `TensorPrimitives` operations.

Change: Delete `SpectralArena`, its four cases, `Spectrum`, and all seven `MatrixKernel` spectral members. Retain the row-column kernel as the body of `Spectral.Transform`, return `Fin<Unit>` for its in-place mutation, finite-gate the result, and remove `System.Runtime.InteropServices`.

Delta: -106 LOC; -22 module-level members; -1 module-level type and -4 nested case types.

Ripples: In `libs/dotnet/Rasm.Materials/.planning/Raster/filter.md`, call `Spectral.Transform` around direct `TensorPrimitives.Multiply<Complex>` modulation. In `libs/dotnet/Rasm.Materials/.planning/Raster/tile.md`, call `Spectral.Transform` and project bin power with `Complex.MagnitudeSquared`. In `libs/dotnet/Rasm.Fabrication/.planning/Additive/implicit.md`, retain the owned `Complex[]` and `CellLattice`, derive axes with `Fourier.FrequencyScale`, multiply directly, and call `Spectral.Transform` for both legs. In `libs/dotnet/Rasm.Compute/.planning/Tensor/quadrature.md`, call `Fourier.ForwardReal`/`InverseReal` or the split `Fourier.Forward`/`Inverse` directly and derive frequency bins with `Fourier.FrequencyScale`. Remove the deleted carriers from `libs/dotnet/Rasm.Fabrication/ARCHITECTURE.md`, `libs/dotnet/Rasm.Materials/ARCHITECTURE.md`, `libs/dotnet/Rasm/ARCHITECTURE.md`, `libs/dotnet/Rasm/RULINGS.md`, and the stacking note in `libs/dotnet/.api/api-mathnet-numerics.md`.

# 11. Make tap-window admission construct the only valid shape

From: [transform.md:278](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:278)
```csharp
public readonly record struct TapWindow : IValidityEvidence {
    private TapWindow(int extent, int origin, int from, int run, int stride) =>
        (Extent, Origin, From, Run, Stride) = (extent, origin, from, run, stride);
    public int Extent { get; }
    public int Origin { get; }
    public int From { get; }
    public int Run { get; }
    public int Stride { get; }

    public static Fin<TapWindow> Of(Dimension extent, Dimension stride, int origin, int from, Dimension run) {
        return Admit.Claims((origin >= 0, "origin"),
                (from >= 0, "from"),
                (origin <= from, "origin-precedes-from"),
                (run.Value <= extent.Value - from, "run-within-extent"))
            .Map(_ => new TapWindow(extent: extent.Value, origin: origin, from: from, run: run.Value, stride: stride.Value));
    }
    public static TapWindow Whole(Dimension extent, Dimension stride) =>
        new(extent: extent.Value, origin: 0, from: 0, run: extent.Value, stride: stride.Value);
    public bool IsValid => ValidityClaim.All(
        Extent >= 1, Stride >= 1, Run >= 1, From >= 0, Origin >= 0, Origin <= From, Run <= Extent - From);
}
```
To:
```csharp
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
```
Why: The struct default is an invalid public value, forcing every consumer to re-run `IsValid`. A sealed reference owner with a private constructor makes `Of` the only construction path; `Whole` is directly `Of(extent, stride, 0, 0, extent)`.

Change: Convert `TapWindow` to a sealed record, delete `IsValid` and `Whole`, and trust the admitted value inside the convolution kernel.

Delta: -6 LOC; -2 module-level members; 0 types.

Ripples: In `libs/dotnet/Rasm.Materials/.planning/Raster/filter.md`, replace `TapWindow.Whole` with `TapWindow.Of(..., origin: 0, from: 0, run: extent)` and bind its `Fin` into the existing fold.

# 12. Absorb the span convolution into TapSeries

From: [transform.md:315](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:315)
```csharp
public Fin<Unit> Convolve(ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border) =>
    MatrixKernel.TapFold(series: this, source: source, folded: folded, window: window, border: border);
```
To:
```csharp
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
```
Why: The public member and `MatrixKernel.TapFold` are a two-hop surface, and the current `Bind` lambda captures `ReadOnlySpan<double>` and `Span<double>`, which C# forbids for ref-like values. The admitted reference `TapWindow` also makes the repeated `window.IsValid` claim obsolete.

Change: Move the `TapFold` admission body and `TapFoldCore` unchanged onto `TapSeries`, rename the private body `ConvolveCore`, use the statement-form failure branch required by the span kernel, and remove `TapSeries.IsValid` by converting `TapSeries` to a sealed record with its existing private factory.

Delta: -3 LOC; -2 module-level members; 0 types.

# 13. Delete the unused lattice-convolution shell

From: [transform.md:319](/Users/bardiasamiee/Documents/99.Github/Rasm/libs/dotnet/Rasm/.planning/Numerics/transform.md:319)
```csharp
public static class LatticeConvolution {
    extension(CellLattice lattice) {
        public Fin<Unit> Convolve(Span<double> values, Arr<TapSeries> axes, TapBorder border) =>
            MatrixKernel.TapFoldLattice(values: values, lattice: lattice, axes: axes, border: border);
    }
}
```
To:
```csharp
// LatticeConvolution DELETED
```
Why: No consumer calls the extension. Its kernel is the caller-derivable repetition of the retained one-axis `TapSeries.Convolve`, and keeping an unconsumed public shell plus a second imperative axis walk expands both surface and maintenance cost.

Change: Delete `LatticeConvolution` and `MatrixKernel.TapFoldLattice`; after the preceding absorptions, delete the empty `MatrixKernel` partial declaration from this spec and remove `CommunityToolkit.HighPerformance.Buffers`.

Delta: -32 LOC; -2 module-level members; -1 module-level type.

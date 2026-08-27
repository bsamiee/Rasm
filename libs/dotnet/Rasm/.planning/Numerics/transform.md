# [RASM_NUMERICS_TRANSFORM]

`Rasm.Numerics` owns the transform band beside the solve kernel: the taper roster a spectral caller windows with, the capability-typed interpolant every one-dimensional fit mints into, and the spectral plane — one arena union over the four MathNet buffer layouts, one transform entry, one `Spectrum`, and BOTH routes of the one convolution correspondence, the pointwise spectral product and the sample-domain tap fold. It is `Numerics/matrix`'s own surface split at the transform boundary: zero edges into the solve core, its own consumer population (`Rasm.Materials` raster planes, `Rasm.Fabrication` implicit lattices, `Rasm.Compute` signal spectra), and one `partial` half of `MatrixKernel` so the one-funnel ruling binds the TYPE and no consumer reaches a raw MathNet transform member on either side of the file cut.

Rebuilds compose the `Rasm.Domain` types as the validity floor and `Numerics/atoms` as the addressing floor: `CellLattice` is the ONE linearization a lattice-backed plane addresses through, `Dimension`/`PositiveMagnitude`/`UnitInterval`/`SignedAxis` carry every extent, rate, fraction, and axis, `EpsilonPolicy` anchors the two floors this band reads. MathNet's managed provider is the pinned realization — the multidim entrypoints are unservable by construction and the separable row-column fold IS the platform-total N-dimensional transform.

## [01]-[INDEX]

- [02]-[WINDOW]: taper roster with framing and shape as columns, the one taper sample entry.
- [03]-[INTERPOLATE]: capability-typed interpolant, per-scheme typed factories, and the two coefficient and transformed mints.
- [04]-[SPECTRAL]: scaling and sense rows, tap series, border, and window, the arena union, its `Spectrum`, and the `MatrixKernel` transform half.

## [02]-[WINDOW]

- Owner: `WindowTaper` is the ONE taper roster — seventeen keyless `[SmartEnum]` rows, a process-local behavior roster whose singleton names ARE the vocabulary and no wire identity exists, fifteen binding a package factory as the `Design` delegate column and two carrying a branch-owned design for the responses the package omits, every row carrying its periodic twin as the one `Option<Func<int, double[]>>` plain column; `TaperSampling` the request union — `Symmetric(Option<TaperShape>)` the endpoint-aligned filter-design sampling that alone carries a shape, `Periodic` the FFT-frame sampling — and `TaperShape` the shaped rows' parameter, each case carrying exactly the admission its factory consumes.
- Entry: `taper.Sample(width, sampling)` samples one taper, the generated `Switch` over the request dispatching the row's `Design` or its periodic column; a row asked for a periodic twin it does not carry, or a shape it does not consume, refuses typed rather than substituting the form it happens to have, and a periodic request cannot carry a shape by construction.
- Law: the roster's package provenance is the decompile-verified `Window` surface — fifteen tapers with the four `*Periodic` twins, and no `Kaiser` or `Bohman` on it — so a package-served taper is one row binding one factory and never a hand-authored coefficient loop; the two responses the package omits are solution ceilings the BAND carries rather than N consumers each forking one `I0` fold, each stating its publication at its row and composing `SpecialFunctions.BesselI0` where a series would otherwise be hand-authored.
- Auto: every sampled coefficient run gates whole through `TensorPrimitives.IsFiniteAll` on the `Fin` result before it leaves the entry, so an admitted but extreme Kaiser beta whose `I0` ratio overflows refuses typed instead of shipping a success-shaped `NaN`.
- Packages: MathNet.Numerics (the `Window` roster, `SpecialFunctions.BesselI0` the Kaiser leg's own evaluation), System.Numerics.Tensors (`TensorPrimitives.IsFiniteAll` the sampled-result gate), `Numerics/atoms` (`Dimension`, `PositiveMagnitude`, `UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a taper is one row; its periodic twin a column on that row; a shaped taper one `TaperShape` case and the accessor its row hands `Parameterized`, so a new parameter never mints a kernel twin; a sampling geometry is one `TaperSampling` case that breaks the `Sample` dispatch at compile time.
- Boundary: framing is a COLUMN on the row and never a second roster — only four of the seventeen rows carry the periodic twin, and the shaped rows carry filter-design alone; `Rasm.Compute` `Stats/signal` `WindowKind` was the strata twin this roster absorbed — its `rectangular` spelling reads `Dirichlet` here and its default sigma and fraction stay Compute policy values handed in as `TaperShape`, never roster defaults.

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

// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class WindowTaper {
    [UseDelegateFromConstructor]
    private partial Fin<Arr<double>> Design(int width, Option<TaperShape> shape);
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

    private Option<Func<int, double[]>> PeriodicDesign { get; }
    public Fin<Arr<double>> Sample(Dimension width, TaperSampling sampling) {
        return Optional(sampling).ToFin(new KernelFault.InvalidInput()).Bind(request => request.Switch(
                symmetric: symmetric => Design(width.Value, symmetric.Shape),
                periodic: _ => PeriodicDesign.ToFin(new KernelFault.InvalidInput()).Map(design => new Arr<double>(design(arg: width.Value)))))
            .Bind(samples => TensorPrimitives.IsFiniteAll<double>(samples.AsSpan()) ? Fin.Succ(samples) : Fin.Fail<Arr<double>>(new KernelFault.InvalidResult()));
    }

    private static Func<int, Option<TaperShape>, Fin<Arr<double>>> Unparameterized(Func<int, double[]> design) =>
        (width, shape, key) => shape.IsSome
            ? Fin.Fail<Arr<double>>(new KernelFault.InvalidInput())
            : Fin.Succ(new Arr<double>(design(arg: width)));
    private static Func<int, Option<TaperShape>, Fin<Arr<double>>> Parameterized(Func<int, double, double[]> design, Func<TaperShape, Option<double>> parameter) =>
        (width, shape, key) => shape.Bind(parameter).ToFin(new KernelFault.InvalidInput())
            .Map(value => new Arr<double>(design(arg1: width, arg2: value)));
}
```

## [03]-[INTERPOLATE]

- Owner: `Interpolant<TCapability>` is the ONE interpolation capability owner — the fitted-curve capsule whose type parameter lifts the package's two runtime support flags into a compile-time capability, so an unsupported `Derivative`, `SecondDerivative`, or `Integrate` call is unspellable rather than a throw; `IEvaluable` is the marker floor, `IDifferentiable`/`IIntegrable` the two orthogonal capability markers over it, and `ICalculus` their join — the three inhabited tiers are those interfaces themselves as the type argument, no witness struct standing beside them; the `Interpolant` static owner carries one typed factory per package scheme, each named for the algorithm it binds, and the two mints the convenience roster omits.
- Cases: the tier matrix mirrors the two flags as an ORTHOGONAL pair — `ICalculus` (both: natural, Akima, PCHIP, and Hermite cubic, linear, quadratic-segment, step), `IDifferentiable` (derivative alone: Neville polynomial, log-linear), `IEvaluable` (neither: Floater-Hormann and Bulirsch-Stoer rational, equidistant barycentric polynomial, transformed fit) — the tiers are DECOMPILE-VERIFIED per class, and no shipped scheme integrates without differentiating, so `IIntegrable` alone is uninhabited and mints no factory; making `IIntegrable : IDifferentiable` would turn that observed roster into a false semantic law.
- Entry: `Interpolant.NaturalCubicSpline`/`AkimaSpline`/`PchipSpline`/`CubicHermiteSpline`/`LinearSpline`/`StepInterpolation` mint `ICalculus`; `NevillePolynomial`/`LogLinearSpline` mint `IDifferentiable`; `FloaterHormannRational`/`BulirschStoerRational`/`EquidistantBarycentricPolynomial` mint `IEvaluable`; `QuadraticSpline` admits per-segment quadratic coefficients through the package constructor and `TransformedInterpolation` composes a domain transform with its inverse around the fit — the two schemes the convenience roster omits, reached at their own owners; `Evaluate(t)` reads on every tier and the capability extension blocks reach the rest, `Integrate(upper, lower)` carrying the definite integral's lower limit as an `Option` rather than an arity twin whose two spellings meant two different integrals.
- Auto: one `Build` fold ACCUMULATES every misaligned, non-finite, or unordered-sample claim through `Admit.Claims` — Hermite's slope column rides the same fold as an `Option<Arr<double>>` whose extent and finiteness claims read `true` when absent, so a caller handed a short values column, an unordered abscissa, and a short slope column learns all three, and an empty or single-sample input reaches the typed `points-extent` claim because the order claim is total below two samples — hands the package its `*Sorted` array constructor over columns the fold already proved strictly ascending, so no package entry copies and re-sorts them, and captures the throwing package factory through `Try.lift`; every read is finite-gated on `Fin` with the operation key, because the step scheme returns `NaN` at a sample point and the rational schemes return `NaN` below ULP, so an ungated read poisons a gradient silently.
- Law: `Rasm.Compute` `Tensor/sampling` is the richest consumer and this owner adopts its capability form whole; the former keyed `InterpolationRoute` roster is DELETED — its rows became the typed factories, because a keyed row cannot return a per-row capability type. NAMED LOSS: selecting a scheme by wire key at runtime; no consumer selects one, and a peer that does maps its key onto the factory at its own edge. WITNESS: `InterpolationRoute.CubicSpline.Fit(points, values)` rebuilt as `Interpolant.NaturalCubicSpline(points, values)`, the result now typed `Interpolant<ICalculus>` whose `Integrate` compiles. `Interpolate.Common` and `Interpolate.RationalWithoutPoles` are one Floater-Hormann scheme, so `FloaterHormannRational` is its one factory and no duplicate wrapper stands beside it; `Differentiate2` is the second derivative and `Integrate(a, b)` a signed integral, so the reads carry those names and never `Curvature` or `Area`.
- Packages: MathNet.Numerics (the `*Sorted` array constructors on `CubicSpline`, `LinearSpline`, `StepInterpolation`, `NevillePolynomialInterpolation`, `LogLinear`, `BulirschStoerRationalInterpolation`, `Barycentric`, and `TransformedInterpolation`, the `QuadraticSpline` coefficient constructor, `IInterpolation`), System.Numerics.Tensors (`TensorPrimitives.IsFiniteAll`), `Numerics/atoms`, Rasm.Domain (`Admit.Claims`), LanguageExt.Core, BCL inbox.
- Growth: a scheme is one typed factory returning its tier, named for the algorithm it binds; a capability is one marker interface with one extension block, its inhabited join one further interface — never a witness struct; a per-scheme class family or a runtime capability boolean is the deleted form.
- Boundary: `MathNet.Numerics.Interpolation` is one-dimensional whole, so a bicubic or scattered-surface reconstruction is the regression route's and never a factory here; the extension blocks are the ONLY reach to `Differentiate`/`Differentiate2`/`Integrate`, so a consumer holding an `Interpolant<IEvaluable>` cannot spell them.

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

// --- [TYPES] ---------------------------------------------------------------------------
public interface IEvaluable { }
public interface IDifferentiable : IEvaluable { }
public interface IIntegrable : IEvaluable { }
public interface ICalculus : IDifferentiable, IIntegrable { }

// --- [MODELS] --------------------------------------------------------------------------
public sealed class Interpolant<TCapability> where TCapability : IEvaluable {
    internal Interpolant(IInterpolation curve) => Curve = curve;
    internal IInterpolation Curve { get; }

    public Fin<double> Evaluate(double t) => Interpolant.AdmitFinite(value: Curve.Interpolate(t: t));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Interpolant {
    public static Fin<Interpolant<ICalculus>> NaturalCubicSpline(Arr<double> points, Arr<double> values) =>
        Build<ICalculus>(points, values, static (p, v) => CubicSpline.InterpolateNaturalSorted(p, v));
    public static Fin<Interpolant<ICalculus>> AkimaSpline(Arr<double> points, Arr<double> values) =>
        Build<ICalculus>(points, values, static (p, v) => CubicSpline.InterpolateAkimaSorted(p, v));
    public static Fin<Interpolant<ICalculus>> PchipSpline(Arr<double> points, Arr<double> values) =>
        Build<ICalculus>(points, values, static (p, v) => CubicSpline.InterpolatePchipSorted(p, v));
    public static Fin<Interpolant<ICalculus>> CubicHermiteSpline(Arr<double> points, Arr<double> values, Arr<double> slopes) =>
        Build<ICalculus>(points, values,
            (p, v) => CubicSpline.InterpolateHermiteSorted(p, v, [.. slopes.AsIterable()]), slopes: Some(slopes));
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
            .Bind(_ => Try.lift(() => Fin.Succ(new Interpolant<ICalculus>(new MathNet.Numerics.Interpolation.QuadraticSpline(
                x: [.. knots.AsIterable()], c0: [.. constant.AsIterable()], c1: [.. linear.AsIterable()], c2: [.. quadratic.AsIterable()])))).Run().Bind(static inner => inner));
    }
    public static Fin<Interpolant<IEvaluable>> TransformedInterpolation(Func<double, double> transform, Func<double, double> inverse, Arr<double> points, Arr<double> values) =>
        Build<IEvaluable>(points, values, (p, v) => MathNet.Numerics.Interpolation.TransformedInterpolation.InterpolateSorted(transform: transform, transformInverse: inverse, x: p, y: v));

    private static Fin<Interpolant<TCapability>> Build<TCapability>(Arr<double> points, Arr<double> values, Func<double[], double[], IInterpolation> factory, Option<Arr<double>> slopes = default)
        where TCapability : IEvaluable =>
        Admit.Claims((points.Count >= 2, "points-extent"),
                (points.Count == values.Count, "sample-arity"),
                (TensorPrimitives.IsFiniteAll<double>(points.AsSpan()), "points-finite"),
                (TensorPrimitives.IsFiniteAll<double>(values.AsSpan()), "values-finite"),
                (slopes.Map(s => s.Count == points.Count).IfNone(true), "slopes-extent"),
                (slopes.Map(s => TensorPrimitives.IsFiniteAll<double>(s.AsSpan())).IfNone(true), "slopes-finite"),
                (Ascending(points), "points-ascending"))
            .Bind(_ => Try.lift(() => Fin.Succ(new Interpolant<TCapability>(factory(arg1: [.. points.AsIterable()], arg2: [.. values.AsIterable()])))).Run().Bind(static inner => inner));
    private static bool Ascending(Arr<double> points) =>
        points.Count < 2 || Enumerable.Range(start: 1, count: points.Count - 1).All(index => points[index - 1] < points[index]);
    internal static Fin<double> AdmitFinite(double value) =>
        double.IsFinite(value) ? Acceptance.Value(value: value) : Fin.Fail<double>(new KernelFault.InvalidResult());

    extension<TCapability>(Interpolant<TCapability> self) where TCapability : IDifferentiable {
        public Fin<double> Derivative(double t) => AdmitFinite(value: self.Curve.Differentiate(t: t));
        public Fin<double> SecondDerivative(double t) => AdmitFinite(value: self.Curve.Differentiate2(t: t));
    }
    extension<TCapability>(Interpolant<TCapability> self) where TCapability : IIntegrable {
        public Fin<double> Integrate(double upper, Option<double> lower = default) =>
            AdmitFinite(value: lower.Match(Some: a => self.Curve.Integrate(a: a, b: upper), None: () => self.Curve.Integrate(t: upper)));
    }
}
```

## [04]-[SPECTRAL]

- Owner: `SpectralArena` is the ONE transform carrier — four cases, each holding the buffer layout its MathNet entrypoint owns and exactly the extent its arm consumes; `Spectrum` the internally minted result naming the mutable arena a transform leaves — no public constructor, no derived energy column, valid exactly when its arena is; `SpectralScaling` the declared convention row governing both transform owners at once and `SpectralSense` the direction row carrying the four entrypoint pairs as columns; `TapSeries` the admitted sample-domain convolution kernel, `TapBorder` its closed out-of-extent vocabulary answering an `Option<int>` so an absent tap is a carrier and not a sentinel, and `TapWindow` the staged-window geometry a banded caller admits through its own gated `Of`; the `MatrixKernel` `partial` half on this page is the one path to every transform, power, frequency, modulation, and tap-fold body.
- Entry: `arena.Transform(sense, scaling)` is the one transform entry and the arena case its discriminant, so no per-carrier entrypoint family and no mode flag exist; `spectrum.Power`/`Frequencies`/`Modulate` read and re-mint off the spectrum; `series.Convolve(source, folded, window, border)` folds one strided axis in the sample domain and `lattice.Convolve(values, axes, border)` is its separable lattice form on the ADDRESSING owner, the lattice being the discriminant — one series per axis, never a static twin wearing the instance member's name.
- Auto: rank 2 and rank 3 ARE the row-column fold over the managed-complete 1D pair (Radix-2 at a power of two, Bluestein otherwise), and symmetric scaling composes per axis (`1/sqrt(w) · 1/sqrt(h) = 1/sqrt(w·h)`), so the folded transform carries the convention the 1D row declares and `RoundTripFactor` reads the cell count once; the tap fold divides every output by its RESOLVED-weight sum, so partition of unity holds at every border by construction — no caller pre-normalizes a table, `TapSeries.Of` canonicalizes the coefficients to unit sum at the mint so a series and its scalar multiple — a negative one included — are ONE stored value with one equality identity and not merely one folded sample, an `Omit`-dropped tap leaves the divisor rather than darkening the rim, and a rim record whose resolved sum cancels or overflows refuses typed rather than certifying a fabricated sample; both raw-span entries refuse non-finite source material before any mutation, and the core refuses a non-finite output lane at the record that derives it; the lattice tap fold is the SAME per-axis line fold the rank-2/3 transform takes, walking the lattice's own linearization strides; `Power` reads ONE single-pass pair fold across the interleaved and packed layouts — byte-identical `(re, im)` runs a `MemoryMarshal.Cast` unifies, each bin summing its own two squares with no scratch plane, since no tensor primitive reduces adjacent pairs into a half-length destination — beside the vectorized multiply-then-multiply-add pair on the split spans, the reason the split case exists, and never a square root it only squares back; `Frequencies` reads ONE generated union fold inside the sole frequency operation — sample count, published bin count, and sampling rate per case, the lattice arm reading `CellLattice.Extent`/`Spacing`, the packed arm publishing exactly its `floor(N/2)+1` bins — so a spectrum reads its own axis instead of a caller-passed rate that can disagree with the grid, a negative `SignedAxis` row negates the coordinates rather than aliasing its positive twin, and an out-of-rank ordinal states absence once rather than at four call sites; Hartley power reads the defining `(H[k]² + H[N−k]²)/2` identity by reflected index with no copied plane. Every multi-column admission on the page ACCUMULATES through `Admit.Claims`, each clause naming its axis. `SpectralArena.IsValid` refuses the defaultable zero-cell lattice and the zero-sample packed census before any arm reads a stride, and `Power` gates its squared bins finite at the operation that squares them, so an overflowed magnitude refuses where it is produced rather than at construction of an unrelated public scalar — a summed energy is layout-dependent (the packed case never doubles its conjugate interior) and is therefore no carrier evidence.
- Law: `SpectralScaling` publishes both convention columns so a package binding MathNet's transform entrypoints directly reads the declared row instead of re-spelling a second `FourierOptions` vocabulary; `Rasm.Compute` `Stats/signal` composes it and its eight raw `FourierOptions` sites are the deleted form.
- Packages: MathNet.Numerics (`Fourier` interleaved, split, and packed pairs, `FrequencyScale`, `Hartley.NaiveForward`/`NaiveInverse`, `FourierOptions`/`HartleyOptions`), System.Numerics.Tensors (`TensorPrimitives.Multiply`/`MultiplyAdd`/`Divide`/`Negate`/`Sum`/`IsFiniteAll`), CommunityToolkit.HighPerformance (`MemoryOwner<T>.Allocate` — the lattice staging pair; the separable line stays an exact-extent array because the package entrypoint transforms its whole length), `Numerics/atoms` (`CellLattice` with its per-axis `Extent`/`Stride`/`Spacing`, `Dimension`, `PositiveMagnitude`, `SignedAxis`, `EpsilonPolicy`), Rasm.Domain (`Admit.Claims`, `Admit.FiniteComplexSpan`, `ValidityClaim`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL (`System.Numerics.Complex`, `MemoryMarshal.Cast`).
- Growth: a border law is one `TapBorder` row every tap fold reads with no kernel edit; a scaling convention is one `SpectralScaling` row governing both owners at once; a buffer layout is one `SpectralArena` case whose arms break every fold at compile time.
- Boundary: `Fourier.Forward2D`/`Inverse2D`/`ForwardMultiDim`/`InverseMultiDim` never spell in a fence — all four route to the multidim provider interface whose managed realization throws `NotSupportedException`, and the admitted native adapters ship no arm64 asset, so the managed-provider pin makes them unservable by construction. Every transform overwrites the caller's arena, so an immutable spectrum value is unrepresentable and `Spectrum` names the arena the result lives in — the same instance for the three in-place cases, a fresh one for the Hartley case, the sole entrypoint that allocates its output. A packed-real arena carries EXACTLY the `N+2`/`N+1` cells MathNet's `ForwardReal` owns — a longer buffer refuses, since its tail would survive mutation yet vanish from `Power`, `Frequencies`, and the inverse. Separable convolution has NO package primitive — `System.Numerics.Tensors` carries no `Conv1D`, `Conv2D`, `Conv3D`, or `MatMul` — so this band owns BOTH routes of the one convolution correspondence itself: the pointwise spectral product between the transform legs (`Spectrum.Modulate`) and the sample-domain tap fold (`TapSeries.Convolve`); a consumer composes one of the two and spells no fold of its own, while its tap GENERATION stays the consumer's domain policy. Zero-sum series are DIFFERENCE stencils and refuse at the mint: `Numerics/calculus#NABLA` owns those, so the two owners partition on the tap sum rather than overlapping. `CellLattice` is the addressing carrier for a lattice-backed plane and owns the per-axis `Extent`/`Stride`/`Spacing` read every separable walk takes, so the band mints no second linearization, no sibling 2D arena, and no strided-view owner beside it — the `Tensor<T>` plane stays refused on four structural grounds: array-only static entrypoints at the mint, `ref struct` span views that cannot cross `Fin`, an allocating `PermuteDimensions` on every transpose, and this carrier's one-linearization law. Named statement-kernel exemption covers the separable axis gather-scatter and the tap-fold record walk — measured strided-line hot paths.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using MathNet.Numerics.IntegralTransforms;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
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

public readonly record struct TapSeries : IValidityEvidence {
    private TapSeries(Arr<double> taps) => Taps = taps;

    public Arr<double> Taps { get; }
    public int Radius => Taps.Count / 2;
    public bool IsValid => ValidityClaim.All(Taps.Count >= 1);

    public static Fin<TapSeries> Of(Arr<double> taps) {
        double sum = TensorPrimitives.Sum<double>(taps.AsSpan());
        if (taps.Count < 1 || int.IsEvenInteger(taps.Count) || !TensorPrimitives.IsFiniteAll<double>(taps.AsSpan()) || !double.IsFinite(sum) || Math.Abs(sum) <= EpsilonPolicy.ZeroTolerance) { return Fin.Fail<TapSeries>(new KernelFault.InvalidInput()); }
        double[] normalized = new double[taps.Count];
        TensorPrimitives.Divide<double>(taps.AsSpan(), sum, normalized);
        return TensorPrimitives.IsFiniteAll<double>(normalized) ? Fin.Succ(new TapSeries(new Arr<double>(normalized))) : Fin.Fail<TapSeries>(new KernelFault.InvalidResult());
    }

    public Fin<Unit> Convolve(ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border) =>
        MatrixKernel.TapFold(series: this, source: source, folded: folded, window: window, border: border);
}

public static class LatticeConvolution {
    extension(CellLattice lattice) {
        public Fin<Unit> Convolve(Span<double> values, Arr<TapSeries> axes, TapBorder border) =>
            MatrixKernel.TapFoldLattice(values: values, lattice: lattice, axes: axes, border: border);
    }
}

[Union]
public abstract partial record SpectralArena : IValidityEvidence {
    private SpectralArena() { }
    public sealed record Interleaved(Complex[] Values, CellLattice Lattice) : SpectralArena;
    public sealed record Split(double[] Real, double[] Imaginary, PositiveMagnitude Rate) : SpectralArena;
    public sealed record HalfSpectrum(double[] Values, Dimension Samples, PositiveMagnitude Rate) : SpectralArena;
    public sealed record RealValued(Arr<double> Samples, PositiveMagnitude Rate) : SpectralArena;

    public Fin<Spectrum> Transform(SpectralSense sense, SpectralScaling scaling) =>
        MatrixKernel.SpectralTransform(arena: this, sense: sense, scaling: scaling);
    public bool IsValid => ValidityClaim.All(Switch(
        interleaved: static a => a.Lattice.CellCount >= 1L && a.Values.Length == a.Lattice.CellCount && Admit.FiniteComplexSpan(a.Values.AsSpan()),
        split: static s => s.Real.Length >= 1 && s.Real.Length == s.Imaginary.Length
            && TensorPrimitives.IsFiniteAll<double>(s.Real) && TensorPrimitives.IsFiniteAll<double>(s.Imaginary),
        halfSpectrum: static h => h.Samples.Value >= 1 && h.Values.Length == PackedLength(samples: h.Samples.Value) && TensorPrimitives.IsFiniteAll<double>(h.Values),
        realValued: static r => r.Samples.Count >= 1 && TensorPrimitives.IsFiniteAll<double>(r.Samples.AsSpan())));
    public int Rank => Switch(interleaved: static a => a.Lattice.Rank, split: static _ => 1, halfSpectrum: static _ => 1, realValued: static _ => 1);
    public long Cells => Switch(
        interleaved: static a => a.Lattice.CellCount,
        split: static s => (long)s.Real.Length,
        halfSpectrum: static h => (long)h.Samples.Value,
        realValued: static r => (long)r.Samples.Count);
    public static int PackedLength(int samples) => int.IsEvenInteger(samples) ? samples + 2 : samples + 1;
}

public sealed class Spectrum : IValidityEvidence {
    internal Spectrum(SpectralArena arena, SpectralSense sense, SpectralScaling scaling) =>
        (Arena, Sense, Scaling) = (arena, sense, scaling);
    public SpectralArena Arena { get; }
    public SpectralSense Sense { get; }
    public SpectralScaling Scaling { get; }
    public int Rank => Arena.Rank;
    public long Cells => Arena.Cells;
    public double RoundTripFactor => Scaling.RoundTrip(cells: Cells);
    public bool IsValid => Arena is not null && Sense is not null && Scaling is not null && Arena.IsValid;
    public Fin<Arr<double>> Power() => MatrixKernel.SpectralPower(arena: Arena);
    public Fin<Arr<double>> Frequencies(SignedAxis axis) =>
        MatrixKernel.SpectralFrequencies(arena: Arena, axis: axis);
    public Fin<Spectrum> Modulate(ReadOnlySpan<Complex> symbol) =>
        MatrixKernel.SpectralModulate(spectrum: this, symbol: symbol);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class MatrixKernel {
    // --- [SPECTRAL] --------------------------------------------------------------------
    internal static Fin<Spectrum> SpectralTransform(SpectralArena arena, SpectralSense sense, SpectralScaling scaling) =>
        arena is null || sense is null || scaling is null || !arena.IsValid
            ? Fin.Fail<Spectrum>(new KernelFault.InvalidInput())
            : Try.lift(() => SpectrumOf(arena: arena.Switch(
                state: (Sense: sense, Scaling: scaling),
                interleaved: static (s, a) => FoldSeparable(arena: a, sense: s.Sense, options: s.Scaling.FourierConvention),
                split: static (s, a) => {
                    s.Sense.Split(real: a.Real, imaginary: a.Imaginary, options: s.Scaling.FourierConvention);
                    return (SpectralArena)a;
                },
                halfSpectrum: static (s, a) => {
                    s.Sense.Packed(arena: a.Values, samples: a.Samples.Value, options: s.Scaling.FourierConvention);
                    return (SpectralArena)a;
                },
                realValued: static (s, a) => new SpectralArena.RealValued(
                    Samples: new Arr<double>(s.Sense.RealValued(samples: [.. a.Samples.AsIterable()], options: s.Scaling.HartleyConvention)), Rate: a.Rate)),
                sense: sense, scaling: scaling)).Run().Bind(static inner => inner);
    private static SpectralArena FoldSeparable(SpectralArena.Interleaved arena, SpectralSense sense, FourierOptions options) {
        CellLattice lattice = arena.Lattice;
        int cells = arena.Values.Length;
        for (int axis = 0; axis < lattice.Rank; axis++) {
            int count = lattice.Extent(ordinal: axis).Value, stride = lattice.Stride(ordinal: axis);
            Complex[] line = new Complex[count];
            for (int origin = 0; origin < cells; origin++) {
                if (origin / stride % count != 0) { continue; }
                for (int k = 0; k < count; k++) { line[k] = arena.Values[origin + (k * stride)]; }
                sense.Interleaved(arena: line, options: options);
                for (int k = 0; k < count; k++) { arena.Values[origin + (k * stride)] = line[k]; }
            }
        }
        return arena;
    }
    private static Fin<Spectrum> SpectrumOf(SpectralArena arena, SpectralSense sense, SpectralScaling scaling) {
        Spectrum spectrum = new(arena: arena, sense: sense, scaling: scaling);
        return spectrum.IsValid ? Fin.Succ(spectrum) : Fin.Fail<Spectrum>(new KernelFault.InvalidResult());
    }
    internal static Fin<Arr<double>> SpectralPower(SpectralArena arena) =>
        arena is null || !arena.IsValid
            ? Fin.Fail<Arr<double>>(new KernelFault.InvalidInput())
            : Try.lift(() => Fin.Succ(arena.Switch(
                interleaved: static a => PairPower(pairs: MemoryMarshal.Cast<Complex, double>(a.Values), bins: a.Values.Length),
                split: static s => {
                    double[] power = new double[s.Real.Length];
                    TensorPrimitives.Multiply<double>(s.Real, s.Real, power);
                    TensorPrimitives.MultiplyAdd<double>(s.Imaginary, s.Imaginary, power, power);
                    return new Arr<double>(power);
                },
                halfSpectrum: static h => PairPower(pairs: h.Values, bins: SpectralArena.PackedLength(samples: h.Samples.Value) / 2),
                realValued: static r => {
                    double[] power = new double[r.Samples.Count];
                    power[0] = r.Samples[0] * r.Samples[0];
                    for (int bin = 1; bin < power.Length; bin++) { double direct = r.Samples[bin], reflected = r.Samples[power.Length - bin]; power[bin] = 0.5 * ((direct * direct) + (reflected * reflected)); }
                    return new Arr<double>(power);
                }))).Run().Bind(static inner => inner)
              .Bind(power => TensorPrimitives.IsFiniteAll<double>(power.AsSpan())
                  ? Fin.Succ(power)
                  : Fin.Fail<Arr<double>>(new KernelFault.InvalidResult()));
    private static Arr<double> PairPower(ReadOnlySpan<double> pairs, int bins) {
        double[] power = new double[bins];
        for (int bin = 0; bin < bins; bin++) { double real = pairs[2 * bin], imaginary = pairs[(2 * bin) + 1]; power[bin] = (real * real) + (imaginary * imaginary); }
        return new Arr<double>(power);
    }
    internal static Fin<Arr<double>> SpectralFrequencies(SpectralArena arena, SignedAxis axis) {
        if (arena is null || axis is null || !arena.IsValid) { return Fin.Fail<Arr<double>>(new KernelFault.InvalidInput()); }
        Option<(int Samples, int Bins, double Rate)> metric = arena.Switch(
            state: Math.Abs(value: axis.Key) - 1,
            interleaved: static (o, a) => o >= 0 && o < a.Lattice.Rank ? Some((a.Lattice.Extent(o).Value, a.Lattice.Extent(o).Value, 1.0 / a.Lattice.Spacing(o))) : Option<(int, int, double)>.None,
            split: static (o, a) => o is 0 ? Some((a.Real.Length, a.Real.Length, a.Rate.Value)) : Option<(int, int, double)>.None,
            halfSpectrum: static (o, a) => o is 0 ? Some((a.Samples.Value, SpectralArena.PackedLength(a.Samples.Value) / 2, a.Rate.Value)) : Option<(int, int, double)>.None,
            realValued: static (o, a) => o is 0 ? Some((a.Samples.Count, a.Samples.Count, a.Rate.Value)) : Option<(int, int, double)>.None);
        return metric.ToFin(new KernelFault.InvalidInput(Axis: Some("spectral-ordinal"))).Bind(row =>
            row.Samples < 1 || row.Bins < 1 || row.Bins > row.Samples || !double.IsFinite(row.Rate) || row.Rate <= 0.0
                ? Fin.Fail<Arr<double>>(new KernelFault.InvalidInput())
                : Try.lift(() => {
                    double[] scale = Fourier.FrequencyScale(length: row.Samples, sampleRate: row.Rate);
                    if (axis.Key < 0) { TensorPrimitives.Negate<double>(scale, scale); }
                    Arr<double> bins = new(row.Bins == scale.Length ? scale : scale[..row.Bins]);
                    return TensorPrimitives.IsFiniteAll<double>(bins.AsSpan()) ? Fin.Succ(bins) : Fin.Fail<Arr<double>>(new KernelFault.InvalidResult());
                }).Run().Bind(static inner => inner));
    }

    internal static Fin<Spectrum> SpectralModulate(Spectrum spectrum, ReadOnlySpan<Complex> symbol) {
        if (!spectrum.IsValid || spectrum.Arena is not SpectralArena.Interleaved plane || plane.Values.Length != symbol.Length || !Admit.FiniteComplexSpan(symbol)) {
            return Fin.Fail<Spectrum>(new KernelFault.InvalidInput());
        }
        TensorPrimitives.Multiply<Complex>(plane.Values, symbol, plane.Values);
        return SpectrumOf(arena: plane, sense: spectrum.Sense, scaling: spectrum.Scaling);
    }

    // --- [TAP_FOLD] --------------------------------------------------------------------
    internal static Fin<Unit> TapFold(TapSeries series, ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border) {
        int stride = window.Stride, staged = stride >= 1 ? source.Length / stride : 0;
        bool whole = window.Origin == 0 && staged == window.Extent;
        return Admit.Claims((series.IsValid, "series"),
                (TensorPrimitives.IsFiniteAll<double>(source), "source-finite"),
                (window.IsValid, "window"),
                (border is not null, "border"),
                (source.Length == staged * stride, "source-extent"),
                (folded.Length == (long)window.Run * stride, "folded-extent"),
                ((long)window.Origin <= Math.Max(0L, (long)window.From - series.Radius), "staging-head"),
                ((long)window.Origin + staged > Math.Min((long)window.Extent - 1, (long)window.From + window.Run - 1 + series.Radius), "staging-tail"),
                (whole || border == TapBorder.Omit, "partial-window-border"))
            .Bind(_ => TapFoldCore(series: series, source: source, folded: folded, window: window, border: border));
    }
    internal static Fin<Unit> TapFoldLattice(Span<double> values, CellLattice lattice, Arr<TapSeries> axes, TapBorder border) {
        int longest = Math.Max(val1: lattice.Columns.Value, val2: Math.Max(val1: lattice.Rows.Value, val2: lattice.Layers.Value));
        Fin<Unit> admitted = Admit.Claims((border is not null, "border"),
            (lattice.CellCount >= 1L, "lattice-census"),
            (axes.Count == lattice.Rank, "axis-arity"),
            (values.Length == lattice.CellCount, "value-extent"),
            (TensorPrimitives.IsFiniteAll<double>(values), "values-finite"),
            (longest <= Array.MaxLength / 2, "staging-extent"),
            (axes.ForAll(static series => series.IsValid), "axis-series"));
        if (admitted.IsFail) { return admitted; }
        int cells = values.Length;
        using MemoryOwner<double> staging = MemoryOwner<double>.Allocate(size: longest * 2);
        Span<double> line = staging.Span[..longest], result = staging.Span[longest..];
        for (int axis = 0; axis < axes.Count; axis++) {
            int count = lattice.Extent(ordinal: axis).Value, stride = lattice.Stride(ordinal: axis);
            TapWindow window = TapWindow.Whole(extent: lattice.Extent(ordinal: axis), stride: Dimension.Create(value: 1));
            for (int origin = 0; origin < cells; origin++) {
                if (origin / stride % count != 0) { continue; }
                for (int k = 0; k < count; k++) { line[k] = values[origin + (k * stride)]; }
                Fin<Unit> lineFold = TapFoldCore(series: axes[axis], source: line[..count], folded: result[..count], window: window, border: border);
                if (lineFold.IsFail) { return lineFold; }
                for (int k = 0; k < count; k++) { values[origin + (k * stride)] = result[k]; }
            }
        }
        return Fin.Succ(unit);
    }
    private static Fin<Unit> TapFoldCore(TapSeries series, ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border) {
        ReadOnlySpan<double> taps = series.Taps.AsSpan();
        int radius = series.Radius, stride = window.Stride;
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
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

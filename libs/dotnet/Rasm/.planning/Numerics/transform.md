# [RASM_NUMERICS_TRANSFORM]

`Rasm.Numerics` owns the transform band beside the solve kernel: the taper roster a spectral caller windows with, the capability-typed interpolant every one-dimensional fit mints into, and the spectral plane — one arena union over the four MathNet buffer layouts, one transform entry, one `Spectrum`, and BOTH routes of the one convolution correspondence, the pointwise spectral product and the sample-domain tap fold. It is `Numerics/matrix`'s own surface split at the transform boundary: zero edges into the solve core, its own consumer population (`Rasm.Materials` raster planes, `Rasm.Fabrication` implicit lattices, `Rasm.Compute` signal spectra), and one `partial` half of `MatrixKernel` so the one-funnel ruling binds the TYPE and no consumer reaches a raw MathNet transform member on either side of the file cut.

Rebuilds compose the `Rasm.Domain` types as the validity floor and `Numerics/atoms` as the addressing floor: `CellLattice` is the ONE linearization a lattice-backed plane addresses through, `Dimension`/`PositiveMagnitude`/`UnitInterval`/`SignedAxis` carry every extent, rate, fraction, and axis, `EpsilonPolicy` anchors the two floors this band reads, and `Op` keys every entry under the optional-key spelling. MathNet's managed provider is the pinned realization — the multidim entrypoints are unservable by construction and the separable row-column fold IS the platform-total N-dimensional transform.

## [01]-[INDEX]

- [02]-[WINDOW]: taper roster with framing and shape as columns, the one taper sample entry.
- [03]-[INTERPOLATE]: capability-typed interpolant, per-scheme typed factories, and the two coefficient and transformed mints.
- [04]-[SPECTRAL]: scaling and sense rows, tap series, border, and window, the arena union, its `Spectrum`, and the `MatrixKernel` transform half.

## [02]-[WINDOW]

- Owner: `WindowTaper` is the ONE taper roster — seventeen `[SmartEnum<string>]` rows keyed on the wire spelling every peer already speaks, thirteen binding a package factory as a column and two carrying a branch-owned design for the responses the package omits; `TaperFraming` the filter-design-versus-FFT-framing axis and `TaperShape` the shaped rows' parameter, each case carrying exactly the admission its factory consumes.
- Entry: `taper.Of(width, framing, shape, key)` samples one taper; a row asked for a framing it does not serve, or a shape it does not consume, refuses typed rather than substituting the form it happens to have.
- Law: the roster's package provenance is the decompile-verified `Window` surface — fifteen tapers with the four `*Periodic` twins, and no `Kaiser` or `Bohman` on it — so a package-served taper is one row binding one factory and never a hand-authored coefficient loop; the two responses the package omits are solution ceilings the BAND carries rather than N consumers each forking one `I0` fold, each stating its publication at its row and composing `SpecialFunctions.BesselI0` where a series would otherwise be hand-authored.
- Packages: MathNet.Numerics (the `Window` roster, `SpecialFunctions.BesselI0` the Kaiser leg's own evaluation), `Numerics/atoms` (`Dimension`, `PositiveMagnitude`, `UnitInterval`), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox.
- Growth: a taper is one row; its FFT-framing twin a column on that row; a shaped taper one `TaperShape` case and the accessor its row hands `Shaped`, so a new parameter never mints a kernel twin.
- Boundary: framing is a COLUMN on the row and never a second roster — only four of the seventeen rows carry the periodic twin, and the shaped rows carry filter-design alone; `Rasm.Compute` `Stats/signal` `WindowKind` was the strata twin this roster absorbed — its `rectangular` spelling reads `Dirichlet` here and its default sigma and fraction stay Compute policy values handed in as `TaperShape`, never roster defaults.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Linq;
using LanguageExt;
using MathNet.Numerics;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class TaperFraming {
    public static readonly TaperFraming FilterDesign = new(key: 0);
    public static readonly TaperFraming FftFrame = new(key: 1);
}

[Union]
public abstract partial record TaperShape {
    private TaperShape() { }
    public sealed record Spread(PositiveMagnitude Sigma) : TaperShape;
    public sealed record Tapered(UnitInterval Fraction) : TaperShape;
    public sealed record Beta(PositiveMagnitude Value) : TaperShape;
}

internal delegate Fin<Arr<double>> TaperKernel(int width, Option<TaperShape> shape, TaperFraming framing, Op key);

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WindowTaper {
    public static readonly WindowTaper Hann = new(key: "hann", sample: Fixed(Window.Hann, Some<Func<int, double[]>>(Window.HannPeriodic)));
    public static readonly WindowTaper Hamming = new(key: "hamming", sample: Fixed(Window.Hamming, Some<Func<int, double[]>>(Window.HammingPeriodic)));
    public static readonly WindowTaper Cosine = new(key: "cosine", sample: Fixed(Window.Cosine, Some<Func<int, double[]>>(Window.CosinePeriodic)));
    public static readonly WindowTaper Lanczos = new(key: "lanczos", sample: Fixed(Window.Lanczos, Some<Func<int, double[]>>(Window.LanczosPeriodic)));
    public static readonly WindowTaper Blackman = new(key: "blackman", sample: Fixed(Window.Blackman, None));
    public static readonly WindowTaper BlackmanHarris = new(key: "blackman-harris", sample: Fixed(Window.BlackmanHarris, None));
    public static readonly WindowTaper BlackmanNuttall = new(key: "blackman-nuttall", sample: Fixed(Window.BlackmanNuttall, None));
    public static readonly WindowTaper Nuttall = new(key: "nuttall", sample: Fixed(Window.Nuttall, None));
    public static readonly WindowTaper FlatTop = new(key: "flat-top", sample: Fixed(Window.FlatTop, None));
    public static readonly WindowTaper Bartlett = new(key: "bartlett", sample: Fixed(Window.Bartlett, None));
    public static readonly WindowTaper BartlettHann = new(key: "bartlett-hann", sample: Fixed(Window.BartlettHann, None));
    public static readonly WindowTaper Triangular = new(key: "triangular", sample: Fixed(Window.Triangular, None));
    public static readonly WindowTaper Dirichlet = new(key: "dirichlet", sample: Fixed(Window.Dirichlet, None));
    public static readonly WindowTaper Gauss = new(key: "gauss",
        sample: Shaped(Window.Gauss, static shape => shape is TaperShape.Spread spread ? Some(spread.Sigma.Value) : None));
    public static readonly WindowTaper Tukey = new(key: "tukey",
        sample: Shaped(Window.Tukey, static shape => shape is TaperShape.Tapered tapered ? Some(tapered.Fraction.Value) : None));
    public static readonly WindowTaper Kaiser = new(key: "kaiser",
        sample: Shaped(KaiserDesign, static shape => shape is TaperShape.Beta beta ? Some(beta.Value.Value) : None));
    public static readonly WindowTaper Bohman = new(key: "bohman", sample: Fixed(BohmanDesign, None));

    internal TaperKernel Sample { get; }
    public Fin<Arr<double>> Of(Dimension width, TaperFraming framing, Option<TaperShape> shape = default, Op? key = null) =>
        Optional(framing).ToFin(key.OrDefault().InvalidInput()).Bind(row => Sample(width.Value, shape, row, key.OrDefault()));

    private static TaperKernel Fixed(Func<int, double[]> design, Option<Func<int, double[]>> framed) =>
        (width, shape, framing, key) => shape.IsSome
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : framing.Switch(
                filterDesign: () => Fin.Succ(new Arr<double>(design(arg: width))),
                fftFrame: () => framed.ToFin(key.InvalidInput()).Map(twin => new Arr<double>(twin(arg: width))));
    private static TaperKernel Shaped(Func<int, double, double[]> design, Func<TaperShape, Option<double>> parameter) =>
        (width, shape, framing, key) => framing.Switch(
            filterDesign: () => shape.Bind(parameter).ToFin(key.InvalidInput()).Map(value => new Arr<double>(design(arg1: width, arg2: value))),
            fftFrame: () => Fin.Fail<Arr<double>>(key.InvalidInput()));

    private static double[] KaiserDesign(int width, double beta) {
        double norm = SpecialFunctions.BesselI0(x: beta), span = Math.Max(val1: 1, val2: width - 1);
        return [.. Enumerable.Range(start: 0, count: width).Select(n =>
            SpecialFunctions.BesselI0(x: beta * Math.Sqrt(d: Math.Max(val1: 0.0, val2: 1.0 - Math.Pow(x: (2.0 * n / span) - 1.0, y: 2.0)))) / norm)];
    }
    private static double[] BohmanDesign(int width) {
        double span = Math.Max(val1: 1, val2: width - 1);
        return [.. Enumerable.Range(start: 0, count: width).Select(n => Bohmanned(x: Math.Abs(value: (2.0 * n / span) - 1.0)))];
        static double Bohmanned(double x) => ((1.0 - x) * Math.Cos(d: Math.PI * x)) + (Math.Sin(a: Math.PI * x) / Math.PI);
    }
}
```

## [03]-[INTERPOLATE]

- Owner: `Interpolant<TCap>` is the ONE interpolation capability owner — the fitted-curve capsule whose type parameter lifts the package's two runtime support flags into a compile-time capability, so an unsupported `Slope`, `Curvature`, or `Area` call is unspellable rather than a throw; `IInterpolantCapability` with `IDifferentiable`/`IIntegrable` is the marker family and `Smooth`/`Differentiable`/`Sampled` the three inhabited tiers; the `Interpolant` static owner carries one typed factory per package scheme and the two mints the factory roster omits.
- Cases: the tier matrix mirrors the two flags as an ORTHOGONAL pair — `Smooth` (both: natural, Akima, PCHIP, and Hermite cubic, linear, quadratic-segment, step), `Differentiable` (derivative alone: Neville polynomial, log-linear), `Sampled` (neither: Floater-Hormann and Bulirsch-Stoer rational, equidistant barycentric polynomial, transformed fit) — the tiers are DECOMPILE-VERIFIED per class, and no shipped scheme integrates without differentiating, so the fourth corner is uninhabited and mints no marker.
- Entry: `Interpolant.CubicSpline`/`CubicSplineRobust`/`CubicSplineMonotone`/`Hermite`/`Linear`/`Step` mint `Smooth`; `Polynomial`/`LogLinear` mint `Differentiable`; `Common`/`RationalWithoutPoles`/`RationalWithPoles`/`PolynomialEquidistant` mint `Sampled`; `OfSegments` admits per-segment quadratic coefficients through the package constructor and `OfTransformed` composes a domain transform with its inverse around the fit — the two schemes the factory roster omits, reached at their own owners; `Value(t)` reads on every tier and the capability extension blocks reach the rest, `Area(to, from)` carrying the definite integral's lower limit as an `Option` rather than an arity twin whose two spellings meant two different integrals.
- Auto: one `Build` fold ACCUMULATES every misaligned, non-finite, or unordered-sample claim through `Admit.Claims`, so a caller handed a short values column and an unordered abscissa learns both, and captures the throwing package factory through `Op.Catch`; every read is finite-gated on `Fin` with the operation key, because the step scheme returns `NaN` at a sample point and the rational schemes return `NaN` below ULP, so an ungated read poisons a gradient silently.
- Law: `Rasm.Compute` `Tensor/sampling` is the richest consumer and this owner adopts its capability form whole; the former keyed `InterpolationRoute` roster is DELETED — its rows became the typed factories, because a keyed row cannot return a per-row capability type. NAMED LOSS: selecting a scheme by wire key at runtime; no consumer selects one, and a peer that does maps its key onto the factory at its own edge. WITNESS: `InterpolationRoute.CubicSpline.Fit(points, values)` rebuilt as `Interpolant.CubicSpline(points, values)`, the result now typed `Interpolant<Smooth>` whose `Area` compiles.
- Packages: MathNet.Numerics (`Interpolate` roster, `IInterpolation`, `QuadraticSpline`, `TransformedInterpolation`), System.Numerics.Tensors (`TensorPrimitives.IsFiniteAll`), `Numerics/atoms`, Rasm.Domain (`Op`, `Admit.Claims`), LanguageExt.Core, BCL inbox.
- Growth: a scheme is one typed factory returning its tier; a capability is one marker interface with one extension block; a per-scheme class family or a runtime capability boolean is the deleted form.
- Boundary: `MathNet.Numerics.Interpolation` is one-dimensional whole, so a bicubic or scattered-surface reconstruction is the regression route's and never a factory here; the extension blocks are the ONLY reach to `Differentiate`/`Differentiate2`/`Integrate`, so a consumer holding an `Interpolant<Sampled>` cannot spell them.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using LanguageExt;
using MathNet.Numerics;
using MathNet.Numerics.Interpolation;
using Rasm.Domain;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IInterpolantCapability { }
public interface IDifferentiable : IInterpolantCapability { }
public interface IIntegrable : IInterpolantCapability { }

public readonly struct Smooth : IDifferentiable, IIntegrable { }
public readonly struct Differentiable : IDifferentiable { }
public readonly struct Sampled : IInterpolantCapability { }

// --- [MODELS] --------------------------------------------------------------------------
public sealed class Interpolant<TCap> where TCap : IInterpolantCapability {
    internal Interpolant(IInterpolation curve) => Curve = curve;
    internal IInterpolation Curve { get; }

    public Fin<double> Value(double t, Op? key = null) => Interpolant.Finite(value: Curve.Interpolate(t: t), key: key.OrDefault());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Interpolant {
    public static Fin<Interpolant<Smooth>> CubicSpline(Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Smooth>(points, values, key.OrDefault(), static (p, v) => Interpolate.CubicSpline(p, v));
    public static Fin<Interpolant<Smooth>> CubicSplineRobust(Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Smooth>(points, values, key.OrDefault(), static (p, v) => Interpolate.CubicSplineRobust(p, v));
    public static Fin<Interpolant<Smooth>> CubicSplineMonotone(Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Smooth>(points, values, key.OrDefault(), static (p, v) => Interpolate.CubicSplineMonotone(p, v));
    public static Fin<Interpolant<Smooth>> Hermite(Arr<double> points, Arr<double> values, Arr<double> slopes, Op? key = null) {
        Op op = key.OrDefault();
        return slopes.Count == points.Count && TensorPrimitives.IsFiniteAll<double>(slopes.AsSpan())
            ? Build<Smooth>(points, values, op, (p, v) => Interpolate.CubicSplineWithDerivatives(p, v, slopes.AsIterable()))
            : Fin.Fail<Interpolant<Smooth>>(op.InvalidInput());
    }
    public static Fin<Interpolant<Smooth>> Linear(Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Smooth>(points, values, key.OrDefault(), static (p, v) => Interpolate.Linear(p, v));
    public static Fin<Interpolant<Smooth>> Step(Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Smooth>(points, values, key.OrDefault(), static (p, v) => Interpolate.Step(p, v));
    public static Fin<Interpolant<Differentiable>> Polynomial(Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Differentiable>(points, values, key.OrDefault(), static (p, v) => Interpolate.Polynomial(p, v));
    public static Fin<Interpolant<Differentiable>> LogLinear(Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Differentiable>(points, values, key.OrDefault(), static (p, v) => Interpolate.LogLinear(p, v));
    public static Fin<Interpolant<Sampled>> Common(Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Sampled>(points, values, key.OrDefault(), static (p, v) => Interpolate.Common(p, v));
    public static Fin<Interpolant<Sampled>> RationalWithoutPoles(Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Sampled>(points, values, key.OrDefault(), static (p, v) => Interpolate.RationalWithoutPoles(p, v));
    public static Fin<Interpolant<Sampled>> RationalWithPoles(Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Sampled>(points, values, key.OrDefault(), static (p, v) => Interpolate.RationalWithPoles(p, v));
    public static Fin<Interpolant<Sampled>> PolynomialEquidistant(Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Sampled>(points, values, key.OrDefault(), static (p, v) => Interpolate.PolynomialEquidistant(p, v));

    public static Fin<Interpolant<Smooth>> OfSegments(Arr<double> knots, Arr<double> constant, Arr<double> linear, Arr<double> quadratic, Op? key = null) {
        Op op = key.OrDefault();
        return Admit.Claims(op,
                (knots.Count >= 2, "knots-extent"),
                (constant.Count == knots.Count - 1, "constant-extent"),
                (linear.Count == constant.Count, "linear-extent"),
                (quadratic.Count == constant.Count, "quadratic-extent"),
                (TensorPrimitives.IsFiniteAll<double>(knots.AsSpan()), "knots-finite"),
                (TensorPrimitives.IsFiniteAll<double>(constant.AsSpan()), "constant-finite"),
                (TensorPrimitives.IsFiniteAll<double>(linear.AsSpan()), "linear-finite"),
                (TensorPrimitives.IsFiniteAll<double>(quadratic.AsSpan()), "quadratic-finite"),
                (Ascending(knots), "knots-ascending"))
            .Bind(_ => op.Catch(() => Fin.Succ(new Interpolant<Smooth>(new QuadraticSpline(
                x: [.. knots.AsIterable()], c0: [.. constant.AsIterable()], c1: [.. linear.AsIterable()], c2: [.. quadratic.AsIterable()])))));
    }
    public static Fin<Interpolant<Sampled>> OfTransformed(Func<double, double> transform, Func<double, double> inverse, Arr<double> points, Arr<double> values, Op? key = null) =>
        Build<Sampled>(points, values, key.OrDefault(), (p, v) => TransformedInterpolation.Interpolate(transform: transform, transformInverse: inverse, x: p, y: v));

    private static Fin<Interpolant<TCap>> Build<TCap>(Arr<double> points, Arr<double> values, Op key, Func<IEnumerable<double>, IEnumerable<double>, IInterpolation> factory)
        where TCap : IInterpolantCapability =>
        Admit.Claims(key,
                (points.Count >= 2, "points-extent"),
                (points.Count == values.Count, "sample-arity"),
                (TensorPrimitives.IsFiniteAll<double>(points.AsSpan()), "points-finite"),
                (TensorPrimitives.IsFiniteAll<double>(values.AsSpan()), "values-finite"),
                (Ascending(points), "points-ascending"))
            .Bind(_ => key.Catch(() => Fin.Succ(new Interpolant<TCap>(factory(arg1: points.AsIterable(), arg2: values.AsIterable())))));
    private static bool Ascending(Arr<double> points) =>
        Enumerable.Range(start: 1, count: points.Count - 1).All(index => points[index - 1] < points[index]);
    internal static Fin<double> Finite(double value, Op key) =>
        double.IsFinite(value) ? key.AcceptValue(value: value) : Fin.Fail<double>(key.InvalidResult());

    extension<TCap>(Interpolant<TCap> self) where TCap : IDifferentiable {
        public Fin<double> Slope(double t, Op? key = null) => Finite(value: self.Curve.Differentiate(t: t), key: key.OrDefault());
        public Fin<double> Curvature(double t, Op? key = null) => Finite(value: self.Curve.Differentiate2(t: t), key: key.OrDefault());
    }
    extension<TCap>(Interpolant<TCap> self) where TCap : IIntegrable {
        public Fin<double> Area(double to, Option<double> from = default, Op? key = null) =>
            Finite(value: from.Match(Some: a => self.Curve.Integrate(a: a, b: to), None: () => self.Curve.Integrate(t: to)), key: key.OrDefault());
    }
}
```

## [04]-[SPECTRAL]

- Owner: `SpectralArena` is the ONE transform carrier — four cases, each holding the buffer layout its MathNet entrypoint owns and exactly the extent its arm consumes; `Spectrum` the evidence a transform leaves; `SpectralScaling` the declared convention row governing both transform owners at once and `SpectralSense` the direction row carrying the four entrypoint pairs as columns; `TapSeries` the admitted sample-domain convolution kernel, `TapBorder` its closed out-of-extent vocabulary answering an `Option<int>` so an absent tap is a carrier and not a sentinel, and `TapWindow` the staged-window geometry a banded caller admits through its own gated `Of`; the `MatrixKernel` `partial` half on this page is the one path to every transform, power, axis, modulation, and tap-fold body.
- Entry: `arena.Transform(sense, scaling, key)` is the one transform entry and the arena case its discriminant, so no per-carrier entrypoint family and no mode flag exist; `spectrum.Power`/`Axis`/`Modulate` read and re-mint off the spectrum; `series.Convolve(source, folded, window, border, key)` folds one strided axis in the sample domain and `lattice.Convolve(values, axes, border, key)` is its separable lattice form on the ADDRESSING owner, the lattice being the discriminant — one series per axis, never a static twin wearing the instance member's name.
- Auto: rank 2 and rank 3 ARE the row-column fold over the managed-complete 1D pair (Radix-2 at a power of two, Bluestein otherwise), and symmetric scaling composes per axis (`1/sqrt(w) · 1/sqrt(h) = 1/sqrt(w·h)`), so the folded transform carries the convention the 1D row declares and `RoundTripFactor` reads the cell count once; the tap fold divides every output by its RESOLVED-weight sum, so partition of unity holds at every border by construction — no caller pre-normalizes a table, a series and its scalar multiple fold identically, a `Zero`-dropped tap leaves the divisor rather than darkening the rim, and a rim record whose resolved sum cancels refuses typed rather than certifying a fabricated zero sample; the lattice tap fold is the SAME per-axis line fold the rank-2/3 transform takes, walking the lattice's own linearization strides; `Power` reads ONE pair fold across the interleaved and packed layouts — byte-identical `(re, im)` runs a `MemoryMarshal.Cast` unifies — beside the vectorized multiply-then-multiply-add pair on the split spans, the reason the split case exists, and never a square root it only squares back; `Axis` reads ONE `SpectralArena.Metric(ordinal)` fold — bin count and sampling rate per case, the lattice arm reading `CellLattice.Extent`/`Spacing` — so a spectrum reads its own axis instead of a caller-passed rate that can disagree with the grid, and an out-of-rank ordinal states absence once rather than at four call sites. Every multi-column admission on the page ACCUMULATES through `Admit.Claims`, each clause naming its axis.
- Law: `SpectralScaling` publishes both convention columns so a package binding MathNet's transform entrypoints directly reads the declared row instead of re-spelling a second `FourierOptions` vocabulary; `Rasm.Compute` `Stats/signal` composes it and its eight raw `FourierOptions` sites are the deleted form.
- Packages: MathNet.Numerics (`Fourier` interleaved, split, and packed pairs, `FrequencyScale`, `Hartley.NaiveForward`/`NaiveInverse`, `FourierOptions`/`HartleyOptions`), System.Numerics.Tensors (`TensorPrimitives.Multiply`/`MultiplyAdd`/`Sum`/`IsFiniteAll`), CommunityToolkit.HighPerformance (`MemoryOwner<T>.Allocate` — the lattice staging pair, the squaring buffer, and the Hartley reflection copy; the separable line stays an exact-extent array because the package entrypoint transforms its whole length), `Numerics/atoms` (`CellLattice` with its per-axis `Extent`/`Stride`/`Spacing`, `Dimension`, `PositiveMagnitude`, `SignedAxis`, `EpsilonPolicy`), Rasm.Domain (`Op`, `Admit.Claims`, `Admit.FiniteComplexSpan`, `ValidityClaim`), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL (`System.Numerics.Complex`, `MemoryMarshal.Cast`).
- Growth: a border law is one `TapBorder` row every tap fold reads with no kernel edit; a scaling convention is one `SpectralScaling` row governing both owners at once; a buffer layout is one `SpectralArena` case whose arms break every fold at compile time.
- Boundary: `Fourier.Forward2D`/`Inverse2D`/`ForwardMultiDim`/`InverseMultiDim` never spell in a fence — all four route to the multidim provider interface whose managed realization throws `NotSupportedException`, and the admitted native adapters ship no arm64 asset, so the managed-provider pin makes them unservable by construction. Every transform overwrites the caller's arena, so an immutable spectrum value is unrepresentable and `Spectrum` names the arena the result lives in — the same instance for the three in-place cases, a fresh one for the Hartley case, the sole entrypoint that allocates its output. Separable convolution has NO package primitive — `System.Numerics.Tensors` carries no `Conv1D`, `Conv2D`, `Conv3D`, or `MatMul` — so this band owns BOTH routes of the one convolution correspondence itself: the pointwise spectral product between the transform legs (`Spectrum.Modulate`) and the sample-domain tap fold (`TapSeries.Convolve`); a consumer composes one of the two and spells no fold of its own, while its tap GENERATION stays the consumer's domain policy. Zero-sum series are DIFFERENCE stencils and refuse at the mint: `Numerics/calculus#NABLA` owns those, so the two owners partition on the tap sum rather than overlapping. `CellLattice` is the addressing carrier for a lattice-backed plane and owns the per-axis `Extent`/`Stride`/`Spacing` read every separable walk takes, so the band mints no second linearization, no sibling 2D arena, and no strided-view owner beside it — the `Tensor<T>` plane stays refused on four structural grounds: array-only static entrypoints at the mint, `ref struct` span views that cannot cross `Fin`, an allocating `PermuteDimensions` on every transpose, and this carrier's one-linearization law. Named statement-kernel exemption covers the separable axis gather-scatter and the tap-fold record walk — measured strided-line hot paths.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Linq;
using System.Numerics;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance.Buffers;
using LanguageExt;
using LanguageExt.Common;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SpectralScaling {
    public static readonly SpectralScaling Symmetric = new(key: 0,
        fourierConvention: FourierOptions.Default, hartleyConvention: HartleyOptions.Default, roundTrip: static _ => 1.0);
    public static readonly SpectralScaling AsymmetricInverse = new(key: 1,
        fourierConvention: FourierOptions.AsymmetricScaling, hartleyConvention: HartleyOptions.AsymmetricScaling, roundTrip: static _ => 1.0);
    public static readonly SpectralScaling Unscaled = new(key: 2,
        fourierConvention: FourierOptions.NoScaling, hartleyConvention: HartleyOptions.NoScaling, roundTrip: static cells => (double)cells);
    public FourierOptions FourierConvention { get; }
    public HartleyOptions HartleyConvention { get; }
    [UseDelegateFromConstructor] public partial double RoundTrip(long cells);
}

[SmartEnum<int>]
public sealed partial class SpectralSense {
    public static readonly SpectralSense Forward = new(key: 0,
        interleaved: Fourier.Forward, split: Fourier.Forward, packed: Fourier.ForwardReal, realValued: Hartley.NaiveForward);
    public static readonly SpectralSense Inverse = new(key: 1,
        interleaved: Fourier.Inverse, split: Fourier.Inverse, packed: Fourier.InverseReal, realValued: Hartley.NaiveInverse);
    [UseDelegateFromConstructor] internal partial void Interleaved(Complex[] arena, FourierOptions options);
    [UseDelegateFromConstructor] internal partial void Split(double[] real, double[] imaginary, FourierOptions options);
    [UseDelegateFromConstructor] internal partial void Packed(double[] arena, int samples, FourierOptions options);
    [UseDelegateFromConstructor] internal partial double[] RealValued(double[] samples, HartleyOptions options);
}

[SmartEnum<int>]
public sealed partial class TapBorder {
    public static readonly TapBorder Clamp = new(key: 0, resolve: static (index, extent) => Some(Math.Clamp(value: index, min: 0, max: extent - 1)));
    public static readonly TapBorder Wrap = new(key: 1, resolve: static (index, extent) => Some(((index % extent) + extent) % extent));
    public static readonly TapBorder Mirror = new(key: 2, resolve: static (index, extent) => Some(Reflected(index: index, extent: extent)));
    public static readonly TapBorder Zero = new(key: 3, resolve: static (_, _) => Option<int>.None);

    [UseDelegateFromConstructor] internal partial Option<int> Resolve(int index, int extent);

    private static int Reflected(int index, int extent) {
        int period = Math.Max(val1: 1, val2: (extent - 1) * 2);
        int folded = ((index % period) + period) % period;
        return folded < extent ? folded : period - folded;
    }
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

    public static Fin<TapWindow> Of(Dimension extent, Dimension stride, int origin, int from, Dimension run, Op? key = null) {
        Op op = key.OrDefault();
        return Admit.Claims(op,
                (origin >= 0, "origin"),
                (from >= 0, "from"),
                (origin <= from, "origin-precedes-from"),
                (from + run.Value <= extent.Value, "run-within-extent"))
            .Map(_ => new TapWindow(extent: extent.Value, origin: origin, from: from, run: run.Value, stride: stride.Value));
    }
    internal static TapWindow Whole(Dimension extent, Dimension stride) =>
        new(extent: extent.Value, origin: 0, from: 0, run: extent.Value, stride: stride.Value);
    public bool IsValid => ValidityClaim.All(
        Extent >= 1, Stride >= 1, Run >= 1, From >= 0, Origin >= 0, Origin <= From, From + Run <= Extent);
}

public readonly record struct TapSeries : IValidityEvidence {
    private TapSeries(Arr<double> taps) => Taps = taps;

    public Arr<double> Taps { get; }
    public int Radius => Taps.Count / 2;
    public bool IsValid => ValidityClaim.All(Taps.Count >= 1);

    public static Fin<TapSeries> Of(Arr<double> taps, Op? key = null) =>
        taps.Count >= 1 && int.IsOddInteger(taps.Count) && TensorPrimitives.IsFiniteAll<double>(taps.AsSpan())
            && Math.Abs(value: TensorPrimitives.Sum<double>(taps.AsSpan())) > EpsilonPolicy.ZeroTolerance
            ? Fin.Succ(new TapSeries(taps: taps))
            : Fin.Fail<TapSeries>(error: key.OrDefault().InvalidInput());

    public Fin<Unit> Convolve(ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border, Op? key = null) =>
        MatrixKernel.TapFold(series: this, source: source, folded: folded, window: window, border: border, key: key.OrDefault());
}

public static class LatticeConvolution {
    extension(CellLattice lattice) {
        public Fin<Unit> Convolve(Span<double> values, Arr<TapSeries> axes, TapBorder border, Op? key = null) =>
            MatrixKernel.TapFoldLattice(values: values, lattice: lattice, axes: axes, border: border, key: key.OrDefault());
    }
}

[Union]
public abstract partial record SpectralArena : IValidityEvidence {
    private SpectralArena() { }
    public sealed record Interleaved(Complex[] Values, CellLattice Lattice) : SpectralArena;
    public sealed record Split(double[] Real, double[] Imaginary, PositiveMagnitude Rate) : SpectralArena;
    public sealed record HalfSpectrum(double[] Values, Dimension Samples, PositiveMagnitude Rate) : SpectralArena;
    public sealed record RealValued(Arr<double> Samples, PositiveMagnitude Rate) : SpectralArena;

    public Fin<Spectrum> Transform(SpectralSense sense, SpectralScaling scaling, Op? key = null) =>
        MatrixKernel.SpectralTransform(arena: this, sense: sense, scaling: scaling, key: key.OrDefault());
    public bool IsValid => ValidityClaim.All(Switch(
        interleaved: static a => a.Values.Length == a.Lattice.CellCount && Admit.FiniteComplexSpan(a.Values.AsSpan()),
        split: static s => s.Real.Length >= 1 && s.Real.Length == s.Imaginary.Length
            && TensorPrimitives.IsFiniteAll<double>(s.Real) && TensorPrimitives.IsFiniteAll<double>(s.Imaginary),
        halfSpectrum: static h => h.Values.Length >= PackedLength(samples: h.Samples.Value) && TensorPrimitives.IsFiniteAll<double>(h.Values),
        realValued: static r => r.Samples.Count >= 1 && TensorPrimitives.IsFiniteAll<double>(r.Samples.AsSpan())));
    public int Rank => Switch(interleaved: static a => a.Lattice.Rank, split: static _ => 1, halfSpectrum: static _ => 1, realValued: static _ => 1);
    public long Cells => Switch(
        interleaved: static a => a.Lattice.CellCount,
        split: static s => (long)s.Real.Length,
        halfSpectrum: static h => (long)h.Samples.Value,
        realValued: static r => (long)r.Samples.Count);
    internal Option<(int Count, double SampleRate)> Metric(int ordinal) => Switch(
        state: ordinal,
        interleaved: static (o, a) => o >= 0 && o < a.Lattice.Rank
            ? Some((Count: a.Lattice.Extent(ordinal: o).Value, SampleRate: 1.0 / a.Lattice.Spacing(ordinal: o)))
            : Option<(int, double)>.None,
        split: static (o, a) => o is 0 ? Some((Count: a.Real.Length, SampleRate: a.Rate.Value)) : Option<(int, double)>.None,
        halfSpectrum: static (o, a) => o is 0 ? Some((Count: a.Samples.Value, SampleRate: a.Rate.Value)) : Option<(int, double)>.None,
        realValued: static (o, a) => o is 0 ? Some((Count: a.Samples.Count, SampleRate: a.Rate.Value)) : Option<(int, double)>.None);
    internal static int PackedLength(int samples) => int.IsEvenInteger(samples) ? samples + 2 : samples + 1;
}

public readonly record struct Spectrum(SpectralArena Arena, SpectralSense Sense, SpectralScaling Scaling, double Energy) : IValidityEvidence {
    public int Rank => Arena.Rank;
    public long Cells => Arena.Cells;
    public double RoundTripFactor => Scaling.RoundTrip(cells: Cells);
    public bool IsValid => ValidityClaim.All(
        Arena is not null && Sense is not null && Scaling is not null && Arena.IsValid,
        ValidityClaim.Finite(Energy),
        ValidityClaim.Nonnegative(value: Energy),
        Cells >= 1L && Rank >= 1);
    public Fin<Arr<double>> Power(Op? key = null) => MatrixKernel.SpectralPower(arena: Arena, key: key.OrDefault());
    public Fin<Arr<double>> Axis(SignedAxis axis, Op? key = null) => MatrixKernel.SpectralAxis(arena: Arena, axis: axis, key: key.OrDefault());
    public Fin<Spectrum> Modulate(ReadOnlySpan<Complex> symbol, Op? key = null) =>
        MatrixKernel.SpectralModulate(spectrum: this, symbol: symbol, key: key.OrDefault());
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static partial class MatrixKernel {
    // --- [SPECTRAL] --------------------------------------------------------------------
    internal static Fin<Spectrum> SpectralTransform(SpectralArena arena, SpectralSense sense, SpectralScaling scaling, Op key) =>
        arena is null || sense is null || scaling is null || !arena.IsValid
            ? Fin.Fail<Spectrum>(key.InvalidInput())
            : key.Catch(() => SpectrumOf(arena: Transformed(arena: arena, sense: sense, scaling: scaling), sense: sense, scaling: scaling, key: key));
    private static SpectralArena Transformed(SpectralArena arena, SpectralSense sense, SpectralScaling scaling) =>
        arena.Switch(
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
                Samples: new Arr<double>(s.Sense.RealValued(samples: [.. a.Samples.AsIterable()], options: s.Scaling.HartleyConvention)), Rate: a.Rate));
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
    private static Fin<Spectrum> SpectrumOf(SpectralArena arena, SpectralSense sense, SpectralScaling scaling, Op key) =>
        SpectralPower(arena: arena, key: key).Bind(power => {
            Spectrum spectrum = new(Arena: arena, Sense: sense, Scaling: scaling, Energy: TensorPrimitives.Sum<double>(power.AsSpan()));
            return spectrum.IsValid ? Fin.Succ(spectrum) : Fin.Fail<Spectrum>(key.InvalidResult());
        });
    internal static Fin<Arr<double>> SpectralPower(SpectralArena arena, Op key) =>
        arena is null || !arena.IsValid
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : key.Catch(() => Fin.Succ(arena.Switch(
                interleaved: static a => PairPower(pairs: MemoryMarshal.Cast<Complex, double>(a.Values), bins: a.Values.Length),
                split: static s => SplitPower(real: s.Real, imaginary: s.Imaginary),
                halfSpectrum: static h => PairPower(pairs: h.Values, bins: SpectralArena.PackedLength(samples: h.Samples.Value) / 2),
                realValued: static r => HartleyPower(samples: r.Samples))));
    private static Arr<double> SplitPower(ReadOnlySpan<double> real, ReadOnlySpan<double> imaginary) {
        double[] power = new double[real.Length];
        TensorPrimitives.Multiply<double>(real, real, power);
        TensorPrimitives.MultiplyAdd<double>(imaginary, imaginary, power, power);
        return new Arr<double>(power);
    }
    private static Arr<double> PairPower(ReadOnlySpan<double> pairs, int bins) {
        using MemoryOwner<double> squares = MemoryOwner<double>.Allocate(size: bins * 2);
        TensorPrimitives.Multiply<double>(pairs[..(bins * 2)], pairs[..(bins * 2)], squares.Span);
        double[] power = new double[bins];
        for (int bin = 0; bin < bins; bin++) { power[bin] = squares.Span[2 * bin] + squares.Span[(2 * bin) + 1]; }
        return new Arr<double>(power);
    }
    private static Arr<double> HartleyPower(Arr<double> samples) {
        int n = samples.Count;
        using MemoryOwner<double> reflected = MemoryOwner<double>.Allocate(size: n);
        samples.AsSpan().CopyTo(reflected.Span);
        reflected.Span[1..].Reverse();
        double[] power = new double[n];
        TensorPrimitives.Multiply<double>(samples.AsSpan(), samples.AsSpan(), power);
        TensorPrimitives.MultiplyAdd<double>(reflected.Span, reflected.Span, power, power);
        TensorPrimitives.Multiply<double>(power, 0.5, power);
        return new Arr<double>(power);
    }
    internal static Fin<Arr<double>> SpectralAxis(SpectralArena arena, SignedAxis axis, Op key) =>
        arena is null || axis is null || !arena.IsValid
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : arena.Metric(ordinal: Math.Abs(value: axis.Key) - 1)
                .ToFin(key.InvalidInput(axis: "spectral-ordinal"))
                .Bind(metric => AxisOf(metric: metric, key: key));
    private static Fin<Arr<double>> AxisOf((int Count, double SampleRate) metric, Op key) =>
        metric.Count < 1 || !double.IsFinite(metric.SampleRate) || metric.SampleRate <= 0.0
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : key.Catch(() => {
                Arr<double> bins = new(Fourier.FrequencyScale(length: metric.Count, sampleRate: metric.SampleRate));
                return TensorPrimitives.IsFiniteAll<double>(bins.AsSpan()) ? Fin.Succ(bins) : Fin.Fail<Arr<double>>(key.InvalidResult());
            });

    internal static Fin<Spectrum> SpectralModulate(Spectrum spectrum, ReadOnlySpan<Complex> symbol, Op key) =>
        spectrum.Arena is SpectralArena.Interleaved plane && plane.Values.Length == symbol.Length && Admit.FiniteComplexSpan(symbol)
            ? Modulated(plane: plane, symbol: symbol, spectrum: spectrum, key: key)
            : Fin.Fail<Spectrum>(key.InvalidInput());
    private static Fin<Spectrum> Modulated(SpectralArena.Interleaved plane, ReadOnlySpan<Complex> symbol, Spectrum spectrum, Op key) {
        TensorPrimitives.Multiply<Complex>(plane.Values, symbol, plane.Values);
        return SpectrumOf(arena: plane, sense: spectrum.Sense, scaling: spectrum.Scaling, key: key);
    }

    // --- [TAP_FOLD] --------------------------------------------------------------------
    internal static Fin<Unit> TapFold(TapSeries series, ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border, Op key) {
        int stride = window.Stride, staged = stride >= 1 ? source.Length / stride : 0;
        bool whole = window.Origin == 0 && staged == window.Extent;
        return Admit.Claims(key,
                (series.IsValid, "series"),
                (window.IsValid, "window"),
                (border is not null, "border"),
                (source.Length == staged * stride, "source-extent"),
                (folded.Length == window.Run * stride, "folded-extent"),
                (window.Origin <= Math.Max(val1: 0, val2: window.From - series.Radius), "staging-head"),
                (window.Origin + staged > Math.Min(val1: window.Extent - 1, val2: window.From + window.Run - 1 + series.Radius), "staging-tail"),
                (whole || border == TapBorder.Zero, "partial-window-border"))
            .Bind(_ => TapFoldCore(series: series, source: source, folded: folded, window: window, border: border, key: key));
    }
    internal static Fin<Unit> TapFoldLattice(Span<double> values, CellLattice lattice, Arr<TapSeries> axes, TapBorder border, Op key) {
        Fin<Unit> admitted = Admit.Claims(key,
            (border is not null, "border"),
            (axes.Count == lattice.Rank, "axis-arity"),
            (values.Length == lattice.CellCount, "value-extent"),
            (axes.ForAll(static series => series.IsValid), "axis-series"));
        if (admitted.IsFail) { return admitted; }
        int cells = values.Length;
        int longest = Math.Max(val1: lattice.Columns.Value, val2: Math.Max(val1: lattice.Rows.Value, val2: lattice.Layers.Value));
        using MemoryOwner<double> staging = MemoryOwner<double>.Allocate(size: longest * 2);
        Span<double> line = staging.Span[..longest], result = staging.Span[longest..];
        for (int axis = 0; axis < axes.Count; axis++) {
            int count = lattice.Extent(ordinal: axis).Value, stride = lattice.Stride(ordinal: axis);
            TapWindow window = TapWindow.Whole(extent: lattice.Extent(ordinal: axis), stride: Dimension.Create(value: 1));
            for (int origin = 0; origin < cells; origin++) {
                if (origin / stride % count != 0) { continue; }
                for (int k = 0; k < count; k++) { line[k] = values[origin + (k * stride)]; }
                Fin<Unit> lineFold = TapFoldCore(series: axes[axis], source: line[..count], folded: result[..count], window: window, border: border, key: key);
                if (lineFold.IsFail) { return lineFold; }
                for (int k = 0; k < count; k++) { values[origin + (k * stride)] = result[k]; }
            }
        }
        return Fin.Succ(unit);
    }
    private static Fin<Unit> TapFoldCore(TapSeries series, ReadOnlySpan<double> source, Span<double> folded, TapWindow window, TapBorder border, Op key) {
        ReadOnlySpan<double> taps = series.Taps.AsSpan();
        int radius = series.Radius, stride = window.Stride;
        for (int at = 0; at < window.Run; at++) {
            int record = window.From + at, seat = at * stride;
            Span<double> lane = folded.Slice(seat, stride);
            lane.Clear();
            double admitted = 0.0;
            for (int tap = -radius; tap <= radius; tap++) {
                int logical = record + tap;
                Option<int> resolved = logical >= 0 && logical < window.Extent
                    ? Some(logical)
                    : border.Resolve(index: logical, extent: window.Extent);
                if (resolved.IsNone) { continue; }
                double weight = taps[tap + radius];
                admitted += weight;
                int from = (resolved.IfNone(0) - window.Origin) * stride;
                TensorPrimitives.MultiplyAdd<double>(source.Slice(from, stride), weight, lane, lane);
            }
            if (Math.Abs(value: admitted) <= EpsilonPolicy.ZeroTolerance) {
                return Fin.Fail<Unit>(key.InvalidResult(detail: $"resolved tap-weight sum cancelled at record {record}"));
            }
            TensorPrimitives.Multiply<double>(lane, 1.0 / admitted, lane);
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

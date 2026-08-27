# `transform.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Numerics/transform.md`

Authority: `CLAUDE.md`; monorepo, .NET, and `Rasm` planning law; the full `docs/stacks/csharp/` doctrine; the checked-in LanguageExt, Thinktecture, MathNet.Numerics, tensor, and HighPerformance API catalogs; the package-specific `.api` tier (which contains no dependency used by this page); and every direct `libs/dotnet/` consumer. The queue preserves `WindowTaper`, `Interpolant<TCapability>`, `SpectralArena`, and the `MatrixKernel` partial as the four existing owners. It does not add a second roster, carrier, wrapper, or kernel.

Apply the moves in order. Each numbered subchange is a bounded replacement; mechanically repeated row edits are split so no code replacement exceeds ten lines.

## 1. Remove keys from all four behavior-only rosters

Location: `WindowTaper`, `SpectralScaling`, `SpectralSense`, and `TapBorder` declarations and rows.

### 1a. Make scaling keyless

From:

```csharp
[SmartEnum<int>]
public sealed partial class SpectralScaling {
    public static readonly SpectralScaling Symmetric = new(key: 0,
        fourierConvention: FourierOptions.Default, hartleyConvention: HartleyOptions.Default, roundTrip: static _ => 1.0);
    public static readonly SpectralScaling AsymmetricInverse = new(key: 1,
        fourierConvention: FourierOptions.AsymmetricScaling, hartleyConvention: HartleyOptions.AsymmetricScaling, roundTrip: static _ => 1.0);
    public static readonly SpectralScaling Unscaled = new(key: 2,
        fourierConvention: FourierOptions.NoScaling, hartleyConvention: HartleyOptions.NoScaling, roundTrip: static cells => (double)cells);
```

To:

```csharp
[SmartEnum]
public sealed partial class SpectralScaling {
    public static readonly SpectralScaling Symmetric = new(
        fourierConvention: FourierOptions.Default, hartleyConvention: HartleyOptions.Default, roundTrip: static _ => 1.0);
    public static readonly SpectralScaling AsymmetricInverse = new(
        fourierConvention: FourierOptions.AsymmetricScaling, hartleyConvention: HartleyOptions.AsymmetricScaling, roundTrip: static _ => 1.0);
    public static readonly SpectralScaling Unscaled = new(
        fourierConvention: FourierOptions.NoScaling, hartleyConvention: HartleyOptions.NoScaling, roundTrip: static cells => (double)cells);
```

### 1b. Make direction and border keyless

From:

```csharp
[SmartEnum<int>]
public sealed partial class SpectralSense {
    public static readonly SpectralSense Forward = new(key: 0,
        interleaved: Fourier.Forward, split: Fourier.Forward, packed: Fourier.ForwardReal, realValued: Hartley.NaiveForward);
    public static readonly SpectralSense Inverse = new(key: 1,
        interleaved: Fourier.Inverse, split: Fourier.Inverse, packed: Fourier.InverseReal, realValued: Hartley.NaiveInverse);
```

To:

```csharp
[SmartEnum]
public sealed partial class SpectralSense {
    public static readonly SpectralSense Forward = new(
        interleaved: Fourier.Forward, split: Fourier.Forward, packed: Fourier.ForwardReal, realValued: Hartley.NaiveForward);
    public static readonly SpectralSense Inverse = new(
        interleaved: Fourier.Inverse, split: Fourier.Inverse, packed: Fourier.InverseReal, realValued: Hartley.NaiveInverse);
```

From:

```csharp
[SmartEnum<int>]
public sealed partial class TapBorder {
    public static readonly TapBorder Clamp = new(key: 0, resolve: static (index, extent) => Some(Math.Clamp(value: index, min: 0, max: extent - 1)));
    public static readonly TapBorder Wrap = new(key: 1, resolve: static (index, extent) => Some(((index % extent) + extent) % extent));
    public static readonly TapBorder Mirror = new(key: 2, resolve: static (index, extent) => Some(Reflected(index: index, extent: extent)));
    public static readonly TapBorder Zero = new(key: 3, resolve: static (_, _) => Option<int>.None);
```

To:

```csharp
[SmartEnum]
public sealed partial class TapBorder {
    public static readonly TapBorder Clamp = new(resolve: static (index, extent) => Some(Math.Clamp(value: index, min: 0, max: extent - 1)));
    public static readonly TapBorder Wrap = new(resolve: static (index, extent) => Some(((index % extent) + extent) % extent));
    public static readonly TapBorder Mirror = new(resolve: static (index, extent) => Some(Reflected(index: index, extent: extent)));
    public static readonly TapBorder Zero = new(resolve: static (_, _) => Option<int>.None);
```

### 1c. Remove the unsupported wire identity from the taper roster

From:

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class WindowTaper {
```

To:

```csharp
[SmartEnum]
public sealed partial class WindowTaper {
```

Mechanically remove `key: "...", ` from each of the seventeen row constructors. The row names remain the vocabulary; no replacement string table is introduced.

Effect: target fenced LOC `-1`; module-level and authored member symbols unchanged; generated public `Key`, `Get`, and `TryGet` surfaces disappear from four types, together with their keyed conversion/validation surfaces. Nine meaningless integer literals, seventeen unsupported wire literals, and one comparer attribute disappear.

API/consumer proof: checked-in Thinktecture declares `[SmartEnum]` as the process-local behavior owner and `[SmartEnum<TKey>]` as the wire-keyed vocabulary owner. An exhaustive hidden/ignored-inclusive search finds no `libs/dotnet/` read of `WindowTaper.Key`, no lookup or parse by taper text, and no DTO/schema field carrying a `WindowTaper` key. Some of the same words occur as keys of unrelated owners (`MipPolicy.Kaiser`, `DensityKernel.Triangular`, and others); none resolves or serializes this roster. Every actual `WindowTaper` consumer names a singleton row directly. The target's sentence that these are “wire spellings every peer already speaks” has no owning boundary or consumer and cannot manufacture wire identity by assertion. The three spectral rosters likewise have no key consumer; `TapBorder` equality compares singleton values and needs no key.

Ripples: remove the target's wire-key claim and describe `WindowTaper` as a process-local behavior roster. No consumer fence changes.

## 2. Put periodic sampling on each `WindowTaper` row; replace the framing roster with one request union

Location: `TaperFraming`, `TaperKernel`, the `WindowTaper` declaration/rows, public sampling entry, and `Fixed`/`Shaped`; `Rasm.Compute/.planning/Stats/signal.md` direct calls.

### 2a. Replace the independent framing and shape axes with one admissible request shape

From:

```csharp
[SmartEnum<int>]
public sealed partial class TaperFraming {
    public static readonly TaperFraming FilterDesign = new(key: 0);
    public static readonly TaperFraming FftFrame = new(key: 1);
}
```

To:

```csharp
[Union]
public abstract partial record TaperSampling {
    private TaperSampling() { }
    public sealed record Symmetric(Option<TaperShape> Shape) : TaperSampling;
    public sealed record Periodic : TaperSampling;
}
```

From:

```csharp
internal delegate Fin<Arr<double>> TaperKernel(int width, Option<TaperShape> shape, TaperFraming framing, Op key);

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class WindowTaper {
```

To:

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class WindowTaper {
    [UseDelegateFromConstructor]
    private partial Fin<Arr<double>> Design(int width, Option<TaperShape> shape, Op key);
```

The generated keyless-smart-enum constructor shape is exactly:

```csharp
private WindowTaper(
    Option<Func<int, double[]>> periodicDesign,
    Func<int, Option<TaperShape>, Op, Fin<Arr<double>>> design);
```

`PeriodicDesign` is the one plain column and therefore precedes the delegate-backed `Design` parameter regardless of member source order; the named arguments below bind those exact generated parameter names.

### 2b. Seat the four periodic package twins as row data

From:

```csharp
public static readonly WindowTaper Hann = new(sample: Fixed(Window.Hann, Some<Func<int, double[]>>(Window.HannPeriodic)));
public static readonly WindowTaper Hamming = new(sample: Fixed(Window.Hamming, Some<Func<int, double[]>>(Window.HammingPeriodic)));
public static readonly WindowTaper Cosine = new(sample: Fixed(Window.Cosine, Some<Func<int, double[]>>(Window.CosinePeriodic)));
public static readonly WindowTaper Lanczos = new(sample: Fixed(Window.Lanczos, Some<Func<int, double[]>>(Window.LanczosPeriodic)));
```

To:

```csharp
public static readonly WindowTaper Hann = new(periodicDesign: Some<Func<int, double[]>>(Window.HannPeriodic), design: Unparameterized(Window.Hann));
public static readonly WindowTaper Hamming = new(periodicDesign: Some<Func<int, double[]>>(Window.HammingPeriodic), design: Unparameterized(Window.Hamming));
public static readonly WindowTaper Cosine = new(periodicDesign: Some<Func<int, double[]>>(Window.CosinePeriodic), design: Unparameterized(Window.Cosine));
public static readonly WindowTaper Lanczos = new(periodicDesign: Some<Func<int, double[]>>(Window.LanczosPeriodic), design: Unparameterized(Window.Lanczos));
```

### 2c. Seat the endpoint-aligned-only rows explicitly

From:

```csharp
public static readonly WindowTaper Blackman = new(sample: Fixed(Window.Blackman, None));
public static readonly WindowTaper BlackmanHarris = new(sample: Fixed(Window.BlackmanHarris, None));
public static readonly WindowTaper BlackmanNuttall = new(sample: Fixed(Window.BlackmanNuttall, None));
public static readonly WindowTaper Nuttall = new(sample: Fixed(Window.Nuttall, None));
public static readonly WindowTaper FlatTop = new(sample: Fixed(Window.FlatTop, None));
public static readonly WindowTaper Bartlett = new(sample: Fixed(Window.Bartlett, None));
public static readonly WindowTaper BartlettHann = new(sample: Fixed(Window.BartlettHann, None));
public static readonly WindowTaper Triangular = new(sample: Fixed(Window.Triangular, None));
```

To:

```csharp
public static readonly WindowTaper Blackman = new(periodicDesign: None, design: Unparameterized(Window.Blackman));
public static readonly WindowTaper BlackmanHarris = new(periodicDesign: None, design: Unparameterized(Window.BlackmanHarris));
public static readonly WindowTaper BlackmanNuttall = new(periodicDesign: None, design: Unparameterized(Window.BlackmanNuttall));
public static readonly WindowTaper Nuttall = new(periodicDesign: None, design: Unparameterized(Window.Nuttall));
public static readonly WindowTaper FlatTop = new(periodicDesign: None, design: Unparameterized(Window.FlatTop));
public static readonly WindowTaper Bartlett = new(periodicDesign: None, design: Unparameterized(Window.Bartlett));
public static readonly WindowTaper BartlettHann = new(periodicDesign: None, design: Unparameterized(Window.BartlettHann));
public static readonly WindowTaper Triangular = new(periodicDesign: None, design: Unparameterized(Window.Triangular));
```

From:

```csharp
public static readonly WindowTaper Dirichlet = new(sample: Fixed(Window.Dirichlet, None));
public static readonly WindowTaper Gauss = new(
    sample: Shaped(Window.Gauss, static shape => shape is TaperShape.Spread spread ? Some(spread.Sigma.Value) : None));
public static readonly WindowTaper Tukey = new(
    sample: Shaped(Window.Tukey, static shape => shape is TaperShape.Tapered tapered ? Some(tapered.Fraction.Value) : None));
public static readonly WindowTaper Kaiser = new(
    sample: Shaped(KaiserDesign, static shape => shape is TaperShape.Beta beta ? Some(beta.Value.Value) : None));
public static readonly WindowTaper Bohman = new(sample: Fixed(BohmanDesign, None));
```

To:

```csharp
public static readonly WindowTaper Dirichlet = new(periodicDesign: None, design: Unparameterized(Window.Dirichlet));
public static readonly WindowTaper Gauss = new(
    periodicDesign: None, design: Parameterized(Window.Gauss, static shape => shape is TaperShape.Spread spread ? Some(spread.Sigma.Value) : None));
public static readonly WindowTaper Tukey = new(
    periodicDesign: None, design: Parameterized(Window.Tukey, static shape => shape is TaperShape.Tapered tapered ? Some(tapered.Fraction.Value) : None));
public static readonly WindowTaper Kaiser = new(
    periodicDesign: None, design: Parameterized(KaiserDesign, static shape => shape is TaperShape.Beta beta ? Some(beta.Value.Value) : None));
public static readonly WindowTaper Bohman = new(periodicDesign: None, design: Unparameterized(BohmanDesign));
```

### 2d. Keep one sampling entry and make invalid shape/framing combinations unrepresentable

From:

```csharp
internal TaperKernel Sample { get; }
public Fin<Arr<double>> Of(Dimension width, TaperFraming framing, Option<TaperShape> shape = default, Op? key = null) =>
    Optional(framing).ToFin(key.OrDefault().InvalidInput()).Bind(row => Sample(width.Value, shape, row, key.OrDefault()));
```

To:

```csharp
private Option<Func<int, double[]>> PeriodicDesign { get; }
public Fin<Arr<double>> Sample(Dimension width, TaperSampling sampling, Op? key = null) {
    Op op = key.OrDefault();
    return Optional(sampling).ToFin(op.InvalidInput()).Bind(request => request.Switch(
        symmetric: symmetric => Design(width.Value, symmetric.Shape, op),
        periodic: _ => PeriodicDesign.ToFin(op.InvalidInput()).Map(design => new Arr<double>(design(arg: width.Value)))));
}
```

### 2e. Delete the internal mode switch

From:

```csharp
private static TaperKernel Fixed(Func<int, double[]> design, Option<Func<int, double[]>> framed) =>
    (width, shape, framing, key) => shape.IsSome
        ? Fin.Fail<Arr<double>>(key.InvalidInput())
        : framing.Switch(
            filterDesign: () => Fin.Succ(new Arr<double>(design(arg: width))),
            fftFrame: () => framed.ToFin(key.InvalidInput()).Map(twin => new Arr<double>(twin(arg: width))));
```

To:

```csharp
private static Func<int, Option<TaperShape>, Op, Fin<Arr<double>>> Unparameterized(Func<int, double[]> design) =>
    (width, shape, key) => shape.IsSome
        ? Fin.Fail<Arr<double>>(key.InvalidInput())
        : Fin.Succ(new Arr<double>(design(arg: width)));
```

From:

```csharp
private static TaperKernel Shaped(Func<int, double, double[]> design, Func<TaperShape, Option<double>> parameter) =>
    (width, shape, framing, key) => framing.Switch(
        filterDesign: () => shape.Bind(parameter).ToFin(key.InvalidInput()).Map(value => new Arr<double>(design(arg1: width, arg2: value))),
        fftFrame: () => Fin.Fail<Arr<double>>(key.InvalidInput()));
```

To:

```csharp
private static Func<int, Option<TaperShape>, Op, Fin<Arr<double>>> Parameterized(Func<int, double, double[]> design, Func<TaperShape, Option<double>> parameter) =>
    (width, shape, key) => shape.Bind(parameter).ToFin(key.InvalidInput())
        .Map(value => new Arr<double>(design(arg1: width, arg2: value)));
```

### 2f. Update the only three direct framing calls through the one request-shaped entry

From, in `Rasm.Compute/.planning/Stats/signal.md`:

```csharp
Window.Of(Dimension.Create(value: Grid.Frame), TaperFraming.FftFrame)
window.Of(Dimension.Create(value: corpus.Window.Frame), TaperFraming.FftFrame)
frames.Window.Of(Dimension.Create(value: frames.Grid.Frame), TaperFraming.FftFrame)
```

To:

```csharp
Window.Sample(Dimension.Create(value: Grid.Frame), new TaperSampling.Periodic())
window.Sample(Dimension.Create(value: corpus.Window.Frame), new TaperSampling.Periodic())
frames.Window.Sample(Dimension.Create(value: frames.Grid.Frame), new TaperSampling.Periodic())
```

### 2g. Gate the sampled coefficients at the operation that produces them

Add `using System.Numerics.Tensors;` to the window fence imports.

From, at the end of the move 2d body:

```csharp
return Optional(sampling).ToFin(op.InvalidInput()).Bind(request => request.Switch(
    symmetric: symmetric => Design(width.Value, symmetric.Shape, op),
    periodic: _ => PeriodicDesign.ToFin(op.InvalidInput()).Map(design => new Arr<double>(design(arg: width.Value)))));
```

To:

```csharp
return Optional(sampling).ToFin(op.InvalidInput()).Bind(request => request.Switch(
        symmetric: symmetric => Design(width.Value, symmetric.Shape, op),
        periodic: _ => PeriodicDesign.ToFin(op.InvalidInput()).Map(design => new Arr<double>(design(arg: width.Value)))))
    .Bind(samples => TensorPrimitives.IsFiniteAll<double>(samples.AsSpan()) ? Fin.Succ(samples) : Fin.Fail<Arr<double>>(op.InvalidResult()));
```

Effect: target fenced LOC approximately `-1`; module-level type symbols are exactly `-1` because `TaperFraming` is replaced in place by `TaperSampling` and the top-level `TaperKernel` disappears. Total authored type declarations are `+1`, not `-1`: the old framing type plus delegate (`2`) become the request owner plus its two nested cases (`3`). That bounded nested-case cost removes a module-level delegate and an illegal public product. Public methods remain `1 -> 1`; generated key/lookup surface disappears from the framing axis; and the invalid `FftFrame + Some(shape)` product cannot be constructed. The sampled-result gate prevents a large but admitted Kaiser beta from turning `BesselI0(beta) / BesselI0(beta)` overflow into a success-shaped `NaN`. The earlier two-method proposal is rejected: `Sample`/`Periodic` would be an entrypoint sibling pair selected by modality, directly violating `docs/stacks/csharp/surfaces-and-dispatch.md`'s request-union law.

API/consumer proof: the `Rasm` ruling is explicit: “a framing is a COLUMN on the row, never a second roster.” MathNet publishes exactly four periodic twins (`HannPeriodic`, `HammingPeriodic`, `CosinePeriodic`, `LanczosPeriodic`) and no periodic member for the other rows. All checked-in consumers request FFT framing only at the three lines above. `Option<Func<int,double[]>>` remains the exact row column, while `TaperSampling.Symmetric(Option<TaperShape>) | Periodic` is the one public request algebra: a shape can travel only on the standard symmetric/filter-design case, and the generated `Switch` keeps dispatch exhaustive. `Symmetric`/`Periodic` are the established window-sampling terms; `Filter` would name a downstream use rather than the coefficient geometry. The union earns its bounded replacement shape: without it, `Option<TaperShape>` cannot distinguish an unparameterized symmetric request from periodic sampling, while two public methods would re-mint modality in their names. `TaperShape` remains separate because it is the symmetric case's payload family; merging all three parameter cases into `TaperSampling` would force the public dispatch to repeat four identical design arms and silently turn every future non-periodic case into another entry modality. The checked Thinktecture catalog explicitly defines `[UseDelegateFromConstructor]` as the delegate-backed method surface and states that generated constructors order every plain column before every delegate, name each delegate parameter from its partial method, and order multiple delegates by partial-method declaration order. The resulting keyless `WindowTaper` constructor therefore has `periodicDesign` first and `design` second, exactly as shown above, without a hand-authored delegate type; `TaperKernel`/`FilterKernel` would duplicate generated shape. The checked tensor catalog supplies the whole-span finite gate; it is result admission, not a second design implementation.

Ripples: update `transform.md` owner/case/entry/growth prose, correct its row census from “thirteen package factories plus two owned designs” to the actual fifteen MathNet factories plus two owned designs, and add the tensor finite-result gate to the window package/automatic-law prose; update `Rasm.Compute/Stats/signal.md` entry/package/boundary prose plus the three calls. `Rasm/RULINGS.md` needs no edit: periodic capability is now literally a column on `WindowTaper`, preserving its settled sentence. No other `libs/dotnet/` code fence names `TaperFraming`.

## 3. Give taper payload cases their actual algorithm names and inline the one-use designs

Location: `TaperShape`, the Gauss/Tukey/Kaiser row extractors, `KaiserDesign`, and `BohmanDesign`.

### 3a. Rename the union cases

From:

```csharp
public sealed record Spread(PositiveMagnitude Sigma) : TaperShape;
public sealed record Tapered(UnitInterval Fraction) : TaperShape;
public sealed record Beta(PositiveMagnitude Value) : TaperShape;
```

To:

```csharp
public sealed record Gaussian(PositiveMagnitude Sigma) : TaperShape;
public sealed record Tukey(UnitInterval Fraction) : TaperShape;
public sealed record Kaiser(PositiveMagnitude Beta) : TaperShape;
```

From:

```csharp
periodicDesign: None, design: Parameterized(Window.Gauss, static shape => shape is TaperShape.Spread spread ? Some(spread.Sigma.Value) : None));
periodicDesign: None, design: Parameterized(Window.Tukey, static shape => shape is TaperShape.Tapered tapered ? Some(tapered.Fraction.Value) : None));
periodicDesign: None, design: Parameterized(KaiserDesign, static shape => shape is TaperShape.Beta beta ? Some(beta.Value.Value) : None));
```

To:

```csharp
periodicDesign: None, design: Parameterized(Window.Gauss, static shape => shape is TaperShape.Gaussian gaussian ? Some(gaussian.Sigma.Value) : None));
periodicDesign: None, design: Parameterized(Window.Tukey, static shape => shape is TaperShape.Tukey tukey ? Some(tukey.Fraction.Value) : None));
periodicDesign: None, design: Parameterized(KaiserDesign, static shape => shape is TaperShape.Kaiser kaiser ? Some(kaiser.Beta.Value) : None));
```

### 3b. Inline the single-use Kaiser design

From:

```csharp
public static readonly WindowTaper Kaiser = new(
    periodicDesign: None, design: Parameterized(KaiserDesign, static shape => shape is TaperShape.Kaiser kaiser ? Some(kaiser.Beta.Value) : None));
```

To:

```csharp
public static readonly WindowTaper Kaiser = new(periodicDesign: None, design: Parameterized(static (width, beta) => {
    if (width is 1) { return [1.0]; }
    double norm = SpecialFunctions.BesselI0(x: beta), span = Math.Max(val1: 1, val2: width - 1);
    return [.. Enumerable.Range(start: 0, count: width).Select(n => SpecialFunctions.BesselI0(
        x: beta * Math.Sqrt(d: Math.Max(val1: 0.0, val2: 1.0 - Math.Pow(x: (2.0 * n / span) - 1.0, y: 2.0)))) / norm)];
}, static shape => shape is TaperShape.Kaiser kaiser ? Some(kaiser.Beta.Value) : None));
```

From:

```csharp
private static double[] KaiserDesign(int width, double beta) {
    double norm = SpecialFunctions.BesselI0(x: beta), span = Math.Max(val1: 1, val2: width - 1);
    return [.. Enumerable.Range(start: 0, count: width).Select(n =>
        SpecialFunctions.BesselI0(x: beta * Math.Sqrt(d: Math.Max(val1: 0.0, val2: 1.0 - Math.Pow(x: (2.0 * n / span) - 1.0, y: 2.0)))) / norm)];
}
```

To:

```csharp
// deleted
```

### 3c. Inline the single-use Bohman design

From:

```csharp
public static readonly WindowTaper Bohman = new(periodicDesign: None, design: Unparameterized(BohmanDesign));
```

To:

```csharp
public static readonly WindowTaper Bohman = new(periodicDesign: None, design: Unparameterized(static width => {
    if (width is 1) { return [1.0]; }
    double span = Math.Max(val1: 1, val2: width - 1);
    return [.. Enumerable.Range(start: 0, count: width).Select(n => {
        double x = Math.Abs(value: (2.0 * n / span) - 1.0);
        return ((1.0 - x) * Math.Cos(d: Math.PI * x)) + (Math.Sin(a: Math.PI * x) / Math.PI);
    })];
}));
```

From:

```csharp
private static double[] BohmanDesign(int width) {
    double span = Math.Max(val1: 1, val2: width - 1);
    return [.. Enumerable.Range(start: 0, count: width).Select(n => Bohmanned(x: Math.Abs(value: (2.0 * n / span) - 1.0)))];
    static double Bohmanned(double x) => ((1.0 - x) * Math.Cos(d: Math.PI * x)) + (Math.Sin(a: Math.PI * x) / Math.PI);
}
```

To:

```csharp
// deleted
```

Effect: target fenced LOC approximately unchanged; private member symbols `-2` and local function symbols `-1`; module/public symbol counts unchanged. The generated union case surface is renamed, not enlarged. The double `.Value.Value` projection and the coined `Bohmanned` helper disappear. Both branch-owned designs now publish the unit window at width one instead of evaluating their general `N - 1` formula at a fabricated denominator.

API/consumer proof: MathNet names the two package factories `Gauss(width, sigma)` and `Tukey(width, alpha)`; the local Bessel fold is specifically Kaiser beta. `Spread`, `Tapered`, and `Beta.Value` do not identify an algorithm if read independently. `PositiveMagnitude` deliberately excludes beta zero because Kaiser at beta zero is the already-owned rectangular/Dirichlet response; it does not preserve a name-only duplicate. At width one, the existing `span = Max(1, width - 1)` makes the only normalized coordinate `-1`: Kaiser returns `1 / I0(beta)` and Bohman returns `0`, although a one-sample taper has no edge and must preserve its sole sample with coefficient one. No external code fence constructs a `TaperShape`, so the rename has only target prose ripples. Both deleted methods have exactly one call; their lambdas remain row-owned policy, not a new helper.

Ripples: update the three case names in `transform.md` prose. No cross-package code-fence ripple.

## 4. Replace interpolation phantom structs and make shared admission total

Location: interpolation capability types, `Interpolant<TCap>` constraint, every factory return/build type argument, and both extension constraints.

### 4a. Keep the orthogonal capability algebra without witness structs

From:

```csharp
public interface IInterpolantCapability { }
public interface IDifferentiable : IInterpolantCapability { }
public interface IIntegrable : IInterpolantCapability { }

public readonly struct Smooth : IDifferentiable, IIntegrable { }
public readonly struct Differentiable : IDifferentiable { }
public readonly struct Sampled : IInterpolantCapability { }
```

To:

```csharp
public interface IEvaluable { }
public interface IDifferentiable : IEvaluable { }
public interface IIntegrable : IEvaluable { }
public interface ICalculus : IDifferentiable, IIntegrable { }
```

### 4b. Substitute the four interface tiers mechanically

From:

```csharp
where TCap : IInterpolantCapability
Interpolant<Smooth>
Build<Smooth>
Interpolant<Differentiable>
Build<Differentiable>
Interpolant<Sampled>
Build<Sampled>
```

To:

```csharp
where TCapability : IEvaluable
Interpolant<ICalculus>
Build<ICalculus>
Interpolant<IDifferentiable>
Build<IDifferentiable>
Interpolant<IEvaluable>
Build<IEvaluable>
```

Rename `TCap` to `TCapability` on `Interpolant<TCapability>`, `Build<TCapability>`, and both extension blocks at the same time; it is a generic parameter rename, not another type.

### 4c. Make the shared order claim total over every admitted and refused extent

From:

```csharp
private static bool Ascending(Arr<double> points) =>
    Enumerable.Range(start: 1, count: points.Count - 1).All(index => points[index - 1] < points[index]);
```

To:

```csharp
private static bool Ascending(Arr<double> points) =>
    points.Count < 2 || Enumerable.Range(start: 1, count: points.Count - 1).All(index => points[index - 1] < points[index]);
```

### 4d. Accumulate Hermite's derivative-column claims in the shared admission

From, after the capability substitution:

```csharp
public static Fin<Interpolant<ICalculus>> Hermite(Arr<double> points, Arr<double> values, Arr<double> slopes, Op? key = null) {
    Op op = key.OrDefault();
    return slopes.Count == points.Count && TensorPrimitives.IsFiniteAll<double>(slopes.AsSpan())
        ? Build<ICalculus>(points, values, op, (p, v) => Interpolate.CubicSplineWithDerivatives(p, v, slopes.AsIterable()))
        : Fin.Fail<Interpolant<ICalculus>>(op.InvalidInput());
}
```

To:

```csharp
public static Fin<Interpolant<ICalculus>> Hermite(Arr<double> points, Arr<double> values, Arr<double> slopes, Op? key = null) =>
    Build<ICalculus>(points, values, key.OrDefault(),
        (p, v) => Interpolate.CubicSplineWithDerivatives(p, v, slopes.AsIterable()), slopes: Some(slopes));
```

From:

```csharp
private static Fin<Interpolant<TCapability>> Build<TCapability>(Arr<double> points, Arr<double> values, Op key, Func<IEnumerable<double>, IEnumerable<double>, IInterpolation> factory)
```

To:

```csharp
private static Fin<Interpolant<TCapability>> Build<TCapability>(Arr<double> points, Arr<double> values, Op key, Func<IEnumerable<double>, IEnumerable<double>, IInterpolation> factory, Option<Arr<double>> slopes = default)
```

After the existing `values-finite` claim, add:

```csharp
(slopes.Map(s => s.Count == points.Count).IfNone(true), "slopes-extent"),
(slopes.Map(s => TensorPrimitives.IsFiniteAll<double>(s.AsSpan())).IfNone(true), "slopes-finite"),
```

Effect: target fenced LOC approximately `-4`; module-level/public type symbols `6 -> 4` (`-2`); no member symbol changes. Compile-time availability remains exact: `ICalculus` reaches differentiation and integration, `IDifferentiable` reaches only the two derivative orders, and `IEvaluable` reaches neither. Empty input now reaches the existing `points-extent`/`knots-extent` typed claim instead of `Enumerable.Range(..., -1)` throwing before `Admit.Claims` can settle it. Hermite's independent points, values, and slopes defects now accumulate in one `Admit.Claims` call instead of its slope guard erasing the base-column evidence.

API/consumer proof: the MathNet catalog states that differentiation and integration are orthogonal flags. The four-interface diamond preserves that fact; making `IIntegrable : IDifferentiable` would falsely turn an observed package roster into a semantic law. A type argument may itself be an interface satisfying the generic constraint, so the three zero-state structs carry no information. `Enumerable.Range(start, count)` rejects a negative count, and every tuple handed to `Admit.Claims` is evaluated before the carrier can accumulate it; the explicit small-extent identity is therefore required for typed admission. Hermite is the sole scheme with a third coefficient column, so an `Option<Arr<double>>` on the private shared admission expresses genuine absence without adding a public overload or sentinel; every other factory keeps its existing call. One concrete consumer spells an old tier: `Rasm.Element/Composition/material.md` aliases `Interpolant<Smooth>` for its stored linear curve. That alias must become `Interpolant<ICalculus>` with the factory result; `Rasm.Compute/Tensor/sampling.md` and `Rasm/ARCHITECTURE.md` carry the remaining explanatory `TCap`/tier vocabulary.

Ripples: update interpolation owner/case/entry/growth prose in the target; update both interpolation paragraphs in `Rasm.Compute/.planning/Tensor/sampling.md`; and update `Rasm/ARCHITECTURE.md`'s `Interpolant<TCap>` tree label. The concrete `Rasm.Element/Composition/material.md` alias is affected too; move 5d groups its alias, field, constructor parameter, factory, and read into one non-duplicated replacement.

## 5. Delete the duplicate rational factory and rename the remaining interpolation surface by algorithm

Location: public factories on static `Interpolant`, `Interpolant<TCapability>.Value`, and every direct consumer reference.

### 5a. Delete `Common`

From:

```csharp
public static Fin<Interpolant<IEvaluable>> Common(Arr<double> points, Arr<double> values, Op? key = null) =>
    Build<IEvaluable>(points, values, key.OrDefault(), static (p, v) => Interpolate.Common(p, v));
```

To:

```csharp
// deleted
```

### 5b. Use the actual scheme names

From:

```csharp
CubicSpline
CubicSplineRobust
CubicSplineMonotone
Hermite
Linear
Step
```

To:

```csharp
NaturalCubicSpline
AkimaSpline
PchipSpline
CubicHermiteSpline
LinearSpline
StepInterpolation
```

From:

```csharp
Polynomial
LogLinear
RationalWithoutPoles
RationalWithPoles
PolynomialEquidistant
OfSegments
OfTransformed
```

To:

```csharp
NevillePolynomial
LogLinearSpline
FloaterHormannRational
BulirschStoerRational
EquidistantBarycentricPolynomial
QuadraticSpline
TransformedInterpolation
```

Only the wrapper member names change; their bodies continue to call the exact checked-in MathNet factory names shown in the From state. Qualify `MathNet.Numerics.Interpolation.TransformedInterpolation` in the renamed member body so the wrapper method does not capture the package type's simple name.

### 5c. Rename the universal read

From:

```csharp
public Fin<double> Value(double t, Op? key = null) => Interpolant.Finite(value: Curve.Interpolate(t: t), key: key.OrDefault());
```

To:

```csharp
public Fin<double> Evaluate(double t, Op? key = null) => Interpolant.AdmitFinite(value: Curve.Interpolate(t: t), key: key.OrDefault());
```

From:

```csharp
internal static Fin<double> Finite(double value, Op key) =>
```

To:

```csharp
internal static Fin<double> AdmitFinite(double value, Op key) =>
```

Replace the two differentiation extension-block calls to `Finite(...)` with `AdmitFinite(...)`; the integration call is replaced explicitly below.

From:

```csharp
public Fin<double> Slope(double t, Op? key = null) => Finite(value: self.Curve.Differentiate(t: t), key: key.OrDefault());
public Fin<double> Curvature(double t, Op? key = null) => Finite(value: self.Curve.Differentiate2(t: t), key: key.OrDefault());
```

To:

```csharp
public Fin<double> Derivative(double t, Op? key = null) => AdmitFinite(value: self.Curve.Differentiate(t: t), key: key.OrDefault());
public Fin<double> SecondDerivative(double t, Op? key = null) => AdmitFinite(value: self.Curve.Differentiate2(t: t), key: key.OrDefault());
```

From:

```csharp
public Fin<double> Area(double to, Option<double> from = default, Op? key = null) =>
    Finite(value: from.Match(Some: a => self.Curve.Integrate(a: a, b: to), None: () => self.Curve.Integrate(t: to)), key: key.OrDefault());
```

To:

```csharp
public Fin<double> Integrate(double upper, Option<double> lower = default, Op? key = null) =>
    AdmitFinite(value: lower.Match(Some: a => self.Curve.Integrate(a: a, b: upper), None: () => self.Curve.Integrate(t: upper)), key: key.OrDefault());
```

### 5d. Update every concrete consumer spelling

From:

```csharp
Interpolant.CubicSplineMonotone
using InterpolantSmooth = Rasm.Numerics.Interpolant<Rasm.Numerics.Smooth>;
[IgnoreEquality] private readonly InterpolantSmooth fit;
private SampledCurve(ImmutableArray<double> axis, ImmutableArray<double> values, InterpolantSmooth fit) =>
Interpolant.Linear(toArr(axis.ToArray()), toArr(values.ToArray()), key)
return fit.Value(Math.Clamp(x, a[0], a[^1]), key);
```

To:

```csharp
Interpolant.PchipSpline
using CalculusInterpolant = Rasm.Numerics.Interpolant<Rasm.Numerics.ICalculus>;
[IgnoreEquality] private readonly CalculusInterpolant fit;
private SampledCurve(ImmutableArray<double> axis, ImmutableArray<double> values, CalculusInterpolant fit) =>
Interpolant.LinearSpline(toArr(axis.ToArray()), toArr(values.ToArray()), key)
return fit.Evaluate(Math.Clamp(x, a[0], a[^1]), key);
```

The first occurrence is package prose in `Rasm/.planning/Parametric/curve.md`; the remaining five are the alias, two alias uses, factory call, and read in `Rasm.Element/.planning/Composition/material.md`.

### 5e. Use the sorted package constructors after sorted admission

First make the private factory carry the arrays every `*Sorted` entry requires rather than an enumerable shape that invites every package factory to copy and sort again.

From, after move 4d:

```csharp
private static Fin<Interpolant<TCapability>> Build<TCapability>(Arr<double> points, Arr<double> values, Op key, Func<IEnumerable<double>, IEnumerable<double>, IInterpolation> factory, Option<Arr<double>> slopes = default)
```

To:

```csharp
private static Fin<Interpolant<TCapability>> Build<TCapability>(Arr<double> points, Arr<double> values, Op key, Func<double[], double[], IInterpolation> factory, Option<Arr<double>> slopes = default)
```

From:

```csharp
.Bind(_ => key.Catch(() => Fin.Succ(new Interpolant<TCapability>(factory(arg1: points.AsIterable(), arg2: values.AsIterable())))));
```

To:

```csharp
.Bind(_ => key.Catch(() => Fin.Succ(new Interpolant<TCapability>(factory(arg1: [.. points.AsIterable()], arg2: [.. values.AsIterable()])))));
```

Replace the four cubic factory calls in their already-renamed members:

```csharp
Interpolate.CubicSpline(p, v)                         -> CubicSpline.InterpolateNaturalSorted(p, v)
Interpolate.CubicSplineRobust(p, v)                   -> CubicSpline.InterpolateAkimaSorted(p, v)
Interpolate.CubicSplineMonotone(p, v)                 -> CubicSpline.InterpolatePchipSorted(p, v)
Interpolate.CubicSplineWithDerivatives(p, v, slopes)  -> CubicSpline.InterpolateHermiteSorted(p, v, [.. slopes.AsIterable()])
```

Replace the five direct sorted factory calls:

```csharp
Interpolate.Linear(p, v)                    -> MathNet.Numerics.Interpolation.LinearSpline.InterpolateSorted(p, v)
Interpolate.Step(p, v)                      -> MathNet.Numerics.Interpolation.StepInterpolation.InterpolateSorted(p, v)
Interpolate.Polynomial(p, v)                -> NevillePolynomialInterpolation.InterpolateSorted(p, v)
Interpolate.LogLinear(p, v)                 -> MathNet.Numerics.Interpolation.LogLinear.InterpolateSorted(p, v)
Interpolate.RationalWithPoles(p, v)         -> BulirschStoerRationalInterpolation.InterpolateSorted(p, v)
```

Replace the two barycentric factory calls:

```csharp
Interpolate.RationalWithoutPoles(p, v)      -> Barycentric.InterpolateRationalFloaterHormannSorted(p, v)
Interpolate.PolynomialEquidistant(p, v)     -> Barycentric.InterpolatePolynomialEquidistantSorted(p, v)
```

From, in the renamed transformed member:

```csharp
MathNet.Numerics.Interpolation.TransformedInterpolation.Interpolate(transform: transform, transformInverse: inverse, x: p, y: v)
```

To:

```csharp
MathNet.Numerics.Interpolation.TransformedInterpolation.InterpolateSorted(transform: transform, transformInverse: inverse, x: p, y: v)
```

The thirteenth catalogue row is the constructor the retained segment mint already calls, not another public wrapper:

```csharp
new QuadraticSpline(double[] x, double[] c0, double[] c1, double[] c2)
```

Effect: target fenced LOC remains `-2`; public method symbols remain `-1`; all remaining symbols are renames with no count change. One semantic duplicate and the vague `Common` and `OfTransformed` names disappear; `Slope`/`Curvature` become the exact derivative orders they return; `Area(to, from)` becomes the mathematically exact `Integrate(upper, lower)` rather than naming a signed integral as unsigned area and reversing the conventional bound vocabulary. The shared admission copies each admitted point/value column once into the exact array carrier all twelve sorted constructors consume; no package entry is then asked to sort a sequence `Build` has already proved strictly ascending.

API/consumer proof: the MathNet catalog says `Interpolate.Common` and `Interpolate.RationalWithoutPoles` are the same Floater-Hormann pole-free rational scheme. It also identifies robust cubic as Akima, monotone cubic as PCHIP, polynomial as Neville, rational-with-poles as Bulirsch-Stoer, and the equidistant polynomial as barycentric. Those are established algorithm names, not local labels; `TransformedInterpolation` is likewise the package/domain term, where “fit” would incorrectly imply regression. The pinned 6.0.0-beta2 XML confirms the exact twelve array-taking sorted entries above: four on `CubicSpline`, two on `Barycentric`, and one each on `LinearSpline`, `StepInterpolation`, `NevillePolynomialInterpolation`, `LogLinear`, `BulirschStoerRationalInterpolation`, and `TransformedInterpolation`. It also confirms `QuadraticSpline(double[] x, double[] c0, double[] c1, double[] c2)`. The checked-in catalogue currently records only the enumerable convenience roster and omits those thirteen package surfaces. `Differentiate2` returns the second derivative, not geometric curvature, and MathNet calls the signed operation `Integrate(a, b)`/`Integrate(t)`; “area” would imply an absolute geometric measure that this method does not compute. The only direct code consumer is the `Rasm.Element` linear curve, and its stored alias and `Value` read must move with the factory; all other external occurrences are prose.

Ripples: update the target interpolation owner/case/entry/law/growth prose, `Rasm.Compute/Tensor/sampling.md` including its `Area` witness, `Rasm/Parametric/curve.md`, and all five code spellings listed above in `Rasm.Element/Composition/material.md`. Surgically add the thirteen confirmed constructor/factory rows above to `libs/dotnet/.api/api-mathnet-numerics.md`; do not rename package members in the catalogue.

## 6. Close spectral carrier/result construction and remove layout-dependent energy

Location: `SpectralArena.IsValid`, `Spectrum`, `SpectrumOf`, and `SpectralPower`.

### 6a. Refuse default interleaved and packed-real extents at the carrier gate

From:

```csharp
interleaved: static a => a.Values.Length == a.Lattice.CellCount && Admit.FiniteComplexSpan(a.Values.AsSpan()),
halfSpectrum: static h => h.Values.Length >= PackedLength(samples: h.Samples.Value) && TensorPrimitives.IsFiniteAll<double>(h.Values),
```

To:

```csharp
interleaved: static a => a.Lattice.CellCount >= 1L && a.Values.Length == a.Lattice.CellCount && Admit.FiniteComplexSpan(a.Values.AsSpan()),
halfSpectrum: static h => h.Samples.Value >= 1 && h.Values.Length >= PackedLength(samples: h.Samples.Value) && TensorPrimitives.IsFiniteAll<double>(h.Values),
```

### 6b. Remove the derived public column and the invalid public positional constructor

From:

```csharp
public readonly record struct Spectrum(SpectralArena Arena, SpectralSense Sense, SpectralScaling Scaling, double Energy) : IValidityEvidence {
```

To:

```csharp
public sealed class Spectrum : IValidityEvidence {
    internal Spectrum(SpectralArena arena, SpectralSense sense, SpectralScaling scaling) =>
        (Arena, Sense, Scaling) = (arena, sense, scaling);
    public SpectralArena Arena { get; }
    public SpectralSense Sense { get; }
    public SpectralScaling Scaling { get; }
```

From:

```csharp
public bool IsValid => ValidityClaim.All(
    Arena is not null && Sense is not null && Scaling is not null && Arena.IsValid,
    ValidityClaim.Finite(Energy),
    ValidityClaim.Nonnegative(value: Energy),
    Cells >= 1L && Rank >= 1);
```

To:

```csharp
public bool IsValid => Arena is not null && Sense is not null && Scaling is not null && Arena.IsValid;
```

### 6c. Mint from the already-validated arena

From:

```csharp
private static Fin<Spectrum> SpectrumOf(SpectralArena arena, SpectralSense sense, SpectralScaling scaling, Op key) =>
    SpectralPower(arena: arena, key: key).Bind(power => {
        Spectrum spectrum = new(Arena: arena, Sense: sense, Scaling: scaling, Energy: TensorPrimitives.Sum<double>(power.AsSpan()));
        return spectrum.IsValid ? Fin.Succ(spectrum) : Fin.Fail<Spectrum>(key.InvalidResult());
    });
```

To:

```csharp
private static Fin<Spectrum> SpectrumOf(SpectralArena arena, SpectralSense sense, SpectralScaling scaling, Op key) {
    Spectrum spectrum = new(arena: arena, sense: sense, scaling: scaling);
    return spectrum.IsValid ? Fin.Succ(spectrum) : Fin.Fail<Spectrum>(key.InvalidResult());
}
```

### 6d. Validate power where power is requested

From:

```csharp
: key.Catch(() => Fin.Succ(arena.Switch(
    interleaved: static a => PairPower(pairs: MemoryMarshal.Cast<Complex, double>(a.Values), bins: a.Values.Length),
    split: static s => SplitPower(real: s.Real, imaginary: s.Imaginary),
    halfSpectrum: static h => PairPower(pairs: h.Values, bins: SpectralArena.PackedLength(samples: h.Samples.Value) / 2),
    realValued: static r => HartleyPower(samples: r.Samples))));
```

To:

```csharp
: key.Catch(() => Fin.Succ(arena.Switch(
    interleaved: static a => PairPower(pairs: MemoryMarshal.Cast<Complex, double>(a.Values), bins: a.Values.Length),
    split: static s => SplitPower(real: s.Real, imaginary: s.Imaginary),
    halfSpectrum: static h => PairPower(pairs: h.Values, bins: SpectralArena.PackedLength(samples: h.Samples.Value) / 2),
    realValued: static r => HartleyPower(samples: r.Samples))))
  .Bind(power => TensorPrimitives.IsFiniteAll<double>(power.AsSpan())
      ? Fin.Succ(power)
      : Fin.Fail<Arr<double>>(key.InvalidResult()));
```

Effect: target fenced LOC exactly `+3` for the displayed replacements; module-level type symbols are unchanged and public data members are exactly `-1` (`Energy`). The positional public constructor, generated deconstruction/equality/operators, and defaultable value-result shape disappear; one internal constructor is authored in their place. Every transform and modulation stops allocating and filling a full power array merely to sum it. `Power` gains the missing finite-result gate, so squared overflow is refused by the operation that produced it rather than incidentally by construction of an unrelated public scalar.

API/consumer proof: `Spectrum.Energy` is not layout-invariant. `HalfSpectrum` sums only the stored nonnegative bins without doubling the conjugate interior, while split/interleaved layouts sum all bins, so equal real signals publish different values by arena case. It therefore cannot be valid carrier evidence; callers retain the `Power` operation that computes the requested representation explicitly. No `libs/dotnet/` consumer reads kernel `Spectrum.Energy`, constructs this `Rasm.Numerics.Spectrum`, deconstructs it, or compares it by value, so correcting the carrier has no call-site loss. `CellLattice` and `Dimension` are defaultable record/value structs: their defaults have zero cells/samples, so the former interleaved equality and packed `>= PackedLength(0)` test let empty transforms pass `SpectralArena.IsValid`. The positive censuses belong on that carrier gate. With them present, `Cells >= 1` and `Rank >= 1` are redundant result-level restatements of the four valid carrier cases. The mutable arena makes generated record equality actively misleading, while the public positional record-struct constructor and `default(Spectrum)` allow state that violates `IsValid` and can throw from `Rank`, `Cells`, or `RoundTripFactor` before an operation gate runs. A sealed reference result with an internal constructor matches the sole mint and removes both surfaces. `SpectralArena.IsValid` already proves the transformed buffer finite. Checked-in tensor API establishes `IsFiniteAll` as the whole-span finite gate and `Sum` as a reduction, not evidence that must be stored. The similarly named `Rasm.Compute.Stats.Spectrum` is a different namespace/type and is not a ripple.

Ripples: remove energy from the target owner/result prose, state both positive carrier censuses, describe `Spectrum` as an internally minted mutable-arena result, and update the power-admission statement. No cross-package fence changes.

## 7. Compute paired complex power in one pass without scratch storage

Location: `PairPower`.

From:

```csharp
private static Arr<double> PairPower(ReadOnlySpan<double> pairs, int bins) {
    using MemoryOwner<double> squares = MemoryOwner<double>.Allocate(size: bins * 2);
    TensorPrimitives.Multiply<double>(pairs[..(bins * 2)], pairs[..(bins * 2)], squares.Span);
    double[] power = new double[bins];
    for (int bin = 0; bin < bins; bin++) { power[bin] = squares.Span[2 * bin] + squares.Span[(2 * bin) + 1]; }
    return new Arr<double>(power);
}
```

To:

```csharp
private static Arr<double> PairPower(ReadOnlySpan<double> pairs, int bins) {
    double[] power = new double[bins];
    for (int bin = 0; bin < bins; bin++) { double real = pairs[2 * bin], imaginary = pairs[(2 * bin) + 1]; power[bin] = (real * real) + (imaginary * imaginary); }
    return new Arr<double>(power);
}
```

Effect: target fenced LOC `7 -> 5` (`-2`); symbols unchanged; one pooled allocation, one full squaring pass, and one second read of the scratch plane disappear.

API/consumer proof: both callers present the identical packed `(real, imaginary)` layout and request squared magnitude, not magnitude. Tensor primitives have no strided pair-reduction operation; `Multiply` can square the flat plane but cannot add adjacent pairs into a half-length destination. The direct arithmetic is therefore the one-pass algorithm rather than a hand-rolled substitute for an available primitive. The output array remains necessary because `Power` publishes `Arr<double>`.

Ripples: update the target Auto/Packages prose to remove “squaring buffer” and describe the single pair pass. No consumer changes.

## 8. Collapse the six remaining bounded single-use helpers

Location: `TapBorder.Reflected`, `MatrixKernel.Transformed`, `SplitPower`, `HartleyPower`, `AxisOf`, and `Modulated`. `FoldSeparable` deliberately remains named: it is the page's declared statement-kernel exemption and owns the row/column gather-transform-scatter loop.

### 8a. Inline reflection into its sole row and name omitted samples truthfully

From:

```csharp
public static readonly TapBorder Mirror = new(resolve: static (index, extent) => Some(Reflected(index: index, extent: extent)));
```

To:

```csharp
public static readonly TapBorder Mirror = new(resolve: static (index, extent) => {
    int period = Math.Max(val1: 1, val2: (extent - 1) * 2);
    int folded = ((index % period) + period) % period;
    return Some(folded < extent ? folded : period - folded);
});
```

From:

```csharp
private static int Reflected(int index, int extent) {
    int period = Math.Max(val1: 1, val2: (extent - 1) * 2);
    int folded = ((index % period) + period) % period;
    return folded < extent ? folded : period - folded;
}
```

To:

```csharp
// deleted
```

From:

```csharp
public static readonly TapBorder Zero = new(resolve: static (_, _) => Option<int>.None);
```

To:

```csharp
public static readonly TapBorder Omit = new(resolve: static (_, _) => Option<int>.None);
```

Mechanically replace the three `TapBorder.Zero` consumer spellings in `Rasm.Materials/.planning/Raster/filter.md` with `TapBorder.Omit`.

### 8b. Inline the transform dispatch into its sole entry

From:

```csharp
internal static Fin<Spectrum> SpectralTransform(SpectralArena arena, SpectralSense sense, SpectralScaling scaling, Op key) =>
    arena is null || sense is null || scaling is null || !arena.IsValid
        ? Fin.Fail<Spectrum>(key.InvalidInput())
        : key.Catch(() => SpectrumOf(arena: Transformed(arena: arena, sense: sense, scaling: scaling), sense: sense, scaling: scaling, key: key));
private static SpectralArena Transformed(SpectralArena arena, SpectralSense sense, SpectralScaling scaling) =>
    arena.Switch(
        state: (Sense: sense, Scaling: scaling),
```

To:

```csharp
internal static Fin<Spectrum> SpectralTransform(SpectralArena arena, SpectralSense sense, SpectralScaling scaling, Op key) =>
    arena is null || sense is null || scaling is null || !arena.IsValid
        ? Fin.Fail<Spectrum>(key.InvalidInput())
        : key.Catch(() => SpectrumOf(arena: arena.Switch(
            state: (Sense: sense, Scaling: scaling),
```

At the end of the existing four `Switch` arms, replace:

```csharp
        realValued: static (s, a) => new SpectralArena.RealValued(
            Samples: new Arr<double>(s.Sense.RealValued(samples: [.. a.Samples.AsIterable()], options: s.Scaling.HartleyConvention)), Rate: a.Rate));
```

with:

```csharp
        realValued: static (s, a) => new SpectralArena.RealValued(
            Samples: new Arr<double>(s.Sense.RealValued(samples: [.. a.Samples.AsIterable()], options: s.Scaling.HartleyConvention)), Rate: a.Rate)),
    sense: sense, scaling: scaling, key: key));
```

### 8c. Inline the split and Hartley power arms

From:

```csharp
split: static s => SplitPower(real: s.Real, imaginary: s.Imaginary),
```

To:

```csharp
split: static s => {
    double[] power = new double[s.Real.Length];
    TensorPrimitives.Multiply<double>(s.Real, s.Real, power);
    TensorPrimitives.MultiplyAdd<double>(s.Imaginary, s.Imaginary, power, power);
    return new Arr<double>(power);
},
```

From:

```csharp
private static Arr<double> SplitPower(ReadOnlySpan<double> real, ReadOnlySpan<double> imaginary) {
    double[] power = new double[real.Length];
    TensorPrimitives.Multiply<double>(real, real, power);
    TensorPrimitives.MultiplyAdd<double>(imaginary, imaginary, power, power);
    return new Arr<double>(power);
}
```

To:

```csharp
// deleted
```

From:

```csharp
realValued: static r => HartleyPower(samples: r.Samples)
```

To:

```csharp
realValued: static r => {
    double[] power = new double[r.Samples.Count];
    power[0] = r.Samples[0] * r.Samples[0];
    for (int bin = 1; bin < power.Length; bin++) { double direct = r.Samples[bin], reflected = r.Samples[power.Length - bin]; power[bin] = 0.5 * ((direct * direct) + (reflected * reflected)); }
    return new Arr<double>(power);
}
```

From:

```csharp
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
```

To:

```csharp
// deleted
```

### 8d. Inline the one-use metric fold and make the frequency result match its arena

From:

```csharp
internal Option<(int Count, double SampleRate)> Metric(int ordinal) => Switch(
    state: ordinal,
    interleaved: static (o, a) => o >= 0 && o < a.Lattice.Rank
        ? Some((Count: a.Lattice.Extent(ordinal: o).Value, SampleRate: 1.0 / a.Lattice.Spacing(ordinal: o)))
        : Option<(int, double)>.None,
    split: static (o, a) => o is 0 ? Some((Count: a.Real.Length, SampleRate: a.Rate.Value)) : Option<(int, double)>.None,
    halfSpectrum: static (o, a) => o is 0 ? Some((Count: a.Samples.Value, SampleRate: a.Rate.Value)) : Option<(int, double)>.None,
    realValued: static (o, a) => o is 0 ? Some((Count: a.Samples.Count, SampleRate: a.Rate.Value)) : Option<(int, double)>.None);
```

To:

```csharp
// deleted; its generated union fold moves into the sole frequency operation below
```

From:

```csharp
public Fin<Arr<double>> Axis(SignedAxis axis, Op? key = null) => MatrixKernel.SpectralAxis(arena: Arena, axis: axis, key: key.OrDefault());
```

To:

```csharp
public Fin<Arr<double>> Frequencies(SignedAxis axis, Op? key = null) =>
    MatrixKernel.SpectralFrequencies(arena: Arena, axis: axis, key: key.OrDefault());
```

From:

```csharp
internal static Fin<Arr<double>> SpectralAxis(SpectralArena arena, SignedAxis axis, Op key) =>
    arena is null || axis is null || !arena.IsValid
        ? Fin.Fail<Arr<double>>(key.InvalidInput())
        : arena.Metric(ordinal: Math.Abs(value: axis.Key) - 1)
            .ToFin(key.InvalidInput(axis: "spectral-ordinal"))
            .Bind(metric => AxisOf(metric: metric, key: key));
```

To:

```csharp
internal static Fin<Arr<double>> SpectralFrequencies(SpectralArena arena, SignedAxis axis, Op key) {
    if (arena is null || axis is null || !arena.IsValid) { return Fin.Fail<Arr<double>>(key.InvalidInput()); }
    Option<(int Samples, int Bins, double Rate)> metric = arena.Switch(
        state: Math.Abs(value: axis.Key) - 1,
        interleaved: static (o, a) => o >= 0 && o < a.Lattice.Rank ? Some((a.Lattice.Extent(o).Value, a.Lattice.Extent(o).Value, 1.0 / a.Lattice.Spacing(o))) : Option<(int, int, double)>.None,
        split: static (o, a) => o is 0 ? Some((a.Real.Length, a.Real.Length, a.Rate.Value)) : Option<(int, int, double)>.None,
        halfSpectrum: static (o, a) => o is 0 ? Some((a.Samples.Value, SpectralArena.PackedLength(a.Samples.Value) / 2, a.Rate.Value)) : Option<(int, int, double)>.None,
        realValued: static (o, a) => o is 0 ? Some((a.Samples.Count, a.Samples.Count, a.Rate.Value)) : Option<(int, int, double)>.None);
```

Replace the former `AxisOf` body with the continuation of that method:

```csharp
    return metric.ToFin(key.InvalidInput(axis: "spectral-ordinal")).Bind(row =>
        row.Samples < 1 || row.Bins < 1 || row.Bins > row.Samples || !double.IsFinite(row.Rate) || row.Rate <= 0.0
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : key.Catch(() => {
                double[] scale = Fourier.FrequencyScale(length: row.Samples, sampleRate: row.Rate);
                if (axis.Key < 0) { TensorPrimitives.Negate<double>(scale, scale); }
                Arr<double> bins = new(row.Bins == scale.Length ? scale : scale[..row.Bins]);
                return TensorPrimitives.IsFiniteAll<double>(bins.AsSpan()) ? Fin.Succ(bins) : Fin.Fail<Arr<double>>(key.InvalidResult());
            }));
}
```

Update the two code consumers and the stale result type in `Rasm.Compute/.planning/Tensor/quadrature.md` and `Rasm.Fabrication/.planning/Additive/implicit.md`:

```csharp
public Fin<double[]> Wavenumbers(Rasm.Numerics.Spectrum spectrum, Op key) =>
    spectrum.Frequencies(axis: SignedAxis.PositiveX, key: key).Map(static cycles => {
static Fin<SpectralEvidence> Settled(SpectralPlane plane, Spectral op, WaveAxis axis, Rasm.Numerics.Spectrum inverse, SpectralControl control)

from axes in Seq(SignedAxis.PositiveX, SignedAxis.PositiveY, SignedAxis.PositiveZ)
    .TraverseM(axis => forward.Frequencies(axis, key)).As()
```

### 8e. Inline modulation after its guard

From:

```csharp
internal static Fin<Spectrum> SpectralModulate(Spectrum spectrum, ReadOnlySpan<Complex> symbol, Op key) =>
    spectrum.Arena is SpectralArena.Interleaved plane && plane.Values.Length == symbol.Length && Admit.FiniteComplexSpan(symbol)
        ? Modulated(plane: plane, symbol: symbol, spectrum: spectrum, key: key)
        : Fin.Fail<Spectrum>(key.InvalidInput());
private static Fin<Spectrum> Modulated(SpectralArena.Interleaved plane, ReadOnlySpan<Complex> symbol, Spectrum spectrum, Op key) {
    TensorPrimitives.Multiply<Complex>(plane.Values, symbol, plane.Values);
    return SpectrumOf(arena: plane, sense: spectrum.Sense, scaling: spectrum.Scaling, key: key);
}
```

To:

```csharp
internal static Fin<Spectrum> SpectralModulate(Spectrum spectrum, ReadOnlySpan<Complex> symbol, Op key) {
    if (!spectrum.IsValid || spectrum.Arena is not SpectralArena.Interleaved plane || plane.Values.Length != symbol.Length || !Admit.FiniteComplexSpan(symbol)) {
        return Fin.Fail<Spectrum>(key.InvalidInput());
    }
    TensorPrimitives.Multiply<Complex>(plane.Values, symbol, plane.Values);
    return SpectrumOf(arena: plane, sense: spectrum.Sense, scaling: spectrum.Scaling, key: key);
}
```

Effect: target fenced LOC approximately `-16`; private member symbols `-6` (`Reflected`, `Transformed`, `SplitPower`, `HartleyPower`, `AxisOf`, `Modulated`) and internal member symbols `-1` (`SpectralArena.Metric`); module/public symbol counts remain unchanged. Hartley power also loses one pooled reflection buffer and three whole-span passes. `Axis` becomes the standard DSP name `Frequencies`; packed-real output contracts from `N` entries to its actual `floor(N/2)+1` bins; a negative `SignedAxis` now negates the frequency coordinates instead of being semantically indistinguishable from its positive twin; and `Omit` replaces the false zero-padding name.

API/consumer proof: each deleted helper has exactly one caller. The inline `arena.Switch` remains total and compiler-forced by Thinktecture. Split power keeps the checked-in tensor `Multiply`/`MultiplyAdd` pair over contiguous arrays. Hartley power uses the defining identity `(H[k]^2 + H[N-k]^2)/2`; bin zero is handled once and all other bins read their reflected index directly, so no copied/reversed plane is needed. MathNet documents `FrequencyScale(N, rate)` as `N` wrapped bins but `ForwardReal` as only `floor(N/2)+1` packed complex bins; returning all `N` coordinates for that case disagrees with both `Power()` and the consumer's packed multiplication loop. `SignedAxis` owns six positive/negative rows and no `X`/`Y`/`Z` aliases, so the two consumers' current spellings do not compile and ignoring sign wastes half that owner. Move 6 closes `Spectrum` construction, but its arena arrays remain intentionally caller-owned and mutable; the `IsValid` guard therefore still makes a subsequently corrupted spectrum an input refusal before the in-place write. `TapBorder.Zero` never contributes a zero-valued sample: it returns `None`, removes the coefficient from `admitted`, and the fold renormalizes by the remaining coefficients. `Omit` states that exact action; `Truncate` would ambiguously suggest shortening the kernel or output extent, while “zero” would mean retaining the coefficient and contributing a zero-valued sample. The span-bearing modulation body remains a statement body because spans cannot cross a captured lambda.

Ripples: update target Auto/Packages prose to remove the Hartley reflection rent and rename the frequency read and omitted-border row. In `Rasm.Compute/Tensor/quadrature.md`, replace the stale `SpectralResult.Axis`/`SpectralResult.Modulate` prose with kernel `Spectrum.Frequencies`/`Spectrum.Modulate`, qualify both kernel `Spectrum` parameters because the package already owns a different `SpectralResult`, and use `SignedAxis.PositiveX`. In `Rasm.Fabrication/Additive/implicit.md`, use the three positive axis rows and `Frequencies`. In `Rasm.Materials/Raster/filter.md`, replace all three `TapBorder.Zero` spellings and the surrounding “zero” prose with `TapBorder.Omit`/omission. No other consumer calls the renamed members.

## 9. Make tap-window arithmetic and resolved indices proof-carrying

Location: `TapWindow.Of`/`IsValid`, `TapFold`/`TapFoldLattice` admission, and the `TapFoldCore` inner tap loop.

### 9a. Express bounds without overflowing `int`

From:

```csharp
(from + run.Value <= extent.Value, "run-within-extent"))
```

To:

```csharp
(run.Value <= extent.Value - from, "run-within-extent"))
```

From:

```csharp
Extent >= 1, Stride >= 1, Run >= 1, From >= 0, Origin >= 0, Origin <= From, From + Run <= Extent);
```

To:

```csharp
Extent >= 1, Stride >= 1, Run >= 1, From >= 0, Origin >= 0, Origin <= From, Run <= Extent - From);
```

From:

```csharp
(folded.Length == window.Run * stride, "folded-extent"),
(window.Origin <= Math.Max(val1: 0, val2: window.From - series.Radius), "staging-head"),
(window.Origin + staged > Math.Min(val1: window.Extent - 1, val2: window.From + window.Run - 1 + series.Radius), "staging-tail"),
```

To:

```csharp
(folded.Length == (long)window.Run * stride, "folded-extent"),
((long)window.Origin <= Math.Max(0L, (long)window.From - series.Radius), "staging-head"),
((long)window.Origin + staged > Math.Min((long)window.Extent - 1, (long)window.From + window.Run - 1 + series.Radius), "staging-tail"),
```

### 9b. Read the resolved option by proof, not by fallback

From:

```csharp
int logical = record + tap;
Option<int> resolved = logical >= 0 && logical < window.Extent
    ? Some(logical)
    : border.Resolve(index: logical, extent: window.Extent);
if (resolved.IsNone) { continue; }
double weight = taps[tap + radius];
admitted += weight;
int from = (resolved.IfNone(0) - window.Origin) * stride;
TensorPrimitives.MultiplyAdd<double>(source.Slice(from, stride), weight, lane, lane);
```

To:

```csharp
long logical = (long)record + tap;
Option<int> resolved = logical >= 0L && logical < window.Extent
    ? Some((int)logical)
    : border.Resolve(index: logical, extent: window.Extent);
if (resolved is not { IsSome: true, Case: int sample }) { continue; }
double weight = taps[tap + radius];
admitted += weight;
int sourceOffset = (sample - window.Origin) * stride;
TensorPrimitives.MultiplyAdd<double>(source.Slice(sourceOffset, stride), weight, lane, lane);
```

### 9c. Carry out-of-range border coordinates without narrowing

From, after move 8a:

```csharp
public static readonly TapBorder Clamp = new(resolve: static (index, extent) => Some(Math.Clamp(value: index, min: 0, max: extent - 1)));
public static readonly TapBorder Wrap = new(resolve: static (index, extent) => Some(((index % extent) + extent) % extent));
public static readonly TapBorder Mirror = new(resolve: static (index, extent) => {
    int period = Math.Max(val1: 1, val2: (extent - 1) * 2);
    int folded = ((index % period) + period) % period;
    return Some(folded < extent ? folded : period - folded);
});
public static readonly TapBorder Omit = new(resolve: static (_, _) => Option<int>.None);
[UseDelegateFromConstructor] internal partial Option<int> Resolve(int index, int extent);
```

To:

```csharp
public static readonly TapBorder Clamp = new(resolve: static (index, extent) => Some((int)Math.Clamp(value: index, min: 0L, max: (long)extent - 1)));
public static readonly TapBorder Wrap = new(resolve: static (index, extent) => Some((int)(((index % extent) + extent) % extent)));
public static readonly TapBorder Mirror = new(resolve: static (index, extent) => {
    long period = Math.Max(val1: 1L, val2: ((long)extent - 1) * 2);
    long folded = ((index % period) + period) % period;
    return Some((int)(folded < extent ? folded : period - folded));
});
public static readonly TapBorder Omit = new(resolve: static (_, _) => Option<int>.None);
[UseDelegateFromConstructor] public partial Option<int> Resolve(long index, int extent);
```

### 9d. Prove the doubled lattice staging extent before its pooled allocation

From:

```csharp
Fin<Unit> admitted = Admit.Claims(key,
    (border is not null, "border"),
    (axes.Count == lattice.Rank, "axis-arity"),
    (values.Length == lattice.CellCount, "value-extent"),
    (axes.ForAll(static series => series.IsValid), "axis-series"));
if (admitted.IsFail) { return admitted; }
int cells = values.Length;
int longest = Math.Max(val1: lattice.Columns.Value, val2: Math.Max(val1: lattice.Rows.Value, val2: lattice.Layers.Value));
```

To:

```csharp
int longest = Math.Max(val1: lattice.Columns.Value, val2: Math.Max(val1: lattice.Rows.Value, val2: lattice.Layers.Value));
Fin<Unit> admitted = Admit.Claims(key,
    (border is not null, "border"),
    (lattice.CellCount >= 1L, "lattice-census"),
    (axes.Count == lattice.Rank, "axis-arity"),
    (values.Length == lattice.CellCount, "value-extent"),
    (longest <= Array.MaxLength / 2, "staging-extent"),
    (axes.ForAll(static series => series.IsValid), "axis-series"));
if (admitted.IsFail) { return admitted; }
int cells = values.Length;
```

The allocation remains `MemoryOwner<double>.Allocate(size: longest * 2)` and now executes only under the difference-form capacity proof.

Effect: target fenced LOC `+2`; symbol counts unchanged; `TapBorder.Resolve` changes `internal -> public` because a live `Rasm.Materials` consumer already composes the owner across the assembly boundary. The default zero-cell lattice is refused before its zero stride reaches the axis walk; the run and staging gates cannot wrap into a success-shaped negative value at large valid dimensions; `record + tap` and the reflected period remain exact beyond `int`; the absent arm no longer shares a sentinel `0` with a real resolved index; the local `from` is renamed to the actual byte-independent coordinate it carries; and the only remaining doubled staging extent is bounded before multiplication.

API/consumer proof: `CellLattice` is a defaultable record struct, and its default has zero dimensions, `CellCount == 0`, and zero strides; `values.Length == lattice.CellCount` alone therefore certified an empty lattice that later divides by `stride`. The sibling calculus admission already uses the exact `lattice-census` claim. `Dimension` admits every positive `int`, so `from + run`, `run * stride`, `record + tap`, `(extent - 1) * 2`, a radius-expanded tail, and `longest * 2` are not intrinsically overflow-safe. Difference-form containment and `long` intermediates preserve the same inequalities without a checked exception or wrapped acceptance; every resolved index narrows only after the border fold proves it lies in `[0, extent)`. `Array.MaxLength / 2` proves both the `int` multiplication and the runtime array-length ceiling while preserving the one pooled staging allocation. The checked-in LanguageExt catalog identifies `{ IsSome: true, Case: T value }` as the proof-carrying `Option<T>` read; `Option<T>.Value` is inaccessible and `IfNone(0)` is a value fallback, not a presence proof. The guard establishes the exact proof needed before `Case` is read.

Ripples: none.

## 10. Canonicalize tap coefficients and close packed-spectrum cardinality

Location: `TapSeries.Of`, `TapFold` admission/result gating, and `SpectralArena.HalfSpectrum` validity.

### 10a. Store one coefficient representation for one normalized filter

From:

```csharp
public static Fin<TapSeries> Of(Arr<double> taps, Op? key = null) =>
    taps.Count >= 1 && int.IsOddInteger(taps.Count) && TensorPrimitives.IsFiniteAll<double>(taps.AsSpan())
        && Math.Abs(value: TensorPrimitives.Sum<double>(taps.AsSpan())) > EpsilonPolicy.ZeroTolerance
        ? Fin.Succ(new TapSeries(taps: taps))
        : Fin.Fail<TapSeries>(error: key.OrDefault().InvalidInput());
```

To:

```csharp
public static Fin<TapSeries> Of(Arr<double> taps, Op? key = null) {
    Op op = key.OrDefault();
    double sum = TensorPrimitives.Sum<double>(taps.AsSpan());
    if (taps.Count < 1 || int.IsEvenInteger(taps.Count) || !TensorPrimitives.IsFiniteAll<double>(taps.AsSpan()) || !double.IsFinite(sum) || Math.Abs(sum) <= EpsilonPolicy.ZeroTolerance) { return Fin.Fail<TapSeries>(op.InvalidInput()); }
    double[] normalized = new double[taps.Count];
    TensorPrimitives.Divide<double>(taps.AsSpan(), sum, normalized);
    return TensorPrimitives.IsFiniteAll<double>(normalized) ? Fin.Succ(new TapSeries(new Arr<double>(normalized))) : Fin.Fail<TapSeries>(op.InvalidResult());
}
```

### 10b. Refuse non-finite input, cancellation, and output at the operation that produces it

From:

```csharp
(series.IsValid, "series"),
(window.IsValid, "window"),
```

To:

```csharp
(series.IsValid, "series"),
(TensorPrimitives.IsFiniteAll<double>(source), "source-finite"),
(window.IsValid, "window"),
```

From, in `TapFoldLattice`:

```csharp
Fin<Unit> admitted = Admit.Claims(key,
    (border is not null, "border"),
    (lattice.CellCount >= 1L, "lattice-census"),
    (axes.Count == lattice.Rank, "axis-arity"),
    (values.Length == lattice.CellCount, "value-extent"),
    (longest <= Array.MaxLength / 2, "staging-extent"),
    (axes.ForAll(static series => series.IsValid), "axis-series"));
```

To:

```csharp
Fin<Unit> admitted = Admit.Claims(key,
    (border is not null, "border"),
    (lattice.CellCount >= 1L, "lattice-census"),
    (axes.Count == lattice.Rank, "axis-arity"),
    (values.Length == lattice.CellCount, "value-extent"),
    (TensorPrimitives.IsFiniteAll<double>(values), "values-finite"),
    (longest <= Array.MaxLength / 2, "staging-extent"),
    (axes.ForAll(static series => series.IsValid), "axis-series"));
```

From:

```csharp
if (Math.Abs(value: admitted) <= EpsilonPolicy.ZeroTolerance) {
    return Fin.Fail<Unit>(key.InvalidResult(detail: $"resolved tap-weight sum cancelled at record {record}"));
}
TensorPrimitives.Multiply<double>(lane, 1.0 / admitted, lane);
```

To:

```csharp
if (!double.IsFinite(admitted) || Math.Abs(value: admitted) <= EpsilonPolicy.ZeroTolerance) {
    return Fin.Fail<Unit>(key.InvalidResult(detail: $"resolved tap-weight sum invalid at record {record}"));
}
TensorPrimitives.Multiply<double>(lane, 1.0 / admitted, lane);
if (!TensorPrimitives.IsFiniteAll<double>(lane)) { return Fin.Fail<Unit>(key.InvalidResult()); }
```

### 10c. Admit exactly the packed buffer MathNet owns

From:

```csharp
halfSpectrum: static h => h.Samples.Value >= 1 && h.Values.Length >= PackedLength(samples: h.Samples.Value) && TensorPrimitives.IsFiniteAll<double>(h.Values),
```

To:

```csharp
halfSpectrum: static h => h.Samples.Value >= 1 && h.Values.Length == PackedLength(samples: h.Samples.Value) && TensorPrimitives.IsFiniteAll<double>(h.Values),
```

Effect: target fenced LOC exactly `+6` for the displayed replacements; symbols unchanged. `TapSeries` gains a canonical representation: multiplying every authored coefficient by any scalar for which both forms remain admissible, including a negative scalar, now mints the same stored coefficients and the same equality identity instead of only happening to produce the same folded samples. Both the direct and lattice entries refuse non-finite source material before mutation; the core refuses non-finite output and resolved-sum overflow at the operation that derives them. A packed-real arena can no longer carry an ignored finite tail that neither transform nor power reads.

API/consumer proof: the fold divides every record by the resolved coefficient sum, so coefficient scale and sign are already semantically erased; retaining them in `TapSeries.Taps` made structurally unequal values denote the same filter. `TensorPrimitives.Divide` and `IsFiniteAll` are checked-in whole-span operations. Finite individual coefficients can still overflow `Sum`, and finite input/coefficient products can still overflow the output lane, so both derived values need their own finite gate. `TapFoldLattice` calls `TapFoldCore` directly, so gating only the public one-line `TapFold` source would leave the lattice path accepting non-finite input and failing only after partial mutation; its own `values-finite` claim is required. MathNet fixes `ForwardReal` storage at `N+2` for even `N` and `N+1` for odd `N`; `>=` admitted tail cells that survive mutation but disappear from `Power`, `Frequencies`, and inverse semantics. Both known packed consumers allocate the exact published length already.

Ripples: update target Auto/Boundary prose to state canonical unit-sum coefficients, source/result finiteness, and exact packed cardinality. No consumer code changes.

## 11. Align every cross-assembly tap/packed call with the owner's accessibility and admission

Location: `TapWindow.Whole`, `TapBorder.Resolve` from move 9c, `SpectralArena.PackedLength`, `Rasm.Materials/.planning/Raster/filter.md`, and `Rasm.Compute/.planning/Tensor/quadrature.md`.

### 11a. Publish the valid whole-window construction

From:

```csharp
internal static TapWindow Whole(Dimension extent, Dimension stride) =>
```

To:

```csharp
public static TapWindow Whole(Dimension extent, Dimension stride) =>
```

### 11b. Publish the packed-storage cardinality

From:

```csharp
internal static int PackedLength(int samples) => int.IsEvenInteger(samples) ? samples + 2 : samples + 1;
```

To:

```csharp
public static int PackedLength(int samples) => int.IsEvenInteger(samples) ? samples + 2 : samples + 1;
```

### 11c. Preserve the raster kernel's deliberate absent-index projection

From, in `Rasm.Materials/.planning/Raster/filter.md`:

```csharp
internal static int Address(int index, int extent, EdgeMode edge) =>
    index >= 0 && index < extent ? index : Border(edge).Resolve(index, extent);
```

To:

```csharp
internal static int Address(int index, int extent, EdgeMode edge) =>
    index >= 0 && index < extent ? index : Border(edge).Resolve(index, extent).IfNone(-1);
```

The package-local raster kernel intentionally projects absence to `-1` because its four hot callers already test `< 0` before indexing. The projection belongs at that package-local boundary; `TapBorder.Resolve` itself stays option-shaped and never publishes a sentinel.

### 11d. Route the partial window through its existing admission factory

From, in `Rasm.Materials/.planning/Raster/filter.md`:

```csharp
TapWindow columns = new(Extent: present, Origin: 0, From: window.Halo - lead, Run: window.Own, Stride: width * lanes);
Fin<Unit> folded = series.Convolve(window.Staging.Slice(lead * width * lanes, present * width * lanes),
    vertical.Span, columns, TapBorder.Omit, key);
```

To:

```csharp
Fin<TapWindow> admitted = TapWindow.Of(
    extent: Dimension.Create(present), stride: Dimension.Create(width * lanes), origin: 0,
    from: window.Halo - lead, run: Dimension.Create(window.Own), key: key);
if (admitted.Case is not TapWindow columns) { return admitted.Map(static _ => 0.0); }
Fin<Unit> folded = series.Convolve(window.Staging.Slice(lead * width * lanes, present * width * lanes),
    vertical.Span, columns, TapBorder.Omit, key);
```

Effect: target fenced LOC and total symbol counts unchanged; three existing members change `internal -> public` (`TapBorder.Resolve` in move 9c, `TapWindow.Whole`, and `SpectralArena.PackedLength`). The consumer grows by approximately three lines to use the admission owner it already imports; the existing `Address` expression gains the one explicit option projection its `int` contract already requires. This is not speculative surface growth: it aligns declared accessibility with live separate-assembly calls and removes a construction bypass.

API/consumer proof: `Rasm.Materials/Raster/filter.md` calls both `TapWindow.Whole` and `TapBorder.Resolve`, and `Rasm.Compute/Tensor/quadrature.md` calls `SpectralArena.PackedLength`; all three are separate-assembly calls where `internal` is unreachable. `Resolve` returns `Option<int>`, while the raster `Address` helper returns `int` and its four callers deliberately use negative as the hot-loop absence projection, so the current direct return is also a type mismatch; `.IfNone(-1)` makes that boundary explicit without weakening the kernel owner. The same Materials method invokes `new TapWindow(...)`, but `TapWindow` exposes only a private constructor: making it public would bypass the owner's `origin/from/run` claims, while `TapWindow.Of` is already public and returns the exact `Fin<TapWindow>` the caller can propagate. Repeating whole construction or packed arithmetic at the consumer would fork the one staged-window mint or MathNet cardinality formula, so the owner publishes only the operations callers already compose.

Ripples: the one Materials `Address` projection and the one partial-window construction above; update package prose only if it currently describes any of the three members as internal or calls omission “zero.”

## 12. Remove imports the refined spectral fence no longer reaches

Location: `[04]-[SPECTRAL]` imports.

From:

```csharp
using System.Linq;
using LanguageExt.Common;
using MathNet.Numerics;
```

To:

```csharp
// deleted
```

Effect: target fenced LOC `-3`; symbols and behavior unchanged.

API/consumer proof: the spectral fence uses no LINQ operator after the direct pair/Hartley loops, no `LanguageExt.Common` type, and no root `MathNet.Numerics` member. `Fourier`, `Hartley`, and both option enums come from the retained `MathNet.Numerics.IntegralTransforms`; `Fin`/`Option`/`Arr` come from retained `LanguageExt`.

Ripples: none.

## Net result after all moves

Target fenced LOC: approximately `-18` before prose updates and ordinary formatting; every move with a non-approximate displayed replacement states its exact local delta. Module-level type symbols are exactly `-3` (the top-level `TaperKernel` plus the interpolation capability reduction); `TaperFraming` is replaced by, rather than counted as deletion against, the stronger `TaperSampling` request union. Total authored type declarations are exactly `-1`: the sampling move costs one nested declaration, while the interpolation capability family deletes two. Authored member declarations fall by exactly twelve: sampling `-1`, the two row-local taper methods `-2`, duplicate interpolation factory `-1`, energy `-1`, and spectral helper/metric surface `-7`; the local `Bohmanned` function is a thirteenth non-member symbol deletion. Three already-required cross-assembly members become public without changing declaration count. Generated key/lookup/conversion surface disappears from all four process-local smart-enum owners, including the seventeen unsupported taper keys, and changing `Spectrum` from a positional record struct to an internally constructed sealed class also removes its generated deconstruction/value-equality/operator surface. Runtime work removes one full power materialization and reduction from every transform/modulation, the pair-power scratch rent, and the Hartley reflection rent plus three passes; the interpolation mint stops re-sorting columns already proved ascending; taper sampling gains one whole-result finite gate; tap admission adds one canonicalization pass at mint and finite gates at both raw-span entries and the derived-result boundary.

The required ripple is exactly: three FFT-window calls in `libs/dotnet/Rasm.Compute/.planning/Stats/signal.md`; five interpolation code spellings in `libs/dotnet/Rasm.Element/.planning/Composition/material.md`; one interpolation name in `libs/dotnet/Rasm/.planning/Parametric/curve.md`; one frequency-read call plus two stale kernel-spectrum parameter spellings in `libs/dotnet/Rasm.Compute/.planning/Tensor/quadrature.md`; one frequency-read call plus the three positive-axis row spellings in `libs/dotnet/Rasm.Fabrication/.planning/Additive/implicit.md`; one `Address` option projection, one partial-window construction, and three omitted-border spellings in `libs/dotnet/Rasm.Materials/.planning/Raster/filter.md`; prose in the target, `libs/dotnet/Rasm.Compute/.planning/Tensor/sampling.md`, `libs/dotnet/Rasm.Compute/.planning/Stats/signal.md`, `libs/dotnet/Rasm.Compute/.planning/Tensor/quadrature.md`, `libs/dotnet/Rasm/.planning/Parametric/curve.md`, `libs/dotnet/Rasm.Materials/.planning/Raster/filter.md`, and `libs/dotnet/Rasm/ARCHITECTURE.md`; and the thirteen missing confirmed interpolation API rows in `libs/dotnet/.api/api-mathnet-numerics.md`. `Rasm.Materials` retains its spectral case, transform, power, modulation, and tap flow but projects the owner-level border absence once, admits the partial `TapWindow`, and renames the false zero border; `Rasm.Fabrication` changes only the three positive-axis row names and `Axis -> Frequencies`.

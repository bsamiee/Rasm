# `stats.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Domain/stats.md`

This audit counts nonblank authored C# lines in the affected fence fragments. Required consumer edits outside the target are named but excluded from the target LOC total. The queue is ordered so generated shape changes land before local operator and kernel reductions.

Evidence basis: the full target; `CLAUDE.md`; the branch planning laws; the C# language, shape, surface, result, algorithm, system-API, validation, and compute standards; both checked-in `.api` tiers, with full attention to LanguageExt, Thinktecture, NodaTime, RhinoCommon, CommunityToolkit.HighPerformance, and `System.Numerics.Tensors`; every current consumer of the affected symbols; the local .NET 10 reference surface; and the prior root audit form at commit `c166d0f69`.

Accepted total for target fences: **-26 LOC, -3 authored type symbols, -10 authored member symbols**, plus removal of the unearned generated keyed lookup/conversion surfaces from three process-local smart-enum rosters.

## 1. Make only the three genuinely process-local policy rosters keyless

### Location

- `libs/dotnet/Rasm/.planning/Domain/stats.md:38-42`, anchor `[SmartEnum<int>] public sealed partial class ScalarMetric`
- `libs/dotnet/Rasm/.planning/Domain/stats.md:52-59`, anchor `[SmartEnum<int>] public sealed partial class ExtremumDirection`
- `libs/dotnet/Rasm/.planning/Domain/stats.md:65-70`, anchor `[SmartEnum<string>] public sealed partial class MomentNormalizer`

### From

```csharp
[SmartEnum<int>]
public sealed partial class ScalarMetric {
    public static readonly ScalarMetric Magnitude = new(key: 0, vector: Some<Func<Vector3d, double>>(static value => value.Length), curvature: None);
    public static readonly ScalarMetric Gaussian = new(key: 1, vector: None, curvature: Some<Func<SurfaceCurvature, double>>(static value => value.Gaussian));
    public static readonly ScalarMetric Mean = new(key: 2, vector: None, curvature: Some<Func<SurfaceCurvature, double>>(static value => value.Mean));
```

```csharp
[SmartEnum<int>]
public sealed partial class ExtremumDirection {
    public static readonly ExtremumDirection Maximum = new(key: +1, seed: double.NegativeInfinity,
```

```csharp
    public static readonly ExtremumDirection Minimum = new(key: -1, seed: double.PositiveInfinity,
```

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class MomentNormalizer {
    public static readonly MomentNormalizer Population = new(key: "population", apply: static (m2, count, mass) => m2 / mass);
    public static readonly MomentNormalizer Sample = new(key: "sample", apply: static (m2, count, mass) => count > 1 && mass > 1.0 ? m2 / (mass - 1.0) : double.NaN);
```

### To

```csharp
[SmartEnum]
public sealed partial class ScalarMetric {
    public static readonly ScalarMetric Magnitude = new(vector: Some<Func<Vector3d, double>>(static value => value.Length), curvature: None);
    public static readonly ScalarMetric Gaussian = new(vector: None, curvature: Some<Func<SurfaceCurvature, double>>(static value => value.Gaussian));
    public static readonly ScalarMetric Mean = new(vector: None, curvature: Some<Func<SurfaceCurvature, double>>(static value => value.Mean));
```

```csharp
[SmartEnum]
public sealed partial class ExtremumDirection {
    public static readonly ExtremumDirection Maximum = new(seed: double.NegativeInfinity,
```

```csharp
    public static readonly ExtremumDirection Minimum = new(seed: double.PositiveInfinity,
```

```csharp
[SmartEnum]
public sealed partial class MomentNormalizer {
    public static readonly MomentNormalizer Population = new(apply: static (m2, count, mass) => m2 / mass);
    public static readonly MomentNormalizer Sample = new(apply: static (m2, count, mass) => count > 1 && mass > 1.0 ? m2 / (mass - 1.0) : double.NaN);
```

### Effect

- Target fenced LOC: `18 -> 17` (**-1**).
- Authored symbols: unchanged; three key arguments and one key comparer attribute disappear.
- Generated surface: all three owners retain `Items` and total `Switch`/`Map`; their unused key member, keyed lookup, keyed conversion, and keyed-owner conformance disappear.

### API and consumer proof

Thinktecture's keyless `[SmartEnum]` still owns roster identity, columns, constructor delegates, and total dispatch. No planning consumer reads a key, performs lookup, converts, parses, serializes, or persists any of these three owners. This move intentionally excludes `QuantileRule`: `Rasm.Persistence/.planning/Query/serving.md:257,390` reads `QuantileRule.Interpolated.Key` and `rule.Key` to emit the backend quantile spelling, so removing that key would destroy a live boundary fact.

### Ripples

- Same file: lines 15-16 and density rows 02, 03, and 05 must say keyless `[SmartEnum]`; the case rosters and consumer signatures do not change.
- Outside target: none.

## 2. Delete the single-message join from both carrier admissions

### Location

- `libs/dotnet/Rasm/.planning/Domain/stats.md:105-108`, anchor `Scalar.ValidateFactoryArguments`
- `libs/dotnet/Rasm/.planning/Domain/stats.md:120-123`, anchor `Elapsed.ValidateFactoryArguments`

### From

```csharp
static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
    validationError = ValidityClaim.Finite(value: value).Holds
        ? null
        : new ValidationError(string.Join(" | ", new object?[] { "Scalar admits a finite measurement; the host sentinel and the infinities do not." }));
```

```csharp
static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
    validationError = ValidityClaim.Finite(value: value).Holds
        ? null
        : new ValidationError(string.Join(" | ", new object?[] { "Elapsed admits a finite second count." }));
```

### To

```csharp
static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
    validationError = ValidityClaim.Finite(value).Holds ? null
        : ValidationError.Create("Scalar admits a finite measurement; the host sentinel and the infinities do not.");
```

```csharp
static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
    validationError = ValidityClaim.Finite(value).Holds ? null
        : ValidationError.Create("Elapsed admits a finite second count.");
```

### Effect

- Target fenced LOC: `8 -> 6` (**-2**).
- Authored symbols: unchanged; two one-element arrays, two `string.Join` calls, and their named-argument noise disappear.

### API and consumer proof

The Thinktecture catalogue exposes `ValidationError.Create(string)` as the default error mint, so the hook uses the generated contract rather than a constructor spelling. Each current join has exactly one constant element, making its result byte-identical to that element with no possible separator. The generated validation hook and the `Op.AcceptValidated` boundary remain unchanged.

### Ripples

None.

## 3. Derive only the extremum seed from the closed roster

### Location

- `libs/dotnet/Rasm/.planning/Domain/stats.md:53-62`, anchor `public sealed partial class ExtremumDirection`
- `libs/dotnet/Rasm/.planning/Domain/stats.md:280-284`, anchor `initialState: (Best: direction.Seed, ...)`

### From

```csharp
public static readonly ExtremumDirection Maximum = new(seed: double.NegativeInfinity,
    beats: static (candidate, best, band) => candidate > best + band,
    within: static (candidate, best, band) => candidate >= best - band);
public static readonly ExtremumDirection Minimum = new(seed: double.PositiveInfinity,
    beats: static (candidate, best, band) => candidate < best - band,
    within: static (candidate, best, band) => candidate <= best + band);
public double Seed { get; }
[UseDelegateFromConstructor] public partial bool Beats(double candidate, double best, double band);
[UseDelegateFromConstructor] public partial bool Within(double candidate, double best, double band);
```

```csharp
initialState: (Best: direction.Seed, Hits: Seq<(TItem Item, double Score)>(), Band: band.Value, Axis: direction, Projection: projection),
```

### To

```csharp
public static readonly ExtremumDirection Maximum = new(beats: static (candidate, best, band) => candidate > best + band,
    within: static (candidate, best, band) => candidate >= best - band);
public static readonly ExtremumDirection Minimum = new(beats: static (candidate, best, band) => candidate < best - band,
    within: static (candidate, best, band) => candidate <= best + band);
[UseDelegateFromConstructor] public partial bool Beats(double candidate, double best, double band);
[UseDelegateFromConstructor] public partial bool Within(double candidate, double best, double band);
```

```csharp
initialState: (Best: direction.Map(maximum: double.NegativeInfinity, minimum: double.PositiveInfinity), Hits: Seq<(TItem Item, double Score)>(), Band: band.Value, Axis: direction, Projection: projection),
```

### Effect

- Target fenced LOC: `10 -> 7` across the owner and seed read (**-3**).
- Authored symbols: **-1 public member** (`Seed`); one generated plain constructor column and its backing state also disappear.
- Logic: the case-derived seed comes from generated total `Map`; both hot-loop comparisons remain lazy delegate columns on their selected row.

### API and consumer proof

Thinktecture's keyless smart-enum `Map` remains exhaustive over `Maximum` and `Minimum`, and `Seed` has one consumer inside `Stat.Extrema`. `Within` deliberately stays delegate-backed: `Map(maximum: candidate >= best - band, minimum: candidate <= best + band)` evaluates both result arguments before dispatch, doubling arithmetic and comparisons in the per-candidate hot loop. Deriving it through `!Beats(best, candidate, band)` is also refused because it can round `candidate + band` differently from `best - band`. Every comparison expression and external `Within` call therefore remains unchanged.

### Ripples

- Same file: `ExtremumDirection` prose and density row 03 should say only the seed derives by total generated dispatch; `Beats` and `Within` remain delegate-backed relations.
- Outside target: none; the public signature and all `Stat.Extrema` call sites remain unchanged.

## 4. Let `Option` own absent provenance and collapse `StatContext` to its two payloads

### Location

- `libs/dotnet/Rasm/.planning/Domain/stats.md:88-97`, anchor `[Union(ConversionFromValue = ConversionOperatorsGeneration.None)] public partial record StatContext`
- `libs/dotnet/Rasm/.planning/Domain/stats.md:203-230`, anchor `public readonly record struct Stat<TCarrier>`
- `libs/dotnet/Rasm/.planning/Domain/stats.md:230-277`, anchors `context.IfNone(StatContext.None)` and `private static Fin<Stat<TCarrier>> Admit`

### From

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public partial record StatContext {
    private StatContext() { }
    public sealed record NoneCase : StatContext;
    public sealed record MetricCase(ScalarMetric Metric) : StatContext;
    public sealed record BandCase(Tolerance Band) : StatContext;
    public static StatContext None { get; } = new NoneCase();
    public static StatContext Metric(ScalarMetric metric) => new MetricCase(Metric: metric);
    public static StatContext Band(Tolerance band) => new BandCase(Band: band);
}
```

```csharp
    StatContext Context) : IValidityEvidence
```

```csharp
public ValidityClaim WithinBand => Context is StatContext.BandCase held
    && Math.Max(val1: Math.Abs(value: Minimum.To()), val2: Math.Abs(value: Maximum.To())) <= held.Band.Value;
```

```csharp
context: context.IfNone(StatContext.None)
```

```csharp
context: StatContext.None
```

```csharp
private static Fin<Stat<TCarrier>> Admit(Moments held, StatContext context, Op key) =>
```

### To

```csharp
[Union<ScalarMetric, Tolerance>(T1Name = "Metric", T2Name = "Band")]
public readonly partial struct StatContext;
```

```csharp
    Option<StatContext> Context) : IValidityEvidence
```

```csharp
public ValidityClaim WithinBand => Context is { IsSome: true, Case: StatContext held } && held.IsBand
    && Math.Max(Math.Abs(Minimum.To()), Math.Abs(Maximum.To())) <= held.AsBand.Value;
```

```csharp
context: context
```

```csharp
context: None
```

```csharp
private static Fin<Stat<TCarrier>> Admit(Moments held, Option<StatContext> context, Op key) =>
```

### Effect

- Target fenced LOC: `18 -> 10` (**-8**).
- Authored symbols: **-3 nested types** (`NoneCase`, `MetricCase`, `BandCase`) and **-3 members** (`None`, `Metric`, `Band`). `StatContext` remains the sole provenance type.
- Semantic reduction: absence is no longer a fake third domain case. The existing `Option<StatContext>` entry axis remains intact through storage, while the generated union discriminates only the two distinct payload shapes.

### API and consumer proof

LanguageExt's proof-carrying `Option` pattern owns false-on-absence projection without capturing the enclosing record-struct `this` in a lambda. Thinktecture's ad-hoc union owns implicit payload intake, structural equality, and the generated `IsBand`/`AsBand` probes. No consumer switches on a nested `StatContext` case type. `Metric` is not deleted: `Stat.Merge` compares the context structurally, so the payload prevents a Gaussian-curvature population from merging with a mean-curvature population. `Merge` continues that admission law through `Option<StatContext>` equality.

### Ripples

- Same file: the two `Stat.Of` arms pass `context` unchanged; the span overload passes `None`; `Admit` takes `Option<StatContext>`; lines 15-19, 135-140, and density row 04 describe two payload variants plus optional absence.
- `libs/dotnet/Rasm/.planning/Parametric/locate.md:484,537`: `Some(StatContext.Metric(metric))` -> `Some((StatContext)metric)`.
- `libs/dotnet/Rasm/.planning/Analysis/measure.md:708,713`: `Some(StatContext.Band(band))` -> `Some((StatContext)band)`.
- `libs/dotnet/Rasm.Element/.planning/Graph/wireevidence.md:186`: the rebuilt `Stat<Scalar>` constructor's final argument becomes `Option<StatContext>.None`.
- `libs/dotnet/Rasm.Fabrication/.planning/Spec/capability.md:1203-1205`: the `Tolerance.Of(...).Match(...)` becomes `Tolerance.Of(...).Map(static band => (StatContext)band).ToOption()`, and the existing `with { Context = banded }` remains unchanged.

## 5. Collapse symmetric indexing directly onto the packed-index owner

### Location

`libs/dotnet/Rasm/.planning/Domain/stats.md:301-306`, anchor `public double this[int row, int column]`.

### From

```csharp
public double this[int row, int column] {
    get {
        (int i, int j) = row <= column ? (row, column) : (column, row);
        return UpperCovariance[index: SymmetricMatrix.FlatIndex(n: Dimension, i: i, j: j)];
    }
}
```

### To

```csharp
public double this[int row, int column] => UpperCovariance[
    SymmetricMatrix.FlatIndex(Dimension, Math.Min(row, column), Math.Max(row, column))];
```

### Effect

- Target fenced LOC: `6 -> 2` (**-4**).
- Authored symbols: unchanged; the statement body and two local aliases disappear.

### API and consumer proof

`SymmetricMatrix.FlatIndex(n, i, j)` remains the sole packed-upper address mint. `Math.Min`/`Math.Max` express the same total `(lower, upper)` normalization without a second named indexing stage. Every consumer still reads the same symmetric indexer.

### Ripples

None.

## 6. Inline the single-use percentile gate into the accumulating traversal

### Location

`libs/dotnet/Rasm/.planning/Domain/stats.md:422,431-434`, anchors `percentiles.Traverse(row => Admit(...))` and `private static Validation<Error, double> Admit`.

### From

```csharp
percentiles.Traverse(row => Admit(row: row, key: key)).As().ToFin()
```

```csharp
private static Validation<Error, double> Admit(double row, Op key) =>
    Band.Percentile.Admits(value: row)
        ? Validation<Error, double>.Success(row)
        : Validation<Error, double>.Fail(key.InvalidInput());
```

### To

```csharp
percentiles.Traverse(row => Band.Percentile.Admits(row)
    ? Validation<Error, double>.Success(row)
    : Validation<Error, double>.Fail(key.InvalidInput())).As().ToFin()
```

### Effect

- Target fenced LOC: `5 -> 3` (**-2**).
- Authored symbols: **-1 private member** (`Admit`).

### API and consumer proof

`Seq.Traverse` still performs the independent applicative inversion; `Validation<Error, double>` still accumulates every malformed percentile before the single `ToFin` exit. The removed helper has one call site and adds no domain meaning beyond that predicate-to-carrier expression.

### Ripples

None.

## 7. Delete `MarkerRow.Finite` and let the summary call the tensor owner directly

### Location

- `libs/dotnet/Rasm/.planning/Domain/stats.md:372-380`, third-fence imports
- `libs/dotnet/Rasm/.planning/Domain/stats.md:399-404`, anchor `public readonly bool Finite()`
- `libs/dotnet/Rasm/.planning/Domain/stats.md:450-455`, anchor `public bool IsValid => ValidityClaim.All`

### From

```csharp
using System.Linq;
using System.Runtime.CompilerServices;
```

```csharp
public readonly bool Finite() {
    foreach (double value in this) {
        if (!double.IsFinite(value)) { return false; }
    }
    return true;
}
```

```csharp
public bool IsValid => ValidityClaim.All(
    Fraction is > 0.0 and < 1.0,
    ValidityClaim.Nonnegative(Count),
    Heights.Finite());
```

### To

```csharp
using System.Linq;
using System.Numerics.Tensors;
using System.Runtime.CompilerServices;
```

```csharp
public bool IsValid {
    get {
        MarkerRow heights = Heights;
        return ValidityClaim.All(Fraction is > 0.0 and < 1.0, ValidityClaim.Nonnegative(Count),
            TensorPrimitives.IsFiniteAll<double>((ReadOnlySpan<double>)heights));
    }
}
```

### Effect

- Target fenced LOC: `12 -> 10` across the import, helper, and validity fragments (**-2**).
- Authored symbols: **-1 public member** (`MarkerRow.Finite`).
- The local copy exists only to provide stable storage for the inline-array-to-`ReadOnlySpan<double>` conversion; it does not mint a second buffer.

### API and consumer proof

`MarkerRow` is a contiguous five-double `[InlineArray]`. `TensorPrimitives.IsFiniteAll` is the admitted SIMD/scalar-fallback owner for the exact predicate. `QuantileSketch.IsValid` is the only consumer of `MarkerRow.Finite`; no external surface loses a used capability.

### Ripples

Add `System.Numerics.Tensors (IsFiniteAll)` to the `[04]-[ORDER_STATISTICS]` package row. No consumer edit is required.

## 8. Inline the single-use P² desired-position expression

### Location

`libs/dotnet/Rasm/.planning/Domain/stats.md:489,501-505`, anchors `double drift = Desired(...)` and `private static double Desired`.

### From

```csharp
double drift = Desired(marker: marker, fraction: prior.Fraction, count: count) - n[marker];
```

```csharp
private static double Desired(int marker, double fraction, int count) => marker switch {
    1 => 1.0 + (fraction * count / 2.0),
    2 => 1.0 + (fraction * count),
    _ => 1.0 + ((1.0 + fraction) * count / 2.0),
};
```

### To

```csharp
double drift = (marker switch {
    1 => 1.0 + (prior.Fraction * count / 2.0),
    2 => 1.0 + (prior.Fraction * count),
    _ => 1.0 + ((1.0 + prior.Fraction) * count / 2.0),
}) - n[marker];
```

### Effect

- Target fenced LOC: `6 -> 5` (**-1**).
- Authored symbols: **-1 private member** (`Desired`).

### API and consumer proof

The expression has one call site and consumes only the surrounding marker loop's three locals. Inlining preserves all three P² coefficients and seats the desired-position formula exactly where it becomes drift; no other consumer or abstraction boundary names it.

### Ripples

None.

## 9. Remove ranking state already captured by `PriorityQueue`

### Location

- `libs/dotnet/Rasm/.planning/Domain/stats.md:510-523`, anchors `private static readonly Comparer<TKey> Forward`, `private readonly Comparer<TKey> evict`, and `public ExtremumDirection Direction`
- `libs/dotnet/Rasm/.planning/Domain/stats.md:527-530`, anchor `public void Offer`

### From

```csharp
private static readonly Comparer<TKey> Forward = Comparer<TKey>.Default;
private static readonly Comparer<TKey> Reversed = Comparer<TKey>.Create(static (left, right) => right.CompareTo(left));

private readonly PriorityQueue<T, TKey> heap;
private readonly Comparer<TKey> evict;
private readonly int keep;

public Ranked(int keep, ExtremumDirection direction) {
    (this.keep, Direction) = (keep, direction);
    evict = direction.Map(maximum: Forward, minimum: Reversed);
    heap = new PriorityQueue<T, TKey>(initialCapacity: keep, comparer: evict);
}

public ExtremumDirection Direction { get; }
```

```csharp
public void Offer(T item, TKey key) {
    if (heap.Count < keep) { heap.Enqueue(item, key); }
    else if (heap.TryPeek(out _, out TKey worst) && evict.Compare(key, worst) > 0) { heap.EnqueueDequeue(item, key); }
}
```

### To

```csharp
private static readonly Comparer<TKey> Reversed = Comparer<TKey>.Create(static (left, right) => right.CompareTo(left));

private readonly PriorityQueue<T, TKey> heap;
private readonly int keep;

public Ranked(int keep, ExtremumDirection direction) {
    this.keep = keep;
    heap = new PriorityQueue<T, TKey>(initialCapacity: keep,
        comparer: direction.Map(maximum: Comparer<TKey>.Default, minimum: Reversed));
}
```

```csharp
public void Offer(T item, TKey key) {
    if (heap.Count < keep) { heap.Enqueue(item, key); }
    else if (heap.TryPeek(out _, out TKey worst) && heap.Comparer.Compare(key, worst) > 0) { heap.EnqueueDequeue(item, key); }
}
```

### Effect

- Target fenced LOC: `15 -> 12` (**-3**).
- Authored symbols: **-3 members** (`Forward`, `evict`, `Direction`).
- Runtime shape: the reversed comparer remains one static instance per closed `TKey`; no per-cell comparer allocation is introduced.

### API and consumer proof

The local .NET 10 reference surface exposes `PriorityQueue<TElement,TPriority>.Comparer`, the exact comparer supplied at construction. `Comparer<TKey>.Default` is already the forward singleton, so `Forward` only forwards it. The only three streaming-cell consumers (`Rasm.Compute/Model/run.md:335`, `Rasm.Persistence/Query/retrieval.md:452`, and `Rasm/Spatial/index.md:595`) use `Offer`, `Bound`, and `Drain`; none reads `Direction`. Static `Ranked.Top` consumers are unaffected.

### Ripples

None; the owner prose already treats direction as construction policy and the heap comparer as its operational realization.

## Deliberately retained

- `StatContext.Metric` remains as one ad-hoc-union payload: `Stat.Merge` compares context structurally, so deleting the apparently unread label would admit a Gaussian-curvature summary to merge with a mean-curvature summary. Only the fake `NoneCase` collapses onto `Option`.
- `QuantileRule` remains keyed: Persistence emits its string key into backend query syntax at two live sites.
- `ScalarMetric.Read<TPayload>` remains: both typed `Of` overloads consume it, and it centralizes the identical payload-admission/projection-admission chain. Duplicating that body to remove a two-consumer helper is relocation, not refinement.
- `Moments.Advance` remains: it binds `mass` and `delta` once for the dense four-moment recurrence. Inlining repeats those expressions or adds statement state.
- `Stat<TCarrier>.State` remains: both `Merge` and `Update` use the one projection from admitted summary back to recurrence state.
- `SampleMoment.Of` retains its left-to-right `Seq.Fold` weight sum: replacing it with `TensorPrimitives.Sum(raw.AsSpan())` can reassociate IEEE-754 addition under SIMD, changing the normalized weights and covariance by rounding; fewer source lines do not justify a different statistic.
- `Distribution.Settle` remains: it names the dependent carrier chain and keeps the median local without an artificial success binding or repeated quantile reads.
- `QuantileSketch.Of` retains the initial `Positions = [1,2,3,4,5]` fill: `Positions` is a public primary-constructor property and participates in record equality, hashing, formatting, and any boundary projection. Later seed updates overwrite those slots before adjustment, but replacing the factory result with `default` is still an observable state change, not dead-code deletion.
- `Ranked` remains split into generic cell and non-generic one-shot companion: collapsing the companion loses generic inference or the streaming modality without reducing behavior.
- `Scalar` and `Elapsed` remain distinct value objects: their domain identities and boundary bridges differ even though their generic-math conformance is shared.

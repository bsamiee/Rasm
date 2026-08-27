# `Numerics/integrate.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Numerics/integrate.md`

Apply the twelve moves in order. Counts refer to authored, nonblank C# fence lines; generated Thinktecture members are excluded. The ordered result removes the duplicate tableau walk, two payload-free mirror vocabularies, the transient quadrature outcome type, one hand-written Thinktecture delegate field, six single-use/forwarding members, independently optional embedded-tableau facts, split controller history, and the public reach to the quadrature-table implementation. It also nests the three implementation-only tableau/Smolyak owners under the operations that own them, corrects the `IntegrationStep`/trajectory-driver signature mismatch, and deletes an infinite-bound column whose sole false row contradicts the installed MathNet facade. No move adds a module-level helper, enum, policy rail, wrapper, or compatibility alias.

Authority: `CLAUDE.md`; the owning `libs/`, `libs/dotnet/`, and `libs/dotnet/Rasm/` architecture and ruling surfaces; the full `docs/stacks/csharp/` doctrine, especially `algorithms.md` `[INTEGRATOR_TABLEAU]` and `[QUADRATURE]`; both checked-in `.api` tiers, especially LanguageExt, Thinktecture, MathNet.Numerics, System.Numerics.Tensors, CommunityToolkit.HighPerformance, and TYoshimura.DoubleDouble; and direct consumers across `Rasm`, `Rasm.Compute`, and `Rasm.Rhino`.

## 1. Derive verified order in the one 106-bit condition walk

`ConditionsOf` already evaluates every rooted-tree condition through `ddouble`. `VerifiedOrderOf` repeats the complete walk in `double`, requiring a second coupling-matrix materialization and a second recursive `Weight` implementation. Carry the verified prefix on `OrderConditions` and delete the lower-precision twin.

### 1a. Compute and carry the verified prefix

**Location:** `libs/dotnet/Rasm/.planning/Numerics/integrate.md`, anchors `internal OrderConditions ConditionsOf` and `public readonly record struct OrderConditions`.

**From:**

```csharp
(int Count, int Failed, double Max) state = (0, 0, 0.0);
for (int p = 1; p <= order; p++) {
    foreach (RootedTree tree in RootedTree.OfOrder(order: p)) {
        ddouble[] phi = tree.Weight(a: aWide, stages: StageCount);
        ddouble lhs = 0.0;
        for (int i = 0; i < StageCount; i++) lhs += (ddouble)b[i] * phi[i];
        double residual = Math.Abs(value: (double)lhs - (1.0 / tree.Density));
        state = (
            Count: state.Count + 1,
            Failed: state.Failed + (double.IsFinite(residual) && residual <= CoefficientTolerance ? 0 : 1),
            Max: Math.Max(val1: state.Max, val2: residual));
    }
}
return new OrderConditions(StageCount: StageCount, MethodOrder: order, EmbeddedOrder: embeddedOrder, CheckedConditionCount: state.Count, FailedConditionCount: state.Failed, MaxResidual: state.Max);
```

**To:**

```csharp
(int Count, int Failed, double Max, int Verified) state = (0, 0, 0.0, 0);
for (int p = 1; p <= order; p++) {
    int failedBefore = state.Failed;
    foreach (RootedTree tree in RootedTree.OfOrder(p)) {
        ddouble[] phi = tree.Weight(aWide, StageCount);
        ddouble lhs = 0.0;
        for (int i = 0; i < StageCount; i++) lhs += b[i] * phi[i];
        double residual = Math.Abs((double)lhs - (1.0 / tree.Density));
        state = (state.Count + 1,
            state.Failed + (double.IsFinite(residual) && residual <= CoefficientTolerance ? 0 : 1),
            Math.Max(state.Max, residual), state.Verified);
    }
    if (state.Failed == failedBefore && state.Verified == p - 1) state.Verified = p;
}
return new(StageCount, order, embeddedOrder, state.Verified, state.Count, state.Failed, state.Max);
```

**From:**

```csharp
public readonly record struct OrderConditions(int StageCount, int MethodOrder, Option<int> EmbeddedOrder, int CheckedConditionCount, int FailedConditionCount, double MaxResidual) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        StageCount >= 1 && MethodOrder >= 1 && CheckedConditionCount >= 0,
        EmbeddedOrder.Map(static order => order >= 1).IfNone(noneValue: true),
        ValidityClaim.CountExactly(count: FailedConditionCount, expected: 0),
        ValidityClaim.Nonnegative(value: MaxResidual),
        MaxResidual <= ButcherTableau.CoefficientTolerance);
}
```

**To:**

```csharp
internal readonly record struct OrderConditions(
    int StageCount, int MethodOrder, Option<int> EmbeddedOrder, int VerifiedOrder,
    int CheckedConditionCount, int FailedConditionCount, double MaxResidual) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        StageCount >= 1 && MethodOrder >= 1 && VerifiedOrder == MethodOrder && CheckedConditionCount >= 0,
        EmbeddedOrder.Map(static order => order >= 1).IfNone(true),
        ValidityClaim.CountExactly(FailedConditionCount, 0),
        ValidityClaim.Nonnegative(MaxResidual),
        MaxResidual <= ButcherTableau.CoefficientTolerance);
}
```

### 1b. Stop storing and passing a second verified-order column

**Location:** same file, anchors `public int VerifiedOrder`, `private static IntegratorKind Of`, and `internal Fin<ButcherTableau> Admit`.

**From:**

```csharp
public ButcherTableau Tableau { get; }
public OrderConditions Conditions { get; }
public int VerifiedOrder { get; }
```

**To:**

```csharp
internal ButcherTableau Tableau { get; }
internal ButcherTableau.OrderConditions Conditions { get; }
```

**From:**

```csharp
return new(key: key, tableau: tableau, conditions: tableau.ConditionsOf(weights: tableau.Weights, order: order, embeddedOrder: tableau.EmbeddedOrder),
    verifiedOrder: tableau.VerifiedOrderOf(), denseFamily: DenseOutputCoefficientFamily.Identify(tableau: tableau));
```

**To:**

```csharp
ButcherTableau.OrderConditions conditions = tableau.ConditionsOf(tableau.Weights, order, tableau.EmbeddedOrder);
return new(key: key, tableau: tableau, conditions: conditions,
    denseFamily: DenseOutputCoefficientFamily.Identify(tableau));
```

**From:**

```csharp
internal Fin<ButcherTableau> Admit(OrderConditions conditions, int verifiedOrder, Op key) =>
    Valid(conditions: conditions)
        ? Fin.Succ(this)
        : Fin.Fail<ButcherTableau>(new KernelFault.InvalidValue(
            Label: $"butcher-tableau:stages={StageCount}:order={MethodOrder}",
            Requirement: $"every order condition within {CoefficientTolerance:e1} — failed {conditions.FailedConditionCount} of {conditions.CheckedConditionCount}, max residual {conditions.MaxResidual:e3}, verified order {verifiedOrder}",
            Key: key));
```

**To:**

```csharp
internal Fin<ButcherTableau> Admit(OrderConditions conditions, Op key) =>
    Valid(conditions)
        ? Fin.Succ(this)
        : Fin.Fail<ButcherTableau>(new KernelFault.InvalidValue(
            Label: $"butcher-tableau:stages={StageCount}:order={MethodOrder}",
            Requirement: $"every order condition within {CoefficientTolerance:e1} — failed {conditions.FailedConditionCount} of {conditions.CheckedConditionCount}, max residual {conditions.MaxResidual:e3}, verified order {conditions.VerifiedOrder}",
            Key: key));
```

Update both factory calls from `Admit(conditions: active.Conditions, verifiedOrder: active.VerifiedOrder, key: key)` to `Admit(active.Conditions, key)`.

### 1c. Delete the duplicate `double` route

**Location:** same file, anchors `internal int VerifiedOrderOf`, `private double[,] CouplingMatrix`, `private ddouble[,] WideCouplingMatrix`, `public static int VerifiedOrder`, and `public double[] Weight`.

**From:** the five declarations and bodies.

**To:** delete them whole; retain the `ddouble[] Weight` recursion as the one tree arithmetic. `WideCouplingMatrix` is also a single-use materializer, so inline its fill at the start of `ConditionsOf` and delete that helper:

```csharp
// From
ddouble[,] aWide = WideCouplingMatrix();

// To
ddouble[,] aWide = new ddouble[StageCount, StageCount];
int row = 0;
foreach (Seq<double> coupling in Coupling) {
    int column = 0;
    foreach (double coefficient in coupling) aWide[row, column++] = coefficient;
    row++;
}
```

Also change `public readonly record struct ButcherTableau` to `internal readonly record struct ButcherTableau`. Move the complete `RootedTree` declaration unchanged inside `ButcherTableau` as `private sealed record RootedTree`, immediately before the tableau's closing brace. Move the revised `OrderConditions` declaration from module scope into the same owner as `internal readonly record struct OrderConditions`, and qualify both outside-owner reads as `ButcherTableau.OrderConditions` (the `conditions` local in `Of` and the row's `Conditions` column). Neither nested type has an independent consumer or lifecycle; leaving either at module scope preserves the internal-symbol spread this move removes.

Move the existing ceiling from the nested algorithm onto its containing tableau so both admission and generation read one private authority:

```csharp
// From, inside RootedTree
private const int PoolCeiling = 10;

// To, inside ButcherTableau
private const int OrderCeiling = 10;
```

Change `RootedTree.Generate` to loop through `OrderCeiling`, tighten the existing `MethodOrder > 0` validity clause to `MethodOrder is > 0 and <= OrderCeiling`, and apply the same ceiling to the embedded order. A containing type cannot read a private member declared by its nested type; moving the unchanged constant outward keeps it private, adds no symbol, and lets the nested `RootedTree` read its containing type's private authority. Otherwise an out-of-pool declared order sees an empty `OfOrder` result and can advance the verified prefix without checking a condition.

**Effect:** fenced LOC approximately `75 -> 40` (`-35`); declared members `-6` (`IntegratorKind.VerifiedOrder`, `ButcherTableau.VerifiedOrderOf`, both coupling-matrix helpers, `RootedTree.VerifiedOrder`, and one `RootedTree.Weight` overload); publicly reachable types `-3`; module-level types `-2`; full order-condition walks per row `2 -> 1`; coupling matrices per row `2 -> 1`.

**API/consumer proof:** `ddouble` admits mixed `double` arithmetic and comparisons, and its 106-bit result narrows explicitly to `double`; the folder catalogue makes that the stronger existing route. No `libs/dotnet/` consumer names `ButcherTableau`, `RootedTree`, `OrderConditions`, `IntegratorKind.Tableau`, `Conditions`, or `VerifiedOrder`. `FieldIntegrator.MethodOrder` and `EmbeddedOrder` remain the public result columns consumed by `Processing/flow.md`.

**Ripples:** update the `[02]-[TABLEAU_VOCABULARY]` card to state that `OrderConditions.VerifiedOrder` derives in the same 106-bit walk. Remove every claim of a separate `VerifiedOrderOf` pass. No consumer fence changes beyond the two local factory calls.

## 2. Couple embedded weights and order in one optional value

`EmbeddedWeights` and `EmbeddedOrder` are one fact split into two independently absent columns. The current validity body does not require their presence to agree: an embedded weight row with no embedded order is admitted by substituting order one, and the adaptive step silently substitutes exponent `1.0`. Make the embedded pair one `Option` so the invalid half-present states cannot be constructed, and consume its order directly after the adaptive factory has proved presence.

**Location:** same file, anchors the adaptive `IntegratorKind` rows, private `Of`, `ButcherTableau` constructor/properties, `Valid`, `ConditionsOf`, `OrderConditions`, `IsAdaptive`, `AdaptiveExponent`, and the adaptive `Step` arm.

**From:**

```csharp
private static IntegratorKind Of(int key, int order, double[][] coupling, double[] weights, double[]? errorWeights = null, int? embeddedOrder = null) {
    ButcherTableau tableau = ButcherTableau.Of(
        coupling: toSeq(coupling.Select(static row => toSeq(row))), weights: toSeq(weights),
        embedded: Optional(errorWeights).Map(toSeq), order: order, embeddedOrder: Optional(embeddedOrder));
```

**To:**

```csharp
private static IntegratorKind Of(
    int key, int order, double[][] coupling, double[] weights,
    Option<(double[] Weights, int Order)> embedded = default) {
    ButcherTableau tableau = ButcherTableau.Of(
        toSeq(coupling.Select(static row => toSeq(row))), toSeq(weights),
        embedded.Map(static pair => (toSeq(pair.Weights), pair.Order)), order);
```

Change each adaptive row tail from separate `embeddedOrder:` and `errorWeights:` arguments to one explicitly target-typed tuple, for example `embedded: Some<(double[] Weights, int Order)>((Weights: [...], Order: 2))`. Apply the same spelling to Cash-Karp and Dormand-Prince with order `4`; the explicit generic argument gives each collection expression its required `double[]` target.

**From:**

```csharp
private ButcherTableau(Seq<Seq<double>> coupling, Seq<double> abscissae, Seq<double> weights, Option<Seq<double>> embeddedWeights, int methodOrder, Option<int> embeddedOrder) =>
    (Coupling, Abscissae, Weights, EmbeddedWeights, MethodOrder, EmbeddedOrder) =
        (coupling, abscissae, weights, embeddedWeights, methodOrder, embeddedOrder);
public Option<Seq<double>> EmbeddedWeights { get; }
public Option<int> EmbeddedOrder { get; }
```

**To:**

```csharp
private ButcherTableau(
    Seq<Seq<double>> coupling, Seq<double> abscissae, Seq<double> weights,
    Option<(Seq<double> Weights, int Order)> embedded, int methodOrder) =>
    (Coupling, Abscissae, Weights, Embedded, MethodOrder) =
        (coupling, abscissae, weights, embedded, methodOrder);
internal Option<(Seq<double> Weights, int Order)> Embedded { get; }
```

Replace the two independent embedded validity clauses with one fold:

```csharp
Embedded.Map(pair => pair.Order is > 0 and < MethodOrder
    && pair.Order <= RootedTree.PoolCeiling
    && pair.Weights.Count == StageCount
    && CoefficientsMatch(pair.Weights, 1.0)
    && ConditionsOf(pair.Weights, pair.Order).IsValid).IfNone(true)
```

Remove `embeddedOrder` from `ConditionsOf` and remove `EmbeddedOrder` from `OrderConditions`; after move 1 its constructor is `new(StageCount, order, state.Verified, state.Count, state.Failed, state.Max)`. Delete `IntegratorKind.IsAdaptive` and `AdaptiveExponent`. The fixed/adaptive factory guards become `!active.Tableau.Embedded.IsSome`/`active.Tableau.Embedded.IsSome`; `FieldIntegrator.EmbeddedOrder` projects `Kind.Tableau.Embedded.Map(static pair => pair.Order)` directly.

Delete the internal `FieldIntegrator.Tableau => Kind.Tableau` forwarding property in the same pass. Its two reads become direct owner projections:

```csharp
public int MethodOrder => Kind.Tableau.MethodOrder;
public Option<int> EmbeddedOrder => Kind.Tableau.Embedded.Map(static pair => pair.Order);
```

In the adaptive step arm, replace the independent weight extraction and optional exponent fallback:

```csharp
from embedded in c.Kind.Tableau.Embedded.ToFin(s.Key.InvalidInput())
from ks in Stages(/* unchanged */)
// ...
let secondary = s.Module.Combine(embedded.Weights, ks)
let scale = c.Control.Rescale(s.History, err, c.Tolerance.Value, 1.0 / (embedded.Order + 1.0))
```

**Effect:** fenced LOC approximately `32 -> 23` (`-9`); tableau columns `-1`; `OrderConditions` columns `-1`; internal members `-3` (`IsAdaptive`, `AdaptiveExponent`, `FieldIntegrator.Tableau`); optional factory parameters `2 -> 1`; half-present embedded states and silent exponent fallbacks removed.

**API/consumer proof:** LanguageExt `Option<(Weights, Order)>` is the admitted presence carrier; the tuple has one lifecycle and no independent identity. Every current adaptive row supplies both values, every fixed row supplies neither, and the only consumers need either both (adaptive stepping) or the projected order (`FieldIntegrator.EmbeddedOrder`). No generated Thinktecture type is needed for a private two-field payload.

**Ripples:** update `[02]-[TABLEAU_VOCABULARY]` and `[04]-[STEPPER]` to describe one optional embedded formula. No consumer signature changes.

## 3. Inline the one moment sum and use `ddouble.Pow`

`MomentSum` exists only to serve `MomentResidual`, and `Raise` exists only to serve `MomentSum`. Seat the exact arithmetic at the residual that owns it and let DoubleDouble provide integral power.

**Location:** same file, anchors `internal static double MomentSum`, `internal static ddouble Raise`, and `private static (bool Failed, double Max) MomentResidual`.

**From:**

```csharp
internal static double MomentSum(Seq<double> weights, Seq<double> against, int power) =>
    (double)weights.Zip(against).Fold(initialState: (ddouble)0.0, f: (sum, pair) => sum + ((ddouble)pair.First * Raise(value: pair.Second, power: power)));
internal static ddouble Raise(double value, int power) {
    ddouble accumulated = (ddouble)1.0;
    for (int step = 0; step < power; step++) { accumulated *= (ddouble)value; }
    return accumulated;
}
```

**To:** delete both declarations whole, then replace the residual projection:

```csharp
private static (bool Failed, double Max) MomentResidual(
    ButcherTableau tableau, Seq<double> weights, double theta, int order) =>
    Enumerable.Range(0, order)
        .Select(moment => {
            ddouble actual = weights.Zip(tableau.Abscissae).Fold((ddouble)0.0,
                (sum, pair) => sum + (pair.First * ddouble.Pow(pair.Second, moment)));
            return Math.Abs((double)actual - (Math.Pow(theta, moment + 1) / (moment + 1.0)));
        })
        .Aggregate(seed: (Failed: false, Max: 0.0), func: static (state, residual) => (
            Failed: state.Failed || !double.IsFinite(residual) || residual > ButcherTableau.CoefficientTolerance,
            Max: Math.Max(state.Max, residual)));
```

**Effect:** the two helper bodies plus the old residual body contract from `13 -> 10` authored lines (`-3`); internal members `-2`; handwritten loops `-1`; single-use forwarding members `-1`.

**API/consumer proof:** `api-doubledouble.md` proves `ddouble.Pow(x, long n)` and implicit widening from `double`; the result therefore remains 106-bit before its one explicit narrowing. `MomentResidual` is the only caller of `MomentSum`, and `MomentSum` is the only caller of `Raise`.

**Ripples:** in `[02]-[TABLEAU_VOCABULARY]`, replace “integral powers raise by repeated multiplication” with “each residual raises through `ddouble.Pow` inside its 106-bit fold.”

## 4. Delete `DenseOutputSource`; the published table option already is the discriminant

`DenseOutputSource` is a payload-free two-case roster mirroring `DenseOutputCoefficientFamily.Published.IsSome`. The C# shape law explicitly rejects such a family; the `Option` already carries both the discriminant and the published payload.

**Location:** same file, anchors `public sealed partial class DenseOutputSource`, `DenseOutputCoefficientFamily.Published`, `Source`, `Matches`, `MaxAbs`, and all `.Source` reads.

**From:**

```csharp
[SmartEnum<string>]
public sealed partial class DenseOutputSource {
    public static readonly DenseOutputSource Published = new("published");
    public static readonly DenseOutputSource MomentFit = new("moment-fit");
}
```

**To:**

```csharp
```

**From:**

```csharp
public sealed partial class DenseOutputCoefficientFamily {
    public int FixedDenseOrder { get; }
    private Option<(double[] Fingerprint, double[][] Table)> Published { get; }
    public DenseOutputSource Source => Published.IsSome ? DenseOutputSource.Published : DenseOutputSource.MomentFit;
```

**To:**

```csharp
[SmartEnum]
internal sealed partial class DenseOutputCoefficientFamily {
    internal int FixedDenseOrder { get; }
    internal Option<(double[] Fingerprint, double[][] Table)> Published { get; }
```

Change `[SmartEnum<int>]` to keyless `[SmartEnum]` and remove `key: 0/1/2` from its three row constructors; no consumer reads, serializes, looks up, or persists this implementation-only roster's key. Replace every `family.Source == DenseOutputSource.Published` with `family.Published.IsSome`, and every moment-fit comparison with `!family.Published.IsSome`.

Remove `Family` and the constant `CheckedThetaCount` from `DenseConditions` entirely. `Conditions` always probes exactly `0`, `0.5`, and `1`; storing `3` on every result adds a column that can only disagree with the algorithm. Its evidence-shape law becomes:

**From:**

```csharp
CheckedConditionCount >= CheckedThetaCount,
Endpoints.Derivatives.IsSome == (Family.Source == DenseOutputSource.Published),
CorrectionSolve.IsSome == (Family.Source == DenseOutputSource.MomentFit && CheckedThetaCount >= 3),
```

**To:**

```csharp
CheckedConditionCount >= 3,
Endpoints.Derivatives.IsSome ^ CorrectionSolve.IsSome,
```

Delete `CheckedThetaCount: 3` and `Family: family` at the sole `DenseConditions` construction and remove both positional columns.

Delete the two tableau forwarding shells in the same ownership repair:

**From:**

```csharp
internal Fin<DenseConditions> DenseConditionsOf(DenseOutputCoefficientFamily family, ButcherDenseOutput.DenseOutputInterpolant interpolant, Op key) =>
    ButcherDenseOutput.Conditions(tableau: this, family: family, interpolant: interpolant, key: key);
internal Fin<Seq<double>> DenseWeightsAt(DenseOutputCoefficientFamily family, ButcherDenseOutput.DenseOutputInterpolant interpolant, double theta, Op key) =>
    ButcherDenseOutput.WeightsAt(tableau: this, family: family, interpolant: interpolant, theta: theta, key: key);
```

**To:** delete both declarations. The four local calls reach the actual owner directly:

```csharp
ButcherDenseOutput.Conditions(tableau, family, interpolant, key)
ButcherDenseOutput.WeightsAt(tableau, family, interpolant, theta, key)
```

The wrappers expose no simpler shape, enforce no invariant, and are unreachable without the same internal family/interpolant values; they are rename-forwarders inside one fence, not tableau behavior. Also inline both `MaxAbs` calls into the two `EndpointEvidence` arms and delete its helper:

```csharp
ValueLeft: atZero.Fold(0.0, static (max, value) => Math.Max(max, Math.Abs(value))),
ValueLeft: atZero.Values.Fold(0.0, static (max, value) => Math.Max(max, Math.Abs(value))),
```

`Matches` is likewise a one-caller predicate with no meaning outside `Identify`; collapse it after the published-option replacement:

```csharp
// From
internal static DenseOutputCoefficientFamily Identify(ButcherTableau tableau) =>
    toSeq(Items).Find(family => family.Published.IsSome && family.Matches(tableau)).IfNone(GenericMomentFit);
private bool Matches(ButcherTableau tableau) =>
    Published.Exists(held => tableau.StageCount == held.Fingerprint.Length
        && tableau.IsFunctionalSameAsLast
        && held.Fingerprint.Zip(tableau.Abscissae).All(pair => Math.Abs(pair.First - pair.Second) <= ButcherTableau.CoefficientTolerance));

// To
internal static DenseOutputCoefficientFamily Identify(ButcherTableau tableau) =>
    toSeq(Items).Find(family => family.Published.Exists(held =>
        tableau.StageCount == held.Fingerprint.Length && tableau.IsFunctionalSameAsLast
        && held.Fingerprint.Zip(tableau.Abscissae).All(pair =>
            Math.Abs(pair.First - pair.Second) <= ButcherTableau.CoefficientTolerance)))
        .IfNone(GenericMomentFit);
```

**Effect:** fenced LOC approximately `26 -> 11` (`-15`); module-level types `-1`; declared members `-5` (`Source`, `DenseConditionsOf`, `DenseWeightsAt`, `MaxAbs`, `Matches`); declared smart-enum rows `-2`, publicly reachable smart-enum rows `-5`; public evidence columns `-2`; one unused keyed lookup/conversion/serialization surface disappears; `DenseOutputCoefficientFamily` becomes implementation-only.

**API/consumer proof:** `Option<T>.IsSome` is the package-owned presence discriminant. The published case carries endpoint derivative evidence and no correction solve; the fitted case carries a correction solve and no derivative pair, so the XOR is the complete evidence-shape law without a parallel family tag. No `libs/dotnet/` consumer names `DenseOutputSource`, `DenseOutputCoefficientFamily`, or `DenseConditions.Family`.

**Ripples:** remove `DenseOutputSource` and the two false tableau “entries” from `[01]-[INDEX]`, `[03]-[DENSE_OUTPUT]`, and `[06]-[DENSITY_BAR]`; describe the published-table option as the one source discriminant and `ButcherDenseOutput.Conditions`/`WeightsAt` as the internal operations. No other file changes.

## 5. Keep controller history at the driver and make its pair total

A fixed method cannot reject, so every rejection has an error while a fixed acceptance has none. The current `StepHistory(Option<double>, double)` nevertheless permits a half-present pair, and both drivers assign nonexistent `accepted.History`/`rejected.History` members. History is a RUN fact, not a kernel outcome: Compute clamps the suggested step against `MaxStep` and the remaining horizon, Processing owns its next trace step, and Gustafsson's previous scale is the driver-selected `h[n+1] / h[n]` after policy caps, not the stepper's uncapped suggestion. Keep the step result minimal, make rejection error mandatory, and let each driver mint the pair after selecting its next step.

**Location:** same file, anchors `StepHistory`, the three `StepLaw` history reads, `IntegrationStep`, the `Step` history parameter, and all accepted/rejected constructions.

**From:**

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct StepHistory(Option<double> PreviousError, double PreviousScale) {
    public static StepHistory Fresh => new(PreviousError: None, PreviousScale: 1.0);
}
```

**To:**

```csharp
public readonly record struct StepHistory(double Error, double Scale);
```

Change each controller's `history.PreviousError.Map(...)` to `history.Map(previous => ...)`, read `previous.Error`/`previous.Scale`, and change `StepLaw.Rescale` plus `StepControl.Rescale` to accept `Option<StepHistory> history`.

Change only the rejected payload; `AcceptedCase.Error` remains the honest fixed/adaptive discriminator:

```csharp
// From
public sealed record RejectedCase(
    double SuggestedStep, Option<double> Error) : IntegrationStep<TState, TDelta>;

// To
public sealed record RejectedCase(
    double SuggestedStep, double Error) : IntegrationStep<TState, TDelta>;
```

Change the public step parameter from `StepHistory history = default` to `Option<StepHistory> history = default`. The fixed accepted arm still returns `Error: None`; the adaptive arm returns its measured error without constructing run state:

```csharp
let scale = c.Control.Rescale(s.History, err, c.Tolerance.Value, 1.0 / (embedded.Order + 1.0))
let next = s.Module.Add(s.State, s.H, primary)
from result in err <= c.Tolerance.Value
    ? DenseOutputSpan<TState, TDelta>.Of(/* unchanged; no end argument */)
        .Map(dense => (IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.AcceptedCase(
            next, s.H * scale, Some(err), dense))
    : Fin.Succ((IntegrationStep<TState, TDelta>)new IntegrationStep<TState, TDelta>.RejectedCase(
        s.H * scale, err))
```

Hide `DenseOutputSpan`'s implementation state in the same move. The current public positional record exposes `ButcherTableau`, the coefficient family, and an internal nested interpolant; after moves 1 and 4 internalize those types, that shape is not merely wide but inconsistently accessible. No consumer reads any positional column except `Conditions`; consumers call `PointAt`. Replace the positional header with an explicit body, private constructor, and private fields:

```csharp
// From
public readonly record struct DenseOutputSpan<TState, TDelta>(
    TState Start, TState End, double Step, Seq<TDelta> Stages, ButcherTableau Tableau,
    DenseOutputCoefficientFamily Family, ButcherDenseOutput.DenseOutputInterpolant Interpolant,
    DenseConditions Conditions, IntegrationModule<TState, TDelta> Module) {

// To
public readonly record struct DenseOutputSpan<TState, TDelta> {
private readonly TState start;
private readonly double step;
private readonly Seq<TDelta> stages;
private readonly ButcherTableau tableau;
private readonly DenseOutputCoefficientFamily family;
private readonly ButcherDenseOutput.DenseOutputInterpolant interpolant;
private readonly IntegrationModule<TState, TDelta> module;
public DenseConditions Conditions { get; }

private DenseOutputSpan(
    TState start, double step, Seq<TDelta> stages, ButcherTableau tableau,
    DenseOutputCoefficientFamily family, ButcherDenseOutput.DenseOutputInterpolant interpolant,
    DenseConditions conditions, IntegrationModule<TState, TDelta> module) =>
    (this.start, this.step, this.stages, this.tableau, this.family, this.interpolant, Conditions, this.module) =
        (start, step, stages, tableau, family, interpolant, conditions, module);
```

Update `Of` and `PointAt` to the private field names. Remove `TState end` from `Of`, `End: end` from construction, and both local `end:` arguments. `PointAt` reconstructs from `start + h·Σbᵢ(θ)kᵢ`, and `AcceptedCase.Next` remains the one terminal state; carrying it on both nested results was duplication.

**Effect:** target fenced LOC approximately `+10` for the accessibility repair; `StepHistory` columns `2 -> 2` but invalid half-present states become unrepresentable; rejected error optionality `Option<double> -> double`; public span columns `9 -> 1`; the accepted-state `Add` runs `2 -> 1`. The overall audit remains substantially LOC-negative while making the intended internalizations legal.

**API/consumer proof:** `Option<StepHistory>` on each driver means exactly “the previous step was adaptive”; `RejectedCase.Error` is mandatory because only the adaptive arm can construct that case. Thinktecture's generated `Switch` remains exhaustive, and the controller reads one admitted previous pair rather than two independently valid fields. The driver knows both the step actually passed to `Step` and every cap it will apply to a continuing next step, so it alone can mint the selected scale Gustafsson reads.

**Ripples:**

- `Rasm.Compute/.planning/Tensor/quadrature.md`: change `TrajectoryCursor.History` to `Option<StepHistory>` and initialize it to `None`; delete the parallel `Option<double> Error` cursor column; pass the actual `step` into both outcome helpers. The accepted helper computes the capped proposal and remaining horizon, then applies the horizon cap only when `Land` will continue (`remaining > control.MinStep`); a terminal cursor keeps the positive capped proposal because there is no next realized step. The rejected helper keeps the current time and caps the proposal by both `control.MaxStep` and the unchanged remaining horizon before minting history. The accepted arm mints `accepted.Error.Map(error => new StepHistory(error, nextStep / step))`; the rejected arm mints `Some(new StepHistory(rejected.Error, nextStep / step))`. The rejection finite gate reads `double.IsFinite(rejected.Error)`. Keep passing the cursor's optional history into `Step`. Any result projection reads `Cursor.History.Map(static h => h.Error)`.
- `Rasm.Compute/.planning/Solver/route.md`: the current fence already reaches nonexistent `TrajectoryRun.LastError`, `.Steps`, `.Achieved`, and `.Rejects` forwarding members. Do not add wrappers. In `Witnessed`, bind `Option<double> lastError = run.Cursor.History.Map(static h => h.Error)` and read `run.Cursor.State`, `.Steps`, `.Time`, and `.Rejects` directly; this composes the run's documented “cursor whole” shape and prevents the history repair from minting another forwarding surface. Replace the stale `TrajectoryTerminal` comparisons with the actual `TerminalDisposition` union: divergence is `run.Terminal is TerminalDisposition.Divergent`, and `Verdict` exhaustively `Switch`es `Converged`, `Relaxable` (only `RelaxAxis.Steps` maps to `Convergence.Exhausted`), and `Divergent`. Delete the existing `TrajectoryRun.Final => Cursor.State` forwarding property after its route call changes to `run.Cursor.State`.
- `Rasm/.planning/Processing/flow.md`: add `Option<StepHistory> History` to `StreamlineState`, initialize it to `None`, pass it into `Step`, and mint history in `Advance` from the error and `suggested / H`. Delete the parallel `LastError` state column; `ToTrace` projects `History.Map(static h => h.Error)`, while `MaxError` continues accumulating from the same error. The accepted arm passes `accepted.Error`; the rejected arm passes `Some(rejected.Error)`.
- Update both driver cards to say the driver mints, stores, and threads history while the stateless stepper returns error plus a suggestion. `FieldIntegrator.Admit` remains: its two boundary callers use the named owner gate, and deleting it merely duplicates `Optional(...).ToFin(...)` outside the owner without reducing repository LOC.

The two state replacements are exact, not “add another history field”:

```csharp
// Rasm.Compute: From
Option<double> Error, StepHistory History, Seq<TrajectorySample<TState>> Samples, int Station);
// To
Option<StepHistory> History, Seq<TrajectorySample<TState>> Samples, int Station);

// accepted
double remaining = spec.Horizon - (cursor.Time + step);
double proposed = Math.Min(accepted.SuggestedStep, control.MaxStep);
double nextStep = remaining > control.MinStep ? Math.Min(proposed, remaining) : proposed;
History = accepted.Error.Map(error => new StepHistory(error, nextStep / step)),
// rejected
double remaining = spec.Horizon - cursor.Time;
double nextStep = Math.Min(Math.Min(rejected.SuggestedStep, control.MaxStep), remaining);
History = Some(new StepHistory(rejected.Error, nextStep / step)),
```

```csharp
// Processing: From
double MinStep, double MaxStep, Option<double> LastError, double MaxError,
// To
double MinStep, double MaxStep, Option<StepHistory> History, double MaxError,

// accepted / rejected transition arguments
Advance(suggested: accepted.SuggestedStep, error: accepted.Error)
Advance(suggested: rejected.SuggestedStep, error: Some(rejected.Error))

private StreamlineState Advance(double suggested, Option<double> error) {
    Option<StepHistory> history = error.Map(value => new StepHistory(value, suggested / H));
    return this with {
        H = suggested, MinStep = Math.Min(MinStep, suggested), MaxStep = Math.Max(MaxStep, suggested),
        History = history, MaxError = Math.Max(MaxError, error.IfNone(0.0)),
    };
}
```

Align the Processing driver's rejection bound with Compute while touching that transition. `RejectBudget` is the number of rejections permitted; a zero budget stops on the first rejection, and a budget of three stops on the fourth. Replace `Rejects + 1 >= rejectBudget` with `Rejects + 1 > rejectBudget`. Compute already encodes the same law as `next.Streak <= spec.Integrator.RejectBudget` on the continuing arm.

## 6. Delete `ConvergenceClaim`; error-estimate presence is the evidence

`ConvergenceClaim.Estimated` occurs exactly when `Error` is `Some`, and `Unwitnessed` exactly when it is `None`. The payload-free smart enum is a hand-maintained mirror that can contradict the evidence it claims to classify.

**Location:** same file, anchors `ConvergenceClaim`, `KernelOutcome`, `QuadratureEvidence`, route row constructions, and `Quadrature.Admit`.

**From:**

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ConvergenceClaim {
    public static readonly ConvergenceClaim Estimated = new("estimated");
    public static readonly ConvergenceClaim Unwitnessed = new("unwitnessed");
}

public readonly record struct KernelOutcome(double Value, Option<double> Error, Option<double> L1Norm, ConvergenceClaim Claim);
```

**To:**

```csharp
```

Do not rename `KernelOutcome` into another module-level carrier. It is a transient uniform return shape for the route delegate and private quadrature operations; use the named tuple `(double Value, Option<double> Error, Option<double> L1Norm)` only at the internal delegate, `Quadrature.Admit`, and nested Smolyak signatures. The tuple never crosses `Quadrature.Integrate`; `QuadratureEvidence` remains the public named result class.

**From:**

```csharp
public sealed record QuadratureEvidence(double Value, Option<double> Error, Option<double> L1Norm, Option<double> Ratio, int Skipped, ConvergenceClaim Claim);
```

**To:**

```csharp
public sealed record QuadratureEvidence(
    double Value, Option<double> Error, Option<double> L1Norm, Option<double> Ratio, int Skipped);
```

Remove every `Claim:` constructor argument. Replace:

```csharp
ctl.RequireErrorWitness && outcome.Claim.Equals(ConvergenceClaim.Unwitnessed)
```

with:

```csharp
ctl.RequireErrorWitness && !outcome.Error.IsSome
```

and remove `Claim: outcome.Claim` from both evidence constructions.

Nest the implementation-only sparse-grid algorithm under its sole caller while changing that return shape:

**From:**

```csharp
public static class SmolyakCubature {
    public static KernelOutcome Integrate(
        Func<double[], double> f, Arr<IntervalSpec> bounds, int level) {
```

**To:** move the body unchanged inside `Quadrature`, before its closing brace:

```csharp
private static class Smolyak {
    private static (double Value, Option<double> Error, Option<double> L1Norm) Integrate(
        Func<double[], double> f, Arr<IntervalSpec> bounds, int level) {
```

Change the sole call from `SmolyakCubature.Integrate(...)` to `Smolyak.Integrate(...)`. The containing `Quadrature` owner can reach the nested type's private method; no other fence names the algorithm owner.

**Effect:** target fenced LOC approximately `12 -> 3` (`-9`); module-level types `-3` (`ConvergenceClaim` and `KernelOutcome` deleted, `SmolyakCubature` nested); public types `-3`; public smart-enum rows `-2`; public result columns `-1`; contradictory states removed `2` (`Estimated + None`, `Unwitnessed + Some`). `QuadratureEvidence` retains its existing reference/value behavior; only its redundant claim column disappears.

**API/consumer proof:** `QuadratureRoute.GaussKronrod` is the only row that receives MathNet's `out error`; every other route constructs `Error: None`. The MathNet catalogue proves it is the sole error-estimate channel. LanguageExt `Option` is already the honest absence carrier. The private named tuple is scoped to implementation transit, while the retained `QuadratureEvidence` class remains the result consumed by Compute and solver contracts. Repository-wide inspection finds no `SmolyakCubature` consumer outside this target; its only call is the `IntegrationDomain.SparseGrid` arm.

**Ripples:** in `Rasm.Compute/.planning/Tensor/quadrature.md`, change `Evidence.Count(row => row.Claim == ConvergenceClaim.Unwitnessed)` to `Evidence.Count(static row => !row.Error.IsSome)` and update its cards/prose. In `Rasm.Compute/.planning/Solver/route.md`, reuse move 5's local `lastError`, delete `claim`, gate directly on `!lastError.IsSome && row.Accuracy.RequireErrorWitness`, and remove `Claim: claim` from the constructed `QuadratureEvidence`. No wire shape exists yet; this is a spec-sheet break with every caller updated same pass.

## 7. Let Thinktecture own the quadrature-route delegate

`QuadratureRoute` manually stores and forwards a delegate even though the admitted package generates exactly this column from `[UseDelegateFromConstructor]`. Name the operation for what it does and return move 6's named estimate tuple; no record is earned for a private three-slot transit shape.

**Location:** same file, anchors the three `QuadratureRoute` row `kernel:` arguments, private `kernel` field, and `Run`.

**From:**

```csharp
private readonly Func<Func<double, double>, double, double, QuadratureControl, KernelOutcome> kernel;

public bool InfiniteBounds { get; }

public KernelOutcome Run(Func<double, double> f, double lower, double upper, QuadratureControl control) => kernel(f, lower, upper, control);
```

**To:**

```csharp
public bool InfiniteBounds { get; }

[UseDelegateFromConstructor]
internal partial (double Value, Option<double> Error, Option<double> L1Norm) Evaluate(
    Func<double, double> integrand, double lower, double upper, QuadratureControl control);
```

Rename each constructor argument `kernel:` to `evaluate:` and the sole `l.Route.Run(...)` call to `l.Route.Evaluate(...)`. Replace each `new KernelOutcome(...)` with a target-typed named tuple, spelling absent channels as `Option<double>.None`; move 6 has already changed nested `Smolyak.Integrate` and `Quadrature.Admit` to the same tuple signature.

**Effect:** authored fenced LOC `5 -> 4` (`-1`); private authored fields `-1`; public methods `-1`; hand-written forwarding bodies `-1`; behavior delegates remain one generated field per row.

**API/consumer proof:** `api-thinktecture-runtime-extensions.md` proves `[UseDelegateFromConstructor]` emits a ctor-supplied partial method, with delegate parameters ordered by partial-method declaration. This fence already uses the same generator surface correctly on `StepLaw.Rescale`.

**Ripples:** remove `KernelOutcome` from the local quadrature card. No external consumer names the deleted type or the private tuple.

## 8. Make quadrature tables implementation-only and normalize ladders at construction

`ReferenceElement.Rule` is the public election. Exposing all seventeen table fields lets consumers bypass the typed ladder-exhaustion proof; `Rasm.Compute/Analysis/frame.md` does so today. `QuadratureRule.Dimension` has no reader anywhere in `libs/dotnet/` and duplicates the owning `ReferenceElement` row. Ladder ordering belongs in Thinktecture's generated constructor hook as normalization, not in a cross-roster lazy plus a private proof helper executed on every first `Rule` reach. Exhaustion, including an accidentally empty row, remains the typed `Rule` refusal; the constructor hook must not throw and bypass the result carrier.

**Location:** same file, anchor `public readonly record struct QuadratureRule` and its static rows.

**From:**

```csharp
public readonly record struct QuadratureRule(int Order, int Dimension, ImmutableArray<(double X, double Y, double Z, double Weight)> Points) {
    public static readonly QuadratureRule Line2 = new(2, 1, [.. Gauss2.Select(static g => (g.Node, 0.0, 0.0, g.Weight))]);
    public static readonly QuadratureRule Line3 = new(3, 1, [.. Gauss3.Select(static g => (g.Node, 0.0, 0.0, g.Weight))]);
```

**To:**

```csharp
public readonly record struct QuadratureRule(
    int Order, ImmutableArray<(double X, double Y, double Z, double Weight)> Points) {
    internal static readonly QuadratureRule Line2 = new(2, [.. Gauss2.Select(static g => (g.Node, 0.0, 0.0, g.Weight))]);
    internal static readonly QuadratureRule Line3 = new(3, [.. Gauss3.Select(static g => (g.Node, 0.0, 0.0, g.Weight))]);
```

Apply `internal` to every remaining table row and remove the middle dimension argument from each construction and builder return. Keep `Order` and `Points` public because the elected proof is what `DiscreteMesh` and assembly consume.

Delete the ladder-ceiling property at the same owner. It has one read inside `Rule`, so narrowing it to a private member would preserve a single-use extraction:

```csharp
// From
public int Ceiling => rules[^1].Order;
// To
// delete; Rule owns the total local below
```

Replace the manual lazy proof:

```csharp
_ = Monotone.Value;
// ...
private static readonly Lazy<Unit> Monotone = new(ProveAscending, LazyThreadSafetyMode.ExecutionAndPublication);
static Unit ProveAscending() { /* Items loop */ }
```

with the row-construction normalization the admitted package owns:

```csharp
static partial void ValidateConstructorArguments(
    ref string key, ref ImmutableArray<QuadratureRule> rules) =>
    rules = rules.IsDefaultOrEmpty ? [] : [.. rules
        .OrderBy(static rule => rule.Order)
        .DistinctBy(static rule => rule.Order)];
```

Then `Rule` starts directly with a total ceiling and positive-order election:

```csharp
int ceiling = rules.IsDefaultOrEmpty ? 0 : rules[^1].Order;
return toSeq(rules).Find(rule => order > 0 && rule.Order >= order).ToFin(
    new KernelFault.OutOfRange(
        Label: $"reference-rule:{Key}", Scalar: order,
        Requirement: $"a positive order with an owned rule at or above it, ceiling {ceiling}", Key: key));
```

The generated constructor sorts each committed roster once and removes a later same-exactness rung that the order-only election could never select; the local ceiling is total over an empty/default array; and a nonpositive or missing requested order, whether from an empty roster or a finite ceiling, remains a typed `KernelFault.OutOfRange`. The current public `Rule(0, ...)` otherwise elects the first row despite zero not being an integration order. No constructor-time exception crosses above the `Fin` boundary.

**Effect:** target fenced LOC approximately `14 -> 4` (`-10`) beyond the table access changes; public members `-19` (the 17 row fields, `Dimension`, and `Ceiling`); declared members `-3` net (`Dimension`, the lazy field, proof helper, and `Ceiling` delete while one generated hook replaces them); public bypasses around `ReferenceElement.Rule` `17 -> 0`; duplicated dimension facts `17 -> 0`.

**API/consumer proof:** repository-wide consumer inspection finds no `QuadratureRule.Dimension` read. The only rule-field bypass is `QuadratureRule.Line2` in `Rasm.Compute/.planning/Analysis/frame.md`; every solver-element consumer already reads a `Fin<QuadratureRule>` from `ElementClass.Quadrature`, exactly as `Rasm.Compute/RULINGS.md` requires. The Thinktecture catalogue proves `ValidateConstructorArguments(ref key, ref columns...)` covers the smart-enum key and plain columns on every construction path; `docs/stacks/csharp/surfaces-and-dispatch.md [CONSTRUCTION_ADVICE]` forbids rejection logic or throwing in that hook. Sort-and-distinct is safe ref-normalization because `Rule` discriminates only by exactness, while every nonpositive or exhausted request still receives the typed `Fin` refusal.

**Ripples:** in `Analysis/frame.md`, bind `model.Policy.Formulation.Quadrature` before constructing `DiscreteMesh` and pass the admitted `rule` instead of `QuadratureRule.Line2`. Update its Packages row. In the target, replace the accessor-forced lazy-proof claim with constructor-time sort-and-distinct normalization. In the target and `Rasm.Compute/.planning/Tensor/quadrature.md`, replace claims that refusal reads the `Ceiling` member with the exact behavior: `Rule` computes the normalized ladder's terminal order locally and carries that value in the typed refusal. `Solver/element.md`'s lowercase conceptual “ceiling” remains true and names no deleted member. No other consumer fence changes.

## 9. Correct the exactness carried by the line and conical rows

The `Order` column is polynomial exactness, not node count: `TensorCube` already computes `2n-1`, and `ReferenceElement.Rule` compares the requested integration order against it. The two line rows currently store node count (`2`, `3`). The conical builder stores `n-1` (`1`, `2`) without deriving either leg, and both pyramid rows reuse the same three-point height rule. After the `(1-z)^2` Jacobian factor, an `m`-point Gauss-Legendre height leg supports pyramid polynomial degree `2m-3`; therefore both current conical constructions have height ceiling three. The alleged second rung raises only the base leg and earns no higher exactness, exactly the form `Rasm/RULINGS.md` rejects. The first construction also contains twelve points, so `Pyramid5` is objectively false terminology.

**Location:** same file, anchors `Line2`, `Line3`, `Pyramid5`, `Pyramid27`, `Conical`, and `ReferenceElement.Pyramid`.

**From:**

```csharp
internal static readonly QuadratureRule Line2 = new(2, /* points unchanged */);
internal static readonly QuadratureRule Line3 = new(3, /* points unchanged */);
```

**To:**

```csharp
internal static readonly QuadratureRule Line2 = new(3, /* points unchanged */);
internal static readonly QuadratureRule Line3 = new(5, /* points unchanged */);
```

**From:**

```csharp
internal static readonly QuadratureRule Pyramid5 = Conical(n: 2);
internal static readonly QuadratureRule Pyramid27 = Conical(n: 3);
```

**To:**

```csharp
internal static readonly QuadratureRule Pyramid12 = Conical(baseLine: Gauss2, heightLine: Gauss3);
```

**From:**

```csharp
private static QuadratureRule Conical(int n) {
    ImmutableArray<(double Node, double Weight)> baseLine = n == 2 ? Gauss2 : Gauss3;
    (double Node, double Weight)[] zeta = [.. Gauss3.Select(static g => (Node: (g.Node + 1.0) * 0.5, Weight: g.Weight * 0.5))];
```

**To:**

```csharp
private static QuadratureRule Conical(
    ImmutableArray<(double Node, double Weight)> baseLine,
    ImmutableArray<(double Node, double Weight)> heightLine) {
    (double Node, double Weight)[] zeta = [.. heightLine.Select(static g =>
        (Node: (g.Node + 1.0) * 0.5, Weight: g.Weight * 0.5))];
```

**From:**

```csharp
return new(Math.Max(val1: 1, val2: baseLine.Length - 1), [.. rows]);
```

**To:**

```csharp
int baseOrder = (2 * baseLine.Length) - 1;
int heightOrder = (2 * heightLine.Length) - 3;
return new(Math.Min(baseOrder, heightOrder), [.. rows]);
```

Update `ReferenceElement.Pyramid` to `[QuadratureRule.Pyramid12]`; delete `Pyramid27` whole.

**Effect:** fenced LOC approximately unchanged after spelling both exactness legs; magic integer mode knobs `-1`; false names `-1`; redundant rule rows `-1`; line ceiling `3 -> 5`; pyramid ceiling `2 -> 3`; conical tables `2 -> 1`; conical points materialized at type initialization `39 -> 12`.

**API/consumer proof:** Gauss-Legendre with `n` nodes is exact through degree `2n-1`; the existing `TensorCube` and `PrismProduct` code already encode that rule. Under the conical map, the height integrand includes the degree-two Jacobian factor, leaving exact pyramid degree `2m-3` on an `m`-point height leg. `Rasm/RULINGS.md` fixes prism and conical exactness at the weaker leg. Both current rows use `Gauss3` vertically, so raising the base from `Gauss2` to `Gauss3` does not raise the degree-three minimum and cannot survive as a second rung.

**Ripples:** update the target card and change the density row from `7·17` to `7·16` (the reference-domain roster now owns sixteen rules); in `Rasm.Compute/.planning/Tensor/quadrature.md`, change the stated line ceiling `3` to `5` and pyramid ceiling `2` to `3`. `ElementClass.Pyramid5` requests order two and continues electing the sole row. A future degree-five pyramid rung must raise the height leg to at least `Gauss4`; it is not part of this refinement pass.

## 10. Compose MathNet's binomial function and delete the private duplicate

**Location:** same file, anchors `CombinationLevels` and `private static double Binomial`.

**From:**

```csharp
int coefficient = (((q - total) & 1) == 0 ? 1 : -1) * (int)Binomial(n: dimensions - 1, k: q - total);
```

**To:**

```csharp
int coefficient = (((q - total) & 1) == 0 ? 1 : -1)
    * (int)SpecialFunctions.Binomial(dimensions - 1, q - total);
```

**From:**

```csharp
private static double Binomial(int n, int k) =>
    k < 0 || k > n ? 0.0 : Enumerable.Range(start: 0, count: k).Aggregate(seed: 1.0, func: (acc, i) => acc * (n - i) / (i + 1));
```

**To:**

```csharp
```

**Effect:** fenced LOC `3 -> 2` (`-1`); private members `-1`; hand-rolled combinatoric folds `-1`.

**API/consumer proof:** `api-mathnet-numerics.md` includes `Binomial` in the `SpecialFunctions` combinatoric roster, while the pinned MathNet.Numerics `6.0.0-beta2` `net8.0` XML supplies the exact callable signature `SpecialFunctions.Binomial(int, int)`. The fence already imports `MathNet.Numerics`. `CombinationLevels` is the sole caller, and its arguments are nonnegative with `k <= n` by loop construction.

**Ripples:** add the exact `SpecialFunctions.Binomial(int, int)` surface to `libs/dotnet/.api/api-mathnet-numerics.md`, then add `SpecialFunctions.Binomial` to `[05]-[QUADRATURE]` Packages/Auto. No consumer changes.

## 11. Delete the constant infinite-bound column and admit every MathNet line route

The finite-only claim conflates MathNet's public `Integrate.*` facades with the lower direct kernels named in the C# algorithm warning. The shipped 6.0.0-beta2 XML for all three public entries — `DoubleExponential`, `GaussLegendre`, and both `GaussKronrod` overloads — explicitly admits one or both infinite limits and performs the substitution. The current `GaussKronrod` row is therefore the false capability value. Once corrected, all three rows answer `true`, so the column and its gate cannot discriminate and must disappear.

**Location:** same file, anchors the three `QuadratureRoute` rows, `InfiniteBounds`, and the `IntegrationDomain.Line` arm.

**From:**

```csharp
public static readonly QuadratureRoute DoubleExponential = new("double-exponential", infiniteBounds: true,
    evaluate: /* public MathNet Integrate call */);
public static readonly QuadratureRoute GaussLegendre = new("gauss-legendre", infiniteBounds: true,
    evaluate: /* public MathNet Integrate call */);
public static readonly QuadratureRoute GaussKronrod = new("gauss-kronrod", infiniteBounds: false,
    evaluate: /* public MathNet Integrate call */);

public bool InfiniteBounds { get; }
```

**To:**

```csharp
public static readonly QuadratureRoute DoubleExponential = new("double-exponential",
    evaluate: /* public MathNet Integrate call */);
public static readonly QuadratureRoute GaussLegendre = new("gauss-legendre",
    evaluate: /* public MathNet Integrate call */);
public static readonly QuadratureRoute GaussKronrod = new("gauss-kronrod",
    evaluate: /* public MathNet Integrate call */);
```

Move 7 has already renamed the delegate parameter `evaluate:`.

**From:**

```csharp
line: l => !Ordered(bounds: l.Bounds)
    ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(
        Label: nameof(IntegrationDomain.Line), Requirement: NaNFreeAscending, Key: Some(op)))
    : l.Bounds.Infinite && !l.Route.InfiniteBounds
        ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(
            Label: l.Route.Key, Requirement: "a route carrying InfiniteBounds", Key: Some(op)))
        : Counted(/* route evaluation */),
```

**To:**

```csharp
line: l => !(l.Bounds.Lower < l.Bounds.Upper)
    ? Fin.Fail<QuadratureEvidence>(new KernelFault.InvalidValue(
        Label: nameof(IntegrationDomain.Line),
        Requirement: "a NaN-free ascending interval",
        Key: Some(op)))
    : Counted(/* route evaluation */),
```

Inline and delete the single-use `NaNFreeAscending` constant. The negated `<` comparison is deliberate: signed infinities preserve their mathematical ordering, while either NaN makes `<` false and therefore enters the refusal arm.

After the line gate disappears, `IntervalSpec.Infinite` has one remaining read inside `FiniteOrdered`; delete that forwarding property too and seat the finite predicate at its owner:

```csharp
// From
public bool Infinite => double.IsInfinity(Lower) || double.IsInfinity(Upper);
private static bool Ordered(IntervalSpec bounds) =>
    !double.IsNaN(bounds.Lower) && !double.IsNaN(bounds.Upper) && bounds.Lower < bounds.Upper;
private static bool FiniteOrdered(IntervalSpec bounds) => !bounds.Infinite && Ordered(bounds);

// To
private static bool FiniteOrdered(IntervalSpec bounds) =>
    double.IsFinite(bounds.Lower) && double.IsFinite(bounds.Upper) && bounds.Lower < bounds.Upper;
```

**Effect:** fenced LOC approximately `15 -> 7` (`-8`); public members `-2`; declared members `-4` (`QuadratureRoute.InfiniteBounds`, `IntervalSpec.Infinite`, `NaNFreeAscending`, and `Ordered`); constructor columns `-1`; false capability values `-1`; single-use constants `-1`; no route-key switch or replacement vocabulary added.

**API/consumer proof:** `Directory.Packages.props` pins MathNet.Numerics `6.0.0-beta2`, and that installed package's `net8.0` XML states the same “either or both limits” infinite-limit contract on `Integrate.DoubleExponential`, `Integrate.GaussLegendre`, and both `Integrate.GaussKronrod` overloads. `docs/stacks/csharp/algorithms.md` requires substitution at the facade entry and rejects passing infinity to the lower direct kernel; this code calls those public facades. Every finite consumer remains unchanged, and `GaussKronrod` now truthfully admits the same unbounded line domain as its siblings.

**Ripples:** in `api-mathnet-numerics.md` `[ENTRYPOINT_SCOPE]: quadrature via Integrate`, amend rows `[03]`–`[05]` so each capability explicitly says “public facade; either or both limits may be infinite through the facade substitution”; keep `[IMPLEMENTATION_LAW]`'s direct-kernel warning and add that it does not apply to these three facade entries. Update the target/Compute prose to say the line domain owns that capability structurally. Rectangle, cuboid, and sparse-grid bounds continue to refuse non-finite endpoints through the inlined `FiniteOrdered` predicate.

## 12. Replace generic or misleading owner names with established numerical terminology

Apply these exact breaking renames last; do not add aliases.

### 12a. Runge-Kutta owners and controller terms

**Location:** target declarations and every direct consumer across `libs/dotnet/`.

| From | To | Reason |
| :--- | :--- | :--- |
| `IntegratorKind` | `RungeKuttaMethod` | each row is a named Butcher method, not an unspecified “kind” |
| `FieldIntegrator` | `RungeKuttaIntegrator` | the owner integrates scalars, complex values, springs, fields, and geometric state; “field” is false narrowing |
| `RungeKuttaIntegrator.Kind` | `Method` | after the owner rename the column is the selected numerical method, not a generic kind |
| `StepLaw` | `StepController` | I, PI, and Gustafsson are adaptive step-size controllers in the numerical literature |
| `StepLaw.Elementary` | `StepController.Integral` | the row uses current error only: the standard I-controller |
| `StepLaw.Proportional` | `StepController.ProportionalIntegral` | the row uses current and previous error: the standard PI-controller, not a proportional-only law |
| `StepControl.Law` | `Controller` | the policy column names the controller it carries |
| `DenseOutputCoefficientFamily` | `DenseFormula` | each row is one continuous-extension coefficient formula; “family” adds no axis |
| `ButcherDenseOutput` | `DenseOutput` | dense output is the established Runge-Kutta term; the owner already sits beside the Butcher tableau |
| `DenseOutputInterpolant` | `DenseInterpolant` | the nested value is the admitted dense interpolant, not another output owner |

Make the renamed controller roster keyless in the same replacement:

```csharp
[SmartEnum]
public sealed partial class StepController {
    public static readonly StepController Integral = new(rescale: /* existing current-error body */);
    public static readonly StepController ProportionalIntegral = new(rescale: /* existing PI body */);
    public static readonly StepController Gustafsson = new(rescale: /* existing Gustafsson body */);
```

Delete `[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]` and the three string key arguments. No target or consumer reads, looks up, serializes, persists, or reports a controller key; `StepControl` carries the row object itself, so keyed generation is unearned surface.

Representative exact declaration replacement:

**From:**

```csharp
[SmartEnum<int>]
public sealed partial class IntegratorKind {
```

**To:**

```csharp
[SmartEnum<int>]
public sealed partial class RungeKuttaMethod {
```

**From:**

```csharp
[Union]
public abstract partial record FieldIntegrator {
```

**To:**

```csharp
[Union]
public abstract partial record RungeKuttaIntegrator {
```

Update every `c.Kind`, `active.Kind`, `.Integrator.Kind`, `DenseFamily`, and prose occurrence coherently; the target properties become `RungeKuttaIntegrator.Method` and `RungeKuttaMethod.Formula`.

### 12b. Quadrature input shapes

| From | To | Reason |
| :--- | :--- | :--- |
| `IntegrationDomain` | `QuadratureDomain` | the union carries quadrature integrands and rule controls, never an ODE integration domain |
| `IntervalSpec` | `IntegrationInterval` | “Spec” adds no domain meaning; the value is the integration interval |
| `QuadratureDomain.Simplex` | `Reference` | the case admits line, quadrilateral, hexahedron, wedge, and pyramid rows as well as simplices |

**Effect:** fenced LOC `-1` from the removed comparer attribute; misleading/coined names `-13`; compatibility symbols `0`; one unused keyed lookup/conversion/serialization surface disappears.

**API/consumer proof:** `IntegrationModule<TState,TDelta>` and `IntegrationStep<TState,TDelta>` remain generic mathematical owners. The renamed method/integrator pair is specifically Runge-Kutta by its validated Butcher tableau. Hairer-Wanner's established controller terminology matches the implemented current-error, current-plus-previous-error, and Gustafsson formulas. The renamed domain union is consumed only by `Quadrature.Integrate`, and `Reference` is the only case name valid for all seven `ReferenceElement` rows. No external package API constrains these Rasm-authored spellings.

**Ripples:** repository-wide exact-name inspection yields this closed path set; update every occurrence in it and no speculative README/ruling surface:

- `libs/dotnet/Rasm/.planning/Numerics/integrate.md` and `libs/dotnet/Rasm/ARCHITECTURE.md`;
- `libs/dotnet/Rasm/.planning/Parametric/nurbs.md`, `projections.md`, and `surface.md`;
- `libs/dotnet/Rasm/.planning/Processing/extract.md`, `flow.md`, and `intent.md`;
- `libs/dotnet/Rasm.Compute/.planning/Tensor/quadrature.md`, `Solver/contract.md`, `Solver/route.md`, and `libs/dotnet/Rasm.Compute/ARCHITECTURE.md`;
- `libs/dotnet/Rasm.Rhino/.planning/Viewport/motion.md` and `operations.md`.

Use no forwarding type, obsolete alias, or compatibility factory. Update every fence and prose owner in one landing pass.

## Ordered result

- Fenced LOC: at least approximately `-68` in the target before line wrapping from the final renames; the two driver-history ripples add a few lines while closing code that cannot transcribe as written. This conservative total includes the explicit `Fin.Succ`/`Fin.Fail` spellings and the `DenseOutputSpan` accessibility repair required by the internalizations above.
- Module-level types: `-6` — `DenseOutputSource`, `ConvergenceClaim`, and `KernelOutcome` delete; `RootedTree`, `OrderConditions`, and `SmolyakCubature` nest under their sole owner. `ButcherTableau` also ceases to be public.
- Declared members: at least `-24`, including the duplicated tableau route, two split embedded columns/projections, `MomentSum`, `Raise`, the tableau/dense forwarding shells, two dense-evidence mirrors, `QuadratureRule.Dimension`, the route delegate field, the ladder lazy, `Binomial`, and `InfiniteBounds`.
- Publicly reachable members: at least `-40`, dominated by the now-internal quadrature tables, deleted `ReferenceElement.Ceiling`, nested Smolyak implementation, the `DenseOutputSpan` reduction from nine public columns to one, deleted mirror vocabularies, and tableau internals.
- Duplicate/hand-rolled/forwarding bodies: `-7` (verified-order walk, power loop, one-use moment wrapper, two dense forwarding shells, route forwarding, binomial fold).
- Impossible or contradictory states removed: half-present embedded formula; split error/scale history; published/fitted source disagreement; estimated/unwitnessed claim disagreement; a supposedly finite-only public MathNet facade.
- Consumer ripples: all exact and enumerated above; no compatibility residue.

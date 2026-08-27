# 1. Delete the packed-index forwarding surface

**From**

`libs/dotnet/Rasm/.planning/Solving/solver.md:363`

```csharp
public static class Lm {
    internal static int PackedIndex(int n, int i, int j) => SymmetricMatrix.FlatIndex(n: n, i: i, j: j);
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:413`

```csharp
int di = PackedIndex(dof, i, i);
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:526`

```csharp
normal[Lm.PackedIndex(n: n, i: a, j: b)] += partials[a] * partials[b];
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:1032`

```csharp
normal[Lm.PackedIndex(n, li, lj)] += pi * pj;
```

**To**

```csharp
// Lm.PackedIndex DELETED
```

```csharp
int di = SymmetricMatrix.FlatIndex(dof, i, i);
```

```csharp
normal[SymmetricMatrix.FlatIndex(n, a, b)] += partials[a] * partials[b];
```

```csharp
normal[SymmetricMatrix.FlatIndex(n, li, lj)] += pi * pj;
```

**Why**

`Lm.PackedIndex` only renames the canonical packed-upper address owner. It adds a module member and a second name for one layout without adding domain behavior.

**Change**

Delete `Lm.PackedIndex`, call `SymmetricMatrix.FlatIndex` directly at every diagonal and scatter site, and replace every target-page prose claim that names the forwarding member with the canonical owner.

**Delta**

Code-fence LOC: -1. Module-level members: 1 removed, 0 added, net -1. Module-level types: 0 removed, 0 added, net 0. Net declared symbols: -1.

**Ripples**

Replace `Lm.PackedIndex` with `SymmetricMatrix.FlatIndex` in `libs/dotnet/Rasm/.planning/Solving/fit.md:683` and `libs/dotnet/Rasm/.planning/Processing/register.md:643`; update the corresponding package/card text in `Solving/fit.md`, `Domain/stats.md`, and `Numerics/matrix.md` so none names the deleted indirection.

# 2. Inline the one-use lambda transitions

**From**

`libs/dotnet/Rasm/.planning/Solving/solver.md:108`

```csharp
internal double Lower(double lambda) => double.Max(lambda / LambdaDown, LambdaFloor);
internal double Raise(double lambda) => lambda * LambdaUp;
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:441`

```csharp
: new LmPass.Running(State: moved with { Lambda = policy.Lower(state.Lambda) }),
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:449`

```csharp
new LmPass.Running(State: state with { Lambda = policy.Raise(state.Lambda), Normal = Some(normal) });
```

**To**

```csharp
// SolvePolicy.Lower DELETED
// SolvePolicy.Raise DELETED
```

```csharp
: new LmPass.Running(State: moved with {
    Lambda = double.Max(state.Lambda / policy.LambdaDown, policy.LambdaFloor),
}),
```

```csharp
new LmPass.Running(State: state with { Lambda = state.Lambda * policy.LambdaUp, Normal = Some(normal) });
```

**Why**

Each method owns one arithmetic expression and one call site. The policy already exposes the operands, so neither forwarding member preserves an independent transition invariant.

**Change**

Inline the clamped division and multiplication into their state transitions and delete both policy methods.

**Delta**

Code-fence LOC: -2. Type members: 2 removed, 0 added, net -2. Module-level types: 0 removed, 0 added, net 0. Net declared symbols: -2.

# 3. Replace bespoke iteration cases with the native recursion carrier

**From**

`libs/dotnet/Rasm/.planning/Solving/solver.md:112`

```csharp
readonly record struct LmNormal(double[] Packed, double[] Gradient);

readonly record struct LmState(double[] Parameters, ddouble Norm, double Lambda, int Iterations, Option<LmNormal> Normal);

[Union]
abstract partial record LmPass {
    private LmPass() { }
    public sealed record Running(LmState State) : LmPass;
    public sealed record Settled(LmState State, SolveStatus Status) : LmPass;
}
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:385`

```csharp
static Fin<LmResult> Iterate(ILmModel model, int dof, SolvePolicy policy, LmState seed, Op key) =>
    IO.pure(value: unit).FoldUntil(
            schedule: Schedule.recurs(times: policy.MaxIterations.Value - 1),
            initialState: Fin.Succ<LmPass>(new LmPass.Running(State: seed)),
            folder: (acc, _) => acc.Bind(active => active.Switch(
                state: (Model: model, Dof: dof, Policy: policy, Key: key),
                running: static (s, live) => Pass(model: s.Model, dof: s.Dof, policy: s.Policy, state: live.State, key: s.Key),
                settled: static (_, done) => Fin.Succ<LmPass>(done))),
            stateIs: static state => state.Match(Succ: static pass => pass is LmPass.Settled, Fail: static _ => true))
        .Run()
        .Bind(pass => pass.Switch(
            state: key,
            running: static (op, live) => Result(live.State, SolveStatus.Exhausted, op),
            settled: static (op, done) => Result(done.State, done.Status, op)));
```

**To**

```csharp
readonly record struct LmState(
    double[] Parameters,
    ddouble Norm,
    double Lambda,
    int Iterations,
    Option<(double[] Packed, double[] Gradient)> Normal);

// LmNormal DELETED
// LmPass DELETED
```

```csharp
static Fin<LmResult> Iterate(ILmModel model, int dof, SolvePolicy policy, LmState seed, Op key) =>
    IO.pure(unit).FoldUntil(
            schedule: Schedule.recurs(policy.MaxIterations.Value - 1),
            initialState: Fin.Succ(Next.Loop<LmState, (LmState State, SolveStatus Status)>(seed)),
            folder: (acc, _) => acc.Bind(next => next.Match(
                state => Pass(model, dof, policy, state, key),
                _ => Fin.Succ(next))),
            stateIs: static state => state.Match(
                Succ: static next => next.IsDone,
                Fail: static _ => true))
        .Run()
        .Bind(next => next.Match(
            state => Result(state, SolveStatus.Exhausted, key),
            done => Result(done.State, done.Status, key)));
```

**Why**

`LmNormal` is a behavior-free pair, while `LmPass` hand-rolls the loop/done shape already supplied by LanguageExt `Next<A, B>`. Putting an optional status on `LmState` weakens that shape by permitting terminal status and mutable iteration state to coexist independently; `Next` removes the bespoke union without creating that invalid combination.

**Change**

Store the cached normal pair as a named tuple; make `Pass`, `Trial`, `Accept`, and `Singular` return `Fin<Next<LmState, (LmState State, SolveStatus Status)>>`; make `Descend` and `Reject` return that `Next` directly; emit `Next.Done` for convergence or stationarity and `Next.Loop` for another trial. Keep `Schedule.recurs` as the sole budget owner and map a surviving loop to `Exhausted` only after the fold ends.

**Delta**

Code-fence LOC: -6. Module-level types: 2 removed, 0 added, net -2. Nested types: 2 removed, 0 added, net -2. Record properties: 5 removed, 0 added, net -5. Net declared symbols: -9.

# 4. Read the linear solution at its factorization site

**From**

`libs/dotnet/Rasm/.planning/Solving/solver.md:421`

```csharp
return Solved(
    solve: SymmetricMatrix.Of(Dimension.Create(dof), new Arr<double>(damped), key)
        .Bind(spd => spd.DecomposeCholesky(key))
        .Bind(chol => chol.SolveDetailed(new Arr<double>(rhs), key)),
    key: key)
    .Match(
        Succ: delta => Accept(model: model, policy: policy, state: state, normal: normal, delta: delta, key: key),
        Fail: _ => Fin.Succ(Reject(policy: policy, state: state, normal: normal)));
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:431`

```csharp
static Fin<Arr<double>> Solved(Fin<LinearSolution> solve, Op key) =>
    solve.Bind(solved => solved.IsValid ? Fin.Succ(solved.Solution) : Fin.Fail<Arr<double>>(key.InvalidResult()));
```

**To**

```csharp
return SymmetricMatrix.Of(Dimension.Create(dof), new Arr<double>(damped), key)
    .Bind(spd => spd.DecomposeCholesky(key))
    .Bind(chol => chol.SolveDetailed(new Arr<double>(rhs), key))
    .Bind(solved => solved.IsValid
        ? Fin.Succ(solved.Solution)
        : Fin.Fail<Arr<double>>(key.InvalidResult()))
    .Match(
        Succ: delta => Accept(model, policy, state, normal, delta, key),
        Fail: _ => Fin.Succ(Reject(policy, state, normal)));

// Lm.Solved DELETED
```

**Why**

`Solved` is a one-call carrier rename around the required whole-result validity read. It separates `LinearSolution` evidence from the factorization pipeline that owns it.

**Change**

Bind `LinearSolution`, read `IsValid`, and project `Solution` directly between `SolveDetailed` and the trial match.

**Delta**

Code-fence LOC: -2. Module-level members: 1 removed, 0 added, net -1. Module-level types: 0 removed, 0 added, net 0. Net declared symbols: -1.

# 5. Inline single-use constraint formulas into exhaustive dispatch

**From**

`libs/dotnet/Rasm/.planning/Solving/solver.md:654`

```csharp
distance:      static (p, d) => Seq(DistanceRow(d.A, d.B, d.Target, p)),
angle:         static (p, a) => AngleRow(a.A, a.B, a.Radians, p).ToSeq(),
coincident:    static (p, c) => CoincidentRows(c.A, c.B, p),
concentric:    static (p, c) => CoincidentRows(c.A, c.B, p),
parallel:      static (p, l) => CrossRow(l.A, l.B, p).ToSeq(),
perpendicular: static (p, l) => DotRow(l.A, l.B, p).ToSeq(),
tangent:       static (p, t) => TangentRow(t.Line, t.Circle, p).ToSeq(),
pointOnLine:   static (p, o) => PointOnLineRow(o.Point, o.Line, p).ToSeq(),
midpoint:      static (p, m) => MidpointRows(m.Point, m.Line, p).IfNone(Seq<ResidualRow>()),
axis:          static (p, x) => AxisRow(x.Line, x.Lock, p).ToSeq(),
equal:         static (p, e) => EqualRow(e.A, e.B, p).ToSeq(),
symmetric:     static (p, s) => SymmetricRows(s.A, s.B, s.Axis, p).IfNone(Seq<ResidualRow>()),
ground:        static (p, g) => GroundRows(g.Point, g.X, g.Y, p),
radius:        static (p, r) => RadiusRow(r.Circle, r.Target, p).ToSeq(),
onCircle:      static (p, o) => OnCircleRow(o.Point, o.Circle, p).ToSeq());
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:712`

```csharp
static ResidualRow DistanceRow(Entity a, Entity b, double target, ReadOnlySpan<double> p) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:721`

```csharp
static Option<ResidualRow> AngleRow(Entity a, Entity b, double radians, ReadOnlySpan<double> p) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:743`

```csharp
static Option<ResidualRow> CrossRow(Entity a, Entity b, ReadOnlySpan<double> p) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:750`

```csharp
static Option<ResidualRow> DotRow(Entity a, Entity b, ReadOnlySpan<double> p) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:757`

```csharp
static Option<ResidualRow> TangentRow(Entity line, Entity circle, ReadOnlySpan<double> p) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:780`

```csharp
static Option<ResidualRow> PointOnLineRow(Entity point, Entity line, ReadOnlySpan<double> p) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:788`

```csharp
static Option<Seq<ResidualRow>> MidpointRows(Entity point, Entity line, ReadOnlySpan<double> p) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:795`

```csharp
static Option<ResidualRow> AxisRow(Entity line, AxisLock axis, ReadOnlySpan<double> p) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:801`

```csharp
static Option<ResidualRow> EqualRow(Entity a, Entity b, ReadOnlySpan<double> p) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:813`

```csharp
static Option<Seq<ResidualRow>> SymmetricRows(Entity a, Entity b, Entity axis, ReadOnlySpan<double> p) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:831`

```csharp
static Seq<ResidualRow> GroundRows(Entity point, double x, double y, ReadOnlySpan<double> p) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:838`

```csharp
static Option<ResidualRow> RadiusRow(Entity circle, double target, ReadOnlySpan<double> p) =>
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:841`

```csharp
static Option<ResidualRow> OnCircleRow(Entity point, Entity circle, ReadOnlySpan<double> p) {
```

**To**

```csharp
distance: static (p, d) => {
    Point3d a = d.A.Origin(p), b = d.B.Origin(p);
    double dx = a.X - b.X, dy = a.Y - b.Y;
    double residual = dx * dx + dy * dy - d.Target * d.Target;
    return Seq(new ResidualRow(residual, Seq(
        (d.A.Offset, 2.0 * dx), (d.A.Offset + 1, 2.0 * dy),
        (d.B.Offset, -2.0 * dx), (d.B.Offset + 1, -2.0 * dy))));
},
coincident: static (p, c) => CoincidentRows(c.A, c.B, p),
concentric: static (p, c) => CoincidentRows(c.A, c.B, p),
```

```csharp
// DistanceRow DELETED
// AngleRow DELETED
// CrossRow DELETED
// DotRow DELETED
// TangentRow DELETED
// PointOnLineRow DELETED
// MidpointRows DELETED
// AxisRow DELETED
// EqualRow DELETED
// SymmetricRows DELETED
// GroundRows DELETED
// RadiusRow DELETED
// OnCircleRow DELETED
```

**Why**

Thirteen private methods are called by exactly one generated `Switch` arm and exist only to move the formula away from its case. They add member surface and parameter-forwarding lines without reuse. `CoincidentRows` is the sole shared formula and remains justified by its two cases.

**Change**

Move each listed method body unchanged into its owning exhaustive `Residual` arm, replacing forwarded parameters with the case payload; retain `CoincidentRows` for `Coincident` and `Concentric` only.

**Delta**

Code-fence LOC: -13. Type members: 13 removed, 0 added, net -13. Module-level types: 0 removed, 0 added, net 0. Net declared symbols: -13.

# 6. Derive mirrored counts from owned carriers

**From**

`libs/dotnet/Rasm/.planning/Solving/solver.md:673`

```csharp
public int RowCount =>
    Switch(
        distance:      static _ => 1,
        angle:         static _ => 1,
        coincident:    static _ => 2,
        concentric:    static _ => 2,
        parallel:      static _ => 1,
        perpendicular: static _ => 1,
        tangent:       static _ => 1,
        pointOnLine:   static _ => 1,
        midpoint:      static _ => 2,
        axis:          static _ => 1,
        equal:         static _ => 1,
        symmetric:     static _ => 2,
        ground:        static _ => 2,
        radius:        static _ => 1,
        onCircle:      static _ => 1);

public bool WellFormed(double[] p) => Residual(p).Count == RowCount;
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:873`

```csharp
public sealed record ConstraintSystem(
    Seq<Entity> Entities,
    Seq<Constraint> Constraints,
    Arr<double> Seed,
    int ParameterCount) {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:882`

```csharp
internal Lazy<int> ResidualRows { get; } = new(() => Constraints.Sum(static constraint => constraint.RowCount));
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:909`

```csharp
.Filter(row => row.Constraint.Touches.ForAll(entity => placedSet.Contains(entity)) && !row.Constraint.WellFormed(seed))
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:1051`

```csharp
int rows = island.Constraints.Sum(ci => system.Constraints[ci].RowCount);
```

**To**

```csharp
// Constraint.RowCount DELETED
// Constraint.WellFormed DELETED
```

```csharp
public sealed record ConstraintSystem(
    Seq<Entity> Entities,
    Seq<Constraint> Constraints,
    Arr<double> Seed) {
```

```csharp
// ConstraintSystem.ParameterCount DELETED
// ConstraintSystem.ResidualRows DELETED
```

```csharp
.Filter(row => row.Constraint.Touches.ForAll(entity => placedSet.Contains(entity))
    && row.Constraint.Residual(seed).IsEmpty)
```

```csharp
int rows = island.Constraints.Sum(ci => system.Constraints[ci].Residual(system.SeedVector.Value).Count);
```

**Why**

`RowCount` is a hand-maintained case mirror of the sequence the exhaustive residual algebra already emits. `WellFormed` compares the live value to that mirror, `ResidualRows` caches one final read, and `ParameterCount` repeats `Seed.Count` without a consumer.

**Change**

Use `Residual(parameters).Count` as the sole row-cardinality authority, use an empty residual to refuse an operand-kind mismatch, derive the final row total from the solved parameter vector, remove the unused parameter-count column, and update construction to pass only `placed`, `constraints`, and `seed`.

**Delta**

Code-fence LOC: -20. Type members: 4 removed, 0 added, net -4. Module-level types: 0 removed, 0 added, net 0. Net declared symbols: -4.

# 7. Replace oracle and provenance jargon with one rank method

**From**

`libs/dotnet/Rasm/.planning/Solving/solver.md:600`

```csharp
[SmartEnum<string>]
public sealed partial class DofOracle {
    public static readonly DofOracle RowCount = new(key: "row-count", adjudicate: ConstraintSolver.CountRank);
    public static readonly DofOracle Matching = new(key: "matching", adjudicate: ConstraintSolver.MatchRank);
    public static readonly DofOracle Witness  = new(key: "witness", adjudicate: ConstraintSolver.WitnessRank);

    [UseDelegateFromConstructor] public partial DofReport Adjudicate(ConstraintSystem system, Op key);
}
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:852`

```csharp
[SmartEnum<int>]
public sealed partial class RankProvenance {
    public static readonly RankProvenance Witnessed = new(key: 0);
    public static readonly RankProvenance Matched   = new(key: 1);
    public static readonly RankProvenance Counted   = new(key: 2);
}

public readonly record struct IslandVerdict(
    int Island, Determinacy Verdict, int FreeDof, int Deficiency, int Rank, RankProvenance Provenance);
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:862`

```csharp
public sealed record DofReport(Determinacy Verdict, Seq<IslandVerdict> Islands) : IValidityEvidence {
    public int StructuralRank => Islands.Sum(static row => row.Rank);
    public int MatchingDeficiency => Islands.Sum(static row => row.Deficiency);
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:1045`

```csharp
public static DofReport Analyze(ConstraintSystem system, DofOracle oracle, Op? key = null) =>
    oracle.Adjudicate(system: system, key: key.OrDefault());
```

**To**

```csharp
[SmartEnum<string>]
public sealed partial class RankMethod {
    public static readonly RankMethod Count    = new(key: "count", analyze: ConstraintSystem.CountRank);
    public static readonly RankMethod Matching = new(key: "matching", analyze: ConstraintSystem.MatchRank);
    public static readonly RankMethod Witness  = new(key: "witness", analyze: ConstraintSystem.WitnessRank);

    [UseDelegateFromConstructor] public partial DofReport Analyze(ConstraintSystem system, Op key);
}
```

```csharp
// RankProvenance DELETED

public readonly record struct IslandVerdict(
    int Island, Determinacy Verdict, int FreeDof, int Deficiency, int Rank, RankMethod Method);
```

```csharp
public sealed record DofReport(Determinacy Verdict, Seq<IslandVerdict> Islands) : IValidityEvidence {
    public int Rank => Islands.Sum(static row => row.Rank);
    public int Deficiency => Islands.Sum(static row => row.Deficiency);
```

```csharp
// ConstraintSolver.Analyze DELETED
```

**Why**

`Oracle` and `Adjudicate` are coined names for selecting and running a rank method. `RankProvenance` then mirrors that same selection in a second generated roster, while the report totals use method-specific names even when the witness method produced them.

**Change**

Rename the policy owner to `RankMethod`, its delegate to `Analyze`, and `RowCount` to `Count`; store the actual deciding method on each island, including `Count` on witness fallback; delete `RankProvenance`; rename the totals to `Rank` and `Deficiency`; delete the forwarding `ConstraintSolver.Analyze` member. Update the generated-constructor delegate label from `adjudicate` to `analyze` and every target code/prose use in the same move.

**Delta**

Code-fence LOC: -8. Module-level types: 1 removed, 0 added, net -1. Generated smart-enum rows: 3 removed, 0 added, net -3. Module-level members: 1 removed, 0 added, net -1. Net declared symbols: -5.

# 8. Replace the behavior-free island record with a named tuple

**From**

`libs/dotnet/Rasm/.planning/Solving/solver.md:850`

```csharp
public readonly record struct ConstraintIsland(Seq<int> Entities, Seq<int> Constraints);
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:880`

```csharp
internal Lazy<Seq<ConstraintIsland>> Islands { get; } = new(() => Decompose(Entities, Constraints));
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:920`

```csharp
static Seq<ConstraintIsland> Decompose(Seq<Entity> entities, Seq<Constraint> constraints) {
```

**To**

```csharp
// ConstraintIsland DELETED
```

```csharp
internal Lazy<Seq<(Seq<int> Entities, Seq<int> Constraints)>> Islands { get; } =
    new(() => Decompose(Entities, Constraints));
```

```csharp
static Seq<(Seq<int> Entities, Seq<int> Constraints)> Decompose(
    Seq<Entity> entities,
    Seq<Constraint> constraints) {
```

**Why**

`ConstraintIsland` carries two sequences and no invariant, identity, admission, behavior, or public consumer. Named tuple fields preserve every internal read without minting a module type and two record properties.

**Change**

Replace every `ConstraintIsland` annotation and construction with the named tuple `(Seq<int> Entities, Seq<int> Constraints)`.

**Delta**

Code-fence LOC: -1. Module-level types: 1 removed, 0 added, net -1. Record properties: 2 removed, 0 added, net -2. Net declared symbols: -3.

# 9. Make the constraint system the single solver owner

**From**

`libs/dotnet/Rasm/.planning/Solving/solver.md:59`

```csharp
internal static class KeyedSeverity {
    internal static TSelf Worst<TSelf>(TSelf left, TSelf right) where TSelf : class, IKeyedObject<int> =>
        left.ToValue() >= right.ToValue() ? left : right;
}
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:974`

```csharp
internal static class ResidualFold {
    internal static ddouble Norm(this Seq<ResidualRow> rows) =>
        ddouble.Sqrt(rows.Map(static row => (ddouble)row.Value * row.Value).Sum());
}
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:979`

```csharp
internal sealed class ConstraintModel : ILmModel {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:1045`

```csharp
public static class ConstraintSolver {
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:1147`

```csharp
public static Fin<Solution> Solve(ConstraintSystem system, SolvePolicy policy, Op? key = null) {
    Op op = key.OrDefault();
    DofReport report = DofOracle.Witness.Adjudicate(system: system, key: op);
    Seq<ConstraintIsland> islands = system.Islands.Value;
```

**To**

```csharp
public sealed record ConstraintSystem(
    Seq<Entity> Entities,
    Seq<Constraint> Constraints,
    Arr<double> Seed) {
    // Build, decomposition, rank methods, and Solve share this owner.

    static T Worst<T>(T left, T right) where T : class, IKeyedObject<int> =>
        left.ToValue() >= right.ToValue() ? left : right;

    static ddouble Norm(Seq<ResidualRow> rows) =>
        ddouble.Sqrt(rows.Map(static row => (ddouble)row.Value * row.Value).Sum());

    private sealed class Model : ILmModel {
```

```csharp
// KeyedSeverity DELETED
// ResidualFold DELETED
// ConstraintModel DELETED
// ConstraintSolver DELETED
```

```csharp
public Fin<Solution> Solve(SolvePolicy policy, Op? key = null) {
    Op op = key.OrDefault();
    DofReport report = RankMethod.Witness.Analyze(system: this, key: op);
    Seq<(Seq<int> Entities, Seq<int> Constraints)> islands = Islands.Value;
```

**Why**

`ConstraintSolver` is a static service shell around one `ConstraintSystem`, `ConstraintModel` exists only as that system's island implementation, and both utility classes serve only those operations. Keeping four implementation types at module scope splits one cohesive solver and forces its methods to receive the owner they belong to.

**Change**

Move `CountRank`, `MatchRank`, `WitnessRank`, their private numeric helpers, and `Solve` onto `ConstraintSystem`; rename and nest `ConstraintModel` as private `Model`; absorb `KeyedSeverity.Worst` and `ResidualFold.Norm` as private static methods and update their internal calls; change the public call from `ConstraintSolver.Solve(system, policy, key)` to `system.Solve(policy, key)`; keep `RankMethod` delegates pointed at the system's internal static rank methods. Reorder `Solution` as needed so `ConstraintSystem` remains one declaration rather than a second partial shell.

**Delta**

Code-fence LOC: -7. Module-level types: 4 removed, 0 added, net -4. Total types: 4 removed, 1 added, net -3. Method parameters: 1 removed, 0 added, net -1. Net declared symbols: -3.

# 10. Remove decomposition and residual intermediates

**From**

`libs/dotnet/Rasm/.planning/Solving/solver.md:921`

```csharp
FrozenDictionary<int, int> byOffset = entities.Map(static (entity, ordinal) => (entity.Offset, Ordinal: ordinal))
    .ToDictionary(static row => row.Offset, static row => row.Ordinal)
    .ToFrozenDictionary();
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:948`

```csharp
return toSeq(entityRows.Select((rows, ordinal) => new ConstraintIsland(Entities: rows, Constraints: constraintRows[ordinal])));
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:1013`

```csharp
public ddouble Norm(ReadOnlySpan<double> parameters) {
    Scatter(parameters);
    double[] image = scratch;
    ConstraintSystem home = system;
    return constraints.Bind(ordinal => home.Constraints[ordinal].Residual(image)).Norm();
}
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:1070`

```csharp
FrozenDictionary<int, int> local = columns.Map(static (column, index) => (Column: column, Index: index))
    .ToDictionary(static row => row.Column, static row => row.Index)
    .ToFrozenDictionary();
```

**To**

```csharp
FrozenDictionary<int, int> byOffset = Enumerable.Range(0, entities.Count)
    .ToFrozenDictionary(ordinal => entities[ordinal].Offset);
```

```csharp
return toSeq(entityRows).Map((rows, ordinal) => (Entities: rows, Constraints: constraintRows[ordinal]));
```

```csharp
public ddouble Norm(ReadOnlySpan<double> parameters) {
    Scatter(parameters);
    return ConstraintSystem.Norm(constraints.Bind(ordinal => system.Constraints[ordinal].Residual(scratch)));
}
```

```csharp
FrozenDictionary<int, int> local = Enumerable.Range(0, columns.Count)
    .ToFrozenDictionary(index => columns[index]);
```

**Why**

Both frozen maps materialize a mutable dictionary solely to freeze it, the island return crosses LINQ only to re-enter `Seq`, and the residual norm aliases two fields immediately before reading them. These stages add allocations and local names without changing ownership or evidence.

**Change**

Build each ordinal map directly from `Enumerable.Range`, map the island array through `Seq.Map`, and feed the existing fields directly to the system-owned norm fold.

**Delta**

Code-fence LOC: -4. Local variables: 2 removed, 0 added, net -2. Module-level types/members: 0 removed, 0 added, net 0. Intermediate dictionary materializations: -2. Net declared symbols: -2.

# 11. Traverse independent islands into one solution result

**From**

`libs/dotnet/Rasm/.planning/Solving/solver.md:952`

```csharp
public sealed record Convergence(
    SolveStatus Status,
    Determinacy Dof,
    double ResidualNorm,
    int Iterations,
    double TerminalLambda,
    int ResidualRows,
    int Islands) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Status is not null && Dof is not null,
        ValidityClaim.Finite(ResidualNorm),
        ValidityClaim.Nonnegative(ResidualNorm),
        ValidityClaim.CountAtLeast(count: Iterations, floor: 0),
        ValidityClaim.CountAtLeast(count: ResidualRows, floor: 0),
        ValidityClaim.CountAtLeast(count: Islands, floor: 1),
        ValidityClaim.Finite(TerminalLambda),
        ValidityClaim.Positive(TerminalLambda));
}

public sealed record Solution(Arr<double> Parameters, Convergence Convergence);
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:1151`

```csharp
return islands.Fold(
        Fin.Succ((Parameters: system.SeedVector.Value.ToArray(), Iterations: 0, Terminal: Option<(double Lambda, SolveStatus Status)>.None)),
        (acc, island) => acc.Bind(state =>
            ConstraintModel.Of(system: system, island: island, current: state.Parameters, key: op)
                .Bind(model => Lm.Minimize(model, policy, op))
                .Map(result => (
                    Scatter(state.Parameters, system, island, result.Parameters),
                    state.Iterations + result.Iterations,
                    Some(state.Terminal.Match(
                        Some: held => (Math.Max(held.Lambda, result.Lambda), KeyedSeverity.Worst(held.Status, result.Status)),
                        None: () => (result.Lambda, result.Status)))))))
    .Bind(state => state.Terminal.ToFin(op.InvalidInput()).Bind(terminal => {
        double norm = (double)system.Constraints.Bind(constraint => constraint.Residual(state.Parameters)).Norm();
        return report.Verdict == Determinacy.Over && norm >= policy.ResidualTolerance.Value
            ? Fin.Fail<Solution>(new GeometryFault.OverConstrained(report.MatchingDeficiency, norm))
            : Fin.Succ(new Solution(
                new Arr<double>(state.Parameters),
                new Convergence(terminal.Status, report.Verdict, norm, state.Iterations,
                    terminal.Lambda, system.ResidualRows.Value, islands.Count)));
    }));
```

`libs/dotnet/Rasm/.planning/Solving/solver.md:1173`

```csharp
static double[] Scatter(double[] parameters, ConstraintSystem system, ConstraintIsland island, Arr<double> local) {
    double[] next = (double[])parameters.Clone();
    int cursor = 0;
    foreach (int ordinal in island.Entities) {
        Entity entity = system.Entities[ordinal];
        for (int k = 0; k < entity.Arity; k++) next[entity.Offset + k] = local[cursor++];
    }
    return next;
}
```

**To**

```csharp
public sealed record Solution(
    Arr<double> Parameters,
    SolveStatus Status,
    Determinacy Determinacy,
    double ResidualNorm,
    int Iterations,
    double Lambda,
    int ResidualRows,
    int Islands) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Status is not null && Determinacy is not null,
        ValidityClaim.Finite(ResidualNorm),
        ValidityClaim.Nonnegative(ResidualNorm),
        ValidityClaim.CountAtLeast(Iterations, 0),
        ValidityClaim.CountAtLeast(ResidualRows, 0),
        ValidityClaim.CountAtLeast(Islands, 1),
        ValidityClaim.Finite(Lambda),
        ValidityClaim.Positive(Lambda));
}

// Convergence DELETED
```

```csharp
return islands.TraverseM(island =>
        Model.Of(this, island, SeedVector.Value, op)
            .Bind(model => Lm.Minimize(model, policy, op))
            .Map(result => (Island: island, Result: result)))
    .As()
    .Bind(solved => {
        double[] parameters = SeedVector.Value.ToArray();
        int iterations = 0;
        double lambda = 0.0;
        SolveStatus status = SolveStatus.Converged;
        foreach (var (island, result) in solved) {
            int cursor = 0;
            foreach (int ordinal in island.Entities) {
                Entity entity = Entities[ordinal];
                for (int k = 0; k < entity.Arity; k++) parameters[entity.Offset + k] = result.Parameters[cursor++];
            }
            iterations += result.Iterations;
            lambda = Math.Max(lambda, result.Lambda);
            status = Worst(status, result.Status);
        }
        double norm = (double)Norm(Constraints.Bind(constraint => constraint.Residual(parameters)));
        return report.Verdict == Determinacy.Over && norm >= policy.ResidualTolerance.Value
            ? Fin.Fail<Solution>(new GeometryFault.OverConstrained(report.Deficiency, norm))
            : Fin.Succ(new Solution(new Arr<double>(parameters), status, report.Verdict,
                norm, iterations, lambda,
                Constraints.Sum(constraint => constraint.Residual(parameters).Count), islands.Count));
    });
```

```csharp
// Scatter DELETED
```

**Why**

Islands are column-disjoint, but the monadic fold makes each model consume the previous island's scattered vector and carries an optional terminal tuple solely to prove non-emptiness. `Scatter` then clones that vector once per island despite having one caller. `Solution` wraps a second public result type whose fields have no separate lifecycle or consumer.

**Change**

Use `Seq.TraverseM` to sequence each island's `Fin<LmResult>` from the common admitted seed; assemble the column-disjoint results into one cloned seed while accumulating summary values, and delete the one-call `Scatter` helper. Absorb `Convergence` into the sole public `Solution` carrier and derive the final row count from the solved vector.

**Delta**

Code-fence LOC: -1. Module-level types: 1 removed, 0 added, net -1. Module-level members: 1 removed, 0 added, net -1. Record properties: 8 removed, 7 added, net -1. Net declared symbols: -3.

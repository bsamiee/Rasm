# `matrix.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Numerics/matrix.md`

This audit counts nonblank authored C# lines in the affected fence fragments. Required consumer edits outside the target are named but excluded from the target LOC total. The queue is ordered so generated-shape and policy reductions land before the gauge collapse, carrier trimming, kernel inlining, and vocabulary cleanup.

Evidence basis: the full target; `CLAUDE.md`; the branch and `Rasm` planning laws; the complete C# stack standards; the shared and package-local `.api` catalogues relevant to LanguageExt, Thinktecture, MathNet.Numerics, CSparse, `System.Numerics.Tensors`, CommunityToolkit.HighPerformance, and TYoshimura.DoubleDouble; the direct `libs/dotnet/` consumers and prose references of every affected symbol; and the prior root audit form at commit `f17b2d8521806b567232dd8c28167e4cbe294da4`.

Accepted total for target fences: **-52 LOC, -2 authored type symbols, -25 authored member symbols**, plus removal of the unearned generated key, keyed lookup/conversion, and keyed-owner surfaces from ten process-local smart-enum rosters.

## 1. Make only the non-keyed numeric vocabularies keyless

### Location

- `matrix.md:77-244`, anchors `ResidualCap`, `KrylovSolver`, `SparsePreconditioner`, `EigenOrder`, `SolveStop`, `EigenSolveStop`, `MatrixNormKind`, `OperatorSense`, and `GaugeShift`
- `matrix.md:708-713`, anchor `MatrixDrawLane`

### From

```csharp
[SmartEnum<int>]
public sealed partial class ResidualCap {
    public static readonly ResidualCap Converged = new(key: 0, floor: EpsilonPolicy.SqrtEpsilon, lane: ToleranceLane.Residual);
```

```csharp
[SmartEnum<int>]
public sealed partial class KrylovSolver {
    public static readonly KrylovSolver BiCgStab = new(key: 0,
```

```csharp
[SmartEnum<int>]
public sealed partial class SparsePreconditioner {
    public static readonly SparsePreconditioner None = new(key: 0,
```

```csharp
[SmartEnum<int>]
public sealed partial class EigenOrder {
    public static readonly EigenOrder DescendingMagnitude = new(key: 0);
```

```csharp
[SmartEnum<int>]
public sealed partial class SolveStop {
    public static readonly SolveStop DirectSolved = new(key: 0, isUsable: true);
```

```csharp
[SmartEnum<int>]
public sealed partial class EigenSolveStop {
    public static readonly EigenSolveStop DirectSolved = new(key: 0, isUsable: true);
```

```csharp
[SmartEnum<int>]
public sealed partial class MatrixNormKind {
    public static readonly MatrixNormKind Frobenius = new(key: 0, compute: static m => TensorPrimitives.Norm<double>(m.Entries.AsSpan()));
```

```csharp
[SmartEnum<int>]
public sealed partial class OperatorSense {
    public static readonly OperatorSense Forward = new(key: 0,
```

```csharp
[SmartEnum<int>]
public sealed partial class GaugeShift {
    public static readonly GaugeShift None = new(0);
```

```csharp
[SmartEnum<int>]
public sealed partial class MatrixDrawLane : IDrawLane<MatrixDrawLane> {
    public static readonly MatrixDrawLane RealBasis = new(key: 0, lane: 17L);
```

### To

```csharp
[SmartEnum]
public sealed partial class ResidualCap {
    public static readonly ResidualCap Converged = new(floor: EpsilonPolicy.SqrtEpsilon, lane: ToleranceLane.Residual);
```

```csharp
[SmartEnum]
public sealed partial class KrylovSolver {
    public static readonly KrylovSolver BiCgStab = new(
```

```csharp
[SmartEnum]
public sealed partial class SparsePreconditioner {
    public static readonly SparsePreconditioner None = new(
```

```csharp
[SmartEnum]
public sealed partial class EigenOrder {
    public static readonly EigenOrder DescendingMagnitude = new();
```

```csharp
[SmartEnum]
public sealed partial class SolveStop {
    public static readonly SolveStop DirectSolved = new(isUsable: true);
```

```csharp
[SmartEnum]
public sealed partial class EigenSolveStop {
    public static readonly EigenSolveStop DirectSolved = new(isUsable: true);
```

```csharp
[SmartEnum]
public sealed partial class MatrixNormKind {
    public static readonly MatrixNormKind Frobenius = new(compute: static m => TensorPrimitives.Norm<double>(m.Entries.AsSpan()));
```

```csharp
[SmartEnum]
public sealed partial class OperatorSense {
    public static readonly OperatorSense Forward = new(
```

```csharp
[SmartEnum]
public sealed partial class GaugeShift {
    public static readonly GaugeShift None = new();
```

```csharp
[SmartEnum]
public sealed partial class MatrixDrawLane : IDrawLane<MatrixDrawLane> {
    public static readonly MatrixDrawLane RealBasis = new(lane: 17L);
```

Apply the same exact key-argument deletion to every remaining row of these ten owners.

### Effect

- Target fenced LOC: unchanged.
- Authored symbols: unchanged; thirty-five integer constructor keys disappear.
- Generated surface: all owners retain `Items`, identity, their columns/delegates, and total `Switch`/`Map`; unused key properties, keyed lookup/conversion, and keyed-owner conformance disappear.

### API and consumer proof

The checked Thinktecture surface distinguishes `[SmartEnum]` as the keyless roster form and confirms that it retains roster and exhaustive-dispatch generation. A complete `libs/dotnet/` read finds no key read, key lookup, key conversion, parse, serialization, or persistence use for any of these ten owners. `MatrixDrawLane` is consumed through `IDrawLane<T>.Lane`, not a Thinktecture key. This move deliberately excludes `SolveTrait`, because `CapabilitySet<TCapability>` requires its stable string `Key`, and excludes `SolvePath`/`EigenSolvePath`, because route evidence and faults read `path.Key`.

### Ripples

- Same file: update package/density prose that spells `[SmartEnum<int>]` for these owners.
- Outside target: none; case references, equality, `Items`, delegate calls, and exhaustive dispatch remain source-compatible.

## 2. Fold the route law helper into the capability owner

### Location

- `matrix.md:58-74`, anchors the close of `SolveTrait`, `public static class SolveTraitLaw`, `Routes`, and `Admit`
- `matrix.md:146-176`, every `SolveTraitLaw.Admit(...)` route construction

### From

```csharp
}

public static class SolveTraitLaw {
    public static readonly CapabilityLaw<SolveTrait> Routes = new(Legal: Seq(
```

```csharp
    internal static CapabilitySet<SolveTrait> Admit(params ReadOnlySpan<SolveTrait> held) =>
        Routes.Admit(held: CapabilitySet<SolveTrait>.Of(held)).ThrowIfFail();
}
```

```csharp
traits: SolveTraitLaw.Admit(SolveTrait.Direct, SolveTrait.Square)
```

### To

```csharp
    private static readonly CapabilityLaw<SolveTrait> Routes = new(Legal: Seq(
```

```csharp
    internal static CapabilitySet<SolveTrait> Admit(params ReadOnlySpan<SolveTrait> held) =>
        Routes.Admit(held: CapabilitySet<SolveTrait>.Of(held)).ThrowIfFail();
}
```

```csharp
traits: SolveTrait.Admit(SolveTrait.Direct, SolveTrait.Square)
```

Move the existing `Routes` initializer and `Admit` body unchanged inside `SolveTrait`, and apply the same qualifier deletion to every remaining solve/eigen route row.

### Effect

- Target fenced LOC: helper block `15 -> 13` (**-2**).
- Authored symbols: **-1 public type** (`SolveTraitLaw`); `Routes` becomes a private implementation fact on `SolveTrait`, while `Admit` remains internal.
- Ownership: the capability vocabulary, its legal set, and its only admission door become one owner instead of a roster plus a module-level companion.

### API and consumer proof

`SolveTraitLaw` has no consumer outside this fence. Its only state is the legal-corner table for `SolveTrait`, and its only operation admits a `CapabilitySet<SolveTrait>` against that table. Seating both members on the generated partial owner follows the repository's “extend the owner before minting a sibling” law without changing `CapabilitySet`, any route payload, or the construction-time refusal.

### Ripples

- Same file: `SolveTraitLaw.Admit(...)` -> `SolveTrait.Admit(...)` on all nine linear and five eigen route rows; replace the vocabulary prose's `SolveTraitLaw` name with “`SolveTrait`'s legal-corner table”.
- Outside target: none.

## 3. Replace the two-row Krylov fallback roster with the boolean policy fact it is

### Location

- `matrix.md:113-127`, anchors `KrylovRescue` and `KrylovPolicy`
- `matrix.md:1047-1084`, anchor `SparseSolve`

### From

```csharp
[SmartEnum<string>]
public sealed partial class KrylovRescue {
    public static readonly KrylovRescue Refused = new(key: "refused");
    public static readonly KrylovRescue Densify = new(key: "densify");
}
```

```csharp
Option<KrylovStop> Stop, KrylovRescue Rescue) {
public static Fin<KrylovPolicy> Of(SparsePreconditioner preconditioner, double tolerance, Dimension budget,
    Option<KrylovSolver> solver = default, Option<KrylovStop> stop = default, Option<KrylovRescue> rescue = default, Op? key = null) =>
```

```csharp
Tolerance: tolerance, Budget: budget, Stop: stop, Rescue: rescue.IfNone(noneValue: KrylovRescue.Refused));
```

```csharp
Tolerance: SolvePath.SparseKrylov.Cap.Floor, Budget: autoBudget, Stop: None, Rescue: KrylovRescue.Densify)) is var active
```

```csharp
: active.Rescue.Equals(KrylovRescue.Densify)
```

### To

```csharp
```

```csharp
Option<KrylovStop> Stop, bool CanFallback) {
public static Fin<KrylovPolicy> Of(SparsePreconditioner preconditioner, double tolerance, Dimension budget,
    Option<KrylovSolver> solver = default, Option<KrylovStop> stop = default, bool canFallback = false, Op? key = null) =>
```

```csharp
Tolerance: tolerance, Budget: budget, Stop: stop, CanFallback: canFallback);
```

```csharp
Tolerance: SolvePath.SparseKrylov.Cap.Floor, Budget: autoBudget, Stop: None, CanFallback: true)) is var active
```

```csharp
: active.CanFallback
```

### Effect

- Target fenced LOC: `10 -> 5` across the declaration and reads (**-5**).
- Authored symbols: **-1 public type** (`KrylovRescue`) and **-2 public row members** (`Refused`, `Densify`); the policy's `Rescue` property is replaced one-for-one by `CanFallback`.
- Logic: removes a string-keyed generated type whose entire semantic space is permission or refusal of one action.

### API and consumer proof

There is no third state, row behavior, wire spelling, lookup, or consumer-held identity. The only branch is equality with `Densify`; absence and `Refused` are identical. A boolean is therefore the exact independent policy axis, not a lossy collapse of an ADT. No outside consumer constructs `KrylovPolicy.Of`, `KrylovRescue`, or the positional policy record.

### Ripples

- Same file: replace “rescue row”/`KrylovRescue` prose with “dense-fallback permission”.
- Outside target: none.

## 4. Make the Krylov stop carrier the callback it names

### Location

- `matrix.md:111,119-127`, anchors `KrylovStop` and `KrylovPolicy.Stop`
- `matrix.md:1064-1068`, anchor `active.Stop.Map`

### From

```csharp
public readonly record struct KrylovStop(Func<int, double, bool> Halt);
```

```csharp
Option<KrylovStop> Stop, bool CanFallback) {
```

```csharp
Option<KrylovSolver> solver = default, Option<KrylovStop> stop = default, bool canFallback = false, Op? key = null) =>
```

```csharp
.. active.Stop.Map(static rule => (MathNet.Numerics.LinearAlgebra.Solvers.IIterationStopCriterion<double>)
    new MathNet.Numerics.LinearAlgebra.Solvers.DelegateStopCriterion<double>((iteration, _, _, residual) =>
        rule.Halt(arg1: iteration, arg2: residual.L2Norm())
```

### To

```csharp
public delegate bool KrylovStop(int iteration, double residual);
```

```csharp
Option<KrylovStop> Stop, bool CanFallback) {
```

```csharp
Option<KrylovSolver> solver = default, Option<KrylovStop> stop = default, bool canFallback = false, Op? key = null) =>
```

```csharp
.. active.Stop.Map(static halt => (MathNet.Numerics.LinearAlgebra.Solvers.IIterationStopCriterion<double>)
    new MathNet.Numerics.LinearAlgebra.Solvers.DelegateStopCriterion<double>((iteration, _, _, residual) =>
        halt(iteration, residual.L2Norm())
```

### Effect

- Target fenced LOC: unchanged.
- Authored symbols: **-1 generated positional member** (`KrylovStop.Halt`); the named callback type and policy property remain.
- Logic: removes a transparent one-field record and its unwrap without erasing the callback's `iteration`/`residual` domain into an undifferentiated `Func<int, double, bool>`.

### API and consumer proof

LanguageExt `Option<T>` already owns presence, so a record around the callback adds neither state nor admission. The named delegate remains necessary under the strong-type law: its parameter names preserve the iteration/residual contract that `Func<int, double, bool>` erases. MathNet's `DelegateStopCriterion<T>` accepts that callback directly, and repository search finds no construction or use of the old `KrylovStop.Halt` wrapper outside this one unwrap.

### Ripples

- Same file: the sparse-solve policy prose should name the optional `KrylovStop` callback, not a wrapper record.
- Outside target: none.

## 5. Construct `KrylovPlan` at its sole evidence mint

### Location

- `matrix.md:139`, anchor `internal KrylovPlan Plan`
- `matrix.md:1075`, anchor `PathEvidence.Iterative`

### From

```csharp
internal KrylovPlan Plan => new(Preconditioner: Preconditioner, Solver: Solver);
```

```csharp
PathEvidence evidence = new PathEvidence.Iterative(Iterations: seen, Budget: active.Budget, Tolerance: active.Tolerance, Plan: Some(active.Plan));
```

### To

```csharp
```

```csharp
PathEvidence evidence = new PathEvidence.Iterative(Iterations: seen, Budget: active.Budget, Tolerance: active.Tolerance, Plan: Some(new KrylovPlan(Preconditioner: active.Preconditioner, Solver: active.Solver)));
```

### Effect

- Target fenced LOC: `2 -> 1` (**-1**).
- Authored symbols: **-1 internal member** (`KrylovPolicy.Plan`).
- Logic: the evidence mint remains explicit and the public `KrylovPlan` evidence carrier remains intact.

### API and consumer proof

`Plan` has exactly one read and is a constructor-forwarding property. `PathEvidence.Iterative.Plan` is the correct durable owner because the `Rasm` ruling requires every `LinearSolution` to carry and preserve its exact solver/preconditioner evidence. This move removes only the one-hop projection, not the evidence type or column.

### Ripples

None.

## 6. Compose CSparse ordering and drop the redundant stored permutation

### Location

- `matrix.md:531-547`, anchors `CholeskySparse(... int[] permutation ...)`, the explicit `AMD.Generate`, and `internal int[] Permutation`

### From

```csharp
private CholeskySparse(SparseMatrix source, CSparse.Double.Factorization.SparseCholesky factor, int[] permutation, Dimension order) {
    Source = source; Factor = factor; Permutation = permutation; Order = order;
}
```

```csharp
from permutation in key.OrDefault().Catch(() => Fin.Succ(CSparse.Ordering.AMD.Generate(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA)))
from factor in key.OrDefault().Catch(() => Fin.Succ(progress.Match(
    Some: report => CSparse.Double.Factorization.SparseCholesky.Create(A: csc, p: permutation, progress: report),
    None: () => CSparse.Double.Factorization.SparseCholesky.Create(A: csc, p: permutation))))
```

```csharp
select new CholeskySparse(source: symmetric, factor: factor, permutation: permutation, order: symmetric.Rows);
```

```csharp
internal int[] Permutation { get; }
```

### To

```csharp
private CholeskySparse(SparseMatrix source, CSparse.Double.Factorization.SparseCholesky factor, Dimension order) {
    Source = source; Factor = factor; Order = order;
}
```

```csharp
from factor in key.OrDefault().Catch(() => Fin.Succ(progress.Match(
    Some: report => CSparse.Double.Factorization.SparseCholesky.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA, progress: report),
    None: () => CSparse.Double.Factorization.SparseCholesky.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA))))
```

```csharp
select new CholeskySparse(source: symmetric, factor: factor, order: symmetric.Rows);
```

```csharp
```

### Effect

- Target fenced LOC: `9 -> 7` across the stored permutation and explicit ordering mint (**-2**).
- Authored symbols: **-1 internal member** (`CholeskySparse.Permutation`); the constructor parameter is removed with its redundant property.
- Logic: CSparse's ordering-taking `SparseCholesky.Create` mints the AMD permutation once and forwards to the same factorization path; the never-read copy on the completed factor carrier and the hand-written package step both disappear.

### API and consumer proof

`Matrix.With` and `SymmetricMatrix.With` remain as valid immutable update capability; current consumer count does not decide their survival. `CholeskySparse.Permutation`, by contrast, only stores the package-generated ordering after factor creation and exposes no domain operation or evidence. The checked CSparse catalogue states that the ordering-taking `Create` overload performs `AMD.Generate` and forwards to the `int[]` overload; `Refactorize`, `Update`, `Downdate`, `Sweep`, and guarded solve all operate on the retained factor and never need the array again.

### Ripples

- Same file: in the sparse owner prose, replace “under a CACHED AMD permutation” with “built under one AMD ordering”; the completed symbolic factor is cached, not an authored permutation array.
- Outside target: none.

## 7. Collapse pin arity into one span-fed factory

### Location

- `matrix.md:255-258`, anchors `PinConstant` and `Pinned`

### From

```csharp
public static GaugePolicy PinConstant(int index, Option<Arr<double>> mass = default, Option<GaugeShift> shift = default) =>
    new Pin(Indices: new Arr<int>([index]), Values: new Arr<double>([0.0]), Mass: mass, PostShift: shift.IfNone(noneValue: GaugeShift.None));
public static GaugePolicy Pinned(Seq<int> indices, Option<Arr<double>> mass = default, Option<GaugeShift> shift = default) =>
```

### To

```csharp
public static GaugePolicy Pinned(ReadOnlySpan<int> indices, Option<Arr<double>> mass = default, Option<GaugeShift> shift = default) =>
    new Pin(Indices: new Arr<int>([.. indices]), Values: new Arr<double>(new double[indices.Length]), Mass: mass, PostShift: shift.IfNone(noneValue: GaugeShift.None));
```

### Effect

- Target fenced LOC: `4 -> 2` (**-2**).
- Authored symbols: **-1 public member** (`GaugePolicy.PinConstant`).
- Logic: singular and plural pinning become collection expressions on one `ReadOnlySpan<int>` entry; pin construction and zero-value derivation have one owner, and the zero values allocate directly at the admitted cardinality instead of mapping the indices.

### API and consumer proof

The C# arity law allows one `ReadOnlySpan<T>` boundary plus collection expressions to absorb singular and plural calls. Keeping `indices` first preserves ordinary positional use and avoids optional policy parameters ahead of a `params` spread; every current call can supply `[0]` or `[.. sources]` directly. `Pinned` already performs the complete operation for every cardinality, including one, and all existing callers request the same zero pin values. The span is consumed synchronously into the two `Arr` payloads and never escapes. No caller depends on a distinct return type or behavior.

### Ripples

- `libs/dotnet/Rasm/.planning/Meshing/dec.md:224`: `GaugePolicy.PinConstant(index: 0, mass: ..., shift: ...)` -> `GaugePolicy.Pinned(indices: [0], mass: ..., shift: ...)`.
- `libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:574`: same replacement; line 17 prose must name `Pinned` as well.
- `libs/dotnet/Rasm/.planning/Processing/sample.md:143`: same replacement.
- `libs/dotnet/Rasm/.planning/Processing/geodesics.md:142`: preserve the existing multi-pin call as `GaugePolicy.Pinned(indices: [.. sources], mass: ..., shift: ...)` under the new span signature; line 17 prose must show the same collection-expression intake rather than the old `Seq<int>` call shape.

## 8. Collapse the gauge projections into the one solve that consumes them

### Location

- `matrix.md:265-288`, anchors `NullspaceDim`, `Shift`, `Mass`, `Basis`, `PinIndices`, `Compatible`, `DeflatesRhs`, and `Path`
- `matrix.md:1178-1199`, anchors `MassDiagonal`, `NullspaceColumns`, `compatibility`, and the `GaugeFix` mint
- `matrix.md:1218-1229`, anchors `MassDiagonal`, `NullspaceColumns`, and `PinColumns`

### From

```csharp
internal int NullspaceDim => Switch(
    pin: static p => p.Indices.Count,
    meanZeroDeflation: static d => d.Nullspace.Count,
    lagrangeKKT: static k => k.Nullspace.Count);
internal GaugeShift Shift => Switch(
    pin: static p => p.PostShift,
    meanZeroDeflation: static d => d.PostShift,
    lagrangeKKT: static k => k.PostShift);
internal Option<Arr<double>> Mass => Switch(
    pin: static p => p.Mass, meanZeroDeflation: static d => d.Mass, lagrangeKKT: static k => k.Mass);
internal Arr<Arr<double>> Basis => Switch(
    pin: static _ => new Arr<Arr<double>>([]),
    meanZeroDeflation: static d => d.Nullspace,
    lagrangeKKT: static k => k.Nullspace);
internal Arr<int> PinIndices => Switch(
    pin: static p => p.Indices, meanZeroDeflation: static _ => new Arr<int>([]), lagrangeKKT: static _ => new Arr<int>([]));
internal bool Compatible => Switch(
    pin: static _ => false, meanZeroDeflation: static _ => true, lagrangeKKT: static _ => true);
internal bool DeflatesRhs => Switch(
    pin: static _ => false, meanZeroDeflation: static _ => true, lagrangeKKT: static _ => false);
internal SolvePath Path => Switch(
    pin: static _ => SolvePath.SparseCholesky,
    meanZeroDeflation: static _ => SolvePath.SparseKrylov,
    lagrangeKKT: static _ => SolvePath.SparseLdl);
```

```csharp
Matrix<double> mass = MassDiagonal(mass: gauge.Mass, dimension: n);
Matrix<double> nullspace = NullspaceColumns(gauge: gauge, dimension: n);
double compatibility = gauge.Compatible ? nullspace.TransposeThisAndMultiply(b).L2Norm() : 0.0;
bool projectRhs = gauge.DeflatesRhs
    && compatibility > context.For(lane: ToleranceLane.Kkt).Value * Math.Max(val1: 1.0, val2: b.InfinityNorm());
LinearVector rhsGauged = projectRhs ? DeflateRhs(nullspace: nullspace, mass: mass, b: b, key: key) : b;
```

```csharp
LinearVector shifted = ApplyShift(shift: gauge.Shift, mass: mass, x: stage.X, rows: n);
```

```csharp
Path: gauge.Path, NullspaceDim: gauge.NullspaceDim, NullspaceDimNumeric: stage.NullspaceDimNumeric,
```

```csharp
PinIndices: gauge.PinIndices, ConstraintRows: gauge.NullspaceDim, PostShiftApplied: gauge.Shift,
```

```csharp
private static Matrix<double> MassDiagonal(Option<Arr<double>> mass, int dimension) =>
    mass.Match(
        Some: diagonal => (Matrix<double>)DenseMatrixD.OfDiagonalVector(DenseVectorD.OfArray([.. diagonal.AsIterable()])),
        None: () => DenseMatrixD.CreateIdentity(order: dimension));
private static Matrix<double> NullspaceColumns(GaugePolicy gauge, int dimension) =>
    gauge.Switch(
        state: dimension,
        pin: static (dim, p) => PinColumns(indices: p.Indices, dimension: dim),
        meanZeroDeflation: static (_, d) => BasisColumns(basis: d.Nullspace),
        lagrangeKKT: static (_, k) => BasisColumns(basis: k.Nullspace));
private static Matrix<double> PinColumns(Arr<int> indices, int dimension) =>
    DenseMatrixD.OfColumnVectors([.. indices.AsIterable().Select(index => DenseVectorD.Create(dimension, i => i == index ? 1.0 : 0.0))]);
```

### To

```csharp
```

```csharp
Option<Arr<double>> weights = gauge.Switch(
    pin: static p => p.Mass, meanZeroDeflation: static d => d.Mass, lagrangeKKT: static k => k.Mass);
Matrix<double> mass = weights.Match(
    Some: diagonal => (Matrix<double>)DenseMatrixD.OfDiagonalVector(DenseVectorD.OfArray([.. diagonal.AsIterable()])),
    None: () => DenseMatrixD.CreateIdentity(order: n));
Matrix<double> nullspace = gauge.Switch(
    state: n,
    pin: static (dim, p) => DenseMatrixD.OfColumnVectors([.. p.Indices.AsIterable().Select(index => DenseVectorD.Create(dim, i => i == index ? 1.0 : 0.0))]),
    meanZeroDeflation: static (_, d) => BasisColumns(basis: d.Nullspace), lagrangeKKT: static (_, k) => BasisColumns(basis: k.Nullspace));
GaugeShift shift = gauge.Switch(
    pin: static p => p.PostShift, meanZeroDeflation: static d => d.PostShift, lagrangeKKT: static k => k.PostShift);
int nullspaceDim = nullspace.ColumnCount;
Arr<int> pinIndices = gauge.Switch(
    pin: static p => p.Indices, meanZeroDeflation: static _ => new Arr<int>([]), lagrangeKKT: static _ => new Arr<int>([]));
double compatibility = gauge.Switch(
    state: (Nullspace: nullspace, Rhs: b),
    pin: static (_, _) => 0.0,
    meanZeroDeflation: static (s, _) => s.Nullspace.TransposeThisAndMultiply(s.Rhs).L2Norm(),
    lagrangeKKT: static (s, _) => s.Nullspace.TransposeThisAndMultiply(s.Rhs).L2Norm());
bool projectRhs = gauge.Switch(
    state: compatibility > context.For(lane: ToleranceLane.Kkt).Value * Math.Max(val1: 1.0, val2: b.InfinityNorm()),
    pin: static (_, _) => false, meanZeroDeflation: static (project, _) => project, lagrangeKKT: static (_, _) => false);
LinearVector rhsGauged = projectRhs ? DeflateRhs(nullspace: nullspace, mass: mass, b: b, key: key) : b;
```

```csharp
LinearVector shifted = ApplyShift(shift: shift, mass: mass, x: stage.X, rows: n);
```

```csharp
Path: stage.Path, NullspaceDim: nullspaceDim, NullspaceDimNumeric: stage.NullspaceDimNumeric,
```

```csharp
PinIndices: pinIndices, ConstraintRows: nullspaceDim, PostShiftApplied: shift,
```

```csharp
```

### Effect

- Target fenced LOC: the `36` projection/helper lines delete while the solve-site fragment grows `6 -> 23`, for **-19** net.
- Authored symbols: **-11 internal/private members** — all eight `GaugePolicy` projections plus the one-use `MassDiagonal`, `NullspaceColumns`, and `PinColumns` helpers. `BasisColumns` stays because both non-pin cases compose it.
- Correctness: `GaugeFix.Path` now reports the route returned by `GaugeStage`, including `SparseSolve`'s dense fallback and the KKT `SparseLdl -> SparseLu` fallback, rather than the policy's initial route prediction. `NullspaceDim` derives from the exact matrix passed to every arm, so the evidence cannot drift from the applied basis.

### API and consumer proof

Thinktecture's generated `Switch` stays on every case-dependent fact — mass weights, nullspace construction, shift, pin evidence, compatibility, and right-hand-side deflation — so a fourth gauge case still breaks every unresolved semantic axis at compile time. `Basis` and the other removed properties only forward nested-case state into this one kernel entry; the generated case dispatch remains the authoritative projection. Every `SolvePin`, `SolveDeflated`, and `SolveKkt` arm already mints the factual `GaugeStage.Path`; discarding it for `gauge.Path` falsifies evidence after a conditioned solve. Consumers in `Meshing/dec`, `Processing/geodesics`, `Processing/sample`, and `Meshing/reconstruct` carry or inspect the returned solution evidence and require the actual path.

### Ripples

- Same file: rewrite the vocabulary case paragraph at line 18 so it names `SingularGaugeSolve` as the one generated-dispatch consumer of mass, basis, shift, and pin payloads instead of advertising the deleted projection members.
- Outside target: none; no public signature changes.

## 9. Put post-solve normalization behavior on `GaugeShift`

### Location

- `matrix.md:241-247`, anchor `GaugeShift`
- `matrix.md:1191`, anchor `ApplyShift`
- `matrix.md:1255-1266`, anchors `ApplyShift` and `MassWeightedMean`

### From

```csharp
[SmartEnum]
public sealed partial class GaugeShift {
    public static readonly GaugeShift None = new();
    public static readonly GaugeShift MeanZero = new();
    public static readonly GaugeShift MinZero = new();
    public static readonly GaugeShift PinZero = new();
}
```

```csharp
LinearVector shifted = ApplyShift(shift: shift, mass: mass, x: stage.X, rows: n);
```

```csharp
private static LinearVector ApplyShift(GaugeShift shift, Matrix<double> mass, LinearVector x, int rows) =>
    shift.Switch(
        state: (Mass: mass, X: x, Rows: rows),
        none: static s => s.X,
        meanZero: static s => s.X - (MassWeightedMean(mass: s.Mass, x: s.X) * DenseVectorD.Create(s.Rows, static _ => 1.0)),
        minZero: static s => s.X - (s.X.Minimum() * DenseVectorD.Create(s.X.Count, static _ => 1.0)),
        pinZero: static s => s.X);
private static double MassWeightedMean(Matrix<double> mass, LinearVector x) {
    LinearVector ones = DenseVectorD.Create(x.Count, static _ => 1.0);
    LinearVector massOnes = mass.Multiply(ones);
    return massOnes.DotProduct(x) / Math.Max(val1: EpsilonPolicy.SqrtEpsilon, val2: massOnes.DotProduct(ones));
}
```

### To

```csharp
[SmartEnum]
public sealed partial class GaugeShift {
    public static readonly GaugeShift None = new(apply: static (_, x) => x);
    public static readonly GaugeShift MeanZero = new(apply: static (mass, x) => {
        LinearVector ones = DenseVectorD.Create(x.Count, static _ => 1.0);
        LinearVector massOnes = mass.Multiply(ones);
        return x - ((massOnes.DotProduct(x) / Math.Max(EpsilonPolicy.SqrtEpsilon, massOnes.DotProduct(ones))) * ones);
    });
    public static readonly GaugeShift MinZero = new(apply: static (_, x) => x - (x.Minimum() * DenseVectorD.Create(x.Count, static _ => 1.0)));
    [UseDelegateFromConstructor] internal partial LinearVector Apply(Matrix<double> mass, LinearVector x);
}
```

```csharp
LinearVector shifted = shift.Apply(mass: mass, x: stage.X);
```

### Effect

- Target fenced LOC: the keyless `GaugeShift` declaration plus its two helpers shrink `19 -> 11` (**-8**); the use-site replacement is count-neutral.
- Authored symbols: **-2 members** net — the duplicate `PinZero` row and two private kernel helpers disappear, while one internal generated behavior member replaces them.
- Logic: each shift row carries the operation it selects; the mean-zero arm computes the ones vector once, and the redundant `rows` parameter disappears because `x.Count` already owns the dimension.

### API and consumer proof

The C# dispatch chooser selects `[UseDelegateFromConstructor]` when the vocabulary item is the behavior. `GaugeShift` is already the closed owner of the normalization policies, and `ApplyShift` only re-dispatches that roster at its sole call. `PinZero` and `None` both return the solution unchanged; pin enforcement and its evidence already come from `GaugePolicy.Pin`, `SolvePin`, and `GaugeFix.PinIndices`, so retaining a second no-op post-shift row creates semantic differentiation with no behavioral or evidentiary fact. Thinktecture's generated delegate method retains row-exhaustive construction: a new shift cannot initialize without supplying its behavior. `MassWeightedMean` has no independent caller or policy and becomes the `MeanZero` row body. `SvdResult.Spectral` and `Condition` deliberately remain: they are standard decomposition facts on the carrier that owns the admitted descending singular values, not caller-side forwarding shells, and zero current consumers does not make that domain surface dead.

### Ripples

- Same file: update the vocabulary prose so `GaugeShift` owns its post-solve normalization behavior rather than advertising a kernel-side `Switch`; remove `PinZero` from the shift roster.
- `libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:17`: replace `GaugePolicy.PinConstant(interior, GaugeShift.PinZero)` with `GaugePolicy.Pinned([interior])` in the Poisson route prose.
- `libs/dotnet/Rasm/.planning/Meshing/reconstruct.md:574`: after move 7, replace `GaugePolicy.Pinned(indices: [0], shift: GaugeShift.PinZero)` with `GaugePolicy.Pinned(indices: [0])`; the factory default is `GaugeShift.None`, and `GaugeFix.PinIndices` retains the applied-pin evidence.

## 10. Remove the lossy boolean restatement from `GaugeFix`

### Location

- `matrix.md:667-673`, anchor `GaugeFix.RhsProjected`

### From

```csharp
public bool RhsProjected => RhsMutationNorm > 0.0;
```

### To

```csharp
```

### Effect

- Target fenced LOC: `1 -> 0` (**-1**).
- Authored symbols: **-1 public member**.
- Evidence: `RhsMutationNorm` remains the quantitative fact; the lossy boolean projection disappears.

### API and consumer proof

The public `RhsMutationNorm` carries strictly more information and is already validated nonnegative inside `GaugeFix.IsValid`; `RhsProjected` adds no independent capability or evidence and hard-codes one threshold interpretation. No current `libs/dotnet/` consumer reads the projection, so removing it has no call-site ripple; a future decision can state its threshold at its own boundary.

### Ripples

None.

## 11. Mint the compressed sparse result where compression finishes

### Location

- `matrix.md:976-982`, anchors `return CompressedOf` and `private static Fin<SparseMatrix> CompressedOf`

### From

```csharp
return CompressedOf(storage: storage, rows: rows, cols: cols, op: op);
}
private static Fin<SparseMatrix> CompressedOf(SparseCompressedRowMatrixStorage<double> storage, Dimension rows, Dimension cols, Op op) {
    SparseMatrix result = SparseMatrix.Trusted(rows: rows, cols: cols,
        rowPtr: new Arr<int>(storage.RowPointers), colInd: new Arr<int>(storage.ColumnIndices[..storage.ValueCount]), values: new Arr<double>(storage.Values[..storage.ValueCount]));
    return result.IsValid ? Fin.Succ(result) : Fin.Fail<SparseMatrix>(op.InvalidResult());
}
```

### To

```csharp
SparseMatrix result = SparseMatrix.Trusted(rows: rows, cols: cols,
    rowPtr: new Arr<int>(storage.RowPointers), colInd: new Arr<int>(storage.ColumnIndices[..storage.ValueCount]), values: new Arr<double>(storage.Values[..storage.ValueCount]));
return result.IsValid ? Fin.Succ(result) : Fin.Fail<SparseMatrix>(op.InvalidResult());
}
```

### Effect

- Target fenced LOC: `7 -> 4` (**-3**).
- Authored symbols: **-1 private member** (`CompressedOf`).
- Logic: duplicate normalization and zero removal still finish before the sole trusted mint and validity gate.

### API and consumer proof

MathNet's CSR storage owns sorting and duplicate/zero normalization immediately above this fragment. `CompressedOf` has one call, contributes no alternate route, and simply forwards all locals already in scope into `SparseMatrix.Trusted`. The resulting public carrier and admission failure are unchanged.

### Ripples

None.

## 12. Inline the one-use real-to-complex dense bridge

### Location

- `matrix.md:794-795`, anchor `ToMathNetComplex`
- `matrix.md:918`, anchor `GeneralEigen`

### From

```csharp
private static DenseMatrixC ToMathNetComplex(Matrix m) =>
    (DenseMatrixC)DenseMatrixC.Build.Dense(m.Rows.Value, m.Cols.Value, (i, j) => new Complex(m.At(i: i, j: j), 0.0));
```

```csharp
Matrix<Complex> mathNet = ToMathNetComplex(matrix);
```

### To

```csharp
```

```csharp
Matrix<Complex> mathNet = DenseMatrixC.Build.Dense(matrix.Rows.Value, matrix.Cols.Value, (i, j) => new Complex(matrix.At(i, j), 0.0));
```

### Effect

- Target fenced LOC: `3 -> 1` (**-2**).
- Authored symbols: **-1 private member** (`ToMathNetComplex`).
- Logic: the conversion stays adjacent to the sole asymmetric complex EVD that requires it.

### API and consumer proof

The MathNet catalogue confirms `DenseMatrix<Complex>.Build.Dense(rows, columns, init)` and complex `Evd(Symmetricity)` as the direct supported surfaces. This helper has one caller, no policy, no validation, and no reuse across the held-handle bridges; inlining removes an otherwise module-level package spelling without changing the public boundary.

### Ripples

None.

## 13. Inline the two one-use LOBPCG projections

### Location

- `matrix.md:1413-1415`, anchors `MaxColumnNorm` and `SurvivingColumns`
- `matrix.md:1457-1459`, anchor `private static int[] SurvivingColumns`
- `matrix.md:1478-1480`, anchor `private static double MaxColumnNorm`

### From

```csharp
bool hasPrevious = iter > 0 && MaxColumnNorm(m: P) > EpsilonPolicy.SqrtEpsilon;
Matrix<T> S = orthonormalise(arg: hasPrevious ? X.Append(W).Append(P) : X.Append(W));
int[] survivors = SurvivingColumns(m: S);
```

```csharp
private static int[] SurvivingColumns<T>(Matrix<T> m)
    where T : struct, IEquatable<T>, IFormattable =>
    [.. Enumerable.Range(start: 0, count: m.ColumnCount).Where(j => m.Column(j).L2Norm() > EpsilonPolicy.SqrtEpsilon)];
```

```csharp
private static double MaxColumnNorm<T>(Matrix<T> m)
    where T : struct, IEquatable<T>, IFormattable =>
    Enumerable.Range(start: 0, count: m.ColumnCount).Aggregate(seed: 0.0, func: (max, j) => Math.Max(max, m.Column(j).L2Norm()));
```

### To

```csharp
bool hasPrevious = iter > 0 && Enumerable.Range(0, P.ColumnCount).Any(j => P.Column(j).L2Norm() > EpsilonPolicy.SqrtEpsilon);
Matrix<T> S = orthonormalise(arg: hasPrevious ? X.Append(W).Append(P) : X.Append(W));
int[] survivors = [.. Enumerable.Range(0, S.ColumnCount).Where(j => S.Column(j).L2Norm() > EpsilonPolicy.SqrtEpsilon)];
```

### Effect

- Target fenced LOC: `9 -> 3` (**-6**).
- Authored symbols: **-2 private members**.
- Logic: the first query becomes the predicate the branch actually needs (`Any`) and short-circuits instead of computing a maximum over every column; the second projection remains a single enumeration at its only use.

### API and consumer proof

Both helpers have one caller in `LobpcgCore` and neither crosses the numeric boundary. `hasPrevious` asks only whether any prior direction is above the floor; `Any` is behaviorally equivalent to `MaxColumnNorm > floor` because every L2 norm is nonnegative, but avoids the full aggregate once a surviving column is found. The survivor list must still enumerate all columns, so its direct collection expression preserves the existing algorithm exactly.

### Ripples

None.

## 14. Replace “rescue” with route and residual terminology

### Location

- `matrix.md:161-163`, anchors `SolvePath.SparseRescue`
- `matrix.md:192-195`, anchors `SolveStop.RescueSolved` and `RescueRejected`
- `matrix.md:1080-1095`, anchors `Rescue(...)` and its path/stop rows
- `matrix.md:1133`, the conditioned sparse-direct rejection

### From

```csharp
public static readonly SolvePath SparseKrylov = new(key: 7,
    traits: SolveTraitLaw.Admit(SolveTrait.Iterative, SolveTrait.Sparse, SolveTrait.Square), cap: ResidualCap.Converged, conditioned: static () => SparseRescue);
public static readonly SolvePath SparseRescue = new(key: 8,
    traits: SolveTraitLaw.Admit(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Square, SolveTrait.Fallback), cap: ResidualCap.Relaxed, conditioned: static () => SparseRescue);
```

```csharp
public static readonly SolveStop RescueSolved = new(isUsable: true);
public static readonly SolveStop RescueRejected = new(isUsable: false);
```

```csharp
? Rescue(matrix: matrix, a: a, b: b, rhs: rhs, key: key)
```

```csharp
private static Fin<LinearSolution> Rescue(SparseMatrix matrix, Matrix<double> a, LinearVector b, Arr<double> rhs, Op key) {
```

```csharp
stop: double.IsFinite(residual) && residual <= SolvePath.SparseRescue.Cap.Floor ? SolveStop.RescueSolved : SolveStop.RescueRejected,
```

```csharp
stop: double.IsFinite(residual) && residual <= SolvePath.SparseLu.Cap.Floor ? SolveStop.DirectSolved : SolveStop.RescueRejected,
```

### To

```csharp
public static readonly SolvePath SparseKrylov = new(key: 7,
    traits: SolveTrait.Admit(SolveTrait.Iterative, SolveTrait.Sparse, SolveTrait.Square), cap: ResidualCap.Converged, conditioned: static () => DenseFallback);
public static readonly SolvePath DenseFallback = new(key: 8,
    traits: SolveTrait.Admit(SolveTrait.Direct, SolveTrait.Sparse, SolveTrait.Square, SolveTrait.Fallback), cap: ResidualCap.Relaxed, conditioned: static () => DenseFallback);
```

```csharp
public static readonly SolveStop ResidualRejected = new(isUsable: false);
```

```csharp
? DenseFallback(matrix: matrix, a: a, b: b, rhs: rhs, key: key)
```

```csharp
private static Fin<LinearSolution> DenseFallback(SparseMatrix matrix, Matrix<double> a, LinearVector b, Arr<double> rhs, Op key) {
```

```csharp
stop: double.IsFinite(residual) && residual <= SolvePath.DenseFallback.Cap.Floor ? SolveStop.DirectSolved : SolveStop.ResidualRejected,
```

```csharp
stop: double.IsFinite(residual) && residual <= SolvePath.SparseLu.Cap.Floor ? SolveStop.DirectSolved : SolveStop.ResidualRejected,
```

Apply the same exact `SparseRescue -> DenseFallback` replacement to the remaining route reads in this fence.

### Effect

- Target fenced LOC: `2 -> 1` across the stop roster (**-1**); route and use-site renames are count-neutral.
- Authored symbols: **-1 public row member**; fallback success reuses `DirectSolved`, while `ResidualRejected` names the shared factual failure at both dense-fallback and sparse-LU exits.
- Naming: the route states the actual algorithmic transition—iterative sparse solve to direct dense fallback—and “rescue” ceases to be an unexplained local metaphor.

### API and consumer proof

The target itself names the old action `Densify` and constructs the fallback only after the iterative route fails. `Fallback` is already the formal capability (`SolveTrait.Fallback`), so `DenseFallback` states the route without a coined synonym. The carried `SolvePath` already distinguishes an ordinary direct solve from the direct fallback, making a second successful stop row purely semantic duplication. Conversely, the same failed residual gate is already shared by the fallback and `SparseLu`; `ResidualRejected` accurately names both where `FallbackRejected` would lie about the latter. No outside `libs/dotnet/` consumer references any renamed or removed row.

### Ripples

- Same file: replace “rescue” with “dense fallback” in sparse-solve prose, density rows, and comments; replace the non-usable-stop prose's `RescueRejected` with `ResidualRejected`.
- Outside target: none.

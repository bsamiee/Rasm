# 1. Make the mode row the sole operation discriminant

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:69`**

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SimplifyKind {
    public static readonly SimplifyKind QuadricCollapse = new("quadric-collapse", traits: CapabilitySet<SimplifyTrait>.Of(SimplifyTrait.Topology), weigh: static (op, context, key, plane) => Simplify.Uniform(plane));
    public static readonly SimplifyKind ProgressiveMesh = new("progressive-mesh", traits: CapabilitySet<SimplifyTrait>.Of(SimplifyTrait.Reversible, SimplifyTrait.Topology), weigh: Simplify.Curvature);
    public static readonly SimplifyKind VoxelRemesh     = new("voxel-remesh", traits: CapabilitySet<SimplifyTrait>.Of(SimplifyTrait.Resample), weigh: static (op, context, key, plane) => Simplify.Uniform(plane));
    public static readonly SimplifyKind FeaturePreserve = new("feature-preserve", traits: CapabilitySet<SimplifyTrait>.Of(SimplifyTrait.Reversible, SimplifyTrait.Topology), weigh: Simplify.FeaturePins);

    public CapabilitySet<SimplifyTrait> Traits { get; }

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Weigh(SimplifyOp op, Context context, Op key, Memory<double> plane);
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:290`**

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SimplifyOp {
    private SimplifyOp() { }

    public sealed record QuadricCollapse(MeshSpace Mesh, SimplifyPolicy Policy) : SimplifyOp;
    public sealed record ProgressiveMesh(MeshSpace Mesh, SimplifyPolicy Policy) : SimplifyOp;
    public sealed record VoxelRemesh(MeshSpace Mesh, SimplifyPolicy Policy) : SimplifyOp;
    public sealed record FeaturePreserve(MeshSpace Mesh, SimplifyPolicy Policy) : SimplifyOp;

    public SimplifyKind Kind =>
        Switch(
            quadricCollapse: static _ => SimplifyKind.QuadricCollapse,
            progressiveMesh: static _ => SimplifyKind.ProgressiveMesh,
            voxelRemesh:     static _ => SimplifyKind.VoxelRemesh,
            featurePreserve: static _ => SimplifyKind.FeaturePreserve);

    public MeshSpace Mesh =>
        Switch(
            quadricCollapse: static q => q.Mesh, progressiveMesh: static p => p.Mesh,
            voxelRemesh:     static v => v.Mesh, featurePreserve: static f => f.Mesh);

    public SimplifyPolicy Policy =>
        Switch(
            quadricCollapse: static q => q.Policy, progressiveMesh: static p => p.Policy,
            voxelRemesh:     static v => v.Policy, featurePreserve: static f => f.Policy);
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:324`**

```csharp
public static Fin<DecimationResult> Apply(SimplifyOp op, Op? key = null) {
    Op token = key.OrDefault();
    Context context = op.Mesh.Tolerance;
    return Resample(op, context, token).Bind(space => {
        MeshEdit edit = MeshEdit.Of(space);
        try {
            using QuadricStore store = QuadricStore.Seed(edit);
            int budget = op.Policy.Budget.For(store.Live);
            return store.Live == 0
                ? Fin.Fail<DecimationResult>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "decimation: no live faces"))
                : Collapse(store, edit, op, budget, context, token)
                    .Bind(_ => Emit(store, edit, op, budget, context, token));
        }
        finally { edit.Dispose(); }
    });
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:341`**

```csharp
static Fin<MeshSpace> Resample(SimplifyOp op, Context context, Op key) =>
    op.Kind.Traits.Admits(SimplifyTrait.Resample) ? Voxelize(op.Mesh, op.Policy, context, key) : Fin.Succ(op.Mesh);
```

**To**

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SimplifyMode {
    public static readonly SimplifyMode QuadricCollapse = new("quadric-collapse", traits: CapabilitySet<SimplifyTrait>.Of(SimplifyTrait.Topology), weigh: static (op, context, key, plane) => Simplify.Uniform(plane));
    public static readonly SimplifyMode ProgressiveMesh = new("progressive-mesh", traits: CapabilitySet<SimplifyTrait>.Of(SimplifyTrait.Reversible, SimplifyTrait.Topology), weigh: Simplify.Curvature);
    public static readonly SimplifyMode VoxelRemesh = new("voxel-remesh", traits: CapabilitySet<SimplifyTrait>.Of(SimplifyTrait.Resample), weigh: static (op, context, key, plane) => Simplify.Uniform(plane));
    public static readonly SimplifyMode FeaturePreserve = new("feature-preserve", traits: CapabilitySet<SimplifyTrait>.Of(SimplifyTrait.Reversible, SimplifyTrait.Topology), weigh: Simplify.FeaturePins);
    public CapabilitySet<SimplifyTrait> Traits { get; }
    [UseDelegateFromConstructor]
    public partial Fin<Unit> Weigh(SimplifyOp op, Context context, Op key, Memory<double> plane);
}
```

```csharp
public sealed record SimplifyOp(MeshSpace Mesh, SimplifyPolicy Policy, SimplifyMode Mode);
```

```csharp
public static Fin<DecimationResult> Apply(SimplifyOp op, Op? key = null) {
    Op token = key.OrDefault();
    Context context = op.Mesh.Tolerance;
    return (op.Mode.Traits.Admits(SimplifyTrait.Resample)
            ? Voxelize(op.Mesh, op.Policy, context, token)
            : Fin.Succ(op.Mesh))
        .Bind(space => {
            using MeshEdit edit = MeshEdit.Of(space);
            using QuadricStore store = QuadricStore.Seed(edit);
            int target = op.Policy.Target.Count(store.Live);
            return store.Live == 0
                ? Fin.Fail<DecimationResult>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "decimation: no live faces"))
                : Collapse(store, edit, op, target, context, token)
                    .Bind(_ => Emit(store, edit, op, target, context, token));
        });
}
```

```csharp
// Resample DELETED
```

**Why**

The four operation cases carry identical `(Mesh, Policy)` payloads and exist only to mirror the row that already owns every behavioral difference. The union adds four nested types and three exhaustive projections without adding a distinct operation shape. `Resample` is a one-call forwarding member.

**Change**

Rename the behavioral row to `SimplifyMode`, replace the duplicate-payload union with one inert request record carrying that mode, replace every `Kind` read with `Mode`, inline resampling at the entry, and express both arena lifetimes with `using` declarations. Update the owner, entry, laws, output, growth, and diagram text to name the mode row.

**Delta**

Code-fence LOC: `-23` net. Authored types: `-4` nested operation cases. Authored members: `-4` net (`Kind`, `Mesh`, `Policy`, and `Resample` removed). Capability is unchanged.

**Ripples**

`libs/dotnet/Rasm/RULINGS.md:118` must name `SimplifyMode.VoxelRemesh` instead of `SimplifyKind.VoxelRemesh`. `libs/dotnet/Rasm/.planning/Processing/intent.md` continues to consume `SimplifyOp` directly and needs no adapter.

# 2. Collapse the face-target hierarchy into one generated value

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:85`**

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SimplifyBudget {
    private SimplifyBudget() { }

    public sealed record Fraction(UnitInterval Ratio) : SimplifyBudget;
    public sealed record Faces(Dimension Count) : SimplifyBudget;

    public int For(int sourceFaces) =>
        Switch(
            state: sourceFaces,
            fraction: static (source, budget) => Math.Max(4, (int)Math.Round(budget.Ratio.Value * source)),
            faces:    static (source, budget) => Math.Min(budget.Count.Value, source));
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:109`**

```csharp
public SimplifyBudget Budget { get; }
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:272`**

```csharp
int RequestedFaces,
```

**To**

```csharp
[Union<UnitInterval, Dimension>(T1Name = "Fraction", T2Name = "Faces")]
public readonly partial struct FaceTarget {
    internal int Count(int sourceFaces) => Switch(
        state: sourceFaces,
        fraction: static (source, ratio) => Math.Clamp((int)Math.Round(ratio.Value * source), Math.Min(4, source), source),
        faces: static (source, count) => Math.Min(count.Value, source));
}
```

```csharp
public FaceTarget Target { get; }
```

```csharp
int TargetFaces,
```

**Why**

The budget is only a choice between two already-admitted scalar owners. A regular-union base, private constructor, and two nested records add no identity or behavior beyond Thinktecture's generated two-value union. `Budget` and `RequestedFaces` misname a desired output extent, and the fraction arm can report four faces for a source containing fewer than four.

**Change**

Replace the hierarchy with `FaceTarget`, rename `For` to internal `Count`, clamp the fraction result to the source extent, and ripple `Budget`/`RequestedFaces` to `Target`/`TargetFaces` throughout the target fence. Gate the generated struct's no-case default in policy admission.

**Delta**

Code-fence LOC: `-4` net. Authored types: `-2` net (three regular-union types replaced by one ad-hoc union). Authored member count is neutral.

# 3. Accumulate independent policy admission failures

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:130`**

```csharp
public static Fin<SimplifyPolicy> Of(
    Option<SimplifyBudget> budget = default, Option<double> hausdorffCeiling = default,
    Option<double> boundaryPenalty = default, Option<double> featurePinWeight = default,
    Option<double> creaseDihedral = default, Option<double> curvatureGain = default,
    Option<Dimension> voxelResolution = default, Option<Dimension> hausdorffSamplesPerFace = default,
    Option<Dimension> collapsePasses = default, Option<long> seed = default, Op? key = null) {
    Op op = key.OrDefault();
    return from penalty in Magnitude(op, boundaryPenalty, Canonical.BoundaryPenalty)
           from pin in Magnitude(op, featurePinWeight, Canonical.FeaturePinWeight)
           from gain in Magnitude(op, curvatureGain, Canonical.CurvatureGain)
           from ceiling in hausdorffCeiling.Match(
               Some: value => op.AcceptValidated<PositiveMagnitude>(candidate: value).Map(Some),
               None: () => Fin.Succ(Canonical.HausdorffCeiling))
           from crease in creaseDihedral.Match(
               Some: value => op.AcceptValidated<VectorAngle>(candidate: value),
               None: () => Fin.Succ(Canonical.CreaseDihedral))
           from _ in guard(crease.Value < Math.PI, op.InvalidInput())
           let cells = voxelResolution.IfNone(Canonical.VoxelResolution)
           from __ in guard(cells.Value >= 2, op.InvalidInput())
           select new SimplifyPolicy(budget.IfNone(Canonical.Budget), ceiling, penalty, pin, crease, gain, cells,
               hausdorffSamplesPerFace.IfNone(Canonical.HausdorffSamplesPerFace),
               collapsePasses.IfNone(Canonical.CollapsePasses), seed.IfNone(Canonical.Seed));
    static Fin<PositiveMagnitude> Magnitude(Op op, Option<double> candidate, PositiveMagnitude fallback) =>
        candidate.Match(Some: value => op.AcceptValidated<PositiveMagnitude>(candidate: value), None: () => Fin.Succ(fallback));
}
```

**To**

```csharp
public static Fin<SimplifyPolicy> Of(
    Option<FaceTarget> target = default, Option<double> hausdorffCeiling = default, Option<double> boundaryPenalty = default,
    Option<double> featurePinWeight = default, Option<double> creaseDihedral = default, Option<double> curvatureGain = default,
    Option<Dimension> voxelResolution = default, Option<Dimension> hausdorffSamplesPerFace = default,
    Option<Dimension> collapsePasses = default, Option<long> seed = default, Op? key = null) {
    Op op = key.OrDefault();
    return (boundaryPenalty.Traverse(value => op.AcceptValidated<PositiveMagnitude>(candidate: value).ToValidation()).As(),
            featurePinWeight.Traverse(value => op.AcceptValidated<PositiveMagnitude>(candidate: value).ToValidation()).As(),
            curvatureGain.Traverse(value => op.AcceptValidated<PositiveMagnitude>(candidate: value).ToValidation()).As(),
            hausdorffCeiling.Traverse(value => op.AcceptValidated<PositiveMagnitude>(candidate: value).ToValidation()).As(),
            creaseDihedral.Traverse(value => op.AcceptValidated<VectorAngle>(candidate: value).ToValidation()).As())
        .Apply(static (penalty, pin, gain, ceiling, crease) => (Penalty: penalty, Pin: pin, Gain: gain, Ceiling: ceiling, Crease: crease))
        .As().ToFin()
        .Bind(admitted => {
            FaceTarget selected = target.IfNone(Canonical.Target);
            VectorAngle angle = admitted.Crease.IfNone(Canonical.CreaseDihedral);
            (Dimension cells, Dimension samples) = (voxelResolution.IfNone(Canonical.VoxelResolution), hausdorffSamplesPerFace.IfNone(Canonical.HausdorffSamplesPerFace));
            return from _ in guard((selected.IsFraction || selected.IsFaces) && angle.Value < Math.PI && cells.Value >= 2
                                   && samples.Value >= 1, op.InvalidInput())
                   select new SimplifyPolicy(selected, admitted.Ceiling, admitted.Penalty.IfNone(Canonical.BoundaryPenalty),
                       admitted.Pin.IfNone(Canonical.FeaturePinWeight), angle, admitted.Gain.IfNone(Canonical.CurvatureGain),
                       cells, samples, collapsePasses.IfNone(Canonical.CollapsePasses), seed.IfNone(Canonical.Seed));
        });
}
```

```csharp
// Magnitude DELETED
```

**Why**

Five independent raw scalar admissions are hand-threaded through `Fin`, so the first invalid option hides every later invalid option. `Magnitude` only renames generated value-object admission. The current dependent gate also admits a forged zero sample count, which reaches an empty Hausdorff plane.

**Change**

Traverse each optional raw scalar into `Validation<Error, Option<_>>`, join all five with tuple `Apply`, and lower once to `Fin`. Apply defaults only after successful accumulation, then gate the dependent target-case, crease, voxel, and sampling constraints before constructing the policy. Delete `Magnitude`.

**Delta**

Code-fence LOC: `-1` net. Authored symbols: `-1` local helper. Module-level types and members are unchanged; five independent failures now accumulate without a second validation layer.

# 4. Let one complete priority queue own termination

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:103`**

```csharp
Dimension voxelResolution, Dimension hausdorffSamplesPerFace, Dimension collapsePasses, long seed) =>
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:117`**

```csharp
public Dimension CollapsePasses { get; }
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:127`**

```csharp
voxelResolution: Dimension.Create(value: 128), hausdorffSamplesPerFace: Dimension.Create(value: 1),
collapsePasses: Dimension.Create(value: 16), seed: 0x5EED);
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:100`**

```csharp
private SimplifyPolicy(
    SimplifyBudget budget, Option<PositiveMagnitude> hausdorffCeiling, PositiveMagnitude boundaryPenalty,
    PositiveMagnitude featurePinWeight, VectorAngle creaseDihedral, PositiveMagnitude curvatureGain,
    Dimension voxelResolution, Dimension hausdorffSamplesPerFace, Dimension collapsePasses, long seed) =>
    (Budget, HausdorffCeiling, BoundaryPenalty, FeaturePinWeight, CreaseDihedral, CurvatureGain,
        VoxelResolution, HausdorffSamplesPerFace, CollapsePasses, Seed) =
    (budget, hausdorffCeiling, boundaryPenalty, featurePinWeight, creaseDihedral, curvatureGain,
        voxelResolution, hausdorffSamplesPerFace, collapsePasses, seed);
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:135`**

```csharp
Option<Dimension> collapsePasses = default, Option<long> seed = default, Op? key = null) {
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:149`**

```csharp
select new SimplifyPolicy(budget.IfNone(Canonical.Budget), ceiling, penalty, pin, crease, gain, cells,
    hausdorffSamplesPerFace.IfNone(Canonical.HausdorffSamplesPerFace),
    collapsePasses.IfNone(Canonical.CollapsePasses), seed.IfNone(Canonical.Seed));
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:345`**

```csharp
static Fin<Unit> Collapse(QuadricStore store, MeshEdit edit, SimplifyOp op, int budget, Context context, Op key) {
    using MemoryOwner<double> weights = MemoryOwner<double>.Allocate(edit.VertexCount, AllocationMode.Clear);
    return op.Kind.Weigh(op, context, key, weights.Memory).Bind(_ => {
        Accumulate(store, edit, weights.Memory, op.Policy);
        Atom<bool> settled = Atom(value: store.Live <= budget);
        Transition<bool> driven = Cell.Converge(
            cell: settled,
            step: done => Some(done || CollapsePass()),
            settled: static done => done,
            budget: op.Policy.CollapsePasses,
            declined: key.InvalidResult());
        return driven.Current && store.Live <= budget
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.FaceBudgetMissed(budget, store.Live));
        bool CollapsePass() {
            EnqueueAll(store, edit, key);
            Drain(store, edit, budget, key);
            return store.Live <= budget || NoAdmissibleCollapse(store, edit, key);
        }
    });
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:417`**

```csharp
static IEnumerable<(int U, int V)> LiveEdges(QuadricStore store, MeshEdit edit) {
    for (int u = 0; u < edit.VertexCount; u++) {
        if (!store.Alive(u)) continue;
        foreach (int w in store.Ring[u]) {
            if (w > u && store.Alive(w)) yield return (u, w);
        }
    }
}
static void EnqueueAll(QuadricStore store, MeshEdit edit, Op key) {
    foreach ((int u, int w) in LiveEdges(store, edit)) { Enqueue(store, edit, u, w, key); }
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:436`**

```csharp
static void Drain(QuadricStore store, MeshEdit edit, int budget, Op key) {
    while (store.Live > budget && store.Pq.TryDequeue(out EdgeRef edge, out double _)) {
        if (Stale(store, edge)) continue;
        if (!CollapseValid(store, edit, edge.U, edge.V, edge.Target)) continue;
        ApplyCollapse(store, edit, edge, key);
    }
}
static bool Stale(QuadricStore store, EdgeRef edge) =>
    !store.Alive(edge.U) || !store.Alive(edge.V)
    || store.Versions[edge.U] != edge.VersionU || store.Versions[edge.V] != edge.VersionV;
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:489`**

```csharp
foreach (int w in store.Ring[v]) {
    store.Ring[w].Remove(v);
    if (w != u) { store.Ring[w].Add(u); store.Ring[u].Add(w); store.Versions[w]++; }
}
store.Ring[u].Remove(v);
store.Ring[v].Clear();
store.Quadrics[u] = store.Quadrics[u].Add(store.Quadrics[v]);
store.Kill(v);
store.Versions[u]++;
foreach (int w in store.Ring[u]) {
    if (store.Alive(w)) Enqueue(store, edit, u, w, key);
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:503`**

```csharp
static bool NoAdmissibleCollapse(QuadricStore store, MeshEdit edit, Op key) =>
    !LiveEdges(store, edit).Any(row => {
        (Point3d target, double _, PositionRoute __) = OptimalPosition(store.Quadrics[row.U].Add(store.Quadrics[row.V]), edit.Position(row.U), edit.Position(row.V), key);
        return CollapseValid(store, edit, row.U, row.V, target);
    });
```

**To**

```csharp
// SimplifyPolicy.CollapsePasses DELETED
```

```csharp
private SimplifyPolicy(
    FaceTarget target, Option<PositiveMagnitude> hausdorffCeiling, PositiveMagnitude boundaryPenalty,
    PositiveMagnitude featurePinWeight, VectorAngle creaseDihedral, PositiveMagnitude curvatureGain,
    Dimension voxelResolution, Dimension hausdorffSamplesPerFace, long seed) =>
    (Target, HausdorffCeiling, BoundaryPenalty, FeaturePinWeight, CreaseDihedral, CurvatureGain,
        VoxelResolution, HausdorffSamplesPerFace, Seed) =
    (target, hausdorffCeiling, boundaryPenalty, featurePinWeight, creaseDihedral, curvatureGain,
        voxelResolution, hausdorffSamplesPerFace, seed);
```

```csharp
voxelResolution: Dimension.Create(value: 128), hausdorffSamplesPerFace: Dimension.Create(value: 1),
seed: 0x5EED);
```

```csharp
Option<long> seed = default, Op? key = null) {
```

```csharp
select new SimplifyPolicy(selected, admitted.Ceiling, admitted.Penalty.IfNone(Canonical.BoundaryPenalty),
    admitted.Pin.IfNone(Canonical.FeaturePinWeight), angle, admitted.Gain.IfNone(Canonical.CurvatureGain), cells, samples, seed.IfNone(Canonical.Seed));
```

```csharp
static Fin<Unit> Collapse(QuadricStore store, MeshEdit edit, SimplifyOp op, int target, Context context, Op key) {
    using MemoryOwner<double> weights = MemoryOwner<double>.Allocate(edit.VertexCount, AllocationMode.Clear);
    return op.Mode.Weigh(op, context, key, weights.Memory).Bind(_ => {
        Accumulate(store, edit, weights.Memory, op.Policy);
        for (int u = 0; u < edit.VertexCount; u++) {
            if (!store.Alive(u)) continue;
            foreach (int v in store.Ring[u]) {
                if (v > u && store.Alive(v)) Enqueue(store, edit, u, v, key);
            }
        }
        while (store.Live > target && store.Pq.TryDequeue(out Edge edge, out double _)) {
            if (!store.Alive(edge.U) || !store.Alive(edge.V)
                || store.Versions[edge.U] != edge.VersionU || store.Versions[edge.V] != edge.VersionV) continue;
            if (CollapseValid(store, edit, edge.U, edge.V, edge.Target)) ApplyCollapse(store, edit, edge, key);
        }
        return store.Live <= target
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new GeometryFault.FaceBudgetMissed(target, store.Live));
    });
}
```

```csharp
// LiveEdges DELETED
// EnqueueAll DELETED
// Drain DELETED
// Stale DELETED
```

```csharp
foreach (int w in store.Ring[v]) {
    store.Ring[w].Remove(v);
    if (w != u) { store.Ring[w].Add(u); store.Ring[u].Add(w); }
}
store.Ring[u].Remove(v);
store.Ring[v].Clear();
store.Quadrics[u] += store.Quadrics[v];
store.Kill(v);
store.Versions[u]++;
foreach (int w in store.Ring[u]) {
    if (store.Alive(w)) Enqueue(store, edit, u, w, key);
}
```

```csharp
// NoAdmissibleCollapse DELETED
```

**Why**

The pass/fixpoint layer compensates for an incomplete queue model with global edge rescans and a second full admissibility probe. A collapse changes only the survivor's quadric and incident edge set. Incrementing every former neighbor's version incorrectly discards still-current neighbor-to-neighbor costs; those edges already remain in the initial queue and re-run topology admission when dequeued.

**Change**

Seed every live edge once, keep unaffected neighbor versions stable, increment only the survivor version after its topology and quadric update, and enqueue the survivor's complete current ring at that point. Inline the sole queue drain into `Collapse`; queue exhaustion proves that no queued candidate can meet the target. Remove the public pass budget, `Cell.Converge`, both rescans, and the stale wrapper. Update the law and diagram from bounded fixpoint passes to queue exhaustion.

**Delta**

Code-fence LOC: `-30` net. Authored members: `-6` (`CollapsePasses`, `LiveEdges`, `EnqueueAll`, `Drain`, `Stale`, and `NoAdmissibleCollapse`). No type or capability is added or removed.

# 5. Internalize and compress the quadric kernel

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:58`**

```csharp
[SmartEnum<int>]
public sealed partial class PositionRoute {
    public static readonly PositionRoute Optimal = new(key: 0);
    public static readonly PositionRoute Midpoint = new(key: 1);
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:161`**

```csharp
public readonly record struct EdgeRef(int U, int V, int VersionU, int VersionV, Point3d Target, double Cost, PositionRoute Route);
public readonly record struct FacePlane(double A, double B, double C, double D, double W);
public readonly record struct Quadric(
    ddouble A00, ddouble A01, ddouble A02, ddouble A03,
    ddouble A11, ddouble A12, ddouble A13,
    ddouble A22, ddouble A23, ddouble A33) {
    public static readonly Quadric Zero = default;
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:176`**

```csharp
public Quadric Add(Quadric o) =>
    new(A00 + o.A00, A01 + o.A01, A02 + o.A02, A03 + o.A03,
        A11 + o.A11, A12 + o.A12, A13 + o.A13,
        A22 + o.A22, A23 + o.A23, A33 + o.A33);
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:190`**

```csharp
public sealed class QuadricStore : IDisposable {
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:198`**

```csharp
internal readonly PriorityQueue<EdgeRef, double> Pq = new();
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:241`**

```csharp
static void Bump(Dictionary<long, (int, int)> fan, int a, int b, int f) {
    long key = EdgeKey(a, b);
    fan[key] = fan.TryGetValue(key, out (int Count, int Face) row) ? (row.Count + 1, row.Face) : (1, f);
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:249`**

```csharp
public bool Alive(int v) => valid.Span[v];
public bool OnBoundary(int v) => boundaryVertex.Span[v];
public void Kill(int v) => valid.Span[v] = false;

public int SharedLink(int u, int v) {
    (IndexSet small, IndexSet large) = Ring[u].Count <= Ring[v].Count ? (Ring[u], Ring[v]) : (Ring[v], Ring[u]);
    return small.Count(large.Contains);
}

public int EdgeFaces(int u, int v) {
    (IndexSet small, IndexSet large) = Incident[u].Count <= Incident[v].Count ? (Incident[u], Incident[v]) : (Incident[v], Incident[u]);
    return small.Count(large.Contains);
}

public static long EdgeKey(int u, int v) { (int lo, int hi) = u < v ? (u, v) : (v, u); return ((long)lo << 32) | (uint)hi; }
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:401`**

```csharp
static void Boundaries(QuadricStore store, MeshEdit edit, ReadOnlyMemory<FacePlane> planes, SimplifyPolicy policy) {
    foreach ((int u, int v, int face) in store.BoundaryEdges) {
        FacePlane p = planes.Span[face];
        if (p.W <= 0.0) continue;
        (Point3d pu, Point3d pv) = (edit.Position(u), edit.Position(v));
        Vector3d constraint = Vector3d.CrossProduct(pv - pu, new Vector3d(p.A, p.B, p.C));
        double len = constraint.Length;
        if (len <= 0.0) continue;
        constraint = (1.0 / len) * constraint;
        double d = -(constraint.X * pu.X + constraint.Y * pu.Y + constraint.Z * pu.Z);
        Quadric k = Quadric.OfPlane(constraint.X, constraint.Y, constraint.Z, d, policy.BoundaryPenalty.Value);
        store.Quadrics[u] = store.Quadrics[u].Add(k);
        store.Quadrics[v] = store.Quadrics[v].Add(k);
    }
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:368`**

```csharp
static void Accumulate(QuadricStore store, MeshEdit edit, ReadOnlyMemory<double> weights, SimplifyPolicy policy) {
    using MemoryOwner<FacePlane> planes = MemoryOwner<FacePlane>.Allocate(edit.FaceCount, AllocationMode.Clear);
    edit.Parallel(edit.FaceCount, new PlanePass(edit, weights, planes.Memory));
    edit.Parallel(edit.VertexCount, new QuadricPass(store, planes.Memory));
    Boundaries(store, edit, planes.Memory, policy);
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:390`**

```csharp
Quadric q = Quadric.Zero;
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:430`**

```csharp
static void Enqueue(QuadricStore store, MeshEdit edit, int u, int v, Op key) {
    if (!store.Alive(u) || !store.Alive(v)) return;
    (Point3d target, double cost, PositionRoute route) = OptimalPosition(store.Quadrics[u].Add(store.Quadrics[v]), edit.Position(u), edit.Position(v), key);
    store.Pq.Enqueue(new EdgeRef(u, v, store.Versions[u], store.Versions[v], target, cost, route), cost);
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:448`**

```csharp
int fan = store.EdgeFaces(u, v);
int shared = store.SharedLink(u, v);
bool link = fan switch {
    2 => shared == 2 && !(store.OnBoundary(u) && store.OnBoundary(v)),
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:472`**

```csharp
static void ApplyCollapse(QuadricStore store, MeshEdit edit, EdgeRef edge, Op key) {
    (int u, int v, Point3d target) = (edge.U, edge.V, edge.Target);
    if (edge.Route.Equals(PositionRoute.Midpoint)) { store.Midpoints++; }
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:496`**

```csharp
store.Kill(v);
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:510`**

```csharp
static (Point3d Target, double Cost, PositionRoute Route) OptimalPosition(Quadric q, Point3d u, Point3d v, Op key) {
    Fin<Arr<double>> solve = SymmetricMatrix.Of(
            dim: Dimension.Create(3),
            upper: new Arr<double>([(double)q.A00, (double)q.A01, (double)q.A02, (double)q.A11, (double)q.A12, (double)q.A22]),
            key: key)
        .Bind(spd => spd.DecomposeCholesky(key: key))
        .Bind(chol => chol.SolveDetailed(new Arr<double>([(double)(-q.A03), (double)(-q.A13), (double)(-q.A23)]), key))
        .Map(static solve => solve.Solution);
    return solve.Match(
        Succ: x => { Point3d p = new(x[0], x[1], x[2]); return (p, q.Evaluate(p), PositionRoute.Optimal); },
        Fail: _ => { Point3d p = new(0.5 * (u.X + v.X), 0.5 * (u.Y + v.Y), 0.5 * (u.Z + v.Z)); return (p, q.Evaluate(p), PositionRoute.Midpoint); });
}
```

**To**

```csharp
// PositionRoute DELETED
```

```csharp
private readonly record struct Edge(int U, int V, int VersionU, int VersionV, Point3d Target, double Cost, bool UsedMidpoint);
private readonly record struct FacePlane(double A, double B, double C, double D, double W);
private readonly record struct Quadric(
    ddouble A00, ddouble A01, ddouble A02, ddouble A03,
    ddouble A11, ddouble A12, ddouble A13,
    ddouble A22, ddouble A23, ddouble A33) {
```

```csharp
public static Quadric operator +(Quadric left, Quadric right) =>
    new(left.A00 + right.A00, left.A01 + right.A01, left.A02 + right.A02, left.A03 + right.A03,
        left.A11 + right.A11, left.A12 + right.A12, left.A13 + right.A13,
        left.A22 + right.A22, left.A23 + right.A23, left.A33 + right.A33);
```

```csharp
private sealed class QuadricStore : IDisposable {
```

```csharp
internal readonly PriorityQueue<Edge, double> Pq = new();
```

```csharp
static void Bump(Dictionary<long, (int, int)> fan, int a, int b, int f) {
    (int lo, int hi) = a < b ? (a, b) : (b, a);
    long key = ((long)lo << 32) | (uint)hi;
    fan[key] = fan.TryGetValue(key, out (int Count, int Face) row) ? (row.Count + 1, row.Face) : (1, f);
}
```

```csharp
public bool Alive(int v) => valid.Span[v];
```

```csharp
// Quadric.Zero DELETED
// QuadricStore.OnBoundary DELETED
// QuadricStore.Kill DELETED
// QuadricStore.SharedLink DELETED
// QuadricStore.EdgeFaces DELETED
// QuadricStore.EdgeKey DELETED
// Boundaries DELETED
// OptimalPosition DELETED
```

```csharp
Quadric q = default;
```

```csharp
static void Accumulate(QuadricStore store, MeshEdit edit, ReadOnlyMemory<double> weights, SimplifyPolicy policy) {
    using MemoryOwner<FacePlane> planes = MemoryOwner<FacePlane>.Allocate(edit.FaceCount, AllocationMode.Clear);
    edit.Parallel(edit.FaceCount, new PlanePass(edit, weights, planes.Memory));
    edit.Parallel(edit.VertexCount, new QuadricPass(store, planes.Memory));
    foreach ((int u, int v, int face) in store.BoundaryEdges) {
        FacePlane p = planes.Span[face];
        if (p.W <= 0.0) continue;
        (Point3d pu, Point3d pv) = (edit.Position(u), edit.Position(v));
        Vector3d constraint = Vector3d.CrossProduct(pv - pu, new Vector3d(p.A, p.B, p.C));
        double len = constraint.Length;
        if (len <= 0.0) continue;
        constraint = (1.0 / len) * constraint;
        double d = -(constraint.X * pu.X + constraint.Y * pu.Y + constraint.Z * pu.Z);
        Quadric boundary = Quadric.OfPlane(constraint.X, constraint.Y, constraint.Z, d, policy.BoundaryPenalty.Value);
        store.Quadrics[u] += boundary;
        store.Quadrics[v] += boundary;
    }
}
```

```csharp
static void Enqueue(QuadricStore store, MeshEdit edit, int u, int v, Op key) {
    if (!store.Alive(u) || !store.Alive(v)) return;
    Quadric q = store.Quadrics[u] + store.Quadrics[v];
    (Point3d a, Point3d b) = (edit.Position(u), edit.Position(v));
    Fin<Arr<double>> solve = SymmetricMatrix.Of(
            dim: Dimension.Create(3),
            upper: new Arr<double>([(double)q.A00, (double)q.A01, (double)q.A02, (double)q.A11, (double)q.A12, (double)q.A22]),
            key: key)
        .Bind(matrix => matrix.DecomposeCholesky(key: key))
        .Bind(cholesky => cholesky.SolveDetailed(new Arr<double>([(double)(-q.A03), (double)(-q.A13), (double)(-q.A23)]), key))
        .Map(static result => result.Solution);
    (Point3d target, bool midpoint) = solve.Match(
        Succ: x => (new Point3d(x[0], x[1], x[2]), false),
        Fail: _ => (new Point3d(0.5 * (a.X + b.X), 0.5 * (a.Y + b.Y), 0.5 * (a.Z + b.Z)), true));
    double cost = q.Evaluate(target);
    store.Pq.Enqueue(new Edge(u, v, store.Versions[u], store.Versions[v], target, cost, midpoint), cost);
}
```

```csharp
int fan = Shared(store.Incident, u, v);
int shared = Shared(store.Ring, u, v);
bool link = fan switch {
    2 => shared == 2 && !(store.boundaryVertex.Span[u] && store.boundaryVertex.Span[v]),
```

```csharp
static int Shared(IndexSet[] graph, int a, int b) {
    (IndexSet small, IndexSet large) = graph[a].Count <= graph[b].Count ? (graph[a], graph[b]) : (graph[b], graph[a]);
    return small.Count(large.Contains);
}
```

```csharp
static void ApplyCollapse(QuadricStore store, MeshEdit edit, Edge edge, Op key) {
    (int u, int v, Point3d target) = (edge.U, edge.V, edge.Target);
    if (edge.UsedMidpoint) store.Midpoints++;
```

```csharp
store.valid.Span[v] = false;
```

**Why**

`PositionRoute` is a generated public type used only as a midpoint counter bit. `EdgeRef`, `FacePlane`, `Quadric`, and `QuadricStore` are private implementation state exposed at module level. `Zero`, `Kill`, `OnBoundary`, `EdgeKey`, the two identical intersection methods, `Boundaries`, and `OptimalPosition` are one-operation or one-call wrappers around code that has a single coherent owner.

**Change**

Nest the four surviving kernel types privately under `Simplify`, rename `EdgeRef` to `Edge`, carry a `UsedMidpoint` bit, replace quadric `.Add` with the natural `+` operator, and use `default` for zero. Inline boundary accumulation into `Accumulate`, the key encoding into `Bump`, and the sole optimal-position solve into `Enqueue`. Replace the duplicated link-count methods with one local `Shared` fold and use the private validity/boundary planes directly for kill and boundary reads.

**Delta**

Code-fence LOC: `-15` net. Module-level types: `-5`; total authored types: `-1`. Authored members: `-8` net after the `+` operator and local `Shared` replace the deleted wrappers.

# 6. Measure lost geometry against the simplified mesh

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:580`**

```csharp
edit.ToSpace(key).Bind(space =>
    Hausdorff(edit, op.Mesh, op.Policy, key).Bind(bound =>
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:596`**

```csharp
static Fin<double> Hausdorff(MeshEdit lod, MeshSpace source, SimplifyPolicy policy, Op key) {
    MeshEdit src = MeshEdit.Of(source);
    try {
        BoundingBox[] boxes = new BoundingBox[src.FaceCount];
        for (int f = 0; f < src.FaceCount; f++) boxes[f] = src.Bounds(f);
        return SpatialIndex.Build(SpatialKind.Bvh, boxes, BuildPolicy.Canonical, key)
            .Bind(index => {
                int count = lod.FaceCount * policy.HausdorffSamplesPerFace.Value;
                using MemoryOwner<Point3d> samples = MemoryOwner<Point3d>.Allocate(count, AllocationMode.Clear);
                int filled = SamplePoints(lod, policy.HausdorffSamplesPerFace.Value, policy.Seed, samples.Span);
                using MemoryOwner<double> distances = MemoryOwner<double>.Allocate(Math.Max(1, filled), AllocationMode.Clear);
                Atom<Seq<int>> misses = Atom(value: Seq<int>());
                src.Parallel(filled, new DirectedDistance(index, src, samples.Memory, distances.Memory, misses, key));
                return filled == 0
                    ? Fin.Fail<double>(key.InvalidResult())
                    : misses.Value.IsEmpty
                        ? Fin.Succ(TensorPrimitives.Max<double>(distances.Span[..filled]))
                        : Fin.Fail<double>(key.InvalidResult($"hausdorff: nearest-query miss at samples {string.Join(',', misses.Value.OrderBy(static ordinal => ordinal))}"));
            });
    }
    finally { src.Dispose(); }
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:619`**

```csharp
readonly struct DirectedDistance(SpatialIndex index, MeshEdit source, ReadOnlyMemory<Point3d> samples, Memory<double> distances, Atom<Seq<int>> misses, Op key) : IAction {
    public void Invoke(int i) {
        Point3d sample = samples.Span[i];
        Fin<double> measured = index.Query(sample, 1, key)
            .Bind(hits => hits.Head.ToFin(key.InvalidResult()).Map(id => Foot(source, id, sample)));
        if (measured.Case is double distance) { distances.Span[i] = distance; }
        else { _ = misses.Swap(held => held.Add(i)); }
    }

    static double Foot(MeshEdit source, int face, Point3d sample) {
        (int a, int b, int c) = source.Face(face);
        return SpatialIndex.ClosestOnTriangle(sample, source.Position(a), source.Position(b), source.Position(c)).Distance;
    }
}
```

**To**

```csharp
edit.ToSpace(key).Bind(space =>
    Hausdorff(op.Mesh, space, op.Policy, key).Bind(bound =>
```

```csharp
static Fin<double> Hausdorff(MeshSpace source, MeshSpace simplified, SimplifyPolicy policy, Op key) {
    using MeshEdit edit = MeshEdit.Of(source);
    int count = edit.FaceCount * policy.HausdorffSamplesPerFace.Value;
    using MemoryOwner<Point3d> samples = MemoryOwner<Point3d>.Allocate(count, AllocationMode.Clear);
    int filled = SamplePoints(edit, policy.HausdorffSamplesPerFace.Value, policy.Seed, samples.Span);
    if (filled == 0) return Fin.Fail<double>(key.InvalidResult());
    using MemoryOwner<double> distances = MemoryOwner<double>.Allocate(filled, AllocationMode.Clear);
    for (int i = 0; i < filled; i++) {
        Point3d nearest = simplified.Native.ClosestPoint(samples.Span[i]);
        if (!nearest.IsValid) return Fin.Fail<double>(key.InvalidResult($"hausdorff: nearest-query miss at sample {i}"));
        distances.Span[i] = samples.Span[i].DistanceTo(nearest);
    }
    return Fin.Succ(TensorPrimitives.Max<double>(distances.Span));
}
```

```csharp
// DirectedDistance DELETED
// DirectedDistance.Foot DELETED
```

**Why**

Sampling the simplified mesh and measuring toward the source can miss a region removed by decimation, so it does not enforce the stated lost-geometry ceiling. Querying one nearest AABB and refining only that triangle is not an exact nearest-triangle result: the closest box need not own the closest triangle. Rhino's admitted `Mesh.ClosestPoint(Point3d)` already owns the exact mesh query.

**Change**

Sample the original source and measure each sample against the frozen simplified mesh through `Mesh.ClosestPoint`. Remove the custom BVH build, one-box query, triangle-foot wrapper, miss atom, and parallel action type; retain the pooled distance plane and `TensorPrimitives.Max` reduction. Update the lead, law, claim description, and diagram to state directed source-to-simplified sampled Hausdorff distance rather than exact bidirectional Hausdorff.

**Delta**

Code-fence LOC: `-21` net. Authored types: `-1` (`DirectedDistance`). Authored members: `-2` (`Invoke` and `Foot`). Sampling and failure reporting remain.

# 7. Reuse admitted mesh state and emit features from the result

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:529`**

```csharp
internal static Fin<Unit> Curvature(SimplifyOp op, Context context, Op key, Memory<double> plane) =>
    Uniform(plane).Bind(_ =>
        VectorCloud.Cluster(toSeq(VertexPositions(op.Mesh)), context)
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:557`**

```csharp
static IEnumerable<Point3d> VertexPositions(MeshSpace space) {
    Mesh native = space.DuplicateNative();
    for (int v = 0; v < native.Vertices.Count; v++) {
        Point3f p = native.Vertices[v];
        yield return new Point3d(p.X, p.Y, p.Z);
    }
}
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:566`**

```csharp
static Fin<MeshSpace> Voxelize(MeshSpace mesh, SimplifyPolicy policy, Context context, Op key) {
    BoundingBox bounds = mesh.DuplicateNative().GetBoundingBox(accurate: true);
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:580`**

```csharp
static Fin<DecimationResult> Emit(QuadricStore store, MeshEdit edit, SimplifyOp op, int budget, Context context, Op key) =>
    edit.ToSpace(key).Bind(space =>
        Hausdorff(edit, op.Mesh, op.Policy, key).Bind(bound =>
            op.Policy.HausdorffCeiling.Filter(ceiling => bound > ceiling.Value).Case is not PositiveMagnitude breached
                ? Preserved(op, context, key).Map(features => new DecimationResult(
                    space,
                    Enumerable.Range(0, edit.VertexCount).Count(store.Alive),
                    store.Live,
                    budget,
                    bound,
                    store.Midpoints,
                    op.Kind.Traits,
                    features,
                    op.Kind.Traits.Admits(SimplifyTrait.Reversible) ? toSeq(store.Splits).Strict() : Seq<VertexSplit>()))
                : Fin.Fail<DecimationResult>(key.InvalidResult($"hausdorff {bound:G6} over ceiling {breached.Value:G6}"))));
```

**From — `libs/dotnet/Rasm/.planning/Processing/decimate.md:651`**

```csharp
static Fin<Seq<FeatureEdge>> Preserved(SimplifyOp op, Context context, Op key) =>
    op is SimplifyOp.FeaturePreserve
        ? MeshFeaturePolicy.Of(dihedralRadians: op.Policy.CreaseDihedral.Value, space: op.Mesh, faceRegions: Option<Arr<int>>.None, key: key)
            .Bind(features => VectorIntent.Features(op.Mesh, features, key))
            .Bind(intent => intent.Project<FeatureEdges>(context, key))
            .Map(static features => features.Edges.Filter(static e => e.Kind.Equals(MeshFeatureKind.Crease) || e.Kind.Equals(MeshFeatureKind.Boundary)))
        : Fin.Succ(Seq<FeatureEdge>());
```

**To**

```csharp
internal static Fin<Unit> Curvature(SimplifyOp op, Context context, Op key, Memory<double> plane) =>
    Uniform(plane).Bind(_ =>
        VectorCloud.Cluster(toSeq(Enumerable.Range(0, op.Mesh.Native.Vertices.Count)
            .Select(index => (Point3d)op.Mesh.Native.Vertices[index])), context)
```

```csharp
// VertexPositions DELETED
```

```csharp
static Fin<MeshSpace> Voxelize(MeshSpace mesh, SimplifyPolicy policy, Context context, Op key) {
    BoundingBox bounds = mesh.Bounds;
```

```csharp
static Fin<DecimationResult> Emit(QuadricStore store, MeshEdit edit, SimplifyOp op, int target, Context context, Op key) =>
    edit.ToSpace(key).Bind(space =>
        Hausdorff(op.Mesh, space, op.Policy, key).Bind(bound =>
            op.Policy.HausdorffCeiling.Filter(ceiling => bound > ceiling.Value).Case is PositiveMagnitude breached
                ? Fin.Fail<DecimationResult>(key.InvalidResult($"hausdorff {bound:G6} over ceiling {breached.Value:G6}"))
                : (op.Mode.Equals(SimplifyMode.FeaturePreserve)
                    ? MeshFeaturePolicy.Of(op.Policy.CreaseDihedral.Value, space, Option<Arr<int>>.None, key)
                        .Bind(features => VectorIntent.Features(space, features, key))
                        .Bind(intent => intent.Project<FeatureEdges>(context, key))
                        .Map(static features => features.Edges.Filter(static edge =>
                            edge.Kind.Equals(MeshFeatureKind.Crease) || edge.Kind.Equals(MeshFeatureKind.Boundary)))
                    : Fin.Succ(Seq<FeatureEdge>()))
                .Map(features => new DecimationResult(space,
                    Enumerable.Range(0, edit.VertexCount).Count(store.Alive), store.Live, target, bound, store.Midpoints,
                    op.Mode.Traits, features,
                    op.Mode.Traits.Admits(SimplifyTrait.Reversible) ? toSeq(store.Splits).Strict() : Seq<VertexSplit>()))));
```

```csharp
// Preserved DELETED
```

**Why**

`MeshSpace` already owns an admitted immutable snapshot and memoized bounds, yet two paths allocate duplicate native meshes only to read vertices or bounds; the iterator also leaves its duplicate undisposed. `Preserved` has one call site and recomputes output feature evidence from the source mesh, so the published `Features` do not describe the published simplified `Mesh`.

**Change**

Read vertex storage from the internal admitted snapshot, read `MeshSpace.Bounds`, and delete the iterator. Inline the single feature-projection call into `Emit`, run it against the frozen simplified `space`, and keep the ceiling failure ahead of that work. Rename remaining `budget` locals to `target` and use the mode row for result traits.

**Delta**

Code-fence LOC: `-12` net. Authored members: `-2` (`VertexPositions` and `Preserved`). No type or capability is added or removed; result feature evidence now belongs to the emitted mesh.

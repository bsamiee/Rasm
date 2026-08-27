# 1. Delete the heal request wrapper

## From

`libs/dotnet/Rasm/.planning/Processing/repair.md:125`

```csharp
public sealed record HealPlan(MeshSpace Input, Seq<HealOp> Ops, RepairPolicy Policy) : IValidityEvidence {
    public bool IsValid => ValidityClaim.CountAtLeast(count: Ops.Count, floor: 1);

    public static Fin<HealPlan> Of(MeshSpace input, Option<Seq<HealOp>> ops = default, Option<RepairPolicy> policy = default, Op? key = null) {
        Op op = key.OrDefault();
        Seq<HealOp> sequence = ops.IfNone(() => Heal.Standard);
        return from space in op.AcceptInput(input)
               from _ in guard(!sequence.IsEmpty, op.InvalidInput())
               select new HealPlan(space, sequence, policy.IfNone(RepairPolicy.Canonical));
    }
}
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:226`

```csharp
public static Fin<HealSession> Repair(HealPlan plan, Op? key = null) {
    Op op = key.OrDefault();
    Context context = plan.Input.Tolerance;
    MeshEdit live = MeshEdit.Of(plan.Input, plan.Policy.Arena);
    try {
        return Status(plan.Input, context, op).Bind(first =>
            plan.Ops.Fold(
                Fin.Succ((Space: plan.Input, Status: first, Steps: Seq<HealStep>(), Carry: Option<Incidence>.None)),
                (acc, heal) => acc.Bind(state =>
                    from edit in heal.Apply(live, state.Space, plan.Policy, op, state.Carry)
                    from space in Publish(edit)
                    from after in Status(space, context, op)
                    from step in heal.Stage.Step(StepSeed.Of(plan.Policy, state.Status, after, live, edit, op))
                    select (Space: space, Status: after, Steps: state.Steps.Add(step), edit.Carry)))
            .Map(state => new HealSession(Input: plan.Input, Healed: state.Space, Steps: state.Steps)));
    }
    finally { live.Dispose(); }
}
```

## To

```csharp
// HealPlan DELETED
```

```csharp
public static Fin<HealSession> Repair(
    MeshSpace input, Option<Seq<HealOp>> ops = default,
    Option<RepairPolicy> policy = default, Op? key = null) {
    Op op = key.OrDefault();
    Seq<HealOp> sequence = ops.IfNone(() => Standard);
    RepairPolicy repair = policy.IfNone(RepairPolicy.Canonical);
    return from space in op.AcceptInput(input)
           from _ in guard(!sequence.IsEmpty, op.InvalidInput())
           from session in Run(space)
           select session;

    Fin<HealSession> Run(MeshSpace admitted) {
        Context context = admitted.Tolerance;
        MeshEdit live = MeshEdit.Of(admitted, repair.Arena);
        try {
            return Status(admitted, context, op).Bind(first =>
                sequence.Fold(
                    Fin.Succ((Space: admitted, Status: first, Steps: Seq<HealStep>(), Carry: Option<Incidence>.None)),
                    (acc, heal) => acc.Bind(state =>
                        from edit in heal.Apply(live, state.Space, repair, op, state.Carry)
                        from space in Publish(edit)
                        from after in Status(space, context, op)
                        from step in heal.Stage.Step(StepSeed.Of(repair, state.Status, after, live, edit, op))
                        select (Space: space, Status: after, Steps: state.Steps.Add(step), edit.Carry)))
                .Map(state => new HealSession(admitted, state.Space, state.Steps));
        }
        finally { live.Dispose(); }

        Fin<MeshSpace> Publish(HealEdit edit) {
            if (!ReferenceEquals(edit.Edit, live)) { live.Dispose(); live = edit.Edit; }
            return live.ToSpace(op);
        }
    }
}
```

## Why

`HealPlan` only repackages three arguments for its sole consumer. `IsValid` repeats the empty-sequence gate already performed by `Of`, and the wrapper preserves no evidence or lifecycle beyond the call.

## Change

Move input admission, defaults, and the non-empty operation gate onto `Heal.Repair`; allocate the arena only after admission and keep its lifetime inside `Run`.

## Delta

Code-fence LOC: target -1; Fabrication consumer -1; net -2. Module-level types: -1 (`HealPlan`). Module-level members: -2 (`HealPlan.IsValid`, `HealPlan.Of`). Added module-level symbols: 0.

## Ripples

`libs/dotnet/Rasm.Fabrication/.planning/Ingress/solid.md:703`

```csharp
? HealPlan.Of(space, key: policy.Key)
    .Bind(plan => Heal.Repair(plan, policy.Key))
```

becomes:

```csharp
? Heal.Repair(space, key: policy.Key)
```

Remove `HealPlan` from the owner, entry, growth, density, diagram, package, and boundary text in `repair.md`, `Processing/session.md`, `Rasm/README.md`, `Rasm/ARCHITECTURE.md`, and Fabrication ingress.

# 2. Collapse the duplicate stage mapping

## From

`libs/dotnet/Rasm/.planning/Processing/repair.md:49`

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HealStage {
    public static readonly HealStage Weld = new("weld", rebuildsTopology: true, collects: true,
    mint: Some<Func<HealOp>>(static () => new HealOp.DuplicateWeld()),
    step: static seed => Fin.Succ<HealStep>(new HealStep.Weld(
        HealStage.Weld, seed.Context.For(seed.Policy.Arena.Weld), seed.Before, seed.After, seed.Vertices)));
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:79`

```csharp
public static readonly HealStage Boolean = new("boolean", rebuildsTopology: true, collects: false, mint: None,
    step: static seed => seed.Merge.ToFin(seed.Key.InvalidResult()).Map(merge =>
        (HealStep)new HealStep.Merge(
            HealStage.Boolean, merge.Op, merge.Census, seed.Before, seed.After,
            seed.ExtentFaces, seed.ExtentVertices)));

public bool RebuildsTopology { get; }
public bool Collects { get; }
public Option<Func<HealOp>> Mint { get; }
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:145`

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HealOp {
    private HealOp() { }

    public sealed record DuplicateWeld : HealOp;
    public sealed record DegenerateCollapse : HealOp;
    public sealed record GapClose : HealOp;
    public sealed record ManifoldRepair : HealOp;
    public sealed record OrientNormals : HealOp;
    public sealed record SelfIntersectResolve : HealOp;
    public sealed record Boolean(BooleanOp Op, MeshSpace Tool) : HealOp;

    public HealStage Stage =>
        Switch(
            duplicateWeld:        static _ => HealStage.Weld,
            degenerateCollapse:   static _ => HealStage.Degenerate,
            gapClose:             static _ => HealStage.Gap,
            manifoldRepair:       static _ => HealStage.Manifold,
            orientNormals:        static _ => HealStage.Orient,
            selfIntersectResolve: static _ => HealStage.SelfIntersect,
            boolean:              static _ => HealStage.Boolean);
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:220`

```csharp
public static readonly Seq<HealOp> Standard = Minted(static _ => true) + Minted(static stage => stage.Collects);

static Seq<HealOp> Minted(Func<HealStage, bool> admits) =>
    toSeq(HealStage.Items).Filter(admits).Bind(static stage => stage.Mint.ToSeq()).Map(static mint => mint());
```

## To

Apply the first change identically to all seven `HealStage` rows.

```csharp
[SmartEnum]
public sealed partial class HealStage {
    public static readonly HealStage Weld = new(rebuildsTopology: true, collects: true,
    step: static seed => Fin.Succ<HealStep>(new HealStep.Weld(
        HealStage.Weld, seed.Context.For(seed.Policy.Arena.Weld), seed.Before, seed.After, seed.Vertices)));
```

```csharp
public static readonly HealStage Boolean = new(rebuildsTopology: true, collects: false,
    step: static seed => seed.Merge.ToFin(seed.Key.InvalidResult()).Map(merge =>
        (HealStep)new HealStep.Boolean(
            HealStage.Boolean, merge.Op, merge.Census, seed.Before, seed.After,
            seed.ExtentFaces, seed.ExtentVertices)));

public bool RebuildsTopology { get; }
public bool Collects { get; }
```

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HealOp(HealStage Stage) {
    public sealed record Weld : HealOp(HealStage.Weld);
    public sealed record Degenerate : HealOp(HealStage.Degenerate);
    public sealed record Gap : HealOp(HealStage.Gap);
    public sealed record Manifold : HealOp(HealStage.Manifold);
    public sealed record Orient : HealOp(HealStage.Orient);
    public sealed record SelfIntersect : HealOp(HealStage.SelfIntersect);
    public sealed record Boolean(BooleanOp Op, MeshSpace Tool) : HealOp(HealStage.Boolean);
```

```csharp
public static readonly Seq<HealOp> Standard = toSeq<HealOp>([
    new HealOp.Weld(), new HealOp.Degenerate(), new HealOp.Gap(), new HealOp.Manifold(),
    new HealOp.Orient(), new HealOp.SelfIntersect(),
    new HealOp.Weld(), new HealOp.Degenerate(),
]);

// Minted DELETED
```

## Why

`HealStage.Mint` stores constructors for another closed family only to synthesize one fixed sequence. `HealOp.Stage` then re-dispatches the same one-to-one correspondence. No consumer converts, parses, persists, or looks up the stage's string key, so the keyed smart enum adds a generated key surface without an identity boundary. Action-suffixed op cases and `HealStep.Merge` also disagree with the canonical stage vocabulary.

## Change

Make `HealStage` keyless, put the stage on the `HealOp` root, have each case select it at construction, rename cases to the stage terms, rename `HealStep.Merge` to `HealStep.Boolean`, spell the standard sequence with catalogued `toSeq`, and delete `Mint` and `Minted`. Keep the ruled stage boolean pair and step behavior.

## Delta

Code-fence LOC: -15. Declared module-level members: -2 (`HealStage.Mint`, `Heal.Minted`); `HealOp.Stage` remains one positional member, net 0. Generated surface: -1 key property, -7 constructor delegates, and the keyed parse/lookup/conversion family. Added module-level symbols: 0.

## Ripples

Rename all target case uses to the six canonical names and `HealStep.Merge` to `HealStep.Boolean`. Update `repair.md`, `Processing/session.md`, and `Numerics/faults.md` prose/fault rendering for the keyless stage roster, the op-carried stage, and the Boolean step name.

# 3. Delete the unused policy factory

## From

`libs/dotnet/Rasm/.planning/Processing/repair.md:105`

```csharp
public sealed record RepairPolicy(
    ToleranceLane Gap, ToleranceLane Sliver, Dimension MaxManifoldPasses,
    ArenaPolicy Arena, IntersectPolicy Intersect, TessellationPolicy Retile, ArrangementPolicy Arrangement) {
    public static readonly RepairPolicy Canonical = new(
        Gap: ToleranceLane.Closure, Sliver: ToleranceLane.Area,
        MaxManifoldPasses: Dimension.Create(value: 8),
        Arena: ArenaPolicy.Canonical, Intersect: IntersectPolicy.Canonical,
        Retile: TessellationPolicy.Constrained, Arrangement: ArrangementPolicy.Canonical);

    public static Fin<RepairPolicy> Of(
        int maxManifoldPasses,
        Option<ToleranceLane> gap = default, Option<ToleranceLane> sliver = default, Option<ArenaPolicy> arena = default,
        Option<IntersectPolicy> intersect = default, Option<TessellationPolicy> retile = default,
        Option<ArrangementPolicy> arrangement = default, Op? key = null) =>
        key.OrDefault().AcceptValidated<Dimension>(candidate: maxManifoldPasses)
            .Map(passes => new RepairPolicy(gap.IfNone(ToleranceLane.Closure), sliver.IfNone(ToleranceLane.Area), passes,
                arena.IfNone(ArenaPolicy.Canonical), intersect.IfNone(IntersectPolicy.Canonical),
                retile.IfNone(TessellationPolicy.Constrained), arrangement.IfNone(ArrangementPolicy.Canonical)));
}
```

## To

```csharp
public sealed record RepairPolicy(
    ToleranceLane Gap, ToleranceLane Sliver, Dimension ManifoldPasses,
    ArenaPolicy Arena, IntersectPolicy Intersection, TessellationPolicy Tessellation, ArrangementPolicy Arrangement) {
    public static readonly RepairPolicy Canonical = new(
        Gap: ToleranceLane.Closure, Sliver: ToleranceLane.Area,
        ManifoldPasses: Dimension.Create(value: 8),
        Arena: ArenaPolicy.Canonical, Intersection: IntersectPolicy.Canonical,
        Tessellation: TessellationPolicy.Constrained, Arrangement: ArrangementPolicy.Canonical);

    // RepairPolicy.Of DELETED
}
```

## Why

`RepairPolicy.Of` has no repository consumer and only converts one raw `int` into the already-validating Thinktecture `Dimension`; every other parameter is an optional spelling of the canonical record columns. `MaxManifoldPasses`, `Intersect`, and `Retile` are longer or less precise duplicates of the budget, intersection policy, and tessellation policy types they carry.

## Change

Delete the unused convenience factory. Keep `Dimension` as the pass-budget admission owner and rename the three columns to `ManifoldPasses`, `Intersection`, and `Tessellation` throughout the repair fold.

## Delta

Code-fence LOC: -10. Module-level members: -1 (`RepairPolicy.Of`). Parameters: -8. Added module-level symbols: 0.

## Ripples

Rename the three policy-column reads and the owner/growth/boundary text in `repair.md` and `Processing/session.md`; remove claims that policy admission occurs at `RepairPolicy.Of`.

# 4. Delete the step payload record

## From

`libs/dotnet/Rasm/.planning/Processing/repair.md:89`

```csharp
[UseDelegateFromConstructor]
internal partial Fin<HealStep> Step(StepSeed seed);
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:234`

```csharp
from edit in heal.Apply(live, state.Space, plan.Policy, op, state.Carry)
from space in Publish(edit)
from after in Status(space, context, op)
from step in heal.Stage.Step(StepSeed.Of(plan.Policy, state.Status, after, live, edit, op))
```

`libs/dotnet/Rasm/.planning/Processing/session.md:44`

```csharp
internal readonly record struct StepSeed(
    RepairPolicy Policy, MeshEdit Result, ManifoldStatus Before, ManifoldStatus After,
    Set<int> Faces, Set<int> Vertices, Option<Incidence> Carry,
    Option<(BooleanOp Op, BooleanCensus Census)> Merge, Op Key) {
    internal Context Context => Result.Tolerance;

    internal Set<int> ExtentFaces => toSet(Range(0, Result.FaceCount));
    internal Set<int> ExtentVertices => toSet(Range(0, Result.VertexCount));

    internal static StepSeed Of(RepairPolicy policy, ManifoldStatus before, ManifoldStatus after, MeshEdit result, HealEdit edit, Op key) =>
        new(Policy: policy, Result: result, Before: before, After: after,
            Faces: toSet(result.DirtyFaces()), Vertices: toSet(result.DirtyVertices()),
            Carry: edit.Carry, Merge: edit.Merge, Key: key);
}
```

## To

```csharp
[UseDelegateFromConstructor]
internal partial Fin<HealStep> Step((
    RepairPolicy Policy, MeshEdit Result, ManifoldStatus Before, ManifoldStatus After,
    Option<Incidence> Carry, Option<(BooleanOp Op, BooleanCensus Census)> Merge, Op Key) seed);
```

```csharp
from edit in heal.Apply(live, state.Space, repair, op, state.Carry)
from space in Publish(edit)
from after in Status(space, context, op)
from step in heal.Stage.Step((repair, live, state.Status, after, edit.Carry, edit.Merge, op))
```

```csharp
// StepSeed DELETED
```

In each stage delegate, replace `seed.Context`, `Faces`, `Vertices`, `ExtentFaces`, and `ExtentVertices` with `seed.Result.Tolerance`, `toSet(seed.Result.DirtyFaces())`, `toSet(seed.Result.DirtyVertices())`, `toSet(Range(0, seed.Result.FaceCount))`, and `toSet(Range(0, seed.Result.VertexCount))` respectively; read the pass budget from `seed.Policy.ManifoldPasses`.

## Why

`StepSeed` materializes nine fields and four forwarding members for one immediate delegate call. Dirty sets and full extents are derived values read by only the selected stage, so precomputing all of them expands surface and performs unnecessary work.

## Change

Pass the six live owners plus the key as one named tuple, derive each selected stage's context and sets in that delegate, and delete `StepSeed` whole.

## Delta

Code-fence LOC: repair +2; session -14; net -12. Module-level types: -1 (`StepSeed`). Module-level members: -13. Added module-level symbols: 0.

## Ripples

Remove `StepSeed` from `Processing/session.md` owner, entry, law, growth, density, and transcription prose.

# 5. Replace private sum and product wrappers with aliases

## From

`libs/dotnet/Rasm/.planning/Processing/repair.md:42`

```csharp
using FaceKeySet = System.Collections.Generic.HashSet<(int, int, int)>;
using IndexSet = System.Collections.Generic.HashSet<int>;
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:93`

```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Cut {
    private Cut() { }

    public sealed record Pierced(int A, int B, int Face) : Cut;
    public sealed record Coplanar(int A, int B, int CarrierU, int CarrierV) : Cut;

    public (int A, int B) Pair =>
        Switch(pierced: static p => (p.A, p.B), coplanar: static c => (c.A, c.B));
}
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:138`

```csharp
internal readonly record struct HealEdit(MeshEdit Edit, Option<(BooleanOp Op, BooleanCensus Census)> Merge, Option<Incidence> Carry) {
    public static HealEdit Same(MeshEdit edit) => new(edit, None, None);

    public static HealEdit Carrying(MeshEdit edit, Incidence current) => new(edit, None, Some(current));
}
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:183`

```csharp
internal static Incidence Of(MeshEdit edit) {
    Dictionary<(int U, int V), List<int>> edges = new(3 * edit.FaceCount);
    for (int f = 0; f < edit.FaceCount; f++) {
        if (!edit.Alive(f)) continue;
        (int a, int b, int c) = edit.Face(f);
        Note(edges, a, b, f); Note(edges, b, c, f); Note(edges, c, a, f);
    }
    return new Incidence(edges);

    static void Note(Dictionary<(int U, int V), List<int>> edges, int u, int v, int f) =>
        (edges.TryGetValue(Key(u, v), out List<int>? faces) ? faces : edges[Key(u, v)] = []).Add(f);
}

internal static (int U, int V) Key(int u, int v) => u < v ? (u, v) : (v, u);

internal Arr<(int Tail, int Head, int Face)> Boundary(MeshEdit edit) =>
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:423`

```csharp
(Point3d p, Point3d q, Point3d r) = cut.Switch(
    state: (Soup: soup, Lift: lift),
    pierced:  static (s, c) => Corners(s.Soup, c.Face),
    coplanar: static (s, c) => (s.Soup.Position(c.CarrierU), s.Soup.Position(c.CarrierV), s.Soup.Position(c.CarrierU) + s.Lift));
(int a, int b) = cut.Pair;
conforms.Add(new Conform.Crossing(Intern(a), Intern(b), p, q, r));
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:392`

```csharp
Dictionary<int, List<Cut>> patches = new();
foreach ((int a, int b, int fa, int fb) in table.Segments) {
    if (a == b) continue;
    Note(patches, fa, new Cut.Pierced(a, b, fb)); Note(patches, fb, new Cut.Pierced(a, b, fa));
}
foreach ((int a, int b, int fa, int fb, int cu, int cv, _) in table.Coplanar) {
    if (a == b) continue;
    Note(patches, fa, new Cut.Coplanar(a, b, cu, cv)); Note(patches, fb, new Cut.Coplanar(a, b, cu, cv));
}
```

## To

```csharp
using Cut = LanguageExt.Either<
    (int A, int B, int Face),
    (int A, int B, int CarrierU, int CarrierV)>;
using RepairEdit = (
    Rasm.Meshing.MeshEdit Edit,
    LanguageExt.Option<(Rasm.Meshing.BooleanOp Op, Rasm.Meshing.BooleanCensus Census)> Boolean,
    LanguageExt.Option<Rasm.Processing.Incidence> Incidence);
```

```csharp
// Cut DELETED
```

```csharp
// HealEdit DELETED
```

```csharp
internal static Incidence Of(MeshEdit edit) {
    Dictionary<(int U, int V), List<int>> edges = new(3 * edit.FaceCount);
    for (int f = 0; f < edit.FaceCount; f++) {
        if (!edit.Alive(f)) continue;
        (int a, int b, int c) = edit.Face(f);
        Note(edges, a, b, f); Note(edges, b, c, f); Note(edges, c, a, f);
    }
    return new Incidence(edges);

    static void Note(Dictionary<(int U, int V), List<int>> edges, int u, int v, int f) {
        (int U, int V) edge = u < v ? (u, v) : (v, u);
        (edges.TryGetValue(edge, out List<int>? faces) ? faces : edges[edge] = []).Add(f);
    }
}

internal Arr<(int Tail, int Head)> Boundary(MeshEdit edit) =>
```

```csharp
(int a, int b, Point3d p, Point3d q, Point3d r) = cut.Match(
    Left: pierced => {
        (Point3d p, Point3d q, Point3d r) = Corners(soup, pierced.Face);
        return (pierced.A, pierced.B, p, q, r);
    },
    Right: coplanar => (coplanar.A, coplanar.B, soup.Position(coplanar.CarrierU),
        soup.Position(coplanar.CarrierV), soup.Position(coplanar.CarrierU) + lift));
conforms.Add(new Conform.Crossing(Intern(a), Intern(b), p, q, r));
```

```csharp
Dictionary<int, List<Cut>> patches = new();
foreach ((int a, int b, int fa, int fb) in table.Segments) {
    if (a == b) continue;
    Note(patches, fa, Cut.Left((a, b, fb))); Note(patches, fb, Cut.Left((a, b, fa)));
}
foreach ((int a, int b, int fa, int fb, int cu, int cv, _) in table.Coplanar) {
    if (a == b) continue;
    Note(patches, fa, Cut.Right((a, b, cu, cv))); Note(patches, fb, Cut.Right((a, b, cu, cv)));
}
```

Replace `new Cut.Pierced(...)` with `Cut.Left((...))` and `new Cut.Coplanar(...)` with `Cut.Right((...))`, using the package's explicit `Either.Left`/`Right` factories rather than relying on an inferred conversion. Replace every `HealEdit` constructor/helper call with a `RepairEdit` tuple, rename its payload slots to `Boolean` and `Incidence`, replace the two collection aliases with their BCL types, and narrow every rim tuple to `(int Tail, int Head)`.

## Why

`Cut` is a two-case scratch sum for one tessellation; `HealEdit` is a three-field scratch product confined to this fold. Their records add generated types, positional members, constructors, and forwarding methods without domain identity. `Merge` and `Carry` obscure that the optional payloads are Boolean evidence and a reusable incidence snapshot. `Incidence.Key` has one local caller, and `Boundary` publishes a face ordinal no consumer reads.

## Change

Use LanguageExt `Either` for the local sum and a C# tuple alias for the local product. Fold each cut once, construct edit results directly, localize edge canonicalization, and narrow boundary rows to consumed indices.

## Delta

Code-fence LOC: -14. Declared types: -4 (`Cut`, `Cut.Pierced`, `Cut.Coplanar`, `HealEdit`). Module-level members: -11. Source aliases added: 2; compiled module-level symbols added: 0.

## Ripples

Replace `HealEdit` with `RepairEdit` and its `Merge`/`Carry` reads with `Boolean`/`Incidence` in `Processing/session.md`; remove both deleted scratch types from owner, growth, density, and exemption text.

# 6. Drive manifold repair with the arena loop

## From

`libs/dotnet/Rasm/.planning/Processing/repair.md:312`

```csharp
internal static Fin<HealEdit> Split(MeshEdit edit, RepairPolicy policy, Option<Incidence> carry) {
    Dimension passes = policy.MaxManifoldPasses;
    Atom<(Option<int> Found, Incidence Last)> cell = Atom(value: (Found: Option<int>.None, Last: carry.IfNone(() => Incidence.Of(edit))));
    Transition<(Option<int> Found, Incidence Last)> driven = Cell.Converge(
        cell: cell,
        step: state => Some(state.Found == Some(0)
            ? state
            : SplitPass(edit, state.Found.IsNone ? state.Last : Incidence.Of(edit))),
        settled: static state => state.Found == Some(0),
        budget: passes,
        declined: new GeometryFault.UnrepairableMesh(HealStage.Manifold, Some(passes), cell.Value.Last.NonManifold().Count));
    (Option<int> found, Incidence last) = driven.Current;
    if (found == Some(0)) return Fin.Succ(HealEdit.Carrying(edit, last));
    Incidence settled = Incidence.Of(edit);
    int remaining = settled.NonManifold().Count;
    return remaining == 0
        ? Fin.Succ(HealEdit.Carrying(edit, settled))
        : Fin.Fail<HealEdit>(new GeometryFault.UnrepairableMesh(HealStage.Manifold, Some(passes), remaining));

    static (Option<int> Found, Incidence Last) SplitPass(MeshEdit edit, Incidence incidence) {
        Arr<((int U, int V) Edge, List<int> Fans)> rows = incidence.NonManifold();
        foreach (((int u, int v), List<int> fans) in rows) {
            foreach (int extra in fans.Skip(2)) {
                int du = edit.AddVertex(edit.Position(u));
                int dv = edit.AddVertex(edit.Position(v));
                (int a, int b, int c) = edit.Face(extra);
                edit.SetFace(extra, Re(a, u, du, v, dv), Re(b, u, du, v, dv), Re(c, u, du, v, dv));
            }
        }
        return (Some(rows.Count), incidence);

        static int Re(int corner, int u, int du, int v, int dv) => corner == u ? du : corner == v ? dv : corner;
    }
}
```

## To

```csharp
internal static Fin<RepairEdit> Split(MeshEdit edit, RepairPolicy policy, Option<Incidence> incidence) {
    Incidence current = incidence.IfNone(() => Incidence.Of(edit));
    for (int pass = 0; pass < policy.ManifoldPasses.Value; pass++) {
        Arr<((int U, int V) Edge, List<int> Fans)> rows = current.NonManifold();
        if (rows.IsEmpty) return Fin.Succ<RepairEdit>((edit, None, Some(current)));
        foreach (((int u, int v), List<int> fans) in rows) {
            foreach (int face in fans.Skip(2)) {
                int du = edit.AddVertex(edit.Position(u));
                int dv = edit.AddVertex(edit.Position(v));
                (int a, int b, int c) = edit.Face(face);
                edit.SetFace(face, Replace(a, u, du, v, dv), Replace(b, u, du, v, dv), Replace(c, u, du, v, dv));
            }
        }
        current = Incidence.Of(edit);
    }
    int remaining = current.NonManifold().Count;
    return remaining == 0
        ? Fin.Succ<RepairEdit>((edit, None, Some(current)))
        : Fin.Fail<RepairEdit>(new GeometryFault.UnrepairableMesh(
            HealStage.Manifold, Some(policy.ManifoldPasses), remaining));

    static int Replace(int corner, int u, int du, int v, int dv) =>
        corner == u ? du : corner == v ? dv : corner;
}
```

## Why

This is a single-writer mutation kernel, not a contended cell. `Atom`, `Transition`, `Option<int>`, and `Cell.Converge` add CAS machinery and discard its verdict before recomputing incidence. The arena owns the state and the admitted `Dimension.Value` owns the bound.

## Change

Iterate directly to the pass budget, return on empty incidence, recompute once after each mutating pass, and preserve the typed terminal `UnrepairableMesh` failure.

## Delta

Code-fence LOC: -9. Local carrier shapes: -3. Local functions: -1 (`SplitPass`), with endpoint replacement retained locally. Module-level symbols: unchanged.

# 7. Orient all shells in one package traversal

## From

`libs/dotnet/Rasm/.planning/Processing/repair.md:33`

```csharp
using QuikGraph.Algorithms;
using QuikGraph.Algorithms.Search;
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:348`

```csharp
internal static Fin<HealEdit> Orient(MeshEdit edit, Option<Incidence> carry) {
    Incidence incidence = carry.IfNone(() => Incidence.Of(edit));
    AdjacencyGraph<int, TaggedEdge<int, (int U, int V)>> dual = incidence.Dual(edit);
    Dictionary<int, int> shell = new(edit.FaceCount);
    dual.WeaklyConnectedComponents(shell);
    Dictionary<int, int> seeds = new();
    for (int f = 0; f < edit.FaceCount; f++) {
        if (edit.Alive(f) && shell.TryGetValue(f, out int component)) seeds.TryAdd(component, f);
    }
    foreach (int seed in seeds.Values) {
        BreadthFirstSearchAlgorithm<int, TaggedEdge<int, (int U, int V)>> walk = new(dual);
        walk.TreeEdge += arc => {
            if (SameTraversal(edit.Face(arc.Source), edit.Face(arc.Target), arc.Tag)) {
                (int a, int b, int c) = edit.Face(arc.Target);
                edit.SetFace(arc.Target, a, c, b);
            }
        };
        walk.Compute(seed);
    }
    return Fin.Succ(HealEdit.Carrying(edit, incidence));
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:209`

```csharp
internal AdjacencyGraph<int, TaggedEdge<int, (int U, int V)>> Dual(MeshEdit edit) {
    AdjacencyGraph<int, TaggedEdge<int, (int U, int V)>> dual = new(allowParallelEdges: true);
    dual.AddVertexRange(Enumerable.Range(0, edit.FaceCount).Where(edit.Alive));
    foreach (((int U, int V) edge, List<int> faces) in Edges.Where(static row => row.Value.Count == 2)) {
        dual.AddEdge(new TaggedEdge<int, (int U, int V)>(faces[0], faces[1], edge));
        dual.AddEdge(new TaggedEdge<int, (int U, int V)>(faces[1], faces[0], edge));
    }
    return dual;
}
```

## To

```csharp
using QuikGraph.Algorithms.Search;
```

```csharp
internal static Fin<RepairEdit> Orient(MeshEdit edit, Option<Incidence> carried) {
    Incidence incidence = carried.IfNone(() => Incidence.Of(edit));
    AdjacencyGraph<int, TaggedEdge<int, (int U, int V)>> dual = new(allowParallelEdges: true);
    dual.AddVertexRange(Enumerable.Range(0, edit.FaceCount).Where(edit.Alive));
    foreach (((int U, int V) edge, List<int> faces) in incidence.Edges.Where(static row => row.Value.Count == 2)) {
        dual.AddEdge(new TaggedEdge<int, (int U, int V)>(faces[0], faces[1], edge));
        dual.AddEdge(new TaggedEdge<int, (int U, int V)>(faces[1], faces[0], edge));
    }
    DepthFirstSearchAlgorithm<int, TaggedEdge<int, (int U, int V)>> walk = new(dual) {
        ProcessAllComponents = true,
    };
    walk.TreeEdge += arc => {
        if (SameTraversal(edit.Face(arc.Source), edit.Face(arc.Target), arc.Tag)) {
            (int a, int b, int c) = edit.Face(arc.Target);
            edit.SetFace(arc.Target, a, c, b);
        }
    };
    walk.Compute();
    return Fin.Succ<RepairEdit>((edit, None, Some(incidence)));
}
```

```csharp
// Incidence.Dual DELETED
```

## Why

QuikGraph `DepthFirstSearchAlgorithm.ProcessAllComponents` already emits a spanning forest and `TreeEdge` for every component. Component labeling, a seed dictionary, and one breadth-first object per shell hand-roll that package traversal. `Incidence.Dual` has one caller and no policy of its own.

## Change

Build the dual at its only consumer, attach the orientation action to one all-component depth-first traversal, and remove `Incidence.Dual`, the component extension import, and both dictionaries.

## Delta

Code-fence LOC: -8. Module-level members: -1 (`Incidence.Dual`). Local allocations: -2 dictionaries and N-1 traversal objects for N shells. Added module-level symbols: 0.

# 8. Collapse single-call kernels onto exhaustive result flow

## From

`libs/dotnet/Rasm/.planning/Processing/repair.md:250`

```csharp
internal static Fin<ManifoldStatus> Status(MeshSpace space, Context context, Op key) =>
    VectorIntent.Topology(space, key)
        .Bind(intent => intent.Project<(int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus)>(context: context, key: key))
        .Map(ManifoldStatus.Of);
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:277`

```csharp
internal static Fin<HealEdit> Close(MeshEdit edit, RepairPolicy policy, Op key, Option<Incidence> carry) {
    Incidence incidence = carry.IfNone(() => Incidence.Of(edit));
    Arr<(int Tail, int Head, int Face)> rim = incidence.Boundary(edit);
    if (rim.Count < 2) return Fin.Succ(HealEdit.Carrying(edit, incidence));
    double span = edit.Tolerance.For(policy.Gap).Value;
    Point3d[] heads = [.. rim.Map(h => edit.Position(h.Head))];
    return NeighborIndex.Of(new NeighborSource.PointsCase(toSeq(rim.Map(h => edit.Position(h.Tail)))), key)
        .Bind(index => key.AcceptValidated<PositiveMagnitude>(candidate: span)
            .Bind(reach => NeighborKernel.GraphOf(index: index, needles: heads, count: Option<Dimension>.None, radius: Some(reach), key: key)))
        .Map(graph => Bridge(edit, rim, graph.Ids, span, incidence));
}

static HealEdit Bridge(MeshEdit edit, Arr<(int Tail, int Head, int Face)> rim, int[][] candidates, double span, Incidence incidence) {
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:377`

```csharp
internal static Fin<HealEdit> Resolve(MeshEdit edit, MeshSpace current, RepairPolicy policy, Op key) =>
    Intersection.Apply(new IntersectOp.SelfMesh(current, policy.Intersect), key)
        .Bind(result => result is IntersectResult.Chains hit
            ? Fin.Succ(hit.Table)
            : Fin.Fail<CrossTable>(key.InvalidResult()))
        .Bind(table => table.Segments.Count == 0 && table.Coplanar.Count == 0
            ? Fin.Succ(HealEdit.Same(edit))
            : Recut(edit, current, table, policy, key));

static Fin<HealEdit> Recut(MeshEdit edit, MeshSpace current, CrossTable table, RepairPolicy policy, Op key) {
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:412`

```csharp
static Fin<Unit> Subdivide(MeshEdit edit, MeshEdit soup, CrossTable table, int face, int tableFace, List<Cut> cuts, Dictionary<Point3d, int> minted, RepairPolicy policy, Op key) {
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:453`

```csharp
static Unit Splice(MeshEdit edit, int face, Arr<(Point3d A, Point3d B, Point3d C)> triangles, Dictionary<Point3d, int> corner, Dictionary<Point3d, int> minted, bool mirrored) {
```

`libs/dotnet/Rasm/.planning/Processing/repair.md:468`

```csharp
internal static Fin<HealEdit> Merge(HealOp.Boolean op, MeshSpace current, RepairPolicy policy, Op key) =>
    Arrangement.Apply(new ArrangementOp.MeshBoolean(Seq(current, op.Tool), op.Op, policy.Arrangement), key)
        .Bind(result => result switch {
            ArrangementResult.Boolean { Shells: [MeshSpace solid] } merged =>
                Fin.Succ(new HealEdit(MeshEdit.Of(solid, policy.Arena), Some((op.Op, merged.Census)), None)),
            ArrangementResult.Boolean severed =>
                Fin.Fail<HealEdit>(new GeometryFault.UnrepairableMesh(HealStage.Boolean, Option<Dimension>.None, severed.Shells.Count)),
            _ => Fin.Fail<HealEdit>(key.InvalidResult()),
        });
```

`libs/dotnet/Rasm/.planning/Processing/session.md:34`

```csharp
public readonly record struct ManifoldStatus(
    int EulerCharacteristic, int BoundaryComponents, bool IsManifold, bool IsOriented,
    int NonManifoldEdges, Option<int> Genus) {
    public static ManifoldStatus Of((int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus) projection) =>
        new(projection.Euler, projection.BoundaryComponents, projection.IsManifold, projection.IsOriented, projection.NonManifoldEdges, projection.Genus);

    public bool GenusClosed => Genus.Exists(genus => IsManifold && BoundaryComponents == 0 && NonManifoldEdges == 0
        && EulerCharacteristic == 2 - (2 * genus));
}
```

## To

```csharp
internal static Fin<ManifoldStatus> Status(MeshSpace space, Context context, Op key) =>
    VectorIntent.Topology(space, key)
        .Bind(intent => intent.Project<(int Euler, int BoundaryComponents, bool IsManifold, bool IsOriented, int NonManifoldEdges, Option<int> Genus)>(context: context, key: key))
        .Map(static projection => new ManifoldStatus(
            projection.Euler, projection.BoundaryComponents, projection.IsManifold,
            projection.IsOriented, projection.NonManifoldEdges, projection.Genus));
```

Inline the existing `Bridge` body into `Close`'s `Map` block and return a `RepairEdit` tuple.

```csharp
// Bridge DELETED
```

```csharp
internal static Fin<RepairEdit> Resolve(MeshEdit edit, MeshSpace current, RepairPolicy policy, Op key) =>
    Intersection.Apply(new IntersectOp.SelfMesh(current, policy.Intersection), key)
        .Bind(result => result.Switch(
            state: key,
            points: static (site, _) => Fin.Fail<CrossTable>(site.InvalidResult()),
            segments: static (site, _) => Fin.Fail<CrossTable>(site.InvalidResult()),
            chains: static (_, hit) => Fin.Succ(hit.Table)))
        .Bind(table => table.Segments.Count == 0 && table.Coplanar.Count == 0
            ? Fin.Succ<RepairEdit>((edit, None, None))
            : Recut(table));
```

Move `Recut` into `Resolve` as a local function, move `Subdivide` inside it, inline `Splice` into the `Triangles().Map` body, and remove the redundant `Strict()` before the one-pass `TraverseM`.

```csharp
// Recut DELETED
```

```csharp
// Subdivide DELETED
```

```csharp
// Splice DELETED
```

```csharp
internal static Fin<RepairEdit> Boolean(HealOp.Boolean op, MeshSpace current, RepairPolicy policy, Op key) =>
    Arrangement.Apply(new ArrangementOp.MeshBoolean(Seq(current, op.Tool), op.Op, policy.Arrangement), key)
        .Bind(result => result.Switch(
            state: (Op: op.Op, Policy: policy, Key: key),
            boolean: static (state, merged) => merged.Shells is [MeshSpace solid]
                ? Fin.Succ<RepairEdit>((MeshEdit.Of(solid, state.Policy.Arena), Some((state.Op, merged.Census)), None))
                : Fin.Fail<RepairEdit>(new GeometryFault.UnrepairableMesh(
                    HealStage.Boolean, Option<Dimension>.None, merged.Shells.Count)),
            overlay: static (state, _) => Fin.Fail<RepairEdit>(state.Key.InvalidResult()),
            complex: static (state, _) => Fin.Fail<RepairEdit>(state.Key.InvalidResult())));
```

```csharp
public readonly record struct ManifoldStatus(
    int EulerCharacteristic, int BoundaryComponents, bool IsManifold, bool IsOriented,
    int NonManifoldEdges, Option<int> Genus) {
    public bool GenusClosed => Genus.Exists(genus => IsManifold && BoundaryComponents == 0 && NonManifoldEdges == 0
        && EulerCharacteristic == 2 - (2 * genus));
}
```

## Why

`Bridge`, `Recut`, `Subdivide`, and `Splice` each have one caller and no independent policy. `ManifoldStatus.Of` is a one-call tuple copier. The `is` probe and catch-all C# switch over generated closed unions suppress the compile breaks owed by new `IntersectResult` and `ArrangementResult` cases.

## Change

Localize each one-call kernel at its owning operation, inline the terminal mutation bodies, construct `ManifoldStatus` directly, use generated exhaustive `Switch` for both package unions, and rename `Heal.Merge` to the operation it performs, `Heal.Boolean`. Preserve the typed severance failure.

## Delta

Code-fence LOC: target -2; session -2; exhaustive dispatch +2; net -2. Module-level members: -5 (`Heal.Bridge`, `Heal.Recut`, `Heal.Subdivide`, `Heal.Splice`, `ManifoldStatus.Of`). Closed-family catch-all/type-test arms: -2. `Heal.Merge` is renamed in place; added module-level symbols: 0.

## Ripples

Remove `ManifoldStatus.Of` from `Processing/session.md` entry and density text. Rename the `Rasm/RULINGS.md` `Heal.Merge` decision to `Heal.Boolean` without changing its typed-severance law.

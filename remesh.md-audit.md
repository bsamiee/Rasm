# 1. Consolidate the n-RoSy order at the field owner

**From**

`libs/dotnet/Rasm/.planning/Processing/remesh.md:69`
```csharp
[SmartEnum<int>]
public sealed partial class RoSyOrder {
    public static readonly RoSyOrder Line = new(key: 2);
    public static readonly RoSyOrder Cross = new(key: 4);
    public static readonly RoSyOrder Hex = new(key: 6);
}
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:106`
```csharp
public sealed record Isotropic(MeshSpace Mesh, PositiveMagnitude TargetLength, RemeshPolicy Policy) : RemeshOp;
public sealed record QuadField(MeshSpace Mesh, PositiveMagnitude TargetLength, RoSyOrder Symmetry, RemeshPolicy Policy) : RemeshOp;
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:438`
```csharp
return SegmentKernel.CrossFieldAt(space, op.Symmetry.Key, None, None, seed, key)
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:441`
```csharp
from cross in VectorField.CrossField(space, op.Symmetry.Key, None, None, key)
from rotated in VectorField.CrossField(space, op.Symmetry.Key, Some(Seq((a, turned))), None, key)
```

**To**

```csharp
// RoSyOrder DELETED
```

```csharp
public sealed record Isotropic(MeshSpace Mesh, PositiveMagnitude TargetLength, RemeshPolicy Policy) : RemeshOp;
public sealed record QuadField(MeshSpace Mesh, PositiveMagnitude TargetLength, RosyOrder Order, RemeshPolicy Policy) : RemeshOp;
```

```csharp
return SegmentKernel.CrossFieldAt(space, op.Order, None, None, seed, key)
```

```csharp
from cross in VectorField.CrossField(space, op.Order, None, None, key)
from rotated in VectorField.CrossField(space, op.Order, Some(Seq((a, turned))), None, key)
```

**Why**

`RoSyOrder` duplicates the closed order vocabulary already required by the direction-field kernel. The two rosters also disagree mathematically: order one is a vector field, order two a line field, and orders four and six the cross and hex fields. Projecting either owner to `int` at every seam discards admission and forces the next owner to admit the same order again.

**Change**

Delete `RoSyOrder`. Rename the field owner's `RosySymmetry` to `RosyOrder`, give it the canonical `Vector`/`Line`/`Cross`/`Hex` rows keyed `1`/`2`/`4`/`6`, and carry that generated owner through the remesh request, vector-field case, and segment kernel. Read `.Key` only inside numeric phase arithmetic and cache coordinates.

**Delta**

Target code-fence LOC: -5. Target module surface: -1 smart-enum type and -3 static rows. Project surface after ripples: -1 module-level type and -3 declared rows net; the surviving four-row owner replaces four misnamed rows rather than adding a second roster.

**Ripples**

- `libs/dotnet/Rasm/.planning/Processing/segment.md`: rename `RosySymmetry` to `RosyOrder`, replace the four rows with `Vector`/`Line`/`Cross`/`Hex`, delete the forwarding `Phase` property, accept `RosyOrder` directly in `CrossFieldAt`, remove `AcceptValidated<RosySymmetry>`, and use `.Key` only in phase, power, and cache arithmetic.
- `libs/dotnet/Rasm/.planning/Spatial/fields.md`: declare the currently missing `CrossFieldCase` and its admitting `VectorField.CrossField` factory with `RosyOrder`; the sampler forwards the admitted row instead of its raw key.
- `libs/dotnet/Rasm/.planning/Parametric/panelize.md`: replace `RoSyOrder` with `RosyOrder` on both panel-family cases and every field-frame call.

# 2. Collapse the request union and inline its only admission hop

**From**

`libs/dotnet/Rasm/.planning/Processing/remesh.md:102`
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RemeshOp {
    private RemeshOp() { }

    public sealed record Isotropic(MeshSpace Mesh, PositiveMagnitude TargetLength, RemeshPolicy Policy) : RemeshOp;
    public sealed record QuadField(MeshSpace Mesh, PositiveMagnitude TargetLength, RoSyOrder Symmetry, RemeshPolicy Policy) : RemeshOp;
}
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:110`
```csharp
public static class Remeshing {
    public static Fin<RewriteResult> Apply(RemeshOp op, Op? key = null) =>
        op.Switch(
            state: key.OrDefault(),
            isotropic: static (token, i) => Admit(i.Mesh, i.Policy).Bind(_ =>
                Equalize(i.Mesh, i.TargetLength.Value, i.Policy, token).Map(static pair => new RewriteResult(pair.Space, pair.Trace, None))),
            quadField: static (token, q) => Admit(q.Mesh, q.Policy).Bind(_ => Quadrangulate(q, token)));
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:118`
```csharp
static Fin<Unit> Admit(MeshSpace mesh, RemeshPolicy policy) =>
    mesh.Native.Faces.Count == 0 ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "empty mesh"))
    : !policy.IsValid ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "invalid remesh policy"))
    : Fin.Succ(unit);
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:429`
```csharp
static Fin<RewriteResult> Quadrangulate(RemeshOp.QuadField op, Op key) =>
```

**To**

```csharp
public sealed record RemeshOp(
    MeshSpace Mesh, PositiveMagnitude TargetLength, RemeshPolicy Policy,
    Option<RosyOrder> Order = default);
```

```csharp
public static class Remeshing {
    public static Fin<RewriteResult> Apply(RemeshOp request, Op? key = null) {
        Op op = key.OrDefault();
        return request switch {
            { Mesh.Native.Faces.Count: 0 } => Fin.Fail<RewriteResult>(
                new GeometryFault.DegenerateInput(Kind.Mesh, None, "empty mesh")),
            { Policy.IsValid: false } => Fin.Fail<RewriteResult>(
                new GeometryFault.DegenerateInput(Kind.Mesh, None, "invalid remesh policy")),
            { Order.Case: RosyOrder order } => Quadrangulate(request, order, op),
            _ => Equalize(request.Mesh, request.TargetLength, request.Policy, op),
        };
    }
```

```csharp
// Admit DELETED
```

```csharp
static Fin<RewriteResult> Quadrangulate(RemeshOp request, RosyOrder order, Op key) =>
```

**Why**

Both union cases carry the same mesh, target, and policy; the only discriminant is whether an admitted order exists. The root plus two nested case types therefore duplicate the exact `Option<RosyOrder>` shape. `Admit` is a one-call forwarding method, and its two guards are clearer in the entry's single structural dispatch.

**Change**

Replace the regular union with one request record whose optional order selects quad extraction. Flatten faceless-mesh, invalid-policy, present-order, and isotropic handling into one request switch; delete `Admit`; pass the present order explicitly into quadrangulation. Keep the two distinct failure details and add no constructor factories.

**Delta**

Target code-fence LOC: -7. Target declared surface: -2 nested case types and -1 class method; the generated union dispatch surface disappears, net -3 declared symbols with no replacement type.

**Ripples**

- `libs/dotnet/Rasm/.planning/Parametric/panelize.md`: construct `new RemeshOp(source.Mesh, family.TargetLength, policy.Remesh, Some(family.Order))` instead of `RemeshOp.QuadField`.
- `libs/dotnet/Rasm/.planning/Processing/intent.md`: update the temporary payload construction to the single record until task 4 deletes the unused rewrite relay.

# 3. Shrink policy to admitted variation

**From**

`libs/dotnet/Rasm/.planning/Processing/remesh.md:50`
```csharp
public sealed record RemeshPolicy(
    Dimension Iterations, PositiveMagnitude SplitRatio, UnitInterval CollapseRatio, VectorAngle CreaseDihedral,
    UnitInterval ConvergenceBand, Dimension ProjectCandidates, Dimension ParallelFloor,
    Dimension InteriorValence, Dimension BoundaryValence,
    Option<Func<Point3d, double>> Sizing) : IValidityEvidence {
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:55`
```csharp
const double CreaseDihedralRadians = 40.0 * Math.PI / 180.0;
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:59`
```csharp
CollapseRatio: UnitInterval.Create(value: 4.0 / 5.0), CreaseDihedral: VectorAngle.Create(value: CreaseDihedralRadians),
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:61`
```csharp
ParallelFloor: Dimension.Create(value: 4_096),
InteriorValence: Dimension.Create(value: 6), BoundaryValence: Dimension.Create(value: 4),
Sizing: Option<Func<Point3d, double>>.None);
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:135`
```csharp
Func<Point3d, double> targetAt = policy.Sizing.Match(
    Some: field => (Func<Point3d, double>)(at => field(at) is > 0.0 and var local && double.IsFinite(local) ? local : target),
    None: () => _ => target);
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:188`
```csharp
sealed record Edges(Dictionary<(int U, int V), (int F0, Option<int> F1)> Table, EdgeKeySet Feature, IndexSet Pinned, IndexSet Boundary) {
    public int FeatureCount => Feature.Count;
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:315`
```csharp
(int interior, int rim) = (policy.InteriorValence.Value, policy.BoundaryValence.Value);
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:324`
```csharp
int Deviate(int v, int delta) => Math.Abs(valence.GetValueOrDefault(v) + delta - (edges.Boundary.Contains(v) ? rim : interior));
```

**To**

```csharp
public sealed record RemeshPolicy(
    Dimension Iterations, PositiveMagnitude SplitRatio, UnitInterval CollapseRatio, VectorAngle CreaseDihedral,
    UnitInterval ConvergenceBand, Dimension ProjectCandidates, Dimension ParallelFloor,
    Option<Func<Point3d, PositiveMagnitude>> Sizing) : IValidityEvidence {
```

```csharp
// CreaseDihedralRadians DELETED
```

```csharp
CollapseRatio: UnitInterval.Create(value: 4.0 / 5.0),
CreaseDihedral: VectorAngle.Create(value: 40.0 * Math.PI / 180.0),
```

```csharp
ParallelFloor: Dimension.Create(value: 4_096),
Sizing: Option<Func<Point3d, PositiveMagnitude>>.None);
```

```csharp
Func<Point3d, double> targetAt = policy.Sizing.Match(
    Some: field => point => field(point).Value,
    None: () => _ => target.Value);
```

```csharp
sealed record Edges(Dictionary<(int U, int V), (int F0, Option<int> F1)> Table, EdgeKeySet Feature, IndexSet Pinned, IndexSet Boundary) {
```

```csharp
int Deviate(int vertex, int delta) => Math.Abs(
    valence.GetValueOrDefault(vertex) + delta - (edges.Boundary.Contains(vertex) ? 4 : 6));
```

**Why**

Regular triangular valence is six in the interior and four on the boundary; exposing those constants as policy admits unsupported topology targets. The sizing delegate returns raw `double`, forcing every evaluation to repeat positivity and finiteness checks and silently substitute another target on refusal. `FeatureCount` and the dihedral constant each forward one read.

**Change**

Delete both valence properties, `Edges.FeatureCount`, and `CreaseDihedralRadians`. Derive regular valence at the flip objective, inline the one-use angle conversion, and require the sizing function to return `PositiveMagnitude` so the inner loop reads admitted values without fallback validation. Rename the pass counters to `splits`, `collapses`, and `flips`, and the collapse veto from `minted` to `stretches` when task 4 rebuilds the pass state.

**Delta**

Target code-fence LOC: -3. Target declared surface: -2 public policy properties, -1 nested-type property, and -1 constant; net -4 members.

# 4. Collapse pass and trace carriers into one typed result

**From**

`libs/dotnet/Rasm/.planning/Processing/remesh.md:76`
```csharp
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PassVerdict {
    private PassVerdict() { }

    public sealed record RunningCase : PassVerdict;
    public sealed record ConvergedCase : PassVerdict;
    public sealed record FaultedCase(Error Cause) : PassVerdict;
}
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:85`
```csharp
public sealed record RemeshTrace(
    double TargetLength, Stat<Scalar> Deviation, int Iterations,
    int Splits, int Collapses, int Flips, int FeatureEdges);

public sealed record QuadProvenance(Arr<int> Corners, Arr<int> PatchOf, Arr<double> U, Arr<double> V, Arr<int> SingularFaces);

public sealed record RewriteResult(MeshSpace Mesh, RemeshTrace Trace, Option<QuadProvenance> Quads) {
    internal Fin<TOut> Project<TOut>(Op key) {
        RewriteResult self = this;
        return ResultProjection.Rows<RewriteResult, TOut>(self: self, key: key,
            ProjectionRow.Of<MeshSpace>(() => Fin.Succ(self.Mesh)),
            ProjectionRow.Of<RemeshTrace>(() => Fin.Succ(self.Trace)),
            ProjectionRow.Of<QuadProvenance>(() => self.Quads.ToFin(key.Unsupported(inputType: typeof(RewriteResult), outputType: typeof(QuadProvenance)))));
    }
}
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:124`
```csharp
sealed record PassState(
    int Rounds, int Splits, int Collapses, int Flips, int Features,
    Option<Stat<Scalar>> Deviation, PassVerdict Verdict) {
    internal static readonly PassState Seed = new(0, 0, 0, 0, 0, None, new PassVerdict.RunningCase());

    internal bool Settled => Verdict is not PassVerdict.RunningCase;
}
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:132`
```csharp
static Fin<(MeshSpace Space, RemeshTrace Trace)> Equalize(MeshSpace source, double target, RemeshPolicy policy, Op key) {
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:138`
```csharp
Atom<PassState> cell = Atom(value: PassState.Seed);
Transition<PassState> driven = Cell.Converge(
    cell: cell,
    step: state => Some(state.Settled ? state : Pass(state)),
    settled: static state => state.Settled,
    budget: policy.Iterations,
    declined: key.InvalidResult());
PassState terminal = driven.Current;
return terminal.Verdict.Switch(
    state: (State: terminal, Policy: policy, Target: target, Arena: arena, Key: key),
    runningCase: static (s, _) => s.State.Deviation.Match(
        Some: measured => measured.Mean <= s.Policy.ConvergenceBand.Value
            ? Emit(s.Arena, s.Target, s.State, measured, s.Key)
            : Fin.Fail<(MeshSpace, RemeshTrace)>(new GeometryFault.RemeshStalled(PositiveMagnitude.Create(value: s.Target), Some(s.Target * (1.0 + measured.Mean)), s.State.Rounds)),
        None: () => Fin.Fail<(MeshSpace, RemeshTrace)>(new GeometryFault.RemeshStalled(PositiveMagnitude.Create(value: s.Target), Option<double>.None, 0))),
    convergedCase: static (s, _) => s.State.Deviation
        .ToFin(s.Key.InvalidResult())
        .Bind(measured => Emit(s.Arena, s.Target, s.State, measured, s.Key)),
    faultedCase: static (s, faulted) => Fin.Fail<(MeshSpace, RemeshTrace)>(faulted.Cause));
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:168`
```csharp
if (Project(arena, frozen, policy, key).Case is Error fault) {
    return state with { Verdict = new PassVerdict.FaultedCase(fault) };
}
return Deviation(arena, targetAt, key).Match(
    Succ: spread => state with {
        Rounds = state.Rounds + 1, Splits = state.Splits + did, Collapses = state.Collapses + killed,
        Flips = state.Flips + turned, Features = features, Deviation = Some(spread),
        Verdict = did + killed + turned == 0 && spread.Mean <= policy.ConvergenceBand.Value
            ? new PassVerdict.ConvergedCase()
            : state.Verdict,
    },
    Fail: fault => state with { Verdict = new PassVerdict.FaultedCase(fault) });
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:184`
```csharp
static Fin<(MeshSpace Space, RemeshTrace Trace)> Emit(MeshEdit arena, double target, PassState state, Stat<Scalar> measured, Op key) =>
    arena.ToSpace(key).Map(space => (space, new RemeshTrace(
        target, measured, state.Rounds, state.Splits, state.Collapses, state.Flips, state.Features)));
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:429`
```csharp
static Fin<RewriteResult> Quadrangulate(RemeshOp.QuadField op, Op key) =>
    Equalize(op.Mesh, op.TargetLength.Value, op.Policy, key).Bind(pair => {
        MeshSpace space = pair.Space;
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:447`
```csharp
.Bind(uv => ExtractQuads(space, uv.Us, uv.Vs, pair.Trace, key)));
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:458`
```csharp
static Fin<RewriteResult> ExtractQuads(MeshSpace space, Arr<double> u, Arr<double> v, RemeshTrace trace, Op key) {
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:521`
```csharp
return emit.ToSpace(key).Map(mesh => new RewriteResult(
    mesh, trace, Some(new QuadProvenance(toArray(corners), toArray(patchOf), toArray(uOut), toArray(vOut), toArray(singular.Order())))));
```

**To**

```csharp
// PassVerdict DELETED
// RemeshTrace DELETED

public sealed record QuadLayout(Arr<int> Corners, Arr<int> PatchOf, Arr<double> U, Arr<double> V, Arr<int> SingularFaces);

public sealed record RemeshResult(
    MeshSpace Mesh, PositiveMagnitude TargetLength, Stat<Scalar> Deviation,
    int Iterations, int Splits, int Collapses, int Flips, int FeatureEdges,
    Option<QuadLayout> Quads);
```

```csharp
sealed record PassState(
    int Iterations, int Splits, int Collapses, int Flips, int FeatureEdges,
    Option<Stat<Scalar>> Deviation, bool Converged);
```

```csharp
static Fin<RemeshResult> Equalize(
    MeshSpace source, PositiveMagnitude target, RemeshPolicy policy, Op key) {
```

```csharp
Atom<Fin<PassState>> cell = Atom(Fin.Succ(new PassState(0, 0, 0, 0, 0, None, false)));
Fin<PassState> terminal = Cell.Converge(
    cell: cell,
    step: outcome => Some(outcome.Bind(Pass)),
    settled: outcome => outcome.Match(Succ: static state => state.Converged, Fail: static _ => true),
    budget: policy.Iterations,
    declined: key.InvalidResult()).Current;
return terminal.Bind(state => state.Deviation.Match(
    Some: measured => measured.Mean <= policy.ConvergenceBand.Value
        ? arena.ToSpace(key).Map(space => new RemeshResult(
            space, target, measured, state.Iterations, state.Splits, state.Collapses,
            state.Flips, state.FeatureEdges, None))
        : Fin.Fail<RemeshResult>(new GeometryFault.RemeshStalled(
            target, Some(target.Value * (1.0 + measured.Mean)), state.Iterations)),
    None: () => Fin.Fail<RemeshResult>(new GeometryFault.RemeshStalled(target, None, 0))));
```

```csharp
Fin<PassState> Pass(PassState state) {
    // split, collapse, and flip bodies stay in place
    return Project(arena, frozen, policy, key)
        .Bind(_ => Deviation(arena, targetAt, key))
        .Map(spread => state with {
            Iterations = state.Iterations + 1, Splits = state.Splits + splits,
            Collapses = state.Collapses + collapses, Flips = state.Flips + flips,
            FeatureEdges = features, Deviation = Some(spread),
            Converged = splits + collapses + flips == 0
                && spread.Mean <= policy.ConvergenceBand.Value,
        });
}
```

```csharp
// Emit DELETED
```

```csharp
static Fin<RemeshResult> Quadrangulate(RemeshOp request, RosyOrder order, Op key) =>
    Equalize(request.Mesh, request.TargetLength, request.Policy, key).Bind(remesh => {
        MeshSpace space = remesh.Mesh;
```

```csharp
.Bind(uv => ExtractQuads(remesh, uv.Us, uv.Vs, key)));
```

```csharp
static Fin<RemeshResult> ExtractQuads(RemeshResult remesh, Arr<double> u, Arr<double> v, Op key) {
    MeshSpace space = remesh.Mesh;
```

```csharp
return emit.ToSpace(key).Map(mesh => remesh with {
    Mesh = mesh,
    Quads = Some(new QuadLayout(
        toArray(corners), toArray(patchOf), toArray(uOut), toArray(vOut), toArray(singular.Order()))),
});
```

**Why**

`PassVerdict` duplicates `Fin`'s success/failure partition and adds three generated case types to carry one error and one convergence bit. `RemeshTrace` has the same lifetime and consumer as `RewriteResult`, while `Project<TOut>` exists only for an unconsumed generic relay. The current flow also erases `PositiveMagnitude`, re-admits it on failure, returns an internal tuple, and forwards construction through one-call `Emit`. `QuadProvenance` is not source history; it is the retained quad layout, and the branch-wide ruling retires `Provenance` for this meaning.

**Change**

Run `Cell.Converge` over `Atom<Fin<PassState>>`, letting projection or deviation failure terminate on `Fin` and retaining only the convergence fact on state. Merge trace fields directly into `RemeshResult`, preserve `PositiveMagnitude`, rename `QuadProvenance` to `QuadLayout`, carry equalization as a result instead of a tuple, and update quad extraction with one `with` expression. Delete `PassVerdict`, `RemeshTrace`, `RewriteResult.Project`, `PassState.Seed`, `PassState.Settled`, and `Emit`.

**Delta**

Target code-fence LOC: -31. Target declared surface: -2 module-level types, -3 nested case types, -2 class methods, -1 static field, and -1 nested property; two result types are rebuilt as two denser replacements, net -9 declared symbols.

**Ripples**

- `libs/dotnet/Rasm/.planning/Processing/intent.md`: delete `RewriteCase`, `VectorIntent.Rewrite`, and the `rewriteCase` dispatch arm; no repository consumer constructs the relay, and no replacement projection wrapper is added.
- `libs/dotnet/Rasm/.planning/Parametric/panelize.md`: consume `RemeshResult` directly, rename `QuadProvenance` to `QuadLayout`, and keep reading the optional quad payload without generic projection.
- `libs/dotnet/Rasm/README.md`, `libs/dotnet/Rasm/ARCHITECTURE.md`, and the remesh/panelize page prose and diagrams: use `RemeshResult` and quad-layout terminology, remove `VectorIntent.Rewrite`, `PassVerdict`, and separate-trace ownership claims.

# 5. Inline original-surface projection and fold effects without output collection

**From**

`libs/dotnet/Rasm/.planning/Processing/remesh.md:132`
```csharp
static Fin<(MeshSpace Space, RemeshTrace Trace)> Equalize(MeshSpace source, double target, RemeshPolicy policy, Op key) {
    using MeshEdit arena = MeshEdit.Of(source, ArenaPolicy.Canonical with { ParallelFloor = policy.ParallelFloor });
    return SourceIndex(source, key).Bind(frozen => {
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:168`
```csharp
if (Project(arena, frozen, policy, key).Case is Error fault) {
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:381`
```csharp
// --- [PROJECTION]
sealed record Source(SpatialIndex Index, (Point3d A, Point3d B, Point3d C)[] Faces);

static Fin<Source> SourceIndex(MeshSpace source, Op key) {
    using MeshEdit soup = MeshEdit.Of(source);
    BoundingBox[] boxes = new BoundingBox[soup.FaceCount];
    (Point3d A, Point3d B, Point3d C)[] corners = new (Point3d A, Point3d B, Point3d C)[soup.FaceCount];
    for (int f = 0; f < soup.FaceCount; f++) {
        boxes[f] = soup.Bounds(f);
        (int a, int b, int c) = soup.Face(f);
        corners[f] = (soup.Position(a), soup.Position(b), soup.Position(c));
    }
    return SpatialIndex.Build(SpatialKind.Bvh, boxes, BuildPolicy.Canonical, key)
        .Map(built => new Source(built, corners));
}
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:397`
```csharp
static Fin<Unit> Project(MeshEdit arena, Source source, RemeshPolicy policy, Op key) =>
    Range(0, arena.VertexCount).ToSeq().TraverseM(v => {
        Point3d p = arena.Position(v);
        return source.Index.Query(p, policy.ProjectCandidates.Value, key)
            .Map(hits => {
                (Point3d at, Option<double> _) = hits.Fold(
                    (At: p, Distance: Option<double>.None),
                    (best, f) => {
                        (Point3d foot, double d) = SpatialIndex.ClosestOnTriangle(p, source.Faces[f].A, source.Faces[f].B, source.Faces[f].C);
                        return best.Distance.Map(held => d < held).IfNone(true) ? (foot, Some(d)) : best;
                    });
                arena.SetPosition(v, at);
                return unit;
            });
    }).As().Map(_ => unit);
```

**To**

```csharp
static Fin<RemeshResult> Equalize(
    MeshSpace source, PositiveMagnitude target, RemeshPolicy policy, Op key) {
    using MeshEdit arena = MeshEdit.Of(
        source, ArenaPolicy.Canonical with { ParallelFloor = policy.ParallelFloor });
    using MeshEdit original = MeshEdit.Of(source);
    BoundingBox[] boxes = new BoundingBox[original.FaceCount];
    (Point3d A, Point3d B, Point3d C)[] faces = new (Point3d, Point3d, Point3d)[original.FaceCount];
    for (int face = 0; face < original.FaceCount; face++) {
        boxes[face] = original.Bounds(face);
        (int a, int b, int c) = original.Face(face);
        faces[face] = (original.Position(a), original.Position(b), original.Position(c));
    }
    return SpatialIndex.Build(SpatialKind.Bvh, boxes, BuildPolicy.Canonical, key).Bind(index => {
```

```csharp
return Reproject()
```

```csharp
// Source DELETED
// SourceIndex DELETED
// Project DELETED

Fin<Unit> Reproject() =>
    Range(0, arena.VertexCount).FoldM(unit, (_, vertex) => {
        Point3d point = arena.Position(vertex);
        return index.Query(point, policy.ProjectCandidates.Value, key).Map(hits => {
            (Point3d at, Option<double> _) = hits.Fold(
                (At: point, Distance: Option<double>.None),
                (best, face) => {
                    (Point3d foot, double distance) = SpatialIndex.ClosestOnTriangle(
                        point, faces[face].A, faces[face].B, faces[face].C);
                    return best.Distance.Map(held => distance < held).IfNone(true)
                        ? (foot, Some(distance))
                        : best;
                });
            arena.SetPosition(vertex, at);
            return unit;
        });
    }).As();
```

**Why**

`Source` is a private two-field carrier with one producer and one consumer; `SourceIndex` and `Project` are class methods used only inside `Equalize`. `TraverseM` materializes a `Seq<Unit>` that the projection immediately discards. LanguageExt's monadic fold sequences the same dependent `Fin<Unit>` effects while retaining only the unit state.

**Change**

Build the original-face boxes and corners at the start of `Equalize`, bind the BVH once around the pass fold, and move projection into the capturing `Reproject` local operation. Replace `ToSeq().TraverseM(...).Map(_ => unit)` with `Range.FoldM(unit, ...)` and one `.As()` re-anchor. Delete the `Source` carrier and both one-consumer class methods.

**Delta**

Target code-fence LOC: -6. Target class surface: -1 nested type and -2 class methods; +1 local operation, net -3 class-scope declarations and no output collection.

# 6. Fuse each field construction with its stripe sample

**From**

`libs/dotnet/Rasm/.planning/Processing/remesh.md:438`
```csharp
return SegmentKernel.CrossFieldAt(space, op.Symmetry.Key, None, None, seed, key)
    .Bind(baseDir => Direction.Of(value: Vector3d.CrossProduct(normal, baseDir), context: space.Tolerance, key: key))
    .Bind(turned =>
        from cross in VectorField.CrossField(space, op.Symmetry.Key, None, None, key)
        from rotated in VectorField.CrossField(space, op.Symmetry.Key, Some(Seq((a, turned))), None, key)
        select (Cross: cross, Rotated: rotated))
    .Bind(fields =>
        (SampleStripes(space, fields.Cross, frequency, key).ToValidation(),
         SampleStripes(space, fields.Rotated, frequency, key).ToValidation())
            .Apply(static (us, vs) => (Us: us, Vs: vs)).As().ToFin()
            .Bind(uv => ExtractQuads(space, uv.Us, uv.Vs, pair.Trace, key)));
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:451`
```csharp
static Fin<Arr<double>> SampleStripes(MeshSpace space, VectorField field, double frequency, Op key) =>
    Range(0, space.Native.Vertices.Count)
        .TraverseM(v => SegmentKernel.StripeAt(space, field, frequency, space.Native.Vertices[v], key)).As()
        .Map(static values => toArray(values));
```

**To**

```csharp
return SegmentKernel.CrossFieldAt(space, order, None, None, seed, key)
    .Bind(baseDirection => Direction.Of(
        Vector3d.CrossProduct(normal, baseDirection), space.Tolerance, key))
    .Bind(rotated =>
        (Stripes(None).ToValidation(),
         Stripes(Some(Seq((a, rotated)))).ToValidation())
            .Apply(static (u, v) => (U: u, V: v)).As().ToFin()
            .Bind(uv => ExtractQuads(remesh, uv.U, uv.V, key)));

Fin<Arr<double>> Stripes(Option<Seq<(int Vertex, Direction Hint)>> constraints) =>
    VectorField.CrossField(space, order, constraints, None, key)
        .Bind(field => Range(0, space.Native.Vertices.Count)
            .TraverseM(vertex => SegmentKernel.StripeAt(
                space, field, frequency, space.Native.Vertices[vertex], key)).As())
        .Map(static values => toArray(values));
```

**Why**

The body first sequences two independent field constructions, stores an intermediate pair, and only then re-enters accumulating validation for the two independent stripe samples. `SampleStripes` forwards one call and exists only for those branches.

**Change**

Make each independent branch construct its admitted field and immediately sample it, accumulate the two complete branches once with `Validation.Apply`, and keep one local `Stripes` operation over the only varying input: optional constraints. Delete `SampleStripes` and the intermediate field pair.

**Delta**

Target code-fence LOC: -5. Target class surface: -1 method; +1 local operation, net -1 class-scope member.

# 7. Label quad patches with the disjoint-set owner

**From**

`libs/dotnet/Rasm/.planning/Processing/remesh.md:34`
```csharp
using QuikGraph;
using QuikGraph.Algorithms;
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:474`
```csharp
UndirectedGraph<int, SEdge<int>> adjacency = new(allowParallelEdges: false);
adjacency.AddVertexRange(Enumerable.Range(0, soup.FaceCount).Where(f => !singular.Contains(f)));
Dictionary<(int, int), int> byEdge = [];
for (int f = 0; f < soup.FaceCount; f++) {
    if (singular.Contains(f)) { continue; }
    (int a, int b, int c) = soup.Face(f);
    foreach ((int p, int q) in (ReadOnlySpan<(int, int)>)[(a, b), (b, c), (c, a)]) {
        (int cp, int cq) = (int.Min(p, q), int.Max(p, q));
        if (byEdge.TryGetValue((cp, cq), out int g) && !singular.Contains(g)) { adjacency.AddEdge(new SEdge<int>(int.Min(f, g), int.Max(f, g))); }
        else { byEdge[(cp, cq)] = f; }
    }
}
Dictionary<int, int> faceComponent = [];
adjacency.ConnectedComponents(faceComponent);
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:498`
```csharp
int patch = faceComponent.GetValueOrDefault(located[0].Face);
```

**To**

```csharp
using QuikGraph.Collections;
```

```csharp
ForestDisjointSet<int> patches = new(capacity: soup.FaceCount - singular.Count);
for (int face = 0; face < soup.FaceCount; face++) {
    if (!singular.Contains(face)) { patches.MakeSet(face); }
}
Dictionary<(int, int), int> byEdge = [];
for (int face = 0; face < soup.FaceCount; face++) {
    if (singular.Contains(face)) { continue; }
    (int a, int b, int c) = soup.Face(face);
    foreach ((int p, int q) in (ReadOnlySpan<(int, int)>)[(a, b), (b, c), (c, a)]) {
        (int cp, int cq) = (int.Min(p, q), int.Max(p, q));
        if (byEdge.TryGetValue((cp, cq), out int adjacent) && !singular.Contains(adjacent)) {
            patches.Union(face, adjacent);
        }
        else { byEdge[(cp, cq)] = face; }
    }
}
```

```csharp
int patch = patches.FindSet(located[0].Face);
```

**Why**

The graph object stores edges only to run connected components once and then discards both the graph and component dictionary. QuikGraph's `ForestDisjointSet<int>` is the package's incremental component owner and directly retains the only needed result: a stable representative for each nonsingular face.

**Change**

Create one set per nonsingular face, union the two faces sharing each nonsingular edge, and read the representative as the patch label. Replace the graph and algorithms imports with `QuikGraph.Collections`; remove `UndirectedGraph`, `SEdge`, `ConnectedComponents`, and the second component map without hand-rolling union-find.

**Delta**

Target code-fence LOC: -3. Target declared surface: unchanged. Runtime working state removes one graph container, its edge objects, and one component dictionary; no new module-level symbol is added.

# 8. Localize phase-only geometry operations

**From**

`libs/dotnet/Rasm/.planning/Processing/remesh.md:209`
```csharp
bool crease = f1.Match(
    Some: g => Vector3d.VectorAngle(Normal(arena, f0), Normal(arena, g)) > featureAngle,
    None: static () => true);
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:221`
```csharp
static Vector3d Normal(MeshEdit arena, int f) {
    (int a, int b, int c) = arena.Face(f);
    return Vector3d.CrossProduct(arena.Position(b) - arena.Position(a), arena.Position(c) - arena.Position(a));
}
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:322`
```csharp
if (Opposite(arena, f0, a, b).Case is not int c) { continue; }
if (Opposite(arena, f1, a, b).Case is not int d || c == d) { continue; }
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:340`
```csharp
static Option<int> Opposite(MeshEdit arena, int f, int u, int v) {
    (int a, int b, int c) = arena.Face(f);
    return a != u && a != v ? Some(a) : b != u && b != v ? Some(b) : c != u && c != v ? Some(c) : Option<int>.None;
}
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:464`
```csharp
(double du, double dv) = (Spread(u[a], u[b], u[c]), Spread(v[a], v[b], v[c]));
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:494`
```csharp
Option<Seq<(Point3d At, int Face)>> ring = toSeq(Ring).Fold(
    Some(Seq<(Point3d At, int Face)>()),
    (acc, step) => acc.Bind(rows => Locate(soup, u, v, cellFaces, iu + step.Du, iv + step.Dv).Map(rows.Add)));
```

`libs/dotnet/Rasm/.planning/Processing/remesh.md:525`
```csharp
static Option<(Point3d At, int Face)> Locate(MeshEdit soup, Arr<double> u, Arr<double> v, Dictionary<(long Iu, long Iv), List<int>> cells, long cu, long cv) {
    foreach ((long du, long dv) in (ReadOnlySpan<(long, long)>)[(0, 0), (-1, 0), (0, -1), (-1, -1)]) {
        if (!cells.TryGetValue((cu + du, cv + dv), out List<int>? members)) { continue; }
        foreach (int f in members) {
            (int a, int b, int c) = soup.Face(f);
            double det = ((u[b] - u[a]) * (v[c] - v[a])) - ((u[c] - u[a]) * (v[b] - v[a]));
            if (Math.Abs(det) <= EpsilonPolicy.ZeroTolerance) { continue; }
            double wb = (((cu - u[a]) * (v[c] - v[a])) - ((cv - v[a]) * (u[c] - u[a]))) / det;
            double wc = (((cv - v[a]) * (u[b] - u[a])) - ((cu - u[a]) * (v[b] - v[a]))) / det;
            double wa = 1.0 - wb - wc;
            if (wa is < 0.0 or > 1.0 || wb is < 0.0 or > 1.0 || wc is < 0.0 or > 1.0) { continue; }
            (Point3d pa, Point3d pb, Point3d pc) = (soup.Position(a), soup.Position(b), soup.Position(c));
            return Some((new Point3d(
                (wa * pa.X) + (wb * pb.X) + (wc * pc.X),
                (wa * pa.Y) + (wb * pb.Y) + (wc * pc.Y),
                (wa * pa.Z) + (wb * pb.Z) + (wc * pc.Z)), f));
        }
    }
    return None;
}

static double Spread(double a, double b, double c) => Math.Max(a, Math.Max(b, c)) - Math.Min(a, Math.Min(b, c));
```

**To**

```csharp
bool crease = f1.Match(
    Some: adjacent => Vector3d.VectorAngle(FaceNormal(f0), FaceNormal(adjacent)) > featureAngle,
    None: static () => true);
```

```csharp
Vector3d FaceNormal(int face) {
    (int a, int b, int c) = arena.Face(face);
    return Vector3d.CrossProduct(
        arena.Position(b) - arena.Position(a), arena.Position(c) - arena.Position(a));
}
```

```csharp
if (Opposite(f0, a, b).Case is not int c) { continue; }
if (Opposite(f1, a, b).Case is not int d || c == d) { continue; }
```

```csharp
Option<int> Opposite(int face, int u, int v) {
    (int a, int b, int c) = arena.Face(face);
    return a != u && a != v ? Some(a)
        : b != u && b != v ? Some(b)
        : c != u && c != v ? Some(c)
        : None;
}
```

```csharp
(double du, double dv) = (
    Math.Max(u[a], Math.Max(u[b], u[c])) - Math.Min(u[a], Math.Min(u[b], u[c])),
    Math.Max(v[a], Math.Max(v[b], v[c])) - Math.Min(v[a], Math.Min(v[b], v[c])));
```

```csharp
Option<Seq<(Point3d At, int Face)>> ring = toSeq(Ring).Fold(
    Some(Seq<(Point3d At, int Face)>()),
    (acc, step) => acc.Bind(rows => Locate(iu + step.Du, iv + step.Dv).Map(rows.Add)));
```

```csharp
Option<(Point3d At, int Face)> Locate(long cu, long cv) {
    foreach ((long du, long dv) in (ReadOnlySpan<(long, long)>)[(0, 0), (-1, 0), (0, -1), (-1, -1)]) {
        if (!cellFaces.TryGetValue((cu + du, cv + dv), out List<int>? members)) { continue; }
        foreach (int face in members) {
            (int a, int b, int c) = soup.Face(face);
            double det = ((u[b] - u[a]) * (v[c] - v[a])) - ((u[c] - u[a]) * (v[b] - v[a]));
            if (Math.Abs(det) <= EpsilonPolicy.ZeroTolerance) { continue; }
            double wb = (((cu - u[a]) * (v[c] - v[a])) - ((cv - v[a]) * (u[c] - u[a]))) / det;
            double wc = (((cv - v[a]) * (u[b] - u[a])) - ((cu - u[a]) * (v[b] - v[a]))) / det;
            double wa = 1.0 - wb - wc;
            if (wa is < 0.0 or > 1.0 || wb is < 0.0 or > 1.0 || wc is < 0.0 or > 1.0) { continue; }
            (Point3d pa, Point3d pb, Point3d pc) = (soup.Position(a), soup.Position(b), soup.Position(c));
            return Some((new Point3d(
                (wa * pa.X) + (wb * pb.X) + (wc * pc.X),
                (wa * pa.Y) + (wb * pb.Y) + (wc * pc.Y),
                (wa * pa.Z) + (wb * pb.Z) + (wc * pc.Z)), face));
        }
    }
    return None;
}

// Spread DELETED
```

**Why**

`Normal`, `Opposite`, and `Locate` each depend on one phase's captured arena or extraction state and have no independent consumer; class placement manufactures reusable-looking surface and long parameter lists. `Spread` is a one-expression wrapper used once.

**Change**

Move `FaceNormal` into `Edges.Of`, `Opposite` into `Flip`, and `Locate` into `ExtractQuads`, capturing their phase state instead of forwarding it through parameters. Inline the two scalar ranges and delete `Spread`. Keep `Follows` and `Holds` at class scope because split and flip genuinely share them; keep the static `Ring` table because both corner location and vertex interning read the same fixed ordering.

**Delta**

Target code-fence LOC: -2. Target class surface: -4 methods; +3 local operations, net -4 class-scope members and -1 callable symbol overall.

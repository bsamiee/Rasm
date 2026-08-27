# 1. Collapse the two-row finish type into the policy fact it represents

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:64-70`, anchor `[SmartEnum] public sealed partial class BranchFinish`; `:74-86`, anchor `public sealed record SkeletonPolicy`; `:120-127`, anchor `public static Fin<CurveSkeleton> Apply`.

From:

```csharp
[SmartEnum]
public sealed partial class BranchFinish {
    public static readonly BranchFinish Sampled  = new(static skeleton => skeleton);
    public static readonly BranchFinish Smoothed = new(Skeletonize.Smooth);

    [UseDelegateFromConstructor]
    internal partial CurveSkeleton Resample(CurveSkeleton skeleton);
}

// SkeletonPolicy columns/default
BranchFinish Finish
Finish: BranchFinish.Smoothed

// Apply tail
.Map(state => op.Policy.Finish.Resample(Extract(state, op.Policy)));
```

To:

```csharp
// BranchFinish DELETED

// SkeletonPolicy column/default
bool SmoothBranches
SmoothBranches: true

// Apply tail
.Map(state => Extract(state, op.Policy));
```

Why: sampled versus smoothed is a two-state fact with no case payload. The generated smart enum contributes a public module type, two public rows, and a generated delegate member only to select identity versus one real transform. A policy boolean carries that decision; `Extract` reads it before deriving node evidence, so the genuine resampling capability remains without a second policy owner. Update Owner/Cases/Auto/Growth, the diagram, and density bar to describe `SmoothBranches` and no `BranchFinish` owner.

# 2. Resample contracted positions before deriving radius and section evidence

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:369-384`, anchors the node-coordinate fill and the radius/witness fold inside `Extract`; `:458-498`, anchors `internal static CurveSkeleton Smooth` and `static IEnumerable<int[]> Branches`.

From:

```csharp
for (int nId = 0; nId < nodes; nId++) {
    Point3d p = arena.Position(survivors[nId]);
    (nx[nId], ny[nId], nz[nId]) = (p.X, p.Y, p.Z);
}
```

```csharp
internal static CurveSkeleton Smooth(CurveSkeleton skeleton) {
    UndirectedGraph<int, SEdge<int>> graph = Enumerable.Range(0, skeleton.ArcCount)
        .Select(a => new SEdge<int>(skeleton.ArcFrom[a], skeleton.ArcTo[a]))
        .ToUndirectedGraph<int, SEdge<int>>(allowParallelEdges: false);
    (double[] nx, double[] ny, double[] nz) = ([.. skeleton.NodeX], [.. skeleton.NodeY], [.. skeleton.NodeZ]);
    foreach (int[] chain in Branches(skeleton, graph)) {
        if (chain.Length < 4) { continue; }
        double[] t = new double[chain.Length];
        for (int i = 1; i < chain.Length; i++) { t[i] = t[i - 1] + skeleton.NodeAt(chain[i - 1]).DistanceTo(skeleton.NodeAt(chain[i])); }
        IInterpolation sx = Interpolate.CubicSplineRobust(t, [.. chain.Select(n => skeleton.NodeX[n])]);
        IInterpolation sy = Interpolate.CubicSplineRobust(t, [.. chain.Select(n => skeleton.NodeY[n])]);
        IInterpolation sz = Interpolate.CubicSplineRobust(t, [.. chain.Select(n => skeleton.NodeZ[n])]);
        for (int i = 1; i < chain.Length - 1; i++) {
            (nx[chain[i]], ny[chain[i]], nz[chain[i]]) = (sx.Interpolate(t[i]), sy.Interpolate(t[i]), sz.Interpolate(t[i]));
        }
    }
    (int[] arcFrom, int[] arcTo) = ([.. skeleton.ArcFrom], [.. skeleton.ArcTo]);
    return skeleton with {
        NodeX = new Arr<double>(nx), NodeY = new Arr<double>(ny), NodeZ = new Arr<double>(nz),
        Reach = ReachOf(nx, ny, nz, arcFrom, arcTo, skeleton.Reach.Ceiling),
    };
}

static IEnumerable<int[]> Branches(CurveSkeleton skeleton, UndirectedGraph<int, SEdge<int>> graph) {
```

To:

```csharp
for (int nId = 0; nId < nodes; nId++) {
    Point3d p = arena.Position(survivors[nId]);
    (nx[nId], ny[nId], nz[nId]) = (p.X, p.Y, p.Z);
}
if (policy.SmoothBranches) {
    UndirectedGraph<int, SEdge<int>> branchGraph = tree
        .ToUndirectedGraph<int, SEdge<int>>(allowParallelEdges: false);
    foreach (int[] chain in Branches(branchGraph)) {
        if (chain.Length < 4) { continue; }
        double[] t = new double[chain.Length];
        for (int i = 1; i < chain.Length; i++) {
            int prior = chain[i - 1]; int at = chain[i];
            t[i] = t[i - 1] + new Point3d(nx[prior], ny[prior], nz[prior])
                .DistanceTo(new Point3d(nx[at], ny[at], nz[at]));
        }
        IInterpolation x = Interpolate.CubicSplineRobust(t, [.. chain.Select(n => nx[n])]);
        IInterpolation y = Interpolate.CubicSplineRobust(t, [.. chain.Select(n => ny[n])]);
        IInterpolation z = Interpolate.CubicSplineRobust(t, [.. chain.Select(n => nz[n])]);
        for (int i = 1; i < chain.Length - 1; i++) {
            double station = t[^1] * i / (chain.Length - 1);
            (nx[chain[i]], ny[chain[i]], nz[chain[i]]) =
                (x.Interpolate(station), y.Interpolate(station), z.Interpolate(station));
        }
    }
}

// local inside Extract
static IEnumerable<int[]> Branches(UndirectedGraph<int, SEdge<int>> graph) {
    EdgeKeySet visited = [];
    foreach (int anchor in graph.Vertices.Where(n => graph.AdjacentDegree(n) != 2)) {
        foreach (int start in Around(graph, anchor)) {
            if (!visited.Add((anchor, start))) { continue; }
            List<int> chain = [anchor, start];
            (int prior, int at) = (anchor, start);
            while (graph.AdjacentDegree(at) == 2) {
                int forward = Around(graph, at).First(w => w != prior);
                visited.Add((at, forward));
                chain.Add(forward);
                (prior, at) = (at, forward);
            }
            visited.Add((at, prior));
            yield return [.. chain];
        }
    }
}

// Skeletonize.Smooth DELETED
// Skeletonize.Branches DELETED; Extract.Branches retained locally
```

Why: `CubicSplineRobust(t, values)` is interpolatory, so evaluating at the source knots makes the current default pass a no-op. Moving the pass into `Extract` evaluates uniform chord-length stations before radius, witness, section, `SkeletonGraph`, and clearance-index construction; every dependent fact therefore derives from the resampled positions instead of becoming stale after a result mutation. The one-call `Smooth` and `Branches` class members disappear, and extraction constructs the final graph once from the settled coordinates and evidence.

# 3. Remove the unused radius callback that violates the clearance contract

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:74-79`, anchor `public sealed record SkeletonPolicy` and its trailing `RadiusMeasure` parameter.

From:

```csharp
public sealed record SkeletonPolicy(
    PositiveMagnitude LaplaceSeed, PositiveMagnitude ContractionScale, PositiveMagnitude Attraction,
    PositiveMagnitude CotangentCeiling, Dimension MaxIterations, Tolerance CollapseAreaRatio,
    Tolerance StallBand, double SamplingWeight, BranchFinish Finish, Dimension ParallelFloor,
    Dimension ProbeCeiling,
    Option<Func<Point3d, Point3d, Fin<double>>> RadiusMeasure = default) : IValidityEvidence {
```

To:

```csharp
public sealed record SkeletonPolicy(
    PositiveMagnitude LaplaceSeed, PositiveMagnitude ContractionScale, PositiveMagnitude Attraction,
    PositiveMagnitude CotangentCeiling, Dimension MaxIterations, Tolerance CollapseAreaRatio,
    Tolerance StallBand, double SamplingWeight, bool SmoothBranches, Dimension ParallelFloor,
    Dimension ProbeCeiling) : IValidityEvidence {

// SkeletonPolicy.RadiusMeasure DELETED
```

Why: `RadiusMeasure` has no read; extraction always uses Euclidean distance from an original boundary vertex to its contracted survivor. The page and `offset.md` fix `Radius` as distance-to-boundary, so an arbitrary callback would create a second authority capable of violating `ClearanceNode`. Remove its Auto and Growth claims.

# 4. Delete the one-modality request wrapper and require the operation key

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:93-94`, anchor `public sealed record SkeletonOp`; `:119-142`, anchors `Skeletonize.Apply` and `Admit`; `:159-180`, anchor `Contract`.

From:

```csharp
public sealed record SkeletonOp(MeshSpace Mesh, SkeletonPolicy Policy);

public static Fin<CurveSkeleton> Apply(SkeletonOp op, Op? key = null) {
    Op site = key.OrDefault();
    return Admit(op).Bind(_ => {
        using MeshEdit arena = MeshEdit.Of(op.Mesh, ArenaPolicy.Canonical with { ParallelFloor = op.Policy.ParallelFloor });
        return Contract(arena, op, site)
            .Bind(state => Surgery(state, op.Policy))
            .Map(state => op.Policy.Finish.Resample(Extract(state, op.Policy)));
    });
}
```

To:

```csharp
// SkeletonOp DELETED

public static Fin<CurveSkeleton> Apply(MeshSpace mesh, SkeletonPolicy policy, Op key) {
    return Admit(mesh, policy).Bind(_ => {
        using MeshEdit arena = MeshEdit.Of(mesh, ArenaPolicy.Canonical with { ParallelFloor = policy.ParallelFloor });
        return Contract(arena, policy, key)
            .Bind(state => Surgery(state, policy))
            .Map(state => Extract(state, policy));
    });
}

static Fin<Unit> Admit(MeshSpace mesh, SkeletonPolicy policy) =>
    mesh.Native.Faces.Count == 0 ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "empty mesh"))
    : !policy.IsValid ? Fin.Fail<Unit>(new GeometryFault.DegenerateInput(Kind.Mesh, None, "negative sampling weight"))
    : MeshKernel.TopologyDetailed(mesh).Bind(static topology => {

static Fin<ContractState> Contract(MeshEdit arena, SkeletonPolicy policy, Op key) {
    Array.Fill(wh, policy.Attraction.Value);
    Atom<Fin<ContractRound>> cell = Atom(Fin.Succ(
        new ContractRound(0, policy.LaplaceSeed.Value * Math.Sqrt(meanFace), 1.0, None)));
    // Round(arena, policy, ...)
    // budget: policy.MaxIterations
}
```

Why: `SkeletonOp` carries no modality, admission, invariant, or generated dispatch; `Apply` immediately unpacks its only fields. No file outside the target constructs it or calls `Skeletonize.Apply`, so direct parameters remove one public module type without a compatibility surface. Requiring `Op` also removes the `site` alias and `OrDefault` hop, and obeys the package ruling that optional operation keys belong only on public host-boundary entries. Update lead/index/Owner/Entry/diagram/density-bar references.

# 5. Let `Fin` carry a stalled contraction and delete `ContractStop`

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:53-62`, anchor `[SmartEnum] internal sealed partial class ContractStop`; `:152`, anchor `sealed record ContractRound`; `:191-208`, anchor `Round`.

From:

```csharp
[SmartEnum]
internal sealed partial class ContractStop {
    public static readonly ContractStop Settled = new(static (_, _) => Fin.Succ(unit));
    public static readonly ContractStop Stalled = new(static (round, ratio) =>
        Fin.Fail<Unit>(new GeometryFault.CollapseStalled(round, ratio)));

    [UseDelegateFromConstructor]
    internal partial Fin<Unit> Settle(int round, double ratio);
}

sealed record ContractRound(int Round, double Wl, double Ratio, Option<ContractStop> Stop);
```

```csharp
.Map(_ => {
    double ratio = ringArea.Sum() / totalSeed;
    return new ContractRound(
        Round: at.Round + 1, Wl: at.Wl * policy.ContractionScale.Value, Ratio: ratio,
        Stop: ratio <= policy.CollapseAreaRatio.Value ? Some(ContractStop.Settled)
            : at.Ratio - ratio < policy.StallBand.Value * at.Ratio ? Some(ContractStop.Stalled)
            : None);
});
```

To:

```csharp
// ContractStop DELETED

sealed record ContractRound(int Round, double Wl, double Ratio);
```

```csharp
.Bind(_ => {
    double ratio = ringArea.Sum() / totalSeed;
    int round = at.Round + 1;
    return ratio > policy.CollapseAreaRatio.Value
        && at.Ratio - ratio < policy.StallBand.Value * at.Ratio
            ? Fin.Fail<ContractRound>(new GeometryFault.CollapseStalled(round, ratio))
            : Fin.Succ(new ContractRound(round, at.Wl * policy.ContractionScale.Value, ratio));
});
```

Why: the two rows encode success and failure already owned by `Fin<ContractRound>`. `Settled` delays `Succ(unit)` and `Stalled` delays `Fail`; neither is an independent policy case. Failing from the round preserves `CollapseStalled` evidence and lets the carrier stop directly. Update Owner/Cases/Auto/diagram/density-bar references; add no replacement flag.

# 6. Replace the local atomic convergence machine with one bounded `FoldMaybe`

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:152`, anchor `sealed record ContractRound`; `:171-188`, anchors `Atom<Fin<ContractRound>> cell` and `Cell.Converge`; `:191`, anchor the `Round` signature.

From:

```csharp
sealed record ContractRound(int Round, double Wl, double Ratio);

Atom<Fin<ContractRound>> cell = Atom(value: Fin.Succ(
    new ContractRound(Round: 0, Wl: policy.LaplaceSeed.Value * Math.Sqrt(meanFace), Ratio: 1.0)));
Transition<Fin<ContractRound>> driven = Cell.Converge(
    cell: cell,
    step: state => Some(state.Bind(active => active.Round > 0 && active.Ratio <= policy.CollapseAreaRatio.Value
        ? Fin.Succ(active)
        : Round(arena, policy, active, wh, ringSeed, totalSeed, key))),
    settled: state => state.Match(
        Succ: active => active.Round > 0 && active.Ratio <= policy.CollapseAreaRatio.Value,
        Fail: static _ => true),
    budget: policy.MaxIterations,
    declined: key.InvalidResult());

return driven.Current.Bind(final =>
    guard(final.Round > 0 && final.Ratio <= policy.CollapseAreaRatio.Value,
        new GeometryFault.CollapseStalled(final.Round, final.Ratio)).ToFin()
        .Map(_ => {
            ForestDisjointSet<int> merged = new(capacity: n);
            for (int v = 0; v < n; v++) { merged.MakeSet(v); }
            return new ContractState(arena, original, faces, merged, [.. Enumerable.Range(0, n)]);
        }));
```

To:

```csharp
// ContractRound DELETED

Fin<(int Round, double Wl, double Ratio)> contracted = Range(0, policy.MaxIterations.Value).FoldMaybe(
    Fin.Succ((Round: 0, Wl: policy.LaplaceSeed.Value * Math.Sqrt(meanFace), Ratio: 1.0)),
    (state, _) => state.Match(
        Succ: active => active.Round > 0 && active.Ratio <= policy.CollapseAreaRatio.Value
            ? None
            : Some(Round(arena, policy, active, wh, ringSeed, totalSeed, key)),
        Fail: static _ => None));

return contracted.Bind(final =>
    guard(final.Round > 0 && final.Ratio <= policy.CollapseAreaRatio.Value,
        new GeometryFault.CollapseStalled(final.Round, final.Ratio)).ToFin()
        .Map(_ => {
            ForestDisjointSet<int> merged = new(capacity: n);
            for (int v = 0; v < n; v++) { merged.MakeSet(v); }
            return new ContractState(arena, original, faces, merged, [.. Enumerable.Range(0, n)]);
        }));
```

```csharp
static Fin<(int Round, double Wl, double Ratio)> Round(
    MeshEdit arena, SkeletonPolicy policy, (int Round, double Wl, double Ratio) at,
    double[] wh, double[] ringSeed, double totalSeed, Op key) =>
```

```csharp
? Fin.Fail<(int Round, double Wl, double Ratio)>(new GeometryFault.CollapseStalled(round, ratio))
: Fin.Succ((Round: round, Wl: at.Wl * policy.ContractionScale.Value, Ratio: ratio));
```

Why: contraction is one producer in one run; no thread observes its round state, so `Atom`, CAS transitions, and a declined-transition error are false concurrency. LanguageExt's `FoldMaybe` is the catalogued bounded fold whose folder owns the stop: a converged success or an already-committed failure returns `None`, while an active round commits `Some(next)`. This avoids the pure `FoldUntil` predicate's `(State, Value)` signature and does not re-derive the stop in a second predicate. The tuple deletes the single-use `ContractRound` type, the final guard preserves typed exhaustion, and `Round > 0` retains the mandatory first pass when the threshold is `1.0`.

# 7. Make radius equal the nearest boundary distance already witnessed

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:376-384`, anchors `radius[nId] += d`, the `best` witness update, and the radius-averaging loop.

From:

```csharp
double d = state.Original[o].DistanceTo(arena.Position(survivors[nId]));
radius[nId] += d;
count[nId]++;
if (o < seed[nId]) { seed[nId] = o; }
if (d < best[nId]) { (best[nId], witness[nId]) = (d, o); }
}
for (int nId = 0; nId < nodes; nId++) { radius[nId] /= double.Max(count[nId], 1); }
```

To:

```csharp
Point3d node = new(nx[nId], ny[nId], nz[nId]);
double d = state.Original[o].DistanceTo(node);
count[nId]++;
if (o < seed[nId]) { seed[nId] = o; }
if (d < best[nId]) { (best[nId], radius[nId], witness[nId]) = (d, d, o); }
}

// mean-radius division DELETED
```

Why: clearance radius is distance to the boundary, and extraction already finds the nearest original boundary vertex in `best`/`witness`. Averaging every merge-set distance overstates clearance and disconnects `Radius` from its witness. Measuring against the coordinate arrays also makes the result derive from task 2's resampled position rather than the pre-resampling arena vertex. Recording the minimum removes accumulation/division and repairs `Clearance(Point3d)`; `count` remains for section-moment normalization.

# 8. Store the composed `SkeletonGraph` instead of re-minting its rows from parallel columns

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:96-115`, anchors `public sealed record CurveSkeleton` and `public SkeletonGraph Graph`; `:392-397`, anchor `return new CurveSkeleton`.

From:

```csharp
public sealed record CurveSkeleton(
    Arr<double> NodeX, Arr<double> NodeY, Arr<double> NodeZ, Arr<double> Radius, Arr<double> SectionA,
    Arr<double> SectionB, Arr<int> Witness, Arr<int> ArcFrom, Arr<int> ArcTo, Arr<int> ArcOrigin, Arr<int> Component,
    ClearanceProbe Reach) {
    public int NodeCount => Radius.Count;
    public int ArcCount => ArcFrom.Count;
    public Point3d NodeAt(int n) => new(NodeX[n], NodeY[n], NodeZ[n]);
    public SkeletonGraph Graph => new(
        toSeq(Enumerable.Range(0, NodeCount).Select(n => new ClearanceNode(NodeAt(n), Radius[n], Witness[n]))),
        toSeq(Enumerable.Range(0, ArcCount).Select(a => new SkeletonArc(ArcFrom[a], ArcTo[a], ArcOrigin[a]))));
    internal (int From, int To) Ends(int primitive) => ArcCount > 0 ? (ArcFrom[primitive], ArcTo[primitive]) : (primitive, primitive);
```

```csharp
return new CurveSkeleton(
    NodeX: new Arr<double>(nx), NodeY: new Arr<double>(ny), NodeZ: new Arr<double>(nz),
    Radius: new Arr<double>(radius), SectionA: new Arr<double>(sectionA), SectionB: new Arr<double>(sectionB),
    Witness: new Arr<int>(witness), ArcFrom: new Arr<int>(arcFrom), ArcTo: new Arr<int>(arcTo),
    ArcOrigin: new Arr<int>(arcOrigin), Component: new Arr<int>(arcComponent),
    Reach: ReachOf(nx, ny, nz, arcFrom, arcTo, policy.ProbeCeiling));
```

To:

```csharp
public sealed record CurveSkeleton(
    SkeletonGraph Graph, Arr<double> SectionA, Arr<double> SectionB, Arr<int> Component, ClearanceProbe Reach) {
    public ClearanceNode Clearance(Point3d probe) {
        (double distance, int primitive, double t) = Reach.Nearest(probe);
        (int from, int to) = Graph.Arcs.Count > 0
            ? (Graph.Arcs[primitive].From, Graph.Arcs[primitive].To)
            : (primitive, primitive);
        double radius = ((1.0 - t) * Graph.Nodes[from].Radius) + (t * Graph.Nodes[to].Radius);
        return new ClearanceNode(probe, radius - distance, primitive);
    }
}

// CurveSkeleton.NodeX DELETED
// CurveSkeleton.NodeY DELETED
// CurveSkeleton.NodeZ DELETED
// CurveSkeleton.Radius DELETED
// CurveSkeleton.Witness DELETED
// CurveSkeleton.ArcFrom DELETED
// CurveSkeleton.ArcTo DELETED
// CurveSkeleton.ArcOrigin DELETED
// CurveSkeleton.NodeCount DELETED
// CurveSkeleton.ArcCount DELETED
// CurveSkeleton.NodeAt DELETED
// CurveSkeleton.Ends DELETED
```

```csharp
SkeletonGraph output = new(
    toSeq(Enumerable.Range(0, nodes).Select(n => new ClearanceNode(
        new Point3d(nx[n], ny[n], nz[n]), radius[n], witness[n]))),
    toSeq(Enumerable.Range(0, tree.Length).Select(a =>
        new SkeletonArc(arcFrom[a], arcTo[a], arcOrigin[a]))));
return new CurveSkeleton(
    Graph: output, SectionA: new Arr<double>(sectionA), SectionB: new Arr<double>(sectionB),
    Component: new Arr<int>(arcComponent),
    Reach: ReachOf(nx, ny, nz, arcFrom, arcTo, policy.ProbeCeiling));
```

Why: `ClearanceNode`, `SkeletonArc`, and `SkeletonGraph` already own the exact result shape, yet `CurveSkeleton` stores eight parallel primitive columns and reconstructs those owners on every `Graph` read. Carrying the graph directly replaces those columns with one existing owner, removes three public aliases/projections and one internal helper, and makes unequal coordinate or endpoint counts unrepresentable. The section and component arrays survive because they carry genuinely additional per-node and per-arc evidence absent from the shared clearance family.

Ripples: in `libs/dotnet/Rasm.Fabrication/.planning/Toolpath/guard.md:456-470`, validate `channel.Graph.Nodes`, `channel.Graph.Arcs`, `SectionA`/`SectionB`, and `Component` against the graph's authoritative node and arc counts; this also closes the current omission that never checks section lengths or finiteness. At `:918-926`, build benchmark probes from `channel.Graph.Nodes.Select(node => node.At)` and `channel.Graph.Arcs` rather than reconstructing points and endpoints from the deleted columns. Update the target lead/Owner/Auto/Output/Boundary/density-bar prose to state that `CurveSkeleton` carries the composed graph plus its section/component sidecars.

# 9. Build the clearance index from the stored graph owner

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:392-397`, anchor the `CurveSkeleton` construction and its `ReachOf` call; `:448-456`, anchor `internal static ClearanceProbe ReachOf`.

From:

```csharp
Reach: ReachOf(nx, ny, nz, arcFrom, arcTo, policy.ProbeCeiling));
```

```csharp
internal static ClearanceProbe ReachOf(double[] nx, double[] ny, double[] nz, int[] arcFrom, int[] arcTo, Dimension ceiling) {
    int primitives = arcFrom.Length > 0 ? arcFrom.Length : nx.Length;
    (Point3d[] from, Point3d[] to) = (new Point3d[primitives], new Point3d[primitives]);
    for (int p = 0; p < primitives; p++) {
        (int a, int b) = arcFrom.Length > 0 ? (arcFrom[p], arcTo[p]) : (p, p);
        (from[p], to[p]) = (new Point3d(nx[a], ny[a], nz[a]), new Point3d(nx[b], ny[b], nz[b]));
    }
    return ClearanceProbe.Of(from: new Arr<Point3d>(from), to: new Arr<Point3d>(to), ceiling: ceiling);
}
```

To:

```csharp
Reach: ReachOf(output, policy.ProbeCeiling));

// local inside Extract
static ClearanceProbe ReachOf(SkeletonGraph graph, Dimension ceiling) {
    int primitives = graph.Arcs.Count > 0 ? graph.Arcs.Count : graph.Nodes.Count;
    (Point3d[] from, Point3d[] to) = (new Point3d[primitives], new Point3d[primitives]);
    for (int p = 0; p < primitives; p++) {
        (int a, int b) = graph.Arcs.Count > 0
            ? (graph.Arcs[p].From, graph.Arcs[p].To)
            : (p, p);
        (from[p], to[p]) = (graph.Nodes[a].At, graph.Nodes[b].At);
    }
    return ClearanceProbe.Of(new Arr<Point3d>(from), new Arr<Point3d>(to), ceiling);
}

// Skeletonize.ReachOf DELETED; Extract.ReachOf retained locally
```

Why: coordinate and endpoint arrays are a second parameter-level representation after extraction constructs `SkeletonGraph`. Passing the existing owner removes five parameters, makes every clearance primitive derive from the exact node and arc rows returned to callers, and retains the one-call helper as an extraction-local derivation rather than a class member.

# 10. Remove unused surgery work and an avoidable set copy

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:282-297`, anchors `int rounds = 0`, `rounds++`, and `foreach (int f in uFaces.ToArray())`.

From:

```csharp
int rounds = 0;
// ...
rounds++;
foreach (int f in uFaces.ToArray()) {
```

To:

```csharp
// rounds DELETED
// rounds++ DELETED
foreach (int f in uFaces) {
```

Why: `rounds` is never read. The loop does not mutate `uFaces`: rewired faces enter `facesOf[v]`, a distinct set because `u != v`, and `facesOf[u]` is removed after enumeration. Direct enumeration removes one array allocation per collapse.

# 11. Localize the surgery-only cost calculation

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:320-326`, anchor local `Link`/`Incident`; `:336-340`, anchor class member `Cost`.

From:

```csharp
static double Cost(MeshEdit arena, UndirectedGraph<int, SEdge<int>> adjacency, int u, int v, double lambda) {
    double length = arena.Position(u).DistanceTo(arena.Position(v));
    double sampling = Around(adjacency, u).Sum(w => arena.Position(u).DistanceTo(arena.Position(w)));
    return length + (lambda * length * sampling);
}
```

To:

```csharp
// local inside Surgery beside Link and Incident
static double Cost(MeshEdit arena, UndirectedGraph<int, SEdge<int>> graph, int u, int v, double lambda) {
    double length = arena.Position(u).DistanceTo(arena.Position(v));
    double sampling = Around(graph, u).Sum(w => arena.Position(u).DistanceTo(arena.Position(w)));
    return length + (lambda * length * sampling);
}

// Skeletonize.Cost DELETED; Surgery.Cost retained locally
```

Why: both calls belong to the surgery queue, so `Cost` is kernel-local rather than a class operation. Localizing removes one class-level member. Keep `Around` at class scope because surgery and `Extract.Branches` both consume it.

# 12. Read traversed axis solutions without copying the `Seq`

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:236-245`, anchor `toSeq(rhs).TraverseM` and terminal `Map`.

From:

```csharp
.Map(axes => {
    Arr<double>[] solved = [.. axes];
    for (int v = 0; v < n; v++) { arena.SetPosition(v, new Point3d(solved[0][v], solved[1][v], solved[2][v])); }
    return unit;
});
```

To:

```csharp
.Map(axes => {
    ReadOnlySpan<Arr<double>> solved = axes.AsSpan();
    for (int v = 0; v < n; v++) { arena.SetPosition(v, new Point3d(solved[0][v], solved[1][v], solved[2][v])); }
    return unit;
});

// Arr<double>[] solved copy DELETED
```

Why: `TraverseM` remains the correct dependent, short-circuiting inversion and each `LinearSolution.IsValid` gate stays before writes. `Seq.AsSpan()` is the catalogued zero-copy read; materializing another array adds no value.

# 13. Inline the one-use face-membership predicate

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:293-295`, anchor the live shared-face gate; `:329`, anchor `static bool Holds`.

From:

```csharp
if (!(facesOf.TryGetValue(u, out IndexSet? uFaces) && uFaces.Any(f => arena.Alive(f) && Holds(arena.Face(f), v)))) {
    continue;
}
static bool Holds((int A, int B, int C) face, int v) => face.A == v || face.B == v || face.C == v;
```

To:

```csharp
if (!(facesOf.TryGetValue(u, out IndexSet? uFaces)
    && uFaces.Any(f => arena.Alive(f)
        && arena.Face(f) is (var a, var b, var c)
        && (a == v || b == v || c == v)))) {
    continue;
}

// Skeletonize.Holds DELETED
```

Why: the predicate has one call and only destructures one tuple for three comparisons. Inlining removes a class member and keeps the full stale-edge gate at its decision site.

# 14. Compare the area lane against area rather than doubled area

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:217-222`, anchors `double twoArea` and its floor gate in `Assemble`; `:259-264`, anchor the cross-product floor gate in `KillDegenerate`.

From:

```csharp
double twoArea = Vector3d.CrossProduct(pb - pa, pc - pa).Length;
if (twoArea <= areaFloor) { continue; }
```

```csharp
if (Vector3d.CrossProduct(arena.Position(b) - arena.Position(a), arena.Position(c) - arena.Position(a)).Length <= areaFloor) {
    arena.KillFace(f);
}
```

To:

```csharp
double twoArea = Vector3d.CrossProduct(pb - pa, pc - pa).Length;
if (0.5 * twoArea < areaFloor) { continue; }
```

```csharp
double twoArea = Vector3d.CrossProduct(
    arena.Position(b) - arena.Position(a), arena.Position(c) - arena.Position(a)).Length;
if (0.5 * twoArea < areaFloor) { arena.KillFace(f); }
```

Why: `ToleranceLane.Area` derives a squared-length area threshold, while the cross-product magnitude is twice the triangle area. Comparing that magnitude directly to `areaFloor` admits triangles between one-half and one full floor in both assembly and face killing. The same `0.5 * twoArea < areaFloor` gate matches the mesh owner's established degeneracy test while preserving `twoArea` as the denominator `Cotangent.OfEdges` requires.

# 15. Inline degenerate-face removal into its only contraction round

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:195-198`, anchor `KillDegenerate(arena, areaFloor)`; `:259-267`, anchor `static void KillDegenerate`.

From:

```csharp
double areaFloor = arena.Tolerance.For(lane: ToleranceLane.Area).Value;
KillDegenerate(arena, areaFloor);
double[] ringArea = RingAreas(arena);

static void KillDegenerate(MeshEdit arena, double areaFloor) {
    for (int f = 0; f < arena.FaceCount; f++) {
        if (!arena.Alive(f)) { continue; }
        (int a, int b, int c) = arena.Face(f);
        if (Vector3d.CrossProduct(arena.Position(b) - arena.Position(a), arena.Position(c) - arena.Position(a)).Length <= areaFloor) {
            arena.KillFace(f);
        }
    }
}
```

To:

```csharp
double areaFloor = arena.Tolerance.For(lane: ToleranceLane.Area).Value;
for (int f = 0; f < arena.FaceCount; f++) {
    if (!arena.Alive(f)) { continue; }
    (int a, int b, int c) = arena.Face(f);
    double twoArea = Vector3d.CrossProduct(
        arena.Position(b) - arena.Position(a), arena.Position(c) - arena.Position(a)).Length;
    if (0.5 * twoArea < areaFloor) { arena.KillFace(f); }
}
double[] ringArea = RingAreas(arena);

// Skeletonize.KillDegenerate DELETED
```

Why: face killing is the ordered middle of one contraction round, between coordinate write-back and live one-ring refresh. Inlining exposes that dependency and removes a one-call class member. `RingAreas` remains because setup and every round both consume it; task 14's area-unit correction moves with the body unchanged.

# 16. Inline the one-call union-find mutation into surgery

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:144-150`, anchor `sealed record ContractState`; `:315-316`, anchors `adjacency.RemoveVertex(u)` and `state.Collapse`.

From:

```csharp
sealed record ContractState(MeshEdit Arena, Point3d[] Original, (int A, int B, int C)[] OriginalFaces,
    ForestDisjointSet<int> Merged, int[] Live) {
    internal int Survivor(int original) => Live[Merged.FindSet(original)];
    internal void Collapse(int victim, int survivor) {
        if (Merged.Union(victim, survivor)) { Live[Merged.FindSet(survivor)] = survivor; }
    }
}
```

```csharp
adjacency.RemoveVertex(u);
state.Collapse(victim: u, survivor: v);
```

To:

```csharp
sealed record ContractState(MeshEdit Arena, Point3d[] Original, (int A, int B, int C)[] OriginalFaces,
    ForestDisjointSet<int> Merged, int[] Live) {
    internal int Survivor(int original) => Live[Merged.FindSet(original)];
}

// ContractState.Collapse DELETED
```

```csharp
adjacency.RemoveVertex(u);
if (state.Merged.Union(u, v)) { state.Live[state.Merged.FindSet(v)] = v; }
```

Why: collapse mutation occurs at one surgery site immediately after the victim leaves adjacency. Inlining removes a private member and keeps the union direction, representative lookup, and live-survivor update beside the topology mutation they must follow. `Survivor` remains because extraction reads that projection repeatedly.

# 17. Remove imports with no fence consumer

Location: `libs/dotnet/Rasm/.planning/Meshing/skeleton.md:39-40`, anchors QuikGraph observer and search imports.

From:

```csharp
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;
```

To:

```csharp
// QuikGraph.Algorithms.Observers DELETED
// QuikGraph.Algorithms.Search DELETED
```

Why: neither namespace contributes a symbol. Keep `QuikGraph.Algorithms` for MST/components, `QuikGraph.Collections` for union-find, MathNet interpolation for retained resampling, and `EdgeKeySet` for branch traversal.

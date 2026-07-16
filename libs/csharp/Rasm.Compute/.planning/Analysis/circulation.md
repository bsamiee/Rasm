# [COMPUTE_CIRCULATION]

Rasm.Compute egress/life-safety runner: the `Discipline.Circulation` arm of the `Analysis/assessment` spine (IBC Ch.10 / EN route rows on the one `AssessmentRoute` axis). Space nodes, door/opening adjacency, and exit targets read off the concrete `Rasm.Element` `ElementGraph` directly, never a second graph store; each request builds a `QuikGraph` `AdjacencyGraph` view — travel distance over `ShortestPathsDijkstra`/`ShortestPathsAStar`, exit capacity as a push-relabel max-flow/min-cut over the admitted `Google.OrTools.Graph` `MaxFlow` (occupant-load supplies, door/corridor widths as arc capacities, the saturated `Flow == Capacity` arcs naming the min-cut bottleneck), occupant load distributed at least travel cost through `MinCostFlow`. Every check folds every requested space and lands as the one uniform `AssessmentResult` fact stream the spine writes back — dead-end and common-path included.

Ingress is honest: targets, spaces, and adjacency arrive off the concrete graph, and planar space boundaries resolve through the same `GeometrySource` content-key port every geometry-reading runner threads (the floor-plate upgrade decoding the kernel slice-stack wire — `Rasm/Meshing/slice` `Slicing.Apply` story contours through `LayerPlan.AtElevations`, outer-CCW/holes-CW — Compute decodes, never re-slices). Isovist/visibility polygons, corridor medial-axis clearance, and occupant areas fold through the centrally-pinned `NetTopologySuite`/`Clipper2` float production-plane tools at the discipline boundary, the kernel staying the exact-geometry owner. Occupant-load factors are policy rows (IBC Table 1004.5 / EN occupancy classes), allowable travel distances policy data on the request; a space whose boundary cannot resolve rails `AnalysisFailed(SolvePhase.Admission, FailureKind.Input, …)`, never a silent skip. Zero new central pins — OrTools/QuikGraph/NTS/Clipper2 are admitted substrate, the folder csproj rows landing with this first compose.

## [01]-[INDEX]

- [01]-[EGRESS_GRAPH]: the space-adjacency view off the concrete graph; Dijkstra/A* travel distance; max-flow/min-cut exit capacity; occupant-load distribution; dead-end/common-path checks.
- [02]-[PLANAR_SIDE]: isovist/visibility polygons, corridor medial-axis clearance, and occupant areas over NTS/Clipper2; the slice-stack floor-plate ingress.

## [02]-[EGRESS_GRAPH]

- Owner: `EgressGraph` the per-request space-adjacency view (space nodes `NodeId`-keyed, door/corridor edges with clear width and length, exits marked), built once from the concrete `ElementGraph` and discarded with the run — a persistent second graph store is the deleted form; `OccupancyClass` `[SmartEnum<string>]` the occupant-load-factor policy rows (IBC Table 1004.5 / EN vocabulary as data); `EgressPolicy` the request policy record; `CirculationAnalysis` the runner fold; `EgressFinding` the per-space typed finding.
- Cases: three checks over one adjacency view — travel distance (`ShortestPathsDijkstra` to each nearest exit, `ShortestPathsAStar` on large plates, worst path against `AllowableTravel`), exit capacity (the `MaxFlow` push-relabel over a super-source fanning occupant-load supplies to a super-sink draining exits, arc capacities door-width × capacity rate; a non-`OPTIMAL` `MaxFlow.Status` is a typed `(Solve, Numeric)` failure naming the status, never a silent zero; the saturated arcs name the min-cut bottleneck), and dead-end/common-path (folds over the same Dijkstra accessors — a single-exit path past the dead-end allowance, and the shared prefix of the two shortest disjoint exit paths against the common-path allowance).
- Entry: `Run(graph, request, geometry, clocks)` builds the view, runs the three folds, distributes occupant load at least travel cost through `MinCostFlow`, and mints the fact stream (per-space `travel-distance`, `exit-capacity`, `min-cut` bottleneck references, `dead-end`/`common-path` lengths, the governing ratio the worst of travel/allowable and demand/capacity); an unreachable space rails `AnalysisFailed(Solve, Input, "<egress-unreachable:…>")` — a life-safety fact, typed and cached, never dropped.
- Receipt: rides the one `ComputeReceipt.Assessment` case, no circulation-local receipt; the min-cut bottleneck and achieved throughput land as facts, auditable off the baked node.
- Packages: QuikGraph (`AdjacencyGraph<TVertex, TEdge>`, `ShortestPathsDijkstra`/`ShortestPathsAStar` → `TryFunc<TVertex, IEnumerable<TEdge>>` accessors, the SCC census — its first Compute consumer), Google.OrTools (`Google.OrTools.Graph` `MaxFlow`/`MinCostFlow` natives — each `IDisposable`, `int` node/arc indices, `long` capacities/costs; CP-SAT/MILP stay `Solver/optimizer`'s), Rasm.Element (`ElementGraph`, `NodeId`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new occupancy class is one `OccupancyClass` row; a new egress check is one fold over the same view; a new code edition is the route row's `SolverVersion` bump; zero new surface — a `TravelDistanceAnalyzer`/`ExitCapacityAnalyzer` sibling family the collapsed defect, a managed Edmonds-Karp beside the OrTools push-relabel the rejected reinvention. Seeded runners land one row/route/runner when chartered: MEP network-flow (`Discipline.Water`, the Todini global-gradient algebra over the sparse solver + QuikGraph), geometric room-acoustics (image-source over the clash BVH), and the `StronglyConnectedComponents` wayfinding deepening for multi-storey routes.
- Boundary: flow engines are the admitted `Google.OrTools.Graph` natives (capacities `long` — clear widths quantize to millimetres so the integers are exact), the min-cut reading off the saturated arcs (`Flow(arc) == Capacity`), never a second cut algorithm; the path algebra is QuikGraph's `TryFunc` accessors (a false try the unreachable-space typed failure), never a hand-rolled Dijkstra; occupant load is `OccupancyClass` factor × the planar occupant area from `[03]`, never a hardcoded count; every requested space folds — an unresolvable boundary, an unreachable space, and an over-capacity exit are all typed outcomes, never silent skips; a deterministic egress failure caches as a Failed node under the spine's `AnalysisFailed` lifecycle.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// IBC Table 1004.5 / EN occupancy classes as policy rows: the m²-per-occupant factor is row data; a new class is one row.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class OccupancyClass {
    public static readonly OccupancyClass Assembly = new("assembly", areaPerOccupantM2: 0.65);
    public static readonly OccupancyClass Business = new("business", areaPerOccupantM2: 14.0);
    public static readonly OccupancyClass Educational = new("educational", areaPerOccupantM2: 1.9);
    public static readonly OccupancyClass Mercantile = new("mercantile", areaPerOccupantM2: 5.6);
    public static readonly OccupancyClass Residential = new("residential", areaPerOccupantM2: 18.6);
    public static readonly OccupancyClass Storage = new("storage", areaPerOccupantM2: 46.5);

    public double AreaPerOccupantM2 { get; }
}

// Request policy: allowable travel/dead-end/common-path lengths (route-row values the caller may tighten), minimum clear width, per-metre capacity rate.
public sealed record EgressPolicy(
    double AllowableTravelM, double AllowableDeadEndM, double AllowableCommonPathM,
    double MinimumClearWidthM, double CapacityPerMetreWidth) {
    public static readonly EgressPolicy Ibc = new(AllowableTravelM: 76.0, AllowableDeadEndM: 6.1, AllowableCommonPathM: 23.0, MinimumClearWidthM: 0.813, CapacityPerMetreWidth: 197.0);
}

// --- [MODELS] ------------------------------------------------------------------------------
// Per-request space-adjacency view: seam NodeId spaces, door/corridor edges with clear width and length, exits marked — discarded with the run.
public sealed record EgressEdge(NodeId From, NodeId To, double ClearWidthM, double LengthM) : QuikGraph.IEdge<NodeId> {
    NodeId QuikGraph.IEdge<NodeId>.Source => From;
    NodeId QuikGraph.IEdge<NodeId>.Target => To;
}

public sealed record EgressGraph(
    QuikGraph.AdjacencyGraph<NodeId, EgressEdge> Adjacency,
    Seq<(NodeId Space, double AreaM2, OccupancyClass Occupancy)> Spaces,
    Seq<NodeId> Exits) {
    public int OccupantLoad((NodeId Space, double AreaM2, OccupancyClass Occupancy) space) =>
        (int)Math.Ceiling(space.AreaM2 / space.Occupancy.AreaPerOccupantM2);
}

// Per-space typed finding the fact stream projects: worst travel path, nearest exit, dead-end/common-path lengths — one shape, never per-check records.
public readonly record struct EgressFinding(NodeId Space, double TravelM, Option<NodeId> NearestExit, double DeadEndM, double CommonPathM);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class CirculationAnalysis {
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Circulation request, GeometrySource geometry, ClockPolicy clocks) =>
        from view in EgressView.Of(graph, request, geometry)
        from findings in Travel(view, request.Policy)
        from capacity in Capacity(view, request.Policy)
        let govern = Governing(findings, capacity, request.Policy)
        from travel in findings.TraverseM(f => AssessmentFact.Ratio($"{f.Space.Value}/travel-distance", f.TravelM / request.Policy.AllowableTravelM)).As()
        from deadEnds in findings.Filter(static f => f.DeadEndM > 0.0).TraverseM(f => AssessmentFact.Ratio($"{f.Space.Value}/dead-end", f.DeadEndM / request.Policy.AllowableDeadEndM)).As()
        from commonPaths in findings.Filter(static f => f.CommonPathM > 0.0).TraverseM(f => AssessmentFact.Ratio($"{f.Space.Value}/common-path", f.CommonPathM / request.Policy.AllowableCommonPathM)).As()
        from exits in AssessmentFact.Rows(
            AssessmentFact.Ratio("exit-capacity", capacity.DemandOccupants / Math.Max(1.0, capacity.ThroughputOccupants)),
            AssessmentFact.Measure("evacuation-throughput", Dimension.Dimensionless, capacity.ThroughputOccupants))
        select AssessmentResult.Of(
            request.Route,
            travel + deadEnds + commonPaths + exits
                + capacity.Bottleneck.Map(static edge => AssessmentFact.Reference("min-cut-bottleneck", edge.From)),
            govern,
            new Provenance("CirculationAnalysis", request.Route.Standard, request.Route.SolverVersion, clocks.Now));

    // TRAVEL: one Dijkstra accessor per exit-rooted reverse view (edge weights the corridor lengths); the worst-space
    // path, the dead-end fold (a single path past the allowance), and the common-path fold (the shared prefix of the two
    // shortest edge-disjoint paths) ride the same TryFunc accessors; an unreachable space is the typed life-safety failure.
    static Fin<Seq<EgressFinding>> Travel(EgressGraph view, EgressPolicy policy) {
        var paths = view.Adjacency.ShortestPathsDijkstra(static edge => edge.LengthM, view.Exits.Head.IfNone(() => default));
        return view.Spaces.Traverse(space =>
            paths(space.Space, out System.Collections.Generic.IEnumerable<EgressEdge>? path)
                ? Fin.Succ(new EgressFinding(space.Space, toSeq(path).Sum(static e => e.LengthM), view.Exits.Head, DeadEnd(view, space.Space), CommonPath(view, space.Space)))
                : Fin.Fail<EgressFinding>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Input, $"<egress-unreachable:{space.Space.Value}>")));
    }

    // CAPACITY: super-source -> space supplies -> width-capacitated door arcs -> exits -> super-sink; widths quantize to
    // millimetres so long capacities are exact, a non-OPTIMAL status the typed solve verdict, the saturated arcs the min-cut.
    static Fin<(double DemandOccupants, double ThroughputOccupants, Seq<EgressEdge> Bottleneck)> Capacity(EgressGraph view, EgressPolicy policy) {
        using var flow = new Google.OrTools.Graph.MaxFlow();
        var index = view.Spaces.Map(static s => s.Space).Append(view.Exits).Distinct().ToSeq();
        int NodeOf(NodeId id) => index.FindIndex(v => v == id) + 2;
        foreach (var space in view.Spaces) { flow.AddArcWithCapacity(0, NodeOf(space.Space), view.OccupantLoad(space)); }
        Seq<(int Arc, EgressEdge Edge)> arcs = view.Adjacency.Edges.ToSeq().Map(edge =>
            (flow.AddArcWithCapacity(NodeOf(edge.From), NodeOf(edge.To), (long)(edge.ClearWidthM * 1000.0 * policy.CapacityPerMetreWidth / 1000.0)), edge));
        foreach (NodeId exit in view.Exits) { flow.AddArcWithCapacity(NodeOf(exit), 1, long.MaxValue / 4); }
        return flow.Solve(0, 1) switch {
            Google.OrTools.Graph.MaxFlow.Status.OPTIMAL => Fin.Succ((
                (double)view.Spaces.Sum(view.OccupantLoad),
                (double)flow.OptimalFlow(),
                arcs.Filter(pair => flow.Flow(pair.Arc) == flow.Capacity(pair.Arc)).Map(static pair => pair.Edge))),
            var status => Fin.Fail<(double, double, Seq<EgressEdge>)>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, $"<egress-flow:{status}>")),
        };
    }

    static double Governing(Seq<EgressFinding> findings, (double Demand, double Throughput, Seq<EgressEdge> Cut) capacity, EgressPolicy policy) =>
        Math.Max(
            findings.Map(f => f.TravelM / policy.AllowableTravelM).Max() | 0.0,
            capacity.Demand / Math.Max(1.0, capacity.Throughput));

    static double DeadEnd(EgressGraph view, NodeId space) { /* the single-exit-path over-allowance fold */ }

    static double CommonPath(EgressGraph view, NodeId space) { /* the shared-prefix fold over the two shortest edge-disjoint exit paths */ }
}
```

## [03]-[PLANAR_SIDE]

- Owner: `EgressView` the ingress projection building the view — space boundaries resolved through the `GeometrySource` port (or decoded off the kernel slice-stack story contours), occupant areas the NTS polygon areas, corridor clearance the Clipper2 inward offset, isovists the planar sight fold; `SpaceBoundary` the resolved planar carrier.
- Entry: `Of(graph, request, geometry)` resolves each space's boundary by content key through the `GeometrySource` port (an unresolvable boundary rails `AnalysisFailed(Admission, Input, "<space-boundary-unresolved:…>")`), derives occupant areas off NTS `Polygon.Area`, corridor clear widths off the Clipper2 inward `InflatePaths(paths, -delta, JoinType.Miter, EndType.Polygon)` collapse test (an offset vanishing at half the minimum clear width is under-width — the medial axis without a hand-rolled skeleton), and door adjacency off the opening edges.
- Packages: NetTopologySuite (`Polygon`/`MultiPolygon`, `Geometry.Buffer`/`Area`/`Intersection` — the isovist and occupant-area folds), Clipper2 (`InflatePaths`/`Union` — the corridor-clearance offset), Rasm (project — the decoded `SliceStack` story-contour wire), Rasm.Element.
- Growth: a new planar check (exit-signage isovist, refuge-area fold) is one fold over the same boundaries; zero new surface.
- Boundary: NTS/Clipper2 are float production-plane tools at the discipline boundary — no predicate decisions ride them, the kernel staying the exact-geometry owner, never a second exact rail; the floor-plate ingress is the decoded kernel slice-stack wire (`Rasm/Meshing/slice` `Slicing.Apply` story contours through `LayerPlan.AtElevations`, outer-CCW/holes-CW), Compute decoding and never re-slicing; boundary resolution is the one `GeometrySource` port, a circulation-local decode path the deleted form.

```csharp signature
// Resolved planar space: seam node, boundary polygon (GeometrySource-resolved or slice-stack-decoded), derived occupant area — the planar half the graph folds consume.
public sealed record SpaceBoundary(NodeId Space, NetTopologySuite.Geometries.Polygon Boundary, OccupancyClass Occupancy) {
    public double AreaM2 => Boundary.Area;
}

public static class EgressView {
    public static Fin<EgressGraph> Of(ElementGraph graph, AssessmentRequest.Circulation request, GeometrySource geometry) { /* the
        boundary resolution (GeometrySource by content key; a miss is the typed (Admission, Input) failure),
        the occupant-area fold, the door-adjacency edge build, and the corridor-clearance offset test */ }

    // Corridor clearance as offset collapse: the inward Clipper2 inflate at -MinimumClearWidth/2 vanishing marks under-width — the medial axis without a skeleton.
    public static bool UnderWidth(NetTopologySuite.Geometries.Polygon corridor, double minimumClearWidthM) { /* InflatePaths(-w/2) emptiness */ }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

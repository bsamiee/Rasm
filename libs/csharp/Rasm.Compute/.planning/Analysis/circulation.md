# [COMPUTE_CIRCULATION]

Rasm.Compute egress/life-safety runner: the `Discipline.Circulation` arm of the `Analysis/assessment` spine (the seam row landed with this discipline — IBC Ch.10 / EN route rows on the one `AssessmentRoute` axis). It reads the concrete `Rasm.Element` `ElementGraph` directly — the space nodes, the door/opening adjacency, and the exit targets arrive off the graph, never a second graph store — builds the space-adjacency VIEW per request as a `QuikGraph` `AdjacencyGraph` (the admitted substrate's path/topology algebra: `ShortestPathsDijkstra`/`ShortestPathsAStar` travel distance, the strongly-connected reachability census), solves exit CAPACITY as a push-relabel max-flow/min-cut over the same adjacency through the admitted `Google.OrTools.Graph` `MaxFlow` (occupant-load supplies on space nodes, door/corridor widths as arc capacities, `Solve(source, sink)` the evacuation throughput, the saturated `Flow == Capacity` arcs naming the min-cut bottleneck) with `MinCostFlow` distributing occupant load at least travel cost, and folds the planar side — isovist/visibility polygons, corridor medial-axis clearance, occupant areas — through the centrally-pinned `NetTopologySuite`/`Clipper2` float production-plane tools at the discipline boundary (the kernel stays the exact-geometry owner; these are never a second exact rail). Ingress is HONEST: targets, space nodes, and adjacency arrive off the concrete `ElementGraph`; planar space boundaries resolve through the SAME `GeometrySource` content-key port every geometry-reading runner threads (never a second decode path), the floor-plate upgrade consumes the DECODED kernel slice-stack wire (`Rasm/Meshing/slice` `Slicing.Apply` story contours through `LayerPlan.AtElevations`, oriented outer-CCW/holes-CW — Compute decodes, never re-slices); a space whose boundary cannot resolve is a typed `AnalysisFailed(SolvePhase.Admission, FailureKind.Input, …)`, never a silently-skipped node. The occupant-load factors are POLICY ROWS (IBC Table 1004.5 / EN occupancy classes), the allowable travel distances policy data on the request, and every result lands as the ONE uniform `AssessmentResult` fact stream the spine writes back — dead-end and common-path checks included. ZERO new central pins: OrTools/QuikGraph/NTS/Clipper2 are admitted substrate; the folder csproj rows land with this first compose.

## [01]-[INDEX]

- [02]-[EGRESS_GRAPH]: the space-adjacency view off the concrete graph; Dijkstra/A* travel distance; max-flow/min-cut exit capacity; occupant-load distribution; dead-end/common-path checks.
- [03]-[PLANAR_SIDE]: isovist/visibility polygons, corridor medial-axis clearance, and occupant areas over NTS/Clipper2; the slice-stack floor-plate ingress.

## [02]-[EGRESS_GRAPH]

- Owner: `EgressGraph` the per-request space-adjacency VIEW — space nodes (`NodeId`-keyed), door/corridor edges carrying clear width and length, exit nodes marked — built once from the concrete `ElementGraph` and DISCARDED with the run (a persistent second graph store beside the seam graph is the deleted form); `OccupancyClass` `[SmartEnum<string>]` the occupant-load-factor policy rows (assembly · business · educational · mercantile · residential · storage — each row carrying its m²-per-occupant factor, the IBC Table 1004.5 / EN occupancy vocabulary as data); `EgressPolicy` the request policy record (allowable travel distance, allowable dead-end/common-path lengths, minimum clear width, per-occupant capacity rate); `CirculationAnalysis` the runner fold; `EgressFinding` the per-space typed finding the fact stream projects.
- Cases: the three egress checks ride three folds over ONE adjacency view — TRAVEL DISTANCE (`ShortestPathsDijkstra` from every space to its nearest exit, `ShortestPathsAStar` with the spatial-envelope heuristic on large plates, worst path against `EgressPolicy.AllowableTravel`), EXIT CAPACITY (the `MaxFlow` push-relabel over a super-source fanning the occupant-load supplies and a super-sink draining the exits, arc capacities the door clear-width × per-occupant capacity rate; a `MaxFlow.Status` other than `OPTIMAL` is the typed solve verdict — `POSSIBLE_OVERFLOW`/`BAD_INPUT`/`BAD_RESULT` each a typed `(Solve, Numeric)` failure naming the status, never a silent zero; the saturated arcs name the min-cut bottleneck the facts reference), and DEAD-END/COMMON-PATH (the path-algebra folds over the same Dijkstra accessors — a space whose ONLY exit path exceeds the dead-end allowance, and the shared prefix of the two shortest disjoint exit paths against the common-path allowance).
- Entry: `public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Circulation request, GeometrySource geometry, ClockPolicy clocks)` — builds the view, runs the three folds, distributes occupant load at least travel cost through `MinCostFlow` (`SetNodeSupply` the space loads, `AddArcWithCapacityAndUnitCost` the width-capacity/length-cost arcs, `SolveMaxFlowWithMinCost` the assignment), and mints the uniform fact stream: per-space `travel-distance`, `exit-capacity` (throughput vs demand), `min-cut` bottleneck references, `dead-end`/`common-path` lengths, and the governing ratio (the worst of travel-distance/allowable and occupant-demand/exit-capacity); a space with no exit path is `AnalysisFailed(SolvePhase.Solve, FailureKind.Input, "<egress-unreachable:…>")` — a life-safety fact, typed and cached, never an unreachable node dropped from the fold.
- Receipt: the run rides the one `ComputeReceipt.Assessment` case the spine mints — no circulation-local receipt; the min-cut bottleneck and the achieved throughput land as facts, auditable off the baked node.
- Packages: QuikGraph (the `AdjacencyGraph<TVertex, TEdge>` container, `ShortestPathsDijkstra`/`ShortestPathsAStar` → `TryFunc<TVertex, IEnumerable<TEdge>>` path accessors, the SCC reachability census — the admitted substrate's FIRST Compute consumer), Google.OrTools (the `Google.OrTools.Graph` `MaxFlow`/`MinCostFlow` native engines — each `IDisposable` over a native graph, `int` node/arc indices with `long` capacities/costs; the CP-SAT/MILP lanes stay `Solver/optimizer`'s), Rasm.Element (project — `ElementGraph`, `NodeId`, the space/opening nodes and adjacency edges), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new occupancy class is one `OccupancyClass` row carrying its load factor; a new egress check is one fold over the SAME adjacency view plus its fact rows; a new code edition is the route row's `SolverVersion` bump on the `Analysis/assessment` axis; zero new surface — a `TravelDistanceAnalyzer`/`ExitCapacityAnalyzer` sibling family is the collapsed defect, and a managed Edmonds-Karp minted beside the OrTools push-relabel is the rejected reinvention.
- Boundary: the adjacency view derives from the CONCRETE graph per request and dies with the run — space nodes, door edges, and exits are seam facts, never re-modeled; the flow engines are the admitted `Google.OrTools.Graph` natives (capacities as `long` — clear widths quantize to millimetres so the integer capacities are exact) and the min-cut reads off the saturated arcs (`Flow(arc) == Capacity`), never a second cut algorithm; the occupant load is POLICY ROW data (`OccupancyClass` factor × the planar occupant area from `[03]`), never a hardcoded per-space count; the path algebra is QuikGraph's (`TryFunc` accessors resolved per space — a false try is the unreachable-space typed failure), never a hand-rolled Dijkstra; the checks fold EVERY requested space — a boundary that cannot resolve, an unreachable space, and an over-capacity exit are all TYPED outcomes on the one fact stream, never silent skips; the run composes the spine's `AnalysisFailed` lifecycle — a deterministic egress failure caches as a Failed node under the lifecycle-spine law.

```csharp signature
// --- [TYPES] -------------------------------------------------------------------------------
// IBC Table 1004.5 / EN occupancy classes as POLICY ROWS: the m²-per-occupant factor is row data the
// occupant-load derivation reads — a new class is one row, never a hardcoded per-space count.
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

// The request policy: allowable travel distance / dead-end / common-path lengths (route-row code values the
// caller may tighten), the minimum clear width, and the per-metre-of-width occupant capacity rate.
public sealed record EgressPolicy(
    double AllowableTravelM, double AllowableDeadEndM, double AllowableCommonPathM,
    double MinimumClearWidthM, double CapacityPerMetreWidth) {
    public static readonly EgressPolicy Ibc = new(AllowableTravelM: 76.0, AllowableDeadEndM: 6.1, AllowableCommonPathM: 23.0, MinimumClearWidthM: 0.813, CapacityPerMetreWidth: 197.0);
}

// --- [MODELS] ------------------------------------------------------------------------------
// The per-request space-adjacency VIEW: seam NodeId spaces, door/corridor edges with clear width and length,
// exits marked — derived from the concrete ElementGraph, discarded with the run.
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

// The per-space typed finding the fact stream projects: worst travel path, nearest exit, dead-end and
// common-path lengths — one shape for every space, never per-check result records.
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

    // TRAVEL: one Dijkstra accessor per exit-rooted reverse view (edge weights the corridor lengths); the
    // worst-space path, the dead-end fold (a space whose ONLY path exceeds the allowance), and the
    // common-path fold (the shared prefix of the two shortest edge-disjoint exit paths) ride the same
    // TryFunc accessors. An unreachable space is the typed life-safety failure, never a dropped node.
    static Fin<Seq<EgressFinding>> Travel(EgressGraph view, EgressPolicy policy) {
        var paths = view.Adjacency.ShortestPathsDijkstra(static edge => edge.LengthM, view.Exits.Head.IfNone(() => default));
        return view.Spaces.Traverse(space =>
            paths(space.Space, out System.Collections.Generic.IEnumerable<EgressEdge>? path)
                ? Fin.Succ(new EgressFinding(space.Space, toSeq(path).Sum(static e => e.LengthM), view.Exits.Head, DeadEnd(view, space.Space), CommonPath(view, space.Space)))
                : Fin.Fail<EgressFinding>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Input, $"<egress-unreachable:{space.Space.Value}>")));
    }

    // CAPACITY: super-source -> space supplies (occupant loads) -> width-capacitated door arcs -> exits ->
    // super-sink; widths quantize to millimetres so long capacities are exact. A non-OPTIMAL status is the
    // typed solve verdict naming the row; the saturated arcs name the min-cut bottleneck.
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

- Owner: `EgressView` the ingress projection building the `EgressGraph` view — space boundaries resolved as `Polygon`/`MultiPolygon` through the `GeometrySource` port (or decoded off the kernel slice-stack story contours), occupant areas the NTS polygon areas, corridor clearance the Clipper2 inward offset, isovist/visibility polygons the planar sight fold; `SpaceBoundary` the resolved planar carrier.
- Entry: `public static Fin<EgressGraph> Of(ElementGraph graph, AssessmentRequest.Circulation request, GeometrySource geometry)` — resolves each requested space's boundary by content key through the SAME `GeometrySource` port every geometry-reading runner threads (an unresolvable boundary is `AnalysisFailed(SolvePhase.Admission, FailureKind.Input, "<space-boundary-unresolved:…>")`, never a skipped node), derives occupant areas off the NTS `Polygon.Area`, corridor clear widths off the Clipper2 inward `InflatePaths(paths, -delta, JoinType.Miter, EndType.Polygon)` collapse test (a corridor whose inward offset at half the minimum clear width vanishes is under-width — the medial-axis clearance read as an offset collapse, never a hand-rolled skeleton), and door adjacency off the graph's opening edges.
- Packages: NetTopologySuite (the `Polygon`/`MultiPolygon` boundaries, `Geometry.Buffer`/`Area`/`Intersection` — the isovist/visibility and occupant-area folds), Clipper2 (`InflatePaths`/`Union` — the corridor-clearance offset algebra), Rasm (project — the decoded `SliceStack` story-contour wire), Rasm.Element (project).
- Growth: a new planar check (a visibility-of-exit-signage isovist row, a refuge-area fold) is one fold over the same resolved boundaries; zero new surface.
- Boundary: NTS/Clipper2 are FLOAT production-plane tools at the discipline boundary — the kernel stays the exact-geometry owner and these never become a second exact rail (no predicate decisions ride them; areas, offsets, and visibility are tolerance-honest planar measures); the floor-plate ingress is the DECODED kernel slice-stack wire — `Rasm/Meshing/slice` `Slicing.Apply` emits the story contours through `LayerPlan.AtElevations` with outer-CCW/holes-CW orientation and typed OPEN-chain rows, Compute decodes and NEVER re-slices (the kernel ledger names this runner the consumer); the boundary resolution is the ONE `GeometrySource` port — a circulation-local decode path beside it is the deleted form.

```csharp signature
// The resolved planar space: the seam space node, its boundary polygon (GeometrySource-resolved or
// slice-stack-decoded), and its derived occupant area — the planar half the graph folds consume.
public sealed record SpaceBoundary(NodeId Space, NetTopologySuite.Geometries.Polygon Boundary, OccupancyClass Occupancy) {
    public double AreaM2 => Boundary.Area;
}

public static class EgressView {
    public static Fin<EgressGraph> Of(ElementGraph graph, AssessmentRequest.Circulation request, GeometrySource geometry) { /* the
        boundary resolution (GeometrySource by content key; a miss is the typed (Admission, Input) failure),
        the occupant-area fold, the door-adjacency edge build, and the corridor-clearance offset test */ }

    // Corridor clearance as offset collapse: the inward Clipper2 inflate at -MinimumClearWidth/2 vanishing
    // marks an under-width corridor — the medial-axis read without a hand-rolled skeleton.
    public static bool UnderWidth(NetTopologySuite.Geometries.Polygon corridor, double minimumClearWidthM) { /* InflatePaths(-w/2) emptiness */ }
}
```

## [04]-[RESEARCH]

- [SEEDED_GROWTH]: MEP network-flow (the Todini global-gradient algebra over the admitted sparse solver + QuikGraph — `Discipline.Water`'s runner when chartered) and geometric room-acoustics (image-source over the clash BVH) ride the same one-row-one-route-one-runner growth law this page proves; the SCC reachability census (`StronglyConnectedComponents`) is the wayfinding topology deepening the adjacency view takes when a multi-storey route model lands.

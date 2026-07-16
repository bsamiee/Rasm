# [COMPUTE_CIRCULATION]

Rasm.Compute egress/life-safety runner owns the `Discipline.Circulation` assessment arm. Space, door, occupancy, and exit evidence read from the concrete `ElementGraph`; a request builds one discarded QuikGraph adjacency view. Exit-rooted Dijkstra paths govern travel, dead-end, common-path, and RSET; OR-Tools `MaxFlow` governs throughput and exposes saturated-capacity bottlenecks; `MinCostFlow.SolveMaxFlowWithMinCost` routes the feasible occupant flow at least travel cost.

Ingress is honest: targets, spaces, and adjacency arrive off the concrete graph, and planar space boundaries resolve through the same `GeometrySource` content-key port every geometry-reading runner threads (the floor-plate upgrade decoding the kernel slice-stack wire — `Rasm/Meshing/slice` `Slicing.Apply` story contours through `LayerPlan.AtElevations`, outer-CCW/holes-CW — Compute decodes, never re-slices). Isovist/visibility polygons, corridor medial-axis clearance, and occupant areas fold through the centrally-pinned `NetTopologySuite`/`Clipper2` float production-plane tools at the discipline boundary, the kernel staying the exact-geometry owner. Occupant-load factors are policy rows (IBC Table 1004.5 / EN occupancy classes), allowable travel distances policy data on the request; a space whose boundary cannot resolve rails `AnalysisFailed(SolvePhase.Admission, FailureKind.Input, …)`, never a silent skip. Zero new central pins — OrTools/QuikGraph/NTS/Clipper2 are admitted substrate, the folder csproj rows landing with this first compose.

## [01]-[INDEX]

- [01]-[EGRESS_GRAPH]: the space-adjacency view, Dijkstra travel, max-flow capacity, feasible min-cost distribution, saturated-capacity bottlenecks, route-tail checks, and RSET.
- [02]-[PLANAR_SIDE]: isovist/visibility polygons, corridor medial-axis clearance, and occupant areas over NTS/Clipper2; the slice-stack floor-plate ingress.

## [02]-[EGRESS_GRAPH]

- Owner: `EgressGraph` the per-request space-adjacency view (space nodes `NodeId`-keyed, door/corridor edges with clear width and length, exits marked), built once from the concrete `ElementGraph` and discarded with the run — a persistent second graph store is the deleted form; `OccupancyClass` `[SmartEnum<string>]` the occupant-load-factor policy rows (IBC Table 1004.5 / EN vocabulary as data); `EgressPolicy` the request policy record; `CirculationAnalysis` the runner fold; `EgressFinding` the per-space typed finding.
- Cases: one exit-rooted path family drives travel, common path, dead end, and RSET; `MaxFlow` derives throughput; `SolveMaxFlowWithMinCost` prices the routable occupant flow without turning an over-capacity design into a solver fault; saturated adjacency arcs identify capacity bottlenecks without claiming an uncomputed cut partition.
- Entry: `Run(graph, request, geometry, clocks)` emits travel, dead-end, common-path, RSET, throughput, feasible distribution cost, width, and saturated-capacity facts; governing includes every acceptance check.
- Receipt: the shared assessment receipt carries achieved throughput; saturated arcs land as ONE `saturated-capacity-bottlenecks` `List` fact of typed `Reference` values (per-arc facts under a repeated name would overwrite in the write-back `Results` bag); no circulation-local receipt exists.
- Packages: QuikGraph (`AdjacencyGraph<TVertex, TEdge>`, `ShortestPathsDijkstra`/`ShortestPathsAStar` → `TryFunc<TVertex, IEnumerable<TEdge>>` accessors, the SCC census — its first Compute consumer), Google.OrTools (`Google.OrTools.Graph` `MaxFlow`/`MinCostFlow` natives — each `IDisposable`, `int` node/arc indices, `long` capacities/costs; CP-SAT/MILP stay `Solver/optimizer`'s), Rasm.Element (`ElementGraph`, `NodeId`), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new occupancy class is one `OccupancyClass` row; a new egress check is one fold over the same view; a new code edition is the route row's `SolverVersion` bump; zero new surface — a `TravelDistanceAnalyzer`/`ExitCapacityAnalyzer` sibling family the collapsed defect, a managed Edmonds-Karp beside the OrTools push-relabel the rejected reinvention. Seeded runners land one row/route/runner when chartered: MEP network-flow (`Discipline.Water`, the Todini global-gradient algebra over the sparse solver + QuikGraph), geometric room-acoustics (image-source over the clash BVH), and the `StronglyConnectedComponents` wayfinding deepening for multi-storey routes.
- Boundary: flow capacities and costs quantize to `long`; saturated arcs are bottleneck candidates, not a min-cut partition. QuikGraph owns paths, occupancy is mandatory request evidence, and door width/geometry must resolve before either graph algorithm runs.

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

// Request policy: allowable travel/dead-end/common-path lengths (route-row values the caller may tighten), minimum clear
// width, per-metre capacity rate, plus the hydraulic-model columns the RSET fold reads — unimpeded walking speed and the
// SFPE specific door flow — and the optional performance-based RSET acceptance (0 = informational, the no-target convention).
[ComplexValueObject]
public sealed partial class EgressPolicy {
    public double AllowableTravelM { get; }
    public double AllowableDeadEndM { get; }
    public double AllowableCommonPathM { get; }
    public double MinimumClearWidthM { get; }
    public double CapacityPerMetreWidth { get; }
    public double UnimpededSpeedMPerS { get; }
    public double SpecificFlowPersonsPerMS { get; }
    public double AllowableRsetMinutes { get; }

    public static readonly EgressPolicy Ibc = Create(76.0, 6.1, 23.0, 0.813, 197.0, 1.2, 1.3, 0.0);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double allowableTravelM, ref double allowableDeadEndM,
        ref double allowableCommonPathM, ref double minimumClearWidthM, ref double capacityPerMetreWidth,
        ref double unimpededSpeedMPerS, ref double specificFlowPersonsPerMS, ref double allowableRsetMinutes) =>
        validationError = new[] { allowableTravelM, allowableDeadEndM, allowableCommonPathM, minimumClearWidthM, capacityPerMetreWidth, unimpededSpeedMPerS, specificFlowPersonsPerMS }
            .All(static value => double.IsFinite(value) && value > 0.0) && double.IsFinite(allowableRsetMinutes) && allowableRsetMinutes >= 0.0
                ? null
                : new ValidationError(message: "<egress-policy-invalid>");
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
    Seq<NodeId> Exits,
    Seq<SpaceBoundary> Boundaries) {
    public int OccupantLoad((NodeId Space, double AreaM2, OccupancyClass Occupancy) space) =>
        (int)Math.Ceiling(space.AreaM2 / space.Occupancy.AreaPerOccupantM2);
}

// Per-space typed finding the fact stream projects: worst travel path, nearest exit, dead-end/common-path lengths, and
// Hydraulic RSET combines travel and door-queue time in the same shape, never per-check records.
public readonly record struct EgressFinding(NodeId Space, double TravelM, NodeId NearestExit, double DeadEndM, double CommonPathM, double RsetMinutes);

// --- [OPERATIONS] --------------------------------------------------------------------------
public static class CirculationAnalysis {
    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Circulation request, GeometrySource geometry, ClockPolicy clocks) =>
        from view in EgressView.Of(graph, request, geometry)
        from findings in Travel(view, request.Policy)
        from capacity in Capacity(view, request.Policy)
        from distribution in Distribute(view, request.Policy)
        let govern = Governing(findings, capacity, view.Boundaries.Exists(b => EgressView.UnderWidth(b.Boundary, request.Policy.MinimumClearWidthM)), request.Policy)
        from travel in findings.TraverseM(f => AssessmentFact.Ratio($"{f.Space.Value}/travel-distance", f.TravelM / request.Policy.AllowableTravelM)).As()
        from deadEnds in findings.Filter(static f => f.DeadEndM > 0.0).TraverseM(f => AssessmentFact.Ratio($"{f.Space.Value}/dead-end", f.DeadEndM / request.Policy.AllowableDeadEndM)).As()
        from commonPaths in findings.Filter(static f => f.CommonPathM > 0.0).TraverseM(f => AssessmentFact.Ratio($"{f.Space.Value}/common-path", f.CommonPathM / request.Policy.AllowableCommonPathM)).As()
        from rset in findings.TraverseM(f => AssessmentFact.Measure($"{f.Space.Value}/rset", Dimension.DurationDim, f.RsetMinutes * 60.0)).As()
        let nearestExits = findings.Map(static finding => AssessmentFact.Reference($"{finding.Space.Value}/nearest-exit", finding.NearestExit))
        let underWidth = view.Boundaries.Filter(b => EgressView.UnderWidth(b.Boundary, request.Policy.MinimumClearWidthM))
            .Map(b => AssessmentFact.Flag($"{b.Space.Value}/under-width", true))
        from exits in AssessmentFact.Rows(
            AssessmentFact.Ratio("exit-capacity", capacity.DemandOccupants / Math.Max(1.0, capacity.ThroughputOccupants)),
            AssessmentFact.Measure("evacuation-throughput", Dimension.Dimensionless, capacity.ThroughputOccupants),
            AssessmentFact.Measure("occupant-distribution-cost", Dimension.Dimensionless, distribution))
        // Saturated arcs collapse into ONE List fact of typed References: per-arc facts under a repeated name would
        // overwrite each other in the write-back Results bag (AddOrUpdate keyed on PropertyName), surviving as one arbitrary arc.
        let bottlenecks = capacity.Bottleneck.IsEmpty
            ? Seq<AssessmentFact>()
            : Seq(AssessmentFact.List("saturated-capacity-bottlenecks",
                capacity.Bottleneck.Map(static edge => (PropertyValue)new PropertyValue.Reference(edge.From))))
        select AssessmentResult.Of(
            request.Route,
            travel + deadEnds + commonPaths + rset + nearestExits + underWidth + exits + bottlenecks,
            govern,
            new Provenance("CirculationAnalysis", request.Route.Standard, request.Route.SolverVersion, clocks.Now));

    // TRAVEL: one Dijkstra accessor PER EXIT root (the view's edges run both directions, so an exit-rooted tree reads
    // space->exit distance); per space the nearest exit wins, the dead-end and common-path folds ride the SAME per-exit
    // paths, and the hydraulic RSET adds the door-queue time — an exit-unreachable space is the typed life-safety failure.
    static Fin<Seq<EgressFinding>> Travel(EgressGraph view, EgressPolicy policy) {
        Seq<(NodeId Exit, TryFunc<NodeId, System.Collections.Generic.IEnumerable<EgressEdge>> Paths)> rooted =
            view.Exits.Map(exit => (exit, view.Adjacency.ShortestPathsDijkstra(static edge => edge.LengthM, exit)));
        return view.Spaces.TraverseM(space => {
            Seq<(NodeId Exit, Seq<EgressEdge> Path, double LengthM)> routes = rooted
                .Choose(root => root.Paths(space.Space, out System.Collections.Generic.IEnumerable<EgressEdge> path) ? Some((root.Exit, toSeq(path), toSeq(path).Sum(static e => e.LengthM))) : None)
                .OrderBy(static r => r.LengthM).ToSeq();
            return routes.IsEmpty
                ? Fin.Fail<EgressFinding>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Input, $"<egress-unreachable:{space.Space.Value}>"))
                : Finding(view, space, routes, policy);
        }).As();
    }

    // Per-space fold derives nearest travel and the two best routes' shared space-side common path. Exit-rooted paths
    // reach the space last, so their shared tail is the occupant's shared first leg;
    // dead-end the whole route when ONE exit serves the space (a single escape direction); RSET the SFPE hydraulic
    // t_travel + t_queue over the nearest route's terminal door width.
    static Fin<EgressFinding> Finding(EgressGraph view, (NodeId Space, double AreaM2, OccupancyClass Occupancy) space, Seq<(NodeId Exit, Seq<EgressEdge> Path, double LengthM)> routes, EgressPolicy policy) {
        (NodeId Exit, Seq<EgressEdge> Path, double LengthM) nearest = routes[0];
        double common = routes.Count >= 2 ? SharedTail(nearest.Path, routes[1].Path) : nearest.LengthM;
        double deadEnd = routes.Count >= 2 ? 0.0 : nearest.LengthM;
        int occupants = view.OccupantLoad(space);
        return nearest.Path.Head
            .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Extraction, FailureKind.Input, $"<egress-route-empty:{space.Space.Value}>"))
            .Map(door => new EgressFinding(space.Space, nearest.LengthM, nearest.Exit, deadEnd, common,
                (nearest.LengthM / policy.UnimpededSpeedMPerS + occupants / (door.ClearWidthM * policy.SpecificFlowPersonsPerMS)) / 60.0));
    }

    // Shared space-side run of two exit-rooted paths folds reversed edge sequences until they diverge;
    // summed length is the common path of egress travel the two-route availability check bounds.
    static double SharedTail(Seq<EgressEdge> a, Seq<EgressEdge> b) =>
        a.Rev().Zip(b.Rev()).TakeWhile(static pair => pair.Item1.From == pair.Item2.From && pair.Item1.To == pair.Item2.To)
            .Sum(static pair => pair.Item1.LengthM);

    // CAPACITY: super-source -> space supplies -> width-capacitated door arcs -> exits -> super-sink; a capacity is an
    // occupant COUNT (width x per-metre rate, rounded once) so the long arcs are exact integers, a non-OPTIMAL status the
    // typed solve verdict; saturated adjacency arcs are capacity bottlenecks, never mislabeled as a cut partition.
    // Exemption: the OrTools arc-building loop is the native-solver marshaling statement seam.
    static Fin<(double DemandOccupants, double ThroughputOccupants, Seq<EgressEdge> Bottleneck)> Capacity(EgressGraph view, EgressPolicy policy) {
        using Google.OrTools.Graph.MaxFlow flow = new();
        Seq<NodeId> index = (view.Spaces.Map(static s => s.Space) + view.Exits).Distinct().ToSeq();
        int NodeOf(NodeId id) => index.FindIndex(v => v == id) + 2;
        foreach ((NodeId Space, double AreaM2, OccupancyClass Occupancy) space in view.Spaces) { flow.AddArcWithCapacity(0, NodeOf(space.Space), view.OccupantLoad(space)); }
        Seq<(int Arc, EgressEdge Edge)> arcs = view.Adjacency.Edges.ToSeq().Map(edge =>
            (flow.AddArcWithCapacity(NodeOf(edge.From), NodeOf(edge.To), (long)Math.Round(edge.ClearWidthM * policy.CapacityPerMetreWidth)), edge));
        foreach (NodeId exit in view.Exits) { flow.AddArcWithCapacity(NodeOf(exit), 1, long.MaxValue / 4); }
        return flow.Solve(0, 1) switch {
            Google.OrTools.Graph.MaxFlow.Status.OPTIMAL => Fin.Succ((
                (double)view.Spaces.Sum(view.OccupantLoad),
                (double)flow.OptimalFlow(),
                arcs.Filter(pair => flow.Flow(pair.Arc) == flow.Capacity(pair.Arc)).Map(static pair => pair.Edge))),
            Google.OrTools.Graph.MaxFlow.Status status => Fin.Fail<(double, double, Seq<EgressEdge>)>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, $"<egress-flow:{status}>")),
        };
    }

    // DISTRIBUTION: occupant load routed to exits at least travel cost through MinCostFlow — supplies at spaces, one
    // drain per exit, arc costs the corridor lengths quantized to millimetres so the long costs are exact; the optimal
    // cost (occupant-millimetres, reported as occupant-metres) is the wayfinding-efficiency evidence a layout variant
    // screens on. A non-OPTIMAL status is the typed solve verdict.
    // Exemption: the OrTools arc-building loop is the native-solver marshaling statement seam.
    static Fin<double> Distribute(EgressGraph view, EgressPolicy policy) {
        using Google.OrTools.Graph.MinCostFlow flow = new();
        Seq<NodeId> index = (view.Spaces.Map(static s => s.Space) + view.Exits).Distinct().ToSeq();
        int NodeOf(NodeId id) => index.FindIndex(v => v == id) + 1;
        long total = view.Spaces.Sum(view.OccupantLoad);
        foreach ((NodeId Space, double AreaM2, OccupancyClass Occupancy) space in view.Spaces) { flow.SetNodeSupply(NodeOf(space.Space), view.OccupantLoad(space)); }
        foreach (EgressEdge edge in view.Adjacency.Edges) {
            flow.AddArcWithCapacityAndUnitCost(NodeOf(edge.From), NodeOf(edge.To),
                (long)Math.Round(edge.ClearWidthM * policy.CapacityPerMetreWidth), (long)Math.Round(edge.LengthM * 1000.0));
        }
        foreach (NodeId exit in view.Exits) { flow.AddArcWithCapacityAndUnitCost(NodeOf(exit), 0, long.MaxValue / 4, 0); }
        flow.SetNodeSupply(0, -total);
        return flow.SolveMaxFlowWithMinCost() switch {
            Google.OrTools.Graph.MinCostFlow.Status.OPTIMAL => Fin.Succ(flow.OptimalCost() / 1000.0),
            Google.OrTools.Graph.MinCostFlow.Status status => Fin.Fail<double>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, $"<egress-distribution:{status}>")),
        };
    }

    // Governing: worst travel ratio, capacity demand/throughput, and — when the policy states a performance target —
    // Worst RSET compares against the target; informational zero-target RSET contributes nothing.
    static double Governing(Seq<EgressFinding> findings, (double Demand, double Throughput, Seq<EgressEdge> Bottleneck) capacity, bool underWidth, EgressPolicy policy) =>
        Math.Max(
            Math.Max(
                findings.Map(f => Math.Max(f.TravelM / policy.AllowableTravelM,
                    Math.Max(f.DeadEndM / policy.AllowableDeadEndM, f.CommonPathM / policy.AllowableCommonPathM))).Max() | 0.0,
                policy.AllowableRsetMinutes > 0.0 ? (findings.Map(f => f.RsetMinutes / policy.AllowableRsetMinutes).Max() | 0.0) : 0.0),
            Math.Max(capacity.Demand / Math.Max(1.0, capacity.Throughput), underWidth ? Math.BitIncrement(1.0) : 0.0));
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
    const string SpaceBoundaryWire = "IfcRelSpaceBoundary";
    static readonly PropertyName HostAttr = PropertyName.Create("Host");
    static readonly PropertyName WidthQuantity = PropertyName.Create("Width");
    static readonly NetTopologySuite.Geometries.GeometryFactory Planar = new();

    // Ingress projects (1) boundaries by resolving each target footprint through
    // GeometrySource port into an NTS polygon (a miss is the typed (Admission, Input) failure — a life-safety input,
    // never a skip), the occupant area its Polygon.Area, and occupancy required from the request map;
    // (2) DOORS — every Host-attributed space-boundary edge names a door node, two spaces sharing one door adjoin
    // through BOTH-direction EgressEdges (clear width from the door Qto; length through the door footprint centroid),
    // so an exit-rooted Dijkstra reads space-to-exit distance off the same view; a door
    // bounding exactly ONE space discharges to the exterior and IS an exit node; (3) CONNECTIVITY CENSUS — the
    // StronglyConnectedComponents sweep rails a no-exit component as one typed (Admission, Input) failure naming a
    // member space, before any per-space fold runs; the retained Boundaries carry the resolved polygons the Run-side
    // Clipper2 under-width probe folds into per-space flags.
    public static Fin<EgressGraph> Of(ElementGraph graph, AssessmentRequest.Circulation request, GeometrySource geometry) =>
        request.Targets
            .TraverseM(id => graph.Find<Node.Object>(id)
                .Bind(o => geometry.Footprint(o.Representations))
                .Filter(static f => !f.IsEmpty)
                .Map(f => (Space: id, Boundary: Polygon(f)))
                .Filter(static row => row.Boundary.IsValid && row.Boundary.Area > 0.0)
                .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<space-boundary-unresolved:{id.Value}>")))
            .As()
            .Bind(resolved => resolved.TraverseM(row => request.Occupancies.Find(row.Space)
                .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<space-occupancy-unresolved:{row.Space.Value}>"))
                .Map(occupancy => (row.Space, row.Boundary, Occupancy: occupancy))).As()
            .Bind(admitted => PlanarAdmission(admitted).Bind(_ => {
                Seq<(NodeId Space, double AreaM2, OccupancyClass Occupancy)> spaces = admitted.Map(static row =>
                    (row.Space, row.Boundary.Area, row.Occupancy));
                Seq<(NodeId Space, NodeId Door)> doorBindings = resolved.Bind(r =>
                    graph.EdgesAt(r.Space).Choose(e => e is Relationship.Generic g && g.WireName == SpaceBoundaryWire && g.Relating == r.Space && g.Attributes.Find(HostAttr).IsSome
                        ? Some((r.Space, g.Related)) : None).ToSeq());
                Seq<NodeId> exits = doorBindings.GroupBy(static b => b.Door).Filter(static g => g.Count() == 1).Map(static g => g.Key).ToSeq();
                Seq<SpaceBoundary> boundaries = admitted.Map(static row => new SpaceBoundary(row.Space, row.Boundary, row.Occupancy));
                return DoorEdges(graph, geometry, resolved, doorBindings)
                    .Bind(edges => {
                        QuikGraph.AdjacencyGraph<NodeId, EgressEdge> adjacency = new();
                        adjacency.AddVertexRange(spaces.Map(static s => s.Space) + exits);
                        adjacency.AddEdgeRange(edges);
                        return Census(adjacency, spaces, exits).Map(_ => new EgressGraph(adjacency, spaces, exits, boundaries));
                    });
            })));

    // Robust bulk union detects positive-area overlap; `STRtree` narrows the conflicting pair and a prepared boundary
    // amortizes repeated predicates. Touching space boundaries remain legal.
    static Fin<Unit> PlanarAdmission(Seq<(NodeId Space, NetTopologySuite.Geometries.Polygon Boundary, OccupancyClass Occupancy)> admitted) {
        NetTopologySuite.Index.Strtree.STRtree<NodeId> index = new();
        foreach ((NodeId Space, NetTopologySuite.Geometries.Polygon Boundary, OccupancyClass Occupancy) row in admitted) {
            index.Insert(row.Boundary.EnvelopeInternal, row.Space);
        }
        NetTopologySuite.Geometries.Geometry union = NetTopologySuite.Operation.OverlayNG.OverlayNGRobust.Union(
            admitted.Map(static row => (NetTopologySuite.Geometries.Geometry)row.Boundary));
        double overlapArea = admitted.Sum(static row => row.Boundary.Area) - union.Area;
        if (overlapArea <= 1e-9) { return Fin.Succ(unit); }
        Option<NodeId> conflict = admitted.Choose(row => {
            NetTopologySuite.Geometries.Prepared.IPreparedGeometry prepared = NetTopologySuite.Geometries.Prepared.PreparedGeometryFactory.Prepare(row.Boundary);
            return toSeq(index.Query(row.Boundary.EnvelopeInternal))
                .Filter(other => StringComparer.Ordinal.Compare(row.Space.Value, other.Value) < 0)
                .Find(other => admitted.Find(candidate => candidate.Space == other).Exists(candidate =>
                    prepared.Overlaps(candidate.Boundary) || prepared.Contains(candidate.Boundary.Centroid) || prepared.Covers(candidate.Boundary)));
        }).Head;
        return Fin.Fail<Unit>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input,
            $"<egress-space-boundaries-overlap:{conflict.Map(static id => id.Value).IfNone("unresolved")}>"));
    }

    // Two spaces sharing one interior door adjoin both directions; an exit door binds its one space to the exit node.
    // Clear width and the door footprint are required evidence. Interior edges traverse space-centroid → door-centroid →
    // space-centroid; exterior doors add both directions so exit-rooted and occupant-rooted algorithms share one view.
    static Fin<Seq<EgressEdge>> DoorEdges(ElementGraph graph, GeometrySource geometry,
        Seq<(NodeId Space, NetTopologySuite.Geometries.Polygon Boundary)> resolved, Seq<(NodeId Space, NodeId Door)> bindings) =>
        bindings.GroupBy(static binding => binding.Door).ToSeq().TraverseM(door =>
            from width in DoorWidth(graph, door.Key)
                .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<egress-door-width-unresolved:{door.Key.Value}>"))
            from point in graph.Find<Node.Object>(door.Key).Bind(node => geometry.Footprint(node.Representations)).Filter(static footprint => !footprint.IsEmpty)
                .Map(footprint => Polygon(footprint).Centroid)
                .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<egress-door-footprint-unresolved:{door.Key.Value}>"))
            let bound = toSeq(door).Map(static binding => binding.Space)
            from edges in bound.Count >= 2
                ? bound.Bind(a => bound.Filter(b => b != a).Map(b => (From: a, To: b))).TraverseM(pair =>
                    from a in Centroid(resolved, pair.From)
                    from b in Centroid(resolved, pair.To)
                    select new EgressEdge(pair.From, pair.To, width, a.Distance(point) + point.Distance(b))).As()
                : bound.Head.ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<egress-door-unbound:{door.Key.Value}>"))
                    .Bind(space => Centroid(resolved, space).Map(center => {
                        double length = center.Distance(point);
                        return Seq(new EgressEdge(space, door.Key, width, length), new EgressEdge(door.Key, space, width, length));
                    }))
            select edges)
        .As().Map(static rows => rows.Fold(Seq<EgressEdge>(), static (edges, row) => edges + row));

    static Option<double> DoorWidth(ElementGraph graph, NodeId door) =>
        graph.EdgesAt(door).Choose(e => e is Relationship.Assign { SubKind: AssignKind.PropertyDefinition } a && a.Subject == door
                ? graph.Find<Node.QuantitySet>(a.Definition) : None)
            .Choose(q => q.Bag.Values.Find(WidthQuantity))
            .Head.Map(static m => m.Si);

    static Fin<NetTopologySuite.Geometries.Point> Centroid(Seq<(NodeId Space, NetTopologySuite.Geometries.Polygon Boundary)> resolved, NodeId space) =>
        resolved.Find(row => row.Space == space).Map(static row => row.Boundary.Centroid)
            .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<egress-space-centroid-unresolved:{space.Value}>"));

    // SCC connectivity census classifies a space component reaching no exit as a no-egress island — one typed
    // failure naming a member space, railed BEFORE the per-space travel fold spends its per-exit Dijkstra sweeps.
    static Fin<Unit> Census(QuikGraph.AdjacencyGraph<NodeId, EgressEdge> adjacency, Seq<(NodeId Space, double AreaM2, OccupancyClass Occupancy)> spaces, Seq<NodeId> exits) {
        adjacency.StronglyConnectedComponents(out System.Collections.Generic.IDictionary<NodeId, int> components);
        Seq<int> exitComponents = exits.Choose(e => components.TryGetValue(e, out int c) ? Some(c) : None).Distinct();
        return spaces.Find(s => components.TryGetValue(s.Space, out int c) && !exitComponents.Contains(c)).Match(
            Some: s => Fin.Fail<Unit>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<egress-island:{s.Space.Value}>")),
            None: () => Fin.Succ(unit));
    }

    static NetTopologySuite.Geometries.Polygon Polygon(FootprintPolygon footprint) =>
        Planar.CreatePolygon([.. footprint.Ring.Map(static p => new NetTopologySuite.Geometries.Coordinate(p.X, p.Y)), new(footprint.Ring[0].X, footprint.Ring[0].Y)]);

    // Corridor clearance as offset collapse: the inward Clipper2 inflate at -MinimumClearWidth/2 vanishing marks
    // under-width — the medial axis without a hand-rolled skeleton. Ring coordinates lift onto PathsD once; a Miter
    // join keeps re-entrant corners honest where a Round join would smooth a pinch past the test.
    public static bool UnderWidth(NetTopologySuite.Geometries.Polygon corridor, double minimumClearWidthM) {
        Clipper2Lib.PathsD paths = [[.. corridor.ExteriorRing.Coordinates.Select(static c => new Clipper2Lib.PointD(c.X, c.Y))]];
        return Clipper2Lib.Clipper.InflatePaths(paths, -minimumClearWidthM / 2.0, Clipper2Lib.JoinType.Miter, Clipper2Lib.EndType.Polygon).Count == 0;
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

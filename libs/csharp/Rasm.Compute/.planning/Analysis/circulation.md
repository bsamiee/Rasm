# [COMPUTE_CIRCULATION]

Rasm.Compute egress/life-safety runner owns the `Discipline.Circulation` assessment arm. Space, door, occupancy, and exit evidence read from the concrete `ElementGraph`; a request builds one discarded QuikGraph adjacency view. Exit-rooted Dijkstra paths govern travel, dead-end, common-path, and RSET; OR-Tools `MaxFlow` governs throughput and exposes saturated-capacity bottlenecks; `MinCostFlow.SolveMaxFlowWithMinCost` routes the feasible occupant flow at least travel cost.

Ingress is honest: targets, spaces, and adjacency arrive off the concrete graph, and planar space boundaries resolve through the same `GeometrySource` content-key port every geometry-reading runner threads (the floor-plate upgrade decoding the kernel slice-stack wire — `Rasm/Meshing/slice` `Slicing.Apply` story contours through `LayerPlan.AtElevations`, outer-CCW/holes-CW — Compute decodes, never re-slices). Isovist/visibility polygons, corridor medial-axis clearance, and occupant areas fold through the centrally-pinned `NetTopologySuite`/`Clipper2` float production-plane tools at the discipline boundary, the kernel staying the exact-geometry owner. Occupant-load factors are policy rows (IBC Table 1004.5 / EN occupancy classes), allowable travel distances policy data on the request; a space whose boundary cannot resolve rails `AnalysisFailed(SolvePhase.Admission, FailureKind.Input, …)`, never a silent skip. Zero new central pins — OrTools/QuikGraph/NTS/Clipper2 are admitted substrate, the folder csproj rows landing with this first compose.

## [01]-[INDEX]

- [02]-[EGRESS_GRAPH]: `Run` folds travel, throughput, feasible distribution, bottleneck, route-tail, and RSET facts over one space-adjacency view.
- [03]-[PLANAR_SIDE]: NTS/Clipper2 derive isovist polygons, corridor medial-axis clearance, and occupant areas off the slice-stack floor-plate ingress.

## [02]-[EGRESS_GRAPH]

- Owner: `EgressGraph` the per-request space-adjacency view (space nodes `NodeId`-keyed, door/corridor edges with clear width and length, exits marked), built once from the concrete `ElementGraph` and discarded with the run — a persistent second graph store is the deleted form; `OccupancyClass` `[SmartEnum<string>]` the occupant-load-factor policy rows (IBC Table 1004.5 / EN vocabulary as data); `EgressPolicy` the request policy record; `CirculationAnalysis` the runner fold; `EgressFinding` the per-space typed finding.
- Cases: one exit-rooted path family drives travel, common path, dead end, and RSET; `MaxFlow` derives throughput; `SolveMaxFlowWithMinCost` prices the routable occupant flow without turning an over-capacity design into a solver fault; saturated adjacency arcs identify capacity bottlenecks without claiming an uncomputed cut partition.
- Entry: `Run(graph, request, geometry, clock)` emits travel, dead-end, common-path, RSET, throughput, feasible distribution cost, measured clear-width, under-width flag, and saturated-capacity facts; governing folds every MEASURED acceptance ratio, the clear-width one riding the offset-collapse bisection rather than a boolean.
- Receipt: the shared assessment receipt carries achieved throughput; saturated arcs land as ONE `saturated-capacity-bottlenecks` `List` fact of typed `Reference` values (per-arc facts under a repeated name overwrite in the write-back `Results` bag); no circulation-local receipt exists.
- Packages: QuikGraph (`AdjacencyGraph<TVertex, TEdge>`, `ShortestPathsDijkstra` → `TryFunc<TVertex, IEnumerable<TEdge>>` accessors, `StronglyConnectedComponents` filling a caller-supplied `IDictionary<TVertex, int>` and returning the component count — its first Compute consumer; `ShortestPathsAStar` is NOT composed and must not be proposed, because the egress question is every-space-to-nearest-exit and one exit-rooted Dijkstra tree per exit answers it whole, where a heuristic single-pair search would re-run per space-exit pair), Google.OrTools (`Google.OrTools.Graph` `MaxFlow`/`MinCostFlow` natives — each `IDisposable`, `int` node/arc indices, `long` capacities/costs, `MinCostFlow` pre-sizing through its `(nodes, arcs)` ctor and its `Status` declared on the shared `MinCostFlowBase`; CP-SAT/MILP stay `Solver/optimizer`'s), Rasm.Element (`ElementGraph`, `NodeId`, `QuantityRows`/`BoundaryRows` the row vocabulary the door and boundary reads key), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new occupancy class is one `OccupancyClass` row; a new egress check is one fold over the same view; a new code edition is the route row's `SolverVersion` bump; vertical circulation is one `EgressEdge` kind row carrying the SFPE stair/ramp specific-flow columns, which is also what makes a multi-storey merge expressible; zero new surface — a `TravelDistanceAnalyzer`/`ExitCapacityAnalyzer` sibling family the collapsed defect, a managed Edmonds-Karp beside the OrTools push-relabel the rejected reinvention.
- Boundary: flow capacities and costs quantize to `long`; saturated arcs are bottleneck candidates, not a min-cut partition. QuikGraph owns paths, occupancy is mandatory request evidence, and door width/geometry must resolve before either graph algorithm runs. The graph this page assesses is PLANAR and single-storey: `EgressEdge` carries `ClearWidthM`/`LengthM` alone, so every travel, capacity, and RSET fold reads one storey's horizontal circulation and a stair or ramp has no column to declare its specific flow on. Solver node addressing builds ONE keyed index per solve — a linear scan per arc is `O(n²)` over the arc set. Clear width is MEASURED, never flagged into the ratio channel: a boolean lifted to an epsilon above unity is a utilization no probe produced.

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
    const double WidthFloorM = 1e-9;   // divisor floor for a fully collapsed passage

    public static Fin<AssessmentResult> Run(ElementGraph graph, AssessmentRequest.Circulation request, GeometrySource geometry, IClock clock) =>
        from view in EgressView.Of(graph, request, geometry)
        from findings in Travel(view, request.Policy)
        from capacity in Capacity(view, request.Policy)
        from distribution in Distribute(view, request.Policy)
        // Widths measure ONCE per boundary and feed both the governing ratio and the emitted evidence, so the probe
        // never runs twice for one question and the ratio and the fact cannot disagree.
        let widths = view.Boundaries.Map(b => (b.Space, WidthM: EgressView.ClearWidthM(b.Boundary, request.Policy.MinimumClearWidthM)))
        let govern = Governing(findings, capacity, widths, request.Policy)
        from travel in findings.TraverseM(f => AssessmentFact.Ratio($"{f.Space.Value}/travel-distance", f.TravelM / request.Policy.AllowableTravelM)).As()
        from deadEnds in findings.Filter(static f => f.DeadEndM > 0.0).TraverseM(f => AssessmentFact.Ratio($"{f.Space.Value}/dead-end", f.DeadEndM / request.Policy.AllowableDeadEndM)).As()
        from commonPaths in findings.Filter(static f => f.CommonPathM > 0.0).TraverseM(f => AssessmentFact.Ratio($"{f.Space.Value}/common-path", f.CommonPathM / request.Policy.AllowableCommonPathM)).As()
        from rset in findings.TraverseM(f => AssessmentFact.Measure($"{f.Space.Value}/rset", Dimension.DurationDim, f.RsetMinutes * 60.0)).As()
        let nearestExits = findings.Map(static finding => AssessmentFact.Reference($"{finding.Space.Value}/nearest-exit", finding.NearestExit))
        from clearWidths in widths.TraverseM(row => AssessmentFact.Measure($"{row.Space.Value}/clear-width", Dimension.LengthDim, row.WidthM)).As()
        let underWidth = widths.Filter(row => row.WidthM < request.Policy.MinimumClearWidthM)
            .Map(static row => AssessmentFact.Flag($"{row.Space.Value}/under-width", true))
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
            travel + deadEnds + commonPaths + rset + nearestExits + clearWidths + underWidth + exits + bottlenecks,
            govern,
            new Provenance("CirculationAnalysis", request.Route.Standard, request.Route.SolverVersion, clock.GetCurrentInstant()));

    // TRAVEL: one Dijkstra accessor PER EXIT root (the view's edges run both directions, so an exit-rooted tree reads
    // space->exit distance); per space the nearest exit wins, the dead-end and common-path folds ride the SAME per-exit
    // paths, and the hydraulic RSET adds the door-queue time — an exit-unreachable space is the typed life-safety failure.
    static Fin<Seq<EgressFinding>> Travel(EgressGraph view, EgressPolicy policy) {
        Seq<(NodeId Exit, TryFunc<NodeId, System.Collections.Generic.IEnumerable<EgressEdge>> Paths)> rooted =
            view.Exits.Map(exit => (exit, view.Adjacency.ShortestPathsDijkstra(static edge => edge.LengthM, exit)));
        return view.Spaces.TraverseM(space => {
            Seq<(NodeId Exit, Seq<EgressEdge> Path, double LengthM)> routes = toSeq(rooted
                .Choose(root => root.Paths(space.Space, out System.Collections.Generic.IEnumerable<EgressEdge> path) ? Some((root.Exit, toSeq(path), toSeq(path).Sum(static e => e.LengthM))) : None)
                .OrderBy(static r => r.LengthM));
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
        Map<NodeId, int> index = NodeIndex(view, offset: 2);
        int NodeOf(NodeId id) => index[id];
        foreach ((NodeId Space, double AreaM2, OccupancyClass Occupancy) space in view.Spaces) { flow.AddArcWithCapacity(0, NodeOf(space.Space), view.OccupantLoad(space)); }
        // Each arc carries the capacity it was BUILT with, so saturation compares two values this fold already holds.
        Seq<(int Arc, long Capacity, EgressEdge Edge)> arcs = view.Adjacency.Edges.ToSeq().Map(edge => {
            long capacity = (long)Math.Round(edge.ClearWidthM * policy.CapacityPerMetreWidth);
            return (flow.AddArcWithCapacity(NodeOf(edge.From), NodeOf(edge.To), capacity), capacity, edge);
        });
        foreach (NodeId exit in view.Exits) { flow.AddArcWithCapacity(NodeOf(exit), 1, long.MaxValue / 4); }
        return flow.Solve(0, 1) switch {
            Google.OrTools.Graph.MaxFlow.Status.OPTIMAL => Fin.Succ((
                (double)view.Spaces.Sum(view.OccupantLoad),
                (double)flow.OptimalFlow(),
                arcs.Filter(pair => flow.Flow(pair.Arc) == pair.Capacity).Map(static pair => pair.Edge))),
            Google.OrTools.Graph.MaxFlow.Status status => Fin.Fail<(double, double, Seq<EgressEdge>)>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, $"<egress-flow:{status}>")),
        };
    }

    // Solver node addressing is ONE keyed index built per solve; the linear FindIndex it replaces made every arc
    // build an O(n) scan and the whole arc set O(n²) on a corpus-scale storey. The offset reserves the super-source
    // and super-sink ordinals each flow rail claims.
    static Map<NodeId, int> NodeIndex(EgressGraph view, int offset) =>
        (view.Spaces.Map(static s => s.Space) + view.Exits).Distinct().ToSeq()
            .Fold(Map<NodeId, int>(), (acc, id) => acc.ContainsKey(id) ? acc : acc.Add(id, acc.Count + offset));

    // DISTRIBUTION: occupant load routed to exits at least travel cost through MinCostFlow — supplies at spaces, one
    // drain per exit, arc costs the corridor lengths quantized to millimetres so the long costs are exact; the optimal
    // cost (occupant-millimetres, reported as occupant-metres) is the wayfinding-efficiency evidence a layout variant
    // screens on. A non-OPTIMAL status is the typed solve verdict.
    // Exemption: the OrTools arc-building loop is the native-solver marshaling statement seam.
    static Fin<double> Distribute(EgressGraph view, EgressPolicy policy) {
        Map<NodeId, int> index = NodeIndex(view, offset: 1);
        // Pre-sized ctor: the node and arc counts are known before the first Add, so the engine allocates once.
        using Google.OrTools.Graph.MinCostFlow flow = new(index.Count + 1, view.Adjacency.EdgeCount + view.Exits.Count);
        int NodeOf(NodeId id) => index[id];
        long total = view.Spaces.Sum(view.OccupantLoad);
        foreach ((NodeId Space, double AreaM2, OccupancyClass Occupancy) space in view.Spaces) { flow.SetNodeSupply(NodeOf(space.Space), view.OccupantLoad(space)); }
        foreach (EgressEdge edge in view.Adjacency.Edges) {
            flow.AddArcWithCapacityAndUnitCost(NodeOf(edge.From), NodeOf(edge.To),
                (long)Math.Round(edge.ClearWidthM * policy.CapacityPerMetreWidth), (long)Math.Round(edge.LengthM * 1000.0));
        }
        foreach (NodeId exit in view.Exits) { flow.AddArcWithCapacityAndUnitCost(NodeOf(exit), 0, long.MaxValue / 4, 0); }
        flow.SetNodeSupply(0, -total);
        // Status is declared on MinCostFlowBase, the shared node/arc base MinCostFlow derives from.
        return flow.SolveMaxFlowWithMinCost() switch {
            Google.OrTools.Graph.MinCostFlowBase.Status.OPTIMAL => Fin.Succ(flow.OptimalCost() / 1000.0),
            Google.OrTools.Graph.MinCostFlowBase.Status status => Fin.Fail<double>(new ComputeFault.AnalysisFailed(SolvePhase.Solve, FailureKind.Numeric, $"<egress-distribution:{status}>")),
        };
    }

    // Governing folds MEASURED ratios only: worst travel/dead-end/common-path, the policy-gated RSET, the capacity
    // demand-over-throughput, and the clear-width demand-over-achieved. Width is a real quantity the Clipper2
    // offset-collapse bisection measures, so its ratio is reproducible and chartable — the `BitIncrement(1.0)`
    // epsilon it replaces smuggled a boolean into the ratio channel as "just over unity", a number no probe emitted
    // and no operator could reproduce, while the flag it stood for now rides the typed AssessmentFact.Flag stream.
    static double Governing(Seq<EgressFinding> findings, (double DemandOccupants, double ThroughputOccupants, Seq<EgressEdge> Bottleneck) capacity, Seq<(NodeId Space, double WidthM)> widths, EgressPolicy policy) =>
        Math.Max(
            Math.Max(
                findings.Map(f => Math.Max(f.TravelM / policy.AllowableTravelM,
                    Math.Max(f.DeadEndM / policy.AllowableDeadEndM, f.CommonPathM / policy.AllowableCommonPathM))).Max(0.0),
                policy.AllowableRsetMinutes > 0.0 ? findings.Map(f => f.RsetMinutes / policy.AllowableRsetMinutes).Max(0.0) : 0.0),
            Math.Max(
                capacity.DemandOccupants / Math.Max(1.0, capacity.ThroughputOccupants),
                widths.Map(row => policy.MinimumClearWidthM / Math.Max(row.WidthM, WidthFloorM)).Max(0.0)));
}
```

## [03]-[PLANAR_SIDE]

- Owner: `EgressView` the ingress projection building the view — space boundaries resolved through the `GeometrySource` port (or decoded off the kernel slice-stack story contours), occupant areas the NTS polygon areas, corridor clearance the Clipper2 inward offset, isovists the planar sight fold; `SpaceBoundary` the resolved planar carrier.
- Entry: `Of(graph, request, geometry)` resolves each space's boundary by content key through the `GeometrySource` port (an unresolvable boundary rails `AnalysisFailed(Admission, Input, "<space-boundary-unresolved:…>")`), derives occupant areas off NTS `Polygon.Area`, corridor clear widths off the Clipper2 inward `InflatePaths(paths, -delta, JoinType.Miter, EndType.Polygon)` collapse bisection (the largest surviving inward offset is the inradius, so twice it is the narrowest passage — the medial axis without a hand-rolled skeleton, and a MEASURED width rather than a predicate), and door adjacency off the opening edges.
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
    // Clipper2 bisection measures each per-space clear width from.
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
                    graph.EdgesAt(r.Space).Choose(e => e is Relationship.Generic g && g.WireName == SpaceBoundaryWire && g.Relating == r.Space && g.Attribute(BoundaryRows.Host).IsSome
                        ? Some((r.Space, g.Related)) : None).ToSeq());
                Seq<NodeId> exits = toSeq(doorBindings.GroupBy(static b => b.Door)).Filter(static g => g.Count() == 1).Map(static g => g.Key);
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
        toSeq(bindings.GroupBy(static binding => binding.Door)).TraverseM(door =>
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

    static Option<double> DoorWidth(ElementGraph graph, NodeId door) => graph.Magnitude(door, QuantityRows.Width);

    static Fin<NetTopologySuite.Geometries.Point> Centroid(Seq<(NodeId Space, NetTopologySuite.Geometries.Polygon Boundary)> resolved, NodeId space) =>
        resolved.Find(row => row.Space == space).Map(static row => row.Boundary.Centroid)
            .ToFin(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<egress-space-centroid-unresolved:{space.Value}>"));

    // SCC connectivity census classifies a space component reaching no exit as a no-egress island — one typed
    // failure naming a member space, railed BEFORE the per-space travel fold spends its per-exit Dijkstra sweeps.
    static Fin<Unit> Census(QuikGraph.AdjacencyGraph<NodeId, EgressEdge> adjacency, Seq<(NodeId Space, double AreaM2, OccupancyClass Occupancy)> spaces, Seq<NodeId> exits) {
        // The labeling extension FILLS a supplied dictionary and returns the component count — the caller owns the
        // map, so it is constructed here rather than recovered from an out parameter the surface does not carry.
        System.Collections.Generic.Dictionary<NodeId, int> components = [];
        adjacency.StronglyConnectedComponents(components);
        Seq<int> exitComponents = exits.Choose(e => components.TryGetValue(e, out int c) ? Some(c) : None).Distinct();
        return spaces.Find(s => components.TryGetValue(s.Space, out int c) && !exitComponents.Contains(c)).Match(
            Some: s => Fin.Fail<Unit>(new ComputeFault.AnalysisFailed(SolvePhase.Admission, FailureKind.Input, $"<egress-island:{s.Space.Value}>")),
            None: () => Fin.Succ(unit));
    }

    static NetTopologySuite.Geometries.Polygon Polygon(FootprintPolygon footprint) =>
        Planar.CreatePolygon([.. footprint.Ring.Map(static p => new NetTopologySuite.Geometries.Coordinate(p.X, p.Y)), new(footprint.Ring[0].X, footprint.Ring[0].Y)]);

    const int WidthBisections = 12;   // inflate-delta refinements — sub-millimetre over any code clear-width bracket

    // Corridor clearance MEASURES rather than flags: the largest inward Clipper2 inflate a polygon survives is its
    // inradius, so the narrowest passage is twice that, and a bounded bisection over the inflate delta reads the
    // width directly — the medial axis without a hand-rolled skeleton. The policy minimum is the upper bracket, so a
    // compliant corridor costs ONE probe and reports at-or-above minimum without measuring further; only a pinched
    // one pays the refinement. Ring coordinates lift onto PathsD once; a Miter join keeps re-entrant corners honest
    // where a Round join would smooth a pinch past the test.
    public static double ClearWidthM(NetTopologySuite.Geometries.Polygon corridor, double minimumClearWidthM) {
        Clipper2Lib.PathsD paths = [[.. corridor.ExteriorRing.Coordinates.Select(static c => new Clipper2Lib.PointD(c.X, c.Y))]];
        bool Survives(double delta) =>
            Clipper2Lib.Clipper.InflatePaths(paths, -delta, Clipper2Lib.JoinType.Miter, Clipper2Lib.EndType.Polygon).Count > 0;
        double half = minimumClearWidthM / 2.0;
        if (Survives(half)) { return minimumClearWidthM; }
        (double Low, double High) bracket = (0.0, half);
        for (int step = 0; step < WidthBisections; step++) {
            double mid = 0.5 * (bracket.Low + bracket.High);
            bracket = Survives(mid) ? (mid, bracket.High) : (bracket.Low, mid);
        }
        return 2.0 * bracket.Low;
    }
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

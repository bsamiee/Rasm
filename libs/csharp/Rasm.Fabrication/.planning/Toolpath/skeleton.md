# [RASM_FABRICATION_SKELETON]

`Skeleton` owns clearance-derived constant-engagement milling over one admitted `SkeletonDemand`. Kernel `SkeletonGraph` supplies medial topology and per-node clearance radii, `WalkStrategy` rows own the motion grammar each cut modality demands, `EngagementLimit` rows own every competing radial ceiling, and `ArcAlgebra.Apply` owns arc-native emission. `Link.Route` remains the only inter-element travel owner.

`Skeleton.Walk` partitions the graph into connected components and walks each one against its own clearance profile, so a single narrow channel constrains only the component containing it. Every emitted element carries the component that generated it as construction-time evidence, never a proximity reconstruction. `SkeletonDemand` admits topology once and carries it forward, so no consumer recomputes the graph. `Cam` alone repeats the planar result across axial depth and composes linking, workholding, guarding, and kinematics.

## [01]-[INDEX]

- [02]-[LIMITS]: `EngagementLimit` rows derive every competing radial ceiling and fold to the binding one.
- [03]-[STRATEGY]: `WalkStrategy` rows own the motion grammar each modality lowers into `ArcOp`.
- [04]-[DEMAND]: `SkeletonDemand` closes stock, graph, cutter, engagement, strategy, and topology admission.
- [05]-[WALK]: `Skeleton.Walk` walks each component against its own clearance and emits typed receipts.

## [02]-[LIMITS]

- Owner: `EngagementLimit` is the vocabulary of radial ceilings; each row carries its own `Ceiling` derivation delegate and the fold selects the minimum as the binding row.
- Cases: `Channel` bounds engagement by the narrowest admitted clearance in the walked component; `Immersion` bounds it by the requested engagement angle; `Width` and `Scallop` bound it by the process budget and the finish demand; `Deflection`, `Stability`, and `ChipThinning` read the `ProcessBudget.Subtractive` columns the demand already admits, so tool-deflection, chatter, and chip-thinning ceilings bind the same fold as the geometric ones.
- Law: a ceiling is a row, never an inline `double.Min` term — a new limit axis lands as one row with every consumer and every receipt untouched.
- Law: `MeasuredLoad` reads the optional `Kinematics/observation.md` `LoadWindow` paired with its evaluation instant in one `Option` — a load without its instant is unrepresentable, never a sentinel-guarded state — scales a fresh positive sample around its reference radial depth, and returns a conservative zero for an invalid present window; only absence removes the ceiling. `EngagementSolution.Binding` still names the governing bound with no receipt change.
- Receipt: `EngagementSolution` carries every row's keyed ceiling and the binding row itself, so a constrained walk names which physics bound it rather than reporting an unattributed scalar.
- Boundary: the fold owns selection; no consumer re-derives a ceiling or re-orders the rows.

## [03]-[STRATEGY]

- Owner: `WalkStrategy` rows carry the `Operation` delegate lowering an admitted engagement into the `ArcOp` case that emits it.
- Cases: `Clearing` walks the component's offset family at constant engagement; `Trochoid` walks the medial chain as overlapping loops for full-width slotting; `Peel` walks one flank at reduced radial depth and full axial depth.
- Law: the medial chain is the guide, so motion follows the admitted clearance family rather than re-deriving a path from stock alone; a strategy differing only in emission is a row, never a sibling entrypoint.
- Boundary: `ArcAlgebra` owns exact-arc generation; the row selects the case and its arguments, and owns no geometry.

## [04]-[DEMAND]

- Owner: `SkeletonDemand` carries every value needed to reproduce the walk, topology included.
- Law: the demand carries the topology it validated against, so `Walk` reads admitted evidence and no second graph pass exists.
- Law: each component's canonical `SkeletonArc.OriginEdge` set travels into every element key and pass receipt as a counted ordinal run, so the digest is self-delimiting and no delimiter collision forges equality.
- Entry: `SkeletonDemand.Admit` mints `SkeletonTopology` once, folds stock, node, incidence, edge, geometry, budget, and clearance faults into one accumulated rail, and constructs through the generated factory.
- Auto: `UndirectedGraph<int, SEdge<int>>` rejects parallel edges and supplies `ConnectedComponents` labels during admission; `SkeletonTopology` retains detached edges and component labels.
- Packages: `TensorPrimitives.IsFiniteAll` admits coordinate, radius, and process batches before scalar inequalities classify them.
- Growth: a new graph producer projects the existing `SkeletonGraph`, and a new engagement axis becomes one `EngagementLimit` row.
- Boundary: `SkeletonTopology` confines QuikGraph's mutable graph and label-map materialization to one admission seam; every carried field is detached evidence.

## [05]-[WALK]

- Law: each connected component walks independently, so the binding limit is component-local and one narrow channel never collapses engagement across the whole part.
- Law: `ElementVariant.Of` derives the walk's rotation penalty, thermal exposure, and pierce count off emitted motion at the link owner, so a placeholder and a page-local re-derivation are both deleted forms; `CutElement.Identify` mints the key from the component, its origin edges, and the run ordinal.
- Entry: `Skeleton.Walk(SkeletonDemand)` is the only operation.
- Auto: the component's medial chain orders from its maximum-clearance node outward and enters `ArcOp` as the guide loop, so emission follows the admitted skeleton.
- Output: each component's emitted moves become one `CutElement` per contiguous cutting run, with rapid delimiters dropped, so branch and component travel stays absent from the cutting owner.
- Receipt: `SkeletonReceipt` carries per-component passes with their limit tables and binding rows, the graph census, and the flattened element projection `Cam` lowers; the settled census fires the `FabricationFact.Engine.Of` node, arc, and pass rows through the caller-supplied `FabricationTap`, defaulting silent for headless callers.
- Packages: `LanguageExt.Core` owns accumulation, keyed lookup, and traversal; `Thinktecture.Runtime.Extensions` owns demand construction and the delegate-bearing rows; `QuikGraph` owns component topology; `System.Numerics.Tensors` owns batch finiteness; `CavalierContours` arrives through `ArcAlgebra.Apply`; `MTConnect.NET-Common` arrives through `CutterForm` admission.
- Boundary: `ArcAlgebra` owns exact-arc path generation, `Cam` owns axial repetition and safety composition, and `Link` owns travel.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------------------------------------------------------------
using System.Numerics.Tensors;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using QuikGraph;
using QuikGraph.Algorithms;
using Rasm.Element.Projection;
using Rasm.Fabrication.Geometry2D;
using Rasm.Fabrication.Kinematics;
using Rasm.Fabrication.Process;
using Rasm.Fabrication.Spec;
using Rasm.Meshing;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Fabrication.Toolpath;

// --- [VOCABULARY] ---------------------------------------------------------------------------------------------------------------------------------
public readonly record struct EngagementInputs(
    double CutterRadius,
    double ChannelClearance,
    double TargetAngleDeg,
    double ScallopStep,
    ProcessBudget.Subtractive Budget,
    Option<(LoadWindow Window, Instant EvaluatedAt)> MeasuredLoad = default);

[SmartEnum<string>]
public sealed partial class EngagementLimit {
    public static readonly EngagementLimit Channel = new("channel", ChannelCeiling);
    public static readonly EngagementLimit Immersion = new("immersion", ImmersionCeiling);
    public static readonly EngagementLimit Width = new("width", WidthCeiling);
    public static readonly EngagementLimit Scallop = new("scallop", ScallopCeiling);
    public static readonly EngagementLimit Deflection = new("deflection", DeflectionCeiling);
    public static readonly EngagementLimit Stability = new("stability", StabilityCeiling);
    public static readonly EngagementLimit ChipThinning = new("chip-thinning", ChipThinningCeiling);
    public static readonly EngagementLimit MeasuredLoad = new("measured-load", MeasuredLoadCeiling);

    [UseDelegateFromConstructor]
    public partial double Ceiling(EngagementInputs inputs);

    private static double ChannelCeiling(EngagementInputs inputs) => inputs.ChannelClearance;

    private static double ImmersionCeiling(EngagementInputs inputs) =>
        inputs.CutterRadius * (1.0 - Math.Cos(inputs.TargetAngleDeg * Math.PI / 180.0));

    private static double WidthCeiling(EngagementInputs inputs) => inputs.Budget.WidthOfCut;

    private static double ScallopCeiling(EngagementInputs inputs) => inputs.ScallopStep;

    private static double DeflectionCeiling(EngagementInputs inputs) =>
        inputs.Budget.DeflectionMm > 0.0
            ? inputs.Budget.WidthOfCut * inputs.Budget.DeflectionMm / (inputs.Budget.DeflectionMm + inputs.CutterRadius)
            : double.PositiveInfinity;

    private static double StabilityCeiling(EngagementInputs inputs) =>
        inputs.Budget.StabilityLimitMm > 0.0 ? inputs.Budget.StabilityLimitMm : double.PositiveInfinity;

    private static double ChipThinningCeiling(EngagementInputs inputs) =>
        inputs.Budget.ChipThinningFactor > 0.0
            ? inputs.CutterRadius * inputs.Budget.ChipThinningFactor
            : double.PositiveInfinity;

    // Cutting load scales near-linearly with radial engagement. A present window that is zero, future, or expired
    // binds conservatively to zero; only an absent window removes this ceiling.
    private static double MeasuredLoadCeiling(EngagementInputs inputs) =>
        inputs.MeasuredLoad.Match(
            Some: static measured => measured.Window.Ceiling(measured.EvaluatedAt).IfNone(0.0),
            None: static () => double.PositiveInfinity);

    public static EngagementSolution Solve(EngagementInputs inputs) =>
        Items.ToSeq()
            .Map(row => (Row: row, Ceiling: row.Ceiling(inputs)))
            .Fold(
                (Table: HashMap<EngagementLimit, double>(), Binding: Channel, Radial: double.PositiveInfinity),
                static (state, row) => (
                    state.Table.Add(row.Row, row.Ceiling),
                    row.Ceiling < state.Radial ? row.Row : state.Binding,
                    double.Min(state.Radial, row.Ceiling)))
            .Apply(state => new EngagementSolution(
                state.Table,
                state.Binding,
                state.Radial));
}

// Each row names the `CutStrategy` its emission IS, so the element key the canonical mint digests carries the same
// strategy discriminant a motion-generated element does and the two schemes stay comparable.
[SmartEnum<string>]
public sealed partial class WalkStrategy {
    public static readonly WalkStrategy Clearing = new("clearing", CutStrategy.Adaptive, ClearingOp);
    public static readonly WalkStrategy Trochoid = new("trochoid", CutStrategy.Trochoidal, TrochoidOp);
    public static readonly WalkStrategy Peel = new("peel", CutStrategy.Slot, PeelOp);

    public CutStrategy Cut { get; }

    [UseDelegateFromConstructor]
    public partial ArcOp Operation(ArcForest stock, Loop guide, EngagementSolution engagement, double cutterRadius, double feed, CutSense sense);

    private static ArcOp ClearingOp(ArcForest stock, Loop guide, EngagementSolution engagement, double cutterRadius, double feed, CutSense sense) =>
        new ArcOp.Adaptive(stock, Some(guide), cutterRadius, engagement.Radial, engagement.Step, feed, sense);

    private static ArcOp TrochoidOp(ArcForest stock, Loop guide, EngagementSolution engagement, double cutterRadius, double feed, CutSense sense) =>
        new ArcOp.Adaptive(stock, Some(guide), cutterRadius, double.Min(engagement.Radial, cutterRadius), engagement.Step, feed, sense);

    private static ArcOp PeelOp(ArcForest stock, Loop guide, EngagementSolution engagement, double cutterRadius, double feed, CutSense sense) =>
        new ArcOp.Adaptive(stock, Some(guide), cutterRadius, engagement.Step, engagement.Step, feed, sense);
}

// --- [MODELS] -------------------------------------------------------------------------------------------------------------------------------------
public sealed record EngagementSolution(
    HashMap<EngagementLimit, double> Ceilings,
    EngagementLimit Binding,
    double Radial) {
    public double Step => Radial;
}

public sealed record SkeletonTopology(
    Arr<(int From, int To)> Edges,
    Arr<int> Components,
    int ComponentCount,
    int DuplicateEdges) {
    public Seq<int> NodesOf(int component) =>
        Range(0, Components.Count).ToSeq().Filter(index => Components[index] == component).ToSeq();
}

[ComplexValueObject]
public sealed partial class SkeletonDemand {
    public ArcForest Stock { get; }
    public SkeletonGraph Graph { get; }
    public CutterForm Cutter { get; }
    public EngagementPolicy Engagement { get; }
    public CutSense Sense { get; }
    public WalkStrategy Strategy { get; }
    public ProcessModality Modality { get; }
    public SkeletonTopology Topology { get; }
    public Option<(LoadWindow Window, Instant EvaluatedAt)> MeasuredLoad { get; }

    public static Fin<SkeletonDemand> Admit(
        ArcForest stock,
        SkeletonGraph graph,
        CutterForm cutter,
        EngagementPolicy engagement,
        CutSense sense,
        WalkStrategy strategy,
        ProcessModality modality,
        Option<(LoadWindow Window, Instant EvaluatedAt)> measuredLoad = default) =>
        from admitted in Optional(graph).ToFin(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "skeleton:graph-absent"))
        let topology = Skeleton.Topology(admitted)
        from _ in Skeleton.Facts(stock, admitted, cutter, engagement, topology).ToFin()
        from demand in Validate(stock, admitted, cutter, engagement, sense, strategy, modality, topology, measuredLoad,
            out SkeletonDemand row) is { } error
            ? Fin.Fail<SkeletonDemand>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, error.Message))
            : Fin.Succ(row)
        select demand;

    static partial void ValidateFactoryArguments(
        ref ValidationError? validationError,
        ref ArcForest stock,
        ref SkeletonGraph graph,
        ref CutterForm cutter,
        ref EngagementPolicy engagement,
        ref CutSense sense,
        ref WalkStrategy strategy,
        ref ProcessModality modality,
        ref SkeletonTopology topology,
        ref Option<(LoadWindow Window, Instant EvaluatedAt)> measuredLoad) =>
        validationError = topology is null || modality is null || topology.Components.Count != graph.Nodes.Count
            ? new ValidationError("<skeleton-topology-mismatch>")
            : null;
}

public sealed record SkeletonPass(
    int Component,
    Seq<int> OriginEdges,
    Seq<CutElement> Elements,
    EngagementSolution Engagement,
    double ChannelClearance,
    int NodeCount,
    int ArcCount);

public sealed record SkeletonReceipt(
    Seq<SkeletonPass> Passes,
    Arr<int> Components,
    int ComponentCount,
    int NodeCount,
    int ArcCount,
    double CutterRadius) {
    public Seq<CutElement> Elements => Passes.Bind(static pass => pass.Elements);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------------------------------------------------------------
public static class Skeleton {
    public static Fin<SkeletonReceipt> Walk(SkeletonDemand demand, FabricationTap? tap = null) =>
        from tolerance in Tolerance.Apply(new ToleranceRequest.Scallop(demand.Engagement.Finish, demand.Cutter))
        from scallop in tolerance is ToleranceReceipt.Scallop receipt
            ? Fin.Succ(receipt.StepMm)
            : Fin.Fail<double>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "skeleton:scallop-receipt"))
        from budget in demand.Engagement.Budget is ProcessBudget.Subtractive subtractive
            ? Fin.Succ(subtractive)
            : Fin.Fail<ProcessBudget.Subtractive>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "skeleton:budget"))
        let cutterRadius = demand.Cutter.Diameter / 2.0
        from passes in Range(0, demand.Topology.ComponentCount).ToSeq()
            .Map(component => Component(demand, budget, cutterRadius, scallop, component))
            .TraverseM(identity)
            .As()
        let walked = new SkeletonReceipt(
            passes,
            demand.Topology.Components,
            demand.Topology.ComponentCount,
            demand.Graph.Nodes.Count,
            demand.Graph.Arcs.Count,
            cutterRadius)
        let _fact = FabricationFact.Engine.Of(walked).Map((tap ?? FabricationTap.Silent).Fire).Strict()
        select walked;

    private static Fin<SkeletonPass> Component(
        SkeletonDemand demand,
        ProcessBudget.Subtractive budget,
        double cutterRadius,
        double scallop,
        int component) =>
        from nodes in demand.Topology.NodesOf(component) is { IsEmpty: false } members
            ? Fin.Succ(members)
            : Fin.Fail<Seq<int>>(new GeometryFault.DegenerateInput(Kind.Curve, component, $"skeleton:component-{component}:empty").ToError())
        from arcs in demand.Graph.Arcs.Filter(arc => demand.Topology.Components[arc.From] == component) is { IsEmpty: false } spans
            ? Fin.Succ(spans)
            : Fin.Fail<Seq<SkeletonArc>>(new GeometryFault.DegenerateInput(Kind.Curve, component, $"skeleton:component-{component}:arcs").ToError())
        let origins = arcs.Map(static arc => arc.OriginEdge).Distinct().Order().ToSeq()
        let clearance = arcs
            .Map(arc => double.Min(demand.Graph.Nodes[arc.From].Radius, demand.Graph.Nodes[arc.To].Radius) - cutterRadius)
            .Fold(double.PositiveInfinity, double.Min)
        let engagement = EngagementLimit.Solve(
            new EngagementInputs(
                cutterRadius, clearance, demand.Engagement.TargetAngle, scallop, budget, demand.MeasuredLoad))
        from _ in guard(
            double.IsFinite(engagement.Radial) && engagement.Radial > 0.0
                && double.IsFinite(engagement.Step) && engagement.Step > 0.0,
            (Error)new FabricationFault.PolicyInadmissible(
                FabConcern.Toolpath, $"skeleton:component-{component}:{engagement.Binding.Key}")).ToFin()
        from guide in Chain(demand.Graph, demand.Topology, nodes, demand.Stock.Tolerance, component)
        from trace in ArcAlgebra.Apply(demand.Strategy.Operation(
            demand.Stock, guide, engagement, cutterRadius, budget.FeedRate, demand.Sense))
        from motion in trace is ArcTrace.Motion moved
            ? Fin.Succ(moved.Receipt)
            : Fin.Fail<MotionReceipt>(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, $"skeleton:component-{component}:arc-trace"))
        from elements in Elements(demand, origins, component, motion.Moves)
        select new SkeletonPass(component, origins, elements, engagement, clearance, nodes.Count, arcs.Count);

    internal static SkeletonTopology Topology(SkeletonGraph source) {
        UndirectedGraph<int, SEdge<int>> graph = new(allowParallelEdges: false);
        graph.AddVertexRange(Enumerable.Range(0, source.Nodes.Count));
        (int Duplicates, Arr<(int From, int To)> Edges) topology = source.Arcs.Fold(
            (Duplicates: 0, Edges: Arr<(int From, int To)>()),
            (state, arc) => arc is null || arc.From < 0 || arc.From >= source.Nodes.Count
                || arc.To < 0 || arc.To >= source.Nodes.Count || arc.From == arc.To
                    ? state
                    : graph.AddEdge(new SEdge<int>(arc.From, arc.To))
                        ? (state.Duplicates, state.Edges.Add((arc.From, arc.To)))
                        : (state.Duplicates + 1, state.Edges));
        Dictionary<int, int> labels = [];
        int components = graph.ConnectedComponents(labels);
        return new SkeletonTopology(
            topology.Edges,
            Enumerable.Range(0, source.Nodes.Count).Select(index => labels.GetValueOrDefault(index, -1)).ToArr(),
            components,
            topology.Duplicates);
    }

    internal static Validation<Error, Unit> Facts(
        ArcForest? stock,
        SkeletonGraph graph,
        CutterForm? cutter,
        EngagementPolicy? engagement,
        SkeletonTopology topology) =>
        (stock, cutter, engagement) switch {
            ({ } admittedStock, { } admittedCutter, { } admittedEngagement) =>
                (admittedCutter.Diameter / 2.0)
                    .Apply(cutterRadius => DemandFacts(admittedStock, graph, admittedEngagement, topology)
                        + NodeFacts(graph, cutterRadius, topology)
                        + ArcFacts(graph, cutterRadius))
                    .Apply(static facts => facts.IsEmpty
                        ? Validation<Error, Unit>.Success(unit)
                        : Validation<Error, Unit>.Fail(Error.Many([.. facts]))),
            _ => Validation<Error, Unit>.Fail(new FabricationFault.PolicyInadmissible(FabConcern.Toolpath, "skeleton:demand-absent")),
        };

    private static Seq<Error> DemandFacts(
        ArcForest stock,
        SkeletonGraph graph,
        EngagementPolicy engagement,
        SkeletonTopology topology) =>
        Axes(-1, Seq(
            (Ok: !stock.Loops.IsEmpty, Axis: "stock-empty"),
            (Ok: !graph.Nodes.IsEmpty, Axis: "nodes-empty"),
            (Ok: !graph.Arcs.IsEmpty, Axis: "arcs-empty"),
            (Ok: topology.DuplicateEdges == 0, Axis: "duplicate-edge"),
            (Ok: engagement.Budget is ProcessBudget.Subtractive, Axis: "budget"),
            (Ok: engagement.TargetAngle is > 0.0 and < 180.0, Axis: "target-angle")));

    private static Seq<Error> NodeFacts(SkeletonGraph graph, double cutterRadius, SkeletonTopology topology) =>
        graph.Nodes
            .Map((node, index) => node is null
                ? Seq(new GeometryFault.DegenerateInput(Kind.Curve, index, $"skeleton:node-{index}:absent").ToError())
                : Axes(index, Seq(
                    (Ok: TensorPrimitives.IsFiniteAll<double>([node.At.X, node.At.Y, node.At.Z, node.Radius]), Axis: "finite"),
                    (Ok: node.At.IsValid, Axis: "point"),
                    (Ok: node.Radius > cutterRadius, Axis: "clearance"),
                    (Ok: topology.Edges.Exists(edge => edge.From == index || edge.To == index), Axis: "isolated")), "node"))
            .Bind(static errors => errors);

    private static Seq<Error> ArcFacts(SkeletonGraph graph, double cutterRadius) =>
        graph.Arcs
            .Map((arc, index) => arc switch {
                null => Seq(new GeometryFault.DegenerateInput(Kind.Curve, index, $"skeleton:arc-{index}:absent").ToError()),
                { } admitted => (
                        Endpoints: admitted.From >= 0 && admitted.From < graph.Nodes.Count
                            && admitted.To >= 0 && admitted.To < graph.Nodes.Count,
                        Distinct: admitted.From != admitted.To)
                    .Apply(state => (state.Endpoints && state.Distinct
                            && graph.Nodes[admitted.From] is { } from && graph.Nodes[admitted.To] is { } to
                                ? Some((Length: from.At.DistanceTo(to.At), Clearance: double.Min(from.Radius, to.Radius) - cutterRadius))
                                : Option<(double Length, double Clearance)>.None)
                        .Apply(span => Axes(index, Seq(
                            (Ok: state.Endpoints, Axis: "endpoint"),
                            (Ok: state.Distinct, Axis: "self-loop"),
                            (Ok: admitted.OriginEdge >= 0, Axis: "origin-edge"),
                            (Ok: span.ForAll(static value => double.IsFinite(value.Length) && value.Length > 0.0), Axis: "length"),
                            (Ok: span.ForAll(static value => double.IsFinite(value.Clearance) && value.Clearance > 0.0), Axis: "channel")), "arc"))),
            })
            .Bind(static errors => errors);

    private static Seq<Error> Axes(int index, Seq<(bool Ok, string Axis)> axes, string owner = "") =>
        axes.Choose(fact => fact.Ok
            ? Option<Error>.None
            : Some(new GeometryFault.DegenerateInput(Kind.Curve, index < 0 ? Option<int>.None : index, index < 0
                ? $"skeleton:{fact.Axis}"
                : $"skeleton:{owner}-{index}:{fact.Axis}").ToError()));

    private static Fin<Loop> Chain(
        SkeletonGraph graph,
        SkeletonTopology topology,
        Seq<int> nodes,
        Context tolerance,
        int component) =>
        topology.Edges
            .Filter(edge => topology.Components[edge.From] == component)
            .Fold(
                HashMap<int, Seq<int>>(),
                static (map, edge) => map
                    .AddOrUpdate(edge.From, existing => existing.Add(edge.To), Seq(edge.To))
                    .AddOrUpdate(edge.To, existing => existing.Add(edge.From), Seq(edge.From)))
            .Apply(adjacency => nodes
                .Fold(
                    (Seed: Option<int>.None, Radius: double.NegativeInfinity),
                    (best, index) => graph.Nodes[index].Radius > best.Radius
                        ? (Some(index), graph.Nodes[index].Radius)
                        : best)
                .Apply(best => best.Seed.Map(seed => Ordered(adjacency, seed)).IfNone(Seq<int>())))
            .Apply(ordered => ordered.Count >= 2
                ? Loop.Admit(
                    ordered.Map(index => graph.Nodes[index].At).ToArr(),
                    closed: false,
                    Arr<double>(),
                    tolerance)
                : Fin.Fail<Loop>(new GeometryFault.DegenerateInput(Kind.Curve, component, $"skeleton:component-{component}:chain").ToError()));

    private static Seq<int> Ordered(HashMap<int, Seq<int>> adjacency, int seed) {
        HashSet<int> visited = [];
        List<int> order = [];
        Visit(seed, returnToParent: false);
        return toSeq(order);

        void Visit(int node, bool returnToParent) {
            visited.Add(node);
            order.Add(node);
            int[] children = adjacency.Find(node).IfNone(Seq<int>()).Order().ToArray();
            for (int index = 0; index < children.Length; index++) {
                int next = children[index];
                if (visited.Contains(next))
                    continue;
                bool childReturns = returnToParent || children.Skip(index + 1).Any(candidate => !visited.Contains(candidate));
                Visit(next, childReturns);
                if (childReturns)
                    order.Add(node);
            }
        }
    }

    private static Fin<Seq<CutElement>> Elements(
        SkeletonDemand demand,
        Seq<int> origins,
        int component,
        Seq<Move> moves) =>
        moves.Fold(
                (Paths: Seq<Seq<Move>>(), Current: Seq<Move>()),
                static (state, move) => move is Move.Rapid
                    ? (state.Current.IsEmpty ? state.Paths : state.Paths.Add(state.Current), Seq<Move>())
                    : (state.Paths, state.Current.Add(move)))
            .Apply(static state => state.Current.IsEmpty ? state.Paths : state.Paths.Add(state.Current))
            .Filter(static path => !path.IsEmpty)
            .Map((path, index) => (Path: path, Index: index))
            .TraverseM(row => Element(demand, origins, component, row.Path, row.Index))
            .As();

    // Identity and the objective axes are BOTH the link owner's: the walk supplies its discriminants — the walked
    // component, its canonical origin-edge set, and the contiguous run's ordinal — and `CutElement.Identify` digests
    // them beside tool, work offset, cutter geometry, and every move. An interpolated page-local string could not
    // separate two walks of one component that differ only in emitted arcs, and it minted a second identity scheme
    // beside the canonical one. `Cam` owns the modality this element runs under, so the demand carries it forward.
    private static Fin<CutElement> Element(
        SkeletonDemand demand,
        Seq<int> origins,
        int component,
        Seq<Move> path,
        int index) =>
        from key in CutElement.Identify(new CutElementIdentity.Skeleton(
            component,
            origins,
            index,
            demand.Strategy.Cut,
            ToolKey(demand),
            demand.Engagement.WorkOffset,
            demand.Cutter.Family.Key,
            demand.Cutter.Diameter,
            demand.Cutter.CornerRadius,
            demand.Cutter.TaperAngle,
            demand.Cutter.FluteLength,
            path))
        from element in CutElement.Admit(
            key,
            ToolKey(demand),
            demand.Engagement.WorkOffset,
            new EntryFamily.Fixed(ElementVariant.Of(key, path, demand.Modality)))
        select element;

    private static string ToolKey(SkeletonDemand demand) =>
        demand.Cutter.Evidence.Map(static evidence => evidence.ToolId).IfNone(demand.Cutter.Family.Key);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

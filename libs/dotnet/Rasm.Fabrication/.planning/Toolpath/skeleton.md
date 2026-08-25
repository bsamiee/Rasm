# [RASM_FABRICATION_SKELETON]

`Skeleton` owns clearance-derived constant-engagement milling over one admitted `SkeletonDemand`. Kernel `SkeletonGraph` supplies medial topology and per-node clearance radii, `WalkStrategy` rows own the motion grammar each cut modality demands, `EngagementLimit` rows own every competing ceiling on the two engagement axes, and `ArcAlgebra.Apply` owns arc-native emission. `Link.Route` remains the only inter-element travel owner.

`Skeleton.Walk` partitions the graph into connected components and walks each one against its own clearance profile, so a single narrow channel constrains only the component containing it. Each component walk is one `DepthFirstSearchAlgorithm` descent whose recorded tree edges and parent map build the Euler guide, and whose component label and visit ordinal ride the pass as published columns. `SkeletonDemand` admits topology once and carries the walked graph forward, so no consumer rebuilds adjacency. `Cam` alone repeats the planar result across axial depth and composes linking, workholding, guarding, and kinematics.

## [01]-[INDEX]

- [02]-[LIMITS]: `EngagementLimit` rows bind the radial and advance axes separately and fold to the binding row on each.
- [03]-[STRATEGY]: `WalkStrategy` rows own the motion grammar each modality lowers into `ArcOp`.
- [04]-[DEMAND]: `SkeletonDemand` closes stock, graph, cutter, engagement, strategy, and topology admission through one hook.
- [05]-[WALK]: `Skeleton.Walk` walks each component against its own clearance and emits typed passes.

## [02]-[LIMITS]

- Owner: `EngagementAxis` names the two axes an adaptive cut prices independently — the arc's radial immersion and the advance between successive offset levels; `EngagementLimit` is the vocabulary of ceilings, each row carrying the axis it binds and its own `Ceiling` derivation delegate.
- Cases: `Channel` bounds immersion by the narrowest admitted clearance in the walked component; `Immersion` bounds it by the requested engagement angle; `Deflection`, `Stability`, `ChipThinning`, and `MeasuredLoad` bound it from the `ProcessBudget.Subtractive` columns the demand already admits. `Width` and `Scallop` bind the ADVANCE axis, because a stepover is what the process budget's width of cut and the finish demand's scallop step actually constrain.
- Law: a ceiling is `Option<double>` — a row that does not bind is ABSENT, never a positive-infinity sentinel a fold must filter and a pass would publish as a measurement. `Ceilings` therefore holds the binding rows alone.
- Law: `MeasuredLoad` reads the optional `Kinematics/observation.md` `LoadWindow` paired with its evaluation instant in one `Option` — a load without its instant is unrepresentable — scales a fresh positive sample around its reference radial depth, and binds conservatively at zero for an invalid present window; only absence removes the ceiling.
- Entry: `EngagementLimit.Solve` returns `Fin<EngagementSolution>`, so an axis no row bound and a non-positive bound both refuse HERE naming the binding row, and every consumer reads an engagement it never has to re-gate.
- Output: `EngagementSolution` carries every bound row's keyed ceiling and the binding row on each axis, so a constrained walk names which physics bound its immersion and which bound its advance rather than reporting one unattributed scalar.
- Boundary: the fold owns selection; no consumer re-derives a ceiling or re-orders the rows.

## [03]-[STRATEGY]

- Owner: `WalkStrategy` rows carry the `Operation` delegate lowering an admitted engagement into the `ArcOp` case that emits it.
- Cases: `Clearing` drives the component's offset family at the full admitted immersion and the admitted advance; `Trochoid` caps immersion at the cutter's own radius so a full-width slot stays representable; `Peel` drives ONE flank at the advance width, which is the reduced radial engagement the grammar exists for.
- Law: the three rows differ in the ARGUMENTS they draw from the two engagement axes, never in a shared scalar — a grammar whose immersion and advance both read one value cannot keep its own promise, so the axis split is what makes `Peel` a distinct cut rather than `Clearing` under another key.
- Law: the medial chain is the guide, so motion follows the admitted clearance family rather than re-deriving a path from stock alone; a strategy differing only in emission is a row, never a sibling entrypoint.
- Boundary: `ArcAlgebra` owns exact-arc generation; the row selects the case and its arguments, and owns no geometry.

## [04]-[DEMAND]

- Owner: `SkeletonTopology` owns the ONE admitted graph — the directed container carrying each medial arc in both senses, its component labels, its detached edge list, and its duplicate census; `SkeletonDemand` carries every value needed to reproduce the walk, that topology included.
- Law: admission is CLOSED — the geometric fact fold runs inside the generated validation hook, so `Create`, `TryCreate`, and `Validate` all cross it and no caller can seat a demand whose graph the facts would have refused.
- Law: the topology mints only through its own internal factory off a `SkeletonGraph`, so a fabricated label map or edge list cannot enter beside a graph that disagrees with it.
- Law: each component's canonical `SkeletonArc.OriginEdge` set travels into every element key and pass as a counted ordinal run, so the digest is self-delimiting and no delimiter collision forges equality.
- Auto: `BidirectionalGraph<int, SEdge<int>>` rejects parallel edges, so a repeated arc is counted rather than doubled; `WeaklyConnectedComponents` labels the symmetric pairs and `OutDegree` answers isolation in one probe.
- Packages: `TensorPrimitives.IsFiniteAll` admits coordinate, radius, and process batches before scalar inequalities classify them.
- Growth: a new graph producer projects the existing `SkeletonGraph`, and a new engagement axis becomes one `EngagementLimit` row carrying the `EngagementAxis` it binds.
- Boundary: `SkeletonTopology` confines QuikGraph's mutable container, its label materialization, and the per-component descent to one owner; every published field is detached evidence.

## [05]-[WALK]

- Law: each connected component walks independently, so the binding limit is component-local and one narrow channel never collapses engagement across the whole part.
- Law: the component descent is ONE `DepthFirstSearchAlgorithm` run — `OutEdgesFilter` carries the deterministic child order, `EdgeRecorderObserver` records the tree edges in discovery order, and `VertexPredecessorRecorderObserver` records the parent map the return legs ascend, both attached over the disposable scope the observer seam returns. The emitted guide is the Euler walk of that tree, so consecutive guide vertices stay adjacent and a hand recursive descent with its unbounded depth is the deleted form.
- Law: `ElementVariant.Of` derives the walk's rotation penalty, thermal exposure, and pierce count off emitted motion at the link owner, so a placeholder and a page-local re-derivation are both deleted forms; `CutElement.Identify` mints the key from the component, its origin edges, and the run ordinal.
- Entry: `Skeleton.Walk(SkeletonDemand)` is the only operation.
- Entry: `SkeletonBench.Workload` builds the `skeleton-offset` measured demand from literal ordinals — a multi-component chain graph with per-channel clearance bands over an injected cutter and engagement — and `SkeletonBench.Run` is the fold the corpus gate times against `FabricationBenchClaims.SkeletonOffset`; measurement and benchmark projection stay the bench edge's under the AppHost claim-field map.
- Auto: the component's medial chain seeds at its maximum-clearance node and enters `ArcOp` as the guide loop, so emission follows the admitted skeleton.
- Output: each component's emitted moves become one `CutElement` per contiguous cutting run, with rapid delimiters dropped, so branch and component travel stays absent from the cutting owner.
- Output: `SkeletonPass` publishes the algorithm's OWN outputs beside the geometry — the component label it walked, the discovery-ordered visit sequence, and the tree-edge count — so a consumer reads the traversal evidence rather than reconstructing it from proximity. `SkeletonWalk` carries those passes with their limit tables and binding rows, the graph census, and the flattened element projection `Cam` lowers; the settled census writes its node, arc, and pass counts onto `FabricationInstruments.EngineSteps` through the caller-supplied set, which defaults absent for headless callers.
- Packages: `LanguageExt.Core` owns accumulation, keyed lookup, and traversal; `Thinktecture.Runtime.Extensions` owns demand construction and the delegate-bearing rows; `QuikGraph` owns component topology, the depth-first descent, and its observers; `System.Numerics.Tensors` owns batch finiteness; `CavalierContours` arrives through `ArcAlgebra.Apply`; `MTConnect.NET-Common` arrives through `CutterForm` admission.
- Boundary: `ArcAlgebra` owns exact-arc path generation, `Cam` owns axial repetition and safety composition, and `Link` owns travel.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Numerics.Tensors;
using LanguageExt;
using LanguageExt.Common;
using NodaTime;
using Rasm.Domain;
using QuikGraph;
using QuikGraph.Algorithms;
using UnitsNet;
using QuikGraph.Algorithms.Observers;
using QuikGraph.Algorithms.Search;
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

// --- [VOCABULARY] ----------------------------------------------------------------------
public readonly record struct EngagementInputs(
    double CutterRadius,
    double ChannelClearance,
    Angle TargetAngle,
    double ScallopStep,
    ProcessBudget.Subtractive Budget,
    Option<(LoadWindow Window, Instant EvaluatedAt)> MeasuredLoad = default);

[SmartEnum<string>]
public sealed partial class EngagementAxis {
    public static readonly EngagementAxis Radial = new("radial");
    public static readonly EngagementAxis Advance = new("advance");
}

[SmartEnum<string>]
public sealed partial class EngagementLimit {
    public static readonly EngagementLimit Channel = new("channel", EngagementAxis.Radial, ChannelCeiling);
    public static readonly EngagementLimit Immersion = new("immersion", EngagementAxis.Radial, ImmersionCeiling);
    public static readonly EngagementLimit Deflection = new("deflection", EngagementAxis.Radial, DeflectionCeiling);
    public static readonly EngagementLimit Stability = new("stability", EngagementAxis.Radial, StabilityCeiling);
    public static readonly EngagementLimit ChipThinning = new("chip-thinning", EngagementAxis.Radial, ChipThinningCeiling);
    public static readonly EngagementLimit MeasuredLoad = new("measured-load", EngagementAxis.Radial, MeasuredLoadCeiling);
    public static readonly EngagementLimit Width = new("width", EngagementAxis.Advance, WidthCeiling);
    public static readonly EngagementLimit Scallop = new("scallop", EngagementAxis.Advance, ScallopCeiling);

    public EngagementAxis Axis { get; }

    [UseDelegateFromConstructor]
    public partial Option<double> Ceiling(EngagementInputs inputs);

    private static Option<double> ChannelCeiling(EngagementInputs inputs) => Some(inputs.ChannelClearance);

    private static Option<double> ImmersionCeiling(EngagementInputs inputs) =>
        Some(inputs.CutterRadius * (1.0 - Math.Cos(inputs.TargetAngle.Radians)));

    private static Option<double> WidthCeiling(EngagementInputs inputs) => Some(inputs.Budget.WidthOfCut);

    private static Option<double> ScallopCeiling(EngagementInputs inputs) => Some(inputs.ScallopStep);

    private static Option<double> DeflectionCeiling(EngagementInputs inputs) =>
        inputs.Budget.DeflectionMm > 0.0
            ? Some(inputs.Budget.WidthOfCut * inputs.Budget.DeflectionMm / (inputs.Budget.DeflectionMm + inputs.CutterRadius))
            : None;

    private static Option<double> StabilityCeiling(EngagementInputs inputs) =>
        inputs.Budget.StabilityLimitMm > 0.0 ? Some(inputs.Budget.StabilityLimitMm) : None;

    private static Option<double> ChipThinningCeiling(EngagementInputs inputs) =>
        inputs.Budget.ChipThinningFactor > 0.0 ? Some(inputs.CutterRadius * inputs.Budget.ChipThinningFactor) : None;

    private static Option<double> MeasuredLoadCeiling(EngagementInputs inputs) =>
        inputs.MeasuredLoad.Map(static measured => measured.Window.Ceiling(measured.EvaluatedAt).IfNone(0.0));

    public static Fin<EngagementSolution> Solve(EngagementInputs inputs) =>
        Items.ToSeq()
            .Choose(row => row.Ceiling(inputs).Filter(double.IsFinite).Map(ceiling => (Row: row, Ceiling: ceiling)))
            .Fold(
                (Table: HashMap<EngagementLimit, double>(),
                 Bounds: HashMap<EngagementAxis, (EngagementLimit Row, double Value)>()),
                static (state, row) => (
                    state.Table.Add(row.Row, row.Ceiling),
                    state.Bounds.AddOrUpdate(
                        row.Row.Axis,
                        held => row.Ceiling < held.Value ? (row.Row, row.Ceiling) : held,
                        (row.Row, row.Ceiling))))
            .Apply(state => (state.Bounds.Find(EngagementAxis.Radial), state.Bounds.Find(EngagementAxis.Advance))
                .Apply((radial, advance) => new EngagementSolution(
                    state.Table, radial.Row, radial.Value, advance.Row, advance.Value))
                .As()
                .ToFin(new KernelFault.InvalidValue("skeleton", "skeleton:engagement-unbound")))
            .Bind(static solution => solution.Radial > 0.0 && solution.Advance > 0.0
                ? Fin.Succ(solution)
                : Fin.Fail<EngagementSolution>(new KernelFault.InvalidValue("skeleton", $"skeleton:engagement:{(solution.Radial > 0.0 ? solution.BindingAdvance : solution.BindingRadial).Key}")));
}

[SmartEnum<string>]
public sealed partial class WalkStrategy {
    public static readonly WalkStrategy Clearing = new("clearing", CutStrategy.Adaptive, ClearingOp);
    public static readonly WalkStrategy Trochoid = new("trochoid", CutStrategy.Trochoidal, TrochoidOp);
    public static readonly WalkStrategy Peel = new("peel", CutStrategy.Slot, PeelOp);

    public CutStrategy Cut { get; }

    [UseDelegateFromConstructor]
    public partial ArcOp Operation(
        ArcForest stock, Loop guide, EngagementSolution engagement, double cutterRadius, double feed, CutSense sense);

    private static ArcOp ClearingOp(
        ArcForest stock, Loop guide, EngagementSolution engagement, double cutterRadius, double feed, CutSense sense) =>
        new ArcOp.Adaptive(stock, Some(guide), cutterRadius, engagement.Radial, engagement.Advance, feed, sense);

    private static ArcOp TrochoidOp(
        ArcForest stock, Loop guide, EngagementSolution engagement, double cutterRadius, double feed, CutSense sense) =>
        new ArcOp.Adaptive(
            stock, Some(guide), cutterRadius, double.Min(engagement.Radial, cutterRadius), engagement.Advance, feed, sense);

    private static ArcOp PeelOp(
        ArcForest stock, Loop guide, EngagementSolution engagement, double cutterRadius, double feed, CutSense sense) =>
        new ArcOp.Adaptive(stock, Some(guide), cutterRadius, engagement.Advance, engagement.Advance, feed, sense);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record EngagementSolution(
    HashMap<EngagementLimit, double> Ceilings,
    EngagementLimit BindingRadial,
    double Radial,
    EngagementLimit BindingAdvance,
    double Advance);

public sealed class SkeletonTopology {
    private SkeletonTopology(
        Arr<(int From, int To)> edges,
        Arr<int> components,
        int componentCount,
        int duplicateEdges,
        BidirectionalGraph<int, SEdge<int>> walked) =>
        (Edges, Components, ComponentCount, DuplicateEdges, Walked) =
            (edges, components, componentCount, duplicateEdges, walked);

    public Arr<(int From, int To)> Edges { get; }
    public Arr<int> Components { get; }
    public int ComponentCount { get; }
    public int DuplicateEdges { get; }

    internal BidirectionalGraph<int, SEdge<int>> Walked { get; }

    public Seq<int> NodesOf(int component) =>
        Range(0, Components.Count).ToSeq().Filter(index => Components[index] == component);

    public bool Isolated(int node) => Walked.OutDegree(node) == 0;

    internal static SkeletonTopology Of(SkeletonGraph source) {
        int count = source.Nodes.Count;
        BidirectionalGraph<int, SEdge<int>> walked = new(allowParallelEdges: false);
        walked.AddVertexRange(Enumerable.Range(0, count));
        (int Duplicates, Arr<(int From, int To)> Edges) folded = source.Arcs.Fold(
            (Duplicates: 0, Edges: Arr<(int From, int To)>()),
            (state, arc) => arc.From >= 0 && arc.From < count && arc.To >= 0 && arc.To < count && arc.From != arc.To
                ? Both(walked, arc.From, arc.To)
                    ? (state.Duplicates, state.Edges.Add((arc.From, arc.To)))
                    : (state.Duplicates + 1, state.Edges)
                : state);
        Dictionary<int, int> labels = [];
        int components = walked.WeaklyConnectedComponents(labels);
        return new SkeletonTopology(
            folded.Edges,
            toSeq(Enumerable.Range(0, count).Select(index => labels.GetValueOrDefault(index, -1))).ToArr(),
            components,
            folded.Duplicates,
            walked);
    }

    private static bool Both(BidirectionalGraph<int, SEdge<int>> graph, int from, int to) =>
        graph.AddEdge(new SEdge<int>(from, to)) & graph.AddEdge(new SEdge<int>(to, from));
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
        Validate(stock, graph, cutter, engagement, sense, strategy, modality, SkeletonTopology.Of(graph), measuredLoad,
            out SkeletonDemand demand).Admitted(demand);

    [BoundaryAdapter]
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
        ref Option<(LoadWindow Window, Instant EvaluatedAt)> measuredLoad) {
        if (topology.Components.Count != graph.Nodes.Count
            || !Skeleton.Facts(stock, graph, cutter, engagement, topology).IsEmpty)
            validationError = new ValidationError("skeleton:topology");
    }
}

public sealed record SkeletonPass(
    int Component,
    Seq<int> OriginEdges,
    Seq<CutElement> Elements,
    EngagementSolution Engagement,
    double ChannelClearance,
    int NodeCount,
    int ArcCount,
    Seq<int> Visit,
    int TreeEdges);

public sealed record SkeletonWalk(
    Seq<SkeletonPass> Passes,
    Arr<int> Components,
    int ComponentCount,
    int NodeCount,
    int ArcCount,
    double CutterRadius) {
    public Seq<CutElement> Elements => Passes.Bind(static pass => pass.Elements);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Skeleton {
    public static Fin<SkeletonWalk> Walk(SkeletonDemand demand, Option<InstrumentSet> set = default) =>
        from scallop in ToleranceSpec.Apply(new ToleranceRequest.Scallop(demand.Engagement.Finish.Roughness, demand.Cutter))
        from budget in demand.Engagement.Budget is ProcessBudget.Subtractive subtractive
            ? Fin.Succ(subtractive)
            : Fin.Fail<ProcessBudget.Subtractive>(new KernelFault.InvalidValue("skeleton", "skeleton:budget"))
        let cutterRadius = demand.Cutter.Diameter / 2.0
        from passes in Range(0, demand.Topology.ComponentCount).ToSeq()
            .Map(component => Component(demand, budget, cutterRadius, scallop.StepMm, component))
            .TraverseM(identity)
            .As()
        let walked = new SkeletonWalk(
            passes,
            demand.Topology.Components,
            demand.Topology.ComponentCount,
            demand.Graph.Nodes.Count,
            demand.Graph.Arcs.Count,
            cutterRadius)
        from _ in set.Steps((EnginePhase.Nodes, walked.NodeCount), (EnginePhase.Arcs, walked.ArcCount), (EnginePhase.Passes, walked.Passes.Count))
        select walked;

    private static Fin<SkeletonPass> Component(
        SkeletonDemand demand,
        ProcessBudget.Subtractive budget,
        double cutterRadius,
        double scallop,
        int component) =>
        from nodes in demand.Topology.NodesOf(component) is { IsEmpty: false } members
            ? Fin.Succ(members)
            : Fin.Fail<Seq<int>>(new GeometryFault.DegenerateInput(Kind.Curve, component, $"skeleton:component-{component}:empty"))
        from arcs in demand.Graph.Arcs.Filter(arc => demand.Topology.Components[arc.From] == component) is { IsEmpty: false } spans
            ? Fin.Succ(spans)
            : Fin.Fail<Seq<SkeletonArc>>(new GeometryFault.DegenerateInput(Kind.Curve, component, $"skeleton:component-{component}:arcs"))
        let origins = toSeq(arcs.Map(static arc => arc.OriginEdge).Distinct().Order())
        let spans = arcs.Map(arc =>
            double.Min(demand.Graph.Nodes[arc.From].Radius, demand.Graph.Nodes[arc.To].Radius) - cutterRadius)
        let clearance = spans.Fold(spans[0], double.Min)
        from engagement in EngagementLimit.Solve(new EngagementInputs(
            cutterRadius, clearance, demand.Engagement.Finish.TargetAngle, scallop, budget, demand.MeasuredLoad))
        from walked in Chain(demand.Graph, demand.Topology, nodes, demand.Stock.Tolerance, component)
        from trace in ArcAlgebra.Apply(demand.Strategy.Operation(
            demand.Stock, walked.Guide, engagement, cutterRadius, budget.FeedRate, demand.Sense))
        from motion in trace is ArcTrace.Motion moved
            ? Fin.Succ(moved.Evidence)
            : Fin.Fail<ArcMotionEvidence>(new KernelFault.InvalidValue("skeleton", $"skeleton:component-{component}:arc-trace"))
        from elements in Elements(demand, origins, component, motion.Moves)
        select new SkeletonPass(
            component, origins, elements, engagement, clearance, nodes.Count, arcs.Count, walked.Visit, walked.TreeEdges);

    internal static Seq<Error> Facts(
        ArcForest stock,
        SkeletonGraph graph,
        CutterForm cutter,
        EngagementPolicy engagement,
        SkeletonTopology topology) =>
        (cutter.Diameter / 2.0).Apply(cutterRadius =>
            DemandFacts(stock, graph, engagement, topology)
            + NodeFacts(graph, cutterRadius, topology)
            + ArcFacts(graph, cutterRadius));

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
            (Ok: engagement.Budget is ProcessBudget.Subtractive, Axis: "budget")));

    private static Seq<Error> NodeFacts(SkeletonGraph graph, double cutterRadius, SkeletonTopology topology) =>
        graph.Nodes
            .Map((node, index) => Axes(index, Seq(
                (Ok: TensorPrimitives.IsFiniteAll<double>([node.At.X, node.At.Y, node.At.Z, node.Radius]), Axis: "finite"),
                (Ok: node.At.IsValid, Axis: "point"),
                (Ok: node.Radius > cutterRadius, Axis: "clearance"),
                (Ok: !topology.Isolated(index), Axis: "isolated")), "node"))
            .Bind(static errors => errors);

    private static Seq<Error> ArcFacts(SkeletonGraph graph, double cutterRadius) =>
        graph.Arcs
            .Map((arc, index) => (
                    Endpoints: arc.From >= 0 && arc.From < graph.Nodes.Count
                        && arc.To >= 0 && arc.To < graph.Nodes.Count,
                    Distinct: arc.From != arc.To)
                .Apply(state => (state.Endpoints && state.Distinct
                        ? Some((
                            Length: graph.Nodes[arc.From].At.DistanceTo(graph.Nodes[arc.To].At),
                            Clearance: double.Min(graph.Nodes[arc.From].Radius, graph.Nodes[arc.To].Radius) - cutterRadius))
                        : Option<(double Length, double Clearance)>.None)
                    .Apply(span => Axes(index, Seq(
                        (Ok: state.Endpoints, Axis: "endpoint"),
                        (Ok: state.Distinct, Axis: "self-loop"),
                        (Ok: arc.OriginEdge >= 0, Axis: "origin-edge"),
                        (Ok: span.ForAll(static value => double.IsFinite(value.Length) && value.Length > 0.0), Axis: "length"),
                        (Ok: span.ForAll(static value => double.IsFinite(value.Clearance) && value.Clearance > 0.0), Axis: "channel")), "arc"))))
            .Bind(static errors => errors);

    private static Seq<Error> Axes(int index, Seq<(bool Ok, string Axis)> axes, string owner = "") =>
        axes.Choose(fact => fact.Ok
            ? Option<Error>.None
            : Some(new GeometryFault.DegenerateInput(Kind.Curve, index < 0 ? Option<int>.None : index, index < 0
                ? $"skeleton:{fact.Axis}"
                : $"skeleton:{owner}-{index}:{fact.Axis}")));

    private static Fin<(Loop Guide, Seq<int> Visit, int TreeEdges)> Chain(
        SkeletonGraph graph,
        SkeletonTopology topology,
        Seq<int> nodes,
        Context tolerance,
        int component) =>
        nodes.Fold(
                Option<int>.None,
                (best, index) => best
                    .Filter(held => graph.Nodes[held].Radius >= graph.Nodes[index].Radius)
                    .Match(Some: static held => Some(held), None: () => Some(index)))
            .ToFin(new GeometryFault.DegenerateInput(Kind.Curve, component, $"skeleton:component-{component}:seed"))
            .Map(seed => Descend(topology, seed, nodes.Count))
            .Bind(walk => walk.Order.Count >= 2
                ? Loop.Admit(walk.Order.Map(index => graph.Nodes[index].At).ToArr(), closed: false, Arr<double>(), tolerance)
                    .Map(guide => (Guide: guide, walk.Visit, walk.TreeEdges))
                : Fin.Fail<(Loop, Seq<int>, int)>(
                    new GeometryFault.DegenerateInput(Kind.Curve, component, $"skeleton:component-{component}:chain")));

    private static (Seq<int> Order, Seq<int> Visit, int TreeEdges) Descend(SkeletonTopology topology, int seed, int bound) {
        DepthFirstSearchAlgorithm<int, SEdge<int>> search = new(
            host: null,
            topology.Walked,
            new Dictionary<int, GraphColor>(),
            static edges => edges.OrderBy(static edge => edge.Target));
        EdgeRecorderObserver<int, SEdge<int>> tree = new();
        VertexPredecessorRecorderObserver<int, SEdge<int>> parents = new();
        using (tree.Attach(search))
        using (parents.Attach(search))
            search.Compute(seed);
        Seq<SEdge<int>> edges = toSeq(tree.Edges);
        HashMap<int, int> parent = toHashMap(parents.VerticesPredecessors.Select(static row => (row.Key, row.Value.Source)));
        return (
            Order: edges.Fold(
                (Cursor: seed, Nodes: Seq(seed)),
                (state, edge) => (
                    edge.Target,
                    state.Nodes + Ascent(parent, state.Cursor, edge.Source, bound).Add(edge.Target))).Nodes,
            Visit: Seq(seed) + edges.Map(static edge => edge.Target),
            TreeEdges: edges.Count);
    }

    private static Seq<int> Ascent(HashMap<int, int> parents, int from, int to, int bound) =>
        Range(0, bound).ToSeq()
            .Fold(
                (Cursor: from, Path: Seq<int>()),
                (state, _) => state.Cursor == to
                    ? state
                    : parents.Find(state.Cursor).Match(
                        Some: parent => (parent, state.Path.Add(parent)),
                        None: () => (to, state.Path)))
            .Path;

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
            CutSignature.Of(demand.Strategy.Cut, ToolKey(demand), demand.Engagement.Route.WorkOffset, demand.Cutter, path)))
        from element in CutElement.Admit(
            key,
            ToolKey(demand),
            demand.Engagement.Route.WorkOffset,
            new EntryFamily.Fixed(ElementVariant.Of(key, path, demand.Modality)))
        select element;

    private static string ToolKey(SkeletonDemand demand) =>
        demand.Cutter.Evidence.Map(static evidence => evidence.ToolId).IfNone(demand.Cutter.Family.Key);
}

public static class SkeletonBench {
    private const int Channels = 3;
    private const int Spans = 32;
    private const double SpacingMm = 4.0;
    private const double PitchMm = 40.0;

    public static Fin<SkeletonDemand> Workload(CutterForm cutter, EngagementPolicy engagement) =>
        from tolerance in Context.Millimeters().ToFin()
        from boundary in Loop.Admit(
            Arr(new Point3d(-PitchMm, -PitchMm, 0.0), new Point3d((Spans * SpacingMm) + PitchMm, -PitchMm, 0.0),
                new Point3d((Spans * SpacingMm) + PitchMm, Channels * PitchMm, 0.0), new Point3d(-PitchMm, Channels * PitchMm, 0.0)),
            true, [], tolerance)
        from stock in ArcForest.Admit(Seq(boundary), tolerance, boundary.Plane)
        from demand in SkeletonDemand.Admit(stock, Channel(cutter.Diameter * 0.5), cutter, engagement,
            CutSense.Climb, WalkStrategy.Clearing, ProcessModality.Subtractive)
        select demand;

    public static Fin<SkeletonWalk> Run(SkeletonDemand demand) => Skeleton.Walk(demand);

    private static SkeletonGraph Channel(double cutterRadius) => new(
        Range(0, Channels * Spans).ToSeq().Map(node => new ClearanceNode(
            new Point3d((node % Spans) * SpacingMm, (node / Spans) * PitchMm, 0.0),
            cutterRadius + 0.5 + (0.25 * (node % 5)) + (node / Spans), node)),
        Range(0, Channels * Spans).ToSeq().Filter(static node => node % Spans != Spans - 1)
            .Map(node => new SkeletonArc(node, node + 1, node)));
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

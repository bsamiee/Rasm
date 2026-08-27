# [PERSISTENCE_QUERY_TOPOLOGY]

Rasm.Persistence answers authoritative topology synchronously from the ONE kind-filtered `QuikGraph` view the `Rasm.Element` boundary memoizes per read snapshot. `Graph/element#ELEMENT_GRAPH` owns the whole view vocabulary — `EdgeFilter` the kind roster, `EdgeOrientation` the leg projection, `TypedEdge` the relationship-carrying edge, and `ElementGraph.View(filter, orientation)` the demand-built memo — so this lane declares none of it and re-derives no incidence. `TopologyView` is the MODEL-SCOPED owner: it binds a `ModelId` to a snapshot, lifts view-local vertices into the model-qualified selection currency, and delegates every view read to the boundary. `ProjectView` composes the per-model views with the durable `ModelLink` rows into the one federated multi-graph over `SetKey` vertices — the capability the boundary does not carry, because a cross-model edge is a Persistence coordination row and never an in-model `Relationship`. Every traversal composes `QuikGraph.AlgorithmExtensions`; no second incidence index and no hand-rolled graph walk exists. `TopologyQuery` owns requests, `TopologyResult` owns results, and `TopologyFault` owns rejection. `Rasm.Bim` consumes results by reference through the topology-to-cache read chain, and no Bim type crosses down.

## [01]-[INDEX]

- [02]-[GRAPH_TOPOLOGY]: boundary view vocabulary this lane composes, `TopologyView` the model-scoped snapshot owner, and the `ProjectView` federated multi-graph lifting the per-model views over the durable `ModelLink` rows.
- [03]-[TRAVERSAL]: `TopologyQuery` request family, the `TopologyResult` typed result, the `TopologyFault` band, and the `Traversals` static surface composing the `AlgorithmExtensions` facade — containment ancestry/descent, connection adjacency, void resolution, nearest-common-container, shortest path, components, islands, topological order, anchors, and cycle detection — every result a `KeySelection`.

## [02]-[GRAPH_TOPOLOGY]

- Owner: `TopologyView` binds a model to a snapshot, carrying its `ModelId`, that boundary snapshot, and its content identity; `ProjectView` the federation-altitude composition — `ProjectEdge` the `SetKey`-vertexed edge and `ProjectTie` its two-kind payload, one lifted in-model relationship or one durable `ModelLink` row. View vocabulary itself belongs to the BOUNDARY: `EdgeFilter`, `EdgeOrientation`, `TypedEdge`, and the `(filter, orientation)`-keyed `ElementGraph.View` memo all arrive from `Rasm.Element` `Graph/element#ELEMENT_GRAPH`, and this page declares no row of any of them.
- Cases: the boundary `EdgeFilter` rows this lane reads are `All` (every edge — the full reachability/cycle/island graph), `Composition` (`Compose` of any sub-kind), `Containment` (the narrow `IfcRelContainedInSpatialStructure` element→placement edge a pure storey-membership query reads), `Spatial` (`Contain | Aggregate` — the FULL IFC spatial-structure tree the ancestry/descent/LCA/anchors walks climb, the `IfcRelAggregates` site→building→storey decomposition the storey→element containment hangs off), `Connection` (MEP/path adjacency), `Void` (host→feature opening resolution), and `Assignment` (group/system/type membership); `ProjectTie` is `Relation | Link`.
- Entry: `TopologyView.Of(ModelId, ElementGraph)` seats a model's view and `View(filter, orientation)` reads the boundary memo; `Key(NodeId)` lifts a view-local vertex into the model-qualified `SetKey` and `Scope` projects the single-model roster a result carries; `Advance(GraphDelta)` validates the next frozen snapshot through `ElementGraph.Apply` and re-seats the model onto it; `ProjectView.Of(Seq<TopologyView>, Seq<ModelLink>)` seats the federated view, its `Expand` is the one-hop delegate the selection `Closure` fold threads, and its `Scope` projects the member roster a caller hands to `Evaluate`.
- Auto: the view is built ONCE per read snapshot AT THE BOUNDARY — the live authoring/delta path uses the boundary's `TrackingHashMap` HAMT structural-sharing form (`Graph/delta`, O(log n) edits) and the boundary freezes the incidence index, the node map, and the demand-built `(EdgeFilter, EdgeOrientation)` view cache at the read-snapshot boundary (`Graph/element#ELEMENT_GRAPH` `Of`/`View`/`Topology`), so this lane NEVER re-derives incidence and never mints a second view structure — a degree read goes through `graph.EdgesAt(node)` and a kind-scoped walk through `graph.View(filter, orientation)`; `Advance` re-seats the model onto the admitted next snapshot, whose own view cache re-materializes on demand; `ProjectView` lifts each per-model view off that memo and adds only the durable link rows, an undirected `LinkKind` row contributing both orientations.
- Law: composing the boundary view REPLACES a per-model incremental patcher with a per-snapshot demand memo. NAMED LOSS: `Advance`'s `O(delta)` clone-and-patch of every already-materialized filter/orientation view, which re-used the previous snapshot's edges instead of re-scanning. WITNESS: `Advance` is now `Graph.Apply(delta).Map(next => Of(Model, next))`, and the next snapshot's first `View(filter, orientation)` pays one `O(V+E)` scan per DEMANDED row pair — a second incidence structure keyed to this lane is exactly what the boundary's own ban names, and a patcher obliged to stay bit-identical to the boundary builder across every new `EdgeFilter` row is the drift the memo deletes.
- Law: the boundary legs are RICHER than the endpoint pair this lane used to project. `EdgeOrientation.Legs` returns each admitted edge's `DirectedPairs` — a binary edge one leg, a realized `Connect` the two legs `from→realizing→to`, a `Generic` edge one leg per roster member — where the deleted local `Project` read `edge.Endpoints` and returned exactly one. CAPABILITY GAINED: reachability now traverses THROUGH a realizing intermediary and reaches every n-ary participant, so a connection walk that previously stopped at a realizing element crosses it, and the `Composition` filter row the local roster lacked is available with no edit here.
- Packages: QuikGraph (`BidirectionalGraph`/`IEdge<TVertex>`/`AddVerticesAndEdge`/`AddVertexRange` — the federated container alone; every per-model container is the boundary's), Rasm.Element (`Graph/element#ELEMENT_GRAPH` `ElementGraph`/`EdgeFilter`/`EdgeOrientation`/`TypedEdge`/`View`/`EdgesAt`/`Apply`, `Relationship`, `NodeId`, `ContentAddress.OfGraph`), Rasm.Persistence (`Element/graph#STORE_HOOKS` `ModelId`/`ModelLink`/`LinkKind`; `Query/lane#ELEMENT_SET_ALGEBRA` `SetKey`/`SetScope`/`KeySelection`), Rasm (`Rasm.Domain` `FaultBand`/`[FaultCase]`/`Fault` — the fault floor), CommunityToolkit.HighPerformance, System.Collections.Frozen, BCL inbox.
- Growth: a new edge filter or orientation is one row AT THE BOUNDARY and zero edits here; a new cross-model relationship class reaches the federated view as one `LinkKind` row with zero edits here; zero new surface — an external graph database for authoritative topology, a per-read whole-edge scan, a local re-declaration of the boundary view vocabulary, or a SECOND incidence structure beside the one the boundary snapshot freezes is the deleted form, because the boundary owns incidence, the roster, and the per-model view, and `ProjectView` COMPOSES those views off the one memo — lifting edges, never re-deriving incidence — adding only the durable link rows no boundary incidence carries.
- Boundary: the in-process QuikGraph view is the default authoritative topology owner; Apache AGE remains an optional analytical projection. Boundary `EdgeFilter.Containment` admits only direct placement while spatial ancestry, descent, LCA, and anchors take `Spatial` to include `Aggregate` — the distinction is the boundary's row law, read here and never re-derived by an inline `is` test. `Advance` validates through `ElementGraph.Apply` and becomes no second graph-mutation owner. `ProjectView` takes link rows the durable read already validity-filtered — bitemporal selection stays the `Element/graph#STORE_HOOKS` reader's — and the in-model `Relationship` never widens to carry a cross-model edge: federation crosses ONLY on `ModelLink` rows, so the federated walk answers exactly what a coordination row declared. ROOTED spatial ancestry as a breadcrumb is Bim `Model/spatial` `SpatialStructure.Ancestry`'s alone; this lane answers the bounded reachable SET, which is a different question the multi-parent `Compose` graph can answer without a precedence law.

```csharp
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LanguageExt;
using QuikGraph;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Projection;
using Rasm.Element.Query;
using Thinktecture;
using Rasm.Persistence.Element;
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [MODELS] --------------------------------------------------------------------------
public sealed class TopologyView {
    public ModelId Model { get; }
    public ElementGraph Graph { get; }
    public UInt128 Address { get; }

    private TopologyView(ModelId model, ElementGraph graph) => (Model, Graph, Address) = (model, graph, ContentAddress.OfGraph(graph).ToValue());

    public static TopologyView Of(ModelId model, ElementGraph graph) => new(model, graph);

    public SetKey Key(NodeId node) => new(Model, node);

    public SetScope Scope => new(Seq(Model));

    public BidirectionalGraph<NodeId, TypedEdge> View(EdgeFilter filter, EdgeOrientation orientation) =>
        Graph.View(filter, orientation);

    public Fin<TopologyView> Advance(GraphDelta delta) => Graph.Apply(delta).Map(next => Of(Model, next));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectTie {
    private ProjectTie() { }
    public sealed record Relation(Relationship Edge) : ProjectTie;
    public sealed record Link(LinkKind Kind, Guid Id) : ProjectTie;
}

public readonly record struct ProjectEdge(SetKey Source, SetKey Target, ProjectTie Tie) : IEdge<SetKey>;

public sealed class ProjectView {
    private readonly ConcurrentDictionary<(EdgeFilter Filter, EdgeOrientation Orientation), BidirectionalGraph<SetKey, ProjectEdge>> cache = new();
    public Seq<TopologyView> Models { get; }
    public Seq<ModelLink> Links { get; }

    private ProjectView(Seq<TopologyView> models, Seq<ModelLink> links) => (Models, Links) = (models, links);

    public static ProjectView Of(Seq<TopologyView> models, Seq<ModelLink> links) => new(models, links);

    public SetScope Scope => new(Models.Map(static view => view.Model));

    public BidirectionalGraph<SetKey, ProjectEdge> View(EdgeFilter filter, EdgeOrientation orientation) =>
        cache.GetOrAdd((filter, orientation), (_, state) => Federated(state.Filter, state.Orientation), (Filter: filter, Orientation: orientation));

    BidirectionalGraph<SetKey, ProjectEdge> Federated(EdgeFilter filter, EdgeOrientation orientation) {
        BidirectionalGraph<SetKey, ProjectEdge> view = new(allowParallelEdges: true);
        Models.Iter(model => {
            BidirectionalGraph<NodeId, TypedEdge> local = model.View(filter, orientation);
            view.AddVertexRange(local.Vertices.Select(model.Key));
            local.Edges.Iter(edge => view.AddVerticesAndEdge(
                new ProjectEdge(model.Key(edge.Source), model.Key(edge.Target), new ProjectTie.Relation(edge.Edge))));
        });
        Links.Iter(link => {
            SetKey from = new(link.FromModel, link.FromNode);
            SetKey to = new(link.ToModel, link.ToNode);
            view.AddVerticesAndEdge(new ProjectEdge(from, to, new ProjectTie.Link(link.Kind, link.Id)));
            if (!link.Kind.Directed) { view.AddVerticesAndEdge(new ProjectEdge(to, from, new ProjectTie.Link(link.Kind, link.Id))); }
        });
        return view;
    }

    public Fin<Seq<SetKey>> Expand(Seq<SetKey> frontier) {
        BidirectionalGraph<SetKey, ProjectEdge> all = View(EdgeFilter.All, EdgeOrientation.Forward);
        return Fin.Succ(toSeq(frontier.Filter(all.ContainsVertex).Bind(key => toSeq(all.OutEdges().Select(static e => e.Target)))).Distinct());
    }
}
```

| [INDEX] | [POLICY]          | [VALUE]                                        | [BINDING]                                                        |
| :-----: | :---------------- | :--------------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | default topology  | in-process QuikGraph view                      | AGE demoted to optional self-hosted (`H5`)                       |
|  [02]   | view vocabulary   | boundary `EdgeFilter`/`EdgeOrientation`/`Edge` | declared at `Graph/element`; a local row is the deleted form     |
|  [03]   | incidence + memo  | boundary `EdgesAt` + `ElementGraph.View`       | NEVER re-derived here; a second index is the deleted form (`H3`) |
|  [04]   | model scoping     | `TopologyView.Key`/`Scope`                     | the one thing this lane adds over the boundary snapshot          |
|  [05]   | snapshot identity | `ContentAddress.OfGraph` (boundary hasher)     | a `GraphDelta` advances identity; never a second hasher          |
|  [06]   | delta advance     | `Graph.Apply` then re-seat                     | boundary memo re-materializes; no local patcher to drift         |
|  [07]   | federated build   | `ProjectView` over links                       | the ONE view this lane builds; per-model views are lifted        |

## [03]-[TRAVERSAL]

- Owner: `TopologyQuery` the `[Union]` traversal-request family; `TopologyResult` the typed result `[Union]`; `[FaultCase]`/`TopologyFault` the closed band the absent-root, cyclic-order, and cyclic-LCA rejections ride; `Traversals` the static surface composing the `AlgorithmExtensions` facade — containment ancestry/descent, direct placement, group/system membership, connection adjacency, void resolution, nearest-common-container, shortest path, strongly/weakly connected components, topological order, spatial-structure anchors, and cycle detection.
- Cases: `TopologyQuery` covers bounded reach, adjacency, containment, shortest path, partitions, ordering, cycles, and reduction. `TopologyResult` distinguishes `Common` from `Unrelated` and `Route` from `Disconnected`; no empty path or absent ancestor acts as a sentinel. `Pruned` carries redundant edge pairs in edge space.
- Entry: `Run` dispatches through the generated total `Switch`; rooted queries return `RootAbsent`; LCA returns `Cyclic` for cycles and `NotForest` for multiple parents before invoking `OfflineLeastCommonAncestor`; cycle recovery includes singleton self-loops.
- Auto: spatial ancestry runs `TreeBreadthFirstSearch` over the ascending `Spatial` view and descent over the forward view. Connection adjacency unions `OutEdges` and `InEdges` because `IfcRelConnectsElements` is an undirected join. Placement uses ascending `Containment`; membership uses ascending `Assignment`; void resolution uses forward `Void`. LCA folds `OfflineLeastCommonAncestor` over every forest root after an acyclicity gate. Shortest path uses unit-weight `ShortestPathsDijkstra`; metric routing remains in the `pgrouting` lane. Components, islands, and cycles share ONE partition body over `StronglyConnectedComponents`/`WeaklyConnectedComponents`; order uses gated `SourceFirstTopologicalSort`; redundancy diffs the spatial view against `ComputeTransitiveReduction`. Every reachability result projects to `KeySelection` under the view's own model scope.
- Law: three component reads spelled their own label buffer, labeller call, and grouping apiece. `Grouped` folds them into one body differing by two arguments — which labeller runs, and which partitions survive. That buffer stays a plain `Dictionary` because the QuikGraph labellers take a mutable `IDictionary<TVertex,int>` OUT-PARAMETER by contract; freezing a buffer filled once and enumerated once buys a build and no lookup, so the mutability here is the library's shape and the duplication was the actual defect.
- Packages: QuikGraph (`AlgorithmExtensions.TreeBreadthFirstSearch`/`OfflineLeastCommonAncestor`/`ShortestPathsDijkstra`/`StronglyConnectedComponents`/`WeaklyConnectedComponents`/`SourceFirstTopologicalSort`/`IsDirectedAcyclicGraph`/`ComputeTransitiveReduction`/`Roots`/`Sinks`, `BidirectionalGraph.OutEdges`/`InEdges`/`InDegree`/`ContainsEdge`, `SEquatableEdge`/`TryFunc`), Rasm.Element (the boundary view vocabulary + `Query/predicate#PREDICATE_ALGEBRA` `WalkDepth`), Rasm.Persistence (`Query/lane#ELEMENT_SET_ALGEBRA` `KeySelection`/`SetKey`/`SetScope`/`SelectionFault` — the admitted bounded-depth carrier and selection currency), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new traversal is one `TopologyQuery` case carrying one `AlgorithmExtensions` composition over the matching boundary `EdgeFilter` view; zero new surface — a hand-rolled BFS/DFS, a recursive ltree CTE for in-memory ancestry, a second path solver, or a silent empty-result fallback is the deleted form because QuikGraph owns the graph algorithms, every result is a `KeySelection`, and an absent root yields the typed band.
- Boundary: every traversal composes `AlgorithmExtensions`; generated `query.Switch(...)` dispatch remains exhaustive. `Rooted` and `Paired` convert unknown endpoints to `TopologyFault.RootAbsent`, while `IsDirectedAcyclicGraph` gates order, LCA, and reduction. `Placement`, `Members`, `Void`, and symmetric connection adjacency make every boundary filter row this lane reads operational. Topological distance remains in-process, metric distance remains in `pgrouting`, and both return `KeySelection`-compatible keys.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TopologyQuery {
    private TopologyQuery() { }
    public sealed record Ancestry(SetKey Node, WalkDepth Depth) : TopologyQuery;
    public sealed record Descent(SetKey Node, WalkDepth Depth) : TopologyQuery;
    public sealed record Neighbors(SetKey Node, Neighborhood Kind) : TopologyQuery;
    public sealed record Ancestor(SetKey Left, SetKey Right) : TopologyQuery;
    public sealed record Path(SetKey From, SetKey To) : TopologyQuery;
    public sealed record Components : TopologyQuery;
    public sealed record Islands : TopologyQuery;
    public sealed record Anchors : TopologyQuery;
    public sealed record Order : TopologyQuery;
    public sealed record Cycles : TopologyQuery;
    public sealed record Redundant : TopologyQuery;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class Neighborhood {
    public static readonly Neighborhood Connected = new("connected", EdgeFilter.Connection, EdgeOrientation.Forward, symmetric: true);
    public static readonly Neighborhood Openings = new("openings", EdgeFilter.Void, EdgeOrientation.Forward, symmetric: false);
    public static readonly Neighborhood Placement = new("placement", EdgeFilter.Containment, EdgeOrientation.Ascending, symmetric: false);
    public static readonly Neighborhood Members = new("members", EdgeFilter.Assignment, EdgeOrientation.Ascending, symmetric: false);
    public EdgeFilter Filter { get; }
    public EdgeOrientation Orientation { get; }
    public bool Symmetric { get; }
    private Neighborhood(string key, EdgeFilter filter, EdgeOrientation orientation, bool symmetric) : this(key) =>
        (Filter, Orientation, Symmetric) = (filter, orientation, symmetric);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TopologyResult {
    private TopologyResult() { }
    public sealed record Reached(KeySelection Set) : TopologyResult;
    public sealed record Route(Seq<SetKey> Path, double Cost) : TopologyResult;
    public sealed record Common(SetKey Ancestor) : TopologyResult;
    public sealed record Unrelated(SetKey Left, SetKey Right) : TopologyResult;
    public sealed record Disconnected(SetKey From, SetKey To) : TopologyResult;
    public sealed record Partitions(Seq<KeySelection> Components) : TopologyResult;
    public sealed record Ordered(Seq<SetKey> Topological) : TopologyResult;
    public sealed record Cyclic(Seq<KeySelection> Cycles) : TopologyResult;
    public sealed record Pruned(Seq<(SetKey From, SetKey To)> Redundant) : TopologyResult;
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TopologyFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.StoreTopology;
    private TopologyFault() { }

    [FaultCase(0)]
    public sealed partial record RootAbsent(string Detail) : TopologyFault();
    [FaultCase(1)]
    public sealed partial record Cyclic(string Detail) : TopologyFault();
    [FaultCase(2)]
    public sealed partial record NotForest(string Node) : TopologyFault();

    public override string Message => Switch(
        rootAbsent: static c => $"<topology-root-absent:{c.Detail}>",
        cyclic:     static c => $"<topology-cyclic-order:{c.Detail}>",
        notForest:  static c => $"<topology-multiple-parents:{c.Node}>");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Traversals {
    public static Fin<TopologyResult> Run(TopologyView view, TopologyQuery query) => query.Switch(
        ancestry:   a => Rooted(view, a.Node, () => new TopologyResult.Reached(Walk(view, EdgeFilter.Spatial, EdgeOrientation.Ascending, a.Node.Node, a.Depth.ToValue()))),
        descent:    d => Rooted(view, d.Node, () => new TopologyResult.Reached(Walk(view, EdgeFilter.Spatial, EdgeOrientation.Forward, d.Node.Node, d.Depth.ToValue()))),
        neighbors:  n => Rooted(view, n.Node, () => new TopologyResult.Reached(Neighbors(view, n.Node.Node, n.Kind))),
        ancestor:   a => Paired(view, a.Left, a.Right, () => Lca(view, a.Left.Node, a.Right.Node)),
        path:       p => Paired(view, p.From, p.To, () => Fin.Succ(Shortest(view, p.From.Node, p.To.Node))),
        components: _ => Fin.Succ<TopologyResult>(new TopologyResult.Partitions(Components(view))),
        islands:    _ => Fin.Succ<TopologyResult>(new TopologyResult.Partitions(Islands(view))),
        anchors:    _ => Fin.Succ<TopologyResult>(new TopologyResult.Reached(Anchors(view))),
        order:      _ => Topological(view),
        cycles:     _ => Fin.Succ<TopologyResult>(new TopologyResult.Cyclic(Cycles(view, view.View(EdgeFilter.All, EdgeOrientation.Forward)))),
        redundant:  _ => Reduction(view));

    static Fin<TopologyResult> Rooted(TopologyView view, SetKey root, Func<TopologyResult> run) =>
        root.Model == view.Model && view.Graph.Nodes.ContainsKey(root.Node)
            ? Fin.Succ(run())
            : Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(root.Node.ToValue()));

    static Fin<TopologyResult> Paired(TopologyView view, SetKey left, SetKey right, Func<Fin<TopologyResult>> run) =>
        left.Model != view.Model || !view.Graph.Nodes.ContainsKey(left.Node) ? Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(left.Node.ToValue()))
        : right.Model != view.Model || !view.Graph.Nodes.ContainsKey(right.Node) ? Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(right.Node.ToValue()))
        : run();

    static KeySelection Walk(TopologyView view, EdgeFilter filter, EdgeOrientation orientation, NodeId root, int depth) {
        BidirectionalGraph<NodeId, TypedEdge> graph = view.View(filter, orientation);
        TryFunc<NodeId, IEnumerable<TypedEdge>> paths = graph.TreeBreadthFirstSearch(root);
        IEnumerable<NodeId> bounded = graph.Vertices.Where(v => v != root && paths(v, out IEnumerable<TypedEdge>? edges) && Enumerable.Count(edges) <= depth);
        return KeySelection.Of(toSeq(bounded.Select(view.Key)), view.Scope);
    }

    static KeySelection Adjacent(TopologyView owner, BidirectionalGraph<NodeId, TypedEdge> view, NodeId node) =>
        view.ContainsVertex(node)
            ? KeySelection.Of(toSeq(view.OutEdges(node).Select(e => owner.Key(e.Target))), owner.Scope)
            : KeySelection.Empty(owner.Scope);

    static KeySelection Incident(TopologyView owner, BidirectionalGraph<NodeId, TypedEdge> view, NodeId node) =>
        view.ContainsVertex(node)
            ? KeySelection.Of(toSeq(view.OutEdges(node).Select(e => owner.Key(e.Target)).Concat(view.InEdges(node).Select(e => owner.Key(e.Source)))), owner.Scope)
            : KeySelection.Empty(owner.Scope);

    static KeySelection Neighbors(TopologyView view, NodeId node, Neighborhood kind) {
        BidirectionalGraph<NodeId, TypedEdge> graph = view.View(kind.Filter, kind.Orientation);
        return kind.Symmetric ? Incident(view, graph, node) : Adjacent(view, graph, node);
    }

    static Fin<TopologyResult> Lca(TopologyView view, NodeId left, NodeId right) {
        BidirectionalGraph<NodeId, TypedEdge> tree = view.View(EdgeFilter.Spatial, EdgeOrientation.Forward);
        SEquatableEdge<NodeId> pair = new(left, right);
        if (!tree.IsDirectedAcyclicGraph()) {
            return Fin.Fail<TopologyResult>(new TopologyFault.Cyclic(Cycles(view, tree).Count.ToString(CultureInfo.InvariantCulture)));
        }
        Option<NodeId> ambiguous = toSeq(tree.Vertices).Find(vertex => tree.InDegree(vertex) > 1);
        return ambiguous.Match(
            Some: node => Fin.Fail<TopologyResult>(new TopologyFault.NotForest(node.ToValue())),
            None: () => toSeq(tree.Roots()).Fold(Option<NodeId>.None, (held, root) =>
                    held.IsSome
                        ? held
                        : ResolveAncestor(tree, root, pair))
                .Match<Fin<TopologyResult>>(
                    Some: ancestor => Fin.Succ<TopologyResult>(new TopologyResult.Common(view.Key(ancestor))),
                    None: () => Fin.Succ<TopologyResult>(new TopologyResult.Unrelated(view.Key(left), view.Key(right)))));
    }

    static Option<NodeId> ResolveAncestor(BidirectionalGraph<NodeId, TypedEdge> tree, NodeId root, SEquatableEdge<NodeId> pair) {
        TryFunc<SEquatableEdge<NodeId>, NodeId> resolve = tree.OfflineLeastCommonAncestor(root, [pair]);
        return resolve(pair, out NodeId ancestor) ? Some(ancestor) : None;
    }

    static TopologyResult Shortest(TopologyView view, NodeId from, NodeId to) {
        BidirectionalGraph<NodeId, TypedEdge> graph = view.View(EdgeFilter.All, EdgeOrientation.Forward);
        TryFunc<NodeId, IEnumerable<TypedEdge>> paths = graph.ShortestPathsDijkstra(static _ => 1.0, from);
        return paths(to, out IEnumerable<TypedEdge>? edges)
            ? new TopologyResult.Route(view.Key(from).Cons(toSeq(edges.Select(e => view.Key(e.Target)))), Enumerable.Count(edges))
            : new TopologyResult.Disconnected(view.Key(from), view.Key(to));
    }

    static Seq<KeySelection> Grouped(
        TopologyView owner, BidirectionalGraph<NodeId, TypedEdge> graph,
        Func<BidirectionalGraph<NodeId, TypedEdge>, IDictionary<NodeId, int>, int> label,
        Func<Seq<NodeId>, bool> keep) {
        Dictionary<NodeId, int> labels = [];
        ignore(label(graph, labels));
        return toSeq(labels.GroupBy(static row => row.Value).Select(static group => toSeq(group.Select(static row => row.Key))))
            .Filter(keep)
            .Map(members => KeySelection.Of(members.Map(owner.Key), owner.Scope));
    }

    static Seq<KeySelection> Components(TopologyView view) =>
        Grouped(view, view.View(EdgeFilter.All, EdgeOrientation.Forward),
            static (graph, labels) => graph.StronglyConnectedComponents(labels), static _ => true);

    static Seq<KeySelection> Islands(TopologyView view) =>
        Grouped(view, view.View(EdgeFilter.All, EdgeOrientation.Forward),
            static (graph, labels) => graph.WeaklyConnectedComponents(labels), static _ => true);

    static Seq<KeySelection> Cycles(TopologyView view, BidirectionalGraph<NodeId, TypedEdge> graph) =>
        Grouped(view, graph, static (g, labels) => g.StronglyConnectedComponents(labels),
            members => members.Count > 1 || members.Exists(node => graph.ContainsEdge(node, node)));

    static KeySelection Anchors(TopologyView view) {
        BidirectionalGraph<NodeId, TypedEdge> spatial = view.View(EdgeFilter.Spatial, EdgeOrientation.Forward);
        return KeySelection.Of(toSeq(spatial.Roots().Concat(spatial.Sinks()).Select(view.Key)), view.Scope);
    }

    static Fin<TopologyResult> Topological(TopologyView view) {
        BidirectionalGraph<NodeId, TypedEdge> graph = view.View(EdgeFilter.All, EdgeOrientation.Forward);
        return graph.IsDirectedAcyclicGraph()
            ? Fin.Succ<TopologyResult>(new TopologyResult.Ordered(toSeq(graph.SourceFirstTopologicalSort().Select(view.Key))))
            : Fin.Fail<TopologyResult>(new TopologyFault.Cyclic(Cycles(view, graph).Count.ToString(CultureInfo.InvariantCulture)));
    }

    static Fin<TopologyResult> Reduction(TopologyView view) {
        BidirectionalGraph<NodeId, TypedEdge> tree = view.View(EdgeFilter.Spatial, EdgeOrientation.Forward);
        if (!tree.IsDirectedAcyclicGraph()) {
            return Fin.Fail<TopologyResult>(new TopologyFault.Cyclic(Cycles(view, tree).Count.ToString(CultureInfo.InvariantCulture)));
        }
        BidirectionalGraph<NodeId, TypedEdge> reduced = tree.ComputeTransitiveReduction();
        return Fin.Succ<TopologyResult>(new TopologyResult.Pruned(
            toSeq(tree.Edges.Where(edge => !reduced.ContainsEdge(edge)).Select(edge => (view.Key(edge.Source), view.Key(edge.Target))))));
    }
}
```

| [INDEX] | [POLICY]             | [VALUE]                                        | [BINDING]                                    |
| :-----: | :------------------- | :--------------------------------------------- | :------------------------------------------- |
|  [01]   | algorithm owner      | QuikGraph `AlgorithmExtensions`                | no hand-rolled walk or recursive CTE         |
|  [02]   | dispatch             | generated `query.Switch(...)`                  | exhaustive; no silent `_` arm                |
|  [03]   | absent root          | `Rooted`/`Paired` → `TopologyFault.RootAbsent` | typed fault, never an empty sentinel         |
|  [04]   | result shape         | `KeySelection` under the view's model scope    | composes with the boundary selection algebra |
|  [05]   | component partition  | one `Grouped` body, two row arguments          | shared by components, islands, and cycles    |
|  [06]   | void resolution      | `Neighbors` under `Neighborhood.Openings`      | one polymorphic case, never a Resolve twin   |
|  [07]   | nearest container    | forest-gated `OfflineLeastCommonAncestor`      | cycles and multiple parents reject           |
|  [08]   | shortest path        | unit-weight `ShortestPathsDijkstra`            | metric routing belongs to `pgrouting`        |
|  [09]   | spatial walks        | boundary `Spatial` (`Contain` ∪ `Aggregate`)   | full-tree climb                              |
|  [10]   | connection adjacency | `OutEdges` ∪ `InEdges` over `Connection`       | symmetric read                               |
|  [11]   | placement / members  | `Containment` / `Assignment`                   | direct storey / reverse membership           |
|  [12]   | bounded depth        | boundary `WalkDepth`                           | no raw `int` enters `Walk`                   |
|  [13]   | redundancy           | DAG-gated `ComputeTransitiveReduction` diff    | redundant edges return typed `Pruned` pairs  |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

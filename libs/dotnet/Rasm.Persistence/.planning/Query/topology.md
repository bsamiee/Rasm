# [PERSISTENCE_QUERY_TOPOLOGY]

Rasm.Persistence answers authoritative topology synchronously from the ONE kind-filtered `QuikGraph` view the `Rasm.Element` seam memoizes per read snapshot. `Graph/element#ELEMENT_GRAPH` owns the whole view vocabulary — `EdgeFilter` the kind roster, `EdgeOrientation` the leg projection, `TypedEdge` the relationship-carrying edge, and `ElementGraph.View(filter, orientation)` the demand-built memo — so this lane declares none of it and re-derives no incidence. `TopologyView` is the MODEL-SCOPED owner: it binds a `ModelId` to a snapshot, lifts view-local vertices into the model-qualified selection currency, and delegates every view read to the seam. `ProjectView` composes the per-model views with the durable `ModelLink` rows into the one federated multi-graph over `SetKey` vertices — the capability the seam does not carry, because a cross-model edge is a Persistence coordination row and never an in-model `Relationship`. Every traversal composes `QuikGraph.AlgorithmExtensions`; no second incidence index and no hand-rolled graph walk exists. `TopologyQuery` owns requests, `TopologyResult` owns receipts, and `TopologyFault` owns rejection. `Rasm.Bim` consumes results by reference through the topology-to-cache read chain, and no Bim type crosses down.

## [01]-[INDEX]

- [02]-[GRAPH_TOPOLOGY]: seam view vocabulary this lane composes, `TopologyView` the model-scoped snapshot owner, and the `ProjectView` federated multi-graph lifting the per-model views over the durable `ModelLink` rows.
- [03]-[TRAVERSAL]: `TopologyQuery` request family, the `TopologyResult` typed receipt, the `TopologyFault` band, and the `Traversals` static surface composing the `AlgorithmExtensions` facade — containment ancestry/descent, connection adjacency, void resolution, nearest-common-container, shortest path, components, islands, topological order, anchors, and cycle detection — every result a `KeySelection`.

## [02]-[GRAPH_TOPOLOGY]

- Owner: `TopologyView` binds a model to a snapshot, carrying its `ModelId`, that seam snapshot, and its content identity; `ProjectView` the federation-altitude composition — `ProjectEdge` the `SetKey`-vertexed edge and `ProjectTie` its two-kind payload, one lifted seam relationship or one durable `ModelLink` row. View vocabulary itself belongs to the SEAM: `EdgeFilter`, `EdgeOrientation`, `TypedEdge`, and the `(filter, orientation)`-keyed `ElementGraph.View` memo all arrive from `Rasm.Element` `Graph/element#ELEMENT_GRAPH`, and this page declares no row of any of them.
- Cases: the seam `EdgeFilter` rows this lane reads are `All` (every edge — the full reachability/cycle/island graph), `Composition` (`Compose` of any sub-kind), `Containment` (the narrow `IfcRelContainedInSpatialStructure` element→placement edge a pure storey-membership query reads), `Spatial` (`Contain | Aggregate` — the FULL IFC spatial-structure tree the ancestry/descent/LCA/anchors walks climb, the `IfcRelAggregates` site→building→storey decomposition the storey→element containment hangs off), `Connection` (MEP/path adjacency), `Void` (host→feature opening resolution), and `Assignment` (group/system/type membership); `ProjectTie` is `Seam | Link`.
- Entry: `TopologyView.Of(ModelId, ElementGraph)` seats a model's view and `View(filter, orientation)` reads the seam memo; `Key(NodeId)` lifts a view-local vertex into the model-qualified `SetKey` and `Scope` projects the single-model roster a result carries; `Advance(GraphDelta, Op)` validates the next frozen snapshot through `ElementGraph.Apply` and re-seats the model onto it; `ProjectView.Of(Seq<TopologyView>, Seq<ModelLink>)` seats the federated view, its `Expand` is the one-hop delegate the selection `Closure` fold threads, and its `Scope` projects the member roster a caller hands to `Evaluate`.
- Auto: the view is built ONCE per read snapshot AT THE SEAM — the live authoring/delta path uses the seam's `TrackingHashMap` HAMT structural-sharing form (`Graph/delta`, O(log n) edits) and the seam freezes the incidence index, the node map, and the demand-built `(EdgeFilter, EdgeOrientation)` view cache at the read-snapshot boundary (`Graph/element#ELEMENT_GRAPH` `Of`/`View`/`Topology`), so this lane NEVER re-derives incidence and never mints a second view structure — a degree read goes through `graph.EdgesAt(node)` and a kind-scoped walk through `graph.View(filter, orientation)`; `Advance` re-seats the model onto the admitted next snapshot, whose own view cache re-materializes on demand; `ProjectView` lifts each per-model view off that memo and adds only the durable link rows, an undirected `LinkKind` row contributing both orientations.
- Law: composing the seam view REPLACES a per-model incremental patcher with a per-snapshot demand memo. NAMED LOSS: `Advance`'s `O(delta)` clone-and-patch of every already-materialized filter/orientation view, which re-used the previous snapshot's edges instead of re-scanning. WITNESS: `Advance` is now `Graph.Apply(delta, key).Map(next => Of(Model, next))`, and the next snapshot's first `View(filter, orientation)` pays one `O(V+E)` scan per DEMANDED row pair — a second incidence structure keyed to this lane is exactly what the seam's own ban names, and a patcher obliged to stay bit-identical to the seam builder across every new `EdgeFilter` row is the drift the memo deletes.
- Law: the seam legs are RICHER than the endpoint pair this lane used to project. `EdgeOrientation.Legs` returns each admitted edge's `DirectedPairs` — a binary edge one leg, a realized `Connect` the two legs `from→realizing→to`, a `Generic` edge one leg per roster member — where the deleted local `Project` read `edge.Endpoints` and returned exactly one. CAPABILITY GAINED: reachability now traverses THROUGH a realizing intermediary and reaches every n-ary participant, so a connection walk that previously stopped at a realizing element crosses it, and the `Composition` filter row the local roster lacked is available with no edit here.
- Receipt: a federated view build rides `store.topology.build` carrying the model count, the link count, and the filter key; per-model build and memo-hit receipts DELETE with the builder: this lane no longer distinguishes a seam build from a seam memo hit, so a receipt claiming one FORGES the other — a consumer wanting the view's size reads `VertexCount`/`EdgeCount` off the returned graph.
- Packages: QuikGraph (`BidirectionalGraph`/`IEdge<TVertex>`/`AddVerticesAndEdge`/`AddVertexRange` — the federated container alone; every per-model container is the seam's), Rasm.Element (`Graph/element#ELEMENT_GRAPH` `ElementGraph`/`EdgeFilter`/`EdgeOrientation`/`TypedEdge`/`View`/`EdgesAt`/`Apply`, `Relationship`, `NodeId`, `ContentAddress.OfGraph`), Rasm.Persistence (`Element/graph#STORE_RAIL` `ModelId`/`ModelLink`/`LinkKind`; `Query/lane#ELEMENT_SET_ALGEBRA` `SetKey`/`SetScope`/`KeySelection`), Rasm (`Rasm.Domain` `FaultBand`/`[FaultCase]`/`Fault` — the fault floor), CommunityToolkit.HighPerformance, System.Collections.Frozen, BCL inbox.
- Growth: a new edge filter or orientation is one row AT THE SEAM and zero edits here; a new cross-model relationship class reaches the federated view as one `LinkKind` row with zero edits here; zero new surface — an external graph database for authoritative topology, a per-read whole-edge scan, a local re-declaration of the seam view vocabulary, or a SECOND incidence structure beside the one the seam snapshot freezes is the deleted form, because the seam owns incidence, the roster, and the per-model view, and `ProjectView` COMPOSES those views off the one memo — lifting edges, never re-deriving incidence — adding only the durable link rows no seam incidence carries.
- Boundary: the in-process QuikGraph view is the default authoritative topology owner; Apache AGE remains an optional analytical projection. Seam `EdgeFilter.Containment` admits only direct placement while spatial ancestry, descent, LCA, and anchors take `Spatial` to include `Aggregate` — the distinction is the seam's row law, read here and never re-derived by an inline `is` test. `Advance` validates through `ElementGraph.Apply` and becomes no second graph-mutation owner. `ProjectView` takes link rows the durable read already validity-filtered — bitemporal selection stays the `Element/graph#STORE_RAIL` reader's — and the in-model seam `Relationship` never widens to carry a cross-model edge: federation crosses ONLY on `ModelLink` rows, so the federated walk answers exactly what a coordination row declared. ROOTED spatial ancestry as a breadcrumb is Bim `Model/spatial` `SpatialStructure.Ancestry`'s alone; this lane answers the bounded reachable SET, which is a different question the multi-parent `Compose` graph can answer without a precedence law.

```csharp signature
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LanguageExt;
using QuikGraph;
using Rasm.Domain;                                // FaultBand + [FaultCase]/Fault — the kernel registry and fault floor
using Rasm.Element.Graph;                         // ElementGraph/EdgeFilter/EdgeOrientation/TypedEdge — the ONE view vocabulary
using Rasm.Element.Projection;
using Rasm.Element.Query;                         // WalkDepth — the seam bounded-depth axis
using Thinktecture;
using Rasm.Persistence.Element;                   // ModelId/ModelLink/LinkKind — the stream and coordination vocabulary
using static LanguageExt.Prelude;

namespace Rasm.Persistence.Query;

// --- [MODELS] -----------------------------------------------------------------------------
// `TopologyView` binds a MODEL to a seam snapshot: it carries the owning `ModelId`, the snapshot, and the
// snapshot's content identity, lifts view-local vertices into the model-qualified selection currency, and
// DELEGATES every view read to the seam memo. It holds no cache of its own — the `(EdgeFilter,
// EdgeOrientation)` memo is `ElementGraph`'s, and a second one keyed here would have to stay bit-identical to
// bit-identical to the seam builder across every new filter row.
public sealed class TopologyView {
    public ModelId Model { get; }
    public ElementGraph Graph { get; }
    public UInt128 Address { get; }

    private TopologyView(ModelId model, ElementGraph graph) => (Model, Graph, Address) = (model, graph, ContentAddress.OfGraph(graph).Value);

    public static TopologyView Of(ModelId model, ElementGraph graph) => new(model, graph);

    public SetKey Key(NodeId node) => new(Model, node);

    // Single-model results carry a single-model scope, so every `KeySelection` this lane mints declares the
    // roster its members came from without the caller re-supplying one.
    public SetScope Scope => new(Seq(Model));

    public BidirectionalGraph<NodeId, TypedEdge> View(EdgeFilter filter, EdgeOrientation orientation) =>
        Graph.View(filter, orientation);

    // `Advance` admits the next frozen snapshot and re-seats the model onto it. The advanced snapshot carries a
    // fresh view memo the seam re-materializes per demanded row pair.
    public Fin<TopologyView> Advance(GraphDelta delta, Op key) => Graph.Apply(delta, key).Map(next => Of(Model, next));
}

// `ProjectTie` carries the federated edge: an in-model seam relationship LIFTED under its owning model, or a
// durable cross-model `ModelLink` row — the two tie kinds one project walk crosses with no second incidence structure.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProjectTie {
    private ProjectTie() { }
    public sealed record Seam(Relationship Edge) : ProjectTie;
    public sealed record Link(LinkKind Kind, Guid Id) : ProjectTie;
}

public readonly record struct ProjectEdge(SetKey Source, SetKey Target, ProjectTie Tie) : IEdge<SetKey>;

// `ProjectView` COMPOSES the per-model seam views and adds only the durable link rows — it re-derives no
// per-model incidence (each lifted edge set comes off the seam memo) and mints no second per-model structure,
// so the seam second-incidence ban holds whole at the federation altitude. This IS the one view build this
// lane still performs, because a cross-model edge is a Persistence coordination row no seam incidence carries.
// Callers supply link rows already validity-filtered at the durable read; an undirected `LinkKind` row
// adds both orientations. Identity is the per-model address roster, so a delta advancing any member re-keys it.
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
                new ProjectEdge(model.Key(edge.Source), model.Key(edge.Target), new ProjectTie.Seam(edge.Edge))));
        });
        Links.Iter(link => {
            SetKey from = new(link.FromModel, link.FromNode);
            SetKey to = new(link.ToModel, link.ToNode);
            view.AddVerticesAndEdge(new ProjectEdge(from, to, new ProjectTie.Link(link.Kind, link.Id)));
            if (!link.Kind.Directed) { view.AddVerticesAndEdge(new ProjectEdge(to, from, new ProjectTie.Link(link.Kind, link.Id))); }
        });
        return view;
    }

    // `Expand` is the one-hop expansion the `Query/lane#ELEMENT_SET_ALGEBRA` `Closure` fold threads as
    // `SetResolve.Expand`: out-neighbours over the full federated view, so a bounded walk crosses models exactly
    // where a durable link row carries it.
    public Fin<Seq<SetKey>> Expand(Seq<SetKey> frontier) {
        BidirectionalGraph<SetKey, ProjectEdge> all = View(EdgeFilter.All, EdgeOrientation.Forward);
        return Fin.Succ(toSeq(frontier.Filter(all.ContainsVertex).Bind(key => toSeq(all.OutEdges(key).Select(static e => e.Target)))).Distinct());
    }
}
```

| [INDEX] | [POLICY]          | [VALUE]                                    | [BINDING]                                                        |
| :-----: | :---------------- | :----------------------------------------- | :--------------------------------------------------------------- |
|  [01]   | default topology  | in-process QuikGraph view                  | AGE demoted to optional self-hosted (`H5`)                       |
|  [02]   | view vocabulary   | seam `EdgeFilter`/`EdgeOrientation`/`Edge` | declared at `Graph/element`; a local row is the deleted form     |
|  [03]   | incidence + memo  | seam `EdgesAt` + `ElementGraph.View`       | NEVER re-derived here; a second index is the deleted form (`H3`) |
|  [04]   | model scoping     | `TopologyView.Key`/`Scope`                 | the one thing this lane adds over the seam snapshot              |
|  [05]   | snapshot identity | `ContentAddress.OfGraph` (seam hasher)     | a `GraphDelta` advances identity; never a second hasher          |
|  [06]   | delta advance     | `Graph.Apply` then re-seat                 | seam memo re-materializes; no local patcher to drift             |
|  [07]   | federated build   | `ProjectView` over links                   | the ONE view this lane builds; per-model views are lifted        |

## [03]-[TRAVERSAL]

- Owner: `TopologyQuery` the `[Union]` traversal-request family; `TopologyResult` the typed result `[Union]`; `[FaultCase]`/`TopologyFault` the closed band the absent-root, cyclic-order, and cyclic-LCA rejections rail; `Traversals` the static surface composing the `AlgorithmExtensions` facade — containment ancestry/descent, direct placement, group/system membership, connection adjacency, void resolution, nearest-common-container, shortest path, strongly/weakly connected components, topological order, spatial-structure anchors, and cycle detection.
- Cases: `TopologyQuery` covers bounded reach, adjacency, containment, shortest path, partitions, ordering, cycles, and reduction. `TopologyResult` distinguishes `Common` from `Unrelated` and `Route` from `Disconnected`; no empty path or absent ancestor acts as a sentinel. `Pruned` carries redundant edge pairs in edge space.
- Entry: `Run` dispatches through the generated total `Switch`; rooted queries rail `RootAbsent`; LCA rails `Cyclic` for cycles and `NotForest` for multiple parents before invoking `OfflineLeastCommonAncestor`; cycle recovery includes singleton self-loops.
- Auto: spatial ancestry runs `TreeBreadthFirstSearch` over the ascending `Spatial` view and descent over the forward view. Connection adjacency unions `OutEdges` and `InEdges` because `IfcRelConnectsElements` is an undirected join. Placement uses ascending `Containment`; membership uses ascending `Assignment`; void resolution uses forward `Void`. LCA folds `OfflineLeastCommonAncestor` over every forest root after an acyclicity gate. Shortest path uses unit-weight `ShortestPathsDijkstra`; metric routing remains in the `pgrouting` lane. Components, islands, and cycles share ONE partition body over `StronglyConnectedComponents`/`WeaklyConnectedComponents`; order uses gated `SourceFirstTopologicalSort`; redundancy diffs the spatial view against `ComputeTransitiveReduction`. Every reachability result projects to `KeySelection` under the view's own model scope.
- Law: three component reads spelled their own label buffer, labeller call, and grouping apiece. `Grouped` folds them into one body differing by two arguments — which labeller runs, and which partitions survive. That buffer stays a plain `Dictionary` because the QuikGraph labellers take a mutable `IDictionary<TVertex,int>` OUT-PARAMETER by contract; freezing a buffer filled once and enumerated once buys a build and no lookup, so the mutability here is the library's shape and the duplication was the actual defect.
- Receipt: a traversal rides `store.topology.traverse` carrying the query case, the reached count, and the depth; a cycle detection rides `store.topology.cycle` carrying the cycle count; an absent-root rejection rides the `TopologyFault.RootAbsent` rail.
- Packages: QuikGraph (`AlgorithmExtensions.TreeBreadthFirstSearch`/`OfflineLeastCommonAncestor`/`ShortestPathsDijkstra`/`StronglyConnectedComponents`/`WeaklyConnectedComponents`/`SourceFirstTopologicalSort`/`IsDirectedAcyclicGraph`/`ComputeTransitiveReduction`/`Roots`/`Sinks`, `BidirectionalGraph.OutEdges`/`InEdges`/`InDegree`/`ContainsEdge`, `SEquatableEdge`/`TryFunc`), Rasm.Element (the seam view vocabulary + `Query/predicate#PREDICATE_ALGEBRA` `WalkDepth`), Rasm.Persistence (`Query/lane#ELEMENT_SET_ALGEBRA` `KeySelection`/`SetKey`/`SetScope`/`SelectionFault` — the admitted bounded-depth carrier and selection currency), LanguageExt.Core, Thinktecture.Runtime.Extensions, BCL inbox.
- Growth: a new traversal is one `TopologyQuery` case carrying one `AlgorithmExtensions` composition over the matching seam `EdgeFilter` view; zero new surface — a hand-rolled BFS/DFS, a recursive ltree CTE for in-memory ancestry, a second path solver, or a silent empty-result fallback is the deleted form because QuikGraph owns the graph algorithms, every result is a `KeySelection`, and an absent root rails the typed band.
- Boundary: every traversal composes `AlgorithmExtensions`; generated `query.Switch(...)` dispatch remains exhaustive. `Rooted` and `Paired` convert unknown endpoints to `TopologyFault.RootAbsent`, while `IsDirectedAcyclicGraph` gates order, LCA, and reduction. `Placement`, `Members`, `Void`, and symmetric connection adjacency make every seam filter row this lane reads operational. Topological distance remains in-process, metric distance remains in `pgrouting`, and both return `KeySelection`-compatible keys.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
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

// `Neighborhood` names one adjacency question as a row over the SEAM's filter and orientation vocabularies —
// pairing rides the row as data, so a new adjacency is one row and never a second dispatch arm.
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

// --- [ERRORS] ---------------------------------------------------------------------------
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

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Traversals {
    public static readonly Seq<StoreSlot> Slots = Seq(
        StoreSlot.Create("store.topology.build"), StoreSlot.Create("store.topology.traverse"),
        StoreSlot.Create("store.topology.cycle"));

    public static Fin<TopologyResult> Run(TopologyView view, TopologyQuery query) => query.Switch(
        ancestry:   a => Rooted(view, a.Node, () => new TopologyResult.Reached(Walk(view, EdgeFilter.Spatial, EdgeOrientation.Ascending, a.Node.Node, a.Depth.Value))),
        descent:    d => Rooted(view, d.Node, () => new TopologyResult.Reached(Walk(view, EdgeFilter.Spatial, EdgeOrientation.Forward, d.Node.Node, d.Depth.Value))),
        neighbors:  n => Rooted(view, n.Node, () => new TopologyResult.Reached(Neighbors(view, n.Node.Node, n.Kind))),
        ancestor:   a => Paired(view, a.Left, a.Right, () => Lca(view, a.Left.Node, a.Right.Node)),
        path:       p => Paired(view, p.From, p.To, () => Fin.Succ(Shortest(view, p.From.Node, p.To.Node))),
        components: _ => Fin.Succ<TopologyResult>(new TopologyResult.Partitions(Components(view))),
        islands:    _ => Fin.Succ<TopologyResult>(new TopologyResult.Partitions(Islands(view))),
        anchors:    _ => Fin.Succ<TopologyResult>(new TopologyResult.Reached(Anchors(view))),
        order:      _ => Topological(view),
        cycles:     _ => Fin.Succ<TopologyResult>(new TopologyResult.Cyclic(Cycles(view, view.View(EdgeFilter.All, EdgeOrientation.Forward)))),
        redundant:  _ => Reduction(view));

    // Rooted-query guard rails `RootAbsent` when a query names a foreign model or a node absent from the
    // snapshot, rather than returning a silent empty set — a wrong-model root IS absent from this view.
    static Fin<TopologyResult> Rooted(TopologyView view, SetKey root, Func<TopologyResult> run) =>
        root.Model == view.Model && view.Graph.Nodes.ContainsKey(root.Node)
            ? Fin.Succ(run())
            : Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(root.Node.Value));

    // `Paired` rails either absent endpoint before path or ancestor algorithms execute.
    // Its body retains its own `Fin` so LCA cycle and forest faults compose.
    static Fin<TopologyResult> Paired(TopologyView view, SetKey left, SetKey right, Func<Fin<TopologyResult>> run) =>
        left.Model != view.Model || !view.Graph.Nodes.ContainsKey(left.Node) ? Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(left.Node.Value))
        : right.Model != view.Model || !view.Graph.Nodes.ContainsKey(right.Node) ? Fin.Fail<TopologyResult>(new TopologyFault.RootAbsent(right.Node.Value))
        : run();

    // `TreeBreadthFirstSearch` supplies bounded reachability over a pre-oriented seam view; results lift through
    // `view.Key` into the model-qualified currency. Roots remain excluded.
    static KeySelection Walk(TopologyView view, EdgeFilter filter, EdgeOrientation orientation, NodeId root, int depth) {
        BidirectionalGraph<NodeId, TypedEdge> graph = view.View(filter, orientation);
        TryFunc<NodeId, IEnumerable<TypedEdge>> paths = graph.TreeBreadthFirstSearch(root);
        IEnumerable<NodeId> bounded = graph.Vertices.Where(v => v != root && paths(v, out IEnumerable<TypedEdge>? edges) && Enumerable.Count(edges) <= depth);
        return KeySelection.Of(toSeq(bounded.Select(view.Key)), view.Scope);
    }

    // DIRECTIONAL out-neighbour set resolves a Void host's openings (the Void edge is
    // directional Host->Feature, so the forward out-edges ARE the openings and an in-edge would be a different host).
    static KeySelection Adjacent(TopologyView owner, BidirectionalGraph<NodeId, TypedEdge> view, NodeId node) =>
        view.ContainsVertex(node)
            ? KeySelection.Of(toSeq(view.OutEdges(node).Select(e => owner.Key(e.Target))), owner.Scope)
            : KeySelection.Empty(owner.Scope);

    // Symmetric connection adjacency unions outgoing targets and incoming sources.
    // `BidirectionalGraph` supplies both directions without building a reversed view.
    static KeySelection Incident(TopologyView owner, BidirectionalGraph<NodeId, TypedEdge> view, NodeId node) =>
        view.ContainsVertex(node)
            ? KeySelection.Of(toSeq(view.OutEdges(node).Select(e => owner.Key(e.Target)).Concat(view.InEdges(node).Select(e => owner.Key(e.Source)))), owner.Scope)
            : KeySelection.Empty(owner.Scope);

    static KeySelection Neighbors(TopologyView view, NodeId node, Neighborhood kind) {
        BidirectionalGraph<NodeId, TypedEdge> graph = view.View(kind.Filter, kind.Orientation);
        return kind.Symmetric ? Incident(view, graph, node) : Adjacent(view, graph, node);
    }

    // Offline LCA runs only after cycle and multiple-parent gates prove a spatial forest.
    // Folding every root distinguishes unrelated trees from pairs under later federated roots.
    static Fin<TopologyResult> Lca(TopologyView view, NodeId left, NodeId right) {
        BidirectionalGraph<NodeId, TypedEdge> tree = view.View(EdgeFilter.Spatial, EdgeOrientation.Forward);
        SEquatableEdge<NodeId> pair = new(left, right);
        if (!tree.IsDirectedAcyclicGraph()) {
            return Fin.Fail<TopologyResult>(new TopologyFault.Cyclic(Cycles(view, tree).Count.ToString(CultureInfo.InvariantCulture)));
        }
        Option<NodeId> ambiguous = toSeq(tree.Vertices).Find(vertex => tree.InDegree(vertex) > 1);
        return ambiguous.Match(
            Some: node => Fin.Fail<TopologyResult>(new TopologyFault.NotForest(node.Value)),
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

    // ONE component-partition body: the labeller and the surviving-partition predicate are its two arguments,
    // where components, islands, and cycles each spelled their own buffer, call, and grouping. The buffer is a
    // mutable Dictionary because the QuikGraph labellers take an IDictionary out-parameter by contract.
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

    // Cycles are strongly-connected partitions of more than one vertex, or singletons carrying their own self-loop.
    static Seq<KeySelection> Cycles(TopologyView view, BidirectionalGraph<NodeId, TypedEdge> graph) =>
        Grouped(view, graph, static (g, labels) => g.StronglyConnectedComponents(labels),
            members => members.Count > 1 || members.Exists(node => graph.ContainsEdge(node, node)));

    // Spatial anchors are roots and sinks of the full containment-plus-aggregation tree.
    static KeySelection Anchors(TopologyView view) {
        BidirectionalGraph<NodeId, TypedEdge> spatial = view.View(EdgeFilter.Spatial, EdgeOrientation.Forward);
        return KeySelection.Of(toSeq(spatial.Roots().Concat(spatial.Sinks()).Select(view.Key)), view.Scope);
    }

    // `Topological` gates on the DAG check, so source-first sorting never silently drops cyclic remainders and
    // `SourceFirstTopologicalSort` never raises its `NonAcyclicGraphException` past the rail.
    static Fin<TopologyResult> Topological(TopologyView view) {
        BidirectionalGraph<NodeId, TypedEdge> graph = view.View(EdgeFilter.All, EdgeOrientation.Forward);
        return graph.IsDirectedAcyclicGraph()
            ? Fin.Succ<TopologyResult>(new TopologyResult.Ordered(toSeq(graph.SourceFirstTopologicalSort().Select(view.Key))))
            : Fin.Fail<TopologyResult>(new TopologyFault.Cyclic(Cycles(view, graph).Count.ToString(CultureInfo.InvariantCulture)));
    }

    // Transitive reduction diffs a DAG-gated spatial view against its minimal reachability-equivalent graph.
    // Removed direct edges return as typed redundant pairs.
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

| [INDEX] | [POLICY]             | [VALUE]                                        | [BINDING]                                   |
| :-----: | :------------------- | :--------------------------------------------- | :------------------------------------------ |
|  [01]   | algorithm owner      | QuikGraph `AlgorithmExtensions`                | no hand-rolled walk or recursive CTE        |
|  [02]   | dispatch             | generated `query.Switch(...)`                  | exhaustive; no silent `_` arm               |
|  [03]   | absent root          | `Rooted`/`Paired` → `TopologyFault.RootAbsent` | typed fault, never an empty sentinel        |
|  [04]   | result shape         | `KeySelection` under the view's model scope    | composes with the seam selection algebra    |
|  [05]   | component partition  | one `Grouped` body, two row arguments          | shared by components, islands, and cycles   |
|  [06]   | void resolution      | `Neighbors` under `Neighborhood.Openings`      | one polymorphic case, never a Resolve twin  |
|  [07]   | nearest container    | forest-gated `OfflineLeastCommonAncestor`      | cycles and multiple parents rail            |
|  [08]   | shortest path        | unit-weight `ShortestPathsDijkstra`            | metric routing belongs to `pgrouting`       |
|  [09]   | spatial walks        | seam `Spatial` (`Contain` ∪ `Aggregate`)       | full-tree climb                             |
|  [10]   | connection adjacency | `OutEdges` ∪ `InEdges` over `Connection`       | symmetric read                              |
|  [11]   | placement / members  | `Containment` / `Assignment`                   | direct storey / reverse membership          |
|  [12]   | bounded depth        | seam `WalkDepth`                               | no raw `int` enters `Walk`                  |
|  [13]   | redundancy           | DAG-gated `ComputeTransitiveReduction` diff    | redundant edges return typed `Pruned` pairs |

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

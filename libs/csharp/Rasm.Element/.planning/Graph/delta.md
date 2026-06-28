# [ELEMENT_DELTA]

The ONE graph-mutation owner: a `GraphMutation` `[Union]` request (`PutNode`/`DropNode`/`Link`/`Unlink`/`Batch`) applied through a total `Switch` to a HAMT `WorkingGraph` producing a `GraphDelta` — the single immutable change record that IS the persistable event body. This is the live-authoring counterpart to the frozen `Graph/element#ELEMENT_GRAPH` read snapshot: the `WorkingGraph` is an `ImmutableDictionary` HAMT over nodes plus an `ImmutableList` of edges (O(log n) structural sharing per edit), `Thaw` lowers a frozen `ElementGraph` into it, `Freeze` lifts it back to a read snapshot (rebuilding the incidence index and the `QuikGraph` view once). Every mutation enforces the seam's STRUCTURAL edge law — a `Link` validates its endpoint node-kinds against the structural legality (`Compose` joins objects, `Associate` binds a resource, `Assign` targets a definition), railing `Projection/fault#FAULT_BAND` `ElementFault.RelationshipInvalid`; a `DropNode` cascades its incident edges so the graph never dangles; a duplicate link or a drop-absent rails `ElementFault.DeltaConflict` — but it enforces ONLY structural invariants, the IFC-semantic legality being the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint`. The `GraphDelta` is a monoid (`Empty` + `Merge`) so a `Batch` accumulates one delta, an optional `Header` on the delta lets the model-creating event establish the model header (release/view/georeference/STEP), and `ReplayOnto` folds a delta into a frozen graph — the persistence path where Marten stores the `GraphDelta` event and the inline projection folds `GraphDelta → ElementGraph`, never a whole-graph snapshot per event. The page composes `Graph/element#ELEMENT_GRAPH` for the frozen snapshot and `Relations/relation#EDGE_ALGEBRA` for the edge endpoints; a malformed mutation rails `ElementFault`.

## [01]-[INDEX]

- [01]-[GRAPH_DELTA]: the `WorkingGraph` HAMT live form, the `GraphMutation` `[Union]` request + total `Switch`, the structural edge law, the `Apply` fold, the `GraphDelta` monoid event body (with the `Put`/`Link`/`Reheader` projector builders + the `Merge` monoid the `Projection/projection#PROJECTION_CONTRACT` `Project` fold composes, the `NodeCount`/`EdgeCount` magnitude reads, and the `ToCanonicalBytes` delta content key), and the `ReplayOnto` persistence fold.

## [02]-[GRAPH_DELTA]

- Owner: `WorkingGraph` the HAMT live-authoring form (`ImmutableDictionary<NodeId, Node>` + `ImmutableList<Relationship>`); `GraphMutation` the `[Union]` mutation request; `GraphDelta` the immutable change record that IS the persistable event body; the structural edge law `LegalLink`.
- Cases: `PutNode` (upsert a node — add if absent, revise if present, recording the before/after) · `DropNode` (remove a node and CASCADE its incident edges) · `Link` (add an edge after validating endpoint presence and structural legality) · `Unlink` (remove an edge by structural equality) · `Batch` (a `Seq<GraphMutation>` folded fail-fast into one accumulated delta); the closed mutation family.
- Entry: `WorkingGraph.Thaw(ElementGraph)` lowers a frozen snapshot into the HAMT form; `Apply(GraphMutation, key)` applies a mutation, `Fin<T>` returning the next `WorkingGraph` plus the `GraphDelta` it produced (railing `ElementFault.NodeAbsent` on a link to an absent endpoint, `ElementFault.RelationshipInvalid` on an illegal endpoint-kind pair, `ElementFault.DeltaConflict` on a drop-absent or duplicate link); `Freeze()` lifts the HAMT form back to a frozen `ElementGraph` (rebuilding the incidence index and `QuikGraph` view once); `GraphDelta.ReplayOnto(ElementGraph)` folds a delta into a frozen graph for the persistence rehydrate.
- Auto: `Apply` dispatches the generated total `Switch` — `PutNode` upserts into the HAMT (an existing id revises and records the before/after in `RevisedNodes`, an absent id adds to `AddedNodes`), `DropNode` removes the node and every incident edge (recorded in `RemovedNodes`/`RemovedEdges`) so no edge dangles, `Link` validates the endpoints present and the structural `LegalLink` legality then appends the edge (recorded in `AddedEdges`, a duplicate railing `DeltaConflict`), `Unlink` removes the structurally-equal edge, `Batch` folds each sub-mutation through `Apply` accumulating the deltas via `Merge` and fail-fast on the first error; the `WorkingGraph` shares structure across edits (HAMT), so an authoring session mutates in O(log n) per node and the read snapshot materializes only at `Freeze`.
- Receipt: the `GraphDelta` is the one change record — the Marten event body carrying the added/removed/revised nodes and added/removed edges, NOT a whole-graph snapshot per event; the inline `SingleStreamProjection` folds `GraphDelta → ElementGraph` through `ReplayOnto` so the read snapshot rebuilds from the delta stream, a periodic `AggregateSnapshot` bounding replay, the cadence reading `NodeCount`/`EdgeCount` for the change magnitude; `ToCanonicalBytes(tolerance)` derives the delta's content key (the Persistence event dedup and the Version op-identity) on the same `XxHash128` canonical rail the node/edge addresses use, so a re-applied or duplicated delta is detected by content, never a wall-clock; the `Generator.Equals` `Inequalities` member diff (`Graph/element#ELEMENT_GRAPH`) and the `GraphDelta` are the two change surfaces — the diff for a content-3-way merge, the delta for the forward event log.
- Packages: Thinktecture.Runtime.Extensions (`[Union]`), LanguageExt.Core (`Seq`/`Option`/`Fin`), System.Collections.Immutable/Frozen, `Rasm` (the kernel `Op` op-key).
- Growth: a new mutation is one `GraphMutation` case routed through the total `Switch` (a `Merge` mutation folding a sub-graph); a new structural invariant is one arm in `LegalLink`; the `GraphDelta` event body grows by column, not by a parallel event type; never a per-node-kind mutation and never a whole-graph snapshot per event.
- Boundary: `GraphMutation` is the ONE mutation owner applied through a total `Switch` — a per-node-kind add/remove method family is the deleted form; the `WorkingGraph` is the HAMT live form and `ElementGraph` the frozen read snapshot, `Thaw`/`Freeze` the only crossings, so a mutation never touches the frozen snapshot's incidence index or memo (a new snapshot is built once at `Freeze`); the structural edge law `LegalLink` enforces ONLY the seam's structural invariants (endpoint-kind legality), the IFC-semantic legality (containment-relating-must-be-spatial, `Void` element→opening, type-may-not-aggregate-occurrence) being the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` `Validation<Error,Unit>`, so the seam carries no IFC vocabulary; a `DropNode` cascades its incident edges so the graph never dangles, and a drop-absent or a duplicate link rails `ElementFault.DeltaConflict`, never a silent no-op; the `GraphDelta` is the event body (the change, not the whole graph) and `ReplayOnto` the persistence fold, so a stream of deltas rebuilds any snapshot and the whole-graph-per-event bloat is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [MODELS] -----------------------------------------------------------------------------
// The persistable event body — the change, never a whole-graph snapshot. A monoid so a Batch
// accumulates one delta and the Marten inline projection folds GraphDelta → ElementGraph.
public sealed record GraphDelta(
 Seq<Node> AddedNodes,
 Seq<NodeId> RemovedNodes,
 Seq<(Node Before, Node After)> RevisedNodes,
 Seq<Relationship> AddedEdges,
 Seq<Relationship> RemovedEdges,
 Option<Header> Header) {

 public static readonly GraphDelta Empty = new(Empty<Node>(), Empty<NodeId>(), Empty<(Node, Node)>(), Empty<Relationship>(), Empty<Relationship>(), None);

 // The monoid append — the next delta's changes accumulate and its header (if any) wins.
 public GraphDelta Merge(GraphDelta next) => new(
 AddedNodes + next.AddedNodes, RemovedNodes + next.RemovedNodes, RevisedNodes + next.RevisedNodes,
 AddedEdges + next.AddedEdges, RemovedEdges + next.RemovedEdges,
 next.Header.IsSome ? next.Header : Header);

 public bool IsEmpty => AddedNodes.IsEmpty && RemovedNodes.IsEmpty && RevisedNodes.IsEmpty && AddedEdges.IsEmpty && RemovedEdges.IsEmpty && Header.IsNone;

 // The change magnitude the Rasm.Persistence Version/ledger snapshot cadence reads to bound replay — the node
 // touch count (added + revised + removed) and the edge touch count (added + removed).
 public int NodeCount => AddedNodes.Count + RevisedNodes.Count + RemovedNodes.Count;
 public int EdgeCount => AddedEdges.Count + RemovedEdges.Count;

 // The standalone content key a Persistence event dedup and a Version op-identity derive from the delta — the SAME
 // XxHash128 canonical rail the node/edge/graph addresses use, composed through the per-member ToCanonicalBytes.
 // Wall-clock-bearing header fields (the Instant At, the StepHeader timestamps) are EXCLUDED so two structurally
 // identical model-creating deltas address identically; only the header's schema/view/tolerance/EPSG contribute.
 public ReadOnlyMemory<byte> ToCanonicalBytes(double tolerance) {
 CanonicalWriter w = new(tolerance);
 w.Ordinal(AddedNodes.Count); foreach (Node n in AddedNodes) { w.String(n.Id.Value).Raw(n.ToCanonicalBytes(tolerance).Span); }
 w.Ordinal(RevisedNodes.Count); foreach (var (_, after) in RevisedNodes) { w.String(after.Id.Value).Raw(after.ToCanonicalBytes(tolerance).Span); }
 w.Ordinal(RemovedNodes.Count); foreach (NodeId id in RemovedNodes) { w.String(id.Value); }
 w.Ordinal(AddedEdges.Count); foreach (Relationship e in AddedEdges) { w.Raw(e.ToCanonicalBytes().Span); }
 w.Ordinal(RemovedEdges.Count); foreach (Relationship e in RemovedEdges) { w.Raw(e.ToCanonicalBytes().Span); }
 w.Bool(Header.IsSome); Header.IfSome(h => { w.String(h.Schema.Key); w.String(h.View.Key); w.Double(h.Tolerance); h.Reference.Epsg.IfSome(epsg => w.Ordinal(epsg)); });
 return w.ToBytes();
 }

 // Projector builders — an IElementProjection.Project fold accumulates authored nodes/edges/header onto the running
 // delta (the seam-owned composition path beside the GraphMutation/Apply authoring path). A Node carries its own
 // content-addressed NodeId, so Put needs no separate key; Put/Link echo the GraphMutation case names and Reheader
 // establishes the model header on the model-creating delta; the monoid append is Merge.
 public GraphDelta Put(Node node) => this with { AddedNodes = AddedNodes.Add(node) };
 public GraphDelta Link(Relationship edge) => this with { AddedEdges = AddedEdges.Add(edge) };
 public GraphDelta Reheader(Header header) => this with { Header = Some(header) };

 static Seq<T> Empty<T>() => Seq<T>();

 // The persistence rehydrate fold: replay a delta onto a frozen snapshot. The delta was validated
 // when produced, so replay re-applies raw — thaw, apply the recorded changes, freeze under the delta's own
 // header when it carries one (the model-establishing event) else the existing graph header.
 public ElementGraph ReplayOnto(ElementGraph graph) {
 var working = WorkingGraph.Thaw(graph);
 working = RemovedNodes.Fold(working, static (w, id) => w.Erase(id));
 working = AddedNodes.Fold(working, static (w, n) => w.Set(n));
 working = RevisedNodes.Fold(working, static (w, r) => w.Set(r.After));
 working = RemovedEdges.Fold(working, static (w, e) => w.Detach(e));
 working = AddedEdges.Fold(working, static (w, e) => w.Attach(e));
 return working.Freeze(Header.IfNone(graph.Header));
 }
}

[Union]
public abstract partial record GraphMutation {
 private GraphMutation() { }
 public sealed record PutNode(Node Node) : GraphMutation;
 public sealed record DropNode(NodeId Id) : GraphMutation;
 public sealed record Link(Relationship Edge) : GraphMutation;
 public sealed record Unlink(Relationship Edge) : GraphMutation;
 public sealed record Batch(Seq<GraphMutation> Mutations) : GraphMutation;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The HAMT live-authoring form — ImmutableDictionary nodes + ImmutableList edges, O(log n) per edit.
public sealed record WorkingGraph(ImmutableDictionary<NodeId, Node> Nodes, ImmutableList<Relationship> Edges) {
 public static WorkingGraph Thaw(ElementGraph graph) =>
 new(graph.Nodes.ToImmutableDictionary(), graph.Edges.ToImmutableList());

 public ElementGraph Freeze(Header header) =>
 ElementGraph.Of(header, Nodes.ToFrozenDictionary(), [.. Edges]);

 internal WorkingGraph Set(Node node) => this with { Nodes = Nodes.SetItem(node.Id, node) };
 internal WorkingGraph Erase(NodeId id) => this with { Nodes = Nodes.Remove(id), Edges = Edges.RemoveAll(e => e.Touches(id)) };
 internal WorkingGraph Attach(Relationship edge) => this with { Edges = Edges.Add(edge) };
 internal WorkingGraph Detach(Relationship edge) => this with { Edges = Edges.Remove(edge) };

 public Fin<(WorkingGraph Graph, GraphDelta Delta)> Apply(GraphMutation mutation, Op key) =>
 mutation.Switch<(WorkingGraph, Op), Fin<(WorkingGraph, GraphDelta)>>(
 state: (this, key),
 putNode: static (s, m) => Fin.Succ(s.Item1.Nodes.TryGetValue(m.Node.Id, out Node? prior)
 ? (s.Item1.Set(m.Node), GraphDelta.Empty with { RevisedNodes = Seq1((prior, m.Node)) })
 : (s.Item1.Set(m.Node), GraphDelta.Empty with { AddedNodes = Seq1(m.Node) })),
 dropNode: static (s, m) => s.Item1.Nodes.ContainsKey(m.Id)
 ? Fin.Succ((s.Item1.Erase(m.Id), GraphDelta.Empty with { RemovedNodes = Seq1(m.Id), RemovedEdges = s.Item1.Edges.Filter(e => e.Touches(m.Id)).ToSeq() }))
 : ElementFault.DeltaConflict(s.Item2, $"<drop-absent-node:{m.Id.Value}>"),
 link: static (s, m) => LegalLink(m.Edge, s.Item1.Nodes, s.Item2)
 .Bind(_ => s.Item1.Edges.Contains(m.Edge)
 ? Fin.Fail<(WorkingGraph, GraphDelta)>(ElementFault.DeltaConflict(s.Item2, "<duplicate-link>"))
 : Fin.Succ((s.Item1.Attach(m.Edge), GraphDelta.Empty with { AddedEdges = Seq1(m.Edge) }))),
 unlink: static (s, m) => s.Item1.Edges.Contains(m.Edge)
 ? Fin.Succ((s.Item1.Detach(m.Edge), GraphDelta.Empty with { RemovedEdges = Seq1(m.Edge) }))
 : ElementFault.DeltaConflict(s.Item2, "<unlink-absent-edge>"),
 batch: static (s, m) => m.Mutations.Fold(
 Fin.Succ((s.Item1, GraphDelta.Empty)),
 (acc, next) => acc.Bind(state => state.Item1.Apply(next, s.Item2).Map(step => (step.Graph, state.Item2.Merge(step.Delta))))));

 // The seam's STRUCTURAL edge law: endpoint presence + endpoint-kind legality ONLY. The IFC-semantic
 // legality is the consumer's IGraphConstraint, never here.
 static Fin<Unit> LegalLink(Relationship edge, ImmutableDictionary<NodeId, Node> nodes, Op key) {
 var (relating, related) = edge.Endpoints;
 return !nodes.TryGetValue(relating, out Node? r) ? ElementFault.NodeAbsent(key, $"<link-relating-absent:{relating.Value}>")
 : !nodes.TryGetValue(related, out Node? d) ? ElementFault.NodeAbsent(key, $"<link-related-absent:{related.Value}>")
 : edge switch {
 Relationship.Compose => r is Node.Object && d is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<compose-endpoints-must-be-objects>"),
 Relationship.Connect => r is Node.Object && d is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<connect-endpoints-must-be-objects>"),
 Relationship.Void => r is Node.Object && d is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<void-endpoints-must-be-objects>"),
 Relationship.Associate => (r is Node.Object) && (d is (Node.Material or Node.Appearance or Node.Coverage)) ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<associate-resource-must-be-material-appearance-or-coverage>"),
 Relationship.Assign a => LegalAssign(a, r, d, key),
 Relationship.Generic => Fin.Succ(unit),
 _ => Fin.Succ(unit),
 };
 }

 static Fin<Unit> LegalAssign(Relationship.Assign a, Node subject, Node definition, Op key) =>
 subject is not Node.Object ? ElementFault.RelationshipInvalid(key, "<assign-subject-must-be-object>")
 : a.SubKind.Switch(
 propertyDefinition: () => definition is Node.PropertySet or Node.QuantitySet ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-property-definition-must-target-bag>"),
 typeDefinition: () => definition is Node.Object { Kind: var k } && k == ObjectKind.Type ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-type-definition-must-target-type-object>"),
 group: () => definition is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-group-must-target-object>"),
 assessment: () => definition is Node.Assessment ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-assessment-must-target-assessment>"));
}
```

## [03]-[RESEARCH]

- [DELTA_EVENT_BODY]: the `GraphDelta` (not a whole-graph snapshot) is the Marten event body — the stream grain is one stream PER MODEL (or spatial partition), the event the delta, and the inline `SingleStreamProjection` folds `GraphDelta → ElementGraph` through `ReplayOnto` with a periodic `AggregateSnapshot` bounding replay; the `GraphDelta` monoid (`Empty`/`Merge`) lets a `Batch` mutation produce one delta and a stream segment fold to one cumulative change, so the persistence path never carries the whole-graph bloat a snapshot-per-event design would imply — the `Rasm.Persistence` `Version/ledger` owns the Marten stream and the snapshot cadence, this owner producing the delta the stream stores.
- [STRUCTURAL_VS_SEMANTIC]: the seam's `LegalLink` enforces ONLY the structural edge law — endpoint presence and endpoint-kind legality (`Compose` joins objects, `Associate` binds a material/appearance/coverage resource, `Assign` targets a definition by its kind) — railing `ElementFault.RelationshipInvalid`; the IFC-semantic legality (containment-relating-must-be-spatial, `Void` must be element→opening, a `Type` object may not aggregate an `Occurrence`) is the `Rasm.Bim`-implemented `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` returning `Validation<Error,Unit>`, composed AFTER the structural law at the projector boundary; the two-interface split has the seam carrying the structural floor and the Bim constraint the schema legality, so the seam never names an IFC entity or relationship.

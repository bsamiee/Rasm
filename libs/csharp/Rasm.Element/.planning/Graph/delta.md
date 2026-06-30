# [ELEMENT_DELTA]

The ONE graph-mutation owner: a `GraphMutation` `[Union]` request (`PutNode`/`DropNode`/`Link`/`Unlink`/`Reheader`/`Batch`) applied through the generated total `Switch` to a HAMT `WorkingGraph` producing a `GraphDelta` — the single immutable change record that IS the persistable event body. This is the live-authoring counterpart to the frozen `Graph/element#ELEMENT_GRAPH` read snapshot: the `WorkingGraph` is an `ImmutableDictionary` HAMT over nodes plus an `ImmutableList` of edges (O(log n) structural sharing per edit), `Thaw` lowers a frozen `ElementGraph` into it, `Freeze` lifts it back to a read snapshot (rebuilding the incidence index and the `QuikGraph` view once). Every mutation enforces the seam's STRUCTURAL edge law through the generated total `Switch` over the `Relations/relation#EDGE_ALGEBRA` cases — a `Link` validates its endpoint node-kinds against the structural legality (`Compose`/`Connect`/`Void` join objects, `Associate` binds a material/appearance/coverage resource, `Assign` targets a definition by its sub-kind), railing `Projection/fault#FAULT_BAND` `ElementFault.RelationshipInvalid`; a `DropNode` cascades its incident edges so the graph never dangles; a duplicate link or a drop-absent rails `ElementFault.DeltaConflict` — but it enforces ONLY structural invariants, the IFC-semantic legality being the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint`. The `GraphDelta` is a SEQUENTIAL-COMPOSITION monoid (`Empty` + `Merge`): `Merge` cancels a node added-then-removed and drops the stale revise of a node revised-then-removed so the law `a.Merge(b).ReplayOnto(g) == b.ReplayOnto(a.ReplayOnto(g))` holds — a `Batch` accumulates ONE faithful delta and a Persistence stream segment compacts to one cumulative change without the naive concatenation's resurrected nodes. A `Reheader` mutation (mirrored by the `GraphDelta.Reheader` projector builder) establishes the model header on the model-creating event, so the interactive authoring path sets release/view/georeference/tolerance through the SAME algebra the projector uses, and `ReplayOnto` folds a delta into a frozen graph — the persistence path where Marten stores the `GraphDelta` event and the inline projection folds `GraphDelta → ElementGraph`, never a whole-graph snapshot per event — while `AdmitOnto` is the structural-VALIDATING sibling that routes a projector-built delta through `WorkingGraph.Apply` (the `LegalLink` law per `Link`) for the `Projection/projection#PROJECTION_CONTRACT` `Assemble` admission. The page composes `Graph/element#ELEMENT_GRAPH` for the frozen snapshot, `Relations/relation#EDGE_ALGEBRA` for the edge endpoints, and `Projection/address#CANONICAL_WRITER` for the order-independent content key; a malformed mutation rails `ElementFault`.

## [01]-[INDEX]

- [01]-[GRAPH_DELTA]: the `WorkingGraph` HAMT live form, the `GraphMutation` `[Union]` request + generated total `Switch`, the structural edge law `LegalLink`/`LegalAssign` over the generated `Switch` (no runtime-default arm), the `Apply` fold, the `GraphDelta` sequential-composition monoid event body (with the `Put`/`Link`/`Reheader` projector builders, the cancellation-correct `Merge` the `Projection/projection#PROJECTION_CONTRACT` `Project` fold and the Persistence stream compaction compose, the `NodeCount`/`EdgeCount` magnitude reads, and the order-independent `ToCanonicalBytes` delta content key), the `ReplayOnto` raw persistence fold, and its structural-validating projection-admission sibling `AdmitOnto`.

## [02]-[GRAPH_DELTA]

- Owner: `WorkingGraph` the HAMT live-authoring form (`ImmutableDictionary<NodeId, Node>` + `ImmutableList<Relationship>`); `GraphMutation` the `[Union]` mutation request; `GraphDelta` the immutable change record that IS the persistable event body; the structural edge law `LegalLink`/`LegalAssign`.
- Cases: `PutNode` (upsert a node — add if absent, revise if present, recording the before/after) · `DropNode` (remove a node and CASCADE its incident edges) · `Link` (add an edge after validating endpoint presence and structural legality) · `Unlink` (remove an edge by structural equality) · `Reheader` (establish/revise the model header on the model-creating event — the interactive-authoring counterpart to the projector's `GraphDelta.Reheader` builder) · `Batch` (a `Seq<GraphMutation>` folded fail-fast into one accumulated delta); the closed mutation family.
- Entry: `WorkingGraph.Thaw(ElementGraph)` lowers a frozen snapshot into the HAMT form; `Apply(GraphMutation, key)` applies a mutation, `Fin<T>` returning the next `WorkingGraph` plus the `GraphDelta` it produced (railing `ElementFault.NodeAbsent` on a link to an absent endpoint, `ElementFault.RelationshipInvalid` on an illegal endpoint-kind pair, `ElementFault.DeltaConflict` on a drop-absent or duplicate link); `Freeze(header)` lifts the HAMT form back to a frozen `ElementGraph` under the resolved header (rebuilding the incidence index and `QuikGraph` view once), the header an interactive session resolves as `accumulatedDelta.Header.IfNone(baseHeader)` — the SAME resolution `ReplayOnto` uses; `GraphDelta.ReplayOnto(ElementGraph)` folds a delta into a frozen graph for the persistence rehydrate (re-applying RAW, the delta validated when produced); `GraphDelta.AdmitOnto(ElementGraph, Op)` is the structural-VALIDATING sibling routing a projector-built delta through `WorkingGraph.Apply` so `LegalLink` runs per `Link` (the `Projection/projection#PROJECTION_CONTRACT` `Assemble` admission step), `Fin<T>` returning the frozen graph plus the re-derived event body and railing `RelationshipInvalid`/`NodeAbsent`/`DeltaConflict` on a structurally-illegal projection.
- Auto: `Apply` dispatches the generated total `Switch` with state — `PutNode` upserts into the HAMT (an existing id revises and records the before/after in `RevisedNodes`, an absent id adds to `AddedNodes`), `DropNode` removes the node and every incident edge (recorded in `RemovedNodes`/`RemovedEdges`) so no edge dangles, `Link` validates the endpoints present and the structural `LegalLink` legality then appends the edge (recorded in `AddedEdges`, a duplicate railing `DeltaConflict`), `Unlink` removes the structurally-equal edge, `Reheader` leaves the nodes/edges untouched and records the header on the delta (the WorkingGraph stays a pure node/edge HAMT — the header rides the delta and the frozen snapshot, never the working form), `Batch` folds each sub-mutation through `Apply` accumulating the deltas via `Merge` and fail-fast on the first error; the `WorkingGraph` shares structure across edits (HAMT), so an authoring session mutates in O(log n) per node and the read snapshot materializes only at `Freeze`. `LegalLink` and `LegalAssign` dispatch the generated total `Switch` over the `Relations/relation#EDGE_ALGEBRA` cases and the `AssignKind` sub-kinds — compile-time exhaustive over the closed family, NO runtime-default arm.
- Receipt: the `GraphDelta` is the one change record — the Marten event body carrying the added/removed/revised nodes and added/removed edges, NOT a whole-graph snapshot per event; the inline `SingleStreamProjection` folds `GraphDelta → ElementGraph` through `ReplayOnto` so the read snapshot rebuilds from the delta stream, a periodic `AggregateSnapshot` bounding replay, the cadence reading `NodeCount`/`EdgeCount` for the change magnitude; `ToCanonicalBytes(tolerance)` derives the delta's ORDER-INDEPENDENT content key (the Persistence event dedup and the Version op-identity) on the same `XxHash128` canonical rail the node/edge/graph addresses use — nodes sorted by id, edges by canonical bytes, the full `Geospatial/reference#GEO_REFERENCE` `GeoReference` folded into the header contribution — so a re-applied, duplicated, or recording-order-permuted delta is detected by content, never a wall-clock; the `Generator.Equals` member diff (`Graph/element#ELEMENT_GRAPH` `ElementGraph.EqualityComparer.Default.Inequalities`) and the `GraphDelta` are the two change surfaces — the diff for a content-3-way merge, the delta for the forward event log.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` + the generated total `Switch`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Fold`), System.Collections.Immutable/Frozen, `Rasm` (the kernel `Op` op-key).
- Growth: a new mutation is one `GraphMutation` case routed through the total `Switch` (the `Reheader` header-establishing mutation landed exactly this way); a new structural invariant is one arm in the `LegalLink` generated `Switch`; the `GraphDelta` event body grows by column, not by a parallel event type; never a per-node-kind mutation and never a whole-graph snapshot per event.
- Boundary: `GraphMutation` is the ONE mutation owner applied through the generated total `Switch` — a per-node-kind add/remove method family is the deleted form, and the structural edge law is the generated total `Switch` over the closed edge algebra (a C# type-pattern `switch` with a runtime-silent `_` default arm is the deleted form, because the closed family must break loudly at compile time when a case is added); the `WorkingGraph` is the HAMT live form and `ElementGraph` the frozen read snapshot, `Thaw`/`Freeze` the only crossings, so a mutation never touches the frozen snapshot's incidence index or memo (a new snapshot is built once at `Freeze`); the `WorkingGraph` carries no header (the model header rides the `GraphDelta` event and the frozen `ElementGraph`, never the working form), so a `Reheader` mutation records the header on the delta and `Freeze`/`ReplayOnto` resolve it as `delta.Header.IfNone(graphHeader)`; the structural edge law `LegalLink` enforces ONLY the seam's structural invariants (endpoint-kind legality), the IFC-semantic legality (containment-relating-must-be-spatial, `Void` element→opening, type-may-not-aggregate-occurrence) being the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` `Validation<Error,Unit>`, so the seam carries no IFC vocabulary; the `GraphDelta` `Merge` is a SEQUENTIAL-COMPOSITION monoid — a naive set-union concatenation is the deleted form because `ReplayOnto`'s remove-before-set order would resurrect a node added-then-removed (or revised-then-removed) in one segment, so `Merge` drops this's pending adds and revises that next removes and cancels a removal that targets one of this's pending adds, making the `Batch` accumulation and the Persistence stream compaction faithful; a `DropNode` cascades its incident edges so the graph never dangles, and a drop-absent or a duplicate link rails `ElementFault.DeltaConflict`, never a silent no-op; the `GraphDelta` is the event body (the change, not the whole graph) and `ReplayOnto` the persistence fold, so a stream of deltas rebuilds any snapshot and the whole-graph-per-event bloat is the deleted form; `ReplayOnto` re-applies RAW (the delta was validated when produced) while `AdmitOnto` routes a projector-built delta through `WorkingGraph.Apply` so the structural `LegalLink` law runs at the projection boundary — the two share the thaw→apply→freeze crossing but split on validation, and a projector path that called `ReplayOnto` (skipping `LegalLink`) instead of `AdmitOnto` is the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [MODELS] -----------------------------------------------------------------------------
// The persistable event body — the change, never a whole-graph snapshot. A sequential-composition monoid so
// a Batch accumulates one FAITHFUL delta and the Marten inline projection folds GraphDelta → ElementGraph.
public sealed record GraphDelta(
 Seq<Node> AddedNodes,
 Seq<NodeId> RemovedNodes,
 Seq<(Node Before, Node After)> RevisedNodes,
 Seq<Relationship> AddedEdges,
 Seq<Relationship> RemovedEdges,
 Option<Header> Header) {

 public static readonly GraphDelta Empty = new([], [], [], [], [], None);

 // The sequential-composition monoid append (this THEN next). next's removals (`killed`) DROP this's pending adds and
 // revises so ReplayOnto's remove-before-set order can never resurrect them; next's removal that targets one of this's
 // pending adds (`addedIds`) is itself dropped (the add+remove cancel to nothing — the node never reaches the base);
 // a revised-then-removed node needs only its revise dropped, because next's own removal entry already records the
 // removal (no separate promotion — that would double-count the id and fork the content key). Edges cancel symmetrically
 // by structural equality. This keeps the monoid law a.Merge(b).ReplayOnto(g) == b.ReplayOnto(a.ReplayOnto(g)); a
 // remove-then-add of one id is kept on both sides and replays correctly, surviving adds/revises last-write-wins.
 public GraphDelta Merge(GraphDelta next) {
  var killed = next.RemovedNodes.ToHashSet();
  var addedIds = AddedNodes.Map(static n => n.Id).ToHashSet();
  var droppedEdges = next.RemovedEdges.ToHashSet();
  var addedEdges = AddedEdges.ToHashSet();
  return new(
   AddedNodes.Filter(n => !killed.Contains(n.Id)) + next.AddedNodes,
   RemovedNodes + next.RemovedNodes.Filter(id => !addedIds.Contains(id)),
   RevisedNodes.Filter(r => !killed.Contains(r.After.Id)) + next.RevisedNodes,
   AddedEdges.Filter(e => !droppedEdges.Contains(e)) + next.AddedEdges,
   RemovedEdges + next.RemovedEdges.Filter(e => !addedEdges.Contains(e)),
   next.Header.IsSome ? next.Header : Header);
 }

 public bool IsEmpty => AddedNodes.IsEmpty && RemovedNodes.IsEmpty && RevisedNodes.IsEmpty && AddedEdges.IsEmpty && RemovedEdges.IsEmpty && Header.IsNone;

 // The change magnitude the Rasm.Persistence Version/ledger snapshot cadence reads to bound replay — the node
 // touch count (added + revised + removed) and the edge touch count (added + removed).
 public int NodeCount => AddedNodes.Count + RevisedNodes.Count + RemovedNodes.Count;
 public int EdgeCount => AddedEdges.Count + RemovedEdges.Count;

 // The standalone ORDER-INDEPENDENT content key a Persistence event dedup and a Version op-identity derive from the delta
 // — the SAME XxHash128 canonical rail the node/edge/graph addresses use, composed through the per-member ToCanonicalBytes.
 // Nodes sort by id and edges by canonical bytes (the Projection/address#CONTENT_ADDRESS OfGraph order-independence law) so
 // two structurally identical deltas with permuted recording order dedup identically; section counts make the layout
 // self-delimiting. Wall-clock-bearing header fields (the Instant At, the StepHeader timestamps) are EXCLUDED so two
 // structurally identical model-creating deltas address identically, but the full GeoReference (origin/rotation/scale/datum/
 // EPSG) contributes so two headers that differ only in map placement do NOT collide.
 public ReadOnlyMemory<byte> ToCanonicalBytes(double tolerance) {
  CanonicalWriter w = new(tolerance);
  w.Ordinal(AddedNodes.Count); foreach (Node n in AddedNodes.OrderBy(static n => n.Id.Value, StringComparer.Ordinal)) { w.String(n.Id.Value).Raw(n.ToCanonicalBytes(tolerance).Span); }
  w.Ordinal(RevisedNodes.Count); foreach (var (_, after) in RevisedNodes.OrderBy(static r => r.After.Id.Value, StringComparer.Ordinal)) { w.String(after.Id.Value).Raw(after.ToCanonicalBytes(tolerance).Span); }
  w.Ordinal(RemovedNodes.Count); foreach (NodeId id in RemovedNodes.OrderBy(static id => id.Value, StringComparer.Ordinal)) { w.String(id.Value); }
  w.Ordinal(AddedEdges.Count); foreach (ReadOnlyMemory<byte> b in AddedEdges.Map(static e => e.ToCanonicalBytes()).OrderBy(static x => x, ContentAddress.ByteOrder)) { w.Raw(b.Span); }
  w.Ordinal(RemovedEdges.Count); foreach (ReadOnlyMemory<byte> b in RemovedEdges.Map(static e => e.ToCanonicalBytes()).OrderBy(static x => x, ContentAddress.ByteOrder)) { w.Raw(b.Span); }
  w.Bool(Header.IsSome);
  // The ONE Graph/element#ELEMENT_GRAPH Header.CanonicalBytes projection (schema/view/tolerance + the full
  // Geospatial/reference#GEO_REFERENCE GeoReference — Epsg the CRS identity, the resolved name and the StepHeader/Instant
  // provenance excluded) — the SAME bytes the Projection/address#CONTENT_ADDRESS OfGraph snapshot header key composes, so a
  // delta's header contribution and the snapshot address never diverge, the projection owned ONCE on Header not re-spelled here.
  Header.IfSome(h => h.CanonicalBytes(w));
  return w.ToBytes();
 }

 // Projector builders — an IElementProjection.Project fold accumulates authored nodes/edges/header onto the running
 // delta (the seam-owned composition path beside the GraphMutation/Apply authoring path). A Node carries its own
 // content-addressed NodeId, so Put needs no separate key; Put/Link/Reheader echo the GraphMutation case names; the
 // monoid append is Merge.
 public GraphDelta Put(Node node) => this with { AddedNodes = AddedNodes.Add(node) };
 public GraphDelta Link(Relationship edge) => this with { AddedEdges = AddedEdges.Add(edge) };
 public GraphDelta Reheader(Header header) => this with { Header = Some(header) };

 // The persistence rehydrate fold: replay a delta onto a frozen snapshot. The delta was validated
 // when produced, so replay re-applies raw — thaw, apply the recorded changes, freeze under the delta's own
 // header when it carries one (the model-establishing event) else the existing graph header. Removed nodes erase
 // FIRST (cascading their edges), then sets, so the cancellation-correct Merge never leaves a remove+set on one id.
 public ElementGraph ReplayOnto(ElementGraph graph) {
  var working = WorkingGraph.Thaw(graph);
  working = RemovedNodes.Fold(working, static (w, id) => w.Erase(id));
  working = AddedNodes.Fold(working, static (w, n) => w.Set(n));
  working = RevisedNodes.Fold(working, static (w, r) => w.Set(r.After));
  working = RemovedEdges.Fold(working, static (w, e) => w.Detach(e));
  working = AddedEdges.Fold(working, static (w, e) => w.Attach(e));
  return working.Freeze(Header.IfNone(graph.Header));
 }

 // The structural-VALIDATING sibling of ReplayOnto: a PROJECTOR builds its delta through the Put/Link/Reheader
 // builders (never WorkingGraph.Apply), so the seam's structural edge law LegalLink has NOT run. AdmitOnto routes
 // the delta's changes through WorkingGraph.Apply (the LegalLink generated total Switch runs per Link) and freezes
 // under the delta's resolved header, carrying that header onto the re-derived event body — railing
 // RelationshipInvalid/NodeAbsent/DeltaConflict on a structurally-illegal projection; node mutations precede edge
 // mutations (the ReplayOnto application order) so a Link sees the nodes the same delta adds, and an empty delta
 // short-circuits so a no-projector assembly never rebuilds the seed snapshot. This is the Projection/projection#
 // PROJECTION_CONTRACT Assemble admission step (the validating counterpart to the raw persistence-rehydrate ReplayOnto).
 public Fin<(ElementGraph Graph, GraphDelta Delta)> AdmitOnto(ElementGraph graph, Op key) =>
  IsEmpty
   ? Fin.Succ((graph, GraphDelta.Empty))
   : WorkingGraph.Thaw(graph)
    .Apply(new GraphMutation.Batch(
     RemovedNodes.Map(static id => (GraphMutation)new GraphMutation.DropNode(id))
     + AddedNodes.Map(static node => (GraphMutation)new GraphMutation.PutNode(node))
     + RevisedNodes.Map(static revision => (GraphMutation)new GraphMutation.PutNode(revision.After))
     + RemovedEdges.Map(static edge => (GraphMutation)new GraphMutation.Unlink(edge))
     + AddedEdges.Map(static edge => (GraphMutation)new GraphMutation.Link(edge))), key)
    .Map(step => (step.Graph.Freeze(Header.IfNone(graph.Header)), step.Delta with { Header = Header }));
}

[Union]
public abstract partial record GraphMutation {
 private GraphMutation() { }
 public sealed record PutNode(Node Node) : GraphMutation;
 public sealed record DropNode(NodeId Id) : GraphMutation;
 public sealed record Link(Relationship Edge) : GraphMutation;
 public sealed record Unlink(Relationship Edge) : GraphMutation;
 public sealed record Reheader(Header Header) : GraphMutation;
 public sealed record Batch(Seq<GraphMutation> Mutations) : GraphMutation;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// The HAMT live-authoring form — ImmutableDictionary nodes + ImmutableList edges, O(log n) per edit. Header-free:
// the model header rides the GraphDelta event and the frozen ElementGraph, never the working form.
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
  mutation.Switch<(WorkingGraph Graph, Op Key), Fin<(WorkingGraph, GraphDelta)>>(
   (this, key),
   putNode: static (s, m) => Fin.Succ(s.Graph.Nodes.TryGetValue(m.Node.Id, out Node? prior)
    ? (s.Graph.Set(m.Node), GraphDelta.Empty with { RevisedNodes = [(prior!, m.Node)] })
    : (s.Graph.Set(m.Node), GraphDelta.Empty with { AddedNodes = [m.Node] })),
   dropNode: static (s, m) => s.Graph.Nodes.ContainsKey(m.Id)
    ? Fin.Succ((s.Graph.Erase(m.Id), GraphDelta.Empty with { RemovedNodes = [m.Id], RemovedEdges = toSeq(s.Graph.Edges).Filter(e => e.Touches(m.Id)) }))
    : ElementFault.DeltaConflict(s.Key, $"<drop-absent-node:{m.Id.Value}>"),
   link: static (s, m) => LegalLink(m.Edge, s.Graph.Nodes, s.Key)
    .Bind(_ => s.Graph.Edges.Contains(m.Edge)
     ? Fin.Fail<(WorkingGraph, GraphDelta)>(ElementFault.DeltaConflict(s.Key, "<duplicate-link>"))
     : Fin.Succ((s.Graph.Attach(m.Edge), GraphDelta.Empty with { AddedEdges = [m.Edge] }))),
   unlink: static (s, m) => s.Graph.Edges.Contains(m.Edge)
    ? Fin.Succ((s.Graph.Detach(m.Edge), GraphDelta.Empty with { RemovedEdges = [m.Edge] }))
    : ElementFault.DeltaConflict(s.Key, "<unlink-absent-edge>"),
   reheader: static (s, m) => Fin.Succ((s.Graph, GraphDelta.Empty with { Header = Some(m.Header) })),
   batch: static (s, m) => m.Mutations.Fold(
    Fin.Succ((Graph: s.Graph, Delta: GraphDelta.Empty)),
    (acc, next) => acc.Bind(state => state.Graph.Apply(next, s.Key).Map(step => (step.Graph, state.Delta.Merge(step.Delta))))));

 // The seam's STRUCTURAL edge law: endpoint presence + endpoint-kind legality ONLY, dispatched through the generated
 // total Switch over the closed edge algebra (compile-time exhaustive, NO runtime-default arm). The IFC-semantic
 // legality is the consumer's IGraphConstraint, never here.
 static Fin<Unit> LegalLink(Relationship edge, ImmutableDictionary<NodeId, Node> nodes, Op key) {
  var (relating, related) = edge.Endpoints;
  return !nodes.TryGetValue(relating, out Node? r) ? ElementFault.NodeAbsent(key, $"<link-relating-absent:{relating.Value}>")
   : !nodes.TryGetValue(related, out Node? d) ? ElementFault.NodeAbsent(key, $"<link-related-absent:{related.Value}>")
   : edge.Switch<(Node Relating, Node Related, Op Key), Fin<Unit>>(
    (r!, d!, key),
    compose: static (s, _) => s.Relating is Node.Object && s.Related is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(s.Key, "<compose-endpoints-must-be-objects>"),
    assign: static (s, a) => LegalAssign(a, s.Relating, s.Related, s.Key),
    associate: static (s, _) => s.Relating is Node.Object && s.Related is (Node.Material or Node.Appearance or Node.Coverage) ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(s.Key, "<associate-resource-must-be-material-appearance-or-coverage>"),
    connect: static (s, _) => s.Relating is Node.Object && s.Related is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(s.Key, "<connect-endpoints-must-be-objects>"),
    @void: static (s, _) => s.Relating is Node.Object && s.Related is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(s.Key, "<void-endpoints-must-be-objects>"),
    generic: static (s, _) => Fin.Succ(unit));
 }

 static Fin<Unit> LegalAssign(Relationship.Assign a, Node subject, Node definition, Op key) =>
  subject is not Node.Object ? ElementFault.RelationshipInvalid(key, "<assign-subject-must-be-object>")
  : a.SubKind.Switch(
   propertyDefinition: () => definition is Node.PropertySet or Node.QuantitySet ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-property-definition-must-target-bag>"),
   typeDefinition: () => definition is Node.Object o && o.Kind == ObjectKind.Type ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-type-definition-must-target-type-object>"),
   group: () => definition is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-group-must-target-object>"),
   assessment: () => definition is Node.Assessment ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-assessment-must-target-assessment>"));
}
```

## [03]-[RESEARCH]

- [DELTA_EVENT_BODY]: the `GraphDelta` (not a whole-graph snapshot) is the Marten event body — the stream grain is one stream PER MODEL (or spatial partition), the event the delta, and the inline `SingleStreamProjection` folds `GraphDelta → ElementGraph` through `ReplayOnto` with a periodic `AggregateSnapshot` bounding replay; a model-creating delta carries the `Header` (authored by the `Reheader` mutation or the `GraphDelta.Reheader` builder) so `ReplayOnto` onto `Genesis` establishes release/view/georeference/tolerance from the first event; the `ToCanonicalBytes` content key is ORDER-INDEPENDENT (nodes sorted by id, edges by canonical bytes, the full `Geospatial/reference#GEO_REFERENCE` `GeoReference` folded into the header contribution) on the one `XxHash128` seed-zero rail the `Projection/address#CONTENT_ADDRESS` graph address uses, so a duplicated or recording-order-permuted delta dedups by content — the `Rasm.Persistence` `Element/graph` Marten stream stores the delta as the `GraphEvent` body (the inline `GraphProjection` folding it through `ReplayOnto`, a periodic `AggregateSnapshot` bounding replay), the `Version/ledger` op-log/snapshot-cadence engine projecting FROM those committed events, this owner producing the delta the stream stores.
- [DELTA_MONOID]: the `GraphDelta` `Merge` is a SEQUENTIAL-COMPOSITION monoid (`Empty` the identity, `Merge` associative) satisfying `a.Merge(b).ReplayOnto(g) == b.ReplayOnto(a.ReplayOnto(g))`, the law that makes a `Batch` mutation accumulate ONE faithful delta and a Persistence stream segment compact to one cumulative change; the naive set-union concatenation is rejected because `ReplayOnto` erases removed nodes BEFORE setting added/revised ones, so a node added-then-removed (or revised-then-removed) in one segment would be resurrected — `Merge` therefore DROPS this's pending adds and revises that next removes (so the remove-before-set order cannot resurrect them) and cancels next's removal that targets one of this's pending adds (the add and the removal net to nothing, the node never reaching the base); a revised-then-removed node needs only its stale revise dropped because next's own removal entry already records the removal (no separate promotion, which would double-count the id and fork the content key), edges cancelling symmetrically by structural equality; a remove-then-add of one id is kept on both lists and replays correctly (erase the old plus its edges, set the new), and surviving adds/revises are last-write-wins on replay so a cross-segment re-add never forks identity.
- [STRUCTURAL_VS_SEMANTIC]: the seam's `LegalLink` enforces ONLY the structural edge law through the GENERATED total `Switch` over the closed edge algebra — endpoint presence and endpoint-kind legality (`Compose`/`Connect`/`Void` join objects, `Associate` binds a material/appearance/coverage resource, `Assign` targets a definition by its `AssignKind`) — railing `ElementFault.RelationshipInvalid`, a C# type-pattern `switch` with a runtime-silent `_` default arm being the deleted form because the closed family must break loudly at compile time when a case is added; the IFC-semantic legality (containment-relating-must-be-spatial, `Void` must be element→opening, a `Type` object may not aggregate an `Occurrence`) is the `Rasm.Bim`-implemented `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` returning `Validation<Error,Unit>`, composed AFTER the structural law at the projector boundary; the two-interface split has the seam carrying the structural floor and the Bim constraint the schema legality, so the seam never names an IFC entity or relationship.

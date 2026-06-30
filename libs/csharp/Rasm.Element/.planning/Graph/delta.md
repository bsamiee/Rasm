# [ELEMENT_DELTA]

The ONE graph-mutation owner: a `GraphMutation` `[Union]` request (`PutNode`/`DropNode`/`Link`/`Unlink`/`Reheader`/`Batch`) applied through the generated total `Switch` to a HAMT `WorkingGraph` producing a `GraphDelta` — the single immutable change record that IS the persistable event body. This is the live-authoring counterpart to the frozen `Graph/element#ELEMENT_GRAPH` read snapshot: the `WorkingGraph` is an `ImmutableDictionary` HAMT over nodes plus an `ImmutableList` of edges (O(log n) structural sharing per edit), `Thaw` lowers a frozen `ElementGraph` into it, `Freeze` lifts it back to a read snapshot (rebuilding the incidence index and the `QuikGraph` view once). Every mutation enforces the seam's STRUCTURAL edge law through the generated total `Switch` over the `Relations/relation#EDGE_ALGEBRA` cases — a `Link` validates its endpoint node-kinds against the structural legality (`Compose`/`Connect`/`Void` join objects — a `Connect`'s optional realizing intermediary, when present, validated present-and-an-`Object` the SAME as its binary endpoints because it rides the topology `From→Realizing→To` leg and the cascade — `Associate` binds a material/appearance/coverage resource, `Assign` targets a definition by its sub-kind), railing `Projection/fault#FAULT_BAND` `ElementFault.RelationshipInvalid`; a `DropNode` cascades its incident edges so the graph never dangles; a duplicate link or a drop-absent rails `ElementFault.DeltaConflict` — but it enforces ONLY structural invariants, the IFC-semantic legality being the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint`. The `GraphDelta` is a SEQUENTIAL-COMPOSITION monoid (`Empty` + `Merge`) producing a UNIQUE-PER-ID normal form (every node id in at most one of `{added, revised}`, remove-then-add the lone deliberate removed-plus-added pair): `Merge` cancels a node added-then-removed, drops the stale revise of a node revised-then-removed, AND coalesces a node touched twice (an add-then-revise into one add of the latest, a revise-then-revise into one revise) so BOTH the replay law `a.Merge(b).ReplayOnto(g) == b.ReplayOnto(a.ReplayOnto(g))` AND the address law (`ToCanonicalBytes` is the well-defined content key of the net change, never an id written twice into one slot list) hold — a `Batch` accumulates ONE faithful delta and a Persistence stream segment compacts to one cumulative change that DEDUPS by content, without the naive concatenation's resurrected or double-counted nodes. A `Reheader` mutation (mirrored by the `GraphDelta.Reheader` projector builder) establishes the model header on the model-creating event, so the interactive authoring path sets release/view/georeference/tolerance through the SAME algebra the projector uses, and `ReplayOnto` folds a delta into a frozen graph — the persistence path where Marten stores the `GraphDelta` event and the inline projection folds `GraphDelta → ElementGraph`, never a whole-graph snapshot per event — while `AdmitOnto` is the structural-VALIDATING sibling that routes a projector-built delta through `WorkingGraph.Apply` (the `LegalLink` law per `Link`) for the `Projection/projection#PROJECTION_CONTRACT` `Assemble` admission. The page composes `Graph/element#ELEMENT_GRAPH` for the frozen snapshot, `Relations/relation#EDGE_ALGEBRA` for the edge endpoints, and `Projection/address#CANONICAL_WRITER` for the order-independent content key; a malformed mutation rails `ElementFault`.

## [01]-[INDEX]

- [01]-[GRAPH_DELTA]: the `WorkingGraph` HAMT live form, the `GraphMutation` `[Union]` request + generated total `Switch`, the structural edge law `LegalLink`/`LegalAssign` over the generated `Switch` (no runtime-default arm), the `Apply` fold, the `GraphDelta` sequential-composition monoid event body (with the `Put`/`Link`/`Reheader` projector builders, the cancellation-and-coalescing `Merge` — the unique-per-id normal form the `Projection/projection#PROJECTION_CONTRACT` `Project` fold and the Persistence stream compaction compose — the `NodeCount`/`EdgeCount` magnitude reads, and the order-independent `ToCanonicalBytes` delta content key the normal form makes well-defined), the `ReplayOnto` raw persistence fold, and its structural-validating projection-admission sibling `AdmitOnto`.

## [02]-[GRAPH_DELTA]

- Owner: `WorkingGraph` the HAMT live-authoring form (`ImmutableDictionary<NodeId, Node>` + `ImmutableList<Relationship>`); `GraphMutation` the `[Union]` mutation request; `GraphDelta` the immutable change record that IS the persistable event body; the structural edge law `LegalLink`/`LegalAssign`.
- Cases: `PutNode` (upsert a node — add if absent, revise if present, recording the before/after) · `DropNode` (remove a node and CASCADE its incident edges) · `Link` (add an edge after validating endpoint presence and structural legality) · `Unlink` (remove an edge by structural equality) · `Reheader` (establish/revise the model header on the model-creating event — the interactive-authoring counterpart to the projector's `GraphDelta.Reheader` builder) · `Batch` (a `Seq<GraphMutation>` folded fail-fast into one accumulated delta); the closed mutation family.
- Entry: `WorkingGraph.Thaw(ElementGraph)` lowers a frozen snapshot into the HAMT form; `Apply(GraphMutation, key)` applies a mutation, `Fin<T>` returning the next `WorkingGraph` plus the `GraphDelta` it produced (railing `ElementFault.NodeAbsent` on a link to an absent endpoint, `ElementFault.RelationshipInvalid` on an illegal endpoint-kind pair, `ElementFault.DeltaConflict` on a drop-absent or duplicate link); `Freeze(header)` lifts the HAMT form back to a frozen `ElementGraph` under the resolved header (rebuilding the incidence index and `QuikGraph` view once), the header an interactive session resolves as `accumulatedDelta.Header.IfNone(baseHeader)` — the SAME resolution `ReplayOnto` uses; `GraphDelta.ReplayOnto(ElementGraph)` folds a delta into a frozen graph for the persistence rehydrate (re-applying RAW, the delta validated when produced); `GraphDelta.AdmitOnto(ElementGraph, Op)` is the structural-VALIDATING sibling routing a projector-built delta through `WorkingGraph.Apply` so `LegalLink` runs per `Link` (the `Projection/projection#PROJECTION_CONTRACT` `Assemble` admission step), `Fin<T>` returning the frozen graph plus the re-derived event body and railing `RelationshipInvalid`/`NodeAbsent`/`DeltaConflict` on a structurally-illegal projection.
- Auto: `Apply` dispatches the generated total `Switch` with state — `PutNode` upserts into the HAMT (an existing id revises and records the before/after in `RevisedNodes`, an absent id adds to `AddedNodes`), `DropNode` removes the node and every incident edge (recorded in `RemovedNodes`/`RemovedEdges`) so no edge dangles, `Link` validates the endpoints present and the structural `LegalLink` legality then appends the edge (recorded in `AddedEdges`, a duplicate railing `DeltaConflict`), `Unlink` removes the structurally-equal edge, `Reheader` leaves the nodes/edges untouched and records the header on the delta (the WorkingGraph stays a pure node/edge HAMT — the header rides the delta and the frozen snapshot, never the working form), `Batch` folds each sub-mutation through `Apply` accumulating the deltas via `Merge` and fail-fast on the first error, so a batch that touches one node twice (a `PutNode` then another `PutNode` on the same id) coalesces to ONE entry through `Merge`'s unique-per-id normal form rather than two id-slots that would fork the delta content key; the `WorkingGraph` shares structure across edits (HAMT), so an authoring session mutates in O(log n) per node and the read snapshot materializes only at `Freeze`. `LegalLink` and `LegalAssign` dispatch the generated total `Switch` over the `Relations/relation#EDGE_ALGEBRA` cases and the `AssignKind` sub-kinds — compile-time exhaustive over the closed family, NO runtime-default arm.
- Receipt: the `GraphDelta` is the one change record — the Marten event body carrying the added/removed/revised nodes and added/removed edges, NOT a whole-graph snapshot per event; the inline `SingleStreamProjection` folds `GraphDelta → ElementGraph` through `ReplayOnto` so the read snapshot rebuilds from the delta stream, the periodic Marten snapshot (`Projections.Snapshot<T>(SnapshotLifecycle.Inline)`) bounding replay, the cadence reading `NodeCount`/`EdgeCount` for the change magnitude; `ToCanonicalBytes(tolerance)` derives the delta's ORDER-INDEPENDENT content key (the Persistence event dedup and the Version op-identity) on the same `XxHash128` canonical rail the node/edge/graph addresses use — nodes sorted by id, edges by canonical bytes, the full `Geospatial/reference#GEO_REFERENCE` `GeoReference` folded into the header contribution — so a re-applied, duplicated, or recording-order-permuted delta is detected by content, never a wall-clock; the `Generator.Equals` member diff (`Graph/element#ELEMENT_GRAPH` `ElementGraph.EqualityComparer.Default.Inequalities`) and the `GraphDelta` are the two change surfaces — the diff for a content-3-way merge, the delta for the forward event log.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` + the generated total `Switch`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Fold`), System.Collections.Immutable/Frozen, `Rasm` (the kernel `Op` op-key).
- Growth: a new mutation is one `GraphMutation` case routed through the total `Switch` (the `Reheader` header-establishing mutation landed exactly this way); a new structural invariant is one arm in the `LegalLink` generated `Switch`; the `GraphDelta` event body grows by column, not by a parallel event type; never a per-node-kind mutation and never a whole-graph snapshot per event.
- Boundary: `GraphMutation` is the ONE mutation owner applied through the generated total `Switch` — a per-node-kind add/remove method family is the deleted form, and the structural edge law is the generated total `Switch` over the closed edge algebra (a C# type-pattern `switch` with a runtime-silent `_` default arm is the deleted form, because the closed family must break loudly at compile time when a case is added); the `WorkingGraph` is the HAMT live form and `ElementGraph` the frozen read snapshot, `Thaw`/`Freeze` the only crossings, so a mutation never touches the frozen snapshot's incidence index or memo (a new snapshot is built once at `Freeze`); the `WorkingGraph` carries no header (the model header rides the `GraphDelta` event and the frozen `ElementGraph`, never the working form), so a `Reheader` mutation records the header on the delta and `Freeze`/`ReplayOnto` resolve it as `delta.Header.IfNone(graphHeader)`; the structural edge law `LegalLink` enforces ONLY the seam's structural invariants (endpoint-kind legality), the IFC-semantic legality (containment-relating-must-be-spatial, `Void` element→opening, type-may-not-aggregate-occurrence) being the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` `Validation<Error,Unit>`, so the seam carries no IFC vocabulary; the `GraphDelta` `Merge` is a SEQUENTIAL-COMPOSITION monoid producing the unique-per-id normal form — a naive set-union concatenation is the deleted form on TWO counts: `ReplayOnto`'s remove-before-set order would resurrect a node added-then-removed (or revised-then-removed) in one segment, AND a node touched twice would occupy two slots and write its id TWICE in `ToCanonicalBytes`, forking the content key off a structurally-identical single-touch delta and breaking the dedup/op-identity contract; so `Merge` drops this's pending adds and revises that next removes, cancels a removal that targets one of this's pending adds, AND coalesces a surviving double-touch (add-then-revise into one add of the latest, revise-then-revise into one revise spanning this.Before→next.After), making the `Batch` accumulation and the Persistence stream compaction faithful AND content-addressable; a `DropNode` cascades its incident edges so the graph never dangles, and a drop-absent or a duplicate link rails `ElementFault.DeltaConflict`, never a silent no-op; the `GraphDelta` is the event body (the change, not the whole graph) and `ReplayOnto` the persistence fold, so a stream of deltas rebuilds any snapshot and the whole-graph-per-event bloat is the deleted form; `ReplayOnto` re-applies RAW (the delta was validated when produced) while `AdmitOnto` routes a projector-built delta through `WorkingGraph.Apply` so the structural `LegalLink` law runs at the projection boundary — the two share the thaw→apply→freeze crossing but split on validation, and a projector path that called `ReplayOnto` (skipping `LegalLink`) instead of `AdmitOnto` is the deleted form.

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

 // The sequential-composition monoid append (this THEN next), the invariant being that EVERY id lands in AT MOST ONE of
 // {added, revised} (the lone deliberate cross-slot is remove-then-add — removed PLUS added — the faithful erase-then-
 // recreate, a different net op than a revise because the erase cascades the node's edges). One faithful entry per
 // touched node makes the order-independent ToCanonicalBytes content key well-defined; an id written twice into one slot
 // list forks the key and breaks the dedup/op-identity contract. Three coalescences enforce it. (1) CANCELLATION: next's
 // removals (`killed`) drop this's pending adds and revises so ReplayOnto's remove-before-set order can never resurrect
 // them, and next's removal that targets one of this's pending adds (`addedIds`) nets to nothing (the add+remove cancel —
 // the node never reaches the base, neither entry survives); a revised-then-removed node keeps only next's removal entry
 // (its stale revise dropped — never both, which would double-count). (2) ADD-ABSORBS-REVISE: a node this ADDS and next
 // REVISES stays ONE add carrying next's after-value (`reviseOf` looks the revise up, the add re-points to it), because
 // the base sees a single set of the latest value — next's standalone revise of that id is then suppressed by the
 // `addedIds` filter. (3) REVISE-COALESCE: a node this REVISES and next REVISES again folds to ONE revise (this.Before,
 // next.After), the intermediate value dropped, and next's standalone revise of that id suppressed by the `RevisedNodes`
 // membership filter. Edges coalesce by structural equality on BOTH axes (the same dedup the node slots get, the
 // edge equality the Relationship [Equatable] supplies): (a) CANCELLATION — next's RemovedEdges drop this's matching
 // AddedEdges (a re-add in next then survives once, re-add-wins), and next's RemovedEdges that target one of this's
 // AddedEdges net to nothing; (b) IDEMPOTENCE — next's AddedEdges that this ALREADY adds are suppressed (`!AddedEdges
 // .Contains`) and next's RemovedEdges that this already removes likewise, so the SAME edge authored by two merged
 // segments (two independent projector deltas authoring one Associate, a Persistence stream-compaction overlap) lands
 // ONCE, never the [e, e] duplicate the prior endpoints-only union produced — a duplicate edge writes its bytes TWICE
 // in ToCanonicalBytes and forks the key off a structurally-identical single-add delta, breaking the address law. The
 // result satisfies BOTH the replay law a.Merge(b).ReplayOnto(g) == b.ReplayOnto(a.ReplayOnto(g)) AND the address law
 // a.Merge(b).ToCanonicalBytes(t) == the content key of that net graph change; a remove-then-add of one id replays
 // correctly (erase plus its edges, then set).
 public GraphDelta Merge(GraphDelta next) {
  var killed = next.RemovedNodes.ToHashSet();
  var addedIds = AddedNodes.Map(static n => n.Id).ToHashSet();
  // next's revises indexed by id (HashMap<NodeId,Node>, the After value) — the lookup that lets this's surviving add or
  // revise of the SAME id absorb next's revise into ONE entry, so the id never lands in two {added, revised} slots. A
  // HashMap (equality-keyed), NOT a Map (ordered AVL): NodeId carries [KeyMemberEqualityComparer] but no comparison axis.
  var reviseOf = next.RevisedNodes.Map(static r => (r.After.Id, r.After)).ToHashMap();
  return new(
   AddedNodes.Filter(n => !killed.Contains(n.Id)).Map(n => reviseOf.Find(n.Id).IfNone(n)) + next.AddedNodes,
   RemovedNodes + next.RemovedNodes.Filter(id => !addedIds.Contains(id)),
   RevisedNodes.Filter(r => !killed.Contains(r.After.Id)).Map(r => (r.Before, reviseOf.Find(r.After.Id).IfNone(r.After)))
    + next.RevisedNodes.Filter(r => !addedIds.Contains(r.After.Id) && !RevisedNodes.Exists(s => s.After.Id == r.After.Id)),
   AddedEdges.Filter(e => !next.RemovedEdges.Contains(e)) + next.AddedEdges.Filter(e => !AddedEdges.Contains(e)),
   RemovedEdges + next.RemovedEdges.Filter(e => !AddedEdges.Contains(e) && !RemovedEdges.Contains(e)),
   next.Header.IsSome ? next.Header : Header);
 }

 public bool IsEmpty => AddedNodes.IsEmpty && RemovedNodes.IsEmpty && RevisedNodes.IsEmpty && AddedEdges.IsEmpty && RemovedEdges.IsEmpty && Header.IsNone;

 // The change magnitude the Rasm.Persistence Version/ledger snapshot cadence reads to bound replay — the node
 // touch count (added + revised + removed) and the edge touch count (added + removed).
 public int NodeCount => AddedNodes.Count + RevisedNodes.Count + RemovedNodes.Count;
 public int EdgeCount => AddedEdges.Count + RemovedEdges.Count;

 // The standalone ORDER-INDEPENDENT content key a Persistence event dedup and a Version op-identity derive from the delta
 // — the SAME XxHash128 canonical rail the node/edge/graph addresses use, composed through the per-member ToCanonicalBytes
 // under the MODEL tolerance: the node members thread `tolerance` to `Node.ToCanonicalBytes(tolerance)` and the edges thread
 // it to `Relationship.ToCanonicalBytes(tolerance)` (the Relations/relation#EDGE_ALGEBRA tolerance-threaded edge key) so a
 // Generic edge's PropertyValue.Measure attributes quantize to Header.Tolerance on the SAME grid the node measures use — the
 // five typed edge cases carry no Measure and are tolerance-insensitive, but a tolerance-0 edge key would fork a Generic
 // edge whose measures differ below tolerance off a structurally-identical delta. Nodes sort by id and edges by canonical
 // bytes (the Projection/address#CONTENT_ADDRESS OfGraph order-independence law) so two structurally identical deltas with
 // permuted recording order dedup identically; section counts make the layout self-delimiting. Wall-clock-bearing header
 // fields (the Instant At, the StepHeader timestamps) are EXCLUDED so two structurally identical model-creating deltas
 // address identically, but the full GeoReference (origin/rotation/scale/datum/EPSG) contributes so two headers that differ
 // only in map placement do NOT collide.
 public ReadOnlyMemory<byte> ToCanonicalBytes(double tolerance) {
  CanonicalWriter w = new(tolerance);
  w.Ordinal(AddedNodes.Count); foreach (Node n in AddedNodes.OrderBy(static n => n.Id.Value, StringComparer.Ordinal)) { w.String(n.Id.Value).Raw(n.ToCanonicalBytes(tolerance).Span); }
  w.Ordinal(RevisedNodes.Count); foreach (var (_, after) in RevisedNodes.OrderBy(static r => r.After.Id.Value, StringComparer.Ordinal)) { w.String(after.Id.Value).Raw(after.ToCanonicalBytes(tolerance).Span); }
  w.Ordinal(RemovedNodes.Count); foreach (NodeId id in RemovedNodes.OrderBy(static id => id.Value, StringComparer.Ordinal)) { w.String(id.Value); }
  w.Ordinal(AddedEdges.Count); foreach (ReadOnlyMemory<byte> b in AddedEdges.Map(e => e.ToCanonicalBytes(tolerance)).OrderBy(static x => x, ContentAddress.ByteOrder)) { w.Raw(b.Span); }
  w.Ordinal(RemovedEdges.Count); foreach (ReadOnlyMemory<byte> b in RemovedEdges.Map(e => e.ToCanonicalBytes(tolerance)).OrderBy(static x => x, ContentAddress.ByteOrder)) { w.Raw(b.Span); }
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
 // mutations (the ReplayOnto application order) so a Link sees the nodes the same delta adds. The short-circuit fires
 // ONLY for a FULLY empty delta (no node/edge changes AND no header — IsEmpty requires Header.IsNone), the genuine
 // no-op that returns the seed untouched. A header-establishing assembly is NOT this case: Projection/projection#
 // PROJECTION_CONTRACT Assemble SEEDS the fold with GraphDelta.Empty.Reheader(ctx.Header), so even a no-projector (or
 // all-empty-payload) assembly produces a delta with Header.IsSome -> IsEmpty is FALSE -> AdmitOnto proceeds and
 // FREEZES the seed under ctx.Header to ESTABLISH the model header (the model-creating event the Genesis snapshot
 // needs). This is the Projection/projection#PROJECTION_CONTRACT Assemble admission step (the validating counterpart
 // to the raw persistence-rehydrate ReplayOnto).
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
 // legality is the consumer's IGraphConstraint, never here. The object-pair predicate (Compose/Connect/Void all join two
 // Object nodes) is ONE parameterized local — the case keeps its arm for compile-time totality, the predicate and its
 // detail are policy, never three copies of `Relating is Object && Related is Object` differing only by the literal.
 // The Connect REALIZING intermediary (Relations/relation#EDGE_ALGEBRA) is a first-class structural participant — it
 // rides Members/DirectedPairs/the incidence index and the Erase cascade (Touches), and the topology traverses
 // From->Realizing->To — so LegalConnect validates it the SAME as the binary endpoints (present AND an Object) when
 // Some, never the prior endpoints-only check that admitted a Connect whose realizing leg was absent or a non-Object
 // (a Material/PropertySet) and stranded an ill-typed node in the topology view.
 static Fin<Unit> LegalLink(Relationship edge, ImmutableDictionary<NodeId, Node> nodes, Op key) {
  static Fin<Unit> BothObjects(Node relating, Node related, Op key, string detail) =>
   relating is Node.Object && related is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, detail);
  var (relating, related) = edge.Endpoints;
  return !nodes.TryGetValue(relating, out Node? r) ? ElementFault.NodeAbsent(key, $"<link-relating-absent:{relating.Value}>")
   : !nodes.TryGetValue(related, out Node? d) ? ElementFault.NodeAbsent(key, $"<link-related-absent:{related.Value}>")
   : edge.Switch<(Node Relating, Node Related, ImmutableDictionary<NodeId, Node> Nodes, Op Key), Fin<Unit>>(
    (r!, d!, nodes, key),
    compose: static (s, _) => BothObjects(s.Relating, s.Related, s.Key, "<compose-endpoints-must-be-objects>"),
    assign: static (s, a) => LegalAssign(a, s.Relating, s.Related, s.Key),
    associate: static (s, _) => s.Relating is Node.Object && s.Related is (Node.Material or Node.Appearance or Node.Coverage) ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(s.Key, "<associate-resource-must-be-material-appearance-or-coverage>"),
    connect: static (s, c) => LegalConnect(c, s.Relating, s.Related, s.Nodes, s.Key),
    @void: static (s, _) => BothObjects(s.Relating, s.Related, s.Key, "<void-endpoints-must-be-objects>"),
    generic: static (s, _) => Fin.Succ(unit));
 }

 // The Connect structural law: the From/To endpoints are Objects (the BothObjects pair the binary edges share), AND
 // the optional realizing intermediary — when present — resolves in the graph AND is an Object, because a realizing
 // node rides the topology From->Realizing->To leg and the Erase cascade, so an absent realizing id is a NodeAbsent
 // and a non-Object realizing node a RelationshipInvalid (the SAME endpoint-presence + endpoint-kind law the binary
 // endpoints get). An absent realizing leg (None) is a plain binary connection and passes on the endpoint pair alone.
 static Fin<Unit> LegalConnect(Relationship.Connect c, Node from, Node to, ImmutableDictionary<NodeId, Node> nodes, Op key) =>
  from is not Node.Object || to is not Node.Object
   ? ElementFault.RelationshipInvalid(key, "<connect-endpoints-must-be-objects>")
   : c.Realizing.Match(
      None: () => Fin.Succ(unit),
      Some: realizing => !nodes.TryGetValue(realizing, out Node? n)
       ? ElementFault.NodeAbsent(key, $"<connect-realizing-absent:{realizing.Value}>")
       : n is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<connect-realizing-must-be-object>"));

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

- [DELTA_EVENT_BODY]: the `GraphDelta` (not a whole-graph snapshot) is the Marten event body — the stream grain is one stream PER MODEL (or spatial partition), the event the delta, and the inline `SingleStreamProjection` folds `GraphDelta → ElementGraph` through `ReplayOnto` with the periodic Marten snapshot (`Projections.Snapshot<T>(SnapshotLifecycle.Inline)`) bounding replay; a model-creating delta carries the `Header` (authored by the `Reheader` mutation or the `GraphDelta.Reheader` builder) so `ReplayOnto` onto `Genesis` establishes release/view/georeference/tolerance from the first event; the `ToCanonicalBytes` content key is ORDER-INDEPENDENT (nodes sorted by id, edges by canonical bytes, the full `Geospatial/reference#GEO_REFERENCE` `GeoReference` folded into the header contribution) on the one `XxHash128` seed-zero rail the `Projection/address#CONTENT_ADDRESS` graph address uses, so a duplicated or recording-order-permuted delta dedups by content — the `Rasm.Persistence` `Element/graph` Marten stream stores the delta as the `GraphEvent` body (the inline `GraphProjection` folding it through `ReplayOnto`, the periodic Marten snapshot bounding replay), the `Version/ledger` op-log/snapshot-cadence engine projecting FROM those committed events, this owner producing the delta the stream stores.
- [DELTA_MONOID]: the `GraphDelta` `Merge` is a LEFT-FOLD-FAITHFUL sequential composition (`Empty` the two-sided identity, `Merge` the `this`-THEN-`next` compose) whose result is the UNIQUE-PER-ID normal form — every node id lands in AT MOST ONE of `{added, revised}` (remove-then-add the lone deliberate removed-plus-added pair, the faithful erase-then-recreate whose erase cascades the node's edges) — so it satisfies BOTH the two-segment replay law `a.Merge(b).ReplayOnto(g) == b.ReplayOnto(a.ReplayOnto(g))` AND the address law `a.Merge(b).ToCanonicalBytes(t)` equals the content key of that net graph change, the pair that makes a `Batch` mutation accumulate ONE faithful delta and a Persistence stream segment compact to one cumulative change that DEDUPS by content. The compose is sequential, NOT fully associative: a three-segment edge re-add `add(e) ∘ remove(e) ∘ add(e)` nets `{e}` under the LEFT association (the cancellation arm sees the prior add, then the trailing add re-adds) but `{}` under the RIGHT — so the law claimed is the LEFT-fold faithfulness the only two accumulation paths use (`Batch`'s `Fold` and the Persistence stream compaction both fold strictly left, the accumulator on the left of every `Merge`), never an unqualified monoid associativity the order-sensitive edge cancellation does not satisfy; right-association is unreachable, so the production normal form is always the left-fold result. Two failures the naive set-union concatenation hits, both closed here. (1) RESURRECTION: `ReplayOnto` erases removed nodes BEFORE setting added/revised ones, so a node added-then-removed (or revised-then-removed) in one segment would be resurrected — `Merge` DROPS this's pending adds and revises that next removes, cancels next's removal that targets one of this's pending adds (add+removal net to nothing, the node never reaching the base), and keeps only next's removal entry for a revised-then-removed node (its stale revise dropped — never both). (2) DOUBLE-COUNT: a node this touches and next touches AGAIN would otherwise occupy two slots and write its id TWICE in `ToCanonicalBytes`, forking the key off a structurally-identical single-touch delta — so a node this ADDS and next REVISES coalesces to ONE add carrying next's after-value (the base sees one set of the latest), and a node this REVISES and next REVISES folds to ONE revise `(this.Before, next.After)` (the intermediate dropped); next's standalone revise of either id is then suppressed. Edges coalesce by structural equality (the `Relationship` `[Equatable]` content equality) on BOTH axes the node slots get — CANCELLATION (a remove in next drops a matching add in this, an add+removal of one edge nets to nothing) AND IDEMPOTENCE (the SAME edge added — or removed — by two merged segments lands ONCE, `!AddedEdges.Contains`/`!RemovedEdges.Contains` suppressing the duplicate), because an `Assemble` fold over two INDEPENDENT projector deltas (or a Persistence stream-compaction overlap) can author one edge twice and a `[e, e]` duplicate would write `e`'s bytes twice in `ToCanonicalBytes`, forking the key off a single-add delta — the same double-count failure the node arms close, closed symmetrically for edges. A remove-then-add of one id is kept on both lists (an erase plus its edges, then a fresh set) and replays correctly; because every surviving id is single-slot AND every surviving edge single-entry, the content key is well-defined rather than relying on a last-write-wins replay to paper over a double-written id or edge.
- [STRUCTURAL_VS_SEMANTIC]: the seam's `LegalLink` enforces ONLY the structural edge law through the GENERATED total `Switch` over the closed edge algebra — endpoint presence and endpoint-kind legality (`Compose`/`Connect`/`Void` join objects with a `Connect`'s optional realizing intermediary validated present-and-an-`Object` the SAME as its binary endpoints — it rides the topology `From→Realizing→To` leg and the `Erase` cascade, so an absent realizing leg rails `NodeAbsent` and a non-`Object` one `RelationshipInvalid`, the endpoints-only check that stranded it the deleted form — `Associate` binds a material/appearance/coverage resource, `Assign` targets a definition by its `AssignKind`) — railing `ElementFault.RelationshipInvalid`, a C# type-pattern `switch` with a runtime-silent `_` default arm being the deleted form because the closed family must break loudly at compile time when a case is added; the IFC-semantic legality (containment-relating-must-be-spatial, `Void` must be element→opening, a `Type` object may not aggregate an `Occurrence`) is the `Rasm.Bim`-implemented `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint.Validate` returning `Validation<Error,Unit>`, composed AFTER the structural law at the projector boundary; the two-interface split has the seam carrying the structural floor and the Bim constraint the schema legality, so the seam never names an IFC entity or relationship.

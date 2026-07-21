# [ELEMENT_DELTA]

The ONE graph-mutation owner: a `GraphMutation` `[Union]` request (`PutNode`/`DropNode`/`Link`/`Unlink`/`Reheader`/`Batch`) applied through the generated total `Switch` to a HAMT `WorkingGraph` producing a `GraphDelta` — the single immutable change record that IS the persistable event body. This is the live-authoring counterpart to the frozen `Graph/element#ELEMENT_GRAPH` read snapshot: the `WorkingGraph` is an `ImmutableDictionary` HAMT over nodes plus an `ImmutableList` of edges (O(log n) structural sharing per edit), `Thaw` lowers a frozen `ElementGraph` into it, `Freeze` lifts it back to a read snapshot (rebuilding the incidence index and the `QuikGraph` view once). Every mutation enforces the seam's STRUCTURAL edge law through the generated total `Switch` over the `Relations/relation#EDGE_ALGEBRA` cases — a `Link` validates endpoint presence, non-`Generic` irreflexivity, and endpoint-kind legality (`Compose`/`Connect`/`Void` join objects, a `Connect`'s optional realizing intermediary validated the SAME as its binary endpoints and distinct from both, `Associate` binds a material/appearance/coverage resource, `Assign` targets a definition by its sub-kind), railing `Projection/fault#FAULT_BAND` `ElementFault.RelationshipInvalid`; a `DropNode` cascades its incident edges so the graph never dangles; a duplicate link or a drop-absent rails `ElementFault.DeltaConflict` — structural invariants ONLY, the IFC-semantic legality being the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint`. `Merge` is the left-fold-faithful sequential compactor used by `Batch` and persistence compaction; its cancellation-and-coalescing append produces a UNIQUE-PER-ID normal form (every node id in at most one of `{added, revised}`, remove-then-add the lone deliberate removed-plus-added pair), so BOTH the ordered replay law `a.Merge(b).ReplayOnto(g) == b.ReplayOnto(a.ReplayOnto(g))` AND the address law (`ToCanonicalBytes` is the well-defined content key of the net change) hold without an unproved associativity claim. A `Reheader` mutation (mirrored by the `GraphDelta.Reheader` projector builder) establishes the model header on the model-creating event, so the interactive authoring path sets release/view/georeference/tolerance through the SAME algebra the projector uses, and `ReplayOnto` folds a delta into a frozen graph — the persistence path where Marten stores the `GraphDelta` event and the inline projection folds `GraphDelta → ElementGraph`, never a whole-graph snapshot per event — while `AdmitOnto` is the structural-VALIDATING sibling that routes a projector-built delta through `WorkingGraph.Apply` (the `LegalLink` law per `Link`) for the `Projection/projection#PROJECTION_CONTRACT` `Assemble` admission. The page composes `Graph/element#ELEMENT_GRAPH` for the frozen snapshot, `Relations/relation#EDGE_ALGEBRA` for the edge endpoints, and `Projection/address#CANONICAL_WRITER` for the order-independent content key; a malformed mutation rails `ElementFault`.

## [01]-[INDEX]

- [01]-[GRAPH_DELTA]: the `WorkingGraph` HAMT live form, the `GraphMutation` `[Union]` request + generated total `Switch`, the structural edge law `LegalLink`/`LegalConnect`/`LegalAssign` over the generated `Switch` (no runtime-default arm), the `Apply` fold, the `GraphDelta` left-fold sequential event body (with the `Put`/`Link`/`Reheader` projector builders, the cancellation-and-coalescing `Merge` — the unique-per-id normal form the `Projection/projection#PROJECTION_CONTRACT` `Project` fold and the Persistence stream compaction compose — the `NodeCount`/`EdgeCount` magnitude reads, and the order-independent `ToCanonicalBytes` delta content key the normal form makes well-defined), the `Diff(before, after)` snapshot subtraction (the normal-form event body between two frozen graphs — the re-import reconcile), the `IsNormalForm` foreign-delta shape gate the `Graph/wire#WIRE_CODEC` decode composes, the `ReplayOnto` raw persistence fold, and its structural-validating projection-admission sibling `AdmitOnto`.

## [02]-[GRAPH_DELTA]

- Owner: `WorkingGraph` the HAMT live-authoring form (`ImmutableDictionary<NodeId, Node>` + `ImmutableList<Relationship>`); `GraphMutation` the `[Union]` mutation request; `GraphDelta` the immutable change record that IS the persistable event body; the structural edge law `LegalLink`/`LegalConnect`/`LegalAssign`.
- Cases: `PutNode` (upsert a node — add if absent, revise if present, recording the before/after) · `DropNode` (remove a node and CASCADE its incident edges) · `Link` (add an edge after validating endpoint presence and structural legality) · `Unlink` (remove an edge by structural equality) · `Reheader` (establish/revise the model header on the model-creating event — the interactive-authoring counterpart to the projector's `GraphDelta.Reheader` builder) · `Batch` (a `Seq<GraphMutation>` folded fail-fast into one accumulated delta); the closed mutation family.
- Entry: `WorkingGraph.Thaw(ElementGraph)` lowers a frozen snapshot into the HAMT form; `Apply(GraphMutation, key)` applies a mutation, `Fin<T>` returning the next `WorkingGraph` plus the `GraphDelta` it produced (railing `ElementFault.NodeAbsent` on a link to an absent endpoint or absent realizing intermediary, `ElementFault.RelationshipInvalid` on an illegal endpoint-kind pair, a non-`Generic` self-loop, or an endpoint-coincident realizing node, `ElementFault.DeltaConflict` on a drop-absent or duplicate link); `Freeze(header)` lifts the HAMT form back to a frozen `ElementGraph` under the resolved header (rebuilding the incidence index and `QuikGraph` view once), the header an interactive session resolves as `accumulatedDelta.Header.IfNone(baseHeader)` — the SAME resolution `ReplayOnto` uses; `GraphDelta.ReplayOnto(ElementGraph)` folds a delta into a frozen graph for the persistence rehydrate (re-applying RAW, the delta validated when produced); `GraphDelta.AdmitOnto(ElementGraph, Op)` is the structural-VALIDATING sibling routing a projector-built delta through `WorkingGraph.Apply` so `LegalLink` runs per `Link` (the `Projection/projection#PROJECTION_CONTRACT` `Assemble` admission step), `Fin<T>` returning the frozen graph plus the re-derived event body and railing `RelationshipInvalid`/`NodeAbsent`/`DeltaConflict` on a structurally-illegal projection; `GraphDelta.Diff(before, after)` subtracts two frozen snapshots into the normal-form delta whose replay law `Diff(a, b).ReplayOnto(a) == b` holds under the `[Equatable]` graph equality — the re-import reconcile and the cross-runtime model-diff lane produce their event body here rather than a per-consumer subtraction.
- Auto: `Apply` dispatches the generated total `Switch`: `PutNode` adds or revises, `DropNode` removes the node and every incident edge, `Link` admits one legal non-duplicate edge, `Unlink` removes one structural match, `Reheader` records only the header, and `Batch` left-folds sub-mutations through `Merge`. `LegalLink` and `LegalAssign` are generated total dispatches with no runtime-default arm. The HAMT preserves structural sharing until `Freeze` materializes the read snapshot.
- Receipt: the `GraphDelta` is the one change record — the Marten event body carrying the added/removed/revised nodes and added/removed edges, NOT a whole-graph snapshot per event; the inline `SingleStreamProjection` folds `GraphDelta → ElementGraph` through `ReplayOnto` so the read snapshot rebuilds from the delta stream, the periodic Marten snapshot (`Projections.Snapshot<T>(SnapshotLifecycle.Inline)`) bounding replay, the cadence reading `NodeCount`/`EdgeCount` for the change magnitude; `ToCanonicalBytes(tolerance)` derives the delta's ORDER-INDEPENDENT content key (the Persistence event dedup and the Version op-identity) on the same `XxHash128` canonical rail the node/edge/graph addresses use — nodes sorted by id, edges by canonical bytes, the section counts self-delimiting the layout and every collection inside the node bytes count-prefixed per the `Projection/address#CANONICAL_WRITER` law (the injectivity precondition of the raw-append `String(id)`+bytes joins), the full `Geospatial/reference#GEO_REFERENCE` `GeoReference` folded into the header contribution — so a re-applied, duplicated, or recording-order-permuted delta is detected by content, never a wall-clock; the `Generator.Equals` member diff (`Graph/element#ELEMENT_GRAPH` `ElementGraph.EqualityComparer.Default.Inequalities`) and the `GraphDelta` are the two change surfaces — the diff for a content-3-way merge, the delta for the forward event log — and `Diff(before, after)` closes the loop between them, deriving the replayable event body a member-path diff report can never be.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` + the generated total `Switch`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Fold`), System.Collections.Immutable/Frozen, `Rasm` (the kernel `Op` op-key).
- Growth: a new mutation is one `GraphMutation` case routed through the total `Switch` (the `Reheader` header-establishing mutation landed exactly this way); a new structural invariant is one arm in the `LegalLink` generated `Switch`; the `GraphDelta` event body grows by column, not by a parallel event type; never a per-node-kind mutation and never a whole-graph snapshot per event.
- Boundary: `GraphMutation` is the ONE request owner, `WorkingGraph` the HAMT live form, `GraphDelta` the event body, and `ElementGraph` the frozen read form. `LegalLink` enforces endpoint presence, typed-edge irreflexivity, and endpoint-kind legality; Bim constraints own IFC semantics. `Merge` is a strict-left sequential compactor, not a generally associative monoid. `ReplayOnto` trusts seam-produced deltas, while `AdmitOnto` replays foreign/projector deltas through structural admission.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Graph;

// --- [MODELS] -----------------------------------------------------------------------------
// The persistable event body — the change, never a whole-graph snapshot. Merge is the faithful left-fold compactor
// used by Batch and stream compaction; no associativity claim exceeds those ordered accumulation paths.
public sealed record GraphDelta(
 Seq<Node> AddedNodes,
 Seq<NodeId> RemovedNodes,
 Seq<(Node Before, Node After)> RevisedNodes,
 Seq<Relationship> AddedEdges,
 Seq<Relationship> RemovedEdges,
 Option<Header> Header) {

 public static readonly GraphDelta Empty = new([], [], [], [], [], None);

 // The left-fold sequential append (this THEN next). Invariant: every surviving node id lands in AT MOST ONE
 // of {added, revised} (remove-then-add is the lone deliberate removed-plus-added pair — the faithful erase-then-recreate,
 // its erase cascading edges) and every surviving edge is single-entry per list, so the order-independent ToCanonicalBytes
 // key is well-defined — a double-written id or edge forks the key off a structurally identical delta. Node coalescences:
 // (1) CANCELLATION — next's removals (`killed`) drop this's pending adds/revises (ReplayOnto's remove-before-set order
 // can never resurrect them); a removal targeting one of this's pending adds (`addedIds`) nets to nothing; a
 // revised-then-removed id keeps only next's removal. (2) ADD-IDEMPOTENCE — a both-sides add keeps ONLY next's entry
 // (next-wins; routine, a non-rooted id IS its content hash). (3) ADD-ABSORBS-REVISE — this's add re-points to next's
 // after-value via `reviseOf`, next's standalone revise suppressed. (4) REVISE-COALESCE — revise-then-revise folds to ONE
 // (this.Before, next.After). Edges coalesce by Relationship [Equatable] content equality on BOTH directions of
 // cancellation AND on idempotence: add-then-removed drops both, removed-then-RE-ADDED (the unlink-relink compaction)
 // ALSO drops both — the pair replays to the identity on the guaranteed-present base (Unlink validated presence when
 // the removal was produced), so keeping it would fork the address off the net change — EXCEPT when a surviving node
 // erase touches the edge (`Entangled`): the erase replays FIRST and cascades the edge physically, so a cancelled
 // re-add would never restore it — the entangled pair stays the deliberate removed-plus-added edge pair (detach a
 // no-op, attach restoring). The RE-ADD exemption keeps next's remove-then-re-add of an edge this added
 // (`|| next.RemovedEdges.Contains(e)`) — idempotent suppression there would erase a live edge from the compacted
 // delta. Node removals carry the matching single-entry guard. Satisfies the replay law
 // a.Merge(b).ReplayOnto(g) == b.ReplayOnto(a.ReplayOnto(g)) AND the address law; [03] DELTA_MONOID carries the
 // LEFT-fold-faithfulness qualification.
 public GraphDelta Merge(GraphDelta next) {
  HashSet<NodeId> killed = next.RemovedNodes.ToHashSet();
  HashSet<NodeId> addedIds = AddedNodes.Map(static n => n.Id).ToHashSet();
  HashSet<NodeId> nextAdds = next.AddedNodes.Map(static n => n.Id).ToHashSet();
  // next's revises indexed by id — a HashMap (equality-keyed), NOT a Map (ordered AVL): NodeId carries
  // [KeyMemberEqualityComparer] but no comparison axis.
  HashMap<NodeId, Node> reviseOf = next.RevisedNodes.Map(static r => (r.After.Id, r.After)).ToHashMap();
  // The merged surviving erase set, computed FIRST — it gates the unlink-relink edge cancellation below.
  Seq<NodeId> removals = RemovedNodes + next.RemovedNodes.Filter(id => !addedIds.Contains(id) && !RemovedNodes.Contains(id));
  bool Entangled(Relationship edge) => removals.Exists(edge.Touches);
  return new(
   AddedNodes.Filter(n => !killed.Contains(n.Id) && !nextAdds.Contains(n.Id)).Map(n => reviseOf.Find(n.Id).IfNone(n)) + next.AddedNodes,
   removals,
   RevisedNodes.Filter(r => !killed.Contains(r.After.Id) && !nextAdds.Contains(r.After.Id)).Map(r => (r.Before, reviseOf.Find(r.After.Id).IfNone(r.After)))
    + next.RevisedNodes.Filter(r => !addedIds.Contains(r.After.Id) && !RevisedNodes.Exists(s => s.After.Id == r.After.Id)),
   AddedEdges.Filter(e => !next.RemovedEdges.Contains(e))
    + next.AddedEdges.Filter(e => (!AddedEdges.Contains(e) || next.RemovedEdges.Contains(e)) && (!RemovedEdges.Contains(e) || Entangled(e))),
   RemovedEdges.Filter(e => !next.AddedEdges.Contains(e) || Entangled(e))
    + next.RemovedEdges.Filter(e => !AddedEdges.Contains(e) && !RemovedEdges.Contains(e)),
   next.Header.IsSome ? next.Header : Header);
 }

 public bool IsEmpty => AddedNodes.IsEmpty && RemovedNodes.IsEmpty && RevisedNodes.IsEmpty && AddedEdges.IsEmpty && RemovedEdges.IsEmpty && Header.IsNone;

 // The foreign-delta SHAPE gate the wire boundary composes: TRUE iff every set-side node id occupies at most one of
 // {added, revised}, no id is both revised and removed (remove-then-add stays the lone deliberate removed-plus-added
 // pair), and every edge list is single-entry — the unique-per-id normal form Merge produces and ToCanonicalBytes
 // presupposes. A GraphDeltaWire transcription re-crosses this predicate (ElementWire rails DeltaConflict on a
 // double-entry payload) and then applies ONLY through AdmitOnto; ReplayOnto trusts a delta THIS algebra produced.
 public bool IsNormalForm {
  get {
   Seq<NodeId> setIds = AddedNodes.Map(static n => n.Id) + RevisedNodes.Map(static r => r.After.Id);
   return setIds.ToHashSet().Count == setIds.Count
    && RemovedNodes.ToHashSet().Count == RemovedNodes.Count
    && RevisedNodes.ForAll(static revision => revision.Before.Id == revision.After.Id)
    && RevisedNodes.ForAll(r => !RemovedNodes.Contains(r.After.Id))
    && AddedEdges.ToHashSet().Count == AddedEdges.Count
    && RemovedEdges.ToHashSet().Count == RemovedEdges.Count
    && AddedEdges.ForAll(edge => !RemovedEdges.Contains(edge) || RemovedNodes.Exists(edge.Touches));
  }
 }

 // The change magnitude the Rasm.Persistence Version/ledger snapshot cadence reads to bound replay — the node
 // touch count (added + revised + removed) and the edge touch count (added + removed).
 public int NodeCount => AddedNodes.Count + RevisedNodes.Count + RemovedNodes.Count;
 public int EdgeCount => AddedEdges.Count + RemovedEdges.Count;

 // The ORDER-INDEPENDENT delta content key (Persistence event dedup, Version op-identity) on the SAME XxHash128 canonical
 // rail as the node/edge/graph addresses: nodes sort by id, edges by canonical bytes, section counts self-delimit the
 // layout, so a duplicated or recording-order-permuted delta dedups by content. The String(id).Raw(nodeBytes) runs stay
 // injective BECAUSE every collection inside Node.ToCanonicalBytes is count-prefixed per the
 // Projection/address#CANONICAL_WRITER law — an uncounted trailing bag run absorbing the following node's String(id)
 // bytes (two distinct deltas, one hash) is the named deleted form. `tolerance` threads to BOTH the node and
 // the edge ToCanonicalBytes so a Generic edge's PropertyValue.Measure attributes quantize on the node grid — a
 // tolerance-0 edge key would fork a Generic edge differing below tolerance. The header contribution excludes wall-clock
 // provenance and folds the full GeoReference (Header.CanonicalBytes owns the projection).
 public ReadOnlyMemory<byte> ToCanonicalBytes(double tolerance) {
  CanonicalWriter w = new(tolerance);
  w.Ordinal(AddedNodes.Count); foreach (Node n in AddedNodes.OrderBy(static n => n.Id.Value, StringComparer.Ordinal)) { w.String(n.Id.Value).Raw(n.ToCanonicalBytes(tolerance).Span); }
  w.Ordinal(RevisedNodes.Count); foreach ((Node _, Node after) in RevisedNodes.OrderBy(static r => r.After.Id.Value, StringComparer.Ordinal)) { w.String(after.Id.Value).Raw(after.ToCanonicalBytes(tolerance).Span); }
  w.Ordinal(RemovedNodes.Count); foreach (NodeId id in RemovedNodes.OrderBy(static id => id.Value, StringComparer.Ordinal)) { w.String(id.Value); }
  w.Ordinal(AddedEdges.Count); foreach (ReadOnlyMemory<byte> b in AddedEdges.Map(e => e.ToCanonicalBytes(tolerance)).OrderBy(static x => x, ContentAddress.ByteOrder)) { w.Raw(b.Span); }
  w.Ordinal(RemovedEdges.Count); foreach (ReadOnlyMemory<byte> b in RemovedEdges.Map(e => e.ToCanonicalBytes(tolerance)).OrderBy(static x => x, ContentAddress.ByteOrder)) { w.Raw(b.Span); }
  w.Bool(Header.IsSome);
  // The ONE Graph/element#ELEMENT_GRAPH Header.CanonicalBytes projection — the SAME bytes the
  // Projection/address#CONTENT_ADDRESS OfGraph snapshot header key composes, never re-spelled here.
  Header.IfSome(h => h.CanonicalBytes(w));
  return w.ToBytes();
 }

 // Projector builders — an IElementProjection.Project fold accumulates authored nodes/edges/header onto the running
 // delta (the seam-owned composition path beside the GraphMutation/Apply authoring path). A Node carries its own
 // content-addressed NodeId, so Put needs no separate key; Put/Link/Reheader echo the GraphMutation case names; the
 // Production accumulation is a strict left fold through Merge.
 public GraphDelta Put(Node node) => Merge(GraphDelta.Empty with { AddedNodes = [node] });
 public GraphDelta Link(Relationship edge) => Merge(GraphDelta.Empty with { AddedEdges = [edge] });
 public GraphDelta Reheader(Header header) => this with { Header = Some(header) };

 // Snapshot SUBTRACTION — the normal-form delta between two frozen graphs, the inverse the re-import reconcile and
 // the cross-runtime model-diff lane compose: a revised IFC re-ingested through Assemble yields a fresh graph, and
 // Diff(stored, reassembled) IS the event body the stream appends — a re-import becomes an ordinary event, and the
 // stream compaction gains the ground truth Diff(a, b).ReplayOnto(a) == b under the [Equatable] graph equality.
 // Node change detection is the Node [Equatable] structural equality over the id-keyed maps; a removed node's
 // incident edges are NOT re-issued (the erase cascade owns them — the AdmitOnto re-issue filter, mirrored), so the
 // delta is minimal; the header contributes only when the two differ. Normal-form by construction: each id in
 // exactly one slot, each edge single-entry.
 public static GraphDelta Diff(ElementGraph before, ElementGraph after) {
  Seq<NodeId> removed = toSeq(before.Nodes.Keys).Filter(id => !after.Nodes.ContainsKey(id));
  Seq<Node> added = toSeq(after.Nodes.Values).Filter(n => !before.Nodes.ContainsKey(n.Id));
  Seq<(Node Before, Node After)> revised = toSeq(after.Nodes.Values)
   .Choose(n => before.Nodes.TryGetValue(n.Id, out Node? prior) && prior is { } existing && !Equals(existing, n) ? Some((Before: existing, After: n)) : None);
  HashSet<Relationship> beforeEdges = before.Edges.ToHashSet();
  HashSet<Relationship> afterEdges = after.Edges.ToHashSet();
  return new(
   added, removed, revised,
   toSeq(after.Edges).Filter(e => !beforeEdges.Contains(e)),
   toSeq(before.Edges).Filter(e => !afterEdges.Contains(e) && !removed.Exists(e.Touches)),
   before.Header.Equals(after.Header) ? None : Some(after.Header));
 }

 // The persistence rehydrate fold: replay a delta onto a frozen snapshot. The delta was validated
 // when produced, so replay re-applies raw — thaw, apply the recorded changes, freeze under the delta's own
 // header when it carries one (the model-establishing event) else the existing graph header. Removed nodes erase
 // FIRST (cascading their edges), then sets, so the cancellation-correct Merge never leaves a remove+set on one id.
 public ElementGraph ReplayOnto(ElementGraph graph) {
  WorkingGraph working = WorkingGraph.Thaw(graph);
  working = RemovedNodes.Fold(working, static (w, id) => w.Erase(id));
  working = AddedNodes.Fold(working, static (w, n) => w.Set(n));
  working = RevisedNodes.Fold(working, static (w, r) => w.Set(r.After));
  working = RemovedEdges.Fold(working, static (w, e) => w.Detach(e));
  working = AddedEdges.Fold(working, static (w, e) => w.Attach(e));
  return working.Freeze(Header.IfNone(graph.Header));
 }

 // The structural-VALIDATING sibling of ReplayOnto: a projector builds its delta through the Put/Link/Reheader builders,
 // so LegalLink has NOT run — AdmitOnto routes the changes through WorkingGraph.Apply (LegalLink per Link), node
 // mutations before edge mutations (the ReplayOnto order, so a Link sees the nodes the same delta adds), freezing under
 // the resolved header and carrying it onto the re-derived event body. The short-circuit fires ONLY for a FULLY empty
 // delta (IsEmpty requires Header.IsNone): Assemble seeds its fold with GraphDelta.Empty.Reheader(ctx.Header), so even a
 // no-projector assembly has Header.IsSome and proceeds to FREEZE the seed under ctx.Header — the model-creating event.
 // A removed edge incident to a removed node is NOT re-issued as an Unlink (the DropNode cascade erases and re-records
 // it; a second Unlink would spuriously DeltaConflict), so only the pure edge removals re-issue.
 public Fin<(ElementGraph Graph, GraphDelta Delta)> AdmitOnto(ElementGraph graph, Op key) =>
  IsEmpty
   ? Fin.Succ((graph, GraphDelta.Empty))
   : WorkingGraph.Thaw(graph)
    .Apply(new GraphMutation.Batch(
     RemovedNodes.Map(static id => (GraphMutation)new GraphMutation.DropNode(id))
     + AddedNodes.Map(static node => (GraphMutation)new GraphMutation.PutNode(node))
     + RevisedNodes.Map(static revision => (GraphMutation)new GraphMutation.PutNode(revision.After))
     + RemovedEdges.Filter(edge => !RemovedNodes.Exists(edge.Touches)).Map(static edge => (GraphMutation)new GraphMutation.Unlink(edge))
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
 // The DropNode cascade keys on Relationship.Touches (Members), so it drops EVERY edge tied to the id — the binary
 // endpoints, a Connect's realizing intermediary, AND a Generic edge whose only tie is a PropertyValue.Reference buried
 // in its attributes. Cascade, never strip-the-attribute: the closed GraphMutation family carries no edge-attribute
 // mutation op, and a surviving Generic edge with a dangling buried Reference is exactly the asymmetry Members closes.
 internal WorkingGraph Erase(NodeId id) => this with { Nodes = Nodes.Remove(id), Edges = Edges.RemoveAll(e => e.Touches(id)) };
 internal WorkingGraph Attach(Relationship edge) => this with { Edges = Edges.Add(edge) };
 internal WorkingGraph Detach(Relationship edge) => this with { Edges = Edges.Remove(edge) };

 public Fin<(WorkingGraph Graph, GraphDelta Delta)> Apply(GraphMutation mutation, Op key) =>
  mutation.Switch<(WorkingGraph Graph, Op Key), Fin<(WorkingGraph, GraphDelta)>>(
   (this, key),
   putNode: static (s, m) => Fin.Succ(s.Graph.Nodes.TryGetValue(m.Node.Id, out Node? prior)
    ? Equals(prior, m.Node)
     ? (s.Graph, GraphDelta.Empty)
     : (s.Graph.Set(m.Node), GraphDelta.Empty with { RevisedNodes = [(prior!, m.Node)] })
    : (s.Graph.Set(m.Node), GraphDelta.Empty with { AddedNodes = [m.Node] })),
   dropNode: static (s, m) => s.Graph.Nodes.ContainsKey(m.Id)
    ? Fin.Succ((s.Graph.Erase(m.Id), GraphDelta.Empty with { RemovedNodes = [m.Id], RemovedEdges = toSeq(s.Graph.Edges).Filter(e => e.Touches(m.Id)) }))
    : ElementFault.DeltaConflict(s.Key, $"<drop-absent-node:{m.Id.Value}>"),
   link: static (s, m) => LegalLink(m.Edge, s.Graph.Nodes, s.Key)
    .Bind(_ => s.Graph.Edges.Contains(m.Edge)
     ? ElementFault.DeltaConflict(s.Key, "<duplicate-link>")
     : Fin.Succ((s.Graph.Attach(m.Edge), GraphDelta.Empty with { AddedEdges = [m.Edge] }))),
   unlink: static (s, m) => s.Graph.Edges.Contains(m.Edge)
    ? Fin.Succ((s.Graph.Detach(m.Edge), GraphDelta.Empty with { RemovedEdges = [m.Edge] }))
    : ElementFault.DeltaConflict(s.Key, "<unlink-absent-edge>"),
   reheader: static (s, m) => Fin.Succ((s.Graph, GraphDelta.Empty with { Header = Some(m.Header) })),
   batch: static (s, m) => m.Mutations.Fold(
    Fin.Succ((Graph: s.Graph, Delta: GraphDelta.Empty)),
    (acc, next) => acc.Bind(state => state.Graph.Apply(next, s.Key).Map(step => (step.Graph, state.Delta.Merge(step.Delta))))));

 // The object-pair predicate Compose/Connect/Void share — ONE parameterized policy local (each case keeps its arm for
 // compile-time totality; the detail literal is the only variation), never three copies of the type test.
 static Fin<Unit> BothObjects(Node relating, Node related, Op key, string detail) =>
  relating is Node.Object && related is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, detail);

 // The seam's STRUCTURAL edge law: endpoint presence + non-Generic irreflexivity + endpoint-kind legality ONLY,
 // dispatched through the generated total Switch over the closed edge algebra (compile-time exhaustive, NO
 // runtime-default arm); the IFC-semantic legality is the consumer's IGraphConstraint, never here. Every typed kind
 // is IRREFLEXIVE (IFC schema WRs forbid self-aggregation/nesting/connection/typing/grouping; a self-loop would
 // double-enter Members and fork the incidence/DirectedPairs topology) — only the Generic passthrough stays
 // self-permissive. The Connect REALIZING intermediary is a first-class structural participant (it rides
 // Members/DirectedPairs, the incidence index, and the Erase cascade), so LegalConnect validates it the SAME as the
 // binary endpoints — present, an Object, AND distinct from both — when Some.
 static Fin<Unit> LegalLink(Relationship edge, ImmutableDictionary<NodeId, Node> nodes, Op key) {
  (NodeId relating, NodeId related) = edge.Endpoints;
  Option<NodeId> absent = edge.Members.Find(member => !nodes.ContainsKey(member));
  return absent.IsSome ? ElementFault.NodeAbsent(key, $"<link-member-absent:{absent.IfNone(default(NodeId)!).Value}>")
   : !nodes.TryGetValue(relating, out Node? r) ? ElementFault.NodeAbsent(key, $"<link-relating-absent:{relating.Value}>")
   : !nodes.TryGetValue(related, out Node? d) ? ElementFault.NodeAbsent(key, $"<link-related-absent:{related.Value}>")
   : relating == related && edge is not Relationship.Generic ? ElementFault.RelationshipInvalid(key, $"<link-self-loop:{relating.Value}>")
   : edge.Switch<(Node Relating, Node Related, ImmutableDictionary<NodeId, Node> Nodes, Op Key), Fin<Unit>>(
    (r!, d!, nodes, key),
    compose: static (s, _) => BothObjects(s.Relating, s.Related, s.Key, "<compose-endpoints-must-be-objects>"),
    assign: static (s, a) => LegalAssign(a, s.Relating, s.Related, s.Key),
    associate: static (s, _) => s.Relating is Node.Object && s.Related is (Node.Material or Node.Appearance or Node.Coverage) ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(s.Key, "<associate-resource-must-be-material-appearance-or-coverage>"),
    connect: static (s, c) => LegalConnect(c, s.Relating, s.Related, s.Nodes, s.Key),
    @void: static (s, _) => BothObjects(s.Relating, s.Related, s.Key, "<void-endpoints-must-be-objects>"),
    generic: static (s, _) => Fin.Succ(unit));
 }

 // The Connect law: BothObjects on From/To, then the optional realizing intermediary — when Some — is DISTINCT from
 // both endpoints (a coincident realizing node would duplicate Members and collapse the From→Realizing→To legs),
 // resolves in the graph, AND is an Object (coincident/non-Object rails RelationshipInvalid, absent rails NodeAbsent);
 // None is a plain binary connection passing on the endpoint pair alone.
 static Fin<Unit> LegalConnect(Relationship.Connect c, Node from, Node to, ImmutableDictionary<NodeId, Node> nodes, Op key) =>
  BothObjects(from, to, key, "<connect-endpoints-must-be-objects>")
   .Bind(_ => c.Realizing.Match(
     None: () => Fin.Succ(unit),
     Some: realizing => realizing == c.From || realizing == c.To
      ? ElementFault.RelationshipInvalid(key, $"<connect-realizing-must-be-distinct:{realizing.Value}>")
      : !nodes.TryGetValue(realizing, out Node? n)
       ? ElementFault.NodeAbsent(key, $"<connect-realizing-absent:{realizing.Value}>")
       : n is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<connect-realizing-must-be-object>")));

 static Fin<Unit> LegalAssign(Relationship.Assign a, Node subject, Node definition, Op key) =>
  subject is not Node.Object ? ElementFault.RelationshipInvalid(key, "<assign-subject-must-be-object>")
  : a.SubKind.Switch(
   propertyDefinition: () => definition is Node.PropertySet or Node.QuantitySet ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-property-definition-must-target-bag>"),
   typeDefinition: () => definition is Node.Object o && o.Kind == ObjectKind.Type ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-type-definition-must-target-type-object>"),
   group: () => definition is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-group-must-target-object>"),
   assessment: () => definition is Node.Assessment ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-assessment-must-target-assessment>"));
}
```

## [03]-[IMPLEMENTATION_LAW]

- [DELTA_EVENT_BODY]: `GraphDelta` is the persistence event body. A creating delta carries the model header, `ReplayOnto` folds events into snapshots, and periodic persistence snapshots bound replay. `ToCanonicalBytes` sorts node and edge contributions and includes the semantic header, so recording order does not fork the event address.
- [DELTA_MONOID]: `Merge` is the strict-left sequential composition used by `Batch` and stream compaction; it claims no general associativity. Node slots coalesce by id, edge slots coalesce by structural equality, add/remove pairs cancel when replay semantics permit, and node-removal entanglement preserves the edge re-add required after cascade. `IsNormalForm` enforces revision identity, unique slots, and the same entanglement exception before foreign deltas enter the fold.
- [STRUCTURAL_VS_SEMANTIC]: `LegalLink` owns endpoint presence, typed-edge irreflexivity, realizing-node distinctness, and endpoint-kind legality through generated total dispatch. `Generic` remains self-permissive for unmodeled relationships. `IGraphConstraint.Validate` owns IFC semantic legality after the structural gate, so the seam never names IFC entity classes.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

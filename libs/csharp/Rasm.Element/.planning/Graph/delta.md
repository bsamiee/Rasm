# [ELEMENT_DELTA]

`GraphMutation` is the ONE graph-mutation owner: a `[Union]` request applied through the generated total `Switch` to a HAMT `WorkingGraph`, producing the `GraphDelta` — the immutable change record that IS the persistable event body. `WorkingGraph` is the live-authoring counterpart to the frozen `Graph/element#ELEMENT_GRAPH` snapshot: `Thaw` lowers, `Freeze` lifts back, and its `TrackingHashMap` accumulates per-key `Change<Node>` records, so `Diff` reads the added/revised/removed partition off the structure that logged it while `Apply` authors its own per-mutation delta.

Every mutation enforces the seam's STRUCTURAL edge law through the generated total `Switch` over the `Relations/relation#EDGE_ALGEBRA` cases, railing `Projection/fault#FAULT_BAND` `ElementFault` — structural invariants ONLY, IFC-semantic legality being the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint`. `Merge` compacts sequentially through the `NodeSlot.Advance` transition table into the unique-per-id normal form `IsNormalForm` reads back, so the ordered replay law and the `ToCanonicalBytes` address law hold.

`Reheader` establishes the model header on the model-creating event through the SAME algebra the projector's `GraphDelta.Reheader` builder uses; `Rekey` re-stamps every `NodeId` through one map and LOWERS onto the existing delta vocabulary. `ReplayOnto` folds a delta into a frozen graph — Marten stores the `GraphDelta` event, never a whole-graph snapshot per event — and `AdmitOnto` is the structural-validating sibling routing a projector-built delta through `WorkingGraph.Apply` for the `Projection/projection#PROJECTION_CONTRACT` `Assemble` admission.

## [01]-[INDEX]

- [02]-[GRAPH_DELTA]: `WorkingGraph` HAMT live authoring, the `GraphMutation` request family under its structural edge law, and the `GraphDelta` event body — `Merge`, `Diff`, `IsNormalForm`, `ReplayOnto`, `AdmitOnto`, and the order-independent `ToCanonicalBytes` content key.

## [02]-[GRAPH_DELTA]

- Owner: `WorkingGraph` the HAMT live-authoring form (`TrackingHashMap<NodeId, Node>` + `ImmutableList<Relationship>`); `GraphMutation` the `[Union]` mutation request; `GraphDelta` the immutable change record that IS the persistable event body; `NodeSlot` the per-id slot vocabulary whose `Advance` transition table states the coalescence law once; `NodePartition` the three node lists as one value the slot fold projects onto; the structural edge law `LegalLink`/`LegalConnect`/`LegalAssign`.
- Cases: `PutNode` (upsert a node — add if absent, revise if present, recording the before/after) · `DropNode` (remove a node and CASCADE its incident edges) · `Link` (add an edge after validating endpoint presence and structural legality) · `Unlink` (remove an edge by structural equality) · `Rekey` (re-stamp every `NodeId` the graph carries through one map — the endpoint-alignment re-identify) · `Reheader` (establish/revise the model header on the model-creating event — the interactive-authoring counterpart to the projector's `GraphDelta.Reheader` builder) · `Batch` (a `Seq<GraphMutation>` folded fail-fast into one accumulated delta); the closed mutation family.
- Cases: `NodeSlot` closes the per-id positions a delta can hold — `Absent` (untouched) · `Added` (minted) · `Revised` (re-set over a base-present node) · `Erased` (removed with its edges cascaded) · `Recreated` (the lone deliberate erase-then-mint pair on one id).
- Entry: `WorkingGraph.Thaw(ElementGraph)` lowers a frozen snapshot into the HAMT form; `Apply(GraphMutation, key)` applies a mutation, `Fin<T>` returning the next `WorkingGraph` and the `GraphDelta` it produced (railing `ElementFault.NodeAbsent` on a link to an absent endpoint or absent realizing intermediary, `ElementFault.RelationshipInvalid` on an illegal endpoint-kind pair, a non-`Generic` self-loop, or an endpoint-coincident realizing node, `ElementFault.DeltaConflict` on a drop-absent or duplicate link); `Freeze(header)` lifts the HAMT form back to a frozen `ElementGraph` under the resolved header (rebuilding the incidence index and `QuikGraph` view once), every caller resolving that header through the one `GraphDelta.HeaderFor(graph)` member; `GraphDelta.ReplayOnto(ElementGraph)` folds a delta into a frozen graph for the persistence rehydrate (re-applying RAW, the delta validated when produced); `GraphDelta.AdmitOnto(ElementGraph, Op)` is the structural-VALIDATING sibling routing a projector-built delta through `WorkingGraph.Apply` so `LegalLink` runs per `Link` (the `Projection/projection#PROJECTION_CONTRACT` `Assemble` admission step), `Fin<T>` returning the frozen graph and the re-derived event body and railing `RelationshipInvalid`/`NodeAbsent`/`DeltaConflict` on a structurally-illegal projection; `GraphDelta.Diff(before, after)` subtracts two frozen snapshots into the normal-form delta whose replay law `Diff(a, b).ReplayOnto(a) == b` holds under the `[Equatable]` graph equality — the re-import reconcile and the cross-runtime model-diff lane produce their event body here rather than a per-consumer subtraction.
- Auto: `Apply` dispatches the generated total `Switch`: `PutNode` adds or revises, `DropNode` removes the node and every incident edge in one sweep, `Link` admits one legal non-duplicate edge, `Unlink` removes one structural match, `Rekey` rebuilds the graph under the mapped ids and lowers the rewrite onto the existing delta slots, `Reheader` records only the header, and `Batch` left-folds sub-mutations through `Merge`. `LegalLink` and `LegalAssign` are generated total dispatches with no runtime-default arm, so an `AssignKind` row lands its arm here or the dispatch stops compiling. `WorkingGraph` preserves structural sharing on the HAMT until `Freeze` materializes the read snapshot.
- Law: `NodeSlot.Advance` is the SINGLE statement of the node coalescence law — `Merge` folds it over the two sides' claim runs into the unique-per-id normal form (every node id in at most one of `{added, revised}`, remove-then-add the lone deliberate removed-and-added pair), and `IsNormalForm` is exactly the fixpoint test that folding a delta's own claims re-derives its own three node lists, so a normal-form conjunct can never drift from the fold that produces it and the ordered replay law `a.Merge(b).ReplayOnto(g) == b.ReplayOnto(a.ReplayOnto(g))` holds.
- Law: `Rekey` LOWERS and mints no delta slot — a node whose OWN id moves lands as an erase at the old key beside an add at the new one (two distinct ids, never the same-id `Recreated` pair), a node whose id holds but whose buried `PropertyValue.Reference` moved lands as a revision, an untouched node contributes nothing, and every touched edge lands as a removal beside its remapped add because an edge carries no key to revise under.
- Receipt: the `GraphDelta` is the one change record — the Marten event body carrying the added/removed/revised nodes and added/removed edges, NOT a whole-graph snapshot per event; the inline `SingleStreamProjection` folds `GraphDelta → ElementGraph` through `ReplayOnto` so the read snapshot rebuilds from the delta stream, the periodic Marten snapshot (`Projections.Snapshot<T>(SnapshotLifecycle.Inline)`) bounding replay, the cadence reading `NodeCount`/`EdgeCount` for the change magnitude; `ToCanonicalBytes(tolerance)` derives the delta's ORDER-INDEPENDENT content key (the Persistence event dedup and the Version op-identity) on the same `XxHash128` canonical rail the node/edge/graph addresses use — nodes sorted by id, edges by canonical bytes, the section counts self-delimiting the layout and every collection inside the node bytes count-prefixed per the `Projection/address#CANONICAL_WRITER` law (the injectivity precondition of the raw-append `String(id)`+bytes joins), the full `Geospatial/reference#GEO_REFERENCE` `GeoReference` folded into the header contribution — so a re-applied, duplicated, or recording-order-permuted delta is detected by content, never a wall-clock; the `Generator.Equals` member diff (`Graph/element#ELEMENT_GRAPH` `ElementGraph.EqualityComparer.Default.Inequalities`) and the `GraphDelta` are the two change surfaces — the diff for a content-3-way merge, the delta for the forward event log — and `Diff(before, after)` closes the loop between them, deriving the replayable event body a member-path diff report can never be.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` + the generated total `Switch`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Fold`), System.Collections.Immutable/Frozen, `Rasm` (the kernel `Op` op-key).
- Growth: a new mutation is one `GraphMutation` case routed through the total `Switch`; a new coalescence rule is one row in the `NodeSlot.Advance` product pattern, which `IsNormalForm` inherits without an edit; a new structural invariant is one arm in the `LegalLink` generated `Switch`; the `GraphDelta` event body grows by column, not by a parallel event type; never a per-node-kind mutation and never a whole-graph snapshot per event.
- Boundary: `GraphMutation` is the ONE request owner, `WorkingGraph` the HAMT live form, `GraphDelta` the event body, and `ElementGraph` the frozen read form. `LegalLink` enforces endpoint presence, typed-edge irreflexivity, and endpoint-kind legality; Bim constraints own IFC semantics. `Merge` is a strict-left sequential compactor, not a generally associative monoid. `ReplayOnto` trusts seam-produced deltas, while `AdmitOnto` replays foreign/projector deltas through structural admission.
- Boundary: the working EDGE half is an ordered `ImmutableList`, so edge membership (the `Link` duplicate gate, the `Unlink` presence gate) and the `Erase` splice each cost O(edges) — the one cost this owner does not amortize, and it is a batch-shaped cost rather than a read-path one because EVERY degree-keyed, containment, and reachability read is a FROZEN-snapshot read served by `Graph/element#ELEMENT_GRAPH`'s built-once incidence index and `QuikGraph` view. Membership sets and working incidence indexes answer both in O(log edges) and do not land here: the list ORDER is what `Graph/wire#WIRE_CODEC` emits and the corpus fingerprint reproduces, and a hash-ordered set forfeits it under per-process string-hash randomization. `Erase` therefore computes its cascade and its surviving list in ONE sweep and hands the cascaded run back, so `DropNode` never re-filters the edge run to author its delta.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Graph;

// --- [TYPES] ------------------------------------------------------------------------------
// NodeSlot is the position ONE node id occupies in a delta, and Advance is the merge law itself: the surviving slot
// for every (this-side, next-side) pair over the closed family. Merge folds it and IsNormalForm reads it back, so the
// unique-per-id normal form has ONE owner instead of a forward spelling in the fold and a backward conjunct in the
// gate — a coalescence rule added to the table is inherited by the predicate with no second edit.
[Union]
public abstract partial record NodeSlot {
 private NodeSlot() { }
 public sealed record Absent : NodeSlot;                            // this side does not touch the id
 public sealed record Added(Node After) : NodeSlot;                 // minted here
 public sealed record Revised(Node Before, Node After) : NodeSlot;  // re-set over a base-present node
 public sealed record Erased(NodeId Id) : NodeSlot;                 // removed, incident edges cascaded
 public sealed record Recreated(NodeId Id, Node After) : NodeSlot;  // erase-then-mint, the lone removed-plus-added pair

 public static readonly NodeSlot Nothing = new Absent();

 // Advance flattens the transition table as ONE pattern over the slot product (the joint-discriminant form, never a
 // dispatch nested in a dispatch arm). Reading the rows: a next-side ADD is next-wins over a live slot (a non-rooted
 // id IS its content hash, so the payloads agree by construction) and the deliberate erase-then-mint over an erase;
 // a next-side REVISE re-points whatever this side already set and coalesces a revise pair onto one (before, after);
 // a next-side ERASE cancels a pending mint outright, because ReplayOnto's remove-before-set order can never
 // resurrect it, and collapses a recreate back to the plain erase.
 public static NodeSlot Advance(NodeSlot held, NodeSlot next) => (held, next) switch {
  (_, Absent) => held,
  (Absent, _) => next,
  (Erased erased, Added added) => new Recreated(erased.Id, added.After),
  (Recreated recreated, Added added) => new Recreated(recreated.Id, added.After),
  (_, Added added) => new Added(added.After),
  (Added, Revised revised) => new Added(revised.After),
  (Revised opened, Revised revised) => new Revised(opened.Before, revised.After),
  (Recreated recreated, Revised revised) => new Recreated(recreated.Id, revised.After),
  (_, Revised revised) => revised,
  (Added, Erased) => Nothing,
  (Recreated recreated, Erased) => new Erased(recreated.Id),
  (_, Erased erased) => erased,
 };
}

// --- [MODELS] -----------------------------------------------------------------------------
// NodePartition carries the delta's three node lists as ONE value, so the slot fold projects them through a single
// total Switch and no arm can populate one list and forget another. Diff builds its subtraction into the same shape, so the snapshot-read
// partition and the merge-fold partition are one type rather than two structurally identical local tuples.
public readonly record struct NodePartition(
 Seq<Node> Added, Seq<NodeId> Removed, Seq<(Node Before, Node After)> Revised) {
 public static readonly NodePartition Empty = new([], [], []);
}

// GraphDelta is the persistable event body — the change, never a whole-graph snapshot. Merge is the faithful
// left-fold compactor used by Batch and stream compaction; no associativity claim exceeds those ordered paths.
public sealed record GraphDelta(
 Seq<Node> AddedNodes,
 Seq<NodeId> RemovedNodes,
 Seq<(Node Before, Node After)> RevisedNodes,
 Seq<Relationship> AddedEdges,
 Seq<Relationship> RemovedEdges,
 Option<Header> Header) {

 public static readonly GraphDelta Empty = new([], [], [], [], [], None);

 // Every (id, slot) claim this delta's three node lists make, in the SAME order ReplayOnto applies them — erases,
 // then mints, then revisions — so folding a claim run through NodeSlot.Advance reproduces replay semantics exactly.
 Seq<(NodeId Id, NodeSlot Slot)> Claims =>
  RemovedNodes.Map(static id => (Id: id, Slot: (NodeSlot)new NodeSlot.Erased(id)))
  + AddedNodes.Map(static node => (Id: node.Id, Slot: (NodeSlot)new NodeSlot.Added(node)))
  + RevisedNodes.Map(static revision => (Id: revision.After.Id, Slot: (NodeSlot)new NodeSlot.Revised(revision.Before, revision.After)));

 // Fold the claim run onto one slot per id, then project in FIRST-APPEARANCE order so the compacted lists keep the
 // recording order the wire and the corpus fingerprint read; the HashMap answers the per-id fold and the ordered
 // id run answers the projection, so neither hash order nor a quadratic membership probe reaches the output.
 static NodePartition Coalesced(Seq<(NodeId Id, NodeSlot Slot)> claims) {
  HashMap<NodeId, NodeSlot> slots = claims.Fold(
   HashMap<NodeId, NodeSlot>.Empty,
   static (state, claim) => state.AddOrUpdate(claim.Id, held => NodeSlot.Advance(held, claim.Slot), claim.Slot));
  return toSeq(claims.Map(static claim => claim.Id).AsEnumerable().Distinct())
   .Choose(id => slots.Find(id))
   .Fold(NodePartition.Empty, static (partition, slot) => slot.Switch(
    state: partition,
    absent: static (acc, _) => acc,
    added: static (acc, s) => acc with { Added = acc.Added.Add(s.After) },
    revised: static (acc, s) => acc with { Revised = acc.Revised.Add((s.Before, s.After)) },
    erased: static (acc, s) => acc with { Removed = acc.Removed.Add(s.Id) },
    recreated: static (acc, s) => acc with { Removed = acc.Removed.Add(s.Id), Added = acc.Added.Add(s.After) }));
 }

 // Merge is the left-fold sequential append (this THEN next). The node half IS the slot fold: concatenating both sides' claim
 // runs and folding NodeSlot.Advance discharges cancellation, add-idempotence, add-absorbs-revise, revise-coalescing,
 // and the removed-plus-added exemption in one pass, so the surviving id set carries at most one of {added, revised}
 // and the order-independent ToCanonicalBytes key stays well-defined. Edges coalesce by Relationship [Equatable]
 // content equality on BOTH directions of cancellation AND on idempotence: add-then-removed drops both,
 // removed-then-RE-ADDED (the unlink-relink compaction) ALSO drops both — the pair replays to the identity on the
 // guaranteed-present base (Unlink validated presence when the removal was produced), so keeping it would fork the
 // address off the net change — EXCEPT when a surviving node erase touches the edge (`Entangled`): the erase replays
 // FIRST and cascades the edge physically, so a cancelled re-add never restores it, and the entangled pair stays the
 // deliberate removed-plus-added edge pair (detach a no-op, attach restoring). The RE-ADD exemption keeps next's
 // remove-then-re-add of an edge this added (`|| next.RemovedEdges.Contains(e)`), where idempotent suppression would
 // erase a live edge from the compacted delta. Satisfies the replay law
 // a.Merge(b).ReplayOnto(g) == b.ReplayOnto(a.ReplayOnto(g)) AND the address law; [03] DELTA_MONOID carries the
 // LEFT-fold-faithfulness qualification.
 public GraphDelta Merge(GraphDelta next) {
  NodePartition nodes = Coalesced(Claims + next.Claims);
  // Membership, not order, is what the entanglement test asks — a hoisted hash set answers it once per edge instead
  // of re-scanning the removal run for every edge on both sides of the merge.
  System.Collections.Generic.HashSet<NodeId> erased = nodes.Removed.ToHashSet();
  bool Entangled(Relationship edge) => edge.Members.Exists(erased.Contains);
  return new(
   nodes.Added, nodes.Removed, nodes.Revised,
   AddedEdges.Filter(e => !next.RemovedEdges.Contains(e))
    + next.AddedEdges.Filter(e => (!AddedEdges.Contains(e) || next.RemovedEdges.Contains(e)) && (!RemovedEdges.Contains(e) || Entangled(e))),
   RemovedEdges.Filter(e => !next.AddedEdges.Contains(e) || Entangled(e))
    + next.RemovedEdges.Filter(e => !AddedEdges.Contains(e) && !RemovedEdges.Contains(e)),
   next.Header.IsSome ? next.Header : Header);
 }

 public bool IsEmpty => AddedNodes.IsEmpty && RemovedNodes.IsEmpty && RevisedNodes.IsEmpty && AddedEdges.IsEmpty && RemovedEdges.IsEmpty && Header.IsNone;

 // IsNormalForm is the foreign-delta SHAPE gate the wire boundary composes, read off the SAME transition table Merge
 // folds: a delta is in normal form iff folding its own claims re-derives its own three node lists. Every coalescence Advance can
 // perform — a double-written id, a duplicate removal, a revise over an erase — STRICTLY shortens a list, while the
 // deliberate erase-then-mint pair re-expands to the removal and the add it came from, so the three count identities
 // ARE the old conjunct set with no rule restated. A GraphDeltaWire transcription re-crosses this predicate
 // (ElementWire rails DeltaConflict on a double-entry payload) and then applies ONLY through AdmitOnto; ReplayOnto
 // trusts a delta THIS algebra produced.
 public bool IsNormalForm {
  get {
   NodePartition folded = Coalesced(Claims);
   return folded.Added.Count == AddedNodes.Count
    && folded.Removed.Count == RemovedNodes.Count
    && folded.Revised.Count == RevisedNodes.Count
    && RevisedNodes.ForAll(static revision => revision.Before.Id == revision.After.Id)
    && AddedEdges.ToHashSet(EqualityComparer<Relationship>.Default).Count == AddedEdges.Count
    && RemovedEdges.ToHashSet(EqualityComparer<Relationship>.Default).Count == RemovedEdges.Count
    && AddedEdges.ForAll(edge => !RemovedEdges.Contains(edge) || RemovedNodes.Exists(edge.Touches));
  }
 }

 // NodeCount and EdgeCount are the change magnitudes the Rasm.Persistence Version/ledger snapshot cadence reads to
 // bound replay — the node touch count (added + revised + removed) and the edge touch count (added + removed).
 public int NodeCount => AddedNodes.Count + RevisedNodes.Count + RemovedNodes.Count;
 public int EdgeCount => AddedEdges.Count + RemovedEdges.Count;

 // ToCanonicalBytes mints the ORDER-INDEPENDENT delta content key (Persistence event dedup, Version op-identity) on the
 // SAME XxHash128 canonical rail as the node/edge/graph addresses: nodes sort by id, edges by canonical bytes, section
 // counts self-delimit the layout, so a duplicated or recording-order-permuted delta dedups by content. The
 // String(id).Raw(nodeBytes) runs stay injective BECAUSE every collection inside Node.ToCanonicalBytes is count-prefixed
 // per the Projection/address#CANONICAL_WRITER law — an uncounted trailing bag run absorbing the following node's
 // String(id) bytes (two distinct deltas, one hash) is the named deleted form. `tolerance` threads to BOTH the node and
 // edge ToCanonicalBytes so a Generic edge's PropertyValue.Measure attributes quantize on the node grid — a tolerance-0
 // edge key forks a Generic edge differing below tolerance. The header contribution excludes wall-clock provenance and
 // folds the full GeoReference (Header.CanonicalBytes owns the projection).
 public ReadOnlyMemory<byte> ToCanonicalBytes(double tolerance) {
  CanonicalWriter w = new(tolerance);
  w.Ordinal(AddedNodes.Count); foreach (Node n in AddedNodes.OrderBy(static n => n.Id.Value, StringComparer.Ordinal)) { w.String(n.Id.Value).Raw(n.ToCanonicalBytes(tolerance).Span); }
  w.Ordinal(RevisedNodes.Count); foreach ((Node _, Node after) in RevisedNodes.OrderBy(static r => r.After.Id.Value, StringComparer.Ordinal)) { w.String(after.Id.Value).Raw(after.ToCanonicalBytes(tolerance).Span); }
  w.Ordinal(RemovedNodes.Count); foreach (NodeId id in RemovedNodes.OrderBy(static id => id.Value, StringComparer.Ordinal)) { w.String(id.Value); }
  w.Ordinal(AddedEdges.Count); foreach (ReadOnlyMemory<byte> b in AddedEdges.Map(e => e.ToCanonicalBytes(tolerance)).OrderBy(static x => x, ContentAddress.ByteOrder)) { w.Raw(b.Span); }
  w.Ordinal(RemovedEdges.Count); foreach (ReadOnlyMemory<byte> b in RemovedEdges.Map(e => e.ToCanonicalBytes(tolerance)).OrderBy(static x => x, ContentAddress.ByteOrder)) { w.Raw(b.Span); }
  w.Bool(Header.IsSome);
  // Header.CanonicalBytes is the ONE Graph/element#ELEMENT_GRAPH header projection — the SAME bytes the
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

 // Snapshot SUBTRACTION — the normal-form delta between two frozen graphs, the inverse the re-import reconcile and the
 // cross-runtime model-diff lane compose: a revised IFC re-ingested through Assemble yields a fresh graph, and
 // Diff(stored, reassembled) IS the event body the stream appends — a re-import becomes an ordinary event, and the
 // stream compaction gains the ground truth Diff(a, b).ReplayOnto(a) == b under the [Equatable] graph equality.
 // Diff READS the node partition off the working map's own change record, never deriving it a second time: `Snapshot`
 // zeroes the log to make `before` the baseline, the erase-then-set replay morphs it to `after`, and `Changes` then
 // holds exactly the added/mapped/removed classification, projected onto the SAME NodePartition the merge fold builds.
 // Its mapped arm keeps the `[Equatable]` inequality test because the map's own log records a write, not a semantic
 // change, so the delta's minimality law stays this seam's and never rides an assumption about the structure's
 // logging grain. A removed node's incident edges are NOT re-issued (the erase cascade owns them, the same filter
 // AdmitOnto applies), so the delta is minimal; the header contributes only when the two differ. Normal-form by
 // construction: each id in exactly one slot, each edge single-entry. The edge half stays a set subtraction under the
 // generated comparer — the tracking map keys on node id and an edge is not key-shaped, so the structure carries
 // nothing for it.
 public static GraphDelta Diff(ElementGraph before, ElementGraph after) {
  TrackingHashMap<NodeId, Node> baseline = toHashMap(before.Nodes).ToTrackingHashMap().Snapshot();
  TrackingHashMap<NodeId, Node> morphed = toSeq(after.Nodes.Values).Fold(
   toSeq(before.Nodes.Keys).Filter(id => !after.Nodes.ContainsKey(id)).Fold(baseline, static (map, id) => map.Remove(id)),
   static (map, node) => map.AddOrUpdate(node.Id, node));
  // `Changes` is a HashMap, whose two IEnumerable constructions make `toSeq(map)` an inference failure; AsIterable
  // is the pair walk, and its NAMED (Key, Value) element is what the classification arms below read.
  NodePartition partition = morphed.Changes.AsIterable().Fold(NodePartition.Empty, static (acc, entry) => entry.Value switch {
   EntryAdded<Node> a => acc with { Added = acc.Added.Add(a.Value) },
   EntryMapped<Node, Node> m when !EqualityComparer<Node>.Default.Equals(m.From, m.To) => acc with { Revised = acc.Revised.Add((m.From, m.To)) },
   EntryRemoved<Node> => acc with { Removed = acc.Removed.Add(entry.Key) },
   _ => acc,
  });
  System.Collections.Generic.HashSet<Relationship> beforeEdges = before.Edges.ToHashSet(EqualityComparer<Relationship>.Default);
  System.Collections.Generic.HashSet<Relationship> afterEdges = after.Edges.ToHashSet(EqualityComparer<Relationship>.Default);
  return new(
   partition.Added, partition.Removed, partition.Revised,
   toSeq(after.Edges).Filter(e => !beforeEdges.Contains(e)),
   toSeq(before.Edges).Filter(e => !afterEdges.Contains(e) && !partition.Removed.Exists(e.Touches)),
   before.Header.Equals(after.Header) ? None : Some(after.Header));
 }

 // HeaderFor is the ONE header resolution every freeze reads: the delta's own header on a model-establishing event,
 // else the base graph's. ReplayOnto, AdmitOnto, and an interactive session's Freeze all route here, so a second inline projection
 // cannot drift from it.
 public Header HeaderFor(ElementGraph graph) => Header.IfNone(graph.Header);

 // ReplayOnto is the persistence rehydrate fold: replay a delta onto a frozen snapshot. The delta was validated
 // when produced, so replay re-applies raw — thaw, apply the recorded changes, freeze under the resolved header.
 // Removed nodes erase FIRST (cascading their edges), then sets, so the cancellation-correct Merge never leaves a
 // remove+set on one id.
 public ElementGraph ReplayOnto(ElementGraph graph) {
  WorkingGraph working = WorkingGraph.Thaw(graph);
  working = RemovedNodes.Fold(working, static (w, id) => w.Erase(id).Graph);
  working = AddedNodes.Fold(working, static (w, n) => w.Set(n));
  working = RevisedNodes.Fold(working, static (w, r) => w.Set(r.After));
  working = RemovedEdges.Fold(working, static (w, e) => w.Detach(e));
  working = AddedEdges.Fold(working, static (w, e) => w.Attach(e));
  return working.Freeze(HeaderFor(graph));
 }

 // AdmitOnto is the structural-VALIDATING sibling of ReplayOnto: a projector builds its delta through the
 // Put/Link/Reheader builders, so LegalLink has NOT run — AdmitOnto routes the changes through WorkingGraph.Apply
 // (LegalLink per Link), node mutations before edge mutations (the ReplayOnto order, so a Link sees the nodes the same
 // delta adds), freezing under the resolved header and carrying it onto the re-derived event body. The short-circuit
 // fires ONLY for a FULLY empty delta (IsEmpty requires Header.IsNone): Assemble seeds its fold with
 // GraphDelta.Empty.Reheader(ctx.Header), so even a no-projector assembly has Header.IsSome and proceeds to FREEZE the
 // seed under ctx.Header — the model-creating event. Removed edges incident to a removed node are NOT re-issued as
 // Unlinks (the DropNode cascade erases and re-records them; a second Unlink spuriously DeltaConflicts), so only the
 // pure edge removals re-issue.
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
    .Map(step => (step.Graph.Freeze(HeaderFor(graph)), step.Delta with { Header = Header }));
}

[Union]
public abstract partial record GraphMutation {
 private GraphMutation() { }
 public sealed record PutNode(Node Node) : GraphMutation;
 public sealed record DropNode(NodeId Id) : GraphMutation;
 public sealed record Link(Relationship Edge) : GraphMutation;
 public sealed record Unlink(Relationship Edge) : GraphMutation;
 // Rekey is the endpoint-alignment re-stamp: ONE map over every NodeId the graph carries — own ids, edge endpoints, a
 // Connect's realizing intermediary, and every PropertyValue.Reference buried in a bag or a Generic attribute. The
 // map is a POLICY VALUE the caller hands in (the same shape Node.Remap and Relationship.Remap already take), so an
 // unmapped id passes through unchanged and a partial alignment is expressible without a per-node mutation run.
 public sealed record Rekey(Func<NodeId, NodeId> Map) : GraphMutation;
 public sealed record Reheader(Header Header) : GraphMutation;
 public sealed record Batch(Seq<GraphMutation> Mutations) : GraphMutation;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// WorkingGraph is the HAMT live-authoring form — TrackingHashMap nodes + ImmutableList edges, O(log n) per edit.
// Header-free: the model header rides the GraphDelta event and the frozen ElementGraph, never the working form.
//
// TrackingHashMap owns the node half because that structure IS the snapshot delta's node half by construction: the
// same HAMT plus a per-key change record accumulated from the last Snapshot, so Diff reads the added/revised/removed
// partition instead of recomputing it by O(n) membership subtraction. Apply keeps its OWN per-mutation delta and
// does not read the record: a mutation answers a typed conflict (drop-absent, duplicate-link) the change log cannot
// express, so a mutation's evidence is authored where the fault is decided while the snapshot partition is read
// where the whole morph is visible.
//
// ImmutableList keeps the edge half ORDERED and pays O(edges) for membership (the Link duplicate gate, the Unlink
// presence gate) and for the Erase splice — the one unamortized cost in this owner, and a batch-shaped one, because
// every degree-keyed, containment, and reachability read is a FROZEN-snapshot read served by the built-once incidence
// index and QuikGraph view. A membership set or a working incidence index answers both in O(log edges) and does not
// land here: list ORDER is what Graph/wire#WIRE_CODEC emits and the corpus fingerprint reproduces, and a hash-ordered
// set forfeits it, since Generator.Equals hashes through per-process-randomized string hashing.
public sealed record WorkingGraph(TrackingHashMap<NodeId, Node> Nodes, ImmutableList<Relationship> Edges) {
 public static WorkingGraph Thaw(ElementGraph graph) =>
  new(toHashMap(graph.Nodes).ToTrackingHashMap(), graph.Edges.ToImmutableList());

 public ElementGraph Freeze(Header header) =>
  ElementGraph.Of(header, Nodes.ToFrozenDictionary(), [.. Edges]);

 internal WorkingGraph Set(Node node) => this with { Nodes = Nodes.AddOrUpdate(node.Id, node) };

 // DropNode's cascade keys on Relationship.Touches (Members), so it drops EVERY edge tied to the id — the binary
 // endpoints, a Connect's realizing intermediary, AND a Generic edge whose only tie is a PropertyValue.Reference buried
 // in its attributes. Cascade, never strip-the-attribute: the closed GraphMutation family carries no edge-attribute
 // mutation op, and a surviving Generic edge with a dangling buried Reference is exactly the asymmetry Members closes.
 // Erase hands the cascaded run back WITH the graph so the mutation's delta reads the one sweep that produced it; a
 // caller re-filtering the edge list to author RemovedEdges pays the O(edges) scan twice for one drop.
 internal (WorkingGraph Graph, Seq<Relationship> Cascaded) Erase(NodeId id) {
  Seq<Relationship> cascaded = toSeq(Edges).Filter(edge => edge.Touches(id));
  return (this with { Nodes = Nodes.Remove(id), Edges = Edges.RemoveRange(cascaded, EqualityComparer<Relationship>.Default) }, cascaded);
 }

 internal WorkingGraph Attach(Relationship edge) => this with { Edges = Edges.Add(edge) };
 internal WorkingGraph Detach(Relationship edge) => this with { Edges = Edges.Remove(edge, EqualityComparer<Relationship>.Default) };

 // Rekey rebuilds the graph WHOLE under the mapped ids rather than folding Erase per node: an erase cascades incident
 // edges, so a per-node lowering would drop exactly the edges the same rewrite is re-attaching. The delta it authors
 // lowers onto the existing slots — a moved own id is an erase at the old key beside an add at the new one (two
 // distinct ids, so never the same-id Recreated pair), a held id whose buried reference moved is a revision, and a
 // touched edge is a removal beside its remapped add because an edge carries no key to revise under. An untouched
 // node or edge contributes nothing, so a partial alignment produces a minimal delta.
 static (WorkingGraph Graph, GraphDelta Delta) Rekeyed(WorkingGraph graph, Func<NodeId, NodeId> map) {
  Seq<(Node Before, Node After)> nodes = toSeq(graph.Nodes.Values).Map(node => (Before: node, After: node.Remap(map)));
  Seq<(Relationship Before, Relationship After)> edges = toSeq(graph.Edges).Map(edge => (Before: edge, After: edge.Remap(map)));
  Seq<(Node Before, Node After)> rekeyed = nodes.Filter(static pair => pair.Before.Id != pair.After.Id);
  Seq<(Node Before, Node After)> rewritten = nodes.Filter(static pair =>
   pair.Before.Id == pair.After.Id && !EqualityComparer<Node>.Default.Equals(pair.Before, pair.After));
  Seq<(Relationship Before, Relationship After)> relinked = edges.Filter(static pair =>
   !EqualityComparer<Relationship>.Default.Equals(pair.Before, pair.After));
  return (
   new WorkingGraph(
    nodes.Fold(
     rekeyed.Fold(graph.Nodes, static (state, pair) => state.Remove(pair.Before.Id)),
     static (state, pair) => state.AddOrUpdate(pair.After.Id, pair.After)),
    [.. edges.Map(static pair => pair.After)]),
   GraphDelta.Empty with {
    RemovedNodes = rekeyed.Map(static pair => pair.Before.Id),
    AddedNodes = rekeyed.Map(static pair => pair.After),
    RevisedNodes = rewritten,
    RemovedEdges = relinked.Map(static pair => pair.Before),
    AddedEdges = relinked.Map(static pair => pair.After),
   });
 }

 public Fin<(WorkingGraph Graph, GraphDelta Delta)> Apply(GraphMutation mutation, Op key) =>
  mutation.Switch<(WorkingGraph Graph, Op Key), Fin<(WorkingGraph, GraphDelta)>>(
   (this, key),
   // Revise-versus-no-op discriminates on the GENERATED comparer, never object.Equals: Node is a class-root union
   // whose equality is Generator.Equals's, and the static helper resolves the same members only by accident.
   putNode: static (s, m) => Fin.Succ(s.Graph.Nodes.TryGetValue(m.Node.Id, out Node? prior)
    ? EqualityComparer<Node>.Default.Equals(prior, m.Node)
     ? (s.Graph, GraphDelta.Empty)
     : (s.Graph.Set(m.Node), GraphDelta.Empty with { RevisedNodes = [(prior!, m.Node)] })
    : (s.Graph.Set(m.Node), GraphDelta.Empty with { AddedNodes = [m.Node] })),
   dropNode: static (s, m) => s.Graph.Nodes.ContainsKey(m.Id)
    ? s.Graph.Erase(m.Id) switch {
       var (next, cascaded) => Fin.Succ((next, GraphDelta.Empty with { RemovedNodes = [m.Id], RemovedEdges = cascaded })),
      }
    : ElementFault.DeltaConflict(s.Key, $"<drop-absent-node:{m.Id.Value}>"),
   link: static (s, m) => LegalLink(m.Edge, s.Graph.Nodes, s.Key)
    .Bind(_ => s.Graph.Edges.Contains(m.Edge)
     ? ElementFault.DeltaConflict(s.Key, "<duplicate-link>")
     : Fin.Succ((s.Graph.Attach(m.Edge), GraphDelta.Empty with { AddedEdges = [m.Edge] }))),
   unlink: static (s, m) => s.Graph.Edges.Contains(m.Edge)
    ? Fin.Succ((s.Graph.Detach(m.Edge), GraphDelta.Empty with { RemovedEdges = [m.Edge] }))
    : ElementFault.DeltaConflict(s.Key, "<unlink-absent-edge>"),
   // Re-stamps are TOTAL: a rekey rewrites ids the graph already holds and introduces no endpoint LegalLink has not
   // already admitted, so no arm here can refuse and the structural law re-runs only when the lowered delta crosses
   // AdmitOnto on the far side of a persistence round trip.
   rekey: static (s, m) => Fin.Succ(Rekeyed(s.Graph, m.Map)),
   reheader: static (s, m) => Fin.Succ((s.Graph, GraphDelta.Empty with { Header = Some(m.Header) })),
   batch: static (s, m) => m.Mutations.Fold(
    Fin.Succ((Graph: s.Graph, Delta: GraphDelta.Empty)),
    (acc, next) => acc.Bind(state => state.Graph.Apply(next, s.Key).Map(step => (step.Graph, state.Delta.Merge(step.Delta))))));

 // BothObjects is the object-pair predicate Compose/Connect/Void share — ONE parameterized policy local (each case keeps its arm for
 // compile-time totality; the detail literal is the only variation), never three copies of the type test.
 static Fin<Unit> BothObjects(Node relating, Node related, Op key, string detail) =>
  relating is Node.Object && related is Node.Object ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, detail);

 // LegalLink is the seam's STRUCTURAL edge law: endpoint presence + non-Generic irreflexivity + endpoint-kind legality
 // ONLY, dispatched through the generated total Switch over the closed edge algebra (compile-time exhaustive, NO
 // runtime-default arm); the IFC-semantic legality is the consumer's IGraphConstraint, never here. Every typed kind
 // is IRREFLEXIVE (IFC schema WRs forbid self-aggregation/nesting/connection/typing/grouping; a self-loop double-enters
 // Members and forks the incidence/DirectedPairs topology) — only the Generic passthrough stays
 // self-permissive. The Connect REALIZING intermediary is a first-class structural participant (it rides
 // Members/DirectedPairs, the incidence index, and the Erase cascade), so LegalConnect validates it the SAME as the
 // binary endpoints — present, an Object, AND distinct from both — when Some.
 static Fin<Unit> LegalLink(Relationship edge, TrackingHashMap<NodeId, Node> nodes, Op key) {
  (NodeId relating, NodeId related) = edge.Endpoints;
  return edge.Members.Find(member => !nodes.ContainsKey(member)).Match(
   Some: member => ElementFault.NodeAbsent(key, $"<link-member-absent:{member.Value}>"),
   None: () => LegalPresent(edge, relating, related, nodes, key));
 }

 static Fin<Unit> LegalPresent(Relationship edge, NodeId relating, NodeId related, TrackingHashMap<NodeId, Node> nodes, Op key) =>
   !nodes.TryGetValue(relating, out Node? r) ? ElementFault.NodeAbsent(key, $"<link-relating-absent:{relating.Value}>")
   : !nodes.TryGetValue(related, out Node? d) ? ElementFault.NodeAbsent(key, $"<link-related-absent:{related.Value}>")
   : relating == related && edge is not Relationship.Generic ? ElementFault.RelationshipInvalid(key, $"<link-self-loop:{relating.Value}>")
   : edge.Switch<(Node Relating, Node Related, TrackingHashMap<NodeId, Node> Nodes, Op Key), Fin<Unit>>(
    (r!, d!, nodes, key),
    compose: static (s, _) => BothObjects(s.Relating, s.Related, s.Key, "<compose-endpoints-must-be-objects>"),
    assign: static (s, a) => LegalAssign(a, s.Relating, s.Related, s.Key),
    associate: static (s, _) => s.Relating is Node.Object && s.Related is (Node.Material or Node.Appearance or Node.Coverage) ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(s.Key, "<associate-resource-must-be-material-appearance-or-coverage>"),
    connect: static (s, c) => LegalConnect(c, s.Relating, s.Related, s.Nodes, s.Key),
    @void: static (s, _) => BothObjects(s.Relating, s.Related, s.Key, "<void-endpoints-must-be-objects>"),
    generic: static (s, _) => Fin.Succ(unit));
 }

 // LegalConnect states the Connect law: BothObjects on From/To, then the optional realizing intermediary — when Some —
 // is DISTINCT from both endpoints (a coincident realizing node duplicates Members and collapses the From→Realizing→To legs),
 // resolves in the graph, AND is an Object (coincident/non-Object rails RelationshipInvalid, absent rails NodeAbsent);
 // None is a plain binary connection passing on the endpoint pair alone.
 static Fin<Unit> LegalConnect(Relationship.Connect c, Node from, Node to, TrackingHashMap<NodeId, Node> nodes, Op key) =>
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
   assessment: () => definition is Node.Assessment ? Fin.Succ(unit) : ElementFault.RelationshipInvalid(key, "<assign-assessment-must-target-assessment>"),
   // Occurrence-only by construction: a Component names no instrument, so a Type subject refuses here rather
   // than minting a series the named type fold would then have to skip.
   observation: () => definition is not Node.Observation ? ElementFault.RelationshipInvalid(key, "<assign-observation-must-target-observation>")
    : subject is Node.Object { Kind: ObjectKind.Occurrence } ? Fin.Succ(unit)
    : ElementFault.RelationshipInvalid(key, "<assign-observation-subject-must-be-occurrence>"));
}
```

## [03]-[IMPLEMENTATION_LAW]

- [DELTA_EVENT_BODY]: `GraphDelta` is the persistence event body. Creating deltas carry the model header, `ReplayOnto` folds events into snapshots, and periodic persistence snapshots bound replay. `ToCanonicalBytes` sorts node and edge contributions and includes the semantic header, so recording order never forks the event address.
- [DELTA_MONOID]: `Merge` is the strict-left sequential composition used by `Batch` and stream compaction; it claims no general associativity. `NodeSlot.Advance` owns node coalescence as one flattened pattern over the slot product — `Merge` folds it over both sides' claim runs and `IsNormalForm` reads it back as the fixpoint test. Edge slots coalesce by structural equality under the generated comparer, and node-removal entanglement preserves the edge re-add required after cascade.
- [ID_RESTAMP]: `Rekey` is the graph-wide re-identify: one `Func<NodeId, NodeId>` policy value composed through `Node.Remap` and `Relationship.Remap`, rebuilding the working graph whole because an erase cascades incident edges the same rewrite is re-attaching. It LOWERS onto the existing delta vocabulary and mints no slot, so a `Rasm.Persistence` `Reconcile` and a `Rasm.Bim` re-identify compose one mutation.
- [STRUCTURAL_VS_SEMANTIC]: `LegalLink` owns endpoint presence, typed-edge irreflexivity, realizing-node distinctness, and endpoint-kind legality through generated total dispatch. `Generic` remains self-permissive for unmodeled relationships. `IGraphConstraint.Validate` owns IFC semantic legality after the structural gate, so the seam never names IFC entity classes.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

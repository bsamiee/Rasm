# [ELEMENT_DELTA]

`GraphMutation` is the ONE graph-mutation owner: a `[Union]` request applied through the generated total `Switch` to a HAMT `WorkingGraph`, producing the `GraphDelta` — the immutable change record that IS the persistable event body. `WorkingGraph` is the live-authoring counterpart to the frozen `Graph/element#ELEMENT_GRAPH` snapshot: `Thaw` lowers, `Freeze` lifts back, and `Apply` authors its own per-mutation delta off the HAMT it edits.

Every mutation enforces the seam's STRUCTURAL edge law through the generated total `Switch` over the `Relations/relation#EDGE_ALGEBRA` cases, railing `Projection/fault#FAULT_BAND` `ElementFault` — structural invariants ONLY, IFC-semantic legality being the consumer's `Projection/projection#GRAPH_CONSTRAINT` `IGraphConstraint`. `Merge` compacts sequentially through the `NodeSlot.Advance` and `EdgeSlot.Advance` transition tables into the normal form `NormalForm` reads back, so the ordered replay law and the `Address` content-key law hold.

`Reheader` establishes the model header on the model-creating event through the SAME algebra the projector's `GraphDelta.Reheader` builder uses. `ReplayOnto` folds a delta into a frozen graph — Marten stores the `GraphDelta` event, never a whole-graph snapshot per event — and `AdmitOnto` is the structural-validating sibling routing a projector-built delta through `WorkingGraph.Apply` for the `Projection/projection#PROJECTION_CONTRACT` `Assemble` admission.

## [01]-[INDEX]

- [02]-[GRAPH_DELTA]: `WorkingGraph` HAMT live authoring, the `GraphMutation` request family under its structural edge law, and the `GraphDelta` event body — `Merge` over the two slot tables, `NormalForm`, `ReplayOnto`, `AdmitOnto`, and the order-independent `Address` content key.

## [02]-[GRAPH_DELTA]

- Owner: `WorkingGraph` the HAMT live-authoring form (`HashMap<NodeId, Node>` + `ImmutableList<Relationship>`); `GraphMutation` the `[Union]` mutation request; `GraphDelta` the immutable change record that IS the persistable event body; `NodeSlot` and `EdgeSlot` the per-key slot vocabularies whose `Advance` transition tables state BOTH coalescence laws as declared rows (the edge table carries no `Revised` position — an edge has no key to revise under, so the two tables are deliberately distinct types); `NodePartition` the three node lists as one value the slot fold projects onto; the structural edge law `LegalLink`/`LegalConnect`/`LegalAssign`.
- Cases: `PutNode` (upsert a node — add if absent, revise if present, recording the before/after) · `DropNode` (remove a node and CASCADE its incident edges) · `Link` (add an edge after validating endpoint presence and structural legality) · `Unlink` (remove an edge by structural equality) · `Reheader` (establish/revise the model header on the model-creating event — the interactive-authoring counterpart to the projector's `GraphDelta.Reheader` builder) · `Batch` (a `Seq<GraphMutation>` folded fail-fast into one accumulated delta); the closed mutation family.
- Cases: `NodeSlot` closes the per-id positions a delta can hold — `Absent` (untouched) · `Added` (minted) · `Revised` (re-set over a base-present node) · `Erased` (removed with its edges cascaded) · `Recreated` (the lone deliberate erase-then-mint pair on one id); `EdgeSlot` closes the per-edge positions — `Absent` · `Added` · `Erased` · `Restored` (the entangled erase-then-re-add pair a surviving node erase keeps load-bearing).
- Entry: `WorkingGraph.Thaw(ElementGraph)` lowers a frozen snapshot into the HAMT form; `Apply(GraphMutation, key)` applies a mutation, `Fin<T>` returning the next `WorkingGraph` and the `GraphDelta` it produced (railing `ElementFault.NodeAbsent` on a link to an absent endpoint or absent realizing intermediary, `ElementFault.RelationshipInvalid` on an illegal endpoint-kind pair, a non-`Generic` self-loop, or an endpoint-coincident realizing node, `ElementFault.DeltaConflict` on a drop-absent or duplicate link); `Freeze(header)` lifts the HAMT form back to a frozen `ElementGraph` under the resolved header (rebuilding the incidence index and `QuikGraph` view once), every caller resolving that header through the one `GraphDelta.HeaderFor(graph)` member; `GraphDelta.ReplayOnto(ElementGraph)` folds a delta into a frozen graph for the persistence rehydrate (re-applying RAW, the delta validated when produced); `GraphDelta.AdmitOnto(ElementGraph, Op)` is the structural-VALIDATING sibling routing a projector-built delta through `WorkingGraph.Apply` so `LegalLink` runs per `Link` (the `Projection/projection#PROJECTION_CONTRACT` `Assemble` admission step), `Fin<T>` returning the frozen graph and the re-derived event body and railing `RelationshipInvalid`/`NodeAbsent`/`DeltaConflict` on a structurally-illegal projection. The snapshot-subtraction `Diff` DIED with zero consumers (its named consumers reach `Reheader`+`AdmitOnto`); a re-import event body re-derives at the producer that owns both snapshots when one materializes.
- Auto: `Apply` dispatches the generated total `Switch`: `PutNode` adds or revises, `DropNode` removes the node and every incident edge in one sweep, `Link` admits one legal non-duplicate edge, `Unlink` removes one structural match, `Reheader` records only the header, and `Batch` left-folds sub-mutations through `Merge`. `LegalLink` and `LegalAssign` are generated total dispatches with no runtime-default arm, so an `AssignKind` row lands its arm here or the dispatch stops compiling. `WorkingGraph` preserves structural sharing on the HAMT until `Freeze` materializes the read snapshot.
- Law: `NodeSlot.Advance` is the SINGLE statement of the node coalescence law — `Merge` folds it over the two sides' claim runs into the unique-per-id normal form (every node id in at most one of `{added, revised}`, remove-then-add the lone deliberate removed-and-added pair), and `NormalForm(key)` is exactly the fixpoint test that folding a delta's own claims re-derives its own lists — accumulated with a named token per conjunct, so a consumer reports WHICH invariant failed and no conjunct can drift from the fold that produces it and the ordered replay law `a.Merge(b).ReplayOnto(g) == b.ReplayOnto(a.ReplayOnto(g))` holds.
- Output: the `GraphDelta` is the one change record — the Marten event body carrying the added/removed/revised nodes and added/removed edges, NOT a whole-graph snapshot per event; the inline `SingleStreamProjection` folds `GraphDelta → ElementGraph` through `ReplayOnto` so the read snapshot rebuilds from the delta stream, the periodic Marten snapshot (`Projections.Snapshot<T>(SnapshotLifecycle.Inline)`) bounding replay, the cadence reading `NodeCount`/`EdgeCount` for the change magnitude; `Address(tolerance)` derives the delta's ORDER-INDEPENDENT content key (the Persistence event dedup and the Version op-identity) STREAMED on the same `XxHash128` canonical rail the node/edge/graph addresses use (`ToCanonicalBytes(tolerance, key)` the Fin byte leg only the parity corpus reads) — nodes sorted by id, edges by canonical bytes, the section counts self-delimiting the layout and every collection inside the node bytes count-prefixed per `Projection/address#IMPLEMENTATION_LAW` (the injectivity precondition of the raw-append `String(id)`+bytes joins), the full `Geospatial/reference#GEO_REFERENCE` `GeoReference` folded into the header contribution — so a re-applied, duplicated, or recording-order-permuted delta is detected by content, never a wall-clock; the `Generator.Equals` member diff (`Graph/element#ELEMENT_GRAPH` `ElementGraph.EqualityComparer.Default.Inequalities`) and the `GraphDelta` are the two change surfaces — the diff for a content-3-way merge, the delta for the forward event log.
- Packages: Thinktecture.Runtime.Extensions (`[Union]` + the generated total `Switch`), LanguageExt.Core (`Seq`/`Option`/`Fin`/`Fold`), System.Collections.Immutable/Frozen, `Rasm` (the kernel `Op` op-key).
- Growth: a new mutation is one `GraphMutation` case routed through the total `Switch`; a new coalescence rule is one row in a slot table's `Advance` product pattern, which `NormalForm` inherits without an edit; a new structural invariant is one arm in the `LegalLink` generated `Switch`; the `GraphDelta` event body grows by column, not by a parallel event type; never a per-node-kind mutation and never a whole-graph snapshot per event.
- Boundary: `GraphMutation` is the ONE request owner, `WorkingGraph` the HAMT live form, `GraphDelta` the event body, and `ElementGraph` the frozen read form. `LegalLink` enforces endpoint presence, typed-edge irreflexivity, and endpoint-kind legality; Bim constraints own IFC semantics. `Merge` is a strict-left sequential compactor, not a generally associative monoid. `ReplayOnto` trusts seam-produced deltas, while `AdmitOnto` replays foreign/projector deltas through structural admission.
- Boundary: the working EDGE half is an ordered `ImmutableList` whose O(edges) membership/splice cost is batch-shaped and deliberate — the fence's `WorkingGraph` comment owns the full law (order is what the wire emits; every read-path query is a frozen-snapshot read), stated once there.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Projection;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Graph;

// --- [TYPES] ---------------------------------------------------------------------------
[Union]
public abstract partial record NodeSlot {
 private NodeSlot() { }
 public sealed record Absent : NodeSlot;
 public sealed record Added(Node After) : NodeSlot;
 public sealed record Revised(Node Before, Node After) : NodeSlot;
 public sealed record Erased(NodeId Id) : NodeSlot;
 public sealed record Recreated(NodeId Id, Node After) : NodeSlot;

 public static readonly NodeSlot Nothing = new Absent();

 public static NodeSlot Advance(NodeSlot held, NodeSlot next) => (held, next) switch {
  (_, Absent) => held,
  (Absent, _) => next,
  (_, Recreated recreated) => recreated,
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

[Union]
public abstract partial record EdgeSlot {
 private EdgeSlot() { }
 public sealed record Absent : EdgeSlot;
 public sealed record Added(Relationship Edge) : EdgeSlot;
 public sealed record Erased(Relationship Edge) : EdgeSlot;
 public sealed record Restored(Relationship Edge) : EdgeSlot;

 public static readonly EdgeSlot Nothing = new Absent();

 public static EdgeSlot Advance(EdgeSlot held, EdgeSlot next, bool entangled) => (held, next) switch {
  (_, Absent) => held,
  (Absent, _) => next,
  (_, Restored restored) => restored,
  (Added, Erased) => Nothing,
  (Added, Added added) => added,
  (Erased, Added added) => entangled ? new Restored(added.Edge) : Nothing,
  (Erased, Erased erased) => erased,
  (Restored, Erased erased) => new Erased(erased.Edge),
  (Restored restored, Added) => restored,
 };
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct NodePartition(
 Seq<Node> Added, Seq<NodeId> Removed, Seq<(Node Before, Node After)> Revised) {
 public static readonly NodePartition Empty = new([], [], []);
}

public sealed record GraphDelta(
 Seq<Node> AddedNodes,
 Seq<NodeId> RemovedNodes,
 Seq<(Node Before, Node After)> RevisedNodes,
 Seq<Relationship> AddedEdges,
 Seq<Relationship> RemovedEdges,
 Option<Header> Header) {

 public static readonly GraphDelta Empty = new([], [], [], [], [], None);

 Seq<(NodeId Id, NodeSlot Slot)> Claims =>
  RemovedNodes.Map(static id => (Id: id, Slot: (NodeSlot)new NodeSlot.Erased(id)))
  + AddedNodes.Map(static node => (Id: node.Id, Slot: (NodeSlot)new NodeSlot.Added(node)))
  + RevisedNodes.Map(static revision => (Id: revision.After.Id, Slot: (NodeSlot)new NodeSlot.Revised(revision.Before, revision.After)));

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

 Seq<(Relationship Edge, EdgeSlot Slot)> EdgeClaims =>
  RemovedEdges.Map(static edge => (Edge: edge, Slot: (EdgeSlot)new EdgeSlot.Erased(edge)))
  + AddedEdges.Map(static edge => (Edge: edge, Slot: (EdgeSlot)new EdgeSlot.Added(edge)));

 static (Seq<Relationship> Added, Seq<Relationship> Removed) EdgeFold(
  Seq<(Relationship Edge, EdgeSlot Slot)> claims, Func<Relationship, bool> entangled) {
  HashMap<Relationship, EdgeSlot> slots = claims.Fold(
   HashMap<Relationship, EdgeSlot>.Empty,
   (state, claim) => state.AddOrUpdate(claim.Edge, held => EdgeSlot.Advance(held, claim.Slot, entangled(claim.Edge)), claim.Slot));
  return toSeq(claims.Map(static claim => claim.Edge).AsEnumerable().Distinct(EqualityComparer<Relationship>.Default))
   .Choose(edge => slots.Find(edge))
   .Fold((Added: Seq<Relationship>(), Removed: Seq<Relationship>()), static (acc, slot) => slot.Switch(
    state: acc,
    absent: static (state, _) => state,
    added: static (state, slot) => (state.Added.Add(slot.Edge), state.Removed),
    erased: static (state, slot) => (state.Added, state.Removed.Add(slot.Edge)),
    restored: static (state, slot) => (state.Added.Add(slot.Edge), state.Removed.Add(slot.Edge))));
 }

 public GraphDelta Merge(GraphDelta next) {
  NodePartition nodes = Coalesced(Claims + next.Claims);
  System.Collections.Generic.HashSet<NodeId> erased = nodes.Removed.ToHashSet();
  (Seq<Relationship> added, Seq<Relationship> removed) =
   EdgeFold(EdgeClaims + next.EdgeClaims, edge => edge.Members.Exists(erased.Contains));
  return new(nodes.Added, nodes.Removed, nodes.Revised, added, removed, next.Header.IsSome ? next.Header : Header);
 }

 public bool IsInert => AddedNodes.IsEmpty && RemovedNodes.IsEmpty && RevisedNodes.IsEmpty && AddedEdges.IsEmpty && RemovedEdges.IsEmpty;

 public Validation<Error, Unit> NormalForm(Op key) {
  NodePartition folded = Coalesced(Claims);
  return Accumulate(Seq(
   Gate(folded.Added.Count == AddedNodes.Count, key, "<delta-denormal:added-nodes>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Gate(folded.Removed.Count == RemovedNodes.Count, key, "<delta-denormal:removed-nodes>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Gate(folded.Revised.Count == RevisedNodes.Count, key, "<delta-denormal:revised-nodes>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Gate(RevisedNodes.ForAll(static revision => revision.Before.Id == revision.After.Id), key, "<delta-denormal:revision-id-mismatch>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Gate(AddedEdges.ToHashSet(EqualityComparer<Relationship>.Default).Count == AddedEdges.Count, key, "<delta-denormal:duplicate-added-edge>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Gate(RemovedEdges.ToHashSet(EqualityComparer<Relationship>.Default).Count == RemovedEdges.Count, key, "<delta-denormal:duplicate-removed-edge>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d)),
   Gate(AddedEdges.ForAll(edge => !RemovedEdges.Contains(edge) || RemovedNodes.Exists(edge.Touches)), key, "<delta-denormal:unentangled-edge-pair>", static (k, d) => (Error)new ElementFault.ValueRejected(k, d))));
 }

 public int NodeCount => AddedNodes.Count + RevisedNodes.Count + RemovedNodes.Count;
 public int EdgeCount => AddedEdges.Count + RemovedEdges.Count;

 static void Written(GraphDelta delta, double tolerance, CanonicalWriter w) {
  w.Sorted(delta.AddedNodes, static n => n.Id.Value, StringComparer.Ordinal,
   static (n, run) => { run.String(n.Id.Value); n.CanonicalBytes(run); });
  w.Sorted(delta.RevisedNodes, static r => r.After.Id.Value, StringComparer.Ordinal,
   static (r, run) => { run.String(r.After.Id.Value); r.After.CanonicalBytes(run); });
  w.Sorted(delta.RemovedNodes, static id => id.Value, StringComparer.Ordinal,
   static (id, run) => run.String(id.Value));
  w.Sorted(delta.AddedEdges.Map(e => ContentAddress.Of(e, tolerance).Value), static a => a, Comparer<UInt128>.Default, static (a, run) => run.U128(a));
  w.Sorted(delta.RemovedEdges.Map(e => ContentAddress.Of(e, tolerance).Value), static a => a, Comparer<UInt128>.Default, static (a, run) => run.U128(a));
  w.Optional(delta.Header, static (h, run) => h.CanonicalBytes(run));
 }

 public ContentAddress Address(double tolerance) =>
  ContentAddress.Of(this, tolerance, (delta, w) => Written(delta, tolerance, w));

 public Fin<ReadOnlyMemory<byte>> ToCanonicalBytes(double tolerance, Op key) {
  CanonicalWriter w = CanonicalWriter.Retaining(tolerance);
  Written(this, tolerance, w);
  return w.ToBytes(key);
 }

 public GraphDelta Put(Node node) => Merge(GraphDelta.Empty with { AddedNodes = [node] });
 public GraphDelta Link(Relationship edge) => Merge(GraphDelta.Empty with { AddedEdges = [edge] });
 public GraphDelta Reheader(Header header) => this with { Header = Some(header) };

 public Header HeaderFor(ElementGraph graph) => Header.IfNone(graph.Header);

 public ElementGraph ReplayOnto(ElementGraph graph) {
  WorkingGraph working = WorkingGraph.Thaw(graph);
  working = RemovedNodes.Fold(working, static (w, id) => w.Erase(id).Graph);
  working = AddedNodes.Fold(working, static (w, n) => w.Set(n));
  working = RevisedNodes.Fold(working, static (w, r) => w.Set(r.After));
  working = RemovedEdges.Fold(working, static (w, e) => w.Detach(e));
  working = AddedEdges.Fold(working, static (w, e) => w.Attach(e));
  return working.Freeze(HeaderFor(graph));
 }

 public Fin<(ElementGraph Graph, GraphDelta Delta)> AdmitOnto(ElementGraph graph, Op key) =>
  IsInert && Header.IsNone
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
 public sealed record Reheader(Header Header) : GraphMutation;
 public sealed record Batch(Seq<GraphMutation> Mutations) : GraphMutation;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed record WorkingGraph(HashMap<NodeId, Node> Nodes, ImmutableList<Relationship> Edges) {
 public static WorkingGraph Thaw(ElementGraph graph) =>
  new(toHashMap(graph.Nodes), graph.Edges.ToImmutableList());

 public ElementGraph Freeze(Header header) =>
  ElementGraph.Of(header, Nodes.AsIterable().ToFrozenDictionary(static pair => pair.Key, static pair => pair.Value), [.. Edges]);

 internal WorkingGraph Set(Node node) => this with { Nodes = Nodes.AddOrUpdate(node.Id, node) };

 internal (WorkingGraph Graph, Seq<Relationship> Cascaded) Erase(NodeId id) {
  Seq<Relationship> cascaded = toSeq(Edges).Filter(edge => edge.Touches(id));
  return (this with { Nodes = Nodes.Remove(id), Edges = Edges.RemoveRange(cascaded, EqualityComparer<Relationship>.Default) }, cascaded);
 }

 internal WorkingGraph Attach(Relationship edge) => this with { Edges = Edges.Add(edge) };
 internal WorkingGraph Detach(Relationship edge) => this with { Edges = Edges.Remove(edge, EqualityComparer<Relationship>.Default) };

 public Fin<(WorkingGraph Graph, GraphDelta Delta)> Apply(GraphMutation mutation, Op key) =>
  mutation.Switch<(WorkingGraph Graph, Op Key), Fin<(WorkingGraph, GraphDelta)>>(
   (this, key),
   putNode: static (s, m) => Fin.Succ(s.Graph.Nodes.Find(m.Node.Id).Match(
    Some: prior => EqualityComparer<Node>.Default.Equals(prior, m.Node)
     ? (s.Graph, GraphDelta.Empty)
     : (s.Graph.Set(m.Node), GraphDelta.Empty with { RevisedNodes = [(prior, m.Node)] }),
    None: () => (s.Graph.Set(m.Node), GraphDelta.Empty with { AddedNodes = [m.Node] }))),
   dropNode: static (s, m) => s.Graph.Nodes.ContainsKey(m.Id)
    ? s.Graph.Erase(m.Id) switch {
       var (next, cascaded) => Fin.Succ((next, GraphDelta.Empty with { RemovedNodes = [m.Id], RemovedEdges = cascaded })),
      }
    : new ElementFault.DeltaConflict(s.Key, $"<drop-absent-node:{m.Id.Value}>"),
   link: static (s, m) => LegalLink(m.Edge, s.Graph.Nodes, s.Key)
    .Bind(_ => s.Graph.Edges.Contains(m.Edge)
     ? new ElementFault.DeltaConflict(s.Key, "<duplicate-link>")
     : Fin.Succ((s.Graph.Attach(m.Edge), GraphDelta.Empty with { AddedEdges = [m.Edge] }))),
   unlink: static (s, m) => s.Graph.Edges.Contains(m.Edge)
    ? Fin.Succ((s.Graph.Detach(m.Edge), GraphDelta.Empty with { RemovedEdges = [m.Edge] }))
    : new ElementFault.DeltaConflict(s.Key, "<unlink-absent-edge>"),
   reheader: static (s, m) => Fin.Succ((s.Graph, GraphDelta.Empty with { Header = Some(m.Header) })),
   batch: static (s, m) => m.Mutations.Fold(
    Fin.Succ((Graph: s.Graph, Delta: GraphDelta.Empty)),
    (acc, next) => acc.Bind(state => state.Graph.Apply(next, s.Key).Map(step => (step.Graph, state.Delta.Merge(step.Delta))))));

 static Fin<Unit> BothObjects(Node relating, Node related, Op key, string detail) =>
  relating is Node.Object && related is Node.Object ? Fin.Succ(unit) : new ElementFault.RelationshipInvalid(key, detail);

 static Fin<Unit> LegalLink(Relationship edge, HashMap<NodeId, Node> nodes, Op key) {
  (NodeId relating, NodeId related) = edge.Endpoints;
  return edge.Members.Find(member => !nodes.ContainsKey(member)).Match(
   Some: member => new ElementFault.NodeAbsent(key, $"<link-member-absent:{member.Value}>"),
   None: () => LegalPresent(edge, relating, related, nodes, key));
 }

 static Fin<Unit> LegalPresent(Relationship edge, NodeId relating, NodeId related, HashMap<NodeId, Node> nodes, Op key) =>
   relating == related && edge is not Relationship.Generic ? new ElementFault.RelationshipInvalid(key, $"<link-self-loop:{relating.Value}>")
   : edge.Switch<(Node Relating, Node Related, HashMap<NodeId, Node> Nodes, Op Key), Fin<Unit>>(
    (nodes[relating], nodes[related], nodes, key),
    compose: static (s, _) => BothObjects(s.Relating, s.Related, s.Key, "<compose-endpoints-must-be-objects>"),
    assign: static (s, a) => LegalAssign(a, s.Relating, s.Related, s.Key),
    associate: static (s, _) => s.Relating is Node.Object && s.Related is (Node.Material or Node.Appearance or Node.Coverage) ? Fin.Succ(unit) : new ElementFault.RelationshipInvalid(s.Key, "<associate-resource-must-be-material-appearance-or-coverage>"),
    connect: static (s, c) => LegalConnect(c, s.Relating, s.Related, s.Nodes, s.Key),
    @void: static (s, _) => BothObjects(s.Relating, s.Related, s.Key, "<void-endpoints-must-be-objects>"),
    generic: static (s, _) => Fin.Succ(unit));

 static Fin<Unit> LegalConnect(Relationship.Connect c, Node from, Node to, HashMap<NodeId, Node> nodes, Op key) =>
  BothObjects(from, to, key, "<connect-endpoints-must-be-objects>")
   .Bind(_ => c.Realizing.Match(
     None: () => Fin.Succ(unit),
     Some: realizing => realizing == c.From || realizing == c.To
      ? new ElementFault.RelationshipInvalid(key, $"<connect-realizing-must-be-distinct:{realizing.Value}>")
      : nodes.Find(realizing).Match(
        Some: n => n is Node.Object ? Fin.Succ(unit) : new ElementFault.RelationshipInvalid(key, "<connect-realizing-must-be-object>"),
        None: () => Fin.Fail<Unit>(new ElementFault.NodeAbsent(key, $"<connect-realizing-absent:{realizing.Value}>")))));

 static Fin<Unit> LegalAssign(Relationship.Assign a, Node subject, Node definition, Op key) =>
  subject is not Node.Object ? new ElementFault.RelationshipInvalid(key, "<assign-subject-must-be-object>")
  : a.SubKind.Switch(
   propertyDefinition: () => definition is Node.PropertySet or Node.QuantitySet ? Fin.Succ(unit) : new ElementFault.RelationshipInvalid(key, "<assign-property-definition-must-target-bag>"),
   typeDefinition: () => definition is Node.Object o && o.Kind == ObjectKind.Type ? Fin.Succ(unit) : new ElementFault.RelationshipInvalid(key, "<assign-type-definition-must-target-type-object>"),
   group: () => definition is Node.Object ? Fin.Succ(unit) : new ElementFault.RelationshipInvalid(key, "<assign-group-must-target-object>"),
   assessment: () => definition is Node.Assessment ? Fin.Succ(unit) : new ElementFault.RelationshipInvalid(key, "<assign-assessment-must-target-assessment>"),
   observation: () => definition is not Node.Observation ? new ElementFault.RelationshipInvalid(key, "<assign-observation-must-target-observation>")
    : subject is Node.Object { Kind: ObjectKind.Occurrence } ? Fin.Succ(unit)
    : new ElementFault.RelationshipInvalid(key, "<assign-observation-subject-must-be-occurrence>"));
}
```

## [03]-[IMPLEMENTATION_LAW]

- [DELTA_EVENT_BODY]: `GraphDelta` is the persistence event body. Creating deltas carry the model header, `ReplayOnto` folds events into snapshots, and periodic persistence snapshots bound replay. `Address` streams sorted node and edge contributions with the semantic header, so recording order never forks the event address.
- [DELTA_MONOID]: `Merge` is the strict-left sequential composition used by `Batch` and stream compaction; it claims no general associativity. `NodeSlot.Advance` and `EdgeSlot.Advance` own BOTH coalescence laws as flattened patterns over their slot products — `Merge` folds them over both sides' claim runs and `NormalForm` reads them back as the fixpoint test; the entangled `Restored` row preserves the edge re-add a node-erase cascade makes load-bearing.
- [STRUCTURAL_VS_SEMANTIC]: `LegalLink` owns endpoint presence, typed-edge irreflexivity, realizing-node distinctness, and endpoint-kind legality through generated total dispatch. `Generic` remains self-permissive for unmodeled relationships. `IGraphConstraint.Validate` owns IFC semantic legality after the structural gate, so the seam never names IFC entity classes.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

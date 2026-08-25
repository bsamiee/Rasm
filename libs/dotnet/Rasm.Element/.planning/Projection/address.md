# [ELEMENT_ADDRESS]

`ContentAddress` hashes canonical seam bytes through the kernel seed-zero `XxHash128` (`Rasm/Domain/identity#[02]-[CONTENT_KEY]`), shared by geometry, snapshots, precomputed content keys, and cross-runtime peers. No second hasher or non-zero seed exists, and the byte codec is the KERNEL `CanonicalWriter` — this seam declares no writer, re-exports nothing, and contributes only the dimensioned `Measure` extension (`Properties/quantity#MEASURE_CANON`).

Graph addressing folds the semantic `Header`, excludes provenance, sorts node and edge digests, and verifies stored identities through `ElementFault.AddressUnstable`. `GraphMembers` is the delta-composable CACHE of that full-state fold: it holds the member digests a snapshot address sorts, advances by one `GraphDelta`, and re-enters the same private fold — so an incremental address is byte-identical to the recompute by construction, never a second addressing algebra.

## [01]-[INDEX]

- [02]-[CONTENT_ADDRESS]: `ContentAddress` owns structural identity and `BlobKey` the legacy XXH raw-payload key; kernel `ArtifactContent` owns SHA-256 artifact identity plus extent.
- [03]-[INCREMENTAL_ADDRESS]: `GraphMembers` the delta-composable accumulator — the id-keyed node-address map and the digest-keyed edge multiset under the header they folded under, its `Of` seed, its typed incremental/refold `Advance(delta, key)` outcome, and the `ContentAddress.OfGraph(GraphMembers)` re-entry into the ONE private sorting fold.
- [04]-[IMPLEMENTATION_LAW]: the hasher, projection-split, ordering, exclusion, and verification laws every entry above answers to.

## [02]-[CONTENT_ADDRESS]

- Owner: `ContentAddress` is the `[ValueObject<UInt128>]` seam content key over kernel `ContentHash`; `BlobKey` is its raw-payload XXH sibling. Stored evidence artifacts compose kernel `ArtifactContent`, never either semantic key.
- Entry: `ContentAddress` entries own graph identity and verification. Artifact admission stays at the kernel owner.
- Auto: `Of(Node)` writes the id before the node's canonical fold, so two occurrences with identical content stay distinct by id — DISTINCT from the id-EXCLUSIVE `NodeId` content mint, which digests the content alone because there the id derives from it. `OfGraph` folds `Header.CanonicalBytes`, then the sorted node digests, then the sorted edge digests, each run count-framed by the kernel `Sorted`/`Rows` composite. `Verify` reads `node.Seed(tolerance)` — the `NodeSeed` mint-regime witness `Graph/element#ELEMENT_GRAPH` publishes — and re-mints through the ONE `NodeId.Of(NodeSeed)` entry: a `Placement` seed verifies vacuously (a random Guid-v7 has no content preimage), every other regime re-derives and compares.
- Output: a `ContentAddress` is the stable cross-runtime seam content key — a content-derived `NodeId`'s preimage, a node's dedup/diff key, a snapshot's identity the `Rasm.Persistence` spine and the `Rasm.Compute` assessment cache key on; the `Verify` `Fin`/`Validation` is the rehydrate integrity verdict a content-keyed store reads before trusting a persisted id.
- Packages: `Rasm` (kernel `ContentHash`, `CanonicalWriter`, `Op`), System.IO.Hashing (`XxHash128` the streaming accumulator each fold seeds at zero), Thinktecture.Runtime.Extensions (`[ValueObject<UInt128>]`/`[ObjectFactory<string>]`), LanguageExt.Core (`Fin`, `Validation`, `Error`, `Seq.Traverse`, `.As()`).
- Growth: a new structural identity adds one input-shaped `Of` or `Verify` overload; a precomputed key composes `Of(UInt128)`; a new by-reference payload kind composes `BlobKey`; canonical vocabulary grows only on the KERNEL writer, and the dimensioned leg on `Properties/quantity#MEASURE_CANON`.
- Boundary: the WIRE face is the X32 hex string alone — a raw `UInt128` JSON number loses precision past 2^53 in a JS parse, so serializers render and admit through the `[ObjectFactory<string>]` factory. Admission is upper-case-strict: exactly the 32 characters `ToValue` emits. The generated `NodeWire.content_address`, `NodeId` render, and store columns read that one interior spelling.

```csharp
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System.Globalization;
using System.IO.Hashing;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;
using static Rasm.Domain.AdmissionSlots;

namespace Rasm.Element.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
file static class Hex {
 internal static UInt128? Admit(string? value) =>
  value is { Length: 32 } candidate
  && !candidate.AsSpan().ContainsAnyInRange(lowInclusive: 'a', highInclusive: 'f')
  && UInt128.TryParse(candidate, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out UInt128 parsed)
   ? parsed : null;
}

[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All)]
public sealed partial class ContentAddress {
 public static ContentAddress Of(ReadOnlySpan<byte> canonicalBytes) => Create(ContentHash.Of(canonicalBytes));

 public static ContentAddress Of(UInt128 contentHash) => Create(contentHash);

 public static ContentAddress Of<TState>(TState state, double tolerance, Action<TState, CanonicalWriter> fold) {
  CanonicalWriter writer = CanonicalWriter.Streaming(tolerance: tolerance, accumulator: new XxHash128(seed: 0L));
  fold(state, writer);
  return Create(writer.Digest());
 }

 public static ContentAddress Of(Node node, double tolerance) =>
  Of(node, tolerance, static (n, w) => { w.String(n.Id.Value); n.CanonicalBytes(w); });

 public static ContentAddress Of(Relationship edge, double tolerance) =>
  Of(edge, tolerance, static (e, w) => e.CanonicalBytes(w));

 public static ContentAddress OfGraph(ElementGraph graph) =>
  OfGraph(graph.Header,
          toSeq(graph.Nodes.Values).Map(node => Of(node, graph.Header.Tolerance).Value),
          graph.Edges.Map(edge => Of(edge, graph.Header.Tolerance).Value));

 static ContentAddress OfGraph(Header header, Seq<UInt128> nodes, Seq<UInt128> edges) =>
  Of((header, nodes, edges), header.Tolerance, static (s, w) => {
   s.header.CanonicalBytes(w);
   w.Sorted(s.nodes, static a => a, Comparer<UInt128>.Default, static (a, run) => run.U128(a));
   w.Sorted(s.edges, static a => a, Comparer<UInt128>.Default, static (a, run) => run.U128(a));
  });

 public static ValidationError? Validate(string? value, IFormatProvider? provider, out ContentAddress? item) {
  item = Hex.Admit(value) is UInt128 parsed ? Create(parsed) : null;
  return item is null ? ValidationError.Create($"<content-address-hex-invalid:{value}>") : null;
 }

 public string ToValue() => Value.ToString("X32", CultureInfo.InvariantCulture);

 public static Fin<Unit> Verify(Node node, double tolerance, Op key) =>
  node.Seed(tolerance).Switch<Fin<Unit>>(
   placement: static _ => Fin.Succ(unit),
   typeSeed: seed => Remint(seed, node.Id, key),
   content: seed => Remint(seed, node.Id, key),
   precomputed: seed => Remint(seed, node.Id, key));

 private static Fin<Unit> Remint(NodeSeed seed, NodeId stored, Op key) =>
  NodeId.Of(seed) == stored
   ? Fin.Succ(unit)
   : new ElementFault.AddressUnstable(key, $"<node-id-mismatch:{stored.Value}>");

 public static Validation<Error, Unit> Verify(ElementGraph graph, Op key) =>
  toSeq(graph.Nodes.Values)
   .Traverse(n => Verify(n, graph.Header.Tolerance, key).ToValidation())
   .As()
   .Map(static _ => unit);
}

[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All)]
public sealed partial class BlobKey {
 public static BlobKey Of(ReadOnlySpan<byte> payload) => Create(ContentHash.Of(payload));
 public static BlobKey Of(UInt128 digest) => Create(digest);

 public static ValidationError? Validate(string? value, IFormatProvider? provider, out BlobKey? item) {
  item = Hex.Admit(value) is UInt128 parsed ? Create(parsed) : null;
  return item is null ? ValidationError.Create($"<blob-key-hex-invalid:{value}>") : null;
 }

 public string ToValue() => Value.ToString("X32", CultureInfo.InvariantCulture);
}

```

## [03]-[INCREMENTAL_ADDRESS]

- Owner: `GraphMembers` is the delta-composable accumulator over the two member sets `OfGraph` sorts; `GraphMemberStep` is its typed incremental-or-refold outcome.
- Law: the accumulator is a CACHE of the full-state fold, never a second algebra. `OfGraph(GraphMembers)` re-sorts both member sets and re-enters the SAME private `OfGraph(Header, Seq<UInt128>, Seq<UInt128>)` the graph entry calls, so incremental and full-state are byte-identical by construction rather than by agreement. SORTING is the order-independence mechanism and stays so: a commutative hash (XOR- or sum-folded member digests) makes the accumulator trivial and forks EVERY address already persisted estate-wide, so the sort is the cost the identical-bytes guarantee is bought with.
- Entry: `GraphMembers.Of(ElementGraph)` seeds from a snapshot; `Advance(delta, key)` returns `Fin<GraphMemberStep>`; `ContentAddress.OfGraph(GraphMembers)` addresses the resolved members. `Incremental` carries the stepped members, while `Refold` carries the changed header a consumer uses to rebuild from its replayed full graph.
- Auto: `Advance` gates `delta.NormalForm(key)` first; a changed grid validates removals against the retained old-grid identities and emits `Refold` before deriving any new-grid address, while a stable grid applies removals, adds, and revisions in replay order. `Drop` and `Retire` accumulate independent removal faults; within `Retire`, removals group by edge address so each multiset slot checks its demanded count once.
- Output: a `GraphMembers` is the address's own preimage held live — a consumer that owns it answers "what is this snapshot's identity" without re-walking the graph, and `OfGraph` over it is the same value the recompute yields; `Advance` rails `NodeAbsent` on a node removal naming an absent id and `DeltaConflict` on an edge removal overdrawing a slot, because the multiset is exact and a negative count is unrepresentable.
- Packages: LanguageExt.Core (`HashMap`/`Seq`/`Fin`/`Validation`/`Option` + the `Apply` join and `Fold` steps), `Rasm` (the kernel `Op` and `Rasm/Domain/validation#ADMISSION_SLOTS` `Accumulate`), `Graph/element#ELEMENT_GRAPH` (`ElementGraph`/`Header`/`Node`/`NodeId`), `Graph/delta#GRAPH_DELTA` (`GraphDelta` with its accumulated `NormalForm` gate and declared member sets).
- Growth: a new member SET on the snapshot address is one column here and one section in the private fold, landed in the same edit so the two projections cannot diverge; a new delta slot is one arm in `Advance`. A second accumulator shape, a witness carrying a prior ADDRESS rather than its members, and an incremental path re-deriving the layout are each the deleted form.
- Boundary: a tolerance-changing header returns `GraphMemberStep.Refold` as an ordinary outcome — the accumulator retains addresses, not payloads, so it cannot re-quantize. Malformed normal form and absent members remain failures on `Fin`; a caller may never hide them behind the refold. `Header.SameGrid` is the one bitwise grid law, and `Edges` retains multiplicity because a count-less set collapses legal parallel edges.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record GraphMemberStep {
 private GraphMemberStep() { }

 public sealed record Incremental(GraphMembers Members) : GraphMemberStep;
 public sealed record Refold(Header Header) : GraphMemberStep;

 public GraphMembers Resolve(Func<Header, GraphMembers> refold) => Switch(
  state: refold,
  incremental: static (_, step) => step.Members,
  refold: static (rebuild, step) => rebuild(step.Header));
}

public sealed record GraphMembers {
 private GraphMembers(Header header, HashMap<NodeId, ContentAddress> nodes, HashMap<ContentAddress, int> edges) =>
  (Header, Nodes, Edges) = (header, nodes, edges);

 public Header Header { get; }
 public HashMap<NodeId, ContentAddress> Nodes { get; }
 public HashMap<ContentAddress, int> Edges { get; }

 public static GraphMembers Of(ElementGraph graph) =>
  new(graph.Header,
      toSeq(graph.Nodes.Values).Fold(
       HashMap<NodeId, ContentAddress>(),
       (held, node) => held.AddOrUpdate(node.Id, ContentAddress.Of(node, graph.Header.Tolerance))),
      graph.Edges.Fold(HashMap<ContentAddress, int>(), (held, edge) => Admit(held, edge, graph.Header.Tolerance)));

 public Fin<GraphMemberStep> Advance(GraphDelta delta, Op key) =>
  delta.NormalForm(key).ToFin().Bind(_ =>
   delta.Header.Match(
    None: () => Step(delta, Header, key),
    Some: next => next.SameGrid(Header)
     ? Step(delta, next, key)
     : Regrid(delta, next, key)));

 Fin<GraphMemberStep> Regrid(GraphDelta delta, Header next, Op key) =>
  (Drop(Nodes, delta.RemovedNodes, key), Retire(Edges, delta.RemovedEdges, Header.Tolerance, key))
   .Apply((_, _) => (GraphMemberStep)new GraphMemberStep.Refold(next)).As().ToFin();

 Fin<GraphMemberStep> Step(GraphDelta delta, Header header, Op key) =>
  (Drop(Nodes, delta.RemovedNodes, key), Retire(Edges, delta.RemovedEdges, header.Tolerance, key))
   .Apply((kept, edges) => (GraphMemberStep)new GraphMemberStep.Incremental(new GraphMembers(
    header,
    (delta.AddedNodes + delta.RevisedNodes.Map(static revision => revision.After))
     .Fold(kept, (held, node) => held.AddOrUpdate(node.Id, ContentAddress.Of(node, header.Tolerance))),
    delta.AddedEdges.Fold(edges, (held, edge) => Admit(held, edge, header.Tolerance)))))
   .As().ToFin();

 static Validation<Error, HashMap<NodeId, ContentAddress>> Drop(HashMap<NodeId, ContentAddress> held, Seq<NodeId> removals, Op key) =>
  Accumulate(removals.Map(id => held.ContainsKey(id)
    ? Success<Error, Unit>(unit)
    : Fail<Error, Unit>(new ElementFault.NodeAbsent(key, $"<members-remove-absent:{id.Value}>"))))
   .Map(_ => removals.Fold(held, static (map, id) => map.Remove(id)));

 static Validation<Error, HashMap<ContentAddress, int>> Retire(
  HashMap<ContentAddress, int> held, Seq<Relationship> removals, double tolerance, Op key) {
  HashMap<ContentAddress, int> demanded = removals.Fold(
   HashMap<ContentAddress, int>(),
   (map, edge) => map.AddOrUpdate(ContentAddress.Of(edge, tolerance), static count => count + 1, () => 1));
  return Accumulate(toSeq(demanded).Map(pair => held.Find(pair.Key).IfNone(0) >= pair.Value
    ? Success<Error, Unit>(unit)
    : Fail<Error, Unit>(new ElementFault.DeltaConflict(key, $"<members-edge-absent:{pair.Key.ToValue()}>"))))
   .Map(_ => toSeq(demanded).Fold(held, static (map, pair) =>
    map.Find(pair.Key).IfNone(0) - pair.Value switch {
     > 0 and var remaining => map.AddOrUpdate(pair.Key, remaining),
     _ => map.Remove(pair.Key),
    }));
 }

 static HashMap<ContentAddress, int> Admit(HashMap<ContentAddress, int> held, Relationship edge, double tolerance) =>
  held.AddOrUpdate(ContentAddress.Of(edge, tolerance), static count => count + 1, () => 1);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed partial class ContentAddress {
 public static ContentAddress OfGraph(GraphMembers members) =>
  OfGraph(members.Header,
          toSeq(members.Nodes.Values).Map(static address => address.Value),
          toSeq(members.Edges).Bind(static pair => toSeq(Enumerable.Repeat(pair.Key.Value, pair.Value))));
}
```

## [04]-[IMPLEMENTATION_LAW]

- [ONE_HASHER]: every address on this seam composes the KERNEL seed-zero `XxHash128` through `ContentHash.Of` or a seed-zero `CanonicalWriter.Streaming` fold, because a fork in that kernel-shared cross-runtime content space stays invisible until two runtimes disagree on one node's id; a second hasher, a non-zero seed, or a locally-spelled digest is the named defect. `GetHashCode` is process-salted in-memory state, never persisted, wire-compared, or read as identity; `Generator.Equals` stays an orthogonal field diff over the same member set.
- [THREE_PROJECTIONS]: three distinct projections share the ONE kernel writer and never conflate — an id-INCLUSIVE node address writes the id ahead of the node's canonical fold so graph dedup distinguishes two occurrences of identical content, an id-EXCLUSIVE `NodeId` content mint digests the content alone because there the id DERIVES from it, and a GRAPH address folds the header with the sorted member digests. Every call site names which of the three it reaches.
- [ORDER_INDEPENDENCE]: `OfGraph` SORTS the snapshot address — node and edge digests both ascending `UInt128` through the kernel `Sorted`, each run count-framed — so the layout is self-delimiting and identical content addresses identically in any arrival order. The graph key is a digest-of-digests on BOTH member axes (the node axis always was), so no raw member byte run enters the graph preimage and the collision posture is the digest's own 128 bits on every axis. Sorting, never a commutative hash, stays the mechanism: a commutative fold buys cheap incrementality, loses the section framing, admits multiset collisions, and re-keys every persisted address.
- [PROVENANCE_EXCLUSION]: `OfGraph` folds the SEMANTIC header (schema, model view, tolerance, georeference) and EXCLUDES `StepHeader`/`Instant` provenance — the graph-altitude mirror of the node-level `OwnerHistory` exclusion — so a re-export under a new timestamp or author addresses identically while a schema, view, georeference, or tolerance change forks identity honestly. `Object.Placement` rides that same node-level exclusion, so a rigid move is a `Moved` verdict, never a re-key.
- [VERIFY_REGIME]: `Verify` re-mints by the regime that MINTED the id, never by one uniform re-hash — the regime is the `NodeSeed` witness `Node.Seed(tolerance)` publishes, and `NodeId.Of(NodeSeed)` is the one re-mint entry, so a later geometry attach never spuriously fails a sound Type id (its seed excludes `Representations`) and a `Placement` seed verifies vacuously because a random Guid-v7 has no content preimage. Every content-derived arm re-projects under the mint-time `Header.Tolerance`, or the quantized re-projection drifts and a sound node reads unstable.
- [VERIFY_CARRIER]: carrier selects the verification algebra, never a flag — the single-node `Verify` fails fast on `Fin` over its one dependent check, and the snapshot sweep accumulates every drifted node on `Validation` over independent per-node checks.

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

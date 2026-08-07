# [ELEMENT_ADDRESS]

`ContentAddress` hashes canonical seam bytes through kernel seed-zero `XxHash128`, shared by geometry, snapshots, precomputed content keys, and cross-runtime peers. No second hasher or non-zero seed exists.

`CanonicalWriter` owns deterministic little-endian projection, canonical IEEE-754 values, tolerance-quantized measures, count-framed strings and collections, and explicit attribute order. Non-finite scalars normalize; `Associate.ReferenceExtent` depends on the stable unset-`NaN` canon.

Every `CanonicalBytes` contribution composes this codec, so identity, content address, and `StructuralMerge` keys share one encoding. `Generator.Equals` remains an orthogonal field diff. Graph addressing folds semantic `Header`, excludes provenance, sorts nodes and edges, and verifies stored identities through `ElementFault.AddressUnstable`.

`GraphMembers` is the delta-composable CACHE of that full-state fold: it holds the member sets a snapshot address sorts, advances by one `GraphDelta`, and re-enters the same private fold — so an incremental address is byte-identical to the recompute by construction, never a second addressing algebra.

## [01]-[INDEX]

- [02]-[CONTENT_ADDRESS]: the `ContentAddress` `[ValueObject<UInt128>]` over the kernel seed-zero `XxHash128`, the raw-hash/precomputed-wrap/node/graph/verification entries, the id-inclusive node and order-independent graph addressing (semantic header folded, provenance excluded), and the `Verify` re-derive dual that re-mints by the minting regime and rails `ElementFault.AddressUnstable` on a mismatch.
- [03]-[INCREMENTAL_ADDRESS]: the `GraphMembers` accumulator — the id-keyed node-address map and the content-keyed edge-byte multiset under the header they folded under, its `Of` seed, its `Advance(delta, key)` step with the tolerance-reheader refusal, and the `ContentAddress.OfGraph(GraphMembers)` re-entry into the ONE private sorting fold.
- [04]-[CANONICAL_WRITER]: the `CanonicalWriter` ONE deterministic byte-projection codec (IEEE-754 LE, sign/`NaN`/∞ canon, tolerance-quantized measures with their presence-prefixed uncertainty band, length-prefixed strings and count-prefixed collections, explicit attribute order) every seam value's `CanonicalBytes`/`ToCanonicalBytes` composes, so identity, content address, and 3-way-merge key project through one encoding.

## [02]-[CONTENT_ADDRESS]

- Owner: `ContentAddress` is the `[ValueObject<UInt128>]` content key over kernel seed-zero `XxHash128`; `ByteOrder` is the shared edge-byte comparer for snapshot and `GraphDelta.ToCanonicalBytes` sorting. The FULL-STATE fold over the graph is the DEFINITION of a snapshot address — the `[03]` `GraphMembers` accumulator is that definition's provably-identical cache, re-entering this owner's own private fold, never a parallel derivation that could fall out of step.
- Entry: `ContentAddress.Of(ReadOnlySpan<byte>)` hashes canonical bytes; `Of(UInt128)` wraps a precomputed hash; `Of(Node, tolerance)` addresses an id-inclusive node; `OfGraph(ElementGraph)` addresses an order-independent snapshot; `OfGraph(GraphMembers)` addresses the same snapshot from the accumulated member sets; `Verify(Node, tolerance, key)` re-derives one identity; `Verify(ElementGraph, key)` accumulates snapshot mismatches.
- Auto: `Of(Node)` writes the id before `node.ToCanonicalBytes(tolerance)`. `OfGraph` writes `Header.CanonicalBytes`, sorted node addresses, and lexicographically sorted edge bytes with section counts. `Verify` re-mints Types through `NodeId.RootedType`, non-rooted nodes through `NodeId.OfContent`, and admits Occurrences vacuously because their random Guid-v7 has no content preimage.
- Receipt: a `ContentAddress` is the stable cross-runtime content key — a `NodeId.Content` for a non-rooted node, a node's dedup/diff key, a snapshot's identity the `Rasm.Persistence` spine and the `Rasm.Compute` assessment cache key on; the `Verify` `Fin`/`Validation` is the rehydrate integrity verdict a content-keyed store reads before trusting a persisted id.
- Packages: `Rasm` supplies kernel `Domain.ContentHash` and `Op`; Thinktecture.Runtime.Extensions generates `[ValueObject<UInt128>]` members; LanguageExt.Core supplies `Fin`, `Validation`, `Error`, `Unit`, `Seq.Traverse`, and `.As()`.
- Growth: a new structural identity adds one input-shaped `Of` or `Verify` overload; a precomputed key composes `Of(UInt128)`; canonical vocabulary grows only on `CanonicalWriter`.
- Boundary: the WIRE face is the X32 hex string alone — a raw `UInt128` JSON number loses precision past 2^53 in a JS parse, so every serializer framework renders and admits through the `[ObjectFactory<string>]` factory, the admission preserving the canonical 32-digit spelling exactly so a padded, over-long, or prefix-bearing alias never round-trips into a different string than it arrived as; the `[05]` cluster owns the hasher, projection-split, ordering, exclusion, preimage, and verification laws this owner answers to.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Rasm.Element.Graph;
using Rasm.Element.Properties;
using Rasm.Element.Relations;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Projection;

// --- [TYPES] ------------------------------------------------------------------------------
// KeyMemberName/KeyMemberAccessModifier are EXPLICIT: the UInt128 Value is read publicly across the seam (this OfGraph
// node sort reads `Of(n, _).Value`, `NodeId.OfContent(address)` formats `address.Value` X32), so the public-key
// spelling is pinned at declaration rather than left to a generated default the consumers cannot rely on.
[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All)]
public sealed partial class ContentAddress {
 // Lexicographic byte-sequence ordering is a cached singleton over BCL span comparison, shared by `OfGraph` and
 // `GraphDelta.ToCanonicalBytes` edge sorts and internal to the assembly.
 internal static readonly IComparer<ReadOnlyMemory<byte>> ByteOrder =
  Comparer<ReadOnlyMemory<byte>>.Create(static (x, y) => x.Span.SequenceCompareTo(y.Span));

 // Kernel `ContentHash` is the one seed-zero hasher over canonical bytes.
 public static ContentAddress Of(ReadOnlySpan<byte> canonicalBytes) => Create(ContentHash.Of(canonicalBytes));

 // Wrap a PRECOMPUTED content hash (a RepresentationContentHash body key, a Coverage.RasterKey, an
 // Assessment.InputKey) as a ContentAddress without re-hashing — the carrier over an already-derived UInt128,
 // distinct from the hashing entry above.
 public static ContentAddress Of(UInt128 contentHash) => Create(contentHash);

 // Id-INCLUSIVE node addressing writes the id then `node.ToCanonicalBytes`, so two occurrences
 // with identical content stay distinct by id. DISTINCT from the id-EXCLUSIVE non-rooted `NodeId.Content` mint,
 // which hashes the content bytes ALONE (the id derives from them) — the two share the one writer, never the one hash.
 public static ContentAddress Of(Node node, double tolerance) {
  CanonicalWriter w = new(tolerance);
  w.String(node.Id.Value).Raw(node.ToCanonicalBytes(tolerance).Span);
  return Of(w.ToBytes().Span);
 }

 // Order-INDEPENDENT snapshot addressing folds semantic `Header.CanonicalBytes` first,
 // then node addresses sorted by UInt128, then edge canonical bytes sorted lexicographically, section counts making the
 // layout self-delimiting — identical content addresses identically regardless of insertion order, while a
 // schema/view/georeference change forks identity.
 public static ContentAddress OfGraph(ElementGraph graph) =>
  OfGraph(graph.Header,
          toSeq(graph.Nodes.Values).Map(node => Of(node, graph.Header.Tolerance).Value),
          graph.Edges.Map(e => e.ToCanonicalBytes(graph.Header.Tolerance)));

 // Node addresses cross as a bare address RUN: the id keys them at the graph, the sort keys them here, and a
 // re-keyed intermediate map only re-states an identity the caller's own store already holds.
 static ContentAddress OfGraph(Header header, Seq<UInt128> nodes, Seq<ReadOnlyMemory<byte>> edges) {
  CanonicalWriter w = new(header.Tolerance);
  header.CanonicalBytes(w);
  w.Ordinal(nodes.Count);
  // Default UInt128 ascending comparison is the canonical cross-runtime node order;
  // node and edge sorts are ONE ordering discipline.
  foreach (UInt128 nodeAddress in nodes.OrderBy(static a => a)) { w.U128(nodeAddress); }
  // SAME `Relationship.ToCanonicalBytes(tolerance)` projection keys merge edges, so graph address
  // and edge merge-key never diverge. Threading Header.Tolerance matters ONLY for the Generic passthrough (its
  // PropertyValue.Measure attributes quantize through w.Measure; the five typed cases carry no Measure and are
  // tolerance-insensitive) — the tolerance-0 hardcode that silently forked a below-tolerance Generic edge is the
  // deleted form.
  w.Ordinal(edges.Count);
  foreach (ReadOnlyMemory<byte> edge in edges.OrderBy(static b => b, ByteOrder)) { w.Raw(edge.Span); }
  return Of(w.ToBytes().Span);
 }

 // The TYPE arm re-mints over the Representations-EXCLUDED ToTypeSeedBytes seed, so a later geometry attach cannot
 // spuriously fail a sound id while a forged or corrupted one is still caught. Each content arm composes the seam's
 // own mint entries and compares NodeId to NodeId under the declared [KeyMemberEqualityComparer] — the X32 spelling
 // is owned by NodeId, never re-spelled here. [VERIFY_REGIME] owns the arm law.
 // [ObjectFactory<string>] contracts are IObjectFactory<ContentAddress, string, ValidationError> and
 // IConvertible<string> (decompile-verified), so every serializer framework picks up the hex face through the
 // generated converters with zero local edits and the TS/Python peers hold the key as that string.
 public static ValidationError? Validate(string? value, IFormatProvider? provider, out ContentAddress? item) {
  // Admission preserves the CANONICAL X32 spelling exactly: the wire form ToValue emits is the ONLY form admitted,
  // so an unpadded, over-long, or sign/prefix-bearing hex alias never round-trips into a different spelling than it
  // arrived as (a variable-width TryParse alone admitted "ABC" and re-emitted 29 leading zeros — normalization
  // drift a cross-runtime peer comparing wire strings byte-wise would read as a fork).
  // AllowHexSpecifier ALONE (HexNumber folds in leading/trailing-whitespace allowances that would let a padded
  // 32-char string carry fewer than 32 hex digits under the width check).
  item = value is { Length: 32 } && UInt128.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out UInt128 parsed)
   ? Create(parsed)
   : null;
  return item is null ? ValidationError.Create($"<content-address-hex-invalid:{value}>") : null;
 }

 public string ToValue() => Value.ToString("X32", CultureInfo.InvariantCulture);

 public static Fin<Unit> Verify(Node node, double tolerance, Op key) =>
  node.Switch<Fin<Unit>>(
   // An OCCURRENCE Object's random Guid-v7 has no content preimage, so it verifies vacuously; a TYPE Object re-mints
   // from its volatile-excluded seed. Every other case verifies by content through the one body.
   @object: o => o.Kind == ObjectKind.Occurrence
    ? Fin.Succ(unit)
    : NodeId.RootedType(o.ToTypeSeedBytes(tolerance).Span) == o.Id
     ? Fin.Succ(unit)
     : ElementFault.AddressUnstable(key, $"<type-id-mismatch:{o.Id.Value}>"),
   material: m => VerifyContent(m, tolerance, key),
   propertySet: p => VerifyContent(p, tolerance, key),
   quantitySet: q => VerifyContent(q, tolerance, key),
   assessment: a => VerifyContent(a, tolerance, key),
   appearance: a => VerifyContent(a, tolerance, key),
   coverage: c => VerifyContent(c, tolerance, key),
   observation: o => VerifyContent(o, tolerance, key));

 private static Fin<Unit> VerifyContent(Node node, double tolerance, Op key) =>
  NodeId.OfContent(Of(node.ToCanonicalBytes(tolerance).Span)) == node.Id
   ? Fin.Succ(unit)
   : ElementFault.AddressUnstable(key, $"<content-id-mismatch:{node.Id.Value}>");

 // Snapshot rehydrate re-verifies every node through the single-node `Verify`, with `Validation` ACCUMULATING all
 // mismatches (independent checks license accumulation — a corrupt snapshot reports every drifted node at once), the
 // caller converting at the boundary so an unstable snapshot never enters the read path as trusted. The carrier-owned
 // `.ToValidation()` re-anchors each `Fin` into the accumulating algebra — never a hand-rolled `Match(Succ, Fail)`
 // re-deriving what the carrier owns — and applicative `.Traverse` unions every mismatch through `Error.Combine`.
 public static Validation<Error, Unit> Verify(ElementGraph graph, Op key) =>
  toSeq(graph.Nodes.Values)
   .Traverse(n => Verify(n, graph.Header.Tolerance, key).ToValidation())
   .As()
   .Map(static _ => unit);
}
```

## [03]-[INCREMENTAL_ADDRESS]

- Owner: `GraphMembers` the delta-composable accumulator over exactly the two member sets `OfGraph` sorts — `Nodes` the id-keyed map of id-INCLUSIVE node addresses and `Edges` the content-keyed multiset of edge canonical bytes with their multiplicity — held under the `Header` they folded under; `EdgeMember` the multiset cell pairing an edge's retained bytes with its count.
- Law: the accumulator is a CACHE of the full-state fold, never a second algebra. `OfGraph(GraphMembers)` re-sorts both member sets and re-enters the SAME private `OfGraph(Header, Seq<UInt128>, Seq<ReadOnlyMemory<byte>>)` the graph entry calls, so incremental and full-state are byte-identical by construction rather than by agreement. SORTING is the order-independence mechanism and stays so: a commutative hash (XOR- or sum-folded member digests) would make the accumulator trivial and fork EVERY address already persisted estate-wide, so the sort is the cost the identical-bytes guarantee is bought with.
- Entry: `GraphMembers.Of(ElementGraph)` seeds from a frozen snapshot; `Advance(GraphDelta delta, Op key)` steps by one delta returning the next accumulator; `ContentAddress.OfGraph(GraphMembers)` addresses it. The consumer spelling is the TWO-ARG form the `Rasm.Persistence` `Element/codec` contract records — `Advance(prior, delta)` then `OfGraph(members)` — so a `Version/timetravel` `Scrub` reel or a `Bisect` probe pays one member step per event instead of one full-state fold per event.
- Auto: `Advance` gates `delta.IsNormalForm` FIRST (the accumulator's arithmetic is exact, so a delta carrying a cancelled pair or a duplicate id would double-count), resolves the header as `delta.Header.IfNone(prior.Header)`, then applies removals, adds, and revisions in the SAME order `GraphDelta.ReplayOnto` folds them. It applies the delta's DECLARED sets and derives no cascade — the `DropNode` sweep already ran where the delta was produced, so the accumulator mirrors the replay rather than re-deciding it. A node removal naming an absent id rails `NodeAbsent` and an edge removal naming an absent member `DeltaConflict`, because the multiset is exact and a negative count is unrepresentable.
- Receipt: a `GraphMembers` is the address's own preimage held live — a consumer that owns it can answer "what is this snapshot's identity" without re-walking the graph, and `OfGraph` over it is the same value the recompute yields.
- Packages: LanguageExt.Core (`HashMap`/`Seq`/`Fin`/`Option` + the `Fold` step and the `Bind` rail), `Rasm` (the kernel `Op` op-key), `Graph/element#ELEMENT_GRAPH` (`ElementGraph`/`Header`/`Node`/`NodeId`), `Graph/delta#GRAPH_DELTA` (`GraphDelta` with its `IsNormalForm` gate and its declared member sets), BCL inbox (`BitConverter.DoubleToInt64Bits` the bitwise tolerance comparison).
- Growth: a new member SET on the snapshot address is one column here and one section in the private fold, landed in the same edit so the two projections cannot diverge; a new delta slot is one arm in `Advance`. A second accumulator shape, a witness carrying a prior ADDRESS rather than its members, or an incremental path that re-derives the layout is the deleted form — an address is only ever the sorted member fold.
- Boundary: a delta whose header changes `Tolerance` REFUSES — the accumulator holds node ADDRESSES, not node payloads, so it cannot re-quantize a measure onto a new grid, and every stored node address is grid-bound; the refusal is an explicit arm naming the full-state re-fold, never a caller obligation a consumer discovers by drifting. The comparison is BITWISE (the `Federate` header law's own spelling), so a `-0.0`/`0.0` or `NaN` re-header is a real fork rather than an `==` that silently rules two grids the same. The edge multiset keys on the edge's own `ContentAddress` — the identity space every seam key already rides — and RETAINS the bytes, because the private fold sorts the byte runs and a key alone cannot reproduce them; a count-less set would collapse the parallel edges `allowParallelEdges` admits and address a different graph.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
// The edge multiset cell: the canonical bytes the sort reads back, beside the multiplicity a parallel-edge graph
// carries. Bytes are RETAINED rather than re-projected because the accumulator holds no Relationship — re-deriving
// them would demand the payload the accumulator exists to avoid keeping.
public readonly record struct EdgeMember(ReadOnlyMemory<byte> Bytes, int Count);

// The address preimage held live. Nodes is a MAP, not a bare address multiset, because GraphDelta.RemovedNodes is a
// Seq<NodeId> carrying NO content — a removal can only be applied against a set keyed by id. HashMap is the carrier
// on both columns: ordering is the OfGraph sort's job and never the map's, so the keys owe no comparison axis.
public sealed record GraphMembers {
 private GraphMembers(Header header, HashMap<NodeId, ContentAddress> nodes, HashMap<ContentAddress, EdgeMember> edges) =>
  (Header, Nodes, Edges) = (header, nodes, edges);

 public Header Header { get; }
 public HashMap<NodeId, ContentAddress> Nodes { get; }
 public HashMap<ContentAddress, EdgeMember> Edges { get; }

 // The seed reads exactly what the full-state fold reads: the id-inclusive node address per node and the edge
 // canonical bytes per edge, both under the snapshot's own Header.Tolerance.
 public static GraphMembers Of(ElementGraph graph) =>
  new(graph.Header,
      toSeq(graph.Nodes.Values).Fold(
       HashMap<NodeId, ContentAddress>(),
       (held, node) => held.AddOrUpdate(node.Id, ContentAddress.Of(node, graph.Header.Tolerance))),
      toSeq(graph.Edges).Fold(HashMap<ContentAddress, EdgeMember>(), (held, edge) => Admit(held, edge, graph.Header.Tolerance)));

 // One delta, one member step. The normal-form gate runs FIRST because the arithmetic below is exact: a delta
 // carrying an add-then-remove pair or a duplicate id would double-count where ReplayOnto's set semantics absorb it.
 // Removals precede adds precede revisions — the ReplayOnto order — so the two folds agree on a delta that both
 // erases and re-Sets one id.
 public Fin<GraphMembers> Advance(GraphDelta delta, Op key) =>
  !delta.IsNormalForm
   ? ElementFault.DeltaConflict(key, "<members-delta-denormal>")
   : Regrid(delta, key).Bind(header =>
      Drop(Nodes, delta.RemovedNodes, key).Bind(kept =>
       Retire(Edges, delta.RemovedEdges, header.Tolerance, key).Map(edges =>
        new GraphMembers(
         header,
         (delta.AddedNodes + delta.RevisedNodes.Map(static revision => revision.After))
          .Fold(kept, (held, node) => held.AddOrUpdate(node.Id, ContentAddress.Of(node, header.Tolerance))),
         delta.AddedEdges.Fold(edges, (held, edge) => Admit(held, edge, header.Tolerance))))));

 // The tolerance-reheader REFUSAL, stated as an arm rather than left to a caller: every held node address was minted
 // on the prior grid and the accumulator kept no payload to re-quantize, so the only honest answer is the full-state
 // re-fold this detail names. Bitwise, per the Federate header law — an == would rule -0.0 and 0.0 one grid.
 // INSTANCE, not static: the resolution reads the accumulator's own held Header as the IfNone floor.
 Fin<Header> Regrid(GraphDelta delta, Op key) =>
  delta.Header.Match(
   None: () => Fin.Succ(Header),
   Some: next => BitConverter.DoubleToInt64Bits(next.Tolerance) == BitConverter.DoubleToInt64Bits(Header.Tolerance)
    ? Fin.Succ(next)
    : ElementFault.DeltaConflict(key, string.Create(CultureInfo.InvariantCulture, $"<members-tolerance-reheader:{next.Tolerance:R}>")));

 static Fin<HashMap<NodeId, ContentAddress>> Drop(HashMap<NodeId, ContentAddress> held, Seq<NodeId> removals, Op key) =>
  removals.Fold(Fin.Succ(held), (state, id) => state.Bind(map =>
   map.ContainsKey(id)
    ? Fin.Succ(map.Remove(id))
    : ElementFault.NodeAbsent(key, $"<members-remove-absent:{id.Value}>")));

 static Fin<HashMap<ContentAddress, EdgeMember>> Retire(
  HashMap<ContentAddress, EdgeMember> held, Seq<Relationship> removals, double tolerance, Op key) =>
  removals.Fold(Fin.Succ(held), (state, edge) => state.Bind(map =>
   Slot(edge, tolerance) switch {
    var (slot, _) => map.Find(slot).Match(
     // Decrement, then drop at zero — a parallel edge removed once leaves its twin addressable.
     Some: member => Fin.Succ(member.Count > 1 ? map.AddOrUpdate(slot, member with { Count = member.Count - 1 }) : map.Remove(slot)),
     None: () => ElementFault.DeltaConflict(key, $"<members-edge-absent:{slot.ToValue()}>")),
   }));

 static HashMap<ContentAddress, EdgeMember> Admit(HashMap<ContentAddress, EdgeMember> held, Relationship edge, double tolerance) =>
  Slot(edge, tolerance) switch {
   var (slot, bytes) => held.AddOrUpdate(
    slot, member => member with { Count = member.Count + 1 }, new EdgeMember(bytes, 1)),
  };

 // The SAME Relationship.ToCanonicalBytes(tolerance) projection the full-state fold sorts, addressed through the one
 // seam key so the multiset and the byte run key on one identity.
 static (ContentAddress Slot, ReadOnlyMemory<byte> Bytes) Slot(Relationship edge, double tolerance) =>
  edge.ToCanonicalBytes(tolerance) switch { var bytes => (ContentAddress.Of(bytes.Span), bytes) };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public sealed partial class ContentAddress {
 // The incremental entry re-enters the PRIVATE full-state fold above — same header contribution, same node-address
 // sort, same lexicographic edge-byte sort, same section counts — so the two paths are one projection with two
 // ingresses. Each multiset cell expands to its own count of byte runs, because the full-state fold writes one run
 // per edge INSTANCE and a parallel edge must contribute twice.
 public static ContentAddress OfGraph(GraphMembers members) =>
  OfGraph(members.Header,
          toSeq(members.Nodes.Values).Map(static address => address.Value),
          toSeq(members.Edges.Values).Bind(static member => toSeq(Enumerable.Repeat(member.Bytes, member.Count))));
}
```

## [04]-[CANONICAL_WRITER]

- Owner: `CanonicalWriter` is the one tolerance-bound byte codec composed by `Node`, `Relationship`, `PropertyValue`, `MaterialComposition`, `MeasureValue`, `GeoReference`, and `CoverageGrid` canonical projections, so identity, address, and merge keys share one encoding.
- Entry: `new CanonicalWriter(tolerance)` opens a projection bound to the model tolerance; the `Double`/`String`/`Ordinal`/`I64`/`U128`/`Bool`/`Optional`/`Measure`/`Raw` primitives write canonically and each returns the writer for fluent chaining; `Frame(IBufferWriter<byte>, string)` exposes the same string/count preimage framing without opening a model projection; `ToBytes()` reads the accumulated `WrittenMemory`; `Tolerance` exposes the bound grid a sibling `CanonicalBytes` fold reads.
- Auto: `Double` canonicalizes `-0.0`→`0.0` and every `NaN`→one quiet-NaN pattern then writes the IEEE-754 little-endian bits (±∞ keep their already-canonical bits), so a sign-of-zero or a payload-`NaN` never forks the hash and an unset-`NaN` sentinel canonicalizes stably; `Measure` quantizes the SI magnitude AND its uncertainty band to the tolerance grid through `MeasureValue.Quantize` then writes the length-prefixed `QuantityType` discriminator token, the magnitude, the seven `Dimension` exponents, and the presence-prefixed band (kind token, grid-quantized bounds, presence-prefixed standard deviation and coverage factor) — NOT the display unit string — so a `Torque` and an `Energy` (or a `SectionModulus` and a `Volume`, or a dimension-anonymous `OfSi` and a named `Volume`) that share a `Dimension` stay distinct under the `NodeId.Content` mint, the content-dedup key, and the 3-way merge, while the SI-native `OfSi` and the UnitsNet-coerced `Of` still project one physical measure to one byte sequence and two measures within tolerance address identically; `String` composes `Frame`, which writes the little-endian UTF-8 byte count before the bytes so a delimiter collision cannot forge equality; `Ordinal`/`I64`/`U128`/`Bool` write fixed-width little-endian (the count prefixes that make a collection layout self-delimiting, `I64` the instant/tick canon a temporal row writes); `ToBytes` reads the accumulated memory without a copy.
- Receipt: accumulated bytes are the shared projection read by `NodeId.Content`, `ContentAddress`, and `StructuralMerge`; cross-runtime vectors pin float canon and counted bag layouts.
- Packages: System.Buffers supplies `ArrayBufferWriter` and `IBufferWriter<byte>`; System.Buffers.Binary supplies fixed-width writes; System.Text supplies UTF-8; `MeasureValue` supplies quantization, dimensions, and quantity types.
- Growth: a new primitive encoding is one method on `CanonicalWriter`; a new seam value type contributes one `CanonicalBytes(CanonicalWriter)` method co-located with its owner composing the existing primitives, never a parallel codec and never a per-type ad-hoc serialization.
- Boundary: `CanonicalWriter` is the one canonical codec; ad-hoc serialization, `double.GetHashCode`, and culture-formatted strings are deleted forms. Every collection writes `Ordinal(count)` before its rows and every optional column its presence bit, preserving raw-append injectivity. Scalars normalize `-0.0`, `NaN`, and infinities; value admission owns rejection. Reference semantics permit capture in union `Switch` projections.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
// ONE canonical value codec keeps identity, address, and merge keys aligned.
// Reference semantics permit sibling `CanonicalBytes` folds to capture the writer in union switches.
public sealed class CanonicalWriter(double tolerance) {
 private readonly ArrayBufferWriter<byte> buffer = new();
 public double Tolerance => tolerance;

 public CanonicalWriter Ordinal(int value) { Span<byte> s = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(s, value); buffer.Write(s); return this; }

 public CanonicalWriter U128(UInt128 value) { Span<byte> s = stackalloc byte[16]; BinaryPrimitives.WriteUInt128LittleEndian(s, value); buffer.Write(s); return this; }

 // Fixed-width 64-bit canon carries instant ticks under the same little-endian discipline as `Ordinal` and `U128`.
 public CanonicalWriter I64(long value) { Span<byte> s = stackalloc byte[8]; BinaryPrimitives.WriteInt64LittleEndian(s, value); buffer.Write(s); return this; }

 public CanonicalWriter Bool(bool value) { buffer.Write([(byte)(value ? 1 : 0)]); return this; }

 public CanonicalWriter String(string value) {
  Frame(buffer, value);
  return this;
 }

 public static void Frame(IBufferWriter<byte> destination, string value) {
  ArgumentNullException.ThrowIfNull(destination);
  ArgumentNullException.ThrowIfNull(value);
  int count = Encoding.UTF8.GetByteCount(value);
  Span<byte> prefix = destination.GetSpan(sizeof(int));
  BinaryPrimitives.WriteInt32LittleEndian(prefix, count);
  destination.Advance(sizeof(int));
  destination.Advance(Encoding.UTF8.GetBytes(value, destination.GetSpan(count)));
 }

 public CanonicalWriter Double(double value) {
  double canon = value == 0.0 ? 0.0 : value;   // -0.0 → +0.0; ±∞ keep their canonical IEEE bits
  long bits = double.IsNaN(canon) ? unchecked((long)0x7FF8_0000_0000_0000) : BitConverter.DoubleToInt64Bits(canon);
  Span<byte> s = stackalloc byte[8]; BinaryPrimitives.WriteInt64LittleEndian(s, bits); buffer.Write(s);
  return this;
 }

 public CanonicalWriter Measure(MeasureValue measure) {
  MeasureValue q = measure.Quantize(tolerance);
  Dimension d = q.Dimension;
  // Identity is the QuantityType discriminator + the SI magnitude + the 7-vector, NOT the display unit string: the
  // 7-vector is NOT injective over quantity types (Torque/Energy share [L2.M.T-2], a SectionModulus and a Volume share
  // [L3], a dimension-anonymous OfSi and a named Volume share [L3]), so dropping the type token would conflate
  // physically-distinct measures under the NodeId.Content mint, the content-dedup key, and the 3-way StructuralMerge;
  // dropping the unit string keeps OfSi (SI-native) and Of (UnitsNet-coerced) — and 1000 mm vs 1 m — addressing one
  // physical measure identically. The token is length-prefixed (String), so a name boundary can never blur into the magnitude.
  CanonicalWriter signature = String(q.Type.Value).Double(q.Si)
   .Ordinal(d.Length).Ordinal(d.Mass).Ordinal(d.Time).Ordinal(d.Current).Ordinal(d.Temperature).Ordinal(d.Amount).Ordinal(d.LuminousIntensity);
  // The UNCERTAINTY BAND is preimage, not decoration. Two measures agreeing on magnitude and differing only in
  // declared band are DIFFERENT evidence, so a preimage omitting it deduped them to ONE content-keyed node and
  // silently destroyed the surviving band — a measured 42 ± 3 and a bare 42 must never share a NodeId.Content.
  // Presence-prefixed, so an unbanded measure costs one byte and every key stays derivable from the value alone.
  // Bounds and sigma arrive already on the tolerance grid from MeasureValue.Quantize (the ONE rounding owner, so
  // two bands within tolerance address identically); the coverage factor is a declared policy scalar on no physical
  // axis, so it writes raw — a length tolerance has nothing to say about k = 2.
  return q.Uncertainty.Match(
   Some: band => signature.Bool(true).String(band.Kind.Key)
    .Double(band.LowerSi).Double(band.UpperSi)
    .Optional(band.StandardDeviationSi).Optional(band.CoverageFactor),
   None: () => signature.Bool(false));
 }

 // Presence-prefixed optional scalar: the bit, then the value only when present — so an absent column can never
 // alias a written 0.0 and the layout stays self-delimiting under raw-append.
 public CanonicalWriter Optional(Option<double> value) =>
  value.Match(Some: scalar => Bool(true).Double(scalar), None: () => Bool(false));

 public CanonicalWriter Raw(ReadOnlySpan<byte> bytes) { buffer.Write(bytes); return this; }

 public ReadOnlyMemory<byte> ToBytes() => buffer.WrittenMemory;
}
```

## [05]-[IMPLEMENTATION_LAW]

- [ONE_HASHER]: every address on this seam composes the KERNEL seed-zero `XxHash128` through `ContentHash.Of` — a second hasher, a non-zero seed, or a locally-spelled digest is the named defect, because the content space is shared with the kernel `GeometryHash` and the Python and TypeScript peers and a fork is invisible until two runtimes disagree about one node's id. `GetHashCode` is process-salted in-memory state and is NEVER persisted, wire-compared, or read as identity; `Generator.Equals` stays an orthogonal field diff over the same member set.
- [THREE_PROJECTIONS]: three distinct projections share the ONE `CanonicalWriter` and are never conflated. The id-INCLUSIVE node address writes the id before the node's canonical bytes, because graph dedup must distinguish two occurrences whose content is identical. The id-EXCLUSIVE `NodeId.Content` mint hashes the content bytes ALONE, because there the id DERIVES from them. The GRAPH address folds the header and the sorted member sets. A consumer reaching for "the node's hash" without naming which of the three is the recurring error this split forecloses.
- [ORDER_INDEPENDENCE]: the snapshot address SORTS — node addresses by ascending `UInt128`, edge byte runs lexicographically through `ByteOrder` — with a section count before each run, so the layout is self-delimiting and identical content addresses identically whatever order it was inserted, decoded, or federated in. Sorting, not a commutative hash, is the mechanism: a commutative fold would buy cheap incrementality and lose the section framing, admit multiset collisions, and re-key every persisted address in the estate.
- [PROVENANCE_EXCLUSION]: the graph address folds the SEMANTIC header (schema, model view, tolerance, georeference) and EXCLUDES `StepHeader`/`Instant` provenance — the graph-altitude mirror of the node-level `OwnerHistory` exclusion — so a re-export under a new timestamp or author addresses identically while a schema, view, georeference, or tolerance change forks identity honestly. The node-level mirror carries `Object.Placement` in the same exclusion, so a rigid move is a `Moved` verdict and not a re-key.
- [PREIMAGE_COVERAGE]: a value's preimage covers EVERY axis its admission can distinguish. Two values that a consumer can tell apart and the writer cannot are one node under the content-keyed mint, and the surviving one's dropped axis is destroyed silently — which is why `Measure` writes the uncertainty band beside the magnitude, every optional column carries a presence bit, and every collection carries its count. Widening a preimage RE-KEYS every node that reaches it exactly once: no pin window is declared anywhere in the estate (the `RULINGS` `ValueBag` identity row establishes that precedent and its terms), so the widening lands as an edit and every corpus snapshot key derives at its own landing rather than as a migration.
- [VERIFY_REGIME]: `Verify` re-mints by the regime that MINTED the id, never by one uniform re-hash — a TYPE `Object` through `NodeId.RootedType` over its volatile-excluded seed, a non-rooted node through `NodeId.OfContent` over its full canonical bytes, an OCCURRENCE `Object` vacuously because a random Guid-v7 has no content preimage. The single-node `Verify` is a `Fin` (one dependent check, fail-fast) and the snapshot sweep a `Validation` (independent node checks accumulate, so a corrupt snapshot reports every drifted node at once) — the carrier selects the algebra, never a flag. The tolerance MUST be the mint-time `Header.Tolerance` on every content-derived arm, or the quantized re-projection drifts and a sound node reads unstable.

## [06]-[RESEARCH]

(none)

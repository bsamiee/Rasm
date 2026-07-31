# [ELEMENT_ADDRESS]

`ContentAddress` hashes canonical seam bytes through kernel seed-zero `XxHash128`, shared by geometry, snapshots, precomputed content keys, and cross-runtime peers. No second hasher or non-zero seed exists.

`CanonicalWriter` owns deterministic little-endian projection, canonical IEEE-754 values, tolerance-quantized measures, count-framed strings and collections, and explicit attribute order. Non-finite scalars normalize; `Associate.ReferenceExtent` depends on the stable unset-`NaN` canon.

Every `CanonicalBytes` contribution composes this codec, so identity, content address, and `StructuralMerge` keys share one encoding. `Generator.Equals` remains an orthogonal field diff. Graph addressing folds semantic `Header`, excludes provenance, sorts nodes and edges, and verifies stored identities through `ElementFault.AddressUnstable`.

## [01]-[INDEX]

- [02]-[CONTENT_ADDRESS]: the `ContentAddress` `[ValueObject<UInt128>]` over the kernel seed-zero `XxHash128`, the raw-hash/precomputed-wrap/node/graph/verification entries, the id-inclusive node and order-independent graph addressing (semantic header folded, provenance excluded), and the `Verify` re-derive dual that re-mints by the minting regime and rails `ElementFault.AddressUnstable` on a mismatch.
- [03]-[CANONICAL_WRITER]: the `CanonicalWriter` ONE deterministic byte-projection codec (IEEE-754 LE, sign/`NaN`/∞ canon, tolerance-quantized measures, length-prefixed strings and count-prefixed collections, explicit attribute order) every seam value's `CanonicalBytes`/`ToCanonicalBytes` composes, so identity, content address, and 3-way-merge key project through one encoding.

## [02]-[CONTENT_ADDRESS]

- Owner: `ContentAddress` is the `[ValueObject<UInt128>]` content key over kernel seed-zero `XxHash128`; `ByteOrder` is the shared edge-byte comparer for snapshot and `GraphDelta.ToCanonicalBytes` sorting.
- Entry: `ContentAddress.Of(ReadOnlySpan<byte>)` hashes canonical bytes; `Of(UInt128)` wraps a precomputed hash; `Of(Node, tolerance)` addresses an id-inclusive node; `OfGraph(ElementGraph)` addresses an order-independent snapshot and `OfGraph(GraphAddress prior, GraphDelta delta, Header header)` is its incremental arity re-deriving only the members the delta names, both over ONE fold body; `Verify(Node, tolerance, key)` re-derives one identity; `Verify(ElementGraph, key)` accumulates snapshot mismatches.
- Auto: `Of(Node)` writes the id before `node.ToCanonicalBytes(tolerance)`. `OfGraph` writes `Header.CanonicalBytes`, sorted node addresses, and lexicographically sorted edge bytes with section counts. The incremental arity carries the `GraphMembers` accumulator — the per-node address map and the per-edge canonical-byte set the fold consumes — never a prior digest: a digest is a terminal projection, so the sorted member set the full-state address folds is exactly what a prior address discarded, and only the accumulator can carry it forward. `Advance` re-addresses the delta's added and revised nodes, drops its removed ids, and swaps its edge byte projections through the SAME `Relationship.ToCanonicalBytes` the merge key uses; a moved `Header.Tolerance` rails `ElementFault.AddressUnstable` because every member's canonical bytes quantize through it, so no member survives a tolerance change and the caller re-derives whole. `Verify` re-mints Types through `NodeId.RootedType`, non-rooted nodes through `NodeId.OfContent`, and admits Occurrences vacuously because their random Guid-v7 has no content preimage.
- Receipt: a `ContentAddress` is the stable cross-runtime content key — a `NodeId.Content` for a non-rooted node, a node's dedup/diff key, a snapshot's identity the `Rasm.Persistence` spine and the `Rasm.Compute` assessment cache key on; the `Verify` `Fin`/`Validation` is the rehydrate integrity verdict a content-keyed store reads before trusting a persisted id.
- Packages: `Rasm` supplies kernel `Domain.ContentHash` and `Op`; Thinktecture.Runtime.Extensions generates `[ValueObject<UInt128>]` members; LanguageExt.Core supplies `Fin`, `Validation`, `Error`, `Unit`, `Seq.Traverse`, and `.As()`.
- Growth: a new structural identity adds one input-shaped `Of` or `Verify` overload; a precomputed key composes `Of(UInt128)`; canonical vocabulary grows only on `CanonicalWriter`.
- Boundary: `ContentAddress` composes the KERNEL seed-zero `XxHash128` — a second hasher or a non-zero seed is the named defect, and `GetHashCode` is process-salted and NEVER persisted or wire-compared; the id-INCLUSIVE node address (graph dedup distinguishes nodes by id) and the id-EXCLUSIVE non-rooted `NodeId.Content` mint (the id derives from content) are two distinct projections sharing the one `CanonicalWriter`, never conflated; the graph address folds the SEMANTIC header and EXCLUDES the `StepHeader`/`Instant` provenance — the graph-altitude mirror of the node-level `OwnerHistory` exclusion; the single-node `Verify` is a `Fin` (fail-fast) and the graph sweep a `Validation` (independent node checks accumulate), the carrier selecting the algebra.

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
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element.Projection;

// --- [TYPES] ------------------------------------------------------------------------------
// The composable accumulator the content-address contract owns — the per-node address map and the per-edge
// canonical-byte set the snapshot fold consumes, held UNSORTED (the fold sorts) and keyed so a delta revises
// exactly the members it names. This is the value the incremental derivation carries forward, because a digest
// is terminal: a prior address plus a delta cannot rebuild the sorted member set the fold reads.
public readonly record struct GraphMembers(Header Header, HashMap<NodeId, UInt128> Nodes, Seq<ReadOnlyMemory<byte>> Edges) {
 public static GraphMembers Of(ElementGraph graph) =>
  new(graph.Header,
      toSeq(graph.Nodes).Fold(HashMap<NodeId, UInt128>(),
       (acc, pair) => acc.Add(pair.Key, ContentAddress.Of(pair.Value, graph.Header.Tolerance).Value)),
      graph.Edges.Map(e => e.ToCanonicalBytes(graph.Header.Tolerance)));

 // A delta touches a BOUNDED member set, so the advance re-derives that set alone: added and revised nodes
 // re-address, removed nodes drop, and the edge set drops the removed byte projections and appends the added
 // ones — the SAME Relationship.ToCanonicalBytes projection the full fold and the edge merge-key already share,
 // so a removal matches by exact bytes and never by reference. A tolerance change is the one arm that cannot
 // reuse anything: tolerance is an input to every member's canonical bytes, so a member quantized at the prior
 // tolerance would address a graph nobody has, and that arm rails to the caller for a full re-derivation.
 public static Fin<GraphMembers> Advance(GraphMembers prior, GraphDelta delta, Header header) =>
  header.Tolerance != prior.Header.Tolerance
   ? ElementFault.AddressUnstable(Op.Of(nameof(Advance)), $"<address-tolerance-moved:{prior.Header.Tolerance:R}:{header.Tolerance:R}>")
   : Fin.Succ(new GraphMembers(
      header,
      delta.RemovedNodes.Fold(
       delta.RevisedNodes.Map(static r => r.After).Append(delta.AddedNodes).Fold(prior.Nodes,
        (acc, node) => acc.AddOrUpdate(node.Id, ContentAddress.Of(node, header.Tolerance).Value)),
       static (acc, id) => acc.Remove(id)),
      prior.Edges
       .Filter(bytes => !delta.RemovedEdges
        .Exists(e => ContentAddress.ByteOrder.Compare(e.ToCanonicalBytes(header.Tolerance), bytes) == 0))
       .Append(delta.AddedEdges.Map(e => e.ToCanonicalBytes(header.Tolerance)))));
}

// The snapshot address BESIDE the accumulator that produced it: a caller holding this between passes can run
// the incremental derivation, and a caller holding the address alone cannot — which is the whole distinction.
public readonly record struct GraphAddress(ContentAddress Address, GraphMembers Members);

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
 public static ContentAddress OfGraph(ElementGraph graph) => OfGraph(GraphMembers.Of(graph)).Address;

 // The INCREMENTAL entry over the SAME fold, arity discriminating on the input: a full snapshot re-derives every
 // member, a (prior witness, delta) pair re-derives only the members the delta names and reuses the rest. Both
 // land on one body, so the two derivations cannot diverge.
 //
 // The accumulator is the member witness, NEVER the prior digest: a digest is a terminal projection, so a
 // full-state address cannot be reconstructed from a prior address plus a delta — the sorted member set the fold
 // consumes is exactly what the digest discarded. Carrying that set forward is what makes the incremental form
 // sound, which is why the witness and not the address is the value a caller holds between passes; a prior
 // `UInt128` in this parameter slot is unimplementable, not merely slower.
 public static Fin<GraphAddress> OfGraph(GraphAddress prior, GraphDelta delta, Header header) =>
  GraphMembers.Advance(prior.Members, delta, header).Map(OfGraph);

 static GraphAddress OfGraph(GraphMembers members) {
  CanonicalWriter w = new(members.Header.Tolerance);
  members.Header.CanonicalBytes(w);
  w.Ordinal(members.Nodes.Count);
  // Default UInt128 ascending comparison is the canonical cross-runtime node order;
  // node and edge sorts are ONE ordering discipline.
  foreach (UInt128 nodeAddress in members.Nodes.Values.AsIterable().OrderBy(static a => a)) { w.U128(nodeAddress); }
  // SAME `Relationship.ToCanonicalBytes(tolerance)` projection keys merge edges, so graph address
  // and edge merge-key never diverge. Threading Header.Tolerance matters ONLY for the Generic passthrough (its
  // PropertyValue.Measure attributes quantize through w.Measure; the five typed cases carry no Measure and are
  // tolerance-insensitive) — the tolerance-0 hardcode that silently forked a below-tolerance Generic edge is the
  // deleted form.
  w.Ordinal(members.Edges.Count);
  foreach (ReadOnlyMemory<byte> edge in members.Edges.OrderBy(static b => b, ByteOrder)) { w.Raw(edge.Span); }
  return new GraphAddress(Of(w.ToBytes().Span), members);
 }

 // H7 verification re-derives a node's stored id from current content through the SAME identity
 // regime that minted it — an OCCURRENCE Object (random Guid-v7 placement identity, no content preimage, H6) verifies
 // vacuously; a TYPE Object re-mints NodeId.RootedType over the Representations-EXCLUDED ToTypeSeedBytes seed (the
 // omitted volatile block keeps a later geometry attach from spuriously failing it; a forged or corrupted Type id is
 // caught); a NON-ROOTED node re-projects node.ToCanonicalBytes through THIS owner's Of and compares through
 // NodeId.OfContent. Each content arm composes the seam's own mint entries and compares NodeId to NodeId under the
 // declared [KeyMemberEqualityComparer] — the X32 spelling is owned by NodeId, never re-spelled here. The tolerance
 // MUST be the mint-time Header.Tolerance for the content-derived arms, or the quantized re-projection drifts. The
 // single mismatch producer is `ElementFault.AddressUnstable`.
 // WIRE face ([ObjectFactory<string>] — IObjectFactory<ContentAddress, string, ValidationError> and
 // IConvertible<string>, decompile-verified contracts): a raw UInt128 JSON number loses precision past 2^53 in a
 // JS JSON.parse, so EVERY serializer framework renders/parses the canonical X32 hex through this factory — the
 // TS/Python peers hold the key as the hex string and the ModelDiff wire crosses it losslessly; every consumer
 // picks this row through the generated converters with zero local edits.
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
  node switch {
   Node.Object o when o.Kind == ObjectKind.Occurrence => Fin.Succ(unit),
   Node.Object o => NodeId.RootedType(o.ToTypeSeedBytes(tolerance).Span) == o.Id
    ? Fin.Succ(unit)
    : ElementFault.AddressUnstable(key, $"<type-id-mismatch:{o.Id.Value}>"),
   // Every non-rooted case verifies by content, so a new Node case lands on this list or Verify throws
   // SwitchExpressionException at runtime — the arm is deliberately default-free to force that landing.
   Node.Material or Node.PropertySet or Node.QuantitySet or Node.Assessment or Node.Appearance
    or Node.Coverage or Node.Observation => VerifyContent(node, tolerance, key)
  };

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

## [03]-[CANONICAL_WRITER]

- Owner: `CanonicalWriter` is the one tolerance-bound byte codec composed by `Node`, `Relationship`, `PropertyValue`, `MaterialComposition`, `MeasureValue`, `GeoReference`, and `CoverageGrid` canonical projections, so identity, address, and merge keys share one encoding.
- Entry: `new CanonicalWriter(tolerance)` opens a projection bound to the model tolerance; the `Double`/`String`/`Ordinal`/`I64`/`U128`/`Bool`/`Measure`/`Raw` primitives write canonically and each returns the writer for fluent chaining; `Frame(IBufferWriter<byte>, string)` exposes the same string/count preimage framing without opening a model projection; `ToBytes()` reads the accumulated `WrittenMemory`; `Tolerance` exposes the bound grid a sibling `CanonicalBytes` fold reads.
- Auto: `Double` canonicalizes `-0.0`→`0.0` and every `NaN`→one quiet-NaN pattern then writes the IEEE-754 little-endian bits (±∞ keep their already-canonical bits), so a sign-of-zero or a payload-`NaN` never forks the hash and an unset-`NaN` sentinel canonicalizes stably; `Measure` quantizes the SI magnitude to the tolerance grid through `MeasureValue.Quantize` then writes the length-prefixed `QuantityType` discriminator token, the magnitude, and the seven `Dimension` exponents — NOT the display unit string — so a `Torque` and an `Energy` (or a `SectionModulus` and a `Volume`, or a dimension-anonymous `OfSi` and a named `Volume`) that share a `Dimension` stay distinct under the `NodeId.Content` mint, the content-dedup key, and the 3-way merge, while the SI-native `OfSi` and the UnitsNet-coerced `Of` still project one physical measure to one byte sequence and two measures within tolerance address identically; `String` composes `Frame`, which writes the little-endian UTF-8 byte count before the bytes so a delimiter collision cannot forge equality; `Ordinal`/`I64`/`U128`/`Bool` write fixed-width little-endian (the count prefixes that make a collection layout self-delimiting, `I64` the instant/tick canon a temporal row writes); `ToBytes` reads the accumulated memory without a copy.
- Receipt: accumulated bytes are the shared projection read by `NodeId.Content`, `ContentAddress`, and `StructuralMerge`; cross-runtime vectors pin float canon and counted bag layouts.
- Packages: System.Buffers supplies `ArrayBufferWriter` and `IBufferWriter<byte>`; System.Buffers.Binary supplies fixed-width writes; System.Text supplies UTF-8; `MeasureValue` supplies quantization, dimensions, and quantity types.
- Growth: a new primitive encoding is one method on `CanonicalWriter`; a new seam value type contributes one `CanonicalBytes(CanonicalWriter)` method co-located with its owner composing the existing primitives, never a parallel codec and never a per-type ad-hoc serialization.
- Boundary: `CanonicalWriter` is the one canonical codec; ad-hoc serialization, `double.GetHashCode`, and culture-formatted strings are deleted forms. Every collection writes `Ordinal(count)` before its rows, preserving raw-append injectivity. Scalars normalize `-0.0`, `NaN`, and infinities; value admission owns rejection. Reference semantics permit capture in union `Switch` projections.

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

 // Single-precision payload lanes (raster sample grids) keep their width under the same canon discipline;
 // widening to `Double` would double a stored grid for no identity gain.
 public CanonicalWriter Single(float value) {
  float canon = value == 0.0f ? 0.0f : value;   // -0.0f → +0.0f; ±∞ keep their canonical IEEE bits
  int bits = float.IsNaN(canon) ? unchecked((int)0x7FC0_0000) : BitConverter.SingleToInt32Bits(canon);
  Span<byte> s = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(s, bits); buffer.Write(s);
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
  return String(q.Type.Value).Double(q.Si)
   .Ordinal(d.Length).Ordinal(d.Mass).Ordinal(d.Time).Ordinal(d.Current).Ordinal(d.Temperature).Ordinal(d.Amount).Ordinal(d.LuminousIntensity);
 }

 public CanonicalWriter Raw(ReadOnlySpan<byte> bytes) { buffer.Write(bytes); return this; }

 public ReadOnlyMemory<byte> ToBytes() => buffer.WrittenMemory;
}
```

## [04]-[RESEARCH]

(none)

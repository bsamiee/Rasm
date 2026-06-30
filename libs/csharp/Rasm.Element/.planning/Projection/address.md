# [ELEMENT_ADDRESS]

The content-identity owner: the `ContentAddress` `[ValueObject<UInt128>]` that hashes any seam value's canonical bytes through the kernel seed-zero `XxHash128`, and the `CanonicalWriter` deterministic byte projection it composes — fixed IEEE-754 little-endian, `-0.0`→`0.0`, every `NaN`→one quiet pattern, ±∞ at its canonical IEEE bits, measures quantized to `Header.Tolerance`, strings and collections count-prefixed, attributes in explicit order. This is the ONE canonical value codec: the `Graph/element#NODE_MODEL` `Node.ToCanonicalBytes` projection, the non-rooted `NodeId.Content` mint, the `Relations/relation#EDGE_ALGEBRA` `Relationship.ToCanonicalBytes` edge key, and every sibling `CanonicalBytes` contribution (`Properties/property#PROPERTY_VALUE` `PropertyValue`, `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition`, `Properties/quantity#MEASURE_VALUE` `MeasureValue.Quantize`, `Geospatial/coverage#COVERAGE_NODE` `CoverageGrid`) ALL write through this one projection, so a node's identity, its content address, and the bytes the `Rasm.Persistence` content-3-way `StructuralMerge` keys an edge or node on are the SAME canonical encoding — never three divergent serializations. `Generator.Equals` is the ORTHOGONAL structural member diff — field-level `[Equatable]`/`Inequalities`, not a byte projection — composed ALONGSIDE these content keys (the merge keys content by the bytes and localizes member changes structurally), never conflated with the codec. The `ContentAddress` composes the kernel's seed-zero `XxHash128` content-hash entry, the ONE hasher shared with the geometry `GeometryHash`, the snapshot spine, the `RepresentationContentHash`/`Coverage.RasterKey`/`Assessment.InputKey` content keys, and the cross-runtime Python/TypeScript peers — the seam mints no second hasher and seeds no non-zero. The graph content address is order-INDEPENDENT over the node set and edge set: node addresses sort by their `UInt128`, edges by their canonical bytes, the SEMANTIC `Header` (schema, model view, tolerance, georeference) folded ahead and the `StepHeader`/`Instant` provenance excluded — so two graphs with the same content but different insertion order, or a re-export under a new timestamp, address identically, while a schema or CRS change forks the identity. The dual of the mint is `Verify`: re-projecting a non-rooted node and confirming its recomputed content id still equals its stored `NodeId` is the tamper/corruption gate a `Rasm.Persistence` rehydrate and the cross-runtime parity corpus run, a mismatch railing `Projection/fault#FAULT_BAND` `ElementFault.AddressUnstable`. The writer NORMALIZES rather than rails a non-finite scalar — the `-0.0`/`NaN`/±∞ canon IS the cross-runtime stability guarantee AND the canonicalization the `Associate` edge's unset `ReferenceExtent` `NaN` sentinel relies on, so "a non-finite forks the hash" is structurally impossible, not a fault path.

## [01]-[INDEX]

- [01]-[CONTENT_ADDRESS]: the `ContentAddress` `[ValueObject<UInt128>]` over the kernel seed-zero `XxHash128`, the raw-hash/precomputed-wrap/node/graph/verification entries, the id-inclusive node and order-independent graph addressing (semantic header folded, provenance excluded), and the `Verify` re-hash dual that rails `ElementFault.AddressUnstable` on a content-id mismatch.
- [02]-[CANONICAL_WRITER]: the `CanonicalWriter` ONE deterministic byte-projection codec (IEEE-754 LE, sign/`NaN`/∞ canon, tolerance-quantized measures, length-prefixed strings and count-prefixed collections, explicit attribute order) every seam value's `CanonicalBytes`/`ToCanonicalBytes` composes, so identity, content address, and 3-way-merge key project through one encoding.

## [02]-[CONTENT_ADDRESS]

- Owner: `ContentAddress` the `[ValueObject<UInt128>]` content key over the kernel seed-zero `XxHash128`; the raw-hash, precomputed-wrap, node, graph, and verification entries; the shared `ByteOrder` edge-bytes comparer the snapshot edge sort and the `Graph/delta#GRAPH_DELTA` `GraphDelta.ToCanonicalBytes` edge sort both compose.
- Entry: `ContentAddress.Of(ReadOnlySpan<byte>)` is the raw hashing entry the `Graph/element#NODE_MODEL` `NodeId.Content` mint shares; `Of(UInt128)` wraps a PRECOMPUTED content hash (a `RepresentationContentHash` body key, a `Coverage.RasterKey`, an `Assessment.InputKey`) without re-hashing; `Of(Node, tolerance)` is the id-INCLUSIVE node address (the graph-dedup key distinguishing two occurrences with identical content by their ids); `OfGraph(ElementGraph)` the order-INDEPENDENT snapshot address (semantic `Header` + sorted node addresses + sorted edge bytes); `Verify(Node, tolerance, key)` the single-node re-hash gate (`Fin<Unit>`) and `Verify(ElementGraph, key)` the snapshot sweep (`Validation<Error, Unit>`).
- Auto: `Of(Node)` writes the id then appends `node.ToCanonicalBytes(tolerance)` (the `Graph/element#NODE_MODEL` projection) so two occurrences with identical content stay distinct by id, while `OfGraph` folds the semantic `Header` (schema/view/tolerance/georeference) then sorts the node `ContentAddress`es by `UInt128` and the edge canonical bytes lexicographically through `ByteOrder`, the section counts making the node-vs-edge layout self-delimiting; `Verify` re-projects a non-rooted node through `node.ToCanonicalBytes` and the kernel `ContentHash.Of`, comparing the recomputed `X32` to the stored `NodeId.Value` (a rooted `Node.Object` carries a neutral Guid-v7, not a content hash, so it verifies vacuously), the graph overload accumulating every mismatch applicatively; every byte the address consumes is written through the `Projection/address#CANONICAL_WRITER` `CanonicalWriter`, so a node's identity, its content address, and the bytes the `Rasm.Persistence` 3-way `StructuralMerge` keys an edge or node on are ONE encoding — never three divergent serializations.
- Receipt: a `ContentAddress` is the stable cross-runtime content key — a `NodeId.Content` for a non-rooted node, a node's dedup/diff key, a snapshot's identity the `Rasm.Persistence` spine and the `Rasm.Compute` assessment cache key on; the `Verify` `Fin`/`Validation` is the rehydrate integrity verdict a content-keyed store reads before trusting a persisted id; a float-bearing golden vector (an `IfcMaterialLayer`-shaped node) anchors the cross-runtime parity corpus so the C#/Python/TypeScript peers agree byte-for-byte.
- Packages: `Rasm` (the kernel `Domain.ContentHash` seed-zero entry — composed, never a second hasher — plus the `Op` op-key), Thinktecture.Runtime.Extensions (`[ValueObject<UInt128>]` + the generated `Create`/`Value`), LanguageExt.Core (`Fin`/`Validation`/`Error`/`Unit` + the `Seq.Traverse`/`.As()` applicative accumulation the snapshot `Verify` sweep folds every node check through).
- Growth: a new structural identity (a node, an edge, a snapshot, a verification) is one `Of`/`Verify` overload discriminating on input shape; a new precomputed content key is one `Of(UInt128)` caller; never a per-call-site hash, never a second hasher, and never a parallel content-key scheme — the `XxHash128` seed-zero entry is the one identity rail and the addressing grows by the `Projection/address#CANONICAL_WRITER` writer's vocabulary.
- Boundary: `ContentAddress` composes the KERNEL seed-zero `XxHash128` — a second hasher or a non-zero seed is the named defect, and `GetHashCode` is process-salted and NEVER persisted or wire-compared, content addressing being the `XxHash128` canonical-bytes rail only; the node address is id-INCLUSIVE (graph dedup distinguishes nodes by id) while the non-rooted `NodeId.Content` mint is id-EXCLUSIVE (the id derives from content) — two distinct projections sharing the one `CanonicalWriter`, never conflated; the graph address is order-INDEPENDENT (sorted node addresses + sorted edge bytes) and folds the SEMANTIC header while EXCLUDING the `StepHeader`/`Instant` provenance (the graph-altitude mirror of the node-level `OwnerHistory` exclusion), so a re-ordered insertion or a re-export under a new timestamp never forks identity but a schema/CRS change does; `Verify` is the H7 re-hash dual the corruption gate reads — `Projection/fault#FAULT_BAND` `ElementFault.AddressUnstable` its only producer, the single-node check a `Fin` (no accumulation) and the graph sweep a `Validation` (independent node checks accumulate, the carrier selecting the algebra); `Generator.Equals` is the ORTHOGONAL structural member diff (field-level `[Equatable]`/`Inequalities`, NOT a byte projection) the merge composes ALONGSIDE this content key, never conflated with the codec.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Globalization;
using System.Text;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<UInt128>]
public sealed partial class ContentAddress {
 // The lexicographic byte-sequence ordering for the order-independent edge canon — a cached singleton (zero
 // per-call allocation) over the BCL span comparison, the one byte ordering BOTH this OfGraph edge sort AND the
 // Graph/delta#GRAPH_DELTA GraphDelta.ToCanonicalBytes edge sort share — internal (same-assembly), never private.
 internal static readonly IComparer<ReadOnlyMemory<byte>> ByteOrder =
  Comparer<ReadOnlyMemory<byte>>.Create(static (x, y) => x.Span.SequenceCompareTo(y.Span));

 // The kernel `ContentHash` seed-zero entry over the canonical bytes — the ONE hasher.
 public static ContentAddress Of(ReadOnlySpan<byte> canonicalBytes) => Create(ContentHash.Of(canonicalBytes));

 // Wrap a PRECOMPUTED content hash (a RepresentationContentHash body key, a Coverage.RasterKey, an
 // Assessment.InputKey) as a ContentAddress without re-hashing — the carrier over an already-derived UInt128,
 // distinct from the hashing entry above.
 public static ContentAddress Of(UInt128 contentHash) => Create(contentHash);

 // The id-INCLUSIVE node address (the graph-dedup key): the id then `node.ToCanonicalBytes`, so two occurrences
 // with identical content stay distinct by id. DISTINCT from the id-EXCLUSIVE non-rooted `NodeId.Content` mint,
 // which hashes the content bytes ALONE (the id derives from them) — the two share the one writer, never the one hash.
 public static ContentAddress Of(Node node, double tolerance) {
  CanonicalWriter w = new(tolerance);
  w.String(node.Id.Value).Raw(node.ToCanonicalBytes(tolerance).Span);
  return Of(w.ToBytes().Span);
 }

 // The order-INDEPENDENT snapshot address: the semantic Header folded first, then node addresses sorted by
 // UInt128, then edge canonical bytes sorted lexicographically. The section counts make the layout
 // self-delimiting — a node-vs-edge boundary can never blur under concatenation — so two graphs with identical
 // content but different insertion order address identically, while a schema/view/georeference change forks identity.
 public static ContentAddress OfGraph(ElementGraph graph) {
  CanonicalWriter w = new(graph.Header.Tolerance);
  HeaderBytes(w, graph.Header);
  w.Ordinal(graph.Nodes.Count);
  foreach (UInt128 nodeAddress in graph.Nodes.Values.Map(n => Of(n, graph.Header.Tolerance).Value).Order()) { w.U128(nodeAddress); }
  // The edge's standalone canonical bytes are the SAME Relations/relation#EDGE_ALGEBRA Relationship.ToCanonicalBytes()
  // projection a content-3-way merge keys an edge on — graph address and edge merge-key never diverge. Edges carry
  // no tolerance-bearing measure, so the per-edge writer tolerance is 0 (a Generic edge's PropertyValue measures
  // quantize against their OWN node's tolerance at mint, not the edge's).
  w.Ordinal(graph.Edges.Length);
  foreach (ReadOnlyMemory<byte> edge in graph.Edges.Map(static e => e.ToCanonicalBytes()).OrderBy(static b => b, ByteOrder)) { w.Raw(edge.Span); }
  return Of(w.ToBytes().Span);
 }

 // The H7 verification dual of the mint: re-project a NON-ROOTED node and confirm its recomputed content id still
 // equals its stored NodeId — the tamper/corruption gate a Rasm.Persistence rehydrate and the cross-runtime parity
 // corpus run before trusting a persisted id. A rooted Node.Object carries a neutral Guid-v7 (NOT content-derived,
 // H6), so it verifies vacuously; the tolerance MUST be the graph's mint-time Header.Tolerance, or the quantized
 // re-projection drifts. The recompute matches the NodeId.Content mint EXACTLY (kernel ContentHash.Of, X32 invariant).
 public static Fin<Unit> Verify(Node node, double tolerance, Op key) =>
  node is Node.Object
   ? Fin.Succ(unit)
   : ContentHash.Of(node.ToCanonicalBytes(tolerance).Span).ToString("X32", CultureInfo.InvariantCulture) == node.Id.Value
    ? Fin.Succ(unit)
    : ElementFault.AddressUnstable(key, $"<content-id-mismatch:{node.Id.Value}>");

 // The snapshot rehydrate gate: every non-rooted node's stored id re-verified, the Validation ACCUMULATING all
 // mismatches (a corrupt snapshot reports every drifted node at once — the Projection/projection#GRAPH_CONSTRAINT
 // accumulate-all shape, independent checks licensing accumulation), the caller converting to Fin (.ToFin()) at the
 // boundary so an unstable snapshot never enters the read path as trusted.
 public static Validation<Error, Unit> Verify(ElementGraph graph, Op key) =>
  graph.Nodes.Values.ToSeq()
   .Traverse(n => Verify(n, graph.Header.Tolerance, key).Match(
    Succ: static _ => Success<Error, Unit>(unit),
    Fail: static e => Fail<Error, Unit>(e)))
   .As()
   .Map(static _ => unit);

 // The SEMANTIC header identity folded into the graph address — schema, model view, tolerance, and the full GeoReference
 // map-conversion tuple projected through the ONE Geospatial/reference#GEO_REFERENCE GeoReference.CanonicalBytes (Epsg the
 // CRS identity, the resolved CRS-name string EXCLUDED) — so two graphs differing in schema or resolved EPSG address
 // distinctly while two that resolve the SAME EPSG address identically whether or not the name string differs; the
 // StepHeader/Instant PROVENANCE is EXCLUDED (the node-level OwnerHistory exclusion at graph altitude), so a re-export under
 // a new timestamp/author never forks the snapshot identity. The SAME projection the Graph/delta#GRAPH_DELTA header
 // contribution composes, so the delta header key and this snapshot header contribution never diverge.
 static void HeaderBytes(CanonicalWriter w, Header h) {
  w.String(h.Schema.Key).String(h.View.Key).Double(h.Tolerance);
  h.Reference.CanonicalBytes(w);
 }
}
```

## [03]-[CANONICAL_WRITER]

- Owner: `CanonicalWriter` the ONE deterministic byte-projection codec bound to the model tolerance — every seam value's `CanonicalBytes`/`ToCanonicalBytes` writes through it: the `Graph/element#NODE_MODEL` `Node.ToCanonicalBytes`, the `Relations/relation#EDGE_ALGEBRA` `Relationship.ToCanonicalBytes`, and the `Properties/property#PROPERTY_VALUE` `PropertyValue`, `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition`, `Properties/quantity#MEASURE_VALUE` `MeasureValue`, `Geospatial/reference#GEO_REFERENCE` `GeoReference`, and `Geospatial/coverage#COVERAGE_NODE` `CoverageGrid` contributions — so identity, content address, and 3-way-merge key project through one encoding.
- Entry: `new CanonicalWriter(tolerance)` opens a projection bound to the model tolerance; the `Double`/`String`/`Ordinal`/`U128`/`Bool`/`Measure`/`Raw` primitives write canonically and each returns the writer for fluent chaining; `ToBytes()` reads the accumulated `WrittenMemory`; `Tolerance` exposes the bound grid a sibling `CanonicalBytes` fold reads.
- Auto: `Double` canonicalizes `-0.0`→`0.0` and every `NaN`→one quiet-NaN pattern then writes the IEEE-754 little-endian bits (±∞ keep their already-canonical bits), so a sign-of-zero or a payload-`NaN` never forks the hash and an unset-`NaN` sentinel canonicalizes stably; `Measure` quantizes the SI magnitude to the tolerance grid through `MeasureValue.Quantize` then writes the length-prefixed `QuantityType` discriminator token, the magnitude, and the seven `Dimension` exponents — NOT the display unit string — so a `Torque` and an `Energy` (or a `SectionModulus` and a `Volume`, or a dimension-anonymous `OfSi` and a named `Volume`) that share a `Dimension` stay distinct under the `NodeId.Content` mint, the content-dedup key, and the 3-way merge, while the SI-native `OfSi` and the UnitsNet-coerced `Of` still project one physical measure to one byte sequence and two measures within tolerance address identically; `String` length-prefixes the UTF-8 bytes so a delimiter collision cannot forge equality; `Ordinal`/`U128`/`Bool` write fixed-width little-endian (the count prefixes that make a collection layout self-delimiting); `ToBytes` reads the accumulated memory without a copy.
- Receipt: the writer's accumulated bytes ARE the one canonical projection — the non-rooted `Graph/element#NODE_MODEL` `NodeId.Content` mint, the `Projection/address#CONTENT_ADDRESS` `ContentAddress` node/graph address, and the `Rasm.Persistence` `StructuralMerge` edge/node key all read this single encoding, so a value cannot identify one way and hash another; the float-bearing `IfcMaterialLayer`-shaped golden vector is the cross-runtime regression anchor the C#/Python/TypeScript peers reproduce byte-for-byte.
- Packages: System.Buffers (`ArrayBufferWriter` the accumulation buffer), System.Buffers.Binary (`BinaryPrimitives` the fixed-width little-endian writes), System.Text (`Encoding.UTF8` the length-prefixed string bytes), `Properties/quantity#MEASURE_VALUE` (`MeasureValue.Quantize`/`Dimension`/`QuantityType` the `Measure` primitive reads).
- Growth: a new primitive encoding is one method on `CanonicalWriter`; a new seam value type contributes one `CanonicalBytes(CanonicalWriter)` method co-located with its owner composing the existing primitives, never a parallel codec and never a per-type ad-hoc serialization.
- Boundary: `CanonicalWriter` is the ONE canonical codec — a per-call-site or per-type ad-hoc serialization, a `double.GetHashCode`, or a culture-formatted string is the deleted form; the writer NORMALIZES a non-finite scalar (`-0.0`→`0.0`, `NaN`→one quiet pattern, ±∞ at canonical bits) rather than railing — the determinism guarantee AND the canonicalization the `Associate` edge's unset-`NaN` `ReferenceExtent` (`Relations/relation#EDGE_ALGEBRA`) relies on, so a non-finite never forks the hash and the MINT path mints no `Projection/fault#FAULT_BAND` `ElementFault` (a non-finite is rejected at its value-admission gate, never here); it is a reference type (NOT a ref struct) because the sibling `CanonicalBytes` folds capture it in the union `Switch` lambdas (`Graph/element#NODE_MODEL`, `Relations/relation#EDGE_ALGEBRA`) and a ref struct cannot be captured by a closure.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
// The ONE canonical value codec — fixed IEEE-754 LE, sign/NaN/∞ canon, tolerance-quantized measures, count-prefixed
// strings and collections, explicit attribute order. Every seam value writes through it so identity, address, and
// 3-way-merge key agree. A reference type (NOT a ref struct), because the sibling CanonicalBytes folds capture it in
// the union Switch lambdas (Graph/element#NODE_MODEL, Relations/relation#EDGE_ALGEBRA) — a ref struct cannot be
// captured by a closure.
public sealed class CanonicalWriter(double tolerance) {
 readonly ArrayBufferWriter<byte> buffer = new();
 public double Tolerance => tolerance;

 public CanonicalWriter Ordinal(int value) { Span<byte> s = stackalloc byte[4]; BinaryPrimitives.WriteInt32LittleEndian(s, value); buffer.Write(s); return this; }

 public CanonicalWriter U128(UInt128 value) { Span<byte> s = stackalloc byte[16]; BinaryPrimitives.WriteUInt128LittleEndian(s, value); buffer.Write(s); return this; }

 public CanonicalWriter Bool(bool value) { buffer.Write([(byte)(value ? 1 : 0)]); return this; }

 public CanonicalWriter String(string value) {
  int count = Encoding.UTF8.GetByteCount(value);
  Ordinal(count);
  Encoding.UTF8.GetBytes(value, buffer.GetSpan(count));
  buffer.Advance(count);
  return this;
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
  return String(q.Type.Value).Double(q.Si)
   .Ordinal(d.Length).Ordinal(d.Mass).Ordinal(d.Time).Ordinal(d.Current).Ordinal(d.Temperature).Ordinal(d.Amount).Ordinal(d.LuminousIntensity);
 }

 public CanonicalWriter Raw(ReadOnlySpan<byte> bytes) { buffer.Write(bytes); return this; }

 public ReadOnlyMemory<byte> ToBytes() => buffer.WrittenMemory;
}
```

## [04]-[RESEARCH]

- [SHARED_CANONICAL_CODEC]: one canonical projection is the source of every CONTENT identity — the non-rooted `NodeId.Content` mint (id-EXCLUSIVE: the id IS the hash of the content bytes), the `ContentAddress` node address (id-INCLUSIVE: the graph-dedup key) and graph address, and the `Node`/`Relationship.ToCanonicalBytes` content key the `Rasm.Persistence` 3-way `StructuralMerge` aligns edges and nodes on — so a value cannot identify one way and hash another; the `CanonicalWriter` fixes the float canon (`-0.0`→`0.0`, `NaN`→one quiet pattern, ±∞ at canonical IEEE bits, little-endian), quantizes every measure to `Header.Tolerance` before writing, length-prefixes strings and count-prefixes collections, and writes attributes in explicit order, so the projection is byte-stable across the C#/Python/TypeScript runtimes sharing the one `XxHash128` seed-zero rail, the float-bearing `IfcMaterialLayer`-shaped golden vector the regression anchor. `Generator.Equals` is the ORTHOGONAL structural member diff (field-level `[Equatable]`/`Inequalities`, NOT a byte projection) the merge composes ALONGSIDE these content keys to localize a change to a member path — never the canonical codec, never conflated with it.
- [SINGLE_HASHER]: the `ContentAddress` composes the KERNEL seed-zero `XxHash128` content-hash entry — the ONE hasher the geometry `GeometryHash`, the snapshot spine, the `RepresentationContentHash`/`Coverage.RasterKey`/`Assessment.InputKey` content keys, and the Python/TypeScript wire peers all share — so the seam mints no second hasher and a non-zero seed or an alternate algorithm is the named defect; the graph address is order-INDEPENDENT (sorted node addresses + lexicographically-sorted edge bytes) and folds the SEMANTIC `Header` (schema, model view, tolerance, the full `GeoReference` map-conversion tuple) while EXCLUDING the `StepHeader`/`Instant` provenance, so a snapshot's identity depends on its content and schema/CRS — not its insertion order or its re-export timestamp — the property the `Rasm.Persistence` content-keyed object store and the 3-way merge depend on; the writer NORMALIZES a non-finite scalar deterministically rather than railing, the `Associate` edge's unset-`NaN` `ReferenceExtent` (`Relations/relation#EDGE_ALGEBRA`) being the in-corpus value that relies on the canon, so a hash fork on a non-finite is structurally impossible rather than a fault path.
- [VERIFICATION_DUAL]: `Verify` is the H7 re-hash dual the mint owes — a non-rooted node's stored `NodeId` IS the hash of its canonical content, so re-projecting `node.ToCanonicalBytes` through the kernel `ContentHash.Of` and comparing the recomputed `X32` to the stored id detects a tampered or corrupted persisted node, the gate a `Rasm.Persistence` rehydrate runs before trusting an id and the assertion the cross-runtime parity corpus makes; a rooted `Node.Object` carries a neutral Guid-v7 (NOT content-derived, H6) so it verifies vacuously, and the graph overload accumulates every drifted node applicatively (the `Validation<Error,Unit>` accumulate-all shape, independent node checks licensing accumulation while the single-node check is fail-fast `Fin`), so a corrupt snapshot reports all mismatches at once; the ONLY rail is `Projection/fault#FAULT_BAND` `ElementFault.AddressUnstable`, the verification mismatch its sole producer — the prior "a non-finite measure rails `AddressUnstable`" reading is the deleted form, because the writer NORMALIZES non-finite and the upstream `MeasureValue.Of`/`PropertyValue.Of` admissions reject a non-finite at ingress, so a drifted value surfaces as a re-hash mismatch, never an un-producible byte-projection fault.

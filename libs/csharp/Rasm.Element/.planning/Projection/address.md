# [ELEMENT_ADDRESS]

The content-identity owner: the `CanonicalWriter` that projects any seam value into deterministic bytes (fixed IEEE-754 little-endian, `-0.0`→`0.0`, `NaN`→one canonical pattern, measures quantized to `Header.Tolerance`, explicit attribute order) and the `ContentAddress` `[ValueObject<UInt128>]` that hashes those bytes through the kernel seed-zero `XxHash128`. This is the ONE canonical value codec: the `Graph/element#NODE_MODEL` `Node.ToCanonicalBytes` entry, the `Graph/element#NODE_MODEL` non-rooted `NodeId.Content` mint, and the `Graph/element#ELEMENT_GRAPH` `Generator.Equals` content diff ALL project through this writer, so a node's identity, its content address, and its diff bytes are the same canonical projection — never three divergent encodings. The `ContentAddress` composes the kernel's seed-zero `XxHash128` content-hash entry, the ONE hasher shared with the geometry `GeometryHash`, the snapshot spine, and the cross-runtime Python/TypeScript peers — the seam mints no second hasher. The graph content address is order-INDEPENDENT over the node set and edge set (non-geometry canonical ordering): node addresses sort by their `UInt128`, edges by their canonical bytes, so two graphs with the same content but different insertion order address identically. The page composes every seam value type's `CanonicalBytes` contribution (`Properties/property#PROPERTY_VALUE` `PropertyValue`, `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition`, `Relations/relation#EDGE_ALGEBRA` `Relationship`) and the `Properties/quantity#MEASURE_VALUE` `MeasureValue.Quantize`; a non-finite measure surviving to the byte projection rails `Projection/fault#FAULT_BAND` `ElementFault.AddressUnstable`.

## [01]-[INDEX]

- [01]-[CONTENT_ADDRESS]: the `CanonicalWriter` deterministic byte projection (IEEE-754 LE, sign/NaN canon, tolerance quantization, explicit order), the `ContentAddress` `[ValueObject<UInt128>]` over the kernel seed-zero `XxHash128`, and the node + order-independent graph addressing.

## [02]-[CONTENT_ADDRESS]

- Owner: `CanonicalWriter` the deterministic byte projection every seam value writes through; `ContentAddress` the `[ValueObject<UInt128>]` content key over the kernel seed-zero `XxHash128`; the node and graph addressing entries.
- Entry: `new CanonicalWriter(tolerance)` opens a projection bound to the model tolerance; `Double`/`String`/`Ordinal`/`U128`/`Bool`/`Measure` write the primitives canonically; `ContentAddress.Of(node, tolerance)` hashes a node's id plus its `ToCanonicalBytes`; `ContentAddress.OfGraph(graph)` hashes the order-independent node-address and edge-byte sets; `ContentAddress.Of(UInt128)` wraps a precomputed content hash without re-hashing; `ContentAddress.Of(bytes)` is the raw hashing entry the `NodeId.Content` mint shares.
- Auto: `Double` canonicalizes `-0.0` to `0.0` and every `NaN` to one quiet-NaN pattern then writes the IEEE-754 little-endian bits, so a sign-of-zero or a payload-NaN never forks the hash; `Measure` quantizes the SI magnitude to the tolerance grid through `Properties/quantity#MEASURE_VALUE` `MeasureValue.Quantize` then writes the magnitude and the 7-vector dimension exponents — NOT the unit string, so the SI-native `MeasureValue.OfSi` and the UnitsNet-coerced `MeasureValue.Of` project one physical measure to one byte sequence — so two measures within tolerance address identically; `String` length-prefixes the UTF-8 bytes so a delimiter collision cannot forge equality; `ContentAddress.Of(node)` writes the id then appends `node.ToCanonicalBytes(tolerance)` and hashes through the kernel `ContentHash.Of` entry (seed-zero XxHash128), distinguishing two occurrences with identical content by their ids; `OfGraph` sorts the node `ContentAddress`es by their `UInt128` and the edge canonical bytes lexicographically before hashing, so the graph address is insertion-order-independent.
- Receipt: a `ContentAddress` is the stable cross-runtime content key — a `NodeId.Content` for a non-rooted node, a node's diff/dedup key, a graph snapshot's identity the `Rasm.Persistence` spine and the `Rasm.Compute` assessment cache key on, and the `RepresentationContentHash`/`Coverage.RasterKey`/`Assessment.InputKey` content keys all share the one `XxHash128` seed; a float-bearing golden vector (an `IfcMaterialLayer`-shaped node) anchors the cross-runtime parity corpus so the C#/Python/TypeScript peers agree byte-for-byte.
- Packages: `Rasm` (the kernel `Domain.ContentHash` seed-zero entry — composed, never a second hasher — plus the `Op` op-key), Thinktecture.Runtime.Extensions (`[ValueObject<UInt128>]`), System.Buffers (`ArrayBufferWriter`), System.Buffers.Binary (`BinaryPrimitives`).
- Growth: a new seam value type contributes one `CanonicalBytes(CanonicalWriter)` method co-located with its owner; a new primitive encoding is one method on `CanonicalWriter`; never a per-call-site hash and never a second hasher — the `XxHash128` seed-zero entry is the one identity rail; the addressing grows by the writer's vocabulary, not by parallel content-key schemes.
- Boundary: `CanonicalWriter` is the ONE canonical codec — the `Node.ToCanonicalBytes` entry, the `NodeId.Content` mint, and the `Generator.Equals` content diff all project through it, so a node hashes, identifies, and diffs through one encoding, and a per-call-site or per-type ad-hoc serialization is the named defect; the `Double` canon (`-0.0`→`0.0`, `NaN`→one pattern, IEEE-754 LE) and the tolerance-quantized `Measure` are the cross-runtime stability guarantee, a raw `double.GetHashCode` or a culture-formatted string being the deleted form; `ContentAddress` composes the KERNEL seed-zero `XxHash128` — the ONE hasher shared with the geometry `GeometryHash`, the snapshot spine, and the Python/TypeScript peers — and a second hasher or a non-zero seed is the named defect; the graph address is order-INDEPENDENT (sorted node addresses + sorted edge bytes) so a re-ordered insertion never forks identity; `GetHashCode` is process-salted and NEVER persisted or wire-compared — content addressing is the `XxHash128` canonical-bytes rail only.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using LanguageExt;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [SERVICES] ---------------------------------------------------------------------------
// The ONE canonical value codec — fixed IEEE-754 LE, sign/NaN canon, tolerance-quantized measures,
// explicit attribute order. Every seam value writes through it so identity, address, and diff agree.
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
 double canon = value == 0.0 ? 0.0 : value; // -0.0 → +0.0
 long bits = double.IsNaN(canon) ? unchecked((long)0x7FF8_0000_0000_0000) : BitConverter.DoubleToInt64Bits(canon);
 Span<byte> s = stackalloc byte[8]; BinaryPrimitives.WriteInt64LittleEndian(s, bits); buffer.Write(s);
 return this;
 }

 public CanonicalWriter Measure(MeasureValue measure) {
 MeasureValue q = measure.Quantize(tolerance);
 Dimension d = q.Dimension;
 // Identity is the SI magnitude + the 7-vector exponents, NOT the unit string — so OfSi (SI-native, symbol-derived)
 // and Of (UnitsNet-coerced, enum-named) project one physical measure to one byte sequence.
 return Double(q.Si).Ordinal(d.Length).Ordinal(d.Mass).Ordinal(d.Time).Ordinal(d.Current).Ordinal(d.Temperature).Ordinal(d.Amount).Ordinal(d.LuminousIntensity);
 }

 public CanonicalWriter Raw(ReadOnlySpan<byte> bytes) { buffer.Write(bytes); return this; }

 public ReadOnlyMemory<byte> ToBytes() => buffer.WrittenMemory;
}

// --- [TYPES] ------------------------------------------------------------------------------
[ValueObject<UInt128>]
public sealed partial class ContentAddress {
 // The kernel `ContentHash` seed-zero entry over the canonical bytes — the ONE hasher.
 public static ContentAddress Of(ReadOnlySpan<byte> canonicalBytes) =>
 Create(ContentHash.Of(canonicalBytes));

 // Wrap a PRECOMPUTED content hash (a RepresentationContentHash body key, a Coverage.RasterKey, an Assessment.InputKey)
 // as a ContentAddress without re-hashing — the carrier over an already-derived UInt128, distinct from the hashing entry above.
 public static ContentAddress Of(UInt128 contentHash) => Create(contentHash);

 public static ContentAddress Of(Node node, double tolerance) {
 CanonicalWriter w = new(tolerance);
 w.String(node.Id.Value).Raw(node.ToCanonicalBytes(tolerance).Span);
 return Of(w.ToBytes().Span);
 }

 // Order-INDEPENDENT graph address: node addresses sorted by UInt128, edges by canonical bytes.
 // Section counts make the layout self-delimiting — a node-vs-edge boundary can never blur under concatenation.
 public static ContentAddress OfGraph(ElementGraph graph) {
 CanonicalWriter w = new(graph.Header.Tolerance);
 w.Ordinal(graph.Nodes.Count);
 foreach (UInt128 nodeAddress in graph.Nodes.Values.Map(n => Of(n, graph.Header.Tolerance).Value).Order()) { w.U128(nodeAddress); }
 w.Ordinal(graph.Edges.Count);
 foreach (ReadOnlyMemory<byte> edge in graph.Edges.Map(EdgeBytes).OrderBy(static b => b, ByteSequence.Default)) { w.Raw(edge.Span); }
 return Of(w.ToBytes().Span);
 }

 // The edge's standalone canonical bytes — the SAME Relations/relation#EDGE_ALGEBRA Relationship.ToCanonicalBytes()
 // projection a content-3-way merge keys an edge on, so the graph address and the edge diff never diverge.
 static ReadOnlyMemory<byte> EdgeBytes(Relationship edge) => edge.ToCanonicalBytes();
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Lexicographic byte-sequence comparer for the order-independent edge canon.
sealed class ByteSequence : IComparer<ReadOnlyMemory<byte>> {
 public static readonly ByteSequence Default = new();
 public int Compare(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y) => x.Span.SequenceCompareTo(y.Span);
}
```

## [03]-[RESEARCH]

- [SHARED_CANONICAL_CODEC]: one canonical projection is the source of three identities — the non-rooted `NodeId.Content` mint, the `ContentAddress` diff/dedup key, and the `Generator.Equals` content comparison — so a node cannot identify one way and hash another; the `CanonicalWriter` fixes the float canon (`-0.0`→`0.0`, `NaN`→one quiet pattern, IEEE-754 little-endian), quantizes every measure to `Header.Tolerance` before writing, length-prefixes strings, and writes attributes in an explicit order, so the projection is byte-stable across the C#/Python/TypeScript runtimes that share the one `XxHash128` seed-zero rail; the float-bearing `IfcMaterialLayer`-shaped golden vector in the cross-runtime parity corpus is the regression anchor that the three peers agree byte-for-byte.
- [SINGLE_HASHER]: the `ContentAddress` composes the KERNEL seed-zero `XxHash128` content-hash entry — the ONE hasher the geometry `GeometryHash`, the snapshot spine, the `RepresentationContentHash`/`Coverage.RasterKey`/`Assessment.InputKey` content keys, and the Python/TypeScript wire peers all share — so the seam mints no second hasher and a non-zero seed or an alternate algorithm is the named defect; the graph address is order-independent (sorted node addresses + lexicographically-sorted edge bytes), so a snapshot's identity depends on its content, not its insertion order, and a re-serialized graph addresses identically — the property the `Rasm.Persistence` content-keyed object store and the three-way merge depend on.

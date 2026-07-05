# [ELEMENT_ADDRESS]

The content-identity owner: the `ContentAddress` `[ValueObject<UInt128>]` that hashes any seam value's canonical bytes through the kernel seed-zero `XxHash128` — the ONE hasher shared with the geometry `GeometryHash`, the snapshot spine, the `RepresentationContentHash`/`Coverage.RasterKey`/`Assessment.InputKey` content keys, and the cross-runtime Python/TypeScript peers; the seam mints no second hasher and seeds no non-zero — and the `CanonicalWriter` deterministic byte projection it composes: fixed IEEE-754 little-endian, `-0.0`→`0.0`, every `NaN`→one quiet pattern, ±∞ at canonical bits, measures quantized to `Header.Tolerance`, strings and collections count-prefixed, attributes in explicit order, a non-finite scalar NORMALIZED rather than railed (the canon the `Associate` edge's unset-`NaN` `ReferenceExtent` sentinel relies on, so "a non-finite forks the hash" is structurally impossible, not a fault path). Every seam `CanonicalBytes`/`ToCanonicalBytes` contribution writes through this ONE codec, so a value's identity, its content address, and the bytes the `Rasm.Persistence` content-3-way `StructuralMerge` keys an edge or node on are ONE encoding — `Generator.Equals` the ORTHOGONAL field-level `[Equatable]`/`Inequalities` member diff composed ALONGSIDE these content keys, never conflated with the codec. The graph address is order-INDEPENDENT — the SEMANTIC `Header` folded ahead, the `StepHeader`/`Instant` provenance excluded, node addresses and edge bytes sorted — so insertion order and re-export timestamps never fork identity while a schema or CRS change does; the mint's dual is `Verify`, the H7 tamper/corruption gate that re-derives a stored `NodeId` by the SAME identity regime that minted it and rails `Projection/fault#FAULT_BAND` `ElementFault.AddressUnstable` on any mismatch.

## [01]-[INDEX]

- [01]-[CONTENT_ADDRESS]: the `ContentAddress` `[ValueObject<UInt128>]` over the kernel seed-zero `XxHash128`, the raw-hash/precomputed-wrap/node/graph/verification entries, the id-inclusive node and order-independent graph addressing (semantic header folded, provenance excluded), and the `Verify` re-derive dual that re-mints by the minting regime and rails `ElementFault.AddressUnstable` on a mismatch.
- [02]-[CANONICAL_WRITER]: the `CanonicalWriter` ONE deterministic byte-projection codec (IEEE-754 LE, sign/`NaN`/∞ canon, tolerance-quantized measures, length-prefixed strings and count-prefixed collections, explicit attribute order) every seam value's `CanonicalBytes`/`ToCanonicalBytes` composes, so identity, content address, and 3-way-merge key project through one encoding.

## [02]-[CONTENT_ADDRESS]

- Owner: `ContentAddress` the `[ValueObject<UInt128>]` content key over the kernel seed-zero `XxHash128`; the raw-hash, precomputed-wrap, node, graph, and verification entries; the shared `ByteOrder` edge-bytes comparer the snapshot edge sort and the `Graph/delta#GRAPH_DELTA` `GraphDelta.ToCanonicalBytes` edge sort both compose.
- Entry: `ContentAddress.Of(ReadOnlySpan<byte>)` is the raw hashing entry the `Graph/element#NODE_MODEL` `NodeId.Content` mint shares; `Of(UInt128)` wraps a PRECOMPUTED content hash (a `RepresentationContentHash` body key, a `Coverage.RasterKey`, an `Assessment.InputKey`) without re-hashing; `Of(Node, tolerance)` is the id-INCLUSIVE node address (the graph-dedup key distinguishing two occurrences with identical content by their ids); `OfGraph(ElementGraph)` the order-INDEPENDENT snapshot address (semantic `Header` + sorted node addresses + sorted edge bytes); `Verify(Node, tolerance, key)` the single-node re-derive gate (`Fin<Unit>`) that re-mints by the node's identity regime (an Occurrence's random Guid vacuous, a Type's deterministic `NodeId.RootedType` over `ToTypeSeedBytes`, a non-rooted node's content self-hash) and `Verify(ElementGraph, key)` the snapshot sweep (`Validation<Error, Unit>`).
- Auto: `Of(Node)` writes the id then appends `node.ToCanonicalBytes(tolerance)` (the `Graph/element#NODE_MODEL` projection) so two occurrences with identical content stay distinct by id, while `OfGraph` folds the semantic `Header` through the `Graph/element#ELEMENT_GRAPH` `Header.CanonicalBytes` projection (the ONE header-bytes owner the `Graph/delta#GRAPH_DELTA` delta key also composes — schema/view/tolerance/georeference, never re-spelled here) then sorts the node `ContentAddress`es by `UInt128` and the edge canonical bytes lexicographically through `ByteOrder`, the section counts making the node-vs-edge layout self-delimiting; `Verify` re-runs the EXACT mint per regime — the Type arm re-mints `NodeId.RootedType(o.ToTypeSeedBytes(tolerance).Span)`, the non-rooted arm re-projects `node.ToCanonicalBytes` through this owner's `Of` entry and compares through `NodeId.OfContent`, the Occurrence arm passes vacuously (a random Guid-v7 has no content preimage) — the graph overload accumulating every mismatch applicatively.
- Receipt: a `ContentAddress` is the stable cross-runtime content key — a `NodeId.Content` for a non-rooted node, a node's dedup/diff key, a snapshot's identity the `Rasm.Persistence` spine and the `Rasm.Compute` assessment cache key on; the `Verify` `Fin`/`Validation` is the rehydrate integrity verdict a content-keyed store reads before trusting a persisted id.
- Packages: `Rasm` (the kernel `Domain.ContentHash` seed-zero entry — composed, never a second hasher — plus the `Op` op-key), Thinktecture.Runtime.Extensions (`[ValueObject<UInt128>]` + the generated `Create`/`Value`), LanguageExt.Core (`Fin`/`Validation`/`Error`/`Unit` + the `Seq.Traverse`/`.As()` applicative accumulation the snapshot `Verify` sweep folds every node check through).
- Growth: a new structural identity (a node, an edge, a snapshot, a verification) is one `Of`/`Verify` overload discriminating on input shape; a new precomputed content key is one `Of(UInt128)` caller; never a per-call-site hash, never a second hasher, and never a parallel content-key scheme — the `XxHash128` seed-zero entry is the one identity rail and the addressing grows by the `Projection/address#CANONICAL_WRITER` writer's vocabulary.
- Boundary: `ContentAddress` composes the KERNEL seed-zero `XxHash128` — a second hasher or a non-zero seed is the named defect, and `GetHashCode` is process-salted and NEVER persisted or wire-compared; the id-INCLUSIVE node address (graph dedup distinguishes nodes by id) and the id-EXCLUSIVE non-rooted `NodeId.Content` mint (the id derives from content) are two distinct projections sharing the one `CanonicalWriter`, never conflated; the graph address folds the SEMANTIC header and EXCLUDES the `StepHeader`/`Instant` provenance — the graph-altitude mirror of the node-level `OwnerHistory` exclusion; the single-node `Verify` is a `Fin` (fail-fast) and the graph sweep a `Validation` (independent node checks accumulate), the carrier selecting the algebra.

```csharp signature
// --- [RUNTIME_PRELUDE] --------------------------------------------------------------------
using System.Buffers;
using System.Buffers.Binary;
using System.Text;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Domain;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Element;

// --- [TYPES] ------------------------------------------------------------------------------
// KeyMemberName/KeyMemberAccessModifier are EXPLICIT — the established kernel TopoName/TopoSignature
// [ValueObject<UInt128>] form: the UInt128 Value is read publicly across the seam (this OfGraph node sort reads
// `Of(n, _).Value`, the Graph/element#NODE_MODEL `NodeId.OfContent(address)` formats `address.Value` X32), so the
// public-key spelling is pinned at declaration rather than left to a generated default the consumers cannot rely on.
[ValueObject<UInt128>(KeyMemberName = "Value", KeyMemberAccessModifier = AccessModifier.Public)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.All)]
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

 // The order-INDEPENDENT snapshot address: the semantic Header folded first through its OWN Graph/element#ELEMENT_GRAPH
 // Header.CanonicalBytes projection (the ONE header-bytes owner the Graph/delta#GRAPH_DELTA delta key also composes),
 // then node addresses sorted by UInt128, then edge canonical bytes sorted lexicographically, section counts making the
 // layout self-delimiting — identical content addresses identically regardless of insertion order, while a
 // schema/view/georeference change forks identity.
 public static ContentAddress OfGraph(ElementGraph graph) {
  CanonicalWriter w = new(graph.Header.Tolerance);
  graph.Header.CanonicalBytes(w);
  w.Ordinal(graph.Nodes.Count);
  // The default UInt128 ascending comparer is the canonical node order a cross-runtime peer reproduces byte-for-byte;
  // node and edge sorts are ONE ordering discipline.
  foreach (UInt128 nodeAddress in toSeq(graph.Nodes.Values).Map(n => Of(n, graph.Header.Tolerance).Value).OrderBy(static a => a)) { w.U128(nodeAddress); }
  // The SAME Relationship.ToCanonicalBytes(tolerance) projection a content-3-way merge keys an edge on — graph address
  // and edge merge-key never diverge. Threading Header.Tolerance matters ONLY for the Generic passthrough (its
  // PropertyValue.Measure attributes quantize through w.Measure; the five typed cases carry no Measure and are
  // tolerance-insensitive) — the tolerance-0 hardcode that silently forked a below-tolerance Generic edge is the
  // deleted form.
  w.Ordinal(graph.Edges.Length);
  foreach (ReadOnlyMemory<byte> edge in graph.Edges.Map(e => e.ToCanonicalBytes(graph.Header.Tolerance)).OrderBy(static b => b, ByteOrder)) { w.Raw(edge.Span); }
  return Of(w.ToBytes().Span);
 }

 // The H7 verification dual of the mint: re-derive a node's stored id from its current content by the SAME identity
 // regime that minted it — an OCCURRENCE Object (random Guid-v7 placement identity, no content preimage, H6) verifies
 // vacuously; a TYPE Object re-mints NodeId.RootedType over the Representations-EXCLUDED ToTypeSeedBytes seed (the
 // omitted volatile block keeps a later geometry attach from spuriously failing it; a forged or corrupted Type id is
 // caught); a NON-ROOTED node re-projects node.ToCanonicalBytes through THIS owner's Of and compares through
 // NodeId.OfContent. Each content arm composes the seam's own mint entries and compares NodeId to NodeId under the
 // declared [KeyMemberEqualityComparer] — the X32 spelling is owned by NodeId, never re-spelled here. The tolerance
 // MUST be the mint-time Header.Tolerance for the content-derived arms, or the quantized re-projection drifts. The
 // single producer on a mismatch is Projection/fault#FAULT_BAND ElementFault.AddressUnstable.
 // The WIRE face ([ObjectFactory<string>] — IObjectFactory<ContentAddress, string, ValidationError> +
 // IConvertible<string>, decompile-verified contracts): a raw UInt128 JSON number loses precision past 2^53 in a
 // JS JSON.parse, so EVERY serializer framework renders/parses the canonical X32 hex through this factory — the
 // TS/Python peers hold the key as the hex string and the ModelDiff wire crosses it losslessly; every consumer
 // picks this row through the generated converters with zero local edits.
 public static ValidationError? Validate(string? value, IFormatProvider? provider, out ContentAddress? item) {
  item = UInt128.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out UInt128 parsed)
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
   _ => NodeId.OfContent(Of(node.ToCanonicalBytes(tolerance).Span)) == node.Id
    ? Fin.Succ(unit)
    : ElementFault.AddressUnstable(key, $"<content-id-mismatch:{node.Id.Value}>")
  };

 // The snapshot rehydrate gate: every node re-verified through the single-node Verify, the Validation ACCUMULATING all
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

- Owner: `CanonicalWriter` the ONE deterministic byte-projection codec bound to the model tolerance — every seam value's `CanonicalBytes`/`ToCanonicalBytes` writes through it: the `Graph/element#NODE_MODEL` `Node.ToCanonicalBytes`, the `Relations/relation#EDGE_ALGEBRA` `Relationship.ToCanonicalBytes`, and the `Properties/property#PROPERTY_VALUE` `PropertyValue`, `Composition/material#MATERIAL_COMPOSITION` `MaterialComposition`, `Properties/quantity#MEASURE_VALUE` `MeasureValue`, `Geospatial/reference#GEO_REFERENCE` `GeoReference`, and `Geospatial/coverage#COVERAGE_NODE` `CoverageGrid` contributions — so identity, content address, and 3-way-merge key project through one encoding.
- Entry: `new CanonicalWriter(tolerance)` opens a projection bound to the model tolerance; the `Double`/`String`/`Ordinal`/`U128`/`Bool`/`Measure`/`Raw` primitives write canonically and each returns the writer for fluent chaining; `ToBytes()` reads the accumulated `WrittenMemory`; `Tolerance` exposes the bound grid a sibling `CanonicalBytes` fold reads.
- Auto: `Double` canonicalizes `-0.0`→`0.0` and every `NaN`→one quiet-NaN pattern then writes the IEEE-754 little-endian bits (±∞ keep their already-canonical bits), so a sign-of-zero or a payload-`NaN` never forks the hash and an unset-`NaN` sentinel canonicalizes stably; `Measure` quantizes the SI magnitude to the tolerance grid through `MeasureValue.Quantize` then writes the length-prefixed `QuantityType` discriminator token, the magnitude, and the seven `Dimension` exponents — NOT the display unit string — so a `Torque` and an `Energy` (or a `SectionModulus` and a `Volume`, or a dimension-anonymous `OfSi` and a named `Volume`) that share a `Dimension` stay distinct under the `NodeId.Content` mint, the content-dedup key, and the 3-way merge, while the SI-native `OfSi` and the UnitsNet-coerced `Of` still project one physical measure to one byte sequence and two measures within tolerance address identically; `String` length-prefixes the UTF-8 bytes so a delimiter collision cannot forge equality; `Ordinal`/`U128`/`Bool` write fixed-width little-endian (the count prefixes that make a collection layout self-delimiting); `ToBytes` reads the accumulated memory without a copy.
- Receipt: the writer's accumulated bytes ARE the one canonical projection — the non-rooted `Graph/element#NODE_MODEL` `NodeId.Content` mint, the `Projection/address#CONTENT_ADDRESS` `ContentAddress` node/graph address, and the `Rasm.Persistence` `StructuralMerge` edge/node key all read this single encoding, so a value cannot identify one way and hash another; the float-bearing `IfcMaterialLayer`-shaped golden vector is the cross-runtime regression anchor the C#/Python/TypeScript peers reproduce byte-for-byte, and the parity corpus pins the COUNTED layout — a `PropertySet`/`QuantitySet`-bearing content key derives from the count-prefixed bag layout, the recorded wire law the queued `PY_WIRE_ALIGNMENT` mirrors build against, the golden `LayerSet` recipe carrying its own count and unshifted.
- Packages: System.Buffers (`ArrayBufferWriter` the accumulation buffer), System.Buffers.Binary (`BinaryPrimitives` the fixed-width little-endian writes), System.Text (`Encoding.UTF8` the length-prefixed string bytes), `Properties/quantity#MEASURE_VALUE` (`MeasureValue.Quantize`/`Dimension`/`QuantityType` the `Measure` primitive reads).
- Growth: a new primitive encoding is one method on `CanonicalWriter`; a new seam value type contributes one `CanonicalBytes(CanonicalWriter)` method co-located with its owner composing the existing primitives, never a parallel codec and never a per-type ad-hoc serialization.
- Boundary: `CanonicalWriter` is the ONE canonical codec — a per-call-site or per-type ad-hoc serialization, a `double.GetHashCode`, or a culture-formatted string is the deleted form; the COUNT-PREFIX LAW is named here: every collection run in every `CanonicalBytes`/`ToCanonicalBytes` contribution writes `Ordinal(count)` BEFORE its rows — the injectivity precondition of every raw-append join (`ContentAddress.Of(Node)`'s id+bytes concat, `OfGraph`'s edge run, the `Graph/delta#GRAPH_DELTA` node/edge sections) — and an uncounted trailing run that lets a following segment's bytes parse as extra rows is the named defect, the deleted form (the `Graph/element#NODE_MODEL` bag arms the highest-traffic sites the law binds); the writer NORMALIZES a non-finite scalar (`-0.0`→`0.0`, `NaN`→one quiet pattern, ±∞ at canonical bits) rather than railing — the determinism guarantee AND the canonicalization the `Associate` edge's unset-`NaN` `ReferenceExtent` (`Relations/relation#EDGE_ALGEBRA`) relies on, so a non-finite never forks the hash and the MINT path mints no `Projection/fault#FAULT_BAND` `ElementFault` (a non-finite is rejected at its value-admission gate, never here); it is a reference type (NOT a ref struct) because the sibling `CanonicalBytes` folds capture it in the union `Switch` lambdas (`Graph/element#NODE_MODEL`, `Relations/relation#EDGE_ALGEBRA`) and a ref struct cannot be captured by a closure.

```csharp signature
// --- [SERVICES] ---------------------------------------------------------------------------
// The ONE canonical value codec every seam value writes through, so identity, address, and 3-way-merge key agree.
// A reference type (NOT a ref struct): the sibling CanonicalBytes folds capture it in union Switch lambdas.
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

- [SHARED_CANONICAL_CODEC]: the id-EXCLUSIVE non-rooted `NodeId.Content` mint, the id-INCLUSIVE `ContentAddress` node address, the order-independent graph address, and the `Node`/`Relationship.ToCanonicalBytes` merge keys are distinct PROJECTIONS of one codec — a value cannot identify one way and hash another — and the `CanonicalWriter` float canon, tolerance quantization, and length/count prefixing make the projection byte-stable across the C#/Python/TypeScript runtimes, the float-bearing `IfcMaterialLayer`-shaped golden vector the regression anchor pinning the COUNTED bag layout (every `PropertySet`/`QuantitySet` content key derives from the count-prefixed rows — the wire law the queued canonical-writer mirrors reproduce, the golden `LayerSet` recipe always counted and its bytes unshifted). `Generator.Equals` (field-level `[Equatable]`/`Inequalities`, NOT a byte projection) is the ORTHOGONAL structural member diff the merge composes ALONGSIDE these content keys to localize a change to a member path.
- [SINGLE_HASHER]: a second hasher, a non-zero seed, or an alternate algorithm anywhere on the seam is the named defect — every content key composes the one kernel entry. The graph address folds the SEMANTIC `Header` (schema, model view, tolerance, the full `GeoReference` map-conversion tuple) while EXCLUDING the `StepHeader`/`Instant` provenance, so a snapshot's identity depends on its content and schema/CRS — not its insertion order or re-export timestamp — the property the `Rasm.Persistence` content-keyed object store and the 3-way merge depend on.
- [VERIFICATION_DUAL]: the regime split is load-bearing — the Type arm is the integrity check `NodeId.RootedType`'s determinism admits (an id no longer matching its own `Representations`-excluded type-defining content), so a single vacuous arm over ALL `Node.Object` (treating a deterministic Type id like an uncheckable Occurrence Guid) is the deleted under-check; only the Occurrence's random Guid-v7 (no content preimage, H6) verifies vacuously. The ONLY rail is `Projection/fault#FAULT_BAND` `ElementFault.AddressUnstable`, the verification mismatch its sole producer — the prior "a non-finite measure rails `AddressUnstable`" reading is the deleted form, because the writer NORMALIZES non-finite and the upstream `MeasureValue.Of`/`PropertyValue.Of` admissions reject a non-finite at ingress, so a drifted value surfaces as a re-derive mismatch, never an un-producible byte-projection fault.

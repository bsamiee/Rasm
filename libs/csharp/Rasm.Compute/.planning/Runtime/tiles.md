# [COMPUTE_TILES]

Rasm.Compute owns the companion hop that turns a foreign design source into geometry and the streamable octree that partitions that geometry beneath its semantic layer. `Rasm.Bim` owns the IFC/glTF/STEP semantic object model and its import-export surface, reached at the companion seam; this lane is HOST-LOCAL and carries no TS_PROJECTION.

`CompanionRequest` owns every companion-rpc job as one closed family, `CompanionPolicy` the quality-and-partition policy both halves fold into the content key, and the `TileSet`/`TileNode` octree with its `TileMetadata`/`PropertyTable`/`MetadataProperty`/`FeatureBand` family owns the 3D-Tiles partition and its tileset.json manifest. The lane composes the seam `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry`, the kernel `EncodedGeometry` arena, `Runtime/codecs#CONTENT_ADDRESSING` for every key and address, and the `Tensor/memory#STREAM_POOL` capsule for the manifest emit.

## [01]-[INDEX]

- [02]-[TWO_HOP_TESSELLATION]: IFC/AP242/native geometry crosses to the companion, never in-proc; the ifctester IDS-audit oracle and the GeoArrow-buffer consume ride the same companion rpc as one request family.
- [03]-[TILE_PARTITION]: 3D-Tiles octree partition; streamable LOD over the content-keyed geometry; the manifest this page owns and the leaf bodies it never emits.

## [02]-[TWO_HOP_TESSELLATION]

- Owner: `CompanionRequest` — the ONE companion-rpc job family, its three cases sharing one identity regime (a policy-seeded `UInt128` content key rendered as one object-plane address), one admission path (`Plan` gating once onto `Fin`), one payload timing, and ONE consumer (the companion rpc at `Runtime/channels#TRANSPORT_AXIS`); the family is named APART from the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` `TessellationRequest` it composes under, one seam carrying two shapes so neither page re-mints the other's key; the seam `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry` the decoded mesh-pool carrier the re-import lands and the tile partition reads (ONE shape at the seam — the Bim rail produces it, this lane consumes it, no Compute-local twin); `CompanionPolicy` the deflection/tolerance/tile-partition policy folded into the content key.
- Cases: `Tessellate` the two-hop bridge crossing IFC geometry evaluation to the IfcOpenShell companion (`python:geometry/mesh/daemon` `TessellationDaemon.tessellate`, evaluating in-process and serializing GLB to a file sink) and re-importing the GLB through the Bim glTF path; `GeoArrow` the geospatial leg carrying `python:data/spatial/geospatial`'s `EgressFormat.GEOARROW` Arrow IPC bytes to the companion for native geometry conversion before the existing GLB return; `IdsAudit` the companion-rpc leg passing IDS-XML to the Python ifctester oracle and projecting the per-specification pass/fail `GlobalId` set into the Bim `IdsAudit` shape (one invocation beside `IfcConvert`).
- Entry: `public static Fin<CompanionRequest> Plan(CompanionSource source, CompanionPolicy policy)` is the ONE gate — a total `Switch` over the source shape emitting the case whose payload it admits, keyed on the source content and the policy vector; the round-trip rides the `Runtime/wire#PROTO_VOCABULARY` `Tessellate` rpc, whose `TessellationRequest`/`TessellationReceipt` pair this job transcribes onto while the GLB body returns over the ArtifactSyncService frame seam, and the GLB re-enters the Bim glTF import rail as an `ImportedGeometry`.
- Auto: `Plan` gates the hop on the source format's own companion-tessellation flag so a non-IFC format never crosses; the request carries the source bytes, the evaluation policy, and the content key so a re-cross at the same policy is gated. Durable GLB residence is keyed by the Bim `Exchange/tessellation#TESSELLATION_BRIDGE` dual `SourceKey`/`ContentKey` (kernel seed-zero content-hash, never a policy seed) with the Persistence object-store `ContentAddress`, the Bim bridge performing that durable reuse before crossing; this leg's policy-seeded key is the companion-rpc cache-partition over source bytes and evaluation policy that gates re-crossing — distinct cache layers, neither re-minting the other's key. The object-plane address every case answers is the ONE `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Address` grammar, so the three hand interpolations this collapse deletes cannot drift apart.
- Receipt: the `RemoteCall` receipt carries the companion transport, the source content-key, the deflection, and the elapsed; a cache hit on the prior GLB stamps a `Cache` receipt instead of crossing.
- Packages: LanguageExt.Core, NodaTime, System.IO.Hashing, Thinktecture.Runtime.Extensions, Rasm.Element (project — the seam `ImportedGeometry` carrier), Rasm.Persistence (project), BCL inbox
- Growth: a new tessellation companion is one transport-row consumption (never a new transport); a new evaluation parameter is one column on `CompanionPolicy` folded into the content key; a new companion leg is one `CompanionRequest` case whose arm the `Plan` `Switch` demands at compile time and whose address the root fold already answers — the three-record form this collapse deletes cost a record, a `Plan`, and an address derivation per leg; zero new surface.
- Boundary: the two-hop rail is the single IFC-to-geometry path — the Bim IFC object model carries no tessellation kernel, so a managed IFC BRep evaluator is the deleted form; the companion is the IfcOpenShell PyPI package in `libs/python/geometry`, never a NuGet pin, reached only over the existing `Runtime/channels#TRANSPORT_AXIS` UDS/InProcess companion rpc, so this page mints no transport, channel, or second wire vocabulary; a returned GLB re-enters the Bim glTF import rail as one `ImportedGeometry`, and the Bim IFC semantic graph and this hop's tessellated geometry are two projections of one content-keyed IFC artifact joined by the content key; C# never invents a coordinate/offset ABI or hand-triangulates GeoArrow rings, and the opaque Arrow IPC relay bytes are never decoded or re-encoded; the `IdsAudit` case adds one ifctester invocation beside `IfcConvert` over the same companion rpc, passing IDS-XML with IFC content to the `python:geometry/ifc-companion` ifctester (`ids` oracle) and relaying the per-specification verdict wire back, which the Bim-owned `Review/validation#IDS_FACETS` `IdsAudit.Reconcile` composes into `IdsVerdict` rows and joins the C# self-audit against on the (GlobalId, `FacetKey`) axis — `FacetKey` the Bim composite join token unique within a specification (facet-type prefix and value discriminator), never the bare facet-type word — Compute referencing no Bim type and owning only the rpc orchestration and verdict relay, a Compute-minted IDS parser or a second transport the rejected form; the domain family keeps its own case names while the WIRE messages stay `<Verb>Request`/`<Verb>Receipt`, the branch spelling ruling governing the descriptor alone.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
// Compute-lane geometry-quality + tile-partition policy: every output-affecting quality and partition column
// salts the owning compute content key. Renamed off `TessellationPolicy` — the kernel `Rasm/Processing/repair`
// mesher posture owns that bare name and both resolved in this compilation; THIS row is the companion hop's
// OUTPUT-KEY policy (the `CompanionRequest`/`CompanionSource` family's third member), whose columns salt an
// interchange cache key rather than steer a triangulator.
public sealed record CompanionPolicy(
    double Deflection,
    double Tolerance,
    double AngleTolerance,
    int TileMaxDepth,
    double TileGeometricErrorRoot,
    double TileSplitThreshold,
    int CommitWatermark) {
    public static readonly CompanionPolicy Canonical = new(
        Deflection: 0.01, Tolerance: 1e-6, AngleTolerance: 1e-4,
        TileMaxDepth: 16, TileGeometricErrorRoot: 512.0, TileSplitThreshold: 8192.0,
        // Writer commit bound: an octree at the policy depth reaches six figures of nodes, so the emit commits
        // each completed node once the writer's own pending count crosses this width, and the bound moves with
        // the depth that forces it rather than standing as a `const` beside it.
        CommitWatermark: 64 * 1024);

    // The ordered scalar vector the content key folds — one owner order, so a new column salts every key that
    // reads it and no call site re-spells the roster.
    public double[] Vector => [Deflection, Tolerance, AngleTolerance, TileMaxDepth, TileGeometricErrorRoot, TileSplitThreshold];
}

// The source shape a companion leg admits, so `Plan` discriminates on a VALUE rather than on which of three
// entrypoints a caller happened to reach for. `Ifc` carries the format key because the format axis owns the
// companion-tessellation flag the gate reads.
[Union]
public abstract partial record CompanionSource {
    private CompanionSource() { }

    public sealed record Ifc(string FormatKey, bool CompanionTessellates, ReadOnlyMemory<byte> Bytes) : CompanionSource;
    public sealed record GeoArrowIpc(ReadOnlyMemory<byte> Bytes) : CompanionSource;
    public sealed record IdsSpec(ReadOnlyMemory<byte> IfcBytes, ReadOnlyMemory<byte> IdsXml) : CompanionSource;
}

// --- [MODELS] -----------------------------------------------------------------------------
// ONE companion-rpc job family. The three records this replaces shared an identity regime, an admission path, a
// payload timing, and a consumer, and named no discriminant anywhere — so each carried its own `Plan`, its own
// key fold, and its own `{key:x32}:{ext}` interpolation, three copies of one address grammar. Here the key and
// the address live on the ROOT and the cases carry only what actually differs: which payload crosses.
//
// ifctester cross-tool IDS-audit oracle over the settled TWO_HOP companion rpc: Compute orchestrates the
// companion invocation exactly as the tessellation hop, relaying the per-specification verdict wire back as the
// Bim-owned Review/validation#IDS_FACETS IdsVerdict(GlobalId, Specification, Spec, Requirement, Facet, Failure)
// row — Failure is the outcome, its presence carrying the oracle's reason — composed by IdsAudit.Reconcile in Bim
// (the IDS authority), never re-declared here; Compute references no Bim type. Spec is the specification's
// zero-based document ordinal and Requirement the facet's ordinal within its spec, both from the shared document
// order (never the spec NAME, which IDS v1.0 leaves non-unique); Reconcile joins the self-audit against the
// oracle on the ordinal-qualified (GlobalId, Requirement, FacetKey) axis, oracle rows filtered by the Spec ordinal.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CompanionRequest {
    private CompanionRequest(UInt128 contentKey, ReadOnlyMemory<byte> payload, string resultFormatKey) =>
        (ContentKey, Payload, ResultFormatKey) = (contentKey, payload, resultFormatKey);

    // Companion-rpc cache-partition over source bytes + evaluation policy — NOT the durable GLB store address
    // (the Bim TESSELLATION_BRIDGE ContentKey, kernel seed-zero, that the Persistence object store keys).
    // ONE seam, TWO shapes, TWO names: the Bim bridge owns the TessellationRequest (the AEC-domain crossing under
    // its cache-before-cross/store-before-return policy), this lane owns the CompanionRequest (the rpc job
    // planned from source bytes and evaluation policy).
    public UInt128 ContentKey { get; }
    public ReadOnlyMemory<byte> Payload { get; }
    public string ResultFormatKey { get; }

    // ONE address derivation for the whole family, composing the page-owned grammar owner. The three sites this
    // replaces each spelled `$"{key:x32}:{ext}"` by hand.
    public string ArtifactKey => InterchangeIdentity.Address(ContentKey, ResultFormatKey);

    public sealed record Tessellate(UInt128 Key, ReadOnlyMemory<byte> IfcBytes, CompanionPolicy Policy)
        : CompanionRequest(Key, IfcBytes, "glb");

    public sealed record GeoArrow(UInt128 Key, ReadOnlyMemory<byte> ArrowIpc)
        : CompanionRequest(Key, ArrowIpc, "glb");

    public sealed record IdsAudit(UInt128 Key, ReadOnlyMemory<byte> IfcBytes, ReadOnlyMemory<byte> IdsXml)
        : CompanionRequest(Key, IfcBytes, "ids-verdict");

    // ONE total gate. Each arm states only its own refusal and its own key domain; the policy vector and the
    // content-key fold are shared, so a policy column added above salts all three by construction.
    public static Fin<CompanionRequest> Plan(CompanionSource source, CompanionPolicy policy) =>
        source.Switch<Fin<CompanionRequest>>(
            state: policy,
            ifc: static (p, row) => row.CompanionTessellates
                ? Fin.Succ<CompanionRequest>(new Tessellate(
                    InterchangeIdentity.Key(row.FormatKey, row.Bytes.Span, p.Vector), row.Bytes, p))
                : Fin.Fail<CompanionRequest>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input))),
            geoArrowIpc: static (p, row) => row.Bytes.IsEmpty
                ? Fin.Fail<CompanionRequest>(new ComputeFault.PayloadOverBounds("<geoarrow-ipc-empty>"))
                : Fin.Succ<CompanionRequest>(new GeoArrow(
                    InterchangeIdentity.Key("geoarrow", row.Bytes.Span, []), row.Bytes)),
            idsSpec: static (p, row) => row.IdsXml.IsEmpty
                ? Fin.Fail<CompanionRequest>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Input)))
                : Fin.Succ<CompanionRequest>(new IdsAudit(
                    InterchangeIdentity.Key("ids", row.IfcBytes.Span, p.Vector), row.IfcBytes, row.IdsXml)));
}
```

## [03]-[TILE_PARTITION]

- Owner: `TileSet` the 3D-Tiles octree partition over the seam `ImportedGeometry` — one kernel `EncodedGeometry` arena read by descriptor, baked once at `Build`, leaves gathered lane-generically and re-minted as single-block identity pools; `TileNode` the per-node bounding-volume/geometric-error/content-key record carrying its `Option<TileMetadata>` semantic layer and the ONE `Fold` algebra every census, flatten, and emit walks; `MetadataProperty` `[Union]` the `EXT_structural_metadata` typed property-column cases; `PropertyTable` the per-tile feature-keyed property-table carrier; `TileMetadata` the per-leaf content-keyed metadata property table joining the IFC classification column and the solver field-value columns under one feature-id mapping, carrying its own `ReplayKey` so a tile is independently addressable and cache-replayable; `FeatureBand` `[SmartEnum<string>]` the solved-field styling-band rows; `TileNodeWire` the generated per-node tileset.json projection; `LeafContent`/`TilesetCensus`/`TilesetExport` the manifest-plus-leaf-reference export carriers; `TilePartition` the tileset-manifest emit fold.
- Entry: `public static Fin<TilesetExport> ExportTiles(StreamPool pool, CorrelationId correlation, ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, CompanionPolicy policy, IClock clock, Op? key = null)` admits a census-consistent geometry, builds the octree, attaches the per-leaf metadata read at the node content key, serializes the real tileset.json manifest, and enumerates the leaf-content references the manifest names — the leaf BODIES are the Bim glTF codec's product resolved at the content-key URIs, never emitted here; `public static Fin<TileSet> Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, CompanionPolicy policy, IClock clock, Op? key = null)` bakes once and partitions the geometry into the depth-bounded octree; `Fin<T>` aborts on a census disagreement (`PayloadOverBounds` — a vertex-less mesh otherwise emits a manifest of float sentinel bounds), on a bake or per-leaf arena re-mint the kernel refuses, while a throwing serializer remains the original exceptional Error.
- Auto: `Build` bakes the pool through the seam's ONE `Bake(Op)` flatten, then partitions octant-by-octant to the policy max depth or triangle split threshold, geometric error the root error halved per depth, per-node content key via the channel-generic `InterchangeIdentity.Key` over the arena so a re-partition of identical geometry keys identically AND a lane-roster growth re-keys, then reads the per-leaf `TileMetadata` at that content key so one key addresses geometry and metadata. Every geometric read resolves BY DESCRIPTOR through the one `Lane` unpack — bounds and octant assignment stride on `EncodingChannel.Position.Arity`, and the per-leaf `Tessellate` gathers each declared lane by its own arity and re-mints the whole set through the kernel `Encode.Of` raw-lane entry, so a sliced leaf carries every lane its parent declared (UV and colour included) and a new channel row costs this partition nothing. `ExportTiles` serializes the tileset.json manifest through the composition's one pooled stream — each node's OWN members generated by the `[Mapper]` projection and committed past the writer's `BytesPending` watermark so the writer stays one commit wide — keys it off the pooled `ReadOnlySequence<byte>` through the incremental identity so a resident manifest materializes nothing, and folds the octree ONCE for the leaf references, the node census, and the depth the receipt reads. `TileMetadata.Join` folds the `Rasm.Bim` IFC classification and the `Solver/field#DISCRETE_FIELD` `FieldSpace` per-element field values read at the shared content key into one feature-keyed property table under its own tile content key; `TileMetadata.ReplayKey` composes that key with the causal stamp through `InterchangeIdentity.Compose` in (physical, logical) order, and `LeafContent` CARRIES that replay key so the cache-replay law the Boundary legislates has the consumer it names; `FeatureBand.Of` classifies an achieved field value onto its styling band.
- Receipt: the `StreamSegment` receipt carries the manifest artifact id and the emitted bytes, and `TilesetExport.Census` is the value the leaf-reference count, root geometric error, max depth, node count, and per-leaf property-column count all read off — one fold, one carrier, never five columns a projection re-derives; emission rides the sink port.
- Packages: System.IO.Hashing, CommunityToolkit.HighPerformance, SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Ext.3DTiles (the `Schema2.Tiles3D` EXT_structural_metadata/EXT_mesh_features leaf-body schema surface, admitted via `Tiles3DExtensions.RegisterExtensions()` once at composition — the settled Compute admission; models no tileset.json manifest tree), meshoptimizer, Microsoft.IO.RecyclableMemoryStream (the `RecyclableMemoryStream` the `Tensor/memory#STREAM_POOL` capsule grants — never a manager constructed here), Riok.Mapperly (the per-node tileset.json member projection), Generator.Equals, LanguageExt.Core, NodaTime, Rasm (project — the kernel `EncodedGeometry` arena with `Encode.Of`, `Channel`, and `Descriptors`, the `EncodingChannel` lane roster, `ChannelDtype.Unpack`, `CorrelationId`, and `Op` the admission channel), Rasm.Element (project — the seam `ImportedGeometry`/`MeshBlock`/`MeshInstance` carrier), Rasm.Persistence (project), BCL inbox (`System.Text.Json` `Utf8JsonWriter` over the pooled stream with its `BytesPending` commit read, `System.Buffers` `ReadOnlySequence<byte>`)
- Growth: a new tile-partition parameter is one column on `CompanionPolicy` folded into the partition; a new per-vertex attribute is one kernel `EncodingChannel` row the partition reads, slices, and re-mints with ZERO edit here, because every geometric body addresses lanes by descriptor and none names a channel; a new metadata property is one `MetadataProperty` case folded into the property table; a new styling band is one `FeatureBand` row; a new manifest member is one column on `TileNodeWire` the generated mapper fills; a new leaf-tile content format is one row on the Bim format axis the leaf emit reads; zero new surface — a `TileMetadataStore`/`FeatureAttributeTable` sibling owner is collapsed onto the one `TileMetadata`/`PropertyTable` family on the leaf-tile content emit.
- Boundary: the leaf-content URI grammar and content-key crossing are the `tests/contracts/MANIFEST.md` `[02.6]-[GLB_BY_KEY]` binding this page produces for; the 3D-Tiles partition is the streamable-LOD octree over content-keyed geometry the compute lane owns — riding `InterchangeIdentity.Key` and the imported-geometry carrier — while the b3dm/glTF tile content encode is the Bim glTF codec the leaf emit composes; every geometry read here is CHANNEL-GENERIC over the seam carrier's one kernel arena — the descriptor set decides which lanes exist and at what storage width, and `Lane` is the single unpack; the metadata layer is one content-keyed schema column on the leaf-tile emit, never a parallel attribute store or second tiling owner, each `TileMetadata` carrying its own tile content key (independently addressable) and `ReplayKey` composing it with the causal stamp so a leaf tile is cache-replayable without rebuilding the octree; the IFC classification reads the `Rasm.Bim` IFC semantic graph at the shared content key (companion seam, never reaching into the Bim interior) and the per-element field values read the `Solver/field#DISCRETE_FIELD` `FieldSpace` achieved value (never a recomputed metric), so the IFC graph and the tessellated geometry stay two projections of one content-keyed IFC artifact joined at the tile boundary, a re-tessellation at a new deflection re-keying both together; the property-table BUFFER LAYOUT is NOT this page's — `api-sharpgltf-3dtiles#IMPLEMENTATION_LAW` scopes this lane's whole reach into that surface to `Tiles3DExtensions.RegisterExtensions()`, and `UseStructuralMetadata`/`AddMeshFeatureIds`/`PropertyTableProperty.SetValues<T>` own the buffer-view derivation at the Bim authoring fence, so the columns here are the declared SCHEMA a leaf emit lowers and a Compute-side packing of them is out of contract at both ends; a hand-authored `JsonSerializable` extension class over the raw registration is the form `Rasm.Bim` `Exchange/export` already deletes, and Core's name-only `RegisterExtension<TParent,TExt>(string)` overload is `[Obsolete]` in favour of the factory-taking one (the material-PBR surface being the separate string-keyed `MaterialChannel` API in Core and `KnownChannel` enum in Toolkit); meshoptimizer owns the leaf-tile `Meshopt.Simplify`/`OptimizeVertexCache` LOD, never a hand-rolled simplifier; the manifest emit rides the `Tensor/memory#STREAM_POOL` capsule the composition already owns — a growable `ArrayBufferWriter` reaches a policy-depth manifest by doubling through the large-object heap, `GetBuffer` is the contiguity cliff, and `ToArray` the migration copy the pool's own posture bans, so all three are the deleted forms and a manager constructed at this boundary is the second-pool defect that owner forecloses; the leaf-tile content body is NOT emitted here — `ExportTiles` yields one typed `LeafContent` per leaf (content key, replay key, `{contentKey:x32}.glb` URI, metadata-column count), the octree, metadata schema, and quantization-bit policy owned here while the b3dm/glTF body each URI names is the Bim tile-emit cross-package product against the Persistence index, a public leaf-body entry that can only decline the rejected honesty defect and a partition that re-derives the glTF body in-place or a metadata layer that re-reads the IFC parser the rejected form.

```csharp signature
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class FeatureBand {
    public static readonly FeatureBand Nominal = new("nominal", upperFraction: 0.5);
    public static readonly FeatureBand Elevated = new("elevated", upperFraction: 0.8);
    public static readonly FeatureBand Critical = new("critical", upperFraction: 1.0);

    public double UpperFraction { get; }

    public static FeatureBand Of(double value, double minimum, double maximum) {
        double span = Math.Max(1e-12, maximum - minimum);
        double fraction = Math.Clamp((value - minimum) / span, 0.0, 1.0);
        return Items.Find(row => fraction <= row.UpperFraction).IfNone(Critical);
    }
}

// The DECLARED schema columns a leaf emit lowers onto `PropertyTableProperty.SetValues<T>` at the Bim authoring
// fence. NAMED LOSS on the retired `ColumnBytes`/`ComponentType`/`Count` triple and `PropertyTable.Pack`: this
// page no longer derives a buffer-view layout, and with it goes the NUL-joined STRING blob that was a second,
// incompatible encoding of what `EXT_structural_metadata` spells with an explicit offset buffer. Nothing read
// the derived views corpus-wide, and the catalog scopes this lane's reach to the extension registration alone.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record MetadataProperty {
    private MetadataProperty() { }

    [Equatable]
    public sealed partial record Classification(string Name, [property: OrderedEquality] Seq<string> Values) : MetadataProperty;

    [Equatable]
    public sealed partial record Scalar(string Name, string Unit, [property: SequenceEquality] ReadOnlyMemory<float> Values) : MetadataProperty;

    [Equatable]
    public sealed partial record Banded(string Name, [property: SequenceEquality] ReadOnlyMemory<float> Values, [property: OrderedEquality] Seq<string> Bands) : MetadataProperty;

    public string PropertyName =>
        Switch(classification: static c => c.Name, scalar: static s => s.Name, banded: static b => b.Name);
}

// --- [MODELS] -----------------------------------------------------------------------------
public sealed record PropertyTable(string Class, int FeatureCount, Seq<MetadataProperty> Columns);

// `FeatureIds` is a `ReadOnlyMemory<int>`, which record equality compares BY REFERENCE — two byte-identical
// metadata tables would read unequal and re-key a cache that should have hit. The generated comparer reads the
// sequence, which is the value the content key already fixes.
[Equatable]
public sealed partial record TileMetadata(
    UInt128 ContentKey,
    PropertyTable Table,
    [property: SequenceEquality] ReadOnlyMemory<int> FeatureIds) {
    public static TileMetadata Join(UInt128 contentKey, string ifcClass, Seq<string> classification, ReadOnlyMemory<float> fieldValues, string fieldUnit, double minimum, double maximum, ReadOnlyMemory<int> featureIds) {
        Seq<string> bands = toSeq(fieldValues.ToArray().Select(value => FeatureBand.Of(value, minimum, maximum).Key));
        Seq<MetadataProperty> columns = Seq<MetadataProperty>(
            new MetadataProperty.Classification("ifc-class", classification),
            new MetadataProperty.Scalar("field-value", fieldUnit, fieldValues),
            new MetadataProperty.Banded("field-band", fieldValues, bands));
        return new TileMetadata(contentKey, new PropertyTable(ifcClass, classification.Count, columns), featureIds);
    }

    public UInt128 ReplayKey(Instant physical, ulong logical) => InterchangeIdentity.Compose(ContentKey, physical, logical);
}

// The octree node and its ONE fold algebra. `Count`, `Leaves`, the manifest walk, and `Flatten` were four hand
// recursions over this shape; each is now a fold, so a node column added below reaches every walk at once.
// CARVE (kernel-sanctioned): `BoundingVolume` stays a `float[12]` — it is the 3D-Tiles `box` array VERBATIM,
// twelve numbers in the spec's own centre-plus-three-half-axis order, and an `OrientedBox` owner over it was
// REFUTED at the kernel because the value has no life outside this manifest slot. STRATA_TWIN, named here: the
// `Rasm.Bim` `Exchange/export` `TileNode` is the IMPLICIT-TILING authoring coordinate (`Lod`/`X`/`Y`/`Z`
// availability over a Morton subtree bitstream); this one is the EXPLICIT octree node carrying real geometry.
// The two meet at the shared tile index and never at a shared type.
public sealed record TileNode(int Depth, float[] BoundingVolume, double GeometricError, UInt128 ContentKey, Option<TileMetadata> Metadata, Seq<TileNode> Children) {
    // Depth-first pre-order fold: the manifest emit's own order, so every census the receipt reads and the
    // serialization the writer walks agree by construction rather than by inspection.
    public TState Fold<TState>(TState seed, Func<TState, TileNode, TState> step) =>
        Children.Fold(step(seed, this), (state, child) => child.Fold(state, step));

    public bool IsLeaf => Children.IsEmpty;
}

// The census the receipt reads, folded ONCE. Five columns spread across `TileSet` and `LeafContent` had zero
// readers each because each demanded its own walk; one fold answers all of them.
public readonly record struct TilesetCensus(int Nodes, int Leaves, int MaxDepth, double GeometricErrorRoot, int MetadataColumns) {
    public static TilesetCensus Of(TileNode root) =>
        root.Fold(new TilesetCensus(0, 0, 0, root.GeometricError, 0), static (census, node) => new TilesetCensus(
            census.Nodes + 1,
            census.Leaves + (node.IsLeaf ? 1 : 0),
            Math.Max(census.MaxDepth, node.Depth),
            census.GeometricErrorRoot,
            census.MetadataColumns + node.Metadata.Map(static meta => meta.Table.Columns.Count).IfNone(0)));
}

public sealed record TileSet(TileNode Root, TilesetCensus Census, Instant At) {
    // Bake FIRST: the octant walk addresses world-space triangles, so an instanced carrier flattens through the
    // one seam Bake owner and a non-instanced carrier passes through unchanged (its pool IS its scene). Bake
    // re-mints an arena, so it rails — the whole partition rides that rail rather than swallowing a mint refusal.
    public static Fin<TileSet> Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, CompanionPolicy policy, IClock clock, Op? key = null) {
        Op k = key.OrDefault();
        return geometry.Bake(k)
            .Bind(baked => Partition(baked, metadata, policy, depth: 0, k))
            .Map(root => new TileSet(root, TilesetCensus.Of(root), clock.GetCurrentInstant()));
    }

    static Fin<TileNode> Partition(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, CompanionPolicy policy, int depth, Op key) {
        float[] positions = Lane(geometry.Lanes, EncodingChannel.Position);
        float[] bounds = Bounds(positions);
        double error = policy.TileGeometricErrorRoot / Math.Pow(2, depth);
        UInt128 contentKey = InterchangeIdentity.Key(
            geometry.FormatKey, geometry.Lanes, MemoryMarshal.AsBytes(geometry.Indices.AsSpan()), policy.Vector);
        return depth >= policy.TileMaxDepth || geometry.TriangleCount <= policy.TileSplitThreshold
            ? Fin.Succ(new TileNode(depth, bounds, error, contentKey, metadata(contentKey), Seq<TileNode>()))
            : Split(geometry, positions, bounds, key)
                .Bind(leaves => leaves.TraverseM(leaf => Partition(leaf, metadata, policy, depth + 1, key)).As())
                .Map(children => new TileNode(depth, bounds, error, contentKey, None, children));
    }

    // ONE descriptor-addressed lane reader serves the whole partition: the descriptor names the dtype, so Unpack
    // lifts a unorm8 colour and a float32 position through the same call and no arm re-spells a storage width.
    // Absent channels answer the empty array — a MISSING DESCRIPTOR, never a zero-filled buffer a consumer
    // length-probes — which is what keeps every reader below channel-generic and every roster growth free.
    static float[] Lane(EncodedGeometry arena, EncodingChannel channel) {
        if (arena.Descriptors.Find(d => d.Channel == channel).Case is not EncodingChannelDescriptor found) { return []; }
        float[] raw = new float[found.Floats];
        found.Dtype.Unpack(arena.Channel(channel).Span, raw);
        return raw;
    }

    // Strides on the Position row's OWN declared arity, and the reduction rides `Vector3` so the three literal
    // component indexes the hand min/max ladder carried disappear. The returned twelve numbers are the 3D-Tiles
    // `box` array in the spec's own order: centre, then three half-axis vectors.
    // Exemption: a measured min/max reduction over a pre-sized span; the rail resumes at the returned node.
    static float[] Bounds(ReadOnlySpan<float> positions) {
        int arity = EncodingChannel.Position.Arity;
        (Vector3 low, Vector3 high) = (new Vector3(float.MaxValue), new Vector3(float.MinValue));
        for (int offset = 0; offset + arity - 1 < positions.Length; offset += arity) {
            Vector3 point = new(positions[offset], positions[offset + 1], positions[offset + 2]);
            (low, high) = (Vector3.Min(low, point), Vector3.Max(high, point));
        }
        Vector3 centre = (low + high) / 2f, half = (high - low) / 2f;
        return [centre.X, centre.Y, centre.Z, half.X, 0, 0, 0, half.Y, 0, 0, 0, half.Z];
    }

    // Triangle corners resolve through ImportedGeometry.Indices, so indexed shared-vertex meshes partition
    // real corners; triangle-soup ordinal addressing mis-partitions non-identity index buffers.
    static Fin<Seq<ImportedGeometry>> Split(ImportedGeometry geometry, float[] positions, float[] bounds, Op key) {
        (float cx, float cy, float cz) = (bounds[0], bounds[1], bounds[2]);
        return toSeq(Range(0, geometry.TriangleCount)
                .GroupBy(tri => Octant(positions, geometry.Indices.AsSpan(), tri, cx, cy, cz))
                .Select(static group => toSeq(group)))
            .TraverseM(group => Tessellate(geometry, group, key)).As();
    }

    // CONSTRAINT: octant assignment reads the FIRST corner alone, so a triangle straddling the split plane lands
    // entirely in its first vertex's octant. The partition stays a covering (every triangle lands exactly once)
    // and each node's own `Bounds` is measured from the triangles it received, so a straddler widens its node's
    // box rather than leaking geometry out of it — the bound stays honest, the split stays unbalanced.
    static int Octant(ReadOnlySpan<float> positions, ReadOnlySpan<long> indices, int triangle, float cx, float cy, float cz) {
        int v = (int)indices[triangle * 3] * EncodingChannel.Position.Arity;
        return (positions[v] >= cx ? 1 : 0) | (positions[v + 1] >= cy ? 2 : 0) | (positions[v + 2] >= cz ? 4 : 0);
    }

    // Channel-generic corner gather: every DECLARED lane unpacks once, gathers by its OWN arity, and the whole
    // set re-mints through the one kernel raw-lane entry — so a new EncodingChannel row reaches a sliced leaf
    // with zero edit here, where the retired per-column body grew one `if (!srcT.IsEmpty)` rung and one `with`
    // slot per attribute. Descriptor presence IS the absence test; a buffer length probe is the deleted form.
    static Fin<ImportedGeometry> Tessellate(ImportedGeometry geometry, Seq<int> triangles, Op key) {
        ReadOnlySpan<long> srcI = geometry.Indices.AsSpan();
        int vertices = triangles.Count * 3;
        (EncodingChannel Channel, float[] Source, float[] Gathered)[] lanes = [.. geometry.Lanes.Descriptors.Map(d =>
            (d.Channel, Lane(geometry.Lanes, d.Channel), new float[vertices * d.Channel.Arity]))];
        long[] indices = new long[vertices];
        int slot = 0;
        foreach (int tri in triangles) {                                   // Exemption: a measured gather over pre-sized arenas; the rail resumes at the Encode.Of mint
            for (int corner = 0; corner < 3; corner++) {
                int source = (int)srcI[(tri * 3) + corner];
                foreach (var lane in lanes) {
                    int arity = lane.Channel.Arity;
                    lane.Source.AsSpan(source * arity, arity).CopyTo(lane.Gathered.AsSpan((((slot * 3) + corner) * arity), arity));
                }
            }
            (indices[slot * 3], indices[(slot * 3) + 1], indices[(slot * 3) + 2]) = (slot * 3, (slot * 3) + 1, (slot * 3) + 2);
            slot++;
        }
        // Sliced leaves re-describe their pool honestly — one block spanning the leaf, one identity instance,
        // holding the baked-carrier invariant every downstream flat read (payload arms, bounds, splits) reads.
        return Encode.Of(vertices, toSeq(lanes).Map(static lane => (lane.Channel, lane.Gathered)), key)
            .Map(arena => geometry with {
                Lanes = arena, Indices = indices,
                VertexCount = vertices, TriangleCount = triangles.Count,
                Blocks = Seq(new MeshBlock(0, vertices, 0, vertices)),
                Instances = Seq(new MeshInstance(0, Matrix4x4.Identity)),
            });
    }
}

// --- [BOUNDARIES] -------------------------------------------------------------------------
// Tileset EXPORT the partition owns: the real tileset.json Manifest (this page's product) and the LeafContent
// reference set it names — the typed handoff to the Bim glTF leaf codec, resolved against the Persistence index,
// never a body here. `ReplayKey` rides the leaf so the cache-replay law the Boundary legislates has its consumer.
public sealed record LeafContent(UInt128 ContentKey, UInt128 ReplayKey, string Uri, int MetadataColumns);

public sealed record TilesetExport(ComputeArtifact Manifest, TilesetCensus Census, Seq<LeafContent> Leaves);

// The per-node tileset.json member projection, GENERATED. `WriteNode` hand-wrote eleven members against this
// shape, which is a pure owner→DTO rename the `[Mapper]` owns; the recursive `children` array stays a hand
// `Utf8JsonWriter` walk because the manifest is a TREE with no fixed root type and the writer's `BytesPending`
// commit bound is what keeps a policy-depth octree off the large-object heap. That split — generated member
// projection, hand tree emit — is the converter-plus-mapper form the boundaries law names.
public sealed record TileNodeWire(
    [property: JsonPropertyName("boundingVolume")] TileBoxWire BoundingVolume,
    [property: JsonPropertyName("geometricError")] double GeometricError,
    [property: JsonPropertyName("refine")] string Refine,
    [property: JsonPropertyName("content")] TileContentWire? Content);

public sealed record TileBoxWire([property: JsonPropertyName("box")] float[] Box);

public sealed record TileContentWire([property: JsonPropertyName("uri")] string Uri);

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class TileNodeMapper {
    [MapProperty(nameof(TileNode.BoundingVolume), $"{nameof(TileNodeWire.BoundingVolume)}.{nameof(TileBoxWire.Box)}")]
    [MapperIgnoreSource(nameof(TileNode.Depth))]
    [MapperIgnoreSource(nameof(TileNode.Metadata))]
    [MapperIgnoreSource(nameof(TileNode.Children))]
    [MapperIgnoreSource(nameof(TileNode.ContentKey))]
    [MapperIgnoreSource(nameof(TileNode.IsLeaf))]
    public static partial TileNodeWire Wire(TileNode node);

    // The two members no source column supplies: `refine` is the manifest's own constant and `content` exists
    // only on a leaf, so both fill after the generated projection rather than inside it.
    private static string Refine(TileNode node) => "REPLACE";

    private static TileContentWire? Content(TileNode node) =>
        node.IsLeaf ? new TileContentWire(TilePartition.Uri(node.ContentKey)) : null;
}

[JsonSerializable(typeof(TileNodeWire))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public sealed partial class TilesetJsonContext : JsonSerializerContext;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class TilePartition {
    // Arena claim sets already prove descriptor tiling, per-lane recovery inside dtype tolerance, and payload
    // extent, so this gate re-validates none of it. What stays is the CENSUS the two shapes disagree on — the
    // carrier's vertex count against the arena's own element count, the index column against the triangle
    // count, and every index inside the vertex range.
    public static Fin<TilesetExport> ExportTiles(
        StreamPool pool, CorrelationId correlation, ImportedGeometry geometry,
        Func<UInt128, Option<TileMetadata>> metadata, CompanionPolicy policy, IClock clock, Op? key = null) =>
        geometry.VertexCount <= 0 || geometry.TriangleCount <= 0
            || geometry.Lanes.Count != geometry.VertexCount
            || geometry.Indices.Length < geometry.TriangleCount * 3
            || IndexOutOfRange(geometry)
            ? Fin.Fail<TilesetExport>(new ComputeFault.PayloadOverBounds($"<tileset-geometry:{geometry.VertexCount}:{geometry.TriangleCount}:{geometry.Lanes.Count}:{geometry.Indices.Length}>"))
            : TileSet.Build(geometry, metadata, policy, clock, key)
                .Bind(tiles => Tileset(pool, correlation, tiles, policy, clock)
                    .Map(manifest => new TilesetExport(manifest, tiles.Census, Leaves(tiles.Root, clock.GetCurrentInstant()))));

    static bool IndexOutOfRange(ImportedGeometry geometry) {
        ReadOnlySpan<long> indices = geometry.Indices.AsSpan()[..(geometry.TriangleCount * 3)];
        foreach (long index in indices) { if (index < 0 || index >= geometry.VertexCount) { return true; } }
        return false;
    }

    // The ONE leaf-content URI grammar, so the manifest's `content.uri` and the enumerated reference cannot fork.
    internal static string Uri(UInt128 contentKey) => string.Create(CultureInfo.InvariantCulture, $"{contentKey:x32}.glb");

    // tileset.json: refine REPLACE, box bounding volumes off each node's box, geometricError halving per level,
    // leaf content URIs the AppUi/web consumer resolves against the Persistence index. The emit STREAMS through
    // the composition's one `Tensor/memory#STREAM_POOL` capsule rather than an ArrayBufferWriter: a manifest at
    // the policy depth is tens of megabytes, and a growable writer reaches it by doubling through the large-object
    // heap while the pool holds chained blocks and never migrates. `GetBuffer` (the contiguity cliff) and
    // `ToArray` (the migration copy the pool's own ThrowExceptionOnToArray posture bans) never appear — the
    // sequence itself keys the artifact, and the carrier's own mint owns the one copy.
    static Fin<ComputeArtifact> Tileset(StreamPool pool, CorrelationId correlation, TileSet tiles, CompanionPolicy policy, IClock clock) =>
        pool.Get(correlation, new StreamGrant.Open())
            .Bind(staged => Op.Of(name: "tileset.emit").Catch(() => Fin.Succ(Manifested(staged, tiles.Root, policy, clock))));

    // Exemption: the writer-and-stream disposal bracket is the platform-forced statement seam this codec boundary
    // owns; the rail resumes on the returned carrier.
    static ComputeArtifact Manifested(RecyclableMemoryStream staged, TileNode root, CompanionPolicy policy, IClock clock) {
        using (staged) {
            using (Utf8JsonWriter writer = new(staged)) {
                writer.WriteStartObject();
                writer.WriteStartObject("asset");
                writer.WriteString("version", "1.1");
                writer.WriteEndObject();
                writer.WriteNumber("geometricError", root.GeometricError);
                writer.WritePropertyName("root");
                WriteNode(writer, root, policy.CommitWatermark);
                writer.WriteEndObject();
                writer.Flush();
            }
            return ComputeArtifact.Of("tileset.json", staged.GetReadOnlySequence(), clock.GetCurrentInstant(), policy.Vector);
        }
    }

    // The tree walk alone. Each node's OWN members come from the generated projection through the source-gen
    // context, so a manifest member added on `TileNodeWire` reaches the emit with no edit here, and each node
    // commits past the watermark so a deep subtree never accumulates in the writer ahead of the pooled stream.
    static void WriteNode(Utf8JsonWriter writer, TileNode node, int watermark) {
        JsonSerializer.Serialize(writer, TileNodeMapper.Wire(node), TilesetJsonContext.Default.TileNodeWire);
        if (!node.IsLeaf) {
            writer.WriteStartArray("children");
            foreach (TileNode child in node.Children) { WriteNode(writer, child, watermark); }
            writer.WriteEndArray();
        }
        if (writer.BytesPending >= watermark) { writer.Flush(); }
    }

    // One LeafContent per octree leaf — content key, replay key, `{contentKey:x32}.glb` URI, metadata-column
    // count — the typed handoff to the Bim leaf-content producer, whose bodies land on the Persistence blob lane
    // under these keys. ONE fold over the tree, where the census, the flatten, and the filter were three walks.
    static Seq<LeafContent> Leaves(TileNode root, Instant at) =>
        root.Fold(Seq<LeafContent>(), (leaves, node) => node.IsLeaf
            ? leaves.Add(new LeafContent(
                node.ContentKey,
                node.Metadata.Map(meta => meta.ReplayKey(at, 0UL)).IfNone(node.ContentKey),
                Uri(node.ContentKey),
                node.Metadata.Map(static meta => meta.Table.Columns.Count).IfNone(0)))
            : leaves);
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

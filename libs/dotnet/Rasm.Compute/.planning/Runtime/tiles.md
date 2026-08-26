# [COMPUTE_TILES]

Rasm.Compute owns the companion hop that turns IFC into geometry and the streamable octree that partitions that geometry beneath its semantic layer. `Rasm.Bim` owns the IFC semantic object model and its import-export surface, reached at the companion boundary; CAD/STEP/IGES execution crosses the separate `cad.CadService` provider boundary. This lane is HOST-LOCAL and carries no TS_PROJECTION.

Generated `TessellateRequest` owns the companion job and `TilePolicy` owns only local octree and manifest emission. The `TileSet`/`TileNode` octree with its `TileMetadata`/`PropertyTable`/`MetadataProperty`/`FeatureBand` family owns the 3D-Tiles partition and its tileset.json manifest. The lane composes the contract `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry`, the kernel `EncodedGeometry` arena, `Runtime/codecs#CONTENT_ADDRESSING` for every key and address, and the `Tensor/memory#STREAM_POOL` capsule for the manifest emit.

## [01]-[INDEX]

- [02]-[TWO_HOP_TESSELLATION]: canonical IFC SPF crosses to the companion, never in-proc, over the real Tessellate-plus-artifact-fetch peer boundary.
- [03]-[TILE_PARTITION]: 3D-Tiles octree partition; streamable LOD over the content-keyed geometry; the manifest this page owns and the leaf bodies it never emits.

## [02]-[TWO_HOP_TESSELLATION]

- Owner: `CompanionEdge` — the peer client boundary composing generated `ComputeService.Tessellate` with the response-keyed `ArtifactService.Fetch`; `CompanionArtifact` — the response and admitted, whole-hash-verified GLB bytes returned together; the contract `Rasm.Element/Projection/projection#INTERCHANGE_CARRIER` `ImportedGeometry` the decoded mesh-pool carrier the re-import lands and the tile partition reads (ONE shape at the boundary — the Bim pipeline produces it, this lane consumes it, no Compute-local twin).
- Cases: generated `TessellateRequest` IS the IFC job — one required source artifact carrying the complete IfcOpenShell policy, scope, settings, and tolerance. The Python companion returns GLB through the `artifact` frame boundary for Bim glTF re-import; Compute owns no parallel request or policy mirror.
- Entry: `CompanionEdge.Tessellate(services, spine, pool, request, token)` validates and invokes the generated unary client, fetches the returned `ArtifactRef` through the incremental `FrameEdge.Fetch`, and returns one `CompanionArtifact`; the product root first puts the IFC source, then transcribes that admitted ref with the Bim request and returns the GLB through the Bim port for store-before-re-entry.
- Auto: the Bim bridge gates the hop on the source format's companion-tessellation capability, performs durable GLB reuse before crossing, and transcribes every output-affecting setting onto the generated request. `ParseGuard.Validated` applies the corpus rules and `CallSpine.Bounded` applies the transport ceiling before this client invokes the peer; the returned response passes the same corpus admission before its artifact hash becomes the typed fetch request, with no upload-shaped control frame. This service call is not a `ComputeIntent`, so it carries caller cancellation through `CallSpine.Options(token)` and receives the interceptor's canonical hop deadline without manufacturing an `AdmittedIntent`. The one unavoidable raw-byte flatten is admitted as `AllocationClass.EdgeCopy`, and its `AllocationEvidence` settles through `StreamPool.Stamp` before the GLB leaves Compute.
- Output: the generated `TessellateResponse` carries the companion result identity, element/triangle census, optional semantic labels, and generated spill verdict beside the admitted GLB; the product root proves its policy-folded content key against the Bim request and projects the remaining fields onto Bim's immutable peer evidence, where the decoded GLB must reproduce both counts before store; a cache hit is resolved by the Bim bridge before this client is called.
- Growth: a new tessellation companion is one transport-row consumption (never a new transport); a new output-affecting evaluation parameter lands at the corpus and the Bim bridge transcription in the same change, never in a Compute-local mirror; zero new surface.
- Boundary: the two-hop pipeline is the single IFC-to-geometry path — the Bim IFC object model carries no tessellation kernel, so a managed IFC BRep evaluator is the deleted form; the companion is the IfcOpenShell PyPI package in `libs/python/geometry`, never a NuGet pin, reached only over the existing `Runtime/channels#TRANSPORT_AXIS` UDS/InProcess companion rpc, so this page mints no transport, channel, or second wire vocabulary; a returned GLB re-enters the Bim glTF import pipeline as one `ImportedGeometry`, and the Bim IFC semantic graph and this hop's tessellated geometry are two projections of one content-keyed IFC artifact joined by the content key. GeoArrow conversion and IDS audit remain their local capability owners until a distinct corpus message and real peer pair can represent them; neither aliases onto Tessellate.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public sealed record TilePolicy(
    int TileMaxDepth,
    double TileGeometricErrorRoot,
    double TileSplitThreshold,
    int CommitWatermark) {
    public static readonly TilePolicy Canonical = new(
        TileMaxDepth: 16, TileGeometricErrorRoot: 512.0, TileSplitThreshold: 8192.0,
        CommitWatermark: 64 * 1024);

    public ReadOnlyMemory<double> Vector => new double[] { TileMaxDepth, TileGeometricErrorRoot, TileSplitThreshold };
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CompanionArtifact(TessellateResponse Response, ReadOnlyMemory<byte> Glb);

public static class CompanionEdge {
    public static IO<Fin<CompanionArtifact>> Tessellate(
        WireServices services,
        CallSpine spine,
        StreamPool pool,
        TessellateRequest request,
        CancellationToken token) {
        WireCall calls = services.Bind(spine);
        return (from admitted in FinT.lift<IO, TessellateRequest>(ParseGuard.Validated(request).Bind(CallSpine.Bounded))
                from response in FinT.liftIO<IO, TessellateResponse>(CallSpine.Awaited(
                    () => calls.Compute.TessellateAsync(admitted, spine.Options(token)).ResponseAsync,
                    token))
                from admittedResponse in FinT.lift<IO, TessellateResponse>(ParseGuard.Validated(response))
                from copied in FinT.liftIO<IO, FrameCopy>(
                    FrameEdge.Fetch(calls, spine, pool, admittedResponse.Artifact, token))
                let stamped = pool.Stamp(copied.Evidence)
                select new CompanionArtifact(admittedResponse, copied.Payload)).runFin.As();
    }
}
```

## [03]-[TILE_PARTITION]

- Owner: `TileSet` the 3D-Tiles octree partition over the contract `ImportedGeometry` — one kernel `EncodedGeometry` arena read by descriptor, baked once at `Build`, leaves gathered lane-generically and re-minted as single-block identity pools; `TileNode` the per-node bounding-volume/geometric-error/content-key record carrying its `Option<TileMetadata>` semantic layer and the ONE `Fold` algebra every census, flatten, and emit walks; `MetadataProperty` `[Union]` the `EXT_structural_metadata` typed property-column cases; `PropertyTable` the per-tile feature-keyed property-table carrier; `TileMetadata` the per-leaf content-keyed metadata property table joining the IFC classification column and the solver field-value columns under one feature-id mapping, independently addressable at its own tile content key; `FeatureBand` `[SmartEnum<string>]` the solved-field styling-band rows; `TileNodeWire` the generated per-node tileset.json projection; `LeafContent`/`TilesetCensus`/`TilesetExport` the manifest-plus-leaf-reference export carriers; `TilePartition` the tileset-manifest emit fold.
- Entry: `public static Fin<TilesetExport> ExportTiles(StreamPool pool, CorrelationId correlation, ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TilePolicy policy, IClock clock, Op? key = null)` builds the gated octree, attaches the per-leaf metadata read at the node content key, serializes the real tileset.json manifest, and enumerates the leaf-content references the manifest names — the leaf BODIES are the Bim glTF codec's product resolved at the content-key URIs, never emitted here; `public static Fin<TileSet> Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TilePolicy policy, Op? key = null)` admits a census-consistent geometry AT CONSTRUCTION, bakes once, and partitions it into the depth-bounded octree, seating `TileSet.At` from the carrier's own causal stamp; `Fin<T>` aborts on a census disagreement (`PayloadOverBounds` — a vertex-less mesh otherwise emits a manifest of float sentinel bounds, refused at every door because the gate sits on `Build`), on a bake or per-leaf arena re-mint the kernel refuses, while a throwing serializer remains the original exceptional Error.
- Auto: `Build` bakes the pool through the contract's ONE `Bake(Op)` flatten, then partitions octant-by-octant to the policy max depth or triangle split threshold, geometric error the root error halved per depth, per-node content key via the channel-generic `InterchangeIdentity.Key` over the arena so a re-partition of identical geometry keys identically AND a lane-roster growth re-keys, then reads the per-leaf `TileMetadata` at that content key so one key addresses geometry and metadata. Every geometric read resolves BY DESCRIPTOR through the one `Lane` unpack — bounds and octant assignment stride on `EncodingChannel.Position.Arity`, and the per-leaf `Tessellate` gathers each declared lane by its own arity and re-mints the whole set through the kernel `Encode.Of` raw-lane entry, so a sliced leaf carries every lane its parent declared (UV and colour included) and a new channel row costs this partition nothing. `ExportTiles` serializes the tileset.json manifest through the composition's one pooled stream — each node's OWN members generated by the `[Mapper]` projection and committed past the writer's `BytesPending` watermark so the writer stays one commit wide — keys it off the pooled `ReadOnlySequence<byte>` through the incremental identity so a resident manifest materializes nothing, and folds the octree ONCE for the leaf references, the node census, and the depth the result reads. `TileMetadata.Join` folds the `Rasm.Bim` IFC classification and the `Solver/field#DISCRETE_FIELD` `FieldSpace` per-element field values read at the shared content key into one feature-keyed property table under its own tile content key; the leaf replay key composes the node content key with the carrier's causal stamp and the leaf's pre-order ordinal through `InterchangeIdentity.Compose` in (physical, logical) order — both halves deterministic from the admitted content, so one octree exported twice replay-keys identically — and `LeafContent` CARRIES that replay key so the cache-replay law the Boundary legislates has the consumer it names; `FeatureBand.Of` classifies an achieved field value onto its styling band.
- Output: `Streamed` carries the manifest artifact id and the emitted bytes, and `TilesetExport.Census` is the value the leaf-reference count, root geometric error, max depth, node count, and per-leaf property-column count all read off — one fold, one carrier, never five columns a projection re-derives.
- Packages: System.IO.Hashing, CommunityToolkit.HighPerformance, SharpGLTF.Core, SharpGLTF.Toolkit, SharpGLTF.Ext.3DTiles (the `Schema2.Tiles3D` EXT_structural_metadata/EXT_mesh_features leaf-body schema surface, admitted via `Tiles3DExtensions.RegisterExtensions()` once at composition — the settled Compute admission; models no tileset.json manifest tree), meshoptimizer, Microsoft.IO.RecyclableMemoryStream (the `RecyclableMemoryStream` the `Tensor/memory#STREAM_POOL` capsule grants — never a manager constructed here), Riok.Mapperly (the per-node tileset.json member projection), Generator.Equals, LanguageExt.Core, NodaTime, Rasm (project — the kernel `EncodedGeometry` arena with `Encode.Of`, `Channel`, and `Descriptors`, the `EncodingChannel` lane roster, `ChannelDtype.Unpack`, `CorrelationId`, and `Op` the admission channel), Rasm.Element (project — the contract `ImportedGeometry`/`MeshBlock`/`MeshInstance` carrier), Rasm.Persistence (project), BCL inbox (`System.Text.Json` `Utf8JsonWriter` over the pooled stream with its `BytesPending` commit read, `System.Buffers` `ReadOnlySequence<byte>`)
- Growth: a new tile-partition parameter is one column on `TilePolicy` folded into the partition; a new per-vertex attribute is one kernel `EncodingChannel` row the partition reads, slices, and re-mints with ZERO edit here, because every geometric body addresses lanes by descriptor and none names a channel; a new metadata property is one `MetadataProperty` case folded into the property table; a new styling band is one `FeatureBand` row; a new manifest member is one column on `TileNodeWire` the generated mapper fills; a new leaf-tile content format is one row on the Bim format axis the leaf emit reads; zero new surface — a `TileMetadataStore`/`FeatureAttributeTable` sibling owner is collapsed onto the one `TileMetadata`/`PropertyTable` family on the leaf-tile content emit.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
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

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PropertyTable(string Class, int FeatureCount, Seq<MetadataProperty> Columns);

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
}

public sealed record TileNode(int Depth, float[] BoundingVolume, double GeometricError, UInt128 ContentKey, Option<TileMetadata> Metadata, Seq<TileNode> Children) {
    public TState Fold<TState>(TState seed, Func<TState, TileNode, TState> step) =>
        Children.Fold(step(seed, this), (state, child) => child.Fold(state, step));

    public bool IsLeaf => Children.IsEmpty;
}

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
    public static Fin<TileSet> Build(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TilePolicy policy, Op? key = null) {
        Op k = key.OrDefault();
        return geometry.VertexCount <= 0 || geometry.TriangleCount <= 0
            || geometry.Lanes.Count != geometry.VertexCount
            || geometry.Indices.Length < geometry.TriangleCount * 3
            || IndexOutOfRange(geometry)
            ? Fin.Fail<TileSet>(new ComputeFault.PayloadOverBounds($"<tileset-geometry:{geometry.VertexCount}:{geometry.TriangleCount}:{geometry.Lanes.Count}:{geometry.Indices.Length}>"))
            : geometry.Bake(k)
                .Bind(baked => Partition(baked, metadata, policy, depth: 0, k))
                .Map(root => new TileSet(root, TilesetCensus.Of(root), geometry.At));
    }

    static bool IndexOutOfRange(ImportedGeometry geometry) {
        ReadOnlySpan<long> indices = geometry.Indices.AsSpan()[..(geometry.TriangleCount * 3)];
        foreach (long index in indices) { if (index < 0 || index >= geometry.VertexCount) { return true; } }
        return false;
    }

    static Fin<TileNode> Partition(ImportedGeometry geometry, Func<UInt128, Option<TileMetadata>> metadata, TilePolicy policy, int depth, Op key) {
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

    static float[] Lane(EncodedGeometry arena, EncodingChannel channel) {
        if (arena.Descriptors.Find(d => d.Channel == channel).Case is not EncodingChannelDescriptor found) { return []; }
        float[] raw = new float[found.Floats];
        found.Dtype.Unpack(arena.Channel(channel).Span, raw);
        return raw;
    }

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

    static Fin<Seq<ImportedGeometry>> Split(ImportedGeometry geometry, float[] positions, float[] bounds, Op key) {
        (float cx, float cy, float cz) = (bounds[0], bounds[1], bounds[2]);
        return toSeq(Range(0, geometry.TriangleCount)
                .GroupBy(tri => Octant(positions, geometry.Indices.AsSpan(), tri, cx, cy, cz))
                .Select(static group => toSeq(group)))
            .TraverseM(group => Tessellate(geometry, group, key)).As();
    }

    static int Octant(ReadOnlySpan<float> positions, ReadOnlySpan<long> indices, int triangle, float cx, float cy, float cz) {
        int v = (int)indices[triangle * 3] * EncodingChannel.Position.Arity;
        return (positions[v] >= cx ? 1 : 0) | (positions[v + 1] >= cy ? 2 : 0) | (positions[v + 2] >= cz ? 4 : 0);
    }

    static Fin<ImportedGeometry> Tessellate(ImportedGeometry geometry, Seq<int> triangles, Op key) {
        ReadOnlySpan<long> srcI = geometry.Indices.AsSpan();
        int vertices = triangles.Count * 3;
        (EncodingChannel Channel, float[] Source, float[] Gathered)[] lanes = [.. geometry.Lanes.Descriptors.Map(d =>
            (d.Channel, Lane(geometry.Lanes, d.Channel), new float[vertices * d.Channel.Arity]))];
        long[] indices = new long[vertices];
        int slot = 0;
        foreach (int tri in triangles) {
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
        return Encode.Of(vertices, toSeq(lanes).Map(static lane => (lane.Channel, lane.Gathered)), key)
            .Map(arena => geometry with {
                Lanes = arena, Indices = indices,
                VertexCount = vertices, TriangleCount = triangles.Count,
                Blocks = Seq(new MeshBlock(0, vertices, 0, vertices)),
                Instances = Seq(new MeshInstance(0, Matrix4x4.Identity)),
            });
    }
}

// --- [BOUNDARIES] ----------------------------------------------------------------------
public sealed record LeafContent(UInt128 ContentKey, UInt128 ReplayKey, string Uri, int MetadataColumns);

public sealed record TilesetExport(ComputeArtifact Manifest, TilesetCensus Census, Seq<LeafContent> Leaves);

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

    private static string Refine(TileNode node) => "REPLACE";

    private static TileContentWire? Content(TileNode node) =>
        node.IsLeaf ? new TileContentWire(TilePartition.Uri(node.ContentKey)) : null;
}

[JsonSerializable(typeof(TileNodeWire))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public sealed partial class TilesetJsonContext : JsonSerializerContext;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class TilePartition {
    public static Fin<TilesetExport> ExportTiles(
        StreamPool pool, CorrelationId correlation, ImportedGeometry geometry,
        Func<UInt128, Option<TileMetadata>> metadata, TilePolicy policy, IClock clock, Op? key = null) =>
        TileSet.Build(geometry, metadata, policy, key)
            .Bind(tiles => Tileset(pool, correlation, tiles, policy, clock)
                .Map(manifest => new TilesetExport(manifest, tiles.Census, Leaves(tiles.Root, tiles.At))));

    internal static string Uri(UInt128 contentKey) => string.Create(CultureInfo.InvariantCulture, $"{contentKey:x32}.glb");

    static Fin<ComputeArtifact> Tileset(StreamPool pool, CorrelationId correlation, TileSet tiles, TilePolicy policy, IClock clock) =>
        pool.Get(correlation, new StreamGrant.Open())
            .Bind(staged => Op.Of(name: "tileset.emit").Catch(() => Fin.Succ(Manifested(staged, tiles.Root, policy, clock))));

    static ComputeArtifact Manifested(RecyclableMemoryStream staged, TileNode root, TilePolicy policy, IClock clock) {
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

    static void WriteNode(Utf8JsonWriter writer, TileNode node, int watermark) {
        JsonSerializer.Serialize(writer, TileNodeMapper.Wire(node), TilesetJsonContext.Default.TileNodeWire);
        if (!node.IsLeaf) {
            writer.WriteStartArray("children");
            foreach (TileNode child in node.Children) { WriteNode(writer, child, watermark); }
            writer.WriteEndArray();
        }
        if (writer.BytesPending >= watermark) { writer.Flush(); }
    }

    static Seq<LeafContent> Leaves(TileNode root, Instant at) =>
        root.Fold(Seq<LeafContent>(), (leaves, node) => node.IsLeaf
            ? leaves.Add(new LeafContent(
                node.ContentKey,
                InterchangeIdentity.Compose(node.ContentKey, at, (ulong)leaves.Count),
                Uri(node.ContentKey),
                node.Metadata.Map(static meta => meta.Table.Columns.Count).IfNone(0)))
            : leaves);
}
```

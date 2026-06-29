# [COMPUTE_RESIDENCY]

Rasm.Compute interchange lane: the streaming-residency half of artifact interchange, owning the content-keyed GPU-ready **PAYLOAD** codec the web viewer streams cell-by-cell — the meshlet-cluster encode that partitions an octree-leaf `ImportedGeometry` into cone-cullable clusters with the `meshoptimizer` vertex/index/sequence codecs, the quantized-vertex encode that exponent-filters and level-compresses a leaf for a low-VRAM tile, the point-splat encode that decimates a reality-capture point set, and the gaussian-splat encode that octahedral/quaternion/exponent-filters a Python-companion-decoded `SplatScan` into a content-addressed splat tile. The page owns ONE payload axis (`ResidencyKind`), ONE buffer-role axis (`ResidencyStream`), ONE meshopt decode-mode axis (`StreamMode`), ONE filter axis (`StreamFilter`), ONE polymorphic encode input (`ResidencySource`), ONE content-keyed carrier (`ResidencyPayload`), and ONE `Encode` fold over the **safe** `Meshopt` span surface — it produces bytes addressed by the suite `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` key, reads the existing `Runtime/codecs#TILE_PARTITION` `ImportedGeometry` octree leaf (never a second partition), and rides the existing `Runtime/receipts#RECEIPT_UNION` `StreamSegment` slot (never a new receipt case). The streamable manifest that names these payloads — the `WEB_GEOMETRY_RESIDENCY_WIRE` scene-graph plus meshlet/splat residency manifest the TypeScript worker consumes — is minted **once** by `csharp:Rasm.AppUi/Render/pipeline#TS_PROJECTION` `ResidencyManifest.Mint`; this page mints **no** manifest, only the content-keyed payload blob plus the self-describing `StreamSpan` bufferView layout (per-stream `StreamMode`/`StreamFilter`/count/stride), the cone-cull `ResidencyMeshlet` clusters, and the tile bounding sphere — the EXT_meshopt_compression bufferView set the AppUi manifest projects 1:1 by content key, so a Compute-side `ResidencyManifest` is the named second-mint drift defect. The encoded blob lands content-addressed on the Persistence blob lane through the same `Runtime/codecs#CONTENT_ADDRESSING` `ArtifactIndexRow.Admit` path at the app-platform seam, and the splat arm grounds its scan on the Python `realitycapture` companion decode reached as `ArtifactFrame` bytes at the `Runtime/channels#PROTO_VOCABULARY` `ArtifactSync` artifact seam, never an in-process splat fit or an in-process SPZ/SOG decoder; this page is HOST-LOCAL and carries no TS_PROJECTION.

## [01]-[INDEX]

- [01]-[RESIDENCY]: the `ResidencyKind` payload axis, the `ResidencyStream`/`StreamMode`/`StreamFilter` buffer-role, decode-mode, and filter axes, the `ResidencySource` encode input, the `ResidencyPayload` carrier, and the `Encode` fold over the safe `Meshopt` span surface.

## [02]-[RESIDENCY]

- Owner: `ResidencyKind` `[SmartEnum<string>]` the one closed payload axis — `meshlet-cluster` · `quantized-vertex` · `point-splat` · `gaussian-splat` — each row carrying the `ConeCullable`/`SplatBorne` posture the AppUi marshal reads to pick its cull and shader, so a new encoding is one row and never a per-kind payload type; `ResidencyStream` `[SmartEnum<string>]` the one closed buffer-role axis (`positions` · `normals` · `indices` · `triangles` · `scales` · `rotations` · `harmonics`) keying every encoded stream, so a new attribute is one row and never a new payload field; `StreamFilter` `[SmartEnum<string>]` the one closed `meshoptimizer` attribute-filter axis whose key IS the `EXT_meshopt_compression` decode-filter mode the manifest emits (`NONE` · `OCTAHEDRAL` · `QUATERNION` · `EXPONENTIAL`); `ResidencySource` `[Union]` the polymorphic encode input — `Leaf(ResidencyKind, ImportedGeometry)` for the octree-leaf arms, `Splat(SplatScan)` for the companion-decoded scan — so one entry discriminates on input shape, never a `Encode`/`EncodeSplat` name pair; `ResidencyMeshlet` the readonly per-cluster cone-and-sphere descriptor the meshlet arm emits; `ResidencyPolicy` the encode-posture record carrying the meshlet caps, the quantization bit budget, the codec level and pinned format version, the simplification target, the attribute weight, and the cone weight; `StreamMode` `[SmartEnum<string>]` the one closed EXT_meshopt_compression decode-mode axis (`ATTRIBUTES` · `TRIANGLES` · `INDICES` · `RAW`) keying which codec the web worker inverts per stream (vertex-attribute · triangle-index · index-sequence · un-encoded passthrough for the raw meshlet triangle bytes); `StreamSpan` the per-stream EXT_meshopt_compression bufferView descriptor `(Offset, Length, Count, ByteStride, Mode, Filter)` into the payload blob; `ResidencyPayload` the **content-keyed buffer carrier** — the assembled payload blob, the per-stream `StreamSpan` bufferView layout, the cone-cull `ResidencyMeshlet` clusters, the resident count, the tile bounding sphere (`Center`/`Radius`), the gaussian-splat harmonic degree, and the `XxHash128` content key, NOT a manifest; `Residency` the static `Encode` fold projecting a `ResidencySource` into a `ResidencyPayload` through the safe `Meshopt` span surface plus the `Receipt` projection onto the settled `StreamSegment` slot.
- Cases: `ResidencyKind` rows `meshlet-cluster` (cone-cullable cluster set: a global vertex stream, the `EncodeIndexSequence` meshlet-vertex table, the raw local triangle bytes, and the per-cluster `ResidencyMeshlet` descriptors) · `quantized-vertex` (exponent-filtered, level-compressed single tile) · `point-splat` (`SimplifyPoints`-decimated, exponent-filtered positions — the reality-capture LOD floor) · `gaussian-splat` (companion-decoded `SplatScan` whose positions/scales/harmonics exponent-filter and whose rotation quaternions quaternion-filter into content-addressed streams).
- Entry: `public static Fin<ResidencyPayload> Encode(ResidencySource source, ResidencyPolicy policy)` projects an octree-leaf geometry (or a companion-decoded splat scan) onto the kind's encode arm and lands the content-keyed payload; `public static ComputeReceipt.StreamSegment Receipt(ResidencyPayload payload, CorrelationId correlation, WorkLane lane, Duration elapsed)` projects the payload onto the settled receipt slot; `Fin<T>` aborts onto `ComputeFault.PayloadOverBounds` for an empty meshlet build, an out-of-range quantization budget, or an out-of-range simplify target, and onto `ComputeFault.ModelRejected` for a leaf routed at a splat-borne kind (a gaussian splat needs a `Splat` source, never a `Leaf`).
- Auto: `Encode` dispatches the `ResidencySource` union through one state-threaded total `Switch` — the `Leaf` arm sub-dispatches the `ResidencyKind` through a second total `Switch` and the `Splat` arm folds the scan; the `meshlet-cluster` arm cache-optimizes the leaf index buffer through `Meshopt.OptimizeVertexCache` (topology-preserving, never a vertex remap that would strand the index buffer), builds cone-cullable clusters through `Meshopt.BuildMeshlets` sized by `Meshopt.BuildMeshletsBound`, per-cluster cache-optimizes through `Meshopt.OptimizeMeshlet`, computes each cluster's cone-and-sphere `Bounds` through `Meshopt.ComputeMeshletBounds`, encodes the global vertex stream through `Meshopt.EncodeVertexBufferLevel<Vector3>`, encodes the local→global meshlet-vertex table through `Meshopt.EncodeIndexSequence`, and carries the local triangle bytes raw so a mesh-shader consumer reconstructs each meshlet from the `ResidencyMeshlet` offsets; the `quantized-vertex` arm exponent-filters the positions through `Meshopt.EncodeFilterExp` at the policy bit budget and encodes the triangle list through `Meshopt.EncodeIndexBuffer`; the `point-splat` arm decimates through `Meshopt.SimplifyPoints` (normal-weighted so feature edges survive) and exponent-filters the kept positions; the `gaussian-splat` arm exponent-filters positions/scales/harmonics and quaternion-filters the rotation quaternions through `Meshopt.EncodeFilterQuat`; every arm octahedral-filters present normals through `Meshopt.EncodeFilterOct`, assembles the encoded streams into one payload blob, keys it through `InterchangeIdentity.Key` over the whole blob folded with the kind tag and the lossy posture, stamps the keyed `StreamSpan` bufferView layout (per-stream `StreamMode`/`StreamFilter`/count/stride) and the tile bounding sphere (`Meshopt.ComputeSphereBounds`) the manifest reads, and — because `EncodeIndexBuffer`/`EncodeIndexSequence` carry no per-call version — relies on the one-time `Residency` type-init `Meshopt.EncodeIndexVersion` pin so the index/sequence streams are byte-reproducible for identical geometry across processes.
- Receipt: the `Runtime/receipts#RECEIPT_UNION` `StreamSegment(string ArtifactId, int Segments, long Bytes)` slot carries the payload `ArtifactKey` as the artifact id, the cluster count (meshlet) or stream count (other kinds) as the segment count, and the blob byte length — a re-encode of identical leaf geometry at identical policy stamps the same content-key so the residency emission is auditable through the existing slot, never a new receipt case; the addressed blob lands on the Persistence blob lane through `ArtifactIndexRow.Admit` at the app-platform seam so a re-emitted identical payload dedups and a `Cache` receipt records the hit.
- Packages: Alimer.Bindings.MeshOptimizer, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new residency encoding is one `ResidencyKind` row with its encode-arm `Switch` case; a new attribute is one `ResidencyStream` row plus its filtered-stream line; a new filter is one `StreamFilter` row and a new decode mode is one `StreamMode` row, both carried on the `StreamSpan` bufferView; a new posture is one column on `ResidencyPolicy`; a new source modality is one `ResidencySource` case; zero new surface — a `MeshletResidencyEncoder`/`SplatPayloadCodec`/`QuantizedVertexEncoder` sibling owner collapses onto the one `Residency.Encode` fold, a per-kind `ResidencyPayload` subtype collapses onto the one content-keyed carrier whose kind column discriminates, and the parallel `EncodedVertices`/`EncodedIndices`/`EncodedMeshlets` byte fields collapse onto the one `ResidencyStream`-keyed `StreamSpan` layout.
- Boundary: this lane owns ONLY the content-keyed PAYLOAD blob plus the `StreamSpan` layout — the streamable `WEB_GEOMETRY_RESIDENCY_WIRE` manifest that names them is minted once by `csharp:Rasm.AppUi/Render/pipeline#TS_PROJECTION` `ResidencyManifest.Mint` — which projects each resident `ResidencyPayload` 1:1 into an EXT_meshopt_compression bufferView wire row from the payload's `StreamSpan` layout, `ResidencyMeshlet` clusters, tile bounding sphere, harmonic degree, and `ContentKey` (never re-deriving a content key from raw positions, never re-modelling the streams) — and this page mints no manifest, no scene-graph, and no second content-identity value object (a Compute-side `ResidencyManifest` is the named drift defect, and the `:x32` content-key the AppUi marshal renders is the suite `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128.HashToUInt128` value, never a second hash); the leaf geometry is read from the existing `Runtime/codecs#TILE_PARTITION` `TileSet` octree leaf `ImportedGeometry` (`codecs` partitions, this lane encodes the partitioned leaf), so a second octree partition in this page is the rejected form, and the residency content-key composes the same `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Key` law over the WHOLE assembled blob (every stream, not the vertex stream alone) folded with the `kind` tag and the lossy posture scalars so a meshlet tile, a quantized tile, a point tile, and a splat tile are content-keyed rows under one identity scheme the Persistence index owns and two encodes that differ in any policy that changes the bytes key distinctly; the process-global meshopt index/sequence codec version is pinned once at `Residency` type init (`Meshopt.EncodeIndexVersion`, the version-less `EncodeIndexBuffer`/`EncodeIndexSequence` legs having no per-call version arg) so identical leaf geometry produces a byte-identical blob and dedups across processes rather than keying divergently, while the vertex codec rides the per-call `ResidencyPolicy.CodecVersion`; `meshoptimizer` owns every meshlet build, cluster bounds, cache optimization, vertex/index/sequence codec, attribute filter, and point decimation — the package's safe `Span<T>`/generic overloads (pinning internally) own the codec, filter, cache-optimize, and index legs directly, while the four count-bearing builds (`BuildMeshlets`/`ComputeMeshletBounds`/`ComputeSphereBounds`/`SimplifyPoints`) take the explicit pointer overload because the convenience wrapper passes the element-span length where the native wants the true vertex/triangle/point count, so the whole-surface `fixed`-pinned reimplementation collapses to those four pinned boundary kernels plus the `Unsafe.As<Bounds, float>` reinterpret that reads the native `Bounds` `fixed float` prefix, and a hand-rolled meshlet partitioner, a hand-rolled vertex codec, and a hand-rolled point simplifier are the deleted forms; the meshlet residency keeps the cluster structure intact — the global vertex stream encodes once, the local→global `meshlet_vertices` table encodes as an `EncodeIndexSequence` stream, and the local `meshlet_triangles` bytes carry raw under the `ResidencyMeshlet` offsets, so a consumer reconstructs each cluster, and an `EncodeIndexBuffer` over the local triangle bytes (which are NOT triangle-list indices into the vertex buffer) is the deleted correctness defect; the `gaussian-splat` arm homes a splat payload but does NOT admit a splat fit or an in-process SPZ/SOG decoder — the scan is decoded by the Python `realitycapture` companion and crosses the `Runtime/channels#PROTO_VOCABULARY` `ArtifactSync` `ArtifactFrame` byte seam as the `Runtime/channels` `GaussianSplatScan` artifact message (the `GeometryPayload` oneof carries point-cloud/mesh/voxel only, never a splat case — the scan rides the generic `ArtifactFrame` bytes as that standalone artifact), reassembled and admitted as a `SplatScan` upstream at that wire seam, so this arm is fully implemented over whatever scan it is handed and the block is honestly the companion wire, an in-process SPZ/SOG decoder being the same rejected form the `csharp:Rasm.AppUi/Render/reality#SPLAT_SOURCE` `SplatSource` arm marks `[UPSTREAM-BLOCKED]`; the point-cloud-**file** readers (E57/LAS/LAZ/PTS) are the DISTINCT `Runtime/codecs#FIELD_RESULT_CODEC` `point-catalogue-pending` concern this lane neither supersedes nor admits; the assembled blob is the single payload window content-keyed and homed on the blob lane, so a `ToArray` flatten past that one window is the named defect.

```csharp contract
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ResidencyKind {
    public static readonly ResidencyKind MeshletCluster = new("meshlet-cluster", coneCullable: true, splatBorne: false);
    public static readonly ResidencyKind QuantizedVertex = new("quantized-vertex", coneCullable: false, splatBorne: false);
    public static readonly ResidencyKind PointSplat = new("point-splat", coneCullable: false, splatBorne: false);
    public static readonly ResidencyKind GaussianSplat = new("gaussian-splat", coneCullable: false, splatBorne: true);

    public bool ConeCullable { get; }
    public bool SplatBorne { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ResidencyStream {
    public static readonly ResidencyStream Positions = new("positions");
    public static readonly ResidencyStream Normals = new("normals");
    public static readonly ResidencyStream Indices = new("indices");
    public static readonly ResidencyStream Triangles = new("triangles");
    public static readonly ResidencyStream Scales = new("scales");
    public static readonly ResidencyStream Rotations = new("rotations");
    public static readonly ResidencyStream Harmonics = new("harmonics");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class StreamFilter {
    public static readonly StreamFilter None = new("NONE");
    public static readonly StreamFilter Octahedral = new("OCTAHEDRAL");
    public static readonly StreamFilter Quaternion = new("QUATERNION");
    public static readonly StreamFilter Exponential = new("EXPONENTIAL");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class StreamMode {
    public static readonly StreamMode Attributes = new("ATTRIBUTES");
    public static readonly StreamMode Triangles = new("TRIANGLES");
    public static readonly StreamMode Indices = new("INDICES");
    public static readonly StreamMode Raw = new("RAW");
}

[Union]
public abstract partial record ResidencySource {
    private ResidencySource() { }

    public sealed record Leaf(ResidencyKind Kind, ImportedGeometry Geometry) : ResidencySource;

    public sealed record Splat(SplatScan Scan) : ResidencySource;
}

// --- [MODELS] --------------------------------------------------------------------------

public sealed record ResidencyPolicy(
    int MaxVertices,
    int MaxTriangles,
    float ConeWeight,
    int QuantizationBits,
    int CodecLevel,
    int CodecVersion,
    double SimplifyTarget,
    double AttributeWeight) {
    public static readonly ResidencyPolicy Canonical = new(
        MaxVertices: 64, MaxTriangles: 124, ConeWeight: 0.25f, QuantizationBits: 14,
        CodecLevel: 2, CodecVersion: 0, SimplifyTarget: 0.25, AttributeWeight: 0.5);
}

public readonly record struct ResidencyMeshlet(
    int VertexOffset,
    int TriangleOffset,
    int VertexCount,
    int TriangleCount,
    Vector3 Center,
    float Radius,
    Vector3 ConeApex,
    Vector3 ConeAxis,
    float ConeCutoff);

// the EXT_meshopt_compression bufferView descriptor per stream: byte window + the element Count/ByteStride the
// meshopt decoder reads + the decode Mode (attribute/triangle/index codec, or Raw for the un-encoded meshlet
// triangle bytes) + the inverse Filter — the full set the AppUi manifest emits so the web worker decodes the blob
public readonly record struct StreamSpan(int Offset, int Length, int Count, int ByteStride, StreamMode Mode, StreamFilter Filter);

// exp-packed three-component carrier (12 bytes; three little-endian quantized words the meshopt
// exponent filter writes and the codec compresses; never read back as floats on this side)
public readonly record struct Packed12(uint A, uint B, uint C);

public sealed record SplatScan(
    string FormatKey,
    ReadOnlyMemory<float> Positions,
    ReadOnlyMemory<float> Scales,
    ReadOnlyMemory<float> Rotations,
    ReadOnlyMemory<float> Harmonics,
    int HarmonicDegree,
    long SplatCount);

public sealed record ResidencyPayload(
    ResidencyKind Kind,
    UInt128 ContentKey,
    ReadOnlyMemory<byte> Blob,
    FrozenDictionary<ResidencyStream, StreamSpan> Layout,
    Seq<ResidencyMeshlet> Clusters,
    int ResidentCount,
    Vector3 Center,
    float Radius,
    int HarmonicDegree) {
    public string ArtifactKey => $"{ContentKey:x32}:{Kind.Key}";

    public long EncodedBytes => Blob.Length;
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Residency {
    const int PositionStride = 3 * sizeof(float);
    const int OctBits = 8;
    const int IndexCodecVersion = 1;

    // one-time process-global codec-version pin — EncodeIndexBuffer/EncodeIndexSequence carry NO per-call version
    // arg (unlike EncodeVertexBufferLevel), so the meshlet vertex-table + triangle-index streams emit whatever the
    // process-global EncodeIndexVersion holds; pinned here at type init (fires before the first Encode) so identical
    // leaf geometry produces byte-identical blobs and content-keys identically across processes — without the pin,
    // meshlet-payload keys diverge and the Persistence dedup index misses. The per-call ResidencyPolicy.CodecVersion
    // still governs the vertex codec; the vertex global is pinned for symmetry so a default-version vertex path agrees.
    static Residency() {
        Meshopt.EncodeIndexVersion(IndexCodecVersion);
        Meshopt.EncodeVertexVersion(ResidencyPolicy.Canonical.CodecVersion);
    }

    public static Fin<ResidencyPayload> Encode(ResidencySource source, ResidencyPolicy policy) =>
        source.Switch(
            state: policy,
            leaf: static (p, l) => LeafEncode(l.Kind, l.Geometry, p),
            splat: static (p, s) => SplatEncode(s.Scan, p));

    public static ComputeReceipt.StreamSegment Receipt(ResidencyPayload payload, CorrelationId correlation, WorkLane lane, Duration elapsed) =>
        new(payload.ArtifactKey, payload.Clusters.IsEmpty ? payload.Layout.Count : payload.Clusters.Count, payload.EncodedBytes) {
            Correlation = correlation, Lane = lane, Substrate = Substrate.CpuTensor, AllocationClass = AllocationClass.PooledMemory, Elapsed = elapsed,
        };

    static Fin<ResidencyPayload> LeafEncode(ResidencyKind kind, ImportedGeometry leaf, ResidencyPolicy policy) =>
        kind.Switch(
            state: (Leaf: leaf, Policy: policy),
            meshletCluster: static (s, _) => MeshletEncode(s.Leaf, s.Policy),
            quantizedVertex: static (s, _) => QuantizedEncode(s.Leaf, s.Policy),
            pointSplat: static (s, _) => PointEncode(s.Leaf, s.Policy),
            gaussianSplat: static (s, _) => Fin.Fail<ResidencyPayload>(new ComputeFault.ModelRejected($"<residency-splat-needs-scan:{s.Leaf.FormatKey}>")));

    static Fin<ResidencyPayload> MeshletEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        var positions = leaf.Vertices.Span;
        var optimized = new uint[leaf.Indices.Length];
        Meshopt.OptimizeVertexCache(optimized, ToUInt(leaf.Indices.Span), (nuint)leaf.VertexCount);
        nuint maxMeshlets = Meshopt.BuildMeshletsBound((nuint)optimized.Length, (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles);
        var meshlets = new Meshlet[(int)maxMeshlets];
        var meshletVertices = new uint[(int)maxMeshlets * policy.MaxVertices];
        var meshletTriangles = new byte[(int)maxMeshlets * policy.MaxTriangles * 3];
        int count = BuildClusters(optimized, positions, leaf.VertexCount, policy, meshlets, meshletVertices, meshletTriangles);
        if (count == 0) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-meshlet-empty:{leaf.FormatKey}>")); }
        var clusters = new ResidencyMeshlet[count];
        for (int m = 0; m < count; m++) {
            ref readonly var meshlet = ref meshlets[m];
            var localVertices = meshletVertices.AsSpan((int)meshlet.vertex_offset, (int)meshlet.vertex_count);
            var localTriangles = meshletTriangles.AsSpan((int)meshlet.triangle_offset, (int)meshlet.triangle_count * 3);
            Meshopt.OptimizeMeshlet(localVertices, localTriangles, meshlet.triangle_count, meshlet.vertex_count);
            clusters[m] = Cluster(meshlet, ClusterBounds(localVertices, localTriangles, (int)meshlet.triangle_count, positions, leaf.VertexCount));
        }
        ref readonly var tail = ref meshlets[count - 1];
        int usedVertices = (int)(tail.vertex_offset + tail.vertex_count);
        int usedTriangleBytes = (int)(tail.triangle_offset + tail.triangle_count * 3);
        var streams = Seq(
            (ResidencyStream.Positions, StreamMode.Attributes, StreamFilter.None, leaf.VertexCount, PositionStride, EncodeVertices(positions, leaf.VertexCount, policy)),
            (ResidencyStream.Indices, StreamMode.Indices, StreamFilter.None, usedVertices, sizeof(uint), EncodeSequence(meshletVertices.AsSpan(0, usedVertices), leaf.VertexCount)),
            (ResidencyStream.Triangles, StreamMode.Raw, StreamFilter.None, usedTriangleBytes, 1, meshletTriangles.AsMemory(0, usedTriangleBytes)));
        if (HasNormals(leaf)) { streams = streams.Add((ResidencyStream.Normals, StreamMode.Attributes, StreamFilter.Octahedral, leaf.VertexCount, sizeof(uint), EncodeNormals(leaf.Normals.Span, leaf.VertexCount, policy))); }
        return Fin.Succ(Assemble(ResidencyKind.MeshletCluster, leaf.FormatKey, streams, clusters.ToSeq(), leaf.VertexCount, SphereBounds(positions, leaf.VertexCount), 0, policy));
    }

    static Fin<ResidencyPayload> QuantizedEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        if (policy.QuantizationBits is < 1 or > 24) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-quant-bits:{policy.QuantizationBits}>")); }
        var streams = Seq(
            (ResidencyStream.Positions, StreamMode.Attributes, StreamFilter.Exponential, leaf.VertexCount, PositionStride, EncodeExp(leaf.Vertices.Span, leaf.VertexCount, policy)),
            (ResidencyStream.Indices, StreamMode.Triangles, StreamFilter.None, leaf.Indices.Length, sizeof(uint), EncodeTriangles(ToUInt(leaf.Indices.Span), leaf.VertexCount)));
        if (HasNormals(leaf)) { streams = streams.Add((ResidencyStream.Normals, StreamMode.Attributes, StreamFilter.Octahedral, leaf.VertexCount, sizeof(uint), EncodeNormals(leaf.Normals.Span, leaf.VertexCount, policy))); }
        return Fin.Succ(Assemble(ResidencyKind.QuantizedVertex, leaf.FormatKey, streams, Seq<ResidencyMeshlet>(), leaf.VertexCount, SphereBounds(leaf.Vertices.Span, leaf.VertexCount), 0, policy));
    }

    static Fin<ResidencyPayload> PointEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        if (policy.SimplifyTarget is <= 0 or > 1) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-simplify-target:{policy.SimplifyTarget:R}>")); }
        int target = Math.Max(1, (int)(leaf.VertexCount * policy.SimplifyTarget));
        var remap = new uint[target];
        int kept = DecimatePoints(remap, leaf.Vertices.Span, leaf.VertexCount,
            HasNormals(leaf) ? leaf.Normals.Span : ReadOnlySpan<float>.Empty, policy.AttributeWeight, target);
        var gathered = new float[kept * 3];
        var source = leaf.Vertices.Span;
        for (int v = 0; v < kept; v++) { source.Slice((int)remap[v] * 3, 3).CopyTo(gathered.AsSpan(v * 3)); }
        var streams = Seq((ResidencyStream.Positions, StreamMode.Attributes, StreamFilter.Exponential, kept, PositionStride, EncodeExp(gathered, kept, policy)));
        return Fin.Succ(Assemble(ResidencyKind.PointSplat, leaf.FormatKey, streams, Seq<ResidencyMeshlet>(), kept, SphereBounds(gathered, kept), 0, policy));
    }

    static Fin<ResidencyPayload> SplatEncode(SplatScan scan, ResidencyPolicy policy) {
        if (policy.QuantizationBits is < 1 or > 16) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-splat-bits:{policy.QuantizationBits}>")); }
        int n = (int)scan.SplatCount;
        int shFloats = (scan.HarmonicDegree + 1) * (scan.HarmonicDegree + 1) * 3;
        var rotations = new ulong[n];
        Meshopt.EncodeFilterQuat<ulong>(rotations, policy.QuantizationBits, scan.Rotations.Span[..(n * 4)]);
        var harmonics = new uint[n * shFloats];
        Meshopt.EncodeFilterExp<uint>(harmonics, policy.QuantizationBits, scan.Harmonics.Span[..(n * shFloats)], EncodeExpMode.EncodeExpSeparate);
        var streams = Seq(
            (ResidencyStream.Positions, StreamMode.Attributes, StreamFilter.Exponential, n, PositionStride, EncodeExp(scan.Positions.Span, n, policy)),
            (ResidencyStream.Scales, StreamMode.Attributes, StreamFilter.Exponential, n, PositionStride, EncodeExp(scan.Scales.Span, n, policy)),
            (ResidencyStream.Rotations, StreamMode.Attributes, StreamFilter.Quaternion, n, sizeof(ulong), EncodeStream<ulong>(rotations, policy.CodecLevel, policy.CodecVersion)),
            (ResidencyStream.Harmonics, StreamMode.Attributes, StreamFilter.Exponential, n * shFloats, sizeof(uint), EncodeStream<uint>(harmonics, policy.CodecLevel, policy.CodecVersion)));
        return Fin.Succ(Assemble(ResidencyKind.GaussianSplat, scan.FormatKey, streams, Seq<ResidencyMeshlet>(), n, SphereBounds(scan.Positions.Span, n), scan.HarmonicDegree, policy));
    }

    static ResidencyPayload Assemble(ResidencyKind kind, string formatKey,
        Seq<(ResidencyStream Stream, StreamMode Mode, StreamFilter Filter, int Count, int ByteStride, ReadOnlyMemory<byte> Bytes)> streams,
        Seq<ResidencyMeshlet> clusters, int residentCount, (Vector3 Center, float Radius) bounds, int harmonicDegree, ResidencyPolicy policy) {
        var blob = new byte[streams.Sum(static stream => stream.Bytes.Length)];
        var layout = new Dictionary<ResidencyStream, StreamSpan>(streams.Count);
        int cursor = 0;
        foreach (var (stream, mode, filter, count, byteStride, bytes) in streams) {
            bytes.Span.CopyTo(blob.AsSpan(cursor));
            layout[stream] = new StreamSpan(cursor, bytes.Length, count, byteStride, mode, filter);
            cursor += bytes.Length;
        }
        var key = InterchangeIdentity.Key($"{formatKey}:{kind.Key}", blob, policy.ConeWeight, policy.SimplifyTarget, policy.QuantizationBits);
        return new ResidencyPayload(kind, key, blob, layout.ToFrozenDictionary(), clusters, residentCount, bounds.Center, bounds.Radius, harmonicDegree);
    }

    static ReadOnlyMemory<byte> EncodeVertices(ReadOnlySpan<float> positions, int count, ResidencyPolicy policy) =>
        EncodeStream(MemoryMarshal.Cast<float, Vector3>(positions[..(count * 3)]), policy.CodecLevel, policy.CodecVersion);

    static ReadOnlyMemory<byte> EncodeExp(ReadOnlySpan<float> floats, int count, ResidencyPolicy policy) {
        var packed = new Packed12[count];
        Meshopt.EncodeFilterExp<Packed12>(packed, policy.QuantizationBits, floats[..(count * 3)], EncodeExpMode.EncodeExpSharedComponent);
        return EncodeStream<Packed12>(packed, policy.CodecLevel, policy.CodecVersion);
    }

    static ReadOnlyMemory<byte> EncodeNormals(ReadOnlySpan<float> normals, int count, ResidencyPolicy policy) {
        var quad = new float[count * 4];
        for (int v = 0; v < count; v++) { normals.Slice(v * 3, 3).CopyTo(quad.AsSpan(v * 4)); }
        var packed = new uint[count];
        Meshopt.EncodeFilterOct<uint>(packed, OctBits, quad);
        return EncodeStream<uint>(packed, policy.CodecLevel, policy.CodecVersion);
    }

    static ReadOnlyMemory<byte> EncodeStream<T>(ReadOnlySpan<T> packed, int level, int version) where T : unmanaged {
        var buffer = new byte[(int)Meshopt.EncodeVertexBufferBound((nuint)packed.Length, (nuint)Unsafe.SizeOf<T>())];
        return buffer.AsMemory(0, (int)Meshopt.EncodeVertexBufferLevel<T>(buffer, packed, level, version));
    }

    static ReadOnlyMemory<byte> EncodeTriangles(ReadOnlySpan<uint> indices, int vertexCount) {
        var buffer = new byte[(int)Meshopt.EncodeIndexBufferBound((nuint)indices.Length, (nuint)vertexCount)];
        return buffer.AsMemory(0, (int)Meshopt.EncodeIndexBuffer(buffer, indices));
    }

    static ReadOnlyMemory<byte> EncodeSequence(Span<uint> sequence, int vertexCount) {
        var buffer = new byte[(int)Meshopt.EncodeIndexSequenceBound((nuint)sequence.Length, (nuint)vertexCount)];
        return buffer.AsMemory(0, (int)Meshopt.EncodeIndexSequence(buffer, sequence));
    }

    static uint[] ToUInt(ReadOnlySpan<long> indices) {
        var converted = new uint[indices.Length];
        for (int i = 0; i < indices.Length; i++) { converted[i] = (uint)indices[i]; }
        return converted;
    }

    static bool HasNormals(ImportedGeometry leaf) => leaf.Normals.Length >= leaf.VertexCount * 3;

    static ResidencyMeshlet Cluster(in Meshlet meshlet, Bounds bounds) {
        var f = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Bounds, float>(ref bounds), 11);
        return new ResidencyMeshlet(
            (int)meshlet.vertex_offset, (int)meshlet.triangle_offset, (int)meshlet.vertex_count, (int)meshlet.triangle_count,
            new Vector3(f[0], f[1], f[2]), f[3], new Vector3(f[4], f[5], f[6]), new Vector3(f[7], f[8], f[9]), f[10]);
    }

    // explicit-pointer boundary kernels: the safe span overloads pass the element-span length as the
    // semantic vertex/triangle/point count (wrong for interleaved-float positions and 3-byte triangles), so
    // these four count-bearing builds (BuildClusters/ClusterBounds/SphereBounds/DecimatePoints) pin and pass the true counts
    static unsafe int BuildClusters(ReadOnlySpan<uint> indices, ReadOnlySpan<float> positions, int vertexCount, ResidencyPolicy policy,
        Meshlet[] meshlets, uint[] meshletVertices, byte[] meshletTriangles) {
        fixed (Meshlet* meshlet = meshlets)
        fixed (uint* vertices = meshletVertices)
        fixed (byte* triangles = meshletTriangles)
        fixed (uint* index = indices)
        fixed (float* position = positions) {
            return (int)Meshopt.BuildMeshlets(meshlet, vertices, triangles, index, (nuint)indices.Length, position, (nuint)vertexCount,
                (nuint)PositionStride, (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles, policy.ConeWeight);
        }
    }

    static unsafe Bounds ClusterBounds(ReadOnlySpan<uint> meshletVertices, ReadOnlySpan<byte> meshletTriangles, int triangleCount, ReadOnlySpan<float> positions, int vertexCount) {
        fixed (uint* vertices = meshletVertices)
        fixed (byte* triangles = meshletTriangles)
        fixed (float* position = positions) {
            return Meshopt.ComputeMeshletBounds(vertices, triangles, (nuint)triangleCount, position, (nuint)vertexCount, (nuint)PositionStride);
        }
    }

    // tile-level bounding sphere over the leaf/scan positions so the ResidencyPayload is self-describing for the
    // AppUi manifest (frustum cull + placement) — reads the center[3]+radius prefix of the native Bounds
    static unsafe (Vector3 Center, float Radius) SphereBounds(ReadOnlySpan<float> positions, int count) {
        fixed (float* position = positions) {
            var bounds = Meshopt.ComputeSphereBounds(position, (nuint)count, (nuint)PositionStride, null, 0);
            var f = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Bounds, float>(ref bounds), 4);
            return (new Vector3(f[0], f[1], f[2]), f[3]);
        }
    }

    static unsafe int DecimatePoints(uint[] remap, ReadOnlySpan<float> positions, int vertexCount, ReadOnlySpan<float> attributes, double weight, int target) {
        fixed (uint* destination = remap)
        fixed (float* position = positions)
        fixed (float* attribute = attributes) {
            return (int)Meshopt.SimplifyPoints(destination, position, (nuint)vertexCount, (nuint)PositionStride,
                attributes.IsEmpty ? null : attribute, attributes.IsEmpty ? 0 : (nuint)PositionStride, (float)weight, (nuint)target);
        }
    }
}
```

## [03]-[RESEARCH]

- [SPLAT_COMPANION_WIRE]: the SPZ/SOG gaussian-splat scan decode is owned by the Python `realitycapture` companion and crosses the `Runtime/channels#PROTO_VOCABULARY` `ArtifactSync` `ArtifactFrame` byte seam as the `Runtime/channels` `GaussianSplatScan` artifact message (the wire-admission boundary `channels` owns) — the companion-published already-decoded scan-buffer layout (position/scale/rotation/spherical-harmonic float-buffer order, the harmonic-degree band, and the per-splat counts) grounds BOTH that `GaussianSplatScan` wire message AND this page's `SplatScan` member set, reassembled and admitted to a `SplatScan` at that wire seam, NOT in this page (the `GeometryPayload` oneof carries point-cloud/mesh/voxel only, so the scan rides the generic `ArtifactFrame` bytes as that standalone artifact, never a `GeometryPayload` oneof case). The `GaussianSplatScan` admission stays honestly [UPSTREAM-BLOCKED] in `channels` until the companion grounds the SPZ/SOG decode wire and publishes the scan-buffer frame. This page's `gaussian-splat` arm is fully implemented over whatever `SplatScan` it is handed — exponent-filtering positions/scales/harmonics and quaternion-filtering rotations — so the only open leaf is the companion publishing the scan-buffer frame and the Python `:x32` content-key reproduction gated on the `xxhash` cp315/abi3 wheel the companion lacks below 3.15, the same `[UPSTREAM-BLOCKED]` leaf `csharp:Rasm.AppUi/Render/reality#SPLAT_SOURCE` carries.

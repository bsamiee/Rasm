# [COMPUTE_RESIDENCY]

Rasm.Compute streaming-residency lane: the content-keyed GPU-ready payload codec a web viewer streams cell-by-cell. Four encode arms ride one `ResidencyKind` axis — meshlet-cluster partitions an octree-leaf `ImportedGeometry` into cone-cullable clusters, quantized-vertex exponent-filters and level-compresses a leaf for a low-VRAM tile, point-splat decimates a reality-capture point set, and gaussian-splat octahedral/quaternion/exponent-filters a companion-decoded `SplatScan`. One `Encode` fold over the safe `Meshopt` span surface owns every arm, so a per-kind encoder sibling is the collapsed form. This lane produces payload bytes and the self-describing `StreamSpan` bufferView layout only, never a manifest or a scene-graph.

Payload bytes address through the suite `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` key, read the `Runtime/codecs#TILE_PARTITION` `ImportedGeometry` octree leaf (never a second partition), and ride the `Runtime/receipts#RECEIPT_UNION` `StreamSegment` slot (never a new receipt case). `csharp:Rasm.AppUi/Render/pipeline#TS_PROJECTION` `ResidencyManifest.Mint` mints the `WEB_GEOMETRY_RESIDENCY_WIRE` manifest once, projecting each payload 1:1 from its `StreamSpan` layout, `ResidencyMeshlet` clusters, and content key — a Compute-side `ResidencyManifest` is the named drift defect. Encoded blobs land content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` at the app-platform seam. Splat scans arrive from the Python `realitycapture` companion as `ArtifactFrame` bytes at the `Runtime/wire#PROTO_VOCABULARY` `ArtifactSync` seam, never an in-process splat fit or SPZ/SOG decoder. HOST-LOCAL, no TS_PROJECTION.

## [01]-[INDEX]

- [01]-[RESIDENCY]: the `ResidencyKind` payload axis, the `ResidencyStream`/`StreamMode`/`StreamFilter` buffer-role, decode-mode, and filter axes, the `ResidencySource` encode input, the `ResidencyPayload` carrier, and the `Encode` fold over the safe `Meshopt` span surface.

## [02]-[RESIDENCY]

- Owner: `ResidencyKind` `[SmartEnum<string>]` the one closed payload axis, each row's `ConeCullable`/`SplatBorne` columns telling the AppUi marshal which cull and shader to pick, so a new encoding is one row, never a per-kind payload type; `ResidencyStream`, `StreamMode`, `StreamFilter` the closed buffer-role, meshopt decode-mode, and attribute-filter axes whose keys ARE the `EXT_meshopt_compression` wire modes the manifest emits; `ResidencySource` `[Union]` the polymorphic encode input (`Leaf` for octree-leaf arms, `Splat` for a companion scan), so one entry discriminates on shape, never an `Encode`/`EncodeSplat` pair; `ResidencyMeshlet` the per-cluster cone-and-sphere descriptor carrying the cluster-LOD chain columns `Level`/`Parent`/`Error`/`ParentError`; `ResidencyPolicy` the encode-posture record; `ResidencyPayload` the content-keyed buffer carrier (blob, per-stream `StreamSpan` layout, clusters, bounding sphere, content key), not a manifest; `Residency` the static `Encode` fold plus the `StreamSegment` `Receipt` projection.
- Cases: `ResidencyKind` rows `meshlet-cluster` (cone-cullable cluster-LOD chain — global vertex stream, `EncodeIndexSequence` meshlet-vertex table, raw local triangle bytes, per-cluster descriptors across the `Meshopt.Simplify` levels `SimplifyTarget` drives) · `quantized-vertex` (exponent-filtered, level-compressed single tile) · `point-splat` (`SimplifyPoints`-decimated, exponent-filtered positions) · `gaussian-splat` (companion-decoded `SplatScan` — positions/scales/harmonics exponent-filter, rotation quaternions quaternion-filter).
- Entry: `public static Fin<ResidencyPayload> Encode(ResidencySource source, ResidencyPolicy policy)` projects a leaf (or companion scan) onto the kind's arm; `public static ComputeReceipt.StreamSegment Receipt(ResidencyPayload payload, CorrelationId correlation, WorkLane lane, Duration elapsed)` projects onto the settled slot; `Fin<T>` aborts onto `ComputeFault.PayloadOverBounds` for an empty meshlet build, an out-of-range quantization budget, or an out-of-range simplify target, and onto `ComputeFault.ModelRejected` for a leaf routed at a splat-borne kind.
- Auto: `Encode` dispatches the `ResidencySource` union through one state-threaded total `Switch`, the `Leaf` arm sub-dispatching `ResidencyKind` through a second total `Switch`; the meshlet arm cache-optimizes the leaf index buffer through `Meshopt.OptimizeVertexCache` (topology-preserving — never a vertex remap that strands the index buffer), builds cone-cullable clusters, encodes the global vertex stream and the local→global meshlet-vertex table as an `EncodeIndexSequence` stream, and carries the local triangle bytes raw so a mesh-shader consumer reconstructs each meshlet from the `ResidencyMeshlet` offsets; the quantized/point/splat arms exponent-filter positions (point-splat decimates first through `Meshopt.SimplifyPoints`, normal-weighted so feature edges survive; splat quaternion-filters rotations), every arm octahedral-filters present normals, assembles one blob, and keys it through `InterchangeIdentity.Key` over the whole blob folded with the kind tag and lossy posture; `EncodeIndexBuffer`/`EncodeIndexSequence` carry no per-call version, so the one-time `Residency` type-init `Meshopt.EncodeIndexVersion` pin makes the index/sequence streams byte-reproducible for identical geometry across processes.
- Receipt: the `Runtime/receipts#RECEIPT_UNION` `StreamSegment(string ArtifactId, int Segments, long Bytes)` slot carries the payload `ArtifactKey`, the cluster count (meshlet) or stream count (other kinds), and the blob length — a re-encode of identical geometry at identical policy stamps the same content key, so emission is auditable through the existing slot, never a new case; the blob dedups on the Persistence blob lane through `ArtifactIndexRow.Admit` and a hit stamps a `Cache` receipt.
- Packages: Alimer.Bindings.MeshOptimizer, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new encoding is one `ResidencyKind` row with its `Switch` case; a new attribute is one `ResidencyStream` row plus its filtered-stream line; a new filter or decode mode is one `StreamFilter`/`StreamMode` row on the `StreamSpan`; a new posture is one `ResidencyPolicy` column; a new source modality is one `ResidencySource` case; zero new surface — a `MeshletResidencyEncoder`/`SplatPayloadCodec`/`QuantizedVertexEncoder` sibling collapses onto the one `Encode` fold, and parallel `EncodedVertices`/`EncodedIndices`/`EncodedMeshlets` byte fields collapse onto the one `StreamSpan` layout.
- Boundary: this lane owns ONLY the content-keyed payload blob plus the `StreamSpan` layout — `csharp:Rasm.AppUi/Render/pipeline#TS_PROJECTION` `ResidencyManifest.Mint` projects each resident payload 1:1 into an `EXT_meshopt_compression` bufferView wire row from its `StreamSpan`, clusters, sphere, harmonic degree, and `ContentKey`, so a Compute-side `ResidencyManifest`, a scene-graph, a second content-identity value object, or a second hash off raw positions is the named drift defect (the `:x32` key IS the `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128.HashToUInt128` value); leaf geometry reads from the `Runtime/codecs#TILE_PARTITION` `TileSet` octree leaf `ImportedGeometry`, so a second octree partition here is the rejected form, and the content key composes `InterchangeIdentity.Key` over the WHOLE assembled blob (every stream, not the vertex stream alone) folded with the kind tag and lossy posture so two encodes differing in any byte-changing policy key distinctly; the process-global index/sequence codec version pins once at `Residency` type init (`EncodeIndexBuffer`/`EncodeIndexSequence` carry no per-call version arg) so identical geometry produces a byte-identical blob across processes while the vertex codec rides the per-call `ResidencyPolicy.CodecVersion`; `meshoptimizer` owns every meshlet build, cluster bounds, cache optimization, codec, filter, and point decimation — its safe `Span<T>` overloads own the codec/filter/cache-optimize/index legs directly while the four count-bearing builds (`BuildMeshlets`/`ComputeMeshletBounds`/`ComputeSphereBounds`/`SimplifyPoints`) take the explicit pointer overload because the convenience wrapper passes the element-span length where the native wants the true vertex/triangle/point count, so the pinned surface collapses to those four kernels plus the `Unsafe.As<Bounds, float>` prefix reinterpret and a hand-rolled partitioner, vertex codec, or point simplifier is the deleted form; meshlet residency keeps the cluster structure intact — global vertex stream once, local→global `meshlet_vertices` as an `EncodeIndexSequence` stream, local `meshlet_triangles` raw under the `ResidencyMeshlet` offsets — so an `EncodeIndexBuffer` over the local triangle bytes (NOT triangle-list indices into the vertex buffer) is the deleted correctness defect; gaussian-splat homes a splat payload but admits no splat fit and no in-process SPZ/SOG decoder — the Python `realitycapture` companion decodes the scan, which crosses the `Runtime/wire#PROTO_VOCABULARY` `ArtifactSync` byte seam as the standalone `GaussianSplatScan` artifact (the `GeometryPayload` oneof carries point-cloud/mesh/voxel only) and is admitted as a `SplatScan` upstream, the rejected form the `csharp:Rasm.AppUi/Render/reality#SPLAT_SOURCE` arm marks `[UPSTREAM-BLOCKED]`; point-cloud-FILE readers (E57/LAS/LAZ/PTS) are the distinct `Runtime/codecs#FIELD_RESULT_CODEC` `point-catalogue-pending` concern this lane neither supersedes nor admits, and the assembled blob is the single content-keyed payload window so a `ToArray` flatten past it is the named defect.

```csharp signature
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

// Cluster-LOD chain columns: Error is object-space simplification error (level 0 = 0); ParentError raised to at
// least max(children) — MONOTONIC (ParentError >= Error) — so a screen-space cut (finest level whose
// Error <= t < ParentError) is crack-free and double-draw-free. AppUi reads these and never re-clusters.
public readonly record struct ResidencyMeshlet(
    int VertexOffset,
    int TriangleOffset,
    int VertexCount,
    int TriangleCount,
    Vector3 Center,
    float Radius,
    Vector3 ConeApex,
    Vector3 ConeAxis,
    float ConeCutoff,
    int Level,
    int Parent,
    float Error,
    float ParentError);

// per-stream EXT_meshopt_compression bufferView: byte window, Count/ByteStride, decode Mode (attribute/triangle/
// index codec, or Raw for un-encoded meshlet triangle bytes), inverse Filter — the set the AppUi manifest emits
public readonly record struct StreamSpan(int Offset, int Length, int Count, int ByteStride, StreamMode Mode, StreamFilter Filter);

// exp-packed 3-component carrier (12 bytes) the meshopt exponent filter writes; never read back as floats here
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

    // EncodeIndexBuffer/EncodeIndexSequence carry NO per-call version arg (unlike EncodeVertexBufferLevel), so the
    // meshlet vertex-table + triangle-index streams follow the process-global EncodeIndexVersion — pinned here at
    // type init (before the first Encode) so identical geometry keys identically across processes, else the
    // Persistence dedup index misses. Per-call ResidencyPolicy.CodecVersion governs the vertex codec; global pinned for symmetry.
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
        var clusters = new List<ResidencyMeshlet>(count);
        for (int m = 0; m < count; m++) {
            ref readonly var meshlet = ref meshlets[m];
            var localVertices = meshletVertices.AsSpan((int)meshlet.vertex_offset, (int)meshlet.vertex_count);
            var localTriangles = meshletTriangles.AsSpan((int)meshlet.triangle_offset, (int)meshlet.triangle_count * 3);
            Meshopt.OptimizeMeshlet(localVertices, localTriangles, meshlet.triangle_count, meshlet.vertex_count);
            clusters.Add(Cluster(meshlet, ClusterBounds(localVertices, localTriangles, (int)meshlet.triangle_count, positions, leaf.VertexCount), level: 0, error: 0f));
        }
        ref readonly var tail = ref meshlets[count - 1];
        int usedVertices = (int)(tail.vertex_offset + tail.vertex_count);
        int usedTriangleBytes = (int)(tail.triangle_offset + tail.triangle_count * 3);
        var chained = LodChain(optimized, positions, leaf.VertexCount, policy, clusters);
        var streams = Seq(
            (ResidencyStream.Positions, StreamMode.Attributes, StreamFilter.None, leaf.VertexCount, PositionStride, EncodeVertices(positions, leaf.VertexCount, policy)),
            (ResidencyStream.Indices, StreamMode.Indices, StreamFilter.None, usedVertices, sizeof(uint), EncodeSequence(meshletVertices.AsSpan(0, usedVertices), leaf.VertexCount)),
            (ResidencyStream.Triangles, StreamMode.Raw, StreamFilter.None, usedTriangleBytes, 1, meshletTriangles.AsMemory(0, usedTriangleBytes)));
        if (HasNormals(leaf)) { streams = streams.Add((ResidencyStream.Normals, StreamMode.Attributes, StreamFilter.Octahedral, leaf.VertexCount, sizeof(uint), EncodeNormals(leaf.Normals.Span, leaf.VertexCount, policy))); }
        return Fin.Succ(Assemble(ResidencyKind.MeshletCluster, leaf.FormatKey, streams, chained, leaf.VertexCount, SphereBounds(positions, leaf.VertexCount), 0, policy));
    }

    // Each coarser level simplifies the prior level's index buffer through the Meshopt.Simplify ladder (result_error
    // scaled to object space by SimplifyScale), re-clusters, and links each fine cluster to the coarse parent whose
    // sphere CONTAINS it, falling back to nearest center. Monotonic guarantee at link time: a parent's Error rises to
    // at least max(children) before children stamp ParentError, so a screen-space cut is crack-free and
    // double-draw-free. Ladder terminates when a level stops shrinking or one meshlet remains; roots carry
    // Parent = -1, ParentError = +inf.
    static Seq<ResidencyMeshlet> LodChain(uint[] indices, ReadOnlySpan<float> positions, int vertexCount, ResidencyPolicy policy, List<ResidencyMeshlet> level0) {
        var all = new List<ResidencyMeshlet>(level0);
        float scale = Meshopt.SimplifyScale(positions, (nuint)vertexCount, PositionStride);
        uint[] current = indices;
        int level = 0, firstOfLevel = 0, countOfLevel = level0.Count;
        while (countOfLevel > 1) {
            var simplified = new uint[current.Length];
            nuint target = (nuint)Math.Max(3, (long)(current.Length * policy.SimplifyTarget) / 3 * 3);
            nuint written = Meshopt.Simplify(simplified, current, positions, (nuint)vertexCount, PositionStride, target, targetError: float.MaxValue, options: 0, out float resultError);
            if (written >= (nuint)current.Length || written < 3) { break; }
            Array.Resize(ref simplified, (int)written);
            level++;
            float objectError = resultError * scale;
            var (coarse, coarseFirst) = ClusterLevel(simplified, positions, vertexCount, policy, all, level, objectError);
            if (coarse == 0) { break; }
            Link(all, firstOfLevel, countOfLevel, coarseFirst, coarse);
            firstOfLevel = coarseFirst; countOfLevel = coarse; current = simplified;
        }
        return all.ToSeq();
    }

    // Each fine cluster binds the nearest-center coarse cluster whose sphere CONTAINS it (d + fineRadius <=
    // coarseRadius), else nearest center, so a child never binds outside its parent's coverage; the parent's Error
    // raises to max(parent, children) and children re-stamp ParentError from the raised value.
    static void Link(List<ResidencyMeshlet> all, int fineFirst, int fineCount, int coarseFirst, int coarseCount) {
        for (int f = fineFirst; f < fineFirst + fineCount; f++) {
            int nearest = coarseFirst; float nearestDistance = float.MaxValue;
            int covering = -1; float coveringDistance = float.MaxValue;
            for (int c = coarseFirst; c < coarseFirst + coarseCount; c++) {
                float d = Vector3.Distance(all[f].Center, all[c].Center);
                if (d < nearestDistance) { nearestDistance = d; nearest = c; }
                if (d + all[f].Radius <= all[c].Radius && d < coveringDistance) { coveringDistance = d; covering = c; }
            }
            int best = covering >= 0 ? covering : nearest;
            all[best] = all[best] with { Error = Math.Max(all[best].Error, all[f].Error) };
            all[f] = all[f] with { Parent = best };
        }
        for (int f = fineFirst; f < fineFirst + fineCount; f++) {
            if (all[f].Parent >= 0) { all[f] = all[f] with { ParentError = Math.Max(all[all[f].Parent].Error, all[f].Error) }; }
        }
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

    static ResidencyMeshlet Cluster(in Meshlet meshlet, Bounds bounds, int level, float error) {
        var f = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Bounds, float>(ref bounds), 11);
        return new ResidencyMeshlet(
            (int)meshlet.vertex_offset, (int)meshlet.triangle_offset, (int)meshlet.vertex_count, (int)meshlet.triangle_count,
            new Vector3(f[0], f[1], f[2]), f[3], new Vector3(f[4], f[5], f[6]), new Vector3(f[7], f[8], f[9]), f[10],
            Level: level, Parent: -1, Error: error, ParentError: float.PositiveInfinity);
    }

    // One coarser level clustered through the SAME BuildClusters kernel; returns (count, firstIndex).
    static (int Count, int First) ClusterLevel(uint[] simplified, ReadOnlySpan<float> positions, int vertexCount, ResidencyPolicy policy, List<ResidencyMeshlet> all, int level, float objectError) {
        nuint bound = Meshopt.BuildMeshletsBound((nuint)simplified.Length, (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles);
        var meshlets = new Meshlet[(int)bound];
        var vertices = new uint[(int)bound * policy.MaxVertices];
        var triangles = new byte[(int)bound * policy.MaxTriangles * 3];
        int first = all.Count;
        int count = BuildClusters(simplified, positions, vertexCount, policy, meshlets, vertices, triangles);
        for (int m = 0; m < count; m++) {
            ref readonly var meshlet = ref meshlets[m];
            var localVertices = vertices.AsSpan((int)meshlet.vertex_offset, (int)meshlet.vertex_count);
            var localTriangles = triangles.AsSpan((int)meshlet.triangle_offset, (int)meshlet.triangle_count * 3);
            all.Add(Cluster(meshlet, ClusterBounds(localVertices, localTriangles, (int)meshlet.triangle_count, positions, vertexCount), level, objectError));
        }
        return (count, first);
    }

    // The safe span overloads pass element-span length as the semantic vertex/triangle/point count (wrong for
    // interleaved-float positions and 3-byte triangles), so these four count-bearing builds pin and pass true counts
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

    // tile bounding sphere over leaf/scan positions so ResidencyPayload is self-describing for the AppUi manifest
    // (frustum cull + placement) — reads the center[3]+radius prefix of the native Bounds
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [SPLAT_COMPANION_WIRE]-[BLOCKED]: the packed position/scale/rotation/spherical-harmonic buffer order and harmonic-degree band the Python `realitycapture` companion SPZ v4 / SOG v2 decode emits — byte-mirroring this page's `SplatScan` member set (`FormatKey`/`Positions`/`Scales`/`Rotations`/`Harmonics`/`HarmonicDegree`/`SplatCount`) across the `Runtime/wire#PROTO_VOCABULARY` `GaussianSplatScan` frame; the SPZ v4 / SOG v2 published specifications and the companion decode, blocked until that companion leg lands (the render-side consumer riding `csharp:Rasm.AppUi/Render/reality#SPLAT_SOURCE` `[UPSTREAM-BLOCKED]`).

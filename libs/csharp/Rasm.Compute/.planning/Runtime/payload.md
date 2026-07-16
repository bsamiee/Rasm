# [COMPUTE_RESIDENCY]

Rasm.Compute streaming-residency lane: the content-keyed GPU-ready payload codec a web viewer streams cell-by-cell. Four encode arms ride one `ResidencyKind` axis — meshlet-cluster partitions an octree-leaf `ImportedGeometry` into cone-cullable clusters, quantized-vertex exponent-filters and level-compresses a leaf for a low-VRAM tile, point-splat decimates a reality-capture point set, and gaussian-splat octahedral/quaternion/exponent-filters a companion-decoded `SplatScan`. One `Encode` fold over the safe `Meshopt` span surface owns every arm, so a per-kind encoder sibling is the collapsed form. This lane produces payload bytes and the self-describing `StreamSpan` bufferView layout only, never a manifest or a scene-graph.

Payload bytes address through the suite `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` key, read the `Runtime/codecs#TILE_PARTITION` `ImportedGeometry` octree leaf (never a second partition), and ride the `Runtime/receipts#RECEIPT_UNION` `StreamSegment` slot (never a new receipt case). `csharp:Rasm.AppUi/Render/pipeline#TS_PROJECTION` `ResidencyManifest.Mint` mints the `WEB_GEOMETRY_RESIDENCY_WIRE` manifest once, projecting each payload 1:1 from its `StreamSpan` layout, `ResidencyMeshlet` clusters, and content key — a Compute-side `ResidencyManifest` is the named drift defect. Encoded blobs land content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` at the app-platform seam. Splat scans arrive from the Python `realitycapture` companion as `ArtifactFrame` bytes at the `Runtime/wire#PROTO_VOCABULARY` `ArtifactSync` seam, never an in-process splat fit or SPZ/SOG decoder. HOST-LOCAL, no TS_PROJECTION.

## [01]-[INDEX]

- [01]-[RESIDENCY]: the `ResidencyKind` payload axis, the `ResidencyStream`/`StreamMode`/`StreamFilter` buffer-role, decode-mode, and filter axes, the `ResidencySource` encode input, the `ResidencyPayload` carrier, and the `Encode` fold over the safe `Meshopt` span surface.

## [02]-[RESIDENCY]

- Owner: `ResidencyKind` `[SmartEnum<string>]` the one closed payload axis, each row's `ConeCullable`/`SplatBorne` columns telling the AppUi marshal which cull and shader to pick, so a new encoding is one row, never a per-kind payload type; `ResidencyStream`, `StreamMode`, `StreamFilter` the closed buffer-role, meshopt decode-mode, and attribute-filter axes whose keys ARE the `EXT_meshopt_compression` wire modes the manifest emits; `ResidencySource` `[Union]` the polymorphic encode input (`Leaf` for octree-leaf arms, `Splat` for a companion scan), so one entry discriminates on shape, never an `Encode`/`EncodeSplat` pair; `ResidencyMeshlet` the per-cluster cone-and-sphere descriptor carrying the cluster-LOD chain columns `Level`/`Parent`/`Error`/`ParentError`; `ResidencyPolicy` the encode-posture record; `ResidencyPayload` the content-keyed buffer carrier (blob, per-stream `StreamSpan` layout, clusters, bounding sphere, content key), not a manifest; `Residency` the static `Encode` fold plus the `StreamSegment` `Receipt` projection.
- Cases: `ResidencyKind` rows `meshlet-cluster` (cone-cullable cluster-LOD chain — global vertex stream, `EncodeIndexSequence` meshlet-vertex table, raw local triangle bytes, per-cluster descriptors across the `Meshopt.Simplify` levels `SimplifyTarget` drives) · `quantized-vertex` (exponent-filtered, level-compressed single tile) · `point-splat` (`SimplifyPoints`-decimated, exponent-filtered positions) · `gaussian-splat` (companion-decoded `SplatScan` — positions/scales/harmonics exponent-filter, rotation quaternions quaternion-filter).
- Entry: `public static Fin<ResidencyPayload> Encode(ResidencySource source, ResidencyPolicy policy)` projects a leaf (or companion scan) onto the kind's arm; `public static ComputeReceipt.StreamSegment Receipt(ResidencyPayload payload, CorrelationId correlation, WorkLane lane, Duration elapsed)` projects onto the settled slot; `Fin<T>` aborts onto `ComputeFault.PayloadOverBounds` for an empty meshlet build, an out-of-range quantization budget, or an out-of-range simplify target, and onto `ComputeFault.ModelRejected` for a leaf routed at a splat-borne kind.
- Auto: `Encode` admits every policy and source extent before dispatching the `ResidencySource` union; the `Leaf` arm reads the kind's row-owned `LeafArm` `[UseDelegateFromConstructor]` column, so the joint source-kind decision has one dispatch level. Meshlet encoding clusters through the `ClusterBuild` row (`cone` = `BuildMeshlets`, `flex` = `BuildMeshletsFlex`, `spatial` = `BuildMeshletsSpatial`), cache-optimizes the index buffer, and encodes the global vertices plus local-to-global meshlet indices while retaining raw local triangle bytes. Quantized, point, and splat arms filter their admitted attributes, and every stream carries its exact codec version through `StreamSpan.CodecVersion` before the whole blob keys through `InterchangeIdentity.Key`.
- Receipt: the `Runtime/receipts#RECEIPT_UNION` `StreamSegment(string ArtifactId, int Segments, long Bytes)` slot carries the payload `ArtifactKey`, the cluster count (meshlet) or stream count (other kinds), and the blob length — a re-encode of identical geometry at identical policy stamps the same content key, so emission is auditable through the existing slot, never a new case; the blob dedups on the Persistence blob lane through `ArtifactIndexRow.Admit` and a hit stamps a `Cache` receipt.
- Packages: Alimer.Bindings.MeshOptimizer, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new encoding is one `ResidencyKind` row carrying its `LeafArm` delegate column; a new meshlet-build strategy is one `ClusterBuild` row resolved inside the one pinned kernel; a new attribute is one `ResidencyStream` row plus its filtered-stream line; a new filter or decode mode is one `StreamFilter`/`StreamMode` row on the `StreamSpan`; a new posture is one `ResidencyPolicy` column; a new source modality is one `ResidencySource` case; zero new surface — a `MeshletResidencyEncoder`/`SplatPayloadCodec`/`QuantizedVertexEncoder` sibling collapses onto the one `Encode` fold, and parallel `EncodedVertices`/`EncodedIndices`/`EncodedMeshlets` byte fields collapse onto the one `StreamSpan` layout.
- Boundary: this lane owns the content-keyed payload blob plus `StreamSpan`; `csharp:Rasm.AppUi/Render/pipeline#TS_PROJECTION` projects every byte window, codec mode, inverse filter, codec version, cluster, bound, and content key without re-derivation. `InterchangeIdentity.Key` covers the whole assembled blob and its byte-changing policy. Process-global index encoding pins through `EncodeIndexVersion`, vertex encoding carries `ResidencyPolicy.CodecVersion`, and raw meshlet triangles carry version `0`. Count-bearing native calls receive explicit semantic counts through pinned pointer kernels. Gaussian splat fitting and SPZ/SOG decoding remain companion-owned; point-cloud file readers remain the distinct `Runtime/codecs#FIELD_RESULT_CODEC` concern.

```csharp signature
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ResidencyKind {
    public static readonly ResidencyKind MeshletCluster = new("meshlet-cluster", coneCullable: true, splatBorne: false, Residency.MeshletEncode);
    public static readonly ResidencyKind QuantizedVertex = new("quantized-vertex", coneCullable: false, splatBorne: false, Residency.QuantizedEncode);
    public static readonly ResidencyKind PointSplat = new("point-splat", coneCullable: false, splatBorne: false, Residency.PointEncode);
    public static readonly ResidencyKind GaussianSplat = new("gaussian-splat", coneCullable: false, splatBorne: true, Residency.SplatBorneLeafRejected);

    public bool ConeCullable { get; }
    public bool SplatBorne { get; }

    // Row-owned encode arm: the kind IS the behavior, so the source dispatch stays one level deep and a repeated
    // full-coverage kind Switch inside the Leaf arm never arises.
    [UseDelegateFromConstructor]
    public partial Fin<ResidencyPayload> LeafArm(ImportedGeometry leaf, ResidencyPolicy policy);
}

// Meshlet-builder axis over the three count-bearing meshopt builds: cone-weighted scan, variable-size flex
// (min..max triangles under split_factor), spatial-locality fill — a build strategy is a row, never a fork.
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ClusterBuild {
    public static readonly ClusterBuild ConeWeighted = new("cone");
    public static readonly ClusterBuild Flex = new("flex");
    public static readonly ClusterBuild Spatial = new("spatial");
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
    ClusterBuild Cluster,
    int MaxVertices,
    int MinTriangles,
    int MaxTriangles,
    float ConeWeight,
    float SplitFactor,
    float FillWeight,
    int QuantizationBits,
    int CodecLevel,
    int CodecVersion,
    double SimplifyTarget,
    double AttributeWeight) {
    public static readonly ResidencyPolicy Canonical = new(
        Cluster: ClusterBuild.ConeWeighted, MaxVertices: 64, MinTriangles: 32, MaxTriangles: 124,
        ConeWeight: 0.25f, SplitFactor: 2.0f, FillWeight: 0.5f, QuantizationBits: 14,
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
public readonly record struct StreamSpan(int Offset, int Length, int Count, int ByteStride, StreamMode Mode, StreamFilter Filter, int CodecVersion);

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

    // One dispatch level: the source Switch resolves modality, the Leaf arm reads the kind's row-owned LeafArm
    // column — dispatch plus data retrieval, never a second full-coverage Switch nested in the arm.
    public static Fin<ResidencyPayload> Encode(ResidencySource source, ResidencyPolicy policy) =>
        Admit(source, policy).Bind(admitted => admitted.Source.Switch(
            state: admitted.Policy,
            leaf: static (p, l) => l.Kind.LeafArm(l.Geometry, p),
            splat: static (p, s) => SplatEncode(s.Scan, p)));

    public static ComputeReceipt.StreamSegment Receipt(ResidencyPayload payload, CorrelationId correlation, WorkLane lane, Duration elapsed) =>
        new(payload.ArtifactKey, payload.Clusters.IsEmpty ? payload.Layout.Count : payload.Clusters.Count, payload.EncodedBytes) {
            Scope = new ReceiptScope.Execution(correlation, lane, Substrate.CpuTensor, AllocationClass.PooledMemory, elapsed),
        };

    static Fin<(ResidencySource Source, ResidencyPolicy Policy)> Admit(ResidencySource source, ResidencyPolicy policy) {
        Seq<(bool Invalid, string Fact)> checks = Seq(
            (policy.MaxVertices is < 3 or > 255, $"max-vertices:{policy.MaxVertices}"),
            (policy.MaxTriangles is < 4 or > 512 || policy.MaxTriangles % 4 != 0, $"max-triangles:{policy.MaxTriangles}"),
            (policy.MinTriangles is < 1 || policy.MinTriangles > policy.MaxTriangles, $"min-triangles:{policy.MinTriangles}"),
            (policy.ConeWeight is < 0f or > 1f, $"cone-weight:{policy.ConeWeight:R}"),
            (policy.SplitFactor < 1f, $"split-factor:{policy.SplitFactor:R}"),
            (policy.FillWeight is < 0f or > 1f, $"fill-weight:{policy.FillWeight:R}"),
            (policy.QuantizationBits is < 1 or > 24, $"quantization-bits:{policy.QuantizationBits}"),
            (policy.SimplifyTarget is <= 0d or > 1d, $"simplify-target:{policy.SimplifyTarget:R}"),
            (policy.AttributeWeight < 0d || !double.IsFinite(policy.AttributeWeight), $"attribute-weight:{policy.AttributeWeight:R}"));
        Seq<string> faults = checks.Filter(static check => check.Invalid).Map(static check => check.Fact);
        Fin<ResidencySource> admittedSource = source.Switch(
            leaf: static leaf => LeafShapeValid(leaf)
                    ? Fin.Succ<ResidencySource>(leaf)
                    : Fin.Fail<ResidencySource>(new ComputeFault.PayloadOverBounds($"<residency-leaf-shape:{leaf.Geometry.VertexCount}:{leaf.Geometry.Vertices.Length}:{leaf.Geometry.Indices.Length}>")),
            splat: static splat => SplatShapeValid(splat.Scan)
                ? Fin.Succ<ResidencySource>(splat)
                : Fin.Fail<ResidencySource>(new ComputeFault.PayloadOverBounds($"<residency-splat-shape:{splat.Scan.SplatCount}:{splat.Scan.HarmonicDegree}>")));
        return faults.IsEmpty
            ? admittedSource.Map(admitted => (admitted, policy))
            : Fin.Fail<(ResidencySource, ResidencyPolicy)>(new ComputeFault.PayloadOverBounds($"<residency-policy:{string.Join('|', faults)}>"));
    }

    static bool LeafShapeValid(ResidencySource.Leaf leaf) {
        ImportedGeometry geometry = leaf.Geometry;
        bool attributes = geometry.VertexCount > 0
            && geometry.Vertices.Length / 3 >= geometry.VertexCount
            && (geometry.Normals.IsEmpty || geometry.Normals.Length / 3 >= geometry.VertexCount);
        if (!attributes || leaf.Kind == ResidencyKind.PointSplat || leaf.Kind.SplatBorne) { return attributes; }
        if (geometry.Indices.Length < 3 || geometry.Indices.Length % 3 != 0) { return false; }
        foreach (long index in geometry.Indices.Span) {
            if (index < 0 || index >= geometry.VertexCount || index > uint.MaxValue) { return false; }
        }
        return true;
    }

    static bool SplatShapeValid(SplatScan scan) {
        if (scan.SplatCount is <= 0 or > int.MaxValue || scan.HarmonicDegree is < 0 or > 46339) { return false; }
        long degreeWidth = (long)scan.HarmonicDegree + 1;
        long width = degreeWidth * degreeWidth * 3;
        return scan.Positions.Length / 3 >= scan.SplatCount
            && scan.Scales.Length / 3 >= scan.SplatCount
            && scan.Rotations.Length / 4 >= scan.SplatCount
            && width > 0
            && scan.Harmonics.Length / width >= scan.SplatCount;
    }

    internal static Fin<ResidencyPayload> SplatBorneLeafRejected(ImportedGeometry leaf, ResidencyPolicy policy) =>
        Fin.Fail<ResidencyPayload>(new ComputeFault.ModelRejected($"<residency-splat-needs-scan:{leaf.FormatKey}>"));

    internal static Fin<ResidencyPayload> MeshletEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        ReadOnlySpan<float> positions = leaf.Vertices.Span;
        uint[] optimized = new uint[leaf.Indices.Length];
        Meshopt.OptimizeVertexCache(optimized, ToUInt(leaf.Indices.Span), (nuint)leaf.VertexCount);
        nuint maxMeshlets = Meshopt.BuildMeshletsBound((nuint)optimized.Length, (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles);
        Meshlet[] meshlets = new Meshlet[(int)maxMeshlets];
        uint[] meshletVertices = new uint[(int)maxMeshlets * policy.MaxVertices];
        byte[] meshletTriangles = new byte[(int)maxMeshlets * policy.MaxTriangles * 3];
        int count = BuildClusters(optimized, positions, leaf.VertexCount, policy, meshlets, meshletVertices, meshletTriangles);
        if (count == 0) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-meshlet-empty:{leaf.FormatKey}>")); }
        List<ResidencyMeshlet> clusters = new(count);
        for (int m = 0; m < count; m++) {
            ref readonly Meshlet meshlet = ref meshlets[m];
            Span<uint> localVertices = meshletVertices.AsSpan((int)meshlet.vertex_offset, (int)meshlet.vertex_count);
            Span<byte> localTriangles = meshletTriangles.AsSpan((int)meshlet.triangle_offset, (int)meshlet.triangle_count * 3);
            Meshopt.OptimizeMeshlet(localVertices, localTriangles, meshlet.triangle_count, meshlet.vertex_count);
            clusters.Add(Cluster(meshlet, ClusterBounds(localVertices, localTriangles, (int)meshlet.triangle_count, positions, leaf.VertexCount), level: 0, error: 0f));
        }
        ref readonly Meshlet tail = ref meshlets[count - 1];
        int usedVertices = (int)(tail.vertex_offset + tail.vertex_count);
        int usedTriangleBytes = (int)(tail.triangle_offset + tail.triangle_count * 3);
        (Seq<ResidencyMeshlet> Clusters, uint[] Vertices, byte[] Triangles) chained = LodChain(
            optimized, positions, leaf.VertexCount, policy, clusters,
            meshletVertices.AsSpan(0, usedVertices), meshletTriangles.AsSpan(0, usedTriangleBytes));
        Seq<(ResidencyStream Stream, StreamMode Mode, StreamFilter Filter, int Count, int ByteStride, int CodecVersion, ReadOnlyMemory<byte> Bytes)> streams = Seq(
            (ResidencyStream.Positions, StreamMode.Attributes, StreamFilter.None, leaf.VertexCount, PositionStride, policy.CodecVersion, EncodeVertices(positions, leaf.VertexCount, policy)),
            (ResidencyStream.Indices, StreamMode.Indices, StreamFilter.None, chained.Vertices.Length, sizeof(uint), IndexCodecVersion, EncodeSequence(chained.Vertices, leaf.VertexCount)),
            (ResidencyStream.Triangles, StreamMode.Raw, StreamFilter.None, chained.Triangles.Length, 1, 0, chained.Triangles));
        if (HasNormals(leaf)) { streams = streams.Add((ResidencyStream.Normals, StreamMode.Attributes, StreamFilter.Octahedral, leaf.VertexCount, sizeof(uint), policy.CodecVersion, EncodeNormals(leaf.Normals.Span, leaf.VertexCount, policy))); }
        return Fin.Succ(Assemble(ResidencyKind.MeshletCluster, leaf.FormatKey, streams, chained.Clusters, leaf.VertexCount, SphereBounds(positions, leaf.VertexCount), 0, policy));
    }

    // Each coarser level simplifies the prior level's index buffer through the Meshopt.Simplify ladder (result_error
    // scaled to object space by SimplifyScale), re-clusters, and links each fine cluster to the coarse parent whose
    // sphere CONTAINS it, falling back to nearest center. Monotonic guarantee at link time: a parent's Error rises to
    // at least max(children) before children stamp ParentError, so a screen-space cut is crack-free and
    // double-draw-free. Ladder terminates when a level stops shrinking or one meshlet remains; roots carry
    // Parent = -1, ParentError = +inf.
    static (Seq<ResidencyMeshlet> Clusters, uint[] Vertices, byte[] Triangles) LodChain(
        uint[] indices,
        ReadOnlySpan<float> positions,
        int vertexCount,
        ResidencyPolicy policy,
        List<ResidencyMeshlet> level0,
        ReadOnlySpan<uint> level0Vertices,
        ReadOnlySpan<byte> level0Triangles) {
        List<ResidencyMeshlet> all = new(level0);
        List<uint> vertices = new(level0Vertices.ToArray());
        List<byte> triangles = new(level0Triangles.ToArray());
        float scale = Meshopt.SimplifyScale(positions, (nuint)vertexCount, PositionStride);
        uint[] current = indices;
        int level = 0, firstOfLevel = 0, countOfLevel = level0.Count;
        while (countOfLevel > 1) {
            uint[] simplified = new uint[current.Length];
            nuint target = (nuint)Math.Max(3, (long)(current.Length * policy.SimplifyTarget) / 3 * 3);
            nuint written = Meshopt.Simplify(simplified, current, positions, (nuint)vertexCount, PositionStride, target, targetError: float.MaxValue, options: 0, out float resultError);
            if (written >= (nuint)current.Length || written < 3) { break; }
            Array.Resize(ref simplified, (int)written);
            level++;
            float objectError = resultError * scale;
            (int coarse, int coarseFirst) = ClusterLevel(simplified, positions, vertexCount, policy, all, vertices, triangles, level, objectError);
            if (coarse == 0) { break; }
            Link(all, firstOfLevel, countOfLevel, coarseFirst, coarse);
            firstOfLevel = coarseFirst; countOfLevel = coarse; current = simplified;
        }
        return (all.ToSeq(), vertices.ToArray(), triangles.ToArray());
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

    internal static Fin<ResidencyPayload> QuantizedEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        if (policy.QuantizationBits is < 1 or > 24) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-quant-bits:{policy.QuantizationBits}>")); }
        Seq<(ResidencyStream Stream, StreamMode Mode, StreamFilter Filter, int Count, int ByteStride, int CodecVersion, ReadOnlyMemory<byte> Bytes)> streams = Seq(
            (ResidencyStream.Positions, StreamMode.Attributes, StreamFilter.Exponential, leaf.VertexCount, PositionStride, policy.CodecVersion, EncodeExp(leaf.Vertices.Span, leaf.VertexCount, policy)),
            (ResidencyStream.Indices, StreamMode.Triangles, StreamFilter.None, leaf.Indices.Length, sizeof(uint), IndexCodecVersion, EncodeTriangles(ToUInt(leaf.Indices.Span), leaf.VertexCount)));
        if (HasNormals(leaf)) { streams = streams.Add((ResidencyStream.Normals, StreamMode.Attributes, StreamFilter.Octahedral, leaf.VertexCount, sizeof(uint), policy.CodecVersion, EncodeNormals(leaf.Normals.Span, leaf.VertexCount, policy))); }
        return Fin.Succ(Assemble(ResidencyKind.QuantizedVertex, leaf.FormatKey, streams, Seq<ResidencyMeshlet>(), leaf.VertexCount, SphereBounds(leaf.Vertices.Span, leaf.VertexCount), 0, policy));
    }

    internal static Fin<ResidencyPayload> PointEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        if (policy.SimplifyTarget is <= 0 or > 1) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-simplify-target:{policy.SimplifyTarget:R}>")); }
        int target = Math.Max(1, (int)(leaf.VertexCount * policy.SimplifyTarget));
        uint[] remap = new uint[target];
        int kept = DecimatePoints(remap, leaf.Vertices.Span, leaf.VertexCount,
            HasNormals(leaf) ? leaf.Normals.Span : ReadOnlySpan<float>.Empty, policy.AttributeWeight, target);
        float[] gathered = new float[kept * 3];
        ReadOnlySpan<float> source = leaf.Vertices.Span;
        for (int v = 0; v < kept; v++) { source.Slice((int)remap[v] * 3, 3).CopyTo(gathered.AsSpan(v * 3)); }
        Seq<(ResidencyStream Stream, StreamMode Mode, StreamFilter Filter, int Count, int ByteStride, int CodecVersion, ReadOnlyMemory<byte> Bytes)> streams = Seq((ResidencyStream.Positions, StreamMode.Attributes, StreamFilter.Exponential, kept, PositionStride, policy.CodecVersion, EncodeExp(gathered, kept, policy)));
        return Fin.Succ(Assemble(ResidencyKind.PointSplat, leaf.FormatKey, streams, Seq<ResidencyMeshlet>(), kept, SphereBounds(gathered, kept), 0, policy));
    }

    static Fin<ResidencyPayload> SplatEncode(SplatScan scan, ResidencyPolicy policy) {
        if (policy.QuantizationBits is < 1 or > 16) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-splat-bits:{policy.QuantizationBits}>")); }
        int n = (int)scan.SplatCount;
        int shFloats = (scan.HarmonicDegree + 1) * (scan.HarmonicDegree + 1) * 3;
        ulong[] rotations = new ulong[n];
        Meshopt.EncodeFilterQuat<ulong>(rotations, policy.QuantizationBits, scan.Rotations.Span[..(n * 4)]);
        uint[] harmonics = new uint[n * shFloats];
        Meshopt.EncodeFilterExp<uint>(harmonics, policy.QuantizationBits, scan.Harmonics.Span[..(n * shFloats)], EncodeExpMode.EncodeExpSeparate);
        Seq<(ResidencyStream Stream, StreamMode Mode, StreamFilter Filter, int Count, int ByteStride, int CodecVersion, ReadOnlyMemory<byte> Bytes)> streams = Seq(
            (ResidencyStream.Positions, StreamMode.Attributes, StreamFilter.Exponential, n, PositionStride, policy.CodecVersion, EncodeExp(scan.Positions.Span, n, policy)),
            (ResidencyStream.Scales, StreamMode.Attributes, StreamFilter.Exponential, n, PositionStride, policy.CodecVersion, EncodeExp(scan.Scales.Span, n, policy)),
            (ResidencyStream.Rotations, StreamMode.Attributes, StreamFilter.Quaternion, n, sizeof(ulong), policy.CodecVersion, EncodeStream<ulong>(rotations, policy.CodecLevel, policy.CodecVersion)),
            (ResidencyStream.Harmonics, StreamMode.Attributes, StreamFilter.Exponential, n * shFloats, sizeof(uint), policy.CodecVersion, EncodeStream<uint>(harmonics, policy.CodecLevel, policy.CodecVersion)));
        return Fin.Succ(Assemble(ResidencyKind.GaussianSplat, scan.FormatKey, streams, Seq<ResidencyMeshlet>(), n, SphereBounds(scan.Positions.Span, n), scan.HarmonicDegree, policy));
    }

    static ResidencyPayload Assemble(ResidencyKind kind, string formatKey,
        Seq<(ResidencyStream Stream, StreamMode Mode, StreamFilter Filter, int Count, int ByteStride, int CodecVersion, ReadOnlyMemory<byte> Bytes)> streams,
        Seq<ResidencyMeshlet> clusters, int residentCount, (Vector3 Center, float Radius) bounds, int harmonicDegree, ResidencyPolicy policy) {
        byte[] blob = new byte[streams.Sum(static stream => stream.Bytes.Length)];
        Dictionary<ResidencyStream, StreamSpan> layout = new(streams.Count);
        int cursor = 0;
        foreach ((ResidencyStream stream, StreamMode mode, StreamFilter filter, int count, int byteStride, int codecVersion, ReadOnlyMemory<byte> bytes) in streams) {
            bytes.Span.CopyTo(blob.AsSpan(cursor));
            layout[stream] = new StreamSpan(cursor, bytes.Length, count, byteStride, mode, filter, codecVersion);
            cursor += bytes.Length;
        }
        UInt128 key = InterchangeIdentity.Key($"{formatKey}:{kind.Key}", blob, policy.ConeWeight, policy.SimplifyTarget, policy.QuantizationBits);
        return new ResidencyPayload(kind, key, blob, layout.ToFrozenDictionary(), clusters, residentCount, bounds.Center, bounds.Radius, harmonicDegree);
    }

    static ReadOnlyMemory<byte> EncodeVertices(ReadOnlySpan<float> positions, int count, ResidencyPolicy policy) =>
        EncodeStream(MemoryMarshal.Cast<float, Vector3>(positions[..(count * 3)]), policy.CodecLevel, policy.CodecVersion);

    static ReadOnlyMemory<byte> EncodeExp(ReadOnlySpan<float> floats, int count, ResidencyPolicy policy) {
        Packed12[] packed = new Packed12[count];
        Meshopt.EncodeFilterExp<Packed12>(packed, policy.QuantizationBits, floats[..(count * 3)], EncodeExpMode.EncodeExpSharedComponent);
        return EncodeStream<Packed12>(packed, policy.CodecLevel, policy.CodecVersion);
    }

    static ReadOnlyMemory<byte> EncodeNormals(ReadOnlySpan<float> normals, int count, ResidencyPolicy policy) {
        float[] quad = new float[count * 4];
        for (int v = 0; v < count; v++) { normals.Slice(v * 3, 3).CopyTo(quad.AsSpan(v * 4)); }
        uint[] packed = new uint[count];
        Meshopt.EncodeFilterOct<uint>(packed, OctBits, quad);
        return EncodeStream<uint>(packed, policy.CodecLevel, policy.CodecVersion);
    }

    static ReadOnlyMemory<byte> EncodeStream<T>(ReadOnlySpan<T> packed, int level, int version) where T : unmanaged {
        byte[] buffer = new byte[(int)Meshopt.EncodeVertexBufferBound((nuint)packed.Length, (nuint)Unsafe.SizeOf<T>())];
        return buffer.AsMemory(0, (int)Meshopt.EncodeVertexBufferLevel<T>(buffer, packed, level, version));
    }

    static ReadOnlyMemory<byte> EncodeTriangles(ReadOnlySpan<uint> indices, int vertexCount) {
        byte[] buffer = new byte[(int)Meshopt.EncodeIndexBufferBound((nuint)indices.Length, (nuint)vertexCount)];
        return buffer.AsMemory(0, (int)Meshopt.EncodeIndexBuffer(buffer, indices));
    }

    static ReadOnlyMemory<byte> EncodeSequence(Span<uint> sequence, int vertexCount) {
        byte[] buffer = new byte[(int)Meshopt.EncodeIndexSequenceBound((nuint)sequence.Length, (nuint)vertexCount)];
        return buffer.AsMemory(0, (int)Meshopt.EncodeIndexSequence(buffer, sequence));
    }

    static uint[] ToUInt(ReadOnlySpan<long> indices) {
        uint[] converted = new uint[indices.Length];
        for (int i = 0; i < indices.Length; i++) { converted[i] = (uint)indices[i]; }
        return converted;
    }

    static bool HasNormals(ImportedGeometry leaf) => leaf.Normals.Length >= leaf.VertexCount * 3;

    static ResidencyMeshlet Cluster(in Meshlet meshlet, Bounds bounds, int level, float error) {
        ReadOnlySpan<float> f = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Bounds, float>(ref bounds), 11);
        return new ResidencyMeshlet(
            (int)meshlet.vertex_offset, (int)meshlet.triangle_offset, (int)meshlet.vertex_count, (int)meshlet.triangle_count,
            new Vector3(f[0], f[1], f[2]), f[3], new Vector3(f[4], f[5], f[6]), new Vector3(f[7], f[8], f[9]), f[10],
            Level: level, Parent: -1, Error: error, ParentError: float.PositiveInfinity);
    }

    // One coarser level clustered through the SAME BuildClusters kernel; returns (count, firstIndex).
    static (int Count, int First) ClusterLevel(
        uint[] simplified,
        ReadOnlySpan<float> positions,
        int vertexCount,
        ResidencyPolicy policy,
        List<ResidencyMeshlet> all,
        List<uint> payloadVertices,
        List<byte> payloadTriangles,
        int level,
        float objectError) {
        nuint bound = Meshopt.BuildMeshletsBound((nuint)simplified.Length, (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles);
        Meshlet[] meshlets = new Meshlet[(int)bound];
        uint[] vertices = new uint[(int)bound * policy.MaxVertices];
        byte[] triangles = new byte[(int)bound * policy.MaxTriangles * 3];
        int first = all.Count;
        int vertexBase = payloadVertices.Count;
        int triangleBase = payloadTriangles.Count;
        int count = BuildClusters(simplified, positions, vertexCount, policy, meshlets, vertices, triangles);
        for (int m = 0; m < count; m++) {
            ref readonly Meshlet meshlet = ref meshlets[m];
            Span<uint> localVertices = vertices.AsSpan((int)meshlet.vertex_offset, (int)meshlet.vertex_count);
            Span<byte> localTriangles = triangles.AsSpan((int)meshlet.triangle_offset, (int)meshlet.triangle_count * 3);
            ResidencyMeshlet cluster = Cluster(meshlet, ClusterBounds(localVertices, localTriangles, (int)meshlet.triangle_count, positions, vertexCount), level, objectError);
            all.Add(cluster with { VertexOffset = vertexBase + cluster.VertexOffset, TriangleOffset = triangleBase + cluster.TriangleOffset });
        }
        if (count > 0) {
            ref readonly Meshlet tail = ref meshlets[count - 1];
            payloadVertices.AddRange(vertices.AsSpan(0, (int)(tail.vertex_offset + tail.vertex_count)).ToArray());
            payloadTriangles.AddRange(triangles.AsSpan(0, (int)(tail.triangle_offset + tail.triangle_count * 3)).ToArray());
        }
        return (count, first);
    }

    // Safe span overloads pass element-span length as the semantic vertex/triangle/point count (wrong for
    // interleaved-float positions and 3-byte triangles), so these four count-bearing builds pin and pass true
    // counts; the ClusterBuild row resolves by identity INSIDE the fixed block because meshlet pointers cannot
    // cross a generated-Switch lambda — the pinned kernel is the named exemption carrying this one row branch.
    static unsafe int BuildClusters(ReadOnlySpan<uint> indices, ReadOnlySpan<float> positions, int vertexCount, ResidencyPolicy policy,
        Meshlet[] meshlets, uint[] meshletVertices, byte[] meshletTriangles) {
        fixed (Meshlet* meshlet = meshlets)
        fixed (uint* vertices = meshletVertices)
        fixed (byte* triangles = meshletTriangles)
        fixed (uint* index = indices)
        fixed (float* position = positions) {
            return (int)(policy.Cluster == ClusterBuild.Flex
                ? Meshopt.BuildMeshletsFlex(meshlet, vertices, triangles, index, (nuint)indices.Length, position, (nuint)vertexCount,
                    (nuint)PositionStride, (nuint)policy.MaxVertices, (nuint)policy.MinTriangles, (nuint)policy.MaxTriangles, policy.ConeWeight, policy.SplitFactor)
                : policy.Cluster == ClusterBuild.Spatial
                    ? Meshopt.BuildMeshletsSpatial(meshlet, vertices, triangles, index, (nuint)indices.Length, position, (nuint)vertexCount,
                        (nuint)PositionStride, (nuint)policy.MaxVertices, (nuint)policy.MinTriangles, (nuint)policy.MaxTriangles, policy.FillWeight)
                    : Meshopt.BuildMeshlets(meshlet, vertices, triangles, index, (nuint)indices.Length, position, (nuint)vertexCount,
                        (nuint)PositionStride, (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles, policy.ConeWeight));
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
            Bounds bounds = Meshopt.ComputeSphereBounds(position, (nuint)count, (nuint)PositionStride, null, 0);
            ReadOnlySpan<float> f = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Bounds, float>(ref bounds), 4);
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

- [SPLAT_COMPANION_WIRE]-[BLOCKED]: packed position/scale/rotation/spherical-harmonic buffer order and harmonic-degree band emitted by the Python `realitycapture` companion SPZ v4 / SOG v2 decode; byte-mirror against `SplatScan` and `GaussianSplatScan` at the companion seam.

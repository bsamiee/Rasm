# [COMPUTE_RESIDENCY]

Rasm.Compute interchange lane: the streaming-residency half of artifact interchange, owning the content-keyed GPU-ready **PAYLOAD** codec the web viewer streams cell-by-cell — the meshlet-cluster encode that partitions an octree-leaf `ImportedGeometry` into cone-cullable clusters with `meshoptimizer` vertex/index codecs, the quantized-vertex encode that half/N-bit-quantizes and level-compresses a leaf for a low-VRAM tile, and the point-splat / gaussian-splat encode that simplifies a reality-capture point set and homes a Python-companion-decoded splat scan as content-addressed bytes. The page owns ONE payload axis (`ResidencyKind`), ONE content-keyed carrier (`ResidencyPayload`), and ONE state-threaded `Encode` fold over the native `Meshopt` surface — it produces bytes addressed by the suite `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` key, reads the existing `Runtime/codecs#TILE_PARTITION` `ImportedGeometry` octree leaf (never a second partition), and rides the existing `Runtime/receipts#RECEIPT_UNION` `StreamSegment` slot (never a new receipt case). The streamable manifest that names these payloads — the `WEB_GEOMETRY_RESIDENCY_WIRE` scene-graph plus meshlet/splat residency manifest the TypeScript worker consumes — is minted **once** by `csharp:Rasm.AppUi/Render/viewport#WEB_RESIDENCY` `ResidencyManifest.Mint`; this page mints **no** manifest, only the content-keyed bytes that manifest references by key, so a Compute-side `ResidencyManifest` is the named second-mint drift defect. The encoded bytes land content-addressed on the Persistence blob lane through the same `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Admit` path, and the splat payload arm grounds its decoded scan on the Python `realitycapture` companion SPZ/SOG wire reached at the `Runtime/channels#PROTO_VOCABULARY` `GeometryPayload`/`ArtifactSync` artifact seam, never an in-process splat fit; this page is HOST-LOCAL and carries no TS_PROJECTION.

## [01]-[INDEX]

- [01]-[RESIDENCY]: the `ResidencyKind` payload axis, the `ResidencyPayload` carrier, the `Encode` fold over the native `Meshopt` surface, and the splat-companion decode seam.

## [02]-[RESIDENCY]

- Owner: `ResidencyKind` `[SmartEnum<string>]` the one closed payload axis — `meshlet-cluster` · `quantized-vertex` · `point-splat` · `gaussian-splat` — each row a column-bearing posture (cone-cullable, quantizable, level-compressible, splat-borne), so a new encoding is one row and never a per-kind payload type; `ResidencyMeshlet` the readonly per-cluster `Meshlet`/`Bounds` cone-and-sphere record the cluster arm emits; `ResidencyPolicy` the encode-posture policy record carrying the meshlet cluster caps, the quantization bit budget, the simplification target, the codec level, and the cone weight folded into the content-key seed; `ResidencyPayload` the **content-keyed buffer carrier** — the encoded vertex/index/meshlet byte spans, the per-cluster cone-cull `Bounds`, the resident counts, and the `XxHash128` content key, NOT a manifest; `Residency` the static `Encode` fold projecting a `Runtime/codecs#TILE_PARTITION` `ImportedGeometry` octree leaf (or a companion-decoded splat scan) into a `ResidencyPayload` through one state-threaded pass over the native `Meshopt` meshlet/cluster-bounds/spatial-sort/codec/quantize/simplify surface; `MeshoptCodec` the unsafe boundary capsule owning every `fixed`-pinned `Meshopt` P/Invoke so the fold stays expression-shaped over a safe span face.
- Cases: `ResidencyKind` rows `meshlet-cluster` (cone-cullable cluster set with encoded vertex/index buffers) · `quantized-vertex` (half/N-bit-quantized, level-compressed single tile) · `point-splat` (point-cloud-simplified positions, the reality-capture LOD floor) · `gaussian-splat` (Python-companion-decoded SPZ/SOG gaussian scan homed as content-addressed bytes).
- Entry: `public static Fin<ResidencyPayload> Encode(ResidencyKind kind, ImportedGeometry leaf, ResidencyPolicy policy)` projects an octree-leaf geometry onto the kind's encode arm and lands the content-keyed payload; `public static Fin<ResidencyPayload> EncodeSplat(SplatScan scan, ResidencyPolicy policy)` homes a companion-decoded gaussian scan as a `gaussian-splat` payload; `public static Fin<SplatScan> DecodeSplat(string formatKey, ReadOnlyMemory<byte> bytes)` reads an SPZ/SOG companion scan; `Fin<T>` aborts on a meshlet-build bound overflow, a quantization budget the bit width cannot meet, or — for the splat arm — the un-admitted in-process splat decoder.
- Auto: `Encode` dispatches the kind row through one generated `Switch`; the `meshlet-cluster` arm spatially sorts the leaf vertices through `Meshopt.SpatialSortRemap` so cluster locality is maximized, builds cone-cullable clusters through `Meshopt.BuildMeshlets` sized to the policy `max_vertices`/`max_triangles`/`cone_weight` (the destination meshlet/vertex/triangle buffers sized by `Meshopt.BuildMeshletsBound`), computes each cluster's cone-and-sphere `Bounds` through `Meshopt.ComputeMeshletBounds` so the web viewer back-face-and-frustum culls a whole cluster off one cone test, and compresses the cluster vertex and index buffers through `Meshopt.EncodeVertexBuffer`/`Meshopt.EncodeIndexBuffer` (each destination sized by the matching `*Bound`) so a meshlet tile streams pre-compressed; the `quantized-vertex` arm quantizes each interleaved vertex attribute through `Meshopt.QuantizeHalf` (position/normal half-float) and `Meshopt.QuantizeFloat` (N-bit mantissa truncation per the policy bit budget) and emits a level-compressed buffer through `Meshopt.EncodeVertexBufferLevel` at the policy codec level so a low-VRAM tile carries the smallest bounded-loss vertex residence; the `point-splat` arm simplifies the leaf positions through `Meshopt.SimplifyPoints` to the policy target ratio (color-weighted when the scan carries color) so the reality-capture LOD floor streams a decimated point set; the `gaussian-splat` arm carries the companion-decoded `SplatScan` ellipsoid/harmonic bytes through `EncodeVertexBufferLevel` so the splat tile streams content-addressed; every arm computes the cluster-cone `Bounds` once, keys the payload through `InterchangeIdentity.Key` over the encoded bytes folded with the policy posture, and stamps the resident counts on the carrier.
- Receipt: the `Runtime/receipts#RECEIPT_UNION` `StreamSegment(string ArtifactId, int Segments, long Bytes)` slot carries the payload content-key as the artifact id, the cluster/tile/splat count as the segment count, and the encoded byte length — a re-encode of identical leaf geometry at identical policy stamps the same content-key so the residency emission is auditable through the existing slot, never a new receipt case; the addressed bytes land on the Persistence blob lane through `InterchangeIdentity.Admit` so a re-emitted identical payload dedups and a `Cache` receipt records the hit.
- Packages: Alimer.Bindings.MeshOptimizer, System.IO.Hashing, CommunityToolkit.HighPerformance, System.Numerics.Tensors, LanguageExt.Core, Rasm.Persistence (project), BCL inbox
- Growth: a new residency encoding is one `ResidencyKind` row with its encode-arm `Switch` case and its posture column on `ResidencyPolicy`; a new quantization posture is one column on `ResidencyPolicy` folded into the seed; a new meshlet cap is one policy value; a companion-decoded scan format is one `formatKey` discriminant on `DecodeSplat`; zero new surface — a `MeshletResidencyEncoder`/`SplatPayloadCodec`/`QuantizedVertexEncoder` sibling owner is collapsed onto the one `Residency.Encode` fold over the `ResidencyKind` axis, and a per-kind `ResidencyPayload` subtype is collapsed onto the one content-keyed carrier whose kind column discriminates.
- Boundary: this lane owns ONLY the content-keyed PAYLOAD bytes — the streamable `WEB_GEOMETRY_RESIDENCY_WIRE` manifest that names them is minted once by `csharp:Rasm.AppUi/Render/viewport#WEB_RESIDENCY` `ResidencyManifest.Mint` and this page mints no manifest, no scene-graph, and no second content-identity value object (a Compute-side `ResidencyManifest` is the named drift defect, and the `:x32` content-key the AppUi marshal renders is the suite `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128.HashToUInt128` value, never a second hash); the leaf geometry is read from the existing `Runtime/codecs#TILE_PARTITION` `TileSet` octree leaf `ImportedGeometry` (`codecs` partitions, this lane encodes the partitioned leaf), so a second octree partition in this page is the rejected form and the residency content-key composes the same `Runtime/codecs#CONTENT_ADDRESSING` `InterchangeIdentity.Key`/`Admit` law so a meshlet tile, a quantized tile, and a splat tile are content-keyed rows under one identity scheme the Persistence index owns; `meshoptimizer` owns every meshlet build, cluster bounds, spatial sort, vertex/index codec, quantization, and point simplification through the `MeshoptCodec` unsafe boundary capsule — the capsule is the single `fixed`-pinned P/Invoke seam (the native bindings are `unsafe extern` over `uint*`/`void*`/`float*`, so the pinning and the `*Bound` destination sizing are the language-owned statement carve-out the capsule isolates while the `Encode` fold stays expression-shaped over a safe span face), and a hand-rolled meshlet partitioner, a hand-rolled vertex codec, or a hand-rolled point simplifier is the deleted form; the `gaussian-splat` arm homes a splat payload but does NOT admit a splat fit or a point-cloud-file reader — the SPZ/SOG gaussian scan is decoded by the Python `realitycapture` companion and crosses the `Runtime/channels#PROTO_VOCABULARY` `GeometryPayload`/`ArtifactSync` artifact seam as bytes this lane content-keys and homes, so `DecodeSplat` faults `splat-companion-pending` until the companion SPZ/SOG decode wire grounds (this is a DISTINCT concern from the `Runtime/codecs#FIELD_RESULT_CODEC` `point-catalogue-pending` E57/LAS/LAZ/PTS point-cloud-**file** reader fault — residency provides a content-addressed home for splat **payload** but does not itself admit the point-cloud file readers, and the IDEAS "supersedes point-catalogue-pending" phrasing conflated the two formats and is flagged a residual risk, not asserted as a closure); the `point-splat` simplification floor rides `Meshopt.SimplifyPoints` over the leaf positions and the `gaussian-splat` ellipsoid/harmonic payload rides `Meshopt.EncodeVertexBufferLevel`, never a managed copy beside the blob lane — a `ToArray` flatten on the residency emit path past the one encoded payload window is the named defect.

```csharp contract
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<InterchangeKeyPolicy, string>]
[KeyMemberComparer<InterchangeKeyPolicy, string>]
public sealed partial class ResidencyKind {
    public static readonly ResidencyKind MeshletCluster = new("meshlet-cluster", coneCullable: true, quantizable: false, splatBorne: false);
    public static readonly ResidencyKind QuantizedVertex = new("quantized-vertex", coneCullable: false, quantizable: true, splatBorne: false);
    public static readonly ResidencyKind PointSplat = new("point-splat", coneCullable: false, quantizable: true, splatBorne: false);
    public static readonly ResidencyKind GaussianSplat = new("gaussian-splat", coneCullable: false, quantizable: false, splatBorne: true);

    public bool ConeCullable { get; }
    public bool Quantizable { get; }
    public bool SplatBorne { get; }
}

// --- [CONSTANTS] -----------------------------------------------------------------------

public sealed record ResidencyPolicy(
    int MaxVertices,
    int MaxTriangles,
    float ConeWeight,
    int QuantizationBits,
    int CodecLevel,
    double SimplifyTarget,
    double ColorWeight,
    int VertexStride) {
    public static readonly ResidencyPolicy Canonical = new(
        MaxVertices: 64, MaxTriangles: 124, ConeWeight: 0.25f,
        QuantizationBits: 14, CodecLevel: 2, SimplifyTarget: 0.25, ColorWeight: 0.5, VertexStride: 12);
}

// --- [MODELS] --------------------------------------------------------------------------

public readonly record struct ResidencyMeshlet(
    int VertexOffset,
    int TriangleOffset,
    int VertexCount,
    int TriangleCount,
    float[] ConeApex,
    float[] ConeAxis,
    float ConeCutoff,
    float[] Center,
    float Radius);

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
    ReadOnlyMemory<byte> EncodedVertices,
    ReadOnlyMemory<byte> EncodedIndices,
    ReadOnlyMemory<byte> EncodedMeshlets,
    Seq<ResidencyMeshlet> Clusters,
    int ResidentCount,
    long EncodedBytes) {
    public string ArtifactKey => $"{ContentKey:x32}:{Kind.Key}";
}

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Residency {
    public static Fin<ResidencyPayload> Encode(ResidencyKind kind, ImportedGeometry leaf, ResidencyPolicy policy) =>
        kind.Switch(
            meshletCluster: _ => MeshletEncode(leaf, policy),
            quantizedVertex: _ => QuantizedEncode(leaf, policy),
            pointSplat: _ => PointEncode(leaf, policy),
            gaussianSplat: _ => Fin.Fail<ResidencyPayload>(new ComputeFault.ModelRejected($"<residency-splat-needs-scan:{leaf.FormatKey}>")));

    public static Fin<ResidencyPayload> EncodeSplat(SplatScan scan, ResidencyPolicy policy) =>
        Try.lift(() => SplatEncode(scan, policy)).Run().MapFail(static error => (Error)new ComputeFault.ModelRejected(error.Message));

    public static Fin<SplatScan> DecodeSplat(string formatKey, ReadOnlyMemory<byte> bytes) =>
        formatKey is "spz" or "sog"
            ? Fin.Fail<SplatScan>(new ComputeFault.ModelRejected($"<splat-companion-pending:{formatKey}:python-realitycapture-spz-sog-decode-unadmitted>"))
            : Fin.Fail<SplatScan>(new ComputeFault.ModelRejected($"<residency-splat-format-miss:{formatKey}>"));

    static Fin<ResidencyPayload> MeshletEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        var sorted = MeshoptCodec.SpatialSort(leaf.Vertices.Span, leaf.VertexCount, policy.VertexStride);
        var built = MeshoptCodec.BuildMeshlets(leaf.Indices.Span, leaf.VertexCount, sorted, policy);
        if (built.Clusters.IsEmpty) { return Fin.Fail<ResidencyPayload>(new ComputeFault.ModelRejected($"<residency-meshlet-empty:{leaf.FormatKey}>")); }
        var bounded = MeshoptCodec.ComputeBounds(built, sorted, policy.VertexStride);
        var encodedV = MeshoptCodec.EncodeVertices(sorted, leaf.VertexCount, policy.VertexStride);
        var encodedI = MeshoptCodec.EncodeIndices(built.MeshletTriangles, built.TriangleTotal, leaf.VertexCount);
        var encodedM = MeshoptCodec.PackMeshlets(bounded);
        var key = InterchangeIdentity.Key(leaf.FormatKey, encodedV.Span, policy.ConeWeight, policy.SimplifyTarget, policy.QuantizationBits);
        return Fin.Succ(new ResidencyPayload(ResidencyKind.MeshletCluster, key, encodedV, encodedI, encodedM, bounded,
            bounded.Count, encodedV.Length + encodedI.Length + encodedM.Length));
    }

    static Fin<ResidencyPayload> QuantizedEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        var quantized = MeshoptCodec.QuantizeVertices(leaf.Vertices.Span, leaf.VertexCount, policy);
        var encodedV = MeshoptCodec.EncodeVerticesLevel(quantized, leaf.VertexCount, policy.VertexStride, policy.CodecLevel);
        var encodedI = MeshoptCodec.EncodeIndices(leaf.Indices.Span, leaf.TriangleCount * 3, leaf.VertexCount);
        var key = InterchangeIdentity.Key(leaf.FormatKey, encodedV.Span, policy.ConeWeight, policy.SimplifyTarget, policy.QuantizationBits);
        return Fin.Succ(new ResidencyPayload(ResidencyKind.QuantizedVertex, key, encodedV, encodedI, ReadOnlyMemory<byte>.Empty,
            Seq<ResidencyMeshlet>(), leaf.VertexCount, encodedV.Length + encodedI.Length));
    }

    static Fin<ResidencyPayload> PointEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        var simplified = MeshoptCodec.SimplifyPoints(leaf.Vertices.Span, leaf.VertexCount, leaf.Normals.Span, policy);
        var encodedV = MeshoptCodec.EncodeVerticesLevel(simplified.Positions, simplified.Count, policy.VertexStride, policy.CodecLevel);
        var key = InterchangeIdentity.Key(leaf.FormatKey, encodedV.Span, policy.ConeWeight, policy.SimplifyTarget, policy.QuantizationBits);
        return Fin.Succ(new ResidencyPayload(ResidencyKind.PointSplat, key, encodedV, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty,
            Seq<ResidencyMeshlet>(), simplified.Count, encodedV.Length));
    }

    static ResidencyPayload SplatEncode(SplatScan scan, ResidencyPolicy policy) {
        var interleaved = MeshoptCodec.InterleaveSplat(scan);
        int splatStride = (3 + 3 + 4 + (scan.HarmonicDegree + 1) * (scan.HarmonicDegree + 1) * 3) * sizeof(float);
        var encodedV = MeshoptCodec.EncodeVerticesLevel(interleaved, (int)scan.SplatCount, splatStride, policy.CodecLevel);
        var key = InterchangeIdentity.Key(scan.FormatKey, encodedV.Span, policy.ConeWeight, policy.SimplifyTarget, policy.QuantizationBits);
        return new ResidencyPayload(ResidencyKind.GaussianSplat, key, encodedV, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty,
            Seq<ResidencyMeshlet>(), (int)scan.SplatCount, encodedV.Length);
    }
}

public static unsafe class MeshoptCodec {
    public static float[] SpatialSort(ReadOnlySpan<float> vertices, int vertexCount, int stride) {
        var remap = new uint[vertexCount];
        fixed (uint* dst = remap)
        fixed (float* src = vertices) { Meshopt.SpatialSortRemap(dst, src, (nuint)vertexCount, (nuint)stride); }
        var sorted = new float[vertices.Length];
        int floats = stride / sizeof(float);
        for (int v = 0; v < vertexCount; v++) { vertices.Slice(v * floats, floats).CopyTo(sorted.AsSpan((int)remap[v] * floats)); }
        return sorted;
    }

    public static MeshletBuild BuildMeshlets(ReadOnlySpan<long> indices, int vertexCount, float[] sorted, ResidencyPolicy policy) {
        int indexCount = indices.Length;
        var indices32 = new uint[indexCount];
        for (int i = 0; i < indexCount; i++) { indices32[i] = (uint)indices[i]; }
        nuint maxMeshlets = default;
        fixed (uint* idx = indices32) { maxMeshlets = Meshopt.BuildMeshletsBound((nuint)indexCount, (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles); }
        var meshlets = new Meshlet[maxMeshlets];
        var meshletVertices = new uint[(int)maxMeshlets * policy.MaxVertices];
        var meshletTriangles = new byte[(int)maxMeshlets * policy.MaxTriangles * 3];
        nuint count = default;
        fixed (Meshlet* mlt = meshlets)
        fixed (uint* mv = meshletVertices)
        fixed (byte* mt = meshletTriangles)
        fixed (uint* idx = indices32)
        fixed (float* pos = sorted) {
            count = Meshopt.BuildMeshlets(mlt, mv, mt, idx, (nuint)indexCount, pos, (nuint)vertexCount, (nuint)policy.VertexStride,
                (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles, policy.ConeWeight);
        }
        int triangleTotal = 0;
        for (int m = 0; m < (int)count; m++) { triangleTotal += (int)meshlets[m].TriangleCount; }
        return new MeshletBuild(meshlets.AsSpan(0, (int)count).ToArray().ToSeq(), meshletVertices, meshletTriangles, triangleTotal);
    }

    public static Seq<ResidencyMeshlet> ComputeBounds(MeshletBuild build, float[] sorted, int stride) {
        var clusters = Seq<ResidencyMeshlet>();
        foreach (var meshlet in build.Meshlets) {
            Bounds bounds = default;
            fixed (uint* mv = &build.MeshletVertices[meshlet.VertexOffset])
            fixed (byte* mt = &build.MeshletTriangles[meshlet.TriangleOffset])
            fixed (float* pos = sorted) {
                bounds = Meshopt.ComputeMeshletBounds(mv, mt, meshlet.TriangleCount, pos, (nuint)(sorted.Length / (stride / sizeof(float))), (nuint)stride);
            }
            clusters = clusters.Add(new ResidencyMeshlet(
                (int)meshlet.VertexOffset, (int)meshlet.TriangleOffset, (int)meshlet.VertexCount, (int)meshlet.TriangleCount,
                [bounds.ConeApex[0], bounds.ConeApex[1], bounds.ConeApex[2]],
                [bounds.ConeAxis[0], bounds.ConeAxis[1], bounds.ConeAxis[2]],
                bounds.ConeCutoff, [bounds.Center[0], bounds.Center[1], bounds.Center[2]], bounds.Radius));
        }
        return clusters;
    }

    public static ReadOnlyMemory<byte> EncodeVertices(float[] sorted, int vertexCount, int stride) {
        nuint bound = default;
        fixed (float* src = sorted) { bound = Meshopt.EncodeVertexBufferBound((nuint)vertexCount, (nuint)stride); }
        var buffer = new byte[(int)bound];
        nuint written = default;
        fixed (byte* dst = buffer)
        fixed (float* src = sorted) { written = Meshopt.EncodeVertexBuffer(dst, bound, src, (nuint)vertexCount, (nuint)stride); }
        return buffer.AsMemory(0, (int)written);
    }

    public static ReadOnlyMemory<byte> EncodeVerticesLevel(float[] vertices, int vertexCount, int stride, int level) {
        nuint bound = default;
        fixed (float* src = vertices) { bound = Meshopt.EncodeVertexBufferBound((nuint)vertexCount, (nuint)stride); }
        var buffer = new byte[(int)bound];
        nuint written = default;
        fixed (byte* dst = buffer)
        fixed (float* src = vertices) { written = Meshopt.EncodeVertexBufferLevel(dst, bound, src, (nuint)vertexCount, (nuint)stride, level); }
        return buffer.AsMemory(0, (int)written);
    }

    public static ReadOnlyMemory<byte> EncodeIndices(ReadOnlySpan<byte> triangles, int triangleTotal, int vertexCount) {
        int indexCount = triangleTotal * 3;
        var indices32 = new uint[indexCount];
        for (int i = 0; i < indexCount && i < triangles.Length; i++) { indices32[i] = triangles[i]; }
        return EncodeIndices(indices32, indexCount, vertexCount);
    }

    public static ReadOnlyMemory<byte> EncodeIndices(ReadOnlySpan<long> indices, int indexCount, int vertexCount) {
        var indices32 = new uint[indexCount];
        for (int i = 0; i < indexCount; i++) { indices32[i] = (uint)indices[i]; }
        return EncodeIndices(indices32, indexCount, vertexCount);
    }

    static ReadOnlyMemory<byte> EncodeIndices(uint[] indices32, int indexCount, int vertexCount) {
        nuint bound = default;
        fixed (uint* idx = indices32) { bound = Meshopt.EncodeIndexBufferBound((nuint)indexCount, (nuint)vertexCount); }
        var buffer = new byte[(int)bound];
        nuint written = default;
        fixed (byte* dst = buffer)
        fixed (uint* idx = indices32) { written = Meshopt.EncodeIndexBuffer(dst, bound, idx, (nuint)indexCount); }
        return buffer.AsMemory(0, (int)written);
    }

    public static float[] QuantizeVertices(ReadOnlySpan<float> vertices, int vertexCount, ResidencyPolicy policy) {
        var quantized = new float[vertices.Length];
        int floats = policy.VertexStride / sizeof(float);
        for (int v = 0; v < vertexCount; v++) {
            for (int c = 0; c < floats; c++) {
                int slot = v * floats + c;
                quantized[slot] = c < 3
                    ? Meshopt.QuantizeFloat(vertices[slot], policy.QuantizationBits)
                    : Meshopt.DequantizeHalf(Meshopt.QuantizeHalf(vertices[slot]));
            }
        }
        return quantized;
    }

    public static PointSimplify SimplifyPoints(ReadOnlySpan<float> positions, int vertexCount, ReadOnlySpan<float> colors, ResidencyPolicy policy) {
        nuint target = (nuint)(vertexCount * policy.SimplifyTarget);
        var remap = new uint[(int)target];
        bool hasColor = colors.Length >= vertexCount * 3;
        fixed (uint* dst = remap)
        fixed (float* pos = positions)
        fixed (float* col = colors) {
            Meshopt.SimplifyPoints(dst, pos, (nuint)vertexCount, (nuint)policy.VertexStride,
                hasColor ? col : null, hasColor ? (nuint)(3 * sizeof(float)) : 0, (float)policy.ColorWeight, target);
        }
        int floats = policy.VertexStride / sizeof(float);
        var sampled = new float[(int)target * floats];
        for (int v = 0; v < (int)target; v++) { positions.Slice((int)remap[v] * floats, floats).CopyTo(sampled.AsSpan(v * floats)); }
        return new PointSimplify(sampled, (int)target);
    }

    public static float[] InterleaveSplat(SplatScan scan) {
        int harmonics = (scan.HarmonicDegree + 1) * (scan.HarmonicDegree + 1) * 3;
        int floats = 3 + 3 + 4 + harmonics;
        var buffer = new float[(int)scan.SplatCount * floats];
        var pos = scan.Positions.Span;
        var scale = scan.Scales.Span;
        var rot = scan.Rotations.Span;
        var sh = scan.Harmonics.Span;
        for (int s = 0; s < (int)scan.SplatCount; s++) {
            int slot = s * floats;
            pos.Slice(s * 3, 3).CopyTo(buffer.AsSpan(slot));
            scale.Slice(s * 3, 3).CopyTo(buffer.AsSpan(slot + 3));
            rot.Slice(s * 4, 4).CopyTo(buffer.AsSpan(slot + 6));
            sh.Slice(s * harmonics, harmonics).CopyTo(buffer.AsSpan(slot + 10));
        }
        return buffer;
    }

    public static ReadOnlyMemory<byte> PackMeshlets(Seq<ResidencyMeshlet> clusters) {
        const int rowBytes = 4 * sizeof(int) + 11 * sizeof(float);
        var buffer = new byte[clusters.Count * rowBytes];
        var sink = buffer.AsSpan();
        int cursor = 0;
        foreach (var cluster in clusters) {
            BinaryPrimitives.WriteInt32LittleEndian(sink[cursor..], cluster.VertexOffset);
            BinaryPrimitives.WriteInt32LittleEndian(sink[(cursor + 4)..], cluster.TriangleOffset);
            BinaryPrimitives.WriteInt32LittleEndian(sink[(cursor + 8)..], cluster.VertexCount);
            BinaryPrimitives.WriteInt32LittleEndian(sink[(cursor + 12)..], cluster.TriangleCount);
            var floats = sink[(cursor + 16)..];
            MemoryMarshal.Cast<float, byte>([
                cluster.ConeApex[0], cluster.ConeApex[1], cluster.ConeApex[2],
                cluster.ConeAxis[0], cluster.ConeAxis[1], cluster.ConeAxis[2], cluster.ConeCutoff,
                cluster.Center[0], cluster.Center[1], cluster.Center[2], cluster.Radius]).CopyTo(floats);
            cursor += rowBytes;
        }
        return buffer;
    }
}

public readonly record struct MeshletBuild(Seq<Meshlet> Meshlets, uint[] MeshletVertices, byte[] MeshletTriangles, int TriangleTotal) {
    public Seq<Meshlet> Clusters => Meshlets;
}

public readonly record struct PointSimplify(float[] Positions, int Count);
```

## [03]-[RESEARCH]

- [SPLAT_COMPANION_WIRE]: the SPZ/SOG gaussian-splat scan decode is owned by the Python `realitycapture` companion and crosses the `Runtime/channels#PROTO_VOCABULARY` `GeometryPayload`/`ArtifactSync` artifact seam — the companion-published splat-scan wire shape (position/scale/rotation/spherical-harmonic accessor spellings, harmonic-degree band, and the SPZ container framing) grounds the `SplatScan` member set and the `InterleaveSplat` stride at the companion-decode admission gate; C# owns only the `ResidencyPayload` content-keying and the `EncodeVertexBufferLevel` residence of the decoded scan bytes, `DecodeSplat` faults `splat-companion-pending` until the companion wire grounds, and the Python content-key reproduction of the `:x32` form stays gated on the `xxhash` cp315/abi3 wheel the companion lacks below 3.15.
- [MESHLET_TRIANGLE_PACK]: the `Meshopt.BuildMeshlets` meshlet triangle buffer is per-meshlet-local `byte` indices into the meshlet vertex table, so the `EncodeIndices` index-codec arm packs the resolved global indices the cluster vertex table maps; the meshlet-local-to-global resolution (the `Meshlet.VertexOffset`/`Meshlet.TriangleOffset` stride into `meshletVertices`/`meshletTriangles`) and the `Bounds` `ConeApex`/`ConeAxis`/`ConeCutoff`/`Center`/`Radius` field spellings ground against the `Alimer.Bindings.MeshOptimizer` `Meshlet`/`Bounds` struct layout at the meshlet-encode gate, and the `EncodeVertexBufferLevel` codec `level` band (the per-level compression-versus-decode-speed tradeoff) is the policy `CodecLevel` column owned here.

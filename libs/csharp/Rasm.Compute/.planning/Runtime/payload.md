# [COMPUTE_PAYLOAD]

Rasm.Compute streaming-residency lane: the content-keyed GPU-ready payload codec a web viewer streams cell-by-cell. Four encode arms ride one `ResidencyKind` axis — meshlet-cluster partitions an octree-leaf `ImportedGeometry` into cone-cullable clusters, quantized-vertex exponent-filters and level-compresses a leaf for a low-VRAM tile, point-splat decimates a reality-capture point set, and gaussian-splat octahedral/quaternion/exponent-filters a companion-decoded `SplatScan`. One `Encode` fold over the safe `Meshopt` span surface owns every arm, so a per-kind encoder sibling is the collapsed form. This lane produces payload bytes and the self-describing `StreamSpan` bufferView layout only, never a manifest or a scene-graph.

Payload bytes address through the suite `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` key, read the `Runtime/codecs#TILE_PARTITION` `ImportedGeometry` octree leaf (never a second partition), and ride the `Runtime/receipts#RECEIPT_UNION` `StreamSegment` slot (never a new receipt case). `csharp:Rasm.AppUi/Render/pipeline#TS_PROJECTION` `ResidencyManifest.Mint` mints the `WEB_GEOMETRY_RESIDENCY_WIRE` manifest once, projecting each payload 1:1 from its `StreamSpan` layout, `ResidencyMeshlet` clusters, and content key — a Compute-side `ResidencyManifest` is the named drift defect. Encoded blobs land content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` at the app-platform seam. Splat scans arrive from the Python `realitycapture` companion as `ArtifactFrame` bytes at the `Runtime/wire#PROTO_VOCABULARY` `ArtifactSync` seam, never an in-process splat fit or SPZ/SOG decoder. HOST-LOCAL, no TS_PROJECTION.

## [01]-[INDEX]

- [02]-[RESIDENCY]: `Residency.Encode` folds a `ResidencySource` onto its `ResidencyKind` row over the safe `Meshopt` span surface.

## [02]-[RESIDENCY]

- Owner: `ResidencyKind` `[SmartEnum<string>]` the one closed payload axis, each row's `ConeCullable`/`SplatBorne` columns telling the AppUi marshal which cull and shader to pick, so a new encoding is one row, never a per-kind payload type; `ResidencyStream`, `StreamMode`, `StreamFilter` the closed buffer-role, meshopt decode-mode, and attribute-filter axes whose keys ARE the `EXT_meshopt_compression` wire modes the manifest emits; `ResidencySource` `[Union]` the polymorphic encode input (`Leaf` for octree-leaf arms, `Splat` for a companion scan), so one entry discriminates on shape, never an `Encode`/`EncodeSplat` pair; `ResidencyMeshlet` the per-cluster cone-and-sphere descriptor carrying the cluster-LOD chain columns `Level`/`Parent`/`Error`/`ParentError` and the `Shell` connected-component column the parent link searches within; `FaceAdjacency` the shared-vertex-count-tagged triangle-adjacency edge the cut-minimizing build partitions over; `ResidencyPolicy` the encode-posture record; `ResidencyPayload` the content-keyed buffer carrier (blob, per-stream `StreamSpan` layout, clusters, bounding sphere, content key), not a manifest; `ResidencyRuns` the decoded per-vertex attribute-run carrier a host consumer indexes per primitive; `Residency` the static `Encode` fold with the `StreamSegment` `Receipt` projection and the paired `Runs` decode.
- Cases: `ResidencyKind` rows `meshlet-cluster` (cone-cullable cluster-LOD chain — global vertex stream, `EncodeIndexSequence` meshlet-vertex table, raw local triangle bytes, per-cluster descriptors across the `Meshopt.Simplify` levels `SimplifyTarget` drives) · `quantized-vertex` (exponent-filtered, level-compressed single tile) · `point-splat` (`SimplifyPoints`-decimated, exponent-filtered positions) · `gaussian-splat` (companion-decoded `SplatScan` — positions/scales/harmonics exponent-filter, rotation quaternions quaternion-filter, sigmoid-activated alphas raw).
- Entry: `public static Fin<ResidencyPayload> Encode(ResidencySource source, ResidencyPolicy policy)` projects a leaf (or companion scan) onto the kind's arm; `public static Fin<ResidencyRuns> Runs(ResidencyPayload payload)` is the ONE host-side attribute decode — meshlet-cluster only, each stream under its own `Layout` row, the `csharp:Rasm.AppUi/Render/meshlets#CLUSTER_CONSUMPTION` and `Render/pathtrace#BSDF_SHADING` `SurfaceAttribution` data source; `public static ComputeReceipt.StreamSegment Receipt(ResidencyPayload payload, CorrelationId correlation, WorkLane lane, Duration elapsed)` projects onto the settled slot; `Fin<T>` aborts onto `ComputeFault.PayloadOverBounds` for an empty meshlet build, an out-of-range quantization budget, an out-of-range simplify target, or a stream a decode rejects, and onto `ComputeFault.ModelRejected` for a leaf routed at a splat-borne kind or a non-cluster payload handed to `Runs`.
- Auto: `Encode` admits every policy and source extent before dispatching the `ResidencySource` union; the `Leaf` arm reads the kind's row-owned `LeafArm` `[UseDelegateFromConstructor]` column, so the joint source-kind decision has one dispatch level. Meshlet encoding clusters through the `ClusterBuild` row (`cone` = `BuildMeshlets`, `flex` = `BuildMeshletsFlex`, `spatial` = `BuildMeshletsSpatial`, `bisect` = the managed Kernighan-Lin recursion), reads the shell partition once off the union-find forest so every level's parent link stays inside one connected component and the ladder terminates at one meshlet PER SHELL, cache-optimizes the index buffer, and encodes the global vertices and the local-to-global meshlet indices while retaining raw local triangle bytes. Quantized, point, and splat arms filter their admitted attributes, and every stream carries its exact codec version through `StreamSpan.CodecVersion` before the whole blob keys through `InterchangeIdentity.Key`.
- Receipt: the `Runtime/receipts#RECEIPT_UNION` `StreamSegment(string ArtifactId, int Segments, long Bytes)` slot carries the payload `ArtifactKey`, the cluster count (meshlet) or stream count (other kinds), and the blob length — a re-encode of identical geometry at identical policy stamps the same content key, so emission is auditable through the existing slot, never a new case; the blob dedups on the Persistence blob lane through `ArtifactIndexRow.Admit` and a hit stamps a `Cache` receipt.
- Packages: Alimer.Bindings.MeshOptimizer, QuikGraph (`ForestDisjointSet<uint>` the shared-vertex shell partition, `KernighanLinAlgorithm` over `UndirectedGraph<int, FaceAdjacency>` the cut-minimizing cluster build), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Element (project — the seam `ImportedGeometry` leaf carrier), Rasm (project — the kernel `EncodedGeometry` arena with `Channel`/`Descriptors`, the `EncodingChannel` lane roster, and `ChannelDtype.Unpack`), BCL inbox
- Growth: a new encoding is one `ResidencyKind` row carrying its `LeafArm` delegate column; a new meshlet-build strategy is one `ClusterBuild` row whose `Native` column routes it to the pinned native kernel or the managed partition build, never a fork of `BuildClusters`; a new attribute is one `ResidencyStream` row with its filtered-stream line; a new filter or decode mode is one `StreamFilter`/`StreamMode` row on the `StreamSpan`; a new posture is one `ResidencyPolicy` column; a new source modality is one `ResidencySource` case; zero new surface — a `MeshletResidencyEncoder`/`SplatPayloadCodec`/`QuantizedVertexEncoder` sibling collapses onto the one `Encode` fold, and parallel `EncodedVertices`/`EncodedIndices`/`EncodedMeshlets` byte fields collapse onto the one `StreamSpan` layout.
- Boundary: every attribute read addresses the seam carrier by descriptor through one `Lane` reader, so a per-lane branch, a named column, or a literal component stride is the deleted form and a lane the roster grows reaches the encoder with no edit here. This lane owns the content-keyed payload blob and `StreamSpan`; `csharp:Rasm.AppUi/Render/pipeline#TS_PROJECTION` projects every byte window, codec mode, inverse filter, codec version, cluster, bound, and content key without re-derivation. Host-side attribute reads cross through `Runs` alone — AppUi indexes the decoded runs and grows no second stream decoder. `InterchangeIdentity.Key` covers the whole assembled blob and its byte-changing policy. Process-global index encoding pins through `EncodeIndexVersion`, vertex encoding carries `ResidencyPolicy.CodecVersion`, and raw meshlet triangles carry version `0`. Count-bearing native calls receive explicit semantic counts through pinned pointer kernels. Gaussian splat fitting and SPZ/SOG decoding remain companion-owned; point-cloud file readers remain the distinct `Runtime/codecs#FIELD_RESULT_CODEC` concern.

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
    public static readonly ClusterBuild ConeWeighted = new("cone", native: true);
    public static readonly ClusterBuild Flex = new("flex", native: true);
    public static readonly ClusterBuild Spatial = new("spatial", native: true);
    public static readonly ClusterBuild Bisect = new("bisect", native: false);

    // Meshopt's three builders are greedy forward scans: they minimize cone spread and fill, and pay whatever
    // vertex duplication the scan order produces. `Bisect` optimizes the OTHER cost — recursive Kernighan-Lin
    // bisection of the triangle-adjacency graph minimizes the shared-vertex CUT, so a cluster set built for a
    // bandwidth-bound stream carries fewer duplicated vertices at the same triangle budget. The column is what
    // routes the build, so the pinned native kernel never receives a managed row and never forks.
    public bool Native { get; }
}

// Triangle-adjacency edge for the bisection build: the partition algebra constrains its edge to the ordered
// undirected marker AND the double tag it reads as cut weight, so the shared-vertex COUNT rides the tag. The
// setter exists because the interface declares one; the build mints every edge whole and never re-tags.
public sealed class FaceAdjacency(int source, int target, double shared) : IUndirectedEdge<int>, ITagged<double> {
    public int Source { get; } = source;
    public int Target { get; } = target;

    public double Tag {
        get;
        set { if (field != value) { field = value; TagChanged?.Invoke(this, EventArgs.Empty); } }
    } = shared;

    public event EventHandler? TagChanged;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ResidencyStream {
    public static readonly ResidencyStream Positions = new("positions");
    public static readonly ResidencyStream Normals = new("normals");
    public static readonly ResidencyStream Uvs = new("uvs");
    public static readonly ResidencyStream Indices = new("indices");
    public static readonly ResidencyStream Triangles = new("triangles");
    public static readonly ResidencyStream Scales = new("scales");
    public static readonly ResidencyStream Rotations = new("rotations");
    public static readonly ResidencyStream Harmonics = new("harmonics");
    public static readonly ResidencyStream Alphas = new("alphas");
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
// Shell names the connected-component representative of the cluster's own triangles under the shared-vertex
// relation, and a parent link searches WITHIN one shell — so a fine cluster never binds a coarse parent from a
// disjoint piece of geometry whose sphere merely contains it, a cut that then draws two unrelated shells at once.
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
    int Shell,
    float Error,
    float ParentError);

// per-stream EXT_meshopt_compression bufferView: byte window, Count/ByteStride, decode Mode (attribute/triangle/
// index codec, or Raw for un-encoded meshlet triangle bytes), inverse Filter — the set the AppUi manifest emits
public readonly record struct StreamSpan(int Offset, int Length, int Count, int ByteStride, StreamMode Mode, StreamFilter Filter, int CodecVersion);

// exp-packed 3-component carrier (12 bytes) the meshopt exponent filter writes; never read back as floats here
public readonly record struct Packed12(uint A, uint B, uint C);

// Harmonics leads with the SH DC triple (the wire band width (degree+1)^2*3 counts it); Alphas carries the
// sigmoid-activated per-splat opacity in [0,1] — the renderer's direct input — appended past the frozen columns
// under the wire's additive-only law. The python companion composes both at its container fold.
public sealed record SplatScan(
    string FormatKey,
    ReadOnlyMemory<float> Positions,
    ReadOnlyMemory<float> Scales,
    ReadOnlyMemory<float> Rotations,
    ReadOnlyMemory<float> Harmonics,
    int HarmonicDegree,
    long SplatCount,
    ReadOnlyMemory<float> Alphas);

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

// The decoded per-vertex attribute runs a host consumer indexes per primitive — the data source behind the AppUi
// SurfaceAttribution real arm. Positions/Normals/Uvs run in GLOBAL vertex order; MeshletVertices is the decoded
// local-to-global vertex table and MeshletTriangles the raw local triangle bytes, so cluster-local triangle t
// corner c reads global vertex MeshletVertices[cluster.VertexOffset + MeshletTriangles[cluster.TriangleOffset +
// t*3 + c]]. An empty run is typed absence — a source with no normals or no unwrap decodes to empty, never a
// fabricated constant a consumer cannot tell from data.
public sealed record ResidencyRuns(
    ReadOnlyMemory<float> Positions,
    ReadOnlyMemory<float> Normals,
    ReadOnlyMemory<float> Uvs,
    ReadOnlyMemory<uint> MeshletVertices,
    ReadOnlyMemory<byte> MeshletTriangles);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Residency {
    const int PositionStride = 3 * sizeof(float);
    const int UvStride = 2 * sizeof(float);
    const int OctBits = 8;
    const int IndexCodecVersion = 1;
    // KL refinement passes per bisection: the algorithm seeds an arbitrary halving, so a single pass leaves
    // obvious swaps unmade while the gain sequence flattens well before the part size the budget admits.
    const int BisectionPasses = 4;

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

    // The decode projection PAIRED with the meshlet encode — the one host-side attribute reader, so AppUi never
    // grows a second stream decoder. Each stream decodes under its own Layout row: the vertex codec for
    // attribute runs, the index-sequence codec for the meshlet vertex table, raw triangle bytes verbatim; the
    // octahedral normal filter unpacks in place before the snorm8 lanes widen to unit floats. MESHLET-CLUSTER
    // ONLY: the quantized/point/splat kinds ship exponent-filtered streams whose consumer is the web viewer's
    // meshopt decoder, and a host read of those kinds is a routing defect this gate names rather than absorbs.
    public static Fin<ResidencyRuns> Runs(ResidencyPayload payload) {
        if (payload.Kind != ResidencyKind.MeshletCluster) {
            return Fin.Fail<ResidencyRuns>(new ComputeFault.ModelRejected($"<residency-runs-kind:{payload.Kind.Key}>"));
        }
        ReadOnlySpan<byte> blob = payload.Blob.Span;
        Vector3[] positions = new Vector3[Count(payload, ResidencyStream.Positions)];
        uint[] packedNormals = new uint[Count(payload, ResidencyStream.Normals)];
        Vector2[] uvs = new Vector2[Count(payload, ResidencyStream.Uvs)];
        uint[] table = new uint[Count(payload, ResidencyStream.Indices)];
        int status = Decode<Vector3>(payload, ResidencyStream.Positions, positions, blob)
                   | Decode<uint>(payload, ResidencyStream.Normals, packedNormals, blob)
                   | Decode<Vector2>(payload, ResidencyStream.Uvs, uvs, blob)
                   | Decode<uint>(payload, ResidencyStream.Indices, table, blob);
        if (status != 0) { return Fin.Fail<ResidencyRuns>(new ComputeFault.PayloadOverBounds($"<residency-runs-decode:{payload.ArtifactKey}>")); }
        if (packedNormals.Length > 0) { Meshopt.DecodeFilterOct<uint>(packedNormals); }
        ReadOnlyMemory<byte> triangles = payload.Layout.TryGetValue(ResidencyStream.Triangles, out StreamSpan raw)
            ? blob.Slice(raw.Offset, raw.Length).ToArray()
            : ReadOnlyMemory<byte>.Empty;
        return Fin.Succ(new ResidencyRuns(
            MemoryMarshal.Cast<Vector3, float>(positions).ToArray(),
            UnpackSnorm(packedNormals),
            MemoryMarshal.Cast<Vector2, float>(uvs).ToArray(),
            table,
            triangles));
    }

    static int Count(ResidencyPayload payload, ResidencyStream stream) =>
        payload.Layout.TryGetValue(stream, out StreamSpan span) ? span.Count : 0;

    // One decode body, the stream's own Mode selecting the codec; an absent stream decodes nothing and reports
    // clean, so the fold above ORs real statuses only.
    static int Decode<T>(ResidencyPayload payload, ResidencyStream stream, Span<T> destination, ReadOnlySpan<byte> blob) where T : unmanaged =>
        destination.Length > 0 && payload.Layout.TryGetValue(stream, out StreamSpan span)
            ? span.Mode == StreamMode.Indices
                ? Meshopt.DecodeIndexSequence(destination, blob.Slice(span.Offset, span.Length))
                : Meshopt.DecodeVertexBuffer(destination, blob.Slice(span.Offset, span.Length))
            : 0;

    // The OctBits=8 encode stores snorm8 lanes; the filter decode rehydrates them in place and this widening
    // lifts the three component lanes to unit floats — the fourth lane is the filter's reconstruction slot,
    // never data.
    static float[] UnpackSnorm(ReadOnlySpan<uint> packed) {
        float[] wide = new float[packed.Length * 3];
        ReadOnlySpan<sbyte> lanes = MemoryMarshal.Cast<uint, sbyte>(packed);
        for (int v = 0; v < packed.Length; v++) {
            (wide[v * 3], wide[(v * 3) + 1], wide[(v * 3) + 2]) =
                (lanes[v * 4] / 127f, lanes[(v * 4) + 1] / 127f, lanes[(v * 4) + 2] / 127f);
        }
        return wide;
    }

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
                    : Fin.Fail<ResidencySource>(new ComputeFault.PayloadOverBounds($"<residency-leaf-shape:{leaf.Geometry.VertexCount}:{leaf.Geometry.Lanes.Count}:{leaf.Geometry.Indices.Length}>")),
            splat: static splat => SplatShapeValid(splat.Scan)
                ? Fin.Succ<ResidencySource>(splat)
                : Fin.Fail<ResidencySource>(new ComputeFault.PayloadOverBounds($"<residency-splat-shape:{splat.Scan.SplatCount}:{splat.Scan.HarmonicDegree}>")));
        return faults.IsEmpty
            ? admittedSource.Map(admitted => (admitted, policy))
            : Fin.Fail<(ResidencySource, ResidencyPolicy)>(new ComputeFault.PayloadOverBounds($"<residency-policy:{string.Join('|', faults)}>"));
    }

    static bool LeafShapeValid(ResidencySource.Leaf leaf) {
        ImportedGeometry geometry = leaf.Geometry;
        // Per-lane extent is the arena's own claim, so admission proves only the cross-shape census the arena cannot —
        // declared element count and vertex count must be one number.
        bool attributes = geometry.VertexCount > 0 && geometry.Lanes.Count == geometry.VertexCount;
        if (!attributes || leaf.Kind == ResidencyKind.PointSplat || leaf.Kind.SplatBorne) { return attributes; }
        if (geometry.Indices.Length < 3 || geometry.Indices.Length % 3 != 0) { return false; }
        foreach (long index in geometry.Indices.Span) {
            if (index < 0 || index >= geometry.VertexCount || index > uint.MaxValue) { return false; }
        }
        return true;
    }

    static bool SplatShapeValid(SplatScan scan) {
        if (scan.SplatCount is <= 0 or > int.MaxValue || scan.HarmonicDegree is < 0 or > 3) { return false; }    // wire law: harmonic_degree is the SH band 0-3, byte-mirrored from GaussianSplatScan
        long degreeWidth = (long)scan.HarmonicDegree + 1;
        long width = degreeWidth * degreeWidth * 3;
        return scan.Positions.Length / 3 >= scan.SplatCount
            && scan.Scales.Length / 3 >= scan.SplatCount
            && scan.Rotations.Length / 4 >= scan.SplatCount
            && width > 0
            && scan.Harmonics.Length / width >= scan.SplatCount
            && scan.Alphas.Length >= scan.SplatCount;
    }

    internal static Fin<ResidencyPayload> SplatBorneLeafRejected(ImportedGeometry leaf, ResidencyPolicy policy) =>
        Fin.Fail<ResidencyPayload>(new ComputeFault.ModelRejected($"<residency-splat-needs-scan:{leaf.FormatKey}>"));

    internal static Fin<ResidencyPayload> MeshletEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        float[] positions = Lane(leaf.Lanes, EncodingChannel.Position);
        uint[] optimized = new uint[leaf.Indices.Length];
        Meshopt.OptimizeVertexCache(optimized, ToUInt(leaf.Indices.Span), (nuint)leaf.VertexCount);
        nuint maxMeshlets = Meshopt.BuildMeshletsBound((nuint)optimized.Length, (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles);
        Meshlet[] meshlets = new Meshlet[(int)maxMeshlets];
        uint[] meshletVertices = new uint[(int)maxMeshlets * policy.MaxVertices];
        byte[] meshletTriangles = new byte[(int)maxMeshlets * policy.MaxTriangles * 3];
        (Func<uint, int> Of, int Count) shells = Shells(optimized);
        int count = BuildClusters(optimized, positions, leaf.VertexCount, policy, meshlets, meshletVertices, meshletTriangles);
        if (count == 0) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-meshlet-empty:{leaf.FormatKey}>")); }
        List<ResidencyMeshlet> clusters = new(count);
        for (int m = 0; m < count; m++) {
            ref readonly Meshlet meshlet = ref meshlets[m];
            Span<uint> localVertices = meshletVertices.AsSpan((int)meshlet.vertex_offset, (int)meshlet.vertex_count);
            Span<byte> localTriangles = meshletTriangles.AsSpan((int)meshlet.triangle_offset, (int)meshlet.triangle_count * 3);
            Meshopt.OptimizeMeshlet(localVertices, localTriangles, meshlet.triangle_count, meshlet.vertex_count);
            clusters.Add(Cluster(meshlet, ClusterBounds(localVertices, localTriangles, (int)meshlet.triangle_count, positions, leaf.VertexCount), level: 0, error: 0f, shell: shells.Of(localVertices[0])));
        }
        ref readonly Meshlet tail = ref meshlets[count - 1];
        int usedVertices = (int)(tail.vertex_offset + tail.vertex_count);
        int usedTriangleBytes = (int)(tail.triangle_offset + tail.triangle_count * 3);
        (Seq<ResidencyMeshlet> Clusters, uint[] Vertices, byte[] Triangles) chained = LodChain(
            optimized, positions, leaf.VertexCount, policy, clusters, shells,
            meshletVertices.AsSpan(0, usedVertices), meshletTriangles.AsSpan(0, usedTriangleBytes));
        Seq<(ResidencyStream Stream, StreamMode Mode, StreamFilter Filter, int Count, int ByteStride, int CodecVersion, ReadOnlyMemory<byte> Bytes)> streams = Seq(
            (ResidencyStream.Positions, StreamMode.Attributes, StreamFilter.None, leaf.VertexCount, PositionStride, policy.CodecVersion, EncodeVertices(positions, leaf.VertexCount, policy)),
            (ResidencyStream.Indices, StreamMode.Indices, StreamFilter.None, chained.Vertices.Length, sizeof(uint), IndexCodecVersion, EncodeSequence(chained.Vertices, leaf.VertexCount)),
            (ResidencyStream.Triangles, StreamMode.Raw, StreamFilter.None, chained.Triangles.Length, 1, 0, chained.Triangles));
        if (HasNormals(leaf)) { streams = streams.Add((ResidencyStream.Normals, StreamMode.Attributes, StreamFilter.Octahedral, leaf.VertexCount, sizeof(uint), policy.CodecVersion, EncodeNormals(Lane(leaf.Lanes, EncodingChannel.Normal), leaf.VertexCount, policy))); }
        if (HasUvs(leaf)) { streams = streams.Add((ResidencyStream.Uvs, StreamMode.Attributes, StreamFilter.None, leaf.VertexCount, UvStride, policy.CodecVersion, EncodeUvs(Lane(leaf.Lanes, EncodingChannel.Uv), leaf.VertexCount, policy))); }
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
        (Func<uint, int> Of, int Count) shells,
        ReadOnlySpan<uint> level0Vertices,
        ReadOnlySpan<byte> level0Triangles) {
        List<ResidencyMeshlet> all = new(level0);
        List<uint> vertices = new(level0Vertices.ToArray());
        List<byte> triangles = new(level0Triangles.ToArray());
        float scale = Meshopt.SimplifyScale(positions, (nuint)vertexCount, PositionStride);
        uint[] current = indices;
        int level = 0, firstOfLevel = 0, countOfLevel = level0.Count;
        while (countOfLevel > shells.Count) {
            uint[] simplified = new uint[current.Length];
            nuint target = (nuint)Math.Max(3, (long)(current.Length * policy.SimplifyTarget) / 3 * 3);
            nuint written = Meshopt.Simplify(simplified, current, positions, (nuint)vertexCount, PositionStride, target, targetError: float.MaxValue, options: 0, out float resultError);
            if (written >= (nuint)current.Length || written < 3) { break; }
            Array.Resize(ref simplified, (int)written);
            level++;
            float objectError = resultError * scale;
            (int coarse, int coarseFirst) = ClusterLevel(simplified, positions, vertexCount, policy, all, vertices, triangles, level, objectError, shells.Of);
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
                if (all[c].Shell != all[f].Shell) { continue; }
                float d = Vector3.Distance(all[f].Center, all[c].Center);
                if (d < nearestDistance) { nearestDistance = d; nearest = c; }
                if (d + all[f].Radius <= all[c].Radius && d < coveringDistance) { coveringDistance = d; covering = c; }
            }
            if (nearestDistance is float.MaxValue) { continue; }
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
            (ResidencyStream.Positions, StreamMode.Attributes, StreamFilter.Exponential, leaf.VertexCount, PositionStride, policy.CodecVersion, EncodeExp(positions, leaf.VertexCount, policy)),
            (ResidencyStream.Indices, StreamMode.Triangles, StreamFilter.None, leaf.Indices.Length, sizeof(uint), IndexCodecVersion, EncodeTriangles(ToUInt(leaf.Indices.Span), leaf.VertexCount)));
        if (HasNormals(leaf)) { streams = streams.Add((ResidencyStream.Normals, StreamMode.Attributes, StreamFilter.Octahedral, leaf.VertexCount, sizeof(uint), policy.CodecVersion, EncodeNormals(Lane(leaf.Lanes, EncodingChannel.Normal), leaf.VertexCount, policy))); }
        if (HasUvs(leaf)) { streams = streams.Add((ResidencyStream.Uvs, StreamMode.Attributes, StreamFilter.None, leaf.VertexCount, UvStride, policy.CodecVersion, EncodeUvs(Lane(leaf.Lanes, EncodingChannel.Uv), leaf.VertexCount, policy))); }
        return Fin.Succ(Assemble(ResidencyKind.QuantizedVertex, leaf.FormatKey, streams, Seq<ResidencyMeshlet>(), leaf.VertexCount, SphereBounds(positions, leaf.VertexCount), 0, policy));
    }

    internal static Fin<ResidencyPayload> PointEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        if (policy.SimplifyTarget is <= 0 or > 1) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-simplify-target:{policy.SimplifyTarget:R}>")); }
        int target = Math.Max(1, (int)(leaf.VertexCount * policy.SimplifyTarget));
        uint[] remap = new uint[target];
        int kept = DecimatePoints(remap, positions, leaf.VertexCount,
            Has(leaf, EncodingChannel.Normal) ? Lane(leaf.Lanes, EncodingChannel.Normal) : [], policy.AttributeWeight, target);
        float[] gathered = new float[kept * 3];
        float[] source = positions;
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
            (ResidencyStream.Harmonics, StreamMode.Attributes, StreamFilter.Exponential, n * shFloats, sizeof(uint), policy.CodecVersion, EncodeStream<uint>(harmonics, policy.CodecLevel, policy.CodecVersion)),
            // opacity crosses raw: a [0,1] scalar gains nothing from the exponent filter's shared-component pass,
            // and the renderer reads it verbatim beside the filtered attribute streams.
            (ResidencyStream.Alphas, StreamMode.Attributes, StreamFilter.None, n, sizeof(float), policy.CodecVersion, EncodeStream<float>(scan.Alphas.Span[..n], policy.CodecLevel, policy.CodecVersion)));
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

    static ReadOnlyMemory<byte> EncodeUvs(ReadOnlySpan<float> uvs, int count, ResidencyPolicy policy) =>
        EncodeStream(MemoryMarshal.Cast<float, Vector2>(uvs[..(count * 2)]), policy.CodecLevel, policy.CodecVersion);

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

    // ONE descriptor-addressed lane reader serves every channel: the descriptor names the dtype, so a unorm8 colour
    // and a float32 position lift through the same call and no arm carries a literal component stride. An absent
    // channel answers the empty array — a MISSING DESCRIPTOR, never a zero-filled buffer a consumer length-probes.
    static float[] Lane(EncodedGeometry arena, EncodingChannel channel) {
        if (arena.Descriptors.Find(descriptor => descriptor.Channel == channel).Case is not EncodingChannelDescriptor found) { return []; }
        float[] raw = new float[found.Floats];
        found.Dtype.Unpack(arena.Channel(channel).Span, raw);
        return raw;
    }

    static bool Has(ImportedGeometry leaf, EncodingChannel channel) =>
        leaf.Lanes.Descriptors.Exists(descriptor => descriptor.Channel == channel);

    static bool HasNormals(ImportedGeometry leaf) => Has(leaf, EncodingChannel.Normal);

    static bool HasUvs(ImportedGeometry leaf) => Has(leaf, EncodingChannel.Uv);

    static ResidencyMeshlet Cluster(in Meshlet meshlet, Bounds bounds, int level, float error, int shell) {
        ReadOnlySpan<float> f = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<Bounds, float>(ref bounds), 11);
        return new ResidencyMeshlet(
            (int)meshlet.vertex_offset, (int)meshlet.triangle_offset, (int)meshlet.vertex_count, (int)meshlet.triangle_count,
            new Vector3(f[0], f[1], f[2]), f[3], new Vector3(f[4], f[5], f[6]), new Vector3(f[7], f[8], f[9]), f[10],
            Level: level, Parent: -1, Shell: shell, Error: error, ParentError: float.PositiveInfinity);
    }

    // Shared-vertex connectivity over the index buffer through the admitted union-find forest: each triangle
    // unions its three corners, so `FindSet` answers the component representative for any vertex and the cluster's
    // first corner names its shell. `SetCount` is the shell census the ladder reads — one meshlet remaining per
    // shell is the honest ladder terminal, where a global count-of-one never terminates a multi-shell mesh.
    static (Func<uint, int> Of, int Count) Shells(ReadOnlySpan<uint> indices) {
        ForestDisjointSet<uint> forest = new(indices.Length);
        foreach (uint corner in indices) { if (!forest.Contains(corner)) { forest.MakeSet(corner); } }
        for (int t = 0; t + 2 < indices.Length; t += 3) {
            forest.Union(indices[t], indices[t + 1]);
            forest.Union(indices[t], indices[t + 2]);
        }

        Dictionary<uint, int> ordinals = new();
        foreach (uint corner in indices) {
            uint representative = forest.FindSet(corner);
            if (!ordinals.ContainsKey(representative)) { ordinals[representative] = ordinals.Count; }
        }

        return (vertex => ordinals[forest.FindSet(vertex)], forest.SetCount);
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
        float objectError,
        Func<uint, int> shell) {
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
            ResidencyMeshlet cluster = Cluster(meshlet, ClusterBounds(localVertices, localTriangles, (int)meshlet.triangle_count, positions, vertexCount), level, objectError, shell(localVertices[0]));
            all.Add(cluster with { VertexOffset = vertexBase + cluster.VertexOffset, TriangleOffset = triangleBase + cluster.TriangleOffset });
        }
        if (count > 0) {
            ref readonly Meshlet tail = ref meshlets[count - 1];
            payloadVertices.AddRange(vertices.AsSpan(0, (int)(tail.vertex_offset + tail.vertex_count)).ToArray());
            payloadTriangles.AddRange(triangles.AsSpan(0, (int)(tail.triangle_offset + tail.triangle_count * 3)).ToArray());
        }
        return (count, first);
    }

    // Managed cut-minimizing build: the triangle-adjacency graph carries one vertex per triangle and one edge per
    // shared-vertex pair TAGGED with the count of vertices that pair shares, so the Kernighan-Lin cut cost IS the
    // duplicated-vertex count the stream pays. Recursive bisection descends while a part exceeds the triangle
    // budget; a part at or under budget emits one meshlet through the same local-vertex table and raw local
    // triangle bytes the native builds write, so every consumer downstream of `BuildClusters` reads one shape.
    static int BisectClusters(ReadOnlySpan<uint> indices, ResidencyPolicy policy,
        Meshlet[] meshlets, uint[] meshletVertices, byte[] meshletTriangles) {
        Seq<int> faces = toSeq(Enumerable.Range(0, indices.Length / 3));
        uint[] corners = indices.ToArray();
        (int Meshlets, int Vertices, int Triangles) cursor = (0, 0, 0);
        foreach (Seq<int> part in Bisected(faces, corners, policy)) {
            cursor = Emit(part, corners, policy, meshlets, meshletVertices, meshletTriangles, cursor);
        }

        return cursor.Meshlets;
    }

    // One bisection level: an over-budget part splits on the balanced minimum cut and recurses, a within-budget
    // part is a leaf. `nbIterations` is the KL pass count — a single pass leaves obvious swaps unmade on a part
    // whose initial halves the algorithm chose arbitrarily.
    static Seq<Seq<int>> Bisected(Seq<int> faces, uint[] corners, ResidencyPolicy policy) {
        if (faces.Count <= policy.MaxTriangles) { return Seq1(faces); }
        UndirectedGraph<int, FaceAdjacency> adjacency = new(allowParallelEdges: false);
        adjacency.AddVertexRange(faces);
        adjacency.AddEdgeRange(Adjacencies(faces, corners));
        KernighanLinAlgorithm<int, FaceAdjacency> partition = new(adjacency, nbIterations: BisectionPasses);
        partition.Compute();
        Seq<int> left = toSeq(partition.Partition.VertexSetA);
        Seq<int> right = toSeq(partition.Partition.VertexSetB);
        // Recursion stops on a degenerate cut (one side empty) — the part is disconnected past the budget, so it
        // splits on face order rather than looping on the same graph forever.
        return left.IsEmpty || right.IsEmpty
            ? Bisected(toSeq(faces.Take(faces.Count / 2)), corners, policy) + Bisected(toSeq(faces.Skip(faces.Count / 2)), corners, policy)
            : Bisected(left, corners, policy) + Bisected(right, corners, policy);
    }

    // Tag carries the cut weight: two faces sharing two vertices cost twice a pair sharing one, so a minimized cut
    // is a minimized duplicated-vertex count rather than a minimized face-pair count.
    static Seq<FaceAdjacency> Adjacencies(Seq<int> faces, uint[] corners) =>
        toSeq(faces
            .Bind(face => Seq(corners[face * 3], corners[(face * 3) + 1], corners[(face * 3) + 2]).Map(corner => (Corner: corner, Face: face)))
            .GroupBy(static entry => entry.Corner)
            .Bind(static shared => Pairs(toSeq(shared).Map(static entry => entry.Face)))
            .GroupBy(static pair => pair)
            .Map(static group => new FaceAdjacency(group.Key.Low, group.Key.High, group.Count())));

    static Seq<(int Low, int High)> Pairs(Seq<int> faces) =>
        faces.Head.Match(
            Some: head => faces.Tail.Map(other => (Low: Math.Min(head, other), High: Math.Max(head, other))) + Pairs(faces.Tail),
            None: () => Seq<(int, int)>());

    // One leaf part becomes one meshlet in the native builders' own layout: a local-to-global vertex table with
    // each global corner appearing once, and raw local triangle bytes indexing it. Writing the same three buffers
    // is what lets the shared post-build fold, the bounds kernel, and the stream encode stay row-agnostic.
    static (int Meshlets, int Vertices, int Triangles) Emit(
        Seq<int> part, uint[] corners, ResidencyPolicy policy,
        Meshlet[] meshlets, uint[] meshletVertices, byte[] meshletTriangles,
        (int Meshlets, int Vertices, int Triangles) cursor) {
        Dictionary<uint, byte> local = new(policy.MaxVertices);
        int triangle = 0;
        foreach (int face in part) {
            for (int c = 0; c < 3; c++) {
                uint global = corners[(face * 3) + c];
                if (!local.TryGetValue(global, out byte slot)) {
                    slot = (byte)local.Count;
                    local[global] = slot;
                    meshletVertices[cursor.Vertices + slot] = global;
                }

                meshletTriangles[cursor.Triangles + (triangle * 3) + c] = slot;
            }

            triangle++;
        }

        meshlets[cursor.Meshlets] = new Meshlet {
            vertex_offset = (uint)cursor.Vertices,
            triangle_offset = (uint)cursor.Triangles,
            vertex_count = (uint)local.Count,
            triangle_count = (uint)triangle,
        };
        return (cursor.Meshlets + 1, cursor.Vertices + policy.MaxVertices, cursor.Triangles + (policy.MaxTriangles * 3));
    }

    // Safe span overloads pass element-span length as the semantic vertex/triangle/point count (wrong for
    // interleaved-float positions and 3-byte triangles), so these four count-bearing builds pin and pass true
    // counts; the ClusterBuild row resolves by identity INSIDE the fixed block because meshlet pointers cannot
    // cross a generated-Switch lambda — the pinned kernel is the named exemption carrying this one row branch.
    static unsafe int BuildClusters(ReadOnlySpan<uint> indices, ReadOnlySpan<float> positions, int vertexCount, ResidencyPolicy policy,
        Meshlet[] meshlets, uint[] meshletVertices, byte[] meshletTriangles) {
        if (!policy.Cluster.Native) { return BisectClusters(indices, policy, meshlets, meshletVertices, meshletTriangles); }
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

- [SPLAT_COMPANION_WIRE]-[BLOCKED]: the companion decode owner is landed WHOLE for SPZ — `python:geometry/scan/ingestion#SCAN` (`ScanOp.splat` over raw container bytes, the `_container` preamble-and-TOC reader, the channel fold composing the DC triple onto the harmonic head and the sigmoid alpha column, the shape gate re-implementing `SplatShapeValid`'s law at the producing end, content-keyed `ArtifactFrame` bytes on the `ArtifactSync` seam). The remaining question narrows to SOG v2 alone — the companion's `[SOG_V2_LAYOUT]` research row: per-plane image codecs versus this fold's packed-block offset model; the byte-mirror against `SplatScan`/`GaussianSplatScan` verifies once that row resolves.

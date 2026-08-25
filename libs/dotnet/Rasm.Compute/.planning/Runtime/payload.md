# [COMPUTE_PAYLOAD]

Rasm.Compute streaming-residency lane: the content-keyed GPU-ready payload codec a web viewer streams cell-by-cell. Four encode arms ride one `ResidencyKind` axis — meshlet-cluster partitions an octree-leaf `ImportedGeometry` into cone-cullable clusters, quantized-vertex exponent-filters and level-compresses a leaf for a low-VRAM tile, point-splat decimates a reality-capture point set, and gaussian-splat octahedral/quaternion/exponent-filters a companion-decoded `SplatScan`. One `Encode` fold over the safe `Meshopt` span surface owns every arm, so a per-kind encoder sibling is the collapsed form. This lane produces payload bytes and the self-describing `StreamSpan` bufferView layout only, never a manifest or a scene-graph.

Payload bytes address through the suite `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` key, read the `Runtime/tiles#TILE_PARTITION` `ImportedGeometry` octree leaf (never a second partition), and settle as one `Model/run#RUN_MODES` `Streamed` value (never a second stream tally). `dotnet:Rasm.AppUi/Render/pipeline#TS_PROJECTION` `ResidencyMap.Mint` projects each payload's `StreamSpan` layout, `ResidencyMeshlet` clusters, and content key directly into generated `Render.GeometryResidency`; a Compute-side manifest or generated-message mirror is the named drift defect. Encoded blobs land content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` at the app-platform seam. Splat scans arrive from the Python `realitycapture` companion as `ArtifactFrame` bytes at the `Runtime/wire#PROTO_VOCABULARY` `ArtifactService.Fetch` seam, never an in-process splat fit or SPZ/SOG decoder. HOST-LOCAL, no TS_PROJECTION.

## [01]-[INDEX]

- [02]-[RESIDENCY]: `Residency.Encode` folds a `ResidencySource` onto its `ResidencyKind` row over the safe `Meshopt` span surface.

## [02]-[RESIDENCY]

- Owner: `ResidencyKind` `[SmartEnum<string>]` the one closed payload axis, each row's `ConeCullable`/`SplatBorne` columns telling the AppUi marshal which cull and shader to pick, so a new encoding is one row, never a per-kind payload type; `ResidencyStream`, `StreamMode`, `StreamFilter` the closed buffer-role, meshopt decode-mode, and attribute-filter axes whose keys ARE the `EXT_meshopt_compression` wire modes the manifest emits; `ResidencySource` `[Union]` the polymorphic encode input (`Leaf` for octree-leaf arms, `Splat` for a companion scan), so one entry discriminates on shape, never an `Encode`/`EncodeSplat` pair; `ResidencyMeshlet` the per-cluster cone-and-sphere descriptor carrying the cluster-LOD chain columns `Level`, the `Option`-shaped `Parent`/`ParentError` a root simply lacks, `Error`, the `Shell` connected-component column the parent link searches within, the `Curvature` normal-variation bound measured off the cluster's own triangles, and the `Cut` realized shared-boundary-vertex count every build row fills; `FaceAdjacency` the shared-vertex-count-tagged triangle-adjacency edge the cut-minimizing build partitions over; `ResidencyPolicy` the encode-posture record carrying the complete ordered `Vector` every content key folds; `StreamForm`/`StreamDraft` the `(kind, stream) -> form` policy table and the measured-only draft each arm supplies; `AdmittedResidency` the evidence carrier `Admit` mints and every arm takes; `ShellCensus` the shell ordinal table and its count; `CostAxis` the build-objective column; `ResidencyPayload` the content-keyed buffer carrier (blob, per-stream `StreamSpan` layout, clusters, bounding sphere, content key) whose constructor is private so `Assemble` is its one mint, not a manifest; `ResidencyRuns` the decoded per-vertex attribute-run carrier a host consumer indexes per primitive; `Residency` the static `Encode` fold returning `ResidencyPayload`, with the paired `Runs` decode.
- Cases: `ResidencyKind` rows `meshlet-cluster` (cone-cullable cluster-LOD chain — global vertex stream, `EncodeIndexSequence` meshlet-vertex table, raw local triangle bytes, per-cluster descriptors across the `Meshopt.Simplify` levels `SimplifyTarget` drives) · `quantized-vertex` (exponent-filtered, level-compressed single tile) · `point-splat` (`SimplifyPoints`-decimated, exponent-filtered positions) · `gaussian-splat` (companion-decoded `SplatScan` — positions/scales/harmonics exponent-filter, rotation quaternions quaternion-filter, sigmoid-activated alphas raw).
- Entry: `public static Fin<ResidencyPayload> Encode(ResidencySource source, ResidencyPolicy policy)` projects a leaf (or companion scan) onto the kind's arm; `public static Fin<ResidencyRuns> Runs(ResidencyPayload payload)` is the ONE host-side attribute decode — meshlet-cluster only, each stream under its own `Layout` row, the `dotnet:Rasm.AppUi/Render/meshlets#CLUSTER_CONSUMPTION` and `Render/pathtrace#BSDF_SHADING` `SurfaceAttribution` data source; `public static Streamed Streamed(ResidencyPayload payload)` projects the payload onto the settled stream value; `Fin<T>` aborts onto `PayloadOverBounds` for an empty meshlet build, an out-of-range quantization budget, an out-of-range simplify target, an absent mandatory stream, or a stream a decode rejects, while source-kind contract refusals use the typed `ComputeViolation` arms. `public static Fin<Unit> Mount()` pins the process-global index-codec version once at the composition root.
- Auto: `Encode` accumulates every policy bound and the source's own shape census — each `ResidencySource` case carrying its own `Check`, so a third modality cannot land without one — into one `AdmittedResidency` before dispatching the union; the `Leaf` arm reads the kind's row-owned `LeafArm` `[UseDelegateFromConstructor]` column, so the joint source-kind decision has one dispatch level. Meshlet encoding clusters through the `ClusterBuild` row's own kernel ORDINAL (`0` = `BuildMeshlets`, `1` = `BuildMeshletsFlex`, `2` = `BuildMeshletsSpatial`, `Managed` = the Kernighan-Lin recursion) under a closed switch, so a row without an ordinal fails to construct rather than falling into the cone-weighted scan, reads the shell partition once off the union-find forest so every level's parent link stays inside one connected component and the ladder terminates at one meshlet PER SHELL, measures each cluster's curvature bound and its realized `Cut` at the one `Cluster` projection off the local triangles the bounds kernel already reads and the level's own cluster-incidence census — every build row, every ladder level, which is what makes the greedy native scans and the cut-minimizing bisection comparable on the figure that decides stream cost — cache-optimizes the index buffer, and encodes the global vertices and the local-to-global meshlet indices while retaining raw local triangle bytes. Quantized, point, and splat arms filter their admitted attributes, every stream resolves its mode, filter, stride, and codec version off the ONE `(kind, stream)` form table rather than a per-arm literal tuple, and the whole blob keys through `InterchangeIdentity.Key` over `ResidencyPolicy.Vector` — every output-affecting column in owner order, so two payloads built at different cluster budgets, codec levels, or attribute weights cannot key alike.
- Output: `Streamed(string ArtifactId, int Segments, long Bytes, Option<TilesetCensus> Census)` carries the payload `ArtifactKey`, the cluster count (meshlet) or stream count (other kinds), and the blob length; the per-level cut aggregate is `ResidencyPayload.LevelCuts`, a PRODUCER-side derivation a consumer folds off the clusters it already holds and NOT a stream column; a re-encode of identical geometry at identical policy stamps the same content key; the blob dedups on the Persistence blob lane through `ArtifactIndexRow.Admit` and a hit writes `rasm.compute.cache.outcomes` under the blob cache slot.
- Packages: Alimer.Bindings.MeshOptimizer (`SimplificationOptions.SimplifyLockBorder` freezing open edges across the ladder), QuikGraph (`ForestDisjointSet<uint>` the shared-vertex shell partition, `KernighanLinAlgorithm` over `UndirectedGraph<int, FaceAdjacency>` the cut-minimizing cluster build), CommunityToolkit.HighPerformance (`MemoryOwner<int>` the dense incidence plane, `SpanOwner<T>` the per-cluster curvature scratch), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Element (project — the seam `ImportedGeometry` leaf carrier), Rasm (project — the kernel `EncodedGeometry` arena with `Channel`/`Descriptors`, the `EncodingChannel` lane roster, and `ChannelDtype.Unpack`), BCL inbox
- Growth: a new encoding is one `ResidencyKind` row carrying its `LeafArm` delegate column, its required-stream set, and one `StreamForm` row per stream it emits; a new meshlet-build strategy is one `ClusterBuild` row whose `Kernel` ordinal routes it to the pinned native kernel or the managed partition build and whose `CostAxis` states which cost it optimizes, the closed switch breaking until the ordinal lands, never a fork of `BuildClusters`; a new attribute is one `ResidencyStream` row with its filtered-stream line; a new measured per-cluster evidence column is one `ResidencyMeshlet` column filled at the one `Cluster` projection, so every build row and every ladder level carries it with no per-arm edit — `Curvature` and `Cut` are the two standing instances, and a per-level roll-up is one fold beside `LevelCuts`; a new filter or decode mode is one `StreamFilter`/`StreamMode` row on the `StreamSpan`; a new posture is one `ResidencyPolicy` column; a new source modality is one `ResidencySource` case; zero new surface — a `MeshletResidencyEncoder`/`SplatPayloadCodec`/`QuantizedVertexEncoder` sibling collapses onto the one `Encode` fold, and parallel `EncodedVertices`/`EncodedIndices`/`EncodedMeshlets` byte fields collapse onto the one `StreamSpan` layout.
- Boundary: every attribute read addresses the seam carrier by descriptor through one `Lane` reader, so a lane the roster grows reaches the encoder with no edit here. This lane owns the content-keyed payload blob and `StreamSpan`; `dotnet:Rasm.AppUi/Render/pipeline#TS_PROJECTION` projects every byte window, codec mode, inverse filter, codec version, cluster, bound, and content key without re-derivation. Host-side attribute reads cross through `Runs` alone — AppUi indexes the decoded runs and grows no second stream decoder — while per-cluster measured evidence (bounds, cone, shell, error chain, curvature, cut) travels on `ResidencyPayload.Clusters`, so a footprint consumer widens by the producer's `Curvature` column and re-derives no curvature off the decoded runs, and a build-strategy comparison reads the producer's own `Cut` rather than re-counting duplicated vertices from a decoded stream that no longer knows which cluster each came from. `InterchangeIdentity.Key` covers the whole assembled blob and its COMPLETE byte-changing policy vector, and the payload's own id is the folder's one `InterchangeIdentity.Address` grammar rather than a fourth hand interpolation of it. Process-global index encoding pins ONCE at the composition root through `Mount` — a static constructor is unordered against every other type's init, so its before-first-encode claim held only while nothing else touched the encoder — vertex encoding carries `ResidencyPolicy.CodecVersion` per call and is pinned nowhere, and raw meshlet triangles carry version `0`. Count-bearing native calls receive explicit semantic counts through pinned pointer kernels. Gaussian splat fitting and SPZ/SOG decoding remain companion-owned, and SOG v2 settles that ownership rather than qualifying it: the container is per-plane lossless-WebP images under a `meta.json` codebook indirection, not a packed-block offset model, so it decodes on the companion's own arm and reaches this fold only as the admitted `SplatScan` — `meta.count` seats `SplatCount` and `shN.bands` IS `HarmonicDegree` with no unit conversion, and the DC-head harmonic composition and sigmoid alpha column stand exactly as the SPZ arm left them. The .NET-side byte-mirror against that companion holds DECODE-SIDE only: a WebP re-encode is nondeterministic, so a fixture asserting encoded bytes tests the image encoder rather than this contract, and the mirror fixes the decoded `SplatScan` columns alone.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ResidencyKind {
    public static readonly ResidencyKind MeshletCluster = new("meshlet-cluster", coneCullable: true, splatBorne: false,
        required: [ResidencyStream.Positions, ResidencyStream.Indices, ResidencyStream.Triangles], Residency.MeshletEncode);
    public static readonly ResidencyKind QuantizedVertex = new("quantized-vertex", coneCullable: false, splatBorne: false,
        required: [ResidencyStream.Positions, ResidencyStream.Indices], Residency.QuantizedEncode);
    public static readonly ResidencyKind PointSplat = new("point-splat", coneCullable: false, splatBorne: false,
        required: [ResidencyStream.Positions], Residency.PointEncode);
    public static readonly ResidencyKind GaussianSplat = new("gaussian-splat", coneCullable: false, splatBorne: true,
        required: [ResidencyStream.Positions, ResidencyStream.Scales, ResidencyStream.Rotations, ResidencyStream.Harmonics, ResidencyStream.Alphas],
        Residency.SplatBorneLeafRejected);

    public bool ConeCullable { get; }
    public bool SplatBorne { get; }

    public FrozenSet<ResidencyStream> Required { get; }

    public Validation<Error, Unit> Complete(FrozenDictionary<ResidencyStream, StreamSpan> layout) =>
        toSeq(Required).Filter(stream => !layout.ContainsKey(stream)) is { IsEmpty: false } absent
            ? Validation<Error, Unit>.Fail(new ComputeFault.PayloadOverBounds(
                $"<residency-stream-absent:{Key}:{string.Join(',', absent.Map(static stream => stream.Key))}>"))
            : Validation<Error, Unit>.Success(unit);

    [UseDelegateFromConstructor]
    public partial Fin<ResidencyPayload> LeafArm(ImportedGeometry leaf, ResidencyPolicy policy);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class ClusterBuild {
    public static readonly ClusterBuild ConeWeighted = new("cone", kernel: 0, cost: CostAxis.ConeSpread);
    public static readonly ClusterBuild Flex = new("flex", kernel: 1, cost: CostAxis.ConeSpread);
    public static readonly ClusterBuild Spatial = new("spatial", kernel: 2, cost: CostAxis.Locality);
    public static readonly ClusterBuild Bisect = new("bisect", kernel: Managed, cost: CostAxis.VertexCut);

    public const int Managed = -1;

    public int Kernel { get; }

    public CostAxis Cost { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinalIgnoreCase, string>]
public sealed partial class CostAxis {
    public static readonly CostAxis ConeSpread = new("cone-spread");
    public static readonly CostAxis Locality = new("locality");
    public static readonly CostAxis VertexCut = new("vertex-cut");
}

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

    public abstract Validation<Error, Unit> Check();

    public sealed record Leaf(ResidencyKind Kind, ImportedGeometry Geometry) : ResidencySource {
        public override Validation<Error, Unit> Check() =>
            Census("leaf-extent", Geometry.VertexCount > 0 && Geometry.Lanes.Count == Geometry.VertexCount)
            & (Kind == ResidencyKind.PointSplat || Kind.SplatBorne
                ? Validation<Error, Unit>.Success(unit)
                : Census("leaf-triangles", Geometry.Indices.Length >= 3 && Geometry.Indices.Length % 3 == 0)
                  & Census("leaf-index-range", Geometry.Indices.AsSpan().ToArray().All(index => index >= 0 && index < Geometry.VertexCount && index <= uint.MaxValue)));
    }

    public sealed record Splat(SplatScan Scan) : ResidencySource {
        public override Validation<Error, Unit> Check() =>
            Census("splat-count", Scan.SplatCount is > 0 and <= int.MaxValue)
            & Census("splat-degree", Scan.HarmonicDegree is >= 0 and <= 3)
            & Census("splat-positions", Scan.Positions.Length / Residency.PositionArity >= Scan.SplatCount)
            & Census("splat-scales", Scan.Scales.Length / Residency.PositionArity >= Scan.SplatCount)
            & Census("splat-rotations", Scan.Rotations.Length / 4 >= Scan.SplatCount)
            & Census("splat-harmonics", Scan.Harmonics.Length / Math.Max(1L, HarmonicWidth(Scan.HarmonicDegree)) >= Scan.SplatCount)
            & Census("splat-alphas", Scan.Alphas.Length >= Scan.SplatCount);
    }

    internal static long HarmonicWidth(int degree) => ((long)degree + 1) * (degree + 1) * Residency.PositionArity;

    private static Validation<Error, Unit> Census(string axis, bool held) =>
        held
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new ComputeFault.PayloadOverBounds($"<residency-shape:{axis}>"));
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

    public ReadOnlyMemory<double> Vector => new double[] {
        Cluster.Kernel, MaxVertices, MinTriangles, MaxTriangles,
        ConeWeight, SplitFactor, FillWeight, QuantizationBits,
        CodecLevel, CodecVersion, SimplifyTarget, AttributeWeight,
    };
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
    float ConeCutoff,
    int Level,
    Option<int> Parent,
    int Shell,
    float Error,
    Option<float> ParentError,
    float Curvature,
    int Cut);

public readonly record struct StreamSpan(int Offset, int Length, int Count, int ByteStride, StreamMode Mode, StreamFilter Filter, int CodecVersion);

public readonly record struct StreamForm(StreamMode Mode, StreamFilter Filter, int ByteStride, Func<ResidencyPolicy, int> CodecVersion);

public readonly record struct StreamDraft(ResidencyStream Stream, int Count, ReadOnlyMemory<byte> Bytes);

[InlineArray(Slots)]
public struct Packed12 {
    public const int Slots = 3;
    private uint word;
}

public sealed record SplatScan(
    string FormatKey,
    ReadOnlyMemory<float> Positions,
    ReadOnlyMemory<float> Scales,
    ReadOnlyMemory<float> Rotations,
    ReadOnlyMemory<float> Harmonics,
    int HarmonicDegree,
    long SplatCount,
    ReadOnlyMemory<float> Alphas);

public sealed record ResidencyPayload {
    internal ResidencyPayload(
        ResidencyKind kind, UInt128 contentKey, ArtifactContent artifact, ReadOnlyMemory<byte> blob,
        FrozenDictionary<ResidencyStream, StreamSpan> layout, Seq<ResidencyMeshlet> clusters,
        int residentCount, Vector3 center, float radius, int harmonicDegree) {
        (Kind, ContentKey, Artifact, Blob, Layout) = (kind, contentKey, artifact, blob, layout);
        (Clusters, ResidentCount, Center, Radius, HarmonicDegree) = (clusters, residentCount, center, radius, harmonicDegree);
    }

    public ResidencyKind Kind { get; }
    public UInt128 ContentKey { get; }
    public ArtifactContent Artifact { get; }
    public ReadOnlyMemory<byte> Blob { get; }
    public FrozenDictionary<ResidencyStream, StreamSpan> Layout { get; }
    public Seq<ResidencyMeshlet> Clusters { get; }
    public int ResidentCount { get; }
    public Vector3 Center { get; }
    public float Radius { get; }
    public int HarmonicDegree { get; }

    public string ArtifactKey => InterchangeIdentity.Address(ContentKey, Kind.Key);

    public long EncodedBytes => Blob.Length;

    public Seq<int> LevelCuts =>
        toSeq(Clusters.GroupBy(static cluster => cluster.Level).OrderBy(static level => level.Key))
            .Map(static level => level.Sum(static cluster => cluster.Cut));
}

public sealed record ResidencyRuns(
    ReadOnlyMemory<float> Positions,
    ReadOnlyMemory<float> Normals,
    ReadOnlyMemory<float> Uvs,
    ReadOnlyMemory<uint> MeshletVertices,
    ReadOnlyMemory<byte> MeshletTriangles);

// --- [OPERATIONS] ----------------------------------------------------------------------

public static class Residency {
    internal static readonly int PositionArity = EncodingChannel.Position.Arity;
    internal static readonly int NormalArity = EncodingChannel.Normal.Arity;
    static readonly int UvArity = EncodingChannel.Uv.Arity;
    static readonly int PositionStride = PositionArity * sizeof(float);
    static readonly int NormalStride = sizeof(uint);
    static readonly int UvStride = UvArity * sizeof(float);
    const int OctBits = 8;
    const int IndexCodecVersion = 1;
    const int BisectionPasses = 4;
    const int VertexCeiling = 255;
    const int TriangleCeiling = 512;
    const float SliverFloor = 1f / (1 << 24);

    public static Fin<Unit> Mount() =>
        Interlocked.Exchange(ref mounted, 1) is 0
            ? Fin.Succ(fun(() => Meshopt.EncodeIndexVersion(IndexCodecVersion))())
            : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Initialized, new ContractEvidence.None())));

    static int mounted;

    static readonly Lazy<FrozenDictionary<(ResidencyKind Kind, ResidencyStream Stream), StreamForm>> Forms = new(static () => new[] {
        (ResidencyKind.MeshletCluster, ResidencyStream.Positions, new StreamForm(StreamMode.Attributes, StreamFilter.None, PositionStride, static p => p.CodecVersion)),
        (ResidencyKind.MeshletCluster, ResidencyStream.Indices, new StreamForm(StreamMode.Indices, StreamFilter.None, sizeof(uint), static _ => IndexCodecVersion)),
        (ResidencyKind.MeshletCluster, ResidencyStream.Triangles, new StreamForm(StreamMode.Raw, StreamFilter.None, 1, static _ => 0)),
        (ResidencyKind.MeshletCluster, ResidencyStream.Normals, new StreamForm(StreamMode.Attributes, StreamFilter.Octahedral, NormalStride, static p => p.CodecVersion)),
        (ResidencyKind.MeshletCluster, ResidencyStream.Uvs, new StreamForm(StreamMode.Attributes, StreamFilter.None, UvStride, static p => p.CodecVersion)),
        (ResidencyKind.QuantizedVertex, ResidencyStream.Positions, new StreamForm(StreamMode.Attributes, StreamFilter.Exponential, PositionStride, static p => p.CodecVersion)),
        (ResidencyKind.QuantizedVertex, ResidencyStream.Indices, new StreamForm(StreamMode.Triangles, StreamFilter.None, sizeof(uint), static _ => IndexCodecVersion)),
        (ResidencyKind.QuantizedVertex, ResidencyStream.Normals, new StreamForm(StreamMode.Attributes, StreamFilter.Octahedral, NormalStride, static p => p.CodecVersion)),
        (ResidencyKind.QuantizedVertex, ResidencyStream.Uvs, new StreamForm(StreamMode.Attributes, StreamFilter.None, UvStride, static p => p.CodecVersion)),
        (ResidencyKind.PointSplat, ResidencyStream.Positions, new StreamForm(StreamMode.Attributes, StreamFilter.Exponential, PositionStride, static p => p.CodecVersion)),
        (ResidencyKind.GaussianSplat, ResidencyStream.Positions, new StreamForm(StreamMode.Attributes, StreamFilter.Exponential, PositionStride, static p => p.CodecVersion)),
        (ResidencyKind.GaussianSplat, ResidencyStream.Scales, new StreamForm(StreamMode.Attributes, StreamFilter.Exponential, PositionStride, static p => p.CodecVersion)),
        (ResidencyKind.GaussianSplat, ResidencyStream.Rotations, new StreamForm(StreamMode.Attributes, StreamFilter.Quaternion, sizeof(ulong), static p => p.CodecVersion)),
        (ResidencyKind.GaussianSplat, ResidencyStream.Harmonics, new StreamForm(StreamMode.Attributes, StreamFilter.Exponential, sizeof(uint), static p => p.CodecVersion)),
        (ResidencyKind.GaussianSplat, ResidencyStream.Alphas, new StreamForm(StreamMode.Attributes, StreamFilter.None, sizeof(float), static p => p.CodecVersion)),
    }.ToFrozenDictionary(static row => (row.Item1, row.Item2), static row => row.Item3));

    public static Fin<ResidencyPayload> Encode(ResidencySource source, ResidencyPolicy policy) =>
        Admit(source, policy).Bind(static admitted => admitted.Source.Switch(
            state: admitted.Policy,
            leaf: static (p, l) => l.Kind.LeafArm(l.Geometry, p),
            splat: static (p, s) => SplatEncode(s.Scan, p)));

    public static Streamed Streamed(ResidencyPayload payload) =>
        new(payload.ArtifactKey, payload.Clusters.IsEmpty ? payload.Layout.Count : payload.Clusters.Count, payload.EncodedBytes, None);

    public static Fin<ResidencyRuns> Runs(ResidencyPayload payload) {
        if (payload.Kind != ResidencyKind.MeshletCluster) {
            return Fin.Fail<ResidencyRuns>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Key(payload.Kind.Key))));
        }
        ReadOnlySpan<byte> blob = payload.Blob.Span;
        Vector3[] positions = new Vector3[Count(payload, ResidencyStream.Positions)];
        uint[] packedNormals = new uint[Count(payload, ResidencyStream.Normals)];
        Vector2[] uvs = new Vector2[Count(payload, ResidencyStream.Uvs)];
        uint[] table = new uint[Count(payload, ResidencyStream.Indices)];
        Validation<Error, Unit> decoded =
            payload.Kind.Complete(payload.Layout)
            & Decoded(payload, ResidencyStream.Positions, positions, blob)
            & Decoded(payload, ResidencyStream.Normals, packedNormals, blob)
            & Decoded(payload, ResidencyStream.Uvs, uvs, blob)
            & Decoded(payload, ResidencyStream.Indices, table, blob);
        if (packedNormals.Length > 0) { Meshopt.DecodeFilterOct<uint>(packedNormals); }
        ReadOnlyMemory<byte> triangles = payload.Layout.TryGetValue(ResidencyStream.Triangles, out StreamSpan raw)
            ? blob.Slice(raw.Offset, raw.Length).ToArray()
            : ReadOnlyMemory<byte>.Empty;
        return decoded.Map(_ => new ResidencyRuns(
            MemoryMarshal.Cast<Vector3, float>(positions).ToArray(),
            UnpackSnorm(packedNormals),
            MemoryMarshal.Cast<Vector2, float>(uvs).ToArray(),
            table,
            triangles)).ToFin();
    }

    static int Count(ResidencyPayload payload, ResidencyStream stream) =>
        payload.Layout.TryGetValue(stream, out StreamSpan span) ? span.Count : 0;

    static Validation<Error, Unit> Decoded<T>(ResidencyPayload payload, ResidencyStream stream, Span<T> destination, ReadOnlySpan<byte> blob) where T : unmanaged =>
        destination.Length > 0 && payload.Layout.TryGetValue(stream, out StreamSpan span)
            ? (span.Mode == StreamMode.Indices
                ? Meshopt.DecodeIndexSequence(destination, blob.Slice(span.Offset, span.Length))
                : Meshopt.DecodeVertexBuffer(destination, blob.Slice(span.Offset, span.Length))) is var status && status != 0
                ? Validation<Error, Unit>.Fail(new ComputeFault.PayloadOverBounds(
                    $"<residency-runs-decode:{payload.ArtifactKey}:{stream.Key}:{status}>"))
                : Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Success(unit);

    static float[] UnpackSnorm(ReadOnlySpan<uint> packed) {
        float[] wide = new float[packed.Length * NormalArity];
        ReadOnlySpan<sbyte> lanes = MemoryMarshal.Cast<uint, sbyte>(packed);
        for (int v = 0; v < packed.Length; v++) {
            (wide[v * NormalArity], wide[(v * NormalArity) + 1], wide[(v * NormalArity) + 2]) =
                (lanes[v * 4] / 127f, lanes[(v * 4) + 1] / 127f, lanes[(v * 4) + 2] / 127f);
        }
        return wide;
    }

    public sealed record AdmittedResidency {
        internal AdmittedResidency(ResidencySource source, ResidencyPolicy policy) => (Source, Policy) = (source, policy);
        public ResidencySource Source { get; }
        public ResidencyPolicy Policy { get; }
    }

    static Fin<AdmittedResidency> Admit(ResidencySource source, ResidencyPolicy policy) =>
        (Bounded("max-vertices", policy.MaxVertices, policy.MaxVertices is >= 3 and <= VertexCeiling)
         & Bounded("max-triangles", policy.MaxTriangles, policy.MaxTriangles is >= 4 and <= TriangleCeiling && policy.MaxTriangles % 4 == 0)
         & Bounded("min-triangles", policy.MinTriangles, policy.MinTriangles >= 1 && policy.MinTriangles <= policy.MaxTriangles)
         & Bounded("cone-weight", policy.ConeWeight, policy.ConeWeight is >= 0f and <= 1f)
         & Bounded("split-factor", policy.SplitFactor, policy.SplitFactor >= 1f)
         & Bounded("fill-weight", policy.FillWeight, policy.FillWeight is >= 0f and <= 1f)
         & Bounded("quantization-bits", policy.QuantizationBits, policy.QuantizationBits is >= 1 and <= 24)
         & Bounded("simplify-target", policy.SimplifyTarget, policy.SimplifyTarget is > 0d and <= 1d)
         & Bounded("attribute-weight", policy.AttributeWeight, double.IsFinite(policy.AttributeWeight) && policy.AttributeWeight >= 0d)
         & source.Check())
        .Map(_ => new AdmittedResidency(source, policy)).ToFin();

    static Validation<Error, Unit> Bounded<T>(string axis, T value, bool admitted) where T : IFormattable =>
        admitted
            ? Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Fail(new ComputeFault.PayloadOverBounds(
                $"<residency-policy:{axis}:{value.ToString("R", CultureInfo.InvariantCulture)}>"));

    internal static Fin<ResidencyPayload> SplatBorneLeafRejected(ImportedGeometry leaf, ResidencyPolicy policy) =>
        Fin.Fail<ResidencyPayload>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Required(ComputeSubject.Resource)));

    internal static Fin<ResidencyPayload> MeshletEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        float[] positions = Lane(leaf.Lanes, EncodingChannel.Position);
        uint[] optimized = new uint[leaf.Indices.Length];
        Meshopt.OptimizeVertexCache(optimized, ToUInt(leaf.Indices.AsSpan()), (nuint)leaf.VertexCount);
        nuint maxMeshlets = Meshopt.BuildMeshletsBound((nuint)optimized.Length, (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles);
        Meshlet[] meshlets = new Meshlet[(int)maxMeshlets];
        uint[] meshletVertices = new uint[(int)maxMeshlets * policy.MaxVertices];
        byte[] meshletTriangles = new byte[(int)maxMeshlets * policy.MaxTriangles * 3];
        ShellCensus shells = Shells(optimized);
        int count = BuildClusters(optimized, positions, leaf.VertexCount, policy, meshlets, meshletVertices, meshletTriangles);
        if (count == 0) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-meshlet-empty:{leaf.FormatKey}>")); }
        using MemoryOwner<int> incidence = Incidence(meshlets, count, meshletVertices, leaf.VertexCount);
        List<ResidencyMeshlet> clusters = new(count);
        for (int m = 0; m < count; m++) {
            ref readonly Meshlet meshlet = ref meshlets[m];
            Span<uint> localVertices = meshletVertices.AsSpan((int)meshlet.vertex_offset, (int)meshlet.vertex_count);
            Span<byte> localTriangles = meshletTriangles.AsSpan((int)meshlet.triangle_offset, (int)meshlet.triangle_count * 3);
            Meshopt.OptimizeMeshlet(localVertices, localTriangles, meshlet.triangle_count, meshlet.vertex_count);
            clusters.Add(Cluster(meshlet, localVertices, localTriangles, positions, leaf.VertexCount, level: 0, error: 0f,
                shell: shells.Of(localVertices[0]).IfNone(0), incidence.Span));
        }
        ref readonly Meshlet tail = ref meshlets[count - 1];
        int usedVertices = (int)(tail.vertex_offset + tail.vertex_count);
        int usedTriangleBytes = (int)(tail.triangle_offset + tail.triangle_count * 3);
        (Seq<ResidencyMeshlet> Clusters, uint[] Vertices, byte[] Triangles) chained = LodChain(
            optimized, positions, leaf.VertexCount, policy, clusters, shells,
            meshletVertices.AsSpan(0, usedVertices), meshletTriangles.AsSpan(0, usedTriangleBytes));
        Seq<StreamDraft> streams = Seq(
            new StreamDraft(ResidencyStream.Positions, leaf.VertexCount, EncodeStream(MemoryMarshal.Cast<float, Vector3>(positions.AsSpan(0, leaf.VertexCount * PositionArity)), policy)),
            new StreamDraft(ResidencyStream.Indices, chained.Vertices.Length, EncodeSequence(chained.Vertices, leaf.VertexCount)),
            new StreamDraft(ResidencyStream.Triangles, chained.Triangles.Length, chained.Triangles))
            + Optional(leaf, EncodingChannel.Normal, ResidencyStream.Normals, lane => EncodeNormals(lane, leaf.VertexCount, policy))
            + Optional(leaf, EncodingChannel.Uv, ResidencyStream.Uvs, lane => EncodeUvs(lane, leaf.VertexCount, policy));
        return Assemble(ResidencyKind.MeshletCluster, leaf.FormatKey, streams, chained.Clusters, leaf.VertexCount, SphereBounds(positions, leaf.VertexCount), 0, policy);
    }

    static (Seq<ResidencyMeshlet> Clusters, uint[] Vertices, byte[] Triangles) LodChain(
        uint[] indices,
        ReadOnlySpan<float> positions,
        int vertexCount,
        ResidencyPolicy policy,
        List<ResidencyMeshlet> level0,
        ShellCensus shells,
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
            nuint written = Meshopt.Simplify(simplified, current, positions, (nuint)vertexCount, PositionStride, target,
                targetError: float.MaxValue, options: SimplificationOptions.SimplifyLockBorder, out float resultError);
            if (written >= (nuint)current.Length || written < 3) { break; }
            Array.Resize(ref simplified, (int)written);
            level++;
            float objectError = resultError * scale;
            (int coarse, int coarseFirst) = ClusterLevel(simplified, positions, vertexCount, policy, all, vertices, triangles, level, objectError, shells);
            if (coarse == 0) { break; }
            Link(all, firstOfLevel, countOfLevel, coarseFirst, coarse);
            firstOfLevel = coarseFirst; countOfLevel = coarse; current = simplified;
        }
        return (toSeq(all), vertices.ToArray(), triangles.ToArray());
    }

    static void Link(List<ResidencyMeshlet> all, int fineFirst, int fineCount, int coarseFirst, int coarseCount) {
        for (int f = fineFirst; f < fineFirst + fineCount; f++) {
            Option<int> best = Bound(all, f, coarseFirst, coarseCount);
            best.Iter(parent => {
                all[parent] = all[parent] with { Error = Math.Max(all[parent].Error, all[f].Error) };
                all[f] = all[f] with { Parent = Some(parent) };
            });
        }
        for (int f = fineFirst; f < fineFirst + fineCount; f++) {
            all[f] = all[f] with { ParentError = all[f].Parent.Map(parent => Math.Max(all[parent].Error, all[f].Error)) };
        }
    }

    static Option<int> Bound(List<ResidencyMeshlet> all, int fine, int coarseFirst, int coarseCount) =>
        toSeq(Enumerable.Range(coarseFirst, coarseCount))
            .Filter(c => all[c].Shell == all[fine].Shell)
            .Map(c => (Index: c, Distance: Vector3.Distance(all[fine].Center, all[c].Center)))
            .Fold((Covering: Option<(int Index, float Distance)>.None, Nearest: Option<(int Index, float Distance)>.None),
                (acc, row) => (
                    Covering: row.Distance + all[fine].Radius <= all[row.Index].Radius && acc.Covering.Map(held => row.Distance < held.Distance).IfNone(true) ? Some(row) : acc.Covering,
                    Nearest: acc.Nearest.Map(held => row.Distance < held.Distance).IfNone(true) ? Some(row) : acc.Nearest))
            is var found && found.Covering.IsSome
                ? found.Covering.Map(static row => row.Index)
                : found.Nearest.Map(static row => row.Index);

    internal static Fin<ResidencyPayload> QuantizedEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        if (policy.QuantizationBits is < 1 or > 24) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-quant-bits:{policy.QuantizationBits}>")); }
        float[] positions = Lane(leaf.Lanes, EncodingChannel.Position);
        Seq<StreamDraft> streams = Seq(
            new StreamDraft(ResidencyStream.Positions, leaf.VertexCount, EncodeExp(positions, leaf.VertexCount, policy)),
            new StreamDraft(ResidencyStream.Indices, leaf.Indices.Length, EncodeTriangles(ToUInt(leaf.Indices.AsSpan()), leaf.VertexCount)))
            + Optional(leaf, EncodingChannel.Normal, ResidencyStream.Normals, lane => EncodeNormals(lane, leaf.VertexCount, policy))
            + Optional(leaf, EncodingChannel.Uv, ResidencyStream.Uvs, lane => EncodeUvs(lane, leaf.VertexCount, policy));
        return Assemble(ResidencyKind.QuantizedVertex, leaf.FormatKey, streams, Seq<ResidencyMeshlet>(), leaf.VertexCount, SphereBounds(positions, leaf.VertexCount), 0, policy);
    }

    internal static Fin<ResidencyPayload> PointEncode(ImportedGeometry leaf, ResidencyPolicy policy) {
        if (policy.SimplifyTarget is <= 0 or > 1) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-simplify-target:{policy.SimplifyTarget:R}>")); }
        float[] positions = Lane(leaf.Lanes, EncodingChannel.Position);
        int target = Math.Max(1, (int)(leaf.VertexCount * policy.SimplifyTarget));
        uint[] remap = new uint[target];
        int kept = DecimatePoints(remap, positions, leaf.VertexCount,
            Lane(leaf.Lanes, EncodingChannel.Normal), policy.AttributeWeight, target);
        float[] gathered = new float[kept * PositionArity];
        for (int v = 0; v < kept; v++) { positions.AsSpan((int)remap[v] * PositionArity, PositionArity).CopyTo(gathered.AsSpan(v * PositionArity)); }
        return Assemble(ResidencyKind.PointSplat, leaf.FormatKey,
            Seq(new StreamDraft(ResidencyStream.Positions, kept, EncodeExp(gathered, kept, policy))),
            Seq<ResidencyMeshlet>(), kept, SphereBounds(gathered, kept), 0, policy);
    }

    static Fin<ResidencyPayload> SplatEncode(SplatScan scan, ResidencyPolicy policy) {
        if (policy.QuantizationBits is < 1 or > 16) { return Fin.Fail<ResidencyPayload>(new ComputeFault.PayloadOverBounds($"<residency-splat-bits:{policy.QuantizationBits}>")); }
        int n = (int)scan.SplatCount;
        int shFloats = (scan.HarmonicDegree + 1) * (scan.HarmonicDegree + 1) * 3;
        ulong[] rotations = new ulong[n];
        Meshopt.EncodeFilterQuat<ulong>(rotations, policy.QuantizationBits, scan.Rotations.Span[..(n * 4)]);
        uint[] harmonics = new uint[n * shFloats];
        Meshopt.EncodeFilterExp<uint>(harmonics, policy.QuantizationBits, scan.Harmonics.Span[..(n * shFloats)], EncodeExpMode.EncodeExpSeparate);
        Seq<StreamDraft> streams = Seq(
            new StreamDraft(ResidencyStream.Positions, n, EncodeExp(scan.Positions.Span, n, policy)),
            new StreamDraft(ResidencyStream.Scales, n, EncodeExp(scan.Scales.Span, n, policy)),
            new StreamDraft(ResidencyStream.Rotations, n, EncodeStream<ulong>(rotations, policy)),
            new StreamDraft(ResidencyStream.Harmonics, n * shFloats, EncodeStream<uint>(harmonics, policy)),
            new StreamDraft(ResidencyStream.Alphas, n, EncodeStream<float>(scan.Alphas.Span[..n], policy)));
        return Assemble(ResidencyKind.GaussianSplat, scan.FormatKey, streams, Seq<ResidencyMeshlet>(), n, SphereBounds(scan.Positions.Span, n), scan.HarmonicDegree, policy);
    }

    static Fin<ResidencyPayload> Assemble(ResidencyKind kind, string formatKey, Seq<StreamDraft> streams,
        Seq<ResidencyMeshlet> clusters, int residentCount, (Vector3 Center, float Radius) bounds, int harmonicDegree, ResidencyPolicy policy) {
        Seq<StreamDraft> unformed = streams.Filter(draft => !Forms.Value.ContainsKey((kind, draft.Stream)));
        if (!unformed.IsEmpty) {
            return Fin.Fail<ResidencyPayload>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Complete, new ContractEvidence.Count(unformed.Count, 0L))));
        }

        byte[] blob = new byte[streams.Sum(static draft => draft.Bytes.Length)];
        Dictionary<ResidencyStream, StreamSpan> layout = new(streams.Count);
        int cursor = 0;
        foreach (StreamDraft draft in streams) {
            StreamForm form = Forms.Value[(kind, draft.Stream)];
            draft.Bytes.Span.CopyTo(blob.AsSpan(cursor));
            layout[draft.Stream] = new StreamSpan(cursor, draft.Bytes.Length, draft.Count, form.ByteStride, form.Mode, form.Filter, form.CodecVersion(policy));
            cursor += draft.Bytes.Length;
        }

        UInt128 key = InterchangeIdentity.Key($"{formatKey}:{kind.Key}", blob, policy.Vector);
        return ArtifactContent.Of(blob, Op.Of(name: "compute.residency.artifact")).Map(artifact =>
            new ResidencyPayload(kind, key, artifact, blob, layout.ToFrozenDictionary(), clusters,
                residentCount, bounds.Center, bounds.Radius, harmonicDegree));
    }

    static Seq<StreamDraft> Optional(ImportedGeometry leaf, EncodingChannel channel, ResidencyStream stream, Func<float[], ReadOnlyMemory<byte>> encode) =>
        leaf.Lanes.Descriptors.Exists(descriptor => descriptor.Channel == channel)
            ? Seq(new StreamDraft(stream, leaf.VertexCount, encode(Lane(leaf.Lanes, channel))))
            : Seq<StreamDraft>();

    static ReadOnlyMemory<byte> EncodeUvs(ReadOnlySpan<float> uvs, int count, ResidencyPolicy policy) =>
        EncodeStream(MemoryMarshal.Cast<float, Vector2>(uvs[..(count * UvArity)]), policy);

    static ReadOnlyMemory<byte> EncodeExp(ReadOnlySpan<float> floats, int count, ResidencyPolicy policy) {
        Packed12[] packed = new Packed12[count];
        Meshopt.EncodeFilterExp<Packed12>(packed, policy.QuantizationBits, floats[..(count * Packed12.Slots)], EncodeExpMode.EncodeExpSharedComponent);
        return EncodeStream<Packed12>(packed, policy);
    }

    static ReadOnlyMemory<byte> EncodeNormals(ReadOnlySpan<float> normals, int count, ResidencyPolicy policy) {
        float[] quad = new float[count * 4];
        for (int v = 0; v < count; v++) { normals.Slice(v * NormalArity, NormalArity).CopyTo(quad.AsSpan(v * 4)); }
        uint[] packed = new uint[count];
        Meshopt.EncodeFilterOct<uint>(packed, OctBits, quad);
        return EncodeStream<uint>(packed, policy);
    }

    static ReadOnlyMemory<byte> EncodeStream<T>(ReadOnlySpan<T> packed, ResidencyPolicy policy) where T : unmanaged {
        byte[] buffer = new byte[(int)Meshopt.EncodeVertexBufferBound((nuint)packed.Length, (nuint)Unsafe.SizeOf<T>())];
        return buffer.AsMemory(0, (int)Meshopt.EncodeVertexBufferLevel<T>(buffer, packed, policy.CodecLevel, policy.CodecVersion));
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

    static float[] Lane(EncodedGeometry arena, EncodingChannel channel) {
        if (arena.Descriptors.Find(descriptor => descriptor.Channel == channel).Case is not EncodingChannelDescriptor found) { return []; }
        float[] raw = new float[found.Floats];
        found.Dtype.Unpack(arena.Channel(channel).Span, raw);
        return raw;
    }

    static ResidencyMeshlet Cluster(in Meshlet meshlet, ReadOnlySpan<uint> localVertices, ReadOnlySpan<byte> localTriangles,
        ReadOnlySpan<float> positions, int vertexCount, int level, float error, int shell, ReadOnlySpan<int> incidence) {
        Bounds bounds = ClusterBounds(localVertices, localTriangles, (int)meshlet.triangle_count, positions, vertexCount);
        return new ResidencyMeshlet(
            (int)meshlet.vertex_offset, (int)meshlet.triangle_offset, (int)meshlet.vertex_count, (int)meshlet.triangle_count,
            Center: new Vector3(bounds.center[0], bounds.center[1], bounds.center[2]),
            Radius: bounds.radius,
            ConeApex: new Vector3(bounds.cone_apex[0], bounds.cone_apex[1], bounds.cone_apex[2]),
            ConeAxis: new Vector3(bounds.cone_axis[0], bounds.cone_axis[1], bounds.cone_axis[2]),
            ConeCutoff: bounds.cone_cutoff,
            Level: level, Parent: None, Shell: shell, Error: error, ParentError: None,
            Curvature: Curvature(localVertices, localTriangles, (int)meshlet.triangle_count, positions),
            Cut: Cut(localVertices, incidence));
    }

    static MemoryOwner<int> Incidence(Meshlet[] meshlets, int count, uint[] meshletVertices, int vertexCount) {
        MemoryOwner<int> seen = MemoryOwner<int>.Allocate(vertexCount, AllocationMode.Clear);
        Span<int> plane = seen.Span;
        for (int m = 0; m < count; m++) {
            ref readonly Meshlet meshlet = ref meshlets[m];
            foreach (uint global in meshletVertices.AsSpan((int)meshlet.vertex_offset, (int)meshlet.vertex_count)) {
                plane[(int)global]++;
            }
        }

        return seen;
    }

    static int Cut(ReadOnlySpan<uint> localVertices, ReadOnlySpan<int> incidence) {
        int shared = 0;
        foreach (uint global in localVertices) {
            if (incidence[(int)global] > 1) { shared++; }
        }

        return shared;
    }

    static float Curvature(ReadOnlySpan<uint> localVertices, ReadOnlySpan<byte> localTriangles, int triangleCount, ReadOnlySpan<float> positions) {
        ReadOnlySpan<Vector3> points = MemoryMarshal.Cast<float, Vector3>(positions);
        using SpanOwner<Vector3> normalScratch = SpanOwner<Vector3>.Allocate(TriangleCeiling);
        using SpanOwner<Vector3> centroidScratch = SpanOwner<Vector3>.Allocate(TriangleCeiling);
        using SpanOwner<int> incidentScratch = SpanOwner<int>.Allocate(VertexCeiling);
        using SpanOwner<int> chainScratch = SpanOwner<int>.Allocate(TriangleCeiling * 3);
        (Span<Vector3> normals, Span<Vector3> centroids) = (normalScratch.Span, centroidScratch.Span);
        (Span<int> incident, Span<int> chain) = (incidentScratch.Span, chainScratch.Span);
        incident.Fill(-1);
        float bound = 0f;
        for (int t = 0; t < triangleCount; t++) {
            (Vector3 a, Vector3 b, Vector3 c) = (
                points[(int)localVertices[localTriangles[t * 3]]],
                points[(int)localVertices[localTriangles[(t * 3) + 1]]],
                points[(int)localVertices[localTriangles[(t * 3) + 2]]]);
            Vector3 cross = Vector3.Cross(b - a, c - a);
            float extent = (b - a).LengthSquared() + (c - a).LengthSquared();
            normals[t] = cross.LengthSquared() > SliverFloor * SliverFloor * extent * extent ? Vector3.Normalize(cross) : Vector3.Zero;
            centroids[t] = (a + b + c) / 3f;
            for (int corner = 0; corner < 3; corner++) {
                int slot = localTriangles[(t * 3) + corner];
                for (int entry = incident[slot]; entry >= 0; entry = chain[entry]) {
                    bound = Math.Max(bound, Rate(normals[t], centroids[t], normals[entry / 3], centroids[entry / 3]));
                }

                chain[(t * 3) + corner] = incident[slot];
                incident[slot] = (t * 3) + corner;
            }
        }

        return bound;
    }

    static float Rate(Vector3 first, Vector3 firstCentroid, Vector3 second, Vector3 secondCentroid) =>
        Vector3.Distance(firstCentroid, secondCentroid) is var distance && distance > 0f && first != Vector3.Zero && second != Vector3.Zero
            ? float.Atan2(Vector3.Cross(first, second).Length(), Vector3.Dot(first, second)) / distance
            : 0f;

    public readonly record struct ShellCensus(HashMap<uint, int> Ordinals, int Count) {
        public Option<int> Of(uint vertex) => Ordinals.Find(vertex);
    }

    static ShellCensus Shells(ReadOnlySpan<uint> indices) {
        ForestDisjointSet<uint> forest = new(indices.Length);
        foreach (uint corner in indices) { if (!forest.Contains(corner)) { forest.MakeSet(corner); } }
        for (int t = 0; t + 2 < indices.Length; t += 3) {
            forest.Union(indices[t], indices[t + 1]);
            forest.Union(indices[t], indices[t + 2]);
        }

        HashMap<uint, int> representatives = HashMap<uint, int>();
        HashMap<uint, int> ordinals = HashMap<uint, int>();
        foreach (uint corner in indices) {
            uint representative = forest.FindSet(corner);
            representatives = representatives.AddOrUpdate(representative, static held => held, () => representatives.Count);
            ordinals = ordinals.AddOrUpdate(corner, representatives[representative]);
        }

        return new ShellCensus(ordinals, forest.SetCount);
    }

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
        ShellCensus shells) {
        nuint bound = Meshopt.BuildMeshletsBound((nuint)simplified.Length, (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles);
        Meshlet[] meshlets = new Meshlet[(int)bound];
        uint[] vertices = new uint[(int)bound * policy.MaxVertices];
        byte[] triangles = new byte[(int)bound * policy.MaxTriangles * 3];
        int first = all.Count;
        int vertexBase = payloadVertices.Count;
        int triangleBase = payloadTriangles.Count;
        int count = BuildClusters(simplified, positions, vertexCount, policy, meshlets, vertices, triangles);
        using MemoryOwner<int> incidence = Incidence(meshlets, count, vertices, vertexCount);
        for (int m = 0; m < count; m++) {
            ref readonly Meshlet meshlet = ref meshlets[m];
            Span<uint> localVertices = vertices.AsSpan((int)meshlet.vertex_offset, (int)meshlet.vertex_count);
            Span<byte> localTriangles = triangles.AsSpan((int)meshlet.triangle_offset, (int)meshlet.triangle_count * 3);
            ResidencyMeshlet cluster = Cluster(meshlet, localVertices, localTriangles, positions, vertexCount, level, objectError,
                shells.Of(localVertices[0]).IfNone(0), incidence.Span);
            all.Add(cluster with { VertexOffset = vertexBase + cluster.VertexOffset, TriangleOffset = triangleBase + cluster.TriangleOffset });
        }
        if (count > 0) {
            ref readonly Meshlet tail = ref meshlets[count - 1];
            payloadVertices.AddRange(vertices.AsSpan(0, (int)(tail.vertex_offset + tail.vertex_count)).ToArray());
            payloadTriangles.AddRange(triangles.AsSpan(0, (int)(tail.triangle_offset + tail.triangle_count * 3)).ToArray());
        }
        return (count, first);
    }

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

    static Seq<Seq<int>> Bisected(Seq<int> faces, uint[] corners, ResidencyPolicy policy) {
        if (faces.Count <= policy.MaxTriangles) { return Seq(faces); }
        UndirectedGraph<int, FaceAdjacency> adjacency = new(allowParallelEdges: false);
        adjacency.AddVertexRange(faces);
        adjacency.AddEdgeRange(Adjacencies(faces, corners));
        KernighanLinAlgorithm<int, FaceAdjacency> partition = new(adjacency, nbIterations: BisectionPasses);
        partition.Compute();
        Seq<int> left = toSeq(partition.Partition.VertexSetA);
        Seq<int> right = toSeq(partition.Partition.VertexSetB);
        return left.IsEmpty || right.IsEmpty
            ? Bisected(toSeq(faces.Take(faces.Count / 2)), corners, policy) + Bisected(toSeq(faces.Skip(faces.Count / 2)), corners, policy)
            : Bisected(left, corners, policy) + Bisected(right, corners, policy);
    }

    static Seq<FaceAdjacency> Adjacencies(Seq<int> faces, uint[] corners) =>
        toSeq(faces
            .Bind(face => Seq(corners[face * 3], corners[(face * 3) + 1], corners[(face * 3) + 2]).Map(corner => (Corner: corner, Face: face)))
            .GroupBy(static entry => entry.Corner)
            .Bind(static shared => Pairs(toSeq(shared).Map(static entry => entry.Face)))
            .GroupBy(static pair => pair))
            .Map(static group => new FaceAdjacency(group.Key.Low, group.Key.High, group.Count()));

    static Seq<(int Low, int High)> Pairs(Seq<int> faces) =>
        faces.Head.Match(
            Some: head => faces.Tail.Map(other => (Low: Math.Min(head, other), High: Math.Max(head, other))) + Pairs(faces.Tail),
            None: () => Seq<(int, int)>());

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
        return (cursor.Meshlets + 1, cursor.Vertices + local.Count, cursor.Triangles + (triangle * 3));
    }

    static unsafe int BuildClusters(ReadOnlySpan<uint> indices, ReadOnlySpan<float> positions, int vertexCount, ResidencyPolicy policy,
        Meshlet[] meshlets, uint[] meshletVertices, byte[] meshletTriangles) {
        if (policy.Cluster.Kernel is ClusterBuild.Managed) { return BisectClusters(indices, policy, meshlets, meshletVertices, meshletTriangles); }
        fixed (Meshlet* meshlet = meshlets)
        fixed (uint* vertices = meshletVertices)
        fixed (byte* triangles = meshletTriangles)
        fixed (uint* index = indices)
        fixed (float* position = positions) {
            return (int)(policy.Cluster.Kernel switch {
                1 => Meshopt.BuildMeshletsFlex(meshlet, vertices, triangles, index, (nuint)indices.Length, position, (nuint)vertexCount,
                    (nuint)PositionStride, (nuint)policy.MaxVertices, (nuint)policy.MinTriangles, (nuint)policy.MaxTriangles, policy.ConeWeight, policy.SplitFactor),
                2 => Meshopt.BuildMeshletsSpatial(meshlet, vertices, triangles, index, (nuint)indices.Length, position, (nuint)vertexCount,
                    (nuint)PositionStride, (nuint)policy.MaxVertices, (nuint)policy.MinTriangles, (nuint)policy.MaxTriangles, policy.FillWeight),
                0 => Meshopt.BuildMeshlets(meshlet, vertices, triangles, index, (nuint)indices.Length, position, (nuint)vertexCount,
                    (nuint)PositionStride, (nuint)policy.MaxVertices, (nuint)policy.MaxTriangles, policy.ConeWeight),
            });
        }
    }

    static unsafe Bounds ClusterBounds(ReadOnlySpan<uint> meshletVertices, ReadOnlySpan<byte> meshletTriangles, int triangleCount, ReadOnlySpan<float> positions, int vertexCount) {
        fixed (uint* vertices = meshletVertices)
        fixed (byte* triangles = meshletTriangles)
        fixed (float* position = positions) {
            return Meshopt.ComputeMeshletBounds(vertices, triangles, (nuint)triangleCount, position, (nuint)vertexCount, (nuint)PositionStride);
        }
    }

    static unsafe (Vector3 Center, float Radius) SphereBounds(ReadOnlySpan<float> positions, int count) {
        fixed (float* position = positions) {
            Bounds bounds = Meshopt.ComputeSphereBounds(position, (nuint)count, (nuint)PositionStride, null, 0);
            return (new Vector3(bounds.center[0], bounds.center[1], bounds.center[2]), bounds.radius);
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

# [COMPUTE_PAYLOAD]

Rasm.Compute streaming-residency lane: the content-keyed GPU-ready payload codec a web viewer streams cell-by-cell. Four encode arms ride one `ResidencyKind` axis — meshlet-cluster partitions an octree-leaf `ImportedGeometry` into cone-cullable clusters, quantized-vertex exponent-filters and level-compresses a leaf for a low-VRAM tile, point-splat decimates a reality-capture point set, and gaussian-splat octahedral/quaternion/exponent-filters a companion-decoded `SplatScan`. One `Encode` fold over the safe `Meshopt` span surface owns every arm, so a per-kind encoder sibling is the collapsed form. This lane produces payload bytes and the self-describing `StreamSpan` bufferView layout only, never a manifest or a scene-graph.

Payload bytes address through the suite `Runtime/codecs#CONTENT_ADDRESSING` `XxHash128` key, read the `Runtime/tiles#TILE_PARTITION` `ImportedGeometry` octree leaf (never a second partition), and ride the `Runtime/receipts#RECEIPT_UNION` `StreamSegment` slot (never a new receipt case). `csharp:Rasm.AppUi/Render/pipeline#TS_PROJECTION` `ResidencyMap.Mint` projects each payload's `StreamSpan` layout, `ResidencyMeshlet` clusters, and content key directly into generated `Render.V1.GeometryResidency`; a Compute-side manifest or generated-message mirror is the named drift defect. Encoded blobs land content-addressed on the Persistence blob lane through `ArtifactIndexRow.Admit` at the app-platform seam. Splat scans arrive from the Python `realitycapture` companion as `ArtifactFrame` bytes at the `Runtime/wire#PROTO_VOCABULARY` `ArtifactService.Fetch` seam, never an in-process splat fit or SPZ/SOG decoder. HOST-LOCAL, no TS_PROJECTION.

## [01]-[INDEX]

- [02]-[RESIDENCY]: `Residency.Encode` folds a `ResidencySource` onto its `ResidencyKind` row over the safe `Meshopt` span surface.

## [02]-[RESIDENCY]

- Owner: `ResidencyKind` `[SmartEnum<string>]` the one closed payload axis, each row's `ConeCullable`/`SplatBorne` columns telling the AppUi marshal which cull and shader to pick, so a new encoding is one row, never a per-kind payload type; `ResidencyStream`, `StreamMode`, `StreamFilter` the closed buffer-role, meshopt decode-mode, and attribute-filter axes whose keys ARE the `EXT_meshopt_compression` wire modes the manifest emits; `ResidencySource` `[Union]` the polymorphic encode input (`Leaf` for octree-leaf arms, `Splat` for a companion scan), so one entry discriminates on shape, never an `Encode`/`EncodeSplat` pair; `ResidencyMeshlet` the per-cluster cone-and-sphere descriptor carrying the cluster-LOD chain columns `Level`, the `Option`-shaped `Parent`/`ParentError` a root simply lacks, `Error`, the `Shell` connected-component column the parent link searches within, the `Curvature` normal-variation bound measured off the cluster's own triangles, and the `Cut` realized shared-boundary-vertex count every build row fills; `FaceAdjacency` the shared-vertex-count-tagged triangle-adjacency edge the cut-minimizing build partitions over; `ResidencyPolicy` the encode-posture record carrying the complete ordered `Vector` every content key folds; `StreamForm`/`StreamDraft` the `(kind, stream) -> form` policy table and the measured-only draft each arm supplies; `AdmittedResidency` the evidence carrier `Admit` mints and every arm takes; `ShellCensus` the shell ordinal table and its count; `CostAxis` the build-objective column; `ResidencyPayload` the content-keyed buffer carrier (blob, per-stream `StreamSpan` layout, clusters, bounding sphere, content key) whose constructor is private so `Assemble` is its one mint, not a manifest; `ResidencyRuns` the decoded per-vertex attribute-run carrier a host consumer indexes per primitive; `Residency` the static `Encode` fold with the `StreamSegment` `Receipt` projection and the paired `Runs` decode.
- Cases: `ResidencyKind` rows `meshlet-cluster` (cone-cullable cluster-LOD chain — global vertex stream, `EncodeIndexSequence` meshlet-vertex table, raw local triangle bytes, per-cluster descriptors across the `Meshopt.Simplify` levels `SimplifyTarget` drives) · `quantized-vertex` (exponent-filtered, level-compressed single tile) · `point-splat` (`SimplifyPoints`-decimated, exponent-filtered positions) · `gaussian-splat` (companion-decoded `SplatScan` — positions/scales/harmonics exponent-filter, rotation quaternions quaternion-filter, sigmoid-activated alphas raw).
- Entry: `public static Fin<ResidencyPayload> Encode(ResidencySource source, ResidencyPolicy policy)` projects a leaf (or companion scan) onto the kind's arm; `public static Fin<ResidencyRuns> Runs(ResidencyPayload payload)` is the ONE host-side attribute decode — meshlet-cluster only, each stream under its own `Layout` row, the `csharp:Rasm.AppUi/Render/meshlets#CLUSTER_CONSUMPTION` and `Render/pathtrace#BSDF_SHADING` `SurfaceAttribution` data source; `public static ComputeReceipt.StreamSegment Receipt(ResidencyPayload payload, CorrelationId correlation, WorkLane lane, Substrate substrate, Duration elapsed)` projects onto the settled slot under the substrate that actually ran; `Fin<T>` aborts onto `PayloadOverBounds` for an empty meshlet build, an out-of-range quantization budget, an out-of-range simplify target, an absent mandatory stream, or a stream a decode rejects, while source-kind contract refusals use the typed `ComputeViolation` arms. `public static Fin<Unit> Mount()` pins the process-global index-codec version once at the composition root.
- Auto: `Encode` accumulates every policy bound and the source's own shape census — each `ResidencySource` case carrying its own `Check`, so a third modality cannot land without one — into one `AdmittedResidency` before dispatching the union; the `Leaf` arm reads the kind's row-owned `LeafArm` `[UseDelegateFromConstructor]` column, so the joint source-kind decision has one dispatch level. Meshlet encoding clusters through the `ClusterBuild` row's own kernel ORDINAL (`0` = `BuildMeshlets`, `1` = `BuildMeshletsFlex`, `2` = `BuildMeshletsSpatial`, `Managed` = the Kernighan-Lin recursion) under a closed switch, so a row without an ordinal fails to construct rather than falling into the cone-weighted scan, reads the shell partition once off the union-find forest so every level's parent link stays inside one connected component and the ladder terminates at one meshlet PER SHELL, measures each cluster's curvature bound and its realized `Cut` at the one `Cluster` projection off the local triangles the bounds kernel already reads and the level's own cluster-incidence census — every build row, every ladder level, which is what makes the greedy native scans and the cut-minimizing bisection comparable on the figure that decides stream cost — cache-optimizes the index buffer, and encodes the global vertices and the local-to-global meshlet indices while retaining raw local triangle bytes. Quantized, point, and splat arms filter their admitted attributes, every stream resolves its mode, filter, stride, and codec version off the ONE `(kind, stream)` form table rather than a per-arm literal tuple, and the whole blob keys through `InterchangeIdentity.Key` over `ResidencyPolicy.Vector` — every output-affecting column in owner order, so two payloads built at different cluster budgets, codec levels, or attribute weights cannot key alike.
- Receipt: the `Runtime/receipts#RECEIPT_UNION` `StreamSegment(string ArtifactId, int Segments, long Bytes)` slot carries the payload `ArtifactKey`, the cluster count (meshlet) or stream count (other kinds), and the blob length under the ROUTED substrate the caller threads, and the per-level cut aggregate is `ResidencyPayload.LevelCuts`, a PRODUCER-side derivation a consumer folds off the clusters it already holds and NOT a receipt column — `StreamSegment` carries an artifact id, a segment count, and a byte length and nothing else — a re-encode of identical geometry at identical policy stamps the same content key, so emission is auditable through the existing slot, never a new case; the blob dedups on the Persistence blob lane through `ArtifactIndexRow.Admit` and a hit stamps a `Cache` receipt.
- Packages: Alimer.Bindings.MeshOptimizer (`SimplificationOptions.SimplifyLockBorder` freezing open edges across the ladder), QuikGraph (`ForestDisjointSet<uint>` the shared-vertex shell partition, `KernighanLinAlgorithm` over `UndirectedGraph<int, FaceAdjacency>` the cut-minimizing cluster build), CommunityToolkit.HighPerformance (`MemoryOwner<int>` the dense incidence plane, `SpanOwner<T>` the per-cluster curvature scratch), Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, Rasm.Element (project — the seam `ImportedGeometry` leaf carrier), Rasm (project — the kernel `EncodedGeometry` arena with `Channel`/`Descriptors`, the `EncodingChannel` lane roster, and `ChannelDtype.Unpack`), BCL inbox
- Growth: a new encoding is one `ResidencyKind` row carrying its `LeafArm` delegate column, its required-stream set, and one `StreamForm` row per stream it emits; a new meshlet-build strategy is one `ClusterBuild` row whose `Kernel` ordinal routes it to the pinned native kernel or the managed partition build and whose `CostAxis` states which cost it optimizes, the closed switch breaking until the ordinal lands, never a fork of `BuildClusters`; a new attribute is one `ResidencyStream` row with its filtered-stream line; a new measured per-cluster evidence column is one `ResidencyMeshlet` column filled at the one `Cluster` projection, so every build row and every ladder level carries it with no per-arm edit — `Curvature` and `Cut` are the two standing instances, and a per-level roll-up is one fold beside `LevelCuts`; a new filter or decode mode is one `StreamFilter`/`StreamMode` row on the `StreamSpan`; a new posture is one `ResidencyPolicy` column; a new source modality is one `ResidencySource` case; zero new surface — a `MeshletResidencyEncoder`/`SplatPayloadCodec`/`QuantizedVertexEncoder` sibling collapses onto the one `Encode` fold, and parallel `EncodedVertices`/`EncodedIndices`/`EncodedMeshlets` byte fields collapse onto the one `StreamSpan` layout.
- Boundary: every attribute read addresses the seam carrier by descriptor through one `Lane` reader, so a lane the roster grows reaches the encoder with no edit here. This lane owns the content-keyed payload blob and `StreamSpan`; `csharp:Rasm.AppUi/Render/pipeline#TS_PROJECTION` projects every byte window, codec mode, inverse filter, codec version, cluster, bound, and content key without re-derivation. Host-side attribute reads cross through `Runs` alone — AppUi indexes the decoded runs and grows no second stream decoder — while per-cluster measured evidence (bounds, cone, shell, error chain, curvature, cut) travels on `ResidencyPayload.Clusters`, so a footprint consumer widens by the producer's `Curvature` column and re-derives no curvature off the decoded runs, and a build-strategy comparison reads the producer's own `Cut` rather than re-counting duplicated vertices from a decoded stream that no longer knows which cluster each came from. `InterchangeIdentity.Key` covers the whole assembled blob and its COMPLETE byte-changing policy vector, and the payload's own id is the folder's one `InterchangeIdentity.Address` grammar rather than a fourth hand interpolation of it. Process-global index encoding pins ONCE at the composition root through `Mount` — a static constructor is unordered against every other type's init, so its before-first-encode claim held only while nothing else touched the encoder — vertex encoding carries `ResidencyPolicy.CodecVersion` per call and is pinned nowhere, and raw meshlet triangles carry version `0`. Count-bearing native calls receive explicit semantic counts through pinned pointer kernels. Gaussian splat fitting and SPZ/SOG decoding remain companion-owned, and SOG v2 settles that ownership rather than qualifying it: the container is per-plane lossless-WebP images under a `meta.json` codebook indirection, not a packed-block offset model, so it decodes on the companion's own arm and reaches this fold only as the admitted `SplatScan` — `meta.count` seats `SplatCount` and `shN.bands` IS `HarmonicDegree` with no unit conversion, and the DC-head harmonic composition and sigmoid alpha column stand exactly as the SPZ arm left them. The C#-side byte-mirror against that companion holds DECODE-SIDE only: a WebP re-encode is nondeterministic, so a fixture asserting encoded bytes tests the image encoder rather than this contract, and the mirror fixes the decoded `SplatScan` columns alone.

```csharp signature
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

    // The streams a decode of this kind cannot do without. An absent mandatory stream used to allocate a
    // zero-length destination, decode nothing, and report status `0` — so a payload MISSING its positions was
    // indistinguishable from one carrying none, and the decode reported clean over fabricated emptiness.
    public FrozenSet<ResidencyStream> Required { get; }

    public Validation<Error, Unit> Complete(FrozenDictionary<ResidencyStream, StreamSpan> layout) =>
        toSeq(Required).Filter(stream => !layout.ContainsKey(stream)) is { IsEmpty: false } absent
            ? Validation<Error, Unit>.Fail(new ComputeFault.PayloadOverBounds(
                $"<residency-stream-absent:{Key}:{string.Join(',', absent.Map(static stream => stream.Key))}>"))
            : Validation<Error, Unit>.Success(unit);

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
    public static readonly ClusterBuild ConeWeighted = new("cone", kernel: 0, cost: CostAxis.ConeSpread);
    public static readonly ClusterBuild Flex = new("flex", kernel: 1, cost: CostAxis.ConeSpread);
    public static readonly ClusterBuild Spatial = new("spatial", kernel: 2, cost: CostAxis.Locality);
    public static readonly ClusterBuild Bisect = new("bisect", kernel: Managed, cost: CostAxis.VertexCut);

    // The managed build's ordinal, seated OUTSIDE the native span so the pinned kernel's dispatch is a closed
    // switch over `0..2` rather than an identity ladder ending in a catch-all — a fifth row without its own
    // ordinal fails to construct where it used to fall silently into the cone-weighted scan.
    public const int Managed = -1;

    public int Kernel { get; }

    // Meshopt's three builders are greedy forward scans minimizing cone spread or spatial fill, paying whatever
    // vertex duplication the scan order produces; `Bisect` minimizes the shared-vertex CUT instead, so the column
    // states which cost a row optimizes and `ResidencyMeshlet.Cut` is the figure that makes them comparable.
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

// Each case carries its OWN shape census, so a third modality cannot land without one — two `bool`-returning
// validators sitting beside the union could be forgotten by a new case and each discarded WHICH of five
// conditions failed, `Leaf` even returning `true` for a point kind as a shape verdict, conflating "valid" with
// "no further checks apply".
[Union]
public abstract partial record ResidencySource {
    private ResidencySource() { }

    public abstract Validation<Error, Unit> Check();

    public sealed record Leaf(ResidencyKind Kind, ImportedGeometry Geometry) : ResidencySource {
        // Per-lane extent is the arena's own claim, so admission proves only the cross-shape census the arena
        // cannot: declared element count and vertex count are one number, and a topological kind additionally
        // owes whole triangles indexing inside its own vertex range at a width `uint` holds.
        public override Validation<Error, Unit> Check() =>
            Census("leaf-extent", Geometry.VertexCount > 0 && Geometry.Lanes.Count == Geometry.VertexCount)
            & (Kind == ResidencyKind.PointSplat || Kind.SplatBorne
                ? Validation<Error, Unit>.Success(unit)
                : Census("leaf-triangles", Geometry.Indices.Length >= 3 && Geometry.Indices.Length % 3 == 0)
                  & Census("leaf-index-range", Geometry.Indices.AsSpan().ToArray().All(index => index >= 0 && index < Geometry.VertexCount && index <= uint.MaxValue)));
    }

    public sealed record Splat(SplatScan Scan) : ResidencySource {
        // Wire law: `harmonic_degree` is the SH band 0-3, byte-mirrored from `GaussianSplatScan`.
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

    // EVERY output-affecting scalar in owner order, which `Runtime/codecs#CONTENT_ADDRESSING` legislates and the
    // prior key vector broke: three of twelve columns folded, so two payloads built at different cluster budgets,
    // codec levels, or attribute weights keyed IDENTICALLY and the Persistence dedup index served one for the
    // other. The build ROW is a scalar here too — a cone-weighted and a cut-minimizing partition of one mesh emit
    // different bytes — and it folds as its kernel ordinal so the roster's own order is the key's order.
    public ReadOnlySpan<double> Vector => [
        Cluster.Kernel, MaxVertices, MinTriangles, MaxTriangles,
        ConeWeight, SplitFactor, FillWeight, QuantizationBits,
        CodecLevel, CodecVersion, SimplifyTarget, AttributeWeight,
    ];
}

// Cluster-LOD chain columns: Error is object-space simplification error (level 0 = 0); a ROOT carries no parent
// and no parent error, both `Option`-shaped, where `-1` and `+inf` were sentinels every consumer had to know to
// exclude before comparing. A linked child's ParentError is raised to at least max(children) — MONOTONIC
// (ParentError >= Error) — so a screen-space cut (finest level whose Error <= t < ParentError) is crack-free and
// double-draw-free. AppUi reads these and never re-clusters.
// Shell names the connected-component representative of the cluster's own triangles under the shared-vertex
// relation, and a parent link searches WITHIN one shell — so a fine cluster never binds a coarse parent from a
// disjoint piece of geometry whose sphere merely contains it, a cut that then draws two unrelated shells at once.
// Curvature is the cluster's own MEASURED normal-variation bound in radians per object-space unit — the 1/R a
// ray-cone footprint doubles into its spread — appended past the frozen columns so every mirror of this
// descriptor widens by one row rather than re-ordering the four same-typed offset and count slots.
// Cut is the REALIZED shared-boundary-vertex count: how many of this cluster's own vertices a sibling cluster in
// the same level also holds, so the stream pays for them twice. It fills for every ClusterBuild row — that IS the
// point, since the greedy native scans and the cut-minimizing bisection only become comparable on the one figure
// that decides stream cost, and a build chosen on taste rather than this number is the choice this column ends.
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

// per-stream EXT_meshopt_compression bufferView: byte window, Count/ByteStride, decode Mode (attribute/triangle/
// index codec, or Raw for un-encoded meshlet triangle bytes), inverse Filter — the set the AppUi manifest emits
public readonly record struct StreamSpan(int Offset, int Length, int Count, int ByteStride, StreamMode Mode, StreamFilter Filter, int CodecVersion);

// The `(kind, stream) -> form` correspondence as ONE table rather than four hand-written seven-tuples that each
// re-derived mode, filter, stride, and codec version at their own construction site. `Positions` reads
// `Filter.None` under the meshlet arm and `Filter.Exponential` under the other three — a divergence four literal
// tuples could only agree on by inspection — and combinations the codec refuses (`Mode.Raw` under a filter, an
// octahedral filter on a position lane) have no row to occupy. `Lazy` because the table reads `EncodingChannel`
// arities at first touch and every consumer of it runs long after type init.
public readonly record struct StreamForm(StreamMode Mode, StreamFilter Filter, int ByteStride, Func<ResidencyPolicy, int> CodecVersion);

// The assembled stream: role, measured element count, and bytes. Mode, filter, stride, and version come from the
// table, so an arm supplies only what it MEASURED and a seven-slot anonymous tuple stops standing as a parameter
// type at one site and a construction shape at four.
public readonly record struct StreamDraft(ResidencyStream Stream, int Count, ReadOnlyMemory<byte> Bytes);

// exp-packed 3-component carrier (12 bytes) the meshopt exponent filter writes; never read back as floats here,
// so the three words carry no domain meaning of their own and `Slots` is the one number any arm reads off it.
[InlineArray(Slots)]
public struct Packed12 {
    public const int Slots = 3;
    private uint word;
}

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

// The constructor is PRIVATE and `Assemble` the only mint, because every span in `Runs` slices the blob by a
// `Layout` row: a public positional constructor let any caller mint a payload whose declared windows exceed its
// own bytes, and the decode's unguarded slices were then a caller-reachable range fault rather than a refusal.
// Span containment now holds by construction and the interior re-proves nothing.
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

    // The folder's ONE object-plane address grammar, composed rather than interpolated: three siblings
    // spelled `$"{key:x32}:{kind}"` by hand and a separator or hex-width change had three sites to miss.
    public string ArtifactKey => InterchangeIdentity.Address(ContentKey, Kind.Key);

    public long EncodedBytes => Blob.Length;

    // Per-LOD-level cut aggregate, indexed by level: the duplicated-vertex mass one level of the ladder pays,
    // folded off the clusters that already measured it rather than stored as a column a re-encode could contradict.
    // A consumer reads it beside the cluster count the `StreamSegment` receipt carries for the same `ArtifactKey`,
    // so a build-strategy comparison runs on producer evidence with no re-derivation off the decoded runs.
    public Seq<int> LevelCuts =>
        toSeq(Clusters.GroupBy(static cluster => cluster.Level).OrderBy(static level => level.Key))
            .Map(static level => level.Sum(static cluster => cluster.Cut));
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
    // Every component count and stride resolves off the channel descriptor's OWN arity: a literal here silently
    // freezes a lane's width against a roster that grows, which is the per-column read the descriptor law deleted.
    // These are the page's one spelling of each, so no arm carries a second.
    internal static readonly int PositionArity = EncodingChannel.Position.Arity;
    internal static readonly int NormalArity = EncodingChannel.Normal.Arity;
    static readonly int UvArity = EncodingChannel.Uv.Arity;
    static readonly int PositionStride = PositionArity * sizeof(float);
    // The normal lane's own width, never borrowed from the position lane: the octahedral filter packs a normal
    // into one `uint`, so the ENCODED stride is four bytes while the source arity stays the descriptor's, and a
    // literal `PositionStride` on this lane froze a lane's width against a roster that grows.
    static readonly int NormalStride = sizeof(uint);
    static readonly int UvStride = UvArity * sizeof(float);
    const int OctBits = 8;
    const int IndexCodecVersion = 1;
    // KL refinement passes per bisection: the algorithm seeds an arbitrary halving, so a single pass leaves
    // obvious swaps unmade while the gain sequence flattens well before the part size the budget admits.
    const int BisectionPasses = 4;
    // The meshopt meshlet ABI ceilings — a local vertex slot is one byte and the triangle budget is a
    // quarter-aligned span the builders address as a triple. Admission clamps every policy against them AND the
    // per-cluster curvature scratch sizes off them, so the cap is declared once and no second literal drifts.
    const int VertexCeiling = 255;
    const int TriangleCeiling = 512;
    // A triangle whose twice-area falls under this fraction of its own squared extent carries a normal direction
    // assembled from coordinate rounding alone; 2^-24 IS the single-precision significand, so the gate is
    // scale-free and no model tunes it.
    const float SliverFloor = 1f / (1 << 24);

    // EncodeIndexBuffer/EncodeIndexSequence carry NO per-call version arg (unlike EncodeVertexBufferLevel), so the
    // meshlet vertex-table and triangle-index streams follow the process-global EncodeIndexVersion — the ONE fact
    // this lane cannot carry per call, and therefore the only one pinned. `Mount` is an interlocked one-shot the
    // composition root runs, matching the `Runtime/archive#HDF_ARCHIVE` `HdfArchive.Mount` precedent, because a
    // static constructor is unordered against every other type's init and its "before the first Encode" claim
    // holds only while nothing else touches the native encoder. The VERTEX version is policy-carried at every
    // call, so pinning it globally "for symmetry" was two authorities for one fact and the pin deletes.
    public static Fin<Unit> Mount() =>
        Interlocked.Exchange(ref mounted, 1) is 0
            ? Fin.Succ(fun(() => Meshopt.EncodeIndexVersion(IndexCodecVersion))())
            : Fin.Fail<Unit>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Initialized, new ContractEvidence.None())));

    static int mounted;

    // The `(kind, stream) -> form` correspondence: one row per legal pair, so the four arms below supply what they
    // MEASURED and re-derive no mode, filter, stride, or version. A pair with no row is a stream that kind does
    // not emit, which `Assemble` refuses rather than encoding under a guessed form.
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
        // Opacity crosses raw: a [0,1] scalar gains nothing from the exponent filter's shared-component pass, and
        // the renderer reads it verbatim beside the filtered attribute streams.
        (ResidencyKind.GaussianSplat, ResidencyStream.Alphas, new StreamForm(StreamMode.Attributes, StreamFilter.None, sizeof(float), static p => p.CodecVersion)),
    }.ToFrozenDictionary(static row => (row.Item1, row.Item2), static row => row.Item3));

    // One dispatch level: the source Switch resolves modality, the Leaf arm reads the kind's row-owned LeafArm
    // column — dispatch plus data retrieval, never a second full-coverage Switch nested in the arm.
    public static Fin<ResidencyPayload> Encode(ResidencySource source, ResidencyPolicy policy) =>
        Admit(source, policy).Bind(static admitted => admitted.Source.Switch(
            state: admitted.Policy,
            leaf: static (p, l) => l.Kind.LeafArm(l.Geometry, p),
            splat: static (p, s) => SplatEncode(s.Scan, p)));

    // The ROUTED substrate arrives from the caller that ran the encode, never a constant: an encode routed onto a
    // device stamps the device, so a receipt naming a substrate that never ran cannot be minted here at all.
    public static ComputeReceipt.StreamSegment Receipt(ResidencyPayload payload, CorrelationId correlation, WorkLane lane, Substrate substrate, Duration elapsed) =>
        new(payload.ArtifactKey, payload.Clusters.IsEmpty ? payload.Layout.Count : payload.Clusters.Count, payload.EncodedBytes) {
            Scope = new ReceiptScope.Execution(correlation, lane, substrate, AllocationClass.PooledMemory, elapsed),
        };

    // The decode projection PAIRED with the meshlet encode — the one host-side attribute reader, so AppUi never
    // grows a second stream decoder. Each stream decodes under its own Layout row: the vertex codec for
    // attribute runs, the index-sequence codec for the meshlet vertex table, raw triangle bytes verbatim; the
    // octahedral normal filter unpacks in place before the snorm8 lanes widen to unit floats. MESHLET-CLUSTER
    // ONLY: the quantized/point/splat kinds ship exponent-filtered streams whose consumer is the web viewer's
    // meshopt decoder, and a host read of those kinds is a routing defect this gate names rather than absorbs.
    public static Fin<ResidencyRuns> Runs(ResidencyPayload payload) {
        if (payload.Kind != ResidencyKind.MeshletCluster) {
            return Fin.Fail<ResidencyRuns>(new ComputeFault.Violation(ComputeArea.Runtime, new ComputeViolation.Contract(ComputeContract.Compatible, new ContractEvidence.Key(payload.Kind.Key))));
        }
        ReadOnlySpan<byte> blob = payload.Blob.Span;
        Vector3[] positions = new Vector3[Count(payload, ResidencyStream.Positions)];
        uint[] packedNormals = new uint[Count(payload, ResidencyStream.Normals)];
        Vector2[] uvs = new Vector2[Count(payload, ResidencyStream.Uvs)];
        uint[] table = new uint[Count(payload, ResidencyStream.Indices)];
        // Four INDEPENDENT decodes accumulate, so a payload whose normals and uvs both fail names both — a
        // bitwise OR of four native statuses blurred them into one bit and one message naming no stream at all.
        // Mandatory-stream presence accumulates beside them, so an absent positions stream refuses BY NAME rather
        // than allocating a zero-length destination that decodes nothing and reports clean.
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

    // One decode body, the stream's own Mode selecting the codec, and its verdict NAMES the stream. An absent
    // OPTIONAL stream decodes nothing and succeeds — a source with no unwrap genuinely carries no uvs — while an
    // absent mandatory one already refused at the completeness gate above.
    static Validation<Error, Unit> Decoded<T>(ResidencyPayload payload, ResidencyStream stream, Span<T> destination, ReadOnlySpan<byte> blob) where T : unmanaged =>
        destination.Length > 0 && payload.Layout.TryGetValue(stream, out StreamSpan span)
            ? (span.Mode == StreamMode.Indices
                ? Meshopt.DecodeIndexSequence(destination, blob.Slice(span.Offset, span.Length))
                : Meshopt.DecodeVertexBuffer(destination, blob.Slice(span.Offset, span.Length))) is var status && status != 0
                ? Validation<Error, Unit>.Fail(new ComputeFault.PayloadOverBounds(
                    $"<residency-runs-decode:{payload.ArtifactKey}:{stream.Key}:{status}>"))
                : Validation<Error, Unit>.Success(unit)
            : Validation<Error, Unit>.Success(unit);

    // The OctBits=8 encode stores snorm8 lanes; the filter decode rehydrates them in place and this widening
    // lifts the three component lanes to unit floats — the fourth lane is the filter's reconstruction slot,
    // never data.
    static float[] UnpackSnorm(ReadOnlySpan<uint> packed) {
        float[] wide = new float[packed.Length * NormalArity];
        ReadOnlySpan<sbyte> lanes = MemoryMarshal.Cast<uint, sbyte>(packed);
        for (int v = 0; v < packed.Length; v++) {
            (wide[v * NormalArity], wide[(v * NormalArity) + 1], wide[(v * NormalArity) + 2]) =
                (lanes[v * 4] / 127f, lanes[(v * 4) + 1] / 127f, lanes[(v * 4) + 2] / 127f);
        }
        return wide;
    }

    // Admission CARRIES its evidence: `AdmittedResidency` is the only value the arms take, so an interior arm
    // cannot be handed a policy this fold never proved. The prior form returned its two inputs unchanged, which
    // is an admission that admits nothing. Nine policy checks and the source's own shape census accumulate
    // together, so a policy with three breaches and a malformed source names all four — the ternary that returned
    // policy faults first DISCARDED every source fault whenever any policy check tripped.
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
        // The census runs over the WHOLE level before any cluster projects, because a cut is a relation between
        // clusters and no single cluster can measure it from its own table. The pooled plane releases at this
        // scope's end — the projection reads it and nothing outlives the loop.
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

    // Each coarser level simplifies the prior level's index buffer through the Meshopt.Simplify ladder (result_error
    // scaled to object space by SimplifyScale), re-clusters, and links each fine cluster to the coarse parent whose
    // sphere CONTAINS it, falling back to nearest center. Monotonic guarantee at link time: a parent's Error rises to
    // at least max(children) before children stamp ParentError, so a screen-space cut is crack-free and
    // double-draw-free. Ladder terminates when a level stops shrinking or one meshlet remains; roots carry
    // roots carry NO parent and NO parent error, both absent rather than sentinel-valued.
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
            // `LockBorder` freezes open edges, so a shell's boundary survives every ladder level identically and a
            // parent link never inherits a border the simplifier moved — the crack the `Shell` column otherwise
            // works around after the fact. `options: 0` was a bare int standing where a typed flag set belongs.
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

    // Each fine cluster binds the nearest-center coarse cluster whose sphere CONTAINS it (d + fineRadius <=
    // coarseRadius), else nearest center, so a child never binds outside its parent's coverage; the parent's Error
    // raises to max(parent, children) and children re-stamp ParentError from the raised value.
    // Absence is `Option`, never `-1` or `+inf`: a root carries NO parent and NO parent error, and both sentinels
    // were values a consumer had to know to exclude before comparing. The parent search answers `Option<int>` for
    // the same reason — `float.MaxValue` standing for "no candidate in this shell" is a distance a real cluster
    // could in principle carry.
    static void Link(List<ResidencyMeshlet> all, int fineFirst, int fineCount, int coarseFirst, int coarseCount) {
        for (int f = fineFirst; f < fineFirst + fineCount; f++) {
            Option<int> best = Bound(all, f, coarseFirst, coarseCount);
            best.Iter(parent => {
                all[parent] = all[parent] with { Error = Math.Max(all[parent].Error, all[f].Error) };
                all[f] = all[f] with { Parent = Some(parent) };
            });
        }
        // The stamp runs as a SECOND pass by construction: a parent's error rises to at least the max of its
        // children in the pass above, so a child stamping `ParentError` in that same pass would seal a value the
        // next sibling was about to raise — the monotone guarantee (`ParentError >= Error`) is exactly what the
        // split buys, and the two passes are one law rather than two.
        for (int f = fineFirst; f < fineFirst + fineCount; f++) {
            all[f] = all[f] with { ParentError = all[f].Parent.Map(parent => Math.Max(all[parent].Error, all[f].Error)) };
        }
    }

    // The covering parent wins, the nearest center is the fallback, and both search WITHIN the fine cluster's own
    // shell — a parent from a disjoint piece of geometry whose sphere merely contains it draws two unrelated
    // shells at one cut.
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
        // The decimator returns a REMAP over the source vertices, so the gather is a span copy per kept point —
        // `float[]` carries no slice of its own and the array API would allocate one buffer per point.
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

    // The ONE payload mint. Each draft resolves its `(kind, stream)` form off the table, so an arm supplying a
    // stream that kind does not emit refuses here instead of encoding under a guessed mode. The content key binds
    // the LANDED `InterchangeIdentity.Key(string, ReadOnlySpan<byte>, ReadOnlySpan<double>)` declaration — three
    // parameters, the policy span not a scalar tail — and folds `policy.Vector`, every output-affecting column in
    // owner order, where three loose scalars against that three-parameter member both mis-bound the arity and let
    // two payloads at different cluster budgets key identically.
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

    // An optional lane contributes a draft or nothing: two `Has*` forwarders and two `if`-append statements per
    // arm stated the same law four times, and the lane read now resolves BY DESCRIPTOR at one site.
    static Seq<StreamDraft> Optional(ImportedGeometry leaf, EncodingChannel channel, ResidencyStream stream, Func<float[], ReadOnlyMemory<byte>> encode) =>
        leaf.Lanes.Descriptors.Exists(descriptor => descriptor.Channel == channel)
            ? Seq(new StreamDraft(stream, leaf.VertexCount, encode(Lane(leaf.Lanes, channel))))
            : Seq<StreamDraft>();

    static ReadOnlyMemory<byte> EncodeUvs(ReadOnlySpan<float> uvs, int count, ResidencyPolicy policy) =>
        EncodeStream(MemoryMarshal.Cast<float, Vector2>(uvs[..(count * UvArity)]), policy);

    // `Packed12.Slots` is the carrier's OWN slot count, not a channel width: the exponent filter's
    // shared-component mode packs exactly that many lanes per element, so every channel this arm serves is a
    // three-component one by construction and the number lives on the type that defines it.
    static ReadOnlyMemory<byte> EncodeExp(ReadOnlySpan<float> floats, int count, ResidencyPolicy policy) {
        Packed12[] packed = new Packed12[count];
        Meshopt.EncodeFilterExp<Packed12>(packed, policy.QuantizationBits, floats[..(count * Packed12.Slots)], EncodeExpMode.EncodeExpSharedComponent);
        return EncodeStream<Packed12>(packed, policy);
    }

    // The quad width is the octahedral filter's own element shape — three data lanes plus its reconstruction slot.
    static ReadOnlyMemory<byte> EncodeNormals(ReadOnlySpan<float> normals, int count, ResidencyPolicy policy) {
        float[] quad = new float[count * 4];
        for (int v = 0; v < count; v++) { normals.Slice(v * NormalArity, NormalArity).CopyTo(quad.AsSpan(v * 4)); }
        uint[] packed = new uint[count];
        Meshopt.EncodeFilterOct<uint>(packed, OctBits, quad);
        return EncodeStream<uint>(packed, policy);
    }

    // Level and version travel WITH the policy that decides both, so no call site can pair one row's level with
    // another's version — two loose ints at every encode site were exactly that pairing waiting to happen.
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

    // ONE descriptor-addressed lane reader serves every channel: the descriptor names the dtype, so a unorm8 colour
    // and a float32 position lift through the same call and no arm carries a literal component stride. An absent
    // channel answers the empty array — a MISSING DESCRIPTOR, never a zero-filled buffer a consumer length-probes.
    static float[] Lane(EncodedGeometry arena, EncodingChannel channel) {
        if (arena.Descriptors.Find(descriptor => descriptor.Channel == channel).Case is not EncodingChannelDescriptor found) { return []; }
        float[] raw = new float[found.Floats];
        found.Dtype.Unpack(arena.Channel(channel).Span, raw);
        return raw;
    }

    // The ONE per-cluster descriptor projection every build row and every ladder level lands through: the native
    // bounds kernel and the managed curvature measure read the same local vertex table, local triangle bytes, and
    // global position stream, so a further measured column is one line here rather than a per-arm fill.
    // The native `Bounds` is read BY NAME. A positional reinterpret over a foreign struct assumed a declaration
    // order the binding's catalog never pinned — it enumerates the fields grouped by type — so a layout whose
    // `cone_apex` preceded `radius` would have keyed every cluster radius wrong while every type checked.
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

    // Cluster-incidence census over ONE level: each build row emits a local-to-global vertex table in which a
    // global corner appears at most once, so counting corners across the level's clusters counts CLUSTERS, and a
    // corner seen more than once is a vertex the stream duplicates. The walk reads the per-cluster spans rather
    // than the raw table because the managed bisection strides its cursor by the policy cap and leaves the slots
    // between clusters unwritten — a span over the whole table would count that gap as geometry.
    // The key space is DENSE and bounded — every global corner is below the leaf's own vertex count — so the
    // census is a pooled counting PLANE indexed directly, not a hash map paying a bucket probe per corner across
    // every cluster of every ladder level. The owner is disposable and the caller brackets it, which is what
    // makes the rent a release rather than a leak.
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

    // This cluster's share of that census: the vertices it holds that a sibling in the same level also holds. A
    // level of one cluster measures zero by construction, which is the honest floor — nothing is duplicated when
    // nothing borders.
    static int Cut(ReadOnlySpan<uint> localVertices, ReadOnlySpan<int> incidence) {
        int shared = 0;
        foreach (uint global in localVertices) {
            if (incidence[(int)global] > 1) { shared++; }
        }

        return shared;
    }

    // The cluster's own normal-variation BOUND: the largest turn between the face normals of two triangles that
    // SHARE A VERTEX — this page's one triangle-adjacency relation, the same shared-vertex relation `FaceAdjacency`
    // weights and `Shells` partitions on — over the distance their centroids span. The quotient is radians per
    // object-space unit, so a cylinder facet pair reads 1/R and a ray-cone consumer doubles the column into its
    // spread; `Atan2` against the cross length holds the near-planar precision `Acos` loses exactly where the
    // measure decides between flat and barely curved. Edge-only adjacency is the rejected relation. A
    // cluster whose triangles share no vertex admits no path from one facet to another, so every path across it is
    // planar and zero is the MEASURED bound, never an unfilled slot; the same holds level by level, because each
    // coarser level measures its own simplified triangles rather than inheriting a finer level's turn.
    static float Curvature(ReadOnlySpan<uint> localVertices, ReadOnlySpan<byte> localTriangles, int triangleCount, ReadOnlySpan<float> positions) {
        ReadOnlySpan<Vector3> points = MemoryMarshal.Cast<float, Vector3>(positions);
        // ~19 KB of scratch per call (two 512-slot `Vector3` planes, a 1536-slot chain, a 255-slot head table) and
        // the call runs once per cluster per ladder level, so the frames NEST — a `stackalloc` quartet at that
        // depth is a stack budget no ABI ceiling declares. The pooled owners release on the same scope and the
        // ceilings still size them, so the ABI cap stays the one number and the memory stops riding the stack.
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
            // Vector3.Zero is unspellable as a unit normal, so it IS the sliver sentinel the pair fold skips.
            normals[t] = cross.LengthSquared() > SliverFloor * SliverFloor * extent * extent ? Vector3.Normalize(cross) : Vector3.Zero;
            centroids[t] = (a + b + c) / 3f;
            // Corner-keyed incidence chain: each corner visits the triangles already holding that local slot, then
            // joins it — one linear walk over the cluster's corners reaches every vertex-adjacent pair, where a
            // pairwise scan pays the square of the admitted triangle budget and a shared-EDGE map misses a corner.
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

    // One adjacent pair's turn rate. A pair with no direction on either side, or with no travelled distance
    // between its centroids, measures no rate and drops to the fold's identity rather than dividing through a
    // degeneracy — an infinity here would swallow the whole cluster's bound.
    static float Rate(Vector3 first, Vector3 firstCentroid, Vector3 second, Vector3 secondCentroid) =>
        Vector3.Distance(firstCentroid, secondCentroid) is var distance && distance > 0f && first != Vector3.Zero && second != Vector3.Zero
            ? float.Atan2(Vector3.Cross(first, second).Length(), Vector3.Dot(first, second)) / distance
            : 0f;

    // Shared-vertex connectivity over the index buffer through the admitted union-find forest: each triangle
    // unions its three corners, so `FindSet` answers the component representative for any vertex and the cluster's
    // first corner names its shell. `SetCount` is the shell census the ladder reads — one meshlet remaining per
    // shell is the honest ladder terminal, where a global count-of-one never terminates a multi-shell mesh.
    // The shell census is a VALUE, not an opaque `Func` a consumer cannot index: the ordinal table returns whole
    // and `Of` answers `Option` for a vertex the walk never saw, where indexing the closure's captured dictionary
    // threw `KeyNotFoundException` on the page's otherwise zero-throw surface.
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
        if (faces.Count <= policy.MaxTriangles) { return Seq(faces); }
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
            .GroupBy(static pair => pair))
            .Map(static group => new FaceAdjacency(group.Key.Low, group.Key.High, group.Count()));

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
        // The cursor advances by what this part WROTE, not by the policy cap. Striding by the cap left the slots
        // between clusters unwritten, and `MeshletEncode` then sliced the whole table to the tail's extent and
        // handed those gaps to `EncodeIndexSequence` — unwritten `uint` slots riding the encoded index stream as
        // global vertex `0` a decode allocates and reads back as real geometry. Compacting also makes the claim
        // one line above TRUE: this is now the native builders' own layout, not a strided approximation of it.
        return (cursor.Meshlets + 1, cursor.Vertices + local.Count, cursor.Triangles + (triangle * 3));
    }

    // Safe span overloads pass element-span length as the semantic vertex/triangle/point count (wrong for
    // interleaved-float positions and 3-byte triangles), so these four count-bearing builds pin and pass true
    // counts; the ClusterBuild row resolves by identity INSIDE the fixed block because meshlet pointers cannot
    // cross a generated-Switch lambda — the pinned kernel is the named exemption carrying this one row branch.
    static unsafe int BuildClusters(ReadOnlySpan<uint> indices, ReadOnlySpan<float> positions, int vertexCount, ResidencyPolicy policy,
        Meshlet[] meshlets, uint[] meshletVertices, byte[] meshletTriangles) {
        if (policy.Cluster.Kernel is ClusterBuild.Managed) { return BisectClusters(indices, policy, meshlets, meshletVertices, meshletTriangles); }
        fixed (Meshlet* meshlet = meshlets)
        fixed (uint* vertices = meshletVertices)
        fixed (byte* triangles = meshletTriangles)
        fixed (uint* index = indices)
        fixed (float* position = positions) {
            // CLOSED on the row's own ordinal, so a new `ClusterBuild` row without a kernel ordinal breaks the
            // switch — the identity ladder it replaces ended in a catch-all that ran the cone-weighted scan for
            // any row it did not name, turning a missing arm into a silent wrong build.
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

    // tile bounding sphere over leaf/scan positions so ResidencyPayload is self-describing for the AppUi manifest
    // (frustum cull + placement) — reads the center[3]+radius prefix of the native Bounds
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

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

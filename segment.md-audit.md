# 1. Keep descriptor evidence on its actual owners

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:71`

```csharp
public readonly record struct DescriptorSolve(
    DescriptorProfile Spectral, EigenSolution<double, Arr<double>> Eigen, bool Cached,
    int RequestedEigenpairs, int ReturnedEigenpairs,
    int SkippedDegenerateFaces = 0, Option<SpectralAssembly> Assembly = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Spectral.IsValid,
        Eigen.IsValid,
        RequestedEigenpairs >= 1 && ReturnedEigenpairs > 0 && ReturnedEigenpairs <= RequestedEigenpairs,
        ValidityClaim.CountAtLeast(count: SkippedDegenerateFaces, floor: 0),
        ValidityClaim.Evidence(Assembly));
}
```

## To

```csharp
public readonly record struct DescriptorSolve(
    DescriptorProfile Spectral, EigenSolution<double, Arr<double>> Eigen, bool Cached,
    int SkippedDegenerateFaces = 0) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Spectral.IsValid, Eigen.IsValid,
        ValidityClaim.CountAtLeast(count: SkippedDegenerateFaces, floor: 0));
}
```

## Why

`EigenSolution` already owns and validates both pair counts. `SpectralAssembly` is independent DEC evidence owned by `DecAssembly.Build`, not evidence of applying a spectral filter; its optional attachment makes one descriptor result change shape according to a requested projection.

## Change

Delete both pair-count properties, the duplicate invariant, and `Assembly`. Construct `DescriptorSolve` with the eigen solution as the sole eigensolve evidence; callers needing DEC assembly use its existing intent and owner directly.

## Delta

Target code-fence LOC: 11 to 7, net -4. Module-level members: -3 positional properties, +0, net -3. Module-level types: unchanged.

# 2. Collapse descriptor solve and projection forwarding

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:112`

```csharp
internal static Fin<TOut> DescribeShape<TOut>(MeshSpace space, MeshDescriptor kind, int eigenpairs, Op key) =>
    from descriptor in DescribeSpectralShape(space: space, spec: kind, eigenpairs: eigenpairs,
        includeAssembly: typeof(TOut) == typeof(DescriptorResult) || typeof(TOut) == typeof(DescriptorSolve), key: key)
    from output in ProjectDescriptor<TOut>(descriptor: descriptor, key: key)
    select output;
internal static Fin<DescriptorResult> DescribeSpectralShape(MeshSpace space, MeshDescriptor spec, int eigenpairs, Op key) =>
    DescribeSpectralShape(space: space, spec: spec, eigenpairs: eigenpairs, includeAssembly: false, key: key);
private static Fin<DescriptorResult> DescribeSpectralShape(MeshSpace space, MeshDescriptor spec, int eigenpairs, bool includeAssembly, Op key) =>
    from bundle in space.Cache.SpectralBasisBundleOf(k: Dimension.Create(value: eigenpairs), key: key)
    from spectral in spec.Filter.Evaluate(basis: bundle.Basis, sources: spec.Sources, policy: spec.Policy, key: key)
    from assembly in includeAssembly ? DecAssembly.Build(space: space, key: key).Map(calculus => Some(calculus.Assembly)) : Fin.Succ(Option<SpectralAssembly>.None)
    select new DescriptorResult(Values: spectral.Values, Solve: new DescriptorSolve(Spectral: spectral.Profile, Eigen: bundle.Eigen, Cached: bundle.Cached, RequestedEigenpairs: eigenpairs, ReturnedEigenpairs: bundle.Eigen.ReturnedPairs, SkippedDegenerateFaces: bundle.SkippedDegenerateFaces, Assembly: assembly));
private static Fin<TOut> ProjectDescriptor<TOut>(DescriptorResult descriptor, Op key) =>
    ResultProjection.Rows<DescriptorResult, TOut>(self: descriptor, key: key, owner: typeof(MeshDescriptor),
        ProjectionRow.Of<DescriptorSolve>(() => Fin.Succ(descriptor.Solve)),
        ProjectionRow.Of<SpectralDescriptor>(() => Fin.Succ(new SpectralDescriptor(Values: descriptor.Values, Profile: descriptor.Solve.Spectral))),
        ProjectionRow.Of<DescriptorProfile>(() => Fin.Succ(descriptor.Solve.Spectral)),
        ProjectionRow.Of<Arr<double>>(() => Fin.Succ(descriptor.Values)));
```

## To

```csharp
internal static Fin<TOut> DescribeShape<TOut>(MeshSpace space, MeshDescriptor spec, int eigenpairs, Op key) =>
    from result in DescribeSpectralShape(space, spec, eigenpairs, key)
    from output in ResultProjection.Rows<DescriptorResult, TOut>(self: result, key: key, owner: typeof(MeshDescriptor),
        ProjectionRow.Of<DescriptorSolve>(() => Fin.Succ(result.Solve)),
        ProjectionRow.Of<SpectralDescriptor>(() => Fin.Succ(new SpectralDescriptor(result.Values, result.Solve.Spectral))),
        ProjectionRow.Of<DescriptorProfile>(() => Fin.Succ(result.Solve.Spectral)),
        ProjectionRow.Of<Arr<double>>(() => Fin.Succ(result.Values)))
    select output;
internal static Fin<DescriptorResult> DescribeSpectralShape(MeshSpace space, MeshDescriptor spec, int eigenpairs, Op key) =>
    from bundle in space.Cache.SpectralBasisBundleOf(Dimension.Create(eigenpairs), key)
    from spectral in spec.Filter.Evaluate(bundle.Basis, spec.Sources, spec.Policy, key)
    select new DescriptorResult(spectral.Values,
        new DescriptorSolve(spectral.Profile, bundle.Eigen, bundle.Cached, bundle.SkippedDegenerateFaces));

// DescribeSpectralShape(MeshSpace, MeshDescriptor, int, bool, Op) DELETED
// ProjectDescriptor DELETED
```

## Why

The `typeof(TOut)` branch reimplements output dispatch outside `ResultProjection`; the overload and `ProjectDescriptor` then each forward one call. Once unrelated DEC assembly leaves `DescriptorSolve`, every projection is a pure read of one descriptor result and the generated identity fallthrough already owns the whole-result row.

## Change

Delete the forwarding overload, `includeAssembly`, and `ProjectDescriptor`. Keep one internal solve used by descriptor clustering and inline the four non-identity projection rows into `DescribeShape`; do not add an explicit `DescriptorResult` row because `ResultProjection.Rows` already supplies identity fallthrough.

## Delta

Target code-fence LOC: 18 to 15, net -3. Module-level members: -2 methods, +0, net -2. Module-level types: unchanged. Reflection output tests: -2, net -2.

# 3. Flatten feature evidence onto the edge carrier

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:208`

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct FeatureVerdict(MeshFeatureKind Kind, Option<double> DihedralRadians, Option<double> SignedDihedralRadians, Option<double> CurvatureSignal) {
    internal static FeatureVerdict Topological(MeshFeatureKind kind) => new(Kind: kind, DihedralRadians: None, SignedDihedralRadians: None, CurvatureSignal: None);
}

[StructLayout(LayoutKind.Auto)] public readonly record struct FeatureEdge(int A, int B, FeatureVerdict Verdict);
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:220`

```csharp
public int CurvatureRejectedVertices => Math.Max(val1: 0, val2: TopologyVertexCount - CurvatureFiniteVertices);
public int CountOf(MeshFeatureKind kind) => Census.Find(key: kind).IfNone(0);
```

## To

```csharp
[StructLayout(LayoutKind.Auto)]
public readonly record struct FeatureEdge(
    int A, int B, MeshFeatureKind Kind,
    Option<double> SignedDihedralRadians = default,
    Option<double> CurvatureSignal = default);

// FeatureVerdict DELETED
// FeatureVerdict.Topological DELETED
// FeatureEdges.CountOf DELETED
```

## Why

`FeatureVerdict` wraps classification that belongs to one edge, and unsigned dihedral magnitude is exactly `Abs(SignedDihedralRadians)`. The wrapper contradicts existing consumers, which already read `FeatureEdge.Kind` directly.

## Change

Delete `FeatureVerdict`, `Topological`, and the unconsumed `CountOf` convenience wrapper. Return `(Kind, SignedDihedralRadians, CurvatureSignal)` from classification, construct `FeatureEdge` once after the topology pair is known, and read `edge.Kind` in the census and projection. Rename `NgonInteriorSkipped` to `NgonInterior`; skipping is processing behavior, not a geometric kind.

## Delta

Target code-fence LOC: net -11 after construction-site compaction. Module-level members: -5 verdict members, -1 `FeatureEdge.Verdict`, and -1 helper, +3 direct edge properties, net -4. Module-level types: -1, +0, net -1.

# 4. Keep feature admission at the intent boundary

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:267`

```csharp
internal static Fin<FeatureEdges> DetectFeatureEdgesDetailed(MeshSpace space, double dihedralRadians, Op key) =>
    from policy in MeshFeaturePolicy.Of(dihedralRadians: dihedralRadians, space: space, faceRegions: Option<Arr<int>>.None, key: key)
    from features in DetectFeatureEdgesDetailed(space: space, policy: policy, key: key)
    select features;
internal static Fin<FeatureEdges> DetectFeatureEdgesDetailed(MeshSpace space, MeshFeaturePolicy policy, Op key) =>
    policy.Admit(space: space, key: key).Bind(activePolicy => space.FaceNormals(key: key).Map(faceNormals => {
```

## To

```csharp
// DetectFeatureEdgesDetailed(MeshSpace, double, Op) DELETED
```

```csharp
internal static Fin<FeatureEdges> DetectFeatureEdgesDetailed(MeshSpace space, MeshFeaturePolicy policy, Op key) =>
    space.FaceNormals(key: key).Map(faceNormals => {
```

## Why

The scalar overload has no consumer, and the only policy-bearing call enters through `VectorIntent.Features`, which already runs `policy.Admit(space, key)`. The kernel repeats the aggregate gate and all three generated value-object checks on every execution.

## Change

Delete the unused scalar overload. Treat the policy-bearing kernel as admitted-only and remove its `Admit` bind and closing wrapper; retain `MeshFeaturePolicy.Of` for raw policy construction and `VectorIntent.Features` as the aggregate-and-mesh admission boundary.

## Delta

Target code-fence LOC: net -7. Module-level members: -1 method, +0, net -1. Module-level types: unchanged. Runtime policy admissions per feature request: 2 to 1, net -1.

# 5. Localize feature-analysis scratch state

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:265`

```csharp
private readonly record struct FeatureCurvatureSignals(Arr<Option<double>> Edge, int FiniteVertices);
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:313`

```csharp
private static double SignedDihedral(Mesh mesh, int edge, int[] faces, Arr<Vector3d> faceNormals, double angle) {
    Line line = mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: edge);
    if (!line.IsValid) return angle;
    Vector3d axis = line.To - line.From;
    if (!axis.Unitize()) return angle;
    double sign = Vector3d.CrossProduct(a: faceNormals[index: faces[0]], b: faceNormals[index: faces[1]]) * axis;
    return sign < 0.0 ? -angle : angle;
}
```

## To

```csharp
// FeatureCurvatureSignals DELETED
```

```csharp
// SignedDihedral DELETED
```

## Why

The record is a two-value return packet used once, and `SignedDihedral` is a one-call helper whose body is part of the classification decision. Neither owns a reusable module concept.

## Change

Return `(Arr<Option<double>> Edge, int FiniteVertices)` from `EdgeCurvatureSignals`. Inline the signed-angle calculation into `ClassifySmoothFeature`, using a local `Line`/axis pattern before the ridge/valley decision.

## Delta

Target code-fence LOC: net -3. Module-level members: -3 positional/helper members, +0, net -3. Module-level types: -1, +0, net -1.

# 6. Make normalized cut an honest spectral bisection

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:392`

```csharp
public sealed record NormalizedCutCase(MeshScalars Values, Dimension RegionCount, Dimension Eigenpairs, Dimension MaxIterations, PositiveMagnitude Tolerance) : MeshSegmentation;
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:404`

```csharp
public static Fin<MeshSegmentation> NormalizedCut(MeshScalars values, int regionCount, int eigenpairs, int maxIterations, double tolerance, Op? key = null) =>
    key.OrDefault() switch { Op op => from regions in op.AcceptValidated<Dimension>(candidate: regionCount) from _ in guard(regionCount > 1, op.InvalidInput()) from pairs in op.AcceptValidated<Dimension>(candidate: eigenpairs) from __ in guard(eigenpairs > 1, op.InvalidInput()) from cap in op.AcceptValidated<Dimension>(candidate: maxIterations) from eps in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance) select (MeshSegmentation)new NormalizedCutCase(Values: values, RegionCount: regions, Eigenpairs: pairs, MaxIterations: cap, Tolerance: eps) };
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:503`

```csharp
from _ in guard(scalars.FiniteCount >= cut.RegionCount.Value, state.Key.InvalidInput())
from adjacency in FaceAdjacency(space: state.Space, key: state.Key)
from system in NormalizedCutSystemOf(adjacency: adjacency, scalars: scalars.FaceValues, tolerance: cut.Tolerance.Value, key: state.Key)
from eigen in MatrixKernel.GeneralizedEigenpairsDetailed(stiffness: system.Laplacian, mass: system.Degree, k: Math.Min(val1: cut.Eigenpairs.Value, val2: Math.Max(val1: 1, val2: state.Space.Native.Faces.Count - 1)), key: state.Key)
from projection in FiedlerProjection(eigen: eigen, expectedCount: scalars.FaceValues.Count, key: state.Key)
let masked = MaskByScalars(projection: projection, scalars: scalars.FaceValues)
from kmeans in ClusterLabels(values: masked, count: cut.RegionCount.Value, maxIterations: cut.MaxIterations, tolerance: cut.Tolerance.Value, key: state.Key)
```

## To

```csharp
public sealed record NormalizedCutCase(MeshScalars Values, Dimension MaxIterations, PositiveMagnitude Tolerance) : MeshSegmentation;
public static Fin<MeshSegmentation> NormalizedCut(MeshScalars values, int maxIterations, double tolerance, Op? key = null) =>
    key.OrDefault() switch { Op op => from cap in op.AcceptValidated<Dimension>(maxIterations)
                                      from eps in op.AcceptValidated<PositiveMagnitude>(tolerance)
                                      select (MeshSegmentation)new NormalizedCutCase(values, cap, eps) };
```

```csharp
from _ in guard(scalars.FiniteCount >= 2, state.Key.InvalidInput())
from adjacency in FaceAdjacency(state.Space, state.Key)
from system in NormalizedCutSystemOf(adjacency, scalars.FaceValues, cut.Tolerance.Value, state.Key)
from eigen in MatrixKernel.GeneralizedEigenpairsDetailed(system.Laplacian, system.Degree, k: 2, key: state.Key)
from projection in eigen.PairsIn(EigenOrder.Ascending, state.Key).Bind(pairs =>
    pairs.Count >= 2 && pairs[1].Eigenvector.Count == scalars.FaceValues.Count && pairs[1].Eigenvector.ForAll(double.IsFinite)
        ? Fin.Succ(pairs[1].Eigenvector)
        : Fin.Fail<Arr<double>>(state.Key.InvalidResult()))
from kmeans in ClusterLabels(
    values: new Arr<double>([.. Enumerable.Range(0, projection.Count)
        .Select(index => double.IsFinite(scalars.FaceValues[index]) ? projection[index] : double.NaN)]),
    count: 2, maxIterations: cut.MaxIterations, tolerance: cut.Tolerance.Value, key: state.Key)

// FiedlerProjection DELETED
// MaskByScalars DELETED
```

## Why

The arm computes only the second generalized eigenvector. Clustering that one Fiedler coordinate into an arbitrary requested region count is not k-way normalized cuts; it is a spectral bisection with an invented multi-region surface.

## Change

Fix the capability at two regions, request exactly two eigenpairs, and remove `RegionCount` and `Eigenpairs` from the case and factory. Inline the one-use Fiedler and scalar-mask helpers in the arm. Multi-region scalar clustering remains available through `DescriptorClusters` and `ScalarBands` rather than being falsely attributed to normalized cuts.

## Delta

Target code-fence LOC: net -8. Module-level members: -2 case properties and -2 helper methods, +0, net -4. Factory parameters: -2. Module-level types: unchanged.

# 7. Carry the segmentation request as its evidence identity

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:408`

```csharp
[SmartEnum<int>]
public sealed partial class MeshSegmentationAlgorithm {
    public static readonly MeshSegmentationAlgorithm ScalarThresholdComponents = new(key: 0);
    public static readonly MeshSegmentationAlgorithm ScalarBandComponents = new(key: 1);
    public static readonly MeshSegmentationAlgorithm SeededRegionGrow = new(key: 2);
    public static readonly MeshSegmentationAlgorithm DescriptorScalarClusters = new(key: 3);
    public static readonly MeshSegmentationAlgorithm WatershedBasins = new(key: 4);
    public static readonly MeshSegmentationAlgorithm NormalizedCut = new(key: 5);
}

[SmartEnum<int>]
public sealed partial class MeshSegmentationStatus {
    public static readonly MeshSegmentationStatus Completed = new(key: 0);
    public static readonly MeshSegmentationStatus MaxIterationsExhausted = new(key: 1);
}
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:433`

```csharp
public readonly record struct Segmentation(
    MeshSegmentationAlgorithm Algorithm, MeshSegmentationStatus Status, int RequestedRegionCount, int RegionCount, int SeedCount,
    int AssignedFaceCount, int UnassignedFaceCount, int SkippedDegenerateFaces, int SkippedNonFiniteValues, Option<int> Iterations,
    Option<int> MaxIterations, Option<double> Tolerance, Option<double> Threshold, Option<DescriptorSolve> Descriptor, Option<LinearSolution> Solve,
    Option<double> NormalizedCutValue = default, Option<int> AffinityNonZeros = default, Option<int> WatershedSaddleCount = default,
    Option<EigenSolution<double, Arr<double>>> Eigen = default) : IValidityEvidence {
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:460`

```csharp
private readonly record struct SegmentationRun(MeshSegmentationAlgorithm Algorithm, int RequestedRegionCount, int SeedCount, MeshSegmentationStatus Status, Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance, Option<double> Threshold, Option<DescriptorSolve> Descriptor, Option<LinearSolution> Solve = default, Option<double> NormalizedCutValue = default, Option<int> AffinityNonZeros = default, Option<int> WatershedSaddleCount = default, Option<EigenSolution<double, Arr<double>>> Eigen = default);
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:784`

```csharp
private static Arr<Option<RegionLabel>> VertexRegionsOf(Mesh mesh, int[] faceRegions) {
    List<int>[] incident = [.. Enumerable.Range(start: 0, count: mesh.Vertices.Count).Select(static _ => new List<int>())];
    for (int f = 0; f < mesh.Faces.Count; f++) {
        int region = faceRegions[f];
        if (region < 0) continue;
        MeshFace face = mesh.Faces[index: f];
        incident[face.A].Add(item: region); incident[face.B].Add(item: region); incident[face.C].Add(item: region);
        if (face.IsQuad) incident[face.D].Add(item: region);
    }
    return new Arr<Option<RegionLabel>>([.. incident.Select(static regions => regions.Count == 0 ? Option<RegionLabel>.None : RegionLabel.Of(regions.GroupBy(static r => r).OrderByDescending(static g => g.Count()).ThenBy(static g => g.Key).First().Key))]);
}
```

## To

```csharp
// MeshSegmentationAlgorithm DELETED
// MeshSegmentationStatus DELETED
// SegmentationRun DELETED
// VertexRegionsOf DELETED
```

```csharp
public readonly record struct Segmentation(
    MeshSegmentation Request, int RegionCount, Option<int> SeedCount,
    int AssignedFaceCount, int UnassignedFaceCount, int SkippedDegenerateFaces, int SkippedNonFiniteValues,
    Option<int> Iterations, Option<DescriptorSolve> DescriptorSolve = default,
    Option<double> NormalizedCutValue = default, Option<int> AffinityNonZeros = default,
    Option<int> WatershedSaddleCount = default, Option<EigenSolution<double, Arr<double>>> Eigen = default) : IValidityEvidence {
```

## Why

The algorithm roster mirrors every `MeshSegmentation` case, the status roster is a payload-free two-row family, and `SegmentationRun` duplicates nearly the whole public evidence record solely to copy it once. `RequestedRegionCount`, `MaxIterations`, `Tolerance`, and `Threshold` already ride the stored request; `Solve` is never populated. Publishing an unconverged segmentation as success with either a status row or a replacement bool violates `Cell.Converge`'s typed-exhaustion law.

## Change

Store the admitted request and make `SeedCount` optional rather than encoding non-seeded algorithms as zero. Require the `RegionGrowLabels` and `ClusterLabels` transition state to be converged before returning `Fin.Succ`; return the existing invalid-result fault when the budget exhausts. Construct a `Segmentation` draft in each generated `Switch` arm and let `ResultOf` fill derived counts with `with`; localize vertex-majority projection inside that sole result fold. Remove every mirror-row construction, the dead `Solve` column, and `VertexRegionsOf`.

## Delta

Target code-fence LOC: net -37 after arm and result-fold compaction. Module-level members: -8 smart-enum rows, -14 `SegmentationRun` properties, -6 net public evidence properties, and -1 helper, total net -29. Module-level types: -3, +0, net -3.

# 8. Replace private segmentation packets with named tuple state

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:459`

```csharp
private readonly record struct SegmentationScalars(Arr<double> FaceValues, int SkippedDegenerateFaces, int SkippedNonFiniteValues, int FiniteCount, Option<(double Min, double Max)> Band);
private readonly record struct WatershedState(int[] Regions, int SeedCount, int SaddleCount);
private readonly record struct ClusterState(int[] Labels, double[] Centers, int Iterations, bool Converged);
private readonly record struct NormalizedCutSystem(SparseMatrix Laplacian, SparseMatrix Degree, int AffinityNonZeros, double Sigma);
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:622`

```csharp
private readonly record struct GrowState(int[] Regions, int Iterations, bool Converged);
```

## To

```csharp
// SegmentationScalars DELETED
// WatershedState DELETED
// ClusterState DELETED
// NormalizedCutSystem DELETED
// GrowState DELETED
```

## Why

These private records are scratch packets scoped to one algorithm body or return edge. None owns identity, admission, reusable behavior, or a boundary shape; their positional members inflate module surface around values that named tuples already carry.

## Change

Use named tuple return/state shapes at the existing methods and `Cell.Converge` calls. Replace record `with` updates in grow/cluster rounds with tuple literals carrying the updated slots; keep the existing semantic field names at every read.

## Delta

Target code-fence LOC: net -5. Module-level members: -19 positional properties, +0, net -19. Module-level types: -5, +0, net -5.

# 9. Collapse one-call segmentation helpers and the empty cache key

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:371`

```csharp
public Arr<double> Values => Switch(perVertexCase: static row => row.Values, perFaceCase: static row => row.Values);
internal int Expected(Mesh mesh) => Switch(state: mesh, perVertexCase: static (m, _) => m.Vertices.Count, perFaceCase: static (m, _) => m.Faces.Count);
internal double FaceValue(MeshFace face, int index) => Switch(
    state: (Face: face, Index: index),
    perVertexCase: static (s, row) => ((row.Values[index: s.Face.A] + row.Values[index: s.Face.B] + row.Values[index: s.Face.C])
        + (s.Face.IsQuad ? row.Values[index: s.Face.D] : 0.0)) / (s.Face.IsQuad ? 4.0 : 3.0),
    perFaceCase: static (s, row) => row.Values[index: s.Index]);
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:458`

```csharp
[StructLayout(LayoutKind.Auto)] private readonly record struct FaceAdjacencyKey();
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:518`

```csharp
private static Fin<ArrayUndirectedGraph<int, SEdge<int>>> FaceAdjacency(MeshSpace space, Op key) =>
    space.Cache.Memoized(probe: new FaceAdjacencyKey(), compute: () => Fin.Succ(FaceAdjacencyOf(mesh: space.Native)));
private static ArrayUndirectedGraph<int, SEdge<int>> FaceAdjacencyOf(Mesh mesh) {
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:535`

```csharp
private static Fin<SegmentationScalars> SegmentationScalarsOf(Mesh mesh, MeshScalars scalars, Op key) =>
    scalars.Values.Count == scalars.Expected(mesh: mesh)
        ? Fin.Succ(FaceScalarsOf(mesh: mesh, scalars: scalars))
        : Fin.Fail<SegmentationScalars>(key.InvalidInput());
private static SegmentationScalars FaceScalarsOf(Mesh mesh, MeshScalars scalars) {
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:559`

```csharp
private static int BandIndexOf(double value, (double Min, double Max) band, int count) =>
    !double.IsFinite(x: value) ? UnassignedRegion : Math.Abs(value: band.Max - band.Min) <= EpsilonPolicy.SqrtEpsilon ? 0 : Math.Min(val1: count - 1, val2: Math.Max(val1: 0, val2: (int)Math.Floor(d: (value - band.Min) / ((band.Max - band.Min) / count))));
```

## To

```csharp
// FaceAdjacencyKey DELETED
// FaceAdjacencyOf DELETED
// MeshScalars.Values DELETED
// MeshScalars.Expected DELETED
// MeshScalars.FaceValue DELETED
// FaceScalarsOf DELETED
// BandIndexOf DELETED
```

```csharp
private static Fin<ArrayUndirectedGraph<int, SEdge<int>>> FaceAdjacency(MeshSpace space, Op key) =>
    space.Cache.Memoized(probe: unit, compute: () => {
        Mesh mesh = space.Native;
```

```csharp
private static Fin<(Arr<double> FaceValues, int SkippedDegenerateFaces, int SkippedNonFiniteValues, int FiniteCount, Option<(double Min, double Max)> Band)>
    SegmentationScalarsOf(Mesh mesh, MeshScalars scalars, Op key) {
    Fin<(Arr<double>, int, int, int, Option<(double, double)>)> Build(Arr<double> values, Func<MeshFace, int, double> read) {
```

## Why

`Memoized` keys slots by `(TKey,TValue)`, and no other `Unit`/face-graph slot exists, so an empty marker type adds no identity. `FaceAdjacencyOf` and `FaceScalarsOf` each have one caller. `MeshScalars.Values`, `Expected`, and `FaceValue` are forwarding dispatch members that force repeated union folds before the one scalar derivation.

## Change

Use `unit` as the memo probe and inline adjacency construction into its compute callback. Fold `MeshScalars` once in `SegmentationScalarsOf`, passing each case's value array and face projection to a local `Build`; delete `Values`, `Expected`, `FaceValue`, and `FaceScalarsOf`. Inline `BandIndexOf` into the scalar-band arm.

## Delta

Target code-fence LOC: net -17. Module-level members: -6 methods/properties, +0, net -6; one local function is added. Module-level types: -1, +0, net -1.

# 10. Unify n-RoSy order at the direction-field owner

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:807`

```csharp
[SmartEnum<int>]
public sealed partial class RosySymmetry {
    public static readonly RosySymmetry Line = new(key: 1);
    public static readonly RosySymmetry Cross2 = new(key: 2);
    public static readonly RosySymmetry Cross4 = new(key: 4);
    public static readonly RosySymmetry Hex6 = new(key: 6);
    public double Phase => Key;
}
```

## To

```csharp
[SmartEnum<int>]
public sealed partial class RosyOrder {
    public static readonly RosyOrder Vector = new(key: 1);
    public static readonly RosyOrder Line = new(key: 2);
    public static readonly RosyOrder Cross = new(key: 4);
    public static readonly RosyOrder Hex = new(key: 6);
}
```

## Why

The direction-field kernel admits the complete `{1,2,4,6}` n-RoSy domain while `Processing/remesh.md` re-lists only `{2,4,6}`. The narrower remesh roster is therefore the duplicate. `RosySymmetry` still coins number-suffixed rows, mislabels order one as a line field, and adds a forwarding property over generated `Key`.

## Change

Rename the broader owner to `RosyOrder`, use the geometric terms `Vector`, `Line`, `Cross`, and `Hex`, and delete `Phase`. Change `CrossFieldAt` and every internal field method to carry the admitted row directly; read `order.Key` only in numeric phase arithmetic and the Cholesky cache coordinate. Delete the narrower `RoSyOrder` declaration rather than moving the full field vocabulary into a remeshing consumer.

## Delta

Target code-fence LOC: 8 to 7, net -1; ripple code-fence LOC: net -5; project net -6. Module-level members: -1 target forwarding property and -3 duplicate remesh rows, +0, net -4. Module-level types: -1 duplicate remesh type, +0, net -1.

## Ripples

Delete `RoSyOrder` from `libs/dotnet/Rasm/.planning/Processing/remesh.md` and use `RosyOrder` in its `QuadField`. Update `libs/dotnet/Rasm/.planning/Spatial/fields.md` so the `VectorField.CrossField` payload and factory carry `RosyOrder`, removing the integer projection at sampling. Update `libs/dotnet/Rasm/.planning/Parametric/panelize.md` to carry `RosyOrder` and pass it directly to `CrossFieldAt`; ripple the same rename through prose and diagrams on those three pages.

# 11. Inline cross-field forwarding stages at the memo fill

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:827`

```csharp
internal static Fin<Vector3d> CrossFieldAt(MeshSpace space, int symmetry, Option<Seq<(int Vertex, Direction Hint)>> constraints, Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, Point3d sample, Op key) =>
    from rosy in key.AcceptValidated<RosySymmetry>(candidate: symmetry)
    from cached in space.Cache.Memoized(probe: CrossFieldKey.Of(symmetry: rosy, constraints: constraints, cones: cones),
        compute: () => ComputeCrossField(space: space, symmetry: rosy, constraints: constraints, cones: cones, key: key))
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:834`

```csharp
private static Fin<Complex[]> ComputeCrossField(MeshSpace space, RosySymmetry symmetry, Option<Seq<(int Vertex, Direction Hint)>> constraints, Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, Op key) =>
    ResolveEdgeAdjustment(space: space, cones: cones, key: key).Bind(adjustment =>
        constraints.IsSome
            ? SolveConstrainedCrossField(space: space, symmetry: symmetry, hints: constraints.IfNone(toSeq<(int, Direction)>([])), edgeAdjustment: adjustment, key: key)
            : SolveSmoothestCrossField(space: space, symmetry: symmetry, edgeAdjustment: adjustment, key: key));
private static Fin<Option<Arr<double>>> ResolveEdgeAdjustment(MeshSpace space, Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, Op key) =>
```

## To

```csharp
internal static Fin<Vector3d> CrossFieldAt(MeshSpace space, RosyOrder order, Option<Seq<(int Vertex, Direction Hint)>> constraints, Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, Point3d sample, Op key) =>
    from cached in space.Cache.Memoized(probe: CrossFieldKey.Of(order, constraints, cones), compute: () =>
        from adjustment in cones.TraverseM(values =>
            from mesh in space.Cache.IntrinsicMeshSnapshot(key)
            from result in DecAssembly.DistributeHolonomy(space, mesh,
                values.Map(c => (c.Vertex, ConeIndex: c.HolonomyDeficit / (2.0 * Math.PI))), key)
            select result).As()
        from field in constraints.Match(
            Some: hints => SolveConstrainedCrossField(space, order, hints, adjustment, key),
            None: () => SolveSmoothestCrossField(space, order, adjustment, key))
        select field)

// ComputeCrossField DELETED
// ResolveEdgeAdjustment DELETED
```

## Why

`ComputeCrossField` only forwards the memo callback and `ResolveEdgeAdjustment` only unwraps one optional effect. Their `IsSome`/`IfNone(empty)` path fabricates an empty value after separately testing presence instead of traversing the option.

## Change

Inline both stages into the memoized computation. Use `Option.TraverseM(...).As()` for the optional holonomy effect and `Match` for constrained versus smooth solve selection.

## Delta

Target code-fence LOC: net -2. Module-level members: -2 methods, +0, net -2. Module-level types: unchanged.

# 12. Localize single-call direction-field transforms

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:862`

```csharp
private static List<(int Row, int Col, Complex Value)> AssembleHermitianTriplets(Seq<(int I, int J, double Weight, double Rho)> entries, RosySymmetry symmetry) {
    List<(int, int, Complex)> triplets = new(capacity: entries.Count * 3);
    for (int e = 0; e < entries.Count; e++) {
        (int i, int j, double w, double rho) = entries[index: e];
        triplets.Add(item: (i, i, new Complex(real: w, imaginary: 0.0)));
        triplets.Add(item: (j, j, new Complex(real: w, imaginary: 0.0)));
        triplets.Add(item: (i, j, -w * Complex.FromPolarCoordinates(magnitude: 1.0, phase: symmetry.Phase * rho)));
    }
    return triplets;
}
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:898`

```csharp
private static Arr<double> StackMassWeighted(int n, Complex[] qHat, Arr<double> mass) {
    double[] rhs = new double[2 * n];
    for (int v = 0; v < n; v++) { Complex value = mass[index: v] * qHat[v]; rhs[v] += value.Real; rhs[v + n] += value.Imaginary; }
    return new Arr<double>(rhs);
}
private static Arr<Complex> ReassembleComplex(int n, Arr<double> real) {
    Complex[] result = new Complex[n];
    for (int v = 0; v < n; v++) result[v] = new Complex(real: real[index: v], imaginary: real[index: v + n]);
    return new Arr<Complex>(result);
}
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:918`

```csharp
private static Vector3d DecodeRosy(Complex value, Vector3d xAxis, Vector3d yAxis, RosySymmetry symmetry) {
    double angle = Math.Atan2(y: value.Imaginary, x: value.Real) / symmetry.Phase;
    Vector3d result = (Math.Cos(d: angle) * xAxis) + (Math.Sin(a: angle) * yAxis);
    _ = result.Unitize();
    return result;
}
```

## To

```csharp
// AssembleHermitianTriplets DELETED
// StackMassWeighted DELETED
// ReassembleComplex DELETED
// DecodeRosy DELETED
```

## Why

Each transform has exactly one caller and no independent admission, policy, reuse, or boundary meaning. Their names split the connection assembly, constrained right-hand side, solution reassembly, and sample decode across module members without reducing duplication.

## Change

Move the triplet loop into `BuildConnectionLaplacian`; use `order.Key` directly in its polar phase. Move the RHS stack and complex reassembly loops into `SolveConstrainedCrossField` around the one Cholesky call. Put the representative-angle decode directly in the `MeshProbe.ComplexBlend` callback. Keep `NormalizePhases`, which has two callers.

## Delta

Target code-fence LOC: net -4. Module-level members: -4 methods, +0, net -4; operation-local statements move unchanged into their sole callers. Module-level types: unchanged.

# 13. Remove request echoes from remesh capture

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:995`

```csharp
public readonly record struct RemeshCapture(
    RemeshKind Kind, int PreVertexCount, int PreFaceCount, int PostVertexCount, int PostFaceCount,
    Option<double> ReductionRatio, Option<double> TargetLength = default, Option<int> TargetQuadCount = default,
    Option<double> AdaptiveSize = default, Option<bool> AdaptiveQuadCount = default, Option<bool> HardEdgePreservationRequested = default,
    Option<QuadGuideInfluence> GuideInfluence = default, Option<QuadPreserveEdges> PreserveEdges = default, Option<QuadRemeshSymmetryAxis> SymmetryAxis = default,
    int GuideCurveCount = 0, int FaceBlockCount = 0, Option<int> DesiredPolygonCount = default, Option<bool> AllowDistortion = default,
    Option<int> Accuracy = default, Option<bool> NormalizeMeshSize = default, int FaceTagCount = 0, int LockedComponentCount = 0) : IValidityEvidence {
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:1074`

```csharp
private static RemeshCapture QuadCaptureOf(RemeshKind.QuadCase quad, QuadRemeshParameters parameters, Mesh source, Mesh output) =>
    TopologyOf(kind: quad, source: source, output: output) with {
        TargetLength = quad.Target is QuadTarget.EdgeLengthCase edge ? Some(edge.Length.Value) : Option<double>.None,
        TargetQuadCount = quad.Target is QuadTarget.QuadCountCase ? Some(parameters.TargetQuadCount) : Option<int>.None,
        AdaptiveSize = Some(parameters.AdaptiveSize), AdaptiveQuadCount = Some(parameters.AdaptiveQuadCount),
        HardEdgePreservationRequested = Some(quad.DetectHardEdges), GuideInfluence = Some(quad.GuideInfluence),
        PreserveEdges = Some(quad.PreserveEdges), SymmetryAxis = Some(quad.SymmetryAxis),
        GuideCurveCount = quad.GuideCurves.Count, FaceBlockCount = quad.FaceBlocks.Count,
    };
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:1083`

```csharp
private static RemeshCapture ReduceCaptureOf(RemeshKind.SimplifyCase kind, Mesh source, Mesh output) =>
    TopologyOf(kind: kind, source: source, output: output) with {
        DesiredPolygonCount = Some(kind.Parameters.DesiredPolygonCount), AllowDistortion = Some(kind.Parameters.AllowDistortion),
        Accuracy = Some(kind.Parameters.Accuracy), NormalizeMeshSize = Some(kind.Parameters.NormalizeMeshSize),
        FaceTagCount = kind.Parameters.FaceTags?.Length ?? 0, LockedComponentCount = kind.Parameters.LockedComponents?.Length ?? 0,
    };
```

## To

```csharp
public readonly record struct RemeshCapture(
    RemeshKind Request, int PreVertexCount, int PreFaceCount, int PostVertexCount, int PostFaceCount,
    Option<int> DesiredPolygonCount = default, Option<bool> AllowDistortion = default,
    Option<int> Accuracy = default, Option<bool> NormalizeMeshSize = default,
    int FaceTagCount = 0, int LockedComponentCount = 0) : IValidityEvidence {
    public Option<double> FaceRatio =>
        PreFaceCount == 0 ? None : Some((double)PostFaceCount / PreFaceCount);
```

```csharp
// QuadCaptureOf DELETED
// ReduceCaptureOf DELETED
```

## Why

`RemeshCapture.Kind` already stores the full immutable `QuadCase`; ten columns copy its target, policies, flags, and collection counts. `ReductionRatio` is derived entirely from two stored counts, and the stored formula is a remaining-face ratio rather than a reduction amount. Both capture helpers have one call and only apply `with` values around `TopologyOf`. Simplify columns remain because `ReduceMeshParameters` is a mutable foreign carrier whose admitted snapshot cannot be recovered later.

## Change

Rename `Kind` to `Request`, delete the quad-duplicate columns and their validity clauses, and expose the derived value as `FaceRatio`. Construct both captures directly in their `RemeshKind.Switch` arms through `TopologyOf`; keep only the simplify snapshot columns until that foreign request is replaced by an owned immutable shape.

## Delta

Target code-fence LOC: net -14. Module-level members: -11 positional properties and -2 helper methods, +1 computed property, net -12. Module-level types: unchanged.

# 14. Inline quad parameter translation through generated dispatch

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:1062`

```csharp
private static QuadRemeshParameters QuadParametersOf(RemeshKind.QuadCase quad) {
    QuadRemeshParameters parameters = new() { DetectHardEdges = quad.DetectHardEdges, GuideCurveInfluence = quad.GuideInfluence.Key, PreserveMeshArrayEdgesMode = quad.PreserveEdges.Key, SymmetryAxis = quad.SymmetryAxis };
    switch (quad.Target) {
        case QuadTarget.EdgeLengthCase edge: parameters.TargetEdgeLength = edge.Length.Value; break;
        case QuadTarget.QuadCountCase count:
            parameters.TargetQuadCount = count.Count.Value;
            parameters.AdaptiveSize = count.AdaptiveSize.Value * NativeAdaptiveScale;
            parameters.AdaptiveQuadCount = count.AdaptiveQuadCount;
            break;
    }
    return parameters;
}
```

## To

```csharp
// QuadParametersOf DELETED
```

```csharp
QuadRemeshParameters parameters = new() {
    DetectHardEdges = quad.DetectHardEdges, GuideCurveInfluence = quad.GuideInfluence.Key,
    PreserveMeshArrayEdgesMode = quad.PreserveEdges.Key, SymmetryAxis = quad.SymmetryAxis,
};
quad.Target.Switch(state: parameters,
    edgeLengthCase: static (target, edge) => target.TargetEdgeLength = edge.Length.Value,
    quadCountCase: static (target, count) =>
        (target.TargetQuadCount, target.AdaptiveSize, target.AdaptiveQuadCount) =
        (count.Count.Value, count.AdaptiveSize.Value * NativeAdaptiveScale, count.AdaptiveQuadCount));
```

## Why

`QuadParametersOf` has one caller and hand-switches a Thinktecture union. Keeping the translation out of the only native call adds a hop and forfeits generated exhaustive dispatch.

## Change

Inline parameter construction into the `quadCase` boundary arm and use state-threaded `QuadTarget.Switch`; preserve the native `[0,100]` adaptive conversion at that edge.

## Delta

Target code-fence LOC: net -2. Module-level members: -1 method, +0, net -1. Module-level types: unchanged. Hand-authored union dispatch sites: 1 to 0.

# 15. Make flattening one batch surface with derived UV projection

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:1022`

```csharp
public readonly record struct FlattenCapture(int VertexCount, int UvCount, int TextureCoordinateCount, int BoundaryComponents, MeshUnwrapMethod Method, Option<Plane> SymmetryPlane, Option<double> EdgeLengthDistortionRms) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        VertexCount >= 0 && UvCount >= 0 && TextureCoordinateCount >= 0 && BoundaryComponents >= 0,
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:1030`

```csharp
public readonly record struct FlattenResult(Arr<Point2d> Uvs, Mesh Mesh, FlattenCapture Capture) {
    internal Fin<TOut> Project<TOut>(Op key) {
        FlattenResult self = this;
        return ResultProjection.Rows<FlattenResult, TOut>(self: self, key: key,
            ProjectionRow.Of<Arr<Point2d>>(() => Fin.Succ(self.Uvs)),
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:1094`

```csharp
internal static Fin<FlattenResult> ParameterizeFlattenDetailed(MeshSpace space, Op key, Option<MeshUnwrapMethod> method = default, Option<Plane> symmetryPlane = default) => key.Catch(() => {
    MeshUnwrapMethod unwrapMethod = method.IfNone(MeshUnwrapMethod.LSCM);
    if (symmetryPlane.Exists(static plane => !plane.IsValid))
        return Fin.Fail<FlattenResult>(error: key.InvalidInput());
    using Mesh mesh = space.Native.DuplicateMesh();
    using MeshUnwrapper unwrapper = new(mesh);
    symmetryPlane.IfSome(plane => unwrapper.SymmetryPlane = plane);
    if (!unwrapper.Unwrap(method: unwrapMethod) || mesh.TextureCoordinates.Count != mesh.Vertices.Count)
        return Fin.Fail<FlattenResult>(error: key.InvalidResult());
    return ResultOf(mesh: mesh, unwrapMethod: unwrapMethod, symmetryPlane: symmetryPlane, key: key);
});
```

## To

```csharp
public readonly record struct FlattenCapture(
    int VertexCount, int BoundaryComponents, MeshUnwrapMethod Method,
    Option<Plane> SymmetryPlane, Option<double> EdgeLengthDistortionRms) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(VertexCount >= 0 && BoundaryComponents >= 0,
```

```csharp
public readonly record struct FlattenResult(Mesh Mesh, FlattenCapture Capture) {
    internal Fin<TOut> Project<TOut>(Op key) {
        FlattenResult self = this;
        return ResultProjection.Rows<FlattenResult, TOut>(self: self, key: key,
            ProjectionRow.Of<Arr<Point2d>>(() => Fin.Succ(new Arr<Point2d>(
                [.. self.Mesh.TextureCoordinates.Select(static uv => new Point2d(uv.X, uv.Y))]))),
```

```csharp
// ParameterizeFlattenDetailed(MeshSpace, Op, Option<MeshUnwrapMethod>, Option<Plane>) DELETED
```

## Why

Successful unwrap already proves texture-coordinate/vertex parity, and the UV array is a second copy of `Mesh.TextureCoordinates`. The single-item overload duplicates admission, native call, disposal, and result construction while a one-element batch preserves the exact host capability.

## Change

Keep only the `Seq<MeshSpace>` unwrap surface. Store the output mesh once, project `Arr<Point2d>` from its texture coordinates only when requested, and retain one vertex-count authority in the capture.

## Delta

Target code-fence LOC: net -16. Module-level members: -2 capture properties, -1 result property, and -1 overload, +0, net -4. Module-level types: unchanged.

## Ripples

Update `libs/dotnet/Rasm/.planning/Processing/intent.md` `flattenHostCase` to call the batch surface with `Seq(intent.Space)` and bind `results.Head.ToFin(state.Key.InvalidResult())` before projection. Remove prose that lists `UvCount` or `TextureCoordinateCount`; no other fence reads either property.

# 16. Localize the UV distortion fold

## From

`libs/dotnet/Rasm/.planning/Processing/segment.md:1121`

```csharp
[StructLayout(LayoutKind.Auto)]
private readonly record struct UvDistortionAccumulator(double Numerator, double Denominator, double SumRatio, double SumRatioSquared, int Comparable) {
    internal static readonly UvDistortionAccumulator Empty = new(Numerator: 0.0, Denominator: 0.0, SumRatio: 0.0, SumRatioSquared: 0.0, Comparable: 0);
    internal UvDistortionAccumulator Plus(double modelLength, double uvLength) =>
        (uvLength / modelLength) switch {
            double ratio => this with {
                Numerator = Numerator + (modelLength * uvLength), Denominator = Denominator + (uvLength * uvLength),
                SumRatio = SumRatio + ratio, SumRatioSquared = SumRatioSquared + (ratio * ratio), Comparable = Comparable + 1,
            },
        };
    internal Option<double> Rms =>
        Denominator > EpsilonPolicy.SqrtEpsilon && Comparable > 0 && Numerator / Denominator is double scale
        && double.IsFinite(x: scale) && scale > EpsilonPolicy.SqrtEpsilon
        && Math.Sqrt(d: Math.Max(val1: 0.0, val2: ((scale * scale * SumRatioSquared) - (2.0 * scale * SumRatio) + Comparable) / Comparable)) is double rms
        && double.IsFinite(x: rms)
            ? Some(rms)
            : Option<double>.None;
}
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:1140`

```csharp
private static Fin<FlattenResult> ResultOf(Mesh mesh, MeshUnwrapMethod unwrapMethod, Option<Plane> symmetryPlane, Op key) {
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:1157`

```csharp
private static Option<double> UvEdgeLength(Mesh mesh, Arr<Point2d> uvs, int faceIndex, IndexPair pair) {
```

`libs/dotnet/Rasm/.planning/Processing/segment.md:1174`

```csharp
private static int FaceVertexAt(MeshFace face, int corner) => corner switch { 0 => face.A, 1 => face.B, 2 => face.C, _ => face.D };
```

## To

```csharp
// UvDistortionAccumulator DELETED
// ResultOf(Mesh, MeshUnwrapMethod, Option<Plane>, Op) DELETED
// UvEdgeLength DELETED
// FaceVertexAt DELETED
```

## Why

After the singular unwrap overload is removed, `ResultOf` has one call. Its accumulator and UV-edge helper exist only inside that body, while `MeshFace.this[int]` already owns corner indexing. Keeping all four module constructs turns one local quality fold into a parallel surface.

## Change

Inline result construction into the batch `TraverseM` callback. Fold the named tuple `(Numerator, Denominator, SumRatio, SumRatioSquared, Comparable)`, compute scale and RMS once after the fold, and use a local UV-edge function reading `part.TextureCoordinates` with `face[corner]`/`face[next]`. Duplicate the validated part only for the returned owned mesh, preserving disposal of every temporary.

## Delta

Target code-fence LOC: net -11. Module-level members: -8 accumulator members and -3 methods, +0, net -11; one local function is added. Module-level types: -1, +0, net -1.

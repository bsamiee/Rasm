using System.Numerics;
using Foundation.CSharp.Analyzers.Contracts;
using Rhino.Collections;

namespace Rasm.Vectors;

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SparseLaplacian(SparseMatrix Stiffness, SparseMatrix MassConsistent, Arr<double> MassLumped, int SkippedDegenerateFaces = 0) {
    public bool IsValid => Stiffness.IsValid && MassConsistent.IsValid && Stiffness.Rows.Value == MassConsistent.Rows.Value && Stiffness.Cols.Value == MassConsistent.Cols.Value && Stiffness.Rows.Value == Stiffness.Cols.Value && MassLumped.Count == Stiffness.Rows.Value && MassLumped.All(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0);
}
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshSpace {
    private MeshSpace(Mesh native, Context tolerance) { Native = native; Tolerance = tolerance; }
    internal Mesh Native { get; }
    public Context Tolerance { get; }
    public Mesh DuplicateNative() => Native.DuplicateMesh();
    public static Fin<MeshSpace> Of(Mesh native, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(native).ToFin(op.InvalidInput())
               from ctx in Optional(context).ToFin(op.MissingContext())
               from _ in guard(active.IsValid, op.InvalidInput())
               let snapshot = active.DuplicateMesh()
               select new MeshSpace(native: snapshot, tolerance: ctx);
    }
    internal LaplacianCache Cache => LaplacianCache.For(space: this);
    public Fin<SparseLaplacian> Laplacian(MeshLaplacian kind, Op? key = null) =>
        MeshKernel.LaplacianOf(space: this, kind: kind, key: key.OrDefault());
}
internal readonly record struct GeodesicKey(Seq<int> Sources);
[StructLayout(LayoutKind.Auto)] internal readonly record struct McfKey(double TimeStep, int Iterations);
[StructLayout(LayoutKind.Auto)] internal readonly record struct CrossFieldKey(int Symmetry, Option<Seq<(int Vertex, Direction Hint)>> Constraints, Option<Seq<(int Vertex, double HolonomyDeficit)>> Cones);
internal readonly record struct HodgeKey(VectorField Source);
[StructLayout(LayoutKind.Auto)] internal readonly record struct VectorHeatKey(double Time, Seq<(int Vertex, Vector3d Direction)> Sources);
internal sealed class LaplacianCache {
    internal const int DefaultSpectralCount = 32;
    private const double SpdRegularization = 1e-8;
    private static readonly ConditionalWeakTable<object, LaplacianCache> Table = [];
    private sealed class Memo<TKey, T> {
        private readonly Atom<HashMap<TKey, Fin<T>>> cache = Atom(value: HashMap<TKey, Fin<T>>());
        internal Fin<T> Of(TKey probe, Func<Fin<T>> compute) =>
            cache.Value.Find(key: probe).IfNone(() => {
                Fin<T> computed = compute();
                _ = cache.Swap(f: map => map.AddOrUpdate(key: probe, value: computed));
                return computed;
            });
    }
    private readonly Lazy<Fin<SparseLaplacian>> cotangent, intrinsicDelaunay, robust;
    private readonly Lazy<Fin<CholeskySparse>> cholesky;
    private readonly Lazy<Fin<SpectralBasisBundle>> defaultSpectral;
    private readonly Lazy<double> meanEdgeLength;
    private readonly Lazy<Fin<MeshKernel.IntrinsicMesh>> intrinsicMesh;
    private readonly Lazy<Fin<(Arr<double> Values, SolveReceipt Solve)>> signedHeat;
    private readonly Memo<(int Symmetry, double Time), CholeskySparse> connectionCholesky = new();
    private readonly Memo<double, CholeskySparse> scalarHeatCholesky = new(), edgeConnectionCholesky = new();
    private readonly Memo<GeodesicKey, Arr<double>> geodesicCache = new();
    private readonly Memo<McfKey, Arr<double>> mcfCache = new();
    private readonly Memo<CrossFieldKey, Complex[]> crossFieldCache = new();
    private readonly Memo<HodgeKey, MeshKernel.HodgeBundle> hodgeCache = new();
    private readonly Memo<VectorHeatKey, Complex[]> vectorHeatCache = new();
    private readonly MeshSpace space;
    private LaplacianCache(MeshSpace space) {
        this.space = space;
        Op fallback = Op.Of();
        intrinsicMesh = new Lazy<Fin<MeshKernel.IntrinsicMesh>>(valueFactory: () => MeshKernel.BuildIntrinsicMesh(mesh: space.Native, key: fallback));
        cotangent = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleCotangent(mesh: space.Native, key: fallback));
        intrinsicDelaunay = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => intrinsicMesh.Value.Bind(im => MeshKernel.AssembleCotangentFromIntrinsic(imesh: im, key: fallback)));
        robust = new Lazy<Fin<SparseLaplacian>>(valueFactory: () => MeshKernel.AssembleRobust(mesh: space.Native, key: fallback));
        cholesky = new Lazy<Fin<CholeskySparse>>(valueFactory: () =>
            from L in intrinsicDelaunay.Value
            from spd in MeshKernel.AssembleMassStiffnessSystem(laplacian: L, massScale: SpdRegularization, stiffnessScale: 1.0, key: fallback)
            from factor in CholeskySparse.Of(symmetric: spd, key: fallback)
            select factor);
        defaultSpectral = new Lazy<Fin<SpectralBasisBundle>>(valueFactory: () => MeshKernel.ComputeSpectralBasisDetailed(space: space, k: DefaultSpectralCount, key: fallback));
        meanEdgeLength = new Lazy<double>(valueFactory: () => MeshKernel.MeanEdgeLengthOf(mesh: space.Native));
        signedHeat = new Lazy<Fin<(Arr<double> Values, SolveReceipt Solve)>>(valueFactory: () => MeshKernel.ComputeSignedHeatDetailed(space: space, key: fallback));
    }
    internal static LaplacianCache For(MeshSpace space) =>
        Table.GetValue(key: space.Native, createValueCallback: _ => new LaplacianCache(space: space));
    internal Fin<SparseLaplacian> Cotangent => cotangent.Value;
    internal Fin<SparseLaplacian> IntrinsicDelaunay => intrinsicDelaunay.Value;
    internal Fin<SparseLaplacian> Robust => robust.Value;
    internal Fin<CholeskySparse> Cholesky => cholesky.Value;
    internal Fin<MeshKernel.IntrinsicMesh> IntrinsicMeshSnapshot => intrinsicMesh.Value;
    internal Fin<Arr<double>> SignedHeat => signedHeat.Value.Map(static result => result.Values);
    internal Fin<(Arr<double> Values, SolveReceipt Solve)> SignedHeatDetailed => signedHeat.Value;
    internal double MeanEdgeLength => meanEdgeLength.Value;
    internal Fin<SpectralBasisBundle> SpectralBasisBundleOf(int k, Op key) {
        bool cacheHit = k <= DefaultSpectralCount && defaultSpectral.IsValueCreated;
        return k <= DefaultSpectralCount
            ? defaultSpectral.Value.Map(bundle => bundle with { Basis = bundle.Basis.Truncate(k: k), CacheHit = cacheHit })
            : MeshKernel.ComputeSpectralBasisDetailed(space: space, k: k, key: key);
    }
    internal Fin<CholeskySparse> ConnectionCholesky(int symmetry, double time, Option<Arr<double>> edgeAdjustment, Op key) =>
        edgeAdjustment.IsSome
            ? from real in MeshKernel.BuildConnectionLaplacianRealSystem(space: space, symmetry: symmetry, time: time, edgeAdjustment: edgeAdjustment, key: key)
              from factor in CholeskySparse.Of(symmetric: real, key: key)
              select factor
            : connectionCholesky.Of(probe: (symmetry, time), compute: () =>
                from real in MeshKernel.BuildConnectionLaplacianRealSystem(space: space, symmetry: symmetry, time: time, edgeAdjustment: Option<Arr<double>>.None, key: key)
                from factor in CholeskySparse.Of(symmetric: real, key: key)
                select factor);
    internal Fin<CholeskySparse> ScalarHeatCholesky(double time, Op key) =>
        scalarHeatCholesky.Of(probe: time, compute: () =>
            from L in intrinsicDelaunay.Value
            from system in MeshKernel.AssembleMassStiffnessSystem(laplacian: L, stiffnessScale: time, key: key)
            from factor in CholeskySparse.Of(symmetric: system, key: key)
            select factor);
    internal Fin<CholeskySparse> EdgeConnectionCholesky(double time, Op key) =>
        edgeConnectionCholesky.Of(probe: time, compute: () =>
            from imesh in intrinsicMesh.Value
            from system in SpectralCore.BuildCrouzeixRaviartHeatSystem(mesh: imesh, time: time, key: key)
            from factor in CholeskySparse.Of(symmetric: system, key: key)
            select factor);
    internal Fin<Arr<double>> Geodesic(GeodesicKey probe, Func<Fin<Arr<double>>> compute) => geodesicCache.Of(probe, compute);
    internal Fin<Arr<double>> Mcf(McfKey probe, Func<Fin<Arr<double>>> compute) => mcfCache.Of(probe, compute);
    internal Fin<Complex[]> CrossField(CrossFieldKey probe, Func<Fin<Complex[]>> compute) => crossFieldCache.Of(probe, compute);
    internal Fin<MeshKernel.HodgeBundle> Hodge(HodgeKey probe, Func<Fin<MeshKernel.HodgeBundle>> compute) => hodgeCache.Of(probe, compute);
    internal Fin<Complex[]> VectorHeat(VectorHeatKey probe, Func<Fin<Complex[]>> compute) => vectorHeatCache.Of(probe, compute);
}
[SmartEnum<int>] public sealed partial class MeshLaplacian { public static readonly MeshLaplacian Cotangent = new(key: 0, select: static cache => cache.Cotangent), IntrinsicDelaunay = new(key: 1, select: static cache => cache.IntrinsicDelaunay), Robust = new(key: 2, select: static cache => cache.Robust); [UseDelegateFromConstructor] internal partial Fin<SparseLaplacian> Select(LaplacianCache cache); }
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SpectralBasisBundle(SpectralBasis Basis, EigenSolveReceipt<double, Arr<double>> Eigen, bool CacheHit = false, int SkippedDegenerateFaces = 0, Option<int> FactorNonZeros = default);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct TopologyReceipt(int Vertices, int TopologyVertices, int TopologyEdges, int Faces, int Triangles, int Quads, int Ngons, int VisiblePolygons, int BoundaryComponents, int NonManifoldEdges, bool IsManifold, bool IsOriented, int EulerCharacteristic, Option<int> Genus, bool EulerValidated);
[SmartEnum<int>] public sealed partial class MeshFeatureKind { public static readonly MeshFeatureKind Boundary = new(key: 0), Crease = new(key: 1), NonManifold = new(key: 2), Unwelded = new(key: 3), NgonInteriorSkipped = new(key: 4); }
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct FeatureEdge(int A, int B, MeshFeatureKind Kind, Option<double> DihedralRadians);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct FeatureReceipt(Seq<FeatureEdge> Edges, int BoundaryEdges, int CreaseEdges, int NonManifoldEdges, int UnweldedEdges, int NgonInteriorSkippedEdges, double DihedralThresholdRadians);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct FlattenReceipt(int VertexCount, int UvCount, int TextureCoordinateCount, int BoundaryComponents, bool NativeUnwrap, bool Valid, Option<double> EdgeLengthDistortionRms);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct FlattenResult(Arr<Point2d> Uvs, Mesh Mesh, FlattenReceipt Receipt);
[SmartEnum<int>] public sealed partial class RemeshStatus { public static readonly RemeshStatus Completed = new(key: 0), NativeRejected = new(key: 1); }
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct RemeshReceipt(RemeshKind Kind, RemeshStatus Status, Option<double> TargetLength, Option<int> DesiredPolygonCount, int PreVertexCount, int PreFaceCount, int PostVertexCount, int PostFaceCount, double ReductionRatio, bool Valid, bool HardEdgePreservationRequested, bool TopologyChanged);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct RemeshResult(Mesh Mesh, RemeshReceipt Receipt);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct DescriptorReceipt(SpectralDescriptorReceipt Spectral, EigenSolveReceipt<double, Arr<double>> Eigen, int RequestedEigenpairs, int ReturnedEigenpairs, bool ComparisonReady, bool SpectralCacheHit = false, int SkippedDegenerateFaces = 0, Option<int> FactorNonZeros = default);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct DescriptorResult(Arr<double> Values, DescriptorReceipt Receipt);
[SmartEnum<int>] public sealed partial class MeshSegmentationAlgorithm { public static readonly MeshSegmentationAlgorithm ScalarThresholdComponents = new(key: 0), ScalarBandComponents = new(key: 1), SeededRegionGrow = new(key: 2), DescriptorScalarClusters = new(key: 3); }
[SmartEnum<int>] public sealed partial class MeshSegmentationStatus { public static readonly MeshSegmentationStatus Completed = new(key: 0), MaxIterationsExhausted = new(key: 1); }
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshSegmentationReceipt(MeshSegmentationAlgorithm Algorithm, MeshSegmentationStatus Status, int RequestedRegionCount, int RegionCount, int SeedCount, int AssignedFaceCount, int UnassignedFaceCount, int SkippedDegenerateFaces, int SkippedNonFiniteValues, Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance, Option<double> Threshold, Option<DescriptorReceipt> Descriptor, Option<SolveReceipt> Solve, Option<bool> SpectralCacheHit, Option<bool> FactorCacheHit, Option<int> FactorNonZeros);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshSegmentationResult(Arr<int> FaceRegions, Arr<int> VertexRegions, MeshSegmentationReceipt Receipt);
[SmartEnum<int>] public sealed partial class SdfMeshStatus { public static readonly SdfMeshStatus Approximate = new(key: 0), BoundarySourceSignedHeat = new(key: 1), Unsupported = new(key: 2); }
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SdfMeshReceipt(SdfMeshMethod Method, SdfMeshStatus Status, bool IsSolid, bool IsManifold, bool IsOriented, int BoundaryComponents, int NonManifoldEdges, bool UsesGeneralizedWindingApproximation, bool UsesBoundarySignedHeat, Option<SolveReceipt> Solve);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SdfMeshSample(double Distance, SdfMeshReceipt Receipt);
[Union]
public abstract partial record MeshDescriptor {
    private MeshDescriptor() { }
    public sealed record SpectralCase(SpectralFilter Filter, Option<Seq<int>> Sources) : MeshDescriptor;
    internal bool IsValid => this is SpectralCase { Filter: not null };
    public static MeshDescriptor Spectral(SpectralFilter filter, Option<Seq<int>> sources = default) => new SpectralCase(Filter: filter, Sources: sources);
}

[Union]
public abstract partial record MeshSegmentation {
    private MeshSegmentation() { }
    public sealed record ScalarThresholdCase(Arr<double> Values, double Threshold, bool IncludeAbove, bool ValuesAreVertices) : MeshSegmentation;
    public sealed record ScalarBandsCase(Arr<double> Values, Dimension BandCount, bool ValuesAreVertices) : MeshSegmentation;
    public sealed record SeededRegionGrowCase(Arr<double> Values, Seq<int> SeedFaces, PositiveMagnitude Tolerance, Dimension MaxIterations, bool ValuesAreVertices) : MeshSegmentation;
    public sealed record DescriptorClustersCase(MeshDescriptor Descriptor, Dimension Eigenpairs, Dimension RegionCount, Dimension MaxIterations, PositiveMagnitude Tolerance) : MeshSegmentation;
    public static Fin<MeshSegmentation> ScalarThreshold(Arr<double> values, double threshold, bool includeAbove = true, bool valuesAreVertices = false, Op? key = null) =>
        AdmitScalars(values: values, key: key.OrDefault()).Bind(admitted =>
            RhinoMath.IsValidDouble(x: threshold)
                ? Fin.Succ<MeshSegmentation>(new ScalarThresholdCase(Values: admitted, Threshold: threshold, IncludeAbove: includeAbove, ValuesAreVertices: valuesAreVertices))
                : Fin.Fail<MeshSegmentation>(key.OrDefault().InvalidInput()));
    public static Fin<MeshSegmentation> ScalarBands(Arr<double> values, int bandCount, bool valuesAreVertices = false, Op? key = null) =>
        key.OrDefault() switch { Op op => from admitted in AdmitScalars(values: values, key: op) from count in op.AcceptValidated<Dimension>(candidate: bandCount) from _ in bandCount > 1 ? Fin.Succ(unit) : Fin.Fail<Unit>(op.InvalidInput()) select (MeshSegmentation)new ScalarBandsCase(Values: admitted, BandCount: count, ValuesAreVertices: valuesAreVertices) };
    public static Fin<MeshSegmentation> SeededRegionGrow(Arr<double> values, Seq<int> seedFaces, double tolerance, int maxIterations, bool valuesAreVertices = false, Op? key = null) =>
        key.OrDefault() switch { Op op => from admitted in AdmitScalars(values: values, key: op) from seeds in Optional(seedFaces).ToFin(op.InvalidInput()) from _ in seeds.IsEmpty ? Fin.Fail<Unit>(op.InvalidInput()) : Fin.Succ(unit) from eps in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance) from cap in op.AcceptValidated<Dimension>(candidate: maxIterations) select (MeshSegmentation)new SeededRegionGrowCase(Values: admitted, SeedFaces: seeds, Tolerance: eps, MaxIterations: cap, ValuesAreVertices: valuesAreVertices) };
    public static Fin<MeshSegmentation> DescriptorClusters(MeshDescriptor descriptor, int eigenpairs, int regionCount, int maxIterations, double tolerance, Op? key = null) =>
        key.OrDefault() switch { Op op => from active in Optional(descriptor).ToFin(op.InvalidInput()) from _ in guard(active.IsValid, op.InvalidInput()) from pairs in op.AcceptValidated<Dimension>(candidate: eigenpairs) from regions in op.AcceptValidated<Dimension>(candidate: regionCount) from __ in regionCount > 1 ? Fin.Succ(unit) : Fin.Fail<Unit>(op.InvalidInput()) from cap in op.AcceptValidated<Dimension>(candidate: maxIterations) from eps in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance) select (MeshSegmentation)new DescriptorClustersCase(Descriptor: active, Eigenpairs: pairs, RegionCount: regions, MaxIterations: cap, Tolerance: eps) };
    private static Fin<Arr<double>> AdmitScalars(Arr<double> values, Op key) =>
        values.Count == 0
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : Fin.Succ(values);
}

[Union]
public abstract partial record RemeshKind {
    private RemeshKind() { }
    public sealed record QuadCase(PositiveMagnitude TargetLength) : RemeshKind;
    public sealed record SimplifyCase(ReduceMeshParameters Parameters) : RemeshKind;
    public static Fin<RemeshKind> Quad(double targetLength, Op? key = null) => key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: targetLength).Map(t => (RemeshKind)new QuadCase(TargetLength: t));
    public static Fin<RemeshKind> Simplify(ReduceMeshParameters parameters, Op? key = null) =>
        key.OrDefault() switch {
            Op op => Optional(parameters).ToFin(op.InvalidInput())
                .Bind(active => guard(active.DesiredPolygonCount >= 1, op.InvalidInput())
                    .Bind(_ => Fin.Succ<RemeshKind>(new SimplifyCase(Parameters: active)))),
        };
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class MeshKernel {
    private const double DegenerateTriangleArea = 1e-14;
    private const double AspectRatioCeiling = 11.5;
    private const int UnassignedRegion = -1;
    private readonly record struct SegmentationScalars(Arr<double> FaceValues, int SkippedDegenerateFaces, int SkippedNonFiniteValues, int FiniteCount, double Min, double Max);
    private readonly record struct SegmentationRun(MeshSegmentationAlgorithm Algorithm, int RequestedRegionCount, int SeedCount, MeshSegmentationStatus Status, Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance, Option<double> Threshold, Option<DescriptorReceipt> Descriptor);
    private readonly record struct ClusterState(int[] Labels, int Iterations, bool Converged);
    private sealed class LaplacianTriplets {
        private readonly int vertexCount;
        internal readonly List<(int Row, int Col, double Value)> Stiffness = [], Mass = [];
        internal readonly double[] Lumped;
        internal int SkippedDegenerateFaces;
        internal LaplacianTriplets(int vertexCount) {
            this.vertexCount = vertexCount;
            Lumped = new double[vertexCount];
        }
        internal void AddTriangle(int va, int vb, int vc, double area, double cotA, double cotB, double cotC) {
            Stiffness.Add(item: (vb, vc, -0.5 * cotA)); Stiffness.Add(item: (vc, vb, -0.5 * cotA));
            Stiffness.Add(item: (vc, va, -0.5 * cotB)); Stiffness.Add(item: (va, vc, -0.5 * cotB));
            Stiffness.Add(item: (va, vb, -0.5 * cotC)); Stiffness.Add(item: (vb, va, -0.5 * cotC));
            Stiffness.Add(item: (va, va, 0.5 * (cotB + cotC)));
            Stiffness.Add(item: (vb, vb, 0.5 * (cotA + cotC)));
            Stiffness.Add(item: (vc, vc, 0.5 * (cotA + cotB)));
            double m = area / 12.0;
            Mass.Add(item: (va, va, 2.0 * m)); Mass.Add(item: (vb, vb, 2.0 * m)); Mass.Add(item: (vc, vc, 2.0 * m));
            Mass.Add(item: (va, vb, m)); Mass.Add(item: (vb, va, m));
            Mass.Add(item: (vb, vc, m)); Mass.Add(item: (vc, vb, m));
            Mass.Add(item: (va, vc, m)); Mass.Add(item: (vc, va, m));
            double third = area / 3.0;
            Lumped[va] += third; Lumped[vb] += third; Lumped[vc] += third;
        }
        internal Fin<SparseLaplacian> Build(Op key) =>
            from stiff in SparseMatrix.FromTriplets(rows: Dimension.Create(value: vertexCount), cols: Dimension.Create(value: vertexCount), triplets: Stiffness, key: key)
            from mass in SparseMatrix.FromTriplets(rows: Dimension.Create(value: vertexCount), cols: Dimension.Create(value: vertexCount), triplets: Mass, key: key)
            select new SparseLaplacian(Stiffness: stiff, MassConsistent: mass, MassLumped: new Arr<double>(Lumped), SkippedDegenerateFaces: SkippedDegenerateFaces);
    }
    private static bool ContainsQuads(Mesh mesh) => mesh.Faces.QuadCount > 0;

    // --- [VALIDATION] -----------------------------------------------------------------------
    // BOUNDARY ADAPTER — native face iteration avoids materialising large meshes into Seq.
    internal static Fin<Unit> AspectRatioGuard(Mesh mesh, double ceiling, Op key) {
        for (int f = 0; f < mesh.Faces.Count; f++) {
            double aspect = mesh.Faces.GetFaceAspectRatio(index: f);
            if (!RhinoMath.IsValidDouble(x: aspect) || aspect > ceiling)
                return Fin.Fail<Unit>(key.Caution(concern: $"Face {f} aspect ratio exceeds {ceiling}."));
        }
        return Fin.Succ(unit);
    }

    // --- [COTANGENT_ASSEMBLY] ---------------------------------------------------------------
    // BOUNDARY ADAPTER — native face iteration builds sparse triplets without duplicating mesh storage.
    internal static Fin<SparseLaplacian> AssembleCotangent(Mesh mesh, Op key) {
        using Mesh active = mesh.DuplicateMesh();
        if (ContainsQuads(mesh: active) && !active.Faces.ConvertQuadsToTriangles())
            return Fin.Fail<SparseLaplacian>(key.InvalidResult());
        LaplacianTriplets triplets = new(vertexCount: active.Vertices.Count);
        for (int f = 0; f < active.Faces.Count; f++) {
            MeshFace face = active.Faces[index: f];
            if (!face.IsTriangle) continue;
            int va = face.A; int vb = face.B; int vc = face.C;
            Point3d pa = active.Vertices[index: va]; Point3d pb = active.Vertices[index: vb]; Point3d pc = active.Vertices[index: vc];
            Vector3d ab = pb - pa; Vector3d ac = pc - pa; Vector3d bc = pc - pb;
            double area = 0.5 * Vector3d.CrossProduct(a: ab, b: ac).Length;
            if (area < DegenerateTriangleArea) { triplets.SkippedDegenerateFaces++; continue; }
            double cotA = -ab * -ac / (2.0 * area);
            double cotB = ab * -bc / (2.0 * area);
            double cotC = ac * bc / (2.0 * area);
            if (cotA < 0.0 || cotB < 0.0 || cotC < 0.0)
                return Fin.Fail<SparseLaplacian>(key.InvalidResult());
            triplets.AddTriangle(va: va, vb: vb, vc: vc, area: area, cotA: cotA, cotB: cotB, cotC: cotC);
        }
        return triplets.Build(key: key);
    }

    // --- [IDT_AND_NONMANIFOLD] --------------------------------------------------------------
    internal static Fin<IntrinsicMesh> BuildIntrinsicMesh(Mesh mesh, Op key) =>
        from source in IntrinsicMesh.FromMesh(mesh: mesh, key: key)
        from flipped in FlipToDelaunay(imesh: source, key: key)
        select flipped.Freeze();

    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct IntrinsicEdge(int Lo, int Hi, double Length, int Face0, int Face1) {
        internal bool IsInterior => Face1 >= 0;
    }

    internal sealed class IntrinsicMesh {
        internal int VertexCount;
        internal readonly List<(int A, int B, int C)?> Triangles = [];
        internal readonly Dictionary<(int Lo, int Hi), (double Length, List<int> FaceIdx)> EdgeData = [];
        internal bool HasFlips;
        private IntrinsicEdge[]? frozenEdges;
        private Dictionary<(int Lo, int Hi), int>? frozenEdgeIndex;
        private int[][]? frozenFaceEdges;
        private double[]? frozenFaceAreas;
        private int[]? frozenFirstIncidentEdge;
        internal bool IsFrozen => frozenEdges is not null;
        internal int EdgeCount => frozenEdges?.Length ?? 0;
        internal IntrinsicEdge EdgeAt(int index) => frozenEdges![index];
        internal int IndexOfEdge(int lo, int hi) =>
            frozenEdgeIndex is { } edges && edges.TryGetValue(key: EdgeKey(i: lo, j: hi), value: out int idx) ? idx : -1;
        internal int[] EdgesOfFace(int faceIdx) => frozenFaceEdges![faceIdx];
        internal double AreaOfFace(int faceIdx) => frozenFaceAreas![faceIdx];
        internal int FirstIncidentEdge(int vertexIdx) => frozenFirstIncidentEdge![vertexIdx];
        internal IEnumerable<int> LiveFaceIndices() {
            for (int f = 0; f < Triangles.Count; f++) if (Triangles[index: f].HasValue) yield return f;
        }
        internal IntrinsicMesh Freeze() {
            foreach (KeyValuePair<(int Lo, int Hi), (double Length, List<int> FaceIdx)> kv in EdgeData)
                _ = kv.Value.FaceIdx.RemoveAll(match: face => face < 0 || face >= Triangles.Count || !Triangles[index: face].HasValue);
            foreach ((int Lo, int Hi) key in EdgeData
                         .Where(static kv => kv.Value.FaceIdx.Count == 0 || !RhinoMath.IsValidDouble(x: kv.Value.Length) || kv.Value.Length <= RhinoMath.ZeroTolerance)
                         .Select(static kv => kv.Key)
                         .ToArray())
                _ = EdgeData.Remove(key: key);
            List<KeyValuePair<(int Lo, int Hi), (double Length, List<int> FaceIdx)>> orderedEdges = [.. EdgeData
                .OrderBy(static kv => kv.Key.Lo)
                .ThenBy(static kv => kv.Key.Hi)];
            int eCount = orderedEdges.Count;
            frozenEdges = new IntrinsicEdge[eCount];
            frozenEdgeIndex = new Dictionary<(int Lo, int Hi), int>(capacity: eCount);
            int idx = 0;
            foreach (KeyValuePair<(int Lo, int Hi), (double Length, List<int> FaceIdx)> kv in orderedEdges) {
                List<int> faces = kv.Value.FaceIdx;
                int f0 = faces.Count > 0 ? faces[index: 0] : -1;
                int f1 = faces.Count > 1 ? faces[index: 1] : -1;
                frozenEdges[idx] = new IntrinsicEdge(Lo: kv.Key.Lo, Hi: kv.Key.Hi, Length: kv.Value.Length, Face0: f0, Face1: f1);
                frozenEdgeIndex[key: kv.Key] = idx;
                idx++;
            }
            int faceCapacity = Triangles.Count;
            frozenFaceEdges = new int[faceCapacity][];
            frozenFaceAreas = new double[faceCapacity];
            for (int f = 0; f < faceCapacity; f++) {
                (int A, int B, int C)? slot = Triangles[index: f];
                if (slot is null) { frozenFaceEdges[f] = []; continue; }
                (int a, int b, int c) = slot.Value;
                int eAB = IndexOfEdge(lo: a, hi: b);
                int eBC = IndexOfEdge(lo: b, hi: c);
                int eCA = IndexOfEdge(lo: c, hi: a);
                if (eAB < 0 || eBC < 0 || eCA < 0) { Triangles[index: f] = null; frozenFaceEdges[f] = []; continue; }
                frozenFaceEdges[f] = [eAB, eBC, eCA];
                double lAB = frozenEdges[eAB].Length;
                double lBC = frozenEdges[eBC].Length;
                double lCA = frozenEdges[eCA].Length;
                double s = (lAB + lBC + lCA) * 0.5;
                double argInside = s * (s - lAB) * (s - lBC) * (s - lCA);
                frozenFaceAreas[f] = Math.Sqrt(d: Math.Max(val1: 0.0, val2: argInside));
            }
            frozenFirstIncidentEdge = new int[VertexCount];
            System.Array.Fill(array: frozenFirstIncidentEdge, value: -1);
            for (int e = 0; e < frozenEdges.Length; e++) {
                IntrinsicEdge edge = frozenEdges[e];
                if (frozenFirstIncidentEdge[edge.Lo] < 0) frozenFirstIncidentEdge[edge.Lo] = e;
                if (frozenFirstIncidentEdge[edge.Hi] < 0) frozenFirstIncidentEdge[edge.Hi] = e;
            }
            return this;
        }
        private static (int, int) EdgeKey(int i, int j) => (Math.Min(val1: i, val2: j), Math.Max(val1: i, val2: j));
        internal static Fin<IntrinsicMesh> FromMesh(Mesh mesh, Op key) {
            using Mesh active = mesh.DuplicateMesh();
            if (ContainsQuads(mesh: active) && !active.Faces.ConvertQuadsToTriangles())
                return Fin.Fail<IntrinsicMesh>(key.InvalidResult());
            IntrinsicMesh m = new() { VertexCount = active.Vertices.Count };
            for (int f = 0; f < active.Faces.Count; f++) {
                MeshFace face = active.Faces[index: f];
                if (!face.IsTriangle) continue;
                Point3d pa = active.Vertices[index: face.A]; Point3d pb = active.Vertices[index: face.B]; Point3d pc = active.Vertices[index: face.C];
                _ = m.AddTriangle(a: face.A, b: face.B, c: face.C, lAB: pa.DistanceTo(other: pb), lBC: pb.DistanceTo(other: pc), lAC: pa.DistanceTo(other: pc));
            }
            return Fin.Succ(m);
        }
        internal int AddTriangle(int a, int b, int c, double lAB, double lBC, double lAC) {
            int idx = Triangles.Count;
            Triangles.Add(item: (a, b, c));
            AddEdgeReference(i: a, j: b, length: lAB, faceIdx: idx);
            AddEdgeReference(i: b, j: c, length: lBC, faceIdx: idx);
            AddEdgeReference(i: a, j: c, length: lAC, faceIdx: idx);
            return idx;
        }
        private void AddEdgeReference(int i, int j, double length, int faceIdx) {
            (int Lo, int Hi) key = EdgeKey(i: i, j: j);
            ref (double Length, List<int> FaceIdx) edge = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary: EdgeData, key: key, exists: out bool exists);
            if (exists) {
                edge.FaceIdx.Add(item: faceIdx);
                return;
            }
            edge = (Length: length, FaceIdx: [faceIdx]);
        }
        internal double EdgeLengthOf(int i, int j) => EdgeData[key: EdgeKey(i: i, j: j)].Length;
        internal int OppositeVertex(int faceIdx, int i, int j) {
            (int a, int b, int c) = Triangles[index: faceIdx]!.Value;
            return a != i && a != j ? a : b != i && b != j ? b : c;
        }
        internal bool IsInterior(int i, int j) =>
            EdgeData.TryGetValue(key: EdgeKey(i: i, j: j), value: out (double Length, List<int> FaceIdx) e) && e.FaceIdx.Count == 2;
        internal bool IsDelaunay(int i, int j) {
            if (!IsInterior(i: i, j: j)) return true;
            List<int> faces = EdgeData[key: EdgeKey(i: i, j: j)].FaceIdx;
            int k1 = OppositeVertex(faceIdx: faces[index: 0], i: i, j: j);
            int k2 = OppositeVertex(faceIdx: faces[index: 1], i: i, j: j);
            double cosA = CosAngleOppositeEdge(i: i, j: j, k: k1);
            double cosB = CosAngleOppositeEdge(i: i, j: j, k: k2);
            return cosA + cosB >= -RhinoMath.SqrtEpsilon;
        }
        internal double CosAngleOppositeEdge(int i, int j, int k) {
            double lij = EdgeLengthOf(i: i, j: j); double lik = EdgeLengthOf(i: i, j: k); double ljk = EdgeLengthOf(i: j, j: k);
            return ((lik * lik) + (ljk * ljk) - (lij * lij)) / (2.0 * lik * ljk);
        }
        internal Seq<(int, int)> Flip(int i, int j) {
            HasFlips = true;
            (int Lo, int Hi) key = EdgeKey(i: i, j: j);
            List<int> faces = EdgeData[key: key].FaceIdx;
            int f1 = faces[index: 0]; int f2 = faces[index: 1];
            int k1 = OppositeVertex(faceIdx: f1, i: i, j: j);
            int k2 = OppositeVertex(faceIdx: f2, i: i, j: j);
            double li_k1 = EdgeLengthOf(i: i, j: k1); double li_k2 = EdgeLengthOf(i: i, j: k2);
            double lij = EdgeLengthOf(i: i, j: j);
            double lj_k1 = EdgeLengthOf(i: j, j: k1); double lj_k2 = EdgeLengthOf(i: j, j: k2);
            double cosAtI_f1 = ((lij * lij) + (li_k1 * li_k1) - (lj_k1 * lj_k1)) / (2.0 * lij * li_k1);
            double cosAtI_f2 = ((lij * lij) + (li_k2 * li_k2) - (lj_k2 * lj_k2)) / (2.0 * lij * li_k2);
            double sinAtI_f1 = Math.Sqrt(d: Math.Max(val1: 0.0, val2: 1.0 - (cosAtI_f1 * cosAtI_f1)));
            double sinAtI_f2 = Math.Sqrt(d: Math.Max(val1: 0.0, val2: 1.0 - (cosAtI_f2 * cosAtI_f2)));
            double cosSum = (cosAtI_f1 * cosAtI_f2) - (sinAtI_f1 * sinAtI_f2);
            double l_k1k2 = Math.Sqrt(d: Math.Max(val1: 0.0, val2: (li_k1 * li_k1) + (li_k2 * li_k2) - (2.0 * li_k1 * li_k2 * cosSum)));
            _ = EdgeData.Remove(key: key);
            Triangles[index: f1] = null; Triangles[index: f2] = null;
            (int, int)[] surrounding = [(i, k1), (j, k1), (i, k2), (j, k2)];
            for (int s = 0; s < surrounding.Length; s++) {
                (int Lo, int Hi) ek = EdgeKey(i: surrounding[s].Item1, j: surrounding[s].Item2);
                if (EdgeData.TryGetValue(key: ek, value: out (double Length, List<int> FaceIdx) e)) { _ = e.FaceIdx.Remove(item: f1); _ = e.FaceIdx.Remove(item: f2); }
            }
            _ = AddTriangle(a: i, b: k1, c: k2, lAB: li_k1, lBC: l_k1k2, lAC: li_k2);
            _ = AddTriangle(a: j, b: k1, c: k2, lAB: lj_k1, lBC: l_k1k2, lAC: lj_k2);
            return Seq((i, k1), (j, k1), (i, k2), (j, k2));
        }
    }
    private static Fin<IntrinsicMesh> FlipToDelaunay(IntrinsicMesh imesh, Op key) {
        Queue<(int, int)> queue = new(collection: imesh.EdgeData.Keys.OrderBy(static k => k.Lo).ThenBy(static k => k.Hi).Select(static k => (k.Lo, k.Hi)));
        int flipCap = imesh.EdgeData.Count * 16;
        int flips = 0;
        while (queue.Count > 0 && flips < flipCap) {
            (int i, int j) = queue.Dequeue();
            if (!imesh.IsInterior(i: i, j: j) || imesh.IsDelaunay(i: i, j: j)) continue;
            Seq<(int, int)> affected = imesh.Flip(i: i, j: j);
            foreach ((int A, int B) edge in affected.AsIterable()
                         .OrderBy(static edge => Math.Min(val1: edge.Item1, val2: edge.Item2))
                         .ThenBy(static edge => Math.Max(val1: edge.Item1, val2: edge.Item2)))
                queue.Enqueue(item: edge);
            flips++;
        }
        return imesh.EdgeData.Keys.All(edge => imesh.IsDelaunay(i: edge.Lo, j: edge.Hi))
            ? Fin.Succ(imesh)
            : Fin.Fail<IntrinsicMesh>(key.InvalidResult());
    }
    internal static Fin<SparseLaplacian> AssembleCotangentFromIntrinsic(IntrinsicMesh imesh, Op key) {
        LaplacianTriplets triplets = new(vertexCount: imesh.VertexCount);
        for (int f = 0; f < imesh.Triangles.Count; f++) {
            (int A, int B, int C)? slot = imesh.Triangles[index: f];
            if (slot is null) continue;
            (int va, int vb, int vc) = slot.Value;
            double lAB = imesh.EdgeLengthOf(i: va, j: vb);
            double lBC = imesh.EdgeLengthOf(i: vb, j: vc);
            double lAC = imesh.EdgeLengthOf(i: va, j: vc);
            double s = (lAB + lBC + lAC) * 0.5;
            double areaSq = s * (s - lAB) * (s - lBC) * (s - lAC);
            if (areaSq < DegenerateTriangleArea * DegenerateTriangleArea) { triplets.SkippedDegenerateFaces++; continue; }
            double area = Math.Sqrt(d: areaSq);
            double cotA = ((lAC * lAC) + (lAB * lAB) - (lBC * lBC)) / (4.0 * area);
            double cotB = ((lAB * lAB) + (lBC * lBC) - (lAC * lAC)) / (4.0 * area);
            double cotC = ((lAC * lAC) + (lBC * lBC) - (lAB * lAB)) / (4.0 * area);
            if (cotA < -RhinoMath.SqrtEpsilon || cotB < -RhinoMath.SqrtEpsilon || cotC < -RhinoMath.SqrtEpsilon)
                return Fin.Fail<SparseLaplacian>(key.InvalidResult());
            triplets.AddTriangle(va: va, vb: vb, vc: vc, area: area, cotA: cotA, cotB: cotB, cotC: cotC);
        }
        return triplets.Build(key: key);
    }

    // --- [SELECTION] ------------------------------------------------------------------------
    internal static Fin<SparseLaplacian> LaplacianOf(MeshSpace space, MeshLaplacian kind, Op key) =>
        from active in Optional(kind).ToFin(key.InvalidInput())
        from _ in active.Equals(MeshLaplacian.Cotangent)
            ? AspectRatioGuard(mesh: space.Native, ceiling: AspectRatioCeiling, key: key)
            : Fin.Succ(unit)
        from result in active.Select(cache: space.Cache)
        select result;

    // --- [SPD_PIN] --------------------------------------------------------------------------
    // BOUNDARY ADAPTER -- CSR triplet enumeration avoids full MathNet materialisation.
    internal static Fin<SparseMatrix> AssembleMassStiffnessSystem(SparseLaplacian laplacian, double stiffnessScale, Op key, double massScale = 1.0) {
        int n = laplacian.Stiffness.Rows.Value;
        if (n == 0) return Fin.Fail<SparseMatrix>(error: key.InvalidInput());
        List<(int Row, int Col, double Value)> triplets = SparseTripletsOf(matrix: laplacian.Stiffness, capacityBonus: n, scale: stiffnessScale);
        for (int i = 0; i < n; i++) triplets.Add(item: (i, i, massScale * laplacian.MassLumped[index: i]));
        Dimension dim = Dimension.Create(value: n);
        return SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: triplets, key: key);
    }
    private static List<(int Row, int Col, double Value)> SparseTripletsOf(SparseMatrix matrix, int capacityBonus = 0, double scale = 1.0) {
        int n = matrix.Rows.Value;
        List<(int Row, int Col, double Value)> triplets = new(capacity: matrix.NonZeros + capacityBonus);
        for (int i = 0; i < n; i++)
            for (int k = matrix.RowPtr[index: i]; k < matrix.RowPtr[index: i + 1]; k++)
                triplets.Add(item: (i, matrix.ColInd[index: k], scale * matrix.Values[index: k]));
        return triplets;
    }

    internal static Fin<SparseLaplacian> AssembleRobust(Mesh mesh, Op key) =>
        Optional(mesh).ToFin(key.InvalidInput()).Bind(_ =>
            Fin.Fail<SparseLaplacian>(key.Unsupported(geometryType: typeof(MeshLaplacian), outputType: typeof(SparseLaplacian))));

    // --- [SPECTRAL_BASIS] ------------------------------------------------------------------
    internal static Fin<SpectralBasisBundle> ComputeSpectralBasisDetailed(MeshSpace space, int k, Op key) =>
        from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
        from receipt in MatrixKernel.GeneralizedEigenpairsDetailed(stiffness: laplacian.Stiffness, mass: laplacian.MassConsistent, k: k, key: key)
        select new SpectralBasisBundle(
            Basis: new SpectralBasis(
                Eigenvalues: new Arr<double>([.. receipt.Pairs.AsIterable().Select(static p => p.Eigenvalue)]),
                Eigenvectors: new Arr<Arr<double>>([.. receipt.Pairs.AsIterable().Select(static p => p.Eigenvector)])),
            Eigen: receipt,
            CacheHit: false,
            SkippedDegenerateFaces: laplacian.SkippedDegenerateFaces,
            FactorNonZeros: Option<int>.None);

    // --- [METRICS] --------------------------------------------------------------------------
    internal static double MeanEdgeLengthOf(Mesh mesh) {
        double sum = 0.0; int count = 0;
        for (int e = 0; e < mesh.TopologyEdges.Count; e++) {
            Line line = mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: e);
            if (!line.IsValid) continue;
            sum += line.Length;
            count++;
        }
        return count > 0 ? sum / count : 0.0;
    }
    internal static Fin<TopologyReceipt> TopologyDetailed(MeshSpace space, Op key) {
        Mesh mesh = space.Native;
        bool manifold = mesh.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out _);
        int euler = mesh.TopologyVertices.Count - mesh.TopologyEdges.Count + mesh.Faces.Count;
        (int boundaryComponents, int nonManifoldEdges) = TopologyEdgeStatsOf(mesh: mesh);
        int components = Math.Max(val1: 1, val2: mesh.DisjointMeshCount);
        int numerator = (2 * components) - boundaryComponents - euler;
        bool hasGenus = manifold && oriented && numerator >= 0 && numerator % 2 == 0;
        return Fin.Succ(new TopologyReceipt(Vertices: mesh.Vertices.Count, TopologyVertices: mesh.TopologyVertices.Count, TopologyEdges: mesh.TopologyEdges.Count, Faces: mesh.Faces.Count, Triangles: mesh.Faces.TriangleCount, Quads: mesh.Faces.QuadCount, Ngons: mesh.Ngons.Count, VisiblePolygons: mesh.GetNgonAndFacesCount(), BoundaryComponents: boundaryComponents, NonManifoldEdges: nonManifoldEdges, IsManifold: manifold, IsOriented: oriented, EulerCharacteristic: euler, Genus: hasGenus ? Some(numerator / 2) : Option<int>.None, EulerValidated: hasGenus));
    }
    private static (int BoundaryComponents, int NonManifoldEdges) TopologyEdgeStatsOf(Mesh mesh) {
        int nonManifoldEdges = 0;
        for (int edge = 0; edge < mesh.TopologyEdges.Count; edge++) {
            int faceCount = mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: edge).Length;
            if (faceCount > 2) nonManifoldEdges++;
        }
        return (BoundaryComponents: mesh.GetNakedEdges()?.Length ?? 0, NonManifoldEdges: nonManifoldEdges);
    }
    // BOUNDARY ADAPTER — topology-edge classification preserves native ngon boundaries.
    internal static Fin<FeatureReceipt> DetectFeatureEdgesDetailed(MeshSpace space, double dihedralRadians, Op key) {
        if (!RhinoMath.IsValidDouble(x: dihedralRadians) || dihedralRadians <= 0.0)
            return Fin.Fail<FeatureReceipt>(key.InvalidInput());
        Mesh mesh = space.Native;
        _ = mesh.FaceNormals.ComputeFaceNormals();
        Vector3f[] faceNormals = [.. mesh.FaceNormals];
        List<FeatureEdge> features = new(capacity: mesh.TopologyEdges.Count);
        int[] counts = new int[MeshFeatureKind.Items.Count];
        for (int e = 0; e < mesh.TopologyEdges.Count; e++) {
            int[] faces = mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: e);
            IndexPair p = mesh.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: e);
            (MeshFeatureKind Kind, Option<double> Angle)? feature = faces.Length switch {
                1 => (MeshFeatureKind.Boundary, Option<double>.None),
                > 2 => (MeshFeatureKind.NonManifold, Option<double>.None),
                2 => (
                    mesh.Ngons.NgonIndexFromFaceIndex(faces[0]),
                    mesh.Ngons.NgonIndexFromFaceIndex(faces[1]),
                    Vector3d.VectorAngle(a: (Vector3d)faceNormals[faces[0]], b: (Vector3d)faceNormals[faces[1]])) switch {
                        (int a, int b, _) when a >= 0 && a == b => (MeshFeatureKind.NgonInteriorSkipped, Option<double>.None),
                        (_, _, double angle) when angle >= dihedralRadians => (MeshFeatureKind.Crease, Some(angle)),
                        _ => null,
                    },
                _ => null,
            };
            if (feature is not { } edge) continue;
            features.Add(item: new FeatureEdge(A: p.I, B: p.J, Kind: edge.Kind, DihedralRadians: edge.Angle));
            counts[edge.Kind.Key]++;
        }
        return Fin.Succ(new FeatureReceipt(Edges: toSeq(features), BoundaryEdges: counts[MeshFeatureKind.Boundary.Key], CreaseEdges: counts[MeshFeatureKind.Crease.Key], NonManifoldEdges: counts[MeshFeatureKind.NonManifold.Key], UnweldedEdges: counts[MeshFeatureKind.Unwelded.Key], NgonInteriorSkippedEdges: counts[MeshFeatureKind.NgonInteriorSkipped.Key], DihedralThresholdRadians: dihedralRadians));
    }

    internal static Fin<FlattenResult> ParameterizeFlattenDetailed(MeshSpace space, Op key) {
        using Mesh mesh = space.Native.DuplicateMesh();
        using MeshUnwrapper unwrapper = new(mesh);
        bool ok = unwrapper.Unwrap(method: MeshUnwrapMethod.LSCM);
        if (!ok || mesh.TextureCoordinates.Count != mesh.Vertices.Count)
            return Fin.Fail<FlattenResult>(error: key.InvalidResult());
        Arr<Point2d> uvs = new([.. mesh.TextureCoordinates.Select(static t => new Point2d(x: t.X, y: t.Y))]);
        Mesh output = mesh.DuplicateMesh();
        (int boundaryComponents, _) = TopologyEdgeStatsOf(mesh: output);
        return Fin.Succ(new FlattenResult(Uvs: uvs, Mesh: output, Receipt: new FlattenReceipt(VertexCount: output.Vertices.Count, UvCount: uvs.Count, TextureCoordinateCount: output.TextureCoordinates.Count, BoundaryComponents: boundaryComponents, NativeUnwrap: true, Valid: output.IsValid, EdgeLengthDistortionRms: Option<double>.None)));
    }
    // --- [HEAT_METHOD] ----------------------------------------------------------------------
    // Crane-Weischedel-Wardetzky 2013: solve (M + tL)u = δ; X = -∇u/|∇u|; Lφ = ∇·X.
    internal static Fin<double> HeatGeodesicAt(MeshSpace space, Seq<int> sources, Point3d sample, Op key) =>
        from distances in EnsureGeodesicDistances(space: space, sources: sources, key: key)
        from value in InterpolateOnMesh(space: space, sample: sample, perVertex: distances, key: key)
        select value;
    private static Fin<Arr<double>> EnsureGeodesicDistances(MeshSpace space, Seq<int> sources, Op key) {
        int n = space.Native.Vertices.Count;
        Seq<int> ordered = toSeq(sources.AsIterable().Distinct().OrderBy(static i => i));
        return ordered.IsEmpty || ordered.Exists(i => i < 0 || i >= n)
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : space.Cache.Geodesic(probe: new GeodesicKey(Sources: ordered),
                compute: () => from imesh in space.Cache.IntrinsicMeshSnapshot
                               from _ in guard(!imesh.HasFlips, key.Unsupported(geometryType: typeof(IntrinsicMesh), outputType: typeof(Arr<double>)))
                               from L in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
                               from distances in ComputeHeatGeodesic(space: space, laplacian: L, sources: ordered, key: key)
                               select distances);
    }
    private static Fin<Arr<double>> ComputeHeatGeodesic(MeshSpace space, SparseLaplacian laplacian, Seq<int> sources, Op key) {
        int n = space.Native.Vertices.Count;
        if (sources.IsEmpty) return Fin.Fail<Arr<double>>(key.InvalidInput());
        double h = space.Cache.MeanEdgeLength;
        if (h <= RhinoMath.ZeroTolerance) return Fin.Fail<Arr<double>>(key.InvalidResult());
        double t = h * h;
        Dimension dim = Dimension.Create(value: n);
        return from heatFactor in space.Cache.ScalarHeatCholesky(time: t, key: key)
               from delta in Fin.Succ(SpectralCore.BuildSourceDelta(n: n, sources: sources, mass: laplacian.MassLumped))
               from u in heatFactor.Solve(rhs: delta, key: key)
               from gradient in Fin.Succ(SpectralCore.ComputeTriangleGradients(mesh: space.Native, u: u))
               from divergence in Fin.Succ(SpectralCore.ComputeVertexDivergence(mesh: space.Native, gradients: gradient))
               from poisson in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: SpectralCore.PoissonTriplets(L: laplacian, sources: sources), key: key)
               from phi in poisson.Solve(rhs: divergence, key: key)
               select NormalizeToZero(values: phi);
    }
    private static Arr<double> NormalizeToZero(Arr<double> values) {
        double min = values.Fold(initialState: double.MaxValue, f: static (m, v) => Math.Min(val1: m, val2: v));
        return new Arr<double>([.. values.AsIterable().Select(v => v - min)]);
    }
    private static Fin<T> ClosestFace<T>(MeshSpace space, Point3d sample, Op key, Func<Mesh, MeshFace, double[], int, Fin<T>> project) {
        MeshPoint meshPoint = space.Native.ClosestMeshPoint(testPoint: sample, maximumDistance: MeshSearchDistance(space: space));
        return meshPoint is null || meshPoint.FaceIndex < 0
            ? Fin.Fail<T>(key.InvalidResult())
            : project(space.Native, space.Native.Faces[index: meshPoint.FaceIndex], meshPoint.T, meshPoint.FaceIndex);
    }
    private static Fin<double> InterpolateOnMesh(MeshSpace space, Point3d sample, Arr<double> perVertex, Op key) =>
        ClosestFace(space: space, sample: sample, key: key, project: (mesh, face, weights, _) => key.AcceptValue(value: InterpolateFaceValue(mesh: mesh, face: face, weights: weights, perVertex: perVertex)));
    private static double MeshSearchDistance(MeshSpace space) =>
        Math.Max(val1: space.Tolerance.Absolute.Value, val2: space.Cache.MeanEdgeLength);
    private static double InterpolateFaceValue(Mesh mesh, MeshFace face, double[] weights, Arr<double> perVertex) {
        double value = (weights[0] * perVertex[index: face.A]) + (weights[1] * perVertex[index: face.B]) + (weights[2] * perVertex[index: face.C]);
        return face.IsQuad ? value + (weights[3] * perVertex[index: face.D]) : value;
    }

    // --- [MEAN_CURVATURE_FLOW] --------------------------------------------------------------
    internal static Fin<double> MeanCurvatureMagnitudeAt(MeshSpace space, double timeStep, int iterations, Point3d sample, Op key) =>
        from displacements in EnsureMcfDisplacements(space: space, timeStep: timeStep, iterations: iterations, key: key)
        from value in InterpolateOnMesh(space: space, sample: sample, perVertex: displacements, key: key)
        select value;
    private static Fin<Arr<double>> EnsureMcfDisplacements(MeshSpace space, double timeStep, int iterations, Op key) =>
        !RhinoMath.IsValidDouble(x: timeStep) || timeStep <= 0.0 || iterations < 1
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : space.Cache.Mcf(probe: new McfKey(TimeStep: timeStep, Iterations: iterations),
            compute: () => space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
                .Bind(L => from A in AssembleMassStiffnessSystem(laplacian: L, stiffnessScale: timeStep, key: key)
                           from factor in CholeskySparse.Of(symmetric: A, key: key)
                           from final in IterateMcf(space: space, mass: L.MassLumped, system: factor, iterations: iterations, key: key)
                           select ComputeDisplacements(original: space.Native, smoothed: final)));
    private static Fin<double[][]> IterateMcf(MeshSpace space, Arr<double> mass, CholeskySparse system, int iterations, Op key) {
        int n = space.Native.Vertices.Count;
        double[][] coordinates = [new double[n], new double[n], new double[n]];
        for (int i = 0; i < n; i++) { Point3d v = space.Native.Vertices[index: i]; coordinates[0][i] = v.X; coordinates[1][i] = v.Y; coordinates[2][i] = v.Z; }
        return toSeq(Enumerable.Range(start: 0, count: iterations)).Fold(
            initialState: Fin.Succ(coordinates),
            f: (state, _) => state.Bind(current => {
                double[][] rhs = [new double[n], new double[n], new double[n]];
                for (int i = 0; i < n; i++) { double m = mass[index: i]; rhs[0][i] = m * current[0][i]; rhs[1][i] = m * current[1][i]; rhs[2][i] = m * current[2][i]; }
                return toSeq(rhs).TraverseM(axis => system.Solve(rhs: new Arr<double>(axis), key: key).Map(solution => solution.AsIterable().ToArray())).As().Map(axes => axes.AsIterable().ToArray());
            }));
    }
    private static Arr<double> ComputeDisplacements(Mesh original, double[][] smoothed) {
        int n = original.Vertices.Count;
        double[] mag = new double[n];
        for (int i = 0; i < n; i++) {
            Point3d before = original.Vertices[index: i];
            mag[i] = new Vector3d(x: smoothed[0][i] - before.X, y: smoothed[1][i] - before.Y, z: smoothed[2][i] - before.Z).Length;
        }
        return new Arr<double>(mag);
    }

    // --- [TANGENT_FRAMES] -------------------------------------------------------------------
    private sealed record FrameBundle(Vector3d[] X, Vector3d[] Y, Vector3d[] N);
    private static readonly ConditionalWeakTable<object, FrameBundle> FrameTable = [];
    private static FrameBundle EnsureVertexFrames(Mesh mesh) =>
        FrameTable.GetValue(key: mesh, createValueCallback: static m => ComputeVertexFrames(mesh: (Mesh)m));
    private static FrameBundle ComputeVertexFrames(Mesh mesh) {
        int n = mesh.Vertices.Count;
        using Mesh active = mesh.DuplicateMesh();
        _ = active.FaceNormals.ComputeFaceNormals();
        _ = active.Normals.ComputeNormals();
        Vector3d[] normals = new Vector3d[n];
        Vector3d[] xAxes = new Vector3d[n]; Vector3d[] yAxes = new Vector3d[n];
        for (int v = 0; v < n; v++) {
            Vector3d normal = v < active.Normals.Count ? (Vector3d)active.Normals[index: v] : Vector3d.ZAxis;
            if (!normal.IsValid || normal.IsTiny() || !normal.Unitize()) normal = Vector3d.ZAxis;
            normals[v] = normal;
            Vector3d tx = VectorFrame.SeedPerpendicular(axis: normal);
            _ = tx.Unitize();
            xAxes[v] = tx; yAxes[v] = Vector3d.CrossProduct(a: normal, b: tx);
        }
        return new FrameBundle(X: xAxes, Y: yAxes, N: normals);
    }
    private static double EdgeAngleInFrame(Vector3d edge, Vector3d xAxis, Vector3d yAxis) =>
        Math.Atan2(y: edge * yAxis, x: edge * xAxis);

    // Flipped intrinsic edges need signpost transport, so this rail fails before use.
    private static Fin<Seq<(int I, int J, double Weight, double Rho)>> EnumerateConnectionEntries(MeshSpace space, Option<Arr<double>> edgeAdjustment, Op key) =>
        space.Cache.IntrinsicMeshSnapshot.Bind(imesh => {
            if (imesh.HasFlips)
                return Fin.Fail<Seq<(int I, int J, double Weight, double Rho)>>(key.Unsupported(geometryType: typeof(IntrinsicMesh), outputType: typeof(SparseHermitian)));
            Mesh mesh = space.Native;
            FrameBundle fb = EnsureVertexFrames(mesh: mesh);
            int eCount = imesh.EdgeCount;
            bool hasAdjustment = edgeAdjustment.IsSome;
            Arr<double> adjustment = edgeAdjustment.IfNone(new Arr<double>([]));
            Arr<double> weights = SpectralCore.ComputeIntrinsicStar1(imesh: imesh);
            List<(int, int, double, double)> entries = new(capacity: eCount);
            for (int e = 0; e < eCount; e++) {
                double w = weights[index: e];
                if (w < RhinoMath.ZeroTolerance) continue;
                IntrinsicEdge edge = imesh.EdgeAt(index: e);
                int i = edge.Lo, j = edge.Hi;
                Vector3d eij = (Vector3d)(mesh.Vertices[index: j] - mesh.Vertices[index: i]);
                double rho = EdgeAngleInFrame(edge: -eij, xAxis: fb.X[j], yAxis: fb.Y[j])
                           - EdgeAngleInFrame(edge: eij, xAxis: fb.X[i], yAxis: fb.Y[i]);
                if (hasAdjustment && e < adjustment.Count) rho += adjustment[index: e];
                entries.Add(item: (i, j, w, rho));
            }
            return Fin.Succ(toSeq(entries));
        });

    private static Fin<SparseHermitian> BuildConnectionLaplacian(MeshSpace space, double symmetry, Option<Arr<double>> edgeAdjustment, Op key) =>
        from entries in EnumerateConnectionEntries(space: space, edgeAdjustment: edgeAdjustment, key: key)
        let n = space.Native.Vertices.Count
        let triplets = AssembleHermitianTriplets(entries: entries, symmetry: symmetry)
        from result in SparseHermitian.FromTriplets(order: Dimension.Create(value: n), upperTriplets: triplets, key: key)
        select result;

    private static List<(int Row, int Col, Complex Value)> AssembleHermitianTriplets(Seq<(int I, int J, double Weight, double Rho)> entries, double symmetry) {
        List<(int, int, Complex)> triplets = new(capacity: entries.Count * 3);
        for (int e = 0; e < entries.Count; e++) {
            (int i, int j, double w, double rho) = entries[index: e];
            Complex offDiag = -w * Complex.FromPolarCoordinates(magnitude: 1.0, phase: symmetry * rho);
            triplets.Add(item: (i, i, new Complex(real: w, imaginary: 0.0)));
            triplets.Add(item: (j, j, new Complex(real: w, imaginary: 0.0)));
            triplets.Add(item: (i, j, offDiag));
        }
        return triplets;
    }

    // Hermitian H = A + iB embeds as [[A,-B],[B,A]] for direct Cholesky factor-and-solve.
    internal static Fin<SparseMatrix> BuildConnectionLaplacianRealSystem(MeshSpace space, double symmetry, double time, Option<Arr<double>> edgeAdjustment, Op key) =>
        from entries in EnumerateConnectionEntries(space: space, edgeAdjustment: edgeAdjustment, key: key)
        from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
        let n = space.Native.Vertices.Count
        let triplets = AssembleRealBlockTriplets(n: n, mass: laplacian.MassLumped, entries: entries, symmetry: symmetry, time: time)
        from result in SparseMatrix.FromTriplets(rows: Dimension.Create(value: 2 * n), cols: Dimension.Create(value: 2 * n), triplets: triplets, key: key)
        select result;

    private static List<(int Row, int Col, double Value)> AssembleRealBlockTriplets(int n, Arr<double> mass, Seq<(int I, int J, double Weight, double Rho)> entries, double symmetry, double time) {
        List<(int, int, double)> triplets = new(capacity: (2 * n) + (entries.Count * 12));
        for (int v = 0; v < n; v++) {
            triplets.Add(item: (v, v, mass[index: v]));
            triplets.Add(item: (v + n, v + n, mass[index: v]));
        }
        for (int e = 0; e < entries.Count; e++) {
            (int i, int j, double w, double rho) = entries[index: e];
            double re = -w * Math.Cos(d: symmetry * rho) * time;
            double im = -w * Math.Sin(a: symmetry * rho) * time;
            MatrixKernel.AddHermitianRealBlockTriplets(triplets: triplets, order: n, i: i, j: j, real: re, imaginary: im, diagonal: time * w);
        }
        return triplets;
    }

    // --- [CROSS_FIELD] ----------------------------------------------------------------------
    // GODF cross-fields use the connection Laplacian, optional hints, and cone holonomy deficits.
    internal static Fin<Vector3d> CrossFieldAt(MeshSpace space, int symmetry,
        Option<Seq<(int Vertex, Direction Hint)>> constraints,
        Option<Seq<(int Vertex, double HolonomyDeficit)>> cones,
        Point3d sample, Op key) =>
        from cached in space.Cache.CrossField(probe: new CrossFieldKey(Symmetry: symmetry, Constraints: constraints, Cones: cones),
            compute: () => ComputeCrossField(space: space, symmetry: symmetry, constraints: constraints, cones: cones, key: key))
        from value in BarycentricBlend(space: space, sample: sample, perVertex: cached, key: key,
            decode: (value, x, y) => DecodeRosy(value: value, xAxis: x, yAxis: y, symmetry: symmetry))
        select value;
    // Cone path bypasses the cache because the factor depends on the cone prescription.
    private const double ConstrainedShiftReciprocal = 1.0e9;
    private static Fin<Complex[]> ComputeCrossField(MeshSpace space, int symmetry,
        Option<Seq<(int Vertex, Direction Hint)>> constraints,
        Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, Op key) =>
        ResolveEdgeAdjustment(space: space, cones: cones, key: key).Bind(adjustment =>
            constraints.IsSome
                ? SolveConstrainedCrossField(space: space, symmetry: symmetry, hints: constraints.IfNone(toSeq<(int, Direction)>([])), edgeAdjustment: adjustment, key: key)
                : SolveSmoothestCrossField(space: space, symmetry: symmetry, edgeAdjustment: adjustment, key: key));
    private static Fin<Option<Arr<double>>> ResolveEdgeAdjustment(MeshSpace space, Option<Seq<(int Vertex, double HolonomyDeficit)>> cones, Op key) =>
        cones.IsNone
            ? Fin.Succ(Option<Arr<double>>.None)
            : from imesh in space.Cache.IntrinsicMeshSnapshot
              from adjustment in SpectralCore.DistributeHolonomy(space: space, imesh: imesh, cones: cones.IfNone(toSeq<(int, double)>([])).Map(c => (c.Vertex, ConeIndex: c.HolonomyDeficit / (2.0 * Math.PI))), key: key)
              select Some(adjustment);
    private static Fin<Complex[]> SolveSmoothestCrossField(MeshSpace space, int symmetry, Option<Arr<double>> edgeAdjustment, Op key) =>
        BuildConnectionLaplacian(space: space, symmetry: symmetry, edgeAdjustment: edgeAdjustment, key: key)
            .Bind(Lconn => Lconn.SmallestEigenpairsDetailed(k: 1, tolerance: 1e-6, maxIterations: 200, key: key).Map(static receipt => receipt.Pairs))
            .Bind(pairs => pairs.Count > 0
                ? Fin.Succ(pairs[index: 0])
                : Fin.Fail<(double Eigenvalue, Arr<Complex> Eigenvector)>(error: key.InvalidResult()))
            .Map(head => NormaliseComplexEigenvector(eigenvector: head.Eigenvector));
    private static Fin<Complex[]> SolveConstrainedCrossField(MeshSpace space, int symmetry,
        Seq<(int Vertex, Direction Hint)> hints, Option<Arr<double>> edgeAdjustment, Op key) {
        int n = space.Native.Vertices.Count;
        FrameBundle frames = EnsureVertexFrames(mesh: space.Native);
        return from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
               let qHat = EncodeAndRescaleHints(n: n, hints: hints, frames: frames, symmetry: symmetry, mass: laplacian.MassLumped)
               let rhs = StackMassWeighted(n: n, qHat: qHat, mass: laplacian.MassLumped)
               from factor in space.Cache.ConnectionCholesky(symmetry: symmetry, time: ConstrainedShiftReciprocal, edgeAdjustment: edgeAdjustment, key: key)
               from solution in factor.Solve(rhs: rhs, key: key)
               select NormaliseComplexEigenvector(eigenvector: ReassembleComplex(n: n, real: solution));
    }
    // B-norm rescale keeps hint energy independent of hint count.
    private static Complex[] EncodeAndRescaleHints(int n, Seq<(int Vertex, Direction Hint)> hints, FrameBundle frames, int symmetry, Arr<double> mass) {
        Complex[] qHat = new Complex[n];
        for (int s = 0; s < hints.Count; s++) {
            (int v, Direction hint) = hints[index: s];
            if (v < 0 || v >= n) continue;
            Complex tangent = TangentComplex(direction: hint.Value, frames: frames, vertex: v);
            double mag = tangent.Magnitude;
            if (mag < RhinoMath.SqrtEpsilon) continue;
            Complex unit = tangent / mag;
            qHat[v] = Complex.Pow(value: unit, power: symmetry);
        }
        double bNormSq = 0.0;
        for (int v = 0; v < n; v++) bNormSq += mass[index: v] * (qHat[v] * Complex.Conjugate(qHat[v])).Real;
        double bNorm = Math.Sqrt(d: bNormSq);
        if (bNorm > RhinoMath.SqrtEpsilon)
            for (int v = 0; v < n; v++) qHat[v] /= bNorm;
        return qHat;
    }
    // Mass-weighted RHS B·q̂ stacked as [Re; Im] for the real-2V Cholesky.
    private static Arr<double> StackMassWeighted(int n, Complex[] qHat, Arr<double> mass) {
        double[] rhs = new double[2 * n];
        for (int v = 0; v < n; v++) {
            double m = mass[index: v];
            WriteStackedComplex(target: rhs, n: n, vertex: v, value: m * qHat[v]);
        }
        return new Arr<double>(rhs);
    }
    private static Arr<Complex> ReassembleComplex(int n, Arr<double> real) {
        Complex[] result = new Complex[n];
        for (int v = 0; v < n; v++)
            result[v] = new Complex(real: real[index: v], imaginary: real[index: v + n]);
        return new Arr<Complex>(result);
    }
    private static Complex[] NormaliseComplexEigenvector(Arr<Complex> eigenvector) {
        int n = eigenvector.Count;
        Complex[] result = new Complex[n];
        for (int i = 0; i < n; i++) {
            Complex c = eigenvector[index: i];
            double m = c.Magnitude;
            result[i] = m > RhinoMath.ZeroTolerance ? c / m : Complex.Zero;
        }
        return result;
    }

    // --- [COMPLEX_FIELD_SAMPLING] -----------------------------------------------------------
    private static Fin<Vector3d> BarycentricBlend(MeshSpace space, Point3d sample, Complex[] perVertex, Op key, Func<Complex, Vector3d, Vector3d, Vector3d> decode) {
        FrameBundle fb = EnsureVertexFrames(mesh: space.Native);
        return ClosestFace(space: space, sample: sample, key: key, project: (_, face, weights, _) => key.AcceptValue(value:
            BarycentricVector(face: face, weights: weights, at: vertex => decode(perVertex[vertex], fb.X[vertex], fb.Y[vertex]))));
    }
    private static Vector3d DecodeRosy(Complex value, Vector3d xAxis, Vector3d yAxis, int symmetry) {
        double angle = Math.Atan2(y: value.Imaginary, x: value.Real) / Math.Max(val1: 1, val2: symmetry);
        Vector3d result = (Math.Cos(d: angle) * xAxis) + (Math.Sin(a: angle) * yAxis);
        _ = result.Unitize();
        return result;
    }

    // --- [REMESH] ---------------------------------------------------------------------------
    internal static Fin<RemeshResult> ApplyRemeshDetailed(RemeshKind kind, MeshSpace space, Op key) =>
        Optional(kind).ToFin(key.InvalidInput()).Bind(active => active.Switch(
            state: (Space: space, Key: key),
            quadCase: static (state, quad) => state.Space.Native.QuadRemesh(parameters: new QuadRemeshParameters { TargetEdgeLength = quad.TargetLength.Value, AdaptiveSize = 0.5, DetectHardEdges = true }) switch {
                Mesh result when result.IsValid => Fin.Succ(new RemeshResult(Mesh: result, Receipt: RemeshReceiptOf(kind: quad, source: state.Space.Native, output: result, targetLength: Some(quad.TargetLength.Value), desiredPolygonCount: Option<int>.None, hardEdges: true))),
                _ => Fin.Fail<RemeshResult>(error: state.Key.InvalidResult()),
            },
            simplifyCase: static (state, simplify) => state.Space.Native.DuplicateMesh() switch {
                Mesh clone when clone.Reduce(parameters: simplify.Parameters) && clone.IsValid => Fin.Succ(new RemeshResult(Mesh: clone, Receipt: RemeshReceiptOf(kind: simplify, source: state.Space.Native, output: clone, targetLength: Option<double>.None, desiredPolygonCount: Some(simplify.Parameters.DesiredPolygonCount), hardEdges: false))),
                _ => Fin.Fail<RemeshResult>(error: state.Key.InvalidResult()),
            }));
    private static RemeshReceipt RemeshReceiptOf(RemeshKind kind, Mesh source, Mesh output, Option<double> targetLength, Option<int> desiredPolygonCount, bool hardEdges) =>
        new(Kind: kind, Status: RemeshStatus.Completed, TargetLength: targetLength, DesiredPolygonCount: desiredPolygonCount, PreVertexCount: source.Vertices.Count, PreFaceCount: source.Faces.Count, PostVertexCount: output.Vertices.Count, PostFaceCount: output.Faces.Count, ReductionRatio: source.Faces.Count == 0 ? 0.0 : (double)output.Faces.Count / source.Faces.Count, Valid: output.IsValid, HardEdgePreservationRequested: hardEdges, TopologyChanged: source.Vertices.Count != output.Vertices.Count || source.Faces.Count != output.Faces.Count);

    // --- [DESCRIPTORS] ----------------------------------------------------------------------
    // SpectralFilter owns HKS-like heat, unnormalized WKS-style wave, biharmonic, diffusion, commute-time, and identity descriptors.
    internal static Fin<TOut> DescribeShape<TOut>(MeshSpace space, MeshDescriptor kind, int eigenpairs, Op key) =>
        Optional(kind).ToFin(key.InvalidInput()).Bind(active =>
            guard(active.IsValid, key.InvalidInput()).Bind(_ => active.Switch(
                state: (Space: space, Eigenpairs: eigenpairs, Key: key),
                spectralCase: static (state, spec) =>
                    from descriptor in DescribeSpectralShape(space: state.Space, spec: spec, eigenpairs: state.Eigenpairs, key: state.Key)
                    from output in ProjectDescriptor<TOut>(descriptor: descriptor, key: state.Key)
                    select output)));
    internal static Fin<DescriptorResult> DescribeSpectralShape(MeshSpace space, MeshDescriptor.SpectralCase spec, int eigenpairs, Op key) =>
        from bundle in space.Cache.SpectralBasisBundleOf(k: eigenpairs, key: key)
        from spectral in spec.Filter.ApplyDetailed(basis: bundle.Basis, sources: spec.Sources, key: key)
        select new DescriptorResult(Values: spectral.Values, Receipt: new DescriptorReceipt(Spectral: spectral.Receipt, Eigen: bundle.Eigen, RequestedEigenpairs: eigenpairs, ReturnedEigenpairs: bundle.Eigen.ReturnedPairs, ComparisonReady: spectral.Receipt.ComparisonReady, SpectralCacheHit: bundle.CacheHit, SkippedDegenerateFaces: bundle.SkippedDegenerateFaces, FactorNonZeros: bundle.FactorNonZeros));
    private static Fin<TOut> ProjectDescriptor<TOut>(DescriptorResult descriptor, Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(DescriptorResult) => Fin.Succ((TOut)(object)descriptor),
            Type t when t == typeof(DescriptorReceipt) => Fin.Succ((TOut)(object)descriptor.Receipt),
            Type t when t == typeof(SpectralDescriptor) => Fin.Succ((TOut)(object)new SpectralDescriptor(Values: descriptor.Values, Receipt: descriptor.Receipt.Spectral)),
            Type t when t == typeof(SpectralDescriptorReceipt) => Fin.Succ((TOut)(object)descriptor.Receipt.Spectral),
            Type t when t == typeof(Arr<double>) => Fin.Succ((TOut)(object)descriptor.Values),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(MeshDescriptor.SpectralCase), outputType: typeof(TOut))),
        };

    // --- [SEGMENTATION] ---------------------------------------------------------------------
    internal static Fin<TOut> Segment<TOut>(MeshSpace space, MeshSegmentation kind, Op key) =>
        Optional(kind).ToFin(key.InvalidInput()).Bind(active =>
            active.Switch(
                state: (Space: space, Key: key),
                scalarThresholdCase: static (state, threshold) => SegmentThreshold(space: state.Space, kind: threshold, key: state.Key),
                scalarBandsCase: static (state, bands) => SegmentBands(space: state.Space, kind: bands, key: state.Key),
                seededRegionGrowCase: static (state, grow) => SegmentRegionGrow(space: state.Space, kind: grow, key: state.Key),
                descriptorClustersCase: static (state, clusters) => SegmentDescriptorClusters(space: state.Space, kind: clusters, key: state.Key))
            .Bind(result => ProjectSegmentation<TOut>(result: result, key: key)));
    private static Fin<MeshSegmentationResult> SegmentThreshold(MeshSpace space, MeshSegmentation.ScalarThresholdCase kind, Op key) =>
        from scalars in SegmentationScalarsOf(mesh: space.Native, values: kind.Values, valuesAreVertices: kind.ValuesAreVertices, key: key)
        select SegmentationComponentsOf(mesh: space.Native, scalars: scalars, bucket: value => (kind.IncludeAbove ? value >= kind.Threshold : value <= kind.Threshold) ? 0 : UnassignedRegion, run: new SegmentationRun(Algorithm: MeshSegmentationAlgorithm.ScalarThresholdComponents, RequestedRegionCount: 1, SeedCount: 0, Status: MeshSegmentationStatus.Completed, Iterations: Option<int>.None, MaxIterations: Option<int>.None, Tolerance: Option<double>.None, Threshold: Some(kind.Threshold), Descriptor: Option<DescriptorReceipt>.None));
    private static Fin<MeshSegmentationResult> SegmentBands(MeshSpace space, MeshSegmentation.ScalarBandsCase kind, Op key) =>
        from scalars in SegmentationScalarsOf(mesh: space.Native, values: kind.Values, valuesAreVertices: kind.ValuesAreVertices, key: key)
        from _ in scalars.FiniteCount == 0 ? Fin.Fail<Unit>(key.InvalidInput()) : Fin.Succ(unit)
        select SegmentationComponentsOf(mesh: space.Native, scalars: scalars, bucket: value => BandIndexOf(value: value, min: scalars.Min, max: scalars.Max, count: kind.BandCount.Value), run: new SegmentationRun(Algorithm: MeshSegmentationAlgorithm.ScalarBandComponents, RequestedRegionCount: kind.BandCount.Value, SeedCount: 0, Status: MeshSegmentationStatus.Completed, Iterations: Option<int>.None, MaxIterations: Option<int>.None, Tolerance: Option<double>.None, Threshold: Option<double>.None, Descriptor: Option<DescriptorReceipt>.None));
    private static Fin<MeshSegmentationResult> SegmentRegionGrow(MeshSpace space, MeshSegmentation.SeededRegionGrowCase kind, Op key) =>
        from scalars in SegmentationScalarsOf(mesh: space.Native, values: kind.Values, valuesAreVertices: kind.ValuesAreVertices, key: key)
        from labels in RegionGrowLabels(mesh: space.Native, scalars: scalars.FaceValues, seeds: kind.SeedFaces, tolerance: kind.Tolerance.Value, maxIterations: kind.MaxIterations.Value, key: key)
        select SegmentationResultOf(mesh: space.Native, faceRegions: labels.Regions, scalars: scalars, run: new SegmentationRun(Algorithm: MeshSegmentationAlgorithm.SeededRegionGrow, RequestedRegionCount: labels.SeedCount, SeedCount: labels.SeedCount, Status: labels.Exhausted ? MeshSegmentationStatus.MaxIterationsExhausted : MeshSegmentationStatus.Completed, Iterations: Some(labels.Iterations), MaxIterations: Some(kind.MaxIterations.Value), Tolerance: Some(kind.Tolerance.Value), Threshold: Option<double>.None, Descriptor: Option<DescriptorReceipt>.None));
    private static Fin<MeshSegmentationResult> SegmentDescriptorClusters(MeshSpace space, MeshSegmentation.DescriptorClustersCase kind, Op key) =>
        kind.Descriptor is MeshDescriptor.SpectralCase spectral
            ? from descriptor in DescribeSpectralShape(space: space, spec: spectral, eigenpairs: kind.Eigenpairs.Value, key: key)
              from scalars in SegmentationScalarsOf(mesh: space.Native, values: descriptor.Values, valuesAreVertices: true, key: key)
              from clusters in ClusterLabels(values: scalars.FaceValues, count: kind.RegionCount.Value, maxIterations: kind.MaxIterations.Value, tolerance: kind.Tolerance.Value, key: key)
              let labels = ConnectedComponents(mesh: space.Native, buckets: clusters.Labels)
              select SegmentationResultOf(mesh: space.Native, faceRegions: labels, scalars: scalars, run: new SegmentationRun(Algorithm: MeshSegmentationAlgorithm.DescriptorScalarClusters, RequestedRegionCount: kind.RegionCount.Value, SeedCount: 0, Status: clusters.Converged ? MeshSegmentationStatus.Completed : MeshSegmentationStatus.MaxIterationsExhausted, Iterations: Some(clusters.Iterations), MaxIterations: Some(kind.MaxIterations.Value), Tolerance: Some(kind.Tolerance.Value), Threshold: Option<double>.None, Descriptor: Some(descriptor.Receipt)))
            : Fin.Fail<MeshSegmentationResult>(key.Unsupported(geometryType: kind.Descriptor.GetType(), outputType: typeof(MeshSegmentationResult)));
    private static Fin<TOut> ProjectSegmentation<TOut>(MeshSegmentationResult result, Op key) => typeof(TOut) switch { Type t when t == typeof(MeshSegmentationResult) => Fin.Succ((TOut)(object)result), Type t when t == typeof(MeshSegmentationReceipt) => Fin.Succ((TOut)(object)result.Receipt), Type t when t == typeof(Arr<int>) => Fin.Succ((TOut)(object)result.FaceRegions), _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(MeshSegmentation), outputType: typeof(TOut))) };
    private static MeshSegmentationResult SegmentationComponentsOf(Mesh mesh, SegmentationScalars scalars, Func<double, int> bucket, SegmentationRun run) =>
        SegmentationResultOf(mesh: mesh, faceRegions: ConnectedComponents(mesh: mesh, buckets: BucketsOf(values: scalars.FaceValues, bucket: bucket)), scalars: scalars, run: run);
    private static MeshSegmentationResult SegmentationResultOf(Mesh mesh, int[] faceRegions, SegmentationScalars scalars, SegmentationRun run) {
        Arr<int> faces = new(faceRegions);
        int assigned = faceRegions.Count(static label => label >= 0);
        int regionCount = faceRegions.Where(static label => label >= 0).Distinct().Count();
        MeshSegmentationReceipt receipt = new(Algorithm: run.Algorithm, Status: run.Status, RequestedRegionCount: run.RequestedRegionCount, RegionCount: regionCount, SeedCount: run.SeedCount, AssignedFaceCount: assigned, UnassignedFaceCount: faceRegions.Length - assigned, SkippedDegenerateFaces: scalars.SkippedDegenerateFaces, SkippedNonFiniteValues: scalars.SkippedNonFiniteValues, Iterations: run.Iterations, MaxIterations: run.MaxIterations, Tolerance: run.Tolerance, Threshold: run.Threshold, Descriptor: run.Descriptor, Solve: Option<SolveReceipt>.None, SpectralCacheHit: run.Descriptor.Map(static receipt => receipt.SpectralCacheHit), FactorCacheHit: Option<bool>.None, FactorNonZeros: run.Descriptor.Bind(static receipt => receipt.FactorNonZeros));
        return new MeshSegmentationResult(FaceRegions: faces, VertexRegions: VertexRegionsOf(mesh: mesh, faceRegions: faceRegions), Receipt: receipt);
    }
    private static Fin<SegmentationScalars> SegmentationScalarsOf(Mesh mesh, Arr<double> values, bool valuesAreVertices, Op key) =>
        values.Count == (valuesAreVertices ? mesh.Vertices.Count : mesh.Faces.Count)
            ? Fin.Succ(FaceScalarsOf(mesh: mesh, values: values, valuesAreVertices: valuesAreVertices))
            : Fin.Fail<SegmentationScalars>(key.InvalidInput());
    private static SegmentationScalars FaceScalarsOf(Mesh mesh, Arr<double> values, bool valuesAreVertices) {
        double[] faceValues = new double[mesh.Faces.Count];
        System.Array.Fill(array: faceValues, value: double.NaN);
        int skippedDegenerate = 0, skippedNonFinite = 0, finite = 0;
        double min = double.PositiveInfinity, max = double.NegativeInfinity;
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            Point3d a = mesh.Vertices[index: face.A], b = mesh.Vertices[index: face.B], c = mesh.Vertices[index: face.C];
            double area = 0.5 * Vector3d.CrossProduct(a: b - a, b: c - a).Length;
            if ((face.IsTriangle ? area : area + (0.5 * Vector3d.CrossProduct(a: c - a, b: mesh.Vertices[index: face.D] - a).Length)) < DegenerateTriangleArea) { skippedDegenerate++; continue; }
            double value = valuesAreVertices ? (values[index: face.A] + values[index: face.B] + values[index: face.C] + (face.IsQuad ? values[index: face.D] : 0.0)) / (face.IsQuad ? 4.0 : 3.0) : values[index: f];
            if (!RhinoMath.IsValidDouble(x: value)) { skippedNonFinite++; continue; }
            faceValues[f] = value; min = Math.Min(val1: min, val2: value); max = Math.Max(val1: max, val2: value); finite++;
        }
        return new SegmentationScalars(FaceValues: new Arr<double>(faceValues), SkippedDegenerateFaces: skippedDegenerate, SkippedNonFiniteValues: skippedNonFinite, FiniteCount: finite, Min: min, Max: max);
    }
    private static int BandIndexOf(double value, double min, double max, int count) =>
        !RhinoMath.IsValidDouble(x: value) ? UnassignedRegion : Math.Abs(value: max - min) <= RhinoMath.SqrtEpsilon ? 0 : Math.Min(val1: count - 1, val2: Math.Max(val1: 0, val2: (int)Math.Floor(d: (value - min) / ((max - min) / count))));
    private static int[] BucketsOf(Arr<double> values, Func<double, int> bucket) =>
        [.. values.AsIterable().Select(value => RhinoMath.IsValidDouble(x: value) ? bucket(arg: value) : UnassignedRegion)];
    private static int[] ConnectedComponents(Mesh mesh, int[] buckets) {
        int[] regions = [.. Enumerable.Repeat(element: UnassignedRegion, count: mesh.Faces.Count)];
        int[][] adjacency = FaceAdjacencyOf(mesh: mesh);
        int region = 0;
        for (int start = 0; start < buckets.Length; start++) {
            if (buckets[start] < 0 || regions[start] >= 0) continue;
            Queue<int> queue = new(); queue.Enqueue(item: start); regions[start] = region;
            while (queue.Count > 0) {
                int face = queue.Dequeue();
                for (int n = 0; n < adjacency[face].Length; n++) {
                    int next = adjacency[face][n];
                    if (next < 0 || next >= buckets.Length || regions[next] >= 0 || buckets[next] != buckets[start]) continue;
                    regions[next] = region; queue.Enqueue(item: next);
                }
            }
            region++;
        }
        return regions;
    }
    private static int[][] FaceAdjacencyOf(Mesh mesh) {
        List<int>[] adjacency = [.. Enumerable.Range(start: 0, count: mesh.Faces.Count).Select(static _ => new List<int>())];
        for (int edge = 0; edge < mesh.TopologyEdges.Count; edge++) {
            int[] faces = mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: edge);
            for (int a = 0; a < faces.Length; a++)
                for (int b = a + 1; b < faces.Length; b++) {
                    adjacency[faces[a]].Add(item: faces[b]); adjacency[faces[b]].Add(item: faces[a]);
                }
        }
        return [.. adjacency.Select(static faces => faces.Distinct().OrderBy(static f => f).ToArray())];
    }
    private static Fin<(int[] Regions, int Iterations, bool Exhausted, int SeedCount)> RegionGrowLabels(Mesh mesh, Arr<double> scalars, Seq<int> seeds, double tolerance, int maxIterations, Op key) {
        int faceCount = mesh.Faces.Count;
        int[] seedArray = [.. seeds.AsIterable()];
        if (seedArray.Any(seed => seed < 0 || seed >= faceCount || !RhinoMath.IsValidDouble(x: scalars[index: seed]))) return Fin.Fail<(int[], int, bool, int)>(key.InvalidInput());
        int[] regions = [.. Enumerable.Repeat(element: UnassignedRegion, count: faceCount)];
        int[][] adjacency = FaceAdjacencyOf(mesh: mesh);
        List<double> anchors = new(capacity: seedArray.Length);
        for (int s = 0; s < seedArray.Length; s++)
            if (regions[seedArray[s]] < 0) {
                regions[seedArray[s]] = anchors.Count;
                anchors.Add(item: scalars[index: seedArray[s]]);
            }
        if (anchors.Count == 0) return Fin.Fail<(int[], int, bool, int)>(key.InvalidInput());
        int iterations = 0;
        bool HasCandidates() {
            for (int face = 0; face < faceCount; face++)
                if (regions[face] >= 0)
                    for (int i = 0; i < adjacency[face].Length; i++) {
                        int next = adjacency[face][i]; double value = scalars[index: next];
                        if (regions[next] < 0 && RhinoMath.IsValidDouble(x: value) && Math.Abs(value: value - anchors[index: regions[face]]) <= tolerance) return true;
                    }
            return false;
        }
        while (iterations < maxIterations) {
            int[] proposalRegion = [.. Enumerable.Repeat(element: UnassignedRegion, count: faceCount)];
            int[] proposalSource = [.. Enumerable.Repeat(element: int.MaxValue, count: faceCount)];
            for (int face = 0; face < faceCount; face++) {
                int region = regions[face];
                if (region < 0) continue;
                for (int i = 0; i < adjacency[face].Length; i++) {
                    int next = adjacency[face][i]; double value = scalars[index: next];
                    if (regions[next] >= 0 || !RhinoMath.IsValidDouble(x: value) || Math.Abs(value: value - anchors[index: region]) > tolerance) continue;
                    if (proposalRegion[next] < 0 || region < proposalRegion[next] || (region == proposalRegion[next] && face < proposalSource[next])) {
                        proposalRegion[next] = region; proposalSource[next] = face;
                    }
                }
            }
            bool changed = false;
            for (int face = 0; face < faceCount; face++)
                if (proposalRegion[face] >= 0) { regions[face] = proposalRegion[face]; changed = true; }
            if (!changed) return Fin.Succ((regions, iterations, false, anchors.Count));
            iterations++;
        }
        return Fin.Succ((regions, iterations, HasCandidates(), anchors.Count));
    }
    private static Fin<ClusterState> ClusterLabels(Arr<double> values, int count, int maxIterations, double tolerance, Op key) {
        int[] valid = [.. Enumerable.Range(start: 0, count: values.Count).Where(i => RhinoMath.IsValidDouble(x: values[index: i]))];
        if (valid.Length < count) return Fin.Fail<ClusterState>(key.InvalidInput());
        double[] centers = new double[count];
        centers[0] = valid.Min(i => values[index: i]);
        for (int c = 1; c < count; c++) {
            double bestValue = centers[0], bestDistance = double.NegativeInfinity;
            for (int i = 0; i < valid.Length; i++) {
                double value = values[index: valid[i]], nearest = double.PositiveInfinity;
                for (int j = 0; j < c; j++) nearest = Math.Min(val1: nearest, val2: Math.Abs(value: value - centers[j]));
                if (nearest > bestDistance || (Math.Abs(value: nearest - bestDistance) <= RhinoMath.SqrtEpsilon && value < bestValue)) { bestDistance = nearest; bestValue = value; }
            }
            centers[c] = bestValue;
        }
        int[] labels = [.. Enumerable.Repeat(element: UnassignedRegion, count: values.Count)];
        bool converged = false;
        int iteration = 0;
        while (iteration < maxIterations && !converged) {
            double[] sums = new double[count], next = new double[count];
            int[] counts = new int[count];
            for (int i = 0; i < valid.Length; i++) {
                double value = values[index: valid[i]];
                int nearest = 0;
                double best = Math.Abs(value: value - centers[0]);
                for (int c = 1; c < count; c++) {
                    double distance = Math.Abs(value: value - centers[c]);
                    if (distance < best) { best = distance; nearest = c; }
                }
                labels[valid[i]] = nearest; sums[nearest] += value; counts[nearest]++;
            }
            double shift = 0.0;
            for (int c = 0; c < count; c++) {
                next[c] = counts[c] > 0 ? sums[c] / counts[c] : centers[c];
                shift = Math.Max(val1: shift, val2: Math.Abs(value: next[c] - centers[c]));
            }
            centers = next; converged = shift <= tolerance; iteration++;
        }
        return labels.Any(static label => label >= 0)
            ? Fin.Succ(new ClusterState(Labels: labels, Iterations: iteration, Converged: converged))
            : Fin.Fail<ClusterState>(key.InvalidResult());
    }
    private static Arr<int> VertexRegionsOf(Mesh mesh, int[] faceRegions) {
        List<int>[] mutable = [.. Enumerable.Range(start: 0, count: mesh.Vertices.Count).Select(static _ => new List<int>())];
        for (int f = 0; f < mesh.Faces.Count; f++) {
            int region = faceRegions[f];
            if (region < 0) continue;
            MeshFace face = mesh.Faces[index: f];
            mutable[face.A].Add(item: region); mutable[face.B].Add(item: region); mutable[face.C].Add(item: region);
            if (face.IsQuad) mutable[face.D].Add(item: region);
        }
        return new Arr<int>([.. mutable.Select(static regions => regions.Count == 0 ? UnassignedRegion : regions.GroupBy(static r => r).OrderByDescending(static g => g.Count()).ThenBy(static g => g.Key).First().Key)]);
    }

    internal static Fin<double> SpectralDistanceAt(MeshSpace space, SpectralFilter filter, Seq<int> sources, int pairs, Point3d sample, Op key) =>
        from bundle in space.Cache.SpectralBasisBundleOf(k: pairs, key: key)
        from descriptor in filter.ApplyDetailed(basis: bundle.Basis, sources: sources.IsEmpty ? Option<Seq<int>>.None : Some(sources), key: key)
        from interpolated in InterpolateOnMesh(space: space, sample: sample, perVertex: descriptor.Values, key: key)
        select interpolated;

    // --- [HODGE_DECOMPOSITION] -------------------------------------------------------------
    // Hodge decomposition solves a pinned potential, then splits irrotational and solenoidal parts.
    internal sealed record HodgeBundle(Arr<Vector3d> Irrotational, Arr<Vector3d> Solenoidal);
    internal static Fin<Vector3d> HodgeProjectedAt(VectorField source, MeshSpace space, BoundarySense sense, Point3d sample, Op key) =>
        from bundle in space.Cache.Hodge(probe: new HodgeKey(Source: source),
            compute: () => ComputeHodgeBundle(source: source, space: space, key: key))
        from value in InterpolateVectorOnMesh(space: space, sample: sample, perVertex: sense.Equals(BoundarySense.Toward) ? bundle.Irrotational : bundle.Solenoidal, key: key)
        select value;
    private static Fin<HodgeBundle> ComputeHodgeBundle(VectorField source, MeshSpace space, Op key) {
        Mesh mesh = space.Native;
        int nVerts = mesh.Vertices.Count;
        int nEdges = mesh.TopologyEdges.Count;
        double[] negDivergence = new double[nVerts];
        return toSeq(Enumerable.Range(start: 0, count: nEdges)).Fold(
            initialState: Fin.Succ(unit),
            f: (acc, e) => acc.Bind(_ => {
                Line line = mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: e);
                return line.IsValid switch {
                    false => Fin.Succ(unit),
                    true => line.Direction switch {
                        Vector3d tangent when tangent.Unitize() => source.SampleVector(sample: (line.From + line.To) * 0.5, context: space.Tolerance, key: key)
                            .Bind(sampled => key.AcceptValue(value: sampled * tangent * line.Length))
                            .Map(value => {
                                IndexPair pair = mesh.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: e);
                                int lo = Math.Min(val1: pair.I, val2: pair.J);
                                int hi = Math.Max(val1: pair.I, val2: pair.J);
                                negDivergence[lo] += value;
                                negDivergence[hi] -= value;
                                return unit;
                            }),
                        _ => Fin.Succ(unit),
                    },
                };
            }))
            .Bind(_ => space.Laplacian(kind: MeshLaplacian.Cotangent, key: key))
            .Bind(L => SolvePinnedPoisson(stiffness: L.Stiffness, rhs: new Arr<double>(negDivergence), key: key))
            .Bind(potential => BuildHodgeFromPotential(mesh: mesh, source: source, potential: potential, space: space, key: key));
    }
    // Pinning removes the constant Laplacian null mode.
    private static Fin<Arr<double>> SolvePinnedPoisson(SparseMatrix stiffness, Arr<double> rhs, Op key) {
        int n = stiffness.Rows.Value;
        List<(int Row, int Col, double Value)> triplets = SparseTripletsOf(matrix: stiffness, capacityBonus: 1);
        triplets.Add(item: (0, 0, 1.0e10));
        Dimension dim = Dimension.Create(value: n);
        return from pinned in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: triplets, key: key)
               from potential in pinned.Solve(rhs: rhs, key: key)
               select potential;
    }
    private static Fin<HodgeBundle> BuildHodgeFromPotential(Mesh mesh, VectorField source, Arr<double> potential, MeshSpace space, Op key) {
        int nVerts = mesh.Vertices.Count;
        Vector3d[] irrotPerVertex = new Vector3d[nVerts];
        double[] faceTally = new double[nVerts];
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d pa = mesh.Vertices[index: face.A]; Point3d pb = mesh.Vertices[index: face.B]; Point3d pc = mesh.Vertices[index: face.C];
            Vector3d ab = pb - pa; Vector3d ac = pc - pa;
            Vector3d n = Vector3d.CrossProduct(a: ab, b: ac);
            double twoArea = n.Length;
            if (twoArea < RhinoMath.ZeroTolerance) continue;
            Vector3d nUnit = n / twoArea;
            Vector3d grad =
                (potential[index: face.A] * Vector3d.CrossProduct(a: nUnit, b: pc - pb) / twoArea) +
                (potential[index: face.B] * Vector3d.CrossProduct(a: nUnit, b: pa - pc) / twoArea) +
                (potential[index: face.C] * Vector3d.CrossProduct(a: nUnit, b: pb - pa) / twoArea);
            irrotPerVertex[face.A] += grad; faceTally[face.A]++;
            irrotPerVertex[face.B] += grad; faceTally[face.B]++;
            irrotPerVertex[face.C] += grad; faceTally[face.C]++;
        }
        for (int v = 0; v < nVerts; v++)
            if (faceTally[v] > 0.0) irrotPerVertex[v] /= faceTally[v];
        return toSeq(Enumerable.Range(start: 0, count: nVerts)).TraverseM(v =>
            source.SampleVector(sample: mesh.Vertices[index: v], context: space.Tolerance, key: key)
                .Map(sampled => sampled - irrotPerVertex[v])).As()
            .Map(solenoid => new HodgeBundle(
                Irrotational: new Arr<Vector3d>(irrotPerVertex),
                Solenoidal: new Arr<Vector3d>([.. solenoid.AsIterable()])));
    }
    private static Fin<Vector3d> InterpolateVectorOnMesh(MeshSpace space, Point3d sample, Arr<Vector3d> perVertex, Op key) =>
        ClosestFace(space: space, sample: sample, key: key, project: (_, face, weights, _) => key.AcceptValue(value:
            BarycentricVector(face: face, weights: weights, at: vertex => perVertex[index: vertex])));

    // --- [VECTOR_HEAT] ----------------------------------------------------------------------
    // Vector heat uses connection direction, scalar magnitude, and scalar indicator solves.
    internal static Fin<Vector3d> VectorHeatAt(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Point3d sample, Op key) =>
        from cached in EnsureVectorHeat(space: space, sources: sources, time: time, key: key)
        from value in BarycentricBlend(space: space, sample: sample, perVertex: cached, key: key,
            decode: static (value, x, y) => (value.Real * x) + (value.Imaginary * y))
        select value;
    private static Fin<Complex[]> EnsureVectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op key) {
        int n = space.Native.Vertices.Count;
        Seq<(int Vertex, Vector3d Direction)> ordered = toSeq(sources.AsIterable()
            .OrderBy(static s => s.Vertex)
            .ThenBy(static s => s.Direction.X)
            .ThenBy(static s => s.Direction.Y)
            .ThenBy(static s => s.Direction.Z));
        return ordered.IsEmpty || !RhinoMath.IsValidDouble(x: time) || time <= 0.0 || ordered.Exists(s => s.Vertex < 0 || s.Vertex >= n || !s.Direction.IsValid || s.Direction.IsTiny())
            ? Fin.Fail<Complex[]>(key.InvalidInput())
            : space.Cache.VectorHeat(probe: new VectorHeatKey(Time: time, Sources: ordered),
                compute: () => ComputeVectorHeat(space: space, sources: ordered, time: time, key: key));
    }
    private static Fin<Complex[]> ComputeVectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op key) {
        int n = space.Native.Vertices.Count;
        FrameBundle frames = EnsureVertexFrames(mesh: space.Native);
        return from imesh in space.Cache.IntrinsicMeshSnapshot
               from _ in guard(!imesh.HasFlips, key.Unsupported(geometryType: typeof(IntrinsicMesh), outputType: typeof(Complex[])))
               from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
               from connectionFactor in space.Cache.ConnectionCholesky(symmetry: 1, time: time, edgeAdjustment: Option<Arr<double>>.None, key: key)
               from scalarFactor in space.Cache.ScalarHeatCholesky(time: time, key: key)
               let rhs = EncodeVectorHeatSources(n: n, sources: sources, frames: frames, mass: laplacian.MassLumped)
               from direction in connectionFactor.Solve(rhs: rhs.StackedDirection, key: key)
               from magnitude in scalarFactor.Solve(rhs: rhs.Magnitude, key: key)
               from indicator in scalarFactor.Solve(rhs: rhs.Indicator, key: key)
               select RecoverVectorHeat(n: n, direction: direction, magnitude: magnitude, indicator: indicator);
    }
    // Mass weighting matches the scalar heat-method source convention.
    private sealed record VectorHeatRhs(Arr<double> StackedDirection, Arr<double> Magnitude, Arr<double> Indicator);
    private static VectorHeatRhs EncodeVectorHeatSources(int n, Seq<(int Vertex, Vector3d Direction)> sources, FrameBundle frames, Arr<double> mass) {
        double[] stacked = new double[2 * n];
        double[] magnitude = new double[n];
        double[] indicator = new double[n];
        for (int s = 0; s < sources.Count; s++) {
            (int v, Vector3d direction) = sources[index: s];
            if (v < 0 || v >= n) continue;
            double mv = mass[index: v];
            WriteStackedComplex(target: stacked, n: n, vertex: v, value: mv * TangentComplex(direction: direction, frames: frames, vertex: v));
            magnitude[v] += mv * direction.Length;
            indicator[v] += mv;
        }
        return new VectorHeatRhs(StackedDirection: new Arr<double>(stacked), Magnitude: new Arr<double>(magnitude), Indicator: new Arr<double>(indicator));
    }
    private static Complex TangentComplex(Vector3d direction, FrameBundle frames, int vertex) =>
        (direction - (direction * frames.N[vertex] * frames.N[vertex])) switch {
            Vector3d projected => new(real: projected * frames.X[vertex], imaginary: projected * frames.Y[vertex]),
        };
    private static void WriteStackedComplex(double[] target, int n, int vertex, Complex value) {
        target[vertex] += value.Real;
        target[vertex + n] += value.Imaginary;
    }
    private static Complex[] RecoverVectorHeat(int n, Arr<double> direction, Arr<double> magnitude, Arr<double> indicator) {
        Complex[] result = new Complex[n];
        for (int v = 0; v < n; v++) {
            Complex raw = new(real: direction[index: v], imaginary: direction[index: v + n]);
            double mag = raw.Magnitude;
            Complex unit = mag > RhinoMath.SqrtEpsilon ? raw / mag : Complex.Zero;
            double ind = indicator[index: v];
            double scale = ind > RhinoMath.SqrtEpsilon ? magnitude[index: v] / ind : 0.0;
            result[v] = unit * scale;
        }
        return result;
    }

    // --- [GEODESIC_TANGENT] ------------------------------------------------------------------
    // Geodesic tangent samples the precomputed heat-distance gradient.
    internal static Fin<Vector3d> GeodesicTangentAt(MeshSpace space, Seq<int> sources, Point3d sample, Op key) =>
        from distances in EnsureGeodesicDistances(space: space, sources: sources, key: key)
        from tangent in EvaluateGeodesicTangentAt(space: space, distances: distances, sample: sample, key: key)
        select tangent;
    private static Fin<Vector3d> EvaluateGeodesicTangentAt(MeshSpace space, Arr<double> distances, Point3d sample, Op key) {
        Vector3d[] gradients = SpectralCore.ComputeTriangleGradients(mesh: space.Native, u: distances);
        return ClosestFace(space: space, sample: sample, key: key, project: (mesh, face, _, faceIndex) => {
            if (!face.IsTriangle) return Fin.Fail<Vector3d>(error: key.InvalidResult());
            double twoArea = Vector3d.CrossProduct(a: mesh.Vertices[index: face.B] - mesh.Vertices[index: face.A], b: mesh.Vertices[index: face.C] - mesh.Vertices[index: face.A]).Length;
            return twoArea < RhinoMath.ZeroTolerance ? Fin.Fail<Vector3d>(error: key.InvalidResult()) : key.AcceptValue(value: gradients[faceIndex]);
        });
    }

    // --- [STRIPE_PATTERN] --------------------------------------------------------------------
    // Stripes expose cross-field-aligned level-set scalars.
    internal static Fin<double> StripeAt(MeshSpace space, VectorField crossField, double frequency, Point3d sample, Op key) =>
        from cross in crossField.SampleVector(sample: sample, context: space.Tolerance, key: key)
        from output in ComputeStripeValue(space: space, crossSample: cross, sample: sample, frequency: frequency, key: key)
        select output;
    private static Fin<double> ComputeStripeValue(MeshSpace space, Vector3d crossSample, Point3d sample, double frequency, Op key) {
        FrameBundle fb = EnsureVertexFrames(mesh: space.Native);
        return ClosestFace(space: space, sample: sample, key: key, project: (_, face, weights, _) => {
            Vector3d frameX = BarycentricVector(face: face, weights: weights, at: vertex => fb.X[vertex]);
            Vector3d frameY = BarycentricVector(face: face, weights: weights, at: vertex => fb.Y[vertex]);
            _ = frameX.Unitize(); _ = frameY.Unitize();
            double angle = Math.Atan2(y: crossSample * frameY, x: crossSample * frameX);
            return key.AcceptValue(value: Math.Cos(d: frequency * angle));
        });
    }
    private static Vector3d BarycentricVector(MeshFace face, double[] weights, Func<int, Vector3d> at) =>
        (weights[0] * at(face.A)) + (weights[1] * at(face.B)) + (weights[2] * at(face.C)) + (face.IsQuad ? weights[3] * at(face.D) : Vector3d.Zero);

    // --- [SDF_FROM_MESH] ---------------------------------------------------------------------
    internal static Fin<SdfMeshSample> SignedDistanceFromMeshDetailed(MeshSpace space, SdfMeshMethod method, Point3d sample, Op key) =>
        (method.Equals(SdfMeshMethod.BoundarySignedHeat)
            ? SignedHeatDistanceDetailed(space: space, sample: sample, key: key).Map(result => (result.Distance, Status: SdfMeshStatus.BoundarySourceSignedHeat, Solve: Some(result.Solve)))
            : GeneralizedWindingDistance(space: space, sample: sample, key: key).Map(distance => (Distance: distance, Status: SdfMeshStatus.Approximate, Solve: Option<SolveReceipt>.None)))
        .Map(result => new SdfMeshSample(Distance: result.Distance, Receipt: SdfMeshReceiptOf(space: space, method: method, status: result.Status, solve: result.Solve)));
    private static SdfMeshReceipt SdfMeshReceiptOf(MeshSpace space, SdfMeshMethod method, SdfMeshStatus status, Option<SolveReceipt> solve) {
        Mesh mesh = space.Native;
        bool manifold = mesh.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out _);
        (int boundaryComponents, int nonManifoldEdges) = TopologyEdgeStatsOf(mesh: mesh);
        return new SdfMeshReceipt(Method: method, Status: status, IsSolid: mesh.IsSolid, IsManifold: manifold, IsOriented: oriented, BoundaryComponents: boundaryComponents, NonManifoldEdges: nonManifoldEdges, UsesGeneralizedWindingApproximation: method.Equals(SdfMeshMethod.GeneralizedWindingNumber), UsesBoundarySignedHeat: method.Equals(SdfMeshMethod.BoundarySignedHeat), Solve: solve);
    }
    private static Fin<double> GeneralizedWindingDistance(MeshSpace space, Point3d sample, Op key) =>
        Optional(space.Native.ClosestPoint(testPoint: sample)).Filter(static closest => closest.IsValid).ToFin(key.InvalidResult()).Bind(closest =>
            SolidAngleWindingNumber(mesh: space.Native, sample: sample, key: key)
                .Bind(winding => key.AcceptValue(value: Math.Abs(value: winding) > 0.5 ? -sample.DistanceTo(other: closest) : sample.DistanceTo(other: closest))));
    private static Fin<double> SolidAngleWindingNumber(Mesh mesh, Point3d sample, Op key) {
        double solidAngle = 0.0;
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            Point3d a = mesh.Vertices[index: face.A];
            Point3d b = mesh.Vertices[index: face.B];
            Point3d c = mesh.Vertices[index: face.C];
            solidAngle += TriangleSolidAngle(a: a, b: b, c: c, sample: sample);
            if (face.IsQuad) solidAngle += TriangleSolidAngle(a: a, b: c, c: mesh.Vertices[index: face.D], sample: sample);
        }
        return key.AcceptValue(value: solidAngle / (4.0 * Math.PI));
    }
    private static double TriangleSolidAngle(Point3d a, Point3d b, Point3d c, Point3d sample) {
        Vector3d va = a - sample; Vector3d vb = b - sample; Vector3d vc = c - sample;
        double la = va.Length; double lb = vb.Length; double lc = vc.Length;
        if (la <= RhinoMath.ZeroTolerance || lb <= RhinoMath.ZeroTolerance || lc <= RhinoMath.ZeroTolerance) return 0.0;
        double det = Vector3d.CrossProduct(a: va, b: vb) * vc;
        double lengthProduct = la * lb * lc;
        double abDot = va * vb;
        double bcDot = vb * vc;
        double caDot = vc * va;
        double denom = lengthProduct + (abDot * lc) + (bcDot * la) + (caDot * lb);
        return 2.0 * Math.Atan2(y: det, x: denom);
    }
    private static Fin<(double Distance, SolveReceipt Solve)> SignedHeatDistanceDetailed(MeshSpace space, Point3d sample, Op key) {
        Polyline[]? polylines = space.Native.GetNakedEdges();
        return polylines is null || polylines.Length == 0
            ? Fin.Fail<(double, SolveReceipt)>(key.Unsupported(geometryType: typeof(SdfMeshMethod), outputType: typeof((double, SolveReceipt))))
            : from phi in space.Cache.SignedHeatDetailed
              from signed in InterpolateOnMesh(space: space, sample: sample, perVertex: phi.Values, key: key)
              select (signed, phi.Solve);
    }
    // Boundary-source signed heat rejects flipped IDT until CR signpost transfer exists.
    internal static Fin<Arr<double>> ComputeSignedHeat(MeshSpace space, Op key) => ComputeSignedHeatDetailed(space: space, key: key).Map(static result => result.Values);
    internal static Fin<(Arr<double> Values, SolveReceipt Solve)> ComputeSignedHeatDetailed(MeshSpace space, Op key) {
        Mesh mesh = space.Native;
        Polyline[]? polylines = mesh.GetNakedEdges();
        if (polylines is null || polylines.Length == 0) return Fin.Fail<(Arr<double>, SolveReceipt)>(error: key.InvalidInput());
        double h = space.Cache.MeanEdgeLength;
        if (h <= RhinoMath.ZeroTolerance) return Fin.Fail<(Arr<double>, SolveReceipt)>(error: key.InvalidResult());
        double t = 0.5 * h * (0.5 * h);
        return from imesh in space.Cache.IntrinsicMeshSnapshot
               from _ in guard(!imesh.HasFlips, key.Unsupported(geometryType: typeof(IntrinsicMesh), outputType: typeof((Arr<double>, SolveReceipt))))
               let encoded = EncodeSignedHeatBoundarySource(mesh: mesh, imesh: imesh, polylines: polylines)
               from admitted in AdmitSignedHeatSource(encoded: encoded, key: key)
               from heatFactor in space.Cache.EdgeConnectionCholesky(time: t, key: key)
               from heatSolve in heatFactor.SolveDetailed(rhs: admitted.Rhs, key: key)
               let faceField = SpectralCore.SampleCrouzeixRaviartFaceField(mesh: mesh, imesh: imesh, stacked: heatSolve.Solution)
               let divergence = SpectralCore.ComputeIntrinsicVertexDivergence(mesh: mesh, imesh: imesh, faceFields: faceField)
               from poissonFactor in space.Cache.Cholesky
               from poissonSolve in poissonFactor.SolveDetailed(rhs: divergence, key: key)
               from shifted in ShiftSignedHeat(phi: poissonSolve.Solution, sourceVertices: admitted.SourceVertices, key: key)
               select (shifted, poissonSolve);
    }
    // Edge-as-source encoding stacks imaginary CR edge sources with Lo→Hi signs.
    private static (Arr<double> Rhs, Seq<int> SourceVertices) EncodeSignedHeatBoundarySource(Mesh mesh, IntrinsicMesh imesh, Polyline[] polylines) {
        int eCount = imesh.EdgeCount;
        double[] stacked = new double[2 * eCount];
        System.Collections.Generic.HashSet<int> sources = [];
        Point3dList vertices = [.. mesh.Vertices.ToPoint3dArray()];
        double tolSq = RhinoMath.ZeroTolerance * 1.0e2;
        tolSq *= tolSq;
        for (int p = 0; p < polylines.Length; p++) {
            Polyline poly = polylines[p];
            int prev = FindClosestVertex(vertices: vertices, target: poly[0], tolSq: tolSq);
            if (prev >= 0) _ = sources.Add(item: prev);
            for (int q = 1; q < poly.Count; q++) {
                int curr = FindClosestVertex(vertices: vertices, target: poly[q], tolSq: tolSq);
                if (curr >= 0) _ = sources.Add(item: curr);
                if (prev >= 0 && curr >= 0 && imesh.IndexOfEdge(lo: prev, hi: curr) is int e and >= 0) {
                    IntrinsicEdge edge = imesh.EdgeAt(index: e);
                    double sign = prev == edge.Lo ? 1.0 : -1.0;
                    stacked[e + eCount] += edge.Length * sign;
                }
                prev = curr;
            }
        }
        return (Rhs: new Arr<double>(stacked), SourceVertices: toSeq(sources));
    }
    private static Fin<(Arr<double> Rhs, Seq<int> SourceVertices)> AdmitSignedHeatSource((Arr<double> Rhs, Seq<int> SourceVertices) encoded, Op key) =>
        (encoded.SourceVertices.IsEmpty, encoded.Rhs.Fold(initialState: (Finite: true, Active: false), f: static (state, value) => (state.Finite && RhinoMath.IsValidDouble(x: value), state.Active || Math.Abs(value: value) > RhinoMath.ZeroTolerance))) switch {
            (false, (true, true)) => Fin.Succ(encoded),
            _ => Fin.Fail<(Arr<double>, Seq<int>)>(key.InvalidResult()),
        };
    private static int FindClosestVertex(Point3dList vertices, Point3d target, double tolSq) =>
        vertices.ClosestIndex(testPoint: target) switch {
            int best when best >= 0 && vertices[index: best].DistanceToSquared(other: target) <= tolSq => best,
            _ => -1,
        };
    private static Fin<Arr<double>> ShiftSignedHeat(Arr<double> phi, Seq<int> sourceVertices, Op key) =>
        sourceVertices.IsEmpty
            ? Fin.Fail<Arr<double>>(key.InvalidResult())
            : (sourceVertices.Fold(initialState: 0.0, f: (sum, source) => sum - phi[index: source]) / sourceVertices.Count) switch {
                double mean => Fin.Succ(new Arr<double>([.. phi.AsIterable().Select(value => -value - mean)])),
            };
}

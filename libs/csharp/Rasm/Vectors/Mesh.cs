using System.Numerics;
using Foundation.CSharp.Analyzers.Contracts;
using Rhino.Collections;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record MeshDescriptor {
    private MeshDescriptor() { }
    public sealed record SpectralCase(SpectralFilter Filter, Option<Seq<int>> Sources, SpectralDescriptorPolicy Policy) : MeshDescriptor;
    public static MeshDescriptor Spectral(SpectralFilter filter, Option<Seq<int>> sources = default) => Spectral(filter: filter, sources: sources, policy: SpectralDescriptorPolicy.Raw);
    public static MeshDescriptor Spectral(SpectralFilter filter, Option<Seq<int>> sources, SpectralDescriptorPolicy policy) => new SpectralCase(Filter: filter, Sources: sources, Policy: policy);
    internal bool IsValid => this is SpectralCase { Filter: not null } spectral && spectral.Policy.IsValid;
}

[SmartEnum<int>]
public sealed partial class MeshLaplacian {
    public static readonly MeshLaplacian Cotangent = new(key: 0, select: static (cache, key) => cache.Cotangent(key: key));
    public static readonly MeshLaplacian IntrinsicDelaunay = new(key: 1, select: static (cache, key) => cache.IntrinsicDelaunay(key: key));
    public static readonly MeshLaplacian TuftedIntrinsic = new(key: 2, select: static (cache, key) => cache.TuftedIntrinsic(key: key));
    [UseDelegateFromConstructor] internal partial Fin<SparseLaplacian> Select(LaplacianCache cache, Op key);
}

[SmartEnum<int>]
public sealed partial class MeshFeatureAlgorithm { public static readonly MeshFeatureAlgorithm DihedralProxy = new(key: 0); }

[SmartEnum<int>]
public sealed partial class MeshFeatureKind {
    public static readonly MeshFeatureKind Boundary = new(key: 0);
    public static readonly MeshFeatureKind Crease = new(key: 1);
    public static readonly MeshFeatureKind NonManifold = new(key: 2);
    public static readonly MeshFeatureKind Unwelded = new(key: 3);
    public static readonly MeshFeatureKind NgonInteriorSkipped = new(key: 4);
    public static readonly MeshFeatureKind Ridge = new(key: 5);
    public static readonly MeshFeatureKind Valley = new(key: 6);
    public static readonly MeshFeatureKind RegionBoundary = new(key: 7);
}

[SmartEnum<int>]
public sealed partial class MeshSamplingSpectrumAlgorithm { public static readonly MeshSamplingSpectrumAlgorithm CandidateSpectrum = new(key: 0); }

[Union]
public abstract partial record MeshSegmentation {
    private MeshSegmentation() { }
    public sealed record ScalarThresholdCase(Arr<double> Values, double Threshold, bool IncludeAbove, bool ValuesAreVertices) : MeshSegmentation;
    public sealed record ScalarBandsCase(Arr<double> Values, Dimension BandCount, bool ValuesAreVertices) : MeshSegmentation;
    public sealed record SeededRegionGrowCase(Arr<double> Values, Seq<int> SeedFaces, PositiveMagnitude Tolerance, Dimension MaxIterations, bool ValuesAreVertices) : MeshSegmentation;
    public sealed record DescriptorClustersCase(MeshDescriptor Descriptor, Dimension Eigenpairs, Dimension RegionCount, Dimension MaxIterations, PositiveMagnitude Tolerance) : MeshSegmentation;
    public sealed record WatershedCase(Arr<double> Values, PositiveMagnitude MergeTolerance, bool ValuesAreVertices) : MeshSegmentation;
    public sealed record NormalizedCutCase(Arr<double> Values, Dimension RegionCount, Dimension Eigenpairs, Dimension MaxIterations, PositiveMagnitude Tolerance, bool ValuesAreVertices) : MeshSegmentation;
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
    public static Fin<MeshSegmentation> Watershed(Arr<double> values, double mergeTolerance, bool valuesAreVertices = false, Op? key = null) =>
        key.OrDefault() switch { Op op => from admitted in AdmitScalars(values: values, key: op) from tolerance in op.AcceptValidated<PositiveMagnitude>(candidate: mergeTolerance) select (MeshSegmentation)new WatershedCase(Values: admitted, MergeTolerance: tolerance, ValuesAreVertices: valuesAreVertices) };
    public static Fin<MeshSegmentation> NormalizedCut(Arr<double> values, int regionCount, int eigenpairs, int maxIterations, double tolerance, bool valuesAreVertices = false, Op? key = null) =>
        key.OrDefault() switch { Op op => from admitted in AdmitScalars(values: values, key: op) from regions in op.AcceptValidated<Dimension>(candidate: regionCount) from _ in regionCount > 1 ? Fin.Succ(unit) : Fin.Fail<Unit>(op.InvalidInput()) from pairs in op.AcceptValidated<Dimension>(candidate: eigenpairs) from __ in eigenpairs > 1 ? Fin.Succ(unit) : Fin.Fail<Unit>(op.InvalidInput()) from cap in op.AcceptValidated<Dimension>(candidate: maxIterations) from eps in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance) select (MeshSegmentation)new NormalizedCutCase(Values: admitted, RegionCount: regions, Eigenpairs: pairs, MaxIterations: cap, Tolerance: eps, ValuesAreVertices: valuesAreVertices) };
    private static Fin<Arr<double>> AdmitScalars(Arr<double> values, Op key) =>
        values.Count == 0 || !values.AsIterable().All(RhinoMath.IsValidDouble)
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : Fin.Succ(values);
}

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

[SmartEnum<int>]
public sealed partial class RemeshStatus { public static readonly RemeshStatus Completed = new(key: 0); }

[SmartEnum<int>]
public sealed partial class SdfMeshDomain {
    public static readonly SdfMeshDomain SurfaceMesh = new(key: 0);
    public static readonly SdfMeshDomain BoundarySource = new(key: 1);
    public static readonly SdfMeshDomain VolumeGrid = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class SdfMeshStatus {
    public static readonly SdfMeshStatus ApproximateSignClosestDistance = new(key: 0);
    public static readonly SdfMeshStatus BoundarySourceSignedHeat = new(key: 1);
    public static readonly SdfMeshStatus ClosedSurfaceSignedHeat = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class TangentLogMapAlgorithm { public static readonly TangentLogMapAlgorithm VectorHeatApproximate = new(key: 0); }

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshSpace {
    private MeshSpace(Mesh native, Context tolerance) { Native = native; Tolerance = tolerance; }
    public static Fin<MeshSpace> Of(Mesh native, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(native).ToFin(op.InvalidInput())
               from ctx in Optional(context).ToFin(op.MissingContext())
               from _ in guard(active.IsValid, op.InvalidInput())
               let snapshot = active.DuplicateMesh()
               select new MeshSpace(native: snapshot, tolerance: ctx);
    }
    public Context Tolerance { get; }
    internal Mesh Native { get; }
    internal LaplacianCache Cache => LaplacianCache.For(space: this);
    public Mesh DuplicateNative() => Native.DuplicateMesh();
    public Fin<SparseLaplacian> Laplacian(MeshLaplacian kind, Op? key = null) =>
        MeshKernel.LaplacianOf(space: this, kind: kind, key: key.OrDefault());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TopologyReceipt(int Vertices, int TopologyVertices, int TopologyEdges, int Faces, int Triangles, int Quads, int Ngons, int VisiblePolygons, int BoundaryComponents, int NonManifoldEdges, bool HasBoundary, bool IsClosed, bool IsSolid, bool IsWatertight, bool IsManifold, bool IsOriented, int EulerCharacteristic, Option<int> Genus, bool EulerValidated) {
    internal Fin<TOut> Project<TOut>(Op key) {
        int euler = EulerCharacteristic, boundaryComponents = BoundaryComponents;
        Option<int> genusOption = Genus;
        return typeof(TOut) switch {
            Type t when t == typeof(TopologyReceipt) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof((int Euler, int Genus, int BoundaryComponents)) && genusOption.IsSome => Fin.Succ((TOut)(object)(euler, genusOption.IfNone(noneValue: 0), boundaryComponents)),
            Type t when t == typeof((int Euler, int Genus, int BoundaryComponents)) => Fin.Fail<TOut>(key.InvalidResult()),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(TopologyReceipt), outputType: typeof(TOut))),
        };
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TuftedLaplacianReceipt(MeshLaplacian Kind, int OriginalVertices, int OriginalFaces, int IntrinsicVertices, int IntrinsicEdges, int IntrinsicFaces, int LogicalCoverFaces, int LogicalCoverEdges, int BoundaryEdges, int NonManifoldEdges, int MollifiedEdges, double MaxMollification, int IntrinsicFlips, bool DelaunaySatisfied, int StiffnessNonZeros, int MassNonZeros, int PositiveMassCount, int SkippedDegenerateFaces, bool CoverAware, bool CollapsedToOriginalVertices) {
    public bool IsValid =>
        Kind is not null
        && new[] { OriginalVertices, OriginalFaces, IntrinsicVertices, IntrinsicEdges, IntrinsicFaces, LogicalCoverFaces, LogicalCoverEdges, BoundaryEdges, NonManifoldEdges, MollifiedEdges, IntrinsicFlips, StiffnessNonZeros, MassNonZeros, PositiveMassCount, SkippedDegenerateFaces }.All(static value => value >= 0)
        && RhinoMath.IsValidDouble(x: MaxMollification)
        && MaxMollification >= 0.0
        && (!CoverAware || LogicalCoverFaces >= IntrinsicFaces)
        && (!CoverAware || LogicalCoverEdges >= IntrinsicEdges)
        && (!CollapsedToOriginalVertices || IntrinsicVertices == OriginalVertices)
        && PositiveMassCount <= OriginalVertices
        && DelaunaySatisfied;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SparseLaplacian(SparseMatrix Stiffness, SparseMatrix MassConsistent, Arr<double> MassLumped, int SkippedDegenerateFaces = 0, Option<TuftedLaplacianReceipt> Tufted = default) {
    public bool IsValid => Stiffness.IsValid && MassConsistent.IsValid && Stiffness.Rows.Value == MassConsistent.Rows.Value && Stiffness.Cols.Value == MassConsistent.Cols.Value && Stiffness.Rows.Value == Stiffness.Cols.Value && MassLumped.Count == Stiffness.Rows.Value && MassLumped.All(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0) && Tufted.Map(static receipt => receipt.IsValid).IfNone(noneValue: true);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SpectralBasisBundle(SpectralBasis Basis, EigenSolveReceipt<double, Arr<double>> Eigen, bool CacheHit = false, int SkippedDegenerateFaces = 0, Option<int> FactorNonZeros = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct DescriptorReceipt(SpectralDescriptorReceipt Spectral, EigenSolveReceipt<double, Arr<double>> Eigen, int RequestedEigenpairs, int ReturnedEigenpairs, bool SpectralCacheHit = false, int SkippedDegenerateFaces = 0, Option<int> FactorNonZeros = default, Option<SpectralAssemblyReceipt> Assembly = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct DescriptorResult(Arr<double> Values, DescriptorReceipt Receipt);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshSamplingSpectrumReceipt(int VertexCount, int SampleCount, int EigenpairCount, double LowFrequencyEnergy, double TotalEnergy, double SuppressionRatio, double ValidationThreshold, bool Validated, MeshSamplingSpectrumAlgorithm? Algorithm = null) {
    internal bool IsValid => VertexCount > 0 && SampleCount > 0 && EigenpairCount > 0 && new[] { LowFrequencyEnergy, TotalEnergy, SuppressionRatio, ValidationThreshold }.All(RhinoMath.IsValidDouble) && TotalEnergy > 0.0 && SuppressionRatio is >= 0.0 and <= 1.0 && ValidationThreshold is >= 0.0 and <= 1.0 && Validated == (SuppressionRatio <= ValidationThreshold);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct FeatureEdge(int A, int B, MeshFeatureKind Kind, Option<double> DihedralRadians, Option<double> SignedDihedralRadians = default, Option<double> CurvatureSignal = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct FeatureReceipt(Seq<FeatureEdge> Edges, int BoundaryEdges, int CreaseEdges, int NonManifoldEdges, int UnweldedEdges, int NgonInteriorSkippedEdges, double DihedralThresholdRadians, int RidgeEdges = 0, int ValleyEdges = 0, int RegionBoundaryEdges = 0, double CurvatureThreshold = 0.0, double SmoothingScale = 0.0, int CurvatureFiniteVertices = 0, int CurvatureRejectedVertices = 0, MeshFeatureAlgorithm? Algorithm = null) {
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(FeatureReceipt) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(Seq<FeatureEdge>) => Fin.Succ((TOut)(object)Edges),
            Type t when t == typeof(Seq<(int A, int B)>) => Fin.Succ((TOut)(object)toSeq(Edges.AsIterable()
                .Where(static edge => !edge.Kind.Equals(MeshFeatureKind.NgonInteriorSkipped))
                .Select(static edge => (edge.A, edge.B)))),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(FeatureReceipt), outputType: typeof(TOut))),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshFeaturePolicy(VectorAngle DihedralThreshold, PositiveMagnitude CurvatureThreshold, PositiveMagnitude SmoothingScale, Option<Arr<int>> FaceRegions) {
    internal static Fin<MeshFeaturePolicy> Of(double dihedralRadians, MeshSpace space, Option<Arr<int>> faceRegions, Op key) =>
        from activeMesh in FieldNabla.MeshNative(space: space, key: key)
        from dihedral in key.AcceptValidated<VectorAngle>(candidate: dihedralRadians)
        from _ in guard(dihedral.Value > RhinoMath.ZeroTolerance, key.InvalidInput())
        let meanEdge = MeshKernel.MeanEdgeLengthOf(mesh: activeMesh)
        from curvature in key.AcceptValidated<PositiveMagnitude>(candidate: 1.0 / Math.Max(val1: meanEdge, val2: space.Tolerance.Absolute.Value))
        from smooth in key.AcceptValidated<PositiveMagnitude>(candidate: Math.Max(val1: meanEdge, val2: space.Tolerance.Absolute.Value))
        from policy in new MeshFeaturePolicy(DihedralThreshold: dihedral, CurvatureThreshold: curvature, SmoothingScale: smooth, FaceRegions: faceRegions).Admit(space: space, key: key)
        select policy;
    internal Fin<MeshFeaturePolicy> Admit(MeshSpace space, Op key) {
        MeshFeaturePolicy self = this;
        return (from activeMesh in FieldNabla.MeshNative(space: space, key: key)
                from dihedral in key.AcceptValidated<VectorAngle>(candidate: self.DihedralThreshold.Value)
                from _ in guard(dihedral.Value > RhinoMath.ZeroTolerance, key.InvalidInput())
                from curvature in key.AcceptValidated<PositiveMagnitude>(candidate: self.CurvatureThreshold.Value)
                from smooth in key.AcceptValidated<PositiveMagnitude>(candidate: self.SmoothingScale.Value)
                select (Mesh: activeMesh, Policy: new MeshFeaturePolicy(DihedralThreshold: dihedral, CurvatureThreshold: curvature, SmoothingScale: smooth, FaceRegions: self.FaceRegions)))
            .Bind(state => state.Policy.FaceRegions.Match(
                Some: active => guard(active.Count == state.Mesh.Faces.Count, key.InvalidInput()).ToFin().Map(_ => state.Policy),
                None: () => Fin.Succ(state.Policy)));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct FlattenReceipt(int VertexCount, int UvCount, int TextureCoordinateCount, int BoundaryComponents, bool NativeUnwrap, bool Valid, Option<double> EdgeLengthDistortionRms);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct FlattenResult(Arr<Point2d> Uvs, Mesh Mesh, FlattenReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(Arr<Point2d>) => Fin.Succ((TOut)(object)Uvs),
            Type t when t == typeof(FlattenResult) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(FlattenReceipt) => Fin.Succ((TOut)(object)Receipt),
            Type t when t == typeof(Mesh) => key.AcceptValue(value: Mesh).Map(static value => (TOut)(object)value),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(FlattenResult), outputType: typeof(TOut))),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshSegmentationReceipt(MeshSegmentationAlgorithm Algorithm, MeshSegmentationStatus Status, int RequestedRegionCount, int RegionCount, int SeedCount, int AssignedFaceCount, int UnassignedFaceCount, int SkippedDegenerateFaces, int SkippedNonFiniteValues, Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance, Option<double> Threshold, Option<DescriptorReceipt> Descriptor, Option<SolveReceipt> Solve, Option<bool> SpectralCacheHit, Option<bool> FactorCacheHit, Option<int> FactorNonZeros, Option<double> NormalizedCutValue = default, Option<int> AffinityNonZeros = default, Option<int> WatershedSaddleCount = default, Option<EigenSolveReceipt<double, Arr<double>>> Eigen = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshSegmentationResult(Arr<int> FaceRegions, Arr<int> VertexRegions, MeshSegmentationReceipt Receipt);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct RemeshReceipt(RemeshKind Kind, RemeshStatus Status, Option<double> TargetLength, Option<int> DesiredPolygonCount, int PreVertexCount, int PreFaceCount, int PostVertexCount, int PostFaceCount, double ReductionRatio, bool Valid, bool HardEdgePreservationRequested, bool TopologyChanged);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct RemeshResult(Mesh Mesh, RemeshReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(Mesh) => key.AcceptValue(value: Mesh).Map(static value => (TOut)(object)value),
            Type t when t == typeof(RemeshResult) => Fin.Succ((TOut)(object)this),
            Type t when t == typeof(RemeshReceipt) => Fin.Succ((TOut)(object)Receipt),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(RemeshResult), outputType: typeof(TOut))),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SignedHeatReceipt(int BoundarySourceVertexCount, int BoundaryEncodedEdgeSourceCount, int BoundaryRejectedPointCount, int BoundaryUnmatchedSegmentCount, Option<SolveReceipt> HeatSolve, SolveReceipt PoissonSolve, Option<SpectralAssemblyReceipt> EdgeAssembly = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct VolumeGridReceipt(BoundingBox Bounds, int Resolution, int XNodes, int YNodes, int ZNodes, double CellSize, double Padding, int NodeCount, int CellCount, int SourceTriangleCount, int DegenerateTriangleCount, double SourceArea, int InsideNodeCount, int OutsideNodeCount, int NearSurfaceNodeCount, int RejectedVectorCount, double HeatTime, int GaugeNode, double SurfaceShift, VolumeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition, VolumeSolverPolicy Solver, int OperatorNonZeros, Option<int> FactorNonZeros, double Residual);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SdfMeshReceipt(SdfMeshMethod Method, SdfMeshStatus Status, SdfMeshDomain Domain, TopologyReceipt Topology, Option<SignedHeatReceipt> SignedHeat, Option<VolumeGridReceipt> VolumeGrid = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SdfMeshSample(double Distance, SdfMeshReceipt Receipt);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SignpostTransportReceipt(int VertexCount, int IntrinsicEdgeCount, int TransportedEdgeCount, int IntrinsicFlipCount, int ChordFallbackEdges, int MissingFrameEdges, int CommonSubdivisionSegments, bool IntrinsicSnapshot, bool ExactCommonSubdivision, double MaxAngleRadians, double MaxLengthResidual) {
    public bool IsValid =>
        new[] { VertexCount, IntrinsicEdgeCount, TransportedEdgeCount, IntrinsicFlipCount, ChordFallbackEdges, MissingFrameEdges, CommonSubdivisionSegments }.All(static value => value >= 0)
        && TransportedEdgeCount + MissingFrameEdges <= IntrinsicEdgeCount
        && ChordFallbackEdges <= TransportedEdgeCount
        && (!ExactCommonSubdivision || CommonSubdivisionSegments > 0)
        && RhinoMath.IsValidDouble(x: MaxAngleRadians)
        && RhinoMath.IsValidDouble(x: MaxLengthResidual)
        && MaxAngleRadians >= 0.0
        && MaxLengthResidual >= 0.0
        && IntrinsicSnapshot;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct TangentLogMapReceipt(int SourceVertex, int TargetCount, double HeatTime, bool VectorHeatBacked, bool RejectsFlippedIntrinsic, int FiniteLogCount, double MaxMagnitudeResidual, TangentLogMapAlgorithm? Algorithm = null);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct TangentLogMapResult(Vector3d Tangent, TangentLogMapReceipt Receipt);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal readonly record struct BoundarySignedHeatKey(SignedHeatTime Heat, VolumeSolverPolicy Solver);

internal readonly record struct ClosedSignedHeatKey(VolumeGridPolicy Grid, SignedHeatTime Heat, VolumeSolverPolicy Solver, VolumeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition);

[StructLayout(LayoutKind.Auto)]
internal readonly record struct CrossFieldKey(int Symmetry, Option<Arr<(int Vertex, Direction Hint)>> Constraints, Option<Arr<(int Vertex, double HolonomyDeficit)>> Cones) {
    internal static CrossFieldKey Of(int symmetry, Option<Seq<(int Vertex, Direction Hint)>> constraints, Option<Seq<(int Vertex, double HolonomyDeficit)>> cones) =>
        new(
            Symmetry: symmetry,
            Constraints: constraints.Map(static values => new Arr<(int Vertex, Direction Hint)>([.. values.AsIterable().OrderBy(static row => row.Vertex).ThenBy(static row => row.Hint.Value.X).ThenBy(static row => row.Hint.Value.Y).ThenBy(static row => row.Hint.Value.Z)])),
            Cones: cones.Map(static values => new Arr<(int Vertex, double HolonomyDeficit)>([.. values.AsIterable().OrderBy(static row => row.Vertex).ThenBy(static row => row.HolonomyDeficit)])));
}

internal readonly record struct GeodesicKey(Seq<int> Sources);

internal readonly record struct HodgeKey(VectorField Source);

[StructLayout(LayoutKind.Auto)] internal readonly record struct McfKey(double TimeStep, int Iterations);

[StructLayout(LayoutKind.Auto)] internal readonly record struct VectorHeatKey(double Time, Seq<(int Vertex, Vector3d Direction)> Sources);

internal readonly record struct EdgeConnectionFactor(CholeskySparse Factor, SpectralAssemblyReceipt Receipt);

internal readonly record struct BoundarySignedHeatSource(Arr<double> Rhs, Seq<int> SourceVertices, int EncodedEdgeSourceCount, int RejectedBoundaryPointCount, int UnmatchedBoundarySegmentCount);

internal readonly record struct SignedHeatSolution(Arr<double> Values, SignedHeatReceipt Receipt, TopologyReceipt Topology);

internal readonly record struct ClosedSignedHeatSolution(VolumeGridDomain Domain, Arr<double> Values, SignedHeatReceipt Receipt, TopologyReceipt Topology);

internal readonly record struct VolumeGridDomain(BoundingBox Bounds, int Resolution, int XCells, int YCells, int ZCells, double CellSize, double Padding, VolumeGridReceipt Receipt) {
    internal int XNodes => XCells + 1; internal int YNodes => YCells + 1; internal int ZNodes => ZCells + 1; internal int NodeCount => XNodes * YNodes * ZNodes; internal int CellCount => XCells * YCells * ZCells;
    internal int Index(int x, int y, int z) => x + (XNodes * (y + (YNodes * z)));
    internal Point3d PointAt(int x, int y, int z) => new(x: Bounds.Min.X + (x * CellSize), y: Bounds.Min.Y + (y * CellSize), z: Bounds.Min.Z + (z * CellSize));
}

internal sealed class LaplacianCache {
    internal const int DefaultSpectralCount = 32;
    private const double SpdRegularization = 1e-8;
    private static readonly ConditionalWeakTable<object, LaplacianCache> Table = [];
    private sealed class Memo<TKey, T> {
        private readonly Atom<HashMap<TKey, T>> cache = Atom(value: HashMap<TKey, T>());
        internal Fin<T> Of(TKey probe, Func<Fin<T>> compute) =>
            cache.Value.Find(key: probe).Map(static value => Fin.Succ(value)).IfNone(() => {
                Fin<T> computed = compute();
                return computed.Match(
                    Succ: value => {
                        _ = cache.Swap(f: map => map.AddOrUpdate(key: probe, value: value));
                        return Fin.Succ(value);
                    },
                    Fail: error => Fin.Fail<T>(error));
            });
        internal bool Contains(TKey probe) => cache.Value.ContainsKey(key: probe);
    }
    private readonly Memo<Unit, SparseLaplacian> cotangent = new(), intrinsicDelaunay = new(), tuftedIntrinsic = new();
    private readonly Memo<Unit, CholeskySparse> cholesky = new();
    private readonly Memo<Unit, SpectralBasisBundle> defaultSpectral = new();
    private readonly Lazy<double> meanEdgeLength;
    private readonly Memo<Unit, MeshKernel.IntrinsicMesh> intrinsicMesh = new(), tuftedIntrinsicMesh = new();
    private readonly Memo<(int Symmetry, double Time), CholeskySparse> connectionCholesky = new();
    private readonly Memo<double, CholeskySparse> scalarHeatCholesky = new();
    private readonly Memo<double, EdgeConnectionFactor> edgeConnectionCholesky = new();
    private readonly Memo<GeodesicKey, Arr<double>> geodesicCache = new();
    private readonly Memo<McfKey, Arr<double>> mcfCache = new();
    private readonly Memo<CrossFieldKey, Complex[]> crossFieldCache = new();
    private readonly Memo<HodgeKey, MeshKernel.HodgeBundle> hodgeCache = new();
    private readonly Memo<VectorHeatKey, Complex[]> vectorHeatCache = new();
    private readonly Memo<BoundarySignedHeatKey, SignedHeatSolution> signedHeat = new();
    private readonly Memo<ClosedSignedHeatKey, ClosedSignedHeatSolution> closedSignedHeat = new();
    private readonly MeshSpace space;
    private LaplacianCache(MeshSpace space) {
        this.space = space;
        meanEdgeLength = new Lazy<double>(valueFactory: () => MeshKernel.MeanEdgeLengthOf(mesh: space.Native));
    }
    internal static LaplacianCache For(MeshSpace space) =>
        Table.GetValue(key: space.Native, createValueCallback: _ => new LaplacianCache(space: space));
    internal Fin<SparseLaplacian> Cotangent(Op key) =>
        cotangent.Of(probe: unit, compute: () => MeshKernel.AssembleCotangent(mesh: space.Native, key: key));
    internal Fin<SparseLaplacian> IntrinsicDelaunay(Op key) =>
        intrinsicDelaunay.Of(probe: unit, compute: () =>
            from imesh in IntrinsicMeshSnapshot(key: key)
            from laplacian in MeshKernel.AssembleCotangentFromIntrinsic(imesh: imesh, key: key)
            select laplacian);
    internal Fin<SparseLaplacian> TuftedIntrinsic(Op key) =>
        tuftedIntrinsic.Of(probe: unit, compute: () =>
            from imesh in TuftedIntrinsicMeshSnapshot(key: key)
            from laplacian in MeshKernel.AssembleTuftedCotangentFromIntrinsic(imesh: imesh, key: key)
            select laplacian);
    internal Fin<CholeskySparse> Cholesky(Op key) =>
        cholesky.Of(probe: unit, compute: () =>
            from L in IntrinsicDelaunay(key: key)
            from spd in MeshKernel.AssembleMassStiffnessSystem(laplacian: L, massScale: SpdRegularization, stiffnessScale: 1.0, key: key)
            from factor in CholeskySparse.Of(symmetric: spd, key: key)
            select factor);
    internal Fin<MeshKernel.IntrinsicMesh> IntrinsicMeshSnapshot(Op key) =>
        intrinsicMesh.Of(probe: unit, compute: () => MeshKernel.BuildIntrinsicMesh(mesh: space.Native, key: key));
    internal Fin<MeshKernel.IntrinsicMesh> TuftedIntrinsicMeshSnapshot(Op key) =>
        tuftedIntrinsicMesh.Of(probe: unit, compute: () => MeshKernel.BuildTuftedIntrinsicMesh(mesh: space.Native, key: key));
    internal Fin<Arr<double>> SignedHeat(SdfMeshPolicy policy, Op key) => SignedHeatDetailed(policy: policy, key: key).Map(static result => result.Values);
    internal Fin<SignedHeatSolution> SignedHeatDetailed(SdfMeshPolicy policy, Op key) =>
        signedHeat.Of(probe: new BoundarySignedHeatKey(Heat: policy.Heat, Solver: policy.Solver), compute: () => MeshKernel.ComputeSignedHeatDetailed(space: space, policy: policy, key: key));
    internal Fin<ClosedSignedHeatSolution> ClosedSignedHeatDetailed(SdfMeshPolicy policy, Op key) =>
        policy.Grid.ToFin(key.InvalidInput()).Bind(grid => closedSignedHeat.Of(probe: new ClosedSignedHeatKey(Grid: grid, Heat: policy.Heat, Solver: policy.Solver, Interpolation: policy.Interpolation, BoundaryCondition: policy.BoundaryCondition), compute: () => MeshKernel.ComputeClosedSignedHeatDetailed(space: space, policy: policy, key: key)));
    internal double MeanEdgeLength => meanEdgeLength.Value;
    internal Fin<SpectralBasisBundle> SpectralBasisBundleOf(int k, Op key) {
        bool cacheHit = k <= DefaultSpectralCount && defaultSpectral.Contains(probe: unit);
        return k <= DefaultSpectralCount
            ? defaultSpectral.Of(probe: unit, compute: () => MeshKernel.ComputeSpectralBasisDetailed(space: space, k: DefaultSpectralCount, key: key)).Map(bundle =>
                bundle.Basis.Truncate(k: k) switch {
                    SpectralBasis basis => bundle with {
                        Basis = basis,
                        Eigen = bundle.Eigen with { Pairs = toSeq(Enumerable.Take(source: bundle.Eigen.Pairs.AsIterable(), count: basis.Eigenvalues.Count)), RequestedPairs = k, ReturnedPairs = basis.Eigenvalues.Count },
                        CacheHit = cacheHit,
                    },
                })
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
            from L in IntrinsicDelaunay(key: key)
            from system in MeshKernel.AssembleMassStiffnessSystem(laplacian: L, stiffnessScale: time, key: key)
            from factor in CholeskySparse.Of(symmetric: system, key: key)
            select factor);
    internal Fin<EdgeConnectionFactor> EdgeConnectionCholeskyDetailed(double time, Op key) =>
        edgeConnectionCholesky.Of(probe: time, compute: () =>
            from imesh in IntrinsicMeshSnapshot(key: key)
            from system in SpectralCore.BuildCrouzeixRaviartHeatSystemDetailed(mesh: imesh, time: time, key: key)
            from factor in CholeskySparse.Of(symmetric: system.Matrix, key: key)
            select new EdgeConnectionFactor(Factor: factor, Receipt: system.Receipt with { FactorNonZeros = Some(factor.FactorNonZeros) }));
    internal Fin<Arr<double>> Geodesic(GeodesicKey probe, Func<Fin<Arr<double>>> compute) => geodesicCache.Of(probe, compute);
    internal Fin<Arr<double>> Mcf(McfKey probe, Func<Fin<Arr<double>>> compute) => mcfCache.Of(probe, compute);
    internal Fin<Complex[]> CrossField(CrossFieldKey probe, Func<Fin<Complex[]>> compute) => crossFieldCache.Of(probe, compute);
    internal Fin<MeshKernel.HodgeBundle> Hodge(HodgeKey probe, Func<Fin<MeshKernel.HodgeBundle>> compute) => hodgeCache.Of(probe, compute);
    internal Fin<Complex[]> VectorHeat(VectorHeatKey probe, Func<Fin<Complex[]>> compute) => vectorHeatCache.Of(probe, compute);
}

internal static class MeshKernel {
    private const double DegenerateTriangleArea = 1e-14;
    private const double AspectRatioCeiling = 11.5;
    private const double MeshSpectrumLowFrequencyCeiling = 0.5;
    private const double VolumeGridKernelSofteningRatio = 0.0625;
    private const int VolumeGridMaxNodes = 1_000_000;
    private const int UnassignedRegion = -1;
    private readonly record struct SegmentationScalars(Arr<double> FaceValues, int SkippedDegenerateFaces, int SkippedNonFiniteValues, int FiniteCount, double Min, double Max);
    private readonly record struct FeatureCurvatureSignals(double[] Edge, int FiniteVertices, int RejectedVertices);
    private readonly record struct SegmentationRun(MeshSegmentationAlgorithm Algorithm, int RequestedRegionCount, int SeedCount, MeshSegmentationStatus Status, Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance, Option<double> Threshold, Option<DescriptorReceipt> Descriptor, Option<SolveReceipt> Solve = default, Option<bool> FactorCacheHit = default, Option<int> FactorNonZeros = default, Option<double> NormalizedCutValue = default, Option<int> AffinityNonZeros = default, Option<int> WatershedSaddleCount = default, Option<EigenSolveReceipt<double, Arr<double>>> Eigen = default);
    private readonly record struct WatershedState(int[] Regions, int SeedCount, int SaddleCount);
    private readonly record struct NormalizedCutSystem(SparseMatrix Laplacian, SparseMatrix Degree, int AffinityNonZeros, double Sigma);
    private readonly record struct ClusterState(int[] Labels, int Iterations, bool Converged);
    [StructLayout(LayoutKind.Auto)] private readonly record struct IntrinsicLengthSet(double LAb, double LBc, double LAc, int MollifiedEdges, double MaxMollification);
    [StructLayout(LayoutKind.Auto)] private readonly record struct ConnectionEntries(Seq<(int I, int J, double Weight, double Rho)> Rows, SignpostTransportReceipt Receipt);
    [StructLayout(LayoutKind.Auto)] private readonly record struct VolumeSource(Point3d Center, Vector3d Normal, double Area);
    [StructLayout(LayoutKind.Auto)] private readonly record struct VolumeSourceSet(VolumeSource[] Sources, int Degenerate, double Area);
    [StructLayout(LayoutKind.Auto)] private readonly record struct VolumeGridVectors(double[] X, double[] Y, double[] Z, int Rejected, int Inside, int Outside, int NearSurface, int InteriorIndex);
    [StructLayout(LayoutKind.Auto)] private readonly record struct VolumeGridSystem(SparseMatrix Operator, Arr<double> Rhs, int GaugeNode);
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
    internal static Fin<Seq<Point3d>> SurfaceCandidatePoints(MeshSpace space, double density, Op key) {
        if (!RhinoMath.IsValidDouble(x: density) || density <= 0.0) return Fin.Fail<Seq<Point3d>>(key.InvalidInput());
        List<Point3d> samples = [];
        using Mesh triangulated = space.Native.DuplicateMesh();
        if (ContainsQuads(mesh: triangulated) && !triangulated.Faces.ConvertQuadsToTriangles()) return Fin.Fail<Seq<Point3d>>(key.InvalidResult());
        for (int f = 0; f < triangulated.Faces.Count; f++) {
            MeshFace face = triangulated.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d a = triangulated.Vertices[index: face.A]; Point3d b = triangulated.Vertices[index: face.B]; Point3d c = triangulated.Vertices[index: face.C];
            double area = 0.5 * Vector3d.CrossProduct(a: b - a, b: c - a).Length;
            int count = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: area * density)); int side = Math.Max(val1: 1, val2: (int)Math.Ceiling(a: Math.Sqrt(d: count * 2.0)));
            int emitted = 0;
            for (int i = 0; i <= side && emitted < count; i++) {
                for (int j = 0; j <= side - i && emitted < count; j++) {
                    double wa = (i + 1.0) / (side + 3.0); double wb = (j + 1.0) / (side + 3.0); double wc = 1.0 - wa - wb;
                    samples.Add(item: new Point3d(x: (wa * a.X) + (wb * b.X) + (wc * c.X), y: (wa * a.Y) + (wb * b.Y) + (wc * c.Y), z: (wa * a.Z) + (wb * b.Z) + (wc * c.Z)));
                    emitted++;
                }
            }
        }
        return samples.Count > 0 && samples.TrueForAll(static point => point.IsValid)
            ? Fin.Succ(toSeq(samples))
            : Fin.Fail<Seq<Point3d>>(key.InvalidResult());
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
    internal static Fin<IntrinsicMesh> BuildTuftedIntrinsicMesh(Mesh mesh, Op key) =>
        from source in IntrinsicMesh.FromMesh(mesh: mesh, key: key, tuftedCover: true)
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
        internal bool TuftedCover;
        internal int OriginalFaceCount;
        internal int FlipCount;
        internal int MollifiedEdgeCount;
        internal double MaxMollification;
        internal int BoundaryEdgeCount;
        internal int NonManifoldEdgeCount;
        private IntrinsicEdge[]? frozenEdges;
        private Dictionary<(int Lo, int Hi), int>? frozenEdgeIndex;
        private int[][]? frozenFaceEdges;
        private double[]? frozenFaceAreas;
        private int[]? frozenFirstIncidentEdge;
        internal bool IsFrozen => frozenEdges is not null;
        internal int EdgeCount => frozenEdges?.Length ?? 0;
        internal int LiveFaceCount => Triangles.Count(static slot => slot.HasValue);
        internal int LogicalCoverFaceCount => TuftedCover ? LiveFaceCount * 2 : LiveFaceCount;
        internal int LogicalCoverEdgeCount => TuftedCover ? EdgeData.Values.Sum(static edge => Math.Max(val1: 2, val2: edge.FaceIdx.Count * 2)) : EdgeData.Count;
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
            BoundaryEdgeCount = EdgeData.Values.Count(static edge => edge.FaceIdx.Count == 1);
            NonManifoldEdgeCount = EdgeData.Values.Count(static edge => edge.FaceIdx.Count > 2);
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
        internal static Fin<IntrinsicMesh> FromMesh(Mesh mesh, Op key, bool tuftedCover = false) {
            using Mesh active = mesh.DuplicateMesh();
            if (ContainsQuads(mesh: active) && !active.Faces.ConvertQuadsToTriangles())
                return Fin.Fail<IntrinsicMesh>(key.InvalidResult());
            IntrinsicMesh m = new() { VertexCount = active.Vertices.Count, OriginalFaceCount = active.Faces.Count, TuftedCover = tuftedCover };
            for (int f = 0; f < active.Faces.Count; f++) {
                MeshFace face = active.Faces[index: f];
                if (!face.IsTriangle) continue;
                Point3d pa = active.Vertices[index: face.A]; Point3d pb = active.Vertices[index: face.B]; Point3d pc = active.Vertices[index: face.C];
                IntrinsicLengthSet lengths = tuftedCover
                    ? MollifiedLengths(lAB: pa.DistanceTo(other: pb), lBC: pb.DistanceTo(other: pc), lAC: pa.DistanceTo(other: pc))
                    : new IntrinsicLengthSet(LAb: pa.DistanceTo(other: pb), LBc: pb.DistanceTo(other: pc), LAc: pa.DistanceTo(other: pc), MollifiedEdges: 0, MaxMollification: 0.0);
                m.MollifiedEdgeCount += lengths.MollifiedEdges;
                m.MaxMollification = Math.Max(val1: m.MaxMollification, val2: lengths.MaxMollification);
                _ = m.AddTriangle(a: face.A, b: face.B, c: face.C, lAB: lengths.LAb, lBC: lengths.LBc, lAC: lengths.LAc);
            }
            return Fin.Succ(m);
        }
        private static IntrinsicLengthSet MollifiedLengths(double lAB, double lBC, double lAC) {
            double epsilon = RhinoMath.SqrtEpsilon;
            double longest = Math.Max(val1: lAB, val2: Math.Max(val1: lBC, val2: lAC));
            double delta = Math.Max(val1: 0.0, val2: longest - (lAB + lBC + lAC - longest) + epsilon);
            return new IntrinsicLengthSet(
                LAb: lAB + delta,
                LBc: lBC + delta,
                LAc: lAC + delta,
                MollifiedEdges: delta > 0.0 ? 3 : 0,
                MaxMollification: delta);
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
                edge.Length = Math.Max(val1: edge.Length, val2: length);
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
            FlipCount++;
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
    internal static Fin<SparseLaplacian> AssembleTuftedCotangentFromIntrinsic(IntrinsicMesh imesh, Op key) =>
        from laplacian in AssembleCotangentFromIntrinsic(imesh: imesh, key: key)
        let receipt = new TuftedLaplacianReceipt(
            Kind: MeshLaplacian.TuftedIntrinsic,
            OriginalVertices: imesh.VertexCount,
            OriginalFaces: imesh.OriginalFaceCount,
            IntrinsicVertices: imesh.VertexCount,
            IntrinsicEdges: imesh.EdgeCount,
            IntrinsicFaces: imesh.LiveFaceCount,
            LogicalCoverFaces: imesh.LogicalCoverFaceCount,
            LogicalCoverEdges: imesh.LogicalCoverEdgeCount,
            BoundaryEdges: imesh.BoundaryEdgeCount,
            NonManifoldEdges: imesh.NonManifoldEdgeCount,
            MollifiedEdges: imesh.MollifiedEdgeCount,
            MaxMollification: imesh.MaxMollification,
            IntrinsicFlips: imesh.FlipCount,
            DelaunaySatisfied: imesh.EdgeData.Keys.All(edge => imesh.IsDelaunay(i: edge.Lo, j: edge.Hi)),
            StiffnessNonZeros: laplacian.Stiffness.NonZeros,
            MassNonZeros: laplacian.MassConsistent.NonZeros,
            PositiveMassCount: laplacian.MassLumped.Count(static value => value > RhinoMath.ZeroTolerance),
            SkippedDegenerateFaces: laplacian.SkippedDegenerateFaces,
            CoverAware: imesh.TuftedCover,
            CollapsedToOriginalVertices: true)
        from _ in receipt.IsValid ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
        select laplacian with { Tufted = Some(receipt) };

    // --- [SELECTION] ------------------------------------------------------------------------
    internal static Fin<SparseLaplacian> LaplacianOf(MeshSpace space, MeshLaplacian kind, Op key) =>
        from active in Optional(kind).ToFin(key.InvalidInput())
        from _ in active.Equals(MeshLaplacian.Cotangent)
            ? AspectRatioGuard(mesh: space.Native, ceiling: AspectRatioCeiling, key: key)
            : Fin.Succ(unit)
        from result in active.Select(cache: space.Cache, key: key)
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

    // --- [SPECTRAL_BASIS] ------------------------------------------------------------------
    internal static Fin<SpectralBasisBundle> ComputeSpectralBasisDetailed(MeshSpace space, int k, Op key) =>
        Math.Min(val1: k, val2: space.Native.Vertices.Count - 1) switch {
            < 1 => Fin.Fail<SpectralBasisBundle>(key.InvalidInput()),
            int count => from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
                         from receipt in MatrixKernel.GeneralizedEigenpairsDetailed(stiffness: laplacian.Stiffness, mass: laplacian.MassConsistent, k: count, key: key)
                         select new SpectralBasisBundle(
                             Basis: new SpectralBasis(
                                 Eigenvalues: new Arr<double>([.. receipt.Pairs.AsIterable().Select(static p => p.Eigenvalue)]),
                                 Eigenvectors: new Arr<Arr<double>>([.. receipt.Pairs.AsIterable().Select(static p => p.Eigenvector)])),
                             Eigen: receipt,
                             CacheHit: false,
                             SkippedDegenerateFaces: laplacian.SkippedDegenerateFaces,
                             FactorNonZeros: receipt.FactorNonZeros),
        };

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
        bool manifold = mesh.IsManifold(topologicalTest: true, isOriented: out bool oriented, hasBoundary: out bool hasBoundary);
        int euler = mesh.TopologyVertices.Count - mesh.TopologyEdges.Count + mesh.Faces.Count;
        (int boundaryComponents, int nonManifoldEdges) = TopologyEdgeStatsOf(mesh: mesh);
        bool closed = mesh.IsClosed;
        bool solid = mesh.IsSolid;
        bool watertight = closed && solid && manifold && nonManifoldEdges == 0;
        int components = Math.Max(val1: 1, val2: mesh.DisjointMeshCount);
        int numerator = (2 * components) - boundaryComponents - euler;
        bool hasGenus = manifold && oriented && numerator >= 0 && numerator % 2 == 0;
        return Fin.Succ(new TopologyReceipt(Vertices: mesh.Vertices.Count, TopologyVertices: mesh.TopologyVertices.Count, TopologyEdges: mesh.TopologyEdges.Count, Faces: mesh.Faces.Count, Triangles: mesh.Faces.TriangleCount, Quads: mesh.Faces.QuadCount, Ngons: mesh.Ngons.Count, VisiblePolygons: mesh.GetNgonAndFacesCount(), BoundaryComponents: boundaryComponents, NonManifoldEdges: nonManifoldEdges, HasBoundary: hasBoundary || boundaryComponents > 0, IsClosed: closed, IsSolid: solid, IsWatertight: watertight, IsManifold: manifold, IsOriented: oriented, EulerCharacteristic: euler, Genus: hasGenus ? Some(numerator / 2) : Option<int>.None, EulerValidated: hasGenus));
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
    internal static Fin<FeatureReceipt> DetectFeatureEdgesDetailed(MeshSpace space, double dihedralRadians, Op key) =>
        from policy in MeshFeaturePolicy.Of(dihedralRadians: dihedralRadians, space: space, faceRegions: Option<Arr<int>>.None, key: key)
        from receipt in DetectFeatureEdgesDetailed(space: space, policy: policy, key: key)
        select receipt;
    internal static Fin<FeatureReceipt> DetectFeatureEdgesDetailed(MeshSpace space, MeshFeaturePolicy policy, Op key) =>
        policy.Admit(space: space, key: key).Map(activePolicy => {
            Mesh mesh = space.Native;
            _ = mesh.FaceNormals.ComputeFaceNormals();
            Vector3f[] faceNormals = [.. mesh.FaceNormals];
            FeatureCurvatureSignals curvature = EdgeCurvatureSignals(mesh: mesh, faceNormals: faceNormals, smoothingScale: activePolicy.SmoothingScale.Value);
            List<FeatureEdge> features = new(capacity: mesh.TopologyEdges.Count);
            int[] counts = new int[MeshFeatureKind.Items.Count];
            for (int e = 0; e < mesh.TopologyEdges.Count; e++) {
                int[] faces = mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: e);
                IndexPair p = mesh.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: e);
                Option<double> signed = Option<double>.None, signal = Option<double>.None;
                (MeshFeatureKind Kind, Option<double> Angle)? feature = faces.Length switch {
                    1 => (MeshFeatureKind.Boundary, Option<double>.None),
                    > 2 => (MeshFeatureKind.NonManifold, Option<double>.None),
                    2 when mesh.TopologyEdges.IsEdgeUnwelded(topologyEdgeIndex: e) => (MeshFeatureKind.Unwelded, Option<double>.None),
                    2 when mesh.TopologyEdges.IsNgonInterior(topologyEdgeIndex: e) => (MeshFeatureKind.NgonInteriorSkipped, Option<double>.None),
                    2 => ClassifySmoothFeature(mesh: mesh, edge: e, faces: faces, faceNormals: faceNormals, policy: activePolicy, edgeCurvature: curvature.Edge[e], signed: out signed, signal: out signal),
                    _ => null,
                };
                if (feature is not { } edge) continue;
                features.Add(item: new FeatureEdge(A: p.I, B: p.J, Kind: edge.Kind, DihedralRadians: edge.Angle, SignedDihedralRadians: signed, CurvatureSignal: signal));
                counts[edge.Kind.Key]++;
            }
            return new FeatureReceipt(Edges: toSeq(features), BoundaryEdges: counts[MeshFeatureKind.Boundary.Key], CreaseEdges: counts[MeshFeatureKind.Crease.Key], NonManifoldEdges: counts[MeshFeatureKind.NonManifold.Key], UnweldedEdges: counts[MeshFeatureKind.Unwelded.Key], NgonInteriorSkippedEdges: counts[MeshFeatureKind.NgonInteriorSkipped.Key], DihedralThresholdRadians: activePolicy.DihedralThreshold.Value, RidgeEdges: counts[MeshFeatureKind.Ridge.Key], ValleyEdges: counts[MeshFeatureKind.Valley.Key], RegionBoundaryEdges: counts[MeshFeatureKind.RegionBoundary.Key], CurvatureThreshold: activePolicy.CurvatureThreshold.Value, SmoothingScale: activePolicy.SmoothingScale.Value, CurvatureFiniteVertices: curvature.FiniteVertices, CurvatureRejectedVertices: curvature.RejectedVertices, Algorithm: MeshFeatureAlgorithm.DihedralProxy);
        });
    private static (MeshFeatureKind Kind, Option<double> Angle)? ClassifySmoothFeature(Mesh mesh, int edge, int[] faces, Vector3f[] faceNormals, MeshFeaturePolicy policy, double edgeCurvature, out Option<double> signed, out Option<double> signal) {
        double rawAngle = Vector3d.VectorAngle(a: (Vector3d)faceNormals[faces[0]], b: (Vector3d)faceNormals[faces[1]]);
        double signedAngle = SignedDihedral(mesh: mesh, edge: edge, faces: faces, faceNormals: faceNormals, angle: rawAngle);
        Option<double> angle = RhinoMath.IsValidDouble(x: rawAngle) ? Some(rawAngle) : Option<double>.None;
        signed = RhinoMath.IsValidDouble(x: signedAngle) ? Some(signedAngle) : Option<double>.None;
        signal = RhinoMath.IsValidDouble(x: edgeCurvature) ? Some(edgeCurvature) : Option<double>.None;
        if (policy.FaceRegions.Match(Some: regions => regions[index: faces[0]] != regions[index: faces[1]], None: static () => false))
            return (MeshFeatureKind.RegionBoundary, angle);
        if (!RhinoMath.IsValidDouble(x: rawAngle)) return null;
        bool highCurvature = RhinoMath.IsValidDouble(x: edgeCurvature) && edgeCurvature >= policy.CurvatureThreshold.Value;
        if (highCurvature && Math.Abs(value: signedAngle) >= policy.DihedralThreshold.Value)
            return signedAngle >= 0.0 ? (MeshFeatureKind.Ridge, angle) : (MeshFeatureKind.Valley, angle);
        return rawAngle >= policy.DihedralThreshold.Value ? (MeshFeatureKind.Crease, angle) : null;
    }
    private static double SignedDihedral(Mesh mesh, int edge, int[] faces, Vector3f[] faceNormals, double angle) {
        Line line = mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: edge);
        if (!line.IsValid) return angle;
        Vector3d axis = line.To - line.From;
        if (!axis.Unitize()) return angle;
        Vector3d n0 = (Vector3d)faceNormals[faces[0]], n1 = (Vector3d)faceNormals[faces[1]];
        double sign = Vector3d.CrossProduct(a: n0, b: n1) * axis;
        return sign < 0.0 ? -angle : angle;
    }
    private static FeatureCurvatureSignals EdgeCurvatureSignals(Mesh mesh, Vector3f[] faceNormals, double smoothingScale) {
        double[] edgeSignals = new double[mesh.TopologyEdges.Count];
        double[] edgeLengths = new double[mesh.TopologyEdges.Count];
        double[] vertexSum = new double[mesh.TopologyVertices.Count];
        int[] vertexCount = new int[mesh.TopologyVertices.Count];
        for (int e = 0; e < mesh.TopologyEdges.Count; e++) {
            int[] faces = mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: e);
            Line line = mesh.TopologyEdges.EdgeLine(topologyEdgeIndex: e);
            edgeSignals[e] = double.NaN;
            if (faces.Length != 2 || !line.IsValid) continue;
            double length = line.Length;
            if (!RhinoMath.IsValidDouble(x: length) || length <= RhinoMath.SqrtEpsilon) continue;
            double angle = Vector3d.VectorAngle(a: (Vector3d)faceNormals[faces[0]], b: (Vector3d)faceNormals[faces[1]]);
            if (!RhinoMath.IsValidDouble(x: angle)) continue;
            double signal = Math.Abs(value: angle) / length;
            edgeSignals[e] = signal;
            edgeLengths[e] = length;
            IndexPair pair = mesh.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: e);
            if (pair.I >= 0 && pair.I < vertexSum.Length) { vertexSum[pair.I] += signal; vertexCount[pair.I]++; }
            if (pair.J >= 0 && pair.J < vertexSum.Length) { vertexSum[pair.J] += signal; vertexCount[pair.J]++; }
        }
        for (int e = 0; e < mesh.TopologyEdges.Count; e++) {
            if (!RhinoMath.IsValidDouble(x: edgeSignals[e]) || edgeLengths[e] <= RhinoMath.SqrtEpsilon) continue;
            IndexPair pair = mesh.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: e);
            if (pair.I < 0 || pair.J < 0 || pair.I >= vertexSum.Length || pair.J >= vertexSum.Length || vertexCount[pair.I] == 0 || vertexCount[pair.J] == 0) continue;
            double endpointMean = ((vertexSum[pair.I] / vertexCount[pair.I]) + (vertexSum[pair.J] / vertexCount[pair.J])) * 0.5;
            double blend = edgeLengths[e] / Math.Max(val1: edgeLengths[e] + smoothingScale, val2: RhinoMath.SqrtEpsilon);
            edgeSignals[e] = (blend * edgeSignals[e]) + ((1.0 - blend) * endpointMean);
        }
        int finite = vertexCount.Count(static count => count > 0);
        return new FeatureCurvatureSignals(Edge: edgeSignals, FiniteVertices: finite, RejectedVertices: vertexCount.Length - finite);
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
        double numerator = 0.0, denominator = 0.0, sumRatio = 0.0, sumRatioSquared = 0.0;
        int comparable = 0;
        for (int edge = 0; edge < output.TopologyEdges.Count; edge++) {
            Line modelEdge = output.TopologyEdges.EdgeLine(topologyEdgeIndex: edge);
            IndexPair pair = output.TopologyEdges.GetTopologyVertices(topologyEdgeIndex: edge);
            if (!modelEdge.IsValid) continue;
            double modelLength = modelEdge.Length;
            if (!RhinoMath.IsValidDouble(x: modelLength) || modelLength <= RhinoMath.SqrtEpsilon) continue;
            int[] faces = output.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: edge);
            foreach (int faceIndex in faces)
                AccumulateUvEdge(mesh: output, uvs: uvs, faceIndex: faceIndex, pair: pair, modelLength: modelLength, numerator: ref numerator, denominator: ref denominator, sumRatio: ref sumRatio, sumRatioSquared: ref sumRatioSquared, comparable: ref comparable);
        }
        Option<double> distortion = Option<double>.None;
        if (denominator > RhinoMath.SqrtEpsilon && comparable > 0) {
            double scale = numerator / denominator;
            double rmsSquared = ((scale * scale * sumRatioSquared) - (2.0 * scale * sumRatio) + comparable) / comparable;
            double rms = Math.Sqrt(d: Math.Max(val1: 0.0, val2: rmsSquared));
            distortion = RhinoMath.IsValidDouble(x: scale) && scale > RhinoMath.SqrtEpsilon && RhinoMath.IsValidDouble(x: rms) ? Some(rms) : Option<double>.None;
        }
        return Fin.Succ(new FlattenResult(Uvs: uvs, Mesh: output, Receipt: new FlattenReceipt(VertexCount: output.Vertices.Count, UvCount: uvs.Count, TextureCoordinateCount: output.TextureCoordinates.Count, BoundaryComponents: boundaryComponents, NativeUnwrap: true, Valid: output.IsValid, EdgeLengthDistortionRms: distortion)));
        static void AccumulateUvEdge(Mesh mesh, Arr<Point2d> uvs, int faceIndex, IndexPair pair, double modelLength, ref double numerator, ref double denominator, ref double sumRatio, ref double sumRatioSquared, ref int comparable) {
            int[] topology = mesh.TopologyVertices.IndicesFromFace(faceIndex: faceIndex);
            MeshFace face = mesh.Faces[faceIndex];
            int count = face.IsQuad ? 4 : 3;
            if (topology.Length < count) return;
            for (int corner = 0; corner < count; corner++) {
                int next = (corner + 1) % count;
                bool matched = (topology[corner] == pair.I && topology[next] == pair.J) || (topology[corner] == pair.J && topology[next] == pair.I);
                if (!matched) continue;
                int a = FaceVertexAt(face: face, corner: corner);
                int b = FaceVertexAt(face: face, corner: next);
                if (a < 0 || b < 0 || a >= uvs.Count || b >= uvs.Count) return;
                Point2d uvA = uvs[index: a];
                Point2d uvB = uvs[index: b];
                double dx = uvA.X - uvB.X;
                double dy = uvA.Y - uvB.Y;
                double uvLength = Math.Sqrt(d: (dx * dx) + (dy * dy));
                if (!RhinoMath.IsValidDouble(x: uvLength) || uvLength <= RhinoMath.SqrtEpsilon) return;
                numerator += modelLength * uvLength;
                denominator += uvLength * uvLength;
                double ratio = uvLength / modelLength;
                sumRatio += ratio;
                sumRatioSquared += ratio * ratio;
                comparable++;
                return;
            }
        }
        static int FaceVertexAt(MeshFace face, int corner) {
            return corner switch {
                0 => face.A,
                1 => face.B,
                2 => face.C,
                _ => face.D,
            };
        }
    }
    // --- [HEAT_METHOD] ----------------------------------------------------------------------
    // Crane-Weischedel-Wardetzky 2013: solve (M + tL)u = δ; X = -∇u/|∇u|; Lφ = ∇·X.
    internal static Fin<double> HeatGeodesicAt(MeshSpace space, Seq<int> sources, Point3d sample, Op key) =>
        from distances in EnsureGeodesicDistances(space: space, sources: sources, key: key)
        from value in InterpolateOnMesh(space: space, sample: sample, perVertex: distances, key: key)
        select value;
    private static Fin<Arr<double>> EnsureGeodesicDistances(MeshSpace space, Seq<int> sources, Op key) {
        int n = space.Native.Vertices.Count;
        Seq<int> ordered = toSeq(sources.AsIterable().Distinct().Order());
        return ordered.IsEmpty || ordered.Exists(i => i < 0 || i >= n)
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : space.Cache.Geodesic(probe: new GeodesicKey(Sources: ordered),
                compute: () => from imesh in space.Cache.IntrinsicMeshSnapshot(key: key)
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

    internal static Fin<SignpostTransportReceipt> SignpostTransportReceiptOf(MeshSpace space, IntrinsicMesh imesh, Op key) =>
        ConnectionEntriesOf(space: space, imesh: imesh, edgeAdjustment: Option<Arr<double>>.None, key: key).Map(static entries => entries.Receipt);
    private static Fin<ConnectionEntries> EnumerateConnectionEntries(MeshSpace space, Option<Arr<double>> edgeAdjustment, Op key) =>
        space.Cache.IntrinsicMeshSnapshot(key: key).Bind(imesh => ConnectionEntriesOf(space: space, imesh: imesh, edgeAdjustment: edgeAdjustment, key: key));
    private static Fin<ConnectionEntries> ConnectionEntriesOf(MeshSpace space, IntrinsicMesh imesh, Option<Arr<double>> edgeAdjustment, Op key) {
        Mesh mesh = space.Native;
        FrameBundle fb = EnsureVertexFrames(mesh: mesh);
        int eCount = imesh.EdgeCount;
        bool hasAdjustment = edgeAdjustment.IsSome;
        Arr<double> adjustment = edgeAdjustment.IfNone(new Arr<double>([]));
        Arr<double> weights = SpectralCore.ComputeIntrinsicStar1(imesh: imesh);
        List<(int, int, double, double)> entries = new(capacity: eCount);
        int fallback = 0, missing = 0;
        double maxAngle = 0.0, maxResidual = 0.0;
        for (int e = 0; e < eCount; e++) {
            double w = weights[index: e];
            if (w < RhinoMath.ZeroTolerance) continue;
            IntrinsicEdge edge = imesh.EdgeAt(index: e);
            int i = edge.Lo, j = edge.Hi;
            Vector3d eij = (Vector3d)(mesh.Vertices[index: j] - mesh.Vertices[index: i]);
            double chordLength = eij.Length;
            double residual = Math.Abs(value: chordLength - edge.Length) / Math.Max(val1: edge.Length, val2: RhinoMath.SqrtEpsilon);
            if (!eij.IsValid || chordLength <= RhinoMath.ZeroTolerance) { missing++; continue; }
            fallback += imesh.HasFlips || residual > RhinoMath.SqrtEpsilon ? 1 : 0;
            double rho = EdgeAngleInFrame(edge: -eij, xAxis: fb.X[j], yAxis: fb.Y[j])
                       - EdgeAngleInFrame(edge: eij, xAxis: fb.X[i], yAxis: fb.Y[i]);
            if (hasAdjustment && e < adjustment.Count) rho += adjustment[index: e];
            if (!RhinoMath.IsValidDouble(x: rho)) { missing++; continue; }
            maxAngle = Math.Max(val1: maxAngle, val2: Math.Abs(value: Math.IEEERemainder(x: rho, y: RhinoMath.TwoPI)));
            maxResidual = Math.Max(val1: maxResidual, val2: residual);
            entries.Add(item: (i, j, w, rho));
        }
        SignpostTransportReceipt receipt = new(
            VertexCount: mesh.Vertices.Count,
            IntrinsicEdgeCount: eCount,
            TransportedEdgeCount: entries.Count,
            IntrinsicFlipCount: imesh.FlipCount,
            ChordFallbackEdges: fallback,
            MissingFrameEdges: missing,
            CommonSubdivisionSegments: 0,
            IntrinsicSnapshot: imesh.IsFrozen,
            ExactCommonSubdivision: false,
            MaxAngleRadians: maxAngle,
            MaxLengthResidual: maxResidual);
        return receipt.IsValid && entries.Count > 0
            ? Fin.Succ(new ConnectionEntries(Rows: toSeq(entries), Receipt: receipt))
            : Fin.Fail<ConnectionEntries>(key.InvalidResult());
    }

    private static Fin<SparseHermitian> BuildConnectionLaplacian(MeshSpace space, double symmetry, Option<Arr<double>> edgeAdjustment, Op key) =>
        from entries in EnumerateConnectionEntries(space: space, edgeAdjustment: edgeAdjustment, key: key)
        let n = space.Native.Vertices.Count
        let triplets = AssembleHermitianTriplets(entries: entries.Rows, symmetry: symmetry)
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
        let triplets = AssembleRealBlockTriplets(n: n, mass: laplacian.MassLumped, entries: entries.Rows, symmetry: symmetry, time: time)
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
        from cached in space.Cache.CrossField(probe: CrossFieldKey.Of(symmetry: symmetry, constraints: constraints, cones: cones),
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
            : from imesh in space.Cache.IntrinsicMeshSnapshot(key: key)
              from adjustment in SpectralCore.DistributeHolonomy(space: space, imesh: imesh, cones: cones.IfNone(toSeq<(int, double)>([])).Map(c => (c.Vertex, ConeIndex: c.HolonomyDeficit / (2.0 * Math.PI))), key: key)
              select Some(adjustment);
    private static Fin<Complex[]> SolveSmoothestCrossField(MeshSpace space, int symmetry, Option<Arr<double>> edgeAdjustment, Op key) =>
        BuildConnectionLaplacian(space: space, symmetry: symmetry, edgeAdjustment: edgeAdjustment, key: key)
            .Bind(Lconn => Lconn.SmallestEigenpairsDetailed(k: 1, tolerance: 1e-6, maxIterations: 200, key: key)
                .Bind(receipt => receipt.Stop.Equals(EigenSolveStop.ResidualConverged) ? Fin.Succ(receipt.Pairs) : Fin.Fail<Seq<(double Eigenvalue, Arr<Complex> Eigenvector)>>(key.InvalidResult())))
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
            quadCase: static (state, quad) => {
                Mesh? result = state.Space.Native.QuadRemesh(parameters: new QuadRemeshParameters { TargetEdgeLength = quad.TargetLength.Value, AdaptiveSize = 0.5, DetectHardEdges = true });
                if (result is { IsValid: true })
                    return Fin.Succ(new RemeshResult(Mesh: result, Receipt: RemeshReceiptOf(kind: quad, source: state.Space.Native, output: result, targetLength: Some(quad.TargetLength.Value), desiredPolygonCount: Option<int>.None, hardEdges: true)));
                result?.Dispose();
                return Fin.Fail<RemeshResult>(error: state.Key.InvalidResult());
            },
            simplifyCase: static (state, simplify) => {
                Mesh clone = state.Space.Native.DuplicateMesh();
                if (clone.Reduce(parameters: simplify.Parameters) && clone.IsValid)
                    return Fin.Succ(new RemeshResult(Mesh: clone, Receipt: RemeshReceiptOf(kind: simplify, source: state.Space.Native, output: clone, targetLength: Option<double>.None, desiredPolygonCount: Some(simplify.Parameters.DesiredPolygonCount), hardEdges: false)));
                clone.Dispose();
                return Fin.Fail<RemeshResult>(error: state.Key.InvalidResult());
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
                    from descriptor in DescribeSpectralShape(space: state.Space, spec: spec, eigenpairs: state.Eigenpairs, includeAssembly: typeof(TOut) == typeof(DescriptorResult) || typeof(TOut) == typeof(DescriptorReceipt), key: state.Key)
                    from output in ProjectDescriptor<TOut>(descriptor: descriptor, key: state.Key)
                    select output)));
    internal static Fin<DescriptorResult> DescribeSpectralShape(MeshSpace space, MeshDescriptor.SpectralCase spec, int eigenpairs, Op key) =>
        DescribeSpectralShape(space: space, spec: spec, eigenpairs: eigenpairs, includeAssembly: false, key: key);
    private static Fin<DescriptorResult> DescribeSpectralShape(MeshSpace space, MeshDescriptor.SpectralCase spec, int eigenpairs, bool includeAssembly, Op key) =>
        from bundle in space.Cache.SpectralBasisBundleOf(k: eigenpairs, key: key)
        from spectral in spec.Filter.ApplyDetailed(basis: bundle.Basis, sources: spec.Sources, policy: spec.Policy, key: key)
        from assembly in includeAssembly ? SpectralCore.Build(space: space, key: key).Map(calculus => Some(calculus.Receipt)) : Fin.Succ(Option<SpectralAssemblyReceipt>.None)
        select new DescriptorResult(Values: spectral.Values, Receipt: new DescriptorReceipt(Spectral: spectral.Receipt, Eigen: bundle.Eigen, RequestedEigenpairs: eigenpairs, ReturnedEigenpairs: bundle.Eigen.ReturnedPairs, SpectralCacheHit: bundle.CacheHit, SkippedDegenerateFaces: bundle.SkippedDegenerateFaces, FactorNonZeros: bundle.FactorNonZeros, Assembly: assembly));
    private static Fin<TOut> ProjectDescriptor<TOut>(DescriptorResult descriptor, Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(DescriptorResult) => Fin.Succ((TOut)(object)descriptor),
            Type t when t == typeof(DescriptorReceipt) => Fin.Succ((TOut)(object)descriptor.Receipt),
            Type t when t == typeof(SpectralDescriptor) => Fin.Succ((TOut)(object)new SpectralDescriptor(Values: descriptor.Values, Receipt: descriptor.Receipt.Spectral)),
            Type t when t == typeof(SpectralDescriptorReceipt) => Fin.Succ((TOut)(object)descriptor.Receipt.Spectral),
            Type t when t == typeof(Arr<double>) => Fin.Succ((TOut)(object)descriptor.Values),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(MeshDescriptor.SpectralCase), outputType: typeof(TOut))),
        };

    internal static Fin<double> SpectralDistanceAt(MeshSpace space, SpectralFilter filter, Seq<int> sources, int pairs, Point3d sample, Op key) =>
        from bundle in space.Cache.SpectralBasisBundleOf(k: pairs, key: key)
        from descriptor in filter.ApplyDetailed(basis: bundle.Basis, sources: sources.IsEmpty ? Option<Seq<int>>.None : Some(sources), key: key)
        from interpolated in InterpolateOnMesh(space: space, sample: sample, perVertex: descriptor.Values, key: key)
        select interpolated;

    // --- [SEGMENTATION] ---------------------------------------------------------------------
    internal static Fin<TOut> Segment<TOut>(MeshSpace space, MeshSegmentation kind, Op key) =>
        Optional(kind).ToFin(key.InvalidInput()).Bind(active =>
            active.Switch(
                state: (Space: space, Key: key),
                scalarThresholdCase: static (state, threshold) => SegmentThreshold(space: state.Space, kind: threshold, key: state.Key),
                scalarBandsCase: static (state, bands) => SegmentBands(space: state.Space, kind: bands, key: state.Key),
                seededRegionGrowCase: static (state, grow) => SegmentRegionGrow(space: state.Space, kind: grow, key: state.Key),
                descriptorClustersCase: static (state, clusters) => SegmentDescriptorClusters(space: state.Space, kind: clusters, key: state.Key),
                watershedCase: static (state, watershed) => SegmentWatershed(space: state.Space, kind: watershed, key: state.Key),
                normalizedCutCase: static (state, cut) => SegmentNormalizedCut(space: state.Space, kind: cut, key: state.Key))
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
    private static Fin<MeshSegmentationResult> SegmentWatershed(MeshSpace space, MeshSegmentation.WatershedCase kind, Op key) =>
        from scalars in SegmentationScalarsOf(mesh: space.Native, values: kind.Values, valuesAreVertices: kind.ValuesAreVertices, key: key)
        from _ in scalars.FiniteCount == 0 ? Fin.Fail<Unit>(key.InvalidInput()) : Fin.Succ(unit)
        let basins = WatershedLabels(mesh: space.Native, scalars: scalars.FaceValues, mergeTolerance: kind.MergeTolerance.Value)
        select SegmentationResultOf(mesh: space.Native, faceRegions: basins.Regions, scalars: scalars, run: new SegmentationRun(Algorithm: MeshSegmentationAlgorithm.WatershedBasins, RequestedRegionCount: basins.SeedCount, SeedCount: basins.SeedCount, Status: MeshSegmentationStatus.Completed, Iterations: Option<int>.None, MaxIterations: Option<int>.None, Tolerance: Some(kind.MergeTolerance.Value), Threshold: Option<double>.None, Descriptor: Option<DescriptorReceipt>.None, WatershedSaddleCount: Some(basins.SaddleCount)));
    private static Fin<MeshSegmentationResult> SegmentNormalizedCut(MeshSpace space, MeshSegmentation.NormalizedCutCase kind, Op key) =>
        from scalars in SegmentationScalarsOf(mesh: space.Native, values: kind.Values, valuesAreVertices: kind.ValuesAreVertices, key: key)
        from _ in scalars.FiniteCount < kind.RegionCount.Value ? Fin.Fail<Unit>(key.InvalidInput()) : Fin.Succ(unit)
        from system in NormalizedCutSystemOf(mesh: space.Native, scalars: scalars.FaceValues, tolerance: kind.Tolerance.Value, key: key)
        from eigen in MatrixKernel.GeneralizedEigenpairsDetailed(stiffness: system.Laplacian, mass: system.Degree, k: Math.Min(val1: kind.Eigenpairs.Value, val2: Math.Max(val1: 1, val2: space.Native.Faces.Count - 1)), key: key)
        from projection in NormalizedCutProjection(eigen: eigen, expectedCount: scalars.FaceValues.Count, key: key)
        from clusters in ClusterLabels(values: projection, count: kind.RegionCount.Value, maxIterations: kind.MaxIterations.Value, tolerance: kind.Tolerance.Value, key: key)
        let labels = ConnectedComponents(mesh: space.Native, buckets: clusters.Labels)
        let value = NormalizedCutValue(mesh: space.Native, scalars: scalars.FaceValues, labels: labels, sigma: system.Sigma)
        select SegmentationResultOf(mesh: space.Native, faceRegions: labels, scalars: scalars, run: new SegmentationRun(Algorithm: MeshSegmentationAlgorithm.NormalizedCut, RequestedRegionCount: kind.RegionCount.Value, SeedCount: 0, Status: clusters.Converged ? MeshSegmentationStatus.Completed : MeshSegmentationStatus.MaxIterationsExhausted, Iterations: Some(clusters.Iterations), MaxIterations: Some(kind.MaxIterations.Value), Tolerance: Some(kind.Tolerance.Value), Threshold: Option<double>.None, Descriptor: Option<DescriptorReceipt>.None, FactorNonZeros: eigen.FactorNonZeros, NormalizedCutValue: Some(value), AffinityNonZeros: Some(system.AffinityNonZeros), Eigen: Some(eigen)));
    private static Fin<TOut> ProjectSegmentation<TOut>(MeshSegmentationResult result, Op key) => typeof(TOut) switch { Type t when t == typeof(MeshSegmentationResult) => Fin.Succ((TOut)(object)result), Type t when t == typeof(MeshSegmentationReceipt) => Fin.Succ((TOut)(object)result.Receipt), Type t when t == typeof(Arr<int>) => Fin.Succ((TOut)(object)result.FaceRegions), _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(MeshSegmentation), outputType: typeof(TOut))) };
    private static MeshSegmentationResult SegmentationComponentsOf(Mesh mesh, SegmentationScalars scalars, Func<double, int> bucket, SegmentationRun run) =>
        SegmentationResultOf(mesh: mesh, faceRegions: ConnectedComponents(mesh: mesh, buckets: BucketsOf(values: scalars.FaceValues, bucket: bucket)), scalars: scalars, run: run);
    private static MeshSegmentationResult SegmentationResultOf(Mesh mesh, int[] faceRegions, SegmentationScalars scalars, SegmentationRun run) {
        Arr<int> faces = new(faceRegions);
        int assigned = faceRegions.Count(static label => label >= 0);
        int regionCount = faceRegions.Where(static label => label >= 0).Distinct().Count();
        MeshSegmentationReceipt receipt = new(Algorithm: run.Algorithm, Status: run.Status, RequestedRegionCount: run.RequestedRegionCount, RegionCount: regionCount, SeedCount: run.SeedCount, AssignedFaceCount: assigned, UnassignedFaceCount: faceRegions.Length - assigned, SkippedDegenerateFaces: scalars.SkippedDegenerateFaces, SkippedNonFiniteValues: scalars.SkippedNonFiniteValues, Iterations: run.Iterations, MaxIterations: run.MaxIterations, Tolerance: run.Tolerance, Threshold: run.Threshold, Descriptor: run.Descriptor, Solve: run.Solve, SpectralCacheHit: run.Descriptor.Map(static receipt => receipt.SpectralCacheHit), FactorCacheHit: run.FactorCacheHit, FactorNonZeros: run.FactorNonZeros.IsSome ? run.FactorNonZeros : run.Descriptor.Bind(static receipt => receipt.FactorNonZeros), NormalizedCutValue: run.NormalizedCutValue, AffinityNonZeros: run.AffinityNonZeros, WatershedSaddleCount: run.WatershedSaddleCount, Eigen: run.Eigen);
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
        return [.. adjacency.Select(static faces => faces.Distinct().Order().ToArray())];
    }
    private static WatershedState WatershedLabels(Mesh mesh, Arr<double> scalars, double mergeTolerance) {
        int faceCount = mesh.Faces.Count;
        int[][] adjacency = FaceAdjacencyOf(mesh: mesh);
        int[] regions = [.. Enumerable.Repeat(element: UnassignedRegion, count: faceCount)];
        int[] parent = [.. Enumerable.Repeat(element: UnassignedRegion, count: faceCount)];
        double[] seedValue = [.. Enumerable.Repeat(element: double.NaN, count: faceCount)];
        int seedCount = 0, saddleCount = 0;
        int Find(int region) {
            int root = region;
            while (parent[root] != root) root = parent[root];
            while (parent[region] != region) {
                int next = parent[region];
                parent[region] = root;
                region = next;
            }
            return root;
        }
        void Union(int a, int b) {
            int ra = Find(region: a), rb = Find(region: b);
            if (ra == rb) return;
            int keep = seedValue[ra] <= seedValue[rb] ? ra : rb;
            int drop = keep == ra ? rb : ra;
            parent[drop] = keep;
            seedValue[keep] = Math.Min(val1: seedValue[keep], val2: seedValue[drop]);
        }
        foreach (int face in Enumerable.Range(start: 0, count: faceCount).Where(i => RhinoMath.IsValidDouble(x: scalars[index: i])).OrderBy(i => scalars[index: i]).ThenBy(static i => i)) {
            int[] neighbors = [.. adjacency[face].Select(n => regions[n]).Where(static region => region >= 0).Select(Find).Distinct().Order()];
            if (neighbors.Length == 0) {
                parent[seedCount] = seedCount;
                seedValue[seedCount] = scalars[index: face];
                regions[face] = seedCount;
                seedCount++;
                continue;
            }
            int best = neighbors.OrderBy(region => seedValue[Find(region: region)]).ThenBy(static region => region).First();
            regions[face] = best;
            for (int i = 0; i < neighbors.Length; i++) {
                int other = neighbors[i];
                if (Find(region: other) == Find(region: best)) continue;
                if (Math.Abs(value: seedValue[Find(region: other)] - seedValue[Find(region: best)]) <= mergeTolerance) Union(a: best, b: other);
                else saddleCount++;
            }
            regions[face] = Find(region: best);
        }
        Dictionary<int, int> dense = new(capacity: seedCount);
        int nextRegion = 0;
        for (int f = 0; f < regions.Length; f++) {
            if (regions[f] < 0) continue;
            int root = Find(region: regions[f]);
            if (!dense.TryGetValue(key: root, value: out int denseRegion)) {
                denseRegion = nextRegion++;
                dense.Add(key: root, value: denseRegion);
            }
            regions[f] = denseRegion;
        }
        return new WatershedState(Regions: regions, SeedCount: seedCount, SaddleCount: saddleCount);
    }
    private static Fin<NormalizedCutSystem> NormalizedCutSystemOf(Mesh mesh, Arr<double> scalars, double tolerance, Op key) {
        int faceCount = mesh.Faces.Count;
        int[][] adjacency = FaceAdjacencyOf(mesh: mesh);
        double[] degree = new double[faceCount];
        bool hasFinite = false;
        double min = double.PositiveInfinity, max = double.NegativeInfinity;
        for (int i = 0; i < scalars.Count; i++) {
            double value = scalars[index: i];
            if (!RhinoMath.IsValidDouble(x: value)) continue;
            hasFinite = true;
            min = Math.Min(val1: min, val2: value);
            max = Math.Max(val1: max, val2: value);
        }
        double range = hasFinite ? Math.Max(val1: max - min, val2: tolerance) : tolerance;
        double sigma = Math.Max(val1: tolerance, val2: range / Math.Max(val1: 1.0, val2: Math.Sqrt(d: faceCount)));
        List<(int Row, int Col, double Value)> laplacian = new(capacity: faceCount * 5), mass = new(capacity: faceCount);
        int affinities = 0;
        for (int f = 0; f < faceCount; f++) {
            double vf = scalars[index: f];
            if (!RhinoMath.IsValidDouble(x: vf)) continue;
            for (int i = 0; i < adjacency[f].Length; i++) {
                int n = adjacency[f][i];
                if (n <= f) continue;
                double vn = scalars[index: n];
                if (!RhinoMath.IsValidDouble(x: vn)) continue;
                double diff = vf - vn;
                double weight = Math.Exp(d: -(diff * diff) / (2.0 * sigma * sigma));
                if (!RhinoMath.IsValidDouble(x: weight) || weight <= RhinoMath.SqrtEpsilon) continue;
                laplacian.Add(item: (f, n, -weight));
                laplacian.Add(item: (n, f, -weight));
                degree[f] += weight;
                degree[n] += weight;
                affinities += 2;
            }
        }
        for (int f = 0; f < faceCount; f++) {
            double d = degree[f] > RhinoMath.SqrtEpsilon ? degree[f] : 1.0;
            laplacian.Add(item: (f, f, degree[f]));
            mass.Add(item: (f, f, d));
        }
        Dimension dim = Dimension.Create(value: faceCount);
        return affinities == 0
            ? Fin.Fail<NormalizedCutSystem>(key.InvalidInput())
            : from stiffness in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: laplacian, key: key)
              from degreeMatrix in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: mass, key: key)
              select new NormalizedCutSystem(Laplacian: stiffness, Degree: degreeMatrix, AffinityNonZeros: affinities, Sigma: sigma);
    }
    private static Fin<Arr<double>> NormalizedCutProjection(EigenSolveReceipt<double, Arr<double>> eigen, int expectedCount, Op key) {
        (double Eigenvalue, Arr<double> Eigenvector)[] pairs = [.. eigen.Pairs.AsIterable()];
        Arr<double> vector = pairs.Length > 1 ? pairs[1].Eigenvector : pairs.Length == 1 ? pairs[0].Eigenvector : [];
        return eigen.IsUsable && vector.Count == expectedCount && vector.ForAll(RhinoMath.IsValidDouble)
            ? Fin.Succ(vector)
            : Fin.Fail<Arr<double>>(key.InvalidResult());
    }
    private static double NormalizedCutValue(Mesh mesh, Arr<double> scalars, int[] labels, double sigma) {
        int maxRegion = labels.Where(static label => label >= 0).DefaultIfEmpty(defaultValue: -1).Max();
        if (maxRegion < 0) return 0.0;
        int[][] adjacency = FaceAdjacencyOf(mesh: mesh);
        double[] assoc = new double[maxRegion + 1], cut = new double[maxRegion + 1];
        for (int f = 0; f < labels.Length; f++) {
            int lf = labels[f];
            double vf = scalars[index: f];
            if (lf < 0 || !RhinoMath.IsValidDouble(x: vf)) continue;
            for (int i = 0; i < adjacency[f].Length; i++) {
                int n = adjacency[f][i];
                if (n <= f || n >= labels.Length) continue;
                int ln = labels[n];
                double vn = scalars[index: n];
                if (ln < 0 || !RhinoMath.IsValidDouble(x: vn)) continue;
                double diff = vf - vn;
                double weight = Math.Exp(d: -(diff * diff) / (2.0 * sigma * sigma));
                if (!RhinoMath.IsValidDouble(x: weight) || weight <= RhinoMath.SqrtEpsilon) continue;
                assoc[lf] += weight;
                assoc[ln] += weight;
                if (lf == ln) continue;
                cut[lf] += weight;
                cut[ln] += weight;
            }
        }
        double value = 0.0;
        for (int region = 0; region < assoc.Length; region++)
            if (assoc[region] > RhinoMath.SqrtEpsilon) value += cut[region] / assoc[region];
        return RhinoMath.IsValidDouble(x: value) ? value : double.PositiveInfinity;
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

    // --- [MESH_SPECTRUM_SAMPLING] ----------------------------------------------------------
    internal static Fin<SampleResult> ValidateSamplingSpectrum(MeshSpace space, SampleResult result, Op key) =>
        result.Points.IsEmpty || result.Receipt.Algorithm.IsNone || space.Native.Vertices.Count < 3
            ? Fin.Succ(result)
            : (from bundle in space.Cache.SpectralBasisBundleOf(k: Math.Min(val1: 8, val2: Math.Max(val1: 1, val2: space.Native.Vertices.Count - 1)), key: key)
               from receipt in SamplingSpectrumReceiptOf(space: space, points: result.Points, basis: bundle.Basis, key: key)
               select result with {
                   Receipt = result.Receipt with {
                       Algorithm = result.Receipt.Algorithm.Map(algorithm => algorithm with {
                           MeshSpectrumValidated = receipt.Validated,
                           Spectrum = Some(receipt),
                       }),
                   },
               }).Match(Succ: Fin.Succ, Fail: _ => Fin.Succ(result));
    private static Fin<MeshSamplingSpectrumReceipt> SamplingSpectrumReceiptOf(MeshSpace space, Seq<Point3d> points, SpectralBasis basis, Op key) {
        int vertexCount = space.Native.Vertices.Count;
        if (basis.Eigenvectors.Count == 0 || points.IsEmpty) return Fin.Fail<MeshSamplingSpectrumReceipt>(key.InvalidInput());
        double[] indicator = new double[vertexCount];
        double searchDistance = MeshSearchDistance(space: space);
        for (int i = 0; i < points.Count; i++) {
            MeshPoint meshPoint = space.Native.ClosestMeshPoint(testPoint: points[index: i], maximumDistance: searchDistance);
            if (meshPoint is null || meshPoint.FaceIndex < 0 || meshPoint.FaceIndex >= space.Native.Faces.Count) return Fin.Fail<MeshSamplingSpectrumReceipt>(key.InvalidResult());
            MeshFace face = space.Native.Faces[index: meshPoint.FaceIndex];
            double[] weights = meshPoint.T;
            if (weights.Length < 3 || (face.IsQuad && weights.Length < 4)) return Fin.Fail<MeshSamplingSpectrumReceipt>(key.InvalidResult());
            indicator[face.A] += weights[0];
            indicator[face.B] += weights[1];
            indicator[face.C] += weights[2];
            if (face.IsQuad) indicator[face.D] += weights[3];
        }
        double low = 0.0, total = 0.0;
        int lowLimit = Math.Min(val1: 3, val2: basis.Eigenvectors.Count);
        for (int mode = 0; mode < basis.Eigenvectors.Count; mode++) {
            Arr<double> eigenvector = basis.Eigenvectors[index: mode];
            if (eigenvector.Count != vertexCount) return Fin.Fail<MeshSamplingSpectrumReceipt>(key.InvalidResult());
            double coefficient = 0.0;
            for (int v = 0; v < vertexCount; v++) coefficient += indicator[v] * eigenvector[index: v];
            double energy = coefficient * coefficient;
            if (!RhinoMath.IsValidDouble(x: energy)) return Fin.Fail<MeshSamplingSpectrumReceipt>(key.InvalidResult());
            total += energy;
            if (mode < lowLimit) low += energy;
        }
        double ratio = total > RhinoMath.SqrtEpsilon ? low / total : 1.0;
        double boundedRatio = Math.Max(val1: 0.0, val2: Math.Min(val1: 1.0, val2: ratio));
        MeshSamplingSpectrumReceipt receipt = new(VertexCount: vertexCount, SampleCount: points.Count, EigenpairCount: basis.Eigenvectors.Count, LowFrequencyEnergy: low, TotalEnergy: total, SuppressionRatio: boundedRatio, ValidationThreshold: MeshSpectrumLowFrequencyCeiling, Validated: total > RhinoMath.SqrtEpsilon && RhinoMath.IsValidDouble(x: ratio) && boundedRatio <= MeshSpectrumLowFrequencyCeiling, Algorithm: MeshSamplingSpectrumAlgorithm.CandidateSpectrum);
        return receipt.IsValid ? Fin.Succ(receipt) : Fin.Fail<MeshSamplingSpectrumReceipt>(key.InvalidResult());
    }

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
        return from topology in TopologyDetailed(space: space, key: key)
               from _ in topology.Genus.Match(
                   Some: genus => genus > 0
                       ? Fin.Fail<Unit>(key.Unsupported(geometryType: typeof(MeshSpace), outputType: typeof(HodgeBundle)))
                       : Fin.Succ(unit),
                   None: () => Fin.Fail<Unit>(key.Unsupported(geometryType: typeof(MeshSpace), outputType: typeof(HodgeBundle))))
               from bundle in toSeq(Enumerable.Range(start: 0, count: nEdges)).Fold(
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
            .Bind(potential => BuildHodgeFromPotential(mesh: mesh, source: source, potential: potential, space: space, key: key))
               select bundle;
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
        return from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
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

    // --- [TANGENT_LOG_MAP] ------------------------------------------------------------------
    // Vector-heat-backed logarithm approximation: heat geodesic magnitude plus transported source tangent direction.
    internal static Fin<TangentLogMapResult> TangentLogMapAt(MeshSpace space, int source, Point3d sample, double time, Op key) {
        int n = space.Native.Vertices.Count;
        if (source < 0 || source >= n || !RhinoMath.IsValidDouble(x: time) || time <= 0.0) return Fin.Fail<TangentLogMapResult>(key.InvalidInput());
        FrameBundle frames = EnsureVertexFrames(mesh: space.Native);
        Point3d sourcePoint = space.Native.Vertices[index: source];
        Vector3d sourceDirection = sample - sourcePoint;
        sourceDirection -= sourceDirection * frames.N[source] * frames.N[source];
        if (!sourceDirection.Unitize()) sourceDirection = frames.X[source];
        return from distances in EnsureGeodesicDistances(space: space, sources: Seq(source), key: key)
               from distance in InterpolateOnMesh(space: space, sample: sample, perVertex: distances, key: key)
               from transported in VectorHeatAt(space: space, sources: toSeq([(Vertex: source, Direction: sourceDirection)]), time: time, sample: sample, key: key)
               from tangent in distance <= space.Tolerance.Absolute.Value ? key.AcceptValue(value: Vector3d.Zero) : TransportedLog(value: transported, scale: distance)
               let residual = Math.Abs(value: tangent.Length - distance)
               select new TangentLogMapResult(Tangent: tangent, Receipt: new TangentLogMapReceipt(SourceVertex: source, TargetCount: 1, HeatTime: time, VectorHeatBacked: true, RejectsFlippedIntrinsic: true, FiniteLogCount: tangent.IsValid && RhinoMath.IsValidDouble(x: tangent.Length) ? 1 : 0, MaxMagnitudeResidual: residual, Algorithm: TangentLogMapAlgorithm.VectorHeatApproximate));
        Fin<Vector3d> TransportedLog(Vector3d value, double scale) {
            Vector3d tangent = value;
            return tangent.IsValid && RhinoMath.IsValidDouble(x: scale) && scale >= 0.0 && tangent.Unitize()
                ? key.AcceptValue(value: scale * tangent)
                : Fin.Fail<Vector3d>(key.InvalidResult());
        }
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
    internal static Fin<SdfMeshPolicy> AdmitSignedDistanceMeshPolicy(MeshSpace space, SdfMeshPolicy policy, Op key) =>
        from _ in FieldNabla.MeshNative(space: space, key: key)
        from active in policy.Admit(key: key)
        select active;
    internal static Fin<SdfMeshSample> SignedDistanceFromMeshDetailed(MeshSpace space, SdfMeshPolicy policy, Point3d sample, Op key) =>
        AdmitSignedDistanceMeshPolicy(space: space, policy: policy, key: key).Bind(active => active.Method switch {
            SdfMeshMethod method when method.Equals(SdfMeshMethod.ClosedSurfaceSignedHeat) => ClosedSignedHeatDistanceDetailed(space: space, policy: active, sample: sample, key: key).Bind(result => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Some(result.Solution.Receipt), key: key, topology: Some(result.Solution.Topology), volumeGrid: Some(result.Solution.Domain.Receipt)).Map(receipt => new SdfMeshSample(Distance: result.Distance, Receipt: receipt))),
            SdfMeshMethod method when method.Equals(SdfMeshMethod.BoundarySignedHeat) => SignedHeatDistanceDetailed(space: space, policy: active, sample: sample, key: key).Bind(result => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Some(result.Solution.Receipt), key: key, topology: Some(result.Solution.Topology)).Map(receipt => new SdfMeshSample(Distance: result.Distance, Receipt: receipt))),
            SdfMeshMethod method when method.Equals(SdfMeshMethod.GeneralizedWindingNumber) => GeneralizedWindingDistance(space: space, sample: sample, key: key).Bind(distance => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Option<SignedHeatReceipt>.None, key: key).Map(receipt => new SdfMeshSample(Distance: active.SignConvention.Multiplier * distance, Receipt: receipt))),
            _ => Fin.Fail<SdfMeshSample>(key.InvalidInput()),
        });
    internal static Fin<SdfMeshReceipt> PrewarmSignedDistanceEvaluator(MeshSpace space, SdfMeshPolicy policy, Op key) =>
        AdmitSignedDistanceMeshPolicy(space: space, policy: policy, key: key).Bind(active => active.Method switch {
            SdfMeshMethod method when method.Equals(SdfMeshMethod.ClosedSurfaceSignedHeat) => space.Cache.ClosedSignedHeatDetailed(policy: active, key: key).Bind(solution => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Some(solution.Receipt), key: key, topology: Some(solution.Topology), volumeGrid: Some(solution.Domain.Receipt))),
            SdfMeshMethod method when method.Equals(SdfMeshMethod.BoundarySignedHeat) => space.Cache.SignedHeatDetailed(policy: active, key: key).Bind(solution => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Some(solution.Receipt), key: key, topology: Some(solution.Topology))),
            SdfMeshMethod method when method.Equals(SdfMeshMethod.GeneralizedWindingNumber) => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Option<SignedHeatReceipt>.None, key: key),
            _ => Fin.Fail<SdfMeshReceipt>(key.InvalidInput()),
        });
    private static Fin<SdfMeshReceipt> SdfMeshReceiptOf(MeshSpace space, SdfMeshPolicy policy, Option<SignedHeatReceipt> signedHeat, Op key, Option<TopologyReceipt> topology = default, Option<VolumeGridReceipt> volumeGrid = default) =>
        topology.Match(Some: static receipt => Fin.Succ(receipt), None: () => TopologyDetailed(space: space, key: key))
            .Map(topology => new SdfMeshReceipt(Method: policy.Method, Status: policy.Method.Status, Domain: policy.Method.Domain, Topology: topology, SignedHeat: signedHeat, VolumeGrid: volumeGrid));
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
    private static Fin<(double Distance, SignedHeatSolution Solution)> SignedHeatDistanceDetailed(MeshSpace space, SdfMeshPolicy policy, Point3d sample, Op key) =>
        from solution in space.Cache.SignedHeatDetailed(policy: policy, key: key)
        from signed in InterpolateOnMesh(space: space, sample: sample, perVertex: solution.Values, key: key)
        select (policy.SignConvention.Multiplier * signed, solution);
    private static Fin<(double Distance, ClosedSignedHeatSolution Solution)> ClosedSignedHeatDistanceDetailed(MeshSpace space, SdfMeshPolicy policy, Point3d sample, Op key) =>
        from solution in space.Cache.ClosedSignedHeatDetailed(policy: policy, key: key)
        from signed in InterpolateVolumeGrid(domain: solution.Domain, values: solution.Values, sample: sample, key: key)
        select (policy.SignConvention.Multiplier * signed, solution);
    // Boundary-source signed heat rejects flipped IDT until CR signpost transfer exists.
    internal static Fin<Arr<double>> ComputeSignedHeat(MeshSpace space, SdfMeshPolicy policy, Op key) => ComputeSignedHeatDetailed(space: space, policy: policy, key: key).Map(static result => result.Values);
    internal static Fin<SignedHeatSolution> ComputeSignedHeatDetailed(MeshSpace space, SdfMeshPolicy policy, Op key) {
        Mesh mesh = space.Native;
        double h = space.Cache.MeanEdgeLength;
        if (h <= RhinoMath.ZeroTolerance) return Fin.Fail<SignedHeatSolution>(error: key.InvalidResult());
        double t = policy.Heat.Resolve(cellSize: 0.5 * h);
        return from imesh in space.Cache.IntrinsicMeshSnapshot(key: key)
               from _ in guard(!imesh.HasFlips, key.Unsupported(geometryType: typeof(IntrinsicMesh), outputType: typeof(SignedHeatSolution)))
               from admitted in AdmitBoundarySignedHeat(space: space, imesh: imesh, key: key)
               from heatFactor in space.Cache.EdgeConnectionCholeskyDetailed(time: t, key: key)
               from heatSolve in heatFactor.Factor.SolveDetailed(rhs: admitted.Source.Rhs, key: key)
               let faceField = SpectralCore.SampleCrouzeixRaviartFaceField(mesh: mesh, imesh: imesh, stacked: heatSolve.Solution)
               let divergence = SpectralCore.ComputeIntrinsicVertexDivergence(mesh: mesh, imesh: imesh, faceFields: faceField)
               from poissonFactor in space.Cache.Cholesky(key: key)
               from poissonSolve in poissonFactor.SolveDetailed(rhs: divergence, key: key)
               from residuals in heatSolve.Residual <= policy.Solver.ResidualTolerance.Value && poissonSolve.Residual <= policy.Solver.ResidualTolerance.Value ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
               from shifted in ShiftSignedHeat(phi: poissonSolve.Solution, sourceVertices: admitted.Source.SourceVertices, vertexCount: mesh.Vertices.Count, key: key)
               select new SignedHeatSolution(
                   Values: shifted,
                   Receipt: new SignedHeatReceipt(BoundarySourceVertexCount: admitted.Source.SourceVertices.Count, BoundaryEncodedEdgeSourceCount: admitted.Source.EncodedEdgeSourceCount, BoundaryRejectedPointCount: admitted.Source.RejectedBoundaryPointCount, BoundaryUnmatchedSegmentCount: admitted.Source.UnmatchedBoundarySegmentCount, HeatSolve: Some(heatSolve), PoissonSolve: poissonSolve, EdgeAssembly: Some(heatFactor.Receipt)),
                   Topology: admitted.Topology);
    }
    internal static Fin<ClosedSignedHeatSolution> ComputeClosedSignedHeatDetailed(MeshSpace space, SdfMeshPolicy policy, Op key) =>
        from admitted in AdmitClosedSignedHeat(space: space, policy: policy, key: key)
        from gridPolicy in policy.Grid.ToFin(key.InvalidInput())
        from domain in VolumeGridDomainOf(source: admitted.Bounds, grid: gridPolicy, key: key)
        let heatTime = policy.Heat.Resolve(cellSize: domain.CellSize)
        from _ in RhinoMath.IsValidDouble(x: heatTime) && heatTime > RhinoMath.ZeroTolerance ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput())
        from sources in VolumeSourcesOf(mesh: space.Native, normalSign: admitted.NormalSign, key: key)
        from vectors in VolumeGridVectorsOf(mesh: space.Native, domain: domain, sources: sources, heatTime: heatTime, tolerance: space.Tolerance.Absolute.Value, key: key)
        from system in AssembleVolumeGridPoisson(domain: domain, vectors: vectors, key: key)
        from solve in CholeskySparse.Of(symmetric: system.Operator, key: key).Bind(factor => factor.SolveDetailed(rhs: system.Rhs, key: key))
        from __ in solve.Residual <= policy.Solver.ResidualTolerance.Value ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
        from calibrated in CalibrateClosedSignedHeat(domain: domain, raw: solve.Solution, sources: sources, interiorIndex: vectors.InteriorIndex, key: key)
        let receipt = new VolumeGridReceipt(Bounds: domain.Bounds, Resolution: domain.Resolution, XNodes: domain.XNodes, YNodes: domain.YNodes, ZNodes: domain.ZNodes, CellSize: domain.CellSize, Padding: domain.Padding, NodeCount: domain.NodeCount, CellCount: domain.CellCount, SourceTriangleCount: sources.Sources.Length, DegenerateTriangleCount: sources.Degenerate, SourceArea: sources.Area, InsideNodeCount: vectors.Inside, OutsideNodeCount: vectors.Outside, NearSurfaceNodeCount: vectors.NearSurface, RejectedVectorCount: vectors.Rejected, HeatTime: heatTime, GaugeNode: system.GaugeNode, SurfaceShift: calibrated.Shift, Interpolation: policy.Interpolation, BoundaryCondition: policy.BoundaryCondition, Solver: policy.Solver, OperatorNonZeros: system.Operator.NonZeros, FactorNonZeros: solve.FactorNonZeros, Residual: solve.Residual)
        let solvedDomain = domain with { Receipt = receipt }
        select new ClosedSignedHeatSolution(Domain: solvedDomain, Values: calibrated.Values, Receipt: new SignedHeatReceipt(BoundarySourceVertexCount: 0, BoundaryEncodedEdgeSourceCount: 0, BoundaryRejectedPointCount: 0, BoundaryUnmatchedSegmentCount: 0, HeatSolve: Option<SolveReceipt>.None, PoissonSolve: solve), Topology: admitted.Topology);
    private static Fin<(TopologyReceipt Topology, BoundingBox Bounds, double NormalSign)> AdmitClosedSignedHeat(MeshSpace space, SdfMeshPolicy policy, Op key) =>
        from topology in TopologyDetailed(space: space, key: key)
        from _ in policy.Method.Equals(SdfMeshMethod.ClosedSurfaceSignedHeat) && policy.Grid.IsSome && topology.IsWatertight && topology.IsSolid && topology.IsClosed && topology.IsOriented && !topology.HasBoundary && topology.NonManifoldEdges == 0 ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput())
        from bounds in Optional(space.Native.GetBoundingBox(accurate: true)).Filter(static box => box.IsValid && box.Diagonal.Length > RhinoMath.ZeroTolerance).ToFin(key.InvalidInput())
        let orientation = space.Native.SolidOrientation()
        from __ in orientation == 0 ? Fin.Fail<Unit>(key.InvalidInput()) : Fin.Succ(unit)
        select (Topology: topology, Bounds: bounds, NormalSign: orientation > 0 ? 1.0 : -1.0);
    private static Fin<VolumeGridDomain> VolumeGridDomainOf(BoundingBox source, VolumeGridPolicy grid, Op key) {
        double minSpan = Math.Min(val1: source.Max.X - source.Min.X, val2: Math.Min(val1: source.Max.Y - source.Min.Y, val2: source.Max.Z - source.Min.Z));
        double h = grid.CellSize.Match(Some: static size => size.Value, None: () => minSpan / grid.Resolution.Match(Some: static resolution => resolution.Value, None: static () => 16));
        if (!source.IsValid || !RhinoMath.IsValidDouble(x: h) || h <= RhinoMath.ZeroTolerance) return Fin.Fail<VolumeGridDomain>(key.InvalidInput());
        double pad = grid.Padding.Value * h;
        Point3d min = new(x: source.Min.X - pad, y: source.Min.Y - pad, z: source.Min.Z - pad);
        double xSpan = source.Max.X + pad - min.X, ySpan = source.Max.Y + pad - min.Y, zSpan = source.Max.Z + pad - min.Z;
        long xCells = Math.Max(val1: 1L, val2: (long)Math.Ceiling(a: xSpan / h)), yCells = Math.Max(val1: 1L, val2: (long)Math.Ceiling(a: ySpan / h)), zCells = Math.Max(val1: 1L, val2: (long)Math.Ceiling(a: zSpan / h));
        long xNodes = xCells + 1L, yNodes = yCells + 1L, zNodes = zCells + 1L;
        if (xCells <= 0L || yCells <= 0L || zCells <= 0L || xCells > int.MaxValue || yCells > int.MaxValue || zCells > int.MaxValue || xNodes > VolumeGridMaxNodes || yNodes > VolumeGridMaxNodes / xNodes) return Fin.Fail<VolumeGridDomain>(key.InvalidInput());
        long xyNodes = xNodes * yNodes;
        if (zNodes > VolumeGridMaxNodes / xyNodes || yCells > int.MaxValue / xCells) return Fin.Fail<VolumeGridDomain>(key.InvalidInput());
        long nodeCount = xyNodes * zNodes, xyCells = xCells * yCells;
        if (zCells > int.MaxValue / xyCells) return Fin.Fail<VolumeGridDomain>(key.InvalidInput());
        long cellCount = xyCells * zCells;
        int xi = (int)xCells, yi = (int)yCells, zi = (int)zCells;
        BoundingBox bounds = new(min: min, max: new Point3d(x: min.X + (xi * h), y: min.Y + (yi * h), z: min.Z + (zi * h)));
        int resolution = grid.Resolution.Match(Some: static value => value.Value, None: () => Math.Min(val1: xi, val2: Math.Min(val1: yi, val2: zi)));
        return bounds.IsValid ? Fin.Succ(new VolumeGridDomain(Bounds: bounds, Resolution: resolution, XCells: xi, YCells: yi, ZCells: zi, CellSize: h, Padding: pad, Receipt: default)) : Fin.Fail<VolumeGridDomain>(key.InvalidResult());
    }
    // BOUNDARY ADAPTER — native face iteration keeps the watertight mesh snapshot stable.
    private static Fin<VolumeSourceSet> VolumeSourcesOf(Mesh mesh, double normalSign, Op key) {
        using Mesh triangulated = mesh.DuplicateMesh();
        if (triangulated.Faces.QuadCount > 0 && !triangulated.Faces.ConvertQuadsToTriangles()) return Fin.Fail<VolumeSourceSet>(key.InvalidResult());
        List<VolumeSource> sources = new(capacity: triangulated.Faces.TriangleCount);
        double total = 0.0;
        int degenerate = 0;
        for (int f = 0; f < triangulated.Faces.Count; f++) {
            MeshFace face = triangulated.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d a = triangulated.Vertices[index: face.A], b = triangulated.Vertices[index: face.B], c = triangulated.Vertices[index: face.C];
            Vector3d normal = Vector3d.CrossProduct(a: b - a, b: c - a);
            double area = 0.5 * normal.Length;
            if (!RhinoMath.IsValidDouble(x: area) || area <= DegenerateTriangleArea) { degenerate++; continue; }
            _ = normal.Unitize(); normal *= normalSign;
            Point3d center = new(x: (a.X + b.X + c.X) / 3.0, y: (a.Y + b.Y + c.Y) / 3.0, z: (a.Z + b.Z + c.Z) / 3.0);
            sources.Add(item: new VolumeSource(Center: center, Normal: normal, Area: area));
            total += area;
        }
        return sources.Count > 0
            ? Fin.Succ(new VolumeSourceSet(Sources: [.. sources], Degenerate: degenerate, Area: total))
            : Fin.Fail<VolumeSourceSet>(key.InvalidResult());
    }
    // HOT NUMERIC LOOP — every grid node integrates all source triangles for the v1 regular-domain SHM approximation.
    private static Fin<VolumeGridVectors> VolumeGridVectorsOf(Mesh mesh, VolumeGridDomain domain, VolumeSourceSet sources, double heatTime, double tolerance, Op key) {
        double[] xs = new double[domain.NodeCount], ys = new double[domain.NodeCount], zs = new double[domain.NodeCount];
        VolumeSource[] sourceArray = sources.Sources;
        double rootT = Math.Sqrt(d: heatTime), epsilon = domain.CellSize * domain.CellSize * VolumeGridKernelSofteningRatio;
        int rejected = 0, inside = 0, outside = 0, near = 0, interior = -1;
        for (int z = 0; z < domain.ZNodes; z++)
            for (int y = 0; y < domain.YNodes; y++)
                for (int x = 0; x < domain.XNodes; x++) {
                    int index = domain.Index(x: x, y: y, z: z);
                    Point3d point = domain.PointAt(x: x, y: y, z: z);
                    bool isInside = mesh.IsPointInside(point: point, tolerance: tolerance, strictlyIn: true);
                    inside += isInside ? 1 : 0; outside += isInside ? 0 : 1; interior = isInside && interior < 0 ? index : interior;
                    near += mesh.ClosestPoint(testPoint: point, pointOnMesh: out Point3d _, maximumDistance: domain.CellSize) >= 0 ? 1 : 0;
                    Vector3d vector = Vector3d.Zero;
                    for (int s = 0; s < sourceArray.Length; s++) {
                        VolumeSource source = sourceArray[s]; Vector3d delta = point - source.Center; double r = Math.Sqrt(d: delta.SquareLength + epsilon);
                        vector += source.Area * Math.Exp(d: -r / rootT) / r * source.Normal;
                    }
                    bool valid = vector.Unitize();
                    xs[index] = valid ? vector.X : 0.0; ys[index] = valid ? vector.Y : 0.0; zs[index] = valid ? vector.Z : 0.0;
                    rejected += valid ? 0 : 1;
                }
        return rejected < domain.NodeCount && interior >= 0
            ? Fin.Succ(new VolumeGridVectors(X: xs, Y: ys, Z: zs, Rejected: rejected, Inside: inside, Outside: outside, NearSurface: near, InteriorIndex: interior))
            : Fin.Fail<VolumeGridVectors>(key.InvalidResult());
    }
    private static Fin<VolumeGridSystem> AssembleVolumeGridPoisson(VolumeGridDomain domain, VolumeGridVectors vectors, Op key) {
        int gauge = 0;
        double invH = 1.0 / domain.CellSize, invH2 = invH * invH;
        double[] rhs = new double[domain.NodeCount];
        List<(int Row, int Col, double Value)> triplets = new(capacity: domain.NodeCount * 7);
        double Difference(double[] values, int x, int y, int z, int axis) {
            int max = axis == 0 ? domain.XNodes - 1 : axis == 1 ? domain.YNodes - 1 : domain.ZNodes - 1;
            int lo = axis == 0 ? domain.Index(Math.Max(x - 1, 0), y, z) : axis == 1 ? domain.Index(x, Math.Max(y - 1, 0), z) : domain.Index(x, y, Math.Max(z - 1, 0));
            int hi = axis == 0 ? domain.Index(Math.Min(x + 1, max), y, z) : axis == 1 ? domain.Index(x, Math.Min(y + 1, max), z) : domain.Index(x, y, Math.Min(z + 1, max));
            int coord = axis == 0 ? x : axis == 1 ? y : z;
            return (values[hi] - values[lo]) * (coord == 0 || coord == max ? invH : 0.5 * invH);
        }
        for (int z = 0; z < domain.ZNodes; z++)
            for (int y = 0; y < domain.YNodes; y++)
                for (int x = 0; x < domain.XNodes; x++) {
                    int row = domain.Index(x: x, y: y, z: z);
                    if (row == gauge) {
                        rhs[row] = 0.0;
                        triplets.Add(item: (row, row, 1.0));
                        continue;
                    }
                    rhs[row] = -(Difference(values: vectors.X, x: x, y: y, z: z, axis: 0) + Difference(values: vectors.Y, x: x, y: y, z: z, axis: 1) + Difference(values: vectors.Z, x: x, y: y, z: z, axis: 2));
                    double diag = 0.0;
                    void AddNeighbor(int nx, int ny, int nz) {
                        int col = domain.Index(x: nx, y: ny, z: nz); diag += invH2;
                        if (col != gauge) triplets.Add(item: (row, col, -invH2));
                    }
                    if (x > 0) AddNeighbor(nx: x - 1, ny: y, nz: z); if (x < domain.XNodes - 1) AddNeighbor(nx: x + 1, ny: y, nz: z);
                    if (y > 0) AddNeighbor(nx: x, ny: y - 1, nz: z); if (y < domain.YNodes - 1) AddNeighbor(nx: x, ny: y + 1, nz: z);
                    if (z > 0) AddNeighbor(nx: x, ny: y, nz: z - 1); if (z < domain.ZNodes - 1) AddNeighbor(nx: x, ny: y, nz: z + 1);
                    triplets.Add(item: (row, row, diag));
                }
        Dimension dim = Dimension.Create(value: domain.NodeCount);
        return SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: triplets, key: key)
            .Map(matrix => new VolumeGridSystem(Operator: matrix, Rhs: new Arr<double>(rhs), GaugeNode: gauge));
    }
    private static Fin<(Arr<double> Values, double Shift)> CalibrateClosedSignedHeat(VolumeGridDomain domain, Arr<double> raw, VolumeSourceSet sources, int interiorIndex, Op key) {
        VolumeSource[] sourceArray = sources.Sources;
        if (raw.Count != domain.NodeCount || sourceArray.Length <= 0) return Fin.Fail<(Arr<double>, double)>(key.InvalidResult());
        double sourceSum = 0.0;
        for (int s = 0; s < sourceArray.Length; s++) {
            double value = InterpolateVolumeGrid(domain: domain, values: raw, sample: sourceArray[s].Center, key: key).IfFail(double.NaN);
            if (!RhinoMath.IsValidDouble(x: value)) return Fin.Fail<(Arr<double>, double)>(key.InvalidResult());
            sourceSum += value;
        }
        double shift = sourceSum / sourceArray.Length;
        double[] values = new double[raw.Count];
        double flip = interiorIndex >= 0 && raw[index: interiorIndex] - shift > 0.0 ? -1.0 : 1.0;
        for (int i = 0; i < raw.Count; i++) {
            double value = flip * (raw[index: i] - shift);
            if (!RhinoMath.IsValidDouble(x: value)) return Fin.Fail<(Arr<double>, double)>(key.InvalidResult());
            values[i] = value;
        }
        return RhinoMath.IsValidDouble(x: shift) ? Fin.Succ((Values: new Arr<double>(values), Shift: shift)) : Fin.Fail<(Arr<double>, double)>(key.InvalidResult());
    }
    private static Fin<double> InterpolateVolumeGrid(VolumeGridDomain domain, Arr<double> values, Point3d sample, Op key) {
        if (values.Count != domain.XNodes * domain.YNodes * domain.ZNodes || !sample.IsValid) return Fin.Fail<double>(key.InvalidInput());
        double sx = (sample.X - domain.Bounds.Min.X) / domain.CellSize, sy = (sample.Y - domain.Bounds.Min.Y) / domain.CellSize, sz = (sample.Z - domain.Bounds.Min.Z) / domain.CellSize;
        if (!RhinoMath.IsValidDouble(x: sx) || !RhinoMath.IsValidDouble(x: sy) || !RhinoMath.IsValidDouble(x: sz) || sx < 0.0 || sy < 0.0 || sz < 0.0 || sx > domain.XNodes - 1 || sy > domain.YNodes - 1 || sz > domain.ZNodes - 1)
            return Fin.Fail<double>(key.InvalidInput());
        int x0 = sx >= domain.XNodes - 1 ? domain.XNodes - 2 : (int)Math.Floor(d: sx), y0 = sy >= domain.YNodes - 1 ? domain.YNodes - 2 : (int)Math.Floor(d: sy), z0 = sz >= domain.ZNodes - 1 ? domain.ZNodes - 2 : (int)Math.Floor(d: sz);
        double tx = sx - x0, ty = sy - y0, tz = sz - z0;
        double Sample(int dx, int dy, int dz) => values[index: domain.Index(x: x0 + dx, y: y0 + dy, z: z0 + dz)];
        double c00 = (Sample(dx: 0, dy: 0, dz: 0) * (1.0 - tx)) + (Sample(dx: 1, dy: 0, dz: 0) * tx);
        double c10 = (Sample(dx: 0, dy: 1, dz: 0) * (1.0 - tx)) + (Sample(dx: 1, dy: 1, dz: 0) * tx);
        double c01 = (Sample(dx: 0, dy: 0, dz: 1) * (1.0 - tx)) + (Sample(dx: 1, dy: 0, dz: 1) * tx);
        double c11 = (Sample(dx: 0, dy: 1, dz: 1) * (1.0 - tx)) + (Sample(dx: 1, dy: 1, dz: 1) * tx);
        double c0 = (c00 * (1.0 - ty)) + (c10 * ty), c1 = (c01 * (1.0 - ty)) + (c11 * ty);
        return key.AcceptValue(value: (c0 * (1.0 - tz)) + (c1 * tz));
    }
    private static Fin<(TopologyReceipt Topology, BoundarySignedHeatSource Source)> AdmitBoundarySignedHeat(MeshSpace space, IntrinsicMesh imesh, Op key) =>
        from topology in TopologyDetailed(space: space, key: key)
        from polylines in Optional(space.Native.GetNakedEdges()).Filter(static edges => edges.Length > 0).ToFin(key.InvalidInput())
        let encoded = EncodeSignedHeatBoundarySource(mesh: space.Native, imesh: imesh, polylines: polylines)
        from admitted in AdmitSignedHeatSource(encoded: encoded, key: key)
        select (Topology: topology, Source: admitted);
    // Edge-as-source encoding stacks imaginary CR edge sources with Lo→Hi signs.
    private static BoundarySignedHeatSource EncodeSignedHeatBoundarySource(Mesh mesh, IntrinsicMesh imesh, Polyline[] polylines) {
        int eCount = imesh.EdgeCount;
        double[] stacked = new double[2 * eCount];
        System.Collections.Generic.HashSet<int> sources = [];
        Point3dList vertices = [.. mesh.Vertices.ToPoint3dArray()];
        double tolSq = RhinoMath.ZeroTolerance * 1.0e2;
        tolSq *= tolSq;
        int encodedEdges = 0;
        int rejectedPoints = 0;
        int unmatchedSegments = 0;
        for (int p = 0; p < polylines.Length; p++) {
            Polyline poly = polylines[p];
            int prev = FindClosestVertex(vertices: vertices, target: poly[0], tolSq: tolSq);
            _ = prev >= 0 && sources.Add(item: prev);
            rejectedPoints += prev >= 0 ? 0 : 1;
            for (int q = 1; q < poly.Count; q++) {
                int curr = FindClosestVertex(vertices: vertices, target: poly[q], tolSq: tolSq);
                _ = curr >= 0 && sources.Add(item: curr);
                rejectedPoints += curr >= 0 ? 0 : 1;
                int e = prev >= 0 && curr >= 0 ? imesh.IndexOfEdge(lo: prev, hi: curr) : -1;
                if (e >= 0) {
                    IntrinsicEdge edge = imesh.EdgeAt(index: e);
                    double sign = prev == edge.Lo ? 1.0 : -1.0;
                    stacked[e + eCount] += edge.Length * sign;
                    encodedEdges++;
                }
                unmatchedSegments += prev >= 0 && curr >= 0 && e < 0 ? 1 : 0;
                prev = curr;
            }
        }
        return new BoundarySignedHeatSource(Rhs: new Arr<double>(stacked), SourceVertices: toSeq(sources), EncodedEdgeSourceCount: encodedEdges, RejectedBoundaryPointCount: rejectedPoints, UnmatchedBoundarySegmentCount: unmatchedSegments);
    }
    private static Fin<BoundarySignedHeatSource> AdmitSignedHeatSource(BoundarySignedHeatSource encoded, Op key) =>
        (encoded.SourceVertices.IsEmpty, encoded.EncodedEdgeSourceCount <= 0, encoded.Rhs.Fold(initialState: (Finite: true, Active: false), f: static (state, value) => (state.Finite && RhinoMath.IsValidDouble(x: value), state.Active || Math.Abs(value: value) > RhinoMath.ZeroTolerance))) switch {
            (false, false, (true, true)) => Fin.Succ(encoded),
            _ => Fin.Fail<BoundarySignedHeatSource>(key.InvalidResult()),
        };
    private static int FindClosestVertex(Point3dList vertices, Point3d target, double tolSq) =>
        vertices.ClosestIndex(testPoint: target) switch {
            int best when best >= 0 && vertices[index: best].DistanceToSquared(other: target) <= tolSq => best,
            _ => -1,
        };
    private static Fin<Arr<double>> ShiftSignedHeat(Arr<double> phi, Seq<int> sourceVertices, int vertexCount, Op key) =>
        phi.Count != vertexCount || !phi.ForAll(RhinoMath.IsValidDouble) || sourceVertices.IsEmpty || sourceVertices.Exists(source => source < 0 || source >= vertexCount)
            ? Fin.Fail<Arr<double>>(key.InvalidResult())
            : (sourceVertices.Fold(initialState: 0.0, f: (sum, source) => sum - phi[index: source]) / sourceVertices.Count) switch {
                double mean when RhinoMath.IsValidDouble(x: mean) && phi.AsIterable().Select(value => -value - mean).All(static value => RhinoMath.IsValidDouble(x: value)) => Fin.Succ(new Arr<double>([.. phi.AsIterable().Select(value => -value - mean)])),
                _ => Fin.Fail<Arr<double>>(key.InvalidResult()),
            };
}

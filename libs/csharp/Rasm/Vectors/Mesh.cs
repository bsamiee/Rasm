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

[SmartEnum<int>]
public sealed partial class QuadGuideInfluence {
    public static readonly QuadGuideInfluence Approximate = new(key: 0);
    public static readonly QuadGuideInfluence InterpolateRing = new(key: 1);
    public static readonly QuadGuideInfluence InterpolateLoop = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class QuadPreserveEdges {
    public static readonly QuadPreserveEdges Off = new(key: 0);
    public static readonly QuadPreserveEdges Smart = new(key: 1);
    public static readonly QuadPreserveEdges Strict = new(key: 2);
}

[Union]
public abstract partial record QuadTarget {
    private QuadTarget() { }
    public sealed record EdgeLengthCase(PositiveMagnitude Length) : QuadTarget;
    public sealed record QuadCountCase(Dimension Count, UnitInterval AdaptiveSize, bool AdaptiveQuadCount) : QuadTarget;
    public static Fin<QuadTarget> EdgeLength(double length, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: length).Map(static value => (QuadTarget)new EdgeLengthCase(Length: value));
    public static Fin<QuadTarget> QuadCount(int count, double adaptiveSize, bool adaptiveQuadCount = true, Op? key = null) =>
        key.OrDefault() switch { Op op => from quads in op.AcceptValidated<Dimension>(candidate: count) from size in op.AcceptValidated<UnitInterval>(candidate: adaptiveSize) select (QuadTarget)new QuadCountCase(Count: quads, AdaptiveSize: size, AdaptiveQuadCount: adaptiveQuadCount) };
}

[SmartEnum<int>]
public sealed partial class RemeshStatus {
    public static readonly RemeshStatus Completed = new(key: 0);
    public static readonly RemeshStatus InvalidOutput = new(key: 1);
}

[Union]
public abstract partial record RemeshKind {
    private RemeshKind() { }
    public sealed record QuadCase(QuadTarget Target, bool DetectHardEdges, QuadGuideInfluence GuideInfluence, QuadPreserveEdges PreserveEdges, QuadRemeshSymmetryAxis SymmetryAxis, Arr<Curve> GuideCurves, Arr<int> FaceBlocks) : RemeshKind;
    public sealed record SimplifyCase(ReduceMeshParameters Parameters) : RemeshKind;
    public static Fin<RemeshKind> Quad(QuadTarget target, bool detectHardEdges = true, QuadGuideInfluence? guideInfluence = null, QuadPreserveEdges? preserveEdges = null, QuadRemeshSymmetryAxis symmetryAxis = QuadRemeshSymmetryAxis.None, Seq<Curve> guideCurves = default, Seq<int> faceBlocks = default, Op? key = null) =>
        key.OrDefault() switch {
            Op op => from active in Optional(target).ToFin(op.InvalidInput())
                     from curves in guideCurves.IsEmpty ? Fin.Succ(Arr<Curve>.Empty) : guideCurves.AsIterable().All(static curve => curve is { IsValid: true }) ? Fin.Succ(new Arr<Curve>([.. guideCurves.AsIterable()])) : Fin.Fail<Arr<Curve>>(op.InvalidInput())
                     from blocks in faceBlocks.IsEmpty ? Fin.Succ(Arr<int>.Empty) : faceBlocks.AsIterable().All(static index => index >= 0) ? Fin.Succ(new Arr<int>([.. faceBlocks.AsIterable()])) : Fin.Fail<Arr<int>>(op.InvalidInput())
                     select (RemeshKind)new QuadCase(Target: active, DetectHardEdges: detectHardEdges, GuideInfluence: guideInfluence ?? QuadGuideInfluence.Approximate, PreserveEdges: preserveEdges ?? QuadPreserveEdges.Off, SymmetryAxis: symmetryAxis, GuideCurves: curves, FaceBlocks: blocks),
        };
    public static Fin<RemeshKind> Simplify(ReduceMeshParameters parameters, Op? key = null) =>
        key.OrDefault() switch {
            Op op => Optional(parameters).ToFin(op.InvalidInput())
                .Bind(active => guard(active.DesiredPolygonCount >= 1, op.InvalidInput())
                    .Bind(_ => Fin.Succ<RemeshKind>(new SimplifyCase(Parameters: active)))),
        };
}

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
public sealed partial class TangentLogMapAlgorithm {
    public static readonly TangentLogMapAlgorithm VectorHeatApproximate = new(key: 0);
    public static readonly TangentLogMapAlgorithm ExactStraightestExp = new(key: 1);
    public static readonly TangentLogMapAlgorithm ExactWindowPropagation = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class GeodesicStopKind {
    public static readonly GeodesicStopKind LengthReached = new(key: 0);
    public static readonly GeodesicStopKind BoundaryHit = new(key: 1);
    public static readonly GeodesicStopKind BarrierHit = new(key: 2);
    public static readonly GeodesicStopKind IterationCap = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class SignpostEncoding {
    public static readonly SignpostEncoding Signposts = new(key: 0);
    public static readonly SignpostEncoding NormalCoordinates = new(key: 1);
    public static readonly SignpostEncoding Both = new(key: 2);
}

[SmartEnum<int>]
public sealed partial class SignpostGauge {
    public static readonly SignpostGauge FirstHalfedge = new(key: 0);
    public static readonly SignpostGauge LowestVertexNeighbor = new(key: 1);
}

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
        TopologyReceipt self = this;
        return AtomProjection.Rows<TopologyReceipt, TOut>(self: self, key: key,
            new ProjectionRow(typeof((int Euler, int Genus, int BoundaryComponents)), () => self.Genus.Match(
                Some: genus => Fin.Succ<object>((self.EulerCharacteristic, genus, self.BoundaryComponents)),
                None: () => Fin.Fail<object>(key.InvalidResult()))));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TuftedLaplacianReceipt(MeshLaplacian Kind, int OriginalVertices, int OriginalFaces, int IntrinsicVertices, int IntrinsicEdges, int IntrinsicFaces, int CoverFaces, int CoverEdges, int BoundaryEdges, int NonManifoldEdges, bool GluingMapIsBijection, int GluingSymmetryViolations, bool CoverIsEdgeManifold, bool CoverIsClosed, double MollificationEpsilon, int DegenerateTriangleCount, double LengthScaleH, double MinTriangleInequalitySlack, int IntrinsicFlips, int NonDelaunayEdgesRemaining, bool MaxFlipsHit, double MinCotanEdgeWeight, double MinBoundaryEdgeWeight, int NegativeWeightCount, double MinLumpedMass, double TotalCoveredArea, double EnergyScaleApplied, double SymmetryResidual, double RowSumResidual, int DroppedNonTriangleFaces, bool CoverAware, bool CollapsedToOriginalVertices) {
    public bool IsValid =>
        Kind is not null
        && OriginalVertices >= 0 && OriginalFaces >= 0 && IntrinsicVertices >= 0 && IntrinsicEdges >= 0 && IntrinsicFaces >= 0 && CoverFaces >= 0 && CoverEdges >= 0 && BoundaryEdges >= 0 && NonManifoldEdges >= 0 && GluingSymmetryViolations >= 0 && DegenerateTriangleCount >= 0 && IntrinsicFlips >= 0 && NonDelaunayEdgesRemaining >= 0 && NegativeWeightCount >= 0 && DroppedNonTriangleFaces >= 0
        && OriginalFaces >= IntrinsicFaces + DroppedNonTriangleFaces
        && RhinoMath.IsValidDouble(x: MollificationEpsilon) && MollificationEpsilon >= 0.0
        && RhinoMath.IsValidDouble(x: LengthScaleH) && LengthScaleH >= 0.0
        && RhinoMath.IsValidDouble(x: MinTriangleInequalitySlack)
        && RhinoMath.IsValidDouble(x: MinCotanEdgeWeight) && RhinoMath.IsValidDouble(x: MinBoundaryEdgeWeight)
        && RhinoMath.IsValidDouble(x: MinLumpedMass) && RhinoMath.IsValidDouble(x: TotalCoveredArea) && TotalCoveredArea >= 0.0
        && RhinoMath.IsValidDouble(x: EnergyScaleApplied) && EnergyScaleApplied > 0.0
        && RhinoMath.IsValidDouble(x: SymmetryResidual) && SymmetryResidual >= 0.0
        && RhinoMath.IsValidDouble(x: RowSumResidual) && RowSumResidual >= 0.0
        && (!CoverAware || (CoverFaces == 2 * IntrinsicFaces && GluingMapIsBijection && GluingSymmetryViolations == 0 && CoverIsEdgeManifold && CoverIsClosed))
        && (!CoverAware || (NonDelaunayEdgesRemaining == 0 && !MaxFlipsHit))
        && (!CoverAware || (SymmetryResidual <= RhinoMath.SqrtEpsilon && RowSumResidual <= RhinoMath.SqrtEpsilon && MinLumpedMass > 0.0))
        && (!CoverAware || (MinCotanEdgeWeight >= -RhinoMath.SqrtEpsilon && MinBoundaryEdgeWeight >= -RhinoMath.SqrtEpsilon))
        && (!CollapsedToOriginalVertices || IntrinsicVertices == OriginalVertices);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SparseLaplacian(SparseMatrix Stiffness, SparseMatrix MassConsistent, Arr<double> MassLumped, int SkippedDegenerateFaces = 0, Option<TuftedLaplacianReceipt> Tufted = default, int NegativeCotangentCount = 0) {
    public bool IsValid => Stiffness.IsValid && MassConsistent.IsValid && Stiffness.Rows.Value == MassConsistent.Rows.Value && Stiffness.Cols.Value == MassConsistent.Cols.Value && Stiffness.Rows.Value == Stiffness.Cols.Value && MassLumped.Count == Stiffness.Rows.Value && MassLumped.All(static v => RhinoMath.IsValidDouble(x: v) && v >= 0.0) && SkippedDegenerateFaces >= 0 && NegativeCotangentCount >= 0 && Tufted.Map(static receipt => receipt.IsValid).IfNone(noneValue: true);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TuftedCoverPolicy(PositiveMagnitude MollifyFactor, bool MollifyEnabled, PositiveMagnitude DelaunayTolerance, Dimension MaxFlipsPerEdge, UnitInterval EnergyScaleFactor, bool LaplacianReplace, bool MassReplace) {
    public static readonly TuftedCoverPolicy Mesh = new(MollifyFactor: PositiveMagnitude.Create(value: 1.0e-5), MollifyEnabled: true, DelaunayTolerance: PositiveMagnitude.Create(value: RhinoMath.SqrtEpsilon), MaxFlipsPerEdge: Dimension.Create(value: 16), EnergyScaleFactor: UnitInterval.Create(value: 0.5), LaplacianReplace: true, MassReplace: true);
    public static Fin<TuftedCoverPolicy> Of(double mollifyFactor, bool mollifyEnabled, double delaunayTolerance, int maxFlipsPerEdge, double energyScaleFactor, bool laplacianReplace, bool massReplace, Op? key = null) =>
        key.OrDefault() switch {
            Op op =>
                from factor in op.AcceptValidated<PositiveMagnitude>(candidate: mollifyFactor)
                from tolerance in op.AcceptValidated<PositiveMagnitude>(candidate: delaunayTolerance)
                from cap in op.AcceptValidated<Dimension>(candidate: maxFlipsPerEdge)
                from energy in op.AcceptValidated<UnitInterval>(candidate: energyScaleFactor)
                from _ in guard(energy.Value > RhinoMath.ZeroTolerance, op.InvalidInput()).ToFin()
                select new TuftedCoverPolicy(MollifyFactor: factor, MollifyEnabled: mollifyEnabled, DelaunayTolerance: tolerance, MaxFlipsPerEdge: cap, EnergyScaleFactor: energy, LaplacianReplace: laplacianReplace, MassReplace: massReplace),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SignpostPolicy(SignpostEncoding Encoding, UnitInterval LawOfCosinesClamp, PositiveMagnitude DegenerateAreaFloor, int TraceMaxIters, PositiveMagnitude VertexAngleRescaleFloor, SignpostGauge ReferenceDirectionGauge, bool CommonSubdivisionTriangulate) {
    public static readonly SignpostPolicy Default = new(Encoding: SignpostEncoding.Both, LawOfCosinesClamp: UnitInterval.Create(value: 1.0), DegenerateAreaFloor: PositiveMagnitude.Create(value: RhinoMath.SqrtEpsilon), TraceMaxIters: 0, VertexAngleRescaleFloor: PositiveMagnitude.Create(value: RhinoMath.SqrtEpsilon), ReferenceDirectionGauge: SignpostGauge.FirstHalfedge, CommonSubdivisionTriangulate: true);
    internal int TraceCapFor(int edgeCount) => TraceMaxIters > 0 ? TraceMaxIters : Math.Max(val1: 1, val2: edgeCount) * 16;
    public static Fin<SignpostPolicy> Of(SignpostEncoding encoding, double lawOfCosinesClamp, double degenerateAreaFloor, int traceMaxIters, double vertexAngleRescaleFloor, SignpostGauge referenceDirectionGauge, bool commonSubdivisionTriangulate, Op? key = null) =>
        key.OrDefault() switch {
            Op op =>
                from activeEncoding in Optional(encoding).ToFin(op.InvalidInput())
                from activeGauge in Optional(referenceDirectionGauge).ToFin(op.InvalidInput())
                from clamp in op.AcceptValidated<UnitInterval>(candidate: lawOfCosinesClamp)
                from areaFloor in op.AcceptValidated<PositiveMagnitude>(candidate: degenerateAreaFloor)
                from rescaleFloor in op.AcceptValidated<PositiveMagnitude>(candidate: vertexAngleRescaleFloor)
                from _ in guard(traceMaxIters >= 0, op.InvalidInput()).ToFin()
                select new SignpostPolicy(Encoding: activeEncoding, LawOfCosinesClamp: clamp, DegenerateAreaFloor: areaFloor, TraceMaxIters: traceMaxIters, VertexAngleRescaleFloor: rescaleFloor, ReferenceDirectionGauge: activeGauge, CommonSubdivisionTriangulate: commonSubdivisionTriangulate),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct GeodesicTracePolicy(PositiveMagnitude TraceLengthFactor, Dimension MaxSteps, UnitInterval VertexSnap, bool StopAtBoundary, bool StopAtBarriers) {
    public static readonly GeodesicTracePolicy Default = new(TraceLengthFactor: PositiveMagnitude.Create(value: 64.0), MaxSteps: Dimension.Create(value: 4096), VertexSnap: UnitInterval.Create(value: 1.0e-6), StopAtBoundary: true, StopAtBarriers: true);
    public static Fin<GeodesicTracePolicy> Of(double traceLengthFactor, int maxSteps, double vertexSnap, bool stopAtBoundary, bool stopAtBarriers, Op? key = null) =>
        key.OrDefault() switch {
            Op op =>
                from factor in op.AcceptValidated<PositiveMagnitude>(candidate: traceLengthFactor)
                from steps in op.AcceptValidated<Dimension>(candidate: maxSteps)
                from snap in op.AcceptValidated<UnitInterval>(candidate: vertexSnap)
                select new GeodesicTracePolicy(TraceLengthFactor: factor, MaxSteps: steps, VertexSnap: snap, StopAtBoundary: stopAtBoundary, StopAtBarriers: stopAtBarriers),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct WindowPropagationPolicy(Dimension MaxWindowsPerEdge, Dimension BacktraceMaxHops, PositiveMagnitude SaddleAngleThreshold, bool ReportCutLocus) {
    // SaddleAngleThreshold is a cone-angle gate, not a vector angle: a hyperbolic (saddle) cone point carries total
    // angle ABOVE 2pi, so the carrier must admit values past 2pi (PositiveMagnitude, unbounded above) and the default
    // seats at the flat-vertex boundary (2pi). The seeding and first-leg-replay tests use a strict `> 2pi` comparison,
    // so the angle-defect sign fires only on genuine hyperbolic cone points while every flat/convex vertex (<=2pi) stays below it.
    public static readonly WindowPropagationPolicy Default = new(MaxWindowsPerEdge: Dimension.Create(value: 512), BacktraceMaxHops: Dimension.Create(value: 4096), SaddleAngleThreshold: PositiveMagnitude.Create(value: RhinoMath.TwoPI), ReportCutLocus: false);
    public static Fin<WindowPropagationPolicy> Of(int maxWindowsPerEdge, int backtraceMaxHops, double saddleAngleThreshold, bool reportCutLocus, Op? key = null) =>
        key.OrDefault() switch {
            Op op =>
                from windows in op.AcceptValidated<Dimension>(candidate: maxWindowsPerEdge)
                from hops in op.AcceptValidated<Dimension>(candidate: backtraceMaxHops)
                from saddle in op.AcceptValidated<PositiveMagnitude>(candidate: saddleAngleThreshold)
                select new WindowPropagationPolicy(MaxWindowsPerEdge: windows, BacktraceMaxHops: hops, SaddleAngleThreshold: saddle, ReportCutLocus: reportCutLocus),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TuftedBaseFaces(Mesh Triangulated, int TriangleCount, int DroppedNonTriangleFaces) {
    public bool IsValid => Triangulated is not null && Triangulated.IsValid && TriangleCount >= 0 && DroppedNonTriangleFaces >= 0;
    internal static Fin<TuftedBaseFaces> Of(Mesh source, Op key) {
        Mesh duplicate = source.DuplicateMesh();
        if (MeshKernel.ContainsQuads(mesh: duplicate) && !duplicate.Faces.ConvertQuadsToTriangles())
            return Fin.Fail<TuftedBaseFaces>(key.InvalidResult());
        int triangles = 0; int dropped = 0;
        for (int f = 0; f < duplicate.Faces.Count; f++)
            if (duplicate.Faces[index: f].IsTriangle) triangles++; else dropped++;
        return dropped > 0
            ? Fin.Fail<TuftedBaseFaces>(key.InvalidResult(detail: string.Create(provider: CultureInfo.InvariantCulture, $"Tufted cover requires a fully triangulated base; {dropped} non-triangle face(s) remained after quad conversion.")))
            : Fin.Succ(new TuftedBaseFaces(Triangulated: duplicate, TriangleCount: triangles, DroppedNonTriangleFaces: dropped));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SpectralBasisBundle(SpectralBasis Basis, EigenSolveReceipt<double, Arr<double>> Eigen, bool CacheHit = false, int SkippedDegenerateFaces = 0, Option<int> FactorNonZeros = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct DescriptorReceipt(SpectralDescriptorReceipt Spectral, EigenSolveReceipt<double, Arr<double>> Eigen, int RequestedEigenpairs, int ReturnedEigenpairs, bool SpectralCacheHit = false, int SkippedDegenerateFaces = 0, Option<int> FactorNonZeros = default, Option<SpectralAssemblyReceipt> Assembly = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct DescriptorResult(Arr<double> Values, DescriptorReceipt Receipt);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct MeshSamplingSpectrumReceipt(int VertexCount, int SampleCount, int EigenpairCount, double LowFrequencyEnergy, double TotalEnergy, double SuppressionRatio, double ValidationThreshold, bool Validated, MeshSamplingSpectrumAlgorithm? Algorithm = null) {
    internal bool IsValid => VertexCount > 0 && SampleCount > 0 && EigenpairCount > 0 && RhinoMath.IsValidDouble(x: LowFrequencyEnergy) && RhinoMath.IsValidDouble(x: TotalEnergy) && RhinoMath.IsValidDouble(x: SuppressionRatio) && RhinoMath.IsValidDouble(x: ValidationThreshold) && TotalEnergy > 0.0 && SuppressionRatio is >= 0.0 and <= 1.0 && ValidationThreshold is >= 0.0 and <= 1.0 && Validated == (SuppressionRatio <= ValidationThreshold);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct FeatureEdge(int A, int B, MeshFeatureKind Kind, Option<double> DihedralRadians, Option<double> SignedDihedralRadians = default, Option<double> CurvatureSignal = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct FeatureReceipt(Seq<FeatureEdge> Edges, int BoundaryEdges, int CreaseEdges, int NonManifoldEdges, int UnweldedEdges, int NgonInteriorSkippedEdges, double DihedralThresholdRadians, int RidgeEdges = 0, int ValleyEdges = 0, int RegionBoundaryEdges = 0, double CurvatureThreshold = 0.0, double SmoothingScale = 0.0, int CurvatureFiniteVertices = 0, int CurvatureRejectedVertices = 0, MeshFeatureAlgorithm? Algorithm = null) {
    internal Fin<TOut> Project<TOut>(Op key) {
        FeatureReceipt self = this;
        return AtomProjection.Rows<FeatureReceipt, TOut>(self: self, key: key,
            new ProjectionRow(typeof(Seq<FeatureEdge>), () => Fin.Succ<object>(self.Edges)),
            new ProjectionRow(typeof(Seq<(int A, int B)>), () => Fin.Succ<object>(toSeq(self.Edges.AsIterable()
                .Where(static edge => !edge.Kind.Equals(MeshFeatureKind.NgonInteriorSkipped))
                .Select(static edge => (edge.A, edge.B))))));
    }
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

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct FlattenReceipt(int VertexCount, int UvCount, int TextureCoordinateCount, int BoundaryComponents, bool Valid, Option<double> EdgeLengthDistortionRms);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct FlattenResult(Arr<Point2d> Uvs, Mesh Mesh, FlattenReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Op key) {
        FlattenResult self = this;
        return AtomProjection.Rows<FlattenResult, TOut>(self: self, key: key,
            new ProjectionRow(typeof(Arr<Point2d>), () => Fin.Succ<object>(self.Uvs)),
            new ProjectionRow(typeof(FlattenReceipt), () => Fin.Succ<object>(self.Receipt)),
            new ProjectionRow(typeof(Mesh), () => key.AcceptValue(value: self.Mesh).Map(static value => (object)value)));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshSegmentationReceipt(MeshSegmentationAlgorithm Algorithm, MeshSegmentationStatus Status, int RequestedRegionCount, int RegionCount, int SeedCount, int AssignedFaceCount, int UnassignedFaceCount, int SkippedDegenerateFaces, int SkippedNonFiniteValues, Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance, Option<double> Threshold, Option<DescriptorReceipt> Descriptor, Option<SolveReceipt> Solve, Option<bool> SpectralCacheHit, Option<bool> FactorCacheHit, Option<int> FactorNonZeros, Option<double> NormalizedCutValue = default, Option<int> AffinityNonZeros = default, Option<int> WatershedSaddleCount = default, Option<EigenSolveReceipt<double, Arr<double>>> Eigen = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MeshSegmentationResult(Arr<int> FaceRegions, Arr<int> VertexRegions, MeshSegmentationReceipt Receipt);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct RemeshReceipt(RemeshKind Kind, RemeshStatus Status, int PreVertexCount, int PreFaceCount, int PostVertexCount, int PostFaceCount, double ReductionRatio, bool Valid, bool TopologyChanged, Option<double> TargetLength = default, Option<int> TargetQuadCount = default, Option<double> AdaptiveSize = default, Option<bool> AdaptiveQuadCount = default, Option<bool> HardEdgePreservationRequested = default, Option<QuadGuideInfluence> GuideInfluence = default, Option<QuadPreserveEdges> PreserveEdges = default, Option<QuadRemeshSymmetryAxis> SymmetryAxis = default, int GuideCurveCount = 0, int FaceBlockCount = 0, Option<int> DesiredPolygonCount = default, Option<bool> AllowDistortion = default, Option<int> Accuracy = default, Option<bool> NormalizeMeshSize = default, int FaceTagCount = 0, int LockedComponentCount = 0, Option<string> ReduceError = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct RemeshResult(Mesh Mesh, RemeshReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Op key) {
        RemeshResult self = this;
        return AtomProjection.Rows<RemeshResult, TOut>(self: self, key: key,
            new ProjectionRow(typeof(Mesh), () => key.AcceptValue(value: self.Mesh).Map(static value => (object)value)),
            new ProjectionRow(typeof(RemeshReceipt), () => Fin.Succ<object>(self.Receipt)));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SignedHeatReceipt(int BoundarySourceVertexCount, int BoundaryEncodedEdgeSourceCount, int BoundaryRejectedPointCount, int BoundaryUnmatchedSegmentCount, Option<SolveReceipt> HeatSolve, SolveReceipt PoissonSolve, Option<SpectralAssemblyReceipt> EdgeAssembly = default, Option<double> SpdMassShift = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct VolumeGridReceipt(BoundingBox Bounds, int Resolution, int XNodes, int YNodes, int ZNodes, double CellSize, double Padding, int NodeCount, int CellCount, int SourceTriangleCount, int DegenerateTriangleCount, double SourceArea, int InsideNodeCount, int OutsideNodeCount, int NearSurfaceNodeCount, int RejectedVectorCount, double HeatTime, int GaugeNode, double SurfaceShift, VolumeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition, VolumeSolverPolicy Solver, int OperatorNonZeros, Option<int> FactorNonZeros, double Residual);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SdfMeshReceipt(SdfMeshMethod Method, SdfMeshStatus Status, SdfMeshDomain Domain, TopologyReceipt Topology, Option<SignedHeatReceipt> SignedHeat, Option<VolumeGridReceipt> VolumeGrid = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SdfMeshSample(double Distance, SdfMeshReceipt Receipt);

[StructLayout(LayoutKind.Auto)]
public readonly record struct CommonSubdivision(int SubdivisionVertexCount, int SubdivisionFaceCount, Arr<int> SourceFaceA, Arr<int> SourceFaceB, SparseMatrix InterpolationA, SparseMatrix InterpolationB, double RowSumResidualA, double RowSumResidualB, double EdgeLengthInterpolationResidual) {
    public bool IsValid =>
        SubdivisionVertexCount >= 0 && SubdivisionFaceCount >= 0
        && InterpolationA.IsValid && InterpolationB.IsValid
        && InterpolationA.Rows.Value == SubdivisionVertexCount && InterpolationB.Rows.Value == SubdivisionVertexCount
        && InterpolationA.Cols.Value == InterpolationB.Cols.Value
        && SourceFaceA.Count == SubdivisionFaceCount && SourceFaceB.Count == SubdivisionFaceCount
        && SourceFaceA.All(static face => face >= 0) && SourceFaceB.All(static face => face >= 0)
        // Partition-of-unity gate: each interpolation row must sum to 1.0 within the scale-relative band (identity rows are 1.0 by
        // construction; each crossing row is (1-u)+u / (1-t)+t = 1.0), so a residual above SqrtEpsilon is an empty or malformed row
        // (a short edge trace that scattered partial rows) and is rejected, not merely recorded. The arrival residual is the BVP
        // trace's relative arrival gap, +inf when a transverse edge failed to recover its crossings.
        && RhinoMath.IsValidDouble(x: RowSumResidualA) && RowSumResidualA >= 0.0 && RowSumResidualA <= RhinoMath.SqrtEpsilon
        && RhinoMath.IsValidDouble(x: RowSumResidualB) && RowSumResidualB >= 0.0 && RowSumResidualB <= RhinoMath.SqrtEpsilon
        && RhinoMath.IsValidDouble(x: EdgeLengthInterpolationResidual) && EdgeLengthInterpolationResidual >= 0.0;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct SignpostTransportReceipt(int VertexCount, int IntrinsicEdgeCount, int TransportedEdgeCount, int IntrinsicFlipCount, int ChordFallbackEdges, int MissingFrameEdges, int CommonSubdivisionSegments, int TracedPathEdgeCount, int NormalCoordinateParityErrors, int SumNormalCoordinates, bool IntrinsicSnapshot, bool ExactCommonSubdivision, double MaxAngleRadians, double MaxLengthResidual, double MaxSignpostUpdateResidual, Option<CommonSubdivision> Subdivision = default) {
    public bool IsValid =>
        VertexCount >= 0 && IntrinsicEdgeCount >= 0 && TransportedEdgeCount >= 0 && IntrinsicFlipCount >= 0 && ChordFallbackEdges >= 0 && MissingFrameEdges >= 0 && CommonSubdivisionSegments >= 0 && TracedPathEdgeCount >= 0 && NormalCoordinateParityErrors >= 0 && SumNormalCoordinates >= 0
        && TransportedEdgeCount + MissingFrameEdges <= IntrinsicEdgeCount
        && ChordFallbackEdges <= TransportedEdgeCount
        && CommonSubdivisionSegments == SumNormalCoordinates
        && (!ExactCommonSubdivision || NormalCoordinateParityErrors == 0)
        && RhinoMath.IsValidDouble(x: MaxAngleRadians)
        && RhinoMath.IsValidDouble(x: MaxLengthResidual)
        && RhinoMath.IsValidDouble(x: MaxSignpostUpdateResidual)
        && MaxAngleRadians >= 0.0
        && MaxLengthResidual >= 0.0
        && MaxSignpostUpdateResidual >= 0.0
        && Subdivision.Map(static sub => sub.IsValid).IfNone(noneValue: true)
        && IntrinsicSnapshot;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct HodgeDecompositionReceipt(int ExpectedGenus, int ExpectedDimension, int BasisCount, int EdgeCount, int Rank, int Nullity, int FiniteVectorCount, int PositiveStar1Count, double MaxClosedResidual, double MaxCoClosedResidual, double Star1OrthonormalResidual, double ReconstructionResidual, double HarmonicEnergy, GaugeReceipt ExactGauge, HarmonicOneFormReceipt Harmonic) {
    public bool IsValid {
        get {
            double tolerance = Harmonic.SvdTolerance;
            double residualGate = Math.Max(val1: tolerance, val2: RhinoMath.SqrtEpsilon * Math.Max(val1: 1.0, val2: Harmonic.SpectralRadius)) * 1.0e4;
            return ExpectedGenus >= 1 && ExpectedDimension == 2 * ExpectedGenus
                && BasisCount == ExpectedDimension
                && EdgeCount > 0 && Rank >= 0 && Nullity >= 0
                && Rank + Nullity == EdgeCount
                && FiniteVectorCount == EdgeCount
                && PositiveStar1Count > 0 && PositiveStar1Count <= EdgeCount
                && RhinoMath.IsValidDouble(x: MaxClosedResidual) && MaxClosedResidual >= 0.0 && MaxClosedResidual <= residualGate
                && RhinoMath.IsValidDouble(x: MaxCoClosedResidual) && MaxCoClosedResidual >= 0.0 && MaxCoClosedResidual <= residualGate
                && RhinoMath.IsValidDouble(x: Star1OrthonormalResidual) && Star1OrthonormalResidual >= 0.0 && Star1OrthonormalResidual <= residualGate
                && RhinoMath.IsValidDouble(x: ReconstructionResidual) && ReconstructionResidual >= 0.0 && ReconstructionResidual <= residualGate
                && RhinoMath.IsValidDouble(x: HarmonicEnergy) && HarmonicEnergy >= 0.0
                && ExactGauge.IsValid
                && Harmonic.IsValid
                && Harmonic.BasisCount == BasisCount
                && Harmonic.EdgeCount == EdgeCount;
        }
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TangentLogMapReceipt(TangentLogMapAlgorithm Algorithm, int SourceVertex, int TargetCount, bool VectorHeatBacked, bool RejectsFlippedIntrinsic, int FiniteLogCount, Option<double> MaxMagnitudeResidual, Option<double> HeatTime, Arr<int> PathFaces, Arr<int> CrossedEdges, double TracedLength, double PathRelativeResidual, int SegmentCount, int EdgeCrossingCount, int VertexPassCount, Option<GeodesicStopKind> StopKind = default, int WindowCount = 0, int OcclusionClampCount = 0, int PseudosourceCount = 0, int CutLocusCount = 0) {
    public bool IsValid =>
        Algorithm is not null && SourceVertex >= 0 && TargetCount >= 0 && FiniteLogCount >= 0
        && SegmentCount >= 0 && EdgeCrossingCount >= 0 && VertexPassCount >= 0 && WindowCount >= 0 && OcclusionClampCount >= 0 && PseudosourceCount >= 0 && CutLocusCount >= 0
        && PathFaces.Count == SegmentCount && CrossedEdges.Count == EdgeCrossingCount
        && PathFaces.All(static face => face >= 0) && CrossedEdges.All(static edge => edge >= 0)
        && MaxMagnitudeResidual.Map(static residual => RhinoMath.IsValidDouble(x: residual) && residual >= 0.0).IfNone(noneValue: true)
        && HeatTime.Map(static time => RhinoMath.IsValidDouble(x: time) && time >= 0.0).IfNone(noneValue: true)
        && RhinoMath.IsValidDouble(x: TracedLength) && TracedLength >= 0.0
        && RhinoMath.IsValidDouble(x: PathRelativeResidual) && PathRelativeResidual >= 0.0
        && (!StopKind.IsSome || SegmentCount == 0 || SegmentCount == EdgeCrossingCount + VertexPassCount + 1);
}

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

// Surazhsky MMP window over an intrinsic half-edge: [b0,b1] is the covered sub-interval on the edge, (d0,d1) the
// pseudosource distances to its endpoints, sigma the pseudosource accumulated distance, tau the source-side gauge angle.
// The wavefront body lands with the log-direction backtrace (4b); the exp tracer leaves the field inert.
[StructLayout(LayoutKind.Auto)] internal readonly record struct GeodesicWindow(int Edge, double B0, double B1, double D0, double D1, double Sigma, double Tau, int Pseudosource);

[StructLayout(LayoutKind.Auto)]
internal readonly record struct WindowField(int SourceVertex, Arr<GeodesicWindow> Windows, int OcclusionClampCount, int PseudosourceCount, int CutLocusCount) {
    internal static WindowField Empty(int source) => new(SourceVertex: source, Windows: [], OcclusionClampCount: 0, PseudosourceCount: 0, CutLocusCount: 0);
}

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
    private readonly Memo<Unit, SparseLaplacian> cotangent = new(), intrinsicDelaunay = new();
    private readonly Memo<TuftedCoverPolicy, SparseLaplacian> tuftedIntrinsic = new();
    private readonly Memo<Unit, CholeskySparse> cholesky = new();
    private readonly Memo<Unit, SpectralBasisBundle> defaultSpectral = new();
    private readonly Lazy<double> meanEdgeLength;
    private readonly Memo<Unit, MeshKernel.IntrinsicMesh> intrinsicMesh = new(), tuftedIntrinsicMesh = new();
    private readonly Memo<MeshLaplacian, MeshKernel.IntrinsicMesh> frozenIntrinsic = new();
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
    internal Fin<SparseLaplacian> TuftedIntrinsic(Op key) => TuftedIntrinsic(policy: TuftedCoverPolicy.Mesh, key: key);
    internal Fin<SparseLaplacian> TuftedIntrinsic(TuftedCoverPolicy policy, Op key) =>
        tuftedIntrinsic.Of(probe: policy, compute: () =>
            from imesh in TuftedIntrinsicMeshSnapshot(key: key)
            from laplacian in MeshKernel.AssembleTuftedCotangentFromIntrinsic(imesh: imesh, policy: policy, key: key)
            select laplacian);
    // Tikhonov SPD shift derived from mesh scale (mean edge length squared, gated at SqrtEpsilon), never a bare absolute
    // literal: it travels as the named SpdMassShift field on the owning SignedHeatReceipt so the regularized solve is replayable.
    internal double SpdMassShift => Math.Max(val1: MeanEdgeLength, val2: RhinoMath.ZeroTolerance) * Math.Max(val1: MeanEdgeLength, val2: RhinoMath.ZeroTolerance) * RhinoMath.SqrtEpsilon;
    internal Fin<CholeskySparse> Cholesky(Op key) =>
        cholesky.Of(probe: unit, compute: () =>
            from L in IntrinsicDelaunay(key: key)
            from spd in MeshKernel.AssembleMassStiffnessSystem(laplacian: L, massScale: SpdMassShift, stiffnessScale: 1.0, key: key)
            from factor in CholeskySparse.Of(symmetric: spd, key: key)
            select factor);
    internal Fin<MeshKernel.IntrinsicMesh> IntrinsicMeshSnapshot(Op key) =>
        intrinsicMesh.Of(probe: unit, compute: () => MeshKernel.BuildIntrinsicMesh(mesh: space.Native, key: key));
    internal Fin<MeshKernel.IntrinsicMesh> TuftedIntrinsicMeshSnapshot(Op key) =>
        tuftedIntrinsicMesh.Of(probe: unit, compute: () =>
            from baseFaces in TuftedBaseFaces.Of(source: space.Native, key: key)
            from imesh in MeshKernel.BuildIntrinsicMesh(mesh: baseFaces.Triangulated, key: key)
            select imesh);
    internal Fin<MeshKernel.IntrinsicMesh> EnsureFrozenIntrinsic(MeshLaplacian kind, Op key) =>
        frozenIntrinsic.Of(probe: kind, compute: () => MeshKernel.FrozenIntrinsicFor(mesh: space.Native, kind: kind, key: key));
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

// --- [POWER_CELLS] ------------------------------------------------------------------------
// Laguerre (power) diagram restricted to a triangle mesh: weighted-site radical clip per triangle.
// OffsetI is Option<double>.None throughout item 6 — weights-unaware; the continuous-CCVT driver
// recomputes the weighted offset d_ij = l_ij/2 + (w_i-w_j)/(2 l_ij) from live dual weights each outer
// iteration, so a populated midpoint here would be silently trusted. A_ij==A_ji holds because the FIFO
// incident-pair frontier pushes both cell views of every power facet.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PowerFacet(int SiteI, int SiteJ, double Length, Option<double> OffsetI, Point3d Centroid) {
    public bool IsValid =>
        SiteI >= 0 && SiteJ >= 0 && SiteI != SiteJ
        && RhinoMath.IsValidDouble(x: Length) && Length >= 0.0
        && OffsetI.Map(static value => RhinoMath.IsValidDouble(x: value)).IfNone(noneValue: true)
        && Centroid.IsValid;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct PowerCell(int Site, int FragmentCount, double Area, double Mass, Point3d Barycenter, double TransportCost, bool Empty) {
    public bool IsValid =>
        Site >= 0 && FragmentCount >= 0
        && RhinoMath.IsValidDouble(x: Area) && Area >= 0.0
        && RhinoMath.IsValidDouble(x: Mass)
        && RhinoMath.IsValidDouble(x: TransportCost) && TransportCost >= 0.0
        && Empty == (Mass <= 0.0)
        && (Empty || Barycenter.IsValid);
}

[SmartEnum<int>]
public sealed partial class PowerDensityPolicy {
    public static readonly PowerDensityPolicy Constant = new(key: 0, requiresField: false);
    public static readonly PowerDensityPolicy ScalarFanQuadrature = new(key: 1, requiresField: true);
    internal bool RequiresField { get; }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct RestrictedPowerReceipt(
    int SiteCount, int ClippedTriangleCount, int FragmentCount, int IncidentPairCount, int QueuePeakDepth,
    double FragmentAreaMin, double FragmentAreaMax, double TotalArea, double SurfaceArea, double IntegrationResidual,
    int FirstMomentFiniteCount, int NeighborFacetCount, int EmptyCellCount, int BoundarySiteCount,
    int DegenerateClipCount, int ClipDegeneracyCount, int NonFiniteDensityRejectionCount,
    double AreaTolerance, double LengthTolerance, int KNearest, PowerDensityPolicy Density) {
    public bool IsValid =>
        SiteCount > 0 && ClippedTriangleCount >= 0 && FragmentCount >= 0 && IncidentPairCount >= 0 && QueuePeakDepth >= 0
        && FirstMomentFiniteCount >= 0 && NeighborFacetCount >= 0 && EmptyCellCount >= 0 && BoundarySiteCount >= 0
        && DegenerateClipCount >= 0 && ClipDegeneracyCount >= 0 && NonFiniteDensityRejectionCount >= 0
        && FirstMomentFiniteCount <= FragmentCount && EmptyCellCount <= SiteCount && BoundarySiteCount <= SiteCount
        && RhinoMath.IsValidDouble(x: FragmentAreaMin) && RhinoMath.IsValidDouble(x: FragmentAreaMax) && FragmentAreaMax >= FragmentAreaMin
        && RhinoMath.IsValidDouble(x: TotalArea) && TotalArea >= 0.0
        && RhinoMath.IsValidDouble(x: SurfaceArea) && SurfaceArea >= 0.0
        && RhinoMath.IsValidDouble(x: IntegrationResidual) && IntegrationResidual >= 0.0
        && RhinoMath.IsValidDouble(x: AreaTolerance) && AreaTolerance >= 0.0
        && RhinoMath.IsValidDouble(x: LengthTolerance) && LengthTolerance >= 0.0
        && KNearest >= 1 && Density is not null;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct RestrictedPowerDiagram(Arr<PowerCell> Cells, Arr<PowerFacet> Facets, RestrictedPowerReceipt Receipt) {
    public bool IsValid =>
        Cells.Count == Receipt.SiteCount && Receipt.IsValid
        && Cells.All(static cell => cell.IsValid) && Facets.All(static facet => facet.IsValid)
        && Cells.Filter(static cell => cell.Empty).Count == Receipt.EmptyCellCount
        && Facets.Count == Receipt.NeighborFacetCount;
    internal Fin<TOut> Project<TOut>(Op key) {
        RestrictedPowerDiagram self = this;
        return AtomProjection.Rows<RestrictedPowerDiagram, TOut>(self: self, key: key,
            new ProjectionRow(typeof(Arr<PowerCell>), () => Fin.Succ<object>(self.Cells)),
            new ProjectionRow(typeof(Arr<PowerFacet>), () => Fin.Succ<object>(self.Facets)),
            new ProjectionRow(typeof(RestrictedPowerReceipt), () => Fin.Succ<object>(self.Receipt)),
            new ProjectionRow(typeof(Seq<Point3d>), () => Fin.Succ<object>(toSeq(self.Cells.AsIterable().Filter(static cell => !cell.Empty).Map(static cell => cell.Barycenter)))));
    }
}

internal static class MeshKernel {
    private const double AspectRatioCeiling = 11.5;
    private const double MeshSpectrumLowFrequencyCeiling = 0.5;
    private const double VolumeGridKernelSofteningRatio = 0.0625;
    private const int VolumeGridMaxNodes = 1_000_000;
    private const int UnassignedRegion = -1;
    private const double NativeAdaptiveScale = 100.0;
    private readonly record struct SegmentationScalars(Arr<double> FaceValues, int SkippedDegenerateFaces, int SkippedNonFiniteValues, int FiniteCount, double Min, double Max);
    private readonly record struct FeatureCurvatureSignals(double[] Edge, int FiniteVertices, int RejectedVertices);
    private readonly record struct SegmentationRun(MeshSegmentationAlgorithm Algorithm, int RequestedRegionCount, int SeedCount, MeshSegmentationStatus Status, Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance, Option<double> Threshold, Option<DescriptorReceipt> Descriptor, Option<SolveReceipt> Solve = default, Option<bool> FactorCacheHit = default, Option<int> FactorNonZeros = default, Option<double> NormalizedCutValue = default, Option<int> AffinityNonZeros = default, Option<int> WatershedSaddleCount = default, Option<EigenSolveReceipt<double, Arr<double>>> Eigen = default);
    private readonly record struct WatershedState(int[] Regions, int SeedCount, int SaddleCount);
    private readonly record struct NormalizedCutSystem(SparseMatrix Laplacian, SparseMatrix Degree, int AffinityNonZeros, double Sigma);
    private readonly record struct ClusterState(int[] Labels, int Iterations, bool Converged);
    [StructLayout(LayoutKind.Auto)] private readonly record struct ConnectionEntries(Seq<(int I, int J, double Weight, double Rho)> Rows, SignpostTransportReceipt Receipt);
    [StructLayout(LayoutKind.Auto)] private readonly record struct VolumeSource(Point3d Center, Vector3d Normal, double Area);
    [StructLayout(LayoutKind.Auto)] private readonly record struct VolumeSourceSet(VolumeSource[] Sources, int Degenerate, double Area);
    [StructLayout(LayoutKind.Auto)] private readonly record struct VolumeGridVectors(double[] X, double[] Y, double[] Z, int Rejected, int Inside, int Outside, int NearSurface, int InteriorIndex);
    [StructLayout(LayoutKind.Auto)] private readonly record struct VolumeGridSystem(SparseMatrix Operator, Arr<double> Rhs);
    private sealed class LaplacianTriplets {
        private readonly int vertexCount;
        internal readonly List<(int Row, int Col, double Value)> Stiffness = [], Mass = [];
        internal readonly double[] Lumped;
        internal int SkippedDegenerateFaces;
        internal int NegativeCotangentCount;
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
            select new SparseLaplacian(Stiffness: stiff, MassConsistent: mass, MassLumped: new Arr<double>(Lumped), SkippedDegenerateFaces: SkippedDegenerateFaces, NegativeCotangentCount: NegativeCotangentCount);
    }
    internal static bool ContainsQuads(Mesh mesh) => mesh.Faces.QuadCount > 0;

    // --- [VALIDATION] -----------------------------------------------------------------------
    // BOUNDARY ADAPTER — native face iteration avoids materialising large meshes into Seq.
    internal static Fin<Unit> AspectRatioGuard(Mesh mesh, double ceiling, Op key) {
        for (int f = 0; f < mesh.Faces.Count; f++) {
            double aspect = mesh.Faces.GetFaceAspectRatio(index: f);
            if (!RhinoMath.IsValidDouble(x: aspect) || aspect > ceiling)
                return Fin.Fail<Unit>(key.Caution(concern: string.Create(provider: CultureInfo.InvariantCulture, $"Face {f} aspect ratio exceeds {ceiling}.")));
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

    // --- [POWER_CELLS] ----------------------------------------------------------------------
    // NAMED STATEMENT-KERNEL EXEMPTION — the owned Sutherland-Hodgman radical clip, the FIFO incident-pair
    // frontier, and the per-triangle shoelace area/first-moment accumulation run as in-place statement kernels
    // (allocation-sensitive, no rail combinator owns the polygon mutation); the public surface returns
    // Fin<RestrictedPowerDiagram>. Every threshold is scale-derived from the mesh bbox diagonal / mean edge.
    private const int PowerClipKNearest = 16;
    private const double PowerClipBandScale = 1e-9;
    private const double PowerDenomFloorScale = 1e-12;
    private const double PowerAreaFloorScale = 1e-10;
    private const int PowerFanQuadraturePoints = 3;
    // Dimensionless symmetric-stencil node radius (fraction of sqrt(area)) placing the 3 fan-quadrature nodes about the
    // fragment centroid; a fixed quadrature-stencil position, not a convergence/admission epsilon.
    private const double PowerFanQuadratureNodeFraction = 0.25;
    internal readonly record struct PowerClipPolicy(double ClipBand, double DenomFloor, double AreaFloor, double EdgeBand, int KNearest, int MinPolygonVertices, PowerDensityPolicy Density) {
        internal static Fin<PowerClipPolicy> Of(double diagonal, double meanEdge, PowerDensityPolicy density, Op key) =>
            !RhinoMath.IsValidDouble(x: diagonal) || diagonal <= RhinoMath.ZeroTolerance || density is null
                ? Fin.Fail<PowerClipPolicy>(key.InvalidInput())
                : from band in key.AcceptValidated<PositiveMagnitude>(candidate: PowerClipBandScale * diagonal)
                  from denom in key.AcceptValidated<PositiveMagnitude>(candidate: PowerDenomFloorScale * diagonal)
                  from area in key.AcceptValidated<PositiveMagnitude>(candidate: PowerAreaFloorScale * diagonal * diagonal)
                  from edge in key.AcceptValidated<PositiveMagnitude>(candidate: PowerClipBandScale * Math.Max(val1: meanEdge, val2: diagonal))
                  select new PowerClipPolicy(ClipBand: band.Value, DenomFloor: denom.Value, AreaFloor: area.Value, EdgeBand: edge.Value, KNearest: PowerClipKNearest, MinPolygonVertices: 3, Density: density);
    }
    // Origin-shifted weighted sites: power(x) = |x-p|^2 - w, with x and p both shifted by the bbox centre so only
    // weight DIFFERENCES survive the radical constant |p_j'|^2 - |p_i'|^2 - (w_j - w_i) and binary cancellation dies.
    [StructLayout(LayoutKind.Auto)] private readonly record struct PowerSite(Point3d Shifted, double Weight, double NormShiftedSq);
    [StructLayout(LayoutKind.Auto)] private readonly record struct PowerFrame(Point3d Origin, Vector3d Ex, Vector3d Ey);
    // Per-site fold accumulator: shoelace area + first moment (planar, in the triangle frame), transport cost
    // integral, and the facet stubs keyed by neighbouring site. Mutable, operation-local, never escapes the run.
    private sealed class PowerCellAccumulator {
        internal double Area;
        internal double MomentX, MomentY, MomentZ;
        internal double Mass;
        internal double Transport;
        internal int FragmentCount;
        internal bool FirstMomentFinite = true;
        internal bool Boundary;
        internal readonly Dictionary<int, (double Length, double Cx, double Cy, double Cz)> Facets = [];
        internal void AddFragment(double area, Point3d centroid, double mass, double transport) {
            Area += area; Mass += mass; Transport += transport; FragmentCount++;
            MomentX += centroid.X * mass; MomentY += centroid.Y * mass; MomentZ += centroid.Z * mass;
            if (!(RhinoMath.IsValidDouble(x: centroid.X) && RhinoMath.IsValidDouble(x: centroid.Y) && RhinoMath.IsValidDouble(x: centroid.Z) && RhinoMath.IsValidDouble(x: mass)))
                FirstMomentFinite = false;
        }
        internal void AddFacet(int neighbour, double length, Point3d midpoint) {
            (double Length, double Cx, double Cy, double Cz) prior = Facets.TryGetValue(key: neighbour, value: out (double Length, double Cx, double Cy, double Cz) existing) ? existing : default;
            Facets[key: neighbour] = (Length: prior.Length + length, Cx: prior.Cx + (midpoint.X * length), Cy: prior.Cy + (midpoint.Y * length), Cz: prior.Cz + (midpoint.Z * length));
        }
    }
    // Robust 2D power-cell clip: Sutherland-Hodgman against the world-space affine radical functions evaluated at the
    // lifted 3D polygon vertices. g_ij(x) = 2 (p_j' - p_i') . x - (|p_j'|^2 - w_j - |p_i'|^2 + w_i); keep g <= band.
    // No external predicate library exists; cocircular degeneracy near g==0 is counted as a clip-degeneracy rejection.
    internal static Fin<RestrictedPowerDiagram> RestrictedPowerCells(MeshSpace space, Seq<Point3d> sites, Option<Arr<double>> weights, Option<ScalarField> density, Op key) {
        BoundingBox box = space.Native.GetBoundingBox(accurate: true);
        return !box.IsValid || box.Diagonal.Length <= RhinoMath.ZeroTolerance || sites.Count < 1
            ? Fin.Fail<RestrictedPowerDiagram>(key.InvalidInput())
            : FieldNabla.AllFinite(key, sites.ToArray()).Bind(_ => weights.Match(
            Some: w => w.Count == sites.Count && FieldNabla.AllFiniteSpan(w.AsSpan()) ? Fin.Succ(w) : Fin.Fail<Arr<double>>(key.InvalidInput()),
            None: () => Fin.Succ(new Arr<double>([.. Enumerable.Repeat(element: 0.0, count: sites.Count)]))))
        .Bind(activeWeights => PowerClipPolicy.Of(diagonal: box.Diagonal.Length, meanEdge: MeanEdgeLengthOf(mesh: space.Native), density: density.IsSome ? PowerDensityPolicy.ScalarFanQuadrature : PowerDensityPolicy.Constant, key: key)
            .Bind(policy => PowerDiagramRun(space: space, sites: sites, weights: activeWeights, density: density, center: box.Center, policy: policy, key: key)));
    }
    private static Fin<RestrictedPowerDiagram> PowerDiagramRun(MeshSpace space, Seq<Point3d> sites, Arr<double> weights, Option<ScalarField> density, Point3d center, PowerClipPolicy policy, Op key) {
        using Mesh triangulated = space.Native.DuplicateMesh();
        if (ContainsQuads(mesh: triangulated) && !triangulated.Faces.ConvertQuadsToTriangles()) return Fin.Fail<RestrictedPowerDiagram>(key.InvalidResult());
        int siteCount = sites.Count;
        Vector3d shift = new(x: center.X, y: center.Y, z: center.Z);
        PowerSite[] powerSites = new PowerSite[siteCount];
        for (int s = 0; s < siteCount; s++) {
            Point3d shifted = sites[index: s] - shift;
            powerSites[s] = new PowerSite(Shifted: shifted, Weight: weights[index: s], NormShiftedSq: (shifted.X * shifted.X) + (shifted.Y * shifted.Y) + (shifted.Z * shifted.Z));
        }
        Point3d[] siteArray = [.. sites];
        int[][] neighbours = PowerSiteNeighbours(sites: siteArray, kNearest: Math.Min(val1: policy.KNearest, val2: siteCount));
        int[][] faceAdjacency = FaceAdjacencyOf(mesh: triangulated);
        bool[] boundaryFace = BoundaryFacesOf(mesh: triangulated);
        int faceCount = triangulated.Faces.Count;
        PowerCellAccumulator[] cells = [.. Enumerable.Range(start: 0, count: siteCount).Select(static _ => new PowerCellAccumulator())];
        Queue<(int Site, int Face)> frontier = new();
        System.Collections.Generic.HashSet<long> seen = [];
        long Token(int site, int face) => ((long)site * faceCount) + face;
        int[] faceNearest = NearestSitePerFace(triangulated: triangulated, powerSites: powerSites, shift: shift);
        for (int f = 0; f < faceCount; f++)
            if (faceNearest[f] >= 0 && seen.Add(item: Token(site: faceNearest[f], face: f)))
                frontier.Enqueue(item: (Site: faceNearest[f], Face: f));
        int incidentPairs = 0, queuePeak = 0, clippedTriangles = 0, degenerateClips = 0, clipDegeneracies = 0, densityRejections = 0;
        double totalArea = 0.0, fragmentAreaMin = double.PositiveInfinity, fragmentAreaMax = 0.0;
        PowerFrame?[] faceFrames = new PowerFrame?[faceCount];
        (Point3d A, Point3d B, Point3d C)?[] faceVerts = new (Point3d A, Point3d B, Point3d C)?[faceCount];
        while (frontier.Count > 0) {
            queuePeak = Math.Max(val1: queuePeak, val2: frontier.Count);
            (int site, int face) = frontier.Dequeue();
            if (faceVerts[face] is null) {
                MeshFace mf = triangulated.Faces[index: face];
                if (!mf.IsTriangle) { faceVerts[face] = (A: Point3d.Unset, B: Point3d.Unset, C: Point3d.Unset); continue; }
                Point3d va = triangulated.Vertices[index: mf.A], vb = triangulated.Vertices[index: mf.B], vc = triangulated.Vertices[index: mf.C];
                Point3d a = va - shift, b = vb - shift, c = vc - shift;
                faceVerts[face] = (A: a, B: b, C: c);
                faceFrames[face] = PowerFrameOf(a: a, b: b, c: c);
            }
            if (faceFrames[face] is not PowerFrame frame || faceVerts[face] is not (Point3d, Point3d, Point3d) tri || !tri.A.IsValid) continue;
            incidentPairs++;
            (double fragArea, Point3d fragCentroid, double fragPolar, int fragVerts, bool degenerate, List<(int Neighbour, double Length, Point3d Midpoint)>? edgeFacets) = ClipPowerCell(tri: (tri.A, tri.B, tri.C), frame: frame, site: site, neighbours: neighbours[site], powerSites: powerSites, policy: policy);
            if (degenerate) { clipDegeneracies++; }
            if (fragVerts < policy.MinPolygonVertices || fragArea <= policy.AreaFloor) { degenerateClips++; continue; }
            (double mass, double transport, bool densityOk) = IntegrateFragment(centroid: fragCentroid + shift, area: fragArea, polar: fragPolar, site: siteArray[site], density: density, policy: policy, space: space, key: key);
            if (!densityOk) { densityRejections++; continue; }
            cells[site].AddFragment(area: fragArea, centroid: fragCentroid + shift, mass: mass, transport: transport);
            if (boundaryFace[face]) cells[site].Boundary = true;
            totalArea += fragArea;
            fragmentAreaMin = Math.Min(val1: fragmentAreaMin, val2: fragArea); fragmentAreaMax = Math.Max(val1: fragmentAreaMax, val2: fragArea);
            clippedTriangles++;
            foreach ((int neighbour, double length, Point3d midpoint) in edgeFacets) {
                cells[site].AddFacet(neighbour: neighbour, length: length, midpoint: midpoint + shift);
                if (neighbour >= 0 && neighbour < siteCount && seen.Add(item: Token(site: neighbour, face: face))) frontier.Enqueue(item: (Site: neighbour, Face: face));
            }
            foreach (int nf in faceAdjacency[face])
                if (nf >= 0 && nf < faceCount && seen.Add(item: Token(site: site, face: nf))) frontier.Enqueue(item: (Site: site, Face: nf));
        }
        return AssembleDiagram(cells: cells, sites: siteArray, totalArea: totalArea, fragmentAreaMin: double.IsPositiveInfinity(d: fragmentAreaMin) ? 0.0 : fragmentAreaMin, fragmentAreaMax: fragmentAreaMax, incidentPairs: incidentPairs, queuePeak: queuePeak, clippedTriangles: clippedTriangles, degenerateClips: degenerateClips, clipDegeneracies: clipDegeneracies, densityRejections: densityRejections, policy: policy, space: space, key: key);
    }
    // Euclidean k-NN site adjacency seed: RTree.Point3dKNeighbors is Euclidean, NOT power-nearest, so with non-trivial
    // weights the k-th neighbour may not bound the power-incident set (under-clip); KNearest is parameterised and
    // IncidentPairCount/IntegrationResidual/QueuePeakDepth make any under-clip observable in the receipt.
    private static int[][] PowerSiteNeighbours(Point3d[] sites, int kNearest) {
        if (sites.Length <= 1) return [.. sites.Select(static _ => System.Array.Empty<int>())];
        int amount = Math.Min(val1: Math.Max(val1: 2, val2: kNearest + 1), val2: sites.Length);
        int[][] raw = [.. RTree.Point3dKNeighbors(hayPoints: sites, needlePts: sites, amount: amount)];
        return [.. Enumerable.Range(start: 0, count: sites.Length).Select(i => raw[i].Where(j => j != i).ToArray())];
    }
    private static int[] NearestSitePerFace(Mesh triangulated, PowerSite[] powerSites, Vector3d shift) {
        int faceCount = triangulated.Faces.Count;
        int[] nearest = [.. Enumerable.Repeat(element: -1, count: faceCount)];
        for (int f = 0; f < faceCount; f++) {
            MeshFace mf = triangulated.Faces[index: f];
            if (!mf.IsTriangle) continue;
            Point3d va = triangulated.Vertices[index: mf.A], vb = triangulated.Vertices[index: mf.B], vc = triangulated.Vertices[index: mf.C];
            Point3d centroid = ((va + vb + vc) / 3.0) - shift;
            int best = -1; double bestPower = double.PositiveInfinity;
            for (int s = 0; s < powerSites.Length; s++) {
                Point3d d = centroid - new Vector3d(x: powerSites[s].Shifted.X, y: powerSites[s].Shifted.Y, z: powerSites[s].Shifted.Z);
                double power = (d.X * d.X) + (d.Y * d.Y) + (d.Z * d.Z) - powerSites[s].Weight;
                if (power < bestPower) { bestPower = power; best = s; }
            }
            nearest[f] = best;
        }
        return nearest;
    }
    // Faces touching a naked (boundary) edge — a topology edge wired to exactly one face — so a site whose surviving
    // fragments include a boundary face is flagged boundary-incident; BoundarySiteCount then witnesses real evidence.
    private static bool[] BoundaryFacesOf(Mesh mesh) {
        bool[] boundary = new bool[mesh.Faces.Count];
        for (int e = 0; e < mesh.TopologyEdges.Count; e++) {
            int[] faces = mesh.TopologyEdges.GetConnectedFaces(topologyEdgeIndex: e);
            if (faces.Length == 1 && faces[0] >= 0 && faces[0] < boundary.Length) boundary[faces[0]] = true;
        }
        return boundary;
    }
    private static PowerFrame? PowerFrameOf(Point3d a, Point3d b, Point3d c) {
        Vector3d ex = b - a;
        if (!ex.Unitize()) return null;
        Vector3d normal = Vector3d.CrossProduct(a: b - a, b: c - a);
        if (!normal.Unitize()) return null;
        Vector3d ey = Vector3d.CrossProduct(a: normal, b: ex);
        return ey.Unitize() ? new PowerFrame(Origin: a, Ex: ex, Ey: ey) : null;
    }
    private static (double Area, Point3d Centroid, double Polar, int Vertices, bool Degenerate, List<(int Neighbour, double Length, Point3d Midpoint)> EdgeFacets) ClipPowerCell(
        (Point3d A, Point3d B, Point3d C) tri, PowerFrame frame, int site, int[] neighbours, PowerSite[] powerSites, PowerClipPolicy policy) {
        (double U, double V, int Owner)[] poly = [(U: 0.0, V: 0.0, Owner: -1), (Dot2(p: tri.B, frame: frame).U, Dot2(p: tri.B, frame: frame).V, Owner: -1), (Dot2(p: tri.C, frame: frame).U, Dot2(p: tri.C, frame: frame).V, Owner: -1)];
        List<(double U, double V, int Owner)> current = [.. poly];
        bool degenerate = false;
        PowerSite self = powerSites[site];
        foreach (int j in neighbours) {
            if (current.Count < policy.MinPolygonVertices) break;
            PowerSite other = powerSites[j];
            Vector3d grad = new(x: 2.0 * (other.Shifted.X - self.Shifted.X), y: 2.0 * (other.Shifted.Y - self.Shifted.Y), z: 2.0 * (other.Shifted.Z - self.Shifted.Z));
            double constant = other.NormShiftedSq - other.Weight - (self.NormShiftedSq - self.Weight);
            List<(double U, double V, int Owner)> next = [];
            int n = current.Count;
            for (int e = 0; e < n; e++) {
                (double U, double V, int Owner) p0 = current[index: e];
                (double U, double V, int Owner) = current[index: (e + 1) % n];
                double g0 = Lift(u: p0.U, v: p0.V, frame: frame, grad: grad) - constant;
                double g1 = Lift(u: U, v: V, frame: frame, grad: grad) - constant;
                bool in0 = g0 <= policy.ClipBand, in1 = g1 <= policy.ClipBand;
                if (Math.Abs(value: g0) <= policy.ClipBand || Math.Abs(value: g1) <= policy.ClipBand) degenerate = true;
                if (in0) next.Add(item: p0);
                if (in0 != in1) {
                    double denom = g0 - g1;
                    double t = Math.Abs(value: denom) > policy.DenomFloor ? g0 / denom : 0.5;
                    next.Add(item: (U: p0.U + (t * (U - p0.U)), V: p0.V + (t * (V - p0.V)), Owner: j));
                }
            }
            current = next;
        }
        if (current.Count < policy.MinPolygonVertices) return (Area: 0.0, Centroid: Point3d.Unset, Polar: 0.0, Vertices: current.Count, Degenerate: degenerate, EdgeFacets: []);
        (double area, double cu, double cv, double polar) = ShoelaceMoment(poly: current);
        Point3d centroid = LiftPoint(u: cu, v: cv, frame: frame);
        List<(int Neighbour, double Length, Point3d Midpoint)> facets = [];
        int m = current.Count;
        for (int e = 0; e < m; e++) {
            (double U, double V, _) = current[index: e];
            (double U, double V, int Owner) p1 = current[index: (e + 1) % m];
            int owner = p1.Owner;
            if (owner < 0) continue;
            Point3d q0 = LiftPoint(u: U, v: V, frame: frame), q1 = LiftPoint(u: p1.U, v: p1.V, frame: frame);
            double length = q0.DistanceTo(other: q1);
            if (length > policy.EdgeBand) facets.Add(item: (Neighbour: owner, Length: length, Midpoint: (q0 + q1) / 2.0));
        }
        return (Area: area, Centroid: centroid, Polar: polar, Vertices: current.Count, Degenerate: degenerate, EdgeFacets: facets);
    }
    private static (double U, double V) Dot2(Point3d p, PowerFrame frame) {
        Vector3d d = p - frame.Origin;
        return (U: d * frame.Ex, V: d * frame.Ey);
    }
    private static double Lift(double u, double v, PowerFrame frame, Vector3d grad) {
        Point3d x = frame.Origin + (frame.Ex * u) + (frame.Ey * v);
        return (grad.X * x.X) + (grad.Y * x.Y) + (grad.Z * x.Z);
    }
    private static Point3d LiftPoint(double u, double v, PowerFrame frame) => frame.Origin + (frame.Ex * u) + (frame.Ey * v);
    // Planar shoelace area + first moment (centroid) + polar second moment about the centroid. The polar moment is the
    // exact Integral_cell |x - c|^2 over the constant-density fragment: Integral(u^2+v^2) about the origin minus
    // signedArea*(cu^2+cv^2) by the parallel-axis theorem, so item 7's TransportCost = Area*|c-q|^2 + Polar is the exact
    // second-moment integral, not the dropped-polar midpoint surrogate.
    private static (double Area, double Cu, double Cv, double Polar) ShoelaceMoment(List<(double U, double V, int Owner)> poly) {
        double area2 = 0.0, cu = 0.0, cv = 0.0, iuuO = 0.0, ivvO = 0.0;
        int n = poly.Count;
        for (int e = 0; e < n; e++) {
            (double U, double V, _) = poly[index: e];
            (double U, double V, int Owner) p1 = poly[index: (e + 1) % n];
            double cross = (U * p1.V) - (p1.U * V);
            area2 += cross;
            cu += (U + p1.U) * cross;
            cv += (V + p1.V) * cross;
            iuuO += ((U * U) + (U * p1.U) + (p1.U * p1.U)) * cross;
            ivvO += ((V * V) + (V * p1.V) + (p1.V * p1.V)) * cross;
        }
        if (Math.Abs(value: area2) <= 0.0) return (Area: 0.0, Cu: 0.0, Cv: 0.0, Polar: 0.0);
        double signedArea = 0.5 * area2;
        double centroidU = cu / (3.0 * area2), centroidV = cv / (3.0 * area2);
        double polar = Math.Abs(value: ((iuuO + ivvO) / 12.0) - (signedArea * ((centroidU * centroidU) + (centroidV * centroidV))));
        return (Area: 0.5 * Math.Abs(value: area2), Cu: centroidU, Cv: centroidV, Polar: polar);
    }
    // Constant density (rho=1, mass=area) versus a scalar field integrated by symmetric fan quadrature about the
    // fragment centroid; one PowerDensityPolicy.Switch owns the fork — no parallel density code path.
    // Exact constant-density transport: Integral_cell |x - q|^2 = area * |centroid - q|^2 + polar (parallel-axis), the
    // polar second moment about the cell centroid carried from the shoelace pass — not the dropped-polar midpoint surrogate.
    private static (double Mass, double Transport, bool DensityOk) IntegrateFragment(Point3d centroid, double area, double polar, Point3d site, Option<ScalarField> density, PowerClipPolicy policy, MeshSpace space, Op key) =>
        policy.Density.Switch(
            state: (Centroid: centroid, Area: area, Polar: polar, Site: site, Density: density, Space: space, Key: key),
            constant: static state => (Mass: state.Area, Transport: (state.Area * state.Centroid.DistanceToSquared(other: state.Site)) + state.Polar, DensityOk: true),
            scalarFanQuadrature: static state => state.Density.Match(
                Some: field => FanQuadratureDensity(field: field, centroid: state.Centroid, area: state.Area, polar: state.Polar, site: state.Site, space: state.Space, key: state.Key),
                None: () => (Mass: state.Area, Transport: (state.Area * state.Centroid.DistanceToSquared(other: state.Site)) + state.Polar, DensityOk: true)));
    private static (double Mass, double Transport, bool DensityOk) FanQuadratureDensity(ScalarField field, Point3d centroid, double area, double polar, Point3d site, MeshSpace space, Op key) {
        double sum = 0.0; int finite = 0;
        for (int q = 0; q < PowerFanQuadraturePoints; q++) {
            double angle = RhinoMath.TwoPI * q / PowerFanQuadraturePoints;
            Point3d sample = centroid + (new Vector3d(x: Math.Cos(angle), y: Math.Sin(angle), z: 0.0) * Math.Sqrt(d: Math.Max(val1: area, val2: 0.0)) * PowerFanQuadratureNodeFraction);
            _ = field.SampleScalar(sample: sample, context: space.Tolerance, key: key).Match(
                Succ: value => { if (RhinoMath.IsValidDouble(x: value) && value >= 0.0) { sum += value; finite++; } },
                Fail: static _ => { });
        }
        if (finite == 0) return (Mass: 0.0, Transport: 0.0, DensityOk: false);
        double rho = sum / finite;
        double mass = rho * area;
        return RhinoMath.IsValidDouble(x: mass) ? (Mass: mass, Transport: (mass * centroid.DistanceToSquared(other: site)) + (rho * polar), DensityOk: true) : (Mass: 0.0, Transport: 0.0, DensityOk: false);
    }
    private static Fin<RestrictedPowerDiagram> AssembleDiagram(PowerCellAccumulator[] cells, Point3d[] sites, double totalArea, double fragmentAreaMin, double fragmentAreaMax, int incidentPairs, int queuePeak, int clippedTriangles, int degenerateClips, int clipDegeneracies, int densityRejections, PowerClipPolicy policy, MeshSpace space, Op key) {
        using AreaMassProperties? props = AreaMassProperties.Compute(mesh: space.Native, area: true, firstMoments: false, secondMoments: false, productMoments: false);
        double meshArea = Optional(props).Map(static p => p.Area).IfNone(noneValue: 0.0);
        List<PowerCell> powerCells = [];
        Dictionary<(int, int), (double Length, Point3d Centroid)> facetTable = [];
        int emptyCells = 0, firstMomentFinite = 0, fragmentCount = 0;
        for (int s = 0; s < cells.Length; s++) {
            PowerCellAccumulator acc = cells[s];
            bool empty = acc.Mass <= 0.0;
            Point3d barycenter = empty ? Point3d.Unset : new Point3d(x: acc.MomentX / acc.Mass, y: acc.MomentY / acc.Mass, z: acc.MomentZ / acc.Mass);
            powerCells.Add(item: new PowerCell(Site: s, FragmentCount: acc.FragmentCount, Area: acc.Area, Mass: acc.Mass, Barycenter: barycenter, TransportCost: acc.Transport, Empty: empty));
            if (empty) emptyCells++;
            if (acc.FirstMomentFinite) firstMomentFinite += acc.FragmentCount;
            fragmentCount += acc.FragmentCount;
            foreach ((int neighbour, (double length, double cx, double cy, double cz)) in acc.Facets) {
                if (length <= policy.EdgeBand) continue;
                (int Lo, int Hi) keyPair = s < neighbour ? (Lo: s, Hi: neighbour) : (Lo: neighbour, Hi: s);
                Point3d centroid = length > 0.0 ? new Point3d(x: cx / length, y: cy / length, z: cz / length) : Point3d.Unset;
                facetTable[key: keyPair] = facetTable.TryGetValue(key: keyPair, value: out (double Length, Point3d Centroid) prior)
                    ? (Length: Math.Max(val1: prior.Length, val2: length), Centroid: prior.Centroid.IsValid ? prior.Centroid : centroid)
                    : (Length: length, Centroid: centroid);
            }
        }
        List<PowerFacet> facets = [.. facetTable.Where(static row => row.Value.Centroid.IsValid).Select(static row => new PowerFacet(SiteI: row.Key.Item1, SiteJ: row.Key.Item2, Length: row.Value.Length, OffsetI: Option<double>.None, Centroid: row.Value.Centroid))];
        int boundarySites = cells.Count(static acc => acc.Mass > 0.0 && acc.Boundary);
        double integrationResidual = meshArea > 0.0 ? Math.Abs(value: totalArea - meshArea) / meshArea : Math.Abs(value: totalArea - meshArea);
        RestrictedPowerReceipt receipt = new(
            SiteCount: sites.Length, ClippedTriangleCount: clippedTriangles, FragmentCount: fragmentCount, IncidentPairCount: incidentPairs, QueuePeakDepth: queuePeak,
            FragmentAreaMin: fragmentAreaMin, FragmentAreaMax: fragmentAreaMax, TotalArea: totalArea, SurfaceArea: meshArea, IntegrationResidual: integrationResidual,
            FirstMomentFiniteCount: firstMomentFinite, NeighborFacetCount: facets.Count, EmptyCellCount: emptyCells, BoundarySiteCount: boundarySites,
            DegenerateClipCount: degenerateClips, ClipDegeneracyCount: clipDegeneracies, NonFiniteDensityRejectionCount: densityRejections,
            AreaTolerance: policy.AreaFloor, LengthTolerance: policy.EdgeBand, KNearest: policy.KNearest, Density: policy.Density);
        RestrictedPowerDiagram diagram = new(Cells: new Arr<PowerCell>([.. powerCells]), Facets: new Arr<PowerFacet>([.. facets]), Receipt: receipt);
        return diagram.IsValid ? Fin.Succ(diagram) : Fin.Fail<RestrictedPowerDiagram>(key.InvalidResult());
    }

    // --- [COTANGENT_ASSEMBLY] ---------------------------------------------------------------
    // BOUNDARY ADAPTER — native face iteration builds sparse triplets without duplicating mesh storage.
    internal static Fin<SparseLaplacian> AssembleCotangent(Mesh mesh, Op key) {
        using Mesh active = mesh.DuplicateMesh();
        if (ContainsQuads(mesh: active) && !active.Faces.ConvertQuadsToTriangles())
            return Fin.Fail<SparseLaplacian>(key.InvalidResult());
        LaplacianTriplets triplets = new(vertexCount: active.Vertices.Count);
        double degenerateAreaFloor = DegenerateAreaFloorOf(scale: MeanEdgeLengthOf(mesh: active));
        for (int f = 0; f < active.Faces.Count; f++) {
            MeshFace face = active.Faces[index: f];
            if (!face.IsTriangle) continue;
            int va = face.A; int vb = face.B; int vc = face.C;
            Point3d pa = active.Vertices[index: va]; Point3d pb = active.Vertices[index: vb]; Point3d pc = active.Vertices[index: vc];
            Vector3d ab = pb - pa; Vector3d ac = pc - pa; Vector3d bc = pc - pb;
            double area = 0.5 * Vector3d.CrossProduct(a: ab, b: ac).Length;
            if (area < degenerateAreaFloor) { triplets.SkippedDegenerateFaces++; continue; }
            double cotA = -ab * -ac / (2.0 * area);
            double cotB = ab * -bc / (2.0 * area);
            double cotC = ac * bc / (2.0 * area);
            if (cotA < 0.0) triplets.NegativeCotangentCount++;
            if (cotB < 0.0) triplets.NegativeCotangentCount++;
            if (cotC < 0.0) triplets.NegativeCotangentCount++;
            triplets.AddTriangle(va: va, vb: vb, vc: vc, area: area, cotA: cotA, cotB: cotB, cotC: cotC);
        }
        return triplets.Build(key: key);
    }

    // --- [IDT_AND_NONMANIFOLD] --------------------------------------------------------------
    internal static Fin<IntrinsicMesh> BuildIntrinsicMesh(Mesh mesh, Op key) =>
        from source in IntrinsicMesh.FromMesh(mesh: mesh, key: key)
        from flipped in FlipToDelaunay(imesh: source, key: key)
        select flipped.Freeze();
    // Frozen intrinsic snapshot pinned by Laplacian kind: the cotangent kind preserves the input triangulation so
    // intrinsic angles stay consistent with the extrinsic positions; the Delaunay/tufted kinds run the IDT flip.
    internal static Fin<IntrinsicMesh> FrozenIntrinsicFor(Mesh mesh, MeshLaplacian kind, Op key) =>
        kind.Equals(MeshLaplacian.Cotangent)
            ? IntrinsicMesh.FromMesh(mesh: mesh, key: key).Map(static source => source.Freeze())
            : BuildIntrinsicMesh(mesh: mesh, key: key);

    [StructLayout(LayoutKind.Auto)]
    internal readonly record struct IntrinsicEdge(int Lo, int Hi, double Length, int Face0, int Face1, int NormalCoord = -1) {
        internal bool IsInterior => Face1 >= 0;
        internal bool IsOriginalEdge => NormalCoord < 0;
        internal int Crossings => Math.Max(val1: NormalCoord, val2: 0);
    }

    internal sealed class IntrinsicMesh {
        internal int VertexCount;
        internal Point3d[] Positions = [];
        internal readonly List<(int A, int B, int C)?> Triangles = [];
        internal readonly Dictionary<(int Lo, int Hi), (double Length, List<int> FaceIdx, int Normal)> EdgeData = [];
        internal bool HasFlips;
        internal int OriginalFaceCount;
        internal int DroppedNonTriangleFaces;
        internal int FlipCount;
        internal int ParityErrorCount;
        internal int BoundaryEdgeCount;
        internal int NonManifoldEdgeCount;
        private IntrinsicEdge[]? frozenEdges;
        private int[]? frozenNormalCoord;
        private Dictionary<(int Lo, int Hi), int>? frozenEdgeIndex;
        private int[][]? frozenFaceEdges;
        private double[]? frozenFaceAreas;
        private int[]? frozenFirstIncidentEdge;
        internal bool IsFrozen => frozenEdges is not null;
        internal int EdgeCount => frozenEdges?.Length ?? 0;
        internal int LiveFaceCount => Triangles.Count(static slot => slot.HasValue);
        internal IntrinsicEdge EdgeAt(int index) => frozenEdges![index];
        internal int NormalCoordAt(int index) => frozenNormalCoord![index];
        internal int SumNormalCoordinates {
            get { int sum = 0; if (frozenNormalCoord is { } coords) foreach (int n in coords) sum += Math.Max(val1: n, val2: 0); return sum; }
        }
        internal int TransverseEdgeCount {
            get { int count = 0; if (frozenNormalCoord is { } coords) foreach (int n in coords) count += n > 0 ? 1 : 0; return count; }
        }
        internal bool IsInteriorVertex(int vertex) {
            if (frozenEdges is not { } edges) return false;
            bool incident = false;
            foreach (IntrinsicEdge edge in edges)
                if (edge.Lo == vertex || edge.Hi == vertex) { incident = true; if (!edge.IsInterior) return false; }
            return incident;
        }
        internal int IndexOfEdge(int lo, int hi) =>
            frozenEdgeIndex is { } edges && edges.TryGetValue(key: EdgeKey(i: lo, j: hi), value: out int idx) ? idx : -1;
        internal int[] EdgesOfFace(int faceIdx) => frozenFaceEdges![faceIdx];
        internal double AreaOfFace(int faceIdx) => frozenFaceAreas![faceIdx];
        internal int FirstIncidentEdge(int vertexIdx) => frozenFirstIncidentEdge![vertexIdx];
        internal IEnumerable<int> LiveFaceIndices() {
            for (int f = 0; f < Triangles.Count; f++) if (Triangles[index: f].HasValue) yield return f;
        }
        internal IntrinsicMesh Freeze() {
            foreach (KeyValuePair<(int Lo, int Hi), (double Length, List<int> FaceIdx, int Normal)> kv in EdgeData)
                _ = kv.Value.FaceIdx.RemoveAll(match: face => face < 0 || face >= Triangles.Count || !Triangles[index: face].HasValue);
            foreach ((int Lo, int Hi) key in EdgeData
                         .Where(static kv => kv.Value.FaceIdx.Count == 0 || !RhinoMath.IsValidDouble(x: kv.Value.Length) || kv.Value.Length <= RhinoMath.ZeroTolerance)
                         .Select(static kv => kv.Key)
                         .ToArray())
                _ = EdgeData.Remove(key: key);
            BoundaryEdgeCount = EdgeData.Values.Count(static edge => edge.FaceIdx.Count == 1);
            NonManifoldEdgeCount = EdgeData.Values.Count(static edge => edge.FaceIdx.Count > 2);
            List<KeyValuePair<(int Lo, int Hi), (double Length, List<int> FaceIdx, int Normal)>> orderedEdges = [.. EdgeData
                .OrderBy(static kv => kv.Key.Lo)
                .ThenBy(static kv => kv.Key.Hi)];
            int eCount = orderedEdges.Count;
            frozenEdges = new IntrinsicEdge[eCount];
            frozenNormalCoord = new int[eCount];
            frozenEdgeIndex = new Dictionary<(int Lo, int Hi), int>(capacity: eCount);
            int idx = 0;
            foreach (KeyValuePair<(int Lo, int Hi), (double Length, List<int> FaceIdx, int Normal)> kv in orderedEdges) {
                List<int> faces = kv.Value.FaceIdx;
                int f0 = faces.Count > 0 ? faces[index: 0] : -1;
                int f1 = faces.Count > 1 ? faces[index: 1] : -1;
                frozenEdges[idx] = new IntrinsicEdge(Lo: kv.Key.Lo, Hi: kv.Key.Hi, Length: kv.Value.Length, Face0: f0, Face1: f1, NormalCoord: kv.Value.Normal);
                frozenNormalCoord[idx] = kv.Value.Normal;
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
            IntrinsicMesh m = new() { VertexCount = active.Vertices.Count, OriginalFaceCount = active.Faces.Count, Positions = new Point3d[active.Vertices.Count] };
            for (int v = 0; v < active.Vertices.Count; v++) m.Positions[v] = active.Vertices[index: v];
            for (int f = 0; f < active.Faces.Count; f++) {
                MeshFace face = active.Faces[index: f];
                if (!face.IsTriangle) { m.DroppedNonTriangleFaces++; continue; }
                Point3d pa = active.Vertices[index: face.A]; Point3d pb = active.Vertices[index: face.B]; Point3d pc = active.Vertices[index: face.C];
                _ = m.AddTriangle(a: face.A, b: face.B, c: face.C, lAB: pa.DistanceTo(other: pb), lBC: pb.DistanceTo(other: pc), lAC: pa.DistanceTo(other: pc));
            }
            return Fin.Succ(m);
        }
        internal int AddTriangle(int a, int b, int c, double lAB, double lBC, double lAC, int normalAB = -1, int normalBC = -1, int normalCA = -1) {
            int idx = Triangles.Count;
            Triangles.Add(item: (a, b, c));
            AddEdgeReference(i: a, j: b, length: lAB, faceIdx: idx, normal: normalAB);
            AddEdgeReference(i: b, j: c, length: lBC, faceIdx: idx, normal: normalBC);
            AddEdgeReference(i: a, j: c, length: lAC, faceIdx: idx, normal: normalCA);
            return idx;
        }
        private void AddEdgeReference(int i, int j, double length, int faceIdx, int normal = -1) {
            (int Lo, int Hi) key = EdgeKey(i: i, j: j);
            ref (double Length, List<int> FaceIdx, int Normal) edge = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary: EdgeData, key: key, exists: out bool exists);
            if (exists) {
                edge.Length = Math.Max(val1: edge.Length, val2: length);
                edge.FaceIdx.Add(item: faceIdx);
                return;
            }
            edge = (Length: length, FaceIdx: [faceIdx], Normal: normal);
        }
        internal int NormalCoordOf(int i, int j) => EdgeData[key: EdgeKey(i: i, j: j)].Normal;
        internal double EdgeLengthOf(int i, int j) => EdgeData[key: EdgeKey(i: i, j: j)].Length;
        internal int OppositeVertex(int faceIdx, int i, int j) {
            (int a, int b, int c) = Triangles[index: faceIdx]!.Value;
            return a != i && a != j ? a : b != i && b != j ? b : c;
        }
        internal int FaceAcrossEdge(int faceIdx, int i, int j) {
            if (!EdgeData.TryGetValue(key: EdgeKey(i: i, j: j), value: out (double Length, List<int> FaceIdx, int Normal) e)) return -1;
            foreach (int f in e.FaceIdx) if (f != faceIdx && f >= 0 && f < Triangles.Count && Triangles[index: f].HasValue) return f;
            return -1;
        }
        internal int AnyLiveFaceAtVertex(int vertex) {
            foreach (int f in LiveFaceIndices()) { (int a, int b, int c) = Triangles[index: f]!.Value; if (a == vertex || b == vertex || c == vertex) return f; }
            return -1;
        }
        internal bool IsInterior(int i, int j) =>
            EdgeData.TryGetValue(key: EdgeKey(i: i, j: j), value: out (double Length, List<int> FaceIdx, int Normal) e) && e.FaceIdx.Count == 2;
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
            int nij = NormalCoordOf(i: i, j: j);
            int njk = NormalCoordOf(i: j, j: k1); int nki = NormalCoordOf(i: k1, j: i);
            int nil = NormalCoordOf(i: i, j: k2); int nlj = NormalCoordOf(i: k2, j: j);
            int nkl = FlipNormalCoordinate(nij: nij, njk: njk, nki: nki, nil: nil, nlj: nlj);
            _ = EdgeData.Remove(key: key);
            Triangles[index: f1] = null; Triangles[index: f2] = null;
            (int, int)[] surrounding = [(i, k1), (j, k1), (i, k2), (j, k2)];
            for (int s = 0; s < surrounding.Length; s++) {
                (int Lo, int Hi) ek = EdgeKey(i: surrounding[s].Item1, j: surrounding[s].Item2);
                if (EdgeData.TryGetValue(key: ek, value: out (double Length, List<int> FaceIdx, int Normal) e)) { _ = e.FaceIdx.Remove(item: f1); _ = e.FaceIdx.Remove(item: f2); }
            }
            _ = AddTriangle(a: i, b: k1, c: k2, lAB: li_k1, lBC: l_k1k2, lAC: li_k2, normalBC: nkl);
            _ = AddTriangle(a: j, b: k1, c: k2, lAB: lj_k1, lBC: l_k1k2, lAC: lj_k2, normalBC: nkl);
            return Seq((i, k1), (j, k1), (i, k2), (j, k2));
        }
        // --- [NORMAL_COORDINATES] (FLIP-N) ---------------------------------------------------
        // Gillespie-Sharp-Crane integer flip update for the new diagonal kl after flipping ij in quad i-k-j-l.
        // Reference edges carry n=-1; arcsAlongIJ recovers the +1 crossing a flipped-away reference edge contributes.
        // Corner coordinates are carried doubled to keep the kernel integral; quadrupled = 2*doubledAnswer keeps the
        // parity invariant exact: an odd doubledAnswer (quadrupled mod 4 == 2) flags a corner-to-edge orientation defect.
        private int FlipNormalCoordinate(int nij, int njk, int nki, int nil, int nlj) {
            njk = Math.Max(val1: njk, val2: 0); nki = Math.Max(val1: nki, val2: 0);
            nil = Math.Max(val1: nil, val2: 0); nlj = Math.Max(val1: nlj, val2: 0);
            int arcsAlongIJ = -Math.Min(val1: nij, val2: 0);
            nij = Math.Max(val1: nij, val2: 0);
            int eIlj = Math.Max(val1: nlj - nij - nil, val2: 0);
            int eJil = Math.Max(val1: nil - nlj - nij, val2: 0);
            int eLji = Math.Max(val1: nij - nil - nlj, val2: 0);
            int eIjk = Math.Max(val1: njk - nki - nij, val2: 0);
            int eJki = Math.Max(val1: nki - nij - njk, val2: 0);
            int eKij = Math.Max(val1: nij - njk - nki, val2: 0);
            int cIljDoubled = -(Math.Min(val1: nlj - nij - nil, val2: 0) + eJil + eLji);
            int cJilDoubled = -(Math.Min(val1: nil - nlj - nij, val2: 0) + eIlj + eLji);
            int cLjiDoubled = -(Math.Min(val1: nij - nil - nlj, val2: 0) + eIlj + eJil);
            int cIjkDoubled = -(Math.Min(val1: njk - nki - nij, val2: 0) + eJki + eKij);
            int cJkiDoubled = -(Math.Min(val1: nki - nij - njk, val2: 0) + eIjk + eKij);
            int cKijDoubled = -(Math.Min(val1: nij - njk - nki, val2: 0) + eIjk + eJki);
            int quadrupledAnswer = (2 * cLjiDoubled) + (2 * cKijDoubled)
                                 + Math.Abs(value: cJilDoubled - cJkiDoubled) + Math.Abs(value: cIljDoubled - cIjkDoubled)
                                 - (2 * eLji) - (2 * eKij) + (4 * eIlj) + (4 * eIjk) + (4 * eJil) + (4 * eJki);
            ParityErrorCount += (quadrupledAnswer & 3) == 2 ? 1 : 0;
            return (int)Math.Round(value: quadrupledAnswer / 4.0, MidpointRounding.AwayFromZero) + arcsAlongIJ;
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
        double degenerateAreaFloor = DegenerateAreaFloorOf(scale: imesh.EdgeData.Count > 0 ? imesh.EdgeData.Values.Average(static e => e.Length) : 0.0);
        for (int f = 0; f < imesh.Triangles.Count; f++) {
            (int A, int B, int C)? slot = imesh.Triangles[index: f];
            if (slot is null) continue;
            (int va, int vb, int vc) = slot.Value;
            double lAB = imesh.EdgeLengthOf(i: va, j: vb);
            double lBC = imesh.EdgeLengthOf(i: vb, j: vc);
            double lAC = imesh.EdgeLengthOf(i: va, j: vc);
            double s = (lAB + lBC + lAC) * 0.5;
            double areaSq = s * (s - lAB) * (s - lBC) * (s - lAC);
            if (areaSq < degenerateAreaFloor * degenerateAreaFloor) { triplets.SkippedDegenerateFaces++; continue; }
            double area = Math.Sqrt(d: areaSq);
            double cotA = ((lAC * lAC) + (lAB * lAB) - (lBC * lBC)) / (4.0 * area);
            double cotB = ((lAB * lAB) + (lBC * lBC) - (lAC * lAC)) / (4.0 * area);
            double cotC = ((lAC * lAC) + (lBC * lBC) - (lAB * lAB)) / (4.0 * area);
            if (cotA < 0.0) triplets.NegativeCotangentCount++;
            if (cotB < 0.0) triplets.NegativeCotangentCount++;
            if (cotC < 0.0) triplets.NegativeCotangentCount++;
            triplets.AddTriangle(va: va, vb: vb, vc: vc, area: area, cotA: cotA, cotB: cotB, cotC: cotC);
        }
        return triplets.Build(key: key);
    }
    internal static Fin<SparseLaplacian> AssembleTuftedCotangentFromIntrinsic(IntrinsicMesh imesh, TuftedCoverPolicy policy, Op key) =>
        TuftedCoverMesh.Construct(imesh: imesh, policy: policy, key: key).Bind(cover => cover.Assemble(policy: policy, key: key));

    // The cover's flat-array integer glue loops and per-corner length arithmetic are the named statement-kernel exemption.
    // The cover duplicates every intrinsic triangle into a front (2t) and an orientation-reversed back (2t+1) sheet, then
    // side-glues each base edge's incident fan into a single cyclic chain so every cover edge is incident to exactly two
    // cover faces and boundary base edges glue front-to-back -- the closed, edge-manifold tufted cover of Sharp-Crane.
    internal sealed class TuftedCoverMesh {
        private readonly IntrinsicMesh baseMesh;
        private readonly int coverFaceCount;
        private readonly int halfEdgeCount;
        private readonly int[] faceVertexA, faceVertexB, faceVertexC;
        private readonly double[] faceLenAB, faceLenBC, faceLenCA;
        private readonly int[] glue;
        private readonly int[] heLo, heHi;
        private readonly Point3d[] basePositions;
        private readonly System.Collections.Generic.HashSet<(int Lo, int Hi)> originalBoundaryEdges = [];
        internal double MollificationEpsilon { get; private set; }
        internal double LengthScaleH { get; private set; }
        internal int DegenerateTriangleCount { get; private set; }
        internal double MinTriangleInequalitySlack { get; private set; }
        internal bool GluingMapIsBijection { get; private set; }
        internal int GluingSymmetryViolations { get; private set; }
        internal bool CoverIsEdgeManifold { get; private set; }
        internal bool CoverIsClosed { get; private set; }
        internal int CoverEdgeCount { get; private set; }
        internal int BoundaryBaseEdgeCount { get; private set; }
        internal int NonManifoldBaseEdgeCount { get; private set; }
        internal int IntrinsicFlips { get; private set; }
        internal int NonDelaunayEdgesRemaining { get; private set; }
        internal bool MaxFlipsHit { get; private set; }
        private TuftedCoverMesh(IntrinsicMesh baseMesh, int coverFaceCount) {
            this.baseMesh = baseMesh;
            this.coverFaceCount = coverFaceCount;
            basePositions = baseMesh.Positions;
            halfEdgeCount = coverFaceCount * 3;
            faceVertexA = new int[coverFaceCount]; faceVertexB = new int[coverFaceCount]; faceVertexC = new int[coverFaceCount];
            faceLenAB = new double[coverFaceCount]; faceLenBC = new double[coverFaceCount]; faceLenCA = new double[coverFaceCount];
            glue = new int[halfEdgeCount]; heLo = new int[halfEdgeCount]; heHi = new int[halfEdgeCount];
            System.Array.Fill(array: glue, value: -1);
        }
        internal static Fin<TuftedCoverMesh> Construct(IntrinsicMesh imesh, TuftedCoverPolicy policy, Op key) {
            int[] liveFaces = [.. imesh.LiveFaceIndices()];
            int liveCount = liveFaces.Length;
            if (liveCount == 0) return Fin.Fail<TuftedCoverMesh>(key.InvalidResult());
            Dictionary<int, int> baseFaceToLocal = new(capacity: liveCount);
            for (int t = 0; t < liveCount; t++) baseFaceToLocal[key: liveFaces[t]] = t;
            TuftedCoverMesh cover = new(baseMesh: imesh, coverFaceCount: liveCount * 2);
            foreach (KeyValuePair<(int Lo, int Hi), (double Length, List<int> FaceIdx, int Normal)> kv in imesh.EdgeData)
                if (kv.Value.FaceIdx.Count == 1) _ = cover.originalBoundaryEdges.Add(item: (kv.Key.Lo, kv.Key.Hi));
            double h = imesh.EdgeData.Count > 0 ? imesh.EdgeData.Values.Average(static e => e.Length) : 0.0;
            double delta = policy.MollifyEnabled ? policy.MollifyFactor.Value * h : 0.0;
            cover.MeasureAndMollify(liveFaces: liveFaces, delta: delta, h: h);
            cover.BuildGlue(imesh: imesh, baseFaceToLocal: baseFaceToLocal);
            cover.FlipToDelaunayCover(policy: policy);
            return cover.GluingMapIsBijection && cover.CoverIsEdgeManifold && cover.CoverIsClosed && !cover.MaxFlipsHit && cover.NonDelaunayEdgesRemaining == 0
                ? Fin.Succ(cover)
                : Fin.Fail<TuftedCoverMesh>(key.InvalidResult(detail: string.Create(provider: CultureInfo.InvariantCulture, $"Tufted cover failed structural guards: bijection={cover.GluingMapIsBijection} edgeManifold={cover.CoverIsEdgeManifold} closed={cover.CoverIsClosed} maxFlipsHit={cover.MaxFlipsHit} nonDelaunay={cover.NonDelaunayEdgesRemaining}.")));
        }
        // GLOBAL mollification (Sharp-Crane Eq.4): epsilon = max over all corners of max(0, delta - slack); a single epsilon is
        // added to every cover edge so the triangle inequality holds with delta-margin everywhere. Both sheets share lengths.
        // Corner c of a cover face owns half-edge 3*face+c: corner 0 = A->B, corner 1 = B->C, corner 2 = C->A.
        private void MeasureAndMollify(int[] liveFaces, double delta, double h) {
            double epsilon = 0.0; double minSlack = double.PositiveInfinity;
            for (int t = 0; t < liveFaces.Length; t++) {
                (int a, int b, int c) = baseMesh.Triangles[index: liveFaces[t]]!.Value;
                double lab = baseMesh.EdgeLengthOf(i: a, j: b), lbc = baseMesh.EdgeLengthOf(i: b, j: c), lca = baseMesh.EdgeLengthOf(i: c, j: a);
                double slack = Math.Min(val1: lab + lbc - lca, val2: Math.Min(val1: lbc + lca - lab, val2: lca + lab - lbc));
                minSlack = Math.Min(val1: minSlack, val2: slack);
                epsilon = Math.Max(val1: epsilon, val2: Math.Max(val1: 0.0, val2: delta - slack));
            }
            LengthScaleH = h;
            MollificationEpsilon = epsilon;
            MinTriangleInequalitySlack = double.IsFinite(d: minSlack) ? minSlack + epsilon : 0.0;
            int degenerate = 0;
            double degenerateAreaFloor = DegenerateAreaFloorOf(scale: h);
            for (int t = 0; t < liveFaces.Length; t++) {
                (int a, int b, int c) = baseMesh.Triangles[index: liveFaces[t]]!.Value;
                double lab = baseMesh.EdgeLengthOf(i: a, j: b) + epsilon;
                double lbc = baseMesh.EdgeLengthOf(i: b, j: c) + epsilon;
                double lca = baseMesh.EdgeLengthOf(i: c, j: a) + epsilon;
                WriteFace(face: 2 * t, va: a, vb: b, vc: c, lab: lab, lbc: lbc, lca: lca);
                WriteFace(face: (2 * t) + 1, va: a, vb: c, vc: b, lab: lca, lbc: lbc, lca: lab);
                double s = (lab + lbc + lca) * 0.5;
                if (s * (s - lab) * (s - lbc) * (s - lca) < degenerateAreaFloor * degenerateAreaFloor) degenerate += 2;
            }
            DegenerateTriangleCount = degenerate;
        }
        private void WriteFace(int face, int va, int vb, int vc, double lab, double lbc, double lca) {
            faceVertexA[face] = va; faceVertexB[face] = vb; faceVertexC[face] = vc;
            faceLenAB[face] = lab; faceLenBC[face] = lbc; faceLenCA[face] = lca;
            int baseHe = 3 * face;
            heLo[baseHe] = Math.Min(val1: va, val2: vb); heHi[baseHe] = Math.Max(val1: va, val2: vb);
            heLo[baseHe + 1] = Math.Min(val1: vb, val2: vc); heHi[baseHe + 1] = Math.Max(val1: vb, val2: vc);
            heLo[baseHe + 2] = Math.Min(val1: vc, val2: va); heHi[baseHe + 2] = Math.Max(val1: vc, val2: va);
        }
        // Side-glue map G-tilde (Sharp-Crane Alg.1): around each base edge with m incident base faces there are 2m cover sides
        // (front+back of each face). Order the incident faces into one circular dihedral fan, lay sides down as
        // front(f0),back(f0),front(f1),back(f1),... and pair them as the offset-by-one perfect matching on the 2m-cycle,
        // back(f_k)~front(f_{k+1}) with wrap back(f_{m-1})~front(f0). Each side lands in exactly one pair, every cover edge is
        // incident to exactly two cover faces, and the wrap glues a boundary edge (m=1) front-to-back, closing the cover.
        private void BuildGlue(IntrinsicMesh imesh, Dictionary<int, int> baseFaceToLocal) {
            int symmetryViolations = 0; bool bijection = true;
            foreach (KeyValuePair<(int Lo, int Hi), (double Length, List<int> FaceIdx, int Normal)> kv in imesh.EdgeData
                         .OrderBy(static row => row.Key.Lo).ThenBy(static row => row.Key.Hi)) {
                int lo = kv.Key.Lo; int hi = kv.Key.Hi;
                List<int> incident = kv.Value.FaceIdx;
                int m = incident.Count;
                if (m == 1) BoundaryBaseEdgeCount++; else if (m > 2) NonManifoldBaseEdgeCount++;
                int[] fan = OrderFanAroundEdge(imesh: imesh, lo: lo, hi: hi, incident: incident);
                int sides = fan.Length * 2;
                int[] sideHalfEdge = new int[sides];
                for (int s = 0; s < fan.Length; s++) {
                    int local = baseFaceToLocal[key: fan[s]];
                    sideHalfEdge[2 * s] = HalfEdgeOf(face: 2 * local, lo: lo, hi: hi);
                    sideHalfEdge[(2 * s) + 1] = HalfEdgeOf(face: (2 * local) + 1, lo: lo, hi: hi);
                }
                for (int k = 0; k < fan.Length; k++) {
                    int back = sideHalfEdge[(2 * k) + 1];
                    int frontNext = sideHalfEdge[2 * (k + 1) % sides];
                    if (glue[back] != -1 || glue[frontNext] != -1) { symmetryViolations++; bijection = false; continue; }
                    glue[back] = frontNext; glue[frontNext] = back;
                }
            }
            RecomputeGlueDiagnostics(ref symmetryViolations, ref bijection);
        }
        private void RecomputeGlueDiagnostics(ref int symmetryViolations, ref bool bijection) {
            int matched = 0;
            for (int he = 0; he < halfEdgeCount; he++) {
                if (glue[he] == -1 || glue[glue[he]] != he) { symmetryViolations++; bijection = false; } else matched++;
            }
            CoverEdgeCount = matched / 2;
            GluingMapIsBijection = bijection && symmetryViolations == 0;
            GluingSymmetryViolations = symmetryViolations;
            CoverIsEdgeManifold = matched == halfEdgeCount;
            CoverIsClosed = CoverIsEdgeManifold && CoverEdgeCount > 0;
        }
        private int HalfEdgeOf(int face, int lo, int hi) {
            int baseHe = 3 * face;
            int wantLo = Math.Min(val1: lo, val2: hi); int wantHi = Math.Max(val1: lo, val2: hi);
            for (int c = 0; c < 3; c++)
                if (heLo[baseHe + c] == wantLo && heHi[baseHe + c] == wantHi) return baseHe + c;
            return baseHe;
        }
        // Circular dihedral fan: sort the incident base faces by the opposite apex's signed angular position about the edge.
        // The host mesh carries 3D vertex positions, so the canonical fan is the cyclic order of the apex projections onto the
        // plane normal to the edge; absent an embedding the apex-vertex index is the stable tie-break that keeps the fan a fixed
        // circular sequence (the front/back alternation makes any stable cyclic order yield a valid closed edge-manifold cover).
        // A non-degenerate manifold edge (m<=2) and any embedded non-degenerate non-manifold fan (m>2) order by the true signed
        // dihedral angle. The index tie-break is reached only when an apex projection collapses onto the edge axis — a degenerate
        // (near-zero-area) incident triangle already counted by DegenerateTriangleCount. So a non-manifold fan (NonManifoldEdges>0)
        // whose apexes are all non-degenerate is geometrically exact; only the NonManifoldEdges>0 AND DegenerateTriangleCount>0
        // intersection yields a topologically-valid (guards pass) but geometrically-approximate glue, both witnessed on the receipt.
        private int[] OrderFanAroundEdge(IntrinsicMesh imesh, int lo, int hi, List<int> incident) {
            if (incident.Count <= 1) return [.. incident];
            Point3d pLo = basePositions[lo]; Point3d pHi = basePositions[hi];
            Vector3d axis = pHi - pLo;
            bool embedded = axis.Length > RhinoMath.ZeroTolerance && axis.Unitize();
            Vector3d reference = Vector3d.Zero; bool haveReference = false;
            (int Face, double Angle)[] keyed = new (int, double)[incident.Count];
            for (int i = 0; i < incident.Count; i++) {
                int opp = imesh.OppositeVertex(faceIdx: incident[i], i: lo, j: hi);
                double angle;
                if (embedded && opp >= 0 && opp < basePositions.Length) {
                    Vector3d radial = basePositions[opp] - pLo;
                    radial -= axis * (radial * axis);
                    if (radial.Length > RhinoMath.ZeroTolerance && radial.Unitize()) {
                        if (!haveReference) { reference = radial; haveReference = true; angle = 0.0; } else {
                            double cos = Math.Clamp(value: reference * radial, min: -1.0, max: 1.0);
                            double sin = Vector3d.CrossProduct(a: reference, b: radial) * axis;
                            double theta = Math.Atan2(y: sin, x: cos);
                            angle = theta < 0.0 ? theta + (2.0 * Math.PI) : theta;
                        }
                    } else angle = double.PositiveInfinity;
                } else angle = double.PositiveInfinity;
                keyed[i] = (incident[i], angle);
            }
            System.Array.Sort(array: keyed, comparison: static (left, right) =>
                left.Angle != right.Angle ? left.Angle.CompareTo(value: right.Angle) : left.Face.CompareTo(value: right.Face));
            int[] ordered = new int[incident.Count];
            for (int i = 0; i < incident.Count; i++) ordered[i] = keyed[i].Face;
            return ordered;
        }
        // Flip the now-closed edge-manifold cover toward intrinsic Delaunay; boundary base edges are interior in the cover and
        // so become flippable -- the headline guarantee. The frontier is a half-edge work queue keyed on the canonical (lower)
        // half-edge of each glued pair; each non-Delaunay glued edge flips its quadrilateral diagonal via the law-of-cosines,
        // rewriting both faces, re-gluing the four perimeter partners, and gluing the new diagonal between the two faces.
        private void FlipToDelaunayCover(TuftedCoverPolicy policy) {
            double tolerance = policy.DelaunayTolerance.Value;
            int cap = Math.Max(val1: 1, val2: halfEdgeCount * policy.MaxFlipsPerEdge.Value);
            Queue<int> frontier = new();
            for (int he = 0; he < halfEdgeCount; he++) frontier.Enqueue(item: he);
            int flips = 0;
            while (frontier.Count > 0 && flips < cap) {
                int he = frontier.Dequeue();
                if (glue[he] < 0 || glue[glue[he]] != he || IsDelaunayCover(he: he, tolerance: tolerance) || !IsFlippableCover(he: he)) continue;
                foreach (int touched in FlipCover(he: he)) frontier.Enqueue(item: touched);
                flips++;
            }
            int remaining = 0;
            for (int he = 0; he < halfEdgeCount; he++)
                if (glue[he] >= 0 && glue[he] > he && !IsDelaunayCover(he: he, tolerance: tolerance)) remaining++;
            IntrinsicFlips = flips;
            MaxFlipsHit = flips >= cap && remaining > 0;
            NonDelaunayEdgesRemaining = remaining;
            int violations = 0; bool bijection = true;
            RecomputeGlueDiagnostics(ref violations, ref bijection);
        }
        private double CornerAngleOppositeHalfEdge(int he) {
            int face = he / 3; int corner = he % 3;
            double lab = faceLenAB[face]; double lbc = faceLenBC[face]; double lca = faceLenCA[face];
            (double opposite, double adj1, double adj2) = corner switch {
                0 => (lab, lca, lbc),
                1 => (lbc, lab, lca),
                _ => (lca, lbc, lab),
            };
            double cos = adj1 > RhinoMath.ZeroTolerance && adj2 > RhinoMath.ZeroTolerance
                ? Math.Clamp(value: ((adj1 * adj1) + (adj2 * adj2) - (opposite * opposite)) / (2.0 * adj1 * adj2), min: -1.0, max: 1.0)
                : 1.0;
            return Math.Acos(d: cos);
        }
        private bool IsDelaunayCover(int he, double tolerance) =>
            CornerAngleOppositeHalfEdge(he: he) + CornerAngleOppositeHalfEdge(he: glue[he]) <= Math.PI + tolerance;
        // The flipped diagonal connects the two apexes; reject a flip whose apexes coincide or whose post-flip diagonal would
        // already be an existing edge of either face (a fold-back that would desymmetrise the cover).
        private bool IsFlippableCover(int he) {
            int twin = glue[he];
            int ki = ApexOf(he: he); int kj = ApexOf(he: twin);
            int i = heLo[he]; int j = heHi[he];
            return ki != kj && ki != i && ki != j && kj != i && kj != j;
        }
        private double HalfEdgeLength(int he) {
            int face = he / 3;
            return (he % 3) switch { 0 => faceLenAB[face], 1 => faceLenBC[face], _ => faceLenCA[face] };
        }
        private int ApexOf(int he) {
            int face = he / 3;
            int shared0 = heLo[he]; int shared1 = heHi[he];
            int va = faceVertexA[face]; int vb = faceVertexB[face]; int vc = faceVertexC[face];
            return va != shared0 && va != shared1 ? va : vb != shared0 && vb != shared1 ? vb : vc;
        }
        private int[] FlipCover(int he) {
            int twin = glue[he];
            int face0 = he / 3; int face1 = twin / 3;
            int i = heLo[he]; int j = heHi[he];
            int ki = ApexOf(he: he); int kj = ApexOf(he: twin);
            double lij = HalfEdgeLength(he: he);
            double liki = LengthBetween(face: face0, u: i, v: ki); double ljki = LengthBetween(face: face0, u: j, v: ki);
            double likj = LengthBetween(face: face1, u: i, v: kj); double ljkj = LengthBetween(face: face1, u: j, v: kj);
            double cosI0 = SafeAngleCos(opposite: ljki, adj1: lij, adj2: liki);
            double cosI1 = SafeAngleCos(opposite: ljkj, adj1: lij, adj2: likj);
            double sinI0 = Math.Sqrt(d: Math.Max(val1: 0.0, val2: 1.0 - (cosI0 * cosI0)));
            double sinI1 = Math.Sqrt(d: Math.Max(val1: 0.0, val2: 1.0 - (cosI1 * cosI1)));
            double cosSum = (cosI0 * cosI1) - (sinI0 * sinI1);
            double lkikj = Math.Sqrt(d: Math.Max(val1: 0.0, val2: (liki * liki) + (likj * likj) - (2.0 * liki * likj * cosSum)));
            // Snapshot the four perimeter partners by their pre-rewrite half-edge ids before WriteFace renumbers the corners.
            int sIki = HalfEdgeOf(face: face0, lo: i, hi: ki); int pIki = glue[sIki];
            int sJki = HalfEdgeOf(face: face0, lo: j, hi: ki); int pJki = glue[sJki];
            int sIkj = HalfEdgeOf(face: face1, lo: i, hi: kj); int pIkj = glue[sIkj];
            int sJkj = HalfEdgeOf(face: face1, lo: j, hi: kj); int pJkj = glue[sJkj];
            WriteFace(face: face0, va: i, vb: ki, vc: kj, lab: liki, lbc: lkikj, lca: likj);
            WriteFace(face: face1, va: j, vb: kj, vc: ki, lab: ljkj, lbc: lkikj, lca: ljki);
            RestoreSide(face: face0, u: i, v: ki, partner: pIki);
            RestoreSide(face: face0, u: i, v: kj, partner: pIkj);
            RestoreSide(face: face1, u: j, v: ki, partner: pJki);
            RestoreSide(face: face1, u: j, v: kj, partner: pJkj);
            int diag0 = HalfEdgeOf(face: face0, lo: ki, hi: kj);
            int diag1 = HalfEdgeOf(face: face1, lo: ki, hi: kj);
            glue[diag0] = diag1; glue[diag1] = diag0;
            return [.. Enumerable.Range(start: 3 * face0, count: 3), .. Enumerable.Range(start: 3 * face1, count: 3)];
        }
        // The flipped face's perimeter half-edge keeps its pre-flip partner: re-resolve its id from the rewritten face vertices
        // and re-point both ends of the involution. The diagonal is glued separately by the caller.
        private void RestoreSide(int face, int u, int v, int partner) {
            int he = HalfEdgeOf(face: face, lo: u, hi: v);
            glue[he] = partner;
            if (partner >= 0) glue[partner] = he;
        }
        private double LengthBetween(int face, int u, int v) {
            int he = HalfEdgeOf(face: face, lo: u, hi: v);
            return HalfEdgeLength(he: he);
        }
        private static double SafeAngleCos(double opposite, double adj1, double adj2) =>
            adj1 > RhinoMath.ZeroTolerance && adj2 > RhinoMath.ZeroTolerance
                ? Math.Clamp(value: ((adj1 * adj1) + (adj2 * adj2) - (opposite * opposite)) / (2.0 * adj1 * adj2), min: -1.0, max: 1.0)
                : 1.0;
        internal Fin<SparseLaplacian> Assemble(TuftedCoverPolicy policy, Op key) {
            double energyScale = policy.EnergyScaleFactor.Value;
            LaplacianTriplets triplets = new(vertexCount: baseMesh.VertexCount);
            double degenerateAreaFloor = DegenerateAreaFloorOf(scale: LengthScaleH);
            int negativeWeights = 0; double minCotan = double.PositiveInfinity; double minBoundaryCotan = double.PositiveInfinity; double coveredArea = 0.0;
            for (int cf = 0; cf < coverFaceCount; cf++) {
                int va = faceVertexA[cf]; int vb = faceVertexB[cf]; int vc = faceVertexC[cf];
                double lab = faceLenAB[cf]; double lbc = faceLenBC[cf]; double lca = faceLenCA[cf];
                double s = (lab + lbc + lca) * 0.5;
                double areaSq = s * (s - lab) * (s - lbc) * (s - lca);
                if (areaSq < degenerateAreaFloor * degenerateAreaFloor) { triplets.SkippedDegenerateFaces++; continue; }
                double area = Math.Sqrt(d: areaSq);
                coveredArea += area;
                double cotA = ((lca * lca) + (lab * lab) - (lbc * lbc)) / (4.0 * area);
                double cotB = ((lab * lab) + (lbc * lbc) - (lca * lca)) / (4.0 * area);
                double cotC = ((lca * lca) + (lbc * lbc) - (lab * lab)) / (4.0 * area);
                minCotan = Math.Min(val1: minCotan, val2: Math.Min(val1: cotA, val2: Math.Min(val1: cotB, val2: cotC)));
                if (IsOriginalBoundary(u: vb, v: vc)) minBoundaryCotan = Math.Min(val1: minBoundaryCotan, val2: cotA);
                if (IsOriginalBoundary(u: vc, v: va)) minBoundaryCotan = Math.Min(val1: minBoundaryCotan, val2: cotB);
                if (IsOriginalBoundary(u: va, v: vb)) minBoundaryCotan = Math.Min(val1: minBoundaryCotan, val2: cotC);
                if (cotA < 0.0) negativeWeights++;
                if (cotB < 0.0) negativeWeights++;
                if (cotC < 0.0) negativeWeights++;
                // Sharp-Crane halves the STIFFNESS only (two cover sheets collapse onto each base vertex, doubling the
                // Dirichlet energy); the mass reflects the true covered area, so energyScale scales the cotangents but
                // not the area — keeping TotalCoveredArea consistent with MinLumpedMass and the assembled mass matrix.
                triplets.AddTriangle(va: va, vb: vb, vc: vc, area: area, cotA: energyScale * cotA, cotB: energyScale * cotB, cotC: energyScale * cotC);
            }
            triplets.NegativeCotangentCount = negativeWeights;
            int captured = negativeWeights;
            return triplets.Build(key: key).Bind(laplacian => FinishReceipt(
                laplacian: laplacian, energyScale: energyScale,
                minCotan: double.IsFinite(d: minCotan) ? minCotan : 0.0,
                minBoundaryCotan: double.IsFinite(d: minBoundaryCotan) ? minBoundaryCotan : (double.IsFinite(d: minCotan) ? minCotan : 0.0),
                negativeWeights: captured, coveredArea: coveredArea, key: key));
        }
        private bool IsOriginalBoundary(int u, int v) => originalBoundaryEdges.Contains(item: (Math.Min(val1: u, val2: v), Math.Max(val1: u, val2: v)));
        private Fin<SparseLaplacian> FinishReceipt(SparseLaplacian laplacian, double energyScale, double minCotan, double minBoundaryCotan, int negativeWeights, double coveredArea, Op key) {
            (double symmetryResidual, double rowSumResidual) = OperatorResiduals(stiffness: laplacian.Stiffness);
            double minMass = laplacian.MassLumped.Count == 0 ? 0.0 : laplacian.MassLumped.Fold(double.PositiveInfinity, static (acc, value) => Math.Min(val1: acc, val2: value));
            TuftedLaplacianReceipt receipt = new(
                Kind: MeshLaplacian.TuftedIntrinsic,
                OriginalVertices: baseMesh.VertexCount,
                OriginalFaces: baseMesh.OriginalFaceCount,
                IntrinsicVertices: baseMesh.VertexCount,
                IntrinsicEdges: baseMesh.EdgeCount,
                IntrinsicFaces: baseMesh.LiveFaceCount,
                CoverFaces: coverFaceCount,
                CoverEdges: CoverEdgeCount,
                BoundaryEdges: BoundaryBaseEdgeCount,
                NonManifoldEdges: NonManifoldBaseEdgeCount,
                GluingMapIsBijection: GluingMapIsBijection,
                GluingSymmetryViolations: GluingSymmetryViolations,
                CoverIsEdgeManifold: CoverIsEdgeManifold,
                CoverIsClosed: CoverIsClosed,
                MollificationEpsilon: MollificationEpsilon,
                DegenerateTriangleCount: DegenerateTriangleCount,
                LengthScaleH: LengthScaleH,
                MinTriangleInequalitySlack: MinTriangleInequalitySlack,
                IntrinsicFlips: IntrinsicFlips,
                NonDelaunayEdgesRemaining: NonDelaunayEdgesRemaining,
                MaxFlipsHit: MaxFlipsHit,
                MinCotanEdgeWeight: minCotan,
                MinBoundaryEdgeWeight: minBoundaryCotan,
                NegativeWeightCount: negativeWeights,
                MinLumpedMass: minMass,
                TotalCoveredArea: coveredArea,
                EnergyScaleApplied: energyScale,
                SymmetryResidual: symmetryResidual,
                RowSumResidual: rowSumResidual,
                DroppedNonTriangleFaces: baseMesh.DroppedNonTriangleFaces,
                CoverAware: true,
                CollapsedToOriginalVertices: true);
            return receipt.IsValid
                ? Fin.Succ(laplacian with { Tufted = Some(receipt) })
                : Fin.Fail<SparseLaplacian>(key.InvalidResult(detail: "Tufted cover Laplacian failed PSD-proxy validation (symmetry/row-sum/nonnegative-weight)."));
        }
        // PSD-proxy witness over the assembled operator: symmetry residual = max|L_ij - L_ji|, row-sum residual = max|sum_j L_ij|.
        private static (double Symmetry, double RowSum) OperatorResiduals(SparseMatrix stiffness) {
            List<(int Row, int Col, double Value)> entries = MatrixKernel.SparseTripletsOf(matrix: stiffness);
            Dictionary<(int Row, int Col), double> dense = new(capacity: entries.Count);
            double[] rowSums = new double[stiffness.Rows.Value];
            foreach ((int row, int col, double value) in entries) {
                ref double cell = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary: dense, key: (row, col), out _);
                cell += value;
                rowSums[row] += value;
            }
            double symmetry = 0.0;
            foreach (KeyValuePair<(int Row, int Col), double> entry in dense) {
                double mirror = dense.TryGetValue(key: (entry.Key.Col, entry.Key.Row), value: out double other) ? other : 0.0;
                symmetry = Math.Max(val1: symmetry, val2: Math.Abs(value: entry.Value - mirror));
            }
            double rowSum = 0.0;
            for (int i = 0; i < rowSums.Length; i++) rowSum = Math.Max(val1: rowSum, val2: Math.Abs(value: rowSums[i]));
            return (symmetry, rowSum);
        }
    }

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
        List<(int Row, int Col, double Value)> triplets = MatrixKernel.SparseTripletsOf(matrix: laplacian.Stiffness, capacityBonus: n, scale: stiffnessScale);
        for (int i = 0; i < n; i++) triplets.Add(item: (i, i, massScale * laplacian.MassLumped[index: i]));
        Dimension dim = Dimension.Create(value: n);
        return SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: triplets, key: key);
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
    // Degenerate-triangle area floor derived from the local mesh length scale: (mean edge length)^2 * SqrtEpsilon, the same
    // scale-relative form SpdMassShift uses, guarded against a zero scale. Linear-area guards compare against this floor; the
    // signed-area-squared guards compare against its square — one owner so no raw absolute literal scatters across the face loops.
    private static double DegenerateAreaFloorOf(double scale) => Math.Max(val1: scale, val2: RhinoMath.ZeroTolerance) * Math.Max(val1: scale, val2: RhinoMath.ZeroTolerance) * RhinoMath.SqrtEpsilon;
    internal static Fin<TopologyReceipt> TopologyDetailed(MeshSpace space) {
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
        return Fin.Succ(new FlattenResult(Uvs: uvs, Mesh: output, Receipt: new FlattenReceipt(VertexCount: output.Vertices.Count, UvCount: uvs.Count, TextureCoordinateCount: output.TextureCoordinates.Count, BoundaryComponents: boundaryComponents, Valid: output.IsValid, EdgeLengthDistortionRms: distortion)));
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
        return from heatFactor in space.Cache.ScalarHeatCholesky(time: t, key: key)
               from delta in Fin.Succ(SpectralCore.BuildSourceDelta(n: n, sources: sources, mass: laplacian.MassLumped))
               from u in heatFactor.Solve(rhs: delta, key: key)
               from gradient in Fin.Succ(SpectralCore.ComputeTriangleGradients(mesh: space.Native, u: u))
               from divergence in Fin.Succ(SpectralCore.ComputeVertexDivergence(mesh: space.Native, gradients: gradient))
               from phi in laplacian.Stiffness.SingularSolve(rhs: divergence, gauge: GaugePolicy.Pinned(indices: sources, mass: Some(laplacian.MassLumped), shift: GaugeShift.MinZero), context: space.Tolerance, key: key)
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
        ClosestFace(space: space, sample: sample, key: key, project: (_, face, weights, _) => key.AcceptValue(value: InterpolateFaceValue(face: face, weights: weights, perVertex: perVertex)));
    private static double MeshSearchDistance(MeshSpace space) =>
        Math.Max(val1: space.Tolerance.Absolute.Value, val2: space.Cache.MeanEdgeLength);
    private static double InterpolateFaceValue(MeshFace face, double[] weights, Arr<double> perVertex) {
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

    internal static Fin<SignpostTransportReceipt> SignpostTransportReceiptOf(MeshSpace space, IntrinsicMesh imesh, Op key, Option<SignpostPolicy> policy = default) =>
        ConnectionEntriesOf(space: space, imesh: imesh, edgeAdjustment: Option<Arr<double>>.None, policy: policy.IfNone(SignpostPolicy.Default), key: key).Map(static entries => entries.Receipt);
    private static Fin<ConnectionEntries> EnumerateConnectionEntries(MeshSpace space, Option<Arr<double>> edgeAdjustment, Op key) =>
        space.Cache.IntrinsicMeshSnapshot(key: key).Bind(imesh => ConnectionEntriesOf(space: space, imesh: imesh, edgeAdjustment: edgeAdjustment, policy: SignpostPolicy.Default, key: key));
    private static Fin<ConnectionEntries> ConnectionEntriesOf(MeshSpace space, IntrinsicMesh imesh, Option<Arr<double>> edgeAdjustment, SignpostPolicy policy, Op key) {
        Mesh mesh = space.Native;
        FrameBundle fb = EnsureVertexFrames(mesh: mesh);
        int eCount = imesh.EdgeCount;
        bool hasAdjustment = edgeAdjustment.IsSome;
        Arr<double> adjustment = edgeAdjustment.IfNone(new Arr<double>([]));
        Arr<double> weights = SpectralCore.ComputeIntrinsicStar1(imesh: imesh);
        SignpostAngles signposts = BuildSignpostAngles(imesh: imesh, policy: policy);
        List<(int, int, double, double)> entries = new(capacity: eCount);
        int chordFallback = 0, missing = 0;
        double maxAngle = 0.0, maxResidual = 0.0, maxSignpostUpdate = 0.0;
        for (int e = 0; e < eCount; e++) {
            double w = weights[index: e];
            if (w < RhinoMath.ZeroTolerance) continue;
            IntrinsicEdge edge = imesh.EdgeAt(index: e);
            int i = edge.Lo, j = edge.Hi;
            Vector3d eij = (Vector3d)(mesh.Vertices[index: j] - mesh.Vertices[index: i]);
            double chordLength = eij.Length;
            double residual = chordLength <= RhinoMath.ZeroTolerance ? 0.0 : Math.Abs(value: chordLength - edge.Length) / Math.Max(val1: edge.Length, val2: RhinoMath.SqrtEpsilon);
            double phiAtI = signposts.AngleOf(vertex: i, neighbor: j);
            double phiAtJ = signposts.AngleOf(vertex: j, neighbor: i);
            (double rho, bool exact) = RhinoMath.IsValidDouble(x: phiAtI) && RhinoMath.IsValidDouble(x: phiAtJ)
                ? (rho: WrapAngle(angle: phiAtJ + Math.PI - phiAtI), exact: true)
                : eij.IsValid && chordLength > RhinoMath.ZeroTolerance
                    ? (rho: WrapAngle(angle: EdgeAngleInFrame(edge: -eij, xAxis: fb.X[j], yAxis: fb.Y[j]) - EdgeAngleInFrame(edge: eij, xAxis: fb.X[i], yAxis: fb.Y[i])), exact: false)
                    : (rho: double.NaN, exact: false);
            if (hasAdjustment && e < adjustment.Count) rho += adjustment[index: e];
            if (!RhinoMath.IsValidDouble(x: rho)) { missing++; continue; }
            chordFallback += exact ? 0 : 1;
            maxAngle = Math.Max(val1: maxAngle, val2: Math.Abs(value: WrapAngle(angle: rho)));
            maxResidual = Math.Max(val1: maxResidual, val2: residual);
            if (exact) maxSignpostUpdate = Math.Max(val1: maxSignpostUpdate, val2: signposts.UpdateResidualOf(vertex: i, neighbor: j));
            entries.Add(item: (i, j, w, rho));
        }
        CommonSubdivision subdivision = BuildCommonSubdivision(space: space, imesh: imesh, key: key);
        SignpostTransportReceipt receipt = new(
            VertexCount: mesh.Vertices.Count,
            IntrinsicEdgeCount: eCount,
            TransportedEdgeCount: entries.Count,
            IntrinsicFlipCount: imesh.FlipCount,
            ChordFallbackEdges: chordFallback,
            MissingFrameEdges: missing,
            CommonSubdivisionSegments: imesh.SumNormalCoordinates,
            TracedPathEdgeCount: imesh.TransverseEdgeCount,
            NormalCoordinateParityErrors: imesh.ParityErrorCount,
            SumNormalCoordinates: imesh.SumNormalCoordinates,
            IntrinsicSnapshot: imesh.IsFrozen,
            ExactCommonSubdivision: imesh.ParityErrorCount == 0 && subdivision.IsValid,
            MaxAngleRadians: maxAngle,
            MaxLengthResidual: maxResidual,
            MaxSignpostUpdateResidual: maxSignpostUpdate,
            Subdivision: subdivision.IsValid ? Some(subdivision) : Option<CommonSubdivision>.None);
        return receipt.IsValid && entries.Count > 0
            ? Fin.Succ(new ConnectionEntries(Rows: toSeq(entries), Receipt: receipt))
            : Fin.Fail<ConnectionEntries>(key.InvalidResult());
    }
    private static double WrapAngle(double angle) => angle - (RhinoMath.TwoPI * Math.Floor(d: (angle + Math.PI) / RhinoMath.TwoPI));

    // --- [SIGNPOST_ANGLES] (RS)/(SP) -----------------------------------------------------------
    // Per-vertex tangent-gauge halfedge angles from intrinsic corner angles, rescaled so the one-ring
    // spans 2pi exactly (RS). The reference direction (SP) is the policy gauge halfedge. The vertex
    // one-ring fan is walked face-to-face; boundary fans stay open. This is the named statement kernel.
    private sealed class SignpostAngles {
        private readonly Dictionary<(int Vertex, int Neighbor), double> angle = [];
        private readonly Dictionary<(int Vertex, int Neighbor), double> updateResidual = [];
        internal void Set(int vertex, int neighbor, double phi, double residual) {
            angle[key: (vertex, neighbor)] = phi;
            updateResidual[key: (vertex, neighbor)] = residual;
        }
        internal double AngleOf(int vertex, int neighbor) => angle.TryGetValue(key: (vertex, neighbor), value: out double phi) ? phi : double.NaN;
        internal double UpdateResidualOf(int vertex, int neighbor) => updateResidual.TryGetValue(key: (vertex, neighbor), value: out double r) ? r : 0.0;
    }
    private static SignpostAngles BuildSignpostAngles(IntrinsicMesh imesh, SignpostPolicy policy) {
        SignpostAngles table = new();
        double rescaleFloor = policy.VertexAngleRescaleFloor.Value;
        Dictionary<int, List<(int Face, int Prev, int Next, double Corner)>> ring = BuildVertexRing(imesh: imesh);
        foreach ((int vertex, List<(int Face, int Prev, int Next, double Corner)> fan) in ring) {
            if (fan.Count == 0) continue;
            List<(int Next, double Corner)> ordered = OrderFan(imesh: imesh, vertex: vertex, fan: fan);
            // A non-closing fan leaves its neighbors unset (NaN) for the witnessed chord fallback; rescaling a partial sum would corrupt them.
            if (ordered.Count == 0) continue;
            bool interior = imesh.IsInteriorVertex(vertex: vertex);
            double angleSum = ordered.Sum(static corner => corner.Corner);
            double span = interior ? RhinoMath.TwoPI : Math.PI;
            double rescale = angleSum > rescaleFloor ? span / angleSum : 1.0;
            // Rescaling forces the gauge disk to span exactly 2pi (interior) / pi (boundary); the closure
            // residual is the float-precision error of that rescaled sum, NOT the cone-point angle defect.
            double closureResidual = angleSum > rescaleFloor ? Math.Abs(value: (angleSum * rescale) - span) / span : 0.0;
            int gaugeIndex = SelectGauge(ordered: ordered, gauge: policy.ReferenceDirectionGauge);
            double cumulative = 0.0;
            for (int s = 0; s < ordered.Count; s++) {
                int idx = (gaugeIndex + s) % ordered.Count;
                (int neighbor, double corner) = ordered[index: idx];
                double phi = cumulative * rescale;
                table.Set(vertex: vertex, neighbor: neighbor, phi: WrapAngle(angle: phi), residual: closureResidual);
                cumulative += corner;
            }
        }
        return table;
    }
    private static Dictionary<int, List<(int Face, int Prev, int Next, double Corner)>> BuildVertexRing(IntrinsicMesh imesh) {
        Dictionary<int, List<(int Face, int Prev, int Next, double Corner)>> ring = [];
        foreach (int f in imesh.LiveFaceIndices()) {
            (int a, int b, int c) = imesh.Triangles[index: f]!.Value;
            AddCorner(ring: ring, imesh: imesh, face: f, vertex: a, prev: c, next: b);
            AddCorner(ring: ring, imesh: imesh, face: f, vertex: b, prev: a, next: c);
            AddCorner(ring: ring, imesh: imesh, face: f, vertex: c, prev: b, next: a);
        }
        return ring;
    }
    private static void AddCorner(Dictionary<int, List<(int Face, int Prev, int Next, double Corner)>> ring, IntrinsicMesh imesh, int face, int vertex, int prev, int next) {
        double lPrev = imesh.EdgeLengthOf(i: vertex, j: prev);
        double lNext = imesh.EdgeLengthOf(i: vertex, j: next);
        double lOpp = imesh.EdgeLengthOf(i: prev, j: next);
        double corner = AngleFromLengths(opposite: lOpp, adjacent1: lPrev, adjacent2: lNext);
        if (!ring.TryGetValue(key: vertex, value: out List<(int Face, int Prev, int Next, double Corner)>? fan)) { fan = []; ring[key: vertex] = fan; }
        fan.Add(item: (Face: face, Prev: prev, Next: next, Corner: corner));
    }
    private static double AngleFromLengths(double opposite, double adjacent1, double adjacent2) {
        double denom = 2.0 * adjacent1 * adjacent2;
        double cos = denom > RhinoMath.ZeroTolerance ? ((adjacent1 * adjacent1) + (adjacent2 * adjacent2) - (opposite * opposite)) / denom : 1.0;
        return Math.Acos(d: Math.Min(val1: 1.0, val2: Math.Max(val1: -1.0, val2: cos)));
    }
    // The one-ring is ordered by face adjacency, not by the Prev-keyed chain: an IDT flip emits its two new triangles
    // co-oriented around the shared diagonal, so two corners collide on Prev while every face stays unique per vertex.
    // Walking face-to-face through FaceAcrossEdge crosses the live exit edge, so the gauge edges land in true angular
    // order for flipped fans too. A fan that does not close into one manifold ring returns empty, leaving its neighbors
    // unset so ConnectionEntriesOf routes them to the witnessed chord fallback instead of rescaling a partial angle sum.
    private static List<(int Next, double Corner)> OrderFan(IntrinsicMesh imesh, int vertex, List<(int Face, int Prev, int Next, double Corner)> fan) {
        Dictionary<int, (int Prev, int Next, double Corner)> byFace = fan.ToDictionary(static corner => corner.Face, static corner => (corner.Prev, corner.Next, corner.Corner));
        bool LiveAcross(int face, int neighbor) => imesh.FaceAcrossEdge(faceIdx: face, i: vertex, j: neighbor) is int across && across >= 0 && byFace.ContainsKey(key: across);
        // A boundary fan opens at the ring edge with no live face across it; enter from that edge so the open chain walks
        // end-to-end. An interior fan closes, so any seed face works and SelectGauge fixes the reference downstream.
        int startFace = fan[index: 0].Face, entry = fan[index: 0].Prev;
        foreach ((int face, int prev, int next, double _) in fan) {
            if (!LiveAcross(face: face, neighbor: prev)) { (startFace, entry) = (face, prev); break; }
            if (!LiveAcross(face: face, neighbor: next)) { (startFace, entry) = (face, next); break; }
        }
        List<(int Next, double Corner)> ordered = new(capacity: fan.Count + 1);
        System.Collections.Generic.HashSet<int> visited = [];
        int faceCursor = startFace;
        while (byFace.TryGetValue(key: faceCursor, value: out (int Prev, int Next, double Corner) corner) && visited.Add(item: faceCursor)) {
            int exit = entry == corner.Prev ? corner.Next : corner.Prev;
            ordered.Add(item: (Next: entry, corner.Corner));
            int nextFace = imesh.FaceAcrossEdge(faceIdx: faceCursor, i: vertex, j: exit);
            if (nextFace == startFace) break;
            if (nextFace < 0 || !byFace.ContainsKey(key: nextFace)) { ordered.Add(item: (Next: exit, Corner: 0.0)); break; }
            (faceCursor, entry) = (nextFace, exit);
        }
        return visited.Count == fan.Count ? ordered : [];
    }
    private static int SelectGauge(List<(int Next, double Corner)> ordered, SignpostGauge gauge) =>
        gauge.Equals(SignpostGauge.LowestVertexNeighbor)
            ? Math.Max(val1: 0, val2: ordered.FindIndex(match: corner => corner.Next == ordered.Min(static c => c.Next)))
            : 0;

    // --- [COMMON_SUBDIVISION] overlay S = overlay(M,T) -----------------------------------------
    // The overlay vertex set is the VertexCount shared vertices (every IDT vertex coincides with an
    // input-mesh vertex) plus the SumNormalCoordinates EDGE_TRANSVERSE crossing vertices. Each transverse
    // T-edge e (e.Crossings>0) is a straight geodesic on the source mesh M; TraceIntrinsicEdgeAcrossSource
    // (a seat front-end of the ONE shared WalkChart unfold kernel, driven in EdgeOverlay mode — the same kernel the
    // IVP TraceStraightestGeodesic and the BVP log replay walk, so no unfold arithmetic is forked) walks e.Lo->e.Hi
    // over M and recovers the c ordered input-edge crossings: each carries the M-edge it cuts and the exact
    // barycentric parameter u along that M-edge.
    // InterpolationA (M side) is the per-vertex identity on the shared rows plus one M-barycentric row per
    // crossing ((1-u) on the cut M-edge's Lo, u on its Hi; a single nonzero when the crossing grazes an
    // M-vertex within VertexSnap*edgeLength, never a near-singular row). InterpolationB (T side) is the
    // per-vertex identity plus one T-barycentric row per crossing ((1-t_k) on e.Lo, t_k on e.Hi at the exact
    // t_k=(k+1)/(c+1)). EdgeLengthInterpolationResidual is the genuine witness — the worst-edge relative gap
    // between the straight ray's arrival point and e.Hi's unfolded position — so a wrong overlay (a bad seat
    // angle or a missed crossing) fails CommonSubdivision.IsValid through RowSumResidualOf (partition of unity)
    // and this residual, never silently. The n_e=0-everywhere case (tetrahedron / no flip) stays the exact
    // identity fast path. The triplet assembly and per-crossing unfold are the named statement kernel; the
    // M-face centroid reads are the platform-forced boundary copied to detached values at the read site.
    private static CommonSubdivision BuildCommonSubdivision(MeshSpace space, IntrinsicMesh imesh, Op key) {
        int vertexCount = imesh.VertexCount;
        int[] liveFaces = [.. imesh.LiveFaceIndices()];
        int faceCount = liveFaces.Length;
        int crossingTotal = imesh.SumNormalCoordinates;
        int subdivisionVertexCount = vertexCount + crossingTotal;
        List<(int Row, int Col, double Value)> tripletsA = new(capacity: subdivisionVertexCount + (2 * crossingTotal));
        List<(int Row, int Col, double Value)> tripletsB = new(capacity: subdivisionVertexCount + (2 * crossingTotal));
        for (int v = 0; v < vertexCount; v++) { tripletsA.Add(item: (v, v, 1.0)); tripletsB.Add(item: (v, v, 1.0)); }
        double edgeLengthInterpolationResidual = crossingTotal <= 0
            ? 0.0
            : space.Cache.EnsureFrozenIntrinsic(kind: MeshLaplacian.Cotangent, key: key).Match(
                Succ: source => AccumulateTransverseCrossings(source: source, imesh: imesh, baseRow: vertexCount, tripletsA: tripletsA, tripletsB: tripletsB),
                Fail: static _ => double.PositiveInfinity);
        int[] sourceFaceA = new int[faceCount];
        int[] sourceFaceB = new int[faceCount];
        for (int f = 0; f < faceCount; f++) {
            sourceFaceB[f] = liveFaces[f];
            sourceFaceA[f] = MFaceOfIntrinsicFace(space: space, imesh: imesh, face: liveFaces[f]);
        }
        Dimension cols = Dimension.Create(value: Math.Max(val1: 1, val2: vertexCount));
        Dimension rows = Dimension.Create(value: Math.Max(val1: 1, val2: subdivisionVertexCount));
        return SparseMatrix.FromTriplets(rows: rows, cols: cols, triplets: tripletsA, key: key)
            .Bind(a => SparseMatrix.FromTriplets(rows: rows, cols: cols, triplets: tripletsB, key: key).Map(b => (A: a, B: b)))
            .Map(ab => new CommonSubdivision(
                SubdivisionVertexCount: subdivisionVertexCount,
                SubdivisionFaceCount: faceCount,
                SourceFaceA: new Arr<int>(sourceFaceA),
                SourceFaceB: new Arr<int>(sourceFaceB),
                InterpolationA: ab.A,
                InterpolationB: ab.B,
                RowSumResidualA: RowSumResidualOf(matrix: ab.A),
                RowSumResidualB: RowSumResidualOf(matrix: ab.B),
                EdgeLengthInterpolationResidual: edgeLengthInterpolationResidual))
            .IfFail(static _ => default);
    }
    // Scatter the M-side and T-side barycentric rows for every transverse T-edge and return the worst-edge
    // relative residual. The crossing rows occupy baseRow.. in declaration order across edges; the ordered
    // crossings of one edge land at t_k=(k+1)/(c+1). The residual is the relative arrival gap of the BVP trace
    // (how far the straight ray of length e.Length lands from e.Hi's unfolded position) — a genuine geometric
    // witness that the seat angle and the recovered crossings are consistent, not a self-cancelling chord sum.
    // Returns +inf when a transverse edge fails to recover its c crossings (so the residual gate rejects the
    // overlay) rather than silently emitting an identity row.
    private static double AccumulateTransverseCrossings(IntrinsicMesh source, IntrinsicMesh imesh, int baseRow, List<(int Row, int Col, double Value)> tripletsA, List<(int Row, int Col, double Value)> tripletsB) {
        double snapFraction = GeodesicTracePolicy.Default.VertexSnap.Value;
        int eCount = imesh.EdgeCount;
        int row = baseRow;
        double worst = 0.0;
        for (int e = 0; e < eCount; e++) {
            IntrinsicEdge edge = imesh.EdgeAt(index: e);
            int c = edge.Crossings;
            if (c <= 0) continue;
            ExpTrace trace = TraceIntrinsicEdgeAcrossSource(source: source, lo: edge.Lo, hi: edge.Hi, edgeLength: edge.Length);
            if (trace.Crossings.Count != c || trace.Stop != GeodesicStopKind.LengthReached) return double.PositiveInfinity;
            for (int k = 0; k < c; k++) {
                (int mEdge, double u) = trace.Crossings[index: k];
                double tK = (k + 1.0) / (c + 1.0);
                tripletsB.Add(item: (row, edge.Lo, 1.0 - tK));
                tripletsB.Add(item: (row, edge.Hi, tK));
                IntrinsicEdge cut = source.EdgeAt(index: mEdge);
                bool nearLo = u <= snapFraction;
                bool nearHi = u >= 1.0 - snapFraction;
                if (nearLo) tripletsA.Add(item: (row, cut.Lo, 1.0));
                else if (nearHi) tripletsA.Add(item: (row, cut.Hi, 1.0));
                else { tripletsA.Add(item: (row, cut.Lo, 1.0 - u)); tripletsA.Add(item: (row, cut.Hi, u)); }
                row++;
            }
            worst = Math.Max(val1: worst, val2: ArrivalGapOf(trace: trace, source: source, hi: edge.Hi) / Math.Max(val1: edge.Length, val2: RhinoMath.SqrtEpsilon));
        }
        return worst;
    }
    // BVP edge-trace entry of the ONE geodesic tracer: seat the lo->hi chart bearing from the START-FACE 2D layout
    // (consistent with the IVP/BVP seats — never blending extrinsic Point3d chords with intrinsic edge lengths), then
    // drive the shared WalkChart in EdgeOverlay mode (raw crossings, vertex-snap suppressed, stop at hi). The kernel
    // records the cut M-edge + barycentric U per crossing and the unfolded arrival endpoint; the walk loop is owned by
    // WalkChart, so this is a seat front-end, not a forked tracer.
    private static ExpTrace TraceIntrinsicEdgeAcrossSource(IntrinsicMesh source, int lo, int hi, double edgeLength) {
        int startFace = source.AnyLiveFaceAtVertex(vertex: lo);
        if (startFace < 0 || !(edgeLength > RhinoMath.ZeroTolerance)) return EmptyEdgeTrace(startFace: Math.Max(val1: 0, val2: startFace));
        (int a0, int b0, int c0) = source.Triangles[index: startFace]!.Value;
        (int va, int vb, int vc) = lo == a0 ? (a0, b0, c0) : lo == b0 ? (b0, c0, a0) : (c0, a0, b0);
        // Seat lo->hi by the in-chart bearing of hi's unfolded position about lo in the start-face layout: a metric-consistent
        // bearing (the chart is built from intrinsic edge lengths, hi placed by its two intrinsic legs to va/vb), never the
        // extrinsic chord-vs-intrinsic mix. ChartAngleToward returns the angle of the source->toward leg in the +x layout.
        double bearing = ChartAngleToward(imesh: source, va: va, vb: vb, vc: vc, toward: hi);
        return !RhinoMath.IsValidDouble(x: bearing)
            ? EmptyEdgeTrace(startFace: startFace)
            : WalkChart(imesh: source, startFace: startFace, va: va, vb: vb, vc: vc, seatAngle: bearing, seatedWorldDir: Vector3d.Zero, traceLength: edgeLength, mode: GeodesicWalkMode.EdgeOverlay, stopAtVertex: hi, policy: GeodesicTracePolicy.Default);
    }
    private static ExpTrace EmptyEdgeTrace(int startFace) =>
        new(SeatedWorldDir: Vector3d.Zero, TracedLength: 0.0, PathFaces: [], CrossedEdges: [], EdgeCrossingCount: 0, VertexPassCount: 0, Stop: GeodesicStopKind.IterationCap, Crossings: [], EndX: 0.0, EndY: 0.0, ArrivalFace: startFace, ReachedStopVertex: false);
    // The relative arrival gap: the 2D distance between the overlay walk's recovered endpoint and hi's unfolded position in the
    // arrival face — the geometric witness that the straight ray reaches hi. hi is a corner of the arrival face on a clean trace.
    private static double ArrivalGapOf(ExpTrace trace, IntrinsicMesh source, int hi) {
        (int a, int b, int c) = source.Triangles[index: trace.ArrivalFace]!.Value;
        int hiLocal = a == hi ? 0 : b == hi ? 1 : c == hi ? 2 : -1;
        if (hiLocal < 0) return double.PositiveInfinity;
        double[] px = new double[3]; double[] py = new double[3];
        LayoutFace(imesh: source, va: a, vb: b, vc: c, px: px, py: py);
        return Math.Sqrt(d: ((trace.EndX - px[hiLocal]) * (trace.EndX - px[hiLocal])) + ((trace.EndY - py[hiLocal]) * (trace.EndY - py[hiLocal])));
    }
    // In-chart angle of the va->toward leg measured in the (va at origin, vb on +x) layout. Falls back to the +x reference (the
    // va->vb edge) when toward is not a non-origin vertex of the seat face. The single seat geometry the IVP forward seat inverts:
    // forward maps world->chart, this reads chart directly. (RhinoMath.IsValidDouble gate at the call site rejects a NaN seat.)
    private static double ChartAngleToward(IntrinsicMesh imesh, int va, int vb, int vc, int toward) {
        double[] px = new double[3]; double[] py = new double[3];
        LayoutFace(imesh: imesh, va: va, vb: vb, vc: vc, px: px, py: py);
        int local = toward == vb ? 1 : toward == vc ? 2 : -1;
        return local < 0 ? 0.0 : Math.Atan2(y: py[local], x: px[local]);
    }
    // The source M-face an intrinsic (T) face overlaps: the M-face the T-face centroid lands in. T shares M's
    // vertices, so the centroid is the mean of the three intrinsic vertex positions; ClosestMeshPoint resolves
    // the containing original-mesh face, falling back to the same ordinal for the no-flip (M==T) case.
    private static int MFaceOfIntrinsicFace(MeshSpace space, IntrinsicMesh imesh, int face) {
        (int a, int b, int c) = imesh.Triangles[index: face]!.Value;
        Point3d centroid = new(x: (imesh.Positions[a].X + imesh.Positions[b].X + imesh.Positions[c].X) / 3.0, y: (imesh.Positions[a].Y + imesh.Positions[b].Y + imesh.Positions[c].Y) / 3.0, z: (imesh.Positions[a].Z + imesh.Positions[b].Z + imesh.Positions[c].Z) / 3.0);
        MeshPoint meshPoint = space.Native.ClosestMeshPoint(testPoint: centroid, maximumDistance: MeshSearchDistance(space: space));
        return meshPoint is { FaceIndex: >= 0 } ? meshPoint.FaceIndex : Math.Max(val1: 0, val2: face);
    }
    private static double RowSumResidualOf(SparseMatrix matrix) {
        int rows = matrix.Rows.Value;
        double maxResidual = 0.0;
        for (int r = 0; r < rows; r++) {
            double sum = 0.0;
            for (int p = matrix.RowPtr[index: r]; p < matrix.RowPtr[index: r + 1]; p++) sum += matrix.Values[index: p];
            maxResidual = Math.Max(val1: maxResidual, val2: Math.Abs(value: sum - 1.0));
        }
        return maxResidual;
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
    // The LOBPCG residual gate compares the ABSOLUTE block-defect Frobenius norm ‖Lconn·V − V·Λ‖_F against tolerance, so a bare
    // absolute floor misreads the connection Laplacian's magnitude; the floor travels relative to the operator scale (the full-
    // Hermitian Frobenius norm) at RhinoMath.SqrtEpsilon (the canonical relative-residual unit). The iteration ceiling travels off
    // the Krylov dimension that resolves the smallest pair of an n×n Laplacian (Θ(√n)) times a Krylov budget, clamped to n.
    private const int CrossFieldKrylovBudget = 16;
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
            .Bind(Lconn => Lconn.SmallestEigenpairsDetailed(
                    k: 1,
                    tolerance: RhinoMath.SqrtEpsilon * ConnectionOperatorScale(connection: Lconn),
                    maxIterations: KrylovIterationCap(order: Lconn.Order.Value, blocks: 1),
                    key: key)
                .Bind(receipt => receipt.Stop.Equals(EigenSolveStop.ResidualConverged) ? Fin.Succ(receipt.Pairs) : Fin.Fail<Seq<(double Eigenvalue, Arr<Complex> Eigenvector)>>(key.InvalidResult())))
            .Bind(pairs => pairs.Count > 0
                ? Fin.Succ(pairs[index: 0])
                : Fin.Fail<(double Eigenvalue, Arr<Complex> Eigenvector)>(error: key.InvalidResult()))
            .Map(head => NormaliseComplexEigenvector(eigenvector: head.Eigenvector));
    // Operator scale of the upper-triangular Hermitian connection Laplacian: the full-matrix Frobenius norm, reconstructing the
    // mirrored lower triangle (off-diagonal magnitudes count twice, the real diagonal once). Floored at SqrtEpsilon so a zero
    // operator never yields a zero tolerance. This is the σ-scale the relative LOBPCG residual floor is measured against.
    private static double ConnectionOperatorScale(SparseHermitian connection) {
        double diagSq = 0.0; double offSq = 0.0;
        for (int row = 0; row < connection.Order.Value; row++)
            for (int p = connection.RowPtr[index: row]; p < connection.RowPtr[index: row + 1]; p++) {
                double magSq = (connection.Values[index: p] * Complex.Conjugate(value: connection.Values[index: p])).Real;
                if (connection.ColInd[index: p] == row) diagSq += magSq; else offSq += magSq;
            }
        return Math.Max(val1: RhinoMath.SqrtEpsilon, val2: Math.Sqrt(d: diagSq + (2.0 * offSq)));
    }
    // Krylov-dimension-relative iteration ceiling: the smallest eigenpair of an n×n Laplacian resolves in Θ(√n) Krylov steps, so
    // the cap travels off ceil(√n) times the block count and the Krylov budget, clamped to the matrix order (an exact subspace
    // basis cannot exceed n). Replaces a fixed magic ceiling with a dimension-relative bound per CRITERION_PRECEDENCE.
    private static int KrylovIterationCap(int order, int blocks) =>
        Math.Min(val1: Math.Max(val1: 1, val2: order), val2: Math.Max(val1: blocks, val2: 1) * CrossFieldKrylovBudget * (int)Math.Ceiling(a: Math.Sqrt(d: Math.Max(val1: 1, val2: order))));
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
            quadCase: static (state, quad) => state.Key.Catch(() => {
                QuadRemeshParameters parameters = QuadParametersOf(quad: quad);
                Mesh? result = state.Space.Native.QuadRemesh(faceBlocks: quad.FaceBlocks.AsIterable(), parameters: parameters, guideCurves: quad.GuideCurves.AsIterable(), progress: null, cancelToken: CancellationToken.None);
                if (result is { IsValid: true })
                    return Fin.Succ(new RemeshResult(Mesh: result, Receipt: QuadReceiptOf(quad: quad, parameters: parameters, source: state.Space.Native, output: result)));
                result?.Dispose();
                return Fin.Fail<RemeshResult>(error: state.Key.InvalidResult());
            }),
            simplifyCase: static (state, simplify) => state.Key.Catch(() => {
                Mesh clone = state.Space.Native.DuplicateMesh();
                if (clone.Reduce(parameters: simplify.Parameters) && clone.IsValid)
                    return Fin.Succ(new RemeshResult(Mesh: clone, Receipt: ReduceReceiptOf(kind: simplify, source: state.Space.Native, output: clone)));
                clone.Dispose();
                return Fin.Fail<RemeshResult>(error: state.Key.InvalidResult(detail: Optional(simplify.Parameters.Error).Filter(static text => !string.IsNullOrWhiteSpace(value: text)).IfNone(() => $"{nameof(RemeshStatus.InvalidOutput)}: mesh reduction produced no valid output")));
            })));
    private static QuadRemeshParameters QuadParametersOf(RemeshKind.QuadCase quad) {
        QuadRemeshParameters parameters = new() { DetectHardEdges = quad.DetectHardEdges, GuideCurveInfluence = quad.GuideInfluence.Key, PreserveMeshArrayEdgesMode = quad.PreserveEdges.Key, SymmetryAxis = quad.SymmetryAxis };
        switch (quad.Target) {
            case QuadTarget.EdgeLengthCase edge:
                parameters.TargetEdgeLength = edge.Length.Value;
                break;
            case QuadTarget.QuadCountCase count:
                parameters.TargetQuadCount = count.Count.Value;
                parameters.AdaptiveSize = count.AdaptiveSize.Value * NativeAdaptiveScale;
                parameters.AdaptiveQuadCount = count.AdaptiveQuadCount;
                break;
        }
        return parameters;
    }
    private static RemeshReceipt QuadReceiptOf(RemeshKind.QuadCase quad, QuadRemeshParameters parameters, Mesh source, Mesh output) =>
        TopologyOf(kind: quad, source: source, output: output) with {
            TargetLength = quad.Target is QuadTarget.EdgeLengthCase edge ? Some(edge.Length.Value) : Option<double>.None,
            TargetQuadCount = quad.Target is QuadTarget.QuadCountCase ? Some(parameters.TargetQuadCount) : Option<int>.None,
            AdaptiveSize = Some(parameters.AdaptiveSize),
            AdaptiveQuadCount = Some(parameters.AdaptiveQuadCount),
            HardEdgePreservationRequested = Some(quad.DetectHardEdges),
            GuideInfluence = Some(quad.GuideInfluence),
            PreserveEdges = Some(quad.PreserveEdges),
            SymmetryAxis = Some(quad.SymmetryAxis),
            GuideCurveCount = quad.GuideCurves.Count,
            FaceBlockCount = quad.FaceBlocks.Count,
        };
    private static RemeshReceipt ReduceReceiptOf(RemeshKind.SimplifyCase kind, Mesh source, Mesh output) =>
        TopologyOf(kind: kind, source: source, output: output) with {
            DesiredPolygonCount = Some(kind.Parameters.DesiredPolygonCount),
            AllowDistortion = Some(kind.Parameters.AllowDistortion),
            Accuracy = Some(kind.Parameters.Accuracy),
            NormalizeMeshSize = Some(kind.Parameters.NormalizeMeshSize),
            FaceTagCount = kind.Parameters.FaceTags?.Length ?? 0,
            LockedComponentCount = kind.Parameters.LockedComponents?.Length ?? 0,
            ReduceError = Optional(kind.Parameters.Error).Filter(static text => !string.IsNullOrWhiteSpace(value: text)),
        };
    private static RemeshReceipt TopologyOf(RemeshKind kind, Mesh source, Mesh output) =>
        new(Kind: kind, Status: RemeshStatus.Completed, PreVertexCount: source.Vertices.Count, PreFaceCount: source.Faces.Count, PostVertexCount: output.Vertices.Count, PostFaceCount: output.Faces.Count, ReductionRatio: source.Faces.Count == 0 ? 0.0 : (double)output.Faces.Count / source.Faces.Count, Valid: output.IsValid, TopologyChanged: source.Vertices.Count != output.Vertices.Count || source.Faces.Count != output.Faces.Count);

    // --- [DESCRIPTORS] ----------------------------------------------------------------------
    // SpectralFilter owns HKS-like heat, unnormalized WKS-style wave, biharmonic, diffusion, commute-time, and identity descriptors.
    internal static Fin<TOut> DescribeShape<TOut>(MeshSpace space, MeshDescriptor kind, int eigenpairs, Op key) =>
        Optional(kind).ToFin(key.InvalidInput()).Bind(active =>
            guard(active.IsValid, key.InvalidInput()).Bind(_ => active.Switch(
                state: (Space: space, Eigenpairs: eigenpairs, Key: key),
                spectralCase: static ((MeshSpace Space, int Eigenpairs, Op Key) state, MeshDescriptor.SpectralCase spec) =>
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
        AtomProjection.Rows<DescriptorResult, TOut>(self: descriptor, key: key, owner: typeof(MeshDescriptor.SpectralCase),
            new ProjectionRow(typeof(DescriptorReceipt), () => Fin.Succ<object>(descriptor.Receipt)),
            new ProjectionRow(typeof(SpectralDescriptor), () => Fin.Succ<object>(new SpectralDescriptor(Values: descriptor.Values, Receipt: descriptor.Receipt.Spectral))),
            new ProjectionRow(typeof(SpectralDescriptorReceipt), () => Fin.Succ<object>(descriptor.Receipt.Spectral)),
            new ProjectionRow(typeof(Arr<double>), () => Fin.Succ<object>(descriptor.Values)));

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
    private static Fin<TOut> ProjectSegmentation<TOut>(MeshSegmentationResult result, Op key) =>
        AtomProjection.Rows<MeshSegmentationResult, TOut>(self: result, key: key, owner: typeof(MeshSegmentation),
            new ProjectionRow(typeof(MeshSegmentationReceipt), () => Fin.Succ<object>(result.Receipt)),
            new ProjectionRow(typeof(Arr<int>), () => Fin.Succ<object>(result.FaceRegions)));
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
        double degenerateAreaFloor = DegenerateAreaFloorOf(scale: MeanEdgeLengthOf(mesh: mesh));
        for (int f = 0; f < mesh.Faces.Count; f++) {
            MeshFace face = mesh.Faces[index: f];
            Point3d a = mesh.Vertices[index: face.A], b = mesh.Vertices[index: face.B], c = mesh.Vertices[index: face.C];
            double area = 0.5 * Vector3d.CrossProduct(a: b - a, b: c - a).Length;
            if ((face.IsTriangle ? area : area + (0.5 * Vector3d.CrossProduct(a: c - a, b: mesh.Vertices[index: face.D] - a).Length)) < degenerateAreaFloor) { skippedDegenerate++; continue; }
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
        return eigen.IsUsable && pairs.Length >= 2 && pairs[1].Eigenvector.Count == expectedCount && pairs[1].Eigenvector.ForAll(RhinoMath.IsValidDouble)
            ? Fin.Succ(pairs[1].Eigenvector)
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
               });
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
    // The two consumer-sampled per-vertex fields are the potential-gradient split: Irrotational = ∇α
    // (face-averaged cotan-potential gradient), Solenoidal = sampledField − Irrotational. On a genus>0
    // surface that per-vertex Solenoidal therefore carries co-exact + harmonic together; the genuine
    // star1-orthogonal ω = dα + δβ + η split (mean-zero dα, harmonic projection onto the orthonormal
    // basis) is the receipt-only deliverable carried as Decomposition and read by a consumer through
    // Project<HodgeDecompositionReceipt> / Project<HarmonicOneFormReceipt>. The receipt witnesses a split
    // the sampled bundle does not realize — that gap is explicit here, not silent; lifting δβ to per-vertex
    // (Whitney EdgeFormToVertexField) for the sampled bundle is deferred per IMPL-5 scope.
    internal sealed record HodgeBundle(Arr<Vector3d> Irrotational, Arr<Vector3d> Solenoidal, Option<HodgeDecompositionReceipt> Decomposition = default) {
        internal Fin<TOut> Project<TOut>(Op key) {
            HodgeBundle self = this;
            return AtomProjection.Rows<HodgeBundle, TOut>(self: self, key: key,
                new ProjectionRow(typeof(HodgeDecompositionReceipt), () => self.Decomposition.Map(static receipt => (object)receipt).ToFin(key.InvalidResult())),
                new ProjectionRow(typeof(HarmonicOneFormReceipt), () => self.Decomposition.Map(static receipt => (object)receipt.Harmonic).ToFin(key.InvalidResult())));
        }
    }
    internal static Fin<Vector3d> HodgeProjectedAt(VectorField source, MeshSpace space, BoundarySense sense, Point3d sample, Op key) =>
        from bundle in space.Cache.Hodge(probe: new HodgeKey(Source: source),
            compute: () => ComputeHodgeBundle(source: source, space: space, key: key))
        from value in InterpolateVectorOnMesh(space: space, sample: sample, perVertex: sense.Equals(BoundarySense.Toward) ? bundle.Irrotational : bundle.Solenoidal, key: key)
        select value;
    internal static Fin<TOut> HodgeProjected<TOut>(VectorField source, MeshSpace space, Op key) =>
        from bundle in space.Cache.Hodge(probe: new HodgeKey(Source: source),
            compute: () => ComputeHodgeBundle(source: source, space: space, key: key))
        from output in bundle.Project<TOut>(key: key)
        select output;
    private static Fin<HodgeBundle> ComputeHodgeBundle(VectorField source, MeshSpace space, Op key) {
        Mesh mesh = space.Native;
        int nVerts = mesh.Vertices.Count;
        int nEdges = mesh.TopologyEdges.Count;
        double[] negDivergence = new double[nVerts];
        return from topology in TopologyDetailed(space: space)
               from genus in topology.Genus.ToFin(key.Unsupported(geometryType: typeof(MeshSpace), outputType: typeof(HodgeBundle)))
               from decomposition in genus > 0
                   ? SpectralCore.Build(space: space, key: key).Bind(calculus => HodgeDecompositionOf(source: source, space: space, calculus: calculus, key: key).Map(static receipt => Some(receipt)))
                   : Fin.Succ(Option<HodgeDecompositionReceipt>.None)
               from _ in toSeq(Enumerable.Range(start: 0, count: nEdges)).Fold(
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
               from laplacian in space.Laplacian(kind: MeshLaplacian.Cotangent, key: key)
               from potential in laplacian.Stiffness.SingularSolve(rhs: new Arr<double>(negDivergence), gauge: GaugePolicy.PinConstant(index: 0, mass: Some(laplacian.MassLumped), shift: GaugeShift.MeanZero), context: space.Tolerance, key: key)
               from bundle in BuildHodgeFromPotential(mesh: mesh, source: source, potential: potential, decomposition: decomposition, space: space, key: key)
               select bundle;
    }
    // ω is the primal DEC 1-form on the intrinsic edges: the line integral ∫⟨field,dl⟩ over the embedded
    // Lo→Hi segment (field·chord — direction AND magnitude from the one embedded geometry, so a flipped
    // intrinsic edge whose intrinsic length differs from its 3D chord no longer mixes an extrinsic
    // direction with an unrelated intrinsic magnitude). The exact part dα is the
    // mean-zero gradient of the intrinsic cotan-Poisson potential (NEW-COUPLING: MeanZero shift),
    // the harmonic part is the star1-projection of ω−dα onto the orthonormal basis, and the co-exact
    // remainder is ω−dα−η; the three are star1-orthogonal so no indefinite solve runs on the hot path.
    private static Fin<HodgeDecompositionReceipt> HodgeDecompositionOf(VectorField source, MeshSpace space, DiscreteCalculus calculus, Op key) =>
        from imesh in space.Cache.IntrinsicMeshSnapshot(key: key)
        from basis in calculus.Harmonic.ToFin(key.InvalidResult())
        from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
        let edgeCount = calculus.D0.Rows.Value
        from omega in imesh.EdgeCount == edgeCount && calculus.Star1.Count == edgeCount
            ? toSeq(Enumerable.Range(start: 0, count: edgeCount)).TraverseM(e => {
                IntrinsicEdge edge = imesh.EdgeAt(index: e);
                Point3d lo = imesh.Positions[edge.Lo];
                Point3d hi = imesh.Positions[edge.Hi];
                Vector3d chord = hi - lo;
                double length = chord.Length;
                return length <= RhinoMath.ZeroTolerance
                    ? Fin.Succ(0.0)
                    : source.SampleVector(sample: (lo + hi) * 0.5, context: space.Tolerance, key: key).Bind(sampled => key.AcceptValue(value: sampled * chord));
            }).As().Map(static values => new Arr<double>([.. values.AsIterable()]))
            : Fin.Fail<Arr<double>>(key.InvalidResult())
        from receipt in SpectralCore.HodgeDecomposeDetailed(calculus: calculus, basis: basis, stiffness: laplacian.Stiffness, mass: laplacian.MassLumped, omega: omega, context: space.Tolerance, key: key)
        select receipt;
    private static Fin<HodgeBundle> BuildHodgeFromPotential(Mesh mesh, VectorField source, Arr<double> potential, Option<HodgeDecompositionReceipt> decomposition, MeshSpace space, Op key) {
        using Mesh active = mesh.DuplicateMesh();
        if (ContainsQuads(mesh: active) && !active.Faces.ConvertQuadsToTriangles())
            return Fin.Fail<HodgeBundle>(key.InvalidResult());
        int nVerts = active.Vertices.Count;
        Vector3d[] irrotPerVertex = new Vector3d[nVerts];
        double[] faceTally = new double[nVerts];
        for (int f = 0; f < active.Faces.Count; f++) {
            MeshFace face = active.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d pa = active.Vertices[index: face.A]; Point3d pb = active.Vertices[index: face.B]; Point3d pc = active.Vertices[index: face.C];
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
            source.SampleVector(sample: active.Vertices[index: v], context: space.Tolerance, key: key)
                .Map(sampled => sampled - irrotPerVertex[v])).As()
            .Map(solenoid => new HodgeBundle(
                Irrotational: new Arr<Vector3d>(irrotPerVertex),
                Solenoidal: new Arr<Vector3d>([.. solenoid.AsIterable()]),
                Decomposition: decomposition));
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
    // One log-map surface routes three algorithms through the generated Switch (a new TangentLogMapAlgorithm item is a
    // hard compile gate here): VectorHeatApproximate -> heat magnitude plus transported source tangent; ExactStraightestExp
    // -> Polthier straightest-geodesic IVP face-unfolding; ExactWindowPropagation -> the MMP distance plus the BVP backtrace
    // log direction. Func-form Switch keeps the allocating exact arms unevaluated until the algorithm dispatches.
    internal static Fin<TangentLogMapResult> TangentLogMapAt(MeshSpace space, int source, Point3d sample, double time, TangentLogMapAlgorithm algorithm, GeodesicTracePolicy trace, WindowPropagationPolicy windows, Op key) =>
        algorithm.Switch(
            state: (Space: space, Source: source, Sample: sample, Time: time, Trace: trace, Windows: windows, Key: key),
            vectorHeatApproximate: static s => VectorHeatLogMapAt(space: s.Space, source: s.Source, sample: s.Sample, time: s.Time, key: s.Key),
            exactStraightestExp: static s => ExactExpMapAt(space: s.Space, source: s.Source, sample: s.Sample, policy: s.Trace, key: s.Key),
            exactWindowPropagation: static s => ExactLogMapAt(space: s.Space, source: s.Source, sample: s.Sample, policy: s.Windows, key: s.Key));

    // Vector-heat-backed logarithm approximation: heat geodesic magnitude plus transported source tangent direction.
    private static Fin<TangentLogMapResult> VectorHeatLogMapAt(MeshSpace space, int source, Point3d sample, double time, Op key) {
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
               select new TangentLogMapResult(Tangent: tangent, Receipt: new TangentLogMapReceipt(Algorithm: TangentLogMapAlgorithm.VectorHeatApproximate, SourceVertex: source, TargetCount: 1, VectorHeatBacked: true, RejectsFlippedIntrinsic: true, FiniteLogCount: tangent.IsValid && RhinoMath.IsValidDouble(x: tangent.Length) ? 1 : 0, MaxMagnitudeResidual: Some(residual), HeatTime: Some(time), PathFaces: [], CrossedEdges: [], TracedLength: distance, PathRelativeResidual: 0.0, SegmentCount: 0, EdgeCrossingCount: 0, VertexPassCount: 0));
        Fin<Vector3d> TransportedLog(Vector3d value, double scale) {
            Vector3d tangent = value;
            return tangent.IsValid && RhinoMath.IsValidDouble(x: scale) && scale >= 0.0 && tangent.Unitize()
                ? key.AcceptValue(value: scale * tangent)
                : Fin.Fail<Vector3d>(key.InvalidResult());
        }
    }

    // Exact geodesic DISTANCE + DIRECTION via Surazhsky/MMP window propagation over the frozen intrinsic Delaunay half-edge mesh:
    // a pure-scalar GeodesicWindow wavefront advances over a BCL PriorityQueue<.,double> endpoint-min frontier, each popped
    // window unfolds across its edge into the adjacent triangle (edge laid flat, pseudosource at (sx,sy<=0), opposite vertex
    // in front), casts child windows onto the two new edges with the occlusion clamp sy=sqrt(max(0,d0^2-sx^2)), and seeds
    // saddle shadow windows at spherical vertices whose cone angle theta>SaddleAngleThreshold (the classic MMP overestimation
    // fix behind saddles). Vertex distances are MMP-exact; the sample distance interpolates them barycentrically on the closest
    // face. The log DIRECTION is the BVP entry of the one geodesic tracer: BacktraceGeodesicToSource walks the converged field
    // back to the source over the shared chart kernel and inverse-seats the source-outgoing angle through frames.X/Y[source].
    private static Fin<TangentLogMapResult> ExactLogMapAt(MeshSpace space, int source, Point3d sample, WindowPropagationPolicy policy, Op key) {
        int n = space.Native.Vertices.Count;
        return source < 0 || source >= n
            ? Fin.Fail<TangentLogMapResult>(key.InvalidInput())
            : space.Cache.EnsureFrozenIntrinsic(kind: MeshLaplacian.IntrinsicDelaunay, key: key).Bind(imesh => {
                WindowPropagation wave = PropagateWindows(imesh: imesh, source: source, policy: policy);
                WindowField field = wave.Field;
                double[] vertexDistance = wave.VertexDistance;
                return ClosestFace(space: space, sample: sample, key: key, project: (_, face, weights, _) => {
                    if (!face.IsTriangle) return Fin.Fail<TangentLogMapResult>(key.InvalidResult());
                    double distance = (weights[0] * SafeVertexDistance(vertexDistance, face.A)) + (weights[1] * SafeVertexDistance(vertexDistance, face.B)) + (weights[2] * SafeVertexDistance(vertexDistance, face.C));
                    if (!RhinoMath.IsValidDouble(x: distance) || distance < 0.0) return Fin.Fail<TangentLogMapResult>(key.InvalidResult());
                    bool nearSource = distance <= space.Tolerance.Absolute.Value;
                    int intrinsicFace = IntrinsicFaceOfVertices(imesh: imesh, a: face.A, b: face.B, c: face.C);
                    BvpTrace trace = BacktraceGeodesicToSource(imesh: imesh, mesh: space.Native, field: field, targetDistance: distance, source: source, targetFace: intrinsicFace, targetWeights: weights, policy: policy)
                        .IfNone(() => new BvpTrace(WorldLogDir: Vector3d.Zero, TracedLength: 0.0, FieldDistance: distance, PathFaces: [], CrossedEdges: [], EdgeCrossingCount: 0, VertexPassCount: 0, Stop: GeodesicStopKind.IterationCap));
                    // A recovered log direction requires the backtrace to reach the source (LengthReached) with a finite outgoing ray
                    // AND the INDEPENDENT chart geodesic distance (trace.TracedLength = the straight-line distance to the target point
                    // in the source-rooted unfolded chart, a pure-geometry quantity) to match the field-exact distance (trace.FieldDistance,
                    // the converged MMP wavefront's own distance at the target) within the scale-relative band; a cut-locus / multi-saddle
                    // chain (IterationCap), a wrong owning-window pick, a degenerate ray, or the source point itself (log = 0) disagrees the
                    // two and fails the projection. The witness compares two independently-derived distances, never the input echoed back.
                    Vector3d worldLogDir = nearSource ? Vector3d.Zero : trace.WorldLogDir;
                    double witnessDistance = trace.FieldDistance;
                    double pathResidual = nearSource || witnessDistance <= RhinoMath.SqrtEpsilon ? 0.0 : Math.Abs(value: trace.TracedLength - witnessDistance) / Math.Max(val1: witnessDistance, val2: RhinoMath.SqrtEpsilon);
                    bool directionRecovered = trace.Stop == GeodesicStopKind.LengthReached && trace.WorldLogDir.IsValid && trace.WorldLogDir.Length > RhinoMath.ZeroTolerance && pathResidual <= RhinoMath.SqrtEpsilon;
                    int segments = trace.PathFaces.Count;
                    int finiteLog = field.Windows.Count > 0 && RhinoMath.IsValidDouble(x: distance) ? 1 : 0;
                    TangentLogMapReceipt receipt = new(
                        Algorithm: TangentLogMapAlgorithm.ExactWindowPropagation,
                        SourceVertex: source,
                        TargetCount: 1,
                        VectorHeatBacked: false,
                        RejectsFlippedIntrinsic: false,
                        FiniteLogCount: finiteLog,
                        MaxMagnitudeResidual: Option<double>.None,
                        HeatTime: Option<double>.None,
                        PathFaces: new Arr<int>([.. trace.PathFaces]),
                        CrossedEdges: new Arr<int>([.. trace.CrossedEdges]),
                        TracedLength: trace.TracedLength,
                        PathRelativeResidual: pathResidual,
                        SegmentCount: segments,
                        EdgeCrossingCount: trace.EdgeCrossingCount,
                        VertexPassCount: trace.VertexPassCount,
                        StopKind: Some(trace.Stop),
                        WindowCount: field.Windows.Count,
                        OcclusionClampCount: field.OcclusionClampCount,
                        PseudosourceCount: field.PseudosourceCount,
                        CutLocusCount: field.CutLocusCount);
                    return nearSource
                        ? key.AcceptValue(value: Vector3d.Zero).Map(zero => new TangentLogMapResult(Tangent: zero, Receipt: receipt))
                        : receipt.IsValid && directionRecovered
                            ? key.AcceptValue(value: worldLogDir).Map(value => new TangentLogMapResult(Tangent: value, Receipt: receipt))
                            : Fin.Fail<TangentLogMapResult>(key.InvalidResult());
                });
            });
    }
    // Resolve the intrinsic live face whose vertex set is {a,b,c}; after IDT flips the host triangle may not survive as an
    // intrinsic face, in which case the source-incident fallback (AnyLiveFaceAtVertex) keeps the backtrace anchored at a real
    // intrinsic triangle and the residual witness gates the result.
    private static int IntrinsicFaceOfVertices(IntrinsicMesh imesh, int a, int b, int c) {
        foreach (int f in imesh.LiveFaceIndices()) {
            (int x, int y, int z) = imesh.Triangles[index: f]!.Value;
            if ((x == a || x == b || x == c) && (y == a || y == b || y == c) && (z == a || z == b || z == c)) return f;
        }
        return imesh.AnyLiveFaceAtVertex(vertex: a);
    }
    private static double SafeVertexDistance(double[] vertexDistance, int vertex) =>
        vertex >= 0 && vertex < vertexDistance.Length && RhinoMath.IsValidDouble(x: vertexDistance[vertex]) ? vertexDistance[vertex] : 0.0;

    // --- [WINDOW_PROPAGATION] ----------------------------------------------------------------
    // Pure-scalar MMP wavefront state. PendingWindow rides the BCL min-frontier keyed on sigma+min(d0,d1) (the window's
    // nearest reachable distance). The unfold/cast loops are the named statement-kernel exemption; no host state is touched
    // (the intrinsic geometry is already detached into IntrinsicMesh at the FromMesh/Freeze boundary).
    private readonly record struct WindowPropagation(WindowField Field, double[] VertexDistance);
    [StructLayout(LayoutKind.Auto)] private readonly record struct PendingWindow(int Edge, int FromFace, double B0, double B1, double Sx, double Sy, double Sigma, int Pseudosource);
    private static WindowPropagation PropagateWindows(IntrinsicMesh imesh, int source, WindowPropagationPolicy policy) {
        int edgeCount = imesh.EdgeCount;
        int vertexCount = imesh.VertexCount;
        double[] vertexDistance = new double[vertexCount];
        System.Array.Fill(array: vertexDistance, value: double.PositiveInfinity);
        vertexDistance[source] = 0.0;
        int maxPerEdge = policy.MaxWindowsPerEdge.Value;
        double saddleThreshold = policy.SaddleAngleThreshold.Value;
        // Per-edge window lists with overlap resolved by nearest distance. A new window is dropped when an existing window on
        // the same edge already dominates its whole interval (cheaper distance at both endpoints), keeping the list bounded.
        List<GeodesicWindow>[] perEdge = new List<GeodesicWindow>[Math.Max(val1: edgeCount, val2: 0)];
        for (int e = 0; e < perEdge.Length; e++) perEdge[e] = [];
        PriorityQueue<PendingWindow, double> frontier = new();
        int occlusionClamps = 0; int pseudosourceCount = 0;
        // Seed: every face incident to the source casts a window onto its opposite edge, pseudosource = the source vertex,
        // laid flat with the source projected to (sx,sy) from the two endpoint distances; sigma = 0.
        foreach (int f in imesh.LiveFaceIndices()) {
            (int a, int b, int c) = imesh.Triangles[index: f]!.Value;
            if (a != source && b != source && c != source) continue;
            (int vL, int vH) = a == source ? (b, c) : b == source ? (c, a) : (a, b);
            int edgeIndex = imesh.IndexOfEdge(lo: Math.Min(val1: vL, val2: vH), hi: Math.Max(val1: vL, val2: vH));
            if (edgeIndex < 0) continue;
            IntrinsicEdge edge = imesh.EdgeAt(index: edgeIndex);
            double edgeLength = edge.Length;
            if (!(edgeLength > RhinoMath.ZeroTolerance)) continue;
            double dLo = imesh.EdgeLengthOf(i: source, j: edge.Lo);
            double dHi = imesh.EdgeLengthOf(i: source, j: edge.Hi);
            (double sx, double sy) = ProjectPseudosource(b0: 0.0, b1: edgeLength, d0: dLo, d1: dHi);
            vertexDistance[edge.Lo] = Math.Min(val1: vertexDistance[edge.Lo], val2: dLo);
            vertexDistance[edge.Hi] = Math.Min(val1: vertexDistance[edge.Hi], val2: dHi);
            EnqueueWindow(frontier: frontier, perEdge: perEdge, maxPerEdge: maxPerEdge, edgeIndex: edgeIndex, fromFace: f, b0: 0.0, b1: edgeLength, sx: sx, sy: sy, sigma: 0.0, pseudosource: source, dropped: out _);
        }
        // Wavefront: pop nearest window, propagate across its edge into the opposite triangle, cast onto the two far edges
        // with the occlusion clamp, and shed a saddle shadow window when the crossed-into vertex is spherical (cone angle
        // exceeds the threshold). The pop budget is bounded by maxPerEdge*edgeCount so a pathological mesh cannot spin.
        int popBudget = Math.Max(val1: 1, val2: maxPerEdge) * Math.Max(val1: edgeCount, val2: 1) * 4;
        for (int pops = 0; frontier.Count > 0 && pops < popBudget; pops++) {
            PendingWindow win = frontier.Dequeue();
            int across = imesh.FaceAcrossEdge(faceIdx: win.FromFace, i: imesh.EdgeAt(index: win.Edge).Lo, j: imesh.EdgeAt(index: win.Edge).Hi);
            if (across < 0) continue;
            IntrinsicEdge baseEdge = imesh.EdgeAt(index: win.Edge);
            double baseLength = baseEdge.Length;
            int apex = imesh.OppositeVertex(faceIdx: across, i: baseEdge.Lo, j: baseEdge.Hi);
            // Lay the across-face flat sharing the base edge (Lo at origin, Hi at (baseLength,0)); apex in the +y half-plane.
            double lLoApex = imesh.EdgeLengthOf(i: baseEdge.Lo, j: apex);
            double lHiApex = imesh.EdgeLengthOf(i: baseEdge.Hi, j: apex);
            if (!(baseLength > RhinoMath.ZeroTolerance) || !(lLoApex > RhinoMath.ZeroTolerance) || !(lHiApex > RhinoMath.ZeroTolerance)) continue;
            double apexX = ((lLoApex * lLoApex) - (lHiApex * lHiApex) + (baseLength * baseLength)) / (2.0 * baseLength);
            double apexY = Math.Sqrt(d: Math.Max(val1: 0.0, val2: (lLoApex * lLoApex) - (apexX * apexX)));
            // Update apex vertex distance from this window's pseudosource (straight-line in the unfolded chart, when the apex
            // is inside the window's angular shadow); plus the always-valid edge-relaxation through the nearer base endpoint.
            double apexDistanceDirect = win.Sigma + Math.Sqrt(d: ((apexX - win.Sx) * (apexX - win.Sx)) + ((apexY - win.Sy) * (apexY - win.Sy)));
            if (WithinShadow(sx: win.Sx, sy: win.Sy, b0: win.B0, b1: win.B1, px: apexX, py: apexY))
                vertexDistance[apex] = Math.Min(val1: vertexDistance[apex], val2: apexDistanceDirect);
            // Cast child windows onto the two far edges (Lo->apex and Hi->apex). Each child interval is the far edge's full
            // span clipped to the cone from the pseudosource through [b0,b1]; the occlusion clamp keeps the pseudosource on
            // the correct side so distance behind a saddle is never underestimated into an overestimate.
            int eLoApex = imesh.IndexOfEdge(lo: Math.Min(val1: baseEdge.Lo, val2: apex), hi: Math.Max(val1: baseEdge.Lo, val2: apex));
            int eHiApex = imesh.IndexOfEdge(lo: Math.Min(val1: baseEdge.Hi, val2: apex), hi: Math.Max(val1: baseEdge.Hi, val2: apex));
            CastChild(frontier: frontier, perEdge: perEdge, maxPerEdge: maxPerEdge, imesh: imesh, fromFace: across, win: win, edgeIndex: eLoApex, near: baseEdge.Lo, nearX: 0.0, nearY: 0.0, farX: apexX, farY: apexY, clamps: ref occlusionClamps);
            CastChild(frontier: frontier, perEdge: perEdge, maxPerEdge: maxPerEdge, imesh: imesh, fromFace: across, win: win, edgeIndex: eHiApex, near: baseEdge.Hi, nearX: baseLength, nearY: 0.0, farX: apexX, farY: apexY, clamps: ref occlusionClamps);
            // Saddle pseudosource: when the apex is a spherical (cone-angle > threshold) interior vertex now reached, it
            // re-emits the wavefront as a new pseudosource (sigma = its converged distance), shedding shadow windows onto its
            // incident far edges. theta > SaddleAngleThreshold is the structural saddle test.
            if (RhinoMath.IsValidDouble(x: vertexDistance[apex]) && imesh.IsInteriorVertex(vertex: apex) && ConeAngleAt(imesh: imesh, vertex: apex) > saddleThreshold) {
                int seeded = SeedSaddleWindows(frontier: frontier, perEdge: perEdge, maxPerEdge: maxPerEdge, imesh: imesh, saddle: apex, sigma: vertexDistance[apex], vertexDistance: vertexDistance);
                if (seeded > 0) pseudosourceCount++;
            }
        }
        // Vertex-level cleanup: best-effort one-hop relaxation for vertices the wavefront never covered (stranded at +Inf with
        // a finite one-ring neighbor). Relax both endpoints against a SNAPSHOT of the pre-pass distances (Jacobi, symmetric) so
        // the result is order-independent — reading the just-updated Lo into the Hi relaxation (Gauss-Seidel) makes a single
        // edge sweep depend on edge enumeration order. Wavefront-covered vertices already carry their MMP-exact distance and the
        // snapshot Min never raises them; multi-hop strands are not the wavefront's claim and stay scoped to this fallback hop.
        double[] vertexSnapshot = [.. vertexDistance];
        for (int e = 0; e < edgeCount; e++) {
            IntrinsicEdge edge = imesh.EdgeAt(index: e);
            if (!(edge.Length > RhinoMath.ZeroTolerance)) continue;
            vertexDistance[edge.Lo] = Math.Min(val1: vertexDistance[edge.Lo], val2: vertexSnapshot[edge.Hi] + edge.Length);
            vertexDistance[edge.Hi] = Math.Min(val1: vertexDistance[edge.Hi], val2: vertexSnapshot[edge.Lo] + edge.Length);
        }
        for (int v = 0; v < vertexCount; v++) if (!RhinoMath.IsValidDouble(x: vertexDistance[v]) || double.IsInfinity(d: vertexDistance[v])) vertexDistance[v] = 0.0;
        // Cut locus census: a vertex covered by two or more windows whose distances disagree beyond a length-relative band sits
        // on (or near) the cut locus where geodesics from distinct pseudosources meet. Reported only when the policy asks.
        int cutLocus = policy.ReportCutLocus ? CountCutLocus(imesh: imesh, perEdge: perEdge) : 0;
        Seq<GeodesicWindow> windows = toSeq(Enumerable.Range(start: 0, count: perEdge.Length).SelectMany(e => perEdge[e]));
        WindowField field = new(SourceVertex: source, Windows: new Arr<GeodesicWindow>([.. windows]), OcclusionClampCount: occlusionClamps, PseudosourceCount: pseudosourceCount, CutLocusCount: cutLocus);
        return new WindowPropagation(Field: field, VertexDistance: vertexDistance);
    }
    // Project the pseudosource into the edge frame from two endpoint distances; sy is forced non-positive (source behind the
    // edge) so the unfold side convention holds. The occlusion clamp sy=sqrt(max(0,d0^2-sx^2)) prevents an imaginary height
    // (the MMP overestimation bug behind saddles) collapsing to a real but wrong distance.
    private static (double Sx, double Sy) ProjectPseudosource(double b0, double b1, double d0, double d1) {
        double span = b1 - b0;
        double sx = span > RhinoMath.ZeroTolerance ? b0 + (((d0 * d0) - (d1 * d1) + (span * span)) / (2.0 * span)) : b0;
        double syArg = (d0 * d0) - ((sx - b0) * (sx - b0));
        return (sx, -Math.Sqrt(d: Math.Max(val1: 0.0, val2: syArg)));
    }
    private static bool WithinShadow(double sx, double sy, double b0, double b1, double px, double py) {
        // The apex is inside the window's angular cone when the ray pseudosource->apex passes between the two interval edges.
        double cross0 = ((b0 - sx) * (py - sy)) - ((px - sx) * (0.0 - sy));
        double cross1 = ((b1 - sx) * (py - sy)) - ((px - sx) * (0.0 - sy));
        return (cross0 <= RhinoMath.SqrtEpsilon && cross1 >= -RhinoMath.SqrtEpsilon) || (cross0 >= -RhinoMath.SqrtEpsilon && cross1 <= RhinoMath.SqrtEpsilon);
    }
    private static void CastChild(PriorityQueue<PendingWindow, double> frontier, List<GeodesicWindow>[] perEdge, int maxPerEdge, IntrinsicMesh imesh, int fromFace, PendingWindow win, int edgeIndex, int near, double nearX, double nearY, double farX, double farY, ref int clamps) {
        if (edgeIndex < 0 || edgeIndex >= perEdge.Length) return;
        IntrinsicEdge edge = imesh.EdgeAt(index: edgeIndex);
        double childLength = edge.Length;
        if (!(childLength > RhinoMath.ZeroTolerance)) return;
        // Distances from the window pseudosource to the child edge's two endpoints, in the shared unfolded chart.
        double dNear = win.Sigma + Math.Sqrt(d: ((nearX - win.Sx) * (nearX - win.Sx)) + ((nearY - win.Sy) * (nearY - win.Sy)));
        double dFar = win.Sigma + Math.Sqrt(d: ((farX - win.Sx) * (farX - win.Sx)) + ((farY - win.Sy) * (farY - win.Sy)));
        // Order endpoints by the child edge's canonical (Lo,Hi) so b0,b1 align with the stored window convention.
        bool nearIsLo = edge.Lo == near;
        (double d0, double d1) = nearIsLo ? (dNear, dFar) : (dFar, dNear);
        (double sx, double sy) = ProjectPseudosource(b0: 0.0, b1: childLength, d0: d0, d1: d1);
        // Occlusion clamp witness: the syArg underflow inside ProjectPseudosource is the clamp; detect it here so the receipt
        // counts how often the pseudosource grazed (or fell behind) the child edge, the brittle saddle-shadow numeric point.
        double syArg = (d0 * d0) - (sx * sx);
        if (syArg < 0.0) clamps++;
        EnqueueWindow(frontier: frontier, perEdge: perEdge, maxPerEdge: maxPerEdge, edgeIndex: edgeIndex, fromFace: fromFace, b0: 0.0, b1: childLength, sx: sx, sy: sy, sigma: win.Sigma, pseudosource: win.Pseudosource, dropped: out _);
    }
    private static int SeedSaddleWindows(PriorityQueue<PendingWindow, double> frontier, List<GeodesicWindow>[] perEdge, int maxPerEdge, IntrinsicMesh imesh, int saddle, double sigma, double[] vertexDistance) {
        int seeded = 0;
        foreach (int f in imesh.LiveFaceIndices()) {
            (int a, int b, int c) = imesh.Triangles[index: f]!.Value;
            if (a != saddle && b != saddle && c != saddle) continue;
            (int vL, int vH) = a == saddle ? (b, c) : b == saddle ? (c, a) : (a, b);
            int edgeIndex = imesh.IndexOfEdge(lo: Math.Min(val1: vL, val2: vH), hi: Math.Max(val1: vL, val2: vH));
            if (edgeIndex < 0) continue;
            IntrinsicEdge edge = imesh.EdgeAt(index: edgeIndex);
            if (!(edge.Length > RhinoMath.ZeroTolerance)) continue;
            double dLo = sigma + imesh.EdgeLengthOf(i: saddle, j: edge.Lo);
            double dHi = sigma + imesh.EdgeLengthOf(i: saddle, j: edge.Hi);
            (double sx, double sy) = ProjectPseudosource(b0: 0.0, b1: edge.Length, d0: dLo, d1: dHi);
            vertexDistance[edge.Lo] = Math.Min(val1: vertexDistance[edge.Lo], val2: dLo);
            vertexDistance[edge.Hi] = Math.Min(val1: vertexDistance[edge.Hi], val2: dHi);
            EnqueueWindow(frontier: frontier, perEdge: perEdge, maxPerEdge: maxPerEdge, edgeIndex: edgeIndex, fromFace: f, b0: 0.0, b1: edge.Length, sx: sx, sy: sy, sigma: sigma, pseudosource: saddle, dropped: out bool dropped);
            if (!dropped) seeded++;
        }
        return seeded;
    }
    private static void EnqueueWindow(PriorityQueue<PendingWindow, double> frontier, List<GeodesicWindow>[] perEdge, int maxPerEdge, int edgeIndex, int fromFace, double b0, double b1, double sx, double sy, double sigma, int pseudosource, out bool dropped) {
        dropped = true;
        if (edgeIndex < 0 || edgeIndex >= perEdge.Length || !(b1 > b0) || !RhinoMath.IsValidDouble(x: sigma)) return;
        double d0 = Math.Sqrt(d: ((b0 - sx) * (b0 - sx)) + (sy * sy));
        double d1 = Math.Sqrt(d: ((b1 - sx) * (b1 - sx)) + (sy * sy));
        if (!RhinoMath.IsValidDouble(x: d0) || !RhinoMath.IsValidDouble(x: d1)) return;
        List<GeodesicWindow> windows = perEdge[edgeIndex];
        // Drop a child wholly dominated by an existing window (cheaper or equal sigma+endpoint distance at both ends over a
        // covering interval); else admit it. The min-frontier converges, so the first window to cover a point is the cheapest.
        foreach (GeodesicWindow existing in windows)
            if (existing.B0 <= b0 + RhinoMath.SqrtEpsilon && existing.B1 >= b1 - RhinoMath.SqrtEpsilon
                && existing.Sigma + existing.D0 <= sigma + d0 + RhinoMath.SqrtEpsilon && existing.Sigma + existing.D1 <= sigma + d1 + RhinoMath.SqrtEpsilon)
                return;
        if (windows.Count >= maxPerEdge) {
            // Budget reached: evict the farthest existing window if the new one is nearer, else drop the new one.
            int farthest = -1; double worst = sigma + Math.Min(val1: d0, val2: d1);
            for (int i = 0; i < windows.Count; i++) { double near = windows[index: i].Sigma + Math.Min(val1: windows[index: i].D0, val2: windows[index: i].D1); if (near > worst) { worst = near; farthest = i; } }
            if (farthest < 0) return;
            windows.RemoveAt(index: farthest);
        }
        windows.Add(item: new GeodesicWindow(Edge: edgeIndex, B0: b0, B1: b1, D0: d0, D1: d1, Sigma: sigma, Tau: 0.0, Pseudosource: pseudosource));
        frontier.Enqueue(element: new PendingWindow(Edge: edgeIndex, FromFace: fromFace, B0: b0, B1: b1, Sx: sx, Sy: sy, Sigma: sigma, Pseudosource: pseudosource), priority: sigma + Math.Min(val1: d0, val2: d1));
        dropped = false;
    }
    private static double ConeAngleAt(IntrinsicMesh imesh, int vertex) {
        double total = 0.0;
        foreach (int f in imesh.LiveFaceIndices()) {
            (int a, int b, int c) = imesh.Triangles[index: f]!.Value;
            if (a != vertex && b != vertex && c != vertex) continue;
            (int prev, int next) = a == vertex ? (c, b) : b == vertex ? (a, c) : (b, a);
            double lp = imesh.EdgeLengthOf(i: vertex, j: prev); double ln = imesh.EdgeLengthOf(i: vertex, j: next); double lo = imesh.EdgeLengthOf(i: prev, j: next);
            double denom = 2.0 * lp * ln;
            double cos = denom > RhinoMath.ZeroTolerance ? ((lp * lp) + (ln * ln) - (lo * lo)) / denom : 1.0;
            total += Math.Acos(d: Math.Min(val1: 1.0, val2: Math.Max(val1: -1.0, val2: cos)));
        }
        return total;
    }
    private static int CountCutLocus(IntrinsicMesh imesh, List<GeodesicWindow>[] perEdge) {
        int count = 0;
        for (int e = 0; e < perEdge.Length; e++) {
            List<GeodesicWindow> windows = perEdge[e];
            if (windows.Count < 2) continue;
            double band = RhinoMath.SqrtEpsilon * Math.Max(val1: 1.0, val2: imesh.EdgeAt(index: e).Length);
            // Two windows whose pseudosources disagree and whose endpoint distances cross within the same interval mark the cut
            // locus: geodesics from distinct sources meet on this edge. One witness per multiply-covered edge.
            bool crossing = false;
            for (int i = 0; i < windows.Count && !crossing; i++)
                for (int j = i + 1; j < windows.Count && !crossing; j++)
                    if (windows[index: i].Pseudosource != windows[index: j].Pseudosource
                        && Math.Abs(value: windows[index: i].Sigma + windows[index: i].D0 - (windows[index: j].Sigma + windows[index: j].D0)) > band)
                        crossing = true;
            if (crossing) count++;
        }
        return count;
    }

    // --- [BACKTRACE_GEODESIC_BVP] ------------------------------------------------------------
    // BVP mode of the one geodesic tracer: the IVP entry (ExactExpMapAt -> TraceStraightestGeodesic, source + direction) and this
    // BVP entry (target -> source) share the same WalkChart unfold kernel; neither re-derives the unfold arithmetic. The backtrace
    // recovers the boundary conditions from the converged WindowField — the owning window at the target (admitted by OwningWindowAt
    // through the SAME WithinShadow predicate the forward wavefront used for apex admission, so forward and backward provably agree),
    // the field-exact distance at the target (the owning window's barycentric distance interpolation, the converged wavefront's own
    // distance), and the pseudosource chain back to the source (pivoting at each saddle / cut-locus pseudosource where
    // w.Pseudosource != source, monotone-checked so a non-decreasing pivot stops the chain). The source-outgoing chart angle is
    // inverse-seated to world through frames.X/Y[source] and the shared WalkChart replays the ray to the firstWaypoint vertex, so
    // TracedLength is the arc INDEPENDENTLY consumed reaching v_s — witnessed against the field distance via PathRelativeResidual,
    // never the input echoed back. The hop count honors BacktraceMaxHops -> IterationCap so a cut-locus cycle never spins.
    internal readonly record struct BvpTrace(Vector3d WorldLogDir, double TracedLength, double FieldDistance, List<int> PathFaces, List<int> CrossedEdges, int EdgeCrossingCount, int VertexPassCount, GeodesicStopKind Stop);
    internal static Option<BvpTrace> BacktraceGeodesicToSource(IntrinsicMesh imesh, Mesh mesh, WindowField field, double targetDistance, int source, int targetFace, double[] targetWeights, WindowPropagationPolicy policy) {
        if (source < 0 || targetFace < 0 || targetWeights.Length < 3) return None;
        (int a, int b, int c) = imesh.Triangles[index: targetFace]!.Value;
        int[] faceVerts = [a, b, c];
        // Owning-window selection at the target via the forward WithinShadow predicate. Returns the owning pseudosource and the
        // field-exact distance, both derived from the wavefront's own provenance (forward and backward use the SAME shadow predicate).
        Option<(int Pseudosource, double FieldDistance)> entry = OwningWindowAt(imesh: imesh, field: field, faceVerts: faceVerts, weights: targetWeights);
        if (entry.IsNone) return None;
        (int owningPseudosource, double fieldDistance) = entry.IfNone((Pseudosource: source, FieldDistance: targetDistance));
        // fieldDistance is the converged wavefront's own interpolated distance at the target (the witness reference the INDEPENDENT
        // chart geodesic distance is compared against, never a replay length fed back).
        if (!RhinoMath.IsValidDouble(x: fieldDistance) || fieldDistance < 0.0) return None;
        // Walk the saddle chain target->source to confirm reachability. The DIRECT case (owning pseudosource == source: a single
        // straight source-incident leg) is covered by the rigorous independent witness below. A genuine multi-saddle (cut-locus-bent)
        // chain is confirmed monotone-decreasing toward the source; with the default policy (SaddleAngleThreshold = 2pi) seeded saddle
        // pseudosources are admitted only at hyperbolic cone points (theta > 2pi), so a chain can exist. When the chain reaches the
        // source AND its FIRST leg (source -> firstSaddle, the last pivot before source) develops back to a hyperbolic interior cone
        // point, replay only that first leg through the SAME strip-development + seat + WalkChart kernel the direct case uses, reporting
        // the realized first-leg direction. An unconfirmed bend (no reach, no interior cone, or a failed replay) keeps the honest
        // IterationCap terminal partition with the MMP-exact distance recorded, never asserting a path the wavefront never took.
        int maxHops = Math.Max(val1: 1, val2: policy.BacktraceMaxHops.Value);
        bool directLeg = owningPseudosource == source;
        if (!directLeg) {
            int pivot = owningPseudosource;
            int firstSaddle = -1;
            GeodesicStopKind chainStop = GeodesicStopKind.IterationCap;
            for (int hop = 0; hop < maxHops; hop++) {
                if (pivot == source) { chainStop = GeodesicStopKind.LengthReached; break; }
                if (pivot < 0 || pivot >= imesh.VertexCount) return None;
                double pivotReach = SaddleReach(imesh: imesh, field: field, saddle: pivot);
                Option<int> step = PseudosourceTowardSource(imesh: imesh, field: field, saddle: pivot, source: source);
                if (step.IsNone) return None;
                int next = step.IfNone(source);
                double nextReach = next == source ? 0.0 : SaddleReach(imesh: imesh, field: field, saddle: next);
                if (next == source) { firstSaddle = pivot; chainStop = GeodesicStopKind.LengthReached; break; }
                if (nextReach >= pivotReach - RhinoMath.SqrtEpsilon) break;
                pivot = next;
            }
            // First-leg replay: when the chain reached the source through a hyperbolic interior cone firstSaddle, develop source->firstSaddle
            // into one plane, seat the source-outgoing angle, and replay through WalkChart stopping at firstSaddle. The realized first leg's
            // direction is reported; TracedLength is the INDEPENDENT strip-chart distance and FieldDistance the saddle's converged reach.
            if (chainStop == GeodesicStopKind.LengthReached && firstSaddle >= 0 && imesh.IsInteriorVertex(vertex: firstSaddle) && ConeAngleAt(imesh: imesh, vertex: firstSaddle) > RhinoMath.TwoPI
                && StripAngleToVertex(imesh: imesh, field: field, source: source, target: firstSaddle, maxHops: maxHops) is { IsSome: true, Case: ValueTuple<double, double, int> leg }
                && SeatSourceOutgoing(imesh: imesh, mesh: mesh, source: source, seatFace: leg.Item3, chartAngle: leg.Item1) is { IsSome: true, Case: ValueTuple<int, int, int, int, double, Vector3d> legSeat }
                && legSeat.Item1 >= 0 && legSeat.Item6.IsValid) {
                double legChart = leg.Item2;
                ExpTrace legWalk = WalkChart(imesh: imesh, startFace: legSeat.Item1, va: legSeat.Item2, vb: legSeat.Item3, vc: legSeat.Item4, seatAngle: legSeat.Item5, seatedWorldDir: legSeat.Item6, traceLength: legChart, mode: GeodesicWalkMode.Straightest, stopAtVertex: firstSaddle, policy: GeodesicTracePolicy.Default);
                if (legWalk.Stop == GeodesicStopKind.LengthReached && legWalk.ReachedStopVertex)
                    return new BvpTrace(WorldLogDir: legChart * legWalk.SeatedWorldDir, TracedLength: legChart,
                        FieldDistance: SaddleReach(imesh: imesh, field: field, saddle: firstSaddle),
                        PathFaces: legWalk.PathFaces, CrossedEdges: legWalk.CrossedEdges, EdgeCrossingCount: legWalk.EdgeCrossingCount,
                        VertexPassCount: legWalk.VertexPassCount, Stop: GeodesicStopKind.LengthReached);
            }
            // Unconfirmed bend: record the MMP-exact distance and defer the bent-path direction to the honest IterationCap partition.
            return new BvpTrace(WorldLogDir: Vector3d.Zero, TracedLength: chainStop == GeodesicStopKind.LengthReached ? fieldDistance : 0.0, FieldDistance: fieldDistance, PathFaces: [], CrossedEdges: [], EdgeCrossingCount: 0, VertexPassCount: 0, Stop: GeodesicStopKind.IterationCap);
        }
        // Direct geodesic: aim at the ACTUAL barycentric target POINT in the source-rooted chart (not a face corner) so the outgoing
        // tangent is the true source->sample direction. The CHART geodesic distance to that point (sqrt(tx^2+ty^2) in the unfolded
        // layout, a pure geometry quantity) is the INDEPENDENT witness length — compared against fieldDistance (the MMP wavefront's own
        // distance) it catches a wrong owning-window pick that disagrees the two. Inverse-seat to world through the same basis the IVP
        // forward seat inverts, then replay through WalkChart to produce the path evidence (PathFaces / CrossedEdges / segment law).
        Option<(double Angle, double ChartDistance, int RootFace)> target =
            ChartAngleToTargetPoint(imesh: imesh, source: source, targetFace: targetFace, weights: targetWeights) is { IsSome: true, Case: ValueTuple<double, double> direct }
                ? Some((Angle: direct.Item1, ChartDistance: direct.Item2, RootFace: targetFace))
                : StripAngleToTargetPoint(imesh: imesh, field: field, source: source, targetFace: targetFace, targetWeights: targetWeights, maxHops: maxHops);
        if (target.IsNone) return None;
        (double directAngle, double chartDistance, int rootFace) = target.IfNone((Angle: 0.0, ChartDistance: 0.0, RootFace: -1));
        if (rootFace < 0 || !(chartDistance > RhinoMath.ZeroTolerance)) return None;
        Option<(int StartFace, int Va, int Vb, int Vc, double ChartAngle, Vector3d WorldDir)> seat = SeatSourceOutgoing(imesh: imesh, mesh: mesh, source: source, seatFace: rootFace, chartAngle: directAngle);
        if (seat.IsNone) return None;
        (int startFace, int va, int vb, int vc, double chartAngle, Vector3d worldDir) = seat.IfNone((StartFace: -1, Va: -1, Vb: -1, Vc: -1, ChartAngle: 0.0, WorldDir: Vector3d.Zero));
        if (startFace < 0 || !worldDir.IsValid) return None;
        ExpTrace forward = WalkChart(imesh: imesh, startFace: startFace, va: va, vb: vb, vc: vc, seatAngle: chartAngle, seatedWorldDir: worldDir, traceLength: chartDistance, mode: GeodesicWalkMode.Straightest, stopAtVertex: -1, policy: GeodesicTracePolicy.Default);
        // TracedLength is the INDEPENDENT chart geodesic distance to the target point (geometry-derived), witnessed upstream against
        // fieldDistance (MMP-derived). WorldLogDir scales the VALIDATED outgoing direction the replay traced by that length.
        Vector3d worldLogDir = chartDistance * forward.SeatedWorldDir;
        return new BvpTrace(WorldLogDir: worldLogDir, TracedLength: chartDistance, FieldDistance: fieldDistance, PathFaces: forward.PathFaces, CrossedEdges: forward.CrossedEdges, EdgeCrossingCount: forward.EdgeCrossingCount, VertexPassCount: forward.VertexPassCount, Stop: forward.Stop);
    }
    // Select the window owning the target point by the SAME WithinShadow predicate the forward wavefront used: across the face's
    // three edges, project the barycentric point into each candidate window's pseudosource chart (Sx,Sy from the window's endpoint
    // distances) and keep the shadow-covered window of least interpolated distance. Returns the owning pseudosource and the
    // field-exact interpolated distance at the target — the witness reference. WithinShadow makes forward admission and backward
    // selection the SAME predicate, so a target the forward pass covered is never mis-owned or missed by the backward pass.
    private static Option<(int Pseudosource, double FieldDistance)> OwningWindowAt(IntrinsicMesh imesh, WindowField field, int[] faceVerts, double[] weights) {
        double best = double.PositiveInfinity;
        Option<(int Pseudosource, double FieldDistance)> owner = None;
        for (int e = 0; e < 3; e++) {
            int vi = faceVerts[e]; int vj = faceVerts[(e + 1) % 3];
            int edgeIndex = imesh.IndexOfEdge(lo: Math.Min(val1: vi, val2: vj), hi: Math.Max(val1: vi, val2: vj));
            if (edgeIndex < 0) continue;
            IntrinsicEdge edge = imesh.EdgeAt(index: edgeIndex);
            double wi = weights[e]; double wj = weights[(e + 1) % 3];
            double denom = wi + wj;
            double frac = denom > RhinoMath.ZeroTolerance ? (edge.Lo == vi ? wj : wi) / denom : 0.5;
            double bary = Math.Min(val1: 1.0, val2: Math.Max(val1: 0.0, val2: frac)) * edge.Length;
            foreach (GeodesicWindow window in field.Windows) {
                if (window.Edge != edgeIndex) continue;
                // Reconstruct the window's pseudosource projection (Sx,Sy<=0) in the edge frame from its stored endpoint distances
                // (the same ProjectPseudosource geometry the forward cast used), then admit the target by WithinShadow.
                (double sx, double sy) = ProjectPseudosource(b0: window.B0, b1: window.B1, d0: window.D0, d1: window.D1);
                if (!WithinShadow(sx: sx, sy: sy, b0: window.B0, b1: window.B1, px: bary, py: 0.0)) continue;
                double span = Math.Max(val1: window.B1 - window.B0, val2: RhinoMath.ZeroTolerance);
                double here = window.Sigma + ((((window.B1 - bary) * window.D0) + ((bary - window.B0) * window.D1)) / span);
                if (here < best) { best = here; owner = (window.Pseudosource, FieldDistance: here); }
            }
        }
        return owner;
    }
    // The geodesic reach of a saddle pseudosource: the least field distance to the saddle vertex over its incident windows (the
    // converged wavefront distance the saddle re-emitted from). Used as the chain-monotonicity reference and the direct-leg length.
    private static double SaddleReach(IntrinsicMesh imesh, WindowField field, int saddle) {
        double best = double.PositiveInfinity;
        foreach (GeodesicWindow window in field.Windows) {
            IntrinsicEdge edge = imesh.EdgeAt(index: window.Edge);
            double reach = edge.Lo == saddle ? window.Sigma + window.D0 : edge.Hi == saddle ? window.Sigma + window.D1 : double.PositiveInfinity;
            if (reach < best) best = reach;
        }
        return best;
    }
    // From a saddle waypoint, the next pseudosource toward the source is the owning pseudosource of the saddle's cheapest incident
    // window — the wavefront leg that delivered the saddle its converged distance. A self-loop or absent window returns None so the
    // caller emits IterationCap rather than spinning; the chain length itself is read from SaddleReach (the converged field distance).
    private static Option<int> PseudosourceTowardSource(IntrinsicMesh imesh, WindowField field, int saddle, int source) {
        double best = double.PositiveInfinity;
        Option<int> next = None;
        foreach (GeodesicWindow window in field.Windows) {
            if (window.Pseudosource == saddle) continue;
            IntrinsicEdge edge = imesh.EdgeAt(index: window.Edge);
            if (edge.Lo != saddle && edge.Hi != saddle) continue;
            double reach = window.Sigma + Math.Min(val1: window.D0, val2: window.D1);
            if (reach < best) { best = reach; next = window.Pseudosource; }
        }
        return next.IsSome ? next : (saddle == source ? Some(source) : None);
    }
    // In-chart angle AND straight-line distance of the source-outgoing leg toward the barycentric target POINT, valid only when the
    // target face is incident to the source (the direct 1-ring case): lay the source-rooted target face flat (source at origin, a
    // neighbor on +x), place the target point by its barycentric weights, and read its chart angle + radius. The angle is the
    // geometrically exact outgoing tangent of a direct geodesic (aiming at the true sample point, never a face corner) and the radius
    // is the exact chart geodesic distance — the INDEPENDENT witness length. None when the source is not a vertex of the target face.
    private static Option<(double Angle, double ChartDistance)> ChartAngleToTargetPoint(IntrinsicMesh imesh, int source, int targetFace, double[] weights) {
        (int a, int b, int c) = imesh.Triangles[index: targetFace]!.Value;
        int sLocal = a == source ? 0 : b == source ? 1 : c == source ? 2 : -1;
        if (sLocal < 0) return None;
        (int va, int vb, int vc) = sLocal == 0 ? (a, b, c) : sLocal == 1 ? (b, c, a) : (c, a, b);
        (double w0, double w1, double w2) = sLocal == 0 ? (weights[0], weights[1], weights[2]) : sLocal == 1 ? (weights[1], weights[2], weights[0]) : (weights[2], weights[0], weights[1]);
        double[] px = new double[3]; double[] py = new double[3];
        LayoutFace(imesh: imesh, va: va, vb: vb, vc: vc, px: px, py: py);
        double tx = (w0 * px[0]) + (w1 * px[1]) + (w2 * px[2]);
        double ty = (w0 * py[0]) + (w1 * py[1]) + (w2 * py[2]);
        double radius = Math.Sqrt(d: (tx * tx) + (ty * ty));
        return radius > RhinoMath.ZeroTolerance ? Some((Angle: Math.Atan2(y: ty, x: tx), ChartDistance: radius)) : None;
    }
    // Inverse-seat the source-outgoing chart angle (the target-point bearing measured in the source-rooted seatFace layout) to world
    // through the source reference edge / normal basis the forward seat (TraceStraightestGeodesic) inverts — placing the log direction
    // in frames.X/Y[source]'s tangent plane without a fabricated gauge. Returns the seat face vertex order so WalkChart replays from
    // the exact (va,vb,vc) the chart angle was measured in (so the forward and inverse seats are one shared geometry).
    private static Option<(int StartFace, int Va, int Vb, int Vc, double ChartAngle, Vector3d WorldDir)> SeatSourceOutgoing(IntrinsicMesh imesh, Mesh mesh, int source, int seatFace, double chartAngle) {
        if (seatFace < 0) return None;
        (int a0, int b0, int c0) = imesh.Triangles[index: seatFace]!.Value;
        (int va, int vb, int vc) = source == a0 ? (a0, b0, c0) : source == b0 ? (b0, c0, a0) : (c0, a0, b0);
        FrameBundle frames = EnsureVertexFrames(mesh: mesh);
        Vector3d worldEdge = (Vector3d)(mesh.Vertices[index: vb] - mesh.Vertices[index: va]);
        worldEdge -= worldEdge * frames.N[va] * frames.N[va];
        if (!worldEdge.IsValid || !(worldEdge.Length > RhinoMath.ZeroTolerance) || !worldEdge.Unitize()) worldEdge = frames.X[va];
        Vector3d worldPerp = Vector3d.CrossProduct(a: frames.N[va], b: worldEdge);
        Vector3d worldDir = (Math.Cos(d: chartAngle) * worldEdge) + (Math.Sin(a: chartAngle) * worldPerp);
        return worldDir.IsValid && worldDir.Unitize() ? Some((StartFace: seatFace, Va: va, Vb: vb, Vc: vc, ChartAngle: chartAngle, WorldDir: worldDir)) : None;
    }
    // Isometric edge-strip development for a target whose owning window is a seeded saddle pseudosource (the multi-saddle /
    // bent-path case the 1-ring ChartAngleToTargetPoint cannot reach): backtrace face-to-face from the target toward the source
    // image, laying each crossed edge flat in one shared 2D plane (pure Euclidean, no MathNet/CSparse). The strip terminates at the
    // face incident to the source; the source-rooted chord is then read by atan2. The source image inside each strip face is
    // reconstructed by the SAME ProjectPseudosource (sy<=0) geometry the forward cast used and the SAME UnfoldNeighbor interiorSide
    // sign — the mirror-image side-sign is load-bearing, so the developed chord agrees with the forward wavefront's chirality.
    private static Option<(double Tx, double Ty, int RootFace)> DevelopStripToSource(IntrinsicMesh imesh, WindowField field, int source, int targetFace, double targetX, double targetY, int maxHops) {
        int face = targetFace;
        double[] px = new double[3]; double[] py = new double[3];
        (int a, int b, int c) = imesh.Triangles[index: face]!.Value;
        int[] vid = [a, b, c];
        LayoutFace(imesh: imesh, va: a, vb: b, vc: c, px: px, py: py);
        double tx = targetX; double ty = targetY;
        System.Collections.Generic.HashSet<int> seen = [face];
        for (int hop = 0; hop < maxHops; hop++) {
            int sLocal = vid[0] == source ? 0 : vid[1] == source ? 1 : vid[2] == source ? 2 : -1;
            if (sLocal >= 0) {
                double ox = px[sLocal]; double oy = py[sLocal];
                int nLocal = (sLocal + 1) % 3;
                double ex = px[nLocal] - ox; double ey = py[nLocal] - oy;
                double elen = Math.Sqrt(d: (ex * ex) + (ey * ey));
                if (!(elen > RhinoMath.ZeroTolerance)) return None;
                double cx = ex / elen; double cy = ey / elen;
                double rx = tx - ox; double ry = ty - oy;
                return ((rx * cx) + (ry * cy), (-rx * cy) + (ry * cx), face);
            }
            (int exitLocal, _, _) = RayExitOfFace(px: px, py: py, qx: tx, qy: ty, dx: SourceImageDirX(imesh, field, vid, px, py, tx), dy: SourceImageDirY(imesh, field, vid, px, py, ty));
            if (exitLocal < 0) return None;
            int ea = vid[exitLocal]; int eb = vid[(exitLocal + 1) % 3];
            int across = imesh.FaceAcrossEdge(faceIdx: face, i: ea, j: eb);
            if (across < 0 || !seen.Add(item: across)) return None;
            (px, py, vid) = UnfoldNeighbor(imesh: imesh, face: across, ea: ea, eb: eb, sharedAx: px[exitLocal], sharedAy: py[exitLocal], sharedBx: px[(exitLocal + 1) % 3], sharedBy: py[(exitLocal + 1) % 3], interiorX: px[(exitLocal + 2) % 3], interiorY: py[(exitLocal + 2) % 3]);
            face = across;
        }
        return None;
    }
    private static double SourceImageDirX(IntrinsicMesh imesh, WindowField field, int[] vid, double[] px, double[] py, double tx) =>
        StripSourceImage(imesh, field, vid, px, py).Map(s => s.Ix - tx).IfNone(noneValue: 0.0);
    private static double SourceImageDirY(IntrinsicMesh imesh, WindowField field, int[] vid, double[] px, double[] py, double ty) =>
        StripSourceImage(imesh, field, vid, px, py).Map(s => s.Iy - ty).IfNone(noneValue: 0.0);
    // Reconstruct the source image inside one strip face from the cheapest shadow-covering window over its three edges, projecting
    // the pseudosource by the forward ProjectPseudosource (sy<=0) into the laid-out edge frame and offsetting by sy on the interior
    // side (the UnfoldNeighbor sign convention). The image steers the strip's next backtrace hop toward the source.
    private static Option<(double Ix, double Iy)> StripSourceImage(IntrinsicMesh imesh, WindowField field, int[] vid, double[] px, double[] py) {
        double best = double.PositiveInfinity; Option<(double Ix, double Iy)> image = None;
        for (int e = 0; e < 3; e++) {
            int vi = vid[e]; int vj = vid[(e + 1) % 3];
            int edgeIndex = imesh.IndexOfEdge(lo: Math.Min(val1: vi, val2: vj), hi: Math.Max(val1: vi, val2: vj));
            if (edgeIndex < 0) continue;
            IntrinsicEdge edge = imesh.EdgeAt(index: edgeIndex);
            foreach (GeodesicWindow window in field.Windows) {
                if (window.Edge != edgeIndex) continue;
                (double sx, double sy) = ProjectPseudosource(b0: window.B0, b1: window.B1, d0: window.D0, d1: window.D1);
                double reach = window.Sigma + Math.Min(val1: window.D0, val2: window.D1);
                if (reach >= best) continue;
                double ax = px[e]; double ay = py[e]; double bx = px[(e + 1) % 3]; double by = py[(e + 1) % 3];
                bool eIsLo = edge.Lo == vi;
                double frac = eIsLo ? sx / Math.Max(val1: edge.Length, val2: RhinoMath.ZeroTolerance) : 1.0 - (sx / Math.Max(val1: edge.Length, val2: RhinoMath.ZeroTolerance));
                double ux = bx - ax; double uy = by - ay; double ulen = Math.Sqrt(d: (ux * ux) + (uy * uy));
                if (!(ulen > RhinoMath.ZeroTolerance)) continue;
                double tnx = ux / ulen; double tny = uy / ulen; double nx = -tny; double ny = tnx;
                double interiorSide = ((px[(e + 2) % 3] - ax) * nx) + ((py[(e + 2) % 3] - ay) * ny);
                double sign = interiorSide >= 0.0 ? 1.0 : -1.0;
                double along = frac * ulen;
                best = reach; image = (ax + (along * tnx) + (sign * Math.Abs(value: sy) * nx), ay + (along * tny) + (sign * Math.Abs(value: sy) * ny));
            }
        }
        return image;
    }
    // Strip read-off to an interior barycentric target point: develop the strip from the target face to the source, then read the
    // source-rooted chord angle + radius by atan2. The radius IS the chart geodesic distance (the INDEPENDENT witness length), so
    // it inherits the SAME single seat rescale the 1-ring path applies — never doubled. None on a degenerate (zero-radius) chord.
    private static Option<(double Angle, double ChartDistance, int RootFace)> StripAngleToTargetPoint(IntrinsicMesh imesh, WindowField field, int source, int targetFace, double[] targetWeights, int maxHops) {
        (int a, int b, int c) = imesh.Triangles[index: targetFace]!.Value;
        double[] px = new double[3]; double[] py = new double[3];
        LayoutFace(imesh: imesh, va: a, vb: b, vc: c, px: px, py: py);
        double tx = (targetWeights[0] * px[0]) + (targetWeights[1] * px[1]) + (targetWeights[2] * px[2]);
        double ty = (targetWeights[0] * py[0]) + (targetWeights[1] * py[1]) + (targetWeights[2] * py[2]);
        return DevelopStripToSource(imesh: imesh, field: field, source: source, targetFace: targetFace, targetX: tx, targetY: ty, maxHops: maxHops)
            .Bind(dev => Math.Sqrt(d: (dev.Tx * dev.Tx) + (dev.Ty * dev.Ty)) is double r && r > RhinoMath.ZeroTolerance
                ? Some((Angle: Math.Atan2(y: dev.Ty, x: dev.Tx), ChartDistance: r, dev.RootFace)) : None);
    }
    // Strip read-off to a saddle vertex (the first-leg replay target of the multi-saddle arm): develop to the source, then accept
    // only when the developed radius matches the saddle's converged field reach within a scale-relative band (the geometric witness
    // that the strip is the geodesic the wavefront took). The band travels off SaddleReach, not a bare literal.
    private static Option<(double Angle, double ChartDistance, int RootFace)> StripAngleToVertex(IntrinsicMesh imesh, WindowField field, int source, int target, int maxHops) {
        int targetFace = imesh.AnyLiveFaceAtVertex(vertex: target);
        if (targetFace < 0) return None;
        (int a, int b, int c) = imesh.Triangles[index: targetFace]!.Value;
        int tLocal = a == target ? 0 : b == target ? 1 : c == target ? 2 : -1;
        if (tLocal < 0) return None;
        double[] px = new double[3]; double[] py = new double[3];
        LayoutFace(imesh: imesh, va: a, vb: b, vc: c, px: px, py: py);
        double reach = SaddleReach(imesh: imesh, field: field, saddle: target);
        return DevelopStripToSource(imesh: imesh, field: field, source: source, targetFace: targetFace, targetX: px[tLocal], targetY: py[tLocal], maxHops: maxHops)
            .Bind(dev => {
                double radius = Math.Sqrt(d: (dev.Tx * dev.Tx) + (dev.Ty * dev.Ty));
                double band = RhinoMath.SqrtEpsilon * Math.Max(val1: 1.0, val2: reach);
                return radius > RhinoMath.ZeroTolerance && RhinoMath.IsValidDouble(x: reach) && Math.Abs(value: radius - reach) <= band
                    ? Some((Angle: Math.Atan2(y: dev.Ty, x: dev.Tx), ChartDistance: radius, dev.RootFace)) : None;
            });
    }

    // --- [STRAIGHTEST_GEODESIC_EXP] ----------------------------------------------------------
    // Polthier straightest-geodesic IVP: seat the world tangent toward the sample into the source-face intrinsic chart,
    // then unfold face-to-face on intrinsic edge lengths (each face laid out fresh sharing the crossed edge's 2D placement,
    // so the straight ray continues with no transform drift). Grazing-vertex exits snap inside VertexSnap*edgeLength and
    // continue through the spherical vertex by splitting the cone angle theta_l = theta_r = theta/2. Stops on
    // LengthReached / BoundaryHit / BarrierHit / IterationCap. The unfold/ray-exit loops are the named statement kernel;
    // the source-frame chord reads are the platform-forced boundary, copied to detached values at the read site.
    internal static Fin<TangentLogMapResult> ExactExpMapAt(MeshSpace space, int source, Point3d sample, GeodesicTracePolicy policy, Op key) {
        int n = space.Native.Vertices.Count;
        if (source < 0 || source >= n) return Fin.Fail<TangentLogMapResult>(key.InvalidInput());
        FrameBundle frames = EnsureVertexFrames(mesh: space.Native);
        Point3d sourcePoint = space.Native.Vertices[index: source];
        Vector3d worldDir = sample - sourcePoint;
        worldDir -= worldDir * frames.N[source] * frames.N[source];
        double chord = worldDir.Length;
        if (!worldDir.Unitize()) worldDir = frames.X[source];
        return chord <= space.Tolerance.Absolute.Value
            ? key.AcceptValue(value: Vector3d.Zero).Map(zero => new TangentLogMapResult(Tangent: zero, Receipt: new TangentLogMapReceipt(Algorithm: TangentLogMapAlgorithm.ExactStraightestExp, SourceVertex: source, TargetCount: 1, VectorHeatBacked: false, RejectsFlippedIntrinsic: false, FiniteLogCount: 1, MaxMagnitudeResidual: Option<double>.None, HeatTime: Option<double>.None, PathFaces: [], CrossedEdges: [], TracedLength: 0.0, PathRelativeResidual: 0.0, SegmentCount: 0, EdgeCrossingCount: 0, VertexPassCount: 0, StopKind: Some(GeodesicStopKind.LengthReached))))
            : space.Cache.EnsureFrozenIntrinsic(kind: MeshLaplacian.IntrinsicDelaunay, key: key).Bind(imesh => {
                int startFace = imesh.AnyLiveFaceAtVertex(vertex: source);
                if (startFace < 0) return Fin.Fail<TangentLogMapResult>(key.InvalidResult());
                double meanEdge = space.Cache.MeanEdgeLength;
                double traceLength = chord > RhinoMath.ZeroTolerance ? chord : Math.Max(val1: meanEdge, val2: space.Tolerance.Absolute.Value) * policy.TraceLengthFactor.Value;
                ExpTrace result = TraceStraightestGeodesic(imesh: imesh, mesh: space.Native, source: source, startFace: startFace, worldDir: worldDir, traceLength: traceLength, policy: policy);
                Vector3d tangent = result.SeatedWorldDir * result.TracedLength;
                if (!tangent.IsValid || !RhinoMath.IsValidDouble(x: tangent.Length)) return Fin.Fail<TangentLogMapResult>(key.InvalidResult());
                int segments = result.PathFaces.Count;
                // Closing residual: the relative shortfall of the traced geodesic against the requested length. Zero when the
                // straightest-geodesic IVP completed (LengthReached); positive when a boundary/cap/iteration terminal cut it short.
                double residual = traceLength > RhinoMath.SqrtEpsilon ? Math.Abs(value: traceLength - result.TracedLength) / traceLength : 0.0;
                TangentLogMapReceipt receipt = new(
                    Algorithm: TangentLogMapAlgorithm.ExactStraightestExp,
                    SourceVertex: source,
                    TargetCount: 1,
                    VectorHeatBacked: false,
                    RejectsFlippedIntrinsic: false,
                    FiniteLogCount: 1,
                    MaxMagnitudeResidual: Option<double>.None,
                    HeatTime: Option<double>.None,
                    PathFaces: new Arr<int>([.. result.PathFaces]),
                    CrossedEdges: new Arr<int>([.. result.CrossedEdges]),
                    TracedLength: result.TracedLength,
                    PathRelativeResidual: residual,
                    SegmentCount: segments,
                    EdgeCrossingCount: result.EdgeCrossingCount,
                    VertexPassCount: result.VertexPassCount,
                    StopKind: Some(result.Stop));
                return receipt.IsValid
                    ? key.AcceptValue(value: tangent).Map(value => new TangentLogMapResult(Tangent: value, Receipt: receipt))
                    : Fin.Fail<TangentLogMapResult>(key.InvalidResult());
            });
    }
    // The one face-unfolding walk owner. Three entries seat it differently and read different projections off it, but a single
    // loop owns the unfold orchestration (no forked tracer): the IVP exp seats a world tangent, the BVP log replay seats the
    // recovered source-outgoing chart angle and stops at the source vertex, and the overlay edge-trace seats the lo->hi chart
    // bearing and records per-crossing barycentric (CutEdge,U). GeodesicWalkMode discriminates record-crossings/snap-suppression;
    // StopAtVertex (>=0) adds the vertex-hit terminal so the BVP replay produces an INDEPENDENT arc length, not the input length.
    private readonly record struct GeodesicWalkMode(bool RecordCrossings, bool SuppressVertexSnap) {
        internal static readonly GeodesicWalkMode Straightest = new(RecordCrossings: false, SuppressVertexSnap: false);
        internal static readonly GeodesicWalkMode EdgeOverlay = new(RecordCrossings: true, SuppressVertexSnap: true);
    }
    private readonly record struct ExpTrace(Vector3d SeatedWorldDir, double TracedLength, List<int> PathFaces, List<int> CrossedEdges, int EdgeCrossingCount, int VertexPassCount, GeodesicStopKind Stop, List<(int CutEdge, double U)> Crossings, double EndX, double EndY, int ArrivalFace, bool ReachedStopVertex);
    // IVP straightest-geodesic seat: seat the world tangent by the signed angle from the world chord (source -> firstNeighbor)
    // to worldDir about the source normal, then walk via the shared kernel. SeatedWorldDir echoes the validated direction.
    private static ExpTrace TraceStraightestGeodesic(IntrinsicMesh imesh, Mesh mesh, int source, int startFace, Vector3d worldDir, double traceLength, GeodesicTracePolicy policy) {
        (int a0, int b0, int c0) = imesh.Triangles[index: startFace]!.Value;
        (int va, int vb, int vc) = source == a0 ? (a0, b0, c0) : source == b0 ? (b0, c0, a0) : (c0, a0, b0);
        FrameBundle frames = EnsureVertexFrames(mesh: mesh);
        Vector3d worldEdge = (Vector3d)(mesh.Vertices[index: vb] - mesh.Vertices[index: va]);
        worldEdge -= worldEdge * frames.N[va] * frames.N[va];
        double seatAngle = worldEdge.IsValid && worldEdge.Length > RhinoMath.ZeroTolerance && worldEdge.Unitize()
            ? Math.Atan2(y: Vector3d.CrossProduct(a: worldEdge, b: worldDir) * frames.N[va], x: worldEdge * worldDir)
            : 0.0;
        return WalkChart(imesh: imesh, startFace: startFace, va: va, vb: vb, vc: vc, seatAngle: seatAngle, seatedWorldDir: worldDir, traceLength: traceLength, mode: GeodesicWalkMode.Straightest, stopAtVertex: -1, policy: policy);
    }
    // Shared unfold walk: lay the start face flat (va at origin, vb on +x), shoot the ray at seatAngle, and unfold face-to-face on
    // intrinsic lengths. EdgeOverlay records the cut M-edge + barycentric U per crossing and suppresses the vertex snap (raw
    // crossings). StopAtVertex terminates with ReachedStopVertex when the ray reaches that vertex (the BVP source terminal), so
    // TracedLength is the arc actually consumed, never the requested length. The loop is the named statement-kernel exemption.
    private static ExpTrace WalkChart(IntrinsicMesh imesh, int startFace, int va, int vb, int vc, double seatAngle, Vector3d seatedWorldDir, double traceLength, GeodesicWalkMode mode, int stopAtVertex, GeodesicTracePolicy policy) {
        List<int> pathFaces = []; List<int> crossedEdges = []; List<(int CutEdge, double U)> crossings = [];
        int maxSteps = policy.MaxSteps.Value;
        double snapFraction = mode.SuppressVertexSnap ? 0.0 : policy.VertexSnap.Value;
        double[] px = new double[3]; double[] py = new double[3];
        int[] vid = [va, vb, vc];
        LayoutFace(imesh: imesh, va: va, vb: vb, vc: vc, px: px, py: py);
        double qx = px[0], qy = py[0];
        double dx = Math.Cos(d: seatAngle), dy = Math.Sin(a: seatAngle);
        int face = startFace; double traversed = 0.0; GeodesicStopKind stop = GeodesicStopKind.IterationCap; int edgeCrossings = 0; int vertexPasses = 0;
        double endX = qx; double endY = qy; int arrivalFace = startFace; bool reachedStop = false;
        for (int step = 0; step < maxSteps; step++) {
            pathFaces.Add(item: face);
            (int exitLocal, double exitT, double tHit) = RayExitOfFace(px: px, py: py, qx: qx, qy: qy, dx: dx, dy: dy);
            if (exitLocal < 0) { stop = GeodesicStopKind.IterationCap; break; }
            int ea = vid[exitLocal]; int eb = vid[(exitLocal + 1) % 3];
            double remaining = traceLength - traversed;
            // Vertex terminal: when the ray exit grazes the stop vertex (within VertexSnap*edge), the source is reached and the
            // arc consumed so far is the independent length the witness compares against the field distance.
            double exitEdgeLength = imesh.EdgeLengthOf(i: ea, j: eb);
            double vertexFraction = mode.SuppressVertexSnap ? policy.VertexSnap.Value : snapFraction;
            if (stopAtVertex >= 0 && exitEdgeLength > RhinoMath.ZeroTolerance && tHit <= remaining + RhinoMath.SqrtEpsilon
                && ((ea == stopAtVertex && exitT <= vertexFraction) || (eb == stopAtVertex && exitT >= 1.0 - vertexFraction))) {
                traversed += tHit; endX = qx + (tHit * dx); endY = qy + (tHit * dy); arrivalFace = face; reachedStop = true; stop = GeodesicStopKind.LengthReached; break;
            }
            if (tHit >= remaining) { traversed = traceLength; endX = qx + (remaining * dx); endY = qy + (remaining * dy); arrivalFace = face; stop = GeodesicStopKind.LengthReached; break; }
            traversed += tHit;
            double edgeLength = exitEdgeLength;
            // A grazing exit within VertexSnap*edgeLength of an endpoint is a vertex pass, not an edge crossing; snapping it
            // before recording keeps the crossing count and crossed-edge list in lockstep (segment law SegmentCount =
            // EdgeCrossingCount + VertexPassCount + 1). EdgeOverlay suppresses the snap so every transverse cut is recorded raw.
            bool nearStart = exitT <= snapFraction; bool nearEnd = exitT >= 1.0 - snapFraction;
            if ((nearStart || nearEnd) && edgeLength > RhinoMath.ZeroTolerance) {
                int hitVertex = nearStart ? ea : eb;
                vertexPasses++;
                (int nextFace, int nva, int nvb, int nvc, double startAngle) = ContinueThroughVertex(imesh: imesh, face: face, hitVertex: hitVertex, fromVertex: nearStart ? eb : ea);
                if (nextFace < 0) { stop = policy.StopAtBoundary ? GeodesicStopKind.BoundaryHit : GeodesicStopKind.IterationCap; break; }
                face = nextFace; vid = [nva, nvb, nvc];
                LayoutFace(imesh: imesh, va: nva, vb: nvb, vc: nvc, px: px, py: py);
                qx = px[0]; qy = py[0]; dx = Math.Cos(d: startAngle); dy = Math.Sin(a: startAngle);
                continue;
            }
            int edgeIndex = imesh.IndexOfEdge(lo: ea, hi: eb);
            int across = edgeIndex < 0 ? -1 : imesh.FaceAcrossEdge(faceIdx: face, i: ea, j: eb);
            if (across < 0) { stop = policy.StopAtBoundary ? GeodesicStopKind.BoundaryHit : GeodesicStopKind.IterationCap; break; }
            crossedEdges.Add(item: edgeIndex); edgeCrossings++;
            if (mode.RecordCrossings) {
                IntrinsicEdge cut = imesh.EdgeAt(index: edgeIndex);
                double u = cut.Lo == ea ? exitT : 1.0 - exitT;
                crossings.Add(item: (CutEdge: edgeIndex, U: Math.Min(val1: 1.0, val2: Math.Max(val1: 0.0, val2: u))));
            }
            double exX = qx + (tHit * dx); double exY = qy + (tHit * dy);
            (px, py, vid) = UnfoldNeighbor(imesh: imesh, face: across, ea: ea, eb: eb, sharedAx: px[exitLocal], sharedAy: py[exitLocal], sharedBx: px[(exitLocal + 1) % 3], sharedBy: py[(exitLocal + 1) % 3], interiorX: px[(exitLocal + 2) % 3], interiorY: py[(exitLocal + 2) % 3]);
            face = across; qx = exX; qy = exY; endX = exX; endY = exY; arrivalFace = across;
        }
        return new ExpTrace(SeatedWorldDir: seatedWorldDir, TracedLength: traversed, PathFaces: pathFaces, CrossedEdges: crossedEdges, EdgeCrossingCount: edgeCrossings, VertexPassCount: vertexPasses, Stop: stop, Crossings: crossings, EndX: endX, EndY: endY, ArrivalFace: arrivalFace, ReachedStopVertex: reachedStop);
    }
    private static void LayoutFace(IntrinsicMesh imesh, int va, int vb, int vc, double[] px, double[] py) {
        double lab = imesh.EdgeLengthOf(i: va, j: vb);
        double lac = imesh.EdgeLengthOf(i: va, j: vc);
        double lbc = imesh.EdgeLengthOf(i: vb, j: vc);
        px[0] = 0.0; py[0] = 0.0; px[1] = lab; py[1] = 0.0;
        double cx = lab > RhinoMath.ZeroTolerance ? ((lac * lac) - (lbc * lbc) + (lab * lab)) / (2.0 * lab) : 0.0;
        double cyArg = (lac * lac) - (cx * cx);
        px[2] = cx; py[2] = Math.Sqrt(d: Math.Max(val1: 0.0, val2: cyArg));
    }
    private static (int ExitLocal, double ExitT, double THit) RayExitOfFace(double[] px, double[] py, double qx, double qy, double dx, double dy) {
        int bestEdge = -1; double bestT = double.MaxValue; double bestParam = 0.0;
        for (int e = 0; e < 3; e++) {
            int i = e; int j = (e + 1) % 3;
            double ex = px[j] - px[i]; double ey = py[j] - py[i];
            double denom = (dx * ey) - (dy * ex);
            if (Math.Abs(value: denom) < RhinoMath.ZeroTolerance) continue;
            double wx = px[i] - qx; double wy = py[i] - qy;
            double t = ((wx * ey) - (wy * ex)) / denom;
            double u = ((wx * dy) - (wy * dx)) / denom;
            if (t > RhinoMath.SqrtEpsilon && u >= -RhinoMath.SqrtEpsilon && u <= 1.0 + RhinoMath.SqrtEpsilon && t < bestT) {
                bestT = t; bestEdge = e; bestParam = Math.Min(val1: 1.0, val2: Math.Max(val1: 0.0, val2: u));
            }
        }
        return (bestEdge, bestParam, bestT);
    }
    private static (double[] Px, double[] Py, int[] Vid) UnfoldNeighbor(IntrinsicMesh imesh, int face, int ea, int eb, double sharedAx, double sharedAy, double sharedBx, double sharedBy, double interiorX, double interiorY) {
        int opp = imesh.OppositeVertex(faceIdx: face, i: ea, j: eb);
        double lOppA = imesh.EdgeLengthOf(i: opp, j: ea);
        double lOppB = imesh.EdgeLengthOf(i: opp, j: eb);
        double ux = sharedBx - sharedAx; double uy = sharedBy - sharedAy;
        double edge = Math.Sqrt(d: (ux * ux) + (uy * uy));
        double[] px = new double[3]; double[] py = new double[3]; int[] vid = [ea, eb, opp];
        px[0] = sharedAx; py[0] = sharedAy; px[1] = sharedBx; py[1] = sharedBy;
        if (edge <= RhinoMath.ZeroTolerance) { px[2] = sharedAx; py[2] = sharedAy; return (px, py, vid); }
        double tx = ux / edge; double ty = uy / edge; double nx = -ty; double ny = tx;
        double along = ((lOppA * lOppA) - (lOppB * lOppB) + (edge * edge)) / (2.0 * edge);
        double perp = Math.Sqrt(d: Math.Max(val1: 0.0, val2: (lOppA * lOppA) - (along * along)));
        double interiorSide = ((interiorX - sharedAx) * nx) + ((interiorY - sharedAy) * ny);
        double sign = interiorSide >= 0.0 ? -1.0 : 1.0;
        px[2] = sharedAx + (along * tx) + (sign * perp * nx);
        py[2] = sharedAy + (along * ty) + (sign * perp * ny);
        return (px, py, vid);
    }
    private static (int NextFace, int Va, int Vb, int Vc, double StartAngle) ContinueThroughVertex(IntrinsicMesh imesh, int face, int hitVertex, int fromVertex) {
        // Straightest continuation through a spherical vertex: split the total cone angle at the vertex into equal left
        // and right halves (theta_l = theta_r = theta/2). Pick the outgoing face whose half-cone the bisector enters.
        List<(int Face, int Prev, int Next, double Corner)> fan = [];
        foreach (int f in imesh.LiveFaceIndices()) {
            (int a, int b, int c) = imesh.Triangles[index: f]!.Value;
            if (a != hitVertex && b != hitVertex && c != hitVertex) continue;
            (int prev, int next) = a == hitVertex ? (c, b) : b == hitVertex ? (a, c) : (b, a);
            double lp = imesh.EdgeLengthOf(i: hitVertex, j: prev); double ln = imesh.EdgeLengthOf(i: hitVertex, j: next); double lo = imesh.EdgeLengthOf(i: prev, j: next);
            double denom = 2.0 * lp * ln;
            double cos = denom > RhinoMath.ZeroTolerance ? ((lp * lp) + (ln * ln) - (lo * lo)) / denom : 1.0;
            fan.Add(item: (Face: f, Prev: prev, Next: next, Corner: Math.Acos(d: Math.Min(val1: 1.0, val2: Math.Max(val1: -1.0, val2: cos)))));
        }
        if (fan.Count == 0) return (-1, hitVertex, hitVertex, hitVertex, 0.0);
        double total = fan.Sum(static corner => corner.Corner);
        double half = total * 0.5;
        // Walk the fan from the incoming face along its incoming edge (hitVertex, fromVertex), accumulating corners until
        // half the cone is spanned; that face carries the straightest continuation. Anchoring on the incoming face keeps the
        // bisector measured from the arrival edge even when the vertex one-ring repeats a neighbor.
        int anchored = fan.FindIndex(match: corner => corner.Face == face && (corner.Prev == fromVertex || corner.Next == fromVertex));
        int startIndex = anchored >= 0 ? anchored : Math.Max(val1: 0, val2: fan.FindIndex(match: corner => corner.Prev == fromVertex || corner.Next == fromVertex));
        double accum = 0.0;
        for (int s = 0; s < fan.Count; s++) {
            (int f, int prev, int next, double corner) = fan[index: (startIndex + s) % fan.Count];
            if (accum + corner >= half - RhinoMath.SqrtEpsilon) {
                double withinFace = Math.Max(val1: 0.0, val2: half - accum);
                int third = imesh.OppositeVertex(faceIdx: f, i: hitVertex, j: prev);
                return (f, hitVertex, prev, third, withinFace);
            }
            accum += corner;
        }
        (int lf, int lprev, int lnext, _) = fan[index: startIndex];
        return (lf, hitVertex, lprev, lnext, 0.0);
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
            SdfMeshMethod method when method.Equals(SdfMeshMethod.ClosedSurfaceSignedHeat) => ClosedSignedHeatDistanceDetailed(space: space, policy: active, sample: sample, key: key).Bind(result => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Some(result.Solution.Receipt), topology: Some(result.Solution.Topology), volumeGrid: Some(result.Solution.Domain.Receipt)).Map(receipt => new SdfMeshSample(Distance: result.Distance, Receipt: receipt))),
            SdfMeshMethod method when method.Equals(SdfMeshMethod.BoundarySignedHeat) => SignedHeatDistanceDetailed(space: space, policy: active, sample: sample, key: key).Bind(result => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Some(result.Solution.Receipt), topology: Some(result.Solution.Topology)).Map(receipt => new SdfMeshSample(Distance: result.Distance, Receipt: receipt))),
            SdfMeshMethod method when method.Equals(SdfMeshMethod.GeneralizedWindingNumber) => GeneralizedWindingDistance(space: space, sample: sample, key: key).Bind(distance => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Option<SignedHeatReceipt>.None).Map(receipt => new SdfMeshSample(Distance: active.SignConvention.Multiplier * distance, Receipt: receipt))),
            _ => Fin.Fail<SdfMeshSample>(key.InvalidInput()),
        });
    internal static Fin<SdfMeshReceipt> PrewarmSignedDistanceEvaluator(MeshSpace space, SdfMeshPolicy policy, Op key) =>
        AdmitSignedDistanceMeshPolicy(space: space, policy: policy, key: key).Bind(active => active.Method switch {
            SdfMeshMethod method when method.Equals(SdfMeshMethod.ClosedSurfaceSignedHeat) => space.Cache.ClosedSignedHeatDetailed(policy: active, key: key).Bind(solution => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Some(solution.Receipt), topology: Some(solution.Topology), volumeGrid: Some(solution.Domain.Receipt))),
            SdfMeshMethod method when method.Equals(SdfMeshMethod.BoundarySignedHeat) => space.Cache.SignedHeatDetailed(policy: active, key: key).Bind(solution => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Some(solution.Receipt), topology: Some(solution.Topology))),
            SdfMeshMethod method when method.Equals(SdfMeshMethod.GeneralizedWindingNumber) => SdfMeshReceiptOf(space: space, policy: active, signedHeat: Option<SignedHeatReceipt>.None),
            _ => Fin.Fail<SdfMeshReceipt>(key.InvalidInput()),
        });
    private static Fin<SdfMeshReceipt> SdfMeshReceiptOf(MeshSpace space, SdfMeshPolicy policy, Option<SignedHeatReceipt> signedHeat, Option<TopologyReceipt> topology = default, Option<VolumeGridReceipt> volumeGrid = default) =>
        topology.Match(Some: static receipt => Fin.Succ(receipt), None: () => TopologyDetailed(space: space))
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
                   Receipt: new SignedHeatReceipt(BoundarySourceVertexCount: admitted.Source.SourceVertices.Count, BoundaryEncodedEdgeSourceCount: admitted.Source.EncodedEdgeSourceCount, BoundaryRejectedPointCount: admitted.Source.RejectedBoundaryPointCount, BoundaryUnmatchedSegmentCount: admitted.Source.UnmatchedBoundarySegmentCount, HeatSolve: Some(heatSolve), PoissonSolve: poissonSolve, EdgeAssembly: Some(heatFactor.Receipt), SpdMassShift: Some(space.Cache.SpdMassShift)),
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
        from solve in system.Operator.SingularSolveDetailed(rhs: system.Rhs, gauge: GaugePolicy.MeanZeroConstant(dimension: domain.NodeCount, shift: GaugeShift.MinZero), context: space.Tolerance, key: key)
        from __ in solve.Residual <= policy.Solver.ResidualTolerance.Value ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
        from calibrated in CalibrateClosedSignedHeat(domain: domain, raw: solve.Solution, sources: sources, interiorIndex: vectors.InteriorIndex, key: key)
        let receipt = new VolumeGridReceipt(Bounds: domain.Bounds, Resolution: domain.Resolution, XNodes: domain.XNodes, YNodes: domain.YNodes, ZNodes: domain.ZNodes, CellSize: domain.CellSize, Padding: domain.Padding, NodeCount: domain.NodeCount, CellCount: domain.CellCount, SourceTriangleCount: sources.Sources.Length, DegenerateTriangleCount: sources.Degenerate, SourceArea: sources.Area, InsideNodeCount: vectors.Inside, OutsideNodeCount: vectors.Outside, NearSurfaceNodeCount: vectors.NearSurface, RejectedVectorCount: vectors.Rejected, HeatTime: heatTime, GaugeNode: solve.Gauge.Bind(static gauge => gauge.PinnedIndex).IfNone(noneValue: 0), SurfaceShift: calibrated.Shift, Interpolation: policy.Interpolation, BoundaryCondition: policy.BoundaryCondition, Solver: policy.Solver, OperatorNonZeros: system.Operator.NonZeros, FactorNonZeros: solve.FactorNonZeros, Residual: solve.Residual)
        let solvedDomain = domain with { Receipt = receipt }
        select new ClosedSignedHeatSolution(Domain: solvedDomain, Values: calibrated.Values, Receipt: new SignedHeatReceipt(BoundarySourceVertexCount: 0, BoundaryEncodedEdgeSourceCount: 0, BoundaryRejectedPointCount: 0, BoundaryUnmatchedSegmentCount: 0, HeatSolve: Option<SolveReceipt>.None, PoissonSolve: solve), Topology: admitted.Topology);
    private static Fin<(TopologyReceipt Topology, BoundingBox Bounds, double NormalSign)> AdmitClosedSignedHeat(MeshSpace space, SdfMeshPolicy policy, Op key) =>
        from topology in TopologyDetailed(space: space)
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
        double degenerateAreaFloor = DegenerateAreaFloorOf(scale: MeanEdgeLengthOf(mesh: triangulated));
        for (int f = 0; f < triangulated.Faces.Count; f++) {
            MeshFace face = triangulated.Faces[index: f];
            if (!face.IsTriangle) continue;
            Point3d a = triangulated.Vertices[index: face.A], b = triangulated.Vertices[index: face.B], c = triangulated.Vertices[index: face.C];
            Vector3d normal = Vector3d.CrossProduct(a: b - a, b: c - a);
            double area = 0.5 * normal.Length;
            if (!RhinoMath.IsValidDouble(x: area) || area <= degenerateAreaFloor) { degenerate++; continue; }
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
                    rhs[row] = -(Difference(values: vectors.X, x: x, y: y, z: z, axis: 0) + Difference(values: vectors.Y, x: x, y: y, z: z, axis: 1) + Difference(values: vectors.Z, x: x, y: y, z: z, axis: 2));
                    double diag = 0.0;
                    void AddNeighbor(int nx, int ny, int nz) {
                        int col = domain.Index(x: nx, y: ny, z: nz); diag += invH2;
                        triplets.Add(item: (row, col, -invH2));
                    }
                    if (x > 0) AddNeighbor(nx: x - 1, ny: y, nz: z); if (x < domain.XNodes - 1) AddNeighbor(nx: x + 1, ny: y, nz: z);
                    if (y > 0) AddNeighbor(nx: x, ny: y - 1, nz: z); if (y < domain.YNodes - 1) AddNeighbor(nx: x, ny: y + 1, nz: z);
                    if (z > 0) AddNeighbor(nx: x, ny: y, nz: z - 1); if (z < domain.ZNodes - 1) AddNeighbor(nx: x, ny: y, nz: z + 1);
                    triplets.Add(item: (row, row, diag));
                }
        Dimension dim = Dimension.Create(value: domain.NodeCount);
        return SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: triplets, key: key)
            .Map(matrix => new VolumeGridSystem(Operator: matrix, Rhs: new Arr<double>(rhs)));
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
        from topology in TopologyDetailed(space: space)
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

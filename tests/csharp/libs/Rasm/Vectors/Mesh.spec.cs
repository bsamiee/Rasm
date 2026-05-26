using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino;
using Rhino.Geometry;
using Dimension = Rasm.Vectors.Dimension;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class MeshGens {
    public static readonly Op Key = Op.Of(name: "mesh-test");
    public static readonly Dimension Dim2 = Dimension.Create(value: 2);
    public static readonly SparseMatrix Identity2 = Spec.SuccValue(SparseMatrix.FromTriplets(
        rows: Dim2,
        cols: Dim2,
        triplets: [(0, 0, 1.0), (1, 1, 1.0)],
        key: Key), label: "identity sparse");
    public static readonly MeshLaplacian[] Laplacians = [MeshLaplacian.Cotangent, MeshLaplacian.IntrinsicDelaunay, MeshLaplacian.Robust];
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class MeshCatalogLaws {
    [Fact]
    public void LaplacianCatalogKeysAreDistinctAndRobustIsClassified() {
        Spec.SmartEnumKeysUnique(items: MeshGens.Laplacians, key: static kind => kind.Key);
        Assert.Contains(expected: MeshLaplacian.Robust, collection: MeshGens.Laplacians);
    }
}

public sealed class SparseLaplacianLaws {
    [Fact]
    public void SparseLaplacianValidityRequiresAlignedFiniteMassRails() {
        SparseLaplacian valid = new(
            Stiffness: MeshGens.Identity2,
            MassConsistent: MeshGens.Identity2,
            MassLumped: new Arr<double>([1.0, 2.0]),
            SkippedDegenerateFaces: 0);
        SparseLaplacian badMass = valid with { MassLumped = new Arr<double>([1.0]) };
        SparseLaplacian negativeMass = valid with { MassLumped = new Arr<double>([1.0, -1.0]) };
        SparseLaplacian nonFiniteMass = valid with { MassLumped = new Arr<double>([1.0, double.NaN]) };
        SparseLaplacian badShape = valid with {
            MassConsistent = Spec.SuccValue(SparseMatrix.FromTriplets(
                rows: Dimension.Create(value: 3),
                cols: Dimension.Create(value: 3),
                triplets: [(0, 0, 1.0)],
                key: MeshGens.Key), label: "bad shape"),
        };
        SparseLaplacian badColumns = valid with {
            MassConsistent = Spec.SuccValue(SparseMatrix.FromTriplets(
                rows: Dimension.Create(value: 2),
                cols: Dimension.Create(value: 3),
                triplets: [(0, 0, 1.0)],
                key: MeshGens.Key), label: "bad columns"),
        };
        Assert.True(condition: valid.IsValid);
        Assert.False(condition: badMass.IsValid);
        Assert.False(condition: negativeMass.IsValid);
        Assert.False(condition: nonFiniteMass.IsValid);
        Assert.False(condition: badShape.IsValid);
        Assert.False(condition: badColumns.IsValid);
    }
}

public sealed class MeshSegmentationLaws {
    [Fact]
    public void SegmentationFactoriesGateInputsAndPreserveModes() {
        MeshDescriptor descriptor = MeshDescriptor.Spectral(filter: SpectralFilter.Identity);
        _ = Assert.IsType<MeshSegmentation.ScalarThresholdCase>(@object: Spec.SuccValue(MeshSegmentation.ScalarThreshold(values: [0.0, 1.0], threshold: 0.5, includeAbove: true, key: MeshGens.Key), label: "threshold"));
        _ = Assert.IsType<MeshSegmentation.ScalarBandsCase>(@object: Spec.SuccValue(MeshSegmentation.ScalarBands(values: [0.0, 1.0], bandCount: 2, key: MeshGens.Key), label: "bands"));
        _ = Assert.IsType<MeshSegmentation.SeededRegionGrowCase>(@object: Spec.SuccValue(MeshSegmentation.SeededRegionGrow(values: [0.0, 0.1], seedFaces: Seq(0), tolerance: 0.2, maxIterations: 16, key: MeshGens.Key), label: "grow"));
        _ = Assert.IsType<MeshSegmentation.DescriptorClustersCase>(@object: Spec.SuccValue(MeshSegmentation.DescriptorClusters(descriptor: descriptor, eigenpairs: 2, regionCount: 2, maxIterations: 16, tolerance: 1.0e-9, key: MeshGens.Key), label: "clusters"));
        Spec.FailCategory(MeshSegmentation.ScalarThreshold(values: [], threshold: 0.0, key: MeshGens.Key), category: "Input");
        Spec.FailCategory(MeshSegmentation.ScalarThreshold(values: [0.0], threshold: double.NaN, key: MeshGens.Key), category: "Input");
        Spec.FailCategory(MeshSegmentation.ScalarBands(values: [0.0], bandCount: 1, key: MeshGens.Key), category: "Input");
        Spec.FailCategory(MeshSegmentation.SeededRegionGrow(values: [0.0], seedFaces: Seq<int>(), tolerance: 0.2, maxIterations: 16, key: MeshGens.Key), category: "Input");
        Spec.FailCategory(MeshSegmentation.DescriptorClusters(descriptor: descriptor, eigenpairs: 2, regionCount: 1, maxIterations: 16, tolerance: 1.0e-9, key: MeshGens.Key), category: "Input");
    }
}

public sealed class MeshDescriptorAndRemeshLaws {
    [Fact]
    public void DescriptorAndRemeshFactoriesPreserveValidatedPayloads() {
        SpectralDescriptorPolicy policy = new(ScaleNormalization: SpectralScaleNormalization.FirstNonZeroEigenvalue, EnergyNormalization: SpectralEnergyNormalization.UnitL1, ZeroModePolicy: SpectralZeroModePolicy.Drop, CropCount: Some(Dimension.Create(value: 4)));
        MeshDescriptor descriptor = MeshDescriptor.Spectral(filter: SpectralFilter.Identity, sources: Some(Seq(1, 2)), policy: policy);
        MeshDescriptor.SpectralCase spectral = Assert.IsType<MeshDescriptor.SpectralCase>(@object: descriptor);
        Assert.True(condition: descriptor.IsValid);
        Assert.False(condition: MeshDescriptor.Spectral(filter: null!).IsValid);
        Assert.False(condition: MeshDescriptor.Spectral(filter: SpectralFilter.Identity, sources: Option<Seq<int>>.None, policy: default).IsValid);
        Assert.Equal(expected: SpectralFilter.Identity, actual: spectral.Filter);
        Assert.Equal(expected: policy.ScaleNormalization, actual: spectral.Policy.ScaleNormalization);
        Assert.Equal(expected: policy.EnergyNormalization, actual: spectral.Policy.EnergyNormalization);
        Assert.Equal(expected: policy.ZeroModePolicy, actual: spectral.Policy.ZeroModePolicy);
        Spec.Some(policy.CropCount, expected => Spec.Some(spectral.Policy.CropCount, actual => Assert.Equal(expected: expected.Value, actual: actual.Value)));
        Spec.Some(spectral.Sources, sources => {
            Assert.Equal(expected: 2, actual: sources.Count);
            Assert.Equal(expected: 1, actual: sources[index: 0]);
            Assert.Equal(expected: 2, actual: sources[index: 1]);
        });
        Spec.Succ(RemeshKind.Quad(targetLength: 2.5, key: MeshGens.Key), then: remesh =>
            Spec.Equal(left: Assert.IsType<RemeshKind.QuadCase>(@object: remesh).TargetLength.Value, right: 2.5, tolerance: 0.0, what: "quad length"));
        Spec.Succ(RemeshKind.Simplify(parameters: new ReduceMeshParameters { DesiredPolygonCount = 4 }, key: MeshGens.Key), then: remesh =>
            Assert.Equal(expected: 4, actual: Assert.IsType<RemeshKind.SimplifyCase>(@object: remesh).Parameters.DesiredPolygonCount));
        Spec.FailCategory(RemeshKind.Quad(targetLength: 0.0, key: MeshGens.Key), category: "Tolerance");
        Spec.FailCategory(RemeshKind.Simplify(parameters: null!, key: MeshGens.Key), category: "Input");
        Spec.FailCategory(RemeshKind.Simplify(parameters: new ReduceMeshParameters { DesiredPolygonCount = 0 }, key: MeshGens.Key), category: "Input");
    }
    [Fact]
    public void DiscreteCalculusAssemblyReceiptPreservesMatrixAndTopologyFacts() {
        SpectralAssemblyReceipt receipt = new(VertexCount: 4, EdgeCount: 6, FaceCount: 4, AdmittedFaceCount: 4, SkippedDegenerateFaces: 0, SkippedMissingEdges: 0, SkippedInvalidNormals: 0, SkippedInvalidTangents: 0, FlippedIntrinsicRejected: false, MatrixRows: 10, MatrixCols: 10, NonZeros: 36, PositiveStar0Count: 4, PositiveStar1Count: 6, PositiveStar2Count: 4, BoundaryCompositionResidual: 0.0, Genus: Some(0), HarmonicDimension: 0, BoundaryEdgeCount: 0, BoundaryComponentCount: 0, NonManifoldEdgeCount: 0, EulerCharacteristic: 2, TopologyEulerValidated: true, Kind: SpectralAssemblyKind.Dec);
        SpectralAssemblyReceipt genusPositive = receipt with { Genus = Some(2), HarmonicDimension = 4 };
        Assert.True(condition: receipt.IsValid);
        Assert.True(condition: genusPositive.IsValid);
        Assert.Equal(expected: 4, actual: genusPositive.HarmonicDimension);
        Assert.Equal(expected: 2, actual: receipt.EulerCharacteristic);
        Assert.True(condition: receipt.TopologyEulerValidated);
        Assert.False(condition: (genusPositive with { HarmonicDimension = 3 }).IsValid);
        Assert.False(condition: (receipt with { AdmittedFaceCount = 5 }).IsValid);
    }
    [Fact]
    public void EdgeConnectionAssemblyReceiptUsesEdgeSpaceCounts() {
        SpectralAssemblyReceipt receipt = new(VertexCount: 0, EdgeCount: 6, FaceCount: 4, AdmittedFaceCount: 4, SkippedDegenerateFaces: 0, SkippedMissingEdges: 0, SkippedInvalidNormals: 0, SkippedInvalidTangents: 0, FlippedIntrinsicRejected: false, MatrixRows: 12, MatrixCols: 12, NonZeros: 48, PositiveStar0Count: 0, PositiveStar1Count: 0, PositiveStar2Count: 4, BoundaryCompositionResidual: 0.0, Genus: Option<int>.None, HarmonicDimension: 0, ComponentCount: 2, PositiveMassCount: 6, SymmetryResidual: 0.0, FactorNonZeros: Some(32), Kind: SpectralAssemblyKind.EdgeConnection);
        Assert.True(condition: receipt.IsValid);
        Assert.False(condition: (receipt with { ComponentCount = 3 }).IsValid);
        Assert.False(condition: (receipt with { MatrixRows = 6 }).IsValid);
        Assert.False(condition: (receipt with { PositiveMassCount = 7 }).IsValid);
    }
}

public sealed class MeshTopologyAndSdfLaws {
    [Fact]
    public void ReceiptsExposeWatertightAndClosedSignedHeatDomainFacts() {
        TopologyReceipt closed = new(Vertices: 8, TopologyVertices: 8, TopologyEdges: 12, Faces: 6, Triangles: 0, Quads: 6, Ngons: 0, VisiblePolygons: 6, BoundaryComponents: 0, NonManifoldEdges: 0, HasBoundary: false, IsClosed: true, IsSolid: true, IsWatertight: true, IsManifold: true, IsOriented: true, EulerCharacteristic: 2, Genus: Some(0), EulerValidated: true);
        TopologyReceipt open = closed with { BoundaryComponents = 1, HasBoundary = true, IsClosed = false, IsSolid = false, IsWatertight = false };
        Assert.True(condition: closed.IsWatertight);
        Assert.False(condition: open.IsWatertight);
        Assert.Equal(expected: SdfMeshStatus.ClosedSurfaceSignedHeat, actual: SdfMeshMethod.ClosedSurfaceSignedHeat.Status);
        Assert.Equal(expected: SdfMeshDomain.VolumeGrid, actual: SdfMeshMethod.ClosedSurfaceSignedHeat.Domain);
        Assert.NotEqual(expected: SdfMeshDomain.BoundarySource, actual: SdfMeshMethod.ClosedSurfaceSignedHeat.Domain);
        Assert.NotEqual(expected: SdfMeshDomain.SurfaceMesh, actual: SdfMeshMethod.ClosedSurfaceSignedHeat.Domain);
        Assert.Equal(expected: SdfMeshStatus.BoundarySourceSignedHeat, actual: SdfMeshMethod.BoundarySignedHeat.Status);
        Assert.Equal(expected: SdfMeshDomain.SurfaceMesh, actual: SdfMeshMethod.GeneralizedWindingNumber.Domain);
        Spec.SmartEnumKeysUnique(items: [SdfMeshStatus.ApproximateSignClosestDistance, SdfMeshStatus.BoundarySourceSignedHeat, SdfMeshStatus.ClosedSurfaceSignedHeat], key: static status => status.Key);
        Spec.SmartEnumKeysUnique(items: [SdfMeshDomain.SurfaceMesh, SdfMeshDomain.BoundarySource, SdfMeshDomain.VolumeGrid, SdfMeshDomain.VolumeTet], key: static domain => domain.Key);
    }
    [Fact]
    public void ClosedSignedHeatPolicyAndReceiptsExposeVolumeGridWithoutFallback() {
        VolumeGridPolicy grid = Spec.SuccValue(VolumeGridPolicy.ByResolution(resolution: 4, padding: 1.0, key: MeshGens.Key), label: "grid policy");
        SignedHeatTime heat = Spec.SuccValue(SignedHeatTime.Scaled(coefficient: 1.0, key: MeshGens.Key), label: "heat");
        VolumeSolverPolicy solver = Spec.SuccValue(VolumeSolverPolicy.SparseCholesky(key: MeshGens.Key), label: "solver");
        SdfMeshPolicy policy = Spec.SuccValue(SdfMeshPolicy.ClosedSignedHeat(grid: grid, heat: heat, solver: solver, key: MeshGens.Key), label: "closed policy");
        TopologyReceipt topology = new(Vertices: 8, TopologyVertices: 8, TopologyEdges: 12, Faces: 6, Triangles: 12, Quads: 0, Ngons: 0, VisiblePolygons: 12, BoundaryComponents: 0, NonManifoldEdges: 0, HasBoundary: false, IsClosed: true, IsSolid: true, IsWatertight: true, IsManifold: true, IsOriented: true, EulerCharacteristic: 2, Genus: Some(0), EulerValidated: true);
        SolveReceipt poisson = new(
            Solution: new Arr<double>([0.0, 1.0]),
            Path: SolvePath.SparseCholesky,
            Stop: SolveStop.DirectSolved,
            Rows: Dimension.Create(value: 2),
            Cols: Dimension.Create(value: 2),
            RhsLength: 2,
            Iterations: Option<int>.None,
            MaxIterations: Option<int>.None,
            Tolerance: Some(solver.ResidualTolerance.Value),
            Residual: 0.0,
            FullRank: Option<bool>.None,
            InputNonZeros: Some(2),
            FactorNonZeros: Some(2));
        VolumeGridReceipt volumeReceipt = new(
            Bounds: new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0)),
            Resolution: grid.Resolution.Map(static value => value.Value).IfNone(0),
            XNodes: 5,
            YNodes: 5,
            ZNodes: 5,
            CellSize: 0.25,
            Padding: grid.Padding.Value,
            NodeCount: 125,
            CellCount: 64,
            SourceTriangleCount: 12,
            DegenerateTriangleCount: 0,
            SourceArea: 24.0,
            InsideNodeCount: 27,
            OutsideNodeCount: 98,
            NearSurfaceNodeCount: 54,
            RejectedVectorCount: 0,
            HeatTime: heat.Coefficient.Value * 0.25 * 0.25,
            GaugeNode: 0,
            SurfaceShift: 0.1,
            Interpolation: VolumeInterpolation.Trilinear,
            BoundaryCondition: VolumeBoundaryCondition.NeumannGaugePinned,
            Solver: solver,
            OperatorNonZeros: 725,
            FactorNonZeros: Some(512),
            Residual: 0.0);
        SdfMeshReceipt receipt = new(Method: policy.Method, Status: policy.Method.Status, Domain: policy.Method.Domain, Topology: topology, SignedHeat: Some(new SignedHeatReceipt(BoundarySourceVertexCount: 0, BoundaryEncodedEdgeSourceCount: 0, BoundaryRejectedPointCount: 0, BoundaryUnmatchedSegmentCount: 0, HeatSolve: Option<SolveReceipt>.None, PoissonSolve: poisson)), VolumeGrid: Some(volumeReceipt));
        Assert.Equal(expected: SdfMeshDomain.VolumeGrid, actual: receipt.Domain);
        Assert.Equal(expected: SdfMeshStatus.ClosedSurfaceSignedHeat, actual: receipt.Status);
        Spec.Some(receipt.VolumeGrid, volume => {
            Assert.True(condition: volume.NodeCount > 0 && volume.CellCount > 0 && volume.SourceTriangleCount == 12);
            Assert.True(condition: volume.OperatorNonZeros > 0 && volume.FactorNonZeros.IsSome);
            Assert.True(condition: RhinoMath.IsValidDouble(x: volume.Residual));
        });
        Spec.Some(receipt.SignedHeat, signed => {
            Assert.Equal(expected: 0, actual: signed.BoundaryEncodedEdgeSourceCount);
            Assert.True(condition: signed.HeatSolve.IsNone);
            Assert.True(condition: signed.PoissonSolve.FactorNonZeros.IsSome);
        });
    }
    [Fact]
    public void SdfMeshPolicyAdmissionRejectsDefaultAndConstructorBypass() {
        Spec.Fail(default(SdfMeshPolicy).Admit(key: MeshGens.Key));
        VolumeGridPolicy grid = Spec.SuccValue(VolumeGridPolicy.ByResolution(key: MeshGens.Key), label: "grid");
        VolumeGridPolicy sized = Spec.SuccValue(VolumeGridPolicy.ByCellSize(cellSize: 0.25, key: MeshGens.Key), label: "sized grid");
        SdfMeshPolicy closed = Spec.SuccValue(SdfMeshPolicy.ClosedSignedHeat(grid: grid, key: MeshGens.Key), label: "closed");
        SdfMeshPolicy noGridClosed = Spec.SuccValue(SdfMeshPolicy.GeneralizedWinding(key: MeshGens.Key), label: "winding") with { Method = SdfMeshMethod.ClosedSurfaceSignedHeat };
        SdfMeshPolicy gridOnSurface = closed with { Method = SdfMeshMethod.GeneralizedWindingNumber };
        SdfMeshPolicy bothGridScales = closed with { Grid = Some(grid with { CellSize = sized.CellSize }) };
        SdfMeshPolicy noGridScale = closed with { Grid = Some(grid with { Resolution = Option<Dimension>.None }) };
        SdfMeshPolicy noSign = closed with { SignConvention = null! };
        SdfMeshPolicy noSolver = closed with { Solver = default };
        Seq<SdfMeshPolicy> invalid = Seq(noGridClosed, gridOnSurface, bothGridScales, noGridScale, noSign, noSolver);
        _ = invalid.Iter(policy => Spec.Fail(policy.Admit(key: MeshGens.Key)));
    }
}

public sealed class MeshScalarPolicyLaws {
    [Fact]
    public void ScalarContourPolicyGatesFinitePayloadAndLevels() {
        ContourPolicy policy = Spec.SuccValue(ContourPolicy.MeshScalar(values: new Arr<double>([-1.0, 1.0, 1.0]), levels: Seq(0.0), key: MeshGens.Key), label: "scalar contour policy");
        ContourPolicy.MeshScalarCase scalar = Assert.IsType<ContourPolicy.MeshScalarCase>(@object: policy);
        Assert.Equal(expected: 3, actual: scalar.Values.Count);
        _ = Assert.Single(collection: scalar.Levels);
        Spec.FailCategory(ContourPolicy.MeshScalar(values: new Arr<double>([double.NaN]), levels: Seq(0.0), key: MeshGens.Key), category: "Input");
        Spec.FailCategory(ContourPolicy.MeshScalar(values: new Arr<double>([0.0]), levels: Seq<double>(), key: MeshGens.Key), category: "Input");
    }
}

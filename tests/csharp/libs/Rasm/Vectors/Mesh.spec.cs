using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
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
        MeshDescriptor descriptor = MeshDescriptor.Spectral(filter: SpectralFilter.Identity, sources: Some(Seq(1, 2)));
        MeshDescriptor.SpectralCase spectral = Assert.IsType<MeshDescriptor.SpectralCase>(@object: descriptor);
        Assert.True(condition: descriptor.IsValid);
        Assert.False(condition: MeshDescriptor.Spectral(filter: null!).IsValid);
        Assert.Equal(expected: SpectralFilter.Identity, actual: spectral.Filter);
        Spec.Some(spectral.Sources, sources => {
            Assert.Equal(expected: 2, actual: sources.Count);
            Assert.Equal(expected: 1, actual: sources[index: 0]);
            Assert.Equal(expected: 2, actual: sources[index: 1]);
        });
        Spec.Succ(RemeshKind.Quad(targetLength: 2.5, key: MeshGens.Key), then: remesh =>
            Spec.EqualWithin(left: Assert.IsType<RemeshKind.QuadCase>(@object: remesh).TargetLength.Value, right: 2.5, tolerance: 0.0, what: "quad length"));
        Spec.Succ(RemeshKind.Simplify(parameters: new ReduceMeshParameters { DesiredPolygonCount = 4 }, key: MeshGens.Key), then: remesh =>
            Assert.Equal(expected: 4, actual: Assert.IsType<RemeshKind.SimplifyCase>(@object: remesh).Parameters.DesiredPolygonCount));
        Spec.FailCategory(RemeshKind.Quad(targetLength: 0.0, key: MeshGens.Key), category: "Tolerance");
        Spec.FailCategory(RemeshKind.Simplify(parameters: null!, key: MeshGens.Key), category: "Input");
        Spec.FailCategory(RemeshKind.Simplify(parameters: new ReduceMeshParameters { DesiredPolygonCount = 0 }, key: MeshGens.Key), category: "Input");
    }
}

public sealed class MeshTopologyAndSdfLaws {
    [Fact]
    public void ReceiptsExposeWatertightAndClosedSignedHeatDomainFacts() {
        TopologyReceipt closed = new(Vertices: 8, TopologyVertices: 8, TopologyEdges: 12, Faces: 6, Triangles: 0, Quads: 6, Ngons: 0, VisiblePolygons: 6, BoundaryComponents: 0, NonManifoldEdges: 0, HasBoundary: false, IsClosed: true, IsSolid: true, IsWatertight: true, IsManifold: true, IsOriented: true, EulerCharacteristic: 2, Genus: Some(0), EulerValidated: true);
        TopologyReceipt open = closed with { BoundaryComponents = 1, HasBoundary = true, IsClosed = false, IsSolid = false, IsWatertight = false };
        Assert.True(condition: closed.IsWatertight);
        Assert.False(condition: open.IsWatertight);
        Assert.Equal(expected: SdfMeshStatus.ClosedSurfaceSignedHeatUnsupported, actual: SdfMeshMethod.ClosedSurfaceSignedHeat.Status);
        Assert.Equal(expected: SdfMeshDomain.VolumeTet, actual: SdfMeshMethod.ClosedSurfaceSignedHeat.Domain);
        Assert.NotEqual(expected: SdfMeshDomain.BoundarySource, actual: SdfMeshMethod.ClosedSurfaceSignedHeat.Domain);
        Assert.NotEqual(expected: SdfMeshDomain.SurfaceMesh, actual: SdfMeshMethod.ClosedSurfaceSignedHeat.Domain);
        Assert.Equal(expected: SdfMeshStatus.BoundarySourceSignedHeat, actual: SdfMeshMethod.BoundarySignedHeat.Status);
        Assert.Equal(expected: SdfMeshDomain.SurfaceMesh, actual: SdfMeshMethod.GeneralizedWindingNumber.Domain);
        Spec.SmartEnumKeysUnique(items: [SdfMeshStatus.ApproximateSignClosestDistance, SdfMeshStatus.BoundarySourceSignedHeat, SdfMeshStatus.ClosedSurfaceSignedHeatUnsupported], key: static status => status.Key);
        Spec.SmartEnumKeysUnique(items: [SdfMeshDomain.SurfaceMesh, SdfMeshDomain.BoundarySource, SdfMeshDomain.VolumeGrid, SdfMeshDomain.VolumeTet], key: static domain => domain.Key);
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

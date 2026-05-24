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
            MassLumped: new Arr<double>([1.0, 2.0]));
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
        Assert.True(condition: valid.IsValid);
        Assert.False(condition: badMass.IsValid);
        Assert.False(condition: negativeMass.IsValid);
        Assert.False(condition: nonFiniteMass.IsValid);
        Assert.False(condition: badShape.IsValid);
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

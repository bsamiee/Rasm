using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [MODELS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native topology scalars; static owns Topologies/TopologyScalar catalogs, Operation dispatch, Euler V-E+F over MeshFixture.
internal static class TopologyGens {
    public static readonly Op Key = Op.Of(name: "topology-test");
    public static readonly Gen<Topologies> ScalarAspects = Gen.OneOfConst([.. TopologyScalar.Items.Select(static Topologies (s) => new Topologies.ScalarCase(Scalar: s))]);
    // chi=1 (open sheet) vs the testkit closed fixtures' chi=2 is the open-vs-closed discriminant the oracle catches.
    public static readonly Gens.MeshFixture OpenQuadSheet = new(
        Vertices: Seq(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(1, 1, 0), new Point3d(0, 1, 0)),
        Triangles: Seq((0, 1, 2), (0, 2, 3)));
    public static int DistinctUndirectedEdgeCount(Gens.MeshFixture fixture) =>
        fixture.Triangles
            .Bind(static t => Seq((Math.Min(t.A, t.B), Math.Max(t.A, t.B)), (Math.Min(t.B, t.C), Math.Max(t.B, t.C)), (Math.Min(t.A, t.C), Math.Max(t.A, t.C))))
            .Distinct().Count;
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class TopologyScalarCatalogLaws {
    [Fact]
    public void KeysAreContiguousAndOutputTypesArePinned() =>
        Spec.SmartEnumOutputCatalog(items: TopologyScalar.Items, expectedKeys: [0, 1, 2, 3, 4, 5, 6, 7], key: static s => s.Key,
            output: static s => s.Output, expectedOutput: static s => s.Key == 0 ? typeof(bool) : typeof(int));
    [Fact]
    public void ManifoldAloneProjectsBoolEveryOtherScalarProjectsInt() =>
        Assert.Multiple(
            () => Assert.Equal(expected: 1, actual: TopologyScalar.Items.Count(static s => s.Output == typeof(bool))),
            () => Assert.Equal(expected: 7, actual: TopologyScalar.Items.Count(static s => s.Output == typeof(int))));
    [Fact]
    public void UnsupportedFaultRailCarriesStableCategoryAndTypePair() {
        Error fault = TopologyGens.Key.Unsupported(geometryType: typeof(Mesh), outputType: typeof(Plane));
        Assert.Equal(expected: "Unsupported", actual: fault.Category());
        Fault.Unsupported unsupported = Assert.IsType<Fault.Unsupported>(@object: fault);
        Assert.Equal(expected: typeof(Mesh), actual: unsupported.GeometryType);
        Assert.Equal(expected: typeof(Plane), actual: unsupported.OutputType);
    }
}

public sealed class TopologiesUnionCatalogLaws {
    public static readonly (string Label, Topologies Aspect)[] Cases =
        [("Kind", Topologies.Kind), ("Domains", Topologies.Domains), ("SolidOrientation", Topologies.SolidOrientation),
         ("Components", Topologies.Components), ("ContainsPoint", Topologies.ContainsPoint(point: Point3d.Origin)),
         ("Manifold", Topologies.Manifold), ("Euler", Topologies.Euler), ("BoundaryLoops", Topologies.BoundaryLoops),
         ("Genus", Topologies.Genus), ("HoleCount", Topologies.HoleCount), ("FaceCount", Topologies.FaceCount),
         ("EdgeCount", Topologies.EdgeCount), ("VertexCount", Topologies.VertexCount)];
    [Fact]
    public void ScalarFactoriesProjectDistinctScalarCasesAndContainsPointTransportsPayload() {
        Spec.Cases(items: TopologyScalar.Items, key: static s => s.Key, law: static s =>
            Assert.Same(expected: s, actual: Assert.IsType<Topologies.ScalarCase>(@object: new Topologies.ScalarCase(Scalar: s)).Scalar));
        Point3d payload = new(x: 3.0, y: -7.0, z: 11.0);
        Assert.Equal(expected: payload, actual: Assert.IsType<Topologies.ContainsPointCase>(@object: Topologies.ContainsPoint(point: payload)).Point);
        Assert.Equal(expected: 6, actual: Cases.Select(static c => c.Aspect.GetType()).Distinct().Count());
        Assert.Equal(expected: 8, actual: Cases.Count(static c => c.Aspect is Topologies.ScalarCase));
    }
}

public sealed class TopologyScalarDispatchLaws {
    [Fact]
    public void MatchedOutputTypeBuildsSupportedOperationForTopologyGeometry() =>
        Spec.Cases(items: TopologyScalar.Items.Where(static s => s.Output == typeof(int)).ToArray(), key: static s => s.Key, law: static s => {
            Assert.True(condition: new Topologies.ScalarCase(Scalar: s).Operation<Mesh, int>().IsSupported);
            Assert.True(condition: new Topologies.ScalarCase(Scalar: s).Operation<Brep, int>().IsSupported);
        });
    [Fact]
    public void ManifoldScalarRejectsIntAndCountScalarsRejectBool() {
        Assert.False(condition: Topologies.Manifold.Operation<Mesh, int>().IsSupported);
        Assert.True(condition: Topologies.Manifold.Operation<Mesh, bool>().IsSupported);
        Spec.Cases(items: TopologyScalar.Items.Where(static s => s.Output == typeof(int)).ToArray(), key: static s => s.Key, law: static s =>
            Assert.False(condition: new Topologies.ScalarCase(Scalar: s).Operation<Mesh, bool>().IsSupported));
    }
    [Fact]
    public void NonTopologyGeometryOrForeignOutputCollapsesToUnsupported() =>
        Spec.ForAll(TopologyGens.ScalarAspects, static aspect => {
            Assert.False(condition: aspect.Operation<Point3d, int>().IsSupported);
            Assert.False(condition: aspect.Operation<Curve, int>().IsSupported);
            Assert.False(condition: aspect.Operation<Mesh, Plane>().IsSupported);
        });
}

public sealed class TopologyAspectDispatchLaws {
    [Fact]
    public void OutputShapeGovernsSupportPerAspect() =>
        Spec.SupportMatrix(
            ("Domains Curve→Interval", static () => Topologies.Domains.Operation<Curve, Interval>().IsSupported, true),
            ("Domains Surface→Interval", static () => Topologies.Domains.Operation<Surface, Interval>().IsSupported, true),
            ("Domains Mesh→Interval", static () => Topologies.Domains.Operation<Mesh, Interval>().IsSupported, false),
            ("Domains Curve→int", static () => Topologies.Domains.Operation<Curve, int>().IsSupported, false),
            ("SolidOrientation Brep→BrepSolidOrientation", static () => Topologies.SolidOrientation.Operation<Brep, BrepSolidOrientation>().IsSupported, true),
            ("SolidOrientation Brep→int", static () => Topologies.SolidOrientation.Operation<Brep, int>().IsSupported, false),
            ("Components Mesh→Mesh", static () => Topologies.Components.Operation<Mesh, Mesh>().IsSupported, true),
            ("Components Brep→Brep", static () => Topologies.Components.Operation<Brep, Brep>().IsSupported, true),
            ("Components Mesh→Brep", static () => Topologies.Components.Operation<Mesh, Brep>().IsSupported, false),
            ("ContainsPoint Mesh→bool", static () => Topologies.ContainsPoint(point: Point3d.Origin).Operation<Mesh, bool>().IsSupported, true),
            ("ContainsPoint Curve→bool", static () => Topologies.ContainsPoint(point: Point3d.Origin).Operation<Curve, bool>().IsSupported, false),
            ("Kind Mesh→Kind", static () => Topologies.Kind.Operation<Mesh, Kind>().IsSupported, true),
            ("Kind Mesh→string", static () => Topologies.Kind.Operation<Mesh, string>().IsSupported, true),
            ("Kind Mesh→Topology", static () => Topologies.Kind.Operation<Mesh, Topology>().IsSupported, true),
            ("Kind Mesh→double", static () => Topologies.Kind.Operation<Mesh, double>().IsSupported, false));
    [Fact]
    public void InvalidContainmentPointCollapsesToUnsupported() =>
        Assert.False(condition: Topologies.ContainsPoint(point: Point3d.Unset).Operation<Mesh, bool>().IsSupported);
    [Fact]
    public void NullAspectRejectsWithInputFaultAndUnsupportedCarriesTypePair() {
        Spec.Invalid(Analyze.Run(operation: Analyze.Topologies<Mesh, int>(aspect: null!), input: default(Mesh)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(Analyze.Run(operation: Topologies.Domains.Operation<Mesh, Interval>(), input: default(Mesh)!),
            then: static error => {
                Fault.Unsupported fault = Assert.IsType<Fault.Unsupported>(@object: error);
                Assert.Equal(expected: typeof(Mesh), actual: fault.GeometryType);
                Assert.Equal(expected: typeof(Interval), actual: fault.OutputType);
            });
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public sealed class EulerCharacteristicOracleLaws {
    // Independent Euler V-E+F over MeshFixture index data (production EulerOf is bridge-deferred).
    [Theory]
    [MemberData(nameof(Surfaces))]
    public void EulerCharacteristicMatchesClosedFormOverFixtureIndices(Gens.MeshFixture fixture, int expectedChi) {
        ArgumentNullException.ThrowIfNull(argument: fixture);
        Assert.Equal(
            expected: expectedChi,
            actual: Numeric.EulerCharacteristic(
                vertices: fixture.Vertices.Count,
                edges: TopologyGens.DistinctUndirectedEdgeCount(fixture: fixture),
                faces: fixture.Triangles.Count));
    }
    public static TheoryData<Gens.MeshFixture, int> Surfaces => new() {
        { Gens.UnitTetrahedronFixture, 2 },
        { Gens.UnitCubeFixture, 2 },
        { TopologyGens.OpenQuadSheet, 1 },
    };
}

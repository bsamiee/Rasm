using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// Rasm.Analysis.Topology is bridge-heavy: every geometric scalar (EulerOf/GenusOf/BoundaryLoopsOf/
// HoleCountOf/ManifoldOf/SolidOrientationOf/DomainsOf/ContainsPoint/ComponentsOf/ElementCountOf) and
// TopologyScalar.Extract evaluate a live native Mesh/Brep (TopologyEdges, SolidOrientation, IsPointInside,
// SplitDisjointPieces, GetNakedEdges). Those successes are owned by *.verify.csx and are NOT faked here.
// The static rail owns the pure-managed surface only: the Topologies union catalog, the TopologyScalar
// SmartEnum catalog (key-uniqueness + per-case Output type), and the operation-construction DISPATCH —
// Analyze.Topology*<TGeometry,TOut>() routes on pure Type-shape predicates (typeof + GeometryKernel.Can*
// reflection, zero Rhino runtime) to either a built Operation (IsSupported) or key.Unsupported<>(); that
// decision table is asserted against an INDEPENDENT (geometry × output) support specification. The Euler
// closed form (V-E+F) is anchored over pure MeshFixture data via the independent Numeric.EulerCharacteristic
// oracle — the bridge scenario's documented predictor for native EulerOf.
internal static class TopologyGens {
    public static readonly Op Key = Op.Of(name: "topology-test");
    public static readonly TopologyScalar[] Scalars =
        [TopologyScalar.Manifold, TopologyScalar.Euler, TopologyScalar.BoundaryLoops, TopologyScalar.Genus,
         TopologyScalar.HoleCount, TopologyScalar.FaceCount, TopologyScalar.EdgeCount, TopologyScalar.VertexCount];
    // Mesh/Brep evaluate topology; Interval is curve/surface-only; bool is the containment/manifold output;
    // scalar.Output (int) is the count output — the independent (geometry × output) dispatch specification below.
    public static readonly Gen<Topologies> ScalarAspects = Gen.OneOfConst([.. Scalars.Select(static Topologies (s) => new Topologies.ScalarCase(Scalar: s))]);
    // V=4,E=5,F=2 -> chi=1 distinguishes an open sheet from a closed surface; the testkit closed fixtures are chi=2.
    public static readonly Gens.MeshFixture OpenQuadSheet = new(
        Vertices: Seq(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(1, 1, 0), new Point3d(0, 1, 0)),
        Triangles: Seq((0, 1, 2), (0, 2, 3)));
    public static int DistinctUndirectedEdgeCount(Gens.MeshFixture fixture) =>
        fixture.Triangles
            .Bind(static t => Seq((Math.Min(t.A, t.B), Math.Max(t.A, t.B)), (Math.Min(t.B, t.C), Math.Max(t.B, t.C)), (Math.Min(t.A, t.C), Math.Max(t.A, t.C))))
            .Distinct().Count;
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class TopologyScalarCatalogLaws {
    [Fact]
    public void KeysAreContiguousAndOutputTypesArePinned() {
        Spec.SmartEnumCatalogMatches(production: TopologyGens.Scalars, expectedKeys: [0, 1, 2, 3, 4, 5, 6, 7], key: static s => s.Key);
        Spec.Cases(items: TopologyGens.Scalars, key: static s => s.Key, law: static s =>
            Assert.Equal(expected: s.Key == 0 ? typeof(bool) : typeof(int), actual: s.Output));
    }
    [Fact]
    public void ManifoldAloneProjectsBoolEveryOtherScalarProjectsInt() {
        Assert.Equal(expected: 1, actual: TopologyGens.Scalars.Count(static s => s.Output == typeof(bool)));
        Assert.Equal(expected: 7, actual: TopologyGens.Scalars.Count(static s => s.Output == typeof(int)));
    }
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
        Spec.Cases(items: TopologyGens.Scalars, key: static s => s.Key, law: static s =>
            Assert.Same(expected: s, actual: Assert.IsType<Topologies.ScalarCase>(@object: new Topologies.ScalarCase(Scalar: s)).Scalar));
        Point3d payload = new(x: 3.0, y: -7.0, z: 11.0);
        Assert.Equal(expected: payload, actual: Assert.IsType<Topologies.ContainsPointCase>(@object: Topologies.ContainsPoint(point: payload)).Point);
        // Five non-scalar factories project five distinct union case types; eight scalar factories all project ScalarCase.
        Assert.Equal(expected: 6, actual: Cases.Select(static c => c.Aspect.GetType()).Distinct().Count());
        Assert.Equal(expected: 8, actual: Cases.Count(static c => c.Aspect is Topologies.ScalarCase));
    }
}

public sealed class TopologyScalarDispatchLaws {
    [Fact]
    public void MatchedOutputTypeBuildsSupportedOperationForTopologyGeometry() =>
        Spec.Cases(items: TopologyGens.Scalars.Where(static s => s.Output == typeof(int)).ToArray(), key: static s => s.Key, law: static s => {
            Assert.True(condition: new Topologies.ScalarCase(Scalar: s).Operation<Mesh, int>().IsSupported);
            Assert.True(condition: new Topologies.ScalarCase(Scalar: s).Operation<Brep, int>().IsSupported);
        });
    [Fact]
    public void ManifoldScalarRejectsIntAndCountScalarsRejectBool() {
        Assert.False(condition: Topologies.Manifold.Operation<Mesh, int>().IsSupported);
        Assert.True(condition: Topologies.Manifold.Operation<Mesh, bool>().IsSupported);
        Spec.Cases(items: TopologyGens.Scalars.Where(static s => s.Output == typeof(int)).ToArray(), key: static s => s.Key, law: static s =>
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
    public void OutputShapeGovernsSupportPerAspect() {
        Assert.True(condition: Topologies.Domains.Operation<Curve, Interval>().IsSupported);
        Assert.True(condition: Topologies.Domains.Operation<Surface, Interval>().IsSupported);
        Assert.False(condition: Topologies.Domains.Operation<Mesh, Interval>().IsSupported);
        Assert.False(condition: Topologies.Domains.Operation<Curve, int>().IsSupported);
        Assert.True(condition: Topologies.SolidOrientation.Operation<Brep, BrepSolidOrientation>().IsSupported);
        Assert.False(condition: Topologies.SolidOrientation.Operation<Brep, int>().IsSupported);
        Assert.True(condition: Topologies.Components.Operation<Mesh, Mesh>().IsSupported);
        Assert.True(condition: Topologies.Components.Operation<Brep, Brep>().IsSupported);
        Assert.False(condition: Topologies.Components.Operation<Mesh, Brep>().IsSupported);
        Assert.True(condition: Topologies.ContainsPoint(point: Point3d.Origin).Operation<Mesh, bool>().IsSupported);
        Assert.False(condition: Topologies.ContainsPoint(point: Point3d.Origin).Operation<Curve, bool>().IsSupported);
        Assert.True(condition: Topologies.Kind.Operation<Mesh, Kind>().IsSupported);
        Assert.True(condition: Topologies.Kind.Operation<Mesh, string>().IsSupported);
        Assert.True(condition: Topologies.Kind.Operation<Mesh, Topology>().IsSupported);
        Assert.False(condition: Topologies.Kind.Operation<Mesh, double>().IsSupported);
    }
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

// --- [EDGE_CASES] ---------------------------------------------------------------------------
public sealed class EulerCharacteristicOracleLaws {
    // INDEPENDENT ORACLE: production EulerOf reads a native Mesh (TopologyVertices/TopologyEdges/Faces) and is
    // bridge-deferred. Here V-E+F is re-derived over pure MeshFixture index data via Numeric.EulerCharacteristic
    // and TopologyGens.DistinctUndirectedEdgeCount — a closed-form path distinct from any production type-switch.
    // A closed triangulated 2-sphere (tetrahedron, cube triangulation) gives chi=2; an open quad sheet gives chi=1,
    // so the oracle catches the open-vs-closed distinction and is the documented predictor for the bridge scenario.
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

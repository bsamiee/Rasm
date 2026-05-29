using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// Rasm.Analysis.Mesh is bridge-heavy: every MeshSampleKind.Sample delegate reads a live native Mesh
// (IsValid/IsClosed/IsManifold/IsSolid/Faces/TopologyVertices/FaceNormals/GetNgonAndFacesCount...) and
// every MeshMetric.Measure/Shape/VerticesOf/NormalOf/FaceArea/FaceMaxDihedral evaluates native faces,
// ngons, and dihedral adjacency; MeshNakedEdges/MeshOutline/MeshAtVisiblePolygon/MeshVisiblePolygonCount
// and the MeshCheckParameters report all bridge-defer. Those successes are owned by *.verify.csx and are
// NOT faked here — this spec never calls .Sample/.Measure/.Shape nor Applies an Operation to a real Mesh.
// The static rail owns the pure-managed surface only: the Meshes union catalog (7 cases) + factory case/
// payload transport (Outline carries Plane, AtVisiblePolygon carries Option<int>, FaceQuality carries the
// MeshMetric), the three SmartEnum catalogs (MeshSampleGroup/MeshSampleKind/MeshMetric key-uniqueness +
// Group<->Kinds partition oracle + Inspect flag), MeshMetric.None InvalidInput rejection, and the
// Operation<TGeometry,TOut>() DISPATCH — a pure Type-shape decision (typeof(TOut) + Metric.None guard,
// zero Rhino runtime) routing to a built Operation (IsSupported) or a key.Unsupported<>()/Reject rail.
// Reject short-circuits at Operation.Supported() BEFORE any native Apply, so Analyze.Run over a Rejected
// op against default(Mesh)! is a pure managed rail observation, exactly like Topology's null-aspect law.
internal static class MeshGens {
    public static readonly Op Key = Op.Of(name: "mesh-test");
    public static readonly MeshSampleGroup[] Groups =
        [MeshSampleGroup.None, MeshSampleGroup.Validity, MeshSampleGroup.Count, MeshSampleGroup.Defect, MeshSampleGroup.Quality];
    public static readonly MeshMetric[] Metrics =
        [MeshMetric.None, MeshMetric.EdgeAspect, MeshMetric.Area, MeshMetric.Perimeter, MeshMetric.Skewness, MeshMetric.DihedralAngle];
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class MeshSampleGroupCatalogLaws {
    [Fact]
    public void KeysArePinnedLabelsMatchAndOnlyDefectInspects() {
        Spec.SmartEnumCatalogMatches(production: MeshGens.Groups, expectedKeys: [0, 1, 2, 3, 4], key: static g => g.Key);
        Spec.Cases(items: MeshGens.Groups, key: static g => g.Key, law: static g => {
            Assert.Equal(expected: g.Key switch { 0 => "None", 1 => "Validity", 2 => "Count", 3 => "Defect", _ => "Quality" }, actual: g.Label);
            Assert.Equal(expected: g.Key == 3, actual: g.Inspect);
        });
    }
    // INDEPENDENT PARTITION ORACLE: every MeshSampleKind declares exactly one Group; Group.Kinds re-derives
    // that membership by filtering the full catalog. Asserting each Kind's Group equals the owning partition
    // AND that the partition sizes sum to the full catalog catches any kind reassigned to the wrong group.
    [Fact]
    public void GroupKindsPartitionsTheKindCatalogExactly() {
        Seq<MeshSampleKind> all = toSeq(MeshSampleKind.Items);
        Spec.Cases(items: MeshGens.Groups, key: static g => g.Key, law: g => {
            Seq<MeshSampleKind> partition = all.Filter(k => k.Group.Equals(g));
            _ = partition.Iter(k => Assert.Same(expected: g, actual: k.Group));
            Assert.Equal(expected: partition.Count, actual: g.Kinds.Count);
        });
        Assert.Equal(expected: all.Count, actual: MeshGens.Groups.Sum(g => all.Filter(k => k.Group.Equals(g)).Count));
    }
}

public sealed class MeshSampleKindCatalogLaws {
    [Fact]
    public void KeysAreUniqueAndEveryKindGroupIsACatalogMember() {
        MeshSampleKind[] all = [.. MeshSampleKind.Items];
        Spec.Cases(items: all, key: static k => k.Key, law: static k =>
            Assert.Contains(expected: k.Group, collection: MeshSampleGroup.Items));
    }
}

public sealed class MeshMetricCatalogLaws {
    [Fact]
    public void KeysArePinnedAndNoneAloneRejectsInput() {
        Spec.SmartEnumCatalogMatches(production: MeshGens.Metrics, expectedKeys: [0, 1, 2, 3, 4, 5], key: static m => m.Key);
        Spec.Invalid(
            Analyze.Run(operation: Meshes.FaceQuality(metric: MeshMetric.None).Operation<Mesh, MeshMetricSample>(), input: default(Mesh)!),
            then: static error => {
                Assert.Equal(expected: "Input", actual: error.Category());
                Fault.InvalidInput fault = Assert.IsType<Fault.InvalidInput>(@object: error);
                Assert.Equal(expected: "MeshFaceQuality", actual: fault.Key.ToString());
            });
    }
}

public sealed class MeshesUnionCatalogLaws {
    public static readonly (string Label, Meshes Aspect)[] Cases =
        [("Validity", Meshes.Validity), ("Counts", Meshes.Counts), ("Defects", Meshes.Defects), ("Quality", Meshes.Quality),
         ("FaceQuality", Meshes.FaceQuality()), ("FaceShape", Meshes.FaceShape), ("AtVisiblePolygon", Meshes.AtVisiblePolygon()),
         ("VisiblePolygonCount", Meshes.VisiblePolygonCount), ("NakedEdges", Meshes.NakedEdges), ("Outline", Meshes.Outline(plane: Plane.WorldXY))];
    [Fact]
    public void SampleFactoriesProjectGroupCasesAndCarryDistinctPayloads() {
        // Four Samples factories all project SamplesCase but transport four distinct groups (no swap).
        Spec.Cases(
            items: [(Meshes.Validity, MeshSampleGroup.Validity), (Meshes.Counts, MeshSampleGroup.Count),
                    (Meshes.Defects, MeshSampleGroup.Defect), (Meshes.Quality, MeshSampleGroup.Quality)],
            key: static c => c.Item2.Key,
            law: static c => Assert.Same(expected: c.Item2, actual: Assert.IsType<Meshes.SamplesCase>(@object: c.Item1).Group));
        // Six non-Samples factories project six distinct case types; four Samples factories share SamplesCase.
        Assert.Equal(expected: 7, actual: Cases.Select(static c => c.Aspect.GetType()).Distinct().Count());
        Assert.Equal(expected: 4, actual: Cases.Count(static c => c.Aspect is Meshes.SamplesCase));
    }
    [Fact]
    public void TransportingFactoriesCarryTheirPayloadAndDefaultsResolve() {
        // new Plane(origin, normal) P/Invokes native axis derivation; copy WorldXY + shift origin stays managed.
        Plane plane = new(other: Plane.WorldXY) { Origin = new Point3d(x: 2.0, y: -3.0, z: 5.0) };
        Spec.Equal(left: plane, right: Assert.IsType<Meshes.OutlineCase>(@object: Meshes.Outline(plane: plane)).Plane, what: "outline plane");
        Spec.Some(Assert.IsType<Meshes.AtVisiblePolygonCase>(@object: Meshes.AtVisiblePolygon(index: 7)).Value, then: static v => Assert.Equal(expected: 7, actual: v));
        Spec.None(Assert.IsType<Meshes.AtVisiblePolygonCase>(@object: Meshes.AtVisiblePolygon()).Value);
        Assert.Same(expected: MeshMetric.EdgeAspect, actual: Assert.IsType<Meshes.FaceQualityCase>(@object: Meshes.FaceQuality()).Metric);
        Assert.Same(expected: MeshMetric.Skewness, actual: Assert.IsType<Meshes.FaceQualityCase>(@object: Meshes.FaceQuality(metric: MeshMetric.Skewness)).Metric);
    }
}

public sealed class MeshesDispatchLaws {
    [Fact]
    public void FaceQualityMetricGovernsOutputAndNoneRejects() {
        Assert.True(condition: Meshes.FaceQuality(metric: MeshMetric.Area).Operation<Mesh, MeshMetricSample>().IsSupported);
        Assert.True(condition: Meshes.FaceQuality(metric: MeshMetric.Area).Operation<Mesh, Stat>().IsSupported);
        Assert.False(condition: Meshes.FaceQuality(metric: MeshMetric.Area).Operation<Mesh, MeshFaceShape>().IsSupported);
        Assert.False(condition: Meshes.FaceQuality(metric: MeshMetric.None).Operation<Mesh, MeshMetricSample>().IsSupported);
    }
    [Fact]
    public void FaceShapeOnlyAcceptsMeshFaceShapeOutput() {
        Assert.True(condition: Meshes.FaceShape.Operation<Mesh, MeshFaceShape>().IsSupported);
        Assert.False(condition: Meshes.FaceShape.Operation<Mesh, MeshMetricSample>().IsSupported);
        Assert.False(condition: Meshes.FaceShape.Operation<Mesh, int>().IsSupported);
    }
    [Fact]
    public void UntypedLiftedCasesBuildRegardlessOfDeclaredOutput() {
        // Samples/AtVisiblePolygon/VisiblePolygonCount/NakedEdges arms always MeshLift (no typeof guard); their
        // support is unconditional at construction — native projection narrows the value at Apply time. Outline's
        // dispatch eagerly probes plane.IsValid (native rhcommon_c P/Invoke) → bridge-deferred, not asserted here.
        Assert.True(condition: Meshes.Counts.Operation<Mesh, MeshSample>().IsSupported);
        Assert.True(condition: Meshes.AtVisiblePolygon(index: 0).Operation<Mesh, TopologyProjection>().IsSupported);
        Assert.True(condition: Meshes.VisiblePolygonCount.Operation<Mesh, int>().IsSupported);
        Assert.True(condition: Meshes.NakedEdges.Operation<Mesh, Polyline>().IsSupported);
    }
    [Fact]
    public void UnsupportedOutputCollapsesToUnsupportedFaultWithTypePair() {
        Spec.Invalid(
            Analyze.Run(operation: Meshes.FaceShape.Operation<Mesh, int>(), input: default(Mesh)!),
            then: static error => {
                Fault.Unsupported fault = Assert.IsType<Fault.Unsupported>(@object: error);
                Assert.Equal(expected: typeof(Mesh), actual: fault.GeometryType);
                Assert.Equal(expected: typeof(int), actual: fault.OutputType);
            });
        Spec.Invalid(
            Analyze.Run(operation: Meshes.FaceQuality(metric: MeshMetric.Area).Operation<Mesh, MeshFaceShape>(), input: default(Mesh)!),
            then: static error => {
                Fault.Unsupported fault = Assert.IsType<Fault.Unsupported>(@object: error);
                Assert.Equal(expected: typeof(MeshFaceShape), actual: fault.OutputType);
            });
    }
}

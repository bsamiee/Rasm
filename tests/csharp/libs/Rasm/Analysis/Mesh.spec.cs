using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native MeshSample/Metric; static owns Meshes catalog, sample/metric metadata, Group partition oracle, Operation dispatch pre-native.
internal static class MeshGens {
    public static readonly Op Key = Op.Of(name: "mesh-test");
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class MeshSampleGroupCatalogLaws {
    [Fact]
    public void KeysArePinnedLabelsMatchAndOnlyDefectInspects() {
        Spec.SmartEnumCatalogMatches(production: MeshSampleGroup.Items, expectedKeys: [0, 1, 2, 3, 4], key: static g => g.Key);
        Spec.Cases(items: MeshSampleGroup.Items, key: static g => g.Key, law: static g => {
            Assert.Equal(expected: g.Key switch { 0 => "None", 1 => "Validity", 2 => "Count", 3 => "Defect", _ => "Quality" }, actual: g.Label);
            Assert.Equal(expected: g.Key == 3, actual: g.Inspect);
        });
    }
    // Independent partition oracle: re-derive Group membership from the full catalog; sizes must sum to the catalog.
    [Fact]
    public void GroupKindsPartitionsTheKindCatalogExactly() {
        Seq<MeshSampleKind> all = toSeq(MeshSampleKind.Items);
        Spec.Cases(items: MeshSampleGroup.Items, key: static g => g.Key, law: g => {
            Seq<MeshSampleKind> partition = all.Filter(k => k.Group.Equals(g));
            _ = partition.Iter(k => Assert.Same(expected: g, actual: k.Group));
            Assert.Equal(expected: partition.Count, actual: g.Kinds.Count);
        });
        Assert.Equal(expected: all.Count, actual: MeshSampleGroup.Items.Sum(g => all.Filter(k => k.Group.Equals(g)).Count));
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
        Spec.SmartEnumCatalogMatches(production: MeshMetric.Items, expectedKeys: [0, 1, 2, 3, 4, 5], key: static m => m.Key);
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
        Spec.Cases(
            items: [(Meshes.Validity, MeshSampleGroup.Validity), (Meshes.Counts, MeshSampleGroup.Count),
                    (Meshes.Defects, MeshSampleGroup.Defect), (Meshes.Quality, MeshSampleGroup.Quality)],
            key: static c => c.Item2.Key,
            law: static c => Assert.Same(expected: c.Item2, actual: Assert.IsType<Meshes.SamplesCase>(@object: c.Item1).Group));
        Assert.Equal(expected: 7, actual: Cases.Select(static c => c.Aspect.GetType()).Distinct().Count());
        Assert.Equal(expected: 4, actual: Cases.Count(static c => c.Aspect is Meshes.SamplesCase));
    }
    [Fact]
    public void TransportingFactoriesCarryTheirPayloadAndDefaultsResolve() {
        Spec.ForAll(Gens.ManagedPlane, static plane =>
            Spec.Equal(left: plane, right: Assert.IsType<Meshes.OutlineCase>(@object: Meshes.Outline(plane: plane)).Plane, what: "outline plane"));
        Spec.Some(Assert.IsType<Meshes.AtVisiblePolygonCase>(@object: Meshes.AtVisiblePolygon(index: 7)).Value, then: static v => Assert.Equal(expected: 7, actual: v));
        Spec.None(Assert.IsType<Meshes.AtVisiblePolygonCase>(@object: Meshes.AtVisiblePolygon()).Value);
        Assert.Same(expected: MeshMetric.EdgeAspect, actual: Assert.IsType<Meshes.FaceQualityCase>(@object: Meshes.FaceQuality()).Metric);
        Assert.Same(expected: MeshMetric.Skewness, actual: Assert.IsType<Meshes.FaceQualityCase>(@object: Meshes.FaceQuality(metric: MeshMetric.Skewness)).Metric);
    }
}

public sealed class MeshesDispatchLaws {
    [Fact]
    public void FaceQualityMetricGovernsOutputAndNoneRejects() =>
        Spec.SupportMatrix(
            ("FaceQuality(Area) Mesh→MeshMetricSample", static () => Meshes.FaceQuality(metric: MeshMetric.Area).Operation<Mesh, MeshMetricSample>().IsSupported, true),
            ("FaceQuality(Area) Mesh→Stat", static () => Meshes.FaceQuality(metric: MeshMetric.Area).Operation<Mesh, Stat>().IsSupported, true),
            ("FaceQuality(Area) Mesh→MeshFaceShape", static () => Meshes.FaceQuality(metric: MeshMetric.Area).Operation<Mesh, MeshFaceShape>().IsSupported, false),
            ("FaceQuality(None) Mesh→MeshMetricSample", static () => Meshes.FaceQuality(metric: MeshMetric.None).Operation<Mesh, MeshMetricSample>().IsSupported, false));
    [Fact]
    public void FaceShapeOnlyAcceptsMeshFaceShapeOutput() =>
        Spec.SupportMatrix(
            ("FaceShape Mesh→MeshFaceShape", static () => Meshes.FaceShape.Operation<Mesh, MeshFaceShape>().IsSupported, true),
            ("FaceShape Mesh→MeshMetricSample", static () => Meshes.FaceShape.Operation<Mesh, MeshMetricSample>().IsSupported, false),
            ("FaceShape Mesh→int", static () => Meshes.FaceShape.Operation<Mesh, int>().IsSupported, false));
    // MeshLift support is unconditional at construction; native projection narrows at Apply (Outline plane.IsValid is bridge-deferred).
    [Fact]
    public void UntypedLiftedCasesBuildRegardlessOfDeclaredOutput() =>
        Spec.SupportMatrix(
            ("Counts Mesh→MeshSample", static () => Meshes.Counts.Operation<Mesh, MeshSample>().IsSupported, true),
            ("AtVisiblePolygon Mesh→TopologyProjection", static () => Meshes.AtVisiblePolygon(index: 0).Operation<Mesh, TopologyProjection>().IsSupported, true),
            ("VisiblePolygonCount Mesh→int", static () => Meshes.VisiblePolygonCount.Operation<Mesh, int>().IsSupported, true),
            ("NakedEdges Mesh→Polyline", static () => Meshes.NakedEdges.Operation<Mesh, Polyline>().IsSupported, true));
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

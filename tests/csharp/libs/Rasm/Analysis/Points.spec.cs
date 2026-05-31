using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native point extraction/spread; static owns SpreadAspect/Points catalogs and (case x geometry x output) oracle.
internal static class PointsGens {
    public static readonly Op Key = Op.Of(name: "points-test");
    public static readonly Seq<Vector3d> Directions = Seq(new Vector3d(x: 1.0, y: 0.0, z: 0.0), new Vector3d(x: 0.0, y: 2.0, z: -3.0));
    // ExtremaCase is shared: both Quadrants and Extrema project it (six factories, five distinct case types).
    public static readonly (string Label, Points Aspect)[] Factories =
        [("Quadrants", Points.Quadrants), ("Extrema", Points.Extrema(directions: Directions)), ("EdgeMidpoints", Points.EdgeMidpoints),
         ("Vertices", Points.Vertices), ("ControlPoints", Points.ControlPoints), ("Spread", Points.Spread(aspect: SpreadAspect.Frame))];
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class SpreadAspectCatalogLaws {
    [Fact]
    public void KeysAreContiguousAndOutputTypesArePinnedPerCase() =>
        Spec.SmartEnumOutputCatalog(items: SpreadAspect.Items, expectedKeys: [0, 1, 2, 3, 4], key: static a => a.Key,
            output: static a => a.Output,
            expectedOutput: static a => a.Key switch { 0 or 1 => typeof(Plane), 2 => typeof(Stat), _ => typeof(bool) });
    [Fact]
    public void OutputTypePartitionMatchesGeometricVsScalarVsPredicate() {
        Assert.Equal(expected: 2, actual: SpreadAspect.Items.Count(static a => a.Output == typeof(Plane)));
        Assert.Equal(expected: 1, actual: SpreadAspect.Items.Count(static a => a.Output == typeof(Stat)));
        Assert.Equal(expected: 2, actual: SpreadAspect.Items.Count(static a => a.Output == typeof(bool)));
    }
}

public sealed class PointsUnionCatalogLaws {
    [Fact]
    public void FactoriesProjectDistinctCasesAndExtremaTransportsDirectionPayload() {
        Spec.None(result: Assert.IsType<Points.ExtremaCase>(@object: Points.Quadrants).Directions);
        Spec.Some(result: Assert.IsType<Points.ExtremaCase>(@object: Points.Extrema(directions: PointsGens.Directions)).Directions,
            then: actual => Assert.Equal(expected: PointsGens.Directions, actual: actual));
        _ = Assert.IsType<Points.EdgeMidpointsCase>(@object: Points.EdgeMidpoints);
        _ = Assert.IsType<Points.VerticesCase>(@object: Points.Vertices);
        _ = Assert.IsType<Points.ControlPointsCase>(@object: Points.ControlPoints);
        Assert.Same(expected: SpreadAspect.PrincipalFrame, actual: Assert.IsType<Points.SpreadCase>(@object: Points.Spread(aspect: SpreadAspect.PrincipalFrame)).Aspect);
        Assert.Equal(expected: 5, actual: PointsGens.Factories.Select(static f => f.Aspect.GetType()).Distinct().Count());
        Assert.Equal(expected: 2, actual: PointsGens.Factories.Count(static f => f.Aspect is Points.ExtremaCase));
    }
}

public sealed class PointsPointExtractionDispatchLaws {
    // Extraction cases gate on Kind predicates; oracle is the Geometry.cs topology table, not the production switch.
    [Fact]
    public void EachExtractionCaseHonorsItsOwnReadabilityPredicateUnderPoint3d() =>
        Spec.SupportMatrix(
            ("Quadrants Curve (curve-form)", static () => Points.Quadrants.Operation<Curve, Point3d>().IsSupported, true),
            ("Extrema Curve (curve-form)", static () => Points.Extrema(directions: PointsGens.Directions).Operation<Curve, Point3d>().IsSupported, true),
            ("Quadrants Mesh (not curve-form)", static () => Points.Quadrants.Operation<Mesh, Point3d>().IsSupported, false),
            ("EdgeMidpoints Mesh (edges)", static () => Points.EdgeMidpoints.Operation<Mesh, Point3d>().IsSupported, true),
            ("EdgeMidpoints Brep (edges)", static () => Points.EdgeMidpoints.Operation<Brep, Point3d>().IsSupported, true),
            ("EdgeMidpoints Curve (no edges)", static () => Points.EdgeMidpoints.Operation<Curve, Point3d>().IsSupported, false),
            ("Vertices Brep", static () => Points.Vertices.Operation<Brep, Point3d>().IsSupported, true),
            ("Vertices Mesh", static () => Points.Vertices.Operation<Mesh, Point3d>().IsSupported, true),
            ("Vertices Surface (no vertices)", static () => Points.Vertices.Operation<Surface, Point3d>().IsSupported, false),
            ("ControlPoints Surface", static () => Points.ControlPoints.Operation<Surface, Point3d>().IsSupported, true),
            ("ControlPoints Curve", static () => Points.ControlPoints.Operation<Curve, Point3d>().IsSupported, true),
            ("ControlPoints Mesh (no control net)", static () => Points.ControlPoints.Operation<Mesh, Point3d>().IsSupported, false));
    [Fact]
    public void NonPoint3dOutputCollapsesEveryExtractionCaseToUnsupported() =>
        Spec.SupportMatrix(
            ("Quadrants→Plane", static () => Points.Quadrants.Operation<Curve, Plane>().IsSupported, false),
            ("EdgeMidpoints→double", static () => Points.EdgeMidpoints.Operation<Mesh, double>().IsSupported, false),
            ("Vertices→Vector3d", static () => Points.Vertices.Operation<Brep, Vector3d>().IsSupported, false),
            ("ControlPoints→int", static () => Points.ControlPoints.Operation<Brep, int>().IsSupported, false),
            ("Extrema→Plane", static () => Points.Extrema(directions: PointsGens.Directions).Operation<Curve, Plane>().IsSupported, false));
}

public sealed class PointsSpreadDispatchLaws {
    // SpreadCase requires vertex-readable geometry (Mesh yes, Surface no) plus matching aspect output type.
    [Fact]
    public void MatchedAspectOutputBuildsOverVertexGeometryAndMismatchRejects() =>
        Spec.SupportMatrix(
            ("Frame Mesh→Plane", static () => Points.Spread(aspect: SpreadAspect.Frame).Operation<Mesh, Plane>().IsSupported, true),
            ("PrincipalFrame Mesh→Plane", static () => Points.Spread(aspect: SpreadAspect.PrincipalFrame).Operation<Mesh, Plane>().IsSupported, true),
            ("Distribution Mesh→Stat", static () => Points.Spread(aspect: SpreadAspect.Distribution).Operation<Mesh, Stat>().IsSupported, true),
            ("Collinear Mesh→bool", static () => Points.Spread(aspect: SpreadAspect.Collinear).Operation<Mesh, bool>().IsSupported, true),
            ("Coplanar Mesh→bool", static () => Points.Spread(aspect: SpreadAspect.Coplanar).Operation<Mesh, bool>().IsSupported, true),
            ("Frame Surface (no vertices)", static () => Points.Spread(aspect: SpreadAspect.Frame).Operation<Surface, Plane>().IsSupported, false));
    [Fact]
    public void ForeignOutputCollapsesSpreadToUnsupportedPerAspect() =>
        Spec.SupportMatrix(
            ("Frame→Stat", static () => Points.Spread(aspect: SpreadAspect.Frame).Operation<Mesh, Stat>().IsSupported, false),
            ("Distribution→Plane", static () => Points.Spread(aspect: SpreadAspect.Distribution).Operation<Mesh, Plane>().IsSupported, false),
            ("Coplanar→Plane", static () => Points.Spread(aspect: SpreadAspect.Coplanar).Operation<Mesh, Plane>().IsSupported, false),
            ("PrincipalFrame→bool", static () => Points.Spread(aspect: SpreadAspect.PrincipalFrame).Operation<Mesh, bool>().IsSupported, false),
            ("Collinear→Stat", static () => Points.Spread(aspect: SpreadAspect.Collinear).Operation<Mesh, Stat>().IsSupported, false));
}

// --- [EDGE_CASES] ---------------------------------------------------------------------------
public sealed class PointsUnsupportedRailLaws {
    [Fact]
    public void UnsupportedOperationCarriesStableCategoryAndExactTypePair() =>
        Spec.Invalid(Analyze.Run(operation: Points.Spread(aspect: SpreadAspect.Frame).Operation<Mesh, Stat>(), input: default(Mesh)!),
            then: static error => {
                Assert.Equal(expected: "Unsupported", actual: error.Category());
                Fault.Unsupported unsupported = Assert.IsType<Fault.Unsupported>(@object: error);
                Assert.Equal(expected: typeof(Mesh), actual: unsupported.GeometryType);
                Assert.Equal(expected: typeof(Stat), actual: unsupported.OutputType);
            });
    [Fact]
    public void ForeignGeometryRejectsEveryCaseRegardlessOfOutput() =>
        Spec.Cases(items: PointsGens.Factories, key: static f => f.Label, law: static f => {
            Assert.False(condition: f.Aspect.Operation<int, Point3d>().IsSupported);
            Assert.False(condition: f.Aspect.Operation<string, Plane>().IsSupported);
        });

    [Fact]
    public void NullAspectRejectsWithInputCategoryBeforeGeometryEvaluation() =>
        Spec.Invalid(Analyze.Run(operation: Analyze.Points<Mesh, Point3d>(sampling: null!), input: default(Mesh)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
    [Fact]
    public void NullSpreadAspectRejectsWithInputCategoryBeforeGeometryEvaluation() =>
        Spec.Invalid(Analyze.Run(operation: Points.Spread(aspect: null!).Operation<Mesh, Plane>(), input: default(Mesh)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
}

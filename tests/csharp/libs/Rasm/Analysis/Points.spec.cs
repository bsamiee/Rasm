using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// Rasm.Analysis.Points is bridge-heavy: every extracted point set (ExtremaCase via ExtremeParameters/
// CurveForm, EdgeMidpointsCase via PointAtNormalizedLength, VerticesCase via GeometryKernel.VerticesOf,
// ControlPointsCase via Nurbs control nets) and every spread computation (FitPlaneToPoints, OrientedFrame,
// PrincipalAngle eigen, Stat.Of distances) evaluate live native geometry and Vector3d.Unitize/IsTiny inside
// Analyze.ControlPointsOf/SpreadProject. Those successes are owned by *.verify.csx and are NOT faked here.
// The static rail owns the pure-managed surface only: the SpreadAspect SmartEnum catalog (key-uniqueness +
// per-case declared Output type — Frame/PrincipalFrame->Plane, Distribution->Stat, Collinear/Coplanar->bool),
// the Points union catalog (factory-to-case projection + Quadrants None vs Extrema Some directions payload),
// and the operation-construction DISPATCH — Points.Operation<TGeometry,TOut>() routes purely on Type-shape
// predicates (typeof + GeometryKernel.Can* reflection over the Kind registry, zero Rhino runtime) to either a
// built Operation (IsSupported) or key.Unsupported<>(). That decision table is asserted against an INDEPENDENT
// (case x geometry x output) support specification derived from Geometry.cs Kind topology sets, not the switch.
internal static class PointsGens {
    public static readonly Op Key = Op.Of(name: "points-test");
    public static readonly SpreadAspect[] Aspects =
        [SpreadAspect.Frame, SpreadAspect.PrincipalFrame, SpreadAspect.Distribution, SpreadAspect.Collinear, SpreadAspect.Coplanar];
    public static readonly Seq<Vector3d> Directions = Seq(new Vector3d(x: 1.0, y: 0.0, z: 0.0), new Vector3d(x: 0.0, y: 2.0, z: -3.0));
    // Five point-extraction factories project five distinct union case types; ExtremaCase appears twice (Quadrants/Extrema).
    public static readonly (string Label, Points Aspect)[] Factories =
        [("Quadrants", Points.Quadrants), ("Extrema", Points.Extrema(directions: Directions)), ("EdgeMidpoints", Points.EdgeMidpoints),
         ("Vertices", Points.Vertices), ("ControlPoints", Points.ControlPoints), ("Spread", Points.Spread(aspect: SpreadAspect.Frame))];
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class SpreadAspectCatalogLaws {
    [Fact]
    public void KeysAreContiguousAndOutputTypesArePinnedPerCase() {
        Spec.SmartEnumCatalogMatches(production: PointsGens.Aspects, expectedKeys: [0, 1, 2, 3, 4], key: static a => a.Key);
        Spec.Cases(items: PointsGens.Aspects, key: static a => a.Key, law: static a => Assert.Equal(
            expected: a.Key switch { 0 or 1 => typeof(Plane), 2 => typeof(Stat), _ => typeof(bool) },
            actual: a.Output));
    }
    [Fact]
    public void OutputTypePartitionMatchesGeometricVsScalarVsPredicate() {
        Assert.Equal(expected: 2, actual: PointsGens.Aspects.Count(static a => a.Output == typeof(Plane)));
        Assert.Equal(expected: 1, actual: PointsGens.Aspects.Count(static a => a.Output == typeof(Stat)));
        Assert.Equal(expected: 2, actual: PointsGens.Aspects.Count(static a => a.Output == typeof(bool)));
    }
}

public sealed class PointsUnionCatalogLaws {
    [Fact]
    public void FactoriesProjectDistinctCasesAndExtremaTransportsDirectionPayload() {
        // Quadrants projects ExtremaCase with no custom directions; Extrema(dirs) projects ExtremaCase carrying Some.
        Spec.None(result: Assert.IsType<Points.ExtremaCase>(@object: Points.Quadrants).Directions);
        Spec.Some(result: Assert.IsType<Points.ExtremaCase>(@object: Points.Extrema(directions: PointsGens.Directions)).Directions,
            then: actual => Assert.Equal(expected: PointsGens.Directions, actual: actual));
        _ = Assert.IsType<Points.EdgeMidpointsCase>(@object: Points.EdgeMidpoints);
        _ = Assert.IsType<Points.VerticesCase>(@object: Points.Vertices);
        _ = Assert.IsType<Points.ControlPointsCase>(@object: Points.ControlPoints);
        Assert.Same(expected: SpreadAspect.PrincipalFrame, actual: Assert.IsType<Points.SpreadCase>(@object: Points.Spread(aspect: SpreadAspect.PrincipalFrame)).Aspect);
        // Six factories produce five distinct case types (ExtremaCase shared by Quadrants + Extrema).
        Assert.Equal(expected: 5, actual: PointsGens.Factories.Select(static f => f.Aspect.GetType()).Distinct().Count());
        Assert.Equal(expected: 2, actual: PointsGens.Factories.Count(static f => f.Aspect is Points.ExtremaCase));
    }
}

public sealed class PointsPointExtractionDispatchLaws {
    // The four point-extraction cases all require TOut == Point3d and gate the geometry channel on a distinct
    // Kind predicate: Extrema/Quadrants->CanCurveForm, EdgeMidpoints->CanReadEdges, Vertices->CanReadVertices,
    // ControlPoints->CanReadControlPoints. Oracle is the Geometry.cs Kind topology truth table — Curve is curve-
    // form + vertex-readable + control-readable but NOT edge-readable; Mesh is edge/vertex-readable but neither
    // curve-form nor control-readable; Surface is control-readable only; Brep is edge/vertex/control-readable.
    [Fact]
    public void EachExtractionCaseHonorsItsOwnReadabilityPredicateUnderPoint3d() {
        Assert.True(condition: Points.Quadrants.Operation<Curve, Point3d>().IsSupported);
        Assert.True(condition: Points.Extrema(directions: PointsGens.Directions).Operation<Curve, Point3d>().IsSupported);
        Assert.False(condition: Points.Quadrants.Operation<Mesh, Point3d>().IsSupported);
        Assert.True(condition: Points.EdgeMidpoints.Operation<Mesh, Point3d>().IsSupported);
        Assert.True(condition: Points.EdgeMidpoints.Operation<Brep, Point3d>().IsSupported);
        Assert.False(condition: Points.EdgeMidpoints.Operation<Curve, Point3d>().IsSupported);
        Assert.True(condition: Points.Vertices.Operation<Brep, Point3d>().IsSupported);
        Assert.True(condition: Points.Vertices.Operation<Mesh, Point3d>().IsSupported);
        Assert.False(condition: Points.Vertices.Operation<Surface, Point3d>().IsSupported);
        Assert.True(condition: Points.ControlPoints.Operation<Surface, Point3d>().IsSupported);
        Assert.True(condition: Points.ControlPoints.Operation<Curve, Point3d>().IsSupported);
        Assert.False(condition: Points.ControlPoints.Operation<Mesh, Point3d>().IsSupported);
    }
    [Fact]
    public void NonPoint3dOutputCollapsesEveryExtractionCaseToUnsupported() {
        Assert.False(condition: Points.Quadrants.Operation<Curve, Plane>().IsSupported);
        Assert.False(condition: Points.EdgeMidpoints.Operation<Mesh, double>().IsSupported);
        Assert.False(condition: Points.Vertices.Operation<Brep, Vector3d>().IsSupported);
        Assert.False(condition: Points.ControlPoints.Operation<Brep, int>().IsSupported);
        Assert.False(condition: Points.Extrema(directions: PointsGens.Directions).Operation<Curve, Plane>().IsSupported);
    }
}

public sealed class PointsSpreadDispatchLaws {
    // SpreadCase gates support on s.Aspect.Output == typeof(TOut) AND CanReadVertices(TGeometry). Frame/PrincipalFrame
    // accept Plane, Distribution accepts Stat, Collinear/Coplanar accept bool. Mesh is vertex-readable, Surface is not,
    // so the matched output succeeds only over Mesh and every mismatched (aspect x output) pair stays Unsupported.
    [Fact]
    public void MatchedAspectOutputBuildsOverVertexGeometryAndMismatchRejects() {
        Assert.True(condition: Points.Spread(aspect: SpreadAspect.Frame).Operation<Mesh, Plane>().IsSupported);
        Assert.True(condition: Points.Spread(aspect: SpreadAspect.PrincipalFrame).Operation<Mesh, Plane>().IsSupported);
        Assert.True(condition: Points.Spread(aspect: SpreadAspect.Distribution).Operation<Mesh, Stat>().IsSupported);
        Assert.True(condition: Points.Spread(aspect: SpreadAspect.Collinear).Operation<Mesh, bool>().IsSupported);
        Assert.True(condition: Points.Spread(aspect: SpreadAspect.Coplanar).Operation<Mesh, bool>().IsSupported);
        Assert.False(condition: Points.Spread(aspect: SpreadAspect.Frame).Operation<Surface, Plane>().IsSupported);
    }
    [Fact]
    public void ForeignOutputCollapsesSpreadToUnsupportedPerAspect() {
        Assert.False(condition: Points.Spread(aspect: SpreadAspect.Frame).Operation<Mesh, Stat>().IsSupported);
        Assert.False(condition: Points.Spread(aspect: SpreadAspect.Distribution).Operation<Mesh, Plane>().IsSupported);
        Assert.False(condition: Points.Spread(aspect: SpreadAspect.Coplanar).Operation<Mesh, Plane>().IsSupported);
        Assert.False(condition: Points.Spread(aspect: SpreadAspect.PrincipalFrame).Operation<Mesh, bool>().IsSupported);
        Assert.False(condition: Points.Spread(aspect: SpreadAspect.Collinear).Operation<Mesh, Stat>().IsSupported);
    }
}

// --- [EDGE_CASES] ---------------------------------------------------------------------------
public sealed class PointsUnsupportedRailLaws {
    [Fact]
    public void UnsupportedFaultCarriesStableCategoryAndExactTypePair() {
        Error fault = PointsGens.Key.Unsupported(geometryType: typeof(Mesh), outputType: typeof(Plane));
        Assert.Equal(expected: "Unsupported", actual: fault.Category());
        Fault.Unsupported unsupported = Assert.IsType<Fault.Unsupported>(@object: fault);
        Assert.Equal(expected: typeof(Mesh), actual: unsupported.GeometryType);
        Assert.Equal(expected: typeof(Plane), actual: unsupported.OutputType);
    }
    [Fact]
    public void ForeignGeometryRejectsEveryCaseRegardlessOfOutput() =>
        Spec.Cases(items: PointsGens.Factories, key: static f => f.Label, law: static f => {
            Assert.False(condition: f.Aspect.Operation<int, Point3d>().IsSupported);
            Assert.False(condition: f.Aspect.Operation<string, Plane>().IsSupported);
        });
}

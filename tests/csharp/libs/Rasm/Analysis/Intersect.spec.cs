using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [MODELS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native Intersection.* / Analyze.*; static owns result construction, CanProject, unordered:true Supports/Shape, CanSelfIntersect, CanDeviation.
internal static class IntersectGens {
    public static readonly Op Key = Op.Of(name: "intersect-test");
    // Distinct per-element payloads so a verbatim-echo swap (e.g. Values[0]<->Values[1]) is observable.
    public static readonly Seq<Line> Lines = Seq(
        new Line(from: new Point3d(x: 0.0, y: 0.0, z: 0.0), to: new Point3d(x: 1.0, y: 0.0, z: 0.0)),
        new Line(from: new Point3d(x: 2.0, y: 0.0, z: 0.0), to: new Point3d(x: 2.0, y: 3.0, z: 0.0)),
        new Line(from: new Point3d(x: 4.0, y: 5.0, z: 6.0), to: new Point3d(x: 7.0, y: 8.0, z: 9.0)));
    public static readonly Seq<Point3d> Points = Seq(
        new Point3d(x: 2.0, y: 3.0, z: 5.0), new Point3d(x: 7.0, y: 11.0, z: 13.0), new Point3d(x: 17.0, y: 19.0, z: 23.0));
    public static readonly Seq<Interval> Intervals = Seq(
        new Interval(t0: 0.1, t1: 0.2), new Interval(t0: 0.3, t1: 0.5), new Interval(t0: 0.7, t1: 0.9));
    public static readonly IntersectionResult LinesCase = new IntersectionResult.Lines(Values: Lines);
    public static readonly IntersectionResult PointsCase = new IntersectionResult.Points(Values: Points);
    public static readonly IntersectionResult IntervalsCase = new IntersectionResult.Intervals(Values: Intervals);
    public static readonly Seq<(Polyline Curve, IntersectionKind Kind)> PolylineHits = Seq(
        (Curve: new Polyline([new Point3d(x: 0.0, y: 0.0, z: 0.0), new Point3d(x: 1.0, y: 2.0, z: 3.0)]), Kind: IntersectionKind.Curve),
        (Curve: new Polyline([new Point3d(x: 5.0, y: 7.0, z: 11.0), new Point3d(x: 13.0, y: 17.0, z: 19.0)]), Kind: IntersectionKind.Overlap));
    public static readonly Seq<IntersectionHit> Hits = Seq(
        IntersectionHit.At(point: Points[0], tangency: IntersectionTangency.Tangent),
        IntersectionHit.Overlap(start: Points[1], end: Points[2], overlapA: Intervals[0], overlapB: Intervals[1]));
    public static readonly IntersectionResult PolylinesCase = new IntersectionResult.Polylines(Values: PolylineHits);
    public static readonly IntersectionResult HitsCase = new IntersectionResult.Hits(Values: Hits);
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class IntersectionGeneratedOwnerLaws {
    [Fact]
    public void TangencyCatalogIsGeneratedOwner() =>
        Spec.SmartEnumCatalogMatches(production: IntersectionTangency.Items, expectedKeys: [0, 1, 2], key: static tangency => tangency.Key);
}

public sealed class IntersectionResultProjectionLaws {
    [Fact]
    public void EachValuePayloadCaseProjectsItsNativeOutputVerbatimAndInOrder() {
        Spec.Succ(result: IntersectGens.LinesCase.Project<Line>(key: IntersectGens.Key),
            then: static lines => Assert.Equal(expected: IntersectGens.Lines, actual: lines));
        Spec.Succ(result: IntersectGens.PointsCase.Project<Point3d>(key: IntersectGens.Key),
            then: static points => Assert.Equal(expected: IntersectGens.Points, actual: points));
        Spec.Succ(result: IntersectGens.IntervalsCase.Project<Interval>(key: IntersectGens.Key),
            then: static intervals => Assert.Equal(expected: IntersectGens.Intervals, actual: intervals));
    }
    [Fact]
    public void UniformIntersectionKindProjectionEmitsPerCaseTagRepeatedAcrossPayload() {
        Spec.Succ(result: IntersectGens.LinesCase.Project<IntersectionKind>(key: IntersectGens.Key),
            then: static kinds => Assert.Equal(expected: Seq(IntersectionKind.Curve, IntersectionKind.Curve, IntersectionKind.Curve), actual: kinds));
        Spec.Succ(result: IntersectGens.PointsCase.Project<IntersectionKind>(key: IntersectGens.Key),
            then: static kinds => Assert.Equal(expected: Seq(IntersectionKind.Point, IntersectionKind.Point, IntersectionKind.Point), actual: kinds));
        Spec.Succ(result: IntersectGens.IntervalsCase.Project<IntersectionKind>(key: IntersectGens.Key),
            then: static kinds => Assert.Equal(expected: Seq(IntersectionKind.Overlap, IntersectionKind.Overlap, IntersectionKind.Overlap), actual: kinds));
    }
    [Fact]
    public void ForeignOutputTypeCollapsesToUnsupportedCarryingCasePlusOutputPair() {
        Spec.FailUnsupportedFor(result: IntersectGens.LinesCase.Project<Point3d>(key: IntersectGens.Key), geometryType: typeof(IntersectionResult.Lines), outputType: typeof(Point3d));
        Spec.FailUnsupportedFor(result: IntersectGens.PointsCase.Project<Line>(key: IntersectGens.Key), geometryType: typeof(IntersectionResult.Points), outputType: typeof(Line));
        Spec.FailCategory(result: IntersectGens.IntervalsCase.Project<double>(key: IntersectGens.Key), category: "Unsupported");
    }

    [Fact]
    public void PolylineAndHitCasesProjectIndependentPayloadChannels() {
        Spec.Succ(result: IntersectGens.PolylinesCase.Project<IntersectionKind>(key: IntersectGens.Key),
            then: static kinds => Assert.Equal(expected: IntersectGens.PolylineHits.Map(static hit => hit.Kind), actual: kinds));
        Spec.Succ(result: IntersectGens.HitsCase.Project<Point3d>(key: IntersectGens.Key),
            then: static points => Assert.Equal(expected: Seq(IntersectGens.Points[0], IntersectGens.Points[1], IntersectGens.Points[2]), actual: points));
        Spec.Succ(result: IntersectGens.HitsCase.Project<IntersectionTangency>(key: IntersectGens.Key),
            then: static tangencies => Assert.Equal(expected: Seq(IntersectionTangency.Tangent, IntersectionTangency.Unknown), actual: tangencies));
    }
}

public sealed class IntersectionResultCanProjectLaws {
    // IntersectionResult is internal — use a private case table via Spec.Cases, not public Theory/MemberData rows.
    private static readonly (IntersectionResult Case, Type[] Accepted, Type[] Rejected)[] Rows = [
        (IntersectGens.LinesCase, [typeof(Line), typeof(IntersectionKind)], [typeof(Point3d), typeof(Interval), typeof(Polyline), typeof(double)]),
        (IntersectGens.PointsCase, [typeof(Point3d), typeof(IntersectionKind)], [typeof(Line), typeof(Interval), typeof(Polyline), typeof(double)]),
        (IntersectGens.IntervalsCase, [typeof(Interval), typeof(IntersectionKind)], [typeof(Line), typeof(Point3d), typeof(Polyline), typeof(double)]),
        (IntersectGens.PolylinesCase, [typeof(Polyline), typeof(IntersectionKind)], [typeof(Line), typeof(Point3d), typeof(Interval), typeof(double)]),
        (IntersectGens.HitsCase, [typeof(IntersectionHit), typeof(Curve), typeof(Point3d), typeof(Interval), typeof(IntersectionKind), typeof(IntersectionTangency)], [typeof(Line), typeof(Polyline), typeof(double)])];
    [Fact]
    public void CanProjectAcceptsNativeAndKindAndRejectsForeignChannels() =>
        Spec.Cases(items: Rows, key: static r => r.Case.GetType(), law: static r => {
            _ = toSeq(r.Accepted).Iter(channel => Spec.Holds(condition: r.Case.CanProject(output: channel), label: $"{r.Case.GetType().Name} accepts {channel.Name}"));
            _ = toSeq(r.Rejected).Iter(channel => Spec.Holds(condition: !r.Case.CanProject(output: channel), label: $"{r.Case.GetType().Name} rejects {channel.Name}"));
        });
}

public sealed class IntersectionShapeSupportConsistencyLaws {
    // BRIDGE-DEFERRED: unordered:false scans native IntersectionCases; static owns unordered:true direct/swap/wildcard resolution.
    private static readonly (Type Left, Type Right, Type Output)[] DirectShapes = [
        (typeof(Line), typeof(Line), typeof(Point3d)), (typeof(Plane), typeof(Plane), typeof(Line)),
        (typeof(Line), typeof(BoundingBox), typeof(Interval)), (typeof(Mesh), typeof(Plane), typeof(Polyline)),
        (typeof(Curve), typeof(Curve), typeof(IntersectionHit)), (typeof(Line), typeof(Plane), typeof(Point3d))];
    [Fact]
    public void DirectShapeTriplesResolveToAShapeThatCanProjectTheOutputAndSupportTracksIt() =>
        _ = toSeq(DirectShapes).Iter(static t => {
            Spec.Some(result: Analyze.IntersectionShape(left: t.Left, right: t.Right, output: t.Output, unordered: true),
                then: shape => Spec.Holds(condition: shape.CanProject(output: t.Output), label: $"{t.Left.Name}x{t.Right.Name}->{t.Output.Name} shape projects output"));
            Spec.Holds(condition: IntersectionResult.Supports(left: t.Left, right: t.Right, output: t.Output, unordered: true), label: $"Supports tracks shape for {t.Left.Name}x{t.Right.Name}");
        });
    // Plane/Line has no Plane-first ordered shape; unordered:true recovers support via the LinePlane case.
    [Fact]
    public void UnorderedSupportRecoversThroughArgumentSwap() =>
        Spec.SupportMatrix(
            ("Plane×Line→Point3d (swap recovery)", static () => IntersectionResult.Supports(left: typeof(Plane), right: typeof(Line), output: typeof(Point3d), unordered: true), true),
            ("Line×Plane→Point3d", static () => IntersectionResult.Supports(left: typeof(Line), right: typeof(Plane), output: typeof(Point3d), unordered: true), true));
    [Fact]
    public void ObjectWildcardSupportsAnyOutputAProjectableShapeOwnsAndForeignOutputCollapses() =>
        Spec.SupportMatrix(
            ("object×Line→Point3d", static () => IntersectionResult.Supports(left: typeof(object), right: typeof(Line), output: typeof(Point3d), unordered: true), true),
            ("Mesh×object→Polyline", static () => IntersectionResult.Supports(left: typeof(Mesh), right: typeof(object), output: typeof(Polyline), unordered: true), true),
            ("object×object→string (no shape)", static () => IntersectionResult.Supports(left: typeof(object), right: typeof(object), output: typeof(string), unordered: true), false),
            ("string×Guid→Point3d (no shape)", static () => IntersectionResult.Supports(left: typeof(string), right: typeof(Guid), output: typeof(Point3d), unordered: true), false));
}

public sealed class CapabilityPredicateLaws {
    [Fact]
    public void SelfIntersectAcceptsCurveAndMeshAndWildcardRejectingEverythingElse() {
        Spec.Cases(items: new[] { typeof(object), typeof(Curve), typeof(Mesh) }, key: static t => t,
            law: static t => Assert.True(condition: Analyze.CanSelfIntersect(geometry: t)));
        Spec.Cases(items: new[] { typeof(Point3d), typeof(Plane), typeof(Brep), typeof(Surface), typeof(string) }, key: static t => t,
            law: static t => Assert.False(condition: Analyze.CanSelfIntersect(geometry: t)));
    }
    [Fact]
    public void DeviationRequiresBothOperandsCoerceToCurveForm() =>
        Spec.SupportMatrix(
            ("Curve×Line", static () => Analyze.CanDeviation(left: typeof(Curve), right: typeof(Line)), true),
            ("Circle×Arc", static () => Analyze.CanDeviation(left: typeof(Circle), right: typeof(Arc)), true),
            ("Curve×Mesh", static () => Analyze.CanDeviation(left: typeof(Curve), right: typeof(Mesh)), false),
            ("Plane×Curve", static () => Analyze.CanDeviation(left: typeof(Plane), right: typeof(Curve)), false),
            ("Mesh×Brep", static () => Analyze.CanDeviation(left: typeof(Mesh), right: typeof(Brep)), false));
}

public sealed class AnalysisQueryRelationSupportLaws {
    private static readonly RayQuery ForwardRay = RayQuery.Of(new Ray3d(position: new Point3d(x: -1.0, y: 0.0, z: 0.0), direction: Vector3d.XAxis));

    [Fact]
    public void QueryRailOwnsPairRelationCapabilitiesAndRejectsForeignOutputs() =>
        Spec.SupportMatrix(
            ("Intersections Line×Line→Point3d", static () => Analyze.Query<Line, Line, Point3d>(AnalysisQuery.Intersections).IsSupported, true),
            ("Intersections Line×Line→string", static () => Analyze.Query<Line, Line, string>(AnalysisQuery.Intersections).IsSupported, false),
            ("Classification Curve×Curve→IntersectionTangency", static () => Analyze.Query<Curve, Curve, IntersectionTangency>(AnalysisQuery.Classification).IsSupported, true),
            ("Deviation Curve×Line→CurveDeviation", static () => Analyze.Query<Curve, Line, CurveDeviation>(AnalysisQuery.CurveDeviation).IsSupported, true),
            ("Deviation Curve×Mesh→CurveDeviation", static () => Analyze.Query<Curve, Mesh, CurveDeviation>(AnalysisQuery.CurveDeviation).IsSupported, false),
            ("Conformance Curve×Plane→double", static () => Analyze.Query<Curve, Plane, double>(AnalysisQuery.Conformance(metric: ConformanceMetric.Distance, count: 8)).IsSupported, true),
            ("Conformance Point3d×Plane→double", static () => Analyze.Query<Point3d, Plane, double>(AnalysisQuery.Conformance(metric: ConformanceMetric.Distance, count: 8)).IsSupported, false));

    [Fact]
    public void QueryRailOwnsSingleGeometryRelationCapabilitiesAndInvalidRayInput() {
        Spec.SupportMatrix(
            ("Self Curve→IntersectionHit", static () => Analyze.Query<Curve, IntersectionHit>(AnalysisQuery.SelfIntersection).IsSupported, true),
            ("Self Brep→IntersectionHit", static () => Analyze.Query<Brep, IntersectionHit>(AnalysisQuery.SelfIntersection).IsSupported, false),
            ("Ray Mesh→Point3d", static () => Analyze.Query<Mesh, Point3d>(AnalysisQuery.Ray(ForwardRay)).IsSupported, true),
            ("Ray Mesh→string", static () => Analyze.Query<Mesh, string>(AnalysisQuery.Ray(ForwardRay)).IsSupported, false));
        Spec.FailCategory(result: IntersectGens.Key.AcceptInput(value: RayQuery.Of(new Ray3d(position: Point3d.Origin, direction: Vector3d.Unset))), category: "Input");
    }
}

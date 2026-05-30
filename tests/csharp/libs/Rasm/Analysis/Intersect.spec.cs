using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
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
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
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
}

public sealed class IntersectionResultCanProjectLaws {
    // IntersectionResult is internal — use a private case table via Spec.Cases, not public Theory/MemberData rows.
    private static readonly (IntersectionResult Case, Type Native)[] Rows = [
        (IntersectGens.LinesCase, typeof(Line)), (IntersectGens.PointsCase, typeof(Point3d)), (IntersectGens.IntervalsCase, typeof(Interval))];
    private static readonly Type[] NativeChannels = [typeof(Line), typeof(Point3d), typeof(Interval), typeof(Polyline)];
    [Fact]
    public void CanProjectAcceptsNativeAndKindAndRejectsForeignChannels() =>
        Spec.Cases(items: Rows, key: static r => r.Native, law: static r => {
            Spec.Holds(condition: r.Case.CanProject(output: r.Native), label: $"CanProject native {r.Native.Name}");
            Spec.Holds(condition: r.Case.CanProject(output: typeof(IntersectionKind)), label: "CanProject IntersectionKind");
            Spec.Holds(condition: !r.Case.CanProject(output: typeof(double)), label: "rejects double");
            Spec.Holds(condition: !r.Case.CanProject(output: typeof(Plane)), label: "rejects Plane");
            _ = toSeq(NativeChannels).Filter(channel => channel != r.Native)
                .Iter(channel => Spec.Holds(condition: !r.Case.CanProject(output: channel), label: $"{r.Native.Name} rejects foreign {channel.Name}"));
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

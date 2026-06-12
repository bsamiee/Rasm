using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [MODELS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native edge/iso/silhouette extraction; static owns Curves union, output dispatch, and unsupported rails.
internal static class CurvesGens {
    public static readonly Vector3d Direction = new(x: 1.0, y: -2.0, z: 3.0);
    public static readonly (string Label, Curves Aspect)[] Cases =
        [("All", Curves.All), ("Boundary", Curves.Boundary), ("NakedOuter", Curves.NakedOuter), ("NakedInner", Curves.NakedInner),
         ("Interior", Curves.Interior), ("NonManifold", Curves.NonManifold), ("OuterLoop", Curves.OuterLoop), ("InnerLoop", Curves.InnerLoop),
         ("Segments", Curves.Segments(smooth: true)), ("Iso", Curves.Iso(direction: IsoStatus.X, normalized: 0.25)),
         ("Silhouette", Curves.Silhouette(direction: Direction)), ("Draft", Curves.Draft(direction: Direction, angle: 0.5)),
         ("At", Curves.At(index: 2)), ("Form", Curves.Form(index: 3))];
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class CurvesUnionCatalogLaws {
    [Fact]
    public void FactoriesProjectBoundedCasesAndCarryDistinctPayloads() {
        Assert.Equal(expected: 6, actual: CurvesGens.Cases.Select(static c => c.Aspect.GetType()).Distinct().Count());
        Assert.Equal(expected: 8, actual: CurvesGens.Cases.Count(static c => c.Aspect is Curves.EdgesCase));
        Assert.True(condition: Assert.IsType<Curves.SegmentsCase>(@object: Curves.Segments(smooth: true)).Smooth);
        Curves.IsoCase iso = Assert.IsType<Curves.IsoCase>(@object: Curves.Iso(direction: IsoStatus.Y, normalized: 0.75));
        Assert.Equal(expected: IsoStatus.Y, actual: iso.Direction);
        Spec.Equal(left: iso.Normalized, right: 0.75, tolerance: 0.0, what: "iso normalized");
        Curves.SilhouetteCase draft = Assert.IsType<Curves.SilhouetteCase>(@object: Curves.Draft(direction: CurvesGens.Direction, angle: 0.125));
        Assert.Equal(expected: CurvesGens.Direction, actual: draft.Direction);
        Spec.Some(draft.DraftAngle, angle => Spec.Equal(left: angle, right: 0.125, tolerance: 0.0, what: "draft angle"));
        Assert.Equal(expected: 9, actual: Assert.IsType<Curves.AtCase>(@object: Curves.At(index: 9)).Value);
        Assert.Equal(expected: 11, actual: Assert.IsType<Curves.FormCase>(@object: Curves.Form(index: 11)).Index);
    }
}

public sealed class CurvesDispatchLaws {
    [Fact]
    public void CurveExtractionSupportFollowsTopologyAndOutputShape() =>
        Spec.SupportMatrix(
            ("All Curve→Curve", static () => Curves.All.Operation<Curve, Curve>().IsSupported, true),
            ("All Curve→TopologyProjection", static () => Curves.All.Operation<Curve, TopologyProjection>().IsSupported, true),
            ("All Curve→CurveFeature", static () => Curves.All.Operation<Curve, CurveFeature>().IsSupported, true),
            ("All Curve→ComponentIndex", static () => Curves.All.Operation<Curve, ComponentIndex>().IsSupported, true),
            ("All Curve→Point3d", static () => Curves.All.Operation<Curve, Point3d>().IsSupported, false),
            ("Boundary Mesh→Curve", static () => Curves.Boundary.Operation<Mesh, Curve>().IsSupported, true),
            ("NakedOuter Mesh→Curve", static () => Curves.NakedOuter.Operation<Mesh, Curve>().IsSupported, false),
            ("Interior Mesh→ComponentIndex", static () => Curves.Interior.Operation<Mesh, ComponentIndex>().IsSupported, true),
            ("OuterLoop Brep→Curve", static () => Curves.OuterLoop.Operation<Brep, Curve>().IsSupported, true),
            ("OuterLoop Mesh→Curve", static () => Curves.OuterLoop.Operation<Mesh, Curve>().IsSupported, false));
    [Fact]
    public void DerivedCurveCasesClassifyStaticSupportWithoutExecutingNativeExtraction() =>
        Spec.SupportMatrix(
            ("Segments Curve→Curve", static () => Curves.Segments().Operation<Curve, Curve>().IsSupported, true),
            ("Segments Mesh→Curve", static () => Curves.Segments().Operation<Mesh, Curve>().IsSupported, false),
            ("Iso Surface→Curve", static () => Curves.Iso(direction: IsoStatus.X).Operation<Surface, Curve>().IsSupported, true),
            ("Iso Brep→Curve", static () => Curves.Iso(direction: IsoStatus.North).Operation<Brep, Curve>().IsSupported, true),
            ("Iso Mesh→Curve", static () => Curves.Iso(direction: IsoStatus.Y).Operation<Mesh, Curve>().IsSupported, false),
            ("Silhouette Mesh→Curve", static () => Curves.Silhouette().Operation<Mesh, Curve>().IsSupported, true),
            ("Silhouette Curve→Curve", static () => Curves.Silhouette().Operation<Curve, Curve>().IsSupported, false),
            ("Draft Surface→Curve", static () => Curves.Draft(angle: 0.25).Operation<Surface, Curve>().IsSupported, true),
            ("At Surface→Curve", static () => Curves.At().Operation<Surface, Curve>().IsSupported, true),
            ("Form Mesh→CurveForm", static () => Curves.Form().Operation<Mesh, CurveForm>().IsSupported, true),
            ("All Mesh→CurveForm", static () => Curves.All.Operation<Mesh, CurveForm>().IsSupported, false));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public sealed class CurvesRejectionRailLaws {
    [Fact]
    public void NullAspectAndForeignOutputRejectBeforeNativeEvaluation() {
        Spec.Invalid(Analyze.Run(operation: Analyze.Query<Curve, Curve>(query: null), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(Analyze.Run(operation: Curves.All.Operation<Curve, Point3d>(), input: default(Curve)!),
            then: static error => {
                Fault.Unsupported fault = Assert.IsType<Fault.Unsupported>(@object: error);
                Assert.Equal(expected: typeof(Curve), actual: fault.GeometryType);
                Assert.Equal(expected: typeof(Point3d), actual: fault.OutputType);
            });
    }
}

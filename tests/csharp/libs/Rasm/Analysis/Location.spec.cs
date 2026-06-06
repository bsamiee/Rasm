using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;
using LocationAspect = Rasm.Analysis.Location;

namespace Rasm.Tests.Analysis;

// --- [MODELS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native curve/surface point, frame, curvature, division, and short-path evaluation; static owns locator/value union dispatch.
internal static class LocationGens {
    public static readonly Point2d Uv = new(x: 0.25, y: 0.75);
    public static readonly Point3d Probe = new(x: 3.0, y: -5.0, z: 7.0);
    public static readonly Plane Frame = Plane.WorldXY;
    public static readonly Seq<double> Parameters = Seq(0.1, 0.2, 0.9);
    public static readonly (string Label, LocationAspect Aspect)[] Cases =
        [("PointAtCurve", LocationAspect.At(at: new Locator.CurveParameter(T: 0.5), value: LocationValue.Point)),
         ("FrameAtSurface", LocationAspect.At(at: new Locator.SurfaceParameter(Uv: Uv), value: LocationValue.Frame)),
         ("ClosestNormal", LocationAspect.At(at: new Locator.ClosestTo(Probe: Probe), value: LocationValue.Normal)),
         ("Curvature", LocationAspect.Curvature(count: 5, mode: CurvatureMode.Vector)),
         ("CurvatureExtrema", LocationAspect.CurvatureExtrema(count: 7, mode: CurvatureMode.Scalar(metric: ScalarMetric.Magnitude), direction: ExtremumDirection.Maximum)),
         ("DivideCount", LocationAspect.DivideByCount(count: 4)), ("DivideLength", LocationAspect.DivideByLength(length: 2.5)),
         ("Orientation", LocationAspect.Orientation(plane: Frame)), ("Contains", LocationAspect.Contains(point: Probe, plane: Frame)),
         ("ShortPath", LocationAspect.ShortPath(start: Uv, end: new Point2d(x: 0.8, y: 0.2)))];
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class LocationUnionCatalogLaws {
    [Fact]
    public void LocatorFactoriesCarryDistinctPayloads() {
        Spec.Equal(left: Assert.IsType<Locator.CurveParameter>(@object: new Locator.CurveParameter(T: 0.25)).T, right: 0.25, tolerance: 0.0, what: "curve t");
        Assert.Equal(expected: LocationGens.Uv, actual: Assert.IsType<Locator.SurfaceParameter>(@object: new Locator.SurfaceParameter(Uv: LocationGens.Uv)).Uv);
        Spec.Equal(left: Assert.IsType<Locator.ArcLength>(@object: new Locator.ArcLength(Distance: 8.0)).Distance, right: 8.0, tolerance: 0.0, what: "arc distance");
        Assert.Equal(expected: LocationGens.Probe, actual: Assert.IsType<Locator.ClosestTo>(@object: new Locator.ClosestTo(Probe: LocationGens.Probe)).Probe);
        Assert.Equal(expected: LocationGens.Parameters, actual: Assert.IsType<Locator.PerpendicularParameters>(@object: new Locator.PerpendicularParameters(Ts: LocationGens.Parameters)).Ts);
    }
    [Fact]
    public void LocationFactoriesProjectBoundedCasesAndCarryPayloads() {
        Assert.Equal(expected: 7, actual: LocationGens.Cases.Select(static c => c.Aspect.GetType()).Distinct().Count());
        LocationAspect.AtCase at = Assert.IsType<LocationAspect.AtCase>(@object: LocationAspect.At(at: new Locator.CurveParameter(T: 0.5), value: LocationValue.Tangent));
        _ = Assert.IsType<Locator.CurveParameter>(@object: at.Locator);
        _ = Assert.IsType<LocationValue.TangentCase>(@object: at.Value);
        Assert.Equal(expected: 12, actual: Assert.IsType<LocationValue.DerivativeCase>(@object: LocationValue.Derivative(order: 12)).Order);
        Assert.Equal(expected: 9, actual: Assert.IsType<Division.ByCount>(@object: Assert.IsType<LocationAspect.DivideCase>(@object: LocationAspect.DivideByCount(count: 9)).By).Count);
        Spec.Equal(left: Assert.IsType<Division.ByLength>(@object: Assert.IsType<LocationAspect.DivideCase>(@object: LocationAspect.DivideByLength(length: 1.25)).By).Length, right: 1.25, tolerance: 0.0, what: "division length");
    }
}

public sealed class LocationDispatchLaws {
    [Fact]
    public void LocatedCurveValuesSupportOnlyTheirDeclaredOutputShape() =>
        Spec.SupportMatrix(
            ("CurveParameter Point Curve→Point3d", static () => LocationAspect.At(new Locator.CurveParameter(T: 0.5), LocationValue.Point).Operation<Curve, Point3d>().IsSupported, true),
            ("CurveParameter Frame Curve→Plane", static () => LocationAspect.At(new Locator.CurveParameter(T: 0.5), LocationValue.Frame).Operation<Curve, Plane>().IsSupported, true),
            ("CurveParameter Tangent Curve→Vector3d", static () => LocationAspect.At(new Locator.CurveParameter(T: 0.5), LocationValue.Tangent).Operation<Curve, Vector3d>().IsSupported, true),
            ("CurveParameter Length Curve→double", static () => LocationAspect.At(new Locator.CurveParameter(T: 0.5), LocationValue.Length).Operation<Curve, double>().IsSupported, true),
            ("CurveParameter Point Curve→Plane", static () => LocationAspect.At(new Locator.CurveParameter(T: 0.5), LocationValue.Point).Operation<Curve, Plane>().IsSupported, false),
            ("SurfaceParameter Point Surface→Point3d", static () => LocationAspect.At(new Locator.SurfaceParameter(Uv: LocationGens.Uv), LocationValue.Point).Operation<Surface, Point3d>().IsSupported, true),
            ("SurfaceParameter Normal Surface→Vector3d", static () => LocationAspect.At(new Locator.SurfaceParameter(Uv: LocationGens.Uv), LocationValue.Normal).Operation<Surface, Vector3d>().IsSupported, true),
            ("SurfaceParameter Length Surface→double", static () => LocationAspect.At(new Locator.SurfaceParameter(Uv: LocationGens.Uv), LocationValue.Length).Operation<Surface, double>().IsSupported, false));
    [Fact]
    public void HigherLevelLocationAspectsClassifyStaticSupportWithoutNativeEvaluation() =>
        Spec.SupportMatrix(
            ("Curvature Curve→Vector3d", static () => LocationAspect.Curvature(count: 3, mode: CurvatureMode.Vector).Operation<Curve, Vector3d>().IsSupported, true),
            ("Curvature Curve→double", static () => LocationAspect.Curvature(count: 3, mode: CurvatureMode.Scalar(metric: ScalarMetric.Magnitude)).Operation<Curve, double>().IsSupported, true),
            ("Curvature Surface→Stat", static () => LocationAspect.Curvature(count: 3, mode: CurvatureMode.Scalar(metric: ScalarMetric.Mean)).Operation<Surface, Stat>().IsSupported, true),
            ("CurvatureExtrema Curve→Point3d", static () => LocationAspect.CurvatureExtrema(count: 3, mode: CurvatureMode.Scalar(metric: ScalarMetric.Magnitude), direction: ExtremumDirection.Minimum).Operation<Curve, Point3d>().IsSupported, true),
            ("DivideByCount Curve→Point3d", static () => LocationAspect.DivideByCount(count: 3).Operation<Curve, Point3d>().IsSupported, true),
            ("DivideByLength Curve→Point3d", static () => LocationAspect.DivideByLength(length: 1.0).Operation<Curve, Point3d>().IsSupported, true),
            ("Orientation Curve→CurveOrientation", static () => LocationAspect.Orientation(plane: LocationGens.Frame).Operation<Curve, CurveOrientation>().IsSupported, true),
            ("Contains Curve→PointContainment", static () => LocationAspect.Contains(point: LocationGens.Probe, plane: LocationGens.Frame).Operation<Curve, PointContainment>().IsSupported, true),
            ("ShortPath Surface→Curve", static () => LocationAspect.ShortPath(start: LocationGens.Uv, end: LocationGens.Uv).Operation<Surface, Curve>().IsSupported, true),
            ("DivideByCount Mesh→Point3d", static () => LocationAspect.DivideByCount(count: 3).Operation<Mesh, Point3d>().IsSupported, false));
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public sealed class LocationRejectionRailLaws {
    [Fact]
    public void InvalidDerivativeAndNullAspectRejectBeforeNativeEvaluation() {
        Spec.Invalid(Analyze.Run(operation: Analyze.Location<Curve, Point3d>(aspect: null!), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(
            Analyze.Run(operation: LocationAspect.At(new Locator.CurveParameter(T: 0.5), LocationValue.Derivative(order: -1)).Operation<Curve, Vector3d>(), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(
            Analyze.Run(operation: LocationAspect.At(new Locator.SurfaceParameter(Uv: LocationGens.Uv), LocationValue.Length).Operation<Surface, double>(), input: default(Surface)!),
            then: static error => {
                Fault.Unsupported fault = Assert.IsType<Fault.Unsupported>(@object: error);
                Assert.Equal(expected: typeof(Surface), actual: fault.GeometryType);
                Assert.Equal(expected: typeof(double), actual: fault.OutputType);
            });
    }

    [Fact]
    public void InvalidClosestTargetAndNonPositiveCurvatureCountRejectBeforeNativeEvaluation() {
        Spec.Invalid(
            Analyze.Run(operation: LocationAspect.At(new Locator.ClosestTo(Probe: Point3d.Unset), LocationValue.Point).Operation<Curve, Point3d>(), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(
            Analyze.Run(operation: LocationAspect.Curvature(count: 0, mode: CurvatureMode.Vector).Operation<Curve, Vector3d>(), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(
            Analyze.Run(operation: LocationAspect.CurvatureExtrema(count: -3, mode: CurvatureMode.Scalar(metric: ScalarMetric.Magnitude), direction: ExtremumDirection.Maximum).Operation<Curve, Point3d>(), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
    }

    [Fact]
    public void InvalidDivideInputsRejectBeforeNativeEvaluation() {
        Spec.Invalid(
            Analyze.Run(operation: LocationAspect.DivideByCount(count: 0).Operation<Curve, Point3d>(), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(
            Analyze.Run(operation: LocationAspect.DivideByCount(count: -2).Operation<Curve, Point3d>(), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(
            Analyze.Run(operation: LocationAspect.DivideByLength(length: 0.0).Operation<Curve, Point3d>(), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(
            Analyze.Run(operation: LocationAspect.DivideByLength(length: double.NaN).Operation<Curve, Point3d>(), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(
            Analyze.Run(operation: LocationAspect.DivideByLength(length: double.PositiveInfinity).Operation<Curve, Point3d>(), input: default(Curve)!),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
    }
}

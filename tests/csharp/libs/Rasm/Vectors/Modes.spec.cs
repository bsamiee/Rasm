using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [MODELS] ----------------------------------------------------------------------------
internal static class ModeGens {
    public static readonly Op Key = Op.Of(name: "modes-test");
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: Rhino.UnitSystem.Millimeters).ToFin(), label: "modes context");
    public static readonly CurveProjection[] CurveModes = [
        CurveProjection.Tangent, CurveProjection.Curvature, CurveProjection.Frame, CurveProjection.PerpendicularFrame, CurveProjection.ArcLength,
        CurveProjection.FrameNormal, CurveProjection.FrameBinormal, CurveProjection.PerpendicularNormal, CurveProjection.PerpendicularBinormal,
    ];
    public static readonly SurfaceProjection[] SurfaceModes = [
        SurfaceProjection.PrincipalCurvatures, SurfaceProjection.Gaussian, SurfaceProjection.Mean, SurfaceProjection.MaximumOsculatingCircle,
        SurfaceProjection.Normal, SurfaceProjection.ShapeOperator, SurfaceProjection.MinimumOsculatingCircle, SurfaceProjection.Point,
        SurfaceProjection.Frame, SurfaceProjection.UvFrame, SurfaceProjection.Jacobian, SurfaceProjection.Metric, SurfaceProjection.AreaScale,
    ];
    public static readonly ConeProjection[] ConeModes = [
        ConeProjection.HalfAngle, ConeProjection.SolidAngle, ConeProjection.Axis, ConeProjection.Apex,
    ];
    public static readonly MotionInterpolation[] Motions = [MotionInterpolation.Linear, MotionInterpolation.Slerp];
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class ModeCatalogLaws {
    [Fact]
    public void ProjectionCatalogsHaveStableDistinctKeys() {
        Spec.SmartEnumKeysUnique(items: ModeGens.CurveModes, key: static mode => mode.Key);
        Spec.SmartEnumKeysUnique(items: ModeGens.SurfaceModes, key: static mode => mode.Key);
        Spec.SmartEnumKeysUnique(items: ModeGens.ConeModes, key: static mode => mode.Key);
        Spec.SmartEnumKeysUnique(items: ModeGens.Motions, key: static mode => mode.Key);
    }
}

public sealed class NativeProjectionBoundaryLaws {
    [Fact]
    public void NullCurveAndSurfaceReturnTypedFailuresBeforeNativeDispatch() {
        Spec.FailCategory(CurveProjection.Tangent.Project<Vector3d>(curve: null!, parameter: 0.0, context: ModeGens.Model, key: ModeGens.Key), category: "Input");
        Spec.FailCategory(SurfaceProjection.Normal.Project<Vector3d>(surface: null!, u: 0.0, v: 0.0, context: ModeGens.Model, key: ModeGens.Key), category: "Input");
    }
}

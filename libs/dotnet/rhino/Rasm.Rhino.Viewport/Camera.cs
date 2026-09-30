using Rasm.Rhino.Document;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Viewport;

// --- [TYPES] ---------------------------------------------------------------------------
public enum CameraProjection { Parallel = 0, Perspective = 1, TwoPoint = 2 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record CameraPose(Plane Frame, Point3d Target, double CameraAngle, CameraProjection Projection);

public sealed record DepthExtent(double Near, double Far);

public sealed record CameraFrustum(double Left, double Right, double Bottom, double Top, double Near, double Far, double Aspect, BoundingBox Bounds);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DetailLength {
    public sealed record Paper(double Length) : DetailLength;

    public sealed record Model(double Length) : DetailLength;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Cameras {
    // --- [POSE]
    public static IO<CameraPose> ReadPose(RhinoViewport viewport) =>
        from frame in IO.lift(() => Refused.Unless(viewport.GetCameraFrame(out Plane read), read, nameof(RhinoViewport.GetCameraFrame)))
        from target in IO.lift(() => viewport.CameraTarget)
        from half in IO.lift(() => viewport.CameraAngle)
        from angle in IO.lift(InvalidAnswer.Unless(half > 0.0, half, nameof(RhinoViewport.CameraAngle)))
        from projection in ReadProjection(viewport)
        select new CameraPose(frame, target, angle, projection);

    public static IO<Unit> WritePose(RhinoViewport viewport, CameraPose pose) =>
        from actual in ReadProjection(viewport)
        from same in IO.lift(() => ProjectionMismatch.Unless(pose.Projection, actual))
        from written in IO.lift(() => {
            viewport.SetCameraLocations(pose.Target, pose.Frame.Origin);
            viewport.SetCameraDirection(-pose.Frame.ZAxis, updateTargetLocation: false);
            viewport.CameraUp = pose.Frame.YAxis;
            viewport.CameraAngle = pose.CameraAngle;
        })
        select written;

    public static IO<ViewportInfo> ToViewportInfo(CameraPose pose, BoundingBox bounds) =>
        from viewport in IO.lift(static () => new ViewportInfo())
        from placed in DisposalOps.OnFailure(
            from aimed in IO.lift(() =>
                from located in Refused.Unless(viewport.SetCameraLocation(pose.Frame.Origin), nameof(ViewportInfo.SetCameraLocation))
                from directed in Refused.Unless(viewport.SetCameraDirection(-pose.Frame.ZAxis), nameof(ViewportInfo.SetCameraDirection))
                from upright in Refused.Unless(viewport.SetCameraUp(pose.Frame.YAxis), nameof(ViewportInfo.SetCameraUp))
                select unit)
            from projected in IO.lift(() => pose.Projection switch {
                CameraProjection.Parallel => Refused.Unless(viewport.ChangeToParallelProjection(symmetricFrustum: true), nameof(ViewportInfo.ChangeToParallelProjection)),
                CameraProjection.Perspective => Refused.Unless(
                    viewport.ChangeToPerspectiveProjection(pose.Frame.Origin.DistanceTo(pose.Target), symmetricFrustum: true, Lens(viewport, pose.CameraAngle)),
                    nameof(ViewportInfo.ChangeToPerspectiveProjection)),
                CameraProjection.TwoPoint => Refused.Unless(
                    viewport.ChangeToTwoPointPerspectiveProjection(pose.Frame.Origin.DistanceTo(pose.Target), pose.Frame.YAxis, Lens(viewport, pose.CameraAngle)),
                    nameof(ViewportInfo.ChangeToTwoPointPerspectiveProjection)),
            })
            from clipped in IO.lift(() => Refused.Unless(viewport.SetFrustumNearFar(bounds), nameof(ViewportInfo.SetFrustumNearFar)))
            select viewport,
            IO.lift(viewport.Dispose))
        select placed;

    private static IO<CameraProjection> ReadProjection(RhinoViewport viewport) =>
        IO.lift(() => viewport switch {
            { IsTwoPointPerspectiveProjection: true } => CameraProjection.TwoPoint,
            { IsPerspectiveProjection: true } => CameraProjection.Perspective,
            _ => CameraProjection.Parallel,
        });

    private static double Lens(ViewportInfo viewport, double cameraAngle) {
        viewport.CameraAngle = cameraAngle;
        return viewport.Camera35mmLensLength;
    }

    // --- [FRUSTUM]
    public static IO<CameraFrustum> GetFrustum(RhinoViewport viewport) =>
        from box in IO.lift(viewport.GetFrustumBoundingBox)
        from bounds in IO.lift(InvalidAnswer.Unless(box.IsValid, box, nameof(RhinoViewport.GetFrustumBoundingBox)))
        from frustum in IO.lift(() => Refused.Unless(
            viewport.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out double far),
            new CameraFrustum(left, right, bottom, top, near, far, viewport.FrustumAspect, bounds),
            nameof(RhinoViewport.GetFrustum)))
        select frustum;

    public static IO<Line> GetFrustumLine(RhinoViewport viewport, double screenX, double screenY) =>
        IO.lift(() => Refused.Unless(viewport.GetFrustumLine(screenX, screenY, out Line line), line, nameof(RhinoViewport.GetFrustumLine)));

    public static IO<double> GetDepth(RhinoViewport viewport, Point3d point) =>
        IO.lift(() => Refused.Unless(viewport.GetDepth(point, out double distance), distance, nameof(RhinoViewport.GetDepth)));

    public static IO<DepthExtent> GetDepth(RhinoViewport viewport, BoundingBox bounds) =>
        IO.lift(() => Refused.Unless(viewport.GetDepth(bounds, out double near, out double far), new DepthExtent(near, far), nameof(RhinoViewport.GetDepth)));

    public static IO<DepthExtent> GetDepth(RhinoViewport viewport, Sphere sphere) =>
        IO.lift(() => Refused.Unless(viewport.GetDepth(sphere, out double near, out double far), new DepthExtent(near, far), nameof(RhinoViewport.GetDepth)));

    // --- [COORDINATES]
    public static IO<Transform> Xform(RhinoViewport viewport, CoordinateSystem source, CoordinateSystem destination) =>
        DisposalOps.Using(() => new ViewportInfo(viewport), info =>
            from xform in IO.lift(() => info.GetXform(source, destination))
            from valid in IO.lift(InvalidAnswer.Unless(xform.IsValid, xform, nameof(ViewportInfo.GetXform)))
            select valid);

    public static IO<double> GetWorldToScreenScale(RhinoViewport viewport, Point3d at) =>
        IO.lift(() => Refused.Unless(viewport.GetWorldToScreenScale(at, out double pixelsPerUnit), pixelsPerUnit, nameof(RhinoViewport.GetWorldToScreenScale)));

    public static IO<ConstructionPlane> GetConstructionPlane(RhinoViewport viewport) =>
        IO.lift(() => Missing.Unless(viewport.GetConstructionPlane(), nameof(RhinoViewport.GetConstructionPlane)));

    // --- [DETAIL]
    public static IO<DetailLength> ConvertDetailLength(DetailViewObject detail, DetailLength length) =>
        IO.lift(() => length.Switch(
            detail,
            paper: static (view, paper) =>
                Refused.Unless<DetailLength>(view.TryGetModelLength(paper.Length, out double model), new DetailLength.Model(model), nameof(DetailViewObject.TryGetModelLength)),
            model: static (view, model) =>
                Refused.Unless<DetailLength>(view.TryGetPaperLength(model.Length, out double paper), new DetailLength.Paper(paper), nameof(DetailViewObject.TryGetPaperLength))));
}

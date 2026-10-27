using Rasm.RhinoCommon.Document.Units;
using Rhino.Display;
using Rhino.DocObjects;
using UnitsNet;

namespace Rasm.RhinoCommon.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record CameraLens {
    public sealed record Parallel(double Extent) : CameraLens;

    public sealed record Perspective(Angle Angle) : CameraLens;

    public sealed record TwoPoint(Angle Angle, Vector3d Up) : CameraLens;
}

public sealed record CameraPose(Plane Frame, Point3d Target, CameraLens Lens) {
    public bool Matches(CameraPose other, Tolerances tolerances) =>
        tolerances.Matches(Frame, other.Frame)
        && Target.DistanceTo(other.Target) <= tolerances.Absolute
        && Lens.Switch(
            (Other: other.Lens, Tolerances: tolerances),
            parallel: static (at, lens) => at.Other is CameraLens.Parallel held && Math.Abs(lens.Extent - held.Extent) <= at.Tolerances.Absolute,
            perspective: static (at, lens) => at.Other is CameraLens.Perspective held && Math.Abs((lens.Angle - held.Angle).Radians.ToDouble()) <= at.Tolerances.Angle,
            twoPoint: static (at, lens) => at.Other is CameraLens.TwoPoint held
                && Math.Abs((lens.Angle - held.Angle).Radians.ToDouble()) <= at.Tolerances.Angle
                && Vector3d.VectorAngle(lens.Up, held.Up) <= at.Tolerances.Angle);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Cameras {
    // --- [POSE]
    public static IO<CameraPose> Read(ViewportInfo camera) =>
        IO.lift(() => RhinoFault.Unless(camera.GetCameraFrame(out Point3d location, out Vector3d x, out Vector3d y, out _)).Map(_ => new Plane(location, x, y))
            .Bind(frame => (camera.IsParallelProjection
                    ? RhinoFault.Unless(camera.GetFrustum(out double left, out double right, out double bottom, out double top, out _, out _))
                        .Map(_ => (CameraLens)new CameraLens.Parallel(Math.Min(Math.Max(right, -left), Math.Max(top, -bottom))))
                    : camera.IsTwoPointPerspectiveProjection
                        ? (CameraLens)new CameraLens.TwoPoint(Angle.FromRadians(camera.CameraAngle), camera.CameraUp)
                        : new CameraLens.Perspective(Angle.FromRadians(camera.CameraAngle)))
                .Map(lens => new CameraPose(frame, camera.TargetPoint, lens))));

    public static IO<CameraPose> Read(RhinoViewport viewport) =>
        use(() => new ViewportInfo(viewport)).Bind(static info => Read(info)).Bracket();

    public static Fin<Unit> Write(RhinoViewport viewport, CameraPose pose) {
        using ViewportInfo copy = new(viewport);
        return Pose(copy, pose).Bind(posed => Applied(viewport, posed));
    }

    public static Fin<ViewportInfo> Pose(ViewportInfo camera, CameraPose pose) {
        camera.UnlockCamera();
        return from directed in RhinoFault.Unless(camera.SetCameraDirection(-pose.Frame.ZAxis))
               from aimed in RhinoFault.Unless(camera.SetCameraUp(pose.Frame.YAxis))
               from projected in pose.Lens.Switch(
                   (Camera: camera, Distance: pose.Frame.Origin.DistanceTo(pose.Target)),
                   parallel: static (state, parallel) =>
                       from changed in RhinoFault.Unless(state.Camera.ChangeToParallelProjection(symmetricFrustum: true))
                       let width = parallel.Extent * Math.Max(1d, state.Camera.FrustumAspect)
                       let height = parallel.Extent * Math.Max(1d, 1d / state.Camera.FrustumAspect)
                       from framed in RhinoFault.Unless(state.Camera.SetFrustum(-width, width, -height, height, state.Camera.FrustumNear, state.Camera.FrustumFar))
                       select framed,
                   perspective: static (state, perspective) =>
                       RhinoFault.Unless(state.Camera.ChangeToPerspectiveProjection(state.Distance, symmetricFrustum: true, state.Camera.Camera35mmLensLength))
                           .Map(fun((Unit _) => { state.Camera.CameraAngle = perspective.Angle.Radians.ToDouble(); })),
                   twoPoint: static (state, twoPoint) =>
                       RhinoFault.Unless(state.Camera.ChangeToTwoPointPerspectiveProjection(state.Distance, twoPoint.Up, state.Camera.Camera35mmLensLength))
                           .Map(fun((Unit _) => { state.Camera.CameraAngle = twoPoint.Angle.Radians.ToDouble(); })))
               from located in RhinoFault.Unless(camera.SetCameraLocation(pose.Frame.Origin))
                   .Map(fun((Unit _) => { camera.TargetPoint = pose.Target; }))
               select camera;
    }

    // --- [HOLD]
    public static IO<T> Within<T>(RhinoViewport viewport, Func<ViewportInfo, IO<Unit>> edit, IO<T> body) =>
        from held in use(() => new ViewportInfo(viewport))
        from edited in (from camera in use(() => new ViewportInfo(viewport))
                        from staged in edit(camera)
                        from applied in IO.lift(() => Applied(viewport, camera))
                        select applied).Bracket()
        from value in body.Finally(IO.lift(() => Applied(viewport, held)))
        select value;

    private static Fin<Unit> Applied(RhinoViewport viewport, ViewportInfo camera) =>
        RhinoFault.Unless(!viewport.LockedProjection)
            .Bind(_ => RhinoFault.Unless(viewport.SetViewProjection(camera, updateTargetLocation: false)))
            .Map(fun((Unit _) => viewport.SetCameraTarget(camera.TargetPoint, updateCameraLocation: false)));
}

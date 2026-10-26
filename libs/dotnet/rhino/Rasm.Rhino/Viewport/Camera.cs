using Rasm.Rhino.Document.Units;
using Rhino.Display;
using Rhino.DocObjects;
using UnitsNet;

namespace Rasm.Rhino.Viewport;

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
        IO.lift(() => Refused.Unless(camera.GetCameraFrame(out Point3d location, out Vector3d x, out Vector3d y, out _), new Plane(location, x, y), nameof(ViewportInfo.GetCameraFrame))
            .Bind(frame => (camera.IsParallelProjection
                    ? Refused.Unless(camera.GetFrustum(out double left, out double right, out double bottom, out double top, out _, out _), nameof(ViewportInfo.GetFrustum))
                        .Map(_ => (CameraLens)new CameraLens.Parallel(Math.Min(Math.Max(right, -left), Math.Max(top, -bottom))))
                    : camera.IsTwoPointPerspectiveProjection
                        ? (CameraLens)new CameraLens.TwoPoint(Angle.FromRadians(camera.CameraAngle), camera.CameraUp)
                        : new CameraLens.Perspective(Angle.FromRadians(camera.CameraAngle)))
                .Map(lens => new CameraPose(frame, camera.TargetPoint, lens))));

    public static IO<CameraPose> Read(RhinoViewport viewport) =>
        DisposalOps.Within(() => new ViewportInfo(viewport), Read);

    public static Fin<Unit> Write(RhinoViewport viewport, CameraPose pose) {
        using ViewportInfo copy = new(viewport);
        return Pose(copy, pose).Bind(posed => Applied(viewport, posed));
    }

    public static Fin<ViewportInfo> Pose(ViewportInfo camera, CameraPose pose) {
        camera.UnlockCamera();
        return from directed in Refused.Unless(camera.SetCameraDirection(-pose.Frame.ZAxis), nameof(ViewportInfo.SetCameraDirection))
               from aimed in Refused.Unless(camera.SetCameraUp(pose.Frame.YAxis), nameof(ViewportInfo.SetCameraUp))
               from projected in pose.Lens.Switch(
                   (Camera: camera, Distance: pose.Frame.Origin.DistanceTo(pose.Target)),
                   parallel: static (state, parallel) =>
                       from changed in Refused.Unless(state.Camera.ChangeToParallelProjection(symmetricFrustum: true), nameof(ViewportInfo.ChangeToParallelProjection))
                       let width = parallel.Extent * Math.Max(1d, state.Camera.FrustumAspect)
                       let height = parallel.Extent * Math.Max(1d, 1d / state.Camera.FrustumAspect)
                       from framed in Refused.Unless(state.Camera.SetFrustum(-width, width, -height, height, state.Camera.FrustumNear, state.Camera.FrustumFar), nameof(ViewportInfo.SetFrustum))
                       select framed,
                   perspective: static (state, perspective) =>
                       Refused.Unless(state.Camera.ChangeToPerspectiveProjection(state.Distance, symmetricFrustum: true, state.Camera.Camera35mmLensLength), nameof(ViewportInfo.ChangeToPerspectiveProjection))
                           .Map(fun((Unit _) => { state.Camera.CameraAngle = perspective.Angle.Radians.ToDouble(); })),
                   twoPoint: static (state, twoPoint) =>
                       Refused.Unless(state.Camera.ChangeToTwoPointPerspectiveProjection(state.Distance, twoPoint.Up, state.Camera.Camera35mmLensLength), nameof(ViewportInfo.ChangeToTwoPointPerspectiveProjection))
                           .Map(fun((Unit _) => { state.Camera.CameraAngle = twoPoint.Angle.Radians.ToDouble(); })))
               from located in Refused.Unless(camera.SetCameraLocation(pose.Frame.Origin), nameof(ViewportInfo.SetCameraLocation))
                   .Map(fun((Unit _) => { camera.TargetPoint = pose.Target; }))
               select camera;
    }

    // --- [HOLD]
    public static IO<T> Within<T>(RhinoViewport viewport, Func<ViewportInfo, IO<Unit>> edit, IO<T> body) =>
        DisposalOps.Within(() => new ViewportInfo(viewport), held =>
            DisposalOps.Within(() => new ViewportInfo(viewport), camera => edit(camera).Bind(_ => IO.lift(() => Applied(viewport, camera))))
                .Bind(_ => DisposalOps.Within(IO.pure(held), _ => body, _ => IO.lift(() => Applied(viewport, held)))));

    private static Fin<Unit> Applied(RhinoViewport viewport, ViewportInfo camera) =>
        Refused.Unless(!viewport.LockedProjection, nameof(RhinoViewport.LockedProjection))
            .Bind(_ => Refused.Unless(viewport.SetViewProjection(camera, updateTargetLocation: false), nameof(RhinoViewport.SetViewProjection)))
            .Map(fun((Unit _) => viewport.SetCameraTarget(camera.TargetPoint, updateCameraLocation: false)));
}

using Rasm.Rhino.Document.Units;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct CameraAngle : System.Numerics.IMinMaxValue<CameraAngle> {
    public static CameraAngle MinValue { get; } = new(double.BitIncrement(0d));
    public static CameraAngle MaxValue { get; } = new(double.BitDecrement(0.5 * Math.PI * (1d - RhinoMath.SqrtEpsilon)));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct ParallelExtent : System.Numerics.IMinMaxValue<ParallelExtent> {
    public static ParallelExtent MinValue { get; } = new(double.BitIncrement(0d));
    public static ParallelExtent MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[Union]
public abstract partial record CameraLens {
    public sealed record Parallel(ParallelExtent Extent) : CameraLens;

    public sealed record Perspective(CameraAngle Angle) : CameraLens;

    public sealed record TwoPoint(CameraAngle Angle, Vector3d Up) : CameraLens;
}

public sealed record CameraPose(Plane Frame, Point3d Target, CameraLens Lens) {
    public bool Matches(CameraPose other, Tolerances tolerances) =>
        tolerances.Matches(Frame, other.Frame)
        && Target.DistanceTo(other.Target) <= tolerances.Absolute
        && Lens.Switch(
            (Other: other.Lens, Tolerances: tolerances),
            parallel: static (at, lens) => at.Other is CameraLens.Parallel held && Math.Abs((double)lens.Extent - (double)held.Extent) <= at.Tolerances.Absolute,
            perspective: static (at, lens) => at.Other is CameraLens.Perspective held && Math.Abs((double)lens.Angle - (double)held.Angle) <= at.Tolerances.Angle,
            twoPoint: static (at, lens) => at.Other is CameraLens.TwoPoint held
                && Math.Abs((double)lens.Angle - (double)held.Angle) <= at.Tolerances.Angle
                && Vector3d.VectorAngle(lens.Up, held.Up) <= at.Tolerances.Angle);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Cameras {
    // --- [POSE]
    public static IO<CameraPose> Read(ViewportInfo camera) =>
        IO.lift(() => (
                Refused.Unless(camera.GetCameraFrame(out Point3d location, out Vector3d x, out Vector3d y, out _), new Plane(location, x, y), nameof(ViewportInfo.GetCameraFrame)),
                camera.IsParallelProjection
                    ? Refused.Unless(camera.GetFrustum(out double left, out double right, out double bottom, out double top, out _, out _), Math.Min(Math.Max(right, -left), Math.Max(top, -bottom)), nameof(ViewportInfo.GetFrustum))
                        .Bind(ParallelExtent.Validated).Map<CameraLens>(static extent => extent)
                    : CameraAngle.Validated(camera.CameraAngle)
                        .Map<CameraLens>(angle => camera.IsTwoPointPerspectiveProjection ? new CameraLens.TwoPoint(angle, camera.CameraUp) : angle))
            .Apply((frame, lens) => new CameraPose(frame, camera.TargetPoint, lens)).As());

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
                       let width = (double)parallel.Extent * Math.Max(1d, state.Camera.FrustumAspect)
                       let height = (double)parallel.Extent * Math.Max(1d, 1d / state.Camera.FrustumAspect)
                       from framed in Refused.Unless(state.Camera.SetFrustum(-width, width, -height, height, state.Camera.FrustumNear, state.Camera.FrustumFar), nameof(ViewportInfo.SetFrustum))
                       select framed,
                   perspective: static (state, perspective) =>
                       Refused.Unless(state.Camera.ChangeToPerspectiveProjection(state.Distance, symmetricFrustum: true, state.Camera.Camera35mmLensLength), nameof(ViewportInfo.ChangeToPerspectiveProjection))
                           .Map(fun((Unit _) => { state.Camera.CameraAngle = (double)perspective.Angle; })),
                   twoPoint: static (state, twoPoint) =>
                       Refused.Unless(state.Camera.ChangeToTwoPointPerspectiveProjection(state.Distance, twoPoint.Up, state.Camera.Camera35mmLensLength), nameof(ViewportInfo.ChangeToTwoPointPerspectiveProjection))
                           .Map(fun((Unit _) => { state.Camera.CameraAngle = (double)twoPoint.Angle; })))
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

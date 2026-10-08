using Rasm.Imaging.Pixels;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct CameraAngle : System.Numerics.IMinMaxValue<CameraAngle> {
    public static CameraAngle MinValue { get; } = new(double.BitIncrement(0d));
    public static CameraAngle MaxValue { get; } = new(double.BitDecrement(0.5 * Math.PI * (1d - RhinoMath.SqrtEpsilon)));

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct ParallelExtent : System.Numerics.IMinMaxValue<ParallelExtent> {
    public static ParallelExtent MinValue { get; } = new(double.BitIncrement(0d));
    public static ParallelExtent MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union]
public abstract partial record CameraLens {
    public sealed record Parallel(ParallelExtent Extent) : CameraLens;

    public sealed record Perspective(CameraAngle Angle) : CameraLens;

    public sealed record TwoPoint(CameraAngle Angle, Vector3d Up) : CameraLens;
}

public sealed record CameraPose(Plane Frame, Point3d Target, CameraLens Lens);

// --- [SERVICES] ------------------------------------------------------------------------
public readonly record struct CameraSnapshot(ViewportInfo Camera, uint ChangeCounter) : IDisposable {
    public void Dispose() => Camera.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Cameras {
    // --- [POSE]
    public static IO<CameraPose> ReadPose(RhinoViewport viewport) =>
        IO.lift(() => (Refused.Unless(viewport.GetCameraFrame(out Plane frame), frame, nameof(RhinoViewport.GetCameraFrame)), Lens(viewport))
            .Apply((held, lens) => new CameraPose(held, viewport.CameraTarget, lens))
            .As());

    public static Fin<Unit> WritePose(RhinoViewport viewport, CameraPose pose) {
        using ViewportInfo copy = new(viewport);
        return Posed(copy, pose).Bind(posed => Restore(viewport, posed, pose.Target));
    }

    public static IO<TValue> WithPose<TValue>(RhinoViewport viewport, CameraPose pose, IO<TValue> body) =>
        use(Snapshot(viewport))
            .Bind(snapshot => IO.lift(() => WritePose(viewport, pose))
                .Bind(_ => body)
                .Finally(IO.lift(() => Restore(viewport, snapshot.Camera, snapshot.Camera.TargetPoint))))
            .Bracket();

    public static IO<ViewportInfo> ToViewportInfo(CameraPose pose, BoundingBox bounds) =>
        IO.lift(static () => new ViewportInfo()).Bind(camera => DisposalOps.OnFailure(
            IO.lift(() => Posed(camera, pose).Bind(posed => Refused.Unless(posed.SetFrustumNearFar(bounds), posed, nameof(ViewportInfo.SetFrustumNearFar)))),
            IO.lift(camera.Dispose)));

    private static Fin<CameraLens> Lens(RhinoViewport viewport) =>
        viewport.IsParallelProjection
            ? Refused.Unless(viewport.GetFrustum(out double left, out double right, out double bottom, out double top, out _, out _), Math.Min(Math.Max(right, -left), Math.Max(top, -bottom)), nameof(RhinoViewport.GetFrustum))
                .Bind(Conversions.Validated<ParallelExtent, double, InvalidRhinoValue>)
                .Map<CameraLens>(static extent => extent)
            : Refused.Unless(viewport.CameraAngle is var angle && angle > 0d, angle, nameof(RhinoViewport.CameraAngle))
                .Bind(Conversions.Validated<CameraAngle, double, InvalidRhinoValue>)
                .Map<CameraLens>(valid => viewport.IsTwoPointPerspectiveProjection ? new CameraLens.TwoPoint(valid, viewport.CameraUp) : valid);

    private static Fin<ViewportInfo> Posed(ViewportInfo camera, CameraPose pose) =>
        from half in (
                Refused.Unless(camera.ChangeToParallelProjection(symmetricFrustum: true), nameof(ViewportInfo.ChangeToParallelProjection)),
                Fin.Succ(ignore(camera.SetCameraDirection(-pose.Frame.ZAxis))),
                Refused.Unless(camera.SetCameraUp(pose.Frame.YAxis), nameof(ViewportInfo.SetCameraUp)),
                pose.Lens.Switch(
                    (Camera: camera, Distance: pose.Frame.Origin.DistanceTo(pose.Target)),
                    parallel: static (_, parallel) => Fin.Succ((double)parallel.Extent),
                    perspective: static (state, perspective) =>
                        Refused.Unless(
                            state.Camera.ChangeToPerspectiveProjection(state.Distance, symmetricFrustum: true, state.Camera.Camera35mmLensLength),
                            state.Camera.FrustumNear * Math.Tan(perspective.Angle),
                            nameof(ViewportInfo.ChangeToPerspectiveProjection)),
                    twoPoint: static (state, twoPoint) =>
                        Refused.Unless(
                            state.Camera.ChangeToTwoPointPerspectiveProjection(state.Distance, twoPoint.Up, state.Camera.Camera35mmLensLength),
                            state.Camera.FrustumNear * Math.Tan(twoPoint.Angle),
                            nameof(ViewportInfo.ChangeToTwoPointPerspectiveProjection))))
            .Apply(static (_, _, _, held) => held)
            .As()
        let width = half * Math.Max(1d, camera.FrustumAspect)
        let height = half * Math.Max(1d, 1d / camera.FrustumAspect)
        from framed in (
                Refused.Unless(camera.SetCameraLocation(pose.Frame.Origin), nameof(ViewportInfo.SetCameraLocation)),
                Refused.Unless(camera.SetFrustum(-width, width, -height, height, camera.FrustumNear, camera.FrustumFar), camera, nameof(ViewportInfo.SetFrustum)))
            .Apply(static (_, posed) => posed)
            .As()
        select framed;

    // --- [SNAPSHOTS]
    public static IO<CameraSnapshot> Snapshot(RhinoViewport viewport) =>
        IO.lift(() => new CameraSnapshot(new ViewportInfo(viewport), viewport.ChangeCounter)).Bind(static snapshot => DisposalOps.OnFailure(
            IO.lift(() => (
                    InvalidAnswer.Unless(snapshot.Camera.IsValidCamera, snapshot, nameof(ViewportInfo.IsValidCamera)),
                    InvalidAnswer.Unless(snapshot.Camera.IsValidFrustum, snapshot, nameof(ViewportInfo.IsValidFrustum)))
                .Apply(static (held, _) => held)
                .As()),
            IO.lift(snapshot.Dispose)));

    public static IO<bool> Moved(RhinoViewport viewport, CameraSnapshot snapshot) =>
        IO.lift(() => viewport.ChangeCounter != snapshot.ChangeCounter);

    public static Fin<Unit> Restore(RhinoViewport viewport, ViewportInfo camera, Point3d target) =>
        CameraLocked.Unless(!viewport.LockedProjection)
            .Bind(_ => Refused.Unless(viewport.SetViewProjection(camera, updateTargetLocation: false), nameof(RhinoViewport.SetViewProjection)))
            .Map(fun((Unit _) => viewport.SetCameraTarget(target, updateCameraLocation: false)));

    // --- [FRAME]
    public static Fin<Transform> GetXform(ViewportInfo camera, CoordinateSystem source, CoordinateSystem destination) =>
        camera.GetXform(source, destination) is { IsValid: true } xform ? xform : new Refused(nameof(ViewportInfo.GetXform));

    public static System.Numerics.Matrix4x4 ToMatrix(Transform xform) =>
        new(
            (float)xform.M00, (float)xform.M10, (float)xform.M20, (float)xform.M30,
            (float)xform.M01, (float)xform.M11, (float)xform.M21, (float)xform.M31,
            (float)xform.M02, (float)xform.M12, (float)xform.M22, (float)xform.M32,
            (float)xform.M03, (float)xform.M13, (float)xform.M23, (float)xform.M33);

    public static Fin<Option<LensFocus>> Focus(ViewInfo view, LengthUnit spaceUnit) =>
        view.FocalBlurMode == ViewInfoFocalBlurModes.Manual && !view.Viewport.IsParallelProjection
            ? from millimetres in Refused.Unless(view.Viewport.Camera35mmLensLength is var length && length > 0d, length, nameof(ViewportInfo.Camera35mmLensLength))
              from parts in (
                      Conversions.Validated<FocalLength, float, InvalidPixelValue>((float)(millimetres * LengthUnit.Scale(LengthUnit.Millimeters, LengthUnit.Meters))),
                      Conversions.Validated<FocusDistance, float, InvalidPixelValue>((float)(view.FocalBlurDistance * LengthUnit.Scale(spaceUnit, LengthUnit.Meters))))
                  .Apply(static (focal, focus) => (Focal: focal, Focus: focus))
                  .As()
              from lens in LensFocus.Validate(parts.Focal, parts.Focus, out LensFocus held) is { } invalid ? Fin.Fail<LensFocus>(invalid) : Fin.Succ(held)
              select Some(lens)
            : Fin.Succ(Option<LensFocus>.None);

    public static Fin<Camera> ToCamera(ViewInfo view, LengthUnit spaceUnit, PixelExtent extent, Option<LensFocus> lens) =>
        (LengthUnit.Scale(spaceUnit, LengthUnit.Meters), view.Viewport.IsParallelProjection) switch {
            var (metres, parallel) => (
                    GetXform(view.Viewport, CoordinateSystem.Camera, CoordinateSystem.World).Map(ToMatrix),
                    Refused.Unless(view.Viewport.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out _), parallel ? metres : 1d / near, nameof(ViewportInfo.GetFrustum))
                        .Bind(scale => ViewWindow.Validate((float)(left * scale), (float)(right * scale), (float)(bottom * scale), (float)(top * scale), out ViewWindow held) is { } invalid
                            ? Fin.Fail<ViewWindow>(invalid)
                            : Fin.Succ(held)))
                .Apply((pose, window) => new Camera(
                    pose with { Translation = pose.Translation * (float)metres },
                    parallel ? new Frustum.Parallel(window) : new Frustum.Perspective(window),
                    extent,
                    lens))
                .As(),
        };

    public static IO<SceneLights> ToLights(RhinoDoc doc) =>
        IO.lift(() => (float)LengthUnit.Scale(doc.ModelUnits, LengthUnit.Meters) switch {
            var metres => new SceneLights(
                doc.Lights.Sun is { Enabled: true } sun ? Some(-new System.Numerics.Vector3((float)sun.Vector.X, (float)sun.Vector.Y, (float)sun.Vector.Z)) : None,
                toHashMap(doc.Lights.AsIterable()
                    .Filter(static light => !light.IsDeleted && light.LightGeometry.IsEnabled)
                    .Map(light => (light.Id, new System.Numerics.Vector3((float)light.LightGeometry.Location.X, (float)light.LightGeometry.Location.Y, (float)light.LightGeometry.Location.Z) * metres)))),
        });
}

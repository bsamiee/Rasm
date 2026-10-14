using System.Runtime.InteropServices;
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

public readonly record struct CameraSnapshot(ViewportInfo Camera, uint ChangeCounter);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Cameras {
    // --- [POSE]
    public static IO<CameraPose> ReadPose(RhinoViewport viewport) =>
        IO.lift(() => (
                Refused.Unless(viewport.GetCameraFrame(out Plane read), read, nameof(RhinoViewport.GetCameraFrame)),
                viewport.IsParallelProjection
                    ? Refused.Unless(viewport.GetFrustum(out double left, out double right, out double bottom, out double top, out _, out _), Math.Min(Math.Max(right, -left), Math.Max(top, -bottom)), nameof(RhinoViewport.GetFrustum))
                        .Bind(Conversions.Validated<ParallelExtent, double, InvalidRhinoValue>).Map<CameraLens>(static extent => extent)
                    : Conversions.Validated<CameraAngle, double, InvalidRhinoValue>(viewport.CameraAngle)
                        .Map<CameraLens>(angle => viewport.IsTwoPointPerspectiveProjection ? new CameraLens.TwoPoint(angle, viewport.CameraUp) : angle))
            .Apply((frame, lens) => new CameraPose(frame, viewport.CameraTarget, lens)).As());

    public static Fin<Unit> WritePose(RhinoViewport viewport, CameraPose pose) {
        using ViewportInfo copy = new(viewport);
        return Posed(copy, pose).Bind(posed => Restore(viewport, posed, pose.Target));
    }

    public static IO<TValue> WithPose<TValue>(RhinoViewport viewport, CameraPose pose, IO<TValue> body) =>
        DisposalOps.Within<IDisposable, TValue>(retain => Snapshot(viewport, retain).Bind(snapshot => DisposalOps.Within(
            () => snapshot,
            _ => IO.lift(() => WritePose(viewport, pose)).Bind(_ => body),
            held => IO.lift(() => Restore(viewport, held.Camera, held.Camera.TargetPoint)))), DisposalOps.Release);

    public static IO<ViewportInfo> ToViewportInfo(CameraPose pose, BoundingBox bounds, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(static () => new ViewportInfo()), static camera => IO.lift(camera.Dispose), retain)
            .Bind(camera => IO.lift(() => Posed(camera, pose).Bind(posed => Refused.Unless(posed.SetFrustumNearFar(bounds), posed, nameof(ViewportInfo.SetFrustumNearFar)))));

    private static Fin<ViewportInfo> Posed(ViewportInfo camera, CameraPose pose) =>
        from parallel in Refused.Unless(camera.ChangeToParallelProjection(symmetricFrustum: true), nameof(ViewportInfo.ChangeToParallelProjection))
        from directed in Refused.Unless(camera.SetCameraDirection(-pose.Frame.ZAxis), nameof(ViewportInfo.SetCameraDirection))
        from aimed in Refused.Unless(camera.SetCameraUp(pose.Frame.YAxis), nameof(ViewportInfo.SetCameraUp))
        from half in pose.Lens.Switch(
            (Camera: camera, Distance: pose.Frame.Origin.DistanceTo(pose.Target), Length: camera.Camera35mmLensLength),
            parallel: static (_, parallel) => Fin.Succ((double)parallel.Extent),
            perspective: static (state, perspective) => Refused.Unless(state.Camera.ChangeToPerspectiveProjection(state.Distance, symmetricFrustum: true, state.Length), state.Camera.FrustumNear * Math.Tan(perspective.Angle), nameof(ViewportInfo.ChangeToPerspectiveProjection)),
            twoPoint: static (state, twoPoint) => Refused.Unless(state.Camera.ChangeToTwoPointPerspectiveProjection(state.Distance, twoPoint.Up, state.Length), state.Camera.FrustumNear * Math.Tan(twoPoint.Angle), nameof(ViewportInfo.ChangeToTwoPointPerspectiveProjection)))
        from located in Refused.Unless(camera.SetCameraLocation(pose.Frame.Origin), nameof(ViewportInfo.SetCameraLocation))
        let width = half * Math.Max(1d, camera.FrustumAspect)
        let height = half * Math.Max(1d, 1d / camera.FrustumAspect)
        from framed in Refused.Unless(camera.SetFrustum(-width, width, -height, height, camera.FrustumNear, camera.FrustumFar), camera, nameof(ViewportInfo.SetFrustum))
        select framed;

    // --- [SNAPSHOTS]
    public static IO<CameraSnapshot> Snapshot(RhinoViewport viewport, Action<IDisposable> retain) =>
        DisposalOps.Acquire(IO.lift(() => new ViewportInfo(viewport)), static camera => IO.lift(camera.Dispose), retain)
            .Bind(camera => IO.lift(() => (
                InvalidAnswer.Unless(camera.IsValidCamera, camera, nameof(camera)),
                InvalidAnswer.Unless(camera.IsValidFrustum, camera, nameof(camera)))
                .Apply((held, _) => new CameraSnapshot(held, viewport.ChangeCounter)).As()));

    public static Fin<Unit> Restore(RhinoViewport viewport, ViewportInfo camera, Point3d target) =>
        guard<Error>(!viewport.LockedProjection, new CameraLocked()).ToFin()
            .Bind(_ => Refused.Unless(viewport.SetViewProjection(camera, updateTargetLocation: false), nameof(RhinoViewport.SetViewProjection)))
            .Map(fun((Unit _) => viewport.SetCameraTarget(target, updateCameraLocation: false)));

    // --- [FRAME]
    public static Fin<System.Numerics.Matrix4x4> ToMatrix(Transform xform) =>
        xform.ToFloatArray(rowDominant: false) switch {
            var entries => entries.All(float.IsFinite)
                ? MemoryMarshal.Cast<float, System.Numerics.Matrix4x4>(entries.AsSpan())[0]
                : new InvalidRhinoValue(),
        };

    public static Fin<System.Numerics.Vector3> Numeric(Point3d point) =>
        Seq(point.X, point.Y, point.Z).ForAll(static value => value is >= float.MinValue and <= float.MaxValue)
            ? new System.Numerics.Vector3((float)point.X, (float)point.Y, (float)point.Z)
            : new InvalidRhinoValue();

    public static Fin<System.Numerics.Vector3> Numeric(Vector3d vector) =>
        Seq(vector.X, vector.Y, vector.Z).ForAll(static value => value is >= float.MinValue and <= float.MaxValue)
            ? new System.Numerics.Vector3((float)vector.X, (float)vector.Y, (float)vector.Z)
            : new InvalidRhinoValue();

    public static Fin<Option<LensFocus>> Focus(ViewInfo view, LengthUnit spaceUnit, Option<FocalLength> focal, Option<FNumber> aperture) =>
        aperture.IsSome
            ? focal.Traverse(length => Conversions.Validated<FocusDistance, float, InvalidPixelValue>((float)(view.FocalBlurDistance * LengthUnit.Scale(spaceUnit, LengthUnit.Meters)))
                .Bind(focus => LensFocus.Validate(length, focus, out LensFocus held) is { } invalid ? Fin.Fail<LensFocus>(invalid) : Fin.Succ(held))).As()
            : Fin.Succ(Option<LensFocus>.None);

    public static Fin<Camera> ToCamera(ViewInfo view, LengthUnit spaceUnit, PixelExtent extent, Option<LensFocus> lens) =>
        (LengthUnit.Scale(spaceUnit, LengthUnit.Meters), view.Viewport.IsParallelProjection, view.Viewport.GetXform(CoordinateSystem.Camera, CoordinateSystem.World)) switch {
            var (metres, parallel, xform) => (
                    Refused.Unless(xform.IsValid, xform, nameof(ViewportInfo.GetXform))
                        .Bind(frame => ToMatrix(frame with { M03 = frame.M03 * metres, M13 = frame.M13 * metres, M23 = frame.M23 * metres })),
                    Refused.Unless(view.Viewport.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out _),
                            (Left: parallel ? left * metres : left / near, Right: parallel ? right * metres : right / near,
                                Bottom: parallel ? bottom * metres : bottom / near, Top: parallel ? top * metres : top / near), nameof(ViewportInfo.GetFrustum))
                        .Bind(static window => ViewWindow.Validate((window.Left / 2d) + (window.Right / 2d), (window.Bottom / 2d) + (window.Top / 2d), window.Right - window.Left, window.Top - window.Bottom, out ViewWindow held) is { } invalid ? Fin.Fail<ViewWindow>(invalid) : Fin.Succ(held)))
                .Apply((pose, window) => new Camera(pose, parallel ? new Frustum.Parallel(window) : new Frustum.Perspective(window), extent, lens))
                .As(),
        };

    public static IO<SceneLights> ToLights(RhinoDoc doc) =>
        IO.lift(Fin<SceneLights> () => (LengthUnit.Scale(doc.ModelUnits, LengthUnit.Meters),
                Conversions.Rows(doc.Lights).Filter(static light => !light.IsDeleted && light.LightGeometry.IsEnabled).Partition(static light => light.LightGeometry.IsDirectionalLight)) switch {
                    var (metres, (directional, placed)) => (
                        (doc.Lights.Sun is { Enabled: true } sun ? Some(-sun.Vector) : None).Traverse(Numeric).As(),
                        (placed.Map(static light => (light.Id, light.LightGeometry.Location))
                                + Conversions.Rows(doc.Objects.GetObjectsByType<PointObject>()).Map(static point => (point.Id, point.PointGeometry.Location))
                                + Conversions.Rows(doc.Objects.GetObjectsByType<InstanceObject>()).Map(static block => (block.Id, Location: block.InsertionPoint)))
                            .Traverse(row => Numeric(row.Location * metres).Map(position => (row.Id, position))).As(),
                        directional.Traverse(static light => Numeric(-light.LightGeometry.Direction).Map(direction => (light.Id, direction))).As())
                        .Apply(static (sun, points, directions) => new SceneLights(sun, toHashMap(points), toHashMap(directions))).As(),
                });
}

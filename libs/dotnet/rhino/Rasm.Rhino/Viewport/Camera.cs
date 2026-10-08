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

// --- [SERVICES] ------------------------------------------------------------------------
public readonly record struct CameraSnapshot(ViewportInfo Camera, uint ChangeCounter) : IDisposable {
    public void Dispose() => Camera.Dispose();
}

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
        (from snapshot in use(Snapshot(viewport))
         from value in IO.lift(() => WritePose(viewport, pose)).Bind(_ => body).Finally(IO.lift(() => Restore(viewport, snapshot.Camera, snapshot.Camera.TargetPoint)))
         select value).Bracket();

    public static IO<ViewportInfo> ToViewportInfo(CameraPose pose, BoundingBox bounds) =>
        IO.lift(static () => new ViewportInfo()).Bind(camera => DisposalOps.OnFailure(
            IO.lift(() => Posed(camera, pose).Bind(posed => Refused.Unless(posed.SetFrustumNearFar(bounds), posed, nameof(ViewportInfo.SetFrustumNearFar)))),
            IO.lift(camera.Dispose)));

    private static Fin<ViewportInfo> Posed(ViewportInfo camera, CameraPose pose) =>
        from parallel in Refused.Unless(camera.ChangeToParallelProjection(symmetricFrustum: true), nameof(ViewportInfo.ChangeToParallelProjection))
        from aimed in Refused.Unless((camera.SetCameraDirection(-pose.Frame.ZAxis), camera.SetCameraUp(pose.Frame.YAxis)) is (_, true), nameof(ViewportInfo.SetCameraUp))
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
    public static IO<CameraSnapshot> Snapshot(RhinoViewport viewport) =>
        IO.lift(() => new CameraSnapshot(new ViewportInfo(viewport), viewport.ChangeCounter)).Bind(static snapshot => DisposalOps.OnFailure(
            IO.lift(() => (InvalidAnswer.Unless(snapshot.Camera.IsValidCamera, snapshot, nameof(ViewportInfo.IsValidCamera)), InvalidAnswer.Unless(snapshot.Camera.IsValidFrustum, snapshot, nameof(ViewportInfo.IsValidFrustum))).Apply(static (held, _) => held).As()),
            IO.lift(snapshot.Dispose)));

    public static Fin<Unit> Restore(RhinoViewport viewport, ViewportInfo camera, Point3d target) =>
        guard<Error>(!viewport.LockedProjection, new CameraLocked()).ToFin()
            .Bind(_ => Refused.Unless(viewport.SetViewProjection(camera, updateTargetLocation: false), nameof(RhinoViewport.SetViewProjection)))
            .Map(fun((Unit _) => viewport.SetCameraTarget(target, updateCameraLocation: false)));

    // --- [FRAME]
    public static System.Numerics.Matrix4x4 ToMatrix(Transform xform) =>
        MemoryMarshal.Cast<float, System.Numerics.Matrix4x4>(xform.ToFloatArray(rowDominant: false).AsSpan())[0];

    public static Fin<Option<LensFocus>> Focus(ViewInfo view, LengthUnit spaceUnit) =>
        view.FocalBlurMode == ViewInfoFocalBlurModes.Manual && !view.Viewport.IsParallelProjection
            ? from millimetres in Conversions.Validated<LensLength, double, InvalidRhinoValue>(view.Viewport.Camera35mmLensLength)
              from lens in (
                      Conversions.Validated<FocalLength, float, InvalidPixelValue>((float)(millimetres * LengthUnit.Scale(LengthUnit.Millimeters, LengthUnit.Meters))),
                      Conversions.Validated<FocusDistance, float, InvalidPixelValue>((float)(view.FocalBlurDistance * LengthUnit.Scale(spaceUnit, LengthUnit.Meters))))
                  .Apply(static (focal, focus) => LensFocus.Validate(focal, focus, out LensFocus held) is { } invalid ? Fin.Fail<LensFocus>(invalid) : Fin.Succ(held))
                  .As().Flatten()
              select Some(lens)
            : Fin.Succ(Option<LensFocus>.None);

    public static Fin<Camera> ToCamera(ViewInfo view, LengthUnit spaceUnit, PixelExtent extent, Option<LensFocus> lens) =>
        (LengthUnit.Scale(spaceUnit, LengthUnit.Meters), view.Viewport.IsParallelProjection, view.Viewport.GetXform(CoordinateSystem.Camera, CoordinateSystem.World)) switch {
            var (metres, parallel, xform) => (
                    Refused.Unless(xform.IsValid, xform, nameof(ViewportInfo.GetXform)).Map(ToMatrix),
                    Refused.Unless(view.Viewport.GetFrustum(out double left, out double right, out double bottom, out double top, out double near, out _), parallel ? metres : 1d / near, nameof(ViewportInfo.GetFrustum))
                        .Bind(scale => ViewWindow.Validate((float)(left * scale), (float)(right * scale), (float)(bottom * scale), (float)(top * scale), out ViewWindow held) is { } invalid ? Fin.Fail<ViewWindow>(invalid) : Fin.Succ(held)))
                .Apply((pose, window) => new Camera(pose with { Translation = pose.Translation * (float)metres }, parallel ? new Frustum.Parallel(window) : new Frustum.Perspective(window), extent, lens))
                .As(),
        };

    public static IO<SceneLights> ToLights(RhinoDoc doc) =>
        IO.lift(() => ((float)LengthUnit.Scale(doc.ModelUnits, LengthUnit.Meters),
                Conversions.Rows(doc.Lights).Filter(static light => !light.IsDeleted && light.LightGeometry.IsEnabled).Partition(static light => light.LightGeometry.IsDirectionalLight)) switch {
                    var (metres, (directional, placed)) => new SceneLights(
                        doc.Lights.Sun is { Enabled: true } sun ? Some(-Numeric(new Point3d(sun.Vector))) : None,
                        toHashMap((placed.Map(static light => (light.Id, light.LightGeometry.Location))
                                + Conversions.Rows(doc.Objects.GetObjectsByType<PointObject>()).Map(static point => (point.Id, point.PointGeometry.Location))
                                + Conversions.Rows(doc.Objects.GetObjectsByType<InstanceObject>()).Map(static block => (block.Id, Location: block.InsertionPoint)))
                            .Map(row => (row.Id, Numeric(row.Location) * metres))),
                        toHashMap(directional.Map(static light => (light.Id, -Numeric(new Point3d(light.LightGeometry.Direction)))))),
                });

    private static System.Numerics.Vector3 Numeric(Point3d point) => new((float)point.X, (float)point.Y, (float)point.Z);
}

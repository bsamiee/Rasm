using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(AllowDefaultStructs = true, DefaultInstancePropertyName = "Zero", SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct FramingMargin : System.Numerics.IMinMaxValue<FramingMargin> {
    public static FramingMargin MinValue { get; } = new(0d);
    public static FramingMargin MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct Magnification : System.Numerics.IMinMaxValue<Magnification> {
    public static Magnification MinValue { get; } = new(double.BitIncrement(0d));
    public static Magnification MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct LensLength : System.Numerics.IMinMaxValue<LensLength> {
    public static LensLength MinValue { get; } = new(double.BitIncrement(0d));
    public static LensLength MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct TargetDistance : System.Numerics.IMinMaxValue<TargetDistance> {
    public static TargetDistance MinValue { get; } = new(double.BitIncrement(0d));
    public static TargetDistance MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record KeyGesture {
    public sealed record Rotate(bool LeftRight, double AngleRadians) : KeyGesture;

    public sealed record Dolly(bool LeftRight, double Amount) : KeyGesture;

    public sealed record DollyInOut(double Amount) : KeyGesture;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DragGesture {
    public sealed record RotateAroundTarget() : DragGesture;

    public sealed record RotateCamera() : DragGesture;

    public sealed record InOutDolly() : DragGesture;

    public sealed record Magnify() : DragGesture;

    public sealed record Tilt() : DragGesture;

    public sealed record DollyZoom() : DragGesture;

    public sealed record LateralDolly() : DragGesture;

    public sealed record AdjustLens(bool MoveTarget) : DragGesture;
}

[Union]
public abstract partial record ZoomTarget {
    public sealed record ToBox(BoundingBox Subject, FramingMargin Margin) : ZoomTarget;

    public sealed record ToWindow(Rectangle Client) : ZoomTarget;

    public sealed record ByFactor(Magnification Factor, bool LensZoom, Option<System.Drawing.Point> FixedPoint) : ZoomTarget;

    public sealed record ToExtents() : ZoomTarget;

    public sealed record ToSelected() : ZoomTarget;
}

[Union]
public abstract partial record ProjectionChange {
    public sealed record ToParallel(bool Symmetric) : ProjectionChange;

    public sealed record ToPerspective(Option<TargetDistance> Distance, bool Symmetric, LensLength Length) : ProjectionChange;

    public sealed record ToTwoPoint(Option<(TargetDistance Distance, Vector3d Up)> Placement, LensLength Length) : ProjectionChange;

    public sealed record ToReflected() : ProjectionChange;

    public sealed record ToPlan(Option<Plane> Frame, bool SetConstructionPlane) : ProjectionChange;

    public sealed record Defined(DefinedViewportProjection Projection, Option<string> ViewName, bool UpdateConstructionPlane) : ProjectionChange;

    public sealed record Isometric(IsometricCamera Camera, Option<string> ViewName, bool UpdateConstructionPlane) : ProjectionChange;

    public sealed record Lens(LensLength Length) : ProjectionChange;

    public sealed record LockedProjection(bool Locked) : ProjectionChange;
}

[SmartEnum]
public sealed partial class StackStep {
    public static readonly StackStep PreviousViewProjection = new(static viewport => viewport.PreviousViewProjection());
    public static readonly StackStep NextViewProjection = new(static viewport => viewport.NextViewProjection());
    public static readonly StackStep PopConstructionPlane = new(static viewport => viewport.PopConstructionPlane());
    public static readonly StackStep PreviousConstructionPlane = new(static viewport => viewport.PreviousConstructionPlane());
    public static readonly StackStep NextConstructionPlane = new(static viewport => viewport.NextConstructionPlane());

    [UseDelegateFromConstructor]
    public partial bool Step(RhinoViewport viewport);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Navigation {
    // --- [GESTURES]
    public static Fin<Unit> ApplyKey(RhinoViewport viewport, KeyGesture gesture) =>
        gesture.Switch(
            viewport,
            rotate: static (port, rotate) => Refused.Unless(port.KeyboardRotate(rotate.LeftRight, rotate.AngleRadians), nameof(RhinoViewport.KeyboardRotate)),
            dolly: static (port, dolly) => Refused.Unless(port.KeyboardDolly(dolly.LeftRight, dolly.Amount), nameof(RhinoViewport.KeyboardDolly)),
            dollyInOut: static (port, dolly) => Refused.Unless(port.KeyboardDollyInOut(dolly.Amount), nameof(RhinoViewport.KeyboardDollyInOut)));

    public static Fin<Unit> ApplyDrag(RhinoViewport viewport, DragGesture gesture, System.Drawing.Point from, System.Drawing.Point to) =>
        gesture.Switch(
            (Port: viewport, From: from, To: to),
            rotateAroundTarget: static (drag, _) => Refused.Unless(drag.Port.MouseRotateAroundTarget(drag.From, drag.To), nameof(RhinoViewport.MouseRotateAroundTarget)),
            rotateCamera: static (drag, _) => Refused.Unless(drag.Port.MouseRotateCamera(drag.From, drag.To), nameof(RhinoViewport.MouseRotateCamera)),
            inOutDolly: static (drag, _) => Refused.Unless(drag.Port.MouseInOutDolly(drag.From, drag.To), nameof(RhinoViewport.MouseInOutDolly)),
            magnify: static (drag, _) => Refused.Unless(drag.Port.MouseMagnify(drag.From, drag.To), nameof(RhinoViewport.MouseMagnify)),
            tilt: static (drag, _) => Refused.Unless(drag.Port.MouseTilt(drag.From, drag.To), nameof(RhinoViewport.MouseTilt)),
            dollyZoom: static (drag, _) => Refused.Unless(drag.Port.MouseDollyZoom(drag.From, drag.To), nameof(RhinoViewport.MouseDollyZoom)),
            lateralDolly: static (drag, _) => Refused.Unless(drag.Port.MouseLateralDolly(drag.From, drag.To), nameof(RhinoViewport.MouseLateralDolly)),
            adjustLens: static (drag, adjust) => Refused.Unless(drag.Port.MouseAdjustLensLength(drag.From, drag.To, adjust.MoveTarget), nameof(RhinoViewport.MouseAdjustLensLength)));

    // --- [FRAMING]
    public static Fin<Unit> Zoom(RhinoViewport viewport, ZoomTarget target) =>
        target.Switch(
            viewport,
            toBox: static (port, box) => {
                BoundingBox framed = box.Subject;
                framed.Inflate(box.Subject.Diagonal.Length * box.Margin);
                return Refused.Unless(port.ZoomBoundingBox(framed), nameof(RhinoViewport.ZoomBoundingBox));
            },
            toWindow: static (port, window) => Refused.Unless(port.ZoomWindow(window.Client), nameof(RhinoViewport.ZoomWindow)),
            byFactor: static (port, factor) => Refused.Unless(
                factor.FixedPoint.Match(
                    Some: at => port.Magnify(factor.Factor, factor.LensZoom, at),
                    None: () => port.Magnify(factor.Factor, factor.LensZoom)),
                nameof(RhinoViewport.Magnify)),
            toExtents: static (port, _) => Refused.Unless(port.ZoomExtents(), nameof(RhinoViewport.ZoomExtents)),
            toSelected: static (port, _) => Refused.Unless(port.ZoomExtentsSelected(), nameof(RhinoViewport.ZoomExtentsSelected)));

    // --- [PROJECTIONS]
    public static Fin<Unit> ChangeProjection(RhinoViewport viewport, ProjectionChange change) =>
        change.Switch(
            viewport,
            toParallel: static (port, parallel) =>
                Refused.Unless(port.ChangeToParallelProjection(parallel.Symmetric), nameof(RhinoViewport.ChangeToParallelProjection)),
            toPerspective: static (port, perspective) => Refused.Unless(
                perspective.Distance.Match(
                    Some: distance => port.ChangeToPerspectiveProjection(distance, perspective.Symmetric, perspective.Length),
                    None: () => port.ChangeToPerspectiveProjection(perspective.Symmetric, perspective.Length)),
                nameof(RhinoViewport.ChangeToPerspectiveProjection)),
            toTwoPoint: static (port, twoPoint) => Refused.Unless(
                twoPoint.Placement.Match(
                    Some: placed => port.ChangeToTwoPointPerspectiveProjection(placed.Distance, placed.Up, twoPoint.Length),
                    None: () => port.ChangeToTwoPointPerspectiveProjection(twoPoint.Length)),
                nameof(RhinoViewport.ChangeToTwoPointPerspectiveProjection)),
            toReflected: static (port, _) =>
                Refused.Unless(port.ChangeToParallelReflectedProjection(), nameof(RhinoViewport.ChangeToParallelReflectedProjection)),
            toPlan: static (port, plan) => {
                Plane frame = plan.Frame.IfNone(port.ConstructionPlane);
                return Refused.Unless(port.SetToPlanView(frame.Origin, frame.XAxis, frame.YAxis, plan.SetConstructionPlane), nameof(RhinoViewport.SetToPlanView));
            },
            defined: static (port, defined) => Refused.Unless(
                port.SetProjection(defined.Projection, Conversions.Unset(defined.ViewName), defined.UpdateConstructionPlane),
                nameof(RhinoViewport.SetProjection)),
            isometric: static (port, isometric) => Refused.Unless(
                port.SetProjection(isometric.Camera, Conversions.Unset(isometric.ViewName), isometric.UpdateConstructionPlane),
                nameof(RhinoViewport.SetProjection)),
            lens: static (port, lens) => {
                port.Camera35mmLensLength = lens.Length;
                return Fin.Succ(unit);
            },
            lockedProjection: static (port, locked) => {
                port.LockedProjection = locked.Locked;
                return Fin.Succ(unit);
            });

    // --- [VIEWPORTS]
    public static IO<Seq<bool>> ApplyToViewports(RhinoDoc doc, ViewportSet viewports, Func<RhinoViewport, Fin<Unit>> edit, RedrawPolicy redraw) =>
        Commits.WithinRedraw(
            doc,
            redraw,
            Viewports.ResolveViewports(doc, viewports).Bracket(
                Use: rows => rows.TraverseM(row => Edit(row, edit)).As(),
                Fin: DisposalOps.Release));

    private static IO<bool> Edit(ViewportRef row, Func<RhinoViewport, Fin<Unit>> edit) =>
        from before in IO.lift(() => row.Viewport.ChangeCounter)
        from edited in IO.lift(() => edit(row.Viewport))
        from after in IO.lift(() => row.Viewport.ChangeCounter)
        from committed in IO.lift(row.CommitViewportChanges)
        select after != before;
}

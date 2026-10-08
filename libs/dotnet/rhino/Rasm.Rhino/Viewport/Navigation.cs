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

[SmartEnum]
public sealed partial class KeyGesture {
    public static readonly KeyGesture RotateLeftRight = new(static (port, amount) => Refused.Unless(port.KeyboardRotate(leftRight: true, amount), nameof(RhinoViewport.KeyboardRotate)));
    public static readonly KeyGesture RotateUpDown = new(static (port, amount) => Refused.Unless(port.KeyboardRotate(leftRight: false, amount), nameof(RhinoViewport.KeyboardRotate)));
    public static readonly KeyGesture DollyLeftRight = new(static (port, amount) => Refused.Unless(port.KeyboardDolly(leftRight: true, amount), nameof(RhinoViewport.KeyboardDolly)));
    public static readonly KeyGesture DollyUpDown = new(static (port, amount) => Refused.Unless(port.KeyboardDolly(leftRight: false, amount), nameof(RhinoViewport.KeyboardDolly)));
    public static readonly KeyGesture DollyInOut = new(static (port, amount) => Refused.Unless(port.KeyboardDollyInOut(amount), nameof(RhinoViewport.KeyboardDollyInOut)));

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Press(RhinoViewport viewport, double amount);
}

[SmartEnum]
public sealed partial class DragGesture {
    public static readonly DragGesture RotateAroundTarget = new(static (port, previous, current) => Refused.Unless(port.MouseRotateAroundTarget(previous, current), nameof(RhinoViewport.MouseRotateAroundTarget)));
    public static readonly DragGesture RotateCamera = new(static (port, previous, current) => Refused.Unless(port.MouseRotateCamera(previous, current), nameof(RhinoViewport.MouseRotateCamera)));
    public static readonly DragGesture InOutDolly = new(static (port, previous, current) => Refused.Unless(port.MouseInOutDolly(previous, current), nameof(RhinoViewport.MouseInOutDolly)));
    public static readonly DragGesture Magnify = new(static (port, previous, current) => Refused.Unless(port.MouseMagnify(previous, current), nameof(RhinoViewport.MouseMagnify)));
    public static readonly DragGesture Tilt = new(static (port, previous, current) => Refused.Unless(port.MouseTilt(previous, current), nameof(RhinoViewport.MouseTilt)));
    public static readonly DragGesture DollyZoom = new(static (port, previous, current) => Refused.Unless(port.MouseDollyZoom(previous, current), nameof(RhinoViewport.MouseDollyZoom)));
    public static readonly DragGesture LateralDolly = new(static (port, previous, current) => Refused.Unless(port.MouseLateralDolly(previous, current), nameof(RhinoViewport.MouseLateralDolly)));
    public static readonly DragGesture AdjustLensLength = new(static (port, previous, current) => Refused.Unless(port.MouseAdjustLensLength(previous, current, moveTarget: false), nameof(RhinoViewport.MouseAdjustLensLength)));
    public static readonly DragGesture AdjustLensLengthMovingTarget = new(static (port, previous, current) => Refused.Unless(port.MouseAdjustLensLength(previous, current, moveTarget: true), nameof(RhinoViewport.MouseAdjustLensLength)));

    [UseDelegateFromConstructor]
    public partial Fin<Unit> Drag(RhinoViewport viewport, System.Drawing.Point mousePreviousPoint, System.Drawing.Point mouseCurrentPoint);
}

[Union]
public abstract partial record ZoomTarget {
    public sealed record ToBox(BoundingBox Subject, FramingMargin Margin) : ZoomTarget;

    public sealed record ToWindow(System.Drawing.Rectangle Client) : ZoomTarget;

    public sealed record ByFactor(Magnification Factor, bool LensZoom, Option<System.Drawing.Point> FixedPoint) : ZoomTarget;

    public sealed record ToExtents() : ZoomTarget;

    public sealed record ToSelected() : ZoomTarget;
}

[Union]
public abstract partial record ProjectionChange {
    public sealed record ToParallel(bool Symmetric) : ProjectionChange;

    public sealed record ToPerspective(Option<TargetDistance> Distance, bool Symmetric, LensLength Length) : ProjectionChange;

    public sealed record ToTwoPoint(Option<TargetDistance> Distance, Option<Vector3d> Up, LensLength Length) : ProjectionChange;

    public sealed record ToReflected() : ProjectionChange;

    public sealed record ToPlan(Option<Plane> Frame, bool SetConstructionPlane) : ProjectionChange;

    public sealed record Defined(DefinedViewportProjection Projection, Option<string> ViewName, bool UpdateConstructionPlane) : ProjectionChange;

    public sealed record Isometric(IsometricCamera Camera, Option<string> ViewName, bool UpdateConstructionPlane) : ProjectionChange;

    public sealed record Lens(LensLength Length) : ProjectionChange;
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
    // --- [FRAMING]
    public static Fin<Unit> Zoom(RhinoViewport viewport, ZoomTarget target) =>
        target.Switch(
            viewport,
            toBox: static (port, box) => {
                BoundingBox framed = box.Subject;
                framed.Inflate(framed.Diagonal.Length * box.Margin);
                return Refused.Unless(port.ZoomBoundingBox(framed), nameof(RhinoViewport.ZoomBoundingBox));
            },
            toWindow: static (port, window) => Refused.Unless(port.ZoomWindow(window.Client), nameof(RhinoViewport.ZoomWindow)),
            byFactor: static (port, factor) => Refused.Unless(
                factor.FixedPoint.Match(Some: at => port.Magnify(factor.Factor, factor.LensZoom, at), None: () => port.Magnify(factor.Factor, factor.LensZoom)),
                nameof(RhinoViewport.Magnify)),
            toExtents: static (port, _) => Refused.Unless(port.ZoomExtents(), nameof(RhinoViewport.ZoomExtents)),
            toSelected: static (port, _) => Refused.Unless(port.ZoomExtentsSelected(), nameof(RhinoViewport.ZoomExtentsSelected)));

    // --- [PROJECTIONS]
    public static Fin<Unit> ChangeProjection(RhinoViewport viewport, ProjectionChange change) =>
        change.Switch(
            viewport,
            toParallel: static (port, parallel) => Refused.Unless(port.ChangeToParallelProjection(parallel.Symmetric), nameof(RhinoViewport.ChangeToParallelProjection)),
            toPerspective: static (port, perspective) => Refused.Unless(
                port.ChangeToPerspectiveProjection(Conversions.Unset(perspective.Distance.Map<double>(static distance => distance)), perspective.Symmetric, perspective.Length),
                nameof(RhinoViewport.ChangeToPerspectiveProjection)),
            toTwoPoint: static (port, twoPoint) => Refused.Unless(
                port.ChangeToTwoPointPerspectiveProjection(Conversions.Unset(twoPoint.Distance.Map<double>(static distance => distance)), twoPoint.Up.IfNone(Vector3d.Zero), twoPoint.Length),
                nameof(RhinoViewport.ChangeToTwoPointPerspectiveProjection)),
            toReflected: static (port, _) => Refused.Unless(port.ChangeToParallelReflectedProjection(), nameof(RhinoViewport.ChangeToParallelReflectedProjection)),
            toPlan: static (port, plan) => plan.Frame.IfNone(port.ConstructionPlane) switch {
                var frame => Refused.Unless(port.SetToPlanView(frame.Origin, frame.XAxis, frame.YAxis, plan.SetConstructionPlane), nameof(RhinoViewport.SetToPlanView)),
            },
            defined: static (port, defined) => Refused.Unless(port.SetProjection(defined.Projection, Conversions.Unset(defined.ViewName), defined.UpdateConstructionPlane), nameof(RhinoViewport.SetProjection)),
            isometric: static (port, isometric) => Refused.Unless(port.SetProjection(isometric.Camera, Conversions.Unset(isometric.ViewName), isometric.UpdateConstructionPlane), nameof(RhinoViewport.SetProjection)),
            lens: static (port, lens) => {
                port.Camera35mmLensLength = lens.Length;
                return Fin.Succ(unit);
            });

    // --- [VIEWPORTS]
    public static IO<Seq<bool>> ApplyToViewports(RhinoDoc doc, ViewportSet viewports, Func<RhinoViewport, Fin<Unit>> edit, RedrawPolicy redraw) =>
        Commits.WithinRedraw(doc, redraw, Viewports.ResolveViewports(doc, viewports).Bracket(
            Use: rows => rows.TraverseM(row =>
                from before in IO.lift(() => row.Viewport.ChangeCounter)
                from edited in IO.lift(() => edit(row.Viewport))
                from moved in IO.lift(() => row.Viewport.ChangeCounter != before)
                from committed in when(moved, IO.lift(row.CommitViewportChanges)).As()
                select moved).As(),
            Fin: DisposalOps.Release));
}

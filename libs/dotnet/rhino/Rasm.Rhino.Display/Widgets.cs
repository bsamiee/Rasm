using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.DocObjects.Tables;
using Rhino.UI;

namespace Rasm.Rhino.Display;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GripConstraint {
    public sealed record ToCurve(Curve Curve) : GripConstraint;

    public sealed record ToCircle(Circle Circle) : GripConstraint;

    public sealed record ToLine(Line Line) : GripConstraint;

    public sealed record ToArc(Arc Arc) : GripConstraint;
}

public sealed record GripAppearance(Option<GripUserInterfaceObjectShape> Shape, Option<float> RotationRadians, Option<float> StrokeWidth, Option<float> Radius, Option<Color> Color, Option<Color> Fill);

public sealed record GripSnap(Option<bool> ObjectSnapPermitted, Option<bool> ObjectSnapCursors, Option<bool> OnObjectCursors, Option<GripConstraint> Constraint, Seq<Point3d> SnapPoints);

public sealed record ControlAppearance(
    Option<string> Text,
    Option<ControlHorizontalAlignment> HorizontalAlignment,
    Option<ControlVerticalAlignment> VerticalAlignment,
    Option<float> TextHeight,
    Option<Color> TextColor,
    Option<ControlHorizontalAlignment> TextHorizontalAlignment,
    Option<ControlVerticalAlignment> TextVerticalAlignment,
    Option<Point3d> TrackingPoint);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetSpec {
    public sealed record Grip(Point3d Location, GripAppearance Appearance, GripSnap Snap) : WidgetSpec;

    public sealed record DirectionGrip(
        Point3d Location,
        Vector3d Direction,
        GripAppearance Appearance,
        GripSnap Snap,
        Option<float> LineLength,
        Option<float> ArrowRadius,
        Option<GripUserInterfaceObjectShape> ArrowShape,
        Option<bool> GripPointVisible,
        Option<bool> OneWay) : WidgetSpec;

    public sealed record RotationGrip(Plane Plane, double Radius, GripAppearance Appearance, GripSnap Snap, Option<bool> GripPointVisible) : WidgetSpec;

    public sealed record TextDot(Point3d Location, string Text, Option<Color> TextColor, Option<Color> Background, Option<Color> Border, Option<int> TextHeight, Option<int> HoverTextHeight) : WidgetSpec;

    public sealed record Svg(System.Drawing.Point Location, Size Size, string Markup, ControlAppearance Appearance) : WidgetSpec;

    public sealed record Slider(
        PointF Location,
        SizeF Size,
        ControlAppearance Appearance,
        global::Rhino.Geometry.Interval Range,
        double Value,
        Option<bool> Horizontal,
        Option<bool> DisplayValue,
        Option<bool> AllowBeforeStart,
        Option<bool> AllowAfterEnd,
        Option<int> DigitPrecision) : WidgetSpec;
}

public sealed record WidgetPresence(Option<bool> Visible, Option<bool> BoundToActiveView, Option<string> Tooltip);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetScope {
    public sealed record AllDocuments() : WidgetScope;

    public sealed record InDocument(RhinoDoc Doc, Guid Group) : WidgetScope;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetEvent {
    public sealed record Mouse(MousePhase Phase, MouseState State) : WidgetEvent;

    public sealed record Dragged(Point3d To, MouseState State) : WidgetEvent;

    public sealed record Rotated(double Angle, MouseState State) : WidgetEvent;

    public sealed record ValueChanged(double Value) : WidgetEvent;
}

public sealed record WidgetCallbacks(Func<WidgetEvent, IO<Unit>> Sink, Option<Func<DrawEventArgs, IO<Unit>>> Overlay, Option<Func<RhinoDoc, RunMode, MouseState, IO<Unit>>> Command, Action<Error> Reject);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class GripWidget(Point3d location, WidgetCallbacks callbacks) : GripUserInterfaceObject(location) {
    protected override void OnDraw(DrawEventArgs args) {
        base.OnDraw(args);
        Widgets.Overlay(callbacks, args);
    }

    protected override void OnMouseDown(MouseState mouse) {
        base.OnMouseDown(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Down, mouse));
    }

    protected override void OnMouseUp(MouseState mouse) {
        base.OnMouseUp(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Up, mouse));
    }

    protected override void OnMouseMove(MouseState mouse) {
        base.OnMouseMove(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Move, mouse));
    }

    protected override void OnMouseEnter(MouseState mouse) {
        base.OnMouseEnter(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Enter, mouse));
    }

    protected override void OnMouseLeave(MouseState mouse) {
        base.OnMouseLeave(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Leave, mouse));
    }

    protected override void OnMouseClick(MouseState mouse) {
        base.OnMouseClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Click, mouse));
    }

    protected override void OnMouseDoubleClick(MouseState mouse) {
        base.OnMouseDoubleClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.DoubleClick, mouse));
    }

    protected override void OnDrag(Point3d newLocation, MouseState mouse) {
        base.OnDrag(newLocation, mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Dragged(newLocation, mouse));
    }

    protected override Result OnRunCommand(RhinoDoc doc, RunMode mode, MouseState mouse) =>
        Widgets.Command(callbacks, doc, mode, mouse, () => base.OnRunCommand(doc, mode, mouse));
}

public sealed class DirectionWidget(Point3d location, Vector3d direction, WidgetCallbacks callbacks) : DirectionGripUserInterfaceObject(location, direction) {
    protected override void OnDraw(DrawEventArgs args) {
        base.OnDraw(args);
        Widgets.Overlay(callbacks, args);
    }

    protected override void OnMouseDown(MouseState mouse) {
        base.OnMouseDown(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Down, mouse));
    }

    protected override void OnMouseUp(MouseState mouse) {
        base.OnMouseUp(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Up, mouse));
    }

    protected override void OnMouseMove(MouseState mouse) {
        base.OnMouseMove(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Move, mouse));
    }

    protected override void OnMouseEnter(MouseState mouse) {
        base.OnMouseEnter(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Enter, mouse));
    }

    protected override void OnMouseLeave(MouseState mouse) {
        base.OnMouseLeave(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Leave, mouse));
    }

    protected override void OnMouseClick(MouseState mouse) {
        base.OnMouseClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Click, mouse));
    }

    protected override void OnMouseDoubleClick(MouseState mouse) {
        base.OnMouseDoubleClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.DoubleClick, mouse));
    }

    protected override void OnDrag(Point3d newLocation, MouseState mouse) {
        base.OnDrag(newLocation, mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Dragged(newLocation, mouse));
    }

    protected override Result OnRunCommand(RhinoDoc doc, RunMode mode, MouseState mouse) =>
        Widgets.Command(callbacks, doc, mode, mouse, () => base.OnRunCommand(doc, mode, mouse));
}

public sealed class RotationWidget(Plane plane, double radius, WidgetCallbacks callbacks) : RotationGripUserInterfaceObject(plane, radius) {
    protected override void OnDraw(DrawEventArgs args) {
        base.OnDraw(args);
        Widgets.Overlay(callbacks, args);
    }

    protected override void OnMouseDown(MouseState mouse) {
        base.OnMouseDown(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Down, mouse));
    }

    protected override void OnMouseUp(MouseState mouse) {
        base.OnMouseUp(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Up, mouse));
    }

    protected override void OnMouseMove(MouseState mouse) {
        base.OnMouseMove(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Move, mouse));
    }

    protected override void OnMouseEnter(MouseState mouse) {
        base.OnMouseEnter(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Enter, mouse));
    }

    protected override void OnMouseLeave(MouseState mouse) {
        base.OnMouseLeave(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Leave, mouse));
    }

    protected override void OnMouseClick(MouseState mouse) {
        base.OnMouseClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Click, mouse));
    }

    protected override void OnMouseDoubleClick(MouseState mouse) {
        base.OnMouseDoubleClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.DoubleClick, mouse));
    }

    protected override void OnDrag(Point3d newLocation, MouseState mouse) {
        base.OnDrag(newLocation, mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Dragged(newLocation, mouse));
    }

    protected override void OnRotationDrag(double angle, MouseState mouse) {
        base.OnRotationDrag(angle, mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Rotated(angle, mouse));
    }

    protected override Result OnRunCommand(RhinoDoc doc, RunMode mode, MouseState mouse) =>
        Widgets.Command(callbacks, doc, mode, mouse, () => base.OnRunCommand(doc, mode, mouse));
}

public sealed class TextDotWidget(Point3d location, string text, WidgetCallbacks callbacks) : TextDotUserInterfaceObject(location, text) {
    protected override void OnDraw(DrawEventArgs args) {
        base.OnDraw(args);
        Widgets.Overlay(callbacks, args);
    }

    protected override void OnMouseDown(MouseState mouse) {
        base.OnMouseDown(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Down, mouse));
    }

    protected override void OnMouseUp(MouseState mouse) {
        base.OnMouseUp(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Up, mouse));
    }

    protected override void OnMouseMove(MouseState mouse) {
        base.OnMouseMove(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Move, mouse));
    }

    protected override void OnMouseEnter(MouseState mouse) {
        base.OnMouseEnter(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Enter, mouse));
    }

    protected override void OnMouseLeave(MouseState mouse) {
        base.OnMouseLeave(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Leave, mouse));
    }

    protected override void OnMouseClick(MouseState mouse) {
        base.OnMouseClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Click, mouse));
    }

    protected override void OnMouseDoubleClick(MouseState mouse) {
        base.OnMouseDoubleClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.DoubleClick, mouse));
    }

    protected override Result OnRunCommand(RhinoDoc doc, RunMode mode, MouseState mouse) =>
        Widgets.Command(callbacks, doc, mode, mouse, () => base.OnRunCommand(doc, mode, mouse));
}

public sealed class SvgWidget(System.Drawing.Point location, Size size, WidgetCallbacks callbacks) : UserInterfaceControl(location, size) {
    protected override void OnDraw(DrawEventArgs args) {
        base.OnDraw(args);
        Widgets.Overlay(callbacks, args);
    }

    protected override void OnMouseDown(MouseState mouse) {
        base.OnMouseDown(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Down, mouse));
    }

    protected override void OnMouseUp(MouseState mouse) {
        base.OnMouseUp(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Up, mouse));
    }

    protected override void OnMouseMove(MouseState mouse) {
        base.OnMouseMove(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Move, mouse));
    }

    protected override void OnMouseEnter(MouseState mouse) {
        base.OnMouseEnter(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Enter, mouse));
    }

    protected override void OnMouseLeave(MouseState mouse) {
        base.OnMouseLeave(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Leave, mouse));
    }

    protected override void OnMouseClick(MouseState mouse) {
        base.OnMouseClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Click, mouse));
    }

    protected override void OnMouseDoubleClick(MouseState mouse) {
        base.OnMouseDoubleClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.DoubleClick, mouse));
    }

    protected override Result OnRunCommand(RhinoDoc doc, RunMode mode, MouseState mouse) =>
        Widgets.Command(callbacks, doc, mode, mouse, () => base.OnRunCommand(doc, mode, mouse));
}

public sealed class SliderWidget(WidgetCallbacks callbacks) : UserInterfaceSlider {
    protected override void OnDraw(DrawEventArgs args) {
        base.OnDraw(args);
        Widgets.Overlay(callbacks, args);
    }

    protected override void OnMouseDown(MouseState mouse) {
        base.OnMouseDown(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Down, mouse));
    }

    protected override void OnMouseUp(MouseState mouse) {
        base.OnMouseUp(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Up, mouse));
    }

    protected override void OnMouseMove(MouseState mouse) {
        base.OnMouseMove(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Move, mouse));
    }

    protected override void OnMouseEnter(MouseState mouse) {
        base.OnMouseEnter(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Enter, mouse));
    }

    protected override void OnMouseLeave(MouseState mouse) {
        base.OnMouseLeave(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Leave, mouse));
    }

    protected override void OnMouseClick(MouseState mouse) {
        base.OnMouseClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.Click, mouse));
    }

    protected override void OnMouseDoubleClick(MouseState mouse) {
        base.OnMouseDoubleClick(mouse);
        Widgets.Deliver(callbacks, new WidgetEvent.Mouse(MousePhase.DoubleClick, mouse));
    }

    protected override void OnValueChanged() {
        base.OnValueChanged();
        Widgets.Deliver(callbacks, new WidgetEvent.ValueChanged(Value));
    }

    protected override Result OnRunCommand(RhinoDoc doc, RunMode mode, MouseState mouse) =>
        Widgets.Command(callbacks, doc, mode, mouse, () => base.OnRunCommand(doc, mode, mouse));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Widgets {
    // --- [REGISTRATION]
    public static IO<UserInterfaceObjectBase> Register(WidgetSpec spec, WidgetScope scope, WidgetPresence presence, WidgetCallbacks callbacks) =>
        from item in IO.lift(() => spec.Switch<WidgetCallbacks, UserInterfaceObjectBase>(
            callbacks,
            grip: static (callbacks, grip) => Snapped(Styled(new GripWidget(grip.Location, callbacks), grip.Appearance), grip.Snap),
            directionGrip: static (callbacks, grip) => {
                DirectionWidget widget = new(grip.Location, grip.Direction, callbacks);
                _ = grip.LineLength.Iter(length => widget.DirectionLineLength = length);
                _ = grip.ArrowRadius.Iter(radius => widget.ArrowRadius = radius);
                _ = grip.ArrowShape.Iter(shape => widget.ArrowShape = shape);
                _ = grip.GripPointVisible.Iter(visible => widget.GripPointVisible = visible);
                _ = grip.OneWay.Iter(oneWay => widget.OneWay = oneWay);
                return Snapped(Styled(widget, grip.Appearance), grip.Snap);
            },
            rotationGrip: static (callbacks, grip) => {
                RotationWidget widget = new(grip.Plane, grip.Radius, callbacks);
                _ = grip.GripPointVisible.Iter(visible => widget.GripPointVisible = visible);
                return Snapped(Styled(widget, grip.Appearance), grip.Snap);
            },
            textDot: static (callbacks, dot) => {
                TextDotWidget widget = new(dot.Location, dot.Text, callbacks);
                _ = dot.TextColor.Iter(color => widget.TextColor = color);
                _ = dot.Background.Iter(color => widget.DotBackgroundColor = color);
                _ = dot.Border.Iter(color => widget.DotBorderColor = color);
                _ = dot.TextHeight.Iter(height => widget.TextHeight = height);
                _ = dot.HoverTextHeight.Iter(height => widget.MouseOverTextHeight = height);
                return widget;
            },
            svg: static (callbacks, svg) => {
                SvgWidget widget = new(svg.Location, svg.Size, callbacks);
                widget.SetSvg(svg.Markup);
                return Styled(widget, svg.Appearance);
            },
            slider: static (callbacks, slider) => {
                SliderWidget widget = new(callbacks) { Location = slider.Location, Size = slider.Size, Range = slider.Range, Value = slider.Value };
                _ = slider.Horizontal.Iter(horizontal => widget.HorizontalOrientation = horizontal);
                _ = slider.DisplayValue.Iter(display => widget.DisplayValue = display);
                _ = slider.AllowBeforeStart.Iter(allow => widget.AllowValueBeforeRangeStart = allow);
                _ = slider.AllowAfterEnd.Iter(allow => widget.AllowValueAfterRangeEnd = allow);
                _ = slider.DigitPrecision.Iter(digits => widget.DigitPrecision = digits);
                return Styled(widget, slider.Appearance);
            }))
        from shown in IO.lift(() => {
            _ = presence.Visible.Iter(visible => item.Visible = visible);
            _ = presence.BoundToActiveView.Iter(bound => item.BoundToActiveView = bound);
            _ = presence.Tooltip.Iter(tooltip => item.Tooltip = tooltip);
        })
        from registered in IO.lift(() => scope.Switch(
            item,
            allDocuments: static (widget, _) => Refused.Unless(widget.RegisterForAllDocuments(), nameof(UserInterfaceObjectBase.RegisterForAllDocuments)),
            inDocument: static (widget, bound) => Refused.Unless(bound.Doc.ViewUserInterface.Add(widget, bound.Group), nameof(ViewUserInterfaceTable.Add))))
        select item;

    public static IO<Unit> Unregister(UserInterfaceObjectBase item, WidgetScope scope) =>
        scope.Switch(
            item,
            allDocuments: static (widget, _) => IO.lift(widget.Unregister),
            inDocument: static (widget, bound) => IO.lift(() => Refused.Unless(bound.Doc.ViewUserInterface.Remove(widget) != 0, nameof(ViewUserInterfaceTable.Remove))));

    private static GripUserInterfaceObject Styled(GripUserInterfaceObject grip, GripAppearance appearance) {
        _ = appearance.Shape.Iter(shape => grip.GripShape = shape);
        _ = appearance.RotationRadians.Iter(radians => grip.GripShapeRotationRadians = radians);
        _ = appearance.StrokeWidth.Iter(width => grip.GripStrokeWidth = width);
        _ = appearance.Radius.Iter(radius => grip.GripRadius = radius);
        _ = appearance.Color.Iter(color => grip.GripColor = color);
        _ = appearance.Fill.Iter(fill => grip.GripFillColor = fill);
        return grip;
    }

    private static GripUserInterfaceObject Snapped(GripUserInterfaceObject grip, GripSnap snap) {
        _ = snap.ObjectSnapPermitted.Iter(permitted => grip.ObjectSnapPermitted = permitted);
        _ = snap.ObjectSnapCursors.Iter(cursors => grip.ObjectSnapCursorsEnabled = cursors);
        _ = snap.OnObjectCursors.Iter(cursors => grip.OnObjectCursorsEnabled = cursors);
        _ = snap.Constraint.Iter(constraint => constraint.Switch(
            grip,
            toCurve: static (target, curve) => target.Constrain(curve.Curve),
            toCircle: static (target, circle) => target.Constrain(circle.Circle),
            toLine: static (target, line) => target.Constrain(line.Line),
            toArc: static (target, arc) => target.Constrain(arc.Arc)));
        if (!snap.SnapPoints.IsEmpty)
            grip.SetSnapPoints(snap.SnapPoints);
        return grip;
    }

    private static UserInterfaceControl Styled(UserInterfaceControl control, ControlAppearance appearance) {
        _ = appearance.Text.Iter(text => control.Text = text);
        _ = appearance.HorizontalAlignment.Iter(alignment => control.HorizontalAlignment = alignment);
        _ = appearance.VerticalAlignment.Iter(alignment => control.VerticalAlignment = alignment);
        _ = appearance.TextHeight.Iter(height => control.TextHeight = height);
        _ = appearance.TextColor.Iter(color => control.TextColor = color);
        _ = appearance.TextHorizontalAlignment.Iter(alignment => control.TextHorizontalAlignment = alignment);
        _ = appearance.TextVerticalAlignment.Iter(alignment => control.TextVerticalAlignment = alignment);
        _ = appearance.TrackingPoint.Iter(point => control.TrackingPoint = point);
        return control;
    }

    // --- [CALLBACKS]
    internal static void Deliver(WidgetCallbacks callbacks, WidgetEvent evt) =>
        _ = Answers.Answer(callbacks.Sink(evt), callbacks.Reject, unit);

    internal static void Overlay(WidgetCallbacks callbacks, DrawEventArgs args) =>
        _ = Answers.Answer(callbacks.Overlay.Map(draw => draw(args)), callbacks.Reject, static () => unit);

    internal static Result Command(WidgetCallbacks callbacks, RhinoDoc doc, RunMode mode, MouseState mouse, Func<Result> fallback) =>
        callbacks.Command.Match(
            Some: run => Answers.ToResult(run(doc, mode, mouse).RunSafe(), callbacks.Reject),
            None: fallback);
}

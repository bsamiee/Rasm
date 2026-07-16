# [RASM_RHINO_INTERACTION]

The in-viewport interaction owner (`Rasm.Rhino.Display`). Three host tiers become three typed owners on one fact vocabulary: `Pointers` adapts the document-wide `MouseCallback` hook — the six paired begin/end overrides plus enter/hover/leave — into a `PointerFact` stream delivered through a bounded channel so a per-move callback submits and returns; `GumballRig` owns the manipulator lifecycle — one `GumballSeat` `[Union]` collapsing the `GumballObject.SetFrom*` family, `GumballAppearanceSettings` as policy, the `GumballDisplayConduit` seat/pick/update drag fold over `PickGumball` and both `UpdateGumball` overloads; and `WidgetHost` registers the Rhino 9 `UserInterfaceObjectBase` families — grip, direction grip, rotation grip, text dot, SVG control, and slider — as typed `WidgetSpec` rows whose events fold into one `WidgetFact` stream and whose hit facts come from the picked `MouseState` (`Button`, `FrustumLine`, curve/line `IsMouseOver` tests). Every fact carries kernel-neutral values — screen points as `Point2d`, world rays as `Line`, identities as `Guid` — and no live host handle, `MouseCallbackEventArgs`, or `MouseState` crosses into a consumer.

## [01]-[INDEX]

- [02]-[POINTER_STREAM]: `PointerPhase` rows, `PointerFact`, and the `Pointers` mouse-hook adapter with channel handoff.
- [03]-[GUMBALL]: `GumballSeat` seating union, `GumballRig` the conduit lifecycle with the pick/update drag fold and transform evidence.
- [04]-[WIDGETS]: `WidgetSpec` rows over the Rhino 9 in-viewport UI-object families, `WidgetFact`, and the `WidgetHost` registration lifecycle.

## [02]-[POINTER_STREAM]

- Owner: `PointerPhase` `[SmartEnum<int>]` — the hook phases with their host pairing as a column: `Move`/`EndMove`, `Down`/`EndDown`, `Up`/`EndUp`, `DoubleClick`, `Enter`, `Hover`, `Leave` — the `Paired` column marks the begin/end pairs Rhino 9 delivers, so a consumer distinguishing raw from post-processed mouse traffic reads the row, never a naming convention. `PointerFact` — the typed fact: phase, viewport id, screen point as `Point2d`, the `PointerButton` row re-closed from the host `MouseButton` with the shift/control modifier flags, the over-gumball flag, and the capture timestamp. `Pointers` — the public pointer rail whose ONE private `MouseCallback` hook projects args into facts and `TryWrite`s them into a bounded channel; `Enabled` is the mount bit, and the host attaches each event only where an override exists, so the hook overrides all ten.
- Entry: `Pointers.Mount(Option<int> capacity, Op?) : Fin<(ChannelReader<PointerFact>, IDisposable)>` — the reader is the consumer seam, the disposer disables the hook and completes the writer.
- Law: the callback submits and returns — a full channel drops the oldest (`BoundedChannelFullMode.DropOldest`) because pointer traffic is latest-wins for every consumer this package serves; blocking the host mouse thread is unrepresentable.
- Law: tooltip text rides the one `Pointers.Tooltip` verb over `MouseCursor.SetToolTip(string)` — a fact-driven response beside the stream, never an ambient static call scattered at consumers.
- Boundary: a fact carries `IsOverGumball()` as data so a drag consumer yields to the manipulator without probing host state; the gumball itself is `[03]`'s owner.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Threading.Channels;
using Rasm.Domain;
using Rasm.Numerics;
using Rasm.Rhino.Document;
using Rasm.Rhino.Viewport;

namespace Rasm.Rhino.Display;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class PointerPhase {
    public static readonly PointerPhase Move = new(key: 0, paired: true);
    public static readonly PointerPhase EndMove = new(key: 1, paired: true);
    public static readonly PointerPhase Down = new(key: 2, paired: true);
    public static readonly PointerPhase EndDown = new(key: 3, paired: true);
    public static readonly PointerPhase Up = new(key: 4, paired: true);
    public static readonly PointerPhase EndUp = new(key: 5, paired: true);
    public static readonly PointerPhase DoubleClick = new(key: 6, paired: false);
    public static readonly PointerPhase Enter = new(key: 7, paired: false);
    public static readonly PointerPhase Hover = new(key: 8, paired: false);
    public static readonly PointerPhase Leave = new(key: 9, paired: false);

    public bool Paired { get; }
}

[SmartEnum<int>]
public sealed partial class PointerButton {
    public static readonly PointerButton None = new(key: 0);
    public static readonly PointerButton Left = new(key: 1);
    public static readonly PointerButton Right = new(key: 2);
    public static readonly PointerButton Middle = new(key: 3);

    internal static PointerButton Of(Rhino.UI.MouseButton button) =>
        button switch {
            Rhino.UI.MouseButton.Left => Left,
            Rhino.UI.MouseButton.Right => Right,
            Rhino.UI.MouseButton.Middle => Middle,
            _ => None,
        };
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct PointerFact(
    PointerPhase Phase,
    Guid ViewportId,
    Point2d At,
    PointerButton Button,
    bool Shift,
    bool Control,
    bool OverGumball,
    long Timestamp);

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Pointers {
    public static Fin<Unit> Tooltip(string text, Op? key = null) {
        Op op = key.OrDefault();
        return from valid in op.AcceptText(value: text)
               from _ in op.Catch(() => Fin.Succ(value: Op.Side(() => Rhino.UI.MouseCursor.SetToolTip(valid))))
               select unit;
    }

    public static Fin<(ChannelReader<PointerFact> Facts, IDisposable Mount)> Mount(Option<int> capacity = default, Op? key = null) =>
        key.OrDefault().Catch(() => {
            Channel<PointerFact> channel = Channel.CreateBounded<PointerFact>(new BoundedChannelOptions(capacity.IfNone(256)) {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = false,
                SingleWriter = true,
            });
            Hook hook = new(sink: channel.Writer) { Enabled = true };
            return Fin.Succ<(ChannelReader<PointerFact>, IDisposable)>((channel.Reader, Subscription.Of(detach: () => {
                hook.Enabled = false;
                _ = channel.Writer.TryComplete();
            })));
        });

    private sealed class Hook : Rhino.UI.MouseCallback {
        private readonly ChannelWriter<PointerFact> sink;

        internal Hook(ChannelWriter<PointerFact> sink) => this.sink = sink;

        protected override void OnMouseMove(Rhino.UI.MouseCallbackEventArgs e) => Emit(phase: PointerPhase.Move, e: e);
        protected override void OnEndMouseMove(Rhino.UI.MouseCallbackEventArgs e) => Emit(phase: PointerPhase.EndMove, e: e);
        protected override void OnMouseDown(Rhino.UI.MouseCallbackEventArgs e) => Emit(phase: PointerPhase.Down, e: e);
        protected override void OnEndMouseDown(Rhino.UI.MouseCallbackEventArgs e) => Emit(phase: PointerPhase.EndDown, e: e);
        protected override void OnMouseUp(Rhino.UI.MouseCallbackEventArgs e) => Emit(phase: PointerPhase.Up, e: e);
        protected override void OnEndMouseUp(Rhino.UI.MouseCallbackEventArgs e) => Emit(phase: PointerPhase.EndUp, e: e);
        protected override void OnMouseDoubleClick(Rhino.UI.MouseCallbackEventArgs e) => Emit(phase: PointerPhase.DoubleClick, e: e);
        protected override void OnMouseEnter(Rhino.UI.MouseCallbackEventArgs e) => Emit(phase: PointerPhase.Enter, e: e);
        protected override void OnMouseHover(Rhino.UI.MouseCallbackEventArgs e) => Emit(phase: PointerPhase.Hover, e: e);
        protected override void OnMouseLeave(Rhino.UI.MouseCallbackEventArgs e) => Emit(phase: PointerPhase.Leave, e: e);

        private void Emit(PointerPhase phase, Rhino.UI.MouseCallbackEventArgs e) =>
            _ = sink.TryWrite(new PointerFact(
                Phase: phase,
                ViewportId: e.View.ActiveViewport.Id,
                At: new Point2d(e.ViewportPoint.X, e.ViewportPoint.Y),
                Button: PointerButton.Of(button: e.MouseButton),
                Shift: e.ShiftKeyDown,
                Control: e.CtrlKeyDown,
                OverGumball: e.IsOverGumball() != Rhino.UI.Gumball.GumballMode.None,
                Timestamp: Environment.TickCount64));
    }
}
```

## [03]-[GUMBALL]

- Owner: `GumballSeat` `[Union]` — one seating vocabulary over the `GumballObject.SetFrom*` family: `BoundsCase(BoundingBox, Option<Plane>)` through `SetFromBoundingBox(boundingBox:)` or the framed `SetFromBoundingBox(frame:, frameBoundingBox:)`, plus `LineCase`, `PlaneCase`, `ArcCase`, `CircleCase`, `EllipseCase`, `CurveCase`, `ExtrusionCase`, `LightCase`, `HatchCase` — each one host call, so seating any geometry family is one union case. `GumballRig` — the lifecycle capsule: constructs the `GumballObject` and `GumballDisplayConduit`, applies the `GumballAppearanceSettings` policy through `SetBaseGumball`, mounts, and folds the drag: `Pick(PickContext, GetPoint)` through `PickGumball`, `Drag(Point3d, Line)` and `Drag(Plane)` through the two `UpdateGumball` overloads, and `Evidence()` projecting the drag's total and incremental transforms (`TotalTransform`/`GumballTransform`) as values.
- Entry: `GumballRig.Mount(GumballSeat, Option<GumballAppearanceSettings>, ActiveSpace, Op?) : Fin<GumballRig>` — the conduit constructs space-bound through `GumballDisplayConduit(ActiveSpace)`, the parameterless host constructor being obsolete; the rig is `IDisposable` and disposal disables the conduit.
- Law: the drag is a fold over host updates — pick seats the drag, each update recomputes the conduit's transform, and `Evidence` reads it as a `Transform` value with the seat echo; a consumer never mutates geometry from inside the drag — it applies the evidence transform through its own transaction rail after the drag commits.
- Law: appearance is policy data applied once at `SetBaseGumball`; per-drag appearance mutation re-seats the rig.
- Boundary: `PickContext` and `GetPoint` arrive from the interaction unit's acquisition rail as borrowed host values inside the pick call — the rig holds neither.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GumballSeat {
    private GumballSeat() { }
    public sealed record BoundsCase(BoundingBox Bounds, Option<Plane> Frame) : GumballSeat;
    public sealed record LineCase(Line Value) : GumballSeat;
    public sealed record PlaneCase(Plane Value) : GumballSeat;
    public sealed record ArcCase(Arc Value) : GumballSeat;
    public sealed record CircleCase(Circle Value) : GumballSeat;
    public sealed record EllipseCase(Ellipse Value) : GumballSeat;
    public sealed record CurveCase(Curve Value) : GumballSeat;
    public sealed record ExtrusionCase(Extrusion Value) : GumballSeat;
    public sealed record LightCase(Light Value) : GumballSeat;
    public sealed record HatchCase(Hatch Value) : GumballSeat;

    internal Fin<Unit> Seat(Rhino.UI.Gumball.GumballObject gumball, Op key) =>
        Switch(
            state: (Gumball: gumball, Op: key),
            boundsCase: static (ctx, seat) => seat.Frame.Match(
                Some: frame => ctx.Op.Confirm(success: ctx.Gumball.SetFromBoundingBox(frame: frame, frameBoundingBox: seat.Bounds)),
                None: () => ctx.Op.Confirm(success: ctx.Gumball.SetFromBoundingBox(boundingBox: seat.Bounds))),
            lineCase: static (ctx, seat) => ctx.Op.Confirm(success: ctx.Gumball.SetFromLine(line: seat.Value)),
            planeCase: static (ctx, seat) => ctx.Op.Confirm(success: ctx.Gumball.SetFromPlane(plane: seat.Value)),
            arcCase: static (ctx, seat) => ctx.Op.Confirm(success: ctx.Gumball.SetFromArc(arc: seat.Value)),
            circleCase: static (ctx, seat) => ctx.Op.Confirm(success: ctx.Gumball.SetFromCircle(circle: seat.Value)),
            ellipseCase: static (ctx, seat) => ctx.Op.Confirm(success: ctx.Gumball.SetFromEllipse(ellipse: seat.Value)),
            curveCase: static (ctx, seat) => ctx.Op.Confirm(success: ctx.Gumball.SetFromCurve(curve: seat.Value)),
            extrusionCase: static (ctx, seat) => ctx.Op.Confirm(success: ctx.Gumball.SetFromExtrusion(extrusion: seat.Value)),
            lightCase: static (ctx, seat) => ctx.Op.Confirm(success: ctx.Gumball.SetFromLight(light: seat.Value)),
            hatchCase: static (ctx, seat) => ctx.Op.Confirm(success: ctx.Gumball.SetFromHatch(hatch: seat.Value)));
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct GumballEvidence(Transform Total, Transform Incremental, GumballSeat Seat, bool Dragging);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class GumballRig : IDisposable {
    private readonly Rhino.UI.Gumball.GumballObject gumball;
    private readonly Rhino.UI.Gumball.GumballDisplayConduit conduit;
    private readonly GumballSeat seat;
    private bool dragging;
    private int released;

    private GumballRig(Rhino.UI.Gumball.GumballObject gumball, Rhino.UI.Gumball.GumballDisplayConduit conduit, GumballSeat seat) {
        this.gumball = gumball;
        this.conduit = conduit;
        this.seat = seat;
    }

    public static Fin<GumballRig> Mount(GumballSeat seat, Option<Rhino.UI.Gumball.GumballAppearanceSettings> appearance = default, ActiveSpace space = ActiveSpace.ModelSpace, Op? key = null) {
        Op op = key.OrDefault();
        return from request in Optional(seat).ToFin(Fail: op.InvalidInput())
               from rig in op.Catch(() => {
                   Rhino.UI.Gumball.GumballObject ball = new();
                   Rhino.UI.Gumball.GumballDisplayConduit pipe = new(space: space);
                   return Fin.Succ(new GumballRig(gumball: ball, conduit: pipe, seat: request));
               })
               from _ in request.Seat(gumball: rig.gumball, key: op)
               from __ in op.Catch(() => {
                   rig.conduit.SetBaseGumball(gumball: rig.gumball, appearanceSettings: appearance.IfNone(() => new Rhino.UI.Gumball.GumballAppearanceSettings()));
                   rig.conduit.Enabled = true;
                   return Fin.Succ(value: unit);
               })
               select rig;
    }

    public Fin<bool> Pick(Input.Custom.PickContext pick, Input.Custom.GetPoint point, Op? key = null) {
        Op op = key.OrDefault();
        return from context in Optional(pick).ToFin(Fail: op.InvalidInput())
               from getter in Optional(point).ToFin(Fail: op.InvalidInput())
               from picked in op.Catch(() => Fin.Succ(conduit.PickGumball(pickContext: context, getPoint: getter)))
               from _ in Fin.Succ(ignore(dragging = picked))
               select picked;
    }

    public Fin<Unit> Drag(Point3d point, Line worldLine, Op? key = null) =>
        key.OrDefault().Confirm(success: conduit.UpdateGumball(point: point, worldLine: worldLine));

    public Fin<Unit> Drag(Plane frame, Op? key = null) =>
        key.OrDefault().Confirm(success: conduit.UpdateGumball(frame: frame));

    public GumballEvidence Evidence() => new(Total: conduit.TotalTransform, Incremental: conduit.GumballTransform, Seat: seat, Dragging: dragging);

    public Unit Commit() => ignore(dragging = false);

    public void Dispose() {
        if (Interlocked.Exchange(location1: ref released, value: 1) is not 0) { return; }
        conduit.Enabled = false;
        conduit.Dispose();
        gumball.Dispose();
    }
}
```

## [04]-[WIDGETS]

- Owner: `WidgetSpec` `[Union]` — the Rhino 9 in-viewport widget rows: `GripCase` a constrained snap-point grip (the location constructor plus `GripRadius`/`Constrain`/`SetSnapPoints`/`ObjectSnapPermitted`, with an optional chrome tuple over `GripShape`/`GripColor`/`GripFillColor`/`GripStrokeWidth`/`GripShapeRotationRadians`), `DirectionCase` a direction-arrow grip (the location-direction constructor plus `ArrowRadius`, `OneWay`, and the optional `DirectionLineLength`), `RotationCase(Plane, double)` a rotation-arc grip (the plane-radius constructor) with the `OnRotationDrag(double, MouseState)` hook, `TextDotCase` (the location-text constructor plus `TextHeight`, an optional ink triple over `TextColor`/`DotBackgroundColor`/`DotBorderColor`, and `MouseOverTextHeight`), `SvgCase(string, Point2d, Size2i)` an SVG-backed control (the protected screen location-size constructor plus `SetSvg`), and `SliderCase` a ranged slider (`Range`/`Value`/`HorizontalOrientation`/`DisplayValue`/`DigitPrecision` with the `OnValueChanged` hook). `WidgetFact` `[Union]` — the event stream: `PressedCase(Line)` carrying the pick ray, `ClickedCase(bool)` single-versus-double, `HoverCase(bool)` from the enter/leave pair, `MovedCase(Point3d)` from the grip's `OnDrag(Point3d, MouseState)` drag hook, `RotatedCase(double)`, `SlidCase(double)`, `HitCase(Option<double>)` from the picked `MouseState` curve/line tests. `WidgetSink` — the internal projection record every adapter shares: identity, channel writer, and the optional painter, so a fact spelling exists exactly once and an adapter override is one verb call. `WidgetHost` — the registration lifecycle: mints the internal host adapter per row, registers through `UserInterfaceObjectBase.RegisterForAllDocuments()`, retires through `Unregister()`, and folds every widget's events into one channel; `BoundToActiveView` and `Visible` are mount policy bits.
- Law: every family's `OnDraw(DrawEventArgs)` rides `UserInterfaceObjectBase`, so `Mount`'s optional paint composes the draw page's mark algebra over any widget kind — an in-viewport widget is a pipeline participant, never a private renderer; mouse overrides receive the picked `MouseState` and project `Button`, `FrustumLine`, and `IsMouseOver(Curve, out double)`/`IsMouseOver(Line)` into `WidgetFact` rows before any consumer sees them.
- Law: one host adapter class per host base is an internal fact; the public surface is the spec union plus the fact stream, so a new widget family is one spec case, one internal adapter over the shared `WidgetSink`, and one fact projection — consumers never subclass host bases.
- Growth: the Rhino 9 `UserInterfaceControl` SVG surface makes custom chrome one `SvgCase` payload; a richer custom widget is a new spec case over `UserInterfaceObjectBase`, never a parallel registration path.
- Boundary: widget geometry is world-space host drawing under the active view binding; HUD-space chrome belongs to the draw page's screen band inside a conduit, and the two do not mix on one spec.

```csharp
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetSpec {
    private WidgetSpec() { }
    public sealed record GripCase(
        Point3d At, double Radius, Option<Curve> Constraint, Seq<Point3d> SnapPoints, bool ObjectSnap,
        Option<(Rhino.UI.GripUserInterfaceObjectShape Shape, PerceptualColor Stroke, PerceptualColor Fill, float StrokeWidth, double RotationRadians)> Chrome = default) : WidgetSpec;
    public sealed record DirectionCase(Point3d At, Vector3d Direction, double ArrowRadius, Option<double> LineLength = default, bool OneWay = false) : WidgetSpec;
    public sealed record RotationCase(Plane Arc, double Radius) : WidgetSpec;
    public sealed record TextDotCase(
        string Text, int Height, Point3d At,
        Option<(PerceptualColor Text, PerceptualColor Back, PerceptualColor Border)> Ink = default, Option<int> HoverHeight = default) : WidgetSpec;
    public sealed record SvgCase(string Svg, Point2d At, Size2i Extent) : WidgetSpec;
    public sealed record SliderCase(Interval Range, double Value, bool Horizontal = true, bool DisplayValue = true, Option<int> Precision = default) : WidgetSpec;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WidgetFact {
    private WidgetFact() { }
    public sealed record PressedCase(Guid Widget, Line PickRay) : WidgetFact;
    public sealed record ClickedCase(Guid Widget, bool Double) : WidgetFact;
    public sealed record HoverCase(Guid Widget, bool Over) : WidgetFact;
    public sealed record MovedCase(Guid Widget, Point3d To) : WidgetFact;
    public sealed record RotatedCase(Guid Widget, double Angle) : WidgetFact;
    public sealed record SlidCase(Guid Widget, double Value) : WidgetFact;
    public sealed record HitCase(Guid Widget, Option<double> CurveParameter) : WidgetFact;
}

// --- [SERVICES] -----------------------------------------------------------------------------
internal sealed record WidgetSink(Guid Identity, System.Threading.Channels.ChannelWriter<WidgetFact> Writer, Option<Func<ConduitFrame, Fin<Unit>>> Painter) {
    internal Unit Paint(DrawEventArgs args) =>
        ignore(Painter.Iter(body => ignore(body(new ConduitFrame(
            Pipeline: args.Display, Viewport: args.Viewport, Context: FrameContext.Of(pipeline: args.Display), Phase: ConduitPhase.PostObjects)))));

    internal Unit Pressed(Rhino.UI.MouseState mouse) => ignore(Writer.TryWrite(new WidgetFact.PressedCase(Widget: Identity, PickRay: mouse.FrustumLine)));

    internal Unit Clicked(bool doubled) => ignore(Writer.TryWrite(new WidgetFact.ClickedCase(Widget: Identity, Double: doubled)));

    internal Unit Hover(bool over) => ignore(Writer.TryWrite(new WidgetFact.HoverCase(Widget: Identity, Over: over)));

    internal Unit Moved(Point3d to) => ignore(Writer.TryWrite(new WidgetFact.MovedCase(Widget: Identity, To: to)));

    internal Unit Rotated(double angle) => ignore(Writer.TryWrite(new WidgetFact.RotatedCase(Widget: Identity, Angle: angle)));

    internal Unit Slid(double value) => ignore(Writer.TryWrite(new WidgetFact.SlidCase(Widget: Identity, Value: value)));

    internal Unit Hit(Option<double> parameter) => ignore(Writer.TryWrite(new WidgetFact.HitCase(Widget: Identity, CurveParameter: parameter)));
}

internal sealed class GripWidget : Rhino.UI.GripUserInterfaceObject {
    private readonly WidgetSink sink;
    private readonly Option<Curve> constraint;

    internal GripWidget(WidgetSpec.GripCase spec, WidgetSink sink)
        : base(spec.At) {
        this.sink = sink;
        constraint = spec.Constraint;
        GripRadius = (float)spec.Radius;
        ObjectSnapPermitted = spec.ObjectSnap;
        _ = spec.Constraint.Iter(curve => Constrain(curve: curve));
        _ = Op.SideWhen(!spec.SnapPoints.IsEmpty, () => SetSnapPoints(points: spec.SnapPoints.AsEnumerable()));
        _ = spec.Chrome.Iter(chrome => {
            GripShape = chrome.Shape;
            GripColor = Quant.Sys(chrome.Stroke);
            GripFillColor = Quant.Sys(chrome.Fill);
            GripStrokeWidth = chrome.StrokeWidth;
            GripShapeRotationRadians = (float)chrome.RotationRadians;
        });
    }

    protected override void OnDraw(DrawEventArgs args) => sink.Paint(args: args);

    protected override void OnMouseDown(Rhino.UI.MouseState mouse) => sink.Pressed(mouse: mouse);

    protected override void OnDrag(Point3d newLocation, Rhino.UI.MouseState mouse) => sink.Moved(to: newLocation);

    protected override void OnMouseMove(Rhino.UI.MouseState mouse) =>
        _ = constraint.Iter(curve => sink.Hit(parameter: mouse.IsMouseOver(curve, out double t) ? Some(t) : Option<double>.None));

    protected override void OnMouseUp(Rhino.UI.MouseState mouse) => sink.Moved(to: GripLocation);

    protected override void OnMouseEnter(Rhino.UI.MouseState mouse) => sink.Hover(over: true);

    protected override void OnMouseLeave(Rhino.UI.MouseState mouse) => sink.Hover(over: false);

    protected override void OnMouseClick(Rhino.UI.MouseState mouse) => sink.Clicked(doubled: false);

    protected override void OnMouseDoubleClick(Rhino.UI.MouseState mouse) => sink.Clicked(doubled: true);
}

internal sealed class DirectionWidget : Rhino.UI.DirectionGripUserInterfaceObject {
    private readonly WidgetSink sink;

    internal DirectionWidget(WidgetSpec.DirectionCase spec, WidgetSink sink)
        : base(spec.At, spec.Direction) {
        this.sink = sink;
        ArrowRadius = (float)spec.ArrowRadius;
        OneWay = spec.OneWay;
        _ = spec.LineLength.Iter(length => DirectionLineLength = (float)length);
    }

    protected override void OnDraw(DrawEventArgs args) => sink.Paint(args: args);

    protected override void OnMouseDown(Rhino.UI.MouseState mouse) => sink.Pressed(mouse: mouse);

    protected override void OnMouseEnter(Rhino.UI.MouseState mouse) => sink.Hover(over: true);

    protected override void OnMouseLeave(Rhino.UI.MouseState mouse) => sink.Hover(over: false);

    protected override void OnMouseClick(Rhino.UI.MouseState mouse) => sink.Clicked(doubled: false);
}

internal sealed class RotationWidget : Rhino.UI.RotationGripUserInterfaceObject {
    private readonly WidgetSink sink;

    internal RotationWidget(WidgetSpec.RotationCase spec, WidgetSink sink)
        : base(spec.Arc, spec.Radius) =>
        this.sink = sink;

    protected override void OnDraw(DrawEventArgs args) => sink.Paint(args: args);

    protected override void OnMouseDown(Rhino.UI.MouseState mouse) => sink.Pressed(mouse: mouse);

    protected override void OnRotationDrag(double angle, Rhino.UI.MouseState mouse) => sink.Rotated(angle: angle);

    protected override void OnMouseEnter(Rhino.UI.MouseState mouse) => sink.Hover(over: true);

    protected override void OnMouseLeave(Rhino.UI.MouseState mouse) => sink.Hover(over: false);
}

internal sealed class TextDotWidget : Rhino.UI.TextDotUserInterfaceObject {
    private readonly WidgetSink sink;

    internal TextDotWidget(WidgetSpec.TextDotCase spec, WidgetSink sink)
        : base(spec.At, spec.Text) {
        this.sink = sink;
        TextHeight = spec.Height;
        _ = spec.Ink.Iter(ink => {
            TextColor = Quant.Sys(ink.Text);
            DotBackgroundColor = Quant.Sys(ink.Back);
            DotBorderColor = Quant.Sys(ink.Border);
        });
        _ = spec.HoverHeight.Iter(height => MouseOverTextHeight = height);
    }

    protected override void OnDraw(DrawEventArgs args) => sink.Paint(args: args);

    protected override void OnMouseDown(Rhino.UI.MouseState mouse) => sink.Pressed(mouse: mouse);

    protected override void OnMouseEnter(Rhino.UI.MouseState mouse) => sink.Hover(over: true);

    protected override void OnMouseLeave(Rhino.UI.MouseState mouse) => sink.Hover(over: false);

    protected override void OnMouseClick(Rhino.UI.MouseState mouse) => sink.Clicked(doubled: false);

    protected override void OnMouseDoubleClick(Rhino.UI.MouseState mouse) => sink.Clicked(doubled: true);
}

internal sealed class SvgWidget : Rhino.UI.UserInterfaceControl {
    private readonly WidgetSink sink;

    internal SvgWidget(WidgetSpec.SvgCase spec, WidgetSink sink)
        : base(new System.Drawing.Point((int)spec.At.X, (int)spec.At.Y), spec.Extent.Native) {
        this.sink = sink;
        SetSvg(svg: spec.Svg);
    }

    protected override void OnDraw(DrawEventArgs args) => sink.Paint(args: args);

    protected override void OnMouseDown(Rhino.UI.MouseState mouse) => sink.Pressed(mouse: mouse);

    protected override void OnMouseEnter(Rhino.UI.MouseState mouse) => sink.Hover(over: true);

    protected override void OnMouseLeave(Rhino.UI.MouseState mouse) => sink.Hover(over: false);

    protected override void OnMouseClick(Rhino.UI.MouseState mouse) => sink.Clicked(doubled: false);

    protected override void OnMouseDoubleClick(Rhino.UI.MouseState mouse) => sink.Clicked(doubled: true);
}

internal sealed class SliderWidget : Rhino.UI.UserInterfaceSlider {
    private readonly WidgetSink sink;

    internal SliderWidget(WidgetSpec.SliderCase spec, WidgetSink sink) {
        this.sink = sink;
        Range = spec.Range;
        Value = spec.Value;
        HorizontalOrientation = spec.Horizontal;
        DisplayValue = spec.DisplayValue;
        _ = spec.Precision.Iter(digits => DigitPrecision = digits);
    }

    protected override void OnDraw(DrawEventArgs args) => sink.Paint(args: args);

    protected override void OnValueChanged() => sink.Slid(value: Value);
}

public sealed class WidgetHost : IDisposable {
    private readonly System.Threading.Channels.Channel<WidgetFact> channel;
    private readonly Atom<HashMap<Guid, Rhino.UI.UserInterfaceObjectBase>> mounted = Atom(HashMap<Guid, Rhino.UI.UserInterfaceObjectBase>());
    private int released;

    private WidgetHost(System.Threading.Channels.Channel<WidgetFact> channel) => this.channel = channel;

    public System.Threading.Channels.ChannelReader<WidgetFact> Facts => channel.Reader;

    public static WidgetHost Of(Option<int> capacity = default) =>
        new(channel: System.Threading.Channels.Channel.CreateBounded<WidgetFact>(new System.Threading.Channels.BoundedChannelOptions(capacity.IfNone(256)) {
            FullMode = System.Threading.Channels.BoundedChannelFullMode.DropOldest,
        }));

    public Fin<Guid> Mount(WidgetSpec spec, Option<Func<ConduitFrame, Fin<Unit>>> paint = default, bool activeViewOnly = true, Op? key = null) {
        Op op = key.OrDefault();
        WidgetSink sink = new(Identity: Guid.NewGuid(), Writer: channel.Writer, Painter: paint);
        return from widget in spec.Switch<Fin<Rhino.UI.UserInterfaceObjectBase>>(
                   gripCase: row => op.Catch(() => Fin.Succ((Rhino.UI.UserInterfaceObjectBase)new GripWidget(spec: row, sink: sink))),
                   directionCase: row => op.Catch(() => Fin.Succ((Rhino.UI.UserInterfaceObjectBase)new DirectionWidget(spec: row, sink: sink))),
                   rotationCase: row => op.Catch(() => Fin.Succ((Rhino.UI.UserInterfaceObjectBase)new RotationWidget(spec: row, sink: sink))),
                   textDotCase: row => op.Catch(() => Fin.Succ((Rhino.UI.UserInterfaceObjectBase)new TextDotWidget(spec: row, sink: sink))),
                   svgCase: row => op.Catch(() => Fin.Succ((Rhino.UI.UserInterfaceObjectBase)new SvgWidget(spec: row, sink: sink))),
                   sliderCase: row => op.Catch(() => Fin.Succ((Rhino.UI.UserInterfaceObjectBase)new SliderWidget(spec: row, sink: sink))))
               from _ in op.Catch(() => {
                   widget.BoundToActiveView = activeViewOnly;
                   widget.Visible = true;
                   return op.Confirm(success: widget.RegisterForAllDocuments());
               })
               from __ in Fin.Succ(ignore(mounted.Swap(held => held.AddOrUpdate(sink.Identity, widget))))
               select sink.Identity;
    }

    public Fin<Unit> Retire(Guid widget, Op? key = null) {
        Op op = key.OrDefault();
        return mounted.Value.Find(widget).ToFin(Fail: op.InvalidInput())
            .Bind(held => op.Catch(() => {
                held.Unregister();
                _ = mounted.Swap(map => map.Remove(widget));
                return Fin.Succ(value: unit);
            }));
    }

    public void Dispose() {
        if (Interlocked.Exchange(location1: ref released, value: 1) is not 0) { return; }
        _ = toSeq(mounted.Value.Values).Iter(static widget => widget.Unregister());
        _ = channel.Writer.TryComplete();
    }
}
```

```mermaid
flowchart LR
    Hook["Pointers hook : MouseCallback — paired begin/end overrides"] -->|PointerFact channel| Consumers["drag folds · hover chrome · acquisition rails"]
    Seat["GumballSeat — SetFrom* union"] --> Rig["GumballRig — SetBaseGumball · PickGumball · UpdateGumball"]
    Rig -->|GumballEvidence Transform| Commit["document transaction rail"]
    Specs["WidgetSpec rows — grip · direction · rotation · text dot · svg · slider"] --> Host["WidgetHost — RegisterForAllDocuments / Unregister"]
    Host -->|WidgetFact channel| Consumers
    Host -->|OnDraw DrawEventArgs| Draw["draw.md Marks over ConduitFrame"]
    State["MouseState — Button · FrustumLine · IsMouseOver"] -->|typed hit facts| Host
```

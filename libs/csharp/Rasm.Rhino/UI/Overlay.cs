using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Rasm.Analysis;
using DrawingColor = System.Drawing.Color;
using GumballMode = Rhino.UI.Gumball.GumballMode;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
[Flags]
public enum GumballAxes {
    None = 0,
    TranslateX = 1, TranslateY = 2, TranslateZ = 4, TranslateXY = 8, TranslateYZ = 16, TranslateZX = 32,
    RotateX = 64, RotateY = 128, RotateZ = 256, ScaleX = 512, ScaleY = 1024, ScaleZ = 2048,
    Menu = 4096, Relocate = 8192,
    Translate = TranslateX | TranslateY | TranslateZ | TranslateXY | TranslateYZ | TranslateZX,
    Rotate = RotateX | RotateY | RotateZ,
    Scale = ScaleX | ScaleY | ScaleZ,
    All = Translate | Rotate | Scale | Menu | Relocate,
}

public enum GumballAxis { None, Free, X, Y, Z, XY, YZ, ZX }

public enum GumballVerb { None, Menu, Translate, Scale, Rotate, Extrude, Cut }

[SmartEnum<int>]
public sealed partial class MousePhase {
    public static readonly MousePhase Move = new(key: 0, viewportNative: true, cancellable: true);
    public static readonly MousePhase MoveEnd = new(key: 1, viewportNative: true, cancellable: false);
    public static readonly MousePhase Down = new(key: 2, viewportNative: true, cancellable: true);
    public static readonly MousePhase DownEnd = new(key: 3, viewportNative: true, cancellable: false);
    public static readonly MousePhase Up = new(key: 4, viewportNative: true, cancellable: true);
    public static readonly MousePhase UpEnd = new(key: 5, viewportNative: true, cancellable: false);
    public static readonly MousePhase DoubleClick = new(key: 6, viewportNative: true, cancellable: true);
    public static readonly MousePhase Enter = new(key: 7, viewportNative: true, cancellable: false);
    public static readonly MousePhase Hover = new(key: 8, viewportNative: true, cancellable: false);
    public static readonly MousePhase Leave = new(key: 9, viewportNative: true, cancellable: false);
    public static readonly MousePhase Wheel = new(key: 10, viewportNative: false, cancellable: false);
    public bool ViewportNative { get; }
    public bool Cancellable { get; }
}

[SmartEnum<int>]
public sealed partial class OverlayPhase {
    public static readonly OverlayPhase Enabled = new(key: 0, draws: false, bounding: false);
    public static readonly OverlayPhase Cull = new(key: 1, draws: false, bounding: false);
    public static readonly OverlayPhase PreDrawObjects = new(key: 2, draws: false, bounding: false);
    public static readonly OverlayPhase PreDrawObject = new(key: 3, draws: false, bounding: false);
    public static readonly OverlayPhase Foreground = new(key: 4, draws: true, bounding: false);
    public static readonly OverlayPhase Overlay = new(key: 5, draws: true, bounding: false);
    public static readonly OverlayPhase PostDraw = new(key: 6, draws: true, bounding: false);
    public static readonly OverlayPhase Bounds = new(key: 7, draws: false, bounding: true);
    public static readonly OverlayPhase ZoomBounds = new(key: 8, draws: false, bounding: true);
    public bool Draws { get; }
    public bool Bounding { get; }
}

public enum UiGradientAxis { LongestEdge, Diagonal, X, Y, Z }

// Generic union: plain abstract record + manual switch (Thinktecture [Union] mis-generates the TState
// parameter for generic unions — coding-csharp rule: generic/ref-struct sums use manual switch).
public abstract record InteractionStep<TState> {
    public sealed record PhaseGuard(InteractionGuard Guard) : InteractionStep<TState>;
    public sealed record Snap(Plane Plane, Func<MouseContext<TState>, Point3d, TState> Project) : InteractionStep<TState>;
    public sealed record Transition(Func<MouseContext<TState>, TState> Project) : InteractionStep<TState>;
    public sealed record Emit(Func<MouseContext<TState>, Fin<Unit>> Effect) : InteractionStep<TState>;
    public sealed record Debounce(TimeSpan Interval, TimeProvider Clock) : InteractionStep<TState> { internal Atom<long> Gate { get; } = Atom(0L); }   // 0L → GetElapsedTime returns uptime (passes first event); long.MinValue overflowed negative and rejected it

    private InteractionStep() { }

    internal Fin<MouseDecision<TState>> Apply(MouseContext<TState> context, MouseDecision<TState> current) =>
        this switch {
            PhaseGuard phase => phase.Guard.AdmitViewport().Bind(_ => guard(phase.Guard.Matches(context: context), (Error)new Fault.Cancelled()).ToFin().Map(_ => current)),
            Snap snap => from point in context.Project(plane: snap.Plane) select current with { State = snap.Project(arg1: context, arg2: point) },
            Transition transition => Fin.Succ(value: current with { State = transition.Project(arg: context) }),
            Emit emit => emit.Effect(arg: context).Map(_ => current),
            Debounce debounce => (debounce.Clock.GetElapsedTime(startingTimestamp: debounce.Gate.Value) >= debounce.Interval) switch {
                true => Fin.Succ(value: (debounce.Gate.Swap(_ => debounce.Clock.GetTimestamp()), current).current),
                false => Fin.Fail<MouseDecision<TState>>(error: new Fault.Cancelled()),
            },
            _ => Fin.Succ(value: current),
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct GumballAction(GumballVerb Verb, GumballAxis Axis) {
    private static readonly Seq<(GumballMode Mode, GumballAction Action)> Modes =
        Rows(GumballVerb.Menu, [(GumballMode.Menu, GumballAxis.None)])
        + Rows(GumballVerb.Translate, [(GumballMode.TranslateFree, GumballAxis.Free), (GumballMode.TranslateX, GumballAxis.X), (GumballMode.TranslateY, GumballAxis.Y), (GumballMode.TranslateZ, GumballAxis.Z), (GumballMode.TranslateXY, GumballAxis.XY), (GumballMode.TranslateYZ, GumballAxis.YZ), (GumballMode.TranslateZX, GumballAxis.ZX)])
        + Rows(GumballVerb.Scale, [(GumballMode.ScaleX, GumballAxis.X), (GumballMode.ScaleY, GumballAxis.Y), (GumballMode.ScaleZ, GumballAxis.Z), (GumballMode.ScaleXY, GumballAxis.XY), (GumballMode.ScaleYZ, GumballAxis.YZ), (GumballMode.ScaleZX, GumballAxis.ZX)])
        + Rows(GumballVerb.Rotate, [(GumballMode.RotateX, GumballAxis.X), (GumballMode.RotateY, GumballAxis.Y), (GumballMode.RotateZ, GumballAxis.Z)])
        + Rows(GumballVerb.Extrude, [(GumballMode.ExtrudeX, GumballAxis.X), (GumballMode.ExtrudeY, GumballAxis.Y), (GumballMode.ExtrudeZ, GumballAxis.Z)])
        + Rows(GumballVerb.Cut, [(GumballMode.CutX, GumballAxis.X), (GumballMode.CutY, GumballAxis.Y), (GumballMode.CutZ, GumballAxis.Z)]);

    internal static GumballAction Of(GumballMode mode) =>
        Modes.Find(row => row.Mode == mode)
            .Map(static row => row.Action)
            .IfNone(new GumballAction(Verb: GumballVerb.None, Axis: GumballAxis.None));

    public bool Active => Verb is not GumballVerb.None;
    public bool Planar => Axis is GumballAxis.XY or GumballAxis.YZ or GumballAxis.ZX;

    private static Seq<(GumballMode Mode, GumballAction Action)> Rows(GumballVerb verb, (GumballMode Mode, GumballAxis Axis)[] items) =>
        toSeq(items).Map(item => (item.Mode, new GumballAction(Verb: verb, Axis: item.Axis)));
}

public readonly record struct InteractionGuard(MousePhase Phase, Option<global::Rhino.UI.MouseButton> Button = default) {
    internal Fin<Unit> AdmitViewport() => guard(Phase.ViewportNative, Op.Of().InvalidInput()).ToFin();
    internal bool Matches<TState>(MouseContext<TState> context) =>
        context.Phase == Phase && Button.Map(button => context.MouseButton == button).IfNone(noneValue: true);
}

public readonly record struct MouseDecision<TState>(TState State, bool Cancel, Option<string> ToolTip = default);

public static class MouseDecisionSurface {
    // One decision constructor: Pass = Of(s), Stop = Of(s, cancel: true), Reject = Of(s, cancel: true, tip: ...) reconstruct from the (cancel, tip) value, so they were knobs, not cases.
    // Method-level generics on a non-generic owner: a static member on the generic MouseDecision<TState> receiver is CA1000.
    public static MouseDecision<TState> Of<TState>(TState state, bool cancel = false, Option<string> tip = default) =>
        new(State: state, Cancel: cancel, ToolTip: tip);

    public static MouseDecision<TState> Hint<TState>(TState state, string value) =>
        string.IsNullOrWhiteSpace(value: value) ? Of(state) : Of(state, cancel: false, tip: Some(value));
}

public readonly record struct MouseContext<TState>(MousePhase Phase, TState State, global::Rhino.UI.MouseCallbackEventArgs Args) : IUiInput<TState> {
    public bool Cancelled => Args.Cancel;
    public Point2d CursorLocation => global::Rhino.UI.MouseCursor.Location;
    public GumballMode GumballMode => Args.IsOverGumball();
    public bool IsOverGumball => GumballMode != GumballMode.None;
    public global::Rhino.UI.MouseButton MouseButton => Args.MouseButton;
    public bool Shift => Args.ShiftKeyDown;
    public bool Control => Args.CtrlKeyDown;
    public bool Alt => false;   // MouseCallbackEventArgs carries no Alt modifier; matches the UiInputEvent.viewportMouse projection
    public Option<RhinoView> View => Optional(Args.View);
    public Option<System.Drawing.Point> ViewportPoint => View.IsSome ? Some(Args.ViewportPoint) : Option<System.Drawing.Point>.None;   // top-left (0,0) is a valid pixel; gate on View presence, not IsEmpty
    public Option<Line> WorldLine =>
        (View.Case, ViewportPoint.Case) switch {
            (RhinoView view, System.Drawing.Point point) => Optional(view.Document).Case switch {
                RhinoDoc document => Camera.RhinoCamera.Live(document: document)
                    .RunValue(
                        operation: Camera.CameraOps.Query(query: scope => scope.FrustumLine(screenX: point.X, screenY: point.Y)),
                        target: new Camera.ViewportTarget.View(Value: view))
                    .ToOption(),
                _ => Option<Line>.None,
            },
            _ => Option<Line>.None,
        };
    public MouseDecision<TState> Stop => MouseDecisionSurface.Of(State, cancel: true);
    public MouseDecision<TState> RejectIfGumball => IsOverGumball ? MouseDecisionSurface.Of(State, cancel: true, tip: Some("Gumball active")) : MouseDecisionSurface.Of(State);   // hard-cancel the native event while a gumball owns the cursor
    public Fin<Point3d> Project(Plane plane) {
        Option<Line> worldLine = WorldLine;   // single evaluation of the camera-ray derivation
        return from line in Op.Of().Need(worldLine)
               from validPlane in guard(plane.IsValid, Op.Of().InvalidInput()).ToFin().Map(_ => plane)
               from point in guard(global::Rhino.Geometry.Intersect.Intersection.LinePlane(line: line, plane: validPlane, lineParameter: out double parameter), Op.Of().InvalidResult()).ToFin().Map(_ => line.PointAt(t: parameter))
               select point;
    }
}

public readonly record struct OverlayContext<TState>(OverlayPhase Phase, TState State, object? Args = null, bool Enabled = false) {
    public Option<TArgs> As<TArgs>() where TArgs : class => Optional(Args as TArgs);
    public Fin<TArgs> Require<TArgs>() where TArgs : class => Op.Of().Need(As<TArgs>());
}

public readonly record struct OverlayDecision(Option<BoundingBox> Bounds = default, Option<bool> Cull = default, Option<Func<DisplayPipeline, Fin<Unit>>> Draw = default, Option<Func<DrawObjectEventArgs, Fin<Unit>>> DrawObject = default) {
    public static OverlayDecision Ignore => new();
    public static Fin<OverlayDecision> Include(BoundingBox bounds) => bounds.IsValid switch { true => Fin.Succ(value: new OverlayDecision(Bounds: Some(bounds))), false => Fin.Fail<OverlayDecision>(error: Op.Of(name: nameof(OverlayDecision)).InvalidResult()) };
    public static Fin<OverlayDecision> Include(GeometryBase geometry) =>
        BoundsOf(source: geometry, op: Op.Of(name: nameof(OverlayDecision))).Bind(Include);
    public static OverlayDecision CullObject(bool cull = true) => new(Cull: Some(cull));
    public static Fin<OverlayDecision> Paint(Func<DisplayPipeline, Fin<Unit>> draw) =>
        Op.Of().Need(draw)
            .Map(static value => new OverlayDecision(Draw: Some(value)));
    public static Fin<OverlayDecision> PaintObject(Func<DrawObjectEventArgs, Fin<Unit>> draw) =>
        Op.Of().Need(draw)
            .Map(static value => new OverlayDecision(DrawObject: Some(value)));

    public static OverlayDecision Add(OverlayDecision left, OverlayDecision right) => new(
        Bounds: (left.Bounds.Case, right.Bounds.Case) switch { (BoundingBox a, BoundingBox b) => Some(BoundingBox.Union(a, b)), _ => right.Bounds | left.Bounds },
        Cull: right.Cull | left.Cull,
        Draw: Combine(left: left.Draw, right: right.Draw),
        DrawObject: Combine(left: left.DrawObject, right: right.DrawObject));

    public static OverlayDecision Fold(Seq<OverlayDecision> decisions) => decisions.Fold(Ignore, static (state, decision) => state + decision);
    public static OverlayDecision operator +(OverlayDecision left, OverlayDecision right) => Add(left: left, right: right);

    internal static Fin<BoundingBox> BoundsOf(object source, Option<Plane> frame = default, Op? op = null) {
        Op key = op.OrDefault(name: nameof(BoundsOf));
        return (frame.Case, source) switch {
            (Plane plane, GeometryBase geometry) => key.Need(geometry).Bind(value => key.AcceptValue(value: value.GetBoundingBox(plane: plane))),
            _ => Analyze.Run<object, BoundingBox>(query: AnalysisQuery.Bounds(Analysis.Bounds.AxisAligned), input: source)
                .ToFin()
                .Bind(boxes => boxes.Count switch { > 0 => Fin.Succ(value: boxes[0]), _ => Fin.Fail<BoundingBox>(error: key.InvalidResult()) })
                .Bind(box => key.AcceptValue(value: box)),
        };
    }

    private static Option<Func<TArg, Fin<Unit>>> Combine<TArg>(Option<Func<TArg, Fin<Unit>>> left, Option<Func<TArg, Fin<Unit>>> right) =>
        (left.Case, right.Case) switch {
            (Func<TArg, Fin<Unit>> a, Func<TArg, Fin<Unit>> b) =>
                Some<Func<TArg, Fin<Unit>>>(arg =>
                    (SafePaint(paint: a, arg: arg).ToValidation() &
                     SafePaint(paint: b, arg: arg).ToValidation())
                    .ToFin().Map(static _ => unit)),
            _ => right | left,
        };

    private static Fin<Unit> SafePaint<TArg>(Func<TArg, Fin<Unit>> paint, TArg arg) =>
        Op.Of(name: nameof(Combine)).Catch(() => paint(arg: arg));
}

public readonly record struct OverlayFilter(Option<ObjectType> Geometry = default, Option<ActiveSpace> Space = default, Option<Seq<Guid>> ObjectIds = default, Option<(bool On, bool CheckSubObjects)> Selection = default, Option<Seq<(RhinoViewport Viewport, bool Exclusive)>> Viewport = default, bool Unbind = false) {
    public static OverlayFilter Reset => new(Unbind: true);
    public static OverlayFilter operator +(OverlayFilter left, OverlayFilter right) => right.Unbind ? right : new(
        Geometry: right.Geometry | left.Geometry,
        Space: right.Space | left.Space,
        ObjectIds: right.ObjectIds | left.ObjectIds,
        Selection: right.Selection | left.Selection,
        Viewport: right.Viewport | left.Viewport,
        Unbind: left.Unbind || right.Unbind);

    internal Fin<Unit> Apply(DisplayConduit conduit) {
        OverlayFilter filter = this;   // struct lambdas cannot capture `this`
        return Op.Of(name: nameof(OverlayFilter)).Need(conduit).Map(valid => {
            _ = Op.SideWhen(filter.Unbind, () => {
                valid.GeometryFilter = ObjectType.AnyObject;
                valid.SpaceFilter = ActiveSpace.None;
                valid.SetSelectionFilter(on: false, checkSubObjects: false);
                valid.SetObjectIdFilter(ids: Seq<Guid>().AsIterable());
                valid.UnbindAll();
            });
            _ = filter.Geometry.Iter(value => valid.GeometryFilter = value);
            _ = filter.Space.Iter(value => valid.SpaceFilter = value);
            _ = filter.Selection.Iter(value => valid.SetSelectionFilter(on: value.On, checkSubObjects: value.CheckSubObjects));
            _ = filter.ObjectIds.Map(static ids => ids.Filter(static id => id != Guid.Empty).Distinct()).Iter(ids => valid.SetObjectIdFilter(ids: ids.AsIterable()));
            _ = filter.Viewport.Iter(bindings => {
                valid.UnbindAll();   // one UnbindAll, then bind each viewport — a one-element Seq is the common single-viewport case (MODAL_ARITY: arity is the Seq shape, not a flag)
                _ = bindings.Iter(value => _ = value.Exclusive ? Op.Side(() => valid.ExclusiveBind(viewport: value.Viewport)) : Op.Side(() => valid.Bind(viewport: value.Viewport)));
            });
            return unit;
        });
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct UiDirection(Point3d Tip, Vector3d Direction);

[StructLayout(LayoutKind.Auto)]
public readonly record struct UiGradient(Seq<ColorStop> Stops, bool Linear = true, float Repeat = 0f, UiGradientAxis Axis = UiGradientAxis.LongestEdge) {
    public static UiGradient Of(DrawingColor from, DrawingColor to, bool linear = true, UiGradientAxis axis = UiGradientAxis.LongestEdge) =>
        new(Stops: Seq(new ColorStop(from, 0d), new ColorStop(to, 1d)), Linear: linear, Axis: axis);
    internal (Point3d P1, Point3d P2) AxisOf(BoundingBox box) {
        Point3d c = box.Center;
        Vector3d d = box.Diagonal;
        (Point3d P1, Point3d P2) pick = Axis switch {
            UiGradientAxis.Diagonal => (box.Min, box.Max),
            UiGradientAxis.X => (new Point3d(box.Min.X, c.Y, c.Z), new Point3d(box.Max.X, c.Y, c.Z)),
            UiGradientAxis.Y => (new Point3d(c.X, box.Min.Y, c.Z), new Point3d(c.X, box.Max.Y, c.Z)),
            UiGradientAxis.Z => (new Point3d(c.X, c.Y, box.Min.Z), new Point3d(c.X, c.Y, box.Max.Z)),
            // longest box-edge wins; Seq order X→Y→Z makes MaxBy left-biased on a magnitude tie
            _ => Seq(
                    (Span: Math.Abs(d.X), P1: new Point3d(box.Min.X, c.Y, c.Z), P2: new Point3d(box.Max.X, c.Y, c.Z)),
                    (Span: Math.Abs(d.Y), P1: new Point3d(c.X, box.Min.Y, c.Z), P2: new Point3d(c.X, box.Max.Y, c.Z)),
                    (Span: Math.Abs(d.Z), P1: new Point3d(c.X, c.Y, box.Min.Z), P2: new Point3d(c.X, c.Y, box.Max.Z)))
                .MaxBy(static e => e.Span) switch { var longest => (longest.P1, longest.P2) },
        };
        return pick.P1.DistanceTo(pick.P2) < RhinoMath.ZeroTolerance ? (box.Min, box.Max) : pick;
    }
}

public readonly record struct UiGumballSnapshot(
    Transform PreTransform,
    Transform GumballTransform,
    Transform TotalTransform,
    GumballMode Mode,
    bool InRelocate,
    Option<global::Rhino.UI.Gumball.GumballFrame> Frame = default) {
    public GumballAction Action => GumballAction.Of(mode: Mode);

    public Fin<TGeometry> Apply<TGeometry>(TGeometry geometry, bool duplicate = true) where TGeometry : GeometryBase { Transform transform = TotalTransform; return from _ in guard(transform.IsValid, Op.Of(name: nameof(Apply)).InvalidInput()) from source in Op.Of(name: nameof(Apply)).Need(geometry) from target in duplicate switch { true => Optional(source.Duplicate() as TGeometry).ToFin(Fail: Op.Of().InvalidResult()), false => Fin.Succ(value: source) } from __ in Op.Of(name: nameof(Apply)).Confirm(success: target.Transform(xform: transform)) select target; }
    public Fin<Seq<TGeometry>> Apply<TGeometry>(IEnumerable<TGeometry> geometry, bool duplicate = true) where TGeometry : GeometryBase { UiGumballSnapshot snapshot = this; return Op.Of(name: nameof(Apply)).Need(geometry).Bind(items => toSeq(items).TraverseM(item => snapshot.Apply(geometry: item, duplicate: duplicate)).As()); }
}

public sealed record UiGumballSpec {
    private readonly Func<global::Rhino.UI.Gumball.GumballObject, Fin<Unit>> configure;

    private UiGumballSpec(Func<global::Rhino.UI.Gumball.GumballObject, Fin<Unit>> configure, global::Rhino.UI.Gumball.GumballAppearanceSettings? appearance, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> appearanceActions) {
        this.configure = configure;
        Appearance = appearanceActions.IsEmpty
            ? appearance
            : appearanceActions.Fold(
                appearance ?? new global::Rhino.UI.Gumball.GumballAppearanceSettings(),
                static (s, a) => { a(obj: s); return s; });
    }

    public static UiGumballSpec Of(
        object source,
        Option<Plane> frame = default,
        GumballAxes axes = GumballAxes.All,
        global::Rhino.UI.Gumball.GumballAppearanceSettings? appearance = null,
        params Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>[] appearanceActions) =>
        new(
            configure: gumball => Op.Of(name: nameof(UiGumballSpec)).Need(source).Bind(value => From(gumball: gumball, source: value, frame: frame)),
            appearance: appearance,
            appearanceActions: (axes == GumballAxes.All ? Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>>() : Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>>(s => axes.ApplyTo(settings: s))) + toSeq(appearanceActions));

    internal global::Rhino.UI.Gumball.GumballAppearanceSettings? Appearance { get; }

    internal Fin<Unit> Configure(global::Rhino.UI.Gumball.GumballObject gumball) =>
        Op.Of(name: nameof(UiGumballSpec)).Need(configure)
            .Bind(valid => valid(arg: gumball));

    private static Fin<Unit> From(global::Rhino.UI.Gumball.GumballObject gumball, object source, Option<Plane> frame) =>
        GumballSource.Of(source: source, frame: frame).Bind(value => value.Apply(gumball: gumball));

    private readonly record struct GumballSource(Func<global::Rhino.UI.Gumball.GumballObject, bool> ApplyNative) {
        internal Fin<Unit> Apply(global::Rhino.UI.Gumball.GumballObject gumball) =>
            Op.Of(name: nameof(UiGumballSpec)).Need(ApplyNative)
                .Bind(run => Op.Of(name: nameof(UiGumballSpec)).Confirm(success: run(arg: gumball)));

        internal static Fin<GumballSource> Of(object source, Option<Plane> frame) =>
            (frame.Case, source) switch {
                (Plane plane, BoundingBox box) => Fin.Succ(value: From(apply: g => g.SetFromBoundingBox(frame: plane, frameBoundingBox: box))),
                (Plane plane, GeometryBase geometry) => OverlayDecision.BoundsOf(source: geometry, frame: Some(plane), op: Op.Of(name: nameof(UiGumballSpec))).Map(box => From(apply: g => g.SetFromBoundingBox(frame: plane, frameBoundingBox: box))),
                (_, BoundingBox box) => Fin.Succ(value: From(apply: g => g.SetFromBoundingBox(boundingBox: box))),
                (_, Line line) => Fin.Succ(value: From(apply: g => g.SetFromLine(line: line))),
                (_, Plane plane) => Fin.Succ(value: From(apply: g => g.SetFromPlane(plane: plane))),
                (_, Arc arc) => Fin.Succ(value: From(apply: g => g.SetFromArc(arc: arc))),
                (_, Circle circle) => Fin.Succ(value: From(apply: g => g.SetFromCircle(circle: circle))),
                (_, Ellipse ellipse) => Fin.Succ(value: From(apply: g => g.SetFromEllipse(ellipse: ellipse))),
                (_, Light light) => Fin.Succ(value: From(apply: g => g.SetFromLight(light: light))),
                (_, Hatch hatch) => Fin.Succ(value: From(apply: g => g.SetFromHatch(hatch: hatch))),
                (_, Curve curve) => Fin.Succ(value: From(apply: g => g.SetFromCurve(curve: curve))),
                (_, Extrusion extrusion) => Fin.Succ(value: From(apply: g => g.SetFromExtrusion(extrusion: extrusion))),
                (_, object value) => OverlayDecision.BoundsOf(source: value, op: Op.Of(name: nameof(UiGumballSpec))).Map(box => From(apply: g => g.SetFromBoundingBox(boundingBox: box))),
                _ => Fin.Fail<GumballSource>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidInput()),
            };

        private static GumballSource From(Func<global::Rhino.UI.Gumball.GumballObject, bool> apply) =>
            new(ApplyNative: apply);
    }
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct UiRenderMode(bool Capturing, bool Printing, bool Dynamic, int NestLevel) {
    public bool Decorative => !Capturing && !Printing;
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct UiRenderState(bool OnTop = false, Option<bool> DepthWrite = default, Option<CullFaceMode> CullFace = default, Option<Transform> Model = default, bool Screen2d = false, Option<bool> ClipTest = default) {
    internal Unit Scope(DisplayPipeline pipeline, Action draw) {
        _ = Op.SideWhen(OnTop, () => pipeline.PushDepthTesting(enable: false));
        _ = Op.SideWhen(Screen2d, pipeline.Push2dProjection);   // pixel-space 2D projection; matched by PopProjection (both void on DisplayPipeline, not RhinoViewport)
        _ = DepthWrite.Iter(pipeline.PushDepthWriting);
        _ = CullFace.Iter(pipeline.PushCullFaceMode);
        _ = ClipTest.Iter(pipeline.PushDepthTesting);   // a clip-tested overlay lands as a populated Option; PushClipTesting is an obsolete alias of PushDepthTesting, nested inside the OnTop depth push so the LIFO stack stays balanced; default None is a no-op (ANTICIPATORY_COLLAPSE)
        _ = Model.Iter(pipeline.PushModelTransform);
        try { draw(); } finally {
            _ = Model.Iter(_ => pipeline.PopModelTransform());
            _ = ClipTest.Iter(_ => pipeline.PopDepthTesting());
            _ = CullFace.Iter(_ => pipeline.PopCullFaceMode());
            _ = DepthWrite.Iter(_ => pipeline.PopDepthWriting());
            _ = Op.SideWhen(Screen2d, pipeline.PopProjection);   // reverse-order pop of the 2D projection
            _ = Op.SideWhen(OnTop, pipeline.PopDepthTesting);   // Pop matches the entry Push(false) — balanced stack.
        }
        return unit;
    }
}

internal sealed class ProjectionMemo {
    private readonly Atom<(uint Epoch, HashMap<Point3d, System.Drawing.PointF> Cache)> cell = Atom((0u, HashMap<Point3d, System.Drawing.PointF>()));
    internal Fin<System.Drawing.PointF> Screen(uint epoch, Point3d world, Func<Point3d, Fin<System.Drawing.PointF>> project) {
        HashMap<Point3d, System.Drawing.PointF> live = cell.Swap(p => p.Epoch == epoch ? p : (epoch, HashMap<Point3d, System.Drawing.PointF>())).Cache;
        return live.Find(world) is { IsSome: true, Case: System.Drawing.PointF hit }
            ? Fin.Succ(value: hit)
            : project(world).Map(pt => { _ = cell.Swap(p => p.Epoch == epoch ? (p.Epoch, p.Cache.AddOrUpdate(world, pt)) : p); return pt; });
    }
}

public readonly record struct UiPreviewContext(
    RhinoDoc Document,
    OverlayPhase Phase,
    RhinoViewport Viewport,
    DisplayPipeline Display,
    Option<UiGumballSnapshot> Gumball,
    UiSpriteAtlas? Atlas = null) {
    internal ProjectionMemo? Memo { get; init; }   // conduit-supplied per-frame projection cache; internal so it stays off the public ctor surface
    internal object? Args { get; init; }           // PreDrawObject event args for per-object restyle (As<DrawObjectEventArgs>)
    public UiRenderMode Mode => new(Capturing: Display.IsInViewCapture, Printing: Display.IsPrinting, Dynamic: Display.IsDynamicDisplay, NestLevel: Display.NestLevel);
    internal float DpiScale => Display.DpiScale;
    internal UiHudLayout Layout => UiHudLayout.Of(viewport: Viewport, dpiScale: Display.DpiScale);
    public Option<T> As<T>() where T : class => Optional(Args as T);
    public Fin<double> PixelScale(Point3d at) {
        RhinoViewport viewport = Viewport;
        return Op.Of().Catch(() =>
            viewport.GetWorldToScreenScale(pointInFrustum: at, pixelsPerUnit: out double ppu) && ppu > 0d
                ? Fin.Succ(value: ppu) : Fin.Fail<double>(error: Op.Of().InvalidResult()));
    }
    public Fin<double> WorldSize(Point3d at, float pixels) =>
        PixelScale(at: at).Map(ppu => (double)pixels / ppu);   // GetWorldToScreenScale ppu is logical pixels/world-unit; pixels are logical — DpiScale would double-scale
    public Fin<System.Drawing.PointF> Screen(Point3d world) {
        RhinoDoc document = Document;
        RhinoViewport viewport = Viewport;
        Guid viewportId = viewport.Id;   // RhinoViewport.Id calls NonConstPointer per access; read once — invariant within a frame, projection runs per cache miss
        Fin<System.Drawing.PointF> Project(Point3d w) =>
            Camera.RhinoCamera.Live(document: document).RunValue(
                operation: Camera.CameraOps.Query(query: scope => scope.ScreenPoint(point: w)),
                target: new Camera.ViewportTarget.Id(Value: viewportId));
        return Memo is { } memo ? memo.Screen(epoch: viewport.ChangeCounter, world: world, project: Project) : Project(w: world);
    }
    public Fin<Unit> Label(Point3d world, string text, UiAnchor anchor = UiAnchor.TopCenter, DrawingColor? color = null) =>
        Label(subject: new Camera.CameraSubject.AtPoint(Value: world), text: text, anchor: anchor, color: color);
    public Fin<Unit> Label(Camera.CameraSubject subject, string text, UiAnchor anchor = UiAnchor.TopCenter, DrawingColor? color = null) {
        RhinoDoc document = Document;
        RhinoViewport viewport = Viewport;
        UiPreviewContext self = this;
        return from visible in Camera.RhinoCamera.Live(document: document).RunValue(
                   operation: Camera.CameraOps.Query(query: scope => scope.Visible(source: subject)),
                   target: new Camera.ViewportTarget.Id(Value: viewport.Id))
               from drawn in visible switch {
                   true => from box in subject.BoundsOf(op: Op.Of())
                           from at in self.Screen(world: box.Center)
                           from marked in self.Mark(mark: new UiMark.Text(Value: text, At: at, Color: color ?? DrawingColor.White, Anchor: anchor))
                           select marked,
                   false => Fin.Succ(value: unit),
               }
               select drawn;
    }

    internal Fin<Unit> Mark(UiMark mark) => mark.Render(surface: new UiSurface.Pipeline(Display: Display, Atlas: Atlas));
}

public readonly record struct UiPreviewScope(
    RhinoDoc Document,
    RasmOverlay<UiViewportPreview> Overlay,
    Option<UiGumball> Gumball) {
    public Fin<Unit> Set(UiViewportPreview preview) {
        RasmOverlay<UiViewportPreview> overlay = Overlay;
        RhinoDoc document = Document;
        return Op.Of().Need(preview)
            .Bind(valid => valid.Validate().Map(_ => valid))
            .Bind(valid => overlay.Transition(transition: _ => valid, document: document));
    }

    public Fin<bool> PickGumball(PickContext pick, GetPoint point) => from active in Op.Of().Need(Gumball) from validPick in Op.Of().Need(pick) from validPoint in Op.Of().Need(point) from picked in active.Pick(pick: validPick, point: validPoint) select picked;

    public Fin<bool> UpdateGumball(Point3d point, Line worldLine) =>
        from active in Op.Of().Need(Gumball)
        from _ in active.CheckKeys()
        from changed in active.Update(point: point, line: worldLine)
        select changed;
}

public readonly record struct UiPreviewStyle(
    Option<UiStroke> Stroke = default,
    Option<DrawingColor> Text = default,
    Option<OverlayPhase> Phase = default,
    Option<DisplayMaterial> Material = default,
    int Thickness = 2,
    int WireDensity = 0,
    float PointRadius = 4f,
    PointStyle PointStyle = PointStyle.Simple,
    double Transparency = 0.55,
    bool FalseColor = false,
    UiRenderState Render = default,
    double MarkerSize = 16.0,
    Option<UiGradient> Gradient = default,
    bool Dotted = false) {
    internal OverlayPhase PhaseOrDefault => Phase.IfNone(OverlayPhase.PostDraw);

    [SuppressMessage(category: "Reliability", checkId: "CA2000:Dispose objects before losing scope", Justification = "Owned default DisplayMaterial is disposed in the method's terminal finally; caller-supplied materials are not owned.")]
    internal Fin<Unit> Draw(UiPreviewContext context, object geometry) {
        UiPreviewStyle s = this;   // struct copy: nested closures cannot capture `this` on a readonly record struct
        DisplayPipeline pipeline = context.Display;
        DrawingColor stroke = s.Stroke.Map(static value => value.Color).IfNone(() => context.Document.CreateDefaultAttributes().DrawColor(context.Document));
        DrawingColor text = s.Text.IfNone(DrawingColor.White);
        (DisplayMaterial material, bool ownsMaterial) = s.Material.Case switch {
            DisplayMaterial supplied => (supplied, false),
            _ => (new DisplayMaterial(diffuse: stroke, transparency: s.Transparency), true),
        };
        Fin<Unit> Paint(Action draw) => Fin.Succ(value: s.Render.Scope(pipeline: pipeline, draw: draw));
        IEnumerable<Point3d> PolyPoints(Curve curve) {   // screen-anchored dotted sampling over the curve domain
            const int CurveSamples = 64;   // segment count → CurveSamples + 1 fence-post samples
            Interval domain = curve.Domain;
            return toSeq(Enumerable.Range(start: 0, count: CurveSamples + 1)).Map(i => curve.PointAt(t: domain.ParameterAt(normalizedParameter: (double)i / CurveSamples)));
        }
        Unit DrawCurveWithPen(Curve curve) => (s.Dotted, s.Stroke.Case) switch {
            (true, _) => Op.Side(() => pipeline.DrawDottedPolyline(points: PolyPoints(curve: curve), color: stroke, close: curve.IsClosed)),
            (false, UiStroke value) => Op.Side(() => pipeline.DrawCurve(curve: curve, pen: value.ToDisplayPen())),
            (false, _) => Op.Side(() => pipeline.DrawCurve(curve: curve, color: stroke, thickness: s.Thickness)),
        };
        Fin<Unit> Lines(Curve curve) => Paint(() => _ = DrawCurveWithPen(curve: curve));                                          // caller-owned curve — never disposed here
        Fin<Unit> Owned(Func<Curve> make) => Paint(() => { using Curve managed = make(); _ = DrawCurveWithPen(curve: managed); });  // transient curve created + disposed in scope
        Fin<Unit> Shaded(Action shade, Action wires) => Paint(() => { shade(); wires(); });                                        // param order = draw order (shade beneath wires)
        Option<Fin<Unit>> GradientPaint(UiGradient g) {
            MeshingParameters mp = context.Mode.Dynamic ? MeshingParameters.FastRenderMesh : MeshingParameters.QualityRenderMesh;
            Fin<Unit> MeshGradient(Mesh? m, BoundingBox box) =>
                m is { IsValid: true } && m.Vertices.Count > 0
                    ? Paint(() => { (Point3d p1, Point3d p2) = g.AxisOf(box); pipeline.DrawGradientMesh(mesh: m, stops: g.Stops.AsEnumerable(), point1: p1, point2: p2, linearGradient: g.Linear, repeat: g.Repeat); })
                    : Fin.Succ(value: unit);   // degenerate render mesh: the style asked for a gradient, so no-op rather than fall to solid
            Fin<Unit> Meshed(Func<Mesh?> make, BoundingBox box) =>
                Op.Of().Catch(() => { using Mesh? managed = make(); return MeshGradient(m: managed, box: box); });
            return geometry switch {
                Mesh mesh => Some(MeshGradient(m: mesh, box: mesh.GetBoundingBox(accurate: false))),   // native mesh is caller-owned — never disposed here
                Brep brep => Some(Meshed(make: () => Combine(Mesh.CreateFromBrep(brep: brep, meshingParameters: mp)), box: brep.GetBoundingBox(accurate: false))),
                SubD subd => Some(Meshed(make: () => Mesh.CreateFromSubD(subd: subd, displayDensity: s.WireDensity > 0 ? Math.Clamp(s.WireDensity, 1, 6) : 3), box: subd.GetBoundingBox(accurate: false))),
                Extrusion extrusion => Some(Meshed(make: () => Mesh.CreateFromExtrusion(extrusion: extrusion, meshingParameters: mp), box: extrusion.GetBoundingBox(accurate: false))),   // Extrusion : Surface — must precede Surface
                Surface surface => Some(Meshed(make: () => Mesh.CreateFromSurface(surface: surface, meshingParameters: mp), box: surface.GetBoundingBox(accurate: false))),
                Hatch hatch => Some(Paint(() => { (Point3d p1, Point3d p2) = g.AxisOf(hatch.GetBoundingBox(accurate: false)); pipeline.DrawGradientHatch(hatch: hatch, stops: g.Stops.AsEnumerable(), point1: p1, point2: p2, linearGradient: g.Linear, repeat: g.Repeat, boundaryThickness: s.Thickness, boundaryColor: stroke); })),
                Curve curve => Some(Paint(() => {
                    Seq<Point3d> pts = toSeq(PolyPoints(curve: curve));
                    (Point3d p1, Point3d p2) = g.AxisOf(curve.GetBoundingBox(accurate: false));
                    pipeline.DrawGradientLines(lines: pts.Zip(pts.Tail).Map(static pair => new Line(pair.First, pair.Second)).AsEnumerable(), strokeWidth: s.Thickness, stops: g.Stops.AsEnumerable(), point1: p1, point2: p2, linearGradient: g.Linear, repeat: g.Repeat);
                })),
                _ => Option<Fin<Unit>>.None,
            };
            static Mesh? Combine(Mesh[]? parts) => parts is { Length: > 0 } faces ? Append(faces) : null;
            static Mesh Append(Mesh[] faces) {
                Mesh joined = new();
                try {
                    joined.Append(meshes: faces);
                    return joined;
                } catch {
                    joined.Dispose();
                    throw;
                } finally {
                    _ = toSeq(faces).Iter(static face => face.Dispose());
                }
            }
        }
        try {
            return geometry switch {
                null => Fin.Fail<Unit>(error: Op.Of().InvalidInput()),
                _ when s.Gradient.Bind(GradientPaint).Case is Fin<Unit> painted => painted,
                Line line => Owned(() => new LineCurve(line)),
                Arc arc => Owned(() => new ArcCurve(arc)),
                Circle circle => Owned(() => new ArcCurve(circle)),
                Polyline polyline => Owned(polyline.ToNurbsCurve),
                Ellipse ellipse => Owned(() => ellipse.ToNurbsCurve()),
                Curve curve => Lines(curve),
                Mesh mesh => s.FalseColor
                    ? Paint(() => pipeline.DrawMeshFalseColors(mesh: mesh))
                    : Shaded(() => pipeline.DrawMeshShaded(mesh: mesh, material: material), () => pipeline.DrawMeshWires(mesh: mesh, color: stroke, thickness: s.Thickness)),
                Brep brep => Shaded(() => pipeline.DrawBrepShaded(brep: brep, material: material), () => pipeline.DrawBrepWires(brep: brep, color: stroke, wireDensity: s.WireDensity)),
                SubD subd => Shaded(() => pipeline.DrawSubDShaded(subd: subd, material: material), () => pipeline.DrawSubDWires(subd: subd, color: stroke, thickness: s.Thickness)),
                Ray3d ray => Paint(() => pipeline.DrawArrow(line: new Line(from: ray.Position, to: ray.Position + ray.Direction), color: stroke, screenSize: s.MarkerSize, relativeSize: 0d)),
                UiDirection direction => Paint(() => pipeline.DrawMarker(tip: direction.Tip, direction: direction.Direction, color: stroke, thickness: s.Thickness, size: s.MarkerSize, rotation: 0d)),
                Sphere sphere => Paint(() => pipeline.DrawSphere(sphere: sphere, color: stroke, thickness: s.Thickness)),
                Cone cone => Paint(() => pipeline.DrawCone(cone: cone, color: stroke, thickness: s.Thickness)),
                Cylinder cylinder => Paint(() => pipeline.DrawCylinder(cylinder: cylinder, color: stroke, thickness: s.Thickness)),
                Torus torus => Paint(() => pipeline.DrawTorus(torus: torus, color: stroke, thickness: s.Thickness)),
                Box box => Paint(() => pipeline.DrawBox(box: box, color: stroke, thickness: s.Thickness)),
                BoundingBox box => Paint(() => pipeline.DrawBox(box, stroke, s.Thickness)),
                Extrusion extrusion => Paint(() => pipeline.DrawExtrusionWires(extrusion: extrusion, color: stroke, wireDensity: s.WireDensity)),
                ClippingPlaneSurface clipping => Paint(() => pipeline.DrawClippingPlaneWires(clippingPlane: clipping, color: stroke)),
                Surface surface => Paint(() => pipeline.DrawSurface(surface: surface, wireColor: stroke, wireDensity: s.WireDensity)),
                Point point => Paint(() => pipeline.DrawPoint(point: point.Location, style: s.PointStyle, strokeColor: stroke, fillColor: stroke, radius: s.PointRadius, strokeWidth: s.Thickness, secondarySize: 0f, rotationRadians: 0f, diameterIsInPixels: true, autoScaleForDpi: true)),
                Point3d point => Paint(() => pipeline.DrawPoint(point: point, style: s.PointStyle, strokeColor: stroke, fillColor: stroke, radius: s.PointRadius, strokeWidth: s.Thickness, secondarySize: 0f, rotationRadians: 0f, diameterIsInPixels: true, autoScaleForDpi: true)),
                ConstructionPlane cp => Paint(() => pipeline.DrawConstructionPlane(constructionPlane: cp)),
                PointCloud cloud => Paint(() => pipeline.DrawPointCloud(cloud: cloud, size: s.PointRadius, color: stroke)),
                Hatch hatch => Paint(() => pipeline.DrawHatch(hatch: hatch, hatchColor: stroke, boundaryColor: stroke)),
                TextDot dot => Paint(() => pipeline.DrawDot(dot: dot, fillColor: stroke, textColor: text, borderColor: stroke)),
                TextEntity entity => Paint(() => pipeline.DrawText(text: entity, color: text)),
                AnnotationBase annotation => Paint(() => pipeline.DrawAnnotation(annotation: annotation, color: text)),
                Light light => Paint(() => pipeline.DrawLight(light: light, wireframeColor: stroke)),
                _ => Fin.Succ(value: unit),
            };
        } finally {
            _ = Op.SideWhen(ownsMaterial, material.Dispose);
        }
    }
}

public sealed record UiViewportPreview {
    private readonly Func<UiPreviewContext, Fin<Unit>> draw;
    private readonly Func<Fin<OverlayDecision>> bounds;
    private readonly Func<Fin<Unit>> validate;
    private readonly Func<OverlayPhase, bool> fires;   // declared phase gate; the ctor enforces it so a draw func can never fire off-phase

    private UiViewportPreview(Func<UiPreviewContext, Fin<Unit>> draw, Func<Fin<OverlayDecision>> bounds, Func<Fin<Unit>> validate, Func<OverlayPhase, bool> fires) {
        this.fires = fires;
        this.draw = context => fires(arg: context.Phase) ? draw(arg: context) : Fin.Succ(value: unit);   // structural guard: off-phase is a no-op
        this.bounds = bounds;
        this.validate = validate;
    }

    public static UiViewportPreview Empty { get; } =
        new(draw: static _ => Fin.Succ(value: unit), bounds: static () => Fin.Succ(value: OverlayDecision.Ignore), validate: static () => Fin.Succ(value: unit), fires: static _ => false);

    public static UiViewportPreview Of<TGeometry>(IEnumerable<TGeometry> geometry, UiPreviewStyle style = default) where TGeometry : notnull {
        Fin<Seq<object>> items = Op.Of(name: nameof(UiViewportPreview)).Need(geometry)
            .Bind(static source => toSeq(source).Map(static item => (object)item).TraverseM(item => Op.Of(name: nameof(UiViewportPreview)).Need(item)
                .Bind(static value => OverlayDecision.BoundsOf(source: value, op: Op.Of(name: nameof(UiViewportPreview))).Map(_ => value))).As())
            .Bind(static values => guard(!values.IsEmpty, Op.Of(name: nameof(UiViewportPreview)).InvalidInput()).ToFin().Map(_ => values));
        return new(
            draw: context => from active in items
                             from _ in active.TraverseM(item => style.Draw(context: context, geometry: item)).As()
                             select unit,
            bounds: () => from active in items
                          from decision in active
                .TraverseM(static item => OverlayDecision.BoundsOf(source: item, op: Op.Of(name: nameof(UiViewportPreview))).Bind(OverlayDecision.Include))
                .As()
                .Map(static decisions => decisions.Fold(OverlayDecision.Ignore, static (state, decision) => state + decision))
                          select decision,
            validate: () => items.Map(static _ => unit),
            fires: phase => phase == style.PhaseOrDefault);
    }

    public static UiViewportPreview FromSelection(Commands.CommandSelection selection, UiPreviewStyle style = default) {
        Fin<UiViewportPreview> preview =
            Op.Of().Need(selection)
                .Bind(valid => valid.Geometry<GeometryBase>())
                .Map(geometry => Of(geometry: geometry, style: style));

        return new(
            draw: context => preview.Bind(active => active.Draw(context: context)),
            bounds: () => preview.Bind(active => active.Bounds()),
            validate: () => preview.Map(static _ => unit),
            fires: phase => phase == style.PhaseOrDefault);
    }
    public static UiViewportPreview Draw(Func<UiPreviewContext, Fin<Unit>> draw, Func<Fin<OverlayDecision>> bounds, Func<OverlayPhase, bool>? fires = null) =>
        new(
            draw: context => Op.Of().Need(draw).Bind(valid => valid(arg: context)),
            bounds: () => Op.Of().Need(bounds).Bind(valid => valid()),
            validate: () => Op.Of().Need(draw)
                .Bind(_ => Op.Of().Need(bounds))
                .Map(static _ => unit),
            fires: fires ?? (static phase => phase.Draws));

    public static UiViewportPreview Hud(Func<UiPreviewContext, Fin<UiHud>> hud) =>
        Draw(
            draw: context => context.Mode.Decorative
                ? Op.Of().Need(hud).Bind(valid => valid(arg: context)).Bind(h => h.Render(surface: new UiSurface.Pipeline(Display: context.Display, Atlas: context.Atlas)))
                : Fin.Succ(value: unit),
            bounds: static () => Fin.Succ(value: OverlayDecision.Ignore),
            fires: static phase => phase == OverlayPhase.Foreground || phase == OverlayPhase.Overlay);
    public static UiViewportPreview Replace(Func<RhinoObject, Option<UiPreviewStyle>> restyle) =>
        Replace(validRestyle: Op.Of().Need(restyle));

    private static UiViewportPreview Replace(Fin<Func<RhinoObject, Option<UiPreviewStyle>>> validRestyle) =>
        new(
            draw: context => from valid in validRestyle
                             from result in context.As<DrawObjectEventArgs>().Case switch {
                                 DrawObjectEventArgs target => valid(arg: target.RhinoObject).Case switch {
                                     UiPreviewStyle style => style.Draw(context: context, geometry: target.RhinoObject.Geometry).Map(_ => { target.DrawObject = false; return unit; }),
                                     _ => Fin.Succ(value: unit),
                                 },
                                 _ => Fin.Succ(value: unit),
                             }
                             select result,
            bounds: static () => Fin.Succ(value: OverlayDecision.Ignore),
            validate: () => validRestyle.Map(static _ => unit),
            fires: static phase => phase == OverlayPhase.PreDrawObject);

    public static UiViewportPreview Add(UiViewportPreview left, UiViewportPreview right) =>
        (Optional(left).IfNone(Empty), Optional(right).IfNone(Empty)) switch {
            (UiViewportPreview a, UiViewportPreview b) => new(
                draw: context =>
                    (SafeDraw(preview: a, context: context).ToValidation() &
                     SafeDraw(preview: b, context: context).ToValidation())
                    .ToFin().Map(static _ => unit),
                bounds: () => from first in a.Bounds() from second in b.Bounds() select first + second,
                validate: () => a.Validate().Bind(_ => b.Validate()),
                fires: phase => a.Fires(phase: phase) || b.Fires(phase: phase)),
        };

    public static UiViewportPreview FromMany(Seq<UiViewportPreview> previews) =>
        previews.Fold(Empty, static (state, preview) => state + preview);

    public static UiViewportPreview operator +(UiViewportPreview left, UiViewportPreview right) =>
        Add(left: left, right: right);

    internal bool Fires(OverlayPhase phase) => fires(arg: phase);
    internal Fin<Unit> Draw(UiPreviewContext context) => draw(arg: context);
    internal Fin<OverlayDecision> Bounds() => bounds();
    internal Fin<Unit> Validate() => validate();

    internal static Fin<T> Use<T>(RhinoDoc document, UiViewportPreview preview, Option<UiGumballSpec> gumball, Func<UiPreviewScope, Fin<T>> run) =>
        from validDocument in Op.Of().Need(document)
        from validPreview in Op.Of().Need(preview).Bind(valid => valid.Validate().Map(_ => valid))
        from validRun in Op.Of().Need(run)
        from result in gumball.Case switch {
            UiGumballSpec spec => UiGumball.Use(
                document: validDocument,
                spec: spec,
                run: active => new Conduit(document: validDocument, initial: validPreview, gumball: () => Some(active.Snapshot)).Use(
                    document: validDocument,
                    run: overlay => validRun(arg: new UiPreviewScope(Document: validDocument, Overlay: overlay, Gumball: Some(active))))),
            _ => new Conduit(document: validDocument, initial: validPreview, gumball: static () => Option<UiGumballSnapshot>.None).Use(
                document: validDocument,
                run: overlay => validRun(arg: new UiPreviewScope(Document: validDocument, Overlay: overlay, Gumball: Option<UiGumball>.None))),
        }
        select result;

    private static Fin<Unit> SafeDraw(UiViewportPreview preview, UiPreviewContext context) =>
        Op.Of(name: nameof(Add)).Catch(() => preview.Draw(context: context));

    private sealed class Conduit(RhinoDoc document, UiViewportPreview initial, Func<Option<UiGumballSnapshot>> gumball) : RasmOverlay<UiViewportPreview>(initial) {
        private readonly UiSpriteAtlas atlas = new();
        private readonly ProjectionMemo memo = new();   // per-conduit world->screen cache threaded into each frame's context
        protected override Fin<OverlayDecision> Change(OverlayContext<UiViewportPreview> context) =>
            context.Phase.Bounding
                ? context.State.Bounds()
                : context.Phase == OverlayPhase.PreDrawObject
                    ? context.State.Fires(phase: OverlayPhase.PreDrawObject)
                        ? context.Require<DrawObjectEventArgs>()
                            .Bind(_ => OverlayDecision.PaintObject(draw: target => context.State.Draw(context: new UiPreviewContext(
                                Document: document,
                                Phase: context.Phase,
                                Viewport: target.Viewport,
                                Display: target.Display,
                                Gumball: gumball(),
                                Atlas: atlas) { Memo = memo, Args = target })))
                        : Fin.Succ(value: OverlayDecision.Ignore)
                    : context.Phase.Draws && context.State.Fires(phase: context.Phase)
                        ? context.Require<DrawEventArgs>()
                            .Bind(args => OverlayDecision.Paint(draw: display => context.State.Draw(context: new UiPreviewContext(
                                Document: document,
                                Phase: context.Phase,
                                Viewport: args.Viewport,
                                Display: display,
                                Gumball: gumball(),
                                Atlas: atlas) { Memo = memo })))
                        : Fin.Succ(value: OverlayDecision.Ignore);
        protected override void Dispose(bool disposing) {
            if (disposing) {
                atlas.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

public sealed record UiViewportInteraction<TState>(
    TState Initial,
    UiViewportPreview Preview,
    Func<MouseContext<TState>, Fin<MouseDecision<TState>>> Mouse,
    Option<UiGumballSpec> Gumball = default) {
    internal Fin<T> Use<T>(RhinoDoc document, Func<UiPreviewScope, Fin<T>> run) =>
        from validRun in Op.Of().Need(run)
        from validMouse in Op.Of().Need(Mouse)
        from result in UiViewportPreview.Use(document: document, preview: Preview, gumball: Gumball, run: scope => new Callback(initial: Initial, change: validMouse).Use(run: _ => validRun(arg: scope)))
        select result;

    private sealed class Callback(TState initial, Func<MouseContext<TState>, Fin<MouseDecision<TState>>> change) : global::Rhino.UI.MouseCallback, IDisposable {
        private readonly Atom<TState> state = Atom(initial);
        private bool ownsToolTip;
        private bool disposed;

        internal Fin<T> Use<T>(Func<Callback, Fin<T>> run) =>
            from validRun in Op.Of().Need(run)
            from _ in guard(!disposed, Op.Of().InvalidInput())
            from result in RhinoUi.Protect(valid: () => {
                Enabled = true;
                try { return validRun(arg: this); } finally { Dispose(); }
            })
            select result;

        public void Dispose() {
            _ = disposed switch {
                true => unit,
                false => Disable(),
            };
            disposed = true;
            GC.SuppressFinalize(obj: this);
        }

        protected override void OnMouseMove(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Move, args: e);
        protected override void OnEndMouseMove(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.MoveEnd, args: e);
        protected override void OnMouseDown(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Down, args: e);
        protected override void OnEndMouseDown(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.DownEnd, args: e);
        protected override void OnMouseUp(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Up, args: e);
        protected override void OnEndMouseUp(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.UpEnd, args: e);
        protected override void OnMouseDoubleClick(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.DoubleClick, args: e);
        protected override void OnMouseEnter(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Enter, args: e);
        protected override void OnMouseHover(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Hover, args: e);
        protected override void OnMouseLeave(global::Rhino.UI.MouseCallbackEventArgs e) => _ = Apply(phase: MousePhase.Leave, args: e);

        private Unit Apply(MousePhase phase, global::Rhino.UI.MouseCallbackEventArgs args) {
            MouseContext<TState> context = new(Phase: phase, State: state.Value, Args: args);
            return RhinoUi.Protect(valid: () => change(arg: context))
                .Map(decision => {
                    _ = state.Swap(_ => decision.State);
                    _ = decision.ToolTip.Case switch {
                        string tooltip => Op.Side(() => {
                            ownsToolTip = true;
                            global::Rhino.UI.MouseCursor.SetToolTip(tooltip: tooltip);
                        }),
                        _ when ownsToolTip => Op.Side(() => {
                            ownsToolTip = false;
                            global::Rhino.UI.MouseCursor.SetToolTip(tooltip: string.Empty);
                        }),
                        _ => unit,
                    };
                    args.Cancel = context.Phase.Cancellable switch {
                        true => args.Cancel || decision.Cancel,
                        _ => args.Cancel,
                    };
                    return unit;
                })
                .Match(Succ: static _ => unit, Fail: error => error is Fault.Cancelled ? unit : ((Func<Unit>)(() => {
                    RhinoApp.WriteLine(message: $"{nameof(UiViewportInteraction<>)}: {error}");
                    return Disable();
                }))());
        }

        private Unit Disable() {
            _ = ownsToolTip switch {
                true => Op.Side(static () => global::Rhino.UI.MouseCursor.SetToolTip(tooltip: string.Empty)),
                false => unit,
            };
            ownsToolTip = false;
            Enabled = false;
            return unit;
        }
    }
}

public static partial class UiViewportRequest {
    public static UiIntent<T> Preview<T>(UiViewportPreview preview, Func<UiPreviewScope, Fin<T>> run, Option<UiGumballSpec> gumball = default, bool interactive = true) =>
        UiIntent.OfScope(run: scope => UiViewportPreview.Use(document: scope.Document, preview: preview, gumball: gumball, run: run), interactive: interactive);

    public static UiIntent<T> Interaction<TState, T>(UiViewportInteraction<TState> interaction, Func<UiPreviewScope, Fin<T>> run) =>
        UiIntent.OfScope(run: scope =>
            from active in Op.Of().Need(interaction)
            from result in active.Use(document: scope.Document, run: run)
            select result, interactive: true);
}

// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class RasmOverlay<TState>(TState initial) : DisplayConduit, IDisposable {
    private readonly Atom<TState> state = Atom(initial);
    private bool disposed;

    public TState State => state.Value;

    public Fin<Unit> Transition(Func<TState, TState> transition, RhinoDoc? document = null) =>
        Live(op: Op.Of(), body: () => Op.Of().Need(transition)
            .Map(apply => { _ = state.Swap(f: apply); _ = Optional(document).Iter(static doc => doc.Views.Redraw()); return unit; }));

    public Fin<Unit> Enable(bool enabled = true, RhinoDoc? document = null) =>
        Live(op: Op.Of(), body: () => Fin.Succ(value: Op.Side(() => { Enabled = enabled; _ = Optional(document).Iter(static doc => doc.Views.Redraw()); })));

    public Fin<Unit> Filter(OverlayFilter filter, RhinoDoc? document = null) =>
        Live(op: Op.Of(), body: () => filter.Apply(conduit: this).Map(_ => { _ = Optional(document).Iter(static doc => doc.Views.Redraw()); return unit; }));

    public Fin<T> Use<T>(RhinoDoc document, Func<RasmOverlay<TState>, Fin<T>> run) =>
        from validDocument in Op.Of().Need(document)
        from validRun in Op.Of().Need(run)
        from result in Live(op: Op.Of(), body: () => RhinoUi.Protect(valid: () => {
            Enabled = true;
            validDocument.Views.Redraw();
            try { return validRun(arg: this); } finally { Dispose(); validDocument.Views.Redraw(); }
        }))
        select result;

    public void Dispose() {
        Dispose(disposing: true);
        GC.SuppressFinalize(obj: this);
    }

    protected virtual void Dispose(bool disposing) {
        _ = disposing;
        _ = disposed switch {
            true => unit,
            false => Disable(),
        };
        disposed = true;
    }

    protected virtual Fin<OverlayDecision> Change(OverlayContext<TState> context) =>
        Fin.Succ(value: OverlayDecision.Ignore);

    protected sealed override void OnEnable(bool enable) =>
        _ = Apply(phase: OverlayPhase.Enabled, args: null, enabled: enable);

    protected sealed override void ObjectCulling(CullObjectEventArgs e) =>
        _ = Apply(phase: OverlayPhase.Cull, args: e);

    protected sealed override void PreDrawObjects(DrawEventArgs e) =>
        _ = Apply(phase: OverlayPhase.PreDrawObjects, args: e);

    protected sealed override void PreDrawObject(DrawObjectEventArgs e) =>
        _ = Apply(phase: OverlayPhase.PreDrawObject, args: e);

    protected sealed override void DrawForeground(DrawEventArgs e) =>
        _ = Apply(phase: OverlayPhase.Foreground, args: e);

    protected sealed override void DrawOverlay(DrawEventArgs e) =>
        _ = Apply(phase: OverlayPhase.Overlay, args: e);

    protected sealed override void PostDrawObjects(DrawEventArgs e) =>
        _ = Apply(phase: OverlayPhase.PostDraw, args: e);

    protected sealed override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e) =>
        _ = Apply(phase: OverlayPhase.Bounds, args: e);

    protected sealed override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) =>
        _ = Apply(phase: OverlayPhase.ZoomBounds, args: e);

    private Fin<T> Live<T>(Op op, Func<Fin<T>> body) =>
        from _ in guard(!disposed, op.InvalidInput())
        from result in body()
        select result;

    private Unit Disable() {
        Enabled = false;
        return unit;
    }

    private Fin<Unit> Apply(OverlayPhase phase, object? args, bool enabled = false) =>
        RhinoUi.Protect(valid: () =>
            Change(context: new OverlayContext<TState>(Phase: phase, State: State, Args: args, Enabled: enabled))
                .Bind(decision => ((Optional(args as DrawEventArgs).Case, Optional(args as DrawObjectEventArgs).Case, decision.Draw.Case, decision.DrawObject.Case) switch {
                    (_, DrawObjectEventArgs target, _, Func<DrawObjectEventArgs, Fin<Unit>> paint) => paint(arg: target),
                    (DrawEventArgs target, _, Func<DisplayPipeline, Fin<Unit>> paint, _) => paint(arg: target.Display),
                    _ => Fin.Succ(value: unit),
                }).Map(_ => {
                    _ = Optional(args as CalculateBoundingBoxEventArgs).Iter(target => decision.Bounds.Iter(box => target.IncludeBoundingBox(box)));
                    _ = Optional(args as CullObjectEventArgs).Iter(target => decision.Cull.Iter(value => target.CullObject = value));
                    return unit;
                })));
}

public sealed class UiGumball : IDisposable {
    private readonly RhinoDoc document;
    private readonly global::Rhino.UI.Gumball.GumballDisplayConduit conduit;
    private bool disposed;

    private UiGumball(RhinoDoc document, global::Rhino.UI.Gumball.GumballDisplayConduit conduit) {
        this.document = document;
        this.conduit = conduit;
    }

    public UiGumballSnapshot Snapshot =>
        new(
            PreTransform: conduit.PreTransform,
            GumballTransform: conduit.GumballTransform,
            TotalTransform: conduit.TotalTransform,
            Mode: conduit.PickResult.Mode,
            InRelocate: conduit.InRelocate,
            Frame: Some(conduit.Gumball.Frame));   // live frame plane/scale-mode (downstream constraint/relocate reads it without re-deriving)

    public Fin<bool> Pick(PickContext pick, GetPoint point) =>
        from _ in Live(op: Op.Of(name: nameof(Pick)))
        from validPick in Op.Of(name: nameof(Pick)).Need(pick)
        from validPoint in Op.Of(name: nameof(Pick)).Need(point)
        from picked in RhinoUi.Protect(valid: () => Fin.Succ(value: conduit.PickGumball(pickContext: validPick, getPoint: validPoint)))
        select picked;

    public Fin<bool> Update(Point3d point, Line line) =>
        Drive(op: Op.Of(), valid: point.IsValid && line.IsValid, update: () => conduit.UpdateGumball(point: point, worldLine: line));

    public Fin<bool> Update(Plane frame) =>
        Drive(op: Op.Of(), valid: frame.IsValid, update: () => conduit.UpdateGumball(frame: frame));
    public Fin<Unit> Constrain(Transform pre) =>
        from _ in Live(op: Op.Of(name: nameof(Constrain)))
        from __ in guard(pre.IsValid, Op.Of(name: nameof(Constrain)).InvalidInput())
        from done in RhinoUi.Protect(valid: () => Fin.Succ(value: Op.Side(() => { conduit.PreTransform = pre; document.Views.Redraw(); })))
        select done;

    public Fin<Unit> Reconfigure(UiGumballSpec spec) =>
        from _ in Live(op: Op.Of(name: nameof(Reconfigure)))
        from valid in Op.Of(name: nameof(Reconfigure)).Need(spec)
        from result in RhinoUi.Protect(valid: () => {
            global::Rhino.UI.Gumball.GumballObject source = new();
            try {
                return valid.Configure(gumball: source).Map(_ => {
                    conduit.SetBaseGumball(gumball: source, appearanceSettings: valid.Appearance);
                    document.Views.Redraw();
                    return unit;
                });
            } finally {
                source.Dispose();
            }
        })
        select result;

    public Fin<Unit> CheckKeys() =>
        from _ in Live(op: Op.Of(name: nameof(CheckKeys)))
        from checkedKeys in RhinoUi.Protect(valid: () => Fin.Succ(value: Op.Side(conduit.CheckShiftAndControlKeys)))
        select checkedKeys;

    public void Dispose() {
        _ = disposed switch {
            true => unit,
            false => Release(),
        };
        disposed = true;
        GC.SuppressFinalize(obj: this);
    }

    internal static Fin<T> Use<T>(RhinoDoc document, UiGumballSpec spec, Func<UiGumball, Fin<T>> run) =>
        from validDocument in Op.Of(name: nameof(UiGumball)).Need(document)
        from validSpec in Op.Of(name: nameof(UiGumball)).Need(spec)
        from scope in RhinoUi.Protect(valid: () => Create(document: validDocument, spec: validSpec))
        from result in RhinoUi.Protect(valid: () => {
            try { return run(arg: scope); } finally { scope.Dispose(); }
        })
        select result;

    private Fin<Unit> Live(Op op) =>
        guard(
            !disposed && document is { IsAvailable: true, IsClosing: false, IsInitializing: false, IsOpening: false, IsCreating: false },
            op.InvalidInput()).ToFin();

    private Fin<bool> Drive(Op op, bool valid, Func<bool> update) =>
        from _ in Live(op: op)
        from __ in guard(valid, op.InvalidInput())
        from changed in op.Catch(() => { bool c = update(); document.Views.Redraw(); return Fin.Succ(value: c); })
        select changed;

    private static Fin<UiGumball> Create(RhinoDoc document, UiGumballSpec spec) {
        global::Rhino.UI.Gumball.GumballObject? nativeSource = new();
        try {
            return spec.Configure(gumball: nativeSource).Bind(_ => RhinoUi.Protect(valid: () => {
                global::Rhino.UI.Gumball.GumballDisplayConduit? nativeConduit = new(document.ActiveSpace);
                try {
                    nativeConduit.SetBaseGumball(gumball: nativeSource, appearanceSettings: spec.Appearance);
                    nativeConduit.Enabled = true;
                    document.Views.Redraw();
                    UiGumball scope = new(document: document, conduit: nativeConduit);
                    nativeConduit = null;
                    return Fin.Succ(value: scope);
                } finally {
                    nativeConduit?.Dispose();
                }
            }));
        } finally {
            nativeSource?.Dispose();
        }
    }

    private Unit Release() {
        try {
            conduit.Enabled = false;
            document.Views.Redraw();
        } finally {
            conduit.Dispose();
        }
        return unit;
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class GumballAxesProjection {
    internal static void ApplyTo(this GumballAxes axes, global::Rhino.UI.Gumball.GumballAppearanceSettings settings) {
        bool H(GumballAxes flag) => (axes & flag) == flag;
        settings.TranslateXEnabled = H(GumballAxes.TranslateX); settings.TranslateYEnabled = H(GumballAxes.TranslateY); settings.TranslateZEnabled = H(GumballAxes.TranslateZ);
        settings.TranslateXYEnabled = H(GumballAxes.TranslateXY); settings.TranslateYZEnabled = H(GumballAxes.TranslateYZ); settings.TranslateZXEnabled = H(GumballAxes.TranslateZX);
        settings.RotateXEnabled = H(GumballAxes.RotateX); settings.RotateYEnabled = H(GumballAxes.RotateY); settings.RotateZEnabled = H(GumballAxes.RotateZ);
        settings.ScaleXEnabled = H(GumballAxes.ScaleX); settings.ScaleYEnabled = H(GumballAxes.ScaleY); settings.ScaleZEnabled = H(GumballAxes.ScaleZ);
        settings.MenuEnabled = H(GumballAxes.Menu); settings.RelocateEnabled = H(GumballAxes.Relocate);
    }
}

public static class InteractionStep {   // non-generic host (CA1000): InteractionStep.Compose(steps) infers TState
    public static Func<MouseContext<TState>, Fin<MouseDecision<TState>>> Compose<TState>(Seq<InteractionStep<TState>> steps) =>
        context => steps.Fold(
            Fin.Succ(value: context.Pass),
            (accumulator, step) => accumulator.Bind(decision => step.Apply(context: context, current: decision)));
}

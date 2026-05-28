using Rasm.Analysis;
using DrawingColor = System.Drawing.Color;

namespace Rasm.Rhino.UI;

// --- [TYPES] ------------------------------------------------------------------------------
public enum MousePhase { Move, MoveEnd, Down, DownEnd, Up, UpEnd, DoubleClick, Enter, Hover, Leave }
public enum OverlayPhase { Enabled, Cull, PreDrawObjects, PreDrawObject, Foreground, Overlay, PostDraw, Bounds, ZoomBounds }

// --- [MODELS] -----------------------------------------------------------------------------
public readonly record struct MouseContext<TState>(MousePhase Phase, TState State, global::Rhino.UI.MouseCallbackEventArgs Args) {
    public bool Cancelled => Args.Cancel;
    public bool CanCancelNative => Phase is MousePhase.Move or MousePhase.Down or MousePhase.Up or MousePhase.DoubleClick;
    public Point2d CursorLocation => global::Rhino.UI.MouseCursor.Location;
    public global::Rhino.UI.Gumball.GumballMode GumballMode => Args.IsOverGumball();
    public bool IsOverGumball => GumballMode != global::Rhino.UI.Gumball.GumballMode.None;
    public global::Rhino.UI.MouseButton MouseButton => Args.MouseButton;
    public bool Shift => Args.ShiftKeyDown;
    public bool Control => Args.CtrlKeyDown;
    public Option<RhinoView> View => Optional(Args.View);
    public Option<System.Drawing.Point> ViewportPoint => Args.ViewportPoint switch { { IsEmpty: false } point => Some(point), _ => Option<System.Drawing.Point>.None };
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
    public MouseDecision<TState> Pass => new(State: State, Cancel: false);
    public MouseDecision<TState> Stop => new(State: State, Cancel: true);
    public MouseDecision<TState> Next(TState state, bool cancel = false, Option<string> toolTip = default) =>
        new(State: state, Cancel: cancel, ToolTip: toolTip);
    public MouseDecision<TState> Hint(string value) =>
        string.IsNullOrWhiteSpace(value: value) switch {
            false => new MouseDecision<TState>(State: State, Cancel: false, ToolTip: Some(value)),
            true => Pass,
        };
    public Fin<Point3d> Project(Plane plane) => from line in Op.Of(name: nameof(Project)).Need(WorldLine) from validPlane in plane.IsValid switch { true => Fin.Succ(value: plane), false => Fin.Fail<Plane>(error: Op.Of(name: nameof(Project)).InvalidInput()) } from point in global::Rhino.Geometry.Intersect.Intersection.LinePlane(line: line, plane: validPlane, lineParameter: out double parameter) switch { true => Fin.Succ(value: line.PointAt(t: parameter)), false => Fin.Fail<Point3d>(error: Op.Of(name: nameof(Project)).InvalidResult()) } select point;
}

public readonly record struct MouseDecision<TState>(TState State, bool Cancel, Option<string> ToolTip = default);

public readonly record struct OverlayContext<TState>(OverlayPhase Phase, TState State, object? Args = null, bool Enabled = false) {
    public Option<TArgs> As<TArgs>() where TArgs : class => Optional(Args as TArgs);
    public Fin<TArgs> Require<TArgs>() where TArgs : class => Op.Of(name: nameof(Require)).Need(As<TArgs>());
}

public readonly record struct OverlayDecision(Option<BoundingBox> Bounds = default, Option<bool> Cull = default, Option<Func<DisplayPipeline, Fin<Unit>>> Draw = default, Option<Func<DrawObjectEventArgs, Fin<Unit>>> DrawObject = default) {
    public static OverlayDecision Ignore => new();
    public static Fin<OverlayDecision> Include(BoundingBox bounds) => bounds.IsValid switch { true => Fin.Succ(value: new OverlayDecision(Bounds: Some(bounds))), false => Fin.Fail<OverlayDecision>(error: Op.Of(name: nameof(OverlayDecision)).InvalidResult()) };
    public static Fin<OverlayDecision> Include(GeometryBase geometry) =>
        BoundsOf(source: geometry, op: Op.Of(name: nameof(OverlayDecision))).Bind(Include);
    public static OverlayDecision CullObject(bool cull = true) => new(Cull: Some(cull));
    public static Fin<OverlayDecision> Paint(Func<DisplayPipeline, Fin<Unit>> draw) =>
        Op.Of(name: nameof(Paint)).Need(draw)
            .Map(static value => new OverlayDecision(Draw: Some(value)));
    public static Fin<OverlayDecision> PaintObject(Func<DrawObjectEventArgs, Fin<Unit>> draw) =>
        Op.Of(name: nameof(PaintObject)).Need(draw)
            .Map(static value => new OverlayDecision(DrawObject: Some(value)));
    public static OverlayDecision operator +(OverlayDecision left, OverlayDecision right) => Add(left: left, right: right);
    public static OverlayDecision Add(OverlayDecision left, OverlayDecision right) => new(
        Bounds: (left.Bounds.Case, right.Bounds.Case) switch { (BoundingBox a, BoundingBox b) => Some(BoundingBox.Union(a, b)), _ => right.Bounds | left.Bounds },
        Cull: right.Cull | left.Cull,
        Draw: Combine(left: left.Draw, right: right.Draw),
        DrawObject: Combine(left: left.DrawObject, right: right.DrawObject));

    private static Option<Func<TArg, Fin<Unit>>> Combine<TArg>(Option<Func<TArg, Fin<Unit>>> left, Option<Func<TArg, Fin<Unit>>> right) =>
        (left.Case, right.Case) switch {
            (Func<TArg, Fin<Unit>> a, Func<TArg, Fin<Unit>> b) => Some<Func<TArg, Fin<Unit>>>(arg => a(arg: arg).Bind(_ => b(arg: arg))),
            _ => right | left,
        };
    internal static Fin<BoundingBox> BoundsOf(object source, Op op) =>
        Analyze.Run(operation: Analyze.Bounds<object, BoundingBox>(aspect: Analysis.Bounds.AxisAligned), input: source)
            .ToFin()
            .Bind(boxes => boxes.Count switch { > 0 => Fin.Succ(value: boxes[0]), _ => Fin.Fail<BoundingBox>(error: op.InvalidResult()) })
            .Bind(box => box.IsValid ? Fin.Succ(value: box) : Fin.Fail<BoundingBox>(error: op.InvalidResult()));
}

public readonly record struct OverlayFilter(Option<ObjectType> Geometry = default, Option<ActiveSpace> Space = default, Option<Seq<Guid>> ObjectIds = default, Option<(bool On, bool CheckSubObjects)> Selection = default, Option<(RhinoViewport Viewport, bool Exclusive)> Viewport = default, bool Unbind = false) {
    public static OverlayFilter Reset => new(Unbind: true);

    internal Fin<Unit> Apply(DisplayConduit conduit) {
        Option<ObjectType> geometry = Geometry; Option<ActiveSpace> space = Space; Option<Seq<Guid>> objectIds = ObjectIds.Map(static ids => ids.Filter(static id => id != Guid.Empty).Distinct()); Option<(bool On, bool CheckSubObjects)> selection = Selection; Option<(RhinoViewport Viewport, bool Exclusive)> viewport = Viewport;
        bool unbind = Unbind;
        return Op.Of(name: nameof(OverlayFilter)).Need(conduit)
            .Map(valid => {
                _ = unbind switch {
                    true => Op.Side(() => {
                        valid.GeometryFilter = ObjectType.AnyObject;
                        valid.SpaceFilter = ActiveSpace.None;
                        valid.SetSelectionFilter(on: false, checkSubObjects: false);
                        valid.SetObjectIdFilter(ids: Seq<Guid>().AsIterable());
                        valid.UnbindAll();
                    }),
                    false => unit,
                };
                _ = geometry.Iter(value => valid.GeometryFilter = value);
                _ = space.Iter(value => valid.SpaceFilter = value);
                _ = selection.Iter(value => valid.SetSelectionFilter(on: value.On, checkSubObjects: value.CheckSubObjects));
                _ = objectIds.Iter(ids => valid.SetObjectIdFilter(ids: ids.AsIterable()));
                _ = unbind switch {
                    true => unit,
                    false => viewport.Map(value => {
                        _ = Op.Side(() => valid.UnbindAll());
                        _ = value.Exclusive switch {
                            true => Op.Side(() => valid.ExclusiveBind(viewport: value.Viewport)),
                            false => Op.Side(() => valid.Bind(viewport: value.Viewport)),
                        };
                        return unit;
                    }).IfNone(unit),
                };
                return unit;
            });
    }

}

public readonly record struct UiPreviewStyle(
    Option<DrawingColor> Stroke = default,
    Option<DrawingColor> Text = default,
    Option<OverlayPhase> Phase = default,
    Option<DisplayMaterial> Material = default,
    int Thickness = 2,
    int WireDensity = 0,
    float PointRadius = 4f,
    PointStyle PointStyle = PointStyle.Simple,
    double Transparency = 0.55) {
    internal OverlayPhase PhaseOrDefault => Phase.IfNone(OverlayPhase.PostDraw);

    internal Fin<Unit> Draw(UiPreviewContext context, object geometry) {
        int thickness = Thickness;
        int wireDensity = WireDensity;
        float pointRadius = PointRadius;
        PointStyle pointStyle = PointStyle;
        double transparency = Transparency;
        DrawingColor stroke = Stroke.IfNone(() => context.Document.CreateDefaultAttributes().DrawColor(context.Document));
        DrawingColor text = Text.IfNone(DrawingColor.White);
        DisplayMaterial material = Material.IfNone(() => new DisplayMaterial(diffuse: stroke, transparency: transparency));
        return (context.Display, geometry) switch {
            (DisplayPipeline pipeline, object value) => value switch {
                Mesh mesh => Paint(() => { pipeline.DrawMeshShaded(mesh: mesh, material: material); pipeline.DrawMeshWires(mesh: mesh, color: stroke, thickness: thickness); }),
                Brep brep => Paint(() => { pipeline.DrawBrepShaded(brep: brep, material: material); pipeline.DrawBrepWires(brep: brep, color: stroke, wireDensity: wireDensity); }),
                Curve curve => Paint(() => pipeline.DrawCurve(curve: curve, color: stroke, thickness: thickness)),
                Extrusion extrusion => Paint(() => pipeline.DrawExtrusionWires(extrusion: extrusion, color: stroke, wireDensity: wireDensity)),
                ClippingPlaneSurface clipping => Paint(() => pipeline.DrawClippingPlaneWires(clippingPlane: clipping, color: stroke)),
                Surface surface => Paint(() => pipeline.DrawSurface(surface: surface, wireColor: stroke, wireDensity: wireDensity)),
                Point point => Paint(() => pipeline.DrawPoint(point: point.Location, style: pointStyle, radius: pointRadius, color: stroke)),
                Point3d point => Paint(() => pipeline.DrawPoint(point: point, style: pointStyle, radius: pointRadius, color: stroke)),
                Line line => Paint(() => pipeline.DrawLine(line: line, color: stroke, thickness: thickness)),
                Polyline polyline => Paint(() => pipeline.DrawPolyline(polyline: polyline, color: stroke, thickness: thickness)),
                Arc arc => Paint(() => pipeline.DrawArc(arc: arc, color: stroke, thickness: thickness)),
                Circle circle => Paint(() => pipeline.DrawCircle(circle: circle, color: stroke, thickness: thickness)),
                Box box => Paint(() => pipeline.DrawBox(box: box, color: stroke, thickness: thickness)),
                BoundingBox box => Paint(() => pipeline.DrawBox(box, stroke, thickness)),
                Sphere sphere => Paint(() => pipeline.DrawSphere(sphere: sphere, color: stroke, thickness: thickness)),
                Ellipse ellipse => Paint(() => { using Curve curve = ellipse.ToNurbsCurve(); pipeline.DrawCurve(curve: curve, color: stroke, thickness: thickness); }),
                PointCloud cloud => Paint(() => pipeline.DrawPointCloud(cloud: cloud, size: pointRadius, color: stroke)),
                SubD subd => Paint(() => { pipeline.DrawSubDShaded(subd: subd, material: material); pipeline.DrawSubDWires(subd: subd, color: stroke, thickness: thickness); }),
                Hatch hatch => Paint(() => pipeline.DrawHatch(hatch: hatch, hatchColor: stroke, boundaryColor: stroke)),
                TextDot dot => Paint(() => pipeline.DrawDot(dot: dot, fillColor: stroke, textColor: text, borderColor: stroke)),
                TextEntity entity => Paint(() => pipeline.DrawText(text: entity, color: text)),
                AnnotationBase annotation => Paint(() => pipeline.DrawAnnotation(annotation: annotation, color: text)),
                Light light => Paint(() => pipeline.DrawLight(light: light, wireframeColor: stroke)),
                _ => Fin.Succ(value: unit),
            },
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(Draw)).InvalidInput()),
        };
    }

    private static Fin<Unit> Paint(Action draw) => Fin.Succ(value: Op.Side(draw));
}

public readonly record struct UiPreviewContext(
    RhinoDoc Document,
    OverlayPhase Phase,
    RhinoViewport Viewport,
    DisplayPipeline Display,
    Option<UiGumballSnapshot> Gumball);

public readonly record struct UiPreviewScope(
    RhinoDoc Document,
    RasmOverlay<UiViewportPreview> Overlay,
    Option<UiGumball> Gumball) {
    public Fin<Unit> Set(UiViewportPreview preview) {
        RasmOverlay<UiViewportPreview> overlay = Overlay;
        RhinoDoc document = Document;
        return Op.Of(name: nameof(Set)).Need(preview)
            .Bind(valid => valid.Validate().Map(_ => valid))
            .Bind(valid => overlay.Transition(transition: _ => valid, document: document));
    }

    public Fin<bool> UpdateGumball(Point3d point, Line worldLine) =>
        from active in Op.Of(name: nameof(UpdateGumball)).Need(Gumball)
        from _ in active.CheckKeys()
        from changed in active.Update(point: point, line: worldLine)
        select changed;

    public Fin<bool> PickGumball(PickContext pick, GetPoint point) => from active in Op.Of(name: nameof(PickGumball)).Need(Gumball) from validPick in Op.Of(name: nameof(PickGumball)).Need(pick) from validPoint in Op.Of(name: nameof(PickGumball)).Need(point) from picked in active.Pick(pick: validPick, point: validPoint) select picked;
}

public sealed record UiViewportInteraction<TState>(
    TState Initial,
    UiViewportPreview Preview,
    Func<MouseContext<TState>, Fin<MouseDecision<TState>>> Mouse,
    Option<UiGumballSpec> Gumball = default) {
    internal Fin<T> Use<T>(RhinoDoc document, Func<UiPreviewScope, Fin<T>> run) =>
        from validRun in Op.Of(name: nameof(Use)).Need(run)
        from validMouse in Op.Of(name: nameof(Use)).Need(Mouse)
        from result in UiViewportPreview.Use(document: document, preview: Preview, gumball: Gumball, run: scope => new Callback(initial: Initial, change: validMouse).Use(run: _ => validRun(arg: scope)))
        select result;

    private sealed class Callback(TState initial, Func<MouseContext<TState>, Fin<MouseDecision<TState>>> change) : global::Rhino.UI.MouseCallback, IDisposable {
        private readonly Atom<TState> state = Atom(initial);
        private bool ownsToolTip;
        private bool disposed;

        internal Fin<T> Use<T>(Func<Callback, Fin<T>> run) =>
            from validRun in Op.Of(name: nameof(Use)).Need(run)
            from _ in guard(!disposed, Op.Of(name: nameof(Use)).InvalidInput())
            from result in RhinoUi.Protect(valid: () => {
                Enabled = true;
                // BOUNDARY ADAPTER - native mouse callback lifetime must close after viewport interaction exits.
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
                        _ => unit,
                    };
                    args.Cancel = context.CanCancelNative switch {
                        true => args.Cancel || decision.Cancel,
                        _ => args.Cancel,
                    };
                    return unit;
                })
                .Match(Succ: static _ => unit, Fail: error => {
                    RhinoApp.WriteLine(message: $"{nameof(UiViewportInteraction<>)}: {error}");
                    return Disable();
                });
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

public sealed record UiViewportPreview {
    private readonly Func<UiPreviewContext, Fin<Unit>> draw;
    private readonly Func<Fin<OverlayDecision>> bounds;
    private readonly Func<Fin<Unit>> validate;

    private UiViewportPreview(Func<UiPreviewContext, Fin<Unit>> draw, Func<Fin<OverlayDecision>> bounds, Func<Fin<Unit>> validate) {
        this.draw = draw;
        this.bounds = bounds;
        this.validate = validate;
    }

    public static UiViewportPreview Empty { get; } =
        new(draw: _ => Fin.Succ(value: unit), bounds: static () => Fin.Succ(value: OverlayDecision.Ignore), validate: static () => Fin.Succ(value: unit));

    public static UiViewportPreview Of<TGeometry>(IEnumerable<TGeometry> geometry, UiPreviewStyle style = default) where TGeometry : notnull {
        Fin<Seq<object>> items = Op.Of(name: nameof(UiViewportPreview)).Need(geometry)
            .Bind(static source => toSeq(source).Map(static item => (object)item).TraverseM(item => Op.Of(name: nameof(UiViewportPreview)).Need(item)
                .Bind(static value => OverlayDecision.BoundsOf(source: value, op: Op.Of(name: nameof(UiViewportPreview))).Map(_ => value))).As())
            .Bind(static values => values.IsEmpty switch {
                false => Fin.Succ(value: values),
                true => Fin.Fail<Seq<object>>(error: Op.Of(name: nameof(UiViewportPreview)).InvalidInput()),
            });
        return new(
            draw: context => (context.Phase == style.PhaseOrDefault) switch {
                true => from active in items
                        from _ in active.TraverseM(item => style.Draw(context: context, geometry: item)).As()
                        select unit,
                _ => Fin.Succ(value: unit),
            },
            bounds: () => from active in items
                          from decision in active
                .TraverseM(static item => OverlayDecision.BoundsOf(source: item, op: Op.Of(name: nameof(UiViewportPreview))).Bind(OverlayDecision.Include))
                .As()
                .Map(static decisions => decisions.Fold(OverlayDecision.Ignore, static (state, decision) => state + decision))
                          select decision,
            validate: () => items.Map(static _ => unit));
    }

    public static UiViewportPreview FromSelection(Commands.CommandSelection selection, UiPreviewStyle style = default) {
        Fin<UiViewportPreview> preview =
            Op.Of(name: nameof(FromSelection)).Need(selection)
                .Bind(valid => valid.Geometry<GeometryBase>())
                .Map(geometry => Of(geometry: geometry, style: style));

        return new(
            draw: context => preview.Bind(active => active.Draw(context: context)),
            bounds: () => preview.Bind(active => active.Bounds()),
            validate: () => preview.Map(static _ => unit));
    }

    public static UiViewportPreview Draw(Func<UiPreviewContext, Fin<Unit>> draw, Func<Fin<OverlayDecision>> bounds) =>
        new(
            draw: context => Op.Of(name: nameof(Draw)).Need(draw).Bind(valid => valid(arg: context)),
            bounds: () => Op.Of(name: nameof(Draw)).Need(bounds).Bind(valid => valid()),
            validate: () => Op.Of(name: nameof(Draw)).Need(draw)
                .Bind(_ => Op.Of(name: nameof(Draw)).Need(bounds))
                .Map(static _ => unit));

    public static UiViewportPreview Add(UiViewportPreview left, UiViewportPreview right) =>
        (Optional(left).IfNone(Empty), Optional(right).IfNone(Empty)) switch {
            (UiViewportPreview a, UiViewportPreview b) => new(
                draw: context => a.Draw(context: context).Bind(_ => b.Draw(context: context)),
                bounds: () => from first in a.Bounds() from second in b.Bounds() select first + second,
                validate: () => a.Validate().Bind(_ => b.Validate())),
        };

    public static UiViewportPreview operator +(UiViewportPreview left, UiViewportPreview right) =>
        Add(left: left, right: right);

    internal Fin<Unit> Draw(UiPreviewContext context) => draw(arg: context);
    internal Fin<OverlayDecision> Bounds() => bounds();
    internal Fin<Unit> Validate() => validate();

    internal static Fin<T> Use<T>(RhinoDoc document, UiViewportPreview preview, Option<UiGumballSpec> gumball, Func<UiPreviewScope, Fin<T>> run) =>
        from validDocument in Op.Of(name: nameof(Use)).Need(document)
        from validPreview in Op.Of(name: nameof(Use)).Need(preview).Bind(valid => valid.Validate().Map(_ => valid))
        from validRun in Op.Of(name: nameof(Use)).Need(run)
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

    private sealed class Conduit(RhinoDoc document, UiViewportPreview initial, Func<Option<UiGumballSnapshot>> gumball) : RasmOverlay<UiViewportPreview>(initial) {
        protected override Fin<OverlayDecision> Change(OverlayContext<UiViewportPreview> context) =>
            context.Phase switch {
                OverlayPhase.Bounds or OverlayPhase.ZoomBounds => context.State.Bounds(),
                OverlayPhase.PostDraw or OverlayPhase.Foreground or OverlayPhase.Overlay =>
                    context.Require<DrawEventArgs>()
                        .Bind(args => OverlayDecision.Paint(draw: display => context.State.Draw(context: new UiPreviewContext(
                            Document: document,
                            Phase: context.Phase,
                            Viewport: args.Viewport,
                            Display: display,
                            Gumball: gumball())))),
                _ => Fin.Succ(value: OverlayDecision.Ignore),
            };
    }
}

public readonly record struct UiGumballSnapshot(
    Transform PreTransform,
    Transform GumballTransform,
    Transform TotalTransform,
    global::Rhino.UI.Gumball.GumballMode Mode,
    bool InRelocate) {
    public Fin<TGeometry> Apply<TGeometry>(TGeometry geometry, bool duplicate = true) where TGeometry : GeometryBase { Transform transform = TotalTransform; return from _ in guard(transform.IsValid, Op.Of(name: nameof(Apply)).InvalidInput()) from source in Op.Of(name: nameof(Apply)).Need(geometry) from target in duplicate switch { true => Optional(source.Duplicate() as TGeometry).ToFin(Fail: Op.Of(name: nameof(Apply)).InvalidResult()), false => Fin.Succ(value: source) } from __ in target.Transform(xform: transform) switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(Apply)).InvalidResult()) } select target; }
    public Fin<Seq<TGeometry>> Apply<TGeometry>(IEnumerable<TGeometry> geometry, bool duplicate = true) where TGeometry : GeometryBase { UiGumballSnapshot snapshot = this; return Op.Of(name: nameof(Apply)).Need(geometry).Bind(items => toSeq(items).TraverseM(item => snapshot.Apply(geometry: item, duplicate: duplicate)).As()); }
}

public sealed record UiGumballSpec {
    private readonly Func<global::Rhino.UI.Gumball.GumballObject, Fin<Unit>> configure;

    private UiGumballSpec(Func<global::Rhino.UI.Gumball.GumballObject, Fin<Unit>> configure, global::Rhino.UI.Gumball.GumballAppearanceSettings? appearance, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> appearanceActions) {
        this.configure = configure;
        Appearance = Settings(seed: appearance, actions: appearanceActions);
    }

    internal global::Rhino.UI.Gumball.GumballAppearanceSettings? Appearance { get; }

    internal Fin<Unit> Configure(global::Rhino.UI.Gumball.GumballObject gumball) =>
        Op.Of(name: nameof(UiGumballSpec)).Need(configure)
            .Bind(valid => valid(arg: gumball));

    public static UiGumballSpec Of(
        object source,
        Option<Plane> frame = default,
        global::Rhino.UI.Gumball.GumballAppearanceSettings? appearance = null,
        params Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>[] appearanceActions) =>
        new(
            configure: gumball => Op.Of(name: nameof(UiGumballSpec)).Need(source).Bind(value => From(gumball: gumball, source: value, frame: frame)),
            appearance: appearance,
            appearanceActions: toSeq(appearanceActions));

    private static Fin<Unit> From(global::Rhino.UI.Gumball.GumballObject gumball, object source, Option<Plane> frame) =>
        (frame.Case, source) switch {
            (Plane plane, BoundingBox box) => Valid(gumball.SetFromBoundingBox(frame: plane, frameBoundingBox: box)),
            (Plane plane, GeometryBase geometry) => Bounds(source: geometry, frame: Some(plane)).Bind(box => Valid(gumball.SetFromBoundingBox(frame: plane, frameBoundingBox: box))),
            (_, BoundingBox box) => Valid(gumball.SetFromBoundingBox(boundingBox: box)),
            (_, Line line) => Valid(gumball.SetFromLine(line: line)),
            (_, Plane plane) => Valid(gumball.SetFromPlane(plane: plane)),
            (_, Arc arc) => Valid(gumball.SetFromArc(arc: arc)),
            (_, Circle circle) => Valid(gumball.SetFromCircle(circle: circle)),
            (_, Ellipse ellipse) => Valid(gumball.SetFromEllipse(ellipse: ellipse)),
            (_, Light light) => Valid(gumball.SetFromLight(light: light)),
            (_, Hatch hatch) => Valid(gumball.SetFromHatch(hatch: hatch)),
            (_, Curve curve) => Valid(gumball.SetFromCurve(curve: curve)),
            (_, Extrusion extrusion) => Valid(gumball.SetFromExtrusion(extrusion: extrusion)),
            (_, object value) => Bounds(source: value, frame: Option<Plane>.None).Bind(box => Valid(gumball.SetFromBoundingBox(boundingBox: box))),
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidInput()),
        };

    private static Fin<Unit> Valid(bool value) => value switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult()) };

    private static Fin<BoundingBox> Bounds(object source, Option<Plane> frame) => (frame.Case, source) switch { (Plane plane, GeometryBase geometry) => Op.Of(name: nameof(UiGumballSpec)).Need(geometry).Bind(value => value.GetBoundingBox(plane: plane) switch { BoundingBox box when box.IsValid => Fin.Succ(value: box), _ => Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult()) }), (_, object value) => OverlayDecision.BoundsOf(source: value, op: Op.Of(name: nameof(UiGumballSpec))).Bind(static box => box.IsValid ? Fin.Succ(value: box) : Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult())) };

    private static global::Rhino.UI.Gumball.GumballAppearanceSettings? Settings(global::Rhino.UI.Gumball.GumballAppearanceSettings? seed, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> actions) => (seed, actions.IsEmpty) switch { (global::Rhino.UI.Gumball.GumballAppearanceSettings settings, false) => Apply(settings: settings, actions: actions), (_, false) => Apply(settings: new global::Rhino.UI.Gumball.GumballAppearanceSettings(), actions: actions), _ => seed };

    private static global::Rhino.UI.Gumball.GumballAppearanceSettings Apply(global::Rhino.UI.Gumball.GumballAppearanceSettings settings, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> actions) { _ = actions.Iter(action => action(obj: settings)); return settings; }
}

// --- [SERVICES] ---------------------------------------------------------------------------
public abstract class RasmOverlay<TState>(TState initial) : DisplayConduit, IDisposable {
    private readonly Atom<TState> state = Atom(initial);
    private bool disposed;

    public TState State => state.Value;

    public Fin<Unit> Transition(Func<TState, TState> transition, RhinoDoc? document = null) =>
        disposed switch { false => Op.Of(name: nameof(Transition)).Need(transition).Map(apply => { _ = state.Swap(f: apply); _ = Optional(document).Iter(static doc => doc.Views.Redraw()); return unit; }), true => Fin.Fail<Unit>(error: Op.Of(name: nameof(Transition)).InvalidInput()) };

    public Fin<Unit> Enable(bool enabled = true, RhinoDoc? document = null) =>
        disposed switch { false => Fin.Succ(value: Op.Side(() => { Enabled = enabled; _ = Optional(document).Iter(static doc => doc.Views.Redraw()); })), true => Fin.Fail<Unit>(error: Op.Of(name: nameof(Enable)).InvalidInput()) };

    public Fin<Unit> Filter(OverlayFilter filter, RhinoDoc? document = null) =>
        disposed switch { false => filter.Apply(conduit: this).Map(_ => { _ = Optional(document).Iter(static doc => doc.Views.Redraw()); return unit; }), true => Fin.Fail<Unit>(error: Op.Of(name: nameof(Filter)).InvalidInput()) };

    public Fin<T> Use<T>(RhinoDoc document, Func<RasmOverlay<TState>, Fin<T>> run) =>
        from validDocument in Op.Of(name: nameof(Use)).Need(document) from validRun in Op.Of(name: nameof(Use)).Need(run) from active in guard(!disposed, Op.Of(name: nameof(Use)).InvalidInput()) from result in RhinoUi.Protect(valid: () => {
            Enabled = true;
            validDocument.Views.Redraw();
            // BOUNDARY ADAPTER - display conduit lifetime must be closed after the caller rail exits.
            try { return validRun(arg: this); } finally { Dispose(); validDocument.Views.Redraw(); }
        }) select result;

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

    private Unit Disable() {
        Enabled = false;
        return unit;
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

    private Fin<Unit> Apply(OverlayPhase phase, object? args, bool enabled = false) =>
        RhinoUi.Protect(valid: () => Change(context: new OverlayContext<TState>(Phase: phase, State: State, Args: args, Enabled: enabled))).Bind(decision => ((Optional(args as DrawEventArgs).Case, Optional(args as DrawObjectEventArgs).Case, decision.Draw.Case, decision.DrawObject.Case) switch {
            (_, DrawObjectEventArgs target, _, Func<DrawObjectEventArgs, Fin<Unit>> paint) => paint(arg: target),
            (DrawEventArgs target, _, Func<DisplayPipeline, Fin<Unit>> paint, _) => paint(arg: target.Display),
            _ => Fin.Succ(value: unit),
        }).Map(_ => {
            _ = Optional(args as CalculateBoundingBoxEventArgs).Iter(target => decision.Bounds.Iter(box => target.IncludeBoundingBox(box)));
            _ = Optional(args as CullObjectEventArgs).Iter(target => decision.Cull.Iter(value => target.CullObject = value));
            return unit;
        }));
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
            InRelocate: conduit.InRelocate);

    public Fin<bool> Pick(PickContext pick, GetPoint point) => from _ in guard(!disposed, Op.Of(name: nameof(Pick)).InvalidInput()) from validPick in Op.Of(name: nameof(Pick)).Need(pick) from validPoint in Op.Of(name: nameof(Pick)).Need(point) select conduit.PickGumball(pickContext: validPick, getPoint: validPoint);

    public Fin<bool> Update(Point3d point, Line line) =>
        from _ in guard(!disposed && point.IsValid && line.IsValid, Op.Of(name: nameof(Update)).InvalidInput()) from changed in Redraw(value: conduit.UpdateGumball(point: point, worldLine: line)) select changed;

    public Fin<bool> Update(Plane frame) =>
        from _ in guard(!disposed && frame.IsValid, Op.Of(name: nameof(Update)).InvalidInput()) from changed in Redraw(value: conduit.UpdateGumball(frame: frame)) select changed;

    public Fin<Unit> CheckKeys() =>
        disposed switch { false => Fin.Succ(value: Op.Side(conduit.CheckShiftAndControlKeys)), true => Fin.Fail<Unit>(error: Op.Of(name: nameof(CheckKeys)).InvalidInput()) };

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
            // BOUNDARY ADAPTER - gumball/conduit are native disposable view state.
            try { return run(arg: scope); } finally { scope.Dispose(); }
        })
        select result;

    private static Fin<UiGumball> Create(RhinoDoc document, UiGumballSpec spec) {
        global::Rhino.UI.Gumball.GumballObject? nativeSource = new();
        // BOUNDARY ADAPTER - native gumball ownership transfers only after conduit creation succeeds.
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

    private Fin<bool> Redraw(bool value) {
        document.Views.Redraw();
        return Fin.Succ(value: value);
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class UiViewportRequest {
    public static UiIntent<T> Preview<T>(UiViewportPreview preview, Func<UiPreviewScope, Fin<T>> run, Option<UiGumballSpec> gumball = default) =>
        UiIntent.OfScope(run: scope => UiViewportPreview.Use(document: scope.Document, preview: preview, gumball: gumball, run: run), interactive: true);

    public static UiIntent<T> Interaction<TState, T>(UiViewportInteraction<TState> interaction, Func<UiPreviewScope, Fin<T>> run) =>
        UiIntent.OfScope(run: scope =>
            from active in Op.Of(name: nameof(Interaction)).Need(interaction)
            from result in active.Use(document: scope.Document, run: run)
            select result, interactive: true);
}

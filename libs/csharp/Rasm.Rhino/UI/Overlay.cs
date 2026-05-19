using DrawingColor = System.Drawing.Color;

namespace Rasm.Rhino.UI;

public enum OverlayPhase { Enabled, Cull, PreDrawObjects, PreDrawObject, Foreground, Overlay, PostDraw, Bounds, ZoomBounds }

public readonly record struct OverlayContext<TState>(OverlayPhase Phase, TState State, object? Args = null, bool Enabled = false) {
    public Option<DrawEventArgs> Draw => Optional(Args as DrawEventArgs);
    public Option<DrawForegroundEventArgs> Foreground => Optional(Args as DrawForegroundEventArgs);
    public Option<DrawObjectEventArgs> DrawObject => Optional(Args as DrawObjectEventArgs);
    public Option<CullObjectEventArgs> Cull => Optional(Args as CullObjectEventArgs);
    public Option<CalculateBoundingBoxEventArgs> Bounds => Optional(Args as CalculateBoundingBoxEventArgs);

    public Fin<TArgs> Require<TArgs>() where TArgs : class =>
        Optional(Args as TArgs).ToFin(Fail: Op.Of(name: nameof(Require)).InvalidInput());
}

public readonly record struct OverlayDecision(Option<BoundingBox> Bounds = default, Option<bool> Cull = default, Option<Func<DisplayPipeline, Fin<Unit>>> Draw = default, Option<Func<DrawObjectEventArgs, Fin<Unit>>> DrawObject = default) {
    public static OverlayDecision Ignore => new();
    public static Fin<OverlayDecision> Include(BoundingBox bounds) => bounds.IsValid switch { true => Fin.Succ(value: new OverlayDecision(Bounds: Some(bounds))), false => Fin.Fail<OverlayDecision>(error: Op.Of(name: nameof(OverlayDecision)).InvalidResult()) };
    public static Fin<OverlayDecision> Include(GeometryBase geometry) =>
        BoundsOf(source: geometry, op: Op.Of(name: nameof(OverlayDecision))).Bind(Include);
    public static OverlayDecision CullObject(bool cull = true) => new(Cull: Some(cull));
    public static Fin<OverlayDecision> Paint(Func<DisplayPipeline, Fin<Unit>> draw) =>
        Optional(draw)
            .ToFin(Fail: Op.Of(name: nameof(Paint)).InvalidInput())
            .Map(static value => new OverlayDecision(Draw: Some(value)));
    public static Fin<OverlayDecision> PaintObject(Func<DrawObjectEventArgs, Fin<Unit>> draw) =>
        Optional(draw)
            .ToFin(Fail: Op.Of(name: nameof(PaintObject)).InvalidInput())
            .Map(static value => new OverlayDecision(DrawObject: Some(value)));
    public static OverlayDecision operator +(OverlayDecision left, OverlayDecision right) => Add(left: left, right: right);
    public static OverlayDecision Add(OverlayDecision left, OverlayDecision right) => new(
        Bounds: (left.Bounds.Case, right.Bounds.Case) switch { (BoundingBox a, BoundingBox b) => Some(BoundingBox.Union(a, b)), (_, BoundingBox b) => Some(b), (BoundingBox a, _) => Some(a), _ => Option<BoundingBox>.None },
        Cull: right.Cull | left.Cull,
        Draw: (left.Draw.Case, right.Draw.Case) switch {
            (Func<DisplayPipeline, Fin<Unit>> a, Func<DisplayPipeline, Fin<Unit>> b) => Some<Func<DisplayPipeline, Fin<Unit>>>(pipeline => a(arg: pipeline).Bind(_ => b(arg: pipeline))),
            (_, Func<DisplayPipeline, Fin<Unit>> b) => Some(b),
            (Func<DisplayPipeline, Fin<Unit>> a, _) => Some(a),
            _ => Option<Func<DisplayPipeline, Fin<Unit>>>.None,
        },
        DrawObject: (left.DrawObject.Case, right.DrawObject.Case) switch {
            (Func<DrawObjectEventArgs, Fin<Unit>> a, Func<DrawObjectEventArgs, Fin<Unit>> b) => Some<Func<DrawObjectEventArgs, Fin<Unit>>>(args => a(arg: args).Bind(_ => b(arg: args))),
            (_, Func<DrawObjectEventArgs, Fin<Unit>> b) => Some(b),
            (Func<DrawObjectEventArgs, Fin<Unit>> a, _) => Some(a),
            _ => Option<Func<DrawObjectEventArgs, Fin<Unit>>>.None,
        });
    internal static Fin<BoundingBox> BoundsOf(object source, Op op) => Optional(source).ToFin(Fail: op.InvalidInput()).Bind(value => value switch { BoundingBox box when box.IsValid => Fin.Succ(value: box), Box box when box.IsValid => Fin.Succ(value: box.BoundingBox), Sphere sphere when sphere.IsValid => Fin.Succ(value: sphere.BoundingBox), Line line when line.IsValid => Fin.Succ(value: line.BoundingBox), Polyline polyline when polyline.IsValid => Fin.Succ(value: polyline.BoundingBox), Circle circle when circle.IsValid => Fin.Succ(value: circle.BoundingBox), Arc arc when arc.IsValid => Fin.Succ(value: arc.BoundingBox()), Point3d point when point.IsValid => Fin.Succ(value: new BoundingBox(point, point)), GeometryBase geometry when geometry.IsValid && geometry.GetBoundingBox(accurate: true) is BoundingBox box && box.IsValid => Fin.Succ(value: box), _ => Fin.Fail<BoundingBox>(error: op.InvalidInput()) });
}

public readonly record struct OverlayFilter(Option<ObjectType> Geometry = default, Option<ActiveSpace> Space = default, Option<Seq<Guid>> ObjectIds = default, Option<(bool On, bool CheckSubObjects)> Selection = default, Option<(RhinoViewport Viewport, bool Exclusive)> Viewport = default, bool Unbind = false) {
    public static OverlayFilter Reset => new(Unbind: true);

    internal Fin<Unit> Apply(DisplayConduit conduit) {
        Option<ObjectType> geometry = Geometry; Option<ActiveSpace> space = Space; Option<Seq<Guid>> objectIds = ObjectIds.Map(static ids => ids.Filter(static id => id != Guid.Empty).Distinct()); Option<(bool On, bool CheckSubObjects)> selection = Selection; Option<(RhinoViewport Viewport, bool Exclusive)> viewport = Viewport;
        bool unbind = Unbind;
        return Optional(conduit)
            .ToFin(Fail: Op.Of(name: nameof(OverlayFilter)).InvalidInput())
            .Map(valid => {
                _ = unbind switch {
                    true => Do(action: () => {
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
                        _ = Do(action: valid.UnbindAll);
                        _ = value.Exclusive switch {
                            true => Do(action: () => valid.ExclusiveBind(viewport: value.Viewport)),
                            false => Do(action: () => valid.Bind(viewport: value.Viewport)),
                        };
                        return unit;
                    }).IfNone(unit),
                };
                return unit;
            });
    }

    private static Unit Do(Action action) { action(); return unit; }
}

public abstract class RasmOverlay<TState>(TState initial) : DisplayConduit, IDisposable {
    private readonly Atom<TState> state = Atom(initial);
    private bool disposed;

    public TState State => state.Value;

    public Fin<Unit> Transition(Func<TState, TState> transition, RhinoDoc? document = null) =>
        disposed switch { false => Optional(transition).ToFin(Fail: Op.Of(name: nameof(Transition)).InvalidInput()).Map(apply => { _ = state.Swap(f: apply); _ = Optional(document).Iter(static doc => doc.Views.Redraw()); return unit; }), true => Fin.Fail<Unit>(error: Op.Of(name: nameof(Transition)).InvalidInput()) };

    public Fin<Unit> Enable(bool enabled = true, RhinoDoc? document = null) =>
        disposed switch { false => Fin.Succ(value: ((Func<Unit>)(() => { Enabled = enabled; _ = Optional(document).Iter(static doc => doc.Views.Redraw()); return unit; }))()), true => Fin.Fail<Unit>(error: Op.Of(name: nameof(Enable)).InvalidInput()) };

    public Fin<Unit> Filter(OverlayFilter filter, RhinoDoc? document = null) =>
        disposed switch { false => filter.Apply(conduit: this).Map(_ => { _ = Optional(document).Iter(static doc => doc.Views.Redraw()); return unit; }), true => Fin.Fail<Unit>(error: Op.Of(name: nameof(Filter)).InvalidInput()) };

    public Fin<T> Use<T>(RhinoDoc document, Func<RasmOverlay<TState>, Fin<T>> run) =>
        from validDocument in Optional(document).ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput()) from validRun in Optional(run).ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput()) from active in guard(!disposed, Op.Of(name: nameof(Use)).InvalidInput()) from result in RhinoUi.Protect(valid: () => {
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

public readonly record struct UiPreviewStyle(
    Option<DrawingColor> Stroke = default,
    Option<DisplayMaterial> Material = default,
    int Thickness = 2,
    int WireDensity = 0,
    float PointRadius = 4f,
    PointStyle PointStyle = PointStyle.Simple,
    double Transparency = 0.55) {
    internal DrawingColor StrokeOrDefault => Stroke.IfNone(DrawingColor.FromArgb(alpha: 220, red: 32, green: 156, blue: 238));
    internal DisplayMaterial MaterialOrDefault {
        get {
            Option<DisplayMaterial> material = Material;
            DrawingColor stroke = StrokeOrDefault;
            double transparency = Transparency;
            return material.IfNone(() => new DisplayMaterial(diffuse: stroke, transparency: transparency));
        }
    }

    internal Unit Draw(DisplayPipeline display, object geometry) {
        DrawingColor stroke = StrokeOrDefault;
        DisplayMaterial material = MaterialOrDefault;
        int thickness = Thickness;
        int wireDensity = WireDensity;
        float pointRadius = PointRadius;
        PointStyle pointStyle = PointStyle;
        return geometry switch {
            Mesh mesh => Effect(action: () => { display.DrawMeshShaded(mesh: mesh, material: material); display.DrawMeshWires(mesh: mesh, color: stroke, thickness: thickness); }),
            Brep brep => Effect(action: () => { display.DrawBrepShaded(brep: brep, material: material); display.DrawBrepWires(brep: brep, color: stroke, wireDensity: wireDensity); }),
            Curve curve => Effect(action: () => display.DrawCurve(curve: curve, color: stroke, thickness: thickness)),
            Extrusion extrusion => Effect(action: () => display.DrawExtrusionWires(extrusion: extrusion, color: stroke, wireDensity: wireDensity)),
            Surface surface => Effect(action: () => display.DrawSurface(surface: surface, wireColor: stroke, wireDensity: wireDensity)),
            Point point => Effect(action: () => display.DrawPoint(point: point.Location, style: pointStyle, radius: pointRadius, color: stroke)),
            Point3d point => Effect(action: () => display.DrawPoint(point: point, style: pointStyle, radius: pointRadius, color: stroke)),
            Line line => Effect(action: () => display.DrawLine(line: line, color: stroke, thickness: thickness)),
            Polyline polyline => Effect(action: () => display.DrawPolyline(polyline: polyline, color: stroke, thickness: thickness)),
            Arc arc => Effect(action: () => display.DrawArc(arc: arc, color: stroke, thickness: thickness)),
            Circle circle => Effect(action: () => display.DrawCircle(circle: circle, color: stroke, thickness: thickness)),
            PointCloud cloud => Effect(action: () => display.DrawPointCloud(cloud: cloud, size: pointRadius, color: stroke)),
            SubD subd => Effect(action: () => { display.DrawSubDShaded(subd: subd, material: material); display.DrawSubDWires(subd: subd, color: stroke, thickness: thickness); }),
            Hatch hatch => Effect(action: () => display.DrawHatch(hatch: hatch, hatchColor: stroke, boundaryColor: stroke)),
            TextDot dot => Effect(action: () => display.DrawDot(dot: dot, fillColor: stroke, textColor: DrawingColor.White, borderColor: stroke)),
            TextEntity text => Effect(action: () => display.DrawText(text: text, color: stroke)),
            AnnotationBase annotation => Effect(action: () => display.DrawAnnotation(annotation: annotation, color: stroke)),
            Light light => Effect(action: () => display.DrawLight(light: light, wireframeColor: stroke)),
            _ => unit,
        };
    }

    private static Unit Effect(Action action) {
        action();
        return unit;
    }
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
        return Optional(preview)
            .ToFin(Fail: Op.Of(name: nameof(Set)).InvalidInput())
            .Bind(valid => valid.Validate().Map(_ => valid))
            .Bind(valid => overlay.Transition(transition: _ => valid, document: document));
    }

    public Fin<bool> UpdateGumball<TState>(MouseContext<TState> mouse, Point3d point) =>
        from active in Gumball.ToFin(Fail: Op.Of(name: nameof(UpdateGumball)).InvalidInput())
        from line in mouse.RequireWorldLine()
        from _ in active.CheckKeys()
        from changed in active.Update(point: point, line: line)
        select changed;
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
        Fin<Seq<object>> items = Optional(geometry)
            .ToFin(Fail: Op.Of(name: nameof(UiViewportPreview)).InvalidInput())
            .Bind(static source => toSeq(source).Map(static item => (object)item).TraverseM(item => Optional(item)
                .ToFin(Fail: Op.Of(name: nameof(UiViewportPreview)).InvalidInput())
                .Bind(static value => value switch {
                    Mesh or Brep or Curve or Extrusion or Surface or Point or Point3d or Line or Polyline or Arc or Circle or PointCloud or SubD or Hatch or TextDot or AnnotationBase or Light => Fin.Succ(value: value),
                    _ => Fin.Fail<object>(error: Op.Of(name: nameof(UiViewportPreview)).InvalidInput()),
                })).As())
            .Bind(static values => values.IsEmpty switch {
                false => Fin.Succ(value: values),
                true => Fin.Fail<Seq<object>>(error: Op.Of(name: nameof(UiViewportPreview)).InvalidInput()),
            });
        return new(
            draw: context => context.Phase switch {
                OverlayPhase.PostDraw => from active in items
                                         from display in Optional(context.Display).ToFin(Fail: Op.Of(name: nameof(UiViewportPreview)).InvalidInput())
                                         select active.Iter(item => style.Draw(display: display, geometry: item)),
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

    public static UiViewportPreview Draw(Func<UiPreviewContext, Fin<Unit>> draw, Func<Fin<OverlayDecision>> bounds) =>
        new(
            draw: context => Optional(draw).ToFin(Fail: Op.Of(name: nameof(Draw)).InvalidInput()).Bind(valid => valid(arg: context)),
            bounds: () => Optional(bounds).ToFin(Fail: Op.Of(name: nameof(Draw)).InvalidInput()).Bind(valid => valid()),
            validate: () => Optional(draw)
                .ToFin(Fail: Op.Of(name: nameof(Draw)).InvalidInput())
                .Bind(_ => Optional(bounds).ToFin(Fail: Op.Of(name: nameof(Draw)).InvalidInput()))
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
        from validDocument in Optional(document).ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput())
        from validPreview in Optional(preview).ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput()).Bind(valid => valid.Validate().Map(_ => valid))
        from validRun in Optional(run).ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput())
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
    public Fin<TGeometry> Apply<TGeometry>(TGeometry geometry, bool duplicate = true) where TGeometry : GeometryBase { Transform transform = TotalTransform; return from _ in guard(transform.IsValid, Op.Of(name: nameof(Apply)).InvalidInput()) from source in Optional(geometry).ToFin(Fail: Op.Of(name: nameof(Apply)).InvalidInput()) from target in duplicate switch { true => Optional(source.Duplicate() as TGeometry).ToFin(Fail: Op.Of(name: nameof(Apply)).InvalidResult()), false => Fin.Succ(value: source) } from __ in target.Transform(xform: transform) switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(Apply)).InvalidResult()) } select target; }
    public Fin<Seq<TGeometry>> Apply<TGeometry>(IEnumerable<TGeometry> geometry, bool duplicate = true) where TGeometry : GeometryBase { UiGumballSnapshot snapshot = this; return Optional(geometry).ToFin(Fail: Op.Of(name: nameof(Apply)).InvalidInput()).Bind(items => toSeq<TGeometry>(items).TraverseM(item => snapshot.Apply(geometry: item, duplicate: duplicate)).As()); }
}

public sealed record UiGumballSpec {
    private readonly Func<global::Rhino.UI.Gumball.GumballObject, Fin<Unit>> configure;

    private UiGumballSpec(Func<global::Rhino.UI.Gumball.GumballObject, Fin<Unit>> configure, global::Rhino.UI.Gumball.GumballAppearanceSettings? appearance, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> appearanceActions) {
        this.configure = configure;
        Appearance = Settings(seed: appearance, actions: appearanceActions);
    }

    internal global::Rhino.UI.Gumball.GumballAppearanceSettings? Appearance { get; }

    internal Fin<Unit> Configure(global::Rhino.UI.Gumball.GumballObject gumball) =>
        Optional(configure)
            .ToFin(Fail: Op.Of(name: nameof(UiGumballSpec)).InvalidInput())
            .Bind(valid => valid(arg: gumball));

    public static UiGumballSpec Of(
        object source,
        Option<Plane> frame = default,
        global::Rhino.UI.Gumball.GumballAppearanceSettings? appearance = null,
        params Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>[] appearanceActions) =>
        new(
            configure: gumball => Optional(source).ToFin(Fail: Op.Of(name: nameof(UiGumballSpec)).InvalidInput()).Bind(value => From(gumball: gumball, source: value, frame: frame)),
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

    private static Fin<BoundingBox> Bounds(object source, Option<Plane> frame) => (frame.Case, source) switch { (Plane plane, GeometryBase geometry) => Optional(geometry).ToFin(Fail: Op.Of(name: nameof(UiGumballSpec)).InvalidInput()).Bind(value => value.GetBoundingBox(plane: plane) switch { BoundingBox box when box.IsValid => Fin.Succ(value: box), _ => Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult()) }), (_, object value) => OverlayDecision.BoundsOf(source: value, op: Op.Of(name: nameof(UiGumballSpec))).Bind(static box => box.IsValid ? Fin.Succ(value: box) : Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult())) };

    private static global::Rhino.UI.Gumball.GumballAppearanceSettings? Settings(global::Rhino.UI.Gumball.GumballAppearanceSettings? seed, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> actions) => (seed, actions.IsEmpty) switch { (global::Rhino.UI.Gumball.GumballAppearanceSettings settings, false) => Apply(settings: settings, actions: actions), (_, false) => Apply(settings: new global::Rhino.UI.Gumball.GumballAppearanceSettings(), actions: actions), _ => seed };

    private static global::Rhino.UI.Gumball.GumballAppearanceSettings Apply(global::Rhino.UI.Gumball.GumballAppearanceSettings settings, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> actions) { _ = actions.Iter(action => action(obj: settings)); return settings; }
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

    public Fin<bool> Pick(global::Rhino.Input.Custom.PickContext pick, GetPoint? point = null) => from _ in guard(!disposed, Op.Of(name: nameof(Pick)).InvalidInput()) from validPick in Optional(pick).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput()) select conduit.PickGumball(pickContext: validPick, getPoint: point);

    public Fin<bool> Update(Point3d point, Line line) =>
        from _ in guard(!disposed && point.IsValid && line.IsValid, Op.Of(name: nameof(Update)).InvalidInput()) from changed in Redraw(value: conduit.UpdateGumball(point: point, worldLine: line)) select changed;

    public Fin<bool> Update(Plane frame) =>
        from _ in guard(!disposed && frame.IsValid, Op.Of(name: nameof(Update)).InvalidInput()) from changed in Redraw(value: conduit.UpdateGumball(frame: frame)) select changed;

    public Fin<Unit> CheckKeys() =>
        disposed switch { false => Fin.Succ(value: ((Func<Unit>)(() => { conduit.CheckShiftAndControlKeys(); return unit; }))()), true => Fin.Fail<Unit>(error: Op.Of(name: nameof(CheckKeys)).InvalidInput()) };

    public void Dispose() {
        _ = disposed switch {
            true => unit,
            false => Release(),
        };
        disposed = true;
        GC.SuppressFinalize(obj: this);
    }

    internal static Fin<T> Use<T>(RhinoDoc document, UiGumballSpec spec, Func<UiGumball, Fin<T>> run) =>
        from scope in Start(document: document, spec: spec)
        from result in RhinoUi.Protect(valid: () => {
            // BOUNDARY ADAPTER - gumball/conduit are native disposable view state.
            try { return run(arg: scope); } finally { scope.Dispose(); }
        })
        select result;

    private static Fin<UiGumball> Start(RhinoDoc document, UiGumballSpec spec) =>
        from validDocument in Optional(document).ToFin(Fail: Op.Of(name: nameof(UiGumball)).InvalidInput())
        from validSpec in Optional(spec).ToFin(Fail: Op.Of(name: nameof(UiGumball)).InvalidInput())
        from scope in RhinoUi.Protect(valid: () => Create(document: validDocument, spec: validSpec))
        select scope;

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

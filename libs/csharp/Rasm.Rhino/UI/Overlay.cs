namespace Rasm.Rhino.UI;

public enum OverlayPhase { Enabled, Cull, PreDrawObjects, PreDrawObject, Foreground, Overlay, PostDraw, Bounds, ZoomBounds }

public readonly record struct OverlayContext<TState>(OverlayPhase Phase, TState State, object? Args = null, bool Enabled = false);

public readonly record struct OverlayDecision(Option<BoundingBox> Bounds = default, Option<bool> Cull = default) {
    public static OverlayDecision Ignore => new();
    public static Fin<OverlayDecision> Include(BoundingBox bounds) => bounds.IsValid switch { true => Fin.Succ(value: new OverlayDecision(Bounds: Some(bounds))), false => Fin.Fail<OverlayDecision>(error: Op.Of(name: nameof(OverlayDecision)).InvalidResult()) };
    public static Fin<OverlayDecision> Include(GeometryBase geometry) =>
        Optional(geometry)
            .ToFin(Fail: Op.Of(name: nameof(OverlayDecision)).InvalidInput())
            .Bind(static value => value.GetBoundingBox(accurate: true) switch {
                BoundingBox box when box.IsValid => Include(bounds: box),
                _ => Fin.Fail<OverlayDecision>(error: Op.Of(name: nameof(OverlayDecision)).InvalidResult()),
            });
    public static OverlayDecision CullObject(bool cull = true) => new(Cull: Some(cull));
    public static OverlayDecision operator +(OverlayDecision left, OverlayDecision right) => Add(left: left, right: right);
    public static OverlayDecision Add(OverlayDecision left, OverlayDecision right) => new(Bounds: (left.Bounds.Case, right.Bounds.Case) switch { (BoundingBox a, BoundingBox b) => Some(BoundingBox.Union(a, b)), (_, BoundingBox b) => Some(b), (BoundingBox a, _) => Some(a), _ => Option<BoundingBox>.None }, Cull: right.Cull | left.Cull);
}

public readonly record struct OverlayFilter(Option<ObjectType> Geometry = default, Option<ActiveSpace> Space = default, Seq<Guid> ObjectIds = default, Option<(bool On, bool CheckSubObjects)> Selection = default, Option<(RhinoViewport Viewport, bool Exclusive)> Viewport = default, bool Unbind = false) {
    internal Fin<Unit> Apply(DisplayConduit conduit) {
        Option<ObjectType> geometry = Geometry; Option<ActiveSpace> space = Space; Seq<Guid> objectIds = ObjectIds; Option<(bool On, bool CheckSubObjects)> selection = Selection; Option<(RhinoViewport Viewport, bool Exclusive)> viewport = Viewport;
        bool unbind = Unbind;
        return Optional(conduit)
            .ToFin(Fail: Op.Of(name: nameof(OverlayFilter)).InvalidInput())
            .Map(valid => {
                _ = geometry.Iter(value => valid.GeometryFilter = value);
                _ = space.Iter(value => valid.SpaceFilter = value);
                _ = selection.Iter(value => valid.SetSelectionFilter(on: value.On, checkSubObjects: value.CheckSubObjects));
                _ = Do(action: () => valid.SetObjectIdFilter(ids: objectIds.AsIterable()));
                _ = unbind switch {
                    true => Do(action: valid.UnbindAll),
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
        Optional(transition)
            .ToFin(Fail: Op.Of(name: nameof(Transition)).InvalidInput())
            .Map(apply => {
                _ = state.Swap(f: apply);
                _ = Optional(document).Map(doc => {
                    doc.Views.Redraw();
                    return unit;
                });
                return unit;
            });

    public Fin<Unit> Enable(bool enabled = true, RhinoDoc? document = null) {
        Enabled = enabled;
        _ = Optional(document).Iter(static doc => doc.Views.Redraw());
        return Fin.Succ(value: unit);
    }

    public Fin<Unit> Filter(OverlayFilter filter, RhinoDoc? document = null) =>
        filter.Apply(conduit: this).Map(_ => {
            _ = Optional(document).Iter(static doc => doc.Views.Redraw());
            return unit;
        });

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
        RhinoUi.Protect(valid: () => Change(context: new OverlayContext<TState>(Phase: phase, State: State, Args: args, Enabled: enabled))).Map(decision => {
            _ = Optional(args as CalculateBoundingBoxEventArgs).Iter(target => decision.Bounds.Iter(box => target.IncludeBoundingBox(box)));
            _ = Optional(args as CullObjectEventArgs).Iter(target => decision.Cull.Iter(value => target.CullObject = value));
            return unit;
        });
}

public readonly record struct UiGumballSnapshot(
    Transform PreTransform,
    Transform GumballTransform,
    Transform TotalTransform,
    global::Rhino.UI.Gumball.GumballMode Mode,
    bool InRelocate);

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
            (Plane plane, GeometryBase geometry) => Bounds(geometry: geometry, frame: plane).Bind(box => Valid(gumball.SetFromBoundingBox(frame: plane, frameBoundingBox: box))),
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
            (_, GeometryBase geometry) => Bounds(geometry: geometry).Bind(box => Valid(gumball.SetFromBoundingBox(boundingBox: box))),
            _ => Fin.Fail<Unit>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidInput()),
        };

    private static Fin<BoundingBox> Bounds(GeometryBase geometry) => Optional(geometry).ToFin(Fail: Op.Of(name: nameof(UiGumballSpec)).InvalidInput()).Bind(static value => value.GetBoundingBox(accurate: true) switch { BoundingBox box when box.IsValid => Fin.Succ(value: box), _ => Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult()) });

    private static Fin<Unit> Valid(bool value) => value switch { true => Fin.Succ(value: unit), false => Fin.Fail<Unit>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult()) };

    private static Fin<BoundingBox> Bounds(GeometryBase geometry, Plane frame) => Optional(geometry).ToFin(Fail: Op.Of(name: nameof(UiGumballSpec)).InvalidInput()).Bind(value => value.GetBoundingBox(plane: frame) switch { BoundingBox box when box.IsValid => Fin.Succ(value: box), _ => Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult()) });

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

    public Fin<bool> Pick(global::Rhino.Input.Custom.PickContext pick, GetPoint point) => (Optional(pick).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput()), Optional(point).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput())).Apply(static (validPick, validPoint) => (Pick: validPick, Point: validPoint)).As().Map(valid => conduit.PickGumball(pickContext: valid.Pick, getPoint: valid.Point));

    public Fin<bool> Update(Point3d point, Line line) =>
        conduit.UpdateGumball(point: point, worldLine: line) switch {
            bool changed => Redraw(value: changed),
        };

    public Fin<bool> Update(Plane frame) =>
        conduit.UpdateGumball(frame: frame) switch {
            bool changed => Redraw(value: changed),
        };

    public Fin<Unit> CheckKeys() {
        conduit.CheckShiftAndControlKeys();
        return Fin.Succ(value: unit);
    }

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

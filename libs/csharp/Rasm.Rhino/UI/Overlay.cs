namespace Rasm.Rhino.UI;

public enum OverlayPhase { Enabled, Cull, PreDrawObjects, PreDrawObject, Foreground, Overlay, PostDraw, Bounds, ZoomBounds }

public readonly record struct OverlayContext<TState>(OverlayPhase Phase, TState State, object? Args = null, bool Enabled = false);

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

    protected virtual Fin<Unit> Change(OverlayContext<TState> context) =>
        Fin.Succ(value: unit);

    protected virtual Fin<BoundingBox> Bounds(OverlayContext<TState> context) =>
        Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(Bounds)).InvalidResult());

    protected sealed override void OnEnable(bool enable) =>
        _ = RhinoUi.Protect(valid: () => Change(context: new OverlayContext<TState>(Phase: OverlayPhase.Enabled, State: State, Enabled: enable)));

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
        _ = Include(phase: OverlayPhase.Bounds, args: e);

    protected sealed override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) =>
        _ = Include(phase: OverlayPhase.ZoomBounds, args: e);

    private Fin<Unit> Apply(OverlayPhase phase, object args) =>
        RhinoUi.Protect(valid: () => Change(context: new OverlayContext<TState>(Phase: phase, State: State, Args: args)));

    private Fin<Unit> Include(OverlayPhase phase, CalculateBoundingBoxEventArgs args) =>
        RhinoUi.Protect(valid: () => Bounds(context: new OverlayContext<TState>(Phase: phase, State: State, Args: args)))
            .Map(box => {
                args.IncludeBoundingBox(box);
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

    private static Fin<BoundingBox> Bounds(GeometryBase geometry) =>
        Optional(geometry)
            .ToFin(Fail: Op.Of(name: nameof(UiGumballSpec)).InvalidInput())
            .Bind(static value => value.GetBoundingBox(accurate: true) switch {
                BoundingBox box when box.IsValid => Fin.Succ(value: box),
                _ => Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult()),
            });

    private static Fin<Unit> Valid(bool value) =>
        value switch {
            true => Fin.Succ(value: unit),
            false => Fin.Fail<Unit>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult()),
        };

    private static Fin<BoundingBox> Bounds(GeometryBase geometry, Plane frame) =>
        Optional(geometry)
            .ToFin(Fail: Op.Of(name: nameof(UiGumballSpec)).InvalidInput())
            .Bind(value => value.GetBoundingBox(plane: frame) switch {
                BoundingBox box when box.IsValid => Fin.Succ(value: box),
                _ => Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(UiGumballSpec)).InvalidResult()),
            });

    private static global::Rhino.UI.Gumball.GumballAppearanceSettings? Settings(global::Rhino.UI.Gumball.GumballAppearanceSettings? seed, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> actions) =>
        (seed, actions.IsEmpty) switch {
            (global::Rhino.UI.Gumball.GumballAppearanceSettings settings, false) => Apply(settings: settings, actions: actions),
            (_, false) => Apply(settings: new global::Rhino.UI.Gumball.GumballAppearanceSettings(), actions: actions),
            _ => seed,
        };

    private static global::Rhino.UI.Gumball.GumballAppearanceSettings Apply(global::Rhino.UI.Gumball.GumballAppearanceSettings settings, Seq<Action<global::Rhino.UI.Gumball.GumballAppearanceSettings>> actions) {
        _ = actions.Iter(action => action(obj: settings));
        return settings;
    }
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

    public Fin<bool> Pick(global::Rhino.Input.Custom.PickContext pick, GetPoint point) =>
        (Optional(pick).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput()), Optional(point).ToFin(Fail: Op.Of(name: nameof(Pick)).InvalidInput()))
            .Apply(static (validPick, validPoint) => (Pick: validPick, Point: validPoint)).As()
            .Map(valid => conduit.PickGumball(pickContext: valid.Pick, getPoint: valid.Point));

    public Fin<bool> Update(Point3d point, Line line) =>
        Fin.Succ(value: conduit.UpdateGumball(point: point, worldLine: line));

    public Fin<bool> Update(Plane frame) =>
        Fin.Succ(value: conduit.UpdateGumball(frame: frame));

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
            return spec.Configure(gumball: nativeSource).Match(
            Succ: _ => {
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
            },
            Fail: Fin.Fail<UiGumball>);
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

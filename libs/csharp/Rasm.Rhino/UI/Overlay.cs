namespace Rasm.Rhino.UI;

// --- [SERVICES] -------------------------------------------------------------------------
public abstract class RasmOverlay<TState>(TState initial) : DisplayConduit {
    private readonly Atom<TState> state = Atom(initial);

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

    public Fin<Unit> BindTo(RhinoViewport viewport, bool exclusive = false) =>
        Optional(viewport)
            .ToFin(Fail: Op.Of(name: nameof(BindTo)).InvalidInput())
            .Map(valid => {
                Action<RhinoViewport> bind = exclusive switch { true => ExclusiveBind, false => Bind };
                bind(obj: valid);
                return unit;
            });

    public Fin<Unit> ClearBindings() {
        UnbindAll();
        return Fin.Succ(value: unit);
    }

    public Fin<Unit> ForObjects(IEnumerable<Guid> objectIds) =>
        Optional(objectIds)
            .ToFin(Fail: Op.Of(name: nameof(ForObjects)).InvalidInput())
            .Map(ids => {
                SetObjectIdFilter(ids);
                return unit;
            });

    public Fin<Unit> ForSelection(bool on, bool checkSubObjects = false) {
        SetSelectionFilter(on, checkSubObjects);
        return Fin.Succ(value: unit);
    }

    protected virtual Fin<Unit> Cull(TState state, CullObjectEventArgs args) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> PreDrawObjects(TState state, DrawEventArgs args) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> PreDrawObject(TState state, DrawObjectEventArgs args) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> Foreground(TState state, DrawEventArgs args) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> Overlay(TState state, DrawEventArgs args) =>
        Fin.Succ(value: unit);

    protected virtual Fin<Unit> PostDraw(TState state, DrawEventArgs args) =>
        Fin.Succ(value: unit);

    protected virtual Fin<BoundingBox> Bounds(TState state, CalculateBoundingBoxEventArgs args) =>
        Fin.Fail<BoundingBox>(error: Op.Of(name: nameof(Bounds)).InvalidResult());

    protected virtual Fin<BoundingBox> ZoomBounds(TState state, CalculateBoundingBoxEventArgs args) =>
        Bounds(state: state, args: args);

    protected virtual Fin<Unit> EnabledChanged(bool enabled) =>
        Fin.Succ(value: unit);

    protected sealed override void OnEnable(bool enable) =>
        _ = RhinoUi.Protect(valid: () => EnabledChanged(enabled: enable));

    protected sealed override void ObjectCulling(CullObjectEventArgs e) =>
        _ = Apply(args: e, change: Cull);

    protected sealed override void PreDrawObjects(DrawEventArgs e) =>
        _ = Apply(args: e, change: PreDrawObjects);

    protected sealed override void PreDrawObject(DrawObjectEventArgs e) =>
        _ = Apply(args: e, change: PreDrawObject);

    protected sealed override void DrawForeground(DrawEventArgs e) =>
        _ = Apply(args: e, change: Foreground);

    protected sealed override void DrawOverlay(DrawEventArgs e) =>
        _ = Apply(args: e, change: Overlay);

    protected sealed override void PostDrawObjects(DrawEventArgs e) =>
        _ = Apply(args: e, change: PostDraw);

    protected sealed override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e) =>
        _ = RhinoUi.Protect(valid: () => Include(bounds: Bounds(state: State, args: e), args: e));

    protected sealed override void CalculateBoundingBoxZoomExtents(CalculateBoundingBoxEventArgs e) =>
        _ = RhinoUi.Protect(valid: () => Include(bounds: ZoomBounds(state: State, args: e), args: e));

    private static Fin<Unit> Include(Fin<BoundingBox> bounds, CalculateBoundingBoxEventArgs args) =>
        bounds.Map(box => {
            args.IncludeBoundingBox(box);
            return unit;
        });

    private Fin<Unit> Apply<TArgs>(TArgs args, Func<TState, TArgs, Fin<Unit>> change) =>
        RhinoUi.Protect(valid: () => change(arg1: State, arg2: args));
}

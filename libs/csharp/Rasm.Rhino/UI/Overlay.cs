namespace Rasm.Rhino.UI;

public enum OverlayPhase { Enabled, Cull, PreDrawObjects, PreDrawObject, Foreground, Overlay, PostDraw, Bounds, ZoomBounds }

public readonly record struct OverlayContext<TState>(OverlayPhase Phase, TState State, object? Args = null, bool Enabled = false);

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

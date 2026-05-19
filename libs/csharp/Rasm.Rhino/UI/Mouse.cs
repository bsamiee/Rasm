namespace Rasm.Rhino.UI;

public enum MousePhase { Move, MoveEnd, Down, DownEnd, Up, UpEnd, DoubleClick, Enter, Hover, Leave }

public readonly record struct MouseContext<TState>(MousePhase Phase, TState State, global::Rhino.UI.MouseCallbackEventArgs Args) {
    public global::Rhino.UI.Gumball.GumballMode GumballMode => Args.IsOverGumball();
    public bool Shift => Args.ShiftKeyDown;
    public bool Control => Args.CtrlKeyDown;
    public Option<RhinoView> View => Optional(Args.View);
    public Option<System.Drawing.Point> ViewportPoint =>
        Args.ViewportPoint switch {
            { IsEmpty: false } point => Some(point),
            _ => Option<System.Drawing.Point>.None,
        };
    public Option<Line> WorldLine =>
        (View.Case, ViewportPoint.Case) switch {
            (RhinoView view, System.Drawing.Point point) => Some(view.ActiveViewport.ClientToWorld(point)),
            _ => Option<Line>.None,
        };
}

public readonly record struct MouseDecision(bool Cancel) {
    public static MouseDecision Pass => new(Cancel: false);
    public static MouseDecision Stop => new(Cancel: true);
    public static MouseDecision operator |(MouseDecision left, MouseDecision right) => BitwiseOr(left: left, right: right);
    public static MouseDecision BitwiseOr(MouseDecision left, MouseDecision right) => new(Cancel: left.Cancel || right.Cancel);
}

public abstract class RasmMouseCallback<TState>(TState initial) : global::Rhino.UI.MouseCallback, IDisposable {
    private readonly Atom<TState> state = Atom(initial);
    private bool disposed;

    public TState State => state.Value;

    public Fin<Unit> Transition(Func<TState, TState> transition) =>
        Optional(transition)
            .ToFin(Fail: Op.Of(name: nameof(Transition)).InvalidInput())
            .Map(apply => {
                _ = state.Swap(f: apply);
                return unit;
            });

    public Fin<Unit> Enable(bool enabled = true) {
        Enabled = enabled;
        return Fin.Succ(value: unit);
    }

    public Fin<T> Use<T>(Func<RasmMouseCallback<TState>, Fin<T>> run) =>
        from valid in Optional(run).ToFin(Fail: Op.Of(name: nameof(Use)).InvalidInput()) from active in guard(!disposed, Op.Of(name: nameof(Use)).InvalidInput())
        from result in RhinoUi.Protect(valid: () => { Enabled = true; try { return valid(arg: this); } finally { Dispose(); } }) select result;

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

    protected virtual Fin<MouseDecision> Change(MouseContext<TState> context) =>
        Fin.Succ(value: MouseDecision.Pass);

    protected sealed override void OnMouseMove(global::Rhino.UI.MouseCallbackEventArgs e) =>
        _ = Apply(phase: MousePhase.Move, args: e);

    protected sealed override void OnEndMouseMove(global::Rhino.UI.MouseCallbackEventArgs e) =>
        _ = Apply(phase: MousePhase.MoveEnd, args: e);

    protected sealed override void OnMouseDown(global::Rhino.UI.MouseCallbackEventArgs e) =>
        _ = Apply(phase: MousePhase.Down, args: e);

    protected sealed override void OnEndMouseDown(global::Rhino.UI.MouseCallbackEventArgs e) =>
        _ = Apply(phase: MousePhase.DownEnd, args: e);

    protected sealed override void OnMouseUp(global::Rhino.UI.MouseCallbackEventArgs e) =>
        _ = Apply(phase: MousePhase.Up, args: e);

    protected sealed override void OnEndMouseUp(global::Rhino.UI.MouseCallbackEventArgs e) =>
        _ = Apply(phase: MousePhase.UpEnd, args: e);

    protected sealed override void OnMouseDoubleClick(global::Rhino.UI.MouseCallbackEventArgs e) =>
        _ = Apply(phase: MousePhase.DoubleClick, args: e);

    protected sealed override void OnMouseEnter(global::Rhino.UI.MouseCallbackEventArgs e) =>
        _ = Apply(phase: MousePhase.Enter, args: e);

    protected sealed override void OnMouseHover(global::Rhino.UI.MouseCallbackEventArgs e) =>
        _ = Apply(phase: MousePhase.Hover, args: e);

    protected sealed override void OnMouseLeave(global::Rhino.UI.MouseCallbackEventArgs e) =>
        _ = Apply(phase: MousePhase.Leave, args: e);

    private Fin<Unit> Apply(MousePhase phase, global::Rhino.UI.MouseCallbackEventArgs args) =>
        RhinoUi.Protect(valid: () => Change(context: new MouseContext<TState>(Phase: phase, State: State, Args: args)))
            .Map(decision => {
                args.Cancel = phase switch {
                    MousePhase.Move or MousePhase.Down or MousePhase.Up or MousePhase.DoubleClick => decision.Cancel,
                    _ => args.Cancel,
                };
                return unit;
            });
}

namespace Rasm.Rhino.UI;

public enum MousePhase { Move, MoveEnd, Down, DownEnd, Up, UpEnd, DoubleClick, Enter, Hover, Leave }

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
            (RhinoView view, System.Drawing.Point point) => view.ActiveViewport.ClientToWorld(point) switch {
                Line line when line.IsValid => Some(line),
                _ => Option<Line>.None,
            },
            _ => Option<Line>.None,
        };
    public Fin<Line> RequireWorldLine() => WorldLine.ToFin(Fail: Op.Of(name: nameof(RequireWorldLine)).InvalidInput());
    public Fin<Point3d> Project(Plane plane) => from line in RequireWorldLine() from validPlane in plane.IsValid switch { true => Fin.Succ(value: plane), false => Fin.Fail<Plane>(error: Op.Of(name: nameof(Project)).InvalidInput()) } from point in global::Rhino.Geometry.Intersect.Intersection.LinePlane(line: line, plane: validPlane, lineParameter: out double parameter) switch { true => Fin.Succ(value: line.PointAt(t: parameter)), false => Fin.Fail<Point3d>(error: Op.Of(name: nameof(Project)).InvalidResult()) } select point;
}

public readonly record struct MouseDecision(bool Cancel, Option<string> ToolTip = default) {
    public static MouseDecision Pass => new(Cancel: false);
    public static MouseDecision Stop => new(Cancel: true);
    public static MouseDecision Hint(string value) =>
        string.IsNullOrWhiteSpace(value: value) switch {
            false => new MouseDecision(Cancel: false, ToolTip: Some(value)),
            true => Pass,
        };
    public static MouseDecision operator |(MouseDecision left, MouseDecision right) => BitwiseOr(left: left, right: right);
    public static MouseDecision BitwiseOr(MouseDecision left, MouseDecision right) => new(Cancel: left.Cancel || right.Cancel, ToolTip: right.ToolTip | left.ToolTip);
}

public abstract class RasmMouseCallback<TState>(TState initial) : global::Rhino.UI.MouseCallback, IDisposable {
    private readonly Atom<TState> state = Atom(initial);
    private readonly Atom<bool> ownsToolTip = Atom(false);
    private bool disposed;

    public TState State => state.Value;

    public Fin<Unit> Transition(Func<TState, TState> transition) =>
        Optional(transition)
            .ToFin(Fail: Op.Of(name: nameof(Transition)).InvalidInput())
            .Map(apply => {
                _ = state.Swap(f: apply);
                return unit;
            });

    protected Fin<Unit> Enable(bool enabled = true) =>
        disposed switch { false => Fin.Succ(value: ((Func<Unit>)(() => { Enabled = enabled; return unit; }))()), true => Fin.Fail<Unit>(error: Op.Of(name: nameof(Enable)).InvalidInput()) };

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
        _ = ownsToolTip.Value switch {
            true => ((Func<Unit>)(static () => { global::Rhino.UI.MouseCursor.SetToolTip(tooltip: string.Empty); return unit; }))(),
            false => unit,
        };
        _ = ownsToolTip.Swap(static _ => false);
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

    private Unit Apply(MousePhase phase, global::Rhino.UI.MouseCallbackEventArgs args) {
        MouseContext<TState> context = new(Phase: phase, State: State, Args: args);
        return RhinoUi.Protect(valid: () => Change(context: context))
            .Map(decision => {
                _ = decision.ToolTip.Case switch {
                    string tooltip => ((Func<Unit>)(() => {
                        _ = ownsToolTip.Swap(static _ => true);
                        global::Rhino.UI.MouseCursor.SetToolTip(tooltip: tooltip);
                        return unit;
                    }))(),
                    _ => unit,
                };
                args.Cancel = context.CanCancelNative switch {
                    true => args.Cancel || decision.Cancel,
                    _ => args.Cancel,
                };
                return unit;
            })
            .Match(
                Succ: static _ => unit,
                Fail: error => {
                    RhinoApp.WriteLine(message: $"{nameof(RasmMouseCallback<TState>)}: {error}");
                    return Disable();
                });
    }
}

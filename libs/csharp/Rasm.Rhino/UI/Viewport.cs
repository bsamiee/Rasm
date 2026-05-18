namespace Rasm.Rhino.UI;

public sealed record ViewportUi<T> {
    private readonly Func<RhinoDoc, Fin<T>> run;

    internal ViewportUi(Func<RhinoDoc, Fin<T>> run) => this.run = run;

    internal Fin<T> Run(RhinoDoc document) =>
        Optional(run)
            .ToFin(Fail: Op.Of(name: nameof(ViewportUi<T>)).InvalidInput())
            .Bind(valid => valid(arg: document));
}

public static class ViewportUi {
    public static ViewportUi<T> Of<T>(Func<RhinoDoc, Fin<T>> run) =>
        new(run: run);
}

public enum MousePhase { Move, MoveEnd, Down, DownEnd, Up, UpEnd }

public readonly record struct MouseContext<TState>(MousePhase Phase, TState State, global::Rhino.UI.MouseCallbackEventArgs Args);

public readonly record struct MouseDecision(bool Cancel) {
    public static MouseDecision Pass => new(Cancel: false);
    public static MouseDecision Stop => new(Cancel: true);
}

public abstract class RasmMouseCallback<TState>(TState initial) : global::Rhino.UI.MouseCallback {
    private readonly Atom<TState> state = Atom(initial);

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

    private Fin<Unit> Apply(MousePhase phase, global::Rhino.UI.MouseCallbackEventArgs args) =>
        RhinoUi.Protect(valid: () => Change(context: new MouseContext<TState>(Phase: phase, State: State, Args: args)))
            .Map(decision => {
                args.Cancel = phase switch {
                    MousePhase.Move or MousePhase.Down or MousePhase.Up => decision.Cancel,
                    _ => args.Cancel,
                };
                return unit;
            });
}

public sealed partial record RhinoUi {
    public Fin<T> Viewport<T>(ViewportUi<T> operation) =>
        Optional(operation)
            .ToFin(Fail: Op.Of(name: nameof(Viewport)).InvalidInput())
            .Bind(valid => OnUiThread(run: () => valid.Run(document: document)));
}

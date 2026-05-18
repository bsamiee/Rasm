namespace Rasm.Rhino.UI;

// --- [MODELS] ---------------------------------------------------------------------------
public sealed record ViewportUi<T> {
    private readonly Func<RhinoDoc, Fin<T>> run;

    internal ViewportUi(Func<RhinoDoc, Fin<T>> run) => this.run = run;

    internal Fin<T> Run(RhinoDoc document) =>
        Optional(run)
            .ToFin(Fail: Op.Of(name: nameof(ViewportUi<T>)).InvalidInput())
            .Bind(valid => valid(arg: document));
}

public static class ViewportUi {
    public static ViewportUi<Unit> Add(global::Rhino.UI.UserInterfaceObjectBase item, Guid groupId = default) =>
        Of(document => Optional(item)
            .ToFin(Fail: Op.Of(name: nameof(Add)).InvalidInput())
            .Bind(valid => document.ViewUserInterface.Add(item: valid, userInterfaceGroupId: groupId) switch {
                true => Fin.Succ(value: unit),
                false => Fin.Fail<Unit>(error: Op.Of(name: nameof(Add)).InvalidResult()),
            }));

    public static ViewportUi<int> Remove(global::Rhino.UI.UserInterfaceObjectBase item) =>
        Of(document => Optional(item)
            .ToFin(Fail: Op.Of(name: nameof(Remove)).InvalidInput())
            .Bind(valid => Count(value: document.ViewUserInterface.Remove(item: valid), op: Op.Of(name: nameof(Remove)))));

    public static ViewportUi<int> Remove(IEnumerable<global::Rhino.UI.UserInterfaceObjectBase> items) =>
        Of(document => Count(
            value: document.ViewUserInterface.Remove(items: Optional(items).Map(static values => toSeq(values)).IfNone(Seq<global::Rhino.UI.UserInterfaceObjectBase>()).AsIterable()),
            op: Op.Of(name: nameof(Remove))));

    public static ViewportUi<int> RemoveGroup(Guid groupId) =>
        Of(document => groupId switch {
            Guid id when id != Guid.Empty => Count(value: document.ViewUserInterface.RemoveByGroupId(userInterfaceGroupId: id), op: Op.Of(name: nameof(RemoveGroup))),
            _ => Fin.Fail<int>(error: Op.Of(name: nameof(RemoveGroup)).InvalidInput()),
        });

    public static ViewportUi<Seq<T>> Find<T>() where T : global::Rhino.UI.UserInterfaceObjectBase =>
        Of(document => Fin.Succ(value: toSeq(document.ViewUserInterface.Find<T>())));

    private static ViewportUi<T> Of<T>(Func<RhinoDoc, Fin<T>> run) =>
        new(run: run);

    private static Fin<int> Count(int value, Op op) =>
        value switch {
            >= 0 => Fin.Succ(value: value),
            _ => Fin.Fail<int>(error: op.InvalidResult()),
        };
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

// --- [SERVICES] -------------------------------------------------------------------------
public sealed partial record RhinoUi {
    public Fin<T> Viewport<T>(ViewportUi<T> operation) =>
        Optional(operation)
            .ToFin(Fail: Op.Of(name: nameof(Viewport)).InvalidInput())
            .Bind(valid => OnUiThread(run: () => valid.Run(document: document)));
}

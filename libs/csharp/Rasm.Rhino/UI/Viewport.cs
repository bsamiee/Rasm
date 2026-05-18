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

// --- [SERVICES] -------------------------------------------------------------------------
public sealed partial record RhinoUi {
    public Fin<T> Viewport<T>(ViewportUi<T> operation) =>
        Optional(operation)
            .ToFin(Fail: Op.Of(name: nameof(Viewport)).InvalidInput())
            .Bind(valid => OnUiThread(run: () => valid.Run(document: document)));
}

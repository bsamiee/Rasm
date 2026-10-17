using Rasm.Rhino;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Events;
using Rhino;

namespace Rasm.Grasshopper.UI;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record Attachment {
    internal sealed record Waiting : Attachment;
    internal sealed record Attaching : Attachment;
    internal sealed record Attached(IDisposable Held) : Attachment;
    internal sealed record Closed : Attachment;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Editors {
    // --- [EDITOR]
    public static IO<Unit> Attach(IPlugInSink sink, Func<Editor, Action<IDisposable>, IO<Unit>> attach, Action<IDisposable> retain) =>
        IO.lift(static () => Optional(Editor.Instance))
            .Bind(found => found.Match(
                Some: editor => DisposalOps.Acquire(keep => attach(editor, keep), retain),
                None: () => Pending(sink, attach, retain)));

    private static IO<Unit> Pending(IPlugInSink sink, Func<Editor, Action<IDisposable>, IO<Unit>> attach, Action<IDisposable> retain) =>
        DisposalOps.Acquire(keep =>
        from state in IO.lift(static () => Ref<Attachment>(new Attachment.Waiting()))
        from cell in DisposalOps.Acquire(IO.lift(() => new Disposal<Ref<Attachment>>(
                    state,
                    static cell => {
                        if (atomic(() => {
                            Attachment held = cell.Value;
                            cell.Value = new Attachment.Closed();
                            return held;
                        }) is Attachment.Attached attached)
                            attached.Held.Dispose();
                    })), keep)
        from waiting in EventKind.EndCommand.Inline(_ => Built(state, attach), sink, keep)
        select unit, retain);

    private static IO<Unit> Built(Ref<Attachment> state, Func<Editor, Action<IDisposable>, IO<Unit>> attach) =>
        DisposalOps.Transacted(
            from available in IO.lift(static () => Optional(Editor.Instance))
            from editor in atomicIO(() => {
                Option<Editor> claimed = available.Filter(_ => state.Value is Attachment.Waiting);
                _ = claimed.Iter(_ => state.Value = new Attachment.Attaching());
                return claimed;
            })
            select editor,
            editor => editor.TraverseM(found => DisposalOps.Acquire(keep => attach(found, keep), (_, release) =>
                atomic(() => {
                    if (state.Value is not Attachment.Attaching) return Fin.Fail<Unit>(Errors.Cancelled);
                    state.Value = new Attachment.Attached(release);
                    return Fin.Succ(unit);
                }).ThrowIfFail())).As(),
            _ => atomicIO(() => state.Swap(static _ => new Attachment.Closed())).Map(static _ => unit),
            static (_, _) => unitIO);

    public static IO<Editor> Show(RhinoDoc document) =>
        IO.lift(static () => Optional(Editor.Instance)).Bind(found => found.Match(
            Some: static _ => IO.lift(static () => Editor.ShowEditor()),
            None: () => Launched(document)));

    private static IO<Editor> Launched(RhinoDoc document) =>
        Macros.ExecuteCommand(document, "_GH2").Bind(static _ => IO.lift(static () => Refused.UnlessFound(Editor.Instance, nameof(Editor.Instance))));

    // --- [GETTERS]
    public static IO<T> RhinoGetter<T>(RhinoDoc document, IO<T> get) =>
        IO.lift(() => Refused.Unless(Editor.BeginRhinoGetter(document), nameof(Editor.BeginRhinoGetter)))
            .Bind(_ => get);
}

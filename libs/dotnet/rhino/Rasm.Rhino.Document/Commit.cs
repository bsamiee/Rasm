using Rhino;
using Rhino.Commands;
using Rhino.Display;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RedrawPolicy {
    public sealed record Silent() : RedrawPolicy;

    public sealed record AllViews(bool Deferred) : RedrawPolicy;

    public sealed record Suppressed(bool RepaintDocument, bool RepaintLayers) : RedrawPolicy;

    public sealed record View(RhinoView Target) : RedrawPolicy;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Commits {
    public static IO<TValue> WithinUndo<TValue>(RhinoDoc doc, string name, IO<TValue> body) =>
        IO.lift(() => (Command.InCommand(), doc.UndoRecordingIsActive, doc.UndoRecordingEnabled)).Bind(recording => recording switch {
            (_, true, _) => body.MapFail(error => error + new UndoRecordOpen(doc.CurrentUndoRecordSerialNumber)),
            (_, false, false) => body,
            (false, false, true) =>
                from serial in IO.lift(() => Answers.Required(doc.BeginUndoRecord(name), nameof(RhinoDoc.BeginUndoRecord)))
                from value in DisposalOps.OnFailure(
                    body,
                    from ended in Ended(doc, serial)
                    from undone in IO.lift(() => Refused.Unless(doc.Undo(), nameof(RhinoDoc.Undo)))
                    from cleared in IO.lift(doc.ClearRedoRecords)
                    select cleared)
                from ended in Ended(doc, serial)
                select value,
            (true, false, true) => IO.fail<TValue>(new UndoRecordUnavailable()),
        });

    public static IO<TValue> WithinRedraw<TValue>(RhinoDoc doc, RedrawPolicy policy, IO<TValue> body) =>
        policy.Switch(
            (Doc: doc, Body: body),
            silent: static (state, _) => state.Body,
            allViews: static (state, all) =>
                from value in state.Body
                from painted in IO.lift(() => state.Doc.Views.Redraw(all.Deferred))
                select value,
            suppressed: static (state, suppressed) =>
                IO.lift(() => {
                    bool prior = state.Doc.Views.RedrawEnabled;
                    state.Doc.Views.EnableRedraw(enable: false, suppressed.RepaintDocument, suppressed.RepaintLayers);
                    return prior;
                }).Bracket(Use: _ => state.Body, Fin: prior => IO.lift(() => state.Doc.Views.EnableRedraw(prior, suppressed.RepaintDocument, suppressed.RepaintLayers))),
            view: static (state, view) =>
                from value in state.Body
                from painted in IO.lift(() => view.Target.Redraw())
                select value);

    public static IO<TValue> Commit<TValue>(RhinoDoc doc, string name, RedrawPolicy redraw, IO<TValue> body) =>
        WithinRedraw(doc, redraw, WithinUndo(doc, name, body));

    private static IO<Unit> Ended(RhinoDoc doc, uint serial) =>
        IO.lift(() => Refused.Unless(doc.EndUndoRecord(serial), nameof(RhinoDoc.EndUndoRecord)));
}

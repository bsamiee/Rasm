using Rhino;
using Rhino.Commands;
using Rhino.Display;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RedrawPolicy {
    public sealed record Silent() : RedrawPolicy;

    public sealed record AllViews(bool Deferred) : RedrawPolicy;

    public sealed record Suppressed(bool Deferred, bool RepaintDocument, bool RepaintLayers) : RedrawPolicy;

    public sealed record View(RhinoView Target) : RedrawPolicy;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record UndoRecord {
    public sealed record Owned(uint Serial) : UndoRecord;

    public sealed record Enlisted(uint Serial) : UndoRecord;

    public sealed record Unavailable(bool InCommand, bool UndoRecordingIsActive) : UndoRecord;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Commits {
    // --- [UNDO]
    public static IO<UndoRecord> BeginUndo(RhinoDoc doc, string name) =>
        IO.lift(() => new UndoRecord.Unavailable(Command.InCommand(), doc.UndoRecordingIsActive) switch {
            { InCommand: false, UndoRecordingIsActive: false } observed => Held(doc.BeginUndoRecord(name), static serial => new UndoRecord.Owned(serial), observed),
            { InCommand: true, UndoRecordingIsActive: true } observed => Held(doc.CurrentUndoRecordSerialNumber, static serial => new UndoRecord.Enlisted(serial), observed),
            UndoRecord.Unavailable observed => observed,
        });

    public static IO<Unit> EndUndo(RhinoDoc doc, UndoRecord undo) =>
        IO.lift(() => undo.Switch(
            doc,
            owned: static (target, owned) => Refused.Unless(target.EndUndoRecord(owned.Serial), nameof(RhinoDoc.EndUndoRecord)),
            enlisted: static (_, _) => unit,
            unavailable: static (_, _) => unit));

    public static IO<Unit> RollbackUndo(RhinoDoc doc, UndoRecord undo) =>
        undo.Switch(
            doc,
            owned: static (target, _) =>
                from undone in IO.lift(() => Refused.Unless(target.Undo(), nameof(RhinoDoc.Undo)))
                from cleared in IO.lift(target.ClearRedoRecords)
                select cleared,
            enlisted: static (_, enlisted) => IO.fail<Unit>(new UndoRecordInCommand(enlisted.Serial)),
            unavailable: static (_, _) => IO.pure(unit));

    public static IO<TValue> WithinUndo<TValue>(RhinoDoc doc, string name, IO<TValue> body) =>
        BeginUndo(doc, name).Bind(undo => undo.Switch(
            (Doc: doc, Body: body),
            owned: static (state, held) => Recorded(state.Doc, held, state.Body),
            enlisted: static (state, held) => Recorded(state.Doc, held, state.Body),
            unavailable: static (_, unavailable) => IO.fail<TValue>(new UndoRecordUnavailable(unavailable.InCommand, unavailable.UndoRecordingIsActive))));

    private static UndoRecord Held(uint serial, Func<uint, UndoRecord> undo, UndoRecord.Unavailable observed) =>
        serial > 0u ? undo(serial) : observed;

    private static IO<TValue> Recorded<TValue>(RhinoDoc doc, UndoRecord undo, IO<TValue> body) =>
        from value in GeometryOps.OnFailure(body, EndUndo(doc, undo).Bind(_ => RollbackUndo(doc, undo)))
        from ended in EndUndo(doc, undo)
        select value;

    // --- [REDRAW]
    public static IO<TValue> WithinRedraw<TValue>(RhinoDoc doc, RedrawPolicy policy, IO<TValue> body) =>
        policy.Switch(
            (Doc: doc, Body: body),
            silent: static (state, _) => state.Body,
            allViews: static (state, all) =>
                from value in state.Body
                from painted in IO.lift(() => state.Doc.Views.Redraw(all.Deferred))
                select value,
            suppressed: static (state, suppressed) =>
                from value in Disposal.Bracketed(
                    IO.lift(() => {
                        bool prior = state.Doc.Views.RedrawEnabled;
                        state.Doc.Views.EnableRedraw(enable: false, suppressed.RepaintDocument, suppressed.RepaintLayers);
                        return prior;
                    }),
                    prior => IO.lift(() => state.Doc.Views.EnableRedraw(prior, suppressed.RepaintDocument, suppressed.RepaintLayers)),
                    _ => state.Body)
                from painted in IO.lift(() => state.Doc.Views.Redraw(suppressed.Deferred))
                select value,
            view: static (state, view) =>
                from value in state.Body
                from painted in IO.lift(() => view.Target.Redraw())
                select value);

    // --- [COMMIT]
    public static IO<TValue> Commit<TValue>(RhinoDoc doc, string name, RedrawPolicy redraw, IO<TValue> body) =>
        WithinRedraw(doc, redraw, WithinUndo(doc, name, body));
}

using Rhino;
using Rhino.Commands;
using Rhino.Display;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record RedrawPolicy {
    public sealed record Silent() : RedrawPolicy;

    public sealed record View(RhinoView Target) : RedrawPolicy;

    public sealed record AllViews(bool Deferred) : RedrawPolicy;

    public sealed record Suppressed(bool Deferred, bool RepaintLayers) : RedrawPolicy;
}

public sealed record Committed<T>(T Value, Option<uint> Record);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Commits {
    public static IO<T> WithinRedraw<T>(RhinoDoc doc, RedrawPolicy redraw, IO<T> body) =>
        redraw.Switch(
            (Document: doc, Body: body),
            silent: static (state, _) => state.Body,
            view: static (state, view) => DisposalOps.Within(static () => unit, _ => state.Body, _ => IO.lift(view.Target.Redraw)),
            allViews: static (state, all) => DisposalOps.Within(static () => unit, _ => state.Body, _ => IO.lift(() => state.Document.Views.Redraw(all.Deferred))),
            suppressed: static (state, suppressed) => DisposalOps.Within(
                () => state.Document.Views.RedrawEnabled,
                _ => IO.lift(() => state.Document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false)).Bind(_ => state.Body),
                prior => Callbacks.Each(Seq(
                    IO.lift(() => state.Document.Views.EnableRedraw(prior, redrawDocument: false, suppressed.RepaintLayers)),
                    when(prior, IO.lift(() => state.Document.Views.Redraw(suppressed.Deferred))).As())).Map(static _ => unit)));

    public static IO<Committed<T>> Commit<T>(RhinoDoc doc, string name, RedrawPolicy redraw, IO<T> body) =>
        WithinRedraw(doc, redraw,
            envIO.Bind(environment => (
            from begun in IO.lift(() => Command.InCommand() && doc.UndoRecordingEnabled && !doc.UndoRecordingIsActive
                ? Fin.Fail<Option<uint>>(new UndoRecordUnavailable()) : Conversions.Present(doc.BeginUndoRecord(name)))
            let ended = IO.lift(() => begun.Traverse(serial => Refused.Unless(doc.EndUndoRecord(serial), serial, nameof(RhinoDoc.EndUndoRecord))).As()
                .Map(closed => closed.Filter(serial => doc.GetUndoRecords().Any(entry => entry.SerialNumber == serial))))
            from value in DisposalOps.OnFailure(IO.liftVAsync(() => body.RunAsync(environment)), ended.Bind(kept => when(kept.IsSome,
                Callbacks.Each(Seq(Step(doc, doc.Undo, nameof(RhinoDoc.Undo)), IO.lift(doc.ClearRedoRecords))).Map(static _ => unit)).As()))
            from record in ended
            select new Committed<T>(value, record)).Uninterruptible()));

    internal static IO<Unit> Step(RhinoDoc doc, Func<bool> step, string member) =>
        IO.lift(() => Refused.Unless(step(), member).Bind(_ => Refused.Unless(doc.EndUndoRecord(doc.CurrentUndoRecordSerialNumber), nameof(RhinoDoc.EndUndoRecord))));
}

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
        use(redraw.Switch(
                doc,
                silent: static (_, _) => IO.pure(IO.pure(unit)),
                view: static (_, view) => IO.pure(IO.lift(view.Target.Redraw)),
                allViews: static (document, all) => IO.pure(IO.lift(() => document.Views.Redraw(all.Deferred))),
                suppressed: static (document, suppressed) =>
                    from prior in IO.lift(() => document.Views.RedrawEnabled)
                    from disabled in IO.lift(() => document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false))
                    select IO.lift(() => document.Views.EnableRedraw(prior, redrawDocument: false, suppressed.RepaintLayers))
                        .Bind(_ => when(prior, IO.lift(() => document.Views.Redraw(suppressed.Deferred))).As())),
            static repaint => repaint).Bind(_ => body).Bracket();

    public static IO<Committed<T>> Commit<T>(RhinoDoc doc, string name, RedrawPolicy redraw, IO<T> body) =>
        WithinRedraw(doc, redraw,
            from begun in IO.lift(() => Command.InCommand() && doc.UndoRecordingEnabled && !doc.UndoRecordingIsActive
                ? Fin.Fail<Option<uint>>(new UndoRecordUnavailable()) : Conversions.Present(doc.BeginUndoRecord(name)))
            let ended = IO.lift(() => begun.Traverse(serial => Refused.Unless(doc.EndUndoRecord(serial), serial, nameof(RhinoDoc.EndUndoRecord))).As()
                .Map(closed => closed.Filter(serial => doc.GetUndoRecords().Any(entry => entry.SerialNumber == serial))))
            from value in DisposalOps.OnFailure(body, ended.Bind(kept => when(kept.IsSome, Step(doc, doc.Undo, nameof(RhinoDoc.Undo)).Bind(_ => IO.lift(doc.ClearRedoRecords))).As()))
            from record in ended
            select new Committed<T>(value, record));

    internal static IO<Unit> Step(RhinoDoc doc, Func<bool> step, string member) =>
        IO.lift(() => Refused.Unless(step(), member).Bind(_ => Refused.Unless(doc.EndUndoRecord(doc.CurrentUndoRecordSerialNumber), nameof(RhinoDoc.EndUndoRecord))));
}

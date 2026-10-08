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
            (Doc: doc, Body: IO.pure(unit).Bind(_ => body)),
            silent: static (state, _) => state.Body,
            view: static (state, view) => state.Body.Finally(IO.lift(view.Target.Redraw)),
            allViews: static (state, all) => state.Body.Finally(IO.lift(() => state.Doc.Views.Redraw(all.Deferred))),
            suppressed: static (state, suppressed) =>
                (from prior in use(
                    () => state.Doc.Views.RedrawEnabled,
                    enabled =>
                        from restored in IO.lift(() => state.Doc.Views.EnableRedraw(enabled, redrawDocument: false, suppressed.RepaintLayers))
                        from painted in when(enabled, IO.lift(() => state.Doc.Views.Redraw(suppressed.Deferred))).As()
                        select painted)
                 from disabled in IO.lift(() => state.Doc.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false))
                 from value in state.Body
                 select value).Bracket());

    public static IO<Committed<T>> Commit<T>(RhinoDoc doc, string name, RedrawPolicy redraw, IO<T> body) =>
        WithinRedraw(doc, redraw, IO.lift(() => (doc.UndoRecordingIsActive, doc.UndoRecordingEnabled, Command.InCommand())).Bind(IO<Committed<T>> ((bool, bool, bool) recording) => recording switch {
            (true, _, _) or (false, false, _) => body.Map(static value => new Committed<T>(value, None)),
            (false, true, false) =>
                from serial in IO.lift(() => Conversions.Required(doc.BeginUndoRecord(name), nameof(RhinoDoc.BeginUndoRecord)))
                let ended =
                    from closed in IO.lift(() => Refused.Unless(doc.EndUndoRecord(serial), nameof(RhinoDoc.EndUndoRecord)))
                    from record in IO.lift(() => doc.GetUndoRecords().AsIterable().Find(entry => entry.SerialNumber == serial).Map(static entry => entry.SerialNumber))
                    select record
                from value in DisposalOps.OnFailure(
                    body,
                    ended.Bind(record => record.Match(
                        Some: _ => IO.lift(() => Refused.Unless(doc.Undo(), nameof(RhinoDoc.Undo)))
                            .Bind(_ => IO.lift(doc.ClearRedoRecords)),
                        None: static () => IO.pure(unit))))
                from record in ended
                select new Committed<T>(value, record),
            (false, true, true) => IO.fail<Committed<T>>(new UndoRecordUnavailable()),
        }));
}

using System.Runtime.CompilerServices;
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
            view: static (state, view) => state.Body.Finally(IO.lift(view.Target.Redraw)),
            allViews: static (state, all) => state.Body.Finally(IO.lift(() => state.Document.Views.Redraw(all.Deferred))),
            suppressed: static (state, suppressed) => IO.lift(() => state.Document.Views.RedrawEnabled).Bind(prior =>
                IO.lift(() => state.Document.Views.EnableRedraw(enable: false, redrawDocument: false, redrawLayers: false))
                >> state.Body.Finally(Callbacks.Each(Seq(
                    IO.lift(() => state.Document.Views.EnableRedraw(prior, redrawDocument: false, suppressed.RepaintLayers)),
                    when(prior, IO.lift(() => state.Document.Views.Redraw(suppressed.Deferred))).As())))));

    public static IO<Committed<T>> Commit<T>(RhinoDoc doc, string name, RedrawPolicy redraw, IO<T> body) =>
        WithinRedraw(doc, redraw,
            from begun in IO.lift(() => RhinoFault.Unless(!Command.InCommand() || !doc.UndoRecordingEnabled || doc.UndoRecordingIsActive)
                .Map(_ => Conversions.Present(doc.BeginUndoRecord(name))))
            from value in body.Catch(error =>
                Ended(doc, begun).Bind(kept => when(kept.IsSome, Callbacks.Each(Seq(Step(doc, doc.Undo), IO.lift(doc.ClearRedoRecords))).Map(unit)).As())
                >> IO.fail<T>(error))
            from record in Ended(doc, begun)
            select new Committed<T>(value, record));

    internal static IO<Unit> Step(RhinoDoc doc, Func<bool> step, [CallerArgumentExpression(nameof(step))] string call = "") =>
        IO.lift(() => RhinoFault.Unless(step(), call: call).Bind(_ => RhinoFault.Unless(doc.EndUndoRecord(doc.CurrentUndoRecordSerialNumber))));

    private static IO<Option<uint>> Ended(RhinoDoc doc, Option<uint> begun) =>
        IO.lift(() => begun.Traverse(serial => RhinoFault.Unless(doc.EndUndoRecord(serial)).Map(_ => serial)).As()
            .Map(closed => closed.Filter(serial => doc.GetUndoRecords().Any(entry => entry.SerialNumber == serial))));
}

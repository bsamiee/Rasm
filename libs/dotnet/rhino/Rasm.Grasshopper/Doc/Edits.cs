using Grasshopper2.Doc;
using Grasshopper2.UI;
using Grasshopper2.Undo;
using Microsoft.VisualStudio.Threading;

namespace Rasm.Grasshopper.Doc;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Edits {
    // --- [RECORDS]
    public static IO<A> Commit<A>(Document document, VerbNoun name, AutoSaveReason reason, Func<ActionList, IO<A>> body) =>
        from actions in IO.lift(static () => ActionList.Empty)
        from value in body(actions).Catch(error => Reverted(document, actions, name) >> IO.fail<A>(error)).As()
        from recorded in Recorded(document, actions, name, reason)
        select value;

    private static IO<Unit> Reverted(Document document, ActionList actions, VerbNoun name) =>
        IO.lift(() => Optional(actions.ToRecord(name))).Bind(record => record.Traverse(held => IO.lift(() => held.Undo(document)) >> Responded(document, held)).As()).Map(unit);

    private static IO<Unit> Recorded(Document document, ActionList actions, VerbNoun name, AutoSaveReason reason) =>
        IO.lift(() => Optional(actions.ToRecord(name))).Bind(record => record.Traverse(held =>
            IO.lift(() => document.Undo.Do(held))
            >> IO.lift(() => ignore(document.File.AutoSave(reason)))
            >> Responded(document, held)).As()).Map(unit);

    private static IO<Unit> Responded(Document document, Record record) =>
        IO.lift(() => {
            if (record.Response.HasFlag(Effects.ExpireLayout) || record.Response.HasFlag(Effects.InvalidateLayout)) _ = Optional(Editor.Instance).Iter(static editor => editor.Canvas.Invalidate());
            if (record.Response.HasFlag(Effects.ExpireSolution) || record.Response.HasFlag(Effects.InvalidateSolution)) document.Solution.Start().Forget();
            if (record.Response.HasFlag(Effects.ExpireDisplay) || record.Response.HasFlag(Effects.InvalidateDisplay)) document.Display.UpdateDisplay();
        });
}

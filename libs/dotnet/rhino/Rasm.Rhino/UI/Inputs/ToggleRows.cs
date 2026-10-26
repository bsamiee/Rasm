using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Rows;
using Rasm.UI;

namespace Rasm.Rhino.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ToggleRows {
    // --- [CHECK_BOXES]
    public static ControlRow Check(string label, bool value, Func<Edit<bool>, IO<Unit>> edited) =>
        new ControlRow.Bound<(bool Value, Func<Edit<bool>, IO<Unit>> Edited)>(label, RowShape.Inline, (value, edited), static (latest, scope, retain) =>
            from box in DisposalOps.Acquire(IO.lift(static () => new CheckBox()), retain)
            let handler = Callbacks.Handler<EventArgs>(
                _ => latest.Bind(row => Optional(box.Checked).Traverse(state => row.Row.Edited(Edit.Commit(state))).As()).Map(static _ => unit),
                new CallbackSite(scope.Sink, typeof(CheckBox), nameof(CheckBox.CheckedChanged)))
            from attached in Subscriptions.Attach(h => box.CheckedChanged += h, h => box.CheckedChanged -= h, handler, retain)
            select new RowCells(box, latest.Bind(row =>
                Subscriptions.Detached(h => box.CheckedChanged += h, h => box.CheckedChanged -= h, handler, IO.lift(() => box.Checked = row.Row.Value).Map(unit)))));
}

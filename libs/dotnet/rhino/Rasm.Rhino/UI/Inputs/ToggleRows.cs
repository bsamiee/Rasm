using Eto.Forms;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.UI;

namespace Rasm.RhinoCommon.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ToggleRows {
    // --- [CHECK_BOXES]
    public static ControlRow Check(string label, bool value, Func<Edit<bool>, IO<Unit>> edited) =>
        new ControlRow.Bound<(bool Value, Func<Edit<bool>, IO<Unit>> Edited)>(label, RowShape.Inline, (value, edited), static (latest, scope) =>
            from box in use(static () => new CheckBox())
            from change in ControlChange<EventArgs>.Attached(
                Subscriptions.Host<EventArgs>(h => box.CheckedChanged += h, h => box.CheckedChanged -= h),
                _ => latest.Bind(row => Optional(box.Checked).Traverse(state => row.Row.Edited(Edit.Commit(state))).As()).Map(static _ => unit),
                scope.Owner)
            select new RowCells(box, latest.Bind(row => change.Detached(IO.lift(() => box.Checked = row.Row.Value).Map(unit)))));
}

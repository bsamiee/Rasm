using Eto.Forms;
using Rasm.Rhino.UI.Rows;

namespace Rasm.Rhino.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ToggleRows {
    // --- [CHECK_BOXES]
    public static ControlRow Check<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, bool> lens, RowRules rules) where TRecord : notnull =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) => Boxed(source, field, lens, scope, retain), rules);

    // --- [REALIZE]
    private static IO<RowCells> Boxed<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, bool> lens, RowScope scope, Action<IDisposable> retain) where TRecord : notnull =>
        DisposalOps.Acquire(owned =>
        from box in DisposalOps.Acquire(IO.lift(static () => new CheckBox()), owned)
        let site = new CallbackSite(scope.Sink, typeof(CheckBox), nameof(CheckBox.CheckedChanged))
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            h => box.CheckedChanged += h, h => box.CheckedChanged -= h,
            edit => Callbacks.Handler<EventArgs>(_ => Ticked(edit, box, source, lens, scope), site),
            value => IO.lift(() => { (box.ThreeState, box.Checked) = (value.IsNone, value.Map(lens.Get).ToNullable()); }),
            scope, site, owned)
        select new RowCells(
            box, None, None, None, [], [], new RowHelp(None, None), bound.Shown, Some<RowEdit>(bound),
            Some(ChoiceRows.Context(box, [], []))), retain);

    private static IO<Unit> Ticked<TRecord>(RowEdit<TRecord> edit, CheckBox box, RowSource<TRecord> source, Lens<TRecord, bool> lens, RowScope scope) where TRecord : notnull =>
        from ticked in IO.lift(() => Optional(box.Checked))
        from held in scope.Read(source)
        from committed in ticked.Traverse(state => edit.Commit(lens.Set(state, held))).As()
        select unit;
}

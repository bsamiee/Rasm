using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;

namespace Rasm.Rhino.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ToggleRows {
    // --- [CHECK_BOXES]
    public static ControlRow Check<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, bool> lens, RowRules rules) where TRecord : notnull =>
        ControlRow.Of(source, field, RowShape.Inline, (scope, retain) => Boxed(source, field, lens, scope, retain), rules);

    // --- [GLYPH_TOGGLES]
    public static IO<ToggleButton> Command(CheckCommand command, (IGlyph Glyph, int Master) glyph, IconSlot slot, IPlugInSink sink, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
        from button in DisposalOps.Acquire(IO.lift(static () => new ToggleButton()), static held => IO.lift(held.Dispose), owned)
        from faced in IO.lift(() => { (button.ToolTip, button.Checked) = (command.ToolTip, command.Checked); })
        from bound in DisposalOps.Change(() => button.Command, value => button.Command = value, command, owned)
        from themed in Icons.Themed(sink, glyph, slot, HostTheme.In(button), icon => button.Image = icon, owned)
        from projected in Subscriptions.Attach(h => command.CheckedChanged += h, h => command.CheckedChanged -= h,
            Callbacks.Handler<EventArgs>(_ => IO.lift(() => { button.Checked = command.Checked; }), new CallbackSite(sink, typeof(CheckCommand), nameof(CheckCommand.CheckedChanged))), owned)
        from changed in Subscriptions.Attach(h => button.CheckedChanged += h, h => button.CheckedChanged -= h,
            Callbacks.Handler<EventArgs>(_ => IO.lift(() => { command.Checked = button.Checked; }), new CallbackSite(sink, typeof(ToggleButton), nameof(ToggleButton.CheckedChanged))), owned)
        select button, (_, lease) => retain(lease));

    // --- [REALIZE]
    private static IO<RowCells> Boxed<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, bool> lens, RowScope scope, Action<IDisposable> retain) where TRecord : notnull =>
        DisposalOps.Acquire(owned =>
        from box in DisposalOps.Acquire(IO.lift(static () => new CheckBox()), static held => IO.lift(held.Dispose), owned)
        let site = new CallbackSite(scope.Sink, typeof(CheckBox), nameof(CheckBox.CheckedChanged))
        from bound in RowEdit.Bind(source, IterableNE.create(field),
            h => box.CheckedChanged += h, h => box.CheckedChanged -= h,
            edit => Callbacks.Handler<EventArgs>(_ =>
                from ticked in IO.lift(() => Optional(box.Checked))
                from held in scope.Read(source)
                from committed in ticked.Traverse(state => edit.Commit(lens.Set(state, held))).As()
                select unit, site),
            value => IO.lift(() => { (box.ThreeState, box.Checked) = (value.IsNone, value.Map(lens.Get).ToNullable()); }),
            scope, site, owned)
        select new RowCells(
            box, None, None, None, [], [], new RowHelp(None, None), bound.Shown, Some<RowEdit>(bound),
            Some(ChoiceRows.Context(box, [], [], scope.Sink))), (_, lease) => retain(lease));
}

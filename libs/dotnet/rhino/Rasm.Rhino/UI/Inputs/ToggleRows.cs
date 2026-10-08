using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;

namespace Rasm.Rhino.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ToggleRows {
    // --- [CHECK_BOXES]
    public static ControlRow Check<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, bool> lens, RowRules rules) where TRecord : notnull =>
        ControlRow.Of(source, field, RowShape.Inline, scope => Boxed(source, field, lens, scope), rules);

    // --- [GLYPH_TOGGLES]
    public static IO<(ToggleButton Control, IDisposable Release)> Command(CheckCommand command, IGlyph glyph, IconSlot slot, IPlugInSink sink) =>
        from button in IO.lift(() => new ToggleButton { Command = command, ToolTip = command.ToolTip })
        let shown = IO.lift(() => { button.Checked = command.Checked; })
        let site = new CallbackSite(sink, typeof(ToggleButton), nameof(ToggleButton.CheckedChanged))
        from seeded in shown
        from held in DisposalOps.AcquireAll(
            Seq(Icons.Themed(sink, glyph, slot, icon => button.Image = icon),
                Subscriptions.Attach(h => command.CheckedChanged += h, h => command.CheckedChanged -= h,
                    Callbacks.Handler<EventArgs>(_ => shown, new CallbackSite(sink, typeof(CheckCommand), nameof(CheckCommand.CheckedChanged)))),
                Subscriptions.Attach(h => button.CheckedChanged += h, h => button.CheckedChanged -= h,
                    Callbacks.Handler<EventArgs>(_ => IO.lift(() => { command.Checked = button.Checked; }), site))),
            DisposalOps.Release)
        select (button, DisposalOps.Composite(held, site));

    // --- [REALIZE]
    private static IO<RowCells> Boxed<TRecord>(RowSource<TRecord> source, RowField<TRecord> field, Lens<TRecord, bool> lens, RowScope scope) where TRecord : notnull =>
        from box in IO.lift(static () => new CheckBox())
        let site = new CallbackSite(scope.Sink, typeof(CheckBox), nameof(CheckBox.CheckedChanged))
        from bound in RowEdit.Bind<TRecord, EventHandler<EventArgs>>(source, IterableNE.create(field),
            h => box.CheckedChanged += h, h => box.CheckedChanged -= h,
            edit => Callbacks.Handler<EventArgs>(_ =>
                from on in IO.lift(() => Optional(box.Checked))
                from held in scope.Read(source)
                from committed in on.Traverse(state => edit.Commit(lens.Set(state, held))).As()
IO.lift(() => {
                (box.ThreeState, box.Checked) = held.Map(lens.Get).Match(Some: static on => (false, (bool?)on), None: static () => (true, (bool?)null));
            }),
            scope, site)
        select new RowCells(
            box, None, None, None, [], [], new RowHelp(None, None), bound.Edit.Shown, Some<RowEdit>(bound.Edit),
            Some(ChoiceRows.Context(box, [], [], scope.Sink)), bound.Release);
}

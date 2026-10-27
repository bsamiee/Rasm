using Eto.Forms;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.RhinoCommon.UI.Styling;
using Rasm.UI;
using Rhino.UI.Controls;

namespace Rasm.RhinoCommon.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ButtonRows {
    // --- [ROWS]
    public static ControlRow Pushed(Command<RowRule, Invoke>.Run run) =>
        new ControlRow.Bound<Command<RowRule, Invoke>.Run>(run.Face.Caption, RowShape.Unlabeled, run, static (latest, scope) =>
            from row in latest
            from button in CommandRows.Button(row.Row, IconSlot.PanelButton, scope)
            select new RowCells(button.Control, button.Refresh)) { Help = run.Face.Help };

    // --- [GROUPS]
    public static IO<(AddRemoveButton Control, IO<Unit> Refresh)> AddRemove(Command<RowRule, Invoke>.Run add, Command<RowRule, Invoke>.Run remove, RowScope scope) =>
        from adding in Bound(add, scope)
        from removing in Bound(remove, scope)
        from pair in use(static () => new AddRemoveButton())
        from faced in IO.lift(() => { (pair.AddToolTip, pair.RemoveToolTip) = (adding.Command.ToolTip, removing.Command.ToolTip); })
        from added in Products.Changed(() => pair.AddCommand, value => pair.AddCommand = value, adding.Command)
        from removed in Products.Changed(() => pair.RemoveCommand, value => pair.RemoveCommand = value, removing.Command)
        from refresh in CommandRows.Listened(Seq(adding.Refresh, removing.Refresh), scope)
        select (pair, refresh);

    public static IO<(StackLayout Control, IO<Unit> Refresh)> Strip(Seq<Seq<Command<RowRule, Invoke>>> groups, IconSlot slot, RowScope scope) =>
        from clusters in CommandRows.Localized(groups, scope.Owner).Filter(static cluster => !cluster.IsEmpty).TraverseM(cluster =>
            from pressed in cluster.TraverseM(command => CommandRows.Pressed(command, slot, scope)).As()
            from row in Containers.Frame(static () => new RhinoButtonRow { VerticalContentAlignment = VerticalAlignment.Center })
            from filled in IO.lift(() => pressed.Iter(made => row.Items.Add(made.Control)))
            select (Row: row, Refreshes: pressed.Bind(static made => made.Refreshes))).As()
        from strip in Containers.Frame(() => new StackLayout([.. clusters.Map(static cluster => (StackLayoutItem)cluster.Row)]) {
            Orientation = Orientation.Horizontal,
            Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Panel).Width,
            VerticalContentAlignment = VerticalAlignment.Center,
        })
        from refresh in CommandRows.Listened(clusters.Bind(static cluster => cluster.Refreshes), scope)
        select (strip, refresh);

    private static IO<(Command Command, IO<Unit> Refresh)> Bound(Command<RowRule, Invoke>.Run run, RowScope scope) =>
        IO.lift(static () => new Command()).Bind(command =>
            CommandRows.Bound(run, CommandRows.Localized(run.Face, Some(run.Invoke), scope.Owner), run.Enabled, command, IO.pure(unit), scope));

    // --- [SEGMENTS]
    internal static IO<TItem> Segment<TItem>() where TItem : SegmentedItem, new() =>
        use(IO.lift(static () => new TItem()), Released);

    private static IO<Unit> Released(SegmentedItem held) =>
        Callbacks.Each(Seq(IO.lift(held.Unbind), IO.lift(held.Dispose))).Map(unit);
}

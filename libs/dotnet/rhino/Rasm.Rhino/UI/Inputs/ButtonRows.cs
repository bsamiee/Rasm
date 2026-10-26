using Eto.Forms;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Inputs;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ButtonRows {
    // --- [GROUPS]
    public static IO<(AddRemoveButton Control, IO<Unit> Refresh)> AddRemove(
        Command<RowRule, Invoke>.Run add, Command<RowRule, Invoke>.Run remove, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from adding in Bound(add, scope, owned)
            from removing in Bound(remove, scope, owned)
            from pair in DisposalOps.Acquire(IO.lift(static () => new AddRemoveButton()), owned)
            from faced in IO.lift(() => { (pair.AddToolTip, pair.RemoveToolTip) = (adding.Command.ToolTip, removing.Command.ToolTip); })
            from added in DisposalOps.Change(() => pair.AddCommand, value => pair.AddCommand = value, adding.Command, owned)
            from removed in DisposalOps.Change(() => pair.RemoveCommand, value => pair.RemoveCommand = value, removing.Command, owned)
            from refresh in CommandRows.Listened(Seq(adding.Watched, removing.Watched), scope, owned)
            select (pair, refresh), retain);

    public static IO<(StackLayout Control, IO<Unit> Refresh)> Strip(Seq<Seq<Command<RowRule, Invoke>>> groups, IconSlot slot, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from clusters in CommandRows.Localized(groups, scope.Sink).Filter(static cluster => !cluster.IsEmpty).TraverseM(cluster =>
                from pressed in cluster.TraverseM(command => CommandRows.Pressed(command, slot, scope, owned)).As()
                from row in Containers.Frame(static () => new RhinoButtonRow { VerticalContentAlignment = VerticalAlignment.Center }, owned)
                from filled in IO.lift(() => pressed.Iter(made => row.Items.Add(made.Control)))
                select (Row: row, Watched: pressed.Bind(static made => made.Watched))).As()
            from strip in Containers.Frame(() => new StackLayout([.. clusters.Map(static cluster => (StackLayoutItem)cluster.Row)]) {
                Orientation = Eto.Forms.Orientation.Horizontal,
                Spacing = RhinoLayout.Spacing(RhinoLayout.SpacingType.Panel).Width,
                VerticalContentAlignment = VerticalAlignment.Center,
            }, owned)
            from refresh in CommandRows.Listened(clusters.Bind(static cluster => cluster.Watched), scope, owned)
            select (strip, refresh), retain);

    private static IO<(Command Command, Watched Watched)> Bound(Command<RowRule, Invoke>.Run run, RowScope scope, Action<IDisposable> retain) =>
        IO.lift(static () => new Command()).Bind(command =>
            CommandRows.Bound(run, CommandRows.Localized(run.Face, Some(run.Invoke), scope.Sink), run.Enabled, command, IO.pure(unit), scope, retain));

    // --- [SEGMENTS]
    internal static IO<TItem> Segment<TItem>(Action<IDisposable> retain) where TItem : SegmentedItem, new() =>
        DisposalOps.Acquire(IO.lift(static () => new TItem()), Released, retain);

    private static IO<Unit> Released(SegmentedItem held) =>
        Callbacks.Each(Seq(IO.lift(held.Unbind), IO.lift(held.Dispose))).Map(unit);
}

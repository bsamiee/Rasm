using Eto.Forms;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SplitAnchor(string Name, double Default, SplitterFixedPanel Fixed) {
    private const string Positions = "splitter-positions";

    public static SplitAnchor Shared(string name) => new(name, 0.5d, SplitterFixedPanel.None);

    public Preference<double> Position => Preferences.Setting(ViewCatalog.Settings.Child(Positions), Name, Default, hidden: true);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContainerRow {
    public Seq<Child> Children =>
        Switch(
            split: static split => toSeq(split.First) + toSeq(split.Second),
            tabs: static tabs => toSeq(tabs.Pages).Bind(static page => toSeq(page.Body)),
            disclosure: static disclosure => toSeq(disclosure.Body),
            tiles: static _ => Seq<Child>());

    public bool Fill => Map(split: true, tabs: true, disclosure: false, tiles: false);

    public sealed record Split(IterableNE<Child> First, IterableNE<Child> Second, Orientation Orientation, SplitAnchor Anchor) : ContainerRow;

    public sealed record Tabs(IterableNE<(string Caption, IterableNE<Child> Body)> Pages) : ContainerRow;

    public sealed record Disclosure(string Caption, bool Expanded, IterableNE<Child> Body) : ContainerRow;

    public sealed record Tiles(IterableNE<Command<Invoke>> Cells) : ContainerRow;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Containers {
    // --- [FRAMES]
    public static IO<T> Frame<T>(Func<T> create) where T : Container => use(IO.lift(create), Emptied);

    private static IO<Unit> Emptied(Container frame) =>
        Products.Each(Seq(IO.lift(frame is StackLayout stack ? stack.Items.Clear : frame.RemoveAll), IO.lift(frame.Dispose))).Map(unit);

    // --- [REALIZATION]
    public static IO<Control> Realize(ContainerRow row, RowScope scope) =>
        row.Switch(
            scope,
            split: static (held, split) => Split(split, held),
            tabs: static (held, tabs) => Tabs(tabs, held),
            disclosure: static (held, disclosure) => SectionHolder.Disclose(disclosure, held).Map(static holder => (Control)holder),
            tiles: static (held, tiles) => Tiles(tiles, held));

    // --- [SPLITS]
    private static IO<Control> Split(ContainerRow.Split split, RowScope scope) =>
        from position in split.Anchor.Position.Read
        from splitter in use(static () => new Splitter())
        from first in ViewOps.Realize(toSeq(split.First), scope)
        from second in ViewOps.Realize(toSeq(split.Second), scope)
        from configured in IO.lift(() => {
            (splitter.ID, splitter.Orientation, splitter.FixedPanel, splitter.SplitterWidth) =
                (split.Anchor.Name, split.Orientation, split.Anchor.Fixed, RhinoLayout.SplitterWidth);
            (splitter.Panel1, splitter.Panel2, splitter.RelativePosition) = (first, second, position);
        })
        from attached in Subscriptions.On<EventArgs>(h => splitter.PositionChangeCompleted += h, h => splitter.PositionChangeCompleted -= h)
            .Attach(_ => IO.lift(() => splitter.RelativePosition).Bind(split.Anchor.Position.Write))
        select (Control)splitter;

    // --- [TABS]
    private static IO<Control> Tabs(ContainerRow.Tabs tabs, RowScope scope) =>
        from control in use(static () => new TabControl())
        from pages in toSeq(tabs.Pages).TraverseM(page =>
            from tab in use(static () => new TabPage())
            let paged = scope.Separated(visible: false)
            from body in ViewOps.Realize(toSeq(page.Body), paged)
            from seated in IO.lift(() => {
                tab.Text = LocalText.Of(page.Caption).Local;
                tab.Content = body;
                control.Pages.Add(tab);
            })
            from sections in paged.OnVisibility(visible => SectionHolder.HolderVisible(body, visible))
            select (Tab: tab, Scope: paged)).As()
        let shown = scope.Visibility.Bind(visible => Products.Each(pages.Map(page => page.Scope.Shown(visible && ReferenceEquals(page.Tab, control.SelectedPage))))).Map(static _ => unit)
        from enclosing in scope.OnVisibility(_ => shown)
        from selected in Subscriptions.On<EventArgs>(h => control.SelectedIndexChanged += h, h => control.SelectedIndexChanged -= h).Attach(_ => shown)
        select (Control)control;

    // --- [TILES]
    private static IO<Control> Tiles(ContainerRow.Tiles tiles, RowScope scope) =>
        from row in use(static () => new TopRowButtonLayout())
        from cells in toSeq(tiles.Cells).TraverseM(command => CommandRows.Button(command, IconSlot.PanelButton, scope)).As()
        from configured in IO.lift(() => {
            row.ItemPadding = RhinoLayout.Padding(RhinoLayout.PaddingType.ButtonRow);
            _ = cells.Iter(cell => row.Items.Add(cell.Control));
        })
        select (Control)row;
}

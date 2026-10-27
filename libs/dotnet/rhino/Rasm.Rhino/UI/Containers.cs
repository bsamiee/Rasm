using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SplitAnchor(string Name, double Default, SplitterFixedPanel Fixed) {
    private const string Positions = "splitter-positions";

    public static SplitAnchor Shared(string name) => new(name, 0.5d, SplitterFixedPanel.None);

    public static SplitAnchor Pinned(string name, double extent, SplitterFixedPanel side) => new(name, extent, side);

    private PlugInSetting<double> Row =>
        new(Name, Default, static (settings, key, out value, legacy) => settings.TryGetDouble(key, out value, legacy), static (settings, key, value) => settings.SetDouble(key, value));

    public IO<double> Open(SettingsNode plugIn) => PlugInSettings.Store(plugIn.Child(Positions), Row).Read.Map(held => held.IfNone(Default));

    public IO<Unit> Keep(SettingsNode plugIn, double position) =>
        from child in SettingRoots.AddChild(plugIn.Child(Positions))
        from hidden in IO.lift(() => child.HiddenFromUserInterface = true)
        from written in PlugInSettings.Store(plugIn.Child(Positions), Row).Put(Some(position))
        select unit;
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
public static class Containers {
    // --- [FRAMES]
    public static IO<T> Frame<T>(Func<T> create) where T : Container => use(IO.lift(create), Emptied);

    private static IO<Unit> Emptied(Container frame) =>
        Callbacks.Each(Seq(IO.lift(frame is StackLayout stack ? stack.Items.Clear : frame.RemoveAll), IO.lift(frame.Dispose))).Map(unit);

    // --- [REALIZATION]
    public static IO<Control> Realize(IPlugInViews owner, ContainerRow row, RowScope scope) =>
        row.Switch(
            (Owner: owner, Scope: scope),
            split: static (state, split) => Split(state.Owner, split, state.Scope),
            tabs: static (state, tabs) => Tabs(state.Owner, tabs, state.Scope),
            disclosure: static (state, disclosure) => SectionHolder.Disclose(state.Owner, disclosure, state.Scope).Map(static holder => (Control)holder),
            tiles: static (state, tiles) => Tiles(tiles, state.Scope));

    // --- [SPLITS]
    private static IO<Control> Split(IPlugInViews owner, ContainerRow.Split split, RowScope scope) =>
        from position in split.Anchor.Open(owner.Settings)
        from splitter in use(static () => new Splitter())
        from first in ViewOps.Realize(owner, toSeq(split.First), scope)
        from second in ViewOps.Realize(owner, toSeq(split.Second), scope)
        from configured in IO.lift(() => {
            (splitter.ID, splitter.Orientation, splitter.FixedPanel, splitter.SplitterWidth) =
                (split.Anchor.Name, split.Orientation, split.Anchor.Fixed, RhinoLayout.SplitterWidth);
            (splitter.Panel1, splitter.Panel2, splitter.RelativePosition) = (first, second, position);
        })
        from attached in Subscriptions.Host<EventArgs>(h => splitter.PositionChangeCompleted += h, h => splitter.PositionChangeCompleted -= h)
            .Inline(_ => IO.lift(() => splitter.RelativePosition).Bind(moved => split.Anchor.Keep(owner.Settings, moved)), owner)
        select (Control)splitter;

    // --- [TABS]
    private static IO<Control> Tabs(IPlugInViews owner, ContainerRow.Tabs tabs, RowScope scope) =>
        from control in use(static () => new TabControl())
        from pages in toSeq(tabs.Pages).TraverseM(page =>
            from tab in use(static () => new TabPage())
            let paged = scope.Separated(visible: false)
            from body in ViewOps.Realize(owner, toSeq(page.Body), paged)
            from seated in IO.lift(() => {
                tab.Text = RowText.Localize(page.Caption, owner).Local;
                tab.Content = body;
                control.Pages.Add(tab);
            })
            from sections in paged.OnVisibility(visible => SectionHolder.Shown(body, visible))
            select (Tab: tab, Scope: paged)).As()
        let shown = scope.Visibility.Bind(visible => Callbacks.Each(pages.Map(page => page.Scope.Shown(visible && ReferenceEquals(page.Tab, control.SelectedPage))))).Map(static _ => unit)
        from enclosing in scope.OnVisibility(_ => shown)
        from selected in Subscriptions.Host<EventArgs>(h => control.SelectedIndexChanged += h, h => control.SelectedIndexChanged -= h).Inline(_ => shown, owner)
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

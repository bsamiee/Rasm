using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
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

    public sealed record Tiles(IterableNE<Command<RowRule, Invoke>> Cells) : ContainerRow;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Containers {
    // --- [FRAMES]
    public static IO<T> Frame<T>(Func<T> create, Action<IDisposable> retain) where T : Container =>
        DisposalOps.Acquire(IO.lift(create), Emptied, retain);

    private static IO<Unit> Emptied(Container frame) =>
        Callbacks.Each(Seq(IO.lift(frame is StackLayout stack ? stack.Items.Clear : frame.RemoveAll), IO.lift(frame.Dispose))).Map(unit);

    // --- [REALIZATION]
    public static IO<Control> Realize(IPlugInViews owner, ContainerRow row, RowScope scope, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned => row.Switch(
            (Owner: owner, Scope: scope, Retain: owned),
            split: static (state, split) => Split(state.Owner, split, state.Scope, state.Retain),
            tabs: static (state, tabs) => Tabs(state.Owner, tabs, state.Scope, state.Retain),
            disclosure: static (state, disclosure) => Disclosure(state.Owner, disclosure, state.Scope, state.Retain),
            tiles: static (state, tiles) => Tiles(tiles, state.Scope, state.Retain)), retain);

    // --- [SPLITS]
    private static IO<Control> Split(IPlugInViews owner, ContainerRow.Split split, RowScope scope, Action<IDisposable> retain) =>
        from position in split.Anchor.Open(owner.Settings)
        from splitter in DisposalOps.Acquire(IO.lift(static () => new Splitter()), retain)
        from first in ViewOps.Realize(owner, toSeq(split.First), scope, retain)
        from second in ViewOps.Realize(owner, toSeq(split.Second), scope, retain)
        from configured in IO.lift(() => {
            (splitter.ID, splitter.Orientation, splitter.FixedPanel, splitter.SplitterWidth) =
                (split.Anchor.Name, split.Orientation, split.Anchor.Fixed, RhinoLayout.SplitterWidth);
            (splitter.Panel1, splitter.Panel2, splitter.RelativePosition) = (first, second, position);
        })
        from attached in Subscriptions.Attach(
            handler => splitter.PositionChangeCompleted += handler, handler => splitter.PositionChangeCompleted -= handler,
            Callbacks.Handler<EventArgs>(_ => IO.lift(() => splitter.RelativePosition).Bind(moved => split.Anchor.Keep(owner.Settings, moved)),
                new CallbackSite(scope.Sink, typeof(Splitter), nameof(Splitter.PositionChangeCompleted))), retain)
        select (Control)splitter;

    // --- [TABS]
    private static IO<Control> Tabs(IPlugInViews owner, ContainerRow.Tabs tabs, RowScope scope, Action<IDisposable> retain) =>
        from control in DisposalOps.Acquire(IO.lift(static () => new TabControl()), retain)
        from pages in toSeq(tabs.Pages).TraverseM(page =>
            from tab in DisposalOps.Acquire(IO.lift(static () => new TabPage()), retain)
            let paged = scope.Separated(visible: false)
            from body in ViewOps.Realize(owner, toSeq(page.Body), paged, retain)
            from seated in IO.lift(() => { tab.Text = RowText.Localize(page.Caption, scope.Sink).Local; tab.Content = body; control.Pages.Add(tab); })
            from sections in paged.OnVisibility(visible => SectionHolder.Shown(body, visible), retain)
            select (Tab: tab, Scope: paged)).As()
        let shown = scope.Visibility.Bind(visible => Callbacks.Each(pages.Map(page => page.Scope.Shown(visible && ReferenceEquals(page.Tab, control.SelectedPage))))).Map(static _ => unit)
        from enclosing in scope.OnVisibility(_ => shown, retain)
        from selected in Subscriptions.Attach(
            handler => control.SelectedIndexChanged += handler, handler => control.SelectedIndexChanged -= handler,
            Callbacks.Handler<EventArgs>(_ => shown, new CallbackSite(scope.Sink, typeof(TabControl), nameof(TabControl.SelectedIndexChanged))), retain)
        select (Control)control;

    // --- [DISCLOSURES]
    private static IO<Control> Disclosure(IPlugInViews owner, ContainerRow.Disclosure disclosure, RowScope scope, Action<IDisposable> retain) =>
        SectionHolder.Disclose(owner, disclosure, scope, retain).Map(static holder => (Control)holder);

    // --- [TILES]
    private static IO<Control> Tiles(ContainerRow.Tiles tiles, RowScope scope, Action<IDisposable> retain) =>
        from row in DisposalOps.Acquire(IO.lift(static () => new TopRowButtonLayout()), retain)
        from cells in toSeq(tiles.Cells).TraverseM(command => CommandRows.Button(command, IconSlot.PanelButton, scope, retain)).As()
        from configured in IO.lift(() => {
            row.ItemPadding = RhinoLayout.Padding(RhinoLayout.PaddingType.ButtonRow);
            _ = cells.Iter(cell => row.Items.Add(cell.Control));
        })
        select (Control)row;
}

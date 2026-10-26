using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct SplitShare : System.Numerics.IMinMaxValue<SplitShare> {
    public static SplitShare MinValue { get; } = new(0d);
    public static SplitShare MaxValue { get; } = new(1d);
    public static SplitShare Even { get; } = new(0.5d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct PaneExtent : System.Numerics.IMinMaxValue<PaneExtent> {
    public static PaneExtent MinValue { get; } = new(0d);
    public static PaneExtent MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

public abstract record SplitAnchor {
    private protected SplitAnchor(SplitterFixedPanel fixedPanel) => Fixed = fixedPanel;

    public SplitterFixedPanel Fixed { get; }

    public abstract string Name { get; }

    public static SplitAnchor Shared(ValueKey<SplitShare, double> key) => new SplitAnchor<SplitShare>(key, SplitterFixedPanel.None);

    public static SplitAnchor Pinned(ValueKey<PaneExtent, double> key, SplitterFixedPanel side) => new SplitAnchor<PaneExtent>(key, side);

    public abstract IO<double> Open(SettingsNode plugIn, CallbackSite site);

    public abstract IO<Unit> Keep(SettingsNode plugIn, double position);
}

internal sealed record SplitAnchor<TValue> : SplitAnchor where TValue : notnull {
    private const string Positions = "splitter-positions";

    internal SplitAnchor(ValueKey<TValue, double> key, SplitterFixedPanel fixedPanel) : base(fixedPanel) =>
        Row = new(key, SettingType.Double);

    private PlugInSetting<TValue, double> Row { get; }

    public override string Name => Row.Value.Name;

    public override IO<double> Open(SettingsNode plugIn, CallbackSite site) =>
        PlugInSettings.Store(plugIn.Child(Positions), Row).Read
            .Catch(
                static error => error.HasCode(ErrorCodes.ParseError) || error.HasCode(ErrorCodes.ValidationFailed),
                error => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member)).Map(Option<TValue>.None))
            .Map(held => Row.Value.ToRaw(held.IfNone(Row.Value.Default)));

    public override IO<Unit> Keep(SettingsNode plugIn, double position) =>
        from value in IO.lift(Row.Value.From(position))
        let positions = plugIn.Child(Positions)
        from child in SettingRoots.AddChild(positions)
        from hidden in IO.lift(() => child.HiddenFromUserInterface = true)
        from written in PlugInSettings.Store(positions, Row).Put(Some(value))
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

    public sealed record Tiles(IterableNE<CommandRow> Cells) : ContainerRow;
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
        from position in split.Anchor.Open(owner.Settings, new CallbackSite(scope.Sink, typeof(SplitAnchor), nameof(SplitAnchor.Open)))
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
            from body in ViewOps.Realize(owner, toSeq(page.Body), scope, retain)
            from seated in IO.lift(() => { tab.Text = RowText.Localize(page.Caption, scope.Sink).Local; tab.Content = body; control.Pages.Add(tab); })
            select unit).As()
        from selected in Subscriptions.Attach(
            handler => control.SelectedIndexChanged += handler, handler => control.SelectedIndexChanged -= handler,
            Callbacks.Handler<EventArgs>(_ => scope.Visibility.Bind(visible => SectionHolder.Shown(control, visible)),
                new CallbackSite(scope.Sink, typeof(TabControl), nameof(TabControl.SelectedIndexChanged))), retain)
        select (Control)control;

    // --- [DISCLOSURES]
    private static IO<Control> Disclosure(IPlugInViews owner, ContainerRow.Disclosure disclosure, RowScope scope, Action<IDisposable> retain) =>
        SectionHolder.Disclose(owner, disclosure, scope, retain).Map(static holder => (Control)holder);

    // --- [TILES]
    private static IO<Control> Tiles(ContainerRow.Tiles tiles, RowScope scope, Action<IDisposable> retain) =>
        from row in DisposalOps.Acquire(IO.lift(static () => new TopRowButtonLayout()), retain)
        from cells in toSeq(tiles.Cells).TraverseM(command => Tile(command, scope, retain)).As()
        let refresh = Callbacks.Each(cells.Map(static cell => cell.Refresh)).Map(unit)
        from signals in toSeq(tiles.Cells).Bind(static command => command.Enabled.ToSeq()).Bind(static rule => rule.Sources).Distinct()
            .TraverseM(source => source.Signals(scope)).As()
        from heard in scope.Listen(_ => refresh, retain)
        from attached in signals.Flatten().TraverseM(signal => signal.Inline(_ => refresh, scope.Sink, retain)).As()
        from configured in IO.lift(() => {
            row.ItemPadding = RhinoLayout.Padding(RhinoLayout.PaddingType.ButtonRow);
            _ = cells.Bind(static cell => cell.Buttons).Iter(row.Items.Add);
        })
        from shown in refresh
        select (Control)row;

    private static IO<(Seq<ImageButton> Buttons, IO<Unit> Refresh)> Tile(CommandRow command, RowScope scope, Action<IDisposable> retain) =>
        from realized in command.Commands(scope, retain)
        from buttons in realized.Commands.TraverseM(cell =>
            from glyph in IO.lift(cell.Face.Glyph.ToFin(new Refused(nameof(CommandFace.Glyph), Refused.Absent)))
            from button in ButtonRows.Image(cell.Command, glyph, IconSlot.PanelButton, scope.Sink, retain)
            select button).As()
        select (buttons, realized.Refresh);
}

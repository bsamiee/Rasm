using Eto.Forms;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct SplitShare : System.Numerics.IMinMaxValue<SplitShare> {
    public static SplitShare MinValue { get; } = new(0d);
    public static SplitShare MaxValue { get; } = new(1d);
    public static SplitShare Even { get; } = new(0.5d);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<InvalidRhinoValue>]
public readonly partial struct PaneExtent : System.Numerics.IMinMaxValue<PaneExtent> {
    public static PaneExtent MinValue { get; } = new(0d);
    public static PaneExtent MaxValue { get; } = new(double.MaxValue);

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref double value) =>
        validationError = value.CompareTo(MinValue._value) >= 0 && value.CompareTo(MaxValue._value) <= 0 ? null : new InvalidRhinoValue();
}

[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class PaneSide {
    public static readonly PaneSide First = new(SplitterFixedPanel.Panel1);
    public static readonly PaneSide Second = new(SplitterFixedPanel.Panel2);

    public SplitterFixedPanel Fixed { get; }
}

public abstract record SplitAnchor {
    private protected SplitAnchor(SplitterFixedPanel fixedPanel) => Fixed = fixedPanel;

    public SplitterFixedPanel Fixed { get; }

    public abstract string Name { get; }

    public static SplitAnchor Shared(ValueKey<SplitShare, double, InvalidRhinoValue> key) => new SplitAnchor<SplitShare>(key, SplitterFixedPanel.None);

    public static SplitAnchor Pinned(ValueKey<PaneExtent, double, InvalidRhinoValue> key, PaneSide side) => new SplitAnchor<PaneExtent>(key, side.Fixed);

    public abstract IO<double> Open(SettingsNode plugIn, CallbackSite site);

    public abstract IO<Unit> Keep(SettingsNode plugIn, double position);
}

internal sealed record SplitAnchor<TValue> : SplitAnchor where TValue : notnull {
    private const string Positions = "splitter-positions";

    internal SplitAnchor(ValueKey<TValue, double, InvalidRhinoValue> key, SplitterFixedPanel fixedPanel) : base(fixedPanel) =>
        Row = new(key, SettingType.Double, Applied.Live, Seq<string>(), Hidden: false);

    private PlugInSetting<TValue, double, InvalidRhinoValue> Row { get; }

    public override string Name => Row.Value.Name;

    public override IO<double> Open(SettingsNode plugIn, CallbackSite site) =>
        PlugInSettings.Store(plugIn.Child(Positions), Row).Read
            .Catch(
                static error => error.IsType<UnreadText>() || error.IsType<InvalidRhinoValue>(),
                error => IO.lift(() => site.Sink.Report(error, site.Owner, site.Member)).Map(static _ => Option<TValue>.None))
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
    // --- [REALIZATION]
    public static IO<ViewBody> Realize(IPlugInViews owner, ContainerRow row, RowScope scope) =>
        row.Switch(
            (Owner: owner, Scope: scope),
            split: static (state, split) => Split(state.Owner, split, state.Scope),
            tabs: static (state, tabs) => Tabs(state.Owner, tabs, state.Scope),
            disclosure: static (state, disclosure) => Disclosure(state.Owner, disclosure, state.Scope),
            tiles: static (state, tiles) => Tiles(tiles, state.Scope));

    // --- [SPLITS]
    private static IO<ViewBody> Split(IPlugInViews owner, ContainerRow.Split split, RowScope scope) =>
        from position in split.Anchor.Open(owner.Settings, new CallbackSite(scope.Sink, typeof(SplitAnchor), nameof(SplitAnchor.Open)))
        from first in ViewOps.Realize(owner, toSeq(split.First), scope)
        from second in DisposalOps.OnFailure(ViewOps.Realize(owner, toSeq(split.Second), scope), first.Release)
        let release = ViewOps.Released(Seq(first, second))
        from splitter in DisposalOps.OnFailure(IO.lift(() => {
            Splitter held = new() {
                ID = split.Anchor.Name,
                Orientation = split.Orientation,
                FixedPanel = split.Anchor.Fixed,
                SplitterWidth = RhinoLayout.SplitterWidth,
                Panel1 = first.Content,
                Panel2 = second.Content,
                RelativePosition = position,
            };
            held.PositionChangeCompleted += Callbacks.Handler<EventArgs>(
                _ => IO.lift(() => held.RelativePosition).Bind(moved => split.Anchor.Keep(owner.Settings, moved)),
                new CallbackSite(scope.Sink, typeof(Splitter), nameof(Splitter.PositionChangeCompleted)));
            return held;
        }), release)
        select new ViewBody(splitter, release);

    // --- [TABS]
    private static IO<ViewBody> Tabs(IPlugInViews owner, ContainerRow.Tabs tabs, RowScope scope) =>
        from pages in DisposalOps.AcquireAll(
            toSeq(tabs.Pages).Map(page => ViewOps.Realize(owner, toSeq(page.Body), scope).Map(body => (page.Caption, Body: body))),
            static held => ViewOps.Released(held.Map(static page => page.Body)))
        let release = ViewOps.Released(pages.Map(static page => page.Body))
        from control in DisposalOps.OnFailure(IO.lift(() => {
            TabControl held = new();
            _ = pages.Iter(page => held.Pages.Add(new TabPage(page.Body.Content) { Text = Wording.English.Shown(page.Caption, scope.Sink) }));
            return held;
        }), release)
        select new ViewBody(control, release);

    // --- [DISCLOSURES]
    private static IO<ViewBody> Disclosure(IPlugInViews owner, ContainerRow.Disclosure disclosure, RowScope scope) =>
        from body in ViewOps.Realize(owner, toSeq(disclosure.Body), scope)
        from expander in DisposalOps.OnFailure(
            IO.lift(() => new Expander {
                Header = new Label { Text = Wording.English.Shown(disclosure.Caption, scope.Sink) },
                Expanded = disclosure.Expanded,
                Content = body.Content,
            }),
            body.Release)
        select new ViewBody(expander, body.Release);

    // --- [TILES]
    private static IO<ViewBody> Tiles(ContainerRow.Tiles tiles, RowScope scope) =>
        from cells in DisposalOps.AcquireAll(
            toSeq(tiles.Cells).Map(command => Tile(command, scope)),
            static held => DisposalOps.Release(held.Bind(static cell => cell.Release)))
        let owned = cells.Bind(static cell => cell.Release)
        let refresh = cells.TraverseM(static cell => cell.Refresh).As().Map(static _ => unit)
        from signals in DisposalOps.OnFailure(
            toSeq(tiles.Cells).Bind(static command => command.Enabled.ToSeq()).Bind(static rule => rule.Sources).Distinct().TraverseM(source => source.Signals(scope)).As(),
            DisposalOps.Release(owned))
        from heard in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(scope.Listen(_ => refresh).Cons(signals.Flatten().Map(signal => signal.Inline(_ => refresh, scope.Sink))), DisposalOps.Release),
            DisposalOps.Release(owned))
        let held = DisposalOps.Composite(owned + heard, new CallbackSite(scope.Sink, typeof(Containers), nameof(Tiles)))
        let release = IO.lift(held.Dispose)
        from row in DisposalOps.OnFailure(IO.lift(() => {
            TopRowButtonLayout laid = new() { ItemPadding = RhinoLayout.Padding(RhinoLayout.PaddingType.ButtonRow) };
            _ = cells.Bind(static cell => cell.Buttons).Iter(laid.Items.Add);
            return laid;
        }), release)
        from shown in DisposalOps.OnFailure(refresh, release)
        select new ViewBody(row, release);

    private static IO<(Seq<ImageButton> Buttons, IO<Unit> Refresh, Seq<IDisposable> Release)> Tile(CommandRow command, RowScope scope) =>
        from realized in command.Switch(
            scope,
            run: static (held, run) => run.Realize(held).Map(done =>
                new Realized<Seq<(Command Command, CommandFace Face)>>(Seq<(Command Command, CommandFace Face)>((done.Commands, run.Face)), done.Refresh, done.Release)),
            check: static (held, check) => check.Realize(held).Map(done =>
                new Realized<Seq<(Command Command, CommandFace Face)>>(Seq<(Command Command, CommandFace Face)>((done.Commands, check.Face)), done.Refresh, done.Release)),
            radio: static (held, radio) => radio.Realize(held).Map(static done =>
                new Realized<Seq<(Command Command, CommandFace Face)>>(done.Commands.Map<(Command Command, CommandFace Face)>(static member => (member.Command, member.Face)), done.Refresh, done.Release)))
        from buttons in DisposalOps.OnFailure(
            DisposalOps.AcquireAll(
                realized.Commands.Map(cell =>
                    from glyph in IO.lift(cell.Face.Glyph.ToFin(new Missing(nameof(CommandFace.Glyph))))
                    from button in ButtonRows.Image(cell.Command, glyph, IconSlot.PanelButton, scope.Sink)
                    select button),
                static held => DisposalOps.Release(held.Map(static button => button.Release))),
            IO.lift(realized.Release.Dispose))
        select (buttons.Map(static button => button.Control), realized.Refresh, realized.Release.Cons(buttons.Map(static button => button.Release)));
}

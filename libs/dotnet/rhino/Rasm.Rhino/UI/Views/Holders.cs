using System.Drawing;
using Eto.Forms;
using NodaTime.Text;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HolderRow {
    public Seq<View.Section> Sections =>
        Switch(
            stacked: static stacked => stacked.Members,
            filled: static filled => filled.Above.Add(filled.Fill) + filled.Below);

    public sealed record Stacked(Seq<View.Section> Members) : HolderRow;

    public sealed record Filled(Seq<View.Section> Above, View.Section Fill, Seq<View.Section> Below) : HolderRow;
}

internal sealed record HeaderButton(GlyphRole Glyph, string ToolTip, IO<Unit> Click);

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SectionBody : IDisposable {
    // --- [STATE]
    private readonly View.Section row;
    private readonly Seq<ControlRow> controls;
    private readonly RowScope scope;
    private readonly RowRules rules;
    private readonly LanguageExt.HashSet<EntryKey> reads;
    private readonly IDisposable release;
    private readonly Atom<bool> hidden = Atom(false);
    private readonly ShownSubscription listeners;

    private SectionBody(View.Section row, Seq<ControlRow> controls, Control content, RowScope scope, Option<IHeaderButtonHandler> header, IDisposable release, CallbackSite site) {
        (this.row, this.controls, Content, this.scope, Header, this.release) = (row, controls, content, scope, header, release);
        rules = new RowRules(row.Enabled, row.Visible);
        reads = toHashSet(rules.Each.Bind(static rule => rule.Reads));
        listeners = new ShownSubscription(Listening(site), site);
    }

    public Control Content { get; }
    public Option<IHeaderButtonHandler> Header { get; }
    public bool Hidden => hidden.Value;
    public int Height => hidden.Value ? 0 : (int)Math.Ceiling(Content.GetPreferredSize().Height);

    // --- [REALIZATION]
    public static IO<SectionBody> Realize(IPlugInViews owner, View.Section row, RowScope enclosing) =>
        from history in row.Store.Traverse(store => IO.lift(ViewOps.Bindings(owner, row, enclosing).ToFin().Bind(group => ViewOps.History(owner, row, store, group, enclosing.Document)))).As()
        let scope = enclosing.Nested(ViewOps.Settings(owner, row), history, ViewOps.Locks(row, enclosing.Document))
        let controls = row.Children.Bind(static child => child.Controls)
        let site = new CallbackSite(owner, row.Identity, nameof(Realize))
        from realized in ViewOps.Realize(owner, row.Children, scope)
        from header in DisposalOps.OnFailure(history.Traverse(held => Header(owner, row, controls, held, scope)).As(), realized.Release)
        from section in IO.lift(() => new SectionBody(
            row, controls, realized.Content, scope,
            header.Map(static held => (IHeaderButtonHandler)held.Handler),
            DisposalOps.Composite(((IDisposable)new Disposal<IO<Unit>>(realized.Release, run => _ = Callbacks.Answer(run, static () => unit, site))).Cons(header.ToSeq().Bind(static held => held.Menus)), site),
            new CallbackSite(owner, row.Identity, nameof(DefinedSection.HolderVisible))))
        from evaluated in DisposalOps.OnFailure(section.Evaluated, IO.lift(section.Dispose))
        select section;

    // --- [EDGES]
    public Unit Shown(bool visible) => listeners.Shown(visible);

    public IO<Unit> Reread => scope.Changed(RealizedView.Keys(row));

    public void Dispose() {
        listeners.Dispose();
        release.Dispose();
    }

    // --- [RULES]
    private IO<Unit> Evaluated =>
        from shows in rules.Shows(scope)
        from enables in rules.Enables(scope)
        from swapped in hidden.SwapIO(_ => !shows)
        from set in IO.lift(() => Content.Enabled = enables)
        select unit;

    private IO<IDisposable> Listening(CallbackSite site) =>
        from signals in toSeq(rules.Each.Bind(static rule => rule.Sources).Except(controls.Choose<ControlRow, RowSource>(static control => control.Bound)))
            .TraverseM(source => source.Signals(scope))
            .As()
        from held in DisposalOps.AcquireAll(
            scope.Listen(keys => when(keys.Exists(reads.Contains), Evaluated).As())
                .Cons(signals.Flatten().Map(signal => signal.Choose(static _ => Some((unit, unit))).Through(Subscriptions.Idle<Unit, Unit>(_ => Evaluated), scope.Sink))),
            DisposalOps.Release)
        from reread in DisposalOps.OnFailure(Reread, DisposalOps.Release(held))
        select DisposalOps.Composite(held, site);

    // --- [HEADER]
    private static IO<(HeaderButtons Handler, Seq<IDisposable> Menus)> Header(IPlugInViews owner, View.Section row, Seq<ControlRow> controls, HistoryBinding history, RowScope scope) =>
        DisposalOps.AcquireAll(
                row.Presets.Map(listing => MenuButton(GlyphRole.Presets, "Presets", listing(scope), owner)).ToSeq()
                    .Add(MenuButton(GlyphRole.Options, "Values", Listing(owner, controls, history, scope), owner)),
                static held => DisposalOps.Release(held.Map(static menu => menu.Release)))
            .Map(menus => (
                new HeaderButtons(
                    menus.Map(static menu => menu.Button).Add(new HeaderButton(
                        GlyphRole.Reset, "Reset to defaults", ViewOps.Defaults(row).Bind(defaults => scope.Commit(history.Group, defaults)).Map(static _ => unit))),
                    owner, row.Identity),
                menus.Map(static menu => menu.Release)));

    private static IO<(HeaderButton Button, IDisposable Release)> MenuButton(GlyphRole glyph, string toolTip, IO<Seq<MenuPick>> listing, IPlugInSink sink) =>
        ChoiceRows.Menu(Seq<Seq<MenuEntry>>(), Seq<IO<Unit>>(), Some(listing), sink)
            .Map(built => (new HeaderButton(glyph, toolTip, IO.lift(() => built.Menu.Show())), built.Release));

    // --- [VALUES]
    private static IO<Seq<MenuPick>> Listing(IPlugInViews owner, Seq<ControlRow> controls, HistoryBinding history, RowScope scope) =>
        from held in history.History
        from pastes in IO.lift(static () => Clipboard.Instance.ContainsText)
        from zone in IO.lift(() => owner.Clock.ToZonedClock().Zone)
        let stamp = LocalDateTimePattern.Create("g", RowText.Culture)
        select Seq(
                held.Done.Head.Map(entry => Pick(RowText.Localize("Undo {0}", arguments: [Described(entry, controls, owner)]).Local, Heard(history.Undo, scope))),
                held.Undone.Head.Map(entry => Pick(RowText.Localize("Redo {0}", arguments: [Described(entry, controls, owner)]).Local, Heard(history.Redo, scope))),
                Some(Pick(RowText.Localize("Copy values").Local, Transfer.Copy(history.Group, None, scope))),
                Some(Pick(RowText.Localize("Paste values").Local, Transfer.Paste(history.Group, None, scope))).Filter(_ => pastes),
                Some(Pick(RowText.Localize("Clear history").Local, history.Clear)).Filter(_ => !(held.Done.IsEmpty && held.Undone.IsEmpty)),
                Some<MenuPick>(new MenuPick.Group(RowText.Localize("Restore").Local, held.Done.Map(entry => Pick(
                    RowText.Localize("{0}, {1}", arguments: [Described(entry, controls, owner), stamp.Format(entry.At.InZone(zone).LocalDateTime)]).Local,
                    Heard(history.Restore(entry.Serial), scope))))).Filter(_ => !held.Done.IsEmpty))
            .Somes();

    private static MenuPick Pick<A>(string caption, IO<A> run) => new MenuPick.Item(caption, Marked: false, run.Map(static _ => unit));

    private static IO<ValueDiff> Heard(IO<ValueDiff> step, RowScope scope) =>
        from diff in step
        from changed in scope.Changed(toSeq(diff.Changes.Keys))
        select diff;

    private static string Described(HistoryEntry entry, Seq<ControlRow> controls, IPlugInSink sink) =>
        controls.Filter(control => control.Keys.Exists(entry.Diff.Changes.ContainsKey)) switch {
            [var only] => only.Wording.Shown(only.Caption, sink),
            _ => RowText.Localize("{0} values", arguments: [entry.Diff.Changes.Count]).Local,
        };
}

internal sealed class HeaderButtons(Seq<HeaderButton> buttons, IPlugInSink sink, Type owner) : IHeaderButtonHandler {
    public bool ButtonDetails(int index, ref Bitmap iconOut, ref string sToolTipOut) {
        (bool Shown, Bitmap Icon, string ToolTip) held = (false, iconOut, sToolTipOut);
        (bool shown, iconOut, sToolTipOut) = Callbacks.Answer(
            buttons.At(index)
                .Traverse(button => Icons.Raster(sink, button.Glyph, IconSlot.SectionHeader).Map(icon => (Shown: true, Icon: icon, ToolTip: RowText.Localize(button.ToolTip).Local)))
                .As()
                .Map(found => found.IfNone(held)),
            () => held,
            new CallbackSite(sink, owner, nameof(ButtonDetails)));
        return shown;
    }

    public bool OnButtonClicked(int index) =>
        Callbacks.Succeeded(buttons.At(index).Map(static button => button.Click), static () => false, new CallbackSite(sink, owner, nameof(OnButtonClicked)));

    public Rectangle ButtonRect(int index, Rectangle rectHeader) =>
        Callbacks.Answer(IconSlot.SectionHeader.Size().Map(static size => new Rectangle(0, 0, size, size)), static () => Rectangle.Empty, new CallbackSite(sink, owner, nameof(ButtonRect)));

    public void DeleteThis() { }
}

public sealed class SectionStack : EtoCollapsibleSectionHolder2 {
    // --- [STATE]
    private readonly Seq<DefinedSection> sections;

    private SectionStack(Seq<DefinedSection> sections, Option<DefinedSection> fill) {
        this.sections = sections;
        _ = sections.Iter(Add);
        _ = fill.Iter(SetFullHeightSection);
    }

    // --- [HOLD]
    public static IO<ViewBody> Hold(IPlugInViews owner, HolderRow holder, RowScope scope) =>
        from built in holder.Switch(
            (Owner: owner, Scope: scope),
            stacked: static (state, stacked) => Built(state.Owner, stacked.Members, state.Scope).Map(static sections => (Sections: sections, Fill: Option<DefinedSection>.None)),
            filled: static (state, filled) =>
                from above in Built(state.Owner, filled.Above, state.Scope)
                from fill in DisposalOps.OnFailure(Section(state.Owner, filled.Fill, state.Scope), DisposalOps.Release(above))
                from below in DisposalOps.OnFailure(Built(state.Owner, filled.Below, state.Scope), DisposalOps.Release(above.Add(fill)))
                select (Sections: above.Add(fill) + below, Fill: Some(fill)))
        from stack in DisposalOps.OnFailure(IO.lift(() => new SectionStack(built.Sections, built.Fill)), DisposalOps.Release(built.Sections))
        select new ViewBody(stack, IO.lift(stack.Dispose));

    public static IO<DefinedSection> Section(IPlugInViews owner, View.Section row, RowScope scope) =>
        from section in ViewOps.Construct<DefinedSection>(row)
        from body in DisposalOps.OnFailure(SectionBody.Realize(owner, row, scope), IO.lift(section.Dispose))
        from attached in DisposalOps.OnFailure(section.Attach(body), DisposalOps.Release(Seq<IDisposable>(section, body)))
        select section;

    private static IO<Seq<DefinedSection>> Built(IPlugInViews owner, Seq<View.Section> rows, RowScope scope) =>
        DisposalOps.AcquireAll(rows.Map(row => Section(owner, row, scope)), DisposalOps.Release);

    // --- [EDGES]
    public static Unit Shown(Control content, bool visible) =>
        content.Cons(Optional(content as Container).ToSeq().Bind(static container => toSeq(container.Children)))
            .Choose<Control, SectionStack>(static control => Optional(control as SectionStack))
            .Iter(stack => stack.sections.Iter(section => section.HolderVisible(visible)));

    protected override void OnPreLoad(EventArgs e) {
        base.OnPreLoad(e);
        if (UseScrollbars && toSeq(Parents).Exists(static parent => parent is Scrollable))
            UseScrollbars = false;
    }
}

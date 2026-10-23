using System.Drawing;
using Eto.Forms;
using NodaTime.Text;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Inputs;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.UI.Controls;
using NativeDataSourceEventArgs = Rhino.UI.Controls.DataSource.EventArgs;

namespace Rasm.Rhino.UI;

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

[SmartEnum<string>(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class SectionAction {
    public static readonly SectionAction Presets = new("Presets", GlyphRole.Presets);
    public static readonly SectionAction Values = new("Values", GlyphRole.Options);
    public static readonly SectionAction Reset = new("Reset", GlyphRole.Reset);

    public GlyphRole Glyph { get; }
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ViewBody : IDisposable {
    // --- [STATE]
    private readonly View row;
    private readonly RowScope enclosing;
    private readonly Retained<IDisposable> owned;
    private readonly Seq<IO<Unit>> clicks;
    private readonly Atom<bool> hidden = Atom(value: false);
    private readonly ShownSubscription listeners;
    private RowRules Rules => new(row.Enabled, row.Visible);
    private bool showing;

    private ViewBody(
        View row, Control content, RowScope scope, RowScope enclosing, BindingGroup group, Seq<IO<Unit>> clicks,
        Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>> listening, Retained<IDisposable> owned) {
        (this.row, Content, Scope, this.enclosing, Group, this.clicks, this.owned) = (row, content, scope, enclosing, group, clicks, owned);
        listeners = new ShownSubscription(retain => Listening(listening, retain));
    }

    public Control Content { get; }
    public RowScope Scope { get; }
    public BindingGroup Group { get; }
    public bool Hidden => hidden.Value;
    public int Height => hidden.Value ? 0 : (int)Math.Ceiling(Content.GetPreferredSize().Height);

    // --- [REALIZATION]
    public static IO<ViewBody> Realize(IPlugInViews owner, View.Section row, RowScope enclosing, Action<IDisposable> retain) =>
        DisposalOps.Acquire(Built(owner, row, enclosing, None), retain);

    public static IO<ViewBody> Open(IPlugInViews owner, View row, Option<RhinoDoc> document, CommitMode mode, Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>> listening) =>
        from enclosing in RowScope.Open(owner, document, mode)
        from body in Built(owner, row, enclosing, listening)
        from opened in DisposalOps.OnFailure(body.Scope.Opened(body.Group), IO.lift(body.Dispose))
        select body;

    private static IO<ViewBody> Built(IPlugInViews owner, View row, RowScope enclosing, Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>> listening) =>
        from owned in IO.lift(static () => new Retained<IDisposable>(DisposalOps.Release))
        from body in DisposalOps.OnFailure(
            from bindings in ViewOps.Bindings(owner, row, enclosing)
            from history in row.Store.Traverse(store => IO.lift(ViewOps.History(owner, row, store, bindings, enclosing.Document))).As()
            let scope = enclosing.Nested(ViewOps.Settings(owner, row), history, ViewOps.Locks(row, enclosing.Document))
            from hidden in scope.Shown(visible: false)
            from content in ViewOps.Realize(owner, row.Children, scope, owned.Retain)
            from clicks in (from section in Optional(row as View.Section) from held in history select Actions(section).TraverseM(entry => entry.Realize(owner, held, scope, owned.Retain)).As())
                .IfNone(IO.pure(Seq<IO<Unit>>()))
            from built in IO.lift(() => new ViewBody(row, content, scope, enclosing, bindings, clicks, listening, owned))
            from visibility in enclosing.OnVisibility(_ => built.Evaluated, owned.Retain)
            select built,
            IO.lift(owned.Dispose))
        select body;

    // --- [EDGES]
    public IO<Unit> Shown(bool visible) => IO.lift(() => showing = visible)
        .Bind(_ => Callbacks.Each(Seq(listeners.Shown(visible), Evaluated))).Map(unit);

    public IO<Unit> Reread => Scope.Changed(ViewCatalog.Tree(row).Bind(Keys));

    public void Dispose() {
        using (owned)
        using (listeners)
            _ = Scope.Shown(visible: false).Uninterruptible().Run();
    }

    // --- [RULES]
    private IO<Unit> Evaluated =>
        from shows in Rules.Shows(Scope)
        from enables in Rules.Enables(Scope)
        from visible in enclosing.Visibility
        from swapped in hidden.SwapIO(_ => !shows)
        from fitted in SectionHolder.Fitted(Content)
        from set in IO.lift(() => { (Content.Visible, Content.Enabled) = (shows, enables); })
        from availability in Scope.Shown(showing && visible && shows && enables)
        from sections in SectionHolder.Shown(Content, showing && visible && shows && enables)
        select unit;

    private IO<Unit> Listening(Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>> listening, Action<IDisposable> retain) =>
        from signals in toSeq(Rules.Each.Bind(static rule => rule.Sources).Except(row.Children.Bind(static child => child.Controls).Choose(static control => control.Bound)))
            .TraverseM(source => source.Signals(Scope)).As()
        let reads = toHashSet(Rules.Each.Bind(static rule => rule.Reads))
        from heard in Scope.Listen(keys => when(keys.Exists(reads.Contains), Evaluated).As(), retain)
        from attached in signals.Flatten().TraverseM(signal => signal.Choose(static _ => Some((unit, unit)))
            .Through(Subscriptions.Idle<Unit, Unit>(_ => Evaluated), Scope.Sink, retain)).As()
        from extra in listening.Traverse(listen => listen(this, retain)).As()
        from reread in Reread
        select unit;

    private static Seq<EntryKey> Keys(View view) =>
        view.Children.Bind(static child => child.Controls).Bind(static control => control.Keys + control.Rules.Each.Bind(static rule => rule.Reads))
        + new RowRules(view.Enabled, view.Visible).Each.Bind(static rule => rule.Reads);

    // --- [HEADER]
    public static Option<IHeaderButtonHandler> Header(View.Section row, IO<Option<ViewBody>> current, IPlugInSink sink) =>
        Actions(row) switch {
            { IsEmpty: true } => None,
            var actions => Some<IHeaderButtonHandler>(new HeaderButtons(actions.Map(static entry => entry.Action), index =>
                current.Bind(held => IO.lift(held.Bind(body => body.clicks.At(index)).ToFin(new Refused(nameof(clicks), Refused.Absent)))).Bind(static click => click), sink, row.Identity)),
        };

    private static Seq<(SectionAction Action, Func<IPlugInViews, HistoryBinding, RowScope, Action<IDisposable>, IO<IO<Unit>>> Realize)> Actions(View.Section row) =>
        row.Store.IsSome
            ? [
                .. row.Presets.Map(static listing => (SectionAction.Presets, fun((IPlugInViews owner, HistoryBinding _, RowScope scope, Action<IDisposable> retain) => Popped(listing(scope), owner, retain)))).ToSeq(),
                (SectionAction.Values, (owner, history, scope, retain) => Popped(Listing(owner, row.Children.Bind(static child => child.Controls), history, scope), owner, retain)),
                (SectionAction.Reset, static (_, history, scope, _) => IO.pure(scope.Commit(history.Group, history.Group.Defaults).Map(unit))),
            ]
            : [];

    private static IO<IO<Unit>> Popped(IO<Seq<MenuPick>> listing, IPlugInSink sink, Action<IDisposable> retain) =>
        ChoiceRows.Menu(Seq<Seq<MenuEntry>>(), Seq<IO<Unit>>(), Some(listing), sink, retain).Map(static menu => IO.lift(() => menu.Show()));

    // --- [VALUES]
    private static IO<Seq<MenuPick>> Listing(IPlugInViews owner, Seq<ControlRow> controls, HistoryBinding history, RowScope scope) =>
        from held in history.History
        from pastes in IO.lift(static () => Clipboard.Instance.ContainsText)
        from zone in IO.lift(() => owner.Clock.ToZonedClock().Zone)
        let stamp = LocalDateTimePattern.Create("g", RowText.Culture)
        select Seq(
                held.Done.Head.Map<MenuPick>(entry => Pick(RowText.Localize("Undo {0}", arguments: [Described(entry, controls, owner)]).Local, Heard(history.Step(StepDirection.Undo), scope))),
                held.Undone.Head.Map<MenuPick>(entry => Pick(RowText.Localize("Redo {0}", arguments: [Described(entry, controls, owner)]).Local, Heard(history.Step(StepDirection.Redo), scope))),
                Some<MenuPick>(Pick(RowText.Localize("Copy values").Local, Transfer.Copy(history.Group, None, scope))),
                Some<MenuPick>(Pick(RowText.Localize("Paste values").Local, Transfer.Paste(history.Group, None, scope))).Filter(_ => pastes),
                Some<MenuPick>(Pick(RowText.Localize("Clear history").Local, history.Clear)).Filter(_ => !(held.Done.IsEmpty && held.Undone.IsEmpty)),
                Some<MenuPick>(new MenuPick.Group(RowText.Localize("Restore").Local, held.Done.Map<MenuPick>(entry => Pick(
                    RowText.Localize("{0}, {1}", arguments: [Described(entry, controls, owner), stamp.Format(entry.At.InZone(zone).LocalDateTime)]).Local,
                    Heard(history.Restore(entry.Serial), scope))))).Filter(_ => !held.Done.IsEmpty))
            .Somes();

    private static MenuPick.Item Pick<T>(string caption, IO<T> run) => new(caption, Marked: false, run.Map(unit));

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

internal sealed class HeaderButtons(Seq<SectionAction> actions, Func<int, IO<Unit>> click, IPlugInSink sink, Type owner) : IHeaderButtonHandler {
    public bool ButtonDetails(int index, ref Bitmap iconOut, ref string sToolTipOut) {
        (bool Shown, Bitmap Icon, string ToolTip) held = (false, iconOut, sToolTipOut);
        (bool shown, iconOut, sToolTipOut) = Callbacks.Answer(
            actions.At(index)
                .Traverse(static action => Icons.Raster(action.Glyph, IconSlot.SectionHeader).Map(icon => (Shown: true, Icon: icon, ToolTip: RowText.Localize(action.Key).Local)))
                .As()
                .Map(found => found.IfNone(held)),
            () => held,
            new CallbackSite(sink, owner, nameof(ButtonDetails)));
        return shown;
    }

    public bool OnButtonClicked(int index) =>
        Callbacks.Succeeded(actions.At(index).Map(_ => click(index)), static () => false, new CallbackSite(sink, owner, nameof(OnButtonClicked)));

    public Rectangle ButtonRect(int index, Rectangle rectHeader) =>
        Callbacks.Answer(IconSlot.SectionHeader.Size().Map(static size => new Rectangle(0, 0, size, size)), static () => Rectangle.Empty, new CallbackSite(sink, owner, nameof(ButtonRect)));

    public void DeleteThis() { }
}

public sealed class SectionLifetime : IDisposable {
    // --- [STATE]
    private readonly EtoCollapsibleSection section;
    private readonly IPlugInSink sink;
    private readonly Option<Guid> data;
    private readonly Func<bool> excluded;
    private readonly ShownSubscription realized;
    private Option<IDisposable> edges;
    private Option<ViewBody> body;
    private bool visible;

    public SectionLifetime(EtoCollapsibleSection section, IPlugInSink sink, Option<Guid> data, Func<bool> excluded, bool visible, Func<Action<IDisposable>, IO<ViewBody>> realize) {
        (this.section, this.sink, this.data, this.excluded, this.visible) = (section, sink, data, excluded, visible);
        realized = new ShownSubscription(retain =>
            from held in realize(retain)
            from placed in DisposalOps.Change(() => (body, section.Content), next => (body, section.Content) = next, (Some(held), held.Content), retain)
            from shown in IO.lift(() => this.visible).Bind(held.Shown)
            select unit);
        _ = ViewOps.WhileLoaded(section, Listening, new CallbackSite(sink, section.GetType(), nameof(Control.LoadComplete)), lease => edges = Some(lease)).Run();
    }

    public int Height => body.Match(Some: static held => held.Height, None: static () => 0);

    public bool Hidden => body.Exists(static held => held.Hidden);

    // --- [EDGES]
    public Option<IHeaderButtonHandler> Header(View.Section row) =>
        ViewBody.Header(row, realized.Shown(shown: true).Bind(_ => IO.lift(() => body)), sink);

    public IO<Unit> Reread => IO.lift(() => body).Bind(static held => held.Traverse(static current => current.Reread).As()).Map(unit);

    public IO<Unit> Visible(bool shown) =>
        IO.lift(() => {
            visible = shown;
            return body;
        }).Bind(held => held.Traverse(current => current.Shown(shown)).As()).Map(unit);

    private IO<Unit> Listening(Action<IDisposable> retain) =>
        Optional(section.Parent as Expander) switch {
            var expander => IO.lift(() => expander.ForAll(static held => held.Expanded) && !excluded()).Bind(realized.Shown) switch {
                var evaluated =>
                    from expansions in expander.Traverse(held => Subscriptions.Attach(
                        handler => held.ExpandedChanged += handler, handler => held.ExpandedChanged -= handler,
                        Callbacks.Handler<EventArgs>(_ => evaluated, new CallbackSite(sink, typeof(Expander), nameof(Expander.ExpandedChanged))), retain)).As()
                    from changes in data.Traverse(type => Subscriptions.Attach(
                        handler => section.DataChanged += handler, handler => section.DataChanged -= handler,
                        Callbacks.Handler<NativeDataSourceEventArgs>(args => when(args.DataType == type, evaluated).As(),
                            new CallbackSite(sink, typeof(EtoCollapsibleSection), nameof(EtoCollapsibleSection.DataChanged))), retain)).As()
                    from shows in Subscriptions.Attach(
                        handler => section.Shown += handler, handler => section.Shown -= handler,
                        Callbacks.Handler<EventArgs>(_ => evaluated, new CallbackSite(sink, typeof(Control), nameof(Control.Shown))), retain)
                    from current in evaluated
                    select unit,
            },
        };

    // --- [RELEASE]
    public void Dispose() {
        try {
            _ = edges.Iter(static lease => lease.Dispose());
        } finally {
            realized.Dispose();
        }
    }
}

public sealed class SectionHolder : EtoCollapsibleSectionHolder2 {
    // --- [STATE]
    private readonly Option<int> above;

    private SectionHolder(Option<int> above) => this.above = above;

    // --- [HOLD]
    public static IO<SectionHolder> Hold(IPlugInViews owner, HolderRow holder, RowScope scope, Action<IDisposable> retain) =>
        from stack in DisposalOps.Acquire(IO.lift(() => new SectionHolder(holder.Switch(stacked: static _ => Option<int>.None, filled: static filled => Some(filled.Above.Count)))), retain)
        from sections in holder.Sections.TraverseM(row => Section(owner, row, scope, stack.Add)).As()
        from fitted in IO.lift(stack.Fit)
        select stack;

    public static IO<DefinedSection> Section(IPlugInViews owner, View.Section row, RowScope scope, Action<DefinedSection> retain) =>
        DisposalOps.Acquire(owned =>
            from section in DisposalOps.Acquire(IO.lift(() => (DefinedSection)Activator.CreateInstance(row.Identity)!), owned)
            from attached in section.Attach(acquired => ViewBody.Realize(owner, row, scope, acquired))
            select section, (section, _) => retain(section));

    // --- [EDGES]
    public static new IO<Unit> Shown(Control content, bool visible) =>
        content switch {
            SectionHolder stack => Callbacks.Each(toSeq(stack.Sections).Choose(static section => Optional(section as EtoCollapsibleSection2))
                .Map(section => IO.lift(() => section.HolderVisible(visible)))).Map(unit),
            Container container => Callbacks.Each(toSeq(container.Controls).Map(child => Shown(child, visible))).Map(unit),
            _ => IO.pure(unit),
        };

    internal static IO<Unit> Fitted(Control content) =>
        IO.lift(() => Optional(content.Parent as DefinedSection).Bind(static section => section.Holder).Iter(static holder => holder.Fit()));

    // --- [LAYOUT]
    private Unit Fit() =>
        above.Iter(count => SetFullHeightSection(SectionAt(toSeq(Sections).Take(count).Filter(static section => !section.Hidden).Count)));

    protected override void OnPreLoad(EventArgs e) {
        base.OnPreLoad(e);
        if (UseScrollbars && toSeq(Parents).Exists(static parent => parent is Scrollable))
            Callbacks.Run(IO.lift(() => { UseScrollbars = false; }), CallbackSite.Of(this));
    }
}

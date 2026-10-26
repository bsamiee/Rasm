using System.Drawing;
using System.Globalization;
using Eto.Forms;
using NodaTime.Text;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rasm.UI;
using Rhino;
using Rhino.Resources;
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
            from clicks in Optional(row as View.Section).ToSeq().Bind(static section => section.Actions).TraverseM(action => CommandRows.Press(action, scope, owned.Retain)).As()
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
        from shows in RowRules.Holds(row.Visible, Scope)
        from enables in RowRules.Holds(row.Enabled, Scope)
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
        row.Actions switch {
            { IsEmpty: true } => None,
            var actions => Some<IHeaderButtonHandler>(new HeaderButtons(actions.Map(static action => action.Face), index =>
                current.Bind(held => IO.lift(held.Bind(body => body.clicks.At(index)).ToFin(new Refused(nameof(clicks), Refused.Absent)))).Bind(static click => click), sink, row.Identity)),
        };
}

internal sealed class HeaderButtons(Seq<CommandFace> faces, Func<int, IO<Unit>> click, IPlugInSink sink, Type owner) : IHeaderButtonHandler {
    public bool ButtonDetails(int index, ref Bitmap iconOut, ref string sToolTipOut) {
        (bool Shown, Bitmap Icon, string ToolTip) held = (false, iconOut, sToolTipOut);
        (bool shown, iconOut, sToolTipOut) = Callbacks.Answer(
            faces.At(index).Bind(static face => face.Glyph.Map(glyph => (Face: face, Glyph: glyph)))
                .Traverse(shown => Icons.Raster(shown.Glyph, IconSlot.SectionHeader).Map(icon => (Shown: true, Icon: icon, ToolTip: RowText.Localize(shown.Face.Caption, sink).Local)))
                .As()
                .Map(found => found.IfNone(held)),
            () => held,
            new CallbackSite(sink, owner, nameof(ButtonDetails)));
        return shown;
    }

    public bool OnButtonClicked(int index) =>
        Callbacks.Succeeded(faces.At(index).Map(_ => click(index)), static () => false, new CallbackSite(sink, owner, nameof(OnButtonClicked)));

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
    private readonly IPlugInViews owner;
    private readonly Option<int> above;

    private SectionHolder(IPlugInViews owner, Option<int> above) => (this.owner, this.above) = (owner, above);

    // --- [HOLD]
    public static IO<SectionHolder> Hold(IPlugInViews owner, HolderRow holder, RowScope scope, Action<IDisposable> retain) =>
        from stack in DisposalOps.Acquire(IO.lift(() => new SectionHolder(owner, holder.Switch(stacked: static _ => Option<int>.None, filled: static filled => Some(filled.Above.Count)))), retain)
        from sections in holder.Sections.TraverseM(row => Section(owner, row, scope, stack.Add)).As()
        from fitted in IO.lift(stack.Fit)
        select stack;

    public static IO<DefinedSection> Section(IPlugInViews owner, View.Section row, RowScope scope, Action<DefinedSection> retain) =>
        Section(owner, row, () => (DefinedSection)Activator.CreateInstance(row.Identity, owner)!, scope, retain);

    public static IO<SectionHolder> Disclose(IPlugInViews owner, ContainerRow.Disclosure disclosure, RowScope scope, Action<IDisposable> retain) =>
        new View.Section { Identity = typeof(DisclosureSection), Caption = disclosure.Caption, Children = toSeq(disclosure.Body) } switch {
            var row =>
                from stack in DisposalOps.Acquire(IO.lift(() => new SectionHolder(owner, None)), retain)
                from section in Section(owner, row, () => new DisclosureSection(row, owner, UUIDNext.Uuid.NewNameBased(owner.PlugIn.Id, disclosure.Caption), disclosure.Expanded), scope, stack.Add)
                select stack,
        };

    private static IO<DefinedSection> Section(IPlugInViews owner, View.Section row, Func<DefinedSection> create, RowScope scope, Action<DefinedSection> retain) =>
        DisposalOps.Acquire(owned =>
            from section in DisposalOps.Acquire(IO.lift(create), owned)
            from attached in section.Attach(acquired => ViewBody.Realize(owner, row, scope, acquired))
            select section, (section, _) => retain(section));

    // --- [EDGES]
    public static new IO<Unit> Shown(Control content, bool visible) =>
        content switch {
            SectionHolder stack => Callbacks.Each(toSeq(stack.Sections).Choose(static section => Optional(section as EtoCollapsibleSection2))
                .Map(section => IO.lift(() => section.HolderVisible(visible)))).Map(unit),
            Container container and not TabControl => Callbacks.Each(toSeq(container.Controls).Map(child => Shown(child, visible))).Map(unit),
            _ => IO.pure(unit),
        };

    internal static IO<Unit> Fitted(Control content) =>
        IO.lift(() => Optional(content.Parent as DefinedSection).Bind(static section => section.Holder).Iter(static holder => holder.Fit()));

    // --- [LAYOUT]
    private Unit Fit() =>
        above.Iter(count => SetFullHeightSection(SectionAt(Stretched(toSeq(Sections).Strict(), count))));

    private static int Stretched(Seq<ICollapsibleSection> sections, int above) =>
        sections.At(above).Exists(static fill => fill.Hidden)
            ? sections.Filter(static section => !section.Hidden).Count
            : sections.Take(above).Filter(static section => !section.Hidden).Count;

    protected override void OnPreLoad(EventArgs e) {
        base.OnPreLoad(e);
        if (UseScrollbars && toSeq(Parents).Exists(static parent => parent is Scrollable))
            Callbacks.Run(IO.lift(() => { UseScrollbars = false; }), CallbackSite.Of(owner, this));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionCommands {
    // --- [ACTIONS]
    public static Command<RowRule, Invoke>.Run Presets(Func<RowScope, IO<Seq<Seq<Command<RowRule, Invoke>>>>> listing) =>
        new(new CommandFace("section-presets", "Presets", None, Some<GlyphSource>(Icons.Embedded(typeof(SectionHolder).Assembly, "presets.svg"))), None, new Invoke.Listed(listing));

    public static Command<RowRule, Invoke>.Run Values(Seq<ControlRow> controls) =>
        new(new CommandFace("section-values", "Values", None, Some<GlyphSource>(Icons.Embedded(typeof(SectionHolder).Assembly, "options.svg"))), None,
            new Invoke.Listed(scope => Historied(scope).Bind(history => Listing(controls, history, scope))));

    public static Command<RowRule, Invoke>.Run Reset { get; } =
        new(new CommandFace("section-reset", "Reset", None, Some<GlyphSource>(Icons.Embedded(Assets.Rhino.Assembly, ResourceIds.Svg_namedItemRestoreHotSvg))), None,
            new Invoke.Effect(static scope => Historied(scope).Bind(history => scope.Commit(history.Group, history.Group.Defaults)).Map(unit)));

    private static IO<HistoryBinding> Historied(RowScope scope) =>
        IO.lift(scope.History.ToFin(new Refused(nameof(RowScope.History), Refused.Absent)));

    // --- [VALUES]
    private static IO<Seq<Seq<Command<RowRule, Invoke>>>> Listing(Seq<ControlRow> controls, HistoryBinding history, RowScope scope) =>
        from held in history.History
        from pastes in IO.lift(static () => Clipboard.Instance.ContainsText)
        from zone in IO.lift(() => history.Clock.ToZonedClock().Zone)
        let stamp = LocalDateTimePattern.Create("g", RowText.Culture)
        select Seq(
            held.Done.Head.Map(entry => Ran("section-undo", RowText.Localize("Undo {0}", arguments: [Described(entry, controls, scope.Sink)]).Local, Heard(history.Undo, scope))).ToSeq()
                + held.Undone.Head.Map(entry => Ran("section-redo", RowText.Localize("Redo {0}", arguments: [Described(entry, controls, scope.Sink)]).Local, Heard(history.Redo, scope))).ToSeq(),
            Seq(Ran("section-copy", "Copy values", DataTransfer.Copy(history.Group, None, scope)))
                + Seq(Ran("section-paste", "Paste values", DataTransfer.Paste(history.Group, None, scope))).Filter(_ => pastes),
            Seq(Ran("section-clear", "Clear history", history.Clear)).Filter(_ => !(held.Done.IsEmpty && held.Undone.IsEmpty))
                + Seq<Command<RowRule, Invoke>>(new Command<RowRule, Invoke>.Menu(new CommandFace("section-restore", "Restore", None, None), None, [held.Done.Map(entry => Ran(
                    string.Create(CultureInfo.InvariantCulture, $"section-restore-{entry.Serial}"),
                    RowText.Localize("{0}, {1}", arguments: [Described(entry, controls, scope.Sink), stamp.Format(entry.At.InZone(zone).LocalDateTime)]).Local,
                    Heard(history.Restore(entry.Serial), scope)))])).Filter(_ => !held.Done.IsEmpty));

    private static Command<RowRule, Invoke> Ran<T>(string name, string caption, IO<T> run) =>
        new Command<RowRule, Invoke>.Run(new CommandFace(name, caption, None, None), None, new Invoke.Effect(_ => run.Map(unit)));

    private static IO<ValueDiff> Heard(IO<ValueDiff> step, RowScope scope) =>
        from diff in step
        from changed in scope.Changed(toSeq(diff.Changes.Keys))
        select diff;

    private static string Described(HistoryEntry entry, Seq<ControlRow> controls, IPlugInSink sink) =>
        controls.Filter(control => control.Keys.Exists(entry.Diff.Changes.ContainsKey)) switch {
            [var only] => RowText.Localize(only.Text.Caption, sink).Local,
            _ => RowText.Localize("{0} values", arguments: [entry.Diff.Changes.Count]).Local,
        };
}

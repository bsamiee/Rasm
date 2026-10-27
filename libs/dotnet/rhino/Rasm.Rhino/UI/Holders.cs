using System.Drawing;
using System.Globalization;
using Eto.Forms;
using NodaTime.Text;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Resources;
using Rhino.UI.Controls;
using DataSourceEventArgs = Rhino.UI.Controls.DataSource.EventArgs;

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
public sealed class ViewBody {
    // --- [STATE]
    private readonly View row;
    private readonly RowScope enclosing;
    private readonly EnvIO scope;
    private readonly Seq<IO<Unit>> clicks;
    private readonly Option<Func<ViewBody, IO<Unit>>> listening;
    private readonly Ref<Option<Lease<Unit>>> listeners = Ref(Option<Lease<Unit>>.None);
    private readonly Atom<bool> hidden = Atom(value: false);
    private bool showing;

    private ViewBody(
        View row, Control content, RowScope nested, RowScope enclosing, BindingGroup group, Seq<IO<Unit>> clicks, Option<Func<ViewBody, IO<Unit>>> listening, EnvIO scope) =>
        (this.row, Content, Scope, this.enclosing, Group, this.clicks, this.listening, this.scope) = (row, content, nested, enclosing, group, clicks, listening, scope);

    public Control Content { get; }
    public RowScope Scope { get; }
    public BindingGroup Group { get; }
    public bool Hidden => hidden.Value;
    public int Height => hidden.Value ? 0 : (int)Math.Ceiling(Content.GetPreferredSize().Height);

    // --- [REALIZATION]
    public static IO<ViewBody> Realize(IPlugInViews owner, View.Section row, RowScope enclosing) => Built(owner, row, enclosing, None);

    public static IO<ViewBody> Open(IPlugInViews owner, View row, Option<RhinoDoc> document, CommitMode mode, Option<Func<ViewBody, IO<Unit>>> listening) =>
        from enclosing in RowScope.Open(owner, document, mode)
        from body in Built(owner, row, enclosing, listening)
        from opened in body.Scope.Opened(body.Group)
        select body;

    private static IO<ViewBody> Built(IPlugInViews owner, View row, RowScope enclosing, Option<Func<ViewBody, IO<Unit>>> listening) =>
        from scope in IO.env
        from bindings in ViewOps.Bindings(owner, row, enclosing)
        from history in IO.lift(() => ViewOps.History(owner, row, bindings, enclosing.Document))
        let nested = enclosing.Nested(ViewOps.Settings(owner, row), history, ViewOps.Locks(row, enclosing.Document))
        from hidden in nested.Shown(visible: false)
        from content in ViewOps.Realize(owner, row.Children, nested)
        from clicks in Optional(row as View.Section).ToSeq().Bind(static section => section.Actions).TraverseM(action => CommandRows.Press(action, nested)).As()
        from built in use(IO.lift(() => new ViewBody(row, content, nested, enclosing, bindings, clicks, listening, scope)), static body => body.Scope.Shown(visible: false))
        from visibility in enclosing.OnVisibility(_ => built.Evaluated)
        select built;

    // --- [EDGES]
    public IO<Unit> Shown(bool visible) =>
        IO.lift(() => showing = visible)
            .Bind(_ => Callbacks.Each(Seq(ViewOps.Toggled(listeners, visible, Listening).Under(scope), Evaluated))).Map(unit);

    public IO<Unit> Showing => use(Shown(visible: true).Map(_ => this), static body => body.Shown(visible: false)).Map(unit);

    public IO<Unit> Reread => Scope.Changed(Group.Keys());

    // --- [RULES]
    private IO<Unit> Evaluated =>
        from shows in CommandRows.Holds(row.Visible, Scope)
        from enables in CommandRows.Holds(row.Enabled, Scope)
        from visible in enclosing.Visibility
        from swapped in hidden.SwapIO(_ => !shows)
        from fitted in SectionHolder.Fitted(Content)
        from set in IO.lift(() => { (Content.Visible, Content.Enabled) = (shows, enables); })
        from availability in Scope.Shown(showing && visible && shows && enables)
        from sections in SectionHolder.Shown(Content, showing && visible && shows && enables)
        select unit;

    private IO<Unit> Listening =>
        from heard in Scope.Listen(_ => Evaluated)
        from extra in listening.Traverse(listen => listen(this)).As()
        from reread in Reread
        select unit;

    // --- [HEADER]
    public static Option<IHeaderButtonHandler> Header(View.Section row, IO<Option<ViewBody>> current, IPlugInLifetime owner) =>
        row.Actions switch {
            { IsEmpty: true } => None,
            var actions => Some<IHeaderButtonHandler>(new HeaderButtons(actions.Map(static action => action.Face), index =>
                current.Bind(held => IO.lift(() => Conversions.Found(held.Bind(body => body.clicks.At(index))))).Bind(static click => click), owner)),
        };
}

internal sealed class HeaderButtons(Seq<CommandFace> faces, Func<int, IO<Unit>> click, IPlugInLifetime owner) : IHeaderButtonHandler {
    public bool ButtonDetails(int index, ref Bitmap iconOut, ref string sToolTipOut) {
        (bool Shown, Bitmap Icon, string ToolTip) held = (false, iconOut, sToolTipOut);
        (bool shown, iconOut, sToolTipOut) = owner.Answer(
            (from face in faces.At(index) from icon in face.Icon select (Face: face, Icon: icon))
                .Traverse(shown => Icons.Raster(shown.Icon, IconSlot.SectionHeader).Map(icon => (Shown: true, Icon: icon, ToolTip: RowText.Localize(shown.Face.Caption, owner).Local)))
                .As()
                .Map(found => found.IfNone(held)),
            held);
        return shown;
    }

    public bool OnButtonClicked(int index) =>
        owner.Answer(faces.At(index).Map(_ => click(index).Map(static _ => true)).IfNone(IO.pure(value: false)), failed: false);

    public Rectangle ButtonRect(int index, Rectangle rectHeader) =>
        owner.Answer(IconSlot.SectionHeader.Extent().Map(static size => new Rectangle(0, 0, size, size)), Rectangle.Empty);

    public void DeleteThis() { }
}

public sealed class SectionLifetime : IDisposable {
    // --- [STATE]
    private readonly EtoCollapsibleSection section;
    private readonly IPlugInLifetime owner;
    private readonly Option<Guid> data;
    private readonly Func<bool> excluded;
    private readonly IO<ViewBody> realize;
    private readonly EnvIO scope = EnvIO.New(token: CancellationToken.None);
    private readonly Ref<Option<Lease<Unit>>> realized = Ref(Option<Lease<Unit>>.None);
    private Option<ViewBody> body;
    private bool visible;

    public SectionLifetime(EtoCollapsibleSection section, IPlugInLifetime owner, Option<Guid> data, Func<bool> excluded, bool visible, IO<ViewBody> realize) {
        (this.section, this.owner, this.data, this.excluded, this.visible, this.realize) = (section, owner, data, excluded, visible, realize);
        _ = owner.Run(ViewOps.WhileLoaded(section, Listening, owner).Under(scope));
    }

    public int Height => body.Match(Some: static held => held.Height, None: static () => 0);

    public bool Hidden => body.Exists(static held => held.Hidden);

    // --- [EDGES]
    public Option<IHeaderButtonHandler> Header(View.Section row) =>
        ViewBody.Header(row, Realizing(shown: true).Bind(_ => IO.lift(() => body)), owner);

    public IO<Unit> Reread => IO.lift(() => body).Bind(static held => held.Traverse(static current => current.Reread).As()).Map(unit);

    public IO<Unit> Visible(bool shown) =>
        IO.lift(() => {
            visible = shown;
            return body;
        }).Bind(held => held.Traverse(current => current.Shown(shown)).As()).Map(unit);

    private IO<Unit> Realizing(bool shown) =>
        ViewOps.Toggled(realized, shown,
            from held in realize
            from placed in Products.Changed(() => (body, section.Content), next => (body, section.Content) = next, (Some(held), held.Content))
            from current in IO.lift(() => visible).Bind(held.Shown)
            select unit).Under(scope);

    private IO<Unit> Listening =>
        IO.lift(() => Optional(section.Parent as Expander)).Bind(expander =>
            IO.lift(() => expander.ForAll(static held => held.Expanded) && !excluded()).Bind(Realizing) switch {
                var evaluated =>
                    from expansions in expander.Traverse(held => Subscriptions.Host<EventArgs>(h => held.ExpandedChanged += h, h => held.ExpandedChanged -= h).Inline(_ => evaluated, owner)).As()
                    from changes in data.Traverse(type => Subscriptions.Host<DataSourceEventArgs>(h => section.DataChanged += h, h => section.DataChanged -= h)
                        .Inline(args => when(args.DataType == type, evaluated).As(), owner)).As()
                    from shows in Subscriptions.Host<EventArgs>(h => section.Shown += h, h => section.Shown -= h).Inline(_ => evaluated, owner)
                    from current in evaluated
                    select unit,
            });

    // --- [RELEASE]
    public void Dispose() {
        _ = owner.Run(IO.lift(scope.DisposeResources));
        scope.Dispose();
    }
}

public sealed class SectionHolder : EtoCollapsibleSectionHolder2 {
    // --- [STATE]
    private readonly IPlugInLifetime owner;
    private readonly Option<int> above;

    private SectionHolder(IPlugInLifetime owner, Option<int> above) => (this.owner, this.above) = (owner, above);

    // --- [HOLD]
    public static IO<SectionHolder> Hold(IPlugInViews owner, HolderRow holder, RowScope scope) =>
        from stack in use(() => new SectionHolder(owner, holder.Switch(stacked: static _ => Option<int>.None, filled: static filled => Some(filled.Above.Count))))
        from sections in holder.Sections.TraverseM(row => Section(owner, row, scope).Bind(section => IO.lift(() => stack.Add(section)))).As()
        from fitted in IO.lift(stack.Fit)
        select stack;

    public static IO<DefinedSection> Section(IPlugInViews owner, View.Section row, RowScope scope) =>
        row.Create.Match(
            Left: create =>
                from section in IO.lift(() => create(row, owner))
                from attached in section.Attach(ViewBody.Realize(owner, row, scope))
                select section,
            Right: _ => IO.fail<DefinedSection>(new RhinoFault(row.Identity.Name, RhinoReason.Unheld)));

    public static IO<SectionHolder> Disclose(IPlugInViews owner, ContainerRow.Disclosure disclosure, RowScope scope) =>
        new View.Section {
            Identity = typeof(DisclosureSection), Caption = disclosure.Caption, Children = toSeq(disclosure.Body),
            Create = Left<Func<View.Section, IPlugInViews, DefinedSection>, Func<View.Section, IPlugInViews, DefinedContentSection>>(
                (row, views) => new DisclosureSection(row, views, UUIDNext.Uuid.NewNameBased(views.PlugIn.Id, disclosure.Caption), disclosure.Expanded)),
        } switch {
            var row =>
                from stack in use(() => new SectionHolder(owner, None))
                from section in Section(owner, row, scope)
                from added in IO.lift(() => stack.Add(section))
                select stack,
        };

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
            _ = owner.Run(IO.lift(() => { UseScrollbars = false; }));
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionCommands {
    // --- [ACTIONS]
    public static Command<Invoke>.Run Presets(Func<RowScope, IO<Seq<Seq<Command<Invoke>>>>> listing) =>
        new(new CommandFace("section-presets", "Presets") { Icon = Some(new IconResource(typeof(SectionHolder).Assembly, "presets.svg")) }, None, new Invoke.Listed(listing));

    public static Command<Invoke>.Run Values { get; } =
        new(new CommandFace("section-values", "Values") { Icon = Some(new IconResource(typeof(SectionHolder).Assembly, "options.svg")) }, None,
            new Invoke.Listed(static scope => Historied(scope).Bind(history => Listing(history, scope))));

    public static Command<Invoke>.Run Reset { get; } =
        new(new CommandFace("section-reset", "Reset") { Icon = Some(new IconResource(Assets.Rhino.Assembly, ResourceIds.Svg_namedItemRestoreHotSvg)) }, None,
            new Invoke.Effect(static scope => Historied(scope).Bind(history => scope.Commit(history.Group, history.Group.Defaults)).Map(unit)));

    private static IO<HistoryBinding> Historied(RowScope scope) => IO.lift(() => Conversions.Found(scope.History));

    // --- [VALUES]
    private static IO<Seq<Seq<Command<Invoke>>>> Listing(HistoryBinding history, RowScope scope) =>
        from held in history.History
        from pastes in IO.lift(static () => Clipboard.Instance.ContainsText)
        from zone in IO.lift(() => history.Clock.ToZonedClock().Zone)
        let stamp = LocalDateTimePattern.Create("g", RowText.Culture)
        select Seq(
            held.Done.Head.Map(entry => Ran("section-undo", RowText.Localize("Undo {0}", arguments: [Described(entry, scope.Owner)]).Local, Heard(history.Undo, scope))).ToSeq()
                + held.Undone.Head.Map(entry => Ran("section-redo", RowText.Localize("Redo {0}", arguments: [Described(entry, scope.Owner)]).Local, Heard(history.Redo, scope))).ToSeq(),
            Seq(Ran("section-copy", "Copy values", DataTransfer.Copy(history.Group, None, scope)))
                + Seq(Ran("section-paste", "Paste values", DataTransfer.Paste(history.Group, None, scope))).Filter(_ => pastes),
            Seq(Ran("section-clear", "Clear history", history.Clear)).Filter(_ => !(held.Done.IsEmpty && held.Undone.IsEmpty))
                + Seq<Command<Invoke>>(new Command<Invoke>.Menu(new CommandFace("section-restore", "Restore"), None, [held.Done.Map(entry => Ran(
                    string.Create(CultureInfo.InvariantCulture, $"section-restore-{entry.Serial}"),
                    RowText.Localize("{0}, {1}", arguments: [Described(entry, scope.Owner), stamp.Format(entry.At.InZone(zone).LocalDateTime)]).Local,
                    Heard(history.Restore(entry.Serial), scope)))])).Filter(_ => !held.Done.IsEmpty));

    private static Command<Invoke> Ran<T>(string name, string caption, IO<T> run) =>
        new Command<Invoke>.Run(new CommandFace(name, caption), None, new Invoke.Effect(_ => run.Map(unit)));

    private static IO<ValueDiff> Heard(IO<ValueDiff> step, RowScope scope) =>
        from diff in step
        from changed in scope.Changed(toSeq(diff.Changes.Keys))
        select diff;

    private static string Described(HistoryEntry entry, IPlugInLifetime owner) =>
        toSeq(entry.Diff.Changes.Keys) switch {
            [var only] => RowText.Localize(EntryKey.Name(only.Path), owner).Local,
            var many => RowText.Localize("{0} values", owner, arguments: [many.Count]).Local,
        };
}

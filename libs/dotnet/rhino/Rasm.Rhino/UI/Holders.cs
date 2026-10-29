using System.Drawing;
using System.Globalization;
using Eto.Forms;
using NodaTime.Text;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.Render.Views;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino.Resources;
using Rhino.UI;
using Rhino.UI.Controls;

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
internal sealed class HostedSection : IDisposable {
    // --- [STATE]
    private readonly EtoCollapsibleSection section;
    private readonly Seq<Command<Invoke>> actions;
    private readonly Option<Guid> data;
    private readonly Func<bool> excluded;
    private readonly IO<ViewBody> realize;
    private readonly EnvIO scope = EnvIO.New(token: CancellationToken.None);
    private readonly Ref<Option<Owned<Unit>>> realized = Ref(Option<Owned<Unit>>.None);
    private Option<ViewBody> body;
    private bool visible;

    internal HostedSection(EtoCollapsibleSection section, Seq<Command<Invoke>> actions, Option<Guid> data, Func<bool> excluded, bool visible, IO<ViewBody> realize) {
        (this.section, this.actions, this.data, this.excluded, this.visible, this.realize) = (section, actions, data, excluded, visible, realize);
        _ = Session.Run(ViewOps.WhileLoaded(section, Listening).Scoped(scope));
    }

    internal int Height => body.Match(Some: static held => held.Height, None: static () => 0);

    internal bool Hidden => body.Exists(static held => held.Hidden);

    // --- [EDGES]
    internal Option<IHeaderButtonHandler> Header =>
        actions.IsEmpty
            ? None
            : Some<IHeaderButtonHandler>(new HeaderButtons(section, actions.Map(static action => action.Face), index =>
                Realizing(shown: true).Bind(_ => IO.lift(() => Conversions.Required(body.Bind(held => held.Clicks.At(index))))).Bind(static click => click)));

    internal Unit Reread => Session.Run(IO.lift(() => body).Bind(static held => held.Traverse(static current => current.Reread).As()));

    internal Unit Visible(bool shown) =>
        Session.Run(IO.lift(() => {
            visible = shown;
            return body;
        }).Bind(held => held.Traverse(current => current.Shown(shown)).As()));

    private IO<Unit> Realizing(bool shown) =>
        ViewOps.Toggled(realized, shown,
            from held in realize
            from placed in Products.Changed(() => (body, section.Content), next => (body, section.Content) = next, (Some(held), held.Content))
            from current in IO.lift(() => visible).Bind(held.Shown)
            select unit).Scoped(scope);

    private IO<Unit> Listening =>
        IO.lift(() => Optional(section.Parent as Expander)).Bind(expander =>
            IO.lift(() => expander.ForAll(static held => held.Expanded) && !excluded()).Bind(Realizing) switch {
                var evaluated =>
                    from expansions in expander.Traverse(held => Subscriptions.On<EventArgs>(h => held.ExpandedChanged += h, h => held.ExpandedChanged -= h).Attach(_ => evaluated)).As()
                    from changes in data.Traverse(type => SectionModels.DataChanged(section).Attach(changed => when(changed == type, evaluated).As())).As()
                    from shows in Subscriptions.On<EventArgs>(h => section.Shown += h, h => section.Shown -= h).Attach(_ => evaluated)
                    from current in evaluated
                    select unit,
            });

    // --- [RELEASE]
    public void Dispose() {
        _ = Session.Run(IO.lift(scope.DisposeResources));
        scope.Dispose();
    }
}

internal sealed class HeaderButtons(Control section, Seq<CommandFace> faces, Func<int, IO<Unit>> click) : IHeaderButtonHandler {
    public bool ButtonDetails(int index, ref Bitmap iconOut, ref string sToolTipOut) {
        (bool Shown, Bitmap Icon, string ToolTip) held = (false, iconOut, sToolTipOut);
        (bool shown, iconOut, sToolTipOut) = Session.Run(
            (from face in faces.At(index) from icon in face.Icon select (Face: face, Icon: icon))
                .Traverse(shown => Icons.Raster(shown.Icon, IconSlot.PanelTab, Some(section)).Map(icon => (Shown: true, Icon: icon, ToolTip: LocalText.Of(shown.Face.Caption).Local)))
                .As()
                .Map(found => found.IfNone(held)),
            held);
        return shown;
    }

    public bool OnButtonClicked(int index) =>
        Session.Run(faces.At(index).Map(_ => click(index).Map(static _ => true)).IfNone(IO.pure(value: false)), failed: false);

    public Rectangle ButtonRect(int index, Rectangle rectHeader) => Rectangle.Empty;

    public void DeleteThis() { }
}

internal sealed class DefinedSection : EtoCollapsibleSection3, ICollapsibleSection {
    // --- [STATE]
    private readonly View.Section row;
    private readonly Guid id;
    private readonly HostedSection hosted;

    internal DefinedSection(View.Section row, Guid id, bool expanded, Func<EtoCollapsibleSection, IO<ViewBody>> realize) {
        (this.row, this.id, InitiallyExpanded) = (row, id, expanded);
        hosted = new HostedSection(this, row.Actions, None, static () => false, visible: false, realize(this));
    }

    // --- [IDENTITY]
    Guid ICollapsibleSection.Id => id;

    public override string SettingsTag => id.ToString("D", CultureInfo.InvariantCulture);

    public override bool InitiallyExpanded { get; }

    public override LocalizeStringPair Caption => LocalText.Of(row.Caption);

    public override Guid PlugInId => Session.PlugIn.Id;

    public override int SectionHeight => hosted.Height;

    public override bool Hidden => hosted.Hidden;

    public override IHeaderButtonHandler NewHeaderButtonHandler() => hosted.Header.IfNone(base.NewHeaderButtonHandler);

    // --- [EDGES]
    internal Option<SectionHolder> Holder { get; private set; }

    public override void OnAttachedToHolder(ICollapsibleSectionHolder2 holder) {
        base.OnAttachedToHolder(holder);
        Holder = Optional(holder as SectionHolder);
    }

    public override void HolderVisible(bool visible) => _ = hosted.Visible(visible);

    public override void UpdateView(uint flags) => _ = hosted.Reread;

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) hosted.Dispose();
        base.Dispose(disposing);
    }
}

internal sealed class SectionHolder : EtoCollapsibleSectionHolder2 {
    // --- [STATE]
    private readonly Option<int> above;

    private SectionHolder(Option<int> above) => this.above = above;

    // --- [HOLD]
    internal static IO<SectionHolder> Hold(HolderRow holder, RowScope scope) =>
        from stack in use(() => new SectionHolder(holder.Switch(stacked: static _ => Option<int>.None, filled: static filled => Some(filled.Above.Count))), Detached)
        from sections in holder.Sections.TraverseM(row => IO.lift(() => stack.Add(Section(row, row.Identity.GUID, expanded: true, scope)))).As()
        from fitted in IO.lift(stack.Fit)
        select stack;

    internal static DefinedSection Section(View.Section row, Guid id, bool expanded, RowScope scope) =>
        new(row, id, expanded, _ => ViewBody.Realize(row, row.Actions, scope));

    internal static IO<SectionHolder> Disclose(ContainerRow.Disclosure disclosure, RowScope scope) =>
        from stack in use(static () => new SectionHolder(None), Detached)
        from added in IO.lift(() => stack.Add(Section(
            new View.Section { Identity = typeof(SectionHolder), Caption = disclosure.Caption, Children = toSeq(disclosure.Body) },
            UUIDNext.Uuid.NewNameBased(Session.PlugIn.Id, disclosure.Caption), disclosure.Expanded, scope)))
        select stack;

    private static void Detached(SectionHolder stack) {
        _ = toSeq(stack.Sections).Strict().Iter(stack.Remove);
        stack.Dispose();
    }

    // --- [EDGES]
    internal static IO<Unit> HolderVisible(Control content, bool visible) =>
        content switch {
            SectionHolder stack => Products.Each(toSeq(stack.Sections).Choose(static section => Optional(section as EtoCollapsibleSection2))
                .Map(section => IO.lift(() => section.HolderVisible(visible)))).Map(unit),
            Container container and not TabControl => Products.Each(toSeq(container.Controls).Map(child => HolderVisible(child, visible))).Map(unit),
            _ => IO.pure(unit),
        };

    internal static IO<Unit> Fitted(Control content) =>
        IO.lift(() => Optional(content.Parent as DefinedSection).Bind(static section => section.Holder).Iter(static holder => holder.Fit()));

    // --- [LAYOUT]
    private Unit Fit() =>
        above.Iter(count => SetFullHeightSection(SectionAt(VisibleFullHeightIndex(toSeq(Sections).Strict(), count))));

    private static int VisibleFullHeightIndex(Seq<ICollapsibleSection> sections, int above) =>
        sections.At(above).Exists(static fill => fill.Hidden)
            ? sections.Filter(static section => !section.Hidden).Count
            : sections.Take(above).Filter(static section => !section.Hidden).Count;

    protected override void OnPreLoad(EventArgs e) {
        base.OnPreLoad(e);
        if (UseScrollbars && toSeq(Parents).Exists(static parent => parent is Scrollable)) UseScrollbars = false;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SectionCommands {
    // --- [ACTIONS]
    public static Command<Invoke>.Run Values { get; } =
        new(new CommandFace("section-values", "Values") { Icon = Some(IconResource.Options) }, None,
            new Invoke.Popup(static scope => Historied(scope).Bind(history => Listing(history, scope))));

    public static Command<Invoke>.Run Reset { get; } =
        new(new CommandFace("section-reset", "Reset") { Icon = Some(new IconResource(Assets.Rhino.Assembly, ResourceIds.Svg_namedItemRestoreHotSvg)) }, None,
            new Invoke.Effect(static scope => Historied(scope).Bind(history => scope.Commit(history.Group, history.Group.Defaults)).Map(unit)));

    private static IO<HistoryBinding> Historied(RowScope scope) => IO.lift(() => scope.History.ToFin(new RhinoFault("section value history", RhinoReason.Missing)));

    // --- [VALUES]
    private static IO<Seq<Seq<Command<Invoke>>>> Listing(HistoryBinding history, RowScope scope) =>
        from held in history.History
        from pastes in IO.lift(static () => Clipboard.Instance.ContainsText)
        from zone in IO.lift(() => history.Clock.ToZonedClock().Zone)
        let stamp = LocalDateTimePattern.Create("g", LocalText.Culture)
        select Seq(
            held.Done.Head.Map(entry => Ran("section-undo", LocalText.Of("Undo {0}", Described(entry)).Local, Heard(history.Undo, scope))).ToSeq()
                + held.Undone.Head.Map(entry => Ran("section-redo", LocalText.Of("Redo {0}", Described(entry)).Local, Heard(history.Redo, scope))).ToSeq(),
            Seq(Ran("section-copy", "Copy values", DataTransfer.Copy(history.Group, None, scope)))
                + Seq(Ran("section-paste", "Paste values", DataTransfer.Paste(history.Group, None, scope))).Filter(_ => pastes),
            Seq(Ran("section-clear", "Clear history", history.Clear)).Filter(_ => !(held.Done.IsEmpty && held.Undone.IsEmpty))
                + Seq<Command<Invoke>>(new Command<Invoke>.Menu(new CommandFace("section-restore", "Restore"), None, [held.Done.Map(entry => Ran(
                    string.Create(CultureInfo.InvariantCulture, $"section-restore-{entry.Serial}"),
                    LocalText.Of("{0}, {1}", Described(entry), stamp.Format(entry.At.InZone(zone).LocalDateTime)).Local,
                    Heard(history.Restore(entry.Serial), scope)))])).Filter(_ => !held.Done.IsEmpty));

    private static Command<Invoke> Ran<T>(string name, string caption, IO<T> run) =>
        new Command<Invoke>.Run(new CommandFace(name, caption), None, new Invoke.Effect(_ => run.Map(unit)));

    private static IO<ValueDiff> Heard(IO<ValueDiff> step, RowScope scope) =>
        from diff in step
        from changed in scope.Changed(toSeq(diff.Changes.Keys))
        select diff;

    private static string Described(HistoryEntry entry) =>
        toSeq(entry.Diff.Changes.Keys) switch {
            [var only] => LocalText.Of(EntryKey.Name(only.Path)).Local,
            var many => LocalText.Of(many.Count is 1 ? "{0} value" : "{0} values", many.Count).Local,
        };
}

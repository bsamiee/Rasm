using System.Drawing;
using Eto.Forms;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.UI;
using Rhino.UI.Controls;
using SuperLinq;

namespace Rasm.Rhino.UI;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ViewBody {
    // --- [STATE]
    private readonly View row;
    private readonly RowScope enclosing;
    private readonly EnvIO scope;
    private readonly Option<Func<ViewBody, IO<Unit>>> listening;
    private readonly Ref<Option<Owned<Unit>>> listeners = Ref(Option<Owned<Unit>>.None);
    private readonly Atom<bool> hidden = Atom(value: false);
    private bool showing;

    private ViewBody(View row, Control content, RowScope nested, RowScope enclosing, BindingGroup group, Seq<IO<Unit>> clicks, Option<Func<ViewBody, IO<Unit>>> listening, EnvIO scope) =>
        (this.row, Content, Scope, this.enclosing, Group, Clicks, this.listening, this.scope) = (row, content, nested, enclosing, group, clicks, listening, scope);

    public Control Content { get; }
    public RowScope Scope { get; }
    public BindingGroup Group { get; }
    public bool Hidden => hidden.Value;
    public int Height => hidden.Value ? 0 : (int)Math.Ceiling(Content.GetPreferredSize().Height);

    internal Seq<IO<Unit>> Clicks { get; }

    // --- [REALIZATION]
    public static IO<ViewBody> Realize(View row, Seq<Command<Invoke>> actions, RowScope enclosing) =>
        Nesting(row, enclosing).Bind(nesting => Built(row, actions, enclosing, nesting.Bindings, nesting.Scope, None));

    public static IO<ViewBody> Disclosed(View row, RowScope enclosing) =>
        ViewOps.Bindings(row, enclosing).Bind(bindings => Built(row, [], enclosing, bindings, enclosing.Separated(visible: false), None));

    public static IO<ViewBody> Open(View row, Option<RhinoDoc> document, CommitMode mode, Option<Func<ViewBody, IO<Unit>>> listening) =>
        from enclosing in RowScope.Open(document, mode)
        from nesting in Nesting(row, enclosing)
        from body in Built(row, [], enclosing, nesting.Bindings, nesting.Scope, listening)
        from opened in body.Scope.Opened(body.Group)
        select body;

    private static IO<(BindingGroup Bindings, RowScope Scope)> Nesting(View row, RowScope enclosing) =>
        from bindings in ViewOps.Bindings(row, enclosing)
        from history in IO.lift(() => ViewOps.History(row, bindings, enclosing.Document))
        select (bindings, enclosing.Nested(ViewOps.Settings(row), history, ViewOps.Locks(row, enclosing.Document)));

    private static IO<ViewBody> Built(View row, Seq<Command<Invoke>> actions, RowScope enclosing, BindingGroup bindings, RowScope nested, Option<Func<ViewBody, IO<Unit>>> listening) =>
        from scope in IO.env
        from hidden in nested.Shown(visible: false)
        from content in ViewOps.Realize(row.Children, nested)
        from clicks in actions.TraverseM(action => CommandRows.Press(action, nested)).As()
        from built in use(IO.lift(() => new ViewBody(row, content, nested, enclosing, bindings, clicks, listening, scope)), static body => body.Scope.Shown(visible: false))
        from visibility in enclosing.OnVisibility(_ => built.Evaluated)
        select built;

    // --- [CALLBACKS]
    public IO<Unit> Shown(bool visible) =>
        IO.lift(() => showing = visible).Bind(_ => Products.Each(Seq(ViewOps.Toggled(listeners, visible, Listening).Scoped(scope), Evaluated))).Map(unit);

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
        from sections in SectionHolder.HolderVisible(Content, showing && visible && shows && enables)
        select unit;

    private IO<Unit> Listening =>
        from heard in Scope.Listen(_ => Evaluated)
        from extra in listening.Traverse(listen => listen(this)).As()
        from reread in Reread
        select unit;
}

internal sealed class HostedView(Func<IO<(ViewBody Body, Control Frame)>> realize) {
    // --- [STATE]
    private readonly Ref<Option<Owned<(ViewBody Body, Control Frame)>>> realized = Ref(Option<Owned<(ViewBody Body, Control Frame)>>.None);

    // --- [REALIZATION]
    internal EnvIO Scope { get; } = EnvIO.New(token: CancellationToken.None);

    internal Option<ViewBody> Current => realized.Value.Map(static held => held.Value.Body);

    internal Option<(ViewBody Body, Control Frame)> Realized =>
        realized.Value.Map(static held => held.Value) || Session.Run(Realizing.Map(Some), Option<(ViewBody Body, Control Frame)>.None);

    internal Option<Control> Frame => Realized.Map(static held => held.Frame);

    internal IO<(ViewBody Body, Control Frame)> Realizing =>
        IO.lift(() => realized.Value).Bind(held => held.Match(
            Some: static owned => IO.pure(owned.Value),
            None: () => Products.Replace(realized,
                from made in realize()
                from frame in IO.lift(() => new HostFrame(this) { Content = made.Frame })
                select (made.Body, Frame: (Control)frame)).Scoped(Scope)));

    internal static IO<(ViewBody Body, Control Frame)> Opened(View row, Option<RhinoDoc> document, CommitMode mode, Option<Func<ViewBody, IO<Unit>>> listening) =>
        from body in ViewBody.Open(row, document, mode, listening)
        from frame in row.Children.Exists(static child => child.Fill)
            ? IO.pure(body.Content)
            : Containers.Frame(static () => new RhinoScrollableDialogPanel()).Bind(scroll => IO.lift(() => {
                scroll.Content = body.Content;
                return (Control)scroll;
            }))
        select (body, frame);

    internal static IO<(ViewBody Body, Control Frame)> Styled(IO<(ViewBody Body, Control Frame)> opened) =>
        opened.Bind(static held => IO.lift(() => {
            held.Frame.UseRhinoStyle();
            return held;
        }));

    // --- [CALLBACKS]
    internal Unit Shown(bool visible) =>
        Session.Run((visible ? Realized : realized.Value.Map(static held => held.Value)).Traverse(held => held.Body.Shown(visible)).As());

    internal bool Activated(bool active) {
        _ = Shown(active);
        return true;
    }

    internal Unit Reread => Session.Run(Current.Traverse(static body => body.Reread).As());

    internal bool Run(Func<ViewBody, IO<Unit>> step) => Session.Run(Current.Traverse(step).As().Map(static _ => true), failed: false);

    internal static string HelpUrl(View row) => Conversions.Unset(HelpTopic.Of(row.Help).Map(static string (topic) => topic));

    internal static Result Scripted(View row, IO<RhinoDoc> document, CommitMode mode) =>
        Session.Run(
            Results.Of(
                from doc in document
                from scope in RowScope.Open(Some(doc), mode)
                from scripted in Showing.RunScript(row, scope, doc)
                select scripted),
            Result.Failure);

    // --- [RELEASE]
    internal Unit Release() {
        _ = Session.Run(IO.lift(Scope.DisposeResources));
        Scope.Dispose();
        return unit;
    }
}

internal sealed class HostFrame(HostedView hosted) : Panel {
    protected override void Dispose(bool disposing) {
        if (disposing) _ = hosted.Release();
        base.Dispose(disposing);
    }
}

internal sealed class HostedWindow : Form {
    internal HostedWindow(View.Window row, RhinoDoc document) : base(row.Handler()) =>
        (Row, Hosted) = (row, new HostedView(() => HostedView.Opened(row, Some(document), new CommitMode.Immediate(), None)));

    internal View.Window Row { get; }

    internal HostedView Hosted { get; }

    protected override void OnClosed(EventArgs e) {
        base.OnClosed(e);
        Dispose();
    }
}

public abstract class DefinedPanel<TSelf> : Panel, IPanel, IHelp where TSelf : DefinedPanel<TSelf>, IDefined<View.Panel> {
    private readonly HostedView hosted;

    protected DefinedPanel(RhinoDoc document) {
        hosted = new HostedView(() => HostedView.Opened(TSelf.Definition, Some(document), new CommitMode.Immediate(), None));
        _ = TSelf.Definition.MinimumSize.Iter(size => MinimumSize = size);
        Content = hosted.Frame.ValueUnsafe();
    }

    public string HelpUrl => HostedView.HelpUrl(TSelf.Definition);

    public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) => _ = hosted.Shown(visible: true);

    public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) => _ = hosted.Shown(visible: false);

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) => _ = hosted.Shown(visible: false);
}

internal sealed class DefinedOptionsPage : OptionsDialogPage {
    private readonly View.Page row;
    private readonly HostedView hosted;

    internal DefinedOptionsPage(View.Page row, Option<RhinoDoc> document) : base(row.Caption) {
        this.row = row;
        hosted = new HostedView(() => HostedView.Styled(HostedView.Opened(row, document, row.Commit, Some(Marking))));
        (NavigationTextIsBold, NavigationTextColor) = (row.NavigationBold, Conversions.Unset(row.NavigationColor));
    }

    public override string LocalPageTitle => LocalText.Of(row.Caption).Local;

    public override Image? PageImage =>
        Session.Run(IconSlot.Page.Square.Bind(pixels => row.Icon.Match(
                Some: icon => Icons.Raster(icon, pixels).Map(static Image? (bitmap) => bitmap),
                None: () => IO.lift(Image? () => Session.PlugIn.Icon(pixels)))),
            failed: null);

    public override object? PageControl => hosted.Frame.ValueUnsafe();

    public override bool ShowApplyButton => row.Commit.Drafts;

    public override bool ShowDefaultsButton => row.Store.IsSome;

    public override bool OnActivate(bool active) => hosted.Activated(active);

    public override void OnHelp() => _ = Session.Run(HelpTopic.Shown(row.Help));

    public override bool OnApply() => hosted.Run(static body => body.Scope.Apply(body.Group));

    public override void OnCancel() => _ = hosted.Run(static body => body.Scope.Revert(body.Group));

    public override void OnDefaults() => _ = hosted.Run(static body => body.Scope.Commit(body.Group, body.Group.Defaults).Map(unit));

    public override Result RunScript(RhinoDoc doc, RunMode mode) => HostedView.Scripted(row, IO.pure(doc), new CommitMode.Immediate());

    private IO<Unit> Marking(ViewBody body) =>
        from listened in body.Scope.Listen(_ => Marked(body))
        from marked in Marked(body)
        select unit;

    private IO<Unit> Marked(ViewBody body) =>
        from pending in body.Scope.Pending
        from shows in CommandRows.Holds(row.Visible, body.Scope)
        from written in IO.lift(() => {
            Modified = pending;
            if (!shows) RemovePage();
        })
        select unit;
}

internal sealed class DefinedPropertiesPage : ObjectPropertiesPage {
    private readonly View.PropertiesPage row;
    private readonly HostedView hosted;

    internal DefinedPropertiesPage(View.PropertiesPage row) {
        this.row = row;
        hosted = new HostedView(() => HostedView.Styled(HostedView.Opened(row, Optional(new ObjectPropertiesPageEventArgs(this).Document), new CommitMode.Targets(this), Refreshing)));
    }

    public override string EnglishPageTitle => row.Caption;

    public override string LocalPageTitle => LocalText.Of(row.Caption).Local;

    public override PropertyPageType PageType => row.Subject.Switch(() => base.PageType, objects: static (fallback, held) => held.PageType.IfNone(fallback), viewport: static (_, _) => PropertyPageType.View);

    public override ObjectType SupportedTypes => Objects.Bind(static held => held.SupportedTypes).IfNone(() => base.SupportedTypes);

    public override bool AllObjectsMustBeSupported => Objects.Bind(static held => held.AllObjectsMustBeSupported).IfNone(() => base.AllObjectsMustBeSupported);

    public override bool SupportsSubObjects => Objects.Bind(static held => held.SupportsSubObjects).IfNone(() => base.SupportsSubObjects);

    public override int Index => row.Index.IfNone(() => base.Index);

    public override Icon? PageIcon(Size sizeInPixels) =>
        Session.Run(row.Icon.TraverseM(icon => Icons.DrawingIcon(icon, sizeInPixels)).As(), Option<Icon>.None).ValueUnsafe();

    public override object? PageControl => hosted.Frame.ValueUnsafe();

    private Option<PageSubject.Objects> Objects => row.Subject.Switch(objects: static held => Some(held), viewport: static _ => Option<PageSubject.Objects>.None);

    public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e) =>
        Session.Run(
            from included in IO.lift(() => row.Subject.Switch(
                (Args: e, Objects: (Func<ObjectPropertiesPageEventArgs, bool>)base.ShouldDisplay),
                objects: static (state, _) => state.Objects(state.Args),
                viewport: static (state, held) => Optional(state.Args.Viewport).Exists(port => held.ViewportTypes.Contains(port.ViewportType))))
            from shown in included
                ? RowScope.Open(Optional(e.Document), new CommitMode.Targets(this)).Bind(scope => CommandRows.Holds(row.Visible, scope))
                : IO.pure(value: false)
            select shown,
            failed: false);

    public override void UpdatePage(ObjectPropertiesPageEventArgs e) => _ = hosted.Reread;

    public override bool OnActivate(bool active) => hosted.Activated(active);

    public override void OnHelp() => _ = Session.Run(HelpTopic.Shown(row.Help));

    public override Result RunScript(ObjectPropertiesPageEventArgs e) =>
        HostedView.Scripted(row, IO.lift(() => Conversions.Required(e.Document)), new CommitMode.Targets(this));

    private Option<Func<ViewBody, IO<Unit>>> Refreshing =>
        row.Subject.Switch(
            this,
            objects: static (page, _) => Some<Func<ViewBody, IO<Unit>>>(body =>
                from post in Mailboxes.Idle<Guid, Unit>(changed =>
                    IO.lift(() => toHashSet(toSeq(page.SelectedObjects).Map(static target => target.Id)))
                        .Bind(selected => when(changed.Keys.Exists(selected.Contains), body.Reread).As()))
                from attached in body.Scope.Document.ToSeq()
                    .Bind(static document => Seq(
                        Subscriptions.On<RhinoModifyObjectAttributesEventArgs>(static h => RhinoDoc.ModifyObjectAttributes += h, static h => RhinoDoc.ModifyObjectAttributes -= h).Documents(static args => args.DocumentSerialNumber).In(document.RuntimeSerialNumber).Choose(static args => Some(args.RhinoObject.Id)),
                        Subscriptions.Watched<RhinoReplaceObjectEventArgs>(static (watcher, h) => watcher.ReplaceRhinoObject += h).Documents(static args => args.DocumentSerialNumber).In(document.RuntimeSerialNumber).Choose(static args => Some(args.ObjectId))))
                    .TraverseM(signal => signal.Attach(id => post(id, unit))).As()
                select unit),
            viewport: static (_, _) => Option<Func<ViewBody, IO<Unit>>>.None);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ViewOps {
    // --- [STORES]
    public static IO<BindingGroup> Bindings(View view, RowScope scope) =>
        view.Store.Bind(static store => store.Bindings).Match(
            Some: stored => scope.DocumentIO.Bind(stored),
            None: () => IO.pure(new BindingGroup(view.Children.Bind(static child => child.Controls).Choose(static rows => rows.Bound).Map(bound => bound(scope)))));

    public static Option<Preference<HashSet<EntryKey>>> Locks(View view, Option<RhinoDoc> document) =>
        from doc in document
        from locks in view.Store.Bind(static store => store.Locks)
        select locks(doc, view.Identity.GUID);

    public static SettingsNode Settings(View view) => ViewCatalog.Settings.Child(EntryKey.TypeOwner(view.Identity.GUID));

    public static Fin<Option<HistoryBinding>> History(View view, BindingGroup bound, Option<RhinoDoc> document) =>
        (from store in view.Store
         from frame in ViewCatalog.Current.Definition.Frames.Choose(static frame => Optional(frame as HistoryFrame)).Head
         select store.Switch(
                 (View: view, Document: document, Frame: frame),
                 applicationStores: static (state, _) => new HistoryScope.ApplicationScope(state.Frame.Registry),
                 documentStores: static (state, held) =>
                     state.Document.ToFin(new RhinoFault(state.View.Identity.Name, RhinoReason.Unsupported)).Map(doc => (HistoryScope)new HistoryScope.DocumentScope(doc, held.Redraw)))
             .Map(scope => new HistoryBinding(view.Identity.GUID, LocalText.Of(view.Caption).Local, bound, scope, frame.Limits, frame.Clock)))
        .Traverse(static held => held)
        .As();

    // --- [REALIZATION]
    public static IO<Control> Realize(Seq<Child> children, RowScope scope) =>
        from stack in Containers.Frame(static () => new RhinoNestedStackLayout(Orientation.Vertical, RhinoLayout.SpacingType.Panel))
        from parts in Runs(children).TraverseM(run => Part(run, scope)).As()
        from configured in IO.lift(() => {
            stack.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            _ = parts.Iter(part => stack.Items.Add(new StackLayoutItem(part.Body, part.Fill)));
        })
        select (Control)stack;

    private static Seq<(Child Lead, Seq<Child> Run)> Runs(Seq<Child> children) =>
        toSeq(children.Segment(static (current, previous, _) => !current.IsControl || !previous.IsControl))
            .Map(static run => (Lead: run[0], Run: toSeq(run)));

    private static IO<(Control Body, bool Fill)> Part((Child Lead, Seq<Child> Run) run, RowScope scope) =>
        run.Lead.Switch(
                (run.Run, Scope: scope),
                control: static (state, _) => RowTable.Realize(Joined(state.Run.Map(static child => child.AsControl)), state.Scope).Map(static table => table.View),
                holder: static (state, holder) => SectionHolder.Hold(holder, state.Scope).Map(static held => (Control)held),
                container: static (state, container) => Containers.Realize(container, state.Scope))
            .Map(body => (Body: body, Fill: run.Run.Exists(static child => child.Fill)));

    private static ControlRows Joined(Seq<ControlRows> runs) =>
        new(scope => runs.TraverseM(rows => rows.Read(scope)).As().Map(static rows => rows.Flatten()), scope => runs.TraverseM(rows => rows.Signals(scope)).As().Map(static signals => signals.Flatten())) {
            Fill = runs.Exists(static rows => rows.Fill),
        };

    // --- [LIFECYCLE]
    public static HostEvent<bool> Loaded(Control control) =>
        Subscriptions.Merged(
            Subscriptions.On<EventArgs>(h => control.LoadComplete += h, h => control.LoadComplete -= h).Choose(static _ => Some(value: true)),
            Subscriptions.On<EventArgs>(h => control.UnLoad += h, h => control.UnLoad -= h).Choose(static _ => Some(value: false)));

    public static IO<Unit> WhileLoaded(Control control, IO<Unit> listen) =>
        from scope in IO.env
        from held in IO.lift(static () => Ref(Option<Owned<Unit>>.None))
        from changes in Loaded(control).Attach(shown => Toggled(held, shown, listen).Scoped(scope))
        from current in IO.lift(() => control.Loaded).Bind(shown => Toggled(held, shown, listen))
        select unit;

    public static IO<Unit> Toggled(Ref<Option<Owned<Unit>>> held, bool shown, IO<Unit> acquire) =>
        shown
            ? IO.lift(() => held.Value.IsSome).Bind(present => unless(present, Products.Replace(held, acquire)).As())
            : Products.Clear(held);
}

using System.Drawing;
using System.Globalization;
using Eto.Forms;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.PlugIns;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Controls.DataSource;
using DataSourceEventArgs = Rhino.UI.Controls.DataSource.EventArgs;

namespace Rasm.Rhino.UI;

// --- [MODELS] --------------------------------------------------------------------------
internal sealed record PageFrame(ViewBody Body, Control Frame);

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ViewHost(IPlugInLifetime owner) : Panel {
    internal EnvIO Scope { get; } = EnvIO.New(token: CancellationToken.None);

    internal IO<Unit> Seat(View row, Control body) =>
        (row.Children.Exists(static child => child.Fill)
            ? IO.pure(body)
            : Containers.Frame(static () => new RhinoScrollableDialogPanel()).Map(scroll => {
                scroll.Content = body;
                return (Control)scroll;
            }))
        .Bind(seated => IO.lift(() => { Content = seated; }))
        .Under(Scope);

    protected override void Dispose(bool disposing) {
        if (disposing) {
            _ = owner.Run(IO.lift(Scope.DisposeResources));
            Scope.Dispose();
        }
        base.Dispose(disposing);
    }
}

public abstract class DefinedWindow(View.Window row, IPlugInViews owner) : Form(row.Kind.Handler()) {
    // --- [STATE]
    internal EnvIO Scope { get; } = EnvIO.New(token: CancellationToken.None);

    // --- [IDENTITY]
    internal View.Window Row => row;

    internal IPlugInViews Views => owner;

    // --- [EDGES]
    protected override void OnClosed(System.EventArgs e) =>
        _ = owner.Run(Callbacks.Each(Seq(IO.lift(() => base.OnClosed(e)), IO.lift(Dispose))).Uninterruptible());

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) {
            _ = owner.Run(IO.lift(Scope.DisposeResources));
            Scope.Dispose();
        }
        base.Dispose(disposing);
    }
}

public abstract class ViewPanel : Panel {
    // --- [STATE]
    private readonly EnvIO scope = EnvIO.New(token: CancellationToken.None);

    // --- [REALIZATION]
    private protected ViewPanel(View row, Option<RhinoDoc> document, IPlugInViews owner) {
        (Row, Owner) = (row, owner);
        _ = owner.Run(
            (from body in ViewBody.Open(owner, row, document, new CommitMode.Immediate(), None)
             from configured in IO.lift(() => {
                 Realized = Some(body);
                 body.Content.UseRhinoStyle();
                 Content = body.Content;
             })
             select unit).Under(scope));
    }

    // --- [IDENTITY]
    private protected View Row { get; }

    private protected IPlugInViews Owner { get; }

    private protected Option<ViewBody> Realized { get; private set; }

    // --- [EDGES]
    private protected new IO<Unit> Shown(bool visible) => Realized.Traverse(body => body.Shown(visible)).As().Map(unit);

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        Realized = None;
        if (disposing) {
            _ = Owner.Run(IO.lift(scope.DisposeResources));
            scope.Dispose();
        }
        base.Dispose(disposing);
    }
}

public abstract class DefinedPanel(RhinoDoc? document, View.Panel row) : ViewPanel(row, Optional(document), PlugInOwners.Owner(row.Identity)), IPanel, IHelp {
    // --- [IDENTITY]
    public string HelpUrl => Conversions.Unset(HelpTopic.Of(Row.Help, Owner).Map(static string (topic) => topic));

    // --- [EDGES]
    public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) => _ = Owner.Run(Shown(visible: true));

    public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) => _ = Owner.Run(Shown(visible: false));

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) => _ = Owner.Run(Shown(visible: false));
}

public abstract class DefinedSection : EtoCollapsibleSection3 {
    // --- [STATE]
    private readonly View.Section row;
    private readonly IPlugInViews owner;
    private readonly SectionLifetime lifetime;
    private Option<IO<ViewBody>> source;

    protected DefinedSection(View.Section row, IPlugInViews owner) {
        (this.row, this.owner) = (row, owner);
        lifetime = new SectionLifetime(this, owner, None, static () => false, visible: false, IO.lift(() => Conversions.Found(source)).Flatten());
    }

    // --- [IDENTITY]
    public override LocalizeStringPair Caption => RowText.Localize(row.Caption, owner);

    public override Guid PlugInId => owner.PlugIn.Id;

    public override int SectionHeight => lifetime.Height;

    public override bool Hidden => lifetime.Hidden;

    public override IHeaderButtonHandler NewHeaderButtonHandler() => lifetime.Header(row).IfNone(base.NewHeaderButtonHandler);

    // --- [REALIZATION]
    public IO<Unit> Attach(IO<ViewBody> realize) => IO.lift(() => { source = Some(realize); });

    // --- [EDGES]
    internal Option<SectionHolder> Holder { get; private set; }

    public override void OnAttachedToHolder(ICollapsibleSectionHolder2 holder) {
        base.OnAttachedToHolder(holder);
        Holder = Optional(holder as SectionHolder);
    }

    public override void HolderVisible(bool visible) => _ = owner.Run(lifetime.Visible(visible));

    public override void UpdateView(uint flags) => _ = owner.Run(lifetime.Reread);

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) lifetime.Dispose();
        base.Dispose(disposing);
    }
}

internal sealed class DisclosureSection(View.Section row, IPlugInViews owner, Guid id, bool expanded) : DefinedSection(row, owner), ICollapsibleSection {
    // --- [IDENTITY]
    Guid ICollapsibleSection.Id => id;

    public override string SettingsTag => id.ToString("D", CultureInfo.InvariantCulture);

    public override bool InitiallyExpanded => expanded;
}

public abstract class DefinedContentSection : EtoContentUISection3 {
    // --- [STATE]
    private readonly View.Section row;
    private readonly ContentScope scope;
    private readonly IPlugInViews owner;
    private readonly SectionLifetime lifetime;

    protected DefinedContentSection(View.Section row, ContentScope scope, IPlugInViews owner) {
        (this.row, this.scope, this.owner) = (row, scope, owner);
        lifetime = new SectionLifetime(this, owner, Some(ProviderIds.ContentSelection), () => Excluded, visible: true, Realized);
    }

    // --- [IDENTITY]
    public override LocalizeStringPair Caption => RowText.Localize(row.Caption, owner);

    public override Guid PlugInId => owner.PlugIn.Id;

    public override int SectionHeight => lifetime.Height;

    public override bool Hidden => Excluded || lifetime.Hidden;

    public override IHeaderButtonHandler NewHeaderButtonHandler() => lifetime.Header(row).IfNone(base.NewHeaderButtonHandler);

    private bool Excluded => base.Hidden || !Selected.Head.ForAll(scope.Covers);

    private Seq<RenderContent> Selected => toSeq(GetSelection()).Strict();

    // --- [EDGES]
    public override void UpdateView(uint flags) => _ = owner.Run(lifetime.Reread);

    // --- [REALIZATION]
    private IO<ViewBody> Realized =>
        from rows in RowScope.Open(owner, None, new CommitMode.Contents(this))
        from held in ViewBody.Realize(owner, row, rows)
        from fields in FieldChanges(rows)
        from selections in Subscriptions.Host<DataSourceEventArgs>(h => DataChanged += h, h => DataChanged -= h)
            .Inline(args => when(args.DataType == ProviderIds.ContentSelection, held.Reread).As(), owner)
        select held;

    private IO<Unit> FieldChanges(RowScope rows) =>
        from post in Mailboxes.Idle<EntryKey, Guid>(changed => IO.lift(() => toHashSet(Selected.Map(static content => content.Id)))
            .Bind(selected => rows.Changed(toSeq(changed.Filter(selected.Contains).Keys))))(owner)
        from attached in EventKind.ContentFieldChanged.Inline(args => post(ViewRegistration.FieldKey(args.Content, args.FieldName), args.Content.Id), owner)
        select unit;

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) lifetime.Dispose();
        base.Dispose(disposing);
    }
}

public abstract class DefinedOptionsPage : OptionsDialogPage {
    // --- [STATE]
    private readonly View row;
    private readonly CommitMode commit;
    private readonly IPlugInViews owner;
    private readonly Atom<Option<RhinoDoc>> document = Atom(Option<RhinoDoc>.None);
    private readonly Ref<Option<Lease<PageFrame>>> page = Ref(Option<Lease<PageFrame>>.None);

    protected DefinedOptionsPage(View.OptionsPage row, IPlugInViews owner) : this(row, row.Commit, row.Navigation, owner) { }

    protected DefinedOptionsPage(View.DocumentPage row, IPlugInViews owner) : this(row, row.Commit, row.Navigation, owner) { }

    private DefinedOptionsPage(View row, CommitMode commit, Option<NavigationText> navigation, IPlugInViews owner) : base(row.Caption) {
        (this.row, this.commit, this.owner) = (row, commit, owner);
        _ = navigation.Iter(text => (NavigationTextIsBold, NavigationTextColor) = (text.Bold, Conversions.Unset(text.Color)));
    }

    // --- [IDENTITY]
    public override string LocalPageTitle => RowText.Localize(row.Caption, owner).Local;

    public override Image? PageImage =>
        owner.Answer(
            new Size(128, 128) switch {
                var size => row.Icon.Match(
                    Some: icon => Icons.Raster(icon, size).Map(static Image? (bitmap) => bitmap),
                    None: () => IO.lift(Image? () => owner.PlugIn.Icon(size))),
            },
            failed: null);

    public override object? PageControl => Realized.Map(static object (held) => held.Frame).ValueUnsafe();

    public override bool ShowApplyButton => commit.Drafts;

    public override bool ShowDefaultsButton => row.Store.IsSome;

    public IO<Unit> ForDocument(RhinoDoc held) => document.SwapIO(_ => Some(held)).Map(unit);

    public IO<Unit> Navigate(PageTarget target) => Showing.SetActivePageTo(this, target);

    // --- [EDGES]
    public override bool OnActivate(bool active) =>
        owner.Answer((active ? Realized : Current).Traverse(held => held.Body.Shown(active)).As().Map(static _ => true), failed: false);

    public override void OnHelp() => _ = owner.Run(HelpTopic.Of(row.Help, owner).Traverse(static topic => topic.Show).As());

    // --- [COMMITS]
    public override bool OnApply() =>
        owner.Answer(Current.Traverse(static held => held.Body.Scope.Apply(held.Body.Group)).As().Map(static _ => true), failed: false);

    public override void OnCancel() =>
        _ = owner.Run(Current.Traverse(static held => held.Body.Scope.Revert(held.Body.Group)).As());

    public override void OnDefaults() =>
        _ = owner.Run(Current.Traverse(static held => held.Body.Scope.Commit(held.Body.Group, held.Body.Group.Defaults)).As());

    // --- [SCRIPTS]
    public override Result RunScript(RhinoDoc doc, RunMode mode) =>
        owner.Answer(Results.Of(RowScope.Open(owner, Some(doc), new CommitMode.Immediate()).Bind(scope => Showing.RunScript(owner, row, scope, doc))), Result.Failure);

    // --- [REALIZATION]
    private Option<PageFrame> Current => page.Value.Map(static lease => lease.Value);

    private Option<PageFrame> Realized =>
        Current || owner.Answer(
            from held in document.ValueIO
            from frame in IO.lift(() => new ViewHost(owner))
            from realized in Products.Swapped(page,
                    from body in ViewBody.Open(owner, row, held, commit, Some(Marking))
                    from seated in frame.Seat(row, body.Content)
                    from configured in IO.lift(frame.UseRhinoStyle)
                    select new PageFrame(body, frame))
                .Under(frame.Scope)
                .Catch(error => IO.lift(frame.Dispose) >> IO.fail<PageFrame>(error))
            select Some(realized),
            Option<PageFrame>.None);

    private IO<Unit> Marking(ViewBody body) =>
        from listened in body.Scope.Listen(_ => Marked(body))
        from marked in Marked(body)
        select unit;

    private IO<Unit> Marked(ViewBody body) =>
        body.Scope.Pending.Bind(pending => IO.lift(() => { Modified = pending; }));
}

public abstract class DefinedPropertiesPage(View.PropertiesPage row, IPlugInViews owner) : ObjectPropertiesPage {
    // --- [STATE]
    private readonly Ref<Option<Lease<PageFrame>>> page = Ref(Option<Lease<PageFrame>>.None);

    // --- [IDENTITY]
    public override string EnglishPageTitle => row.Caption;

    public override string LocalPageTitle => RowText.Localize(row.Caption, owner).Local;

    public override PropertyPageType PageType => row.Subject.Switch(() => base.PageType, objects: static (fallback, held) => held.PageType.IfNone(fallback), viewport: static (_, _) => PropertyPageType.View);

    public override ObjectType SupportedTypes => ObjectSubject.Bind(static held => held.SupportedTypes).IfNone(() => base.SupportedTypes);

    public override bool AllObjectsMustBeSupported => ObjectSubject.Bind(static held => held.AllObjectsMustBeSupported).IfNone(() => base.AllObjectsMustBeSupported);

    public override bool SupportsSubObjects => ObjectSubject.Bind(static held => held.SupportsSubObjects).IfNone(() => base.SupportsSubObjects);

    public override int Index => row.Index.IfNone(() => base.Index);

    public override string? PageIconEmbeddedResourceString =>
        row.Icon.Filter(icon => icon.Assembly == GetType().Assembly).Map(static icon => icon.Id).ValueUnsafe();

    public override Icon? PageIcon(Size sizeInPixels) =>
        owner.Answer(row.Icon.TraverseM(icon => Icons.DrawingIcon(icon, sizeInPixels)).As(), Option<Icon>.None).ValueUnsafe();

    public override object? PageControl => Current.Map(static object (held) => held.Frame).ValueUnsafe();

    private Option<PageSubject.Objects> ObjectSubject =>
        row.Subject.Switch(objects: static held => Some(held), viewport: static _ => Option<PageSubject.Objects>.None);

    // --- [EDGES]
    public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e) =>
        owner.Answer(
            from included in IO.lift(() => row.Subject.Switch(
                (Args: e, Objects: (Func<ObjectPropertiesPageEventArgs, bool>)base.ShouldDisplay),
                objects: static (state, _) => state.Objects(state.Args),
                viewport: static (state, held) => Optional(state.Args.Viewport).Exists(port => held.ViewportTypes.Contains(port.ViewportType))))
            from shown in included
                ? RowScope.Open(owner, Optional(e.Document), new CommitMode.Targets(this)).Bind(scope => CommandRows.Holds(row.Visible, scope))
                : IO.pure(value: false)
            select shown,
            failed: false);

    public override void UpdatePage(ObjectPropertiesPageEventArgs e) =>
        _ = owner.Run(Current.Traverse(static held => held.Body.Reread).As());

    public override bool OnActivate(bool active) =>
        owner.Answer((active ? Realized : Current).Traverse(held => held.Body.Shown(active)).As().Map(static _ => true), failed: false);

    public override void OnHelp() => _ = owner.Run(HelpTopic.Of(row.Help, owner).Traverse(static topic => topic.Show).As());

    // --- [SCRIPTS]
    public override Result RunScript(ObjectPropertiesPageEventArgs e) =>
        owner.Answer(
            Results.Of(
                from document in IO.lift(() => Conversions.Found(e.Document))
                from scope in RowScope.Open(owner, Some(document), new CommitMode.Targets(this))
                from scripted in Showing.RunScript(owner, row, scope, document)
                select scripted),
            Result.Failure);

    // --- [REALIZATION]
    private Option<PageFrame> Current => page.Value.Map(static lease => lease.Value);

    private Option<PageFrame> Realized =>
        Current || owner.Answer(
            from document in IO.lift(() => Optional(new ObjectPropertiesPageEventArgs(this).Document))
            from host in IO.lift(() => new ViewHost(owner))
            from realized in Products.Swapped(page,
                    from body in ViewBody.Open(owner, row, document, new CommitMode.Targets(this), Refreshing)
                    from configured in IO.lift(() => host.Content = body.Content)
                    select new PageFrame(body, host))
                .Under(host.Scope)
                .Catch(error => IO.lift(host.Dispose) >> IO.fail<PageFrame>(error))
            select Some(realized),
            Option<PageFrame>.None);

    private Option<Func<ViewBody, IO<Unit>>> Refreshing =>
        row.Subject.Switch(
            this,
            objects: static (page, _) => Some<Func<ViewBody, IO<Unit>>>(body =>
                from post in Mailboxes.Idle<Guid, Unit>(changed =>
                    IO.lift(() => toHashSet(Conversions.Rows(new ObjectPropertiesPageEventArgs(page).Objects).Map(static target => target.Id)))
                        .Bind(selected => when(changed.Keys.Exists(selected.Contains), body.Reread).As()))(body.Scope.Owner)
                from attached in body.Scope.Document.ToSeq()
                    .Bind(static document => Seq(
                        EventKind.ModifyObjectAttributes.In(document.RuntimeSerialNumber).Choose(static args => Some(args.RhinoObject.Id)),
                        EventKind.ReplaceRhinoObject.In(document.RuntimeSerialNumber).Choose(static args => Some(args.ObjectId))))
                    .TraverseM(signal => signal.Inline(id => post(id, unit), body.Scope.Owner)).As()
                select unit),
            viewport: static (_, _) => Option<Func<ViewBody, IO<Unit>>>.None);
}

public abstract class DefinedTab(View.Tab row) : ViewPanel(row, Optional(RhinoDoc.ActiveDoc), PlugInOwners.Owner(row.Identity)) {
    // --- [EDGES]
    public void OnVisibilityChanged(bool visible) => _ = Owner.Run(Shown(visible));

    public void TabActivated(uint state) => _ = Owner.Run(Shown(state != 0));

    public void DisplayData() => _ = Owner.Run(Realized.Traverse(static body => body.Reread).As());

    public void DoHelp() => _ = Owner.Run(HelpTopic.Of(Row.Help, Owner).Traverse(static topic => topic.Show).As());

    // --- [RELEASE]
    public void Destroy() => Dispose();
}

public abstract class DefinedPane(View.Pane row) : ViewPanel(row, Optional(RhinoDoc.ActiveDoc), PlugInOwners.Owner(row.Identity)), IHelp {
    // --- [IDENTITY]
    public string HelpUrl => Conversions.Unset(HelpTopic.Of(Row.Help, Owner).Map(static string (topic) => topic));

    // --- [EDGES]
    public void OnVisibilityChanged(bool visible) => _ = Owner.Run(Shown(visible));

    // --- [RELEASE]
    public void Destroy() => Dispose();
}

using System.Globalization;
using Eto.Forms;
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
using NativeDataSourceEventArgs = Rhino.UI.Controls.DataSource.EventArgs;

namespace Rasm.Rhino.UI;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class ViewHost : Panel {
    internal Retained<IDisposable> Owned { get; } = new(DisposalOps.Release);

    internal IO<Unit> Seat(View row, Control body) =>
        (row.Children.Exists(static child => child.Fill)
            ? IO.pure(body)
            : Containers.Frame(static () => new RhinoScrollableDialogPanel(), Owned.Retain).Map(scroll => {
                scroll.Content = body;
                return (Control)scroll;
            }))
        .Bind(seated => IO.lift(() => { Content = seated; }));

    protected override void Dispose(bool disposing) {
        try { if (disposing) Owned.Dispose(); } finally { base.Dispose(disposing); }
    }
}

public abstract class DefinedWindow(View.Window row, IPlugInViews owner) : Form(row.Kind.Handler()) {
    // --- [STATE]
    internal Retained<IDisposable> Owned { get; } = new(DisposalOps.Release);

    // --- [IDENTITY]
    internal View.Window Row => row;

    internal IPlugInViews Views => owner;

    // --- [EDGES]
    protected override void OnClosed(System.EventArgs e) =>
        Callbacks.Run(Callbacks.Each(Seq(IO.lift(() => base.OnClosed(e)), IO.lift(Dispose))).Uninterruptible(), CallbackSite.Of(owner, this));

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        try { if (disposing) Owned.Dispose(); } finally { base.Dispose(disposing); }
    }
}

public abstract class ViewPanel : Panel {
    // --- [STATE]
    private readonly Retained<IDisposable> owned = new(DisposalOps.Release);

    // --- [REALIZATION]
    private protected ViewPanel(View row, Option<RhinoDoc> document, IPlugInViews owner) {
        (Row, Owner) = (row, owner);
        Callbacks.Run(from body in DisposalOps.Acquire(ViewBody.Open(owner, row, document, new CommitMode.Immediate(), None), owned.Retain)
                      from configured in IO.lift(() => { Realized = Some(body); body.Content.UseRhinoStyle(); Content = body.Content; })
                      select unit, CallbackSite.Of(owner, this));
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
        try { if (disposing) owned.Dispose(); } finally { base.Dispose(disposing); }
    }
}

public abstract class DefinedPanel(RhinoDoc? document, View.Panel row) : ViewPanel(row, Optional(document), HostedTypes.Owner(row.Identity)), IPanel, IHelp {
    // --- [IDENTITY]
    public string HelpUrl => Conversions.Unset(HelpTopic.Of(Row.Help, Owner).Map(static string (topic) => topic));

    // --- [EDGES]
    public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) => Callbacks.Run(Shown(visible: true), CallbackSite.Of(Owner, this));

    public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) => Callbacks.Run(Shown(visible: false), CallbackSite.Of(Owner, this));

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) => Callbacks.Run(Shown(visible: false), CallbackSite.Of(Owner, this));
}

public abstract class DefinedSection : EtoCollapsibleSection3 {
    // --- [STATE]
    private readonly View.Section row;
    private readonly IPlugInViews owner;
    private readonly SectionLifetime lifetime;
    private Option<Func<Action<IDisposable>, IO<ViewBody>>> source;

    protected DefinedSection(View.Section row, IPlugInViews owner) {
        (this.row, this.owner) = (row, owner);
        lifetime = new SectionLifetime(this, this.owner, None, static () => false, visible: false,
            retain => IO.lift(() => source.ToFin(new Refused(nameof(Attach), Refused.Absent))).Bind(realize => realize(retain)));
    }

    // --- [IDENTITY]
    public override LocalizeStringPair Caption => RowText.Localize(row.Caption, owner);

    public override Guid PlugInId => owner.PlugIn.Id;

    public override int SectionHeight => lifetime.Height;

    public override bool Hidden => lifetime.Hidden;

    public override IHeaderButtonHandler NewHeaderButtonHandler() => lifetime.Header(row).IfNone(base.NewHeaderButtonHandler);

    // --- [REALIZATION]
    public IO<Unit> Attach(Func<Action<IDisposable>, IO<ViewBody>> realize) => IO.lift(() => { source = Some(realize); });

    // --- [EDGES]
    internal Option<SectionHolder> Holder { get; private set; }

    public override void OnAttachedToHolder(ICollapsibleSectionHolder2 holder) {
        base.OnAttachedToHolder(holder);
        Holder = Optional(holder as SectionHolder);
    }

    public override void HolderVisible(bool visible) => Callbacks.Run(lifetime.Visible(visible), CallbackSite.Of(owner, this));

    public override void UpdateView(uint flags) => Callbacks.Run(lifetime.Reread, CallbackSite.Of(owner, this));

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing)
                lifetime.Dispose();
        } finally {
            base.Dispose(disposing);
        }
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
    public override void UpdateView(uint flags) => Callbacks.Run(lifetime.Reread, CallbackSite.Of(owner, this));

    // --- [REALIZATION]
    private IO<ViewBody> Realized(Action<IDisposable> retain) =>
        from rows in RowScope.Open(owner, None, new CommitMode.Contents(this))
        from held in ViewBody.Realize(owner, row, rows, retain)
        from fields in FieldChanges(rows, retain)
        from selections in SelectionChanges(held, retain)
        select held;

    private IO<Unit> FieldChanges(RowScope rows, Action<IDisposable> retain) =>
        EventKind.ContentFieldChanged
            .Choose(static args => Some((ViewRegistration.FieldKey(args.Content, args.FieldName), args.Content.Id)))
            .Through(Subscriptions.Idle<EntryKey, Guid>(changed => IO.lift(() => toHashSet(Selected.Map(static content => content.Id)))
                .Bind(selected => rows.Changed(toSeq(changed.Filter(selected.Contains).Keys)))), owner, retain);

    private IO<Unit> SelectionChanges(ViewBody held, Action<IDisposable> retain) =>
        Subscriptions.Attach(
            handler => DataChanged += handler,
            handler => DataChanged -= handler,
            Callbacks.Handler<NativeDataSourceEventArgs>(args => when(args.DataType == ProviderIds.ContentSelection, held.Reread).As(), CallbackSite.Of(owner, this, nameof(DataChanged))), retain);

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        try {
            if (disposing)
                lifetime.Dispose();
        } finally {
            base.Dispose(disposing);
        }
    }
}

public abstract class DefinedOptionsPage : OptionsDialogPage {
    // --- [STATE]
    private readonly View row;
    private readonly CommitMode commit;
    private readonly IPlugInViews owner;
    private readonly Atom<Option<RhinoDoc>> document = Atom(Option<RhinoDoc>.None);
    private readonly Atom<Option<(ViewBody Body, Control Frame)>> page = Atom(Option<(ViewBody Body, Control Frame)>.None);

    protected DefinedOptionsPage(View.OptionsPage row, IPlugInViews owner) : this(row, row.Commit, row.Navigation, owner) { }

    protected DefinedOptionsPage(View.DocumentPage row, IPlugInViews owner) : this(row, row.Commit, row.Navigation, owner) { }

    private DefinedOptionsPage(View row, CommitMode commit, Option<NavigationText> navigation, IPlugInViews owner) : base(row.Caption) {
        (this.row, this.commit, this.owner) = (row, commit, owner);
        _ = navigation.Iter(text => (NavigationTextIsBold, NavigationTextColor) = (text.Bold, Conversions.Unset(text.Color)));
    }

    // --- [IDENTITY]
    public override string LocalPageTitle => RowText.Localize(row.Caption, owner).Local;

    public override System.Drawing.Image? PageImage =>
        Callbacks.Answer(
            new System.Drawing.Size(128, 128) switch {
                var size => row.Icon.Match(
                    Some: glyph => Icons.Raster(glyph, size).Map(static System.Drawing.Image? (bitmap) => bitmap),
                    None: () => IO.lift(System.Drawing.Image? () => owner.PlugIn.Icon(size))),
            },
            static () => null, CallbackSite.Of(owner, this));

    public override object? PageControl => Realized.Map(static object (held) => held.Frame).ValueUnsafe();

    public override bool ShowApplyButton => commit.Drafts;

    public override bool ShowDefaultsButton => row.Store.IsSome;

    public IO<Unit> ForDocument(RhinoDoc held) => document.SwapIO(_ => Some(held)).Map(unit);

    public IO<Unit> Navigate(PageTarget target) => Showing.SetActivePageTo(this, target);

    // --- [EDGES]
    public override bool OnActivate(bool active) =>
        Callbacks.Succeeded((active ? Realized : page.Value).Traverse(held => held.Body.Shown(active)).As(), CallbackSite.Of(owner, this));

    public override void OnHelp() => Callbacks.Run(HelpTopic.Of(row.Help, owner).Map(static topic => topic.Show), CallbackSite.Of(owner, this));

    // --- [COMMITS]
    public override bool OnApply() =>
        Callbacks.Succeeded(page.Value.Traverse(static held => held.Body.Scope.Apply(held.Body.Group)).As(), CallbackSite.Of(owner, this));

    public override void OnCancel() =>
        Callbacks.Run(page.Value.Map(static held => held.Body.Scope.Revert(held.Body.Group)), CallbackSite.Of(owner, this));

    public override void OnDefaults() =>
        Callbacks.Run(page.Value.Map(static held => held.Body.Scope.Commit(held.Body.Group, held.Body.Group.Defaults).Map(unit)), CallbackSite.Of(owner, this));

    // --- [SCRIPTS]
    public override Result RunScript(RhinoDoc doc, RunMode mode) =>
        Callbacks.Answer(
            Conversions.ToResult(RowScope.Open(owner, Some(doc), new CommitMode.Immediate())
                .Bind(scope => Showing.RunScript(owner, row, scope, doc))),
            static () => Result.Failure,
            CallbackSite.Of(owner, this));

    // --- [REALIZATION]
    private Option<(ViewBody Body, Control Frame)> Realized =>
        page.Value || Callbacks.Answer(
            from held in document.ValueIO
            from frame in IO.lift(static () => new ViewHost())
            from realized in DisposalOps.OnFailure(
                from body in DisposalOps.Acquire(ViewBody.Open(owner, row, held, commit, Some(Marking)), frame.Owned.Retain)
                from seated in frame.Seat(row, body.Content)
                from configured in IO.lift(frame.UseRhinoStyle)
                from published in page.SwapIO(_ => Some((Body: body, Frame: (Control)frame)))
                select published,
                IO.lift(frame.Dispose))
            select realized,
            static () => Option<(ViewBody Body, Control Frame)>.None, CallbackSite.Of(owner, this, nameof(PageControl)));

    private IO<Unit> Marking(ViewBody body, Action<IDisposable> retain) =>
        from listened in body.Scope.Listen(_ => Marked(body), retain)
        from marked in Marked(body)
        select unit;

    private IO<Unit> Marked(ViewBody body) =>
        body.Scope.Pending.Bind(pending => IO.lift(() => { Modified = pending; }));
}

public abstract class DefinedPropertiesPage(View.PropertiesPage row, IPlugInViews owner) : ObjectPropertiesPage {
    // --- [STATE]
    private readonly Atom<Option<(ViewBody Body, Control Host)>> page = Atom(Option<(ViewBody Body, Control Host)>.None);

    // --- [IDENTITY]
    public override string EnglishPageTitle => row.Caption;

    public override string LocalPageTitle => RowText.Localize(row.Caption, owner).Local;

    public override PropertyPageType PageType => row.Subject.Switch(() => base.PageType, objects: static (fallback, held) => held.PageType.IfNone(fallback), viewport: static (_, _) => PropertyPageType.View);

    public override ObjectType SupportedTypes => ObjectSubject.Bind(static held => held.SupportedTypes).IfNone(() => base.SupportedTypes);

    public override bool AllObjectsMustBeSupported => ObjectSubject.Bind(static held => held.AllObjectsMustBeSupported).IfNone(() => base.AllObjectsMustBeSupported);

    public override bool SupportsSubObjects => ObjectSubject.Bind(static held => held.SupportsSubObjects).IfNone(() => base.SupportsSubObjects);

    public override int Index => row.Index.IfNone(() => base.Index);

    public override string? PageIconEmbeddedResourceString =>
        row.Icon.Bind(glyph => glyph.Switch(
            GetType().Assembly,
            tinted: static (_, _) => Option<string>.None,
            paired: static (own, art) => art.Assembly == own ? Some(art.Light) : None,
            picture: static (_, _) => Option<string>.None)).ValueUnsafe();

    public override System.Drawing.Icon? PageIcon(System.Drawing.Size sizeInPixels) =>
        Callbacks.Answer(row.Icon.TraverseM(glyph => Icons.DrawingIcon(glyph, sizeInPixels)).As(),
            static () => Option<System.Drawing.Icon>.None, CallbackSite.Of(owner, this)).ValueUnsafe();

    public override object? PageControl => page.Value.Map(static object (held) => held.Host).ValueUnsafe();

    private Option<PageSubject.Objects> ObjectSubject =>
        row.Subject.Switch(objects: static held => Some(held), viewport: static _ => Option<PageSubject.Objects>.None);

    // --- [EDGES]
    public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e) =>
        Callbacks.Answer(
            from included in IO.lift(() => row.Subject.Switch(
                (Args: e, Objects: (Func<ObjectPropertiesPageEventArgs, bool>)base.ShouldDisplay),
                objects: static (state, _) => state.Objects(state.Args),
                viewport: static (state, held) => Optional(state.Args.Viewport).Exists(port => held.ViewportTypes.Contains(port.ViewportType))))
            from shown in included
                ? RowScope.Open(owner, Optional(e.Document), new CommitMode.Targets(this)).Bind(scope => RowRules.Holds(row.Visible, scope))
                : IO.pure(value: false)
            select shown,
            static () => false,
            CallbackSite.Of(owner, this));

    public override void UpdatePage(ObjectPropertiesPageEventArgs e) =>
        Callbacks.Run(page.Value.Map(static held => held.Body.Reread), CallbackSite.Of(owner, this));

    public override bool OnActivate(bool active) =>
        Callbacks.Succeeded((active ? Realized : page.Value).Traverse(held => held.Body.Shown(active)).As(), CallbackSite.Of(owner, this));

    public override void OnHelp() => Callbacks.Run(HelpTopic.Of(row.Help, owner).Map(static topic => topic.Show), CallbackSite.Of(owner, this));

    // --- [SCRIPTS]
    public override Result RunScript(ObjectPropertiesPageEventArgs e) =>
        Callbacks.Answer(
            Conversions.ToResult(
                from document in IO.lift(() => Refused.UnlessFound(e.Document, nameof(ObjectPropertiesPageEventArgs.Document)))
                from scope in RowScope.Open(owner, Some(document), new CommitMode.Targets(this))
                from scripted in Showing.RunScript(owner, row, scope, document)
                select scripted),
            static () => Result.Failure,
            CallbackSite.Of(owner, this));

    // --- [REALIZATION]
    private Option<(ViewBody Body, Control Host)> Realized =>
        page.Value || Callbacks.Answer(
            from document in IO.lift(() => Optional(new ObjectPropertiesPageEventArgs(this).Document))
            from host in IO.lift(static () => new ViewHost())
            from realized in DisposalOps.OnFailure(
                from body in DisposalOps.Acquire(ViewBody.Open(owner, row, document, new CommitMode.Targets(this), Refreshing), host.Owned.Retain)
                from configured in IO.lift(() => host.Content = body.Content)
                from published in page.SwapIO(_ => Some((Body: body, Host: (Control)host)))
                select published,
                IO.lift(host.Dispose))
            select realized,
            static () => Option<(ViewBody Body, Control Host)>.None, CallbackSite.Of(owner, this, nameof(OnActivate)));

    private Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>> Refreshing =>
        row.Subject.Switch(
            this,
            objects: static (owner, _) => Some<Func<ViewBody, Action<IDisposable>, IO<Unit>>>((body, retain) =>
                body.Scope.Document.ToSeq()
                    .Bind(static document => Seq(
                        EventKind.ModifyObjectAttributes.In(document.RuntimeSerialNumber).Choose(static args => Some((args.RhinoObject.Id, unit))),
                        EventKind.ReplaceRhinoObject.In(document.RuntimeSerialNumber).Choose(static args => Some((args.ObjectId, unit)))))
                    .TraverseM(signal => signal.Through(Subscriptions.Idle<Guid, Unit>(changed =>
                        IO.lift(() => toHashSet(Conversions.Rows(new ObjectPropertiesPageEventArgs(owner).Objects).Map(static target => target.Id)))
                            .Bind(selected => when(changed.Keys.Exists(selected.Contains), body.Reread).As())), body.Scope.Sink, retain)).As().Map(unit)),
            viewport: static (_, _) => Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>>.None);
}

public abstract class DefinedTab(View.Tab row) : ViewPanel(row, Optional(RhinoDoc.ActiveDoc), HostedTypes.Owner(row.Identity)) {
    // --- [EDGES]
    public void OnVisibilityChanged(bool visible) => Callbacks.Run(Shown(visible), CallbackSite.Of(Owner, this));

    public void TabActivated(uint state) => Callbacks.Run(Shown(state != 0), CallbackSite.Of(Owner, this));

    public void DisplayData() => Callbacks.Run(Realized.Map(static body => body.Reread), CallbackSite.Of(Owner, this));

    public void DoHelp() => Callbacks.Run(HelpTopic.Of(Row.Help, Owner).Map(static topic => topic.Show), CallbackSite.Of(Owner, this));

    // --- [RELEASE]
    public void Destroy() => Dispose();
}

public abstract class DefinedPane(View.Pane row) : ViewPanel(row, Optional(RhinoDoc.ActiveDoc), HostedTypes.Owner(row.Identity)), IHelp {
    // --- [IDENTITY]
    public string HelpUrl => Conversions.Unset(HelpTopic.Of(Row.Help, Owner).Map(static string (topic) => topic));

    // --- [EDGES]
    public void OnVisibilityChanged(bool visible) => Callbacks.Run(Shown(visible), CallbackSite.Of(Owner, this));

    // --- [RELEASE]
    public void Destroy() => Dispose();
}

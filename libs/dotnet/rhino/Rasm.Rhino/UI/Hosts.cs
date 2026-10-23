using System.Globalization;
using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
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

    internal void Seat(View row, Control body) =>
        Content = row.Children.Exists(static child => child.Fill) ? body : Scrolled(body);

    private RhinoScrollableDialogPanel Scrolled(Control body) {
        RhinoScrollableDialogPanel scroll = new() { Content = body };
        Owned.Retain(scroll);
        return scroll;
    }

    protected override void Dispose(bool disposing) {
        try { if (disposing) Owned.Dispose(); } finally { base.Dispose(disposing); }
    }
}

public abstract class DefinedWindow(View.Window row) : Form(row.Kind.Handler()) {
    // --- [STATE]
    internal Retained<IDisposable> Owned { get; } = new(DisposalOps.Release);

    // --- [IDENTITY]
    internal View.Window Row => row;

    // --- [EDGES]
    protected override void OnClosed(System.EventArgs e) =>
        Callbacks.Run(Callbacks.Each(Seq(IO.lift(() => base.OnClosed(e)), IO.lift(Dispose))).Uninterruptible(), CallbackSite.Of(this));

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        try { if (disposing) Owned.Dispose(); } finally { base.Dispose(disposing); }
    }
}

public abstract class ViewPanel : Panel {
    // --- [STATE]
    private readonly Retained<IDisposable> owned = new(DisposalOps.Release);

    // --- [REALIZATION]
    private protected ViewPanel(View row, Option<RhinoDoc> document) {
        Row = row;
        Callbacks.Run(from body in DisposalOps.Acquire(ViewBody.Open(IPlugInViews.Of(this), row, document, new CommitMode.Immediate(), None), owned.Retain)
                      from configured in IO.lift(() => { Realized = Some(body); body.Content.UseRhinoStyle(); Content = body.Content; })
                      select unit, CallbackSite.Of(this));
    }

    // --- [IDENTITY]
    private protected View Row { get; }

    private protected Option<ViewBody> Realized { get; private set; }

    // --- [EDGES]
    private protected new IO<Unit> Shown(bool visible) => Realized.Traverse(body => body.Shown(visible)).As().Map(unit);

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        Realized = None;
        try { if (disposing) owned.Dispose(); } finally { base.Dispose(disposing); }
    }
}

public abstract class DefinedPanel(RhinoDoc? document, View.Panel row) : ViewPanel(row, Optional(document)), IPanel, IHelp {
    // --- [IDENTITY]
    public string HelpUrl => Conversions.Unset(HelpTopic.Of(Row.Help, this).Map(static string (topic) => topic));

    // --- [EDGES]
    public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) => Callbacks.Run(Shown(visible: true), CallbackSite.Of(this));

    public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) => Callbacks.Run(Shown(visible: false), CallbackSite.Of(this));

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) => Callbacks.Run(Shown(visible: false), CallbackSite.Of(this));
}

public abstract class DefinedSection : EtoCollapsibleSection3 {
    // --- [STATE]
    private readonly View.Section row;
    private readonly IPlugInViews owner;
    private readonly SectionLifetime lifetime;
    private Option<Func<Action<IDisposable>, IO<ViewBody>>> source;

    protected DefinedSection(View.Section row) : this(row, None) { }

    private protected DefinedSection(View.Section row, Option<IPlugInViews> owner) {
        (this.row, this.owner) = (row, owner.IfNone(() => IPlugInViews.Of(this)));
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

    public override void HolderVisible(bool visible) => Callbacks.Run(lifetime.Visible(visible), new CallbackSite(owner, GetType(), nameof(HolderVisible)));

    public override void UpdateView(uint flags) => Callbacks.Run(lifetime.Reread, new CallbackSite(owner, GetType(), nameof(UpdateView)));

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

internal sealed class DisclosureSection(View.Section row, IPlugInViews owner, Guid id, bool expanded) : DefinedSection(row, Some(owner)), ICollapsibleSection {
    // --- [IDENTITY]
    Guid ICollapsibleSection.Id => id;

    public override string SettingsTag => id.ToString("D", CultureInfo.InvariantCulture);

    public override bool InitiallyExpanded => expanded;
}

public abstract class DefinedContentSection : EtoContentUISection3 {
    // --- [STATE]
    private readonly View.Section row;
    private readonly ContentScope scope;
    private readonly SectionLifetime lifetime;

    protected DefinedContentSection(View.Section row, ContentScope scope) {
        IPlugInViews owner = IPlugInViews.Of(this);
        (this.row, this.scope) = (row, scope);
        lifetime = new SectionLifetime(this, owner, Some(ProviderIds.ContentSelection), () => Excluded, visible: true, retain => Realized(owner, retain));
    }

    // --- [IDENTITY]
    public override LocalizeStringPair Caption => RowText.Localize(row.Caption, IPlugInSink.Of(this));

    public override Guid PlugInId => IPlugInViews.Of(this).PlugIn.Id;

    public override int SectionHeight => lifetime.Height;

    public override bool Hidden => Excluded || lifetime.Hidden;

    public override IHeaderButtonHandler NewHeaderButtonHandler() => lifetime.Header(row).IfNone(base.NewHeaderButtonHandler);

    private bool Excluded => base.Hidden || !Selected.Head.ForAll(scope.Covers);

    private Seq<RenderContent> Selected => toSeq(GetSelection()).Strict();

    // --- [EDGES]
    public override void UpdateView(uint flags) => Callbacks.Run(lifetime.Reread, CallbackSite.Of(this));

    // --- [REALIZATION]
    private IO<ViewBody> Realized(IPlugInViews owner, Action<IDisposable> retain) =>
        from rows in RowScope.Open(owner, None, new CommitMode.Contents(this))
        from held in ViewBody.Realize(owner, row, rows, retain)
        from fields in FieldChanges(rows, owner, retain)
        from selections in SelectionChanges(held, owner, retain)
        select held;

    private IO<Unit> FieldChanges(RowScope rows, IPlugInSink sink, Action<IDisposable> retain) =>
        EventKind.ContentFieldChanged
            .Choose(static args => Some((ViewRegistration.FieldKey(args.Content, args.FieldName), args.Content.Id)))
            .Through(Subscriptions.Idle<EntryKey, Guid>(changed => IO.lift(() => toHashSet(Selected.Map(static content => content.Id)))
                .Bind(selected => rows.Changed(toSeq(changed.Filter(selected.Contains).Keys)))), sink, retain);

    private IO<Unit> SelectionChanges(ViewBody held, IPlugInSink sink, Action<IDisposable> retain) =>
        Subscriptions.Attach(
            handler => DataChanged += handler,
            handler => DataChanged -= handler,
            Callbacks.Handler<NativeDataSourceEventArgs>(args => when(args.DataType == ProviderIds.ContentSelection, held.Reread).As(), new CallbackSite(sink, GetType(), nameof(DataChanged))), retain);

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
    private readonly Atom<Option<RhinoDoc>> document = Atom(Option<RhinoDoc>.None);
    private readonly Atom<Option<(ViewBody Body, Control Frame)>> page = Atom(Option<(ViewBody Body, Control Frame)>.None);

    protected DefinedOptionsPage(View.OptionsPage row) : this(row, row.Commit, row.Navigation) { }

    protected DefinedOptionsPage(View.DocumentPage row) : this(row, row.Commit, row.Navigation) { }

    private DefinedOptionsPage(View row, CommitMode commit, Option<NavigationText> navigation) : base(row.Caption) {
        (this.row, this.commit) = (row, commit);
        _ = navigation.Iter(text => (NavigationTextIsBold, NavigationTextColor) = (text.Bold, Conversions.Unset(text.Color)));
    }

    // --- [IDENTITY]
    public override string LocalPageTitle => RowText.Localize(row.Caption, IPlugInSink.Of(this)).Local;

    public override System.Drawing.Image? PageImage =>
        Callbacks.Answer(IO.lift(System.Drawing.Image? () => IPlugInViews.Of(this).PlugIn.Icon(new(128, 128))), static () => null, CallbackSite.Of(this));

    public override object? PageControl => Realized.Map(static object (held) => held.Frame).ValueUnsafe();

    public override bool ShowApplyButton => commit.Drafts;

    public override bool ShowDefaultsButton => row.Store.IsSome;

    public IO<Unit> ForDocument(RhinoDoc held) => document.SwapIO(_ => Some(held)).Map(unit);

    public IO<Unit> Navigate(PageTarget target) => Showing.SetActivePageTo(this, target);

    // --- [EDGES]
    public override bool OnActivate(bool active) =>
        Callbacks.Succeeded((active ? Realized : page.Value).Traverse(held => held.Body.Shown(active)).As(), CallbackSite.Of(this));

    public override void OnHelp() => Callbacks.Run(HelpTopic.Of(row.Help, this).Map(static topic => topic.Show), CallbackSite.Of(this));

    // --- [COMMITS]
    public override bool OnApply() =>
        Callbacks.Succeeded(page.Value.Traverse(static held => held.Body.Scope.Apply(held.Body.Group)).As(), CallbackSite.Of(this));

    public override void OnCancel() =>
        Callbacks.Run(page.Value.Map(static held => held.Body.Scope.Revert(held.Body.Group)), CallbackSite.Of(this));

    public override void OnDefaults() =>
        Callbacks.Run(page.Value.Map(static held => held.Body.Scope.Commit(held.Body.Group, held.Body.Group.Defaults).Map(unit)), CallbackSite.Of(this));

    // --- [SCRIPTS]
    public override Result RunScript(RhinoDoc doc, RunMode mode) =>
        Callbacks.Answer(
            Conversions.ToResult(RowScope.Open(IPlugInSink.Of(this), Some(doc), new CommitMode.Immediate())
                .Bind(scope => Showing.RunScript(IPlugInViews.Of(this), row, scope, doc))),
            static () => Result.Failure,
            CallbackSite.Of(this));

    // --- [REALIZATION]
    private Option<(ViewBody Body, Control Frame)> Realized =>
        page.Value || Callbacks.Answer(
            from held in document.ValueIO
            from frame in IO.lift(static () => new ViewHost())
            from realized in DisposalOps.OnFailure(
                from body in DisposalOps.Acquire(ViewBody.Open(IPlugInViews.Of(this), row, held, commit, Some(Marking)), frame.Owned.Retain)
                from configured in IO.lift(() => { frame.Seat(row, body.Content); frame.UseRhinoStyle(); })
                from published in page.SwapIO(_ => Some((Body: body, Frame: (Control)frame)))
                select published,
                IO.lift(frame.Dispose))
            select realized,
            static () => Option<(ViewBody Body, Control Frame)>.None, CallbackSite.Of(this, nameof(PageControl)));

    private IO<Unit> Marking(ViewBody body, Action<IDisposable> retain) =>
        from listened in body.Scope.Listen(_ => Marked(body), retain)
        from marked in Marked(body)
        select unit;

    private IO<Unit> Marked(ViewBody body) =>
        body.Scope.Pending.Bind(pending => IO.lift(() => { Modified = pending; }));
}

public abstract class DefinedPropertiesPage(View.PropertiesPage row) : ObjectPropertiesPage {
    // --- [STATE]
    private readonly Atom<Option<(ViewBody Body, Control Host)>> page = Atom(Option<(ViewBody Body, Control Host)>.None);

    // --- [IDENTITY]
    public override string EnglishPageTitle => row.Caption;

    public override string LocalPageTitle => RowText.Localize(row.Caption, IPlugInSink.Of(this)).Local;

    public override PropertyPageType PageType => row.PageType.IfNone(() => base.PageType);

    public override ObjectType SupportedTypes => row.SupportedTypes.IfNone(() => base.SupportedTypes);

    public override bool AllObjectsMustBeSupported => row.AllObjectsMustBeSupported.IfNone(() => base.AllObjectsMustBeSupported);

    public override bool SupportsSubObjects => row.SupportsSubObjects.IfNone(() => base.SupportsSubObjects);

    public override int Index => row.Index.IfNone(() => base.Index);

    public override string PageIconEmbeddedResourceString => Conversions.Unset(row.Icon.Map(static glyph => glyph.Key));

    public override System.Drawing.Icon? PageIcon(System.Drawing.Size sizeInPixels) =>
        Callbacks.Answer(row.Icon.TraverseM(glyph => Icons.DrawingIcon(glyph, sizeInPixels)).As(),
            static () => Option<System.Drawing.Icon>.None, CallbackSite.Of(this)).ValueUnsafe();

    public override object? PageControl => Realized.Map(static object (held) => held.Host).ValueUnsafe();

    private bool ViewPage => row.PageType.Exists(static type => type is PropertyPageType.View);

    // --- [EDGES]
    public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e) =>
        Callbacks.Answer(
            from included in IO.lift(() => ViewPage ? e.View is not null : base.ShouldDisplay(e))
            from shown in included && row.Visible.IsSome ? Realized.Match(Some: held => RowRules.Holds(row.Visible, held.Body.Scope), None: static () => IO.pure(value: false)) : IO.pure(included)
            select shown,
            static () => false,
            CallbackSite.Of(this));

    public override void UpdatePage(ObjectPropertiesPageEventArgs e) =>
        Callbacks.Run(page.Value.Map(static held => held.Body.Reread), CallbackSite.Of(this));

    public override bool OnActivate(bool active) =>
        Callbacks.Succeeded((active ? Realized : page.Value).Traverse(held => held.Body.Shown(active)).As(), CallbackSite.Of(this));

    public override void OnHelp() => Callbacks.Run(HelpTopic.Of(row.Help, this).Map(static topic => topic.Show), CallbackSite.Of(this));

    // --- [SCRIPTS]
    public override Result RunScript(ObjectPropertiesPageEventArgs e) =>
        Callbacks.Answer(
            Conversions.ToResult(
                from held in IO.lift(() => Realized.ToFin(new Refused(nameof(PageControl), Refused.Absent)))
                from document in IO.lift(() => Refused.UnlessFound(e.Document, nameof(ObjectPropertiesPageEventArgs.Document)))
                from scripted in Showing.RunScript(IPlugInViews.Of(this), row, held.Body.Scope, document)
                select scripted),
            static () => Result.Failure,
            CallbackSite.Of(this));

    // --- [REALIZATION]
    private Option<(ViewBody Body, Control Host)> Realized =>
        page.Value || Callbacks.Answer(
            from document in IO.lift(() => Optional(new ObjectPropertiesPageEventArgs(this).Document))
            from host in IO.lift(static () => new ViewHost())
            from realized in DisposalOps.OnFailure(
                from body in DisposalOps.Acquire(ViewBody.Open(IPlugInViews.Of(this), row, document, new CommitMode.Targets(this), Refreshing), host.Owned.Retain)
                from configured in IO.lift(() => host.Content = body.Content)
                from published in page.SwapIO(_ => Some((Body: body, Host: (Control)host)))
                select published,
                IO.lift(host.Dispose))
            select realized,
            static () => Option<(ViewBody Body, Control Host)>.None, CallbackSite.Of(this, nameof(PageControl)));

    private Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>> Refreshing =>
        ViewPage
            ? Option<Func<ViewBody, Action<IDisposable>, IO<Unit>>>.None
            : Some<Func<ViewBody, Action<IDisposable>, IO<Unit>>>(static (body, retain) =>
                body.Scope.Document.ToSeq()
                    .Bind(static document => Seq(
                        EventKind.ModifyObjectAttributes.In(document.RuntimeSerialNumber).Choose(static _ => Some((unit, unit))),
                        EventKind.ReplaceRhinoObject.In(document.RuntimeSerialNumber).Choose(static _ => Some((unit, unit)))))
                    .TraverseM(signal => signal.Through(Subscriptions.Idle<Unit, Unit>(_ => body.Reread), body.Scope.Sink, retain)).As().Map(unit));
}

public abstract class DefinedTab(View.Tab row) : ViewPanel(row, Optional(RhinoDoc.ActiveDoc)) {
    // --- [EDGES]
    public void OnVisibilityChanged(bool visible) => Callbacks.Run(Shown(visible), CallbackSite.Of(this));

    public void TabActivated(uint state) => Callbacks.Run(Shown(state != 0), CallbackSite.Of(this));

    public void DisplayData() => Callbacks.Run(Realized.Map(static body => body.Reread), CallbackSite.Of(this));

    public void DoHelp() => Callbacks.Run(HelpTopic.Of(Row.Help, this).Map(static topic => topic.Show), CallbackSite.Of(this));

    // --- [RELEASE]
    public void Destroy() => Dispose();
}

public abstract class DefinedPane(View.Pane row) : ViewPanel(row, Optional(RhinoDoc.ActiveDoc)), IHelp {
    // --- [IDENTITY]
    public string HelpUrl => Conversions.Unset(HelpTopic.Of(Row.Help, this).Map(static string (topic) => topic));

    // --- [EDGES]
    public void OnVisibilityChanged(bool visible) => Callbacks.Run(Shown(visible), CallbackSite.Of(this));

    // --- [RELEASE]
    public void Destroy() => Dispose();
}

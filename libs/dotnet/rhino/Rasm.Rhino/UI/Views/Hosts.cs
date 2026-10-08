using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;
using DataSourceEventArgs = Rhino.UI.Controls.DataSource.EventArgs;
using ProviderIds = Rhino.UI.Controls.DataSource.ProviderIds;

namespace Rasm.Rhino.UI.Views;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class RealizedView : Panel {
    // --- [STATE]
    private readonly View row;
    private readonly RowRules rules;
    private readonly LanguageExt.HashSet<EntryKey> reads;
    private readonly CallbackSite site;
    private readonly Disposal<IO<Unit>> released;
    private readonly ShownSubscription listeners;

    private RealizedView(IPlugInViews owner, View row, RowScope scope, BindingGroup group, ViewBody body, Option<Func<RealizedView, IO<IDisposable>>> listening) {
        (this.row, Scope, Group) = (row, scope, group);
        rules = new RowRules(row.Enabled, row.Visible);
        reads = toHashSet(rules.Each.Bind(static rule => rule.Reads));
        site = new CallbackSite(owner, row.Identity, nameof(Shown));
        released = new Disposal<IO<Unit>>(body.Release, run => _ = Callbacks.Answer(run, static () => unit, new CallbackSite(owner, row.Identity, nameof(Dispose))));
        listeners = new ShownSubscription(Listening(listening), site);
        Content = body.Content;
    }

    public RowScope Scope { get; }
    public BindingGroup Group { get; }

    // --- [REALIZATION]
    public static IO<RealizedView> Open(IPlugInViews owner, View row, Option<RhinoDoc> document, CommitMode mode, Option<Func<RealizedView, IO<IDisposable>>> listening) =>
        from bare in RowScope.Open(owner, document, mode)
        from bindings in IO.lift(ViewOps.Bindings(owner, row, bare).ToFin())
        from history in IO.lift(row.Store.Traverse(store => ViewOps.History(owner, row, store, bindings, document)).As())
        let scope = bare.Nested(ViewOps.Settings(owner, row), history, ViewOps.Locks(row, document))
        from opened in scope.Opened(bindings)
        from body in ViewOps.Realize(owner, row.Children, scope)
        from view in DisposalOps.OnFailure(IO.lift(() => new RealizedView(owner, row, scope, bindings, body, listening)), body.Release)
        from evaluated in DisposalOps.OnFailure(view.Evaluated, IO.lift(view.Dispose))
        select view;

    // --- [EDGES]
    public Unit Shown(bool visible) {
        _ = SectionStack.Shown(this, visible);
        _ = listeners.Shown(visible);
        return visible ? Callbacks.Answer(Scope.Changed(Keys(row)).Bind(_ => Evaluated), static () => unit, site) : unit;
    }

    public IO<Unit> Reread => Scope.Changed(ViewCollection.Tree(row).Bind(Keys)).Bind(_ => Evaluated);

    // --- [RULES]
    public IO<bool> Shows => rules.Shows(Scope);

    private IO<Unit> Evaluated =>
        from shows in rules.Shows(Scope)
        from enables in rules.Enables(Scope)
        from set in IO.lift(() => { (Visible, Enabled) = (shows, enables); })
        select set;

    private IO<IDisposable> Listening(Option<Func<RealizedView, IO<IDisposable>>> listening) =>
        DisposalOps.AcquireAll(Scope.Listen(keys => when(keys.Exists(reads.Contains), Evaluated).As()).Cons(listening.Map(listen => listen(this)).ToSeq()), DisposalOps.Release)
            .Map(held => DisposalOps.Composite(held, site));

    internal static Seq<EntryKey> Keys(View view) =>
        view.Children.Bind(static child => child.Controls).Bind(static control => control.Keys + control.Rules.Each.Bind(static rule => rule.Reads))
        + new RowRules(view.Enabled, view.Visible).Each.Bind(static rule => rule.Reads);

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) {
            listeners.Dispose();
            released.Dispose();
        }
        base.Dispose(disposing);
    }
}

public abstract class DefinedWindow(View.Window row) : Form(row.Kind.Handler()) {
    // --- [IDENTITY]
    internal View.Window Row => row;

    // --- [EDGES]
    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);
        _ = SectionStack.Shown(Content, visible: true);
    }

    protected override void OnUnLoad(EventArgs e) {
        _ = SectionStack.Shown(Content, visible: false);
        base.OnUnLoad(e);
    }
}

public abstract class ViewPanel : Panel {
    // --- [REALIZATION]
    private protected ViewPanel(View row, Option<RhinoDoc> document) {
        Row = row;
        Realized = Callbacks.Answer(
            RealizedView.Open((IPlugInViews)IPlugInSink.Of(this), row, document, new CommitMode.Immediate(), None).Map(Some),
            static () => Option<RealizedView>.None,
            CallbackSite.Of(this));
        _ = Realized.Iter(static view => view.UseRhinoStyle());
        Content = Realized.Map(static Control (view) => view).ValueUnsafe();
    }

    // --- [IDENTITY]
    private protected View Row { get; }

    private protected Option<RealizedView> Realized { get; }

    // --- [EDGES]
    private protected Unit Shown(bool visible) => Realized.Iter(view => view.Shown(visible));
}

public abstract class DefinedPanel(RhinoDoc? document, View.Panel row) : ViewPanel(row, Optional(document)), IPanel, IHelp {
    // --- [IDENTITY]
    public string HelpUrl => Row.HelpUrl;

    // --- [EDGES]
    public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) => _ = Shown(true);

    public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) => _ = Shown(false);

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) => _ = Shown(false);
}

public abstract class DefinedSection(View.Section row) : EtoCollapsibleSection3 {
    // --- [STATE]
    private readonly Atom<Option<SectionBody>> realized = Atom(Option<SectionBody>.None);

    // --- [IDENTITY]
    public override LocalizeStringPair Caption => RowText.Localize(row.Caption, table: Some<object>(IPlugInSink.Of(this)));

    public override Guid PlugInId => ((PlugIn)IPlugInSink.Of(this)).Id;

    public override int SectionHeight => realized.Value.Match(Some: static body => body.Height, None: static () => 0);

    public override bool Hidden => realized.Value.Exists(static body => body.Hidden);

    public override IHeaderButtonHandler NewHeaderButtonHandler() => realized.Value.Bind(static body => body.Header).IfNone(base.NewHeaderButtonHandler);

    // --- [REALIZATION]
    public IO<Unit> Attach(SectionBody body) =>
        IO.lift(() => {
            _ = realized.Swap(_ => Some(body));
            Content = body.Content;
        });

    // --- [EDGES]
    public override void HolderVisible(bool visible) => _ = realized.Value.Iter(body => body.Shown(visible));

    public override void UpdateView(uint flags) =>
        _ = Callbacks.Answer(realized.Value.Map(static body => body.Reread), static () => unit, static () => unit, CallbackSite.Of(this));

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing)
            _ = Callbacks.Answer(DisposalOps.Release(realized.Value.ToSeq()), static () => unit, CallbackSite.Of(this));
        base.Dispose(disposing);
    }
}

public abstract class DefinedContentSection : EtoContentUISection3 {
    // --- [STATE]
    private readonly View.Section row;
    private readonly ContentScope scope;
    private readonly Option<(RowScope Rows, SectionBody Body)> realized;
    private readonly ShownSubscription listeners;

    protected DefinedContentSection(View.Section row, ContentScope scope) {
        IPlugInViews owner = (IPlugInViews)IPlugInSink.Of(this);
        CallbackSite site = new(owner, GetType(), nameof(HolderVisible));
        (this.row, this.scope) = (row, scope);
        realized = Callbacks.Answer(
            from rows in RowScope.Open(owner, None, new CommitMode.Contents(this))
            from body in SectionBody.Realize(owner, row, rows)
            select Some((Rows: rows, Body: body)),
            static () => Option<(RowScope Rows, SectionBody Body)>.None,
            CallbackSite.Of(this));
        listeners = new ShownSubscription(
            DisposalOps.AcquireAll(realized.ToSeq().Bind(held => Seq(FieldChanges(held.Rows, owner), SelectionChanges(held.Body, owner))), DisposalOps.Release)
                .Map(attached => DisposalOps.Composite(attached, site)),
            site);
        Content = realized.Map(static Control (held) => held.Body.Content).ValueUnsafe();
    }

    // --- [IDENTITY]
    public override LocalizeStringPair Caption => RowText.Localize(row.Caption, table: Some<object>(IPlugInSink.Of(this)));

    public override Guid PlugInId => ((PlugIn)IPlugInSink.Of(this)).Id;

    public override int SectionHeight => realized.Match(Some: static held => held.Body.Height, None: static () => 0);

    public override bool Hidden => base.Hidden || realized.ForAll(held => held.Body.Hidden || !Selected.Head.ForAll(scope.Covers));

    public override IHeaderButtonHandler NewHeaderButtonHandler() => realized.Bind(static held => held.Body.Header).IfNone(base.NewHeaderButtonHandler);

    private Seq<RenderContent> Selected => toSeq<RenderContent>(GetSelection()).Strict();

    // --- [EDGES]
    public override void HolderVisible(bool visible) {
        _ = realized.Iter(held => held.Body.Shown(visible));
        _ = listeners.Shown(visible);
    }

    public override void UpdateView(uint flags) =>
        _ = Callbacks.Answer(realized.Map(static held => held.Body.Reread), static () => unit, static () => unit, CallbackSite.Of(this));

    private IO<IDisposable> FieldChanges(RowScope rows, IPlugInSink sink) =>
        EventKind.ContentFieldChanged
            .Choose(static args => Some((ViewRegistration.FieldKey(args.Content, args.FieldName), args.Content.Id)))
            .Through(Subscriptions.Idle<EntryKey, Guid>(changed => IO.lift(() => toHashSet(Selected.Map(static content => content.Id)))
                .Bind(selected => rows.Changed(toSeq(changed.Filter(selected.Contains).Keys)))), sink);

    private IO<IDisposable> SelectionChanges(SectionBody body, IPlugInSink sink) =>
        Subscriptions.Attach<EventHandler<DataSourceEventArgs>>(
            handler => DataChanged += handler,
            handler => DataChanged -= handler,
            Callbacks.Handler<DataSourceEventArgs>(args => when(args.DataType == ProviderIds.ContentSelection, body.Reread).As(), new CallbackSite(sink, GetType(), nameof(DataChanged))));

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) {
            listeners.Dispose();
            _ = realized.Iter(static held => held.Body.Dispose());
        }
        base.Dispose(disposing);
    }
}

public abstract class DefinedOptionsPage : OptionsDialogPage {
    // --- [STATE]
    private readonly View row;
    private readonly CommitMode commit;
    private readonly Atom<Option<RhinoDoc>> document = Atom(Option<RhinoDoc>.None);
    private readonly Atom<Option<(RealizedView View, Control Frame)>> page = Atom(Option<(RealizedView View, Control Frame)>.None);

    protected DefinedOptionsPage(View.OptionsPage row) : this(row, row.Commit, row.Navigation) { }

    protected DefinedOptionsPage(View.DocumentPage row) : this(row, row.Commit, row.Navigation) { }

    private DefinedOptionsPage(View row, CommitMode commit, Option<NavigationText> navigation) : base(row.Caption) {
        (this.row, this.commit) = (row, commit);
        _ = navigation.Iter(text => (NavigationTextIsBold, NavigationTextColor) = (text.Bold, Conversions.Unset(text.Color)));
    }

    // --- [IDENTITY]
    public override string LocalPageTitle => RowText.Localize(row.Caption, table: Some<object>(IPlugInSink.Of(this))).Local;

    public override System.Drawing.Image? PageImage =>
        Callbacks.Answer(Icons.PageImage(IPlugInSink.Of(this)).Map(static System.Drawing.Image? (bitmap) => bitmap), static () => null, CallbackSite.Of(this));

    public override object? PageControl => Realized.Map(static object (held) => held.Frame).ValueUnsafe();

    public override bool ShowApplyButton => commit.Map(immediate: false, revertible: false, deferred: true, targets: false, contents: false);

    public override bool ShowDefaultsButton => row.Store.IsSome;

    public IO<Unit> ForDocument(RhinoDoc held) => document.SwapIO(_ => Some(held)).Map(static _ => unit);

    public IO<Unit> Navigate(PageTarget target) => Showing.SetActivePageTo(this, target);

    // --- [EDGES]
    public override bool OnActivate(bool active) {
        _ = (active ? Realized : page.Value).Iter(held => held.View.Shown(active));
        return true;
    }

    public override void OnHelp() => _ = Callbacks.Answer(row.Help.Map(static topic => topic.Show), static () => unit, static () => unit, CallbackSite.Of(this));

    // --- [COMMITS]
    public override bool OnApply() {
        _ = Callbacks.Answer(page.Value.Map(static held => held.View.Scope.Apply(held.View.Group)), static () => unit, static () => unit, CallbackSite.Of(this));
        return true;
    }

    public override void OnCancel() =>
        _ = Callbacks.Answer(page.Value.Map(static held => held.View.Scope.Revert(held.View.Group)), static () => unit, static () => unit, CallbackSite.Of(this));

    public override void OnDefaults() =>
        _ = Callbacks.Answer(
            page.Value.Map(held => ViewOps.Defaults(row).Bind(defaults => held.View.Scope.Commit(held.View.Group, defaults)).Map(static _ => unit)),
            static () => unit,
            static () => unit,
            CallbackSite.Of(this));

    // --- [SCRIPTS]
    public override Result RunScript(RhinoDoc doc, RunMode mode) =>
        Callbacks.Answer(
            Conversions.ToResult(RowScope.Open(IPlugInSink.Of(this), Some(doc), new CommitMode.Immediate())
                .Bind(scope => Showing.RunScript((IPlugInViews)IPlugInSink.Of(this), row, scope, doc))),
            static () => Result.Failure,
            CallbackSite.Of(this));

    // --- [REALIZATION]
    private Option<(RealizedView View, Control Frame)> Realized =>
        page.Value || Callbacks.Answer(
            from held in document.ValueIO
            from view in RealizedView.Open((IPlugInViews)IPlugInSink.Of(this), row, held, commit, Some<Func<RealizedView, IO<IDisposable>>>(Marking))
            from frame in DisposalOps.OnFailure(
                IO.lift(() => {
                    RhinoScrollableDialogPanel framed = new() { Content = view };
                    framed.UseRhinoStyle();
                    return (Control)framed;
                }),
                IO.lift(view.Dispose))
            from opened in page.SwapIO(_ => Some((View: view, Frame: frame)))
            select opened,
            static () => Option<(RealizedView View, Control Frame)>.None,
            CallbackSite.Of(this, nameof(PageControl)));

    private IO<IDisposable> Marking(RealizedView view) =>
        from listened in view.Scope.Listen(_ => Marked(view))
        from marked in DisposalOps.OnFailure(Marked(view), IO.lift(listened.Dispose))
        select listened;

    private IO<Unit> Marked(RealizedView view) =>
        view.Scope.Pending.Bind(pending => IO.lift(() => { Modified = pending; }));
}

public abstract class DefinedPropertiesPage(View.PropertiesPage row) : ObjectPropertiesPage {
    // --- [STATE]
    private readonly Atom<Option<RealizedView>> page = Atom(Option<RealizedView>.None);

    // --- [IDENTITY]
    public override string EnglishPageTitle => row.Caption;

    public override string LocalPageTitle => RowText.Localize(row.Caption, table: Some<object>(IPlugInSink.Of(this))).Local;

    public override PropertyPageType PageType => row.PageType.IfNone(() => base.PageType);

    public override ObjectType SupportedTypes => row.SupportedTypes.IfNone(() => base.SupportedTypes);

    public override bool AllObjectsMustBeSupported => row.AllObjectsMustBeSupported.IfNone(() => base.AllObjectsMustBeSupported);

    public override bool SupportsSubObjects => row.SupportsSubObjects.IfNone(() => base.SupportsSubObjects);

    public override int Index => row.Index.IfNone(() => base.Index);

    public override string PageIconEmbeddedResourceString => Conversions.Unset(row.Icon.Map(static glyph => Icons.ResourceName(glyph, IconSlot.PropertiesPage.Master)));

    public override object? PageControl => Realized.Map(static object (view) => view).ValueUnsafe();

    private bool ViewPage => row.PageType.Exists(static type => type == PropertyPageType.View);

    // --- [EDGES]
    public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e) =>
        Callbacks.Answer(
            from included in IO.lift(() => ViewPage ? e.View is not null : base.ShouldDisplay(e))
            from shown in included && row.Visible.IsSome ? Realized.Match(Some: static view => view.Shows, None: static () => IO.pure(false)) : IO.pure(included)
            select shown,
            static () => false,
            CallbackSite.Of(this));

    public override void UpdatePage(ObjectPropertiesPageEventArgs e) =>
        _ = Callbacks.Answer(page.Value.Map(static view => view.Reread), static () => unit, static () => unit, CallbackSite.Of(this));

    public override bool OnActivate(bool active) {
        _ = (active ? Realized : page.Value).Iter(view => view.Shown(active));
        return true;
    }

    public override void OnHelp() => _ = Callbacks.Answer(row.Help.Map(static topic => topic.Show), static () => unit, static () => unit, CallbackSite.Of(this));

    // --- [SCRIPTS]
    public override Result RunScript(ObjectPropertiesPageEventArgs e) =>
        Callbacks.Answer(
            Conversions.ToResult(
                from view in IO.lift(() => Realized.ToFin(new Missing(nameof(PageControl))))
                from document in IO.lift(() => Missing.Unless(e.Document, nameof(ObjectPropertiesPageEventArgs.Document)))
                from scripted in Showing.RunScript((IPlugInViews)IPlugInSink.Of(this), row, view.Scope, document)
                select scripted),
            static () => Result.Failure,
            CallbackSite.Of(this));

    // --- [REALIZATION]
    private Option<RealizedView> Realized =>
        page.Value || Callbacks.Answer(
            from document in IO.lift(() => Optional(new ObjectPropertiesPageEventArgs(this).Document))
            from view in RealizedView.Open((IPlugInViews)IPlugInSink.Of(this), row, document, new CommitMode.Targets(this), Refreshing)
            from opened in page.SwapIO(_ => Some(view))
            select opened,
            static () => Option<RealizedView>.None,
            CallbackSite.Of(this, nameof(PageControl)));

    private Option<Func<RealizedView, IO<IDisposable>>> Refreshing =>
        ViewPage
            ? Option<Func<RealizedView, IO<IDisposable>>>.None
            : Some<Func<RealizedView, IO<IDisposable>>>(view => DisposalOps.AcquireAll(
                    view.Scope.Document.ToSeq()
                        .Bind(static document => Seq(
                            EventKind.ModifyObjectAttributes.In(document.RuntimeSerialNumber).Choose(static _ => Some((unit, unit))),
                            EventKind.ReplaceRhinoObject.In(document.RuntimeSerialNumber).Choose(static _ => Some((unit, unit)))))
                        .Map(signal => signal.Through(Subscriptions.Idle<Unit, Unit>(_ => view.Reread), view.Scope.Sink)),
                    DisposalOps.Release)
                .Map(held => DisposalOps.Composite(held, CallbackSite.Of(this, nameof(UpdatePage)))));
}

public abstract class DefinedTab(View.Tab row) : ViewPanel(row, Optional(RhinoDoc.ActiveDoc)) {
    // --- [EDGES]
    public void OnVisibilityChanged(bool visible) => _ = Shown(visible);

    public void TabActivated(uint state) => _ = Shown(state != 0);

    public void DisplayData() => _ = Callbacks.Answer(Realized.Map(static view => view.Reread), static () => unit, static () => unit, CallbackSite.Of(this));

    public void DoHelp() => _ = Callbacks.Answer(Row.Help.Map(static topic => topic.Show), static () => unit, static () => unit, CallbackSite.Of(this));

    // --- [RELEASE]
    public void Destroy() => Dispose();
}

public abstract class DefinedPane(View.Pane row) : ViewPanel(row, Optional(RhinoDoc.ActiveDoc)), IHelp {
    // --- [IDENTITY]
    public string HelpUrl => Row.HelpUrl;

    // --- [EDGES]
    public void OnVisibilityChanged(bool visible) => _ = Shown(visible);

    // --- [RELEASE]
    public void Destroy() => Dispose();
}

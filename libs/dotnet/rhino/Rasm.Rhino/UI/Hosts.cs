using Eto.Forms;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Render;
using Rhino.Runtime;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Controls.DataSource;
using NativeDataSourceEventArgs = Rhino.UI.Controls.DataSource.EventArgs;

namespace Rasm.Rhino.UI;

// --- [SERVICES] ------------------------------------------------------------------------
internal sealed class RealizedView : Panel {
    // --- [STATE]
    private readonly View row;
    private RowRules Rules => new(row.Enabled, row.Visible);
    private Seq<IDisposable> owned;
    private readonly ShownSubscription listeners;
    private bool showing;

    private RealizedView(View row, RowScope scope, BindingGroup group, Option<Func<RealizedView, Action<IDisposable>, IO<Unit>>> listening) {
        (this.row, Scope, Group) = (row, scope, group);
        listeners = new ShownSubscription(retain => Listening(listening, retain));
    }

    public RowScope Scope { get; }
    public BindingGroup Group { get; }

    // --- [REALIZATION]
    public static IO<RealizedView> Open(IPlugInViews owner, View row, Option<RhinoDoc> document, CommitMode mode, Option<Func<RealizedView, Action<IDisposable>, IO<Unit>>> listening, Action<IDisposable> retain) =>
            from bare in RowScope.Open(owner, document, mode)
            from bindings in IO.lift(ViewOps.Bindings(owner, row, bare))
            from history in IO.lift(row.Store.Traverse(store => ViewOps.History(owner, row, store, bindings, document)).As())
            let scope = bare.Nested(ViewOps.Settings(owner, row), history, ViewOps.Locks(row, document))
            from hidden in scope.Shown(visible: false)
            from opened in scope.Opened(bindings)
            from view in DisposalOps.Acquire(IO.lift(() => new RealizedView(row, scope, bindings, listening)), static view => IO.lift(view.Dispose), retain)
            from content in ViewOps.Realize(owner, row.Children, scope, resource => view.owned = view.owned.Add(resource))
            from configured in IO.lift(() => view.Content = content)
            from evaluated in view.Evaluated
            from visibility in DisposalOps.Acquire(IO.lift(() => new Disposal<Control>(content, static held => SectionHolder.Shown(held, visible: false).Run())), static lease => IO.lift(lease.Dispose), resource => view.owned = view.owned.Add(resource))
            select view;

    // --- [EDGES]
    public new IO<Unit> Shown(bool visible) =>
        IO.lift(() => showing = visible).Bind(_ => Callbacks.Each(Seq(
            listeners.Shown(visible),
            visible ? Scope.Changed(Keys(row)).Bind(_ => Evaluated) : Evaluated))).Map(static _ => unit);

    public IO<Unit> Reread => Scope.Changed(ViewCatalog.Tree(row).Bind(Keys)).Bind(_ => Evaluated);

    // --- [RULES]
    private IO<Unit> Evaluated =>
        from shows in Rules.Shows(Scope)
        from enables in Rules.Enables(Scope)
        from set in IO.lift(() => { (Visible, Enabled) = (shows, enables); })
        from availability in Scope.Shown(showing && shows && enables)
        from sections in SectionHolder.Shown(Content, showing && shows && enables)
        select unit;

    private IO<Unit> Listening(Option<Func<RealizedView, Action<IDisposable>, IO<Unit>>> listening, Action<IDisposable> retain) =>
        from signals in Rules.Each.Bind(static rule => rule.Sources).Distinct().TraverseM(source => source.Signals(Scope)).As()
        let reads = toHashSet(Rules.Each.Bind(static rule => rule.Reads))
        from changes in Scope.Listen(keys => when(keys.Exists(reads.Contains), Evaluated).As(), retain)
        from attached in signals.Flatten().TraverseM(signal => signal.Choose(static _ => Some((unit, unit)))
            .Through(Subscriptions.Idle<Unit, Unit>(_ => Evaluated), Scope.Sink, retain)).As()
        from extra in listening.Traverse(listen => listen(this, retain)).As()
        select unit;

    internal static Seq<EntryKey> Keys(View view) =>
        view.Children.Bind(static child => child.Controls).Bind(static control => control.Keys + control.Rules.Each.Bind(static rule => rule.Reads))
        + new RowRules(view.Enabled, view.Visible).Each.Bind(static rule => rule.Reads);

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) {
            try {
                _ = Shown(visible: false).Uninterruptible().Run();
            } finally {
                try {
                    listeners.Dispose();
                } finally {
                    _ = IO.lift(() => { Content = null; base.Dispose(disposing); })
                        .Finally(IO.lift(() => { Seq<IDisposable> released = owned; owned = []; return released; })
                            .Bind(DisposalOps.Release)).Uninterruptible().Run();
                }
            }
        } else
            base.Dispose(disposing);
    }
}

internal sealed class ViewFrame : RhinoScrollableDialogPanel {
    private Seq<IDisposable> owned;

    internal void Retain(IDisposable resource) => owned = owned.Add(resource);

    protected override void Dispose(bool disposing) {
        if (disposing) {
            _ = IO.lift(() => { Content = null; base.Dispose(disposing); })
                .Finally(IO.lift(() => { Seq<IDisposable> released = owned; owned = []; return released; })
                    .Bind(DisposalOps.Release)).Uninterruptible().Run();
        } else
            base.Dispose(disposing);
    }
}

public abstract class DefinedWindow(View.Window row) : Form(row.Kind.Handler()) {
    // --- [STATE]
    private Seq<IDisposable> owned;

    internal void Retain(IDisposable resource) => owned = owned.Add(resource);

    // --- [IDENTITY]
    internal View.Window Row => row;

    // --- [EDGES]
    protected override void OnLoad(System.EventArgs e) {
        base.OnLoad(e);
        Callbacks.Run(SectionHolder.Shown(Content, visible: true), CallbackSite.Of(this));
    }

    protected override void OnUnLoad(System.EventArgs e) =>
        _ = Callbacks.Each(Seq(SectionHolder.Shown(Content, visible: false), IO.lift(() => base.OnUnLoad(e)))).Uninterruptible().Run();

    protected override void OnClosed(System.EventArgs e) =>
        Callbacks.Run(Callbacks.Each(Seq(IO.lift(() => base.OnClosed(e)), IO.lift(Dispose))).Uninterruptible(), CallbackSite.Of(this));

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) {
            _ = IO.lift(() => { Content = null; base.Dispose(disposing); })
                .Finally(IO.lift(() => { Seq<IDisposable> released = owned; owned = []; return released; })
                    .Bind(DisposalOps.Release)).Uninterruptible().Run();
        } else
            base.Dispose(disposing);
    }
}

public abstract class ViewPanel : Panel {
    // --- [STATE]
    private Seq<IDisposable> owned;

    // --- [REALIZATION]
    private protected ViewPanel(View row, Option<RhinoDoc> document) {
        Row = row;
        Callbacks.Run(from view in RealizedView.Open(IPlugInViews.Of(this), row, document, new CommitMode.Immediate(), None, resource => owned = owned.Add(resource))
                      from configured in IO.lift(() => { Realized = Some(view); view.UseRhinoStyle(); Content = view; })
                      select unit, CallbackSite.Of(this));
    }

    // --- [IDENTITY]
    private protected View Row { get; }

    private protected Option<RealizedView> Realized { get; private set; }

    // --- [EDGES]
    private protected new IO<Unit> Shown(bool visible) => Realized.Traverse(view => view.Shown(visible)).As().Map(static _ => unit);

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) {
            _ = IO.lift(() => { Content = null; base.Dispose(disposing); })
                .Finally(IO.lift(() => { Realized = None; Seq<IDisposable> released = owned; owned = []; return released; })
                    .Bind(DisposalOps.Release)).Uninterruptible().Run();
        } else
            base.Dispose(disposing);
    }
}

public abstract class DefinedPanel(RhinoDoc? document, View.Panel row) : ViewPanel(row, Optional(document)), IPanel, IHelp {
    // --- [IDENTITY]
    public string HelpUrl => Row.HelpReference;

    // --- [EDGES]
    public void PanelShown(uint documentSerialNumber, ShowPanelReason reason) => Callbacks.Run(Shown(visible: true), CallbackSite.Of(this));

    public void PanelHidden(uint documentSerialNumber, ShowPanelReason reason) => Callbacks.Run(Shown(visible: false), CallbackSite.Of(this));

    public void PanelClosing(uint documentSerialNumber, bool onCloseDocument) => Callbacks.Run(Shown(visible: false), CallbackSite.Of(this));
}

public abstract class DefinedSection(View.Section row) : EtoCollapsibleSection3 {
    // --- [STATE]
    private readonly Atom<Option<SectionBody>> realized = Atom(Option<SectionBody>.None);
    private Seq<IDisposable> owned;

    private Option<SectionBody> Current => realized.Value;

    // --- [IDENTITY]
    public override LocalizeStringPair Caption => RowText.Localize(row.Caption, table: Some<object>(IPlugInSink.Of(this)));

    public override Guid PlugInId => IPlugInViews.Of(this).PlugIn.Id;

    public override int SectionHeight => Current.Match(Some: static held => held.Height, None: static () => 0);

    public override bool Hidden => Current.Exists(static held => held.Hidden);

    public override IHeaderButtonHandler NewHeaderButtonHandler() => Current.Bind(static held => held.Header).IfNone(base.NewHeaderButtonHandler);

    // --- [REALIZATION]
    public IO<Unit> Attach(Func<Action<IDisposable>, IO<SectionBody>> body) =>
        DisposalOps.Replace(body, (value, release) => {
            Seq<IDisposable> prior = owned;
            Control? content = Content;
            owned = prior.Add(release);
            IO<Unit> rejected = IO.lift(() => owned = prior).Bind(_ => IO.lift(release.Dispose));
            IO<Unit> restore = IO.lift(() => { Content = content; });
            return from assigned in DisposalOps.OnFailure(IO.lift(() => { Content = value.Content; }), restore.Bind(_ => rejected))
                   from published in IO.lift(() => { _ = realized.Swap(_ => Some(value)); owned = Seq(release); })
                   from released in DisposalOps.Release(prior)
                   select unit;
        }).Map(static _ => unit);

    // --- [EDGES]
    public override void HolderVisible(bool visible) =>
        Callbacks.Run(Current.Traverse(held => held.Shown(visible)).As(), CallbackSite.Of(this));

    public override void UpdateView(uint flags) =>
        Callbacks.Run(Current.Map(static held => held.Reread), CallbackSite.Of(this));

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) {
            _ = IO.lift(() => { Content = null; base.Dispose(disposing); })
                .Finally(IO.lift(() => { _ = realized.Swap(static _ => None); Seq<IDisposable> released = owned; owned = []; return released; })
                    .Bind(DisposalOps.Release)).Uninterruptible().Run();
        } else
            base.Dispose(disposing);
    }
}

public abstract class DefinedContentSection : EtoContentUISection3 {
    // --- [STATE]
    private readonly View.Section row;
    private readonly ContentScope scope;
    private readonly Option<(RowScope Rows, SectionBody Body)> realized;
    private readonly ShownSubscription listeners;
    private Seq<IDisposable> owned;

    protected DefinedContentSection(View.Section row, ContentScope scope) {
        IPlugInViews owner = IPlugInViews.Of(this);
        (this.row, this.scope) = (row, scope);
        realized = Callbacks.Answer(
            from rows in RowScope.Open(owner, None, new CommitMode.Contents(this))
            from body in SectionBody.Realize(owner, row, rows, resource => owned = owned.Add(resource))
            from configured in IO.lift(() => Content = body.Content)
            select Some((Rows: rows, Body: body)),
            static () => Option<(RowScope Rows, SectionBody Body)>.None, CallbackSite.Of(this));
        listeners = new ShownSubscription(retain => realized.Traverse(value => FieldChanges(value.Rows, owner, retain)
            .Bind(_ => SelectionChanges(value.Body, owner, retain))).As().Map(static _ => unit));
    }

    // --- [IDENTITY]
    public override LocalizeStringPair Caption => RowText.Localize(row.Caption, table: Some<object>(IPlugInSink.Of(this)));

    public override Guid PlugInId => IPlugInViews.Of(this).PlugIn.Id;

    public override int SectionHeight => realized.Match(Some: static held => held.Body.Height, None: static () => 0);

    public override bool Hidden => base.Hidden || realized.ForAll(held => held.Body.Hidden || !Selected.Head.ForAll(scope.Covers));

    public override IHeaderButtonHandler NewHeaderButtonHandler() => realized.Bind(static held => held.Body.Header).IfNone(base.NewHeaderButtonHandler);

    private Seq<RenderContent> Selected => toSeq(GetSelection()).Strict();

    // --- [EDGES]
    public override void HolderVisible(bool visible) =>
        Callbacks.Run(Callbacks.Each(Seq(
            realized.Traverse(held => held.Body.Shown(visible)).As().Map(static _ => unit), listeners.Shown(visible))), CallbackSite.Of(this));

    public override void UpdateView(uint flags) =>
        Callbacks.Run(realized.Map(static held => held.Body.Reread), CallbackSite.Of(this));

    private IO<Unit> FieldChanges(RowScope rows, IPlugInSink sink, Action<IDisposable> retain) =>
        EventKind.ContentFieldChanged
            .Choose(static args => Some((ViewRegistration.FieldKey(args.Content, args.FieldName), args.Content.Id)))
            .Through(Subscriptions.Idle<EntryKey, Guid>(changed => IO.lift(() => toHashSet(Selected.Map(static content => content.Id)))
                .Bind(selected => rows.Changed(toSeq(changed.Filter(selected.Contains).Keys)))), sink, retain);

    private IO<Unit> SelectionChanges(SectionBody body, IPlugInSink sink, Action<IDisposable> retain) =>
        Subscriptions.Attach(
            handler => DataChanged += handler,
            handler => DataChanged -= handler,
            Callbacks.Handler<NativeDataSourceEventArgs>(args => when(args.DataType == ProviderIds.ContentSelection, body.Reread).As(), new CallbackSite(sink, GetType(), nameof(DataChanged))), retain);

    // --- [RELEASE]
    protected override void Dispose(bool disposing) {
        if (disposing) {
            try {
                listeners.Dispose();
            } finally {
                _ = IO.lift(() => { Content = null; base.Dispose(disposing); })
                    .Finally(IO.lift(() => { Seq<IDisposable> released = owned; owned = []; return released; })
                        .Bind(DisposalOps.Release)).Uninterruptible().Run();
            }
        } else
            base.Dispose(disposing);
    }
}

public abstract class DefinedOptionsPage : OptionsDialogPage {
    // --- [STATE]
    private readonly View row;
    private readonly CommitMode commit;
    private readonly Atom<Option<RhinoDoc>> document = Atom(Option<RhinoDoc>.None);
    private readonly Atom<Option<(RealizedView View, Control Frame)>> page = Atom(Option<(RealizedView View, Control Frame)>.None);
    private Seq<IDisposable> owned;

    protected DefinedOptionsPage(View.OptionsPage row) : this(row, row.Commit, row.Navigation) { }

    protected DefinedOptionsPage(View.DocumentPage row) : this(row, row.Commit, row.Navigation) { }

    private DefinedOptionsPage(View row, CommitMode commit, Option<NavigationText> navigation) : base(row.Caption) {
        (this.row, this.commit) = (row, commit);
        _ = navigation.Iter(text => (NavigationTextIsBold, NavigationTextColor) = (text.Bold, Conversions.Unset(text.Color)));
    }

    // --- [IDENTITY]
    public override string LocalPageTitle => RowText.Localize(row.Caption, table: Some<object>(IPlugInSink.Of(this))).Local;

    public override System.Drawing.Image? PageImage =>
        Callbacks.Answer(IO.lift(System.Drawing.Image? () => IPlugInViews.Of(this).PlugIn.Icon(new(128, 128))), static () => null, CallbackSite.Of(this));

    public override object? PageControl => Realized.Map(static object (held) => held.Frame).ValueUnsafe();

    public override bool ShowApplyButton => commit.Map(immediate: false, revertible: false, deferred: true, targets: false, contents: false);

    public override bool ShowDefaultsButton => row.Store.IsSome;

    public IO<Unit> ForDocument(RhinoDoc held) => document.SwapIO(_ => Some(held)).Map(static _ => unit);

    public IO<Unit> Navigate(PageTarget target) => Showing.SetActivePageTo(this, target);

    // --- [EDGES]
    public override bool OnActivate(bool active) =>
        Callbacks.Succeeded((active ? Realized : page.Value).Traverse(held => held.View.Shown(active)).As().Map(ignore), CallbackSite.Of(this));

    public override void OnHelp() => Callbacks.Run(row.Help.Map(static topic => topic.Show), CallbackSite.Of(this));

    // --- [COMMITS]
    public override bool OnApply() =>
        Callbacks.Succeeded(page.Value.Traverse(static held => held.View.Scope.Apply(held.View.Group)).As().Map(ignore), CallbackSite.Of(this));

    public override void OnCancel() =>
        Callbacks.Run(page.Value.Map(static held => held.View.Scope.Revert(held.View.Group)), CallbackSite.Of(this));

    public override void OnDefaults() =>
        Callbacks.Run(page.Value.Map(static held => held.View.Scope.Commit(held.View.Group, held.View.Group.Defaults).Map(static _ => unit)), CallbackSite.Of(this));

    // --- [SCRIPTS]
    public override Result RunScript(RhinoDoc doc, RunMode mode) =>
        Callbacks.Answer(
            Conversions.ToResult(RowScope.Open(IPlugInSink.Of(this), Some(doc), new CommitMode.Immediate())
                .Bind(scope => Showing.RunScript(IPlugInViews.Of(this), row, scope, doc))),
            static () => Result.Failure,
            CallbackSite.Of(this));

    // --- [REALIZATION]
    private Option<(RealizedView View, Control Frame)> Realized =>
        page.Value || Callbacks.Answer(
            from held in document.ValueIO
            from frame in DisposalOps.Acquire(IO.lift(static () => new ViewFrame()), static frame => IO.lift(frame.Dispose), resource => owned = owned.Add(resource))
            from view in RealizedView.Open(IPlugInViews.Of(this), row, held, commit, Some(Marking), frame.Retain)
            from configured in IO.lift(() => { frame.Content = view; frame.UseRhinoStyle(); })
            from published in page.SwapIO(_ => Some((View: view, Frame: (Control)frame)))
            select published,
            static () => Option<(RealizedView View, Control Frame)>.None, CallbackSite.Of(this, nameof(PageControl)));

    private IO<Unit> Marking(RealizedView view, Action<IDisposable> retain) =>
        from listened in view.Scope.Listen(_ => Marked(view), retain)
        from marked in Marked(view)
        select unit;

    private IO<Unit> Marked(RealizedView view) =>
        view.Scope.Pending.Bind(pending => IO.lift(() => { Modified = pending; }));
}

public abstract class DefinedPropertiesPage(View.PropertiesPage row) : ObjectPropertiesPage {
    // --- [STATE]
    private readonly Atom<Option<RealizedView>> page = Atom(Option<RealizedView>.None);
    private Seq<IDisposable> owned;

    // --- [IDENTITY]
    public override string EnglishPageTitle => row.Caption;

    public override string LocalPageTitle => RowText.Localize(row.Caption, table: Some<object>(IPlugInSink.Of(this))).Local;

    public override PropertyPageType PageType => row.PageType.IfNone(() => base.PageType);

    public override ObjectType SupportedTypes => row.SupportedTypes.IfNone(() => base.SupportedTypes);

    public override bool AllObjectsMustBeSupported => row.AllObjectsMustBeSupported.IfNone(() => base.AllObjectsMustBeSupported);

    public override bool SupportsSubObjects => row.SupportsSubObjects.IfNone(() => base.SupportsSubObjects);

    public override int Index => row.Index.IfNone(() => base.Index);

    public override string PageIconEmbeddedResourceString =>
        Conversions.Unset(row.Icon.Filter(static _ => HostUtils.RunningOnOSX)
            .Map(static glyph => Path.ChangeExtension(Icons.ResourceName(glyph.Glyph, glyph.Master), ".png")));

    public override System.Drawing.Icon? PageIcon(System.Drawing.Size sizeInPixels) =>
        Callbacks.Answer(row.Icon.TraverseM(glyph => Icons.DrawingIcon(glyph.Glyph, glyph.Master, sizeInPixels)).As(),
            static () => Option<System.Drawing.Icon>.None, CallbackSite.Of(this)).ValueUnsafe();

    public override object? PageControl => Realized.Map(static object (view) => view).ValueUnsafe();

    private bool ViewPage => row.PageType.Exists(static type => type == PropertyPageType.View);

    // --- [EDGES]
    public override bool ShouldDisplay(ObjectPropertiesPageEventArgs e) =>
        Callbacks.Answer(
            from included in IO.lift(() => ViewPage ? e.View is not null : base.ShouldDisplay(e))
            from shown in included && row.Visible.IsSome ? Realized.Match(Some: view => RowRules.Holds(row.Visible, view.Scope), None: static () => IO.pure(value: false)) : IO.pure(included)
            select shown,
            static () => false,
            CallbackSite.Of(this));

    public override void UpdatePage(ObjectPropertiesPageEventArgs e) =>
        Callbacks.Run(page.Value.Map(static view => view.Reread), CallbackSite.Of(this));

    public override bool OnActivate(bool active) =>
        Callbacks.Succeeded((active ? Realized : page.Value).Traverse(view => view.Shown(active)).As().Map(ignore), CallbackSite.Of(this));

    public override void OnHelp() => Callbacks.Run(row.Help.Map(static topic => topic.Show), CallbackSite.Of(this));

    // --- [SCRIPTS]
    public override Result RunScript(ObjectPropertiesPageEventArgs e) =>
        Callbacks.Answer(
            Conversions.ToResult(
                from view in IO.lift(() => Realized.ToFin(new Missing(nameof(PageControl))))
                from document in IO.lift(() => Missing.Unless(e.Document, nameof(ObjectPropertiesPageEventArgs.Document)))
                from scripted in Showing.RunScript(IPlugInViews.Of(this), row, view.Scope, document)
                select scripted),
            static () => Result.Failure,
            CallbackSite.Of(this));

    // --- [REALIZATION]
    private Option<RealizedView> Realized =>
        page.Value || Callbacks.Answer(
            from document in IO.lift(() => Optional(new ObjectPropertiesPageEventArgs(this).Document))
            from view in RealizedView.Open(IPlugInViews.Of(this), row, document, new CommitMode.Targets(this), Refreshing, resource => owned = owned.Add(resource))
            from published in page.SwapIO(_ => Some(view))
            select published,
            static () => Option<RealizedView>.None, CallbackSite.Of(this, nameof(PageControl)));

    private Option<Func<RealizedView, Action<IDisposable>, IO<Unit>>> Refreshing =>
        ViewPage
            ? Option<Func<RealizedView, Action<IDisposable>, IO<Unit>>>.None
            : Some<Func<RealizedView, Action<IDisposable>, IO<Unit>>>(static (view, retain) =>
                view.Scope.Document.ToSeq()
                    .Bind(static document => Seq(
                        EventKind.ModifyObjectAttributes.In(document.RuntimeSerialNumber).Choose(static _ => Some((unit, unit))),
                        EventKind.ReplaceRhinoObject.In(document.RuntimeSerialNumber).Choose(static _ => Some((unit, unit)))))
                    .TraverseM(signal => signal.Through(Subscriptions.Idle<Unit, Unit>(_ => view.Reread), view.Scope.Sink, retain)).As().Map(static _ => unit));

}

public abstract class DefinedTab(View.Tab row) : ViewPanel(row, Optional(RhinoDoc.ActiveDoc)) {
    // --- [EDGES]
    public void OnVisibilityChanged(bool visible) => Callbacks.Run(Shown(visible), CallbackSite.Of(this));

    public void TabActivated(uint state) => Callbacks.Run(Shown(state != 0), CallbackSite.Of(this));

    public void DisplayData() => Callbacks.Run(Realized.Map(static view => view.Reread), CallbackSite.Of(this));

    public void DoHelp() => Callbacks.Run(Row.Help.Map(static topic => topic.Show), CallbackSite.Of(this));

    // --- [RELEASE]
    public void Destroy() => Dispose();
}

public abstract class DefinedPane(View.Pane row) : ViewPanel(row, Optional(RhinoDoc.ActiveDoc)), IHelp {
    // --- [IDENTITY]
    public string HelpUrl => Row.HelpReference;

    // --- [EDGES]
    public void OnVisibilityChanged(bool visible) => Callbacks.Run(Shown(visible), CallbackSite.Of(this));

    // --- [RELEASE]
    public void Destroy() => Dispose();
}

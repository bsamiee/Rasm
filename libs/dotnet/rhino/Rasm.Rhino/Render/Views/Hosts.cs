using System.Globalization;
using Rasm.Rhino.Commands;
using Rasm.Rhino.Persistence;
using Rasm.Rhino.UI;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Commands;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;
using Rhino.UI.Controls.DataSource;

namespace Rasm.Rhino.Render.Views;

// --- [SERVICES] ------------------------------------------------------------------------
public abstract class DefinedTab<TSelf> : Eto.Forms.Panel where TSelf : DefinedTab<TSelf>, IDefined<View.Tab> {
    private readonly HostedView hosted;

    protected DefinedTab(RhinoDoc document) {
        hosted = new HostedView(() => HostedView.Styled(HostedView.Seated(TSelf.Definition, Some(document), new CommitMode.Immediate(), None)));
        Content = hosted.Realized.Map(static held => held.Frame).ValueUnsafe();
    }

    public IO<Option<RenderWindow>> Window => IO.lift(() => Conversions.Present(RenderTabs.SidePaneUiIdFromTab(this)).Bind(static id => Optional(RenderWindow.FromSessionId(id))));

    public void OnVisibilityChanged(bool visible) => _ = hosted.Shown(visible, realizing: visible);

    public void TabActivated(uint state) => _ = hosted.Shown(state != 0, realizing: state != 0);

    public void DisplayData() => _ = hosted.Reread;

    public void DoHelp() => _ = Session.Run(HelpTopic.Of(TSelf.Definition.Help).Traverse(static topic => topic.Show).As());

    public void Destroy() => Dispose();

    public static IO<Option<TSelf>> FromRenderSessionId(Guid renderSessionId) =>
        IO.lift(() => Optional(RenderTabs.FromRenderSessionId(Session.PlugIn, typeof(TSelf), renderSessionId) as TSelf));
}

public abstract class DefinedPane<TSelf> : Eto.Forms.Panel, IHelp where TSelf : DefinedPane<TSelf>, IDefined<View.Pane> {
    private readonly HostedView hosted;

    protected DefinedPane(RhinoDoc document) {
        hosted = new HostedView(() => HostedView.Styled(HostedView.Seated(TSelf.Definition, Some(document), new CommitMode.Immediate(), None)));
        Content = hosted.Realized.Map(static held => held.Frame).ValueUnsafe();
    }

    public string HelpUrl => HostedView.HelpUrl(TSelf.Definition);

    public IO<Option<RenderWindow>> Window =>
        IO.lift(() => Optional(Internals.FindExistingPanelData(owner: null, Session.PlugIn.Id, typeof(TSelf).GUID))
            .Bind(data => Conversions.Present(Internals.SidePaneUiIdFromPanelObject(data, this)))
            .Bind(static id => Optional(RenderWindow.FromSessionId(id))));

    public void OnVisibilityChanged(bool visible) => _ = hosted.Shown(visible, realizing: visible);

    public void Destroy() => Dispose();

    public static IO<Option<TSelf>> FromRenderSessionId(Guid renderSessionId) =>
        IO.lift(() => Optional(RenderPanels.FromRenderSessionId(Session.PlugIn, typeof(TSelf), renderSessionId))
            .Bind(static instance => Optional(Internals.PanelObject(instance) as TSelf)));
}

internal sealed class DefinedContentSection : EtoContentUISection3, ICollapsibleSection {
    private readonly View.ContentSection row;
    private readonly HostedSection hosted;

    internal DefinedContentSection(View.ContentSection row) {
        this.row = row;
        hosted = new HostedSection(this, row.Actions, Some(ProviderIds.ContentSelection), () => Excluded, visible: true, Realized);
    }

    Guid ICollapsibleSection.Id => row.Identity.GUID;

    public override string SettingsTag => row.Identity.GUID.ToString("D", CultureInfo.InvariantCulture);

    public override LocalizeStringPair Caption => LocalText.Of(row.Caption);

    public override Guid PlugInId => Session.PlugIn.Id;

    public override int SectionHeight => hosted.Height;

    public override bool Hidden => Excluded || hosted.Hidden;

    public override IHeaderButtonHandler NewHeaderButtonHandler() => hosted.Header.IfNone(base.NewHeaderButtonHandler);

    public override void UpdateView(uint flags) => _ = hosted.Reread;

    private bool Excluded => base.Hidden || !Selected.Head.ForAll(row.Scope.Covers);

    private Seq<RenderContent> Selected => toSeq(GetSelection()).Strict();

    private IO<ViewBody> Realized =>
        from rows in RowScope.Open(Selected.Head.Bind(static content => Optional(content.DocumentOwner)), new CommitMode.Contents(this))
        from held in ViewBody.Realize(row, row.Actions, rows)
        from fields in FieldChanges(rows)
        select held;

    private IO<Unit> FieldChanges(RowScope rows) =>
        from post in Mailboxes.Idle<EntryKey, Guid>(changed => IO.lift(() => toHashSet(Selected.Map(static content => content.Id)))
            .Bind(selected => rows.Changed(toSeq(changed.Filter(selected.Contains).Keys))))
        from attached in Subscriptions.On<RenderContentFieldChangedEventArgs>(static h => RenderContent.ContentFieldChanged += h, static h => RenderContent.ContentFieldChanged -= h)
            .Documents(static args => args.Document)
            .Attach(args => post(ContentViews.FieldKey(args.Content, args.FieldName), args.Content.Id))
        select unit;

    protected override void Dispose(bool disposing) {
        if (disposing) hosted.Dispose();
        base.Dispose(disposing);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ContentViews {
    // --- [SECTIONS]
    internal static IO<Unit> RegisterSections() =>
        Subscriptions.On<AddCustomUISectionsEventArgs>(static h => AddCustomUISections.OnAddCustomUISections += h, static h => AddCustomUISections.OnAddCustomUISections -= h)
            .Attach(static args => IO.lift(() => ViewCatalog.Current.Listed<View.ContentSection>().Iter(row => args.ExpandableContentUI.AddSection(new DefinedContentSection(row)))));

    public static EntryKey FieldKey(RenderContent content, string field) => EntryKey.Named(EntryKey.TypeOwner(content.TypeId), field);

    // --- [MENUS]
    public static IO<Unit> AddMenuItem(Seq<ContentMenuRow> rows) =>
        Products.Each(rows.Map(static (row, order) =>
                from pixels in IconSlot.Small.Pixels
                from icon in Icons.DrawingIcon(row.Icon, pixels)
                from added in IO.lift(() => RenderContentMenu.AddMenuItem(
                    Session.PlugIn.Id,
                    LocalText.Of(row.Caption).Local,
                    order,
                    row.Separator,
                    row.TopLevel,
                    icon,
                    collection => Session.Run(Results.Of(IO.lift(() => Conversions.Rows(collection)).Bind(row.Run)), Result.Failure),
                    (collection, context) => Session.Run(IO.lift(() => row.Contexts.Contains(context) && Conversions.Rows(collection) is { IsEmpty: false } selected && selected.ForAll(row.Scope.Covers)), failed: false)))
                select added))
            .Map(unit);
}

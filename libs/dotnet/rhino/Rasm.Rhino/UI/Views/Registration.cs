using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI.Views;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ContentScope {
    public bool Covers(RenderContent content) =>
        Switch(
            content,
            kinds: static (held, scope) => scope.Mask.HasFlag(Conversions.KindOf(held)),
            types: static (held, scope) => scope.TypeIds.Contains(held.TypeId));

    public sealed record Kinds(RenderContentKind Mask) : ContentScope;

    public sealed record Types(LanguageExt.HashSet<Guid> TypeIds) : ContentScope;
}

public sealed record ContentMenuRow(
    string Caption,
    IGlyph Icon,
    ContentScope Scope,
    LanguageExt.HashSet<RenderContentMenu.Context> Contexts,
    RenderContentMenu.SeparatorStyle Separator,
    bool TopLevel,
    Func<Seq<RenderContent>, IO<Unit>> Run);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewRegistration {
    // --- [FOLDS]
    private static IO<Unit> Each<TCase>(IPlugInViews plugIn, Func<TCase, IO<Unit>> register) where TCase : View =>
        IO.lift(() => plugIn.Views.Listed<TCase>()).Bind(rows => Callbacks.Each(rows.Map(register))).Map(static _ => unit);

    private static IO<Unit> Hosted(Type view, Action register) =>
        IO.lift(register).MapFail(error => error.HasException<ArgumentException>() ? new UnsupportedView(view, Some(error)) : error);

    // --- [PANELS]
    public static readonly Func<PlugIn, IPlugInSink, IO<IDisposable>> RegisterPanels =
        static (plugIn, sink) => Each<View.Panel>((IPlugInViews)sink, row => Hosted(row.Identity, () => Panels.RegisterPanel(
                plugIn,
                row.Identity,
                RowText.Localize(row.Caption, table: Some<object>(sink)).Local,
                plugIn.Assembly,
                Conversions.Unset(row.Icon.Map(static glyph => Icons.ResourceName(glyph, IconSlot.PanelTab.Master))),
                row.Kind.IfNone(PanelType.PerDoc))))
            .Map(static _ => Thinktecture.Empty.Disposable());

    // --- [PAGES]
    public static IO<Unit> OptionsDialogPages(IPlugInViews plugIn, List<OptionsDialogPage> pages) =>
        Each<View.OptionsPage>(plugIn, row => Tree(plugIn, row, static held => held.SubPages, None).Bind(page => IO.lift(() => page.Iter(pages.Add))));

    public static IO<Unit> DocumentPropertiesDialogPages(IPlugInViews plugIn, RhinoDoc document, List<OptionsDialogPage> pages) =>
        Each<View.DocumentPage>(plugIn, row => Tree(plugIn, row, static held => held.SubPages, Some(document)).Bind(page => IO.lift(() => page.Iter(pages.Add))));

    public static IO<Option<OptionsDialogPage>> RenderOptionsDialogPage(IPlugInViews plugIn, View.DocumentPage row, RhinoDoc document) =>
        Tree(plugIn, row, static held => held.SubPages, Some(document));

    public static IO<Unit> ObjectPropertiesPages(IPlugInViews plugIn, ObjectPropertiesPageCollection collection) =>
        Each<View.PropertiesPage>(plugIn, row => ViewOps.Construct<DefinedPropertiesPage>(row).Bind(page => IO.lift(() => collection.Add(page))));

    private static IO<Option<OptionsDialogPage>> Tree<TCase>(IPlugInViews plugIn, TCase row, Func<TCase, Seq<TCase>> subPages, Option<RhinoDoc> document) where TCase : View =>
        from scope in RowScope.Open(plugIn, document, new CommitMode.Immediate())
        from shown in RowRules.Holds(row.Visible, scope)
        from page in shown
            ? from built in ViewOps.Construct<DefinedOptionsPage>(row)
              from _ in document.TraverseM(built.ForDocument).As()
              from children in subPages(row).TraverseM(sub => Tree(plugIn, sub, subPages, document)).As()
              from __ in IO.lift(() => built.Children.AddRange(children.Somes()))
              select Some<OptionsDialogPage>(built)
            : IO.pure(Option<OptionsDialogPage>.None)
        select page;

    // --- [RENDER_WINDOW]
    public static IO<Unit> RegisterRenderTabs<TPlugIn>(TPlugIn plugIn, RenderTabs tabs) where TPlugIn : PlugIn, IPlugInViews =>
        Each<View.Tab>(plugIn, row =>
            from icon in row.Icon.TraverseM(glyph => Icons.DrawingIcon(plugIn, glyph, IconSlot.RenderTab)).As()
            from registered in Hosted(row.Identity, () => tabs.RegisterTab(
                plugIn, row.Identity, row.Scope.TabEngineId(plugIn), RowText.Localize(row.Caption, table: Some<object>(plugIn)).Local, icon.ValueUnsafe()))
            select registered);

    public static IO<Unit> RegisterRenderPanels<TPlugIn>(TPlugIn plugIn, RenderPanels panels) where TPlugIn : PlugIn, IPlugInViews =>
        Each<View.Pane>(plugIn, row => Hosted(row.Identity, () => panels.RegisterPanel(
            plugIn, RenderPanelType.RenderWindow, row.Identity, plugIn.Id, RowText.Localize(row.Caption, table: Some<object>(plugIn)).Local, row.Scope.AlwaysShow, row.InitialShow)));

    // --- [CUSTOM_SECTIONS]
    public static IO<Unit> CustomSections(IPlugInViews plugIn, Seq<View.Section> rows, List<ICollapsibleSection> sections) =>
        Documents.WithDocument(new DocumentSource.Active(), document =>
            from scope in RowScope.Open(plugIn, Some(document), new CommitMode.Immediate())
            from _ in Callbacks.Each(rows.Map(row => SectionStack.Section(plugIn, row, scope).Bind(section => IO.lift(() => sections.Add(section)))))
            select unit);

    // --- [CONTENT_SECTIONS]
    public static readonly Func<PlugIn, IPlugInSink, IO<IDisposable>> AddCustomUISections =
        static (_, sink) => EventKind.OnAddCustomUISections.Inline(
            args => Each<View.Section>((IPlugInViews)sink, row => when(
                row.Identity.IsSubclassOf(typeof(DefinedContentSection)),
                from section in ViewOps.Construct<DefinedContentSection>(row)
                from added in DisposalOps.OnFailure(IO.lift(() => args.ExpandableContentUI.AddSection(section)), IO.lift(section.Dispose))
                select added).As()),
            sink);

    public static EntryKey FieldKey(RenderContent content, string field) =>
        new(EntryKey.TypeOwner(content.TypeId), Seq(field));

    // --- [CONTENT_MENUS]
    public static Func<PlugIn, IPlugInSink, IO<IDisposable>> AddMenuItem(Seq<ContentMenuRow> rows) =>
        (plugIn, sink) => Callbacks.Each(rows.Map((row, order) =>
                from icon in Icons.DrawingIcon(sink, row.Icon, IconSlot.ContentMenu)
                let site = new CallbackSite(sink, typeof(RenderContentMenu), row.Caption)
                from added in IO.lift(() => RenderContentMenu.AddMenuItem(
                    plugIn.Id,
                    RowText.Localize(row.Caption, table: Some<object>(sink)).Local,
                    order,
                    row.Separator,
                    row.TopLevel,
                    icon,
                    collection => Executed(row, collection, site),
                    (collection, context) => Enabled(row, collection, context, site)))
                select added))
            .Map(static _ => Thinktecture.Empty.Disposable());

    private static Result Executed(ContentMenuRow row, RenderContentCollection collection, CallbackSite site) =>
        Callbacks.Answer(Conversions.ToResult(IO.lift(() => Conversions.Rows(collection)).Bind(row.Run)), static () => Result.Failure, site);

    private static bool Enabled(ContentMenuRow row, RenderContentCollection collection, RenderContentMenu.Context context, CallbackSite site) =>
        Callbacks.Answer(
            IO.lift(() => row.Contexts.Contains(context) && Conversions.Rows(collection) is { IsEmpty: false } selected && selected.ForAll(row.Scope.Covers)),
            static () => false,
            site);
}

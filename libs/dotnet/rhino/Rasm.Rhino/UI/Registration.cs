using Rasm.Rhino.Document.Files;
using Rasm.Rhino.Events;
using Rasm.Rhino.Persistence.Settings;
using Rasm.Rhino.Persistence.Stores;
using Rasm.Rhino.UI.Assets;
using Rasm.Rhino.UI.Rows;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI;

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
    (IGlyph Glyph, int Master) Icon,
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
    public static readonly Func<IPlugInViews, Action<IDisposable>, IO<Unit>> RegisterPanels =
        static (plugIn, retain) => {
            Func<View.Panel, IO<Option<System.Drawing.Icon>>> image = static row =>
                from size in IconSlot.PanelTab.Size()
                from pixels in HostTheme.Device(new(size, size))
                from icon in row.Icon.TraverseM(glyph => Icons.DrawingIcon(glyph.Glyph, glyph.Master, pixels)).As()
                select icon;
            IO<Unit> refresh = Each<View.Panel>(plugIn, row => image(row)
                .Bind(icon => IO.lift(() => icon.Iter(held => Panels.ChangePanelIcon(row.Identity, held)))).Map(static _ => unit));
            return DisposalOps.Acquire(owned =>
                from registered in Each<View.Panel>(plugIn, row => image(row).Bind(icon => Hosted(row.Identity, () => Panels.RegisterPanel(
                    plugIn.PlugIn, row.Identity, RowText.Localize(row.Caption, table: Some<object>(plugIn)).Local,
                    icon.ValueUnsafe(), row.Kind.IfNone(PanelType.PerDoc)))))
                from subscriptions in HostTheme.Changed.Cons(Appearance.TabIconSize.Changed.ToSeq())
                    .TraverseM(signal => signal.Inline(_ => refresh, plugIn, owned)).As()
                select unit, (_, release) => retain(release));
        };

    // --- [PAGES]
    public static IO<Unit> OptionsDialogPages(IPlugInViews plugIn, ICollection<OptionsDialogPage> pages) =>
        Each<View.OptionsPage>(plugIn, row => Tree(plugIn, row, static held => held.SubPages, None).Bind(page => IO.lift(() => page.Iter(pages.Add))));

    public static IO<Unit> DocumentPropertiesDialogPages(IPlugInViews plugIn, RhinoDoc document, ICollection<OptionsDialogPage> pages) =>
        Each<View.DocumentPage>(plugIn, row => Tree(plugIn, row, static held => held.SubPages, Some(document)).Bind(page => IO.lift(() => page.Iter(pages.Add))));

    public static IO<Option<OptionsDialogPage>> RenderOptionsDialogPage(IPlugInViews plugIn, View.DocumentPage row, RhinoDoc document) =>
        Tree(plugIn, row, static held => held.SubPages, Some(document));

    public static IO<Unit> ObjectPropertiesPages(IPlugInViews plugIn, ObjectPropertiesPageCollection collection) =>
        Each<View.PropertiesPage>(plugIn, row => IO.lift(() => (DefinedPropertiesPage)Activator.CreateInstance(row.Identity)!).Bind(page => IO.lift(() => collection.Add(page))));

    private static IO<Option<OptionsDialogPage>> Tree<TCase>(IPlugInViews plugIn, TCase row, Func<TCase, Seq<TCase>> subPages, Option<RhinoDoc> document) where TCase : View =>
        from scope in RowScope.Open(plugIn, document, new CommitMode.Immediate())
        from shown in RowRules.Holds(row.Visible, scope)
        from page in shown
            ? from built in IO.lift(() => (DefinedOptionsPage)Activator.CreateInstance(row.Identity)!)
              from _ in document.TraverseM(built.ForDocument).As()
              from children in subPages(row).TraverseM(sub => Tree(plugIn, sub, subPages, document)).As()
              from __ in IO.lift(() => built.Children.AddRange(children.Somes()))
              select Some<OptionsDialogPage>(built)
            : IO.pure(Option<OptionsDialogPage>.None)
        select page;

    // --- [RENDER_WINDOW]
    public static IO<Unit> RegisterRenderTabs<TPlugIn>(TPlugIn plugIn, RenderTabs tabs) where TPlugIn : PlugIn, IPlugInViews =>
        Each<View.Tab>(plugIn, row =>
            from size in IconSlot.RenderTab.Size()
            from pixels in HostTheme.Device(new(size, size))
            from icon in row.Icon.TraverseM(glyph => Icons.DrawingIcon(glyph.Glyph, glyph.Master, pixels)).As()
            from registered in Hosted(row.Identity, () => tabs.RegisterTab(
                plugIn, row.Identity, row.Scope.TabEngineId(plugIn), RowText.Localize(row.Caption, table: Some<object>(plugIn)).Local, icon.ValueUnsafe()))
            select registered);

    public static IO<Unit> RegisterRenderPanels<TPlugIn>(TPlugIn plugIn, RenderPanels panels) where TPlugIn : PlugIn, IPlugInViews =>
        Each<View.Pane>(plugIn, row => Hosted(row.Identity, () => panels.RegisterPanel(
            plugIn, RenderPanelType.RenderWindow, row.Identity, row.Scope.TabEngineId(plugIn), RowText.Localize(row.Caption, table: Some<object>(plugIn)).Local,
            row.Scope.AlwaysShow, row.InitialShow, row.Position)));

    // --- [CUSTOM_SECTIONS]
    public static IO<Unit> CustomSections(IPlugInViews plugIn, Seq<View.Section> rows, ICollection<ICollapsibleSection> sections) =>
        Documents.WithDocument(new DocumentSource.Active(), document =>
            from scope in RowScope.Open(plugIn, Some(document), new CommitMode.Immediate())
            from _ in Callbacks.Each(rows.Map(row => SectionHolder.Section(plugIn, row, scope, sections.Add).Map(static _ => unit)))
            select unit);

    // --- [CONTENT_SECTIONS]
    public static readonly Func<IPlugInViews, Action<IDisposable>, IO<Unit>> AddCustomUISections =
        static (plugIn, retain) => EventKind.OnAddCustomUISections.Inline(
            args => Each<View.Section>(plugIn, row => when(
                row.Identity.IsSubclassOf(typeof(DefinedContentSection)),
                DisposalOps.Acquire(IO.lift(() => (DefinedContentSection)Activator.CreateInstance(row.Identity)!),
                    static section => IO.lift(section.Dispose), args.ExpandableContentUI.AddSection).Map(static _ => unit)).As()),
            plugIn, retain);

    public static EntryKey FieldKey(RenderContent content, string field) =>
        EntryKey.Named(EntryKey.TypeOwner(content.TypeId), field);

    // --- [CONTENT_MENUS]
    public static Func<IPlugInViews, Action<IDisposable>, IO<Unit>> AddMenuItem(Seq<ContentMenuRow> rows) =>
        (plugIn, _) => Callbacks.Each(rows.Map((row, order) =>
                from size in IconSlot.ContentMenu.Size()
                from pixels in HostTheme.Device(new(size, size))
                from icon in Icons.DrawingIcon(row.Icon.Glyph, row.Icon.Master, pixels)
                let site = new CallbackSite(plugIn, typeof(RenderContentMenu), row.Caption)
                from added in IO.lift(() => RenderContentMenu.AddMenuItem(
                    plugIn.PlugIn.Id,
                    RowText.Localize(row.Caption, table: Some<object>(plugIn)).Local,
                    order,
                    row.Separator,
                    row.TopLevel,
                    icon,
                    collection => Executed(row, collection, site),
                    (collection, context) => Enabled(row, collection, context, site)))
                select added))
            .Map(static _ => unit);

    private static Result Executed(ContentMenuRow row, RenderContentCollection collection, CallbackSite site) =>
        Callbacks.Answer(Conversions.ToResult(IO.lift(() => Conversions.Rows(collection)).Bind(row.Run)), static () => Result.Failure, site);

    private static bool Enabled(ContentMenuRow row, RenderContentCollection collection, RenderContentMenu.Context context, CallbackSite site) =>
        Callbacks.Answer(
            IO.lift(() => row.Contexts.Contains(context) && Conversions.Rows(collection) is { IsEmpty: false } selected && selected.ForAll(row.Scope.Covers)),
            static () => false,
            site);
}

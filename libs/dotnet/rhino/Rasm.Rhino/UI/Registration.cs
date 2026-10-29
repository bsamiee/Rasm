using System.Collections.Generic;
using Rasm.Rhino.Render.Views;
using Rasm.Rhino.UI.Rows;
using Rasm.Rhino.UI.Styling;
using Rhino;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.Rhino.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class ViewRegistration {
    // --- [FOLDS]
    private static IO<Unit> Each<TCase>(Func<TCase, IO<Unit>> register) where TCase : View =>
        IO.lift(static () => ViewCatalog.Current.Listed<TCase>()).Bind(rows => Products.Each(rows.Map(register))).Map(unit);

    private static IO<Unit> Hosted(View view, Action register) =>
        IO.lift(register).MapFail(error => error.HasException<ArgumentException>() ? new RhinoFault(view.Caption, RhinoReason.Unsupported, Inner: error) : error);

    private static string Caption(View view) => LocalText.Of(view.Caption).Local;

    // --- [PANELS]
    public static IO<Unit> RegisterPanels() =>
        Each<View.Panel>(static row => Hosted(row, () => Panels.RegisterPanel(
            Session.PlugIn, row.Identity, Caption(row), row.Icon.Map(static icon => icon.Assembly).ValueUnsafe(), row.Icon.Map(static icon => icon.Id).ValueUnsafe(), row.Kind.IfNone(PanelType.PerDoc))));

    // --- [PAGES]
    public static IO<Unit> OptionsDialogPages(ICollection<OptionsDialogPage> pages) => Pages(PageDialog.Options, None, pages);

    public static IO<Unit> DocumentPropertiesDialogPages(RhinoDoc doc, ICollection<OptionsDialogPage> pages) => Pages(PageDialog.DocumentProperties, Some(doc), pages);

    public static IO<Option<OptionsDialogPage>> RenderOptionsDialogPage(View.Page row, RhinoDoc doc) => Tree(row, Some(doc));

    public static IO<Unit> ObjectPropertiesPages(ObjectPropertiesPageCollection collection) =>
        Each<View.PropertiesPage>(row => IO.lift(() => collection.Add(new DefinedPropertiesPage(row))));

    private static IO<Unit> Pages(PageDialog dialog, Option<RhinoDoc> document, ICollection<OptionsDialogPage> pages) =>
        Each<View.Page>(row => when(row.Kind == dialog, Tree(row, document).Bind(page => IO.lift(() => page.Iter(pages.Add)))).As());

    private static IO<Option<OptionsDialogPage>> Tree(View.Page row, Option<RhinoDoc> document) =>
        from scope in RowScope.Open(document, new CommitMode.Immediate())
        from shown in CommandRows.Holds(row.Visible, scope)
        from page in shown
            ? from built in IO.lift(() => new DefinedOptionsPage(row, document))
              from children in row.SubPages.TraverseM(sub => Tree(sub, document)).As()
              from added in IO.lift(() => built.Children.AddRange(children.Somes()))
              select Some<OptionsDialogPage>(built)
            : IO.pure(Option<OptionsDialogPage>.None)
        select page;

    // --- [SECTIONS]
    public static IO<Unit> CustomSections(Seq<View.Section> rows, ICollection<ICollapsibleSection> sections) =>
        IO.lift(() => rows.Iter(row => sections.Add(new DefinedSection(row, row.Identity.GUID, expanded: true, section =>
            from document in SectionModels.Read(section, Provider.RhinoSettings, static settings => IO.lift(() => Conversions.Required(Optional(settings.ActiveView()).Map(static view => view.Document))))
            from scope in RowScope.Open(Some(document), new CommitMode.Immediate())
            from body in ViewBody.Realize(row, row.Actions, scope)
            select body))));

    // --- [RENDER_WINDOW]
    public static IO<Unit> RegisterRenderTabs(RenderTabs tabs) =>
        Each<View.Tab>(row =>
            from pixels in IconSlot.RenderTab.Pixels
            from icon in row.Icon.TraverseM(art => Icons.DrawingIcon(art, pixels)).As()
            from registered in Hosted(row, () => tabs.RegisterTab(Session.PlugIn, row.Identity, row.Scope.TabEngineId(), Caption(row), icon.ValueUnsafe()))
            select registered);

    public static IO<Unit> RegisterRenderPanels(RenderPanels panels) =>
        Each<View.Pane>(row => Hosted(row, () => panels.RegisterPanel(
            Session.PlugIn, RenderPanelType.RenderWindow, row.Identity, row.Scope.TabEngineId(), Caption(row), row.Scope.AlwaysShow, row.InitialShow, row.Position)));
}

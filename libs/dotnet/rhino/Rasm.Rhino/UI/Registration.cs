using System.Collections.Generic;
using Rasm.RhinoCommon.Commands;
using Rasm.RhinoCommon.Document.Files;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.Persistence.Stores;
using Rasm.RhinoCommon.PlugIns;
using Rasm.RhinoCommon.UI.Rows;
using Rasm.RhinoCommon.UI.Styling;
using Rasm.UI;
using Rhino;
using Rhino.Commands;
using Rhino.PlugIns;
using Rhino.Render;
using Rhino.UI;
using Rhino.UI.Controls;

namespace Rasm.RhinoCommon.UI;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ViewRegistration {
    // --- [FOLDS]
    private static IO<Unit> Each<TCase>(IPlugInViews plugIn, Func<TCase, IO<Unit>> register) where TCase : View =>
        IO.lift(() => plugIn.Views.Listed<TCase>()).Bind(rows => Callbacks.Each(rows.Map(register))).Map(unit);

    private static IO<Unit> Hosted(Type view, Action register) =>
        IO.lift(register).MapFail(error => error.HasException<ArgumentException>() ? new RhinoFault(view.Name, RhinoReason.Unheld, Inner: error) : error);

    // --- [PANELS]
    public static readonly Func<IPlugInViews, IO<Unit>> RegisterPanels =
        static plugIn => Each<View.Panel>(plugIn, row => (Caption: RowText.Localize(row.Caption, plugIn).Local, Kind: row.Kind.IfNone(PanelType.PerDoc)) switch {
            var face => row.Icon.Match(
                Some: glyph => glyph.Switch(
                    (PlugIn: plugIn, Row: row, Face: face, Glyph: glyph),
                    tinted: static (at, _) => Drawn(at.PlugIn, at.Row, at.Glyph, at.Face.Caption, at.Face.Kind),
                    paired: static (at, art) => Hosted(at.Row.Identity, () => Panels.RegisterPanel(at.PlugIn.PlugIn, at.Row.Identity, at.Face.Caption, art.Assembly, art.Light, at.Face.Kind))),
                None: () => Hosted(row.Identity, () => Panels.RegisterPanel(plugIn.PlugIn, row.Identity, face.Caption, iconAssembly: null, iconResourceId: null, face.Kind))),
        });

    private static IO<Unit> Drawn(IPlugInViews plugIn, View.Panel row, GlyphSource glyph, string caption, PanelType kind) =>
        from pixels in IconSlot.PanelTab.Pixels
        from icon in Icons.DrawingIcon(glyph, pixels)
        from registered in Hosted(row.Identity, () => Panels.RegisterPanel(plugIn.PlugIn, row.Identity, caption, icon, kind))
        select registered;

    // --- [PAGES]
    public static IO<Unit> OptionsDialogPages(IPlugInViews plugIn, ICollection<OptionsDialogPage> pages) =>
        Each<View.OptionsPage>(plugIn, row => Tree(plugIn, row, static held => held.SubPages, static held => held.Create, None).Bind(page => IO.lift(() => page.Iter(pages.Add))));

    public static IO<Unit> DocumentPropertiesDialogPages(IPlugInViews plugIn, RhinoDoc document, ICollection<OptionsDialogPage> pages) =>
        Each<View.DocumentPage>(plugIn, row => Tree(plugIn, row, static held => held.SubPages, static held => held.Create, Some(document)).Bind(page => IO.lift(() => page.Iter(pages.Add))));

    public static IO<Option<OptionsDialogPage>> RenderOptionsDialogPage(IPlugInViews plugIn, View.DocumentPage row, RhinoDoc document) =>
        Tree(plugIn, row, static held => held.SubPages, static held => held.Create, Some(document));

    public static IO<Unit> ObjectPropertiesPages(IPlugInViews plugIn, ObjectPropertiesPageCollection collection) =>
        Each<View.PropertiesPage>(plugIn, row => IO.lift(() => row.Create(plugIn)).Bind(page => IO.lift(() => collection.Add(page))));

    private static IO<Option<OptionsDialogPage>> Tree<TCase>(
        IPlugInViews plugIn, TCase row, Func<TCase, Seq<TCase>> subPages, Func<TCase, Func<IPlugInViews, DefinedOptionsPage>> create, Option<RhinoDoc> document) where TCase : View =>
        from scope in RowScope.Open(plugIn, document, new CommitMode.Immediate())
        from shown in CommandRows.Holds(row.Visible, scope)
        from page in shown
            ? from built in IO.lift(() => create(row)(plugIn))
              from _ in document.TraverseM(built.ForDocument).As()
              from children in subPages(row).TraverseM(sub => Tree(plugIn, sub, subPages, create, document)).As()
              from __ in IO.lift(() => built.Children.AddRange(children.Somes()))
              select Some<OptionsDialogPage>(built)
            : IO.pure(Option<OptionsDialogPage>.None)
        select page;

    // --- [RENDER_WINDOW]
    public static IO<Unit> RegisterRenderTabs<TPlugIn>(TPlugIn plugIn, RenderTabs tabs) where TPlugIn : PlugIn, IPlugInViews =>
        Each<View.Tab>(plugIn, row =>
            from pixels in IconSlot.RenderTab.Pixels
            from icon in row.Icon.TraverseM(glyph => Icons.DrawingIcon(glyph, pixels)).As()
            from registered in Hosted(row.Identity, () => tabs.RegisterTab(
                plugIn, row.Identity, row.Scope.TabEngineId(plugIn), RowText.Localize(row.Caption, plugIn).Local, icon.ValueUnsafe()))
            select registered);

    public static IO<Unit> RegisterRenderPanels<TPlugIn>(TPlugIn plugIn, RenderPanels panels) where TPlugIn : PlugIn, IPlugInViews =>
        Each<View.Pane>(plugIn, row => Hosted(row.Identity, () => panels.RegisterPanel(
            plugIn, RenderPanelType.RenderWindow, row.Identity, row.Scope.TabEngineId(plugIn), RowText.Localize(row.Caption, plugIn).Local,
            row.Scope.AlwaysShow, row.InitialShow, row.Position)));

    // --- [CUSTOM_SECTIONS]
    public static IO<Unit> CustomSections(IPlugInViews plugIn, Seq<View.Section> rows, ICollection<ICollapsibleSection> sections) =>
        Documents.WithDocument(new DocumentSource.Active(), document =>
            from scope in RowScope.Open(plugIn, Some(document), new CommitMode.Immediate())
            from _ in Callbacks.Each(rows.Map(row => SectionHolder.Section(plugIn, row, scope).Bind(section => IO.lift(() => sections.Add(section)))))
            select unit);

    // --- [CONTENT_SECTIONS]
    public static readonly Func<IPlugInViews, IO<Unit>> AddCustomUISections =
        static plugIn => EventKind.OnAddCustomUISections.Inline(
            args => Each<View.Section>(plugIn, row => row.Create.Match(
                Left: static _ => IO.pure(unit),
                Right: content => IO.lift(() => args.ExpandableContentUI.AddSection(content(row, plugIn))))),
            plugIn);

    public static EntryKey FieldKey(RenderContent content, string field) =>
        EntryKey.Named(EntryKey.TypeOwner(content.TypeId), field);

    // --- [CONTENT_MENUS]
    public static Func<IPlugInViews, IO<Unit>> AddMenuItem(Seq<ContentMenuRow> rows) =>
        plugIn => Callbacks.Each(rows.Map((row, order) =>
                from pixels in IconSlot.Small.Pixels
                from icon in Icons.DrawingIcon(row.Icon, pixels)
                from added in IO.lift(() => RenderContentMenu.AddMenuItem(
                    plugIn.PlugIn.Id,
                    RowText.Localize(row.Caption, plugIn).Local,
                    order,
                    row.Separator,
                    row.TopLevel,
                    icon,
                    collection => Executed(row, collection, plugIn),
                    (collection, context) => Enabled(row, collection, context, plugIn)))
                select added))
            .Map(unit);

    private static Result Executed(ContentMenuRow row, RenderContentCollection collection, IPlugInLifetime owner) =>
        owner.Answer(Results.Of(IO.lift(() => Conversions.Rows(collection)).Bind(row.Run)), Result.Failure);

    private static bool Enabled(ContentMenuRow row, RenderContentCollection collection, RenderContentMenu.Context context, IPlugInLifetime owner) =>
        owner.Answer(IO.lift(() => row.Contexts.Contains(context) && Conversions.Rows(collection) is { IsEmpty: false } selected && selected.ForAll(row.Scope.Covers)), failed: false);
}

using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ViewportTarget {
    public sealed record Active() : ViewportTarget;

    public sealed record Named(string Name) : ViewportTarget;

    public sealed record Id(Guid ViewportId) : ViewportTarget;

    public sealed record Serial(uint ViewSerial) : ViewportTarget;

    public sealed record Page(Guid PageViewId) : ViewportTarget;

    public sealed record Detail(Guid PageViewId, Guid DetailId) : ViewportTarget;

    public sealed record Every(ViewTypeFilter Filter, bool Details) : ViewportTarget;

    public sealed record Group(ComponentRef Address) : ViewportTarget;
}

public readonly record struct ViewportRef(RhinoView View, RhinoViewport Viewport, Option<DetailViewObject> Detail);

public readonly record struct ViewportIdentity(Guid Id, string Name, uint ViewSerial);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Viewports {
    // --- [RESOLUTION]
    public static IO<Seq<ViewportRef>> ResolveViewports(RhinoDoc doc, ViewportTarget target) =>
        target.Switch(
            doc,
            active: static (document, _) => IO.lift(() =>
                Optional(document.Views.ActiveView)
                    .Map(static view => Seq(new ViewportRef(view, view.ActiveViewport, Option<DetailViewObject>.None)))
                    .ToFin(new NoActiveView())),
            named: static (document, named) => IO.lift(() =>
                Missing.Unless(document.Views.Find(named.Name, compareCase: false), nameof(ViewTable.Find))
                    .Map(static view => Seq(Main(view)))),
            id: static (document, id) => IO.lift(() => Answers.NonEmpty(
                Optional(document.Views.Find(id.ViewportId)).Match(
                    Some: static view => Seq(Main(view)),
                    None: () => Addressed(toSeq(document.Views.GetPageViews()), id.ViewportId).Map(DetailRef).Strict()),
                nameof(ViewTable.Find))),
            serial: static (_, serial) => IO.lift(() =>
                Missing.Unless(RhinoView.FromRuntimeSerialNumber(serial.ViewSerial), nameof(RhinoView.FromRuntimeSerialNumber))
                    .Map(static view => Seq(Main(view)))),
            page: static (document, page) => IO.lift(() =>
                Pages(document, page.PageViewId).Map(static pages => pages.Map(Main).Strict())),
            detail: static (document, detail) =>
                from pages in IO.lift(() => Pages(document, detail.PageViewId))
                from rows in IO.lift(() => Answers.NonEmpty(Addressed(pages, detail.DetailId).Map(DetailRef).Strict(), nameof(RhinoPageView.GetDetailViews)))
                select rows,
            every: static (document, every) =>
                from views in IO.lift(() => toSeq(document.Views.GetViewList(every.Filter)))
                from rows in IO.lift(() => Answers.NonEmpty(
                    views.Map(Main) + (every.Details ? Details(views.Choose(static view => Optional(view as RhinoPageView))) : Seq<ViewportRef>()),
                    nameof(ViewTable.GetViewList)))
                select rows,
            group: static (document, grouped) =>
                from address in TableOps.Find<PageViewGroup>(
                    grouped.Address,
                    id => Optional(document.PageViewGroups.FindId(id)),
                    index => Optional(document.PageViewGroups.FindIndex(index)),
                    name => Optional(document.PageViewGroups.FindName(name)))
                from found in IO.lift(() => address.ToFin(new Missing(nameof(PageViewGroupTable))))
                from rows in IO.lift(() => Answers.NonEmpty(
                    toSeq(document.Views.GetPageViews()).Filter(page => page.IsInPageViewGroup(found.Index)).Map(Main).Strict(),
                    nameof(RhinoPageView.IsInPageViewGroup)))
                select rows);

    public static IO<ViewportRef> ResolveViewport(RhinoDoc doc, ViewportTarget target) =>
        from rows in ResolveViewports(doc, target)
        from row in IO.lift<ViewportRef>(() => rows is [var only] ? only : new Ambiguous(nameof(ResolveViewports), rows.Count))
        select row;

    public static IO<RhinoPageView> ResolvePage(RhinoDoc doc, Guid pageViewId) =>
        IO.lift(() => Pages(doc, pageViewId).Bind(static pages => pages is [var only] ? Fin.Succ(only) : new Ambiguous(nameof(ResolvePage), pages.Count)));

    public static IO<DetailViewObject> ResolveDetail(RhinoDoc doc, Guid pageViewId, Guid detailId) =>
        IO.lift(() =>
            from pages in Pages(doc, pageViewId)
            from rows in Answers.NonEmpty(Addressed(pages, detailId), nameof(RhinoPageView.GetDetailViews))
            from detail in rows is [var only] ? Fin.Succ(only.Detail) : new Ambiguous(nameof(ResolveDetail), rows.Count)
            select detail);

    public static ViewportIdentity Identity(RhinoView view, RhinoViewport viewport) =>
        new(viewport.Id, viewport.Name, view.RuntimeSerialNumber);

    private static ViewportRef Main(RhinoView view) =>
        new(view, view.MainViewport, Option<DetailViewObject>.None);

    private static ViewportRef DetailRef((RhinoPageView Page, DetailViewObject Detail) row) =>
        new(row.Page, row.Detail.Viewport, Some(row.Detail));

    private static Seq<(RhinoPageView Page, DetailViewObject Detail)> DetailViews(Seq<RhinoPageView> pages) =>
        (from page in pages
         from detail in toSeq(page.GetDetailViews())
         select (Page: page, Detail: detail)).Strict();

    private static Seq<ViewportRef> Details(Seq<RhinoPageView> pages) =>
        DetailViews(pages).Map(DetailRef).Strict();

    private static Seq<(RhinoPageView Page, DetailViewObject Detail)> Addressed(Seq<RhinoPageView> pages, Guid id) =>
        DetailViews(pages).Filter(row => (row.Detail.Viewport.Id == id) || (row.Detail.Id == id));

    private static Fin<Seq<RhinoPageView>> Pages(RhinoDoc doc, Guid pageViewId) =>
        Answers.NonEmpty(toSeq(doc.Views.GetPageViews()).Filter(page => page.MainViewport.Id == pageViewId).Strict(), nameof(ViewTable.GetPageViews));

    // --- [SCOPES]
    public static IO<TValue> WithMode<TValue>(Guid id, Func<DisplayModeDescription, IO<TValue>> body) =>
        Disposal.Using(IO.lift(() => Missing.Unless(DisplayModeDescription.GetDisplayMode(id), nameof(DisplayModeDescription.GetDisplayMode))), body);

    public static IO<TValue> WithActiveView<TValue>(RhinoDoc doc, RhinoView view, IO<TValue> body) =>
        Disposal.Bracketed(
            from prior in IO.lift(() => Optional(doc.Views.ActiveView).ToFin(new NoActiveView()))
            from activated in IO.lift(() => { doc.Views.ActiveView = view; })
            select prior,
            prior => IO.lift(() => { doc.Views.ActiveView = prior; }),
            _ => body);
}

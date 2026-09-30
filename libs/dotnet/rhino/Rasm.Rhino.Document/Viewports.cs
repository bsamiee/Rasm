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

public readonly record struct ViewportIdentity(Guid Id, string Name, uint ViewSerial);

// --- [SERVICES] ------------------------------------------------------------------------
public readonly record struct ViewportRef(RhinoView View, RhinoViewport Viewport, Option<DetailViewObject> Detail) : IDisposable {
    public void Dispose() {
        if (Detail.IsSome)
            Viewport.Dispose();
    }
}

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
            page: static (document, page) => IO.lift(() => Page(document, page.PageViewId).Map(static found => Seq(Main(found)))),
            detail: static (document, detail) =>
                from page in IO.lift(() => Page(document, detail.PageViewId))
                from rows in IO.lift(() => Answers.NonEmpty(Addressed(Seq(page), detail.DetailId).Map(DetailRef).Strict(), nameof(RhinoPageView.GetDetailViews)))
                select rows,
            every: static (document, every) =>
                from views in IO.lift(() => toSeq(document.Views.GetViewList(every.Filter)))
                from rows in IO.lift(() => Answers.NonEmpty(
                    views.Map(Main) + (every.Details ? Details(views.Choose(static view => Optional(view as RhinoPageView))) : Seq<ViewportRef>()),
                    nameof(ViewTable.GetViewList)))
                select rows,
            group: static (document, grouped) =>
                from found in TableOps.Find(document.PageViewGroups, grouped.Address, includeDeleted: false)
                from rows in IO.lift(() => Answers.NonEmpty(
                    toSeq(document.Views.GetPageViews()).Filter(page => page.IsInPageViewGroup(found.Index)).Map(Main).Strict(),
                    nameof(RhinoPageView.IsInPageViewGroup)))
                select rows);

    public static IO<ViewportRef> ResolveViewport(RhinoDoc doc, ViewportTarget target) =>
        from rows in ResolveViewports(doc, target)
        from row in DisposalOps.OnFailure(IO.lift<ViewportRef>(() => rows is [var only] ? only : new Ambiguous(nameof(ResolveViewports), rows.Count)), DisposalOps.Release(rows))
        select row;

    public static IO<RhinoPageView> ResolvePage(RhinoDoc doc, Guid pageViewId) =>
        IO.lift(() => Page(doc, pageViewId));

    public static IO<DetailViewObject> ResolveDetail(RhinoDoc doc, Guid pageViewId, Guid detailId) =>
        IO.lift(() => Page(doc, pageViewId).Bind(page => Addressed(Seq(page), detailId).Head.Map(static row => row.Detail).ToFin(new Missing(nameof(RhinoPageView.GetDetailViews)))));

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
        DetailViews(pages).Filter(row => (row.Detail.Id == id) || (ViewportId(row.Detail) == id)).Strict();

    private static Guid ViewportId(DetailViewObject detail) {
        using RhinoViewport viewport = detail.Viewport;
        return viewport.Id;
    }

    private static Fin<RhinoPageView> Page(RhinoDoc doc, Guid pageViewId) =>
        Missing.Unless(doc.Views.Find(pageViewId) as RhinoPageView, nameof(ViewTable.Find));

    // --- [SCOPES]
    public static IO<TValue> WithMode<TValue>(Guid id, Func<DisplayModeDescription, IO<TValue>> body) =>
        DisposalOps.Using(IO.lift(() => Missing.Unless(DisplayModeDescription.GetDisplayMode(id), nameof(DisplayModeDescription.GetDisplayMode))), body);

    public static IO<TValue> WithActiveView<TValue>(RhinoDoc doc, RhinoView view, IO<TValue> body) =>
        (from prior in IO.lift(() => Optional(doc.Views.ActiveView).ToFin(new NoActiveView()))
         from activated in IO.lift(() => { doc.Views.ActiveView = view; })
         select prior)
        .Bracket(Use: _ => body, Fin: prior => IO.lift(() => { doc.Views.ActiveView = prior; }));
}

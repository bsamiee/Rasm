using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rasm.Rhino.Document.Tables;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ViewportTarget {
    public sealed record Active() : ViewportTarget;

    public sealed record Named(string Name) : ViewportTarget;

    public sealed record Id(Guid ViewportId) : ViewportTarget;

    public sealed record Serial(uint ViewSerial) : ViewportTarget;

    public sealed record Detail(Guid DetailId) : ViewportTarget;
}

[Union]
public abstract partial record ViewportSet {
    public sealed record One(ViewportTarget Target) : ViewportSet;

    public sealed record Every(ViewTypeFilter Filter, bool Details) : ViewportSet;

    public sealed record Group(ComponentRef<PageViewGroup> Address) : ViewportSet;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record ViewportRef(RhinoView View, Option<DetailViewObject> Detail) : IDisposable {
    public RhinoViewport Viewport => Detail.Case is DetailViewObject detail ? detail.Viewport : View.MainViewport;

    public Option<bool> CommitViewportChanges() => Detail.Map(static detail => detail.CommitViewportChanges());

    public void Dispose() => Viewport.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Viewports {
    // --- [RESOLUTION]
    public static IO<ViewportRef> ResolveViewport(RhinoDoc doc, ViewportTarget target) =>
        target.Switch(
            doc,
            active: static (document, _) => IO.lift(() =>
                NoActiveView.Unless(document.Views.ActiveView)
                    .Map(static view => new ViewportRef(view, Optional((view as RhinoPageView)?.ActiveDetail)))),
            named: static (document, named) => IO.lift(() =>
                Missing.Unless(document.Views.Find(named.Name, compareCase: false), nameof(ViewTable.Find))
                    .Map(static view => new ViewportRef(view, None))),
            id: static (document, id) => IO.lift(() =>
                (Optional(document.Views.Find(id.ViewportId)).Map(static view => new ViewportRef(view, None))
                 || toSeq(document.Views.GetPageViews()).Bind(Details).Find(row => {
                     using RhinoViewport viewport = row.Viewport;
                     return viewport.Id == id.ViewportId;
                 }))
                    .ToFin(new Missing(nameof(ViewTable.Find)))),
            serial: static (document, serial) => IO.lift(() =>
                Optional(RhinoView.FromRuntimeSerialNumber(serial.ViewSerial))
                    .Filter(view => view.Document?.RuntimeSerialNumber == document.RuntimeSerialNumber)
                    .Map(static view => new ViewportRef(view, None))
                    .ToFin(new Missing(nameof(RhinoView.FromRuntimeSerialNumber)))),
            detail: static (document, detail) =>
                from found in ResolveDetail(document, detail.DetailId)
                from page in IO.lift(() => Missing.Unless(found.ParentPageView, nameof(DetailViewObject.ParentPageView)))
                select new ViewportRef(page, Some(found)));

    public static IO<Seq<ViewportRef>> ResolveViewports(RhinoDoc doc, ViewportSet set) =>
        set.Switch(
            doc,
            one: static (document, one) => ResolveViewport(document, one.Target).Map(static row => Seq(row)),
            every: static (document, every) => IO.lift(() => {
                Seq<RhinoView> views = toSeq(document.Views.GetViewList(every.Filter));
                return Conversions.NonEmpty(
                    views.Filter(static view => view is not RhinoPageView).Map(static view => new ViewportRef(view, None))
                    + Pages(views.Choose(static view => Optional(view as RhinoPageView)), every.Details), nameof(ViewTable.GetViewList));
            }),
            group: static (document, grouped) =>
                from found in TableOps.Find(document.PageViewGroups, grouped.Address, includeDeleted: false)
                from rows in IO.lift(() => Conversions.NonEmpty(
                    Pages(toSeq(found.GetMembers()), details: false),
                    nameof(PageViewGroup.GetMembers)))
                select rows);

    public static IO<RhinoPageView> ResolvePage(RhinoDoc doc, Guid pageViewId) =>
        IO.lift(() => Missing.Unless(doc.Views.Find(pageViewId), nameof(ViewTable.Find))
            .Bind(static view => WrongType.Unless<RhinoPageView>(view)));

    public static IO<DetailViewObject> ResolveDetail(RhinoDoc doc, Guid detailId) =>
        IO.lift(() => Missing.Unless(doc.Objects.FindId(detailId), nameof(ObjectTable.FindId))
            .Bind(static found => WrongType.Unless<DetailViewObject>(found)));

    private static Seq<ViewportRef> Pages(Seq<RhinoPageView> pages, bool details) =>
        toSeq(pages.OrderBy(static page => page.PageNumber))
            .Bind(page => new ViewportRef(page, None).Cons(details ? Details(page) : Seq<ViewportRef>())).Strict();

    private static Seq<ViewportRef> Details(RhinoPageView page) =>
        toSeq(page.GetDetailViews()).Map(detail => new ViewportRef(page, Some(detail)));

    // --- [SCOPES]
    public static IO<TValue> WithMode<TValue>(Guid id, Func<DisplayModeDescription, IO<TValue>> body) =>
        use(IO.lift(() => Missing.Unless(DisplayModeDescription.GetDisplayMode(id), nameof(DisplayModeDescription.GetDisplayMode)))).Bind(body).Bracket();

    public static IO<TValue> WithActiveView<TValue>(RhinoDoc doc, RhinoView view, IO<TValue> body) =>
        (from prior in IO.lift(() => NoActiveView.Unless(doc.Views.ActiveView))
         from activated in IO.lift(() => { doc.Views.ActiveView = view; })
         select prior)
        .Bracket(Use: _ => body, Fin: prior => IO.lift(() => { doc.Views.ActiveView = prior; }));
}

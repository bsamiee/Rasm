using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Modeling;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Viewport;

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
public sealed class ViewportRef(RhinoView view, Option<DetailViewObject> detail) : IDisposable {
    public RhinoView View { get; } = view;

    public Option<DetailViewObject> Detail { get; } = detail;

    public RhinoViewport Viewport { get; } = detail.Case is DetailViewObject found ? found.Viewport : view.MainViewport;

    public Fin<Unit> CommitViewportChanges() =>
        Detail.Case is DetailViewObject detail ? Refused.Unless(detail.CommitViewportChanges(), nameof(DetailViewObject.CommitViewportChanges)) : unit;

    public void Dispose() => Viewport.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Viewports {
    // --- [RESOLUTION]
    public static IO<ViewportRef> ResolveViewport(RhinoDoc doc, ViewportTarget target, Action<IDisposable> retain) =>
        Copies.Owned(
            () => target.Switch(
                doc,
                active: static (document, _) => Missing.Unless(document.Views.ActiveView, nameof(ViewTable.ActiveView))
                    .Map(static view => new ViewportRef(view, Optional((view as RhinoPageView)?.ActiveDetail))),
                named: static (document, named) => Missing.Unless(document.Views.Find(named.Name, compareCase: false), nameof(ViewTable.Find))
                    .Map(static view => new ViewportRef(view, None)),
                id: static (document, id) => (Optional(document.Views.Find(id.ViewportId)).Map(static view => new ViewportRef(view, None))
                    || Pages(toSeq(document.Views.GetPageViews()), details: true).Filter(static row => row.Detail.IsSome).Find(row => {
                        using ViewInfo view = new(row.Viewport);
                        return view.Viewport.Id == id.ViewportId;
                    })).ToFin(new Missing(nameof(ViewTable.Find))),
                serial: static (document, serial) => Optional(RhinoView.FromRuntimeSerialNumber(serial.ViewSerial))
                    .Filter(view => view.Document?.RuntimeSerialNumber == document.RuntimeSerialNumber)
                    .Map(static view => new ViewportRef(view, None))
                    .ToFin(new Missing(nameof(RhinoView.FromRuntimeSerialNumber))),
                detail: static (document, detail) =>
                    from found in Missing.Unless(document.Objects.FindId(detail.DetailId), nameof(ObjectTable.FindId))
                    from detailView in WrongType.Unless<DetailViewObject>(found)
                    from page in Missing.Unless(detailView.ParentPageView, nameof(DetailViewObject.ParentPageView))
                    select new ViewportRef(page, Some(detailView))),
            static answer => answer,
            static answer => IO.lift(() => { _ = answer.IfSucc(static row => row.Dispose()); }),
            retain);

    public static IO<Seq<ViewportRef>> ResolveViewports(RhinoDoc doc, ViewportSet set, Action<IDisposable> retain) =>
        set.Switch(
            (Document: doc, Retain: retain),
            one: static (state, one) => ResolveViewport(state.Document, one.Target, state.Retain).Map(static row => Seq(row)),
            every: static (state, every) => Copies.Owned(
                () => toSeq(state.Document.Views.GetViewList(every.Filter)) switch {
                    var views => views.Filter(static view => view is not RhinoPageView).Map(static view => new ViewportRef(view, None))
                        + Pages(views.Choose(static view => Optional(view as RhinoPageView)), every.Details),
                },
                static rows => Conversions.NonEmpty(rows, nameof(ViewTable.GetViewList)),
                DisposalOps.Release,
                state.Retain),
            group: static (state, grouped) => TableOps.Find(state.Document.PageViewGroups, grouped.Address, includeDeleted: false)
                .Bind(found => Copies.Owned(() => Pages(toSeq(found.GetMembers()), details: false),
                    static rows => Conversions.NonEmpty(rows, nameof(PageViewGroup.GetMembers)), DisposalOps.Release, state.Retain)));

    public static IO<RhinoPageView> ResolvePage(RhinoDoc doc, Guid pageViewId) =>
        IO.lift(() => Missing.Unless(doc.Views.Find(pageViewId), nameof(ViewTable.Find))
            .Bind(static view => WrongType.Unless<RhinoPageView>(view)));

    public static IO<DetailViewObject> ResolveDetail(RhinoDoc doc, Guid detailId) =>
        IO.lift(() => Missing.Unless(doc.Objects.FindId(detailId), nameof(ObjectTable.FindId))
            .Bind(static found => WrongType.Unless<DetailViewObject>(found)));

    private static Seq<ViewportRef> Pages(Seq<RhinoPageView> pages, bool details) =>
        toSeq(pages.OrderBy(static page => page.PageNumber))
            .Bind(page => new ViewportRef(page, None).Cons(details ? toSeq(page.GetDetailViews()).Map(detail => new ViewportRef(page, Some(detail))) : Seq<ViewportRef>()))
            .Strict();

    // --- [SCOPES]
    public static IO<TValue> WithMode<TValue>(Guid id, Func<DisplayModeDescription, IO<TValue>> body) =>
        DisposalOps.Within(() => DisplayModeDescription.GetDisplayMode(id),
            mode => IO.lift(() => Missing.Unless(mode, nameof(DisplayModeDescription.GetDisplayMode))).Bind(body),
            static mode => Optional(mode).Traverse(static held => IO.lift(held.Dispose)).As().Map(static _ => unit));

    public static IO<TValue> WithActiveView<TValue>(RhinoDoc doc, RhinoView view, IO<TValue> body) =>
        DisposalOps.Within((Action<IDisposable> retain) =>
            from prior in IO.lift(() => Missing.Unless(doc.Views.ActiveView, nameof(ViewTable.ActiveView)))
            from activated in DisposalOps.Change(() => prior, held => doc.Views.ActiveView = held, view, retain)
            from value in body
            select value,
            DisposalOps.Release);
}

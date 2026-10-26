using Rasm.Rhino.Display;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Events;
using Rasm.Rhino.Modeling;
using Rasm.Rhino.Persistence.Stores;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Rhino.UI;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ViewportTarget {
    public sealed record Active() : ViewportTarget;

    public sealed record Named(string Name) : ViewportTarget;

    public sealed record Id(Guid ViewportId) : ViewportTarget;

    public sealed record Serial(uint ViewSerial) : ViewportTarget;
}

[Union]
public abstract partial record ViewportSet {
    public sealed record One(ViewportTarget Target) : ViewportSet;

    public sealed record Every(ViewTypeFilter Filter, bool Details) : ViewportSet;

    public sealed record Group(ComponentRef<PageViewGroup> Address, bool Details) : ViewportSet;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ViewportRef : IDisposable {
    private readonly Option<DetailViewObject> pending;

    internal ViewportRef(RhinoView view, RhinoViewport viewport, Option<DetailViewObject> pending) {
        View = view;
        Viewport = viewport;
        this.pending = pending;
    }

    public RhinoView View { get; }

    public RhinoViewport Viewport { get; }

    internal Fin<Unit> Commit() =>
        pending.Case is DetailViewObject detail ? Refused.Unless(detail.CommitViewportChanges(), nameof(DetailViewObject.CommitViewportChanges)) : unit;

    public void Dispose() => Viewport.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Viewports {
    // --- [RESOLUTION]
    public static IO<ViewportRef> Resolve(RhinoDoc doc, ViewportTarget target, Action<IDisposable> retain) =>
        Copies.Owned(
            () => target.Switch(
                doc,
                active: static (document, _) => Refused.UnlessFound(document.Views.ActiveView, nameof(ViewTable.ActiveView))
                    .Map(static view => new ViewportRef(view, view.ActiveViewport, None)),
                named: static (document, named) => Refused.UnlessFound(document.Views.Find(named.Name, compareCase: false), nameof(ViewTable.Find))
                    .Map(static view => new ViewportRef(view, view.MainViewport, None)),
                id: static (document, id) => document.Objects.FindId(id.ViewportId) is DetailViewObject detail
                    ? Refused.UnlessFound(detail.ParentPageView, nameof(DetailViewObject.ParentPageView)).Map(page => Detailed(page, detail))
                    : Refused.UnlessFound(document.Views.Find(id.ViewportId), nameof(ViewTable.Find)).Map(static view => new ViewportRef(view, view.MainViewport, None)),
                serial: static (document, serial) => Optional(RhinoView.FromRuntimeSerialNumber(serial.ViewSerial))
                    .Filter(view => view.Document?.RuntimeSerialNumber == document.RuntimeSerialNumber)
                    .Map(static view => new ViewportRef(view, view.MainViewport, None))
                    .ToFin(new Refused(nameof(RhinoView.FromRuntimeSerialNumber), Refused.Absent))),
            static answer => answer,
            static answer => IO.lift(() => { _ = answer.IfSucc(static row => row.Dispose()); }),
            retain);

    public static IO<Seq<ViewportRef>> Resolve(RhinoDoc doc, ViewportSet set, Action<IDisposable> retain) =>
        set.Switch(
            (Document: doc, Retain: retain),
            one: static (state, one) => Resolve(state.Document, one.Target, state.Retain).Map(static row => Seq(row)),
            every: static (state, every) => Copies.Owned(
                () => toSeq(state.Document.Views.GetViewList(every.Filter)) switch {
                    var views => views.Filter(static view => view is not RhinoPageView).Map(static view => new ViewportRef(view, view.MainViewport, None))
                        + Rows(Pages(views.Choose(static view => Optional(view as RhinoPageView))), every.Details),
                },
                static rows => IO.pure(rows),
                DisposalOps.Release,
                state.Retain),
            group: static (state, grouped) => TableOps.Find(state.Document.PageViewGroups, grouped.Address, includeDeleted: false)
                .Bind(found => Copies.Owned(() => Rows(Pages(found.GetMembers()), grouped.Details), static rows => IO.pure(rows), DisposalOps.Release, state.Retain)));

    public static IO<T> With<T>(RhinoDoc doc, ViewportTarget target, Func<ViewportRef, IO<T>> body) =>
        DisposalOps.Within<IDisposable, T>(retain => Resolve(doc, target, retain).Bind(body), DisposalOps.Release);

    internal static Seq<RhinoPageView> Pages(IEnumerable<RhinoPageView> pages) =>
        toSeq(pages.OrderBy(static page => page.PageNumber)).Strict();

    private static Seq<ViewportRef> Rows(Seq<RhinoPageView> pages, bool details) =>
        pages.Bind(page => new ViewportRef(page, page.MainViewport, None)
            .Cons(details ? toSeq(page.GetDetailViews()).Map(detail => Detailed(page, detail)) : Seq<ViewportRef>())).Strict();

    private static ViewportRef Detailed(RhinoPageView page, DetailViewObject detail) =>
        detail.IsActive ? new ViewportRef(page, page.ActiveViewport, None) : new ViewportRef(page, detail.Viewport, Some(detail));

    // --- [EDITS]
    public static IO<Seq<bool>> Edit(RhinoDoc doc, ViewportSet set, Func<RhinoViewport, IO<Unit>> edit, RedrawPolicy redraw) =>
        Commits.WithinRedraw(doc, redraw, DisposalOps.Within((Action<IDisposable> retain) =>
            Resolve(doc, set, retain).Bind(rows => Callbacks.Each(rows.Map(row =>
                from before in IO.lift(() => row.Viewport.ChangeCounter)
                from edited in edit(row.Viewport)
                from moved in IO.lift(() => row.Viewport.ChangeCounter != before)
                from committed in IO.lift(row.Commit)
                select moved))),
            DisposalOps.Release));

    // --- [SIGNALS]
    public static HostEvent<Guid> Changed(RhinoDoc doc) =>
        Subscriptions.Host(typeof(Viewports), () => doc.RuntimeSerialNumber switch {
            var serial => Seq(EventKind.RhinoViewRename, EventKind.RhinoViewSetActive, EventKind.RhinoViewModified)
                    .Map(row => row.In(serial).Choose(static args => Some(args.View.ActiveViewportID)))
                + Seq(
                    EventKind.ViewportProjectionChanged.In(serial).Choose(static args => Some(args.Viewport.Id)),
                    EventKind.DisplayModeChanged.In(serial).Choose(DisplayModes.Switched),
                    EventKind.PageViewSpaceChange.In(serial).Choose(static args => Conversions.Present(args.NewActiveDetailId) || Optional(args.PageView).Map(static page => page.MainViewport.Id)),
                    EventKind.PageViewPropertiesChange.In(serial).Choose(static args => Optional(args.PageView).Map(static page => page.MainViewport.Id))),
        });

    internal static HostEvent<Unit> Changed(RhinoDoc doc, ViewportTarget target) =>
        Changed(doc).Choose(id => Callbacks.Found(
            target.Switch(id, active: static (_, _) => true, named: static (_, _) => true, id: static (changed, held) => held.ViewportId == changed, serial: static (_, _) => true), unit));

    public static HostEvent<RhinoView> Framed(RhinoDoc doc) =>
        EventKind.DrawForeground.In(doc.RuntimeSerialNumber)
            .Choose(static args => Optional(args.Viewport.ParentView))
            .DistinctUntilChanged(static view => (view.RuntimeSerialNumber, view.ScreenRectangle));

    // --- [STORES]
    public static ValueStore<T> Member<T>(
        RhinoDoc doc, ViewportTarget target, Func<RhinoViewport, IO<T>> read, Func<RhinoViewport, T, IO<Unit>> write, Option<Func<T, T, bool>> same = default) where T : notnull =>
        ValueStore.Of(
            With(doc, target, row => read(row.Viewport).Map(static value => Some(value))),
            value => value.Traverse(held => Edit(doc, new ViewportSet.One(target), port => write(port, held), new RedrawPolicy.AllViews(Deferred: true))).As().Map(unit),
            Applied.Live,
            Some(Changed(doc, target)),
            same);

    // --- [SCREEN]
    public static IO<Eto.Drawing.RectangleF> Frame(RhinoView view) =>
        from window in IO.lift(() => Refused.UnlessFound(RhinoEtoApp.MainWindowForDocument(view.Document), nameof(RhinoEtoApp.MainWindowForDocument)))
        from frame in IO.lift(() => view.ScreenRectangle)
        select new Eto.Drawing.RectangleF(frame.X, frame.Y, frame.Width, frame.Height) / window.LogicalPixelSize;

    // --- [SPACE]
    public static LengthUnit Units(RhinoDoc doc, Option<RhinoViewport> viewport) =>
        viewport.Exists(static found => found.ViewportType is ViewportType.PageViewMainViewport) ? doc.PageUnits : doc.ModelUnits;
}

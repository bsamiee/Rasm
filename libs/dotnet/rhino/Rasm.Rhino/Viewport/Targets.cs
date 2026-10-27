using System.Collections.Generic;
using Eto.Drawing;
using Rasm.RhinoCommon.Display;
using Rasm.RhinoCommon.Document;
using Rasm.RhinoCommon.Document.Tables;
using Rasm.RhinoCommon.Events;
using Rasm.RhinoCommon.Persistence.Stores;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.UI;

namespace Rasm.RhinoCommon.Viewport;

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
        pending.Case is DetailViewObject detail ? RhinoFault.Unless(detail.CommitViewportChanges()) : unit;

    public void Dispose() => Viewport.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Viewports {
    // --- [RESOLUTION]
    public static IO<ViewportRef> Resolve(RhinoDoc doc, ViewportTarget target) =>
        IO.lift(() => target.Switch(
                doc,
                active: static (document, _) => Conversions.Found(document.Views.ActiveView)
                    .Map(static view => new ViewportRef(view, view.ActiveViewport, None)),
                named: static (document, named) => Conversions.Found(document.Views.Find(named.Name, compareCase: false))
                    .Map(static view => new ViewportRef(view, view.MainViewport, None)),
                id: static (document, id) => document.Objects.FindId(id.ViewportId) is DetailViewObject detail
                    ? Conversions.Found(detail.ParentPageView).Map(page => Detailed(page, detail))
                    : Conversions.Found(document.Views.Find(id.ViewportId)).Map(static view => new ViewportRef(view, view.MainViewport, None)),
                serial: static (document, serial) => Conversions.Found(Optional(RhinoView.FromRuntimeSerialNumber(serial.ViewSerial))
                        .Filter(view => view.Document?.RuntimeSerialNumber == document.RuntimeSerialNumber))
                    .Map(static view => new ViewportRef(view, view.MainViewport, None))))
            .Bind(static row => use(() => row));

    public static IO<Seq<ViewportRef>> Resolve(RhinoDoc doc, ViewportSet set) =>
        set.Switch(
            doc,
            one: static (document, one) => Resolve(document, one.Target).Map(static row => Seq(row)),
            every: static (document, every) => IO.lift(() => toSeq(document.Views.GetViewList(every.Filter)) switch {
                var views => views.Filter(static view => view is not RhinoPageView).Map(static view => new ViewportRef(view, view.MainViewport, None))
                    + Rows(Pages(views.Choose(static view => Optional(view as RhinoPageView))), every.Details),
            }).Bind(Owned),
            group: static (document, grouped) => TableOps.Find(document.PageViewGroups, grouped.Address, includeDeleted: false)
                .Bind(found => IO.lift(() => Rows(Pages(found.GetMembers()), grouped.Details)).Bind(Owned)));

    public static IO<T> With<T>(RhinoDoc doc, ViewportTarget target, Func<ViewportRef, IO<T>> body) =>
        Resolve(doc, target).Bind(body).Bracket();

    internal static Seq<RhinoPageView> Pages(IEnumerable<RhinoPageView> pages) =>
        toSeq(pages.OrderBy(static page => page.PageNumber)).Strict();

    private static IO<Seq<ViewportRef>> Owned(Seq<ViewportRef> rows) =>
        rows.TraverseM(static row => use(() => row)).As();

    private static Seq<ViewportRef> Rows(Seq<RhinoPageView> pages, bool details) =>
        pages.Bind(page => new ViewportRef(page, page.MainViewport, None)
            .Cons(details ? toSeq(page.GetDetailViews()).Map(detail => Detailed(page, detail)) : Seq<ViewportRef>())).Strict();

    private static ViewportRef Detailed(RhinoPageView page, DetailViewObject detail) =>
        detail.IsActive ? new ViewportRef(page, page.ActiveViewport, None) : new ViewportRef(page, detail.Viewport, Some(detail));

    // --- [EDITS]
    public static IO<Seq<bool>> Edit(RhinoDoc doc, ViewportSet set, Func<RhinoViewport, IO<Unit>> edit, RedrawPolicy redraw) =>
        Commits.WithinRedraw(doc, redraw, Resolve(doc, set).Bind(rows => Callbacks.Each(rows.Map(row =>
            from before in IO.lift(() => row.Viewport.ChangeCounter)
            from edited in edit(row.Viewport)
            from moved in IO.lift(() => row.Viewport.ChangeCounter != before)
            from committed in IO.lift(row.Commit)
            select moved))).Bracket());

    // --- [SIGNALS]
    public static HostEvent<Guid> Changed(RhinoDoc doc) =>
        doc.RuntimeSerialNumber switch {
            var serial => Subscriptions.Merged(
                Seq(EventKind.RhinoViewRename, EventKind.RhinoViewSetActive, EventKind.RhinoViewModified)
                    .Map(row => row.In(serial).Choose(static args => Some(args.View.ActiveViewportID)))
                + Seq(
                    EventKind.ViewportProjectionChanged.In(serial).Choose(static args => Some(args.Viewport.Id)),
                    EventKind.DisplayModeChanged.In(serial).Choose(DisplayModes.Switched),
                    EventKind.PageViewSpaceChange.In(serial).Choose(static args => Conversions.Present(args.NewActiveDetailId) || Optional(args.PageView).Map(static page => page.MainViewport.Id)),
                    EventKind.PageViewPropertiesChange.In(serial).Choose(static args => Optional(args.PageView).Map(static page => page.MainViewport.Id)))),
        };

    internal static HostEvent<Unit> Changed(RhinoDoc doc, ViewportTarget target) =>
        Changed(doc).Choose(id => Conversions.Found(
            target.Switch(id, active: static (_, _) => true, named: static (_, _) => true, id: static (changed, held) => held.ViewportId == changed, serial: static (_, _) => true), unit));

    public static HostEvent<RhinoView> Framed(RhinoDoc doc) =>
        EventKind.DrawForeground.In(doc.RuntimeSerialNumber)
            .Choose(static args => Optional(args.Viewport.ParentView))
            .DistinctUntilChanged(static view => view.RuntimeSerialNumber, static view => view.ScreenRectangle);

    // --- [STORES]
    public static ValueStore<T> Member<T>(
        RhinoDoc doc, ViewportTarget target, Func<RhinoViewport, IO<T>> read, Func<RhinoViewport, T, IO<Unit>> write, Option<Func<T, T, bool>> same = default) where T : notnull =>
        ValueStores.Of(
            With(doc, target, row => read(row.Viewport).Map(static value => Some(value))),
            value => value.Traverse(held => Edit(doc, new ViewportSet.One(target), port => write(port, held), new RedrawPolicy.AllViews(Deferred: true))).As().Map(unit),
            Applied.Live,
            Some(Changed(doc, target)),
            same);

    // --- [SCREEN]
    public static IO<RectangleF> Frame(RhinoView view) =>
        from window in IO.lift(() => Conversions.Found(RhinoEtoApp.MainWindowForDocument(view.Document)))
        from frame in IO.lift(() => view.ScreenRectangle)
        select new RectangleF(frame.X, frame.Y, frame.Width, frame.Height) / window.LogicalPixelSize;

    // --- [SPACE]
    public static LengthUnit Units(RhinoDoc doc, Option<RhinoViewport> viewport) =>
        viewport.Exists(static found => found.ViewportType is ViewportType.PageViewMainViewport) ? doc.PageUnits : doc.ModelUnits;
}

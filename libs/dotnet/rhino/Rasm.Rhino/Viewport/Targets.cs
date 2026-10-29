using System.Collections.Generic;
using Eto.Drawing;
using Rasm.Rhino.Display;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.UI;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
public abstract partial record ViewportTarget {
    public sealed record Active : ViewportTarget;

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
    internal ViewportRef(RhinoView view, RhinoViewport viewport, Option<DetailViewObject> pending) => (View, Viewport, Pending) = (view, viewport, pending);

    public RhinoView View { get; }

    public RhinoViewport Viewport { get; }

    internal Option<DetailViewObject> Pending { get; }

    internal IO<Unit> Commit =>
        Pending.Match(Some: static detail => Products.Accepted(() => detail.CommitViewportChanges()), None: static () => IO.pure(unit));

    public void Dispose() => Viewport.Dispose();
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Viewports {
    // --- [RESOLUTION]
    public static IO<ViewportRef> Resolve(RhinoDoc doc, ViewportTarget target) =>
        IO.lift(() => target.Switch(
                doc,
                active: static (document, _) => Conversions.Required(document.Views.ActiveView)
                    .Map(static view => new ViewportRef(view, view.ActiveViewport, None)),
                named: static (document, named) => Conversions.Required(document.Views.Find(named.Name, compareCase: false))
                    .Map(static view => new ViewportRef(view, view.MainViewport, None)),
                id: static (document, id) => document.Objects.FindId(id.ViewportId) is DetailViewObject detail
                    ? Conversions.Required(detail.ParentPageView).Map(page => Detailed(page, detail))
                    : Conversions.Required(document.Views.Find(id.ViewportId)).Map(static view => new ViewportRef(view, view.MainViewport, None)),
                serial: static (document, serial) => Conversions.Required(Optional(RhinoView.FromRuntimeSerialNumber(serial.ViewSerial))
                        .Filter(view => view.Document?.RuntimeSerialNumber == document.RuntimeSerialNumber))
                    .Map(static view => new ViewportRef(view, view.MainViewport, None))))
            .Bind(static row => use(() => row));

    public static IO<Seq<ViewportRef>> Resolve(RhinoDoc doc, ViewportSet set) =>
        set.Switch(
            doc,
            one: static (document, one) => Resolve(document, one.Target).Map(static row => Seq(row)),
            every: static (document, every) => IO.lift(() => toSeq(document.Views.GetViewList(every.Filter)) switch {
                var views => views.Filter(static view => view is not RhinoPageView).Map(static view => new ViewportRef(view, view.MainViewport, None))
                    + Members(Pages(views.Choose(static view => Optional(view as RhinoPageView))), every.Details),
            }).Bind(Owned),
            group: static (document, grouped) =>
                from found in Rows.Find(document.PageViewGroups, grouped.Address)
                from rows in IO.lift(() => Members(Pages(found.GetMembers()), grouped.Details))
                from owned in Owned(rows)
                select owned);

    public static IO<T> With<T>(RhinoDoc doc, ViewportTarget target, Func<ViewportRef, IO<T>> body) =>
        Resolve(doc, target).Bind(body).Bracket();

    internal static Seq<RhinoPageView> Pages(IEnumerable<RhinoPageView> pages) =>
        toSeq(pages.OrderBy(static page => page.PageNumber)).Strict();

    private static IO<Seq<ViewportRef>> Owned(Seq<ViewportRef> rows) =>
        rows.TraverseM(static row => use(() => row)).As();

    private static Seq<ViewportRef> Members(Seq<RhinoPageView> pages, bool details) =>
        pages.Bind(page => new ViewportRef(page, page.MainViewport, None)
            .Cons(details ? toSeq(page.GetDetailViews()).Map(detail => Detailed(page, detail)) : Seq<ViewportRef>())).Strict();

    private static ViewportRef Detailed(RhinoPageView page, DetailViewObject detail) =>
        detail.IsActive ? new ViewportRef(page, page.ActiveViewport, None) : new ViewportRef(page, detail.Viewport, Some(detail));

    private static Option<Guid> Identified(RhinoDoc doc, ViewportTarget target) =>
        target.Switch(
            doc,
            active: static (document, _) => Optional(document.Views.ActiveView).Map(static view => view.ActiveViewportID),
            named: static (document, named) => Optional(document.Views.Find(named.Name, compareCase: false)).Map(static view => view.MainViewport.Id),
            id: static (_, id) => Some(id.ViewportId),
            serial: static (document, serial) => Optional(RhinoView.FromRuntimeSerialNumber(serial.ViewSerial))
                .Filter(view => view.Document?.RuntimeSerialNumber == document.RuntimeSerialNumber)
                .Map(static view => view.MainViewport.Id));

    // --- [EDITS]
    public static IO<Seq<bool>> Edit(RhinoDoc doc, ViewportSet set, Func<RhinoViewport, IO<Unit>> edit, RedrawPolicy redraw) =>
        Commits.WithinRedraw(doc, redraw, Resolve(doc, set).Bind(rows => Products.Each(rows.Map(row =>
            from before in IO.lift(() => row.Viewport.ChangeCounter)
            from edited in edit(row.Viewport)
            from moved in IO.lift(() => row.Viewport.ChangeCounter != before)
            from committed in when(moved, row.Commit).As()
            select moved))).Bracket());

    // --- [SIGNALS]
    public static readonly DocumentEvent<DrawEventArgs> ProjectionChanged =
        Subscriptions.Inline<DrawEventArgs>(static h => DisplayPipeline.ViewportProjectionChanged += h, static h => DisplayPipeline.ViewportProjectionChanged -= h)
            .DistinctUntilChanged(static args => (Document: Conversions.Serial(args.RhinoDoc), Viewport: args.Viewport.Id), static args => args.Viewport.ChangeCounter)
            .Documents(static args => args.RhinoDoc);

    public static HostEvent<Guid> Changed(RhinoDoc doc) =>
        doc.RuntimeSerialNumber switch {
            var serial => Subscriptions.Merged(
                Subscriptions.Merged(
                        Subscriptions.On<ViewEventArgs>(static h => RhinoView.Rename += h, static h => RhinoView.Rename -= h),
                        Subscriptions.On<ViewEventArgs>(static h => RhinoView.SetActive += h, static h => RhinoView.SetActive -= h),
                        Subscriptions.On<ViewEventArgs>(static h => RhinoView.Modified += h, static h => RhinoView.Modified -= h))
                    .Documents(static args => args.View.Document).In(serial).Choose(static args => Some(args.View.ActiveViewportID)),
                ProjectionChanged.In(serial).Choose(static args => Some(args.Viewport.Id)),
                DisplayModes.Switches.In(serial).Choose(static args => Optional(args.Viewport).Map(static viewport => viewport.Id)),
                Subscriptions.On<PageViewSpaceChangeEventArgs>(static h => RhinoPageView.PageViewSpaceChange += h, static h => RhinoPageView.PageViewSpaceChange -= h)
                    .Documents(static args => args.PageView?.Document).In(serial)
                    .Choose(static args => Conversions.Present(args.NewActiveDetailId) || Optional(args.PageView).Map(static page => page.MainViewport.Id)),
                Subscriptions.On<PageViewPropertiesChangeEventArgs>(static h => RhinoPageView.PageViewPropertiesChange += h, static h => RhinoPageView.PageViewPropertiesChange -= h)
                    .Documents(static args => args.DocumentSerialNumber).In(serial).Choose(static args => Optional(args.PageView).Map(static page => page.MainViewport.Id))),
        };

    internal static HostEvent<Unit> Changed(RhinoDoc doc, ViewportTarget target) =>
        Changed(doc).Choose(id => Conversions.Present(Identified(doc, target) == Some(id), unit));

    public static HostEvent<RhinoView> Framed(RhinoDoc doc) =>
        Subscriptions.On<ViewEventArgs>(static h => RhinoView.Modified += h, static h => RhinoView.Modified -= h)
            .Documents(static args => args.View.Document).In(doc.RuntimeSerialNumber)
            .Choose(static args => Some(args.View))
            .DistinctUntilChanged(static view => view.RuntimeSerialNumber, static view => view.ScreenRectangle);

    // --- [STORES]
    public static Preference<T> Member<T>(RhinoDoc doc, ViewportTarget target, Func<RhinoViewport, IO<T>> read, Func<RhinoViewport, T, IO<Unit>> write) where T : notnull =>
        Preferences.Of(
            With(doc, target, row => read(row.Viewport)),
            value => Edit(doc, new ViewportSet.One(target), port => write(port, value), new RedrawPolicy.AllViews(Deferred: true)).Map(unit),
            Some(Changed(doc, target)));

    // --- [SCREEN]
    public static IO<RectangleF> Frame(RhinoView view) =>
        from window in IO.lift(() => Conversions.Required(RhinoEtoApp.MainWindowForDocument(view.Document)))
        from frame in IO.lift(() => view.ScreenRectangle)
        select new RectangleF(frame.X, frame.Y, frame.Width, frame.Height) / window.LogicalPixelSize;

    // --- [SPACE]
    public static LengthUnit Units(RhinoDoc doc, Option<RhinoViewport> viewport) =>
        viewport.Exists(static found => found.ViewportType is ViewportType.PageViewMainViewport) ? doc.PageUnits : doc.ModelUnits;
}

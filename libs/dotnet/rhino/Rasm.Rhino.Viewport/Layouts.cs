using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayoutOp {
    public sealed record AddPage(string Title, double PageWidth, double PageHeight, bool SetActive) : LayoutOp;

    public sealed record Duplicate(Guid PageViewId, bool DuplicatePageGeometry) : LayoutOp;

    public sealed record Close(Guid PageViewId) : LayoutOp;

    public sealed record Rename(Guid PageViewId, string PageName) : LayoutOp;

    public sealed record Renumber(Guid PageViewId, int PageNumber) : LayoutOp;

    public sealed record Resize(Guid PageViewId, double PageWidth, double PageHeight) : LayoutOp;

    public sealed record Group(Seq<Guid> PageViewIds, Option<string> Name) : LayoutOp;

    public sealed record Ungroup(Guid GroupId) : LayoutOp;

    public sealed record AddDetail(Guid PageViewId, string Title, Point2d Corner0, Point2d Corner1, DefinedViewportProjection InitialProjection) : LayoutOp;

    public sealed record Scale(Guid PageViewId, Guid DetailId, double ModelLength, LengthUnit ModelUnits, double PageLength, LengthUnit PageUnits) : LayoutOp;

    public sealed record LockProjection(Guid PageViewId, Guid DetailId, bool Locked) : LayoutOp;

    public sealed record Activate(Guid PageViewId, Option<Guid> DetailId) : LayoutOp;
}

public sealed record DetailState(Guid Id, string DescriptiveTitle, bool IsActive, bool IsProjectionLocked, Option<(double PageToModelRatio, string FormattedScale)> Scale);

public sealed record PageState(Guid PageViewId, string PageName, int PageNumber, double PageWidth, double PageHeight, Option<Guid> ActiveDetailId, Seq<DetailState> Details);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Layouts {
    // --- [PAGES]
    public static IO<Option<Guid>> Apply(RhinoDoc document, LayoutOp op) =>
        op.Switch(
            document,
            addPage: static (doc, add) => IO.lift(() =>
                Missing.Unless(doc.Views.AddPageView(add.Title, add.PageWidth, add.PageHeight, add.SetActive), nameof(ViewTable.AddPageView))
                    .Map(static page => Some(page.MainViewport.Id))),
            duplicate: static (doc, duplicate) =>
                from page in Viewports.ResolvePage(doc, duplicate.PageViewId)
                from copy in IO.lift(() => Missing.Unless(page.Duplicate(duplicate.DuplicatePageGeometry), nameof(RhinoPageView.Duplicate)))
                select Some(copy.MainViewport.Id),
            close: static (doc, close) =>
                from page in Viewports.ResolvePage(doc, close.PageViewId)
                from closed in IO.lift(() => Refused.Unless(page.Close(), nameof(RhinoView.Close)))
                select Option<Guid>.None,
            rename: static (doc, rename) =>
                from page in Viewports.ResolvePage(doc, rename.PageViewId)
                from renamed in IO.lift(() => { page.PageName = rename.PageName; })
                select Option<Guid>.None,
            renumber: static (doc, renumber) =>
                from page in Viewports.ResolvePage(doc, renumber.PageViewId)
                from renumbered in IO.lift(() => { page.PageNumber = renumber.PageNumber; })
                select Option<Guid>.None,
            resize: static (doc, resize) =>
                from page in Viewports.ResolvePage(doc, resize.PageViewId)
                from resized in IO.lift(() => {
                    page.PageWidth = resize.PageWidth;
                    page.PageHeight = resize.PageHeight;
                })
                select Option<Guid>.None,
            group: static (doc, grouped) =>
                from pages in grouped.PageViewIds.TraverseM(id => Viewports.ResolvePage(doc, id)).As()
                from created in Disposal.Using(
                    () => grouped.Name.Match(Some: static name => new PageViewGroup { Name = name }, None: static () => new PageViewGroup()),
                    settings =>
                        from index in IO.lift(() => Answers.NonNegative(doc.PageViewGroups.Add(settings, pages), nameof(PageViewGroupTable.Add)))
                        from added in IO.lift(() => Missing.Unless(doc.PageViewGroups[index], nameof(PageViewGroupTable)))
                        select Some(added.Id))
                select created,
            ungroup: static (doc, ungroup) =>
                from deleted in IO.lift(() => Refused.Unless(doc.PageViewGroups.Delete(ungroup.GroupId, quiet: false), nameof(PageViewGroupTable.Delete)))
                select Option<Guid>.None,
            addDetail: static (doc, add) =>
                from page in Viewports.ResolvePage(doc, add.PageViewId)
                from detail in Viewports.WithActiveView(doc, page, IO.lift(() =>
                    Missing.Unless(page.AddDetailView(add.Title, add.Corner0, add.Corner1, add.InitialProjection), nameof(RhinoPageView.AddDetailView))))
                select Some(detail.Id),
            scale: static (doc, scale) =>
                from detail in Viewports.ResolveDetail(doc, scale.PageViewId, scale.DetailId)
                from scaled in IO.lift(() => Refused.Unless(detail.DetailGeometry.SetScale(scale.ModelLength, scale.ModelUnits, scale.PageLength, scale.PageUnits), nameof(DetailView.SetScale)))
                from committed in IO.lift(() => Refused.Unless(detail.CommitChanges(), nameof(RhinoObject.CommitChanges)))
                select Option<Guid>.None,
            lockProjection: static (doc, locking) =>
                from detail in Viewports.ResolveDetail(doc, locking.PageViewId, locking.DetailId)
                from locked in IO.lift(() => { detail.DetailGeometry.IsProjectionLocked = locking.Locked; })
                from committed in IO.lift(() => Refused.Unless(detail.CommitChanges(), nameof(RhinoObject.CommitChanges)))
                select Option<Guid>.None,
            activate: static (doc, activate) =>
                from page in Viewports.ResolvePage(doc, activate.PageViewId)
                from activated in IO.lift(() => { doc.Views.ActiveView = page; })
                from selected in IO.lift(() => activate.DetailId.Match(
                    Some: id => Refused.Unless(page.SetActiveDetail(id), nameof(RhinoPageView.SetActiveDetail)),
                    None: () => {
                        page.SetPageAsActive();
                        return Fin.Succ(unit);
                    }))
                select Option<Guid>.None);

    // --- [READS]
    public static IO<PageState> Read(RhinoDoc document, Guid pageViewId, DetailViewObject.ScaleFormat format) =>
        from page in Viewports.ResolvePage(document, pageViewId)
        from state in IO.lift(() => new PageState(
            page.MainViewport.Id,
            page.PageName,
            page.PageNumber,
            page.PageWidth,
            page.PageHeight,
            Answers.Present(page.ActiveDetailId),
            toSeq(page.GetDetailViews()).Map(detail => new DetailState(
                    detail.Id,
                    detail.DescriptiveTitle,
                    detail.IsActive,
                    detail.DetailGeometry.IsProjectionLocked,
                    Answers.Found(detail.DetailGeometry.IsParallelProjection, detail)
                        .Bind(parallel => Answers.Found(parallel.GetFormattedScale(format, out string scale), (parallel.DetailGeometry.PageToModelRatio, scale)))))
                .Strict()))
        select state;

    public static IO<TValue> WithPreview<TValue>(RhinoDoc document, Guid pageViewId, Size size, bool grayScale, Func<Bitmap, IO<TValue>> body) =>
        Viewports.ResolvePage(document, pageViewId).Bind(page => Disposal.Using(
            IO.lift(() => Missing.Unless(page.GetPreviewImage(size, grayScale), nameof(RhinoPageView.GetPreviewImage))),
            body));
}

using System.Drawing;
using Rasm.Rhino.Document;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.DocObjects.Tables;
using Riok.Mapperly.Abstractions;

namespace Rasm.Rhino.Viewport;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayoutCreate {
    public sealed record AddPage(string Title, double PageWidth, double PageHeight, bool SetActive) : LayoutCreate;

    public sealed record Duplicate(Guid PageViewId, bool DuplicatePageGeometry) : LayoutCreate;

    public sealed record Group(Seq<Guid> PageViewIds, Option<string> Name) : LayoutCreate;

    public sealed record AddDetail(Guid PageViewId, string Title, Point2d Corner0, Point2d Corner1, DefinedViewportProjection InitialProjection) : LayoutCreate;
}

public sealed record DetailScale(double ModelLength, LengthUnit ModelUnits, double PageLength, LengthUnit PageUnits);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record LayoutOp {
    public sealed record Close(Guid PageViewId) : LayoutOp;

    public sealed record EditPage(Guid PageViewId, Option<string> PageName, Option<int> PageNumber, Option<double> PageWidth, Option<double> PageHeight, Option<string> Description)
        : LayoutOp;

    public sealed record Ungroup(ComponentRef Group) : LayoutOp;

    public sealed record Join(Guid PageViewId, ComponentRef Group, Option<int> Position) : LayoutOp;

    public sealed record Leave(Guid PageViewId, Option<ComponentRef> Group) : LayoutOp;

    public sealed record EditGroup(ComponentRef Group, Option<string> Name, Option<string> Description, Option<bool> IsExpanded) : LayoutOp;

    public sealed record EditDetail(Guid PageViewId, Guid DetailId, Option<DetailScale> Scale, Option<bool> ProjectionLocked) : LayoutOp;

    public sealed record Activate(Guid PageViewId, Option<Guid> DetailId) : LayoutOp;
}

public sealed record DetailState(Guid Id, Option<string> DescriptiveTitle, bool IsActive, bool IsProjectionLocked, Option<(double PageToModelRatio, string Formatted)> Scale);

public sealed record GroupMembership(int Group, Option<int> SortIndex);

public sealed record PageState(
    Guid PageViewId,
    string PageName,
    int PageNumber,
    double PageWidth,
    double PageHeight,
    Option<string> Description,
    Option<string> PaperName,
    Option<Guid> ActiveDetailId,
    Seq<GroupMembership> Groups,
    Seq<DetailState> Details);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class LayoutMapper {
    [MapProperty(nameof(@RhinoPageView.MainViewport.Id), nameof(PageState.PageViewId), SuppressNullMismatchDiagnostic = true)]
    [MapProperty(nameof(RhinoPageView.PageName), nameof(PageState.PageName), SuppressNullMismatchDiagnostic = true)]
    internal static partial PageState ToState(RhinoPageView page, Seq<GroupMembership> groups, Seq<DetailState> details);

    [MapProperty(nameof(@DetailViewObject.DetailGeometry.IsProjectionLocked), nameof(DetailState.IsProjectionLocked), SuppressNullMismatchDiagnostic = true)]
    internal static partial DetailState ToDetail(DetailViewObject detail, Option<(double PageToModelRatio, string Formatted)> scale);
}

public static class Layouts {
    // --- [CREATES]
    public static IO<Guid> Create(RhinoDoc document, LayoutCreate create) =>
        create.Switch(
            document,
            addPage: static (doc, add) => IO.lift(() =>
                Missing.Unless(doc.Views.AddPageView(add.Title, add.PageWidth, add.PageHeight, add.SetActive), nameof(ViewTable.AddPageView))
                    .Map(static page => page.MainViewport.Id)),
            duplicate: static (doc, duplicate) =>
                from page in Viewports.ResolvePage(doc, duplicate.PageViewId)
                from copy in IO.lift(() => Missing.Unless(page.Duplicate(duplicate.DuplicatePageGeometry), nameof(RhinoPageView.Duplicate)))
                select copy.MainViewport.Id,
            group: static (doc, grouped) =>
                from pages in grouped.PageViewIds.TraverseM(id => Viewports.ResolvePage(doc, id)).As()
                from id in DisposalOps.Using(
                    static () => new PageViewGroup(),
                    settings =>
                        from named in TableOps.Named(settings, grouped.Name)
                        from index in IO.lift(() => Answers.Required(doc.PageViewGroups.Add(settings, pages), nameof(PageViewGroupTable.Add)))
                        select doc.PageViewGroups[index].Id)
                select id,
            addDetail: static (doc, add) =>
                from page in Viewports.ResolvePage(doc, add.PageViewId)
                from detail in Viewports.WithActiveView(doc, page, IO.lift(() =>
                    Missing.Unless(page.AddDetailView(add.Title, add.Corner0, add.Corner1, add.InitialProjection), nameof(RhinoPageView.AddDetailView))))
                select detail.Id);

    // --- [EDITS]
    public static IO<Unit> Apply(RhinoDoc document, LayoutOp op) =>
        op.Switch(
            document,
            close: static (doc, close) =>
                from page in Viewports.ResolvePage(doc, close.PageViewId)
                from closed in IO.lift(() => Refused.Unless(page.Close(), nameof(RhinoView.Close)))
                select closed,
            editPage: static (doc, edit) =>
                from page in Viewports.ResolvePage(doc, edit.PageViewId)
                from written in IO.lift(() => {
                    _ = edit.PageName.Iter(name => page.PageName = name);
                    _ = edit.PageNumber.Iter(number => page.PageNumber = number);
                    _ = edit.PageWidth.Iter(width => page.PageWidth = width);
                    _ = edit.PageHeight.Iter(height => page.PageHeight = height);
                    _ = edit.Description.Iter(text => page.Description = text);
                })
                select written,
            ungroup: static (doc, ungroup) =>
                from resolved in TableOps.Find(doc.PageViewGroups, ungroup.Group, includeDeleted: false)
                from deleted in IO.lift(() => Refused.Unless(doc.PageViewGroups.Delete(resolved.Index, quiet: true), nameof(PageViewGroupTable.Delete)))
                select deleted,
            join: static (doc, joining) =>
                from page in Viewports.ResolvePage(doc, joining.PageViewId)
                from resolved in TableOps.Find(doc.PageViewGroups, joining.Group, includeDeleted: false)
                from position in IO.lift(() => joining.Position.Traverse(static sort => Limits.AtLeast(0).Check(sort, nameof(LayoutOp.Join.Position))).As())
                from joined in IO.lift(() => position.Match(
                    Some: index => page.SetPageViewGroupSortIndex(resolved.Index, index),
                    None: () => page.AddToPageViewGroup(resolved.Index)))
                select joined,
            leave: static (doc, leave) =>
                from page in Viewports.ResolvePage(doc, leave.PageViewId)
                from resolved in leave.Group.Traverse(address => TableOps.Find(doc.PageViewGroups, address, includeDeleted: false)).As()
                from left in IO.lift(() => resolved.Match(Some: row => page.RemoveFromPageViewGroup(row.Index), None: page.RemoveFromAllageViewGroups))
                select left,
            editGroup: static (doc, edit) =>
                from resolved in TableOps.Find(doc.PageViewGroups, edit.Group, includeDeleted: false)
                from named in TableOps.Named(resolved, edit.Name)
                from edited in IO.lift(() => {
                    _ = edit.Description.Iter(text => resolved.Description = text);
                    _ = edit.IsExpanded.Iter(expanded => resolved.IsExpanded = expanded);
                })
                select edited,
            editDetail: static (doc, edit) =>
                from detail in Viewports.ResolveDetail(doc, edit.PageViewId, edit.DetailId)
                from scaled in IO.lift(() => edit.Scale
                    .Traverse(scale => Refused.Unless(detail.DetailGeometry.SetScale(scale.ModelLength, scale.ModelUnits, scale.PageLength, scale.PageUnits), nameof(DetailView.SetScale)))
                    .As())
                from locked in IO.lift(() => edit.ProjectionLocked.Iter(projectionLocked => detail.DetailGeometry.IsProjectionLocked = projectionLocked))
                from committed in IO.lift(() => Refused.Unless(detail.CommitChanges(), nameof(RhinoObject.CommitChanges)))
                select committed,
            activate: static (doc, activate) =>
                from page in Viewports.ResolvePage(doc, activate.PageViewId)
                from activated in IO.lift(() => { doc.Views.ActiveView = page; })
                from selected in activate.DetailId.Match(
                    Some: id => IO.lift(() => Refused.Unless(page.SetActiveDetail(id), nameof(RhinoPageView.SetActiveDetail))),
                    None: () => IO.lift(page.SetPageAsActive))
                select selected);

    // --- [READS]
    public static IO<PageState> Read(RhinoDoc document, Guid pageViewId, DetailViewObject.ScaleFormat format) =>
        from page in Viewports.ResolvePage(document, pageViewId)
        from details in IO.lift(() => toSeq(page.GetDetailViews()).Traverse(detail => Detail(detail, format)).As())
        from groups in IO.lift(() => toSeq(page.GetPageViewGroupList()).Map(index => new GroupMembership(index, Answers.Present(page.PageViewGroupSortIndex(index)))).Strict())
        select LayoutMapper.ToState(page, groups, details);

    public static IO<TValue> WithPreview<TValue>(RhinoDoc document, Guid pageViewId, Size size, bool grayScale, Func<Bitmap, IO<TValue>> body) =>
        Viewports.ResolvePage(document, pageViewId).Bind(page => DisposalOps.Using(
            IO.lift(() => Missing.Unless(page.GetPreviewImage(size, grayScale), nameof(RhinoPageView.GetPreviewImage))),
            body));

    private static Fin<DetailState> Detail(DetailViewObject detail, DetailViewObject.ScaleFormat format) =>
        Answers.Found(detail.DetailGeometry.IsParallelProjection, detail)
            .Traverse(parallel => Refused.Unless(
                parallel.GetFormattedScale(format, out string scale),
                (parallel.DetailGeometry.PageToModelRatio, Formatted: scale),
                nameof(DetailViewObject.GetFormattedScale)))
            .As()
            .Map(scaled => LayoutMapper.ToDetail(detail, scaled));
}

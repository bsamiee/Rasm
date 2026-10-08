using System.Drawing;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PickRegion {
    public sealed record PointPick(System.Drawing.Point Client) : PickRegion;

    public sealed record RectanglePick(Rectangle Client, bool Crossing) : PickRegion;
}

public sealed record PickRequest(ViewportTarget Viewport, PickRegion Region, PickMode PickMode, bool PickGroupsEnabled, bool SubObjectSelectionEnabled);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Selections {
    // --- [FRUSTUM]
    public static IO<T> Frustum<T>(RhinoDoc doc, PickRequest request, Func<PickContext, IO<T>> body) =>
        (from row in use(Viewports.ResolveViewport(doc, request.Viewport))
         from picked in (from context in use(static () => new PickContext())
                         from answer in Activated(row,
                             from frame in IO.lift(() => request.Region.Switch(
                                 row.Viewport,
                                 pointPick: static (viewport, pick) => (Style: PickStyle.PointPick, Transform: viewport.GetPickTransform(pick.Client), pick.Client.X, pick.Client.Y),
                                 rectanglePick: static (viewport, pick) => (Style: pick.Crossing ? PickStyle.CrossingPick : PickStyle.WindowPick,
                                     Transform: viewport.GetPickTransform(pick.Client), X: pick.Client.X + (pick.Client.Width / 2d), Y: pick.Client.Y + (pick.Client.Height / 2d))))
                             from line in IO.lift(() => Refused.Unless(row.Viewport.GetFrustumLine(frame.X, frame.Y, out Line worldLine), worldLine, nameof(RhinoViewport.GetFrustumLine)))
                             from mapped in IO.lift(() => PickMapper.Update(request, context, row.View, frame.Style, line))
                             from transformed in IO.lift(() => context.SetPickTransform(frame.Transform))
                             from clipped in IO.lift(context.UpdateClippingPlanes)
                             from value in body(context)
                             select value)
                         select answer).Bracket()
         select picked).Bracket();

    private static IO<T> Activated<T>(ViewportRef row, IO<T> body) =>
        row.View is RhinoPageView page
            ? (from prior in IO.lift(() => Conversions.Present(page.ActiveDetailId))
               from activated in Active(page, row.Detail.Map(static detail => detail.Id))
               select prior).Bracket(Use: _ => body, Fin: prior => Active(page, prior))
            : body;

    private static IO<Unit> Active(RhinoPageView page, Option<Guid> detail) =>
        detail.Match(
            Some: id => IO.lift(() => Refused.Unless(page.SetActiveDetail(id), nameof(RhinoPageView.SetActiveDetail))),
            None: () => IO.lift(() => page.SetPageAsActive()));

    // --- [PICKS]
    public static IO<Seq<PickCapture>> Pick(RhinoDoc doc, PickRequest request, Option<SelectionOptions> selection = default) =>
        Frustum(doc, request, context => IO.lift(() => toSeq(doc.Objects.PickObjects(context)))
            .Bracket(Use: references => Capture(references, selection), Fin: DisposalOps.Release));

    private static IO<Seq<PickCapture>> Capture(Seq<ObjRef> references, Option<SelectionOptions> selection) =>
        from selected in selection.TraverseM(options => IO.lift(() => Callbacks.Each(references, (reference, index) => Selected(reference, index, options)))).As()
        from captures in references.TraverseM(PickCapture.Of).As()
        select captures;

    private static Fin<Unit> Selected(ObjRef reference, int index, SelectionOptions options) =>
        Conversions.Present(reference.GeometryComponentIndex).Match(
            Some: component => RefusedElement.Unless(
                reference.Object().SelectSubObject(component, select: true, options.SyncHighlight, options.Persistent) != 0, nameof(RhinoObject.SelectSubObject), index),
            None: () => RefusedElement.Unless(
                reference.Object().Select(on: true, options.SyncHighlight, options.Persistent, options.IgnoreGrips, options.IgnoreLayerLocks, options.IgnoreLayerVisibility) != 0,
                nameof(RhinoObject.Select), index));
}

[Mapper]
internal static class PickMapper {
}

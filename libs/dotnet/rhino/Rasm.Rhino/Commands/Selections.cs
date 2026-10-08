using System.Drawing;
using Rasm.Rhino.Document;
using Rasm.Rhino.Document.Tables;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
[Union]
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
                         from answer in Activated(row.Detail, Framed(context, row, request).Bind(_ => body(context)))
                         select answer).Bracket()
         select picked).Bracket();

    private static IO<Unit> Framed(PickContext context, ViewportRef row, PickRequest request) =>
        from frame in IO.lift(() => request.Region.Switch(
            row.Viewport,
            pointPick: static (viewport, pick) => (Style: PickStyle.PointPick, Transform: viewport.GetPickTransform(pick.Client), pick.Client.X, pick.Client.Y),
            rectanglePick: static (viewport, pick) => (
                Style: pick.Crossing ? PickStyle.CrossingPick : PickStyle.WindowPick,
                Transform: viewport.GetPickTransform(pick.Client),
                X: pick.Client.X + (pick.Client.Width / 2d),
                Y: pick.Client.Y + (pick.Client.Height / 2d))))
        from line in IO.lift(() => Refused.Unless(row.Viewport.GetFrustumLine(frame.X, frame.Y, out Line worldLine), worldLine, nameof(RhinoViewport.GetFrustumLine)))
        from framed in IO.lift(() => {
            context.View = row.View;
            context.PickStyle = frame.Style;
            context.PickMode = request.PickMode;
            context.PickGroupsEnabled = request.PickGroupsEnabled;
            context.SubObjectSelectionEnabled = request.SubObjectSelectionEnabled;
            context.SetPickTransform(frame.Transform);
            context.PickLine = line;
            context.UpdateClippingPlanes();
        })
        select framed;

    private static IO<T> Activated<T>(Option<DetailViewObject> detail, IO<T> body) =>
        detail.Match(
            Some: found => (from page in IO.lift(() => Missing.Unless(found.ParentPageView, nameof(DetailViewObject.ParentPageView)))
                            let prior = page.ActiveDetailId
                            from activated in IO.lift(() => Refused.Unless(page.SetActiveDetail(found.Id), nameof(RhinoPageView.SetActiveDetail)))
                            select (Page: page, Prior: prior))
                .Bracket(
                    Use: _ => body,
                    Fin: static held => Conversions.Present(held.Prior).Match(
                        Some: prior => IO.lift(() => Refused.Unless(held.Page.SetActiveDetail(prior), nameof(RhinoPageView.SetActiveDetail))),
                        None: () => IO.lift(held.Page.SetPageAsActive))),
            None: () => body);

    // --- [PICKS]
    public static IO<Seq<PickCapture>> Pick(RhinoDoc doc, PickRequest request) =>
        Picked(doc, request, static references => references.TraverseM(PickCapture.Of).As());

    public static IO<Seq<PickCapture>> Select(RhinoDoc doc, PickRequest request, SelectionOptions options) =>
        Picked(doc, request, references =>
            from selected in IO.lift(() => Callbacks.Each(references, (reference, index) => Selected(reference, index, options)))
            from captures in references.TraverseM(PickCapture.Of).As()
            select captures);

    private static IO<Seq<PickCapture>> Picked(RhinoDoc doc, PickRequest request, Func<Seq<ObjRef>, IO<Seq<PickCapture>>> read) =>
        Frustum(doc, request, context => IO.lift(() => toSeq(doc.Objects.PickObjects(context))).Bracket(Use: read, Fin: DisposalOps.Release));

    private static Fin<Unit> Selected(ObjRef reference, int index, SelectionOptions options) =>
        Conversions.Present(reference.GeometryComponentIndex).Match(
            Some: component => RefusedElement.Unless(
                reference.Object().SelectSubObject(component, select: true, options.SyncHighlight, options.Persistent) != 0, nameof(RhinoObject.SelectSubObject), index),
            None: () => RefusedElement.Unless(
                reference.Object().Select(on: true, options.SyncHighlight, options.Persistent, options.IgnoreGrips, options.IgnoreLayerLocks, options.IgnoreLayerVisibility) != 0,
                nameof(RhinoObject.Select), index));
}

using System.Drawing;
using Rasm.Rhino.Document.Tables;
using Rasm.Rhino.Viewport;
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
        Viewports.With(doc, request.Viewport, row =>
            (from context in use(static () => new PickContext())
             from value in Activated(row, Picked(row, request, context, body))
             select value).Bracket());

    private static IO<T> Picked<T>(ViewportRef row, PickRequest request, PickContext context, Func<PickContext, IO<T>> body) =>
        from frame in IO.lift(() => request.Region.Switch(
            row.Viewport,
            pointPick: static (viewport, pick) => (Style: PickStyle.PointPick, Transform: viewport.GetPickTransform(pick.Client), pick.Client.X, pick.Client.Y),
            rectanglePick: static (viewport, pick) => (Style: pick.Crossing ? PickStyle.CrossingPick : PickStyle.WindowPick,
                Transform: viewport.GetPickTransform(pick.Client), X: pick.Client.X + (pick.Client.Width / 2d), Y: pick.Client.Y + (pick.Client.Height / 2d))))
        from frustum in IO.lift(() => (Found: row.Viewport.GetFrustumLine(frame.X, frame.Y, out Line worldLine), Line: worldLine))
        from line in IO.lift(() => RhinoFault.Unless(frustum.Found, RhinoReason.Declined, (frame.X, frame.Y)).Map(_ => frustum.Line))
        from framed in IO.lift(() => {
            context.View = row.View;
            context.PickStyle = frame.Style;
            context.PickLine = line;
            context.PickMode = request.PickMode;
            context.PickGroupsEnabled = request.PickGroupsEnabled;
            context.SubObjectSelectionEnabled = request.SubObjectSelectionEnabled;
            context.SetPickTransform(frame.Transform);
            context.UpdateClippingPlanes();
        })
        from value in body(context)
        select value;

    private static IO<T> Activated<T>(ViewportRef row, IO<T> body) =>
        row.View is RhinoPageView page
            ? IO.lift(() => Conversions.Present(page.ActiveDetailId))
                .Bind(prior => Active(page, Conversions.Present(row.Viewport.ViewportType is ViewportType.DetailViewport, row.Viewport.Id)) >> body.Finally(Active(page, prior)))
            : body;

    private static IO<Unit> Active(RhinoPageView page, Option<Guid> detail) =>
        detail.Match(
            Some: id => Products.Accepted(() => page.SetActiveDetail(id)),
            None: () => IO.lift(page.SetPageAsActive));

    // --- [PICKS]
    public static IO<Seq<PickCapture>> Pick(RhinoDoc doc, PickRequest request, Option<Func<ObjRef, int>> selection = default) =>
        Frustum(doc, request, context =>
            from picked in IO.lift(() => toSeq(doc.Objects.PickObjects(context)).Strict())
            from references in picked.TraverseM(static reference => Products.Of(() => reference)).As()
            from captured in Capture(references, selection)
            select captured);

    private static IO<Seq<PickCapture>> Capture(Seq<ObjRef> references, Option<Func<ObjRef, int>> selection) =>
        from selected in selection.TraverseM(call => IO.lift(() => Products.Each(references, (reference, index) =>
            RhinoFault.Unless(call(reference) is not 0, RhinoReason.Declined, reference.ObjectId, index)))).As()
        from captures in references.TraverseM(PickCapture.Of).As()
        select captures;
}

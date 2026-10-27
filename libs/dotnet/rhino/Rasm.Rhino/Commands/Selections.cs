using System.Drawing;
using Rasm.RhinoCommon.Document.Tables;
using Rasm.RhinoCommon.Viewport;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Riok.Mapperly.Abstractions;

namespace Rasm.RhinoCommon.Commands;

// --- [MODELS] --------------------------------------------------------------------------
[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PickRegion {
    public sealed record PointPick(System.Drawing.Point Client) : PickRegion;

    public sealed record RectanglePick(Rectangle Client, bool Crossing) : PickRegion;
}

public sealed record PickRequest(ViewportTarget Viewport, PickRegion Region, PickMode PickMode, bool PickGroupsEnabled, bool SubObjectSelectionEnabled);

internal sealed record PickFrame(RhinoView View, PickStyle PickStyle, Line PickLine, PickMode PickMode, bool PickGroupsEnabled, bool SubObjectSelectionEnabled);

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
        from line in IO.lift(() => RhinoFault.Unless(row.Viewport.GetFrustumLine(frame.X, frame.Y, out Line worldLine)).Map(_ => worldLine))
        from mapped in IO.lift(() => PickMapper.Update(new PickFrame(row.View, frame.Style, line, request.PickMode, request.PickGroupsEnabled, request.SubObjectSelectionEnabled), context))
        from transformed in IO.lift(() => context.SetPickTransform(frame.Transform))
        from clipped in IO.lift(context.UpdateClippingPlanes)
        from value in body(context)
        select value;

    private static IO<T> Activated<T>(ViewportRef row, IO<T> body) =>
        row.View is RhinoPageView page
            ? IO.lift(() => Conversions.Present(page.ActiveDetailId))
                .Bind(prior => Active(page, Conversions.Found(row.Viewport.ViewportType is ViewportType.DetailViewport, row.Viewport.Id)) >> body.Finally(Active(page, prior)))
            : body;

    private static IO<Unit> Active(RhinoPageView page, Option<Guid> detail) =>
        detail.Match(
            Some: id => IO.lift(() => RhinoFault.Unless(page.SetActiveDetail(id))),
            None: () => IO.lift(() => page.SetPageAsActive()));

    // --- [PICKS]
    public static IO<Seq<PickCapture>> Pick(RhinoDoc doc, PickRequest request, Option<SelectionOptions> selection = default) =>
        Frustum(doc, request, context => Products.Made(doc.Objects.PickObjects(context)).Bind(references => Capture(references, selection)));

    private static IO<Seq<PickCapture>> Capture(Seq<ObjRef> references, Option<SelectionOptions> selection) =>
        from selected in selection.TraverseM(options => IO.lift(() => Callbacks.Each(references, Selected(options)))).As()
        from captures in references.TraverseM(PickCapture.Of).As()
        select captures;

    private static Func<ObjRef, int, Fin<Unit>> Selected(SelectionOptions options) =>
        (reference, index) => Conversions.Present(reference.GeometryComponentIndex).Match(
            Some: component => RhinoFault.Unless(reference.Object().SelectSubObject(component, select: true, options.SyncHighlight, options.Persistent) is not 0, index),
            None: () => RhinoFault.Unless(
                reference.Object().Select(on: true, options.SyncHighlight, options.Persistent, options.IgnoreGrips, options.IgnoreLayerLocks, options.IgnoreLayerVisibility) is not 0, index));
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Source)]
internal static partial class PickMapper {
    internal static partial void Update(PickFrame frame, PickContext context);
}

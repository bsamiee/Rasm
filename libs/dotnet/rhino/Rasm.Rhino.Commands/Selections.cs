using Rasm.Rhino.Document;
using Rhino;
using Rhino.DocObjects;
using Rhino.Input.Custom;

namespace Rasm.Rhino.Commands;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PickCapture(
    Guid ObjectId,
    ComponentIndex Component,
    SelectionMethod Method,
    Option<Point3d> PickPoint,
    Option<double> CurveParameter,
    Option<(double U, double V)> SurfaceParameter,
    Option<ViewportIdentity> View,
    Option<uint> DetailSerial);

public sealed record PickRequest(
    ViewportTarget View,
    Option<Line> PickLine,
    PickStyle Style,
    PickMode Mode,
    Option<bool> Groups,
    Option<bool> SubObjects,
    Option<Transform> Xform,
    bool UpdateClipping);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Selections {
    // --- [CAPTURE]
    public static IO<PickCapture> Capture(ObjRef reference) =>
        from method in IO.lift(reference.SelectionMethod)
        from parameters in IO.lift(() => method == SelectionMethod.MousePick
            ? Parameters(reference)
            : (Curve: Option<double>.None, Surface: Option<(double U, double V)>.None))
        from capture in IO.lift(() => new PickCapture(
            reference.ObjectId,
            reference.GeometryComponentIndex,
            method,
            Some(reference.SelectionPoint()).Filter(static point => point.IsValid),
            parameters.Curve,
            parameters.Surface,
            Optional(reference.SelectionView()).Map(static view => Viewports.Identity(view, view.MainViewport)),
            Answers.Present(reference.SelectionViewDetailSerialNumber())))
        select capture;

    public static IO<Seq<PickCapture>> CaptureOwned(Seq<ObjRef> owned) =>
        Disposal.Using(IO.pure(owned), static references => references.TraverseM(Capture).As());

    private static (Option<double> Curve, Option<(double U, double V)> Surface) Parameters(ObjRef reference) {
        using Curve? curve = reference.CurveParameter(out double t);
        using Surface? surface = reference.SurfaceParameter(out double u, out double v);
        return (Optional(curve).Map(_ => t), Optional(surface).Map(_ => (u, v)));
    }

    // --- [PICKING]
    public static IO<Seq<PickCapture>> Pick(RhinoDoc doc, PickRequest request) =>
        from row in Viewports.ResolveViewport(doc, request.View)
        from captures in Disposal.Using(static () => new PickContext(), context =>
            from configured in IO.lift(() => {
                context.View = row.View;
                _ = request.PickLine.Iter(line => context.PickLine = line);
                context.PickStyle = request.Style;
                context.PickMode = request.Mode;
                _ = request.Groups.Iter(groups => context.PickGroupsEnabled = groups);
                _ = request.SubObjects.Iter(subObjects => context.SubObjectSelectionEnabled = subObjects);
                _ = request.Xform.Iter(context.SetPickTransform);
                if (request.UpdateClipping)
                    context.UpdateClippingPlanes();
            })
            from captures in IO.lift(() => toSeq(doc.Objects.PickObjects(context))).Bind(CaptureOwned)
            select captures)
        select captures;

    // --- [PARTS]
    public static IO<Option<TValue>> WithPart<T, TValue>(ObjRef reference, Func<ObjRef, T?> part, Func<T, IO<TValue>> body) where T : GeometryBase =>
        Disposal.Bracketed(
            IO.lift(() => Optional(part(reference))),
            static wrapper => Disposal.Release(wrapper.ToSeq()),
            wrapper => wrapper.Traverse(body).As());
}

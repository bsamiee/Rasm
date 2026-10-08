using Rhino.DocObjects;

namespace Rasm.Rhino.Document.Tables;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record PickCapture(
    Guid ObjectId,
    Option<ComponentIndex> GeometryComponentIndex,
    SelectionMethod SelectionMethod,
    Option<Point3d> SelectionPoint,
    Option<double> CurveParameter,
    Option<(double U, double V)> SurfaceParameter,
    Option<ViewportTarget> Viewport) {
    public static IO<PickCapture> Of(ObjRef reference) =>
        IO.lift(reference.SelectionMethod).Map(method => new PickCapture(
            reference.ObjectId,
            Conversions.Present(reference.GeometryComponentIndex),
            method,
            Conversions.Present(reference.SelectionPoint()),
            Callbacks.Found(reference.CurveParameter(out double t) is not null && method is SelectionMethod.MousePick, t),
            Callbacks.Found(reference.SurfaceParameter(out double u, out double v) is not null, (u, v)),
            (Conversions.Present(reference.SelectionViewDetailSerialNumber()).Bind(serial => Optional(reference.Document.Objects.Find(serial))).Map(static detail => detail.Id)
                || Optional(reference.SelectionView()).Map(static view => view.MainViewport.Id))
            .Map<ViewportTarget>(static id => new ViewportTarget.Id(id))));
}

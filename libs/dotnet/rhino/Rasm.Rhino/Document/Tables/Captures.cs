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
        IO.lift(() => new PickCapture(
            reference.ObjectId,
            Conversions.Present(reference.GeometryComponentIndex),
            reference.SelectionMethod(),
            Conversions.Present(reference.SelectionPoint()),
            Callbacks.Found(reference.CurveParameter(out double t) is not null && reference.SelectionMethod() is SelectionMethod.MousePick, t),
            Callbacks.Found(reference.SurfaceParameter(out double u, out double v) is not null, (u, v)),
            Conversions.Present(reference.SelectionViewDetailSerialNumber())
                .Bind(serial => Optional(reference.Document.Objects.Find(serial)))
                .Map<ViewportTarget>(static detail => new ViewportTarget.Detail(detail.Id))
            || Optional(reference.SelectionView()).Map<ViewportTarget>(static view => new ViewportTarget.Id(view.MainViewport.Id))));
}

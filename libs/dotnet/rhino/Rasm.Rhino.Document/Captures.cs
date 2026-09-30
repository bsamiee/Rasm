using Rhino.DocObjects;

namespace Rasm.Rhino.Document;

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

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Captures {
    public static IO<PickCapture> Capture(ObjRef reference) =>
        IO.lift(() => new PickCapture(
            reference.ObjectId,
            reference.GeometryComponentIndex,
            reference.SelectionMethod(),
            Some(reference.SelectionPoint()).Filter(static point => point.IsValid),
            Answers.Found(reference.CurveParameter(out double t) is not null && reference.SelectionMethod() == SelectionMethod.MousePick, t),
            Answers.Found(reference.SurfaceParameter(out double u, out double v) is not null, (u, v)),
            Optional(reference.SelectionView()).Map(static view => Viewports.Identity(view, view.MainViewport)),
            Answers.Present(reference.SelectionViewDetailSerialNumber())));
}

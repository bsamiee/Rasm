using System.Runtime.CompilerServices;
using Rasm.Rhino.Document.Units;
using Rhino.FileIO;
using Rhino.Geometry.Intersect;

namespace Rasm.Rhino.Modeling.Intersections;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveCrossings {
    // --- [EVENTS]
    public static IO<Seq<IntersectionEvent>> Of(Func<CurveIntersections?> host, [CallerArgumentExpression(nameof(host))] string member = "") =>
        (from events in Products.Maybe(host, member)
         select events.Map(static found => Conversions.Rows(found)).IfNone(Seq<IntersectionEvent>())).Bracket();

    public static IO<(Seq<IntersectionEvent> Events, Seq<int> Invalid, string Log)> Validated(
        Func<(CurveIntersections? Events, int[] Invalid, TextLog Log)> host, [CallerArgumentExpression(nameof(host))] string member = "") =>
        (from answer in IO.lift(host)
         from log in use(() => answer.Log)
         from events in Of(() => answer.Events, member)
         select (Events: events, Invalid: toSeq(answer.Invalid), Log: log.ToString())).Bracket();

    // --- [SOLIDS]
    public static IO<(Seq<Curve> Overlaps, Seq<(double Parameter, Point3d Point)> Hits)> CurveBrep(Tolerances tolerances, Curve curve, Brep brep) =>
        from answered in IO.lift(() => (Accepted: Intersection.CurveBrep(curve, brep, tolerances.Absolute, out Curve[] overlaps, out Point3d[] points, out double[] parameters), overlaps, points, parameters))
        from overlaps in Products.Many(() => answered.overlaps, required: false)
        from accepted in Products.Accepted(() => answered.Accepted)
        select (overlaps, Hits: toSeq(answered.parameters).Zip(toSeq(answered.points), static (t, point) => (Parameter: t, Point: point)).Strict());

    public static IO<(Seq<Curve> Curves, Seq<Point3d> Points)> Curves(Func<(bool Accepted, Curve[]? Curves, Point3d[]? Points)> call, [CallerArgumentExpression(nameof(call))] string member = "") =>
        from answered in IO.lift(call)
        from curves in Products.Many(() => answered.Curves, required: false, member)
        from accepted in Products.Accepted(() => answered.Accepted, member)
        select (curves, Conversions.Rows(answered.Points));
}

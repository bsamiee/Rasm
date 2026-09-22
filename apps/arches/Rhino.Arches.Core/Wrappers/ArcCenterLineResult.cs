using Rhino.Geometry;


namespace Rhino.Arches.Core.Wrappers
{

    public record ArchCurveResult(Curve Cv, ArcCenterLineResult CenterLineResult);
    public record ArcCenterLineResult(Line CenterLine,
        Point3d SpringCenter,
        Vector3d PerpendicularDir);

    public record TwoArcsPartResult(
        ArcCenterLineResult CenterLineResult,
        Arc StartArc,
        Arc EndArc,
        Point3d ApexPoint);

    public record MultiArcsPartResult(List<Arc> Arcs,
    ArcCenterLineResult CenterLineResult, Point3d ApexPoint);
}

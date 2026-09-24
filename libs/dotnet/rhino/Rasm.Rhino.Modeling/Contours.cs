using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContourSource {
    public sealed record OfBrep(Brep Brep) : ContourSource;

    public sealed record OfMesh(Mesh Mesh, double Tolerance) : ContourSource;

    public sealed record OfCloud(PointCloud Cloud, double Tolerance) : ContourSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ContourCut {
    public sealed record Section(Plane Plane) : ContourCut;

    public sealed record Sweep(Point3d Start, Point3d End, double Interval) : ContourCut;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Contours {
    public static IO<Seq<Curve>> Create(ContourSource source, ContourCut cut) =>
        from valid in IO.lift(() => Validate(cut))
        from curves in source.Switch(
            cut,
            ofBrep: static (cutting, of) => GeometryResults.Acquire(
                () => cutting.Switch(
                    of,
                    section: static (brep, section) => Brep.CreateContourCurves(brep.Brep, section.Plane),
                    sweep: static (brep, sweep) => Brep.CreateContourCurves(brep.Brep, sweep.Start, sweep.End, sweep.Interval)),
                nameof(Brep.CreateContourCurves)),
            ofMesh: static (cutting, of) => GeometryResults.Acquire(
                () => cutting.Switch(
                    of,
                    section: static (mesh, section) => Mesh.CreateContourCurves(mesh.Mesh, section.Plane, mesh.Tolerance),
                    sweep: static (mesh, sweep) => Mesh.CreateContourCurves(mesh.Mesh, sweep.Start, sweep.End, sweep.Interval, mesh.Tolerance)),
                nameof(Mesh.CreateContourCurves)),
            ofCloud: static (cutting, of) => cutting.Switch(
                of,
                section: static (cloud, section) => GeometryResults.Acquire(() => cloud.Cloud.CreateSectionCurve(section.Plane, cloud.Tolerance), nameof(PointCloud.CreateSectionCurve)),
                sweep: static (cloud, sweep) => GeometryResults.Acquire(() => cloud.Cloud.CreateContourCurves(sweep.Start, sweep.End, sweep.Interval, cloud.Tolerance), nameof(PointCloud.CreateContourCurves))))
        select curves;

    internal static Fin<Unit> Validate(ContourCut cut) =>
        cut.Switch(
            section: static section => Invalid.Unless(section.Plane.IsValid, nameof(Plane.IsValid)),
            sweep: static sweep =>
                from positive in Limits.Above(0.0).Check(sweep.Interval, nameof(ContourCut.Sweep.Interval))
                from apart in Degenerate.Unless(sweep.Start.DistanceTo(sweep.End) > positive, nameof(ContourCut.Sweep))
                select unit);
}

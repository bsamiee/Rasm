using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class CloudCurves {
    public static readonly CloudCurves Splines = new(createSpline: true, createPolyline: false);
    public static readonly CloudCurves Polylines = new(createSpline: false, createPolyline: true);
    public static readonly CloudCurves Both = new(createSpline: true, createPolyline: true);

    public bool CreateSpline { get; }

    public bool CreatePolyline { get; }
}

public sealed record CloudSlab(double MaxDistance, double MinDistance, bool OpenCurves, CloudCurves Curves, double FitTolerance);

[Union]
public abstract partial record ContourSource {
    public sealed record OfBrep(Brep Brep) : ContourSource;

    public sealed record OfMesh(Mesh Mesh) : ContourSource;

    public sealed record OfCloud(PointCloud Cloud, CloudSlab Slab) : ContourSource;
}

[Union]
public abstract partial record ContourCut {
    public sealed record Section(Plane Plane) : ContourCut;

    public sealed record Sweep(Point3d Start, Point3d End, double Interval) : ContourCut;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Contours {
    public static IO<Seq<Curve>> Create(ContourSource source, ContourCut cut, Tolerances tolerances) =>
        source.Switch(
            (Cut: cut, Tolerances: tolerances),
            ofBrep: static (state, of) => Copies.Acquire(
                () => state.Cut.Switch(
                    of.Brep,
                    section: static (brep, section) => Brep.CreateContourCurves(brep, section.Plane),
                    sweep: static (brep, sweep) => Brep.CreateContourCurves(brep, sweep.Start, sweep.End, sweep.Interval)),
                nameof(Brep.CreateContourCurves)),
            ofMesh: static (state, of) => Copies.Acquire(
                () => state.Cut.Switch(
                    (of.Mesh, state.Tolerances.MeshIntersection),
                    section: static (mesh, section) => Mesh.CreateContourCurves(mesh.Mesh, section.Plane, mesh.MeshIntersection),
                    sweep: static (mesh, sweep) => Mesh.CreateContourCurves(mesh.Mesh, sweep.Start, sweep.End, sweep.Interval, mesh.MeshIntersection)),
                nameof(Mesh.CreateContourCurves)),
            ofCloud: static (state, of) => state.Cut.Switch(
                (Of: of, state.Tolerances.Absolute),
                section: static (cloud, section) => Copies.Acquire(
                    () => cloud.Of.Cloud.CreateSectionCurve(section.Plane, cloud.Absolute, cloud.Of.Slab.MaxDistance, cloud.Of.Slab.MinDistance, cloud.Of.Slab.OpenCurves, cloud.Of.Slab.Curves.CreateSpline, cloud.Of.Slab.Curves.CreatePolyline, cloud.Of.Slab.FitTolerance),
                    nameof(PointCloud.CreateSectionCurve)),
                sweep: static (cloud, sweep) => Copies.Acquire(
                    () => cloud.Of.Cloud.CreateContourCurves(sweep.Start, sweep.End, sweep.Interval, cloud.Absolute, cloud.Of.Slab.MaxDistance, cloud.Of.Slab.MinDistance, cloud.Of.Slab.OpenCurves, cloud.Of.Slab.Curves.CreateSpline, cloud.Of.Slab.Curves.CreatePolyline, cloud.Of.Slab.FitTolerance),
                    nameof(PointCloud.CreateContourCurves))));
}

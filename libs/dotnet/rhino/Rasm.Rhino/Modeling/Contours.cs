using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;

namespace Rasm.Rhino.Modeling;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum(SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class CloudCurves {
    public static readonly CloudCurves Splines = new(createSpline: true, createPolyline: false);
    public static readonly CloudCurves Polylines = new(createSpline: false, createPolyline: true);
    public static readonly CloudCurves Both = new(createSpline: true, createPolyline: true);

    public bool CreateSpline { get; }

    public bool CreatePolyline { get; }
}

public sealed record CloudSlab(double MaxDistance, double MinDistance, bool OpenCurves, CloudCurves Curves, double FitTolerance);

[Union<Brep, Mesh, OfCloud>(MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ContourSource {
    public sealed record OfCloud(PointCloud Cloud, CloudSlab Slab);
}

[Union<Plane, Sweep>(MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class ContourCut {
    public sealed record Sweep(Point3d Start, Point3d End, double Interval);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Contours {
    public static IO<Seq<Curve>> Create(ContourSource source, ContourCut cut, Tolerances tolerances) =>
        source.Switch(
            (Cut: cut, Tolerances: tolerances),
            brep: static (state, brep) => (
                Create: fun(() => state.Cut.Switch(
                    brep,
                    plane: static (geometry, plane) => Brep.CreateContourCurves(geometry, plane),
                    sweep: static (geometry, sweep) => Brep.CreateContourCurves(geometry, sweep.Start, sweep.End, sweep.Interval))),
                Member: nameof(Brep.CreateContourCurves)),
            mesh: static (state, mesh) => (
                Create: fun(() => state.Cut.Switch(
                    (Mesh: mesh, state.Tolerances.MeshIntersection),
                    plane: static (geometry, plane) => Mesh.CreateContourCurves(geometry.Mesh, plane, geometry.MeshIntersection),
                    sweep: static (geometry, sweep) => Mesh.CreateContourCurves(geometry.Mesh, sweep.Start, sweep.End, sweep.Interval, geometry.MeshIntersection))),
                Member: nameof(Mesh.CreateContourCurves)),
            ofCloud: static (state, of) => state.Cut.Switch(
                (Of: of, state.Tolerances.Absolute),
                plane: static (cloud, plane) => (
                    Create: fun(() => cloud.Of.Cloud.CreateSectionCurve(plane, cloud.Absolute, cloud.Of.Slab.MaxDistance, cloud.Of.Slab.MinDistance, cloud.Of.Slab.OpenCurves, cloud.Of.Slab.Curves.CreateSpline, cloud.Of.Slab.Curves.CreatePolyline, cloud.Of.Slab.FitTolerance)),
                    Member: nameof(PointCloud.CreateSectionCurve)),
                sweep: static (cloud, sweep) => (
                    Create: fun(() => cloud.Of.Cloud.CreateContourCurves(sweep.Start, sweep.End, sweep.Interval, cloud.Absolute, cloud.Of.Slab.MaxDistance, cloud.Of.Slab.MinDistance, cloud.Of.Slab.OpenCurves, cloud.Of.Slab.Curves.CreateSpline, cloud.Of.Slab.Curves.CreatePolyline, cloud.Of.Slab.FitTolerance)),
                    Member: nameof(PointCloud.CreateContourCurves)))) switch {
                        var operation => Copies.Acquire(operation.Create, operation.Member),
                    };
}

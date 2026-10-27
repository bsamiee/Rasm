using System.Globalization;
using Rasm.Spatial;
using Rasm.Rhino.Modeling.Curves;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
public enum SurfaceDirection { U = 0, V = 1 }

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
[ValidationError<RhinoFault>]
public sealed partial class PointGrid {
    public Seq<Point3d> Points { get; }

    public int UCount { get; }

    public int VCount => Points.Count / UCount;

    static partial void ValidateFactoryArguments(ref RhinoFault? validationError, ref Seq<Point3d> points, ref int uCount) =>
        validationError = uCount >= 2 && points.Count / uCount >= 2 && points.Count % uCount is 0
            ? null
            : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"grid of {points.Count} points with {uCount} in U"), RhinoReason.Outside);
}

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PointSurface {
    public sealed record Through(bool UClosed, bool VClosed) : PointSurface;

    public sealed record ControlPoints : PointSurface;
}

[ComplexValueObject(SkipToString = true)]
[ValidationError<RhinoFault>]
public sealed partial class HermiteGrid {
    public AscendingParameters U { get; }

    public AscendingParameters V { get; }

    public Seq<(Point3d Point, Vector3d UTangent, Vector3d VTangent, Vector3d Twist)> Stations { get; }

    static partial void ValidateFactoryArguments(
        ref RhinoFault? validationError, ref AscendingParameters u, ref AscendingParameters v, ref Seq<(Point3d Point, Vector3d UTangent, Vector3d VTangent, Vector3d Twist)> stations) =>
        validationError = stations.Count == u.Values.Count * v.Values.Count
            ? null
            : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"{stations.Count} Hermite stations for a {u.Values.Count} by {v.Values.Count} grid"), RhinoReason.Outside);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceConstruction {
    // --- [FROM_POINTS]
    public static IO<NurbsSurface> FromPoints(PointGrid grid, int uDegree, int vDegree, PointSurface method) =>
        method.Switch(
            (Grid: grid, U: uDegree, V: vDegree),
            through: static (at, through) => Products.Made(NurbsSurface.CreateThroughPoints(at.Grid.Points, at.Grid.UCount, at.Grid.VCount, at.U, at.V, through.UClosed, through.VClosed)),
            controlPoints: static (at, _) => Products.Made(NurbsSurface.CreateFromPoints(at.Grid.Points, at.Grid.UCount, at.Grid.VCount, at.U, at.V)));

    // --- [HERMITE]
    public static IO<NurbsSurface> Hermite(HermiteGrid grid) =>
        from hermite in use(() => new HermiteSurface(grid.U.Values.Count, grid.V.Values.Count))
        from filled in IO.lift(() => Filled(hermite, grid))
        from surface in Products.Made(hermite.ToNurbsSurface())
        select surface;

    private static Unit Filled(HermiteSurface hermite, HermiteGrid grid) {
        _ = grid.U.Values.Iter(hermite.SetUParameterAt);
        _ = grid.V.Values.Iter(hermite.SetVParameterAt);
        _ = toSeq(grid.Stations.Chunk(grid.V.Values.Count)).Iter((u, row) => toSeq(row).Iter((v, station) => {
            hermite.SetPointAt(u, v, station.Point);
            hermite.SetUTangentAt(u, v, station.UTangent);
            hermite.SetVTangentAt(u, v, station.VTangent);
            hermite.SetTwistAt(u, v, station.Twist);
        }));
        return unit;
    }

    // --- [REFIT]
    public static IO<(NurbsSurface A, NurbsSurface B)> MakeCompatible(Surface a, Surface b) =>
        from made in IO.lift(() => (Accepted: NurbsSurface.MakeCompatible(a, b, out NurbsSurface first, out NurbsSurface second), First: first, Second: second))
        from heldA in Products.Held(made.First)
        from heldB in Products.Held(made.Second)
        from compatible in IO.lift(() => RhinoFault.Unless(made.Accepted))
        from compatibleA in IO.lift(() => Conversions.Found(heldA))
        from compatibleB in IO.lift(() => Conversions.Found(heldB))
        select (A: compatibleA, B: compatibleB);

    // --- [REVOLUTION]
    public static IO<RevSurface> Hyperboloid(Plane frame, double waist, double axial, Interval span) =>
        unitIO.Bind(_ => Products.Made(RevSurface.Create(
            new Line(frame.PointAt(waist, waist * span.T0, axial * span.T0), frame.PointAt(waist, waist * span.T1, axial * span.T1)),
            new Line(frame.Origin, frame.ZAxis, 1d))));

    // --- [LOWERINGS]
    extension(PolynomialPatch patch) {
        public IO<NurbsSurface> ToNurbs(Plane frame) =>
            from grid in IO.pure((Rows: patch.Net.X.RowCount, Columns: patch.Net.X.ColumnCount, Controls: Controls(patch.Net, frame)))
            from surface in Products.Made(NurbsSurface.CreateFromPoints(grid.Controls.Map(static control => control.Place), grid.Rows, grid.Columns, grid.Rows - 1, grid.Columns - 1))
            from shaped in IO.lift(() => RhinoFault.Unless(
                (grid.Controls.ForAll(static control => control.Weight is 1d) || (surface.MakeRational() && grid.Controls.ForAll(control => surface.Points.SetPoint(control.U, control.V, control.Place, control.Weight))))
                && surface.SetDomain((int)SurfaceDirection.U, new Interval(patch.U.Start, patch.U.End)) && surface.SetDomain((int)SurfaceDirection.V, new Interval(patch.V.Start, patch.V.End))))
            select surface;
    }

    private static Seq<(int U, int V, double Weight, Point3d Place)> Controls(HomogeneousGrid net, Plane frame) =>
        (from u in toSeq(Range(0, net.X.RowCount)) from v in toSeq(Range(0, net.X.ColumnCount)) select (U: u, V: v, Weight: net.W[u, v], Place: (new Vector3D<double>(net.X[u, v], net.Y[u, v], net.Z[u, v]) / net.W[u, v]).ToPoint3d(frame))).Strict();
}

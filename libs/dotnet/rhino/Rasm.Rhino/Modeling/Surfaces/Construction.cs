using System.Globalization;
using Rasm.Rhino.Modeling.Curves;
using Rhino.Geometry.Collections;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class SurfaceDirection {
    public static readonly SurfaceDirection U = new(0, static surface => surface.KnotsU, static (surface, degree) => surface.IncreaseDegreeU(degree));
    public static readonly SurfaceDirection V = new(1, static surface => surface.KnotsV, static (surface, degree) => surface.IncreaseDegreeV(degree));

    public int Index { get; }

    public SurfaceDirection Across => this == U ? V : U;

    public Interval Domain(Surface surface) => surface.Domain(Index);

    [UseDelegateFromConstructor]
    public partial NurbsSurfaceKnotList Knots(NurbsSurface surface);

    [UseDelegateFromConstructor]
    public partial bool IncreaseDegree(NurbsSurface surface, int degree);
}

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
            through: static (at, through) => Products.Of(() => NurbsSurface.CreateThroughPoints(at.Grid.Points, at.Grid.UCount, at.Grid.VCount, at.U, at.V, through.UClosed, through.VClosed)),
            controlPoints: static (at, _) => Products.Of(() => NurbsSurface.CreateFromPoints(at.Grid.Points, at.Grid.UCount, at.Grid.VCount, at.U, at.V)));

    // --- [HERMITE]
    public static IO<NurbsSurface> Hermite(HermiteGrid grid) =>
        from hermite in Products.Of(() => new HermiteSurface(grid.U.Values.Count, grid.V.Values.Count))
        from filled in IO.lift(() => Filled(hermite, grid))
        from surface in Products.Of(hermite.ToNurbsSurface)
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
        from answered in IO.lift(() => (Accepted: NurbsSurface.MakeCompatible(a, b, out NurbsSurface first, out NurbsSurface second), First: first, Second: second))
        from first in Products.Maybe(() => answered.First, nameof(NurbsSurface.MakeCompatible))
        from second in Products.Maybe(() => answered.Second, nameof(NurbsSurface.MakeCompatible))
        from compatible in IO.lift(() =>
            from accepted in RhinoFault.Unless(answered.Accepted, RhinoReason.Declined, answered.Accepted)
            from pair in (Conversions.Required(first), Conversions.Required(second)).Apply(static (held, other) => (A: held, B: other)).As()
            select pair)
        select compatible;

    // --- [REVOLUTION]
    public static IO<RevSurface> Hyperboloid(Plane frame, double waist, double axial, Interval span) =>
        Products.Of(() => RevSurface.Create(
            new Line(frame.PointAt(waist, waist * span.T0, axial * span.T0), frame.PointAt(waist, waist * span.T1, axial * span.T1)),
            new Line(frame.Origin, frame.ZAxis, 1d)));
}

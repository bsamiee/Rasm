using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
public enum SurfaceDirection { U = 0, V = 1 }

public enum NetworkContinuity { Loose = 0, Position = 1, Tangency = 2, Curvature = 3 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NetworkCurves(Seq<Curve> Curves, NetworkContinuity Start, NetworkContinuity End);

[Union(MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NetworkMethod {
    public sealed record AutoSorted(Seq<Curve> Curves, NetworkContinuity Continuity) : NetworkMethod;

    public sealed record Ordered(NetworkCurves U, NetworkCurves V) : NetworkMethod;
}

[Union<Curve, Line, Polyline>(MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class RevolveProfile;

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class PointGrid {
    public Seq<Point3d> Points { get; }

    public int UCount { get; }

    public int VCount => Points.Count / UCount;

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref Seq<Point3d> points, ref int uCount) =>
        validationError = uCount >= 2 && points.Count / uCount >= 2 && points.Count % uCount == 0 ? null : new InvalidRhinoValue();
}

[Union<Cone, Cylinder, Sphere, Torus>(MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class AnalyticShape;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceConstruction {
    // --- [FROM_CURVES]
    public static IO<NurbsSurface> Network(NetworkMethod method, double interiorTolerance, Tolerances tolerances) =>
        Copies.Acquire(
            Fin<NurbsSurface> () => method.Switch(
                (Interior: interiorTolerance, Tolerances: tolerances),
                autoSorted: static (network, sorted) => (
                    Surface: NurbsSurface.CreateNetworkSurface(
                        sorted.Curves, (int)sorted.Continuity, network.Tolerances.Absolute, network.Interior, network.Tolerances.Angle, out int error),
                    Error: error),
                ordered: static (network, ordered) => (
                    Surface: NurbsSurface.CreateNetworkSurface(
                        ordered.U.Curves, (int)ordered.U.Start, (int)ordered.U.End, ordered.V.Curves, (int)ordered.V.Start, (int)ordered.V.End,
                        network.Tolerances.Absolute, network.Interior, network.Tolerances.Angle, out int error),
                    Error: error)) switch {
                        (var surface, 0) => surface,
                        (_, var error) => new NetworkSurfaceFailed((NetworkFailure)error),
                    },
            nameof(NurbsSurface.CreateNetworkSurface));

    public static IO<RevSurface> Revolved(RevolveProfile profile, Line axis, Option<(double StartRadians, double EndRadians)> angles) =>
        Copies.Acquire(
            () => angles.Match(
                Some: sweep => profile.Switch(
                    (Axis: axis, Sweep: sweep),
                    curve: static (revolution, curve) => RevSurface.Create(curve, revolution.Axis, revolution.Sweep.StartRadians, revolution.Sweep.EndRadians),
                    line: static (revolution, line) => RevSurface.Create(line, revolution.Axis, revolution.Sweep.StartRadians, revolution.Sweep.EndRadians),
                    polyline: static (revolution, polyline) => RevSurface.Create(polyline, revolution.Axis, revolution.Sweep.StartRadians, revolution.Sweep.EndRadians)),
                None: () => profile.Switch(
                    axis,
                    curve: static (about, curve) => RevSurface.Create(curve, about),
                    line: static (about, line) => RevSurface.Create(line, about),
                    polyline: static (about, polyline) => RevSurface.Create(polyline, about))),
            nameof(RevSurface.Create));

    // --- [FROM_POINTS]
    public static IO<NurbsSurface> FromPoints(PointGrid grid, int uDegree, int vDegree, Option<(bool UClosed, bool VClosed)> interpolation) =>
        interpolation.Match(
            Some: closed => Copies.Acquire(
                () => NurbsSurface.CreateThroughPoints(grid.Points, grid.UCount, grid.VCount, uDegree, vDegree, closed.UClosed, closed.VClosed),
                nameof(NurbsSurface.CreateThroughPoints)),
            None: () => Copies.Acquire(
                () => NurbsSurface.CreateFromPoints(grid.Points, grid.UCount, grid.VCount, uDegree, vDegree), nameof(NurbsSurface.CreateFromPoints)));

    // --- [PRIMITIVES]
    public static IO<NurbsSurface> ToNurbsSurface(AnalyticShape shape) =>
        Copies.Acquire(
            () => shape.Switch(
                cone: static cone => cone.ToNurbsSurface(),
                cylinder: static cylinder => cylinder.ToNurbsSurface(),
                sphere: static sphere => sphere.ToNurbsSurface(),
                torus: static torus => torus.ToNurbsSurface()),
            nameof(Sphere.ToNurbsSurface));

    public static IO<RevSurface> ToRevSurface(AnalyticShape shape) =>
        Copies.Acquire(
            () => shape.Switch(
                cone: static cone => cone.ToRevSurface(),
                cylinder: static cylinder => cylinder.ToRevSurface(),
                sphere: static sphere => sphere.ToRevSurface(),
                torus: static torus => torus.ToRevSurface()),
            nameof(Sphere.ToRevSurface));

    // --- [REFIT]
    public static IO<(NurbsSurface A, NurbsSurface B)> MakeCompatible(Surface a, Surface b) =>
        Copies.Owned(
            IO.lift(() => Refused.Unless(NurbsSurface.MakeCompatible(a, b, out NurbsSurface first, out NurbsSurface second), (A: first, B: second), nameof(NurbsSurface.MakeCompatible))),
            static pair => (Measurements.Valid(pair.A, nameof(NurbsSurface.MakeCompatible)).ToValidation(), Measurements.Valid(pair.B, nameof(NurbsSurface.MakeCompatible)).ToValidation())
                .Apply(static (first, second) => (A: first, B: second)).As().ToFin(),
            static pair => DisposalOps.Release(Seq(pair.A, pair.B)));

    // --- [EDITS]
    public static IO<Surface> VariableOffset(
        Surface surface, (double UMinVMin, double UMinVMax, double UMaxVMin, double UMaxVMax) corners, Seq<(Point2d Parameter, double Distance)> interior,
        Tolerances tolerances) =>
        Copies.Acquire(
            () => interior.IsEmpty
                ? surface.VariableOffset(corners.UMinVMin, corners.UMinVMax, corners.UMaxVMin, corners.UMaxVMax, tolerances.Absolute)
                : surface.VariableOffset(
                    corners.UMinVMin, corners.UMinVMax, corners.UMaxVMin, corners.UMaxVMax,
                    interior.Map(static row => row.Parameter), interior.Map(static row => row.Distance), tolerances.Absolute),
            nameof(Surface.VariableOffset));
}

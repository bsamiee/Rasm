using System.Diagnostics;
using Rasm.Rhino.Document.Notation;
using Rasm.Rhino.Document.Shapes;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
public enum SurfaceDirection { U = 0, V = 1 }

public enum NetworkContinuity { Loose = 0, Position = 1, Tangency = 2, Curvature = 3 }

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NetworkCurves(Seq<Curve> Curves, NetworkContinuity Start, NetworkContinuity End);

[Union]
public abstract partial record NetworkMethod {
    public sealed record AutoSorted(Seq<Curve> Curves, NetworkContinuity Continuity) : NetworkMethod;

    public sealed record Ordered(NetworkCurves U, NetworkCurves V) : NetworkMethod;
}

[Union<Curve, Line, Polyline>]
public sealed partial class RevolveProfile;

[ComplexValueObject]
[ValidationError<InvalidRhinoValue>]
public sealed partial class PointGrid {
    public Seq<Point3d> Points { get; }

    public int UCount { get; }

    public int VCount => Points.Count / UCount;

    static partial void ValidateFactoryArguments(ref InvalidRhinoValue? validationError, ref Seq<Point3d> points, ref int uCount) =>
        validationError = uCount >= 2 && points.Count >= 2 * uCount && points.Count % uCount == 0 ? null : new InvalidRhinoValue();
}

[Union<Cone, Cylinder, Sphere, Torus>]
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
                        (_, 1) => new NetworkSurfaceFailed.Sorting(),
                        (_, 2) => new NetworkSurfaceFailed.Initialization(),
                        (_, 3) => new NetworkSurfaceFailed.Build(),
                        (_, 4) => new NetworkSurfaceFailed.Validity(),
                        _ => throw new UnreachableException(),
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
    public static IO<NurbsSurface> FromPoints(PointGrid grid, int uDegree, int vDegree) =>
        Copies.Acquire(() => NurbsSurface.CreateFromPoints(grid.Points, grid.UCount, grid.VCount, uDegree, vDegree), nameof(NurbsSurface.CreateFromPoints));

    public static IO<NurbsSurface> ThroughPoints(PointGrid grid, int uDegree, int vDegree, bool uClosed, bool vClosed) =>
        Copies.Acquire(
            () => NurbsSurface.CreateThroughPoints(grid.Points, grid.UCount, grid.VCount, uDegree, vDegree, uClosed, vClosed),
            nameof(NurbsSurface.CreateThroughPoints));

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
            IO.lift(() => (Made: NurbsSurface.MakeCompatible(a, b, out NurbsSurface first, out NurbsSurface second), A: first, B: second)),
            static answer =>
                from made in Refused.Unless(answer.Made, answer, nameof(NurbsSurface.MakeCompatible))
                from first in Measurements.Valid(made.A, nameof(NurbsSurface.MakeCompatible))
                from second in Measurements.Valid(made.B, nameof(NurbsSurface.MakeCompatible))
                select (A: first, B: second),
            static answer => DisposalOps.Release(Conversions.Rows(Seq(answer.A, answer.B))));

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

using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [TYPES] ---------------------------------------------------------------------------
public enum SurfaceDirection { U = 0, V = 1 }

public enum NetworkContinuity { Loose = 0, Position = 1, Tangency = 2, Curvature = 3 }

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NetworkMethod {
    public sealed record Auto(Seq<Curve> Curves, NetworkContinuity Continuity) : NetworkMethod;

    public sealed record Uv(Seq<Curve> U, NetworkContinuity UStart, NetworkContinuity UEnd, Seq<Curve> V, NetworkContinuity VStart, NetworkContinuity VEnd) : NetworkMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GridFit {
    public sealed record Control() : GridFit;

    public sealed record Through(bool UClosed, bool VClosed) : GridFit;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CornerSource {
    public sealed record Triangle(Point3d A, Point3d B, Point3d C) : CornerSource;

    public sealed record Quad(Point3d A, Point3d B, Point3d C, Point3d D, Option<double> Tolerance) : CornerSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SurfaceFitMethod {
    public sealed record ToTolerance(int UDegree, int VDegree, double Tolerance) : SurfaceFitMethod;

    public sealed record ToGrid(int UDegree, int VDegree, int UPoints, int VPoints) : SurfaceFitMethod;

    public sealed record InDirection(SurfaceDirection Direction, int PointCount, LoftType Kind, double RefitTolerance) : SurfaceFitMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtrudeTerminal {
    public sealed record Along(Vector3d Direction) : ExtrudeTerminal;

    public sealed record ToApex(Point3d Apex) : ExtrudeTerminal;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RollingBallMethod {
    public sealed record Auto() : RollingBallMethod;

    public sealed record Flipped(bool FlipA, bool FlipB) : RollingBallMethod;

    public sealed record AtUv(Point2d UvA, Point2d UvB) : RollingBallMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record AnalyticSurface {
    public sealed record OfCone(Cone Cone) : AnalyticSurface;

    public sealed record OfCylinder(Cylinder Cylinder) : AnalyticSurface;

    public sealed record OfSphere(Sphere Sphere) : AnalyticSurface;

    public sealed record OfTorus(Torus Torus) : AnalyticSurface;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PlaneFrame {
    public sealed record OfPlane(Plane Plane) : PlaneFrame;

    public sealed record OfLine(Line Line, Vector3d VectorInPlane) : PlaneFrame;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RevolveProfile {
    public sealed record OfCurve(Curve Curve) : RevolveProfile;

    public sealed record OfLine(Line Line) : RevolveProfile;

    public sealed record OfPolyline(Polyline Polyline) : RevolveProfile;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SumExtent {
    public sealed record ByDirection(Vector3d Direction) : SumExtent;

    public sealed record ByCurve(Curve Curve) : SumExtent;
}

public sealed record VariableOffsetOptions(double UMinVMin, double UMinVMax, double UMaxVMin, double UMaxVMax, Seq<(Point2d Uv, double Distance)> Interior);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceConstruction {
    // --- [FROM_CURVES]
    public static IO<NurbsSurface> Network(NetworkMethod method, double edgeTolerance, double interiorTolerance, double angleTolerance) =>
        method.Switch(
            (Edge: edgeTolerance, Interior: interiorTolerance, Angle: angleTolerance),
            auto: static (tolerances, auto) =>
                from valid in IO.lift(() => Invalid.Unless(!auto.Curves.IsEmpty, nameof(NetworkMethod.Auto.Curves)))
                from surface in Networked(() => (Surface: NurbsSurface.CreateNetworkSurface(auto.Curves, (int)auto.Continuity, tolerances.Edge, tolerances.Interior, tolerances.Angle, out int error), Error: error))
                select surface,
            uv: static (tolerances, uv) =>
                from valid in IO.lift(() => GeometryResults.Sets((uv.U, nameof(NetworkMethod.Uv.U)), (uv.V, nameof(NetworkMethod.Uv.V))))
                from surface in Networked(() => (
                    Surface: NurbsSurface.CreateNetworkSurface(uv.U, (int)uv.UStart, (int)uv.UEnd, uv.V, (int)uv.VStart, (int)uv.VEnd, tolerances.Edge, tolerances.Interior, tolerances.Angle, out int error),
                    Error: error))
                select surface);

    public static IO<NurbsSurface> Ruled(Curve a, Curve b) =>
        GeometryResults.Acquire(() => NurbsSurface.CreateRuledSurface(a, b), nameof(NurbsSurface.CreateRuledSurface));

    public static IO<NurbsSurface> RailRevolve(Curve profile, Curve rail, Line axis, bool scaleHeight) =>
        GeometryResults.Acquire(() => NurbsSurface.CreateRailRevolvedSurface(profile, rail, axis, scaleHeight), nameof(NurbsSurface.CreateRailRevolvedSurface));

    public static IO<RevSurface> Revolve(RevolveProfile profile, Line axis, Option<(double StartRadians, double EndRadians)> sweep) =>
        GeometryResults.Acquire(
            () => sweep.Match(
                Some: angles => profile.Switch(
                    (Axis: axis, Angles: angles),
                    ofCurve: static (revolution, curve) => RevSurface.Create(curve.Curve, revolution.Axis, revolution.Angles.StartRadians, revolution.Angles.EndRadians),
                    ofLine: static (revolution, line) => RevSurface.Create(line.Line, revolution.Axis, revolution.Angles.StartRadians, revolution.Angles.EndRadians),
                    ofPolyline: static (revolution, polyline) => RevSurface.Create(polyline.Polyline, revolution.Axis, revolution.Angles.StartRadians, revolution.Angles.EndRadians)),
                None: () => profile.Switch(
                    axis,
                    ofCurve: static (about, curve) => RevSurface.Create(curve.Curve, about),
                    ofLine: static (about, line) => RevSurface.Create(line.Line, about),
                    ofPolyline: static (about, polyline) => RevSurface.Create(polyline.Polyline, about))),
            nameof(RevSurface.Create));

    public static IO<SumSurface> Sum(Curve curve, SumExtent extent) =>
        GeometryResults.Acquire(
            () => extent.Switch(
                curve,
                byDirection: static (profile, direction) => SumSurface.Create(profile, direction.Direction),
                byCurve: static (profile, other) => SumSurface.Create(profile, other.Curve)),
            nameof(SumSurface.Create));

    public static IO<Surface> Extruded(Curve profile, ExtrudeTerminal terminal) =>
        terminal.Switch(
            profile,
            along: static (curve, along) => GeometryResults.Acquire(() => Surface.CreateExtrusion(curve, along.Direction), nameof(Surface.CreateExtrusion)),
            toApex: static (curve, apex) => GeometryResults.Acquire(() => Surface.CreateExtrusionToPoint(curve, apex.Apex), nameof(Surface.CreateExtrusionToPoint)));

    public static IO<NurbsCurve> GeodesicCurve(Surface surface, Seq<Point2d> points, double tolerance, bool periodic) =>
        GeometryResults.Acquire(() => NurbsSurface.CreateCurveOnSurface(surface, points, tolerance, periodic), nameof(NurbsSurface.CreateCurveOnSurface));

    private static IO<NurbsSurface> Networked(Func<(NurbsSurface? Surface, int Error)> create) =>
        from answer in IO.lift(create)
        from surface in DisposalOps.OnFailure(
            IO.lift(() => answer.Error switch {
                1 => Fin.Fail<NurbsSurface>(new NetworkSurfaceFailed.Sorting()),
                2 => Fin.Fail<NurbsSurface>(new NetworkSurfaceFailed.Initialization()),
                3 => Fin.Fail<NurbsSurface>(new NetworkSurfaceFailed.Build()),
                4 => Fin.Fail<NurbsSurface>(new NetworkSurfaceFailed.Validity()),
                _ => Missing.Unless(answer.Surface, nameof(NurbsSurface.CreateNetworkSurface)),
            }),
            DisposalOps.Release(Optional(answer.Surface).ToSeq()))
        select surface;

    // --- [FROM_POINTS]
    public static IO<NurbsSurface> Grid(Seq<Point3d> points, int uCount, int vCount, int uDegree, int vDegree, GridFit fit) =>
        fit.Switch(
            (Counted: CountMismatch.Unless(uCount * vCount, points.Count, nameof(points)), Points: points, UCount: uCount, VCount: vCount, UDegree: uDegree, VDegree: vDegree),
            control: static (grid, _) => GeometryResults.Acquire(
                () => grid.Counted.Map(_ => NurbsSurface.CreateFromPoints(grid.Points, grid.UCount, grid.VCount, grid.UDegree, grid.VDegree)),
                nameof(NurbsSurface.CreateFromPoints)),
            through: static (grid, through) => GeometryResults.Acquire(
                () => grid.Counted.Map(_ => NurbsSurface.CreateThroughPoints(grid.Points, grid.UCount, grid.VCount, grid.UDegree, grid.VDegree, through.UClosed, through.VClosed)),
                nameof(NurbsSurface.CreateThroughPoints)));

    public static IO<NurbsSurface> Corners(CornerSource source) =>
        GeometryResults.Acquire(
            () => source.Switch(
                triangle: static triangle => NurbsSurface.CreateFromCorners(triangle.A, triangle.B, triangle.C),
                quad: static quad => quad.Tolerance.Match(
                    Some: tolerance => NurbsSurface.CreateFromCorners(quad.A, quad.B, quad.C, quad.D, tolerance),
                    None: () => NurbsSurface.CreateFromCorners(quad.A, quad.B, quad.C, quad.D))),
            nameof(NurbsSurface.CreateFromCorners));

    public static IO<NurbsSurface> PlaneGrid(Plane plane, Interval u, Interval v, int uDegree, int vDegree, int uPoints, int vPoints) =>
        GeometryResults.Acquire(() => Invalid.Unless(plane.IsValid, nameof(plane)).Map(_ => NurbsSurface.CreateFromPlane(plane, u, v, uDegree, vDegree, uPoints, vPoints)), nameof(NurbsSurface.CreateFromPlane));

    public static IO<PlaneSurface> BoundedPlane(PlaneFrame frame, BoundingBox box) =>
        GeometryResults.Acquire(
            () => Invalid.Unless(box.IsValid, nameof(box)).Map(_ => frame.Switch(
                box,
                ofPlane: static (bounds, frame) => PlaneSurface.CreateThroughBox(frame.Plane, bounds),
                ofLine: static (bounds, frame) => PlaneSurface.CreateThroughBox(frame.Line, frame.VectorInPlane, bounds))),
            nameof(PlaneSurface.CreateThroughBox));

    public static IO<NurbsSurface> Analytic(AnalyticSurface source) =>
        source.Switch(
            ofCone: static cone => GeometryResults.Acquire(() => NurbsSurface.CreateFromCone(cone.Cone), nameof(NurbsSurface.CreateFromCone)),
            ofCylinder: static cylinder => GeometryResults.Acquire(() => NurbsSurface.CreateFromCylinder(cylinder.Cylinder), nameof(NurbsSurface.CreateFromCylinder)),
            ofSphere: static sphere => GeometryResults.Acquire(() => NurbsSurface.CreateFromSphere(sphere.Sphere), nameof(NurbsSurface.CreateFromSphere)),
            ofTorus: static torus => GeometryResults.Acquire(() => NurbsSurface.CreateFromTorus(torus.Torus), nameof(NurbsSurface.CreateFromTorus)));

    public static IO<RevSurface> AnalyticRevolved(AnalyticSurface source) =>
        source.Switch(
            ofCone: static cone => GeometryResults.Acquire(() => RevSurface.CreateFromCone(cone.Cone), nameof(RevSurface.CreateFromCone)),
            ofCylinder: static cylinder => GeometryResults.Acquire(() => RevSurface.CreateFromCylinder(cylinder.Cylinder), nameof(RevSurface.CreateFromCylinder)),
            ofSphere: static sphere => GeometryResults.Acquire(() => RevSurface.CreateFromSphere(sphere.Sphere), nameof(RevSurface.CreateFromSphere)),
            ofTorus: static torus => GeometryResults.Acquire(() => RevSurface.CreateFromTorus(torus.Torus), nameof(RevSurface.CreateFromTorus)));

    // --- [REFIT]
    public static IO<Surface> Fit(Surface surface, SurfaceFitMethod method) =>
        method.Switch(
            surface,
            toTolerance: static (source, fit) => GeometryResults.Acquire(() => source.Fit(fit.UDegree, fit.VDegree, fit.Tolerance), nameof(Surface.Fit)),
            toGrid: static (source, fit) => GeometryResults.Acquire<Surface>(() => source.Rebuild(fit.UDegree, fit.VDegree, fit.UPoints, fit.VPoints), nameof(Surface.Rebuild)),
            inDirection: static (source, fit) => GeometryResults.Acquire<Surface>(
                () => source.RebuildOneDirection((int)fit.Direction, fit.PointCount, fit.Kind, fit.RefitTolerance),
                nameof(Surface.RebuildOneDirection)));

    public static IO<NurbsSurface> SubDFriendly(Surface surface) =>
        GeometryResults.Acquire(() => NurbsSurface.CreateSubDFriendly(surface), nameof(NurbsSurface.CreateSubDFriendly));

    public static IO<Surface> Periodic(Surface surface, SurfaceDirection direction, bool smooth) =>
        GeometryResults.Acquire(() => Surface.CreatePeriodicSurface(surface, (int)direction, smooth), nameof(Surface.CreatePeriodicSurface));

    public static IO<Surface> SoftEdit(Surface surface, Point2d uv, Vector3d delta, double uLength, double vLength, double tolerance, bool fixEnds) =>
        GeometryResults.Acquire(() => Surface.CreateSoftEditSurface(surface, uv, delta, uLength, vLength, tolerance, fixEnds), nameof(Surface.CreateSoftEditSurface));

    public static IO<(NurbsSurface A, NurbsSurface B)> Compatible(Surface a, Surface b) =>
        IO.lift(() => Refused.Unless(NurbsSurface.MakeCompatible(a, b, out NurbsSurface nurbsA, out NurbsSurface nurbsB), (A: nurbsA, B: nurbsB), nameof(NurbsSurface.MakeCompatible)));

    public static IO<NurbsSurface> MatchToCurve(NurbsSurface surface, Curve target, IsoStatus side, double maxEndDistance, double maxInteriorDistance, double matchTolerance, int maxLevel) =>
        GeometryResults.Acquire(
            () => Invalid.Unless(side != IsoStatus.None, nameof(side)).Map(_ => surface.MatchToCurve(side, target, maxEndDistance, maxInteriorDistance, matchTolerance, maxLevel)),
            nameof(NurbsSurface.MatchToCurve));

    public static IO<Surface> VariableOffset(Surface surface, VariableOffsetOptions options, double tolerance) =>
        GeometryResults.Acquire(
            () => options.Interior.IsEmpty
                ? surface.VariableOffset(options.UMinVMin, options.UMinVMax, options.UMaxVMin, options.UMaxVMax, tolerance)
                : surface.VariableOffset(options.UMinVMin, options.UMinVMax, options.UMaxVMin, options.UMaxVMax, options.Interior.Map(static row => row.Uv), options.Interior.Map(static row => row.Distance), tolerance),
            nameof(Surface.VariableOffset));

    // --- [BETWEEN]
    public static IO<Seq<Surface>> RollingBall(Surface a, Surface b, double radius, RollingBallMethod method, double tolerance) =>
        GeometryResults.Acquire(
            () => method.Switch(
                (A: a, B: b, Radius: radius, Tolerance: tolerance),
                auto: static (pair, _) => Surface.CreateRollingBallFillet(pair.A, pair.B, pair.Radius, pair.Tolerance),
                flipped: static (pair, flipped) => Surface.CreateRollingBallFillet(pair.A, flipped.FlipA, pair.B, flipped.FlipB, pair.Radius, pair.Tolerance),
                atUv: static (pair, at) => Surface.CreateRollingBallFillet(pair.A, at.UvA, pair.B, at.UvB, pair.Radius, pair.Tolerance)),
            nameof(Surface.CreateRollingBallFillet),
            emptyFails: true);

    public static IO<Seq<Surface>> Tween(Surface a, Surface b, int count, int samples, double tolerance) =>
        GeometryResults.Acquire(
            () => Surface.CreateTweenSurfacesWithSampling(a, b, count, samples, tolerance),
            nameof(Surface.CreateTweenSurfacesWithSampling),
            emptyFails: true);
}

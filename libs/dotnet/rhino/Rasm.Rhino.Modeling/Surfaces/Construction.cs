using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling.Surfaces;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NetworkMethod {
    public sealed record Auto(Seq<Curve> Curves, int Continuity) : NetworkMethod;

    public sealed record Uv(Seq<Curve> U, int UStart, int UEnd, Seq<Curve> V, int VStart, int VEnd) : NetworkMethod;
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

    public sealed record InDirection(int Direction, int PointCount, LoftType Kind, double RefitTolerance) : SurfaceFitMethod;
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

public sealed record MatchToCurveOptions(IsoStatus Side, double MaxEndDistance, double MaxInteriorDistance, double MatchTolerance, int MaxLevel);

public sealed record SoftEditOptions(Point2d Uv, Vector3d Delta, double ULength, double VLength, double Tolerance, bool FixEnds);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SurfaceConstruction {
    // --- [LIMITS]
    private const int MinimumPoints = 2;

    private static readonly Fin<Limits<int>> Continuity = Limits.AtLeast(0).AtMost(3, nameof(Continuity));

    // --- [DIRECTIONS]
    internal const int AlongU = 0;

    internal const int AlongV = 1;

    internal static Fin<Unit> Directed(int direction, string member) =>
        Invalid.Unless(direction is AlongU or AlongV, member);

    // --- [FROM_CURVES]
    public static IO<NurbsSurface> Network(NetworkMethod method, double edgeTolerance, double interiorTolerance, double angleTolerance) =>
        method.Switch(
            (Edge: edgeTolerance, Interior: interiorTolerance, Angle: angleTolerance),
            auto: static (tolerances, auto) =>
                from valid in IO.lift(() => Validated(Invalid.Unless(!auto.Curves.IsEmpty, nameof(NetworkMethod.Auto.Curves)), Seq((nameof(NetworkMethod.Auto.Continuity), auto.Continuity))))
                from surface in Networked(() => (Surface: NurbsSurface.CreateNetworkSurface(auto.Curves, auto.Continuity, tolerances.Edge, tolerances.Interior, tolerances.Angle, out int error), Error: error))
                select surface,
            uv: static (tolerances, uv) =>
                from valid in IO.lift(() => Validated(
                    GeometryResults.Sets((uv.U, nameof(NetworkMethod.Uv.U)), (uv.V, nameof(NetworkMethod.Uv.V))).Map(static _ => unit),
                    Seq((nameof(NetworkMethod.Uv.UStart), uv.UStart), (nameof(NetworkMethod.Uv.UEnd), uv.UEnd), (nameof(NetworkMethod.Uv.VStart), uv.VStart), (nameof(NetworkMethod.Uv.VEnd), uv.VEnd))))
                from surface in Networked(() => (Surface: NurbsSurface.CreateNetworkSurface(uv.U, uv.UStart, uv.UEnd, uv.V, uv.VStart, uv.VEnd, tolerances.Edge, tolerances.Interior, tolerances.Angle, out int error), Error: error))
                select surface);

    public static IO<NurbsSurface> Ruled(Curve a, Curve b) =>
        IO.lift(() => Missing.Unless(NurbsSurface.CreateRuledSurface(a, b), nameof(NurbsSurface.CreateRuledSurface)));

    public static IO<NurbsSurface> RailRevolve(Curve profile, Curve rail, Line axis, bool scaleHeight) =>
        IO.lift(() => Missing.Unless(NurbsSurface.CreateRailRevolvedSurface(profile, rail, axis, scaleHeight), nameof(NurbsSurface.CreateRailRevolvedSurface)));

    public static IO<RevSurface> Revolve(RevolveProfile profile, Line axis, Option<(double StartRadians, double EndRadians)> sweep) =>
        IO.lift(() => Missing.Unless(sweep.Match(
                Some: angles => profile.Switch(
                    (Axis: axis, Angles: angles),
                    ofCurve: static (revolution, curve) => RevSurface.Create(curve.Curve, revolution.Axis, revolution.Angles.StartRadians, revolution.Angles.EndRadians),
                    ofLine: static (revolution, line) => RevSurface.Create(line.Line, revolution.Axis, revolution.Angles.StartRadians, revolution.Angles.EndRadians),
                    ofPolyline: static (revolution, polyline) => RevSurface.Create(polyline.Polyline, revolution.Axis, revolution.Angles.StartRadians, revolution.Angles.EndRadians)),
                None: () => profile.Switch(
                    axis,
                    ofCurve: static (about, curve) => RevSurface.Create(curve.Curve, about),
                    ofLine: static (about, line) => RevSurface.Create(line.Line, about),
                    ofPolyline: static (about, polyline) => RevSurface.Create(polyline.Polyline, about))), nameof(RevSurface.Create)));

    public static IO<SumSurface> Sum(Curve curve, SumExtent extent) =>
        IO.lift(() => Missing.Unless(extent.Switch(
                curve,
                byDirection: static (profile, direction) => SumSurface.Create(profile, direction.Direction),
                byCurve: static (profile, other) => SumSurface.Create(profile, other.Curve)), nameof(SumSurface.Create)));

    public static IO<Surface> Extruded(Curve profile, ExtrudeTerminal terminal) =>
        IO.lift(() => terminal.Switch(
            profile,
            along: static (curve, along) => Missing.Unless(Surface.CreateExtrusion(curve, along.Direction), nameof(Surface.CreateExtrusion)),
            toApex: static (curve, apex) => Missing.Unless(Surface.CreateExtrusionToPoint(curve, apex.Apex), nameof(Surface.CreateExtrusionToPoint))));

    public static IO<NurbsCurve> GeodesicCurve(Surface surface, Seq<Point2d> points, double tolerance, bool periodic) =>
        from filled in IO.lift(() => Limits.AtLeast(MinimumPoints).Check(points.Count, nameof(points)))
        from curve in IO.lift(() => Missing.Unless(NurbsSurface.CreateCurveOnSurface(surface, points, tolerance, periodic), nameof(NurbsSurface.CreateCurveOnSurface)))
        select curve;

    private static Fin<Unit> Validated(Fin<Unit> present, Seq<(string Member, int Continuity)> continuities) =>
        from filled in present
        from inRange in continuities.TraverseM(static row => Continuity.Bind(limits => limits.Check(row.Continuity, row.Member))).As()
        select unit;

    private static IO<NurbsSurface> Networked(Func<(NurbsSurface? Surface, int Error)> create) =>
        from answer in IO.lift(create)
        from surface in GeometryOps.OnFailure(
            IO.lift(() => answer.Error switch {
                1 => Fin.Fail<NurbsSurface>(new NetworkSurfaceFailed.Sorting()),
                2 => Fin.Fail<NurbsSurface>(new NetworkSurfaceFailed.Initialization()),
                3 => Fin.Fail<NurbsSurface>(new NetworkSurfaceFailed.Build()),
                4 => Fin.Fail<NurbsSurface>(new NetworkSurfaceFailed.Validity()),
                _ => Missing.Unless(answer.Surface, nameof(NurbsSurface.CreateNetworkSurface)),
            }),
            Disposal.Release(Optional(answer.Surface).ToSeq()))
        select surface;

    // --- [FROM_POINTS]
    public static IO<NurbsSurface> Grid(Seq<Point3d> points, int uCount, int vCount, int uDegree, int vDegree, GridFit fit) =>
        from counted in IO.lift(() => CountMismatch.Unless(uCount * vCount, points.Count, nameof(NurbsSurface.CreateFromPoints)))
        from surface in IO.lift(() => fit.Switch(
            (Points: points, UCount: uCount, VCount: vCount, UDegree: uDegree, VDegree: vDegree),
            control: static (grid, _) => Missing.Unless(NurbsSurface.CreateFromPoints(grid.Points, grid.UCount, grid.VCount, grid.UDegree, grid.VDegree), nameof(NurbsSurface.CreateFromPoints)),
            through: static (grid, through) => Missing.Unless(
                NurbsSurface.CreateThroughPoints(grid.Points, grid.UCount, grid.VCount, grid.UDegree, grid.VDegree, through.UClosed, through.VClosed),
                nameof(NurbsSurface.CreateThroughPoints))))
        select surface;

    public static IO<NurbsSurface> Corners(CornerSource source) =>
        IO.lift(() => Missing.Unless(source.Switch(
                triangle: static triangle => NurbsSurface.CreateFromCorners(triangle.A, triangle.B, triangle.C),
                quad: static quad => quad.Tolerance.Match(
                    Some: tolerance => NurbsSurface.CreateFromCorners(quad.A, quad.B, quad.C, quad.D, tolerance),
                    None: () => NurbsSurface.CreateFromCorners(quad.A, quad.B, quad.C, quad.D))), nameof(NurbsSurface.CreateFromCorners)));

    public static IO<NurbsSurface> PlaneGrid(Plane plane, Interval u, Interval v, int uDegree, int vDegree, int uPoints, int vPoints) =>
        from valid in IO.lift(() => Invalid.Unless(plane.IsValid, nameof(Plane.IsValid)))
        from surface in IO.lift(() => Missing.Unless(NurbsSurface.CreateFromPlane(plane, u, v, uDegree, vDegree, uPoints, vPoints), nameof(NurbsSurface.CreateFromPlane)))
        select surface;

    public static IO<PlaneSurface> BoundedPlane(PlaneFrame frame, BoundingBox box) =>
        from valid in IO.lift(() => Invalid.Unless(box.IsValid, nameof(BoundingBox.IsValid)))
        from surface in IO.lift(() => Missing.Unless(frame.Switch(
                box,
                ofPlane: static (bounds, frame) => PlaneSurface.CreateThroughBox(frame.Plane, bounds),
                ofLine: static (bounds, frame) => PlaneSurface.CreateThroughBox(frame.Line, frame.VectorInPlane, bounds)), nameof(PlaneSurface.CreateThroughBox)))
        select surface;

    public static IO<NurbsSurface> Analytic(AnalyticSurface source) =>
        IO.lift(() => source.Switch(
            ofCone: static cone => Missing.Unless(NurbsSurface.CreateFromCone(cone.Cone), nameof(NurbsSurface.CreateFromCone)),
            ofCylinder: static cylinder => Missing.Unless(NurbsSurface.CreateFromCylinder(cylinder.Cylinder), nameof(NurbsSurface.CreateFromCylinder)),
            ofSphere: static sphere => Missing.Unless(NurbsSurface.CreateFromSphere(sphere.Sphere), nameof(NurbsSurface.CreateFromSphere)),
            ofTorus: static torus => Missing.Unless(NurbsSurface.CreateFromTorus(torus.Torus), nameof(NurbsSurface.CreateFromTorus))));

    public static IO<RevSurface> AnalyticRevolved(AnalyticSurface source) =>
        IO.lift(() => source.Switch(
            ofCone: static cone => Missing.Unless(RevSurface.CreateFromCone(cone.Cone), nameof(RevSurface.CreateFromCone)),
            ofCylinder: static cylinder => Missing.Unless(RevSurface.CreateFromCylinder(cylinder.Cylinder), nameof(RevSurface.CreateFromCylinder)),
            ofSphere: static sphere => Missing.Unless(RevSurface.CreateFromSphere(sphere.Sphere), nameof(RevSurface.CreateFromSphere)),
            ofTorus: static torus => Missing.Unless(RevSurface.CreateFromTorus(torus.Torus), nameof(RevSurface.CreateFromTorus))));

    // --- [REFIT]
    public static IO<Surface> Fit(Surface surface, SurfaceFitMethod method) =>
        IO.lift(() => method.Switch(
            surface,
            toTolerance: static (source, fit) => Missing.Unless(source.Fit(fit.UDegree, fit.VDegree, fit.Tolerance), nameof(Surface.Fit)),
            toGrid: static (source, fit) => Missing.Unless<Surface>(source.Rebuild(fit.UDegree, fit.VDegree, fit.UPoints, fit.VPoints), nameof(Surface.Rebuild)),
            inDirection: static (source, fit) =>
                from directed in Directed(fit.Direction, nameof(Surface.RebuildOneDirection))
                from rebuilt in Missing.Unless<Surface>(source.RebuildOneDirection(fit.Direction, fit.PointCount, fit.Kind, fit.RefitTolerance), nameof(Surface.RebuildOneDirection))
                select rebuilt));

    public static IO<NurbsSurface> SubDFriendly(Surface surface) =>
        IO.lift(() => Missing.Unless(NurbsSurface.CreateSubDFriendly(surface), nameof(NurbsSurface.CreateSubDFriendly)));

    public static IO<Surface> Periodic(Surface surface, int direction, bool smooth) =>
        from directed in IO.lift(() => Directed(direction, nameof(Surface.CreatePeriodicSurface)))
        from periodic in IO.lift(() => Missing.Unless(Surface.CreatePeriodicSurface(surface, direction, smooth), nameof(Surface.CreatePeriodicSurface)))
        select periodic;

    public static IO<Surface> SoftEdit(Surface surface, SoftEditOptions options) =>
        IO.lift(() => Missing.Unless(Surface.CreateSoftEditSurface(surface, options.Uv, options.Delta, options.ULength, options.VLength, options.Tolerance, options.FixEnds), nameof(Surface.CreateSoftEditSurface)));

    public static IO<(NurbsSurface A, NurbsSurface B)> Compatible(Surface a, Surface b) =>
        IO.lift(() => Refused.Unless(NurbsSurface.MakeCompatible(a, b, out NurbsSurface nurbsA, out NurbsSurface nurbsB), (A: nurbsA, B: nurbsB), nameof(NurbsSurface.MakeCompatible)));

    public static IO<NurbsSurface> MatchToCurve(NurbsSurface surface, Curve target, MatchToCurveOptions options) =>
        from sided in IO.lift(() => Invalid.Unless(options.Side != IsoStatus.None, nameof(MatchToCurveOptions.Side)))
        from matched in IO.lift(() => Missing.Unless(surface.MatchToCurve(options.Side, target, options.MaxEndDistance, options.MaxInteriorDistance, options.MatchTolerance, options.MaxLevel), nameof(NurbsSurface.MatchToCurve)))
        select matched;

    public static IO<Surface> VariableOffset(Surface surface, VariableOffsetOptions options, double tolerance) =>
        IO.lift(() => Missing.Unless(options.Interior.IsEmpty
                ? surface.VariableOffset(options.UMinVMin, options.UMinVMax, options.UMaxVMin, options.UMaxVMax, tolerance)
                : surface.VariableOffset(options.UMinVMin, options.UMinVMax, options.UMaxVMin, options.UMaxVMax, options.Interior.Map(static row => row.Uv), options.Interior.Map(static row => row.Distance), tolerance), nameof(Surface.VariableOffset)));

    // --- [BETWEEN]
    public static IO<Seq<Surface>> RollingBall(Surface a, Surface b, double radius, RollingBallMethod method, double tolerance) =>
        from positive in IO.lift(() => Limits.Above(0.0).Check(radius, nameof(radius)))
        from fillets in GeometryResults.Acquire(() => method.Switch(
                (A: a, B: b, Radius: positive, Tolerance: tolerance),
                auto: static (pair, _) => Surface.CreateRollingBallFillet(pair.A, pair.B, pair.Radius, pair.Tolerance),
                flipped: static (pair, flipped) => Surface.CreateRollingBallFillet(pair.A, flipped.FlipA, pair.B, flipped.FlipB, pair.Radius, pair.Tolerance),
                atUv: static (pair, at) => Surface.CreateRollingBallFillet(pair.A, at.UvA, pair.B, at.UvB, pair.Radius, pair.Tolerance)), nameof(Surface.CreateRollingBallFillet))
        select fillets;

    public static IO<Seq<Surface>> Tween(Surface a, Surface b, int count, int samples, double tolerance) =>
        from valid in IO.lift(() =>
            from counted in Limits.AtLeast(1).Check(count, nameof(count))
            from sampled in Limits.AtLeast(MinimumPoints).Check(samples, nameof(samples))
            select (ItemCount: counted, Samples: sampled))
        from tweens in GeometryResults.Acquire(() => Surface.CreateTweenSurfacesWithSampling(a, b, valid.ItemCount, valid.Samples, tolerance), nameof(Surface.CreateTweenSurfacesWithSampling))
        select tweens;
}

using Rasm.Rhino.Document;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SmoothOptions(double Factor, bool X, bool Y, bool Z, bool FixBoundaries, SmoothingCoordinateSystem System, Plane Plane);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record CurveEdit {
    public sealed record RemoveShort(double Tolerance) : CurveEdit;

    public sealed record MakeClosed(double Tolerance) : CurveEdit;

    public sealed record TrimInterval(Interval Domain) : CurveEdit;
}

public sealed record NurbsFitResult(NurbsCurve Curve, Line MaximumSeparation, double SourceParameter, double FitParameter);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ExtendMethod {
    public sealed record ByLength(CurveEnd Side, double Length, CurveExtensionStyle Style) : ExtendMethod;

    public sealed record ToGeometry(CurveEnd Side, CurveExtensionStyle Style, Seq<GeometryBase> Bounds) : ExtendMethod;

    public sealed record ToPoint(CurveEnd Side, CurveExtensionStyle Style, Point3d End) : ExtendMethod;

    public sealed record ByDomain(Interval Domain) : ExtendMethod;

    public sealed record ByLine(CurveEnd Side, Seq<GeometryBase> Bounds) : ExtendMethod;

    public sealed record ByArc(CurveEnd Side, Seq<GeometryBase> Bounds) : ExtendMethod;

    public sealed record OnSurface(CurveEnd Side, Surface Surface) : ExtendMethod;

    public sealed record OnFace(CurveEnd Side, BrepFace Face) : ExtendMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ShortenMethod {
    public sealed record ToDomain(Interval Domain) : ShortenMethod;

    public sealed record AtEnd(CurveEnd Side, double Length) : ShortenMethod;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SplitCutter {
    public sealed record AtParameters(Seq<double> Parameters) : SplitCutter;

    public sealed record ByBrep(Brep Cutter) : SplitCutter;

    public sealed record BySurface(Surface Cutter) : SplitCutter;

    public sealed record ByPlane(Plane Cutter) : SplitCutter;
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveEdits {
    // --- [REFINE]
    public static IO<Curve> Fair(Curve curve, double distanceTolerance, double angleTolerance, int clampStart, int clampEnd, int iterations) =>
        GeometryResults.Acquire(() => curve.Fair(distanceTolerance, angleTolerance, clampStart, clampEnd, iterations), nameof(Curve.Fair));

    public static IO<Curve> Fit(Curve curve, int degree, double fitTolerance, double angleTolerance) =>
        GeometryResults.Acquire(() => curve.Fit(degree, fitTolerance, angleTolerance), nameof(Curve.Fit));

    public static IO<NurbsCurve> Rebuild(Curve curve, int pointCount, int degree, bool preserveTangents) =>
        GeometryResults.Acquire(() => curve.Rebuild(pointCount, degree, preserveTangents), nameof(Curve.Rebuild));

    public static IO<Curve> Smooth(Curve curve, SmoothOptions options) =>
        GeometryResults.Acquire(
            () => Invalid.Unless(options.System != SmoothingCoordinateSystem.CPlane || options.Plane.IsValid, nameof(options))
                .Map(_ => curve.Smooth(options.Factor, options.X, options.Y, options.Z, options.FixBoundaries, options.System, options.Plane)),
            nameof(Curve.Smooth));

    public static IO<Curve> Simplify(Curve curve, CurveSimplifyOptions options, Option<CurveEnd> end, double distanceTolerance, double angleToleranceRadians) =>
        end.Case is CurveEnd side
            ? GeometryResults.Acquire(() => curve.SimplifyEnd(side, options, distanceTolerance, angleToleranceRadians), nameof(Curve.SimplifyEnd))
            : GeometryResults.Acquire(() => curve.Simplify(options, distanceTolerance, angleToleranceRadians), nameof(Curve.Simplify));

    public static IO<Curve> SoftEdit(Curve curve, double t, Vector3d delta, double length, bool fixEnds) =>
        GeometryResults.Acquire(() => Curve.CreateSoftEditCurve(curve, t, delta, length, fixEnds), nameof(Curve.CreateSoftEditCurve));

    public static IO<Curve> Periodic(Curve curve, bool smooth) =>
        GeometryResults.Acquire(() => Curve.CreatePeriodicCurve(curve, smooth), nameof(Curve.CreatePeriodicCurve));

    public static IO<NurbsCurve> SubDFriendly(Curve curve, Option<(int PointCount, bool PeriodicClosed)> size) =>
        GeometryResults.Acquire(
            () => size.Case is (int pointCount, bool periodicClosed) ? NurbsCurve.CreateSubDFriendly(curve, pointCount, periodicClosed) : NurbsCurve.CreateSubDFriendly(curve),
            nameof(NurbsCurve.CreateSubDFriendly));

    public static IO<Seq<NurbsCurve>> Compatible(Seq<Curve> curves, Option<Point3d> start, Option<Point3d> end, int simplifyMethod, int pointCount, double refitTolerance, double angleTolerance) =>
        GeometryResults.Acquire(
            () => NurbsCurve.MakeCompatible(curves, start.IfNone(Point3d.Unset), end.IfNone(Point3d.Unset), simplifyMethod, pointCount, refitTolerance, angleTolerance),
            nameof(NurbsCurve.MakeCompatible),
            emptyFails: false);

    public static IO<(Curve A, Curve B)> MakeEndsMeet(Curve a, bool adjustStartA, Curve b, bool adjustStartB) =>
        from copyA in GeometryOps.Duplicated(a)
        from copyB in DisposalOps.OnFailure(
            GeometryResults.EditCopy(b, copy => Refused.Unless(Curve.MakeEndsMeet(copyA, adjustStartA, copy, adjustStartB), nameof(Curve.MakeEndsMeet))),
            IO.lift(copyA.Dispose))
        select (copyA, copyB.Copy);

    public static IO<Curve> Edit(Curve source, CurveEdit edit) =>
        GeometryResults.EditCopy(source, Applied(edit)).Map(static edited => edited.Copy);

    public static IO<NurbsFitResult> NurbsFit(Curve curve, Interval domain, NurbsCurveFitParameters parameters) =>
        from answer in IO.lift(() => (
            Fit: Curve.CreateNurbsCurveFit(curve, domain, parameters, out Line separation, out double sourceParameter, out double fitParameter),
            Separation: separation,
            SourceParameter: sourceParameter,
            FitParameter: fitParameter))
        from fit in GeometryResults.Acquire(() => answer.Fit, nameof(Curve.CreateNurbsCurveFit))
        select new NurbsFitResult(fit, answer.Separation, answer.SourceParameter, answer.FitParameter);

    private static Func<Curve, Fin<Unit>> Applied(CurveEdit edit) =>
        edit.Switch<Func<Curve, Fin<Unit>>>(
            removeShort: static of => copy => Refused.Unless(copy.RemoveShortSegments(of.Tolerance), nameof(Curve.RemoveShortSegments)),
            makeClosed: static of => copy => Refused.Unless(copy.MakeClosed(of.Tolerance), nameof(Curve.MakeClosed)),
            trimInterval: static of => copy => Refused.Unless(copy.TrimInterval(of.Domain), nameof(Curve.TrimInterval)));

    // --- [EXTENT]
    public static IO<Curve> Extend(Curve curve, ExtendMethod method) =>
        method.Switch(
            curve,
            byLength: static (source, length) => GeometryResults.Acquire(() => source.Extend(length.Side, length.Length, length.Style), nameof(Curve.Extend)),
            toGeometry: static (source, geometry) => GeometryResults.Acquire(() => source.Extend(geometry.Side, geometry.Style, geometry.Bounds), nameof(Curve.Extend)),
            toPoint: static (source, point) => GeometryResults.Acquire(() => source.Extend(point.Side, point.Style, point.End), nameof(Curve.Extend)),
            byDomain: static (source, domain) => GeometryResults.Acquire(() => source.Extend(domain.Domain), nameof(Curve.Extend)),
            byLine: static (source, line) => GeometryResults.Acquire(() => source.ExtendByLine(line.Side, line.Bounds), nameof(Curve.ExtendByLine)),
            byArc: static (source, arc) => GeometryResults.Acquire(() => source.ExtendByArc(arc.Side, arc.Bounds), nameof(Curve.ExtendByArc)),
            onSurface: static (source, surface) => GeometryResults.Acquire(() => source.ExtendOnSurface(surface.Side, surface.Surface), nameof(Curve.ExtendOnSurface)),
            onFace: static (source, face) => GeometryResults.Acquire(() => source.ExtendOnSurface(face.Side, face.Face), nameof(Curve.ExtendOnSurface)));

    public static IO<Curve> Shorten(Curve curve, ShortenMethod method) =>
        method.Switch(
            curve,
            toDomain: static (source, domain) => GeometryResults.Acquire(() => source.Trim(domain.Domain), nameof(Curve.Trim)),
            atEnd: static (source, end) => GeometryResults.Acquire(() => source.Trim(end.Side, end.Length), nameof(Curve.Trim)));

    public static IO<Seq<Curve>> Split(Curve curve, SplitCutter cutter, double tolerance, double angleTolerance) =>
        cutter.Switch(
            (Curve: curve, Tolerance: tolerance, AngleTolerance: angleTolerance),
            atParameters: static (state, at) => GeometryResults.Acquire(
                () => at.Parameters.Traverse(parameter => OutOfDomain.Unless(state.Curve.Domain, parameter, nameof(Curve.Split))).As().Map(_ => state.Curve.Split(at.Parameters)),
                nameof(Curve.Split),
                emptyFails: false),
            byBrep: static (state, brep) => GeometryResults.Acquire(() => state.Curve.Split(brep.Cutter, state.Tolerance, state.AngleTolerance), nameof(Curve.Split), emptyFails: false),
            bySurface: static (state, surface) => GeometryResults.Acquire(() => state.Curve.Split(surface.Cutter, state.Tolerance, state.AngleTolerance), nameof(Curve.Split), emptyFails: false),
            byPlane: static (state, plane) => GeometryResults.Acquire(() => state.Curve.Split(plane.Cutter, state.Tolerance, state.AngleTolerance), nameof(Curve.Split), emptyFails: false));
}

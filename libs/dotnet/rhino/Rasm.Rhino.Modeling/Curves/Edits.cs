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
        IO.lift(() => Limits.AtLeast(1).Check(iterations, nameof(iterations))
            .Bind(valid => Missing.Unless(curve.Fair(distanceTolerance, angleTolerance, clampStart, clampEnd, valid), nameof(Curve.Fair))));

    public static IO<Curve> Fit(Curve curve, int degree, double fitTolerance, double angleTolerance) =>
        IO.lift(() => Limits.AtLeast(1).Check(degree, nameof(degree))
            .Bind(valid => Missing.Unless(curve.Fit(valid, fitTolerance, angleTolerance), nameof(Curve.Fit))));

    public static IO<NurbsCurve> Rebuild(Curve curve, int pointCount, int degree, bool preserveTangents) =>
        IO.lift(() => Limits.AtLeast(degree + 1).Check(pointCount, nameof(pointCount))
            .Bind(valid => Missing.Unless(curve.Rebuild(valid, degree, preserveTangents), nameof(Curve.Rebuild))));

    public static IO<Curve> Smooth(Curve curve, SmoothOptions options) =>
        IO.lift(() => Invalid.Unless(options.Plane.IsValid, nameof(Plane.IsValid))
            .Bind(_ => Missing.Unless(curve.Smooth(options.Factor, options.X, options.Y, options.Z, options.FixBoundaries, options.System, options.Plane), nameof(Curve.Smooth))));

    public static IO<Curve> Simplify(Curve curve, CurveSimplifyOptions options, Option<CurveEnd> end, double distanceTolerance, double angleToleranceRadians) =>
        IO.lift(() => end.Case is CurveEnd side
            ? Missing.Unless(curve.SimplifyEnd(side, options, distanceTolerance, angleToleranceRadians), nameof(Curve.SimplifyEnd))
            : Missing.Unless(curve.Simplify(options, distanceTolerance, angleToleranceRadians), nameof(Curve.Simplify)));

    public static IO<Curve> Edit(Curve source, CurveEdit edit) =>
        GeometryOps.EditCopy(source, Applied(edit)).Map(static edited => edited.Copy);

    private static Func<Curve, Fin<Unit>> Applied(CurveEdit edit) =>
        edit.Switch<Func<Curve, Fin<Unit>>>(
            removeShort: static of => copy => Refused.Unless(copy.RemoveShortSegments(of.Tolerance), nameof(Curve.RemoveShortSegments)),
            makeClosed: static of => copy => Refused.Unless(copy.MakeClosed(of.Tolerance), nameof(Curve.MakeClosed)),
            trimInterval: static of => copy => Refused.Unless(copy.TrimInterval(of.Domain), nameof(Curve.TrimInterval)));

    public static IO<NurbsFitResult> NurbsFit(Curve curve, Interval domain, NurbsCurveFitParameters parameters) =>
        IO.lift(() => Missing.Unless(Curve.CreateNurbsCurveFit(curve, domain, parameters, out Line separation, out double sourceParameter, out double fitParameter), nameof(Curve.CreateNurbsCurveFit))
            .Map(fit => new NurbsFitResult(fit, separation, sourceParameter, fitParameter)));

    // --- [EXTENT]
    public static IO<Curve> Extend(Curve curve, ExtendMethod method) =>
        IO.lift(() => method.Switch(
            curve,
            byLength: static (source, length) => Missing.Unless(source.Extend(length.Side, length.Length, length.Style), nameof(Curve.Extend)),
            toGeometry: static (source, geometry) =>
                from bounded in Invalid.Unless(!geometry.Bounds.IsEmpty, nameof(ExtendMethod.ToGeometry.Bounds))
                from extended in Missing.Unless(source.Extend(geometry.Side, geometry.Style, geometry.Bounds), nameof(Curve.Extend))
                select extended,
            toPoint: static (source, point) => Missing.Unless(source.Extend(point.Side, point.Style, point.End), nameof(Curve.Extend)),
            byDomain: static (source, domain) => Missing.Unless(source.Extend(domain.Domain), nameof(Curve.Extend)),
            byLine: static (source, line) =>
                from bounded in Invalid.Unless(!line.Bounds.IsEmpty, nameof(ExtendMethod.ByLine.Bounds))
                from extended in Missing.Unless(source.ExtendByLine(line.Side, line.Bounds), nameof(Curve.ExtendByLine))
                select extended,
            byArc: static (source, arc) =>
                from bounded in Invalid.Unless(!arc.Bounds.IsEmpty, nameof(ExtendMethod.ByArc.Bounds))
                from extended in Missing.Unless(source.ExtendByArc(arc.Side, arc.Bounds), nameof(Curve.ExtendByArc))
                select extended,
            onSurface: static (source, surface) => Missing.Unless(source.ExtendOnSurface(surface.Side, surface.Surface), nameof(Curve.ExtendOnSurface)),
            onFace: static (source, face) => Missing.Unless(source.ExtendOnSurface(face.Side, face.Face), nameof(Curve.ExtendOnSurface))));

    public static IO<Curve> Shorten(Curve curve, ShortenMethod method) =>
        IO.lift(() => method.Switch(
            curve,
            toDomain: static (source, domain) => Missing.Unless(source.Trim(domain.Domain), nameof(Curve.Trim)),
            atEnd: static (source, end) => Missing.Unless(source.Trim(end.Side, end.Length), nameof(Curve.Trim))));

    public static IO<Seq<Curve>> Split(Curve curve, SplitCutter cutter, double tolerance, double angleTolerance) =>
        cutter.Switch(
            (Curve: curve, Tolerance: tolerance, AngleTolerance: angleTolerance),
            atParameters: static (state, at) =>
                from inside in IO.lift(() =>
                    from filled in Invalid.Unless(!at.Parameters.IsEmpty, nameof(SplitCutter.AtParameters.Parameters))
                    from listed in at.Parameters.TraverseM(parameter => OutOfDomain.Unless(state.Curve.Domain, parameter, nameof(Curve.Split))).As()
                    select unit)
                from pieces in GeometryResults.Acquire(() => state.Curve.Split(at.Parameters), nameof(Curve.Split))
                select pieces,
            byBrep: static (state, brep) => GeometryResults.Acquire(() => state.Curve.Split(brep.Cutter, state.Tolerance, state.AngleTolerance), nameof(Curve.Split)),
            bySurface: static (state, surface) => GeometryResults.Acquire(() => state.Curve.Split(surface.Cutter, state.Tolerance, state.AngleTolerance), nameof(Curve.Split)),
            byPlane: static (state, plane) => GeometryResults.Acquire(() => state.Curve.Split(plane.Cutter, state.Tolerance, state.AngleTolerance), nameof(Curve.Split)));
}

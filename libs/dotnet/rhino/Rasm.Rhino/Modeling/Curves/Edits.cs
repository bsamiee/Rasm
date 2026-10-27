using System.Globalization;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Surfaces;
using Rhino;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NurbsCurveFit(NurbsCurve Curve, Option<Line> MaximumSeparation, Option<(double Source, double Fit)> SeparationParameters);

[Union<Seq<double>, Brep, Surface, Plane>(T1Name = "Parameters", T2Name = "Brep", T3Name = "Surface", T4Name = "Plane", SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class CurveCut;

[Union<Line, Arc, Curve>(T1Name = "Line", T2Name = "Arc", T3Name = "Curve", SkipToString = true, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class PolyCurveSegment;

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveExtension {
    public sealed record Domain(Interval Interval) : CurveExtension;

    public sealed record Length(CurveEnd Side, double Distance, CurveExtensionStyle Style) : CurveExtension;

    public sealed record ToGeometry(CurveEnd Side, CurveExtensionStyle Style, Seq<GeometryBase> Geometry) : CurveExtension;

    public sealed record ToPoint(CurveEnd Side, CurveExtensionStyle Style, Point3d Point) : CurveExtension;

    public sealed record OnSurface(CurveEnd Side, SurfaceTarget Target) : CurveExtension;

    internal IO<EditStep<Curve, int>> Step(Curve curve) =>
        Switch(
            curve,
            domain: static (at, of) => Edits.Replaced<Curve, int>(at.Extend(of.Interval)),
            length: static (at, of) => Edits.Replaced<Curve, int>(at.Extend(of.Side, of.Distance, of.Style)),
            toGeometry: static (at, of) => Edits.Replaced<Curve, int>(at.Extend(of.Side, of.Style, of.Geometry)),
            toPoint: static (at, of) => Edits.Replaced<Curve, int>(at.Extend(of.Side, of.Style, of.Point)),
            onSurface: static (at, of) => of.Target.Switch(
                (Curve: at, of.Side),
                brepFace: static (held, face) => Edits.Replaced<Curve, int>(held.Curve.ExtendOnSurface(held.Side, face)),
                surface: static (held, surface) => Edits.Replaced<Curve, int>(held.Curve.ExtendOnSurface(held.Side, surface))));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveTrim {
    public sealed record Domain(Interval Interval) : CurveTrim;

    public sealed record Length(CurveEnd Side, double Distance) : CurveTrim;

    internal IO<EditStep<Curve, int>> Step(Curve curve) =>
        Switch(
            curve,
            domain: static (at, of) => Edits.Accepted<Curve, int>(at, at.TrimInterval(of.Interval)),
            length: static (at, of) => Edits.Replaced<Curve, int>(at.Trim(of.Side, of.Distance)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ToPolylineLimits {
    public sealed record Tolerance(double Minimum, double Maximum) : ToPolylineLimits;

    public sealed record Divided(
        int MainSegmentCount, int SubSegmentCount, double MaxAngleRadians, double MaxChordLengthRatio, double MaxAspectRatio, double MinEdgeLength, double MaxEdgeLength, bool KeepStartPoint)
        : ToPolylineLimits;

    internal PolylineCurve? ToPolyline(Tolerances tolerances, Curve curve) =>
        Switch(
            (Curve: curve, Tolerances: tolerances),
            tolerance: static (at, of) => at.Curve.ToPolyline(at.Tolerances.Absolute, at.Tolerances.Angle, of.Minimum, of.Maximum),
            divided: static (at, of) => at.Curve.ToPolyline(
                of.MainSegmentCount, of.SubSegmentCount, of.MaxAngleRadians, of.MaxChordLengthRatio, of.MaxAspectRatio, at.Tolerances.Absolute, of.MinEdgeLength, of.MaxEdgeLength, of.KeepStartPoint));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveEdit {
    public sealed record Repair : CurveEdit;
    public sealed record ChangeClosedCurveSeam(double T) : CurveEdit;
    public sealed record MakeClosed : CurveEdit;
    public sealed record CombineShortSegments : CurveEdit;
    public sealed record Reverse : CurveEdit;
    public sealed record SetStartPoint(Point3d Point) : CurveEdit;
    public sealed record SetEndPoint(Point3d Point) : CurveEdit;
    public sealed record RemoveShortSegments : CurveEdit;
    public sealed record ChangeDimension(int Dimension) : CurveEdit;
    public sealed record Extend(CurveExtension Extension) : CurveEdit;
    public sealed record Trim(CurveTrim Extent) : CurveEdit;
    public sealed record Simplify(CurveSimplifyOptions Options) : CurveEdit;
    public sealed record SimplifyEnd(CurveEnd End, CurveSimplifyOptions Options) : CurveEdit;
    public sealed record Fair(PreserveEnd Start, PreserveEnd End, int Iterations) : CurveEdit;
    public sealed record Rebuild(int PointCount, int Degree, bool PreserveTangents) : CurveEdit;
    public sealed record Fit(int Degree) : CurveEdit;
    public sealed record SubDFriendly(Option<(int PointCount, bool Periodic)> Points) : CurveEdit;
    public sealed record Smooth(SmoothOptions Options) : CurveEdit;
    public sealed record Reparameterize : CurveEdit;
    public sealed record MakePeriodic(bool SmoothSeam) : CurveEdit;
    public sealed record ToPolyline(ToPolylineLimits Limits) : CurveEdit;
    public sealed record ToArcsAndLines(double Minimum, double Maximum) : CurveEdit;
    public sealed record ToNurbs(Option<Interval> Subdomain) : CurveEdit;
    public sealed record FilletCorners(double Radius) : CurveEdit;
    public sealed record ChamferCorners(double Distance0, double Distance1) : CurveEdit;

    public IO<EditStep<Curve, int>> Step(Tolerances tolerances, Curve curve) =>
        Switch(
            (Curve: curve, Tolerances: tolerances),
            repair: static (at, _) => Edits.Accepted<Curve, int>(at.Curve, at.Curve.Repair(at.Tolerances.Absolute)),
            changeClosedCurveSeam: static (at, of) => Edits.Accepted<Curve, int>(at.Curve, at.Curve.ChangeClosedCurveSeam(of.T)),
            makeClosed: static (at, _) => Edits.Accepted<Curve, int>(at.Curve, at.Curve.MakeClosed(at.Tolerances.Absolute)),
            combineShortSegments: static (at, _) => Edits.Changed<Curve, int>(at.Curve, at.Curve.CombineShortSegments(at.Tolerances.Absolute)),
            reverse: static (at, _) => Edits.Accepted<Curve, int>(at.Curve, at.Curve.Reverse()),
            setStartPoint: static (at, of) => Edits.Accepted<Curve, int>(at.Curve, at.Curve.SetStartPoint(of.Point)),
            setEndPoint: static (at, of) => Edits.Accepted<Curve, int>(at.Curve, at.Curve.SetEndPoint(of.Point)),
            removeShortSegments: static (at, _) => Edits.Changed<Curve, int>(at.Curve, at.Curve.RemoveShortSegments(at.Tolerances.Absolute)),
            changeDimension: static (at, of) => Edits.Accepted<Curve, int>(at.Curve, at.Curve.ChangeDimension(of.Dimension)),
            extend: static (at, of) => of.Extension.Step(at.Curve),
            trim: static (at, of) => of.Extent.Step(at.Curve),
            simplify: static (at, of) => Edits.Replaced<Curve, int>(at.Curve.Simplify(of.Options, at.Tolerances.Absolute, at.Tolerances.Angle)),
            simplifyEnd: static (at, of) => Edits.Replaced<Curve, int>(at.Curve.SimplifyEnd(of.End, of.Options, at.Tolerances.Absolute, at.Tolerances.Angle)),
            fair: static (at, of) => Edits.Replaced<Curve, int>(at.Curve.Fair(at.Tolerances.Absolute, at.Tolerances.Angle, (int)of.Start - 1, (int)of.End - 1, of.Iterations)),
            rebuild: static (at, of) => Edits.Replaced<Curve, int>(at.Curve.Rebuild(of.PointCount, of.Degree, of.PreserveTangents)),
            fit: static (at, of) => Edits.Replaced<Curve, int>(at.Curve.Fit(of.Degree, at.Tolerances.Absolute, at.Tolerances.Angle)),
            subDFriendly: static (at, of) => Edits.Replaced<Curve, int>(of.Points.Match(
                Some: points => NurbsCurve.CreateSubDFriendly(at.Curve, points.PointCount, points.Periodic),
                None: () => NurbsCurve.CreateSubDFriendly(at.Curve))),
            smooth: static (at, of) => of.Options.Coordinates switch {
                var (system, plane) => Edits.Replaced<Curve, int>(at.Curve.Smooth(of.Options.Factor, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, system, plane)),
            },
            reparameterize: static (at, _) => Edits.Replaced<Curve, int>(at.Curve.Reparameterize()),
            makePeriodic: static (at, of) => Edits.Replaced<Curve, int>(Curve.CreatePeriodicCurve(at.Curve, of.SmoothSeam)),
            toPolyline: static (at, of) => Edits.Replaced<Curve, int>(of.Limits.ToPolyline(at.Tolerances, at.Curve)),
            toArcsAndLines: static (at, of) => Edits.Replaced<Curve, int>(at.Curve.ToArcsAndLines(at.Tolerances.Absolute, at.Tolerances.Angle, of.Minimum, of.Maximum)),
            toNurbs: static (at, of) => Edits.Replaced<Curve, int>(of.Subdomain.Match(
                Some: subdomain => at.Curve.ToNurbsCurve(subdomain),
                None: () => at.Curve.ToNurbsCurve())),
            filletCorners: static (at, of) => Edits.Replaced<Curve, int>(Curve.CreateFilletCornersCurve(at.Curve, of.Radius, at.Tolerances.Absolute, at.Tolerances.Angle)),
            chamferCorners: static (at, of) => Chamfered(at.Tolerances, at.Curve, of.Distance0, of.Distance1));

    private static IO<EditStep<Curve, int>> Chamfered(Tolerances tolerances, Curve curve, double before, double after) =>
        from corners in IO.lift(() => CurveEvaluation.Points(tolerances, curve, new CurvePointSet.Discontinuities(Continuity.G1_continuous, Math.Cos(tolerances.Angle), RhinoMath.SqrtEpsilon)))
        from cuts in IO.lift(() => Cuts(curve, corners, before, after))
        from step in cuts.IsEmpty
            ? Edits.Changed<Curve, int>(curve, changed: false)
            : from pieces in Products.Made(curve.Split(cuts.Bind(static cut => Seq(cut.Start, cut.End))))
              from lines in Products.Made(cuts.Map(cut => new LineCurve(curve.PointAt(cut.Start), curve.PointAt(cut.End))))
              let kept = pieces.Filter(piece => !cuts.Exists(cut => cut.Start < piece.Domain.Mid && piece.Domain.Mid < cut.End))
              from joined in Products.Made(Curve.JoinCurves(kept + lines.Map(static line => (Curve)line), tolerances.Absolute, preserveDirection: true))
              from single in IO.lift(() => RhinoFault.Unless(joined.Count is 1).Map(_ => joined[0]))
              select (EditStep<Curve, int>)new EditStep<Curve, int>.Replaced(single)
        select step;

    private static Fin<Seq<(double Start, double End)>> Cuts(Curve curve, Seq<CurveSample> corners, double before, double after) =>
        (Total: curve.GetLength(), Lengths: corners.Map(corner => curve.GetLength(new Interval(curve.Domain.T0, corner.Parameter)) switch {
            var at => (Start: at - before, End: at + after),
        }).Strict()) switch {
            var measured => Callbacks.Each(measured.Lengths, (span, index) =>
                span.Start > (index > 0 ? measured.Lengths[index - 1].End : 0d) && span.End < (index < measured.Lengths.Count - 1 ? measured.Lengths[index + 1].Start : measured.Total)
                    ? (RhinoFault.Unless(curve.LengthParameter(span.Start, out double start), index).Map(_ => start),
                        RhinoFault.Unless(curve.LengthParameter(span.End, out double end), index).Map(_ => end))
                        .Apply(static (start, end) => (Start: start, End: end)).As()
                    : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"chamfer ({span.Start}, {span.End})"), RhinoReason.Outside, index)),
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record EndCondition {
    public sealed record Position(Point3d Point) : EndCondition;

    public sealed record Tangency(Point3d Point, Vector3d Tangent) : EndCondition;

    public sealed record Curvature(Point3d Point, Vector3d Tangent, Vector3d CurvatureVector) : EndCondition;

    internal (NurbsCurve.NurbsCurveEndConditionType Type, Point3d Point, Vector3d Tangent, Vector3d Curvature) Parameters =>
        Switch(
            position: static of => (NurbsCurve.NurbsCurveEndConditionType.Position, of.Point, Vector3d.Unset, Vector3d.Unset),
            tangency: static of => (NurbsCurve.NurbsCurveEndConditionType.Tangency, of.Point, of.Tangent, Vector3d.Unset),
            curvature: static of => (NurbsCurve.NurbsCurveEndConditionType.Curvature, of.Point, of.Tangent, of.CurvatureVector));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record NurbsCurveEdit {
    public sealed record IncreaseDegree(int Degree) : NurbsCurveEdit;
    public sealed record MakePiecewiseBezier(bool SetEndWeightsToOne) : NurbsCurveEdit;
    public sealed record MakeUniform : NurbsCurveEdit;
    public sealed record UniformKnots(double Spacing) : NurbsCurveEdit;
    public sealed record PeriodicKnots(double Spacing) : NurbsCurveEdit;
    public sealed record InsertKnot(double At, int Multiplicity) : NurbsCurveEdit;
    public sealed record RemoveKnots(int From, int To) : NurbsCurveEdit;
    public sealed record RemoveKnotAt(double At) : NurbsCurveEdit;
    public sealed record RemoveMultipleKnots(int Minimum, int Maximum) : NurbsCurveEdit;
    public sealed record ClampEnd(CurveEnd End) : NurbsCurveEdit;
    public sealed record SetWeight(int Point, double Weight) : NurbsCurveEdit;
    public sealed record ChangeEndWeights(double Start, double End) : NurbsCurveEdit;
    public sealed record MakeRational : NurbsCurveEdit;
    public sealed record MakeNonRational : NurbsCurveEdit;
    public sealed record RationalReparameterize(double C) : NurbsCurveEdit;
    public sealed record SetPoint(int Point, Point4d Location) : NurbsCurveEdit;
    public sealed record SetGrevillePoints(Seq<Point3d> Points) : NurbsCurveEdit;
    public sealed record SetEndCondition(bool SetEnd, EndCondition Condition) : NurbsCurveEdit;
    public sealed record Append(NurbsCurve Next) : NurbsCurveEdit;
    public sealed record MatchToCurve(Curve Target, double MaxEndDistance, double MaxInteriorDistance, int MaxLevel) : NurbsCurveEdit;

    public IO<EditStep<NurbsCurve, int>> Step(Tolerances tolerances, NurbsCurve nurbs) =>
        Switch(
            (Nurbs: nurbs, Tolerances: tolerances),
            increaseDegree: static (at, of) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.IncreaseDegree(of.Degree)),
            makePiecewiseBezier: static (at, of) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.MakePiecewiseBezier(of.SetEndWeightsToOne)),
            makeUniform: static (at, _) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.MakeUniform()),
            uniformKnots: static (at, of) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.CreateUniformKnots(of.Spacing)),
            periodicKnots: static (at, of) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.CreatePeriodicKnots(of.Spacing)),
            insertKnot: static (at, of) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.InsertKnot(of.At, of.Multiplicity)),
            removeKnots: static (at, of) =>
                IO.lift(() => (Conversions.Within(of.From, at.Nurbs.Knots.Count), Conversions.Within(of.To - 1, at.Nurbs.Knots.Count)).Apply(static (_, _) => unit).As())
                .Bind(_ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.RemoveKnots(of.From, of.To))),
            removeKnotAt: static (at, of) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.RemoveKnotAt(of.At)),
            removeMultipleKnots: static (at, of) => Edits.Count(at.Nurbs, at.Nurbs.Knots.RemoveMultipleKnots(of.Minimum, of.Maximum, at.Tolerances.Absolute)),
            clampEnd: static (at, of) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.ClampEnd(of.End)),
            setWeight: static (at, of) => IO.lift(() => Conversions.Within(of.Point, at.Nurbs.Points.Count))
                .Bind(_ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Points.SetWeight(of.Point, of.Weight))),
            changeEndWeights: static (at, of) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Points.ChangeEndWeights(of.Start, of.End)),
            makeRational: static (at, _) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Points.MakeRational()),
            makeNonRational: static (at, _) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Points.MakeNonRational()),
            rationalReparameterize: static (at, of) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Reparameterize(of.C)),
            setPoint: static (at, of) => IO.lift(() => Conversions.Within(of.Point, at.Nurbs.Points.Count))
                .Bind(_ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Points.SetPoint(of.Point, of.Location))),
            setGrevillePoints: static (at, of) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.SetGrevillePoints(of.Points)),
            setEndCondition: static (at, of) => of.Condition.Parameters switch {
                var (type, point, tangent, curvature) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.SetEndCondition(of.SetEnd, type, point, tangent, curvature)),
            },
            append: static (at, of) => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Append(of.Next)),
            matchToCurve: static (at, of) => Edits.Replaced<NurbsCurve, int>(at.Nurbs.MatchToCurve(of.Target, of.MaxEndDistance, of.MaxInteriorDistance, at.Tolerances.Absolute, of.MaxLevel)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PolyCurveEdit {
    public sealed record RemoveNesting : PolyCurveEdit;
    public sealed record Append(PolyCurveSegment Segment) : PolyCurveEdit;
    public sealed record AppendSegment(Curve Segment) : PolyCurveEdit;

    public IO<EditStep<PolyCurve, int>> Step(PolyCurve polycurve) =>
        Switch(
            polycurve,
            removeNesting: static (at, _) => Edits.Changed<PolyCurve, int>(at, at.RemoveNesting()),
            append: static (at, of) => Edits.Accepted<PolyCurve, int>(at, of.Segment.Switch(
                at,
                line: static (target, line) => target.Append(line),
                arc: static (target, arc) => target.Append(arc),
                curve: static (target, curve) => target.Append(curve))),
            appendSegment: static (at, of) => Edits.Accepted<PolyCurve, int>(at, at.AppendSegment(of.Segment)));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveEdits {
    // --- [EDITS]
    public static IO<(Curve Result, Seq<(CurveEdit Edit, EditResult<int> Result)> Results)> Edit(Tolerances tolerances, Curve source, Seq<CurveEdit> edits) =>
        Edits.Apply(source, edits, (curve, edit) => edit.Step(tolerances, curve), static _ => IO.pure(unit));

    public static IO<(NurbsCurve Result, Seq<(NurbsCurveEdit Edit, EditResult<int> Result)> Results)> Edit(Tolerances tolerances, NurbsCurve source, Seq<NurbsCurveEdit> edits) =>
        Edits.Apply(source, edits, (nurbs, edit) => edit.Step(tolerances, nurbs), static _ => IO.pure(unit));

    public static IO<(PolyCurve Result, Seq<(PolyCurveEdit Edit, EditResult<int> Result)> Results)> Edit(PolyCurve source, Seq<PolyCurveEdit> edits) =>
        Edits.Apply(source, edits, static (polycurve, edit) => edit.Step(polycurve), static _ => IO.pure(unit));

    // --- [FITS]
    public static IO<NurbsCurveFit> CreateNurbsCurveFit(Curve curve, Interval domain, NurbsCurveFitParameters rebuildOptions) =>
        unitIO.Bind(_ => Products.Made(Curve.CreateNurbsCurveFit(curve, domain, rebuildOptions, out Line maximumSeparation, out double source, out double fit))
            .Map(fitted => new NurbsCurveFit(fitted, Conversions.Present(maximumSeparation),
                from sourceAt in Conversions.Present(source)
                from fitAt in Conversions.Present(fit)
                select (Source: sourceAt, Fit: fitAt))));

    public static IO<(NurbsCurve Curve, Option<Line> MaximumDeviation)> RebuildToMatchTemplateCurve(Curve source, Curve templateCurve, bool flipSourceDirection, bool preserveEndTangents, bool makeSubDFriendly) =>
        Products.Made(source.RebuildToMatchTemplateCurve(templateCurve, flipSourceDirection, preserveEndTangents, makeSubDFriendly, out Line maximumDeviation))
            .Map(rebuilt => (Curve: rebuilt, MaximumDeviation: Conversions.Present(maximumDeviation)));

    // --- [SPLIT]
    public static IO<Seq<Curve>> Split(Tolerances tolerances, Curve curve, CurveCut cut) =>
        cut.Switch(
            (Curve: curve, Tolerances: tolerances),
            parameters: static (at, values) => Products.Made(at.Curve.Split(values)),
            brep: static (at, cutter) => Products.Made(at.Curve.Split(cutter, at.Tolerances.Absolute, at.Tolerances.Angle)),
            surface: static (at, cutter) => Products.Made(at.Curve.Split(cutter, at.Tolerances.Absolute, at.Tolerances.Angle)),
            plane: static (at, cutter) => Products.Made(at.Curve.Split(cutter, at.Tolerances.Absolute, at.Tolerances.Angle)));

    // --- [MATCHING]
    public static IO<Seq<NurbsCurve>> MakeCompatible(Tolerances tolerances, Seq<Curve> curves, Option<Point3d> startPt, Option<Point3d> endPt, CurveFit fit) =>
        fit.Parameters switch {
            var fitting => Products.Made(NurbsCurve.MakeCompatible(
                    curves, Conversions.Unset(startPt), Conversions.Unset(endPt), (int)fitting.RebuildType, fitting.RebuildPointCount, fitting.RefitTolerance, tolerances.Angle))
                .Bind(static compatible => IO.lift(() => RhinoFault.Unless(!compatible.IsEmpty).Map(_ => compatible))),
        };

    public static IO<(Curve A, Curve B)> MakeEndsMeet(Curve curveA, bool adjustStartCurveA, Curve curveB, bool adjustStartCurveB) =>
        from copyA in Copies.Duplicate(curveA)
        from copyB in Copies.Duplicate(curveB)
        from met in IO.lift(() => RhinoFault.Unless(Curve.MakeEndsMeet(copyA, adjustStartCurveA, copyB, adjustStartCurveB)))
        from held in Products.Made(Seq(copyA, copyB))
        select (A: held[0], B: held[1]);
}

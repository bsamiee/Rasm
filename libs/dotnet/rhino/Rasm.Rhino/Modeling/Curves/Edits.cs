using System.Globalization;
using Rasm.Rhino.Document.Units;
using Rasm.Rhino.Modeling.Surfaces;
using Rhino;
using Rhino.Geometry.Collections;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record NurbsCurveFit(NurbsCurve Curve, Option<Line> MaximumSeparation, Option<(double Source, double Fit)> SeparationParameters);

[ComplexValueObject(SkipToString = true)]
public sealed partial class DomainParameters {
    public Curve Curve { get; }

    public Seq<double> Values { get; }

    internal static Fin<DomainParameters> Validated(Curve curve, Seq<double> values) =>
        ValidationResults.ToFin(Validate(curve, values, out DomainParameters? parameters), parameters);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Curve curve, ref Seq<double> values) {
        Interval domain = curve.Domain;
        Seq<double> outside = values.Filter(t => !domain.IncludesParameter(t));
        validationError = outside.IsEmpty ? null : new ValidationError($"parameters [{string.Join(", ", outside.Map(static value => value.ToString(CultureInfo.InvariantCulture)))}] are outside curve domain {domain}");
    }
}

[ValueObject<PreserveEnd>(SkipIParsable = true, SkipIComparable = true)]
public readonly partial struct PreservedEnd {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref PreserveEnd value) =>
        validationError = value is PreserveEnd.Position or PreserveEnd.Tangency or PreserveEnd.Curvature
            ? null
            : new ValidationError($"{value} preserves nothing at a faired end");
}

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

    internal IO<EditStep<Curve, int>> Step(Curve curve, Action<IDisposable> retain) =>
        Switch(
            (Curve: curve, Retain: retain),
            domain: static (at, of) => Copies.Acquire(nameof(Curve.Extend), _ => Edits.Replaced<Curve, int>(at.Curve.Extend(of.Interval)), at.Retain),
            length: static (at, of) => Copies.Acquire(nameof(Curve.Extend), _ => Edits.Replaced<Curve, int>(at.Curve.Extend(of.Side, of.Distance, of.Style)), at.Retain),
            toGeometry: static (at, of) => Copies.Acquire(nameof(Curve.Extend), _ => Edits.Replaced<Curve, int>(at.Curve.Extend(of.Side, of.Style, of.Geometry)), at.Retain),
            toPoint: static (at, of) => Copies.Acquire(nameof(Curve.Extend), _ => Edits.Replaced<Curve, int>(at.Curve.Extend(of.Side, of.Style, of.Point)), at.Retain),
            onSurface: static (at, of) => of.Target.Switch(
                (at.Curve, at.Retain, of.Side),
                brepFace: static (held, face) => Copies.Acquire(nameof(Curve.ExtendOnSurface), _ => Edits.Replaced<Curve, int>(held.Curve.ExtendOnSurface(held.Side, face)), held.Retain),
                surface: static (held, surface) => Copies.Acquire(nameof(Curve.ExtendOnSurface), _ => Edits.Replaced<Curve, int>(held.Curve.ExtendOnSurface(held.Side, surface)), held.Retain)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record CurveTrim {
    public sealed record Domain(Interval Interval) : CurveTrim;

    public sealed record Length(CurveEnd Side, PositiveLength Distance) : CurveTrim;

    internal IO<EditStep<Curve, int>> Step(Curve curve, Action<IDisposable> retain) =>
        Switch(
            (Curve: curve, Retain: retain),
            domain: static (at, of) => IO.lift(() => CurveEdits.Spanned(at.Curve, of.Interval, nameof(Curve.TrimInterval)))
                >> Copies.Acquire(nameof(Curve.TrimInterval), _ => Edits.Accepted<Curve, int>(at.Curve, at.Curve.TrimInterval(of.Interval)), at.Retain),
            length: static (at, of) => Copies.Acquire(nameof(Curve.Trim), _ => Edits.Replaced<Curve, int>(at.Curve.Trim(of.Side, of.Distance)), at.Retain));
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
    public sealed record Fair(PreservedEnd Start, PreservedEnd End, int Iterations) : CurveEdit;
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
    public sealed record ChamferCorners(PositiveLength Distance0, PositiveLength Distance1) : CurveEdit;

    public IO<EditStep<Curve, int>> Step(Tolerances tolerances, Curve curve, Action<IDisposable> retain) =>
        Switch(
            (Curve: curve, Tolerances: tolerances, Retain: retain),
            repair: static (at, _) => Copies.Acquire(nameof(Curve.Repair), _ => Edits.Accepted<Curve, int>(at.Curve, at.Curve.Repair(at.Tolerances.Absolute)), at.Retain),
            changeClosedCurveSeam: static (at, of) => Copies.Acquire(nameof(Curve.ChangeClosedCurveSeam), _ => Edits.Accepted<Curve, int>(at.Curve, at.Curve.ChangeClosedCurveSeam(of.T)), at.Retain),
            makeClosed: static (at, _) => Copies.Acquire(nameof(Curve.MakeClosed), _ => Edits.Accepted<Curve, int>(at.Curve, at.Curve.MakeClosed(at.Tolerances.Absolute)), at.Retain),
            combineShortSegments: static (at, _) => Copies.Acquire(nameof(Curve.CombineShortSegments), _ =>
                Edits.Changed<Curve, int>(at.Curve, at.Curve.CombineShortSegments(at.Tolerances.Absolute)), at.Retain),
            reverse: static (at, _) => Copies.Acquire(nameof(Curve.Reverse), _ => Edits.Accepted<Curve, int>(at.Curve, at.Curve.Reverse()), at.Retain),
            setStartPoint: static (at, of) => Copies.Acquire(nameof(Curve.SetStartPoint), _ => Edits.Accepted<Curve, int>(at.Curve, at.Curve.SetStartPoint(of.Point)), at.Retain),
            setEndPoint: static (at, of) => Copies.Acquire(nameof(Curve.SetEndPoint), _ => Edits.Accepted<Curve, int>(at.Curve, at.Curve.SetEndPoint(of.Point)), at.Retain),
            removeShortSegments: static (at, _) => Copies.Acquire(nameof(Curve.RemoveShortSegments), _ =>
                Edits.Changed<Curve, int>(at.Curve, at.Curve.RemoveShortSegments(at.Tolerances.Absolute)), at.Retain),
            changeDimension: static (at, of) => Copies.Acquire(nameof(Curve.ChangeDimension), _ => Edits.Accepted<Curve, int>(at.Curve, at.Curve.ChangeDimension(of.Dimension)), at.Retain),
            extend: static (at, of) => of.Extension.Step(at.Curve, at.Retain),
            trim: static (at, of) => of.Extent.Step(at.Curve, at.Retain),
            simplify: static (at, of) => Copies.Acquire(nameof(Curve.Simplify), _ =>
                Edits.Replaced<Curve, int>(at.Curve.Simplify(of.Options, at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain),
            simplifyEnd: static (at, of) => Copies.Acquire(nameof(Curve.SimplifyEnd), _ =>
                Edits.Replaced<Curve, int>(at.Curve.SimplifyEnd(of.End, of.Options, at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain),
            fair: static (at, of) => Copies.Acquire(nameof(Curve.Fair), _ => Edits.Replaced<Curve, int>(
                at.Curve.Fair(at.Tolerances.Absolute, at.Tolerances.Angle, (int)(PreserveEnd)of.Start - 1, (int)(PreserveEnd)of.End - 1, of.Iterations)), at.Retain),
            rebuild: static (at, of) => Copies.Acquire(nameof(Curve.Rebuild), _ => Edits.Replaced<Curve, int>(at.Curve.Rebuild(of.PointCount, of.Degree, of.PreserveTangents)), at.Retain),
            fit: static (at, of) => Copies.Acquire(nameof(Curve.Fit), _ => Edits.Replaced<Curve, int>(at.Curve.Fit(of.Degree, at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain),
            subDFriendly: static (at, of) => Copies.Acquire(nameof(NurbsCurve.CreateSubDFriendly), _ => Edits.Replaced<Curve, int>(of.Points.Match(
                Some: points => NurbsCurve.CreateSubDFriendly(at.Curve, points.PointCount, points.Periodic),
                None: () => NurbsCurve.CreateSubDFriendly(at.Curve))), at.Retain),
            smooth: static (at, of) => of.Options.Coordinates switch {
                var (system, plane) => Copies.Acquire(nameof(Curve.Smooth), _ => Edits.Replaced<Curve, int>(
                    at.Curve.Smooth(of.Options.Factor, of.Options.X, of.Options.Y, of.Options.Z, of.Options.FixBoundaries, system, plane)), at.Retain),
            },
            reparameterize: static (at, _) => Copies.Acquire(nameof(Curve.Reparameterize), _ => Edits.Replaced<Curve, int>(at.Curve.Reparameterize()), at.Retain),
            makePeriodic: static (at, of) => Copies.Acquire(nameof(Curve.CreatePeriodicCurve), _ => Edits.Replaced<Curve, int>(Curve.CreatePeriodicCurve(at.Curve, of.SmoothSeam)), at.Retain),
            toPolyline: static (at, of) => Copies.Acquire(nameof(Curve.ToPolyline), _ => Edits.Replaced<Curve, int>(of.Limits.ToPolyline(at.Tolerances, at.Curve)), at.Retain),
            toArcsAndLines: static (at, of) => Copies.Acquire(nameof(Curve.ToArcsAndLines), _ =>
                Edits.Replaced<Curve, int>(at.Curve.ToArcsAndLines(at.Tolerances.Absolute, at.Tolerances.Angle, of.Minimum, of.Maximum)), at.Retain),
            toNurbs: static (at, of) => Copies.Acquire(nameof(Curve.ToNurbsCurve), _ => Edits.Replaced<Curve, int>(of.Subdomain.Match(
                Some: subdomain => at.Curve.ToNurbsCurve(subdomain),
                None: () => at.Curve.ToNurbsCurve())), at.Retain),
            filletCorners: static (at, of) => Copies.Acquire(nameof(Curve.CreateFilletCornersCurve), _ =>
                Edits.Replaced<Curve, int>(Curve.CreateFilletCornersCurve(at.Curve, of.Radius, at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain),
            chamferCorners: static (at, of) => Chamfered(at.Tolerances, at.Curve, of.Distance0, of.Distance1, at.Retain));

    private static IO<EditStep<Curve, int>> Chamfered(Tolerances tolerances, Curve curve, PositiveLength before, PositiveLength after, Action<IDisposable> retain) =>
        from corners in IO.lift(() => CurveEvaluation.Points(tolerances, curve, new CurvePointSet.Discontinuities(Continuity.G1_continuous, Math.Cos(tolerances.Angle), RhinoMath.SqrtEpsilon)))
        from cuts in IO.lift(() => Cuts(curve, corners, before, after))
        from step in cuts.IsEmpty
            ? Copies.Acquire(nameof(Curve.GetNextDiscontinuity), _ => Edits.Changed<Curve, int>(curve, changed: false), retain)
            : DisposalOps.Within<IDisposable, EditStep<Curve, int>>(temporary =>
                from pieces in Copies.Acquire(nameof(Curve.Split), _ => Answer.Many(curve.Split(cuts.Bind(static cut => Seq(cut.Start, cut.End)))), temporary)
                from lines in Copies.Acquire(nameof(LineCurve), _ => Answer.Many(cuts.Map(cut => new LineCurve(curve.PointAt(cut.Start), curve.PointAt(cut.End)))), temporary)
                let kept = pieces.Filter(piece => !cuts.Exists(cut => cut.Start < piece.Domain.Mid && piece.Domain.Mid < cut.End))
                from joined in Copies.Acquire(nameof(Curve.JoinCurves), _ => Answer.NonEmpty(Curve.JoinCurves(kept + lines.Map(static line => (Curve)line), tolerances.Absolute, preserveDirection: true)), temporary)
                from single in IO.lift(() => joined.Count is 1
                    ? Fin.Succ(joined[0])
                    : new Refused(nameof(Curve.JoinCurves), string.Create(CultureInfo.InvariantCulture, $"answered {joined.Count} curves for one chamfered curve")))
                from copy in Copies.Duplicate(single, retain)
                select (EditStep<Curve, int>)new EditStep<Curve, int>.Replaced(copy), DisposalOps.Release)
        select step;

    private static Fin<Seq<(double Start, double End)>> Cuts(Curve curve, Seq<CurveSample> corners, PositiveLength before, PositiveLength after) =>
        (Total: curve.GetLength(), Lengths: corners.Map(corner => curve.GetLength(new Interval(curve.Domain.T0, corner.Parameter)) switch {
            var at => (Start: at - (double)before, End: at + (double)after),
        }).Strict()) switch {
            var measured => Callbacks.Each(measured.Lengths, (span, index) =>
                span.Start > (index > 0 ? measured.Lengths[index - 1].End : 0d) && span.End < (index < measured.Lengths.Count - 1 ? measured.Lengths[index + 1].Start : measured.Total)
                    ? (Refused.Unless(curve.LengthParameter(span.Start, out double start), start, nameof(Curve.LengthParameter), index),
                        Refused.Unless(curve.LengthParameter(span.End, out double end), end, nameof(Curve.LengthParameter), index))
                        .Apply(static (start, end) => (Start: start, End: end)).As()
                    : Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"chamfer at corner {index} overlaps its neighbour or a curve end"))),
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

    public IO<EditStep<NurbsCurve, int>> Step(Tolerances tolerances, NurbsCurve nurbs, Action<IDisposable> retain) =>
        Switch(
            (Nurbs: nurbs, Tolerances: tolerances, Retain: retain),
            increaseDegree: static (at, of) => Copies.Acquire(nameof(NurbsCurve.IncreaseDegree), _ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.IncreaseDegree(of.Degree)), at.Retain),
            makePiecewiseBezier: static (at, of) => Copies.Acquire(nameof(NurbsCurve.MakePiecewiseBezier), _ =>
                Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.MakePiecewiseBezier(of.SetEndWeightsToOne)), at.Retain),
            makeUniform: static (at, _) => Copies.Acquire(nameof(NurbsCurve.MakeUniform), _ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.MakeUniform()), at.Retain),
            uniformKnots: static (at, of) => Copies.Acquire(nameof(NurbsCurveKnotList.CreateUniformKnots), _ =>
                Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.CreateUniformKnots(of.Spacing)), at.Retain),
            periodicKnots: static (at, of) => Copies.Acquire(nameof(NurbsCurveKnotList.CreatePeriodicKnots), _ =>
                Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.CreatePeriodicKnots(of.Spacing)), at.Retain),
            insertKnot: static (at, of) => Copies.Acquire(nameof(NurbsCurveKnotList.InsertKnot), _ =>
                Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.InsertKnot(of.At, of.Multiplicity)), at.Retain),
            removeKnots: static (at, of) =>
                IO.lift(() => (Conversions.Within(of.From, at.Nurbs.Knots.Count, nameof(NurbsCurveKnotList.RemoveKnots)), Conversions.Within(of.To - 1, at.Nurbs.Knots.Count, nameof(NurbsCurveKnotList.RemoveKnots)))
                    .Apply(static (_, _) => unit).As())
                >> Copies.Acquire(nameof(NurbsCurveKnotList.RemoveKnots), _ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.RemoveKnots(of.From, of.To)), at.Retain),
            removeKnotAt: static (at, of) => Copies.Acquire(nameof(NurbsCurveKnotList.RemoveKnotAt), _ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.RemoveKnotAt(of.At)), at.Retain),
            removeMultipleKnots: static (at, of) => Copies.Acquire(nameof(NurbsCurveKnotList.RemoveMultipleKnots), _ =>
                Edits.Count(at.Nurbs, at.Nurbs.Knots.RemoveMultipleKnots(of.Minimum, of.Maximum, at.Tolerances.Absolute)), at.Retain),
            clampEnd: static (at, of) => Copies.Acquire(nameof(NurbsCurveKnotList.ClampEnd), _ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Knots.ClampEnd(of.End)), at.Retain),
            setWeight: static (at, of) => IO.lift(() => Conversions.Within(of.Point, at.Nurbs.Points.Count, nameof(NurbsCurvePointList.SetWeight)))
                >> Copies.Acquire(nameof(NurbsCurvePointList.SetWeight), _ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Points.SetWeight(of.Point, of.Weight)), at.Retain),
            changeEndWeights: static (at, of) => Copies.Acquire(nameof(NurbsCurvePointList.ChangeEndWeights), _ =>
                Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Points.ChangeEndWeights(of.Start, of.End)), at.Retain),
            makeRational: static (at, _) => Copies.Acquire(nameof(NurbsCurvePointList.MakeRational), _ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Points.MakeRational()), at.Retain),
            makeNonRational: static (at, _) => Copies.Acquire(nameof(NurbsCurvePointList.MakeNonRational), _ =>
                Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Points.MakeNonRational()), at.Retain),
            rationalReparameterize: static (at, of) => Copies.Acquire(nameof(NurbsCurve.Reparameterize), _ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Reparameterize(of.C)), at.Retain),
            setPoint: static (at, of) => IO.lift(() => Conversions.Within(of.Point, at.Nurbs.Points.Count, nameof(NurbsCurvePointList.SetPoint)))
                >> Copies.Acquire(nameof(NurbsCurvePointList.SetPoint), _ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Points.SetPoint(of.Point, of.Location)), at.Retain),
            setGrevillePoints: static (at, of) => Copies.Acquire(nameof(NurbsCurve.SetGrevillePoints), _ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.SetGrevillePoints(of.Points)), at.Retain),
            setEndCondition: static (at, of) => of.Condition.Parameters switch {
                var (type, point, tangent, curvature) => Copies.Acquire(nameof(NurbsCurve.SetEndCondition), _ =>
                    Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.SetEndCondition(of.SetEnd, type, point, tangent, curvature)), at.Retain),
            },
            append: static (at, of) => Copies.Acquire(nameof(NurbsCurve.Append), _ => Edits.Accepted<NurbsCurve, int>(at.Nurbs, at.Nurbs.Append(of.Next)), at.Retain),
            matchToCurve: static (at, of) => Copies.Acquire(nameof(NurbsCurve.MatchToCurve), _ =>
                Edits.Replaced<NurbsCurve, int>(at.Nurbs.MatchToCurve(of.Target, of.MaxEndDistance, of.MaxInteriorDistance, at.Tolerances.Absolute, of.MaxLevel)), at.Retain));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record PolyCurveEdit {
    public sealed record RemoveNesting : PolyCurveEdit;
    public sealed record Append(PolyCurveSegment Segment) : PolyCurveEdit;
    public sealed record AppendSegment(Curve Segment) : PolyCurveEdit;

    public IO<EditStep<PolyCurve, int>> Step(PolyCurve polycurve, Action<IDisposable> retain) =>
        Switch(
            (PolyCurve: polycurve, Retain: retain),
            removeNesting: static (at, _) => Copies.Acquire(nameof(PolyCurve.RemoveNesting), _ => Edits.Changed<PolyCurve, int>(at.PolyCurve, at.PolyCurve.RemoveNesting()), at.Retain),
            append: static (at, of) => Copies.Acquire(nameof(PolyCurve.Append), _ => Edits.Accepted<PolyCurve, int>(at.PolyCurve, of.Segment.Switch(
                at.PolyCurve,
                line: static (target, line) => target.Append(line),
                arc: static (target, arc) => target.Append(arc),
                curve: static (target, curve) => target.Append(curve))), at.Retain),
            appendSegment: static (at, of) => Copies.Acquire(nameof(PolyCurve.AppendSegment), _ => Edits.Accepted<PolyCurve, int>(at.PolyCurve, at.PolyCurve.AppendSegment(of.Segment)), at.Retain));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveEdits {
    // --- [EDITS]
    public static IO<(Curve Result, Seq<(CurveEdit Edit, EditResult<int> Result)> Results)> Edit(Tolerances tolerances, Curve source, Seq<CurveEdit> edits, Action<IDisposable> retain) =>
        Edits.Apply(source, edits, (curve, edit, held) => edit.Step(tolerances, curve, held), static _ => IO.pure(unit), retain);

    public static IO<(NurbsCurve Result, Seq<(NurbsCurveEdit Edit, EditResult<int> Result)> Results)> Edit(
        Tolerances tolerances, NurbsCurve source, Seq<NurbsCurveEdit> edits, Action<IDisposable> retain) =>
        Edits.Apply(source, edits, (nurbs, edit, held) => edit.Step(tolerances, nurbs, held), static _ => IO.pure(unit), retain);

    public static IO<(PolyCurve Result, Seq<(PolyCurveEdit Edit, EditResult<int> Result)> Results)> Edit(PolyCurve source, Seq<PolyCurveEdit> edits, Action<IDisposable> retain) =>
        Edits.Apply(source, edits, static (polycurve, edit, held) => edit.Step(polycurve, held), static _ => IO.pure(unit), retain);

    // --- [FITS]
    public static IO<NurbsCurveFit> CreateNurbsCurveFit(Curve curve, Interval domain, NurbsCurveFitParameters rebuildOptions, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.CreateNurbsCurveFit), _ =>
            Answer.One(Curve.CreateNurbsCurveFit(curve, domain, rebuildOptions, out Line maximumSeparation, out double source, out double fit))
                .Map(fitted => new NurbsCurveFit(fitted, Conversions.Present(maximumSeparation),
                    from sourceAt in Conversions.Present(source)
                    from fitAt in Conversions.Present(fit)
                    select (Source: sourceAt, Fit: fitAt))), retain);

    public static IO<(NurbsCurve Curve, Option<Line> MaximumDeviation)> RebuildToMatchTemplateCurve(Curve source, Curve templateCurve, bool flipSourceDirection, bool preserveEndTangents, bool makeSubDFriendly, Action<IDisposable> retain) =>
        Copies.Acquire(nameof(Curve.RebuildToMatchTemplateCurve), _ =>
            Answer.One(source.RebuildToMatchTemplateCurve(templateCurve, flipSourceDirection, preserveEndTangents, makeSubDFriendly, out Line maximumDeviation))
                .Map(rebuilt => (Curve: rebuilt, MaximumDeviation: Conversions.Present(maximumDeviation))), retain);

    // --- [SPLIT]
    public static IO<Seq<Curve>> Split(Tolerances tolerances, Curve curve, CurveCut cut, Action<IDisposable> retain) =>
        cut.Switch(
            (Curve: curve, Tolerances: tolerances, Retain: retain),
            parameters: static (at, values) => IO.lift(() => DomainParameters.Validated(at.Curve, values))
                .Bind(set => Copies.Acquire(nameof(Curve.Split), _ => Answer.Many(set.Curve.Split(set.Values)), at.Retain)),
            brep: static (at, cutter) => Copies.Acquire(nameof(Curve.Split), _ => Answer.Many(at.Curve.Split(cutter, at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain),
            surface: static (at, cutter) => Copies.Acquire(nameof(Curve.Split), _ => Answer.Many(at.Curve.Split(cutter, at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain),
            plane: static (at, cutter) => Copies.Acquire(nameof(Curve.Split), _ => Answer.Many(at.Curve.Split(cutter, at.Tolerances.Absolute, at.Tolerances.Angle)), at.Retain));

    internal static Fin<Interval> Spanned(Curve curve, Interval span, string member) =>
        from inside in DomainParameters.Validated(curve, Seq(span.T0, span.T1))
        from increasing in guard(span.IsIncreasing, Error.New(ErrorCodes.ValidationFailed, $"{member} span {span} is not increasing")).ToFin()
        select span;

    // --- [MATCHING]
    public static IO<Seq<NurbsCurve>> MakeCompatible(Tolerances tolerances, Seq<Curve> curves, Option<Point3d> startPt, Option<Point3d> endPt, CurveFit fit, Action<IDisposable> retain) =>
        fit.Parameters switch {
            var fitting => Copies.Acquire(nameof(NurbsCurve.MakeCompatible), _ => Answer.NonEmpty(
                NurbsCurve.MakeCompatible(curves, Conversions.Unset(startPt), Conversions.Unset(endPt), (int)fitting.RebuildType, fitting.RebuildPointCount, fitting.RefitTolerance, tolerances.Angle)), retain),
        };

    public static IO<(Curve A, Curve B)> MakeEndsMeet(Curve curveA, bool adjustStartCurveA, Curve curveB, bool adjustStartCurveB, Action<IDisposable> retain) =>
        DisposalOps.Acquire(owned =>
            from copyA in Copies.Duplicate(curveA, owned)
            from copyB in Copies.Duplicate(curveB, owned)
            from met in Copies.Acquire(nameof(Curve.MakeEndsMeet), _ =>
                (Answer.Accepted(Curve.MakeEndsMeet(copyA, adjustStartCurveA, copyB, adjustStartCurveB)), Answer.One(copyA) with { Leaves = [] }, Answer.One(copyB) with { Leaves = [] })
                    .Apply(static (_, a, b) => (A: a, B: b)).As(), owned)
            select met, retain);
}

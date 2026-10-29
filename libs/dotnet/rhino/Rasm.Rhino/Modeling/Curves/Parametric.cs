using System.Globalization;
using Rasm.Rhino.Document.Units;
using Rasm.Spatial;
using SuperLinq;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ParametricCurve {
    private ParametricCurve(Func<double, Point3d> point, Seq<double> joins) => (Point, Joins) = (point, joins);
    public Func<double, Point3d> Point { get; }
    public Seq<double> Joins { get; }

    public IO<NurbsCurve> ToNurbs(Tolerances tolerances) => CurveLowerings.Traced(Point, Joins, tolerances);

    public sealed record Function : ParametricCurve {
        private Function(Func<double, Point3d> map, Seq<double> joins) : base(map, joins) { }

        public static Fin<Function> Validated(Func<double, Point3d> map, Interval domain, Seq<double> breaks) =>
            (Seq(domain.T0) + breaks + Seq(domain.T1)) switch {
                var joins => Products.Each(toSeq(joins.Window(2, static pair => (Before: pair[0], After: pair[1]))), static (pair, index) => pair.Before < pair.After
                        ? Fin.Succ(unit)
                        : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"break {pair.After} after {pair.Before}"), RhinoReason.Outside, index))
                    .Map(_ => new Function(map, joins)),
            };
    }

    public sealed record Involute : ParametricCurve {
        private Involute(Curve baseCurve, double start)
            : base(t => baseCurve.PointAt(t) - (baseCurve.TangentAt(t) * (t >= start ? baseCurve.GetLength(new Interval(start, t)) : -baseCurve.GetLength(new Interval(t, start)))), Joined(baseCurve.Domain, [start])) => (Base, Start) = (baseCurve, start);
        public Curve Base { get; }
        public double Start { get; }

        public static Fin<Involute> Validated(Tolerances tolerances, Curve baseCurve, double start) =>
            (Conversions.Within(baseCurve.Domain, start),
             CurveEvaluation.Kinks(tolerances, baseCurve, Continuity.G1_continuous) switch { var kinks => RhinoFault.Unless(kinks.IsEmpty, RhinoReason.Invalid, kinks.Count) })
                .Apply((at, _) => new Involute(baseCurve, at)).As();
    }

    public sealed record Evolute : ParametricCurve {
        private Evolute(Curve baseCurve, Seq<double> joins) : base(t => baseCurve.CurvatureAt(t) switch { var curvature => baseCurve.PointAt(t) + (curvature / curvature.SquareLength) }, joins) => Base = baseCurve;
        public Curve Base { get; }

        public static Fin<Evolute> Validated(Tolerances tolerances, Curve baseCurve) =>
            (new Spacing.Inflections().Along(tolerances, baseCurve).Bind(static inflections => RhinoFault.Unless(inflections.IsEmpty, RhinoReason.Invalid, inflections.Count)),
             new Spacing.CurvatureMaxima().Along(tolerances, baseCurve),
             new Spacing.Discontinuities(Continuity.G2_continuous).Along(tolerances, baseCurve))
                .Apply((_, maxima, breaks) => new Evolute(baseCurve, Joined(baseCurve.Domain, (maxima + breaks).Map(static row => row.Parameter)))).As();
    }

    private static Seq<double> Joined(Interval domain, Seq<double> interior) => Seq(domain.T0) + toSeq(interior.Filter(t => t > domain.T0 && t < domain.T1).Distinct().Order()) + Seq(domain.T1);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class CurveLowerings {
    // --- [CLOSED_FORMS]
    extension(AnalyticCurve curve) {
        public IO<NurbsCurve> ToNurbs(Plane frame, Tolerances tolerances) => Traced(t => curve.At(t).ToPoint3d(frame), curve.Joins, tolerances);
    }

    extension(RationalQuadratic conic) {
        public IO<NurbsCurve> ToNurbs(Plane frame) =>
            from bezier in Products.Of(() => new BezierCurve([new Point4d(conic.Start.ToPoint3d(frame)), new Point4d(conic.Apex.ToPoint3d(frame)) * conic.Weight, new Point4d(conic.End.ToPoint3d(frame))]))
            from nurbs in Products.Of(() => bezier.ToNurbsCurve())
            select nurbs;
    }

    extension(HermiteSpline spline) {
        public IO<NurbsCurve> ToNurbs(Plane frame) =>
            from pieces in spline.Spans.TraverseM(span =>
                from bezier in Products.Of(() => new BezierCurve([span.Start.ToPoint3d(frame), span.StartControl.ToPoint3d(frame), span.EndControl.ToPoint3d(frame), span.End.ToPoint3d(frame)]))
                from nurbs in Products.Of(() => bezier.ToNurbsCurve())
                select nurbs).As()
            from joined in Appended(pieces)
            select joined;
    }

    extension(Ellipse ellipse) {
        public IO<NurbsCurve> ToNurbs(Interval angles) =>
            from circle in Products.Of(() => NurbsCurve.CreateFromArc(new Arc(new Circle(ellipse.Plane, 1d), angles)))
            from scaled in Products.Accepted(() => circle.Transform(Transform.Scale(ellipse.Plane, ellipse.Radius1, ellipse.Radius2, 1d)))
            select circle;
    }

    // --- [TRACING]
    internal static IO<NurbsCurve> Traced(Func<double, Point3d> point, Seq<double> joins, Tolerances tolerances) =>
        from pieces in toSeq(joins.Window(2, static span => new Line(span[0], 0d, 0d, span[1], 0d, 0d))).TraverseM(span =>
            from line in Products.Of(() => NurbsCurve.CreateFromLine(span))
            from morphed in Morphs.Morph(tolerances, Seq(line), new MorphKind.Point(at => point(at.X)), preserveStructure: false, quickPreview: false)
            select morphed[0]).As()
        from joined in Appended(pieces)
        select joined;

    private static IO<NurbsCurve> Appended(Seq<NurbsCurve> pieces) =>
        Edits.Apply(Stages.Geometry<NurbsCurve>(), pieces[0], pieces.Tail.Map(static next => Steps.Of<NurbsCurve>(whole => whole.Append(next))));
}

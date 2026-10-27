using System.Globalization;
using Rasm.RhinoCommon.Document.Units;
using Rhino;
using SuperLinq;

namespace Rasm.RhinoCommon.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ParametricCurve {
    private ParametricCurve(Func<double, Point3d> point, Seq<double> joins) => (Point, Joins) = (point, joins);
    public Func<double, Point3d> Point { get; }
    public Seq<double> Joins { get; }

    public IO<NurbsCurve> ToNurbs(Tolerances tolerances) => Traced(Point, Joins, tolerances);

    public sealed record Function : ParametricCurve {
        private Function(Func<double, Point3d> map, Seq<double> joins) : base(map, joins) { }

        public static Fin<Function> Validated(Func<double, Point3d> map, Interval domain, Seq<double> breaks) =>
            (Seq(domain.T0) + breaks + Seq(domain.T1)) switch {
                var joins => Callbacks.Each(toSeq(joins.Window(2, static pair => (Before: pair[0], After: pair[1]))), static (pair, index) => pair.Before < pair.After
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
             CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.Discontinuities(Continuity.G1_continuous, Math.Cos(tolerances.Angle), RhinoMath.SqrtEpsilon))
                 .Bind(static kinks => RhinoFault.Unless(kinks.IsEmpty)))
                .Apply((at, _) => new Involute(baseCurve, at)).As();
    }

    public sealed record Evolute : ParametricCurve {
        private Evolute(Curve baseCurve, Seq<double> joins) : base(t => baseCurve.CurvatureAt(t) switch { var curvature => baseCurve.PointAt(t) + (curvature / curvature.SquareLength) }, joins) => Base = baseCurve;
        public Curve Base { get; }

        public static Fin<Evolute> Validated(Tolerances tolerances, Curve baseCurve) =>
            (CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.InflectionPoints()).Bind(static inflections => RhinoFault.Unless(inflections.IsEmpty)),
             CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.MaxCurvaturePoints()),
             CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.Discontinuities(Continuity.G2_continuous, Math.Cos(tolerances.Angle), RhinoMath.SqrtEpsilon)))
                .Apply((_, maxima, breaks) => new Evolute(baseCurve, Joined(baseCurve.Domain, (maxima + breaks).Map(static sample => sample.Parameter)))).As();
    }

    internal static IO<NurbsCurve> Traced(Func<double, Point3d> point, Seq<double> joins, Tolerances tolerances) =>
        from pieces in toSeq(joins.Window(2, static span => new Line(span[0], 0d, 0d, span[1], 0d, 0d))).TraverseM(span =>
            from line in Products.Made(NurbsCurve.CreateFromLine(span))
            from morphed in Morphs.Morph(tolerances, Seq(line), new MorphKind.Point(at => point(at.X)), new MorphSettings(QuickPreview: false, PreserveStructure: false))
            select morphed[0]).As()
        from joined in CurveConstruction.Appended(pieces)
        select joined;

    private static Seq<double> Joined(Interval domain, Seq<double> interior) => Seq(domain.T0) + toSeq(interior.Filter(t => t > domain.T0 && t < domain.T1).Distinct().Order()) + Seq(domain.T1);
}

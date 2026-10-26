using System.Globalization;
using Rasm.Rhino.Document.Units;
using Rhino;
using SuperLinq;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ParametricCurve {
    private ParametricCurve(Func<double, Point3d> point, Seq<double> joins) => (Point, Joins) = (point, joins);
    public Func<double, Point3d> Point { get; }
    public Seq<double> Joins { get; }

    public IO<NurbsCurve> ToNurbs(Tolerances tolerances, Action<IDisposable> retain) => Traced(Point, Joins, tolerances, retain);

    public sealed record Function : ParametricCurve {
        private Function(Func<double, Point3d> map, Seq<double> joins) : base(map, joins) { }

        public static Fin<Function> Validated(Func<double, Point3d> map, Interval domain, Seq<double> breaks) =>
            (Seq(domain.T0) + breaks + Seq(domain.T1)) switch {
                var joins => joins.Window(2, static pair => pair[0] < pair[1]).All(static ascending => ascending) ? new Function(map, joins) : new RhinoFault($"parametric breaks {breaks} do not ascend strictly inside {domain}"),
            };
    }

    public sealed record Involute : ParametricCurve {
        private Involute(Curve baseCurve, double start)
            : base(t => baseCurve.PointAt(t) - (baseCurve.TangentAt(t) * (t >= start ? baseCurve.GetLength(new Interval(start, t)) : -baseCurve.GetLength(new Interval(t, start)))), Joined(baseCurve.Domain, [start])) => (Base, Start) = (baseCurve, start);
        public Curve Base { get; }
        public double Start { get; }

        public static Fin<Involute> Validated(Tolerances tolerances, Curve baseCurve, double start) =>
            (Conversions.Within(baseCurve.Domain, start, nameof(Curve.GetLength)),
             CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.Discontinuities(Continuity.G1_continuous, Math.Cos(tolerances.Angle), RhinoMath.SqrtEpsilon)).Bind(static kinks => kinks.IsEmpty
                 ? Fin.Succ(unit)
                 : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"involute base holds {kinks.Count} tangent breaks, where the involute jumps"))))
                .Apply((at, _) => new Involute(baseCurve, at)).As();
    }

    public sealed record Evolute : ParametricCurve {
        private Evolute(Curve baseCurve, Seq<double> joins) : base(t => baseCurve.CurvatureAt(t) switch { var curvature => baseCurve.PointAt(t) + (curvature / curvature.SquareLength) }, joins) => Base = baseCurve;
        public Curve Base { get; }

        public static Fin<Evolute> Validated(Tolerances tolerances, Curve baseCurve) =>
            (CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.InflectionPoints()).Bind(static inflections => inflections.IsEmpty
                 ? Fin.Succ(unit)
                 : new RhinoFault(string.Create(CultureInfo.InvariantCulture, $"evolute base holds {inflections.Count} inflection points, where the evolute runs to infinity"))),
             CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.MaxCurvaturePoints()),
             CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.Discontinuities(Continuity.G2_continuous, Math.Cos(tolerances.Angle), RhinoMath.SqrtEpsilon)))
                .Apply((_, maxima, breaks) => new Evolute(baseCurve, Joined(baseCurve.Domain, (maxima + breaks).Map(static sample => sample.Parameter)))).As();
    }

    internal static IO<NurbsCurve> Traced(Func<double, Point3d> point, Seq<double> joins, Tolerances tolerances, Action<IDisposable> retain) =>
        DisposalOps.Within<IDisposable, NurbsCurve>(temporary =>
            from pieces in toSeq(joins.Window(2, static span => new Line(span[0], 0d, 0d, span[1], 0d, 0d))).TraverseM(span =>
                from line in Copies.Acquire(nameof(NurbsCurve.CreateFromLine), _ => Answer.One(NurbsCurve.CreateFromLine(span)), temporary)
                from morphed in Morphs.Morph(tolerances, Seq(line), new MorphKind.Point(at => point(at.X)), new MorphSettings(QuickPreview: false, PreserveStructure: false), temporary)
                select morphed[0]).As()
            from joined in CurveConstruction.Appended(pieces, retain)
            select joined, DisposalOps.Release);

    private static Seq<double> Joined(Interval domain, Seq<double> interior) => Seq(domain.T0) + toSeq(interior.Filter(t => t > domain.T0 && t < domain.T1).Distinct().Order()) + Seq(domain.T1);
}

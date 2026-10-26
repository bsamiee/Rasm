using MathNet.Numerics;
using Rasm.Rhino.Document.Units;
using Rhino;

namespace Rasm.Rhino.Modeling.Curves;

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Roll {
    public sealed record Line(int Arches) : Roll;

    public sealed record Outside(int Fixed, int Rolling) : Roll;

    public sealed record Inside(int Fixed, int Rolling) : Roll;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record TracingPoint {
    public sealed record Rim : TracingPoint;

    public sealed record At(double Distance) : TracingPoint;
}

public readonly record struct HarmonicTerm(int Frequency, Vector3d Cosine, Vector3d Sine);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record ParametricCurve {
    private ParametricCurve(Func<double, Point3d> point, Interval domain, Seq<double> breaks) => (Point, Domain, Breaks) = (point, domain, breaks);

    public Func<double, Point3d> Point { get; }

    public Interval Domain { get; }

    public Seq<double> Breaks { get; }

    public IO<NurbsCurve> ToNurbs(Tolerances tolerances, Action<IDisposable> retain) =>
        DisposalOps.Within<IDisposable, NurbsCurve>(temporary =>
            from pieces in (Seq(Domain.T0) + Breaks + Seq(Domain.T1)) switch {
                var ends => ends.Zip(ends.Tail).TraverseM(span => Piece(tolerances, span.First, span.Second, temporary)).As(),
            }
            from joined in CurveConstruction.Appended(pieces, retain)
            select joined, DisposalOps.Release);

    public sealed record Trochoid : ParametricCurve {
        private Trochoid(Plane frame, Roll roll, double rollingRadius, TracingPoint pen, (Func<double, Point3d> Point, Interval Domain, Seq<double> Breaks) traced)
            : base(traced.Point, traced.Domain, traced.Breaks) => (Frame, Roll, RollingRadius, Pen) = (frame, roll, rollingRadius, pen);

        public Plane Frame { get; }

        public Roll Roll { get; }

        public double RollingRadius { get; }

        public TracingPoint Pen { get; }

        public static Fin<Trochoid> Validated(Plane frame, Roll roll, double rollingRadius, TracingPoint pen) =>
            roll.Switch(
                (Frame: frame, Radius: rollingRadius, Pen: pen.Switch(rollingRadius, rim: static (radius, _) => (Distance: radius, Cusps: true), at: static (_, at) => (at.Distance, Cusps: false))),
                line: static (state, line) =>
                    from arches in Counted(line.Arches, "trochoid arch count")
                    select (
                        Point: new Func<double, Point3d>(t => state.Frame.PointAt((state.Radius * t) - (state.Pen.Distance * Math.Sin(t)), state.Radius - (state.Pen.Distance * Math.Cos(t)))),
                        Domain: new Interval(0d, double.Tau * arches),
                        Breaks: state.Pen.Cusps ? toSeq(Range(1, arches - 1)).Map(static arch => double.Tau * arch).Strict() : Seq<double>()),
                outside: static (state, outside) => Circular(state.Frame, state.Radius, state.Pen, outside.Fixed, outside.Rolling, 1),
                inside: static (state, inside) => Circular(state.Frame, state.Radius, state.Pen, inside.Fixed, inside.Rolling, -1))
            .Map(traced => new Trochoid(frame, roll, rollingRadius, pen, traced));
    }

    public sealed record Harmonic : ParametricCurve {
        private Harmonic(Plane frame, Seq<HarmonicTerm> terms, Interval domain)
            : base(t => terms.Fold(Vector3d.Zero, (sum, term) => sum + (Math.Cos(term.Frequency * t) * term.Cosine) + (Math.Sin(term.Frequency * t) * term.Sine)) switch {
                var local => frame.PointAt(local.X, local.Y, local.Z),
            }, domain, []) => (Frame, Terms) = (frame, terms);

        public Plane Frame { get; }

        public Seq<HarmonicTerm> Terms { get; }

        public static Fin<Harmonic> Validated(Plane frame, Seq<HarmonicTerm> terms) =>
            from held in Callbacks.Each(terms, static (term, index) => term.Frequency >= 0
                ? Fin.Succ(term)
                : Invalid($"harmonic term {index} has negative frequency {term.Frequency}"))
            let moving = held.Filter(static term => term.Frequency > 0 && !(term.Cosine.IsZero && term.Sine.IsZero)).Map(static term => (long)term.Frequency).Strict()
            from traced in guard(!moving.IsEmpty, Invalid($"harmonic terms trace a point")).ToFin()
            select new Harmonic(frame, held, new Interval(0d, double.Tau / Euclid.GreatestCommonDivisor(moving.ToArray())));
    }

    public sealed record Superformula : ParametricCurve {
        private Superformula(Plane frame, int symmetryNumerator, int symmetryDenominator, double n1, double n2, double n3, double a, double b, Interval domain, Seq<double> breaks)
            : base(phi => (symmetryNumerator * phi / (4d * symmetryDenominator)) switch {
                var quarter => Math.Pow(Math.Pow(Math.Abs(Math.Cos(quarter) / a), n2) + Math.Pow(Math.Abs(Math.Sin(quarter) / b), n3), -1d / n1) switch {
                    var radius => frame.PointAt(radius * Math.Cos(phi), radius * Math.Sin(phi)),
                },
            }, domain, breaks) => (Frame, SymmetryNumerator, SymmetryDenominator, N1, N2, N3, A, B) = (frame, symmetryNumerator, symmetryDenominator, n1, n2, n3, a, b);

        public Plane Frame { get; }

        public int SymmetryNumerator { get; }

        public int SymmetryDenominator { get; }

        public double N1 { get; }

        public double N2 { get; }

        public double N3 { get; }

        public double A { get; }

        public double B { get; }

        public static Fin<Superformula> Validated(Plane frame, int symmetryNumerator, int symmetryDenominator, double n1, double n2, double n3, double a, double b) =>
            from p in Counted(symmetryNumerator, "superformula symmetry numerator")
            from q in Counted(symmetryDenominator, "superformula symmetry denominator")
            from shaped in guard(n1 is not 0d && n2 > 0d && n3 > 0d && a > 0d && b > 0d, Invalid($"superformula needs n1 other than 0 and positive n2 {n2}, n3 {n3}, a {a} and b {b}")).ToFin()
            let divisor = Euclid.GreatestCommonDivisor(p, 2L * q)
            let lobes = (int)(p / divisor)
            let domain = new Interval(0d, double.Tau * 2d * q / divisor)
            select new Superformula(frame, p, q, n1, n2, n3, a, b, domain, Interior(domain, Seq(
                    (Corner: n2 <= 1d, Zeros: toSeq(Range(0, lobes)).Map(lobe => double.Tau * q * ((2 * lobe) + 1) / p)),
                    (Corner: n3 <= 1d, Zeros: toSeq(Range(1, lobes - 1)).Map(lobe => 2d * double.Tau * q * lobe / p)))
                .Filter(static term => term.Corner)
                .Bind(static term => term.Zeros)));
    }

    public sealed record Intrinsic : ParametricCurve {
        private Intrinsic(Plane frame, Seq<double> curvature, Interval length)
            : base(s => frame.PointAt(
                Integrate.GaussKronrod(u => Math.Cos(Turning(curvature, u)), 0d, s, out _, out _),
                Integrate.GaussKronrod(u => Math.Sin(Turning(curvature, u)), 0d, s, out _, out _)), length, []) => (Frame, Curvature, Length) = (frame, curvature, length);

        public Plane Frame { get; }

        public Seq<double> Curvature { get; }

        public Interval Length { get; }

        public static Fin<Intrinsic> Validated(Plane frame, Seq<double> curvature, Interval length) =>
            from varying in guard(curvature.Tail.Exists(static coefficient => coefficient is not 0d), Invalid($"intrinsic curvature holds no nonzero coefficient past the first, a line or an arc")).ToFin()
            from spanned in guard(length.IsIncreasing, Invalid($"intrinsic length {length} is not increasing")).ToFin()
            select new Intrinsic(frame, curvature, length);

        private static double Turning(Seq<double> curvature, double s) =>
            curvature.Map(static (coefficient, power) => (Coefficient: coefficient, Power: power + 1)).Fold(0d, (sum, term) => sum + (term.Coefficient * Math.Pow(s, term.Power) / term.Power));
    }

    public sealed record Involute : ParametricCurve {
        private Involute(Curve baseCurve, double start)
            : base(t => baseCurve.PointAt(t) - (baseCurve.TangentAt(t) * (t >= start ? baseCurve.GetLength(new Interval(start, t)) : -baseCurve.GetLength(new Interval(t, start)))),
                baseCurve.Domain, Interior(baseCurve.Domain, [start])) => (Base, Start) = (baseCurve, start);

        public Curve Base { get; }

        public double Start { get; }

        public static Fin<Involute> Validated(Tolerances tolerances, Curve baseCurve, double start) =>
            from at in Conversions.Within(baseCurve.Domain, start, nameof(Curve.GetLength))
            from kinks in CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.Discontinuities(Continuity.G1_continuous, Math.Cos(tolerances.Angle), RhinoMath.SqrtEpsilon))
            from smooth in guard(kinks.IsEmpty, Invalid($"involute base holds {kinks.Count} tangent breaks, where the involute jumps")).ToFin()
            select new Involute(baseCurve, at);
    }

    public sealed record Evolute : ParametricCurve {
        private Evolute(Curve baseCurve, Seq<double> breaks)
            : base(t => baseCurve.CurvatureAt(t) switch {
                var curvature => baseCurve.PointAt(t) + (curvature / curvature.SquareLength),
            }, baseCurve.Domain, breaks) => Base = baseCurve;

        public Curve Base { get; }

        public static Fin<Evolute> Validated(Tolerances tolerances, Curve baseCurve) =>
            from inflections in CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.InflectionPoints())
            from convex in guard(inflections.IsEmpty, Invalid($"evolute base holds {inflections.Count} inflection points, where the evolute runs to infinity")).ToFin()
            from maxima in CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.MaxCurvaturePoints())
            from breaks in CurveEvaluation.Points(tolerances, baseCurve, new CurvePointSet.Discontinuities(Continuity.G2_continuous, Math.Cos(tolerances.Angle), RhinoMath.SqrtEpsilon))
            select new Evolute(baseCurve, Interior(baseCurve.Domain, (maxima + breaks).Map(static sample => sample.Parameter)));
    }

    public sealed record Function : ParametricCurve {
        private Function(Func<double, Point3d> map, Interval domain, Seq<double> breaks) : base(map, domain, breaks) { }

        public static Fin<Function> Validated(Func<double, Point3d> map, Interval domain, Seq<double> breaks) =>
            from spanned in guard(domain.IsIncreasing, Invalid($"parametric domain {domain} is not increasing")).ToFin()
            from ordered in guard(
                breaks.ForAll(t => t > domain.T0 && t < domain.T1) && breaks.Zip(breaks.Tail).ForAll(static pair => pair.First < pair.Second),
                Invalid($"parametric breaks are not strictly increasing inside {domain}")).ToFin()
            select new Function(map, domain, breaks);
    }

    private static Fin<(Func<double, Point3d> Point, Interval Domain, Seq<double> Breaks)> Circular(Plane frame, double radius, (double Distance, bool Cusps) pen, int fixedCount, int rollingCount, int side) =>
        from fixedTeeth in Counted(fixedCount, "trochoid fixed count")
        from rollingTeeth in Counted(rollingCount, "trochoid rolling count")
        let divisor = (int)Euclid.GreatestCommonDivisor(fixedTeeth, rollingTeeth)
        let p = fixedTeeth / divisor
        let q = rollingTeeth / divisor
        from rolled in guard(side > 0 || (p != q && p != 2 * q), Invalid($"inside roll {fixedCount}:{rollingCount} traces a point, a line or an ellipse")).ToFin()
        let center = radius * (p + (side * q)) / q
        let speed = (p + (side * q)) / (double)q
        select (
            Point: new Func<double, Point3d>(t => frame.PointAt((center * Math.Cos(t)) - (side * pen.Distance * Math.Cos(speed * t)), (center * Math.Sin(t)) - (pen.Distance * Math.Sin(speed * t)))),
            Domain: new Interval(0d, double.Tau * q),
            Breaks: pen.Cusps ? toSeq(Range(1, p - 1)).Map(cusp => double.Tau * cusp * q / p).Strict() : Seq<double>());

    private static Fin<int> Counted(int count, string subject) =>
        count >= 1 ? count : Invalid($"{subject} {count} is below 1");

    private static Seq<double> Interior(Interval domain, Seq<double> candidates) =>
        toSeq(candidates.Filter(t => t > domain.T0 && t < domain.T1).Distinct().Order()).Strict();

    private IO<NurbsCurve> Piece(Tolerances tolerances, double start, double end, Action<IDisposable> temporary) =>
        from line in Copies.Acquire(nameof(NurbsCurve.CreateFromLine), _ => Answer.One(NurbsCurve.CreateFromLine(new Line(start, 0d, 0d, end, 0d, 0d))), temporary)
        from morphed in Morphs.Morph(tolerances, Seq(line), new MorphKind.Point(at => Point(at.X)), new MorphSettings(QuickPreview: false, PreserveStructure: false), temporary)
        select morphed[0];

    private static Error Invalid(FormattableString rule) => Error.New(ErrorCodes.ValidationFailed, FormattableString.Invariant(rule));
}

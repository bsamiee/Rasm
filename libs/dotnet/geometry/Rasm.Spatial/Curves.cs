using System.Numerics;
using MathNet.Numerics;

namespace Rasm.Spatial;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class ParameterSpan {
    public double Start { get; }
    public double End { get; }
    public double Length => End - Start;

    public static Fin<ParameterSpan> Validated(double start, double end) =>
        SpatialFault.Unless(double.IsFinite(start) && double.IsFinite(end) && start < end, SpatialRule.Order, (start, end)).Map(_ => new ParameterSpan(start, end));
}

[SmartEnum]
public sealed partial class RollSide {
    public static readonly RollSide Outside = new(1);
    public static readonly RollSide Inside = new(-1);

    public int Sign { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record Roll {
    public sealed record Line(int Arches) : Roll;
    public sealed record Circle(int Fixed, int Rolling, RollSide Side) : Roll;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record TracingPoint {
    public sealed record Rim : TracingPoint;
    public sealed record At(double Distance) : TracingPoint;
}

public sealed record HarmonicTerm(int Frequency, Vector3D<double> Cosine, Vector3D<double> Sine);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public abstract partial record AnalyticCurve {
    private AnalyticCurve(Seq<double> joins) => Joins = joins;
    public Seq<double> Joins { get; }
    public abstract Vector3D<double> At(double t);

    public sealed record Trochoid : AnalyticCurve {
        private Trochoid(Roll roll, double rollingRadius, TracingPoint pen, Seq<double> joins) : base(joins) => (Roll, RollingRadius, Pen) = (roll, rollingRadius, pen);
        public Roll Roll { get; }
        public double RollingRadius { get; }
        public TracingPoint Pen { get; }

        public override Vector3D<double> At(double t) =>
            Planar(Roll.Switch(
                (Radius: RollingRadius, Arm: Pen.Switch(RollingRadius, rim: static (radius, _) => radius, at: static (_, at) => at.Distance), Turn: t),
                line: static (pen, _) => new Complex((pen.Radius * pen.Turn) - (pen.Arm * double.Sin(pen.Turn)), pen.Radius - (pen.Arm * double.Cos(pen.Turn))),
                circle: static (pen, circle) => ((circle.Fixed + (circle.Side.Sign * circle.Rolling)) / (double)circle.Rolling) switch {
                    var speed => (pen.Radius * speed * Complex.FromPolarCoordinates(1d, pen.Turn)) - (circle.Side.Sign * Complex.FromPolarCoordinates(pen.Arm, circle.Side.Sign * speed * pen.Turn)),
                }));

        public static Fin<Trochoid> Validated(Roll roll, double rollingRadius, TracingPoint pen) =>
            (SpatialFault.Unless(double.IsFinite(rollingRadius) && rollingRadius > 0d, SpatialRule.Range, rollingRadius), roll.Switch(
                pen is TracingPoint.Rim,
                line: static (rim, line) => SpatialFault.Unless(line.Arches >= 1, SpatialRule.Count, line.Arches).Map(_ => Spaced(double.Tau * line.Arches, rim ? line.Arches : 1)),
                circle: static (rim, circle) =>
                    (SpatialFault.Unless(circle.Fixed >= 1 && circle.Rolling >= 1, SpatialRule.Count, (circle.Fixed, circle.Rolling)),
                     SpatialFault.Unless(circle.Side == RollSide.Outside || (circle.Fixed != circle.Rolling && circle.Fixed != 2 * circle.Rolling), SpatialRule.Degenerate, (circle.Side, circle.Fixed, circle.Rolling)))
                        .Apply((_, _) => Euclid.GreatestCommonDivisor(circle.Fixed, circle.Rolling) switch { var gcd => Spaced(double.Tau * circle.Rolling / gcd, rim ? (int)(circle.Fixed / gcd) : 1) }).As()))
                .Apply((_, joins) => new Trochoid(roll, rollingRadius, pen, joins)).As();
    }

    public sealed record Harmonic : AnalyticCurve {
        private Harmonic(Seq<HarmonicTerm> terms, Seq<double> joins) : base(joins) => Terms = terms;
        public Seq<HarmonicTerm> Terms { get; }
        public override Vector3D<double> At(double t) =>
            Terms.Fold(new Vector3D<double>(0d, 0d, 0d), (sum, term) => double.SinCos(term.Frequency * t) switch { var (sin, cos) => sum + (cos * term.Cosine) + (sin * term.Sine) });

        public static Fin<Harmonic> Validated(Seq<HarmonicTerm> terms) =>
            terms.Filter(static term => term.Frequency > 0 && (term.Cosine.LengthSquared() > 0d || term.Sine.LengthSquared() > 0d)).Map(static term => (long)term.Frequency) switch {
                var moving => (terms.Map(static (term, index) => SpatialFault.Unless(term.Frequency >= 0, SpatialRule.Range, term.Frequency, index)).Traverse(static held => held).As(), SpatialFault.Unless(!moving.IsEmpty, SpatialRule.Degenerate, moving))
                    .Apply((_, _) => new Harmonic(terms, Seq(0d, double.Tau / Euclid.GreatestCommonDivisor(moving.ToArray())))).As(),
            };
    }

    public sealed record Superformula : AnalyticCurve {
        private Superformula(int symmetryNumerator, int symmetryDenominator, double n1, double n2, double n3, double a, double b, Seq<double> joins) : base(joins) =>
            (Symmetry, Exponents, Radii) = ((symmetryNumerator, symmetryDenominator), (n1, n2, n3), (a, b));
        public (int Numerator, int Denominator) Symmetry { get; }
        public (double N1, double N2, double N3) Exponents { get; }
        public (double A, double B) Radii { get; }
        public override Vector3D<double> At(double t) =>
            double.SinCos(Symmetry.Numerator * t / (4d * Symmetry.Denominator)) switch { var (sin, cos) => Planar(Complex.FromPolarCoordinates(double.Pow(double.Pow(double.Abs(cos / Radii.A), Exponents.N2) + double.Pow(double.Abs(sin / Radii.B), Exponents.N3), -1d / Exponents.N1), t)) };

        public static Fin<Superformula> Validated(int symmetryNumerator, int symmetryDenominator, double n1, double n2, double n3, double a, double b) =>
            (SpatialFault.Unless(symmetryNumerator >= 1 && symmetryDenominator >= 1, SpatialRule.Count, (symmetryNumerator, symmetryDenominator)),
             SpatialFault.Unless(double.IsFinite(n1) && n1 is not 0d && n2 > 0d && n3 > 0d && a > 0d && b > 0d, SpatialRule.Range, (n1, n2, n3, a, b)))
                .Apply((_, _) => (Step: double.Tau * symmetryDenominator / symmetryNumerator, Count: (int)(2L * symmetryNumerator / Euclid.GreatestCommonDivisor(symmetryNumerator, 2L * symmetryDenominator))) switch {
                    var zeros => new Superformula(symmetryNumerator, symmetryDenominator, n1, n2, n3, a, b, toSeq(Range(0, zeros.Count + 1))
                        .Filter(zero => zero is 0 || zero == zeros.Count || (int.IsOddInteger(zero) ? n2 <= 1d : n3 <= 1d)).Map(zero => zeros.Step * zero).Strict()),
                }).As();
    }

    public sealed record Intrinsic : AnalyticCurve {
        private Intrinsic(Polynomial curvature, ParameterSpan length) : base(Seq(length.Start, length.End)) => (Curvature, Length, Turning) = (curvature, length, curvature.Integrate());
        public Polynomial Curvature { get; }
        public ParameterSpan Length { get; }
        private Polynomial Turning { get; }
        public override Vector3D<double> At(double t) => Planar(ContourIntegrate.GaussKronrod(u => Complex.FromPolarCoordinates(1d, Turning.Evaluate(u)), 0d, t));

        public static Fin<Intrinsic> Validated(Polynomial curvature, ParameterSpan length) => SpatialFault.Unless(curvature.Degree >= 1, SpatialRule.Degenerate, curvature.Degree).Map(_ => new Intrinsic(curvature, length));
    }

    private static Vector3D<double> Planar(Complex point) => new(point.Real, point.Imaginary, 0d);
    private static Seq<double> Spaced(double end, int pieces) => toSeq(Range(0, pieces + 1)).Map(piece => end * piece / pieces).Strict();
}

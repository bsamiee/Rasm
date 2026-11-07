using MathNet.Numerics.RootFinding;

namespace Rasm.UI.Motion;

// --- [MODELS] --------------------------------------------------------------------------

[ComplexValueObject(SkipFactoryMethods = true)]
public sealed partial class Easing {
    public static Easing Linear { get; } = new(0d, 0d, 1d, 1d);
    public static Easing Ease { get; } = new(0.25d, 0.1d, 0.25d, 1d);
    public static Easing EaseIn { get; } = new(0.42d, 0d, 1d, 1d);
    public static Easing EaseOut { get; } = new(0d, 0d, 0.58d, 1d);
    public static Easing EaseInOut { get; } = new(0.42d, 0d, 0.58d, 1d);

    private static readonly double Accuracy = MathF.BitIncrement(1f) - 1f;

    public double X1 { get; }
    public double Y1 { get; }
    public double X2 { get; }
    public double Y2 { get; }

    public static Fin<Easing> Validated(double x1, double y1, double x2, double y2) =>
        UIFault.Unless(x1 is >= 0d and <= 1d && x2 is >= 0d and <= 1d && double.IsFinite(y1) && double.IsFinite(y2), UIReason.Range, (x1, y1, x2, y2)).Map(_ => new Easing(x1, y1, x2, y2));

    public Option<(double Progress, double Slope)> At(Fraction time) =>
        Brent.TryFindRoot(at => Bezier(at, X1, X2) - (float)time, 0d, 1d, Accuracy, maxIterations: 64, out double s)
            ? Some((Bezier(s, Y1, Y2), (Tangent(s, X1, X2), Tangent(s, Y1, Y2)) switch {
                (0d, 0d) => (Acceleration(s, X1, X2), Acceleration(s, Y1, Y2)) switch {
                    (0d, 0d) => (1d - (3d * Y2) + (3d * Y1)) / (1d - (3d * X2) + (3d * X1)),
                    var (across, up) => up / across,
                },
                var (across, up) => up / across,
            }))
            : None;

    private static double Bezier(double s, double first, double second) => (3d * (1d - s) * (1d - s) * s * first) + (3d * (1d - s) * s * s * second) + (s * s * s);

    private static double Tangent(double s, double first, double second) => (3d * (1d - s) * (1d - s) * first) + (6d * (1d - s) * s * (second - first)) + (3d * s * s * (1d - second));

    private static double Acceleration(double s, double first, double second) => (6d * (1d - s) * (second - (2d * first))) + (6d * s * (1d - (2d * second) + first));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TimingCurve {

    // --- [SAMPLING]

    internal abstract Option<(double Progress, double Rate)> At(Duration elapsed);

    internal virtual TimingCurve WithVelocity(double velocity) => this;

    internal bool Animates => Map(instant: false, eased: true, spring: true, decay: true);

    private static Option<(double Progress, double Rate)> Oscillated(double damping, double phase, double start, Factor rest) {
        double root = double.Sqrt(double.Abs((1d - damping) * (1d + damping)));
        double decay = double.Exp(-damping * phase);
        double slow = double.Exp(-phase / (damping + root));
        double gap = double.ExpM1(-2d * root * phase);
        (double even, double odd) = (damping - 1d) switch {
            < 0d => (decay * double.Cos(root * phase), decay * double.Sin(root * phase) / root),
            > 0d => (0.5d * slow * (2d + gap), -0.5d * slow * gap / root),
            _ => (decay, phase * decay),
        };
        double remaining = even + ((damping - start) * odd);
        double rate = (start * even) + ((1d - (damping * start)) * odd);
        return double.Abs(remaining) < (float)rest && double.Abs(rate) < (float)rest ? None : Some((1d - remaining, rate));
    }

    // --- [CASES]

    public sealed record Instant : TimingCurve {
        internal override Option<(double Progress, double Rate)> At(Duration elapsed) => None;
    }

    public sealed record Eased(Duration Length, Easing Easing) : TimingCurve {
        internal override Option<(double Progress, double Rate)> At(Duration elapsed) =>
            Fraction.Validated((float)(elapsed / Length)).ToOption().Filter(_ => elapsed < Length).Bind(Easing.At).Map(sampled => (sampled.Progress, sampled.Slope / Length.TotalSeconds));
    }

    public sealed record Spring(Duration Response, Factor Damping, Factor Rest) : TimingCurve {
        public double Initial { get; init; }

        private double Frequency => double.Tau / Response.TotalSeconds;

        internal override Option<(double Progress, double Rate)> At(Duration elapsed) =>
            Oscillated((float)Damping, double.Tau * (elapsed / Response), Initial / Frequency, Rest).Map(moved => (moved.Progress, Frequency * moved.Rate));

        internal override TimingCurve WithVelocity(double velocity) => this with { Initial = velocity };
    }

    public sealed record Decay(Duration TimeConstant, Factor Rest) : TimingCurve {
        private readonly Option<double> initial;

        public double Initial { get => initial.IfNone(1d / TimeConstant.TotalSeconds); init => initial = value; }

        internal override Option<(double Progress, double Rate)> At(Duration elapsed) =>
            Oscillated(1d, elapsed / TimeConstant, Initial * TimeConstant.TotalSeconds, Rest).Map(moved => (moved.Progress, moved.Rate / TimeConstant.TotalSeconds));

        internal override TimingCurve WithVelocity(double velocity) => this with { Initial = velocity };
    }
}

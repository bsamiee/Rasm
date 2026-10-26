using System.Globalization;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<Invalid>]
public sealed partial class DampingRatio {
    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref double value) =>
        validationError = double.IsFinite(value) && value > 0d ? null : new Invalid(string.Create(CultureInfo.InvariantCulture, $"damping ratio {value} is not finite and positive"));
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
[ValidationError<Invalid>]
public sealed partial class VisibleChange {
    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref double value) =>
        validationError = value is > 0d and <= 1d ? null : new Invalid(string.Create(CultureInfo.InvariantCulture, $"visible change {value} lies outside (0, 1]"));
}

[ComplexValueObject]
[ValidationError<Invalid>]
public sealed partial class Easing {
    private const int Bisections = 32;

    public static Easing Linear { get; } = Create(0d, 0d, 1d, 1d);
    public static Easing Ease { get; } = Create(0.25d, 0.1d, 0.25d, 1d);
    public static Easing EaseIn { get; } = Create(0.42d, 0d, 1d, 1d);
    public static Easing EaseOut { get; } = Create(0d, 0d, 0.58d, 1d);
    public static Easing EaseInOut { get; } = Create(0.42d, 0d, 0.58d, 1d);

    public double X1 { get; }
    public double Y1 { get; }
    public double X2 { get; }
    public double Y2 { get; }

    public double Progress(double time) => Bezier(Solved(time, 0d, 1d, Bisections), Y1, Y2);

    public double Slope(double time) => Solved(time, 0d, 1d, Bisections) switch {
        var solved => Tangent(solved, Y1, Y2) / Tangent(solved, X1, X2),
    };

    private double Solved(double time, double low, double high, int steps) =>
        ((low + high) / 2d) switch {
            var middle when steps is 0 => middle,
            var middle when Bezier(middle, X1, X2) < time => Solved(time, middle, high, steps - 1),
            var middle => Solved(time, low, middle, steps - 1),
        };

    private static double Bezier(double s, double first, double second) => (3d * (1d - s) * (1d - s) * s * first) + (3d * (1d - s) * s * s * second) + (s * s * s);

    private static double Tangent(double s, double first, double second) => (3d * (1d - s) * (1d - s) * first) + (6d * (1d - s) * s * (second - first)) + (3d * s * s * (1d - second));

    static partial void ValidateFactoryArguments(ref Invalid? validationError, ref double x1, ref double y1, ref double x2, ref double y2) =>
        validationError = x1 is >= 0d and <= 1d && x2 is >= 0d and <= 1d && double.IsFinite(y1) && double.IsFinite(y2)
            ? null
            : new Invalid(string.Create(CultureInfo.InvariantCulture, $"easing ({x1}, {y1}, {x2}, {y2}) holds an abscissa outside [0, 1] or an unbounded ordinate"));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TimingCurve {
    // --- [SAMPLE]
    public static Reduced<double> End { get; } = Reduced.Done(1d);

    public abstract Reduced<double> Sample(Duration elapsed);

    public abstract double Velocity(Duration elapsed);

    public virtual TimingCurve From(double velocity) => this;

    private static Reduced<(double Progress, double Rate)> Oscillated(double damping, double phase, double start, VisibleChange rest) =>
        Modes(damping, phase) switch {
            var (even, odd) => (Remaining: even + ((damping - start) * odd), Rate: (start * even) + ((1d - (damping * start)) * odd)) switch {
                var moved when double.Abs(moved.Remaining) < rest && double.Abs(moved.Rate) < rest => Reduced.Done((1d, 0d)),
                var moved => Reduced.Continue((1d - moved.Remaining, moved.Rate)),
            },
        };

    private static (double Even, double Odd) Modes(double damping, double phase) =>
        (damping - 1d) switch {
            < 0d => (Root: double.Sqrt((1d - damping) * (1d + damping)), Decay: double.Exp(-damping * phase)) switch {
                var under => (under.Decay * double.Cos(under.Root * phase), under.Decay * double.Sin(under.Root * phase) / under.Root),
            },
            > 0d => double.Sqrt((damping - 1d) * (damping + 1d)) switch {
                var root => (Slow: double.Exp(-phase / (damping + root)), Gap: double.ExpM1(-2d * root * phase)) switch {
                    var over => (0.5d * over.Slow * (2d + over.Gap), -0.5d * over.Slow * over.Gap / root),
                },
            },
            _ => double.Exp(-phase) switch {
                var decay => (decay, phase * decay),
            },
        };

    // --- [CASES]
    public sealed record Instant : TimingCurve {
        public override Reduced<double> Sample(Duration elapsed) => End;

        public override double Velocity(Duration elapsed) => 0d;
    }

    public sealed record Eased(Duration Length, Easing Easing) : TimingCurve {
        public override Reduced<double> Sample(Duration elapsed) => elapsed >= Length ? End : Reduced.Continue(Easing.Progress(elapsed / Length));

        public override double Velocity(Duration elapsed) => elapsed >= Length ? 0d : Easing.Slope(elapsed / Length) / Length.TotalSeconds;
    }

    public sealed record Spring(Duration Response, DampingRatio Damping, VisibleChange Rest) : TimingCurve {
        public double Initial { get; init; }

        private double Frequency => double.Tau / Response.TotalSeconds;

        public override Reduced<double> Sample(Duration elapsed) => Moved(elapsed).Map(static moved => moved.Progress);

        public override double Velocity(Duration elapsed) => Frequency * Moved(elapsed).Value.Rate;

        public override TimingCurve From(double velocity) => this with { Initial = velocity };

        private Reduced<(double Progress, double Rate)> Moved(Duration elapsed) => Oscillated((double)Damping, double.Tau * (elapsed / Response), Initial / Frequency, Rest);
    }

    public sealed record Glide(Duration TimeConstant, VisibleChange Rest) : TimingCurve {
        public double Initial { get; init; } = 1d / TimeConstant.TotalSeconds;

        public override Reduced<double> Sample(Duration elapsed) => Moved(elapsed).Map(static moved => moved.Progress);

        public override double Velocity(Duration elapsed) => Moved(elapsed).Value.Rate / TimeConstant.TotalSeconds;

        public override TimingCurve From(double velocity) => this with { Initial = velocity };

        private Reduced<(double Progress, double Rate)> Moved(Duration elapsed) => Oscillated(1d, elapsed / TimeConstant, Initial * TimeConstant.TotalSeconds, Rest);
    }
}

public sealed record Animated(float From, float To, Duration Start, TimingCurve Curve) {
    public Reduced<float> At(Duration now) => Curve.Sample(now - Start).Map(progress => float.Lerp(From, To, (float)progress));

    public Animated Toward(float target, Duration now, TimingCurve curve) =>
        To.CompareTo(target) is 0
            ? this
            : At(now).Value switch {
                var at => new(at, target, now, curve.From(at.CompareTo(target) is 0 ? 0d : Curve.Velocity(now - Start) * (To - From) / (target - at))),
            };
}

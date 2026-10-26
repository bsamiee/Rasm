using System.Globalization;
using System.Text.RegularExpressions;

namespace Rasm.UI;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<Duration>(AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct MotionSpan : System.Numerics.IMinMaxValue<MotionSpan> {
    public static MotionSpan MinValue { get; } = new(Duration.Epsilon);
    public static MotionSpan MaxValue { get; } = new(Duration.FromMilliseconds(int.MaxValue));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Duration value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DampingRatio : System.Numerics.IMinMaxValue<DampingRatio> {
    public static DampingRatio MinValue { get; } = new(double.BitIncrement(0d));
    public static DampingRatio MaxValue { get; } = new(double.Sqrt(double.MaxValue));

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct VisibleChange : System.Numerics.IMinMaxValue<VisibleChange> {
    public static VisibleChange MinValue { get; } = new(double.BitIncrement(0d));
    public static VisibleChange MaxValue { get; } = new(1d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) => validationError = ValidationResults.OutsideRange(value, MinValue, MaxValue);
}

[ComplexValueObject]
public sealed partial class Easing {
    private const int Bisections = 32;

    public static Easing Linear { get; } = Create(0d, 0d, 1d, 1d);

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

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double x1, ref double y1, ref double x2, ref double y2) =>
        validationError = ValidationResults.OutsideRange(x1, 0d, 1d) ?? ValidationResults.OutsideRange(x2, 0d, 1d)
            ?? (double.IsFinite(y1) && double.IsFinite(y2) ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"easing ordinates {y1} and {y2} are not finite")));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TimingForm {
    public static readonly TimingForm Linear = new("linear", Easing.Linear);
    public static readonly TimingForm Ease = new("ease", Easing.Create(0.25d, 0.1d, 0.25d, 1d));
    public static readonly TimingForm EaseIn = new("ease-in", Easing.Create(0.42d, 0d, 1d, 1d));
    public static readonly TimingForm EaseOut = new("ease-out", Easing.Create(0d, 0d, 0.58d, 1d));
    public static readonly TimingForm EaseInOut = new("ease-in-out", Easing.Create(0.42d, 0d, 0.58d, 1d));
    public static readonly TimingForm CubicBezier = new("cubic-bezier", None, 4, static (span, points) =>
        ValidationResults.ToFin(Easing.Validate(points[0], points[1], points[2], points[3], out Easing? easing), easing).Map(easing => (TimingCurve)new TimingCurve.Eased(span, easing)));
    public static readonly TimingForm Spring = new("spring", None, 2, static (span, points) =>
        (DampingRatio.Validated(points[0]), VisibleChange.Validated(points[1])).Apply((damping, rest) => (TimingCurve)new TimingCurve.Spring(span, damping, rest)).As());
    public static readonly TimingForm Glide = new("glide", None, 1, static (span, points) =>
        VisibleChange.Validated(points[0]).Map(rest => (TimingCurve)new TimingCurve.Glide(span, rest)));

    private TimingForm(string key, Easing curve) : this(key, curve, 0, (span, _) => new TimingCurve.Eased(span, curve)) { }

    public Option<Easing> Curve { get; }
    public int Arity { get; }

    [UseDelegateFromConstructor]
    public partial Fin<TimingCurve> Make(MotionSpan span, Seq<double> points);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
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

    // --- [WIRE]
    private const string Instantly = "instant";

    private protected abstract Option<(TimingForm Form, Seq<double> Points, MotionSpan Span)> Wire { get; }

    [GeneratedRegex(@"^(?<form>[a-z]+(?:-[a-z]+)*)(?:\((?<points>[^)]*)\))?\s+(?<time>[-+0-9.eE]+)(?<milli>m)?s\z", RegexOptions.CultureInvariant)]
    private static partial Regex Grammar { get; }

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out TimingCurve? item) {
        (item, ValidationError? fault) = value is null
            ? (null, null)
            : Read(value).Match<(TimingCurve?, ValidationError?)>(Succ: static curve => (curve, null), Fail: static failure => (null, new ValidationError(failure.Message)));
        return fault;
    }

    public string ToValue() =>
        Wire.Match(
            Some: static wire => string.Create(CultureInfo.InvariantCulture,
                $"{wire.Form.Key}{(wire.Points.IsEmpty ? "" : $"({string.Join(", ", wire.Points.Map(static point => point.ToString("R", CultureInfo.InvariantCulture)))})")} {((Duration)wire.Span).TotalMilliseconds:R}ms"),
            None: static () => Instantly);

    private static Fin<TimingCurve> Read(string text) =>
        string.Equals(text, Instantly, StringComparison.Ordinal)
            ? new Instant()
            : Grammar.Match(text) switch {
                { Success: false } => Error.New(ErrorCodes.ValidationFailed, $"'{text}' is neither {Instantly} nor <form>[(<points>)] <time>ms or <time>s"),
                var match =>
                    from form in TimingForm.Validated(match.Groups["form"].Value)
                    from points in match.Groups["points"] switch {
                        { Success: true } held => toSeq(held.Value.Split(',', StringSplitOptions.TrimEntries)).Traverse(Number).As(),
                        _ => Seq<double>(),
                    }
                    from _ in guard(points.Count == form.Arity, Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"'{text}' gives {form.Key} {points.Count} points, not {form.Arity}")))
                    from time in Number(match.Groups["time"].Value)
                    from span in MotionSpan.Validated(Duration.FromNanoseconds(long.CreateSaturating(double.Round(time * (match.Groups["milli"].Success ? NodaConstants.NanosecondsPerMillisecond : NodaConstants.NanosecondsPerSecond), MidpointRounding.ToEven))))
                    from curve in form.Make(span, points)
                    select curve,
            };

    private static Fin<double> Number(string text) =>
        Invariant.Number<double>(text).ToFin(Error.New(ErrorCodes.ValidationFailed, $"'{text}' is not a number"));

    // --- [CASES]
    public sealed record Instant : TimingCurve {
        private protected override Option<(TimingForm Form, Seq<double> Points, MotionSpan Span)> Wire => None;

        public override Reduced<double> Sample(Duration elapsed) => End;

        public override double Velocity(Duration elapsed) => 0d;
    }

    public sealed record Eased(MotionSpan Length, Easing Easing) : TimingCurve {
        private protected override Option<(TimingForm Form, Seq<double> Points, MotionSpan Span)> Wire =>
            toSeq(TimingForm.Items).Find(form => form.Curve == Some(Easing)).Match(
                Some: form => (form, Seq<double>(), Length),
                None: () => (TimingForm.CubicBezier, Seq(Easing.X1, Easing.Y1, Easing.X2, Easing.Y2), Length));

        public override Reduced<double> Sample(Duration elapsed) => elapsed >= Length ? End : Reduced.Continue(Easing.Progress(elapsed / Length));

        public override double Velocity(Duration elapsed) => elapsed >= Length ? 0d : Easing.Slope(elapsed / Length) / ((Duration)Length).TotalSeconds;
    }

    public sealed record Spring(MotionSpan Response, DampingRatio Damping, VisibleChange Rest) : TimingCurve {
        public double Initial { get; init; }

        private protected override Option<(TimingForm Form, Seq<double> Points, MotionSpan Span)> Wire => (TimingForm.Spring, Seq((double)Damping, (double)Rest), Response);

        private double Frequency => double.Tau / ((Duration)Response).TotalSeconds;

        public override Reduced<double> Sample(Duration elapsed) => Moved(elapsed).Map(static moved => moved.Progress);

        public override double Velocity(Duration elapsed) => Frequency * Moved(elapsed).Value.Rate;

        public override TimingCurve From(double velocity) => this with { Initial = velocity };

        private Reduced<(double Progress, double Rate)> Moved(Duration elapsed) => Oscillated(Damping, double.Tau * (elapsed / Response), Initial / Frequency, Rest);
    }

    public sealed record Glide(MotionSpan TimeConstant, VisibleChange Rest) : TimingCurve {
        public double Initial { get; init; } = 1d / ((Duration)TimeConstant).TotalSeconds;

        private protected override Option<(TimingForm Form, Seq<double> Points, MotionSpan Span)> Wire => (TimingForm.Glide, Seq((double)Rest), TimeConstant);

        public override Reduced<double> Sample(Duration elapsed) => Moved(elapsed).Map(static moved => moved.Progress);

        public override double Velocity(Duration elapsed) => Moved(elapsed).Value.Rate / ((Duration)TimeConstant).TotalSeconds;

        public override TimingCurve From(double velocity) => this with { Initial = velocity };

        private Reduced<(double Progress, double Rate)> Moved(Duration elapsed) => Oscillated(1d, elapsed / TimeConstant, Initial * ((Duration)TimeConstant).TotalSeconds, Rest);
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

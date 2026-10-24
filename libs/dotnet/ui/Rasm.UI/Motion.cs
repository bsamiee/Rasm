using System.Globalization;

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
    public static DampingRatio MinValue { get; } = new(1d);
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

    public static readonly Easing Linear = new(0d, 0d, 1d, 1d);
    public static readonly Easing In = new(1d / 3d, 0d, 2d / 3d, 0d);
    public static readonly Easing Out = new(1d / 3d, 1d, 2d / 3d, 1d);
    public static readonly Easing InOut = new(0.65d, 0d, 0.35d, 1d);

    public double X1 { get; }
    public double Y1 { get; }
    public double X2 { get; }
    public double Y2 { get; }

    public double Progress(double time) => Bezier(Solved(time, 0d, 1d, Bisections), Y1, Y2);

    private double Solved(double time, double low, double high, int steps) =>
        ((low + high) / 2d) switch {
            var middle when steps is 0 => middle,
            var middle when Bezier(middle, X1, X2) < time => Solved(time, middle, high, steps - 1),
            var middle => Solved(time, low, middle, steps - 1),
        };

    private static double Bezier(double s, double first, double second) => (3d * (1d - s) * (1d - s) * s * first) + (3d * (1d - s) * s * s * second) + (s * s * s);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double x1, ref double y1, ref double x2, ref double y2) =>
        validationError = ValidationResults.OutsideRange(x1, 0d, 1d) ?? ValidationResults.OutsideRange(x2, 0d, 1d)
            ?? (double.IsFinite(y1) && double.IsFinite(y2) ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"easing ordinates {y1} and {y2} are not finite")));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
[ObjectFactory<string>(UseForSerialization = SerializationFrameworks.SystemTextJson)]
public abstract partial record TimingCurve {
    // --- [SAMPLE]
    public static Reduced<double> End { get; } = Reduced.Done(1d);

    public abstract Reduced<double> Sample(Duration elapsed);

    private static Reduced<double> Decayed(double remaining, VisibleChange rest) => remaining < rest ? End : Reduced.Continue(1d - remaining);

    // --- [WIRE]
    private const string Instantly = "instant";
    private const string Milliseconds = "ms";
    private static readonly Seq<(string Name, Easing Easing)> Named = [("linear", Easing.Linear), ("in", Easing.In), ("out", Easing.Out), ("in-out", Easing.InOut)];
    private static readonly Seq<(string Name, int Count, Func<MotionSpan, Seq<double>, Fin<TimingCurve>> Make)> Forms =
        Named.Map(static named => (named.Name, 0, (Func<MotionSpan, Seq<double>, Fin<TimingCurve>>)((span, _) => new Eased(span, named.Easing))))
        + [
            ("cubic-bezier", 4, static (span, points) => ValidationResults.ToFin(Easing.Validate(points[0], points[1], points[2], points[3], out Easing? easing), easing).Map(easing => (TimingCurve)new Eased(span, easing))),
            ("spring", 2, static (span, points) => (DampingRatio.Validated(points[0]), VisibleChange.Validated(points[1])).Apply((damping, rest) => (TimingCurve)new Spring(span, damping, rest)).As()),
            ("glide", 1, static (span, points) => VisibleChange.Validated(points[0]).Map(rest => (TimingCurve)new Glide(span, rest))),
        ];

    public static ValidationError? Validate(string? value, IFormatProvider? provider, out TimingCurve? item) {
        (item, ValidationError? fault) = value is null
            ? (null, null)
            : Read(value).Match<(TimingCurve?, ValidationError?)>(Succ: static curve => (curve, null), Fail: static failure => (null, new ValidationError(failure.Message)));
        return fault;
    }

    public string ToValue() =>
        Switch(
            instant: static _ => Instantly,
            eased: static eased => $"{Named.Find(named => named.Easing == eased.Easing).Map(static named => named.Name).IfNone(() => string.Create(CultureInfo.InvariantCulture, $"cubic-bezier({eased.Easing.X1}, {eased.Easing.Y1}, {eased.Easing.X2}, {eased.Easing.Y2})"))} {Spanned(eased.Length)}",
            spring: static spring => string.Create(CultureInfo.InvariantCulture, $"spring({(double)spring.Damping}, {(double)spring.Rest}) {Spanned(spring.Response)}"),
            glide: static glide => string.Create(CultureInfo.InvariantCulture, $"glide({(double)glide.Rest}) {Spanned(glide.TimeConstant)}"));

    private static Fin<TimingCurve> Read(string text) =>
        string.Equals(text, Instantly, StringComparison.Ordinal)
            ? new Instant()
            : from held in (from gap in Some(text.LastIndexOf(' ')).Filter(static gap => gap > 0)
                            let head = text[..gap].Split('(', 2)
                            from form in Forms.Find(form => string.Equals(form.Name, head[0], StringComparison.Ordinal))
                            from points in Points(head)
                            where points.Count == form.Count
                            from count in text.EndsWith(Milliseconds, StringComparison.Ordinal) && int.TryParse(text.AsSpan(gap + 1, text.Length - gap - 1 - Milliseconds.Length), NumberStyles.None, CultureInfo.InvariantCulture, out int whole) ? Some(whole) : None
                            select (Form: form, Points: points, Count: count))
                  .ToFin(Error.New(ErrorCodes.ValidationFailed, $"'{text}' is neither {Instantly} nor a curve form followed by a whole span in {Milliseconds}"))
              from span in MotionSpan.Validated(Duration.FromMilliseconds(held.Count))
              from curve in held.Form.Make(span, held.Points)
              select curve;

    private static Option<Seq<double>> Points(string[] head) =>
        head.Length is 1
            ? Some(Seq<double>())
            : head[1].EndsWith(')') ? toSeq(head[1][..^1].Split(',', StringSplitOptions.TrimEntries)).Traverse(static part => double.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) ? Some(parsed) : None).As() : None;

    private static string Spanned(MotionSpan span) => string.Create(CultureInfo.InvariantCulture, $"{((Duration)span).TotalMilliseconds}{Milliseconds}");

    // --- [CASES]
    public sealed record Instant : TimingCurve {
        public override Reduced<double> Sample(Duration elapsed) => End;
    }

    public sealed record Eased(MotionSpan Length, Easing Easing) : TimingCurve {
        public override Reduced<double> Sample(Duration elapsed) => elapsed >= Length ? End : Reduced.Continue(Easing.Progress(elapsed / Length));
    }

    public sealed record Spring(MotionSpan Response, DampingRatio Damping, VisibleChange Rest) : TimingCurve {
        public override Reduced<double> Sample(Duration elapsed) =>
            Decayed(Remaining(double.Tau * (elapsed / Response), Damping, double.Sqrt((Damping - 1d) * (Damping + 1d))), Rest);

        private static double Remaining(double phase, double damping, double root) =>
            root is 0d
                ? (1d + phase) * double.Exp(-phase)
                : (-2d * root * phase) switch {
                    var gap => 0.5d * double.Exp(-phase / (damping + root)) * (1d + double.Exp(gap) - (damping / root * double.ExpM1(gap))),
                };
    }

    public sealed record Glide(MotionSpan TimeConstant, VisibleChange Rest) : TimingCurve {
        public override Reduced<double> Sample(Duration elapsed) => Decayed(double.Exp(-(elapsed / TimeConstant)), Rest);
    }
}

public sealed record Animated(float From, float To, Duration Start, TimingCurve Curve) {
    public Reduced<float> At(Duration now) => Curve.Sample(now - Start).Map(progress => From + ((To - From) * (float)progress));

    public Animated Toward(float target, Duration now, TimingCurve curve) => To.CompareTo(target) is 0 ? this : new(At(now).Value, target, now, curve);
}

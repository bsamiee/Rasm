using System.Globalization;
using Rasm.UI.Styling;
using Duration = Rasm.UI.Styling.Duration;

namespace Rasm.UI.Motion;

// --- [MODELS] --------------------------------------------------------------------------
[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct DampingRatio {
    public static DampingRatio Critical { get; } = Create(1d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = value >= 1d && double.IsFinite(value) ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"damping ratio {value} is below critical or unbounded"));
}

[ValueObject<double>(SkipIParsable = true, AdditionOperators = OperatorsGeneration.None, SubtractionOperators = OperatorsGeneration.None, MultiplyOperators = OperatorsGeneration.None, DivisionOperators = OperatorsGeneration.None)]
public readonly partial struct VisibleChange {
    public static VisibleChange Pixel { get; } = Create(1d / 512d);

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double value) =>
        validationError = value is > 0d and <= 1d ? null : new ValidationError(string.Create(CultureInfo.InvariantCulture, $"visible change {value} lies outside (0, 1]"));
}

[SmartEnum]
public sealed partial class Easing {
    public static readonly Easing Linear = new(static time => time);
    public static readonly Easing In = new(static time => time * time * time);
    public static readonly Easing Out = new(static time => 1f - float.Pow(1f - time, 3f));
    public static readonly Easing InOut = new(static time => time < 0.5f ? 4f * time * time * time : 1f - (float.Pow((-2f * time) + 2f, 3f) / 2f));

    [UseDelegateFromConstructor]
    public partial float Progress(float time);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TimingCurve {
    public abstract TimeSpan Length { get; }

    public abstract float Progress(TimeSpan elapsed);

    public sealed record Eased(Duration Duration, Easing Easing) : TimingCurve {
        public override TimeSpan Length => Duration.Span;

        public override float Progress(TimeSpan elapsed) => elapsed >= Length ? 1f : Easing.Progress((float)(elapsed / Length));
    }

    public sealed record Spring(TimeSpan Response, DampingRatio Damping, VisibleChange Rest) : TimingCurve {
        private const int Refinements = 24;

        public override TimeSpan Length { get; } = Settled(Response, Damping, Rest);

        public override float Progress(TimeSpan elapsed) => elapsed >= Length ? 1f : (float)(1d - Remaining(Response, Damping, elapsed));

        private static double Remaining(TimeSpan response, DampingRatio damping, TimeSpan elapsed) =>
            (Phase: double.Tau * (elapsed / response), Ratio: (double)damping, Root: double.Sqrt(((double)damping - 1d) * ((double)damping + 1d))) switch {
                var held when held.Root is 0d => (1d + held.Phase) * double.Exp(-held.Phase),
                var held => (-2d * held.Root * held.Phase) switch {
                    var gap => 0.5d * double.Exp(-held.Phase / (held.Ratio + held.Root)) * (1d + double.Exp(gap) - (held.Ratio / held.Root * double.ExpM1(gap))),
                },
            };

        private static TimeSpan Settled(TimeSpan response, DampingRatio damping, VisibleChange rest) =>
            Bisected(response, damping, rest, TimeSpan.Zero, Bracketed(response, damping, rest, response), Refinements);

        private static TimeSpan Bracketed(TimeSpan response, DampingRatio damping, VisibleChange rest, TimeSpan upper) =>
            Remaining(response, damping, upper) < (double)rest ? upper : Bracketed(response, damping, rest, upper * 2d);

        private static TimeSpan Bisected(TimeSpan response, DampingRatio damping, VisibleChange rest, TimeSpan lower, TimeSpan upper, int steps) =>
            steps is 0 ? upper : (lower + ((upper - lower) / 2d)) switch {
                var middle when Remaining(response, damping, middle) < (double)rest => Bisected(response, damping, rest, lower, middle, steps - 1),
                var middle => Bisected(response, damping, rest, middle, upper, steps - 1),
            };
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Settling {
    public Settling And(Settling other) =>
        Switch(
            other,
            settled: static (held, _) => held,
            busy: static (held, mine) => held.Switch(
                mine,
                settled: static (busy, _) => busy,
                busy: static (busy, theirs) => new Busy(busy.Deadline > theirs.Deadline ? busy.Deadline : theirs.Deadline)));

    public sealed record Settled : Settling;

    public sealed record Busy(TimeSpan Deadline) : Settling;
}

public sealed record Animated<T>(T From, T To, TimeSpan Start, TimingCurve Curve, Func<T, T, float, T> Mix) where T : notnull {
    public TimeSpan End => Start + Curve.Length;

    public static Animated<T> Resting(T value, TimingCurve curve, Func<T, T, float, T> mix) => new(value, value, TimeSpan.Zero, curve, mix);

    public T At(TimeSpan now) => now >= End ? To : now <= Start ? From : Mix(From, To, Curve.Progress(now - Start));

    public Settling Status(TimeSpan now) => now >= End ? new Settling.Settled() : new Settling.Busy(End);

    public Animated<T> Chain(T target, TimeSpan now, DisplayOptions display) => Chain(target, now, Curve, display);

    public Animated<T> Chain(T target, TimeSpan now, TimingCurve curve, DisplayOptions display) =>
        EqualityComparer<T>.Default.Equals(To, target)
            ? this
            : new(At(now), target, display.ReduceMotion ? now - curve.Length : now, curve, Mix);
}

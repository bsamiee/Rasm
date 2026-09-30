using System.Numerics;

namespace Rasm.Rhino.Document;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Limits<T> where T : struct, INumber<T> {
    internal Limits(Option<(T Value, bool Exclusive)> lower, Option<(T Value, bool Exclusive)> upper) => (Lower, Upper) = (lower, upper);

    public Option<(T Value, bool Exclusive)> Lower { get; }

    public Option<(T Value, bool Exclusive)> Upper { get; }

    public Limits<T> AtMost(T upper) =>
        new(Lower, Some((upper, false)));

    public Fin<T> Check(T value, string subject) =>
        Check(value, subject, static bound => bound);

    public Fin<T> Check(T value, string subject, Func<T, IFormattable> display) =>
        Violation(value, subject, display).Match(Some: static violation => Fin.Fail<T>(violation), None: () => Fin.Succ(value));

    public ValidationFailure? Violated(T value, string subject) =>
        Violation(value, subject, static bound => bound).ValueUnsafe();

    private Option<ValidationFailure> Violation(T value, string subject, Func<T, IFormattable> display) =>
        !T.IsFinite(value)
            ? new Invalid(subject)
            : Lower.Bind<ValidationFailure>(bound => bound switch {
                (var low, true) when value <= low => new NotGreaterThan(subject, display(low)),
                (var low, false) when value < low => new BelowLowerLimit(subject, display(low)),
                _ => None,
            })
            || Upper.Bind<ValidationFailure>(bound => bound switch {
                (var high, true) when value >= high => new NotLessThan(subject, display(high)),
                (var high, false) when value > high => new AboveUpperLimit(subject, display(high)),
                _ => None,
            });
}

public static class Limits {
    public static Limits<T> AtLeast<T>(T lower) where T : struct, INumber<T> => new(Some((lower, false)), Option<(T, bool)>.None);

    public static Limits<T> Above<T>(T lower) where T : struct, INumber<T> => new(Some((lower, true)), Option<(T, bool)>.None);

    public static Limits<T> AtMost<T>(T upper) where T : struct, INumber<T> => new(Option<(T, bool)>.None, Some((upper, false)));

    public static Limits<T> Below<T>(T upper) where T : struct, INumber<T> => new(Option<(T, bool)>.None, Some((upper, true)));
}

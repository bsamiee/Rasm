using System.Collections.Generic;
using System.Globalization;
using MathNet.Numerics;

namespace Rasm.TestSupport;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Tolerance(double AbsoluteError, double RelativeError, Option<ulong> Ulps = default) {
    public static Tolerance Absolute(double epsilon) => new(epsilon, RelativeError: 0.0);
    public static Tolerance Relative(double epsilon) => new(AbsoluteError: 0.0, epsilon);
    public static Tolerance WithinUlps(ulong units) => new(AbsoluteError: 0.0, RelativeError: 0.0, Some(units));
    public static Tolerance Default { get; } = new(AbsoluteError: 1.0e-12, RelativeError: 1.0e-9);

    public bool Matches(double left, double right) =>
        left.Equals(right)
        || (double.IsFinite(left) && double.IsFinite(right)
            && (Math.Abs(left - right) <= AbsoluteError + (RelativeError * Math.Max(Math.Abs(left), Math.Abs(right)))
                || Ulps.Exists(ulps => left.NumbersBetween(right) <= ulps)));

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"absolute={AbsoluteError:R}, relative={RelativeError:R}, ulps={Ulps}");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class NumericComparison {
    public static bool Elementwise(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance) =>
        left.SequenceEqual(right, EqualityComparer<double>.Create(tolerance.Matches));

    public static bool SignInvariant(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance) =>
        Elementwise(left, right, tolerance) || left.SequenceEqual(right, EqualityComparer<double>.Create((l, r) => tolerance.Matches(l, -r)));

    public static Func<ReadOnlySpan<double>, ReadOnlySpan<double>, Tolerance, bool> Periodic(double period) =>
        double.IsFinite(period) && period > 0d
            ? (left, right, tolerance) => left.SequenceEqual(right, EqualityComparer<double>.Create((l, r) => tolerance.Matches(Math.IEEERemainder(l - r, period), 0d)))
            : throw new ArgumentOutOfRangeException(nameof(period), period, "period must be finite and positive");
}

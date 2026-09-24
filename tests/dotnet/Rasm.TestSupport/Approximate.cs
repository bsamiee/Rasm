using System.Globalization;

namespace Rasm.TestSupport;

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Tolerance(double AbsoluteError, double RelativeError, Option<long> Ulps = default) {
    public static Tolerance Absolute(double epsilon) => new(epsilon, RelativeError: 0.0);
    public static Tolerance Relative(double epsilon) => new(AbsoluteError: 0.0, epsilon);
    public static Tolerance Combined(double absolute, double relative) => new(absolute, relative);
    public static Tolerance WithinUlps(long units) => new(AbsoluteError: 0.0, RelativeError: 0.0, Some(units));
    public static Tolerance Default { get; } = Combined(absolute: 1.0e-12, relative: 1.0e-9);

    public bool Matches(double left, double right) =>
        left.Equals(right)
        || (double.IsFinite(left) && double.IsFinite(right)
            && (Math.Abs(left - right) <= AbsoluteError + (RelativeError * Math.Max(Math.Abs(left), Math.Abs(right)))
                || Ulps.Exists(ulps => UlpDistance(left, right) <= ulps)));

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"absolute={AbsoluteError:R}, relative={RelativeError:R}, ulps={Ulps}");

    private static Int128 UlpDistance(double left, double right) => Int128.Abs((Int128)OrderedBits(left) - OrderedBits(right));

    private static long OrderedBits(double value) {
        long bits = BitConverter.DoubleToInt64Bits(value);
        return bits >= 0L ? bits : long.MinValue - bits;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record NumericComparison {
    public abstract bool Matches(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance);

    private static bool Pairwise(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Func<double, double, bool> close) {
        if (left.Length != right.Length) return false;
        for (int i = 0; i < left.Length; i++)
            if (!close(left[i], right[i])) return false;
        return true;
    }

    public sealed record Elementwise() : NumericComparison {
        public override bool Matches(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance) =>
            Pairwise(left, right, tolerance.Matches);
    }

    public sealed record SignInvariant() : NumericComparison {
        public override bool Matches(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance) =>
            Pairwise(left, right, tolerance.Matches) || Pairwise(left, right, (l, r) => tolerance.Matches(l, -r));
    }

    public sealed record Periodic : NumericComparison {
        public Periodic(double period) =>
            Period = double.IsFinite(period) && period > 0.0 ? period : throw new ArgumentOutOfRangeException(nameof(period), period, "period must be finite and positive");

        public double Period { get; }

        public override bool Matches(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance) =>
            Pairwise(left, right, (l, r) => tolerance.Matches(Math.Abs(Math.IEEERemainder(l - r, Period)), 0.0));

        public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"Periodic({Period:R})");
    }
}

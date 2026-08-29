using System.Globalization;

namespace Rasm.TestSupport;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate bool VectorComparison(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance);

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Tolerance(double AbsoluteError, double RelativeError, long Ulps = 0L) {
    public static Tolerance Absolute(double epsilon) => new(epsilon, RelativeError: 0.0);
    public static Tolerance Relative(double epsilon) => new(AbsoluteError: 0.0, epsilon);
    public static Tolerance Combined(double absolute, double relative) => new(absolute, relative);
    public static Tolerance WithinUlps(long units) => new(AbsoluteError: 0.0, RelativeError: 0.0, units);
    public static Tolerance Default { get; } = Combined(absolute: 1.0e-12, relative: 1.0e-9);

    public bool Matches(double left, double right) =>
        Math.Abs(left - right) <= AbsoluteError + (RelativeError * Math.Max(1.0, Math.Max(Math.Abs(left), Math.Abs(right))))
        || (Ulps > 0L && double.IsFinite(left) && double.IsFinite(right) && UlpDistance(left, right) <= Ulps);

    private static Int128 UlpDistance(double left, double right) => Int128.Abs((Int128)OrderedBits(left) - OrderedBits(right));

    private static long OrderedBits(double value) {
        long bits = BitConverter.DoubleToInt64Bits(value);
        return bits >= 0L ? bits : long.MinValue - bits;
    }
}

public sealed record NumericComparison(string Name, VectorComparison Matches) {
    public static readonly NumericComparison Elementwise = new(nameof(Elementwise), static (left, right, tolerance) => CompareElements(left, right, tolerance, negate: false));
    public static readonly NumericComparison SignInvariant = new(nameof(SignInvariant), static (left, right, tolerance) =>
        CompareElements(left, right, tolerance, negate: false) || CompareElements(left, right, tolerance, negate: true));

    public static NumericComparison Periodic(double period) {
        _ = double.IsFinite(period) && period > 0.0 ? period : throw new ArgumentOutOfRangeException(nameof(period), period, "period must be finite and positive");
        return new NumericComparison(string.Create(CultureInfo.InvariantCulture, $"Periodic({period:R})"), (left, right, tolerance) => {
            if (left.Length != right.Length) {
                return false;
            }
            for (int i = 0; i < left.Length; i++) {
                if (!tolerance.Matches(Math.Abs(Math.IEEERemainder(left[i] - right[i], period)), 0.0)) {
                    return false;
                }
            }
            return true;
        });
    }

    private static bool CompareElements(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance, bool negate) {
        if (left.Length != right.Length) {
            return false;
        }
        for (int i = 0; i < left.Length; i++) {
            if (!tolerance.Matches(left[i], negate ? -right[i] : right[i])) {
                return false;
            }
        }
        return true;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Approximate {
    public static bool Equal(double left, double right, Tolerance tolerance, NumericComparison? comparison = null) =>
        (comparison ?? NumericComparison.Elementwise).Matches([left], [right], tolerance);
    public static bool Equal(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance, NumericComparison? comparison = null) =>
        (comparison ?? NumericComparison.Elementwise).Matches(left, right, tolerance);
    public static bool Equal(Seq<double> left, Seq<double> right, Tolerance tolerance, NumericComparison? comparison = null) =>
        Equal(left.ToArray(), right.ToArray(), tolerance, comparison);
}

// --- [ASSERTIONS]
public static partial class TestAssertions {
    public static void Equal(double left, double right, double tolerance = 1.0e-9, NumericComparison? comparison = null, string? label = null) =>
        Equal(left, right, Tolerance.Absolute(tolerance), comparison, label);
    public static void Equal(double left, double right, Tolerance tolerance, NumericComparison? comparison = null, string? label = null) =>
        True(Approximate.Equal(left, right, tolerance, comparison),
             string.Create(CultureInfo.InvariantCulture, $"{label ?? "Equal"} ({(comparison ?? NumericComparison.Elementwise).Name}): {left:R} vs {right:R} exceed (absolute={tolerance.AbsoluteError:R}, relative={tolerance.RelativeError:R}, ulps={tolerance.Ulps})"));
    public static void Equal(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance, NumericComparison? comparison = null, string? label = null) {
        True(left.Length == right.Length, string.Create(CultureInfo.InvariantCulture, $"{label ?? "Equal"}: length {left.Length} != {right.Length}"));
        True(Approximate.Equal(left, right, tolerance, comparison),
             $"{label ?? "Equal"} ({(comparison ?? NumericComparison.Elementwise).Name}): {Render(left)} vs {Render(right)} exceed tolerance");
    }
    public static void Equal(Seq<double> left, Seq<double> right, Tolerance tolerance, NumericComparison? comparison = null, string? label = null) =>
        Equal(left.ToArray(), right.ToArray(), tolerance, comparison, label);

    private static string Render(ReadOnlySpan<double> values) {
        string head = string.Join(", ", values[..Math.Min(8, values.Length)].ToArray().Select(static x => x.ToString("R", CultureInfo.InvariantCulture)));
        return values.Length > 8 ? $"[{head}, .. {values.Length} total]" : $"[{head}]";
    }
}

using System.Globalization;

namespace Rasm.TestKit;

// --- [TYPES] ---------------------------------------------------------------------------
public delegate bool MetricGate(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance);

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct Tolerance(double Abs, double Rel, long Ulps = 0L) {
    public static Tolerance Absolute(double epsilon) => new(epsilon, Rel: 0.0);
    public static Tolerance Relative(double epsilon) => new(Abs: 0.0, epsilon);
    public static Tolerance Hybrid(double absolute, double relative) => new(absolute, relative);
    public static Tolerance WithinUlps(long units) => new(Abs: 0.0, Rel: 0.0, units);
    public static Tolerance Default { get; } = Hybrid(absolute: 1.0e-12, relative: 1.0e-9);

    public bool Admits(double left, double right) =>
        Math.Abs(left - right) <= Abs + (Rel * Math.Max(1.0, Math.Max(Math.Abs(left), Math.Abs(right))))
        || (Ulps > 0L && double.IsFinite(left) && double.IsFinite(right) && UlpDistance(left, right) <= Ulps);

    private static Int128 UlpDistance(double left, double right) => Int128.Abs((Int128)Lexical(left) - Lexical(right));

    private static long Lexical(double value) {
        long bits = BitConverter.DoubleToInt64Bits(value);
        return bits >= 0L ? bits : long.MinValue - bits;
    }
}

public sealed record Metric(string Name, MetricGate Admits) {
    public static readonly Metric Absolute = new(nameof(Absolute), static (left, right, tolerance) => Elementwise(left, right, tolerance, negate: false));
    public static readonly Metric SignAmbiguous = new(nameof(SignAmbiguous), static (left, right, tolerance) =>
        Elementwise(left, right, tolerance, negate: false) || Elementwise(left, right, tolerance, negate: true));

    public static Metric Periodic(double period) {
        _ = double.IsFinite(period) && period > 0.0 ? period : throw new ArgumentOutOfRangeException(nameof(period), period, "period must be finite and positive");
        return new Metric(string.Create(CultureInfo.InvariantCulture, $"Periodic({period:R})"), (left, right, tolerance) => {
            if (left.Length != right.Length) {
                return false;
            }
            for (int i = 0; i < left.Length; i++) {
                if (!tolerance.Admits(Math.Abs(Math.IEEERemainder(left[i] - right[i], period)), 0.0)) {
                    return false;
                }
            }
            return true;
        });
    }

    private static bool Elementwise(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance, bool negate) {
        if (left.Length != right.Length) {
            return false;
        }
        for (int i = 0; i < left.Length; i++) {
            if (!tolerance.Admits(left[i], negate ? -right[i] : right[i])) {
                return false;
            }
        }
        return true;
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Approx {
    public static bool Equal(double left, double right, Tolerance tolerance, Metric? metric = null) =>
        (metric ?? Metric.Absolute).Admits([left], [right], tolerance);
    public static bool Equal(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance, Metric? metric = null) =>
        (metric ?? Metric.Absolute).Admits(left, right, tolerance);
    public static bool Equal(Seq<double> left, Seq<double> right, Tolerance tolerance, Metric? metric = null) =>
        Equal(left.ToArray(), right.ToArray(), tolerance, metric);
}

// --- [GATES]
public static partial class Spec {
    public static void Equal(double left, double right, double tolerance = 1.0e-9, Metric? metric = null, string? what = null) =>
        Equal(left, right, Tolerance.Absolute(tolerance), metric, what);
    public static void Equal(double left, double right, Tolerance tolerance, Metric? metric = null, string? what = null) =>
        Holds(Approx.Equal(left, right, tolerance, metric),
              string.Create(CultureInfo.InvariantCulture, $"{what ?? "Equal"} ({(metric ?? Metric.Absolute).Name}): {left:R} vs {right:R} diverge beyond (abs={tolerance.Abs:R}, rel={tolerance.Rel:R}, ulps={tolerance.Ulps})"));
    public static void Equal(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance, Metric? metric = null, string? what = null) {
        Holds(left.Length == right.Length, string.Create(CultureInfo.InvariantCulture, $"{what ?? "Equal"}: length {left.Length} != {right.Length}"));
        Holds(Approx.Equal(left, right, tolerance, metric),
              $"{what ?? "Equal"} ({(metric ?? Metric.Absolute).Name}): {Render(left)} vs {Render(right)} diverge beyond tolerance");
    }
    public static void Equal(Seq<double> left, Seq<double> right, Tolerance tolerance, Metric? metric = null, string? what = null) =>
        Equal(left.ToArray(), right.ToArray(), tolerance, metric, what);

    private static string Render(ReadOnlySpan<double> values) {
        string head = string.Join(", ", values[..Math.Min(8, values.Length)].ToArray().Select(static x => x.ToString("R", CultureInfo.InvariantCulture)));
        return values.Length > 8 ? $"[{head}, .. {values.Length} total]" : $"[{head}]";
    }
}

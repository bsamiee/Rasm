using System.Globalization;
using System.Runtime.InteropServices;

namespace Rasm.TestKit;

// --- [TYPES] --------------------------------------------------------------------------------
// One gate delegate: a metric row owns how a pair of sequences is admitted under a tolerance.
public delegate bool MetricGate(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance);

// --- [MODELS] -------------------------------------------------------------------------------
// One `(abs, rel, ulps)` regime covers every tolerance variant; `Ulps` admits bit-adjacent floats
// the magnitude gates reject near cancellation bands, and 0 disables the lane entirely.
[StructLayout(LayoutKind.Auto)]
public readonly record struct Tolerance(double Abs, double Rel, long Ulps = 0L) {
    public static Tolerance Absolute(double epsilon) => new(Abs: epsilon, Rel: 0.0);
    public static Tolerance Relative(double epsilon) => new(Abs: 0.0, Rel: epsilon);
    public static Tolerance Hybrid(double absolute, double relative) => new(Abs: absolute, Rel: relative);
    public static Tolerance WithinUlps(long units) => new(Abs: 0.0, Rel: 0.0, Ulps: units);
    public static Tolerance Default { get; } = Hybrid(absolute: 1.0e-12, relative: 1.0e-9);
    public double Within(double left, double right) =>
        Abs + (Rel * Math.Max(val1: 1.0, val2: Math.Max(val1: Math.Abs(value: left), val2: Math.Abs(value: right))));
    public bool WithinUlpsOf(double left, double right) =>
        Ulps > 0L && double.IsFinite(d: left) && double.IsFinite(d: right) && UlpDistance(left: left, right: right) <= Ulps;
    // Lexicographic bit order: adjacent finite floats differ by exactly 1 and +0.0/-0.0 coincide.
    private static Int128 UlpDistance(double left, double right) =>
        Int128.Abs(value: (Int128)Lexical(value: left) - Lexical(value: right));
    private static long Lexical(double value) {
        long bits = BitConverter.DoubleToInt64Bits(value: value);
        return bits >= 0L ? bits : long.MinValue - bits;
    }
}

// Metric rows are the comparison policy: sign ambiguity (eigenvectors, principal axes) and angular
// wrap (headings, phase, rotation seams) are rows, never sibling methods. NaN admits nothing on
// any row — non-finite equality is always a failure.
public sealed record Metric(string Name, MetricGate Admits) {
    public static readonly Metric Absolute = new(Name: nameof(Absolute), Admits: static (left, right, tolerance) => Elementwise(left: left, right: right, tolerance: tolerance, negate: false));
    public static readonly Metric SignAmbiguous = new(Name: nameof(SignAmbiguous), Admits: static (left, right, tolerance) =>
        Elementwise(left: left, right: right, tolerance: tolerance, negate: false) || Elementwise(left: left, right: right, tolerance: tolerance, negate: true));
    // Values equal modulo `period` admit: IEEERemainder folds the delta into [-period/2, period/2]
    // and propagates NaN for non-finite input, so the wrap seam and the NaN ban share one gate.
    public static Metric Periodic(double period) {
        _ = double.IsFinite(d: period) && period > 0.0
            ? period : throw new ArgumentOutOfRangeException(paramName: nameof(period), actualValue: period, message: "period must be finite and positive");
        return new Metric(Name: string.Create(provider: CultureInfo.InvariantCulture, $"Periodic({period:R})"), Admits: (left, right, tolerance) => {
            if (left.Length != right.Length) {
                return false;
            }
            for (int i = 0; i < left.Length; i++) {
                double residual = Math.Abs(value: Math.IEEERemainder(x: left[i] - right[i], y: period));
                if (!(residual <= tolerance.Within(left: left[i], right: right[i]) || tolerance.WithinUlpsOf(left: residual, right: 0.0))) {
                    return false;
                }
            }
            return true;
        });
    }
    // Span traversal is the kernel exemption: elementwise relative scaling cannot ride LINQ.
    private static bool Elementwise(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance, bool negate) {
        if (left.Length != right.Length) {
            return false;
        }
        for (int i = 0; i < left.Length; i++) {
            double mirrored = negate ? -right[i] : right[i];
            if (!(Math.Abs(value: left[i] - mirrored) <= tolerance.Within(left: left[i], right: mirrored) || tolerance.WithinUlpsOf(left: left[i], right: mirrored))) {
                return false;
            }
        }
        return true;
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// One metric-driven oracle: scalar, span, and Seq call shapes over the same row dispatch.
public static class Approx {
    public static bool Equal(double left, double right, Tolerance tolerance, Metric? metric = null) =>
        (metric ?? Metric.Absolute).Admits([left], [right], tolerance);
    public static bool Equal(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance, Metric? metric = null) =>
        (metric ?? Metric.Absolute).Admits(left, right, tolerance);
    public static bool Equal(Seq<double> left, Seq<double> right, Tolerance tolerance, Metric? metric = null) {
        double[] head = [.. left];
        double[] tail = [.. right];
        return Equal(left: head, right: tail, tolerance: tolerance, metric: metric);
    }
}

// --- [GATES]
// The throwing gates live beside their oracle so Approx.cs stays the one equality owner.
public static partial class Spec {
    public static void Equal(double left, double right, double tolerance = 1.0e-9, Metric? metric = null, string? what = null) =>
        Holds(condition: Approx.Equal(left: left, right: right, tolerance: Tolerance.Absolute(epsilon: tolerance), metric: metric),
              label: string.Create(provider: CultureInfo.InvariantCulture, $"{what ?? "Equal"} ({(metric ?? Metric.Absolute).Name}): |{left:R} - {right:R}| = {Math.Abs(value: left - right):R} > {tolerance:R}"));
    public static void Equal(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance, Metric? metric = null, string? what = null) {
        Holds(condition: left.Length == right.Length, label: string.Create(provider: CultureInfo.InvariantCulture, $"{what ?? "Equal"}: length {left.Length} != {right.Length}"));
        Holds(condition: Approx.Equal(left: left, right: right, tolerance: tolerance, metric: metric),
              label: $"{what ?? "Equal"} ({(metric ?? Metric.Absolute).Name}): sequences diverge beyond tolerance");
    }
    public static void Equal(Seq<double> left, Seq<double> right, Tolerance tolerance, Metric? metric = null, string? what = null) {
        double[] head = [.. left];
        double[] tail = [.. right];
        Equal(left: head, right: tail, tolerance: tolerance, metric: metric, what: what);
    }
}

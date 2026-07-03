using System.Globalization;
using System.Runtime.InteropServices;

namespace Rasm.TestKit;

// --- [TYPES] --------------------------------------------------------------------------------
// One gate delegate: a metric row owns how a pair of sequences is admitted under a tolerance.
public delegate bool MetricGate(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance);

// --- [MODELS] -------------------------------------------------------------------------------
// One `(abs, rel)` regime covers every tolerance variant; a union would duplicate equality bodies.
[StructLayout(LayoutKind.Auto)]
public readonly record struct Tolerance(double Abs, double Rel) {
    public static Tolerance Absolute(double epsilon) => new(Abs: epsilon, Rel: 0.0);
    public static Tolerance Relative(double epsilon) => new(Abs: 0.0, Rel: epsilon);
    public static Tolerance Hybrid(double absolute, double relative) => new(Abs: absolute, Rel: relative);
    public static Tolerance Default { get; } = Hybrid(absolute: 1.0e-12, relative: 1.0e-9);
    public double Within(double left, double right) =>
        Abs + (Rel * Math.Max(val1: 1.0, val2: Math.Max(val1: Math.Abs(value: left), val2: Math.Abs(value: right))));
}

// Metric rows are the comparison policy: sign ambiguity (eigenvectors, principal axes) is a row,
// never a sibling method. NaN admits nothing on any row — non-finite equality is always a failure.
public sealed record Metric(string Name, MetricGate Admits) {
    public static readonly Metric Absolute = new(Name: nameof(Absolute), Admits: static (left, right, tolerance) => Elementwise(left: left, right: right, tolerance: tolerance, negate: false));
    public static readonly Metric SignAmbiguous = new(Name: nameof(SignAmbiguous), Admits: static (left, right, tolerance) =>
        Elementwise(left: left, right: right, tolerance: tolerance, negate: false) || Elementwise(left: left, right: right, tolerance: tolerance, negate: true));
    // Span traversal is the kernel exemption: elementwise relative scaling cannot ride LINQ.
    private static bool Elementwise(ReadOnlySpan<double> left, ReadOnlySpan<double> right, Tolerance tolerance, bool negate) {
        if (left.Length != right.Length) {
            return false;
        }
        for (int i = 0; i < left.Length; i++) {
            double mirrored = negate ? -right[i] : right[i];
            if (!(Math.Abs(value: left[i] - mirrored) <= tolerance.Within(left: left[i], right: mirrored))) {
                return false;
            }
        }
        return true;
    }
}

// --- [SERVICES] -----------------------------------------------------------------------------
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

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The throwing gates live beside their oracle so Approx.cs stays the one equality owner.
public static partial class Spec {
    public static void Equal(double left, double right, double tolerance = 1.0e-9, Metric? metric = null, string? what = null) =>
        Holds(condition: Approx.Equal(left: left, right: right, tolerance: Tolerance.Absolute(epsilon: tolerance), metric: metric),
              label: string.Create(provider: CultureInfo.InvariantCulture, $"{what ?? "Equal"}: |{left:R} - {right:R}| = {Math.Abs(value: left - right):R} > {tolerance:R}"));
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

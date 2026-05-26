using System.Runtime.InteropServices;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;
using Complex = System.Numerics.Complex;

namespace Rasm.TestKit;

// --- [MODELS] -------------------------------------------------------------------------------
// Tolerance reduces to (abs + rel*max(1,|l|,|r|)) — Absolute/Relative/Hybrid/Context all degenerate to one
// (abs, rel) tuple; the four cases of the original [Union] had identical bodies, so a record struct is denser.
[StructLayout(LayoutKind.Auto)]
public readonly record struct Tolerance(double Abs, double Rel) {
    public static Tolerance Absolute(double epsilon) => new(Abs: epsilon, Rel: 0.0);
    public static Tolerance Relative(double epsilon) => new(Abs: 0.0, Rel: epsilon);
    public static Tolerance Hybrid(double absolute, double relative) => new(Abs: absolute, Rel: relative);
    public static Tolerance FromContext(Context context) {
        ArgumentNullException.ThrowIfNull(argument: context);
        return new(Abs: context.Absolute.Value, Rel: context.Relative.Value);
    }
    public static Tolerance Default { get; } = Hybrid(absolute: RhinoMath.ZeroTolerance, relative: 1.0e-9);
    public double Within(double left, double right) =>
        Abs + (Rel * Math.Max(val1: 1.0, val2: Math.Max(val1: Math.Abs(value: left), val2: Math.Abs(value: right))));
}

// --- [SERVICES] -----------------------------------------------------------------------------
// Polymorphic equality oracle — magnitude-based for Point3d/Vector3d/Complex, elementwise (Indexed fold) for
// Transform/Arr/Seq/Matrix, recursive for Plane (Origin + 3 axes). Tolerance.Within owns the regime.
public static class Approx {
    public static bool Equal(double left, double right, Tolerance tolerance) =>
        Math.Abs(value: left - right) <= tolerance.Within(left: left, right: right);
    public static bool Equal(Point3d left, Point3d right, Tolerance tolerance) =>
        Equal(left: left.DistanceTo(other: right), right: 0.0, tolerance: tolerance);
    public static bool Equal(Vector3d left, Vector3d right, Tolerance tolerance) =>
        Equal(left: (left - right).Length, right: 0.0, tolerance: tolerance);
    public static bool Equal(Complex left, Complex right, Tolerance tolerance) =>
        Equal(left: (left - right).Magnitude, right: 0.0, tolerance: tolerance);
    public static bool Equal(Plane left, Plane right, Tolerance tolerance) =>
        Equal(left: left.Origin, right: right.Origin, tolerance: tolerance)
        && Equal(left: left.XAxis, right: right.XAxis, tolerance: tolerance)
        && Equal(left: left.YAxis, right: right.YAxis, tolerance: tolerance)
        && Equal(left: left.ZAxis, right: right.ZAxis, tolerance: tolerance);
    public static bool Equal(Transform left, Transform right, Tolerance tolerance) =>
        Indexed(count: 16, get: static (i, t) => t[i / 4, i % 4], left: left, right: right, tolerance: tolerance);
    public static bool Equal(Arr<double> left, Arr<double> right, Tolerance tolerance) =>
        left.Count == right.Count && Indexed(count: left.Count, get: static (i, a) => a[index: i], left: left, right: right, tolerance: tolerance);
    public static bool Equal(Seq<double> left, Seq<double> right, Tolerance tolerance) =>
        left.Count == right.Count && Indexed(count: left.Count, get: static (i, s) => s[index: i], left: left, right: right, tolerance: tolerance);
    public static bool Equal(VectorMatrix left, VectorMatrix right, Tolerance tolerance) =>
        left.Rows.Value == right.Rows.Value && left.Cols.Value == right.Cols.Value && Equal(left: left.Entries, right: right.Entries, tolerance: tolerance);
    // Elementwise fold over [0, count) — single dispatch surface for any T-indexed double sequence.
    private static bool Indexed<T>(int count, Func<int, T, double> get, T left, T right, Tolerance tolerance) =>
        toSeq(Enumerable.Range(start: 0, count: count)).ForAll(i => Equal(left: get(i, left), right: get(i, right), tolerance: tolerance));
}

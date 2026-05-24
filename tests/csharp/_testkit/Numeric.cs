using System.Numerics;
using Rhino;
using Rhino.Geometry;
using VectorMatrix = Rasm.Vectors.Matrix;

namespace Rasm.TestKit;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Numeric {
    public static double Sum(Seq<double> values) =>
        values.Fold(initialState: 0.0, f: static (sum, value) => sum + value);
    public static Point3d Centroid(Seq<Point3d> points) =>
        WeightedCentroid(points: points, weights: toSeq(Enumerable.Repeat(element: 1.0 / points.Count, count: points.Count)));
    private static Point3d WeightedCentroid(Seq<Point3d> points, Seq<double> weights) {
        (double x, double y, double z, double w) = toSeq(Enumerable.Range(start: 0, count: Math.Min(val1: points.Count, val2: weights.Count)))
            .Fold(initialState: (X: 0.0, Y: 0.0, Z: 0.0, W: 0.0), f: (state, i) => (
                X: state.X + (points[index: i].X * weights[index: i]),
                Y: state.Y + (points[index: i].Y * weights[index: i]),
                Z: state.Z + (points[index: i].Z * weights[index: i]),
                W: state.W + weights[index: i]));
        return w > RhinoMath.ZeroTolerance
            ? new Point3d(x: x / w, y: y / w, z: z / w)
            : Point3d.Unset;
    }
    public static Arr<double> CovarianceUpper(Seq<Point3d> points, Option<Seq<double>> weights = default) {
        Seq<double> mass = weights.IfNone(toSeq(Enumerable.Repeat(element: 1.0 / points.Count, count: points.Count)));
        Point3d mean = WeightedCentroid(points: points, weights: mass);
        return new Arr<double>([
            WeightedMoment(points: points, weights: mass, mean: mean, left: static p => p.X, right: static p => p.X),
            WeightedMoment(points: points, weights: mass, mean: mean, left: static p => p.X, right: static p => p.Y),
            WeightedMoment(points: points, weights: mass, mean: mean, left: static p => p.X, right: static p => p.Z),
            WeightedMoment(points: points, weights: mass, mean: mean, left: static p => p.Y, right: static p => p.Y),
            WeightedMoment(points: points, weights: mass, mean: mean, left: static p => p.Y, right: static p => p.Z),
            WeightedMoment(points: points, weights: mass, mean: mean, left: static p => p.Z, right: static p => p.Z),
        ]);
    }
    public static double ArcLength(Seq<Point3d> points) =>
        toSeq(Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: points.Count - 1)))
            .Fold(initialState: 0.0, f: (sum, i) => sum + points[index: i - 1].DistanceTo(other: points[index: i]));
    public static void Entrywise(int rows, int cols, Func<int, int, double> expected, Func<int, int, double> actual, double tolerance, string label) =>
        _ = toSeq(Enumerable.Range(start: 0, count: rows * cols)).Iter(idx => {
            int row = idx / cols, col = idx % cols;
            Spec.EqualWithin(left: actual(row, col), right: expected(row, col), tolerance: tolerance, what: $"{label}[{row},{col}]");
        });
    public static void Symmetric(int dimension, Func<int, int, double> at, double tolerance, string label) =>
        Entrywise(rows: dimension, cols: dimension, expected: (row, col) => at(col, row), actual: at, tolerance: tolerance, label: label);
    public static double Dot(int count, Func<int, double> left, Func<int, double> right) =>
        Enumerable.Range(start: 0, count: count).Sum(i => left(i) * right(i));
    public static Complex DotComplex(int count, Func<int, Complex> left, Func<int, Complex> right) =>
        Enumerable.Range(start: 0, count: count).Aggregate(Complex.Zero, (sum, i) => sum + (Complex.Conjugate(left(i)) * right(i)));
    public static double ProductAt(int width, Func<int, int, double> left, Func<int, int, double> right, int row, int col) =>
        Dot(count: width, left: k => left(row, k), right: k => right(k, col));
    public static void Product(int rows, int width, int cols, Func<int, int, double> left, Func<int, int, double> right, Func<int, int, double> actual, double tolerance, string label) =>
        Entrywise(rows: rows, cols: cols, expected: (row, col) => ProductAt(width: width, left: left, right: right, row: row, col: col), actual: actual, tolerance: tolerance, label: label);
    public static double Determinant(int n, Func<int, int, double> at) {
        ArgumentNullException.ThrowIfNull(at);
        return n switch {
            0 => 1.0,
            1 => at(0, 0),
            2 => (at(0, 0) * at(1, 1)) - (at(0, 1) * at(1, 0)),
            _ => Enumerable.Range(start: 0, count: n).Sum(col =>
                ((col & 1) == 0 ? 1.0 : -1.0) * at(0, col) * Determinant(n: n - 1, at: (row, minorCol) => at(row + 1, minorCol < col ? minorCol : minorCol + 1))),
        };
    }
    public static void ColumnGramIdentity(VectorMatrix matrix, Arr<double>? sigma, double tolerance, string label) =>
        Entrywise(rows: matrix.Cols.Value, cols: matrix.Cols.Value,
            expected: (row, col) => row == col && (sigma is null || row >= sigma.Value.Count || sigma.Value[row] > RhinoMath.ZeroTolerance) ? 1.0 : 0.0,
            actual: (row, col) => Dot(count: matrix.Rows.Value, left: k => matrix.At(i: k, j: row), right: k => matrix.At(i: k, j: col)),
            tolerance: tolerance,
            label: label);
    public static double Norm2(Arr<double> vector) =>
        Math.Sqrt(d: vector.AsIterable().Sum(static value => value * value));
    public static void Residual(VectorMatrix matrix, Arr<double> x, Arr<double> b, double tolerance, string label) =>
        _ = x.Count == matrix.Cols.Value && b.Count == matrix.Rows.Value
            ? toSeq(Enumerable.Range(start: 0, count: matrix.Rows.Value)).Iter(row =>
                Spec.EqualWithin(
                    left: Dot(count: matrix.Cols.Value, left: col => matrix.At(i: row, j: col), right: col => x[index: col]),
                    right: b[index: row],
                    tolerance: tolerance,
                    what: $"{label}[{row}]"))
            : throw new Xunit.Sdk.XunitException($"{label}: residual shape mismatch A={matrix.Rows.Value}x{matrix.Cols.Value}, x={x.Count}, b={b.Count}");
    public static void Eigenpair(VectorMatrix matrix, double eigenvalue, Arr<double> eigenvector, Func<double, double, bool> eq, string label) {
        Assert.Equal(expected: matrix.Cols.Value, actual: eigenvector.Count);
        _ = toSeq(Enumerable.Range(start: 0, count: matrix.Rows.Value)).Iter(row => {
            double left = Dot(count: matrix.Cols.Value, left: col => matrix.At(i: row, j: col), right: col => eigenvector[index: col]);
            double right = eigenvalue * eigenvector[index: row];
            Spec.Holds(condition: (eq ?? throw new ArgumentNullException(nameof(eq)))(left, right), label: $"{label}[{row}]: {left:R} != {right:R}");
        });
    }
    private static double WeightedMoment(Seq<Point3d> points, Seq<double> weights, Point3d mean, Func<Point3d, double> left, Func<Point3d, double> right) =>
        toSeq(Enumerable.Range(start: 0, count: Math.Min(val1: points.Count, val2: weights.Count)))
            .Fold(initialState: 0.0, f: (sum, i) => {
                Point3d point = points[index: i];
                return sum + (weights[index: i] * (left(point) - left(mean)) * (right(point) - right(mean)));
            });
}

using System.Numerics;
using Rhino;
using Rhino.Geometry;

namespace Rasm.TestKit;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class Numeric {
    public static double Sum(Seq<double> values) =>
        values.Fold(initialState: 0.0, f: static (sum, value) => sum + value);
    public static bool IsTiny(Vector3d value, double tolerance = RhinoMath.ZeroTolerance) =>
        (value.X * value.X) + (value.Y * value.Y) + (value.Z * value.Z) <= tolerance * tolerance;
    public static Point3d Centroid(Seq<Point3d> points) =>
        WeightedCentroid(points: points, weights: toSeq(Enumerable.Repeat(element: 1.0 / points.Count, count: points.Count)));
    private static Point3d WeightedCentroid(Seq<Point3d> points, Seq<double> weights) {
        Assert.Equal(expected: points.Count, actual: weights.Count);
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
        Assert.Equal(expected: points.Count, actual: mass.Count);
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
    // Row-major (Row, Col) projection shared by every matrix-walking oracle.
    private static IEnumerable<(int Row, int Col)> Cells(int rows, int cols) =>
        Enumerable.Range(start: 0, count: rows * cols).Select(idx => (Row: idx / cols, Col: idx % cols));
    public static void Entrywise(int rows, int cols, Func<int, int, double> expected, Func<int, int, double> actual, double tolerance, string label) =>
        _ = toSeq(Cells(rows: rows, cols: cols)).Iter(rc =>
            Spec.Equal(left: actual(rc.Row, rc.Col), right: expected(rc.Row, rc.Col), tolerance: tolerance, what: $"{label}[{rc.Row},{rc.Col}]"));
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
            actual: ColumnGram(matrix: matrix), tolerance: tolerance, label: label);
    public static double Norm2(Arr<double> vector) =>
        Math.Sqrt(d: vector.AsIterable().Sum(static value => value * value));
    public static void Residual(VectorMatrix matrix, Arr<double> x, Arr<double> b, double tolerance, string label) =>
        _ = x.Count == matrix.Cols.Value && b.Count == matrix.Rows.Value
            ? toSeq(Enumerable.Range(start: 0, count: matrix.Rows.Value)).Iter(row =>
                Spec.Equal(
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

    // --- [GEOMETRY_ORACLES] ----------------------------------------------------------------
    public static (double Min, double Mean, double Max) PairwiseDistances(Seq<Point3d> points) {
        double[] distances = [.. Enumerable.Range(start: 0, count: points.Count).SelectMany(i =>
            Enumerable.Range(start: i + 1, count: points.Count - i - 1).Select(j => points[index: i].DistanceTo(other: points[index: j])))];
        return distances.Length == 0 ? (Min: 0.0, Mean: 0.0, Max: 0.0)
            : (Min: distances.Min(), Mean: distances.Average(), Max: distances.Max());
    }
    // V - E + F; full Euler is 2 - 2g - b - h.
    public static int EulerCharacteristic(int vertices, int edges, int faces) => vertices - edges + faces;
    public static double TriangleArea(Point3d a, Point3d b, Point3d c) =>
        0.5 * Vector3d.CrossProduct(a: b - a, b: c - a).Length;
    // Rodrigues axis-angle rotation matrix.
    public static double[][] RotationMatrix(Vector3d axis, double angle) {
        Vector3d k = IsTiny(value: axis) ? Vector3d.ZAxis : axis / axis.Length;
        double c = Math.Cos(d: angle), s = Math.Sin(a: angle), t = 1.0 - c;
        return [
            [c + (t * k.X * k.X),       (t * k.X * k.Y) - (s * k.Z), (t * k.X * k.Z) + (s * k.Y)],
            [(t * k.X * k.Y) + (s * k.Z), c + (t * k.Y * k.Y),       (t * k.Y * k.Z) - (s * k.X)],
            [(t * k.X * k.Z) - (s * k.Y), (t * k.Y * k.Z) + (s * k.X), c + (t * k.Z * k.Z)]];
    }
    public static double AngularDistance(Vector3d a, Vector3d b) =>
        Math.Acos(d: Math.Clamp(value: IsTiny(value: a) || IsTiny(value: b) ? 1.0 : a * b / (a.Length * b.Length), min: -1.0, max: 1.0));

    // --- [MATRIX_ORACLES] -----------------------------------------------------------------
    // Column Gram entry (M^T M)[r,c].
    private static Func<int, int, double> ColumnGram(VectorMatrix matrix) =>
        (row, col) => Dot(count: matrix.Rows.Value, left: k => matrix.At(i: k, j: row), right: k => matrix.At(i: k, j: col));
    // Independent O(n^3) oracle — must not reuse VectorMatrix production multiply.
    public static double[][] Multiply(VectorMatrix left, VectorMatrix right) =>
        [.. Enumerable.Range(start: 0, count: left.Rows.Value).Select(r =>
            Enumerable.Range(start: 0, count: right.Cols.Value).Select(c =>
                ProductAt(width: left.Cols.Value, left: left.At, right: right.At, row: r, col: c)).ToArray())];
    public static double Norm(VectorMatrix matrix, string kind) {
        IEnumerable<int> rows = Enumerable.Range(start: 0, count: matrix.Rows.Value);
        IEnumerable<int> cols = Enumerable.Range(start: 0, count: matrix.Cols.Value);
        return kind switch {
            "MaxAbs" => matrix.Entries.AsIterable().Max(static x => Math.Abs(value: x)),
            "L1" => cols.Max(c => rows.Sum(r => Math.Abs(value: matrix.At(i: r, j: c)))),
            "LInf" => rows.Max(r => cols.Sum(c => Math.Abs(value: matrix.At(i: r, j: c)))),
            _ => throw new ArgumentException(message: $"Norm: unknown kind '{kind}'", paramName: nameof(kind)),
        };
    }
    // Frobenius residual ||left - right||_F.
    public static double FrobeniusDistance(Func<int, int, double> left, Func<int, int, double> right, int rows, int cols) =>
        Math.Sqrt(d: Cells(rows: rows, cols: cols).Sum(rc => { double d = left(rc.Row, rc.Col) - right(rc.Row, rc.Col); return d * d; }));
    public static double OrthogonalityResidual(VectorMatrix matrix) {
        int n = matrix.Cols.Value;
        return FrobeniusDistance(left: ColumnGram(matrix: matrix), right: (r, c) => r == c ? 1.0 : 0.0, rows: n, cols: n);
    }

    // --- [SPECTRAL_ORACLES] ---------------------------------------------------------------
    // Path-graph Laplacian; eigenvalue closed form λ_k = 2 - 2cos(kπ/N).
    public static VectorMatrix PathGraphLaplacian(int n) =>
        VectorMatrix.Of(
            rows: Dim.TryCreate(value: n, obj: out Dim r) ? r : throw new InvalidOperationException(message: "PathGraphLaplacian dim"),
            cols: Dim.TryCreate(value: n, obj: out Dim c) ? c : throw new InvalidOperationException(message: "PathGraphLaplacian dim"),
            entries: new Arr<double>([.. Cells(rows: n, cols: n).Select(rc => (rc.Row, rc.Col) switch {
                var (i, j) when i == j && (i == 0 || i == n - 1) => 1.0,
                var (i, j) when i == j => 2.0,
                var (i, j) when Math.Abs(value: i - j) == 1 => -1.0,
                _ => 0.0,
            })])).Match(Succ: static m => m, Fail: static _ => throw new InvalidOperationException("PathGraphLaplacian invariant"));
    public static double LaplacianRowSum(VectorMatrix L, int row) =>
        Enumerable.Range(start: 0, count: L.Cols.Value).Sum(c => L.At(i: row, j: c));
    // Heat-kernel closed form k(x,y,t) = Σ exp(-λ_i t) φ_i(x) φ_i(y).
    public static double HeatKernel(Arr<double> eigenvalues, Func<int, int, double> eigenvectors, double t, int x, int y) =>
        Enumerable.Range(start: 0, count: eigenvalues.Count).Sum(i => Math.Exp(d: -eigenvalues[index: i] * t) * eigenvectors(i, x) * eigenvectors(i, y));
    public static bool OrthonormalBasisCheck(Vector3d a, Vector3d b, Vector3d c, double tolerance = 1e-9) =>
        Math.Abs(value: a.Length - 1.0) <= tolerance && Math.Abs(value: b.Length - 1.0) <= tolerance && Math.Abs(value: c.Length - 1.0) <= tolerance
        && Math.Abs(value: a * b) <= tolerance && Math.Abs(value: b * c) <= tolerance && Math.Abs(value: a * c) <= tolerance;

    // --- [FIELD_ORACLES] -------------------------------------------------------------------
    public static double SignedDistanceSphere(Point3d p, Point3d center, double radius) =>
        p.DistanceTo(other: center) - radius;
    // Axis-aligned box SDF; half* are half-extents.
    public static double SignedDistanceBox(Point3d p, Point3d center, double halfX, double halfY, double halfZ) {
        Vector3d q = new(x: Math.Abs(p.X - center.X) - halfX, y: Math.Abs(p.Y - center.Y) - halfY, z: Math.Abs(p.Z - center.Z) - halfZ);
        Vector3d clamped = new(x: Math.Max(val1: q.X, val2: 0.0), y: Math.Max(val1: q.Y, val2: 0.0), z: Math.Max(val1: q.Z, val2: 0.0));
        return clamped.Length + Math.Min(val1: Math.Max(val1: q.X, val2: Math.Max(val1: q.Y, val2: q.Z)), val2: 0.0);
    }
    public static Vector3d GradientCentralDifference(Func<Point3d, double> f, Point3d p, double eps) {
        ArgumentNullException.ThrowIfNull(argument: f);
        return new Vector3d(
            x: (f(new Point3d(x: p.X + eps, y: p.Y, z: p.Z)) - f(new Point3d(x: p.X - eps, y: p.Y, z: p.Z))) / (2.0 * eps),
            y: (f(new Point3d(x: p.X, y: p.Y + eps, z: p.Z)) - f(new Point3d(x: p.X, y: p.Y - eps, z: p.Z))) / (2.0 * eps),
            z: (f(new Point3d(x: p.X, y: p.Y, z: p.Z + eps)) - f(new Point3d(x: p.X, y: p.Y, z: p.Z - eps))) / (2.0 * eps));
    }

    // --- [FLOW_ORACLES] --------------------------------------------------------------------
    public static Vector3d AnalyticLinearField(Vector3d a, Vector3d b, Point3d p) =>
        a + new Vector3d(x: b.X * p.X, y: b.Y * p.Y, z: b.Z * p.Z);
    public static double ConvergenceOrder(double coarseError, double fineError, double stepRatio = 2.0) =>
        coarseError <= 0.0 || fineError <= 0.0 ? double.NaN : Math.Log(d: coarseError / fineError) / Math.Log(d: stepRatio);
}

using System.Numerics;

namespace Rasm.TestKit;

// --- [TYPES] --------------------------------------------------------------------------------
// The matrix-norm vocabulary: each row carries its own projector; call sites never re-dispatch
// on a string kind, and a new norm is one row beside these.
[SmartEnum]
public sealed partial class Norm {
    public static readonly Norm MaxAbs = new(static (rows, cols, at) =>
        Numeric.Cells(rows: rows, cols: cols).Max(rc => Math.Abs(value: at(rc.Row, rc.Col))));
    public static readonly Norm L1 = new(static (rows, cols, at) =>
        Enumerable.Range(start: 0, count: cols).Max((int c) => Enumerable.Range(start: 0, count: rows).Sum((int r) => Math.Abs(value: at(r, c)))));
    public static readonly Norm LInf = new(static (rows, cols, at) =>
        Enumerable.Range(start: 0, count: rows).Max((int r) => Enumerable.Range(start: 0, count: cols).Sum((int c) => Math.Abs(value: at(r, c)))));
    public static readonly Norm Frobenius = new(static (rows, cols, at) =>
        Math.Sqrt(d: Numeric.Cells(rows: rows, cols: cols).Sum(rc => at(rc.Row, rc.Col) * at(rc.Row, rc.Col))));

    [UseDelegateFromConstructor]
    public partial double Of(int rows, int cols, Func<int, int, double> at);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
// Independent double/array oracles: every member RETURNS a value — a residual, a moment, a
// closed form — and the caller's Spec gate decides pass or fail. No oracle asserts mid-flight.
public static class Numeric {
    // --- [SCALAR_FOLDS]
    public static double Sum(Seq<double> values) =>
        values.Fold(initialState: 0.0, f: static (sum, value) => sum + value);
    public static double Dot(int count, Func<int, double> left, Func<int, double> right) =>
        Enumerable.Range(start: 0, count: count).Sum(i => left(i) * right(i));
    public static Complex DotComplex(int count, Func<int, Complex> left, Func<int, Complex> right) =>
        Enumerable.Range(start: 0, count: count).Aggregate(Complex.Zero, (sum, i) => sum + (Complex.Conjugate(value: left(i)) * right(i)));
    public static double ConvergenceOrder(double coarseError, double fineError, double stepRatio = 2.0) =>
        coarseError <= 0.0 || fineError <= 0.0 ? double.NaN : Math.Log(d: coarseError / fineError) / Math.Log(d: stepRatio);

    // --- [POINT_MOMENTS]
    // Points are d-dimensional rows; mismatched weights or a vanishing mass return NaN vectors so
    // the calling gate fails loudly instead of the oracle asserting.
    public static double[] Centroid(double[][] points, double[]? weights = null) {
        ArgumentNullException.ThrowIfNull(argument: points);
        ArgumentOutOfRangeException.ThrowIfZero(value: points.Length, paramName: nameof(points));
        int dim = points[0].Length;
        double[] mass = weights ?? [.. Enumerable.Repeat(element: 1.0 / points.Length, count: points.Length)];
        double total = mass.Sum();
        return mass.Length != points.Length || Math.Abs(value: total) <= double.Epsilon
            ? [.. Enumerable.Repeat(element: double.NaN, count: dim)]
            : [.. Enumerable.Range(start: 0, count: dim).Select(axis =>
                Enumerable.Range(start: 0, count: points.Length).Sum(i => points[i][axis] * mass[i]) / total)];
    }
    // Packed upper triangle, row-major: length d(d+1)/2 over the weighted central second moments.
    public static double[] CovarianceUpper(double[][] points, double[]? weights = null) {
        ArgumentNullException.ThrowIfNull(argument: points);
        double[] mean = Centroid(points: points, weights: weights);
        double[] mass = weights ?? [.. Enumerable.Repeat(element: 1.0 / points.Length, count: points.Length)];
        int dim = mean.Length;
        return [.. Enumerable.Range(start: 0, count: dim).SelectMany(row => Enumerable.Range(start: row, count: dim - row).Select(col =>
            Enumerable.Range(start: 0, count: points.Length).Sum(i => mass[i] * (points[i][row] - mean[row]) * (points[i][col] - mean[col]))))];
    }
    public static double ArcLength(double[][] points) {
        ArgumentNullException.ThrowIfNull(argument: points);
        return Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: points.Length - 1)).Sum(i => Distance(left: points[i - 1], right: points[i]));
    }
    public static (double Min, double Mean, double Max) PairwiseDistances(double[][] points) {
        ArgumentNullException.ThrowIfNull(argument: points);
        double[] distances = [.. Enumerable.Range(start: 0, count: points.Length).SelectMany(i =>
            Enumerable.Range(start: i + 1, count: points.Length - i - 1).Select(j => Distance(left: points[i], right: points[j])))];
        return distances.Length == 0 ? (Min: 0.0, Mean: 0.0, Max: 0.0) : (Min: distances.Min(), Mean: distances.Average(), Max: distances.Max());
    }
    // A dimension mismatch is NaN, never a silent shorter-prefix distance.
    public static double Distance(double[] left, double[] right) {
        ArgumentNullException.ThrowIfNull(argument: left);
        ArgumentNullException.ThrowIfNull(argument: right);
        return left.Length != right.Length
            ? double.NaN
            : Math.Sqrt(d: Enumerable.Range(start: 0, count: left.Length).Sum(i => (left[i] - right[i]) * (left[i] - right[i])));
    }

    // --- [GEOMETRY_ORACLES]
    // Shoelace signed area over a closed 2D ring; orientation rides the sign and malformed rows
    // return NaN so the calling gate fails loudly.
    public static double ShoelaceArea(double[][] ring) {
        ArgumentNullException.ThrowIfNull(argument: ring);
        return ring.Any(predicate: static point => point.Length < 2)
            ? double.NaN
            : 0.5 * Enumerable.Range(start: 0, count: ring.Length).Sum(i => {
                (double[] a, double[] b) = (ring[i], ring[(i + 1) % ring.Length]);
                return (a[0] * b[1]) - (b[0] * a[1]);
            });
    }
    // Divergence-theorem signed volume: origin-anchored tetrahedra over the fan triangulation of
    // each face. Watertight outward-oriented meshes conserve the enclosed volume exactly.
    public static double SignedVolume(double[][] vertices, int[][] faces) {
        ArgumentNullException.ThrowIfNull(argument: vertices);
        ArgumentNullException.ThrowIfNull(argument: faces);
        return vertices.Any(predicate: static vertex => vertex.Length < 3)
            || faces.Any(face => face.Length < 3 || face.Any(index => index < 0 || index >= vertices.Length))
            ? double.NaN
            : faces.Sum(face => Enumerable.Range(start: 1, count: face.Length - 2)
                .Sum(k => SignedTetraVolume(a: vertices[face[0]], b: vertices[face[k]], c: vertices[face[k + 1]])));
    }
    public static double SignedTetraVolume(double[] a, double[] b, double[] c) {
        ArgumentNullException.ThrowIfNull(argument: a);
        ArgumentNullException.ThrowIfNull(argument: b);
        ArgumentNullException.ThrowIfNull(argument: c);
        return a.Length < 3 || b.Length < 3 || c.Length < 3
            ? double.NaN
            : ((a[0] * ((b[1] * c[2]) - (b[2] * c[1]))) - (a[1] * ((b[0] * c[2]) - (b[2] * c[0]))) + (a[2] * ((b[0] * c[1]) - (b[1] * c[0])))) / 6.0;
    }
    // Exact orientation over binary64-exact scaled integers: 3 points read as a 2D left-turn, 4 as
    // a 3D above-plane test. No rounding can flip the sign, so adaptive predicates gate against it.
    public static int OrientSign(double[][] simplex) {
        ArgumentNullException.ThrowIfNull(argument: simplex);
        int dim = simplex.Length - 1;
        _ = simplex.Length is 3 or 4 && simplex.All(point => point.Length >= dim && point.Take(count: dim).All(predicate: double.IsFinite))
            ? dim : throw new ArgumentException(message: "OrientSign expects 3 finite 2D points or 4 finite 3D points", paramName: nameof(simplex));
        (BigInteger Mantissa, int Exponent)[][] parts = [.. simplex.Select(point => ((BigInteger Mantissa, int Exponent)[])[.. point.Take(count: dim).Select(Decompose)])];
        int floor = parts.SelectMany(selector: static point => point).Min(selector: static part => part.Exponent);
        BigInteger[][] scaled = [.. parts.Select(point => (BigInteger[])[.. point.Select(part => part.Mantissa << (part.Exponent - floor))])];
        BigInteger[][] edges = [.. scaled.Skip(count: 1).Select(point => (BigInteger[])[.. point.Select((value, axis) => value - scaled[0][axis])])];
        BigInteger determinant = dim == 2
            ? (edges[0][0] * edges[1][1]) - (edges[0][1] * edges[1][0])
            : (edges[0][0] * ((edges[1][1] * edges[2][2]) - (edges[1][2] * edges[2][1])))
                - (edges[0][1] * ((edges[1][0] * edges[2][2]) - (edges[1][2] * edges[2][0])))
                + (edges[0][2] * ((edges[1][0] * edges[2][1]) - (edges[1][1] * edges[2][0])));
        return determinant.Sign;
    }
    // Every finite double is mantissa·2^exponent exactly; subnormals share the 2^-1074 scale.
    private static (BigInteger Mantissa, int Exponent) Decompose(double value) {
        long bits = BitConverter.DoubleToInt64Bits(value: value);
        int exponentBits = (int)((bits >> 52) & 0x7FF);
        long fraction = bits & 0xF_FFFF_FFFF_FFFF;
        BigInteger mantissa = exponentBits == 0 ? fraction : fraction | (1L << 52);
        return (bits < 0L ? -mantissa : mantissa, (exponentBits == 0 ? 1 : exponentBits) - 1075);
    }

    // --- [MATRIX_ORACLES]
    // Row-major cell order is shared by every entrywise residual and projection.
    public static IEnumerable<(int Row, int Col)> Cells(int rows, int cols) =>
        Enumerable.Range(start: 0, count: rows * cols).Select(idx => (Row: idx / cols, Col: idx % cols));
    public static double ProductAt(int width, Func<int, int, double> left, Func<int, int, double> right, int row, int col) =>
        Dot(count: width, left: k => left(row, k), right: k => right(k, col));
    public static double Determinant(int n, Func<int, int, double> at) {
        ArgumentNullException.ThrowIfNull(argument: at);
        return n switch {
            0 => 1.0,
            1 => at(0, 0),
            2 => (at(0, 0) * at(1, 1)) - (at(0, 1) * at(1, 0)),
            _ => Enumerable.Range(start: 0, count: n).Sum((int col) =>
                ((col & 1) == 0 ? 1.0 : -1.0) * at(0, col) * Determinant(n: n - 1, at: (int row, int minorCol) => at(row + 1, minorCol < col ? minorCol : minorCol + 1))),
        };
    }
    public static double EntrywiseResidual(int rows, int cols, Func<int, int, double> expected, Func<int, int, double> actual) =>
        Cells(rows: rows, cols: cols).Max(rc => Math.Abs(value: actual(rc.Row, rc.Col) - expected(rc.Row, rc.Col)));
    public static double SymmetryResidual(int dimension, Func<int, int, double> at) =>
        EntrywiseResidual(rows: dimension, cols: dimension, expected: (row, col) => at(col, row), actual: at);
    public static double ProductResidual(int rows, int width, int cols, Func<int, int, double> left, Func<int, int, double> right, Func<int, int, double> actual) =>
        EntrywiseResidual(rows: rows, cols: cols, expected: (row, col) => ProductAt(width: width, left: left, right: right, row: row, col: col), actual: actual);
    // max_i |(A x - b)_i|; a shape mismatch is NaN so the calling gate fails loudly.
    public static double SolveResidual(int rows, int cols, Func<int, int, double> at, double[] x, double[] b) {
        ArgumentNullException.ThrowIfNull(argument: x);
        ArgumentNullException.ThrowIfNull(argument: b);
        return x.Length != cols || b.Length != rows
            ? double.NaN
            : Enumerable.Range(start: 0, count: rows).Max(row => Math.Abs(value: Dot(count: cols, left: col => at(row, col), right: col => x[col]) - b[row]));
    }
    public static double EigenpairResidual(int n, Func<int, int, double> at, double eigenvalue, double[] eigenvector) {
        ArgumentNullException.ThrowIfNull(argument: eigenvector);
        return eigenvector.Length != n
            ? double.NaN
            : Enumerable.Range(start: 0, count: n).Max(row =>
                Math.Abs(value: Dot(count: n, left: col => at(row, col), right: col => eigenvector[col]) - (eigenvalue * eigenvector[row])));
    }
    public static double FrobeniusDistance(Func<int, int, double> left, Func<int, int, double> right, int rows, int cols) =>
        Math.Sqrt(d: Cells(rows: rows, cols: cols).Sum(rc => { double d = left(rc.Row, rc.Col) - right(rc.Row, rc.Col); return d * d; }));
    public static double OrthogonalityResidual(int rows, int cols, Func<int, int, double> at) =>
        FrobeniusDistance(
            left: (row, col) => Dot(count: rows, left: k => at(k, row), right: k => at(k, col)),
            right: static (row, col) => row == col ? 1.0 : 0.0,
            rows: cols, cols: cols);

    // --- [SPECTRAL_ORACLES]
    // Path-graph Laplacian; closed-form eigenpairs lambda_k = 2 - 2cos(k*pi/n) with
    // phi_k(j) = cos(k*pi*(j + 1/2)/n). A single node has no path structure to gate.
    public static double[][] PathGraphLaplacian(int n) {
        ArgumentOutOfRangeException.ThrowIfLessThan(value: n, other: 2);
        return [.. Enumerable.Range(start: 0, count: n).Select(row => (double[])[.. Enumerable.Range(start: 0, count: n).Select(col => (row, col) switch {
            var (i, j) when i == j && (i == 0 || i == n - 1) => 1.0,
            var (i, j) when i == j => 2.0,
            var (i, j) when Math.Abs(value: i - j) == 1 => -1.0,
            _ => 0.0,
        })])];
    }
    public static double LaplacianRowSum(double[][] laplacian, int row) {
        ArgumentNullException.ThrowIfNull(argument: laplacian);
        return laplacian[row].Sum();
    }
    // Heat-kernel closed form k(x,y,t) = sum_i exp(-lambda_i t) phi_i(x) phi_i(y).
    public static double HeatKernel(double[] eigenvalues, Func<int, int, double> eigenvectors, double t, int x, int y) {
        ArgumentNullException.ThrowIfNull(argument: eigenvalues);
        return Enumerable.Range(start: 0, count: eigenvalues.Length).Sum(i => Math.Exp(d: -eigenvalues[i] * t) * eigenvectors(i, x) * eigenvectors(i, y));
    }

    // --- [TOPOLOGY_ORACLES]
    // V - E + F; full Euler is 2 - 2g - b - h.
    public static int EulerCharacteristic(int vertices, int edges, int faces) => vertices - edges + faces;
}

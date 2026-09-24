using System.Diagnostics;
using System.Numerics;

namespace Rasm.TestSupport;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ReferenceCalculations {
    // --- [SCALAR_OPERATIONS]
    public static double Dot(int count, Func<int, double> left, Func<int, double> right) => Enumerable.Range(0, count).Sum(i => left(i) * right(i));
    public static Complex DotComplex(int count, Func<int, Complex> left, Func<int, Complex> right) =>
        Enumerable.Range(0, count).Aggregate(Complex.Zero, (sum, i) => sum + (Complex.Conjugate(left(i)) * right(i)));
    public static Option<double> ConvergenceOrder(double coarseError, double fineError, double stepRatio = 2.0) {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(stepRatio, 1.0);
        return coarseError > 0.0 && fineError > 0.0 ? Some(Math.Log(coarseError / fineError) / Math.Log(stepRatio)) : None;
    }

    // --- [POINT_MOMENTS]
    public static double[] Centroid(double[][] points, Option<double[]> weights = default) {
        ArgumentOutOfRangeException.ThrowIfZero(points.Length, nameof(points));
        int dim = points[0].Length;
        double[] mass = Mass(points.Length, weights);
        double total = mass.Sum();
        Shape(mass.Length == points.Length, "one weight per point", nameof(weights));
        Shape(Math.Abs(total) > 0.0, "weights sum to zero", nameof(weights));
        Shape(points.All(point => point.Length == dim), "points differ in dimension", nameof(points));
        return [.. Enumerable.Range(0, dim).Select(axis => Enumerable.Range(0, points.Length).Sum(i => points[i][axis] * mass[i]) / total)];
    }
    public static double[] ScatterMatrixUpperTriangle(double[][] points, Option<double[]> weights = default) {
        double[] mass = Mass(points.Length, weights);
        double[] mean = Centroid(points, mass);
        int dim = mean.Length;
        return [.. from row in Enumerable.Range(0, dim)
                   from col in Enumerable.Range(row, dim - row)
                   select Enumerable.Range(0, points.Length).Sum(i => mass[i] * (points[i][row] - mean[row]) * (points[i][col] - mean[col]))];
    }
    public static double ArcLength(double[][] points) => points.Zip(points.Skip(1), Distance).Sum();
    public static (double Min, double Mean, double Max) PairwiseDistances(double[][] points) {
        ArgumentOutOfRangeException.ThrowIfLessThan(points.Length, 2, nameof(points));
        double[] distances = [.. from i in Enumerable.Range(0, points.Length)
                                 from j in Enumerable.Range(i + 1, points.Length - i - 1)
                                 select Distance(points[i], points[j])];
        return (Min: distances.Min(), Mean: distances.Average(), Max: distances.Max());
    }
    public static double Distance(double[] left, double[] right) {
        Shape(left.Length == right.Length, "points differ in dimension", nameof(right));
        return Math.Sqrt(Enumerable.Range(0, left.Length).Sum(i => (left[i] - right[i]) * (left[i] - right[i])));
    }
    private static double[] Mass(int count, Option<double[]> weights) => weights.IfNone(() => [.. Enumerable.Repeat(1.0, count)]);

    // --- [GEOMETRY_CALCULATIONS]
    public static double SignedShoelaceArea(double[][] ring) {
        Shape(ring.All(static point => point.Length >= 2), "every ring point needs two coordinates", nameof(ring));
        return 0.5 * Enumerable.Range(0, ring.Length).Sum(i => {
            (double[] a, double[] b) = (ring[i], ring[(i + 1) % ring.Length]);
            return (a[0] * b[1]) - (b[0] * a[1]);
        });
    }
    public static double SignedVolume(double[][] vertices, int[][] faces) {
        Shape(vertices.All(static vertex => vertex.Length >= 3), "every vertex needs three coordinates", nameof(vertices));
        Shape(faces.All(face => face.Length >= 3 && face.All(index => index >= 0 && index < vertices.Length)), "every face needs three valid vertex indices", nameof(faces));
        return faces.Sum(face => Enumerable.Range(1, face.Length - 2).Sum(k => SignedTetraVolume(vertices[face[0]], vertices[face[k]], vertices[face[k + 1]])));
    }
    public static double SignedTetraVolume(double[] a, double[] b, double[] c) =>
        (a, b, c) switch {
            ([var ax, var ay, var az, ..], [var bx, var by, var bz, ..], [var cx, var cy, var cz, ..]) =>
                ((ax * ((by * cz) - (bz * cy))) - (ay * ((bx * cz) - (bz * cx))) + (az * ((bx * cy) - (by * cx)))) / 6.0,
            _ => throw new ArgumentException("every vertex needs three coordinates", nameof(a)),
        };
    public static int OrientationSign(double[][] simplex) {
        int dim = simplex.Length - 1;
        bool valid = simplex.Length is 3 or 4 && simplex.All(point => point.Length >= dim && point.Take(dim).All(double.IsFinite));
        Shape(valid, "OrientationSign expects 3 finite 2D points or 4 finite 3D points", nameof(simplex));
        (BigInteger Mantissa, int Exponent)[][] parts = [.. simplex.Select(point => ((BigInteger Mantissa, int Exponent)[])[.. point.Take(dim).Select(Decompose)])];
        int floor = parts.SelectMany(static point => point).Min(static part => part.Exponent);
        BigInteger[][] scaled = [.. parts.Select(point => (BigInteger[])[.. point.Select(part => part.Mantissa << (part.Exponent - floor))])];
        BigInteger[][] edges = scaled switch {
            [BigInteger[] origin, .. BigInteger[][] rest] => [.. rest.Select(point => (BigInteger[])[.. point.Select((value, axis) => value - origin[axis])])],
            _ => throw new UnreachableException(),
        };
        BigInteger determinant = edges switch {
            [[var a0, var a1], [var b0, var b1]] => (a0 * b1) - (a1 * b0),
            [[var a0, var a1, var a2], [var b0, var b1, var b2], [var c0, var c1, var c2]] =>
                (a0 * ((b1 * c2) - (b2 * c1))) - (a1 * ((b0 * c2) - (b2 * c0))) + (a2 * ((b0 * c1) - (b1 * c0))),
            _ => throw new UnreachableException(),
        };
        return determinant.Sign;
    }
    private static (BigInteger Mantissa, int Exponent) Decompose(double value) {
        long bits = BitConverter.DoubleToInt64Bits(value);
        int exponentBits = (int)((bits >> 52) & 0x7FF);
        long fraction = bits & 0xF_FFFF_FFFF_FFFF;
        BigInteger mantissa = exponentBits == 0 ? fraction : fraction | (1L << 52);
        return (bits < 0L ? -mantissa : mantissa, (exponentBits == 0 ? 1 : exponentBits) - 1075);
    }

    // --- [MATRIX_CALCULATIONS]
    public static IEnumerable<(int Row, int Col)> MatrixIndices(int rows, int cols) =>
        Enumerable.Range(0, rows * cols).Select(idx => (Row: idx / cols, Col: idx % cols));
    public static double MatrixProductEntry(int width, Func<int, int, double> left, Func<int, int, double> right, int row, int column) =>
        Dot(width, index => left(row, index), index => right(index, column));
    public static double Determinant(int n, Func<int, int, double> at) =>
        n switch {
            0 => 1.0,
            1 => at(0, 0),
            2 => (at(0, 0) * at(1, 1)) - (at(0, 1) * at(1, 0)),
            _ => Enumerable.Range(0, n).Sum(col => ((col & 1) == 0 ? 1.0 : -1.0) * at(0, col) * Determinant(n - 1, (row, minorCol) => at(row + 1, minorCol < col ? minorCol : minorCol + 1))),
        };
    public static double EntrywiseResidual(int rows, int cols, Func<int, int, double> expected, Func<int, int, double> actual) =>
        MatrixNorm.MaxAbsoluteEntry.Evaluate(rows, cols, (row, col) => actual(row, col) - expected(row, col));
    public static double SymmetryResidual(int dimension, Func<int, int, double> at) =>
        EntrywiseResidual(dimension, dimension, (row, col) => at(col, row), at);
    public static double ProductResidual(int rows, int width, int cols, Func<int, int, double> left, Func<int, int, double> right, Func<int, int, double> actual) =>
        EntrywiseResidual(rows, cols, (row, col) => MatrixProductEntry(width, left, right, row, col), actual);
    public static double SolveResidual(int rows, int cols, Func<int, int, double> at, double[] x, double[] b) {
        Shape(x.Length == cols, "one unknown per column", nameof(x));
        Shape(b.Length == rows, "one right-hand value per row", nameof(b));
        return Enumerable.Range(0, rows).Max(row => Math.Abs(Dot(cols, col => at(row, col), col => x[col]) - b[row]));
    }
    public static double EigenpairResidual(int n, Func<int, int, double> at, double eigenvalue, double[] eigenvector) =>
        SolveResidual(n, n, at, eigenvector, [.. eigenvector.Select(entry => eigenvalue * entry)]);
    public static double FrobeniusDistance(int rows, int cols, Func<int, int, double> left, Func<int, int, double> right) =>
        MatrixNorm.Frobenius.Evaluate(rows, cols, (row, col) => left(row, col) - right(row, col));
    public static double OrthogonalityResidual(int rows, int cols, Func<int, int, double> at) =>
        FrobeniusDistance(cols, cols,
            (row, col) => Dot(rows, k => at(k, row), k => at(k, col)),
            static (row, col) => row == col ? 1.0 : 0.0);

    // --- [SPECTRAL_CALCULATIONS]
    public static double[][] PathGraphLaplacian(int n) {
        ArgumentOutOfRangeException.ThrowIfLessThan(n, 2);
        return [.. Enumerable.Range(0, n).Select(row => (double[])[.. Enumerable.Range(0, n).Select(col => (row, col) switch {
            (int i, int j) when i == j && (i == 0 || i == n - 1) => 1.0,
            (int i, int j) when i == j => 2.0,
            (int i, int j) when Math.Abs(i - j) == 1 => -1.0,
            _ => 0.0,
        })])];
    }
    public static double HeatKernel(double[] eigenvalues, Func<int, int, double> eigenvectors, double t, int x, int y) =>
        Enumerable.Range(0, eigenvalues.Length).Sum(i => Math.Exp(-eigenvalues[i] * t) * eigenvectors(i, x) * eigenvectors(i, y));

    // --- [PRECONDITIONS]
    private static void Shape(bool valid, string message, string paramName) {
        if (!valid) throw new ArgumentException(message, paramName);
    }
}

// --- [NORMS] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class MatrixNorm {
    public static readonly MatrixNorm MaxAbsoluteEntry = new(static (rows, columns, at) => ReferenceCalculations.MatrixIndices(rows, columns).Max(index => Math.Abs(at(index.Row, index.Col))));
    public static readonly MatrixNorm L1 = new(static (rows, columns, at) => Enumerable.Range(0, columns).Max(column => Enumerable.Range(0, rows).Sum(row => Math.Abs(at(row, column)))));
    public static readonly MatrixNorm LInfinity = new(static (rows, columns, at) => Enumerable.Range(0, rows).Max(row => Enumerable.Range(0, columns).Sum(column => Math.Abs(at(row, column)))));
    public static readonly MatrixNorm Frobenius = new(static (rows, columns, at) => Math.Sqrt(ReferenceCalculations.MatrixIndices(rows, columns).Sum(index => at(index.Row, index.Col) * at(index.Row, index.Col))));

    [UseDelegateFromConstructor]
    public partial double Evaluate(int rows, int columns, Func<int, int, double> at);

    public override string ToString() =>
        Map(maxAbsoluteEntry: nameof(MaxAbsoluteEntry), l1: nameof(L1), lInfinity: nameof(LInfinity), frobenius: nameof(Frobenius));
}

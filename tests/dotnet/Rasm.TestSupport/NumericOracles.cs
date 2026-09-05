using System.Numerics;

namespace Rasm.TestSupport;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class MatrixNorm {
    public static readonly MatrixNorm MaxAbsoluteEntry = new(static (rows, columns, at) => NumericOracles.MatrixIndices(rows, columns).Max(index => Math.Abs(at(index.Row, index.Col))));
    public static readonly MatrixNorm L1 = new(static (rows, columns, at) => Enumerable.Range(0, columns).Max(column => Enumerable.Range(0, rows).Sum(row => Math.Abs(at(row, column)))));
    public static readonly MatrixNorm LInfinity = new(static (rows, columns, at) => Enumerable.Range(0, rows).Max(row => Enumerable.Range(0, columns).Sum(column => Math.Abs(at(row, column)))));
    public static readonly MatrixNorm Frobenius = new(static (rows, columns, at) => Math.Sqrt(NumericOracles.MatrixIndices(rows, columns).Sum(index => at(index.Row, index.Col) * at(index.Row, index.Col))));

    [UseDelegateFromConstructor]
    public partial double Evaluate(int rows, int columns, Func<int, int, double> at);

    // Keyless [SmartEnum] generates no ToString, the map stays exhaustive
    public override string ToString() =>
        Map(maxAbsoluteEntry: nameof(MaxAbsoluteEntry), l1: nameof(L1), lInfinity: nameof(LInfinity), frobenius: nameof(Frobenius));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
// A shape the oracle cannot evaluate is a defect in the test and throws, a NaN result fails the assertion under another name
public static class NumericOracles {
    // --- [SCALAR_OPERATIONS] -----------------------------------------------------------
    // Seq<double> reaches both the LanguageExt and the LINQ Sum, and a call to either is ambiguous
    public static double Sum(Seq<double> values) => values.Fold(0.0, static (sum, value) => sum + value);
    public static double Dot(int count, Func<int, double> left, Func<int, double> right) => Enumerable.Range(0, count).Sum(i => left(i) * right(i));
    public static Complex DotComplex(int count, Func<int, Complex> left, Func<int, Complex> right) =>
        Enumerable.Range(0, count).Aggregate(Complex.Zero, (sum, i) => sum + (Complex.Conjugate(left(i)) * right(i)));
    // A zero error has no order, and NaN is the IEEE value of the quotient
    public static double ConvergenceOrder(double coarseError, double fineError, double stepRatio = 2.0) {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(stepRatio, 1.0);
        return coarseError <= 0.0 || fineError <= 0.0 ? double.NaN : Math.Log(coarseError / fineError) / Math.Log(stepRatio);
    }

    // --- [POINT_MOMENTS] ---------------------------------------------------------------
    public static double[] Centroid(double[][] points, double[]? weights = null) {
        ArgumentNullException.ThrowIfNull(points);
        ArgumentOutOfRangeException.ThrowIfZero(points.Length, nameof(points));
        int dim = points[0].Length;
        double[] mass = weights ?? Uniform(points.Length);
        double total = mass.Sum();
        Shape(mass.Length == points.Length, "one weight per point", nameof(weights));
        Shape(Math.Abs(total) > 0.0, "weights sum to zero", nameof(weights));
        Shape(points.All(point => point.Length == dim), "points differ in dimension", nameof(points));
        return [.. Enumerable.Range(0, dim).Select(axis => Enumerable.Range(0, points.Length).Sum(i => points[i][axis] * mass[i]) / total)];
    }
    public static double[] ScatterMatrixUpperTriangle(double[][] points, double[]? weights = null) {
        double[] mean = Centroid(points, weights);
        double[] mass = weights ?? Uniform(points.Length);
        int dim = mean.Length;
        return [.. Enumerable.Range(0, dim).SelectMany(row => Enumerable.Range(row, dim - row).Select(col =>
            Enumerable.Range(0, points.Length).Sum(i => mass[i] * (points[i][row] - mean[row]) * (points[i][col] - mean[col]))))];
    }
    public static double ArcLength(double[][] points) {
        ArgumentNullException.ThrowIfNull(points);
        return Enumerable.Range(1, Math.Max(0, points.Length - 1)).Sum(i => Distance(points[i - 1], points[i]));
    }
    public static (double Min, double Mean, double Max) PairwiseDistances(double[][] points) {
        ArgumentNullException.ThrowIfNull(points);
        ArgumentOutOfRangeException.ThrowIfLessThan(points.Length, 2, nameof(points));
        double[] distances = [.. Enumerable.Range(0, points.Length).SelectMany(i =>
            Enumerable.Range(i + 1, points.Length - i - 1).Select(j => Distance(points[i], points[j])))];
        return (Min: distances.Min(), Mean: distances.Average(), Max: distances.Max());
    }
    public static double Distance(double[] left, double[] right) {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);
        Shape(left.Length == right.Length, "points differ in dimension", nameof(right));
        return Math.Sqrt(Enumerable.Range(0, left.Length).Sum(i => (left[i] - right[i]) * (left[i] - right[i])));
    }

    // --- [GEOMETRY_ORACLES] ------------------------------------------------------------
    public static double SignedShoelaceArea(double[][] ring) {
        ArgumentNullException.ThrowIfNull(ring);
        Shape(ring.All(static point => point.Length >= 2), "every ring point needs two coordinates", nameof(ring));
        return 0.5 * Enumerable.Range(0, ring.Length).Sum(i => {
            (double[] a, double[] b) = (ring[i], ring[(i + 1) % ring.Length]);
            return (a[0] * b[1]) - (b[0] * a[1]);
        });
    }
    public static double SignedVolume(double[][] vertices, int[][] faces) {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(faces);
        Shape(vertices.All(static vertex => vertex.Length >= 3), "every vertex needs three coordinates", nameof(vertices));
        Shape(faces.All(face => face.Length >= 3 && face.All(index => index >= 0 && index < vertices.Length)), "every face needs three valid vertex indices", nameof(faces));
        return faces.Sum(face => Enumerable.Range(1, face.Length - 2).Sum(k => SignedTetraVolume(vertices[face[0]], vertices[face[k]], vertices[face[k + 1]])));
    }
    public static double SignedTetraVolume(double[] a, double[] b, double[] c) {
        ArgumentNullException.ThrowIfNull(a);
        ArgumentNullException.ThrowIfNull(b);
        ArgumentNullException.ThrowIfNull(c);
        Shape(a.Length >= 3 && b.Length >= 3 && c.Length >= 3, "every vertex needs three coordinates", nameof(a));
        return ((a[0] * ((b[1] * c[2]) - (b[2] * c[1]))) - (a[1] * ((b[0] * c[2]) - (b[2] * c[0]))) + (a[2] * ((b[0] * c[1]) - (b[1] * c[0])))) / 6.0;
    }
    // Exact through integer arithmetic on the binary expansions, the sign of the double determinant is wrong near collinear input
    public static int OrientationSign(double[][] simplex) {
        ArgumentNullException.ThrowIfNull(simplex);
        int dim = simplex.Length - 1;
        bool valid = simplex.Length is 3 or 4 && simplex.All(point => point.Length >= dim && point.Take(dim).All(double.IsFinite));
        Shape(valid, "OrientationSign expects 3 finite 2D points or 4 finite 3D points", nameof(simplex));
        (BigInteger Mantissa, int Exponent)[][] parts = [.. simplex.Select(point => ((BigInteger Mantissa, int Exponent)[])[.. point.Take(dim).Select(Decompose)])];
        int floor = parts.SelectMany(static point => point).Min(static part => part.Exponent);
        BigInteger[][] scaled = [.. parts.Select(point => (BigInteger[])[.. point.Select(part => part.Mantissa << (part.Exponent - floor))])];
        BigInteger[][] edges = [.. scaled.Skip(1).Select(point => (BigInteger[])[.. point.Select((value, axis) => value - scaled[0][axis])])];
        BigInteger determinant = dim == 2
            ? (edges[0][0] * edges[1][1]) - (edges[0][1] * edges[1][0])
            : (edges[0][0] * ((edges[1][1] * edges[2][2]) - (edges[1][2] * edges[2][1])))
                - (edges[0][1] * ((edges[1][0] * edges[2][2]) - (edges[1][2] * edges[2][0])))
                + (edges[0][2] * ((edges[1][0] * edges[2][1]) - (edges[1][1] * edges[2][0])));
        return determinant.Sign;
    }

    // --- [MATRIX_ORACLES] --------------------------------------------------------------
    public static IEnumerable<(int Row, int Col)> MatrixIndices(int rows, int cols) =>
        Enumerable.Range(0, rows * cols).Select(idx => (Row: idx / cols, Col: idx % cols));
    public static double MatrixProductEntry(int width, Func<int, int, double> left, Func<int, int, double> right, int row, int column) =>
        Dot(width, index => left(row, index), index => right(index, column));
    // Laplace expansion, factorial in the dimension and meant for the small matrices a test holds
    public static double Determinant(int n, Func<int, int, double> at) {
        ArgumentNullException.ThrowIfNull(at);
        return n switch {
            0 => 1.0,
            1 => at(0, 0),
            2 => (at(0, 0) * at(1, 1)) - (at(0, 1) * at(1, 0)),
            _ => Enumerable.Range(0, n).Sum(col => ((col & 1) == 0 ? 1.0 : -1.0) * at(0, col) * Determinant(n - 1, (row, minorCol) => at(row + 1, minorCol < col ? minorCol : minorCol + 1))),
        };
    }
    public static double EntrywiseResidual(int rows, int cols, Func<int, int, double> expected, Func<int, int, double> actual) =>
        MatrixIndices(rows, cols).Max(index => Math.Abs(actual(index.Row, index.Col) - expected(index.Row, index.Col)));
    public static double SymmetryResidual(int dimension, Func<int, int, double> at) =>
        EntrywiseResidual(dimension, dimension, (row, col) => at(col, row), at);
    public static double ProductResidual(int rows, int width, int cols, Func<int, int, double> left, Func<int, int, double> right, Func<int, int, double> actual) =>
        EntrywiseResidual(rows, cols, (row, col) => MatrixProductEntry(width, left, right, row, col), actual);
    public static double SolveResidual(int rows, int cols, Func<int, int, double> at, double[] x, double[] b) {
        ArgumentNullException.ThrowIfNull(x);
        ArgumentNullException.ThrowIfNull(b);
        Shape(x.Length == cols, "one unknown per column", nameof(x));
        Shape(b.Length == rows, "one right-hand value per row", nameof(b));
        return Enumerable.Range(0, rows).Max(row => Math.Abs(Dot(cols, col => at(row, col), col => x[col]) - b[row]));
    }
    public static double EigenpairResidual(int n, Func<int, int, double> at, double eigenvalue, double[] eigenvector) {
        ArgumentNullException.ThrowIfNull(eigenvector);
        Shape(eigenvector.Length == n, "one eigenvector entry per dimension", nameof(eigenvector));
        return Enumerable.Range(0, n).Max(row => Math.Abs(Dot(n, col => at(row, col), col => eigenvector[col]) - (eigenvalue * eigenvector[row])));
    }
    public static double FrobeniusDistance(int rows, int cols, Func<int, int, double> left, Func<int, int, double> right) =>
        Math.Sqrt(MatrixIndices(rows, cols).Sum(index => { double difference = left(index.Row, index.Col) - right(index.Row, index.Col); return difference * difference; }));
    public static double OrthogonalityResidual(int rows, int cols, Func<int, int, double> at) =>
        FrobeniusDistance(cols, cols,
            (row, col) => Dot(rows, k => at(k, row), k => at(k, col)),
            static (row, col) => row == col ? 1.0 : 0.0);

    // --- [SPECTRAL_ORACLES] ------------------------------------------------------------
    public static double[][] PathGraphLaplacian(int n) {
        ArgumentOutOfRangeException.ThrowIfLessThan(n, 2);
        return [.. Enumerable.Range(0, n).Select(row => (double[])[.. Enumerable.Range(0, n).Select(col => (row, col) switch {
            (int i, int j) when i == j && (i == 0 || i == n - 1) => 1.0,
            (int i, int j) when i == j => 2.0,
            (int i, int j) when Math.Abs(i - j) == 1 => -1.0,
            _ => 0.0,
        })])];
    }
    public static double HeatKernel(double[] eigenvalues, Func<int, int, double> eigenvectors, double t, int x, int y) {
        ArgumentNullException.ThrowIfNull(eigenvalues);
        return Enumerable.Range(0, eigenvalues.Length).Sum(i => Math.Exp(-eigenvalues[i] * t) * eigenvectors(i, x) * eigenvectors(i, y));
    }

    // --- [TOPOLOGY_ORACLES] ------------------------------------------------------------
    public static int EulerCharacteristic(int vertices, int edges, int faces) => vertices - edges + faces;

    private static void Shape(bool valid, string message, string paramName) {
        if (!valid) throw new ArgumentException(message, paramName);
    }
    // Unit masses leave the centroid unchanged, because it divides by the total, and make the scatter matrix the unnormalized sum its name states
    private static double[] Uniform(int count) => [.. Enumerable.Repeat(1.0, count)];
    private static (BigInteger Mantissa, int Exponent) Decompose(double value) {
        long bits = BitConverter.DoubleToInt64Bits(value);
        int exponentBits = (int)((bits >> 52) & 0x7FF);
        long fraction = bits & 0xF_FFFF_FFFF_FFFF;
        BigInteger mantissa = exponentBits == 0 ? fraction : fraction | (1L << 52);
        return (bits < 0L ? -mantissa : mantissa, (exponentBits == 0 ? 1 : exponentBits) - 1075);
    }
}

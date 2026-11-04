using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using BigInteger = System.Numerics.BigInteger;

namespace Rasm.TestSupport;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ReferenceCalculations {
    // --- [SCALAR_OPERATIONS]
    public static Option<double> ConvergenceOrder(double coarseError, double fineError, double stepRatio = 2d) {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(stepRatio, 1d);
        if (!(coarseError > 0d && fineError > 0d)) return None;
        if (double.IsPositiveInfinity(coarseError) || double.IsPositiveInfinity(fineError))
            return Some(Math.Log(coarseError / fineError) / Math.Log(stepRatio));
        int coarseExponent = double.ILogB(coarseError);
        int fineExponent = double.ILogB(fineError);
        int difference = coarseExponent - fineExponent;
        int direction = Math.Sign(difference);
        double coarse = double.ScaleB(coarseError, direction - coarseExponent);
        double fine = double.ScaleB(fineError, -fineExponent);
        double logarithm = Math.FusedMultiplyAdd(difference - direction, Constants.Ln2, SpecialFunctions.Log1p((coarse - fine) / fine));
        return Some(logarithm / Math.Log(stepRatio));
    }

    // --- [POINT_MOMENTS]
    public static Vector<double> Centroid(Matrix<double> points, Option<Vector<double>> weights = default) {
        Vector<double> mass = Mass(points.RowCount, weights);
        double total = mass.Sum();
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(double.Abs(total), 0d, nameof(weights));
        return points.TransposeThisAndMultiply(mass) / total;
    }

    public static Matrix<double> Scatter(Matrix<double> points, Option<Vector<double>> weights = default) {
        Vector<double> mass = Mass(points.RowCount, weights);
        Vector<double> mean = Centroid(points, mass);
        Matrix<double> centered = points.MapIndexed((_, axis, value) => value - mean[axis]);
        return centered.TransposeThisAndMultiply(Matrix<double>.Build.DiagonalOfDiagonalVector(mass) * centered);
    }

    public static double ArcLength(Matrix<double> points) =>
        points.EnumerateRows().Zip(points.EnumerateRows().Skip(1), static (a, b) => (b - a).L2Norm()).Sum();

    public static (double Min, double Mean, double Max) PairwiseDistances(Matrix<double> points) {
        ArgumentOutOfRangeException.ThrowIfLessThan(points.RowCount, 2, nameof(points));
        double[] distances = [.. from i in Enumerable.Range(0, points.RowCount)
                                 from j in Enumerable.Range(i + 1, points.RowCount - i - 1)
                                 select (points.Row(i) - points.Row(j)).L2Norm()];
        return (distances.Min(), distances.Average(), distances.Max());
    }

    private static Vector<double> Mass(int count, Option<Vector<double>> weights) =>
        weights.IfNone(() => Vector<double>.Build.Dense(count, 1d));

    // --- [GEOMETRY_CALCULATIONS]
    public static double SignedShoelaceArea(Matrix<double> ring) =>
        0.5d * Enumerable.Range(0, ring.RowCount).Sum(i => {
            int next = (i + 1) % ring.RowCount;
            return (ring[i, 0] * ring[next, 1]) - (ring[next, 0] * ring[i, 1]);
        });

    public static double SignedVolume(Matrix<double> vertices, IReadOnlyList<IReadOnlyList<int>> faces) =>
        faces.Sum(face => face.Count >= 3
            ? Enumerable.Range(1, face.Count - 2).Sum(k => SignedTetraVolume(vertices.Row(face[0]), vertices.Row(face[k]), vertices.Row(face[k + 1])))
            : throw new ArgumentException("every face needs three vertex indices", nameof(faces)));

    public static double SignedTetraVolume(Vector<double> a, Vector<double> b, Vector<double> c) =>
        Matrix<double>.Build.DenseOfRowVectors(new[] { a, b, c }.Select(static point => point.SubVector(0, 3))).Determinant() / 6d;

    public static int OrientationSign(Matrix<double> simplex) {
        int dimension = simplex.RowCount - 1;
        if (simplex.RowCount is not (3 or 4) || simplex.ColumnCount < dimension || !simplex.EnumerateRows().All(point => point.Take(dimension).All(double.IsFinite)))
            throw new ArgumentException("orientation needs three finite 2D points or four finite 3D points", nameof(simplex));
        (BigInteger Mantissa, int Exponent)[][] parts = [.. simplex.EnumerateRows().Select(point => ((BigInteger Mantissa, int Exponent)[])[.. point.Take(dimension).Select(Decompose)])];
        int floor = parts.SelectMany(static point => point).Min(static part => part.Exponent);
        BigInteger[][] scaled = [.. parts.Select(point => (BigInteger[])[.. point.Select(part => part.Mantissa << (part.Exponent - floor))])];
        BigInteger[][] edges = [.. scaled.Skip(1).Select(point => (BigInteger[])[.. point.Select((value, axis) => value - scaled[0][axis])])];
        return (dimension is 2
            ? (edges[0][0] * edges[1][1]) - (edges[0][1] * edges[1][0])
            : (edges[0][0] * ((edges[1][1] * edges[2][2]) - (edges[1][2] * edges[2][1])))
                - (edges[0][1] * ((edges[1][0] * edges[2][2]) - (edges[1][2] * edges[2][0])))
                + (edges[0][2] * ((edges[1][0] * edges[2][1]) - (edges[1][1] * edges[2][0])))).Sign;
    }

    private static (BigInteger Mantissa, int Exponent) Decompose(double value) {
        long bits = BitConverter.DoubleToInt64Bits(value);
        int exponentBits = (int)((bits >> 52) & 0x7FF);
        long fraction = bits & 0xF_FFFF_FFFF_FFFF;
        BigInteger mantissa = exponentBits is 0 ? fraction : fraction | (1L << 52);
        return (bits < 0L ? -mantissa : mantissa, (exponentBits is 0 ? 1 : exponentBits) - 1075);
    }

    // --- [MATRIX_CALCULATIONS]
    public static double EntrywiseResidual(Matrix<double> expected, Matrix<double> actual) =>
        (actual - expected).Enumerate().Max(static value => Math.Abs(value));

    public static double SymmetryResidual(Matrix<double> matrix) => EntrywiseResidual(matrix.Transpose(), matrix);

    public static double ProductResidual(Matrix<double> left, Matrix<double> right, Matrix<double> actual) =>
        EntrywiseResidual(left * right, actual);

    public static double SolveResidual(Matrix<double> matrix, Vector<double> x, Vector<double> b) => ((matrix * x) - b).InfinityNorm();

    public static double EigenpairResidual(Matrix<double> matrix, double eigenvalue, Vector<double> eigenvector) =>
        ((matrix * eigenvector) - (eigenvalue * eigenvector)).InfinityNorm();

    public static double FrobeniusDistance(Matrix<double> left, Matrix<double> right) => (left - right).FrobeniusNorm();

    public static double OrthogonalityResidual(Matrix<double> matrix) =>
        (matrix.TransposeThisAndMultiply(matrix) - Matrix<double>.Build.DenseIdentity(matrix.ColumnCount)).FrobeniusNorm();

    // --- [SPECTRAL_CALCULATIONS]
    public static Matrix<double> PathGraphLaplacian(int count) {
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 2);
        return Matrix<double>.Build.SparseOfIndexed(count, count,
            from row in Enumerable.Range(0, count)
            from column in Enumerable.Range(row - 1, 3)
            where column >= 0 && column < count
            select (row, column, row == column ? row is 0 || row == count - 1 ? 1d : 2d : -1d));
    }

    public static double HeatKernel(Vector<double> eigenvalues, Matrix<double> eigenvectors, double time, int x, int y) =>
        eigenvectors.Row(x).PointwiseMultiply(eigenvectors.Row(y)).DotProduct(eigenvalues.Map(value => Math.Exp(-value * time)));
}

using Rasm.Vectors;
using Rhino;

namespace Rasm.TestKit;

// --- [SERVICES] -----------------------------------------------------------------------------
public static class NumericSpec {
    public static void Entrywise(int rows, int cols, Func<int, int, double> expected, Func<int, int, double> actual, double tolerance, string label) =>
        _ = toSeq(Enumerable.Range(start: 0, count: rows * cols)).Iter(idx => {
            int row = idx / cols, col = idx % cols;
            Spec.EqualWithin(left: actual(row, col), right: expected(row, col), tolerance: tolerance, what: $"{label}[{row},{col}]");
        });
    public static void Symmetric(int dimension, Func<int, int, double> at, double tolerance, string label) =>
        Entrywise(rows: dimension, cols: dimension, expected: (row, col) => at(col, row), actual: at, tolerance: tolerance, label: label);
    public static void GramIdentity(Matrix matrix, Arr<double>? sigma, double tolerance, string label) {
        int n = matrix.Cols.Value;
        Matrix gram = matrix.Transpose() * matrix;
        Entrywise(rows: n, cols: n,
            expected: (row, col) => row == col && (sigma is null || row >= sigma.Value.Count || sigma.Value[row] > RhinoMath.ZeroTolerance) ? 1.0 : 0.0,
            actual: gram.At,
            tolerance: tolerance,
            label: label);
    }
    public static void Eigenpair(Matrix matrix, double eigenvalue, Arr<double> eigenvector, Func<double, double, bool> eq, string label) =>
        _ = toSeq(Enumerable.Range(start: 0, count: matrix.Rows.Value)).Iter(row => {
            double left = Enumerable.Range(start: 0, count: matrix.Cols.Value).Sum(col => matrix.At(i: row, j: col) * eigenvector[index: col]);
            double right = eigenvalue * eigenvector[index: row];
            Spec.Holds(condition: (eq ?? throw new ArgumentNullException(nameof(eq)))(left, right), label: $"{label}[{row}]: {left:R} != {right:R}");
        });
}

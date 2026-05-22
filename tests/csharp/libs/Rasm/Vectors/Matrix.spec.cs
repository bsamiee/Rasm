using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Xunit.Sdk;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// Algebraic SVD laws are oracle-independent: every assertion derives from `(U, Sigma, V)`
// using only matrix multiplication and identity comparison — never recomputing SVD itself.
// This spec is the regression ratchet for the SVD entry-point. Today it pins the one-sided
// Jacobi implementation; tomorrow it pins the Golub-Reinsch replacement.
public static class MatrixGens {
    public static readonly Op Key = Op.Of(name: "matrix-test");
    // Reconstruction U * diag(Sigma) * V^T involves O(n^3) flops; loosen Approx vs the 1e-9
    // baseline to accommodate accumulated roundoff at n up to 6.
    public static readonly Func<double, double, bool> Approx = Gens.Approx(relativeTolerance: 1.0e-7);
    // SvdJacobi assumes m >= n (U emerges as m x n; V as n x n). Generate dims independently
    // then Take exactly Rows*Cols entries from a max-sized buffer — this avoids CsCheck's
    // dimension/length-shrink coupling problem that would otherwise produce count mismatches
    // during shrinking.
    private const int MaxDim = 6;
    public static readonly Gen<Matrix> TallOrSquare =
        Gen.Int[start: 2, finish: MaxDim].Select(
            Gen.Int[start: 2, finish: MaxDim],
            Gen.Double[start: -10.0, finish: 10.0].Array[MaxDim * MaxDim],
            static (int a, int b, double[] buffer) => {
                int rows = Math.Max(val1: a, val2: b);
                int cols = Math.Min(val1: a, val2: b);
                return Matrix.Of(
                    rows: Dimension.Create(value: rows),
                    cols: Dimension.Create(value: cols),
                    entries: [.. buffer.Take(count: rows * cols)],
                    key: Key)
                .Match(
                    Succ: static m => m,
                    Fail: static _ => throw new InvalidOperationException(message: "generator invariant broken: matrix"));
            });
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class SvdLaws {
    [Fact]
    public void ReconstructsAEqualsUSigmaVtransposed() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static input => {
            Fin<SvdResult> result = input.DecomposeSvd(key: MatrixGens.Key);
            Spec.Succ(result: result, then: svd => AssertReconstruction(original: input, svd: svd));
        });

    [Fact]
    public void SigmaSortedDescendingAndNonNegative() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static input => {
            Fin<SvdResult> result = input.DecomposeSvd(key: MatrixGens.Key);
            Spec.Succ(result: result, then: static svd => AssertSigmaSortedNonNegative(sigma: svd.Sigma));
        });

    [Fact]
    public void UColumnsAreOrthonormalOnNonZeroSigma() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static input => {
            Fin<SvdResult> result = input.DecomposeSvd(key: MatrixGens.Key);
            Spec.Succ(result: result, then: static svd => AssertSigmaMaskedOrthogonality(matrix: svd.U, sigma: svd.Sigma, label: "U"));
        });

    [Fact]
    public void VIsFullOrthogonal() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static input => {
            Fin<SvdResult> result = input.DecomposeSvd(key: MatrixGens.Key);
            Spec.Succ(result: result, then: static svd => AssertOrthonormalColumns(matrix: svd.V, label: "V"));
        });

    [Fact]
    public void SigmaTopEqualsSpectralNorm() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static input => {
            Fin<SvdResult> svdResult = input.DecomposeSvd(key: MatrixGens.Key);
            Spec.Succ(result: svdResult, then: svd => {
                Fin<double> spectralResult = input.Spectral(key: MatrixGens.Key);
                Spec.Succ(result: spectralResult, then: spectral =>
                    Spec.EqualWithin(left: spectral, right: svd.Sigma.IsEmpty ? 0.0 : svd.Sigma[0],
                        tolerance: 1.0e-9, what: "spectral norm vs Sigma[0]"));
            });
        });

    // --- [HELPERS] --------------------------------------------------------------------------
    // Build A_reconstructed = U * diag(Sigma) * V^T (m x n) and assert entrywise approximate
    // equality vs the original. diag(Sigma) is k x n where k = Sigma.Count (== V.Cols).
    private static void AssertReconstruction(Matrix original, SvdResult svd) {
        int m = original.Rows.Value;
        int n = original.Cols.Value;
        int k = svd.Sigma.Count;
        Arr<double> sigma = svd.Sigma;
        Matrix sigmaDiag = new(Rows: Dimension.Create(value: k), Cols: Dimension.Create(value: n),
            Entries: [.. Enumerable.Range(start: 0, count: k * n)
                .Select(idx => idx / n == idx % n && idx / n < k ? sigma[idx / n] : 0.0)]);
        Matrix reconstructed = svd.U * sigmaDiag * svd.V.Transpose();
        _ = toSeq(Enumerable.Range(start: 0, count: m * n)).Iter(idx => {
            double a = original.At(i: idx / n, j: idx % n);
            double b = reconstructed.At(i: idx / n, j: idx % n);
            _ = MatrixGens.Approx(a, b)
                ? unit
                : throw new XunitException(userMessage: $"A != U*Sigma*V^T at [{idx / n},{idx % n}]: {a:R} vs {b:R}");
        });
    }

    private static void AssertSigmaSortedNonNegative(Arr<double> sigma) {
        _ = toSeq(Enumerable.Range(start: 0, count: sigma.Count)).Iter(i =>
            _ = sigma[i] >= -1.0e-12
                ? unit
                : throw new XunitException(userMessage: $"negative singular value sigma[{i}]={sigma[i]:R}"));
        _ = toSeq(Enumerable.Range(start: 0, count: sigma.Count - 1)).Iter(i =>
            _ = sigma[i] >= sigma[i + 1] - 1.0e-12
                ? unit
                : throw new XunitException(userMessage: $"sigma not descending at i={i}: {sigma[i]:R} < {sigma[i + 1]:R}"));
    }

    // Orthonormal columns: M^T * M = I_n. Tolerance is loosened from baseline because the
    // Gram product accumulates O(m*eps) error per entry.
    private static void AssertOrthonormalColumns(Matrix matrix, string label) {
        int n = matrix.Cols.Value;
        Matrix gram = matrix.Transpose() * matrix;
        _ = toSeq(Enumerable.Range(start: 0, count: n * n)).Iter(idx => {
            int row = idx / n;
            int col = idx % n;
            double expected = row == col ? 1.0 : 0.0;
            double actual = gram.At(i: row, j: col);
            _ = MatrixGens.Approx(actual, expected)
                ? unit
                : throw new XunitException(userMessage: $"{label}^T*{label} != I at [{row},{col}]: {actual:R} vs {expected:R}");
        });
    }

    // Sigma-masked orthogonality: diagonal entries are 1 where sigma[i] > tol (active column)
    // and 0 where sigma[i] is degenerate (column is zeroed by BuildSvd); off-diagonal entries
    // are always 0. Captures the thin-SVD behaviour for rank-deficient inputs.
    private static void AssertSigmaMaskedOrthogonality(Matrix matrix, Arr<double> sigma, string label) {
        int n = matrix.Cols.Value;
        const double sigmaThreshold = 1.0e-9;
        Matrix gram = matrix.Transpose() * matrix;
        _ = toSeq(Enumerable.Range(start: 0, count: n * n)).Iter(idx => {
            int row = idx / n;
            int col = idx % n;
            double expected = row == col && sigma[row] > sigmaThreshold ? 1.0 : 0.0;
            double actual = gram.At(i: row, j: col);
            _ = MatrixGens.Approx(actual, expected)
                ? unit
                : throw new XunitException(userMessage: $"{label}^T*{label}[{row},{col}]: {actual:R} vs expected {expected:R} (sigma[{row}]={sigma[row]:R})");
        });
    }
}

using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino;
using Xunit.Sdk;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
public static class MatrixGens {
    public static readonly Op Key = Op.Of(name: "matrix-test");
    public static readonly Func<double, double, bool> Approx = Gens.Approx(relativeTolerance: 1.0e-7);
    private const int MaxDim = 6;
    private static Matrix MakeMatrix(int rows, int cols, double[] buffer) =>
        Matrix.Of(rows: Dimension.Create(value: rows), cols: Dimension.Create(value: cols),
                entries: [.. buffer.Take(count: rows * cols)], key: Key)
            .Match(Succ: static m => m, Fail: static _ => throw new InvalidOperationException(message: "matrix"));
    public static readonly Gen<Matrix> TallOrSquare =
        Gen.Int[start: 2, finish: MaxDim].Select(
            Gen.Int[start: 2, finish: MaxDim], Gen.Double[start: -10.0, finish: 10.0].Array[MaxDim * MaxDim],
            static (int a, int b, double[] buf) => MakeMatrix(rows: Math.Max(val1: a, val2: b), cols: Math.Min(val1: a, val2: b), buffer: buf));
    public static readonly Gen<Matrix> Square =
        Gen.Int[start: 2, finish: MaxDim].Select(Gen.Double[start: -10.0, finish: 10.0].Array[MaxDim * MaxDim],
            static (int n, double[] buf) => MakeMatrix(rows: n, cols: n, buffer: buf));
    public static readonly Gen<Matrix> NonSingularSquare = Square.Select(static (Matrix a) => {
        int n = a.Rows.Value;
        return toSeq(Enumerable.Range(start: 0, count: n)).Fold(initialState: a,
            f: (m, i) => m.With(i: i, j: i, value: m.At(i: i, j: i) + (n + 20.0)));
    });
    public static readonly Gen<SymmetricMatrix> Spd = Square.Select(static (Matrix a) => {
        int n = a.Rows.Value;
        Matrix shifted = toSeq(Enumerable.Range(start: 0, count: n)).Fold(initialState: a.Transpose() * a,
            f: (m, i) => m.With(i: i, j: i, value: m.At(i: i, j: i) + n));
        return SymmetricMatrix.Of(dim: Dimension.Create(value: n),
            upper: [.. Enumerable.Range(start: 0, count: n).SelectMany(i =>
                Enumerable.Range(start: i, count: n - i).Select(j => shifted.At(i: i, j: j)))], key: Key)
            .Match(Succ: static s => s, Fail: static _ => throw new InvalidOperationException(message: "spd"));
    });
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class MatrixCoreLaws {
    [Fact]
    public void OfRejectsCountMismatchAndNonFinite() {
        Spec.Fail(Matrix.Of(rows: Dimension.Create(value: 2), cols: Dimension.Create(value: 3), entries: [1.0, 2.0], key: MatrixGens.Key));
        Spec.ForAll(Gen.OneOfConst(double.NaN, double.PositiveInfinity, double.NegativeInfinity), x =>
            Spec.Fail(Matrix.Of(rows: Dimension.Create(value: 2), cols: Dimension.Create(value: 2), entries: [x, 0.0, 0.0, x], key: MatrixGens.Key)));
        Spec.Fail(SymmetricMatrix.Of(dim: Dimension.Create(value: 3), upper: [1.0, 2.0], key: MatrixGens.Key));
    }
    [Fact]
    public void IdentityIsBilateralUnitAndTransposeIsInvolution() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => {
            MatrixOracles.AssertEntrywise(expected: a, actual: Matrix.Identity(dim: a.Rows) * a, label: "I*A");
            MatrixOracles.AssertEntrywise(expected: a, actual: a * Matrix.Identity(dim: a.Cols), label: "A*I");
            MatrixOracles.AssertEntrywise(expected: a, actual: a.Transpose().Transpose(), label: "T(T(A))");
        });
    [Fact]
    public void TraceAndFrobeniusMatchClosedForm() {
        Spec.ForAll(MatrixGens.Square, static a => Spec.Succ(a.Trace(key: MatrixGens.Key), then: t => Spec.EqualWithin(left: t,
            right: toSeq(Enumerable.Range(start: 0, count: a.Rows.Value)).Fold(initialState: 0.0, f: (s, i) => s + a.At(i: i, j: i)),
            tolerance: 1.0e-12, what: "trace")));
        Spec.ForAll(MatrixGens.TallOrSquare, static a =>
            Spec.EqualWithin(left: a.Frobenius,
                right: Math.Sqrt(d: a.Entries.Fold(initialState: 0.0, f: static (s, e) => s + (e * e))),
                tolerance: 1.0e-10, what: "frobenius"));
    }
    [Fact]
    public void NormKindsHaveDistinctKeysAndNonNegativeValues() {
        MatrixNormKind[] all = [MatrixNormKind.Frobenius, MatrixNormKind.MaxAbs, MatrixNormKind.L1, MatrixNormKind.LInf];
        Spec.SmartEnumKeysUnique(items: all, key: static k => k.Key);
        Spec.ForAll(MatrixGens.TallOrSquare, a =>
            _ = toSeq(all).Iter(k => Spec.Succ(a.Norm(kind: k, key: MatrixGens.Key), then: n => Assert.True(n >= 0.0, userMessage: $"{k.Key} norm negative: {n:R}"))));
    }
}

public sealed class SymmetricMatrixLaws {
    [Fact]
    public void ToDenseProducesSymmetricMatrix() =>
        Spec.ForAll(MatrixGens.Spd, static spd => {
            Matrix dense = spd.ToDense();
            int n = spd.Dimension.Value;
            _ = toSeq(Enumerable.Range(start: 0, count: n * n)).Iter(idx => {
                int row = idx / n, col = idx % n;
                _ = MatrixGens.Approx(dense.At(i: row, j: col), dense.At(i: col, j: row))
                    ? unit : throw new XunitException(userMessage: $"asymmetry at [{row},{col}]: {dense.At(i: row, j: col):R} vs {dense.At(i: col, j: row):R}");
            });
        });
}

public sealed class DecompositionLaws {
    [Fact]
    public void SvdReconstructsAEqualsUSigmaVtransposed() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => Spec.Succ(a.DecomposeSvd(key: MatrixGens.Key),
            then: svd => MatrixOracles.AssertEntrywise(expected: a, actual: SvdReconstruct(svd: svd, n: a.Cols.Value), label: "U*Sigma*V^T")));
    [Fact]
    public void SvdSigmaIsSortedNonNegativeAndMatchesSpectral() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => Spec.Succ(a.DecomposeSvd(key: MatrixGens.Key), then: svd => {
            Arr<double> s = svd.Sigma;
            _ = toSeq(Enumerable.Range(start: 0, count: s.Count)).Iter(i =>
                _ = s[i] >= -1.0e-12 && (i == s.Count - 1 || s[i] >= s[i + 1] - 1.0e-12)
                    ? unit : throw new XunitException(userMessage: $"sigma invariant fails at i={i}: {s[i]:R}"));
            Spec.Succ(a.Spectral(key: MatrixGens.Key), then: sp =>
                Spec.EqualWithin(left: sp, right: s.IsEmpty ? 0.0 : s[0], tolerance: 1.0e-9, what: "spectral=Sigma[0]"));
        }));
    [Fact]
    public void SvdFactorsAreOrthogonal() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => Spec.Succ(a.DecomposeSvd(key: MatrixGens.Key), then: svd => {
            MatrixOracles.AssertGram(matrix: svd.V, sigma: null, label: "V");
            MatrixOracles.AssertGram(matrix: svd.U, sigma: null, label: "U");
        }));
    [Fact]
    public void QrReconstructsAndQIsOrthogonal() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => Spec.Succ(a.DecomposeQr(key: MatrixGens.Key), then: qr => {
            MatrixOracles.AssertEntrywise(expected: a, actual: qr.Q * qr.R, label: "Q*R");
            MatrixOracles.AssertGram(matrix: qr.Q, sigma: null, label: "Q");
        }));
    [Fact]
    public void LuResultKeepsSourceAndDeterminant() =>
        Spec.ForAll(MatrixGens.NonSingularSquare, static a => Spec.Succ(a.DecomposeLu(key: MatrixGens.Key), then: lu => {
            Assert.True(condition: lu.IsValid);
            MatrixOracles.AssertEntrywise(expected: a, actual: lu.Source, label: "LU source");
            Spec.Succ(a.Determinant(key: MatrixGens.Key), then: det =>
                Spec.EqualWithin(left: lu.Determinant, right: det, tolerance: 1.0e-9, what: "LU determinant"));
        }));
    [Fact]
    public void CholeskyReconstructsSpd() =>
        Spec.ForAll(MatrixGens.Spd, static spd => Spec.Succ(spd.DecomposeCholesky(key: MatrixGens.Key), then: chol =>
            MatrixOracles.AssertEntrywise(expected: spd.ToDense(), actual: chol.L * chol.L.Transpose(), label: "L*L^T")));
    [Fact]
    public void SymmetricEigenSatisfiesAvEqualsLambdaV() =>
        Spec.ForAll(MatrixGens.Spd, static spd => Spec.Succ(spd.DecomposeEigen(key: MatrixGens.Key), then: eigs => {
            Matrix dense = spd.ToDense();
            int n = spd.Dimension.Value;
            _ = eigs.Iter(pair => {
                Arr<double> av = ProductMv(matrix: dense, vector: pair.Eigenvector);
                _ = toSeq(Enumerable.Range(start: 0, count: n)).Iter(i => {
                    double expected = pair.Eigenvalue * pair.Eigenvector[i];
                    _ = MatrixGens.Approx(av[i], expected)
                        ? unit : throw new XunitException(userMessage: $"A*v != lambda*v at i={i}: {av[i]:R} vs {expected:R}");
                });
            });
        }));
    private static Matrix SvdReconstruct(SvdResult svd, int n) =>
        svd.U * new Matrix(Rows: svd.U.Cols, Cols: Dimension.Create(value: n),
            Entries: [.. Enumerable.Range(start: 0, count: svd.U.Cols.Value * n).Select(idx =>
                idx / n == idx % n && idx / n < svd.Sigma.Count ? svd.Sigma[idx / n] : 0.0)]) * svd.V.Transpose();
    private static Arr<double> ProductMv(Matrix matrix, Arr<double> vector) =>
        [.. Enumerable.Range(start: 0, count: matrix.Rows.Value).Select(i =>
            Enumerable.Range(start: 0, count: matrix.Cols.Value).Sum(j => matrix.At(i: i, j: j) * vector[j]))];
}

internal static class MatrixOracles {
    internal static void AssertEntrywise(Matrix expected, Matrix actual, string label) {
        int n = expected.Cols.Value;
        _ = toSeq(Enumerable.Range(start: 0, count: expected.Rows.Value * n)).Iter(idx => {
            int row = idx / n, col = idx % n;
            _ = MatrixGens.Approx(actual.At(i: row, j: col), expected.At(i: row, j: col))
                ? unit : throw new XunitException(userMessage: $"{label} mismatch [{row},{col}]: {actual.At(i: row, j: col):R} vs {expected.At(i: row, j: col):R}");
        });
    }
    internal static void AssertGram(Matrix matrix, Arr<double>? sigma, string label) {
        int n = matrix.Cols.Value;
        Matrix gram = matrix.Transpose() * matrix;
        _ = toSeq(Enumerable.Range(start: 0, count: n * n)).Iter(idx => {
            int row = idx / n, col = idx % n;
            double expected = row == col && (sigma is null || row >= sigma.Value.Count || sigma.Value[row] > RhinoMath.ZeroTolerance) ? 1.0 : 0.0;
            _ = MatrixGens.Approx(gram.At(i: row, j: col), expected)
                ? unit : throw new XunitException(userMessage: $"{label}^T*{label}[{row},{col}] expected {expected:R}, got {gram.At(i: row, j: col):R}");
        });
    }
}

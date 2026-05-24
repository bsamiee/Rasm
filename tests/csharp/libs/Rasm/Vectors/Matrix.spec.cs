using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
public static class MatrixGens {
    public static readonly Op Key = Op.Of(name: "matrix-test");
    public static readonly Func<double, double, bool> Approx = Gens.Approx(relativeTolerance: 1.0e-7);
    private const int MaxDim = 6;
    private static Matrix MakeMatrix(int rows, int cols, double[] buffer) =>
        Spec.SuccValue(Matrix.Of(rows: Dimension.Create(value: rows), cols: Dimension.Create(value: cols), entries: [.. buffer.Take(count: rows * cols)], key: Key), label: "matrix gen");
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
        return Spec.SuccValue(SymmetricMatrix.Of(dim: Dimension.Create(value: n),
            upper: [.. Enumerable.Range(start: 0, count: n).SelectMany(i => Enumerable.Range(start: i, count: n - i).Select(j => shifted.At(i: i, j: j)))], key: Key), label: "spd gen");
    });
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class MatrixCoreLaws {
    [Fact]
    public void OfRejectsCountMismatchAndNonFinite() {
        Spec.Fail(Matrix.Of(rows: Dimension.Create(value: 2), cols: Dimension.Create(value: 3), entries: [1.0, 2.0], key: MatrixGens.Key));
        Spec.ForAll(Gens.NonFinite, x =>
            Spec.Fail(Matrix.Of(rows: Dimension.Create(value: 2), cols: Dimension.Create(value: 2), entries: [x, 0.0, 0.0, x], key: MatrixGens.Key)));
        Spec.Fail(SymmetricMatrix.Of(dim: Dimension.Create(value: 3), upper: [1.0, 2.0], key: MatrixGens.Key));
    }
    [Fact]
    public void IdentityIsBilateralUnitAndTransposeIsInvolution() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => {
            NumericSpec.Entrywise(rows: a.Rows.Value, cols: a.Cols.Value, expected: a.At, actual: (Matrix.Identity(dim: a.Rows) * a).At, tolerance: 1.0e-7, label: "I*A");
            NumericSpec.Entrywise(rows: a.Rows.Value, cols: a.Cols.Value, expected: a.At, actual: (a * Matrix.Identity(dim: a.Cols)).At, tolerance: 1.0e-7, label: "A*I");
            NumericSpec.Entrywise(rows: a.Rows.Value, cols: a.Cols.Value, expected: a.At, actual: a.Transpose().Transpose().At, tolerance: 1.0e-7, label: "T(T(A))");
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
            NumericSpec.Symmetric(dimension: spd.Dimension.Value, at: dense.At, tolerance: 1.0e-7, label: "dense");
        });
}

public sealed class SparseMatrixLaws {
    private static SparseMatrix Sparse(int dimension, params (int Row, int Col, double Value)[] triplets) =>
        Spec.SuccValue(SparseMatrix.FromTriplets(rows: Dimension.Create(value: dimension), cols: Dimension.Create(value: dimension), triplets: triplets, key: MatrixGens.Key), label: "sparse");
    private static SparseMatrix Sparse2(params (int Row, int Col, double Value)[] triplets) =>
        Sparse(dimension: 2, triplets: triplets);
    [Fact]
    public void SparseSolveDetailedUsesBoundedMetadata() {
        SparseMatrix matrix = Sparse2((0, 0, 4.0), (1, 1, 2.0));
        Spec.Succ(matrix.SolveDetailed(rhs: [8.0, 6.0], key: MatrixGens.Key), then: result => {
            Spec.SmartEnumKeysUnique(items: [SparseSolveMode.BiCgStabDiagonal, SparseSolveMode.MathNetSolveFallback], key: static s => s.Key);
            Assert.Contains(expected: result.Mode, collection: [SparseSolveMode.BiCgStabDiagonal, SparseSolveMode.MathNetSolveFallback]);
            Assert.Equal(expected: SparseSolveStop.ResidualConverged, actual: result.StopStatus);
            Spec.EqualWithin(left: result.Solution[index: 0], right: 2.0, tolerance: 1.0e-8, what: "x0");
            Spec.EqualWithin(left: result.Solution[index: 1], right: 3.0, tolerance: 1.0e-8, what: "x1");
        });
    }
    [Fact]
    public void SparseCholeskyRejectsNonSymmetricAdmission() {
        Spec.Fail(CholeskySparse.Of(symmetric: Sparse2((0, 0, 2.0), (0, 1, 1.0), (1, 1, 2.0)), key: MatrixGens.Key));
        Spec.Fail(CholeskySparse.Of(symmetric: Sparse2((0, 0, 2.0), (0, 1, 1.0), (1, 0, 0.5), (1, 1, 2.0)), key: MatrixGens.Key));
    }
    [Fact]
    public void SparseCholeskyRejectsIndefiniteSymmetricMatrix() =>
        Spec.Fail(CholeskySparse.Of(symmetric: Sparse2((0, 0, 1.0), (0, 1, 2.0), (1, 0, 2.0), (1, 1, 1.0)), key: MatrixGens.Key));
    [Fact]
    public void SparseCholeskySolvesAndRejectsRhsMismatch() {
        CholeskySparse factor = Spec.SuccValue(CholeskySparse.Of(symmetric: Sparse2((0, 0, 4.0), (1, 1, 2.0)), key: MatrixGens.Key), label: "sparse cholesky");
        Spec.Succ(factor.Solve(rhs: [8.0, 6.0], key: MatrixGens.Key), then: solution => {
            Spec.EqualWithin(left: solution[index: 0], right: 2.0, tolerance: 1.0e-10, what: "chol x0");
            Spec.EqualWithin(left: solution[index: 1], right: 3.0, tolerance: 1.0e-10, what: "chol x1");
        });
        Spec.Fail(factor.Solve(rhs: [1.0], key: MatrixGens.Key));
    }
    [Fact]
    public void LobpcgConvergesOnDiagonalAndFailsWhenExhausted() {
        SparseMatrix diagonal = Sparse(dimension: 3, (0, 0, 1.0), (1, 1, 3.0), (2, 2, 7.0));
        Spec.Succ(diagonal.SmallestEigenpairs(k: 1, tolerance: 1.0e-5, maxIterations: 80, key: MatrixGens.Key), then: pairs => {
            (double eigenvalue, Arr<double> eigenvector) = pairs[0];
            Spec.EqualWithin(left: eigenvalue, right: 1.0, tolerance: 1.0e-4, what: "smallest eigenvalue");
            NumericSpec.Eigenpair(matrix: diagonal.ToDense(), eigenvalue: eigenvalue, eigenvector: eigenvector, eq: Gens.Approx(relativeTolerance: 1.0e-4), label: "lobpcg");
        });
        Spec.Fail(diagonal.SmallestEigenpairs(k: 1, tolerance: 1.0e-12, maxIterations: 0, key: MatrixGens.Key));
    }
}

public sealed class DecompositionLaws {
    [Fact]
    public void SvdReconstructsAEqualsUSigmaVtransposed() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => Spec.Succ(a.DecomposeSvd(key: MatrixGens.Key),
            then: svd => NumericSpec.Entrywise(rows: a.Rows.Value, cols: a.Cols.Value, expected: a.At, actual: SvdReconstruct(svd: svd, n: a.Cols.Value).At, tolerance: 1.0e-7, label: "U*Sigma*V^T")));
    [Fact]
    public void SvdSigmaIsSortedNonNegativeAndMatchesSpectral() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => Spec.Succ(a.DecomposeSvd(key: MatrixGens.Key), then: svd => {
            Arr<double> s = svd.Sigma;
            _ = toSeq(Enumerable.Range(start: 0, count: s.Count)).Iter(i =>
                Spec.Holds(condition: s[i] >= -1.0e-12 && (i == s.Count - 1 || s[i] >= s[i + 1] - 1.0e-12), label: $"sigma invariant fails at i={i}: {s[i]:R}"));
            Spec.Succ(a.Spectral(key: MatrixGens.Key), then: sp =>
                Spec.EqualWithin(left: sp, right: s.IsEmpty ? 0.0 : s[0], tolerance: 1.0e-9, what: "spectral=Sigma[0]"));
        }));
    [Fact]
    public void SvdFactorsAreOrthogonal() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => Spec.Succ(a.DecomposeSvd(key: MatrixGens.Key), then: svd => {
            NumericSpec.GramIdentity(matrix: svd.V, sigma: null, tolerance: 1.0e-7, label: "V");
            NumericSpec.GramIdentity(matrix: svd.U, sigma: null, tolerance: 1.0e-7, label: "U");
        }));
    [Fact]
    public void QrReconstructsAndQIsOrthogonal() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => Spec.Succ(a.DecomposeQr(key: MatrixGens.Key), then: qr => {
            NumericSpec.Entrywise(rows: a.Rows.Value, cols: a.Cols.Value, expected: a.At, actual: (qr.Q * qr.R).At, tolerance: 1.0e-7, label: "Q*R");
            NumericSpec.GramIdentity(matrix: qr.Q, sigma: null, tolerance: 1.0e-7, label: "Q");
        }));
    [Fact]
    public void LuResultKeepsSourceAndDeterminant() =>
        Spec.ForAll(MatrixGens.NonSingularSquare, static a => Spec.Succ(a.DecomposeLu(key: MatrixGens.Key), then: lu => {
            Assert.True(condition: lu.IsValid);
            NumericSpec.Entrywise(rows: a.Rows.Value, cols: a.Cols.Value, expected: a.At, actual: lu.Source.At, tolerance: 1.0e-7, label: "LU source");
            Spec.Succ(a.Determinant(key: MatrixGens.Key), then: det =>
                Spec.EqualWithin(left: lu.Determinant, right: det, tolerance: 1.0e-9, what: "LU determinant"));
        }));
    [Fact]
    public void CholeskyReconstructsSpd() =>
        Spec.ForAll(MatrixGens.Spd, static spd => Spec.Succ(spd.DecomposeCholesky(key: MatrixGens.Key), then: chol =>
            NumericSpec.Entrywise(rows: spd.Dimension.Value, cols: spd.Dimension.Value, expected: spd.ToDense().At, actual: (chol.L * chol.L.Transpose()).At, tolerance: 1.0e-7, label: "L*L^T")));
    [Fact]
    public void SymmetricEigenSatisfiesAvEqualsLambdaV() =>
        Spec.ForAll(MatrixGens.Spd, static spd => Spec.Succ(spd.DecomposeEigen(key: MatrixGens.Key), then: eigs => {
            Matrix dense = spd.ToDense();
            _ = eigs.Iter(pair => NumericSpec.Eigenpair(matrix: dense, eigenvalue: pair.Eigenvalue, eigenvector: pair.Eigenvector, eq: MatrixGens.Approx, label: "symmetric eigen"));
        }));
    private static Matrix SvdReconstruct(SvdResult svd, int n) =>
        svd.U * new Matrix(Rows: svd.U.Cols, Cols: Dimension.Create(value: n),
            Entries: [.. Enumerable.Range(start: 0, count: svd.U.Cols.Value * n).Select(idx =>
                idx / n == idx % n && idx / n < svd.Sigma.Count ? svd.Sigma[idx / n] : 0.0)]) * svd.V.Transpose();
}

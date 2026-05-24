using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class MatrixGens {
    public static readonly Op Key = Op.Of(name: "matrix-test");
    public static readonly Func<double, double, bool> Approx = Gens.Approx(relativeTolerance: 1.0e-7);
    private const int MaxDim = 6;
    private static Matrix MakeMatrix(int rows, int cols, double[] buffer) =>
        Spec.SuccValue(Matrix.Of(rows: Dimension.Create(value: rows), cols: Dimension.Create(value: cols), entries: [.. buffer.Take(count: rows * cols)], key: Key), label: "matrix gen");
    public static SparseMatrix Sparse(int dimension, params (int Row, int Col, double Value)[] triplets) =>
        Spec.SuccValue(SparseMatrix.FromTriplets(rows: Dimension.Create(value: dimension), cols: Dimension.Create(value: dimension), triplets: triplets, key: Key), label: "sparse");
    public static SparseMatrix Sparse2(params (int Row, int Col, double Value)[] triplets) =>
        Sparse(dimension: 2, triplets: triplets);
    public static readonly Gen<Matrix> TallOrSquare =
        Gen.Int[start: 2, finish: MaxDim].Select(
            Gen.Int[start: 2, finish: MaxDim], Gen.Double[start: -10.0, finish: 10.0].Array[MaxDim * MaxDim],
            static (int a, int b, double[] buf) => MakeMatrix(rows: Math.Max(val1: a, val2: b), cols: Math.Min(val1: a, val2: b), buffer: buf));
    public static readonly Gen<Matrix> Rectangular =
        Gen.Int[start: 2, finish: MaxDim].Select(
            Gen.Int[start: 2, finish: MaxDim], Gen.Double[start: -10.0, finish: 10.0].Array[MaxDim * MaxDim],
            static (int a, int b, double[] buf) => MakeMatrix(rows: a == b ? (a == MaxDim ? a - 1 : a + 1) : a, cols: b, buffer: buf));
    public static readonly Gen<Matrix> Square =
        Gen.Int[start: 2, finish: MaxDim].Select(Gen.Double[start: -10.0, finish: 10.0].Array[MaxDim * MaxDim],
            static (int n, double[] buf) => MakeMatrix(rows: n, cols: n, buffer: buf));
    public static readonly Gen<Matrix> NonSingularSquare = Square.Select(static (Matrix a) => {
        int n = a.Rows.Value;
        return toSeq(Enumerable.Range(start: 0, count: n)).Fold(initialState: a,
            f: (m, i) => m.With(i: i, j: i, value: 1.0 + toSeq(Enumerable.Range(start: 0, count: n)).Fold(initialState: 0.0, f: (sum, j) => sum + Math.Abs(value: i == j ? 0.0 : m.At(i: i, j: j)))));
    });
    public static readonly Gen<SymmetricMatrix> Spd = Square.Select(static (Matrix a) => {
        int n = a.Rows.Value;
        return Spec.SuccValue(SymmetricMatrix.Of(dim: Dimension.Create(value: n),
            upper: [.. Enumerable.Range(start: 0, count: n).SelectMany(i => Enumerable.Range(start: i, count: n - i).Select(j =>
                Numeric.Dot(count: n, left: k => a.At(i: k, j: i), right: k => a.At(i: k, j: j)) + (i == j ? n : 0.0)))], key: Key), label: "spd gen");
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
            Spec.Succ(Matrix.Identity(dim: a.Rows).Multiply(other: a, key: MatrixGens.Key), then: product =>
                Numeric.Product(rows: a.Rows.Value, width: a.Rows.Value, cols: a.Cols.Value, left: Matrix.Identity(dim: a.Rows).At, right: a.At, actual: product.At, tolerance: 1.0e-7, label: "I*A"));
            Spec.Succ(a.Multiply(other: Matrix.Identity(dim: a.Cols), key: MatrixGens.Key), then: product =>
                Numeric.Product(rows: a.Rows.Value, width: a.Cols.Value, cols: a.Cols.Value, left: a.At, right: Matrix.Identity(dim: a.Cols).At, actual: product.At, tolerance: 1.0e-7, label: "A*I"));
            Numeric.Entrywise(rows: a.Rows.Value, cols: a.Cols.Value, expected: a.At, actual: a.Transpose().Transpose().At, tolerance: 1.0e-7, label: "T(T(A))");
        });
    [Fact]
    public void MultiplyRailRejectsDimensionMismatchAndMatchesIndependentProduct() {
        Matrix left = Spec.SuccValue(Matrix.Of(rows: Dimension.Create(value: 2), cols: Dimension.Create(value: 3), entries: [1.0, 2.0, 3.0, 4.0, 5.0, 6.0], key: MatrixGens.Key), label: "left");
        Matrix right = Spec.SuccValue(Matrix.Of(rows: Dimension.Create(value: 2), cols: Dimension.Create(value: 2), entries: [1.0, 0.0, 0.0, 1.0], key: MatrixGens.Key), label: "right");
        Spec.FailCategory(left.Multiply(other: right, key: MatrixGens.Key), category: "Input");
        Matrix compatible = Spec.SuccValue(Matrix.Of(rows: Dimension.Create(value: 3), cols: Dimension.Create(value: 2), entries: [1.0, 2.0, 0.0, 1.0, -1.0, 0.0], key: MatrixGens.Key), label: "compatible");
        Spec.Succ(left.Multiply(other: compatible, key: MatrixGens.Key), then: product =>
            Numeric.Product(rows: left.Rows.Value, width: left.Cols.Value, cols: compatible.Cols.Value, left: left.At, right: compatible.At, actual: product.At, tolerance: 1.0e-12, label: "A*B"));
    }
    [Fact]
    public void TraceAndFrobeniusMatchClosedForm() {
        Spec.ForAll(MatrixGens.Square, static a => Spec.Succ(a.Trace(key: MatrixGens.Key), then: t => Spec.EqualWithin(left: t,
            right: toSeq(Enumerable.Range(start: 0, count: a.Rows.Value)).Fold(initialState: 0.0, f: (s, i) => s + a.At(i: i, j: i)),
            tolerance: 1.0e-12, what: "trace")));
        Spec.ForAll(MatrixGens.Rectangular, static a => Spec.FailCategory(a.Trace(key: MatrixGens.Key), category: "Input"));
        Spec.ForAll(MatrixGens.TallOrSquare, static a =>
            Spec.EqualWithin(left: a.Frobenius,
                right: Math.Sqrt(d: a.Entries.Fold(initialState: 0.0, f: static (s, e) => s + (e * e))),
                tolerance: 1.0e-10, what: "frobenius"));
    }
    [Fact]
    public void SolveInversePseudoInverseAndRankUseResidualOrShapeLaws() =>
        Spec.ForAll(MatrixGens.NonSingularSquare, static a => {
            Arr<double> rhs = new([.. Enumerable.Range(start: 1, count: a.Rows.Value).Select(static i => (double)i)]);
            Spec.Succ(a.Solve(rhs: rhs, key: MatrixGens.Key), then: x => Numeric.Residual(matrix: a, x: x, b: rhs, tolerance: 1.0e-7, label: "solve"));
            Spec.Succ(a.Inverse(key: MatrixGens.Key), then: inverse => {
                Numeric.Product(rows: a.Rows.Value, width: a.Cols.Value, cols: a.Cols.Value, left: a.At, right: inverse.At, actual: (row, col) => row == col ? 1.0 : 0.0, tolerance: 1.0e-7, label: "A*A^-1");
                Numeric.Product(rows: inverse.Rows.Value, width: inverse.Cols.Value, cols: inverse.Cols.Value, left: inverse.At, right: a.At, actual: (row, col) => row == col ? 1.0 : 0.0, tolerance: 1.0e-7, label: "A^-1*A");
            });
            Spec.Succ(a.PseudoInverse(key: MatrixGens.Key), then: pinv =>
                Numeric.Entrywise(rows: a.Rows.Value, cols: a.Cols.Value, expected: a.At,
                    actual: (row, col) => Numeric.ProductAt(width: pinv.Cols.Value, left: a.At, right: (i, j) => Numeric.ProductAt(width: a.Cols.Value, left: pinv.At, right: a.At, row: i, col: j), row: row, col: col),
                    tolerance: 1.0e-6, label: "A*A+*A"));
            Spec.Succ(a.Rank(key: MatrixGens.Key), then: rank => Assert.Equal(expected: a.Rows.Value, actual: rank));
        });
    [Fact]
    public void LeastSquaresDetailedUsesQrReceiptAndNormalEquationOracle() {
        Matrix design = Spec.SuccValue(Matrix.Of(rows: Dimension.Create(value: 3), cols: Dimension.Create(value: 2), entries: [1.0, 0.0, 1.0, 1.0, 1.0, 2.0], key: MatrixGens.Key), label: "design");
        Arr<double> rhs = [1.0, 3.0, 5.0];
        Spec.Succ(design.LeastSquaresDetailed(rhs: rhs, key: MatrixGens.Key), then: receipt => {
            Assert.Equal(expected: SolvePath.DenseQrLeastSquares, actual: receipt.Path);
            Assert.Equal(expected: SolveStop.LeastSquaresSolved, actual: receipt.Stop);
            Spec.Some(receipt.FullRank, fullRank => Assert.True(condition: fullRank));
            Spec.EqualWithin(left: receipt.Solution[index: 0], right: 1.0, tolerance: 1.0e-10, what: "least-squares intercept");
            Spec.EqualWithin(left: receipt.Solution[index: 1], right: 2.0, tolerance: 1.0e-10, what: "least-squares slope");
            Spec.EqualWithin(left: receipt.Residual, right: 0.0, tolerance: 1.0e-12, what: "least-squares residual");
            _ = toSeq(Enumerable.Range(start: 0, count: design.Cols.Value)).Iter(col => {
                double normalResidual = toSeq(Enumerable.Range(start: 0, count: design.Rows.Value))
                    .Fold(initialState: 0.0, f: (sum, row) => sum + (design.At(i: row, j: col) * ((design.At(i: row, j: 0) * receipt.Solution[index: 0]) + (design.At(i: row, j: 1) * receipt.Solution[index: 1]) - rhs[index: row])));
                Spec.EqualWithin(left: normalResidual, right: 0.0, tolerance: 1.0e-10, what: $"normal residual {col}");
            });
        });
        Spec.FailCategory(design.LeastSquares(rhs: [1.0, 2.0], key: MatrixGens.Key), category: "Input");
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
            Numeric.Symmetric(dimension: spd.Dimension.Value, at: dense.At, tolerance: 1.0e-7, label: "dense");
        });
}

public sealed class SparseMatrixLaws {
    [Fact]
    public void SparseSolveDetailedUsesBoundedMetadata() {
        SparseMatrix matrix = MatrixGens.Sparse2((0, 0, 4.0), (1, 1, 2.0));
        Spec.Succ(matrix.SolveDetailed(rhs: [8.0, 6.0], key: MatrixGens.Key), then: result => {
            Spec.SmartEnumKeysUnique(items: [SolvePath.DenseLu, SolvePath.DenseQrLeastSquares, SolvePath.DenseCholesky, SolvePath.SparseBiCgStabDiagonal, SolvePath.SparseMathNetQrFallback, SolvePath.SparseCholesky], key: static s => s.Key);
            Spec.SmartEnumKeysUnique(items: [SolveStop.DirectSolved, SolveStop.LeastSquaresSolved, SolveStop.ResidualConverged], key: static s => s.Key);
            Assert.Contains(expected: result.Path, collection: [SolvePath.SparseBiCgStabDiagonal, SolvePath.SparseMathNetQrFallback]);
            Assert.True(condition: result.Path.IsSparse);
            Assert.Equal(expected: SolveStop.ResidualConverged, actual: result.Stop);
            Spec.Some(result.MaxIterations, cap => Assert.True(condition: cap >= 64));
            Spec.Some(result.InputNonZeros, nnz => Assert.Equal(expected: matrix.NonZeros, actual: nnz));
            Spec.EqualWithin(left: result.Solution[index: 0], right: 2.0, tolerance: 1.0e-8, what: "x0");
            Spec.EqualWithin(left: result.Solution[index: 1], right: 3.0, tolerance: 1.0e-8, what: "x1");
        });
    }
    [Fact]
    public void SparseAssemblySumsDuplicatesAndKeepsRowPointersMonotone() {
        SparseMatrix matrix = MatrixGens.Sparse(dimension: 3, (0, 0, 1.0), (0, 0, 2.0), (1, 2, 4.0), (2, 1, -1.0));
        Assert.True(condition: matrix.IsValid);
        Assert.True(condition: SparseMatrix.RowPointersAreMonotone(rowPtr: matrix.RowPtr));
        Matrix dense = matrix.ToDense();
        Spec.EqualWithin(left: dense.At(i: 0, j: 0), right: 3.0, tolerance: 0.0, what: "duplicate sum");
        Spec.EqualWithin(left: dense.At(i: 1, j: 2), right: 4.0, tolerance: 0.0, what: "entry 1,2");
        Spec.EqualWithin(left: dense.At(i: 2, j: 1), right: -1.0, tolerance: 0.0, what: "entry 2,1");
        Spec.FailCategory(SparseMatrix.FromTriplets(rows: Dimension.Create(value: 2), cols: Dimension.Create(value: 2), triplets: [(0, 2, 1.0)], key: MatrixGens.Key), category: "Input");
    }
    [Fact]
    public void SparseCholeskyAdmitsOneTriangleMirrorsAndRejectsConflicts() {
        AssertCholesky2(MatrixGens.Sparse2((0, 0, 4.0), (0, 1, 1.0), (1, 1, 3.0)));
        AssertCholesky2(MatrixGens.Sparse2((0, 0, 4.0), (1, 0, 1.0), (1, 1, 3.0)));
        AssertCholesky2(MatrixGens.Sparse2((0, 0, 4.0), (0, 1, 1.0), (1, 0, 1.0), (1, 1, 3.0)));
        Spec.Fail(CholeskySparse.Of(symmetric: MatrixGens.Sparse2((0, 0, 4.0), (0, 1, 1.0), (1, 0, 0.5), (1, 1, 3.0)), key: MatrixGens.Key));
    }
    [Fact]
    public void SparseCholeskyRejectsIndefiniteSymmetricMatrix() =>
        Spec.Fail(CholeskySparse.Of(symmetric: MatrixGens.Sparse2((0, 0, 1.0), (0, 1, 2.0), (1, 0, 2.0), (1, 1, 1.0)), key: MatrixGens.Key));
    [Fact]
    public void SparseCholeskySolvesAndRejectsRhsMismatch() {
        CholeskySparse factor = Spec.SuccValue(CholeskySparse.Of(symmetric: MatrixGens.Sparse2((0, 0, 4.0), (1, 1, 2.0)), key: MatrixGens.Key), label: "sparse cholesky");
        Spec.Succ(factor.SolveDetailed(rhs: [8.0, 6.0], key: MatrixGens.Key), then: receipt => {
            Assert.Equal(expected: SolvePath.SparseCholesky, actual: receipt.Path);
            Assert.Equal(expected: SolveStop.DirectSolved, actual: receipt.Stop);
            Spec.EqualWithin(left: receipt.Solution[index: 0], right: 2.0, tolerance: 1.0e-10, what: "chol x0");
            Spec.EqualWithin(left: receipt.Solution[index: 1], right: 3.0, tolerance: 1.0e-10, what: "chol x1");
            Spec.Some(receipt.InputNonZeros, nnz => Assert.Equal(expected: factor.Source.NonZeros, actual: nnz));
            Spec.Some(receipt.FactorNonZeros, nnz => Assert.True(condition: nnz > 0));
        });
        Spec.Fail(factor.Solve(rhs: [1.0], key: MatrixGens.Key));
    }
    [Fact]
    public void LobpcgConvergesOnDiagonalAndFailsWhenExhausted() {
        SparseMatrix diagonal = MatrixGens.Sparse(dimension: 3, (0, 0, 1.0), (1, 1, 3.0), (2, 2, 7.0));
        Spec.Succ(diagonal.SmallestEigenpairsDetailed(k: 1, tolerance: 1.0e-5, maxIterations: 80, key: MatrixGens.Key), then: receipt => {
            Assert.Equal(expected: EigenSolvePath.SparseLobpcg, actual: receipt.Path);
            Assert.Equal(expected: EigenSolveStop.ResidualConverged, actual: receipt.Stop);
            (double eigenvalue, Arr<double> eigenvector) = receipt.Pairs[0];
            Spec.EqualWithin(left: eigenvalue, right: 1.0, tolerance: 1.0e-4, what: "smallest eigenvalue");
            Numeric.Eigenpair(matrix: diagonal.ToDense(), eigenvalue: eigenvalue, eigenvector: eigenvector, eq: Gens.Approx(relativeTolerance: 1.0e-4), label: "lobpcg");
            Assert.True(condition: receipt.MaxResidual <= 1.0e-4);
        });
        Spec.FailCategory(diagonal.SmallestEigenpairs(k: 1, tolerance: 1.0e-12, maxIterations: 0, key: MatrixGens.Key), category: "Input");
        SparseMatrix conflictingMirror = MatrixGens.Sparse(dimension: 3, (0, 0, 1.0), (0, 1, 2.0), (1, 0, 4.0), (1, 1, 3.0), (2, 2, 7.0));
        Spec.FailCategory(conflictingMirror.SmallestEigenpairsDetailed(k: 1, tolerance: 1.0e-5, maxIterations: 80, key: MatrixGens.Key), category: "Input");
    }
    private static void AssertCholesky2(SparseMatrix matrix) {
        CholeskySparse factor = Spec.SuccValue(CholeskySparse.Of(symmetric: matrix, key: MatrixGens.Key), label: "sparse cholesky admission");
        Spec.Succ(factor.SolveDetailed(rhs: [1.0, 2.0], key: MatrixGens.Key), then: receipt => {
            Assert.Equal(expected: SolvePath.SparseCholesky, actual: receipt.Path);
            Assert.True(condition: receipt.Residual < 1.0e-10);
            Spec.EqualWithin(left: receipt.Solution[index: 0], right: 1.0 / 11.0, tolerance: 1.0e-10, what: "admission x0");
            Spec.EqualWithin(left: receipt.Solution[index: 1], right: 7.0 / 11.0, tolerance: 1.0e-10, what: "admission x1");
        });
    }
}

public sealed class DecompositionLaws {
    [Fact]
    public void SvdReconstructsAEqualsUSigmaVtransposed() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => Spec.Succ(a.DecomposeSvd(key: MatrixGens.Key),
            then: svd => Numeric.Entrywise(rows: a.Rows.Value, cols: a.Cols.Value, expected: a.At,
                actual: (row, col) => Numeric.Dot(count: svd.Sigma.Count, left: k => svd.U.At(i: row, j: k), right: k => svd.Sigma[k] * svd.V.At(i: col, j: k)),
                tolerance: 1.0e-7, label: "U*Sigma*V^T")));
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
            Numeric.ColumnGramIdentity(matrix: svd.V, sigma: null, tolerance: 1.0e-7, label: "V");
            Numeric.ColumnGramIdentity(matrix: svd.U, sigma: null, tolerance: 1.0e-7, label: "U");
        }));
    [Fact]
    public void QrReconstructsAndQIsOrthogonal() =>
        Spec.ForAll(MatrixGens.TallOrSquare, static a => Spec.Succ(a.DecomposeQr(key: MatrixGens.Key), then: qr => {
            Numeric.Entrywise(rows: a.Rows.Value, cols: a.Cols.Value, expected: a.At,
                actual: (row, col) => Numeric.ProductAt(width: qr.Q.Cols.Value, left: qr.Q.At, right: qr.R.At, row: row, col: col),
                tolerance: 1.0e-7, label: "Q*R");
            Numeric.ColumnGramIdentity(matrix: qr.Q, sigma: null, tolerance: 1.0e-7, label: "Q");
        }));
    [Fact]
    public void LuResultKeepsSourceAndDeterminant() =>
        Spec.ForAll(MatrixGens.NonSingularSquare, static a => Spec.Succ(a.DecomposeLu(key: MatrixGens.Key), then: lu => {
            Assert.True(condition: lu.IsValid);
            Numeric.Entrywise(rows: a.Rows.Value, cols: a.Cols.Value, expected: a.At, actual: lu.Source.At, tolerance: 1.0e-7, label: "LU source");
            double det = Numeric.Determinant(n: a.Rows.Value, at: a.At);
            Spec.EqualWithin(left: lu.Determinant, right: det, tolerance: Math.Max(val1: 1.0e-8, val2: Math.Abs(value: det) * 1.0e-12), what: "LU determinant");
            Arr<double> rhs = new([.. Enumerable.Range(start: 1, count: a.Rows.Value).Select(static i => (double)i)]);
            Spec.Succ(lu.Solve(rhs: rhs, key: MatrixGens.Key), then: x => Numeric.Residual(matrix: a, x: x, b: rhs, tolerance: 1.0e-7, label: "LU solve"));
            Spec.FailCategory(lu.Solve(rhs: new Arr<double>([1.0]), key: MatrixGens.Key), category: "Input");
        }));
    [Fact]
    public void DenseSolveReceiptsReportOwnedPaths() {
        Matrix matrix = Spec.SuccValue(Matrix.Of(rows: Dimension.Create(value: 2), cols: Dimension.Create(value: 2), entries: [4.0, 1.0, 1.0, 3.0], key: MatrixGens.Key), label: "dense solve matrix");
        SymmetricMatrix spd = Spec.SuccValue(SymmetricMatrix.Of(dim: Dimension.Create(value: 2), upper: [4.0, 1.0, 3.0], key: MatrixGens.Key), label: "dense spd");
        Arr<double> rhs = [1.0, 2.0];
        Spec.Succ(matrix.SolveDetailed(rhs: rhs, key: MatrixGens.Key), then: receipt => AssertDenseSolve(receipt: receipt, path: SolvePath.DenseLu));
        Spec.Succ(matrix.DecomposeLu(key: MatrixGens.Key).Bind(lu => lu.SolveDetailed(rhs: rhs, key: MatrixGens.Key)), then: receipt => AssertDenseSolve(receipt: receipt, path: SolvePath.DenseLu));
        Spec.Succ(spd.DecomposeCholesky(key: MatrixGens.Key).Bind(cholesky => cholesky.SolveDetailed(rhs: rhs, key: MatrixGens.Key)), then: receipt => AssertDenseSolve(receipt: receipt, path: SolvePath.DenseCholesky));
        static void AssertDenseSolve(SolveReceipt receipt, SolvePath path) {
            Assert.Equal(expected: path, actual: receipt.Path);
            Assert.Equal(expected: SolveStop.DirectSolved, actual: receipt.Stop);
            Spec.EqualWithin(left: receipt.Solution[index: 0], right: 1.0 / 11.0, tolerance: 1.0e-10, what: $"{path.Key} x0");
            Spec.EqualWithin(left: receipt.Solution[index: 1], right: 7.0 / 11.0, tolerance: 1.0e-10, what: $"{path.Key} x1");
            Assert.True(condition: receipt.Residual < 1.0e-10);
        }
    }
    [Fact]
    public void CholeskyReconstructsSpd() =>
        Spec.ForAll(MatrixGens.Spd, static spd => Spec.Succ(spd.DecomposeCholesky(key: MatrixGens.Key), then: chol => {
            Matrix dense = spd.ToDense();
            Numeric.Entrywise(rows: spd.Dimension.Value, cols: spd.Dimension.Value, expected: dense.At,
                actual: (row, col) => Numeric.ProductAt(width: spd.Dimension.Value, left: chol.L.At, right: (i, j) => chol.L.At(i: j, j: i), row: row, col: col),
                tolerance: 1.0e-7, label: "L*L^T");
        }));
    [Fact]
    public void SymmetricEigenSatisfiesAvEqualsLambdaV() =>
        Spec.ForAll(MatrixGens.Spd, static spd => Spec.Succ(spd.DecomposeEigenDetailed(key: MatrixGens.Key), then: receipt => {
            Matrix dense = spd.ToDense();
            Assert.Equal(expected: EigenSolvePath.DenseSymmetricEvd, actual: receipt.Path);
            Assert.Equal(expected: EigenSolveStop.DirectSolved, actual: receipt.Stop);
            Assert.Equal(expected: spd.Dimension.Value, actual: receipt.Pairs.Count);
            Assert.True(condition: receipt.MaxResidual < 1.0e-7);
            _ = receipt.Pairs.Iter(pair => Numeric.Eigenpair(matrix: dense, eigenvalue: pair.Eigenvalue, eigenvector: pair.Eigenvector, eq: MatrixGens.Approx, label: "symmetric eigen"));
        }));
    [Fact]
    public void GeneralEigenReceiptKeepsComplexPairs() {
        Matrix rotation = Spec.SuccValue(Matrix.Of(rows: Dimension.Create(value: 2), cols: Dimension.Create(value: 2), entries: [0.0, -1.0, 1.0, 0.0], key: MatrixGens.Key), label: "rotation eigen");
        Spec.Succ(rotation.DecomposeEigenDetailed(key: MatrixGens.Key), then: receipt => {
            Assert.Equal(expected: EigenSolvePath.DenseGeneralEvd, actual: receipt.Path);
            Assert.Equal(expected: EigenSolveStop.DirectSolved, actual: receipt.Stop);
            Assert.Equal(expected: 2, actual: receipt.ReturnedPairs);
            Assert.True(condition: receipt.MaxResidual < 1.0e-8);
            Assert.Contains([.. receipt.Pairs.AsIterable()], static pair => Math.Abs(pair.Eigenvalue.Real) < 1.0e-10 && Math.Abs(pair.Eigenvalue.Imaginary - 1.0) < 1.0e-10);
            Assert.Contains([.. receipt.Pairs.AsIterable()], static pair => Math.Abs(pair.Eigenvalue.Real) < 1.0e-10 && Math.Abs(pair.Eigenvalue.Imaginary + 1.0) < 1.0e-10);
        });
    }
    [Fact]
    public void GeneralizedEigenReceiptNamesCholeskyCongruencePath() {
        SparseMatrix stiffness = MatrixGens.Sparse(dimension: 3, (0, 0, 2.0), (1, 1, 3.0), (2, 2, 5.0));
        SparseMatrix mass = MatrixGens.Sparse(dimension: 3, (0, 0, 1.0), (1, 1, 1.0), (2, 2, 1.0));
        Spec.Succ(MatrixKernel.GeneralizedEigenpairsDetailed(stiffness: stiffness, mass: mass, k: 1, key: MatrixGens.Key), then: receipt => {
            Assert.Equal(expected: EigenSolvePath.SparseGeneralizedCholeskyCongruence, actual: receipt.Path);
            Assert.Equal(expected: EigenSolveStop.DirectSolved, actual: receipt.Stop);
            Assert.Equal(expected: 1, actual: receipt.ReturnedPairs);
            Spec.EqualWithin(left: receipt.Pairs[0].Eigenvalue, right: 2.0, tolerance: 1.0e-10, what: "generalized eigenvalue");
            Assert.True(condition: receipt.MaxResidual < 1.0e-10);
        });
    }
}

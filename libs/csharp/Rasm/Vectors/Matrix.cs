using System.Numerics;
using Foundation.CSharp.Analyzers.Contracts;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using ComplexVector = MathNet.Numerics.LinearAlgebra.Vector<System.Numerics.Complex>;
using DenseMatrixC = MathNet.Numerics.LinearAlgebra.Complex.DenseMatrix;
using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVectorC = MathNet.Numerics.LinearAlgebra.Complex.DenseVector;
using DenseVectorD = MathNet.Numerics.LinearAlgebra.Double.DenseVector;
using LinearVector = MathNet.Numerics.LinearAlgebra.Vector<double>;
using SparseMatrixC = MathNet.Numerics.LinearAlgebra.Complex.SparseMatrix;
using SparseMatrixD = MathNet.Numerics.LinearAlgebra.Double.SparseMatrix;

namespace Rasm.Vectors;

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SymmetricMatrix(Dimension Dimension, Arr<double> Upper) {
    public bool IsValid =>
        Upper.Count == (Dimension.Value * (Dimension.Value + 1) / 2)
        && Upper.All(RhinoMath.IsValidDouble);
    internal double At(int i, int j) => Upper[FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j))];
    internal SymmetricMatrix With(int i, int j, double value) =>
        this with { Upper = Upper.SetItem(FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j)), value) };
    private static int FlatIndex(int n, int i, int j) => (i * n) - (i * (i - 1) / 2) + (j - i);
    public static Fin<SymmetricMatrix> Of(Dimension dim, Arr<double> upper, Op? key = null) {
        Op op = key.OrDefault();
        int expected = dim.Value * (dim.Value + 1) / 2;
        return upper.Count == expected && upper.All(RhinoMath.IsValidDouble)
            ? Fin.Succ(new SymmetricMatrix(Dimension: dim, Upper: upper))
            : Fin.Fail<SymmetricMatrix>(error: op.InvalidInput());
    }
    public Matrix ToDense() {
        SymmetricMatrix self = this;
        int dim = Dimension.Value;
        return new Matrix(Rows: Dimension, Cols: Dimension,
            Entries: [.. toSeq(Enumerable.Range(start: 0, count: dim * dim))
                .Map(idx => self.At(i: idx / dim, j: idx % dim))]);
    }
    public Fin<Seq<(double Eigenvalue, Arr<double> Eigenvector)>> DecomposeEigen(Op? key = null) =>
        DecomposeEigenDetailed(key: key.OrDefault()).Map(static receipt => receipt.Pairs);
    public Fin<EigenSolveReceipt<double, Arr<double>>> DecomposeEigenDetailed(Op? key = null) =>
        MatrixKernel.SymmetricEigen(matrix: this, key: key.OrDefault());
    public Fin<CholeskyResult> DecomposeCholesky(Op? key = null) =>
        MatrixKernel.Cholesky(matrix: this, key: key.OrDefault());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct Matrix(Dimension Rows, Dimension Cols, Arr<double> Entries) {
    public bool IsValid =>
        Entries.Count == Rows.Value * Cols.Value
        && Entries.All(RhinoMath.IsValidDouble);
    internal double At(int i, int j) => Entries[(i * Cols.Value) + j];
    internal Matrix With(int i, int j, double value) =>
        this with { Entries = Entries.SetItem((i * Cols.Value) + j, value) };
    public static Fin<Matrix> Of(Dimension rows, Dimension cols, Arr<double> entries, Op? key = null) {
        Op op = key.OrDefault();
        int expected = rows.Value * cols.Value;
        return entries.Count == expected && entries.All(RhinoMath.IsValidDouble)
            ? Fin.Succ(new Matrix(Rows: rows, Cols: cols, Entries: entries))
            : Fin.Fail<Matrix>(error: op.InvalidInput());
    }
    public static Matrix Identity(Dimension dim) =>
        MatrixKernel.FromMathNet(m: DenseMatrixD.CreateIdentity(order: dim.Value), rows: dim, cols: dim);
    public static Matrix Zero(Dimension rows, Dimension cols) =>
        MatrixKernel.FromMathNet(m: DenseMatrixD.Create(rows: rows.Value, columns: cols.Value, value: 0.0), rows: rows, cols: cols);
    public Matrix Transpose() => MatrixKernel.FromMathNet(MatrixKernel.ToMathNet(this).Transpose(), Cols, Rows);
    public Fin<Matrix> Multiply(Matrix other, Op? key = null) {
        Op op = key.OrDefault();
        Matrix self = this;
        return !self.IsValid || !other.IsValid || self.Cols.Value != other.Rows.Value
            ? Fin.Fail<Matrix>(error: op.InvalidInput())
            : op.Catch(() => {
                Matrix product = MatrixKernel.FromMathNet(MatrixKernel.ToMathNet(self).Multiply(MatrixKernel.ToMathNet(other)), self.Rows, other.Cols);
                return product.IsValid ? Fin.Succ(product) : Fin.Fail<Matrix>(error: op.InvalidResult());
            });
    }
    public Fin<Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)>> DecomposeEigen(Op? key = null) =>
        DecomposeEigenDetailed(key: key.OrDefault()).Map(static receipt => receipt.Pairs);
    public Fin<EigenSolveReceipt<Complex, Arr<Complex>>> DecomposeEigenDetailed(Op? key = null) =>
        MatrixKernel.GeneralEigen(matrix: this, key: key.OrDefault());
    public Fin<SvdResult> DecomposeSvd(Op? key = null) => MatrixKernel.Svd(matrix: this, key: key.OrDefault());
    public Fin<LuResult> DecomposeLu(Op? key = null) => MatrixKernel.Lu(matrix: this, key: key.OrDefault());
    public Fin<QrResult> DecomposeQr(Op? key = null) => MatrixKernel.Qr(matrix: this, key: key.OrDefault());
    public double Frobenius => MatrixNormKind.Frobenius.Compute(matrix: this);
    public Fin<double> Norm(MatrixNormKind kind, Op? key = null) =>
        kind is null ? Fin.Fail<double>(error: key.OrDefault().InvalidInput()) : Fin.Succ(kind.Compute(matrix: this));
    public Fin<double> Trace(Op? key = null) {
        Op op = key.OrDefault();
        return Rows.Value != Cols.Value
            ? Fin.Fail<double>(op.InvalidInput())
            : op.AcceptValue(value: MatrixKernel.ToMathNet(this).Trace());
    }
    public Fin<double> Determinant(Op? key = null) =>
        MatrixKernel.Determinant(matrix: this, key: key.OrDefault());
    public Fin<double> Spectral(Op? key = null) =>
        DecomposeSvd(key: key.OrDefault()).Map(static svd => svd.Sigma.IsEmpty ? 0.0 : svd.Sigma[0]);
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) =>
        SolveDetailed(rhs: rhs, key: key.OrDefault()).Map(static receipt => receipt.Solution);
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.Solve(matrix: this, rhs: rhs, key: key.OrDefault());
    public Fin<Arr<double>> LeastSquares(Arr<double> rhs, Op? key = null) =>
        LeastSquaresDetailed(rhs: rhs, key: key.OrDefault()).Map(static receipt => receipt.Solution);
    public Fin<SolveReceipt> LeastSquaresDetailed(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.LeastSquares(matrix: this, rhs: rhs, key: key.OrDefault());
    public Fin<int> Rank(Op? key = null) =>
        DecomposeSvd(key: key.OrDefault()).Map(static svd => svd.NumericalRank());
    public Fin<Matrix> Inverse(Op? key = null) {
        Op op = key.OrDefault();
        Matrix self = this;
        return Rows.Value != Cols.Value
            ? Fin.Fail<Matrix>(error: op.InvalidInput())
            : op.Catch(() => {
                Matrix inverse = MatrixKernel.FromMathNet(MatrixKernel.ToMathNet(self).Inverse(), self.Rows, self.Cols);
                return inverse.IsValid ? Fin.Succ(inverse) : Fin.Fail<Matrix>(op.InvalidResult());
            });
    }
    public Fin<Matrix> PseudoInverse(Op? key = null) =>
        MatrixKernel.PseudoInverse(matrix: this, key: key.OrDefault());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SvdResult(Matrix U, Arr<double> Sigma, Matrix V, int Rank) {
    public bool IsValid => U.IsValid && V.IsValid && Sigma.All(static value => RhinoMath.IsValidDouble(x: value) && value >= 0.0);
    public int NumericalRank() => Rank;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct LuResult {
    internal LuResult(Matrix source, double determinant, MathNet.Numerics.LinearAlgebra.Factorization.LU<double> factor) {
        Source = source;
        Determinant = determinant;
        Factor = factor;
    }
    public Matrix Source { get; }
    public double Determinant { get; }
    internal MathNet.Numerics.LinearAlgebra.Factorization.LU<double> Factor { get; }
    public bool IsValid => Source.IsValid && RhinoMath.IsValidDouble(x: Determinant);
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) =>
        SolveDetailed(rhs: rhs, key: key.OrDefault()).Map(static receipt => receipt.Solution);
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.LuSolve(lu: this, rhs: rhs, key: key.OrDefault());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct QrResult(Matrix Q, Matrix R) {
    public bool IsValid => Q.IsValid && R.IsValid;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CholeskyResult {
    internal CholeskyResult(Matrix l, Matrix source, MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> factor) {
        L = l;
        Source = source;
        Factor = factor;
    }
    public Matrix L { get; }
    public Matrix Source { get; }
    internal MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> Factor { get; }
    public bool IsValid => L.IsValid && Source.IsValid && L.Rows.Value == L.Cols.Value && Source.Rows.Value == Source.Cols.Value;
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) =>
        SolveDetailed(rhs: rhs, key: key.OrDefault()).Map(static receipt => receipt.Solution);
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.CholeskySolve(cholesky: this, rhs: rhs, key: key.OrDefault());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SparseMatrix(Dimension Rows, Dimension Cols, Arr<int> RowPtr, Arr<int> ColInd, Arr<double> Values) {
    public bool IsValid =>
        RowPtr.Count == Rows.Value + 1
        && ColInd.Count == Values.Count
        && Values.All(RhinoMath.IsValidDouble)
        && RowPtr[0] == 0
        && RowPtr[Rows.Value] == Values.Count
        && RowPointersAreMonotone(RowPtr)
        && ColumnIndicesAreBoundedAndSorted(rowPtr: RowPtr, colInd: ColInd, cols: Cols.Value);
    public int NonZeros => Values.Count;
    public static Fin<SparseMatrix> FromTriplets(Dimension rows, Dimension cols, IEnumerable<(int Row, int Col, double Value)> triplets, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(triplets).ToFin(op.InvalidInput()).Bind(active => MatrixKernel.AssembleSparse(rows: rows, cols: cols, triplets: active, op: op));
    }
    public Fin<Arr<double>> Multiply(Arr<double> vector, Op? key = null) {
        Op op = key.OrDefault();
        return vector.Count != Cols.Value
            ? Fin.Fail<Arr<double>>(op.InvalidInput())
            : Fin.Succ(MatrixKernel.SparseMatVec(self: this, x: vector));
    }
    public Matrix ToDense() => MatrixKernel.SparseToDense(self: this);
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) =>
        SolveDetailed(rhs: rhs, key: key.OrDefault()).Map(static result => result.Solution);
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.SparseSolve(matrix: this, rhs: rhs, key: key.OrDefault());
    public Fin<Seq<(double Eigenvalue, Arr<double> Eigenvector)>> SmallestEigenpairs(int k, double tolerance, int maxIterations = 200, Op? key = null) =>
        SmallestEigenpairsDetailed(k: k, tolerance: tolerance, maxIterations: maxIterations, key: key.OrDefault()).Map(static receipt => receipt.Pairs);
    public Fin<EigenSolveReceipt<double, Arr<double>>> SmallestEigenpairsDetailed(int k, double tolerance, int maxIterations = 200, Op? key = null) =>
        MatrixKernel.Lobpcg(matrix: this, k: k, tolerance: tolerance, maxIterations: maxIterations, key: key.OrDefault());
    internal static bool RowPointersAreMonotone(Arr<int> rowPtr) =>
        toSeq(Enumerable.Range(start: 0, count: Math.Max(val1: 0, val2: rowPtr.Count - 1)))
            .ForAll(i => rowPtr[i] <= rowPtr[i + 1]);
    private static bool ColumnIndicesAreBoundedAndSorted(Arr<int> rowPtr, Arr<int> colInd, int cols) =>
        toSeq(Enumerable.Range(start: 0, count: Math.Max(val1: 0, val2: rowPtr.Count - 1)))
            .ForAll(row => toSeq(Enumerable.Range(start: rowPtr[row], count: rowPtr[row + 1] - rowPtr[row]))
                .Fold(initialState: (Ok: true, Prev: -1), f: (state, k) => (
                    Ok: state.Ok && colInd[k] >= 0 && colInd[k] < cols && colInd[k] > state.Prev,
                    Prev: colInd[k])).Ok);
}

public readonly record struct SolveReceipt(
    Arr<double> Solution,
    SolvePath Path,
    SolveStop Stop,
    Dimension Rows,
    Dimension Cols,
    int RhsLength,
    Option<int> Iterations,
    Option<int> MaxIterations,
    Option<double> Tolerance,
    double Residual,
    Option<bool> FullRank,
    Option<int> InputNonZeros,
    Option<int> FactorNonZeros);

[SmartEnum<int>]
public sealed partial class SolvePath {
    public static readonly SolvePath DenseLu = new(key: 0, isSparse: false, isFallback: false, usesDiagonalPreconditioner: false);
    public static readonly SolvePath DenseQrLeastSquares = new(key: 1, isSparse: false, isFallback: false, usesDiagonalPreconditioner: false);
    public static readonly SolvePath DenseCholesky = new(key: 2, isSparse: false, isFallback: false, usesDiagonalPreconditioner: false);
    public static readonly SolvePath SparseBiCgStabDiagonal = new(key: 3, isSparse: true, isFallback: false, usesDiagonalPreconditioner: true);
    public static readonly SolvePath SparseMathNetQrFallback = new(key: 4, isSparse: true, isFallback: true, usesDiagonalPreconditioner: false);
    public static readonly SolvePath SparseCholesky = new(key: 5, isSparse: true, isFallback: false, usesDiagonalPreconditioner: false);
    public bool IsSparse { get; }
    public bool IsFallback { get; }
    public bool UsesDiagonalPreconditioner { get; }
}

[SmartEnum<int>]
public sealed partial class SolveStop {
    public static readonly SolveStop DirectSolved = new(key: 0);
    public static readonly SolveStop LeastSquaresSolved = new(key: 1);
    public static readonly SolveStop ResidualConverged = new(key: 2);
}

public readonly record struct EigenSolveReceipt<TEigen, TVector>(
    Seq<(TEigen Eigenvalue, TVector Eigenvector)> Pairs,
    EigenSolvePath Path,
    EigenSolveStop Stop,
    int RequestedPairs,
    int ReturnedPairs,
    Option<int> Iterations,
    Option<int> MaxIterations,
    Option<double> Tolerance,
    double MaxResidual);

[SmartEnum<int>]
public sealed partial class EigenSolvePath {
    public static readonly EigenSolvePath DenseSymmetricEvd = new(key: 0, isSparse: false, isComplex: false);
    public static readonly EigenSolvePath DenseGeneralEvd = new(key: 1, isSparse: false, isComplex: true);
    public static readonly EigenSolvePath SparseLobpcg = new(key: 2, isSparse: true, isComplex: false);
    public static readonly EigenSolvePath SparseHermitianLobpcg = new(key: 3, isSparse: true, isComplex: true);
    public static readonly EigenSolvePath SparseGeneralizedCholeskyCongruence = new(key: 4, isSparse: true, isComplex: false);
    public bool IsSparse { get; }
    public bool IsComplex { get; }
}

[SmartEnum<int>]
public sealed partial class EigenSolveStop {
    public static readonly EigenSolveStop DirectSolved = new(key: 0);
    public static readonly EigenSolveStop ResidualConverged = new(key: 1);
}

[BoundaryAdapter]
public sealed record CholeskySparse {
    private CholeskySparse(SparseMatrix source, CSparse.Double.Factorization.SparseCholesky factor, Dimension order) {
        Source = source;
        Factor = factor;
        Order = order;
    }
    public SparseMatrix Source { get; }
    internal CSparse.Double.Factorization.SparseCholesky Factor { get; }
    public Dimension Order { get; }
    public bool IsValid => Source.IsValid && Factor.NonZerosCount > 0 && Order.Value > 0;
    public static Fin<CholeskySparse> Of(SparseMatrix symmetric, Op? key = null) {
        Op op = key.OrDefault();
        return symmetric.Rows.Value != symmetric.Cols.Value || !symmetric.IsValid
            ? Fin.Fail<CholeskySparse>(error: op.InvalidInput())
            : from csc in MatrixKernel.ToCSparseSymmetric(s: symmetric, key: op)
              from factor in op.Catch(() => {
                  CSparse.Double.Factorization.SparseCholesky factor =
                      CSparse.Double.Factorization.SparseCholesky.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA);
                  return Fin.Succ(factor);
              })
              select new CholeskySparse(source: symmetric, factor: factor, order: symmetric.Rows);
    }
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) =>
        SolveDetailed(rhs: rhs, key: key.OrDefault()).Map(static receipt => receipt.Solution);
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) {
        Op op = key.OrDefault();
        return !IsValid || rhs.Count != Order.Value || !rhs.All(RhinoMath.IsValidDouble)
            ? Fin.Fail<SolveReceipt>(error: op.InvalidInput())
            : op.Catch(() => {
                double[] b = [.. rhs.AsIterable()];
                double[] x = new double[Order.Value];
                Factor.Solve(input: b.AsSpan(), result: x.AsSpan());
                Arr<double> solution = new(x);
                return MatrixKernel.SparseSymmetricResidual(matrix: Source, solution: solution, rhs: rhs, key: op).Bind(residual =>
                    x.All(RhinoMath.IsValidDouble) && RhinoMath.IsValidDouble(x: residual)
                    ? Fin.Succ(new SolveReceipt(
                        Solution: solution,
                        Path: SolvePath.SparseCholesky,
                        Stop: SolveStop.DirectSolved,
                        Rows: Source.Rows,
                        Cols: Source.Cols,
                        RhsLength: rhs.Count,
                        Iterations: Option<int>.None,
                        MaxIterations: Option<int>.None,
                        Tolerance: Option<double>.None,
                        Residual: residual,
                        FullRank: Option<bool>.None,
                        InputNonZeros: Some(Source.NonZeros),
                        FactorNonZeros: Some(Factor.NonZerosCount)))
                    : Fin.Fail<SolveReceipt>(error: op.InvalidResult()));
            });
    }
}

// Upper-triangular storage; Multiply reconstructs the lower triangle by conjugate transpose.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SparseHermitian(Dimension Order, Arr<int> RowPtr, Arr<int> ColInd, Arr<Complex> Values) {
    public bool IsValid =>
        RowPtr.Count == Order.Value + 1
        && ColInd.Count == Values.Count
        && Values.All(static c => RhinoMath.IsValidDouble(c.Real) && RhinoMath.IsValidDouble(c.Imaginary))
        && RowPtr[0] == 0
        && RowPtr[Order.Value] == Values.Count
        && SparseMatrix.RowPointersAreMonotone(RowPtr)
        && UpperColumnIndicesAreBoundedAndSorted(rowPtr: RowPtr, colInd: ColInd, order: Order.Value);
    public int NonZeros => Values.Count;
    public static Fin<SparseHermitian> FromTriplets(Dimension order, IEnumerable<(int Row, int Col, Complex Value)> upperTriplets, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(upperTriplets).ToFin(op.InvalidInput()).Bind(active => MatrixKernel.AssembleHermitian(order: order, triplets: active, op: op));
    }
    public Fin<Arr<Complex>> Multiply(Arr<Complex> vector, Op? key = null) {
        Op op = key.OrDefault();
        return vector.Count != Order.Value
            ? Fin.Fail<Arr<Complex>>(op.InvalidInput())
            : Fin.Succ(MatrixKernel.HermitianMatVec(self: this, x: vector));
    }
    public Fin<Seq<(double Eigenvalue, Arr<Complex> Eigenvector)>> SmallestEigenpairs(int k, double tolerance, int maxIterations = 200, Op? key = null) =>
        SmallestEigenpairsDetailed(k: k, tolerance: tolerance, maxIterations: maxIterations, key: key.OrDefault()).Map(static receipt => receipt.Pairs);
    public Fin<EigenSolveReceipt<double, Arr<Complex>>> SmallestEigenpairsDetailed(int k, double tolerance, int maxIterations = 200, Op? key = null) =>
        MatrixKernel.LobpcgHermitian(matrix: this, k: k, tolerance: tolerance, maxIterations: maxIterations, key: key.OrDefault());
    private static bool UpperColumnIndicesAreBoundedAndSorted(Arr<int> rowPtr, Arr<int> colInd, int order) =>
        toSeq(Enumerable.Range(start: 0, count: Math.Max(val1: 0, val2: rowPtr.Count - 1)))
            .ForAll(row => toSeq(Enumerable.Range(start: rowPtr[row], count: rowPtr[row + 1] - rowPtr[row]))
                .Fold(initialState: (Ok: true, Prev: row - 1), f: (state, k) => (
                    Ok: state.Ok && colInd[k] >= row && colInd[k] < order && colInd[k] > state.Prev,
                    Prev: colInd[k])).Ok);
}

[SmartEnum<int>]
public sealed partial class MatrixNormKind {
    public static readonly MatrixNormKind Frobenius = new(key: 0, compute: static m => MatrixKernel.ToMathNet(m).FrobeniusNorm());
    public static readonly MatrixNormKind MaxAbs = new(key: 1, compute: static m => MatrixKernel.ToMathNet(m).Enumerate().Aggregate(0.0, static (acc, e) => Math.Max(acc, Math.Abs(e))));
    public static readonly MatrixNormKind L1 = new(key: 2, compute: static m => MatrixKernel.ToMathNet(m).L1Norm());
    public static readonly MatrixNormKind LInf = new(key: 3, compute: static m => MatrixKernel.ToMathNet(m).InfinityNorm());
    [UseDelegateFromConstructor] internal partial double Compute(Matrix matrix);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class MatrixKernel {
    private const int RealInitialBasisSeed = 17;
    private const int HermitianInitialBasisSeed = 19;
    private static Fin<List<(int Row, int Col, double Value)>> NormalizeSymmetricUpperEntries(SparseMatrix s, Op key) {
        if (!s.IsValid || s.Rows.Value != s.Cols.Value) return Fin.Fail<List<(int Row, int Col, double Value)>>(key.InvalidInput());
        int n = s.Rows.Value;
        Dictionary<(int Row, int Col), double> upper = [];
        for (int row = 0; row < n; row++)
            for (int k = s.RowPtr[row]; k < s.RowPtr[row + 1]; k++) {
                int col = s.ColInd[k];
                double value = s.Values[k];
                (int Row, int Col) normalized = (Math.Min(val1: row, val2: col), Math.Max(val1: row, val2: col));
                if (upper.TryGetValue(key: normalized, value: out double existing)
                    && Math.Abs(value - existing) > RhinoMath.SqrtEpsilon * Math.Max(val1: 1.0, val2: Math.Max(val1: Math.Abs(value), val2: Math.Abs(existing))))
                    return Fin.Fail<List<(int Row, int Col, double Value)>>(key.InvalidInput());
                upper[normalized] = value;
            }
        return Fin.Succ(upper
            .Select(static pair => (pair.Key.Row, pair.Key.Col, pair.Value))
            .OrderBy(static e => e.Row).ThenBy(static e => e.Col)
            .ToList());
    }
    internal static Fin<CSparse.Double.SparseMatrix> ToCSparseSymmetric(SparseMatrix s, Op key) =>
        NormalizeSymmetricUpperEntries(s: s, key: key).Map(upper => {
            int n = s.Rows.Value;
            List<(int Row, int Col, double Value)> ordered = [.. upper.OrderBy(static e => e.Col).ThenBy(static e => e.Row)];
            int[] columnPointers = new int[n + 1];
            int[] rowIndices = new int[ordered.Count];
            double[] values = new double[ordered.Count];
            int cursor = 0;
            for (int col = 0; col < n; col++) {
                columnPointers[col] = cursor;
                while (cursor < ordered.Count && ordered[cursor].Col == col) {
                    rowIndices[cursor] = ordered[cursor].Row;
                    values[cursor] = ordered[cursor].Value;
                    cursor++;
                }
            }
            columnPointers[n] = cursor;
            return new CSparse.Double.SparseMatrix(
                rowCount: n,
                columnCount: n,
                values: values,
                rowIndices: rowIndices,
                columnPointers: columnPointers);
        });

    internal static DenseMatrixD ToMathNet(Matrix m) =>
        (DenseMatrixD)DenseMatrixD.Build.DenseOfRowMajor(m.Rows.Value, m.Cols.Value, m.Entries.AsIterable());
    internal static Matrix FromMathNet(Matrix<double> m, Dimension rows, Dimension cols) =>
        new(Rows: rows, Cols: cols, Entries: new Arr<double>(m.ToRowMajorArray()));
    private static DenseMatrixC ToMathNetComplex(Matrix m) =>
        (DenseMatrixC)DenseMatrixC.Build.Dense(m.Rows.Value, m.Cols.Value, (i, j) => new Complex(m.At(i: i, j: j), 0.0));
    private static Arr<double> ArrFromVector(LinearVector v) => new(v.ToArray());
    private static Arr<Complex> ArrFromComplexVector(ComplexVector v) => new(v.ToArray());
    private static double RelativeResidual(Matrix<double> a, LinearVector x, LinearVector b) =>
        (b - a.Multiply(x)).L2Norm() / Math.Max(val1: 1.0, val2: b.L2Norm());
    internal static double SparseResidual(SparseMatrix matrix, Arr<double> solution, Arr<double> rhs) =>
        RelativeResidual(
            a: ToMathNetSparse(s: matrix),
            x: DenseVectorD.OfArray([.. solution.AsIterable()]),
            b: DenseVectorD.OfArray([.. rhs.AsIterable()]));
    internal static Fin<double> SparseSymmetricResidual(SparseMatrix matrix, Arr<double> solution, Arr<double> rhs, Op key) {
        int n = matrix.Rows.Value;
        double[] y = new double[n];
        return NormalizeSymmetricUpperEntries(s: matrix, key: key).Bind(upper => {
            foreach ((int row, int col, double value) in upper) {
                y[row] += value * solution[index: col];
                if (row != col) y[col] += value * solution[index: row];
            }
            double residual = 0.0;
            double rhsNorm = 0.0;
            for (int i = 0; i < n; i++) {
                double delta = rhs[index: i] - y[i];
                residual += delta * delta;
                rhsNorm += rhs[index: i] * rhs[index: i];
            }
            return key.AcceptValue(value: Math.Sqrt(d: residual) / Math.Max(val1: 1.0, val2: Math.Sqrt(d: rhsNorm)));
        });
    }
    private static double RealEigenResidual(Matrix<double> a, Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs) =>
        pairs.Fold(initialState: 0.0, f: (max, pair) => {
            LinearVector v = DenseVectorD.OfArray([.. pair.Eigenvector.AsIterable()]);
            double residual = (a.Multiply(v) - (pair.Eigenvalue * v)).L2Norm() / Math.Max(val1: 1.0, val2: v.L2Norm());
            return Math.Max(val1: max, val2: residual);
        });
    private static double GeneralizedEigenResidual(Matrix<double> stiffness, Matrix<double> mass, Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs) =>
        pairs.Fold(initialState: 0.0, f: (max, pair) => {
            LinearVector v = DenseVectorD.OfArray([.. pair.Eigenvector.AsIterable()]);
            LinearVector lhs = stiffness.Multiply(v);
            LinearVector rhs = mass.Multiply(v) * pair.Eigenvalue;
            double residual = (lhs - rhs).L2Norm() / Math.Max(val1: 1.0, val2: lhs.L2Norm());
            return Math.Max(val1: max, val2: residual);
        });
    private static double ComplexEigenResidual(Matrix<Complex> a, Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)> pairs) =>
        pairs.Fold(initialState: 0.0, f: (max, pair) => {
            ComplexVector v = DenseVectorC.OfArray([.. pair.Eigenvector.AsIterable()]);
            double residual = (a.Multiply(v) - (v * pair.Eigenvalue)).L2Norm() / Math.Max(val1: 1.0, val2: v.L2Norm());
            return Math.Max(val1: max, val2: residual);
        });
    private static double HermitianEigenResidual(Matrix<Complex> a, Seq<(double Eigenvalue, Arr<Complex> Eigenvector)> pairs) =>
        pairs.Fold(initialState: 0.0, f: (max, pair) => {
            ComplexVector v = DenseVectorC.OfArray([.. pair.Eigenvector.AsIterable()]);
            double residual = (a.Multiply(v) - (v * pair.Eigenvalue)).L2Norm() / Math.Max(val1: 1.0, val2: v.L2Norm());
            return Math.Max(val1: max, val2: residual);
        });

    // --- [DENSE_DECOMPOSITIONS] -------------------------------------------------------------
    internal static Fin<SvdResult> Svd(Matrix matrix, Op key) => key.Catch(() => {
        MathNet.Numerics.LinearAlgebra.Factorization.Svd<double> svd = ToMathNet(matrix).Svd(computeVectors: true);
        return Fin.Succ(new SvdResult(
            U: FromMathNet(svd.U, matrix.Rows, matrix.Rows),
            Sigma: ArrFromVector(svd.S),
            V: FromMathNet(svd.VT.Transpose(), matrix.Cols, matrix.Cols),
            Rank: svd.Rank));
    });
    internal static Fin<LuResult> Lu(Matrix matrix, Op key) =>
        matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<LuResult>(key.InvalidInput())
            : key.Catch(() => {
                MathNet.Numerics.LinearAlgebra.Factorization.LU<double> lu = ToMathNet(matrix).LU();
                return Fin.Succ(new LuResult(source: matrix, determinant: lu.Determinant, factor: lu));
            });
    internal static Fin<QrResult> Qr(Matrix matrix, Op key) => key.Catch(() => {
        MathNet.Numerics.LinearAlgebra.Factorization.QR<double> qr = ToMathNet(matrix).QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Full);
        return Fin.Succ(new QrResult(
            Q: FromMathNet(qr.Q, matrix.Rows, matrix.Rows),
            R: FromMathNet(qr.R, matrix.Rows, matrix.Cols)));
    });
    internal static Fin<CholeskyResult> Cholesky(SymmetricMatrix matrix, Op key) =>
        key.Catch(() => {
            Matrix source = matrix.ToDense();
            MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> factor = ToMathNet(source).Cholesky();
            return Fin.Succ(new CholeskyResult(
                l: FromMathNet(factor.Factor, matrix.Dimension, matrix.Dimension),
                source: source,
                factor: factor));
        });
    internal static Fin<EigenSolveReceipt<double, Arr<double>>> SymmetricEigen(SymmetricMatrix matrix, Op key) => key.Catch(() => {
        Matrix dense = matrix.ToDense();
        Matrix<double> mathNet = ToMathNet(dense);
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = mathNet.Evd(Symmetricity.Symmetric);
        int n = matrix.Dimension.Value;
        Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: n)
            .Select(i => (Eigenvalue: evd.EigenValues[i].Real, Eigenvector: ArrFromVector(evd.EigenVectors.Column(i))))
            .OrderByDescending(static p => Math.Abs(p.Eigenvalue)));
        return Fin.Succ(new EigenSolveReceipt<double, Arr<double>>(
            Pairs: pairs,
            Path: EigenSolvePath.DenseSymmetricEvd,
            Stop: EigenSolveStop.DirectSolved,
            RequestedPairs: n,
            ReturnedPairs: pairs.Count,
            Iterations: Option<int>.None,
            MaxIterations: Option<int>.None,
            Tolerance: Option<double>.None,
            MaxResidual: RealEigenResidual(a: mathNet, pairs: pairs)));
    });
    internal static Fin<EigenSolveReceipt<Complex, Arr<Complex>>> GeneralEigen(Matrix matrix, Op key) {
        return matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<EigenSolveReceipt<Complex, Arr<Complex>>>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<Complex> mathNet = ToMathNetComplex(matrix);
                MathNet.Numerics.LinearAlgebra.Factorization.Evd<Complex> evd = mathNet.Evd(Symmetricity.Asymmetric);
                int n = matrix.Rows.Value;
                Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: n)
                    .Select(i => (Eigenvalue: evd.EigenValues[i], Eigenvector: ArrFromComplexVector(evd.EigenVectors.Column(i)))));
                return Fin.Succ(new EigenSolveReceipt<Complex, Arr<Complex>>(
                    Pairs: pairs,
                    Path: EigenSolvePath.DenseGeneralEvd,
                    Stop: EigenSolveStop.DirectSolved,
                    RequestedPairs: n,
                    ReturnedPairs: pairs.Count,
                    Iterations: Option<int>.None,
                    MaxIterations: Option<int>.None,
                    Tolerance: Option<double>.None,
                    MaxResidual: ComplexEigenResidual(a: mathNet, pairs: pairs)));
            });
    }
    internal static Fin<SolveReceipt> Solve(Matrix matrix, Arr<double> rhs, Op key) =>
        !matrix.IsValid || rhs.Count != matrix.Rows.Value || matrix.Rows.Value != matrix.Cols.Value || !rhs.All(RhinoMath.IsValidDouble)
            ? Fin.Fail<SolveReceipt>(error: key.InvalidInput())
            : Lu(matrix: matrix, key: key).Bind(lu => LuSolve(lu: lu, rhs: rhs, key: key));
    internal static Fin<SolveReceipt> LeastSquares(Matrix matrix, Arr<double> rhs, Op key) =>
        rhs.Count != matrix.Rows.Value || !matrix.IsValid || !rhs.All(RhinoMath.IsValidDouble)
            ? Fin.Fail<SolveReceipt>(error: key.InvalidInput())
            : key.Catch(() => {
                Matrix<double> design = ToMathNet(matrix);
                LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
                MathNet.Numerics.LinearAlgebra.Factorization.QR<double> qr = design.QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Full);
                LinearVector x = qr.Solve(b);
                double residual = RelativeResidual(a: design, x: x, b: b);
                Arr<double> solved = ArrFromVector(x);
                return solved.Count == matrix.Cols.Value && solved.ForAll(RhinoMath.IsValidDouble) && RhinoMath.IsValidDouble(x: residual)
                    ? Fin.Succ(new SolveReceipt(
                        Solution: solved,
                        Path: SolvePath.DenseQrLeastSquares,
                        Stop: SolveStop.LeastSquaresSolved,
                        Rows: matrix.Rows,
                        Cols: matrix.Cols,
                        RhsLength: rhs.Count,
                        Iterations: Option<int>.None,
                        MaxIterations: Option<int>.None,
                        Tolerance: Option<double>.None,
                        Residual: residual,
                        FullRank: Some(qr.IsFullRank),
                        InputNonZeros: Option<int>.None,
                        FactorNonZeros: Option<int>.None))
                    : Fin.Fail<SolveReceipt>(key.InvalidResult());
            });
    internal static Fin<SolveReceipt> LuSolve(LuResult lu, Arr<double> rhs, Op key) =>
        !lu.IsValid || rhs.Count != lu.Source.Rows.Value || lu.Source.Rows.Value != lu.Source.Cols.Value || !rhs.All(RhinoMath.IsValidDouble)
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<double> a = ToMathNet(lu.Source);
                LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
                LinearVector x = lu.Factor.Solve(b);
                double residual = RelativeResidual(a: a, x: x, b: b);
                Arr<double> solved = ArrFromVector(x);
                return solved.ForAll(RhinoMath.IsValidDouble) && RhinoMath.IsValidDouble(x: residual)
                    ? Fin.Succ(new SolveReceipt(
                        Solution: solved,
                        Path: SolvePath.DenseLu,
                        Stop: SolveStop.DirectSolved,
                        Rows: lu.Source.Rows,
                        Cols: lu.Source.Cols,
                        RhsLength: rhs.Count,
                        Iterations: Option<int>.None,
                        MaxIterations: Option<int>.None,
                        Tolerance: Option<double>.None,
                        Residual: residual,
                        FullRank: Option<bool>.None,
                        InputNonZeros: Option<int>.None,
                        FactorNonZeros: Option<int>.None))
                    : Fin.Fail<SolveReceipt>(key.InvalidResult());
            });
    internal static Fin<double> Determinant(Matrix matrix, Op key) =>
        matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<double>(error: key.InvalidInput())
            : key.Catch(() => Fin.Succ(ToMathNet(matrix).Determinant()));
    internal static Fin<Matrix> PseudoInverse(Matrix matrix, Op key) =>
        key.Catch(() => Fin.Succ(FromMathNet(m: ToMathNet(matrix).PseudoInverse(), rows: matrix.Cols, cols: matrix.Rows)));

    // --- [DENSE_CHOLESKY_SOLVE] -------------------------------------------------------------
    internal static Fin<SolveReceipt> CholeskySolve(CholeskyResult cholesky, Arr<double> rhs, Op key) {
        return !cholesky.IsValid || rhs.Count != cholesky.Source.Rows.Value || cholesky.Source.Rows.Value != cholesky.Source.Cols.Value || !rhs.All(RhinoMath.IsValidDouble)
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<double> a = ToMathNet(cholesky.Source);
                LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
                LinearVector x = cholesky.Factor.Solve(b);
                double residual = RelativeResidual(a: a, x: x, b: b);
                Arr<double> solved = ArrFromVector(x);
                return solved.ForAll(RhinoMath.IsValidDouble) && RhinoMath.IsValidDouble(x: residual)
                    ? Fin.Succ(new SolveReceipt(
                        Solution: solved,
                        Path: SolvePath.DenseCholesky,
                        Stop: SolveStop.DirectSolved,
                        Rows: cholesky.Source.Rows,
                        Cols: cholesky.Source.Cols,
                        RhsLength: rhs.Count,
                        Iterations: Option<int>.None,
                        MaxIterations: Option<int>.None,
                        Tolerance: Option<double>.None,
                        Residual: residual,
                        FullRank: Some(true),
                        InputNonZeros: Option<int>.None,
                        FactorNonZeros: Option<int>.None))
                    : Fin.Fail<SolveReceipt>(key.InvalidResult());
            });
    }

    // --- [SPARSE_ASSEMBLY] ------------------------------------------------------------------
    internal static Fin<SparseMatrix> AssembleSparse(Dimension rows, Dimension cols, IEnumerable<(int Row, int Col, double Value)> triplets, Op op) {
        List<(int Row, int Col, double Value)> raw = [.. triplets];
        if (raw.Exists(t => !RhinoMath.IsValidDouble(t.Value) || t.Row < 0 || t.Row >= rows.Value || t.Col < 0 || t.Col >= cols.Value))
            return Fin.Fail<SparseMatrix>(op.InvalidInput());
        List<(int Row, int Col, double Value)> indexed = [.. raw
            .GroupBy(static t => (t.Row, t.Col))
            .Select(static g => (g.Key.Row, g.Key.Col, Value: g.Sum(static t => t.Value)))];
        SparseMatrixD mathNet = SparseMatrixD.OfIndexed(rows: rows.Value, columns: cols.Value, enumerable: indexed);
        List<(int Row, int Col, double Value)> sorted = [.. mathNet.EnumerateIndexed(Zeros.AllowSkip)
            .Where(static t => RhinoMath.IsValidDouble(x: t.Item3) && Math.Abs(value: t.Item3) > 0.0)
            .OrderBy(static t => t.Item1).ThenBy(static t => t.Item2)
            .Select(static t => (Row: t.Item1, Col: t.Item2, Value: t.Item3))];
        int[] rowPtr = new int[rows.Value + 1];
        int[] colInd = new int[sorted.Count];
        double[] values = new double[sorted.Count];
        int cursor = 0;
        for (int row = 0; row < rows.Value; row++) {
            rowPtr[row] = cursor;
            while (cursor < sorted.Count && sorted[cursor].Row == row) {
                colInd[cursor] = sorted[cursor].Col;
                values[cursor] = sorted[cursor].Value;
                cursor++;
            }
        }
        rowPtr[rows.Value] = cursor;
        return Fin.Succ(new SparseMatrix(Rows: rows, Cols: cols, RowPtr: new Arr<int>(rowPtr), ColInd: new Arr<int>(colInd), Values: new Arr<double>(values)));
    }
    internal static Fin<SparseHermitian> AssembleHermitian(Dimension order, IEnumerable<(int Row, int Col, Complex Value)> triplets, Op op) {
        List<(int Row, int Col, Complex Value)> raw = [.. triplets];
        if (raw.Exists(static t => !RhinoMath.IsValidDouble(t.Value.Real) || !RhinoMath.IsValidDouble(t.Value.Imaginary))
            || raw.Exists(t => t.Row < 0 || t.Col < 0 || t.Row >= order.Value || t.Col >= order.Value || t.Row > t.Col))
            return Fin.Fail<SparseHermitian>(op.InvalidInput());
        List<(int Row, int Col, Complex Value)> upper = [.. raw
            .GroupBy(static t => (t.Row, t.Col))
            .Select(static g => (g.Key.Row, g.Key.Col, Value: g.Aggregate(Complex.Zero, static (acc, t) => acc + t.Value)))
            .OrderBy(static t => t.Row).ThenBy(static t => t.Col)];
        int[] rowPtr = new int[order.Value + 1];
        int[] colInd = new int[upper.Count];
        Complex[] values = new Complex[upper.Count];
        int cursor = 0;
        for (int row = 0; row < order.Value; row++) {
            rowPtr[row] = cursor;
            while (cursor < upper.Count && upper[cursor].Row == row) {
                colInd[cursor] = upper[cursor].Col;
                values[cursor] = upper[cursor].Row == upper[cursor].Col ? new Complex(upper[cursor].Value.Real, 0.0) : upper[cursor].Value;
                cursor++;
            }
        }
        rowPtr[order.Value] = cursor;
        return Fin.Succ(new SparseHermitian(Order: order, RowPtr: new Arr<int>(rowPtr), ColInd: new Arr<int>(colInd), Values: new Arr<Complex>(values)));
    }

    // --- [SPARSE_OPERATIONS] ----------------------------------------------------------------
    internal static Arr<double> SparseMatVec(SparseMatrix self, Arr<double> x) =>
        ArrFromVector(ToMathNetSparse(s: self).Multiply(DenseVectorD.OfArray([.. x.AsIterable()])));
    internal static Arr<Complex> HermitianMatVec(SparseHermitian self, Arr<Complex> x) {
        int n = self.Order.Value;
        Complex[] y = new Complex[n];
        // Upper triangle stored; lower triangle = conjugate-transpose.
        for (int i = 0; i < n; i++) {
            for (int k = self.RowPtr[i]; k < self.RowPtr[i + 1]; k++) {
                int j = self.ColInd[k];
                Complex v = self.Values[k];
                y[i] += v * x[j];
                if (i != j) y[j] += Complex.Conjugate(v) * x[i];
            }
        }
        return new Arr<Complex>(y);
    }
    internal static Matrix SparseToDense(SparseMatrix self) =>
        FromMathNet(m: ToMathNetSparse(s: self), rows: self.Rows, cols: self.Cols);
    internal static Fin<SolveReceipt> SparseSolve(SparseMatrix matrix, Arr<double> rhs, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value || rhs.Count != matrix.Rows.Value || !rhs.All(RhinoMath.IsValidDouble)
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<double> A = ToMathNetSparse(s: matrix);
                LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
                MathNet.Numerics.LinearAlgebra.Double.Solvers.DiagonalPreconditioner preconditioner = new();
                preconditioner.Initialize(matrix: A);
                int iterationCap = Math.Max(val1: 64, val2: matrix.Rows.Value * 8);
                MathNet.Numerics.LinearAlgebra.Solvers.Iterator<double> iterator = new([
                    new MathNet.Numerics.LinearAlgebra.Solvers.FailureStopCriterion<double>(),
                    new MathNet.Numerics.LinearAlgebra.Solvers.DivergenceStopCriterion<double>(maximumRelativeIncrease: 1e3, minimumIterations: 8),
                    new MathNet.Numerics.LinearAlgebra.Solvers.ResidualStopCriterion<double>(maximum: RhinoMath.SqrtEpsilon, minimumIterationsBelowMaximum: 2),
                    new MathNet.Numerics.LinearAlgebra.Solvers.IterationCountStopCriterion<double>(maximumNumberOfIterations: iterationCap),
                ]);
                LinearVector iterative = A.SolveIterative(input: b, solver: new MathNet.Numerics.LinearAlgebra.Double.Solvers.BiCgStab(), iterator: iterator, preconditioner: preconditioner);
                double iterativeResidual = (b - A.Multiply(iterative)).L2Norm() / Math.Max(val1: 1.0, val2: b.L2Norm());
                bool iterativeConverged = RhinoMath.IsValidDouble(x: iterativeResidual) && iterativeResidual <= Math.Sqrt(RhinoMath.SqrtEpsilon);
                LinearVector x = iterativeConverged ? iterative : A.Solve(b);
                double residual = (b - A.Multiply(x)).L2Norm() / Math.Max(val1: 1.0, val2: b.L2Norm());
                return RhinoMath.IsValidDouble(x: residual) && residual <= Math.Sqrt(RhinoMath.SqrtEpsilon)
                    ? Fin.Succ(new SolveReceipt(
                        Solution: ArrFromVector(x),
                        Path: iterativeConverged ? SolvePath.SparseBiCgStabDiagonal : SolvePath.SparseMathNetQrFallback,
                        Stop: SolveStop.ResidualConverged,
                        Rows: matrix.Rows,
                        Cols: matrix.Cols,
                        RhsLength: rhs.Count,
                        Iterations: Option<int>.None,
                        MaxIterations: Some(iterationCap),
                        Tolerance: Some(RhinoMath.SqrtEpsilon),
                        Residual: residual,
                        FullRank: Option<bool>.None,
                        InputNonZeros: Some(matrix.NonZeros),
                        FactorNonZeros: Option<int>.None))
                    : Fin.Fail<SolveReceipt>(key.InvalidResult());
            });
    internal static Fin<Seq<(double Eigenvalue, Arr<double> Eigenvector)>> GeneralizedEigenpairs(SparseMatrix stiffness, SparseMatrix mass, int k, Op key) =>
        GeneralizedEigenpairsDetailed(stiffness: stiffness, mass: mass, k: k, key: key).Map(static receipt => receipt.Pairs);
    internal static Fin<EigenSolveReceipt<double, Arr<double>>> GeneralizedEigenpairsDetailed(SparseMatrix stiffness, SparseMatrix mass, int k, Op key) =>
        stiffness.Rows.Value != stiffness.Cols.Value || mass.Rows.Value != mass.Cols.Value || stiffness.Rows.Value != mass.Rows.Value || k < 1 || k >= stiffness.Rows.Value
            ? Fin.Fail<EigenSolveReceipt<double, Arr<double>>>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<double> stiffnessM = ToMathNetSparse(stiffness);
                Matrix<double> massM = ToMathNetSparse(mass);
                (LinearVector vals, Matrix<double> vecs) = SolveGeneralised(Ahat: stiffnessM, Mhat: massM);
                Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: vals.Count)
                    .OrderBy(i => vals[i])
                    .Take(k)
                    .Select(i => (Eigenvalue: vals[i], Eigenvector: ArrFromVector(vecs.Column(i)))));
                return Fin.Succ(new EigenSolveReceipt<double, Arr<double>>(
                    Pairs: pairs,
                    Path: EigenSolvePath.SparseGeneralizedCholeskyCongruence,
                    Stop: EigenSolveStop.DirectSolved,
                    RequestedPairs: k,
                    ReturnedPairs: pairs.Count,
                    Iterations: Option<int>.None,
                    MaxIterations: Option<int>.None,
                    Tolerance: Option<double>.None,
                    MaxResidual: GeneralizedEigenResidual(stiffness: stiffnessM, mass: massM, pairs: pairs)));
            });
    // --- [LOBPCG] ---------------------------------------------------------------------------
    // Knyazev 2001. Subspace span([X_i, R_i, P_i]) reduced via Rayleigh-Ritz;
    // first iteration omits the zero previous-direction block to avoid rank-deficient mass.
    internal static Fin<EigenSolveReceipt<double, Arr<double>>> Lobpcg(SparseMatrix matrix, int k, double tolerance, int maxIterations, Op key) {
        int n = matrix.Rows.Value;
        return !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value || k < 1 || k >= n || !RhinoMath.IsValidDouble(tolerance) || tolerance <= 0 || maxIterations < 1
            ? Fin.Fail<EigenSolveReceipt<double, Arr<double>>>(key.InvalidInput())
            : NormalizeSymmetricUpperEntries(s: matrix, key: key).Bind(_ => {
                Matrix<double> A = ToMathNetSparse(matrix);
                Matrix<double> X = OrthonormalRandom(rows: n, k: k, seed: RealInitialBasisSeed);
                LinearVector jacobi = ExtractDiagonalInverse(A);
                Matrix<double> P = DenseMatrixD.Create(n, k, 0.0);
                return Iterate(iter: 0, X: X, P: P);
                Fin<EigenSolveReceipt<double, Arr<double>>> Iterate(int iter, Matrix<double> X, Matrix<double> P) {
                    if (iter >= maxIterations) return Fin.Fail<EigenSolveReceipt<double, Arr<double>>>(key.InvalidResult());
                    Matrix<double> AX = A * X;
                    LinearVector lambda = RayleighQuotients(X: X, AX: AX);
                    Matrix<double> R = AX - (X * DenseMatrixD.OfDiagonalVector(lambda));
                    if (MaxColumnNorm(R) < tolerance) {
                        Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: k)
                            .Select(i => (Eigenvalue: lambda[i], Eigenvector: ArrFromVector(X.Column(i))))
                            .OrderBy(static p => p.Eigenvalue));
                        return Fin.Succ(new EigenSolveReceipt<double, Arr<double>>(
                            Pairs: pairs,
                            Path: EigenSolvePath.SparseLobpcg,
                            Stop: EigenSolveStop.ResidualConverged,
                            RequestedPairs: k,
                            ReturnedPairs: pairs.Count,
                            Iterations: Some(iter),
                            MaxIterations: Some(maxIterations),
                            Tolerance: Some(tolerance),
                            MaxResidual: RealEigenResidual(a: A, pairs: pairs)));
                    }
                    Matrix<double> W = ApplyJacobi(R: R, invDiag: jacobi);
                    bool hasPrevious = iter > 0 && MaxColumnNorm(P) > RhinoMath.SqrtEpsilon;
                    Matrix<double> S = AssembleSubspace(X: X, W: W, P: P, includePrevious: hasPrevious);
                    Matrix<double> Ahat = S.Transpose() * (A * S);
                    Matrix<double> Mhat = S.Transpose() * S;
                    Fin<(LinearVector Vals, Matrix<double> Vecs)> solved = key.Catch(() => Fin.Succ(SolveGeneralised(Ahat: Ahat, Mhat: Mhat)));
                    return solved.Bind(solution => {
                        Matrix<double> Z = TakeSmallest(eigVals: solution.Vals, eigVecs: solution.Vecs, k: k);
                        Matrix<double> Xnew = S * Z;
                        Matrix<double> previous = hasPrevious ? P * Z.SubMatrix(2 * k, k, 0, k) : DenseMatrixD.Create(n, k, 0.0);
                        Matrix<double> Pnew = (W * Z.SubMatrix(k, k, 0, k)) + previous;
                        return Iterate(iter: iter + 1, X: OrthonormaliseColumns(Xnew), P: Pnew);
                    });
                }
            });
    }
    internal static Fin<EigenSolveReceipt<double, Arr<Complex>>> LobpcgHermitian(SparseHermitian matrix, int k, double tolerance, int maxIterations, Op key) {
        int n = matrix.Order.Value;
        if (k < 1 || k >= n || !RhinoMath.IsValidDouble(tolerance) || tolerance <= 0 || maxIterations < 1)
            return Fin.Fail<EigenSolveReceipt<double, Arr<Complex>>>(key.InvalidInput());
        Matrix<Complex> A = ToMathNetHermitian(matrix);
        Matrix<Complex> X = OrthonormalRandomComplex(rows: n, k: k, seed: HermitianInitialBasisSeed);
        ComplexVector jacobi = ExtractDiagonalInverseComplex(A);
        Matrix<Complex> P = DenseMatrixC.Create(n, k, Complex.Zero);
        return Iterate(iter: 0, X: X, P: P);
        Fin<EigenSolveReceipt<double, Arr<Complex>>> Iterate(int iter, Matrix<Complex> X, Matrix<Complex> P) {
            if (iter >= maxIterations) return Fin.Fail<EigenSolveReceipt<double, Arr<Complex>>>(key.InvalidResult());
            Matrix<Complex> AX = A * X;
            ComplexVector lambda = RayleighQuotientsComplex(X: X, AX: AX);
            Matrix<Complex> R = AX - (X * DenseMatrixC.OfDiagonalVector(lambda));
            if (MaxColumnNormComplex(R) < tolerance) {
                Seq<(double Eigenvalue, Arr<Complex> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: k)
                    .Select(i => (Eigenvalue: lambda[i].Real, Eigenvector: ArrFromComplexVector(X.Column(i))))
                    .OrderBy(static p => p.Eigenvalue));
                return Fin.Succ(new EigenSolveReceipt<double, Arr<Complex>>(
                    Pairs: pairs,
                    Path: EigenSolvePath.SparseHermitianLobpcg,
                    Stop: EigenSolveStop.ResidualConverged,
                    RequestedPairs: k,
                    ReturnedPairs: pairs.Count,
                    Iterations: Some(iter),
                    MaxIterations: Some(maxIterations),
                    Tolerance: Some(tolerance),
                    MaxResidual: HermitianEigenResidual(a: A, pairs: pairs)));
            }
            Matrix<Complex> W = ApplyJacobiComplex(R: R, invDiag: jacobi);
            bool hasPrevious = iter > 0 && MaxColumnNormComplex(P) > RhinoMath.SqrtEpsilon;
            Matrix<Complex> S = AssembleSubspaceComplex(X: X, W: W, P: P, includePrevious: hasPrevious);
            Matrix<Complex> Ahat = S.ConjugateTranspose() * (A * S);
            Matrix<Complex> Mhat = S.ConjugateTranspose() * S;
            Fin<(ComplexVector Vals, Matrix<Complex> Vecs)> solved = key.Catch(() => Fin.Succ(SolveGeneralisedComplex(Ahat: Ahat, Mhat: Mhat)));
            return solved.Match(
                Succ: solution => {
                    Matrix<Complex> Z = TakeSmallestComplex(eigVals: solution.Vals, eigVecs: solution.Vecs, k: k);
                    Matrix<Complex> Xnew = S * Z;
                    Matrix<Complex> previous = hasPrevious ? P * Z.SubMatrix(2 * k, k, 0, k) : DenseMatrixC.Create(n, k, Complex.Zero);
                    Matrix<Complex> Pnew = (W * Z.SubMatrix(k, k, 0, k)) + previous;
                    return Iterate(iter: iter + 1, X: OrthonormaliseColumnsComplex(Xnew), P: Pnew);
                },
                Fail: Fin.Fail<EigenSolveReceipt<double, Arr<Complex>>>);
        }
    }

    // --- [LOBPCG_PRIMITIVES] ----------------------------------------------------------------
    private static Matrix<double> ToMathNetSparse(SparseMatrix s) =>
        DenseMatrixD.Build.Sparse(storage: SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            rows: s.Rows.Value,
            columns: s.Cols.Value,
            valueCount: s.Values.Count,
            rowPointers: [.. s.RowPtr.AsIterable()],
            columnIndices: [.. s.ColInd.AsIterable()],
            values: [.. s.Values.AsIterable()]));
    private static Matrix<Complex> ToMathNetHermitian(SparseHermitian s) {
        SparseMatrixC m = new(rows: s.Order.Value, columns: s.Order.Value);
        for (int i = 0; i < s.Order.Value; i++)
            for (int kIndex = s.RowPtr[i]; kIndex < s.RowPtr[i + 1]; kIndex++) {
                int j = s.ColInd[kIndex];
                Complex v = s.Values[kIndex];
                m[i, j] = v;
                if (i != j) m[j, i] = Complex.Conjugate(v);
            }
        return m;
    }
#pragma warning disable CA5394 // Random is sufficient for LOBPCG initial guess; not security-sensitive.
    private static Matrix<double> OrthonormalRandom(int rows, int k, int seed) {
        Random rng = new(seed);
        DenseMatrixD m = DenseMatrixD.Create(rows, k, (_, _) => (rng.NextDouble() * 2.0) - 1.0);
        return OrthonormaliseColumns(m);
    }
    private static Matrix<Complex> OrthonormalRandomComplex(int rows, int k, int seed) {
        Random rng = new(seed);
        DenseMatrixC m = DenseMatrixC.Create(rows, k, (_, _) => new Complex((rng.NextDouble() * 2.0) - 1.0, (rng.NextDouble() * 2.0) - 1.0));
        return OrthonormaliseColumnsComplex(m);
    }
#pragma warning restore CA5394
    // Modified Gram-Schmidt with one reorthogonalisation pass for numerical stability on
    // ill-conditioned X. Drops columns whose norm collapses to ~0 (linearly dependent on
    // earlier columns) by leaving them zero; caller must handle rank deficiency.
    private static Matrix<double> OrthonormaliseColumns(Matrix<double> m) {
        int n = m.RowCount;
        int k = m.ColumnCount;
        DenseMatrixD q = DenseMatrixD.Create(n, k, 0.0);
        for (int j = 0; j < k; j++) {
            LinearVector v = m.Column(j);
            for (int i = 0; i < j; i++) {
                double dot = q.Column(i).DotProduct(v);
                v -= q.Column(i) * dot;
            }
            double norm = v.L2Norm();
            if (norm > RhinoMath.SqrtEpsilon) q.SetColumn(j, v / norm);
        }
        return q;
    }
    private static Matrix<Complex> OrthonormaliseColumnsComplex(Matrix<Complex> m) {
        int n = m.RowCount;
        int k = m.ColumnCount;
        DenseMatrixC q = DenseMatrixC.Create(n, k, Complex.Zero);
        for (int j = 0; j < k; j++) {
            ComplexVector v = m.Column(j);
            for (int i = 0; i < j; i++) {
                Complex dot = q.Column(i).ConjugateDotProduct(v);
                v -= q.Column(i) * dot;
            }
            double norm = v.L2Norm();
            if (norm > RhinoMath.SqrtEpsilon) q.SetColumn(j, v / norm);
        }
        return q;
    }
    private static LinearVector ExtractDiagonalInverse(Matrix<double> A) {
        DenseVectorD inv = DenseVectorD.Create(A.RowCount, 1.0);
        for (int i = 0; i < A.RowCount; i++) {
            double d = A[i, i];
            inv[i] = Math.Abs(d) > RhinoMath.SqrtEpsilon ? 1.0 / d : 1.0;
        }
        return inv;
    }
    private static ComplexVector ExtractDiagonalInverseComplex(Matrix<Complex> A) {
        DenseVectorC inv = DenseVectorC.Create(A.RowCount, Complex.One);
        for (int i = 0; i < A.RowCount; i++) {
            Complex d = A[i, i];
            inv[i] = Complex.Abs(d) > RhinoMath.SqrtEpsilon ? Complex.One / d : Complex.One;
        }
        return inv;
    }
    private static LinearVector RayleighQuotients(Matrix<double> X, Matrix<double> AX) {
        DenseVectorD r = DenseVectorD.Create(X.ColumnCount, 0.0);
        for (int j = 0; j < X.ColumnCount; j++) r[j] = X.Column(j).DotProduct(AX.Column(j)) / Math.Max(X.Column(j).DotProduct(X.Column(j)), RhinoMath.ZeroTolerance);
        return r;
    }
    private static ComplexVector RayleighQuotientsComplex(Matrix<Complex> X, Matrix<Complex> AX) {
        DenseVectorC r = DenseVectorC.Create(X.ColumnCount, Complex.Zero);
        for (int j = 0; j < X.ColumnCount; j++) {
            Complex num = X.Column(j).ConjugateDotProduct(AX.Column(j));
            Complex den = X.Column(j).ConjugateDotProduct(X.Column(j));
            r[j] = Complex.Abs(den) > RhinoMath.ZeroTolerance ? num / den : Complex.Zero;
        }
        return r;
    }
    private static double MaxColumnNorm(Matrix<double> m) {
        double max = 0.0;
        for (int j = 0; j < m.ColumnCount; j++) max = Math.Max(max, m.Column(j).L2Norm());
        return max;
    }
    private static double MaxColumnNormComplex(Matrix<Complex> m) {
        double max = 0.0;
        for (int j = 0; j < m.ColumnCount; j++) max = Math.Max(max, m.Column(j).L2Norm());
        return max;
    }
    private static Matrix<double> ApplyJacobi(Matrix<double> R, LinearVector invDiag) {
        DenseMatrixD W = DenseMatrixD.Create(R.RowCount, R.ColumnCount, 0.0);
        for (int j = 0; j < R.ColumnCount; j++)
            for (int i = 0; i < R.RowCount; i++) W[i, j] = R[i, j] * invDiag[i];
        return W;
    }
    private static Matrix<Complex> ApplyJacobiComplex(Matrix<Complex> R, ComplexVector invDiag) {
        DenseMatrixC W = DenseMatrixC.Create(R.RowCount, R.ColumnCount, Complex.Zero);
        for (int j = 0; j < R.ColumnCount; j++)
            for (int i = 0; i < R.RowCount; i++) W[i, j] = R[i, j] * invDiag[i];
        return W;
    }
    private static Matrix<double> AssembleSubspace(Matrix<double> X, Matrix<double> W, Matrix<double> P, bool includePrevious) {
        int n = X.RowCount;
        int k = X.ColumnCount;
        DenseMatrixD S = DenseMatrixD.Create(n, includePrevious ? 3 * k : 2 * k, 0.0);
        S.SetSubMatrix(0, 0, X);
        S.SetSubMatrix(0, k, W);
        if (includePrevious) S.SetSubMatrix(0, 2 * k, P);
        return OrthonormaliseColumns(S);
    }
    private static Matrix<Complex> AssembleSubspaceComplex(Matrix<Complex> X, Matrix<Complex> W, Matrix<Complex> P, bool includePrevious) {
        int n = X.RowCount;
        int k = X.ColumnCount;
        DenseMatrixC S = DenseMatrixC.Create(n, includePrevious ? 3 * k : 2 * k, Complex.Zero);
        S.SetSubMatrix(0, 0, X);
        S.SetSubMatrix(0, k, W);
        if (includePrevious) S.SetSubMatrix(0, 2 * k, P);
        return OrthonormaliseColumnsComplex(S);
    }
    // Generalised eigenproblem A z = λ M z via the symmetric Cholesky congruence L⁻¹AL⁻ᵀ.
    private static (LinearVector Vals, Matrix<double> Vecs) SolveGeneralised(Matrix<double> Ahat, Matrix<double> Mhat) {
        MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> cholesky = Mhat.Cholesky();
        Matrix<double> reduced = CongruentReduce(factor: cholesky.Factor, matrix: Ahat);
        Matrix<double> sym = (reduced + reduced.Transpose()) * 0.5;
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = sym.Evd(Symmetricity.Symmetric);
        return (
            Vals: DenseVectorD.Create(evd.EigenValues.Count, i => evd.EigenValues[i].Real),
            Vecs: BackTransform(factor: cholesky.Factor, vectors: evd.EigenVectors));
    }
    private static (ComplexVector Vals, Matrix<Complex> Vecs) SolveGeneralisedComplex(Matrix<Complex> Ahat, Matrix<Complex> Mhat) {
        MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<Complex> cholesky = Mhat.Cholesky();
        Matrix<Complex> reduced = CongruentReduceComplex(factor: cholesky.Factor, matrix: Ahat);
        Matrix<Complex> herm = (reduced + reduced.ConjugateTranspose()) * 0.5;
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<Complex> evd = herm.Evd(Symmetricity.Hermitian);
        return (Vals: evd.EigenValues, Vecs: BackTransformComplex(factor: cholesky.Factor, vectors: evd.EigenVectors));
    }
    private static Matrix<double> CongruentReduce(Matrix<double> factor, Matrix<double> matrix) {
        int n = matrix.RowCount;
        Matrix<double> adjoint = factor.Transpose();
        Matrix<double> identity = DenseMatrixD.CreateIdentity(order: n);
        return factor.Solve(matrix * adjoint.Solve(identity));
    }
    private static Matrix<double> BackTransform(Matrix<double> factor, Matrix<double> vectors) {
        Matrix<double> adjoint = factor.Transpose();
        return adjoint.Solve(vectors);
    }
    private static Matrix<Complex> CongruentReduceComplex(Matrix<Complex> factor, Matrix<Complex> matrix) {
        int n = matrix.RowCount;
        Matrix<Complex> adjoint = factor.ConjugateTranspose();
        Matrix<Complex> identity = DenseMatrixC.CreateIdentity(order: n);
        return factor.Solve(matrix * adjoint.Solve(identity));
    }
    private static Matrix<Complex> BackTransformComplex(Matrix<Complex> factor, Matrix<Complex> vectors) {
        Matrix<Complex> adjoint = factor.ConjugateTranspose();
        return adjoint.Solve(vectors);
    }
    private static Matrix<double> TakeSmallest(LinearVector eigVals, Matrix<double> eigVecs, int k) {
        int[] sorted = [.. Enumerable.Range(0, eigVals.Count).OrderBy(i => eigVals[i]).Take(k)];
        DenseMatrixD Z = DenseMatrixD.Create(eigVecs.RowCount, k, 0.0);
        for (int idx = 0; idx < k; idx++) Z.SetColumn(idx, eigVecs.Column(sorted[idx]));
        return Z;
    }
    private static Matrix<Complex> TakeSmallestComplex(ComplexVector eigVals, Matrix<Complex> eigVecs, int k) {
        int[] sorted = [.. Enumerable.Range(0, eigVals.Count).OrderBy(i => eigVals[i].Real).Take(k)];
        DenseMatrixC Z = DenseMatrixC.Create(eigVecs.RowCount, k, Complex.Zero);
        for (int idx = 0; idx < k; idx++) Z.SetColumn(idx, eigVecs.Column(sorted[idx]));
        return Z;
    }
}

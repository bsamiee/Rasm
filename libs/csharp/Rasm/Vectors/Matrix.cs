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

// --- [TYPES] ------------------------------------------------------------------------------
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
    public static readonly EigenSolveStop DirectSolved = new(key: 0, isUsable: true), ResidualConverged = new(key: 1, isUsable: true), MaxIterationsExhausted = new(key: 2, isUsable: false);
    public bool IsUsable { get; }
}

[SmartEnum<int>]
public sealed partial class MatrixNormKind {
    public static readonly MatrixNormKind Frobenius = new(key: 0, compute: static m => MatrixKernel.ToMathNet(m).FrobeniusNorm());
    public static readonly MatrixNormKind MaxAbs = new(key: 1, compute: static m => MatrixKernel.ToMathNet(m).Enumerate().Aggregate(0.0, static (acc, e) => Math.Max(acc, Math.Abs(e))));
    public static readonly MatrixNormKind L1 = new(key: 2, compute: static m => MatrixKernel.ToMathNet(m).L1Norm());
    public static readonly MatrixNormKind LInf = new(key: 3, compute: static m => MatrixKernel.ToMathNet(m).InfinityNorm());
    [UseDelegateFromConstructor] internal partial double Compute(Matrix matrix);
}

[SmartEnum<int>]
public sealed partial class SolvePath {
    public static readonly SolvePath DenseLu = new(key: 0, isSparse: false, isFallback: false, usesDiagonalPreconditioner: false);
    public static readonly SolvePath DenseQrLeastSquares = new(key: 1, isSparse: false, isFallback: false, usesDiagonalPreconditioner: false);
    public static readonly SolvePath DenseCholesky = new(key: 2, isSparse: false, isFallback: false, usesDiagonalPreconditioner: false);
    public static readonly SolvePath SparseBiCgStabDiagonal = new(key: 3, isSparse: true, isFallback: false, usesDiagonalPreconditioner: true);
    public static readonly SolvePath SparseMathNetDirectFallback = new(key: 4, isSparse: true, isFallback: true, usesDiagonalPreconditioner: false);
    public static readonly SolvePath SparseCholesky = new(key: 5, isSparse: true, isFallback: false, usesDiagonalPreconditioner: false);
    public bool IsSparse { get; }
    public bool IsFallback { get; }
    public bool UsesDiagonalPreconditioner { get; }
}

[SmartEnum<int>]
public sealed partial class SolveStop {
    public static readonly SolveStop DirectSolved = new(key: 0, isUsable: true), LeastSquaresSolved = new(key: 1, isUsable: true), ResidualConverged = new(key: 2, isUsable: true), DirectFallbackSolved = new(key: 3, isUsable: true);
    public bool IsUsable { get; }
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct Matrix(Dimension Rows, Dimension Cols, Arr<double> Entries) {
    public bool IsValid => Entries.Count == Rows.Value * Cols.Value && Entries.All(RhinoMath.IsValidDouble);
    internal double At(int i, int j) => Entries[(i * Cols.Value) + j];
    internal Matrix With(int i, int j, double value) =>
        this with { Entries = Entries.SetItem((i * Cols.Value) + j, value) };
    public static Fin<Matrix> Of(Dimension rows, Dimension cols, Arr<double> entries, Op? key = null) =>
        from _ in guard(entries.Count == rows.Value * cols.Value && entries.All(RhinoMath.IsValidDouble), key.OrDefault().InvalidInput()).ToFin()
        select new Matrix(Rows: rows, Cols: cols, Entries: entries);
    public static Matrix Identity(Dimension dim) =>
        MatrixKernel.FromMathNet(m: DenseMatrixD.CreateIdentity(order: dim.Value), rows: dim, cols: dim);
    public Matrix Transpose() => MatrixKernel.FromMathNet(MatrixKernel.ToMathNet(this).Transpose(), Cols, Rows);
    public Fin<Matrix> Multiply(Matrix other, Op? key = null) =>
        !IsValid || !other.IsValid || Cols.Value != other.Rows.Value
            ? Fin.Fail<Matrix>(error: key.OrDefault().InvalidInput())
            : MatrixKernel.DenseResult(source: this, rows: Rows, cols: other.Cols, key: key.OrDefault(), project: left => left.Multiply(MatrixKernel.ToMathNet(other)));
    public Fin<EigenSolveReceipt<Complex, Arr<Complex>>> DecomposeEigenDetailed(Op? key = null) =>
        MatrixKernel.GeneralEigen(matrix: this, key: key.OrDefault());
    public Fin<SvdResult> DecomposeSvd(Op? key = null) => MatrixKernel.Svd(matrix: this, key: key.OrDefault());
    public Fin<LuResult> DecomposeLu(Op? key = null) => MatrixKernel.Lu(matrix: this, key: key.OrDefault());
    public Fin<QrResult> DecomposeQr(Op? key = null) => MatrixKernel.Qr(matrix: this, key: key.OrDefault());
    public double Frobenius => MatrixNormKind.Frobenius.Compute(matrix: this);
    public Fin<double> Norm(MatrixNormKind kind, Op? key = null) =>
        kind is null ? Fin.Fail<double>(error: key.OrDefault().InvalidInput()) : key.OrDefault().AcceptValue(value: kind.Compute(matrix: this));
    public Fin<double> Trace(Op? key = null) => Rows.Value != Cols.Value ? Fin.Fail<double>(key.OrDefault().InvalidInput()) : key.OrDefault().AcceptValue(value: MatrixKernel.ToMathNet(this).Trace());
    public Fin<double> Determinant(Op? key = null) =>
        MatrixKernel.Determinant(matrix: this, key: key.OrDefault());
    public Fin<double> Spectral(Op? key = null) =>
        DecomposeSvd(key: key.OrDefault()).Bind(svd => key.OrDefault().AcceptValue(value: svd.Sigma.IsEmpty ? 0.0 : svd.Sigma[0]));
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.Solve(matrix: this, rhs: rhs, key: key.OrDefault());
    public Fin<SolveReceipt> LeastSquaresDetailed(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.LeastSquares(matrix: this, rhs: rhs, key: key.OrDefault());
    public Fin<int> Rank(Op? key = null) =>
        DecomposeSvd(key: key.OrDefault()).Map(static svd => svd.Rank);
    public Fin<Matrix> Inverse(Op? key = null) =>
        Rows.Value != Cols.Value
            ? Fin.Fail<Matrix>(error: key.OrDefault().InvalidInput())
            : MatrixKernel.DenseResult(source: this, rows: Rows, cols: Cols, key: key.OrDefault(), project: static matrix => matrix.Inverse());
    public Fin<Matrix> PseudoInverse(Op? key = null) =>
        MatrixKernel.DenseResult(source: this, rows: Cols, cols: Rows, key: key.OrDefault(), project: static matrix => matrix.PseudoInverse());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SymmetricMatrix(Dimension Dimension, Arr<double> Upper) {
    public bool IsValid => Upper.Count == (Dimension.Value * (Dimension.Value + 1) / 2) && Upper.All(RhinoMath.IsValidDouble);
    internal double At(int i, int j) => Upper[FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j))];
    internal SymmetricMatrix With(int i, int j, double value) =>
        this with { Upper = Upper.SetItem(FlatIndex(n: Dimension.Value, i: Math.Min(val1: i, val2: j), j: Math.Max(val1: i, val2: j)), value) };
    private static int FlatIndex(int n, int i, int j) => (i * n) - (i * (i - 1) / 2) + (j - i);
    public static Fin<SymmetricMatrix> Of(Dimension dim, Arr<double> upper, Op? key = null) =>
        from _ in guard(upper.Count == dim.Value * (dim.Value + 1) / 2 && upper.All(RhinoMath.IsValidDouble), key.OrDefault().InvalidInput()).ToFin()
        select new SymmetricMatrix(Dimension: dim, Upper: upper);
    public Matrix ToDense() {
        SymmetricMatrix self = this;
        int dim = Dimension.Value;
        return new(Rows: Dimension, Cols: Dimension, Entries: [.. toSeq(Enumerable.Range(start: 0, count: dim * dim)).Map(idx => self.At(i: idx / dim, j: idx % dim))]);
    }
    public Fin<Seq<(double Eigenvalue, Arr<double> Eigenvector)>> DecomposeEigen(Op? key = null) =>
        DecomposeEigenDetailed(key: key.OrDefault()).Map(static receipt => receipt.Pairs);
    public Fin<EigenSolveReceipt<double, Arr<double>>> DecomposeEigenDetailed(Op? key = null) =>
        MatrixKernel.SymmetricEigen(matrix: this, key: key.OrDefault());
    public Fin<CholeskyResult> DecomposeCholesky(Op? key = null) =>
        MatrixKernel.Cholesky(matrix: this, key: key.OrDefault());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SparseMatrix(Dimension Rows, Dimension Cols, Arr<int> RowPtr, Arr<int> ColInd, Arr<double> Values) {
    public bool IsValid => RowPtr.Count == Rows.Value + 1 && ColInd.Count == Values.Count && Values.All(RhinoMath.IsValidDouble) && RowPtr[0] == 0 && RowPtr[Rows.Value] == Values.Count && RowPointersAreMonotone(RowPtr) && RowColumnsAreStrict(rowPtr: RowPtr, colInd: ColInd, minCol: static _ => 0, maxCol: Cols.Value);
    public int NonZeros => Values.Count;
    public static Fin<SparseMatrix> FromTriplets(Dimension rows, Dimension cols, IEnumerable<(int Row, int Col, double Value)> triplets, Op? key = null) { Op op = key.OrDefault(); return Optional(triplets).ToFin(op.InvalidInput()).Bind(active => MatrixKernel.AssembleSparse(rows: rows, cols: cols, triplets: active, op: op)); }
    public Fin<Arr<double>> Multiply(Arr<double> vector, Op? key = null) => !IsValid || vector.Count != Cols.Value || vector.AsIterable().Any(static value => !RhinoMath.IsValidDouble(x: value)) ? Fin.Fail<Arr<double>>(key.OrDefault().InvalidInput()) : MatrixKernel.SparseMatVec(self: this, x: vector, key: key.OrDefault());
    public Matrix ToDense() => MatrixKernel.SparseToDense(self: this);
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) =>
        SolveDetailed(rhs: rhs, key: key.OrDefault()).Map(static result => result.Solution);
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.SparseSolve(matrix: this, rhs: rhs, key: key.OrDefault());
    public Fin<EigenSolveReceipt<double, Arr<double>>> SmallestEigenpairsDetailed(int k, double tolerance, int maxIterations = 200, Op? key = null) =>
        MatrixKernel.Lobpcg(matrix: this, k: k, tolerance: tolerance, maxIterations: maxIterations, key: key.OrDefault());
    internal static bool RowPointersAreMonotone(Arr<int> rowPtr) =>
        toSeq(Enumerable.Range(start: 0, count: Math.Max(val1: 0, val2: rowPtr.Count - 1)))
            .ForAll(i => rowPtr[i] <= rowPtr[i + 1]);
    internal static bool RowColumnsAreStrict(Arr<int> rowPtr, Arr<int> colInd, Func<int, int> minCol, int maxCol) =>
        toSeq(Enumerable.Range(start: 0, count: Math.Max(val1: 0, val2: rowPtr.Count - 1)))
            .ForAll(row => toSeq(Enumerable.Range(start: rowPtr[row], count: rowPtr[row + 1] - rowPtr[row]))
                .Fold(initialState: (Ok: true, Prev: minCol(arg: row) - 1), f: (state, k) => (
                    Ok: state.Ok && colInd[k] >= minCol(arg: row) && colInd[k] < maxCol && colInd[k] > state.Prev,
                    Prev: colInd[k])).Ok);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SparseHermitian(Dimension Order, Arr<int> RowPtr, Arr<int> ColInd, Arr<Complex> Values) {
    public bool IsValid => RowPtr.Count == Order.Value + 1 && ColInd.Count == Values.Count && Values.All(static c => RhinoMath.IsValidDouble(c.Real) && RhinoMath.IsValidDouble(c.Imaginary)) && RowPtr[0] == 0 && RowPtr[Order.Value] == Values.Count && SparseMatrix.RowPointersAreMonotone(RowPtr) && SparseMatrix.RowColumnsAreStrict(rowPtr: RowPtr, colInd: ColInd, minCol: static row => row, maxCol: Order.Value) && DiagonalIsReal;
    public int NonZeros => Values.Count;
    public static Fin<SparseHermitian> FromTriplets(Dimension order, IEnumerable<(int Row, int Col, Complex Value)> upperTriplets, Op? key = null) { Op op = key.OrDefault(); return Optional(upperTriplets).ToFin(op.InvalidInput()).Bind(active => MatrixKernel.AssembleHermitian(order: order, triplets: active, op: op)); }
    public Fin<Arr<Complex>> Multiply(Arr<Complex> vector, Op? key = null) => !IsValid || vector.Count != Order.Value || vector.AsIterable().Any(static value => !RhinoMath.IsValidDouble(x: value.Real) || !RhinoMath.IsValidDouble(x: value.Imaginary)) ? Fin.Fail<Arr<Complex>>(key.OrDefault().InvalidInput()) : MatrixKernel.HermitianMatVec(self: this, x: vector, key: key.OrDefault());
    public Fin<EigenSolveReceipt<double, Arr<Complex>>> SmallestEigenpairsDetailed(int k, double tolerance, int maxIterations = 200, Op? key = null) =>
        MatrixKernel.LobpcgHermitian(matrix: this, k: k, tolerance: tolerance, maxIterations: maxIterations, key: key.OrDefault());
    private bool DiagonalIsReal {
        get {
            Dimension order = Order;
            Arr<int> rowPtr = RowPtr;
            Arr<int> colInd = ColInd;
            Arr<Complex> values = Values;
            return Enumerable.Range(start: 0, count: order.Value).All(row => Enumerable.Range(start: rowPtr[row], count: rowPtr[row + 1] - rowPtr[row])
                .All(k => colInd[k] != row || Math.Abs(value: values[k].Imaginary) <= RhinoMath.SqrtEpsilon));
        }
    }
}

public readonly record struct SolveReceipt(Arr<double> Solution, SolvePath Path, SolveStop Stop, Dimension Rows, Dimension Cols, int RhsLength, Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance, double Residual, Option<bool> FullRank, Option<int> InputNonZeros, Option<int> FactorNonZeros) {
    public bool IsUsable =>
        Stop.IsUsable
        && RhinoMath.IsValidDouble(x: Residual)
        && Solution.Count == Cols.Value
        && Solution.ForAll(RhinoMath.IsValidDouble);
}

// Upper-triangular storage; Multiply reconstructs the lower triangle by conjugate transpose.

public readonly record struct EigenSolveReceipt<TEigen, TVector>(Seq<(TEigen Eigenvalue, TVector Eigenvector)> Pairs, EigenSolvePath Path, EigenSolveStop Stop, int RequestedPairs, int ReturnedPairs, Option<int> Iterations, Option<int> MaxIterations, Option<double> Tolerance, double MaxResidual, Option<int> FactorNonZeros = default) {
    public bool IsUsable =>
        Stop.IsUsable
        && ReturnedPairs is > 0
        && ReturnedPairs <= RequestedPairs
        && Pairs.Count == ReturnedPairs
        && RhinoMath.IsValidDouble(x: MaxResidual);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CholeskyResult {
    internal CholeskyResult(Matrix l, Matrix source, MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> factor) { L = l; Source = source; Factor = factor; }
    public Matrix L { get; }
    public Matrix Source { get; }
    internal MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> Factor { get; }
    public bool IsValid => L.IsValid && Source.IsValid && L.Rows.Value == L.Cols.Value && Source.Rows.Value == Source.Cols.Value;
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.CholeskySolve(cholesky: this, rhs: rhs, key: key.OrDefault());
}

[BoundaryAdapter]
public sealed record CholeskySparse {
    private CholeskySparse(SparseMatrix source, CSparse.Double.Factorization.SparseCholesky factor, Dimension order) { Source = source; Factor = factor; Order = order; }
    public SparseMatrix Source { get; }
    internal CSparse.Double.Factorization.SparseCholesky Factor { get; }
    public Dimension Order { get; }
    public int FactorNonZeros => Factor.NonZerosCount;
    public bool IsValid => Source.IsValid && Factor.NonZerosCount > 0 && Order.Value > 0;
    public static Fin<CholeskySparse> Of(SparseMatrix symmetric, Op? key = null) =>
        symmetric.Rows.Value != symmetric.Cols.Value || !symmetric.IsValid
            ? Fin.Fail<CholeskySparse>(error: key.OrDefault().InvalidInput())
            : from csc in MatrixKernel.ToCSparseSymmetric(s: symmetric, key: key.OrDefault())
              from factor in key.OrDefault().Catch(() => {
                  CSparse.Double.Factorization.SparseCholesky factor = CSparse.Double.Factorization.SparseCholesky.Create(A: csc, order: CSparse.ColumnOrdering.MinimumDegreeAtPlusA);
                  return Fin.Succ(factor);
              })
              select new CholeskySparse(source: symmetric, factor: factor, order: symmetric.Rows);
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) =>
        SolveDetailed(rhs: rhs, key: key.OrDefault()).Map(static receipt => receipt.Solution);
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) =>
        !IsValid || !MatrixKernel.SolveInputIsValid(rows: Order.Value, rhs: rhs)
            ? Fin.Fail<SolveReceipt>(error: key.OrDefault().InvalidInput())
            : key.OrDefault().Catch(() => {
                double[] b = [.. rhs.AsIterable()];
                double[] x = new double[Order.Value];
                Factor.Solve(input: b.AsSpan(), result: x.AsSpan());
                Arr<double> solution = new(x);
                return MatrixKernel.SparseSymmetricResidual(matrix: Source, solution: solution, rhs: rhs, key: key.OrDefault()).Bind(residual =>
                    MatrixKernel.SolveSuccess(solution: solution, solutionLength: Order.Value, path: SolvePath.SparseCholesky, stop: SolveStop.DirectSolved, rows: Source.Rows, cols: Source.Cols, rhsLength: rhs.Count, residual: residual, key: key.OrDefault(), inputNonZeros: Some(Source.NonZeros), factorNonZeros: Some(Factor.NonZerosCount)));
            });
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct LuResult {
    internal LuResult(Matrix source, double determinant, MathNet.Numerics.LinearAlgebra.Factorization.LU<double> factor) { Source = source; Determinant = determinant; Factor = factor; }
    public Matrix Source { get; }
    public double Determinant { get; }
    internal MathNet.Numerics.LinearAlgebra.Factorization.LU<double> Factor { get; }
    public bool IsValid => Source.IsValid && RhinoMath.IsValidDouble(x: Determinant);
    public Fin<SolveReceipt> SolveDetailed(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.LuSolve(lu: this, rhs: rhs, key: key.OrDefault());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct QrResult(Matrix Q, Matrix R) { public bool IsValid => Q.IsValid && R.IsValid; }

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SvdResult(Matrix U, Arr<double> Sigma, Matrix V, int Rank) { public bool IsValid => U.IsValid && V.IsValid && Sigma.All(static value => RhinoMath.IsValidDouble(x: value) && value >= 0.0); }

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class MatrixKernel {
    private const int RealInitialBasisSeed = 17, HermitianInitialBasisSeed = 19;
    private static Fin<List<(int Row, int Col, double Value)>> NormalizeSymmetricUpperEntries(SparseMatrix s, Op key) {
        if (!s.IsValid || s.Rows.Value != s.Cols.Value) return Fin.Fail<List<(int Row, int Col, double Value)>>(key.InvalidInput());
        List<(int Row, int Col, double[] Values)> grouped = [.. Enumerable.Range(start: 0, count: s.Rows.Value)
            .SelectMany(row => Enumerable.Range(start: s.RowPtr[row], count: s.RowPtr[row + 1] - s.RowPtr[row])
                .Select(k => (Row: Math.Min(val1: row, val2: s.ColInd[k]), Col: Math.Max(val1: row, val2: s.ColInd[k]), Value: s.Values[k])))
            .GroupBy(static e => (e.Row, e.Col))
            .Select(static group => (group.Key.Row, group.Key.Col, Values: group.Select(static e => e.Value).ToArray()))];
        return grouped.Exists(static group => group.Values.Any(value => Math.Abs(value - group.Values[0]) > RhinoMath.SqrtEpsilon * Math.Max(val1: 1.0, val2: Math.Max(val1: Math.Abs(value), val2: Math.Abs(group.Values[0])))))
            ? Fin.Fail<List<(int Row, int Col, double Value)>>(key.InvalidInput())
            : Fin.Succ(grouped
            .Select(static group => (group.Row, group.Col, Value: group.Values[0]))
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
    internal static Fin<Matrix> DenseResult(Matrix source, Dimension rows, Dimension cols, Op key, Func<Matrix<double>, Matrix<double>> project) =>
        !source.IsValid
            ? Fin.Fail<Matrix>(error: key.InvalidInput())
            : key.Catch(() => {
                Matrix result = FromMathNet(m: project(arg: ToMathNet(source)), rows: rows, cols: cols);
                return result.IsValid ? Fin.Succ(result) : Fin.Fail<Matrix>(key.InvalidResult());
            });
    private static DenseMatrixC ToMathNetComplex(Matrix m) =>
        (DenseMatrixC)DenseMatrixC.Build.Dense(m.Rows.Value, m.Cols.Value, (i, j) => new Complex(m.At(i: i, j: j), 0.0));
    private static Matrix<Complex> ToMathNetHermitian(SparseHermitian s) =>
        SparseMatrixC.OfIndexed(rows: s.Order.Value, columns: s.Order.Value, enumerable: Enumerable.Range(start: 0, count: s.Order.Value)
            .SelectMany(row => Enumerable.Range(start: s.RowPtr[row], count: s.RowPtr[row + 1] - s.RowPtr[row])
                .SelectMany(k => row == s.ColInd[k]
                    ? [(row, row, s.Values[k])]
                    : new[] { (row, s.ColInd[k], s.Values[k]), (s.ColInd[k], row, Complex.Conjugate(s.Values[k])) })));
    private static Matrix<double> ToMathNetSparse(SparseMatrix s) =>
        DenseMatrixD.Build.Sparse(storage: SparseCompressedRowMatrixStorage<double>.OfCompressedSparseRowFormat(
            rows: s.Rows.Value,
            columns: s.Cols.Value,
            valueCount: s.Values.Count,
            rowPointers: [.. s.RowPtr.AsIterable()],
            columnIndices: [.. s.ColInd.AsIterable()],
            values: [.. s.Values.AsIterable()]));
    private static Arr<double> ArrFromVector(LinearVector v) => new(v.ToArray());
    private static Arr<Complex> ArrFromComplexVector(ComplexVector v) => new(v.ToArray());
    private static double RelativeResidual(Matrix<double> a, LinearVector x, LinearVector b) =>
        (b - a.Multiply(x)).L2Norm() / Math.Max(val1: 1.0, val2: b.L2Norm());
    internal static bool SolveInputIsValid(int rows, Arr<double> rhs) =>
        rhs.Count == rows && rhs.All(RhinoMath.IsValidDouble);
    private static Fin<SolveReceipt> DenseSolveGated(Matrix source, Arr<double> rhs, Op key, bool square, Func<Matrix, Arr<double>, Op, Fin<SolveReceipt>> solve) =>
        !source.IsValid || !SolveInputIsValid(rows: source.Rows.Value, rhs: rhs) || (square && source.Rows.Value != source.Cols.Value)
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : solve(source, rhs, key);
    internal static double SparseResidual(SparseMatrix matrix, Arr<double> solution, Arr<double> rhs) =>
        RelativeResidual(a: ToMathNetSparse(s: matrix), x: DenseVectorD.OfArray([.. solution.AsIterable()]), b: DenseVectorD.OfArray([.. rhs.AsIterable()]));
    internal static Fin<double> SparseSymmetricResidual(SparseMatrix matrix, Arr<double> solution, Arr<double> rhs, Op key) =>
        NormalizeSymmetricUpperEntries(s: matrix, key: key).Bind(upper => key.AcceptValue(value: RelativeResidual(
            a: ToMathNetSymmetric(matrix: matrix, upper: upper),
            x: DenseVectorD.OfArray([.. solution.AsIterable()]),
            b: DenseVectorD.OfArray([.. rhs.AsIterable()]))));
    private static Matrix<double> ToMathNetSymmetric(SparseMatrix matrix, IEnumerable<(int Row, int Col, double Value)> upper) =>
        SparseMatrixD.OfIndexed(rows: matrix.Rows.Value, columns: matrix.Cols.Value, enumerable: upper.SelectMany(static e => e.Row == e.Col
            ? [(e.Row, e.Col, e.Value)]
            : new[] { (e.Row, e.Col, e.Value), (e.Col, e.Row, e.Value) }));
    private static double RealEigenResidual(Matrix<double> a, Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs) =>
        EigenResidual(a: a, pairs: pairs, vector: static v => DenseVectorD.OfArray([.. v.AsIterable()]), scale: static pair => pair.Eigenvalue * pair.Vector);
    private static double GeneralizedEigenResidual(Matrix<double> stiffness, Matrix<double> mass, Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs) =>
        pairs.Fold(initialState: 0.0, f: (max, pair) => {
            LinearVector v = DenseVectorD.OfArray([.. pair.Eigenvector.AsIterable()]);
            LinearVector lhs = stiffness.Multiply(v);
            LinearVector rhs = mass.Multiply(v) * pair.Eigenvalue;
            double residual = (lhs - rhs).L2Norm() / Math.Max(val1: 1.0, val2: lhs.L2Norm());
            return Math.Max(val1: max, val2: residual);
        });
    private static double ComplexEigenResidual(Matrix<Complex> a, Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)> pairs) =>
        EigenResidual(a: a, pairs: pairs, vector: static v => DenseVectorC.OfArray([.. v.AsIterable()]), scale: static pair => pair.Vector * pair.Eigenvalue);
    private static double HermitianEigenResidual(Matrix<Complex> a, Seq<(double Eigenvalue, Arr<Complex> Eigenvector)> pairs) =>
        EigenResidual(a: a, pairs: pairs, vector: static v => DenseVectorC.OfArray([.. v.AsIterable()]), scale: static pair => pair.Vector * pair.Eigenvalue);
    private static double EigenResidual<T, TEigen, TVector>(Matrix<T> a, Seq<(TEigen Eigenvalue, TVector Eigenvector)> pairs, Func<TVector, MathNet.Numerics.LinearAlgebra.Vector<T>> vector, Func<(MathNet.Numerics.LinearAlgebra.Vector<T> Vector, TEigen Eigenvalue), MathNet.Numerics.LinearAlgebra.Vector<T>> scale)
        where T : struct, IEquatable<T>, IFormattable =>
        pairs.Fold(initialState: 0.0, f: (max, pair) => {
            MathNet.Numerics.LinearAlgebra.Vector<T> v = vector(arg: pair.Eigenvector);
            double residual = (a.Multiply(v) - scale(arg: (v, pair.Eigenvalue))).L2Norm() / Math.Max(val1: 1.0, val2: v.L2Norm());
            return Math.Max(val1: max, val2: residual);
        });
    internal static Fin<SolveReceipt> SolveSuccess(Arr<double> solution, int solutionLength, SolvePath path, SolveStop stop, Dimension rows, Dimension cols, int rhsLength, double residual, Op key, double residualCap = double.PositiveInfinity, Option<int> iterations = default, Option<int> maxIterations = default, Option<double> tolerance = default, Option<bool> fullRank = default, Option<int> inputNonZeros = default, Option<int> factorNonZeros = default) =>
        solution.Count == solutionLength && solution.ForAll(RhinoMath.IsValidDouble) && RhinoMath.IsValidDouble(x: residual) && residual <= residualCap
            ? Fin.Succ(new SolveReceipt(Solution: solution, Path: path, Stop: stop, Rows: rows, Cols: cols, RhsLength: rhsLength, Iterations: iterations, MaxIterations: maxIterations, Tolerance: tolerance, Residual: residual, FullRank: fullRank, InputNonZeros: inputNonZeros, FactorNonZeros: factorNonZeros))
            : Fin.Fail<SolveReceipt>(key.InvalidResult());
    private static Fin<EigenSolveReceipt<TEigen, TVector>> EigenReceiptOf<TEigen, TVector>(Seq<(TEigen Eigenvalue, TVector Eigenvector)> pairs, EigenSolvePath path, EigenSolveStop stop, int requestedPairs, double maxResidual, Op key, Option<int> expectedVectorLength = default, Option<int> iterations = default, Option<int> maxIterations = default, Option<double> tolerance = default, Option<int> factorNonZeros = default) =>
        requestedPairs >= 1 && pairs.Count is > 0 && pairs.Count <= requestedPairs && RhinoMath.IsValidDouble(x: maxResidual) && pairs.ForAll(pair => EigenvalueIsFinite(value: pair.Eigenvalue) && EigenvectorIsFinite(vector: pair.Eigenvector, expectedLength: expectedVectorLength))
            ? Fin.Succ(new EigenSolveReceipt<TEigen, TVector>(Pairs: pairs, Path: path, Stop: stop, RequestedPairs: requestedPairs, ReturnedPairs: pairs.Count, Iterations: iterations, MaxIterations: maxIterations, Tolerance: tolerance, MaxResidual: maxResidual, FactorNonZeros: factorNonZeros))
            : Fin.Fail<EigenSolveReceipt<TEigen, TVector>>(key.InvalidResult());
    private static Fin<EigenSolveReceipt<double, TVector>> LobpcgReceiptOf<TVector>(Seq<(double Eigenvalue, TVector Eigenvector)> pairs, EigenSolvePath path, EigenSolveStop stop, int requestedPairs, double maxResidual, int iterations, int maxIterations, double tolerance, Op key) =>
        EigenReceiptOf(pairs: pairs, path: path, stop: stop, requestedPairs: requestedPairs, maxResidual: maxResidual, key: key, expectedVectorLength: pairs.IsEmpty ? Option<int>.None : Some(VectorLengthOf(vector: pairs[0].Eigenvector)), iterations: Some(iterations), maxIterations: Some(maxIterations), tolerance: Some(tolerance));
    private static bool EigenvalueIsFinite<TEigen>(TEigen value) =>
        value switch {
            double real => RhinoMath.IsValidDouble(x: real),
            Complex complex => RhinoMath.IsValidDouble(x: complex.Real) && RhinoMath.IsValidDouble(x: complex.Imaginary),
            _ => false,
        };
    private static bool EigenvectorIsFinite<TVector>(TVector vector, Option<int> expectedLength) =>
        vector switch {
            Arr<double> real => VectorLengthMatches(count: real.Count, expectedLength: expectedLength) && real.ForAll(RhinoMath.IsValidDouble) && real.AsIterable().Any(static value => Math.Abs(value: value) > RhinoMath.SqrtEpsilon),
            Arr<Complex> complex => VectorLengthMatches(count: complex.Count, expectedLength: expectedLength) && complex.ForAll(static value => RhinoMath.IsValidDouble(x: value.Real) && RhinoMath.IsValidDouble(x: value.Imaginary)) && complex.AsIterable().Any(static value => value.Magnitude > RhinoMath.SqrtEpsilon),
            _ => false,
        };
    private static bool VectorLengthMatches(int count, Option<int> expectedLength) =>
        count > 0 && expectedLength.Map(expected => expected == count).IfNone(noneValue: true);
    private static int VectorLengthOf<TVector>(TVector vector) =>
        vector switch { Arr<double> real => real.Count, Arr<Complex> complex => complex.Count, _ => 0 };

    // --- [DENSE_DECOMPOSITIONS] -------------------------------------------------------------
    internal static Fin<SvdResult> Svd(Matrix matrix, Op key) => !matrix.IsValid ? Fin.Fail<SvdResult>(key.InvalidInput()) : key.Catch(() => {
        MathNet.Numerics.LinearAlgebra.Factorization.Svd<double> svd = ToMathNet(matrix).Svd(computeVectors: true);
        SvdResult result = new(U: FromMathNet(svd.U, matrix.Rows, matrix.Rows), Sigma: ArrFromVector(svd.S), V: FromMathNet(svd.VT.Transpose(), matrix.Cols, matrix.Cols), Rank: svd.Rank);
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<SvdResult>(key.InvalidResult());
    });
    internal static Fin<LuResult> Lu(Matrix matrix, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<LuResult>(key.InvalidInput())
            : key.Catch(() => {
                MathNet.Numerics.LinearAlgebra.Factorization.LU<double> lu = ToMathNet(matrix).LU();
                LuResult result = new(source: matrix, determinant: lu.Determinant, factor: lu);
                return result.IsValid ? Fin.Succ(result) : Fin.Fail<LuResult>(key.InvalidResult());
            });
    internal static Fin<QrResult> Qr(Matrix matrix, Op key) => !matrix.IsValid ? Fin.Fail<QrResult>(key.InvalidInput()) : key.Catch(() => {
        MathNet.Numerics.LinearAlgebra.Factorization.QR<double> qr = ToMathNet(matrix).QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Full);
        QrResult result = new(Q: FromMathNet(qr.Q, matrix.Rows, matrix.Rows), R: FromMathNet(qr.R, matrix.Rows, matrix.Cols));
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<QrResult>(key.InvalidResult());
    });
    internal static Fin<CholeskyResult> Cholesky(SymmetricMatrix matrix, Op key) =>
        !matrix.IsValid ? Fin.Fail<CholeskyResult>(key.InvalidInput()) : key.Catch(() => {
            Matrix source = matrix.ToDense();
            MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> factor = ToMathNet(source).Cholesky();
            CholeskyResult result = new(l: FromMathNet(factor.Factor, matrix.Dimension, matrix.Dimension), source: source, factor: factor);
            return result.IsValid ? Fin.Succ(result) : Fin.Fail<CholeskyResult>(key.InvalidResult());
        });
    internal static Fin<EigenSolveReceipt<double, Arr<double>>> SymmetricEigen(SymmetricMatrix matrix, Op key) =>
        !matrix.IsValid
            ? Fin.Fail<EigenSolveReceipt<double, Arr<double>>>(key.InvalidInput())
            : key.Catch(() => {
                Matrix dense = matrix.ToDense();
                Matrix<double> mathNet = ToMathNet(dense);
                MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = mathNet.Evd(Symmetricity.Symmetric);
                int n = matrix.Dimension.Value;
                Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: n)
                    .Select(i => (Eigenvalue: evd.EigenValues[i].Real, Eigenvector: ArrFromVector(evd.EigenVectors.Column(i))))
                    .OrderByDescending(static p => Math.Abs(p.Eigenvalue)));
                return EigenReceiptOf(pairs: pairs, path: EigenSolvePath.DenseSymmetricEvd, stop: EigenSolveStop.DirectSolved, requestedPairs: n, maxResidual: RealEigenResidual(a: mathNet, pairs: pairs), key: key);
            });
    internal static Fin<EigenSolveReceipt<Complex, Arr<Complex>>> GeneralEigen(Matrix matrix, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<EigenSolveReceipt<Complex, Arr<Complex>>>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<Complex> mathNet = ToMathNetComplex(matrix);
                MathNet.Numerics.LinearAlgebra.Factorization.Evd<Complex> evd = mathNet.Evd(Symmetricity.Asymmetric);
                int n = matrix.Rows.Value;
                Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: n)
                    .Select(i => (Eigenvalue: evd.EigenValues[i], Eigenvector: ArrFromComplexVector(evd.EigenVectors.Column(i)))));
                return EigenReceiptOf(pairs: pairs, path: EigenSolvePath.DenseGeneralEvd, stop: EigenSolveStop.DirectSolved, requestedPairs: n, maxResidual: ComplexEigenResidual(a: mathNet, pairs: pairs), key: key);
            });
    internal static Fin<SolveReceipt> Solve(Matrix matrix, Arr<double> rhs, Op key) =>
        DenseSolveGated(source: matrix, rhs: rhs, key: key, square: true, solve: static (source, right, op) => Lu(matrix: source, key: op).Bind(lu => LuSolve(lu: lu, rhs: right, key: op)));
    internal static Fin<SolveReceipt> LeastSquares(Matrix matrix, Arr<double> rhs, Op key) =>
        DenseSolveGated(source: matrix, rhs: rhs, key: key, square: false, solve: static (source, right, op) => op.Catch(() => {
            Matrix<double> design = ToMathNet(source);
            MathNet.Numerics.LinearAlgebra.Factorization.QR<double> qr = design.QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Full);
            return DenseSolve(source: source, rhs: right, key: op, path: SolvePath.DenseQrLeastSquares, stop: SolveStop.LeastSquaresSolved, solve: new Func<LinearVector, LinearVector>(qr.Solve), fullRank: Some(qr.IsFullRank));
        }));
    internal static Fin<SolveReceipt> LuSolve(LuResult lu, Arr<double> rhs, Op key) =>
        !lu.IsValid
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : DenseSolveGated(source: lu.Source, rhs: rhs, key: key, square: true, solve: (_, right, op) => DenseSolve(source: lu.Source, rhs: right, key: op, path: SolvePath.DenseLu, stop: SolveStop.DirectSolved, solve: new Func<LinearVector, LinearVector>(lu.Factor.Solve), residualCap: Math.Sqrt(RhinoMath.SqrtEpsilon)));
    internal static Fin<double> Determinant(Matrix matrix, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<double>(error: key.InvalidInput())
            : key.Catch(() => key.AcceptValue(value: ToMathNet(matrix).Determinant()));
    // --- [DENSE_CHOLESKY_SOLVE] -------------------------------------------------------------
    internal static Fin<SolveReceipt> CholeskySolve(CholeskyResult cholesky, Arr<double> rhs, Op key) =>
        !cholesky.IsValid
            ? Fin.Fail<SolveReceipt>(key.InvalidInput())
            : DenseSolveGated(source: cholesky.Source, rhs: rhs, key: key, square: true, solve: (_, right, op) => DenseSolve(source: cholesky.Source, rhs: right, key: op, path: SolvePath.DenseCholesky, stop: SolveStop.DirectSolved, solve: new Func<LinearVector, LinearVector>(cholesky.Factor.Solve), fullRank: Some(value: true), residualCap: Math.Sqrt(RhinoMath.SqrtEpsilon)));
    private static Fin<SolveReceipt> DenseSolve(Matrix source, Arr<double> rhs, Op key, SolvePath path, SolveStop stop, Func<LinearVector, LinearVector> solve, Option<bool> fullRank = default, double residualCap = double.PositiveInfinity) =>
        key.Catch(() => {
            Matrix<double> a = ToMathNet(source);
            LinearVector b = DenseVectorD.OfArray([.. rhs.AsIterable()]);
            LinearVector x = solve(arg: b);
            return SolveSuccess(solution: ArrFromVector(x), solutionLength: source.Cols.Value, path: path, stop: stop, rows: source.Rows, cols: source.Cols, rhsLength: rhs.Count, residual: RelativeResidual(a: a, x: x, b: b), key: key, residualCap: residualCap, fullRank: fullRank);
        });

    // --- [SPARSE_ASSEMBLY] ------------------------------------------------------------------
    internal static Fin<SparseMatrix> AssembleSparse(Dimension rows, Dimension cols, IEnumerable<(int Row, int Col, double Value)> triplets, Op op) {
        List<(int Row, int Col, double Value)> raw = [.. triplets];
        if (raw.Exists(t => !RhinoMath.IsValidDouble(t.Value) || t.Row < 0 || t.Row >= rows.Value || t.Col < 0 || t.Col >= cols.Value)) return Fin.Fail<SparseMatrix>(op.InvalidInput());
        List<(int Row, int Col, double Value)> indexed = [.. raw
            .GroupBy(static t => (t.Row, t.Col))
            .Select(static g => (g.Key.Row, g.Key.Col, Value: g.Sum(static t => t.Value)))];
        if (indexed.Exists(static t => !RhinoMath.IsValidDouble(x: t.Value))) return Fin.Fail<SparseMatrix>(op.InvalidResult());
        SparseMatrixD mathNet = SparseMatrixD.OfIndexed(rows: rows.Value, columns: cols.Value, enumerable: indexed);
        List<(int Row, int Col, double Value)> sorted = [.. mathNet.EnumerateIndexed(Zeros.AllowSkip)
            .Where(static t => RhinoMath.IsValidDouble(x: t.Item3) && Math.Abs(value: t.Item3) > 0.0)
            .OrderBy(static t => t.Item1).ThenBy(static t => t.Item2)
            .Select(static t => (Row: t.Item1, Col: t.Item2, Value: t.Item3))];
        (Arr<int> rowPtr, Arr<int> colInd, Arr<double> values) = CompressRows(rowCount: rows.Value, sorted: sorted);
        SparseMatrix result = new(Rows: rows, Cols: cols, RowPtr: rowPtr, ColInd: colInd, Values: values);
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<SparseMatrix>(op.InvalidResult());
    }
    internal static Fin<SparseHermitian> AssembleHermitian(Dimension order, IEnumerable<(int Row, int Col, Complex Value)> triplets, Op op) {
        List<(int Row, int Col, Complex Value)> raw = [.. triplets];
        if (raw.Exists(static t => !RhinoMath.IsValidDouble(t.Value.Real) || !RhinoMath.IsValidDouble(t.Value.Imaginary)) || raw.Exists(t => t.Row < 0 || t.Col < 0 || t.Row >= order.Value || t.Col >= order.Value || t.Row > t.Col) || raw.Exists(static t => t.Row == t.Col && Math.Abs(value: t.Value.Imaginary) > RhinoMath.SqrtEpsilon)) return Fin.Fail<SparseHermitian>(op.InvalidInput());
        List<(int Row, int Col, Complex Value)> upper = [.. raw
            .GroupBy(static t => (t.Row, t.Col))
            .Select(static g => (g.Key.Row, g.Key.Col, Value: g.Aggregate(Complex.Zero, static (acc, t) => acc + t.Value)))
            .OrderBy(static t => t.Row).ThenBy(static t => t.Col)];
        if (upper.Exists(static t => !RhinoMath.IsValidDouble(x: t.Value.Real) || !RhinoMath.IsValidDouble(x: t.Value.Imaginary) || (t.Row == t.Col && Math.Abs(value: t.Value.Imaginary) > RhinoMath.SqrtEpsilon))) return Fin.Fail<SparseHermitian>(op.InvalidResult());
        (Arr<int> rowPtr, Arr<int> colInd, Arr<Complex> values) = CompressRows(rowCount: order.Value, sorted: [.. upper.Select(static t => (t.Row, t.Col, Value: t.Row == t.Col ? new Complex(t.Value.Real, 0.0) : t.Value))]);
        SparseHermitian result = new(Order: order, RowPtr: rowPtr, ColInd: colInd, Values: values);
        return result.IsValid ? Fin.Succ(result) : Fin.Fail<SparseHermitian>(op.InvalidResult());
    }
    private static (Arr<int> RowPtr, Arr<int> ColInd, Arr<T> Values) CompressRows<T>(int rowCount, List<(int Row, int Col, T Value)> sorted) {
        int[] rowPtr = new int[rowCount + 1];
        int[] colInd = new int[sorted.Count];
        T[] values = new T[sorted.Count];
        int cursor = 0;
        for (int row = 0; row < rowCount; row++) {
            rowPtr[row] = cursor;
            while (cursor < sorted.Count && sorted[cursor].Row == row) {
                colInd[cursor] = sorted[cursor].Col;
                values[cursor] = sorted[cursor].Value;
                cursor++;
            }
        }
        rowPtr[rowCount] = cursor;
        return (new Arr<int>(rowPtr), new Arr<int>(colInd), new Arr<T>(values));
    }

    // --- [SPARSE_OPERATIONS] ----------------------------------------------------------------
    internal static Fin<Arr<double>> SparseMatVec(SparseMatrix self, Arr<double> x, Op key) =>
        ArrFromVector(ToMathNetSparse(s: self).Multiply(DenseVectorD.OfArray([.. x.AsIterable()]))) switch {
            Arr<double> result when result.ForAll(RhinoMath.IsValidDouble) => Fin.Succ(result),
            _ => Fin.Fail<Arr<double>>(key.InvalidResult()),
        };
    internal static void AddHermitianRealBlockTriplets(List<(int Row, int Col, double Value)> triplets, int order, int i, int j, double real, double imaginary, double diagonal) =>
        triplets.AddRange([
            (i, i, diagonal), (j, j, diagonal), (i + order, i + order, diagonal), (j + order, j + order, diagonal),
            (i, j, real), (j, i, real), (i + order, j + order, real), (j + order, i + order, real),
            (i, j + order, -imaginary), (j + order, i, -imaginary), (i + order, j, imaginary), (j, i + order, imaginary),
        ]);
    internal static Fin<Arr<Complex>> HermitianMatVec(SparseHermitian self, Arr<Complex> x, Op key) =>
        ArrFromComplexVector(ToMathNetHermitian(s: self).Multiply(DenseVectorC.OfArray([.. x.AsIterable()]))) switch {
            Arr<Complex> result when result.ForAll(static value => RhinoMath.IsValidDouble(x: value.Real) && RhinoMath.IsValidDouble(x: value.Imaginary)) => Fin.Succ(result),
            _ => Fin.Fail<Arr<Complex>>(key.InvalidResult()),
        };
    internal static Matrix SparseToDense(SparseMatrix self) =>
        FromMathNet(m: ToMathNetSparse(s: self), rows: self.Rows, cols: self.Cols);
    internal static Fin<SolveReceipt> SparseSolve(SparseMatrix matrix, Arr<double> rhs, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value || !SolveInputIsValid(rows: matrix.Rows.Value, rhs: rhs)
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
                double iterativeResidual = RelativeResidual(a: A, x: iterative, b: b);
                bool iteratorConverged = iterator.Status == MathNet.Numerics.LinearAlgebra.Solvers.IterationStatus.Converged;
                bool iterativeConverged = iteratorConverged && RhinoMath.IsValidDouble(x: iterativeResidual) && iterativeResidual <= RhinoMath.SqrtEpsilon;
                LinearVector x = iterativeConverged ? iterative : A.Solve(b);
                double residual = RelativeResidual(a: A, x: x, b: b);
                return SolveSuccess(solution: ArrFromVector(x), solutionLength: matrix.Cols.Value, path: iterativeConverged ? SolvePath.SparseBiCgStabDiagonal : SolvePath.SparseMathNetDirectFallback, stop: iterativeConverged ? SolveStop.ResidualConverged : SolveStop.DirectFallbackSolved, rows: matrix.Rows, cols: matrix.Cols, rhsLength: rhs.Count, residual: residual, key: key, residualCap: Math.Sqrt(RhinoMath.SqrtEpsilon), maxIterations: Some(iterationCap), tolerance: Some(RhinoMath.SqrtEpsilon), inputNonZeros: Some(matrix.NonZeros));
            });
    internal static Fin<EigenSolveReceipt<double, Arr<double>>> GeneralizedEigenpairsDetailed(SparseMatrix stiffness, SparseMatrix mass, int k, Op key) =>
        !stiffness.IsValid || !mass.IsValid || stiffness.Rows.Value != stiffness.Cols.Value || mass.Rows.Value != mass.Cols.Value || stiffness.Rows.Value != mass.Rows.Value || k < 1 || k >= stiffness.Rows.Value
            ? Fin.Fail<EigenSolveReceipt<double, Arr<double>>>(key.InvalidInput())
            : from stiffnessUpper in NormalizeSymmetricUpperEntries(s: stiffness, key: key)
              from massUpper in NormalizeSymmetricUpperEntries(s: mass, key: key)
              from receipt in key.Catch(() => {
                  Matrix<double> stiffnessM = ToMathNetSymmetric(matrix: stiffness, upper: stiffnessUpper);
                  Matrix<double> massM = ToMathNetSymmetric(matrix: mass, upper: massUpper);
                  (LinearVector vals, Matrix<double> vecs, int factorNonZeros) = SolveGeneralised(Ahat: stiffnessM, Mhat: massM);
                  Seq<(double Eigenvalue, Arr<double> Eigenvector)> pairs = toSeq(Enumerable.Range(start: 0, count: vals.Count)
                      .OrderBy(i => vals[i])
                      .Take(k)
                      .Select(i => (Eigenvalue: vals[i], Eigenvector: ArrFromVector(vecs.Column(i)))));
                  return EigenReceiptOf(pairs: pairs, path: EigenSolvePath.SparseGeneralizedCholeskyCongruence, stop: EigenSolveStop.DirectSolved, requestedPairs: k, maxResidual: GeneralizedEigenResidual(stiffness: stiffnessM, mass: massM, pairs: pairs), key: key, factorNonZeros: Some(factorNonZeros));
              })
              select receipt;
    // Generalised eigenproblem A z = λ M z via the symmetric Cholesky congruence L⁻¹AL⁻ᵀ.
    private static (LinearVector Vals, Matrix<double> Vecs, int FactorNonZeros) SolveGeneralised(Matrix<double> Ahat, Matrix<double> Mhat) {
        MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<double> cholesky = Mhat.Cholesky();
        Matrix<double> reduced = CongruentReduce(factor: cholesky.Factor, matrix: Ahat, identity: DenseMatrixD.CreateIdentity(order: Ahat.RowCount), adjoint: static m => m.Transpose());
        Matrix<double> sym = (reduced + reduced.Transpose()) * 0.5;
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = sym.Evd(Symmetricity.Symmetric);
        return (
            Vals: DenseVectorD.Create(evd.EigenValues.Count, i => evd.EigenValues[i].Real),
            Vecs: BackTransform(factor: cholesky.Factor, vectors: evd.EigenVectors, adjoint: static m => m.Transpose()),
            FactorNonZeros: cholesky.Factor.Enumerate(Zeros.AllowSkip).Count(static value => Math.Abs(value: value) > RhinoMath.ZeroTolerance));
    }
    private static (ComplexVector Vals, Matrix<Complex> Vecs) SolveGeneralisedComplex(Matrix<Complex> Ahat, Matrix<Complex> Mhat) {
        MathNet.Numerics.LinearAlgebra.Factorization.Cholesky<Complex> cholesky = Mhat.Cholesky();
        Matrix<Complex> reduced = CongruentReduce(factor: cholesky.Factor, matrix: Ahat, identity: DenseMatrixC.CreateIdentity(order: Ahat.RowCount), adjoint: static m => m.ConjugateTranspose());
        Matrix<Complex> herm = (reduced + reduced.ConjugateTranspose()) * 0.5;
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<Complex> evd = herm.Evd(Symmetricity.Hermitian);
        return (Vals: evd.EigenValues, Vecs: BackTransform(factor: cholesky.Factor, vectors: evd.EigenVectors, adjoint: static m => m.ConjugateTranspose()));
    }
    private static Matrix<T> CongruentReduce<T>(Matrix<T> factor, Matrix<T> matrix, Matrix<T> identity, Func<Matrix<T>, Matrix<T>> adjoint)
        where T : struct, IEquatable<T>, IFormattable =>
        factor.Solve(matrix * adjoint(arg: factor).Solve(identity));
    private static Matrix<T> BackTransform<T>(Matrix<T> factor, Matrix<T> vectors, Func<Matrix<T>, Matrix<T>> adjoint)
        where T : struct, IEquatable<T>, IFormattable =>
        adjoint(arg: factor).Solve(vectors);
    // --- [LOBPCG] ---------------------------------------------------------------------------
    // Knyazev 2001: span([X_i, R_i, P_i]) Rayleigh-Ritz; first iteration omits zero previous direction.
    internal static Fin<EigenSolveReceipt<double, Arr<double>>> Lobpcg(SparseMatrix matrix, int k, double tolerance, int maxIterations, Op key) =>
        !matrix.IsValid || matrix.Rows.Value != matrix.Cols.Value || k < 1 || k >= matrix.Rows.Value || !RhinoMath.IsValidDouble(tolerance) || tolerance <= 0 || maxIterations < 1
            ? Fin.Fail<EigenSolveReceipt<double, Arr<double>>>(key.InvalidInput())
            : NormalizeSymmetricUpperEntries(s: matrix, key: key).Bind(upper => {
                Matrix<double> A = ToMathNetSymmetric(matrix: matrix, upper: upper);
                return LobpcgCore(A: A, X: OrthonormalRandom(rows: matrix.Rows.Value, k: k, seed: RealInitialBasisSeed, sample: NextSignedUnit, orthonormalise: OrthonormaliseColumns), P: DenseMatrixD.Create(matrix.Rows.Value, k, 0.0), jacobi: ExtractDiagonalInverse(A), k: k, tolerance: tolerance, maxIterations: maxIterations, key: key, path: EigenSolvePath.SparseLobpcg, rayleigh: RayleighQuotients, diagonal: DenseMatrixD.OfDiagonalVector, adjoint: static m => m.Transpose(), orthonormalise: OrthonormaliseColumns, solveGeneralised: static (Ahat, Mhat) => {
                    (LinearVector Vals, Matrix<double> Vecs, int FactorNonZeros) = SolveGeneralised(Ahat: Ahat, Mhat: Mhat);
                    return (Vals, Vecs);
                }, eigenvalue: static value => value, vector: static v => ArrFromVector(v: v), residual: static (a, pairs) => RealEigenResidual(a: a, pairs: pairs));
            });
    internal static Fin<EigenSolveReceipt<double, Arr<Complex>>> LobpcgHermitian(SparseHermitian matrix, int k, double tolerance, int maxIterations, Op key) =>
        !matrix.IsValid || k < 1 || k >= matrix.Order.Value || !RhinoMath.IsValidDouble(tolerance) || tolerance <= 0 || maxIterations < 1
            ? Fin.Fail<EigenSolveReceipt<double, Arr<Complex>>>(key.InvalidInput())
            : key.Catch(() => {
                Matrix<Complex> A = ToMathNetHermitian(matrix);
                return LobpcgCore(A: A, X: OrthonormalRandom(rows: matrix.Order.Value, k: k, seed: HermitianInitialBasisSeed, sample: NextSignedComplexUnit, orthonormalise: OrthonormaliseColumnsComplex), P: DenseMatrixC.Create(matrix.Order.Value, k, Complex.Zero), jacobi: ExtractDiagonalInverseComplex(A), k: k, tolerance: tolerance, maxIterations: maxIterations, key: key, path: EigenSolvePath.SparseHermitianLobpcg, rayleigh: RayleighQuotientsComplex, diagonal: DenseMatrixC.OfDiagonalVector, adjoint: static m => m.ConjugateTranspose(), orthonormalise: OrthonormaliseColumnsComplex, solveGeneralised: static (Ahat, Mhat) => SolveGeneralisedComplex(Ahat: Ahat, Mhat: Mhat), eigenvalue: static value => value.Real, vector: static v => ArrFromComplexVector(v: v), residual: static (a, pairs) => HermitianEigenResidual(a: a, pairs: pairs));
            });
    private static Fin<EigenSolveReceipt<double, TVector>> LobpcgCore<T, TVector>(Matrix<T> A, Matrix<T> X, Matrix<T> P, MathNet.Numerics.LinearAlgebra.Vector<T> jacobi, int k, double tolerance, int maxIterations, Op key, EigenSolvePath path, Func<Matrix<T>, Matrix<T>, MathNet.Numerics.LinearAlgebra.Vector<T>> rayleigh, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, Matrix<T>> diagonal, Func<Matrix<T>, Matrix<T>> adjoint, Func<Matrix<T>, Matrix<T>> orthonormalise, Func<Matrix<T>, Matrix<T>, (MathNet.Numerics.LinearAlgebra.Vector<T> Vals, Matrix<T> Vecs)> solveGeneralised, Func<T, double> eigenvalue, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, TVector> vector, Func<Matrix<T>, Seq<(double Eigenvalue, TVector Eigenvector)>, double> residual)
        where T : struct, IEquatable<T>, IFormattable {
        int n = A.RowCount;
        return Iterate(iter: 0, X: X, P: P);
        Fin<EigenSolveReceipt<double, TVector>> Iterate(int iter, Matrix<T> X, Matrix<T> P) =>
            iter >= maxIterations
                ? LobpcgReceipt(A: A, lambda: rayleigh(arg1: X, arg2: A * X), X: X, k: k, iter: iter, maxIterations: maxIterations, tolerance: tolerance, path: path, stop: EigenSolveStop.MaxIterationsExhausted, eigenvalue: eigenvalue, vector: vector, residual: residual, key: key)
                : Step(iter: iter, X: X, P: P);
        Fin<EigenSolveReceipt<double, TVector>> Step(int iter, Matrix<T> X, Matrix<T> P) {
            Matrix<T> AX = A * X;
            MathNet.Numerics.LinearAlgebra.Vector<T> lambda = rayleigh(arg1: X, arg2: AX);
            Matrix<T> R = AX - (X * diagonal(arg: lambda));
            Seq<(double Eigenvalue, TVector Eigenvector)> pairs = LobpcgPairs(lambda: lambda, X: X, k: k, eigenvalue: eigenvalue, vector: vector);
            return residual(arg1: A, arg2: pairs) < tolerance
                ? LobpcgReceipt(A: A, lambda: lambda, X: X, k: k, iter: iter, maxIterations: maxIterations, tolerance: tolerance, path: path, stop: EigenSolveStop.ResidualConverged, eigenvalue: eigenvalue, vector: vector, residual: residual, key: key)
                : Continue(iter: iter, X: X, P: P, R: R);
        }
        Fin<EigenSolveReceipt<double, TVector>> Continue(int iter, Matrix<T> X, Matrix<T> P, Matrix<T> R) {
            Matrix<T> W = ApplyJacobi(R: R, invDiag: jacobi);
            bool hasPrevious = iter > 0 && MaxColumnNorm(m: P) > RhinoMath.SqrtEpsilon;
            Matrix<T> S = AssembleSubspace(X: X, W: W, P: P, includePrevious: hasPrevious, orthonormalise: orthonormalise);
            Matrix<T> AS = A * S;
            Matrix<T> ST = adjoint(arg: S);
            Fin<(MathNet.Numerics.LinearAlgebra.Vector<T> Vals, Matrix<T> Vecs)> solved = key.Catch(() => Fin.Succ(solveGeneralised(arg1: ST * AS, arg2: ST * S)));
            return solved.Bind(solution => {
                Matrix<T> Z = TakeSmallest(eigVals: solution.Vals, eigVecs: solution.Vecs, k: k, key: eigenvalue);
                Matrix<T> previous = hasPrevious ? P * Z.SubMatrix(2 * k, k, 0, k) : Matrix<T>.Build.Dense(n, k);
                return Iterate(iter: iter + 1, X: orthonormalise(arg: S * Z), P: (W * Z.SubMatrix(k, k, 0, k)) + previous);
            });
        }
    }
    private static Fin<EigenSolveReceipt<double, TVector>> LobpcgReceipt<T, TVector>(Matrix<T> A, MathNet.Numerics.LinearAlgebra.Vector<T> lambda, Matrix<T> X, int k, int iter, int maxIterations, double tolerance, EigenSolvePath path, EigenSolveStop stop, Func<T, double> eigenvalue, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, TVector> vector, Func<Matrix<T>, Seq<(double Eigenvalue, TVector Eigenvector)>, double> residual, Op key)
        where T : struct, IEquatable<T>, IFormattable {
        Seq<(double Eigenvalue, TVector Eigenvector)> pairs = LobpcgPairs(lambda: lambda, X: X, k: k, eigenvalue: eigenvalue, vector: vector);
        return LobpcgReceiptOf(pairs: pairs, path: path, stop: stop, requestedPairs: k, maxResidual: residual(arg1: A, arg2: pairs), iterations: iter, maxIterations: maxIterations, tolerance: tolerance, key: key);
    }
    private static Seq<(double Eigenvalue, TVector Eigenvector)> LobpcgPairs<T, TVector>(MathNet.Numerics.LinearAlgebra.Vector<T> lambda, Matrix<T> X, int k, Func<T, double> eigenvalue, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, TVector> vector)
        where T : struct, IEquatable<T>, IFormattable =>
        toSeq(Enumerable.Range(start: 0, count: k)
            .Select(i => (Eigenvalue: eigenvalue(arg: lambda[i]), Eigenvector: vector(arg: X.Column(i))))
            .OrderBy(static p => p.Eigenvalue));

    // --- [LOBPCG_PRIMITIVES] ----------------------------------------------------------------
#pragma warning disable CA5394 // Random is sufficient for LOBPCG initial guess; not security-sensitive.
    private static double NextSignedUnit(Random rng) => (rng.NextDouble() * 2.0) - 1.0;
    private static Complex NextSignedComplexUnit(Random rng) => new(real: NextSignedUnit(rng: rng), imaginary: NextSignedUnit(rng: rng));
    private static Matrix<T> OrthonormalRandom<T>(int rows, int k, int seed, Func<Random, T> sample, Func<Matrix<T>, Matrix<T>> orthonormalise)
        where T : struct, IEquatable<T>, IFormattable {
        Random rng = new(seed);
        return orthonormalise(arg: Matrix<T>.Build.Dense(rows, k, (_, _) => sample(arg: rng)));
    }
#pragma warning restore CA5394
    // Modified Gram-Schmidt with one reorthogonalisation pass; rank-collapsed columns remain zero for caller handling.
    private static Matrix<double> OrthonormaliseColumns(Matrix<double> m) =>
        OrthonormaliseColumns(m: m, zero: 0.0, inner: static (basis, value) => basis.DotProduct(value), remove: static (value, basis, dot) => value - (basis * dot), normalise: static (value, norm) => value / norm);
    private static Matrix<Complex> OrthonormaliseColumnsComplex(Matrix<Complex> m) =>
        OrthonormaliseColumns(m: m, zero: Complex.Zero, inner: static (basis, value) => basis.ConjugateDotProduct(value), remove: static (value, basis, dot) => value - (basis * dot), normalise: static (value, norm) => value / norm);
    private static Matrix<T> OrthonormaliseColumns<T>(Matrix<T> m, T zero, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, MathNet.Numerics.LinearAlgebra.Vector<T>, T> inner, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, MathNet.Numerics.LinearAlgebra.Vector<T>, T, MathNet.Numerics.LinearAlgebra.Vector<T>> remove, Func<MathNet.Numerics.LinearAlgebra.Vector<T>, double, MathNet.Numerics.LinearAlgebra.Vector<T>> normalise)
        where T : struct, IEquatable<T>, IFormattable {
        int n = m.RowCount;
        int k = m.ColumnCount;
        Matrix<T> q = Matrix<T>.Build.Dense(rows: n, columns: k, value: zero);
        for (int j = 0; j < k; j++) {
            MathNet.Numerics.LinearAlgebra.Vector<T> v = m.Column(j);
            for (int i = 0; i < j; i++) {
                MathNet.Numerics.LinearAlgebra.Vector<T> basis = q.Column(i);
                v = remove(arg1: v, arg2: basis, arg3: inner(arg1: basis, arg2: v));
            }
            double norm = v.L2Norm();
            if (norm > RhinoMath.SqrtEpsilon) q.SetColumn(columnIndex: j, column: normalise(arg1: v, arg2: norm));
        }
        return q;
    }
    private static LinearVector ExtractDiagonalInverse(Matrix<double> A) =>
        DenseVectorD.Create(A.RowCount, i => Math.Abs(A[i, i]) > RhinoMath.SqrtEpsilon ? 1.0 / A[i, i] : 1.0);
    private static ComplexVector ExtractDiagonalInverseComplex(Matrix<Complex> A) =>
        DenseVectorC.Create(A.RowCount, i => Complex.Abs(A[i, i]) > RhinoMath.SqrtEpsilon ? Complex.One / A[i, i] : Complex.One);
    private static LinearVector RayleighQuotients(Matrix<double> X, Matrix<double> AX) =>
        DenseVectorD.Create(
            X.ColumnCount,
            j => X.Column(j).DotProduct(AX.Column(j)) / Math.Max(X.Column(j).DotProduct(X.Column(j)), RhinoMath.ZeroTolerance));
    private static ComplexVector RayleighQuotientsComplex(Matrix<Complex> X, Matrix<Complex> AX) =>
        DenseVectorC.Create(
            X.ColumnCount,
            j => X.Column(j).ConjugateDotProduct(X.Column(j)) switch {
                Complex den when Complex.Abs(den) > RhinoMath.ZeroTolerance => X.Column(j).ConjugateDotProduct(AX.Column(j)) / den,
                _ => Complex.Zero
            });
    private static double MaxColumnNorm<T>(Matrix<T> m)
        where T : struct, IEquatable<T>, IFormattable =>
        Enumerable.Range(start: 0, count: m.ColumnCount).Aggregate(seed: 0.0, func: (max, j) => Math.Max(max, m.Column(j).L2Norm()));
    private static Matrix<T> ApplyJacobi<T>(Matrix<T> R, MathNet.Numerics.LinearAlgebra.Vector<T> invDiag)
        where T : struct, IEquatable<T>, IFormattable =>
        Matrix<T>.Build.DenseOfDiagonalVector(invDiag).Multiply(R);
    private static Matrix<T> AssembleSubspace<T>(Matrix<T> X, Matrix<T> W, Matrix<T> P, bool includePrevious, Func<Matrix<T>, Matrix<T>> orthonormalise)
        where T : struct, IEquatable<T>, IFormattable =>
        orthonormalise(arg: includePrevious ? X.Append(W).Append(P) : X.Append(W));
    private static Matrix<T> TakeSmallest<T>(MathNet.Numerics.LinearAlgebra.Vector<T> eigVals, Matrix<T> eigVecs, int k, Func<T, double> key)
        where T : struct, IEquatable<T>, IFormattable =>
        Matrix<T>.Build.DenseOfColumnVectors([.. Enumerable.Range(start: 0, count: eigVals.Count).OrderBy(i => key(arg: eigVals[i])).Take(count: k).Select(eigVecs.Column)]);
}

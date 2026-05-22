using System.Numerics;
using Foundation.CSharp.Analyzers.Contracts;
using MathNet.Numerics.LinearAlgebra;
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
        new(Rows: dim, Cols: dim,
            Entries: [.. toSeq(Enumerable.Range(start: 0, count: dim.Value * dim.Value))
                .Map(idx => (idx / dim.Value) == (idx % dim.Value) ? 1.0 : 0.0)]);
    public static Matrix Zero(Dimension rows, Dimension cols) =>
        new(Rows: rows, Cols: cols,
            Entries: [.. Enumerable.Repeat(element: 0.0, count: rows.Value * cols.Value)]);
    public Matrix Transpose() => MatrixKernel.FromMathNet(MatrixKernel.ToMathNet(this).Transpose(), Cols, Rows);
    public static Matrix operator *(Matrix a, Matrix b) =>
        MatrixKernel.FromMathNet(MatrixKernel.ToMathNet(a).Multiply(MatrixKernel.ToMathNet(b)), a.Rows, b.Cols);
    public Fin<Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)>> DecomposeEigen(Op? key = null) =>
        MatrixKernel.GeneralEigen(matrix: this, key: key.OrDefault());
    public Fin<SvdResult> DecomposeSvd(Op? key = null) => MatrixKernel.Svd(matrix: this, key: key.OrDefault());
    public Fin<LuResult> DecomposeLu(Op? key = null) => MatrixKernel.Lu(matrix: this, key: key.OrDefault());
    public Fin<QrResult> DecomposeQr(Op? key = null) => MatrixKernel.Qr(matrix: this, key: key.OrDefault());
    public double Frobenius => MatrixNormKind.Frobenius.Compute(matrix: this);
    public Fin<double> Norm(MatrixNormKind kind, Op? key = null) =>
        kind is null ? Fin.Fail<double>(error: key.OrDefault().InvalidInput()) : Fin.Succ(kind.Compute(matrix: this));
    public Fin<double> Trace(Op? key = null) => key.OrDefault().AcceptValue(value: MatrixKernel.ToMathNet(this).Trace());
    public Fin<double> Determinant(Op? key = null) =>
        DecomposeLu(key: key.OrDefault()).Map(static lu => lu.Determinant);
    public Fin<double> Spectral(Op? key = null) =>
        DecomposeSvd(key: key.OrDefault()).Map(static svd => svd.Sigma.IsEmpty ? 0.0 : svd.Sigma[0]);
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.Solve(matrix: this, rhs: rhs, key: key.OrDefault());
    public Fin<int> Rank(Op? key = null) =>
        DecomposeSvd(key: key.OrDefault()).Map(static svd => svd.NumericalRank());
    public Fin<Matrix> Inverse(Op? key = null) {
        Op op = key.OrDefault();
        return Rows.Value != Cols.Value
            ? Fin.Fail<Matrix>(error: op.InvalidInput())
            : Fin.Succ(MatrixKernel.FromMathNet(MatrixKernel.ToMathNet(this).Inverse(), Rows, Cols));
    }
    public Fin<Matrix> PseudoInverse(Op? key = null) =>
        DecomposeSvd(key: key.OrDefault()).Map(static svd => svd.PseudoInverse());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SvdResult(Matrix U, Arr<double> Sigma, Matrix V) {
    public bool IsValid => U.IsValid && V.IsValid && Sigma.All(static value => RhinoMath.IsValidDouble(x: value) && value >= 0.0);
    public int NumericalRank() {
        Arr<double> sigma = Sigma;
        if (sigma.IsEmpty) return 0;
        double maxSigma = sigma.Fold(initialState: 0.0, f: static (m, s) => Math.Max(val1: m, val2: s));
        int largerDim = Math.Max(val1: U.Rows.Value, val2: V.Cols.Value);
        double threshold = maxSigma * RhinoMath.SqrtEpsilon * largerDim;
        return sigma.Fold(initialState: 0, f: (count, s) => s > threshold ? count + 1 : count);
    }
    public Matrix PseudoInverse() {
        Arr<double> sigma = Sigma;
        int uCols = U.Cols.Value;
        int vCols = V.Cols.Value;
        double threshold = sigma.IsEmpty ? 0.0 :
            sigma.Fold(initialState: 0.0, f: static (m, s) => Math.Max(val1: m, val2: s)) * RhinoMath.SqrtEpsilon * Math.Max(uCols, vCols);
        Matrix sigmaInv = new(Rows: V.Cols, Cols: U.Cols,
            Entries: [.. toSeq(Enumerable.Range(start: 0, count: vCols * uCols))
                .Map(idx => (idx / uCols) == (idx % uCols) && (idx / uCols) < sigma.Count && sigma[idx / uCols] > threshold
                    ? 1.0 / sigma[idx / uCols]
                    : 0.0)]);
        return V * sigmaInv * U.Transpose();
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct LuResult(Arr<int> Permutation, int SwapCount, Matrix L, Matrix U, Matrix Source) {
    public bool IsValid => L.IsValid && U.IsValid && Permutation.Count == L.Rows.Value;
    public double Determinant {
        get {
            Matrix u = U;
            int n = L.Rows.Value;
            double sign = (SwapCount % 2 == 0) ? 1.0 : -1.0;
            return sign * toSeq(Enumerable.Range(start: 0, count: n))
                .Fold(initialState: 1.0, f: (product, i) => product * u.At(i: i, j: i));
        }
    }
    public Arr<double> Solve(Arr<double> rhs) => MatrixKernel.LuSolve(lu: this, rhs: rhs);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct QrResult(Matrix Q, Matrix R) {
    public bool IsValid => Q.IsValid && R.IsValid;
}

// Dense lower-triangular factor L (A = L Lᵀ); sparse inputs are densified before factorization.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CholeskyResult(Matrix L, Matrix Source) {
    public bool IsValid => L.IsValid && Source.IsValid && L.Rows.Value == L.Cols.Value && Source.Rows.Value == Source.Cols.Value;
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) =>
        MatrixKernel.CholeskySolve(source: Source, rhs: rhs, key: key.OrDefault());
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
    public Fin<CholeskyResult> DecomposeCholesky(Op? key = null) =>
        MatrixKernel.SparseCholesky(matrix: this, key: key.OrDefault());
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) =>
        DecomposeCholesky(key: key).Bind(c => c.Solve(rhs: rhs, key: key));
    public Fin<Seq<(double Eigenvalue, Arr<double> Eigenvector)>> SmallestEigenpairs(int k, double tolerance, int maxIterations = 200, Op? key = null) =>
        MatrixKernel.Lobpcg(matrix: this, k: k, tolerance: tolerance, maxIterations: maxIterations, key: key.OrDefault());
    private static bool RowPointersAreMonotone(Arr<int> rowPtr) =>
        toSeq(Enumerable.Range(start: 0, count: Math.Max(val1: 0, val2: rowPtr.Count - 1)))
            .ForAll(i => rowPtr[i] <= rowPtr[i + 1]);
    private static bool ColumnIndicesAreBoundedAndSorted(Arr<int> rowPtr, Arr<int> colInd, int cols) =>
        toSeq(Enumerable.Range(start: 0, count: Math.Max(val1: 0, val2: rowPtr.Count - 1)))
            .ForAll(row => toSeq(Enumerable.Range(start: rowPtr[row], count: rowPtr[row + 1] - rowPtr[row]))
                .Fold(initialState: (Ok: true, Prev: -1), f: (state, k) => (
                    Ok: state.Ok && colInd[k] >= 0 && colInd[k] < cols && colInd[k] > state.Prev,
                    Prev: colInd[k])).Ok);
}

// Upper-triangular storage; Multiply reconstructs the lower triangle by conjugate transpose.
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SparseHermitian(Dimension Order, Arr<int> RowPtr, Arr<int> ColInd, Arr<Complex> Values) {
    public bool IsValid =>
        RowPtr.Count == Order.Value + 1
        && ColInd.Count == Values.Count
        && Values.All(static c => RhinoMath.IsValidDouble(c.Real) && RhinoMath.IsValidDouble(c.Imaginary))
        && RowPtr[0] == 0
        && RowPtr[Order.Value] == Values.Count;
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
    public Fin<Arr<Complex>> Solve(Arr<Complex> rhs, Op? key = null) {
        Op op = key.OrDefault();
        SparseHermitian self = this;
        return rhs.Count != Order.Value
            ? Fin.Fail<Arr<Complex>>(op.InvalidInput())
            : op.Catch(() => Fin.Succ(MatrixKernel.SolveHermitianDense(self: self, rhs: rhs)));
    }
    public Fin<Seq<(double Eigenvalue, Arr<Complex> Eigenvector)>> SmallestEigenpairs(int k, double tolerance, int maxIterations = 200, Op? key = null) =>
        MatrixKernel.LobpcgHermitian(matrix: this, k: k, tolerance: tolerance, maxIterations: maxIterations, key: key.OrDefault());
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
    // --- [BRIDGE] ---------------------------------------------------------------------------
    internal static DenseMatrixD ToMathNet(Matrix m) =>
        (DenseMatrixD)DenseMatrixD.Build.Dense(m.Rows.Value, m.Cols.Value, m.At);
    internal static Matrix FromMathNet(Matrix<double> m, Dimension rows, Dimension cols) {
        double[] entries = new double[rows.Value * cols.Value];
        for (int i = 0; i < rows.Value; i++)
            for (int j = 0; j < cols.Value; j++)
                entries[(i * cols.Value) + j] = m[i, j];
        return new Matrix(Rows: rows, Cols: cols, Entries: new Arr<double>(entries));
    }
    private static DenseMatrixC ToMathNetComplex(Matrix m) =>
        (DenseMatrixC)DenseMatrixC.Build.Dense(m.Rows.Value, m.Cols.Value, (i, j) => new Complex(m.At(i: i, j: j), 0.0));
    private static Arr<double> ArrFromVector(LinearVector v) => new(v.ToArray());

    // --- [DENSE_DECOMPOSITIONS] -------------------------------------------------------------
    internal static Fin<SvdResult> Svd(Matrix matrix, Op key) {
        MathNet.Numerics.LinearAlgebra.Factorization.Svd<double> svd = ToMathNet(matrix).Svd(computeVectors: true);
        return Fin.Succ(new SvdResult(
            U: FromMathNet(svd.U, matrix.Rows, matrix.Rows),
            Sigma: ArrFromVector(svd.S),
            V: FromMathNet(svd.VT.Transpose(), matrix.Cols, matrix.Cols)));
    }
    internal static Fin<LuResult> Lu(Matrix matrix, Op key) {
        if (matrix.Rows.Value != matrix.Cols.Value) return Fin.Fail<LuResult>(key.InvalidInput());
        MathNet.Numerics.LinearAlgebra.Factorization.LU<double> lu = ToMathNet(matrix).LU();
        int n = matrix.Rows.Value;
        Arr<int> permutation = ExtractPermutation(p: lu.P, n: n);
        return Fin.Succ(new LuResult(
            Permutation: permutation,
            SwapCount: CountSwaps(perm: permutation, n: n),
            L: FromMathNet(lu.L, matrix.Rows, matrix.Cols),
            U: FromMathNet(lu.U, matrix.Rows, matrix.Cols),
            Source: matrix));
    }
    internal static Fin<QrResult> Qr(Matrix matrix, Op key) {
        MathNet.Numerics.LinearAlgebra.Factorization.QR<double> qr = ToMathNet(matrix).QR(MathNet.Numerics.LinearAlgebra.Factorization.QRMethod.Full);
        return Fin.Succ(new QrResult(
            Q: FromMathNet(qr.Q, matrix.Rows, matrix.Rows),
            R: FromMathNet(qr.R, matrix.Rows, matrix.Cols)));
    }
    internal static Fin<CholeskyResult> Cholesky(SymmetricMatrix matrix, Op key) =>
        key.Catch(() => {
            Matrix source = matrix.ToDense();
            return Fin.Succ(new CholeskyResult(
                L: FromMathNet(ToMathNet(source).Cholesky().Factor, matrix.Dimension, matrix.Dimension),
                Source: source));
        });
    internal static Fin<Seq<(double Eigenvalue, Arr<double> Eigenvector)>> SymmetricEigen(SymmetricMatrix matrix, Op key) {
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = ToMathNet(matrix.ToDense()).Evd(Symmetricity.Symmetric);
        int n = matrix.Dimension.Value;
        return Fin.Succ(toSeq(Enumerable.Range(start: 0, count: n)
            .Select(i => (Eigenvalue: evd.EigenValues[i].Real, Eigenvector: ArrFromVector(evd.EigenVectors.Column(i))))
            .OrderByDescending(static p => Math.Abs(p.Eigenvalue))));
    }
    internal static Fin<Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)>> GeneralEigen(Matrix matrix, Op key) {
        if (matrix.Rows.Value != matrix.Cols.Value) return Fin.Fail<Seq<(Complex, Arr<Complex>)>>(key.InvalidInput());
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<Complex> evd = ToMathNetComplex(matrix).Evd(Symmetricity.Asymmetric);
        int n = matrix.Rows.Value;
        return Fin.Succ(toSeq(Enumerable.Range(start: 0, count: n)
            .Select(i => (Eigenvalue: evd.EigenValues[i], Eigenvector: ArrFromComplexVector(evd.EigenVectors.Column(i))))));
    }
    private static Arr<Complex> ArrFromComplexVector(ComplexVector v) => new(v.ToArray());
    internal static Fin<Arr<double>> Solve(Matrix matrix, Arr<double> rhs, Op key) =>
        rhs.Count != matrix.Rows.Value || matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<Arr<double>>(error: key.InvalidInput())
            : key.Catch(() => Fin.Succ(ArrFromVector(ToMathNet(matrix).LU().Solve(DenseVectorD.OfArray([.. rhs.AsIterable()])))));
    internal static Arr<double> LuSolve(LuResult lu, Arr<double> rhs) =>
        ArrFromVector(ToMathNet(lu.Source).LU().Solve(DenseVectorD.OfArray([.. rhs.AsIterable()])));
    private static Arr<int> ExtractPermutation(MathNet.Numerics.Permutation p, int n) =>
        [.. Enumerable.Range(0, n).Select(i => p[i])];
    private static int CountSwaps(Arr<int> perm, int n) {
        bool[] visited = new bool[n];
        int swaps = 0;
        for (int i = 0; i < n; i++) {
            if (visited[i] || perm[i] == i) continue;
            int j = i;
            int cycle = 0;
            while (!visited[j]) { visited[j] = true; j = perm[j]; cycle++; }
            swaps += cycle - 1;
        }
        return swaps;
    }

    // --- [DENSE_CHOLESKY_SOLVE] -------------------------------------------------------------
    internal static Fin<Arr<double>> CholeskySolve(Matrix source, Arr<double> rhs, Op key) {
        return rhs.Count != source.Rows.Value || source.Rows.Value != source.Cols.Value
            ? Fin.Fail<Arr<double>>(key.InvalidInput())
            : key.Catch(() => Fin.Succ(ArrFromVector(ToMathNet(source).Cholesky().Solve(DenseVectorD.OfArray([.. rhs.AsIterable()])))));
    }

    // --- [SPARSE_ASSEMBLY] ------------------------------------------------------------------
    internal static Fin<SparseMatrix> AssembleSparse(Dimension rows, Dimension cols, IEnumerable<(int Row, int Col, double Value)> triplets, Op op) {
        List<(int Row, int Col, double Value)> indexed = [.. triplets.Where(t => RhinoMath.IsValidDouble(t.Value) && t.Row >= 0 && t.Row < rows.Value && t.Col >= 0 && t.Col < cols.Value)
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
        List<(int Row, int Col, Complex Value)> upper = [.. triplets.Where(t => t.Row >= 0 && t.Col >= 0 && t.Row < order.Value && t.Col < order.Value && t.Row <= t.Col)
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
    internal static Arr<Complex> SolveHermitianDense(SparseHermitian self, Arr<Complex> rhs) {
        Matrix<Complex> dense = ToMathNetHermitian(self);
        ComplexVector b = DenseVectorC.OfArray([.. rhs.AsIterable()]);
        ComplexVector x = dense.Cholesky().Solve(b);
        return new Arr<Complex>([.. x]);
    }
    internal static Matrix SparseToDense(SparseMatrix self) =>
        FromMathNet(m: ToMathNetSparse(s: self), rows: self.Rows, cols: self.Cols);
    internal static Fin<CholeskyResult> SparseCholesky(SparseMatrix matrix, Op key) =>
        matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<CholeskyResult>(key.InvalidInput())
            : key.Catch(() => {
                Matrix source = SparseToDense(self: matrix);
                return Fin.Succ(new CholeskyResult(
                    L: FromMathNet(ToMathNet(source).Cholesky().Factor, matrix.Rows, matrix.Cols),
                    Source: source));
            });

    // --- [LOBPCG] ---------------------------------------------------------------------------
    // Knyazev 2001. Subspace span([X_i, R_i, P_i]) reduced via Rayleigh-Ritz to a 3k x 3k
    // generalised eigenproblem; Jacobi-diagonal preconditioner; converges on residual L2 norm.
    internal static Fin<Seq<(double Eigenvalue, Arr<double> Eigenvector)>> Lobpcg(SparseMatrix matrix, int k, double tolerance, int maxIterations, Op key) {
        int n = matrix.Rows.Value;
        if (matrix.Rows.Value != matrix.Cols.Value || k < 1 || k >= n || !RhinoMath.IsValidDouble(tolerance) || tolerance <= 0)
            return Fin.Fail<Seq<(double, Arr<double>)>>(key.InvalidInput());
        Matrix<double> A = ToMathNetSparse(matrix);
        Matrix<double> X = OrthonormalRandom(rows: n, k: k, seed: 17);
        LinearVector jacobi = ExtractDiagonalInverse(A);
        Matrix<double> P = DenseMatrixD.Create(n, k, 0.0);
        LinearVector? eigenvalues = null;
        Matrix<double>? eigenvectors = null;
        for (int iter = 0; iter < maxIterations; iter++) {
            Matrix<double> AX = A * X;
            LinearVector lambda = RayleighQuotients(X: X, AX: AX);
            Matrix<double> R = AX - (X * DenseMatrixD.OfDiagonalVector(lambda));
            if (MaxColumnNorm(R) < tolerance) {
                eigenvalues = lambda;
                eigenvectors = X;
                break;
            }
            Matrix<double> W = ApplyJacobi(R: R, invDiag: jacobi);
            Matrix<double> S = AssembleSubspace(X: X, W: W, P: P);
            Matrix<double> Ahat = S.Transpose() * (A * S);
            Matrix<double> Mhat = S.Transpose() * S;
            (LinearVector eigVals, Matrix<double> eigVecs) = SolveGeneralised(Ahat: Ahat, Mhat: Mhat);
            Matrix<double> Z = TakeSmallest(eigVals: eigVals, eigVecs: eigVecs, k: k);
            Matrix<double> Xnew = S * Z;
            P = (W * Z.SubMatrix(k, k, 0, k)) + (P * Z.SubMatrix(2 * k, k, 0, k));
            X = OrthonormaliseColumns(Xnew);
            eigenvalues = lambda;
            eigenvectors = X;
        }
        if (eigenvalues is null || eigenvectors is null) return Fin.Fail<Seq<(double, Arr<double>)>>(key.InvalidResult());
        LinearVector finalVals = eigenvalues;
        Matrix<double> finalVecs = eigenvectors;
        IEnumerable<(double Eigenvalue, Arr<double> Eigenvector)> pairs = Enumerable.Range(0, k)
            .Select(i => (Eigenvalue: finalVals[i], Eigenvector: ArrFromVector(finalVecs.Column(i))))
            .OrderBy(static p => p.Eigenvalue);
        return Fin.Succ(toSeq(pairs));
    }
    internal static Fin<Seq<(double Eigenvalue, Arr<Complex> Eigenvector)>> LobpcgHermitian(SparseHermitian matrix, int k, double tolerance, int maxIterations, Op key) {
        int n = matrix.Order.Value;
        if (k < 1 || k >= n || !RhinoMath.IsValidDouble(tolerance) || tolerance <= 0)
            return Fin.Fail<Seq<(double, Arr<Complex>)>>(key.InvalidInput());
        Matrix<Complex> A = ToMathNetHermitian(matrix);
        Matrix<Complex> X = OrthonormalRandomComplex(rows: n, k: k, seed: 19);
        ComplexVector jacobi = ExtractDiagonalInverseComplex(A);
        Matrix<Complex> P = DenseMatrixC.Create(n, k, Complex.Zero);
        ComplexVector? eigenvalues = null;
        Matrix<Complex>? eigenvectors = null;
        for (int iter = 0; iter < maxIterations; iter++) {
            Matrix<Complex> AX = A * X;
            ComplexVector lambda = RayleighQuotientsComplex(X: X, AX: AX);
            Matrix<Complex> R = AX - (X * DenseMatrixC.OfDiagonalVector(lambda));
            if (MaxColumnNormComplex(R) < tolerance) {
                eigenvalues = lambda;
                eigenvectors = X;
                break;
            }
            Matrix<Complex> W = ApplyJacobiComplex(R: R, invDiag: jacobi);
            Matrix<Complex> S = AssembleSubspaceComplex(X: X, W: W, P: P);
            Matrix<Complex> Ahat = S.ConjugateTranspose() * (A * S);
            Matrix<Complex> Mhat = S.ConjugateTranspose() * S;
            (ComplexVector eigVals, Matrix<Complex> eigVecs) = SolveGeneralisedComplex(Ahat: Ahat, Mhat: Mhat);
            Matrix<Complex> Z = TakeSmallestComplex(eigVals: eigVals, eigVecs: eigVecs, k: k);
            Matrix<Complex> Xnew = S * Z;
            P = (W * Z.SubMatrix(k, k, 0, k)) + (P * Z.SubMatrix(2 * k, k, 0, k));
            X = OrthonormaliseColumnsComplex(Xnew);
            eigenvalues = lambda;
            eigenvectors = X;
        }
        if (eigenvalues is null || eigenvectors is null) return Fin.Fail<Seq<(double, Arr<Complex>)>>(key.InvalidResult());
        ComplexVector finalVals = eigenvalues;
        Matrix<Complex> finalVecs = eigenvectors;
        IEnumerable<(double Eigenvalue, Arr<Complex> Eigenvector)> pairs = Enumerable.Range(0, k)
            .Select(i => (Eigenvalue: finalVals[i].Real, Eigenvector: ArrFromComplexVector(finalVecs.Column(i))))
            .OrderBy(static p => p.Eigenvalue);
        return Fin.Succ(toSeq(pairs));
    }

    // --- [LOBPCG_PRIMITIVES] ----------------------------------------------------------------
    private static Matrix<double> ToMathNetSparse(SparseMatrix s) {
        SparseMatrixD m = new(rows: s.Rows.Value, columns: s.Cols.Value);
        for (int i = 0; i < s.Rows.Value; i++)
            for (int kIndex = s.RowPtr[i]; kIndex < s.RowPtr[i + 1]; kIndex++)
                m[i, s.ColInd[kIndex]] = s.Values[kIndex];
        return m;
    }
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
    private static Matrix<double> AssembleSubspace(Matrix<double> X, Matrix<double> W, Matrix<double> P) {
        int n = X.RowCount;
        int k = X.ColumnCount;
        DenseMatrixD S = DenseMatrixD.Create(n, 3 * k, 0.0);
        S.SetSubMatrix(0, 0, X);
        S.SetSubMatrix(0, k, W);
        S.SetSubMatrix(0, 2 * k, P);
        return OrthonormaliseColumns(S);
    }
    private static Matrix<Complex> AssembleSubspaceComplex(Matrix<Complex> X, Matrix<Complex> W, Matrix<Complex> P) {
        int n = X.RowCount;
        int k = X.ColumnCount;
        DenseMatrixC S = DenseMatrixC.Create(n, 3 * k, Complex.Zero);
        S.SetSubMatrix(0, 0, X);
        S.SetSubMatrix(0, k, W);
        S.SetSubMatrix(0, 2 * k, P);
        return OrthonormaliseColumnsComplex(S);
    }
    // Generalised eigenproblem A z = λ M z via Cholesky reduction: M = L Lᵀ → (L⁻¹ A L⁻ᵀ) y = λ y, z = L⁻ᵀ y.
    private static (LinearVector Vals, Matrix<double> Vecs) SolveGeneralised(Matrix<double> Ahat, Matrix<double> Mhat) {
        Matrix<double> Linv = Mhat.Cholesky().Factor.Inverse();
        Matrix<double> reduced = Linv * Ahat * Linv.Transpose();
        Matrix<double> sym = (reduced + reduced.Transpose()) * 0.5;
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = sym.Evd(Symmetricity.Symmetric);
        return (
            Vals: DenseVectorD.Create(evd.EigenValues.Count, i => evd.EigenValues[i].Real),
            Vecs: Linv.Transpose() * evd.EigenVectors);
    }
    private static (ComplexVector Vals, Matrix<Complex> Vecs) SolveGeneralisedComplex(Matrix<Complex> Ahat, Matrix<Complex> Mhat) {
        Matrix<Complex> Linv = Mhat.Cholesky().Factor.Inverse();
        Matrix<Complex> reduced = Linv * Ahat * Linv.ConjugateTranspose();
        Matrix<Complex> herm = (reduced + reduced.ConjugateTranspose()) * 0.5;
        MathNet.Numerics.LinearAlgebra.Factorization.Evd<Complex> evd = herm.Evd(Symmetricity.Hermitian);
        return (Vals: evd.EigenValues, Vecs: Linv.ConjugateTranspose() * evd.EigenVectors);
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

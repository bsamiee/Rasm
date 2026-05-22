using System.Numerics;
using Foundation.CSharp.Analyzers.Contracts;

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
        MatrixKernel.JacobiSymmetric(matrix: this, key: key.OrDefault());
    public Fin<Matrix> DecomposeCholesky(Op? key = null) =>
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
    public Matrix Transpose() {
        Matrix self = this;
        int r = Rows.Value;
        int c = Cols.Value;
        return new Matrix(Rows: Cols, Cols: Rows,
            Entries: [.. toSeq(Enumerable.Range(start: 0, count: r * c))
                .Map(idx => self.At(i: idx % r, j: idx / r))]);
    }
    public static Matrix operator *(Matrix a, Matrix b) {
        int aRows = a.Rows.Value;
        int aCols = a.Cols.Value;
        int bCols = b.Cols.Value;
        return new Matrix(Rows: a.Rows, Cols: b.Cols,
            Entries: [.. toSeq(Enumerable.Range(start: 0, count: aRows * bCols))
                .Map(idx => toSeq(Enumerable.Range(start: 0, count: aCols))
                    .Fold(initialState: 0.0, f: (sum, k) => sum + (a.At(i: idx / bCols, j: k) * b.At(i: k, j: idx % bCols))))]);
    }
    public Fin<Matrix> ReduceHessenberg(Op? key = null) =>
        MatrixKernel.Hessenberg(matrix: this, key: key.OrDefault());
    public Fin<Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)>> DecomposeEigen(Op? key = null) =>
        MatrixKernel.FrancisQr(matrix: this, key: key.OrDefault());
    public Fin<SvdResult> DecomposeSvd(Op? key = null) =>
        MatrixKernel.SvdJacobi(matrix: this, key: key.OrDefault());
    public Fin<LuResult> DecomposeLu(Op? key = null) =>
        MatrixKernel.LuPartialPivot(matrix: this, key: key.OrDefault());
    public Fin<QrResult> DecomposeQr(Op? key = null) =>
        MatrixKernel.QrHouseholder(matrix: this, key: key.OrDefault());
    public double Frobenius => MatrixNormKind.Frobenius.Compute(matrix: this);
    public Fin<double> Norm(MatrixNormKind kind, Op? key = null) {
        Op op = key.OrDefault();
        return kind is null
            ? Fin.Fail<double>(error: op.InvalidInput())
            : Fin.Succ(kind.Compute(matrix: this));
    }
    public Fin<double> Trace(Op? key = null) {
        Op op = key.OrDefault();
        int n = Math.Min(val1: Rows.Value, val2: Cols.Value);
        Matrix self = this;
        return op.AcceptValue(value: toSeq(Enumerable.Range(start: 0, count: n))
            .Fold(initialState: 0.0, f: (sum, i) => sum + self.At(i: i, j: i)));
    }
    public Fin<double> Determinant(Op? key = null) =>
        DecomposeLu(key: key.OrDefault()).Map(static lu => lu.Determinant);
    public Fin<double> Spectral(Op? key = null) =>
        DecomposeSvd(key: key.OrDefault()).Map(static svd => svd.Sigma.IsEmpty ? 0.0 : svd.Sigma[0]);
    public Fin<Arr<double>> Solve(Arr<double> rhs, Op? key = null) {
        Op op = key.OrDefault();
        int n = Rows.Value;
        return rhs.Count != n || n != Cols.Value
            ? Fin.Fail<Arr<double>>(error: op.InvalidInput())
            : DecomposeLu(key: op).Map(lu => lu.Solve(rhs: rhs));
    }
    public Fin<int> Rank(Op? key = null) {
        Op op = key.OrDefault();
        Matrix self = this;
        int dim = Math.Max(val1: Rows.Value, val2: Cols.Value);
        return DecomposeSvd(key: op).Map(svd => {
            double maxSigma = svd.Sigma.Fold(initialState: 0.0, f: static (m, s) => Math.Max(val1: m, val2: s));
            double threshold = maxSigma * RhinoMath.SqrtEpsilon * dim;
            return svd.Sigma.Fold(initialState: 0, f: (count, s) => s > threshold ? count + 1 : count);
        });
    }
    public Fin<Matrix> Inverse(Op? key = null) {
        Op op = key.OrDefault();
        int n = Rows.Value;
        return n != Cols.Value
            ? Fin.Fail<Matrix>(error: op.InvalidInput())
            : DecomposeLu(key: op).Map(lu => MatrixKernel.LuInverse(lu: lu, n: n));
    }
    public Fin<Matrix> PseudoInverse(Op? key = null) =>
        DecomposeSvd(key: key.OrDefault()).Map(static svd => svd.PseudoInverse());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SvdResult(Matrix U, Arr<double> Sigma, Matrix V) {
    public bool IsValid => U.IsValid && V.IsValid && Sigma.All(static value => RhinoMath.IsValidDouble(x: value) && value >= 0.0);
    public Matrix PseudoInverse() {
        Arr<double> sigma = Sigma;
        int n = sigma.Count;
        int uCols = U.Cols.Value;
        int vCols = V.Cols.Value;
        Matrix sigmaInv = new(Rows: V.Cols, Cols: U.Cols,
            Entries: [.. toSeq(Enumerable.Range(start: 0, count: vCols * uCols))
                .Map(idx => (idx / uCols) == (idx % uCols) && (idx / uCols) < n && sigma[idx / uCols] > RhinoMath.SqrtEpsilon
                    ? 1.0 / sigma[idx / uCols]
                    : 0.0)]);
        return V * sigmaInv * U.Transpose();
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct LuResult(Arr<int> Permutation, int SwapCount, Matrix L, Matrix U) {
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

[SmartEnum<int>]
public sealed partial class MatrixNormKind {
    public static readonly MatrixNormKind Frobenius = new(key: 0,
        compute: static m => Math.Sqrt(d: m.Entries.Fold(initialState: 0.0, f: static (sum, e) => sum + (e * e))));
    public static readonly MatrixNormKind MaxAbs = new(key: 1,
        compute: static m => m.Entries.Fold(initialState: 0.0, f: static (acc, e) => Math.Max(val1: acc, val2: Math.Abs(value: e))));
    public static readonly MatrixNormKind L1 = new(key: 2,
        compute: static m => MatrixKernel.AbsoluteColumnSumMax(matrix: m));
    public static readonly MatrixNormKind LInf = new(key: 3,
        compute: static m => MatrixKernel.AbsoluteRowSumMax(matrix: m));
    [UseDelegateFromConstructor] internal partial double Compute(Matrix matrix);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class MatrixKernel {
    private const int MaxSweeps = 64;
    private const double JacobiEpsilon = 1e-14;
    private const double QrEpsilon = 1e-12;
    private const int MaxQrIterations = 256;

    // --- [NORMS_SOLVERS] ------------------------------------------------------------------
    internal static double AbsoluteColumnSumMax(Matrix matrix) {
        Matrix self = matrix;
        int rows = matrix.Rows.Value;
        int cols = matrix.Cols.Value;
        return toSeq(Enumerable.Range(start: 0, count: cols))
            .Fold(initialState: 0.0, f: (best, j) => Math.Max(val1: best, val2:
                toSeq(Enumerable.Range(start: 0, count: rows))
                    .Fold(initialState: 0.0, f: (col, i) => col + Math.Abs(value: self.At(i: i, j: j)))));
    }
    internal static double AbsoluteRowSumMax(Matrix matrix) {
        Matrix self = matrix;
        int rows = matrix.Rows.Value;
        int cols = matrix.Cols.Value;
        return toSeq(Enumerable.Range(start: 0, count: rows))
            .Fold(initialState: 0.0, f: (best, i) => Math.Max(val1: best, val2:
                toSeq(Enumerable.Range(start: 0, count: cols))
                    .Fold(initialState: 0.0, f: (row, j) => row + Math.Abs(value: self.At(i: i, j: j)))));
    }
    internal static Arr<double> LuSolve(LuResult lu, Arr<double> rhs) {
        int n = lu.L.Rows.Value;
        Arr<int> perm = lu.Permutation;
        Matrix L = lu.L;
        Matrix U = lu.U;
        Arr<double> permuted = [.. Enumerable.Range(start: 0, count: n).Select(i => rhs[perm[i]])];
        Arr<double> y = toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: new Arr<double>(Enumerable.Repeat(element: 0.0, count: n)),
            f: (acc, i) => acc.SetItem(i, permuted[i] - toSeq(Enumerable.Range(start: 0, count: i))
                .Fold(initialState: 0.0, f: (s, j) => s + (L.At(i: i, j: j) * acc[j]))));
        return toSeq(Enumerable.Range(start: 0, count: n).Reverse()).Fold(
            initialState: new Arr<double>(Enumerable.Repeat(element: 0.0, count: n)),
            f: (acc, i) => acc.SetItem(i, (y[i] - toSeq(Enumerable.Range(start: i + 1, count: n - i - 1))
                .Fold(initialState: 0.0, f: (s, j) => s + (U.At(i: i, j: j) * acc[j]))) / U.At(i: i, j: i)));
    }
    // Solve M*X = I column by column: each column j of X is the solution to M*x = e_j.
    // Row-major storage means entries[i*n + j] = inverse[i,j] = column_j[i].
    internal static Matrix LuInverse(LuResult lu, int n) {
        Seq<Arr<double>> columns = toSeq(Enumerable.Range(start: 0, count: n).Select(j =>
            LuSolve(lu: lu, rhs: [.. Enumerable.Range(start: 0, count: n).Select(i => i == j ? 1.0 : 0.0)])));
        return new Matrix(Rows: lu.L.Rows, Cols: lu.U.Cols,
            Entries: [.. Enumerable.Range(start: 0, count: n * n).Select(idx => columns[idx % n][idx / n])]);
    }
    // Householder QR: A = Q * R where Q is orthogonal (m x m) and R is upper-triangular (m x n).
    // For each column j, build a Householder reflector H_j that zeros R[j+1..m, j], apply it
    // from the LEFT to R, and from the RIGHT to the accumulating Q (since
    // Q = H_0 * H_1 * ... * H_{n-1} after left-applications). Householder loses orthogonality
    // far slower than Modified Gram-Schmidt for ill-conditioned inputs.
    internal static Fin<QrResult> QrHouseholder(Matrix matrix, Op key) {
        int m = matrix.Rows.Value;
        int n = matrix.Cols.Value;
        int limit = Math.Min(val1: m, val2: n);
        Matrix initialQ = Matrix.Identity(dim: matrix.Rows);
        (Matrix Q, Matrix R) = toSeq(Enumerable.Range(start: 0, count: limit)).Fold(
            initialState: (Q: initialQ, R: matrix),
            f: (state, j) => HouseholderQrStep(state: state, column: j, m: m, n: n));
        return Fin.Succ(new QrResult(Q: Q, R: R));
    }
    private static (Matrix Q, Matrix R) HouseholderQrStep((Matrix Q, Matrix R) state, int column, int m, int n) {
        int subSize = m - column;
        Matrix rSnap = state.R;
        Arr<double> x = [.. toSeq(Enumerable.Range(start: 0, count: subSize))
            .Map(i => rSnap.At(i: column + i, j: column))];
        double normX = Math.Sqrt(d: x.Fold(initialState: 0.0, f: (sum, v) => sum + (v * v)));
        return normX < RhinoMath.ZeroTolerance
            ? state
            : QrApplyReflector(state: state, column: column, n: n, x: x, normX: normX);
    }
    private static (Matrix Q, Matrix R) QrApplyReflector((Matrix Q, Matrix R) state, int column, int n, Arr<double> x, double normX) {
        double sign = x[0] >= 0.0 ? 1.0 : -1.0;
        double alpha = -sign * normX;
        Arr<double> v = x.SetItem(0, x[0] - alpha);
        double vNormSq = v.Fold(initialState: 0.0, f: (sum, value) => sum + (value * value));
        return vNormSq < RhinoMath.ZeroTolerance
            ? state
            : (Q: ReflectRightSubspace(matrix: state.Q, column: column, v: v, beta: 2.0 / vNormSq),
               R: ReflectLeftSubspace(matrix: state.R, column: column, n: n, v: v, beta: 2.0 / vNormSq));
    }
    // Apply H = I - beta v v^T to matrix M from the LEFT (only rows column..m and columns
    // column..n change). Computes column dot products against v then subtracts the rank-1
    // update beta * v * dot_k along each column k >= column.
    private static Matrix ReflectLeftSubspace(Matrix matrix, int column, int n, Arr<double> v, double beta) {
        int subSize = v.Count;
        Matrix self = matrix;
        return toSeq(Enumerable.Range(start: column, count: n - column)).Fold(
            initialState: matrix,
            f: (m, k) => {
                double colDot = toSeq(Enumerable.Range(start: 0, count: subSize))
                    .Fold(initialState: 0.0, f: (sum, l) => sum + (v[l] * self.At(i: column + l, j: k)));
                return toSeq(Enumerable.Range(start: 0, count: subSize)).Fold(
                    initialState: m,
                    f: (mm, l) => mm.With(i: column + l, j: k, value: mm.At(i: column + l, j: k) - (beta * v[l] * colDot)));
            });
    }
    // Apply H = I - beta v v^T to matrix M from the RIGHT (only columns column..column+|v|
    // change). Computes row dot products against v then subtracts the rank-1 update.
    private static Matrix ReflectRightSubspace(Matrix matrix, int column, Arr<double> v, double beta) {
        int subSize = v.Count;
        int rows = matrix.Rows.Value;
        Matrix self = matrix;
        return toSeq(Enumerable.Range(start: 0, count: rows)).Fold(
            initialState: matrix,
            f: (m, i) => {
                double rowDot = toSeq(Enumerable.Range(start: 0, count: subSize))
                    .Fold(initialState: 0.0, f: (sum, l) => sum + (self.At(i: i, j: column + l) * v[l]));
                return toSeq(Enumerable.Range(start: 0, count: subSize)).Fold(
                    initialState: m,
                    f: (mm, l) => mm.With(i: i, j: column + l, value: mm.At(i: i, j: column + l) - (beta * rowDot * v[l])));
            });
    }

    // --- [SYMMETRIC_EIGEN] ----------------------------------------------------------------
    // Jacobi cyclic eigendecomposition for symmetric N x N matrices. State carries A and the
    // N columns of V (the accumulated orthogonal rotation). After convergence A is diagonal and
    // V columns are eigenvectors. The sweep order is canonical lexicographic (p, q) with p < q
    // traversal, materialised as a Seq once per call.
    internal static Fin<Seq<(double Eigenvalue, Arr<double> Eigenvector)>> JacobiSymmetric(SymmetricMatrix matrix, Op key) {
        int n = matrix.Dimension.Value;
        Seq<(int P, int Q)> pairs = SweepPairs(dimension: n);
        Arr<Arr<double>> identity = [.. Enumerable.Range(start: 0, count: n).Select(i =>
            new Arr<double>(Enumerable.Range(start: 0, count: n).Select(j => i == j ? 1.0 : 0.0)))];
        (SymmetricMatrix A, Arr<Arr<double>> V) = toSeq(Enumerable.Range(start: 0, count: MaxSweeps)).Fold(
            initialState: (A: matrix, V: identity),
            f: (state, _) => SymmetricConverged(matrix: state.A, pairs: pairs, dimension: n)
                ? state
                : pairs.Fold(initialState: state, f: static (s, pq) => Givens(state: s, p: pq.P, q: pq.Q)));
        return Fin.Succ(toSeq(Enumerable.Range(start: 0, count: n)
            .Select(i => (Eigenvalue: A.At(i: i, j: i), Eigenvector: V[i]))
            .OrderByDescending(static p => Math.Abs(value: p.Eigenvalue))));
    }
    internal static Seq<(int P, int Q)> SweepPairs(int dimension) =>
        toSeq(from p in Enumerable.Range(start: 0, count: dimension - 1)
              from q in Enumerable.Range(start: p + 1, count: dimension - 1 - p)
              select (P: p, Q: q));
    private static bool SymmetricConverged(SymmetricMatrix matrix, Seq<(int P, int Q)> pairs, int dimension) {
        double offDiag = pairs.Fold(initialState: 0.0, f: (sum, pq) => sum + Math.Abs(value: matrix.At(i: pq.P, j: pq.Q)));
        double diag = toSeq(Enumerable.Range(start: 0, count: dimension))
            .Fold(initialState: 0.0, f: (sum, i) => sum + Math.Abs(value: matrix.At(i: i, j: i)));
        return offDiag < JacobiEpsilon * diag;
    }
    // One Givens rotation on the (p, q) plane: zeros A_pq, updates A_pp / A_qq and the
    // off-diagonal entries A_rp / A_rq for every r not in {p, q}, and rotates the corresponding
    // V_p / V_q eigenvector columns. Skip threshold is relative-scaled to the diagonal entries
    // to avoid spurious work on already-converged pairs at any matrix magnitude.
    private static (SymmetricMatrix A, Arr<Arr<double>> V) Givens(
        (SymmetricMatrix A, Arr<Arr<double>> V) state, int p, int q) {
        double apq = state.A.At(i: p, j: q);
        double scale = Math.Sqrt(d: Math.Abs(value: state.A.At(i: p, j: p)) * Math.Abs(value: state.A.At(i: q, j: q)));
        return Math.Abs(value: apq) < RhinoMath.SqrtEpsilon * Math.Max(val1: RhinoMath.ZeroTolerance, val2: scale)
            ? state
            : ApplyJacobi(state: state, p: p, q: q);
    }
    // Computes Givens (cos, sin) from the symmetric 2x2 eigenvalue problem with diagonals
    // (app, aqq) and off-diagonal apq. Uses the sign-preserving tangent formula
    // t = sign(theta) / (|theta| + sqrt(1 + theta^2)) to avoid catastrophic cancellation.
    private static (double Cos, double Sin) ComputeGivensAngle(double app, double aqq, double apq) {
        double theta = (aqq - app) / (2.0 * apq);
        double t = theta >= 0.0
            ? 1.0 / (theta + Math.Sqrt(d: 1.0 + (theta * theta)))
            : 1.0 / (theta - Math.Sqrt(d: 1.0 + (theta * theta)));
        double cos = 1.0 / Math.Sqrt(d: 1.0 + (t * t));
        return (Cos: cos, Sin: t * cos);
    }
    private static (SymmetricMatrix A, Arr<Arr<double>> V) ApplyJacobi(
        (SymmetricMatrix A, Arr<Arr<double>> V) state, int p, int q) {
        double apq = state.A.At(i: p, j: q);
        double app = state.A.At(i: p, j: p);
        double aqq = state.A.At(i: q, j: q);
        (double cosVal, double sinVal) = ComputeGivensAngle(app: app, aqq: aqq, apq: apq);
        double newApp = (cosVal * cosVal * app) - (2.0 * sinVal * cosVal * apq) + (sinVal * sinVal * aqq);
        double newAqq = (sinVal * sinVal * app) + (2.0 * sinVal * cosVal * apq) + (cosVal * cosVal * aqq);
        int dimension = state.A.Dimension.Value;
        SymmetricMatrix newA = toSeq(Enumerable.Range(start: 0, count: dimension))
            .Filter(r => r != p && r != q)
            .Fold(
                initialState: state.A.With(i: p, j: p, value: newApp).With(i: q, j: q, value: newAqq).With(i: p, j: q, value: 0.0),
                f: (m, r) => m.With(i: r, j: p, value: (cosVal * state.A.At(i: r, j: p)) - (sinVal * state.A.At(i: r, j: q)))
                    .With(i: r, j: q, value: (sinVal * state.A.At(i: r, j: p)) + (cosVal * state.A.At(i: r, j: q))));
        Arr<double> oldVp = state.V[p];
        Arr<double> oldVq = state.V[q];
        Arr<double> newVp = [.. Enumerable.Range(start: 0, count: dimension).Select(k => (cosVal * oldVp[k]) - (sinVal * oldVq[k]))];
        Arr<double> newVq = [.. Enumerable.Range(start: 0, count: dimension).Select(k => (sinVal * oldVp[k]) + (cosVal * oldVq[k]))];
        return (A: newA, V: state.V.SetItem(p, newVp).SetItem(q, newVq));
    }

    // --- [CHOLESKY] -----------------------------------------------------------------------
    // Right-looking Cholesky: A = L * Lᵀ where L is lower triangular. Each column k computes
    // L_kk = sqrt(A_kk - sum_{j<k} L_kj²) and L_ik = (A_ik - sum_{j<k} L_ij * L_kj) / L_kk.
    // Fails on non-SPD input (negative or zero pivot).
    internal static Fin<Matrix> Cholesky(SymmetricMatrix matrix, Op key) {
        int n = matrix.Dimension.Value;
        return toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: Fin.Succ(Matrix.Zero(rows: matrix.Dimension, cols: matrix.Dimension)),
            f: (acc, k) => acc.Bind(L => CholeskyColumn(matrix: matrix, L: L, k: k, n: n, key: key)));
    }
    private static Fin<Matrix> CholeskyColumn(SymmetricMatrix matrix, Matrix L, int k, int n, Op key) {
        double sumDiag = toSeq(Enumerable.Range(start: 0, count: k))
            .Fold(initialState: 0.0, f: (sum, j) => sum + (L.At(i: k, j: j) * L.At(i: k, j: j)));
        double diagVal = matrix.At(i: k, j: k) - sumDiag;
        return diagVal > RhinoMath.ZeroTolerance
            ? toSeq(Enumerable.Range(start: k + 1, count: n - k - 1)).Fold(
                initialState: Fin.Succ(L.With(i: k, j: k, value: Math.Sqrt(d: diagVal))),
                f: (rowAcc, i) => rowAcc.Map(Lk => {
                    double sumOff = toSeq(Enumerable.Range(start: 0, count: k))
                        .Fold(initialState: 0.0, f: (sum, j) => sum + (Lk.At(i: i, j: j) * Lk.At(i: k, j: j)));
                    return Lk.With(i: i, j: k, value: (matrix.At(i: i, j: k) - sumOff) / Lk.At(i: k, j: k));
                }))
            : Fin.Fail<Matrix>(error: key.InvalidInput());
    }

    // --- [LU] -----------------------------------------------------------------------------
    // LU with partial pivoting (Doolittle). PA = LU where L is unit lower triangular and U is
    // upper triangular. Permutation[i] = original row index now placed at row i; SwapCount
    // tracks the number of row exchanges (used for determinant sign).
    internal static Fin<LuResult> LuPartialPivot(Matrix matrix, Op key) {
        int n = matrix.Rows.Value;
        Arr<int> initialPerm = [.. Enumerable.Range(start: 0, count: n)];
        return n != matrix.Cols.Value
            ? Fin.Fail<LuResult>(error: key.InvalidInput())
            : toSeq(Enumerable.Range(start: 0, count: n - 1)).Fold(
                initialState: Fin.Succ((A: matrix, P: initialPerm, Swaps: 0)),
                f: (acc, k) => acc.Bind(state => LuStep(state: state, k: k, n: n, key: key)))
                .Map(state => SplitLu(combined: state.A, perm: state.P, swaps: state.Swaps, n: n));
    }
    private static Fin<(Matrix A, Arr<int> P, int Swaps)> LuStep((Matrix A, Arr<int> P, int Swaps) state, int k, int n, Op key) {
        Matrix A = state.A;
        (int pivotRow, double pivotVal) = toSeq(Enumerable.Range(start: k, count: n - k))
            .Fold(initialState: (Row: k, Value: 0.0),
                f: (best, r) => Math.Abs(value: A.At(i: r, j: k)) > Math.Abs(value: best.Value)
                    ? (Row: r, Value: A.At(i: r, j: k))
                    : best);
        return Math.Abs(value: pivotVal) <= RhinoMath.ZeroTolerance
            ? Fin.Fail<(Matrix, Arr<int>, int)>(error: key.InvalidResult())
            : Fin.Succ(LuPivotAndEliminate(state: state, k: k, pivotRow: pivotRow, n: n));
    }
    private static (Matrix A, Arr<int> P, int Swaps) LuPivotAndEliminate((Matrix A, Arr<int> P, int Swaps) state, int k, int pivotRow, int n) {
        Matrix source = state.A;
        Matrix swapped = pivotRow == k
            ? state.A
            : toSeq(Enumerable.Range(start: 0, count: n)).Fold(
                initialState: state.A,
                f: (m, j) => m.With(i: k, j: j, value: source.At(i: pivotRow, j: j)).With(i: pivotRow, j: j, value: source.At(i: k, j: j)));
        Arr<int> newPerm = pivotRow == k ? state.P : state.P.SetItem(k, state.P[pivotRow]).SetItem(pivotRow, state.P[k]);
        int newSwaps = pivotRow == k ? state.Swaps : state.Swaps + 1;
        double pivot = swapped.At(i: k, j: k);
        Matrix eliminated = toSeq(Enumerable.Range(start: k + 1, count: n - k - 1)).Fold(
            initialState: swapped,
            f: (m, i) => {
                double factor = m.At(i: i, j: k) / pivot;
                Matrix afterStore = m.With(i: i, j: k, value: factor);
                return toSeq(Enumerable.Range(start: k + 1, count: n - k - 1)).Fold(
                    initialState: afterStore,
                    f: (mm, j) => mm.With(i: i, j: j, value: mm.At(i: i, j: j) - (factor * mm.At(i: k, j: j))));
            });
        return (A: eliminated, P: newPerm, Swaps: newSwaps);
    }
    private static LuResult SplitLu(Matrix combined, Arr<int> perm, int swaps, int n) {
        Matrix self = combined;
        Matrix L = new(Rows: combined.Rows, Cols: combined.Cols,
            Entries: [.. toSeq(Enumerable.Range(start: 0, count: n * n))
                .Map(idx => (idx / n) switch {
                    int i when i == idx % n => 1.0,
                    int i when i > idx % n => self.At(i: i, j: idx % n),
                    _ => 0.0,
                })]);
        Matrix U = new(Rows: combined.Rows, Cols: combined.Cols,
            Entries: [.. toSeq(Enumerable.Range(start: 0, count: n * n))
                .Map(idx => (idx / n) switch {
                    int i when i <= idx % n => self.At(i: i, j: idx % n),
                    _ => 0.0,
                })]);
        return new LuResult(Permutation: perm, SwapCount: swaps, L: L, U: U);
    }

    // --- [HESSENBERG] ---------------------------------------------------------------------
    // Householder reductions to upper Hessenberg form. For each column k in [0, n-2):
    // build a reflector that zeroes out entries below position (k+1, k), then apply it from
    // both sides. Result: H = Qᵀ A Q with H_ij = 0 for i > j + 1.
    internal static Fin<Matrix> Hessenberg(Matrix matrix, Op key) {
        int n = matrix.Rows.Value;
        return n != matrix.Cols.Value
            ? Fin.Fail<Matrix>(error: key.InvalidInput())
            : Fin.Succ(toSeq(Enumerable.Range(start: 0, count: n - 2)).Fold(
                initialState: matrix,
                f: (A, k) => HouseholderStep(matrix: A, column: k, n: n)));
    }
    private static Matrix HouseholderStep(Matrix matrix, int column, int n) {
        int subSize = n - column - 1;
        Arr<double> x = [.. toSeq(Enumerable.Range(start: column + 1, count: subSize))
            .Map(i => matrix.At(i: i, j: column))];
        double normX = Math.Sqrt(d: x.Fold(initialState: 0.0, f: (sum, v) => sum + (v * v)));
        return normX < RhinoMath.ZeroTolerance
            ? matrix
            : ApplyHouseholder(matrix: matrix, column: column, n: n, x: x, normX: normX);
    }
    private static Matrix ApplyHouseholder(Matrix matrix, int column, int n, Arr<double> x, double normX) {
        double sign = x[0] >= 0.0 ? 1.0 : -1.0;
        double alpha = -sign * normX;
        Arr<double> v = x.SetItem(0, x[0] - alpha);
        double vNormSq = v.Fold(initialState: 0.0, f: (sum, value) => sum + (value * value));
        return vNormSq < RhinoMath.ZeroTolerance
            ? matrix
            : ApplyReflector(matrix: matrix, column: column, n: n, v: v, beta: 2.0 / vNormSq);
    }
    private static Matrix ApplyReflector(Matrix matrix, int column, int n, Arr<double> v, double beta) {
        int subSize = v.Count;
        int subStart = column + 1;
        Matrix afterLeft = toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: matrix,
            f: (m, j) => {
                double dot = toSeq(Enumerable.Range(start: 0, count: subSize))
                    .Fold(initialState: 0.0, f: (sum, k) => sum + (v[k] * m.At(i: subStart + k, j: j)));
                return toSeq(Enumerable.Range(start: 0, count: subSize)).Fold(
                    initialState: m,
                    f: (mm, k) => mm.With(i: subStart + k, j: j, value: mm.At(i: subStart + k, j: j) - (beta * v[k] * dot)));
            });
        Matrix afterRight = toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: afterLeft,
            f: (m, i) => {
                double dot = toSeq(Enumerable.Range(start: 0, count: subSize))
                    .Fold(initialState: 0.0, f: (sum, k) => sum + (v[k] * m.At(i: i, j: subStart + k)));
                return toSeq(Enumerable.Range(start: 0, count: subSize)).Fold(
                    initialState: m,
                    f: (mm, k) => mm.With(i: i, j: subStart + k, value: mm.At(i: i, j: subStart + k) - (beta * v[k] * dot)));
            });
        return afterRight;
    }

    // --- [GENERAL_EIGEN] ------------------------------------------------------------------
    // Francis-style explicit-shift QR with single Wilkinson shift, applied to the Hessenberg
    // form. After convergence, 1x1 diagonal blocks give real eigenvalues; 2x2 blocks give
    // conjugate pairs via characteristic polynomial. Eigenvectors are recovered via inverse
    // iteration on (H - lambda I).
    internal static Fin<Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)>> FrancisQr(Matrix matrix, Op key) =>
        matrix.Rows.Value != matrix.Cols.Value
            ? Fin.Fail<Seq<(Complex, Arr<Complex>)>>(error: key.InvalidInput())
            : Hessenberg(matrix: matrix, key: key)
                .Bind(H => ConvergeQr(H: H, key: key))
                .Bind(T => ExtractEigenpairs(matrix: matrix, schur: T, key: key));
    private static Fin<Matrix> ConvergeQr(Matrix H, Op key) {
        int n = H.Rows.Value;
        Matrix converged = toSeq(Enumerable.Range(start: 0, count: MaxQrIterations)).Fold(
            initialState: (H, ConvergedSize: n),
            f: static (state, _) => state.ConvergedSize <= 1
                ? state
                : QrSweep(state: state)).H;
        return Fin.Succ(converged);
    }
    private static (Matrix H, int ConvergedSize) QrSweep((Matrix H, int ConvergedSize) state) {
        int p = state.ConvergedSize - 1;
        double subdiag = Math.Abs(value: state.H.At(i: p, j: p - 1));
        double scale = Math.Abs(value: state.H.At(i: p - 1, j: p - 1)) + Math.Abs(value: state.H.At(i: p, j: p));
        return subdiag < QrEpsilon * Math.Max(val1: RhinoMath.ZeroTolerance, val2: scale)
            ? (state.H, ConvergedSize: state.ConvergedSize - 1)
            : (WilkinsonShiftQr(H: state.H, p: p), state.ConvergedSize);
    }
    private static Matrix WilkinsonShiftQr(Matrix H, int p) {
        double a = H.At(i: p - 1, j: p - 1);
        double b = H.At(i: p - 1, j: p);
        double c = H.At(i: p, j: p - 1);
        double d = H.At(i: p, j: p);
        double trace = a + d;
        double det = (a * d) - (b * c);
        double disc = (trace * trace * 0.25) - det;
        double shift = disc >= 0.0
            ? (Math.Abs(value: (trace * 0.5) - Math.Sqrt(d: disc) - d) < Math.Abs(value: (trace * 0.5) + Math.Sqrt(d: disc) - d)
                ? (trace * 0.5) - Math.Sqrt(d: disc)
                : (trace * 0.5) + Math.Sqrt(d: disc))
            : trace * 0.5;
        int n = H.Rows.Value;
        Matrix shifted = toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: H,
            f: (m, i) => m.With(i: i, j: i, value: m.At(i: i, j: i) - shift));
        Matrix afterStep = GivensSweepQr(H: shifted, p: p);
        return toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: afterStep,
            f: (m, i) => m.With(i: i, j: i, value: m.At(i: i, j: i) + shift));
    }
    private static Matrix GivensSweepQr(Matrix H, int p) {
        int n = H.Rows.Value;
        return toSeq(Enumerable.Range(start: 0, count: p)).Fold(
            initialState: H,
            f: (m, k) => ApplyGivensQr(matrix: m, k: k, p: p, n: n));
    }
    private static Matrix ApplyGivensQr(Matrix matrix, int k, int p, int n) {
        double a = matrix.At(i: k, j: k);
        double b = matrix.At(i: k + 1, j: k);
        double r = Math.Sqrt(d: (a * a) + (b * b));
        return r < RhinoMath.ZeroTolerance
            ? matrix
            : GivensRotate(matrix: matrix, k: k, n: n, cos: a / r, sin: b / r);
    }
    private static Matrix GivensRotate(Matrix matrix, int k, int n, double cos, double sin) {
        Matrix afterLeft = toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: matrix,
            f: (m, j) => {
                double top = m.At(i: k, j: j);
                double bot = m.At(i: k + 1, j: j);
                return m.With(i: k, j: j, value: (cos * top) + (sin * bot))
                    .With(i: k + 1, j: j, value: (-sin * top) + (cos * bot));
            });
        return toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: afterLeft,
            f: (m, i) => {
                double left = m.At(i: i, j: k);
                double right = m.At(i: i, j: k + 1);
                return m.With(i: i, j: k, value: (cos * left) + (sin * right))
                    .With(i: i, j: k + 1, value: (-sin * left) + (cos * right));
            });
    }
    private static Fin<Seq<(Complex Eigenvalue, Arr<Complex> Eigenvector)>> ExtractEigenpairs(Matrix matrix, Matrix schur, Op key) {
        int n = schur.Rows.Value;
        Seq<Complex> eigenvalues = ExtractEigenvalues(schur: schur, n: n);
        return eigenvalues.Map(value => InverseIteration(matrix: matrix, eigenvalue: value, key: key)
            .Map(eigenvector => (Eigenvalue: value, Eigenvector: eigenvector)))
            .TraverseM(static pair => pair).As();
    }
    private static Seq<Complex> ExtractEigenvalues(Matrix schur, int n) =>
        toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: (Index: 0, Values: Seq<Complex>()),
            f: (state, _) => state.Index >= n
                ? state
                : state.Index < n - 1 && Math.Abs(value: schur.At(i: state.Index + 1, j: state.Index)) > QrEpsilon
                    ? (Index: state.Index + 2, Values: state.Values.Concat(ExtractPair(schur: schur, k: state.Index)).ToSeq())
                    : (Index: state.Index + 1, Values: state.Values.Add(new Complex(real: schur.At(i: state.Index, j: state.Index), imaginary: 0.0)))).Values;
    private static Seq<Complex> ExtractPair(Matrix schur, int k) {
        double a = schur.At(i: k, j: k);
        double b = schur.At(i: k, j: k + 1);
        double c = schur.At(i: k + 1, j: k);
        double d = schur.At(i: k + 1, j: k + 1);
        double trace = a + d;
        double det = (a * d) - (b * c);
        double disc = (trace * trace * 0.25) - det;
        return disc >= 0.0
            ? Seq(new Complex(real: (trace * 0.5) + Math.Sqrt(d: disc), imaginary: 0.0),
                  new Complex(real: (trace * 0.5) - Math.Sqrt(d: disc), imaginary: 0.0))
            : Seq(new Complex(real: trace * 0.5, imaginary: Math.Sqrt(d: -disc)),
                  new Complex(real: trace * 0.5, imaginary: -Math.Sqrt(d: -disc)));
    }
    // Inverse iteration with cached LU. (A - lambda I) is factorised ONCE per eigenvalue; the
    // 32-iteration loop reuses forward + back substitution against the cached factors. Real
    // shifts factor the n x n shifted matrix; complex shifts factor the 2n x 2n real
    // augmentation [[M, imagI],[-imagI, M]] which encodes the complex system.
    private const int MaxInverseIterations = 32;
    private const double InverseIterationEpsilon = 1e-10;
    private static Fin<Arr<Complex>> InverseIteration(Matrix matrix, Complex eigenvalue, Op key) {
        int n = matrix.Rows.Value;
        Arr<Complex> initial = [.. Enumerable.Range(start: 0, count: n).Select(_ => new Complex(real: 1.0, imaginary: 0.0))];
        return FactorShifted(matrix: matrix, eigenvalue: eigenvalue, key: key)
            .Map(fact => toSeq(Enumerable.Range(start: 0, count: MaxInverseIterations)).Fold(
                initialState: (V: initial, Done: false),
                f: (s, _) => s.Done
                    ? s
                    : fact.Apply(rhs: s.V) switch {
                        Arr<Complex> solved => Normalize(values: solved) switch {
                            Arr<Complex> normalised => (V: normalised, Done: ConvergedComplex(previous: s.V, current: normalised)),
                        },
                    }).V);
    }
    private readonly record struct ShiftedFactorisation(LuResult Lu, bool IsRealShift, int Original) {
        internal Arr<Complex> Apply(Arr<Complex> rhs) {
            int n = Original;
            return IsRealShift
                ? ApplyRealLu(L: Lu.L, U: Lu.U, perm: Lu.Permutation, rhs: rhs, n: n)
                : ApplyAugmentedLu(L: Lu.L, U: Lu.U, perm: Lu.Permutation, rhs: rhs, n: n);
        }
    }
    private static Fin<ShiftedFactorisation> FactorShifted(Matrix matrix, Complex eigenvalue, Op key) {
        int n = matrix.Rows.Value;
        Matrix shifted = ShiftDiagonal(matrix: matrix, n: n, by: eigenvalue.Real);
        return Math.Abs(value: eigenvalue.Imaginary) < QrEpsilon
            ? shifted.DecomposeLu(key: key).Map(lu => new ShiftedFactorisation(Lu: lu, IsRealShift: true, Original: n))
            : BuildComplexAugmented(shifted: shifted, n: n, imag: eigenvalue.Imaginary).DecomposeLu(key: key)
                .Map(lu => new ShiftedFactorisation(Lu: lu, IsRealShift: false, Original: n));
    }
    private static Matrix ShiftDiagonal(Matrix matrix, int n, double by) {
        Matrix self = matrix;
        return toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: matrix,
            f: (m, i) => m.With(i: i, j: i, value: self.At(i: i, j: i) - by));
    }
    private static Matrix BuildComplexAugmented(Matrix shifted, int n, double imag) {
        Matrix self = shifted;
        Dimension big = Dimension.Create(value: 2 * n);
        return new Matrix(Rows: big, Cols: big,
            Entries: [.. toSeq(Enumerable.Range(start: 0, count: 4 * n * n))
                .Map(idx => {
                    int row = idx / (2 * n);
                    int col = idx % (2 * n);
                    int subRow = row % n;
                    int subCol = col % n;
                    return (row < n, col < n) switch {
                        (true, true) or (false, false) => self.At(i: subRow, j: subCol),
                        (true, false) when subRow == subCol => imag,
                        (false, true) when subRow == subCol => -imag,
                        _ => 0.0,
                    };
                })]);
    }
    private static Arr<Complex> ApplyRealLu(Matrix L, Matrix U, Arr<int> perm, Arr<Complex> rhs, int n) {
        Arr<Complex> permuted = [.. Enumerable.Range(start: 0, count: n).Select(i => rhs[perm[i]])];
        Arr<Complex> y = ForwardSolveComplex(L: L, b: permuted, n: n);
        return BackSolveComplex(U: U, b: y, n: n);
    }
    private static Arr<Complex> ApplyAugmentedLu(Matrix L, Matrix U, Arr<int> perm, Arr<Complex> rhs, int n) {
        Arr<double> bigRhs = [.. Enumerable.Range(start: 0, count: 2 * n)
            .Select(i => i < n ? rhs[i].Real : rhs[i - n].Imaginary)];
        Arr<double> permuted = [.. Enumerable.Range(start: 0, count: 2 * n).Select(i => bigRhs[perm[i]])];
        Arr<double> y = ForwardSolveReal(L: L, b: permuted, n: 2 * n);
        Arr<double> x = BackSolveReal(U: U, b: y, n: 2 * n);
        return [.. Enumerable.Range(start: 0, count: n).Select(i => new Complex(real: x[i], imaginary: x[i + n]))];
    }
    private static bool ConvergedComplex(Arr<Complex> previous, Arr<Complex> current) {
        double diff = toSeq(Enumerable.Range(start: 0, count: previous.Count))
            .Fold(initialState: 0.0, f: (sum, i) => sum + Complex.Abs(value: current[i] - previous[i]));
        double scale = toSeq(Enumerable.Range(start: 0, count: previous.Count))
            .Fold(initialState: 0.0, f: (sum, i) => sum + Complex.Abs(value: current[i]));
        return diff < InverseIterationEpsilon * Math.Max(val1: RhinoMath.ZeroTolerance, val2: scale);
    }
    private static Arr<Complex> ForwardSolveComplex(Matrix L, Arr<Complex> b, int n) =>
        toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: new Arr<Complex>(Enumerable.Repeat(element: Complex.Zero, count: n)),
            f: (y, i) => y.SetItem(i,
                (b[i] - toSeq(Enumerable.Range(start: 0, count: i))
                    .Fold(initialState: Complex.Zero, f: (sum, j) => sum + (L.At(i: i, j: j) * y[j]))) / L.At(i: i, j: i)));
    private static Arr<Complex> BackSolveComplex(Matrix U, Arr<Complex> b, int n) =>
        toSeq(Enumerable.Range(start: 0, count: n).Reverse()).Fold(
            initialState: new Arr<Complex>(Enumerable.Repeat(element: Complex.Zero, count: n)),
            f: (x, i) => x.SetItem(i,
                (b[i] - toSeq(Enumerable.Range(start: i + 1, count: n - i - 1))
                    .Fold(initialState: Complex.Zero, f: (sum, j) => sum + (U.At(i: i, j: j) * x[j]))) / U.At(i: i, j: i)));
    private static Arr<double> ForwardSolveReal(Matrix L, Arr<double> b, int n) =>
        toSeq(Enumerable.Range(start: 0, count: n)).Fold(
            initialState: new Arr<double>(Enumerable.Repeat(element: 0.0, count: n)),
            f: (y, i) => y.SetItem(i,
                (b[i] - toSeq(Enumerable.Range(start: 0, count: i))
                    .Fold(initialState: 0.0, f: (sum, j) => sum + (L.At(i: i, j: j) * y[j]))) / L.At(i: i, j: i)));
    private static Arr<double> BackSolveReal(Matrix U, Arr<double> b, int n) =>
        toSeq(Enumerable.Range(start: 0, count: n).Reverse()).Fold(
            initialState: new Arr<double>(Enumerable.Repeat(element: 0.0, count: n)),
            f: (x, i) => x.SetItem(i,
                (b[i] - toSeq(Enumerable.Range(start: i + 1, count: n - i - 1))
                    .Fold(initialState: 0.0, f: (sum, j) => sum + (U.At(i: i, j: j) * x[j]))) / U.At(i: i, j: i)));
    private static Arr<Complex> Normalize(Arr<Complex> values) {
        double normSq = values.Fold(initialState: 0.0,
            f: (sum, value) => sum + (value.Real * value.Real) + (value.Imaginary * value.Imaginary));
        double norm = Math.Sqrt(d: normSq);
        return norm < RhinoMath.ZeroTolerance
            ? values
            : [.. values.AsIterable().Select(v => v / norm)];
    }

    // --- [SVD_GOLUB_REINSCH] --------------------------------------------------------------
    // Golub-Reinsch SVD: Householder bidiagonalisation reduces A (m x n) to upper bidiagonal
    // B with U_left and V_right accumulators capturing the left/right reflectors. The
    // bidiagonalised form is then diagonalised by Givens-based rotations operating on B's
    // columns; because B has at most one super-diagonal entry per row the rotations converge
    // dramatically faster than they would on the dense A. Final U/V incorporate the
    // bidiagonalisation accumulators; singular values are sign-normalised and sorted
    // descending. The pipeline shares `ReflectLeftSubspace`/`ReflectRightSubspace` with QR
    // and Hessenberg, keeping Householder algebra unified.
    internal static Fin<SvdResult> SvdJacobi(Matrix matrix, Op key) {
        int m = matrix.Rows.Value;
        int n = matrix.Cols.Value;
        // Phase 1: Bidiagonalise via two-sided Householder. A = U_bidiag * B * V_bidiag^T.
        (Matrix uBidiag, Matrix bidiag, Matrix vBidiag) = Bidiagonalise(matrix: matrix, m: m, n: n);
        // Phase 2: Givens sweep on B's columns until orthogonal. After convergence
        // B_rotated = B * V_jacobi and ||B_rotated[:, j]|| == sigma_j. V_jacobi composes
        // with V_bidiag to give the final V.
        (Matrix bRotated, Matrix vJacobi) = toSeq(Enumerable.Range(start: 0, count: MaxSweeps)).Fold(
            initialState: (A: bidiag, V: vBidiag),
            f: (state, _) => BidiagonalSweepConverged(matrix: state.A, n: n)
                ? state
                : BidiagonalSweep(state: state, n: n));
        // Phase 3: thin-SVD U_j = (U_bidiag * B_rotated[:, perm[j]]) / sigma_j; V is V_jacobi.
        return Fin.Succ(BuildSortedSvd(UBidiag: uBidiag, BRotated: bRotated, V: vJacobi, m: m, n: n));
    }
    // Phase 1 ----------------------------------------------------------------------------
    private static (Matrix U, Matrix B, Matrix V) Bidiagonalise(Matrix matrix, int m, int n) {
        int limit = Math.Min(val1: m, val2: n);
        Matrix uInit = Matrix.Identity(dim: matrix.Rows);
        Matrix vInit = Matrix.Identity(dim: matrix.Cols);
        return toSeq(Enumerable.Range(start: 0, count: limit)).Fold(
            initialState: (U: uInit, B: matrix, V: vInit),
            f: (state, k) => BidiagonaliseStep(state: state, k: k, m: m, n: n));
    }
    private static (Matrix U, Matrix B, Matrix V) BidiagonaliseStep((Matrix U, Matrix B, Matrix V) state, int k, int m, int n) {
        (Matrix uAfter, Matrix bAfterLeft) = LeftHouseholderColumn(U: state.U, B: state.B, k: k, m: m, n: n);
        return k < n - 1
            ? RightHouseholderRow(B: bAfterLeft, V: state.V, k: k, n: n) switch {
                (Matrix bAfterRight, Matrix vAfter) => (uAfter, bAfterRight, vAfter),
            }
            : (uAfter, bAfterLeft, state.V);
    }
    private static (Matrix U, Matrix B) LeftHouseholderColumn(Matrix U, Matrix B, int k, int m, int n) {
        int subSize = m - k;
        Matrix bSnap = B;
        Arr<double> x = [.. toSeq(Enumerable.Range(start: 0, count: subSize))
            .Map(i => bSnap.At(i: k + i, j: k))];
        double normX = Math.Sqrt(d: x.Fold(initialState: 0.0, f: static (sum, v) => sum + (v * v)));
        return normX < RhinoMath.ZeroTolerance
            ? (U, B)
            : ApplyBidiagonalLeftReflector(U: U, B: B, k: k, n: n, x: x, normX: normX);
    }
    private static (Matrix U, Matrix B) ApplyBidiagonalLeftReflector(Matrix U, Matrix B, int k, int n, Arr<double> x, double normX) {
        double sign = x[0] >= 0.0 ? 1.0 : -1.0;
        double alpha = -sign * normX;
        Arr<double> v = x.SetItem(0, x[0] - alpha);
        double vNormSq = v.Fold(initialState: 0.0, f: static (sum, value) => sum + (value * value));
        return vNormSq < RhinoMath.ZeroTolerance
            ? (U, B)
            : (U: ReflectRightSubspace(matrix: U, column: k, v: v, beta: 2.0 / vNormSq),
               B: ReflectLeftSubspace(matrix: B, column: k, n: n, v: v, beta: 2.0 / vNormSq));
    }
    private static (Matrix B, Matrix V) RightHouseholderRow(Matrix B, Matrix V, int k, int n) {
        int subSize = n - k - 1;
        Matrix bSnap = B;
        Arr<double> y = [.. toSeq(Enumerable.Range(start: 0, count: subSize))
            .Map(i => bSnap.At(i: k, j: k + 1 + i))];
        double normY = Math.Sqrt(d: y.Fold(initialState: 0.0, f: static (sum, v) => sum + (v * v)));
        return normY < RhinoMath.ZeroTolerance
            ? (B, V)
            : ApplyBidiagonalRightReflector(B: B, V: V, k: k, y: y, normY: normY);
    }
    private static (Matrix B, Matrix V) ApplyBidiagonalRightReflector(Matrix B, Matrix V, int k, Arr<double> y, double normY) {
        double sign = y[0] >= 0.0 ? 1.0 : -1.0;
        double alpha = -sign * normY;
        Arr<double> w = y.SetItem(0, y[0] - alpha);
        double wNormSq = w.Fold(initialState: 0.0, f: static (sum, value) => sum + (value * value));
        return wNormSq < RhinoMath.ZeroTolerance
            ? (B, V)
            : (B: ReflectRightSubspace(matrix: B, column: k + 1, v: w, beta: 2.0 / wNormSq),
               V: ReflectRightSubspace(matrix: V, column: k + 1, v: w, beta: 2.0 / wNormSq));
    }
    // Phase 2 ----------------------------------------------------------------------------
    // Givens sweep on bidiagonal-conditioned A: rotates column pairs (p, q) to orthogonalise.
    // Equivalent to one-sided Jacobi but starting from B (much closer to diagonal than A).
    private static bool BidiagonalSweepConverged(Matrix matrix, int n) {
        double offDiag = toSeq(
            from p in Enumerable.Range(start: 0, count: n - 1)
            from q in Enumerable.Range(start: p + 1, count: n - 1 - p)
            select (P: p, Q: q))
            .Fold(initialState: 0.0,
                f: (sum, pq) => sum + Math.Abs(value: BidiagonalColumnDot(matrix: matrix, p: pq.P, q: pq.Q)));
        double diag = toSeq(Enumerable.Range(start: 0, count: n))
            .Fold(initialState: 0.0, f: (sum, i) => sum + BidiagonalColumnDot(matrix: matrix, p: i, q: i));
        return offDiag < JacobiEpsilon * Math.Max(val1: RhinoMath.ZeroTolerance, val2: diag);
    }
    private static double BidiagonalColumnDot(Matrix matrix, int p, int q) {
        int m = matrix.Rows.Value;
        Matrix self = matrix;
        return toSeq(Enumerable.Range(start: 0, count: m))
            .Fold(initialState: 0.0, f: (sum, i) => sum + (self.At(i: i, j: p) * self.At(i: i, j: q)));
    }
    private static (Matrix A, Matrix V) BidiagonalSweep((Matrix A, Matrix V) state, int n) =>
        toSeq(from p in Enumerable.Range(start: 0, count: n - 1)
              from q in Enumerable.Range(start: p + 1, count: n - 1 - p)
              select (P: p, Q: q))
            .Fold(initialState: state, f: static (s, pq) => BidiagonalRotatePair(state: s, p: pq.P, q: pq.Q));
    private static (Matrix A, Matrix V) BidiagonalRotatePair((Matrix A, Matrix V) state, int p, int q) {
        double app = BidiagonalColumnDot(matrix: state.A, p: p, q: p);
        double aqq = BidiagonalColumnDot(matrix: state.A, p: q, q: q);
        double apq = BidiagonalColumnDot(matrix: state.A, p: p, q: q);
        double scale = Math.Sqrt(d: app * aqq);
        return Math.Abs(value: apq) < RhinoMath.SqrtEpsilon * Math.Max(val1: RhinoMath.ZeroTolerance, val2: scale)
            ? state
            : ApplyBidiagonalColumnRotation(state: state, p: p, q: q, app: app, aqq: aqq, apq: apq);
    }
    private static (Matrix A, Matrix V) ApplyBidiagonalColumnRotation((Matrix A, Matrix V) state, int p, int q, double app, double aqq, double apq) {
        (double cosVal, double sinVal) = ComputeGivensAngle(app: app, aqq: aqq, apq: apq);
        return (A: RotateBidiagonalColumns(matrix: state.A, p: p, q: q, cos: cosVal, sin: sinVal),
                V: RotateBidiagonalColumns(matrix: state.V, p: p, q: q, cos: cosVal, sin: sinVal));
    }
    private static Matrix RotateBidiagonalColumns(Matrix matrix, int p, int q, double cos, double sin) {
        int m = matrix.Rows.Value;
        Matrix self = matrix;
        return toSeq(Enumerable.Range(start: 0, count: m)).Fold(
            initialState: matrix,
            f: (mm, i) => mm.With(i: i, j: p, value: (cos * self.At(i: i, j: p)) - (sin * self.At(i: i, j: q)))
                .With(i: i, j: q, value: (sin * self.At(i: i, j: p)) + (cos * self.At(i: i, j: q))));
    }
    // Phase 3 ----------------------------------------------------------------------------
    // Final assembly: column norms of BRotated == singular values; U_intermediate[:, j] =
    // BRotated[:, perm[j]] / sigma_j; final U = UBidiag * U_intermediate. V is the already-
    // composed Jacobi V (which incorporates VBidiag because Phase 2 was seeded with VBidiag).
    // Singular values are sorted descending; columns of U and V are permuted to match.
    private static SvdResult BuildSortedSvd(Matrix UBidiag, Matrix BRotated, Matrix V, int m, int n) {
        Seq<(double Norm, int Column)> norms = toSeq(Enumerable.Range(start: 0, count: n))
            .Map(j => (Norm: Math.Sqrt(d: BidiagonalColumnDot(matrix: BRotated, p: j, q: j)), Column: j))
            .OrderByDescending(static pair => pair.Norm)
            .AsIterable()
            .ToSeq();
        Arr<double> sigma = [.. norms.Map(static pair => pair.Norm)];
        Matrix sortedV = new(Rows: V.Rows, Cols: V.Cols,
            Entries: [.. toSeq(Enumerable.Range(start: 0, count: n * n))
                .Map(idx => V.At(i: idx / n, j: norms[idx % n].Column))]);
        // U_intermediate is m x n: column j = BRotated[:, perm[j]] / sigma_j (zero if
        // singular value is degenerate).
        Matrix uIntermediate = new(Rows: Dimension.Create(value: m), Cols: Dimension.Create(value: n),
            Entries: [.. toSeq(Enumerable.Range(start: 0, count: m * n))
                .Map(idx => norms[idx % n].Norm > RhinoMath.ZeroTolerance
                    ? BRotated.At(i: idx / n, j: norms[idx % n].Column) / norms[idx % n].Norm
                    : 0.0)]);
        // Compose with the bidiagonal U accumulator to recover the SVD identity A = U Σ Vᵀ.
        Matrix sortedU = UBidiag * uIntermediate;
        return new SvdResult(U: sortedU, Sigma: sigma, V: sortedV);
    }
}

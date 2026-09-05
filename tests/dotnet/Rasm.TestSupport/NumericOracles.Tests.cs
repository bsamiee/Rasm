namespace Rasm.TestSupport;

// --- [CONSTANTS] -----------------------------------------------------------------------
internal static class Shapes {
    public static readonly double[][] UnitSquare = Rows(2, 0.0, 0.0, 1.0, 0.0, 1.0, 1.0, 0.0, 1.0);
    public static readonly double[][] UnitCubeVertices = Rows(3, 0.0, 0.0, 0.0, 1.0, 0.0, 0.0, 1.0, 1.0, 0.0, 0.0, 1.0, 0.0, 0.0, 0.0, 1.0, 1.0, 0.0, 1.0, 1.0, 1.0, 1.0, 0.0, 1.0, 1.0);
    // Quads wound counterclockwise seen from outside
    public static readonly int[][] UnitCubeFaces = Rows(4, 0, 3, 2, 1, 4, 5, 6, 7, 0, 1, 5, 4, 2, 3, 7, 6, 0, 4, 7, 3, 1, 2, 6, 5);

    public static T[][] Rows<T>(int width, params T[] values) => [.. values.Chunk(width)];
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class NumericOraclesTests {
    // The double determinant rounds both edge coordinates to the line and reports collinear, the exact predicate sees the nudge
    [Fact]
    public void OrientationSignIsExactWhereTheDoubleDeterminantIsNot() {
        Assert.Equal(0, NumericOracles.OrientationSign(Shapes.Rows(2, 0.5, 0.5, 12.0, 12.0, 24.0, 24.0)));
        Assert.Equal(1, NumericOracles.OrientationSign(Shapes.Rows(2, 0.5, Math.BitIncrement(0.5), 12.0, 12.0, 24.0, 24.0)));
        Assert.Equal(-1, NumericOracles.OrientationSign(Shapes.Rows(2, 0.5, Math.BitDecrement(0.5), 12.0, 12.0, 24.0, 24.0)));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void OrientationSignNegatesWhenTwoPointsSwap(int dimension) =>
        TestAssertions.ForAll(Generators.ModerateMagnitude.Array[dimension].Array[dimension + 1], static simplex =>
            Assert.Equal(-NumericOracles.OrientationSign(simplex), NumericOracles.OrientationSign([simplex[1], simplex[0], .. simplex.Skip(2)])));

    [Fact]
    public void ShoelaceAreaOfTheUnitSquareIsOneAndNegatesWhenReversed() {
        Assert.Equal(1.0, NumericOracles.SignedShoelaceArea(Shapes.UnitSquare));
        Assert.Equal(-1.0, NumericOracles.SignedShoelaceArea([.. Shapes.UnitSquare.Reverse()]));
    }

    [Fact]
    public void SignedVolumeOfTheUnitCubeIsOneWhereverItSits() =>
        TestAssertions.ForAll(Gen.Double[-100.0, 100.0].Array[3], static shift =>
            TestAssertions.Equal(
                NumericOracles.SignedVolume([.. Shapes.UnitCubeVertices.Select(vertex => vertex.Zip(shift, static (x, s) => x + s).ToArray())], Shapes.UnitCubeFaces),
                1.0, Tolerance.Absolute(1.0e-8), label: "volume"));

    [Fact]
    public void CentroidAndScatterOfTheUnitSquareCorners() {
        Assert.Equal([0.5, 0.5], NumericOracles.Centroid(Shapes.UnitSquare));
        // Four corners half a unit from the centroid on each axis, and the mixed term cancels across the diagonal pairs
        Assert.Equal([1.0, 0.0, 1.0], NumericOracles.ScatterMatrixUpperTriangle(Shapes.UnitSquare));
    }

    [Fact]
    public void DeterminantMultipliesUnderTheMatrixProduct() =>
        TestAssertions.ForAll(Gen.Double[-10.0, 10.0].Array[3].Array[3].Select(Gen.Double[-10.0, 10.0].Array[3].Array[3], static (a, b) => (A: a, B: b)), static pair => {
            double product(int row, int col) => NumericOracles.MatrixProductEntry(3, (r, c) => pair.A[r][c], (r, c) => pair.B[r][c], row, col);
            TestAssertions.Equal(
                NumericOracles.Determinant(3, product),
                NumericOracles.Determinant(3, (r, c) => pair.A[r][c]) * NumericOracles.Determinant(3, (r, c) => pair.B[r][c]),
                Tolerance.Combined(absolute: 1.0e-6, relative: 1.0e-9), label: "determinant");
        });

    // A path graph on n vertices has n-1 edges, and each contributes -1 twice off the diagonal and +1 twice on it
    [Fact]
    public void PathGraphLaplacianIsSymmetricWithZeroRowSumsAndOneEdgePerAdjacentPair() {
        double[][] laplacian = NumericOracles.PathGraphLaplacian(6);
        Assert.Equal(0.0, NumericOracles.SymmetryResidual(6, (row, col) => laplacian[row][col]));
        Assert.All(laplacian, static row => Assert.Equal(0.0, row.Sum()));
        Assert.Equal(4.0 * 5.0, laplacian.Sum(static row => row.Sum(Math.Abs)));
    }

    // Rows of an orthogonal matrix are an orthonormal eigenbasis, at time zero every eigenvalue weight is one, and later the trace is the sum of the weights
    [Fact]
    public void HeatKernelIsTheIdentityAtTimeZeroAndTracesTheEigenvalueWeightsAfterIt() =>
        TestAssertions.ForAll(Generators.OrthogonalMatrix(4), static q => {
            double[] spectrum = [.. Enumerable.Range(1, 4).Select(static index => (double)index)];
            Assert.InRange(
                NumericOracles.EntrywiseResidual(4, 4, static (x, y) => x == y ? 1.0 : 0.0, (x, y) => NumericOracles.HeatKernel(spectrum, (i, v) => q[i][v], t: 0.0, x, y)),
                0.0, 1.0e-10);
            TestAssertions.Equal(
                Enumerable.Range(0, 4).Sum(x => NumericOracles.HeatKernel(spectrum, (i, v) => q[i][v], t: 0.25, x, x)),
                spectrum.Sum(static eigenvalue => Math.Exp(-eigenvalue * 0.25)),
                Tolerance.Relative(1.0e-10), label: "trace");
        });

    [Fact]
    public void ConvergenceOrderOfAQuarteredErrorUnderAHalvedStepIsTwo() {
        TestAssertions.Equal(NumericOracles.ConvergenceOrder(coarseError: 4.0, fineError: 1.0), 2.0, Tolerance.Absolute(1.0e-12), label: "order");
        Assert.True(double.IsNaN(NumericOracles.ConvergenceOrder(coarseError: 0.0, fineError: 1.0)), "a zero coarse error has no order");
        _ = Assert.Throws<ArgumentOutOfRangeException>(static () => NumericOracles.ConvergenceOrder(coarseError: 4.0, fineError: 1.0, stepRatio: 1.0));
    }

    [Fact]
    public void MatrixNormsOfAKnownMatrix() {
        double[][] m = Shapes.Rows(2, 1.0, -2.0, 3.0, 4.0);
        double at(int row, int col) => m[row][col];
        Assert.Equal(4.0, MatrixNorm.MaxAbsoluteEntry.Evaluate(2, 2, at));
        Assert.Equal(6.0, MatrixNorm.L1.Evaluate(2, 2, at));
        Assert.Equal(7.0, MatrixNorm.LInfinity.Evaluate(2, 2, at));
        Assert.Equal(Math.Sqrt(30.0), MatrixNorm.Frobenius.Evaluate(2, 2, at));
    }

    [Fact]
    public void ShapeViolationsThrowInsteadOfReturningNaN() {
        _ = Assert.Throws<ArgumentException>(static () => NumericOracles.Distance([1.0], [1.0, 2.0]));
        _ = Assert.Throws<ArgumentException>(static () => NumericOracles.Centroid(Shapes.UnitSquare, weights: [1.0]));
        _ = Assert.Throws<ArgumentOutOfRangeException>(static () => NumericOracles.PairwiseDistances(Shapes.Rows(2, 0.0, 0.0)));
        _ = Assert.Throws<ArgumentException>(static () => NumericOracles.OrientationSign(Shapes.Rows(2, 0.0, 0.0, 1.0, 1.0)));
    }
}

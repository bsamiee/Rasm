using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Storage;
using Rasm.TestSupport;

namespace Rasm.Tests;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class Numerics {
    [Theory]
    [InlineData(double.MaxValue, double.Epsilon, 2098d)]
    [InlineData(double.Epsilon, double.MaxValue, -2098d)]
    public void ExtremeConvergenceOrder(double coarse, double fine, double expected) {
        double actual = Assert.Single(ReferenceCalculations.ConvergenceOrder(coarse, fine).ToSeq());
        Assert.True(Tolerance.WithinUlps(2).Matches(expected, actual));
    }

    [Fact]
    public void BinaryConvergenceOrder() {
        (int low, int high) = (double.ILogB(double.Epsilon), double.ILogB(double.MaxValue));
        Gen.Int[low, high].SelectMany(Gen.Int[low, high],
                (coarse, fine) => Gen.Int[low - int.Min(coarse, fine), high - int.Max(coarse, fine)],
                static (coarse, fine, shift) => (Coarse: coarse, Fine: fine, Shift: shift))
            .ForAll(static powers => {
                double expected = (powers.Coarse - powers.Fine) / 2d;
                (double coarse, double fine) = (double.ScaleB(1d, powers.Coarse), double.ScaleB(1d, powers.Fine));
                double actual = Assert.Single(ReferenceCalculations.ConvergenceOrder(coarse, fine, 4d).ToSeq());
                Assert.True(Tolerance.WithinUlps(2).Matches(expected, actual));
                double scaled = Assert.Single(ReferenceCalculations.ConvergenceOrder(double.ScaleB(coarse, powers.Shift), double.ScaleB(fine, powers.Shift), 4d).ToSeq());
                Assert.True(Tolerance.WithinUlps(2).Matches(expected, scaled));
            });
    }

    [Theory]
    [InlineData(-1022)]
    [InlineData(0)]
    [InlineData(1023)]
    public void AdjacentConvergenceOrder(int exponent) {
        double coarse = double.ScaleB(1d, exponent);
        double fine = double.BitDecrement(coarse);
        double relative = (coarse - fine) / fine;
        double actual = Assert.Single(ReferenceCalculations.ConvergenceOrder(coarse, fine).ToSeq());
        double lower = relative / ((1d + relative) * double.Log(2d));
        double upper = relative / double.Log(2d);
        Assert.InRange(actual, double.BitDecrement(double.BitDecrement(lower)), double.BitIncrement(double.BitIncrement(upper)));
    }

    [Theory]
    [InlineData(0d, 1d)]
    [InlineData(-1d, 1d)]
    [InlineData(double.NaN, 1d)]
    [InlineData(1d, 0d)]
    [InlineData(1d, -1d)]
    [InlineData(1d, double.NaN)]
    public void MissingConvergenceOrder(double coarse, double fine) =>
        Assert.Empty(ReferenceCalculations.ConvergenceOrder(coarse, fine).ToSeq());

    [Theory]
    [InlineData(0d)]
    [InlineData(1d)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(double.NaN)]
    public void ConvergenceStepIsValidatedFirst(double ratio) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => ReferenceCalculations.ConvergenceOrder(0d, 0d, ratio));

    [Theory]
    [InlineData(double.PositiveInfinity, 1d, double.PositiveInfinity)]
    [InlineData(1d, double.PositiveInfinity, double.NegativeInfinity)]
    [InlineData(double.PositiveInfinity, double.PositiveInfinity, double.NaN)]
    public void InfiniteConvergenceOrder(double coarse, double fine, double expected) =>
        Assert.Equal(expected, Assert.Single(ReferenceCalculations.ConvergenceOrder(coarse, fine).ToSeq()));

    [Fact]
    public void InfiniteConvergenceStep() =>
        Assert.Equal(0d, Assert.Single(ReferenceCalculations.ConvergenceOrder(2d, 1d, double.PositiveInfinity).ToSeq()));

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(17)]
    public void PathLaplacianCoefficients(int count) {
        Matrix<double> incidence = Matrix<double>.Build.Dense(count - 1, count, static (edge, vertex) => vertex == edge ? -1d : vertex == edge + 1 ? 1d : 0d);
        Assert.Equal(incidence.TransposeThisAndMultiply(incidence), ReferenceCalculations.PathGraphLaplacian(count));
    }

    [Fact]
    public void PathLaplacianEdgeEnergy() =>
        Gen.Int[-16, 16].Array[2, 64].Select(Gen.Int[-16, 16], static (values, shift) => (Values: values, Shift: shift)).ForAll(static sample => {
            Vector<double> vector = Vector<double>.Build.DenseOfArray([.. sample.Values.Select(static value => (double)value)]);
            Matrix<double> laplacian = ReferenceCalculations.PathGraphLaplacian(vector.Count);
            double expected = sample.Values.Zip(sample.Values.Skip(1), static (left, right) => double.Pow(right - left, 2d)).Sum();
            Assert.Equal(expected, vector.DotProduct(laplacian * vector));
            Vector<double> shifted = vector + sample.Shift;
            Assert.Equal(expected, shifted.DotProduct(laplacian * shifted));
        });

    [Theory]
    [InlineData(2)]
    [InlineData(1024)]
    [InlineData(100_000)]
    public void PathLaplacianStorage(int count) {
        SparseCompressedRowMatrixStorage<double> storage = Assert.IsType<SparseCompressedRowMatrixStorage<double>>(ReferenceCalculations.PathGraphLaplacian(count).Storage);
        int entries = count + (2 * (count - 1));
        Assert.Equal(entries, storage.ValueCount);
        Assert.Equal(entries, storage.Values.Length);
        Assert.Equal(entries, storage.ColumnIndices.Length);
        Assert.Equal(count + 1, storage.RowPointers.Length);
    }
}

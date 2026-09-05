using System.Globalization;
using System.Text;

namespace Rasm.TestSupport;

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class GeneratorsTests {
    [Fact]
    public void UnitVectorsHaveUnitNorm() =>
        TestAssertions.ForAll(Generators.UnitVector(5), static vector =>
            TestAssertions.Equal(Math.Sqrt(vector.Sum(static x => x * x)), 1.0, Tolerance.Absolute(1.0e-12), label: "norm"));

    [Fact]
    public void OrthogonalMatricesHaveOrthonormalColumnsAndUnitDeterminant() =>
        TestAssertions.ForAll(Generators.OrthogonalMatrix(4), static q => {
            Assert.InRange(NumericOracles.OrthogonalityResidual(4, 4, (row, col) => q[row][col]), 0.0, 1.0e-10);
            TestAssertions.Equal(Math.Abs(NumericOracles.Determinant(4, (row, col) => q[row][col])), 1.0, Tolerance.Absolute(1.0e-10), label: "determinant");
        });

    // The spectrum runs from 1 down to the reciprocal of the condition number, and the determinant is the condition number to the power of minus half the dimension
    [Fact]
    public void SymmetricPositiveDefiniteMatricesHaveTheSpectrumTheConditionNumberSets() =>
        TestAssertions.ForAll(Generators.SymmetricPositiveDefiniteMatrix(3, conditionNumber: 100.0), static m => {
            Assert.InRange(NumericOracles.SymmetryResidual(3, (row, col) => m[row][col]), 0.0, 1.0e-12);
            Assert.All(Enumerable.Range(0, 3), i => Assert.True(m[i][i] > 0.0));
            TestAssertions.Equal(NumericOracles.Determinant(3, (row, col) => m[row][col]), Math.Pow(100.0, -1.5), Tolerance.Relative(1.0e-8), label: "determinant");
        });

    [Fact]
    public void SimplexWeightsSumToOne() =>
        TestAssertions.ForAll(Generators.Simplex(6), static weights =>
            TestAssertions.Equal(NumericOracles.Sum(weights), 1.0, Tolerance.Absolute(1.0e-12), label: "total"));

    [Fact]
    public void RadialPolygonsAreCounterclockwise() =>
        TestAssertions.ForAll(Generators.RadialPolygon(7), static ring => {
            double area = NumericOracles.SignedShoelaceArea(ring);
            Assert.True(area > 0.0, string.Create(CultureInfo.InvariantCulture, $"signed area {area:R} is not positive"));
        });

    [Fact]
    public void CorruptedBytesDifferFromTheOriginalAtExactlyOneIndex() =>
        TestAssertions.ForAll(Generators.CorruptedBytes, static pair => {
            Assert.Equal(pair.Original.Length, pair.Corrupted.Length);
            Assert.Equal(1, pair.Original.Zip(pair.Corrupted).Count(static bytes => bytes.First != bytes.Second));
        });

    // A lone surrogate decodes as the replacement character
    [Fact]
    public void UnicodeStringsSurviveUtf8() =>
        TestAssertions.ForAll(Generators.UnicodeString, static text => Assert.Equal(text, Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(text))));

    [Fact]
    public void ConstructedValuesYieldOnlyValuesTheFactoryAccepts() =>
        TestAssertions.ForAll(Generators.ConstructedValues<int, int>(Gen.Int, TryPositive), static value => Assert.True(value > 0));

    [Fact]
    public void AnyDoubleDrawsNonFiniteValuesAtTheDeclaredRate() =>
        TestAssertions.ChiSquared(Generators.AnyDouble, static value => double.IsFinite(value) ? 0 : 1, 9_500, 500);

    private static bool TryPositive(int value, out int positive) {
        positive = value;
        return value > 0;
    }
}

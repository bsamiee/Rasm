using Xunit.Sdk;

namespace Rasm.TestSupport;

// --- [GENERATORS] ----------------------------------------------------------------------
internal static class ApproximateGenerators {
    // Normal magnitudes, a subnormal times one plus a tiny factor rounds back to itself
    public static readonly Gen<double> Normal = Generators.Finite.Where(static x => Math.Abs(x) is >= 1.0e-300 and <= 1.0e300);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class ToleranceTests {
    [Fact]
    public void EveryValueMatchesItselfUnderAZeroTolerance() =>
        TestAssertions.ForAll(Generators.AnyDouble, static value => Assert.True(Tolerance.Absolute(0.0).Matches(value, value)));

    [Fact]
    public void RelativeToleranceScalesWithTheMagnitude() =>
        TestAssertions.ForAll(ApproximateGenerators.Normal, static value => {
            Assert.True(Tolerance.Relative(1.0e-6).Matches(value, value * (1.0 + 0.5e-6)));
            Assert.False(Tolerance.Relative(1.0e-6).Matches(value, value * (1.0 + 4.0e-6)));
        });

    [Fact]
    public void UlpToleranceCountsAdjacentDoubles() =>
        TestAssertions.ForAll(Generators.Finite, static value => {
            double next = Math.BitIncrement(value);
            Assert.True(Tolerance.WithinUlps(1L).Matches(value, next));
            Assert.False(Tolerance.WithinUlps(1L).Matches(value, Math.BitIncrement(next)));
        });

    [Fact]
    public void NonFiniteValuesMatchByEqualityAndFiniteValuesByTheBound() =>
        TestAssertions.CaseTable(
            ("NaN against NaN", static () => Tolerance.Default.Matches(double.NaN, double.NaN), true),
            ("NaN against zero", static () => Tolerance.Default.Matches(double.NaN, 0.0), false),
            ("infinity against itself", static () => Tolerance.Default.Matches(double.PositiveInfinity, double.PositiveInfinity), true),
            ("infinity against its negation", static () => Tolerance.Default.Matches(double.PositiveInfinity, double.NegativeInfinity), false),
            ("absolute bound exceeded", static () => Tolerance.Absolute(1.0e-9).Matches(0.0, 2.0e-9), false));
}

public sealed class NumericComparisonTests {
    [Fact]
    public void SignInvariantAcceptsTheNegatedVector() =>
        TestAssertions.ForAll(Generators.SmallArray(Generators.Finite), static values =>
            Assert.True(Approximate.Equal(values, [.. values.Select(static x => -x)], Tolerance.Absolute(0.0), NumericComparison.SignInvariant),
                $"negation of {values.Length} values rejected"));

    // A vector with one element negated is neither the vector nor its negation, and a comparison that answers true for every input accepts it
    [Fact]
    public void SignInvariantRejectsAPartiallyNegatedVector() =>
        Assert.False(Approximate.Equal((double[])[1.0, 2.0], (double[])[-1.0, 2.0], Tolerance.Absolute(0.0), NumericComparison.SignInvariant));

    [Fact]
    public void PeriodicAcceptsAnglesAWholePeriodApart() =>
        TestAssertions.ForAll(Generators.Angle, static angle =>
            Assert.True(Approximate.Equal(angle, angle + Math.Tau, Tolerance.Absolute(1.0e-9), NumericComparison.Periodic(Math.Tau))));

    [Fact]
    public void ElementwiseRejectsVectorsOfDifferentLength() =>
        Assert.False(Approximate.Equal((double[])[1.0], (double[])[1.0, 1.0], Tolerance.Default));
}

public sealed class VectorAssertionTests {
    [Fact]
    public void LengthMismatchNamesBothLengths() {
        TrueException failure = Assert.Throws<TrueException>(static () => TestAssertions.Equal((double[])[1.0], (double[])[1.0, 1.0], Tolerance.Default, label: "vector"));
        Assert.Contains("length 1 != 2", failure.Message, StringComparison.Ordinal);
    }

    // The message renders the first eight values and then the total, and a failing assertion over a longer vector is still readable
    [Fact]
    public void LongVectorsRenderTheirLeadingValuesAndTotal() {
        double[] left = [.. Enumerable.Range(0, 10).Select(static index => (double)index)];
        TrueException failure = Assert.Throws<TrueException>(() => TestAssertions.Equal(left, [.. left.Select(static value => value + 1.0)], Tolerance.Absolute(0.0), label: "vector"));
        Assert.Contains(".. 10 total", failure.Message, StringComparison.Ordinal);
    }
}

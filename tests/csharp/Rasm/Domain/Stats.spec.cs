using Rasm.Domain;
using Rasm.TestKit;

namespace Rasm.Tests.Domain;

// --- [CONSTANTS] ----------------------------------------------------------------------------
public static class StatGens {
    public static readonly Op Key = Op.Of(name: "stats-test");
    public static readonly Func<double, double, bool> Approx = static (a, b) =>
        Math.Abs(value: a - b) < 1.0e-6 * Math.Max(val1: 1.0, val2: Math.Abs(value: a) + Math.Abs(value: b));
    public static readonly Gen<Seq<double>> NonEmptyFinite = Gens.Finite.Array[1, 256].Select(static (double[] xs) => toSeq(xs));
    public static readonly Gen<Seq<double>> SingletonFinite = Gens.Finite.Select(static (double x) => Seq(x));
    public static readonly Gen<Seq<double>> ConstantFinite = Gens.Finite.Select(Gen.Int[2, 64], static (double x, int n) => toSeq(Enumerable.Repeat(element: x, count: n)));
    public static readonly Gen<ScalarMetric> ScalarMetricCase = Gen.OneOfConst(ScalarMetric.Magnitude, ScalarMetric.Gaussian, ScalarMetric.Mean);
    public static readonly Gen<ExtremumDirection> ExtremumDirectionCase = Gen.OneOfConst(ExtremumDirection.Maximum, ExtremumDirection.Minimum);
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class StatComputationLaws {
    [Fact]
    public void EmptyInputFails() => Spec.Fail(Stat.Of(values: Seq<double>(), key: StatGens.Key));
    [Fact]
    public void SingletonProducesZeroVariance() =>
        Spec.ForAll(StatGens.SingletonFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key), then: static s => {
            Assert.Equal(expected: 1, actual: s.Count);
            Spec.EqualWithin(left: s.Variance, right: 0.0, tolerance: 1.0e-12, what: "singleton variance");
            Spec.EqualWithin(left: s.Minimum, right: s.Maximum, tolerance: 0.0, what: "singleton extrema collapse");
        }));
    [Fact]
    public void ConstantSequenceCollapsesMeanAndExtrema() =>
        Spec.ForAll(StatGens.ConstantFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key), then: x => {
            double observed = xs[0];
            Spec.EqualWithin(left: x.Mean, right: observed, tolerance: 1.0e-9, what: "constant mean");
            Spec.EqualWithin(left: x.Variance, right: 0.0, tolerance: 1.0e-9, what: "constant variance");
            Spec.EqualWithin(left: x.Minimum, right: observed, tolerance: 0.0, what: "constant minimum");
            Spec.EqualWithin(left: x.Maximum, right: observed, tolerance: 0.0, what: "constant maximum");
        }));
    [Fact]
    public void NaNInputFails() =>
        Spec.Fail(Stat.Of(values: Seq(1.0, 2.0, double.NaN), key: StatGens.Key));
    [Fact]
    public void MeanLiesBetweenExtrema() =>
        Spec.ForAll(StatGens.NonEmptyFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key), then: static s =>
            _ = (s.Mean >= s.Minimum - 1.0e-9) && (s.Mean <= s.Maximum + 1.0e-9) ? true : throw new Xunit.Sdk.XunitException($"mean {s.Mean} escaped bounds [{s.Minimum}, {s.Maximum}]")));
    [Fact]
    public void VarianceIsNonNegative() =>
        Spec.ForAll(StatGens.NonEmptyFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key), then: static s =>
            _ = s.Variance >= 0.0 ? true : throw new Xunit.Sdk.XunitException($"negative variance {s.Variance}")));
    [Fact]
    public void CountMatchesInputLength() =>
        Spec.ForAll(StatGens.NonEmptyFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key), then: s => Assert.Equal(expected: xs.Count, actual: s.Count)));
    [Fact]
    public void MeanMatchesLinqAverage() =>
        Spec.Metamorphic(StatGens.NonEmptyFinite,
            path: static (Seq<double> xs) => Stat.Of(values: xs, key: StatGens.Key).Match(Succ: static s => s.Mean, Fail: static _ => double.NaN),
            oracle: static (Seq<double> xs) => System.Linq.Enumerable.Average(xs.AsIterable()),
            eq: StatGens.Approx);
    [Fact]
    public void VarianceMatchesTextbookTwoPassFormula() =>
        Spec.Metamorphic(StatGens.NonEmptyFinite,
            path: static (Seq<double> xs) => Stat.Of(values: xs, key: StatGens.Key).Match(Succ: static s => s.Variance, Fail: static _ => double.NaN),
            oracle: static (Seq<double> xs) => System.Linq.Enumerable.Average(xs.AsIterable().Select(x => (x - System.Linq.Enumerable.Average(xs.AsIterable())) * (x - System.Linq.Enumerable.Average(xs.AsIterable())))),
            eq: StatGens.Approx);
    [Fact]
    public void RmsMatchesDirectQuadraticMean() =>
        Spec.Metamorphic(StatGens.NonEmptyFinite,
            path: static (Seq<double> xs) => Stat.Of(values: xs, key: StatGens.Key).Match(Succ: static s => s.Rms, Fail: static _ => double.NaN),
            oracle: static (Seq<double> xs) => Math.Sqrt(d: System.Linq.Enumerable.Average(xs.AsIterable().Select(static x => x * x))),
            eq: StatGens.Approx);
    [Fact]
    public void ExtremaMatchLinqMinMax() =>
        Spec.ForAll(StatGens.NonEmptyFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key), then: s => {
            Assert.Equal(expected: System.Linq.Enumerable.Min(xs.AsIterable()), actual: s.Minimum);
            Assert.Equal(expected: System.Linq.Enumerable.Max(xs.AsIterable()), actual: s.Maximum);
        }));
}

public sealed class StatContextLaws {
    [Fact]
    public void ToleranceWithinWhenBoundsBelowTolerance() {
        StatContext context = StatContext.Tolerance(tolerance: 1.0, minimum: -0.5, maximum: 0.75);
        Assert.True(context is StatContext.ToleranceCase { WithinTolerance: true });
    }
    [Fact]
    public void ToleranceOutsideWhenAnyBoundExceedsTolerance() {
        StatContext context = StatContext.Tolerance(tolerance: 0.5, minimum: -0.25, maximum: 1.0);
        Assert.True(context is StatContext.ToleranceCase { WithinTolerance: false });
    }
    [Fact]
    public void ToleranceUsesMaximumAbsoluteBound() =>
        Spec.ForAll(Gens.Positive.Select(Gens.Finite, Gens.Finite), tuple => {
            (double tolerance, double min, double max) = tuple;
            StatContext context = StatContext.Tolerance(tolerance: tolerance, minimum: min, maximum: max);
            bool expected = Math.Max(val1: Math.Abs(value: min), val2: Math.Abs(value: max)) <= tolerance;
            Assert.True(context is StatContext.ToleranceCase t && t.WithinTolerance == expected);
        });
}

public sealed class ScalarMetricCases {
    [Fact] public void MagnitudeKeyIsZero() => Assert.Equal(expected: 0, actual: ScalarMetric.Magnitude.Key);
    [Fact] public void GaussianKeyIsOne() => Assert.Equal(expected: 1, actual: ScalarMetric.Gaussian.Key);
    [Fact] public void MeanKeyIsTwo() => Assert.Equal(expected: 2, actual: ScalarMetric.Mean.Key);
    [Fact]
    public void KeysAreDistinctAcrossCases() =>
        Assert.Equal(expected: 3, actual: new[] { ScalarMetric.Magnitude.Key, ScalarMetric.Gaussian.Key, ScalarMetric.Mean.Key }.Distinct().Count());
}

public sealed class ExtremumDirectionLaws {
    [Fact] public void MaximumHasPositiveOneKey() => Assert.Equal(expected: 1, actual: ExtremumDirection.Maximum.Key);
    [Fact] public void MinimumHasNegativeOneKey() => Assert.Equal(expected: -1, actual: ExtremumDirection.Minimum.Key);
    [Fact]
    public void KeysSumToZeroAcrossBothCases() =>
        Assert.Equal(expected: 0, actual: ExtremumDirection.Maximum.Key + ExtremumDirection.Minimum.Key);
}

using Rasm.Domain;
using Rasm.TestKit;
using Rhino;
using Rhino.Geometry;

namespace Rasm.Tests.Domain;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// LOC overage (~213) is a justified multi-concept owner: Stat (Welford) + Distribution (quantile) + SampleMoment (covariance) + ScalarMetric/CurvatureMode/StatContext/ExtremumDirection/ResidualAggregate dispatch.
internal static class StatGens {
    public static readonly Op Key = Op.Of(name: "stats-test");
    public static readonly Func<double, double, bool> Approx = Gens.Approx(relativeTolerance: 1.0e-6);
    public static readonly Gen<Seq<double>> NonEmptyFinite = Gens.NonEmptySeq(element: Gens.Finite);
    public static readonly Gen<Seq<double>> SingletonFinite = Gens.Finite.Select(static (double x) => Seq(x));
    public static readonly Gen<Seq<double>> ConstantFinite = Gens.Finite.Select(Gen.Int[2, 64], static (double x, int n) => toSeq(Enumerable.Repeat(element: x, count: n)));
    public static readonly Gen<ScalarMetric> ScalarMetricCase = Gen.OneOfConst(ScalarMetric.Magnitude, ScalarMetric.Gaussian, ScalarMetric.Mean);
    public static readonly Gen<ExtremumDirection> ExtremumDirectionCase = Gen.OneOfConst(ExtremumDirection.Maximum, ExtremumDirection.Minimum);
    public static readonly Gen<Seq<ResidualSample>> Residuals = Gens.NonEmptyArray(Gens.Finite.Select(Gen.Int[0, 20], static (double d, int i) =>
        new ResidualSample(Index: i, Location: Point3d.Origin, Distance: Math.Abs(d), Tolerance: 0.5, WithinTolerance: Math.Abs(d) <= 0.5)), max: 24).Select(static rows => toSeq(rows));
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class StatComputationLaws {
    [Fact]
    public void EmptyInputFails() => Spec.Fail(Stat.Of(values: Seq<double>(), key: StatGens.Key));
    [Fact]
    public void SingletonProducesZeroVariance() =>
        Spec.ForAll(StatGens.SingletonFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key), then: static s => {
            Assert.Equal(expected: 1, actual: s.Count);
            Spec.Equal(left: s.Variance, right: 0.0, tolerance: 1.0e-12, what: "singleton variance");
            Spec.Equal(left: s.Minimum, right: s.Maximum, tolerance: 0.0, what: "singleton extrema collapse");
        }));
    [Fact]
    public void ConstantSequenceCollapsesMeanAndExtrema() =>
        Spec.ForAll(StatGens.ConstantFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key), then: x => {
            double observed = xs[0];
            Spec.Equal(left: x.Mean, right: observed, tolerance: 1.0e-9, what: "constant mean");
            Spec.Equal(left: x.Variance, right: 0.0, tolerance: 1.0e-9, what: "constant variance");
            Spec.Equal(left: x.Minimum, right: observed, tolerance: 0.0, what: "constant minimum");
            Spec.Equal(left: x.Maximum, right: observed, tolerance: 0.0, what: "constant maximum");
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
            oracle: static (Seq<double> xs) => Enumerable.Average(xs.AsIterable()),
            eq: StatGens.Approx);
    [Fact]
    public void VarianceMatchesTextbookTwoPassFormula() =>
        Spec.Metamorphic(StatGens.NonEmptyFinite,
            path: static (Seq<double> xs) => Stat.Of(values: xs, key: StatGens.Key).Match(Succ: static s => s.Variance, Fail: static _ => double.NaN),
            oracle: static (Seq<double> xs) => Enumerable.Average(xs.AsIterable().Select(x => (x - Enumerable.Average(xs.AsIterable())) * (x - Enumerable.Average(xs.AsIterable())))),
            eq: StatGens.Approx);
    [Fact]
    public void RmsMatchesDirectQuadraticMean() =>
        Spec.Metamorphic(StatGens.NonEmptyFinite,
            path: static (Seq<double> xs) => Stat.Of(values: xs, key: StatGens.Key).Match(Succ: static s => s.Rms, Fail: static _ => double.NaN),
            oracle: static (Seq<double> xs) => Math.Sqrt(d: Enumerable.Average(xs.AsIterable().Select(static x => x * x))),
            eq: StatGens.Approx);
    [Fact]
    public void ExtremaMatchLinqMinMax() =>
        Spec.ForAll(StatGens.NonEmptyFinite, xs => Spec.Succ(Stat.Of(values: xs, key: StatGens.Key), then: s => {
            Assert.Equal(expected: Enumerable.Min(xs.AsIterable()), actual: s.Minimum);
            Assert.Equal(expected: Enumerable.Max(xs.AsIterable()), actual: s.Maximum);
        }));
}

public sealed class DistributionLaws {
    [Fact]
    public void QuantilesIqrAndMedianMatchSortedInterpolationOracle() =>
        Spec.Metamorphic(StatGens.NonEmptyFinite,
            path: static (Seq<double> xs) => Distribution.Of(values: xs, percentiles: Seq(0.0, 25.0, 50.0, 75.0, 100.0), key: StatGens.Key).Match(Succ: static d => (d.Median, d.Iqr, d.Percentiles[2].Value), Fail: static _ => (double.NaN, double.NaN, double.NaN)),
            oracle: static (Seq<double> xs) => xs.AsIterable().Order().ToArray() switch {
                double[] sorted => (Q(sorted: sorted, fraction: 0.5), Q(sorted: sorted, fraction: 0.75) - Q(sorted: sorted, fraction: 0.25), Q(sorted: sorted, fraction: 0.5)),
            },
            eq: static (l, r) => StatGens.Approx(l.Item1, r.Item1) && StatGens.Approx(l.Item2, r.Item2) && StatGens.Approx(l.Item3, r.Item3));
    [Fact]
    public void RejectsInvalidPercentiles() {
        Spec.Fail(Distribution.Of(values: Seq(1.0, 2.0, 3.0), percentiles: Seq(-1.0), key: StatGens.Key));
        Spec.Fail(Distribution.Of(values: Seq(1.0, 2.0, 3.0), percentiles: Seq(101.0), key: StatGens.Key));
        Spec.ForAll(Gens.NonFinite, value => Spec.Fail(Distribution.Of(values: Seq(1.0, 2.0, 3.0), percentiles: Seq(value), key: StatGens.Key)));
    }
    // Boundary guard: 0/100 are inclusive extrema, and emitted tuples must echo every requested percentile in request order (catches a reorder/drop in valid.Map).
    [Fact]
    public void ValidPercentilesEchoRequestOrderAndBoundariesMapToExtrema() =>
        Spec.ForAll(StatGens.NonEmptyFinite, static xs => {
            Seq<double> requested = Seq(0.0, 10.0, 50.0, 90.0, 100.0);
            Spec.Succ(Distribution.Of(values: xs, percentiles: requested, key: StatGens.Key), then: d => {
                Assert.Equal(expected: requested, actual: d.Percentiles.Map(static p => p.Percentile));
                Spec.Equal(left: d.Percentiles[index: 0].Value, right: Enumerable.Min(xs.AsIterable()), tolerance: 1.0e-9, what: "p0 = min");
                Spec.Equal(left: d.Percentiles[index: 4].Value, right: Enumerable.Max(xs.AsIterable()), tolerance: 1.0e-9, what: "p100 = max");
            });
        });
    private static double Q(double[] sorted, double fraction) =>
        ((sorted.Length - 1) * fraction) switch {
            double idx when Math.Abs(idx - Math.Floor(idx)) <= RhinoMath.ZeroTolerance => sorted[(int)Math.Floor(idx)],
            double idx => sorted[(int)Math.Floor(idx)] + ((sorted[(int)Math.Ceiling(idx)] - sorted[(int)Math.Floor(idx)]) * (idx - Math.Floor(idx))),
        };
}

public sealed class SampleMomentLaws {
    // Independent Numeric.CovarianceUpper oracle vs SampleMoment.Of over bounded Point3d clouds.
    private static readonly Gen<Seq<Point3d>> BoundedCloud = Gens.NonEmptyArray(
        Gen.Double[-12.0, 12.0].Select(Gen.Double[-12.0, 12.0], Gen.Double[-12.0, 12.0], static (double x, double y, double z) => new Point3d(x: x, y: y, z: z)), max: 24)
        .Select(static rows => toSeq(rows));
    [Fact]
    public void MeanAndUpperCovarianceMatchWeightedOracle() =>
        Spec.ForAll(BoundedCloud, static points =>
            Spec.Succ(SampleMoment.Of(rows: points.Map(static p => new Arr<double>([p.X, p.Y, p.Z])), dimension: 3, key: StatGens.Key), then: moment => {
                Point3d centroid = Numeric.Centroid(points: points);
                Spec.Equal(left: moment.Mean, right: new Arr<double>([centroid.X, centroid.Y, centroid.Z]), tolerance: 1.0e-6, what: "weighted mean");
                Spec.Equal(left: moment.UpperCovariance, right: Numeric.CovarianceUpper(points: points), tolerance: 1.0e-6, what: "upper covariance");
            }));
    [Fact]
    public void DegenerateShapesRejectWithInputCategory() {
        Spec.FailCategory(SampleMoment.Of(rows: Seq<Arr<double>>(), dimension: 3, key: StatGens.Key), category: "Input");
        Spec.FailCategory(SampleMoment.Of(rows: Seq(new Arr<double>([1.0, 2.0])), dimension: 3, key: StatGens.Key), category: "Input");
        Spec.FailCategory(SampleMoment.Of(rows: Seq(new Arr<double>([1.0, 2.0, 3.0])), dimension: 0, key: StatGens.Key), category: "Input");
        Spec.FailCategory(SampleMoment.Of(rows: Seq(new Arr<double>([double.NaN, 0.0, 0.0])), dimension: 3, key: StatGens.Key), category: "Input");
    }
}

public sealed class ResidualAndCurvatureLaws {
    [Fact]
    public void ResidualDispatchOwnsDistancesSummaryMaximumDistributionAndUnsupported() =>
        Spec.ForAll(StatGens.Residuals, samples => {
            Spec.Succ(Stat.Residuals<Seq<double>>(samples: samples, key: StatGens.Key, aggregate: ResidualAggregate.Distances), then: distances => Assert.Equal(expected: samples.Count, actual: distances.Count));
            Spec.Succ(Stat.Residuals<Stat>(samples: samples, key: StatGens.Key, aggregate: ResidualAggregate.Summary(tolerance: 0.5)), then: summary => Assert.Equal(expected: samples.Count, actual: summary.Count));
            Spec.Succ(Stat.Residuals<ResidualSample>(samples: samples, key: StatGens.Key, aggregate: ResidualAggregate.Maximum), then: max => Assert.Equal(expected: samples.AsIterable().Max(static s => s.Distance), actual: max.Distance));
            Spec.Succ(Stat.Residuals<Distribution>(samples: samples, key: StatGens.Key, aggregate: ResidualAggregate.Distribution(Seq(50.0))), then: dist => Assert.Single(collection: dist.Percentiles));
            Spec.Fail(Stat.Residuals<int>(samples: samples, key: StatGens.Key, aggregate: ResidualAggregate.Distances));
        });
    [Fact]
    public void ExtremaPreservesToleranceTiesAndCurvatureRoutesMetrics() {
        Seq<(string Id, double Score)> items = Seq(("a", 1.0), ("b", 1.04), ("c", 0.0));
        Assert.Equal<string>(expected: ["a", "b"], actual: Stat.Extrema(items: items, projection: static x => x.Score, tolerance: 0.05, direction: ExtremumDirection.Maximum).Map(static x => x.Id).AsIterable().ToArray());
        Assert.True(CurvatureMode.Vector.IsCurveMagnitude);
        Assert.True(CurvatureMode.Scalar(metric: ScalarMetric.Magnitude).IsCurveMagnitude);
        Assert.False(CurvatureMode.Scalar(metric: ScalarMetric.Gaussian).IsCurveMagnitude);
        Assert.False(CurvatureMode.Scalar(metric: ScalarMetric.Mean).IsCurveMagnitude);
        Assert.Equal<ScalarMetric>(expected: [ScalarMetric.Gaussian, ScalarMetric.Mean], actual: CurvatureMode.Vector.SurfaceMetrics.AsIterable().ToArray());
        Assert.Empty(collection: CurvatureMode.Scalar(metric: ScalarMetric.Magnitude).SurfaceMetrics);
        Assert.Equal<ScalarMetric>(expected: [ScalarMetric.Gaussian], actual: CurvatureMode.Scalar(metric: ScalarMetric.Gaussian).SurfaceMetrics.AsIterable().ToArray());
        Assert.Equal<ScalarMetric>(expected: [ScalarMetric.Mean], actual: CurvatureMode.Scalar(metric: ScalarMetric.Mean).SurfaceMetrics.AsIterable().ToArray());
    }
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
    [Fact]
    public void CasesHaveDenseDistinctKeys() =>
        Spec.Cases(
            items: [ScalarMetric.Magnitude, ScalarMetric.Gaussian, ScalarMetric.Mean],
            key: static metric => metric.Key,
            law: static metric => Assert.Contains(expected: metric.Key, collection: [0, 1, 2]));
    // Magnitude projects Vector3d.Length (managed GetLengthHelper, no P/Invoke); surface metrics carry no Vector3d projection and surface Unsupported (Vector3d, double).
    [Fact]
    public void MagnitudeProjectsVectorLengthAndSurfaceMetricsRejectVectors() =>
        Spec.ForAll(Gens.Vec, static v => {
            Spec.Succ(ScalarMetric.Magnitude.Of(value: v, key: StatGens.Key), then: m => Spec.Equal(left: m, right: v.Length, tolerance: 1.0e-9, what: "magnitude = length"));
            Spec.FailUnsupportedFor(result: ScalarMetric.Gaussian.Of(value: v, key: StatGens.Key), geometryType: typeof(Vector3d), outputType: typeof(double));
            Spec.FailUnsupportedFor(result: ScalarMetric.Mean.Of(value: v, key: StatGens.Key), geometryType: typeof(Vector3d), outputType: typeof(double));
        });
}

public sealed class ExtremumDirectionLaws {
    [Fact]
    public void CasesAreOppositeUnitDirections() {
        ExtremumDirection[] cases = [ExtremumDirection.Maximum, ExtremumDirection.Minimum];
        Spec.Cases(items: cases, key: static direction => direction.Key, law: static direction =>
            Assert.Contains(expected: direction.Key, collection: [1, -1]));
        Assert.Equal(expected: 0, actual: cases.Sum(static direction => direction.Key));
    }
}

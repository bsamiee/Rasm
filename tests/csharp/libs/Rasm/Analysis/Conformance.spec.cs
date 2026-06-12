using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [MODELS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native conformance apply; query owns metric payload dispatch and Project over ResidualSamples.
internal static class ConformanceGens {
    public static readonly Op Key = Op.Of(name: "conformance-test");
    public static readonly Type[] OutputsByKey =
        [typeof(double), typeof(double), typeof(bool), typeof(Stat), typeof(ResidualSample), typeof(ResidualSample), typeof(ResidualSample), typeof(Distribution)];
    // Distances non-monotone in index so an argmax / order swap in Maximum/SignedResidual is caught.
    public static readonly Seq<ResidualSample> Residuals = Seq(
        new ResidualSample(Index: 0, Location: new Point3d(x: 1.0, y: 0.0, z: 0.0), Distance: 2.0, Tolerance: 5.0, WithinTolerance: true),
        new ResidualSample(Index: 1, Location: new Point3d(x: 0.0, y: 1.0, z: 0.0), Distance: 7.0, Tolerance: 5.0, WithinTolerance: false),
        new ResidualSample(Index: 2, Location: new Point3d(x: 0.0, y: 0.0, z: 1.0), Distance: 3.0, Tolerance: 5.0, WithinTolerance: true));
    public static readonly Context Model = Context.Of(absolute: 5.0, relative: 1.0e-3, angle: 1.0e-3, units: Rhino.UnitSystem.Millimeters)
        .ToFin().Match(Succ: static c => c, Fail: static e => throw new InvalidOperationException(e.Message));
    // Independent AcceptsTarget oracle, re-derived from metric semantics rather than production's stored predicate.
    public static bool AcceptsTargetOracle(ConformanceMetric metric, Type target, bool curveSource) =>
        (metric.IsContainment, metric.IsSigned) switch {
            (true, _) => target == typeof(Brep) || target == typeof(Mesh),
            (_, true) => GeometryKernel.CanSignedDistance(type: target),
            _ => GeometryKernel.CanClosest(type: target)
                || (curveSource && (target == typeof(Line) || target == typeof(Circle) || target == typeof(Arc) || target == typeof(Polyline) || GeometryKernel.CanCurveForm(type: target))),
        };
    public static readonly Type[] TargetTypes =
        [typeof(Brep), typeof(Mesh), typeof(Plane), typeof(Sphere), typeof(Line), typeof(Circle), typeof(Arc), typeof(Polyline), typeof(Point3d), typeof(Curve), typeof(Surface)];
    public static readonly bool[] CurveSources = [true, false];
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class ConformanceMetricCatalogLaws {
    [Fact]
    public void KeysAreContiguousAndOutputTypesAreNonUniformPerCase() =>
        Spec.SmartEnumOutputCatalog(items: ConformanceMetric.Items, expectedKeys: [0, 1, 2, 3, 4, 5, 6, 7],
            key: static m => m.Key, output: static m => m.Output, expectedOutput: static m => ConformanceGens.OutputsByKey[m.Key]);
    [Fact]
    public void OutputTypeMultiplicityMatchesDeclaredFamilies() =>
        Assert.Multiple(
            () => Assert.Equal(expected: 2, actual: ConformanceMetric.Items.Count(static m => m.Output == typeof(double))),
            () => Assert.Equal(expected: 1, actual: ConformanceMetric.Items.Count(static m => m.Output == typeof(bool))),
            () => Assert.Equal(expected: 1, actual: ConformanceMetric.Items.Count(static m => m.Output == typeof(Stat))),
            () => Assert.Equal(expected: 3, actual: ConformanceMetric.Items.Count(static m => m.Output == typeof(ResidualSample))),
            () => Assert.Equal(expected: 1, actual: ConformanceMetric.Items.Count(static m => m.Output == typeof(Distribution))));
    [Fact]
    public void OnlyContainmentIsContainmentAndSignedExactlyOverlapsContainmentPlusSignedResidual() {
        Assert.Equal(expected: 1, actual: ConformanceMetric.Items.Count(static m => m.IsContainment));
        Assert.True(condition: ConformanceMetric.Containment.IsContainment && ConformanceMetric.Containment.IsSigned);
        Assert.Equal(expected: 2, actual: ConformanceMetric.Items.Count(static m => m.IsSigned));
        Assert.True(condition: ConformanceMetric.SignedResidual.IsSigned && !ConformanceMetric.SignedResidual.IsContainment);
        Assert.Equal(expected: 2, actual: ConformanceMetric.Items.Count(static m => m.ExactCurveDeviation));
    }
}

public sealed class ConformanceMetricAcceptsTargetLaws {
    [Fact]
    public void DecisionTableMatchesIndependentContainmentSignedCurveSourceOracle() =>
        Spec.Cases(items: ConformanceMetric.Items, key: static m => m.Key, law: static m =>
            _ = ConformanceGens.TargetTypes.AsIterable().Iter(target =>
                _ = ConformanceGens.CurveSources.AsIterable().Iter(curveSource =>
                    Assert.Equal(
                        expected: ConformanceGens.AcceptsTargetOracle(metric: m, target: target, curveSource: curveSource),
                        actual: m.AcceptsTarget(target: target, curveSource: curveSource)))));
    [Fact]
    public void ContainmentAcceptsOnlySolidTopologyRegardlessOfCurveSource() {
        Assert.True(condition: ConformanceMetric.Containment.AcceptsTarget(target: typeof(Brep), curveSource: false));
        Assert.True(condition: ConformanceMetric.Containment.AcceptsTarget(target: typeof(Mesh), curveSource: true));
        Assert.False(condition: ConformanceMetric.Containment.AcceptsTarget(target: typeof(Plane), curveSource: true));
        Assert.True(condition: ConformanceMetric.SignedResidual.AcceptsTarget(target: typeof(Plane), curveSource: false));
        Assert.True(condition: ConformanceMetric.Distance.AcceptsTarget(target: typeof(Line), curveSource: true));
        Assert.False(condition: ConformanceMetric.Containment.AcceptsTarget(target: typeof(Curve), curveSource: true));
    }
}

public sealed class ConformanceFactoryDispatchLaws {
    public static readonly (string Label, AnalysisQuery Query, ConformanceMetric Metric)[] Factories =
        [("Distance", AnalysisQuery.Conformance(metric: ConformanceMetric.Distance, count: 8), ConformanceMetric.Distance),
         ("Rms", AnalysisQuery.Conformance(metric: ConformanceMetric.Rms, count: 8), ConformanceMetric.Rms),
         ("WithinTolerance", AnalysisQuery.Conformance(metric: ConformanceMetric.WithinTolerance, count: 8), ConformanceMetric.WithinTolerance),
         ("Summary", AnalysisQuery.Conformance(metric: ConformanceMetric.Summary, count: 8), ConformanceMetric.Summary),
         ("Maximum", AnalysisQuery.Conformance(metric: ConformanceMetric.Maximum, count: 8), ConformanceMetric.Maximum),
         ("SignedResidual", AnalysisQuery.Conformance(metric: ConformanceMetric.SignedResidual, count: 8), ConformanceMetric.SignedResidual),
         ("Containment", AnalysisQuery.Conformance(metric: ConformanceMetric.Containment, count: 8), ConformanceMetric.Containment),
         ("Distribution", AnalysisQuery.Conformance(metric: ConformanceMetric.Distribution, count: 8, 25.0, 75.0), ConformanceMetric.Distribution)];
    [Fact]
    public void EachFactoryCarriesMatchingMetricAndCount() =>
        _ = Factories.AsIterable().Iter(static f => {
            AnalysisQuery.ConformanceCase aspect = Assert.IsType<AnalysisQuery.ConformanceCase>(@object: f.Query);
            Assert.Same(expected: f.Metric, actual: aspect.Metric);
            Assert.Equal(expected: 8, actual: aspect.Count);
        });
    [Fact]
    public void OperationIsSupportedExactlyWhenOutputAndConformabilityMatch() =>
        Spec.SupportMatrix(
            ("Distance Curve/Plane→double", static () => Analyze.Query<Curve, Plane, double>(AnalysisQuery.Conformance(metric: ConformanceMetric.Distance, count: 8)).IsSupported, true),
            ("SignedResidual Surface/Plane→ResidualSample", static () => Analyze.Query<Surface, Plane, ResidualSample>(AnalysisQuery.Conformance(metric: ConformanceMetric.SignedResidual, count: 8)).IsSupported, true),
            ("Containment Surface/Brep→ResidualSample", static () => Analyze.Query<Surface, Brep, ResidualSample>(AnalysisQuery.Conformance(metric: ConformanceMetric.Containment, count: 8)).IsSupported, true),
            ("Summary Curve/Line→Stat", static () => Analyze.Query<Curve, Line, Stat>(AnalysisQuery.Conformance(metric: ConformanceMetric.Summary, count: 8)).IsSupported, true),
            ("Distribution Curve/Line→Distribution", static () => Analyze.Query<Curve, Line, Distribution>(AnalysisQuery.Conformance(metric: ConformanceMetric.Distribution, count: 8, 50.0)).IsSupported, true),
            ("Distance foreign-output Stat", static () => Analyze.Query<Curve, Plane, Stat>(AnalysisQuery.Conformance(metric: ConformanceMetric.Distance, count: 8)).IsSupported, false),
            ("Containment non-solid Plane", static () => Analyze.Query<Curve, Plane, ResidualSample>(AnalysisQuery.Conformance(metric: ConformanceMetric.Containment, count: 8)).IsSupported, false),
            ("Distance foreign-geometry Point3d", static () => Analyze.Query<Point3d, Plane, double>(AnalysisQuery.Conformance(metric: ConformanceMetric.Distance, count: 8)).IsSupported, false));
}

public sealed class ConformanceRejectCategoryLaws {
    [Fact]
    public void NonPositiveCountRejectsWithInputCategory() =>
        Spec.ForAll(Gen.Int[-64, 0], static count =>
            Spec.Invalid(Analyze.Run<Curve, Plane, double>(query: AnalysisQuery.Conformance(metric: ConformanceMetric.Distance, count: count), input: default((Curve A, Plane B))!),
                then: static error => Assert.Equal(expected: "Input", actual: error.Category())));
    [Fact]
    public void OutputMismatchAndForeignGeometryRejectWithUnsupportedCategory() {
        Spec.Invalid(Analyze.Run<Curve, Plane, Stat>(query: AnalysisQuery.Conformance(metric: ConformanceMetric.Distance, count: 8), input: default((Curve A, Plane B))!),
            then: static error => Assert.Equal(expected: "Unsupported", actual: error.Category()));
        Spec.Invalid(Analyze.Run<Curve, Plane, ResidualSample>(query: AnalysisQuery.Conformance(metric: ConformanceMetric.Containment, count: 8), input: default((Curve A, Plane B))!),
            then: static error => Assert.Equal(expected: "Unsupported", actual: error.Category()));
    }
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public sealed class ConformanceProjectionOracleLaws {
    // INDEPENDENT ORACLE over hand-built ResidualSamples (purely managed Stat.Residuals fold; no native).
    [Fact]
    public void DistanceProjectsEachSampleDistanceInOrder() =>
        Spec.Succ(ConformanceMetric.Distance.Project<double>(residuals: ConformanceGens.Residuals, percentiles: Seq<double>(), context: ConformanceGens.Model, key: ConformanceGens.Key),
            then: static values => Spec.Equal(left: values, right: ConformanceGens.Residuals.Map(static s => s.Distance), tolerance: 0.0, what: "distances"));
    [Fact]
    public void RmsProjectsRootMeanSquareOfDistances() {
        Seq<double> distances = ConformanceGens.Residuals.Map(static s => s.Distance);
        double mean = Numeric.Sum(values: distances) / distances.Count;
        double variance = Numeric.Sum(values: distances.Map(d => (d - mean) * (d - mean))) / distances.Count;
        Spec.Succ(ConformanceMetric.Rms.Project<double>(residuals: ConformanceGens.Residuals, percentiles: Seq<double>(), context: ConformanceGens.Model, key: ConformanceGens.Key),
            then: rms => Spec.Equal(left: rms.Head.IfNone(0.0), right: Math.Sqrt(d: (mean * mean) + variance), tolerance: 1.0e-9, what: "rms"));
    }
    [Fact]
    public void MaximumProjectsArgmaxAndSignedResidualPassesThroughAllSamples() {
        Spec.Succ(ConformanceMetric.Maximum.Project<ResidualSample>(residuals: ConformanceGens.Residuals, percentiles: Seq<double>(), context: ConformanceGens.Model, key: ConformanceGens.Key),
            then: static max => Spec.Equal(left: max.Head.IfNone(default(ResidualSample)).Distance, right: 7.0, tolerance: 0.0, what: "argmax distance"));
        Spec.Succ(ConformanceMetric.SignedResidual.Project<ResidualSample>(residuals: ConformanceGens.Residuals, percentiles: Seq<double>(), context: ConformanceGens.Model, key: ConformanceGens.Key),
            then: static all => Assert.Equal(expected: ConformanceGens.Residuals, actual: all));
    }

    [Fact]
    public void SummaryWithinToleranceContainmentAndDistributionProjectIndependentResidualContracts() {
        Spec.Succ(ConformanceMetric.WithinTolerance.Project<bool>(residuals: ConformanceGens.Residuals, percentiles: Seq<double>(), context: ConformanceGens.Model, key: ConformanceGens.Key),
            then: static within => Assert.Equal(expected: Seq(value: false), actual: within));
        Spec.Succ(ConformanceMetric.Summary.Project<Stat>(residuals: ConformanceGens.Residuals, percentiles: Seq<double>(), context: ConformanceGens.Model, key: ConformanceGens.Key),
            then: static summary => {
                Stat stat = summary.Head.IfNone(default(Stat));
                Assert.Equal(expected: 3, actual: stat.Count);
                Spec.Equal(left: stat.Minimum, right: 2.0, tolerance: 0.0, what: "summary min");
                Spec.Equal(left: stat.Maximum, right: 7.0, tolerance: 0.0, what: "summary max");
                Spec.Equal(left: stat.Mean, right: 4.0, tolerance: 1.0e-12, what: "summary mean");
            });
        Spec.Succ(ConformanceMetric.Containment.Project<ResidualSample>(residuals: ConformanceGens.Residuals, percentiles: Seq<double>(), context: ConformanceGens.Model, key: ConformanceGens.Key),
            then: static all => Assert.Equal(expected: ConformanceGens.Residuals, actual: all));
        Spec.Succ(ConformanceMetric.Distribution.Project<Distribution>(residuals: ConformanceGens.Residuals, percentiles: Seq(0.0, 50.0, 100.0), context: ConformanceGens.Model, key: ConformanceGens.Key),
            then: static distributions => {
                Distribution distribution = distributions.Head.IfNone(default(Distribution));
                Assert.Equal(expected: [0.0, 50.0, 100.0], actual: [.. distribution.Percentiles.Map(static p => p.Percentile).AsIterable()]);
                Assert.Equal(expected: [2.0, 3.0, 7.0], actual: [.. distribution.Percentiles.Map(static p => p.Value).AsIterable()]);
            });
    }

    [Fact]
    public void ForeignOutputTypeProjectsUnsupportedCategory() =>
        Spec.FailCategory(ConformanceMetric.Distance.Project<Plane>(residuals: ConformanceGens.Residuals, percentiles: Seq<double>(), context: ConformanceGens.Model, key: ConformanceGens.Key), category: "Unsupported");
}

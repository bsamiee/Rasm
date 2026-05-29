using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED (*.verify.csx): Conformance.Samples/SampleResiduals/DistanceFor + Run(...).Apply (P/Invoke rhcommon_c).
// Static rail owns: ConformanceMetric catalog (NON-uniform per-case Output), AcceptsTarget vs an independent oracle,
// factory→(Metric,Count), dispatch (Run rejects at Supported() pre-native), Project<TOut> over hand-built ResidualSamples.
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

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
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
    public static readonly (string Label, Conformance Aspect, ConformanceMetric Metric)[] Factories =
        [("Distance", Conformance.Distance(count: 8), ConformanceMetric.Distance),
         ("Rms", Conformance.Rms(count: 8), ConformanceMetric.Rms),
         ("WithinTolerance", Conformance.WithinTolerance(count: 8), ConformanceMetric.WithinTolerance),
         ("Summary", Conformance.Summary(count: 8), ConformanceMetric.Summary),
         ("Maximum", Conformance.Maximum(count: 8), ConformanceMetric.Maximum),
         ("SignedResidual", Conformance.SignedResidual(count: 8), ConformanceMetric.SignedResidual),
         ("Containment", Conformance.Containment(count: 8), ConformanceMetric.Containment),
         ("Distribution", Conformance.Distribution(8, 25.0, 75.0), ConformanceMetric.Distribution)];
    [Fact]
    public void EachFactoryCarriesMatchingMetricAndCount() =>
        _ = Factories.AsIterable().Iter(static f => {
            Assert.Same(expected: f.Metric, actual: f.Aspect.Metric);
            Assert.Equal(expected: 8, actual: f.Aspect.Count);
        });
    [Fact]
    public void OperationIsSupportedExactlyWhenOutputAndConformabilityMatch() =>
        Spec.SupportMatrix(
            ("Distance Curve/Plane→double", static () => Conformance.Distance(count: 8).Operation<Curve, Plane, double>().IsSupported, true),
            ("SignedResidual Surface/Plane→ResidualSample", static () => Conformance.SignedResidual(count: 8).Operation<Surface, Plane, ResidualSample>().IsSupported, true),
            ("Containment Surface/Brep→ResidualSample", static () => Conformance.Containment(count: 8).Operation<Surface, Brep, ResidualSample>().IsSupported, true),
            ("Summary Curve/Line→Stat", static () => Conformance.Summary(count: 8).Operation<Curve, Line, Stat>().IsSupported, true),
            ("Distribution Curve/Line→Distribution", static () => Conformance.Distribution(8, 50.0).Operation<Curve, Line, Distribution>().IsSupported, true),
            ("Distance foreign-output Stat", static () => Conformance.Distance(count: 8).Operation<Curve, Plane, Stat>().IsSupported, false),
            ("Containment non-solid Plane", static () => Conformance.Containment(count: 8).Operation<Curve, Plane, ResidualSample>().IsSupported, false),
            ("Distance foreign-geometry Point3d", static () => Conformance.Distance(count: 8).Operation<Point3d, Plane, double>().IsSupported, false));
}

public sealed class ConformanceRejectCategoryLaws {
    [Fact]
    public void NonPositiveCountRejectsWithInputCategory() =>
        Spec.ForAll(Gen.Int[-64, 0], static count =>
            Spec.Invalid(Analyze.Run(operation: Conformance.Distance(count: count).Operation<Curve, Plane, double>(), input: default((Curve, Plane))!),
                then: static error => Assert.Equal(expected: "Input", actual: error.Category())));
    [Fact]
    public void OutputMismatchAndForeignGeometryRejectWithUnsupportedCategory() {
        Spec.Invalid(Analyze.Run(operation: Conformance.Distance(count: 8).Operation<Curve, Plane, Stat>(), input: default((Curve, Plane))!),
            then: static error => Assert.Equal(expected: "Unsupported", actual: error.Category()));
        Spec.Invalid(Analyze.Run(operation: Conformance.Containment(count: 8).Operation<Curve, Plane, ResidualSample>(), input: default((Curve, Plane))!),
            then: static error => Assert.Equal(expected: "Unsupported", actual: error.Category()));
    }
}

// --- [EDGE_CASES] ---------------------------------------------------------------------------
public sealed class ConformanceProjectionOracleLaws {
    // INDEPENDENT ORACLE over hand-built ResidualSamples (purely managed Stat.Residuals fold; no native).
    [Fact]
    public void DistanceProjectsEachSampleDistanceInOrder() =>
        Spec.Succ(Conformance.Distance(count: 3).Project<double>(residuals: ConformanceGens.Residuals, context: ConformanceGens.Model),
            then: static values => Spec.Equal(left: values, right: ConformanceGens.Residuals.Map(static s => s.Distance), tolerance: 0.0, what: "distances"));
    [Fact]
    public void RmsProjectsRootMeanSquareOfDistances() {
        Seq<double> distances = ConformanceGens.Residuals.Map(static s => s.Distance);
        double mean = Numeric.Sum(values: distances) / distances.Count;
        double variance = Numeric.Sum(values: distances.Map(d => (d - mean) * (d - mean))) / distances.Count;
        Spec.Succ(Conformance.Rms(count: 3).Project<double>(residuals: ConformanceGens.Residuals, context: ConformanceGens.Model),
            then: rms => Spec.Equal(left: rms.Head.IfNone(0.0), right: Math.Sqrt(d: (mean * mean) + variance), tolerance: 1.0e-9, what: "rms"));
    }
    [Fact]
    public void MaximumProjectsArgmaxAndSignedResidualPassesThroughAllSamples() {
        Spec.Succ(Conformance.Maximum(count: 3).Project<ResidualSample>(residuals: ConformanceGens.Residuals, context: ConformanceGens.Model),
            then: static max => Spec.Equal(left: max.Head.IfNone(default(ResidualSample)).Distance, right: 7.0, tolerance: 0.0, what: "argmax distance"));
        Spec.Succ(Conformance.SignedResidual(count: 3).Project<ResidualSample>(residuals: ConformanceGens.Residuals, context: ConformanceGens.Model),
            then: static all => Assert.Equal(expected: ConformanceGens.Residuals, actual: all));
    }
    [Fact]
    public void ForeignOutputTypeProjectsUnsupportedCategory() =>
        Spec.FailCategory(Conformance.Distance(count: 3).Project<Plane>(residuals: ConformanceGens.Residuals, context: ConformanceGens.Model), category: "Unsupported");
}

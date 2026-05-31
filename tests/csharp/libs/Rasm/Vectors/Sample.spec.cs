using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;
using Rhino.Geometry;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class SampleGens {
    public static readonly Op Key = Op.Of(name: "sample-test");
    public static readonly Gen<int> PositiveInt = Gen.Int[start: 1, finish: 64];
    public static readonly Gen<int> NonPositiveInt = Gen.Int[start: -64, finish: 0];
    public static readonly Context Model = Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: Rhino.UnitSystem.Millimeters).ToFin(), label: "sample context");
    public static readonly Seq<Point3d> Points = Gens.UnitTriangle3;
    public static readonly Seq<Point3d> DensePoints = Seq(
        new Point3d(x: 0.0, y: 0.0, z: 0.0),
        new Point3d(x: 1.0, y: 0.0, z: 0.0),
        new Point3d(x: 0.0, y: 1.0, z: 0.0),
        new Point3d(x: 1.0, y: 1.0, z: 0.0),
        new Point3d(x: 0.5, y: 0.5, z: 0.0));
    public static readonly Gen<(double Radius, int Count, int Iterations, int Capacity)> Payloads =
        Gens.Positive.Select(Gen.Int[start: 1, finish: 32], Gen.Int[start: 33, finish: 64], Gen.Int[start: 65, finish: 96],
            static (double radius, int count, int iterations, int capacity) => (Radius: radius, Count: count, Iterations: iterations, Capacity: capacity));
    public static readonly Seq<(Point3d Point, double Mass)> WeightedPoints =
        Seq((new Point3d(x: 0.0, y: 0.0, z: 0.0), 7.0), (new Point3d(x: 1.0, y: 0.0, z: 0.0), 13.0), (new Point3d(x: 0.0, y: 1.0, z: 0.0), 3.0));
    public static VectorCloud Cloud(Option<Seq<double>> mass = default) =>
        Cloud(points: Points, mass: mass);
    public static VectorCloud Cloud(Seq<Point3d> points, Option<Seq<double>> mass = default) =>
        mass.Match(
            Some: weights => Spec.SuccValue(VectorCloud.WeightedCluster(points: points, mass: weights, context: Model, key: Key), label: "weighted sample cloud"),
            None: () => Spec.SuccValue(VectorCloud.Cluster(points: points, context: Model, key: Key), label: "sample cloud"));
    public static ExtractionDomain Domain(VectorCloud cloud) =>
        Spec.SuccValue(ExtractionDomain.Cloud(value: cloud, key: Key), label: "sample cloud domain");
    public static VectorIntent Intent(SampleKind kind, Option<Seq<double>> mass = default) =>
        Spec.SuccValue(VectorIntent.Sample(domain: Domain(cloud: Cloud(mass: mass)), kind: kind, key: Key), label: "sample intent");
    public static VectorIntent Intent(VectorCloud cloud, SampleKind kind) =>
        Spec.SuccValue(VectorIntent.Sample(domain: Domain(cloud: cloud), kind: kind, key: Key), label: "sample intent");
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class SampleKindFactoryLaws {
    [Fact]
    public void FactoriesGatePositiveDimensionsAndPreservePayloads() {
        Spec.ForAll(SampleGens.Payloads, p => {
            Spec.Succ(SampleKind.PoissonDisk(radius: p.Radius, key: SampleGens.Key), then: kind =>
                Spec.Equal(left: Assert.IsType<SampleKind.PoissonDiskCase>(@object: kind).Radius.Value, right: p.Radius, tolerance: 0.0, what: "radius"));
            Spec.Succ(SampleKind.Farthest(count: p.Count, key: SampleGens.Key), then: kind =>
                Assert.Equal(expected: p.Count, actual: Assert.IsType<SampleKind.FarthestCase>(@object: kind).Count.Value));
            Spec.Succ(SampleKind.Optimize(count: p.Count, iterations: p.Iterations, key: SampleGens.Key), then: kind => {
                SampleKind.OptimizeCase optimize = Assert.IsType<SampleKind.OptimizeCase>(@object: kind);
                Assert.Equal(expected: p.Count, actual: optimize.Count.Value);
                Assert.Equal(expected: p.Iterations, actual: optimize.Iterations.Value);
            });
            Spec.Succ(SampleKind.Lloyd(count: p.Count, iterations: p.Iterations, key: SampleGens.Key), then: kind => {
                SampleKind.LloydCase lloyd = Assert.IsType<SampleKind.LloydCase>(@object: kind);
                Assert.Equal(expected: p.Count, actual: lloyd.Count.Value);
                Assert.Equal(expected: p.Iterations, actual: lloyd.Iterations.Value);
            });
            Spec.Succ(SampleKind.Capacity(count: p.Count, capacity: p.Capacity, key: SampleGens.Key), then: kind => {
                SampleKind.CapacityCase capacity = Assert.IsType<SampleKind.CapacityCase>(@object: kind);
                Assert.Equal(expected: p.Count, actual: capacity.Count.Value);
                Assert.Equal(expected: p.Capacity, actual: capacity.Limit.Value);
            });
            Spec.Succ(SampleKind.SampleElimination(count: p.Count, oversampleFactor: 5, alpha: 8.0, beta: 0.65, gamma: 1.5, seed: 17, key: SampleGens.Key), then: kind => {
                SampleKind.SampleEliminationCase elimination = Assert.IsType<SampleKind.SampleEliminationCase>(@object: kind);
                Assert.Equal(expected: (p.Count, 5, 8.0, 0.65, 1.5, 17), actual: (elimination.Count.Value, elimination.OversampleFactor.Value, elimination.Alpha.Value, elimination.Beta.Value, elimination.Gamma.Value, elimination.Seed));
            });
            Spec.Succ(SampleKind.Weighted(points: SampleGens.WeightedPoints, key: SampleGens.Key), then: kind => {
                SampleKind.WeightedCase weighted = Assert.IsType<SampleKind.WeightedCase>(@object: kind);
                Assert.Equal(expected: SampleGens.WeightedPoints.Count, actual: weighted.Points.Count);
                Spec.Equal(left: weighted.Points.Map(static item => item.Mass).Fold(initialState: 0.0, f: static (sum, mass) => sum + mass), right: 23.0, tolerance: 0.0, what: "distinct weighted mass");
            });
        });
        Spec.ForAll(SampleGens.NonPositiveInt, n => {
            Spec.FailCategory(SampleKind.Farthest(count: n, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Optimize(count: n, iterations: 1, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Optimize(count: 1, iterations: n, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Lloyd(count: n, iterations: 1, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Lloyd(count: 1, iterations: n, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Capacity(count: n, capacity: 1, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Capacity(count: 1, capacity: n, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.SampleElimination(count: n, oversampleFactor: 5, alpha: 8.0, beta: 0.65, gamma: 1.5, seed: 17, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.SampleElimination(count: 1, oversampleFactor: n, alpha: 8.0, beta: 0.65, gamma: 1.5, seed: 17, key: SampleGens.Key), category: "Tolerance");
        });
        Spec.ForAll(Gens.NonPositive, r => Spec.FailCategory(SampleKind.PoissonDisk(radius: r, key: SampleGens.Key), category: "Tolerance"));
        Spec.ForAll(Gens.NonFinite, r => Spec.FailCategory(SampleKind.PoissonDisk(radius: r, key: SampleGens.Key), category: "Tolerance"));
        Spec.ForAll(Gens.NonFinite, x =>
            Seq<Func<double, Fin<SampleKind>>>(
                value => SampleKind.SampleElimination(count: 2, oversampleFactor: 5, alpha: value, beta: 0.65, gamma: 1.5, seed: 17, key: SampleGens.Key),
                value => SampleKind.SampleElimination(count: 2, oversampleFactor: 5, alpha: 8.0, beta: value, gamma: 1.5, seed: 17, key: SampleGens.Key),
                value => SampleKind.SampleElimination(count: 2, oversampleFactor: 5, alpha: 8.0, beta: 0.65, gamma: value, seed: 17, key: SampleGens.Key))
                .Iter(factory => Spec.FailCategory(factory(arg: x), category: "Tolerance")));
        Spec.FailCategory(SampleKind.SampleElimination(count: 2, oversampleFactor: 1, alpha: 8.0, beta: 0.65, gamma: 1.5, seed: 17, key: SampleGens.Key), category: "Input");
        Spec.FailCategory(SampleKind.SampleElimination(count: 2, oversampleFactor: 5, alpha: 8.0, beta: 1.1, gamma: 1.5, seed: 17, key: SampleGens.Key), category: "Input");
        Spec.FailCategory(SampleKind.Explicit(points: Seq<Point3d>(), key: SampleGens.Key), category: "Input");
        Spec.FailCategory(SampleKind.Weighted(points: Seq<(Point3d Point, double Mass)>(), key: SampleGens.Key), category: "Input");
        Spec.FailCategory(SampleKind.Weighted(points: Seq((Point3d.Origin, -1.0)), key: SampleGens.Key), category: "Input");
        Spec.SmartEnumCatalogMatches(production: SampleAlgorithmKind.Items, expectedKeys: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9], key: static kind => kind.Key);
        Spec.SmartEnumCatalogMatches(production: SampleStopKind.Items, expectedKeys: [0, 1, 2, 3], key: static kind => kind.Key);
    }
    [Fact]
    public void ExplicitSamplesProjectExactOutputsThroughIntentDomainRail() {
        SampleKind kind = Spec.SuccValue(SampleKind.Explicit(points: SampleGens.Points, key: SampleGens.Key), label: "explicit sample kind");
        VectorIntent intent = SampleGens.Intent(kind: kind);
        Spec.Succ(intent.Project<Seq<Point3d>>(context: SampleGens.Model, key: SampleGens.Key),
            then: points => _ = points.Zip(SampleGens.Points).Iter(pair => Spec.Equal(left: pair.Item1, right: pair.Item2, tolerance: 0.0)));
        Spec.Succ(intent.Project<VectorCloud>(context: SampleGens.Model, key: SampleGens.Key),
            then: projected => {
                VectorCloud.ClusterCase cluster = Assert.IsType<VectorCloud.ClusterCase>(@object: projected);
                Assert.Equal(expected: SampleGens.Points.Count, actual: cluster.Vertices.Count);
                Spec.Equal(left: Numeric.Centroid(points: cluster.Vertices), right: Numeric.Centroid(points: SampleGens.Points), tolerance: 1.0e-12);
            });
        Spec.Succ(intent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key),
            then: receipt => {
                Spec.CountsConserve(attempted: receipt.Attempted, emitted: receipt.Emitted, rejected: receipt.Rejected, label: "explicit receipt");
                Assert.Equal(expected: SampleGens.Points.Count, actual: receipt.Attempted);
                Assert.Equal(expected: SampleGens.Points.Count, actual: receipt.Emitted);
                Assert.Equal(expected: 0, actual: receipt.Rejected);
                Spec.Some(receipt.Algorithm, algorithm => Assert.Equal(expected: SampleAlgorithmKind.Explicit, actual: algorithm.Kind));
            });
        Spec.FailCategory(intent.Project<SymmetricMatrix>(context: SampleGens.Model, key: SampleGens.Key), category: "Unsupported");
    }
    [Fact]
    public void ExplicitReceiptCountsRejectedDomainSamplesWithoutForcingCloudOutput() {
        VectorCloud cloud = SampleGens.Cloud();
        SampleKind partial = Spec.SuccValue(SampleKind.Explicit(points: SampleGens.Points.Add(new Point3d(x: 10.0, y: 0.0, z: 0.0)), key: SampleGens.Key), label: "partial explicit sample kind");
        ExtractionDomain domain = SampleGens.Domain(cloud: cloud);
        VectorIntent intent = Spec.SuccValue(VectorIntent.Sample(domain: domain, kind: partial, key: SampleGens.Key), label: "partial sample intent");
        Spec.Succ(intent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), then: receipt => {
            Spec.CountsConserve(attempted: receipt.Attempted, emitted: receipt.Emitted, rejected: receipt.Rejected, label: "partial receipt");
            Assert.Equal(expected: SampleGens.Points.Count, actual: receipt.Emitted);
            Assert.Equal(expected: 1, actual: receipt.Rejected);
        });
        SampleKind rejected = Spec.SuccValue(SampleKind.Explicit(points: Seq(new Point3d(x: 10.0, y: 0.0, z: 0.0)), key: SampleGens.Key), label: "rejected explicit sample kind");
        VectorIntent rejectedIntent = Spec.SuccValue(VectorIntent.Sample(domain: domain, kind: rejected, key: SampleGens.Key), label: "rejected sample intent");
        Spec.Succ(rejectedIntent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), then: receipt => {
            Spec.CountsConserve(attempted: receipt.Attempted, emitted: receipt.Emitted, rejected: receipt.Rejected, label: "rejected receipt");
            Assert.Equal(expected: 0, actual: receipt.Emitted);
            Assert.Equal(expected: 1, actual: receipt.Rejected);
        });
        Spec.FailCategory(rejectedIntent.Project<VectorCloud>(context: SampleGens.Model, key: SampleGens.Key), category: "Input");
        SampleKind invalid = Spec.SuccValue(SampleKind.Explicit(points: Seq(Point3d.Unset), key: SampleGens.Key), label: "invalid explicit sample kind");
        VectorIntent invalidIntent = Spec.SuccValue(VectorIntent.Sample(domain: domain, kind: invalid, key: SampleGens.Key), label: "invalid sample intent");
        Spec.Succ(invalidIntent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), then: receipt => {
            Spec.CountsConserve(attempted: receipt.Attempted, emitted: receipt.Emitted, rejected: receipt.Rejected, label: "invalid explicit receipt");
            Assert.Equal(expected: 1, actual: receipt.Rejected);
        });
    }
}

public sealed class SampleDensityLaws {
    [Fact]
    public void MeshCandidateDensityMatchesIndependentFormulas() {
        double area = 4.0;
        Spec.Succ(SampleKind.PoissonDisk(radius: 2.0, key: SampleGens.Key), then: kind =>
            Spec.Succ(kind.MeshCandidateDensity(area: area, key: SampleGens.Key), then: density =>
                Spec.Equal(left: density, right: 0.25, tolerance: 1.0e-12, what: "poisson density")));
        Spec.Succ(SampleKind.Explicit(points: SampleGens.Points, key: SampleGens.Key), then: kind =>
            Spec.Succ(kind.MeshCandidateDensity(area: area, key: SampleGens.Key), then: density =>
                Spec.Equal(left: density, right: 0.75, tolerance: 1.0e-12, what: "explicit density")));
        Spec.Succ(SampleKind.Farthest(count: 8, key: SampleGens.Key), then: kind =>
            Spec.Succ(kind.MeshCandidateDensity(area: area, key: SampleGens.Key), then: density =>
                Spec.Equal(left: density, right: 2.0, tolerance: 1.0e-12, what: "farthest density")));
        Spec.Succ(SampleKind.Optimize(count: 6, iterations: 2, key: SampleGens.Key), then: kind =>
            Spec.Succ(kind.MeshCandidateDensity(area: area, key: SampleGens.Key), then: density =>
                Spec.Equal(left: density, right: 1.5, tolerance: 1.0e-12, what: "optimize density")));
        Spec.Succ(SampleKind.Lloyd(count: 4, iterations: 2, key: SampleGens.Key), then: kind =>
            Spec.Succ(kind.MeshCandidateDensity(area: area, key: SampleGens.Key), then: density =>
                Spec.Equal(left: density, right: 1.0, tolerance: 1.0e-12, what: "lloyd density")));
        Spec.Succ(SampleKind.Capacity(count: 3, capacity: 5, key: SampleGens.Key), then: kind =>
            Spec.Succ(kind.MeshCandidateDensity(area: 10.0, key: SampleGens.Key), then: density =>
                Spec.Equal(left: density, right: 1.5, tolerance: 1.0e-12, what: "capacity density")));
        Spec.Succ(SampleKind.SampleElimination(count: 3, oversampleFactor: 5, alpha: 8.0, beta: 0.65, gamma: 1.5, seed: 17, key: SampleGens.Key), then: kind =>
            Spec.Succ(kind.MeshCandidateDensity(area: 10.0, key: SampleGens.Key), then: density =>
                Spec.Equal(left: density, right: 1.5, tolerance: 1.0e-12, what: "wse density")));
    }
    [Fact]
    public void ReceiptConservationIsBoundedByAttemptedCandidates() {
        SampleKind kind = Spec.SuccValue(SampleKind.Explicit(points: SampleGens.Points.Add(new Point3d(x: 10.0, y: 0.0, z: 0.0)), key: SampleGens.Key), label: "explicit sample kind");
        VectorIntent intent = SampleGens.Intent(kind: kind);
        Spec.Succ(intent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), then: receipt => {
            Spec.CountsConserve(attempted: receipt.Attempted, emitted: receipt.Emitted, rejected: receipt.Rejected, label: "density receipt");
            Assert.True(condition: receipt.Emitted <= receipt.Attempted);
            Assert.True(condition: receipt.Rejected >= 0);
        });
    }
    [Fact]
    public void CloudCandidateMassIsNormalizedAndPreservedInCandidateSampling() {
        VectorIntent intent = SampleGens.Intent(
            kind: Spec.SuccValue(SampleKind.Farthest(count: 2, key: SampleGens.Key), label: "farthest kind"),
            mass: Some(Seq(1000.0, 1.0, 1.0)));
        Spec.Succ(intent.Project<VectorCloud>(context: SampleGens.Model, key: SampleGens.Key), then: projected => {
            VectorCloud.ClusterCase cluster = Assert.IsType<VectorCloud.ClusterCase>(@object: projected);
            Spec.Some(cluster.Mass, mass => {
                Assert.Equal(expected: cluster.Vertices.Count, actual: mass.Count);
                Spec.Equal(left: Enumerable.Sum(source: mass.AsIterable()), right: 1.0, tolerance: 1.0e-12, what: "selected normalized mass");
            });
        });
        Spec.Succ(intent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), then: receipt =>
            Spec.CountsConserve(attempted: receipt.Attempted, emitted: receipt.Emitted, rejected: receipt.Rejected, label: "weighted cloud receipt"));
    }
    [Fact]
    public void ScalarDensityPriorityMultipliesCloudMassDeterministically() {
        SampleKind density = Spec.SuccValue(SampleKind.ScalarDensity(density: ScalarField.Constant(value: 2.0), count: 2, key: SampleGens.Key), label: "density kind");
        VectorIntent weighted = SampleGens.Intent(kind: density, mass: Some(Seq(1000.0, 1.0, 1.0)));
        VectorIntent uniform = SampleGens.Intent(kind: density);
        Arr<double> weightedMass = Spec.SuccValue(weighted.Project<VectorCloud>(context: SampleGens.Model, key: SampleGens.Key), label: "weighted density cloud") is VectorCloud.ClusterCase weightedCluster
            ? weightedCluster.Mass.IfNone(new Arr<double>([]))
            : new Arr<double>([]);
        Arr<double> uniformMass = Spec.SuccValue(uniform.Project<VectorCloud>(context: SampleGens.Model, key: SampleGens.Key), label: "uniform density cloud") is VectorCloud.ClusterCase uniformCluster
            ? uniformCluster.Mass.IfNone(new Arr<double>([]))
            : new Arr<double>([]);
        Assert.Equal(expected: weightedMass.Count, actual: uniformMass.Count);
        Assert.True(condition: Enumerable.Max(source: weightedMass.AsIterable()) > Enumerable.Max(source: uniformMass.AsIterable()));
        Spec.Succ(weighted.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), then: receipt => {
            Spec.Some(receipt.DensityAccepted, accepted => Assert.Equal(expected: SampleGens.Points.Count, actual: accepted));
            Spec.Some(receipt.DensityRejected, rejected => Assert.Equal(expected: 0, actual: rejected));
            Spec.Some(receipt.Algorithm, algorithm => {
                Assert.Equal(expected: SampleAlgorithmKind.ScalarDensityPriority, actual: algorithm.Kind);
                Assert.False(condition: algorithm.MeshSpectrumValidated);
            });
        });
    }
    [Fact]
    public void AdaptiveDensitySelectionIsRepeatablePrioritySelection() {
        SampleKind adaptive = Spec.SuccValue(SampleKind.Adaptive(density: ScalarField.Constant(value: 1.0), count: 2, minSpacing: 0.01, key: SampleGens.Key), label: "adaptive kind");
        VectorIntent intent = SampleGens.Intent(kind: adaptive);
        Seq<Point3d> first = Spec.SuccValue(intent.Project<Seq<Point3d>>(context: SampleGens.Model, key: SampleGens.Key), label: "first adaptive sample");
        Seq<Point3d> second = Spec.SuccValue(intent.Project<Seq<Point3d>>(context: SampleGens.Model, key: SampleGens.Key), label: "second adaptive sample");
        _ = first.Zip(second).Iter(pair => Spec.Equal(left: pair.Item1, right: pair.Item2, tolerance: 0.0));
        Spec.Succ(intent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), then: receipt => {
            Spec.Some(receipt.DensityAccepted, accepted => Assert.Equal(expected: SampleGens.Points.Count, actual: accepted));
            Spec.Some(receipt.DensityRejected, rejected => Assert.Equal(expected: 0, actual: rejected));
            Assert.Equal(expected: SampleDomainStatus.CandidateAccepted, actual: receipt.DomainStatus);
            Spec.Some(receipt.Algorithm, algorithm => Assert.Equal(expected: SampleAlgorithmKind.AdaptivePriority, actual: algorithm.Kind));
        });
    }
    [Fact]
    public void WeightedSampleEliminationSelectsDeterministicSubsetWithReceiptFacts() {
        SampleKind elimination = Spec.SuccValue(SampleKind.SampleElimination(count: 2, oversampleFactor: 2, alpha: 8.0, beta: 0.65, gamma: 1.5, seed: 23, key: SampleGens.Key), label: "wse kind");
        VectorIntent intent = SampleGens.Intent(cloud: SampleGens.Cloud(points: SampleGens.DensePoints), kind: elimination);
        Seq<Point3d> first = Spec.SuccValue(intent.Project<Seq<Point3d>>(context: SampleGens.Model, key: SampleGens.Key), label: "first wse sample");
        Seq<Point3d> second = Spec.SuccValue(intent.Project<Seq<Point3d>>(context: SampleGens.Model, key: SampleGens.Key), label: "second wse sample");
        Assert.Equal(expected: 2, actual: first.Count);
        _ = first.Zip(second).Iter(pair => Spec.Equal(left: pair.Item1, right: pair.Item2, tolerance: 0.0));
        Spec.Succ(intent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), then: receipt => {
            Spec.CountsConserve(attempted: receipt.Attempted, emitted: receipt.Emitted, rejected: receipt.Rejected, label: "wse receipt");
            Assert.Equal(expected: SampleGens.DensePoints.Count, actual: receipt.CandidateCount.IfNone(0));
            Assert.Equal(expected: 2, actual: receipt.Emitted);
            Assert.Equal(expected: 3, actual: receipt.Rejected);
            Spec.Some(receipt.Algorithm, algorithm => {
                Assert.Equal(expected: SampleAlgorithmKind.YukselWeightedSampleElimination, actual: algorithm.Kind);
                Assert.Equal(expected: (23, 2, SampleGens.DensePoints.Count, 2, 3), actual: (algorithm.Seed.IfNone(0), algorithm.TargetCount.IfNone(0), algorithm.OversampleCount.IfNone(0), algorithm.OversampleFactor.IfNone(0), algorithm.Eliminated.IfNone(0)));
                Assert.True(condition: algorithm.NeighborUpdates.IfNone(0) > 0);
                Assert.Equal(expected: (true, true, false, false, false), actual: (algorithm.DeterministicCandidateSource, algorithm.EuclideanMetric, algorithm.MaximalCoverageGuaranteed, algorithm.OtCvtValidated, algorithm.MeshSpectrumValidated));
            });
        });
    }
    [Fact]
    public void PoissonDiskOnCloudStaysUnsupportedInsteadOfClaimingBridson() {
        SampleKind kind = Spec.SuccValue(SampleKind.PoissonDisk(radius: 0.2, key: SampleGens.Key), label: "candidate radius kind");
        VectorIntent intent = SampleGens.Intent(kind: kind);
        Spec.FailCategory(intent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), category: "Unsupported");
    }
}

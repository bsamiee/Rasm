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
    public static readonly Seq<Point3d> Points = Seq(Point3d.Origin, new Point3d(x: 1.0, y: 0.0, z: 0.0), new Point3d(x: 0.0, y: 1.0, z: 0.0));
    public static readonly Gen<(double Radius, int Count, int Iterations, int Capacity)> Payloads =
        Gens.Positive.Select(Gen.Int[start: 1, finish: 32], Gen.Int[start: 33, finish: 64], Gen.Int[start: 65, finish: 96],
            static (double radius, int count, int iterations, int capacity) => (Radius: radius, Count: count, Iterations: iterations, Capacity: capacity));
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class SampleKindFactoryLaws {
    [Fact]
    public void FactoriesGatePositiveDimensionsAndPreservePayloads() {
        Spec.ForAll(SampleGens.Payloads, p => {
            Spec.Succ(SampleKind.PoissonDisk(radius: p.Radius, key: SampleGens.Key), then: kind =>
                Spec.EqualWithin(left: Assert.IsType<SampleKind.PoissonDiskCase>(@object: kind).Radius.Value, right: p.Radius, tolerance: 0.0, what: "radius"));
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
        });
        Spec.ForAll(SampleGens.NonPositiveInt, n => {
            Spec.FailCategory(SampleKind.Farthest(count: n, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Optimize(count: n, iterations: 1, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Optimize(count: 1, iterations: n, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Lloyd(count: n, iterations: 1, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Lloyd(count: 1, iterations: n, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Capacity(count: n, capacity: 1, key: SampleGens.Key), category: "Tolerance");
            Spec.FailCategory(SampleKind.Capacity(count: 1, capacity: n, key: SampleGens.Key), category: "Tolerance");
        });
        Spec.ForAll(Gens.NonPositive, r => Spec.FailCategory(SampleKind.PoissonDisk(radius: r, key: SampleGens.Key), category: "Tolerance"));
    }
    [Fact]
    public void ExplicitSamplesProjectThroughIntentDomainRail() {
        VectorCloud cloud = Spec.SuccValue(VectorCloud.Cluster(points: SampleGens.Points, context: SampleGens.Model, key: SampleGens.Key), label: "sample cloud");
        SampleKind kind = Spec.SuccValue(SampleKind.Explicit(points: SampleGens.Points, key: SampleGens.Key), label: "explicit sample kind");
        VectorIntent intent = VectorIntent.Sample(domain: ExtractionDomain.Cloud(value: cloud), kind: kind);
        Spec.Succ(intent.Project<VectorCloud>(context: SampleGens.Model, key: SampleGens.Key),
            then: projected => Assert.Equal(expected: SampleGens.Points.Count, actual: Assert.IsType<VectorCloud.ClusterCase>(@object: projected).Vertices.Count));
        Spec.Succ(intent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key),
            then: receipt => {
                Assert.Equal(expected: SampleGens.Points.Count, actual: receipt.Attempted);
                Assert.Equal(expected: SampleGens.Points.Count, actual: receipt.Emitted);
                Assert.Equal(expected: 0, actual: receipt.Rejected);
            });
    }
    [Fact]
    public void ExplicitReceiptCountsRejectedDomainSamplesWithoutForcingCloudOutput() {
        VectorCloud cloud = Spec.SuccValue(VectorCloud.Cluster(points: SampleGens.Points, context: SampleGens.Model, key: SampleGens.Key), label: "sample cloud");
        SampleKind partial = Spec.SuccValue(SampleKind.Explicit(points: SampleGens.Points.Add(new Point3d(x: 10.0, y: 0.0, z: 0.0)), key: SampleGens.Key), label: "partial explicit sample kind");
        VectorIntent intent = VectorIntent.Sample(domain: ExtractionDomain.Cloud(value: cloud), kind: partial);
        Spec.Succ(intent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), then: receipt => {
            Assert.Equal(expected: SampleGens.Points.Count + 1, actual: receipt.Attempted);
            Assert.Equal(expected: SampleGens.Points.Count, actual: receipt.Emitted);
            Assert.Equal(expected: 1, actual: receipt.Rejected);
        });
        SampleKind rejected = Spec.SuccValue(SampleKind.Explicit(points: Seq(new Point3d(x: 10.0, y: 0.0, z: 0.0)), key: SampleGens.Key), label: "rejected explicit sample kind");
        VectorIntent rejectedIntent = VectorIntent.Sample(domain: ExtractionDomain.Cloud(value: cloud), kind: rejected);
        Spec.Succ(rejectedIntent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), then: receipt => {
            Assert.Equal(expected: 1, actual: receipt.Attempted);
            Assert.Equal(expected: 0, actual: receipt.Emitted);
            Assert.Equal(expected: 1, actual: receipt.Rejected);
        });
        Spec.Fail(rejectedIntent.Project<VectorCloud>(context: SampleGens.Model, key: SampleGens.Key));
    }
}

public sealed class SampleDensityLaws {
    [Fact]
    public void MeshCandidateDensityMatchesIndependentFormulas() {
        double area = 4.0;
        Spec.Succ(SampleKind.PoissonDisk(radius: 2.0, key: SampleGens.Key), then: kind =>
            Spec.EqualWithin(left: kind.MeshCandidateDensity(area: area), right: 0.25, tolerance: 1.0e-12, what: "poisson density"));
        Spec.Succ(SampleKind.Farthest(count: 8, key: SampleGens.Key), then: kind =>
            Spec.EqualWithin(left: kind.MeshCandidateDensity(area: area), right: 2.0, tolerance: 1.0e-12, what: "farthest density"));
        Spec.Succ(SampleKind.Capacity(count: 3, capacity: 5, key: SampleGens.Key), then: kind =>
            Spec.EqualWithin(left: kind.MeshCandidateDensity(area: 10.0), right: 1.5, tolerance: 1.0e-12, what: "capacity density"));
    }
    [Fact]
    public void ReceiptConservationIsBoundedByAttemptedCandidates() {
        VectorCloud cloud = Spec.SuccValue(VectorCloud.Cluster(points: SampleGens.Points, context: SampleGens.Model, key: SampleGens.Key), label: "sample cloud");
        SampleKind kind = Spec.SuccValue(SampleKind.Explicit(points: SampleGens.Points.Add(new Point3d(x: 10.0, y: 0.0, z: 0.0)), key: SampleGens.Key), label: "explicit sample kind");
        VectorIntent intent = VectorIntent.Sample(domain: ExtractionDomain.Cloud(value: cloud), kind: kind);
        Spec.Succ(intent.Project<SampleReceipt>(context: SampleGens.Model, key: SampleGens.Key), then: receipt => {
            Assert.Equal(expected: receipt.Attempted, actual: receipt.Emitted + receipt.Rejected);
            Assert.True(condition: receipt.Emitted <= receipt.Attempted);
            Assert.True(condition: receipt.Rejected >= 0);
        });
    }
}

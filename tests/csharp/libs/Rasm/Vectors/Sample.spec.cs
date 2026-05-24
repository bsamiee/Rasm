using Rasm.Domain;
using Rasm.TestKit;
using Rasm.Vectors;

namespace Rasm.Tests.Vectors;

// --- [CONSTANTS] ----------------------------------------------------------------------------
internal static class SampleGens {
    public static readonly Op Key = Op.Of(name: "sample-test");
    public static readonly Gen<int> PositiveInt = Gen.Int[start: 1, finish: 64];
    public static readonly Gen<int> NonPositiveInt = Gen.Int[start: -64, finish: 0];
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
        SampleReceipt receipt = new(Attempted: 11, Emitted: 7, Rejected: 4);
        Assert.Equal(expected: receipt.Attempted, actual: receipt.Emitted + receipt.Rejected);
        Assert.True(condition: receipt.Emitted <= receipt.Attempted);
        Assert.True(condition: receipt.Rejected >= 0);
    }
}

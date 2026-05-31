using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [CONSTANTS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native Tree.*; static owns Probe catalog, ValidatePoints, and PointPairs vs an independent sorted oracle.
internal static class SpatialGens {
    public static readonly Op Key = Op.Of(name: "spatial-test");
    public static readonly Point3d[] InvalidPoints =
        [Point3d.Unset, new(x: double.NaN, y: 0.0, z: 0.0),
         new(x: 0.0, y: double.PositiveInfinity, z: 0.0), new(x: 0.0, y: 0.0, z: double.NegativeInfinity)];
    public static readonly Gen<Point3d[]> ValidPoints = Gens.NonEmptyArray(element: Gens.Point, max: 32);
    public static readonly Gen<int[][]> NeighborRows =
        Gen.Int[0, 16].Array[0, 12].Select(static (int[] counts) =>
            counts.Select(static (int n, int needle) => Enumerable.Range(start: needle, count: n % 6).ToArray()).ToArray());
    public static Seq<Couple> ExpandSorted(int[][] rows) =>
        toSeq(rows.SelectMany(static (int[] ids, int needle) => ids.Select(source => new Couple(A: needle, B: source)))
            .OrderBy(static c => c.A).ThenBy(static c => c.B));
}

// --- [ALGEBRAIC] ----------------------------------------------------------------------------
public sealed class ProbeUnionCatalogLaws {
    public static readonly (string Label, Probe Case)[] Cases =
        [("Nearest", Probe.Nearest(count: 5)), ("Within", Probe.Within(distance: 2.5))];
    [Fact]
    public void FactoriesProjectDistinctCasesAndTransportPayloadVerbatim() {
        Assert.Equal(expected: 2, actual: Cases.Select(static c => c.Case.GetType()).Distinct().Count());
        Assert.Equal(expected: 7, actual: Assert.IsType<Probe.NearestCase>(@object: Probe.Nearest(count: 7)).Count);
        Spec.Equal(left: Assert.IsType<Probe.WithinCase>(@object: Probe.Within(distance: -3.25)).Distance,
            right: -3.25, tolerance: 0.0, what: "within distance transport");
    }
    [Fact]
    public void PayloadChannelsAreIndependentAcrossFactories() =>
        Spec.ForAll(Gen.Int[-50, 50].Select(Gens.Finite, static (int count, double distance) => (Count: count, Distance: distance)), static pair => {
            Assert.Equal(expected: pair.Count, actual: Assert.IsType<Probe.NearestCase>(@object: Probe.Nearest(count: pair.Count)).Count);
            Spec.Equal(left: Assert.IsType<Probe.WithinCase>(@object: Probe.Within(distance: pair.Distance)).Distance,
                right: pair.Distance, tolerance: 0.0, what: "within distance");
        });
}

public sealed class ProbeValidationLaws {
    [Fact]
    public void InvalidProbeParametersRejectBeforeNativeNeighborSearch() {
        Spec.FailCategory(result: Spatial.NearestPoints(points: [Point3d.Origin], needles: [Point3d.Origin], probe: Probe.Nearest(count: 0)).Run(ContextFixtureValue), category: "Input");
        Spec.FailCategory(result: Spatial.NearestPoints(points: [Point3d.Origin], needles: [Point3d.Origin], probe: Probe.Within(distance: 0.0)).Run(ContextFixtureValue), category: "Input");
        Spec.FailCategory(result: Spatial.NearestPoints(points: [Point3d.Origin], needles: [Point3d.Origin], probe: Probe.Within(distance: double.NaN)).Run(ContextFixtureValue), category: "Input");
    }

    [Fact]
    public void InvalidPointInputsRejectBeforeProbeEvaluation() {
        Spec.FailCategory(result: Spatial.NearestPoints(points: [Point3d.Unset], needles: [Point3d.Origin], probe: Probe.Nearest(count: 1)).Run(ContextFixtureValue), category: "Input");
        Spec.FailCategory(result: Spatial.NearestPoints(points: [Point3d.Origin], needles: [Point3d.Unset], probe: Probe.Within(distance: 1.0)).Run(ContextFixtureValue), category: "Input");
    }

    private static readonly Context ContextFixtureValue =
        Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: Rhino.UnitSystem.Millimeters).ToFin(), label: "spatial context");
}

public sealed class ValidatePointsGuardLaws {
    [Fact]
    public void AllValidPointsProjectSameArityAndSequenceVerbatim() =>
        Spec.ForAll(SpatialGens.ValidPoints, static points =>
            Spec.Succ(result: Tree.ValidatePoints(points: points), then: validated => {
                Assert.Equal(expected: points.Length, actual: validated.Length);
                Assert.Equal(expected: points, actual: validated);
            }));
    [Fact]
    public void FirstInvalidPointShortCircuitsWithInputCategory() {
        Spec.Cases(items: [.. SpatialGens.InvalidPoints.Select(static (Point3d p, int i) => (Index: i, Point: p))], key: static c => c.Index, law: static c =>
            Spec.FailCategory(result: Tree.ValidatePoints(points: [c.Point]), category: "Input"));
        Spec.FailCategory(result: Tree.ValidatePoints(points: [Point3d.Origin, Point3d.Unset, new(x: 1.0, y: 1.0, z: 1.0)]), category: "Input");
    }
    [Fact]
    public void EmptySpanProjectsEmptyArray() =>
        Spec.Succ(result: Tree.ValidatePoints(points: []), then: static validated => Assert.Empty(collection: validated));
}

public sealed class PointPairsAlgebraLaws {
    [Fact]
    public void ExpandsNeedleSourcePairsMatchingIndependentSortedOracle() =>
        Spec.ForAll(SpatialGens.NeighborRows, static rows =>
            Spec.Succ(result: Tree.PointPairs(values: rows), then: pairs =>
                Assert.Equal(expected: SpatialGens.ExpandSorted(rows: rows), actual: pairs)));
    [Fact]
    public void OutputIsSortedByAThenBAndConservesPairCount() =>
        Spec.ForAll(SpatialGens.NeighborRows, static rows =>
            Spec.Succ(result: Tree.PointPairs(values: rows), then: pairs => {
                Assert.Equal(expected: rows.Sum(static ids => ids.Length), actual: pairs.Count);
                _ = toSeq(Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: pairs.Count - 1))).Iter(i =>
                    Spec.Holds(condition: (pairs[index: i - 1].A, pairs[index: i - 1].B).CompareTo((pairs[index: i].A, pairs[index: i].B)) <= 0,
                        label: $"PointPairs unsorted at {i}: {pairs[index: i - 1]} > {pairs[index: i]}"));
            }));
    [Fact]
    public void NullSourceCollapsesToResultFault() =>
        Spec.FailCategory(result: Tree.PointPairs(values: null!), category: "Result");
}

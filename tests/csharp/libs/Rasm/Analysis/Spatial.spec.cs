using Rasm.Analysis;
using Rasm.Domain;
using Rasm.TestKit;
using Rhino.Geometry;

namespace Rasm.Tests.Analysis;

// --- [MODELS] ----------------------------------------------------------------------------
// BRIDGE-DEFERRED: native SpatialIndex.*; static owns SpatialProbe catalog, ValidatePoints, and PointPairs vs an independent sorted oracle.
internal static class SpatialGens {
    public static readonly Op Key = Op.Of(name: "spatial-test");
    public static readonly Point3d[] InvalidPoints =
        [Point3d.Unset, new(x: double.NaN, y: 0.0, z: 0.0),
         new(x: 0.0, y: double.PositiveInfinity, z: 0.0), new(x: 0.0, y: 0.0, z: double.NegativeInfinity)];
    public static readonly Gen<Point3d[]> ValidPoints = Gens.NonEmptyArray(element: Gens.Point, max: 32);
    public static readonly Gen<int[][]> NeighborRows =
        Gen.Int[0, 16].Array[0, 12].Select(static (int[] counts) =>
            counts.Select(static (int n, int needle) => Enumerable.Range(start: needle, count: n % 6).ToArray()).ToArray());
    public static SpatialIndex IndexOf(params Point3d[] points) =>
        SpatialIndex.Points(points: points).Match(Succ: static tree => tree, Fail: error => throw new InvalidOperationException(error.Message));
    public static Validation<Error, Seq<SpatialPair>> PointPairs(Point3d[] points, Point3d[] needles, SpatialProbe probe) =>
        Analyze.Run<SpatialPair>(query: AnalysisQuery.PointPairs(points: points, needles: needles, probe: probe));
    public static Seq<SpatialPair> ExpandSorted(int[][] rows) =>
        toSeq(rows.SelectMany(static (int[] ids, int needle) => ids.Select(source => new SpatialPair(A: needle, B: source)))
            .OrderBy(static c => c.A).ThenBy(static c => c.B));
}

// --- [OPERATIONS] ----------------------------------------------------------------------------
public sealed class SpatialProbeUnionCatalogLaws {
    public static readonly (string Label, SpatialProbe Case)[] Cases =
        [("Nearest", SpatialProbe.Nearest(count: 5)), ("Within", SpatialProbe.Within(distance: 2.5))];
    [Fact]
    public void FactoriesProjectDistinctCasesAndTransportPayloadVerbatim() {
        Assert.Equal(expected: 2, actual: Cases.Select(static c => c.Case.GetType()).Distinct().Count());
        Assert.Equal(expected: 7, actual: Assert.IsType<SpatialProbe.NearestCase>(@object: SpatialProbe.Nearest(count: 7)).Count);
        Spec.Equal(left: Assert.IsType<SpatialProbe.WithinCase>(@object: SpatialProbe.Within(distance: -3.25)).Distance,
            right: -3.25, tolerance: 0.0, what: "within distance transport");
    }
    [Fact]
    public void PayloadChannelsAreIndependentAcrossFactories() =>
        Spec.ForAll(Gen.Int[-50, 50].Select(Gens.Finite, static (int count, double distance) => (Count: count, Distance: distance)), static pair => {
            Assert.Equal(expected: pair.Count, actual: Assert.IsType<SpatialProbe.NearestCase>(@object: SpatialProbe.Nearest(count: pair.Count)).Count);
            Spec.Equal(left: Assert.IsType<SpatialProbe.WithinCase>(@object: SpatialProbe.Within(distance: pair.Distance)).Distance,
                right: pair.Distance, tolerance: 0.0, what: "within distance");
        });
}

public sealed class SpatialProbeValidationLaws {
    [Fact]
    public void InvalidProbeParametersRejectBeforeNativeNeighborSearch() {
        Spec.Invalid(result: SpatialGens.PointPairs(points: [Point3d.Origin], needles: [Point3d.Origin], probe: SpatialProbe.Nearest(count: 0)),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(result: SpatialGens.PointPairs(points: [Point3d.Origin], needles: [Point3d.Origin], probe: SpatialProbe.Within(distance: 0.0)),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(result: SpatialGens.PointPairs(points: [Point3d.Origin], needles: [Point3d.Origin], probe: SpatialProbe.Within(distance: double.NaN)),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
    }

    [Fact]
    public void InvalidPointInputsRejectBeforeProbeEvaluation() {
        Spec.Invalid(result: SpatialGens.PointPairs(points: [Point3d.Unset], needles: [Point3d.Origin], probe: SpatialProbe.Nearest(count: 1)),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
        Spec.Invalid(result: SpatialGens.PointPairs(points: [Point3d.Origin], needles: [Point3d.Unset], probe: SpatialProbe.Within(distance: 1.0)),
            then: static error => Assert.Equal(expected: "Input", actual: error.Category()));
    }
}

public sealed class AnalysisQuerySpatialExecutionLaws {
    [Fact]
    public void SpatialIndexSearchByBoxAndSphereReturnsSortedHitIdsThroughServiceQuery() {
        using SpatialIndex tree = SpatialGens.IndexOf(Point3d.Origin, new Point3d(x: 2.0, y: 0.0, z: 0.0), new Point3d(x: 4.0, y: 0.0, z: 0.0));
        Spec.Valid(
            result: Analyze.Run<SpatialHit>(query: AnalysisQuery.Search(index: tree, box: new BoundingBox(min: new Point3d(x: -0.5, y: -0.5, z: -0.5), max: new Point3d(x: 2.5, y: 0.5, z: 0.5)))),
            then: static hits => Assert.Equal(expected: Seq(new SpatialHit(Id: 0), new SpatialHit(Id: 1)), actual: hits));
        Spec.Valid(
            result: Analyze.Run<SpatialHit>(query: AnalysisQuery.Search(index: tree, sphere: new Sphere(center: new Point3d(x: 4.0, y: 0.0, z: 0.0), radius: 0.25))),
            then: static hits => Assert.Equal(expected: Seq(new SpatialHit(Id: 2)), actual: hits));
    }

    [Fact]
    public void SpatialIndexOverlapAndNearestPointPairsReturnSortedPairsThroughServiceQuery() {
        using SpatialIndex left = SpatialGens.IndexOf(Point3d.Origin, new Point3d(x: 2.0, y: 0.0, z: 0.0));
        using SpatialIndex right = SpatialGens.IndexOf(new Point3d(x: 0.1, y: 0.0, z: 0.0), new Point3d(x: 2.2, y: 0.0, z: 0.0));
        Spec.Valid(
            result: Analyze.Run<SpatialPair>(query: AnalysisQuery.Overlaps(left: left, right: right, tolerance: 0.25)),
            then: static pairs => Assert.Equal(expected: Seq(new SpatialPair(A: 0, B: 0), new SpatialPair(A: 1, B: 1)), actual: pairs));
        Spec.Valid(
            result: SpatialGens.PointPairs(
                points: [Point3d.Origin, new Point3d(x: 2.0, y: 0.0, z: 0.0)],
                needles: [new Point3d(x: 0.2, y: 0.0, z: 0.0), new Point3d(x: 1.8, y: 0.0, z: 0.0)],
                probe: SpatialProbe.Nearest(count: 1)),
            then: static pairs => Assert.Equal(expected: Seq(new SpatialPair(A: 0, B: 0), new SpatialPair(A: 1, B: 1)), actual: pairs));
    }

    [Fact]
    public void CancelledScopeStopsPointPairsBeforeNativeNeighborSearch() {
        using CancellationTokenSource source = new();
        source.Cancel();
        Spec.Invalid(
            result: Analyze.In(context: ContextFixtureValue).With(cancellation: source.Token).Run(
                operation: Analyze.Query<SpatialPair>(query: AnalysisQuery.PointPairs(
                    points: [Point3d.Origin],
                    needles: [Point3d.Origin],
                    probe: SpatialProbe.Nearest(count: 1))),
                input: Unit.Default),
            then: static error => Assert.Equal(expected: "Cancelled", actual: error.Category()));
    }

    private static readonly Context ContextFixtureValue =
        Spec.SuccValue(Context.Of(absolute: 0.001, relative: 1.0e-8, angle: 0.01, units: Rhino.UnitSystem.Millimeters).ToFin(), label: "spatial query context");
}

public sealed class ValidatePointsGuardLaws {
    [Fact]
    public void AllValidPointsProjectSameArityAndSequenceVerbatim() =>
        Spec.ForAll(SpatialGens.ValidPoints, static points =>
            Spec.Succ(result: SpatialIndex.ValidatePoints(points: points), then: validated => {
                Assert.Equal(expected: points.Length, actual: validated.Length);
                Assert.Equal(expected: points, actual: validated);
            }));
    [Fact]
    public void FirstInvalidPointShortCircuitsWithInputCategory() {
        Spec.Cases(items: [.. SpatialGens.InvalidPoints.Select(static (Point3d p, int i) => (Index: i, Point: p))], key: static c => c.Index, law: static c =>
            Spec.FailCategory(result: SpatialIndex.ValidatePoints(points: [c.Point]), category: "Input"));
        Spec.FailCategory(result: SpatialIndex.ValidatePoints(points: [Point3d.Origin, Point3d.Unset, new(x: 1.0, y: 1.0, z: 1.0)]), category: "Input");
    }
    [Fact]
    public void EmptySpanProjectsEmptyArray() =>
        Spec.Succ(result: SpatialIndex.ValidatePoints(points: []), then: static validated => Assert.Empty(collection: validated));
}

public sealed class PointPairsAlgebraLaws {
    [Fact]
    public void ExpandsNeedleSourcePairsMatchingIndependentSortedOracle() =>
        Spec.ForAll(SpatialGens.NeighborRows, static rows =>
            Spec.Succ(result: SpatialIndex.PointPairs(values: rows), then: pairs =>
                Assert.Equal(expected: SpatialGens.ExpandSorted(rows: rows), actual: pairs)));
    [Fact]
    public void OutputIsSortedByAThenBAndConservesPairCount() =>
        Spec.ForAll(SpatialGens.NeighborRows, static rows =>
            Spec.Succ(result: SpatialIndex.PointPairs(values: rows), then: pairs => {
                Assert.Equal(expected: rows.Sum(static ids => ids.Length), actual: pairs.Count);
                _ = toSeq(Enumerable.Range(start: 1, count: Math.Max(val1: 0, val2: pairs.Count - 1))).Iter(i =>
                    Spec.Holds(condition: (pairs[index: i - 1].A, pairs[index: i - 1].B).CompareTo((pairs[index: i].A, pairs[index: i].B)) <= 0,
                        label: string.Create(System.Globalization.CultureInfo.InvariantCulture, $"PointPairs unsorted at {i}: {pairs[index: i - 1]} > {pairs[index: i]}")));
            }));
    [Fact]
    public void NullSourceCollapsesToResultFault() =>
        Spec.FailCategory(result: SpatialIndex.PointPairs(values: null!), category: "Result");
}

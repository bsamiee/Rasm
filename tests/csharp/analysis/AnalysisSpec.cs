using Analysis;
using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Xunit;

namespace Analysis.Tests;

// --- [EXAMPLES] --------------------------------------------------------------------------------

public sealed class AnalysisSpec {
    [Fact]
    public void ComputesLineMidpoint() {
        Point3d[] points = Run(
            query: Query.Midpoint<Line, Point3d>(),
            input: [
                new Line(
                    from: Point3d.Origin,
                    to: new Point3d(x: 2.0, y: 0.0, z: 0.0)),
            ]);

        Assert.Equal(
            expected: [new Point3d(x: 1.0, y: 0.0, z: 0.0)],
            actual: points);
    }

    [Fact]
    public void ExecutesEmptyInput() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.Midpoint<Line, Point3d>(),
            input: []);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Point3d> output) => output.IsEmpty,
            Fail: static (Error _) => false));
    }

    [Fact]
    public void RejectsContextRequiredEmptyInputWithoutScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.LengthCentroid,
            input: []);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsNullQuery() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run<Line, Point3d>(
            query: null,
            input: [
                new Line(
                    from: Point3d.Origin,
                    to: new Point3d(x: 1.0, y: 0.0, z: 0.0)),
            ]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsInvalidUnitScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(units: UnitSystem.Unset)
            .Run(
                query: Query.Midpoint<Line, Point3d>(),
                input: [
                    new Line(
                        from: Point3d.Origin,
                        to: new Point3d(x: 2.0, y: 0.0, z: 0.0)),
                ]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsInvalidContextScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: null!)
            .Run(
                query: Query.Midpoint<Line, Point3d>(),
                input: [
                    new Line(
                        from: Point3d.Origin,
                        to: new Point3d(x: 2.0, y: 0.0, z: 0.0)),
                ]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void PreservesOrderAcrossManyInputs() {
        Point3d[] points = Run(
            query: Query.Midpoint<Line, Point3d>(),
            input: [
                new Line(
                    from: Point3d.Origin,
                    to: new Point3d(x: 2.0, y: 0.0, z: 0.0)),
                new Line(
                    from: Point3d.Origin,
                    to: new Point3d(x: 0.0, y: 4.0, z: 0.0)),
            ]);

        Assert.Equal(
            expected: [new Point3d(x: 1.0, y: 0.0, z: 0.0), new Point3d(x: 0.0, y: 2.0, z: 0.0)],
            actual: points);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(5)]
    public void PureQueriesPreserveArityLaws(int count) {
        Line[] input = [.. Enumerable.Range(start: 1, count: count)
            .Select(static (int index) => new Line(
                from: Point3d.Origin,
                to: new Point3d(x: index * 2.0, y: 0.0, z: 0.0)))];

        Point3d[] points = Run(
            query: Query.Midpoint<Line, Point3d>(),
            input: input);

        Assert.Equal(expected: count, actual: points.Length);
        Assert.True(condition: points
            .Select(static (Point3d point) => point.X)
            .SequenceEqual(second: Enumerable.Range(start: 1, count: count)
                .Select(static (int index) => (double)index)));
    }

    [Fact]
    public void PreservesOrderAcrossOddArityInputs() {
        Point3d[] points = Run(
            query: Query.Midpoint<Line, Point3d>(),
            input: [
                new Line(from: Point3d.Origin, to: new Point3d(x: 2.0, y: 0.0, z: 0.0)),
                new Line(from: Point3d.Origin, to: new Point3d(x: 0.0, y: 4.0, z: 0.0)),
                new Line(from: Point3d.Origin, to: new Point3d(x: 0.0, y: 0.0, z: 6.0)),
                new Line(from: Point3d.Origin, to: new Point3d(x: 8.0, y: 0.0, z: 0.0)),
                new Line(from: Point3d.Origin, to: new Point3d(x: 0.0, y: 10.0, z: 0.0)),
            ]);

        Assert.Equal(
            expected: [
                new Point3d(x: 1.0, y: 0.0, z: 0.0),
                new Point3d(x: 0.0, y: 2.0, z: 0.0),
                new Point3d(x: 0.0, y: 0.0, z: 3.0),
                new Point3d(x: 4.0, y: 0.0, z: 0.0),
                new Point3d(x: 0.0, y: 5.0, z: 0.0),
            ],
            actual: points);
    }

    [Fact]
    public void ComputesLineMeasures() {
        Line line = new(
            from: Point3d.Origin,
            to: new Point3d(x: 3.0, y: 4.0, z: 0.0));

        double[] lengths = Run(
            query: Query.Length<Line, double>(),
            input: [line]);
        BoundingBox[] bounds = Run(
            query: Query.Bounds<Line, BoundingBox>(),
            input: [line]);

        Assert.Equal(expected: 5.0, actual: lengths[0]);
        Assert.Equal(expected: new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 3.0, y: 4.0, z: 0.0)), actual: bounds[0]);
    }

    [Fact]
    public void ComputesBoxAnalysis() {
        BoundingBox box = new(
            min: Point3d.Origin,
            max: new Point3d(x: 2.0, y: 4.0, z: 6.0));

        Point3d[] center = Run(
            query: Query.BoundsCenter,
            input: [box]);
        Point3d[] corners = Run(
            query: Query.BoundsCorners,
            input: [box]);
        Line[] edges = Run(
            query: Query.BoxEdges,
            input: [box]);
        double[] area = Run(
            query: Query.BoxArea,
            input: [box]);
        double[] volume = Run(
            query: Query.BoxVolume,
            input: [box]);

        Assert.Equal(expected: new Point3d(x: 1.0, y: 2.0, z: 3.0), actual: center[0]);
        Assert.Equal(expected: 8, actual: corners.Length);
        Assert.Equal(expected: 12, actual: edges.Length);
        Assert.Equal(expected: 88.0, actual: area[0]);
        Assert.Equal(expected: 48.0, actual: volume[0]);
    }

    [Fact]
    public void RejectsUnsupportedQueryBeforeInputExecution() {
        Validation<Error, Seq<Plane>> result = Analyze.Run(
            query: Query.Bounds<Line, Plane>(),
            input: [
                new Line(from: Point3d.Origin, to: new Point3d(x: 1.0, y: 0.0, z: 0.0)),
                new Line(from: Point3d.Origin, to: new Point3d(x: 0.0, y: 1.0, z: 0.0)),
            ]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Plane> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void RejectsUnsupportedBoundsInputBeforeExecution() {
        Validation<Error, Seq<BoundingBox>> result = Analyze.Run(
            query: Query.Bounds<int, BoundingBox>(),
            input: [1, 2, 3]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<BoundingBox> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void RejectsMissingContextOnceBeforeInputExecution() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.LengthCentroid,
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Point3d> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void AccumulatesInvalidInputFailures() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.Midpoint<Line, Point3d>(),
            input: [
                Line.Unset,
                Line.Unset,
            ]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Point3d> _) => false,
            Fail: static (Error error) => error.Count == 2));
    }

    [Fact]
    public void RejectsInvalidValueInput() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.Midpoint<Line, Point3d>(),
            input: [Line.Unset]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    private static TOut[] Run<TGeometry, TOut>(
        Query<TGeometry, TOut> query,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Analyze.Run(
                query: query,
                input: input)
            .ToFin()
            .Match(
                Succ: static (Seq<TOut> output) => output.ToArray(),
                Fail: static (Error error) => throw new Xunit.Sdk.XunitException(error.Message));
}

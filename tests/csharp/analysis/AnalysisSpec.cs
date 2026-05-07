using System.Reflection;
using Analysis;
using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Xunit;

namespace Analysis.Tests;

// --- [EXAMPLES] --------------------------------------------------------------------------------

public sealed class AnalysisSpec {
    [Fact]
    public void ComputesLineMidpoint() {
        Point3d[] points = Run(
            query: Query.Locate<Line, Point3d>(aspect: Location.Midpoint),
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
            query: Query.Locate<Line, Point3d>(aspect: Location.Midpoint),
            input: []);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Point3d> output) => output.IsEmpty,
            Fail: static (Error _) => false));
    }

    [Fact]
    public void RejectsContextRequiredEmptyInputWithoutScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.Measure<Curve, Point3d>(aspect: Measure.Centroid(kind: MassKind.Length)),
            input: []);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsInvalidUnitScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(units: UnitSystem.Unset)
            .Run(
                query: Query.Locate<Line, Point3d>(aspect: Location.Midpoint),
                input: [ValidLine()]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsMissingDocumentScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.From(doc: null)
            .Run(
                query: Query.Locate<Line, Point3d>(aspect: Location.Midpoint),
                input: [ValidLine()]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsNullQuery() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run<Line, Point3d>(
            query: null,
            input: [ValidLine()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Point3d> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void RejectsScopedNullQuery() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: null!)
            .Run<Line, Point3d>(
                query: null,
                input: [ValidLine()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Point3d> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void RejectsNullGeometryInsidePureRail() {
        Validation<Error, Seq<BoundingBox>> result = Analyze.Run(
            query: Query.Bounds<GeometryBase, BoundingBox>(aspect: Bounds.Box),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<BoundingBox> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void KeepsParameterlessFactoriesAsProperties() {
        Assert.NotNull(@object: Query.Edges);
        Assert.NotNull(@object: Query.IsManifold);
        Assert.NotNull(@object: Query.NakedPointStatus);
        Assert.NotNull(@object: Query.SelfIntersections);
    }

    [Fact]
    public void RejectsInvalidContextScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: null!)
            .Run(
                query: Query.Locate<Line, Point3d>(aspect: Location.Midpoint),
                input: [
                    new Line(
                        from: Point3d.Origin,
                        to: new Point3d(x: 2.0, y: 0.0, z: 0.0)),
                ]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void KeepsScopeConstructionBehindContextEntry() =>
        Assert.DoesNotContain(
            collection: typeof(Analyze.Scope).GetConstructors(bindingAttr: BindingFlags.Public | BindingFlags.Instance),
            filter: static (ConstructorInfo constructor) => constructor.IsPublic);

    [Fact]
    public void PreservesOrderAcrossManyInputs() {
        Point3d[] points = Run(
            query: Query.Locate<Line, Point3d>(aspect: Location.Midpoint),
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
            query: Query.Locate<Line, Point3d>(aspect: Location.Midpoint),
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
            query: Query.Locate<Line, Point3d>(aspect: Location.Midpoint),
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
            query: Query.Measure<Line, double>(aspect: Measure.Length),
            input: [line]);
        BoundingBox[] bounds = Run(
            query: Query.Bounds<Line, BoundingBox>(aspect: Bounds.Box),
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
            query: Query.Bounds<BoundingBox, Point3d>(aspect: Bounds.Center),
            input: [box]);
        Point3d[] corners = Run(
            query: Query.Bounds<BoundingBox, Point3d>(aspect: Bounds.Corners),
            input: [box]);
        Line[] edges = Run(
            query: Query.Bounds<BoundingBox, Line>(aspect: Bounds.Edges),
            input: [box]);
        double[] area = Run(
            query: Query.Bounds<BoundingBox, double>(aspect: Bounds.Area),
            input: [box]);
        double[] volume = Run(
            query: Query.Bounds<BoundingBox, double>(aspect: Bounds.Volume),
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
            query: Query.Bounds<Line, Plane>(aspect: Bounds.Box),
            input: [
                new Line(from: Point3d.Origin, to: new Point3d(x: 1.0, y: 0.0, z: 0.0)),
                new Line(from: Point3d.Origin, to: new Point3d(x: 0.0, y: 1.0, z: 0.0)),
            ]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Plane> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "Bounds", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedBoundsInputBeforeExecution() {
        Validation<Error, Seq<BoundingBox>> result = Analyze.Run(
            query: Query.Bounds<int, BoundingBox>(aspect: Bounds.Box),
            input: [1, 2, 3]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<BoundingBox> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void RejectsUnsupportedMeasureOutputBeforeInputExecution() {
        Validation<Error, Seq<Plane>> result = Analyze.Run(
            query: Query.Measure<Mesh, Plane>(aspect: Measure.Area),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Plane> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "Measure", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNoneMassKindOnceBeforeInputExecution() {
        Validation<Error, Seq<double>> result = Analyze.Run(
            query: Query.Measure<Curve, double>(aspect: Measure.Error(kind: MassKind.None)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<double> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "invalid Rhino input", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedPrimitiveOutputBeforeInputExecution() {
        Validation<Error, Seq<Sphere>> result = Analyze.Run(
            query: Query.Primitive<Curve, Sphere>(),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Sphere> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "Primitive", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsMissingContextOnceBeforeInputExecution() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.Measure<Curve, Point3d>(aspect: Measure.Centroid(kind: MassKind.Length)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Point3d> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void AccumulatesInvalidInputFailures() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.Locate<Line, Point3d>(aspect: Location.Midpoint),
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
            query: Query.Locate<Line, Point3d>(aspect: Location.Midpoint),
            input: [Line.Unset]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void KeepsIntersectionFactoriesOnTypedRails() {
        Assert.NotNull(@object: Query.Intersect<Curve, Curve, IntersectionEvent>());
        Assert.NotNull(@object: Query.Intersect<Curve, Curve, IntersectionKind>());
        Assert.NotNull(@object: Query.Intersect<LineCurve, LineCurve, IntersectionEvent>());

        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.Intersect<Line, Line, Point3d>(),
            input: [(ValidLine(), ValidLine())]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsInvalidCurvatureProfileCountBeforeInputExecution() {
        Validation<Error, Seq<Vector3d>> result = Analyze.Run(
            query: Query.Locate<Curve, Vector3d>(aspect: Location.CurvatureProfile(count: 0)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Vector3d> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void RejectsUnsupportedCurvatureProfileSummaryBeforeInputExecution() {
        Validation<Error, Seq<CurvatureProfile>> result = Analyze.Run(
            query: Query.Locate<Line, CurvatureProfile>(aspect: Location.CurvatureProfile(count: 3)),
            input: [ValidLine()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<CurvatureProfile> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedIntersectionClassificationWithOperationVocabulary() {
        Validation<Error, Seq<IntersectionKind>> result = Analyze.Run(
            query: Query.Intersect<Line, Line, IntersectionKind>(),
            input: [(ValidLine(), ValidLine())]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<IntersectionKind> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "Intersect", comparisonType: StringComparison.Ordinal)));
    }

    private static Line ValidLine() =>
        new(
            from: Point3d.Origin,
            to: new Point3d(x: 2.0, y: 0.0, z: 0.0));

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

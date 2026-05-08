using System.Reflection;
using System.Runtime.CompilerServices;
using Analysis;
using Core.Domain;
using Core.Runtime;
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
            query: Query.Locate<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Query.Locate<Line, Point3d>(aspect: new Location.Midpoint()),
            input: []);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Point3d> output) => output.IsEmpty,
            Fail: static (Error _) => false));
    }

    [Fact]
    public void RejectsContextRequiredEmptyInputWithoutScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.Measure<Curve, Point3d>(aspect: new Measure.Centroid(Mass: MassKind.Length)),
            input: []);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsInvalidUnitScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(units: UnitSystem.Unset)
            .Run(
                query: Query.Locate<Line, Point3d>(aspect: new Location.Midpoint()),
                input: [ValidLine()]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsMissingDocumentScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.From(doc: null)
            .Run(
                query: Query.Locate<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Query.Bounds<GeometryBase, BoundingBox>(aspect: new Bounds.Box()),
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
        Assert.NotNull(@object: Query.MeshCheckCount(count: MeshCheckCount.NakedEdges));
        Assert.NotNull(@object: Query.MeshFaceMetric(metric: MeshFaceMetric.AspectRatio));
        Assert.NotNull(@object: Query.Topology<Mesh, Polyline>(aspect: Topology.Boundary));
        Assert.NotNull(@object: Query.Topology<GeometryBase, Point3d>(aspect: Topology.EdgeMidpoints));
        Assert.NotNull(@object: Query.Topology<Mesh, ComponentIndex>(aspect: Topology.Adjacency));
        Assert.NotNull(@object: Query.Topology<Mesh, bool>(aspect: Topology.NonManifold));
        Assert.NotNull(@object: Query.Measure<GeometryBase, Point3d>(aspect: new Measure.SpatialMidpoint()));
        Assert.NotNull(@object: Query.Locate<Curve, double>(aspect: new Location.CurvatureProfile(Count: 3, Scalar: CurvatureScalar.Magnitude)));
        Assert.NotNull(@object: Query.Locate<Surface, double>(aspect: new Location.CurvatureProfile(Count: 3, Scalar: CurvatureScalar.Gaussian)));
        Assert.NotNull(@object: Query.Locate<Surface, double>(aspect: new Location.CurvatureProfile(Count: 3, Scalar: CurvatureScalar.Mean)));
        Assert.NotNull(@object: Query.Conformance<Curve, Line, double>(aspect: Conformance.Distance(count: 3)));
        Assert.NotNull(@object: Query.Conformance<Surface, Plane, bool>(aspect: Conformance.WithinTolerance(count: 2)));
        Assert.NotNull(@object: Query.Conformance<Curve, Line, ResidualProfile>(aspect: Conformance.Profile(count: 3)));
        Assert.NotNull(@object: Query.Conformance<Curve, Circle, double>(aspect: Conformance.Distance(count: 3)));
        Assert.NotNull(@object: Query.Conformance<Curve, Arc, bool>(aspect: Conformance.WithinTolerance(count: 3)));
        Assert.NotNull(@object: Query.Conformance<Surface, Sphere, ResidualSample>(aspect: Conformance.Maximum(count: 2)));
        Assert.NotNull(@object: Query.Deviation<Curve, Curve, CurveDeviation>(aspect: Deviation.Curve));
        Assert.NotNull(@object: Query.EdgeMidpoints<GeometryBase, Point3d>());
        Assert.NotNull(@object: Query.Vertices<GeometryBase, Point3d>());
    }

    [Fact]
    public void RejectsInvalidContextScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: null!)
            .Run(
                query: Query.Locate<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Query.Locate<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Query.Locate<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Query.Locate<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Query.Measure<Line, double>(aspect: new Measure.Length()),
            input: [line]);
        BoundingBox[] bounds = Run(
            query: Query.Bounds<Line, BoundingBox>(aspect: new Bounds.Box()),
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
            query: Query.Bounds<BoundingBox, Point3d>(aspect: new Bounds.Center()),
            input: [box]);
        Point3d[] corners = Run(
            query: Query.Bounds<BoundingBox, Point3d>(aspect: new Bounds.Corners()),
            input: [box]);
        Line[] edges = Run(
            query: Query.Bounds<BoundingBox, Line>(aspect: new Bounds.Edges()),
            input: [box]);
        double[] area = Run(
            query: Query.Bounds<BoundingBox, double>(aspect: new Bounds.Area()),
            input: [box]);
        double[] volume = Run(
            query: Query.Bounds<BoundingBox, double>(aspect: new Bounds.Volume()),
            input: [box]);

        Assert.Equal(expected: new Point3d(x: 1.0, y: 2.0, z: 3.0), actual: center[0]);
        Assert.Equal(expected: 8, actual: corners.Length);
        Assert.Equal(expected: 12, actual: edges.Length);
        Assert.Equal(expected: 88.0, actual: area[0]);
        Assert.Equal(expected: 48.0, actual: volume[0]);
    }

    [Fact]
    public void ComputesPureObjectPointExtractionWithoutContext() {
        object line = new Line(
            from: Point3d.Origin,
            to: new Point3d(x: 2.0, y: 0.0, z: 0.0));
        object polyline = new Polyline([
            Point3d.Origin,
            new Point3d(x: 2.0, y: 0.0, z: 0.0),
            new Point3d(x: 2.0, y: 2.0, z: 0.0),
        ]);
        object box = new BoundingBox(
            min: Point3d.Origin,
            max: new Point3d(x: 2.0, y: 4.0, z: 6.0));

        Point3d[] lineEdgeMidpoint = Run(
            query: Query.EdgeMidpoints<object, Point3d>(),
            input: [line]);
        Point3d[] polylineEdgeMidpoints = Run(
            query: Query.EdgeMidpoints<object, Point3d>(),
            input: [polyline]);
        Point3d[] boxSpatialMidpoint = Run(
            query: Query.Measure<object, Point3d>(aspect: new Measure.SpatialMidpoint()),
            input: [box]);

        Assert.Equal(expected: new Point3d(x: 1.0, y: 0.0, z: 0.0), actual: lineEdgeMidpoint[0]);
        Assert.Equal(
            expected: [
                new Point3d(x: 1.0, y: 0.0, z: 0.0),
                new Point3d(x: 2.0, y: 1.0, z: 0.0),
            ],
            actual: polylineEdgeMidpoints);
        Assert.Equal(expected: new Point3d(x: 1.0, y: 2.0, z: 3.0), actual: boxSpatialMidpoint[0]);
    }

    [Fact]
    public void ComputesBoundsCenterPolymorphicallyOverObject() {
        object box = new BoundingBox(
            min: Point3d.Origin,
            max: new Point3d(x: 2.0, y: 4.0, z: 6.0));

        Point3d[] center = Run(
            query: Query.Bounds<object, Point3d>(aspect: new Bounds.Center()),
            input: [box]);

        _ = Assert.Single(collection: center);
        Assert.Equal(expected: new Point3d(x: 1.0, y: 2.0, z: 3.0), actual: center[0]);
    }

    [Fact]
    public void ComputesBoundsCornersPolymorphicallyOverObject() {
        BoundingBox boundingBox = new(
            min: Point3d.Origin,
            max: new Point3d(x: 2.0, y: 4.0, z: 6.0));

        Point3d[] corners = Run(
            query: Query.Bounds<object, Point3d>(aspect: new Bounds.Corners()),
            input: [(object)boundingBox]);

        Assert.Equal(expected: 8, actual: corners.Length);
        Assert.Equal(
            expected: boundingBox.GetCorners(),
            actual: corners);
    }

    [Fact]
    public void ComputesBoundsCenterAndCornersOverLine() {
        Line line = new(
            from: Point3d.Origin,
            to: new Point3d(x: 4.0, y: 6.0, z: 0.0));

        Point3d[] center = Run(
            query: Query.Bounds<Line, Point3d>(aspect: new Bounds.Center()),
            input: [line]);
        Point3d[] corners = Run(
            query: Query.Bounds<Line, Point3d>(aspect: new Bounds.Corners()),
            input: [line]);

        Assert.Equal(expected: line.BoundingBox.Center, actual: center[0]);
        Assert.Equal(expected: 8, actual: corners.Length);
        Assert.Equal(expected: line.BoundingBox.GetCorners(), actual: corners);
    }

    [Fact]
    public void ComputesBoundsCenterAndCornersOverPolyline() {
        Polyline polyline = new([
            Point3d.Origin,
            new Point3d(x: 2.0, y: 0.0, z: 0.0),
            new Point3d(x: 2.0, y: 3.0, z: 4.0),
        ]);

        Point3d[] center = Run(
            query: Query.Bounds<Polyline, Point3d>(aspect: new Bounds.Center()),
            input: [polyline]);
        Point3d[] corners = Run(
            query: Query.Bounds<Polyline, Point3d>(aspect: new Bounds.Corners()),
            input: [polyline]);

        Assert.Equal(expected: polyline.BoundingBox.Center, actual: center[0]);
        Assert.Equal(expected: 8, actual: corners.Length);
        Assert.Equal(expected: polyline.BoundingBox.GetCorners(), actual: corners);
    }

    [Fact]
    public void RejectsKnownUnitsOverloadWithInvalidArguments() {
        Validation<Error, Seq<BoundingBox>> nonPositiveAbsolute = Analyze.In(
                absolute: 0.0,
                relative: 0.0,
                angle: Math.PI / 180.0,
                units: UnitSystem.Unset)
            .Run(
                query: Query.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.Box()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> outOfRangeRelative = Analyze.In(
                absolute: 0.01,
                relative: 1.0,
                angle: Math.PI / 180.0,
                units: UnitSystem.Unset)
            .Run(
                query: Query.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.Box()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> nonPositiveAngle = Analyze.In(
                absolute: 0.01,
                relative: 0.0,
                angle: 0.0,
                units: UnitSystem.Unset)
            .Run(
                query: Query.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.Box()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> overFullTurnAngle = Analyze.In(
                absolute: 0.01,
                relative: 0.0,
                angle: (2.0 * Math.PI) + 1.0,
                units: UnitSystem.Unset)
            .Run(
                query: Query.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.Box()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> unsetUnits = Analyze.In(
                absolute: 0.01,
                relative: 0.0,
                angle: Math.PI / 180.0,
                units: UnitSystem.Unset)
            .Run(
                query: Query.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.Box()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);

        Assert.True(condition: nonPositiveAbsolute.ToFin().Match(
            Succ: static (Seq<BoundingBox> _) => false,
            Fail: static (Error error) => error.Message.Contains(value: "AbsoluteTolerance", comparisonType: StringComparison.Ordinal)));
        Assert.True(condition: outOfRangeRelative.ToFin().Match(
            Succ: static (Seq<BoundingBox> _) => false,
            Fail: static (Error error) => error.Message.Contains(value: "RelativeTolerance", comparisonType: StringComparison.Ordinal)));
        Assert.True(condition: nonPositiveAngle.ToFin().Match(
            Succ: static (Seq<BoundingBox> _) => false,
            Fail: static (Error error) => error.Message.Contains(value: "AngleTolerance", comparisonType: StringComparison.Ordinal)));
        Assert.True(condition: overFullTurnAngle.ToFin().Match(
            Succ: static (Seq<BoundingBox> _) => false,
            Fail: static (Error error) => error.Message.Contains(value: "AngleTolerance", comparisonType: StringComparison.Ordinal)));
        Assert.True(condition: unsetUnits.ToFin().IsFail);
    }

    [Fact]
    public void RejectsUnsupportedQueryBeforeInputExecution() {
        Validation<Error, Seq<Plane>> result = Analyze.Run(
            query: Query.Bounds<Line, Plane>(aspect: new Bounds.Box()),
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
            query: Query.Bounds<int, BoundingBox>(aspect: new Bounds.Box()),
            input: [1, 2, 3]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<BoundingBox> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void RejectsUnsupportedMeasureOutputBeforeInputExecution() {
        Validation<Error, Seq<Plane>> result = Analyze.Run(
            query: Query.Measure<Mesh, Plane>(aspect: new Measure.Area()),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Plane> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "Measure", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNoneMassKindOnceBeforeInputExecution() {
        Validation<Error, Seq<double>> result = Analyze.Run(
            query: Query.Measure<Curve, double>(aspect: new Measure.Error(Mass: MassKind.None)),
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
    public void RejectsUnsupportedTopologyBeforeInputExecution() {
        Validation<Error, Seq<Curve>> result = Analyze.Run(
            query: Query.Topology<Curve, Curve>(aspect: Topology.Boundary),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Curve> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "Topology", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNullGeometryInsideTopologyRail() {
        Validation<Error, Seq<Polyline>> result = Analyze.Run(
            query: Query.Topology<Mesh, Polyline>(aspect: Topology.Boundary),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Polyline> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void RejectsUnsupportedTopologyOutputWithOperationVocabulary() {
        Validation<Error, Seq<Curve>> result = Analyze.Run(
            query: Query.Topology<Mesh, Curve>(aspect: Topology.Boundary),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Curve> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "Topology", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsInvalidMeshCheckCountBeforeInputExecution() {
        Validation<Error, Seq<int>> result = Analyze.Run(
            query: Query.MeshCheckCount(count: MeshCheckCount.None),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<int> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "MeshCheckCount", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnknownMeshCheckCountBeforeInputExecution() {
        Validation<Error, Seq<int>> result = Analyze.Run(
            query: Query.MeshCheckCount(count: (MeshCheckCount)999),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<int> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "MeshCheckCount", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNullGeometryInsideMeshCheckCountRail() {
        Validation<Error, Seq<int>> result = Analyze.Run(
            query: Query.MeshCheckCount(count: MeshCheckCount.NakedEdges),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<int> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void RejectsMissingContextOnceBeforeInputExecution() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.Measure<Curve, Point3d>(aspect: new Measure.Centroid(Mass: MassKind.Length)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Point3d> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void AccumulatesInvalidInputFailures() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.Locate<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Query.Locate<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Query.Locate<Curve, Vector3d>(aspect: new Location.CurvatureProfile(Count: 0, Scalar: CurvatureScalar.None)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<Vector3d> _) => false,
            Fail: static (Error error) => error.Count == 1));
    }

    [Fact]
    public void RejectsInvalidExplicitCurvatureScalarCountBeforeInputExecution() {
        Validation<Error, Seq<double>> result = Analyze.Run(
            query: Query.Locate<Curve, double>(aspect: new Location.CurvatureProfile(Count: 0, Scalar: CurvatureScalar.Magnitude)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<double> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "invalid Rhino input", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedCurvatureProfileSummaryBeforeInputExecution() {
        Validation<Error, Seq<CurvatureProfile>> result = Analyze.Run(
            query: Query.Locate<Line, CurvatureProfile>(aspect: new Location.CurvatureProfile(Count: 3, Scalar: CurvatureScalar.None)),
            input: [ValidLine()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<CurvatureProfile> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsDefaultCurvatureScalarOutputBeforeInputExecution() {
        Validation<Error, Seq<double>> result = Analyze.Run(
            query: Query.Locate<Curve, double>(aspect: new Location.CurvatureProfile(Count: 3, Scalar: CurvatureScalar.None)),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<double> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedExplicitCurvatureScalarBeforeInputExecution() {
        Validation<Error, Seq<double>> curveMean = Analyze.Run(
            query: Query.Locate<Curve, double>(aspect: new Location.CurvatureProfile(Count: 3, Scalar: CurvatureScalar.Mean)),
            input: [null!]);
        Validation<Error, Seq<double>> curveGaussian = Analyze.Run(
            query: Query.Locate<Curve, double>(aspect: new Location.CurvatureProfile(Count: 3, Scalar: CurvatureScalar.Gaussian)),
            input: [null!]);
        Validation<Error, Seq<double>> surfaceMagnitude = Analyze.Run(
            query: Query.Locate<Surface, double>(aspect: new Location.CurvatureProfile(Count: 3, Scalar: CurvatureScalar.Magnitude)),
            input: [null!]);

        Assert.True(condition: (curveMean, curveGaussian, surfaceMagnitude).Apply(static (Seq<double> _, Seq<double> _, Seq<double> _) => false).As().ToFin().Match(
            Succ: static (bool valid) => valid,
            Fail: static (Error error) => error.Count == 3 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsInvalidConformanceResidualBeforeInputExecution() {
        Validation<Error, Seq<double>> invalidCount = Analyze.Run(
            query: Query.Conformance<Curve, Line, double>(aspect: Conformance.Distance(count: 0)),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);
        Validation<Error, Seq<double>> invalidResidual = Analyze.Run(
            query: Query.Conformance<Curve, Line, double>(aspect: default),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);
        Validation<Error, Seq<ResidualProfile>> invalidProfileCount = Analyze.Run(
            query: Query.Conformance<Curve, Line, ResidualProfile>(aspect: Conformance.Profile(count: 0)),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);

        Assert.True(condition: (invalidCount, invalidResidual, invalidProfileCount).Apply(static (Seq<double> _, Seq<double> _, Seq<ResidualProfile> _) => false).As().ToFin().Match(
            Succ: static (bool valid) => valid,
            Fail: static (Error error) => error.Count == 3 && error.Message.Contains(value: "Conformance", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsInvalidMaximumConformanceBeforeInputExecution() {
        Validation<Error, Seq<ResidualSample>> invalidMaximumCount = Analyze.Run(
            query: Query.Conformance<Curve, Line, ResidualSample>(aspect: Conformance.Maximum(count: 0)),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);
        Validation<Error, Seq<ResidualSample>> unsupportedDeferred = Analyze.Run(
            query: Query.Conformance<Surface, Cylinder, ResidualSample>(aspect: Conformance.Maximum(count: 2)),
            input: [(null!, Cylinder.Unset)]);

        Assert.True(condition: (invalidMaximumCount, unsupportedDeferred).Apply(static (Seq<ResidualSample> _, Seq<ResidualSample> _) => false).As().ToFin().Match(
            Succ: static (bool valid) => valid,
            Fail: static (Error error) => error.Count == 2 && error.Message.Contains(value: "Conformance", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedConformanceOutputWithOperationVocabulary() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Query.Conformance<Curve, Line, Point3d>(aspect: Conformance.Distance(count: 3)),
            input: [(null!, Line.Unset)]);
        Validation<Error, Seq<Point3d>> profile = Analyze.Run(
            query: Query.Conformance<Curve, Line, Point3d>(aspect: Conformance.Profile(count: 3)),
            input: [(null!, Line.Unset)]);

        Assert.True(condition: (result, profile).Apply(static (Seq<Point3d> _, Seq<Point3d> _) => false).As().ToFin().Match(
            Succ: static (bool valid) => valid,
            Fail: static (Error error) => error.Count == 2 && error.Message.Contains(value: "Conformance", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsInvalidMeshFaceMetricRails() {
        Validation<Error, Seq<MeshFaceSample>> invalidMetric = Analyze.Run(
            query: Query.MeshFaceMetric(metric: MeshFaceMetric.None),
            input: [null!, null!]);
        Validation<Error, Seq<MeshFaceSample>> nullMesh = Analyze.Run(
            query: Query.MeshFaceMetric(metric: MeshFaceMetric.AspectRatio),
            input: [null!]);

        Assert.True(condition: (invalidMetric, nullMesh).Apply(static (Seq<MeshFaceSample> _, Seq<MeshFaceSample> _) => false).As().ToFin().Match(
            Succ: static (bool valid) => valid,
            Fail: static (Error error) => error.Count == 2));
    }

    [Fact]
    public void RejectsSpatialInputsBeforeNativeRuntime() {
        Validation<Error, SpatialIndex> invalidPoint = SpatialIndex.Points(points: [Point3d.Unset]);
        Validation<Error, SpatialIndex> invalidBounds = SpatialIndex.Bounds<GeometryBase>(items: [null!]);
        Validation<Error, SpatialIndex> invalidMesh = SpatialIndex.MeshFaces(mesh: null!);
        AnalysisRuntime runtime = new(
            Context: (GeometryContext)RuntimeHelpers.GetUninitializedObject(type: typeof(GeometryContext)));
        Fin<Seq<SpatialPair>> invalidNearest = SpatialIndex.KNearest(
                points: [Point3d.Origin],
                needles: [Point3d.Origin],
                count: 0)
            .Run(runtime);

        Assert.True(condition: invalidPoint.ToFin().IsFail);
        Assert.True(condition: invalidBounds.ToFin().IsFail);
        Assert.True(condition: invalidMesh.ToFin().IsFail);
        Assert.True(condition: invalidNearest.IsFail);
    }

    [Fact]
    public void RejectsNullGeometryInsideConformanceRail() {
        GeometryContext context = (GeometryContext)RuntimeHelpers.GetUninitializedObject(type: typeof(GeometryContext));
        Validation<Error, Seq<double>> result = Analyze.In(context: context)
            .Run(
                query: Query.Conformance<Curve, Line, double>(aspect: Conformance.Distance(count: 3)),
                input: [(null!, ValidLine())]);
        Validation<Error, Seq<ResidualProfile>> profile = Analyze.In(context: context)
            .Run(
                query: Query.Conformance<Curve, Line, ResidualProfile>(aspect: Conformance.Profile(count: 3)),
                input: [(null!, ValidLine())]);

        Assert.True(condition: (result, profile).Apply(static (Seq<double> _, Seq<ResidualProfile> _) => false).As().ToFin().Match(
            Succ: static (bool valid) => valid,
            Fail: static (Error error) => error.Count == 2));
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

    [Fact]
    public void RejectsInvalidDeviationDescriptorBeforeInputExecution() {
        Validation<Error, Seq<CurveDeviation>> result = Analyze.Run(
            query: Query.Deviation<Curve, Curve, CurveDeviation>(aspect: default),
            input: [(null!, null!), (null!, null!)]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<CurveDeviation> _) => false,
            Fail: static (Error error) => error.Count == 1 && error.Message.Contains(value: "Deviation", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedDeviationSurfaceWithOperationVocabulary() {
        Validation<Error, Seq<Point3d>> unsupportedOutput = Analyze.Run(
            query: Query.Deviation<Curve, Curve, Point3d>(aspect: Deviation.Curve),
            input: [(null!, null!)]);
        Validation<Error, Seq<CurveDeviation>> unsupportedPair = Analyze.Run(
            query: Query.Deviation<Curve, Line, CurveDeviation>(aspect: Deviation.Curve),
            input: [(null!, Line.Unset)]);

        Assert.True(condition: (unsupportedOutput, unsupportedPair).Apply(static (Seq<Point3d> _, Seq<CurveDeviation> _) => false).As().ToFin().Match(
            Succ: static (bool valid) => valid,
            Fail: static (Error error) => error.Count == 2 && error.Message.Contains(value: "Deviation", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNullGeometryInsideDeviationRail() {
        GeometryContext context = (GeometryContext)RuntimeHelpers.GetUninitializedObject(type: typeof(GeometryContext));
        Validation<Error, Seq<CurveDeviation>> result = Analyze.In(context: context)
            .Run(
                query: Query.Deviation<Curve, Curve, CurveDeviation>(aspect: Deviation.Curve),
                input: [(null!, null!)]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<CurveDeviation> _) => false,
            Fail: static (Error error) => error.Count == 2));
    }

    [Fact]
    public void RejectsKindForUnsupportedOutputType() {
        Validation<Error, Seq<int>> result = Analyze.Run(
            query: Query.Kind<BoundingBox, int>(),
            input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<int> _) => false,
            Fail: static (Error error) => error.Message.Contains(value: "Kind", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsKindForUnsupportedGeometryType() {
        Validation<Error, Seq<GeometryKind>> result = Analyze.Run(
            query: Query.Kind<int, GeometryKind>(),
            input: [42]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static (Seq<GeometryKind> _) => false,
            Fail: static (Error error) => error.Message.Contains(value: "Kind", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void DetectsLineKindOverObjectInput() {
        object line = new Line(from: Point3d.Origin, to: new Point3d(x: 2.0, y: 0.0, z: 0.0));

        GeometryKind[] kind = Run(
            query: Query.Kind<object, GeometryKind>(),
            input: [line]);

        Assert.Equal(expected: [GeometryKind.Line], actual: kind);
    }

    [Fact]
    public void DetectsBoundingBoxKindOverObjectInput() {
        object bounds = new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0));

        GeometryKind[] kind = Run(
            query: Query.Kind<object, GeometryKind>(),
            input: [bounds]);

        Assert.Equal(expected: [GeometryKind.BoundingBox], actual: kind);
    }

    [Fact]
    public void DetectsBoxKindOverObjectInput() {
        object box = new Box(bbox: new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0)));

        GeometryKind[] kind = Run(
            query: Query.Kind<object, GeometryKind>(),
            input: [box]);

        Assert.Equal(expected: [GeometryKind.Box], actual: kind);
    }

    [Fact]
    public void DetectsSphereKindOverObjectInput() {
        object sphere = new Sphere(center: Point3d.Origin, radius: 1.0);

        GeometryKind[] kind = Run(
            query: Query.Kind<object, GeometryKind>(),
            input: [sphere]);

        Assert.Equal(expected: [GeometryKind.Sphere], actual: kind);
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

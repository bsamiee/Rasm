using System.Reflection;
using LanguageExt;
using LanguageExt.Common;
using Rasm.Analysis;
using Rasm.Domain;
using Rhino;
using Rhino.Geometry;
using Xunit;

namespace Rasm.Tests.Analysis;

// --- [EXAMPLES] --------------------------------------------------------------------------------

public sealed class AnalysisSpec {
    [Fact]
    public void ComputesLineMidpoint() {
        Point3d[] points = Run(
            operation: Analyze.Location<Line, Point3d>(aspect: new Location.MidpointCase()),
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
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Line, Point3d>(aspect: new Location.MidpointCase()),
            input: []);

        Assert.True(condition: result.ToFin().Match(
            Succ: static output => output.IsEmpty,
            Fail: static _ => false));
    }

    [Fact]
    public void ExecutesAggregateOperationAsSequenceEvaluator() {
        Operation<int, int> operation = Operation<int, int>.Build(
            key: Op.Create(value: "SyntheticAggregate"),
            evaluator: static input => Fin.Succ(LanguageExt.Prelude.Seq(input * 2)).ToEff(),
            aggregate: LanguageExt.Prelude.Some<Func<Seq<int>, Eff<Env, Seq<int>>>>(
                static input => input.Fold(initialState: 0, f: static (sum, value) => sum + value) switch {
                    int sum => Fin.Succ(LanguageExt.Prelude.toSeq([sum])).ToEff(),
                }));

        Assert.Equal(expected: [2, 4, 6], actual: Run(operation: operation, input: [1, 2, 3]));
        Assert.False(condition: operation.IsAggregate);
        Assert.True(condition: operation.Aggregate().IsAggregate);
        Assert.Equal(expected: [6], actual: Run(operation: operation.Aggregate(), input: [1, 2, 3]));
    }

    [Fact]
    public void RejectsContextRequiredEmptyInputWithoutScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            operation: Analyze.Measure<Curve, Point3d>(aspect: new Measure.CentroidCase(Mass: MassKind.Length)),
            input: []);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsRequirementDerivedContextNeedWithoutScope() {
        Operation<Line, Point3d> operation = Operation<Line, Point3d>.Build(
            key: Op.Create(value: "SyntheticRequirement"),
            evaluator: static _ => Fin.Succ(LanguageExt.Prelude.Seq(Point3d.Origin)).ToEff(),
            requirement: Requirement.Basic,
            requiresContext: false);

        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            operation: operation,
            input: []);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "model context", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsInvalidUnitScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(units: UnitSystem.Unset)
            .Run(
                operation: Analyze.Location<Line, Point3d>(aspect: new Location.MidpointCase()),
                input: [ValidLine()]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsMissingDocumentScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.From(doc: null)
            .Run(
                operation: Analyze.Location<Line, Point3d>(aspect: new Location.MidpointCase()),
                input: [ValidLine()]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsNullOperation() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run<Line, Point3d>(
            operation: null,
            input: [ValidLine()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsScopedNullOperation() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: null!)
            .Run<Line, Point3d>(
                operation: null,
                input: [ValidLine()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsNullGeometryInsidePureRail() {
        Validation<Error, Seq<BoundingBox>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Bounds<GeometryBase, BoundingBox>(aspect: new Bounds.AxisAlignedCase()),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsNegativeDerivativeCountBeforeRhinoCall() {
        Validation<Error, Seq<Vector3d>> result = Analyze.Run(
            operation: Analyze.Location<Curve, Vector3d>(aspect: new Location.DerivativeAtCase(Parameter: 0.5, Count: -1)),
            input: []);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "DerivativeAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNonGeometryBeforeAggregateMassCast() {
        Validation<Error, Seq<double>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Measure<object, double>(aspect: new Measure.AreaCase()).Aggregate(),
            input: [new object()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Area", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void KeepsParameterlessFactoriesAsProperties() {
        object[] factories = [
            Analyze.Boundaries<Brep, Curve>(aspect: Boundaries.All), Analyze.IsManifold, Analyze.NakedPointStatus, Analyze.Boundaries<Mesh, Polyline>(aspect: Boundaries.SelfIntersection), Analyze.MeshMetric(metric: MeshMetric.AspectRatio),
            Analyze.Boundaries<Mesh, Polyline>(aspect: Boundaries.Naked), Analyze.Points<GeometryBase, Point3d>(sampling: new Points.EdgeMidpointsCase()), Analyze.Curves<Mesh, ComponentIndex>(aspect: Curves.All), Analyze.Curves<Mesh, ComponentIndex>(aspect: Curves.NonManifold),
            Analyze.Measure<GeometryBase, Point3d>(aspect: new Measure.SpatialMidpointCase()), Analyze.Location<Curve, double>(aspect: new Location.CurvatureCase(Count: 3, Mode: CurvatureMode.Scalar(ScalarMetric.Magnitude))), Analyze.Location<Surface, double>(aspect: new Location.CurvatureCase(Count: 3, Mode: CurvatureMode.Scalar(ScalarMetric.Gaussian))), Analyze.Location<Surface, double>(aspect: new Location.CurvatureCase(Count: 3, Mode: CurvatureMode.Scalar(ScalarMetric.Mean))),
            Analyze.Conformance<Curve, Line, double>(aspect: new Conformance.DistanceCase(Count: 3)), Analyze.Conformance<Surface, Plane, bool>(aspect: new Conformance.WithinToleranceCase(Count: 2)), Analyze.Conformance<Curve, Line, Stat>(aspect: new Conformance.SummaryCase(Count: 3)), Analyze.Conformance<Curve, Circle, double>(aspect: new Conformance.DistanceCase(Count: 3)), Analyze.Conformance<Curve, Arc, bool>(aspect: new Conformance.WithinToleranceCase(Count: 3)), Analyze.Conformance<Surface, Sphere, ResidualSample>(aspect: new Conformance.MaximumCase(Count: 2)),
            Analyze.Deviation<Curve, Curve, CurveDeviation>(), Analyze.Points<GeometryBase, Point3d>(sampling: new Points.EdgeMidpointsCase()), Analyze.Points<GeometryBase, Point3d>(sampling: new Points.VerticesCase()),
            Analyze.Faces<GeometryBase, int>(aspect: Faces.All), Analyze.Faces<GeometryBase, Brep>(aspect: Faces.At()), Analyze.Faces<GeometryBase, Plane>(aspect: Faces.At()), Analyze.Faces<GeometryBase, Point3d>(aspect: Faces.At()), Analyze.Faces<GeometryBase, Vector3d>(aspect: Faces.At()), Analyze.Faces<GeometryBase, int>(aspect: Faces.At()), Analyze.Faces<GeometryBase, ComponentIndex>(aspect: Faces.At()), Analyze.Faces<GeometryBase, Interval>(aspect: Faces.At()),
            Analyze.Curves<GeometryBase, Curve>(aspect: Curves.Segments), Analyze.Curves<GeometryBase, Curve>(aspect: Curves.SubCurves), Analyze.Curves<GeometryBase, CurveFeature>(aspect: Curves.All), Analyze.Curves<GeometryBase, ComponentIndex>(aspect: Curves.All), Analyze.Curves<GeometryBase, Curve>(aspect: Curves.Silhouette()), Analyze.Curves<GeometryBase, Curve>(aspect: Curves.Draft()), Analyze.Points<Curve, Point3d>(sampling: new Points.ControlPointsCase()),
        ];

        Assert.All(collection: factories, action: static factory => Assert.NotNull(@object: factory));
    }

    [Fact]
    public void AspectOperationsExposeCanonicalTopologyAndMeshSamples() {
        object[] operations = [
            Meshes.Validity.Operation<object, MeshSample>(), Meshes.Counts.Operation<object, MeshSample>(), Meshes.Defects.Operation<object, MeshSample>(),
            Curves.All.Operation<object, TopologyProjection>(), Faces.All.Operation<object, TopologyProjection>(), Faces.ByCount(static _ => Faces.At()).Operation<object, TopologyProjection>(),
        ];

        Assert.All(collection: operations, action: static operation => Assert.NotNull(@object: operation));
        Assert.True(condition: MeshSampleKind.Validity.Count == 6 && MeshSampleKind.Counts.Count == 6 && MeshSampleKind.Defects.Count == 13);
    }

    [Fact]
    public void RejectsInvalidContextScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: null!)
            .Run(
                operation: Analyze.Location<Line, Point3d>(aspect: new Location.MidpointCase()),
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
            filter: static constructor => constructor.IsPublic);

    [Fact]
    public void PreservesOrderAcrossManyInputs() {
        Point3d[] points = Run(
            operation: Analyze.Location<Line, Point3d>(aspect: new Location.MidpointCase()),
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
    public void PureOperationsPreserveArityLaws(int count) {
        Line[] input = [.. Enumerable.Range(start: 1, count: count)
            .Select(static index => new Line(
                from: Point3d.Origin,
                to: new Point3d(x: index * 2.0, y: 0.0, z: 0.0)))];

        Point3d[] points = Run(
            operation: Analyze.Location<Line, Point3d>(aspect: new Location.MidpointCase()),
            input: input);

        Assert.Equal(expected: count, actual: points.Length);
        Assert.True(condition: points
            .Select(static point => point.X)
            .SequenceEqual(second: Enumerable.Range(start: 1, count: count)
                .Select(static index => (double)index)));
    }

    [Fact]
    public void PreservesOrderAcrossOddArityInputs() {
        Point3d[] points = Run(
            operation: Analyze.Location<Line, Point3d>(aspect: new Location.MidpointCase()),
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
            operation: Analyze.Measure<Line, double>(aspect: new Measure.LengthCase()),
            input: [line]);
        BoundingBox[] bounds = Run(
            operation: Analyze.Bounds<Line, BoundingBox>(aspect: new Bounds.AxisAlignedCase()),
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
            operation: Analyze.Bounds<BoundingBox, Point3d>(aspect: new Bounds.CenterCase()),
            input: [box]);
        Point3d[] corners = Run(
            operation: Analyze.Bounds<BoundingBox, Point3d>(aspect: new Bounds.CornersCase()),
            input: [box]);
        Line[] edges = Run(
            operation: Analyze.Bounds<BoundingBox, Line>(aspect: new Bounds.EdgesCase()),
            input: [box]);
        double[] area = Run(
            operation: Analyze.Bounds<BoundingBox, double>(aspect: new Bounds.AreaCase()),
            input: [box]);
        double[] volume = Run(
            operation: Analyze.Bounds<BoundingBox, double>(aspect: new Bounds.VolumeCase()),
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
            operation: Analyze.Points<object, Point3d>(sampling: new Points.EdgeMidpointsCase()),
            input: [line]);
        Point3d[] polylineEdgeMidpoints = Run(
            operation: Analyze.Points<object, Point3d>(sampling: new Points.EdgeMidpointsCase()),
            input: [polyline]);
        Point3d[] boxSpatialMidpoint = Run(
            operation: Analyze.Measure<object, Point3d>(aspect: new Measure.SpatialMidpointCase()),
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
            operation: Analyze.Bounds<object, Point3d>(aspect: new Bounds.CenterCase()),
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
            operation: Analyze.Bounds<object, Point3d>(aspect: new Bounds.CornersCase()),
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
            operation: Analyze.Bounds<Line, Point3d>(aspect: new Bounds.CenterCase()),
            input: [line]);
        Point3d[] corners = Run(
            operation: Analyze.Bounds<Line, Point3d>(aspect: new Bounds.CornersCase()),
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
            operation: Analyze.Bounds<Polyline, Point3d>(aspect: new Bounds.CenterCase()),
            input: [polyline]);
        Point3d[] corners = Run(
            operation: Analyze.Bounds<Polyline, Point3d>(aspect: new Bounds.CornersCase()),
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
                operation: Analyze.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.AxisAlignedCase()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> outOfRangeRelative = Analyze.In(
                absolute: 0.01,
                relative: 1.0,
                angle: Math.PI / 180.0,
                units: UnitSystem.Unset)
            .Run(
                operation: Analyze.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.AxisAlignedCase()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> nonPositiveAngle = Analyze.In(
                absolute: 0.01,
                relative: 0.0,
                angle: 0.0,
                units: UnitSystem.Unset)
            .Run(
                operation: Analyze.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.AxisAlignedCase()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> overFullTurnAngle = Analyze.In(
                absolute: 0.01,
                relative: 0.0,
                angle: (2.0 * Math.PI) + 1.0,
                units: UnitSystem.Unset)
            .Run(
                operation: Analyze.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.AxisAlignedCase()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> unsetUnits = Analyze.In(
                absolute: 0.01,
                relative: 0.0,
                angle: Math.PI / 180.0,
                units: UnitSystem.Unset)
            .Run(
                operation: Analyze.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.AxisAlignedCase()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);

        Assert.True(condition: nonPositiveAbsolute.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Message.Contains(value: "AbsoluteTolerance", comparisonType: StringComparison.Ordinal)));
        Assert.True(condition: outOfRangeRelative.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Message.Contains(value: "RelativeTolerance", comparisonType: StringComparison.Ordinal)));
        Assert.True(condition: nonPositiveAngle.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Message.Contains(value: "AngleTolerance", comparisonType: StringComparison.Ordinal)));
        Assert.True(condition: overFullTurnAngle.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Message.Contains(value: "AngleTolerance", comparisonType: StringComparison.Ordinal)));
        Assert.True(condition: unsetUnits.ToFin().IsFail);
    }

    [Fact]
    public void RejectsUnsupportedOperationBeforeInputExecution() {
        Validation<Error, Seq<Plane>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Bounds<Line, Plane>(aspect: new Bounds.AxisAlignedCase()),
            input: [
                new Line(from: Point3d.Origin, to: new Point3d(x: 1.0, y: 0.0, z: 0.0)),
                new Line(from: Point3d.Origin, to: new Point3d(x: 0.0, y: 1.0, z: 0.0)),
            ]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Bounds", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedBoundsInputBeforeExecution() {
        Validation<Error, Seq<BoundingBox>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Bounds<int, BoundingBox>(aspect: new Bounds.AxisAlignedCase()),
            input: [1, 2, 3]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsUnsupportedMeasureOutputBeforeInputExecution() {
        Validation<Error, Seq<Plane>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Measure<Mesh, Plane>(aspect: new Measure.AreaCase()),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Measure", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void ReportsRuntimeUnsupportedObjectInputType() {
        Validation<Error, Seq<double>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Measure<object, double>(aspect: new Measure.LengthCase()),
            input: [1.0]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Double", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedAggregateBeforeContextResolution() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            operation: Analyze.Bounds<object, Point3d>(aspect: new Bounds.CornersCase(Unique: true)).Aggregate(),
            input: [Point3d.Origin]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "BoundsCorners", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNoneMassKindOnceBeforeInputExecution() {
        Validation<Error, Seq<double>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Measure<Curve, double>(aspect: new Measure.MassErrorCase(Mass: MassKind.None)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "invalid Rhino input", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedCoercionOutputBeforeInputExecution() {
        Validation<Error, Seq<Sphere>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Coerce<Curve, Sphere>(),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Coerce", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedCurveExtractionBeforeInputExecution() {
        Validation<Error, Seq<Mesh>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Curves<Curve, Mesh>(aspect: Curves.NonManifold),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Curves", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNullGeometryInsideNakedEdgeRail() {
        Validation<Error, Seq<Polyline>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Boundaries<Mesh, Polyline>(aspect: Boundaries.Naked),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsUnsupportedNakedEdgeOutputWithOperationVocabulary() {
        Validation<Error, Seq<Curve>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Boundaries<Mesh, Curve>(aspect: Boundaries.Naked),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "NakedEdges", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsMissingContextOnceBeforeInputExecution() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            operation: Analyze.Measure<Curve, Point3d>(aspect: new Measure.CentroidCase(Mass: MassKind.Length)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void AccumulatesInvalidInputFailures() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Line, Point3d>(aspect: new Location.MidpointCase()),
            input: [
                Line.Unset,
                Line.Unset,
            ]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void RejectsInvalidValueInput() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Line, Point3d>(aspect: new Location.MidpointCase()),
            input: [Line.Unset]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void KeepsIntersectionFactoriesOnTypedRails() {
        Assert.NotNull(@object: Analyze.Intersect<Curve, Curve, IntersectionHit>());
        Assert.NotNull(@object: Analyze.Intersect<Curve, Curve, IntersectionKind>());
        Assert.NotNull(@object: Analyze.Intersect<LineCurve, LineCurve, IntersectionHit>());
        Assert.NotNull(@object: Analyze.Intersect<Plane, Line, Point3d>());
        Assert.NotNull(@object: Analyze.Intersect<Plane, Plane, Line>());
        Assert.NotNull(@object: Analyze.Intersect<Line, Circle, Point3d>());
        Assert.NotNull(@object: Analyze.Intersect<Line, Box, Interval>());

        Validation<Error, Seq<Point3d>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Intersect<Line, Line, Point3d>(),
            input: [(ValidLine(), ValidLine())]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void KeepsIntersectionProjectionCarriersInternalToPublicRails() {
        Assert.True(condition: Analyze.Intersect<Curve, Curve, IntersectionHit>().IsSupported);
        Assert.True(condition: Analyze.Intersect<Curve, Curve, IntersectionKind>().IsSupported);
        Assert.True(condition: Analyze.Intersect<Mesh, Plane, Polyline>().IsSupported);
        Assert.True(condition: Analyze.Intersect<Mesh, Plane, IntersectionKind>().IsSupported);
        Assert.True(condition: Analyze.Intersect<Line, Box, Interval>().IsSupported);
    }

    [Fact]
    public void RejectsUnsupportedIntersectionFamiliesBeforeRuntimeProjection() =>
        Assert.True(condition: !Analyze.Intersect<Point3d, Point3d, IntersectionHit>().IsSupported && !Analyze.Intersect<Curve, Curve, bool>().IsSupported);

    [Fact]
    public void RejectsInvalidCurvatureCountBeforeInputExecution() {
        Validation<Error, Seq<Vector3d>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Curve, Vector3d>(aspect: new Location.CurvatureCase(Count: 0, Mode: CurvatureMode.Vector)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsInvalidExplicitStatKindCountBeforeInputExecution() {
        Validation<Error, Seq<double>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Curve, double>(aspect: new Location.CurvatureCase(Count: 0, Mode: CurvatureMode.Scalar(ScalarMetric.Magnitude))),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "invalid Rhino input", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedCurvatureSummaryBeforeInputExecution() {
        Validation<Error, Seq<Stat>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Line, Stat>(aspect: new Location.CurvatureCase(Count: 3, Mode: CurvatureMode.Vector)),
            input: [ValidLine()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsDefaultStatKindOutputBeforeInputExecution() {
        Validation<Error, Seq<double>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Curve, double>(aspect: new Location.CurvatureCase(Count: 3, Mode: CurvatureMode.Vector)),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedExplicitStatKindBeforeInputExecution() {
        Validation<Error, Seq<double>> curveMean = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Curve, double>(aspect: new Location.CurvatureCase(Count: 3, Mode: CurvatureMode.Scalar(ScalarMetric.Mean))),
            input: [null!]);
        Validation<Error, Seq<double>> curveGaussian = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Curve, double>(aspect: new Location.CurvatureCase(Count: 3, Mode: CurvatureMode.Scalar(ScalarMetric.Gaussian))),
            input: [null!]);
        Validation<Error, Seq<double>> surfaceMagnitude = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Surface, double>(aspect: new Location.CurvatureCase(Count: 3, Mode: CurvatureMode.Scalar(ScalarMetric.Magnitude))),
            input: [null!]);

        Assert.True(condition: (curveMean, curveGaussian, surfaceMagnitude).Apply(static (_, _, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 3 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsCurvaturePreflightMatrixBeforeInputExecution() {
        Validation<Error, Seq<Stat>> invalidSurfaceMagnitude = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Surface, Stat>(aspect: new Location.CurvatureCase(Count: 3, Mode: CurvatureMode.Scalar(ScalarMetric.Magnitude))),
            input: [null!]);
        Validation<Error, Seq<double>> invalidCurveNone = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Curve, double>(aspect: new Location.CurvatureCase(Count: 3, Mode: CurvatureMode.Vector)),
            input: [null!]);
        Validation<Error, Seq<SurfaceCurvature>> invalidSurfaceCount = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Location<Surface, SurfaceCurvature>(aspect: new Location.CurvatureCase(Count: 0, Mode: CurvatureMode.Vector)),
            input: [null!]);

        Assert.True(condition: (invalidSurfaceMagnitude, invalidCurveNone, invalidSurfaceCount).Apply(static (_, _, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 3 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void StatsUseDomainAggregationAndToleranceRail() {
        Op key = Op.Create(value: "stat-rail");
        Fin<Stat> curvature = Stat.Curvature(values: LanguageExt.Prelude.toSeq<double>([1.0, 3.0]), metric: ScalarMetric.Magnitude, key: Op.Create(value: "curvature-stat"));
        Fin<Stat> residual = Stat.Residual(values: LanguageExt.Prelude.toSeq<double>([1.0, 3.0]), tolerance: 3.0, key: Op.Create(value: "residual-stat"));

        Assert.True(condition: Stat.Curvature(values: LanguageExt.Prelude.toSeq<double>([-1.0, 3.0]), metric: ScalarMetric.Gaussian, key: key).IsSucc);
        Assert.True(condition: Stat.Residual(values: LanguageExt.Prelude.toSeq<double>([1.0, 3.0]), tolerance: -1.0, key: key).IsFail);
        Assert.True(condition: curvature.Match(Succ: static stat => stat.Mean == stat.Stats.Mean, Fail: static _ => false));
        Assert.True(condition: residual.Match(Succ: static stat => stat.Rms == stat.Stats.Rms && stat.WithinTolerance, Fail: static _ => false));
    }

    [Fact]
    public void RejectsInvalidConformanceResidualBeforeInputExecution() {
        Validation<Error, Seq<double>> invalidCount = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Conformance<Curve, Line, double>(aspect: new Conformance.DistanceCase(Count: 0)),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);
        Validation<Error, Seq<double>> invalidResidual = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Conformance<Curve, Line, double>(aspect: null!),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);
        Validation<Error, Seq<Stat>> invalidStatCount = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Conformance<Curve, Line, Stat>(aspect: new Conformance.SummaryCase(Count: 0)),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);

        Assert.True(condition: (invalidCount, invalidResidual, invalidStatCount).Apply(static (_, _, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 3 && error.Message.Contains(value: "Conformance", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsInvalidMaximumConformanceBeforeInputExecution() {
        Validation<Error, Seq<ResidualSample>> invalidMaximumCount = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Conformance<Curve, Line, ResidualSample>(aspect: new Conformance.MaximumCase(Count: 0)),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);
        Validation<Error, Seq<ResidualSample>> unsupportedDeferred = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Conformance<Surface, Cylinder, ResidualSample>(aspect: new Conformance.MaximumCase(Count: 2)),
            input: [(null!, Cylinder.Unset)]);

        Assert.True(condition: (invalidMaximumCount, unsupportedDeferred).Apply(static (_, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 2 && error.Message.Contains(value: "Conformance", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedConformanceOutputWithOperationVocabulary() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Conformance<Curve, Line, Point3d>(aspect: new Conformance.DistanceCase(Count: 3)),
            input: [(null!, Line.Unset)]);
        Validation<Error, Seq<Point3d>> stat = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Conformance<Curve, Line, Point3d>(aspect: new Conformance.SummaryCase(Count: 3)),
            input: [(null!, Line.Unset)]);

        Assert.True(condition: (result, stat).Apply(static (_, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 2 && error.Message.Contains(value: "Conformance", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsInvalidMeshMetricRails() {
        Validation<Error, Seq<MeshMetricSample>> invalidMetric = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.MeshMetric(metric: MeshMetric.None),
            input: [null!, null!]);
        Validation<Error, Seq<MeshMetricSample>> nullMesh = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.MeshMetric(metric: MeshMetric.AspectRatio),
            input: [null!]);

        Assert.True(condition: (invalidMetric, nullMesh).Apply(static (_, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void RejectsSpatialInputsBeforeNativeRuntime() {
        Validation<Error, Tree> invalidPoint = Tree.Points(points: [Point3d.Unset]);
        Validation<Error, Tree> invalidBounds = Tree.FromBounds<GeometryBase>(items: [null!]);
        Validation<Error, Tree> invalidMesh = Tree.MeshFaces(mesh: null!);
        Context context = ValidContext();
        Fin<Seq<Couple>> invalidNearest = Spatial.NearestPoints(
                points: [Point3d.Origin],
                needles: [Point3d.Origin],
                probe: new Probe.NearestCase(Count: 0))
            .Run(context);

        Assert.True(condition: invalidPoint.ToFin().IsFail);
        Assert.True(condition: invalidBounds.ToFin().IsFail);
        Assert.True(condition: invalidMesh.ToFin().IsFail);
        Assert.True(condition: invalidNearest.IsFail);
    }

    [Fact]
    public void RejectsNullGeometryInsideConformanceRail() {
        Context context = ValidContext();
        Validation<Error, Seq<double>> result = Analyze.In(context: context)
            .Run(
                operation: Analyze.Conformance<Curve, Line, double>(aspect: new Conformance.DistanceCase(Count: 3)),
                input: [(null!, ValidLine())]);
        Validation<Error, Seq<Stat>> stat = Analyze.In(context: context)
            .Run(
                operation: Analyze.Conformance<Curve, Line, Stat>(aspect: new Conformance.SummaryCase(Count: 3)),
                input: [(null!, ValidLine())]);

        Assert.True(condition: (result, stat).Apply(static (_, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void RejectsUnsupportedIntersectionClassificationWithOperationVocabulary() {
        Validation<Error, Seq<IntersectionKind>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Intersect<Line, int, IntersectionKind>(),
            input: [(ValidLine(), 1)]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Intersect", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void IntersectValidatesBothPairMembersBeforeNativeRuntime() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Intersect<Curve, Curve, Point3d>(),
            input: [(null!, null!)]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void KeepsReverseIntersectionPairsOnTypedRails() {
        Assert.True(condition: Analyze.Intersect<Plane, Line, Point3d>().IsSupported);
        Assert.True(condition: Analyze.Intersect<Line, Curve, IntersectionHit>().IsSupported);
        Assert.True(condition: Analyze.Intersect<Plane, Brep, Curve>().IsSupported);
        Assert.True(condition: Analyze.Intersect<Surface, Brep, Curve>().IsSupported);
        Assert.True(condition: Analyze.Intersect<Plane, Mesh, Polyline>().IsSupported);
    }

    [Fact]
    public void RejectsUnsupportedDeviationSurfaceWithOperationVocabulary() {
        Validation<Error, Seq<Point3d>> unsupportedOutput = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Deviation<Curve, Curve, Point3d>(),
            input: [(null!, null!)]);
        Validation<Error, Seq<CurveDeviation>> unsupportedPair = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Deviation<Curve, Line, CurveDeviation>(),
            input: [(null!, Line.Unset)]);

        Assert.True(condition: (unsupportedOutput, unsupportedPair).Apply(static (_, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 2 && error.Message.Contains(value: "Deviation", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNullGeometryInsideDeviationRail() {
        Context context = ValidContext();
        Validation<Error, Seq<CurveDeviation>> result = Analyze.In(context: context)
            .Run(
                operation: Analyze.Deviation<Curve, Curve, CurveDeviation>(),
                input: [(null!, null!)]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void RejectsKindForUnsupportedOutputType() {
        Validation<Error, Seq<int>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Kind<BoundingBox, int>(),
            input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Message.Contains(value: "Kind", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsKindForUnsupportedGeometryType() {
        Validation<Error, Seq<Kind>> result = Analyze.In(context: ValidContext()).Run(
            operation: Analyze.Kind<int, Kind>(),
            input: [42]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Message.Contains(value: "Kind", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void DetectsObjectKindsInInputOrder() {
        object[] input = [
            new Line(from: Point3d.Origin, to: new Point3d(x: 2.0, y: 0.0, z: 0.0)),
            new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0)),
            new Box(bbox: new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))),
            new Sphere(center: Point3d.Origin, radius: 1.0),
        ];

        Kind[] kind = Run(
            operation: Analyze.Kind<object, Kind>(),
            input: input);

        Assert.Equal(
            expected: [Kind.Line, Kind.BoundingBox, Kind.Box, Kind.Sphere],
            actual: kind);
    }

    [Fact]
    public void ExtractsDirectPointValuesInInputOrder() {
        Point3d first = new(x: 1.0, y: 0.0, z: 0.0);
        Point3d second = new(x: 0.0, y: 2.0, z: 0.0);
        object[] input = [first, second];

        Point3d[] centers = Run(
            operation: Analyze.SpatialMidpoint<object, Point3d>(),
            input: input);
        Point3d[] vertices = Run(
            operation: Analyze.Points<object, Point3d>(sampling: new Points.VerticesCase()),
            input: input);
        Point3d[] corners = Run(
            operation: Analyze.Bounds<object, Point3d>(aspect: new Bounds.CornersCase(Unique: true)),
            input: input);

        Assert.Equal(expected: [first, second], actual: centers);
        Assert.Equal(expected: [first, second], actual: vertices);
        Assert.Equal(expected: [first, second], actual: corners);
    }

    [Fact]
    public void PreflightsGeometryBaseBoundsThroughKernel() =>
        Assert.True(condition: Analyze.Bounds<GeometryBase, BoundingBox>(aspect: new Bounds.AxisAlignedCase()).IsSupported);

    [Fact]
    public void SupportsMeshComponentsFromObjectRail() =>
        Assert.True(condition: Analyze.Components<object, Mesh>().IsSupported);

    [Fact]
    public void SupportsSegmentsThroughCurveTopologyProjection() {
        Assert.True(condition: Analyze.Curves<Curve, Curve>(aspect: Curves.Segments).IsSupported);
        Assert.True(condition: Analyze.Segments<Curve, Curve>().IsSupported);
    }

    [Fact]
    public void SupportsBrepBoundariesThroughCurveTopologyProjection() {
        Assert.True(condition: Analyze.Boundaries<Brep, Curve>(aspect: Boundaries.All).IsSupported);
        Assert.True(condition: Analyze.Boundaries<BrepFace, Curve>(aspect: Boundaries.All).IsSupported);
    }

    [Fact]
    public void RegistersBrepFaceBoundaryAsTrimAwareCurveProjection() {
        Assert.True(condition: Analyze.Boundaries<Surface, Curve>(aspect: Boundaries.All).IsSupported);
        Assert.True(condition: Analyze.Boundaries<BrepFace, Curve>(aspect: Boundaries.All).IsSupported);
    }

    [Fact]
    public void TopologyProjectionFactoriesAreOnlyPublicConstructionSurface() {
        Assert.Empty(collection: typeof(TopologyProjection).GetConstructors(bindingAttr: BindingFlags.Public | BindingFlags.Instance));
        Assert.DoesNotContain(collection: typeof(TopologyProjection).GetNestedTypes(bindingAttr: BindingFlags.Public), filter: static type => type.Name is "CurveCase" or "FaceCase" or "MeshFaceCase");
        Assert.True(condition: TopologyProjection.MeshFace(mesh: null, face: 0).IsFail);
    }

    // Faces / FaceFrame operations run on Brep, BrepFace, Surface, SubD, all of which
    // dispatch to native rhcommon_c during construction and analysis. The xUnit runner here
    // does not load the native runtime; integration coverage for face decomposition,
    // top/bottom selection, index clamping, UV frame alignment, and the Mesh rejection rail
    // should wait for a current Rhino.Testing runtime path. The selector index flows through
    // Faces.At(index) at the GH boundary.

    private static Line ValidLine() =>
        new(
            from: Point3d.Origin,
            to: new Point3d(x: 2.0, y: 0.0, z: 0.0));
    private static Context ValidContext() =>
        Context.Create(
                absolute: 0.01,
                relative: 0.001,
                angle: Math.PI / 180.0,
                units: UnitSystem.Millimeters)
            .ToFin()
            .Match(
                Succ: static context => context,
                Fail: static error => throw new Xunit.Sdk.XunitException(error.Message));
    private static TOut[] Run<TGeometry, TOut>(
        Operation<TGeometry, TOut> operation,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Analyze.In(context: ValidContext()).Run(
                operation: operation,
                input: input)
            .ToFin()
            .Match(
                Succ: static output => output.ToArray(),
                Fail: static error => throw new Xunit.Sdk.XunitException(error.Message));

}

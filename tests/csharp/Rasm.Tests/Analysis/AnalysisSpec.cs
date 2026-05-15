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
            query: Analyze.Location<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Analyze.Location<Line, Point3d>(aspect: new Location.Midpoint()),
            input: []);

        Assert.True(condition: result.ToFin().Match(
            Succ: static output => output.IsEmpty,
            Fail: static _ => false));
    }

    [Fact]
    public void ExecutesAggregateQueryAsSequenceEvaluator() {
        Query<int, int> query = new(
            key: Op.Create(value: "SyntheticAggregate"),
            effect: static input => Fin.Succ(input.Map(static value => value * 2)).ToEff(),
            aggregate: LanguageExt.Prelude.Some<Func<Seq<int>, Eff<Env, Seq<int>>>>(
                static input => input.Fold(initialState: 0, f: static (sum, value) => sum + value) switch {
                    int sum => Fin.Succ(LanguageExt.Prelude.toSeq([sum])).ToEff(),
                }));

        Assert.Equal(expected: [2, 4, 6], actual: Run(query: query, input: [1, 2, 3]));
        Assert.Equal(expected: [6], actual: Run(query: query.Aggregate(), input: [1, 2, 3]));
    }

    [Fact]
    public void RejectsContextRequiredEmptyInputWithoutScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Analyze.Measure<Curve, Point3d>(aspect: new Measure.Centroid(Mass: MassKind.Length)),
            input: []);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsRequirementDerivedContextNeedWithoutScope() {
        Query<Line, Point3d> query = Query<Line, Point3d>.Build(
            key: Op.Create(value: "SyntheticRequirement"),
            evaluator: static _ => Fin.Succ(LanguageExt.Prelude.Seq(Point3d.Origin)).ToEff(),
            requirement: Requirement.Basic,
            requiresContext: false);

        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: query,
            input: []);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "model context", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsInvalidUnitScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(units: UnitSystem.Unset)
            .Run(
                query: Analyze.Location<Line, Point3d>(aspect: new Location.Midpoint()),
                input: [ValidLine()]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsMissingDocumentScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.From(doc: null)
            .Run(
                query: Analyze.Location<Line, Point3d>(aspect: new Location.Midpoint()),
                input: [ValidLine()]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void RejectsNullQuery() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run<Line, Point3d>(
            query: null,
            input: [ValidLine()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsScopedNullQuery() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: null!)
            .Run<Line, Point3d>(
                query: null,
                input: [ValidLine()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsNullGeometryInsidePureRail() {
        Validation<Error, Seq<BoundingBox>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Bounds<GeometryBase, BoundingBox>(aspect: new Bounds.AxisAligned()),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsNegativeDerivativeCountBeforeRhinoCall() {
        Validation<Error, Seq<Vector3d>> result = Analyze.Run(
            query: Analyze.Location<Curve, Vector3d>(aspect: new Location.DerivativeAt(Parameter: 0.5, Count: -1)),
            input: []);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "DerivativeAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNonGeometryBeforeAggregateMassCast() {
        Validation<Error, Seq<double>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Measure<object, double>(aspect: new Measure.Area()).Aggregate(),
            input: [new object()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Area", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void KeepsParameterlessFactoriesAsProperties() {
        object[] factories = [
            Analyze.Boundaries<Brep, Curve>(aspect: Boundaries.All), Analyze.IsManifold, Analyze.NakedPointStatus, Analyze.Boundaries<Mesh, Polyline>(aspect: Boundaries.SelfIntersection),
            Analyze.MeshCheckCount(count: MeshCheckCount.NakedEdges), Analyze.MeshFaceMetric(metric: MeshFaceMetric.AspectRatio),
            Analyze.Boundaries<Mesh, Polyline>(aspect: Boundaries.Naked), Analyze.Points<GeometryBase, Point3d>(sampling: new Points.EdgeMidpoints()), Analyze.Curves<Mesh, ComponentIndex>(aspect: Curves.All), Analyze.Curves<Mesh, ComponentIndex>(aspect: Curves.NonManifold),
            Analyze.Measure<GeometryBase, Point3d>(aspect: new Measure.SpatialMidpoint()), Analyze.Location<Curve, double>(aspect: new Location.Curvature(Count: 3, Scalar: CurvatureScalar.Magnitude)), Analyze.Location<Surface, double>(aspect: new Location.Curvature(Count: 3, Scalar: CurvatureScalar.Gaussian)), Analyze.Location<Surface, double>(aspect: new Location.Curvature(Count: 3, Scalar: CurvatureScalar.Mean)),
            Analyze.Conformance<Curve, Line, double>(aspect: new Conformance.Distance(Count: 3)), Analyze.Conformance<Surface, Plane, bool>(aspect: new Conformance.WithinTolerance(Count: 2)), Analyze.Conformance<Curve, Line, StatProfile>(aspect: new Conformance.Summary(Count: 3)), Analyze.Conformance<Curve, Circle, double>(aspect: new Conformance.Distance(Count: 3)), Analyze.Conformance<Curve, Arc, bool>(aspect: new Conformance.WithinTolerance(Count: 3)), Analyze.Conformance<Surface, Sphere, ResidualSample>(aspect: new Conformance.Maximum(Count: 2)),
            Analyze.Deviation<Curve, Curve, CurveDeviation>(), Analyze.Points<GeometryBase, Point3d>(sampling: new Points.EdgeMidpoints()), Analyze.Points<GeometryBase, Point3d>(sampling: new Points.Vertices()),
            Analyze.Faces<GeometryBase, int>(aspect: Faces.All), Analyze.Faces<GeometryBase, Brep>(aspect: Faces.At()), Analyze.Faces<GeometryBase, Plane>(aspect: Faces.At()), Analyze.Faces<GeometryBase, Point3d>(aspect: Faces.At()), Analyze.Faces<GeometryBase, Vector3d>(aspect: Faces.At()), Analyze.Faces<GeometryBase, int>(aspect: Faces.At()), Analyze.Faces<GeometryBase, ComponentIndex>(aspect: Faces.At()), Analyze.Faces<GeometryBase, Interval>(aspect: Faces.At()),
            Analyze.Curves<GeometryBase, Curve>(aspect: Curves.Segments), Analyze.Curves<GeometryBase, Curve>(aspect: Curves.SubCurves), Analyze.Curves<GeometryBase, CurveFeature>(aspect: Curves.All), Analyze.Curves<GeometryBase, ComponentIndex>(aspect: Curves.All), Analyze.Curves<GeometryBase, Curve>(aspect: Curves.Silhouette()), Analyze.Curves<GeometryBase, Curve>(aspect: Curves.Draft()), Analyze.Points<Curve, Point3d>(sampling: new Points.ControlPoints()),
        ];

        Assert.All(collection: factories, action: static factory => Assert.NotNull(@object: factory));
    }

    [Fact]
    public void RejectsInvalidContextScope() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: null!)
            .Run(
                query: Analyze.Location<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Analyze.Location<Line, Point3d>(aspect: new Location.Midpoint()),
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
            .Select(static index => new Line(
                from: Point3d.Origin,
                to: new Point3d(x: index * 2.0, y: 0.0, z: 0.0)))];

        Point3d[] points = Run(
            query: Analyze.Location<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Analyze.Location<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Analyze.Measure<Line, double>(aspect: new Measure.Length()),
            input: [line]);
        BoundingBox[] bounds = Run(
            query: Analyze.Bounds<Line, BoundingBox>(aspect: new Bounds.AxisAligned()),
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
            query: Analyze.Bounds<BoundingBox, Point3d>(aspect: new Bounds.Center()),
            input: [box]);
        Point3d[] corners = Run(
            query: Analyze.Bounds<BoundingBox, Point3d>(aspect: new Bounds.Corners()),
            input: [box]);
        Line[] edges = Run(
            query: Analyze.Bounds<BoundingBox, Line>(aspect: new Bounds.Edges()),
            input: [box]);
        double[] area = Run(
            query: Analyze.Bounds<BoundingBox, double>(aspect: new Bounds.Area()),
            input: [box]);
        double[] volume = Run(
            query: Analyze.Bounds<BoundingBox, double>(aspect: new Bounds.Volume()),
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
            query: Analyze.Points<object, Point3d>(sampling: new Points.EdgeMidpoints()),
            input: [line]);
        Point3d[] polylineEdgeMidpoints = Run(
            query: Analyze.Points<object, Point3d>(sampling: new Points.EdgeMidpoints()),
            input: [polyline]);
        Point3d[] boxSpatialMidpoint = Run(
            query: Analyze.Measure<object, Point3d>(aspect: new Measure.SpatialMidpoint()),
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
            query: Analyze.Bounds<object, Point3d>(aspect: new Bounds.Center()),
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
            query: Analyze.Bounds<object, Point3d>(aspect: new Bounds.Corners()),
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
            query: Analyze.Bounds<Line, Point3d>(aspect: new Bounds.Center()),
            input: [line]);
        Point3d[] corners = Run(
            query: Analyze.Bounds<Line, Point3d>(aspect: new Bounds.Corners()),
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
            query: Analyze.Bounds<Polyline, Point3d>(aspect: new Bounds.Center()),
            input: [polyline]);
        Point3d[] corners = Run(
            query: Analyze.Bounds<Polyline, Point3d>(aspect: new Bounds.Corners()),
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
                query: Analyze.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.AxisAligned()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> outOfRangeRelative = Analyze.In(
                absolute: 0.01,
                relative: 1.0,
                angle: Math.PI / 180.0,
                units: UnitSystem.Unset)
            .Run(
                query: Analyze.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.AxisAligned()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> nonPositiveAngle = Analyze.In(
                absolute: 0.01,
                relative: 0.0,
                angle: 0.0,
                units: UnitSystem.Unset)
            .Run(
                query: Analyze.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.AxisAligned()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> overFullTurnAngle = Analyze.In(
                absolute: 0.01,
                relative: 0.0,
                angle: (2.0 * Math.PI) + 1.0,
                units: UnitSystem.Unset)
            .Run(
                query: Analyze.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.AxisAligned()),
                input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);
        Validation<Error, Seq<BoundingBox>> unsetUnits = Analyze.In(
                absolute: 0.01,
                relative: 0.0,
                angle: Math.PI / 180.0,
                units: UnitSystem.Unset)
            .Run(
                query: Analyze.Bounds<BoundingBox, BoundingBox>(aspect: new Bounds.AxisAligned()),
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
    public void RejectsUnsupportedQueryBeforeInputExecution() {
        Validation<Error, Seq<Plane>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Bounds<Line, Plane>(aspect: new Bounds.AxisAligned()),
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
            query: Analyze.Bounds<int, BoundingBox>(aspect: new Bounds.AxisAligned()),
            input: [1, 2, 3]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsUnsupportedMeasureOutputBeforeInputExecution() {
        Validation<Error, Seq<Plane>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Measure<Mesh, Plane>(aspect: new Measure.Area()),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Measure", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void ReportsRuntimeUnsupportedObjectInputType() {
        Validation<Error, Seq<double>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Measure<object, double>(aspect: new Measure.Length()),
            input: [1.0]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Double", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedAggregateBeforeContextResolution() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Analyze.Bounds<object, Point3d>(aspect: new Bounds.Corners(Unique: true)).Aggregate(),
            input: [Point3d.Origin]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "BoundsCorners", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNoneMassKindOnceBeforeInputExecution() {
        Validation<Error, Seq<double>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Measure<Curve, double>(aspect: new Measure.MassError(Mass: MassKind.None)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "invalid Rhino input", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedCoercionOutputBeforeInputExecution() {
        Validation<Error, Seq<Sphere>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Coerce<Curve, Sphere>(),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Coerce", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedCurveExtractionBeforeInputExecution() {
        Validation<Error, Seq<Mesh>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Curves<Curve, Mesh>(aspect: Curves.NonManifold),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Curves", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNullGeometryInsideNakedEdgeRail() {
        Validation<Error, Seq<Polyline>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Boundaries<Mesh, Polyline>(aspect: Boundaries.Naked),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsUnsupportedNakedEdgeOutputWithOperationVocabulary() {
        Validation<Error, Seq<Curve>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Boundaries<Mesh, Curve>(aspect: Boundaries.Naked),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "NakedEdges", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsInvalidMeshCheckCountBeforeInputExecution() {
        Validation<Error, Seq<int>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.MeshCheckCount(count: MeshCheckCount.None),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "MeshCheckCount", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsNullGeometryInsideMeshCheckCountRail() {
        Validation<Error, Seq<int>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.MeshCheckCount(count: MeshCheckCount.NakedEdges),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsMissingContextOnceBeforeInputExecution() {
        Validation<Error, Seq<Point3d>> result = Analyze.Run(
            query: Analyze.Measure<Curve, Point3d>(aspect: new Measure.Centroid(Mass: MassKind.Length)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void AccumulatesInvalidInputFailures() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Location<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Analyze.Location<Line, Point3d>(aspect: new Location.Midpoint()),
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
            query: Analyze.Intersect<Line, Line, Point3d>(),
            input: [(ValidLine(), ValidLine())]);

        Assert.True(condition: result.ToFin().IsFail);
    }

    [Fact]
    public void PreservesExplicitIntersectionKindsForPolylineResults() {
        Op key = Op.Create(value: "test");

        IntersectionKind[] kinds = new IntersectionResult.Polylines(
                Values: LanguageExt.Prelude.Seq((Curve: (Polyline)[], Kind: IntersectionKind.Curve), (Curve: (Polyline)[], Kind: IntersectionKind.Overlap)))
            .Project<IntersectionKind>(key: key)
            .Match(
                Succ: static output => output.ToArray(),
                Fail: static error => throw new Xunit.Sdk.XunitException(error.Message));

        Assert.Equal(expected: [IntersectionKind.Curve, IntersectionKind.Overlap], actual: kinds);
    }

    [Fact]
    public void ProjectsSnapshotIntersectionHits() {
        Op key = Op.Create(value: "test");

        IntersectionKind[] kinds = new IntersectionResult.Hits(Values: LanguageExt.Prelude.Seq(IntersectionHit.At(point: Point3d.Origin)))
            .Project<IntersectionKind>(key: key)
            .Match(
                Succ: static output => output.ToArray(),
                Fail: static error => throw new Xunit.Sdk.XunitException(error.Message));

        Assert.Equal(expected: [IntersectionKind.Point], actual: kinds);
    }

    [Fact]
    public void ProjectsEveryIntersectionResultShape() {
        Op key = Op.Create(value: "test");
        Polyline polyline = new([
            Point3d.Origin,
            new Point3d(x: 1.0, y: 0.0, z: 0.0),
        ]);

        Assert.True(condition: LanguageExt.Prelude.Seq(
            new IntersectionResult.Curves(Values: LanguageExt.Prelude.Seq<Curve>()).Project<Curve>(key: key).IsSucc,
            new IntersectionResult.Lines(Values: LanguageExt.Prelude.Seq(ValidLine())).Project<Line>(key: key).IsSucc,
            new IntersectionResult.Circles(Values: LanguageExt.Prelude.Seq(new Circle(plane: Plane.WorldXY, radius: 1.0))).Project<Circle>(key: key).IsSucc,
            new IntersectionResult.Points(Values: LanguageExt.Prelude.Seq(Point3d.Origin)).Project<Point3d>(key: key).IsSucc,
            new IntersectionResult.Intervals(Values: LanguageExt.Prelude.Seq(new Interval(t0: 0.0, t1: 1.0))).Project<Interval>(key: key).IsSucc,
            new IntersectionResult.Polylines(Values: LanguageExt.Prelude.Seq((Curve: polyline, Kind: IntersectionKind.Curve))).Project<Polyline>(key: key).IsSucc,
            new IntersectionResult.Hits(Values: LanguageExt.Prelude.Seq(IntersectionHit.At(point: Point3d.Origin))).Project<IntersectionHit>(key: key).IsSucc).ForAll(static passed => passed));
    }

    [Fact]
    public void RejectsUnsupportedIntersectionResultProjectionWithCaseType() {
        Op key = Op.Create(value: "test");

        Assert.True(condition: new IntersectionResult.Points(Values: LanguageExt.Prelude.Seq(Point3d.Origin)).Project<Line>(key: key).IsFail);
    }

    [Fact]
    public void RejectsInvalidCurvatureCountBeforeInputExecution() {
        Validation<Error, Seq<Vector3d>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Location<Curve, Vector3d>(aspect: new Location.Curvature(Count: 0, Scalar: CurvatureScalar.None)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1));
    }

    [Fact]
    public void RejectsInvalidExplicitCurvatureScalarCountBeforeInputExecution() {
        Validation<Error, Seq<double>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Location<Curve, double>(aspect: new Location.Curvature(Count: 0, Scalar: CurvatureScalar.Magnitude)),
            input: [null!, null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "invalid Rhino input", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedCurvatureSummaryBeforeInputExecution() {
        Validation<Error, Seq<StatProfile>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Location<Line, StatProfile>(aspect: new Location.Curvature(Count: 3, Scalar: CurvatureScalar.None)),
            input: [ValidLine()]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsDefaultCurvatureScalarOutputBeforeInputExecution() {
        Validation<Error, Seq<double>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Location<Curve, double>(aspect: new Location.Curvature(Count: 3, Scalar: CurvatureScalar.None)),
            input: [null!]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedExplicitCurvatureScalarBeforeInputExecution() {
        Validation<Error, Seq<double>> curveMean = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Location<Curve, double>(aspect: new Location.Curvature(Count: 3, Scalar: CurvatureScalar.Mean)),
            input: [null!]);
        Validation<Error, Seq<double>> curveGaussian = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Location<Curve, double>(aspect: new Location.Curvature(Count: 3, Scalar: CurvatureScalar.Gaussian)),
            input: [null!]);
        Validation<Error, Seq<double>> surfaceMagnitude = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Location<Surface, double>(aspect: new Location.Curvature(Count: 3, Scalar: CurvatureScalar.Magnitude)),
            input: [null!]);

        Assert.True(condition: (curveMean, curveGaussian, surfaceMagnitude).Apply(static (_, _, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 3 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsCurvaturePreflightMatrixBeforeInputExecution() {
        Validation<Error, Seq<StatProfile>> invalidSurfaceMagnitude = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Location<Surface, StatProfile>(aspect: new Location.Curvature(Count: 3, Scalar: CurvatureScalar.Magnitude)),
            input: [null!]);
        Validation<Error, Seq<double>> invalidCurveNone = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Location<Curve, double>(aspect: new Location.Curvature(Count: 3, Scalar: CurvatureScalar.None)),
            input: [null!]);
        Validation<Error, Seq<SurfaceCurvature>> invalidSurfaceCount = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Location<Surface, SurfaceCurvature>(aspect: new Location.Curvature(Count: 0, Scalar: CurvatureScalar.None)),
            input: [null!]);

        Assert.True(condition: (invalidSurfaceMagnitude, invalidCurveNone, invalidSurfaceCount).Apply(static (_, _, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 3 && error.Message.Contains(value: "CurvatureAt", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void ProfilesUseStatsAsCanonicalSummary() {
        Fin<StatProfile> curvature = StatProfile.Curvature(values: LanguageExt.Prelude.toSeq<double>([1.0, 3.0]), scalar: CurvatureScalar.Magnitude, key: Op.Create(value: "curvature-profile"));
        Fin<StatProfile> residual = StatProfile.Residual(values: LanguageExt.Prelude.toSeq<double>([1.0, 3.0]), tolerance: 3.0, key: Op.Create(value: "residual-profile"));

        Assert.True(condition: curvature.Match(Succ: static profile => profile.Mean == profile.Stats.Mean, Fail: static _ => false));
        Assert.True(condition: residual.Match(Succ: static profile => profile.Rms == profile.Stats.Rms && profile.WithinTolerance, Fail: static _ => false));
    }

    [Fact]
    public void RejectsInvalidConformanceResidualBeforeInputExecution() {
        Validation<Error, Seq<double>> invalidCount = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Conformance<Curve, Line, double>(aspect: new Conformance.Distance(Count: 0)),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);
        Validation<Error, Seq<double>> invalidResidual = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Conformance<Curve, Line, double>(aspect: null!),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);
        Validation<Error, Seq<StatProfile>> invalidProfileCount = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Conformance<Curve, Line, StatProfile>(aspect: new Conformance.Summary(Count: 0)),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);

        Assert.True(condition: (invalidCount, invalidResidual, invalidProfileCount).Apply(static (_, _, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 3 && error.Message.Contains(value: "Conformance", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsInvalidMaximumConformanceBeforeInputExecution() {
        Validation<Error, Seq<ResidualSample>> invalidMaximumCount = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Conformance<Curve, Line, ResidualSample>(aspect: new Conformance.Maximum(Count: 0)),
            input: [(null!, Line.Unset), (null!, Line.Unset)]);
        Validation<Error, Seq<ResidualSample>> unsupportedDeferred = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Conformance<Surface, Cylinder, ResidualSample>(aspect: new Conformance.Maximum(Count: 2)),
            input: [(null!, Cylinder.Unset)]);

        Assert.True(condition: (invalidMaximumCount, unsupportedDeferred).Apply(static (_, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 2 && error.Message.Contains(value: "Conformance", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsUnsupportedConformanceOutputWithOperationVocabulary() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Conformance<Curve, Line, Point3d>(aspect: new Conformance.Distance(Count: 3)),
            input: [(null!, Line.Unset)]);
        Validation<Error, Seq<Point3d>> profile = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Conformance<Curve, Line, Point3d>(aspect: new Conformance.Summary(Count: 3)),
            input: [(null!, Line.Unset)]);

        Assert.True(condition: (result, profile).Apply(static (_, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 2 && error.Message.Contains(value: "Conformance", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsInvalidMeshFaceMetricRails() {
        Validation<Error, Seq<MeshFaceSample>> invalidMetric = Analyze.In(context: ValidContext()).Run(
            query: Analyze.MeshFaceMetric(metric: MeshFaceMetric.None),
            input: [null!, null!]);
        Validation<Error, Seq<MeshFaceSample>> nullMesh = Analyze.In(context: ValidContext()).Run(
            query: Analyze.MeshFaceMetric(metric: MeshFaceMetric.AspectRatio),
            input: [null!]);

        Assert.True(condition: (invalidMetric, nullMesh).Apply(static (_, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void RejectsSpatialInputsBeforeNativeRuntime() {
        Validation<Error, Tree> invalidPoint = Tree.Points(points: [Point3d.Unset]);
        Validation<Error, Tree> invalidBounds = Tree.Bounds<GeometryBase>(items: [null!]);
        Validation<Error, Tree> invalidMesh = Tree.MeshFaces(mesh: null!);
        Context context = ValidContext();
        Fin<Seq<Couple>> invalidNearest = Spatial.NearestPoints(
                points: [Point3d.Origin],
                needles: [Point3d.Origin],
                probe: new Probe.Nearest(Count: 0))
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
                query: Analyze.Conformance<Curve, Line, double>(aspect: new Conformance.Distance(Count: 3)),
                input: [(null!, ValidLine())]);
        Validation<Error, Seq<StatProfile>> profile = Analyze.In(context: context)
            .Run(
                query: Analyze.Conformance<Curve, Line, StatProfile>(aspect: new Conformance.Summary(Count: 3)),
                input: [(null!, ValidLine())]);

        Assert.True(condition: (result, profile).Apply(static (_, _) => false).As().ToFin().Match(
            Succ: static valid => valid,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void RejectsUnsupportedIntersectionClassificationWithOperationVocabulary() {
        Validation<Error, Seq<IntersectionKind>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Intersect<Line, int, IntersectionKind>(),
            input: [(ValidLine(), 1)]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 1 && error.Message.Contains(value: "Intersect", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void IntersectValidatesBothPairMembersBeforeNativeRuntime() {
        Validation<Error, Seq<Point3d>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Intersect<Curve, Curve, Point3d>(),
            input: [(null!, null!)]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void KeepsReverseIntersectionPairsOnTypedRails() {
        Assert.False(condition: Dispatch.Supports(CapTag.Intersect, typeof(Plane), typeof(Line)));
        Assert.True(condition: Dispatch.Supports(CapTag.Intersect, typeof(Plane), typeof(Line), unordered: true));
        Assert.True(condition: Analyze.Intersect<Line, Curve, IntersectionHit>().Rejection.IsNone);
        Assert.True(condition: Analyze.Intersect<Plane, Brep, Curve>().Rejection.IsNone);
        Assert.True(condition: Analyze.Intersect<Surface, Brep, Curve>().Rejection.IsNone);
        Assert.True(condition: Analyze.Intersect<Plane, Mesh, Polyline>().Rejection.IsNone);
    }

    [Fact]
    public void RejectsUnsupportedDeviationSurfaceWithOperationVocabulary() {
        Validation<Error, Seq<Point3d>> unsupportedOutput = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Deviation<Curve, Curve, Point3d>(),
            input: [(null!, null!)]);
        Validation<Error, Seq<CurveDeviation>> unsupportedPair = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Deviation<Curve, Line, CurveDeviation>(),
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
                query: Analyze.Deviation<Curve, Curve, CurveDeviation>(),
                input: [(null!, null!)]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Count == 2));
    }

    [Fact]
    public void RejectsKindForUnsupportedOutputType() {
        Validation<Error, Seq<int>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Kind<BoundingBox, int>(),
            input: [new BoundingBox(min: Point3d.Origin, max: new Point3d(x: 1.0, y: 1.0, z: 1.0))]);

        Assert.True(condition: result.ToFin().Match(
            Succ: static _ => false,
            Fail: static error => error.Message.Contains(value: "Kind", comparisonType: StringComparison.Ordinal)));
    }

    [Fact]
    public void RejectsKindForUnsupportedGeometryType() {
        Validation<Error, Seq<Kind>> result = Analyze.In(context: ValidContext()).Run(
            query: Analyze.Kind<int, Kind>(),
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
            query: Analyze.Kind<object, Kind>(),
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
            query: Analyze.SpatialMidpoint<object, Point3d>(),
            input: input);
        Point3d[] vertices = Run(
            query: Analyze.Points<object, Point3d>(sampling: new Points.Vertices()),
            input: input);
        Point3d[] corners = Run(
            query: Analyze.Bounds<object, Point3d>(aspect: new Bounds.Corners(Unique: true)),
            input: input);

        Assert.Equal(expected: [first, second], actual: centers);
        Assert.Equal(expected: [first, second], actual: vertices);
        Assert.Equal(expected: [first, second], actual: corners);
    }

    [Fact]
    public void PreflightsGeometryBaseBoundsThroughDispatch() =>
        Assert.True(condition: Analyze.Bounds<GeometryBase, BoundingBox>(aspect: new Bounds.AxisAligned()).Rejection.IsNone);

    [Fact]
    public void SupportsMeshComponentsFromObjectRail() =>
        Assert.True(condition: Analyze.Components<object, Mesh>().Rejection.IsNone);

    [Fact]
    public void SupportsSegmentsThroughCurveTopologyProjection() {
        Assert.True(condition: Analyze.Curves<Curve, Curve>(aspect: Curves.Segments).Rejection.IsNone);
        Assert.True(condition: Analyze.Segments<Curve, Curve>().Rejection.IsNone);
    }

    [Fact]
    public void SupportsBrepBoundariesThroughCurveTopologyProjection() {
        Assert.True(condition: Analyze.Boundaries<Brep, Curve>(aspect: Boundaries.All).Rejection.IsNone);
        Assert.True(condition: Analyze.Boundaries<BrepFace, Curve>(aspect: Boundaries.All).Rejection.IsNone);
    }

    [Fact]
    public void RegistersBrepFaceBoundaryAsTrimAwareCurveCapability() {
        Assert.True(condition: Dispatch.Supports(CapTag.Curves, typeof(BrepFace), variant: CurveFeature.Boundary));
        Assert.True(condition: Analyze.Boundaries<Surface, Curve>(aspect: Boundaries.All).Rejection.IsNone);
        Assert.True(condition: Analyze.Boundaries<BrepFace, Curve>(aspect: Boundaries.All).Rejection.IsNone);
    }

    [Fact]
    public void TopologyProjectionFactoriesAreOnlyPublicConstructionSurface() {
        Assert.Empty(collection: typeof(TopologyProjection).GetConstructors(bindingAttr: BindingFlags.Public | BindingFlags.Instance));
        Assert.DoesNotContain(collection: typeof(TopologyProjection).GetNestedTypes(bindingAttr: BindingFlags.Public), filter: static type => type.Name is "CurveCase" or "FaceCase" or "MeshFaceCase");
        Assert.True(condition: TopologyProjection.MeshFace(mesh: null, face: 0).IsFail);
    }

    // Faces / FaceFrame queries operate on Brep, BrepFace, Surface, SubD — all of which
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
        Query<TGeometry, TOut> query,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Analyze.In(context: ValidContext()).Run(
                query: query,
                input: input)
            .ToFin()
            .Match(
                Succ: static output => output.ToArray(),
                Fail: static error => throw new Xunit.Sdk.XunitException(error.Message));

}

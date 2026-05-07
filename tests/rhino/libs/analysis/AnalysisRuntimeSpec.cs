using Analysis;
using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using NUnit.Framework;
using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Testing.Fixtures;
using static LanguageExt.Prelude;
using AnalysisQuery = Analysis.Query;

namespace Runtime.Rhino.Tests.Libs.Analysis;

// --- [RUNTIME SPECS] ---------------------------------------------------------------------------

[TestFixture]
[RhinoTestFixture]
public sealed class AnalysisRuntimeSpec {
    [Test]
    public void ComputesValueAnalysisThatUsesRhinoNativeRuntime() {
        Polyline polyline = new([
            Point3d.Origin,
            new Point3d(x: 2.0, y: 0.0, z: 0.0),
            new Point3d(x: 4.0, y: 0.0, z: 0.0),
        ]);
        Line line = new(
            from: Point3d.Origin,
            to: new Point3d(x: 3.0, y: 4.0, z: 0.0));
        using LineCurve curve = new(line: line);

        Point3d[] midpoint = Run(
            query: AnalysisQuery.Locate<Polyline, Point3d>(aspect: Location.Midpoint),
            context: Context(),
            input: [polyline]);
        Vector3d[] tangent = Run(
            query: AnalysisQuery.Locate<Line, Vector3d>(aspect: Location.Tangent),
            context: Context(),
            input: [line]);
        Vector3d[] curveTangent = Run(
            query: AnalysisQuery.Locate<Curve, Vector3d>(aspect: Location.Tangent),
            context: Context(),
            input: [curve]);
        Point3d[] closest = Run(
            query: AnalysisQuery.Locate<Line, Point3d>(aspect: Location.Closest(point: new Point3d(x: 2.0, y: 3.0, z: 0.0))),
            context: Context(),
            input: [line]);
        Line[] segments = Run(
            query: AnalysisQuery.Segments<Polyline, Line>(),
            context: Context(),
            input: [polyline]);
        double[] curveLength = Run(
            query: AnalysisQuery.Measure<Curve, double>(aspect: Measure.Length),
            context: Context(),
            input: [curve]);
        Point3d[] lengthCentroid = Run(
            query: AnalysisQuery.Measure<Curve, Point3d>(aspect: Measure.Centroid(kind: MassKind.Length)),
            context: Context(),
            input: [curve]);

        Assert.Multiple(() => {
            Assert.That(actual: midpoint[0], expression: Is.EqualTo(expected: new Point3d(x: 2.0, y: 0.0, z: 0.0)));
            Assert.That(actual: tangent[0], expression: Is.EqualTo(expected: new Vector3d(x: 0.6, y: 0.8, z: 0.0)));
            Assert.That(actual: curveTangent[0], expression: Is.EqualTo(expected: new Vector3d(x: 0.6, y: 0.8, z: 0.0)));
            Assert.That(actual: closest[0], expression: Is.EqualTo(expected: new Point3d(x: 2.04, y: 2.72, z: 0.0)));
            Assert.That(actual: segments, expression: Has.Length.EqualTo(expected: 2));
            Assert.That(actual: curveLength[0], expression: Is.EqualTo(expected: 5.0).Within(1e-12));
            Assert.That(actual: lengthCentroid[0], expression: Is.EqualTo(expected: new Point3d(x: 1.5, y: 2.0, z: 0.0)));
        });
    }

    [Test]
    public void ComputesCurveEvaluationAnalysis() {
        GeometryContext context = Context();
        using LineCurve curve = new(line: new Line(
            from: Point3d.Origin,
            to: new Point3d(x: 3.0, y: 4.0, z: 0.0)));
        using LineCurve concrete = new(line: new Line(
            from: Point3d.Origin,
            to: new Point3d(x: 6.0, y: 8.0, z: 0.0)));
        using NurbsCurve profiled = NurbsCurve.Create(
            periodic: false,
            degree: 3,
            points: [
                Point3d.Origin,
                new Point3d(x: 1.0, y: 3.0, z: 0.0),
                new Point3d(x: 4.0, y: 2.0, z: 0.0),
                new Point3d(x: 5.0, y: 6.0, z: 0.0),
                new Point3d(x: 9.0, y: 4.0, z: 0.0),
            ]);
        double normalizedStart = profiled.NormalizedLengthParameter(
            s: 0.0,
            t: out double startParameter,
            fractionalTolerance: context.Relative.Value) switch {
                true => startParameter,
                false => throw new AssertionException(message: "Expected normalized start parameter."),
            };
        double normalizedMiddle = profiled.NormalizedLengthParameter(
            s: 0.5,
            t: out double middleParameter,
            fractionalTolerance: context.Relative.Value) switch {
                true => middleParameter,
                false => throw new AssertionException(message: "Expected normalized middle parameter."),
            };
        double normalizedEnd = profiled.NormalizedLengthParameter(
            s: 1.0,
            t: out double endParameter,
            fractionalTolerance: context.Relative.Value) switch {
                true => endParameter,
                false => throw new AssertionException(message: "Expected normalized end parameter."),
            };

        double[] lengthError = Run(
            query: AnalysisQuery.Measure<Curve, double>(aspect: Measure.Error(kind: MassKind.Length)),
            context: context,
            input: [curve]);
        Plane[] curveFrames = Run(
            query: AnalysisQuery.Locate<Curve, Plane>(aspect: Location.FrameAt(parameter: curve.Domain.Mid)),
            context: context,
            input: [curve]);
        Vector3d[] curveCurvature = Run(
            query: AnalysisQuery.Locate<Curve, Vector3d>(aspect: Location.CurvatureAt(parameter: curve.Domain.Mid)),
            context: context,
            input: [curve]);
        Vector3d[] curvatureProfile = Run(
            query: AnalysisQuery.Locate<Curve, Vector3d>(aspect: Location.CurvatureProfile(count: 3)),
            context: context,
            input: [curve]);
        Vector3d[] normalizedProfile = Run(
            query: AnalysisQuery.Locate<Curve, Vector3d>(aspect: Location.CurvatureProfile(count: 3)),
            context: context,
            input: [profiled]);
        CurvatureProfile[] curvatureSummary = Run(
            query: AnalysisQuery.Locate<Curve, CurvatureProfile>(aspect: Location.CurvatureProfile(count: 3)),
            context: context,
            input: [profiled]);
        Point3d[] concretePoint = Run(
            query: AnalysisQuery.Locate<LineCurve, Point3d>(aspect: Location.PointAt(parameter: concrete.Domain.Mid)),
            context: context,
            input: [concrete]);
        Plane[] concreteFrame = Run(
            query: AnalysisQuery.Locate<LineCurve, Plane>(aspect: Location.FrameAt(parameter: concrete.Domain.Mid)),
            context: context,
            input: [concrete]);
        Validation<Error, Seq<Vector3d>> invalidParameter = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.Locate<Curve, Vector3d>(aspect: Location.CurvatureAt(parameter: curve.Domain.T1 + 1.0)),
                input: [curve]);
        Validation<Error, Seq<Vector3d>> invalidProfile = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.Locate<Curve, Vector3d>(aspect: Location.CurvatureProfile(count: 0)),
                input: [curve]);

        Assert.Multiple(() => {
            Assert.That(actual: lengthError[0], expression: Is.EqualTo(expected: 0.0).Within(1e-12));
            Assert.That(actual: curveFrames[0].Origin, expression: Is.EqualTo(expected: new Point3d(x: 1.5, y: 2.0, z: 0.0)));
            Assert.That(actual: curveCurvature[0].IsTiny(), expression: Is.True);
            Assert.That(actual: curvatureProfile, expression: Has.Length.EqualTo(expected: 3));
            Assert.That(actual: curvatureProfile.All(static (Vector3d curvature) => curvature.IsValid), expression: Is.True);
            Assert.That(actual: normalizedProfile[0], expression: Is.EqualTo(expected: profiled.CurvatureAt(t: normalizedStart)));
            Assert.That(actual: normalizedProfile[1], expression: Is.EqualTo(expected: profiled.CurvatureAt(t: normalizedMiddle)));
            Assert.That(actual: normalizedProfile[2], expression: Is.EqualTo(expected: profiled.CurvatureAt(t: normalizedEnd)));
            Assert.That(actual: curvatureSummary, expression: Has.Length.EqualTo(expected: 1));
            Assert.That(actual: curvatureSummary[0].Scalar, expression: Is.EqualTo(expected: CurvatureScalar.Magnitude));
            Assert.That(actual: curvatureSummary[0].Count, expression: Is.EqualTo(expected: 3));
            Assert.That(actual: curvatureSummary[0].Minimum, expression: Is.GreaterThanOrEqualTo(expected: 0.0));
            Assert.That(actual: concretePoint[0], expression: Is.EqualTo(expected: new Point3d(x: 3.0, y: 4.0, z: 0.0)));
            Assert.That(actual: concreteFrame[0].Origin, expression: Is.EqualTo(expected: new Point3d(x: 3.0, y: 4.0, z: 0.0)));
            Assert.That(actual: invalidParameter.ToFin().IsFail, expression: Is.True);
            Assert.That(actual: invalidProfile.ToFin().IsFail, expression: Is.True);
        });
    }

    [Test]
    public void PreservesLengthCentroidOrderAcrossManyCurves() {
        GeometryContext context = Context();
        using LineCurve first = new(line: new Line(
            from: Point3d.Origin,
            to: new Point3d(x: 2.0, y: 0.0, z: 0.0)));
        using LineCurve second = new(line: new Line(
            from: Point3d.Origin,
            to: new Point3d(x: 0.0, y: 4.0, z: 0.0)));

        Point3d[] centroids = Run(
            query: AnalysisQuery.Measure<Curve, Point3d>(aspect: Measure.Centroid(kind: MassKind.Length)),
            context: context,
            input: [first, second]);

        Assert.Multiple(() => {
            Assert.That(actual: centroids[0], expression: Is.EqualTo(expected: new Point3d(x: 1.0, y: 0.0, z: 0.0)));
            Assert.That(actual: centroids[1], expression: Is.EqualTo(expected: new Point3d(x: 0.0, y: 2.0, z: 0.0)));
        });
    }

    [Test]
    public void ComputesPolylineLocationByArcLength() {
        GeometryContext context = Context();
        Polyline polyline = new([
            Point3d.Origin,
            new Point3d(x: 10.0, y: 0.0, z: 0.0),
            new Point3d(x: 10.0, y: 2.0, z: 0.0),
        ]);

        Point3d[] midpoint = Run(
            query: AnalysisQuery.Locate<Polyline, Point3d>(aspect: Location.Midpoint),
            context: context,
            input: [polyline]);
        Vector3d[] tangent = Run(
            query: AnalysisQuery.Locate<Polyline, Vector3d>(aspect: Location.Tangent),
            context: context,
            input: [polyline]);

        Assert.Multiple(() => {
            Assert.That(actual: midpoint[0], expression: Is.EqualTo(expected: new Point3d(x: 6.0, y: 0.0, z: 0.0)));
            Assert.That(actual: tangent[0], expression: Is.EqualTo(expected: Vector3d.XAxis));
        });
    }

    [Test]
    public void ComputesSurfaceAnalysis() {
        GeometryContext context = Context();
        using PlaneSurface surface = new(
            plane: Plane.WorldXY,
            xExtents: new Interval(t0: 0.0, t1: 2.0),
            yExtents: new Interval(t0: 0.0, t1: 5.0));

        Plane[] frames = Run(
            query: AnalysisQuery.Locate<Surface, Plane>(aspect: Location.FrameAt(uv: new Point2d(x: 1.0, y: 1.0))),
            context: context,
            input: [surface]);
        Point3d[] points = Run(
            query: AnalysisQuery.Locate<Surface, Point3d>(aspect: Location.PointAt(uv: new Point2d(x: 1.0, y: 1.0))),
            context: context,
            input: [surface]);
        Vector3d[] normals = Run(
            query: AnalysisQuery.Locate<Surface, Vector3d>(aspect: Location.NormalAt(uv: new Point2d(x: 1.0, y: 1.0))),
            context: context,
            input: [surface]);
        Plane[] primitive = Run(
            query: AnalysisQuery.Primitive<Surface, Plane>(),
            context: context,
            input: [surface]);
        SurfaceCurvature[] curvature = Run(
            query: AnalysisQuery.Locate<Surface, SurfaceCurvature>(aspect: Location.CurvatureAt(uv: new Point2d(x: 1.0, y: 1.0))),
            context: context,
            input: [surface]);
        SurfaceCurvature[] curvatureProfile = Run(
            query: AnalysisQuery.Locate<Surface, SurfaceCurvature>(aspect: Location.CurvatureProfile(count: 2)),
            context: context,
            input: [surface]);
        CurvatureProfile[] curvatureSummary = Run(
            query: AnalysisQuery.Locate<Surface, CurvatureProfile>(aspect: Location.CurvatureProfile(count: 2)),
            context: context,
            input: [surface]);
        Interval[] domains = Run(
            query: AnalysisQuery.Domain<Surface, Interval>(),
            context: context,
            input: [surface]);
        Curve[] iso = Run(
            query: AnalysisQuery.Iso(iso: IsoStatus.North),
            context: context,
            input: [surface]);
        Curve[] middleIso = Run(
            query: AnalysisQuery.Iso(iso: IsoStatus.Y, normalized: 0.8),
            context: context,
            input: [surface]);
        Point3d[] concretePoints = Run(
            query: AnalysisQuery.Locate<PlaneSurface, Point3d>(aspect: Location.PointAt(uv: new Point2d(x: 1.0, y: 1.0))),
            context: context,
            input: [surface]);
        Plane[] concreteFrames = Run(
            query: AnalysisQuery.Locate<PlaneSurface, Plane>(aspect: Location.FrameAt(uv: new Point2d(x: 1.0, y: 1.0))),
            context: context,
            input: [surface]);

        Assert.Multiple(() => {
            Assert.That(actual: frames[0].Origin, expression: Is.EqualTo(expected: new Point3d(x: 1.0, y: 1.0, z: 0.0)));
            Assert.That(actual: points[0], expression: Is.EqualTo(expected: new Point3d(x: 1.0, y: 1.0, z: 0.0)));
            Assert.That(actual: normals[0], expression: Is.EqualTo(expected: Vector3d.ZAxis));
            Assert.That(actual: primitive[0].Normal, expression: Is.EqualTo(expected: Vector3d.ZAxis));
            Assert.That(actual: curvature, expression: Has.Length.EqualTo(expected: 1));
            Assert.That(actual: curvatureProfile, expression: Has.Length.EqualTo(expected: 4));
            Assert.That(actual: curvatureSummary, expression: Has.Length.EqualTo(expected: 2));
            Assert.That(actual: curvatureSummary[0].Scalar, expression: Is.EqualTo(expected: CurvatureScalar.Gaussian));
            Assert.That(actual: curvatureSummary[0].Count, expression: Is.EqualTo(expected: 4));
            Assert.That(actual: curvatureSummary[1].Scalar, expression: Is.EqualTo(expected: CurvatureScalar.Mean));
            Assert.That(actual: curvatureSummary[1].Count, expression: Is.EqualTo(expected: 4));
            Assert.That(actual: domains, expression: Has.Length.EqualTo(expected: 2));
            Assert.That(actual: iso, expression: Has.Length.EqualTo(expected: 1));
            Assert.That(actual: middleIso, expression: Has.Length.EqualTo(expected: 1));
            Assert.That(actual: concretePoints[0], expression: Is.EqualTo(expected: new Point3d(x: 1.0, y: 1.0, z: 0.0)));
            Assert.That(actual: concreteFrames[0].Origin, expression: Is.EqualTo(expected: new Point3d(x: 1.0, y: 1.0, z: 0.0)));
        });
    }

    [Test]
    public void ComputesBrepMassAndEdges() {
        GeometryContext context = Context();
        using Brep brep = new Sphere(center: Point3d.Origin, radius: 1.0).ToBrep();
        using Surface surface = new Sphere(center: Point3d.Origin, radius: 1.0).ToNurbsSurface();

        double[] volume = Run(
            query: AnalysisQuery.Measure<GeometryBase, double>(aspect: Measure.Volume),
            context: context,
            input: [brep]);
        double[] concreteVolume = Run(
            query: AnalysisQuery.Measure<Brep, double>(aspect: Measure.Volume),
            context: context,
            input: [brep]);
        double[] area = Run(
            query: AnalysisQuery.Measure<GeometryBase, double>(aspect: Measure.Area),
            context: context,
            input: [brep]);
        double[] concreteArea = Run(
            query: AnalysisQuery.Measure<Brep, double>(aspect: Measure.Area),
            context: context,
            input: [brep]);
        double[] surfaceVolume = Run(
            query: AnalysisQuery.Measure<Surface, double>(aspect: Measure.Volume),
            context: context,
            input: [surface]);
        double[] volumeError = Run(
            query: AnalysisQuery.Measure<GeometryBase, double>(aspect: Measure.Error(kind: MassKind.Volume)),
            context: context,
            input: [brep]);
        Point3d[] centroid = Run(
            query: AnalysisQuery.Measure<GeometryBase, Point3d>(aspect: Measure.Centroid(kind: MassKind.Volume)),
            context: context,
            input: [brep]);
        Point3d[] areaCentroid = Run(
            query: AnalysisQuery.Measure<GeometryBase, Point3d>(aspect: Measure.Centroid(kind: MassKind.Area)),
            context: context,
            input: [brep]);
        Curve[] edges = Run(
            query: AnalysisQuery.Edges,
            context: context,
            input: [brep]);

        Assert.Multiple(() => {
            Assert.That(actual: volume[0], expression: Is.EqualTo(expected: 4.0 * Math.PI / 3.0).Within(1.0e-9));
            Assert.That(actual: concreteVolume[0], expression: Is.EqualTo(expected: 4.0 * Math.PI / 3.0).Within(1.0e-9));
            Assert.That(actual: area[0], expression: Is.EqualTo(expected: 4.0 * Math.PI).Within(1.0e-9));
            Assert.That(actual: concreteArea[0], expression: Is.EqualTo(expected: 4.0 * Math.PI).Within(1.0e-9));
            Assert.That(actual: surfaceVolume[0], expression: Is.EqualTo(expected: 4.0 * Math.PI / 3.0).Within(1.0e-9));
            Assert.That(actual: volumeError[0], expression: Is.GreaterThanOrEqualTo(expected: 0.0));
            Assert.That(actual: centroid[0].DistanceTo(other: Point3d.Origin), expression: Is.LessThan(expected: 1.0e-9));
            Assert.That(actual: areaCentroid[0].DistanceTo(other: Point3d.Origin), expression: Is.LessThan(expected: 1.0e-9));
            Assert.That(actual: edges, expression: Is.Not.Empty);
        });
    }

    [Test]
    public void ComputesMassMomentProjections() {
        GeometryContext context = Context();
        using LineCurve curve = new(line: new Line(
            from: Point3d.Origin,
            to: new Point3d(x: 3.0, y: 4.0, z: 0.0)));
        using Brep brep = new Sphere(center: Point3d.Origin, radius: 1.0).ToBrep();

        Vector3d[] lengthRadii = Run(
            query: AnalysisQuery.Measure<Curve, Vector3d>(aspect: Measure.Radii(kind: MassKind.Length)),
            context: context,
            input: [curve]);
        (double Moment, Vector3d Axis)[] lengthPrincipal = Run(
            query: AnalysisQuery.Measure<Curve, (double Moment, Vector3d Axis)>(aspect: Measure.Principal(kind: MassKind.Length)),
            context: context,
            input: [curve]);
        Vector3d[] areaRadii = Run(
            query: AnalysisQuery.Measure<GeometryBase, Vector3d>(aspect: Measure.Radii(kind: MassKind.Area)),
            context: context,
            input: [brep]);
        (double Moment, Vector3d Axis)[] volumePrincipal = Run(
            query: AnalysisQuery.Measure<GeometryBase, (double Moment, Vector3d Axis)>(aspect: Measure.Principal(kind: MassKind.Volume)),
            context: context,
            input: [brep]);

        Assert.Multiple(() => {
            Assert.That(actual: lengthRadii[0].IsValid, expression: Is.True);
            Assert.That(actual: lengthPrincipal, expression: Has.Length.EqualTo(expected: 3));
            Assert.That(actual: lengthPrincipal.All(static ((double Moment, Vector3d Axis) principal) => principal.Axis.IsValid), expression: Is.True);
            Assert.That(actual: areaRadii[0].IsValid, expression: Is.True);
            Assert.That(actual: volumePrincipal, expression: Has.Length.EqualTo(expected: 3));
        });
    }

    [Test]
    public void ComputesBrepTopologyAnalysis() {
        GeometryContext context = Context();
        using Brep brep = new Sphere(center: Point3d.Origin, radius: 1.0).ToBrep();

        Curve[] nakedEdges = Run(
            query: AnalysisQuery.NakedEdges<Brep, Curve>(),
            context: context,
            input: [brep]);
        Point3d[] vertices = Run(
            query: AnalysisQuery.Vertices<Brep, Point3d>(),
            context: context,
            input: [brep]);
        BrepSolidOrientation[] orientation = Run(
            query: AnalysisQuery.SolidOrientation<Brep, BrepSolidOrientation>(),
            context: context,
            input: [brep]);
        bool[] contains = Run(
            query: AnalysisQuery.IsPointInside<Brep>(point: Point3d.Origin),
            context: context,
            input: [brep]);

        Assert.Multiple(() => {
            Assert.That(actual: nakedEdges, expression: Is.Empty);
            Assert.That(actual: vertices, expression: Is.Not.Empty);
            Assert.That(actual: orientation[0], expression: Is.EqualTo(expected: BrepSolidOrientation.Outward));
            Assert.That(actual: contains[0], expression: Is.True);
        });
    }

    [Test]
    public void RequiresContextForMassQueries() {
        using Brep brep = new Sphere(center: Point3d.Origin, radius: 1.0).ToBrep();

        Validation<Error, Seq<double>> result = Analyze.Run(
            query: AnalysisQuery.Measure<GeometryBase, double>(aspect: Measure.Volume),
            input: [brep]);

        Assert.That(actual: result.ToFin().IsFail, expression: Is.True);
    }

    [Test]
    public void RejectsNullGeometryInsideScopedMassRail() {
        GeometryContext context = Context();

        Validation<Error, Seq<double>> result = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.Measure<Curve, double>(aspect: Measure.Length),
                input: [null!]);

        Assert.That(actual: result.ToFin().IsFail, expression: Is.True);
    }

    [Test]
    public void RejectsOpenSurfaceForVolumeMass() {
        GeometryContext context = Context();
        using PlaneSurface surface = new(
            plane: Plane.WorldXY,
            xExtents: new Interval(t0: 0.0, t1: 1.0),
            yExtents: new Interval(t0: 0.0, t1: 1.0));

        Validation<Error, Seq<double>> result = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.Measure<Surface, double>(aspect: Measure.Volume),
                input: [surface]);

        Assert.That(actual: result.ToFin().IsFail, expression: Is.True);
    }

    [Test]
    public void RejectsInvalidCurveAreaAndContainmentInputs() {
        GeometryContext context = Context();
        using LineCurve open = new(line: new Line(
            from: Point3d.Origin,
            to: new Point3d(x: 1.0, y: 0.0, z: 0.0)));
        using PolylineCurve nonPlanarClosed = new(new Polyline([
            Point3d.Origin,
            new Point3d(x: 1.0, y: 0.0, z: 0.0),
            new Point3d(x: 1.0, y: 1.0, z: 1.0),
            new Point3d(x: 0.0, y: 1.0, z: 0.0),
            Point3d.Origin,
        ]));

        Validation<Error, Seq<double>> openArea = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.Measure<Curve, double>(aspect: Measure.Area),
                input: [open]);
        Validation<Error, Seq<double>> nonPlanarArea = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.Measure<Curve, double>(aspect: Measure.Area),
                input: [nonPlanarClosed]);
        Validation<Error, Seq<PointContainment>> openContainment = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.Locate<Curve, PointContainment>(aspect: Location.Contains(point: Point3d.Origin, plane: Plane.WorldXY)),
                input: [open]);

        Assert.Multiple(() => {
            Assert.That(actual: openArea.ToFin().IsFail, expression: Is.True);
            Assert.That(actual: nonPlanarArea.ToFin().IsFail, expression: Is.True);
            Assert.That(actual: openContainment.ToFin().IsFail, expression: Is.True);
        });
    }

    [Test]
    public void ExtractsOpenMeshNakedEdges() {
        GeometryContext context = Context();
        using Mesh mesh = Mesh.CreateFromPlane(
            plane: Plane.WorldXY,
            xInterval: new Interval(t0: 0.0, t1: 1.0),
            yInterval: new Interval(t0: 0.0, t1: 1.0),
            xCount: 1,
            yCount: 1);

        Polyline[] nakedEdges = Run(
            query: AnalysisQuery.NakedEdges<Mesh, Polyline>(),
            context: context,
            input: [mesh]);
        MeshPoint[] meshPoints = Run(
            query: AnalysisQuery.Locate<Mesh, MeshPoint>(aspect: Location.Closest(point: new Point3d(x: 0.5, y: 0.5, z: 1.0))),
            context: context,
            input: [mesh]);
        Vector3d[] normals = Run(
            query: AnalysisQuery.Locate<Mesh, Vector3d>(aspect: Location.Closest(point: new Point3d(x: 0.5, y: 0.5, z: 1.0))),
            context: context,
            input: [mesh]);
        bool[] manifold = Run(
            query: AnalysisQuery.IsManifold,
            context: context,
            input: [mesh]);
        bool[] nakedPointStatus = Run(
            query: AnalysisQuery.NakedPointStatus,
            context: context,
            input: [mesh]);
        MeshCheckParameters[] meshCheck = Run(
            query: AnalysisQuery.MeshCheck,
            context: context,
            input: [mesh]);
        Polyline[] selfIntersections = Run(
            query: AnalysisQuery.SelfIntersections,
            context: context,
            input: [mesh]);
        Point3d[] vertices = Run(
            query: AnalysisQuery.Vertices<Mesh, Point3d>(),
            context: context,
            input: [mesh]);

        Assert.Multiple(() => {
            Assert.That(actual: nakedEdges, expression: Is.Not.Empty);
            Assert.That(actual: meshPoints[0].Point, expression: Is.EqualTo(expected: new Point3d(x: 0.5, y: 0.5, z: 0.0)));
            Assert.That(actual: normals[0], expression: Is.EqualTo(expected: Vector3d.ZAxis));
            Assert.That(actual: manifold[0], expression: Is.False);
            Assert.That(actual: nakedPointStatus, expression: Has.Length.GreaterThan(expected: 0));
            Assert.That(actual: nakedPointStatus.Any(static (bool status) => status), expression: Is.True);
            Assert.That(actual: meshCheck[0].NakedEdgeCount, expression: Is.GreaterThan(expected: 0));
            Assert.That(actual: selfIntersections, expression: Is.Empty);
            Assert.That(actual: vertices, expression: Has.Length.GreaterThanOrEqualTo(expected: 4));
        });
    }

    [Test]
    public void ComputesPrimitiveMeshAndComponentAdditions() {
        GeometryContext context = Context();
        using Curve circleCurve = new Circle(plane: Plane.WorldXY, radius: 2.0).ToNurbsCurve();
        using Curve arcCurve = new Arc(plane: Plane.WorldXY, radius: 3.0, angleRadians: Math.PI / 2.0).ToNurbsCurve();
        using Curve ellipseCurve = new Ellipse(plane: Plane.WorldXY, radius1: 3.0, radius2: 1.5).ToNurbsCurve();
        using Curve polylineCurve = new Polyline([
            Point3d.Origin,
            new Point3d(x: 1.0, y: 0.0, z: 0.0),
            new Point3d(x: 1.0, y: 1.0, z: 0.0),
        ]).ToPolylineCurve();
        using Surface sphereSurface = new Sphere(center: Point3d.Origin, radius: 2.0).ToNurbsSurface();
        using Surface cylinderSurface = new Cylinder(baseCircle: new Circle(plane: Plane.WorldXY, radius: 2.0), height: 4.0).ToNurbsSurface();
        using Surface coneSurface = new Cone(plane: Plane.WorldXY, height: 4.0, radius: 2.0).ToNurbsSurface();
        using Surface torusSurface = new Torus(Plane.WorldXY, majorRadius: 3.0, minorRadius: 0.5).ToNurbsSurface();
        using Mesh mesh = Mesh.CreateFromSphere(
            sphere: new Sphere(center: Point3d.Origin, radius: 1.0),
            xCount: 16,
            yCount: 16);
        using Mesh crossing = new();
        _ = crossing.Vertices.Add(x: -1.0, y: -1.0, z: 0.0);
        _ = crossing.Vertices.Add(x: 1.0, y: -1.0, z: 0.0);
        _ = crossing.Vertices.Add(x: 0.0, y: 1.0, z: 0.0);
        _ = crossing.Vertices.Add(x: 0.0, y: -0.5, z: -1.0);
        _ = crossing.Vertices.Add(x: 0.0, y: -0.5, z: 1.0);
        _ = crossing.Vertices.Add(x: 0.0, y: 0.75, z: 0.0);
        _ = crossing.Faces.AddFace(vertex1: 0, vertex2: 1, vertex3: 2);
        _ = crossing.Faces.AddFace(vertex1: 3, vertex2: 4, vertex3: 5);
        _ = crossing.Normals.ComputeNormals();
        _ = crossing.Compact();

        Circle[] circles = Run(
            query: AnalysisQuery.Primitive<Curve, Circle>(),
            context: context,
            input: [circleCurve]);
        Arc[] arcs = Run(
            query: AnalysisQuery.Primitive<Curve, Arc>(),
            context: context,
            input: [arcCurve]);
        Ellipse[] ellipses = Run(
            query: AnalysisQuery.Primitive<Curve, Ellipse>(),
            context: context,
            input: [ellipseCurve]);
        Polyline[] polylines = Run(
            query: AnalysisQuery.Primitive<Curve, Polyline>(),
            context: context,
            input: [polylineCurve]);
        Sphere[] spheres = Run(
            query: AnalysisQuery.Primitive<Surface, Sphere>(),
            context: context,
            input: [sphereSurface]);
        Cylinder[] cylinders = Run(
            query: AnalysisQuery.Primitive<Surface, Cylinder>(),
            context: context,
            input: [cylinderSurface]);
        Cone[] cones = Run(
            query: AnalysisQuery.Primitive<Surface, Cone>(),
            context: context,
            input: [coneSurface]);
        Torus[] tori = Run(
            query: AnalysisQuery.Primitive<Surface, Torus>(),
            context: context,
            input: [torusSurface]);
        bool[] contains = Run(
            query: AnalysisQuery.IsPointInside<Mesh>(point: Point3d.Origin),
            context: context,
            input: [mesh]);
        Polyline[] sections = Run(
            query: AnalysisQuery.Intersect<Mesh, Plane, Polyline>(),
            context: context,
            input: [(mesh, Plane.WorldXY)]);
        Polyline[] selfIntersections = Run(
            query: AnalysisQuery.SelfIntersections,
            context: context,
            input: [crossing]);
        Mesh[] components = Run(
            query: AnalysisQuery.Components<Mesh, Mesh>(),
            context: context,
            input: [mesh]);
        MeshCheckParameters[] meshCheck = Run(
            query: AnalysisQuery.MeshCheck,
            context: context,
            input: [mesh]);

        Assert.Multiple(() => {
            Assert.That(actual: context.MeshIntersectionTolerance, expression: Is.EqualTo(expected: context.Absolute.Value * Intersection.MeshIntersectionsTolerancesCoefficient).Within(1e-12));
            Assert.That(actual: circles[0].Radius, expression: Is.EqualTo(expected: 2.0).Within(1e-9));
            Assert.That(actual: arcs[0].Radius, expression: Is.EqualTo(expected: 3.0).Within(1e-9));
            Assert.That(actual: ellipses[0].Radius1, expression: Is.EqualTo(expected: 3.0).Within(1e-9));
            Assert.That(actual: polylines[0], expression: Has.Count.EqualTo(expected: 3));
            Assert.That(actual: spheres[0].Radius, expression: Is.EqualTo(expected: 2.0).Within(1e-9));
            Assert.That(actual: cylinders[0].Radius, expression: Is.EqualTo(expected: 2.0).Within(1e-9));
            Assert.That(actual: cones[0].Radius, expression: Is.EqualTo(expected: 2.0).Within(1e-9));
            Assert.That(actual: tori[0].MajorRadius, expression: Is.EqualTo(expected: 3.0).Within(1e-9));
            Assert.That(actual: contains[0], expression: Is.True);
            Assert.That(actual: sections, expression: Is.Not.Empty);
            Assert.That(actual: selfIntersections, expression: Is.Not.Empty);
            Assert.That(actual: components, expression: Is.Not.Empty);
            Assert.That(actual: meshCheck[0].NakedEdgeCount, expression: Is.EqualTo(expected: 0));
        });
    }

    [Test]
    public void ComputesBounds() {
        GeometryContext context = Context();
        using LineCurve first = new(line: new Line(
            from: new Point3d(x: -1.0, y: 0.0, z: 0.0),
            to: new Point3d(x: 1.0, y: 0.0, z: 0.0)));

        BoundingBox[] transformed = Run(
            query: AnalysisQuery.Bounds<Curve, BoundingBox>(
                aspect: Bounds.Transformed(transform: Transform.Translation(dx: 1.0, dy: 2.0, dz: 0.0))),
            context: context,
            input: [first]);
        Box[] oriented = Run(
            query: AnalysisQuery.Bounds<Curve, Box>(aspect: Bounds.Oriented(plane: Plane.WorldXY)),
            context: context,
            input: [first]);

        Assert.Multiple(() => {
            Assert.That(actual: transformed[0].Center, expression: Is.EqualTo(expected: new Point3d(x: 1.0, y: 2.0, z: 0.0)));
            Assert.That(actual: oriented[0].IsValid, expression: Is.True);
        });
    }

    [Test]
    public void ComputesIntersections() {
        GeometryContext context = Context();
        using LineCurve first = new(line: new Line(
            from: new Point3d(x: -1.0, y: 0.0, z: 0.0),
            to: new Point3d(x: 1.0, y: 0.0, z: 0.0)));
        using LineCurve second = new(line: new Line(
            from: new Point3d(x: 0.0, y: -1.0, z: 0.0),
            to: new Point3d(x: 0.0, y: 1.0, z: 0.0)));
        using LineCurve vertical = new(line: new Line(
            from: new Point3d(x: 0.0, y: 0.0, z: -1.0),
            to: new Point3d(x: 0.0, y: 0.0, z: 1.0)));
        using LineCurve overlapA = new(line: new Line(
            from: new Point3d(x: -1.0, y: 2.0, z: 0.0),
            to: new Point3d(x: 1.0, y: 2.0, z: 0.0)));
        using LineCurve overlapB = new(line: new Line(
            from: new Point3d(x: 0.0, y: 2.0, z: 0.0),
            to: new Point3d(x: 2.0, y: 2.0, z: 0.0)));
        using Brep sphere = new Sphere(center: Point3d.Origin, radius: 1.0).ToBrep();
        using PlaneSurface surface = new(
            plane: Plane.WorldXY,
            xExtents: new Interval(t0: -2.0, t1: 2.0),
            yExtents: new Interval(t0: -2.0, t1: 2.0));
        using Mesh mesh = Mesh.CreateFromSphere(
            sphere: new Sphere(center: Point3d.Origin, radius: 1.0),
            xCount: 16,
            yCount: 16);
        using Mesh shiftedMesh = Mesh.CreateFromSphere(
            sphere: new Sphere(center: new Point3d(x: 0.5, y: 0.0, z: 0.0), radius: 1.0),
            xCount: 16,
            yCount: 16);
        IntersectionEvent[] intersections = Run(
            query: AnalysisQuery.Intersect<Curve, Curve, IntersectionEvent>(),
            context: context,
            input: [(first, second)]);
        IntersectionKind[] pointClassifications = Run(
            query: AnalysisQuery.Intersect<Curve, Curve, IntersectionKind>(),
            context: context,
            input: [(first, second)]);
        IntersectionKind[] overlapClassifications = Run(
            query: AnalysisQuery.Intersect<Curve, Curve, IntersectionKind>(),
            context: context,
            input: [(overlapA, overlapB)]);
        Point3d[] curveLinePoints = Run(
            query: AnalysisQuery.Intersect<Curve, Line, Point3d>(),
            context: context,
            input: [(first, new Line(from: new Point3d(x: 0.0, y: -1.0, z: 0.0), to: new Point3d(x: 0.0, y: 1.0, z: 0.0)))]);
        Point3d[] curvePlanePoints = Run(
            query: AnalysisQuery.Intersect<Curve, Plane, Point3d>(),
            context: context,
            input: [(vertical, Plane.WorldXY)]);
        Point3d[] sectionPoints = Run(
            query: AnalysisQuery.Intersect<Brep, Plane, Point3d>(),
            context: context,
            input: [(sphere, Plane.WorldXY)]);
        Curve[] brepSurfaceCurves = Run(
            query: AnalysisQuery.Intersect<Brep, Surface, Curve>(),
            context: context,
            input: [(sphere, surface)]);
        Point3d[] meshLinePoints = Run(
            query: AnalysisQuery.Intersect<Mesh, Line, Point3d>(),
            context: context,
            input: [(mesh, new Line(from: new Point3d(x: -2.0, y: 0.0, z: 0.0), to: new Point3d(x: 2.0, y: 0.0, z: 0.0)))]);
        Polyline[] meshMeshCurves = Run(
            query: AnalysisQuery.Intersect<Mesh, Mesh, Polyline>(),
            context: context,
            input: [(mesh, shiftedMesh)]);
        IntersectionKind[] meshPlaneClassifications = Run(
            query: AnalysisQuery.Intersect<Mesh, Plane, IntersectionKind>(),
            context: context,
            input: [(mesh, Plane.WorldXY)]);
        IntersectionKind[] meshMeshClassifications = Run(
            query: AnalysisQuery.Intersect<Mesh, Mesh, IntersectionKind>(),
            context: context,
            input: [(mesh, shiftedMesh)]);
        IntersectionEvent[] genericIntersections = Run(
            query: AnalysisQuery.Intersect<LineCurve, LineCurve, IntersectionEvent>(),
            context: context,
            input: [(first, second)]);
        Point3d[] genericCurvePlanePoints = Run(
            query: AnalysisQuery.Intersect<LineCurve, Plane, Point3d>(),
            context: context,
            input: [(vertical, Plane.WorldXY)]);
        Point3d[] genericMeshLinePoints = Run(
            query: AnalysisQuery.Intersect<Mesh, Line, Point3d>(),
            context: context,
            input: [(mesh, new Line(from: new Point3d(x: -2.0, y: 0.0, z: 0.0), to: new Point3d(x: 2.0, y: 0.0, z: 0.0)))]);

        Assert.Multiple(() => {
            Assert.That(actual: intersections, expression: Has.Length.EqualTo(expected: 1));
            Assert.That(actual: intersections[0].PointA, expression: Is.EqualTo(expected: Point3d.Origin));
            Assert.That(actual: pointClassifications, expression: Is.EqualTo(expected: new[] { IntersectionKind.Point }));
            Assert.That(actual: overlapClassifications, expression: Is.EqualTo(expected: new[] { IntersectionKind.Overlap }));
            Assert.That(actual: curveLinePoints[0], expression: Is.EqualTo(expected: Point3d.Origin));
            Assert.That(actual: curvePlanePoints[0], expression: Is.EqualTo(expected: Point3d.Origin));
            Assert.That(actual: sectionPoints, expression: Is.Not.Null);
            Assert.That(actual: brepSurfaceCurves, expression: Is.Not.Empty);
            Assert.That(actual: meshLinePoints, expression: Has.Length.GreaterThanOrEqualTo(expected: 2));
            Assert.That(actual: meshMeshCurves, expression: Is.Not.Empty);
            Assert.That(actual: meshPlaneClassifications, expression: Has.All.EqualTo(expected: IntersectionKind.Overlap));
            Assert.That(actual: meshMeshClassifications, expression: Has.All.EqualTo(expected: IntersectionKind.Overlap));
            Assert.That(actual: genericIntersections, expression: Has.Length.EqualTo(expected: 1));
            Assert.That(actual: genericCurvePlanePoints[0], expression: Is.EqualTo(expected: Point3d.Origin));
            Assert.That(actual: genericMeshLinePoints, expression: Has.Length.GreaterThanOrEqualTo(expected: 2));
        });
    }

    [Test]
    public void RejectsInvalidNativeIntersectionOperandsBeforeDispatch() {
        GeometryContext context = Context();
        using LineCurve curve = new(line: new Line(
            from: new Point3d(x: -1.0, y: 0.0, z: 0.0),
            to: new Point3d(x: 1.0, y: 0.0, z: 0.0)));
        using Mesh mesh = Mesh.CreateFromSphere(
            sphere: new Sphere(center: Point3d.Origin, radius: 1.0),
            xCount: 16,
            yCount: 16);

        Validation<Error, Seq<Point3d>> invalidPlane = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.Intersect<Curve, Plane, Point3d>(),
                input: [(curve, Plane.Unset)]);
        Validation<Error, Seq<Point3d>> invalidLine = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.Intersect<Mesh, Line, Point3d>(),
                input: [(mesh, Line.Unset)]);
        Validation<Error, Seq<Point3d>> invalidGenericPlane = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.Intersect<LineCurve, Plane, Point3d>(),
                input: [(curve, Plane.Unset)]);
        Validation<Error, Seq<Point3d>> invalidGenericLine = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.Intersect<LineCurve, Line, Point3d>(),
                input: [(curve, Line.Unset)]);

        Assert.Multiple(() => {
            Assert.That(actual: invalidPlane.ToFin().IsFail, expression: Is.True);
            Assert.That(actual: invalidLine.ToFin().IsFail, expression: Is.True);
            Assert.That(actual: invalidGenericPlane.ToFin().IsFail, expression: Is.True);
            Assert.That(actual: invalidGenericLine.ToFin().IsFail, expression: Is.True);
        });
    }

    [Test]
    public void RejectsOpenBrepForSolidContainment() {
        GeometryContext context = Context();
        using PlaneSurface surface = new(
            plane: Plane.WorldXY,
            xExtents: new Interval(t0: 0.0, t1: 1.0),
            yExtents: new Interval(t0: 0.0, t1: 1.0));
        using Brep openBrep = surface.ToBrep();

        Validation<Error, Seq<bool>> result = Analyze.In(context: context)
            .Run(
                query: AnalysisQuery.IsPointInside<Brep>(point: Point3d.Origin),
                input: [openBrep]);

        Assert.That(actual: result.ToFin().IsFail, expression: Is.True);
    }

    private static GeometryContext Context() =>
        GeometryContext.FromDocument(doc: Optional(RhinoDoc.ActiveDoc)
                .ToFin(Error.New(message: "Rhino.Testing did not create an active Rhino document."))
                .Match(
                    Succ: static (RhinoDoc candidate) => candidate,
                    Fail: static (Error fault) => throw new AssertionException(message: fault.Message)))
            .ToFin()
            .Match(
                Succ: static (GeometryContext context) => context,
                Fail: static (Error fault) => throw new AssertionException(message: fault.Message));

    private static TOut[] Run<TGeometry, TOut>(
        global::Analysis.Query<TGeometry, TOut> query,
        GeometryContext context,
        params ReadOnlySpan<TGeometry> input) where TGeometry : notnull =>
        Analyze.In(context: context)
            .Run(
                query: query,
                input: input)
            .ToFin()
            .Match(
                Succ: static (Seq<TOut> output) => output.ToArray(),
                Fail: static (Error fault) => throw new AssertionException(message: fault.Message));
}

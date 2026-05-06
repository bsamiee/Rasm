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
            query: AnalysisQuery.Midpoint<Polyline, Point3d>(),
            context: Context(),
            input: [polyline]);
        Vector3d[] tangent = Run(
            query: AnalysisQuery.Tangent<Line, Vector3d>(),
            context: Context(),
            input: [line]);
        Vector3d[] curveTangent = Run(
            query: AnalysisQuery.Tangent<Curve, Vector3d>(),
            context: Context(),
            input: [curve]);
        Point3d[] closest = Run(
            query: AnalysisQuery.Closest<Line, Point3d>(point: new Point3d(x: 2.0, y: 3.0, z: 0.0)),
            context: Context(),
            input: [line]);
        Line[] segments = Run(
            query: AnalysisQuery.Segments<Polyline, Line>(),
            context: Context(),
            input: [polyline]);
        double[] curveLength = Run(
            query: AnalysisQuery.Length<Curve, double>(),
            context: Context(),
            input: [curve]);
        Point3d[] lengthCentroid = Run(
            query: AnalysisQuery.LengthCentroid,
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

        double[] lengthError = Run(
            query: AnalysisQuery.LengthError,
            context: context,
            input: [curve]);
        Plane[] curveFrames = Run(
            query: AnalysisQuery.FrameAt(parameter: curve.Domain.Mid),
            context: context,
            input: [curve]);
        Vector3d[] curveCurvature = Run(
            query: AnalysisQuery.CurvatureAt(parameter: curve.Domain.Mid),
            context: context,
            input: [curve]);

        Assert.Multiple(() => {
            Assert.That(actual: lengthError[0], expression: Is.EqualTo(expected: 0.0).Within(1e-12));
            Assert.That(actual: curveFrames[0].Origin, expression: Is.EqualTo(expected: new Point3d(x: 1.5, y: 2.0, z: 0.0)));
            Assert.That(actual: curveCurvature[0].IsTiny(), expression: Is.True);
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
            query: AnalysisQuery.LengthCentroid,
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
            query: AnalysisQuery.Midpoint<Polyline, Point3d>(),
            context: context,
            input: [polyline]);
        Vector3d[] tangent = Run(
            query: AnalysisQuery.Tangent<Polyline, Vector3d>(),
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
            query: AnalysisQuery.FrameAt(uv: new Point2d(x: 1.0, y: 1.0)),
            context: context,
            input: [surface]);
        Point3d[] points = Run(
            query: AnalysisQuery.PointAt(uv: new Point2d(x: 1.0, y: 1.0)),
            context: context,
            input: [surface]);
        Vector3d[] normals = Run(
            query: AnalysisQuery.NormalAt(uv: new Point2d(x: 1.0, y: 1.0)),
            context: context,
            input: [surface]);
        Plane[] primitive = Run(
            query: AnalysisQuery.Primitive<Surface, Plane>(),
            context: context,
            input: [surface]);
        SurfaceCurvature[] curvature = Run(
            query: AnalysisQuery.CurvatureAt(uv: new Point2d(x: 1.0, y: 1.0)),
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

        Assert.Multiple(() => {
            Assert.That(actual: frames[0].Origin, expression: Is.EqualTo(expected: new Point3d(x: 1.0, y: 1.0, z: 0.0)));
            Assert.That(actual: points[0], expression: Is.EqualTo(expected: new Point3d(x: 1.0, y: 1.0, z: 0.0)));
            Assert.That(actual: normals[0], expression: Is.EqualTo(expected: Vector3d.ZAxis));
            Assert.That(actual: primitive[0].Normal, expression: Is.EqualTo(expected: Vector3d.ZAxis));
            Assert.That(actual: curvature, expression: Has.Length.EqualTo(expected: 1));
            Assert.That(actual: domains, expression: Has.Length.EqualTo(expected: 2));
            Assert.That(actual: iso, expression: Has.Length.EqualTo(expected: 1));
            Assert.That(actual: middleIso, expression: Has.Length.EqualTo(expected: 1));
        });
    }

    [Test]
    public void ComputesBrepMassAndEdges() {
        GeometryContext context = Context();
        using Brep brep = new Sphere(center: Point3d.Origin, radius: 1.0).ToBrep();
        using Surface surface = new Sphere(center: Point3d.Origin, radius: 1.0).ToNurbsSurface();

        double[] volume = Run(
            query: AnalysisQuery.Volume,
            context: context,
            input: [brep]);
        double[] area = Run(
            query: AnalysisQuery.Area,
            context: context,
            input: [brep]);
        double[] surfaceVolume = Run(
            query: AnalysisQuery.Volume,
            context: context,
            input: [surface]);
        double[] volumeError = Run(
            query: AnalysisQuery.VolumeError,
            context: context,
            input: [brep]);
        Point3d[] centroid = Run(
            query: AnalysisQuery.VolumeCentroid,
            context: context,
            input: [brep]);
        Point3d[] areaCentroid = Run(
            query: AnalysisQuery.AreaCentroid,
            context: context,
            input: [brep]);
        Curve[] edges = Run(
            query: AnalysisQuery.Edges,
            context: context,
            input: [brep]);

        Assert.Multiple(() => {
            Assert.That(actual: volume[0], expression: Is.GreaterThan(expected: 4.0));
            Assert.That(actual: area[0], expression: Is.GreaterThan(expected: 12.0));
            Assert.That(actual: surfaceVolume[0], expression: Is.GreaterThan(expected: 4.0));
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
            query: AnalysisQuery.LengthRadii,
            context: context,
            input: [curve]);
        (double Moment, Vector3d Axis)[] lengthPrincipal = Run(
            query: AnalysisQuery.LengthPrincipal,
            context: context,
            input: [curve]);
        Vector3d[] areaRadii = Run(
            query: AnalysisQuery.AreaRadii,
            context: context,
            input: [brep]);
        (double Moment, Vector3d Axis)[] volumePrincipal = Run(
            query: AnalysisQuery.VolumePrincipal,
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
            query: AnalysisQuery.Volume,
            input: [brep]);

        Assert.That(actual: result.ToFin().IsFail, expression: Is.True);
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
            query: AnalysisQuery.Closest<Mesh, MeshPoint>(point: new Point3d(x: 0.5, y: 0.5, z: 1.0)),
            context: context,
            input: [mesh]);
        Vector3d[] normals = Run(
            query: AnalysisQuery.Closest<Mesh, Vector3d>(point: new Point3d(x: 0.5, y: 0.5, z: 1.0)),
            context: context,
            input: [mesh]);
        bool[] manifold = Run(
            query: AnalysisQuery.IsManifold,
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
            Assert.That(actual: vertices, expression: Has.Length.GreaterThanOrEqualTo(expected: 4));
        });
    }

    [Test]
    public void ComputesPrimitiveMeshAndComponentAdditions() {
        GeometryContext context = Context();
        using Curve circleCurve = new Circle(plane: Plane.WorldXY, radius: 2.0).ToNurbsCurve();
        using Mesh mesh = Mesh.CreateFromSphere(
            sphere: new Sphere(center: Point3d.Origin, radius: 1.0),
            xCount: 16,
            yCount: 16);
        using Mesh shiftedMesh = Mesh.CreateFromSphere(
            sphere: new Sphere(center: new Point3d(x: 0.5, y: 0.0, z: 0.0), radius: 1.0),
            xCount: 16,
            yCount: 16);

        Circle[] circles = Run(
            query: AnalysisQuery.Primitive<Curve, Circle>(),
            context: context,
            input: [circleCurve]);
        bool[] contains = Run(
            query: AnalysisQuery.IsPointInside<Mesh>(point: Point3d.Origin),
            context: context,
            input: [mesh]);
        Polyline[] sections = Run(
            query: AnalysisQuery.Intersect<(Mesh Mesh, Plane Plane), Polyline>(),
            context: context,
            input: [(mesh, Plane.WorldXY)]);
        Mesh[] components = Run(
            query: AnalysisQuery.Components<Mesh, Mesh>(),
            context: context,
            input: [mesh]);

        Assert.Multiple(() => {
            Assert.That(actual: circles[0].Radius, expression: Is.EqualTo(expected: 2.0).Within(1e-9));
            Assert.That(actual: contains[0], expression: Is.True);
            Assert.That(actual: sections, expression: Is.Not.Empty);
            Assert.That(actual: components, expression: Is.Not.Empty);
        });
    }

    [Test]
    public void ComputesBoundsAndIntersections() {
        GeometryContext context = Context();
        using LineCurve first = new(line: new Line(
            from: new Point3d(x: -1.0, y: 0.0, z: 0.0),
            to: new Point3d(x: 1.0, y: 0.0, z: 0.0)));
        using LineCurve second = new(line: new Line(
            from: new Point3d(x: 0.0, y: -1.0, z: 0.0),
            to: new Point3d(x: 0.0, y: 1.0, z: 0.0)));
        using Brep sphere = new Sphere(center: Point3d.Origin, radius: 1.0).ToBrep();
        using PlaneSurface surface = new(
            plane: Plane.WorldXY,
            xExtents: new Interval(t0: -2.0, t1: 2.0),
            yExtents: new Interval(t0: -2.0, t1: 2.0));
        using Mesh mesh = Mesh.CreateFromSphere(
            sphere: new Sphere(center: Point3d.Origin, radius: 1.0),
            xCount: 16,
            yCount: 16);

        BoundingBox[] transformed = Run(
            query: AnalysisQuery.TransformedBounds<Curve, BoundingBox>(
                transform: Transform.Translation(dx: 1.0, dy: 2.0, dz: 0.0)),
            context: context,
            input: [first]);
        Box[] oriented = Run(
            query: AnalysisQuery.OrientedBounds<Curve, Box>(plane: Plane.WorldXY),
            context: context,
            input: [first]);
        IntersectionEvent[] intersections = Run(
            query: AnalysisQuery.Intersect<(Curve A, Curve B), IntersectionEvent>(),
            context: context,
            input: [(first, second)]);
        Point3d[] curveLinePoints = Run(
            query: AnalysisQuery.Intersect<(Curve Curve, Line Line), Point3d>(),
            context: context,
            input: [(first, new Line(from: new Point3d(x: 0.0, y: -1.0, z: 0.0), to: new Point3d(x: 0.0, y: 1.0, z: 0.0)))]);
        Point3d[] sectionPoints = Run(
            query: AnalysisQuery.Intersect<(Brep Brep, Plane Plane), Point3d>(),
            context: context,
            input: [(sphere, Plane.WorldXY)]);
        Curve[] brepSurfaceCurves = Run(
            query: AnalysisQuery.Intersect<(Brep Brep, Surface Surface), Curve>(),
            context: context,
            input: [(sphere, surface)]);
        Point3d[] meshLinePoints = Run(
            query: AnalysisQuery.Intersect<(Mesh Mesh, Line Line), Point3d>(),
            context: context,
            input: [(mesh, new Line(from: new Point3d(x: -2.0, y: 0.0, z: 0.0), to: new Point3d(x: 2.0, y: 0.0, z: 0.0)))]);
        Polyline[] meshMeshCurves = Run(
            query: AnalysisQuery.Intersect<(Mesh A, Mesh B), Polyline>(),
            context: context,
            input: [(mesh, shiftedMesh)]);

        Assert.Multiple(() => {
            Assert.That(actual: transformed[0].Center, expression: Is.EqualTo(expected: new Point3d(x: 1.0, y: 2.0, z: 0.0)));
            Assert.That(actual: oriented[0].IsValid, expression: Is.True);
            Assert.That(actual: intersections, expression: Has.Length.EqualTo(expected: 1));
            Assert.That(actual: intersections[0].PointA, expression: Is.EqualTo(expected: Point3d.Origin));
            Assert.That(actual: curveLinePoints[0], expression: Is.EqualTo(expected: Point3d.Origin));
            Assert.That(actual: sectionPoints, expression: Is.Not.Null);
            Assert.That(actual: brepSurfaceCurves, expression: Is.Not.Empty);
            Assert.That(actual: meshLinePoints, expression: Has.Length.GreaterThanOrEqualTo(expected: 2));
            Assert.That(actual: meshMeshCurves, expression: Is.Not.Empty);
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

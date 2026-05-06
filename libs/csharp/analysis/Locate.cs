using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Analysis;

// --- [LOCATION] --------------------------------------------------------------------------------

public static partial class Query {
    public static Query<TGeometry, TOut> Midpoint<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(query: Query<Line, Point3d>.Build(
                    key: MidpointKey,
                    evaluator: static (Line geometry, Fin<GeometryContext> _) => geometry.IsValid switch {
                        true => One(key: MidpointKey, value: geometry.PointAt(t: 0.5)),
                        false => Fin.Fail<Seq<Point3d>>(MidpointKey.InvalidInput()),
                    })),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(query: Query<Polyline, Point3d>.Build(
                    key: MidpointKey,
                    evaluator: static (Polyline geometry, Fin<GeometryContext> _) => {
                        using PolylineCurve curve = geometry.ToPolylineCurve();
                        return curve.IsValid switch {
                            true => One(key: MidpointKey, value: curve.PointAtNormalizedLength(length: 0.5)),
                            false => Fin.Fail<Seq<Point3d>>(MidpointKey.InvalidInput()),
                        };
                    })),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, Point3d>.Build(
                    key: MidpointKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Curve curve => One(key: MidpointKey, value: curve.PointAtNormalizedLength(length: 0.5)),
                        _ => Fin.Fail<Seq<Point3d>>(MidpointKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Point3d))),
                    })),
            _ => MidpointKey.Unsupported<TGeometry, TOut>(),
        };

    public static Query<BoundingBox, Point3d> BoundsCenter =>
        Query<BoundingBox, Point3d>.Build(key: BoundsCenterKey, evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => One(key: BoundsCenterKey, value: geometry.Center));

    public static Query<Curve, Point3d> LengthCentroid =>
        LengthMass(name: nameof(LengthCentroid), project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.Centroid));

    public static Query<GeometryBase, Point3d> AreaCentroid =>
        AreaMass(name: nameof(AreaCentroid), project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.Centroid));

    public static Query<GeometryBase, Point3d> VolumeCentroid =>
        VolumeMass(name: nameof(VolumeCentroid), project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.Centroid));

    public static Query<Curve, Vector3d> LengthCentroidError =>
        LengthMass(name: nameof(LengthCentroidError), project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.CentroidError));

    public static Query<GeometryBase, Vector3d> AreaCentroidError =>
        AreaMass(name: nameof(AreaCentroidError), project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.CentroidError));

    public static Query<GeometryBase, Vector3d> VolumeCentroidError =>
        VolumeMass(name: nameof(VolumeCentroidError), project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.CentroidError));

    public static Query<TGeometry, TOut> Tangent<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(query: Query<Line, Vector3d>.Build(
                    key: TangentKey,
                    evaluator: static (Line geometry, Fin<GeometryContext> _) => One(key: TangentKey, value: geometry.UnitTangent))),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(query: Query<Polyline, Vector3d>.Build(
                    key: TangentKey,
                    evaluator: static (Polyline geometry, Fin<GeometryContext> _) => {
                        using PolylineCurve curve = geometry.ToPolylineCurve();
                        return curve.NormalizedLengthParameter(
                            s: 0.5,
                            t: out double parameter) switch {
                                true => One(key: TangentKey, value: curve.TangentAt(t: parameter)),
                                false => Fin.Fail<Seq<Vector3d>>(TangentKey.InvalidResult()),
                            };
                    })),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, Vector3d>.Build(
                    key: TangentKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Curve curve => curve.NormalizedLengthParameter(
                            s: 0.5,
                            t: out double parameter) switch {
                                true => One(key: TangentKey, value: curve.TangentAt(t: parameter)),
                                false => Fin.Fail<Seq<Vector3d>>(TangentKey.InvalidResult()),
                            },
                        _ => Fin.Fail<Seq<Vector3d>>(TangentKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Vector3d))),
                    })),
            _ => TangentKey.Unsupported<TGeometry, TOut>(),
        };

    public static Query<TGeometry, TOut> Closest<TGeometry, TOut>(Point3d point) where TGeometry : notnull =>
        typeof(TOut) switch {
            Type output when output == typeof(Point3d) => ClosestPoint<TGeometry, TOut>(point: point),
            _ => ClosestDetail<TGeometry, TOut>(point: point),
        };

    private static Query<TGeometry, TOut> ClosestPoint<TGeometry, TOut>(Point3d point) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Line, Point3d>(
                    point: point,
                    project: static (Point3d target, Line geometry) =>
                        One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target, limitToFiniteSegment: true))),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Polyline, Point3d>(
                    point: point,
                    project: static (Point3d target, Polyline geometry) =>
                        One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Curve, Point3d>(
                    point: point,
                    project: static (Point3d target, Curve geometry) => geometry.ClosestPoint(testPoint: target, t: out double parameter) switch {
                        true => One(key: ClosestKey, value: geometry.PointAt(t: parameter)),
                        false => Fin.Fail<Seq<Point3d>>(ClosestKey.InvalidResult()),
                    }),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Surface, Point3d>(
                    point: point,
                    project: static (Point3d target, Surface geometry) => geometry.ClosestPoint(testPoint: target, u: out double u, v: out double v) switch {
                        true => One(key: ClosestKey, value: geometry.PointAt(u: u, v: v)),
                        false => Fin.Fail<Seq<Point3d>>(ClosestKey.InvalidResult()),
                    }),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Brep, Point3d>(
                    point: point,
                    project: static (Point3d target, Brep geometry) =>
                        One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Point3d) =>
                ClosestMatch<TGeometry, TOut, Mesh, Point3d>(
                    point: point,
                    project: static (Point3d target, Mesh geometry) =>
                        One(key: ClosestKey, value: geometry.ClosestPoint(testPoint: target))),
            _ => ClosestKey.Unsupported<TGeometry, TOut>(),
        };

    private static Query<TGeometry, TOut> ClosestDetail<TGeometry, TOut>(Point3d point) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                ClosestMatch<TGeometry, TOut, Curve, double>(
                    point: point,
                    project: static (Point3d target, Curve geometry) => geometry.ClosestPoint(testPoint: target, t: out double parameter) switch {
                        true => One(key: ClosestKey, value: parameter),
                        false => Fin.Fail<Seq<double>>(ClosestKey.InvalidResult()),
                    }),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                ClosestMatch<TGeometry, TOut, Brep, Vector3d>(
                    point: point,
                    project: static (Point3d target, Brep geometry) => geometry.ClosestPoint(
                        testPoint: target,
                        closestPoint: out Point3d _,
                        ci: out ComponentIndex _,
                        s: out double _,
                        t: out double _,
                        maximumDistance: 0.0,
                        normal: out Vector3d normal) switch {
                            true => One(key: ClosestKey, value: normal),
                            false => Fin.Fail<Seq<Vector3d>>(ClosestKey.InvalidResult()),
                        }),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(Vector3d) =>
                ClosestMatch<TGeometry, TOut, Mesh, Vector3d>(
                    point: point,
                    project: static (Point3d target, Mesh geometry) => geometry.ClosestPoint(
                        testPoint: target,
                        pointOnMesh: out Point3d _,
                        normalAtPoint: out Vector3d normal,
                        maximumDistance: 0.0) switch {
                            >= 0 => One(key: ClosestKey, value: normal),
                            _ => Fin.Fail<Seq<Vector3d>>(ClosestKey.InvalidResult()),
                        }),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(ComponentIndex) =>
                ClosestMatch<TGeometry, TOut, Brep, ComponentIndex>(
                    point: point,
                    project: static (Point3d target, Brep geometry) => geometry.ClosestPoint(
                        testPoint: target,
                        closestPoint: out Point3d _,
                        ci: out ComponentIndex component,
                        s: out double _,
                        t: out double _,
                        maximumDistance: 0.0,
                        normal: out Vector3d _) switch {
                            true => One(key: ClosestKey, value: component),
                            false => Fin.Fail<Seq<ComponentIndex>>(ClosestKey.InvalidResult()),
                        }),
            (Type geometry, Type output) when typeof(Mesh).IsAssignableFrom(c: geometry) && output == typeof(MeshPoint) =>
                ClosestMatch<TGeometry, TOut, Mesh, MeshPoint>(
                    point: point,
                    project: static (Point3d target, Mesh geometry) =>
                        Optional(geometry.ClosestMeshPoint(testPoint: target, maximumDistance: 0.0))
                            .ToFin(ClosestKey.InvalidResult())
                            .Map(static (MeshPoint meshPoint) => Seq(meshPoint))),
            _ => ClosestKey.Unsupported<TGeometry, TOut>(),
        };

    public static Query<Surface, Point3d> PointAt(Point2d uv) =>
        SurfaceUv(
            key: PointAtKey,
            uv: uv,
            project: static (Surface geometry, Point2d parameter) =>
                One(key: PointAtKey, value: geometry.PointAt(u: parameter.X, v: parameter.Y)));

    public static Query<Curve, Point3d> PointAt(double parameter) =>
        Query<Curve, Point3d>.Build(
            key: PointAtKey,
            evaluator: (Curve geometry, Fin<GeometryContext> _) =>
                geometry.Domain.IncludesParameter(t: parameter) switch {
                    true => One(key: PointAtKey, value: geometry.PointAt(t: parameter)),
                    false => Fin.Fail<Seq<Point3d>>(PointAtKey.InvalidInput()),
                });

    public static Query<Curve, Point3d> PointAtLength(double length) =>
        Query<Curve, Point3d>.Build(key: PointAtLengthKey, requirement: GeometryRequirement.CurveLength, evaluator: (Curve geometry, Fin<GeometryContext> _) => One(key: PointAtLengthKey, value: geometry.PointAtLength(length: length)));

    public static Query<Surface, Plane> FrameAt(Point2d uv) =>
        SurfaceUv(
            key: FrameAtKey,
            uv: uv,
            project: static (Surface geometry, Point2d parameter) => geometry.FrameAt(
                u: parameter.X,
                v: parameter.Y,
                frame: out Plane frame) switch {
                    true => One(key: FrameAtKey, value: frame),
                    false => Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()),
                });

    public static Query<Curve, Plane> FrameAt(double parameter) =>
        Query<Curve, Plane>.Build(
            key: FrameAtKey,
            evaluator: (Curve geometry, Fin<GeometryContext> _) =>
                geometry.FrameAt(t: parameter, plane: out Plane frame) switch {
                    true => One(key: FrameAtKey, value: frame),
                    false => Fin.Fail<Seq<Plane>>(FrameAtKey.InvalidResult()),
                });

    public static Query<Curve, Plane> PerpendicularFrameAt(double parameter) =>
        Query<Curve, Plane>.Build(
            key: PerpendicularFrameAtKey,
            evaluator: (Curve geometry, Fin<GeometryContext> _) =>
                geometry.PerpendicularFrameAt(t: parameter, plane: out Plane frame) switch {
                    true => One(key: PerpendicularFrameAtKey, value: frame),
                    false => Fin.Fail<Seq<Plane>>(PerpendicularFrameAtKey.InvalidResult()),
                });

    public static Query<Surface, Vector3d> NormalAt(Point2d uv) =>
        SurfaceUv(
            key: NormalAtKey,
            uv: uv,
            project: static (Surface geometry, Point2d parameter) =>
                geometry.NormalAt(u: parameter.X, v: parameter.Y) switch {
                    Vector3d normal when normal.IsValid && !normal.IsTiny() =>
                        One(key: NormalAtKey, value: normal),
                    _ => Fin.Fail<Seq<Vector3d>>(NormalAtKey.InvalidResult()),
                });

    public static Query<Surface, SurfaceCurvature> CurvatureAt(Point2d uv) =>
        SurfaceUv(
            key: CurvatureAtKey,
            uv: uv,
            project: static (Surface geometry, Point2d parameter) =>
                Optional(geometry.CurvatureAt(u: parameter.X, v: parameter.Y))
                    .ToFin(CurvatureAtKey.InvalidResult())
                    .Map(static (SurfaceCurvature curvature) => Seq(curvature)));

    public static Query<Curve, Vector3d> CurvatureAt(double parameter) =>
        Query<Curve, Vector3d>.Build(key: CurvatureAtKey, evaluator: (Curve geometry, Fin<GeometryContext> _) => One(key: CurvatureAtKey, value: geometry.CurvatureAt(t: parameter)));

    public static Query<Curve, Vector3d> DerivativeAt(double parameter, int count) =>
        Query<Curve, Vector3d>.Build(key: DerivativeAtKey, evaluator: (Curve geometry, Fin<GeometryContext> _) => Many(key: DerivativeAtKey, values: geometry.DerivativeAt(t: parameter, derivativeCount: count)));

    public static Query<Curve, Point3d> DivideByCount(int count) =>
        Query<Curve, Point3d>.Build(
            key: DivideByCountKey,
            evaluator: (Curve geometry, Fin<GeometryContext> _) =>
                geometry.DivideByCount(
                    segmentCount: count,
                    includeEnds: true,
                    points: out Point3d[] points) switch {
                        double[] => Many(key: DivideByCountKey, values: points),
                        _ => Fin.Fail<Seq<Point3d>>(DivideByCountKey.InvalidResult()),
                    });

    public static Query<Curve, Point3d> DivideByLength(double length) =>
        Query<Curve, Point3d>.Build(
            key: DivideByLengthKey,
            requirement: GeometryRequirement.CurveLength,
            evaluator: (Curve geometry, Fin<GeometryContext> _) =>
                geometry.DivideByLength(
                    segmentLength: length,
                    includeEnds: true,
                    points: out Point3d[] points) switch {
                        double[] => Many(key: DivideByLengthKey, values: points),
                        _ => Fin.Fail<Seq<Point3d>>(DivideByLengthKey.InvalidResult()),
                    });

    public static Query<TGeometry, TOut> Primitive<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Polyline) =>
                PrimitivePure<TGeometry, TOut, Curve, Polyline>(
                    project: static (Curve geometry, out Polyline value) =>
                        geometry.TryGetPolyline(polyline: out value)),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Arc) =>
                PrimitiveContext<TGeometry, TOut, Curve, Arc>(
                    project: static (Curve geometry, GeometryContext context, out Arc value) =>
                        geometry.TryGetArc(arc: out value, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Circle) =>
                PrimitiveContext<TGeometry, TOut, Curve, Circle>(
                    project: static (Curve geometry, GeometryContext context, out Circle value) =>
                        geometry.TryGetCircle(circle: out value, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Ellipse) =>
                PrimitiveContext<TGeometry, TOut, Curve, Ellipse>(
                    project: static (Curve geometry, GeometryContext context, out Ellipse value) =>
                        geometry.TryGetEllipse(ellipse: out value, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(Plane) =>
                PrimitiveContext<TGeometry, TOut, Curve, Plane>(
                    project: static (Curve geometry, GeometryContext context, out Plane value) =>
                        geometry.TryGetPlane(plane: out value, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Plane) =>
                PrimitiveContext<TGeometry, TOut, Surface, Plane>(
                    project: static (Surface geometry, GeometryContext context, out Plane value) =>
                        geometry.TryGetPlane(plane: out value, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Sphere) =>
                PrimitiveContext<TGeometry, TOut, Surface, Sphere>(
                    project: static (Surface geometry, GeometryContext context, out Sphere value) =>
                        geometry.TryGetSphere(sphere: out value, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Cylinder) =>
                PrimitiveContext<TGeometry, TOut, Surface, Cylinder>(
                    project: static (Surface geometry, GeometryContext context, out Cylinder value) =>
                        geometry.TryGetFiniteCylinder(cylinder: out value, tolerance: context.Absolute.Value)
                        || geometry.TryGetCylinder(cylinder: out value, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Cone) =>
                PrimitiveContext<TGeometry, TOut, Surface, Cone>(
                    project: static (Surface geometry, GeometryContext context, out Cone value) =>
                        geometry.TryGetCone(cone: out value, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Surface).IsAssignableFrom(c: geometry) && output == typeof(Torus) =>
                PrimitiveContext<TGeometry, TOut, Surface, Torus>(
                    project: static (Surface geometry, GeometryContext context, out Torus value) =>
                        geometry.TryGetTorus(torus: out value, tolerance: context.Absolute.Value)),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Box) =>
                PrimitiveContext<TGeometry, TOut, Brep, Box>(
                    project: static (Brep geometry, GeometryContext context, out Box value) => {
                        bool solved = geometry.IsBox(tolerance: context.Absolute.Value);
                        value = solved switch {
                            true => new Box(Plane.WorldXY, geometry.GetBoundingBox(accurate: true)),
                            false => default,
                        };
                        return solved;
                    }),
            (Type geometry, Type output) when typeof(Brep).IsAssignableFrom(c: geometry) && output == typeof(Extrusion) =>
                PrimitiveContext<TGeometry, TOut, Brep, Extrusion>(
                    project: static (Brep geometry, GeometryContext context, out Extrusion value) =>
                        geometry.TryGetExtrusion(
                            extrusion: out value,
                            tolerance: context.Absolute.Value)),
            _ => PrimitiveKey.Unsupported<TGeometry, TOut>(),
        };

    public static Query<Curve, CurveOrientation> Orientation(Plane plane) =>
        Query<Curve, CurveOrientation>.Build(key: OrientationKey, evaluator: (Curve geometry, Fin<GeometryContext> _) => One(key: OrientationKey, value: geometry.ClosedCurveOrientation(plane: plane)));

    public static Query<Curve, PointContainment> Contains(Point3d point, Plane plane) =>
        Query<Curve, PointContainment>.Build(
            key: ContainsKey,
            requiresContext: true,
            evaluator: (Curve geometry, Fin<GeometryContext> context) =>
                context
                    .Bind((GeometryContext geometryContext) => One(
                        key: ContainsKey,
                        value: geometry.Contains(
                            testPoint: point,
                            plane: plane,
                            tolerance: geometryContext.Absolute.Value))));

    public static Query<Surface, Curve> ShortPath(Point2d start, Point2d end) =>
        Query<Surface, Curve>.Build(
            key: ShortPathKey,
            requirement: GeometryRequirement.SurfaceEvaluation,
            evaluator: (Surface geometry, Fin<GeometryContext> context) =>
                context
                    .Bind((GeometryContext geometryContext) => (
                        Uv(surface: geometry, uv: start, context: geometryContext, key: ShortPathKey),
                        Uv(surface: geometry, uv: end, context: geometryContext, key: ShortPathKey)
                    ).Apply(static (Point2d a, Point2d b) => (Start: a, End: b)).As()
                    .Bind((ValueTuple<Point2d, Point2d> uv) => Optional(geometry.ShortPath(
                            start: uv.Item1,
                            end: uv.Item2,
                            tolerance: geometryContext.Absolute.Value))
                        .ToFin(ShortPathKey.InvalidResult())))
                    .Map(static (Curve path) => Seq(path)));

    private static Query<Surface, TOut> SurfaceUv<TOut>(
        OperationKey key,
        Point2d uv,
        Func<Surface, Point2d, Fin<Seq<TOut>>> project) =>
        Query<Surface, TOut>.Build(
            key: key,
            requirement: GeometryRequirement.SurfaceEvaluation,
            evaluator: (Surface geometry, Fin<GeometryContext> context) =>
                context
                    .Bind((GeometryContext model) => Uv(
                        surface: geometry,
                        uv: uv,
                        context: model,
                        key: key))
                    .Bind((Point2d parameter) => project(arg1: geometry, arg2: parameter)));

    private static Fin<Point2d> Uv(Surface surface, Point2d uv, GeometryContext context, OperationKey key) =>
        (surface.Domain(direction: 0), surface.Domain(direction: 1)) switch {
            (Interval u, Interval v) when u.IsValid
                && v.IsValid
                && u.IncludesParameter(t: uv.X)
                && v.IncludesParameter(t: uv.Y)
                && (surface is not BrepFace face || face.IsPointOnFace(
                    u: uv.X,
                    v: uv.Y,
                    tolerance: context.Absolute.Value) != PointFaceRelation.Exterior) => Fin.Succ(uv),
            _ => Fin.Fail<Point2d>(key.InvalidInput()),
        };
}

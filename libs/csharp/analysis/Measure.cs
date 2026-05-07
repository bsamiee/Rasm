using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino.Geometry;
using static LanguageExt.Prelude;
namespace Analysis;

public static partial class Query {
    public static Query<TGeometry, TOut> Bounds<TGeometry, TOut>(Bounds aspect) where TGeometry : notnull =>
        aspect.Kind switch {
            BoundsKind.Box => Box<TGeometry, TOut>(),
            BoundsKind.Oriented => Oriented<TGeometry, TOut>(plane: aspect.Plane),
            BoundsKind.Transformed => Transformed<TGeometry, TOut>(transform: aspect.Transform),
            BoundsKind.Center when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: BoundsCenterKey, query: Query<BoundingBox, Point3d>.Build(
                    key: BoundsCenterKey,
                    evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => One(key: BoundsCenterKey, value: geometry.Center))),
            BoundsKind.Corners when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Point3d) =>
                Cast<TGeometry, TOut>(key: BoundsCornersKey, query: Query<BoundingBox, Point3d>.Build(
                    key: BoundsCornersKey,
                    evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => Many(key: BoundsCornersKey, values: geometry.GetCorners()))),
            BoundsKind.Edges when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(Line) =>
                Cast<TGeometry, TOut>(key: BoxEdgesKey, query: Query<BoundingBox, Line>.Build(
                    key: BoxEdgesKey,
                    evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => Many(key: BoxEdgesKey, values: geometry.GetEdges()))),
            BoundsKind.Area when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(double) =>
                Cast<TGeometry, TOut>(key: BoxAreaKey, query: Query<BoundingBox, double>.Build(
                    key: BoxAreaKey,
                    evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => One(key: BoxAreaKey, value: geometry.Area))),
            BoundsKind.Volume when typeof(TGeometry) == typeof(BoundingBox) && typeof(TOut) == typeof(double) =>
                Cast<TGeometry, TOut>(key: BoxVolumeKey, query: Query<BoundingBox, double>.Build(
                    key: BoxVolumeKey,
                    evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => One(key: BoxVolumeKey, value: geometry.Volume))),
            _ => BoundsKey.Unsupported<TGeometry, TOut>(),
        };
    public static Query<TGeometry, TOut> Measure<TGeometry, TOut>(Measure aspect) where TGeometry : notnull =>
        (aspect.Kind, aspect.Mass) switch {
            (_, MassKind.None) => Query<TGeometry, TOut>.Reject(key: MeasureKey, fault: MeasureKey.InvalidInput()),
            (MeasureKind.Scalar, MassKind.Length) => Length<TGeometry, TOut>(),
            (MeasureKind.Scalar, MassKind.Area) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: nameof(Analysis.Measure.Area)), query: AreaMass<TGeometry, double>(name: nameof(Analysis.Measure.Area), project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.Area))),
            (MeasureKind.Scalar, MassKind.Volume) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: nameof(Analysis.Measure.Volume)), query: VolumeMass<TGeometry, double>(name: nameof(Analysis.Measure.Volume), project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.Volume))),
            (MeasureKind.Error, MassKind.Length) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthError"), query: LengthMass<TGeometry, double>(name: "LengthError", project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.LengthError))),
            (MeasureKind.Error, MassKind.Area) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaError"), query: AreaMass<TGeometry, double>(name: "AreaError", project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.AreaError))),
            (MeasureKind.Error, MassKind.Volume) when typeof(TOut) == typeof(double) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeError"), query: VolumeMass<TGeometry, double>(name: "VolumeError", project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.VolumeError))),
            (MeasureKind.Centroid, MassKind.Length) when typeof(TOut) == typeof(Point3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthCentroid"), query: LengthMass<TGeometry, Point3d>(name: "LengthCentroid", project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.Centroid))),
            (MeasureKind.Centroid, MassKind.Area) when typeof(TOut) == typeof(Point3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaCentroid"), query: AreaMass<TGeometry, Point3d>(name: "AreaCentroid", project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.Centroid))),
            (MeasureKind.Centroid, MassKind.Volume) when typeof(TOut) == typeof(Point3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeCentroid"), query: VolumeMass<TGeometry, Point3d>(name: "VolumeCentroid", project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.Centroid))),
            (MeasureKind.CentroidError, MassKind.Length) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthCentroidError"), query: LengthMass<TGeometry, Vector3d>(name: "LengthCentroidError", project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.CentroidError))),
            (MeasureKind.CentroidError, MassKind.Area) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaCentroidError"), query: AreaMass<TGeometry, Vector3d>(name: "AreaCentroidError", project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.CentroidError))),
            (MeasureKind.CentroidError, MassKind.Volume) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeCentroidError"), query: VolumeMass<TGeometry, Vector3d>(name: "VolumeCentroidError", project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.CentroidError))),
            (MeasureKind.Radii, MassKind.Length) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthRadii"), query: LengthMass<TGeometry, Vector3d>(name: "LengthRadii", project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true)),
            (MeasureKind.Radii, MassKind.Area) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaRadii"), query: AreaMass<TGeometry, Vector3d>(name: "AreaRadii", project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true)),
            (MeasureKind.Radii, MassKind.Volume) when typeof(TOut) == typeof(Vector3d) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumeRadii"), query: VolumeMass<TGeometry, Vector3d>(name: "VolumeRadii", project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration), secondMoments: true)),
            (MeasureKind.Principal, MassKind.Length) when typeof(TOut) == typeof(ValueTuple<double, Vector3d>) => Cast<TGeometry, TOut>(key: new OperationKey(name: "LengthPrincipal"), query: LengthMass<TGeometry, (double Moment, Vector3d Axis)>(name: "LengthPrincipal", project: static (OperationKey key, LengthMassProperties mass) => Principal(key: key, mass: mass), secondMoments: true, productMoments: true)),
            (MeasureKind.Principal, MassKind.Area) when typeof(TOut) == typeof(ValueTuple<double, Vector3d>) => Cast<TGeometry, TOut>(key: new OperationKey(name: "AreaPrincipal"), query: AreaMass<TGeometry, (double Moment, Vector3d Axis)>(name: "AreaPrincipal", project: static (OperationKey key, AreaMassProperties mass) => Principal(key: key, mass: mass), secondMoments: true, productMoments: true)),
            (MeasureKind.Principal, MassKind.Volume) when typeof(TOut) == typeof(ValueTuple<double, Vector3d>) => Cast<TGeometry, TOut>(key: new OperationKey(name: "VolumePrincipal"), query: VolumeMass<TGeometry, (double Moment, Vector3d Axis)>(name: "VolumePrincipal", project: static (OperationKey key, VolumeMassProperties mass) => Principal(key: key, mass: mass), secondMoments: true, productMoments: true)),
            _ => MeasureKey.Unsupported<TGeometry, TOut>(),
        };
    private static Fin<Seq<(double Moment, Vector3d Axis)>> Principal(OperationKey key, LengthMassProperties mass) =>
        key.Principal(
            solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
            x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis);
    private static Fin<Seq<(double Moment, Vector3d Axis)>> Principal(OperationKey key, AreaMassProperties mass) =>
        key.Principal(
            solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
            x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis);
    private static Fin<Seq<(double Moment, Vector3d Axis)>> Principal(OperationKey key, VolumeMassProperties mass) =>
        key.Principal(
            solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
            x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis);
    private static Query<TGeometry, TOut> Box<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(BoundingBox)
                && (typeof(GeometryBase).IsAssignableFrom(c: geometry)
                    || geometry == typeof(Line)
                    || geometry == typeof(Polyline)
                    || geometry == typeof(BoundingBox)) =>
                Cast<TGeometry, TOut>(key: BoundsKey, query: Query<TGeometry, BoundingBox>.Build(
                    key: BoundsKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        GeometryBase native => One(key: BoundsKey, value: native.GetBoundingBox(accurate: true)),
                        Line line => One(key: BoundsKey, value: line.BoundingBox),
                        Polyline polyline => One(key: BoundsKey, value: polyline.BoundingBox),
                        BoundingBox box => One(key: BoundsKey, value: box),
                        _ => Fin.Fail<Seq<BoundingBox>>(BoundsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(BoundingBox))),
                    })),
            _ => BoundsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> Oriented<TGeometry, TOut>(Plane plane) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(GeometryBase).IsAssignableFrom(c: geometry) && output == typeof(Box) =>
                Cast<TGeometry, TOut>(key: OrientedBoundsKey, query: Query<TGeometry, Box>.Build(
                    key: OrientedBoundsKey,
                    evaluator: (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        GeometryBase native => native.GetBoundingBox(plane: plane, worldBox: out Box box) switch {
                            BoundingBox local when local.IsValid => One(key: OrientedBoundsKey, value: box),
                            _ => Fin.Fail<Seq<Box>>(OrientedBoundsKey.InvalidResult()),
                        },
                        _ => Fin.Fail<Seq<Box>>(OrientedBoundsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Box))),
                    })),
            _ => OrientedBoundsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> Transformed<TGeometry, TOut>(Transform transform) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(GeometryBase).IsAssignableFrom(c: geometry) && output == typeof(BoundingBox) =>
                Cast<TGeometry, TOut>(key: TransformedBoundsKey, query: Query<TGeometry, BoundingBox>.Build(
                    key: TransformedBoundsKey,
                    evaluator: (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        GeometryBase native => One(key: TransformedBoundsKey, value: native.GetBoundingBox(xform: transform)),
                        _ => Fin.Fail<Seq<BoundingBox>>(TransformedBoundsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(BoundingBox))),
                    })),
            _ => TransformedBoundsKey.Unsupported<TGeometry, TOut>(),
        };
    private static Query<TGeometry, TOut> Length<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: LengthKey, query: Query<Line, double>.Build(
                    key: LengthKey,
                    evaluator: static (Line geometry, Fin<GeometryContext> _) => One(key: LengthKey, value: geometry.Length))),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: LengthKey, query: Query<Polyline, double>.Build(
                    key: LengthKey,
                    evaluator: static (Polyline geometry, Fin<GeometryContext> _) => One(key: LengthKey, value: geometry.Length))),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                Cast<TGeometry, TOut>(key: LengthKey, query: Query<TGeometry, double>.Build(
                    key: LengthKey,
                    requirement: GeometryRequirement.CurveLength,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> context) => (geometry, context) switch {
                        (Curve curve, Fin<GeometryContext> rail) => rail.Bind((GeometryContext model) => One(
                            key: LengthKey,
                            value: curve.GetLength(fractionalTolerance: model.Relative.Value))),
                        _ => Fin.Fail<Seq<double>>(LengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(double))),
                    })),
            _ => LengthKey.Unsupported<TGeometry, TOut>(),
        };
}

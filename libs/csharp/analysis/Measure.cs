using Core.Domain;
using LanguageExt;
using LanguageExt.Common;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Analysis;

// --- [MEASURE] ---------------------------------------------------------------------------------

public static partial class Query {
    public static Query<TGeometry, TOut> Bounds<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when output == typeof(BoundingBox)
                && (typeof(GeometryBase).IsAssignableFrom(c: geometry)
                    || geometry == typeof(Line)
                    || geometry == typeof(Polyline)
                    || geometry == typeof(BoundingBox)) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, BoundingBox>.Build(
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

    public static Query<TGeometry, TOut> OrientedBounds<TGeometry, TOut>(Plane plane) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(GeometryBase).IsAssignableFrom(c: geometry) && output == typeof(Box) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, Box>.Build(
                    key: OrientedBoundsKey,
                    evaluator: (TGeometry geometry, Fin<GeometryContext> _) =>
                        geometry switch {
                            GeometryBase native => native.GetBoundingBox(plane: plane, worldBox: out Box box) switch {
                                BoundingBox local when local.IsValid => One(key: OrientedBoundsKey, value: box),
                                _ => Fin.Fail<Seq<Box>>(OrientedBoundsKey.InvalidResult()),
                            },
                            _ => Fin.Fail<Seq<Box>>(OrientedBoundsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(Box))),
                        })),
            _ => OrientedBoundsKey.Unsupported<TGeometry, TOut>(),
        };

    public static Query<TGeometry, TOut> TransformedBounds<TGeometry, TOut>(Transform transform) where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when typeof(GeometryBase).IsAssignableFrom(c: geometry) && output == typeof(BoundingBox) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, BoundingBox>.Build(
                    key: TransformedBoundsKey,
                    evaluator: (TGeometry geometry, Fin<GeometryContext> _) =>
                        geometry switch {
                            GeometryBase native => One(key: TransformedBoundsKey, value: native.GetBoundingBox(xform: transform)),
                            _ => Fin.Fail<Seq<BoundingBox>>(TransformedBoundsKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(BoundingBox))),
                        })),
            _ => TransformedBoundsKey.Unsupported<TGeometry, TOut>(),
        };

    public static Query<TGeometry, TOut> Length<TGeometry, TOut>() where TGeometry : notnull =>
        (typeof(TGeometry), typeof(TOut)) switch {
            (Type geometry, Type output) when geometry == typeof(Line) && output == typeof(double) =>
                Cast<TGeometry, TOut>(query: Query<Line, double>.Build(
                    key: LengthKey,
                    evaluator: static (Line geometry, Fin<GeometryContext> _) => One(key: LengthKey, value: geometry.Length))),
            (Type geometry, Type output) when geometry == typeof(Polyline) && output == typeof(double) =>
                Cast<TGeometry, TOut>(query: Query<Polyline, double>.Build(
                    key: LengthKey,
                    evaluator: static (Polyline geometry, Fin<GeometryContext> _) => One(key: LengthKey, value: geometry.Length))),
            (Type geometry, Type output) when typeof(Curve).IsAssignableFrom(c: geometry) && output == typeof(double) =>
                Cast<TGeometry, TOut>(query: Query<TGeometry, double>.Build(
                    key: LengthKey,
                    evaluator: static (TGeometry geometry, Fin<GeometryContext> _) => geometry switch {
                        Curve curve => One(key: LengthKey, value: curve.GetLength()),
                        _ => Fin.Fail<Seq<double>>(LengthKey.Unsupported(geometryType: typeof(TGeometry), outputType: typeof(double))),
                    })),
            _ => LengthKey.Unsupported<TGeometry, TOut>(),
        };

    public static Query<GeometryBase, double> Area =>
        AreaMass(name: nameof(Area), project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.Area));

    public static Query<GeometryBase, double> Volume =>
        VolumeMass(name: nameof(Volume), project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.Volume));

    public static Query<BoundingBox, Point3d> BoundsCorners =>
        Query<BoundingBox, Point3d>.Build(key: BoundsCornersKey, evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => Many(key: BoundsCornersKey, values: geometry.GetCorners()));

    public static Query<BoundingBox, Line> BoxEdges =>
        Query<BoundingBox, Line>.Build(key: BoxEdgesKey, evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => Many(key: BoxEdgesKey, values: geometry.GetEdges()));

    public static Query<BoundingBox, double> BoxArea =>
        Query<BoundingBox, double>.Build(key: BoxAreaKey, evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => One(key: BoxAreaKey, value: geometry.Area));

    public static Query<BoundingBox, double> BoxVolume =>
        Query<BoundingBox, double>.Build(key: BoxVolumeKey, evaluator: static (BoundingBox geometry, Fin<GeometryContext> _) => One(key: BoxVolumeKey, value: geometry.Volume));

    public static Query<Curve, double> LengthError =>
        LengthMass(name: nameof(LengthError), project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.LengthError));

    public static Query<GeometryBase, double> AreaError =>
        AreaMass(name: nameof(AreaError), project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.AreaError));

    public static Query<GeometryBase, double> VolumeError =>
        VolumeMass(name: nameof(VolumeError), project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.VolumeError));

    public static Query<Curve, Vector3d> LengthRadii =>
        LengthMass(
            name: nameof(LengthRadii),
            project: static (OperationKey key, LengthMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration),
            secondMoments: true,
            productMoments: false);

    public static Query<GeometryBase, Vector3d> AreaRadii =>
        AreaMass(
            name: nameof(AreaRadii),
            project: static (OperationKey key, AreaMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration),
            secondMoments: true,
            productMoments: false);

    public static Query<GeometryBase, Vector3d> VolumeRadii =>
        VolumeMass(
            name: nameof(VolumeRadii),
            project: static (OperationKey key, VolumeMassProperties mass) => One(key: key, value: mass.CentroidCoordinatesRadiiOfGyration),
            secondMoments: true,
            productMoments: false);

    public static Query<Curve, (double Moment, Vector3d Axis)> LengthPrincipal =>
        LengthMass(
            name: nameof(LengthPrincipal),
            project: static (OperationKey key, LengthMassProperties mass) => key.Principal(
                solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            secondMoments: true,
            productMoments: true);

    public static Query<GeometryBase, (double Moment, Vector3d Axis)> AreaPrincipal =>
        AreaMass(
            name: nameof(AreaPrincipal),
            project: static (OperationKey key, AreaMassProperties mass) => key.Principal(
                solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            secondMoments: true,
            productMoments: true);

    public static Query<GeometryBase, (double Moment, Vector3d Axis)> VolumePrincipal =>
        VolumeMass(
            name: nameof(VolumePrincipal),
            project: static (OperationKey key, VolumeMassProperties mass) => key.Principal(
                solved: mass.WorldCoordinatesPrincipalMoments(x: out double x, xaxis: out Vector3d xAxis, y: out double y, yaxis: out Vector3d yAxis, z: out double z, zaxis: out Vector3d zAxis),
                x: x, xAxis: xAxis, y: y, yAxis: yAxis, z: z, zAxis: zAxis),
            secondMoments: true,
            productMoments: true);
}

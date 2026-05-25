namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CurveProjection {
    public static readonly CurveProjection Tangent = new(key: 0,
        sample: static (curve, t, _, key) => curve.TangentAt(t: t) switch {
            Vector3d tangent when tangent.IsValid && !tangent.IsTiny() => Fin.Succ((object)tangent),
            _ => Fin.Fail<object>(key.InvalidResult()),
        });
    public static readonly CurveProjection Curvature = new(key: 1,
        sample: static (curve, t, _, key) => curve.CurvatureAt(t: t) switch {
            Vector3d curvature when curvature.IsValid => Fin.Succ((object)curvature),
            _ => Fin.Fail<object>(key.InvalidResult()),
        });
    public static readonly CurveProjection Frame = new(key: 2, sample: static (curve, t, _, key) => CurveFrame(curve: curve, parameter: t, perpendicular: false, key: key, project: static frame => frame));
    public static readonly CurveProjection PerpendicularFrame = new(key: 3, sample: static (curve, t, _, key) => CurveFrame(curve: curve, parameter: t, perpendicular: true, key: key, project: static frame => frame));
    public static readonly CurveProjection ArcLength = new(key: 4,
        sample: static (curve, t, context, key) => curve.GetLength(fractionalTolerance: context.Fractional, subdomain: new Interval(curve.Domain.T0, t)) switch {
            double length when RhinoMath.IsValidDouble(x: length) && (length > 0.0 || Math.Abs(value: t - curve.Domain.T0) <= context.Absolute.Value) => Fin.Succ((object)length),
            _ => Fin.Fail<object>(key.InvalidResult()),
        });
    public static readonly CurveProjection FrameNormal = new(key: 5, sample: static (curve, t, _, key) => CurveFrame(curve: curve, parameter: t, perpendicular: false, key: key, project: static frame => frame.YAxis));
    public static readonly CurveProjection FrameBinormal = new(key: 6, sample: static (curve, t, _, key) => CurveFrame(curve: curve, parameter: t, perpendicular: false, key: key, project: static frame => frame.ZAxis));
    public static readonly CurveProjection PerpendicularNormal = new(key: 7, sample: static (curve, t, _, key) => CurveFrame(curve: curve, parameter: t, perpendicular: true, key: key, project: static frame => frame.YAxis));
    public static readonly CurveProjection PerpendicularBinormal = new(key: 8, sample: static (curve, t, _, key) => CurveFrame(curve: curve, parameter: t, perpendicular: true, key: key, project: static frame => frame.ZAxis));
    [UseDelegateFromConstructor] private partial Fin<object> Sample(Curve curve, double parameter, Context context, Op key);
    internal Fin<TOut> Project<TOut>(Curve curve, double parameter, Context context, Op key) =>
        from active in Optional(curve).ToFin(key.InvalidInput())
        from __ in guard(active.IsValid, key.InvalidInput())
        from _ in guard(active.Domain.IncludesParameter(t: parameter), key.InvalidInput())
        from raw in Sample(curve: active, parameter: parameter, context: context, key: key).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
        from output in (raw, typeof(TOut)) switch {
            (Vector3d v, Type t) when t == typeof(Vector3d) => AtomProjection.Value<Vector3d, TOut>(value: v, key: key),
            (Vector3d v, Type t) when t == typeof(Direction) => Direction.Of(value: v, context: context, key: key).Bind(d => d.Project<TOut>(key: key)),
            (Vector3d v, Type t) when t == typeof(double) && ReferenceEquals(objA: this, objB: Curvature) =>
                key.AcceptValue(value: v).Bind(valid => AtomProjection.Value<double, TOut>(value: valid.Length, key: key)),
            (Plane p, Type t) when t == typeof(Plane) => AtomProjection.Value<Plane, TOut>(value: p, key: key),
            (Plane p, Type t) when t == typeof(VectorFrame) => VectorFrame.Of(origin: p.Origin, normal: p.ZAxis, xHint: Some(p.XAxis), context: context, key: key).Bind(f => f.Project<TOut>(key: key)),
            (double d, Type t) when t == typeof(double) => AtomProjection.Value<double, TOut>(value: d, key: key),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(CurveProjection), outputType: typeof(TOut))),
        }
        select output;
    private static Fin<object> CurveFrame(Curve curve, double parameter, bool perpendicular, Op key, Func<Plane, object> project) =>
        perpendicular switch {
            true => curve.PerpendicularFrameAt(t: parameter, plane: out Plane frame) ? Fin.Succ(project(arg: frame)) : Fin.Fail<object>(key.InvalidResult()),
            false => curve.FrameAt(t: parameter, plane: out Plane frame) ? Fin.Succ(project(arg: frame)) : Fin.Fail<object>(key.InvalidResult()),
        };
}

[SmartEnum<int>]
public sealed partial class SurfaceProjection {
    public static readonly SurfaceProjection PrincipalCurvatures = new(key: 0,
        sample: static (surface, uv, _, key) => WithCurvature(surface: surface, uv: uv, key: key, project: static sc => Fin.Succ((object)Seq(sc.MaximumPrincipalCurvature, sc.MinimumPrincipalCurvature))));
    public static readonly SurfaceProjection Gaussian = new(key: 1, sample: static (surface, uv, _, key) => WithCurvature(surface: surface, uv: uv, key: key, project: sc => ScalarMetric.Gaussian.Of(value: sc, key: key).Map(static value => (object)value)));
    public static readonly SurfaceProjection Mean = new(key: 2, sample: static (surface, uv, _, key) => WithCurvature(surface: surface, uv: uv, key: key, project: sc => ScalarMetric.Mean.Of(value: sc, key: key).Map(static value => (object)value)));
    public static readonly SurfaceProjection MaximumOsculatingCircle = new(key: 3,
        sample: static (surface, uv, _, key) => WithCurvature(surface: surface, uv: uv, key: key, project: sc => sc.OsculatingCircle(0) switch {
            Circle circle when circle.IsValid => Fin.Succ((object)circle),
            _ => Fin.Fail<object>(key.InvalidResult()),
        }));
    public static readonly SurfaceProjection Normal = new(key: 4, sample: static (surface, uv, _, key) => GeometryKernel.NormalAt(surface: surface, uv: uv, key: key).Map(static normal => (object)normal));
    public static readonly SurfaceProjection ShapeOperator = new(key: 5,
        sample: static (surface, uv, context, key) => WithCurvature(surface: surface, uv: uv, key: key, project: sc => ShapeOperatorOf(curvature: sc, context: context, key: key).Map(static value => (object)value)));
    public static readonly SurfaceProjection MinimumOsculatingCircle = new(key: 6,
        sample: static (surface, uv, _, key) => WithCurvature(surface: surface, uv: uv, key: key, project: sc => sc.OsculatingCircle(1) switch {
            Circle circle when circle.IsValid => Fin.Succ((object)circle),
            _ => Fin.Fail<object>(key.InvalidResult()),
        }));
    public static readonly SurfaceProjection Point = new(key: 7, sample: static (surface, uv, _, key) => key.AcceptValue(value: surface.PointAt(u: uv.X, v: uv.Y)).Map(static point => (object)point));
    public static readonly SurfaceProjection Frame = new(key: 8, sample: static (surface, uv, _, key) => GeometryKernel.FrameAt(surface: surface, uv: uv, key: key).Map(static value => (object)value));
    public static readonly SurfaceProjection UvFrame = new(key: 9,
        sample: static (surface, uv, _, key) => SurfaceDerivatives(surface: surface, uv: uv, key: key)
            .Bind(d => OrientedFrame(surface: surface, uv: uv, frame: new Plane(origin: d.Point, xDirection: d.Du, yDirection: d.Dv), key: key).Map(static value => (object)value)));
    public static readonly SurfaceProjection Jacobian = new(key: 10,
        sample: static (surface, uv, _, key) => SurfaceDerivatives(surface: surface, uv: uv, key: key)
            .Bind(d => Matrix.Of(rows: Dimension.Create(value: 3), cols: Dimension.Create(value: 2), entries: [d.Du.X, d.Dv.X, d.Du.Y, d.Dv.Y, d.Du.Z, d.Dv.Z], key: key))
            .Map(static value => (object)value));
    public static readonly SurfaceProjection Metric = new(key: 11,
        sample: static (surface, uv, _, key) => SurfaceDerivatives(surface: surface, uv: uv, key: key)
            .Bind(d => SymmetricMatrix.Of(dim: Dimension.Create(value: 2), upper: [d.Du * d.Du, d.Du * d.Dv, d.Dv * d.Dv], key: key))
            .Map(static value => (object)value));
    public static readonly SurfaceProjection AreaScale = new(key: 12,
        sample: static (surface, uv, _, key) => SurfaceDerivatives(surface: surface, uv: uv, key: key)
            .Bind(d => key.AcceptValue(value: Vector3d.CrossProduct(a: d.Du, b: d.Dv).Length).Map(static value => (object)value)));
    [UseDelegateFromConstructor] private partial Fin<object> Sample(Surface surface, Point2d uv, Context context, Op key);
    internal Fin<TOut> Project<TOut>(Surface surface, double u, double v, Context context, Op key) =>
        from active in Optional(surface).ToFin(key.InvalidInput())
        from __ in guard(active.IsValid, key.InvalidInput())
        from uv in GeometryKernel.SurfaceUv(surface: active, uv: new Point2d(x: u, y: v), context: context, key: key)
        from output in
            from raw in Sample(surface: active, uv: uv, context: context, key: key).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
            from projected in (raw, typeof(TOut)) switch {
                (double d, Type t) when t == typeof(double) => AtomProjection.Value<double, TOut>(value: d, key: key),
                (Circle c, Type t) when t == typeof(Circle) => AtomProjection.Value<Circle, TOut>(value: c, key: key),
                (Point3d p, Type t) when t == typeof(Point3d) => AtomProjection.Value<Point3d, TOut>(value: p, key: key),
                (Plane p, Type t) when t == typeof(Plane) => AtomProjection.Value<Plane, TOut>(value: p, key: key),
                (Plane p, Type t) when t == typeof(VectorFrame) => VectorFrame.Of(origin: p.Origin, normal: p.ZAxis, xHint: Some(p.XAxis), context: context, key: key).Map(static value => (TOut)(object)value),
                (Vector3d n, Type t) when t == typeof(Vector3d) => AtomProjection.Value<Vector3d, TOut>(value: n, key: key),
                (Vector3d n, Type t) when t == typeof(Direction) => Direction.Of(value: n, context: context, key: key).Map(static value => (TOut)(object)value),
                (Matrix matrix, Type t) when t == typeof(Matrix) => matrix.IsValid ? AtomProjection.Value<Matrix, TOut>(value: matrix, key: key) : Fin.Fail<TOut>(error: key.InvalidResult()),
                (Seq<double> ks, Type t) when t == typeof(Seq<double>) =>
                    ks.TraverseM(k => key.AcceptValue(value: k)).As().Map(static valid => (TOut)(object)valid),
                (SymmetricMatrix matrix, Type t) when t == typeof(SymmetricMatrix) => matrix.IsValid ? AtomProjection.Value<SymmetricMatrix, TOut>(value: matrix, key: key) : Fin.Fail<TOut>(error: key.InvalidResult()),
                _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(SurfaceProjection), outputType: typeof(TOut))),
            }
            select projected
        select output;
    private static Fin<T> WithCurvature<T>(Surface surface, Point2d uv, Op key, Func<SurfaceCurvature, Fin<T>> project) {
        using SurfaceCurvature? curvature = surface.CurvatureAt(u: uv.X, v: uv.Y);
        return curvature is { IsSet: true }
            ? project(arg: curvature)
            : Fin.Fail<T>(key.InvalidResult());
    }
    private static Fin<(Point3d Point, Vector3d Du, Vector3d Dv)> SurfaceDerivatives(Surface surface, Point2d uv, Op key) =>
        surface.Evaluate(u: uv.X, v: uv.Y, numberDerivatives: 1, point: out Point3d point, derivatives: out Vector3d[] derivatives)
        && derivatives is not null
        && derivatives.Length >= 2
            ? from validPoint in key.AcceptValue(value: point)
              from du in key.AcceptValue(value: derivatives[0])
              from dv in key.AcceptValue(value: derivatives[1])
              select (Point: validPoint, Du: du, Dv: dv)
            : Fin.Fail<(Point3d Point, Vector3d Du, Vector3d Dv)>(key.InvalidResult());
    private static Fin<Plane> OrientedFrame(Surface surface, Point2d uv, Plane frame, Op key) =>
        frame.IsValid
            ? GeometryKernel.NormalAt(surface: surface, uv: uv, key: key)
                .Bind(normal => key.AcceptValue(value: frame.ZAxis * normal >= 0.0 ? frame : new Plane(origin: frame.Origin, xDirection: frame.XAxis, yDirection: -frame.YAxis)))
            : Fin.Fail<Plane>(key.InvalidResult());
    private static Fin<SymmetricMatrix> ShapeOperatorOf(SurfaceCurvature curvature, Context context, Op key) {
        double k0 = curvature.Kappa(direction: 0);
        double k1 = curvature.Kappa(direction: 1);
        return from d0 in Direction.Of(value: curvature.Direction(direction: 0), context: context, key: key)
               from d1 in Direction.Of(value: curvature.Direction(direction: 1), context: context, key: key)
               from matrix in SymmetricMatrix.Of(
                   dim: Dimension.Create(value: 3),
                   upper: [
                       (k0 * d0.Value.X * d0.Value.X) + (k1 * d1.Value.X * d1.Value.X),
                       (k0 * d0.Value.X * d0.Value.Y) + (k1 * d1.Value.X * d1.Value.Y),
                       (k0 * d0.Value.X * d0.Value.Z) + (k1 * d1.Value.X * d1.Value.Z),
                       (k0 * d0.Value.Y * d0.Value.Y) + (k1 * d1.Value.Y * d1.Value.Y),
                       (k0 * d0.Value.Y * d0.Value.Z) + (k1 * d1.Value.Y * d1.Value.Z),
                       (k0 * d0.Value.Z * d0.Value.Z) + (k1 * d1.Value.Z * d1.Value.Z),
                   ],
                   key: key)
               select matrix;
    }
}

[SmartEnum<int>]
public sealed partial class MotionInterpolation {
    public static readonly MotionInterpolation Linear = new(key: 0, interpolate: static (a, b, t) => InterpolatePlanes(a: a, b: b, t: t, spherical: false));
    public static readonly MotionInterpolation Slerp = new(key: 1, interpolate: static (a, b, t) => InterpolatePlanes(a: a, b: b, t: t, spherical: true));
    [UseDelegateFromConstructor] internal partial Fin<Plane> Interpolate(Plane a, Plane b, UnitInterval t);

    private static Fin<Plane> InterpolatePlanes(Plane a, Plane b, UnitInterval t, bool spherical) =>
        (!a.IsValid || !b.IsValid, a.EpsilonEquals(other: b, epsilon: RhinoMath.ZeroTolerance)) switch {
            (true, _) => Fin.Fail<Plane>(Op.Of().InvalidInput()),
            (_, true) => Fin.Succ(a),
            _ => (Quaternion.Rotation(plane0: Plane.WorldXY, plane1: a), Quaternion.Rotation(plane0: Plane.WorldXY, plane1: b)) switch {
                (Quaternion qa, Quaternion qb) => (spherical switch {
                    true => Quaternion.Slerp(a: qa, b: qb, t: t.Value),
                    false => Quaternion.Lerp(a: qa, b: qb, t: t.Value),
                }).GetRotation(plane: out Plane oriented) && oriented.IsValid
                    ? Fin.Succ(new Plane(origin: a.Origin + ((b.Origin - a.Origin) * t.Value), xDirection: oriented.XAxis, yDirection: oriented.YAxis))
                    : Fin.Fail<Plane>(Op.Of().InvalidResult()),
            },
        };
}

[SmartEnum<int>]
public sealed partial class ConeProjection {
    public static readonly ConeProjection HalfAngle = new(key: 0, sample: static cone => cone.HalfAngle), SolidAngle = new(key: 1, sample: static cone => cone.SolidAngle), Axis = new(key: 2, sample: static cone => cone.Axis), Apex = new(key: 3, sample: static cone => cone.Apex);
    [UseDelegateFromConstructor] private partial object Sample(VectorCone cone);
    internal Fin<TOut> Project<TOut>(VectorCone cone, Op key) =>
        (Sample(cone: cone), typeof(TOut)) switch {
            (VectorAngle a, Type t) when t == typeof(VectorAngle) => Fin.Succ((TOut)(object)a),
            (VectorAngle a, Type t) when t == typeof(double) => AtomProjection.Value<double, TOut>(value: a.Value, key: key),
            (double d, Type t) when t == typeof(double) => AtomProjection.Value<double, TOut>(value: d, key: key),
            (Direction d, Type t) when t == typeof(Direction) => d.Project<TOut>(key: key),
            (Direction d, Type t) when t == typeof(Vector3d) => d.Project<Vector3d>(key: key).Map(static x => (TOut)(object)x),
            (Point3d p, Type t) when t == typeof(Point3d) => AtomProjection.Value<Point3d, TOut>(value: p, key: key),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(ConeProjection), outputType: typeof(TOut))),
        };
}

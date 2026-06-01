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
    public static readonly CurveProjection Frame = CurveFrameProjection(key: 2, perpendicular: false, project: static frame => frame);
    public static readonly CurveProjection PerpendicularFrame = CurveFrameProjection(key: 3, perpendicular: true, project: static frame => frame);
    public static readonly CurveProjection ArcLength = new(key: 4,
        sample: static (curve, t, context, key) => curve.GetLength(fractionalTolerance: context.Fractional, subdomain: new Interval(curve.Domain.T0, t)) switch {
            double length when RhinoMath.IsValidDouble(x: length) && (length > 0.0 || Math.Abs(value: t - curve.Domain.T0) <= context.Absolute.Value) => Fin.Succ((object)length),
            _ => Fin.Fail<object>(key.InvalidResult()),
        });
    public static readonly CurveProjection FrameNormal = CurveFrameProjection(key: 5, perpendicular: false, project: static frame => frame.YAxis);
    public static readonly CurveProjection FrameBinormal = CurveFrameProjection(key: 6, perpendicular: false, project: static frame => frame.ZAxis);
    public static readonly CurveProjection PerpendicularNormal = CurveFrameProjection(key: 7, perpendicular: true, project: static frame => frame.YAxis);
    public static readonly CurveProjection PerpendicularBinormal = CurveFrameProjection(key: 8, perpendicular: true, project: static frame => frame.ZAxis);
    [UseDelegateFromConstructor] private partial Fin<object> Sample(Curve curve, double parameter, Context context, Op key);
    internal Fin<TOut> Project<TOut>(Curve curve, double parameter, Context context, Op key) =>
        from active in Optional(curve).ToFin(key.InvalidInput())
        from __ in guard(active.IsValid, key.InvalidInput())
        from _ in guard(active.Domain.IncludesParameter(t: parameter), key.InvalidInput())
        from raw in Sample(curve: active, parameter: parameter, context: context, key: key).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
        from output in AtomProjection.Raw<TOut>(raw: raw, context: Some(context), key: key, owner: typeof(CurveProjection), admitsVectorMagnitude: ReferenceEquals(objA: this, objB: Curvature))
        select output;
    private static CurveProjection CurveFrameProjection(int key, bool perpendicular, Func<Plane, object> project) =>
        new(key: key, sample: (curve, parameter, _, op) => CurveFrame(curve: curve, parameter: parameter, perpendicular: perpendicular, key: op, project: project));
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
    public static readonly SurfaceProjection UvFrame = Derivatives(key: 9, project: static (surface, uv, derivatives, _, key) => OrientedFrame(surface: surface, uv: uv, frame: new Plane(origin: derivatives.Point, xDirection: derivatives.Du, yDirection: derivatives.Dv), key: key).Map(static value => (object)value));
    public static readonly SurfaceProjection Jacobian = Derivatives(key: 10, project: static (_, _, derivatives, _, key) => Matrix.Of(rows: Dimension.Create(value: 3), cols: Dimension.Create(value: 2), entries: [derivatives.Du.X, derivatives.Dv.X, derivatives.Du.Y, derivatives.Dv.Y, derivatives.Du.Z, derivatives.Dv.Z], key: key).Map(static value => (object)value));
    public static readonly SurfaceProjection Metric = Derivatives(key: 11, project: static (_, _, derivatives, _, key) => SymmetricMatrix.Of(dim: Dimension.Create(value: 2), upper: [derivatives.Du * derivatives.Du, derivatives.Du * derivatives.Dv, derivatives.Dv * derivatives.Dv], key: key).Map(static value => (object)value));
    public static readonly SurfaceProjection AreaScale = Derivatives(key: 12, project: static (_, _, derivatives, _, key) => key.AcceptValue(value: Vector3d.CrossProduct(a: derivatives.Du, b: derivatives.Dv).Length).Map(static value => (object)value));
    [UseDelegateFromConstructor] private partial Fin<object> Sample(Surface surface, Point2d uv, Context context, Op key);
    internal Fin<TOut> Project<TOut>(Surface surface, double u, double v, Context context, Op key) =>
        from active in Optional(surface).ToFin(key.InvalidInput())
        from __ in guard(active.IsValid, key.InvalidInput())
        from uv in GeometryKernel.SurfaceUv(surface: active, uv: new Point2d(x: u, y: v), context: context, key: key)
        from raw in Sample(surface: active, uv: uv, context: context, key: key).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
        from output in AtomProjection.Raw<TOut>(raw: raw, context: Some(context), key: key, owner: typeof(SurfaceProjection))
        select output;
    private static SurfaceProjection Derivatives(int key, Func<Surface, Point2d, (Point3d Point, Vector3d Du, Vector3d Dv), Context, Op, Fin<object>> project) =>
        new(key: key, sample: (surface, uv, context, op) => SurfaceDerivatives(surface: surface, uv: uv, key: op).Bind(derivatives => project(arg1: surface, arg2: uv, arg3: derivatives, arg4: context, arg5: op)));
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
        from validFrame in FieldNabla.Plane(basis: frame, key: key)
        from normal in GeometryKernel.NormalAt(surface: surface, uv: uv, key: key)
        from oriented in FieldNabla.Plane(
            basis: validFrame.ZAxis * normal >= 0.0 ? validFrame : new Plane(origin: validFrame.Origin, xDirection: validFrame.XAxis, yDirection: -validFrame.YAxis),
            key: key)
        select oriented;
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
        AtomProjection.Raw<TOut>(raw: Sample(cone: cone), context: Option<Context>.None, key: key, owner: typeof(ConeProjection));
}

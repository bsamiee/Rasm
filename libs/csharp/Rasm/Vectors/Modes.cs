namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CurveProjection {
    public static readonly CurveProjection Tangent = new(key: 0,
        sample: static (curve, t, _) => curve.TangentAt(t: t) switch {
            Vector3d tangent when tangent.IsValid && !tangent.IsTiny() => Fin.Succ((object)tangent),
            _ => Fin.Fail<object>(Op.Of().InvalidResult()),
        });
    public static readonly CurveProjection Curvature = new(key: 1,
        sample: static (curve, t, _) => curve.CurvatureAt(t: t) switch {
            Vector3d curvature when curvature.IsValid => Fin.Succ((object)curvature),
            _ => Fin.Fail<object>(Op.Of().InvalidResult()),
        });
    public static readonly CurveProjection FrenetFrame = new(key: 2,
        sample: static (curve, t, _) => curve.FrameAt(t: t, plane: out Plane p)
            ? Fin.Succ((object)p)
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    public static readonly CurveProjection BishopFrame = new(key: 3,
        sample: static (curve, t, _) => curve.PerpendicularFrameAt(t: t, plane: out Plane p)
            ? Fin.Succ((object)p)
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    public static readonly CurveProjection ArcLength = new(key: 4,
        sample: static (curve, t, context) => curve.GetLength(fractionalTolerance: context.Fractional, subdomain: new Interval(curve.Domain.T0, t)) switch {
            double length when RhinoMath.IsValidDouble(x: length) && length >= 0.0 => Fin.Succ((object)length),
            _ => Fin.Fail<object>(Op.Of().InvalidResult()),
        });
    [UseDelegateFromConstructor] private partial Fin<object> Sample(Curve curve, double parameter, Context context);
    internal Fin<TOut> Project<TOut>(Curve curve, double parameter, Context context, Op key) =>
        from active in Optional(curve).ToFin(key.InvalidInput())
        from _ in guard(active.Domain.IncludesParameter(t: parameter), key.InvalidInput())
        from raw in Sample(curve: active, parameter: parameter, context: context).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
        from output in (raw, typeof(TOut)) switch {
            (Vector3d v, Type t) when t == typeof(Vector3d) => key.AcceptValue(value: (TOut)(object)v),
            (Vector3d v, Type t) when t == typeof(Direction) => Direction.Of(value: v, context: context, key: key).Bind(d => d.Project<TOut>(key: key)),
            (Vector3d v, Type t) when t == typeof(double) =>
                key.AcceptValue(value: v).Bind(valid => key.AcceptValue(value: (TOut)(object)valid.Length)),
            (Plane p, Type t) when t == typeof(Plane) => key.AcceptValue(value: (TOut)(object)p),
            (Plane p, Type t) when t == typeof(VectorFrame) => VectorFrame.Of(origin: p.Origin, normal: p.ZAxis, xHint: Some(p.XAxis), context: context, key: key).Bind(f => f.Project<TOut>(key: key)),
            (double d, Type t) when t == typeof(double) => key.AcceptValue(value: (TOut)(object)d),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(CurveProjection), outputType: typeof(TOut))),
        }
        select output;
}

[SmartEnum<int>]
public sealed partial class SurfaceProjection {
    public static readonly SurfaceProjection PrincipalCurvatures = new(key: 0,
        sample: static sc => Fin.Succ((object)Seq(sc.MaximumPrincipalCurvature, sc.MinimumPrincipalCurvature)));
    public static readonly SurfaceProjection Gaussian = new(key: 1,
        sample: static sc => Fin.Succ((object)sc.Gaussian));
    public static readonly SurfaceProjection Mean = new(key: 2,
        sample: static sc => Fin.Succ((object)sc.Mean));
    public static readonly SurfaceProjection OsculatingCircle = new(key: 3,
        sample: static sc => sc.OsculatingCircle(0) switch {
            Circle circle when circle.IsValid => Fin.Succ((object)circle),
            _ => Fin.Fail<object>(Op.Of().InvalidResult()),
        });
    public static readonly SurfaceProjection Normal = new(key: 4,
        sample: static sc => Fin.Succ((object)sc.Normal));
    public static readonly SurfaceProjection ShapeOperator = new(key: 5,
        sample: static sc => SymmetricMatrix.Of(
            dim: Dimension.Create(value: 3),
            upper: new Arr<double>([
                (sc.Kappa(direction: 0) * sc.Direction(direction: 0).X * sc.Direction(direction: 0).X) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).X * sc.Direction(direction: 1).X),
                (sc.Kappa(direction: 0) * sc.Direction(direction: 0).X * sc.Direction(direction: 0).Y) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).X * sc.Direction(direction: 1).Y),
                (sc.Kappa(direction: 0) * sc.Direction(direction: 0).X * sc.Direction(direction: 0).Z) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).X * sc.Direction(direction: 1).Z),
                (sc.Kappa(direction: 0) * sc.Direction(direction: 0).Y * sc.Direction(direction: 0).Y) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).Y * sc.Direction(direction: 1).Y),
                (sc.Kappa(direction: 0) * sc.Direction(direction: 0).Y * sc.Direction(direction: 0).Z) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).Y * sc.Direction(direction: 1).Z),
                (sc.Kappa(direction: 0) * sc.Direction(direction: 0).Z * sc.Direction(direction: 0).Z) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).Z * sc.Direction(direction: 1).Z)]),
            key: Op.Of()).Map(static value => (object)value));
    [UseDelegateFromConstructor] private partial Fin<object> Sample(SurfaceCurvature curvature);
    internal Fin<TOut> Project<TOut>(Surface surface, double u, double v, Context context, Op key) =>
        from active in Optional(surface).ToFin(key.InvalidInput())
        from uv in GeometryKernel.SurfaceUv(surface: active, uv: new Point2d(x: u, y: v), context: context, key: key)
        from output in active.CurvatureAt(u: uv.X, v: uv.Y) is SurfaceCurvature sc && sc.IsSet
            ? new Lease<SurfaceCurvature>.Owned(Value: sc).Use(curvature =>
                from raw in Sample(curvature: curvature).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
                from projected in (raw, typeof(TOut)) switch {
                    (double d, Type t) when t == typeof(double) => key.AcceptValue(value: (TOut)(object)d),
                    (Circle c, Type t) when t == typeof(Circle) => key.AcceptValue(value: (TOut)(object)c),
                    (Vector3d n, Type t) when t == typeof(Vector3d) => ReferenceEquals(objA: this, objB: Normal)
                        ? GeometryKernel.NormalAt(surface: active, uv: uv, key: key).Map(static value => (TOut)(object)value)
                        : key.AcceptValue(value: (TOut)(object)n),
                    (Seq<double> ks, Type t) when t == typeof(Seq<double>) =>
                        ks.TraverseM(k => key.AcceptValue(value: k)).As().Map(static valid => (TOut)(object)valid),
                    (SymmetricMatrix matrix, Type t) when t == typeof(SymmetricMatrix) && matrix.IsValid =>
                        key.AcceptValue(value: matrix).Map(static v => (TOut)(object)v),
                    (SymmetricMatrix, Type t) when t == typeof(SymmetricMatrix) => Fin.Fail<TOut>(error: key.InvalidResult()),
                    _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(SurfaceProjection), outputType: typeof(TOut))),
                }
                select projected)
            : Fin.Fail<TOut>(key.InvalidResult())
        select output;
}

[SmartEnum<int>]
public sealed partial class MotionInterpolation {
    public static readonly MotionInterpolation Linear = new(key: 0,
        interpolate: static (a, b, t) => InterpolatePlanes(a: a, b: b, t: t, spherical: false));
    public static readonly MotionInterpolation Slerp = new(key: 1,
        interpolate: static (a, b, t) => InterpolatePlanes(a: a, b: b, t: t, spherical: true));
    [UseDelegateFromConstructor] internal partial Fin<Plane> Interpolate(Plane a, Plane b, UnitInterval t);

    private static Fin<Plane> InterpolatePlanes(Plane a, Plane b, UnitInterval t, bool spherical) {
        Op op = Op.Of();
        if (!a.IsValid || !b.IsValid) return Fin.Fail<Plane>(op.InvalidInput());
        if (a.EpsilonEquals(other: b, epsilon: RhinoMath.ZeroTolerance)) return Fin.Succ(a);
        Quaternion qa = Quaternion.Rotation(plane0: Plane.WorldXY, plane1: a);
        Quaternion qb = Quaternion.Rotation(plane0: Plane.WorldXY, plane1: b);
        Quaternion qt = spherical
            ? Quaternion.Slerp(a: qa, b: qb, t: t.Value)
            : Quaternion.Lerp(a: qa, b: qb, t: t.Value);
        Point3d origin = a.Origin;
        origin.Interpolate(pA: a.Origin, pB: b.Origin, t: t.Value);
        return qt.GetRotation(plane: out Plane oriented) && oriented.IsValid
            ? Fin.Succ(new Plane(origin: origin, xDirection: oriented.XAxis, yDirection: oriented.YAxis))
            : Fin.Fail<Plane>(op.InvalidResult());
    }
}

[SmartEnum<int>]
public sealed partial class ConeProjection {
    public static readonly ConeProjection HalfAngle = new(key: 0, sample: static cone => cone.HalfAngle);
    public static readonly ConeProjection SolidAngle = new(key: 1, sample: static cone => cone.SolidAngle);
    public static readonly ConeProjection Axis = new(key: 2, sample: static cone => cone.Axis);
    public static readonly ConeProjection Apex = new(key: 3, sample: static cone => cone.Apex);
    [UseDelegateFromConstructor] private partial object Sample(VectorCone cone);
    internal Fin<TOut> Project<TOut>(VectorCone cone, Op key) =>
        (Sample(cone: cone), typeof(TOut)) switch {
            (VectorAngle a, Type t) when t == typeof(VectorAngle) => Fin.Succ((TOut)(object)a),
            (VectorAngle a, Type t) when t == typeof(double) => key.AcceptValue(value: a.Value).Map(static x => (TOut)(object)x),
            (double d, Type t) when t == typeof(double) => key.AcceptValue(value: d).Map(static x => (TOut)(object)x),
            (Direction d, Type t) when t == typeof(Direction) => Fin.Succ((TOut)(object)d),
            (Direction d, Type t) when t == typeof(Vector3d) => key.AcceptValue(value: d.Value).Map(static x => (TOut)(object)x),
            (Point3d p, Type t) when t == typeof(Point3d) => key.AcceptValue(value: p).Map(static x => (TOut)(object)x),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: typeof(ConeProjection), outputType: typeof(TOut))),
        };
}

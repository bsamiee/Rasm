namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CurveProjection {
    public static readonly CurveProjection Tangent = new(key: 0,
        sample: static (curve, t, _) => Fin.Succ((object)curve.TangentAt(t: t)));
    public static readonly CurveProjection Curvature = new(key: 1,
        sample: static (curve, t, _) => Fin.Succ((object)curve.CurvatureAt(t: t)));
    public static readonly CurveProjection FrenetFrame = new(key: 2,
        sample: static (curve, t, _) => curve.FrameAt(t: t, plane: out Plane p)
            ? Fin.Succ((object)p)
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    public static readonly CurveProjection BishopFrame = new(key: 3,
        sample: static (curve, t, _) => curve.PerpendicularFrameAt(t: t, plane: out Plane p)
            ? Fin.Succ((object)p)
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    public static readonly CurveProjection ArcLength = new(key: 4,
        sample: static (curve, t, context) => Fin.Succ((object)curve.GetLength(fractionalTolerance: context.Fractional, subdomain: new Interval(curve.Domain.T0, t))));
    [UseDelegateFromConstructor] private partial Fin<object> Sample(Curve curve, double parameter, Context context);
    internal Fin<TOut> Project<TOut>(Curve curve, double parameter, Context context, Op key) =>
        from _ in guard(curve.Domain.IncludesParameter(t: parameter), key.InvalidInput())
        from raw in Sample(curve: curve, parameter: parameter, context: context).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
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
        sample: static sc => sc.OsculatingCircle(0).IsValid
            ? Fin.Succ((object)sc.OsculatingCircle(0))
            : Fin.Fail<object>(Op.Of().InvalidResult()));
    public static readonly SurfaceProjection Normal = new(key: 4,
        sample: static sc => Fin.Succ((object)sc.Normal));
    [UseDelegateFromConstructor] private partial Fin<object> Sample(SurfaceCurvature curvature);
    internal Fin<TOut> Project<TOut>(Surface surface, double u, double v, Context context, Op key) =>
        from _ in guard(surface.Domain(direction: 0).IncludesParameter(t: u) && surface.Domain(direction: 1).IncludesParameter(t: v), key.InvalidInput())
        from output in surface.CurvatureAt(u: u, v: v) is SurfaceCurvature sc && sc.IsSet
            ? new Lease<SurfaceCurvature>.Owned(Value: sc).Use(curvature =>
                from raw in Sample(curvature: curvature).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
                from projected in (raw, typeof(TOut)) switch {
                    (double d, Type t) when t == typeof(double) => key.AcceptValue(value: (TOut)(object)d),
                    (Circle c, Type t) when t == typeof(Circle) => key.AcceptValue(value: (TOut)(object)c),
                    (Vector3d n, Type t) when t == typeof(Vector3d) => key.AcceptValue(value: (TOut)(object)n),
                    (Seq<double> ks, Type t) when t == typeof(Seq<double>) =>
                        ks.TraverseM(k => key.AcceptValue(value: k)).As().Map(static valid => (TOut)(object)valid),
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
    [UseDelegateFromConstructor] internal partial Fin<Plane> Interpolate(Plane a, Plane b, double t);

    private static Fin<Plane> InterpolatePlanes(Plane a, Plane b, double t, bool spherical) {
        if (a.EpsilonEquals(other: b, epsilon: RhinoMath.ZeroTolerance)) return Fin.Succ(a);
        Quaternion qa = Quaternion.Rotation(plane0: Plane.WorldXY, plane1: a);
        Quaternion qb = Quaternion.Rotation(plane0: Plane.WorldXY, plane1: b);
        Quaternion qt = spherical
            ? Quaternion.Slerp(a: qa, b: qb, t: t)
            : Quaternion.Lerp(a: qa, b: qb, t: t);
        Point3d origin = a.Origin;
        origin.Interpolate(pA: a.Origin, pB: b.Origin, t: t);
        return qt.GetRotation(plane: out Plane oriented) && oriented.IsValid
            ? Fin.Succ(new Plane(origin: origin, xDirection: oriented.XAxis, yDirection: oriented.YAxis))
            : Fin.Fail<Plane>(Op.Of().InvalidResult());
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

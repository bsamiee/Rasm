# [RASM_PARAMETRIC_PROJECTIONS]

`CurveProjection`, `SurfaceProjection`, and `ConeProjection` own Rhino-native parametric evaluation, while the host-neutral motion owners govern interpolation and provider-relative time evidence. Each projection selector drains through one `Project<TOut>` gate into `AtomProjection.Raw`, and each captured clock value remains branded to the injected `TimeProvider` timeline that produced it.

The owner laws partition shape-operator construction, quaternion rotation, scalar timing, and monotonic time. `SurfaceProjection.ShapeOperator` assembles the second fundamental form, `MotionInterpolation` derives pose and direction rotation from one quaternion-combination column, and the motion rows carry scalar policy without host UI types; perceptual colour is the `Numerics/atoms` `PerceptualColor`/`BlendPath` owner. `MonotonicTimeline` brands opaque `MonotonicStamp` values with its instance and injected provider, and its admission gate rejects foreign stamps or beats before `TimeProvider.GetElapsedTime` runs. Every fallible read stays on the `Op`-keyed `Fin<T>` rail; Rhino-read material uses the Rhino acceptance oracle, while the host-neutral timing owners use their own validity evidence.

## [01]-[INDEX]

- [02]-[SELECTORS]: `CurveProjection` (tangent/curvature/frames/arc-length), `SurfaceProjection` (curvature bundle/normal/frames/derivative forms/shape operator), `ConeProjection` (solid-angle vocabulary over `VectorCone`) — delegate-row SmartEnums with one `Project<TOut>` gate each, row-factory folds collapsing repeated construction, and the `AdmitsMagnitude` row column replacing identity probes.
- [03]-[MOTION]: `MotionInterpolation` and `SurfaceSpace` own Rhino-parametric rotation and sampling; host-neutral motion rows own shaping; `MonotonicTimeline` and `BeatSeed` brand provider timestamps and discriminate timer-sequence admission.

## [02]-[SELECTORS]

- Owner: `CurveProjection` `[SmartEnum<int>]` — a row vocabulary over one `[UseDelegateFromConstructor]` `Sample(Curve, double, Context, Op)` column plus the `AdmitsMagnitude` policy column. `Vector(key, admitsMagnitude, sample)` owns vector admission, `FrameRow(key, perpendicular, project)` owns moving and sweep-frame recovery plus axis projection, and the arc-length row owns the domain-length call.
- Owner: `SurfaceProjection` `[SmartEnum<int>]` — a row vocabulary over `Sample(Surface, Point2d, Context, Op)`. `WithCurvature` scopes every disposable `SurfaceCurvature` projection on the `Lease` rail, `Derivatives(key, project)` derives the first-fundamental forms from one `Surface.Evaluate(u, v, numberDerivatives: 1, ...)` call, and `ShapeOperator` remains the sole second-fundamental-form owner.
- Owner: `ConeProjection` `[SmartEnum<int>]` — an accessor-row vocabulary over one `Sample(VectorCone)` column. `VectorIntent.Cone(cone, mode)` carries the row as its modality discriminant, which an instance accessor on `VectorCone` cannot replace.
- Entry: each selector exposes exactly one `internal Fin<TOut> Project<TOut>(...)` gate — `CurveProjection.Project<TOut>(Curve, double, Context, Op)` admits the curve (non-null, `IsValid`, `Domain.IncludesParameter`), samples the row, and drains `AtomProjection.Raw<TOut>(raw, Some(context), key, owner: typeof(CurveProjection), admitsVectorMagnitude: AdmitsMagnitude)`; `SurfaceProjection.Project<TOut>(Surface, double u, double v, Context, Op)` admits the surface, normalizes `(u,v)` through the `Domain/evaluation` `SurfaceUv`, samples, and drains the same rail; `ConeProjection.Project<TOut>(VectorCone, Op)` drains context-free. No per-row public methods, no output-type overloads — the raw→typed step is the rail's, and the `AdmitsMagnitude` column kills the `ReferenceEquals(this, Curvature)` identity probe: magnitude admission is row data, not a hidden special case.
- Receipt: none — a selector row is a pure evaluation; the typed value IS the result, and failure evidence rides the `Op` fault (`InvalidInput` for admission refusals, `InvalidResult` for host-evaluation refusals, `Unsupported` for output-type refusals raised inside the rail).
- Packages: RhinoCommon (`Curve.TangentAt`/`CurvatureAt`/`FrameAt`/`PerpendicularFrameAt`/`GetLength(fractionalTolerance, subdomain)`/`Domain.IncludesParameter`; `Surface.CurvatureAt`/`PointAt`/`Evaluate`; `SurfaceCurvature.Kappa`/`Direction`/`OsculatingCircle`/`MaximumPrincipalCurvature`/`MinimumPrincipalCurvature`/`IsSet` — an `IDisposable` bundle; `Interval`, `Circle.IsValid`, `Vector3d.CrossProduct`/`IsValid`/`IsTiny`), Thinktecture.Runtime.Extensions (`[SmartEnum<int>]`, `[UseDelegateFromConstructor]`), LanguageExt.Core (`Fin`/`Option`/`guard`/`Optional`), `Domain/rails` (`Op`, `Lease<T>`), `Domain/context` (`Context.Fractional`/`Absolute`), `Domain/evaluation` (`NormalAt`/`FrameAt`/`SurfaceUv`), `Domain/stats` (`ScalarMetric`), `Numerics/atoms` (`AtomProjection.Raw`, `Direction.Of`, `Dimension`, `VectorCone`), `Numerics/matrix` (`SymmetricMatrix.Of`, `Matrix.Of`).
- Growth: a new curve or surface probe is one row through an existing factory fold (a `Torsion` row is `Vector(...)` over the third-derivative Frenet identity; a `MeanCurvatureVector` row is `WithCurvature` composing `Mean` with `Normal`); a new derivative form is one `Derivatives(...)` row; a new output type for an existing row is a `ProjectionRow` addition in the `Numerics/atoms` rail, never a selector edit. The existing selector gates absorb every row extension.
- Boundary: the selector family is the ONE row vocabulary for parameter-addressed evaluation behind the intent rail — a sibling `CurveEvaluator`/`SurfaceAnalyzer` method family per output is the named defect collapsed here, and a row exists exactly where evaluation carries ROW SEMANTICS (validity gating, magnitude admission, moving-vs-sweep frame choice, the curvature-bundle lease, the derivative fold); `Domain/evaluation` stays the shared derivation floor both the rows and the `Parametric/locate` location arms compose — an arm re-implementing row semantics beside the rail is the killed duplicate, while the `Parametric/locate` surface arms reading the floor directly (point/frame/normal: no row semantics, UV already normalized by the operation) are lawful composition, never a parallel rail; the shape operator is assembled HERE only — `TensorField.Curvature` composes `SurfaceProjection.ShapeOperator.Project`, and a second `k·d⊗d` assembly anywhere in the corpus is the named double-owner defect; rows sample the LIVE Rhino object under the caller's lease discipline (`Parametric/locate` runs them inside `Lease<Curve>`/`Lease<Surface>` scopes; `VectorIntent.CurveCase` holds the caller's reference) — a row never duplicates, caches, or outlives its geometry; `SurfaceCurvature` is disposable host memory, so every bundle read runs inside `Lease<SurfaceCurvature>.Owned(...).Use(...)` and a bundle escaping its row is the named leak defect; the `Domain/evaluation` lattice owns closest-point/normal/frame recovery over ARBITRARY geometry — these selectors own only parameter-addressed evaluation on an already-typed `Curve`/`Surface`, and routing a closest-point query through a selector is the altitude violation.

## [03]-[MOTION]

- Owner: `MotionInterpolation` `[SmartEnum<int>]` — `Linear` (key 0, `Quaternion.Lerp`) and `Slerp` (key 1, `Quaternion.Slerp`) over ONE `[UseDelegateFromConstructor]` `Combine(Quaternion, Quaternion, double)` column; both interpolation surfaces derive from that single column (`DERIVED_LOGIC`): `Interpolate(Plane a, Plane b, UnitInterval t, Op)` — coincidence short-circuit at `RhinoMath.ZeroTolerance`, `Quaternion.Rotation(Plane.WorldXY, …)` rotors combined then `GetRotation(out Plane)`, origin linearly interpolated onto the rotated axes — and `Rotate(Direction a, Direction b, UnitInterval t, Context, Op)` — the antiparallel pair (`IsParallelTo == -1` under `Context.Angle`) takes the π rotor about `VectorFrame.SeedPerpendicular`, every other pair the shortest-arc rotor from `Transform.Rotation(...).GetQuaternion(...)`, combined from `Quaternion.Identity` and applied via `Quaternion.Rotate`. `Slerp` is the geodesic row; `Linear` yields nlerp on directions (renormalized by `Direction.Of` admission) and screw-free frame lerp on poses.
- Owner: `SurfaceSpace` `[BoundaryAdapter]` `readonly record struct` — the validated `Surface` + `Context` capsule: `Of(Surface, Context, Op?)` admits once (context present, surface non-null and `IsValid`) and `Sample<TOut>(SurfaceProjection, double u, double v, Op)` delegates to the selector gate with the captured tolerance — an internal kernel, so the key is required by the `Domain/rails` threading law and the `Processing/intent` `surfaceCase` supplies it. Re-homed from the proximity file to its parametric family: `Spatial/support` keeps `SupportSpace` (closest-point over ANY geometry); `SurfaceSpace` is parameter-addressed evaluation on a typed surface — different concern, different folder, same wire (`VectorIntent.SurfaceCase` carries it).
- Owner: `Easing` `[SmartEnum<int>]` — a family-and-polarity row product over one `[UseDelegateFromConstructor]` `Curve(double)` column. Each named row composes a family kernel with `In`, `Out`, or `InOut`, so polarity behavior remains fold-owned. `Evaluate(UnitInterval t)` is the one read: input arrives admitted, output is unclamped because overshooting kernels legitimately leave the unit band and the consumer's carrier owns its own range semantics.
- Owner: `CyclePlan` `[BoundaryAdapter]` readonly record struct — repeat/yoyo phase arithmetic: `Of(Option<int> count, bool yoyo)` admits the plan (`None` count is unbounded, a bounded count is ≥ 1), and `Phase(elapsed, period, key)` folds wall progress onto the `CyclePhase` evidence — iteration index, mirrored-local `UnitInterval` position (odd iterations reverse under yoyo), and the completion flag that clamps a bounded plan onto its terminal pose instead of wrapping past it.
- Owner: `SpringShape` `[BoundaryAdapter]` readonly record struct — the analytic damped-spring owner: `Of(angularFrequency > 0, dampingRatio ≥ 0)` admits the shape, `Evaluate(SpringState from, target, elapsed, key)` returns the closed-form response with the regime selected by ζ (underdamped through the damped-frequency rotor, critically damped at `|ζ−1| ≤ EpsilonPolicy.SqrtEpsilon`, overdamped through the two real decay rates), and `Step(from, target, h, integrator, key)` runs ONE `FieldIntegrator.Step` over the page-declared `SpringShape.Module` (`IntegrationModule<SpringState, SpringState>` — position/velocity as their own delta algebra, max-norm error) for interactive driven targets where the closed form's fixed target does not hold between frames. `SpringState` carries position and velocity as one evidence value.
- Owner: `MonotonicTimeline` sealed `[BoundaryAdapter]` service — `Of(TimeProvider, Op?)` admits an injected provider with a positive `TimestampFrequency`. Each timeline instance is its own identity token; serialized `Capture` mints an opaque `MonotonicStamp`, `Elapsed` derives a non-negative interval, `Order` returns negative, zero, or positive for left-before, identical, or left-after ordering, and `Beat` derives ordinal timer evidence from one `BeatSeed`. `Order` breaks equal-provider-tick ties by the stamp's private capture ordinal; every duration still calls the capturing provider's `GetElapsedTime`, and no counter or ordinal leaves `MonotonicStamp`.
- Owner: `BeatSeed` `[Union<MonotonicStamp, MonotonicBeat>]` — `Origin` starts an independently branded sequence and `Previous` advances only that sequence's current tail. The generated case probes reject the struct's default ghost before the total `Switch` owns both modalities, and the atomic tail gate refuses replayed or concurrently substituted predecessors.
- Receipt: `MonotonicBeat` exposes immutable `Ordinal`, `Stamp`, `Elapsed`, and `Delta` evidence. Its `ValidityClaim.All` fold requires one timeline, an origin-bound sequence brand, non-negative intervals, monotone elapsed time, and first-beat delta equality; the private origin and sequence brands prevent chain mixing.
- Entry: `MotionInterpolation.Interpolate`/`Rotate` stay internal to the intent dispatch, while `Easing.Evaluate`, `CyclePlan.Phase`, `SpringShape.Evaluate`/`Step`, and `MonotonicTimeline.Of`/`Capture`/`Elapsed`/`Order`/`Beat` form the public motion-time surface; perceptual colour interpolation is the `Numerics/atoms` `PerceptualColor.Mix`/`Ramp` surface over `BlendPath` rows, never a motion-page sibling. Each fallible operation accepts or resolves one `Op` key and returns `Fin<T>`.
- Packages: `TimeProvider` supplies monotonic timestamps and provider-defined elapsed conversion; Thinktecture owns generated `[SmartEnum]` and `[Union]` vocabularies; Foundation analyzer contracts own `[BoundaryAdapter]`; LanguageExt owns `Fin`, `Option`, and guards; RhinoCommon owns rotor and parametric evaluation; `FieldIntegrator` owns driven stepping.
- Growth: [CONSUMER] `MonotonicTimeline` supplies session latency, UI-event ordering, and timer beats through the same branded evidence; a new clock consumer composes that evidence without a host-local counter. A new easing family is one kernel folded through the existing polarities; a new colour interpolation path is one `BlendPath` row on `Numerics/atoms`, never a motion-page policy sibling.
- Boundary: `MotionInterpolation` starts where rotation requires a quaternion; vector arithmetic remains on the admitted direction algebra, and host colour conversion remains at the paint edge. `MonotonicTimeline` admits reference identity with the capturing timeline and provider before any `GetElapsedTime` call, and each beat sequence atomically admits only its current tail, so foreign timestamps and replayed predecessors never enter accepted timing evidence.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Csp;
using Rasm.Domain;
using Rasm.Numerics;
using Rhino;

namespace Rasm.Parametric;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class CurveProjection {
    public static readonly CurveProjection Tangent = Vector(key: 0, admitsMagnitude: false, sample: static (curve, t) => curve.TangentAt(t: t));
    public static readonly CurveProjection Curvature = Vector(key: 1, admitsMagnitude: true, sample: static (curve, t) => curve.CurvatureAt(t: t));
    public static readonly CurveProjection Frame = FrameRow(key: 2, perpendicular: false, project: static frame => frame);
    public static readonly CurveProjection PerpendicularFrame = FrameRow(key: 3, perpendicular: true, project: static frame => frame);
    public static readonly CurveProjection ArcLength = new(key: 4, admitsMagnitude: false,
        sample: static (curve, t, context, key) => curve.GetLength(fractionalTolerance: context.Fractional, subdomain: new Interval(curve.Domain.T0, t)) switch {
            double length when RhinoMath.IsValidDouble(x: length) && (length > 0.0 || curve.Domain.NormalizedParameterAt(t) <= context.Fractional) => Fin.Succ((object)length),
            _ => Fin.Fail<object>(key.InvalidResult()),
        });
    public static readonly CurveProjection FrameNormal = FrameRow(key: 5, perpendicular: false, project: static frame => frame.YAxis);
    public static readonly CurveProjection FrameBinormal = FrameRow(key: 6, perpendicular: false, project: static frame => frame.ZAxis);
    public static readonly CurveProjection PerpendicularNormal = FrameRow(key: 7, perpendicular: true, project: static frame => frame.YAxis);
    public static readonly CurveProjection PerpendicularBinormal = FrameRow(key: 8, perpendicular: true, project: static frame => frame.ZAxis);

    public bool AdmitsMagnitude { get; }
    [UseDelegateFromConstructor] private partial Fin<object> Sample(Curve curve, double parameter, Context context, Op key);

    internal Fin<TOut> Project<TOut>(Curve curve, double parameter, Context context, Op key) =>
        from active in Optional(curve).ToFin(key.InvalidInput())
        from _ in guard(active.IsValid && active.Domain.IncludesParameter(t: parameter), key.InvalidInput())
        from raw in Sample(curve: active, parameter: parameter, context: context, key: key).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
        from output in AtomProjection.Raw<TOut>(raw: raw, context: Some(context), key: key, owner: typeof(CurveProjection), admitsVectorMagnitude: AdmitsMagnitude)
        select output;

    private static CurveProjection Vector(int key, bool admitsMagnitude, Func<Curve, double, Vector3d> sample) =>
        new(key: key, admitsMagnitude: admitsMagnitude, sample: (curve, t, _, op) => sample(arg1: curve, arg2: t) switch {
            Vector3d vector when vector.IsValid && (admitsMagnitude || !vector.IsTiny()) => Fin.Succ((object)vector),
            _ => Fin.Fail<object>(op.InvalidResult()),
        });
    private static CurveProjection FrameRow(int key, bool perpendicular, Func<Plane, object> project) =>
        new(key: key, admitsMagnitude: false, sample: (curve, t, _, op) => perpendicular switch {
            true => curve.PerpendicularFrameAt(t: t, plane: out Plane frame) ? Fin.Succ(project(arg: frame)) : Fin.Fail<object>(op.InvalidResult()),
            false => curve.FrameAt(t: t, plane: out Plane frame) ? Fin.Succ(project(arg: frame)) : Fin.Fail<object>(op.InvalidResult()),
        });
}

[SmartEnum<int>]
public sealed partial class SurfaceProjection {
    public static readonly SurfaceProjection PrincipalCurvatures = new(key: 0, sample: static (surface, uv, _, key) => WithCurvature(surface: surface, uv: uv, key: key, project: static sc => Fin.Succ((object)Seq(sc.MaximumPrincipalCurvature, sc.MinimumPrincipalCurvature))));
    public static readonly SurfaceProjection Gaussian = new(key: 1, sample: static (surface, uv, _, key) => WithCurvature(surface: surface, uv: uv, key: key, project: sc => ScalarMetric.Gaussian.Of(value: sc, key: key).Map(static value => (object)value)));
    public static readonly SurfaceProjection Mean = new(key: 2, sample: static (surface, uv, _, key) => WithCurvature(surface: surface, uv: uv, key: key, project: sc => ScalarMetric.Mean.Of(value: sc, key: key).Map(static value => (object)value)));
    public static readonly SurfaceProjection MaximumOsculatingCircle = Osculating(key: 3, direction: 0);
    public static readonly SurfaceProjection Normal = new(key: 4, sample: static (surface, uv, _, key) => Evaluation.NormalAt(surface: surface, uv: uv, key: key).Map(static normal => (object)normal));
    public static readonly SurfaceProjection ShapeOperator = new(key: 5, sample: static (surface, uv, context, key) => WithCurvature(surface: surface, uv: uv, key: key, project: sc => ShapeOperatorOf(curvature: sc, context: context, key: key).Map(static value => (object)value)));
    public static readonly SurfaceProjection MinimumOsculatingCircle = Osculating(key: 6, direction: 1);
    public static readonly SurfaceProjection Point = new(key: 7, sample: static (surface, uv, _, key) => key.AcceptValue(value: surface.PointAt(u: uv.X, v: uv.Y)).Map(static point => (object)point));
    public static readonly SurfaceProjection Frame = new(key: 8, sample: static (surface, uv, _, key) => Evaluation.FrameAt(surface: surface, uv: uv, key: key).Map(static value => (object)value));
    public static readonly SurfaceProjection UvFrame = Derivatives(key: 9, project: static (surface, uv, d, _, key) => OrientedFrame(surface: surface, uv: uv, frame: new Plane(origin: d.Point, xDirection: d.Du, yDirection: d.Dv), key: key).Map(static value => (object)value));
    public static readonly SurfaceProjection Jacobian = Derivatives(key: 10, project: static (_, _, d, _, key) => Matrix.Of(rows: Dimension.Create(value: 3), cols: Dimension.Create(value: 2), entries: [d.Du.X, d.Dv.X, d.Du.Y, d.Dv.Y, d.Du.Z, d.Dv.Z], key: key).Map(static value => (object)value));
    public static readonly SurfaceProjection Metric = Derivatives(key: 11, project: static (_, _, d, _, key) => SymmetricMatrix.Of(dim: Dimension.Create(value: 2), upper: [d.Du * d.Du, d.Du * d.Dv, d.Dv * d.Dv], key: key).Map(static value => (object)value));
    public static readonly SurfaceProjection AreaScale = Derivatives(key: 12, project: static (_, _, d, _, key) => key.AcceptValue(value: Vector3d.CrossProduct(a: d.Du, b: d.Dv).Length).Map(static value => (object)value));

    [UseDelegateFromConstructor] private partial Fin<object> Sample(Surface surface, Point2d uv, Context context, Op key);

    internal Fin<TOut> Project<TOut>(Surface surface, double u, double v, Context context, Op key) =>
        from active in Optional(surface).ToFin(key.InvalidInput())
        from _ in guard(active.IsValid, key.InvalidInput())
        from uv in Evaluation.SurfaceUv(surface: active, uv: new Point2d(x: u, y: v), context: context, key: key)
        from raw in Sample(surface: active, uv: uv, context: context, key: key).BindFail(_ => Fin.Fail<object>(key.InvalidResult()))
        from output in AtomProjection.Raw<TOut>(raw: raw, context: Some(context), key: key, owner: typeof(SurfaceProjection))
        select output;

    private static Fin<T> WithCurvature<T>(Surface surface, Point2d uv, Op key, Func<SurfaceCurvature, Fin<T>> project) =>
        Optional(surface.CurvatureAt(u: uv.X, v: uv.Y)).ToFin(key.InvalidResult())
            .Bind(sc => new Lease<SurfaceCurvature>.Owned(Value: sc)
                .Use(bundle => bundle.IsSet ? project(arg: bundle) : Fin.Fail<T>(key.InvalidResult())));

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
    private static Fin<(Point3d Point, Vector3d Du, Vector3d Dv)> SurfaceDerivatives(Surface surface, Point2d uv, Op key) =>
        surface.Evaluate(u: uv.X, v: uv.Y, numberDerivatives: 1, point: out Point3d point, derivatives: out Vector3d[] derivatives)
        && derivatives is { Length: >= 2 }
            ? from validPoint in key.AcceptValue(value: point)
              from du in key.AcceptValue(value: derivatives[0])
              from dv in key.AcceptValue(value: derivatives[1])
              select (Point: validPoint, Du: du, Dv: dv)
            : Fin.Fail<(Point3d Point, Vector3d Du, Vector3d Dv)>(key.InvalidResult());
    private static Fin<Plane> OrientedFrame(Surface surface, Point2d uv, Plane frame, Op key) =>
        from basis in Admit.Plane(basis: frame, key: key)
        from normal in Evaluation.NormalAt(surface: surface, uv: uv, key: key)
        from oriented in Admit.Plane(
            basis: basis.ZAxis * normal >= 0.0 ? basis : new Plane(origin: basis.Origin, xDirection: basis.XAxis, yDirection: -basis.YAxis),
            key: key)
        select oriented;
    private static SurfaceProjection Osculating(int key, int direction) =>
        new(key: key, sample: (surface, uv, _, op) => WithCurvature(surface: surface, uv: uv, key: op, project: sc => sc.OsculatingCircle(direction) switch {
            Circle circle when circle.IsValid => Fin.Succ((object)circle),
            _ => Fin.Fail<object>(op.InvalidResult()),
        }));
    private static SurfaceProjection Derivatives(int key, Func<Surface, Point2d, (Point3d Point, Vector3d Du, Vector3d Dv), Context, Op, Fin<object>> project) =>
        new(key: key, sample: (surface, uv, context, op) => SurfaceDerivatives(surface: surface, uv: uv, key: op).Bind(d => project(arg1: surface, arg2: uv, arg3: d, arg4: context, arg5: op)));
}

[SmartEnum<int>]
public sealed partial class ConeProjection {
    public static readonly ConeProjection HalfAngle = new(key: 0, sample: static cone => cone.HalfAngle);
    public static readonly ConeProjection SolidAngle = new(key: 1, sample: static cone => cone.SolidAngle);
    public static readonly ConeProjection Axis = new(key: 2, sample: static cone => cone.Axis);
    public static readonly ConeProjection Apex = new(key: 3, sample: static cone => cone.Apex);
    [UseDelegateFromConstructor] private partial object Sample(VectorCone cone);
    internal Fin<TOut> Project<TOut>(VectorCone cone, Op key) =>
        AtomProjection.Raw<TOut>(raw: Sample(cone: cone), context: Option<Context>.None, key: key, owner: typeof(ConeProjection));
}

[SmartEnum<int>]
public sealed partial class MotionInterpolation {
    public static readonly MotionInterpolation Linear = new(key: 0, combine: static (a, b, t) => Quaternion.Lerp(a: a, b: b, t: t));
    public static readonly MotionInterpolation Slerp = new(key: 1, combine: static (a, b, t) => Quaternion.Slerp(a: a, b: b, t: t));
    [UseDelegateFromConstructor] private partial Quaternion Combine(Quaternion a, Quaternion b, double t);

    internal Fin<Plane> Interpolate(Plane a, Plane b, UnitInterval t, Op key) =>
        from left in Admit.Plane(basis: a, key: key)
        from right in Admit.Plane(basis: b, key: key)
        from output in left.EpsilonEquals(other: right, epsilon: RhinoMath.ZeroTolerance)
            ? Fin.Succ(left)
            : Combine(a: Quaternion.Rotation(plane0: Plane.WorldXY, plane1: left), b: Quaternion.Rotation(plane0: Plane.WorldXY, plane1: right), t: t.Value)
                  .GetRotation(plane: out Plane oriented) && oriented.IsValid
                ? Admit.Plane(basis: new Plane(origin: left.Origin + ((right.Origin - left.Origin) * t.Value), xDirection: oriented.XAxis, yDirection: oriented.YAxis), key: key)
                : Fin.Fail<Plane>(key.InvalidResult())
        select output;

    internal Fin<Direction> Rotate(Direction a, Direction b, UnitInterval t, Context context, Op key) =>
        from rotor in a.Value.IsParallelTo(other: b.Value, angleTolerance: context.Angle.Value) switch {
            -1 => Fin.Succ(Quaternion.Rotation(Math.PI, VectorFrame.SeedPerpendicular(axis: a.Value))),
            _ => Transform.Rotation(startDirection: a.Value, endDirection: b.Value, rotationCenter: Point3d.Origin).GetQuaternion(quaternion: out Quaternion target)
                ? Fin.Succ(target)
                : Fin.Fail<Quaternion>(key.InvalidResult()),
        }
        from rotated in Direction.Of(value: Combine(a: Quaternion.Identity, b: rotor, t: t.Value).Rotate(v: a.Value), context: context, key: key)
        select rotated;
}

[SmartEnum<int>]
public sealed partial class Easing {
    public static readonly Easing Linear = new(key: 0, curve: static t => t);
    public static readonly Easing QuadIn = In(key: 1, family: Power(exponent: 2.0));
    public static readonly Easing QuadOut = Out(key: 2, family: Power(exponent: 2.0));
    public static readonly Easing QuadInOut = InOut(key: 3, family: Power(exponent: 2.0));
    public static readonly Easing CubicIn = In(key: 4, family: Power(exponent: 3.0));
    public static readonly Easing CubicOut = Out(key: 5, family: Power(exponent: 3.0));
    public static readonly Easing CubicInOut = InOut(key: 6, family: Power(exponent: 3.0));
    public static readonly Easing QuintIn = In(key: 7, family: Power(exponent: 5.0));
    public static readonly Easing QuintOut = Out(key: 8, family: Power(exponent: 5.0));
    public static readonly Easing QuintInOut = InOut(key: 9, family: Power(exponent: 5.0));
    public static readonly Easing SineIn = In(key: 10, family: Sine);
    public static readonly Easing SineOut = Out(key: 11, family: Sine);
    public static readonly Easing SineInOut = InOut(key: 12, family: Sine);
    public static readonly Easing ExpoIn = In(key: 13, family: Expo);
    public static readonly Easing ExpoOut = Out(key: 14, family: Expo);
    public static readonly Easing ExpoInOut = InOut(key: 15, family: Expo);
    public static readonly Easing CircIn = In(key: 16, family: Circ);
    public static readonly Easing CircOut = Out(key: 17, family: Circ);
    public static readonly Easing CircInOut = InOut(key: 18, family: Circ);
    public static readonly Easing BackIn = In(key: 19, family: Back(overshoot: 1.70158));
    public static readonly Easing BackOut = Out(key: 20, family: Back(overshoot: 1.70158));
    public static readonly Easing BackInOut = InOut(key: 21, family: Back(overshoot: 1.70158));
    public static readonly Easing ElasticIn = In(key: 22, family: Elastic(amplitude: 1.0, period: 0.3));
    public static readonly Easing ElasticOut = Out(key: 23, family: Elastic(amplitude: 1.0, period: 0.3));
    public static readonly Easing ElasticInOut = InOut(key: 24, family: Elastic(amplitude: 1.0, period: 0.3));
    public static readonly Easing BounceIn = In(key: 25, family: Bounce);
    public static readonly Easing BounceOut = Out(key: 26, family: Bounce);
    public static readonly Easing BounceInOut = InOut(key: 27, family: Bounce);

    [UseDelegateFromConstructor] private partial double Curve(double t);
    public double Evaluate(UnitInterval t) => Curve(t: t.Value);

    private static Easing In(int key, Func<double, double> family) => new(key: key, curve: family);
    private static Easing Out(int key, Func<double, double> family) => new(key: key, curve: t => 1.0 - family(arg: 1.0 - t));
    private static Easing InOut(int key, Func<double, double> family) =>
        new(key: key, curve: t => t < 0.5 ? family(arg: 2.0 * t) / 2.0 : 1.0 - (family(arg: 2.0 - (2.0 * t)) / 2.0));
    private static Func<double, double> Power(double exponent) => t => Math.Pow(x: t, y: exponent);
    private static double Sine(double t) => 1.0 - Math.Cos(d: t * Math.PI / 2.0);
    private static double Expo(double t) => t <= 0.0 ? 0.0 : Math.Pow(x: 2.0, y: 10.0 * (t - 1.0));
    private static double Circ(double t) => 1.0 - Math.Sqrt(d: 1.0 - (t * t));
    private static Func<double, double> Back(double overshoot) => t => t * t * (((overshoot + 1.0) * t) - overshoot);
    private static Func<double, double> Elastic(double amplitude, double period) => t => t switch {
        <= 0.0 => 0.0,
        >= 1.0 => 1.0,
        _ => -(amplitude * Math.Pow(x: 2.0, y: 10.0 * (t - 1.0)) * Math.Sin(a: ((t - 1.0) - (period / (2.0 * Math.PI) * Math.Asin(d: 1.0 / amplitude))) * (2.0 * Math.PI) / period)),
    };
    private static double Bounce(double t) => 1.0 - BounceTail(t: 1.0 - t);
    private static double BounceTail(double t) => t switch {
        < 1.0 / 2.75 => 7.5625 * t * t,
        < 2.0 / 2.75 => (7.5625 * (t - (1.5 / 2.75)) * (t - (1.5 / 2.75))) + 0.75,
        < 2.5 / 2.75 => (7.5625 * (t - (2.25 / 2.75)) * (t - (2.25 / 2.75))) + 0.9375,
        _ => (7.5625 * (t - (2.625 / 2.75)) * (t - (2.625 / 2.75))) + 0.984375,
    };
}

[BoundaryAdapter]
[Union<MonotonicStamp, MonotonicBeat>(T1Name = "Origin", T2Name = "Previous")]
public readonly partial struct BeatSeed;

[BoundaryAdapter]
public sealed class MonotonicTimeline {
    private readonly TimeProvider _provider;
    private readonly object _captureGate = new();
    private long _nextCaptureOrdinal;
    private bool _captureExhausted;

    private MonotonicTimeline(TimeProvider provider) => _provider = provider;

    public static Fin<MonotonicTimeline> Of(TimeProvider provider, Op? key = null) {
        Op op = key.OrDefault();
        return from active in op.Need(value: provider)
               from admitted in op.Catch(body: () => active.TimestampFrequency > 0
                   ? Fin.Succ(active)
                   : Fin.Fail<TimeProvider>(op.InvalidInput()))
               select new MonotonicTimeline(provider: admitted);
    }

    public Fin<MonotonicStamp> Capture(Op? key = null) {
        Op op = key.OrDefault();
        return op.Catch(body: () => {
            lock (_captureGate) {
                if (_captureExhausted) return Fin.Fail<MonotonicStamp>(op.InvalidResult());
                long timestamp = _provider.GetTimestamp();
                long ordinal = _nextCaptureOrdinal;
                if (ordinal == long.MaxValue) _captureExhausted = true;
                else _nextCaptureOrdinal = ordinal + 1L;
                return Fin.Succ(new MonotonicStamp(timeline: this, provider: _provider, timestamp: timestamp, captureOrdinal: ordinal));
            }
        });
    }

    public Fin<TimeSpan> Elapsed(MonotonicStamp start, MonotonicStamp end, Op? key = null) {
        Op op = key.OrDefault();
        return from left in Admit(stamp: start, key: op)
               from right in Admit(stamp: end, key: op)
               from elapsed in Span(start: left, end: right, key: op).Bind(span => Nonnegative(span: span, key: op))
               select elapsed;
    }

    public Fin<int> Order(MonotonicStamp left, MonotonicStamp right, Op? key = null) {
        Op op = key.OrDefault();
        return from first in Admit(stamp: left, key: op)
               from second in Admit(stamp: right, key: op)
               from delta in Span(start: first, end: second, key: op)
               select delta == TimeSpan.Zero ? first.CompareCapture(other: second) : TimeSpan.Zero.CompareTo(delta);
    }

    public Fin<MonotonicBeat> Beat(BeatSeed seed, Op? key = null) {
        Op op = key.OrDefault();
        Fin<BeatSeed> activeSeed = seed.IsOrigin || seed.IsPrevious
            ? Fin.Succ(seed)
            : Fin.Fail<BeatSeed>(op.InvalidInput());
        Fin<(MonotonicStamp Origin, Option<MonotonicBeat> Previous, BeatSequence Sequence)> cursor = activeSeed.Bind(active => active.Switch(
            state: (Timeline: this, Key: op),
            origin: static (state, origin) => state.Timeline.Admit(stamp: origin, key: state.Key)
                .Map(static admitted => (Origin: admitted, Previous: Option<MonotonicBeat>.None, Sequence: new BeatSequence(origin: admitted))),
            previous: static (state, previous) => state.Timeline.Admit(beat: previous, key: state.Key)
                .Map(static admitted => (Origin: admitted.Origin, Previous: Some(admitted), Sequence: admitted.Sequence))));
        return from admitted in cursor
               let start = admitted.Origin
               let prior = admitted.Previous
               from current in Capture(key: op)
               from elapsed in Span(start: start, end: current, key: op).Bind(span => Nonnegative(span: span, key: op))
               from delta in prior.Match(
                   Some: beat => Span(start: beat.Stamp, end: current, key: op).Bind(span => Nonnegative(span: span, key: op)),
                   None: () => Fin.Succ(elapsed))
               from ordered in guard(prior.Map(beat => elapsed >= beat.Elapsed && delta <= elapsed).IfNone(noneValue: true), op.InvalidResult()).ToFin()
               let expected = prior.Map(static beat => beat.Stamp).IfNone(start)
               from ordinal in admitted.Sequence.Advance(expected: expected, current: current, key: op)
               from receipt in op.AcceptValue(value: new MonotonicBeat(ordinal: ordinal, origin: start, sequence: admitted.Sequence, stamp: current, elapsed: elapsed, delta: delta))
               select receipt;
    }

    private Fin<MonotonicStamp> Admit(MonotonicStamp stamp, Op key) =>
        from active in key.Need(value: stamp)
        from owned in guard(active.IsValid && active.BelongsTo(timeline: this), key.InvalidInput()).ToFin()
        select active;

    private Fin<MonotonicBeat> Admit(MonotonicBeat beat, Op key) =>
        from active in key.Need(value: beat)
        from valid in guard(active.IsValid, key.InvalidInput()).ToFin()
        from origin in Admit(stamp: active.Origin, key: key)
        from stamp in Admit(stamp: active.Stamp, key: key)
        select active;

    internal bool Owns(TimeProvider provider) => ReferenceEquals(objA: _provider, objB: provider);

    private Fin<TimeSpan> Span(MonotonicStamp start, MonotonicStamp end, Op key) =>
        start.SpanTo(end: end, key: key);

    private static Fin<TimeSpan> Nonnegative(TimeSpan span, Op key) =>
        span >= TimeSpan.Zero ? Fin.Succ(span) : Fin.Fail<TimeSpan>(key.InvalidResult());
}

// --- [MODELS] -------------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SurfaceSpace {
    private SurfaceSpace(Surface native, Context tolerance) { Native = native; Tolerance = tolerance; }
    public Surface Native { get; }
    public Context Tolerance { get; }
    public static Fin<SurfaceSpace> Of(Surface native, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from ctx in Optional(context).ToFin(op.MissingContext())
               from active in Optional(native).Filter(static surface => surface.IsValid).ToFin(op.InvalidInput())
               select new SurfaceSpace(native: active, tolerance: ctx);
    }
    internal Fin<TOut> Sample<TOut>(SurfaceProjection projection, double u, double v, Op key) {
        (Surface native, Context tolerance) = (Native, Tolerance);
        return Optional(projection).ToFin(key.InvalidInput()).Bind(mode => mode.Project<TOut>(surface: native, u: u, v: v, context: tolerance, key: key));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CyclePhase(long Iteration, UnitInterval Local, bool Reversed, bool Completed) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(ValidityClaim.Of(holds: Iteration >= 0), ValidityClaim.UnitInterval(value: Local.Value));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct CyclePlan(Option<int> Count, bool Yoyo) {
    public static Fin<CyclePlan> Of(Option<int> count, bool yoyo, Op? key = null) {
        Op op = key.OrDefault();
        return count.Match(
            Some: bounded => guard(bounded >= 1, op.InvalidInput()).ToFin().Map(_ => new CyclePlan(Count: Some(bounded), Yoyo: yoyo)),
            None: () => Fin.Succ(new CyclePlan(Count: Option<int>.None, Yoyo: yoyo)));
    }
    public Fin<CyclePhase> Phase(double elapsed, double period, Op key) {
        CyclePlan plan = this;
        return from time in key.Finite(value: elapsed).Bind(value => guard(value >= 0.0, key.InvalidInput()).ToFin().Map(_ => value))
               from span in key.Positive(value: period)
               from progress in key.AcceptValue(value: time / span)
               let completed = plan.Count.Filter(bounded => progress >= bounded)
               from iteration in completed.Match(
                   Some: bounded => Fin.Succ((long)bounded - 1L),
                   None: () => guard(Math.Floor(d: progress) < long.MaxValue, key.InvalidResult()).ToFin()
                       .Map(_ => checked((long)Math.Floor(d: progress))))
               let local = completed.IsSome ? 1.0 : progress - iteration
               let reversed = plan.Yoyo && (iteration % 2L) == 1L
               from phase in key.AcceptValidated<UnitInterval>(candidate: reversed ? 1.0 - local : local)
                   .Map(admitted => new CyclePhase(Iteration: iteration, Local: admitted, Reversed: reversed, Completed: completed.IsSome))
               select phase;
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpringState(double Position, double Velocity) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(ValidityClaim.Of(holds: double.IsFinite(Position)), ValidityClaim.Of(holds: double.IsFinite(Velocity)));
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SpringShape(double AngularFrequency, double DampingRatio) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(ValidityClaim.Of(holds: double.IsFinite(AngularFrequency) && AngularFrequency > 0.0), ValidityClaim.Of(holds: double.IsFinite(DampingRatio) && DampingRatio >= 0.0));

    public static IntegrationModule<SpringState, SpringState> Module { get; } = new(
        Add: static (state, h, delta) => new SpringState(Position: state.Position + (h * delta.Position), Velocity: state.Velocity + (h * delta.Velocity)),
        Scale: static (factor, delta) => new SpringState(Position: factor * delta.Position, Velocity: factor * delta.Velocity),
        Sum: static (left, right) => new SpringState(Position: left.Position + right.Position, Velocity: left.Velocity + right.Velocity),
        Norm: static delta => Math.Max(val1: Math.Abs(value: delta.Position), val2: Math.Abs(value: delta.Velocity)),
        Zero: new SpringState(Position: 0.0, Velocity: 0.0));

    public static Fin<SpringShape> Of(double angularFrequency, double dampingRatio, Op? key = null) {
        Op op = key.OrDefault();
        return from omega in op.Positive(value: angularFrequency)
               from zeta in op.Finite(value: dampingRatio).Bind(value => guard(value >= 0.0, op.InvalidInput()).ToFin().Map(_ => value))
               select new SpringShape(AngularFrequency: omega, DampingRatio: zeta);
    }

    public Fin<SpringState> Evaluate(SpringState from, double target, double elapsed, Op key) {
        (double omega, double zeta) = (AngularFrequency, DampingRatio);
        return from time in key.Finite(value: elapsed).Bind(value => guard(value >= 0.0, key.InvalidInput()).ToFin().Map(_ => value))
               from goal in key.Finite(value: target)
               from settled in key.AcceptValue(value: Math.Abs(value: zeta - 1.0) <= EpsilonPolicy.SqrtEpsilon
                   ? Critical(from: from, target: goal, omega: omega, t: time)
                   : zeta < 1.0
                       ? Underdamped(from: from, target: goal, omega: omega, zeta: zeta, t: time)
                       : Overdamped(from: from, target: goal, omega: omega, zeta: zeta, t: time))
               from valid in guard(settled.IsValid, key.InvalidResult()).ToFin().Map(_ => settled)
               select valid;
    }

    public Fin<IntegrationStep<SpringState, SpringState>> Step(SpringState from, double target, double h, FieldIntegrator integrator, Op key) {
        (double omega, double zeta) = (AngularFrequency, DampingRatio);
        return from active in FieldIntegrator.Admit(value: integrator, key: key)
               from step in active.Step(
                   module: Module,
                   sample: state => key.AcceptValue(value: new SpringState(
                       Position: state.Velocity,
                       Velocity: -(2.0 * zeta * omega * state.Velocity) - (omega * omega * (state.Position - target)))),
                   state: from,
                   h: h,
                   key: key)
               select step;
    }

    private static SpringState Underdamped(SpringState from, double target, double omega, double zeta, double t) {
        double damped = omega * Math.Sqrt(d: 1.0 - (zeta * zeta));
        double a = from.Position - target;
        double b = (from.Velocity + (zeta * omega * a)) / damped;
        double decay = Math.Exp(d: -zeta * omega * t);
        double cos = Math.Cos(d: damped * t);
        double sin = Math.Sin(a: damped * t);
        return new SpringState(
            Position: target + (decay * ((a * cos) + (b * sin))),
            Velocity: (decay * (((b * damped) - (a * zeta * omega)) * cos - ((a * damped) + (b * zeta * omega)) * sin)));
    }
    private static SpringState Critical(SpringState from, double target, double omega, double t) {
        double a = from.Position - target;
        double b = from.Velocity + (omega * a);
        double decay = Math.Exp(d: -omega * t);
        return new SpringState(
            Position: target + (decay * (a + (b * t))),
            Velocity: decay * (b - (omega * (a + (b * t)))));
    }
    private static SpringState Overdamped(SpringState from, double target, double omega, double zeta, double t) {
        double root = omega * Math.Sqrt(d: (zeta * zeta) - 1.0);
        double slow = (-zeta * omega) + root;
        double fast = (-zeta * omega) - root;
        double a = from.Position - target;
        double c2 = ((slow * a) - from.Velocity) / (slow - fast);
        double c1 = a - c2;
        return new SpringState(
            Position: target + (c1 * Math.Exp(d: slow * t)) + (c2 * Math.Exp(d: fast * t)),
            Velocity: (c1 * slow * Math.Exp(d: slow * t)) + (c2 * fast * Math.Exp(d: fast * t)));
    }
}

[BoundaryAdapter]
public sealed class MonotonicStamp : IValidityEvidence {
    private readonly MonotonicTimeline _timeline;
    private readonly TimeProvider _provider;
    private readonly long _timestamp;
    private readonly long _captureOrdinal;

    internal MonotonicStamp(MonotonicTimeline timeline, TimeProvider provider, long timestamp, long captureOrdinal) {
        _timeline = timeline;
        _provider = provider;
        _timestamp = timestamp;
        _captureOrdinal = captureOrdinal;
    }

    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: _timeline is not null),
        ValidityClaim.Of(holds: _provider is not null),
        ValidityClaim.Of(holds: _captureOrdinal >= 0L));
    internal bool BelongsTo(MonotonicTimeline timeline) =>
        ReferenceEquals(objA: _timeline, objB: timeline) && timeline.Owns(provider: _provider);
    internal bool SharesTimeline(MonotonicStamp other) =>
        ReferenceEquals(objA: _timeline, objB: other._timeline) && ReferenceEquals(objA: _provider, objB: other._provider);
    internal int CompareCapture(MonotonicStamp other) => _captureOrdinal.CompareTo(value: other._captureOrdinal);
    internal Fin<TimeSpan> SpanTo(MonotonicStamp end, Op key) =>
        from active in key.Need(value: end)
        from owned in guard(IsValid && active.IsValid && SharesTimeline(other: active), key.InvalidInput()).ToFin()
        from elapsed in key.Catch(body: () => Fin.Succ(_provider.GetElapsedTime(startingTimestamp: _timestamp, endingTimestamp: active._timestamp)))
        select elapsed;
}

[BoundaryAdapter]
internal sealed class BeatSequence {
    private readonly object _gate = new();
    private readonly MonotonicStamp _origin;
    private MonotonicStamp _tail;
    private long _nextOrdinal;
    private bool _exhausted;

    internal BeatSequence(MonotonicStamp origin) {
        _origin = origin;
        _tail = origin;
    }

    internal bool BelongsTo(MonotonicStamp origin) => ReferenceEquals(objA: _origin, objB: origin);

    internal Fin<long> Advance(MonotonicStamp expected, MonotonicStamp current, Op key) {
        lock (_gate) {
            if (_exhausted) return Fin.Fail<long>(key.InvalidResult());
            if (!ReferenceEquals(objA: _tail, objB: expected)) return Fin.Fail<long>(key.InvalidInput());
            long ordinal = _nextOrdinal;
            _tail = current;
            if (ordinal == long.MaxValue) _exhausted = true;
            else _nextOrdinal = ordinal + 1L;
            return Fin.Succ(ordinal);
        }
    }
}

[BoundaryAdapter]
public sealed class MonotonicBeat : IValidityEvidence {
    private readonly MonotonicStamp _origin;
    private readonly BeatSequence _sequence;

    internal MonotonicBeat(long ordinal, MonotonicStamp origin, BeatSequence sequence, MonotonicStamp stamp, TimeSpan elapsed, TimeSpan delta) {
        Ordinal = ordinal;
        _origin = origin;
        _sequence = sequence;
        Stamp = stamp;
        Elapsed = elapsed;
        Delta = delta;
    }

    public long Ordinal { get; }
    public MonotonicStamp Stamp { get; }
    public TimeSpan Elapsed { get; }
    public TimeSpan Delta { get; }
    internal MonotonicStamp Origin => _origin;
    internal BeatSequence Sequence => _sequence;
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Of(holds: Ordinal >= 0L),
        ValidityClaim.Evidence(evidence: _origin),
        ValidityClaim.Of(holds: _sequence is { } sequence && sequence.BelongsTo(origin: _origin)),
        ValidityClaim.Evidence(evidence: Stamp),
        ValidityClaim.Of(holds: (Stamp, _origin) is ({ } stamp, { } origin) && stamp.SharesTimeline(other: origin)),
        ValidityClaim.Of(holds: Elapsed >= TimeSpan.Zero),
        ValidityClaim.Of(holds: Delta >= TimeSpan.Zero && Delta <= Elapsed),
        ValidityClaim.Of(holds: Ordinal != 0 || Delta == Elapsed));
}
```

```mermaid
---
config:
  theme: base
  look: classic
  layout: elk
  elk:
    nodePlacementStrategy: NETWORK_SIMPLEX
    considerModelOrder: NODES_AND_EDGES
  flowchart:
    curve: linear
    padding: 25
  themeVariables:
    darkMode: true
    fontFamily: "SF Mono, Menlo, Cascadia Mono, Segoe UI Mono, Consolas, monospace"
    useGradient: false
    dropShadow: "none"
    background: "#282A36"
    primaryColor: "#44475A"
    primaryTextColor: "#F8F8F2"
    primaryBorderColor: "#BD93F9"
    mainBkg: "#44475A"
    nodeBorder: "#BD93F9"
    lineColor: "#FF79C6"
    arrowheadColor: "#FF79C6"
    textColor: "#F8F8F2"
    titleColor: "#D6BCFA"
    clusterBkg: "#21222C"
    clusterBorder: "#D6BCFA"
    edgeLabelBackground: "#21222C"
    labelBackgroundColor: "#21222C"
  themeCSS: ".nodeLabel{font-size:13px;font-weight:500}.edgeLabel{font-size:12px;font-weight:500}.cluster-label .nodeLabel{font-size:13.5px;font-weight:700;letter-spacing:.08em}.edge-thickness-normal{stroke-width:2px}.edge-thickness-thick{stroke-width:3px}.edge-pattern-dashed,.edge-pattern-dotted{stroke-width:1.5px;stroke-dasharray:4 6}.node rect,.node circle,.node polygon,.node path,.node .outer-path{stroke-width:1.5px;filter:none!important}.cluster rect{stroke-width:1px!important;stroke-dasharray:5 4!important;filter:none!important}.marker path{transform:scale(.8);transform-origin:5px 5px}.marker circle{transform:scale(.48);transform-origin:5px 5px}.edgeLabel rect{transform-box:fill-box;transform-origin:center;transform:scale(1.1,1.2)}"
---
flowchart LR
    accTitle: Parametric projection dependency graph
    accDescr: Projection and timing owners depend on verified geometry, numerical, colour, rail, and monotonic-clock surfaces while host consumers depend only on those owners.
    Intent["Intent dispatch"] -->|"depends on selector rows"| Selectors["Projection selectors"]
    Locate["Location queries"] -->|"depend on selector rows"| Selectors
    Fields["Curvature fields"] -->|"depend on shape operator"| Selectors
    Hosts["Host motion adapters"] -->|"depend on motion policy"| Motion["Motion policy"]
    Consumers["Clock consumers"] -->|"depend on timing evidence"| Timeline["Monotonic timeline"]
    Selectors selectorRhino@-->|"depends on evaluation members"| Rhino["Rhino geometry"]
    Selectors selectorRail@-->|"depends on typed projection"| Projection["Projection rail"]
    Motion motionRhino@-->|"depends on quaternion members"| Rhino
    Motion motionStep@-->|"depends on driven stepping"| Integrator["Field integrator"]
    Timeline timelineProvider@-->|"depends on elapsed conversion"| Provider["Injected TimeProvider"]
    Timeline timelineRail@-->|"depends on operation and validity"| Rails["Operation and validity rails"]
    classDef primary fill:#44475A,stroke:#FF79C6,color:#F8F8F2
    classDef boundary fill:#282A36,stroke:#BD93F9,color:#F8F8F2
    classDef external fill:#8BE9FDBF,stroke:#8BE9FD,color:#282A36
    classDef data fill:#FFB86CBF,stroke:#FFB86C,color:#282A36
    classDef edgeExternal stroke:#8BE9FD,color:#F8F8F2
    classDef edgeData stroke:#FFB86C,color:#F8F8F2
    class Intent,Locate,Fields,Hosts,Consumers boundary
    class Selectors,Motion,Timeline primary
    class Rhino,Integrator,Provider external
    class Projection,Rails data
    class selectorRhino,motionRhino,motionStep,timelineProvider edgeExternal
    class selectorRail,timelineRail edgeData
```

## [04]-[DENSITY_BAR]

One owner per axis; capability is a row, column, or factory fold, never a sibling surface. `[RAIL]` names the one return rail each owner exposes.

| [INDEX] | [OWNER]                       | [SHAPE]                                   | [ENTRY]                                     |
| :-----: | :---------------------------- | :---------------------------------------- | :------------------------------------------ |
|  [01]   | `CurveProjection`             | delegate vocabulary + row folds           | `Project<TOut> → Fin<TOut>`                 |
|  [02]   | `SurfaceProjection`           | delegate vocabulary + lease fold          | `Project<TOut> → Fin<TOut>`                 |
|  [03]   | `ConeProjection`              | accessor vocabulary                       | `Project<TOut> → Fin<TOut>`                 |
|  [04]   | `MotionInterpolation`         | quaternion-combination policy             | `Interpolate / Rotate → Fin<T>`             |
|  [05]   | `SurfaceSpace`                | validated host capsule                    | `Of / Sample → Fin<T>`                      |
|  [06]   | `Easing`                      | family-and-polarity row product           | `Evaluate → double`                         |
|  [07]   | `CyclePlan` + `CyclePhase`    | plan + wide phase evidence                | `Phase → Fin<CyclePhase>`                   |
|  [08]   | `SpringShape` + `SpringState` | analytic + integrated dynamics            | `Evaluate / Step → Fin<T>`                  |
|  [09]   | `MonotonicTimeline` family    | provider-branded timeline + sequence rail | `Capture / Elapsed / Order / Beat → Fin<T>` |

The selector rows, projection gates, motion kernels, and monotonic timeline form one transcription-complete source file. The fence composes `Evaluation`, `AtomProjection`, matrix and direction algebra, `ValidityClaim`, `FieldIntegrator`, and `TimeProvider` directly.

## [05]-[RESEARCH]

### [05.1]-[SELECTOR_ROWS]

A parametric probe is a row, never a method: the projection vocabularies put every evaluation modality behind a delegate column and one `Project<TOut>` gate, so call-site variation is row selection and output variation is the `Numerics/atoms` `ProjectionRow` table. Row construction is fold-owned: `Vector(...)` carries the validity gate, `FrameRow(...)` owns Rhino frame recovery, `Osculating(...)` owns principal-circle projection, and `Derivatives(...)` owns first-derivative forms over one `Surface.Evaluate` call. The `ArcLength` acceptance admits a zero length only at the domain start (`Domain.NormalizedParameterAt(t) ≤ Context.Fractional` — a dimensionless station against the fractional tolerance, never a model-space tolerance against a parameter delta), so a degenerate mid-domain length reads as evaluation failure, not as zero; the length itself gates through `RhinoMath.IsValidDouble`, the host-scalar law.

### [05.2]-[SECOND_FUNDAMENTAL_FORM]

`ShapeOperatorOf` assembles the extrinsic shape operator from the `SurfaceCurvature` principal bundle: κ and principal directions arrive from `Surface.CurvatureAt` already orthonormal in the tangent plane, the admitted `Direction` pair guards degeneracy, and the 3×3 upper triangle lands in the `Numerics/matrix` `SymmetricMatrix` whose eigen-decomposition reproduces (κ₀, κ₁, 0). The derivative family beside it is the first-fundamental side: `Metric` is [E F; F G] = [∂u·∂u, ∂u·∂v; ·, ∂v·∂v], `Jacobian` the 3×2 pushforward, `AreaScale` its `|∂u×∂v|` density — together the complete pointwise differential-geometry reading of a Rhino surface, all addressed by row. `UvFrame` re-orients the derivative-spanned plane to agree with the outward normal by flipping the Y axis, never the X — the frame stays right-handed and the u-direction stays authoritative.

### [05.3]-[ONE_SLERP]

The two interpolation surfaces are one algebra: `Combine` is the row's only behavior, `Interpolate` conjugates it through `Plane`-valued rotors anchored at `Plane.WorldXY` (rotor composition commutes with the anchoring, so anchor choice cancels), and `Rotate` runs it from `Quaternion.Identity` toward the pair's shortest-arc rotor. The antiparallel seam is the one genuinely ambiguous input — `Transform.Rotation` has no unique axis at π — and the pinned answer is the deterministic `VectorFrame.SeedPerpendicular` axis, making antiparallel rotation reproducible across runs and consumers. The `Linear` row is not a degenerate spare: on directions it yields nlerp (constant-chord, cheaper, admissible for dense sampling), on poses a non-constant-velocity frame blend; the row choice is the motion-profile policy the Camera consumer selects by name.

### [05.4]-[ONE_TIMING]

Every named `Easing` row composes a family kernel with a polarity fold, so a new family adds its kernel and polarity row declarations without a parallel evaluator. The spring's closed form is exact for a FIXED target — the regime split at ζ (underdamped rotor, critical double-root, overdamped two-rate) is the complete analytic solution of `ẍ + 2ζωẋ + ω²(x − x*) = 0`, so per-frame re-evaluation against a constant target accumulates zero integration error; the `Step` path exists precisely for the MOVING target, where the sampler re-reads `target` per stage and the `Numerics/integrate` error control earns its keep — one law decides which path a consumer takes: fixed target → `Evaluate`, driven target → `Step`. `CyclePlan` clamps a completed bounded cycle onto its terminal pose (`Local = 1`, reversed parity preserved) instead of wrapping, because a wrapped terminal frame visually snaps; colour interpolation policy lives on the `Numerics/atoms` `BlendPath` rows, whose hue-span cases pin the OKLCH hue arc exactly as the antiparallel rotor pin does on the sphere.

- [MONOTONIC_TIME] — one injected provider and timeline identity brand each stamp. Serialized capture adds a private ordinal used only to totalize equal-tick ordering; elapsed conversion enters through `TimeProvider.GetElapsedTime` after both brands pass. `BeatSeed` dispatches `Origin` into a new atomic sequence and `Previous` against its current tail; stale predecessors and ordinal exhaustion fail, counters remain private, and deterministic providers use the same ambient-clock-free surface.

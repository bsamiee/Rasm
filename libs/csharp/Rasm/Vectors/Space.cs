using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = Hit(key: 0, accepts: static output => output == typeof(Point3d) || output == typeof(ClosestHit), projectRaw: s => s.Output == typeof(Point3d) ? Accept(state: s, value: s.Hit.Point) : Accept(state: s, value: s.Hit));
    public static readonly SupportProjection Direction = new(key: 1, capability: static (_, _) => true, accepts: static output => output == typeof(Direction) || output == typeof(Vector3d), projectRaw: s => DirectionOf(vector: s.Hit.Point - s.Sample, state: s));
    public static readonly SupportProjection Span = SpanOf(key: 2, sign: 1.0);
    public static readonly SupportProjection Normal = new(key: 3, capability: static (space, _) => space.CanClosestNormal, accepts: DirectionOrVector, projectRaw: s => s.Hit.Normal.ToFin(Fail: s.Key.InvalidResult()).Bind(normal => DirectionOf(vector: normal, state: s)));
    public static readonly SupportProjection Distance = HitValue(key: 4, choose: static hit => hit.Distance);
    public static readonly SupportProjection Parameter = HitValue(key: 5, choose: static hit => hit.Parameter);
    public static readonly SupportProjection Uv = HitValue(key: 6, choose: static hit => hit.Uv);
    public static readonly SupportProjection Component = HitValue(key: 7, choose: static hit => hit.Component);
    public static readonly SupportProjection MeshPoint = HitValue(key: 8, choose: static hit => hit.MeshPoint);
    public static readonly SupportProjection SignedDistance = new(key: 9, capability: static (space, hit) => space.AdmitsSignedDistance(hit: hit), accepts: static output => output == typeof(double), projectRaw: s => s.Space.SignedDistance(hit: s.Hit, sample: s.Sample, key: s.Key).Bind(distance => Accept(state: s, value: distance)));
    public static readonly SupportProjection ContainmentDistance = new(key: 10, capability: static (space, hit) => space.AdmitsContainmentDistance(hit: hit), accepts: static output => output == typeof(double), projectRaw: s => s.Space.ContainmentDistance(hit: s.Hit, sample: s.Sample, context: s.Context, key: s.Key).Bind(distance => Accept(state: s, value: distance)));
    public static readonly SupportProjection Tangent = new(key: 11, capability: static (space, _) => GeometryKernel.CanClosestTangent(type: space.SourceType), accepts: DirectionOrVector, projectRaw: s => s.Hit.Tangent.ToFin(Fail: s.Key.InvalidResult()).Bind(tangent => DirectionOf(vector: tangent, state: s)));
    public static readonly SupportProjection Frame = HitValue(key: 12, choose: static hit => hit.Frame, capability: static (space, _) => GeometryKernel.CanClosestFrame(type: space.SourceType));
    public static readonly SupportProjection SignedSpanAway = SpanOf(key: 13, sign: -1.0);
    [UseDelegateFromConstructor] private partial bool Capability(SupportSpace space, ClosestHit hit);
    [UseDelegateFromConstructor] private partial bool Accepts(Type output);
    [UseDelegateFromConstructor] private partial Fin<object> ProjectRaw(SupportProjectionState state);
    internal bool CanProjectVector(SupportSpace space) =>
        Equals(Direction)
        || Equals(Span)
        || Equals(SignedSpanAway)
        || (Equals(Normal) && GeometryKernel.CanClosestNormal(type: space.SourceType))
        || (Equals(Tangent) && GeometryKernel.CanClosestTangent(type: space.SourceType));
    internal Fin<TOut> Project<TOut>(SupportSpace space, ClosestHit hit, Point3d sample, Context context, Op key) =>
        (hit.IsValid, Capability(space: space, hit: hit), Accepts(output: typeof(TOut))) switch {
            (false, _, _) => Fin.Fail<TOut>(error: key.InvalidResult()),
            (_, false, _) => Fin.Fail<TOut>(error: key.Unsupported(geometryType: space.SourceType, outputType: typeof(TOut))),
            (_, _, false) => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(SupportProjection), outputType: typeof(TOut))),
            _ => ProjectRaw(state: new SupportProjectionState(Space: space, Hit: hit, Sample: sample, Context: context, Key: key, Output: typeof(TOut)))
                .Bind(value => value is TOut output ? Fin.Succ(output) : Fin.Fail<TOut>(error: key.InvalidResult())),
        };
    private static bool DirectionOrVector(Type output) => output == typeof(Direction) || output == typeof(Vector3d);
    private static SupportProjection Hit(int key, Func<Type, bool> accepts, Func<SupportProjectionState, Fin<object>> projectRaw, Func<SupportSpace, ClosestHit, bool>? capability = null) =>
        new(key: key, capability: capability ?? ((_, _) => true), accepts: accepts, projectRaw: projectRaw);
    private static SupportProjection HitValue<T>(int key, Func<ClosestHit, Option<T>> choose, Func<SupportSpace, ClosestHit, bool>? capability = null) =>
        Hit(
            key: key,
            accepts: static output => output == typeof(T),
            projectRaw: state =>
                from value in choose(arg: state.Hit).ToFin(Fail: state.Key.InvalidResult())
                from accepted in Accept(state: state, value: value)
                select accepted,
            capability: capability);
    private static SupportProjection SpanOf(int key, double sign) =>
        new(key: key, capability: static (_, _) => true, accepts: static output => output == typeof(VectorSpan) || output == typeof(Vector3d) || output == typeof(Line) || output == typeof(double),
            projectRaw: state => state.Output switch {
                Type t when t == typeof(Vector3d) => Accept(state: state, value: sign * (state.Hit.Point - state.Sample)),
                Type t when t == typeof(double) => Accept(state: state, value: sign * (state.Hit.Point - state.Sample).Length),
                _ => VectorSpan.Of(anchor: state.Sample, vector: sign * (state.Hit.Point - state.Sample), context: state.Context, key: state.Key)
                    .Bind(span => state.Output switch {
                        Type t when t == typeof(VectorSpan) => Accept(state: state, value: span),
                        Type t when t == typeof(Line) => Accept(state: state, value: span.Axis),
                        _ => Fin.Fail<object>(error: state.Key.Unsupported(geometryType: typeof(VectorSpan), outputType: state.Output)),
                    }),
            });
    private static Fin<object> DirectionOf(Vector3d vector, SupportProjectionState state) =>
        Vectors.Direction.Of(value: vector, context: state.Context, key: state.Key)
            .Bind(direction => state.Output == typeof(Direction) ? Accept(state: state, value: direction) : Accept(state: state, value: direction.Value));
    private static Fin<object> Accept<T>(SupportProjectionState state, T value) =>
        state.Output != typeof(T)
            ? Fin.Fail<object>(error: state.Key.Unsupported(geometryType: typeof(T), outputType: state.Output))
            : value switch {
                Direction direction => Fin.Succ((object)direction),
                VectorSpan span => Fin.Succ((object)span),
                ClosestHit hit when hit.IsValid => Fin.Succ((object)hit),
                Plane plane => FieldNabla.Plane(basis: plane, key: state.Key).Map(static accepted => (object)accepted),
                _ => state.Key.AcceptValue(value: value).Map(static accepted => (object)accepted!),
            };
}

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter]
public sealed record SupportSpace {
    private SupportSpace(object value) => Value = value;
    internal object Value { get; }
    internal Type SourceType => Value.GetType();
    internal bool CanClosestNormal => GeometryKernel.CanClosestNormal(type: SourceType);
    internal bool CanSignedDistance => GeometryKernel.CanSignedDistance(type: SourceType);
    internal bool AdmitsSignedDistance(ClosestHit hit) =>
        Value switch {
            Plane or Sphere or Box or BoundingBox => hit.Distance.IsSome,
            _ => CanSignedDistance && hit.Normal.IsSome,
        };
    internal bool AdmitsContainmentDistance(ClosestHit hit) =>
        Value switch {
            Brep { IsSolid: true } or Mesh { IsSolid: true } => hit.Distance.IsSome,
            Brep or Mesh => false,
            _ => AdmitsSignedDistance(hit: hit),
        };
    public static Fin<SupportSpace> Of(object? value, Op? key = null) {
        Op op = key.OrDefault();
        return value switch {
            VectorCloud.ClusterCase cluster =>
                from _ in cluster.Vertices.TraverseM(point => op.AcceptValue(value: point)).As()
                from __ in CloudKernel.MassOf(cluster: cluster, key: op)
                select new SupportSpace(value: cluster),
            _ => from source in Optional(value).ToFin(op.InvalidInput())
                 let type = source.GetType()
                 from _ in guard(type != typeof(object) && type != typeof(GeometryBase) && GeometryKernel.CanClosest(type: type), op.Unsupported(type, typeof(ClosestHit)))
                 from __ in SupportIsValid(source: source, key: op)
                 select new SupportSpace(value: source),
        };
    }
    private static Fin<Unit> SupportIsValid(object source, Op key) =>
        source switch {
            Plane plane => FieldNabla.Plane(basis: plane, key: key).Map(static _ => unit),
            _ => OpAcceptance.ValidityOf(source: source).Filter(static valid => valid).Map(static _ => unit).ToFin(key.InvalidInput()),
        };
    internal Fin<ClosestHit> Closest(Point3d sample, Op key) =>
        Value switch {
            VectorCloud.ClusterCase cluster => cluster.ClosestVertex(sample: sample, key: key),
            _ => GeometryKernel.ClosestOf(geometry: Value, target: sample, key: key),
        };
    internal Fin<double> SignedDistance(ClosestHit hit, Point3d sample, Op key) =>
        GeometryKernel.SignedDistanceOf(geometry: Value, hit: hit, sample: sample, key: key);
    internal Fin<double> ContainmentDistance(ClosestHit hit, Point3d sample, Context context, Op key) =>
        Value switch {
            Brep { IsSolid: true } brep => hit.Distance.ToFin(Fail: key.InvalidResult())
                .Map(distance => (brep.IsPointInside(sample, context.Absolute.Value, strictlyIn: false) ? -1.0 : 1.0) * distance),
            Mesh { IsSolid: true } mesh => hit.Distance.ToFin(Fail: key.InvalidResult())
                .Map(distance => (mesh.IsPointInside(sample, context.Absolute.Value, strictlyIn: false) ? -1.0 : 1.0) * distance),
            Brep or Mesh => Fin.Fail<double>(error: key.InvalidInput()),
            _ => SignedDistance(hit: hit, sample: sample, key: key),
        };
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SurfaceSpace {
    private SurfaceSpace(Surface native, Context tolerance) { Native = native; Tolerance = tolerance; }
    public Surface Native { get; }
    public Context Tolerance { get; }
    public static Fin<SurfaceSpace> Of(Surface native, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from ctx in Optional(context).ToFin(op.MissingContext())
               let candidate = new SurfaceSpace(native: native, tolerance: ctx)
               from active in FieldNabla.SurfaceNative(space: candidate, key: op)
               select new SurfaceSpace(native: active, tolerance: ctx);
    }
    internal Fin<TOut> Sample<TOut>(SurfaceProjection projection, double u, double v, Op? key = null) {
        Op op = key.OrDefault();
        Surface native = Native; Context tolerance = Tolerance;
        return Optional(projection).ToFin(op.InvalidInput()).Bind(p => p.Project<TOut>(surface: native, u: u, v: v, context: tolerance, key: op));
    }
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal readonly record struct SupportProjectionState(SupportSpace Space, ClosestHit Hit, Point3d Sample, Context Context, Op Key, Type Output);

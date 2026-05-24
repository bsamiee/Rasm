using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = Hit(key: 0, accepts: static output => output == typeof(Point3d) || output == typeof(ClosestHit), projectRaw: s => s.Output == typeof(Point3d) ? Accept(state: s, value: s.Hit.Point) : Accept(state: s, value: s.Hit));
    public static readonly SupportProjection Direction = new(key: 1, capability: static (_, _) => true, accepts: static output => output == typeof(Direction) || output == typeof(Vector3d), projectRaw: s => DirectionOf(vector: s.Hit.Point - s.Sample, state: s));
    public static readonly SupportProjection Span = SpanOf(key: 2, sign: 1.0);
    public static readonly SupportProjection SignedSpanAway = SpanOf(key: 13, sign: -1.0);
    public static readonly SupportProjection Normal = new(key: 3, capability: static (space, _) => space.CanClosestNormal, accepts: DirectionOrVector, projectRaw: s => s.Hit.Normal.ToFin(Fail: s.Key.InvalidResult()).Bind(normal => DirectionOf(vector: normal, state: s)));
    public static readonly SupportProjection Distance = Hit(key: 4, accepts: static output => output == typeof(double), projectRaw: s => s.Hit.Distance.ToFin(Fail: s.Key.InvalidResult()).Bind(distance => Accept(state: s, value: distance)));
    public static readonly SupportProjection Parameter = Hit(key: 5, accepts: static output => output == typeof(double), projectRaw: s => s.Hit.Parameter.ToFin(Fail: s.Key.InvalidResult()).Bind(parameter => Accept(state: s, value: parameter)));
    public static readonly SupportProjection Uv = Hit(key: 6, accepts: static output => output == typeof(Point2d), projectRaw: s => s.Hit.Uv.ToFin(Fail: s.Key.InvalidResult()).Bind(uv => Accept(state: s, value: uv)));
    public static readonly SupportProjection Component = Hit(key: 7, accepts: static output => output == typeof(ComponentIndex), projectRaw: s => s.Hit.Component.ToFin(Fail: s.Key.InvalidResult()).Bind(component => Accept(state: s, value: component)));
    public static readonly SupportProjection MeshPoint = Hit(key: 8, accepts: static output => output == typeof(MeshPoint), projectRaw: s => s.Hit.MeshPoint.ToFin(Fail: s.Key.InvalidResult()).Bind(meshPoint => Accept(state: s, value: meshPoint)));
    public static readonly SupportProjection SignedDistance = new(key: 9, capability: static (space, hit) => space.AdmitsSignedDistance(hit: hit), accepts: static output => output == typeof(double), projectRaw: s => s.Space.SignedDistance(hit: s.Hit, sample: s.Sample, key: s.Key).Bind(distance => Accept(state: s, value: distance)));
    public static readonly SupportProjection ContainmentDistance = new(key: 10, capability: static (space, hit) => space.AdmitsContainmentDistance(hit: hit), accepts: static output => output == typeof(double), projectRaw: s => s.Space.ContainmentDistance(hit: s.Hit, sample: s.Sample, context: s.Context, key: s.Key).Bind(distance => Accept(state: s, value: distance)));
    public static readonly SupportProjection Tangent = new(key: 11, capability: static (space, _) => GeometryKernel.CanClosestTangent(type: space.SourceType), accepts: DirectionOrVector, projectRaw: s => s.Hit.Tangent.ToFin(Fail: s.Key.InvalidResult()).Bind(tangent => DirectionOf(vector: tangent, state: s)));
    public static readonly SupportProjection Frame = Hit(key: 12, accepts: static output => output == typeof(Plane), projectRaw: s => s.Hit.Frame.ToFin(Fail: s.Key.InvalidResult()).Bind(frame => Accept(state: s, value: frame)), capability: static (space, _) => GeometryKernel.CanClosestFrame(type: space.SourceType));
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
        (Capability(space: space, hit: hit), Accepts(output: typeof(TOut))) switch {
            (false, _) => Fin.Fail<TOut>(error: key.Unsupported(geometryType: space.SourceType, outputType: typeof(TOut))),
            (_, false) => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(SupportProjection), outputType: typeof(TOut))),
            _ => ProjectRaw(state: new SupportProjectionState(Space: space, Hit: hit, Sample: sample, Context: context, Key: key, Output: typeof(TOut)))
                .Bind(value => value is TOut output ? key.AcceptValue(value: output) : Fin.Fail<TOut>(error: key.InvalidResult())),
        };
    private static bool DirectionOrVector(Type output) => output == typeof(Direction) || output == typeof(Vector3d);
    private static SupportProjection Hit(int key, Func<Type, bool> accepts, Func<SupportProjectionState, Fin<object>> projectRaw, Func<SupportSpace, ClosestHit, bool>? capability = null) =>
        new(key: key, capability: capability ?? ((_, _) => true), accepts: accepts, projectRaw: projectRaw);
    private static SupportProjection SpanOf(int key, double sign) =>
        new(key: key, capability: static (_, _) => true, accepts: static output => output == typeof(VectorSpan) || output == typeof(Vector3d) || output == typeof(Line) || output == typeof(double),
            projectRaw: state => state.Output switch {
                Type t when t == typeof(Vector3d) => Accept(state: state, value: sign * (state.Hit.Point - state.Sample)),
                Type t when t == typeof(double) => Accept(state: state, value: (state.Hit.Point - state.Sample).Length),
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
        state.Key.AcceptValue(value: value).Map(static accepted => (object)accepted!);
}

internal readonly record struct SupportProjectionState(SupportSpace Space, ClosestHit Hit, Point3d Sample, Context Context, Op Key, Type Output);

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter]
public sealed record SupportSpace {
    private SupportSpace(object value) => Value = value;
    internal object Value { get; }
    internal Type SourceType => Value.GetType();
    internal bool CanClosestNormal => GeometryKernel.CanClosestNormal(type: SourceType);
    internal bool CanSignedDistance => GeometryKernel.CanSignedDistance(type: SourceType);
    internal bool AdmitsNormal(ClosestHit hit) =>
        CanClosestNormal && hit.Normal.IsSome;
    internal bool AdmitsTangent(ClosestHit hit) =>
        GeometryKernel.CanClosestTangent(type: SourceType) && hit.Tangent.IsSome;
    internal bool AdmitsFrame(ClosestHit hit) =>
        GeometryKernel.CanClosestFrame(type: SourceType) && hit.Frame.IsSome;
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
            VectorCloud.ClusterCase cluster => ClusterIsValid(cluster)
                ? Fin.Succ(new SupportSpace(value: cluster))
                : Fin.Fail<SupportSpace>(op.InvalidInput()),
            _ => from source in Optional(value).ToFin(op.InvalidInput())
                 let type = source.GetType()
                 from _ in guard(type != typeof(object) && type != typeof(GeometryBase) && GeometryKernel.CanClosest(type: type), op.Unsupported(type, typeof(ClosestHit)))
                 select new SupportSpace(value: source),
        };
    }
    private static bool ClusterIsValid(VectorCloud.ClusterCase cluster) =>
        cluster.Vertices.Count > 0
        && cluster.Vertices.ForAll(static point => point.IsValid)
        && cluster.Mass.Map(mass => mass.Count == cluster.Vertices.Count && mass.ForAll(static value => RhinoMath.IsValidDouble(x: value) && value > 0.0)).IfNone(true);
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
        return from active in Optional(native).ToFin(op.InvalidInput())
               from ctx in Optional(context).ToFin(op.MissingContext())
               from _ in guard(active.IsValid, op.InvalidInput())
               select new SurfaceSpace(native: active, tolerance: ctx);
    }
    internal Fin<TOut> Sample<TOut>(SurfaceProjection projection, double u, double v, Op? key = null) {
        Op op = key.OrDefault();
        Surface native = Native; Context tolerance = Tolerance;
        return Optional(projection).ToFin(op.InvalidInput()).Bind(p => p.Project<TOut>(surface: native, u: u, v: v, context: tolerance, key: op));
    }
}

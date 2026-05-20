namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = new(key: 0, hitOutput: typeof(Point3d), parameterMode: false), Direction = new(key: 1, hitOutput: typeof(void), parameterMode: false), Span = new(key: 2, hitOutput: typeof(void), parameterMode: false), Normal = new(key: 3, hitOutput: typeof(void), parameterMode: false), Distance = new(key: 4, hitOutput: typeof(double), parameterMode: false);
    public static readonly SupportProjection Parameter = new(key: 5, hitOutput: typeof(double), parameterMode: true), Uv = new(key: 6, hitOutput: typeof(Point2d), parameterMode: false), Component = new(key: 7, hitOutput: typeof(ComponentIndex), parameterMode: false), MeshPoint = new(key: 8, hitOutput: typeof(MeshPoint), parameterMode: false), SignedDistance = new(key: 9, hitOutput: typeof(void), parameterMode: false);
    internal Type HitOutput { get; }
    internal bool Hit => HitOutput != typeof(void);
    internal bool ParameterMode { get; }
    internal bool AcceptsHit(Type output) =>
        output == HitOutput || (Equals(Closest) && output == typeof(ClosestHit));
    internal Fin<TOut> Project<TOut>(ClosestHit hit, Point3d sample, Context context, Op key) =>
        this switch {
            SupportProjection p when p.Equals(SignedDistance) && typeof(TOut) == typeof(double) => hit.SignedDistanceFrom(sample: sample, key: key)
                .Bind(distance => key.AcceptValue(value: distance))
                .Map(static value => (TOut)(object)value),
            SupportProjection p when p.Equals(SignedDistance) => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(ClosestHit), outputType: typeof(TOut))),
            SupportProjection p when p.Hit && p.AcceptsHit(output: typeof(TOut)) => hit.Project<TOut>(key: key, parameterMode: p.ParameterMode)
                .Bind(values => values.Head.ToFin(key.InvalidResult())),
            SupportProjection p when p.Hit => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(ClosestHit), outputType: typeof(TOut))),
            SupportProjection p when p.Equals(Direction) => Rasm.Vectors.Direction.Of(value: hit.Point - sample, context: context, key: key)
                .Bind(direction => direction.Project<TOut>(key: key)),
            SupportProjection p when p.Equals(Span) => VectorSpan.Of(anchor: sample, vector: hit.Point - sample, context: context, key: key)
                .Bind(span => span.Project<TOut>(key: key)),
            SupportProjection p when p.Equals(Normal) => hit.Normal.ToFin(Fail: key.InvalidResult())
                .Bind(normal => Rasm.Vectors.Direction.Of(value: normal, context: context, key: key))
                .Bind(direction => direction.Project<TOut>(key: key)),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(SupportProjection), outputType: typeof(TOut))),
        };
}

[SmartEnum<int>]
public sealed partial class VectorRingMetric {
    public static readonly VectorRingMetric Normal = new(key: 0, output: typeof(Vector3d), measure: static (ring, key) => ring.Normal(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric EdgeAspect = new(key: 1, output: typeof(double), measure: static (ring, key) => ring.EdgeAspect(key: key).Map(static value => (object)value)), Area = new(key: 2, output: typeof(double), measure: static (ring, key) => ring.Area(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric Perimeter = new(key: 3, output: typeof(double), measure: static (ring, key) => ring.Perimeter(key: key).Map(static value => (object)value)), Skewness = new(key: 4, output: typeof(double), measure: static (ring, key) => ring.Skewness(key: key).Map(static value => (object)value));
    public Type Output { get; }
    [UseDelegateFromConstructor] private partial Fin<object> Measure(VectorRing ring, Op key);
    internal Fin<TOut> Project<TOut>(VectorRing ring, Op key) =>
        Output.Equals(typeof(TOut)) switch {
            true => Measure(ring: ring, key: key).Bind(value => key.AcceptValue(value: value).Map(static valid => (TOut)valid)),
            false => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorRing), outputType: typeof(TOut))),
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public partial record VectorIntent {
    public sealed record BetweenCase(Point3d Origin, SupportSpace Target, BoundarySense Sense) : VectorIntent;
    public sealed record AxisCase(SignedAxis Value, Option<Plane> Frame) : VectorIntent;
    public sealed record AngularCase(Direction A, Direction B, Option<Plane> Frame) : VectorIntent;
    public sealed record SupportCase(SupportSpace Space, Point3d Sample, SupportProjection Projection) : VectorIntent;
    public sealed record FieldCase(VectorField Value, Point3d Sample) : VectorIntent;
    public sealed record RayCase(Point3d Origin, Direction Direction, RayPolicy Policy) : VectorIntent;
    public sealed record RingCase(Seq<Point3d> Points, VectorRingMetric Metric) : VectorIntent;
    public static VectorIntent Between(Point3d origin, SupportSpace target, BoundarySense? sense = null) =>
        new BetweenCase(Origin: origin, Target: target, Sense: sense ?? BoundarySense.Toward);
    public static VectorIntent Axis(SignedAxis axis, Plane? frame = null) =>
        new AxisCase(Value: axis, Frame: Optional(frame));
    public static VectorIntent Angular(Direction a, Direction b, Plane? frame = null) =>
        new AngularCase(A: a, B: b, Frame: Optional(frame));
    public static VectorIntent Support(SupportSpace space, Point3d sample, SupportProjection projection) =>
        new SupportCase(Space: space, Sample: sample, Projection: projection);
    public static VectorIntent Field(VectorField field, Point3d sample) =>
        new FieldCase(Value: field, Sample: sample);
    public static VectorIntent Ray(Point3d origin, Direction direction, RayPolicy? policy = null) =>
        new RayCase(Origin: origin, Direction: direction, Policy: policy ?? RayPolicy.Forward);
    public static VectorIntent Ring(Seq<Point3d> points, VectorRingMetric metric) =>
        new RingCase(Points: points, Metric: metric);
}

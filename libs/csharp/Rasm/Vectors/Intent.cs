namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = new(key: 0, hitOutput: typeof(Point3d), parameterMode: false, sign: 1.0, capability: static _ => true);
    public static readonly SupportProjection Direction = new(key: 1, hitOutput: typeof(void), parameterMode: false, sign: 1.0, capability: static _ => true);
    public static readonly SupportProjection Span = new(key: 2, hitOutput: typeof(void), parameterMode: false, sign: 1.0, capability: static _ => true);
    public static readonly SupportProjection SignedSpanAway = new(key: 13, hitOutput: typeof(void), parameterMode: false, sign: -1.0, capability: static _ => true);
    public static readonly SupportProjection Normal = new(key: 3, hitOutput: typeof(void), parameterMode: false, sign: 1.0, capability: static space => space.CanClosestNormal);
    public static readonly SupportProjection Distance = new(key: 4, hitOutput: typeof(double), parameterMode: false, sign: 1.0, capability: static _ => true);
    public static readonly SupportProjection Parameter = new(key: 5, hitOutput: typeof(double), parameterMode: true, sign: 1.0, capability: static _ => true);
    public static readonly SupportProjection Uv = new(key: 6, hitOutput: typeof(Point2d), parameterMode: false, sign: 1.0, capability: static _ => true);
    public static readonly SupportProjection Component = new(key: 7, hitOutput: typeof(ComponentIndex), parameterMode: false, sign: 1.0, capability: static _ => true);
    public static readonly SupportProjection MeshPoint = new(key: 8, hitOutput: typeof(MeshPoint), parameterMode: false, sign: 1.0, capability: static _ => true);
    public static readonly SupportProjection SignedDistance = new(key: 9, hitOutput: typeof(void), parameterMode: false, sign: 1.0, capability: static space => space.CanSignedDistance);
    public static readonly SupportProjection ContainmentDistance = new(key: 10, hitOutput: typeof(void), parameterMode: false, sign: 1.0, capability: static _ => true);
    public static readonly SupportProjection Tangent = new(key: 11, hitOutput: typeof(void), parameterMode: false, sign: 1.0, capability: static space => GeometryKernel.CanClosestTangent(space.SourceType));
    public static readonly SupportProjection Frame = new(key: 12, hitOutput: typeof(Plane), parameterMode: false, sign: 1.0, capability: static space => GeometryKernel.CanClosestFrame(space.SourceType));
    internal Type HitOutput { get; }
    internal bool Hit => HitOutput != typeof(void);
    internal bool ParameterMode { get; }
    internal double Sign { get; }
    [UseDelegateFromConstructor] private partial bool Capability(SupportSpace space);
    internal bool AcceptsHit(Type output) =>
        output == HitOutput || (Equals(Closest) && output == typeof(ClosestHit));
    internal Fin<TOut> Project<TOut>(SupportSpace space, ClosestHit hit, Point3d sample, Context context, Op key) =>
        Capability(space: space) switch {
            false => Fin.Fail<TOut>(error: key.Unsupported(geometryType: space.SourceType, outputType: typeof(TOut))),
            true => this switch {
                SupportProjection p when p.Equals(SignedDistance) || p.Equals(ContainmentDistance) => typeof(TOut) == typeof(double)
                    ? (p.Equals(SignedDistance) ? space.SignedDistance(hit: hit, sample: sample, key: key) : space.ContainmentDistance(hit: hit, sample: sample, context: context, key: key))
                        .Bind(distance => key.AcceptValue(value: distance))
                        .Map(static value => (TOut)(object)value)
                    : Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(ClosestHit), outputType: typeof(TOut))),
                SupportProjection p when p.Hit && p.AcceptsHit(output: typeof(TOut)) => hit.Project<TOut>(key: key, parameterMode: p.ParameterMode)
                    .Bind(values => values.Head.ToFin(key.InvalidResult())),
                SupportProjection p when p.Hit => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(ClosestHit), outputType: typeof(TOut))),
                SupportProjection p when p.Equals(Direction) => Rasm.Vectors.Direction.Of(value: hit.Point - sample, context: context, key: key)
                    .Bind(direction => direction.Project<TOut>(key: key)),
                SupportProjection p when p.Equals(Span) || p.Equals(SignedSpanAway) => VectorSpan.Of(anchor: sample, vector: p.Sign * (hit.Point - sample), context: context, key: key)
                    .Bind(span => span.Project<TOut>(key: key)),
                SupportProjection p when p.Equals(Normal) => hit.Normal.ToFin(Fail: key.InvalidResult())
                    .Bind(normal => Rasm.Vectors.Direction.Of(value: normal, context: context, key: key))
                    .Bind(direction => direction.Project<TOut>(key: key)),
                SupportProjection p when p.Equals(Tangent) => hit.Tangent.ToFin(Fail: key.InvalidResult())
                    .Bind(tangent => Rasm.Vectors.Direction.Of(value: tangent, context: context, key: key))
                    .Bind(direction => direction.Project<TOut>(key: key)),
                SupportProjection p when p.Equals(Frame) => hit.Frame.ToFin(Fail: key.InvalidResult())
                    .Bind(frame => key.AcceptValue(value: frame))
                    .Map(static value => (TOut)(object)value),
                _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(SupportProjection), outputType: typeof(TOut))),
            },
        };
}

[SmartEnum<int>]
public sealed partial class VectorRingMetric {
    public static readonly VectorRingMetric Normal = new(key: 0, output: typeof(Vector3d), measure: static (ring, key) => ring.Normal(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric EdgeAspect = new(key: 1, output: typeof(double), measure: static (ring, key) => ring.EdgeAspect(key: key).Map(static value => (object)value)), Area = new(key: 2, output: typeof(double), measure: static (ring, key) => ring.Area(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric Perimeter = new(key: 3, output: typeof(double), measure: static (ring, key) => ring.Perimeter(key: key).Map(static value => (object)value)), Skewness = new(key: 4, output: typeof(double), measure: static (ring, key) => ring.Skewness(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric Centroid = new(key: 5, output: typeof(Point3d), measure: static (ring, key) => ring.Centroid(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric BestFitPlane = new(key: 6, output: typeof(Plane), measure: static (ring, key) => ring.BestFitPlane(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric PrincipalAxis = new(key: 7, output: typeof(Seq<Vector3d>), measure: static (ring, key) => ring.PrincipalAxes(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric PrincipalFrame = new(key: 8, output: typeof(Plane), measure: static (ring, key) => ring.PrincipalFrame(key: key).Map(static value => (object)value));
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
public abstract partial record VectorIntent {
    private VectorIntent() { }
    public abstract Fin<TOut> Project<TOut>(Context context, Op op);
    public sealed record AxisCase(SignedAxis Value, Option<Plane> Frame) : VectorIntent {
        public override Fin<TOut> Project<TOut>(Context context, Op op) =>
            from direction in Rasm.Vectors.Direction.Of(value: Value.Of(frame: Frame), context: context, key: op)
            from output in direction.Project<TOut>(key: op)
            select output;
    }
    public sealed record AngularCase(Direction A, Direction B, AnglePivot Pivot) : VectorIntent {
        public override Fin<TOut> Project<TOut>(Context context, Op op) =>
            from angle in VectorAngle.Of(a: A, b: B, pivot: Pivot, key: op)
            from output in angle.Project<TOut>(key: op)
            select output;
    }
    public sealed record SupportCase(SupportSpace Space, Point3d Sample, SupportProjection Projection) : VectorIntent {
        public override Fin<TOut> Project<TOut>(Context context, Op op) =>
            from hit in Space.Closest(sample: Sample, key: op)
            from output in Projection.Project<TOut>(space: Space, hit: hit, sample: Sample, context: context, key: op)
            select output;
    }
    public sealed record FieldCase(VectorField Value, Point3d Sample) : VectorIntent {
        public override Fin<TOut> Project<TOut>(Context context, Op op) =>
            Value.Project<TOut>(sample: Sample, context: context, key: op);
    }
    public sealed record RayCase(Point3d Origin, Direction Direction, RayPolicy Policy) : VectorIntent {
        public override Fin<TOut> Project<TOut>(Context context, Op op) =>
            Policy.Project<TOut>(origin: Origin, direction: Direction, context: context, key: op);
    }
    public sealed record RingCase(Seq<Point3d> Points, VectorRingMetric Metric) : VectorIntent {
        public override Fin<TOut> Project<TOut>(Context context, Op op) =>
            from ring in VectorRing.Of(points: Points, key: op)
            from output in Metric.Project<TOut>(ring: ring, key: op)
            select output;
    }
    public sealed record ComponentsCase(Point3d Anchor, Vector3d Value, Plane Frame) : VectorIntent {
        public override Fin<TOut> Project<TOut>(Context context, Op op) =>
            from span in VectorSpan.Of(anchor: Anchor, vector: Value, context: context, key: op)
            from components in span.Components(frame: Frame, key: op)
            from output in typeof(TOut) == typeof(ValueTuple<double, double>)
                ? Fin.Succ((TOut)(object)components)
                : Fin.Fail<TOut>(error: op.Unsupported(geometryType: typeof(VectorSpan), outputType: typeof(TOut)))
            select output;
    }
    public sealed record RelationCase(Vector3d A, Vector3d B) : VectorIntent {
        public override Fin<TOut> Project<TOut>(Context context, Op op) =>
            from relation in VectorRelation.Of(a: A, b: B, context: context, key: op)
            from output in relation.Project<TOut>(key: op)
            select output;
    }
    public sealed record BounceCase(Direction Incident, SupportSpace Surface, Point3d Sample, BouncePolicy Policy) : VectorIntent {
        public override Fin<TOut> Project<TOut>(Context context, Op op) =>
            from hit in Surface.Closest(sample: Sample, key: op)
            from rawNormal in hit.Normal.ToFin(Fail: op.InvalidResult())
            from normal in Rasm.Vectors.Direction.Of(value: rawNormal, context: context, key: op)
            from reflected in Policy.Apply(incident: Incident, normal: normal, key: op)
            from output in reflected.Project<TOut>(key: op)
            select output;
    }
    public sealed record StreamlineCase(VectorField Source, Point3d Seed, PositiveMagnitude StepSize, int Steps, FieldIntegrator Integrator) : VectorIntent {
        public override Fin<TOut> Project<TOut>(Context context, Op op) =>
            from seed in op.AcceptValue(value: Seed)
            from trajectory in toSeq(Enumerable.Range(start: 0, count: Steps)).Fold(
                initialState: Fin.Succ(Seq(seed)),
                f: (acc, _) => acc.Bind(trail => Integrator.Step(field: Source, point: trail[trail.Count - 1], h: StepSize.Value, context: context, key: op).Map(trail.Add)))
            from output in typeof(TOut) switch {
                Type t when t == typeof(Seq<Point3d>) => Fin.Succ((TOut)(object)trajectory),
                Type t when t == typeof(Polyline) => op.AcceptValue(value: new Polyline(trajectory.AsIterable())).Map(static value => (TOut)(object)value),
                _ => Fin.Fail<TOut>(error: op.Unsupported(geometryType: typeof(StreamlineCase), outputType: typeof(TOut))),
            }
            select output;
    }
    public static VectorIntent Between(Point3d origin, SupportSpace target, BoundarySense? sense = null) =>
        new SupportCase(Space: target, Sample: origin, Projection: (sense ?? BoundarySense.Toward).Equals(BoundarySense.Toward) ? SupportProjection.Span : SupportProjection.SignedSpanAway);
    public static VectorIntent Axis(SignedAxis axis, Plane? frame = null) =>
        new AxisCase(Value: axis, Frame: Optional(frame));
    public static VectorIntent Angular(Direction a, Direction b, AnglePivot? pivot = null) =>
        new AngularCase(A: a, B: b, Pivot: pivot ?? AnglePivot.World);
    public static VectorIntent Support(SupportSpace space, Point3d sample, SupportProjection projection) =>
        new SupportCase(Space: space, Sample: sample, Projection: projection);
    public static VectorIntent Field(VectorField field, Point3d sample) =>
        new FieldCase(Value: field, Sample: sample);
    public static VectorIntent Ray(Point3d origin, Direction direction, RayPolicy? policy = null) =>
        new RayCase(Origin: origin, Direction: direction, Policy: policy ?? RayPolicy.Forward);
    public static VectorIntent Ring(Seq<Point3d> points, VectorRingMetric metric) =>
        new RingCase(Points: points, Metric: metric);
    public static VectorIntent Components(Point3d anchor, Vector3d value, Plane frame) =>
        new ComponentsCase(Anchor: anchor, Value: value, Frame: frame);
    public static VectorIntent Relation(Vector3d a, Vector3d b) =>
        new RelationCase(A: a, B: b);
    public static VectorIntent Bounce(Direction incident, SupportSpace surface, Point3d sample, BouncePolicy? policy = null) =>
        new BounceCase(Incident: incident, Surface: surface, Sample: sample, Policy: policy ?? BouncePolicy.Reflect);
    public static Fin<VectorIntent> Streamline(VectorField field, Point3d seed, double stepSize, int steps, FieldIntegrator? integrator = null, Op? key = null) {
        Op op = key.OrDefault();
        return from h in op.AcceptValidated<PositiveMagnitude>(candidate: stepSize)
               from _ in guard(steps > 0, op.InvalidInput())
               select (VectorIntent)new StreamlineCase(Source: field, Seed: seed, StepSize: h, Steps: steps, Integrator: integrator ?? FieldIntegrator.RK4);
    }
}

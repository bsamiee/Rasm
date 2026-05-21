namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = new(key: 0, parameterMode: false, sign: 1.0, capability: static _ => true, accepts: static output => output == typeof(Point3d) || output == typeof(ClosestHit));
    public static readonly SupportProjection Direction = new(key: 1, parameterMode: false, sign: 1.0, capability: static _ => true, accepts: static _ => false);
    public static readonly SupportProjection Span = new(key: 2, parameterMode: false, sign: 1.0, capability: static _ => true, accepts: static _ => false);
    public static readonly SupportProjection SignedSpanAway = new(key: 13, parameterMode: false, sign: -1.0, capability: static _ => true, accepts: static _ => false);
    public static readonly SupportProjection Normal = new(key: 3, parameterMode: false, sign: 1.0, capability: static space => space.CanClosestNormal, accepts: static _ => false);
    public static readonly SupportProjection Distance = new(key: 4, parameterMode: false, sign: 1.0, capability: static _ => true, accepts: static output => output == typeof(double));
    public static readonly SupportProjection Parameter = new(key: 5, parameterMode: true, sign: 1.0, capability: static _ => true, accepts: static output => output == typeof(double));
    public static readonly SupportProjection Uv = new(key: 6, parameterMode: false, sign: 1.0, capability: static _ => true, accepts: static output => output == typeof(Point2d));
    public static readonly SupportProjection Component = new(key: 7, parameterMode: false, sign: 1.0, capability: static _ => true, accepts: static output => output == typeof(ComponentIndex));
    public static readonly SupportProjection MeshPoint = new(key: 8, parameterMode: false, sign: 1.0, capability: static _ => true, accepts: static output => output == typeof(MeshPoint));
    public static readonly SupportProjection SignedDistance = new(key: 9, parameterMode: false, sign: 1.0, capability: static space => space.CanSignedDistance, accepts: static _ => false);
    public static readonly SupportProjection ContainmentDistance = new(key: 10, parameterMode: false, sign: 1.0, capability: static _ => true, accepts: static _ => false);
    public static readonly SupportProjection Tangent = new(key: 11, parameterMode: false, sign: 1.0, capability: static space => GeometryKernel.CanClosestTangent(space.SourceType), accepts: static _ => false);
    public static readonly SupportProjection Frame = new(key: 12, parameterMode: false, sign: 1.0, capability: static space => GeometryKernel.CanClosestFrame(space.SourceType), accepts: static output => output == typeof(Plane));
    internal bool ParameterMode { get; }
    internal double Sign { get; }
    [UseDelegateFromConstructor] private partial bool Capability(SupportSpace space);
    [UseDelegateFromConstructor] private partial bool Accepts(Type output);
    internal Fin<TOut> Project<TOut>(SupportSpace space, ClosestHit hit, Point3d sample, Context context, Op key) =>
        Capability(space: space) switch {
            false => Fin.Fail<TOut>(error: key.Unsupported(geometryType: space.SourceType, outputType: typeof(TOut))),
            true => this switch {
                SupportProjection p when p.Equals(SignedDistance) || p.Equals(ContainmentDistance) => typeof(TOut) == typeof(double)
                    ? (p.Equals(SignedDistance) ? space.SignedDistance(hit: hit, sample: sample, key: key) : space.ContainmentDistance(hit: hit, sample: sample, context: context, key: key))
                        .Bind(distance => key.AcceptValue(value: distance))
                        .Map(static value => (TOut)(object)value)
                    : Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(ClosestHit), outputType: typeof(TOut))),
                SupportProjection p when p.Accepts(output: typeof(TOut)) && ClosestHit.CanProjectTo(output: typeof(TOut), parameterMode: p.ParameterMode) => hit.Project<TOut>(key: key, parameterMode: p.ParameterMode)
                    .Bind(values => values.Head.ToFin(key.InvalidResult())),
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
    public static readonly VectorRingMetric PrincipalAxes = new(key: 7, output: typeof(Seq<Vector3d>), measure: static (ring, key) => ring.PrincipalAxes(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric PrincipalFrame = new(key: 8, output: typeof(Plane), measure: static (ring, key) => ring.PrincipalFrame(key: key).Map(static value => (object)value));
    public static readonly VectorRingMetric Shape = new(key: 9, output: typeof(VectorRingShape), measure: static (ring, key) => ring.Shape(key: key).Map(static value => (object)value));
    public Type Output { get; }
    [UseDelegateFromConstructor] private partial Fin<object> Measure(VectorRing ring, Op key);
    internal Fin<TOut> Project<TOut>(VectorRing ring, Op key) =>
        Output.Equals(typeof(TOut)) switch {
            true => Measure(ring: ring, key: key).Bind(value => value switch {
                Seq<Vector3d> axes => axes.TraverseM(axis => key.AcceptValue(value: axis)).As().Map(static valid => (TOut)(object)valid),
                VectorRingShape shape when shape.IsValid => Fin.Succ((TOut)(object)shape),
                VectorRingShape => Fin.Fail<TOut>(key.InvalidResult()),
                _ => key.AcceptValue(value: value).Map(static valid => (TOut)valid),
            }),
            false => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorRing), outputType: typeof(TOut))),
        };
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record VectorIntent {
    private VectorIntent() { }
    public sealed record AxisCase(SignedAxis Value, Option<Plane> Frame) : VectorIntent;
    public sealed record DirectionCase(Vector3d Value) : VectorIntent;
    public sealed record AxesCase(Option<Seq<Vector3d>> Values, bool Planar) : VectorIntent;
    public sealed record AngularCase(Vector3d A, Vector3d B, AnglePivot Pivot) : VectorIntent;
    public sealed record SupportCase(SupportSpace Space, Point3d Sample, SupportProjection Projection) : VectorIntent;
    public sealed record FieldCase(VectorField Value, Point3d Sample) : VectorIntent;
    public sealed record RayCase(Point3d Origin, Direction RayDirection, RayPolicy Policy) : VectorIntent;
    public sealed record RingCase(Seq<Point3d> Points, VectorRingMetric Metric) : VectorIntent;
    public sealed record ComponentsCase(Point3d Anchor, Vector3d Value, Plane Frame) : VectorIntent;
    public sealed record RelationCase(Vector3d A, Vector3d B) : VectorIntent;
    public sealed record BounceCase(Direction Incident, SupportSpace Surface, Point3d Sample, BouncePolicy Policy) : VectorIntent;
    public sealed record StreamlineCase(VectorField Source, Point3d Seed, PositiveMagnitude StepSize, int Steps, FieldIntegrator Integrator) : VectorIntent;
    public Fin<TOut> Project<TOut>(Context context, Op op) => Switch(
        state: (Context: context, Key: op),
        axisCase: static (state, axis) =>
            from direction in Rasm.Vectors.Direction.Of(value: axis.Value.Of(frame: axis.Frame), context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        directionCase: static (state, intent) =>
            from direction in Rasm.Vectors.Direction.Of(value: intent.Value, context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        axesCase: static (state, intent) =>
            from axes in intent.Values.IfNone(SignedAxis.Cardinal(planar: intent.Planar).Map(static axis => axis.World))
                .TraverseM(axis => Rasm.Vectors.Direction.Of(value: axis, context: state.Context, key: state.Key).Map(static direction => direction.Value))
                .As()
            from _ in guard(!axes.IsEmpty, state.Key.InvalidInput())
            from output in typeof(TOut) == typeof(Seq<Vector3d>)
                ? Fin.Succ((TOut)(object)axes)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(AxesCase), outputType: typeof(TOut)))
            select output,
        angularCase: static (state, intent) =>
            from angle in VectorAngle.Of(a: intent.A, b: intent.B, context: state.Context, pivot: intent.Pivot, key: state.Key)
            from output in angle.Project<TOut>(key: state.Key)
            select output,
        supportCase: static (state, intent) =>
            from hit in intent.Space.Closest(sample: intent.Sample, key: state.Key)
            from output in intent.Projection.Project<TOut>(space: intent.Space, hit: hit, sample: intent.Sample, context: state.Context, key: state.Key)
            select output,
        fieldCase: static (state, intent) => intent.Value.Project<TOut>(sample: intent.Sample, context: state.Context, key: state.Key),
        rayCase: static (state, intent) => intent.Policy.Project<TOut>(origin: intent.Origin, direction: intent.RayDirection, context: state.Context, key: state.Key),
        ringCase: static (state, intent) =>
            from ring in VectorRing.Of(points: intent.Points, context: state.Context, key: state.Key)
            from output in intent.Metric.Project<TOut>(ring: ring, key: state.Key)
            select output,
        componentsCase: static (state, intent) =>
            from span in VectorSpan.Of(anchor: intent.Anchor, vector: intent.Value, context: state.Context, key: state.Key)
            from components in span.Components(frame: intent.Frame, key: state.Key)
            from output in typeof(TOut) == typeof(ValueTuple<double, double>)
                ? state.Key.AcceptValue(value: components).Map(static value => (TOut)(object)value)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(VectorSpan), outputType: typeof(TOut)))
            select output,
        relationCase: static (state, intent) =>
            from relation in VectorRelation.Of(a: intent.A, b: intent.B, context: state.Context, key: state.Key)
            from output in relation.Project<TOut>(key: state.Key)
            select output,
        bounceCase: static (state, intent) =>
            from hit in intent.Surface.Closest(sample: intent.Sample, key: state.Key)
            from rawNormal in hit.Normal.ToFin(Fail: state.Key.InvalidResult())
            from normal in Rasm.Vectors.Direction.Of(value: rawNormal, context: state.Context, key: state.Key)
            from reflected in intent.Policy.Apply(incident: intent.Incident, normal: normal, key: state.Key)
            from output in reflected.Project<TOut>(key: state.Key)
            select output,
        streamlineCase: static (state, intent) =>
            from seed in state.Key.AcceptValue(value: intent.Seed)
            from path in toSeq(Enumerable.Range(start: 0, count: intent.Steps)).Fold(
                initialState: Fin.Succ((Trail: Seq(seed), Current: seed, intent.Source, intent.StepSize, intent.Integrator, state.Context, state.Key)),
                f: static (acc, _) => acc.Bind(s => s.Integrator.Step(field: s.Source, point: s.Current, h: s.StepSize.Value, context: s.Context, key: s.Key)
                    .Map(next => (Trail: next.Cons(s.Trail), Current: next, s.Source, s.StepSize, s.Integrator, s.Context, s.Key))))
            let trajectory = path.Trail.Rev()
            from output in typeof(TOut) switch {
                Type t when t == typeof(Seq<Point3d>) => trajectory.TraverseM(point => state.Key.AcceptValue(value: point)).As().Map(static value => (TOut)(object)value),
                Type t when t == typeof(Polyline) => state.Key.AcceptValue(value: new Polyline(trajectory.AsIterable())).Map(static value => (TOut)(object)value),
                _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(StreamlineCase), outputType: typeof(TOut))),
            }
            select output);
    public static VectorIntent Between(Point3d origin, SupportSpace target, BoundarySense? sense = null) =>
        new SupportCase(Space: target, Sample: origin, Projection: (sense ?? BoundarySense.Toward).Equals(BoundarySense.Toward) ? SupportProjection.Span : SupportProjection.SignedSpanAway);
    public static VectorIntent Axis(SignedAxis axis, Plane? frame = null) =>
        new AxisCase(Value: axis, Frame: Optional(frame));
    public static VectorIntent Direction(Vector3d value) =>
        new DirectionCase(Value: value);
    public static VectorIntent Axes(Option<Seq<Vector3d>> values = default, bool planar = false) =>
        new AxesCase(Values: values, Planar: planar);
    public static VectorIntent Angular(Vector3d a, Vector3d b, AnglePivot? pivot = null) =>
        new AngularCase(A: a, B: b, Pivot: pivot ?? AnglePivot.World);
    public static VectorIntent Support(SupportSpace space, Point3d sample, SupportProjection projection) =>
        new SupportCase(Space: space, Sample: sample, Projection: projection);
    public static VectorIntent Field(VectorField field, Point3d sample) =>
        new FieldCase(Value: field, Sample: sample);
    public static VectorIntent Ray(Point3d origin, Direction direction, RayPolicy? policy = null) =>
        new RayCase(Origin: origin, RayDirection: direction, Policy: policy ?? RayPolicy.Forward);
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

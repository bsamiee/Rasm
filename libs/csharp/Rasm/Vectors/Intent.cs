namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = new(key: 0, parameterMode: false, sign: 1.0, capability: static (_, _) => true, accepts: static output => output == typeof(Point3d) || output == typeof(ClosestHit));
    public static readonly SupportProjection Direction = new(key: 1, parameterMode: false, sign: 1.0, capability: static (_, _) => true, accepts: static _ => false);
    public static readonly SupportProjection Span = new(key: 2, parameterMode: false, sign: 1.0, capability: static (_, _) => true, accepts: static _ => false);
    public static readonly SupportProjection SignedSpanAway = new(key: 13, parameterMode: false, sign: -1.0, capability: static (_, _) => true, accepts: static _ => false);
    public static readonly SupportProjection Normal = new(key: 3, parameterMode: false, sign: 1.0, capability: static (space, hit) => space.AdmitsNormal(hit: hit), accepts: static output => output == typeof(ClosestHit));
    public static readonly SupportProjection Distance = new(key: 4, parameterMode: false, sign: 1.0, capability: static (_, hit) => hit.Distance.IsSome, accepts: static output => output == typeof(double) || output == typeof(ClosestHit));
    public static readonly SupportProjection Parameter = new(key: 5, parameterMode: true, sign: 1.0, capability: static (_, hit) => hit.Parameter.IsSome, accepts: static output => output == typeof(double) || output == typeof(ClosestHit));
    public static readonly SupportProjection Uv = new(key: 6, parameterMode: false, sign: 1.0, capability: static (_, hit) => hit.Uv.IsSome, accepts: static output => output == typeof(Point2d) || output == typeof(ClosestHit));
    public static readonly SupportProjection Component = new(key: 7, parameterMode: false, sign: 1.0, capability: static (_, hit) => hit.Component.IsSome, accepts: static output => output == typeof(ComponentIndex) || output == typeof(ClosestHit));
    public static readonly SupportProjection MeshPoint = new(key: 8, parameterMode: false, sign: 1.0, capability: static (_, hit) => hit.MeshPoint.IsSome, accepts: static output => output == typeof(MeshPoint) || output == typeof(ClosestHit));
    public static readonly SupportProjection SignedDistance = new(key: 9, parameterMode: false, sign: 1.0, capability: static (space, hit) => space.AdmitsSignedDistance(hit: hit), accepts: static output => output == typeof(double));
    public static readonly SupportProjection ContainmentDistance = new(key: 10, parameterMode: false, sign: 1.0, capability: static (space, hit) => space.AdmitsContainmentDistance(hit: hit), accepts: static output => output == typeof(double));
    public static readonly SupportProjection Tangent = new(key: 11, parameterMode: false, sign: 1.0, capability: static (space, hit) => space.AdmitsTangent(hit: hit), accepts: static output => output == typeof(ClosestHit));
    public static readonly SupportProjection Frame = new(key: 12, parameterMode: false, sign: 1.0, capability: static (space, hit) => space.AdmitsFrame(hit: hit), accepts: static output => output == typeof(Plane) || output == typeof(ClosestHit));
    internal bool ParameterMode { get; }
    internal double Sign { get; }
    [UseDelegateFromConstructor] private partial bool Capability(SupportSpace space, ClosestHit hit);
    [UseDelegateFromConstructor] private partial bool Accepts(Type output);
    internal Fin<TOut> Project<TOut>(SupportSpace space, ClosestHit hit, Point3d sample, Context context, Op key) =>
        Capability(space: space, hit: hit) switch {
            false => Fin.Fail<TOut>(error: key.Unsupported(geometryType: space.SourceType, outputType: typeof(TOut))),
            true => this switch {
                SupportProjection p when p.Equals(SignedDistance) || p.Equals(ContainmentDistance) => typeof(TOut) == typeof(double)
                    ? (p.Equals(SignedDistance) ? space.SignedDistance(hit: hit, sample: sample, key: key) : space.ContainmentDistance(hit: hit, sample: sample, context: context, key: key))
                        .Bind(distance => key.AcceptValue(value: distance))
                        .Map(static value => (TOut)(object)value)
                    : Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(ClosestHit), outputType: typeof(TOut))),
                SupportProjection p when p.Accepts(output: typeof(TOut)) && ClosestHit.CanProjectTo(output: typeof(TOut), parameterMode: p.ParameterMode) => hit.Project<TOut>(key: key, parameterMode: p.ParameterMode)
                    .Bind(values => values.Head.ToFin(key.InvalidResult())),
                SupportProjection p when p.Equals(Direction) => Vectors.Direction.Of(value: hit.Point - sample, context: context, key: key)
                    .Bind(direction => direction.Project<TOut>(key: key)),
                SupportProjection p when p.Equals(Span) || p.Equals(SignedSpanAway) => VectorSpan.Of(anchor: sample, vector: p.Sign * (hit.Point - sample), context: context, key: key)
                    .Bind(span => span.Project<TOut>(key: key)),
                SupportProjection p when p.Equals(Normal) => hit.Normal.ToFin(Fail: key.InvalidResult())
                    .Bind(normal => Vectors.Direction.Of(value: normal, context: context, key: key))
                    .Bind(direction => direction.Project<TOut>(key: key)),
                SupportProjection p when p.Equals(Tangent) => hit.Tangent.ToFin(Fail: key.InvalidResult())
                    .Bind(tangent => Vectors.Direction.Of(value: tangent, context: context, key: key))
                    .Bind(direction => direction.Project<TOut>(key: key)),
                _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(SupportProjection), outputType: typeof(TOut))),
            },
        };
}

internal readonly record struct StreamlineState(Seq<Point3d> Trail, Point3d Current, double H, double Arc, int Steps, int Rejects, bool Done);

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record VectorIntent {
    private VectorIntent() { }
    public sealed record AxisCase(SignedAxis Value, Option<Plane> Basis) : VectorIntent;
    public sealed record DirectionCase(Vector3d Value) : VectorIntent;
    public sealed record AxesCase(Option<Seq<Vector3d>> Values, bool Planar) : VectorIntent;
    public sealed record AngularCase(Vector3d A, Vector3d B, AnglePivot Pivot) : VectorIntent;
    public sealed record SupportCase(SupportSpace Space, Point3d Sample, SupportProjection Projection) : VectorIntent;
    public sealed record VectorFieldCase(VectorField Value, Point3d Sample) : VectorIntent;
    public sealed record ScalarFieldCase(ScalarField Value, Point3d Sample) : VectorIntent;
    public sealed record RayCase(Point3d Origin, Direction RayDirection, RayPolicy Policy) : VectorIntent;
    public sealed record FrameCase(Point3d Origin, Vector3d Normal, Option<Vector3d> XHint) : VectorIntent;
    public sealed record CurveCase(Curve Source, double Parameter, CurveProjection Mode) : VectorIntent;
    public sealed record CloudCase(VectorCloud Value, VectorCloudMetric Metric) : VectorIntent;
    public sealed record WindingCase(VectorCloud Value, Point3d Query) : VectorIntent;
    public sealed record ConeCase(VectorCone Value, ConeProjection Mode) : VectorIntent;
    public sealed record ComponentsCase(Point3d Anchor, Vector3d Value, Plane Basis) : VectorIntent;
    public sealed record RelationCase(Vector3d A, Vector3d B) : VectorIntent;
    public sealed record BounceCase(Direction Incident, SupportSpace Surface, Point3d Sample, BouncePolicy Policy) : VectorIntent;
    public sealed record StreamlineCase(VectorField Source, Point3d Seed, PositiveMagnitude InitialStep, FieldIntegrator Integrator, Termination Termination) : VectorIntent;
    public Fin<TOut> Project<TOut>(Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Optional(context).ToFin(op.MissingContext())
               from result in Dispatch<TOut>(context: model, op: op)
               select result;
    }
    private Fin<TOut> Dispatch<TOut>(Context context, Op op) => Switch(
        state: (Context: context, Key: op),
        axisCase: static (state, axis) =>
            from direction in Vectors.Direction.Of(value: axis.Value.Of(frame: axis.Basis), context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        directionCase: static (state, intent) =>
            from direction in Vectors.Direction.Of(value: intent.Value, context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        axesCase: static (state, intent) =>
            from axes in intent.Values.IfNone(SignedAxis.Cardinal(planar: intent.Planar).Map(static axis => axis.World))
                .TraverseM(axis => Vectors.Direction.Of(value: axis, context: state.Context, key: state.Key).Map(static direction => direction.Value))
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
        vectorFieldCase: static (state, intent) => intent.Value.Project<TOut>(sample: intent.Sample, context: state.Context, key: state.Key),
        scalarFieldCase: static (state, intent) => intent.Value.Project<TOut>(sample: intent.Sample, context: state.Context, key: state.Key),
        rayCase: static (state, intent) => intent.Policy.Project<TOut>(origin: intent.Origin, direction: intent.RayDirection, context: state.Context, key: state.Key),
        frameCase: static (state, intent) =>
            from frame in VectorFrame.Of(origin: intent.Origin, normal: intent.Normal, xHint: intent.XHint, context: state.Context, key: state.Key)
            from output in frame.Project<TOut>(key: state.Key)
            select output,
        curveCase: static (state, intent) => intent.Mode.Project<TOut>(curve: intent.Source, parameter: intent.Parameter, context: state.Context, key: state.Key),
        cloudCase: static (state, intent) => intent.Metric.Project<TOut>(cloud: intent.Value, key: state.Key),
        windingCase: static (state, intent) => intent.Value.Switch(
            state: (state.Key, intent.Query),
            ringCase: static (s, ring) =>
                from normal in CloudKernel.RingNormalOf(ring: ring, key: s.Key)
                from winding in CloudKernel.PlanarWindingOf(ring: ring.Vertices, planeNormal: normal, query: s.Query, key: s.Key)
                from output in typeof(TOut) == typeof(int)
                    ? Fin.Succ((TOut)(object)winding)
                    : Fin.Fail<TOut>(error: s.Key.Unsupported(geometryType: typeof(WindingCase), outputType: typeof(TOut)))
                select output,
            polylineCase: static (s, c) => Fin.Fail<TOut>(error: s.Key.Unsupported(geometryType: c.GetType(), outputType: typeof(int))),
            clusterCase: static (s, c) => Fin.Fail<TOut>(error: s.Key.Unsupported(geometryType: c.GetType(), outputType: typeof(int)))),
        coneCase: static (state, intent) => intent.Mode.Project<TOut>(cone: intent.Value, key: state.Key),
        componentsCase: static (state, intent) =>
            from span in VectorSpan.Of(anchor: intent.Anchor, vector: intent.Value, context: state.Context, key: state.Key)
            from components in span.Components(frame: intent.Basis, key: state.Key)
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
            from normal in Vectors.Direction.Of(value: rawNormal, context: state.Context, key: state.Key)
            from reflected in intent.Policy.Apply(incident: intent.Incident, normal: normal, key: state.Key)
            from output in reflected.Project<TOut>(key: state.Key)
            select output,
        streamlineCase: static (state, intent) =>
            from trajectory in toSeq(Enumerable.Range(start: 0, count: MaxIterations)).Fold(
                initialState: Fin.Succ(new StreamlineState(Trail: Seq(intent.Seed), Current: intent.Seed, H: intent.InitialStep.Value, Arc: 0.0, Steps: 0, Rejects: 0, Done: false)),
                f: (acc, _) => acc.Bind(s => s.Done
                    ? Fin.Succ(s)
                    : intent.Source.SampleVector(sample: s.Current, context: state.Context, key: state.Key).Bind(vector =>
                        intent.Termination.ShouldStop(stepCount: s.Steps, arcLengthSoFar: s.Arc, currentSample: vector)
                            ? Fin.Succ(s with { Done = true })
                            : intent.Integrator.Step(field: intent.Source, point: s.Current, h: s.H, context: state.Context, key: state.Key).Map(step => step.Accepted switch {
                                true => s with {
                                    Trail = s.Trail.Add(step.Next),
                                    Current = step.Next,
                                    H = step.SuggestedStep,
                                    Arc = s.Arc + step.Next.DistanceTo(other: s.Current),
                                    Steps = s.Steps + 1,
                                    Rejects = 0,
                                },
                                false => s.Rejects >= intent.Integrator.RejectBudget
                                    ? s with { Done = true }
                                    : s with { H = step.SuggestedStep, Rejects = s.Rejects + 1 },
                            }))))
                .Map(static s => s.Trail)
            from output in typeof(TOut) switch {
                Type t when t == typeof(Seq<Point3d>) => trajectory.TraverseM(point => state.Key.AcceptValue(value: point)).As().Map(static value => (TOut)(object)value),
                Type t when t == typeof(Polyline) => state.Key.AcceptValue(value: new Polyline(trajectory.AsIterable())).Map(static value => (TOut)(object)value),
                _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(StreamlineCase), outputType: typeof(TOut))),
            }
            select output);
    public static VectorIntent Between(Point3d origin, SupportSpace target, BoundarySense? sense = null) =>
        new SupportCase(Space: target, Sample: origin, Projection: (sense ?? BoundarySense.Toward).Equals(BoundarySense.Toward) ? SupportProjection.Span : SupportProjection.SignedSpanAway);
    public static VectorIntent Axis(SignedAxis axis, Plane? frame = null) =>
        new AxisCase(Value: axis, Basis: Optional(frame));
    public static VectorIntent Direction(Vector3d value) =>
        new DirectionCase(Value: value);
    public static VectorIntent Axes(Option<Seq<Vector3d>> values = default, bool planar = false) =>
        new AxesCase(Values: values, Planar: planar);
    public static VectorIntent Angular(Vector3d a, Vector3d b, AnglePivot? pivot = null) =>
        new AngularCase(A: a, B: b, Pivot: pivot ?? AnglePivot.World);
    public static VectorIntent Support(SupportSpace space, Point3d sample, SupportProjection projection) =>
        new SupportCase(Space: space, Sample: sample, Projection: projection);
    public static VectorIntent Field(VectorField field, Point3d sample) =>
        new VectorFieldCase(Value: field, Sample: sample);
    public static VectorIntent Scalar(ScalarField field, Point3d sample) =>
        new ScalarFieldCase(Value: field, Sample: sample);
    public static VectorIntent Ray(Point3d origin, Direction direction, RayPolicy? policy = null) =>
        new RayCase(Origin: origin, RayDirection: direction, Policy: policy ?? RayPolicy.Forward);
    public static VectorIntent Frame(Point3d origin, Vector3d normal, Option<Vector3d> xHint = default) =>
        new FrameCase(Origin: origin, Normal: normal, XHint: xHint);
    public static VectorIntent Curve(Curve source, double parameter, CurveProjection mode) =>
        new CurveCase(Source: source, Parameter: parameter, Mode: mode);
    public static Fin<VectorIntent> Cloud(VectorCloud cloud, VectorCloudMetric metric, Op? key = null) {
        Op op = key.OrDefault();
        return from validCloud in Optional(cloud).ToFin(op.InvalidInput())
               from validMetric in Optional(metric).ToFin(op.InvalidInput())
               from _ in guard(validMetric.AdmitsCase(cloud: validCloud), op.Unsupported(geometryType: validCloud.GetType(), outputType: validMetric.Output))
               select (VectorIntent)new CloudCase(Value: validCloud, Metric: validMetric);
    }
    public static Fin<VectorIntent> Winding(VectorCloud cloud, Point3d query, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(cloud).ToFin(op.InvalidInput())
            .Bind(valid => valid is VectorCloud.RingCase
                ? op.AcceptValue(value: query).Map(point => (VectorIntent)new WindingCase(Value: valid, Query: point))
                : Fin.Fail<VectorIntent>(op.Unsupported(geometryType: valid.GetType(), outputType: typeof(int))));
    }
    public static VectorIntent Cone(VectorCone cone, ConeProjection mode) =>
        new ConeCase(Value: cone, Mode: mode);
    public static VectorIntent Components(Point3d anchor, Vector3d value, Plane frame) =>
        new ComponentsCase(Anchor: anchor, Value: value, Basis: frame);
    public static VectorIntent Relation(Vector3d a, Vector3d b) =>
        new RelationCase(A: a, B: b);
    public static VectorIntent Bounce(Direction incident, SupportSpace surface, Point3d sample, BouncePolicy? policy = null) =>
        new BounceCase(Incident: incident, Surface: surface, Sample: sample, Policy: policy ?? BouncePolicy.Reflect);
    public static Fin<VectorIntent> Streamline(VectorField field, Point3d seed, double initialStep, Termination termination, FieldIntegrator? integrator = null, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: initialStep)
            .Map(h => (VectorIntent)new StreamlineCase(Source: field, Seed: seed, InitialStep: h, Integrator: integrator ?? FieldIntegrator.RK4, Termination: termination));
    }
    private const int MaxIterations = 100000;
}

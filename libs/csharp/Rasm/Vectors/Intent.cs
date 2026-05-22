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
    public sealed record LerpCase(Vector3d A, Vector3d B, double Parameter) : VectorIntent;
    public sealed record SlerpCase(Direction A, Direction B, double Parameter) : VectorIntent;
    public sealed record ProjectOntoCase(Vector3d Value, Plane Target) : VectorIntent;
    public sealed record MirrorCase(Vector3d Value, Plane Across) : VectorIntent;
    public sealed record SurfaceCase(SurfaceSpace SurfaceSource, double U, double V, SurfaceProjection Mode) : VectorIntent;
    public sealed record PoseCase(Plane From, Plane To, double Parameter, MotionInterpolation Mode) : VectorIntent;
    public sealed record TensorCase(TensorField Source, Point3d Point) : VectorIntent;
    public sealed record MeshOperatorCase(ScalarField MeshField, Point3d Point) : VectorIntent;
    public sealed record FlattenCase(MeshSpace Space, SurfaceParameterization Kind, Seq<int> ConeVertices) : VectorIntent;
    public sealed record HullCase(VectorCloud Source, HullKind Kind) : VectorIntent;
    public sealed record SampleCase(MeshSpace Domain, SamplingKind Kind) : VectorIntent;
    public sealed record RegisterCase(VectorCloud Source, VectorCloud Target, RegistrationKind Kind) : VectorIntent;
    public sealed record RemeshCase(MeshSpace Space, RemeshKind Kind) : VectorIntent;
    public sealed record TransportCase(VectorCloud Source, VectorCloud Target, double Regularization, int IterationCap, bool Unbiased) : VectorIntent;
    public sealed record TopologyCase(MeshSpace Space) : VectorIntent;
    public sealed record FeaturesCase(MeshSpace Space, double DihedralRadians) : VectorIntent;
    public sealed record DescriptorCase(MeshSpace Space, MeshDescriptor Kind, int EigenpairCount) : VectorIntent;
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
        windingCase: static (state, intent) => intent.Value switch {
            VectorCloud.RingCase ring =>
                from normal in CloudKernel.RingNormalOf(ring: ring, key: state.Key)
                from winding in CloudKernel.PlanarWindingOf(ring: ring.Vertices, planeNormal: normal, query: intent.Query, key: state.Key)
                from output in typeof(TOut) == typeof(int)
                    ? Fin.Succ((TOut)(object)winding)
                    : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(WindingCase), outputType: typeof(TOut)))
                select output,
            _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: intent.Value.GetType(), outputType: typeof(int))),
        },
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
                        intent.Termination.ShouldStop(state: s, currentSample: vector, context: state.Context, key: state.Key)
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
            select output,
        lerpCase: static (state, intent) =>
            from direction in Vectors.Direction.Of(
                value: ((1.0 - intent.Parameter) * intent.A) + (intent.Parameter * intent.B),
                context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        slerpCase: static (state, intent) => Math.Acos(d: Math.Clamp(value: intent.A.Value * intent.B.Value, min: -1.0, max: 1.0)) switch {
            double theta when Math.Abs(value: Math.Sin(a: theta)) < RhinoMath.ZeroTolerance =>
                Vectors.Direction.Of(value: intent.A.Value, context: state.Context, key: state.Key).Bind(d => d.Project<TOut>(key: state.Key)),
            double theta =>
                Vectors.Direction.Of(
                    value: (Math.Sin(a: (1.0 - intent.Parameter) * theta) / Math.Sin(a: theta) * intent.A.Value)
                        + (Math.Sin(a: intent.Parameter * theta) / Math.Sin(a: theta) * intent.B.Value),
                    context: state.Context, key: state.Key).Bind(d => d.Project<TOut>(key: state.Key)),
        },
        projectOntoCase: static (state, intent) =>
            from direction in Vectors.Direction.Of(
                value: intent.Value - (intent.Value * intent.Target.ZAxis * intent.Target.ZAxis),
                context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        mirrorCase: static (state, intent) =>
            from direction in Vectors.Direction.Of(
                value: intent.Value - (2.0 * (intent.Value * intent.Across.ZAxis) * intent.Across.ZAxis),
                context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        surfaceCase: static (state, intent) => intent.SurfaceSource.Sample<TOut>(projection: intent.Mode, u: intent.U, v: intent.V, key: state.Key),
        poseCase: static (state, intent) => intent.Mode.Interpolate(a: intent.From, b: intent.To, t: Math.Clamp(value: intent.Parameter, min: 0.0, max: 1.0)) is Plane p
            && typeof(TOut) == typeof(Plane)
                ? state.Key.AcceptValue(value: p).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(PoseCase), outputType: typeof(TOut))),
        tensorCase: static (state, intent) =>
            from tensor in intent.Source.SampleTensor(sample: intent.Point, context: state.Context, key: state.Key)
            from output in typeof(TOut) switch {
                Type t when t == typeof(SymmetricMatrix) => state.Key.AcceptValue(value: tensor).Map(static v => (TOut)(object)v),
                Type t when t == typeof(Seq<(double Eigenvalue, Direction Eigenvector)>) =>
                    intent.Source.PrincipalDirections(sample: intent.Point, context: state.Context, key: state.Key).Map(static p => (TOut)(object)p),
                _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(TensorCase), outputType: typeof(TOut))),
            }
            select output,
        meshOperatorCase: static (state, intent) => intent.MeshField.Project<TOut>(sample: intent.Point, context: state.Context, key: state.Key),
        flattenCase: static (state, intent) =>
            from coords in intent.Kind.Compute(space: intent.Space, coneVertices: intent.ConeVertices, key: state.Key)
            from output in typeof(TOut) == typeof(Arr<Point2d>)
                ? state.Key.AcceptValue(value: coords).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(FlattenCase), outputType: typeof(TOut)))
            select output,
        hullCase: static (state, intent) =>
            from cloud in intent.Kind.Compute(source: intent.Source, context: state.Context, key: state.Key)
            from output in typeof(TOut) == typeof(VectorCloud)
                ? state.Key.AcceptValue(value: cloud).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(HullCase), outputType: typeof(TOut)))
            select output,
        sampleCase: static (state, intent) =>
            from cloud in intent.Kind.Sample(domain: intent.Domain, context: state.Context, key: state.Key)
            from output in typeof(TOut) == typeof(VectorCloud)
                ? state.Key.AcceptValue(value: cloud).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(SampleCase), outputType: typeof(TOut)))
            select output,
        registerCase: static (state, intent) =>
            from dq in intent.Kind.Align(source: intent.Source, target: intent.Target, context: state.Context, key: state.Key)
            from output in typeof(TOut) switch {
                Type t when t == typeof(DualQuaternion) => state.Key.AcceptValue(value: dq).Map(static v => (TOut)(object)v),
                Type t when t == typeof(Transform) => state.Key.AcceptValue(value: dq.ToTransform()).Map(static v => (TOut)(object)v),
                _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(RegisterCase), outputType: typeof(TOut))),
            }
            select output,
        remeshCase: static (state, intent) =>
            from mesh in intent.Kind.Apply(space: intent.Space, context: state.Context, key: state.Key)
            from output in typeof(TOut) == typeof(Mesh)
                ? state.Key.AcceptValue(value: mesh).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(RemeshCase), outputType: typeof(TOut)))
            select output,
        transportCase: static (state, intent) => IntentKernel.Sinkhorn<TOut>(source: intent.Source, target: intent.Target, regularization: intent.Regularization, maxIterations: intent.IterationCap, unbiased: intent.Unbiased, key: state.Key),
        topologyCase: static (state, intent) =>
            from euler in intent.Space.EulerCharacteristic(key: state.Key)
            from output in typeof(TOut) == typeof(int)
                ? state.Key.AcceptValue(value: euler).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(TopologyCase), outputType: typeof(TOut)))
            select output,
        featuresCase: static (state, intent) =>
            from edges in intent.Space.FeatureEdges(dihedralRadians: intent.DihedralRadians, key: state.Key)
            from output in typeof(TOut) == typeof(Seq<(int A, int B)>)
                ? state.Key.AcceptValue(value: edges).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(FeaturesCase), outputType: typeof(TOut)))
            select output,
        descriptorCase: static (state, intent) => IntentKernel.DescribeShape<TOut>(space: intent.Space, kind: intent.Kind, eigenpairs: intent.EigenpairCount, key: state.Key));
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
    public static VectorIntent Lerp(Vector3d a, Vector3d b, double t) =>
        new LerpCase(A: a, B: b, Parameter: Math.Clamp(value: t, min: 0.0, max: 1.0));
    public static VectorIntent Slerp(Direction a, Direction b, double t) =>
        new SlerpCase(A: a, B: b, Parameter: Math.Clamp(value: t, min: 0.0, max: 1.0));
    public static VectorIntent ProjectOnto(Vector3d value, Plane target) =>
        new ProjectOntoCase(Value: value, Target: target);
    public static VectorIntent Mirror(Vector3d value, Plane across) =>
        new MirrorCase(Value: value, Across: across);
    public static VectorIntent OnSurface(SurfaceSpace space, double u, double v, SurfaceProjection mode) =>
        new SurfaceCase(SurfaceSource: space, U: u, V: v, Mode: mode);
    public static VectorIntent Pose(Plane from, Plane to, double t, MotionInterpolation mode) =>
        new PoseCase(From: from, To: to, Parameter: t, Mode: mode);
    public static VectorIntent Tensor(TensorField source, Point3d point) =>
        new TensorCase(Source: source, Point: point);
    public static VectorIntent MeshOperator(ScalarField meshField, Point3d point) =>
        new MeshOperatorCase(MeshField: meshField, Point: point);
    public static VectorIntent Flatten(MeshSpace space, SurfaceParameterization kind, Seq<int> coneVertices = default) =>
        new FlattenCase(Space: space, Kind: kind, ConeVertices: coneVertices);
    public static VectorIntent Hull(VectorCloud source, HullKind kind) =>
        new HullCase(Source: source, Kind: kind);
    public static VectorIntent Populate(MeshSpace domain, SamplingKind kind) =>
        new SampleCase(Domain: domain, Kind: kind);
    public static VectorIntent Register(VectorCloud source, VectorCloud target, RegistrationKind kind) =>
        new RegisterCase(Source: source, Target: target, Kind: kind);
    public static VectorIntent Remesh(MeshSpace space, RemeshKind kind) =>
        new RemeshCase(Space: space, Kind: kind);
    public static Fin<VectorIntent> Transport(VectorCloud source, VectorCloud target, double regularization, int iterationCap, bool unbiased = false, Op? key = null) {
        Op op = key.OrDefault();
        return RhinoMath.IsValidDouble(x: regularization) && regularization > 0.0 && iterationCap >= 1
            ? Fin.Succ((VectorIntent)new TransportCase(Source: source, Target: target, Regularization: regularization, IterationCap: iterationCap, Unbiased: unbiased))
            : Fin.Fail<VectorIntent>(op.InvalidInput());
    }
    public static VectorIntent Topology(MeshSpace space) =>
        new TopologyCase(Space: space);
    public static Fin<VectorIntent> Features(MeshSpace space, double dihedralRadians, Op? key = null) =>
        RhinoMath.IsValidDouble(x: dihedralRadians) && dihedralRadians > 0.0
            ? Fin.Succ((VectorIntent)new FeaturesCase(Space: space, DihedralRadians: dihedralRadians))
            : Fin.Fail<VectorIntent>(key.OrDefault().InvalidInput());
    public static Fin<VectorIntent> Descriptor(MeshSpace space, MeshDescriptor kind, int eigenpairCount, Op? key = null) =>
        eigenpairCount >= 1
            ? Fin.Succ((VectorIntent)new DescriptorCase(Space: space, Kind: kind, EigenpairCount: eigenpairCount))
            : Fin.Fail<VectorIntent>(key.OrDefault().InvalidInput());
    private const int MaxIterations = 100000;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class IntentKernel {
    // Cuturi 2013 Sinkhorn-Knopp: K = exp(-C/eps); iterate u = a/(K v), v = b/(K^T u);
    // coupling = diag(u) K diag(v); Sinkhorn distance is <coupling, C>.
    internal static Fin<TOut> Sinkhorn<TOut>(VectorCloud source, VectorCloud target, double regularization, int maxIterations, bool unbiased, Op key) =>
        (source, target) switch {
            (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt) => SinkhornCluster<TOut>(source: src, target: tgt, regularization: regularization, maxIterations: maxIterations, unbiased: unbiased, key: key),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: source.GetType(), outputType: typeof(TOut))),
        };
    // ALGORITHM KERNEL — Sinkhorn-Knopp requires mutable row/column scaling vectors; the
    // alternating fixed-point cannot be expressed without state mutation.
    private static Fin<TOut> SinkhornCluster<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double regularization, int maxIterations, bool unbiased, Op key) {
        int m = source.Vertices.Count; int n = target.Vertices.Count;
        if (m < 1 || n < 1) return Fin.Fail<TOut>(key.InvalidInput());
        double[][] cost = [.. Enumerable.Range(0, m).Select(_ => new double[n])];
        double[][] kernel = [.. Enumerable.Range(0, m).Select(_ => new double[n])];
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++) {
                double d2 = source.Vertices[index: i].DistanceToSquared(other: target.Vertices[index: j]);
                cost[i][j] = d2;
                kernel[i][j] = Math.Exp(d: -d2 / regularization);
            }
        double[] u = [.. Enumerable.Repeat(element: 1.0, count: m)];
        double[] v = [.. Enumerable.Repeat(element: 1.0, count: n)];
        double aMass = 1.0 / m; double bMass = 1.0 / n;
        for (int iter = 0; iter < maxIterations; iter++) {
            for (int i = 0; i < m; i++) {
                double sum = 0.0;
                for (int j = 0; j < n; j++) sum += kernel[i][j] * v[j];
                u[i] = sum > RhinoMath.ZeroTolerance ? aMass / sum : aMass;
            }
            for (int j = 0; j < n; j++) {
                double sum = 0.0;
                for (int i = 0; i < m; i++) sum += kernel[i][j] * u[i];
                v[j] = sum > RhinoMath.ZeroTolerance ? bMass / sum : bMass;
            }
        }
        double distance = 0.0;
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++) distance += u[i] * kernel[i][j] * v[j] * cost[i][j];
        return typeof(TOut) switch {
            Type t when t == typeof(double) => key.AcceptValue(value: distance).Map(static d => (TOut)(object)d),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorCloud), outputType: typeof(TOut))),
        };
    }
    // Sun-Ovsjanikov-Guibas 2009 HKS(x, t) = Σ exp(-t λ_i) φ_i²(x); WKS analogous with log-energy.
    internal static Fin<TOut> DescribeShape<TOut>(MeshSpace space, MeshDescriptor kind, int eigenpairs, Op key) =>
        from laplacian in space.Laplacian(kind: MeshLaplacian.Cotangent, key: key)
        from eigen in laplacian.Stiffness.SmallestEigenpairs(k: eigenpairs, tolerance: 1e-6, maxIterations: 200, key: key)
        from output in ProjectDescriptor<TOut>(kind: kind, eigen: eigen, key: key)
        select output;
    private static Fin<TOut> ProjectDescriptor<TOut>(MeshDescriptor kind, Seq<(double Eigenvalue, Arr<double> Eigenvector)> eigen, Op key) =>
        kind switch {
            MeshDescriptor.ShapeDnaCase => typeof(TOut) == typeof(Seq<double>)
                ? key.AcceptValue(value: toSeq(eigen.Map(static p => p.Eigenvalue).AsIterable())).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(MeshDescriptor.ShapeDnaCase), outputType: typeof(TOut))),
            MeshDescriptor.HeatKernelSignatureCase hks => DescriptorAtTimes<TOut>(eigen: eigen, scales: hks.Times, project: static (t, lambda) => Math.Exp(d: -t * lambda), key: key),
            MeshDescriptor.WaveKernelSignatureCase wks => DescriptorAtTimes<TOut>(eigen: eigen, scales: wks.Energies, project: (e, lambda) => Math.Exp(d: -Math.Pow(x: Math.Log(d: Math.Max(val1: lambda, val2: RhinoMath.ZeroTolerance)) - Math.Log(d: e), y: 2) / (2.0 * wks.Sigma * wks.Sigma)), key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: kind.GetType(), outputType: typeof(TOut))),
        };
    private static Fin<TOut> DescriptorAtTimes<TOut>(Seq<(double Eigenvalue, Arr<double> Eigenvector)> eigen, Seq<double> scales, Func<double, double, double> project, Op key) {
        if (eigen.IsEmpty || scales.IsEmpty) return Fin.Fail<TOut>(error: key.InvalidInput());
        int n = eigen[index: 0].Eigenvector.Count;
        double[][] result = [.. Enumerable.Range(0, n).Select(_ => new double[scales.Count])];
        for (int s = 0; s < scales.Count; s++)
            for (int i = 0; i < n; i++) {
                double sum = 0.0;
                for (int e = 0; e < eigen.Count; e++) {
                    double phi = eigen[index: e].Eigenvector[index: i];
                    sum += project(arg1: scales[index: s], arg2: eigen[index: e].Eigenvalue) * phi * phi;
                }
                result[i][s] = sum;
            }
        return typeof(TOut) == typeof(Arr<Arr<double>>)
            ? key.AcceptValue(value: new Arr<Arr<double>>(result.Select(r => new Arr<double>(r)))).Map(static v => (TOut)(object)v)
            : Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(MeshDescriptor), outputType: typeof(TOut)));
    }
}

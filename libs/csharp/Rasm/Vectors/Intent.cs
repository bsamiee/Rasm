using DenseMatrixD = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix;
using DenseVectorD = MathNet.Numerics.LinearAlgebra.Double.DenseVector;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SupportProjection {
    public static readonly SupportProjection Closest = Hit(key: 0, accepts: static output => output == typeof(Point3d) || output == typeof(ClosestHit), projectRaw: s => s.Output == typeof(Point3d) ? Accept(state: s, value: s.Hit.Point) : Accept(state: s, value: s.Hit));
    public static readonly SupportProjection Direction = new(key: 1, capability: static (_, _) => true, accepts: static output => output == typeof(Direction) || output == typeof(Vector3d), projectRaw: s => DirectionOf(vector: s.Hit.Point - s.Sample, state: s));
    public static readonly SupportProjection Span = SpanOf(key: 2, sign: 1.0);
    public static readonly SupportProjection SignedSpanAway = SpanOf(key: 13, sign: -1.0);
    public static readonly SupportProjection Normal = new(key: 3, capability: static (space, hit) => space.AdmitsNormal(hit: hit), accepts: DirectionOrHit, projectRaw: s => s.Output == typeof(ClosestHit) ? Accept(state: s, value: s.Hit) : s.Hit.Normal.ToFin(Fail: s.Key.InvalidResult()).Bind(normal => DirectionOf(vector: normal, state: s)));
    public static readonly SupportProjection Distance = Hit(key: 4, accepts: static output => output == typeof(double) || output == typeof(ClosestHit), projectRaw: s => s.Output == typeof(double) ? s.Hit.Distance.ToFin(Fail: s.Key.InvalidResult()).Bind(distance => Accept(state: s, value: distance)) : Accept(state: s, value: s.Hit), capability: static (_, hit) => hit.Distance.IsSome);
    public static readonly SupportProjection Parameter = Hit(key: 5, accepts: static output => output == typeof(double) || output == typeof(ClosestHit), projectRaw: s => s.Output == typeof(double) ? s.Hit.Parameter.ToFin(Fail: s.Key.InvalidResult()).Bind(parameter => Accept(state: s, value: parameter)) : Accept(state: s, value: s.Hit), capability: static (_, hit) => hit.Parameter.IsSome);
    public static readonly SupportProjection Uv = Hit(key: 6, accepts: static output => output == typeof(Point2d) || output == typeof(ClosestHit), projectRaw: s => s.Output == typeof(Point2d) ? s.Hit.Uv.ToFin(Fail: s.Key.InvalidResult()).Bind(uv => Accept(state: s, value: uv)) : Accept(state: s, value: s.Hit), capability: static (_, hit) => hit.Uv.IsSome);
    public static readonly SupportProjection Component = Hit(key: 7, accepts: static output => output == typeof(ComponentIndex) || output == typeof(ClosestHit), projectRaw: s => s.Output == typeof(ComponentIndex) ? s.Hit.Component.ToFin(Fail: s.Key.InvalidResult()).Bind(component => Accept(state: s, value: component)) : Accept(state: s, value: s.Hit), capability: static (_, hit) => hit.Component.IsSome);
    public static readonly SupportProjection MeshPoint = Hit(key: 8, accepts: static output => output == typeof(MeshPoint) || output == typeof(ClosestHit), projectRaw: s => s.Output == typeof(MeshPoint) ? s.Hit.MeshPoint.ToFin(Fail: s.Key.InvalidResult()).Bind(meshPoint => Accept(state: s, value: meshPoint)) : Accept(state: s, value: s.Hit), capability: static (_, hit) => hit.MeshPoint.IsSome);
    public static readonly SupportProjection SignedDistance = new(key: 9, capability: static (space, hit) => space.AdmitsSignedDistance(hit: hit), accepts: static output => output == typeof(double), projectRaw: s => s.Space.SignedDistance(hit: s.Hit, sample: s.Sample, key: s.Key).Bind(distance => Accept(state: s, value: distance)));
    public static readonly SupportProjection ContainmentDistance = new(key: 10, capability: static (space, hit) => space.AdmitsContainmentDistance(hit: hit), accepts: static output => output == typeof(double), projectRaw: s => s.Space.ContainmentDistance(hit: s.Hit, sample: s.Sample, context: s.Context, key: s.Key).Bind(distance => Accept(state: s, value: distance)));
    public static readonly SupportProjection Tangent = new(key: 11, capability: static (space, hit) => space.AdmitsTangent(hit: hit), accepts: DirectionOrHit, projectRaw: s => s.Output == typeof(ClosestHit) ? Accept(state: s, value: s.Hit) : s.Hit.Tangent.ToFin(Fail: s.Key.InvalidResult()).Bind(tangent => DirectionOf(vector: tangent, state: s)));
    public static readonly SupportProjection Frame = Hit(key: 12, accepts: static output => output == typeof(Plane) || output == typeof(ClosestHit), projectRaw: s => s.Output == typeof(Plane) ? s.Hit.Frame.ToFin(Fail: s.Key.InvalidResult()).Bind(frame => Accept(state: s, value: frame)) : Accept(state: s, value: s.Hit), capability: static (space, hit) => space.AdmitsFrame(hit: hit));
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
    private static bool DirectionOrHit(Type output) => output == typeof(Direction) || output == typeof(Vector3d) || output == typeof(ClosestHit);
    private static SupportProjection Hit(int key, Func<Type, bool> accepts, Func<SupportProjectionState, Fin<object>> projectRaw, Func<SupportSpace, ClosestHit, bool>? capability = null) =>
        new(key: key, capability: capability ?? ((_, _) => true), accepts: accepts, projectRaw: projectRaw);
    private static SupportProjection SpanOf(int key, double sign) =>
        new(key: key, capability: static (_, _) => true, accepts: static output => output == typeof(VectorSpan) || output == typeof(Vector3d) || output == typeof(Line) || output == typeof(double),
            projectRaw: state => VectorSpan.Of(anchor: state.Sample, vector: sign * (state.Hit.Point - state.Sample), context: state.Context, key: state.Key)
                .Bind(span => state.Output switch {
                    Type t when t == typeof(VectorSpan) => Accept(state: state, value: span),
                    Type t when t == typeof(Vector3d) => Accept(state: state, value: span.Value),
                    Type t when t == typeof(Line) => Accept(state: state, value: span.Axis),
                    Type t when t == typeof(double) => Accept(state: state, value: span.Magnitude),
                    _ => Fin.Fail<object>(error: state.Key.Unsupported(geometryType: typeof(VectorSpan), outputType: state.Output)),
                }));
    private static Fin<object> DirectionOf(Vector3d vector, SupportProjectionState state) =>
        Vectors.Direction.Of(value: vector, context: state.Context, key: state.Key)
            .Bind(direction => state.Output == typeof(Direction) ? Accept(state: state, value: direction) : Accept(state: state, value: direction.Value));
    private static Fin<object> Accept<T>(SupportProjectionState state, T value) =>
        state.Key.AcceptValue(value: value).Map(static accepted => (object)accepted!);
}

internal readonly record struct SupportProjectionState(SupportSpace Space, ClosestHit Hit, Point3d Sample, Context Context, Op Key, Type Output);

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record VectorIntent {
    private VectorIntent() { }
    public sealed record AxisCase(SignedAxis Value, Option<Plane> Basis) : VectorIntent;
    public sealed record DirectionCase(Vector3d Value) : VectorIntent;
    public sealed record AxesCase(Option<Seq<Vector3d>> Values, bool Planar) : VectorIntent;
    public sealed record AngularCase(Vector3d A, Vector3d B, AnglePivot Pivot) : VectorIntent;
    public sealed record SupportCase(SupportSpace Space, Point3d Query, SupportProjection Projection) : VectorIntent;
    public sealed record VectorFieldCase(VectorField Value, Point3d Query) : VectorIntent;
    public sealed record ScalarFieldCase(ScalarField Value, Point3d Query) : VectorIntent;
    public sealed record RayCase(Point3d Origin, Direction RayDirection, RayPolicy Policy) : VectorIntent;
    public sealed record FrameCase(Point3d Origin, Vector3d Normal, Option<Vector3d> XHint) : VectorIntent;
    public sealed record CurveCase(Curve Source, double Parameter, CurveProjection Mode) : VectorIntent;
    public sealed record CloudCase(VectorCloud Value, VectorCloudMetric Metric) : VectorIntent;
    public sealed record WindingCase(VectorCloud Value, Point3d Query) : VectorIntent;
    public sealed record ConeCase(VectorCone Value, ConeProjection Mode) : VectorIntent;
    public sealed record ComponentsCase(Point3d Anchor, Vector3d Value, Plane Basis) : VectorIntent;
    public sealed record RelationCase(Vector3d A, Vector3d B) : VectorIntent;
    public sealed record BounceCase(Direction Incident, SupportSpace Surface, Point3d Query, BouncePolicy Policy) : VectorIntent;
    public sealed record StreamlineCase(VectorField Source, Point3d Seed, PositiveMagnitude InitialStep, FieldIntegrator Integrator, Termination Termination) : VectorIntent;
    public sealed record LerpCase(Vector3d A, Vector3d B, double Parameter) : VectorIntent;
    public sealed record SlerpCase(Direction A, Direction B, double Parameter) : VectorIntent;
    public sealed record ProjectOntoCase(Vector3d Value, Plane Target) : VectorIntent;
    public sealed record MirrorCase(Vector3d Value, Plane Across) : VectorIntent;
    public sealed record SurfaceCase(SurfaceSpace SurfaceSource, double U, double V, SurfaceProjection Mode) : VectorIntent;
    public sealed record PoseCase(Plane From, Plane To, double Parameter, MotionInterpolation Mode) : VectorIntent;
    public sealed record TensorCase(TensorField Source, Point3d Point) : VectorIntent;
    public sealed record MeshOperatorCase(ScalarField MeshField, Point3d Point) : VectorIntent;
    public sealed record FlattenCase(MeshSpace Space) : VectorIntent;
    public sealed record HullCase(VectorCloud Source) : VectorIntent;
    public sealed record SampleCase(MeshSpace Domain, SamplingKind Kind) : VectorIntent;
    public sealed record RegisterCase(VectorCloud Source, VectorCloud Target, RegistrationKind Kind) : VectorIntent;
    public sealed record RemeshCase(MeshSpace Space, RemeshKind Kind) : VectorIntent;
    public sealed record TransportCase(VectorCloud Source, VectorCloud Target, PositiveMagnitude Regularization, Dimension IterationCap, bool Unbiased) : VectorIntent;
    public sealed record TopologyCase(MeshSpace Space) : VectorIntent;
    public sealed record FeaturesCase(MeshSpace Space, double DihedralRadians) : VectorIntent;
    public sealed record DescriptorCase(MeshSpace Space, MeshDescriptor Kind, Dimension EigenpairCount) : VectorIntent;
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
            from hit in intent.Space.Closest(sample: intent.Query, key: state.Key)
            from output in intent.Projection.Project<TOut>(space: intent.Space, hit: hit, sample: intent.Query, context: state.Context, key: state.Key)
            select output,
        vectorFieldCase: static (state, intent) => intent.Value.Project<TOut>(sample: intent.Query, context: state.Context, key: state.Key),
        scalarFieldCase: static (state, intent) => intent.Value.Project<TOut>(sample: intent.Query, context: state.Context, key: state.Key),
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
            from hit in intent.Surface.Closest(sample: intent.Query, key: state.Key)
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
                        intent.Termination.ShouldStop(state: s, currentSample: vector, context: state.Context, key: state.Key).Bind(stop => stop
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
                            })))))
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
        slerpCase: static (state, intent) =>
            from rotation in intent.A.Value.IsParallelTo(other: intent.B.Value, angleTolerance: state.Context.Angle.Value) switch {
                -1 => Fin.Succ(Quaternion.Rotation(Math.PI, VectorFrame.SeedPerpendicular(axis: intent.A.Value))),
                _ => Transform.Rotation(startDirection: intent.A.Value, endDirection: intent.B.Value, rotationCenter: Point3d.Origin).GetQuaternion(quaternion: out Quaternion target)
                    ? Fin.Succ(target)
                    : Fin.Fail<Quaternion>(state.Key.InvalidResult()),
            }
            from direction in Vectors.Direction.Of(
                value: Quaternion.Slerp(a: Quaternion.Identity, b: rotation, t: intent.Parameter).Rotate(v: intent.A.Value),
                context: state.Context,
                key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        projectOntoCase: static (state, intent) =>
            from direction in Vectors.Direction.Of(
                value: Transform.PlanarProjection(plane: intent.Target) * intent.Value,
                context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        mirrorCase: static (state, intent) =>
            from direction in Vectors.Direction.Of(
                value: Transform.Mirror(mirrorPlane: intent.Across) * intent.Value,
                context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        surfaceCase: static (state, intent) => intent.SurfaceSource.Sample<TOut>(projection: intent.Mode, u: intent.U, v: intent.V, key: state.Key),
        poseCase: static (state, intent) =>
            from pose in intent.Mode.Interpolate(a: intent.From, b: intent.To, t: intent.Parameter).BindFail(_ => Fin.Fail<Plane>(state.Key.InvalidResult()))
            from output in typeof(TOut) == typeof(Plane)
                ? state.Key.AcceptValue(value: pose).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(PoseCase), outputType: typeof(TOut)))
            select output,
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
            from coords in MeshKernel.ParameterizeFlatten(space: intent.Space, key: state.Key)
            from output in typeof(TOut) == typeof(Arr<Point2d>)
                ? state.Key.AcceptValue(value: coords).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(FlattenCase), outputType: typeof(TOut)))
            select output,
        hullCase: static (state, intent) =>
            from mesh in PopulationKernel.ComputeHull(source: intent.Source, context: state.Context, key: state.Key)
            from output in typeof(TOut) switch {
                Type t when t == typeof(Mesh) => state.Key.AcceptValue(value: mesh).Map(static v => (TOut)(object)v),
                Type t when t == typeof(VectorCloud) => VectorCloud.Cluster(
                    points: toSeq(mesh.Vertices.AsIterable().Select(static v => (Point3d)v)),
                    context: state.Context,
                    key: state.Key).Map(static v => (TOut)(object)v),
                _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(HullCase), outputType: typeof(TOut))),
            }
            select output,
        sampleCase: static (state, intent) =>
            from cloud in intent.Kind.Sample(domain: intent.Domain, context: state.Context, key: state.Key)
            from output in typeof(TOut) == typeof(VectorCloud)
                ? state.Key.AcceptValue(value: cloud).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(SampleCase), outputType: typeof(TOut)))
            select output,
        registerCase: static (state, intent) =>
            from transform in intent.Kind.Align(source: intent.Source, target: intent.Target, context: state.Context, key: state.Key)
            from output in typeof(TOut) switch {
                Type t when t == typeof(Transform) => state.Key.AcceptValue(value: transform).Map(static v => (TOut)(object)v),
                _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(RegisterCase), outputType: typeof(TOut))),
            }
            select output,
        remeshCase: static (state, intent) =>
            from mesh in intent.Kind.Apply(space: intent.Space, context: state.Context, key: state.Key)
            from output in typeof(TOut) == typeof(Mesh)
                ? state.Key.AcceptValue(value: mesh).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(RemeshCase), outputType: typeof(TOut)))
            select output,
        transportCase: static (state, intent) => IntentKernel.Sinkhorn<TOut>(source: intent.Source, target: intent.Target, regularization: intent.Regularization.Value, maxIterations: intent.IterationCap.Value, unbiased: intent.Unbiased, key: state.Key),
        topologyCase: static (state, intent) =>
            from topology in intent.Space.Topology(key: state.Key)
            from output in typeof(TOut) == typeof((int Euler, int Genus, int BoundaryComponents))
                ? state.Key.AcceptValue(value: topology).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(TopologyCase), outputType: typeof(TOut)))
            select output,
        featuresCase: static (state, intent) =>
            from edges in intent.Space.FeatureEdges(dihedralRadians: intent.DihedralRadians, key: state.Key)
            from output in typeof(TOut) == typeof(Seq<(int A, int B)>)
                ? state.Key.AcceptValue(value: edges).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(FeaturesCase), outputType: typeof(TOut)))
            select output,
        descriptorCase: static (state, intent) => IntentKernel.DescribeShape<TOut>(space: intent.Space, kind: intent.Kind, eigenpairs: intent.EigenpairCount.Value, key: state.Key));
    public static VectorIntent Between(Point3d origin, SupportSpace target, BoundarySense? sense = null) =>
        new SupportCase(Space: target, Query: origin, Projection: (sense ?? BoundarySense.Toward).Equals(BoundarySense.Toward) ? SupportProjection.Span : SupportProjection.SignedSpanAway);
    public static VectorIntent Axis(SignedAxis axis, Plane? frame = null) =>
        new AxisCase(Value: axis, Basis: Optional(frame));
    public static VectorIntent Direction(Vector3d value) =>
        new DirectionCase(Value: value);
    public static VectorIntent Axes(Option<Seq<Vector3d>> values = default, bool planar = false) =>
        new AxesCase(Values: values, Planar: planar);
    public static VectorIntent Angular(Vector3d a, Vector3d b, AnglePivot? pivot = null) =>
        new AngularCase(A: a, B: b, Pivot: pivot ?? AnglePivot.World);
    public static VectorIntent Support(SupportSpace space, Point3d sample, SupportProjection projection) =>
        new SupportCase(Space: space, Query: sample, Projection: projection);
    public static VectorIntent Field(VectorField field, Point3d sample) =>
        new VectorFieldCase(Value: field, Query: sample);
    public static VectorIntent Scalar(ScalarField field, Point3d sample) =>
        new ScalarFieldCase(Value: field, Query: sample);
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
        new BounceCase(Incident: incident, Surface: surface, Query: sample, Policy: policy ?? BouncePolicy.Reflect);
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
        new PoseCase(From: from, To: to, Parameter: Math.Clamp(value: t, min: 0.0, max: 1.0), Mode: mode);
    public static VectorIntent Tensor(TensorField source, Point3d point) =>
        new TensorCase(Source: source, Point: point);
    public static VectorIntent MeshOperator(ScalarField meshField, Point3d point) =>
        new MeshOperatorCase(MeshField: meshField, Point: point);
    public static VectorIntent Flatten(MeshSpace space) =>
        new FlattenCase(Space: space);
    public static VectorIntent Hull(VectorCloud source) =>
        new HullCase(Source: source);
    public static VectorIntent Sample(MeshSpace domain, SamplingKind kind) =>
        new SampleCase(Domain: domain, Kind: kind);
    public static VectorIntent Register(VectorCloud source, VectorCloud target, RegistrationKind kind) =>
        new RegisterCase(Source: source, Target: target, Kind: kind);
    public static VectorIntent Remesh(MeshSpace space, RemeshKind kind) =>
        new RemeshCase(Space: space, Kind: kind);
    public static Fin<VectorIntent> Transport(VectorCloud source, VectorCloud target, double regularization, int iterationCap, bool unbiased = false, Op? key = null) {
        Op op = key.OrDefault();
        return from reg in op.AcceptValidated<PositiveMagnitude>(candidate: regularization)
               from cap in op.AcceptValidated<Dimension>(candidate: iterationCap)
               select (VectorIntent)new TransportCase(Source: source, Target: target, Regularization: reg, IterationCap: cap, Unbiased: unbiased);
    }
    public static VectorIntent Topology(MeshSpace space) =>
        new TopologyCase(Space: space);
    public static Fin<VectorIntent> Features(MeshSpace space, double dihedralRadians, Op? key = null) =>
        RhinoMath.IsValidDouble(x: dihedralRadians) && dihedralRadians > 0.0
            ? Fin.Succ((VectorIntent)new FeaturesCase(Space: space, DihedralRadians: dihedralRadians))
            : Fin.Fail<VectorIntent>(key.OrDefault().InvalidInput());
    public static Fin<VectorIntent> Descriptor(MeshSpace space, MeshDescriptor kind, int eigenpairCount, Op? key = null) {
        Op op = key.OrDefault();
        return from count in op.AcceptValidated<Dimension>(candidate: eigenpairCount)
               select (VectorIntent)new DescriptorCase(Space: space, Kind: kind, EigenpairCount: count);
    }
    private const int MaxIterations = 100000;
}

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class IntentKernel {
    internal static Fin<TOut> Sinkhorn<TOut>(VectorCloud source, VectorCloud target, double regularization, int maxIterations, bool unbiased, Op key) =>
        (source, target) switch {
            (VectorCloud.ClusterCase src, VectorCloud.ClusterCase tgt) => SinkhornCluster<TOut>(source: src, target: tgt, regularization: regularization, maxIterations: maxIterations, unbiased: unbiased, key: key),
            _ => Fin.Fail<TOut>(key.Unsupported(geometryType: source.GetType(), outputType: typeof(TOut))),
        };
    private static Fin<TOut> SinkhornCluster<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, double regularization, int maxIterations, bool unbiased, Op key) {
        return source.Vertices.Count < 1 || target.Vertices.Count < 1
            ? Fin.Fail<TOut>(error: key.InvalidInput())
            : (from plan in SinkhornOt(source: source.Vertices, target: target.Vertices, reg: regularization, maxIter: maxIterations, key: key)
               from distance in unbiased
                    ? from sourceBias in SinkhornOt(source: source.Vertices, target: source.Vertices, reg: regularization, maxIter: maxIterations, key: key)
                      from targetBias in SinkhornOt(source: target.Vertices, target: target.Vertices, reg: regularization, maxIter: maxIterations, key: key)
                      select plan.Distance - (0.5 * sourceBias.Distance) - (0.5 * targetBias.Distance)
                    : key.AcceptValue(value: plan.Distance)
               from output in ProjectSinkhorn<TOut>(source: source, target: target, plan: plan, distance: distance, key: key)
               select output);
    }
    private static Fin<TOut> ProjectSinkhorn<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, SinkhornPlan plan, double distance, Op key) =>
        typeof(TOut) switch {
            Type t when t == typeof(double) => key.AcceptValue(value: distance).Map(static d => (TOut)(object)d),
            Type t when t == typeof(Matrix) => ProjectCoupling<TOut>(plan: plan, key: key),
            Type t when t == typeof(VectorCloud) => ProjectTransportedCloud<TOut>(source: source, target: target, plan: plan, key: key),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(VectorIntent.TransportCase), outputType: typeof(TOut))),
        };
    private sealed record SinkhornPlan(double Distance, DenseMatrixD Coupling, double SourceResidual, double TargetResidual, int Iterations, bool IsNumeric);
    private static Fin<SinkhornPlan> SinkhornOt(Seq<Point3d> source, Seq<Point3d> target, double reg, int maxIter, Op key) {
        int m = source.Count; int n = target.Count;
        DenseMatrixD cost = DenseMatrixD.Create(rows: m, columns: n, value: 0.0);
        DenseMatrixD kernel = DenseMatrixD.Create(rows: m, columns: n, value: 0.0);
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++) {
                double d2 = source[index: i].DistanceToSquared(other: target[index: j]);
                cost[i, j] = d2;
                kernel[i, j] = Math.Exp(d: -d2 / reg);
            }
        DenseVectorD u = DenseVectorD.Create(m, 1.0);
        DenseVectorD v = DenseVectorD.Create(n, 1.0);
        double aMass = 1.0 / m; double bMass = 1.0 / n;
        double sourceResidual = double.PositiveInfinity;
        double targetResidual = double.PositiveInfinity;
        int iterations = 0;
        for (int iter = 0; iter < maxIter; iter++) {
            iterations = iter + 1;
            for (int i = 0; i < m; i++) {
                double sum = 0.0;
                for (int j = 0; j < n; j++) sum += kernel[i, j] * v[j];
                u[i] = sum > RhinoMath.ZeroTolerance ? aMass / sum : aMass;
            }
            for (int j = 0; j < n; j++) {
                double sum = 0.0;
                for (int i = 0; i < m; i++) sum += kernel[i, j] * u[i];
                v[j] = sum > RhinoMath.ZeroTolerance ? bMass / sum : bMass;
            }
            (sourceResidual, targetResidual) = SinkhornResiduals(kernel: kernel, u: u, v: v, aMass: aMass, bMass: bMass);
            if (Math.Max(val1: sourceResidual, val2: targetResidual) <= RhinoMath.SqrtEpsilon) break;
        }
        double dist = 0.0;
        DenseMatrixD coupling = DenseMatrixD.Create(rows: m, columns: n, value: 0.0);
        for (int i = 0; i < m; i++)
            for (int j = 0; j < n; j++) {
                coupling[i, j] = u[i] * kernel[i, j] * v[j];
                dist += coupling[i, j] * cost[i, j];
            }
        bool numeric = RhinoMath.IsValidDouble(x: dist) && coupling.Enumerate().All(RhinoMath.IsValidDouble);
        return numeric && Math.Max(val1: sourceResidual, val2: targetResidual) <= RhinoMath.SqrtEpsilon
            ? Fin.Succ(new SinkhornPlan(Distance: dist, Coupling: coupling, SourceResidual: sourceResidual, TargetResidual: targetResidual, Iterations: iterations, IsNumeric: numeric))
            : Fin.Fail<SinkhornPlan>(key.InvalidResult());
    }
    private static (double Source, double Target) SinkhornResiduals(DenseMatrixD kernel, DenseVectorD u, DenseVectorD v, double aMass, double bMass) {
        double sourceResidual = 0.0;
        double targetResidual = 0.0;
        for (int i = 0; i < kernel.RowCount; i++) {
            double row = 0.0;
            for (int j = 0; j < kernel.ColumnCount; j++) row += u[i] * kernel[i, j] * v[j];
            sourceResidual = Math.Max(val1: sourceResidual, val2: Math.Abs(value: row - aMass));
        }
        for (int j = 0; j < kernel.ColumnCount; j++) {
            double col = 0.0;
            for (int i = 0; i < kernel.RowCount; i++) col += u[i] * kernel[i, j] * v[j];
            targetResidual = Math.Max(val1: targetResidual, val2: Math.Abs(value: col - bMass));
        }
        return (Source: sourceResidual, Target: targetResidual);
    }
    private static Fin<TOut> ProjectCoupling<TOut>(SinkhornPlan plan, Op key) {
        Dimension rows = Dimension.Create(value: plan.Coupling.RowCount);
        Dimension cols = Dimension.Create(value: plan.Coupling.ColumnCount);
        return key.AcceptValue(value: MatrixKernel.FromMathNet(m: plan.Coupling, rows: rows, cols: cols))
            .Map(static matrix => (TOut)(object)matrix);
    }
    private static Fin<TOut> ProjectTransportedCloud<TOut>(VectorCloud.ClusterCase source, VectorCloud.ClusterCase target, SinkhornPlan plan, Op key) {
        Point3d[] transported = new Point3d[source.Vertices.Count];
        for (int i = 0; i < source.Vertices.Count; i++) {
            double mass = plan.Coupling.Row(i).Sum();
            Vector3d sum = Vector3d.Zero;
            for (int j = 0; j < target.Vertices.Count; j++) sum += plan.Coupling[i, j] * (Vector3d)target.Vertices[index: j];
            transported[i] = mass > RhinoMath.ZeroTolerance ? Point3d.Origin + (sum / mass) : source.Vertices[index: i];
        }
        return VectorCloud.Cluster(points: toSeq(transported), context: source.Tolerance, key: key)
            .Map(static cloud => (TOut)(object)cloud);
    }
    // Sun-Ovsjanikov-Guibas 2009 HKS(x, t) = Σ exp(-t λ_i) φ_i²(x); WKS analogous with log-energy.
    internal static Fin<TOut> DescribeShape<TOut>(MeshSpace space, MeshDescriptor kind, int eigenpairs, Op key) =>
        from laplacian in space.Laplacian(kind: MeshLaplacian.IntrinsicDelaunay, key: key)
        from eigen in MatrixKernel.GeneralizedEigenpairs(
            stiffness: laplacian.Stiffness,
            mass: laplacian.MassConsistent,
            k: eigenpairs,
            key: key)
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

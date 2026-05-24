namespace Rasm.Vectors;

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record VectorIntent {
    private VectorIntent() { }
    public sealed record AxisCase(SignedAxis Value, Option<Plane> Basis) : VectorIntent;
    public sealed record DirectionCase(Vector3d Value) : VectorIntent;
    public sealed record AxesCase(Option<Seq<Vector3d>> Values, bool Planar) : VectorIntent;
    public sealed record AngularCase(Vector3d A, Vector3d B, AnglePivot Pivot) : VectorIntent;
    public sealed record SupportCase(SupportSpace Space, Point3d Query, SupportProjection Projection) : VectorIntent;
    public sealed record ProbeCase(ExtractionProbe Source, Point3d Query) : VectorIntent;
    public sealed record IsoSurfaceCase(ScalarField Source, BoundingBox Bounds, int Resolution, int MaxRootSteps) : VectorIntent;
    public sealed record ContourCase(ExtractionDomain Domain, ContourPolicy Policy) : VectorIntent;
    public sealed record GlyphCase(VectorField Source, ExtractionDomain Domain, GlyphPolicy Policy) : VectorIntent;
    public sealed record SampleGridCase(ScalarField Source, ExtractionDomain Domain, GridPolicy Policy) : VectorIntent;
    public sealed record StreamBundleCase(VectorField Source, ExtractionDomain Domain, StreamBundlePolicy Policy) : VectorIntent;
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
    public sealed record LerpCase(Vector3d A, Vector3d B, UnitInterval Parameter) : VectorIntent;
    public sealed record SlerpCase(Direction A, Direction B, UnitInterval Parameter) : VectorIntent;
    public sealed record ProjectOntoCase(Vector3d Value, Plane Target) : VectorIntent;
    public sealed record MirrorCase(Vector3d Value, Plane Across) : VectorIntent;
    public sealed record SurfaceCase(SurfaceSpace SurfaceSource, double U, double V, SurfaceProjection Mode) : VectorIntent;
    public sealed record PoseCase(Plane From, Plane To, UnitInterval Parameter, MotionInterpolation Mode) : VectorIntent;
    public sealed record FlattenCase(MeshSpace Space) : VectorIntent;
    public sealed record HullCase(VectorCloud Source) : VectorIntent;
    public sealed record SampleCase(MeshSpace Domain, SampleKind Kind) : VectorIntent;
    public sealed record AlignCase(VectorCloud Source, VectorCloud Target, AlignKind Kind) : VectorIntent;
    public sealed record RemeshCase(MeshSpace Space, RemeshKind Kind) : VectorIntent;
    public sealed record TransportCase(VectorCloud Source, VectorCloud Target, PositiveMagnitude Regularization, Dimension MaxIterations, bool Debiased, Option<PositiveMagnitude> MassRelaxation) : VectorIntent;
    public sealed record TopologyCase(MeshSpace Space) : VectorIntent;
    public sealed record FeaturesCase(MeshSpace Space, VectorAngle Dihedral) : VectorIntent;
    public sealed record DescriptorCase(MeshSpace Space, MeshDescriptor Kind, Dimension Pairs) : VectorIntent;
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
        probeCase: static (state, intent) => Extraction.Probe(source: intent.Source, sample: intent.Query).Project<TOut>(context: state.Context, key: state.Key),
        isoSurfaceCase: static (state, intent) => Extraction.IsoSurface(field: intent.Source, bounds: intent.Bounds, resolution: intent.Resolution, maxRootSteps: intent.MaxRootSteps).Project<TOut>(context: state.Context, key: state.Key),
        contourCase: static (state, intent) => Extraction.Contour(domain: intent.Domain, policy: intent.Policy).Project<TOut>(context: state.Context, key: state.Key),
        glyphCase: static (state, intent) => Extraction.Glyph(field: intent.Source, domain: intent.Domain, policy: intent.Policy).Project<TOut>(context: state.Context, key: state.Key),
        sampleGridCase: static (state, intent) => Extraction.SampleGrid(field: intent.Source, domain: intent.Domain, policy: intent.Policy).Project<TOut>(context: state.Context, key: state.Key),
        streamBundleCase: static (state, intent) => Extraction.StreamBundle(field: intent.Source, domain: intent.Domain, policy: intent.Policy).Project<TOut>(context: state.Context, key: state.Key),
        rayCase: static (state, intent) => intent.Policy.Project<TOut>(origin: intent.Origin, direction: intent.RayDirection, context: state.Context, key: state.Key),
        frameCase: static (state, intent) =>
            from frame in VectorFrame.Of(origin: intent.Origin, normal: intent.Normal, xHint: intent.XHint, context: state.Context, key: state.Key)
            from output in frame.Project<TOut>(key: state.Key)
            select output,
        curveCase: static (state, intent) => intent.Mode.Project<TOut>(curve: intent.Source, parameter: intent.Parameter, context: state.Context, key: state.Key),
        cloudCase: static (state, intent) => intent.Metric.Project<TOut>(cloud: intent.Value, key: state.Key),
        windingCase: static (state, intent) => CloudKernel.Winding<TOut>(cloud: intent.Value, query: intent.Query, key: state.Key),
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
        streamlineCase: static (state, intent) => FlowKernel.Trace<TOut>(source: intent.Source, seed: intent.Seed, initialStep: intent.InitialStep, integrator: intent.Integrator, termination: intent.Termination, context: state.Context, key: state.Key),
        lerpCase: static (state, intent) =>
            from direction in Vectors.Direction.Of(
                value: ((1.0 - intent.Parameter.Value) * intent.A) + (intent.Parameter.Value * intent.B),
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
                value: Quaternion.Slerp(a: Quaternion.Identity, b: rotation, t: intent.Parameter.Value).Rotate(v: intent.A.Value),
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
        flattenCase: static (state, intent) =>
            from coords in MeshKernel.ParameterizeFlatten(space: intent.Space, key: state.Key)
            from output in typeof(TOut) == typeof(Arr<Point2d>)
                ? state.Key.AcceptValue(value: coords).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(FlattenCase), outputType: typeof(TOut)))
            select output,
        hullCase: static (state, intent) =>
            from mesh in CloudKernel.ComputeHull(source: intent.Source, context: state.Context, key: state.Key)
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
            from output in typeof(TOut) switch {
                Type t when t == typeof(VectorCloud) =>
                    intent.Kind.Sample(domain: intent.Domain, context: state.Context, key: state.Key).Map(static v => (TOut)(object)v),
                Type t when t == typeof(SampleReceipt) =>
                    intent.Kind.SampleReceipt(domain: intent.Domain, context: state.Context, key: state.Key).Map(static v => (TOut)(object)v),
                _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(SampleCase), outputType: typeof(TOut))),
            }
            select output,
        alignCase: static (state, intent) =>
            from output in typeof(TOut) switch {
                Type t when t == typeof(Transform) || t == typeof(AlignmentReceipt) =>
                    from receipt in intent.Kind.AlignDetailed(source: intent.Source, target: intent.Target, key: state.Key)
                    from projected in typeof(TOut) switch {
                        Type tt when tt == typeof(Transform) && receipt.Stop.Equals(AlignmentStopKind.Converged) =>
                            state.Key.AcceptValue(value: receipt.Transform).Map(static v => (TOut)(object)v),
                        Type tt when tt == typeof(Transform) => Fin.Fail<TOut>(error: state.Key.InvalidResult()),
                        _ => state.Key.AcceptValue(value: receipt).Map(static v => (TOut)(object)v),
                    }
                    select projected,
                _ => Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(AlignCase), outputType: typeof(TOut))),
            }
            select output,
        remeshCase: static (state, intent) =>
            from mesh in intent.Kind.Apply(space: intent.Space, key: state.Key)
            from output in typeof(TOut) == typeof(Mesh)
                ? state.Key.AcceptValue(value: mesh).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(RemeshCase), outputType: typeof(TOut)))
            select output,
        transportCase: static (state, intent) => CloudKernel.Sinkhorn<TOut>(source: intent.Source, target: intent.Target, regularization: intent.Regularization.Value, maxIterations: intent.MaxIterations.Value, debiased: intent.Debiased, massRelaxation: intent.MassRelaxation, key: state.Key),
        topologyCase: static (state, intent) =>
            from topology in MeshKernel.TopologyOf(space: intent.Space, key: state.Key)
            from output in typeof(TOut) == typeof((int Euler, int Genus, int BoundaryComponents))
                ? state.Key.AcceptValue(value: topology).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(TopologyCase), outputType: typeof(TOut)))
            select output,
        featuresCase: static (state, intent) =>
            from edges in MeshKernel.DetectFeatureEdgesOf(space: intent.Space, dihedralRadians: intent.Dihedral.Value, key: state.Key)
            from output in typeof(TOut) == typeof(Seq<(int A, int B)>)
                ? state.Key.AcceptValue(value: edges).Map(static v => (TOut)(object)v)
                : Fin.Fail<TOut>(error: state.Key.Unsupported(geometryType: typeof(FeaturesCase), outputType: typeof(TOut)))
            select output,
        descriptorCase: static (state, intent) => MeshKernel.DescribeShape<TOut>(space: intent.Space, kind: intent.Kind, eigenpairs: intent.Pairs.Value, key: state.Key));
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
        new ProbeCase(Source: ExtractionProbe.Vector(source: field), Query: sample);
    public static VectorIntent Scalar(ScalarField field, Point3d sample) =>
        new ProbeCase(Source: ExtractionProbe.Scalar(source: field), Query: sample);
    public static VectorIntent Probe(ExtractionProbe source, Point3d sample) =>
        new ProbeCase(Source: source, Query: sample);
    public static Fin<VectorIntent> IsoSurface(ScalarField field, BoundingBox bounds, int resolution, int maxRootSteps, Op? key = null) {
        Op op = key.OrDefault();
        return bounds is { IsValid: true, Diagonal: Vector3d d }
            && d.X > RhinoMath.ZeroTolerance
            && d.Y > RhinoMath.ZeroTolerance
            && d.Z > RhinoMath.ZeroTolerance
            && resolution >= 2
            && maxRootSteps >= 1
            ? Fin.Succ<VectorIntent>(new IsoSurfaceCase(Source: field, Bounds: bounds, Resolution: resolution, MaxRootSteps: maxRootSteps))
            : Fin.Fail<VectorIntent>(op.InvalidInput());
    }
    public static VectorIntent Contour(ExtractionDomain domain, ContourPolicy policy) =>
        new ContourCase(Domain: domain, Policy: policy);
    public static VectorIntent Glyph(VectorField field, ExtractionDomain domain, GlyphPolicy policy) =>
        new GlyphCase(Source: field, Domain: domain, Policy: policy);
    public static VectorIntent SampleGrid(ScalarField field, ExtractionDomain domain, GridPolicy policy) =>
        new SampleGridCase(Source: field, Domain: domain, Policy: policy);
    public static VectorIntent StreamBundle(VectorField field, ExtractionDomain domain, StreamBundlePolicy policy) =>
        new StreamBundleCase(Source: field, Domain: domain, Policy: policy);
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
    public static Fin<VectorIntent> Lerp(Vector3d a, Vector3d b, double t, Op? key = null) =>
        key.OrDefault().AcceptValidated<UnitInterval>(candidate: t)
            .Map(unit => (VectorIntent)new LerpCase(A: a, B: b, Parameter: unit));
    public static Fin<VectorIntent> Slerp(Direction a, Direction b, double t, Op? key = null) =>
        key.OrDefault().AcceptValidated<UnitInterval>(candidate: t)
            .Map(unit => (VectorIntent)new SlerpCase(A: a, B: b, Parameter: unit));
    public static VectorIntent ProjectOnto(Vector3d value, Plane target) =>
        new ProjectOntoCase(Value: value, Target: target);
    public static VectorIntent Mirror(Vector3d value, Plane across) =>
        new MirrorCase(Value: value, Across: across);
    public static VectorIntent OnSurface(SurfaceSpace space, double u, double v, SurfaceProjection mode) =>
        new SurfaceCase(SurfaceSource: space, U: u, V: v, Mode: mode);
    public static Fin<VectorIntent> Pose(Plane from, Plane to, double t, MotionInterpolation mode, Op? key = null) =>
        key.OrDefault().AcceptValidated<UnitInterval>(candidate: t)
            .Map(unit => (VectorIntent)new PoseCase(From: from, To: to, Parameter: unit, Mode: mode));
    public static VectorIntent Tensor(TensorField source, Point3d point) =>
        new ProbeCase(Source: ExtractionProbe.Tensor(source: source), Query: point);
    public static VectorIntent MeshOperator(ScalarField meshField, Point3d point) =>
        new ProbeCase(Source: ExtractionProbe.MeshScalar(source: meshField), Query: point);
    public static VectorIntent Flatten(MeshSpace space) =>
        new FlattenCase(Space: space);
    public static VectorIntent Hull(VectorCloud source) =>
        new HullCase(Source: source);
    public static VectorIntent Sample(MeshSpace domain, SampleKind kind) =>
        new SampleCase(Domain: domain, Kind: kind);
    public static VectorIntent Align(VectorCloud source, VectorCloud target, AlignKind kind) =>
        new AlignCase(Source: source, Target: target, Kind: kind);
    public static VectorIntent Remesh(MeshSpace space, RemeshKind kind) =>
        new RemeshCase(Space: space, Kind: kind);
    // massRelaxation changes the KL marginal penalty over normalized uniform masses.
    public static Fin<VectorIntent> Transport(VectorCloud source, VectorCloud target, double regularization, int maxIterations, bool debiased = false, double? massRelaxation = null, Op? key = null) {
        Op op = key.OrDefault();
        return from reg in op.AcceptValidated<PositiveMagnitude>(candidate: regularization)
               from cap in op.AcceptValidated<Dimension>(candidate: maxIterations)
               from relax in massRelaxation is double lambda
                    ? op.AcceptValidated<PositiveMagnitude>(candidate: lambda).Map(Some)
                    : Fin.Succ(Option<PositiveMagnitude>.None)
               select (VectorIntent)new TransportCase(Source: source, Target: target, Regularization: reg, MaxIterations: cap, Debiased: debiased, MassRelaxation: relax);
    }
    public static VectorIntent Topology(MeshSpace space) =>
        new TopologyCase(Space: space);
    public static Fin<VectorIntent> Features(MeshSpace space, double dihedralRadians, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in Optional(space.Native).ToFin(op.InvalidInput())
               from angle in op.AcceptValidated<VectorAngle>(candidate: dihedralRadians)
               from intent in angle.Value > RhinoMath.ZeroTolerance
                ? Fin.Succ((VectorIntent)new FeaturesCase(Space: space, Dihedral: angle))
                : Fin.Fail<VectorIntent>(op.InvalidInput())
               select intent;
    }
    public static Fin<VectorIntent> Descriptor(MeshSpace space, MeshDescriptor kind, int pairs, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in Optional(space.Native).ToFin(op.InvalidInput())
               from active in Optional(kind).ToFin(op.InvalidInput())
               from __ in guard(active.IsValid, op.InvalidInput())
               from count in op.AcceptValidated<Dimension>(candidate: pairs)
               select (VectorIntent)new DescriptorCase(Space: space, Kind: active, Pairs: count);
    }
}

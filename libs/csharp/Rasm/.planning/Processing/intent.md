# [RASM_VECTORS_INTENT]

`VectorIntent` owns the kernel consumer rail: one `[Union]` spanning every kernel capability band, reached through one `Project<TOut>(Context, Op?)` egress. `Rasm.Rhino` Camera and the settled corpus bind that exact egress signature, so the entry contract holds as a frozen wire name while every owner behind it re-derives freely.

Every case admits through exactly one factory that internalizes `Domain/validation.md` and each payload owner's `Admit`, so dispatch never re-validates an admitted intent. Dispatch composes, never computes: every arm routes to the owning page's entry and projects the result through the `Numerics/atoms.md` `AtomProjection` rail, `Op` threaded as the explicit value key.

## [01]-[INDEX]

- [02]-[CONSTRUCTION]: `VectorIntent` mints one factory per case, admission internalized at each payload's owner.
- [03]-[DISPATCH]: `Project<TOut>` folds every case through the generated total `Switch`, composing owners never computing.

## [02]-[CONSTRUCTION]

- Owner: `VectorIntent` `[Union]` with a private root constructor and no implicit conversions; a case constructor is `internal` where its payload must arrive pre-admitted, positional-public only where every payload is a raw value or an admitted-by-construction carrier the dispatch admits through its owner.
- Entry: exactly one factory per case. Raw scalars admit through `Op.AcceptValidated`, geometry through the `Admit` vocabulary, and a payload carrying an owner re-admits through that owner; an admitted-by-construction payload gates only its presence. Optional policies enter as `Option<T> = default` resolved against the owner's canonical row, never a `bool` knob or sibling overload.
- Growth: a new kernel capability is one case, one factory, and one dispatch arm, so the generated `Switch` breaks every dispatch site at compile time; a new modality of an existing capability is a policy row or case field on the owning page, reaching this rail with zero new surface.
- Boundary: `VectorIntent`'s factory surface is the only construction path, so no un-admitted intent exists; a factory carries a solver decision as the payload's own vocabulary, never interpreting it. View composition homes at `Rasm.Drawing`'s `ViewConvention` catalog, never a geometry-rail case.

## [03]-[DISPATCH]

- Entry: `Project<TOut>(Context, Op?)` is frozen; a context gate rejects a null `Context` before the total `Switch`, and `TOut` — the output discriminant each owner's projection rows resolve — lets one entry serve every typed evidence carrier the owners publish.
- Auto: every arm delegates to its owning page's entry and gates output through the `AtomProjection` rail; the `Switch` body carries each arm's owner target.
- Receipt: this rail mints no receipt of its own; every arm surfaces the owner's typed receipt through the owner's projection rows, so evidence provenance is single-sourced.
- Boundary: dispatch carries zero domain math; `Project<TOut>` is total over the `Fin` rail, an unsupported `TOut` returns the owner's typed `Unsupported` fault naming both the case and the requested type, and the generated `Switch` with no `_` arm is the exhaustiveness proof a new case cannot silently escape.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using LanguageExt;
using Rasm.Domain;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Spatial;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;
// CS0104 guard: Rhino.Geometry declares Matrix/Dimension homonyms under the dual usings.
using Dimension = Rasm.Numerics.Dimension;

namespace Rasm.Processing;

// --- [TYPES] ----------------------------------------------------------------------------------
[Union]
public abstract partial record VectorIntent {
    public sealed record AxisCase(SignedAxis Value, Option<Plane> Basis) : VectorIntent;
    public sealed record DirectionCase(Vector3d Value) : VectorIntent;
    public sealed record AxesCase(Option<Seq<Vector3d>> Values, bool Planar) : VectorIntent;
    public sealed record AngularCase(Vector3d A, Vector3d B, AnglePivot Pivot) : VectorIntent;
    public sealed record SupportCase : VectorIntent { internal SupportCase(SupportSpace space, Point3d query, SupportProjection projection) { Space = space; Query = query; Projection = projection; } public SupportSpace Space { get; } public Point3d Query { get; } public SupportProjection Projection { get; } }
    public sealed record ExtractionCase : VectorIntent { internal ExtractionCase(Extraction value) => Value = value; public Extraction Value { get; } }
    public sealed record RayCase(Point3d Origin, Direction RayDirection, RayPolicy Policy) : VectorIntent;
    public sealed record FrameCase(Point3d Origin, Vector3d Normal, Option<Vector3d> XHint) : VectorIntent;
    public sealed record CurveCase : VectorIntent { internal CurveCase(Curve source, double parameter, CurveProjection mode) { Source = source; Parameter = parameter; Mode = mode; } public Curve Source { get; } public double Parameter { get; } public CurveProjection Mode { get; } }
    public sealed record CloudCase : VectorIntent { internal CloudCase(VectorCloud value, VectorCloudMetric metric, CloudMetricPolicy policy) { Value = value; Metric = metric; Policy = policy; } public VectorCloud Value { get; } public VectorCloudMetric Metric { get; } public CloudMetricPolicy Policy { get; } }
    public sealed record WindingCase : VectorIntent { internal WindingCase(VectorCloud.RingCase value, Point3d query) { Value = value; Query = query; } public VectorCloud.RingCase Value { get; } public Point3d Query { get; } }
    public sealed record ConeCase(VectorCone Value, ConeProjection Mode) : VectorIntent;
    public sealed record ComponentsCase(Point3d Anchor, Vector3d Value, Plane Basis) : VectorIntent;
    public sealed record RelationCase(Vector3d A, Vector3d B) : VectorIntent;
    public sealed record BounceCase(Direction Incident, SupportSpace Target, Point3d Query, BouncePolicy Policy) : VectorIntent;
    public sealed record StreamlineCase : VectorIntent { internal StreamlineCase(VectorField source, Point3d seed, PositiveMagnitude initialStep, FieldIntegrator integrator, Termination termination) { Source = source; Seed = seed; InitialStep = initialStep; Integrator = integrator; Termination = termination; } public VectorField Source { get; } public Point3d Seed { get; } public PositiveMagnitude InitialStep { get; } public FieldIntegrator Integrator { get; } public Termination Termination { get; } }
    public sealed record LerpCase(Vector3d A, Vector3d B, UnitInterval Parameter) : VectorIntent;
    public sealed record SlerpCase(Direction A, Direction B, UnitInterval Parameter) : VectorIntent;
    public sealed record ProjectOntoCase(Vector3d Value, Plane Target) : VectorIntent;
    public sealed record MirrorCase(Vector3d Value, Plane Across) : VectorIntent;
    public sealed record SurfaceCase : VectorIntent { internal SurfaceCase(SurfaceSpace source, Point2d uv, SurfaceProjection mode) { Source = source; Uv = uv; Mode = mode; } public SurfaceSpace Source { get; } public Point2d Uv { get; } public SurfaceProjection Mode { get; } }
    public sealed record PoseCase(Plane From, Plane To, UnitInterval Parameter, MotionInterpolation Mode) : VectorIntent;
    public sealed record FlattenCase : VectorIntent { internal FlattenCase(MeshSpace space) => Space = space; public MeshSpace Space { get; } }
    public sealed record HullCase : VectorIntent { internal HullCase(VectorCloud source, CloudHullKind kind, CloudHullPolicy policy) { Source = source; Kind = kind; Policy = policy; } public VectorCloud Source { get; } public CloudHullKind Kind { get; } public CloudHullPolicy Policy { get; } }
    public sealed record SampleCase : VectorIntent { internal SampleCase(ExtractionDomain domain, SampleKind kind) { Domain = domain; Kind = kind; } public ExtractionDomain Domain { get; } public SampleKind Kind { get; } }
    public sealed record AlignCase : VectorIntent { internal AlignCase(VectorCloud source, VectorCloud target, AlignKind kind, AlignmentPolicy policy) { Source = source; Target = target; Kind = kind; Policy = policy; } public VectorCloud Source { get; } public VectorCloud Target { get; } public AlignKind Kind { get; } public AlignmentPolicy Policy { get; } }
    public sealed record RemeshCase : VectorIntent { internal RemeshCase(MeshSpace space, RemeshKind kind) { Space = space; Kind = kind; } public MeshSpace Space { get; } public RemeshKind Kind { get; } }
    public sealed record TransportCase : VectorIntent { internal TransportCase(VectorCloud source, VectorCloud target, CloudTransportPolicy policy) { Source = source; Target = target; Policy = policy; } public VectorCloud Source { get; } public VectorCloud Target { get; } public CloudTransportPolicy Policy { get; } }
    public sealed record TopologyCase : VectorIntent { internal TopologyCase(MeshSpace space) => Space = space; public MeshSpace Space { get; } }
    public sealed record FeaturesCase : VectorIntent { internal FeaturesCase(MeshSpace space, MeshFeaturePolicy policy) { Space = space; Policy = policy; } public MeshSpace Space { get; } public MeshFeaturePolicy Policy { get; } }
    public sealed record DescriptorCase : VectorIntent { internal DescriptorCase(MeshSpace space, MeshDescriptor kind, Dimension pairs) { Space = space; Kind = kind; Pairs = pairs; } public MeshSpace Space { get; } public MeshDescriptor Kind { get; } public Dimension Pairs { get; } }
    public sealed record DiscreteCalculusCase : VectorIntent { internal DiscreteCalculusCase(MeshSpace space, MeshLaplacian kind) { Space = space; Kind = kind; } public MeshSpace Space { get; } public MeshLaplacian Kind { get; } }
    public sealed record SegmentationCase : VectorIntent { internal SegmentationCase(MeshSpace space, MeshSegmentation kind) { Space = space; Kind = kind; } public MeshSpace Space { get; } public MeshSegmentation Kind { get; } }
    private VectorIntent() { }

    // --- [CONSTRUCTION]
    public static Fin<VectorIntent> Axis(SignedAxis axis, Plane? frame = null, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Admit.NotNull(value: axis, key: op)
               from basis in frame is null ? Fin.Succ(Option<Plane>.None) : Admit.Plane(basis: frame.Value, key: op).Map(static plane => Some(plane))
               select (VectorIntent)new AxisCase(Value: active, Basis: basis);
    }
    public static VectorIntent Direction(Vector3d value) => new DirectionCase(Value: value);
    public static VectorIntent Axes(Option<Seq<Vector3d>> values = default, bool planar = false) => new AxesCase(Values: values, Planar: planar);
    public static VectorIntent Angular(Vector3d a, Vector3d b, AnglePivot? pivot = null) => new AngularCase(A: a, B: b, Pivot: pivot ?? AnglePivot.World);
    public static Fin<VectorIntent> Support(SupportSpace space, Point3d sample, SupportProjection projection, Op? key = null) {
        Op op = key.OrDefault();
        return from validSpace in Admit.NotNull(value: space, key: op)
               from validProjection in Admit.NotNull(value: projection, key: op)
               from validSample in op.AcceptValue(value: sample)
               select (VectorIntent)new SupportCase(space: validSpace, query: validSample, projection: validProjection);
    }
    public static Fin<VectorIntent> Extract(Extraction request, Op? key = null) =>
        Admit.NotNull(value: request, key: key.OrDefault()).Map(static value => (VectorIntent)new ExtractionCase(value: value));
    public static VectorIntent Ray(Point3d origin, Direction direction, RayPolicy? policy = null) =>
        new RayCase(Origin: origin, RayDirection: direction, Policy: policy ?? RayPolicy.Forward);
    public static VectorIntent Frame(Point3d origin, Vector3d normal, Option<Vector3d> xHint = default) =>
        new FrameCase(Origin: origin, Normal: normal, XHint: xHint);
    public static Fin<VectorIntent> Curve(Curve source, double parameter, CurveProjection mode, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Admit.NotNull(value: source, key: op)
               from _ in guard(active.IsValid && active.Domain.IncludesParameter(t: parameter), op.InvalidInput())
               from validMode in Admit.NotNull(value: mode, key: op)
               select (VectorIntent)new CurveCase(source: active, parameter: parameter, mode: validMode);
    }
    public static Fin<VectorIntent> Cloud(VectorCloud cloud, VectorCloudMetric metric, Option<CloudMetricPolicy> policy = default, Op? key = null) {
        Op op = key.OrDefault();
        return from validCloud in Admit.NotNull(value: cloud, key: op)
               from validMetric in Admit.NotNull(value: metric, key: op)
               from validPolicy in CloudMetricPolicy.AdmitOrDefault(policy: policy, key: op)
               from _ in guard(validMetric.AdmitsCase(cloud: validCloud), op.Unsupported(geometryType: validCloud.GetType(), outputType: validMetric.Output))
               select (VectorIntent)new CloudCase(value: validCloud, metric: validMetric, policy: validPolicy);
    }
    public static Fin<VectorIntent> Winding(VectorCloud cloud, Point3d query, Op? key = null) {
        Op op = key.OrDefault();
        return Admit.NotNull(value: cloud, key: op).Bind(valid => valid is VectorCloud.RingCase ring
            ? op.AcceptValue(value: query).Map(point => (VectorIntent)new WindingCase(value: ring, query: point))
            : Fin.Fail<VectorIntent>(op.Unsupported(geometryType: valid.GetType(), outputType: typeof(int))));
    }
    public static Fin<VectorIntent> Cone(VectorCone cone, ConeProjection mode, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in Admit.Cone(apex: cone.Apex, axis: cone.Axis.Value, halfAngle: cone.HalfAngle.Value, key: op)
               from activeMode in Admit.NotNull(value: mode, key: op)
               select (VectorIntent)new ConeCase(Value: cone, Mode: activeMode);
    }
    public static VectorIntent Components(Point3d anchor, Vector3d value, Plane frame) => new ComponentsCase(Anchor: anchor, Value: value, Basis: frame);
    public static VectorIntent Relation(Vector3d a, Vector3d b) => new RelationCase(A: a, B: b);
    public static Fin<VectorIntent> Bounce(Direction incident, SupportSpace surface, Point3d sample, BouncePolicy? policy = null, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(incident.IsValid, op.InvalidInput())
               from target in Admit.NotNull(value: surface, key: op)
               from bounce in Admit.NotNull(value: policy ?? BouncePolicy.Reflect, key: op)
               from point in op.AcceptValue(value: sample)
               select (VectorIntent)new BounceCase(Incident: incident, Target: target, Query: point, Policy: bounce);
    }
    public static Fin<VectorIntent> Streamline(VectorField field, Point3d seed, double initialStep, Termination termination, FieldIntegrator? integrator = null, Op? key = null) {
        Op op = key.OrDefault();
        return from validField in Admit.NotNull(value: field, key: op)
               from validStop in Termination.Admit(value: termination, key: op)
               from h in op.AcceptValidated<PositiveMagnitude>(candidate: initialStep)
               from validSeed in op.AcceptValue(value: seed)
               from validIntegrator in FieldIntegrator.AdmitOrFixed(value: integrator, key: op)
               select (VectorIntent)new StreamlineCase(source: validField, seed: validSeed, initialStep: h, integrator: validIntegrator, termination: validStop);
    }
    public static Fin<VectorIntent> Lerp(Vector3d a, Vector3d b, double t, Op? key = null) =>
        key.OrDefault().AcceptValidated<UnitInterval>(candidate: t).Map(unit => (VectorIntent)new LerpCase(A: a, B: b, Parameter: unit));
    public static Fin<VectorIntent> Slerp(Direction a, Direction b, double t, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in guard(a.IsValid && b.IsValid, op.InvalidInput())
               from unit in op.AcceptValidated<UnitInterval>(candidate: t)
               select (VectorIntent)new SlerpCase(A: a, B: b, Parameter: unit);
    }
    public static VectorIntent ProjectOnto(Vector3d value, Plane target) => new ProjectOntoCase(Value: value, Target: target);
    public static VectorIntent Mirror(Vector3d value, Plane across) => new MirrorCase(Value: value, Across: across);
    public static Fin<VectorIntent> Surface(SurfaceSpace surface, double u, double v, SurfaceProjection mode, Op? key = null) {
        Op op = key.OrDefault();
        return from active in SurfaceSpace.Of(native: surface.Native, context: surface.Tolerance, key: op)
               from uv in Evaluation.SurfaceUv(surface: active.Native, uv: new Point2d(x: u, y: v), context: active.Tolerance, key: op)
               from validMode in Admit.NotNull(value: mode, key: op)
               select (VectorIntent)new SurfaceCase(source: active, uv: uv, mode: validMode);
    }
    public static Fin<VectorIntent> Pose(Plane from, Plane to, double t, MotionInterpolation mode, Op? key = null) {
        Op op = key.OrDefault();
        return from source in Admit.Plane(basis: @from, key: op)
               from target in Admit.Plane(basis: to, key: op)
               from activeMode in Admit.NotNull(value: mode, key: op)
               from unit in op.AcceptValidated<UnitInterval>(candidate: t)
               select (VectorIntent)new PoseCase(From: source, To: target, Parameter: unit, Mode: activeMode);
    }
    public static Fin<VectorIntent> Flatten(MeshSpace space, Op? key = null) =>
        Admit.NotNull(value: space.Native, key: key.OrDefault()).Map(_ => (VectorIntent)new FlattenCase(space: space));
    public static Fin<VectorIntent> Hull(VectorCloud source, Option<CloudHullKind> kind = default, Option<CloudHullPolicy> policy = default, Op? key = null) {
        Op op = key.OrDefault();
        return from validSource in Admit.NotNull(value: source, key: op)
               from validKind in Admit.NotNull(value: kind.IfNone(CloudHullKind.Convex3D), key: op)
               from cluster in validSource is VectorCloud.ClusterCase c
                   ? Fin.Succ(c)
                   : Fin.Fail<VectorCloud.ClusterCase>(op.Unsupported(geometryType: validSource.GetType(), outputType: typeof(CloudHullResult)))
               from validPolicy in CloudHullPolicy.AdmitOrDefault(policy: policy, context: cluster.Tolerance, key: op)
               select (VectorIntent)new HullCase(source: cluster, kind: validKind, policy: validPolicy);
    }
    public static Fin<VectorIntent> Sample(ExtractionDomain domain, SampleKind kind, Op? key = null) {
        Op op = key.OrDefault();
        return from validDomain in Admit.NotNull(value: domain, key: op).Bind(active => active.Admit(key: op))
               from validKind in SampleKind.Admit(value: kind, key: op)
               select (VectorIntent)new SampleCase(domain: validDomain, kind: validKind);
    }
    public static Fin<VectorIntent> Align(VectorCloud source, VectorCloud target, AlignKind kind, AlignmentPolicy? policy = null, Op? key = null) {
        Op op = key.OrDefault();
        return from validSource in Admit.NotNull(value: source, key: op)
               from validTarget in Admit.NotNull(value: target, key: op)
               from validKind in Admit.NotNull(value: kind, key: op)
               from validPolicy in (policy ?? AlignmentPolicy.Default).Admit(key: op)
               select (VectorIntent)new AlignCase(source: validSource, target: validTarget, kind: validKind, policy: validPolicy);
    }
    public static Fin<VectorIntent> Remesh(MeshSpace space, RemeshKind kind, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in Admit.NotNull(value: space.Native, key: op)
               from activeKind in Admit.NotNull(value: kind, key: op)
               select (VectorIntent)new RemeshCase(space: space, kind: activeKind);
    }
    // CloudTransportPolicy admits at its own .Of; this rail rejects only the default-struct sentinel.
    public static Fin<VectorIntent> Transport(VectorCloud source, VectorCloud target, CloudTransportPolicy policy, Op? key = null) {
        Op op = key.OrDefault();
        return from validSource in Admit.NotNull(value: source, key: op)
               from validTarget in Admit.NotNull(value: target, key: op)
               from _ in guard(policy != default(CloudTransportPolicy), op.InvalidInput())
               select (VectorIntent)new TransportCase(source: validSource, target: validTarget, policy: policy);
    }
    public static Fin<VectorIntent> Topology(MeshSpace space, Op? key = null) =>
        Admit.NotNull(value: space.Native, key: key.OrDefault()).Map(_ => (VectorIntent)new TopologyCase(space: space));
    public static Fin<VectorIntent> Features(MeshSpace space, MeshFeaturePolicy policy, Op? key = null) {
        Op op = key.OrDefault();
        return from active in policy.Admit(space: space, key: op)
               select (VectorIntent)new FeaturesCase(space: space, policy: active);
    }
    public static Fin<VectorIntent> Descriptor(MeshSpace space, MeshDescriptor kind, int pairs, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in Admit.NotNull(value: space.Native, key: op)
               from active in Admit.NotNull(value: kind, key: op)
               from count in op.AcceptValidated<Dimension>(candidate: pairs)
               select (VectorIntent)new DescriptorCase(space: space, kind: active, pairs: count);
    }
    public static Fin<VectorIntent> DiscreteCalculus(MeshSpace space, MeshLaplacian? kind = null, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in Admit.NotNull(value: space.Native, key: op)
               from active in Admit.NotNull(value: kind ?? MeshLaplacian.IntrinsicDelaunay, key: op)
               select (VectorIntent)new DiscreteCalculusCase(space: space, kind: active);
    }
    public static Fin<VectorIntent> Segmentation(MeshSpace space, MeshSegmentation kind, Op? key = null) {
        Op op = key.OrDefault();
        return from _ in Admit.NotNull(value: space.Native, key: op)
               from active in Admit.NotNull(value: kind, key: op)
               select (VectorIntent)new SegmentationCase(space: space, kind: active);
    }
    // --- [DISPATCH]
    public Fin<TOut> Project<TOut>(Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from model in Admit.NotNull(value: context, error: op.MissingContext())
               from result in Dispatch<TOut>(context: model, op: op)
               select result;
    }
    private Fin<TOut> Dispatch<TOut>(Context context, Op op) => Switch(
        state: (Context: context, Key: op),
        axisCase: static (state, axis) =>
            from direction in Numerics.Direction.Of(value: axis.Value.Of(frame: axis.Basis), context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        directionCase: static (state, intent) =>
            from direction in Numerics.Direction.Of(value: intent.Value, context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        axesCase: static (state, intent) =>
            from axes in intent.Values.IfNone(SignedAxis.Cardinal(planar: intent.Planar).Map(static axis => axis.World))
                .TraverseM(axis => Numerics.Direction.Of(value: axis, context: state.Context, key: state.Key).Map(static direction => direction.Value))
                .As()
            from _ in guard(!axes.IsEmpty, state.Key.InvalidInput())
            from output in AtomProjection.Self<Seq<Vector3d>, TOut>(value: axes, key: state.Key, owner: typeof(AxesCase))
            select output,
        angularCase: static (state, intent) =>
            from angle in VectorAngle.Of(a: intent.A, b: intent.B, context: state.Context, pivot: intent.Pivot, key: state.Key)
            from output in angle.Project<TOut>(key: state.Key)
            select output,
        supportCase: static (state, intent) =>
            from hit in intent.Space.Closest(sample: intent.Query, key: state.Key)
            from output in intent.Projection.Project<TOut>(space: intent.Space, hit: hit, sample: intent.Query, context: state.Context, key: state.Key)
            select output,
        extractionCase: static (state, intent) => intent.Value.Project<TOut>(context: state.Context, key: state.Key),
        rayCase: static (state, intent) => intent.Policy.Project<TOut>(origin: intent.Origin, direction: intent.RayDirection, context: state.Context, key: state.Key),
        frameCase: static (state, intent) =>
            from frame in VectorFrame.Of(origin: intent.Origin, normal: intent.Normal, xHint: intent.XHint, context: state.Context, key: state.Key)
            from output in frame.Project<TOut>(key: state.Key)
            select output,
        curveCase: static (state, intent) => intent.Mode.Project<TOut>(curve: intent.Source, parameter: intent.Parameter, context: state.Context, key: state.Key),
        cloudCase: static (state, intent) => intent.Metric.Project<TOut>(cloud: intent.Value, policy: intent.Policy, key: state.Key),
        windingCase: static (state, intent) =>
            from normal in CloudKernel.RingNormalOf(ring: intent.Value, key: state.Key)
            from winding in CloudKernel.PlanarWindingOf(ring: intent.Value.Vertices, planeNormal: normal, query: intent.Query, key: state.Key)
            from output in AtomProjection.Self<int, TOut>(value: winding, key: state.Key, owner: typeof(WindingCase))
            select output,
        coneCase: static (state, intent) => intent.Mode.Project<TOut>(cone: intent.Value, key: state.Key),
        componentsCase: static (state, intent) =>
            from span in VectorSpan.Of(anchor: intent.Anchor, vector: intent.Value, context: state.Context, key: state.Key)
            from components in span.Components(frame: intent.Basis, key: state.Key)
            from output in AtomProjection.Self<(double X, double Y), TOut>(value: components, key: state.Key, owner: typeof(ComponentsCase))
            select output,
        relationCase: static (state, intent) =>
            from relation in VectorRelation.Of(a: intent.A, b: intent.B, context: state.Context, key: state.Key)
            from output in relation.Project<TOut>(key: state.Key)
            select output,
        bounceCase: static (state, intent) =>
            from hit in intent.Target.Closest(sample: intent.Query, key: state.Key)
            from rawNormal in hit.Normal.ToFin(Fail: state.Key.InvalidResult())
            from normal in Numerics.Direction.Of(value: rawNormal, context: state.Context, key: state.Key)
            from reflected in intent.Policy.Apply(incident: intent.Incident, normal: normal, key: state.Key)
            from output in reflected.Project<TOut>(key: state.Key)
            select output,
        streamlineCase: static (state, intent) => FlowKernel.Trace<TOut>(source: intent.Source, seed: intent.Seed, initialStep: intent.InitialStep, integrator: intent.Integrator, termination: intent.Termination, context: state.Context, key: state.Key),
        // Lerp/projectOnto/mirror: ONE native affine/Transform expression each, admitted through Direction.Of.
        lerpCase: static (state, intent) =>
            from direction in Numerics.Direction.Of(value: ((1.0 - intent.Parameter.Value) * intent.A) + (intent.Parameter.Value * intent.B), context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        // THE one slerp site is projections.md's MotionInterpolation.Rotate; the antiparallel branch lives there.
        slerpCase: static (state, intent) =>
            from direction in MotionInterpolation.Slerp.Rotate(a: intent.A, b: intent.B, t: intent.Parameter, context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        projectOntoCase: static (state, intent) =>
            from direction in Numerics.Direction.Of(value: Transform.PlanarProjection(plane: intent.Target) * intent.Value, context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        mirrorCase: static (state, intent) =>
            from direction in Numerics.Direction.Of(value: Transform.Mirror(mirrorPlane: intent.Across) * intent.Value, context: state.Context, key: state.Key)
            from output in direction.Project<TOut>(key: state.Key)
            select output,
        surfaceCase: static (state, intent) => intent.Source.Sample<TOut>(projection: intent.Mode, u: intent.Uv.X, v: intent.Uv.Y, key: state.Key),
        poseCase: static (state, intent) =>
            from pose in intent.Mode.Interpolate(a: intent.From, b: intent.To, t: intent.Parameter, key: state.Key)
            from output in Admit.Plane(basis: pose, key: state.Key)
                .Bind(plane => AtomProjection.Self<Plane, TOut>(value: plane, key: state.Key, owner: typeof(PoseCase)))
            select output,
        flattenCase: static (state, intent) =>
            from result in SegmentKernel.ParameterizeFlattenDetailed(space: intent.Space, key: state.Key)
            from output in result.Project<TOut>(key: state.Key)
            select output,
        hullCase: static (state, intent) =>
            from result in CloudKernel.ComputeHullDetailed(source: intent.Source, kind: intent.Kind, policy: intent.Policy, key: state.Key)
            from output in result.Project<TOut>(context: state.Context, key: state.Key)
            select output,
        sampleCase: static (state, intent) => intent.Kind.Project<TOut>(domain: intent.Domain, context: state.Context, key: state.Key),
        alignCase: static (state, intent) =>
            from receipt in intent.Kind.AlignDetailed(source: intent.Source, target: intent.Target, policy: intent.Policy, key: state.Key)
            from output in receipt.Project<TOut>(key: state.Key)
            select output,
        remeshCase: static (state, intent) =>
            from result in SegmentKernel.ApplyRemeshDetailed(kind: intent.Kind, space: intent.Space, key: state.Key)
            from output in result.Project<TOut>(key: state.Key)
            select output,
        transportCase: static (state, intent) =>
            CloudTransport.Sinkhorn<TOut>(source: intent.Source, target: intent.Target, policy: intent.Policy, key: state.Key),
        topologyCase: static (state, intent) =>
            from topology in MeshKernel.TopologyDetailed(space: intent.Space)
            from output in topology.Project<TOut>(key: state.Key)
            select output,
        featuresCase: static (state, intent) =>
            from receipt in SegmentKernel.DetectFeatureEdgesDetailed(space: intent.Space, policy: intent.Policy, key: state.Key)
            from output in receipt.Project<TOut>(key: state.Key)
            select output,
        descriptorCase: static (state, intent) => SegmentKernel.DescribeShape<TOut>(space: intent.Space, kind: intent.Kind, eigenpairs: intent.Pairs.Value, key: state.Key),
        discreteCalculusCase: static (state, intent) =>
            from calculus in DecAssembly.Build(space: intent.Space, kind: intent.Kind, key: state.Key)
            from output in calculus.Project<TOut>(key: state.Key)
            select output,
        segmentationCase: static (state, intent) => SegmentKernel.Segment<TOut>(space: intent.Space, kind: intent.Kind, key: state.Key));
}
```

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

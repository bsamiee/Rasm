namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class FieldBlend {
    public static readonly FieldBlend Sum = new(key: 0, scale: static _ => 1.0);
    public static readonly FieldBlend Average = new(key: 1, scale: static count => count > 0 ? 1.0 / count : 1.0);
    internal Fin<Vector3d> Combine(Seq<Vector3d> vectors, Op key) =>
        vectors.IsEmpty
            ? Fin.Fail<Vector3d>(error: key.InvalidResult())
            : key.AcceptValue(value: vectors.Fold(initialState: Vector3d.Zero, f: static (sum, v) => sum + v) * Scale(count: vectors.Count));
    internal Fin<double> CombineScalar(Seq<double> values, Op key) =>
        values.IsEmpty
            ? Fin.Fail<double>(error: key.InvalidResult())
            : key.AcceptValue(value: values.Fold(initialState: 0.0, f: static (sum, v) => sum + v) * Scale(count: values.Count));
    [UseDelegateFromConstructor] private partial double Scale(int count);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct ButcherTableau(Seq<Seq<double>> Coupling, Seq<double> Weights, Option<Seq<double>> ErrorWeights);

[SmartEnum<int>]
public sealed partial class IntegratorKind {
    public static readonly IntegratorKind Euler = new(key: 0, tableau: new ButcherTableau(
        Coupling: [[]],
        Weights: [1.0],
        ErrorWeights: Option<Seq<double>>.None));
    public static readonly IntegratorKind Heun = new(key: 1, tableau: new ButcherTableau(
        Coupling: [[], [1.0]],
        Weights: [0.5, 0.5],
        ErrorWeights: Option<Seq<double>>.None));
    public static readonly IntegratorKind RK4 = new(key: 2, tableau: new ButcherTableau(
        Coupling: [[], [0.5], [0.0, 0.5], [0.0, 0.0, 1.0]],
        Weights: [1.0 / 6.0, 1.0 / 3.0, 1.0 / 3.0, 1.0 / 6.0],
        ErrorWeights: Option<Seq<double>>.None));
    public static readonly IntegratorKind DormandPrince = new(key: 3, tableau: new ButcherTableau(
        Coupling: [
            [],
            [1.0 / 5.0],
            [3.0 / 40.0, 9.0 / 40.0],
            [44.0 / 45.0, -56.0 / 15.0, 32.0 / 9.0],
            [19372.0 / 6561.0, -25360.0 / 2187.0, 64448.0 / 6561.0, -212.0 / 729.0],
            [9017.0 / 3168.0, -355.0 / 33.0, 46732.0 / 5247.0, 49.0 / 176.0, -5103.0 / 18656.0],
            [35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0]],
        Weights: [35.0 / 384.0, 0.0, 500.0 / 1113.0, 125.0 / 192.0, -2187.0 / 6784.0, 11.0 / 84.0, 0.0],
        ErrorWeights: Some<Seq<double>>([5179.0 / 57600.0, 0.0, 7571.0 / 16695.0, 393.0 / 640.0, -92097.0 / 339200.0, 187.0 / 2100.0, 1.0 / 40.0])));
    public ButcherTableau Tableau { get; }
    internal bool IsAdaptive => Tableau.ErrorWeights.IsSome;
}

[SmartEnum<int>]
public sealed partial class HitProjection {
    public static readonly HitProjection Normal = new(key: 0,
        canProject: static space => GeometryKernel.CanClosestNormal(type: space.SourceType),
        admits: static (space, hit) => space.AdmitsNormal(hit: hit),
        extract: static hit => hit.Normal);
    public static readonly HitProjection Tangent = new(key: 1,
        canProject: static space => GeometryKernel.CanClosestTangent(type: space.SourceType),
        admits: static (space, hit) => space.AdmitsTangent(hit: hit),
        extract: static hit => hit.Tangent);
    [UseDelegateFromConstructor] internal partial bool CanProject(SupportSpace space);
    [UseDelegateFromConstructor] internal partial bool Admits(SupportSpace space, ClosestHit hit);
    [UseDelegateFromConstructor] internal partial Option<Vector3d> Extract(ClosestHit hit);
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record FieldIntegrator {
    private FieldIntegrator() { }
    public sealed record FixedCase(IntegratorKind Kind) : FieldIntegrator;
    public sealed record AdaptiveCase(IntegratorKind Kind, PositiveMagnitude Tolerance, int MaxRejects) : FieldIntegrator;
    public static FieldIntegrator Euler => new FixedCase(Kind: IntegratorKind.Euler);
    public static FieldIntegrator Heun => new FixedCase(Kind: IntegratorKind.Heun);
    public static FieldIntegrator RK4 => new FixedCase(Kind: IntegratorKind.RK4);
    public static Fin<FieldIntegrator> Adaptive(IntegratorKind kind, double tolerance, int maxRejects = 3, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(kind).ToFin(op.InvalidInput())
               from _ in guard(active.IsAdaptive && maxRejects >= 0, op.Unsupported(geometryType: active.GetType(), outputType: typeof(AdaptiveCase)))
               from validated in op.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
               select (FieldIntegrator)new AdaptiveCase(Kind: active, Tolerance: validated, MaxRejects: maxRejects);
    }
    public static Fin<FieldIntegrator> RK45Adaptive(double tolerance, int maxRejects = 3, Op? key = null) =>
        Adaptive(kind: IntegratorKind.DormandPrince, tolerance: tolerance, maxRejects: maxRejects, key: key);
    internal int RejectBudget => Switch(
        state: 0,
        fixedCase: static (s, _) => s,
        adaptiveCase: static (_, c) => c.MaxRejects);
    internal Fin<(Point3d Next, double SuggestedStep, bool Accepted)> Step(VectorField field, Point3d point, double h, Context context, Op key) => Switch(
        state: (Field: field, Point: point, H: h, Context: context, Key: key),
        fixedCase: static (s, c) =>
            from ks in ComputeStages(tableau: c.Kind.Tableau, field: s.Field, point: s.Point, h: s.H, context: s.Context, key: s.Key)
            from next in s.Key.AcceptValue(value: s.Point + (s.H * Combine(coefficients: c.Kind.Tableau.Weights, vectors: ks)))
            select (Next: next, SuggestedStep: s.H, Accepted: true),
        adaptiveCase: static (s, c) =>
            from errWeights in c.Kind.Tableau.ErrorWeights.ToFin(Fail: s.Key.InvalidInput())
            from ks in ComputeStages(tableau: c.Kind.Tableau, field: s.Field, point: s.Point, h: s.H, context: s.Context, key: s.Key)
            let primary = s.Point + (s.H * Combine(coefficients: c.Kind.Tableau.Weights, vectors: ks))
            let secondary = s.Point + (s.H * Combine(coefficients: errWeights, vectors: ks))
            let err = (primary - secondary).Length
            let scale = err > RhinoMath.ZeroTolerance ? Math.Clamp(value: 0.9 * Math.Pow(x: c.Tolerance.Value / err, y: 0.2), min: 0.2, max: 10.0) : 10.0
            from result in err <= c.Tolerance.Value
                ? s.Key.AcceptValue(value: (Next: primary, SuggestedStep: s.H * scale, Accepted: true))
                : s.Key.AcceptValue(value: (Next: s.Point, SuggestedStep: s.H * scale, Accepted: false))
            select result);
    private static Fin<Seq<Vector3d>> ComputeStages(ButcherTableau tableau, VectorField field, Point3d point, double h, Context context, Op key) =>
        tableau.Coupling.Fold(
            initialState: Fin.Succ((Seq<Vector3d>)[]),
            f: (acc, row) => acc.Bind(ks =>
                field.SampleVector(sample: point + (h * Combine(coefficients: row, vectors: ks)), context: context, key: key)
                    .Map(k => ks.Add(k))));
    private static Vector3d Combine(Seq<double> coefficients, Seq<Vector3d> vectors) =>
        toSeq(Enumerable.Range(start: 0, count: coefficients.Count))
            .Fold(initialState: Vector3d.Zero, f: (sum, i) => sum + (coefficients[i] * vectors[i]));
}

[Union]
public abstract partial record Termination {
    private Termination() { }
    public sealed record StepCountCase(int Count) : Termination;
    public sealed record ArcLengthCase(PositiveMagnitude Length) : Termination;
    public sealed record MagnitudeFloorCase(PositiveMagnitude Threshold) : Termination;
    public static Fin<Termination> Steps(int count, Op? key = null) {
        Op op = key.OrDefault();
        return count > 0
            ? Fin.Succ<Termination>(new StepCountCase(Count: count))
            : Fin.Fail<Termination>(op.InvalidInput());
    }
    public static Fin<Termination> ArcLength(double length, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: length)
            .Map(static l => (Termination)new ArcLengthCase(Length: l));
    }
    public static Fin<Termination> Magnitude(double threshold, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: threshold)
            .Map(static t => (Termination)new MagnitudeFloorCase(Threshold: t));
    }
    internal bool ShouldStop(int stepCount, double arcLengthSoFar, Vector3d currentSample) => Switch(
        state: (StepCount: stepCount, ArcLength: arcLengthSoFar, Sample: currentSample),
        stepCountCase: static (s, c) => s.StepCount >= c.Count,
        arcLengthCase: static (s, c) => s.ArcLength >= c.Length.Value,
        magnitudeFloorCase: static (s, c) => s.Sample.Length < c.Threshold.Value);
}

[Union]
public abstract partial record Falloff {
    private Falloff() { }
    public sealed record ConstantCase : Falloff;
    public sealed record InverseCase : Falloff;
    public sealed record InverseSquareCase : Falloff;
    public sealed record GaussianCase(PositiveMagnitude Sigma) : Falloff;
    public static Falloff Constant => new ConstantCase();
    public static Falloff Inverse => new InverseCase();
    public static Falloff InverseSquare => new InverseSquareCase();
    public static Fin<Falloff> Gaussian(double sigma, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: sigma).Map(static value => (Falloff)new GaussianCase(Sigma: value));
    }
    internal Fin<double> Weight(double distance, double tolerance, Op key) => Switch(
        state: (Distance: distance, Tolerance: tolerance, Key: key),
        constantCase: static (_, _) => Fin.Succ(1.0),
        inverseCase: static (state, _) => state.Distance > state.Tolerance ? Fin.Succ(1.0 / state.Distance) : Fin.Fail<double>(state.Key.InvalidInput()),
        inverseSquareCase: static (state, _) => state.Distance > state.Tolerance ? Fin.Succ(1.0 / (state.Distance * state.Distance)) : Fin.Fail<double>(state.Key.InvalidInput()),
        gaussianCase: static (state, gaussian) => Fin.Succ(Math.Exp(-(state.Distance * state.Distance) / (2.0 * gaussian.Sigma.Value * gaussian.Sigma.Value))));
}

[Union]
public abstract partial record RayPolicy {
    private RayPolicy() { }
    public sealed record InfiniteCase(BoundarySense Sense) : RayPolicy;
    public sealed record SegmentCase(BoundarySense Sense, PositiveMagnitude Length) : RayPolicy;
    public static RayPolicy Forward => new InfiniteCase(Sense: BoundarySense.Toward);
    public static RayPolicy Reverse => new InfiniteCase(Sense: BoundarySense.Away);
    public static Fin<RayPolicy> Segment(double length, BoundarySense? sense = null, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: length)
            .Map(value => (RayPolicy)new SegmentCase(Sense: sense ?? BoundarySense.Toward, Length: value));
    }
    internal Fin<TOut> Project<TOut>(Point3d origin, Direction direction, Context context, Op key) =>
        from point in key.AcceptValue(value: origin)
        let policy = Switch(
            state: direction.Value,
            infiniteCase: static (value, c) => (Vector: value * c.Sense.Sign, Length: Option<PositiveMagnitude>.None),
            segmentCase: static (value, c) => (Vector: value * c.Sense.Sign, Length: Some(c.Length)))
        from output in typeof(TOut) switch {
            Type t when t == typeof(Ray3d) => key.AcceptValue(value: new Ray3d(position: point, direction: policy.Vector)).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Plane) => key.AcceptValue(value: new Plane(origin: point, normal: policy.Vector)).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Direction) => Direction.Of(value: policy.Vector, context: context, key: key).Bind(active => active.Project<TOut>(key: key)),
            Type t when t == typeof(Vector3d) => key.AcceptValue(value: policy.Vector).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Line) => policy.Length.ToFin(key.Unsupported(geometryType: typeof(Ray3d), outputType: typeof(TOut)))
                .Bind(length => key.AcceptValue(value: new Line(start: point, direction: policy.Vector, length: length.Value)))
                .Map(static value => (TOut)(object)value),
            Type t when t == typeof(VectorSpan) => policy.Length.ToFin(key.Unsupported(geometryType: typeof(Ray3d), outputType: typeof(TOut)))
                .Bind(length => VectorSpan.Of(anchor: point, vector: policy.Vector * length.Value, context: context, key: key))
                .Bind(span => span.Project<TOut>(key: key)),
            _ => Fin.Fail<TOut>(error: key.Unsupported(geometryType: typeof(Ray3d), outputType: typeof(TOut))),
        }
        select output;
}

[Union]
public abstract partial record BouncePolicy {
    private BouncePolicy() { }
    public sealed record ReflectCase : BouncePolicy;
    public sealed record RefractCase(PositiveMagnitude EtaIncident, PositiveMagnitude EtaTransmitted) : BouncePolicy;
    public static BouncePolicy Reflect => new ReflectCase();
    public static Fin<BouncePolicy> Refract(double etaIncident, double etaTransmitted, Op? key = null) {
        Op op = key.OrDefault();
        return (op.AcceptValidated<PositiveMagnitude>(candidate: etaIncident),
                op.AcceptValidated<PositiveMagnitude>(candidate: etaTransmitted))
            .Apply(static (incident, transmitted) => (BouncePolicy)new RefractCase(EtaIncident: incident, EtaTransmitted: transmitted))
            .As();
    }
    internal Fin<Direction> Apply(Direction incident, Direction normal, Op key) => Switch(
        state: (Incident: incident, Normal: normal, Key: key),
        reflectCase: static (state, _) => Fin.Succ(state.Incident.Reflect(normal: state.Normal)),
        refractCase: static (state, refract) => Direction.Refract(
            incident: state.Incident, normal: state.Normal,
            etaIncident: refract.EtaIncident.Value, etaTransmitted: refract.EtaTransmitted.Value, key: state.Key));
}

[Union]
public partial record VectorField {
    public sealed record ConstantCase(Vector3d Value) : VectorField;
    public sealed record InfluenceCase(SupportSpace Source, Falloff Falloff, BoundarySense Sense, Option<PositiveMagnitude> Radius) : VectorField;
    public sealed record HitFieldCase(SupportSpace Source, HitProjection Projection, BoundarySense Sense) : VectorField;
    public sealed record BlendCase(Seq<VectorField> Fields, FieldBlend Mode) : VectorField;
    public sealed record VortexCase(Point3d Anchor, Direction Axis, Falloff Falloff) : VectorField;
    public sealed record CoulombCase(Seq<(Point3d Position, double Charge)> Charges, Falloff Falloff) : VectorField;
    public sealed record ClusterFieldCase(VectorCloud.ClusterCase Source, Falloff Falloff, PositiveMagnitude Radius, BoundarySense Sense) : VectorField;
    public sealed record DipoleCase(Point3d Origin, Direction Moment, PositiveMagnitude Strength) : VectorField;
    public sealed record HarmonicCase(Seq<(Direction Direction, double Frequency, double Phase, double Amplitude)> Components) : VectorField;
    public sealed record ProjectedCase(VectorField Source, Plane Onto) : VectorField;
    public sealed record WarpCase(VectorField Source, Transform Spatial) : VectorField;
    public sealed record ClampMagnitudeCase(VectorField Source, PositiveMagnitude Min, PositiveMagnitude Max) : VectorField;
    public sealed record ScaledCase(VectorField Source, double Scale) : VectorField;
    public sealed record GradientCase(ScalarField Source, PositiveMagnitude Epsilon) : VectorField;
    public sealed record CurlCase(VectorField Source, PositiveMagnitude Epsilon) : VectorField;
    public static VectorField Constant(Vector3d value) => new ConstantCase(Value: value);
    public static VectorField Influence(SupportSpace source, Falloff? falloff = null, BoundarySense? sense = null) =>
        new InfluenceCase(Source: source, Falloff: falloff ?? Falloff.Inverse, Sense: sense ?? BoundarySense.Toward, Radius: Option<PositiveMagnitude>.None);
    public static Fin<VectorField> Hit(SupportSpace source, HitProjection projection, BoundarySense? sense = null, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from _ in guard(projection.CanProject(space: active), op.Unsupported(active.SourceType, typeof(Vector3d)))
               select (VectorField)new HitFieldCase(Source: active, Projection: projection, Sense: sense ?? BoundarySense.Toward);
    }
    public static Fin<VectorField> Normal(SupportSpace source, BoundarySense? sense = null, Op? key = null) =>
        Hit(source: source, projection: HitProjection.Normal, sense: sense, key: key);
    public static Fin<VectorField> Tangent(SupportSpace source, BoundarySense? sense = null, Op? key = null) =>
        Hit(source: source, projection: HitProjection.Tangent, sense: sense, key: key);
    public static Fin<VectorField> Shell(SupportSpace source, double radius, Falloff? falloff = null, BoundarySense? sense = null, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: radius)
            .Map(value => (VectorField)new InfluenceCase(Source: source, Falloff: falloff ?? Falloff.Constant, Sense: sense ?? BoundarySense.Toward, Radius: Some(value)));
    }
    public static VectorField Blend(Seq<VectorField> fields, FieldBlend? blend = null) =>
        new BlendCase(Fields: fields, Mode: blend ?? FieldBlend.Sum);
    public static VectorField Vortex(Point3d anchor, Direction axis, Falloff? falloff = null) =>
        new VortexCase(Anchor: anchor, Axis: axis, Falloff: falloff ?? Falloff.Constant);
    public static VectorField Coulomb(Seq<(Point3d Position, double Charge)> charges, Falloff? falloff = null) =>
        new CoulombCase(Charges: charges, Falloff: falloff ?? Falloff.InverseSquare);
    public static Fin<VectorField> Cluster(VectorCloud cluster, double radius, Falloff? falloff = null, BoundarySense? sense = null, Op? key = null) {
        Op op = key.OrDefault();
        return cluster switch {
            VectorCloud.ClusterCase c =>
                from r in op.AcceptValidated<PositiveMagnitude>(candidate: radius)
                from f in falloff is null ? Falloff.Gaussian(sigma: r.Value / 3.0, key: op) : Fin.Succ(falloff)
                select (VectorField)new ClusterFieldCase(Source: c, Falloff: f, Radius: r, Sense: sense ?? BoundarySense.Toward),
            _ => Fin.Fail<VectorField>(op.Unsupported(geometryType: cluster.GetType(), outputType: typeof(VectorField))),
        };
    }
    public static Fin<VectorField> Dipole(Point3d origin, Direction moment, double strength, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: strength)
            .Map(s => (VectorField)new DipoleCase(Origin: origin, Moment: moment, Strength: s));
    }
    public static VectorField Harmonic(Seq<(Direction Direction, double Frequency, double Phase, double Amplitude)> components) =>
        new HarmonicCase(Components: components);
    public static VectorField Projected(VectorField source, Plane onto) =>
        new ProjectedCase(Source: source, Onto: onto);
    public static VectorField Warp(VectorField source, Transform spatial) =>
        new WarpCase(Source: source, Spatial: spatial);
    public static Fin<VectorField> ClampMagnitude(VectorField source, double min, double max, Op? key = null) {
        Op op = key.OrDefault();
        return from low in op.AcceptValidated<PositiveMagnitude>(candidate: min)
               from high in op.AcceptValidated<PositiveMagnitude>(candidate: max)
               from _ in guard(low.Value <= high.Value, op.InvalidInput())
               select (VectorField)new ClampMagnitudeCase(Source: source, Min: low, Max: high);
    }
    public static Fin<VectorField> Divide(VectorField source, double divisor, Op? key = null) {
        Op op = key.OrDefault();
        return Math.Abs(value: divisor) > RhinoMath.ZeroTolerance
            ? Fin.Succ<VectorField>(new ScaledCase(Source: source, Scale: 1.0 / divisor))
            : Fin.Fail<VectorField>(op.InvalidInput());
    }
    public static Fin<VectorField> Gradient(ScalarField source, double epsilon, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from eps in op.AcceptValidated<PositiveMagnitude>(candidate: epsilon)
               select (VectorField)new GradientCase(Source: active, Epsilon: eps);
    }
    public static Fin<VectorField> Curl(VectorField source, double epsilon, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from eps in op.AcceptValidated<PositiveMagnitude>(candidate: epsilon)
               select (VectorField)new CurlCase(Source: active, Epsilon: eps);
    }
    public static VectorField Zero { get; } = Constant(value: Vector3d.Zero);
    // Monoid: associative under flatten-into-BlendCase(Sum); Zero is the identity. Canonical sum is always a flat BlendCase, never nested.
    public static VectorField operator +(VectorField left, VectorField right) => (left, right) switch {
        (BlendCase l, BlendCase r) when l.Mode.Equals(FieldBlend.Sum) && r.Mode.Equals(FieldBlend.Sum) =>
            new BlendCase(Fields: l.Fields.Concat(r.Fields).ToSeq(), Mode: FieldBlend.Sum),
        (BlendCase l, _) when l.Mode.Equals(FieldBlend.Sum) =>
            new BlendCase(Fields: l.Fields.Add(right), Mode: FieldBlend.Sum),
        (_, BlendCase r) when r.Mode.Equals(FieldBlend.Sum) =>
            new BlendCase(Fields: Seq(left).Concat(r.Fields).ToSeq(), Mode: FieldBlend.Sum),
        _ => new BlendCase(Fields: Seq(left, right), Mode: FieldBlend.Sum),
    };
    public static VectorField operator -(VectorField left, VectorField right) => left + (-right);
    public static VectorField operator -(VectorField field) => new ScaledCase(Source: field, Scale: -1.0);
    public static VectorField operator *(VectorField field, double scale) => new ScaledCase(Source: field, Scale: scale);
    public static VectorField operator *(double scale, VectorField field) => new ScaledCase(Source: field, Scale: scale);
    internal Fin<TOut> Project<TOut>(Point3d sample, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from point in op.AcceptValue(value: sample)
               from vector in SampleVector(sample: point, context: context, key: op)
               from output in typeof(TOut) switch {
                   Type t when t == typeof(Vector3d) => op.AcceptValue(value: vector).Map(static value => (TOut)(object)value),
                   Type t when t == typeof(double) => op.AcceptValue(value: vector.Length).Map(static value => (TOut)(object)value),
                   _ => VectorSpan.Of(anchor: point, vector: vector, context: context, key: op).Bind(span => span.Project<TOut>(key: op)),
               }
               select output;
    }
    internal Fin<Vector3d> SampleVector(Point3d sample, Context context, Op key) => Switch(
        state: (Sample: sample, Context: context, Key: key),
        constantCase: static (state, c) => state.Key.AcceptValue(value: c.Value),
        influenceCase: static (state, c) => ClosestDirected(
            source: c.Source, sample: state.Sample, sense: c.Sense, context: state.Context, key: state.Key,
            hitToScaled: (hit, op) =>
                from distance in hit.Distance.ToFin(Fail: op.InvalidResult())
                let residual = c.Radius.Map(radius => Math.Abs(distance - radius.Value)).IfNone(distance)
                let shellSign = c.Radius.Map(radius => distance >= radius.Value ? 1.0 : -1.0).IfNone(1.0)
                from weight in c.Falloff.Weight(distance: residual, tolerance: state.Context.Absolute.Value, key: op)
                select (Raw: shellSign * (hit.Point - state.Sample), Scale: c.Radius.IsSome ? residual * weight : weight)),
        hitFieldCase: static (state, c) =>
            from vector in ClosestDirected(
                source: c.Source, sample: state.Sample, sense: c.Sense, context: state.Context, key: state.Key,
                hitToScaled: (hit, op) => from _ in guard(c.Projection.Admits(space: c.Source, hit: hit), op.Unsupported(c.Source.SourceType, typeof(Vector3d)))
                                          from raw in c.Projection.Extract(hit: hit).ToFin(Fail: op.InvalidResult())
                                          select (Raw: raw, Scale: 1.0))
            select vector,
        blendCase: static (state, c) => c.Fields.TraverseM(field => field.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)).As()
            .Bind(vectors => c.Mode.Combine(vectors: vectors, key: state.Key)),
        vortexCase: static (state, c) => {
            Vector3d r = state.Sample - c.Anchor;
            Vector3d rPerp = r - (r * c.Axis.Value * c.Axis.Value);
            return c.Falloff.Weight(distance: rPerp.Length, tolerance: state.Context.Absolute.Value, key: state.Key)
                .Bind(weight => state.Key.AcceptValue(value: Vector3d.CrossProduct(a: c.Axis.Value, b: rPerp) * weight));
        },
        coulombCase: static (state, c) => c.Charges.Fold(
            initialState: Fin.Succ(Vector3d.Zero),
            f: (acc, charge) => acc.Bind(sum => SampleRadialContribution(sum: sum, source: charge.Position, scale: charge.Charge, state: state, falloff: c.Falloff))),
        clusterFieldCase: static (state, c) => c.Source.WithinRadius(sample: state.Sample, radius: c.Radius.Value, key: state.Key)
            .Bind(indices => indices.Fold(
                initialState: Fin.Succ(Vector3d.Zero),
                f: (acc, i) => acc.Bind(sum => SampleRadialContribution(sum: sum, source: c.Source.Vertices[i], scale: c.Sense.Sign, state: state, falloff: c.Falloff)))),
        dipoleCase: static (state, c) =>
            from r in Fin.Succ(state.Sample - c.Origin)
            let distance = r.Length
            from _ in guard(distance > state.Context.Absolute.Value, state.Key.InvalidInput())
            let rHat = r / distance
            from output in state.Key.AcceptValue(value: c.Strength.Value * ((3.0 * (c.Moment.Value * rHat) * rHat) - c.Moment.Value) / (distance * distance * distance))
            select output,
        harmonicCase: static (state, c) => state.Key.AcceptValue(value: c.Components.Fold(
            initialState: Vector3d.Zero,
            f: (sum, comp) => sum + (comp.Direction.Value * comp.Amplitude * Math.Sin(a: (comp.Frequency * (comp.Direction.Value * (Vector3d)state.Sample)) + comp.Phase)))),
        projectedCase: static (state, c) => c.Source.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(v => state.Key.AcceptValue(value: v - (v * c.Onto.ZAxis * c.Onto.ZAxis))),
        warpCase: static (state, c) => c.Spatial.TryGetInverse(inverseTransform: out Transform inverse) switch {
            false => Fin.Fail<Vector3d>(state.Key.InvalidResult()),
            true => c.Source.SampleVector(sample: inverse * state.Sample, context: state.Context, key: state.Key)
                .Bind(v => state.Key.AcceptValue(value: c.Spatial * v)),
        },
        clampMagnitudeCase: static (state, c) => c.Source.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(v => v.Length switch {
                double mag when mag <= state.Context.Absolute.Value => state.Key.AcceptValue(value: Vector3d.Zero),
                double mag => state.Key.AcceptValue(value: v * (Math.Clamp(value: mag, min: c.Min.Value, max: c.Max.Value) / mag)),
            }),
        scaledCase: static (state, c) => c.Source.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(v => state.Key.AcceptValue(value: c.Scale * v)),
        gradientCase: static (state, c) =>
            from samples in FieldNabla.SampleAxes(sampler: p => c.Source.SampleScalar(sample: p, context: state.Context, key: state.Key), point: state.Sample, eps: c.Epsilon.Value)
            let inv2eps = 1.0 / (2.0 * c.Epsilon.Value)
            from grad in state.Key.AcceptValue(value: new Vector3d(
                x: (samples.X1 - samples.X0) * inv2eps,
                y: (samples.Y1 - samples.Y0) * inv2eps,
                z: (samples.Z1 - samples.Z0) * inv2eps))
            select grad,
        curlCase: static (state, c) =>
            from samples in FieldNabla.SampleAxes(sampler: p => c.Source.SampleVector(sample: p, context: state.Context, key: state.Key), point: state.Sample, eps: c.Epsilon.Value)
            let inv2eps = 1.0 / (2.0 * c.Epsilon.Value)
            from curl in state.Key.AcceptValue(value: new Vector3d(
                x: (samples.Y1.Z - samples.Y0.Z - (samples.Z1.Y - samples.Z0.Y)) * inv2eps,
                y: (samples.Z1.X - samples.Z0.X - (samples.X1.Z - samples.X0.Z)) * inv2eps,
                z: (samples.X1.Y - samples.X0.Y - (samples.Y1.X - samples.Y0.X)) * inv2eps))
            select curl);
    private static Fin<Vector3d> SampleRadialContribution(Vector3d sum, Point3d source, double scale, (Point3d Sample, Context Context, Op Key) state, Falloff falloff) {
        Vector3d r = state.Sample - source;
        return r.Length <= state.Context.Absolute.Value
            ? state.Key.AcceptValue(value: sum)
            : falloff.Weight(distance: r.Length, tolerance: state.Context.Absolute.Value, key: state.Key)
                .Bind(weight => state.Key.AcceptValue(value: sum + (scale * weight / r.Length * r)));
    }
    private static Fin<Vector3d> ClosestDirected(
        SupportSpace source, Point3d sample, BoundarySense sense, Context context, Op key,
        Func<ClosestHit, Op, Fin<(Vector3d Raw, double Scale)>> hitToScaled) =>
        from hit in source.Closest(sample: sample, key: key)
        from scaled in hitToScaled(hit, key)
        from direction in Direction.Of(value: sense.Sign * scaled.Raw, context: context, key: key)
        select direction.Value * scaled.Scale;
}

[Union]
public partial record ScalarField {
    public sealed record ConstantCase(double Value) : ScalarField;
    public sealed record DistanceCase(SupportSpace Source, BoundarySense Sense) : ScalarField;
    public sealed record PotentialCase(Seq<(Point3d Position, double Charge)> Charges, Falloff Falloff) : ScalarField;
    public sealed record DensityCase(Point3d Center, PositiveMagnitude Sigma, double Strength) : ScalarField;
    public sealed record BlendCase(Seq<ScalarField> Fields, FieldBlend Mode) : ScalarField;
    public sealed record MagnitudeCase(VectorField Source) : ScalarField;
    public sealed record DivergenceCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField;
    public sealed record LaplacianCase(ScalarField Source, PositiveMagnitude Epsilon) : ScalarField;
    public sealed record ScaledCase(ScalarField Source, double Scale) : ScalarField;
    public static ScalarField Constant(double value) => new ConstantCase(Value: value);
    public static ScalarField Distance(SupportSpace source, BoundarySense? sense = null) =>
        new DistanceCase(Source: source, Sense: sense ?? BoundarySense.Toward);
    public static ScalarField Potential(Seq<(Point3d Position, double Charge)> charges, Falloff? falloff = null) =>
        new PotentialCase(Charges: charges, Falloff: falloff ?? Falloff.Inverse);
    public static Fin<ScalarField> Density(Point3d center, double sigma, double strength, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: sigma)
            .Map(s => (ScalarField)new DensityCase(Center: center, Sigma: s, Strength: strength));
    }
    public static ScalarField Blend(Seq<ScalarField> fields, FieldBlend? blend = null) =>
        new BlendCase(Fields: fields, Mode: blend ?? FieldBlend.Sum);
    public static ScalarField Magnitude(VectorField source) => new MagnitudeCase(Source: source);
    public static Fin<ScalarField> Divergence(VectorField source, double epsilon, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from eps in op.AcceptValidated<PositiveMagnitude>(candidate: epsilon)
               select (ScalarField)new DivergenceCase(Source: active, Epsilon: eps);
    }
    public static Fin<ScalarField> Laplacian(ScalarField source, double epsilon, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from eps in op.AcceptValidated<PositiveMagnitude>(candidate: epsilon)
               select (ScalarField)new LaplacianCase(Source: active, Epsilon: eps);
    }
    public static Fin<ScalarField> Divide(ScalarField source, double divisor, Op? key = null) {
        Op op = key.OrDefault();
        return Math.Abs(value: divisor) > RhinoMath.ZeroTolerance
            ? Fin.Succ<ScalarField>(new ScaledCase(Source: source, Scale: 1.0 / divisor))
            : Fin.Fail<ScalarField>(op.InvalidInput());
    }
    public static ScalarField Zero { get; } = Constant(value: 0.0);
    public static ScalarField operator +(ScalarField left, ScalarField right) => (left, right) switch {
        (BlendCase l, BlendCase r) when l.Mode.Equals(FieldBlend.Sum) && r.Mode.Equals(FieldBlend.Sum) =>
            new BlendCase(Fields: l.Fields.Concat(r.Fields).ToSeq(), Mode: FieldBlend.Sum),
        (BlendCase l, _) when l.Mode.Equals(FieldBlend.Sum) =>
            new BlendCase(Fields: l.Fields.Add(right), Mode: FieldBlend.Sum),
        (_, BlendCase r) when r.Mode.Equals(FieldBlend.Sum) =>
            new BlendCase(Fields: Seq(left).Concat(r.Fields).ToSeq(), Mode: FieldBlend.Sum),
        _ => new BlendCase(Fields: Seq(left, right), Mode: FieldBlend.Sum),
    };
    public static ScalarField operator -(ScalarField left, ScalarField right) => left + (-right);
    public static ScalarField operator -(ScalarField field) => new ScaledCase(Source: field, Scale: -1.0);
    public static ScalarField operator *(ScalarField field, double scale) => new ScaledCase(Source: field, Scale: scale);
    public static ScalarField operator *(double scale, ScalarField field) => new ScaledCase(Source: field, Scale: scale);
    internal Fin<TOut> Project<TOut>(Point3d sample, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return from point in op.AcceptValue(value: sample)
               from value in SampleScalar(sample: point, context: context, key: op)
               from output in typeof(TOut) == typeof(double)
                   ? op.AcceptValue(value: value).Map(static v => (TOut)(object)v)
                   : Fin.Fail<TOut>(error: op.Unsupported(geometryType: typeof(ScalarField), outputType: typeof(TOut)))
               select output;
    }
    internal Fin<double> SampleScalar(Point3d sample, Context context, Op key) => Switch(
        state: (Sample: sample, Context: context, Key: key),
        constantCase: static (state, c) => state.Key.AcceptValue(value: c.Value),
        distanceCase: static (state, c) =>
            from hit in c.Source.Closest(sample: state.Sample, key: state.Key)
            from raw in c.Source.AdmitsSignedDistance(hit: hit)
                ? c.Source.SignedDistance(hit: hit, sample: state.Sample, key: state.Key)
                : hit.Distance.ToFin(Fail: state.Key.InvalidResult())
            from output in state.Key.AcceptValue(value: c.Sense.Sign * raw)
            select output,
        potentialCase: static (state, c) => c.Charges.Fold(
            initialState: Fin.Succ(0.0),
            f: (acc, charge) => acc.Bind(sum => state.Sample.DistanceTo(other: charge.Position) switch {
                double d when d <= state.Context.Absolute.Value => state.Key.AcceptValue(value: sum),
                double d => c.Falloff.Weight(distance: d, tolerance: state.Context.Absolute.Value, key: state.Key)
                    .Bind(weight => state.Key.AcceptValue(value: sum + (charge.Charge * weight))),
            })),
        densityCase: static (state, c) => state.Key.AcceptValue(value:
            c.Strength * Math.Exp(d: -(state.Sample - c.Center).SquareLength / (2.0 * c.Sigma.Value * c.Sigma.Value))),
        blendCase: static (state, c) => c.Fields.TraverseM(f => f.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)).As()
            .Bind(values => c.Mode.CombineScalar(values: values, key: state.Key)),
        magnitudeCase: static (state, c) => c.Source.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(v => state.Key.AcceptValue(value: v.Length)),
        divergenceCase: static (state, c) =>
            from samples in FieldNabla.SampleAxes(sampler: p => c.Source.SampleVector(sample: p, context: state.Context, key: state.Key), point: state.Sample, eps: c.Epsilon.Value)
            let inv2eps = 1.0 / (2.0 * c.Epsilon.Value)
            from div in state.Key.AcceptValue(value:
                (samples.X1.X - samples.X0.X + samples.Y1.Y - samples.Y0.Y + samples.Z1.Z - samples.Z0.Z) * inv2eps)
            select div,
        laplacianCase: static (state, c) =>
            from samples in FieldNabla.SampleAxes(sampler: p => c.Source.SampleScalar(sample: p, context: state.Context, key: state.Key), point: state.Sample, eps: c.Epsilon.Value)
            from center in c.Source.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            let invEpsSq = 1.0 / (c.Epsilon.Value * c.Epsilon.Value)
            from lap in state.Key.AcceptValue(value:
                (samples.X1 + samples.X0 + samples.Y1 + samples.Y0 + samples.Z1 + samples.Z0 - (6.0 * center)) * invEpsSq)
            select lap,
        scaledCase: static (state, c) => c.Source.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(v => state.Key.AcceptValue(value: c.Scale * v)));
}

internal static class FieldNabla {
    internal static Fin<(T X1, T X0, T Y1, T Y0, T Z1, T Z0)> SampleAxes<T>(Func<Point3d, Fin<T>> sampler, Point3d point, double eps) =>
        from xp in sampler(arg: point + (eps * Vector3d.XAxis))
        from xm in sampler(arg: point - (eps * Vector3d.XAxis))
        from yp in sampler(arg: point + (eps * Vector3d.YAxis))
        from ym in sampler(arg: point - (eps * Vector3d.YAxis))
        from zp in sampler(arg: point + (eps * Vector3d.ZAxis))
        from zm in sampler(arg: point - (eps * Vector3d.ZAxis))
        select (X1: xp, X0: xm, Y1: yp, Y0: ym, Z1: zp, Z0: zm);
}

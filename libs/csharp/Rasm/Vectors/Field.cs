namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class FieldBlend {
    public static readonly FieldBlend Sum = new(key: 0, scale: static _ => 1.0);
    public static readonly FieldBlend Average = new(key: 1, scale: static spans => 1.0 / spans.Count);
    internal Fin<Vector3d> Combine(Seq<Vector3d> vectors, Op key) =>
        vectors switch {
            Seq<Vector3d> values when !values.IsEmpty => key.AcceptValue(value: values.Fold(initialState: Vector3d.Zero, f: static (sum, vector) => sum + vector) * Scale(values)),
            _ => Fin.Fail<Vector3d>(error: key.InvalidResult()),
        };
    [UseDelegateFromConstructor] private partial double Scale(Seq<Vector3d> vectors);
}

[SmartEnum<int>]
public sealed partial class IntegratorKind {
    public static readonly IntegratorKind Euler = new(key: 0, step: static (field, point, h, ctx, op) =>
        from sample in field.SampleVector(sample: point, context: ctx, key: op)
        from next in op.AcceptValue(value: point + (sample * h))
        select next);
    public static readonly IntegratorKind Heun = new(key: 1, step: static (field, point, h, ctx, op) =>
        from k1 in field.SampleVector(sample: point, context: ctx, key: op)
        from predictor in op.AcceptValue(value: point + (k1 * h))
        from k2 in field.SampleVector(sample: predictor, context: ctx, key: op)
        from next in op.AcceptValue(value: point + ((k1 + k2) * (h * 0.5)))
        select next);
    public static readonly IntegratorKind RK4 = new(key: 2, step: static (field, point, h, ctx, op) =>
        from k1 in field.SampleVector(sample: point, context: ctx, key: op)
        from p2 in op.AcceptValue(value: point + (k1 * (h * 0.5)))
        from k2 in field.SampleVector(sample: p2, context: ctx, key: op)
        from p3 in op.AcceptValue(value: point + (k2 * (h * 0.5)))
        from k3 in field.SampleVector(sample: p3, context: ctx, key: op)
        from p4 in op.AcceptValue(value: point + (k3 * h))
        from k4 in field.SampleVector(sample: p4, context: ctx, key: op)
        from next in op.AcceptValue(value: point + ((k1 + (2.0 * k2) + (2.0 * k3) + k4) * (h / 6.0)))
        select next);
    [UseDelegateFromConstructor] internal partial Fin<Point3d> Step(VectorField field, Point3d point, double h, Context context, Op key);
}

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record FieldIntegrator {
    private FieldIntegrator() { }
    public sealed record FixedCase(IntegratorKind Kind) : FieldIntegrator;
    public sealed record AdaptiveCase(PositiveMagnitude Tolerance) : FieldIntegrator;
    public static FieldIntegrator Euler => new FixedCase(Kind: IntegratorKind.Euler);
    public static FieldIntegrator Heun => new FixedCase(Kind: IntegratorKind.Heun);
    public static FieldIntegrator RK4 => new FixedCase(Kind: IntegratorKind.RK4);
    public static Fin<FieldIntegrator> RK45Adaptive(double tolerance, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: tolerance)
            .Map(static value => (FieldIntegrator)new AdaptiveCase(Tolerance: value));
    }
    internal Fin<(Point3d Next, double SuggestedStep, bool Accepted)> Step(VectorField field, Point3d point, double h, Context context, Op key) => Switch(
        state: (Field: field, Point: point, H: h, Context: context, Key: key),
        fixedCase: static (s, c) => c.Kind.Step(field: s.Field, point: s.Point, h: s.H, context: s.Context, key: s.Key)
            .Map(next => (Next: next, SuggestedStep: s.H, Accepted: true)),
        adaptiveCase: static (s, c) => DormandPrinceStep(field: s.Field, point: s.Point, h: s.H, tolerance: c.Tolerance.Value, context: s.Context, key: s.Key));
    private static Fin<(Point3d Next, double SuggestedStep, bool Accepted)> DormandPrinceStep(
        VectorField field, Point3d point, double h, double tolerance, Context context, Op key) =>
        from k1 in field.SampleVector(sample: point, context: context, key: key)
        from k2 in field.SampleVector(sample: point + (k1 * (h * 0.2)), context: context, key: key)
        from k3 in field.SampleVector(sample: point + (((k1 * (3.0 / 40.0)) + (k2 * (9.0 / 40.0))) * h), context: context, key: key)
        from k4 in field.SampleVector(sample: point + (((k1 * (44.0 / 45.0)) + (k2 * (-56.0 / 15.0)) + (k3 * (32.0 / 9.0))) * h), context: context, key: key)
        from k5 in field.SampleVector(sample: point + (((k1 * (19372.0 / 6561.0)) + (k2 * (-25360.0 / 2187.0)) + (k3 * (64448.0 / 6561.0)) + (k4 * (-212.0 / 729.0))) * h), context: context, key: key)
        from k6 in field.SampleVector(sample: point + (((k1 * (9017.0 / 3168.0)) + (k2 * (-355.0 / 33.0)) + (k3 * (46732.0 / 5247.0)) + (k4 * (49.0 / 176.0)) + (k5 * (-5103.0 / 18656.0))) * h), context: context, key: key)
        let y5 = point + (((k1 * (35.0 / 384.0)) + (k3 * (500.0 / 1113.0)) + (k4 * (125.0 / 192.0)) + (k5 * (-2187.0 / 6784.0)) + (k6 * (11.0 / 84.0))) * h)
        from k7 in field.SampleVector(sample: y5, context: context, key: key)
        let y4 = point + (((k1 * (5179.0 / 57600.0)) + (k3 * (7571.0 / 16695.0)) + (k4 * (393.0 / 640.0)) + (k5 * (-92097.0 / 339200.0)) + (k6 * (187.0 / 2100.0)) + (k7 * (1.0 / 40.0))) * h)
        let err = (y5 - y4).Length
        let scale = err > RhinoMath.ZeroTolerance ? Math.Clamp(value: 0.9 * Math.Pow(x: tolerance / err, y: 0.2), min: 0.2, max: 10.0) : 10.0
        from result in err <= tolerance
            ? key.AcceptValue(value: (Next: y5, SuggestedStep: h * scale, Accepted: true))
            : key.AcceptValue(value: (Next: point, SuggestedStep: h * scale, Accepted: false))
        select result;
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
    public sealed record NormalCase(SupportSpace Source, BoundarySense Sense) : VectorField;
    public sealed record BlendCase(Seq<VectorField> Fields, FieldBlend Mode) : VectorField;
    public static VectorField Constant(Vector3d value) => new ConstantCase(Value: value);
    public static VectorField Influence(SupportSpace source, Falloff? falloff = null, BoundarySense? sense = null) =>
        new InfluenceCase(Source: source, Falloff: falloff ?? Falloff.Inverse, Sense: sense ?? BoundarySense.Toward, Radius: Option<PositiveMagnitude>.None);
    public static Fin<VectorField> Normal(SupportSpace source, BoundarySense? sense = null, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from field in active.CanClosestNormal switch {
                   true => Fin.Succ((VectorField)new NormalCase(Source: active, Sense: sense ?? BoundarySense.Toward)),
                   false => Fin.Fail<VectorField>(error: op.Unsupported(active.SourceType, typeof(Vector3d))),
               }
               select field;
    }
    public static Fin<VectorField> Shell(SupportSpace source, double radius, Falloff? falloff = null, BoundarySense? sense = null, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: radius)
            .Map(value => (VectorField)new InfluenceCase(Source: source, Falloff: falloff ?? Falloff.Constant, Sense: sense ?? BoundarySense.Toward, Radius: Some(value)));
    }
    public static VectorField Blend(Seq<VectorField> fields, FieldBlend? blend = null) =>
        new BlendCase(Fields: fields, Mode: blend ?? FieldBlend.Sum);
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
    public static VectorField Add(VectorField left, VectorField right) => left + right;
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
        normalCase: static (state, c) =>
            from vector in ClosestDirected(
                source: c.Source, sample: state.Sample, sense: c.Sense, context: state.Context, key: state.Key,
                hitToScaled: (hit, op) => from _ in guard(c.Source.AdmitsNormal(hit: hit), op.Unsupported(c.Source.SourceType, typeof(Vector3d)))
                                          from normal in hit.Normal.ToFin(Fail: op.InvalidResult())
                                          select (Raw: normal, Scale: 1.0))
            select vector,
        blendCase: static (state, c) => c.Fields.TraverseM(field => field.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)).As()
            .Bind(vectors => c.Mode.Combine(vectors: vectors, key: state.Key)));
    private static Fin<Vector3d> ClosestDirected(
        SupportSpace source, Point3d sample, BoundarySense sense, Context context, Op key,
        Func<ClosestHit, Op, Fin<(Vector3d Raw, double Scale)>> hitToScaled) =>
        from hit in source.Closest(sample: sample, key: key)
        from scaled in hitToScaled(hit, key)
        from direction in Direction.Of(value: sense.Sign * scaled.Raw, context: context, key: key)
        select direction.Value * scaled.Scale;
}

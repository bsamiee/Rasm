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
    public static readonly IntegratorKind Midpoint = new(key: 2, tableau: new ButcherTableau(
        Coupling: [[], [0.5]],
        Weights: [0.0, 1.0],
        ErrorWeights: Option<Seq<double>>.None));
    public static readonly IntegratorKind Ralston = new(key: 3, tableau: new ButcherTableau(
        Coupling: [[], [2.0 / 3.0]],
        Weights: [0.25, 0.75],
        ErrorWeights: Option<Seq<double>>.None));
    public static readonly IntegratorKind RK4 = new(key: 4, tableau: new ButcherTableau(
        Coupling: [[], [0.5], [0.0, 0.5], [0.0, 0.0, 1.0]],
        Weights: [1.0 / 6.0, 1.0 / 3.0, 1.0 / 3.0, 1.0 / 6.0],
        ErrorWeights: Option<Seq<double>>.None));
    public static readonly IntegratorKind RK38 = new(key: 5, tableau: new ButcherTableau(
        Coupling: [[], [1.0 / 3.0], [-1.0 / 3.0, 1.0], [1.0, -1.0, 1.0]],
        Weights: [1.0 / 8.0, 3.0 / 8.0, 3.0 / 8.0, 1.0 / 8.0],
        ErrorWeights: Option<Seq<double>>.None));
    public static readonly IntegratorKind BogackiShampine = new(key: 6, tableau: new ButcherTableau(
        Coupling: [[], [0.5], [0.0, 0.75], [2.0 / 9.0, 1.0 / 3.0, 4.0 / 9.0]],
        Weights: [2.0 / 9.0, 1.0 / 3.0, 4.0 / 9.0, 0.0],
        ErrorWeights: Some<Seq<double>>([7.0 / 24.0, 0.25, 1.0 / 3.0, 1.0 / 8.0])));
    public static readonly IntegratorKind CashKarp = new(key: 7, tableau: new ButcherTableau(
        Coupling: [
            [],
            [0.2],
            [3.0 / 40.0, 9.0 / 40.0],
            [0.3, -0.9, 1.2],
            [-11.0 / 54.0, 2.5, -70.0 / 27.0, 35.0 / 27.0],
            [1631.0 / 55296.0, 175.0 / 512.0, 575.0 / 13824.0, 44275.0 / 110592.0, 253.0 / 4096.0]],
        Weights: [37.0 / 378.0, 0.0, 250.0 / 621.0, 125.0 / 594.0, 0.0, 512.0 / 1771.0],
        ErrorWeights: Some<Seq<double>>([2825.0 / 27648.0, 0.0, 18575.0 / 48384.0, 13525.0 / 55296.0, 277.0 / 14336.0, 0.25])));
    public static readonly IntegratorKind DormandPrince = new(key: 8, tableau: new ButcherTableau(
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
    internal int Order => Tableau.Weights.Count;
    // PI-controller step-scale constants used by `FieldIntegrator.AdaptiveCase`. Safety factor
    // 0.9 buffers against under-prediction; order exponent 1/5 matches DormandPrince/CashKarp
    // (5(4) embedded pair); clamp range prevents runaway growth or stalling.
    internal const double AdaptiveSafetyFactor = 0.9;
    internal const double AdaptiveOrderExponent = 0.2;
    internal const double AdaptiveMinScale = 0.2;
    internal const double AdaptiveMaxScale = 10.0;
}

// Constructive solid geometry operations over signed-distance fields. Union = min, Intersect
// = max, Difference = max(a, -b). SmoothUnion is the Pythagorean blend ubiquitous in shader
// SDFs, parameterised by smoothness radius.
[SmartEnum<int>]
public sealed partial class CsgKind {
    public static readonly CsgKind Union = new(key: 0, combine: static (a, b) => Math.Min(val1: a, val2: b));
    public static readonly CsgKind Intersect = new(key: 1, combine: static (a, b) => Math.Max(val1: a, val2: b));
    public static readonly CsgKind Difference = new(key: 2, combine: static (a, b) => Math.Max(val1: a, val2: -b));
    [UseDelegateFromConstructor] internal partial double Combine(double left, double right);
}

// Compact-support radial kernels for SPH-style and RBF interpolation. All return 0 outside the support radius; all are normalised to weight(0, r) = 1 (peak at origin).
[SmartEnum<int>]
public sealed partial class KernelKind {
    public static readonly KernelKind Wendland = new(key: 0, weight: static (d, r) => {
        double q = d / r;
        double q1 = 1.0 - q;
        return d >= r ? 0.0 : q1 * q1 * q1 * q1 * (1.0 + (4.0 * q));
    });
    public static readonly KernelKind Quintic = new(key: 1, weight: static (d, r) => {
        double q1 = 1.0 - (d / r);
        return d >= r ? 0.0 : q1 * q1 * q1 * q1 * q1;
    });
    public static readonly KernelKind Cosine = new(key: 2, weight: static (d, r) =>
        d >= r ? 0.0 : 0.5 * (1.0 + Math.Cos(d: Math.PI * d / r)));
    public static readonly KernelKind Cubic = new(key: 3, weight: static (d, r) => {
        double q1 = 1.0 - (d / r);
        return d >= r ? 0.0 : q1 * q1 * q1;
    });
    public static readonly KernelKind Linear = new(key: 4, weight: static (d, r) =>
        d >= r ? 0.0 : 1.0 - (d / r));
    public static readonly KernelKind Epanechnikov = new(key: 5, weight: static (d, r) => {
        double q = d / r;
        return d >= r ? 0.0 : 1.0 - (q * q);
    });
    [UseDelegateFromConstructor] internal partial double Weight(double distance, double radius);
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
            let scale = err > RhinoMath.ZeroTolerance
                ? Math.Clamp(
                    value: IntegratorKind.AdaptiveSafetyFactor * Math.Pow(x: c.Tolerance.Value / err, y: IntegratorKind.AdaptiveOrderExponent),
                    min: IntegratorKind.AdaptiveMinScale,
                    max: IntegratorKind.AdaptiveMaxScale)
                : IntegratorKind.AdaptiveMaxScale
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
        coefficients.Zip(vectors).Fold(
            initialState: Vector3d.Zero,
            f: static (sum, pair) => sum + (pair.Item1 * pair.Item2));
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
    public sealed record KernelCase(KernelKind Kind, PositiveMagnitude Radius) : Falloff;
    public static Falloff Constant => new ConstantCase();
    public static Falloff Inverse => new InverseCase();
    public static Falloff InverseSquare => new InverseSquareCase();
    public static Fin<Falloff> Gaussian(double sigma, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: sigma).Map(static value => (Falloff)new GaussianCase(Sigma: value));
    }
    public static Fin<Falloff> Kernel(KernelKind kind, double radius, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(kind).ToFin(op.InvalidInput())
               from r in op.AcceptValidated<PositiveMagnitude>(candidate: radius)
               select (Falloff)new KernelCase(Kind: active, Radius: r);
    }
    internal Fin<double> Weight(double distance, double tolerance, Op key) => Switch(
        state: (Distance: distance, Tolerance: tolerance, Key: key),
        constantCase: static (_, _) => Fin.Succ(1.0),
        inverseCase: static (state, _) => state.Distance > state.Tolerance ? Fin.Succ(1.0 / state.Distance) : Fin.Fail<double>(state.Key.InvalidInput()),
        inverseSquareCase: static (state, _) => state.Distance > state.Tolerance ? Fin.Succ(1.0 / (state.Distance * state.Distance)) : Fin.Fail<double>(state.Key.InvalidInput()),
        gaussianCase: static (state, gaussian) => Fin.Succ(Math.Exp(-(state.Distance * state.Distance) / (2.0 * gaussian.Sigma.Value * gaussian.Sigma.Value))),
        kernelCase: static (state, kernel) => Fin.Succ(kernel.Kind.Weight(distance: state.Distance, radius: kernel.Radius.Value)));
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
    public sealed record RingCase(Point3d Center, Direction Axis, PositiveMagnitude Radius, Falloff Falloff) : VectorField;
    public sealed record HelicalCase(Point3d Anchor, Direction Axis, double Axial, double Swirl, Falloff Falloff) : VectorField;
    public sealed record BiotSavartCase(Point3d Start, Point3d End, double Current) : VectorField;
    public sealed record SaddleCase(Point3d Anchor, Plane Basis, double Strength) : VectorField;
    public sealed record CrossProductCase(VectorField Left, VectorField Right) : VectorField;
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
    public static Fin<VectorField> Ring(Point3d center, Direction axis, double radius, Falloff? falloff = null, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: radius)
            .Bind(r => (falloff ?? Falloff.Gaussian(sigma: radius / 3.0, key: op).IfFail(Falloff.Constant)) switch {
                Falloff active => Fin.Succ((VectorField)new RingCase(Center: center, Axis: axis, Radius: r, Falloff: active)),
            });
    }
    public static VectorField Helical(Point3d anchor, Direction axis, double axial, double swirl, Falloff? falloff = null) =>
        new HelicalCase(Anchor: anchor, Axis: axis, Axial: axial, Swirl: swirl, Falloff: falloff ?? Falloff.Constant);
    public static Fin<VectorField> BiotSavart(Point3d start, Point3d end, double current, Op? key = null) {
        Op op = key.OrDefault();
        return (start - end).IsTiny()
            ? Fin.Fail<VectorField>(op.InvalidInput())
            : Fin.Succ((VectorField)new BiotSavartCase(Start: start, End: end, Current: current));
    }
    public static Fin<VectorField> Saddle(Point3d anchor, Plane basis, double strength, Op? key = null) {
        Op op = key.OrDefault();
        return basis.IsValid
            ? Fin.Succ((VectorField)new SaddleCase(Anchor: anchor, Basis: basis, Strength: strength))
            : Fin.Fail<VectorField>(op.InvalidInput());
    }
    public static VectorField CrossProduct(VectorField left, VectorField right) =>
        new CrossProductCase(Left: left, Right: right);
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
            Vector3d rPerp = FieldNabla.PerpendicularComponent(r: state.Sample - c.Anchor, axis: c.Axis.Value);
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
            select curl,
        ringCase: static (state, c) => {
            Vector3d rPerp = FieldNabla.PerpendicularComponent(r: state.Sample - c.Center, axis: c.Axis.Value);
            double residual = Math.Abs(value: rPerp.Length - c.Radius.Value);
            return c.Falloff.Weight(distance: residual, tolerance: state.Context.Absolute.Value, key: state.Key)
                .Bind(weight => state.Key.AcceptValue(value: Vector3d.CrossProduct(a: c.Axis.Value, b: rPerp) * weight));
        },
        helicalCase: static (state, c) => {
            Vector3d rPerp = FieldNabla.PerpendicularComponent(r: state.Sample - c.Anchor, axis: c.Axis.Value);
            return c.Falloff.Weight(distance: rPerp.Length, tolerance: state.Context.Absolute.Value, key: state.Key)
                .Bind(weight => state.Key.AcceptValue(value: weight * ((c.Axial * c.Axis.Value) + (c.Swirl * Vector3d.CrossProduct(a: c.Axis.Value, b: rPerp)))));
        },
        biotSavartCase: static (state, c) => {
            Vector3d wire = c.End - c.Start;
            double wireLen = wire.Length;
            return wireLen < state.Context.Absolute.Value
                ? Fin.Fail<Vector3d>(state.Key.InvalidInput())
                : BiotSavartContribution(start: c.Start, end: c.End, current: c.Current, point: state.Sample, tol: state.Context.Absolute.Value)
                    .Match(Some: value => state.Key.AcceptValue(value: value), None: () => state.Key.AcceptValue(value: Vector3d.Zero));
        },
        saddleCase: static (state, c) => {
            Vector3d r = state.Sample - c.Anchor;
            return Vector3d.Decompose(v: r, a: c.Basis.XAxis, b: c.Basis.YAxis, x: out double u, y: out double v)
                ? state.Key.AcceptValue(value: c.Strength * ((u * c.Basis.XAxis) - (v * c.Basis.YAxis)))
                : Fin.Fail<Vector3d>(state.Key.InvalidResult());
        },
        crossProductCase: static (state, c) =>
            from left in c.Left.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)
            from right in c.Right.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)
            from output in state.Key.AcceptValue(value: Vector3d.CrossProduct(a: left, b: right))
            select output);
    // Biot-Savart law for a finite straight current segment from `start` to `end` carrying
    // `current` amperes. Returns the magnetic field vector at `point`, perpendicular to both
    // the wire and the foot-of-perpendicular vector, scaled by the angular geometry.
    private static Option<Vector3d> BiotSavartContribution(Point3d start, Point3d end, double current, Point3d point, double tol) {
        Vector3d wire = end - start;
        double wireLen = wire.Length;
        Vector3d t = wire / wireLen;
        Vector3d r1 = point - start;
        Vector3d r2 = point - end;
        Vector3d perpVec = FieldNabla.PerpendicularComponent(r: r1, axis: t);
        double R = perpVec.Length;
        double angularFactor = (r2 * t / r2.Length) - (r1 * t / r1.Length);
        double prefactor = current / (4.0 * Math.PI * R);
        return R < tol
            ? None
            : r1.Length < tol || r2.Length < tol
                ? Some(Vector3d.Zero)
                : Some(Vector3d.CrossProduct(a: t, b: perpVec / R) * prefactor * angularFactor);
    }
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
    public sealed record WorleyCase(Seq<Point3d> Seeds, int Order) : ScalarField;
    public sealed record MorseCase(Point3d Center, PositiveMagnitude Depth, PositiveMagnitude Width) : ScalarField;
    public sealed record MollifierCase(Point3d Center, PositiveMagnitude Radius) : ScalarField;
    public sealed record PowerCase(ScalarField Source, double Exponent) : ScalarField;
    public sealed record CsgCase(ScalarField Left, ScalarField Right, CsgKind Op) : ScalarField;
    public sealed record PeriodicCase(ScalarField Source, Vector3d Period) : ScalarField;
    public sealed record StrainMagnitudeCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField;
    public sealed record ClampCase(ScalarField Source, double Minimum, double Maximum) : ScalarField;
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
    public static Fin<ScalarField> Worley(Seq<Point3d> seeds, int order = 1, Op? key = null) {
        Op op = key.OrDefault();
        return seeds.Count >= order && order >= 1
            ? Fin.Succ((ScalarField)new WorleyCase(Seeds: seeds, Order: order))
            : Fin.Fail<ScalarField>(op.InvalidInput());
    }
    public static Fin<ScalarField> Morse(Point3d center, double depth, double width, Op? key = null) {
        Op op = key.OrDefault();
        return from d in op.AcceptValidated<PositiveMagnitude>(candidate: depth)
               from w in op.AcceptValidated<PositiveMagnitude>(candidate: width)
               select (ScalarField)new MorseCase(Center: center, Depth: d, Width: w);
    }
    public static Fin<ScalarField> Mollifier(Point3d center, double radius, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: radius)
            .Map(r => (ScalarField)new MollifierCase(Center: center, Radius: r));
    }
    public static Fin<ScalarField> Power(ScalarField source, double exponent, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from _ in guard(RhinoMath.IsValidDouble(x: exponent), op.InvalidInput())
               select (ScalarField)new PowerCase(Source: active, Exponent: exponent);
    }
    public static ScalarField Union(ScalarField left, ScalarField right) =>
        new CsgCase(Left: left, Right: right, Op: CsgKind.Union);
    public static ScalarField Intersect(ScalarField left, ScalarField right) =>
        new CsgCase(Left: left, Right: right, Op: CsgKind.Intersect);
    public static ScalarField Difference(ScalarField left, ScalarField right) =>
        new CsgCase(Left: left, Right: right, Op: CsgKind.Difference);
    public static Fin<ScalarField> Periodic(ScalarField source, Vector3d period, Op? key = null) {
        Op op = key.OrDefault();
        return !period.IsValid || period.IsTiny()
            ? Fin.Fail<ScalarField>(op.InvalidInput())
            : Fin.Succ((ScalarField)new PeriodicCase(Source: source, Period: period));
    }
    public static Fin<ScalarField> StrainMagnitude(VectorField source, double epsilon, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from eps in op.AcceptValidated<PositiveMagnitude>(candidate: epsilon)
               select (ScalarField)new StrainMagnitudeCase(Source: active, Epsilon: eps);
    }
    public static Fin<ScalarField> Clamp(ScalarField source, double minimum, double maximum, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from _ in guard(RhinoMath.IsValidDouble(x: minimum) && RhinoMath.IsValidDouble(x: maximum) && minimum <= maximum, op.InvalidInput())
               select (ScalarField)new ClampCase(Source: active, Minimum: minimum, Maximum: maximum);
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
            .Bind(v => state.Key.AcceptValue(value: c.Scale * v)),
        worleyCase: static (state, c) =>
            toSeq(c.Seeds.Map(seed => state.Sample.DistanceTo(other: seed)).OrderBy(static d => d).AsIterable()) switch {
                Seq<double> sorted when c.Order - 1 < sorted.Count => state.Key.AcceptValue(value: sorted[c.Order - 1]),
                _ => Fin.Fail<double>(state.Key.InvalidResult()),
            },
        morseCase: static (state, c) =>
            from r in state.Key.AcceptValue(value: state.Sample.DistanceTo(other: c.Center))
            let expTerm = Math.Exp(d: -r / c.Width.Value)
            from output in state.Key.AcceptValue(value: c.Depth.Value * (1.0 - expTerm) * (1.0 - expTerm))
            select output,
        mollifierCase: static (state, c) =>
            from r in state.Key.AcceptValue(value: state.Sample.DistanceTo(other: c.Center))
            let q = r / c.Radius.Value
            from output in state.Key.AcceptValue(value: q >= 1.0 ? 0.0 : Math.Exp(d: -1.0 / (1.0 - (q * q))))
            select output,
        powerCase: static (state, c) => c.Source.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(v => state.Key.AcceptValue(value: Math.Pow(x: v, y: c.Exponent))),
        csgCase: static (state, c) =>
            from leftValue in c.Left.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            from rightValue in c.Right.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            from output in state.Key.AcceptValue(value: c.Op.Combine(left: leftValue, right: rightValue))
            select output,
        periodicCase: static (state, c) => c.Source.SampleScalar(
            sample: FieldNabla.ToroidalWrap(sample: state.Sample, period: c.Period),
            context: state.Context, key: state.Key),
        strainMagnitudeCase: static (state, c) =>
            from samples in FieldNabla.SampleAxes(sampler: p => c.Source.SampleVector(sample: p, context: state.Context, key: state.Key), point: state.Sample, eps: c.Epsilon.Value)
            let inv2eps = 1.0 / (2.0 * c.Epsilon.Value)
            let sxx = (samples.X1.X - samples.X0.X) * inv2eps
            let syy = (samples.Y1.Y - samples.Y0.Y) * inv2eps
            let szz = (samples.Z1.Z - samples.Z0.Z) * inv2eps
            let sxy = 0.5 * (samples.Y1.X - samples.Y0.X + samples.X1.Y - samples.X0.Y) * inv2eps
            let sxz = 0.5 * (samples.Z1.X - samples.Z0.X + samples.X1.Z - samples.X0.Z) * inv2eps
            let syz = 0.5 * (samples.Z1.Y - samples.Z0.Y + samples.Y1.Z - samples.Y0.Z) * inv2eps
            from output in state.Key.AcceptValue(value: Math.Sqrt(d: (sxx * sxx) + (syy * syy) + (szz * szz) + (2.0 * ((sxy * sxy) + (sxz * sxz) + (syz * syz)))))
            select output,
        clampCase: static (state, c) => c.Source.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(v => state.Key.AcceptValue(value: Math.Clamp(value: v, min: c.Minimum, max: c.Maximum))));
}

internal static class FieldNabla {
    // Component of `r` perpendicular to a unit axis: r - (r . axis) * axis. Used by every axis-relative field case (Vortex/Ring/Helical/BiotSavart) to extract the in-plane part.
    internal static Vector3d PerpendicularComponent(Vector3d r, Vector3d axis) => r - (r * axis * axis);
    // Wrap a sample point into the fundamental period parallelepiped via component-wise round-half-to-even reduction. Used by `PeriodicCase` to fold space into a single cell.
    internal static Point3d ToroidalWrap(Point3d sample, Vector3d period) =>
        new(x: sample.X - (Math.Floor(d: (sample.X / period.X) + 0.5) * period.X),
            y: sample.Y - (Math.Floor(d: (sample.Y / period.Y) + 0.5) * period.Y),
            z: sample.Z - (Math.Floor(d: (sample.Z / period.Z) + 0.5) * period.Z));
    internal static Fin<(T X1, T X0, T Y1, T Y0, T Z1, T Z0)> SampleAxes<T>(Func<Point3d, Fin<T>> sampler, Point3d point, double eps) =>
        from xp in sampler(arg: point + (eps * Vector3d.XAxis))
        from xm in sampler(arg: point - (eps * Vector3d.XAxis))
        from yp in sampler(arg: point + (eps * Vector3d.YAxis))
        from ym in sampler(arg: point - (eps * Vector3d.YAxis))
        from zp in sampler(arg: point + (eps * Vector3d.ZAxis))
        from zm in sampler(arg: point - (eps * Vector3d.ZAxis))
        select (X1: xp, X0: xm, Y1: yp, Y0: ym, Z1: zp, Z0: zm);
}

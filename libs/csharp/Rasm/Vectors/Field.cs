using System.Numerics.Tensors;
using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[Union]
public abstract partial record BlendKind {
    private BlendKind() { }
    public sealed record HardCase : BlendKind { internal HardCase() { } }
    public sealed record PolynomialCase : BlendKind { internal PolynomialCase(PositiveMagnitude K) => this.K = K; public PositiveMagnitude K { get; } }
    public sealed record ExponentialCase : BlendKind { internal ExponentialCase(PositiveMagnitude K) => this.K = K; public PositiveMagnitude K { get; } }
    public sealed record RootCase : BlendKind { internal RootCase(PositiveMagnitude K) => this.K = K; public PositiveMagnitude K { get; } }
    public sealed record CubicCase : BlendKind { internal CubicCase(PositiveMagnitude K) => this.K = K; public PositiveMagnitude K { get; } }
    public sealed record ChamferCase : BlendKind { internal ChamferCase(PositiveMagnitude K) => this.K = K; public PositiveMagnitude K { get; } }
    public sealed record GrooveCase : BlendKind { internal GrooveCase(PositiveMagnitude K, PositiveMagnitude D) { this.K = K; this.D = D; } public PositiveMagnitude K { get; } public PositiveMagnitude D { get; } }
    public sealed record RoundCase : BlendKind { internal RoundCase(PositiveMagnitude R) => this.R = R; public PositiveMagnitude R { get; } }
    public static BlendKind Hard => new HardCase();
    public static Fin<BlendKind> Polynomial(double k, Op? key = null) => FieldNabla.WithPositive(candidate: k, make: static v => (BlendKind)new PolynomialCase(K: v), key: key);
    public static Fin<BlendKind> Exponential(double k, Op? key = null) => FieldNabla.WithPositive(candidate: k, make: static v => (BlendKind)new ExponentialCase(K: v), key: key);
    public static Fin<BlendKind> Root(double k, Op? key = null) => FieldNabla.WithPositive(candidate: k, make: static v => (BlendKind)new RootCase(K: v), key: key);
    public static Fin<BlendKind> Cubic(double k, Op? key = null) => FieldNabla.WithPositive(candidate: k, make: static v => (BlendKind)new CubicCase(K: v), key: key);
    public static Fin<BlendKind> Chamfer(double k, Op? key = null) => FieldNabla.WithPositive(candidate: k, make: static v => (BlendKind)new ChamferCase(K: v), key: key);
    public static Fin<BlendKind> Groove(double k, double d, Op? key = null) =>
        FieldNabla.WithPositivePair(left: k, right: d, make: static (kk, dd) => (BlendKind)new GrooveCase(K: kk, D: dd), key: key);
    public static Fin<BlendKind> Round(double r, Op? key = null) => FieldNabla.WithPositive(candidate: r, make: static v => (BlendKind)new RoundCase(R: v), key: key);
    internal double Smin(double a, double b) => Switch(
        state: (A: a, B: b),
        hardCase: static (s, _) => Math.Min(val1: s.A, val2: s.B),
        polynomialCase: static (s, c) => {
            double h = Math.Max(val1: c.K.Value - Math.Abs(value: s.A - s.B), val2: 0.0) / c.K.Value;
            return Math.Min(val1: s.A, val2: s.B) - (h * h * h * c.K.Value * (1.0 / 6.0));
        },
        exponentialCase: static (s, c) => {
            double ax = -c.K.Value * s.A;
            double bx = -c.K.Value * s.B;
            double m = Math.Max(val1: ax, val2: bx);
            return -(m + Math.Log(d: Math.Exp(d: ax - m) + Math.Exp(d: bx - m))) / c.K.Value;
        },
        rootCase: static (s, c) => {
            double h = Math.Max(val1: c.K.Value - Math.Abs(value: s.A - s.B), val2: 0.0);
            return Math.Min(val1: s.A, val2: s.B) - (h * h * 0.25 / c.K.Value);
        },
        cubicCase: static (s, c) => {
            double h = Math.Max(val1: c.K.Value - Math.Abs(value: s.A - s.B), val2: 0.0) / c.K.Value;
            return Math.Min(val1: s.A, val2: s.B) - (h * h * c.K.Value * 0.25);
        },
        chamferCase: static (s, c) => Math.Min(val1: Math.Min(val1: s.A, val2: s.B), val2: (s.A + s.B - c.K.Value) * 0.7071067811865475),
        grooveCase: static (s, c) => Math.Max(val1: s.A, val2: Math.Min(val1: c.D.Value, val2: Math.Min(val1: s.A - c.K.Value, val2: s.B - c.K.Value))),
        roundCase: static (s, c) => {
            double ax = Math.Max(val1: c.R.Value - s.A, val2: 0.0);
            double bx = Math.Max(val1: c.R.Value - s.B, val2: 0.0);
            return Math.Max(val1: c.R.Value, val2: Math.Min(val1: s.A, val2: s.B)) - Math.Sqrt(d: (ax * ax) + (bx * bx));
        });
    internal double Erode(double leftLip, double rightLip) {
        double dominant = Math.Max(val1: leftLip, val2: rightLip);
        return Switch(
            state: dominant,
            hardCase: static (d, _) => d,
            polynomialCase: static (d, _) => d * 1.25,
            exponentialCase: static (d, _) => d * 1.15,
            rootCase: static (d, _) => d * 1.10,
            cubicCase: static (d, _) => d * 1.30,
            chamferCase: static (d, _) => d * 1.50,
            grooveCase: static (d, _) => d * 1.40,
            roundCase: static (d, _) => d * 1.20);
    }
}

[Union]
public abstract partial record BouncePolicy {
    private BouncePolicy() { }
    public sealed record ReflectCase : BouncePolicy;
    public sealed record RefractCase(PositiveMagnitude EtaIncident, PositiveMagnitude EtaTransmitted) : BouncePolicy;
    public static BouncePolicy Reflect => new ReflectCase();
    public static Fin<BouncePolicy> Refract(double etaIncident, double etaTransmitted, Op? key = null) =>
        FieldNabla.WithPositivePair(left: etaIncident, right: etaTransmitted, make: static (incident, transmitted) => (BouncePolicy)new RefractCase(EtaIncident: incident, EtaTransmitted: transmitted), key: key);
    internal Fin<Direction> Apply(Direction incident, Direction normal, Op key) => Switch(
        state: (Incident: incident, Normal: normal, Key: key),
        reflectCase: static (state, _) => Fin.Succ(state.Incident.Reflect(normal: state.Normal)),
        refractCase: static (state, refract) => Direction.Refract(
            incident: state.Incident, normal: state.Normal,
            etaIncident: refract.EtaIncident.Value, etaTransmitted: refract.EtaTransmitted.Value, key: state.Key));
}

[SmartEnum<int>]
public sealed partial class CsgKind {
    public static readonly CsgKind Union = new(key: 0, combine: static (a, b, blend) => blend.Smin(a: a, b: b));
    public static readonly CsgKind Intersect = new(key: 1, combine: static (a, b, blend) => -blend.Smin(a: -a, b: -b));
    public static readonly CsgKind Difference = new(key: 2, combine: static (a, b, blend) => -blend.Smin(a: -a, b: b));
    [UseDelegateFromConstructor] internal partial double Combine(double left, double right, BlendKind blend);
}

[Union]
public abstract partial record Falloff {
    private Falloff() { }
    public sealed record ConstantCase : Falloff { internal ConstantCase() { } }
    public sealed record InverseCase : Falloff { internal InverseCase() { } }
    public sealed record InverseSquareCase : Falloff { internal InverseSquareCase() { } }
    public sealed record GaussianCase : Falloff { internal GaussianCase(PositiveMagnitude Spread) => this.Spread = Spread; public PositiveMagnitude Spread { get; } }
    public sealed record KernelCase : Falloff { internal KernelCase(KernelKind Kind, PositiveMagnitude Radius) { this.Kind = Kind; this.Radius = Radius; } public KernelKind Kind { get; } public PositiveMagnitude Radius { get; } }
    public sealed record AnisotropicKernelCase : Falloff { internal AnisotropicKernelCase(KernelKind Kind, TensorField Metric, PositiveMagnitude Radius) { this.Kind = Kind; this.Metric = Metric; this.Radius = Radius; } public KernelKind Kind { get; } public TensorField Metric { get; } public PositiveMagnitude Radius { get; } }
    public static Falloff Constant => new ConstantCase();
    public static Falloff Inverse => new InverseCase();
    public static Falloff InverseSquare => new InverseSquareCase();
    public static Fin<Falloff> Gaussian(double spread, Op? key = null) =>
        FieldNabla.WithPositive(candidate: spread, make: static value => (Falloff)new GaussianCase(Spread: value), key: key);
    public static Fin<Falloff> Kernel(KernelKind kind, double radius, Op? key = null) =>
        from active in Optional(kind).ToFin(key.OrDefault().InvalidInput()) from r in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius) select (Falloff)new KernelCase(Kind: active, Radius: r);
    public static Fin<Falloff> AnisotropicKernel(KernelKind kind, TensorField metric, double radius, Op? key = null) =>
        from active in Optional(kind).ToFin(key.OrDefault().InvalidInput()) from tensor in Optional(metric).ToFin(key.OrDefault().InvalidInput()) from r in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius) select (Falloff)new AnisotropicKernelCase(Kind: active, Metric: tensor, Radius: r);
    internal Fin<double> Weight(double distance, double tolerance, Op key) =>
        WeightCore(distance: distance, distanceSquared: distance * distance, metric: Option<(Vector3d Offset, Point3d Sample, Context Context)>.None, tolerance: tolerance, key: key);
    internal Fin<double> Weight(Vector3d offset, Point3d sample, Context context, double tolerance, Op key) =>
        WeightCore(distance: offset.Length, distanceSquared: offset.SquareLength, metric: Some((Offset: offset, Sample: sample, Context: context)), tolerance: tolerance, key: key);
    internal Fin<double> Weight(Vector3d offset, double tolerance, Op key) =>
        WeightCore(distance: offset.Length, distanceSquared: offset.SquareLength, metric: Option<(Vector3d Offset, Point3d Sample, Context Context)>.None, tolerance: tolerance, key: key);
    private Fin<double> WeightCore(double distance, double distanceSquared, Option<(Vector3d Offset, Point3d Sample, Context Context)> metric, double tolerance, Op key) =>
        FieldNabla.FalloffInput(distance: distance, distanceSquared: distanceSquared, tolerance: tolerance, key: key).Bind(_ => Switch(
            state: (Distance: distance, DistanceSquared: distanceSquared, Metric: metric, Tolerance: tolerance, Key: key),
            constantCase: static (_, _) => Fin.Succ(1.0),
            inverseCase: static (s, _) => s.Distance > s.Tolerance ? Fin.Succ(1.0 / s.Distance) : Fin.Fail<double>(s.Key.InvalidInput()),
            inverseSquareCase: static (s, _) => s.Distance > s.Tolerance ? Fin.Succ(1.0 / s.DistanceSquared) : Fin.Fail<double>(s.Key.InvalidInput()),
            gaussianCase: static (s, g) => Fin.Succ(Math.Exp(-s.DistanceSquared / (2.0 * g.Spread.Value * g.Spread.Value))),
            kernelCase: static (s, k) => k.Kind.Profile(distance: s.Distance, radius: k.Radius.Value, key: s.Key).Map(static p => p.Value),
            anisotropicKernelCase: static (s, k) =>
                from m in s.Metric.ToFin(s.Key.Unsupported(geometryType: typeof(AnisotropicKernelCase), outputType: typeof(double)))
                from tensor in k.Metric.SampleTensor(sample: m.Sample, context: m.Context, key: s.Key)
                from _ in tensor.Dimension.Value == 3 ? tensor.DecomposeCholesky(key: s.Key).Map(static _ => unit) : Fin.Fail<Unit>(s.Key.InvalidInput())
                from metricDistance in (m.Offset.X, m.Offset.Y, m.Offset.Z) switch {
                    (double x, double y, double z) when
                        (x * ((tensor.At(i: 0, j: 0) * x) + (tensor.At(i: 0, j: 1) * y) + (tensor.At(i: 0, j: 2) * z))) +
                        (y * ((tensor.At(i: 1, j: 0) * x) + (tensor.At(i: 1, j: 1) * y) + (tensor.At(i: 1, j: 2) * z))) +
                        (z * ((tensor.At(i: 2, j: 0) * x) + (tensor.At(i: 2, j: 1) * y) + (tensor.At(i: 2, j: 2) * z))) is double quadratic
                        && RhinoMath.IsValidDouble(x: quadratic) && quadratic > 0.0 => s.Key.AcceptValue(value: Math.Sqrt(d: quadratic)),
                    _ => Fin.Fail<double>(s.Key.InvalidResult()),
                }
                from profile in k.Kind.Profile(distance: metricDistance, radius: k.Radius.Value, key: s.Key)
                select profile.Value));
}

[SmartEnum<int>]
public sealed partial class FieldBlend {
    public static readonly FieldBlend Sum = new(key: 0, scale: static _ => 1.0);
    public static readonly FieldBlend Average = new(key: 1, scale: static count => 1.0 / count);
    [UseDelegateFromConstructor] private partial double Scale(int count);
    internal Fin<Vector3d> Combine(Seq<Vector3d> vectors, Op key) => CombineCore(values: vectors, zero: Vector3d.Zero, add: static (sum, v) => sum + v, scale: static (sum, factor) => sum * factor, key: key);
    internal Fin<double> CombineScalar(Seq<double> values, Op key) => CombineCore(values: values, zero: 0.0, add: static (sum, v) => sum + v, scale: static (sum, factor) => sum * factor, key: key);
    private Fin<T> CombineCore<T>(Seq<T> values, T zero, Func<T, T, T> add, Func<T, double, T> scale, Op key) =>
        from _ in guard(!values.IsEmpty, key.InvalidResult())
        from value in key.AcceptValue(value: scale(arg1: values.Fold(initialState: zero, f: add), arg2: Scale(count: values.Count)))
        select value;
}

[SmartEnum<int>]
public sealed partial class IsoSurfaceStatus {
    public static readonly IsoSurfaceStatus NativeValid = new(key: 0);
    public static readonly IsoSurfaceStatus EvaluatorFailure = new(key: 1);
    public static readonly IsoSurfaceStatus NativeReturnedNull = new(key: 2);
    public static readonly IsoSurfaceStatus NativeInvalidMesh = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class KernelProfileStatus {
    public static readonly KernelProfileStatus Smooth = new(key: 0);
    public static readonly KernelProfileStatus SupportBoundary = new(key: 1);
    public static readonly KernelProfileStatus NonsmoothOrigin = new(key: 2);
    public static readonly KernelProfileStatus OutsideSupport = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class KernelKind {
    public static readonly KernelKind Wendland = new(key: 0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => Pow1(q: q, power: 4) * (1.0 + (4.0 * q)), first: static (q, r) => ((-20.0 * q) + (60.0 * q * q) - (60.0 * q * q * q) + (20.0 * q * q * q * q)) / r, second: static (q, r) => (-20.0 + (120.0 * q) - (180.0 * q * q) + (80.0 * q * q * q)) / (r * r)));
    public static readonly KernelKind Quintic = new(key: 1, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => Pow1(q: q, power: 5), first: static (q, r) => -5.0 * Pow1(q: q, power: 4) / r, second: static (q, r) => 20.0 * Pow1(q: q, power: 3) / (r * r)));
    public static readonly KernelKind Cosine = new(key: 2, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => 0.5 * (1.0 + Math.Cos(d: Math.PI * q)), first: static (q, r) => -0.5 * Math.PI * Math.Sin(a: Math.PI * q) / r, second: static (q, r) => -0.5 * Math.PI * Math.PI * Math.Cos(d: Math.PI * q) / (r * r)));
    public static readonly KernelKind Cubic = new(key: 3, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => Pow1(q: q, power: 3), first: static (q, r) => -3.0 * Pow1(q: q, power: 2) / r, second: static (q, r) => 6.0 * (1.0 - q) / (r * r)));
    public static readonly KernelKind Linear = new(key: 4, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => 1.0 - q, first: static (_, r) => -1.0 / r, second: static (_, _) => 0.0));
    public static readonly KernelKind Epanechnikov = new(key: 5, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => 1.0 - (q * q), first: static (q, r) => -2.0 * q / r, second: static (_, r) => -2.0 / (r * r)));
    [UseDelegateFromConstructor] private partial KernelProfile Evaluate(double distance, double radius);
    internal Fin<KernelProfile> Profile(double distance, double radius, Op key) =>
        from _ in FieldNabla.KernelInput(distance: distance, radius: radius, key: key)
        from profile in Evaluate(distance: distance, radius: radius) switch {
            KernelProfile p when p.IsValid => Fin.Succ(p),
            _ => Fin.Fail<KernelProfile>(key.InvalidResult()),
        }
        select profile;
    internal double Weight(double distance, double radius) => Evaluate(distance: distance, radius: radius).Value;
    private static double Pow1(double q, int power) => Math.Pow(x: 1.0 - q, y: power);
    private static KernelProfile SupportProfile(double distance, double radius, bool nonsmoothAtOrigin, Func<double, double, double> value, Func<double, double, double> first, Func<double, double, double> second) {
        double q = distance / radius;
        return distance > radius
            ? new KernelProfile(Value: 0.0, FirstDerivative: 0.0, SecondDerivative: 0.0, Status: KernelProfileStatus.OutsideSupport)
            : Math.Abs(value: distance - radius) <= RhinoMath.SqrtEpsilon
                ? new KernelProfile(Value: 0.0, FirstDerivative: 0.0, SecondDerivative: 0.0, Status: KernelProfileStatus.SupportBoundary)
                : new KernelProfile(Value: value(arg1: q, arg2: radius), FirstDerivative: first(arg1: q, arg2: radius), SecondDerivative: second(arg1: q, arg2: radius), Status: nonsmoothAtOrigin && distance <= RhinoMath.SqrtEpsilon ? KernelProfileStatus.NonsmoothOrigin : KernelProfileStatus.Smooth);
    }
}

[SmartEnum<int>]
public sealed partial class NoiseKind {
    public static readonly NoiseKind Perlin = new(key: 0, raisesCaution: true, sample: static (p, seed, freq) => FieldNoise.PerlinAt(point: p, seed: seed, frequency: freq));
    public static readonly NoiseKind Simplex = new(key: 1, raisesCaution: false, sample: static (p, seed, freq) => FieldNoise.SkewedSimplexAt(point: p, seed: seed, frequency: freq, smooth: false));
    public static readonly NoiseKind SmoothSimplex = new(key: 2, raisesCaution: false, sample: static (p, seed, freq) => FieldNoise.SkewedSimplexAt(point: p, seed: seed, frequency: freq, smooth: true));
    public static readonly NoiseKind Worley = new(key: 3, raisesCaution: false, sample: static (p, seed, freq) => FieldNoise.WorleyAt(point: p, seed: seed, frequency: freq));
    public bool RaisesCaution { get; }
    [UseDelegateFromConstructor] internal partial double Sample(Point3d point, int seed, double frequency);
}

[SmartEnum<int>]
public sealed partial class ProfileExtrusionFeature {
    public static readonly ProfileExtrusionFeature Interior = new(key: 0);
    public static readonly ProfileExtrusionFeature ProfileBoundary = new(key: 1);
    public static readonly ProfileExtrusionFeature Cap = new(key: 2);
    public static readonly ProfileExtrusionFeature Rim = new(key: 3);
}

[Union]
public abstract partial record RayPolicy {
    private RayPolicy() { }
    public sealed record InfiniteCase(BoundarySense Sense) : RayPolicy;
    public sealed record SegmentCase(BoundarySense Sense, PositiveMagnitude Length) : RayPolicy;
    public static RayPolicy Forward => new InfiniteCase(Sense: BoundarySense.Toward);
    public static RayPolicy Reverse => new InfiniteCase(Sense: BoundarySense.Away);
    public static Fin<RayPolicy> Segment(double length, BoundarySense? sense = null, Op? key = null) =>
        FieldNabla.WithPositive(candidate: length, make: value => (RayPolicy)new SegmentCase(Sense: sense ?? BoundarySense.Toward, Length: value), key: key);
    internal Fin<TOut> Project<TOut>(Point3d origin, Direction direction, Context context, Op key) =>
        from point in key.AcceptValue(value: origin)
        let policy = Switch(
            state: direction.Value,
            infiniteCase: static (value, c) => (Vector: value * c.Sense.Sign, Length: Option<PositiveMagnitude>.None),
            segmentCase: static (value, c) => (Vector: value * c.Sense.Sign, Length: Some(c.Length)))
        from output in typeof(TOut) switch {
            Type t when t == typeof(Ray3d) => key.AcceptValue(value: new Ray3d(position: point, direction: policy.Vector)).Map(static value => (TOut)(object)value),
            Type t when t == typeof(Plane) => FieldNabla.Plane(basis: new Plane(origin: point, normal: policy.Vector), key: key).Map(static value => (TOut)(object)value),
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

[SmartEnum<int>]
public sealed partial class ReconstructionFailureKind { public static readonly ReconstructionFailureKind UnsupportedMode = new(key: 0); }

[SmartEnum<int>]
public sealed partial class ReconstructionMode {
    public static readonly ReconstructionMode RbfInterpolation = new(key: 0, executable: true, requiresNormals: false, requiresSparseSystem: false, polynomialDegree: 0);
    public static readonly ReconstructionMode RbfApproximation = new(key: 1, executable: true, requiresNormals: false, requiresSparseSystem: false, polynomialDegree: 0);
    public static readonly ReconstructionMode MovingLeastSquares = new(key: 2, executable: true, requiresNormals: true, requiresSparseSystem: false, polynomialDegree: 1);
    public static readonly ReconstructionMode LevinMovingLeastSquares = new(key: 3, executable: false, requiresNormals: true, requiresSparseSystem: false, polynomialDegree: 2);
    public static readonly ReconstructionMode AlgebraicPointSetSurfaces = new(key: 4, executable: false, requiresNormals: true, requiresSparseSystem: false, polynomialDegree: 2);
    public static readonly ReconstructionMode Poisson = new(key: 5, executable: false, requiresNormals: true, requiresSparseSystem: true, polynomialDegree: 0);
    public static readonly ReconstructionMode ScreenedPoisson = new(key: 6, executable: false, requiresNormals: true, requiresSparseSystem: true, polynomialDegree: 0);
    public bool Executable { get; }
    public bool RequiresNormals { get; }
    public bool RequiresSparseSystem { get; }
    public int PolynomialDegree { get; }
}

[SmartEnum<int>]
public sealed partial class ReconstructionStatus {
    public static readonly ReconstructionStatus ExactInterpolation = new(key: 0);
    public static readonly ReconstructionStatus ApproximateSdf = new(key: 1);
}

[Union]
public abstract partial record ScalarField {
    private ScalarField() { }
    public sealed record ConstantCase : ScalarField { internal ConstantCase(double Value) => this.Value = Value; public double Value { get; } }
    public sealed record BlendCase : ScalarField { internal BlendCase(Seq<ScalarField> Fields, FieldBlend Mode) { this.Fields = Fields; this.Mode = Mode; } public Seq<ScalarField> Fields { get; } public FieldBlend Mode { get; } }
    public sealed record ScaledCase : ScalarField { internal ScaledCase(ScalarField Source, double Scale) { this.Source = Source; this.Scale = Scale; } public ScalarField Source { get; } public double Scale { get; } }
    public sealed record DistanceCase : ScalarField { internal DistanceCase(SupportSpace Source, BoundarySense Sense) { this.Source = Source; this.Sense = Sense; } public SupportSpace Source { get; } public BoundarySense Sense { get; } }
    public sealed record PotentialCase : ScalarField { internal PotentialCase(Seq<(Point3d Position, double Charge)> Charges, Falloff Falloff) { this.Charges = Charges; this.Falloff = Falloff; } public Seq<(Point3d Position, double Charge)> Charges { get; } public Falloff Falloff { get; } }
    public sealed record DensityCase : ScalarField { internal DensityCase(Point3d Center, PositiveMagnitude Spread, double Strength) { this.Center = Center; this.Spread = Spread; this.Strength = Strength; } public Point3d Center { get; } public PositiveMagnitude Spread { get; } public double Strength { get; } }
    public sealed record MagnitudeCase : ScalarField { internal MagnitudeCase(VectorField Source) => this.Source = Source; public VectorField Source { get; } }
    public sealed record DivergenceCase : ScalarField { internal DivergenceCase(VectorField Source, PositiveMagnitude Epsilon) { this.Source = Source; this.Epsilon = Epsilon; } public VectorField Source { get; } public PositiveMagnitude Epsilon { get; } }
    public sealed record LaplacianCase : ScalarField { internal LaplacianCase(ScalarField Source, PositiveMagnitude Epsilon) { this.Source = Source; this.Epsilon = Epsilon; } public ScalarField Source { get; } public PositiveMagnitude Epsilon { get; } }
    public sealed record StrainMagnitudeCase : ScalarField { internal StrainMagnitudeCase(VectorField Source, PositiveMagnitude Epsilon) { this.Source = Source; this.Epsilon = Epsilon; } public VectorField Source { get; } public PositiveMagnitude Epsilon { get; } }
    public sealed record WorleyCase : ScalarField { internal WorleyCase(Seq<Point3d> Seeds, int Order) { this.Seeds = Seeds; this.Order = Order; } public Seq<Point3d> Seeds { get; } public int Order { get; } }
    public sealed record MorseCase : ScalarField { internal MorseCase(Point3d Center, PositiveMagnitude Depth, PositiveMagnitude Width) { this.Center = Center; this.Depth = Depth; this.Width = Width; } public Point3d Center { get; } public PositiveMagnitude Depth { get; } public PositiveMagnitude Width { get; } }
    public sealed record MollifierCase : ScalarField { internal MollifierCase(Point3d Center, PositiveMagnitude Radius) { this.Center = Center; this.Radius = Radius; } public Point3d Center { get; } public PositiveMagnitude Radius { get; } }
    public sealed record NoiseCase : ScalarField { internal NoiseCase(NoiseKind Kind, int Seed, int Octaves, double Persistence, double Lacunarity, double Frequency) { this.Kind = Kind; this.Seed = Seed; this.Octaves = Octaves; this.Persistence = Persistence; this.Lacunarity = Lacunarity; this.Frequency = Frequency; } public NoiseKind Kind { get; } public int Seed { get; } public int Octaves { get; } public double Persistence { get; } public double Lacunarity { get; } public double Frequency { get; } }
    public sealed record PowerCase : ScalarField { internal PowerCase(ScalarField Source, double Exponent) { this.Source = Source; this.Exponent = Exponent; } public ScalarField Source { get; } public double Exponent { get; } }
    public sealed record CsgCase : ScalarField { internal CsgCase(ScalarField Left, ScalarField Right, CsgKind Op, BlendKind Smoothing) { this.Left = Left; this.Right = Right; this.Op = Op; this.Smoothing = Smoothing; } public ScalarField Left { get; } public ScalarField Right { get; } public CsgKind Op { get; } public BlendKind Smoothing { get; } }
    public sealed record PeriodicCase : ScalarField { internal PeriodicCase(ScalarField Source, Vector3d Period) { this.Source = Source; this.Period = Period; } public ScalarField Source { get; } public Vector3d Period { get; } }
    public sealed record ClampCase : ScalarField { internal ClampCase(ScalarField Source, double Minimum, double Maximum) { this.Source = Source; this.Minimum = Minimum; this.Maximum = Maximum; } public ScalarField Source { get; } public double Minimum { get; } public double Maximum { get; } }
    public sealed record PrimitiveCase : ScalarField { internal PrimitiveCase(SdfKind Kind, ImmutableDictionary<string, double> Parameters, Plane Pose) { this.Kind = Kind; this.Parameters = Parameters; this.Pose = Pose; } public SdfKind Kind { get; } public ImmutableDictionary<string, double> Parameters { get; } public Plane Pose { get; } }
    public sealed record ProfileExtrusionCase : ScalarField { internal ProfileExtrusionCase(Curve Profile, Plane Plane, PositiveMagnitude HalfHeight) { this.Profile = Profile; this.Plane = Plane; this.HalfHeight = HalfHeight; } public Curve Profile { get; } public Plane Plane { get; } public PositiveMagnitude HalfHeight { get; } }
    public sealed record OnionCase : ScalarField { internal OnionCase(ScalarField Source, PositiveMagnitude Thickness) { this.Source = Source; this.Thickness = Thickness; } public ScalarField Source { get; } public PositiveMagnitude Thickness { get; } }
    public sealed record SdfRoundCase : ScalarField { internal SdfRoundCase(ScalarField Source, PositiveMagnitude Radius) { this.Source = Source; this.Radius = Radius; } public ScalarField Source { get; } public PositiveMagnitude Radius { get; } }
    public sealed record ElongateCase : ScalarField { internal ElongateCase(ScalarField Source, Vector3d Extent) { this.Source = Source; this.Extent = Extent; } public ScalarField Source { get; } public Vector3d Extent { get; } }
    public sealed record DisplaceCase : ScalarField { internal DisplaceCase(ScalarField Source, ScalarField Displacement) { this.Source = Source; this.Displacement = Displacement; } public ScalarField Source { get; } public ScalarField Displacement { get; } }
    public sealed record TwistCase : ScalarField { internal TwistCase(ScalarField Source, double AnglePerUnit, Direction Axis) { this.Source = Source; this.AnglePerUnit = AnglePerUnit; this.Axis = Axis; } public ScalarField Source { get; } public double AnglePerUnit { get; } public Direction Axis { get; } }
    public sealed record BendCase : ScalarField { internal BendCase(ScalarField Source, double Curvature, Direction Axis) { this.Source = Source; this.Curvature = Curvature; this.Axis = Axis; } public ScalarField Source { get; } public double Curvature { get; } public Direction Axis { get; } }
    public sealed record GeodesicCase : ScalarField { internal GeodesicCase(MeshSpace Space, Seq<int> Sources) { this.Space = Space; this.Sources = Sources; } public MeshSpace Space { get; } public Seq<int> Sources { get; } }
    public sealed record MeanCurvatureFlowCase : ScalarField { internal MeanCurvatureFlowCase(MeshSpace Space, PositiveMagnitude TimeStep, Dimension Iterations) { this.Space = Space; this.TimeStep = TimeStep; this.Iterations = Iterations; } public MeshSpace Space { get; } public PositiveMagnitude TimeStep { get; } public Dimension Iterations { get; } }
    public sealed record SpectralDistanceCase : ScalarField { internal SpectralDistanceCase(MeshSpace Space, SpectralFilter Filter, Seq<int> Sources, Dimension Pairs) { this.Space = Space; this.Filter = Filter; this.Sources = Sources; this.Pairs = Pairs; } public MeshSpace Space { get; } public SpectralFilter Filter { get; } public Seq<int> Sources { get; } public Dimension Pairs { get; } }
    public sealed record StripeCase : ScalarField { internal StripeCase(MeshSpace Space, VectorField CrossField, PositiveMagnitude Frequency) { this.Space = Space; this.CrossField = CrossField; this.Frequency = Frequency; } public MeshSpace Space { get; } public VectorField CrossField { get; } public PositiveMagnitude Frequency { get; } }
    public sealed record SignedDistanceFromMeshCase : ScalarField { internal SignedDistanceFromMeshCase(MeshSpace Space, SdfMeshPolicy Policy) { this.Space = Space; this.Policy = Policy; } public MeshSpace Space { get; } public SdfMeshPolicy Policy { get; } }
    public sealed record RbfCase : ScalarField { internal RbfCase(Seq<(Point3d Position, double Value)> Samples, KernelKind Kernel, PositiveMagnitude Radius, Arr<double> Coefficients, ReconstructionReceipt Receipt) { this.Samples = Samples; this.Kernel = Kernel; this.Radius = Radius; this.Coefficients = Coefficients; this.Receipt = Receipt; } public Seq<(Point3d Position, double Value)> Samples { get; } public KernelKind Kernel { get; } public PositiveMagnitude Radius { get; } public Arr<double> Coefficients { get; } public ReconstructionReceipt Receipt { get; } }
    public sealed record MlsCase : ScalarField { internal MlsCase(Seq<MlsSample> Samples, KernelKind Kernel, PositiveMagnitude Radius, ReconstructionReceipt Receipt) { this.Samples = Samples; this.Kernel = Kernel; this.Radius = Radius; this.Receipt = Receipt; } public Seq<MlsSample> Samples { get; } public KernelKind Kernel { get; } public PositiveMagnitude Radius { get; } public ReconstructionReceipt Receipt { get; } }
    public sealed record TetSignedHeatCase : ScalarField { internal TetSignedHeatCase(TetMeshDomain Domain, TetSignedHeatPolicy Policy, Arr<double> Values, TetSignedHeatReceipt Receipt) { this.Domain = Domain; this.Policy = Policy; this.Values = Values; this.Receipt = Receipt; } public TetMeshDomain Domain { get; } public TetSignedHeatPolicy Policy { get; } public Arr<double> Values { get; } public TetSignedHeatReceipt Receipt { get; } }
    public static ScalarField Constant(double value) => new ConstantCase(Value: value);
    public static Fin<ScalarField> Density(Point3d center, double spread, double strength, Op? key = null) =>
        FieldNabla.WithPositive(candidate: spread, make: s => (ScalarField)new DensityCase(Center: center, Spread: s, Strength: strength), key: key);
    public static ScalarField Magnitude(VectorField source) => new MagnitudeCase(Source: source);
    public static Fin<ScalarField> Divergence(VectorField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<VectorField, ScalarField>(source, epsilon, static (s, e) => new DivergenceCase(Source: s, Epsilon: e), key);
    public static Fin<ScalarField> Laplacian(ScalarField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<ScalarField, ScalarField>(source, epsilon, static (s, e) => new LaplacianCase(Source: s, Epsilon: e), key);
    public static Fin<ScalarField> Divide(ScalarField source, double divisor, Op? key = null) =>
        FieldNabla.WithDivisor(divisor: divisor, make: scale => (ScalarField)new ScaledCase(Source: source, Scale: scale), key: key);
    public static Fin<ScalarField> Worley(Seq<Point3d> seeds, int order = 1, Op? key = null) =>
        seeds.Count >= order && order >= 1
            ? Fin.Succ((ScalarField)new WorleyCase(Seeds: seeds, Order: order))
            : Fin.Fail<ScalarField>(key.OrDefault().InvalidInput());
    public static Fin<ScalarField> Morse(Point3d center, double depth, double width, Op? key = null) =>
        FieldNabla.WithPositivePair(left: depth, right: width, make: (d, w) => (ScalarField)new MorseCase(Center: center, Depth: d, Width: w), key: key);
    public static Fin<ScalarField> Mollifier(Point3d center, double radius, Op? key = null) =>
        FieldNabla.WithPositive(candidate: radius, make: r => (ScalarField)new MollifierCase(Center: center, Radius: r), key: key);
    public static Fin<ScalarField> Power(ScalarField source, double exponent, Op? key = null) =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput()) from _ in guard(RhinoMath.IsValidDouble(x: exponent), key.OrDefault().InvalidInput()) select (ScalarField)new PowerCase(Source: active, Exponent: exponent);
    public static Fin<ScalarField> Primitive(SdfKind kind, ImmutableDictionary<string, double> parameters, Plane pose, Op? key = null) =>
        from active in Optional(kind).ToFin(key.OrDefault().InvalidInput()) from validParams in Optional(parameters).ToFin(key.OrDefault().InvalidInput()) from _ in guard(active.ValidateParameters(parameters: validParams), key.OrDefault().InvalidInput()) from validPose in FieldNabla.Plane(basis: pose, key: key.OrDefault()) select (ScalarField)new PrimitiveCase(Kind: active, Parameters: validParams, Pose: validPose);
    public static Fin<ScalarField> ProfileExtrusion(Curve profile, Plane plane, double halfHeight, Context context, Op? key = null) =>
        from admitted in FieldNabla.ProfileExtrusionInput(profile: profile, plane: plane, halfHeight: halfHeight, context: context, key: key.OrDefault())
        select (ScalarField)new ProfileExtrusionCase(Profile: admitted.Profile, Plane: admitted.Plane, HalfHeight: admitted.HalfHeight);
    public static Fin<ScalarField> Noise(NoiseKind kind, int seed, int octaves, double persistence, double lacunarity, double frequency, Op? key = null) =>
        from active in Optional(kind).ToFin(key.OrDefault().InvalidInput()) from _ in FieldNabla.NoiseInput(octaves: octaves, persistence: persistence, lacunarity: lacunarity, frequency: frequency, key: key.OrDefault()) select (ScalarField)new NoiseCase(Kind: active, Seed: seed, Octaves: octaves, Persistence: persistence, Lacunarity: lacunarity, Frequency: frequency);
    public static Fin<ScalarField> Onion(ScalarField source, double thickness, Op? key = null) =>
        FieldNabla.WithPositive(candidate: thickness, make: t => (ScalarField)new OnionCase(Source: source, Thickness: t), key: key);
    public static Fin<ScalarField> SdfRound(ScalarField source, double radius, Op? key = null) =>
        FieldNabla.WithPositive(candidate: radius, make: r => (ScalarField)new SdfRoundCase(Source: source, Radius: r), key: key);
    public static Fin<ScalarField> Elongate(ScalarField source, Vector3d extent, Op? key = null) =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput())
        from validExtent in FieldNabla.NonnegativeExtent(extent: extent, key: key.OrDefault())
        select (ScalarField)new ElongateCase(Source: active, Extent: validExtent);
    public static Fin<ScalarField> Twist(ScalarField source, double anglePerUnit, Direction axis, Op? key = null) =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput())
        from validAxis in FieldNabla.Direction(value: axis, key: key.OrDefault())
        from _ in FieldNabla.Finite(value: anglePerUnit, key: key.OrDefault())
        select (ScalarField)new TwistCase(Source: active, AnglePerUnit: anglePerUnit, Axis: validAxis);
    public static Fin<ScalarField> Bend(ScalarField source, double curvature, Direction axis, Op? key = null) =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput())
        from validAxis in FieldNabla.Direction(value: axis, key: key.OrDefault())
        from _ in FieldNabla.Finite(value: curvature, key: key.OrDefault())
        select (ScalarField)new BendCase(Source: active, Curvature: curvature, Axis: validAxis);
    public static Fin<ScalarField> Geodesic(MeshSpace space, Seq<int> sources, Op? key = null) =>
        FieldNabla.MeshVertices(space: space, vertices: sources, allowEmpty: false, key: key.OrDefault()).Map(_ => (ScalarField)new GeodesicCase(Space: space, Sources: sources));
    public static Fin<ScalarField> MeanCurvatureFlow(MeshSpace space, double timeStep, int iterations, Op? key = null) =>
        from _ in FieldNabla.MeshOf(space: space, key: key.OrDefault()) from t in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: timeStep) from count in key.OrDefault().AcceptValidated<Dimension>(candidate: iterations) select (ScalarField)new MeanCurvatureFlowCase(Space: space, TimeStep: t, Iterations: count);
    public static Fin<ScalarField> SpectralDistance(MeshSpace space, SpectralFilter filter, Seq<int> sources, int pairs, Op? key = null) =>
        from _ in FieldNabla.MeshVertices(space: space, vertices: sources, allowEmpty: true, key: key.OrDefault()) from active in Optional(filter).ToFin(key.OrDefault().InvalidInput()) from count in key.OrDefault().AcceptValidated<Dimension>(candidate: pairs) select (ScalarField)new SpectralDistanceCase(Space: space, Filter: active, Sources: sources, Pairs: count);
    public static Fin<ScalarField> Stripe(MeshSpace space, VectorField crossField, double frequency, Op? key = null) =>
        from _ in FieldNabla.MeshOf(space: space, key: key.OrDefault()) from active in Optional(crossField).ToFin(key.OrDefault().InvalidInput()) from freq in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: frequency) select (ScalarField)new StripeCase(Space: space, CrossField: active, Frequency: freq);
    public static Fin<ScalarField> SignedDistanceFromMesh(MeshSpace space, SdfMeshPolicy policy, Op? key = null) =>
        from active in MeshKernel.AdmitSignedDistanceMeshPolicy(space: space, policy: policy, key: key.OrDefault())
        select (ScalarField)new SignedDistanceFromMeshCase(Space: space, Policy: active);
    public static Fin<ScalarField> TetSignedHeat(TetMeshDomain domain, TetSignedHeatPolicy? policy = null, Op? key = null) =>
        from activeDomain in domain.Admit(key: key.OrDefault())
        from activePolicy in policy.HasValue ? policy.Value.Admit(key: key.OrDefault()) : TetSignedHeatPolicy.Of(key: key)
        from solved in SolveTetSignedHeat(domain: activeDomain, policy: activePolicy, key: key.OrDefault())
        select (ScalarField)new TetSignedHeatCase(Domain: activeDomain, Policy: activePolicy, Values: solved.Values, Receipt: solved.Receipt);
    public static Fin<ScalarField> Periodic(ScalarField source, Vector3d period, Op? key = null) =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput())
        from validPeriod in FieldNabla.Period(period: period, key: key.OrDefault())
        select (ScalarField)new PeriodicCase(Source: active, Period: validPeriod);
    public static Fin<ScalarField> StrainMagnitude(VectorField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<VectorField, ScalarField>(source, epsilon, static (s, e) => new StrainMagnitudeCase(Source: s, Epsilon: e), key);
    public static Fin<ScalarField> Clamp(ScalarField source, double minimum, double maximum, Op? key = null) =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput()) from _ in FieldNabla.FiniteRange(minimum: minimum, maximum: maximum, key: key.OrDefault()) select (ScalarField)new ClampCase(Source: active, Minimum: minimum, Maximum: maximum);
    public static Fin<ReconstructionResult> RbfDetailed(Seq<(Point3d Position, double Value)> samples, KernelKind kernel, double radius, double smoothing = 0.0, Op? key = null) =>
        ReconstructCore(mode: smoothing <= RhinoMath.ZeroTolerance ? ReconstructionMode.RbfInterpolation : ReconstructionMode.RbfApproximation, scalarSamples: samples, orientedSamples: Seq<MlsSample>(), kernel: kernel, radius: radius, smoothing: smoothing, context: null, key: key.OrDefault());
    public static Fin<ReconstructionResult> MlsDetailed(Seq<MlsSample> samples, KernelKind kernel, double radius, Context context, Op? key = null) =>
        ReconstructCore(mode: ReconstructionMode.MovingLeastSquares, scalarSamples: Seq<(Point3d Position, double Value)>(), orientedSamples: samples, kernel: kernel, radius: radius, smoothing: 0.0, context: context, key: key.OrDefault());
    public static Fin<ReconstructionResult> ReconstructDetailed(ReconstructionMode mode, Seq<MlsSample> samples, KernelKind kernel, double radius, Context context, double smoothing = 0.0, Op? key = null) =>
        ReconstructCore(mode: mode, scalarSamples: samples.Map(static sample => (sample.Position, sample.Value)), orientedSamples: samples, kernel: kernel, radius: radius, smoothing: smoothing, context: context, key: key.OrDefault());
    public static Fin<ReconstructionAttempt> ReconstructAttemptDetailed(ReconstructionMode mode, Seq<MlsSample> samples, KernelKind kernel, double radius, Context context, double smoothing = 0.0, Op? key = null) =>
        Optional(mode).ToFin(key.OrDefault().InvalidInput()).Bind(active => active.Executable
            ? ReconstructDetailed(mode: active, samples: samples, kernel: kernel, radius: radius, context: context, smoothing: smoothing, key: key.OrDefault())
                .Map(result => new ReconstructionAttempt(Result: Some(result), Failure: Option<ReconstructionFailureReceipt>.None))
            : Fin.Succ(new ReconstructionAttempt(
                Result: Option<ReconstructionResult>.None,
                Failure: Some(new ReconstructionFailureReceipt(Mode: active, Kind: ReconstructionFailureKind.UnsupportedMode, SampleCount: samples.Count, RequiresNormals: active.RequiresNormals, RequiresSparseSystem: active.RequiresSparseSystem, RhinoCommonGeneratorAvailable: false)))));
    public Option<double> LipschitzBound() => this switch {
        PrimitiveCase p => Some(p.Kind.Lipschitz),
        ProfileExtrusionCase => Some(1.0),
        CsgCase c => from l in c.Left.LipschitzBound() from r in c.Right.LipschitzBound() select c.Smoothing.Erode(leftLip: l, rightLip: r),
        OnionCase o => o.Source.LipschitzBound(),
        SdfRoundCase r => r.Source.LipschitzBound(),
        ElongateCase e => e.Source.LipschitzBound(),
        _ => Option<double>.None,
    };
    public static ScalarField operator +(ScalarField left, ScalarField right) =>
        new BlendCase(Fields: (left is BlendCase lb && lb.Mode.Equals(FieldBlend.Sum) ? lb.Fields : Seq(left)).Concat(right is BlendCase rb && rb.Mode.Equals(FieldBlend.Sum) ? rb.Fields : Seq(right)).ToSeq(), Mode: FieldBlend.Sum);
    public static ScalarField operator -(ScalarField left, ScalarField right) => left + (-right);
    public static ScalarField operator -(ScalarField field) => new ScaledCase(Source: field, Scale: -1.0);
    public static ScalarField operator *(ScalarField field, double scale) => new ScaledCase(Source: field, Scale: scale);
    public static ScalarField operator *(double scale, ScalarField field) => new ScaledCase(Source: field, Scale: scale);
    public Fin<SdfSample> SampleSdfDetailed(Point3d sample, Context context, Op? key = null) =>
        Optional(context).ToFin(key.OrDefault().MissingContext()).Bind(model => this switch {
            PrimitiveCase => from value in SampleScalar(sample: sample, context: model, key: key.OrDefault()) select new SdfSample(Value: value, Receipt: SdfReceiptOf(field: this, status: SdfStatus.Analytic, mesh: Option<SdfMeshReceipt>.None)),
            ProfileExtrusionCase profileCase => SampleProfileExtrusion(source: profileCase, sample: sample, context: model, key: key.OrDefault()),
            SignedDistanceFromMeshCase meshCase => from signed in MeshKernel.SignedDistanceFromMeshDetailed(space: meshCase.Space, policy: meshCase.Policy, sample: sample, key: key.OrDefault()) select new SdfSample(Value: signed.Distance, Receipt: SdfReceiptOf(field: this, status: SdfStatus.MeshApproximate, mesh: Some(signed.Receipt))),
            TetSignedHeatCase tetCase => from signed in SampleTetSignedHeat(source: tetCase, sample: sample, context: model, key: key.OrDefault())
                                         select new SdfSample(Value: signed.Value, Receipt: new SdfReceipt(Status: SdfStatus.TetSignedHeat, LipschitzBound: Option<double>.None, AnalyticPrimitive: false, MeshBacked: false, WatertightPreflight: Some(value: true), LossyFallback: false, Mesh: Option<SdfMeshReceipt>.None, TetSignedHeat: Some(value: signed.Receipt), TetInterpolation: Some(value: signed.Interpolation))),
            _ => from _ in LipschitzBound().ToFin(key.OrDefault().Unsupported(geometryType: GetType(), outputType: typeof(SdfSample))) from value in SampleScalar(sample: sample, context: model, key: key.OrDefault()) select new SdfSample(Value: value, Receipt: SdfReceiptOf(field: this, status: SdfStatus.ComposedAnalytic, mesh: Option<SdfMeshReceipt>.None)),
        });
    public Fin<IsoSurfaceResult> IsoSurfaceDetailed(BoundingBox bounds, int resolution, int maxRootSteps, Context context, Op? key = null) {
        Op op = key.OrDefault();
        return IsoSurfaceAttemptDetailed(bounds: bounds, resolution: resolution, maxRootSteps: maxRootSteps, context: context, key: op)
            .Bind(result => result.Receipt.Valid ? Fin.Succ(result) : Fin.Fail<IsoSurfaceResult>(op.InvalidResult()));
    }
    internal Fin<IsoSurfaceResult> IsoSurfaceAttemptDetailed(BoundingBox bounds, int resolution, int maxRootSteps, Context context, Op key) {
        ScalarField self = this;
        return FieldNabla.IsoGrid(bounds: bounds, resolution: resolution, maxRootSteps: maxRootSteps, maxCells: FieldNabla.DefaultIsoSurfaceMaxCells, key: key)
            .Bind(grid => (self is SignedDistanceFromMeshCase meshCase
                ? MeshKernel.PrewarmSignedDistanceEvaluator(space: meshCase.Space, policy: meshCase.Policy, key: key).Map(Some)
                : self.Admit(context: context, key: key).Map(_ => Option<SdfMeshReceipt>.None))
            .Bind(meshPreflight => key.Catch(() => {
                int failures = 0;
                double EvaluateIso(Point3d point) =>
                    self.SampleScalar(sample: point, context: context, key: key)
                        .Match(
                            Succ: static value => value,
                            Fail: _ => {
                                failures = Interlocked.Increment(location: ref failures);
                                return double.NaN;
                            });
                Mesh? result = Mesh.CreateFromIsosurface(
                    scalarFieldEvaluator: EvaluateIso,
                    box: bounds, resolution: resolution, RootFindingMaxSteps: maxRootSteps);
                IsoSurfaceStatus status = (failures, result) switch {
                    ( > 0, _) => IsoSurfaceStatus.EvaluatorFailure,
                    (_, null) => IsoSurfaceStatus.NativeReturnedNull,
                    (_, { IsValid: true }) => IsoSurfaceStatus.NativeValid,
                    _ => IsoSurfaceStatus.NativeInvalidMesh,
                };
                bool valid = status.Equals(IsoSurfaceStatus.NativeValid);
                return Fin.Succ(new IsoSurfaceResult(
                    Mesh: result ?? new Mesh(),
                    Receipt: new IsoSurfaceReceipt(NativeRouted: true, Status: status, Grid: grid, MaxRootSteps: maxRootSteps, ParallelCallback: true, EvaluatorFailures: failures, Valid: valid, VertexCount: result?.Vertices.Count ?? 0, FaceCount: result?.Faces.Count ?? 0, FixedTolerance: Some(0.001), FixedNormalSampleDistance: Some(1.0e-5), MeshPreflight: meshPreflight)));
            })));
    }
    internal Fin<Unit> Admit(Context context, Op? key = null) =>
        Optional(context).ToFin(key.OrDefault().MissingContext()).Bind(model => AdmitScalarPayload(context: model, key: key.OrDefault()));
    private Fin<Unit> AdmitScalarPayload(Context context, Op key) =>
        this switch {
            ConstantCase c => FieldNabla.Finite(value: c.Value, key: key),
            DistanceCase c => AdmitSupportSpace(space: c.Source, key: key),
            BlendCase c => from mode in Optional(c.Mode).ToFin(key.InvalidInput())
                           from _ in AdmitScalarFields(fields: c.Fields, context: context, key: key)
                           select unit,
            ScaledCase c => from scale in FieldNabla.Finite(value: c.Scale, key: key)
                            from source in AdmitScalarSource(source: c.Source, context: context, key: key)
                            select unit,
            PotentialCase c => from charges in AdmitCharges(charges: c.Charges, key: key)
                               from falloff in AdmitFalloff(falloff: c.Falloff, context: context, key: key)
                               select unit,
            DensityCase c => from center in FieldNabla.Finite(point: c.Center, key: key)
                             from spread in FieldNabla.Positive(value: c.Spread, key: key)
                             from strength in FieldNabla.Finite(value: c.Strength, key: key)
                             select unit,
            MagnitudeCase c => AdmitVectorSource(source: c.Source, context: context, key: key),
            DivergenceCase c => from epsilon in FieldNabla.Positive(value: c.Epsilon, key: key)
                                from source in AdmitVectorSource(source: c.Source, context: context, key: key)
                                select unit,
            LaplacianCase c => from epsilon in FieldNabla.Positive(value: c.Epsilon, key: key)
                               from source in AdmitScalarSource(source: c.Source, context: context, key: key)
                               select unit,
            StrainMagnitudeCase c => from epsilon in FieldNabla.Positive(value: c.Epsilon, key: key)
                                     from source in AdmitVectorSource(source: c.Source, context: context, key: key)
                                     select unit,
            WorleyCase c => c.Order >= 1 && c.Order <= c.Seeds.Count && c.Seeds.ForAll(static seed => FieldNabla.Finite(point: seed)) ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput()),
            MorseCase c => from center in FieldNabla.Finite(point: c.Center, key: key)
                           from depth in FieldNabla.Positive(value: c.Depth, key: key)
                           from width in FieldNabla.Positive(value: c.Width, key: key)
                           select unit,
            MollifierCase c => from center in FieldNabla.Finite(point: c.Center, key: key)
                               from radius in FieldNabla.Positive(value: c.Radius, key: key)
                               select unit,
            NoiseCase c => FieldNabla.NoiseInput(octaves: c.Octaves, persistence: c.Persistence, lacunarity: c.Lacunarity, frequency: c.Frequency, key: key),
            PowerCase c => from exponent in FieldNabla.Finite(value: c.Exponent, key: key)
                           from source in AdmitScalarSource(source: c.Source, context: context, key: key)
                           select unit,
            CsgCase c => from op in Optional(c.Op).ToFin(key.InvalidInput())
                         from smoothing in Optional(c.Smoothing).ToFin(key.InvalidInput())
                         from left in AdmitScalarSource(source: c.Left, context: context, key: key)
                         from right in AdmitScalarSource(source: c.Right, context: context, key: key)
                         select unit,
            PeriodicCase c => from period in FieldNabla.Period(period: c.Period, key: key).Map(static _ => unit)
                              from source in AdmitScalarSource(source: c.Source, context: context, key: key)
                              select unit,
            ClampCase c => from range in FieldNabla.FiniteRange(minimum: c.Minimum, maximum: c.Maximum, key: key)
                           from source in AdmitScalarSource(source: c.Source, context: context, key: key)
                           select unit,
            PrimitiveCase c => from kind in Optional(c.Kind).ToFin(key.InvalidInput())
                               from parameters in Optional(c.Parameters).ToFin(key.InvalidInput())
                               from _ in guard(parameters.Values.All(static value => RhinoMath.IsValidDouble(x: value)) && kind.ValidateParameters(parameters: parameters), key.InvalidInput())
                               from __ in FieldNabla.Plane(basis: c.Pose, key: key).Map(static _ => unit)
                               select unit,
            ProfileExtrusionCase c => from admitted in FieldNabla.ProfileExtrusionInput(profile: c.Profile, plane: c.Plane, halfHeight: c.HalfHeight.Value, context: context, key: key)
                                      select unit,
            GeodesicCase c => FieldNabla.MeshVertices(space: c.Space, vertices: c.Sources, allowEmpty: false, key: key).Map(static _ => unit),
            MeanCurvatureFlowCase c => from mesh in FieldNabla.MeshOf(space: c.Space, key: key).Map(static _ => unit)
                                       from time in FieldNabla.Positive(value: c.TimeStep, key: key)
                                       from iterations in FieldNabla.Dimension(value: c.Iterations, key: key)
                                       select unit,
            SpectralDistanceCase c => from mesh in FieldNabla.MeshVertices(space: c.Space, vertices: c.Sources, allowEmpty: true, key: key).Map(static _ => unit)
                                      from filter in Optional(c.Filter).ToFin(key.InvalidInput())
                                      from pairs in FieldNabla.Dimension(value: c.Pairs, key: key)
                                      select unit,
            StripeCase c => from mesh in FieldNabla.MeshOf(space: c.Space, key: key).Map(static _ => unit)
                            from frequency in FieldNabla.Positive(value: c.Frequency, key: key)
                            from cross in AdmitVectorSource(source: c.CrossField, context: context, key: key)
                            select unit,
            SignedDistanceFromMeshCase c => MeshKernel.PrewarmSignedDistanceEvaluator(space: c.Space, policy: c.Policy, key: key).Map(static _ => unit),
            RbfCase c => AdmitRbfPayload(field: c, key: key),
            MlsCase c => AdmitMlsPayload(field: c, context: context, key: key),
            TetSignedHeatCase c => AdmitTetSignedHeatPayload(field: c, key: key),
            OnionCase c => from thickness in FieldNabla.Positive(value: c.Thickness, key: key)
                           from source in AdmitScalarSource(source: c.Source, context: context, key: key)
                           select unit,
            SdfRoundCase c => from radius in FieldNabla.Positive(value: c.Radius, key: key)
                              from source in AdmitScalarSource(source: c.Source, context: context, key: key)
                              select unit,
            ElongateCase c => from extent in FieldNabla.NonnegativeExtent(extent: c.Extent, key: key).Map(static _ => unit)
                              from source in AdmitScalarSource(source: c.Source, context: context, key: key)
                              select unit,
            DisplaceCase c => from source in AdmitScalarSource(source: c.Source, context: context, key: key)
                              from displacement in AdmitScalarSource(source: c.Displacement, context: context, key: key)
                              select unit,
            TwistCase c => from angle in FieldNabla.Finite(value: c.AnglePerUnit, key: key)
                           from axis in FieldNabla.Direction(value: c.Axis, key: key)
                           from source in AdmitScalarSource(source: c.Source, context: context, key: key)
                           select unit,
            BendCase c => from curvature in FieldNabla.Finite(value: c.Curvature, key: key)
                          from axis in FieldNabla.Direction(value: c.Axis, key: key)
                          from source in AdmitScalarSource(source: c.Source, context: context, key: key)
                          select unit,
            _ => Fin.Fail<Unit>(key.InvalidInput()),
        };
    private static Fin<Unit> AdmitScalarSource(ScalarField? source, Context context, Op key) =>
        Optional(source).ToFin(key.InvalidInput()).Bind(field => field.AdmitScalarPayload(context: context, key: key));
    private static Fin<Unit> AdmitScalarFields(Seq<ScalarField> fields, Context context, Op key) =>
        AdmitFieldSequence(fields: fields, context: context, key: key, admit: static (field, model, op) => field.AdmitScalarPayload(context: model, key: op));
    private static Fin<Unit> AdmitVectorSource(VectorField? source, Context context, Op key) =>
        Optional(source).ToFin(key.InvalidInput()).Bind(field => AdmitVectorField(field: field, context: context, key: key));
    private static Fin<Unit> AdmitVectorFields(Seq<VectorField> fields, Context context, Op key) =>
        AdmitFieldSequence(fields: fields, context: context, key: key, admit: static (field, model, op) => AdmitVectorField(field: field, context: model, key: op));
    private static Fin<Unit> AdmitFieldSequence<TField>(Seq<TField> fields, Context context, Op key, Func<TField, Context, Op, Fin<Unit>> admit)
        where TField : class =>
        !fields.IsEmpty ? fields.TraverseM(field => Optional(field).ToFin(key.InvalidInput()).Bind(active => admit(arg1: active, arg2: context, arg3: key))).As().Map(static _ => unit) : Fin.Fail<Unit>(key.InvalidInput());
    private static Fin<Unit> AdmitVectorField(VectorField field, Context context, Op key) =>
        field switch {
            VectorField.ConstantCase c => FieldNabla.Finite(vector: c.Value, key: key),
            VectorField.BlendCase c => from mode in Optional(c.Mode).ToFin(key.InvalidInput())
                                       from fields in AdmitVectorFields(fields: c.Fields, context: context, key: key)
                                       select unit,
            VectorField.ScaledCase c => from scale in FieldNabla.Finite(value: c.Scale, key: key)
                                        from source in AdmitVectorSource(source: c.Source, context: context, key: key)
                                        select unit,
            VectorField.InfluenceCase c => from space in AdmitSupportSpace(space: c.Source, key: key)
                                           from falloff in AdmitFalloff(falloff: c.Falloff, context: context, key: key)
                                           select unit,
            VectorField.HitFieldCase c => from space in AdmitSupportSpace(space: c.Source, key: key)
                                          from projection in Optional(c.Projection).ToFin(key.InvalidInput())
                                          select unit,
            VectorField.VortexCase c => from anchor in FieldNabla.Finite(point: c.Anchor, key: key)
                                        from axis in FieldNabla.Direction(value: c.Axis, key: key)
                                        from falloff in AdmitFalloff(falloff: c.Falloff, context: context, key: key)
                                        select unit,
            VectorField.RingCase c => from center in FieldNabla.Finite(point: c.Center, key: key)
                                      from axis in FieldNabla.Direction(value: c.Axis, key: key)
                                      from falloff in AdmitFalloff(falloff: c.Falloff, context: context, key: key)
                                      select unit,
            VectorField.HelicalCase c => from anchor in FieldNabla.Finite(point: c.Anchor, key: key)
                                         from axis in FieldNabla.Direction(value: c.Axis, key: key)
                                         from axial in FieldNabla.Finite(value: c.Axial, key: key)
                                         from swirl in FieldNabla.Finite(value: c.Swirl, key: key)
                                         from falloff in AdmitFalloff(falloff: c.Falloff, context: context, key: key)
                                         select unit,
            VectorField.CoulombCase c => from charges in AdmitCharges(charges: c.Charges, key: key)
                                         from falloff in AdmitFalloff(falloff: c.Falloff, context: context, key: key)
                                         select unit,
            VectorField.ClusterFieldCase c => from cluster in Optional(c.Source).ToFin(key.InvalidInput())
                                              from mass in CloudKernel.MassOf(cluster: cluster, key: key).Map(static _ => unit)
                                              from falloff in AdmitFalloff(falloff: c.Falloff, context: context, key: key)
                                              select unit,
            VectorField.DipoleCase c => from origin in FieldNabla.Finite(point: c.Origin, key: key)
                                        from moment in FieldNabla.Direction(value: c.Moment, key: key)
                                        select unit,
            VectorField.HarmonicCase c => c.Components.TraverseM(component =>
                from direction in FieldNabla.Direction(value: component.Direction, key: key)
                from frequency in FieldNabla.Finite(value: component.Frequency, key: key)
                from phase in FieldNabla.Finite(value: component.Phase, key: key)
                from amplitude in FieldNabla.Finite(value: component.Amplitude, key: key)
                select unit).As().Map(static _ => unit),
            VectorField.ProjectedCase c => from plane in FieldNabla.Plane(basis: c.Onto, key: key).Map(static _ => unit)
                                           from source in AdmitVectorSource(source: c.Source, context: context, key: key)
                                           select unit,
            VectorField.WarpCase c => from spatial in key.AcceptValue(value: c.Spatial).Map(static _ => unit)
                                      from source in AdmitVectorSource(source: c.Source, context: context, key: key)
                                      select unit,
            VectorField.ClampMagnitudeCase c => from order in guard(c.Min.Value <= c.Max.Value, key.InvalidInput())
                                                from source in AdmitVectorSource(source: c.Source, context: context, key: key)
                                                select unit,
            VectorField.GradientCase c => AdmitScalarSource(source: c.Source, context: context, key: key),
            VectorField.CurlCase c => AdmitVectorSource(source: c.Source, context: context, key: key),
            VectorField.CurlNoiseCase c => AdmitScalarSource(source: c.Potential, context: context, key: key),
            VectorField.CrossProductCase c => from left in AdmitVectorSource(source: c.Left, context: context, key: key)
                                              from right in AdmitVectorSource(source: c.Right, context: context, key: key)
                                              select unit,
            VectorField.BiotSavartCase c => from start in FieldNabla.Finite(point: c.Start, key: key)
                                            from end in FieldNabla.Finite(point: c.End, key: key)
                                            from current in FieldNabla.Finite(value: c.Current, key: key)
                                            from length in guard(!(c.Start - c.End).IsTiny(), key.InvalidInput())
                                            select unit,
            VectorField.SaddleCase c => from anchor in FieldNabla.Finite(point: c.Anchor, key: key)
                                        from basis in FieldNabla.Plane(basis: c.Basis, key: key).Map(static _ => unit)
                                        from strength in FieldNabla.Finite(value: c.Strength, key: key)
                                        select unit,
            VectorField.CrossFieldCase c => AdmitCrossFieldPayload(field: c, key: key),
            VectorField.HodgeCase c => from mesh in FieldNabla.MeshOf(space: c.Space, key: key).Map(static _ => unit)
                                       from source in AdmitVectorSource(source: c.Source, context: context, key: key)
                                       select unit,
            VectorField.VectorHeatCase c => from mesh in FieldNabla.MeshVertices(space: c.Space, vertices: c.Sources.Map(static source => source.Vertex), allowEmpty: false, key: key).Map(static _ => unit)
                                            from vectors in c.Sources.TraverseM(source => FieldNabla.Finite(vector: source.Direction, key: key)).As().Map(static _ => unit)
                                            select unit,
            VectorField.GeodesicTangentCase c => FieldNabla.MeshVertices(space: c.Space, vertices: c.Sources, allowEmpty: false, key: key).Map(static _ => unit),
            VectorField.TangentLogMapCase c => from source in FieldNabla.MeshVertices(space: c.Space, vertices: Seq(c.Source), allowEmpty: false, key: key).Map(static _ => unit)
                                               from time in FieldNabla.Positive(value: c.Time, key: key)
                                               select unit,
            _ => Fin.Fail<Unit>(key.InvalidInput()),
        };
    private static Fin<Unit> AdmitFalloff(Falloff? falloff, Context context, Op key) =>
        Optional(falloff).ToFin(key.InvalidInput()).Bind(active => active switch {
            Falloff.ConstantCase => Fin.Succ(unit),
            Falloff.InverseCase => Fin.Succ(unit),
            Falloff.InverseSquareCase => Fin.Succ(unit),
            Falloff.GaussianCase => Fin.Succ(unit),
            Falloff.KernelCase c => Optional(c.Kind).ToFin(key.InvalidInput()).Map(static _ => unit),
            Falloff.AnisotropicKernelCase c => from kind in Optional(c.Kind).ToFin(key.InvalidInput())
                                               from metric in AdmitTensorField(field: c.Metric, context: context, key: key)
                                               select unit,
            _ => Fin.Fail<Unit>(key.InvalidInput()),
        });
    private static Fin<Unit> AdmitTensorField(TensorField? field, Context context, Op key) =>
        Optional(field).ToFin(key.InvalidInput()).Bind(active => active switch {
            TensorField.ConstantCase c => c.Value.IsValid ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput()),
            TensorField.CurvatureCase c => FieldNabla.SurfaceNative(space: c.Space, key: key).Map(static _ => unit),
            TensorField.LiftCase c => Optional(c.Sampler).ToFin(key.InvalidInput()).Map(static _ => unit),
            TensorField.WarpCase c => from spatial in key.AcceptValue(value: c.Spatial).Map(static _ => unit)
                                      from source in AdmitTensorField(field: c.Source, context: context, key: key)
                                      select unit,
            TensorField.ScaledCase c => from scale in FieldNabla.Finite(value: c.Scale, key: key)
                                        from source in AdmitTensorField(field: c.Source, context: context, key: key)
                                        select unit,
            TensorField.BlendCase c => !c.Fields.IsEmpty ? c.Fields.TraverseM(source => AdmitTensorField(field: source, context: context, key: key)).As().Map(static _ => unit) : Fin.Fail<Unit>(key.InvalidInput()),
            _ => Fin.Fail<Unit>(key.InvalidInput()),
        });
    private static Fin<Unit> AdmitRbfPayload(RbfCase field, Op key) =>
        from kernel in Optional(field.Kernel).ToFin(key.InvalidInput())
        from radius in FieldNabla.Positive(value: field.Radius, key: key)
        from samples in FieldNabla.ReconstructionSamples(samples: field.Samples, key: key)
        from coefficientShape in guard(field.Coefficients.Count == samples.Count && field.Coefficients.ForAll(static value => RhinoMath.IsValidDouble(x: value)), key.InvalidResult())
        from solve in field.Receipt.Solve.ToFin(key.InvalidResult())
        from usable in solve.IsUsable ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
        from receipt in AdmitReconstructionReceipt(receipt: field.Receipt, mode: Option<ReconstructionMode>.None, kernel: kernel, radius: field.Radius.Value, sampleCount: samples.Count, solve: solve, key: key)
        let interpolation = field.Receipt.Smoothing <= RhinoMath.ZeroTolerance
        let rows = interpolation ? samples.Count : 2 * samples.Count
        from mode in guard(field.Receipt.PolynomialDegree == 0 && field.Receipt.Interpolation == interpolation && field.Receipt.Mode.Equals(interpolation ? ReconstructionMode.RbfInterpolation : ReconstructionMode.RbfApproximation), key.InvalidResult())
        from shape in guard(solve.Solution.Count == field.Coefficients.Count && solve.Cols.Value == field.Coefficients.Count && solve.Rows.Value == rows && solve.RhsLength == rows, key.InvalidResult())
        select unit;
    private static Fin<Unit> AdmitMlsPayload(MlsCase field, Context context, Op key) =>
        from kernel in Optional(field.Kernel).ToFin(key.InvalidInput())
        from radius in FieldNabla.Positive(value: field.Radius, key: key)
        from samples in FieldNabla.MlsInput(samples: field.Samples, context: context, key: key)
        from receipt in AdmitReconstructionReceipt(receipt: field.Receipt, mode: Some(ReconstructionMode.MovingLeastSquares), kernel: kernel, radius: field.Radius.Value, sampleCount: samples.Count, solve: Option<SolveReceipt>.None, key: key)
        from mode in guard(!field.Receipt.Interpolation && field.Receipt.PolynomialDegree == 1, key.InvalidResult())
        select unit;
    private static Fin<Unit> AdmitTetSignedHeatPayload(TetSignedHeatCase field, Op key) =>
        from domain in field.Domain.Admit(key: key)
        from policy in field.Policy.Admit(key: key)
        from values in guard(field.Values.Count == domain.Vertices.Count && field.Values.ForAll(RhinoMath.IsValidDouble), key.InvalidResult()).ToFin()
        from _ in guard(
            field.Receipt.Fem.VertexCount == domain.Vertices.Count
            && field.Receipt.Fem.CellCount == domain.Cells.Count
            && field.Receipt.Fem.BoundaryVertexCount == domain.BoundaryVertices.Count
            && field.Receipt.Fem.BoundaryFaceCount == domain.BoundaryFaceCount
            && field.Receipt.Fem.InteriorVertexCount == domain.InteriorVertexCount
            && field.Receipt.Fem.TotalVolume > 0.0
            && field.Receipt.Fem.MassNonZeros > 0
            && field.Receipt.Fem.StiffnessNonZeros > 0
            && field.Receipt.Fem.HeatOperatorNonZeros > 0
            && field.Receipt.Fem.DivergenceNonZeros > 0
            && field.Receipt.Heat.Equals(policy.Heat)
            && field.Receipt.Solver.Equals(policy.Solver)
            && field.Receipt.SignConvention.Equals(policy.SignConvention)
            && field.Receipt.Gauge.Equals(policy.Gauge)
            && field.Receipt.Interpolation.Equals(policy.Interpolation)
            && field.Receipt.GaugeVertex >= 0
            && field.Receipt.GaugeVertex < domain.Vertices.Count
            && field.Receipt.HeatSolve.IsUsable
            && field.Receipt.PoissonSolve.IsUsable
            && field.Receipt.HeatSolve.Residual <= policy.Solver.ResidualTolerance.Value
            && field.Receipt.PoissonSolve.Residual <= policy.Solver.ResidualTolerance.Value,
            key.InvalidResult()).ToFin()
        select unit;
    private static Fin<Unit> AdmitReconstructionReceipt(ReconstructionReceipt receipt, Option<ReconstructionMode> mode, KernelKind kernel, double radius, int sampleCount, Option<SolveReceipt> solve, Op key) =>
        from receiptKernel in Optional(receipt.Kernel).ToFin(key.InvalidResult())
        from receiptMode in Optional(receipt.Mode).ToFin(key.InvalidResult())
        from _ in guard(
            mode.Match(Some: receiptMode.Equals, None: static () => true)
            && receiptKernel.Equals(kernel)
            && Math.Abs(value: receipt.Radius - radius) <= RhinoMath.SqrtEpsilon
            && RhinoMath.IsValidDouble(x: receipt.Smoothing)
            && receipt.Smoothing >= 0.0
            && receipt.SampleCount == sampleCount
            && receipt.CenterCount == sampleCount
            && solve.Match(
                Some: solved => receipt.Solve.Match(Some: actual => actual.Equals(solved), None: static () => false)
                    && solved.IsUsable
                    && solved.Solution.ForAll(static value => RhinoMath.IsValidDouble(x: value))
                    && RhinoMath.IsValidDouble(x: solved.Residual),
                None: () => receipt.Solve.IsNone),
            key.InvalidResult())
        select unit;
    private static Fin<Unit> AdmitCrossFieldPayload(VectorField.CrossFieldCase field, Op key) =>
        from mesh in FieldNabla.MeshOf(space: field.Space, key: key)
        from symmetry in guard(field.Symmetry is 1 or 2 or 4 or 6, key.InvalidInput())
        from constraints in field.Constraints.Match(
            Some: values => FieldNabla.MeshVertices(space: field.Space, vertices: values.Map(static value => value.Vertex), allowEmpty: true, key: key)
                .Bind(_ => values.TraverseM(value => FieldNabla.Direction(value: value.Hint, key: key).Map(static _ => unit)).As().Map(static _ => unit)),
            None: static () => Fin.Succ(unit))
        from cones in field.Cones.Match(
            Some: values => FieldNabla.MeshVertices(space: field.Space, vertices: values.Map(static value => value.Vertex), allowEmpty: true, key: key)
                .Bind(_ => values.TraverseM(value => FieldNabla.Finite(value: value.HolonomyDeficit, key: key)).As().Map(static _ => unit)),
            None: static () => Fin.Succ(unit))
        select unit;
    private static Fin<Unit> AdmitCharges(Seq<(Point3d Position, double Charge)> charges, Op key) =>
        charges.TraverseM(charge =>
            from point in FieldNabla.Finite(point: charge.Position, key: key)
            from value in FieldNabla.Finite(value: charge.Charge, key: key)
            select unit).As().Map(static _ => unit);
    private static Fin<Unit> AdmitSupportSpace(SupportSpace? space, Op key) =>
        Optional(space).ToFin(key.InvalidInput()).Bind(source => Optional(source.Value).ToFin(key.InvalidInput()).Map(static _ => unit));
    internal static bool BoundsAdmitted(BoundingBox bounds) =>
        bounds is { IsValid: true, Diagonal: Vector3d d }
        && d.X > RhinoMath.ZeroTolerance
        && d.Y > RhinoMath.ZeroTolerance
        && d.Z > RhinoMath.ZeroTolerance;
    internal Fin<double> SampleScalar(Point3d sample, Context context, Op key) =>
        FieldNabla.Finite(point: sample, key: key).Bind(finiteSample => Switch(
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
                double d => c.Falloff.Weight(offset: state.Sample - charge.Position, sample: state.Sample, context: state.Context, tolerance: state.Context.Absolute.Value, key: state.Key)
                    .Bind(weight => state.Key.AcceptValue(value: sum + (charge.Charge * weight))),
            })),
        densityCase: static (state, c) => state.Key.AcceptValue(value:
            c.Strength * Math.Exp(d: -(state.Sample - c.Center).SquareLength / (2.0 * c.Spread.Value * c.Spread.Value))),
        blendCase: static (state, c) => c.Fields.TraverseM(f => f.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)).As()
            .Bind(values => c.Mode.CombineScalar(values: values, key: state.Key)),
        magnitudeCase: static (state, c) => c.Source.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(v => state.Key.AcceptValue(value: v.Length)),
        divergenceCase: static (state, c) => FieldNabla.DivergenceAt(field: c.Source, point: state.Sample, eps: c.Epsilon.Value, context: state.Context, key: state.Key),
        laplacianCase: static (state, c) => FieldNabla.LaplacianAt(field: c.Source, point: state.Sample, eps: c.Epsilon.Value, context: state.Context, key: state.Key),
        scaledCase: static (state, c) => SampleMapped(source: c.Source, state: state, data: c.Scale, map: static (scale, value) => scale * value),
        worleyCase: static (state, c) =>
            toSeq(c.Seeds.Map(seed => state.Sample.DistanceTo(other: seed)).Order().AsIterable()) switch {
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
        powerCase: static (state, c) => SampleMapped(source: c.Source, state: state, data: c.Exponent, map: static (exponent, value) => Math.Pow(x: value, y: exponent)),
        csgCase: static (state, c) =>
            from leftValue in c.Left.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            from rightValue in c.Right.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            from output in state.Key.AcceptValue(value: c.Op.Combine(left: leftValue, right: rightValue, blend: c.Smoothing))
            select output,
        periodicCase: static (state, c) => c.Source.SampleScalar(
            sample: FieldNabla.ToroidalWrap(sample: state.Sample, period: c.Period),
            context: state.Context, key: state.Key),
        strainMagnitudeCase: static (state, c) => FieldNabla.StrainMagnitudeAt(field: c.Source, point: state.Sample, eps: c.Epsilon.Value, context: state.Context, key: state.Key),
        clampCase: static (state, c) => SampleMapped(source: c.Source, state: state, data: (c.Minimum, c.Maximum), map: static (range, value) => Math.Clamp(value: value, min: range.Minimum, max: range.Maximum)),
        primitiveCase: static (state, c) => c.Kind.SignedDistance(worldPoint: state.Sample, parameters: c.Parameters, pose: c.Pose, key: state.Key),
        profileExtrusionCase: static (state, c) => SampleProfileExtrusion(source: c, sample: state.Sample, context: state.Context, key: state.Key).Map(static sdf => sdf.Value),
        noiseCase: static (state, c) => {
            (double sum, _, _) = toSeq(Enumerable.Range(start: 0, count: c.Octaves)).Fold(
                initialState: (Sum: 0.0, Amp: 1.0, Freq: c.Frequency),
                f: (acc, octave) => (
                    Sum: acc.Sum + (acc.Amp * c.Kind.Sample(point: state.Sample, seed: c.Seed + octave, frequency: acc.Freq)),
                    Amp: acc.Amp * c.Persistence,
                    Freq: acc.Freq * c.Lacunarity));
            double norm = Math.Abs(value: c.Persistence - 1.0) < RhinoMath.ZeroTolerance
                ? c.Octaves
                : (1.0 - Math.Pow(x: c.Persistence, y: c.Octaves)) / (1.0 - c.Persistence);
            return norm > RhinoMath.ZeroTolerance
                ? state.Key.AcceptValue(value: sum / norm)
                : Fin.Fail<double>(state.Key.InvalidResult());
        },
        onionCase: static (state, c) => SampleMapped(source: c.Source, state: state, data: c.Thickness.Value, map: static (thickness, value) => Math.Abs(value: value) - thickness),
        sdfRoundCase: static (state, c) => SampleMapped(source: c.Source, state: state, data: c.Radius.Value, map: static (radius, value) => value - radius),
        elongateCase: static (state, c) => {
            Point3d shifted = new(
                x: state.Sample.X - Math.Clamp(value: state.Sample.X, min: -c.Extent.X, max: c.Extent.X),
                y: state.Sample.Y - Math.Clamp(value: state.Sample.Y, min: -c.Extent.Y, max: c.Extent.Y),
                z: state.Sample.Z - Math.Clamp(value: state.Sample.Z, min: -c.Extent.Z, max: c.Extent.Z));
            return c.Source.SampleScalar(sample: shifted, context: state.Context, key: state.Key);
        },
        displaceCase: static (state, c) =>
            from a in c.Source.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            from b in c.Displacement.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            from output in state.Key.AcceptValue(value: a + b)
            select output,
        twistCase: static (state, c) => {
            Vector3d axis = c.Axis.Value;
            Vector3d offsetRaw = state.Sample - Point3d.Origin;
            double along = offsetRaw * axis;
            Vector3d offset = Transform.Rotation(angleRadians: -c.AnglePerUnit * along, rotationAxis: axis, rotationCenter: Point3d.Origin) * offsetRaw;
            return c.Source.SampleScalar(sample: Point3d.Origin + offset, context: state.Context, key: state.Key);
        },
        bendCase: static (state, c) => {
            Vector3d axis = c.Axis.Value;
            Vector3d offsetRaw = state.Sample - Point3d.Origin;
            double along = offsetRaw * axis;
            Vector3d perp = offsetRaw - (along * axis);
            Vector3d rotated = Transform.Rotation(angleRadians: c.Curvature * along, rotationAxis: axis, rotationCenter: Point3d.Origin) * perp;
            return c.Source.SampleScalar(sample: Point3d.Origin + (along * axis) + rotated, context: state.Context, key: state.Key);
        },
        geodesicCase: static (state, c) => MeshKernel.HeatGeodesicAt(space: c.Space, sources: c.Sources, sample: state.Sample, key: state.Key),
        meanCurvatureFlowCase: static (state, c) => MeshKernel.MeanCurvatureMagnitudeAt(space: c.Space, timeStep: c.TimeStep.Value, iterations: c.Iterations.Value, sample: state.Sample, key: state.Key),
        spectralDistanceCase: static (state, c) => MeshKernel.SpectralDistanceAt(space: c.Space, filter: c.Filter, sources: c.Sources, pairs: c.Pairs.Value, sample: state.Sample, key: state.Key),
        stripeCase: static (state, c) => MeshKernel.StripeAt(space: c.Space, crossField: c.CrossField, frequency: c.Frequency.Value, sample: state.Sample, key: state.Key),
        signedDistanceFromMeshCase: static (state, c) => MeshKernel.SignedDistanceFromMeshDetailed(space: c.Space, policy: c.Policy, sample: state.Sample, key: state.Key).Map(static result => result.Distance),
        tetSignedHeatCase: static (state, c) => SampleTetSignedHeat(source: c, sample: state.Sample, context: state.Context, key: state.Key).Map(static result => result.Value),
        rbfCase: static (state, c) => EvaluateRbf(samples: c.Samples, kernel: c.Kernel, radius: c.Radius.Value, coefficients: c.Coefficients, sample: state.Sample, key: state.Key),
        mlsCase: static (state, c) => EvaluateMls(samples: c.Samples, kernel: c.Kernel, radius: c.Radius.Value, sample: state.Sample, context: state.Context, key: state.Key).Map(static result => result.Value)));
    public Fin<ReconstructionSample> SampleReconstructionDetailed(Point3d sample, Context context, Op? key = null) =>
        from model in Optional(context).ToFin(key.OrDefault().MissingContext())
        from finiteSample in FieldNabla.Finite(point: sample, key: key.OrDefault())
        from output in this switch {
            RbfCase r => from weights in KernelWeights(left: [sample], right: r.Samples.AsIterable().Select(static s => s.Position), kernel: r.Kernel, radius: r.Radius.Value, key: key.OrDefault())
                         let weightArray = weights.AsIterable().ToArray()
                         let support = weightArray.Count(static weight => Math.Abs(value: weight) > RhinoMath.SqrtEpsilon)
                         let weightSum = weightArray.Sum()
                         from value in weightArray.Length == r.Samples.Count && r.Coefficients.Count == r.Samples.Count
                             ? key.OrDefault().AcceptValue(value: Enumerable.Range(start: 0, count: weightArray.Length).Sum(i => weightArray[i] * r.Coefficients[i]))
                             : Fin.Fail<double>(key.OrDefault().InvalidResult())
                         from solve in r.Receipt.Solve.ToFin(key.OrDefault().InvalidResult())
                         select new ReconstructionSample(Value: value, Receipt: new ReconstructionSampleReceipt(Mode: r.Receipt.Mode, Status: r.Receipt.Interpolation ? ReconstructionStatus.ExactInterpolation : ReconstructionStatus.ApproximateSdf, Kernel: r.Kernel, Radius: r.Radius.Value, SampleCount: r.Samples.Count, NeighborhoodCount: support, RejectedWeightCount: r.Samples.Count - support, WeightSum: weightSum, Rank: solve.IsUsable ? solve.Solution.Count : 0, Condition: Option<double>.None, NormalAgreement: Option<double>.None, GradientNorm: Option<double>.None, Solve: solve)),
            MlsCase m => EvaluateMls(samples: m.Samples, kernel: m.Kernel, radius: m.Radius.Value, sample: sample, context: model, key: key.OrDefault()),
            _ => Fin.Fail<ReconstructionSample>(key.OrDefault().Unsupported(geometryType: GetType(), outputType: typeof(ReconstructionSample))),
        }
        select output;
    private static Fin<double> SampleMapped<T>(ScalarField source, (Point3d Sample, Context Context, Op Key) state, T data, Func<T, double, double> map) =>
        source.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(value => state.Key.AcceptValue(value: map(arg1: data, arg2: value)));
    private static Fin<ReconstructionResult> ReconstructCore(ReconstructionMode mode, Seq<(Point3d Position, double Value)> scalarSamples, Seq<MlsSample> orientedSamples, KernelKind kernel, double radius, double smoothing, Context? context, Op key) =>
        from activeMode in Optional(mode).ToFin(key.InvalidInput())
        from active in Optional(kernel).ToFin(key.InvalidInput())
        from r in key.AcceptValidated<PositiveMagnitude>(candidate: radius)
        from supported in activeMode.Executable ? Fin.Succ(unit) : Fin.Fail<Unit>(key.Unsupported(geometryType: typeof(ReconstructionMode), outputType: typeof(ReconstructionResult)))
        from result in activeMode switch {
            ReconstructionMode m when m.Equals(ReconstructionMode.RbfInterpolation) || m.Equals(ReconstructionMode.RbfApproximation) =>
                BuildRbf(mode: m, samples: scalarSamples, kernel: active, radius: r, smoothing: smoothing, key: key),
            ReconstructionMode m when m.Equals(ReconstructionMode.MovingLeastSquares) =>
                from model in Optional(context).ToFin(key.MissingContext())
                from admittedSamples in FieldNabla.MlsInput(samples: orientedSamples, context: model, key: key)
                let receipt = new ReconstructionReceipt(Mode: m, Kernel: active, Radius: r.Value, Smoothing: 0.0, Interpolation: false, SampleCount: admittedSamples.Count, CenterCount: admittedSamples.Count, PolynomialDegree: m.PolynomialDegree, Solve: Option<SolveReceipt>.None)
                select new ReconstructionResult(Field: new MlsCase(Samples: admittedSamples, Kernel: active, Radius: r, Receipt: receipt), Receipt: receipt),
            _ => Fin.Fail<ReconstructionResult>(key.Unsupported(geometryType: typeof(ReconstructionMode), outputType: typeof(ReconstructionResult))),
        }
        select result;
    private static Fin<ReconstructionResult> BuildRbf(ReconstructionMode mode, Seq<(Point3d Position, double Value)> samples, KernelKind kernel, PositiveMagnitude radius, double smoothing, Op key) =>
        from admittedSamples in FieldNabla.ReconstructionSamples(samples: samples, key: key)
        from admittedSmoothing in FieldNabla.NonnegativeFinite(value: smoothing, key: key)
        let interpolation = mode.Equals(ReconstructionMode.RbfInterpolation)
        from modeSmoothing in interpolation ? guard(admittedSmoothing <= RhinoMath.ZeroTolerance, key.InvalidInput()).ToFin() : guard(admittedSmoothing > RhinoMath.ZeroTolerance, key.InvalidInput()).ToFin()
        let n = admittedSamples.Count
        let sampleArray = admittedSamples.AsIterable().ToArray()
        let rhs = new Arr<double>([.. sampleArray.Select(static sample => sample.Value)])
        let cols = Dimension.Create(value: n)
        from kernelEntries in KernelWeights(left: sampleArray.Select(static sample => sample.Position), right: sampleArray.Select(static sample => sample.Position), kernel: kernel, radius: radius.Value, key: key)
        from matrix in Matrix.Of(
            rows: Dimension.Create(value: interpolation ? n : 2 * n), cols: cols,
            entries: interpolation ? kernelEntries : new Arr<double>([.. kernelEntries.AsIterable().Concat(Enumerable.Range(start: 0, count: n * n).Select(i => i / n == i % n ? Math.Sqrt(d: admittedSmoothing) : 0.0))]), key: key)
        from solved in interpolation
            ? matrix.SolveDetailed(rhs: rhs, key: key)
            : matrix.LeastSquaresDetailed(rhs: new Arr<double>([.. rhs.AsIterable().Concat(Enumerable.Repeat(element: 0.0, count: n))]), key: key)
        from usable in solved.IsUsable ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
        let receipt = new ReconstructionReceipt(Mode: mode, Kernel: kernel, Radius: radius.Value, Smoothing: admittedSmoothing, Interpolation: interpolation, SampleCount: admittedSamples.Count, CenterCount: admittedSamples.Count, PolynomialDegree: mode.PolynomialDegree, Solve: Some(solved))
        select new ReconstructionResult(Field: new RbfCase(Samples: admittedSamples, Kernel: kernel, Radius: radius, Coefficients: solved.Solution, Receipt: receipt), Receipt: receipt);
    [StructLayout(LayoutKind.Auto)] private readonly record struct TetSignedHeatSolution(Arr<double> Values, TetSignedHeatReceipt Receipt);
    [StructLayout(LayoutKind.Auto)] private readonly record struct TetAssembly(SparseMatrix Mass, SparseMatrix Stiffness, SparseMatrix HeatOperator, Arr<double> HeatRhs, Arr<double> MassLumped, int GaugeVertex, double HeatTime);
    [StructLayout(LayoutKind.Auto)] private readonly record struct TetDivergence(Arr<double> Rhs, int NonZeros, int RejectedGradientCellCount);
    [StructLayout(LayoutKind.Auto)] private readonly record struct TetCalibration(Arr<double> Values, double BoundaryShift, double InteriorMean);
    [StructLayout(LayoutKind.Auto)] private readonly record struct TetInterpolated(double Value, TetInterpolationReceipt Receipt);
    private static Fin<TetSignedHeatSolution> SolveTetSignedHeat(TetMeshDomain domain, TetSignedHeatPolicy policy, Op key) =>
        from activeDomain in domain.Admit(key: key)
        from activePolicy in policy.Admit(key: key)
        from _ in activeDomain.InteriorVertexCount > 0 ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput())
        from assembly in AssembleTetFem(domain: activeDomain, policy: activePolicy, key: key)
        from heatSolve in CholeskySparse.Of(symmetric: assembly.HeatOperator, key: key).Bind(factor => factor.SolveDetailed(rhs: assembly.HeatRhs, key: key))
        from heatResidual in heatSolve.Residual <= activePolicy.Solver.ResidualTolerance.Value ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
        from divergence in TetDivergenceOf(domain: activeDomain, heat: heatSolve.Solution, key: key)
        from poissonSolve in assembly.Stiffness.SingularSolveDetailed(rhs: divergence.Rhs, gauge: GaugePolicy.PinConstant(index: assembly.GaugeVertex, mass: Some(assembly.MassLumped), shift: GaugeShift.PinZero), context: activeDomain.Context, key: key)
        from poissonResidual in poissonSolve.Residual <= activePolicy.Solver.ResidualTolerance.Value ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidResult())
        from calibrated in CalibrateTetSignedHeat(domain: activeDomain, raw: poissonSolve.Solution, key: key)
        let fem = new TetFemReceipt(
            VertexCount: activeDomain.Vertices.Count, CellCount: activeDomain.Cells.Count, BoundaryVertexCount: activeDomain.BoundaryVertices.Count,
            BoundaryFaceCount: activeDomain.BoundaryFaceCount, InteriorVertexCount: activeDomain.InteriorVertexCount, IncidenceCount: activeDomain.Cells.Count * 16,
            TotalVolume: activeDomain.TotalVolume, MinCellVolume: Enumerable.Min(activeDomain.CellVolumes.AsIterable()), MaxCellVolume: Enumerable.Max(activeDomain.CellVolumes.AsIterable()),
            MassNonZeros: assembly.Mass.NonZeros, StiffnessNonZeros: assembly.Stiffness.NonZeros, HeatOperatorNonZeros: assembly.HeatOperator.NonZeros,
            DivergenceNonZeros: divergence.NonZeros, RejectedGradientCellCount: divergence.RejectedGradientCellCount)
        let receipt = new TetSignedHeatReceipt(Fem: fem, Heat: activePolicy.Heat, Solver: activePolicy.Solver, SignConvention: activePolicy.SignConvention, Gauge: activePolicy.Gauge, Interpolation: activePolicy.Interpolation, GaugeVertex: assembly.GaugeVertex, HeatTime: assembly.HeatTime, BoundaryShift: calibrated.BoundaryShift, InteriorMean: calibrated.InteriorMean, HeatSolve: heatSolve, PoissonSolve: poissonSolve)
        select new TetSignedHeatSolution(Values: calibrated.Values, Receipt: receipt);
    private static Fin<TetAssembly> AssembleTetFem(TetMeshDomain domain, TetSignedHeatPolicy policy, Op key) {
        Point3d[] points = [.. domain.Vertices.AsIterable()];
        TetCell[] cells = [.. domain.Cells.AsIterable()];
        System.Collections.Generic.HashSet<int> boundary = [.. domain.BoundaryVertices.AsIterable()];
        double heatTime = policy.Heat.Resolve(cellSize: Math.Pow(x: domain.TotalVolume / cells.Length, y: 1.0 / 3.0));
        if (!RhinoMath.IsValidDouble(x: heatTime) || heatTime <= RhinoMath.ZeroTolerance || domain.BoundaryVertices.IsEmpty) return Fin.Fail<TetAssembly>(key.InvalidInput());
        int vertexCount = points.Length;
        double[] heatRhs = new double[vertexCount];
        List<(int Row, int Col, double Value)> massTriplets = new(capacity: cells.Length * 16), stiffnessTriplets = new(capacity: cells.Length * 16);
        for (int c = 0; c < cells.Length; c++) {
            TetCell cell = cells[c];
            int[] ids = cell.Indices;
            TetCellMetric metric = TetMeshDomain.MetricOf(points: points, cell: cell, key: key).Match(Succ: static value => value, Fail: _ => default);
            if (metric.Gradients is null) return Fin.Fail<TetAssembly>(key.InvalidResult());
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++) {
                    double mass = metric.Volume * (i == j ? 2.0 : 1.0) / 20.0;
                    double stiffness = metric.Volume * (metric.Gradients[i] * metric.Gradients[j]);
                    massTriplets.Add(item: (ids[i], ids[j], mass));
                    stiffnessTriplets.Add(item: (ids[i], ids[j], stiffness));
                    heatRhs[ids[i]] += boundary.Contains(item: ids[j]) ? mass : 0.0;
                }
        }
        double[] massLumped = new double[vertexCount];
        foreach ((int row, int _, double value) in massTriplets) massLumped[row] += value;
        Dimension dim = Dimension.Create(value: vertexCount);
        return from mass in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: massTriplets, key: key)
               from stiffness in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: stiffnessTriplets, key: key)
               from heat in SparseMatrix.FromTriplets(rows: dim, cols: dim, triplets: massTriplets.Concat(stiffnessTriplets.Select(t => (t.Row, t.Col, Value: heatTime * t.Value))), key: key)
               select new TetAssembly(Mass: mass, Stiffness: stiffness, HeatOperator: heat, HeatRhs: new Arr<double>(heatRhs), MassLumped: new Arr<double>(massLumped), GaugeVertex: domain.BoundaryVertices[0], HeatTime: heatTime);
    }
    private static Fin<TetDivergence> TetDivergenceOf(TetMeshDomain domain, Arr<double> heat, Op key) {
        Point3d[] points = [.. domain.Vertices.AsIterable()];
        TetCell[] cells = [.. domain.Cells.AsIterable()];
        if (heat.Count != points.Length || !heat.ForAll(RhinoMath.IsValidDouble)) return Fin.Fail<TetDivergence>(key.InvalidResult());
        double[] rhs = new double[points.Length];
        int rejected = 0;
        for (int c = 0; c < cells.Length; c++) {
            TetCell cell = cells[c];
            int[] ids = cell.Indices;
            TetCellMetric metric = TetMeshDomain.MetricOf(points: points, cell: cell, key: key).Match(Succ: static value => value, Fail: _ => default);
            if (metric.Gradients is null) return Fin.Fail<TetDivergence>(key.InvalidResult());
            Vector3d gradient = Vector3d.Zero;
            for (int i = 0; i < 4; i++) gradient += heat[ids[i]] * metric.Gradients[i];
            if (!gradient.Unitize()) { rejected++; continue; }
            for (int i = 0; i < 4; i++) rhs[ids[i]] += -metric.Volume * (metric.Gradients[i] * gradient);
        }
        int nonZeros = rhs.Count(static value => Math.Abs(value: value) > RhinoMath.SqrtEpsilon);
        return nonZeros > 0 && rejected < cells.Length
            ? Fin.Succ(new TetDivergence(Rhs: new Arr<double>(rhs), NonZeros: nonZeros, RejectedGradientCellCount: rejected))
            : Fin.Fail<TetDivergence>(key.InvalidResult());
    }
    private static Fin<TetCalibration> CalibrateTetSignedHeat(TetMeshDomain domain, Arr<double> raw, Op key) {
        System.Collections.Generic.HashSet<int> boundary = [.. domain.BoundaryVertices.AsIterable()];
        if (raw.Count != domain.Vertices.Count || domain.InteriorVertexCount <= 0 || !raw.ForAll(RhinoMath.IsValidDouble)) return Fin.Fail<TetCalibration>(key.InvalidResult());
        double boundaryShift = domain.BoundaryVertices.Fold(initialState: 0.0, f: (sum, index) => sum + raw[index]) / domain.BoundaryVertices.Count;
        double interiorMean = toSeq(Enumerable.Range(start: 0, count: raw.Count))
            .Filter(index => !boundary.Contains(item: index))
            .Fold(initialState: 0.0, f: (sum, index) => sum + raw[index] - boundaryShift) / domain.InteriorVertexCount;
        double sign = interiorMean > 0.0 ? -1.0 : 1.0;
        Arr<double> values = new([.. raw.AsIterable().Select(value => sign * (value - boundaryShift))]);
        double signedInterior = toSeq(Enumerable.Range(start: 0, count: raw.Count))
            .Filter(index => !boundary.Contains(item: index))
            .Fold(initialState: 0.0, f: (sum, index) => sum + values[index]) / domain.InteriorVertexCount;
        return values.ForAll(RhinoMath.IsValidDouble) && RhinoMath.IsValidDouble(x: boundaryShift) && RhinoMath.IsValidDouble(x: signedInterior)
            ? Fin.Succ(new TetCalibration(Values: values, BoundaryShift: boundaryShift, InteriorMean: signedInterior))
            : Fin.Fail<TetCalibration>(key.InvalidResult());
    }
    private static Fin<TetSignedHeatSample> SampleTetSignedHeat(TetSignedHeatCase source, Point3d sample, Context context, Op key) =>
        from model in Optional(context).ToFin(key.MissingContext())
        from interpolated in InterpolateTet(domain: source.Domain, values: source.Values, sample: sample, tolerance: Math.Max(val1: model.Absolute.Value, val2: source.Domain.Context.Absolute.Value), key: key)
        select new TetSignedHeatSample(Value: source.Policy.SignConvention.Multiplier * interpolated.Value, Receipt: source.Receipt, Interpolation: interpolated.Receipt);
    private static Fin<TetInterpolated> InterpolateTet(TetMeshDomain domain, Arr<double> values, Point3d sample, double tolerance, Op key) {
        Point3d[] points = [.. domain.Vertices.AsIterable()];
        TetCell[] cells = [.. domain.Cells.AsIterable()];
        if (values.Count != points.Length || !FieldNabla.Finite(point: sample) || !RhinoMath.IsValidDouble(x: tolerance) || tolerance < 0.0) return Fin.Fail<TetInterpolated>(key.InvalidInput());
        for (int c = 0; c < cells.Length; c++) {
            TetCell cell = cells[c];
            TetCellMetric metric = TetMeshDomain.MetricOf(points: points, cell: cell, key: key).Match(Succ: static value => value, Fail: _ => default);
            if (metric.Gradients is null) return Fin.Fail<TetInterpolated>(key.InvalidResult());
            Vector3d offset = sample - points[cell.A];
            double b1 = metric.Gradients[1] * offset, b2 = metric.Gradients[2] * offset, b3 = metric.Gradients[3] * offset, b0 = 1.0 - b1 - b2 - b3;
            double[] barycentric = [b0, b1, b2, b3];
            if (barycentric.All(value => RhinoMath.IsValidDouble(x: value) && value >= -tolerance && value <= 1.0 + tolerance)) {
                int[] ids = cell.Indices;
                double interpolated = toSeq(Enumerable.Range(start: 0, count: 4)).Fold(initialState: 0.0, f: (sum, i) => sum + (barycentric[i] * values[ids[i]]));
                return RhinoMath.IsValidDouble(x: interpolated)
                    ? Fin.Succ(new TetInterpolated(Value: interpolated, Receipt: new TetInterpolationReceipt(Interpolation: TetInterpolation.Barycentric, CellIndex: c, Barycentric: new Arr<double>(barycentric), Inside: true)))
                    : Fin.Fail<TetInterpolated>(key.InvalidResult());
            }
        }
        return Fin.Fail<TetInterpolated>(key.InvalidInput());
    }
    private static Fin<double> EvaluateRbf(Seq<(Point3d Position, double Value)> samples, KernelKind kernel, double radius, Arr<double> coefficients, Point3d sample, Op key) =>
        coefficients.Count != samples.Count
            ? Fin.Fail<double>(key.InvalidResult())
            : from weights in KernelWeights(left: [sample], right: samples.AsIterable().Select(static s => s.Position), kernel: kernel, radius: radius, key: key)
              from value in key.AcceptValue(value: Enumerable.Zip(first: weights.AsIterable(), second: coefficients.AsIterable(), resultSelector: static (double w, double c) => w * c).Sum())
              select value;
    private static Fin<Arr<double>> KernelWeights(IEnumerable<Point3d> left, IEnumerable<Point3d> right, KernelKind kernel, double radius, Op key) =>
        toSeq(left.SelectMany(l => right.Select(r => l.DistanceTo(other: r))))
            .TraverseM(distance => kernel.Profile(distance: distance, radius: radius, key: key).Map(static profile => profile.Value)).As()
            .Map(static values => new Arr<double>([.. values.AsIterable()]));
    private static Fin<ReconstructionSample> EvaluateMls(Seq<MlsSample> samples, KernelKind kernel, double radius, Point3d sample, Context context, Op key) {
        return (FieldNabla.Finite(point: sample) ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput())).Bind(_ => {
            (MlsSample Sample, Vector3d Offset, KernelProfile Profile)[] neighborhood = [.. toSeq(samples.AsIterable().Select(candidate => (Sample: candidate, Offset: sample - candidate.Position))
                .Select(candidate => (candidate.Sample, candidate.Offset, Profile: kernel.Profile(distance: candidate.Offset.Length, radius: radius, key: key))))
                .Choose(static candidate => candidate.Profile.Match(Succ: profile => Some((candidate.Sample, candidate.Offset, Profile: profile)), Fail: _ => Option<(MlsSample Sample, Vector3d Offset, KernelProfile Profile)>.None))
                .Filter(candidate => candidate.Profile.Value > Math.Max(val1: context.Relative.Value, val2: RhinoMath.SqrtEpsilon)).AsIterable()];
            int rejected = samples.Count - neighborhood.Length;
            double weightSum = neighborhood.Sum(static candidate => candidate.Profile.Value);
            return guard(neighborhood.Length >= 3 && RhinoMath.IsValidDouble(x: weightSum) && weightSum > RhinoMath.ZeroTolerance, key.InvalidInput())
            .Bind(_ => {
                Arr<double> rhs = new([.. neighborhood.SelectMany(static candidate => {
                    double root = Math.Sqrt(d: candidate.Profile.Value);
                    return new[] { root * candidate.Sample.Value, root * candidate.Sample.Normal.X, root * candidate.Sample.Normal.Y, root * candidate.Sample.Normal.Z };
                })]);
                Arr<double> entries = new([.. neighborhood.SelectMany(static candidate => {
                    double root = Math.Sqrt(d: candidate.Profile.Value);
                    return new[] {
                        root, -root * candidate.Offset.X, -root * candidate.Offset.Y, -root * candidate.Offset.Z,
                        0.0, root, 0.0, 0.0,
                        0.0, 0.0, root, 0.0,
                        0.0, 0.0, 0.0, root,
                    };
                })]);
                return Matrix.Of(rows: Dimension.Create(value: 4 * neighborhood.Length), cols: Dimension.Create(value: 4), entries: entries, key: key).Bind((Matrix design) =>
                    design.LeastSquaresDetailed(rhs: rhs, key: key).Bind((SolveReceipt solve) => design.DecomposeSvd(key: key).Bind((SvdResult svd) => {
                        int rank = svd.Rank;
                        double[] positive = [.. svd.Sigma.AsIterable().Where(static (double s) => s > RhinoMath.SqrtEpsilon)];
                        Option<double> condition = positive.Length == 0 ? Option<double>.None : Some(Enumerable.Max(source: positive) / Enumerable.Min(source: positive));
                        double value = solve.Solution.Count > 0 ? solve.Solution[index: 0] : double.NaN;
                        Vector3d weightedNormal = neighborhood.Aggregate(seed: Vector3d.Zero, func: static (Vector3d sum, (MlsSample Sample, Vector3d Offset, KernelProfile Profile) candidate) => sum + (candidate.Profile.Value * candidate.Sample.Normal));
                        Vector3d gradient = solve.Solution.Count >= 4 ? new Vector3d(x: solve.Solution[index: 1], y: solve.Solution[index: 2], z: solve.Solution[index: 3]) : weightedNormal / weightSum;
                        double gradientNorm = gradient.Length;
                        double weightedNorm = weightedNormal.Length;
                        Vector3d direction = gradientNorm > RhinoMath.ZeroTolerance ? gradient / gradientNorm : weightedNorm > RhinoMath.ZeroTolerance ? weightedNormal / weightedNorm : Vector3d.Zero;
                        double normalAgreement = neighborhood.Average(((MlsSample Sample, Vector3d Offset, KernelProfile Profile) candidate) => Math.Abs(value: candidate.Sample.Normal * direction));
                        return rank >= 4 && RhinoMath.IsValidDouble(x: value) && RhinoMath.IsValidDouble(x: gradientNorm) && normalAgreement >= 0.5
                            ? Fin.Succ(new ReconstructionSample(Value: value, Receipt: new ReconstructionSampleReceipt(Mode: ReconstructionMode.MovingLeastSquares, Status: ReconstructionStatus.ApproximateSdf, Kernel: kernel, Radius: radius, SampleCount: samples.Count, NeighborhoodCount: neighborhood.Length, RejectedWeightCount: rejected, WeightSum: weightSum, Rank: rank, Condition: condition, NormalAgreement: Some(normalAgreement), GradientNorm: Some(gradientNorm), Solve: solve)))
                            : Fin.Fail<ReconstructionSample>(key.InvalidResult());
                    })));
            });
        });
    }
    private static Fin<SdfSample> SampleProfileExtrusion(ProfileExtrusionCase source, Point3d sample, Context context, Op key) =>
        source.Plane.RemapToPlaneSpace(ptSample: sample, ptPlane: out Point3d local) switch {
            false => Fin.Fail<SdfSample>(key.InvalidResult()),
            true when source.Plane.PointAt(u: local.X, v: local.Y) is Point3d planar => source.Profile.ClosestPoint(testPoint: planar, t: out double curveParameter) switch {
                false => Fin.Fail<SdfSample>(key.InvalidResult()),
                true => source.Profile.Contains(testPoint: planar, plane: source.Plane, tolerance: context.Absolute.Value) switch {
                    PointContainment.Unset => Fin.Fail<SdfSample>(key.InvalidResult()),
                    PointContainment containment when planar.DistanceTo(other: source.Profile.PointAt(t: curveParameter)) is double dxy
                        && (containment switch { PointContainment.Inside => -dxy, PointContainment.Coincident => 0.0, _ => dxy }, Math.Abs(value: local.Z) - source.HalfHeight.Value) is (double profile, double cap)
                        && (Math.Max(val1: profile, val2: 0.0), Math.Max(val1: cap, val2: 0.0)) is (double px, double pz) =>
                        key.AcceptValue(value: Math.Sqrt(d: (px * px) + (pz * pz)) + Math.Min(val1: Math.Max(val1: profile, val2: cap), val2: 0.0))
                            .Map(distance => new SdfSample(Value: distance, Receipt: SdfReceiptOf(field: source, status: SdfStatus.NativeProfile, mesh: Option<SdfMeshReceipt>.None) with {
                                NativeProfile = true,
                                ToleranceSource = Some(ToleranceSource.Context),
                                Tolerance = Some(context.Absolute.Value),
                                ClosestAccepted = true,
                                ProfileContainment = Some(containment),
                                ProfileFeature = Some((Math.Abs(value: profile) <= context.Absolute.Value, Math.Abs(value: cap) <= context.Absolute.Value) switch {
                                    (true, true) => ProfileExtrusionFeature.Rim,
                                    (true, false) => ProfileExtrusionFeature.ProfileBoundary,
                                    (false, true) => ProfileExtrusionFeature.Cap,
                                    _ => ProfileExtrusionFeature.Interior,
                                }),
                            })),
                    _ => Fin.Fail<SdfSample>(key.InvalidResult()),
                },
            },
            _ => Fin.Fail<SdfSample>(key.InvalidResult()),
        };
    private static SdfReceipt SdfReceiptOf(ScalarField field, SdfStatus status, Option<SdfMeshReceipt> mesh) =>
        new(Status: status, LipschitzBound: field.LipschitzBound(), AnalyticPrimitive: field is PrimitiveCase, MeshBacked: field is SignedDistanceFromMeshCase, WatertightPreflight: mesh.Map(static receipt => receipt.Topology.IsWatertight), LossyFallback: status.Equals(SdfStatus.LossyFallback), Mesh: mesh);
}

[SmartEnum<int>]
public sealed partial class SdfKind {
    public static readonly SdfKind Sphere = new(key: 0, lipschitz: 1.0, requiredKeys: Seq("r"),
        validate: static ps => ps["r"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y) + (p.Z * p.Z)) - ps["r"]);
    public static readonly SdfKind Box = new(key: 1, lipschitz: 1.0, requiredKeys: Seq("x", "y", "z"),
        validate: static ps => ps["x"] > RhinoMath.ZeroTolerance && ps["y"] > RhinoMath.ZeroTolerance && ps["z"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => {
            double qx = Math.Abs(value: p.X) - ps["x"]; double qy = Math.Abs(value: p.Y) - ps["y"]; double qz = Math.Abs(value: p.Z) - ps["z"];
            double ox = Math.Max(val1: qx, val2: 0.0); double oy = Math.Max(val1: qy, val2: 0.0); double oz = Math.Max(val1: qz, val2: 0.0);
            return Math.Sqrt(d: (ox * ox) + (oy * oy) + (oz * oz)) + Math.Min(val1: Math.Max(val1: qx, val2: Math.Max(val1: qy, val2: qz)), val2: 0.0);
        });
    public static readonly SdfKind Capsule = new(key: 2, lipschitz: 1.0, requiredKeys: Seq("h", "r"),
        validate: static ps => ps["h"] > RhinoMath.ZeroTolerance && ps["r"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => (p.Z - Math.Clamp(value: p.Z, min: -ps["h"], max: ps["h"])) switch { double pz => Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y) + (pz * pz)) - ps["r"] });
    public static readonly SdfKind Cylinder = new(key: 3, lipschitz: 1.0, requiredKeys: Seq("h", "r"),
        validate: static ps => ps["h"] > RhinoMath.ZeroTolerance && ps["r"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => (Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y)) - ps["r"], Math.Abs(value: p.Z) - ps["h"]) switch {
            (double dxy, double dz) => Math.Sqrt(d: (Math.Max(val1: dxy, val2: 0.0) * Math.Max(val1: dxy, val2: 0.0)) + (Math.Max(val1: dz, val2: 0.0) * Math.Max(val1: dz, val2: 0.0))) + Math.Min(val1: Math.Max(val1: dxy, val2: dz), val2: 0.0),
        });
    public static readonly SdfKind Cone = new(key: 4, lipschitz: 1.0, requiredKeys: Seq("h", "angle"),
        validate: static ps => ps["h"] > RhinoMath.ZeroTolerance && ps["angle"] > RhinoMath.ZeroTolerance && ps["angle"] < Math.PI,
        compute: static (p, ps) => SdfCappedConeCentered(p: new Point3d(x: p.X, y: p.Y, z: p.Z + (0.5 * ps["h"])), halfHeight: 0.5 * ps["h"], r1: ps["h"] * Math.Tan(a: ps["angle"]), r2: 0.0));
    public static readonly SdfKind HalfSpace = new(key: 5, lipschitz: 1.0, requiredKeys: Seq<string>(), validate: static _ => true, compute: static (p, _) => p.Z);
    public static readonly SdfKind CappedCone = new(key: 6, lipschitz: 1.2, requiredKeys: Seq("h", "r1", "r2"),
        validate: static ps => ps["h"] > RhinoMath.ZeroTolerance && ps["r1"] >= 0.0 && ps["r2"] >= 0.0 && (ps["r1"] > RhinoMath.ZeroTolerance || ps["r2"] > RhinoMath.ZeroTolerance),
        compute: static (p, ps) => SdfCappedConeCentered(p: p, halfHeight: ps["h"], r1: ps["r1"], r2: ps["r2"]));
    public static readonly SdfKind Torus = new(key: 7, lipschitz: 1.0, requiredKeys: Seq("R", "r"),
        validate: static ps => ps["R"] > RhinoMath.ZeroTolerance && ps["r"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => (Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y)) - ps["R"]) switch { double qx => Math.Sqrt(d: (qx * qx) + (p.Z * p.Z)) - ps["r"] });
    public static readonly SdfKind HexPrism = new(key: 8, lipschitz: 1.0, requiredKeys: Seq("h", "r"),
        validate: static ps => ps["h"] > RhinoMath.ZeroTolerance && ps["r"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => {
            double r = ps["r"];
            double k0866 = 0.8660254037844386; double k05 = 0.5;
            double qx = Math.Abs(value: p.X); double qy = Math.Abs(value: p.Y);
            double overflow = Math.Max(val1: (k0866 * qx) + (k05 * qy), val2: qy) - r;
            double dz = Math.Abs(value: p.Z) - ps["h"];
            return Math.Sqrt(d: (Math.Max(val1: overflow, val2: 0.0) * Math.Max(val1: overflow, val2: 0.0)) + (Math.Max(val1: dz, val2: 0.0) * Math.Max(val1: dz, val2: 0.0))) + Math.Min(val1: Math.Max(val1: overflow, val2: dz), val2: 0.0);
        });
    public static readonly SdfKind Octahedron = new(key: 9, lipschitz: 1.0, requiredKeys: Seq("s"),
        validate: static ps => ps["s"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => SdfExactOctahedron(p: p, s: ps["s"]));
    public static readonly SdfKind Ellipsoid = new(key: 10, lipschitz: 2.0, requiredKeys: Seq("x", "y", "z"),
        validate: static ps => ps["x"] > RhinoMath.ZeroTolerance && ps["y"] > RhinoMath.ZeroTolerance && ps["z"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => (ps["x"], ps["y"], ps["z"]) switch {
            (double ax, double ay, double az) when Math.Sqrt(d: (p.X * p.X / (ax * ax * ax * ax)) + (p.Y * p.Y / (ay * ay * ay * ay)) + (p.Z * p.Z / (az * az * az * az))) is double k1 && k1 > RhinoMath.ZeroTolerance => Math.Sqrt(d: (p.X * p.X / (ax * ax)) + (p.Y * p.Y / (ay * ay)) + (p.Z * p.Z / (az * az))) switch { double k0 => k0 * (k0 - 1.0) / k1 },
            _ => 0.0,
        });
    public static readonly SdfKind Slab = new(key: 11, lipschitz: 1.0, requiredKeys: Seq("h"), validate: static ps => ps["h"] > RhinoMath.ZeroTolerance, compute: static (p, ps) => Math.Abs(value: p.Z) - ps["h"]);
    public double Lipschitz { get; }
    public Seq<string> RequiredKeys { get; }
    [UseDelegateFromConstructor] private partial bool Validate(ImmutableDictionary<string, double> parameters);
    [UseDelegateFromConstructor] private partial double Compute(Point3d local, ImmutableDictionary<string, double> parameters);
    internal Fin<double> SignedDistance(Point3d worldPoint, ImmutableDictionary<string, double> parameters, Plane pose, Op key) =>
        pose.RemapToPlaneSpace(ptSample: worldPoint, ptPlane: out Point3d local) ? key.AcceptValue(value: Compute(local: local, parameters: parameters)) : Fin.Fail<double>(key.InvalidResult());
    internal bool ValidateParameters(ImmutableDictionary<string, double> parameters) => parameters.Count == RequiredKeys.Count && RequiredKeys.ForAll(k => parameters.ContainsKey(key: k) && RhinoMath.IsValidDouble(x: parameters[k])) && Validate(parameters: parameters);
    private static double SdfCappedConeCentered(Point3d p, double halfHeight, double r1, double r2) {
        double qx = Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y));
        Vector2d q = new(x: qx, y: p.Z);
        Vector2d k1 = new(x: r2, y: halfHeight);
        Vector2d k2 = new(x: r2 - r1, y: 2.0 * halfHeight);
        Vector2d ca = new(x: q.X - Math.Min(val1: q.X, val2: q.Y < 0.0 ? r1 : r2), y: Math.Abs(value: q.Y) - halfHeight);
        double k2LengthSquared = k2 * k2;
        double t = Math.Clamp(value: (k1 - q) * k2 / k2LengthSquared, min: 0.0, max: 1.0);
        Vector2d cb = q - k1 + (t * k2);
        double sign = cb.X < 0.0 && ca.Y < 0.0 ? -1.0 : 1.0;
        return sign * Math.Sqrt(d: Math.Min(val1: ca * ca, val2: cb * cb));
    }
    private static double SdfExactOctahedron(Point3d p, double s) =>
        (Math.Abs(value: p.X), Math.Abs(value: p.Y), Math.Abs(value: p.Z)) switch {
            (double ax, double ay, double az) when ax + ay + az - s is double m && 3.0 * ax < m => SdfOctant(qx: ax, qy: ay, qz: az, s: s),
            (double ax, double ay, double az) when ax + ay + az - s is double m && 3.0 * ay < m => SdfOctant(qx: ay, qy: az, qz: ax, s: s),
            (double ax, double ay, double az) when ax + ay + az - s is double m && 3.0 * az < m => SdfOctant(qx: az, qy: ax, qz: ay, s: s),
            (double ax, double ay, double az) => (ax + ay + az - s) * 0.5773502691896258,
        };
    private static double SdfOctant(double qx, double qy, double qz, double s) =>
        Math.Clamp(value: 0.5 * (qz - qy + s), min: 0.0, max: s) switch { double k => Math.Sqrt(d: (qx * qx) + ((qy - s + k) * (qy - s + k)) + ((qz - k) * (qz - k))) };
}

[SmartEnum<int>]
public sealed partial class SdfMeshMethod {
    public static readonly SdfMeshMethod GeneralizedWindingNumber = new(key: 0, status: SdfMeshStatus.ApproximateSignClosestDistance, domain: SdfMeshDomain.SurfaceMesh);
    public static readonly SdfMeshMethod BoundarySignedHeat = new(key: 1, status: SdfMeshStatus.BoundarySourceSignedHeat, domain: SdfMeshDomain.BoundarySource);
    public static readonly SdfMeshMethod ClosedSurfaceSignedHeat = new(key: 2, status: SdfMeshStatus.ClosedSurfaceSignedHeat, domain: SdfMeshDomain.VolumeGrid);
    public SdfMeshStatus Status { get; }
    public SdfMeshDomain Domain { get; }
}

[SmartEnum<int>]
public sealed partial class SdfSignConvention {
    public static readonly SdfSignConvention NegativeInsidePositiveOutside = new(key: 0, multiplier: 1.0);
    public static readonly SdfSignConvention PositiveInsideNegativeOutside = new(key: 1, multiplier: -1.0);
    public double Multiplier { get; }
}

[SmartEnum<int>]
public sealed partial class SdfStatus {
    public static readonly SdfStatus Analytic = new(key: 0);
    public static readonly SdfStatus ComposedAnalytic = new(key: 1);
    public static readonly SdfStatus MeshApproximate = new(key: 2);
    public static readonly SdfStatus LossyFallback = new(key: 3);
    public static readonly SdfStatus NativeProfile = new(key: 4);
    public static readonly SdfStatus TetSignedHeat = new(key: 5);
}

[Union]
public abstract partial record TensorField {
    private TensorField() { }
    public sealed record ConstantCase : TensorField { internal ConstantCase(SymmetricMatrix Value) => this.Value = Value; public SymmetricMatrix Value { get; } }
    public sealed record CurvatureCase : TensorField { internal CurvatureCase(SurfaceSpace Space) => this.Space = Space; public SurfaceSpace Space { get; } }
    public sealed record LiftCase : TensorField { internal LiftCase(Func<Point3d, SymmetricMatrix> Sampler) => this.Sampler = Sampler; public Func<Point3d, SymmetricMatrix> Sampler { get; } }
    public sealed record WarpCase : TensorField { internal WarpCase(TensorField Source, Transform Spatial) { this.Source = Source; this.Spatial = Spatial; } public TensorField Source { get; } public Transform Spatial { get; } }
    public sealed record ScaledCase : TensorField { internal ScaledCase(TensorField Source, double Scale) { this.Source = Source; this.Scale = Scale; } public TensorField Source { get; } public double Scale { get; } }
    public sealed record BlendCase : TensorField { internal BlendCase(Seq<TensorField> Fields, FieldBlend Mode) { this.Fields = Fields; this.Mode = Mode; } public Seq<TensorField> Fields { get; } public FieldBlend Mode { get; } }
    public static TensorField Constant(SymmetricMatrix value) => new ConstantCase(Value: value);
    public static Fin<TensorField> Lift(Func<Point3d, SymmetricMatrix>? sampler, Op? key = null) =>
        Optional(sampler).ToFin(key.OrDefault().InvalidInput()).Map(active => (TensorField)new LiftCase(Sampler: active));
    internal Fin<SymmetricMatrix> SampleTensor(Point3d sample, Context context, Op key) =>
        FieldNabla.Finite(point: sample, key: key).Bind(finiteSample => Switch(
        state: (Sample: sample, Context: context, Key: key),
        constantCase: static (s, c) => c.Value.IsValid ? Fin.Succ(c.Value) : Fin.Fail<SymmetricMatrix>(s.Key.InvalidResult()),
        curvatureCase: static (s, c) => FieldNabla.SurfaceNative(space: c.Space, key: s.Key).Bind(surface => surface.ClosestPoint(testPoint: s.Sample, u: out double u, v: out double v) switch {
            false => Fin.Fail<SymmetricMatrix>(error: s.Key.InvalidResult()),
            true => SurfaceProjection.ShapeOperator.Project<SymmetricMatrix>(surface: surface, u: u, v: v, context: c.Space.Tolerance, key: s.Key),
        }),
        liftCase: static (s, c) => s.Key.Catch(() => {
            SymmetricMatrix value = c.Sampler(arg: s.Sample);
            return value.IsValid ? Fin.Succ(value) : Fin.Fail<SymmetricMatrix>(s.Key.InvalidResult());
        }),
        warpCase: static (s, c) => c.Spatial.TryGetInverse(inverseTransform: out Transform inverse)
            ? c.Source.SampleTensor(sample: inverse * s.Sample, context: s.Context, key: s.Key)
                .Bind(tensor => TransformTensor(tensor: tensor, spatial: c.Spatial, key: s.Key))
            : Fin.Fail<SymmetricMatrix>(error: s.Key.InvalidResult()),
        scaledCase: static (s, c) => c.Source.SampleTensor(sample: s.Sample, context: s.Context, key: s.Key)
            .Bind(m => SymmetricMatrix.Of(dim: m.Dimension, upper: [.. m.Upper.AsIterable().Select(v => v * c.Scale)], key: s.Key)),
        blendCase: static (s, c) => c.Fields.TraverseM(f => f.SampleTensor(sample: s.Sample, context: s.Context, key: s.Key)).As()
            .Bind(tensors => BlendTensors(tensors: tensors, mode: c.Mode, key: s.Key))));
    private static Fin<SymmetricMatrix> BlendTensors(Seq<SymmetricMatrix> tensors, FieldBlend mode, Op key) =>
        tensors.IsEmpty || tensors.Exists(t => !t.IsValid || t.Dimension.Value != tensors[index: 0].Dimension.Value)
            ? Fin.Fail<SymmetricMatrix>(error: key.InvalidResult())
            : from upper in toSeq(Enumerable.Range(start: 0, count: tensors[index: 0].Upper.Count))
                .TraverseM(i => mode.CombineScalar(values: tensors.Map(tensor => tensor.Upper[index: i]), key: key)).As()
              from matrix in SymmetricMatrix.Of(dim: tensors[index: 0].Dimension, upper: new Arr<double>([.. upper.AsIterable()]), key: key)
              select matrix;
    internal Fin<Seq<(double Eigenvalue, Direction Eigenvector)>> PrincipalDirections(Point3d sample, Context context, Op key) =>
        from tensor in SampleTensor(sample: sample, context: context, key: key)
        from _ in tensor.Dimension.Value == 3 ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput())
        from eigen in tensor.DecomposeEigen(key: key)
        from directions in eigen.TraverseM(pair => Direction.Of(value: CloudKernel.AsVector3d(v: pair.Eigenvector), context: context, key: key).Map(d => (pair.Eigenvalue, Eigenvector: d))).As()
        select directions;
    private static Fin<SymmetricMatrix> TransformTensor(SymmetricMatrix tensor, Transform spatial, Op key) =>
        tensor.Dimension.Value != 3
            ? Fin.Fail<SymmetricMatrix>(key.InvalidInput())
            : from rotation in Matrix.Of(rows: tensor.Dimension, cols: tensor.Dimension, entries: new Arr<double>([spatial[0, 0], spatial[0, 1], spatial[0, 2], spatial[1, 0], spatial[1, 1], spatial[1, 2], spatial[2, 0], spatial[2, 1], spatial[2, 2]]), key: key)
              from left in rotation.Multiply(other: tensor.ToDense(), key: key)
              from transformed in left.Multiply(other: rotation.Transpose(), key: key)
              from output in SymmetricMatrix.Of(dim: tensor.Dimension, upper: new Arr<double>([
                  transformed.At(i: 0, j: 0), transformed.At(i: 0, j: 1), transformed.At(i: 0, j: 2),
                  transformed.At(i: 1, j: 1), transformed.At(i: 1, j: 2), transformed.At(i: 2, j: 2),
              ]), key: key)
              select output;
}

[SmartEnum<int>]
public sealed partial class TetGaugePolicy { public static readonly TetGaugePolicy PinnedFirstBoundary = new(key: 0); }

[SmartEnum<int>]
public sealed partial class TetInterpolation { public static readonly TetInterpolation Barycentric = new(key: 0); }

[Union]
public abstract partial record VectorField {
    private VectorField() { }
    public sealed record ConstantCase : VectorField { internal ConstantCase(Vector3d Value) => this.Value = Value; public Vector3d Value { get; } }
    public sealed record BlendCase : VectorField { internal BlendCase(Seq<VectorField> Fields, FieldBlend Mode) { this.Fields = Fields; this.Mode = Mode; } public Seq<VectorField> Fields { get; } public FieldBlend Mode { get; } }
    public sealed record ScaledCase : VectorField { internal ScaledCase(VectorField Source, double Scale) { this.Source = Source; this.Scale = Scale; } public VectorField Source { get; } public double Scale { get; } }
    public sealed record InfluenceCase : VectorField { internal InfluenceCase(SupportSpace Source, Falloff Falloff, BoundarySense Sense, Option<PositiveMagnitude> Radius) { this.Source = Source; this.Falloff = Falloff; this.Sense = Sense; this.Radius = Radius; } public SupportSpace Source { get; } public Falloff Falloff { get; } public BoundarySense Sense { get; } public Option<PositiveMagnitude> Radius { get; } }
    public sealed record HitFieldCase : VectorField { internal HitFieldCase(SupportSpace Source, SupportProjection Projection, BoundarySense Sense) { this.Source = Source; this.Projection = Projection; this.Sense = Sense; } public SupportSpace Source { get; } public SupportProjection Projection { get; } public BoundarySense Sense { get; } }
    public sealed record VortexCase : VectorField { internal VortexCase(Point3d Anchor, Direction Axis, Falloff Falloff) { this.Anchor = Anchor; this.Axis = Axis; this.Falloff = Falloff; } public Point3d Anchor { get; } public Direction Axis { get; } public Falloff Falloff { get; } }
    public sealed record RingCase : VectorField { internal RingCase(Point3d Center, Direction Axis, PositiveMagnitude Radius, Falloff Falloff) { this.Center = Center; this.Axis = Axis; this.Radius = Radius; this.Falloff = Falloff; } public Point3d Center { get; } public Direction Axis { get; } public PositiveMagnitude Radius { get; } public Falloff Falloff { get; } }
    public sealed record HelicalCase : VectorField { internal HelicalCase(Point3d Anchor, Direction Axis, double Axial, double Swirl, Falloff Falloff) { this.Anchor = Anchor; this.Axis = Axis; this.Axial = Axial; this.Swirl = Swirl; this.Falloff = Falloff; } public Point3d Anchor { get; } public Direction Axis { get; } public double Axial { get; } public double Swirl { get; } public Falloff Falloff { get; } }
    public sealed record CoulombCase : VectorField { internal CoulombCase(Seq<(Point3d Position, double Charge)> Charges, Falloff Falloff) { this.Charges = Charges; this.Falloff = Falloff; } public Seq<(Point3d Position, double Charge)> Charges { get; } public Falloff Falloff { get; } }
    public sealed record ClusterFieldCase : VectorField { internal ClusterFieldCase(VectorCloud.ClusterCase Source, Falloff Falloff, PositiveMagnitude Radius, BoundarySense Sense) { this.Source = Source; this.Falloff = Falloff; this.Radius = Radius; this.Sense = Sense; } public VectorCloud.ClusterCase Source { get; } public Falloff Falloff { get; } public PositiveMagnitude Radius { get; } public BoundarySense Sense { get; } }
    public sealed record DipoleCase : VectorField { internal DipoleCase(Point3d Origin, Direction Moment, PositiveMagnitude Strength) { this.Origin = Origin; this.Moment = Moment; this.Strength = Strength; } public Point3d Origin { get; } public Direction Moment { get; } public PositiveMagnitude Strength { get; } }
    public sealed record HarmonicCase : VectorField { internal HarmonicCase(Seq<(Direction Direction, double Frequency, double Phase, double Amplitude)> Components) => this.Components = Components; public Seq<(Direction Direction, double Frequency, double Phase, double Amplitude)> Components { get; } }
    public sealed record ProjectedCase : VectorField { internal ProjectedCase(VectorField Source, Plane Onto) { this.Source = Source; this.Onto = Onto; } public VectorField Source { get; } public Plane Onto { get; } }
    public sealed record WarpCase : VectorField { internal WarpCase(VectorField Source, Transform Spatial) { this.Source = Source; this.Spatial = Spatial; } public VectorField Source { get; } public Transform Spatial { get; } }
    public sealed record ClampMagnitudeCase : VectorField { internal ClampMagnitudeCase(VectorField Source, PositiveMagnitude Min, PositiveMagnitude Max) { this.Source = Source; this.Min = Min; this.Max = Max; } public VectorField Source { get; } public PositiveMagnitude Min { get; } public PositiveMagnitude Max { get; } }
    public sealed record GradientCase : VectorField { internal GradientCase(ScalarField Source, PositiveMagnitude Epsilon) { this.Source = Source; this.Epsilon = Epsilon; } public ScalarField Source { get; } public PositiveMagnitude Epsilon { get; } }
    public sealed record CurlCase : VectorField { internal CurlCase(VectorField Source, PositiveMagnitude Epsilon) { this.Source = Source; this.Epsilon = Epsilon; } public VectorField Source { get; } public PositiveMagnitude Epsilon { get; } }
    public sealed record CurlNoiseCase : VectorField { internal CurlNoiseCase(ScalarField Potential, PositiveMagnitude Epsilon, bool RaisesCaution) { this.Potential = Potential; this.Epsilon = Epsilon; this.RaisesCaution = RaisesCaution; } public ScalarField Potential { get; } public PositiveMagnitude Epsilon { get; } public bool RaisesCaution { get; } }
    public sealed record CrossProductCase : VectorField { internal CrossProductCase(VectorField Left, VectorField Right) { this.Left = Left; this.Right = Right; } public VectorField Left { get; } public VectorField Right { get; } }
    public sealed record BiotSavartCase : VectorField { internal BiotSavartCase(Point3d Start, Point3d End, double Current) { this.Start = Start; this.End = End; this.Current = Current; } public Point3d Start { get; } public Point3d End { get; } public double Current { get; } }
    public sealed record SaddleCase : VectorField { internal SaddleCase(Point3d Anchor, Plane Basis, double Strength) { this.Anchor = Anchor; this.Basis = Basis; this.Strength = Strength; } public Point3d Anchor { get; } public Plane Basis { get; } public double Strength { get; } }
    public sealed record CrossFieldCase : VectorField { internal CrossFieldCase(MeshSpace Space, int Symmetry, Option<Seq<(int Vertex, Direction Hint)>> Constraints, Option<Seq<(int Vertex, double HolonomyDeficit)>> Cones) { this.Space = Space; this.Symmetry = Symmetry; this.Constraints = Constraints; this.Cones = Cones; } public MeshSpace Space { get; } public int Symmetry { get; } public Option<Seq<(int Vertex, Direction Hint)>> Constraints { get; } public Option<Seq<(int Vertex, double HolonomyDeficit)>> Cones { get; } }
    public sealed record HodgeCase : VectorField { internal HodgeCase(VectorField Source, MeshSpace Space, BoundarySense Sense) { this.Source = Source; this.Space = Space; this.Sense = Sense; } public VectorField Source { get; } public MeshSpace Space { get; } public BoundarySense Sense { get; } }
    public sealed record VectorHeatCase : VectorField { internal VectorHeatCase(MeshSpace Space, Seq<(int Vertex, Vector3d Direction)> Sources, PositiveMagnitude Time) { this.Space = Space; this.Sources = Sources; this.Time = Time; } public MeshSpace Space { get; } public Seq<(int Vertex, Vector3d Direction)> Sources { get; } public PositiveMagnitude Time { get; } }
    public sealed record GeodesicTangentCase : VectorField { internal GeodesicTangentCase(MeshSpace Space, Seq<int> Sources) { this.Space = Space; this.Sources = Sources; } public MeshSpace Space { get; } public Seq<int> Sources { get; } }
    public sealed record TangentLogMapCase : VectorField { internal TangentLogMapCase(MeshSpace Space, int Source, PositiveMagnitude Time) { this.Space = Space; this.Source = Source; this.Time = Time; } public MeshSpace Space { get; } public int Source { get; } public PositiveMagnitude Time { get; } }
    public static VectorField Constant(Vector3d value) => new ConstantCase(Value: value);
    public static VectorField Blend(Seq<VectorField> fields, FieldBlend? blend = null) =>
        new BlendCase(Fields: fields, Mode: blend ?? FieldBlend.Sum);
    public static Fin<VectorField> Divide(VectorField source, double divisor, Op? key = null) =>
        FieldNabla.WithDivisor(divisor: divisor, make: scale => (VectorField)new ScaledCase(Source: source, Scale: scale), key: key);
    public static Fin<VectorField> Shell(SupportSpace source, double radius, Falloff? falloff = null, BoundarySense? sense = null, Op? key = null) =>
        FieldNabla.WithPositive(candidate: radius, make: value => (VectorField)new InfluenceCase(Source: source, Falloff: falloff ?? Falloff.Constant, Sense: sense ?? BoundarySense.Toward, Radius: Some(value)), key: key);
    public static Fin<VectorField> Hit(SupportSpace source, SupportProjection projection, BoundarySense? sense = null, Op? key = null) =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput()) from selected in Optional(projection).ToFin(key.OrDefault().InvalidInput()) from _ in guard(selected.CanProjectVector(space: active), key.OrDefault().Unsupported(active.SourceType, typeof(Vector3d))) select (VectorField)new HitFieldCase(Source: active, Projection: selected, Sense: sense ?? BoundarySense.Toward);
    public static Fin<VectorField> Ring(Point3d center, Direction axis, double radius, Falloff? falloff = null, Op? key = null) =>
        from r in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius) from f in falloff is null ? Falloff.Gaussian(spread: radius / 3.0, key: key) : Fin.Succ(falloff) select (VectorField)new RingCase(Center: center, Axis: axis, Radius: r, Falloff: f);
    public static Fin<VectorField> Cluster(VectorCloud? cluster, double radius, Falloff? falloff = null, BoundarySense? sense = null, Op? key = null) =>
        Optional(cluster).ToFin(key.OrDefault().InvalidInput()).Bind(active => active switch { VectorCloud.ClusterCase c => from r in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius) from f in falloff is null ? Falloff.Gaussian(spread: r.Value / 3.0, key: key) : Fin.Succ(falloff) select (VectorField)new ClusterFieldCase(Source: c, Falloff: f, Radius: r, Sense: sense ?? BoundarySense.Toward), _ => Fin.Fail<VectorField>(key.OrDefault().Unsupported(geometryType: active.GetType(), outputType: typeof(VectorField))) });
    public static Fin<VectorField> Dipole(Point3d origin, Direction moment, double strength, Op? key = null) =>
        FieldNabla.WithPositive(candidate: strength, make: s => (VectorField)new DipoleCase(Origin: origin, Moment: moment, Strength: s), key: key);
    public static Fin<VectorField> ClampMagnitude(VectorField source, double min, double max, Op? key = null) =>
        from low in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: min) from high in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: max) from _ in guard(low.Value <= high.Value, key.OrDefault().InvalidInput()) select (VectorField)new ClampMagnitudeCase(Source: source, Min: low, Max: high);
    public static Fin<VectorField> Gradient(ScalarField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<ScalarField, VectorField>(source, epsilon, static (s, e) => new GradientCase(Source: s, Epsilon: e), key);
    public static Fin<VectorField> Curl(VectorField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<VectorField, VectorField>(source, epsilon, static (s, e) => new CurlCase(Source: s, Epsilon: e), key);
    public static Fin<VectorField> CurlNoise(ScalarField potential, double epsilon, Op? key = null) =>
        potential is ScalarField.NoiseCase { Kind: var kind } && kind == NoiseKind.Worley
            ? Fin.Fail<VectorField>(key.OrDefault().Unsupported(geometryType: typeof(ScalarField.NoiseCase), outputType: typeof(VectorField)))
            : from active in Optional(potential).ToFin(key.OrDefault().InvalidInput()) from eps in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: epsilon) select (VectorField)new CurlNoiseCase(Potential: active, Epsilon: eps, RaisesCaution: potential is ScalarField.NoiseCase nc && nc.Kind.RaisesCaution);
    public static Fin<VectorField> BiotSavart(Point3d start, Point3d end, double current, Op? key = null) =>
        from a in key.OrDefault().AcceptValue(value: start) from b in key.OrDefault().AcceptValue(value: end) from i in key.OrDefault().AcceptValue(value: current) from _ in guard(!(a - b).IsTiny(), key.OrDefault().InvalidInput()) select (VectorField)new BiotSavartCase(Start: a, End: b, Current: i);
    public static Fin<VectorField> Saddle(Point3d anchor, Plane basis, double strength, Op? key = null) =>
        FieldNabla.Plane(basis: basis, key: key.OrDefault()).Map(validBasis => (VectorField)new SaddleCase(Anchor: anchor, Basis: validBasis, Strength: strength));
    public static Fin<VectorField> CrossField(MeshSpace space, int symmetry, Option<Seq<(int Vertex, Direction Hint)>> constraints = default, Option<Seq<(int Vertex, double HolonomyDeficit)>> cones = default, Op? key = null) =>
        from active in FieldNabla.MeshOf(space: space, key: key.OrDefault()) from __ in guard(symmetry is 1 or 2 or 4 or 6, key.OrDefault().InvalidInput()) let vertexCount = active.Vertices.Count from ___ in guard(constraints.Match(Some: values => values.ForAll(item => item.Vertex >= 0 && item.Vertex < vertexCount), None: static () => true) && cones.Match(Some: values => values.ForAll(item => item.Vertex >= 0 && item.Vertex < vertexCount && RhinoMath.IsValidDouble(x: item.HolonomyDeficit)), None: static () => true), key.OrDefault().InvalidInput()) select (VectorField)new CrossFieldCase(Space: space, Symmetry: symmetry, Constraints: constraints, Cones: cones);
    public static Fin<VectorField> Hodge(VectorField source, MeshSpace space, BoundarySense? sense = null, Op? key = null) =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput()) from _ in FieldNabla.MeshOf(space: space, key: key.OrDefault()) select (VectorField)new HodgeCase(Source: active, Space: space, Sense: sense ?? BoundarySense.Toward);
    public static Fin<VectorField> VectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op? key = null) =>
        from _ in FieldNabla.MeshVertices(space: space, vertices: sources.Map(static s => s.Vertex), allowEmpty: false, key: key.OrDefault()) from t in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: time) select (VectorField)new VectorHeatCase(Space: space, Sources: sources, Time: t);
    public static Fin<VectorField> GeodesicTangent(MeshSpace space, Seq<int> sources, Op? key = null) =>
        FieldNabla.MeshVertices(space: space, vertices: sources, allowEmpty: false, key: key.OrDefault()).Map(_ => (VectorField)new GeodesicTangentCase(Space: space, Sources: sources));
    public static Fin<VectorField> TangentLogMap(MeshSpace space, int source, double time, Op? key = null) =>
        from _ in FieldNabla.MeshVertices(space: space, vertices: Seq(source), allowEmpty: false, key: key.OrDefault())
        from t in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: time)
        select (VectorField)new TangentLogMapCase(Space: space, Source: source, Time: t);
    public static VectorField operator +(VectorField left, VectorField right) =>
        new BlendCase(Fields: (left is BlendCase lb && lb.Mode.Equals(FieldBlend.Sum) ? lb.Fields : Seq(left)).Concat(right is BlendCase rb && rb.Mode.Equals(FieldBlend.Sum) ? rb.Fields : Seq(right)).ToSeq(), Mode: FieldBlend.Sum);
    public static VectorField operator -(VectorField left, VectorField right) => left + (-right);
    public static VectorField operator -(VectorField field) => new ScaledCase(Source: field, Scale: -1.0);
    public static VectorField operator *(VectorField field, double scale) => new ScaledCase(Source: field, Scale: scale);
    public static VectorField operator *(double scale, VectorField field) => new ScaledCase(Source: field, Scale: scale);
    internal Fin<Vector3d> SampleVector(Point3d sample, Context context, Op key) =>
        FieldNabla.Finite(point: sample, key: key).Bind(finiteSample => Switch(
        state: (Sample: sample, Context: context, Key: key),
        constantCase: static (state, c) => state.Key.AcceptValue(value: c.Value),
        influenceCase: static (state, c) => ClosestDirected(
            source: c.Source, sample: state.Sample, sense: c.Sense, context: state.Context, key: state.Key,
            hitToScaled: (hit, op) =>
                from distance in hit.Distance.ToFin(Fail: op.InvalidResult())
                let residual = c.Radius.Map(radius => Math.Abs(distance - radius.Value)).IfNone(distance)
                let shellSign = c.Radius.Map(radius => distance >= radius.Value ? 1.0 : -1.0).IfNone(1.0)
                from weight in c.Falloff.Weight(offset: hit.Point - state.Sample, sample: state.Sample, context: state.Context, tolerance: state.Context.Absolute.Value, key: op)
                select (Raw: shellSign * (hit.Point - state.Sample), Scale: c.Radius.IsSome ? residual * weight : weight)),
        hitFieldCase: static (state, c) =>
            from vector in ClosestDirected(
                source: c.Source, sample: state.Sample, sense: c.Sense, context: state.Context, key: state.Key,
                hitToScaled: (hit, op) => c.Projection.Equals(SupportProjection.Span) || c.Projection.Equals(SupportProjection.SignedSpanAway)
                    ? from span in c.Projection.Project<VectorSpan>(space: c.Source, hit: hit, sample: state.Sample, context: state.Context, key: op)
                      select (Raw: span.Direction.Value, Scale: span.Magnitude.Value)
                    : from raw in c.Projection.Project<Vector3d>(space: c.Source, hit: hit, sample: state.Sample, context: state.Context, key: op)
                      select (Raw: raw, Scale: 1.0))
            select vector,
        blendCase: static (state, c) => c.Fields.TraverseM(field => field.SampleVector(sample: state.Sample, context: state.Context, key: state.Key)).As()
            .Bind(vectors => c.Mode.Combine(vectors: vectors, key: state.Key)),
        vortexCase: static (state, c) => RotationalField(anchor: c.Anchor, axis: c.Axis, falloff: c.Falloff, axial: 0.0, swirl: 1.0, state: state),
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
            .Bind(v => state.Key.AcceptValue(value: Transform.PlanarProjection(plane: c.Onto) * v)),
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
        gradientCase: static (state, c) => FieldNabla.GradientAt(field: c.Source, point: state.Sample, eps: c.Epsilon.Value, context: state.Context, key: state.Key),
        curlCase: static (state, c) => FieldNabla.CurlAt(field: c.Source, point: state.Sample, eps: c.Epsilon.Value, context: state.Context, key: state.Key),
        ringCase: static (state, c) => RotationalField(anchor: c.Center, axis: c.Axis, falloff: c.Falloff, axial: 0.0, swirl: 1.0, state: state),
        helicalCase: static (state, c) => RotationalField(anchor: c.Anchor, axis: c.Axis, falloff: c.Falloff, axial: c.Axial, swirl: c.Swirl, state: state),
        biotSavartCase: static (state, c) => BiotSavartContribution(start: c.Start, end: c.End, current: c.Current, point: state.Sample, tol: state.Context.Absolute.Value, key: state.Key),
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
            select output,
        curlNoiseCase: static (state, c) => FieldNabla.CurlNoiseAt(field: c.Potential, point: state.Sample, eps: c.Epsilon.Value, context: state.Context, key: state.Key),
        crossFieldCase: static (state, c) => MeshKernel.CrossFieldAt(space: c.Space, symmetry: c.Symmetry, constraints: c.Constraints, cones: c.Cones, sample: state.Sample, key: state.Key),
        hodgeCase: static (state, c) => MeshKernel.HodgeProjectedAt(source: c.Source, space: c.Space, sense: c.Sense, sample: state.Sample, key: state.Key),
        vectorHeatCase: static (state, c) => MeshKernel.VectorHeatAt(space: c.Space, sources: c.Sources, time: c.Time.Value, sample: state.Sample, key: state.Key),
        geodesicTangentCase: static (state, c) => MeshKernel.GeodesicTangentAt(space: c.Space, sources: c.Sources, sample: state.Sample, key: state.Key),
        tangentLogMapCase: static (state, c) => MeshKernel.TangentLogMapAt(space: c.Space, source: c.Source, sample: state.Sample, time: c.Time.Value, key: state.Key).Map(static result => result.Tangent)));
    private static Fin<Vector3d> RotationalField(Point3d anchor, Direction axis, Falloff falloff, double axial, double swirl, (Point3d Sample, Context Context, Op Key) state) {
        Vector3d rPerp = FieldNabla.PerpendicularComponent(r: state.Sample - anchor, axis: axis.Value);
        return falloff.Weight(offset: rPerp, sample: state.Sample, context: state.Context, tolerance: state.Context.Absolute.Value, key: state.Key)
            .Bind(weight => state.Key.AcceptValue(value: weight * ((axial * axis.Value) + (swirl * Vector3d.CrossProduct(a: axis.Value, b: rPerp)))));
    }
    private static Fin<Vector3d> BiotSavartContribution(Point3d start, Point3d end, double current, Point3d point, double tol, Op key) {
        Vector3d wire = end - start;
        double wireLen = wire.Length;
        return wireLen < tol ? Fin.Fail<Vector3d>(key.InvalidInput()) : key.Catch(() => {
            Vector3d t = wire / wireLen;
            Vector3d r1 = point - start;
            Vector3d r2 = point - end;
            double r1Length = r1.Length;
            double r2Length = r2.Length;
            Vector3d perpVec = FieldNabla.PerpendicularComponent(r: r1, axis: t);
            double R = perpVec.Length;
            if (R < tol || r1Length < tol || r2Length < tol) return Fin.Fail<Vector3d>(key.InvalidInput());
            double angularFactor = (r1 * t / r1Length) - (r2 * t / r2Length);
            double prefactor = current / (4.0 * Math.PI * R);
            return key.AcceptValue(value: Vector3d.CrossProduct(a: t, b: perpVec / R) * prefactor * angularFactor);
        });
    }
    private static Fin<Vector3d> SampleRadialContribution(Vector3d sum, Point3d source, double scale, (Point3d Sample, Context Context, Op Key) state, Falloff falloff) {
        Vector3d r = state.Sample - source;
        return r.Length <= state.Context.Absolute.Value
            ? state.Key.AcceptValue(value: sum)
            : falloff.Weight(offset: r, tolerance: state.Context.Absolute.Value, key: state.Key)
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

[SmartEnum<int>]
public sealed partial class VolumeBoundaryCondition { public static readonly VolumeBoundaryCondition NeumannGaugePinned = new(key: 0); }

[SmartEnum<int>]
public sealed partial class VolumeInterpolation { public static readonly VolumeInterpolation Trilinear = new(key: 0); }

[SmartEnum<int>]
public sealed partial class VolumeSolverKind { public static readonly VolumeSolverKind SparseCholeskyPinned = new(key: 0); }

// --- [MODELS] -----------------------------------------------------------------------------
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct IsoSurfaceGrid(BoundingBox Bounds, int Resolution, long XCells, long YCells, long ZCells, double CellSize, long HexCellCount, long CornerSampleCount, long CenterSampleCount, long InitialSampleCount, long MaxCells);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct IsoSurfaceReceipt(bool NativeRouted, IsoSurfaceStatus Status, IsoSurfaceGrid Grid, int MaxRootSteps, bool ParallelCallback, int EvaluatorFailures, bool Valid, int VertexCount, int FaceCount, Option<double> FixedTolerance, Option<double> FixedNormalSampleDistance, Option<SdfMeshReceipt> MeshPreflight);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct IsoSurfaceResult(Mesh Mesh, IsoSurfaceReceipt Receipt);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct KernelProfile(double Value, double FirstDerivative, double SecondDerivative, KernelProfileStatus Status) { public bool IsValid => RhinoMath.IsValidDouble(x: Value) && RhinoMath.IsValidDouble(x: FirstDerivative) && RhinoMath.IsValidDouble(x: SecondDerivative); }

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct MlsSample(Point3d Position, Vector3d Normal, double Value);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ReconstructionAttempt(Option<ReconstructionResult> Result, Option<ReconstructionFailureReceipt> Failure);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ReconstructionFailureReceipt(ReconstructionMode Mode, ReconstructionFailureKind Kind, int SampleCount, bool RequiresNormals, bool RequiresSparseSystem, bool RhinoCommonGeneratorAvailable);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ReconstructionReceipt(ReconstructionMode Mode, KernelKind Kernel, double Radius, double Smoothing, bool Interpolation, int SampleCount, int CenterCount, int PolynomialDegree, Option<SolveReceipt> Solve);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ReconstructionResult(ScalarField Field, ReconstructionReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Op key) {
        ReconstructionResult self = this;
        return AtomProjection.Rows<ReconstructionResult, TOut>(self: self, key: key,
            new ProjectionRow(typeof(ReconstructionReceipt), () => Fin.Succ<object>(self.Receipt)),
            new ProjectionRow(typeof(ScalarField), () => Optional(self.Field).ToFin(key.InvalidResult()).Map(static field => (object)field)));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct ReconstructionSample(double Value, ReconstructionSampleReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Op key) {
        ReconstructionSample self = this;
        return AtomProjection.Rows<ReconstructionSample, TOut>(self: self, key: key,
            new ProjectionRow(typeof(ReconstructionSampleReceipt), () => Fin.Succ<object>(self.Receipt)),
            new ProjectionRow(typeof(double), () => key.AcceptValue(value: self.Value).Map(static value => (object)value)));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ReconstructionSampleReceipt(ReconstructionMode Mode, ReconstructionStatus Status, KernelKind Kernel, double Radius, int SampleCount, int NeighborhoodCount, int RejectedWeightCount, double WeightSum, int Rank, Option<double> Condition, Option<double> NormalAgreement, Option<double> GradientNorm, SolveReceipt Solve);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SdfReceipt(SdfStatus Status, Option<double> LipschitzBound, bool AnalyticPrimitive, bool MeshBacked, Option<bool> WatertightPreflight, bool LossyFallback, Option<SdfMeshReceipt> Mesh, bool NativeProfile = false, Option<ToleranceSource> ToleranceSource = default, Option<double> Tolerance = default, bool ClosestAccepted = false, Option<PointContainment> ProfileContainment = default, Option<ProfileExtrusionFeature> ProfileFeature = default, Option<TetSignedHeatReceipt> TetSignedHeat = default, Option<TetInterpolationReceipt> TetInterpolation = default);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SdfSample(double Value, SdfReceipt Receipt) {
    internal Fin<TOut> Project<TOut>(Op key) {
        SdfSample self = this;
        return AtomProjection.Rows<SdfSample, TOut>(self: self, key: key,
            new ProjectionRow(typeof(SdfReceipt), () => Fin.Succ<object>(self.Receipt)),
            new ProjectionRow(typeof(double), () => key.AcceptValue(value: self.Value).Map(static value => (object)value)));
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SignedHeatTime(Option<PositiveMagnitude> Explicit, PositiveMagnitude Coefficient) {
    public static Fin<SignedHeatTime> Scaled(double coefficient = 1.0, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: coefficient).Map(static c => new SignedHeatTime(Explicit: Option<PositiveMagnitude>.None, Coefficient: c));
    public static Fin<SignedHeatTime> Fixed(double value, Op? key = null) =>
        from heat in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: value)
        from coefficient in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: 1.0)
        select new SignedHeatTime(Explicit: Some(heat), Coefficient: coefficient);
    internal double Resolve(double cellSize) {
        double coefficient = Coefficient.Value;
        return Explicit.Match(Some: static heat => heat.Value, None: () => coefficient * cellSize * cellSize);
    }
    internal bool IsValid => Coefficient.Value > 0.0 && Explicit.Map(static heat => heat.Value > 0.0).IfNone(noneValue: true);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct TetCell(int A, int B, int C, int D) { internal int this[int index] => index switch { 0 => A, 1 => B, 2 => C, _ => D }; internal int[] Indices => [A, B, C, D]; }

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct TetFemReceipt(int VertexCount, int CellCount, int BoundaryVertexCount, int BoundaryFaceCount, int InteriorVertexCount, int IncidenceCount, double TotalVolume, double MinCellVolume, double MaxCellVolume, int MassNonZeros, int StiffnessNonZeros, int HeatOperatorNonZeros, int DivergenceNonZeros, int RejectedGradientCellCount);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct TetInterpolationReceipt(TetInterpolation Interpolation, int CellIndex, Arr<double> Barycentric, bool Inside);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TetMeshDomain(Seq<Point3d> Vertices, Seq<TetCell> Cells, Context Context, Arr<double> CellVolumes, Seq<int> BoundaryVertices, BoundingBox Bounds, int BoundaryFaceCount, int InteriorVertexCount, double TotalVolume) {
    public static Fin<TetMeshDomain> Of(Seq<Point3d> vertices, Seq<TetCell> cells, Context context, Op? key = null) {
        Op op = key.OrDefault();
        Point3d[] points = [.. vertices.AsIterable()];
        TetCell[] tets = [.. cells.AsIterable()];
        if (points.Length < 4 || tets.Length == 0 || !points.All(FieldNabla.Finite) || context is null) return Fin.Fail<TetMeshDomain>(op.InvalidInput());
        List<double> volumes = new(capacity: tets.Length);
        Dictionary<TetFaceKey, int> faces = [];
        for (int c = 0; c < tets.Length; c++) {
            TetCell cell = tets[c];
            int[] ids = cell.Indices;
            if (ids.Distinct().Take(count: 5).Count() != 4 || ids.Any(index => index < 0 || index >= points.Length)) return Fin.Fail<TetMeshDomain>(op.InvalidInput());
            double volume = VolumeOf(a: points[cell.A], b: points[cell.B], c: points[cell.C], d: points[cell.D]);
            if (!RhinoMath.IsValidDouble(x: volume) || volume <= RhinoMath.ZeroTolerance) return Fin.Fail<TetMeshDomain>(op.InvalidInput());
            volumes.Add(item: volume);
            TetFaceKey[] localFaces = [
                TetFaceKey.Of(a: cell.A, b: cell.B, c: cell.C),
                TetFaceKey.Of(a: cell.A, b: cell.B, c: cell.D),
                TetFaceKey.Of(a: cell.A, b: cell.C, c: cell.D),
                TetFaceKey.Of(a: cell.B, b: cell.C, c: cell.D),
            ];
            for (int f = 0; f < localFaces.Length; f++) {
                TetFaceKey face = localFaces[f];
                faces[face] = faces.TryGetValue(key: face, value: out int count) ? count + 1 : 1;
            }
        }
        TetFaceKey[] boundaryFaces = [.. faces.Where(static face => face.Value == 1).Select(static face => face.Key)];
        Seq<int> boundary = toSeq(boundaryFaces.SelectMany(static face => face.Indices).Distinct().Order());
        BoundingBox bounds = new(points);
        double total = volumes.Sum();
        return bounds.IsValid && boundary.Count > 0 && RhinoMath.IsValidDouble(x: total) && total > RhinoMath.ZeroTolerance
            ? Fin.Succ(new TetMeshDomain(Vertices: toSeq(points), Cells: toSeq(tets), Context: context, CellVolumes: new Arr<double>([.. volumes]), BoundaryVertices: boundary, Bounds: bounds, BoundaryFaceCount: boundaryFaces.Length, InteriorVertexCount: points.Length - boundary.Count, TotalVolume: total))
            : Fin.Fail<TetMeshDomain>(op.InvalidResult());
    }
    internal Fin<TetMeshDomain> Admit(Op key) =>
        AdmitRebuilt(source: this, key: key);
    private static Fin<TetMeshDomain> AdmitRebuilt(TetMeshDomain source, Op key) =>
        Of(vertices: source.Vertices, cells: source.Cells, context: source.Context, key: key).Bind(rebuilt =>
            source.CellVolumes.Count == rebuilt.CellVolumes.Count
            && source.BoundaryVertices.Count == rebuilt.BoundaryVertices.Count
            && source.BoundaryFaceCount == rebuilt.BoundaryFaceCount
            && source.InteriorVertexCount == rebuilt.InteriorVertexCount
            && Math.Abs(value: source.TotalVolume - rebuilt.TotalVolume) <= Math.Max(val1: source.Context.Absolute.Value, val2: RhinoMath.SqrtEpsilon)
            && toSeq(Enumerable.Range(start: 0, count: source.CellVolumes.Count)).ForAll(i => Math.Abs(value: source.CellVolumes[i] - rebuilt.CellVolumes[i]) <= Math.Max(val1: source.Context.Absolute.Value, val2: RhinoMath.SqrtEpsilon))
                ? Fin.Succ(source)
                : Fin.Fail<TetMeshDomain>(key.InvalidResult()));
    internal static Fin<TetCellMetric> MetricOf(Point3d[] points, TetCell cell, Op key) {
        Vector3d e1 = points[cell.B] - points[cell.A], e2 = points[cell.C] - points[cell.A], e3 = points[cell.D] - points[cell.A];
        double signedSix = Vector3d.CrossProduct(a: e1, b: e2) * e3;
        double volume = Math.Abs(value: signedSix) / 6.0;
        Dimension dim = Dimension.Create(value: 3);
        return !RhinoMath.IsValidDouble(x: volume) || volume <= RhinoMath.ZeroTolerance
            ? Fin.Fail<TetCellMetric>(key.InvalidInput())
            : from jacobian in Matrix.Of(rows: dim, cols: dim, entries: [e1.X, e2.X, e3.X, e1.Y, e2.Y, e3.Y, e1.Z, e2.Z, e3.Z], key: key)
              from inverse in jacobian.Inverse(key: key)
              let g1 = new Vector3d(x: inverse.At(i: 0, j: 0), y: inverse.At(i: 0, j: 1), z: inverse.At(i: 0, j: 2))
              let g2 = new Vector3d(x: inverse.At(i: 1, j: 0), y: inverse.At(i: 1, j: 1), z: inverse.At(i: 1, j: 2))
              let g3 = new Vector3d(x: inverse.At(i: 2, j: 0), y: inverse.At(i: 2, j: 1), z: inverse.At(i: 2, j: 2))
              let g0 = -(g1 + g2 + g3)
              select new TetCellMetric(Volume: volume, Gradients: [g0, g1, g2, g3]);
    }
    private static double VolumeOf(Point3d a, Point3d b, Point3d c, Point3d d) =>
        Math.Abs(value: Vector3d.CrossProduct(a: b - a, b: c - a) * (d - a)) / 6.0;
    [StructLayout(LayoutKind.Auto)]
    private readonly record struct TetFaceKey(int A, int B, int C) {
        internal int[] Indices => [A, B, C];
        internal static TetFaceKey Of(int a, int b, int c) {
            int[] ids = [a, b, c];
            System.Array.Sort(array: ids);
            return new TetFaceKey(A: ids[0], B: ids[1], C: ids[2]);
        }
    }
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VolumeGridPolicy(Option<Dimension> Resolution, Option<PositiveMagnitude> CellSize, PositiveMagnitude Padding) {
    public static Fin<VolumeGridPolicy> ByResolution(int resolution = 16, double padding = 1.0, Op? key = null) =>
        from count in key.OrDefault().AcceptValidated<Dimension>(candidate: resolution)
        from pad in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: padding)
        select new VolumeGridPolicy(Resolution: Some(count), CellSize: Option<PositiveMagnitude>.None, Padding: pad);
    public static Fin<VolumeGridPolicy> ByCellSize(double cellSize, double padding = 1.0, Op? key = null) =>
        from size in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: cellSize)
        from pad in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: padding)
        select new VolumeGridPolicy(Resolution: Option<Dimension>.None, CellSize: Some(size), Padding: pad);
    internal bool IsValid => Padding.Value > 0.0 && Resolution.IsSome != CellSize.IsSome && Resolution.Map(static count => count.Value > 0).IfNone(noneValue: true) && CellSize.Map(static size => size.Value > 0.0).IfNone(noneValue: true);
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct VolumeSolverPolicy(VolumeSolverKind Kind, PositiveMagnitude ResidualTolerance) {
    public static Fin<VolumeSolverPolicy> SparseCholesky(double residualTolerance = 1.0e-8, Op? key = null) =>
        from kind in Optional(VolumeSolverKind.SparseCholeskyPinned).ToFin(key.OrDefault().InvalidInput())
        from tolerance in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: residualTolerance)
        select new VolumeSolverPolicy(Kind: kind, ResidualTolerance: tolerance);
    internal bool IsValid => Kind is not null && Kind.Equals(VolumeSolverKind.SparseCholeskyPinned) && ResidualTolerance.Value > 0.0;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct SdfMeshPolicy(SdfMeshMethod Method, SdfSignConvention SignConvention, Option<VolumeGridPolicy> Grid, SignedHeatTime Heat, VolumeSolverPolicy Solver, VolumeInterpolation Interpolation, VolumeBoundaryCondition BoundaryCondition) {
    public static Fin<SdfMeshPolicy> GeneralizedWinding(SdfSignConvention? signConvention = null, Op? key = null) =>
        Defaults(method: SdfMeshMethod.GeneralizedWindingNumber, signConvention: signConvention, grid: Option<VolumeGridPolicy>.None, key: key.OrDefault());
    public static Fin<SdfMeshPolicy> BoundarySignedHeat(SignedHeatTime? heat = null, VolumeSolverPolicy? solver = null, SdfSignConvention? signConvention = null, Op? key = null) =>
        Defaults(method: SdfMeshMethod.BoundarySignedHeat, signConvention: signConvention, grid: Option<VolumeGridPolicy>.None, heat: heat, solver: solver, key: key.OrDefault());
    public static Fin<SdfMeshPolicy> ClosedSignedHeat(VolumeGridPolicy grid, SignedHeatTime? heat = null, VolumeSolverPolicy? solver = null, SdfSignConvention? signConvention = null, Op? key = null) =>
        Defaults(method: SdfMeshMethod.ClosedSurfaceSignedHeat, signConvention: signConvention, grid: Some(grid), heat: heat, solver: solver, key: key.OrDefault());
    internal Fin<SdfMeshPolicy> Admit(Op key) {
        SdfMeshPolicy self = this;
        SdfMeshMethod method = Method; SdfSignConvention signConvention = SignConvention; VolumeInterpolation interpolation = Interpolation; VolumeBoundaryCondition boundaryCondition = BoundaryCondition;
        SignedHeatTime heat = Heat; VolumeSolverPolicy solver = Solver; Option<VolumeGridPolicy> grid = Grid;
        return from active in Optional(method).ToFin(key.InvalidInput())
               from sign in Optional(signConvention).ToFin(key.InvalidInput())
               from interp in Optional(interpolation).ToFin(key.InvalidInput())
               from boundary in Optional(boundaryCondition).ToFin(key.InvalidInput())
               from _ in guard(sign.Equals(SdfSignConvention.NegativeInsidePositiveOutside) || sign.Equals(SdfSignConvention.PositiveInsideNegativeOutside), key.InvalidInput())
               from __ in guard(interp.Equals(VolumeInterpolation.Trilinear) && boundary.Equals(VolumeBoundaryCondition.NeumannGaugePinned) && heat.IsValid && solver.IsValid, key.InvalidInput())
               from ___ in active.Equals(SdfMeshMethod.ClosedSurfaceSignedHeat)
                   ? grid.Filter(static policy => policy.IsValid).ToFin(key.InvalidInput()).Map(static _ => unit)
                   : guard(grid.IsNone, key.InvalidInput()).ToFin()
               select self;
    }
    private static Fin<SdfMeshPolicy> Defaults(SdfMeshMethod method, SdfSignConvention? signConvention, Option<VolumeGridPolicy> grid, Op key, SignedHeatTime? heat = null, VolumeSolverPolicy? solver = null, VolumeInterpolation? interpolation = null, VolumeBoundaryCondition? boundaryCondition = null) =>
        from active in Optional(method).ToFin(key.InvalidInput())
        from sign in Optional(signConvention ?? SdfSignConvention.NegativeInsidePositiveOutside).ToFin(key.InvalidInput())
        from time in heat.HasValue ? Fin.Succ(heat.Value) : SignedHeatTime.Scaled(key: key)
        from solve in solver.HasValue ? Fin.Succ(solver.Value) : VolumeSolverPolicy.SparseCholesky(key: key)
        from interp in Optional(interpolation ?? VolumeInterpolation.Trilinear).ToFin(key.InvalidInput())
        from boundary in Optional(boundaryCondition ?? VolumeBoundaryCondition.NeumannGaugePinned).ToFin(key.InvalidInput())
        from policy in new SdfMeshPolicy(Method: active, SignConvention: sign, Grid: grid, Heat: time, Solver: solve, Interpolation: interp, BoundaryCondition: boundary).Admit(key: key)
        select policy;
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)]
public readonly record struct TetSignedHeatPolicy(SignedHeatTime Heat, VolumeSolverPolicy Solver, SdfSignConvention SignConvention, TetGaugePolicy Gauge, TetInterpolation Interpolation) {
    public static Fin<TetSignedHeatPolicy> Of(SignedHeatTime? heat = null, VolumeSolverPolicy? solver = null, SdfSignConvention? signConvention = null, TetGaugePolicy? gauge = null, TetInterpolation? interpolation = null, Op? key = null) =>
        from time in heat.HasValue ? Fin.Succ(heat.Value) : SignedHeatTime.Scaled(key: key)
        from solve in solver.HasValue ? Fin.Succ(solver.Value) : VolumeSolverPolicy.SparseCholesky(key: key)
        from sign in Optional(signConvention ?? SdfSignConvention.NegativeInsidePositiveOutside).ToFin(key.OrDefault().InvalidInput())
        from activeGauge in Optional(gauge ?? TetGaugePolicy.PinnedFirstBoundary).ToFin(key.OrDefault().InvalidInput())
        from activeInterpolation in Optional(interpolation ?? TetInterpolation.Barycentric).ToFin(key.OrDefault().InvalidInput())
        from admitted in new TetSignedHeatPolicy(Heat: time, Solver: solve, SignConvention: sign, Gauge: activeGauge, Interpolation: activeInterpolation).Admit(key: key.OrDefault())
        select admitted;
    internal Fin<TetSignedHeatPolicy> Admit(Op key) =>
        Heat.IsValid && Solver.IsValid && SignConvention is not null && Gauge is not null && Interpolation is not null && Gauge.Equals(TetGaugePolicy.PinnedFirstBoundary) && Interpolation.Equals(TetInterpolation.Barycentric)
            ? Fin.Succ(this)
            : Fin.Fail<TetSignedHeatPolicy>(key.InvalidInput());
}

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct TetSignedHeatReceipt(TetFemReceipt Fem, SignedHeatTime Heat, VolumeSolverPolicy Solver, SdfSignConvention SignConvention, TetGaugePolicy Gauge, TetInterpolation Interpolation, int GaugeVertex, double HeatTime, double BoundaryShift, double InteriorMean, SolveReceipt HeatSolve, SolveReceipt PoissonSolve);

[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct TetSignedHeatSample(double Value, TetSignedHeatReceipt Receipt, TetInterpolationReceipt Interpolation);

internal readonly record struct TetCellMetric(double Volume, Vector3d[] Gradients);

// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class FieldNabla {
    internal const long DefaultIsoSurfaceMaxCells = 16_777_216L;
    internal static readonly Vector3d CurlOffset2 = new(x: 31.4159, y: 27.1828, z: 41.4213), CurlOffset3 = new(x: -19.3274, y: 53.2186, z: -67.9531);
    internal static Fin<T> NotNull<T>(T? value, Op key)
        where T : class =>
        Optional(value).ToFin(key.InvalidInput());
    internal static Fin<T> NotNull<T>(T? value, Error error)
        where T : class =>
        Optional(value).ToFin(error);
    internal static Fin<Unit> CountAtLeast(int count, int minimum, Op key) =>
        guard(count >= minimum, key.InvalidInput()).ToFin();
    internal static Fin<Unit> SameCount(int expected, Op key, params int[] counts) =>
        guard(counts.All(count => count == expected), key.InvalidInput()).ToFin();
    internal static Fin<Unit> AllFinite(Seq<Point3d> points, Op key) =>
        guard(points.ForAll(static point => Finite(point: point)), key.InvalidInput()).ToFin();
    internal static Fin<Unit> AllFinite(Op key, params ReadOnlySpan<Point3d> points) {
        foreach (Point3d point in points) {
            if (!Finite(point: point)) {
                return Fin.Fail<Unit>(key.InvalidInput());
            }
        }
        return Fin.Succ(unit);
    }
    internal static Fin<Unit> AllValid(Op key, params ReadOnlySpan<Vector3d> vectors) {
        foreach (Vector3d vector in vectors) {
            if (!Finite(vector: vector)) {
                return Fin.Fail<Unit>(key.InvalidInput());
            }
        }
        return Fin.Succ(unit);
    }
    internal static Fin<Unit> AllFiniteDoubles(ReadOnlySpan<double> values, Op key) =>
        AllFiniteSpan(values) ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput());
    internal static bool AllFiniteSpan(ReadOnlySpan<double> values) =>
        TensorPrimitives.IsFiniteAll(values);
    internal static Fin<Seq<Point3d>> FinitePoints(Seq<Point3d> points, bool allowEmpty, Op key) =>
        (allowEmpty || !points.IsEmpty) && points.ForAll(static point => Finite(point: point)) ? Fin.Succ(points) : Fin.Fail<Seq<Point3d>>(key.InvalidInput());
    internal static Fin<Seq<double>> FiniteScalars(Seq<double> values, bool allowEmpty, Op key) =>
        (allowEmpty || !values.IsEmpty) && values.ForAll(RhinoMath.IsValidDouble) ? Fin.Succ(values) : Fin.Fail<Seq<double>>(key.InvalidInput());
    internal static Fin<(Seq<Point3d> Points, Seq<double> Weights)> WeightedPoints(Seq<Point3d> points, Seq<double> weights, Op key) =>
        points.Count == weights.Count && !points.IsEmpty && points.ForAll(static point => Finite(point: point)) && weights.ForAll(static weight => RhinoMath.IsValidDouble(x: weight) && weight > 0.0)
            ? Fin.Succ((Points: points, Weights: weights))
            : Fin.Fail<(Seq<Point3d> Points, Seq<double> Weights)>(key.InvalidInput());
    internal static Fin<Unit> PositiveFiniteWeights(double[] weights, int count, Op key) =>
        guard(weights.Length == count && weights.All(static weight => RhinoMath.IsValidDouble(x: weight) && weight > 0.0), key.InvalidInput()).ToFin();
    internal static Fin<Unit> Finite(double value, Op key) =>
        guard(RhinoMath.IsValidDouble(x: value), key.InvalidInput()).ToFin();
    internal static Fin<Unit> Finite(Point3d point, Op key) =>
        guard(Finite(point: point), key.InvalidInput()).ToFin();
    internal static Fin<Unit> Finite(Vector3d vector, Op key) =>
        guard(Finite(vector: vector), key.InvalidInput()).ToFin();
    internal static Fin<double> NonnegativeFinite(double value, Op key) =>
        RhinoMath.IsValidDouble(x: value) && value >= 0.0 ? Fin.Succ(value) : Fin.Fail<double>(key.InvalidInput());
    internal static Fin<TResult> WithPositive<TResult>(double candidate, Func<PositiveMagnitude, TResult> make, Op? key) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: candidate).Map(make);
    internal static Fin<TResult> WithPositivePair<TResult>(double left, double right, Func<PositiveMagnitude, PositiveMagnitude, TResult> make, Op? key) =>
        from a in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: left) from b in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: right) select make(arg1: a, arg2: b);
    internal static Fin<Unit> Positive(PositiveMagnitude value, Op key) =>
        key.AcceptValidated<PositiveMagnitude>(candidate: value.Value).Map(static _ => unit);
    internal static Fin<Unit> Dimension(Dimension value, Op key) =>
        key.AcceptValidated<Dimension>(candidate: value.Value).Map(static _ => unit);
    internal static Fin<TResult> WithDivisor<TResult>(double divisor, Func<double, TResult> make, Op? key) =>
        Math.Abs(value: divisor) > RhinoMath.ZeroTolerance ? Fin.Succ(make(arg: 1.0 / divisor)) : Fin.Fail<TResult>(key.OrDefault().InvalidInput());
    internal static Fin<Unit> KernelInput(double distance, double radius, Op key) =>
        guard(RhinoMath.IsValidDouble(x: distance) && RhinoMath.IsValidDouble(x: radius) && distance >= 0.0 && radius > RhinoMath.ZeroTolerance, key.InvalidInput()).ToFin();
    internal static Fin<Unit> FalloffInput(double distance, double distanceSquared, double tolerance, Op key) =>
        guard(RhinoMath.IsValidDouble(x: distance) && RhinoMath.IsValidDouble(x: distanceSquared) && RhinoMath.IsValidDouble(x: tolerance) && distance >= 0.0 && distanceSquared >= 0.0 && tolerance >= 0.0, key.InvalidInput()).ToFin();
    internal static Fin<Unit> NoiseInput(int octaves, double persistence, double lacunarity, double frequency, Op key) =>
        guard(octaves is >= 1 and <= 32 && RhinoMath.IsValidDouble(x: frequency) && frequency > 0.0 && RhinoMath.IsValidDouble(x: persistence) && persistence is > 0.0 and <= 1.0 && RhinoMath.IsValidDouble(x: lacunarity) && lacunarity > 1.0, key.InvalidInput()).ToFin();
    internal static Fin<Vector3d> NonnegativeExtent(Vector3d extent, Op key) =>
        Finite(vector: extent) && extent.X >= 0.0 && extent.Y >= 0.0 && extent.Z >= 0.0 ? Fin.Succ(extent) : Fin.Fail<Vector3d>(key.InvalidInput());
    internal static Fin<Plane> Plane(Plane basis, Op key) =>
        basis.IsValid
        && Finite(point: basis.Origin)
        && Finite(vector: basis.XAxis)
        && Finite(vector: basis.YAxis)
        && Finite(vector: basis.ZAxis)
        && Vector3d.AreOrthonormal(x: basis.XAxis, y: basis.YAxis, z: basis.ZAxis)
        && Vector3d.AreRighthanded(x: basis.XAxis, y: basis.YAxis, z: basis.ZAxis)
            ? Fin.Succ(basis)
            : Fin.Fail<Plane>(key.InvalidInput());
    internal static Fin<Seq<Plane>> PlaneSequence(Seq<Plane> planes, bool allowEmpty, Op key) =>
        (allowEmpty || !planes.IsEmpty) && planes.ForAll(static plane => plane.IsValid && Finite(point: plane.Origin) && Finite(vector: plane.XAxis) && Finite(vector: plane.YAxis) && Finite(vector: plane.ZAxis) && Vector3d.AreOrthonormal(x: plane.XAxis, y: plane.YAxis, z: plane.ZAxis) && Vector3d.AreRighthanded(x: plane.XAxis, y: plane.YAxis, z: plane.ZAxis))
            ? Fin.Succ(planes)
            : Fin.Fail<Seq<Plane>>(key.InvalidInput());
    internal static Fin<Direction> Direction(Direction value, Op key) =>
        Vectors.Direction.Of(value: value.Value, tolerance: RhinoMath.ZeroTolerance, key: key);
    internal static Fin<VectorCone> Cone(VectorCone value, Op key) =>
        from apex in Finite(point: value.Apex, key: key)
        from axis in Direction(value: value.Axis, key: key)
        from angle in key.AcceptValidated<VectorAngle>(candidate: value.HalfAngle.Value)
        from _ in guard(angle.Value <= Math.PI, key.InvalidInput())
        select value;
    internal static Fin<Vector3d> Period(Vector3d period, Op key) =>
        Finite(vector: period) && Math.Abs(value: period.X) > RhinoMath.ZeroTolerance && Math.Abs(value: period.Y) > RhinoMath.ZeroTolerance && Math.Abs(value: period.Z) > RhinoMath.ZeroTolerance ? Fin.Succ(period) : Fin.Fail<Vector3d>(key.InvalidInput());
    internal static Fin<Unit> FiniteRange(double minimum, double maximum, Op key) =>
        guard(RhinoMath.IsValidDouble(x: minimum) && RhinoMath.IsValidDouble(x: maximum) && minimum <= maximum, key.InvalidInput()).ToFin();
    internal static Fin<Seq<(Point3d Position, double Value)>> ReconstructionSamples(Seq<(Point3d Position, double Value)> samples, Op key) =>
        !samples.IsEmpty && samples.ForAll(static sample => Finite(point: sample.Position) && RhinoMath.IsValidDouble(x: sample.Value)) ? Fin.Succ(samples) : Fin.Fail<Seq<(Point3d Position, double Value)>>(key.InvalidInput());
    internal static Fin<Seq<MlsSample>> MlsInput(Seq<MlsSample> samples, Context context, Op key) =>
        Optional(context).ToFin(key.MissingContext()).Bind(model => !samples.IsEmpty
            && samples.ForAll(sample => Finite(point: sample.Position) && Finite(vector: sample.Normal) && Math.Abs(value: sample.Normal.Length - 1.0) <= Math.Max(val1: model.Relative.Value, val2: RhinoMath.SqrtEpsilon) && RhinoMath.IsValidDouble(x: sample.Value))
                ? Fin.Succ(samples)
                : Fin.Fail<Seq<MlsSample>>(key.InvalidInput()));
    internal static Fin<(Curve Profile, Plane Plane, PositiveMagnitude HalfHeight)> ProfileExtrusionInput(Curve profile, Plane plane, double halfHeight, Context context, Op key) =>
        from model in Optional(context).ToFin(key.MissingContext())
        from activePlane in Plane(basis: plane, key: key)
        from height in key.AcceptValidated<PositiveMagnitude>(candidate: halfHeight)
        from admitted in key.Catch(() => {
            Curve? duplicate = profile?.DuplicateCurve();
            Plane profilePlane = default;
            using CurveBooleanRegions? regions = duplicate is null ? null : Curve.CreateBooleanRegions(curves: [duplicate], plane: activePlane, combineRegions: false, tolerance: model.Absolute.Value);
            using CurveIntersections? self = duplicate is null ? null : Intersection.CurveSelf(curve: duplicate, tolerance: model.Absolute.Value);
            bool planeFound = profile is not null && profile.TryGetPlane(plane: out profilePlane, tolerance: model.Absolute.Value);
            bool planeAligned = planeFound && Math.Abs(value: activePlane.ZAxis * profilePlane.ZAxis) >= Math.Cos(d: model.Angle.Value) && Math.Abs(value: activePlane.DistanceTo(testPoint: profilePlane.Origin)) <= model.Absolute.Value;
            bool singleRegion = regions is { RegionCount: 1 } && regions.BoundaryCount(regionIndex: 0) == 1 && self is { Count: 0 };
            return duplicate is { IsValid: true, IsClosed: true } validProfile && planeAligned && singleRegion
                ? Fin.Succ((Profile: validProfile, Plane: activePlane, HalfHeight: height))
                : Fin.Fail<(Curve Profile, Plane Plane, PositiveMagnitude HalfHeight)>(key.InvalidInput());
        })
        select admitted;
    internal static Fin<Unit> IsoSurfaceInput(BoundingBox bounds, int resolution, int maxRootSteps, Op key) =>
        IsoGrid(bounds: bounds, resolution: resolution, maxRootSteps: maxRootSteps, maxCells: DefaultIsoSurfaceMaxCells, key: key).Map(static _ => unit);
    internal static Fin<IsoSurfaceGrid> IsoGrid(BoundingBox bounds, int resolution, int maxRootSteps, long maxCells, Op key) =>
        !ScalarField.BoundsAdmitted(bounds: bounds) || resolution < 2 || maxRootSteps < 1 || maxCells < 1
            ? Fin.Fail<IsoSurfaceGrid>(key.InvalidInput())
            : key.Catch(() => {
                double xSpan = bounds.Max.X - bounds.Min.X;
                double ySpan = bounds.Max.Y - bounds.Min.Y;
                double zSpan = bounds.Max.Z - bounds.Min.Z;
                double cellSize = Math.Min(val1: xSpan, val2: Math.Min(val1: ySpan, val2: zSpan)) / resolution;
                long Cells(double span) {
                    double raw = Math.Floor(d: span / cellSize);
                    return RhinoMath.IsValidDouble(x: raw) && raw >= 1.0 && raw <= long.MaxValue ? Math.Max(val1: 1L, val2: (long)raw) : 0L;
                }
                long xCells = Cells(span: xSpan), yCells = Cells(span: ySpan), zCells = Cells(span: zSpan);
                if (xCells < 1 || yCells < 1 || zCells < 1 || !RhinoMath.IsValidDouble(x: cellSize) || cellSize <= 0.0) return Fin.Fail<IsoSurfaceGrid>(key.InvalidInput());
                long hexCellCount = checked(xCells * yCells * zCells);
                long cornerSampleCount = checked((xCells + 1L) * (yCells + 1L) * (zCells + 1L));
                long initialSampleCount = checked(cornerSampleCount + hexCellCount);
                return hexCellCount <= maxCells && initialSampleCount <= maxCells
                    ? Fin.Succ(new IsoSurfaceGrid(Bounds: bounds, Resolution: resolution, XCells: xCells, YCells: yCells, ZCells: zCells, CellSize: cellSize, HexCellCount: hexCellCount, CornerSampleCount: cornerSampleCount, CenterSampleCount: hexCellCount, InitialSampleCount: initialSampleCount, MaxCells: maxCells))
                    : Fin.Fail<IsoSurfaceGrid>(key.InvalidInput());
            });
    internal static Fin<TResult> WithSourceEpsilon<TSource, TResult>(TSource? source, double epsilon, Func<TSource, PositiveMagnitude, TResult> make, Op? key)
        where TSource : class =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput()) from eps in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: epsilon) select make(active, eps);
    internal static Fin<Mesh> MeshNative(MeshSpace space, Op key) =>
        from mesh in Optional(space.Native).ToFin(key.InvalidInput())
        from _ in guard(mesh.IsValid, key.InvalidInput())
        select mesh;
    internal static Fin<Mesh> MeshOf(MeshSpace space, Op key) => MeshNative(space: space, key: key);
    internal static Fin<Surface> SurfaceNative(SurfaceSpace space, Op key) =>
        from surface in Optional(space.Native).ToFin(key.InvalidInput())
        from _ in guard(surface.IsValid, key.InvalidInput())
        select surface;
    internal static Fin<Mesh> MeshVertices(MeshSpace space, Seq<int> vertices, bool allowEmpty, Op key) =>
        from mesh in MeshOf(space: space, key: key)
        from _ in guard((allowEmpty || !vertices.IsEmpty) && vertices.ForAll(vertex => vertex >= 0 && vertex < mesh.Vertices.Count), key.InvalidInput())
        select mesh;
    internal static Fin<(Mesh Mesh, Seq<int> Vertices)> MeshSources(MeshSpace space, Seq<int> vertices, bool allowEmpty, Op key) =>
        MeshVertices(space: space, vertices: vertices, allowEmpty: allowEmpty, key: key).Map(mesh => (Mesh: mesh, Vertices: vertices));
    internal static Fin<(Mesh Mesh, Arr<double> Scalars)> ScalarMeshPayload(MeshSpace space, Arr<double> scalars, Op key) =>
        from mesh in MeshOf(space: space, key: key)
        from _ in guard(scalars.Count == mesh.Vertices.Count && scalars.ForAll(RhinoMath.IsValidDouble), key.InvalidInput()).ToFin()
        select (Mesh: mesh, Scalars: scalars);
    internal static Vector3d PerpendicularComponent(Vector3d r, Vector3d axis) => r - (r * axis * axis);
    internal static bool Finite(Point3d point) =>
        RhinoMath.IsValidDouble(x: point.X) && RhinoMath.IsValidDouble(x: point.Y) && RhinoMath.IsValidDouble(x: point.Z);
    internal static bool Finite(Vector3d vector) =>
        RhinoMath.IsValidDouble(x: vector.X) && RhinoMath.IsValidDouble(x: vector.Y) && RhinoMath.IsValidDouble(x: vector.Z);
    internal static Point3d ToroidalWrap(Point3d sample, Vector3d period) =>
        new(x: sample.X - (Math.Floor(d: (sample.X / period.X) + 0.5) * period.X), y: sample.Y - (Math.Floor(d: (sample.Y / period.Y) + 0.5) * period.Y), z: sample.Z - (Math.Floor(d: (sample.Z / period.Z) + 0.5) * period.Z));
    internal static Fin<(T X1, T X0, T Y1, T Y0, T Z1, T Z0)> SampleAxes<T>(Func<Point3d, Fin<T>> sampler, Point3d point, double eps, Op key) =>
        from _ in Finite(value: eps, key: key)
        from __ in guard(eps > RhinoMath.ZeroTolerance, key.InvalidInput()).ToFin()
        from xp in sampler(arg: point + (eps * Vector3d.XAxis))
        from xm in sampler(arg: point - (eps * Vector3d.XAxis))
        from yp in sampler(arg: point + (eps * Vector3d.YAxis))
        from ym in sampler(arg: point - (eps * Vector3d.YAxis))
        from zp in sampler(arg: point + (eps * Vector3d.ZAxis))
        from zm in sampler(arg: point - (eps * Vector3d.ZAxis))
        select (X1: xp, X0: xm, Y1: yp, Y0: ym, Z1: zp, Z0: zm);
    internal static Fin<Vector3d> GradientAt(ScalarField field, Point3d point, double eps, Context context, Op key) =>
        from samples in SampleAxes(sampler: p => field.SampleScalar(sample: p, context: context, key: key), point: point, eps: eps, key: key) let inv2eps = 1.0 / (2.0 * eps) select new Vector3d(x: (samples.X1 - samples.X0) * inv2eps, y: (samples.Y1 - samples.Y0) * inv2eps, z: (samples.Z1 - samples.Z0) * inv2eps);
    internal static Fin<Vector3d> CurlAt(VectorField field, Point3d point, double eps, Context context, Op key) =>
        from samples in SampleAxes(sampler: p => field.SampleVector(sample: p, context: context, key: key), point: point, eps: eps, key: key) let inv2eps = 1.0 / (2.0 * eps) from curl in key.AcceptValue(value: new Vector3d(x: (samples.Y1.Z - samples.Y0.Z - (samples.Z1.Y - samples.Z0.Y)) * inv2eps, y: (samples.Z1.X - samples.Z0.X - (samples.X1.Z - samples.X0.Z)) * inv2eps, z: (samples.X1.Y - samples.X0.Y - (samples.Y1.X - samples.Y0.X)) * inv2eps)) select curl;
    internal static Fin<Vector3d> CurlNoiseAt(ScalarField field, Point3d point, double eps, Context context, Op key) =>
        from g1 in GradientAt(field: field, point: point, eps: eps, context: context, key: key) from g2 in GradientAt(field: field, point: point + CurlOffset2, eps: eps, context: context, key: key) from g3 in GradientAt(field: field, point: point + CurlOffset3, eps: eps, context: context, key: key) from raw in key.AcceptValue(value: new Vector3d(x: g3.Y - g2.Z, y: g1.Z - g3.X, z: g2.X - g1.Y)) select raw;
    internal static Fin<double> DivergenceAt(VectorField field, Point3d point, double eps, Context context, Op key) =>
        from samples in SampleAxes(sampler: p => field.SampleVector(sample: p, context: context, key: key), point: point, eps: eps, key: key) let inv2eps = 1.0 / (2.0 * eps) from value in key.AcceptValue(value: (samples.X1.X - samples.X0.X + samples.Y1.Y - samples.Y0.Y + samples.Z1.Z - samples.Z0.Z) * inv2eps) select value;
    internal static Fin<double> LaplacianAt(ScalarField field, Point3d point, double eps, Context context, Op key) =>
        from samples in SampleAxes(sampler: p => field.SampleScalar(sample: p, context: context, key: key), point: point, eps: eps, key: key) from center in field.SampleScalar(sample: point, context: context, key: key) let invEpsSq = 1.0 / (eps * eps) from value in key.AcceptValue(value: (samples.X1 + samples.X0 + samples.Y1 + samples.Y0 + samples.Z1 + samples.Z0 - (6.0 * center)) * invEpsSq) select value;
    internal static Fin<double> StrainMagnitudeAt(VectorField field, Point3d point, double eps, Context context, Op key) =>
        from samples in SampleAxes(sampler: p => field.SampleVector(sample: p, context: context, key: key), point: point, eps: eps, key: key) let inv2eps = 1.0 / (2.0 * eps) let sxx = (samples.X1.X - samples.X0.X) * inv2eps let syy = (samples.Y1.Y - samples.Y0.Y) * inv2eps let szz = (samples.Z1.Z - samples.Z0.Z) * inv2eps let sxy = 0.5 * (samples.Y1.X - samples.Y0.X + samples.X1.Y - samples.X0.Y) * inv2eps let sxz = 0.5 * (samples.Z1.X - samples.Z0.X + samples.X1.Z - samples.X0.Z) * inv2eps let syz = 0.5 * (samples.Z1.Y - samples.Z0.Y + samples.Y1.Z - samples.Y0.Z) * inv2eps from value in key.AcceptValue(value: Math.Sqrt(d: (sxx * sxx) + (syy * syy) + (szz * szz) + (2.0 * ((sxy * sxy) + (sxz * sxz) + (syz * syz))))) select value;
}

internal static class FieldNoise {
    private static readonly int[] PermTable = [
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
    ];
    private static int Perm(int x, int seed) => PermTable[(x + seed) & 0xFF];
    private static double Fade(double t) => t * t * t * ((t * ((t * 6) - 15)) + 10);
    private static double Lerp(double t, double a, double b) => a + (t * (b - a));
    private static double Grad(int hash, double x, double y, double z) =>
        ((hash & 1) == 0 ? ((hash & 15) < 8 ? x : y) : -((hash & 15) < 8 ? x : y)) + ((hash & 2) == 0 ? ((hash & 15) < 4 ? y : (hash & 15) is 12 or 14 ? x : z) : -((hash & 15) < 4 ? y : (hash & 15) is 12 or 14 ? x : z));
    internal static double PerlinAt(Point3d point, int seed, double frequency) {
        double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
        int X = (int)Math.Floor(d: px) & 0xFF; int Y = (int)Math.Floor(d: py) & 0xFF; int Z = (int)Math.Floor(d: pz) & 0xFF;
        double x = px - Math.Floor(d: px); double y = py - Math.Floor(d: py); double z = pz - Math.Floor(d: pz);
        double u = Fade(t: x); double v = Fade(t: y); double w = Fade(t: z);
        int A = Perm(x: X, seed: seed) + Y; int AA = Perm(x: A, seed: seed) + Z; int AB = Perm(x: A + 1, seed: seed) + Z;
        int B = Perm(x: X + 1, seed: seed) + Y; int BA = Perm(x: B, seed: seed) + Z; int BB = Perm(x: B + 1, seed: seed) + Z;
        return Lerp(t: w, a: Lerp(t: v, a: Lerp(t: u, a: Grad(hash: Perm(x: AA, seed: seed), x: x, y: y, z: z), b: Grad(hash: Perm(x: BA, seed: seed), x: x - 1, y: y, z: z)), b: Lerp(t: u, a: Grad(hash: Perm(x: AB, seed: seed), x: x, y: y - 1, z: z), b: Grad(hash: Perm(x: BB, seed: seed), x: x - 1, y: y - 1, z: z))), b: Lerp(t: v, a: Lerp(t: u, a: Grad(hash: Perm(x: AA + 1, seed: seed), x: x, y: y, z: z - 1), b: Grad(hash: Perm(x: BA + 1, seed: seed), x: x - 1, y: y, z: z - 1)), b: Lerp(t: u, a: Grad(hash: Perm(x: AB + 1, seed: seed), x: x, y: y - 1, z: z - 1), b: Grad(hash: Perm(x: BB + 1, seed: seed), x: x - 1, y: y - 1, z: z - 1))));
    }
    internal static double WorleyAt(Point3d point, int seed, double frequency) {
        double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
        int cx = (int)Math.Floor(d: px); int cy = (int)Math.Floor(d: py); int cz = (int)Math.Floor(d: pz);
        return Math.Sqrt(d: (from dx in Enumerable.Range(start: -1, count: 3) from dy in Enumerable.Range(start: -1, count: 3) from dz in Enumerable.Range(start: -1, count: 3) let nx = cx + dx let ny = cy + dy let nz = cz + dz let hashX = Perm(x: Perm(x: Perm(x: nx & 0xFF, seed: seed) + (ny & 0xFF), seed: seed) + (nz & 0xFF), seed: seed) let hashY = Perm(x: hashX + 17, seed: seed) let hashZ = Perm(x: hashY + 31, seed: seed) let ddx = nx + (hashX / 255.0) - px let ddy = ny + (hashY / 255.0) - py let ddz = nz + (hashZ / 255.0) - pz select (ddx * ddx) + (ddy * ddy) + (ddz * ddz)).Min());
    }
    internal static double SkewedSimplexAt(Point3d point, int seed, double frequency, bool smooth) {
        double stretch = (point.X + point.Y + point.Z) * (1.0 / 3.0);
        Point3d skewed = new(x: point.X + stretch, y: point.Y + stretch, z: point.Z + stretch);
        double baseNoise = SimplexAt(point: skewed, seed: seed, frequency: frequency);
        return smooth ? 0.5 * (baseNoise + SimplexAt(point: new Point3d(x: skewed.Y, y: skewed.Z, z: skewed.X), seed: seed + 101, frequency: frequency)) : baseNoise;
    }
    private static double SimplexAt(Point3d point, int seed, double frequency) {
        double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
        int i = (int)Math.Floor(d: px); int j = (int)Math.Floor(d: py); int k = (int)Math.Floor(d: pz);
        double x0 = px - i; double y0 = py - j; double z0 = pz - k;
        (int i1, int j1, int k1, int i2, int j2, int k2) = x0 >= y0
            ? y0 >= z0 ? (1, 0, 0, 1, 1, 0) : x0 >= z0 ? (1, 0, 0, 1, 0, 1) : (0, 0, 1, 1, 0, 1) : y0 < z0 ? (0, 0, 1, 0, 1, 1) : x0 < z0 ? (0, 1, 0, 0, 1, 1) : (0, 1, 0, 1, 1, 0);
        double n0 = SimplexCorner(hash: HashCell(i: i, j: j, k: k, seed: seed), x: x0, y: y0, z: z0);
        double n1 = SimplexCorner(hash: HashCell(i: i + i1, j: j + j1, k: k + k1, seed: seed), x: x0 - i1 + (1.0 / 6.0), y: y0 - j1 + (1.0 / 6.0), z: z0 - k1 + (1.0 / 6.0));
        double n2 = SimplexCorner(hash: HashCell(i: i + i2, j: j + j2, k: k + k2, seed: seed), x: x0 - i2 + (1.0 / 3.0), y: y0 - j2 + (1.0 / 3.0), z: z0 - k2 + (1.0 / 3.0));
        double n3 = SimplexCorner(hash: HashCell(i: i + 1, j: j + 1, k: k + 1, seed: seed), x: x0 - 0.5, y: y0 - 0.5, z: z0 - 0.5);
        return 32.0 * (n0 + n1 + n2 + n3);
    }
    private static int HashCell(int i, int j, int k, int seed) =>
        Perm(x: Perm(x: Perm(x: i & 0xFF, seed: seed) + (j & 0xFF), seed: seed) + (k & 0xFF), seed: seed);
    private static double SimplexCorner(int hash, double x, double y, double z) {
        double t = 0.6 - (x * x) - (y * y) - (z * z);
        return t <= 0.0 ? 0.0 : t * t * t * t * Grad(hash: hash, x: x, y: y, z: z);
    }
}

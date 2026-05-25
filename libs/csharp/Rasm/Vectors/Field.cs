using Foundation.CSharp.Analyzers.Contracts;

namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>] public sealed partial class SdfMeshMethod { public static readonly SdfMeshMethod GeneralizedWindingNumber = new(key: 0), BoundarySignedHeat = new(key: 1); }

[SmartEnum<int>]
public sealed partial class FieldBlend {
    public static readonly FieldBlend Sum = new(key: 0, scale: static _ => 1.0);
    public static readonly FieldBlend Average = new(key: 1, scale: static count => 1.0 / count);
    internal Fin<Vector3d> Combine(Seq<Vector3d> vectors, Op key) => CombineCore(values: vectors, zero: Vector3d.Zero, add: static (sum, v) => sum + v, scale: static (sum, factor) => sum * factor, key: key);
    internal Fin<double> CombineScalar(Seq<double> values, Op key) => CombineCore(values: values, zero: 0.0, add: static (sum, v) => sum + v, scale: static (sum, factor) => sum * factor, key: key);
    private Fin<T> CombineCore<T>(Seq<T> values, T zero, Func<T, T, T> add, Func<T, double, T> scale, Op key) =>
        from _ in guard(!values.IsEmpty, key.InvalidResult())
        from value in key.AcceptValue(value: scale(arg1: values.Fold(initialState: zero, f: add), arg2: Scale(count: values.Count)))
        select value;
    [UseDelegateFromConstructor] private partial double Scale(int count);
}

[SmartEnum<int>]
public sealed partial class CsgKind {
    public static readonly CsgKind Union = new(key: 0, combine: static (a, b, blend) => blend.Smin(a: a, b: b)), Intersect = new(key: 1, combine: static (a, b, blend) => -blend.Smin(a: -a, b: -b)), Difference = new(key: 2, combine: static (a, b, blend) => -blend.Smin(a: -a, b: b));
    [UseDelegateFromConstructor] internal partial double Combine(double left, double right, BlendKind blend);
}

[Union]
public abstract partial record BlendKind {
    private BlendKind() { }
    public sealed record HardCase : BlendKind;
    public sealed record PolynomialCase(PositiveMagnitude K) : BlendKind;
    public sealed record ExponentialCase(PositiveMagnitude K) : BlendKind;
    public sealed record RootCase(PositiveMagnitude K) : BlendKind;
    public sealed record CubicCase(PositiveMagnitude K) : BlendKind;
    public sealed record ChamferCase(PositiveMagnitude K) : BlendKind;
    public sealed record GrooveCase(PositiveMagnitude K, PositiveMagnitude D) : BlendKind;
    public sealed record RoundCase(PositiveMagnitude R) : BlendKind;
    public static BlendKind Hard => new HardCase();
    public static Fin<BlendKind> Polynomial(double k, Op? key = null) => FieldNabla.WithPositive(candidate: k, make: static v => (BlendKind)new PolynomialCase(K: v), key: key);
    public static Fin<BlendKind> Exponential(double k, Op? key = null) => FieldNabla.WithPositive(candidate: k, make: static v => (BlendKind)new ExponentialCase(K: v), key: key);
    public static Fin<BlendKind> Root(double k, Op? key = null) => FieldNabla.WithPositive(candidate: k, make: static v => (BlendKind)new RootCase(K: v), key: key);
    public static Fin<BlendKind> Cubic(double k, Op? key = null) => FieldNabla.WithPositive(candidate: k, make: static v => (BlendKind)new CubicCase(K: v), key: key);
    public static Fin<BlendKind> Chamfer(double k, Op? key = null) => FieldNabla.WithPositive(candidate: k, make: static v => (BlendKind)new ChamferCase(K: v), key: key);
    public static Fin<BlendKind> Round(double r, Op? key = null) => FieldNabla.WithPositive(candidate: r, make: static v => (BlendKind)new RoundCase(R: v), key: key);
    public static Fin<BlendKind> Groove(double k, double d, Op? key = null) =>
        FieldNabla.WithPositivePair(left: k, right: d, make: static (kk, dd) => (BlendKind)new GrooveCase(K: kk, D: dd), key: key);
    internal double Smin(double a, double b) => Switch(
        state: (A: a, B: b),
        hardCase: static (s, _) => Math.Min(val1: s.A, val2: s.B),
        polynomialCase: static (s, c) => {
            double h = Math.Max(val1: c.K.Value - Math.Abs(value: s.A - s.B), val2: 0.0) / c.K.Value;
            return Math.Min(val1: s.A, val2: s.B) - (h * h * h * c.K.Value * (1.0 / 6.0));
        },
        exponentialCase: static (s, c) => -Math.Log(d: Math.Exp(d: -c.K.Value * s.A) + Math.Exp(d: -c.K.Value * s.B)) / c.K.Value,
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
        compute: static (p, ps) => (Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y)), Math.Sin(a: ps["angle"]), Math.Cos(d: ps["angle"])) switch {
            (double qx, double sn, double cs) => Math.Max(val1: (qx * cs) - (p.Z * sn), val2: -p.Z - ps["h"]),
        });
    public static readonly SdfKind HalfSpace = new(key: 5, lipschitz: 1.0, requiredKeys: Seq<string>(), validate: static _ => true, compute: static (p, _) => p.Z);
    public static readonly SdfKind CappedCone = new(key: 6, lipschitz: 1.2, requiredKeys: Seq("h", "r1", "r2"),
        validate: static ps => ps["h"] > RhinoMath.ZeroTolerance && ps["r1"] >= 0.0 && ps["r2"] >= 0.0 && (ps["r1"] > RhinoMath.ZeroTolerance || ps["r2"] > RhinoMath.ZeroTolerance),
        compute: static (p, ps) => {
            double qx = Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y));
            double h = ps["h"]; double r1 = ps["r1"]; double r2 = ps["r2"];
            Vector2d q = new(x: qx, y: p.Z);
            Vector2d k1 = new(x: r2, y: h);
            Vector2d k2 = new(x: r2 - r1, y: 2.0 * h);
            Vector2d ca = new(x: q.X - Math.Min(val1: q.X, val2: q.Y < 0.0 ? r1 : r2), y: Math.Abs(value: q.Y) - h);
            double k2LengthSquared = k2 * k2;
            double t = Math.Clamp(value: (k1 - q) * k2 / k2LengthSquared, min: 0.0, max: 1.0);
            Vector2d cb = q - k1 + (t * k2);
            double sign = cb.X < 0.0 && ca.Y < 0.0 ? -1.0 : 1.0;
            return sign * Math.Sqrt(d: Math.Min(val1: ca * ca, val2: cb * cb));
        });
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
    internal bool ValidateParameters(ImmutableDictionary<string, double> parameters) => RequiredKeys.ForAll(k => parameters.ContainsKey(key: k) && RhinoMath.IsValidDouble(x: parameters[k])) && Validate(parameters: parameters);
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
public sealed partial class NoiseKind {
    public static readonly NoiseKind Perlin = new(key: 0, raisesCaution: true, sample: static (p, seed, freq) => FieldNoise.PerlinAt(point: p, seed: seed, frequency: freq));
    public static readonly NoiseKind Simplex = new(key: 1, raisesCaution: false, sample: static (p, seed, freq) => FieldNoise.SkewedSimplexAt(point: p, seed: seed, frequency: freq, smooth: false));
    public static readonly NoiseKind SmoothSimplex = new(key: 2, raisesCaution: false, sample: static (p, seed, freq) => FieldNoise.SkewedSimplexAt(point: p, seed: seed, frequency: freq, smooth: true));
    public static readonly NoiseKind Worley = new(key: 3, raisesCaution: false, sample: static (p, seed, freq) => FieldNoise.WorleyAt(point: p, seed: seed, frequency: freq));
    public bool RaisesCaution { get; }
    [UseDelegateFromConstructor] internal partial double Sample(Point3d point, int seed, double frequency);
}

[SmartEnum<int>] public sealed partial class SdfStatus { public static readonly SdfStatus Analytic = new(key: 0), ComposedAnalytic = new(key: 1), MeshApproximate = new(key: 2), LossyFallback = new(key: 3); }
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SdfReceipt(SdfStatus Status, Option<double> LipschitzBound, bool AnalyticPrimitive, bool MeshBacked, Option<bool> WatertightPreflight, bool LossyFallback, Option<SdfMeshReceipt> Mesh);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct SdfSample(double Value, SdfReceipt Receipt);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct IsoSurfaceReceipt(bool NativeRouted, BoundingBox Bounds, int Resolution, int MaxRootSteps, bool ParallelCallback, int EvaluatorFailures, bool Valid, int VertexCount, int FaceCount, Option<double> FixedTolerance);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct IsoSurfaceResult(Mesh Mesh, IsoSurfaceReceipt Receipt);
[SmartEnum<int>] public sealed partial class ReconstructionMode { public static readonly ReconstructionMode RbfInterpolation = new(key: 0), RbfApproximation = new(key: 1), MovingLeastSquares = new(key: 2); }
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ReconstructionReceipt(ReconstructionMode Mode, KernelKind Kernel, double Radius, double Smoothing, bool Interpolation, int SampleCount, int CenterCount, int PolynomialDegree, SolveReceipt Solve);
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct ReconstructionResult(ScalarField Field, ReconstructionReceipt Receipt);
[SmartEnum<int>] public sealed partial class KernelProfileStatus { public static readonly KernelProfileStatus Smooth = new(key: 0), SupportBoundary = new(key: 1), NonsmoothOrigin = new(key: 2), OutsideSupport = new(key: 3); }
[BoundaryAdapter, StructLayout(LayoutKind.Auto)] public readonly record struct KernelProfile(double Value, double FirstDerivative, double SecondDerivative, KernelProfileStatus Status) { public bool IsValid => RhinoMath.IsValidDouble(x: Value) && RhinoMath.IsValidDouble(x: FirstDerivative) && RhinoMath.IsValidDouble(x: SecondDerivative); }
[SmartEnum<int>]
public sealed partial class KernelKind {
    public static readonly KernelKind Wendland = new(key: 0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => Pow1(q: q, power: 4) * (1.0 + (4.0 * q)), first: static (q, r) => ((-20.0 * q) + (60.0 * q * q) - (60.0 * q * q * q) + (20.0 * q * q * q * q)) / r, second: static (q, r) => (-20.0 + (120.0 * q) - (180.0 * q * q) + (80.0 * q * q * q)) / (r * r)));
    public static readonly KernelKind Quintic = new(key: 1, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => Pow1(q: q, power: 5), first: static (q, r) => -5.0 * Pow1(q: q, power: 4) / r, second: static (q, r) => 20.0 * Pow1(q: q, power: 3) / (r * r)));
    public static readonly KernelKind Cosine = new(key: 2, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => 0.5 * (1.0 + Math.Cos(d: Math.PI * q)), first: static (q, r) => -0.5 * Math.PI * Math.Sin(a: Math.PI * q) / r, second: static (q, r) => -0.5 * Math.PI * Math.PI * Math.Cos(d: Math.PI * q) / (r * r)));
    public static readonly KernelKind Cubic = new(key: 3, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => Pow1(q: q, power: 3), first: static (q, r) => -3.0 * Pow1(q: q, power: 2) / r, second: static (q, r) => 6.0 * (1.0 - q) / (r * r)));
    public static readonly KernelKind Linear = new(key: 4, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => 1.0 - q, first: static (_, r) => -1.0 / r, second: static (_, _) => 0.0));
    public static readonly KernelKind Epanechnikov = new(key: 5, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => 1.0 - (q * q), first: static (q, r) => -2.0 * q / r, second: static (_, r) => -2.0 / (r * r)));
    internal Fin<KernelProfile> Profile(double distance, double radius, Op key) =>
        !RhinoMath.IsValidDouble(x: distance) || !RhinoMath.IsValidDouble(x: radius) || distance < 0.0 || radius <= RhinoMath.ZeroTolerance
            ? Fin.Fail<KernelProfile>(key.InvalidInput())
            : Evaluate(distance: distance, radius: radius) switch {
                KernelProfile profile when profile.IsValid => Fin.Succ(profile),
                _ => Fin.Fail<KernelProfile>(key.InvalidResult()),
            };
    internal double Weight(double distance, double radius) => Evaluate(distance: distance, radius: radius).Value;
    [UseDelegateFromConstructor] private partial KernelProfile Evaluate(double distance, double radius);
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

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record Falloff {
    private Falloff() { }
    public sealed record ConstantCase : Falloff; public sealed record InverseCase : Falloff; public sealed record InverseSquareCase : Falloff;
    public sealed record GaussianCase(PositiveMagnitude Spread) : Falloff; public sealed record KernelCase(KernelKind Kind, PositiveMagnitude Radius) : Falloff; public sealed record AnisotropicKernelCase(KernelKind Kind, TensorField Metric, PositiveMagnitude Radius) : Falloff;
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
    private Fin<double> WeightCore(double distance, double distanceSquared, Option<(Vector3d Offset, Point3d Sample, Context Context)> metric, double tolerance, Op key) => Switch(
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
            select profile.Value);
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
    public static Fin<BouncePolicy> Refract(double etaIncident, double etaTransmitted, Op? key = null) =>
        FieldNabla.WithPositivePair(left: etaIncident, right: etaTransmitted, make: static (incident, transmitted) => (BouncePolicy)new RefractCase(EtaIncident: incident, EtaTransmitted: transmitted), key: key);
    internal Fin<Direction> Apply(Direction incident, Direction normal, Op key) => Switch(
        state: (Incident: incident, Normal: normal, Key: key),
        reflectCase: static (state, _) => Fin.Succ(state.Incident.Reflect(normal: state.Normal)),
        refractCase: static (state, refract) => Direction.Refract(
            incident: state.Incident, normal: state.Normal,
            etaIncident: refract.EtaIncident.Value, etaTransmitted: refract.EtaTransmitted.Value, key: state.Key));
}

[Union]
public partial record VectorField {
    public sealed record ConstantCase(Vector3d Value) : VectorField; public sealed record BlendCase(Seq<VectorField> Fields, FieldBlend Mode) : VectorField; public sealed record ScaledCase(VectorField Source, double Scale) : VectorField;
    public sealed record InfluenceCase(SupportSpace Source, Falloff Falloff, BoundarySense Sense, Option<PositiveMagnitude> Radius) : VectorField; public sealed record HitFieldCase(SupportSpace Source, SupportProjection Projection, BoundarySense Sense) : VectorField;
    public sealed record VortexCase(Point3d Anchor, Direction Axis, Falloff Falloff) : VectorField; public sealed record RingCase(Point3d Center, Direction Axis, PositiveMagnitude Radius, Falloff Falloff) : VectorField; public sealed record HelicalCase(Point3d Anchor, Direction Axis, double Axial, double Swirl, Falloff Falloff) : VectorField;
    public sealed record CoulombCase(Seq<(Point3d Position, double Charge)> Charges, Falloff Falloff) : VectorField; public sealed record ClusterFieldCase(VectorCloud.ClusterCase Source, Falloff Falloff, PositiveMagnitude Radius, BoundarySense Sense) : VectorField; public sealed record DipoleCase(Point3d Origin, Direction Moment, PositiveMagnitude Strength) : VectorField;
    public sealed record HarmonicCase(Seq<(Direction Direction, double Frequency, double Phase, double Amplitude)> Components) : VectorField; public sealed record ProjectedCase(VectorField Source, Plane Onto) : VectorField; public sealed record WarpCase(VectorField Source, Transform Spatial) : VectorField; public sealed record ClampMagnitudeCase(VectorField Source, PositiveMagnitude Min, PositiveMagnitude Max) : VectorField;
    public sealed record GradientCase(ScalarField Source, PositiveMagnitude Epsilon) : VectorField; public sealed record CurlCase(VectorField Source, PositiveMagnitude Epsilon) : VectorField; public sealed record CurlNoiseCase(ScalarField Potential, PositiveMagnitude Epsilon, bool RaisesCaution) : VectorField; public sealed record CrossProductCase(VectorField Left, VectorField Right) : VectorField;
    public sealed record BiotSavartCase(Point3d Start, Point3d End, double Current) : VectorField; public sealed record SaddleCase(Point3d Anchor, Plane Basis, double Strength) : VectorField;
    public sealed record CrossFieldCase(MeshSpace Space, int Symmetry, Option<Seq<(int Vertex, Direction Hint)>> Constraints, Option<Seq<(int Vertex, double HolonomyDeficit)>> Cones) : VectorField; public sealed record HodgeCase(VectorField Source, MeshSpace Space, BoundarySense Sense) : VectorField; public sealed record VectorHeatCase(MeshSpace Space, Seq<(int Vertex, Vector3d Direction)> Sources, PositiveMagnitude Time) : VectorField; public sealed record GeodesicTangentCase(MeshSpace Space, Seq<int> Sources) : VectorField;
    public static VectorField Constant(Vector3d value) => new ConstantCase(Value: value);
    public static Fin<VectorField> Hit(SupportSpace source, SupportProjection projection, BoundarySense? sense = null, Op? key = null) =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput()) from selected in Optional(projection).ToFin(key.OrDefault().InvalidInput()) from _ in guard(selected.CanProjectVector(space: active), key.OrDefault().Unsupported(active.SourceType, typeof(Vector3d))) select (VectorField)new HitFieldCase(Source: active, Projection: selected, Sense: sense ?? BoundarySense.Toward);
    public static Fin<VectorField> Shell(SupportSpace source, double radius, Falloff? falloff = null, BoundarySense? sense = null, Op? key = null) =>
        FieldNabla.WithPositive(candidate: radius, make: value => (VectorField)new InfluenceCase(Source: source, Falloff: falloff ?? Falloff.Constant, Sense: sense ?? BoundarySense.Toward, Radius: Some(value)), key: key);
    public static VectorField Blend(Seq<VectorField> fields, FieldBlend? blend = null) =>
        new BlendCase(Fields: fields, Mode: blend ?? FieldBlend.Sum);
    public static Fin<VectorField> Cluster(VectorCloud cluster, double radius, Falloff? falloff = null, BoundarySense? sense = null, Op? key = null) =>
        cluster switch { VectorCloud.ClusterCase c => from r in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius) from f in falloff is null ? Falloff.Gaussian(spread: r.Value / 3.0, key: key) : Fin.Succ(falloff) select (VectorField)new ClusterFieldCase(Source: c, Falloff: f, Radius: r, Sense: sense ?? BoundarySense.Toward), _ => Fin.Fail<VectorField>(key.OrDefault().Unsupported(geometryType: cluster.GetType(), outputType: typeof(VectorField))) };
    public static Fin<VectorField> Dipole(Point3d origin, Direction moment, double strength, Op? key = null) =>
        FieldNabla.WithPositive(candidate: strength, make: s => (VectorField)new DipoleCase(Origin: origin, Moment: moment, Strength: s), key: key);
    public static Fin<VectorField> ClampMagnitude(VectorField source, double min, double max, Op? key = null) =>
        from low in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: min) from high in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: max) from _ in guard(low.Value <= high.Value, key.OrDefault().InvalidInput()) select (VectorField)new ClampMagnitudeCase(Source: source, Min: low, Max: high);
    public static Fin<VectorField> Divide(VectorField source, double divisor, Op? key = null) =>
        FieldNabla.WithDivisor(divisor: divisor, make: scale => (VectorField)new ScaledCase(Source: source, Scale: scale), key: key);
    public static Fin<VectorField> Gradient(ScalarField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<ScalarField, VectorField>(source, epsilon, static (s, e) => new GradientCase(Source: s, Epsilon: e), key);
    public static Fin<VectorField> Curl(VectorField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<VectorField, VectorField>(source, epsilon, static (s, e) => new CurlCase(Source: s, Epsilon: e), key);
    public static Fin<VectorField> Ring(Point3d center, Direction axis, double radius, Falloff? falloff = null, Op? key = null) =>
        from r in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius) from f in falloff is null ? Falloff.Gaussian(spread: radius / 3.0, key: key) : Fin.Succ(falloff) select (VectorField)new RingCase(Center: center, Axis: axis, Radius: r, Falloff: f);
    public static Fin<VectorField> BiotSavart(Point3d start, Point3d end, double current, Op? key = null) =>
        from a in key.OrDefault().AcceptValue(value: start) from b in key.OrDefault().AcceptValue(value: end) from i in key.OrDefault().AcceptValue(value: current) from _ in guard(!(a - b).IsTiny(), key.OrDefault().InvalidInput()) select (VectorField)new BiotSavartCase(Start: a, End: b, Current: i);
    public static Fin<VectorField> Saddle(Point3d anchor, Plane basis, double strength, Op? key = null) =>
        basis.IsValid ? Fin.Succ((VectorField)new SaddleCase(Anchor: anchor, Basis: basis, Strength: strength)) : Fin.Fail<VectorField>(key.OrDefault().InvalidInput());
    public static Fin<VectorField> CrossField(MeshSpace space, int symmetry, Option<Seq<(int Vertex, Direction Hint)>> constraints = default, Option<Seq<(int Vertex, double HolonomyDeficit)>> cones = default, Op? key = null) =>
        from active in FieldNabla.MeshOf(space: space, key: key.OrDefault()) from __ in guard(symmetry is 1 or 2 or 4 or 6, key.OrDefault().InvalidInput()) let vertexCount = active.Vertices.Count from ___ in guard(constraints.Match(Some: values => values.ForAll(item => item.Vertex >= 0 && item.Vertex < vertexCount), None: static () => true) && cones.Match(Some: values => values.ForAll(item => item.Vertex >= 0 && item.Vertex < vertexCount && RhinoMath.IsValidDouble(x: item.HolonomyDeficit)), None: static () => true), key.OrDefault().InvalidInput()) select (VectorField)new CrossFieldCase(Space: space, Symmetry: symmetry, Constraints: constraints, Cones: cones);
    public static Fin<VectorField> VectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op? key = null) =>
        from _ in FieldNabla.MeshVertices(space: space, vertices: sources.Map(static s => s.Vertex), allowEmpty: false, key: key.OrDefault()) from t in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: time) select (VectorField)new VectorHeatCase(Space: space, Sources: sources, Time: t);
    public static Fin<VectorField> GeodesicTangent(MeshSpace space, Seq<int> sources, Op? key = null) =>
        FieldNabla.MeshVertices(space: space, vertices: sources, allowEmpty: false, key: key.OrDefault()).Map(_ => (VectorField)new GeodesicTangentCase(Space: space, Sources: sources));
    public static Fin<VectorField> Hodge(VectorField source, MeshSpace space, BoundarySense? sense = null, Op? key = null) =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput()) from _ in FieldNabla.MeshOf(space: space, key: key.OrDefault()) select (VectorField)new HodgeCase(Source: active, Space: space, Sense: sense ?? BoundarySense.Toward);
    public static Fin<VectorField> CurlNoise(ScalarField potential, double epsilon, Op? key = null) =>
        potential is ScalarField.NoiseCase { Kind: var kind } && kind == NoiseKind.Worley
            ? Fin.Fail<VectorField>(key.OrDefault().Unsupported(geometryType: typeof(ScalarField.NoiseCase), outputType: typeof(VectorField)))
            : from active in Optional(potential).ToFin(key.OrDefault().InvalidInput()) from eps in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: epsilon) select (VectorField)new CurlNoiseCase(Potential: active, Epsilon: eps, RaisesCaution: potential is ScalarField.NoiseCase nc && nc.Kind.RaisesCaution);
    public static VectorField operator +(VectorField left, VectorField right) =>
        new BlendCase(Fields: (left is BlendCase lb && lb.Mode.Equals(FieldBlend.Sum) ? lb.Fields : Seq(left)).Concat(right is BlendCase rb && rb.Mode.Equals(FieldBlend.Sum) ? rb.Fields : Seq(right)).ToSeq(), Mode: FieldBlend.Sum);
    public static VectorField operator -(VectorField left, VectorField right) => left + (-right);
    public static VectorField operator -(VectorField field) => new ScaledCase(Source: field, Scale: -1.0);
    public static VectorField operator *(VectorField field, double scale) => new ScaledCase(Source: field, Scale: scale);
    public static VectorField operator *(double scale, VectorField field) => new ScaledCase(Source: field, Scale: scale);
    internal Fin<Vector3d> SampleVector(Point3d sample, Context context, Op key) => Switch(
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
        geodesicTangentCase: static (state, c) => MeshKernel.GeodesicTangentAt(space: c.Space, sources: c.Sources, sample: state.Sample, key: state.Key));
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
            Vector3d perpVec = FieldNabla.PerpendicularComponent(r: r1, axis: t);
            double R = perpVec.Length;
            double angularFactor = (r1 * t / r1.Length) - (r2 * t / r2.Length);
            double prefactor = current / (4.0 * Math.PI * R);
            return R < tol || r1.Length < tol || r2.Length < tol ? Fin.Fail<Vector3d>(key.InvalidInput()) : key.AcceptValue(value: Vector3d.CrossProduct(a: t, b: perpVec / R) * prefactor * angularFactor);
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

[Union]
public partial record TensorField {
    public sealed record ConstantCase(SymmetricMatrix Value) : TensorField; public sealed record CurvatureCase(SurfaceSpace Space) : TensorField; public sealed record LiftCase(Func<Point3d, SymmetricMatrix> Sampler) : TensorField;
    public sealed record WarpCase(TensorField Source, Transform Spatial) : TensorField; public sealed record ScaledCase(TensorField Source, double Scale) : TensorField; public sealed record BlendCase(Seq<TensorField> Fields, FieldBlend Mode) : TensorField;
    public static TensorField Constant(SymmetricMatrix value) => new ConstantCase(Value: value);
    public static Fin<TensorField> Lift(Func<Point3d, SymmetricMatrix>? sampler, Op? key = null) =>
        Optional(sampler).ToFin(key.OrDefault().InvalidInput()).Map(active => (TensorField)new LiftCase(Sampler: active));
    internal Fin<SymmetricMatrix> SampleTensor(Point3d sample, Context context, Op key) => Switch(
        state: (Sample: sample, Context: context, Key: key),
        constantCase: static (s, c) => c.Value.IsValid ? Fin.Succ(c.Value) : Fin.Fail<SymmetricMatrix>(s.Key.InvalidResult()),
        curvatureCase: static (s, c) => c.Space.Native.ClosestPoint(testPoint: s.Sample, u: out double u, v: out double v) switch {
            false => Fin.Fail<SymmetricMatrix>(error: s.Key.InvalidResult()),
            true => SurfaceProjection.ShapeOperator.Project<SymmetricMatrix>(surface: c.Space.Native, u: u, v: v, context: c.Space.Tolerance, key: s.Key),
        },
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
            .Bind(tensors => BlendTensors(tensors: tensors, mode: c.Mode, key: s.Key)));
    private static Fin<SymmetricMatrix> BlendTensors(Seq<SymmetricMatrix> tensors, FieldBlend mode, Op key) =>
        tensors.IsEmpty || tensors.Exists(t => !t.IsValid || t.Dimension.Value != tensors[index: 0].Dimension.Value)
            ? Fin.Fail<SymmetricMatrix>(error: key.InvalidResult())
            : Fin.Succ(new SymmetricMatrix(
                Dimension: tensors[index: 0].Dimension,
                Upper: new Arr<double>([.. Enumerable.Range(start: 0, count: tensors[index: 0].Upper.Count)
                    .Select(i => tensors.Fold(initialState: 0.0, f: (sum, tensor) => sum + tensor.Upper[index: i]) / (mode.Equals(FieldBlend.Sum) ? 1 : tensors.Count))])));
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

[Union]
public partial record ScalarField {
    public sealed record ConstantCase(double Value) : ScalarField; public sealed record BlendCase(Seq<ScalarField> Fields, FieldBlend Mode) : ScalarField; public sealed record ScaledCase(ScalarField Source, double Scale) : ScalarField;
    public sealed record DistanceCase(SupportSpace Source, BoundarySense Sense) : ScalarField; public sealed record PotentialCase(Seq<(Point3d Position, double Charge)> Charges, Falloff Falloff) : ScalarField; public sealed record DensityCase(Point3d Center, PositiveMagnitude Spread, double Strength) : ScalarField;
    public sealed record MagnitudeCase(VectorField Source) : ScalarField; public sealed record DivergenceCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField; public sealed record LaplacianCase(ScalarField Source, PositiveMagnitude Epsilon) : ScalarField; public sealed record StrainMagnitudeCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField;
    public sealed record WorleyCase(Seq<Point3d> Seeds, int Order) : ScalarField; public sealed record MorseCase(Point3d Center, PositiveMagnitude Depth, PositiveMagnitude Width) : ScalarField; public sealed record MollifierCase(Point3d Center, PositiveMagnitude Radius) : ScalarField; public sealed record NoiseCase(NoiseKind Kind, int Seed, int Octaves, double Persistence, double Lacunarity, double Frequency) : ScalarField;
    public sealed record PowerCase(ScalarField Source, double Exponent) : ScalarField; public sealed record CsgCase(ScalarField Left, ScalarField Right, CsgKind Op, BlendKind Smoothing) : ScalarField; public sealed record PeriodicCase(ScalarField Source, Vector3d Period) : ScalarField; public sealed record ClampCase(ScalarField Source, double Minimum, double Maximum) : ScalarField;
    public sealed record PrimitiveCase(SdfKind Kind, ImmutableDictionary<string, double> Parameters, Plane Pose) : ScalarField; public sealed record OnionCase(ScalarField Source, PositiveMagnitude Thickness) : ScalarField; public sealed record SdfRoundCase(ScalarField Source, PositiveMagnitude Radius) : ScalarField; public sealed record ElongateCase(ScalarField Source, Vector3d Extent) : ScalarField;
    public sealed record DisplaceCase(ScalarField Source, ScalarField Displacement) : ScalarField; public sealed record TwistCase(ScalarField Source, double AnglePerUnit, Direction Axis) : ScalarField; public sealed record BendCase(ScalarField Source, double Curvature, Direction Axis) : ScalarField;
    public sealed record GeodesicCase(MeshSpace Space, Seq<int> Sources) : ScalarField; public sealed record MeanCurvatureFlowCase(MeshSpace Space, PositiveMagnitude TimeStep, Dimension Iterations) : ScalarField; public sealed record SpectralDistanceCase(MeshSpace Space, SpectralFilter Filter, Seq<int> Sources, Dimension Pairs) : ScalarField; public sealed record StripeCase(MeshSpace Space, VectorField CrossField, PositiveMagnitude Frequency) : ScalarField; public sealed record SignedDistanceFromMeshCase(MeshSpace Space, SdfMeshMethod Method) : ScalarField;
    public sealed record RbfCase(Seq<(Point3d Position, double Value)> Samples, KernelKind Kernel, PositiveMagnitude Radius, Arr<double> Coefficients, ReconstructionReceipt Receipt) : ScalarField;
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
        from active in Optional(kind).ToFin(key.OrDefault().InvalidInput()) from validParams in Optional(parameters).ToFin(key.OrDefault().InvalidInput()) from _ in guard(active.ValidateParameters(parameters: validParams), key.OrDefault().InvalidInput()) from validPose in key.OrDefault().AcceptValue(value: pose) select (ScalarField)new PrimitiveCase(Kind: active, Parameters: validParams, Pose: validPose);
    public static Fin<ScalarField> Noise(NoiseKind kind, int seed, int octaves, double persistence, double lacunarity, double frequency, Op? key = null) =>
        from active in Optional(kind).ToFin(key.OrDefault().InvalidInput()) from _ in guard(octaves is >= 1 and <= 32 && RhinoMath.IsValidDouble(x: frequency) && frequency > 0.0 && RhinoMath.IsValidDouble(x: persistence) && persistence is > 0.0 and <= 1.0 && RhinoMath.IsValidDouble(x: lacunarity) && lacunarity > 1.0, key.OrDefault().InvalidInput()) select (ScalarField)new NoiseCase(Kind: active, Seed: seed, Octaves: octaves, Persistence: persistence, Lacunarity: lacunarity, Frequency: frequency);
    public static Fin<ScalarField> Onion(ScalarField source, double thickness, Op? key = null) =>
        FieldNabla.WithPositive(candidate: thickness, make: t => (ScalarField)new OnionCase(Source: source, Thickness: t), key: key);
    public static Fin<ScalarField> SdfRound(ScalarField source, double radius, Op? key = null) =>
        FieldNabla.WithPositive(candidate: radius, make: r => (ScalarField)new SdfRoundCase(Source: source, Radius: r), key: key);
    public static Fin<ScalarField> Elongate(ScalarField source, Vector3d extent, Op? key = null) =>
        extent.IsValid && extent.X >= 0.0 && extent.Y >= 0.0 && extent.Z >= 0.0 ? Fin.Succ((ScalarField)new ElongateCase(Source: source, Extent: extent)) : Fin.Fail<ScalarField>(key.OrDefault().InvalidInput());
    public static Fin<ScalarField> Twist(ScalarField source, double anglePerUnit, Direction axis, Op? key = null) =>
        RhinoMath.IsValidDouble(x: anglePerUnit)
            ? Fin.Succ((ScalarField)new TwistCase(Source: source, AnglePerUnit: anglePerUnit, Axis: axis))
            : Fin.Fail<ScalarField>(key.OrDefault().InvalidInput());
    public static Fin<ScalarField> Bend(ScalarField source, double curvature, Direction axis, Op? key = null) =>
        RhinoMath.IsValidDouble(x: curvature)
            ? Fin.Succ((ScalarField)new BendCase(Source: source, Curvature: curvature, Axis: axis))
            : Fin.Fail<ScalarField>(key.OrDefault().InvalidInput());
    public static Fin<ScalarField> Geodesic(MeshSpace space, Seq<int> sources, Op? key = null) =>
        FieldNabla.MeshVertices(space: space, vertices: sources, allowEmpty: false, key: key.OrDefault()).Map(_ => (ScalarField)new GeodesicCase(Space: space, Sources: sources));
    public static Fin<ScalarField> MeanCurvatureFlow(MeshSpace space, double timeStep, int iterations, Op? key = null) =>
        from _ in FieldNabla.MeshOf(space: space, key: key.OrDefault()) from t in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: timeStep) from count in key.OrDefault().AcceptValidated<Dimension>(candidate: iterations) select (ScalarField)new MeanCurvatureFlowCase(Space: space, TimeStep: t, Iterations: count);
    public static Fin<ScalarField> SpectralDistance(MeshSpace space, SpectralFilter filter, Seq<int> sources, int pairs, Op? key = null) =>
        from _ in FieldNabla.MeshVertices(space: space, vertices: sources, allowEmpty: true, key: key.OrDefault()) from active in Optional(filter).ToFin(key.OrDefault().InvalidInput()) from count in key.OrDefault().AcceptValidated<Dimension>(candidate: pairs) select (ScalarField)new SpectralDistanceCase(Space: space, Filter: active, Sources: sources, Pairs: count);
    public static Fin<ScalarField> Stripe(MeshSpace space, VectorField crossField, double frequency, Op? key = null) =>
        from _ in FieldNabla.MeshOf(space: space, key: key.OrDefault()) from active in Optional(crossField).ToFin(key.OrDefault().InvalidInput()) from freq in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: frequency) select (ScalarField)new StripeCase(Space: space, CrossField: active, Frequency: freq);
    public static Fin<ScalarField> SignedDistanceFromMesh(MeshSpace space, SdfMeshMethod method, Op? key = null) =>
        from _ in FieldNabla.MeshOf(space: space, key: key.OrDefault()) from active in Optional(method).ToFin(key.OrDefault().InvalidInput()) select (ScalarField)new SignedDistanceFromMeshCase(Space: space, Method: active);
    public static Fin<ReconstructionResult> RbfDetailed(Seq<(Point3d Position, double Value)> samples, KernelKind kernel, double radius, double smoothing = 0.0, Op? key = null) =>
        from active in Optional(kernel).ToFin(key.OrDefault().InvalidInput())
        from r in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius)
        from _ in guard(!samples.IsEmpty && samples.ForAll(static s => s.Position.IsValid && RhinoMath.IsValidDouble(x: s.Value)) && RhinoMath.IsValidDouble(x: smoothing) && smoothing >= 0.0, key.OrDefault().InvalidInput())
        from solved in SolveRbf(samples: samples, kernel: active, radius: r.Value, smoothing: smoothing, key: key.OrDefault())
        let receipt = new ReconstructionReceipt(Mode: smoothing <= RhinoMath.ZeroTolerance ? ReconstructionMode.RbfInterpolation : ReconstructionMode.RbfApproximation, Kernel: active, Radius: r.Value, Smoothing: smoothing, Interpolation: smoothing <= RhinoMath.ZeroTolerance, SampleCount: samples.Count, CenterCount: samples.Count, PolynomialDegree: 0, Solve: solved)
        select new ReconstructionResult(Field: new RbfCase(Samples: samples, Kernel: active, Radius: r, Coefficients: solved.Solution, Receipt: receipt), Receipt: receipt);
    private static Fin<SolveReceipt> SolveRbf(Seq<(Point3d Position, double Value)> samples, KernelKind kernel, double radius, double smoothing, Op key) {
        int n = samples.Count;
        bool interpolation = smoothing <= RhinoMath.ZeroTolerance;
        (Point3d Position, double Value)[] sampleArray = [.. samples.AsIterable()];
        Arr<double> rhs = new([.. sampleArray.Select(static s => s.Value)]);
        Dimension cols = Dimension.Create(value: n);
        return KernelWeights(left: sampleArray.Select(static s => s.Position), right: sampleArray.Select(static s => s.Position), kernel: kernel, radius: radius, key: key)
            .Bind(kernelEntries =>
                from matrix in Matrix.Of(
                    rows: Dimension.Create(value: interpolation ? n : 2 * n), cols: cols,
                    entries: interpolation ? kernelEntries : new Arr<double>([.. kernelEntries.AsIterable().Concat(Enumerable.Range(start: 0, count: n * n).Select(i => i / n == i % n ? Math.Sqrt(d: smoothing) : 0.0))]), key: key)
                from solved in interpolation
                    ? matrix.SolveDetailed(rhs: rhs, key: key)
                    : matrix.LeastSquaresDetailed(rhs: new Arr<double>([.. rhs.AsIterable().Concat(Enumerable.Repeat(element: 0.0, count: n))]), key: key)
                select solved);
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
    public static Fin<ScalarField> Periodic(ScalarField source, Vector3d period, Op? key = null) =>
        !period.IsValid || Math.Abs(value: period.X) <= RhinoMath.ZeroTolerance || Math.Abs(value: period.Y) <= RhinoMath.ZeroTolerance || Math.Abs(value: period.Z) <= RhinoMath.ZeroTolerance
            ? Fin.Fail<ScalarField>(key.OrDefault().InvalidInput())
            : Fin.Succ((ScalarField)new PeriodicCase(Source: source, Period: period));
    public static Fin<ScalarField> StrainMagnitude(VectorField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<VectorField, ScalarField>(source, epsilon, static (s, e) => new StrainMagnitudeCase(Source: s, Epsilon: e), key);
    public static Fin<ScalarField> Clamp(ScalarField source, double minimum, double maximum, Op? key = null) =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput()) from _ in guard(RhinoMath.IsValidDouble(x: minimum) && RhinoMath.IsValidDouble(x: maximum) && minimum <= maximum, key.OrDefault().InvalidInput()) select (ScalarField)new ClampCase(Source: active, Minimum: minimum, Maximum: maximum);
    public Option<double> LipschitzBound() => this switch {
        PrimitiveCase p => Some(p.Kind.Lipschitz),
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
    public Fin<SdfSample> SampleSdfDetailed(Point3d sample, Context context, Op? key = null) => this switch {
        PrimitiveCase => from value in SampleScalar(sample: sample, context: context, key: key.OrDefault()) select new SdfSample(Value: value, Receipt: SdfReceiptOf(field: this, status: SdfStatus.Analytic, mesh: Option<SdfMeshReceipt>.None)),
        SignedDistanceFromMeshCase meshCase => from signed in MeshKernel.SignedDistanceFromMeshDetailed(space: meshCase.Space, method: meshCase.Method, sample: sample, key: key.OrDefault()) select new SdfSample(Value: signed.Distance, Receipt: SdfReceiptOf(field: this, status: SdfStatus.MeshApproximate, mesh: Some(signed.Receipt))),
        _ => from _ in LipschitzBound().ToFin(key.OrDefault().Unsupported(geometryType: GetType(), outputType: typeof(SdfSample))) from value in SampleScalar(sample: sample, context: context, key: key.OrDefault()) select new SdfSample(Value: value, Receipt: SdfReceiptOf(field: this, status: SdfStatus.ComposedAnalytic, mesh: Option<SdfMeshReceipt>.None)),
    };
    private static SdfReceipt SdfReceiptOf(ScalarField field, SdfStatus status, Option<SdfMeshReceipt> mesh) =>
        new(Status: status, LipschitzBound: field.LipschitzBound(), AnalyticPrimitive: field is PrimitiveCase, MeshBacked: field is SignedDistanceFromMeshCase, WatertightPreflight: mesh.Map(static receipt => receipt.IsSolid && receipt.IsManifold && receipt.IsOriented), LossyFallback: status.Equals(SdfStatus.LossyFallback), Mesh: mesh);
    public Fin<IsoSurfaceResult> IsoSurfaceDetailed(BoundingBox bounds, int resolution, int maxRootSteps, Context context, Op? key = null) {
        Op op = key.OrDefault();
        ScalarField self = this;
        return !BoundsAdmitted(bounds: bounds) || resolution < 2 || maxRootSteps < 1
            ? Fin.Fail<IsoSurfaceResult>(op.InvalidInput())
            : op.Catch(() => {
                int failures = 0;
                double EvaluateIso(Point3d point) =>
                    self.SampleScalar(sample: point, context: context, key: op)
                        .Match(
                            Succ: static value => value,
                            Fail: _ => {
                                failures = Interlocked.Increment(location: ref failures);
                                return double.NaN;
                            });
                Mesh result = Mesh.CreateFromIsosurface(
                    scalarFieldEvaluator: EvaluateIso,
                    box: bounds, resolution: resolution, RootFindingMaxSteps: maxRootSteps);
                return failures > 0
                    ? Fin.Fail<IsoSurfaceResult>(op.InvalidResult())
                    : result is null || !result.IsValid
                    ? Fin.Fail<IsoSurfaceResult>(op.InvalidResult())
                    : Fin.Succ(new IsoSurfaceResult(
                        Mesh: result,
                        Receipt: new IsoSurfaceReceipt(NativeRouted: true, Bounds: bounds, Resolution: resolution, MaxRootSteps: maxRootSteps, ParallelCallback: true, EvaluatorFailures: failures, Valid: result.IsValid, VertexCount: result.Vertices.Count, FaceCount: result.Faces.Count, FixedTolerance: Option<double>.None)));
            });
    }
    internal static bool BoundsAdmitted(BoundingBox bounds) =>
        bounds is { IsValid: true, Diagonal: Vector3d d }
        && d.X > RhinoMath.ZeroTolerance
        && d.Y > RhinoMath.ZeroTolerance
        && d.Z > RhinoMath.ZeroTolerance;
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
        signedDistanceFromMeshCase: static (state, c) => MeshKernel.SignedDistanceFromMeshDetailed(space: c.Space, method: c.Method, sample: state.Sample, key: state.Key).Map(static result => result.Distance),
        rbfCase: static (state, c) => EvaluateRbf(samples: c.Samples, kernel: c.Kernel, radius: c.Radius.Value, coefficients: c.Coefficients, sample: state.Sample, key: state.Key));
    private static Fin<double> SampleMapped<T>(ScalarField source, (Point3d Sample, Context Context, Op Key) state, T data, Func<T, double, double> map) =>
        source.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(value => state.Key.AcceptValue(value: map(arg1: data, arg2: value)));
}

internal static class FieldNabla {
    internal static readonly Vector3d CurlOffset2 = new(x: 31.4159, y: 27.1828, z: 41.4213), CurlOffset3 = new(x: -19.3274, y: 53.2186, z: -67.9531);
    internal static Fin<TResult> WithPositive<TResult>(double candidate, Func<PositiveMagnitude, TResult> make, Op? key) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: candidate).Map(make);
    internal static Fin<TResult> WithPositivePair<TResult>(double left, double right, Func<PositiveMagnitude, PositiveMagnitude, TResult> make, Op? key) =>
        from a in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: left) from b in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: right) select make(arg1: a, arg2: b);
    internal static Fin<TResult> WithDivisor<TResult>(double divisor, Func<double, TResult> make, Op? key) =>
        Math.Abs(value: divisor) > RhinoMath.ZeroTolerance ? Fin.Succ(make(arg: 1.0 / divisor)) : Fin.Fail<TResult>(key.OrDefault().InvalidInput());
    internal static Fin<TResult> WithSourceEpsilon<TSource, TResult>(TSource? source, double epsilon, Func<TSource, PositiveMagnitude, TResult> make, Op? key)
        where TSource : class =>
        from active in Optional(source).ToFin(key.OrDefault().InvalidInput()) from eps in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: epsilon) select make(active, eps);
    internal static Fin<Mesh> MeshOf(MeshSpace space, Op key) => Optional(space.Native).ToFin(key.InvalidInput());
    internal static Fin<Mesh> MeshVertices(MeshSpace space, Seq<int> vertices, bool allowEmpty, Op key) =>
        from mesh in MeshOf(space: space, key: key)
        from _ in guard((allowEmpty || !vertices.IsEmpty) && vertices.ForAll(vertex => vertex >= 0 && vertex < mesh.Vertices.Count), key.InvalidInput())
        select mesh;
    internal static Vector3d PerpendicularComponent(Vector3d r, Vector3d axis) => r - (r * axis * axis);
    internal static Point3d ToroidalWrap(Point3d sample, Vector3d period) =>
        new(x: sample.X - (Math.Floor(d: (sample.X / period.X) + 0.5) * period.X),
            y: sample.Y - (Math.Floor(d: (sample.Y / period.Y) + 0.5) * period.Y),
            z: sample.Z - (Math.Floor(d: (sample.Z / period.Z) + 0.5) * period.Z));
    internal static Fin<(T X1, T X0, T Y1, T Y0, T Z1, T Z0)> SampleAxes<T>(Func<Point3d, Fin<T>> sampler, Point3d point, double eps) =>
        from xp in sampler(arg: point + (eps * Vector3d.XAxis)) from xm in sampler(arg: point - (eps * Vector3d.XAxis)) from yp in sampler(arg: point + (eps * Vector3d.YAxis)) from ym in sampler(arg: point - (eps * Vector3d.YAxis)) from zp in sampler(arg: point + (eps * Vector3d.ZAxis)) from zm in sampler(arg: point - (eps * Vector3d.ZAxis)) select (X1: xp, X0: xm, Y1: yp, Y0: ym, Z1: zp, Z0: zm);
    internal static Fin<Vector3d> GradientAt(ScalarField field, Point3d point, double eps, Context context, Op key) =>
        from samples in SampleAxes(sampler: p => field.SampleScalar(sample: p, context: context, key: key), point: point, eps: eps) let inv2eps = 1.0 / (2.0 * eps) select new Vector3d(x: (samples.X1 - samples.X0) * inv2eps, y: (samples.Y1 - samples.Y0) * inv2eps, z: (samples.Z1 - samples.Z0) * inv2eps);
    internal static Fin<Vector3d> CurlAt(VectorField field, Point3d point, double eps, Context context, Op key) =>
        from samples in SampleAxes(sampler: p => field.SampleVector(sample: p, context: context, key: key), point: point, eps: eps) let inv2eps = 1.0 / (2.0 * eps) from curl in key.AcceptValue(value: new Vector3d(x: (samples.Y1.Z - samples.Y0.Z - (samples.Z1.Y - samples.Z0.Y)) * inv2eps, y: (samples.Z1.X - samples.Z0.X - (samples.X1.Z - samples.X0.Z)) * inv2eps, z: (samples.X1.Y - samples.X0.Y - (samples.Y1.X - samples.Y0.X)) * inv2eps)) select curl;
    internal static Fin<Vector3d> CurlNoiseAt(ScalarField field, Point3d point, double eps, Context context, Op key) =>
        from g1 in GradientAt(field: field, point: point, eps: eps, context: context, key: key) from g2 in GradientAt(field: field, point: point + CurlOffset2, eps: eps, context: context, key: key) from g3 in GradientAt(field: field, point: point + CurlOffset3, eps: eps, context: context, key: key) from raw in key.AcceptValue(value: new Vector3d(x: g3.Y - g2.Z, y: g1.Z - g3.X, z: g2.X - g1.Y)) select raw;
    internal static Fin<double> DivergenceAt(VectorField field, Point3d point, double eps, Context context, Op key) =>
        from samples in SampleAxes(sampler: p => field.SampleVector(sample: p, context: context, key: key), point: point, eps: eps) let inv2eps = 1.0 / (2.0 * eps) from value in key.AcceptValue(value: (samples.X1.X - samples.X0.X + samples.Y1.Y - samples.Y0.Y + samples.Z1.Z - samples.Z0.Z) * inv2eps) select value;
    internal static Fin<double> LaplacianAt(ScalarField field, Point3d point, double eps, Context context, Op key) =>
        from samples in SampleAxes(sampler: p => field.SampleScalar(sample: p, context: context, key: key), point: point, eps: eps) from center in field.SampleScalar(sample: point, context: context, key: key) let invEpsSq = 1.0 / (eps * eps) from value in key.AcceptValue(value: (samples.X1 + samples.X0 + samples.Y1 + samples.Y0 + samples.Z1 + samples.Z0 - (6.0 * center)) * invEpsSq) select value;
    internal static Fin<double> StrainMagnitudeAt(VectorField field, Point3d point, double eps, Context context, Op key) =>
        from samples in SampleAxes(sampler: p => field.SampleVector(sample: p, context: context, key: key), point: point, eps: eps) let inv2eps = 1.0 / (2.0 * eps) let sxx = (samples.X1.X - samples.X0.X) * inv2eps let syy = (samples.Y1.Y - samples.Y0.Y) * inv2eps let szz = (samples.Z1.Z - samples.Z0.Z) * inv2eps let sxy = 0.5 * (samples.Y1.X - samples.Y0.X + samples.X1.Y - samples.X0.Y) * inv2eps let sxz = 0.5 * (samples.Z1.X - samples.Z0.X + samples.X1.Z - samples.X0.Z) * inv2eps let syz = 0.5 * (samples.Z1.Y - samples.Z0.Y + samples.Y1.Z - samples.Y0.Z) * inv2eps from value in key.AcceptValue(value: Math.Sqrt(d: (sxx * sxx) + (syy * syy) + (szz * szz) + (2.0 * ((sxy * sxy) + (sxz * sxz) + (syz * syz))))) select value;
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

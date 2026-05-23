namespace Rasm.Vectors;

// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class SdfMeshMethod {
    public static readonly SdfMeshMethod GeneralizedWindingNumber = new(key: 0);
    public static readonly SdfMeshMethod SignedHeat = new(key: 1);
}

[SmartEnum<int>]
public sealed partial class FieldBlend {
    public static readonly FieldBlend Sum = new(key: 0, scale: static _ => 1.0);
    public static readonly FieldBlend Average = new(key: 1, scale: static count => count > 0 ? 1.0 / count : 1.0);
    internal Fin<Vector3d> Combine(Seq<Vector3d> vectors, Op key) =>
        CombineCore(values: vectors, zero: Vector3d.Zero, add: static (sum, v) => sum + v, scale: static (sum, factor) => sum * factor, key: key);
    internal Fin<double> CombineScalar(Seq<double> values, Op key) =>
        CombineCore(values: values, zero: 0.0, add: static (sum, v) => sum + v, scale: static (sum, factor) => sum * factor, key: key);
    private Fin<T> CombineCore<T>(Seq<T> values, T zero, Func<T, T, T> add, Func<T, double, T> scale, Op key) =>
        from _ in guard(!values.IsEmpty, key.InvalidResult())
        from value in key.AcceptValue(value: scale(arg1: values.Fold(initialState: zero, f: add), arg2: Scale(count: values.Count)))
        select value;
    [UseDelegateFromConstructor] private partial double Scale(int count);
}

// Op is a hard boolean (Union=min, Intersect=max, Difference=max(a,-b)); BlendKind owns
// the smoothing band and the Lipschitz penalty paid for it.
[SmartEnum<int>]
public sealed partial class CsgKind {
    public static readonly CsgKind Union = new(key: 0, combine: static (a, b, blend) => blend.Smin(a: a, b: b));
    public static readonly CsgKind Intersect = new(key: 1, combine: static (a, b, blend) => -blend.Smin(a: -a, b: -b));
    public static readonly CsgKind Difference = new(key: 2, combine: static (a, b, blend) => -blend.Smin(a: -a, b: b));
    [UseDelegateFromConstructor] internal partial double Combine(double left, double right, BlendKind blend);
}

// Quilez smin family. Smin blends scalars; Erode propagates the Lipschitz penalty paid
// for the smoothing band so sphere-tracing step deration stays inside the SDF contract.
[Union]
public abstract partial record BlendKind {
    private BlendKind() { }
    public sealed record HardCase : BlendKind;
    public sealed record PolynomialCase(double K) : BlendKind;
    public sealed record ExponentialCase(double K) : BlendKind;
    public sealed record RootCase(double K) : BlendKind;
    public sealed record CubicCase(double K) : BlendKind;
    public sealed record ChamferCase(double K) : BlendKind;
    public sealed record GrooveCase(double K, double D) : BlendKind;
    public sealed record RoundCase(double R) : BlendKind;
    public static BlendKind Hard => new HardCase();
    private static Fin<BlendKind> WithPositive(double k, Func<double, BlendKind> wrap, Op? key) {
        Op op = key.OrDefault();
        return RhinoMath.IsValidDouble(x: k) && k > 0.0 ? Fin.Succ(wrap(arg: k)) : Fin.Fail<BlendKind>(op.InvalidInput());
    }
    public static Fin<BlendKind> Polynomial(double k, Op? key = null) => WithPositive(k: k, wrap: static v => new PolynomialCase(K: v), key: key);
    public static Fin<BlendKind> Exponential(double k, Op? key = null) => WithPositive(k: k, wrap: static v => new ExponentialCase(K: v), key: key);
    public static Fin<BlendKind> Root(double k, Op? key = null) => WithPositive(k: k, wrap: static v => new RootCase(K: v), key: key);
    public static Fin<BlendKind> Cubic(double k, Op? key = null) => WithPositive(k: k, wrap: static v => new CubicCase(K: v), key: key);
    public static Fin<BlendKind> Chamfer(double k, Op? key = null) => WithPositive(k: k, wrap: static v => new ChamferCase(K: v), key: key);
    public static Fin<BlendKind> Round(double r, Op? key = null) => WithPositive(k: r, wrap: static v => new RoundCase(R: v), key: key);
    public static Fin<BlendKind> Groove(double k, double d, Op? key = null) {
        Op op = key.OrDefault();
        return RhinoMath.IsValidDouble(x: k) && k > 0.0 && RhinoMath.IsValidDouble(x: d) && d > 0.0
            ? Fin.Succ<BlendKind>(new GrooveCase(K: k, D: d))
            : Fin.Fail<BlendKind>(op.InvalidInput());
    }
    internal double Smin(double a, double b) => Switch(
        state: (A: a, B: b),
        hardCase: static (s, _) => Math.Min(val1: s.A, val2: s.B),
        polynomialCase: static (s, c) => {
            double h = Math.Max(val1: c.K - Math.Abs(value: s.A - s.B), val2: 0.0) / c.K;
            return Math.Min(val1: s.A, val2: s.B) - (h * h * h * c.K * (1.0 / 6.0));
        },
        exponentialCase: static (s, c) => -Math.Log(d: Math.Exp(d: -c.K * s.A) + Math.Exp(d: -c.K * s.B)) / c.K,
        rootCase: static (s, c) => {
            double h = Math.Max(val1: c.K - Math.Abs(value: s.A - s.B), val2: 0.0);
            return Math.Min(val1: s.A, val2: s.B) - (h * h * 0.25 / c.K);
        },
        cubicCase: static (s, c) => {
            double h = Math.Max(val1: c.K - Math.Abs(value: s.A - s.B), val2: 0.0) / c.K;
            return Math.Min(val1: s.A, val2: s.B) - (h * h * c.K * 0.25);
        },
        chamferCase: static (s, c) => Math.Min(val1: Math.Min(val1: s.A, val2: s.B), val2: (s.A + s.B - c.K) * 0.7071067811865475),
        grooveCase: static (s, c) => Math.Max(val1: s.A, val2: Math.Min(val1: c.D, val2: Math.Min(val1: s.A - c.K, val2: s.B - c.K))),
        roundCase: static (s, c) => {
            double ax = Math.Max(val1: c.R - s.A, val2: 0.0);
            double bx = Math.Max(val1: c.R - s.B, val2: 0.0);
            return Math.Max(val1: c.R, val2: Math.Min(val1: s.A, val2: s.B)) - Math.Sqrt(d: (ax * ax) + (bx * bx));
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

// Quilez SDF primitives in canonical local pose (origin-centered, axis-aligned). Bounded
// cases (ConeBound/Octahedron/Ellipsoid) over-estimate; consumers read Lipschitz to derate.
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
        compute: static (p, ps) => {
            double pz = p.Z - Math.Clamp(value: p.Z, min: -ps["h"], max: ps["h"]);
            return Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y) + (pz * pz)) - ps["r"];
        });
    public static readonly SdfKind Cylinder = new(key: 3, lipschitz: 1.0, requiredKeys: Seq("h", "r"),
        validate: static ps => ps["h"] > RhinoMath.ZeroTolerance && ps["r"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => {
            double dxy = Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y)) - ps["r"]; double dz = Math.Abs(value: p.Z) - ps["h"];
            double oxy = Math.Max(val1: dxy, val2: 0.0); double oz = Math.Max(val1: dz, val2: 0.0);
            return Math.Sqrt(d: (oxy * oxy) + (oz * oz)) + Math.Min(val1: Math.Max(val1: dxy, val2: dz), val2: 0.0);
        });
    public static readonly SdfKind Cone = new(key: 4, lipschitz: 1.0, requiredKeys: Seq("h", "angle"),
        validate: static ps => ps["h"] > RhinoMath.ZeroTolerance && ps["angle"] > RhinoMath.ZeroTolerance && ps["angle"] < Math.PI,
        compute: static (p, ps) => {
            double qx = Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y));
            double sn = Math.Sin(a: ps["angle"]); double cs = Math.Cos(d: ps["angle"]);
            return Math.Max(val1: (qx * cs) - (p.Z * sn), val2: -p.Z - ps["h"]);
        });
    public static readonly SdfKind ConeBound = new(key: 5, lipschitz: 1.7, requiredKeys: Seq("h", "angle"),
        validate: static ps => ps["h"] > RhinoMath.ZeroTolerance && ps["angle"] > RhinoMath.ZeroTolerance && ps["angle"] < Math.PI,
        compute: static (p, ps) => Math.Max(val1: (Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y)) * Math.Cos(d: ps["angle"])) - (p.Z * Math.Sin(a: ps["angle"])), val2: -p.Z - ps["h"]));
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
        compute: static (p, ps) => {
            double qx = Math.Sqrt(d: (p.X * p.X) + (p.Y * p.Y)) - ps["R"];
            return Math.Sqrt(d: (qx * qx) + (p.Z * p.Z)) - ps["r"];
        });
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
    public static readonly SdfKind Octahedron = new(key: 9, lipschitz: 1.7320508075688772, requiredKeys: Seq("s"),
        validate: static ps => ps["s"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => (Math.Abs(value: p.X) + Math.Abs(value: p.Y) + Math.Abs(value: p.Z) - ps["s"]) * 0.5773502691896258);
    public static readonly SdfKind OctahedronExact = new(key: 10, lipschitz: 1.0, requiredKeys: Seq("s"),
        validate: static ps => ps["s"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => SdfExactOctahedron(p: p, s: ps["s"]));
    public static readonly SdfKind Ellipsoid = new(key: 11, lipschitz: 2.0, requiredKeys: Seq("x", "y", "z"),
        validate: static ps => ps["x"] > RhinoMath.ZeroTolerance && ps["y"] > RhinoMath.ZeroTolerance && ps["z"] > RhinoMath.ZeroTolerance,
        compute: static (p, ps) => {
            double ax = ps["x"]; double ay = ps["y"]; double az = ps["z"];
            double k0 = Math.Sqrt(d: (p.X * p.X / (ax * ax)) + (p.Y * p.Y / (ay * ay)) + (p.Z * p.Z / (az * az)));
            double k1 = Math.Sqrt(d: (p.X * p.X / (ax * ax * ax * ax)) + (p.Y * p.Y / (ay * ay * ay * ay)) + (p.Z * p.Z / (az * az * az * az)));
            return k1 > RhinoMath.ZeroTolerance ? k0 * (k0 - 1.0) / k1 : 0.0;
        });
    public double Lipschitz { get; }
    public Seq<string> RequiredKeys { get; }
    [UseDelegateFromConstructor] private partial bool Validate(ImmutableDictionary<string, double> parameters);
    [UseDelegateFromConstructor] private partial double Compute(Point3d local, ImmutableDictionary<string, double> parameters);
    internal Fin<double> SignedDistance(Point3d worldPoint, ImmutableDictionary<string, double> parameters, Plane pose, Op key) =>
        pose.RemapToPlaneSpace(ptSample: worldPoint, ptPlane: out Point3d local)
            ? key.AcceptValue(value: Compute(local: local, parameters: parameters))
            : Fin.Fail<double>(key.InvalidResult());
    internal bool ValidateParameters(ImmutableDictionary<string, double> parameters) =>
        RequiredKeys.ForAll(k => parameters.ContainsKey(key: k) && RhinoMath.IsValidDouble(x: parameters[k]))
        && Validate(parameters: parameters);
    // Quilez exact octahedron — three-region case analysis around the axis-permuted normal.
    private static double SdfExactOctahedron(Point3d p, double s) {
        double ax = Math.Abs(value: p.X); double ay = Math.Abs(value: p.Y); double az = Math.Abs(value: p.Z);
        double m = ax + ay + az - s;
        (double qx, double qy, double qz) = (3.0 * ax < m) ? (ax, ay, az)
            : (3.0 * ay < m) ? (ay, az, ax)
            : (3.0 * az < m) ? (az, ax, ay)
            : ((double)0.0, (double)0.0, (double)0.0);
        if (qx == 0.0 && qy == 0.0 && qz == 0.0) return m * 0.5773502691896258;
        double k = Math.Clamp(value: 0.5 * (qz - qy + s), min: 0.0, max: s);
        double dx = qx; double dy = qy - s + k; double dz = qz - k;
        return Math.Sqrt(d: (dx * dx) + (dy * dy) + (dz * dz));
    }
}

[SmartEnum<int>]
public sealed partial class NoiseKind {
    public static readonly NoiseKind Perlin = new(key: 0, raisesCaution: true,
        sample: static (p, seed, freq) => FieldNoise.PerlinAt(point: p, seed: seed, frequency: freq));
    public static readonly NoiseKind Simplex = new(key: 1, raisesCaution: false,
        sample: static (p, seed, freq) => FieldNoise.SkewedSimplexAt(point: p, seed: seed, frequency: freq, smooth: false));
    public static readonly NoiseKind SmoothSimplex = new(key: 2, raisesCaution: false,
        sample: static (p, seed, freq) => FieldNoise.SkewedSimplexAt(point: p, seed: seed, frequency: freq, smooth: true));
    public static readonly NoiseKind Worley = new(key: 3, raisesCaution: false,
        sample: static (p, seed, freq) => FieldNoise.WorleyAt(point: p, seed: seed, frequency: freq));
    public bool RaisesCaution { get; }
    [UseDelegateFromConstructor] internal partial double Sample(Point3d point, int seed, double frequency);
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

// --- [MODELS] -----------------------------------------------------------------------------
[Union]
public abstract partial record Falloff {
    private Falloff() { }
    public sealed record ConstantCase : Falloff;
    public sealed record InverseCase : Falloff;
    public sealed record InverseSquareCase : Falloff;
    public sealed record GaussianCase(PositiveMagnitude Spread) : Falloff;
    public sealed record KernelCase(KernelKind Kind, PositiveMagnitude Radius) : Falloff;
    public static Falloff Constant => new ConstantCase();
    public static Falloff Inverse => new InverseCase();
    public static Falloff InverseSquare => new InverseSquareCase();
    public static Fin<Falloff> Gaussian(double spread, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: spread).Map(static value => (Falloff)new GaussianCase(Spread: value));
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
        gaussianCase: static (state, gaussian) => Fin.Succ(Math.Exp(-(state.Distance * state.Distance) / (2.0 * gaussian.Spread.Value * gaussian.Spread.Value))),
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
    public sealed record HitFieldCase(SupportSpace Source, SupportProjection Projection, BoundarySense Sense) : VectorField;
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
    public sealed record CurlNoiseCase(ScalarField Potential, PositiveMagnitude Epsilon, bool RaisesCaution) : VectorField;
    public sealed record CrossFieldCase(MeshSpace Space, int Symmetry, Option<Seq<(int Vertex, Direction Hint)>> Constraints, Option<Seq<(int Vertex, double HolonomyDeficit)>> Cones) : VectorField;
    public sealed record HodgeIrrotationalCase(VectorField Source, MeshSpace Space) : VectorField;
    public sealed record HodgeSolenoidalCase(VectorField Source, MeshSpace Space) : VectorField;
    public sealed record VectorHeatCase(MeshSpace Space, Seq<(int Vertex, Vector3d Direction)> Sources, PositiveMagnitude Time) : VectorField;
    public sealed record GeodesicTangentCase(MeshSpace Space, Seq<int> Sources) : VectorField;
    public static VectorField Constant(Vector3d value) => new ConstantCase(Value: value);
    public static VectorField Influence(SupportSpace source, Falloff? falloff = null, BoundarySense? sense = null) =>
        new InfluenceCase(Source: source, Falloff: falloff ?? Falloff.Inverse, Sense: sense ?? BoundarySense.Toward, Radius: Option<PositiveMagnitude>.None);
    public static Fin<VectorField> Hit(SupportSpace source, SupportProjection projection, BoundarySense? sense = null, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from selected in Optional(projection).ToFin(op.InvalidInput())
               from _ in guard(selected.CanProjectVector(space: active), op.Unsupported(active.SourceType, typeof(Vector3d)))
               select (VectorField)new HitFieldCase(Source: active, Projection: selected, Sense: sense ?? BoundarySense.Toward);
    }
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
                from f in falloff is null ? Falloff.Gaussian(spread: r.Value / 3.0, key: op) : Fin.Succ(falloff)
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
    public static Fin<VectorField> Gradient(ScalarField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<ScalarField, VectorField>(source, epsilon,
            static (s, e) => new GradientCase(Source: s, Epsilon: e), key);
    public static Fin<VectorField> Curl(VectorField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<VectorField, VectorField>(source, epsilon,
            static (s, e) => new CurlCase(Source: s, Epsilon: e), key);
    public static Fin<VectorField> Ring(Point3d center, Direction axis, double radius, Falloff? falloff = null, Op? key = null) {
        Op op = key.OrDefault();
        return from r in op.AcceptValidated<PositiveMagnitude>(candidate: radius)
               from f in falloff is null ? Falloff.Gaussian(spread: radius / 3.0, key: op) : Fin.Succ(falloff)
               select (VectorField)new RingCase(Center: center, Axis: axis, Radius: r, Falloff: f);
    }
    public static VectorField Helical(Point3d anchor, Direction axis, double axial, double swirl, Falloff? falloff = null) =>
        new HelicalCase(Anchor: anchor, Axis: axis, Axial: axial, Swirl: swirl, Falloff: falloff ?? Falloff.Constant);
    public static Fin<VectorField> BiotSavart(Point3d start, Point3d end, double current, Op? key = null) {
        Op op = key.OrDefault();
        return from a in op.AcceptValue(value: start)
               from b in op.AcceptValue(value: end)
               from i in op.AcceptValue(value: current)
               from _ in guard(!(a - b).IsTiny(), op.InvalidInput())
               select (VectorField)new BiotSavartCase(Start: a, End: b, Current: i);
    }
    public static Fin<VectorField> Saddle(Point3d anchor, Plane basis, double strength, Op? key = null) {
        Op op = key.OrDefault();
        return basis.IsValid
            ? Fin.Succ((VectorField)new SaddleCase(Anchor: anchor, Basis: basis, Strength: strength))
            : Fin.Fail<VectorField>(op.InvalidInput());
    }
    public static VectorField CrossProduct(VectorField left, VectorField right) =>
        new CrossProductCase(Left: left, Right: right);
    public static Fin<VectorField> CrossField(MeshSpace space, int symmetry, Option<Seq<(int Vertex, Direction Hint)>> constraints = default, Option<Seq<(int Vertex, double HolonomyDeficit)>> cones = default, Op? key = null) {
        Op op = key.OrDefault();
        return symmetry is 1 or 2 or 4 or 6
            ? Fin.Succ((VectorField)new CrossFieldCase(Space: space, Symmetry: symmetry, Constraints: constraints, Cones: cones))
            : Fin.Fail<VectorField>(op.InvalidInput());
    }
    // Sharp-Soliman-Crane SIGGRAPH 2019 vector heat method via spectral expansion: encode source
    // directions in tangent-frame complex coordinates, project onto Hermitian connection-Laplacian
    // eigenbasis, evolve each mode by e^(-t lambda_k), reconstruct at query vertex.
    public static Fin<VectorField> VectorHeat(MeshSpace space, Seq<(int Vertex, Vector3d Direction)> sources, double time, Op? key = null) {
        Op op = key.OrDefault();
        return sources.IsEmpty || sources.Exists(s => s.Vertex < 0 || s.Vertex >= space.Native.Vertices.Count)
            ? Fin.Fail<VectorField>(op.InvalidInput())
            : op.AcceptValidated<PositiveMagnitude>(candidate: time)
                .Map(t => (VectorField)new VectorHeatCase(Space: space, Sources: sources, Time: t));
    }
    // Tangent vector of normalised geodesic distance from sources -- gradient of HeatGeodesic
    // applied to the surface frame at the query point.
    public static Fin<VectorField> GeodesicTangent(MeshSpace space, Seq<int> sources, Op? key = null) {
        Op op = key.OrDefault();
        return sources.IsEmpty || sources.Exists(i => i < 0 || i >= space.Native.Vertices.Count)
            ? Fin.Fail<VectorField>(op.InvalidInput())
            : Fin.Succ((VectorField)new GeodesicTangentCase(Space: space, Sources: sources));
    }
    // Bhatia-Norgard-Pascucci-Bremer 2013 Helmholtz-Hodge: F = ∇φ + ∇×ψ + harmonic.
    // sense = Toward selects the irrotational part (∇φ); Away selects the solenoidal residual.
    public static Fin<VectorField> Hodge(VectorField source, MeshSpace space, BoundarySense? sense = null, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(source).ToFin(op.InvalidInput())
            .Map<VectorField>(active => (sense ?? BoundarySense.Toward).Equals(BoundarySense.Toward)
                ? new HodgeIrrotationalCase(Source: active, Space: space)
                : new HodgeSolenoidalCase(Source: active, Space: space));
    }
    public static Fin<VectorField> CurlNoise(ScalarField potential, double epsilon, Op? key = null) {
        Op op = key.OrDefault();
        bool isWorley = potential is ScalarField.NoiseCase wn && wn.Kind == NoiseKind.Worley;
        bool isCaution = potential is ScalarField.NoiseCase nc && nc.Kind.RaisesCaution;
        return isWorley
            ? Fin.Fail<VectorField>(op.Unsupported(geometryType: typeof(ScalarField.NoiseCase), outputType: typeof(VectorField)))
            : from active in Optional(potential).ToFin(op.InvalidInput())
              from eps in op.AcceptValidated<PositiveMagnitude>(candidate: epsilon)
              select (VectorField)new CurlNoiseCase(Potential: active, Epsilon: eps, RaisesCaution: isCaution);
    }
    public static VectorField Zero { get; } = Constant(value: Vector3d.Zero);
    private static Seq<VectorField> FlattenSum(VectorField field) =>
        field is BlendCase b && b.Mode.Equals(FieldBlend.Sum) ? b.Fields : Seq(field);
    // Monoid: associative under flatten-into-BlendCase(Sum); Zero is the identity. Canonical sum is always a flat BlendCase, never nested.
    public static VectorField operator +(VectorField left, VectorField right) =>
        new BlendCase(Fields: FlattenSum(left).Concat(FlattenSum(right)).ToSeq(), Mode: FieldBlend.Sum);
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
                hitToScaled: (hit, op) => c.Projection.Equals(SupportProjection.Span) || c.Projection.Equals(SupportProjection.SignedSpanAway)
                    ? from span in c.Projection.Project<VectorSpan>(space: c.Source, hit: hit, sample: state.Sample, context: state.Context, key: op)
                      select (Raw: span.Direction.Value, Scale: span.Magnitude.Value)
                    : from raw in c.Projection.Project<Vector3d>(space: c.Source, hit: hit, sample: state.Sample, context: state.Context, key: op)
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
        gradientCase: static (state, c) =>
            from grad in FieldNabla.GradientAt(field: c.Source, point: state.Sample, eps: c.Epsilon.Value, context: state.Context, key: state.Key)
            from accepted in state.Key.AcceptValue(value: grad)
            select accepted,
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
                : BiotSavartContribution(start: c.Start, end: c.End, current: c.Current, point: state.Sample, tol: state.Context.Absolute.Value, key: state.Key);
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
            select output,
        // Bridson 2007 curl noise: V = ∇×Ψ with three potentials decorrelated by large-prime offsets.
        curlNoiseCase: static (state, c) =>
            from g1 in FieldNabla.GradientAt(field: c.Potential, point: state.Sample, eps: c.Epsilon.Value, context: state.Context, key: state.Key)
            from g2 in FieldNabla.GradientAt(field: c.Potential, point: state.Sample + FieldNabla.CurlOffset2, eps: c.Epsilon.Value, context: state.Context, key: state.Key)
            from g3 in FieldNabla.GradientAt(field: c.Potential, point: state.Sample + FieldNabla.CurlOffset3, eps: c.Epsilon.Value, context: state.Context, key: state.Key)
            from raw in state.Key.AcceptValue(value: new Vector3d(x: g3.Y - g2.Z, y: g1.Z - g3.X, z: g2.X - g1.Y))
            select raw,
        crossFieldCase: static (state, c) => MeshKernel.CrossFieldAt(space: c.Space, symmetry: c.Symmetry, constraints: c.Constraints, cones: c.Cones, sample: state.Sample, key: state.Key),
        hodgeIrrotationalCase: static (state, c) => MeshKernel.HodgeProjectedAt(source: c.Source, space: c.Space, sense: BoundarySense.Toward, sample: state.Sample, key: state.Key),
        hodgeSolenoidalCase: static (state, c) => MeshKernel.HodgeProjectedAt(source: c.Source, space: c.Space, sense: BoundarySense.Away, sample: state.Sample, key: state.Key),
        vectorHeatCase: static (state, c) => MeshKernel.VectorHeatAt(space: c.Space, sources: c.Sources, time: c.Time.Value, sample: state.Sample, key: state.Key),
        geodesicTangentCase: static (state, c) => MeshKernel.GeodesicTangentAt(space: c.Space, sources: c.Sources, sample: state.Sample, key: state.Key));
    // Biot-Savart law for a finite straight current segment from `start` to `end` carrying
    // `current` amperes. Returns the magnetic field vector at `point`, perpendicular to both
    // the wire and the foot-of-perpendicular vector, scaled by the angular geometry.
    private static Fin<Vector3d> BiotSavartContribution(Point3d start, Point3d end, double current, Point3d point, double tol, Op key) {
        Vector3d wire = end - start;
        double wireLen = wire.Length;
        Vector3d t = wire / wireLen;
        Vector3d r1 = point - start;
        Vector3d r2 = point - end;
        Vector3d perpVec = FieldNabla.PerpendicularComponent(r: r1, axis: t);
        double R = perpVec.Length;
        if (R < tol || r1.Length < tol || r2.Length < tol) return Fin.Fail<Vector3d>(key.InvalidInput());
        double angularFactor = (r1 * t / r1.Length) - (r2 * t / r2.Length);
        double prefactor = current / (4.0 * Math.PI * R);
        return key.AcceptValue(value: Vector3d.CrossProduct(a: t, b: perpVec / R) * prefactor * angularFactor);
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
public partial record TensorField {
    public sealed record ConstantCase(SymmetricMatrix Value) : TensorField;
    public sealed record CurvatureCase(SurfaceSpace Space) : TensorField;
    public sealed record LiftCase(Func<Point3d, SymmetricMatrix> Sampler) : TensorField;
    public sealed record WarpCase(TensorField Source, Transform Spatial) : TensorField;
    public sealed record ScaledCase(TensorField Source, double Scale) : TensorField;
    public sealed record BlendCase(Seq<TensorField> Fields, FieldBlend Mode) : TensorField;
    public static TensorField Constant(SymmetricMatrix value) => new ConstantCase(Value: value);
    public static TensorField Curvature(SurfaceSpace space) => new CurvatureCase(Space: space);
    public static Fin<TensorField> Lift(Func<Point3d, SymmetricMatrix>? sampler, Op? key = null) =>
        Optional(sampler).ToFin(key.OrDefault().InvalidInput()).Map(active => (TensorField)new LiftCase(Sampler: active));
    public static TensorField Warp(TensorField source, Transform spatial) => new WarpCase(Source: source, Spatial: spatial);
    public static TensorField ScaleBy(TensorField source, double scale) => new ScaledCase(Source: source, Scale: scale);
    public static TensorField Blend(Seq<TensorField> fields, FieldBlend? mode = null) =>
        new BlendCase(Fields: fields, Mode: mode ?? FieldBlend.Sum);
    internal Fin<SymmetricMatrix> SampleTensor(Point3d sample, Context context, Op key) => Switch(
        state: (Sample: sample, Context: context, Key: key),
        constantCase: static (s, c) => s.Key.AcceptValue(value: c.Value),
        curvatureCase: static (s, c) => CurvatureTensorAt(space: c.Space, sample: s.Sample, key: s.Key),
        liftCase: static (s, c) => s.Key.Catch(() => s.Key.AcceptValue(value: c.Sampler(arg: s.Sample))),
        warpCase: static (s, c) => c.Spatial.TryGetInverse(inverseTransform: out Transform inverse)
            ? c.Source.SampleTensor(sample: inverse * s.Sample, context: s.Context, key: s.Key)
                .Bind(tensor => TransformTensor(tensor: tensor, spatial: c.Spatial, key: s.Key))
            : Fin.Fail<SymmetricMatrix>(error: s.Key.InvalidResult()),
        scaledCase: static (s, c) => c.Source.SampleTensor(sample: s.Sample, context: s.Context, key: s.Key)
            .Bind(m => s.Key.AcceptValue(value: new SymmetricMatrix(Dimension: m.Dimension, Upper: [.. m.Upper.AsIterable().Select(v => v * c.Scale)]))),
        blendCase: static (s, c) => c.Fields.TraverseM(f => f.SampleTensor(sample: s.Sample, context: s.Context, key: s.Key)).As()
            .Bind(tensors => BlendTensors(tensors: tensors, mode: c.Mode, key: s.Key)));
    private static Fin<SymmetricMatrix> BlendTensors(Seq<SymmetricMatrix> tensors, FieldBlend mode, Op key) =>
        tensors.IsEmpty || tensors.Exists(t => !t.IsValid || t.Dimension.Value != tensors[index: 0].Dimension.Value)
            ? Fin.Fail<SymmetricMatrix>(error: key.InvalidResult())
            : key.AcceptValue(value: AverageTensors(tensors: tensors, divisor: mode.Equals(FieldBlend.Sum) ? 1 : tensors.Count));
    internal Fin<Seq<(double Eigenvalue, Direction Eigenvector)>> PrincipalDirections(Point3d sample, Context context, Op key) =>
        from tensor in SampleTensor(sample: sample, context: context, key: key)
        from _ in tensor.Dimension.Value == 3 ? Fin.Succ(unit) : Fin.Fail<Unit>(key.InvalidInput())
        from eigen in tensor.DecomposeEigen(key: key)
        from directions in eigen.TraverseM(pair => Direction.Of(value: CloudKernel.AsVector3d(v: pair.Eigenvector), context: context, key: key).Map(d => (pair.Eigenvalue, Eigenvector: d))).As()
        select directions;
    // Shape operator A = κ₁ d₁ d₁ᵀ + κ₂ d₂ d₂ᵀ lifted to 3D (zero curvature along surface normal).
    private static Fin<SymmetricMatrix> CurvatureTensorAt(SurfaceSpace space, Point3d sample, Op key) {
        Surface surface = space.Native;
        return surface.ClosestPoint(testPoint: sample, u: out double u, v: out double v) switch {
            false => Fin.Fail<SymmetricMatrix>(error: key.InvalidResult()),
            true => surface.CurvatureAt(u: u, v: v) switch {
                SurfaceCurvature sc when sc.IsSet =>
                    SymmetricMatrix.Of(
                        dim: Dimension.Create(value: 3),
                        upper: new Arr<double>([
                            (sc.Kappa(direction: 0) * sc.Direction(direction: 0).X * sc.Direction(direction: 0).X) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).X * sc.Direction(direction: 1).X),
                            (sc.Kappa(direction: 0) * sc.Direction(direction: 0).X * sc.Direction(direction: 0).Y) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).X * sc.Direction(direction: 1).Y),
                            (sc.Kappa(direction: 0) * sc.Direction(direction: 0).X * sc.Direction(direction: 0).Z) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).X * sc.Direction(direction: 1).Z),
                            (sc.Kappa(direction: 0) * sc.Direction(direction: 0).Y * sc.Direction(direction: 0).Y) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).Y * sc.Direction(direction: 1).Y),
                            (sc.Kappa(direction: 0) * sc.Direction(direction: 0).Y * sc.Direction(direction: 0).Z) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).Y * sc.Direction(direction: 1).Z),
                            (sc.Kappa(direction: 0) * sc.Direction(direction: 0).Z * sc.Direction(direction: 0).Z) + (sc.Kappa(direction: 1) * sc.Direction(direction: 1).Z * sc.Direction(direction: 1).Z)]),
                        key: key),
                _ => Fin.Fail<SymmetricMatrix>(error: key.InvalidResult()),
            },
        };
    }
    private static Fin<SymmetricMatrix> TransformTensor(SymmetricMatrix tensor, Transform spatial, Op key) {
        if (tensor.Dimension.Value != 3) return Fin.Fail<SymmetricMatrix>(key.InvalidInput());
        double[] a = [
            tensor.At(i: 0, j: 0), tensor.At(i: 0, j: 1), tensor.At(i: 0, j: 2),
            tensor.At(i: 1, j: 0), tensor.At(i: 1, j: 1), tensor.At(i: 1, j: 2),
            tensor.At(i: 2, j: 0), tensor.At(i: 2, j: 1), tensor.At(i: 2, j: 2),
        ];
        double[] r = [spatial[0, 0], spatial[0, 1], spatial[0, 2], spatial[1, 0], spatial[1, 1], spatial[1, 2], spatial[2, 0], spatial[2, 1], spatial[2, 2]];
        double[] upper = new double[6];
        int cursor = 0;
        for (int i = 0; i < 3; i++)
            for (int j = i; j < 3; j++) {
                double value = 0.0;
                for (int p = 0; p < 3; p++)
                    for (int q = 0; q < 3; q++) value += r[(i * 3) + p] * a[(p * 3) + q] * r[(j * 3) + q];
                upper[cursor++] = value;
            }
        return SymmetricMatrix.Of(dim: Dimension.Create(value: 3), upper: new Arr<double>(upper), key: key);
    }
    private static SymmetricMatrix AverageTensors(Seq<SymmetricMatrix> tensors, int divisor) {
        SymmetricMatrix first = tensors[index: 0];
        double[] accum = [.. first.Upper.AsIterable()];
        foreach (SymmetricMatrix t in tensors.Skip(amount: 1).AsIterable())
            for (int i = 0; i < accum.Length; i++) accum[i] += t.Upper[index: i];
        if (divisor > 1) for (int i = 0; i < accum.Length; i++) accum[i] /= divisor;
        return new SymmetricMatrix(Dimension: first.Dimension, Upper: new Arr<double>(accum));
    }
}

[Union]
public partial record ScalarField {
    public sealed record ConstantCase(double Value) : ScalarField;
    public sealed record DistanceCase(SupportSpace Source, BoundarySense Sense) : ScalarField;
    public sealed record PotentialCase(Seq<(Point3d Position, double Charge)> Charges, Falloff Falloff) : ScalarField;
    public sealed record DensityCase(Point3d Center, PositiveMagnitude Spread, double Strength) : ScalarField;
    public sealed record BlendCase(Seq<ScalarField> Fields, FieldBlend Mode) : ScalarField;
    public sealed record MagnitudeCase(VectorField Source) : ScalarField;
    public sealed record DivergenceCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField;
    public sealed record LaplacianCase(ScalarField Source, PositiveMagnitude Epsilon) : ScalarField;
    public sealed record ScaledCase(ScalarField Source, double Scale) : ScalarField;
    public sealed record WorleyCase(Seq<Point3d> Seeds, int Order) : ScalarField;
    public sealed record MorseCase(Point3d Center, PositiveMagnitude Depth, PositiveMagnitude Width) : ScalarField;
    public sealed record MollifierCase(Point3d Center, PositiveMagnitude Radius) : ScalarField;
    public sealed record PowerCase(ScalarField Source, double Exponent) : ScalarField;
    public sealed record CsgCase(ScalarField Left, ScalarField Right, CsgKind Op, BlendKind Smoothing) : ScalarField;
    public sealed record PeriodicCase(ScalarField Source, Vector3d Period) : ScalarField;
    public sealed record StrainMagnitudeCase(VectorField Source, PositiveMagnitude Epsilon) : ScalarField;
    public sealed record ClampCase(ScalarField Source, double Minimum, double Maximum) : ScalarField;
    public sealed record PrimitiveCase(SdfKind Kind, ImmutableDictionary<string, double> Parameters, Plane Pose) : ScalarField;
    public sealed record NoiseCase(NoiseKind Kind, int Seed, int Octaves, double Persistence, double Lacunarity, double Frequency) : ScalarField;
    public sealed record OnionCase(ScalarField Source, PositiveMagnitude Thickness) : ScalarField;
    public sealed record SdfRoundCase(ScalarField Source, PositiveMagnitude Radius) : ScalarField;
    public sealed record ElongateCase(ScalarField Source, Vector3d Extent) : ScalarField;
    public sealed record DisplaceCase(ScalarField Source, ScalarField Displacement) : ScalarField;
    public sealed record TwistCase(ScalarField Source, double AnglePerUnit, Direction Axis) : ScalarField;
    public sealed record BendCase(ScalarField Source, double Curvature, Direction Axis) : ScalarField;
    public sealed record GeodesicCase(MeshSpace Space, Seq<int> Sources) : ScalarField;
    public sealed record MeanCurvatureFlowCase(MeshSpace Space, PositiveMagnitude TimeStep, Dimension Iterations) : ScalarField;
    public sealed record SpectralDistanceCase(MeshSpace Space, SpectralFilter Filter, Seq<int> Sources, Dimension Pairs) : ScalarField;
    public sealed record LogMapCase(MeshSpace Space, int Origin) : ScalarField;
    public sealed record StripeCase(MeshSpace Space, VectorField CrossField, PositiveMagnitude Frequency) : ScalarField;
    public sealed record SignedDistanceFromMeshCase(MeshSpace Space, SdfMeshMethod Method) : ScalarField;
    public static ScalarField Constant(double value) => new ConstantCase(Value: value);
    public static ScalarField Distance(SupportSpace source, BoundarySense? sense = null) =>
        new DistanceCase(Source: source, Sense: sense ?? BoundarySense.Toward);
    public static ScalarField Potential(Seq<(Point3d Position, double Charge)> charges, Falloff? falloff = null) =>
        new PotentialCase(Charges: charges, Falloff: falloff ?? Falloff.Inverse);
    public static Fin<ScalarField> Density(Point3d center, double spread, double strength, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: spread)
            .Map(s => (ScalarField)new DensityCase(Center: center, Spread: s, Strength: strength));
    }
    public static ScalarField Blend(Seq<ScalarField> fields, FieldBlend? blend = null) =>
        new BlendCase(Fields: fields, Mode: blend ?? FieldBlend.Sum);
    public static ScalarField Magnitude(VectorField source) => new MagnitudeCase(Source: source);
    public static Fin<ScalarField> Divergence(VectorField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<VectorField, ScalarField>(source, epsilon,
            static (s, e) => new DivergenceCase(Source: s, Epsilon: e), key);
    public static Fin<ScalarField> Laplacian(ScalarField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<ScalarField, ScalarField>(source, epsilon,
            static (s, e) => new LaplacianCase(Source: s, Epsilon: e), key);
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
    public static ScalarField Union(ScalarField left, ScalarField right, BlendKind? blend = null) =>
        new CsgCase(Left: left, Right: right, Op: CsgKind.Union, Smoothing: blend ?? BlendKind.Hard);
    public static ScalarField Intersect(ScalarField left, ScalarField right, BlendKind? blend = null) =>
        new CsgCase(Left: left, Right: right, Op: CsgKind.Intersect, Smoothing: blend ?? BlendKind.Hard);
    public static ScalarField Difference(ScalarField left, ScalarField right, BlendKind? blend = null) =>
        new CsgCase(Left: left, Right: right, Op: CsgKind.Difference, Smoothing: blend ?? BlendKind.Hard);
    public static Fin<ScalarField> Primitive(SdfKind kind, ImmutableDictionary<string, double> parameters, Plane pose, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(kind).ToFin(op.InvalidInput())
               from validParams in Optional(parameters).ToFin(op.InvalidInput())
               from _ in guard(active.ValidateParameters(parameters: validParams), op.InvalidInput())
               from validPose in op.AcceptValue(value: pose)
               select (ScalarField)new PrimitiveCase(Kind: active, Parameters: validParams, Pose: validPose);
    }
    public static Fin<ScalarField> Noise(NoiseKind kind, int seed, int octaves, double persistence, double lacunarity, double frequency, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(kind).ToFin(op.InvalidInput())
               from _ in guard(octaves is >= 1 and <= 32 && RhinoMath.IsValidDouble(x: frequency) && frequency > 0.0
                              && RhinoMath.IsValidDouble(x: persistence) && persistence is > 0.0 and <= 1.0
                              && RhinoMath.IsValidDouble(x: lacunarity) && lacunarity > 1.0, op.InvalidInput())
               select (ScalarField)new NoiseCase(Kind: active, Seed: seed, Octaves: octaves, Persistence: persistence, Lacunarity: lacunarity, Frequency: frequency);
    }
    public static Fin<ScalarField> Onion(ScalarField source, double thickness, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: thickness)
            .Map(t => (ScalarField)new OnionCase(Source: source, Thickness: t));
    }
    public static Fin<ScalarField> SdfRound(ScalarField source, double radius, Op? key = null) {
        Op op = key.OrDefault();
        return op.AcceptValidated<PositiveMagnitude>(candidate: radius)
            .Map(r => (ScalarField)new SdfRoundCase(Source: source, Radius: r));
    }
    public static Fin<ScalarField> Elongate(ScalarField source, Vector3d extent, Op? key = null) {
        Op op = key.OrDefault();
        return extent.IsValid && extent.X >= 0.0 && extent.Y >= 0.0 && extent.Z >= 0.0
            ? Fin.Succ((ScalarField)new ElongateCase(Source: source, Extent: extent))
            : Fin.Fail<ScalarField>(op.InvalidInput());
    }
    public static ScalarField Displace(ScalarField source, ScalarField displacement) =>
        new DisplaceCase(Source: source, Displacement: displacement);
    public static Fin<ScalarField> Twist(ScalarField source, double anglePerUnit, Direction axis, Op? key = null) {
        Op op = key.OrDefault();
        return RhinoMath.IsValidDouble(x: anglePerUnit)
            ? Fin.Succ((ScalarField)new TwistCase(Source: source, AnglePerUnit: anglePerUnit, Axis: axis))
            : Fin.Fail<ScalarField>(op.InvalidInput());
    }
    public static Fin<ScalarField> Bend(ScalarField source, double curvature, Direction axis, Op? key = null) {
        Op op = key.OrDefault();
        return RhinoMath.IsValidDouble(x: curvature)
            ? Fin.Succ((ScalarField)new BendCase(Source: source, Curvature: curvature, Axis: axis))
            : Fin.Fail<ScalarField>(op.InvalidInput());
    }
    public static Fin<ScalarField> Geodesic(MeshSpace space, Seq<int> sources, Op? key = null) {
        Op op = key.OrDefault();
        return sources.IsEmpty || sources.Exists(i => i < 0 || i >= space.Native.Vertices.Count)
            ? Fin.Fail<ScalarField>(op.InvalidInput())
            : Fin.Succ((ScalarField)new GeodesicCase(Space: space, Sources: sources));
    }
    public static Fin<ScalarField> MeanCurvatureFlow(MeshSpace space, double timeStep, int iterations, Op? key = null) {
        Op op = key.OrDefault();
        return from t in op.AcceptValidated<PositiveMagnitude>(candidate: timeStep)
               from count in op.AcceptValidated<Dimension>(candidate: iterations)
               select (ScalarField)new MeanCurvatureFlowCase(Space: space, TimeStep: t, Iterations: count);
    }
    // Reuter-Wolter-Peinecke 2006 + Coifman et al. 2005: biharmonic / diffusion / commute-time
    // distance on a mesh through a single SpectralFilter; sources non-empty selects pairwise
    // distance, sources empty selects per-vertex signature.
    public static Fin<ScalarField> SpectralDistance(MeshSpace space, SpectralFilter filter, Seq<int> sources, int pairs, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(filter).ToFin(op.InvalidInput())
               from count in op.AcceptValidated<Dimension>(candidate: pairs)
               from _ in guard(sources.IsEmpty || sources.ForAll(i => i >= 0 && i < space.Native.Vertices.Count), op.InvalidInput())
               select (ScalarField)new SpectralDistanceCase(Space: space, Filter: active, Sources: sources, Pairs: count);
    }
    // Log map at a single origin reduces to scalar geodesic distance from {origin}; consumers
    // wanting tangent direction use VectorField.GeodesicTangent on the same source set.
    public static Fin<ScalarField> LogMap(MeshSpace space, int origin, Op? key = null) {
        Op op = key.OrDefault();
        return origin < 0 || origin >= space.Native.Vertices.Count
            ? Fin.Fail<ScalarField>(op.InvalidInput())
            : Fin.Succ((ScalarField)new LogMapCase(Space: space, Origin: origin));
    }
    // Knoeppel-Crane-Pinkall-Schroeder 2015 stripe patterns: scalar function whose level sets
    // align with the supplied cross-field at the requested spatial frequency.
    public static Fin<ScalarField> Stripe(MeshSpace space, VectorField crossField, double frequency, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(crossField).ToFin(op.InvalidInput())
               from freq in op.AcceptValidated<PositiveMagnitude>(candidate: frequency)
               select (ScalarField)new StripeCase(Space: space, CrossField: active, Frequency: freq);
    }
    // Robust SDF from a (possibly non-watertight) mesh: GeneralizedWindingNumber (Jacobson 2013)
    // for non-manifold input or SignedHeat (Feng-Crane SIGGRAPH 2024) for smoothness.
    public static Fin<ScalarField> SignedDistanceFromMesh(MeshSpace space, SdfMeshMethod method, Op? key = null) {
        Op op = key.OrDefault();
        return Optional(method).ToFin(op.InvalidInput())
            .Map(active => (ScalarField)new SignedDistanceFromMeshCase(Space: space, Method: active));
    }
    public static Fin<ScalarField> Periodic(ScalarField source, Vector3d period, Op? key = null) {
        Op op = key.OrDefault();
        return !period.IsValid || Math.Abs(value: period.X) <= RhinoMath.ZeroTolerance || Math.Abs(value: period.Y) <= RhinoMath.ZeroTolerance || Math.Abs(value: period.Z) <= RhinoMath.ZeroTolerance
            ? Fin.Fail<ScalarField>(op.InvalidInput())
            : Fin.Succ((ScalarField)new PeriodicCase(Source: source, Period: period));
    }
    public static Fin<ScalarField> StrainMagnitude(VectorField source, double epsilon, Op? key = null) =>
        FieldNabla.WithSourceEpsilon<VectorField, ScalarField>(source, epsilon,
            static (s, e) => new StrainMagnitudeCase(Source: s, Epsilon: e), key);
    public static Fin<ScalarField> Clamp(ScalarField source, double minimum, double maximum, Op? key = null) {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from _ in guard(RhinoMath.IsValidDouble(x: minimum) && RhinoMath.IsValidDouble(x: maximum) && minimum <= maximum, op.InvalidInput())
               select (ScalarField)new ClampCase(Source: active, Minimum: minimum, Maximum: maximum);
    }
    public static ScalarField Zero { get; } = Constant(value: 0.0);
    // SDF Lipschitz bound for sphere-tracing consumers. PrimitiveCase carries the SdfKind
    // tabulated bound; CsgCase folds the children through BlendKind.Erode; unary SDF
    // modifiers preserve their source's bound; non-SDF cases return None.
    public Option<double> LipschitzBound() => this switch {
        PrimitiveCase p => Some(p.Kind.Lipschitz),
        CsgCase c => from l in c.Left.LipschitzBound() from r in c.Right.LipschitzBound() select c.Smoothing.Erode(leftLip: l, rightLip: r),
        OnionCase o => o.Source.LipschitzBound(),
        SdfRoundCase r => r.Source.LipschitzBound(),
        ElongateCase e => e.Source.LipschitzBound(),
        _ => Option<double>.None,
    };
    private static Seq<ScalarField> FlattenSum(ScalarField field) =>
        field is BlendCase b && b.Mode.Equals(FieldBlend.Sum) ? b.Fields : Seq(field);
    public static ScalarField operator +(ScalarField left, ScalarField right) =>
        new BlendCase(Fields: FlattenSum(left).Concat(FlattenSum(right)).ToSeq(), Mode: FieldBlend.Sum);
    public static ScalarField operator -(ScalarField left, ScalarField right) => left + (-right);
    public static ScalarField operator -(ScalarField field) => new ScaledCase(Source: field, Scale: -1.0);
    public static ScalarField operator *(ScalarField field, double scale) => new ScaledCase(Source: field, Scale: scale);
    public static ScalarField operator *(double scale, ScalarField field) => new ScaledCase(Source: field, Scale: scale);
    // Rhino-native marching cubes over the scalar field at the zero level set. Delegates the
    // scalar evaluator to SampleScalar; failed samples fall back to 0.0 because the native
    // root-finder cannot tolerate exceptions inside Func<Point3d, double>.
    public Fin<Mesh> IsoSurface(BoundingBox bounds, int resolution, int maxRootSteps, Context context, Op? key = null) {
        Op op = key.OrDefault();
        ScalarField self = this;
        return !bounds.IsValid || resolution < 2 || maxRootSteps < 1
            ? Fin.Fail<Mesh>(op.InvalidInput())
            : op.Catch(() => {
                Mesh result = Mesh.CreateFromIsosurface(
                    scalarFieldEvaluator: p => self.SampleScalar(sample: p, context: context, key: op).IfFail(0.0),
                    box: bounds, resolution: resolution, RootFindingMaxSteps: maxRootSteps);
                return result is null || !result.IsValid
                    ? Fin.Fail<Mesh>(op.InvalidResult())
                    : Fin.Succ(result);
            });
    }
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
            c.Strength * Math.Exp(d: -(state.Sample - c.Center).SquareLength / (2.0 * c.Spread.Value * c.Spread.Value))),
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
            from output in state.Key.AcceptValue(value: c.Op.Combine(left: leftValue, right: rightValue, blend: c.Smoothing))
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
            .Bind(v => state.Key.AcceptValue(value: Math.Clamp(value: v, min: c.Minimum, max: c.Maximum))),
        primitiveCase: static (state, c) => c.Kind.SignedDistance(worldPoint: state.Sample, parameters: c.Parameters, pose: c.Pose, key: state.Key),
        // Multi-octave fBm: octave k contributes persistence^k * sample(freq * lacunarity^k); norm is the closed-form geometric series of amplitudes.
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
        onionCase: static (state, c) => c.Source.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(v => state.Key.AcceptValue(value: Math.Abs(value: v) - c.Thickness.Value)),
        sdfRoundCase: static (state, c) => c.Source.SampleScalar(sample: state.Sample, context: state.Context, key: state.Key)
            .Bind(v => state.Key.AcceptValue(value: v - c.Radius.Value)),
        elongateCase: static (state, c) => {
            // Elongate by clamping the local offset to the extent box; sample at the residual.
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
        logMapCase: static (state, c) => MeshKernel.HeatGeodesicAt(space: c.Space, sources: Seq(c.Origin), sample: state.Sample, key: state.Key),
        stripeCase: static (state, c) => MeshKernel.StripeAt(space: c.Space, crossField: c.CrossField, frequency: c.Frequency.Value, sample: state.Sample, key: state.Key),
        signedDistanceFromMeshCase: static (state, c) => MeshKernel.SignedDistanceFromMeshAt(space: c.Space, method: c.Method, sample: state.Sample, key: state.Key));
}

internal static class FieldNabla {
    // Large-prime offsets that decorrelate the three Bridson curl-noise scalar potentials.
    internal static readonly Vector3d CurlOffset2 = new(x: 31.4159, y: 27.1828, z: 41.4213);
    internal static readonly Vector3d CurlOffset3 = new(x: -19.3274, y: 53.2186, z: -67.9531);
    internal static Fin<TResult> WithSourceEpsilon<TSource, TResult>(TSource? source, double epsilon, Func<TSource, PositiveMagnitude, TResult> make, Op? key)
        where TSource : class {
        Op op = key.OrDefault();
        return from active in Optional(source).ToFin(op.InvalidInput())
               from eps in op.AcceptValidated<PositiveMagnitude>(candidate: epsilon)
               select make(active, eps);
    }
    internal static Vector3d PerpendicularComponent(Vector3d r, Vector3d axis) => r - (r * axis * axis);
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
    internal static Fin<Vector3d> GradientAt(ScalarField field, Point3d point, double eps, Context context, Op key) =>
        from samples in SampleAxes(sampler: p => field.SampleScalar(sample: p, context: context, key: key), point: point, eps: eps)
        let inv2eps = 1.0 / (2.0 * eps)
        select new Vector3d(
            x: (samples.X1 - samples.X0) * inv2eps,
            y: (samples.Y1 - samples.Y0) * inv2eps,
            z: (samples.Z1 - samples.Z0) * inv2eps);
}

// Procedural noise primitives. Perlin uses the canonical Ken Perlin 1985 algorithm with the
// standard 256-entry permutation (Improved Noise 2002 retains the same table). Worley
// evaluates against a deterministic per-cell hash-generated seed pattern across a 3x3x3
// neighbourhood, returning distance to the nearest seed.
internal static class FieldNoise {
    private static readonly int[] PermTable = [
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225,
        140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148,
        247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32,
        57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175,
        74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122,
        60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
        65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169,
        200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64,
        52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212,
        207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213,
        119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9,
        129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104,
        218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241,
        81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157,
        184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93,
        222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
    ];
    private static int Perm(int x, int seed) => PermTable[(x + seed) & 0xFF];
    private static double Fade(double t) => t * t * t * ((t * ((t * 6) - 15)) + 10);
    private static double Lerp(double t, double a, double b) => a + (t * (b - a));
    // Standard Ken Perlin 3D gradient: project (x,y,z) onto one of 12 edge directions selected by the low 4 bits of the permuted hash.
    private static double Grad(int hash, double x, double y, double z) {
        int h = hash & 15;
        double u = h < 8 ? x : y;
        double v = h < 4 ? y : h is 12 or 14 ? x : z;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
    internal static double PerlinAt(Point3d point, int seed, double frequency) {
        double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
        int X = (int)Math.Floor(d: px) & 0xFF; int Y = (int)Math.Floor(d: py) & 0xFF; int Z = (int)Math.Floor(d: pz) & 0xFF;
        double x = px - Math.Floor(d: px); double y = py - Math.Floor(d: py); double z = pz - Math.Floor(d: pz);
        double u = Fade(t: x); double v = Fade(t: y); double w = Fade(t: z);
        int A = Perm(x: X, seed: seed) + Y; int AA = Perm(x: A, seed: seed) + Z; int AB = Perm(x: A + 1, seed: seed) + Z;
        int B = Perm(x: X + 1, seed: seed) + Y; int BA = Perm(x: B, seed: seed) + Z; int BB = Perm(x: B + 1, seed: seed) + Z;
        return Lerp(t: w,
            a: Lerp(t: v,
                a: Lerp(t: u, a: Grad(hash: Perm(x: AA, seed: seed), x: x, y: y, z: z), b: Grad(hash: Perm(x: BA, seed: seed), x: x - 1, y: y, z: z)),
                b: Lerp(t: u, a: Grad(hash: Perm(x: AB, seed: seed), x: x, y: y - 1, z: z), b: Grad(hash: Perm(x: BB, seed: seed), x: x - 1, y: y - 1, z: z))),
            b: Lerp(t: v,
                a: Lerp(t: u, a: Grad(hash: Perm(x: AA + 1, seed: seed), x: x, y: y, z: z - 1), b: Grad(hash: Perm(x: BA + 1, seed: seed), x: x - 1, y: y, z: z - 1)),
                b: Lerp(t: u, a: Grad(hash: Perm(x: AB + 1, seed: seed), x: x, y: y - 1, z: z - 1), b: Grad(hash: Perm(x: BB + 1, seed: seed), x: x - 1, y: y - 1, z: z - 1))));
    }
    // Worley/cellular noise. Iterate the 3x3x3 cell neighbourhood, generate one seed point
    // per cell deterministically via the permutation table, return distance to nearest seed.
    internal static double WorleyAt(Point3d point, int seed, double frequency) {
        double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
        int cx = (int)Math.Floor(d: px); int cy = (int)Math.Floor(d: py); int cz = (int)Math.Floor(d: pz);
        double minDistSq = double.MaxValue;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
                for (int dz = -1; dz <= 1; dz++) {
                    int nx = cx + dx; int ny = cy + dy; int nz = cz + dz;
                    int hashX = Perm(x: Perm(x: Perm(x: nx & 0xFF, seed: seed) + (ny & 0xFF), seed: seed) + (nz & 0xFF), seed: seed);
                    int hashY = Perm(x: hashX + 17, seed: seed);
                    int hashZ = Perm(x: hashY + 31, seed: seed);
                    double sx = nx + (hashX / 255.0); double sy = ny + (hashY / 255.0); double sz = nz + (hashZ / 255.0);
                    double ddx = sx - px; double ddy = sy - py; double ddz = sz - pz;
                    double distSq = (ddx * ddx) + (ddy * ddy) + (ddz * ddz);
                    if (distSq < minDistSq) minDistSq = distSq;
                }
        return Math.Sqrt(d: minDistSq);
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
        (int i1, int j1, int k1, int i2, int j2, int k2) =
            x0 >= y0
                ? y0 >= z0 ? (1, 0, 0, 1, 1, 0) : x0 >= z0 ? (1, 0, 0, 1, 0, 1) : (0, 0, 1, 1, 0, 1)
                : y0 < z0 ? (0, 0, 1, 0, 1, 1) : x0 < z0 ? (0, 1, 0, 0, 1, 1) : (0, 1, 0, 1, 1, 0);
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

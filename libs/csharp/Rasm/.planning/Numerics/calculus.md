# [RASM_NUMERICS_CALCULUS]

The sample-anywhere analytic math floor of `Rasm.Vectors` — differential operators, weight-profile mathematics, and procedural noise lattices, each generic over a SAMPLER so no field union, mesh, or cloud type appears on this floor. The page owns `Nabla`, the six-axis central-difference stencil promoted from the mature validation grab-bag into a pure differential-calculus owner — `GradientAt`, `CurlAt`, `CurlNoiseAt`, `DivergenceAt`, `LaplacianAt`, `StrainMagnitudeAt` over `Func<Point3d, Fin<double>>` / `Func<Point3d, Fin<Vector3d>>` samplers, with the curl-noise decorrelation offsets and toroidal domain wrap as named policy values; the weight-profile family — `KernelProfileStatus`, `KernelProfile` (value + first + second derivative + smoothness status), `KernelKind` the six compactly-supported kernels (Wendland C², quintic, cosine, cubic, linear, Epanechnikov) whose rows carry the profile AND its two derivatives as delegate columns over one `SupportProfile` fold, `WeightKernelFamily` the five reconstruction-weight profiles (smooth-poly, Wendland C², Gaussian, compact-exponential, singular-interpolating) with the `Interpolating` capability column, and `Falloff` the radial-decay `[Union]` (constant, inverse, inverse-square, Gaussian, compact kernel, metric-anisotropic) whose anisotropic case takes a `SymmetricMatrix` METRIC SAMPLER — SPD-gated by leading principal minors per sample — so the tensor-field coupling stays downstream, every case carrying its `SlopeBound` slope-evidence column; and `FieldNoise`, the procedural lattices — Perlin gradient noise over the canonical 256-entry permutation table, skewed and smoothed simplex, and Worley cellular noise — seed-parameterized and deterministic.

`Spatial/fields` composes these owners as its case mathematics (falloff-weighted influence fields, noise cases, kernel-windowed densities); `Meshing/reconstruct` composes `WeightKernelFamily` as its MLS/APSS/Levin weighting; the finite-difference operators serve any implicit field a sampler can express. The admission-guard half of the mature `FieldNabla` (NotNull/AllFinite/Plane/Cone/input gates) is `Rasm.Domain`'s validation vocabulary — this page carries ONLY the mathematics.

## [01]-[INDEX]

- [02]-[DIFFERENTIAL_STENCIL]: `Nabla` — the six-axis central-difference stencil + gradient/curl/curl-noise/divergence/Laplacian/strain-magnitude over samplers + decorrelation offsets + toroidal wrap.
- [03]-[WEIGHT_PROFILES]: `KernelProfileStatus` · `KernelProfile` · `KernelKind` (6 compact-support rows with derivative columns) · `WeightKernelFamily` (5 reconstruction rows) · `Falloff` (6-case radial-decay union with the metric-sampler anisotropic case).
- [04]-[NOISE_LATTICES]: `FieldNoise` — Perlin/simplex/skewed-simplex/Worley lattices over the canonical permutation table.

## [02]-[DIFFERENTIAL_STENCIL]

- Owner: `Nabla` the `static` differential-calculus owner. `SampleAxes(sampler, point, eps, key)` evaluates the six axis-offset samples `f(p ± ε·eᵢ)` through ONE traversal that any operator below composes; `GradientAt` central-differences a scalar sampler into `∇f`; `CurlAt` assembles `∇×F` from the six vector samples; `CurlNoiseAt` builds a divergence-free field from three decorrelated gradient evaluations (Bridson curl-noise — the offsets `CurlOffset2`/`CurlOffset3` are the named decorrelation policy values); `DivergenceAt` reads `∇·F`; `LaplacianAt` runs the seven-point stencil `(Σf(p ± ε·eᵢ) − 6f(p))/ε²`; `StrainMagnitudeAt` forms the symmetric rate-of-strain tensor from the six vector samples and returns its Frobenius norm; `ToroidalWrap(sample, period)` the periodic domain fold.
- Cases: six differential operators + the stencil + the wrap (8 members, one owner).
- Entry: every operator takes `(sampler, point, eps, key)` — `eps` is the caller's scale-derived stencil width (a field consumer derives it from its `Context` tolerance or feature scale; this floor never guesses a scale), gated finite and above `EpsilonPolicy.ZeroTolerance`.
- Auto: all six operators share the ONE `SampleAxes` traversal, so a sampler is invoked exactly six times per first-order operator (seven for the Laplacian's center tap) and failure at any tap short-circuits the rail with the sampler's own typed fault.
- Receipt: none — the operators are pure projections; evidence belongs to the field or solver that composes them.
- Packages: LanguageExt.Core (`Fin`, query expressions), Rasm.Domain (`Op`), RhinoCommon (`Point3d`/`Vector3d` value structs).
- Growth: a new differential operator (Hessian, Jacobian determinant, vector Laplacian) is one member over the same `SampleAxes` stencil; a higher-order stencil (fourth-order central differences) is one alternative stencil member the operators re-bind to — never a per-field re-implementation.
- Boundary: the mature corpus hard-wired these operators to its field unions — the sampler signature inverts that dependency so `Spatial/fields` (downstream) plugs `field.SampleScalar` in as the sampler and this floor stays field-agnostic; the mesh-aware Laplacians (cotangent, intrinsic Delaunay) are `Meshing/mesh`'s operators over connectivity, NOT this stencil — this page differentiates ambient ℝ³ samplers only; `ToroidalWrap` assumes an admitted strictly-positive period — the `Period` guard in the Domain validation vocabulary is its upstream gate, so the wrap itself stays a total pure fold.

```csharp
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
namespace Rasm.Vectors;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Nabla {
    // Bridson curl-noise decorrelation offsets: three gradient taps of one potential, offset far
    // apart so their cross-assembly is divergence-free without visible lattice correlation.
    internal static readonly Vector3d CurlOffset2 = new(x: 31.4159, y: 27.1828, z: 41.4213), CurlOffset3 = new(x: -19.3274, y: 53.2186, z: -67.9531);
    public static Fin<(T X1, T X0, T Y1, T Y0, T Z1, T Z0)> SampleAxes<T>(Func<Point3d, Fin<T>> sampler, Point3d point, double eps, Op key) =>
        from _ in guard(double.IsFinite(eps) && eps > EpsilonPolicy.ZeroTolerance, key.InvalidInput()).ToFin()
        from xp in sampler(arg: point + (eps * Vector3d.XAxis))
        from xm in sampler(arg: point - (eps * Vector3d.XAxis))
        from yp in sampler(arg: point + (eps * Vector3d.YAxis))
        from ym in sampler(arg: point - (eps * Vector3d.YAxis))
        from zp in sampler(arg: point + (eps * Vector3d.ZAxis))
        from zm in sampler(arg: point - (eps * Vector3d.ZAxis))
        select (X1: xp, X0: xm, Y1: yp, Y0: ym, Z1: zp, Z0: zm);
    public static Fin<Vector3d> GradientAt(Func<Point3d, Fin<double>> sampler, Point3d point, double eps, Op key) =>
        from samples in SampleAxes(sampler: sampler, point: point, eps: eps, key: key)
        let inv2eps = 1.0 / (2.0 * eps)
        select new Vector3d(x: (samples.X1 - samples.X0) * inv2eps, y: (samples.Y1 - samples.Y0) * inv2eps, z: (samples.Z1 - samples.Z0) * inv2eps);
    public static Fin<Vector3d> CurlAt(Func<Point3d, Fin<Vector3d>> sampler, Point3d point, double eps, Op key) =>
        from samples in SampleAxes(sampler: sampler, point: point, eps: eps, key: key)
        let inv2eps = 1.0 / (2.0 * eps)
        from curl in key.AcceptValue(value: new Vector3d(
            x: (samples.Y1.Z - samples.Y0.Z - (samples.Z1.Y - samples.Z0.Y)) * inv2eps,
            y: (samples.Z1.X - samples.Z0.X - (samples.X1.Z - samples.X0.Z)) * inv2eps,
            z: (samples.X1.Y - samples.X0.Y - (samples.Y1.X - samples.Y0.X)) * inv2eps))
        select curl;
    public static Fin<Vector3d> CurlNoiseAt(Func<Point3d, Fin<double>> sampler, Point3d point, double eps, Op key) =>
        from g1 in GradientAt(sampler: sampler, point: point, eps: eps, key: key)
        from g2 in GradientAt(sampler: sampler, point: point + CurlOffset2, eps: eps, key: key)
        from g3 in GradientAt(sampler: sampler, point: point + CurlOffset3, eps: eps, key: key)
        from raw in key.AcceptValue(value: new Vector3d(x: g3.Y - g2.Z, y: g1.Z - g3.X, z: g2.X - g1.Y))
        select raw;
    public static Fin<double> DivergenceAt(Func<Point3d, Fin<Vector3d>> sampler, Point3d point, double eps, Op key) =>
        from samples in SampleAxes(sampler: sampler, point: point, eps: eps, key: key)
        let inv2eps = 1.0 / (2.0 * eps)
        from value in key.AcceptValue(value: (samples.X1.X - samples.X0.X + samples.Y1.Y - samples.Y0.Y + samples.Z1.Z - samples.Z0.Z) * inv2eps)
        select value;
    public static Fin<double> LaplacianAt(Func<Point3d, Fin<double>> sampler, Point3d point, double eps, Op key) =>
        from samples in SampleAxes(sampler: sampler, point: point, eps: eps, key: key)
        from center in sampler(arg: point)
        let invEpsSq = 1.0 / (eps * eps)
        from value in key.AcceptValue(value: (samples.X1 + samples.X0 + samples.Y1 + samples.Y0 + samples.Z1 + samples.Z0 - (6.0 * center)) * invEpsSq)
        select value;
    public static Fin<double> StrainMagnitudeAt(Func<Point3d, Fin<Vector3d>> sampler, Point3d point, double eps, Op key) =>
        from samples in SampleAxes(sampler: sampler, point: point, eps: eps, key: key)
        let inv2eps = 1.0 / (2.0 * eps)
        let sxx = (samples.X1.X - samples.X0.X) * inv2eps
        let syy = (samples.Y1.Y - samples.Y0.Y) * inv2eps
        let szz = (samples.Z1.Z - samples.Z0.Z) * inv2eps
        let sxy = 0.5 * (samples.Y1.X - samples.Y0.X + samples.X1.Y - samples.X0.Y) * inv2eps
        let sxz = 0.5 * (samples.Z1.X - samples.Z0.X + samples.X1.Z - samples.X0.Z) * inv2eps
        let syz = 0.5 * (samples.Z1.Y - samples.Z0.Y + samples.Y1.Z - samples.Y0.Z) * inv2eps
        from value in key.AcceptValue(value: Math.Sqrt(d: (sxx * sxx) + (syy * syy) + (szz * szz) + (2.0 * ((sxy * sxy) + (sxz * sxz) + (syz * syz)))))
        select value;
    public static Point3d ToroidalWrap(Point3d sample, Vector3d period) =>
        new(x: sample.X - (Math.Floor(d: (sample.X / period.X) + 0.5) * period.X),
            y: sample.Y - (Math.Floor(d: (sample.Y / period.Y) + 0.5) * period.Y),
            z: sample.Z - (Math.Floor(d: (sample.Z / period.Z) + 0.5) * period.Z));
}
```

## [03]-[WEIGHT_PROFILES]

- Owner: `KernelProfileStatus` the smoothness verdict vocabulary (`Smooth`/`SupportBoundary`/`NonsmoothOrigin`/`OutsideSupport`); `KernelProfile` the evaluation carrier — value, first derivative, second derivative, status — so a consumer differentiating a kernel-windowed quantity reads the derivative off the profile instead of re-differencing; `KernelKind` the `[SmartEnum<int>]` of six compactly-supported kernels whose rows carry `value(q)`, `value′(q)/r`, and `value″(q)/r²` as three delegate columns folded through ONE `SupportProfile` (support clamp at `q = d/r ≥ 1`, boundary band, nonsmooth-origin flag for the odd-power kernels) — Wendland C² `(1−q)⁴(1+4q)`, quintic `(1−q)⁵`, cosine `½(1+cos πq)`, cubic `(1−q)³`, linear `1−q`, Epanechnikov `1−q²` — each row also carrying `DerivativeSupremum` (`sup_{q∈[0,1]}|value′(q)|`, the dimensionless slope-bound numerator); `WeightKernelFamily` the `[SmartEnum<int>]` of five reconstruction-weight profiles — smooth-poly `(1−t²)²`, Wendland C², Gaussian `e^(−t²/σ²)` with `GaussianBandwidthSquared = 1/9`, compact-exponential, and the SINGULAR interpolating weight `1/max(t², ε)` — with the `Interpolating` capability column the MLS routes dispatch on; `Falloff` the `[Union]` radial-decay family — `Constant`, `Inverse` (`1/d`, degeneracy-gated), `InverseSquare`, `Gaussian(spread)`, `Kernel(kind, radius)` (compact support through `KernelKind`), and `Metric(kind, metric, radius)` — the anisotropic case whose `metric: Func<Point3d, Fin<SymmetricMatrix>>` SAMPLER yields the local 3×3 metric tensor, SPD-gated by the Sylvester leading-principal-minors fold, so the Mahalanobis distance `√(offsetᵀ·M·offset)` drives the kernel profile without this floor naming any field type. Every `Falloff` case carries the `SlopeBound` column — `sup|dw/dd|` over its admitted domain (`0` constant; `e^{−1/2}/σ` Gaussian at `d = σ`; `DerivativeSupremum/r` kernel; `None` for the inverse laws and the metric case, whose slopes are tolerance- and sampler-dependent) — the slope evidence the `Spatial/fields` `LipschitzBound` fold absorbs.
- Cases: `KernelKind` 6 · `WeightKernelFamily` 5 · `Falloff` 6 · `KernelProfileStatus` 4.
- Entry: `KernelKind.Profile(distance, radius, key)` — gated through `Admit.KernelInput`, returns the full `KernelProfile`; `KernelKind.Weight(distance, radius)` the bare-value fast path; `WeightKernelFamily.Weight(distance, support)` zero outside support; `Falloff.Weight(...)` three overloads discriminating on input shape — bare distance, offset vector, offset + sample point (the metric case's requirement) — all folding through one `WeightCore` gated by `Admit.FalloffInput`.
- Auto: `SupportProfile` is the one clamp/status fold every kernel row shares — its bands live on the dimensionless `q = d/r` so support classification is invariant under model scale, outside-support and boundary yield exact zeros with their status, and the nonsmooth-origin flag fires only for kernels whose derivative jumps at `d = 0`; the metric falloff admits the sampled tensor as SPD by the three leading principal minors (Sylvester) before forming the quadratic — allocation-free per sample where a factorization would churn the hot loop — so an indefinite metric fails typed instead of producing `√negative`.
- Receipt: `KernelProfile` IS the per-evaluation receipt (value + derivatives + status).
- Packages: Thinktecture.Runtime.Extensions (`[UseDelegateFromConstructor]` columns), LanguageExt.Core, `matrix.md` (`SymmetricMatrix` — the metric carrier), Rasm.Domain (`Op`, the `Admit.KernelInput`/`FalloffInput` gates, the one-type-argument `AcceptValidated<TVO>` bridge).
- Growth: a new kernel is one `KernelKind` row (three delegate columns + its `DerivativeSupremum`); a new reconstruction weight is one `WeightKernelFamily` row; a new decay law is one `Falloff` case + one `WeightCore` arm + its `SlopeBound` column.
- Boundary: `Spatial/fields` wraps `Falloff.Metric` over its `TensorField` by passing `tensorField.Sampler(context)` — the tensor-field type NEVER appears here, keeping this page upstream of the field algebra; `Meshing/reconstruct` composes `KernelKind` for its RBF/MLS windows and `WeightKernelFamily` rows for its Levin/APSS weights — one profile mathematics, zero copies.

```csharp
// --- [TYPES] ------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class KernelProfileStatus {
    public static readonly KernelProfileStatus Smooth = new(key: 0);
    public static readonly KernelProfileStatus SupportBoundary = new(key: 1);
    public static readonly KernelProfileStatus NonsmoothOrigin = new(key: 2);
    public static readonly KernelProfileStatus OutsideSupport = new(key: 3);
}

[StructLayout(LayoutKind.Auto)]
public readonly record struct KernelProfile(double Value, double FirstDerivative, double SecondDerivative, KernelProfileStatus Status) {
    public bool IsValid => double.IsFinite(Value) && double.IsFinite(FirstDerivative) && double.IsFinite(SecondDerivative);
}

[SmartEnum<int>]
public sealed partial class KernelKind {
    public static readonly KernelKind Wendland = new(key: 0, derivativeSupremum: 135.0 / 64.0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => Pow1(q: q, power: 4) * (1.0 + (4.0 * q)), first: static (q, r) => ((-20.0 * q) + (60.0 * q * q) - (60.0 * q * q * q) + (20.0 * q * q * q * q)) / r, second: static (q, r) => (-20.0 + (120.0 * q) - (180.0 * q * q) + (80.0 * q * q * q)) / (r * r)));
    public static readonly KernelKind Quintic = new(key: 1, derivativeSupremum: 5.0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => Pow1(q: q, power: 5), first: static (q, r) => -5.0 * Pow1(q: q, power: 4) / r, second: static (q, r) => 20.0 * Pow1(q: q, power: 3) / (r * r)));
    public static readonly KernelKind Cosine = new(key: 2, derivativeSupremum: Math.PI / 2.0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => 0.5 * (1.0 + Math.Cos(d: Math.PI * q)), first: static (q, r) => -0.5 * Math.PI * Math.Sin(a: Math.PI * q) / r, second: static (q, r) => -0.5 * Math.PI * Math.PI * Math.Cos(d: Math.PI * q) / (r * r)));
    public static readonly KernelKind Cubic = new(key: 3, derivativeSupremum: 3.0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => Pow1(q: q, power: 3), first: static (q, r) => -3.0 * Pow1(q: q, power: 2) / r, second: static (q, r) => 6.0 * (1.0 - q) / (r * r)));
    public static readonly KernelKind Linear = new(key: 4, derivativeSupremum: 1.0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => 1.0 - q, first: static (_, r) => -1.0 / r, second: static (_, _) => 0.0));
    public static readonly KernelKind Epanechnikov = new(key: 5, derivativeSupremum: 2.0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => 1.0 - (q * q), first: static (q, r) => -2.0 * q / r, second: static (_, r) => -2.0 / (r * r)));
    // sup over q ∈ [0,1] of |value′(q)| — the dimensionless slope-bound numerator Falloff.SlopeBound divides
    // by r: Wendland peaks at q = 1/4 (135/64), the odd-power kernels at the origin, cosine at q = 1/2.
    public double DerivativeSupremum { get; }
    [UseDelegateFromConstructor] private partial KernelProfile Evaluate(double distance, double radius);
    public Fin<KernelProfile> Profile(double distance, double radius, Op key) =>
        from _ in Admit.KernelInput(distance: distance, radius: radius, key: key)
        from profile in Evaluate(distance: distance, radius: radius) switch {
            KernelProfile p when p.IsValid => Fin.Succ(p),
            _ => Fin.Fail<KernelProfile>(key.InvalidResult()),
        }
        select profile;
    public double Weight(double distance, double radius) => Evaluate(distance: distance, radius: radius).Value;
    private static double Pow1(double q, int power) => Math.Pow(x: 1.0 - q, y: power);
    // The one support clamp/status fold every kernel row shares. Bands live on the normalized
    // coordinate q = d/r, so they are dimensionless — invariant under model scale.
    private static KernelProfile SupportProfile(double distance, double radius, bool nonsmoothAtOrigin, Func<double, double, double> value, Func<double, double, double> first, Func<double, double, double> second) {
        double q = distance / radius;
        return q > 1.0
            ? new KernelProfile(Value: 0.0, FirstDerivative: 0.0, SecondDerivative: 0.0, Status: KernelProfileStatus.OutsideSupport)
            : Math.Abs(value: q - 1.0) <= EpsilonPolicy.SqrtEpsilon
                ? new KernelProfile(Value: 0.0, FirstDerivative: 0.0, SecondDerivative: 0.0, Status: KernelProfileStatus.SupportBoundary)
                : new KernelProfile(Value: value(arg1: q, arg2: radius), FirstDerivative: first(arg1: q, arg2: radius), SecondDerivative: second(arg1: q, arg2: radius), Status: nonsmoothAtOrigin && q <= EpsilonPolicy.SqrtEpsilon ? KernelProfileStatus.NonsmoothOrigin : KernelProfileStatus.Smooth);
    }
}

[SmartEnum<int>]
public sealed partial class WeightKernelFamily {
    public static readonly WeightKernelFamily SmoothPoly = new(key: 0, interpolating: false, profile: static t => (1.0 - (t * t)) * (1.0 - (t * t)));
    public static readonly WeightKernelFamily WendlandC2 = new(key: 1, interpolating: false, profile: static t => Math.Pow(x: 1.0 - t, y: 4) * (1.0 + (4.0 * t)));
    public static readonly WeightKernelFamily Gaussian = new(key: 2, interpolating: false, profile: static t => Math.Exp(d: -(t * t) / GaussianBandwidthSquared));
    public static readonly WeightKernelFamily CompactExp = new(key: 3, interpolating: false, profile: static t => t >= 1.0 ? 0.0 : Math.Exp(d: -(t * t) / Math.Max(val1: 1.0 - (t * t), val2: EpsilonPolicy.ZeroTolerance)));
    public static readonly WeightKernelFamily Singular = new(key: 4, interpolating: true, profile: static t => 1.0 / Math.Max(val1: t * t, val2: EpsilonPolicy.SqrtEpsilon));
    private const double GaussianBandwidthSquared = 1.0 / 9.0;
    public bool Interpolating { get; }
    [UseDelegateFromConstructor] private partial double Profile(double t);
    public double Weight(double distance, double support) =>
        distance >= support ? 0.0 : Profile(t: Math.Min(val1: distance / support, val2: 1.0));
}

[Union]
public abstract partial record Falloff {
    private Falloff() { }
    public sealed record ConstantCase : Falloff { internal ConstantCase() { } public override Option<double> SlopeBound => Some(0.0); }
    public sealed record InverseCase : Falloff { internal InverseCase() { } public override Option<double> SlopeBound => None; }
    public sealed record InverseSquareCase : Falloff { internal InverseSquareCase() { } public override Option<double> SlopeBound => None; }
    public sealed record GaussianCase : Falloff { internal GaussianCase(PositiveMagnitude Spread) => this.Spread = Spread; public PositiveMagnitude Spread { get; } public override Option<double> SlopeBound => Some(Math.Exp(-0.5) / Spread.Value); }
    public sealed record KernelCase : Falloff { internal KernelCase(KernelKind Kind, PositiveMagnitude Radius) { this.Kind = Kind; this.Radius = Radius; } public KernelKind Kind { get; } public PositiveMagnitude Radius { get; } public override Option<double> SlopeBound => Some(Kind.DerivativeSupremum / Radius.Value); }
    // Anisotropic decay through a sampled SPD metric: the sampler yields the local 3x3 tensor, so the
    // field algebra (downstream) plugs its TensorField in without this floor naming it.
    public sealed record MetricCase : Falloff { internal MetricCase(KernelKind Kind, Func<Point3d, Fin<SymmetricMatrix>> Metric, PositiveMagnitude Radius) { this.Kind = Kind; this.Metric = Metric; this.Radius = Radius; } public KernelKind Kind { get; } public Func<Point3d, Fin<SymmetricMatrix>> Metric { get; } public PositiveMagnitude Radius { get; } public override Option<double> SlopeBound => None; }

    // sup|dw/dd| over the admitted domain — the slope evidence downstream Lipschitz folds read: 0 constant,
    // e^{−1/2}/σ Gaussian (|w′| peaks at d = σ), DerivativeSupremum/r kernel. None where no tolerance-free
    // bound exists: the inverse laws steepen toward the degeneracy gate; the sampled metric's spectral
    // radius is unbounded a priori.
    public abstract Option<double> SlopeBound { get; }
    public static Falloff Constant => new ConstantCase();
    public static Falloff Inverse => new InverseCase();
    public static Falloff InverseSquare => new InverseSquareCase();
    public static Fin<Falloff> Gaussian(double spread, Op? key = null) =>
        key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: spread).Map(static value => (Falloff)new GaussianCase(Spread: value));
    public static Fin<Falloff> Kernel(KernelKind kind, double radius, Op? key = null) =>
        from active in Optional(kind).ToFin(key.OrDefault().InvalidInput())
        from r in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius)
        select (Falloff)new KernelCase(Kind: active, Radius: r);
    public static Fin<Falloff> Metric(KernelKind kind, Func<Point3d, Fin<SymmetricMatrix>> metric, double radius, Op? key = null) =>
        from active in Optional(kind).ToFin(key.OrDefault().InvalidInput())
        from sampler in Optional(metric).ToFin(key.OrDefault().InvalidInput())
        from r in key.OrDefault().AcceptValidated<PositiveMagnitude>(candidate: radius)
        select (Falloff)new MetricCase(Kind: active, Metric: sampler, Radius: r);
    public Fin<double> Weight(double distance, double tolerance, Op key) =>
        WeightCore(distance: distance, distanceSquared: distance * distance, offset: Option<(Vector3d Offset, Point3d Sample)>.None, tolerance: tolerance, key: key);
    public Fin<double> Weight(Vector3d offset, double tolerance, Op key) =>
        WeightCore(distance: offset.Length, distanceSquared: offset.SquareLength, offset: Option<(Vector3d Offset, Point3d Sample)>.None, tolerance: tolerance, key: key);
    public Fin<double> Weight(Vector3d offset, Point3d sample, double tolerance, Op key) =>
        WeightCore(distance: offset.Length, distanceSquared: offset.SquareLength, offset: Some((Offset: offset, Sample: sample)), tolerance: tolerance, key: key);
    private Fin<double> WeightCore(double distance, double distanceSquared, Option<(Vector3d Offset, Point3d Sample)> offset, double tolerance, Op key) =>
        Admit.FalloffInput(distance: distance, distanceSquared: distanceSquared, tolerance: tolerance, key: key).Bind(_ => Switch(
            state: (Distance: distance, DistanceSquared: distanceSquared, Offset: offset, Tolerance: tolerance, Key: key),
            constantCase: static (_, _) => Fin.Succ(1.0),
            inverseCase: static (s, _) => s.Distance > s.Tolerance ? Fin.Succ(1.0 / s.Distance) : Fin.Fail<double>(s.Key.InvalidInput()),
            inverseSquareCase: static (s, _) => s.Distance > s.Tolerance ? Fin.Succ(1.0 / s.DistanceSquared) : Fin.Fail<double>(s.Key.InvalidInput()),
            gaussianCase: static (s, g) => Fin.Succ(Math.Exp(-s.DistanceSquared / (2.0 * g.Spread.Value * g.Spread.Value))),
            kernelCase: static (s, k) => k.Kind.Profile(distance: s.Distance, radius: k.Radius.Value, key: s.Key).Map(static p => p.Value),
            metricCase: static (s, k) =>
                from m in s.Offset.ToFin(s.Key.Unsupported(geometryType: typeof(MetricCase), outputType: typeof(double)))
                from tensor in k.Metric(arg: m.Sample)
                from _ in guard(tensor.Dimension.Value == 3 && SpdByMinors(tensor: tensor), s.Key.InvalidInput())
                // Zero offset (query at the source) is legal: quadratic 0 -> distance 0 -> kernel maximum;
                // the -ZeroTolerance band absorbs rounding of tiny offsets under an SPD-proven tensor.
                from metricDistance in (m.Offset.X, m.Offset.Y, m.Offset.Z) switch {
                    (double x, double y, double z) when
                        (x * ((tensor.At(i: 0, j: 0) * x) + (tensor.At(i: 0, j: 1) * y) + (tensor.At(i: 0, j: 2) * z))) +
                        (y * ((tensor.At(i: 1, j: 0) * x) + (tensor.At(i: 1, j: 1) * y) + (tensor.At(i: 1, j: 2) * z))) +
                        (z * ((tensor.At(i: 2, j: 0) * x) + (tensor.At(i: 2, j: 1) * y) + (tensor.At(i: 2, j: 2) * z))) is double quadratic
                        && double.IsFinite(quadratic) && quadratic > -EpsilonPolicy.ZeroTolerance => s.Key.AcceptValue(value: Math.Sqrt(d: Math.Max(val1: 0.0, val2: quadratic))),
                    _ => Fin.Fail<double>(s.Key.InvalidResult()),
                }
                from profile in k.Kind.Profile(distance: metricDistance, radius: k.Radius.Value, key: s.Key)
                select profile.Value));
    // Sylvester SPD gate — three leading principal minors of the symmetric 3x3, allocation-free;
    // a per-sample factorization is the deleted form in this hot path.
    private static bool SpdByMinors(SymmetricMatrix tensor) {
        double a = tensor.At(i: 0, j: 0), b = tensor.At(i: 0, j: 1), c = tensor.At(i: 0, j: 2);
        double d = tensor.At(i: 1, j: 1), e = tensor.At(i: 1, j: 2), f = tensor.At(i: 2, j: 2);
        double det2 = (a * d) - (b * b);
        return a > 0.0 && det2 > 0.0 && (det2 * f) - (a * e * e) + (2.0 * b * c * e) - (d * c * c) > 0.0;
    }
}
```

## [04]-[NOISE_LATTICES]

- Owner: `FieldNoise` the `internal static` procedural-noise owner — `PerlinAt(point, seed, frequency)` classic gradient noise over the canonical Ken-Perlin 256-entry permutation table (`Fade` quintic smoothing, `Grad` hashed gradient selection, trilinear `Lerp` lattice blend); `SimplexAt` the 3D simplex lattice (corner ranking by fractional ordering, `SimplexCorner` radial-falloff gradient contributions, ×32 amplitude normalization); `SkewedSimplexAt(point, seed, frequency, smooth)` the skew-transformed variant with the optional two-tap rotation smoothing; `WorleyAt` cellular noise (per-cell hashed feature points over the 27-cell neighborhood, nearest-feature Euclidean distance).
- Cases: 3 internal lattice entries (`PerlinAt` · `WorleyAt` · `SkewedSimplexAt` with the smooth flag) spanning the four published lattice modes over one shared hash substrate (`Perm`, `HashCell`); `SimplexAt` is the private skew-domain kernel both simplex modes ride.
- Entry: every lattice takes `(Point3d point, int seed, double frequency)` — deterministic for a given triple, so noise-driven fields replay across processes; input admission (octaves, persistence, lacunarity, frequency bounds for fractal sums) is the CONSUMER's policy admitted through `Admit.NoiseInput` — the lattice itself is total over finite input.
- Auto: `Perm(x, seed)` folds the seed into the table lookup so seeds relabel the lattice without a table copy; Worley hashes three decorrelated channels (offsets 17/31) for the per-cell feature point.
- Receipt: none — pure deterministic functions.
- Packages: BCL only (`Math.Floor`, integer bit ops); RhinoCommon `Point3d` as the coordinate carrier.
- Growth: a new lattice (open-simplex, curl-of-Worley) is one member over the same `Perm`/`HashCell` substrate; fractal octave sums (fBm, turbulence) are the CONSUMER's fold over these single-octave taps — `Spatial/fields` owns the octave policy row.
- Boundary: `PermTable` is the canonical published Perlin permutation — hardcoded-by-specification, the one sanctioned literal table on this page; the noise VOCABULARY (`NoiseKind` rows with caution flags and sampler columns) is `Spatial/fields`' — this page owns only the lattice mathematics those rows point at.

```csharp
// --- [OPERATIONS] -------------------------------------------------------------------------
internal static class FieldNoise {
    // Canonical Ken-Perlin permutation — hardcoded by specification, seed-folded at lookup.
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
        return Lerp(t: w,
            a: Lerp(t: v,
                a: Lerp(t: u, a: Grad(hash: Perm(x: AA, seed: seed), x: x, y: y, z: z), b: Grad(hash: Perm(x: BA, seed: seed), x: x - 1, y: y, z: z)),
                b: Lerp(t: u, a: Grad(hash: Perm(x: AB, seed: seed), x: x, y: y - 1, z: z), b: Grad(hash: Perm(x: BB, seed: seed), x: x - 1, y: y - 1, z: z))),
            b: Lerp(t: v,
                a: Lerp(t: u, a: Grad(hash: Perm(x: AA + 1, seed: seed), x: x, y: y, z: z - 1), b: Grad(hash: Perm(x: BA + 1, seed: seed), x: x - 1, y: y, z: z - 1)),
                b: Lerp(t: u, a: Grad(hash: Perm(x: AB + 1, seed: seed), x: x, y: y - 1, z: z - 1), b: Grad(hash: Perm(x: BB + 1, seed: seed), x: x - 1, y: y - 1, z: z - 1))));
    }
    internal static double WorleyAt(Point3d point, int seed, double frequency) {
        double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
        int cx = (int)Math.Floor(d: px); int cy = (int)Math.Floor(d: py); int cz = (int)Math.Floor(d: pz);
        return Math.Sqrt(d: (from dx in Enumerable.Range(start: -1, count: 3)
                             from dy in Enumerable.Range(start: -1, count: 3)
                             from dz in Enumerable.Range(start: -1, count: 3)
                             let nx = cx + dx
                             let ny = cy + dy
                             let nz = cz + dz
                             let hashX = Perm(x: Perm(x: Perm(x: nx & 0xFF, seed: seed) + (ny & 0xFF), seed: seed) + (nz & 0xFF), seed: seed)
                             let hashY = Perm(x: hashX + 17, seed: seed)
                             let hashZ = Perm(x: hashY + 31, seed: seed)
                             let ddx = nx + (hashX / 255.0) - px
                             let ddy = ny + (hashY / 255.0) - py
                             let ddz = nz + (hashZ / 255.0) - pz
                             select (ddx * ddx) + (ddy * ddy) + (ddz * ddz)).Min());
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
```

## [05]-[DENSITY_BAR]

| [INDEX] | [AXIS/CONCERN]         | [OWNER]                                          | [KIND]                                                       | [CASES] |
| :-----: | :--------------------- | :------------------------------------------------ | :------------------------------------------------------------ | :-----: |
|  [01]   | Differential operators | `Nabla`                                          | sampler-generic central-difference stencil (one `SampleAxes`) |    8    |
|  [02]   | Kernel profiles        | `KernelKind` + `KernelProfile`/`KernelProfileStatus` | `[SmartEnum<int>]` rows with value + two derivative columns + `DerivativeSupremum` |    6    |
|  [03]   | Reconstruction weights | `WeightKernelFamily`                             | `[SmartEnum<int>]` with `Interpolating` capability column     |    5    |
|  [04]   | Radial decay           | `Falloff`                                        | `[Union]` incl. the metric-sampler anisotropic case; `SlopeBound` column per case |    6    |
|  [05]   | Noise lattices         | `FieldNoise`                                     | deterministic Perlin/simplex/Worley over the canonical table  |    4    |

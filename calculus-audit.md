# `calculus.md` Surgical Refinement Audit

Target: `libs/dotnet/Rasm/.planning/Numerics/calculus.md`

This audit counts nonblank authored C# lines in the affected fence fragments. Required consumer edits outside the target are named but excluded from the target LOC total. The queue is ordered so owner-shape collapses land before local algorithm and naming reductions.

Evidence basis: the full target; `CLAUDE.md`; the cross-libs and .NET planning laws; the full `docs/stacks/csharp/` doctrine, with particular attention to language, shapes, generated dispatch, result flow, algorithms, system APIs, validation, and compute; both checked-in `.api` tiers, especially LanguageExt, Thinktecture, tensors, NodaTime, DoubleDouble, and RhinoCommon; every `libs/dotnet/` consumer of the affected symbols; and the root audit form at commit `f17b2d8521806b567232dd8c28167e4cbe294da4`.

Accepted total for target fences: **-55 LOC, -3 authored type symbols, -52 authored member symbols**. Generated keyed lookup/conversion disappears from both `WeightKernel` and `KernelStatus`, while the three unearned generated owners `KernelSupport`, `SimplexBlend`, and `SolarSeries` disappear whole.

## 1. Collapse the two-row support owner into the compact-support fact it mirrors

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:192-197`, anchor `[SmartEnum<int>] public sealed partial class KernelSupport`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:209-249`, anchors `support: KernelSupport.Compact`, `support: KernelSupport.Global`, and `public KernelSupport Support`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:261-269`, anchor `private KernelProfile Profiled`

### From

```csharp
[SmartEnum<int>]
public sealed partial class KernelSupport {
    public static readonly KernelSupport Compact = new(key: 0, ceiling: Some(1.0));
    public static readonly KernelSupport Global = new(key: 1, ceiling: Option<double>.None);
    public Option<double> Ceiling { get; }
}
```

```csharp
support: KernelSupport.Compact
support: KernelSupport.Global
public KernelSupport Support { get; }
```

```csharp
private KernelProfile Profiled(double q, double radius) =>
    Support.Ceiling.Filter(edge => q > edge).IsSome
        ? new KernelProfile(Value: 0.0, FirstDerivative: 0.0, SecondDerivative: 0.0, Status: KernelProfileStatus.OutsideSupport)
        : Support.Ceiling.Filter(edge => Math.Abs(value: q - edge) <= EpsilonPolicy.SqrtEpsilon).IsSome
            ? new KernelProfile(Value: 0.0, FirstDerivative: 0.0, SecondDerivative: 0.0, Status: KernelProfileStatus.SupportBoundary)
            : Shape(q: q, radius: radius) switch {
                (double value, double first, double second) => new KernelProfile(Value: value, FirstDerivative: first, SecondDerivative: second,
                    Status: q <= EpsilonPolicy.SqrtEpsilon ? Origin : KernelProfileStatus.Smooth),
            };
```

### To

```csharp
// KernelSupport deleted.
```

```csharp
isCompact: true
isCompact: false
private bool IsCompact { get; }
```

```csharp
private KernelProfile Profiled(double q, double radius) =>
    IsCompact && q > 1.0
        ? new(0.0, 0.0, 0.0, KernelProfileStatus.OutsideSupport)
        : IsCompact && Math.Abs(q - 1.0) <= EpsilonPolicy.SqrtEpsilon
            ? new(0.0, 0.0, 0.0, KernelProfileStatus.SupportBoundary)
            : Shape(q, radius) switch { var (value, first, second) => new KernelProfile(value, first, second,
                q <= EpsilonPolicy.SqrtEpsilon ? Origin : KernelProfileStatus.Smooth) };
```

### Effect

- Target fenced LOC: **-8**.
- Authored surface: **-1 public type**, **-4 public members** (`KernelSupport.Compact`, `KernelSupport.Global`, `KernelSupport.Ceiling`, `KernelKind.Support`) and **+1 private member** (`KernelKind.IsCompact`), for **-3 authored members net**.
- Generated surface: the complete `KernelSupport` smart-enum surface disappears.
- Logic: normalization already defines every compact kernel on `q = distance / radius` with support boundary `q = 1`; the row therefore needs one fact, not a semantic wrapper around `Some(1.0)` versus `None` and not the same edge literal repeated on every compact row.

### API and consumer proof

No target or external consumer reads `KernelSupport`, either row, `Ceiling`, or `KernelKind.Support`; only `KernelKind.Profiled` asks whether the normalized profile is compact, so the replacement fact is private. A differently scaled finite support still normalizes its physical radius into `q`, so the cutoff remains `1.0`; a genuinely partial-support law would be a different profile formula, not a third support-regime object.

### Ripples

- Same file: rewrite the owner/cases/growth prose to say each `KernelKind` row privately carries whether its normalized profile has compact support; remove the claimed `KernelSupport` growth axis.
- Outside target: none.

## 2. Localize curl-noise decorrelation policy inside its only operation

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:37-39`, anchors `CurlDecorrelation2`, `CurlDecorrelation3`, and `CurlOffset`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:61-66`, anchor `CurlNoiseAt`

### From

```csharp
internal const double CurlDecorrelation2 = 137.0, CurlDecorrelation3 = -311.0;
internal static Vector3d CurlOffset(double eps, double scale) =>
    new(x: scale * eps, y: scale * eps * 1.3, z: scale * eps * 0.7);
```

```csharp
public static Fin<Vector3d> CurlNoiseAt(Func<Point3d, Fin<double>> sampler, Point3d point, double eps, Op key) =>
    from g1 in GradientAt(sampler: sampler, point: point, eps: eps, key: key)
    from g2 in GradientAt(sampler: sampler, point: point + CurlOffset(eps: eps, scale: CurlDecorrelation2), eps: eps, key: key)
    from g3 in GradientAt(sampler: sampler, point: point + CurlOffset(eps: eps, scale: CurlDecorrelation3), eps: eps, key: key)
    from raw in key.AcceptValue(value: new Vector3d(x: g3.Y - g2.Z, y: g1.Z - g3.X, z: g2.X - g1.Y))
    select raw;
```

### To

```csharp
public static Fin<Vector3d> CurlNoiseAt(Func<Point3d, Fin<double>> sampler, Point3d point, double eps, Op key) =>
    from g1 in GradientAt(sampler, point, eps, key)
    let offset = new Vector3d(eps, 1.3 * eps, 0.7 * eps)
    from g2 in GradientAt(sampler, point + (offset * 137.0), eps, key)
    from g3 in GradientAt(sampler, point - (offset * 311.0), eps, key)
    from raw in key.AcceptValue(new Vector3d(g3.Y - g2.Z, g1.Z - g3.X, g2.X - g1.Y))
    select raw;
```

### Effect

- Target fenced LOC: **-2**.
- Authored surface: **-3 internal module members**; the two literals and their anisotropic direction live at the only operation that gives them meaning.
- Logic: one base offset is evaluated once, scalar multiplication expresses the two decorrelated probes, and the single-use forwarding factory disappears.

### API and consumer proof

No target or external consumer reads either constant or calls `CurlOffset`; both calls are inside `CurlNoiseAt`. RhinoCommon supplies vector-scalar multiplication and point-vector addition/subtraction, so the replacement composes the carrier algebra directly.

### Ripples

- Same file: none beyond the exact replacement.
- Outside target: none.

## 3. Remove unearned vocabulary keys and seat row-only facts on their rows

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:184-190`, anchor `KernelProfileStatus`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:201-203,209-268`, `KernelProfile`/`KernelKind` references to `KernelProfileStatus` and `KernelProfile.IsValid`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:233-238,271-272`, Gaussian and inverse-multiquadric bound rows plus their one-use private fields
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:284-299`, anchor `WeightKernelFamily`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:355-356`, the `KernelCase` slope projection

### From

```csharp
[SmartEnum<int>]
public sealed partial class KernelProfileStatus {
    public static readonly KernelProfileStatus Smooth = new(key: 0);
    public static readonly KernelProfileStatus SupportBoundary = new(key: 1);
    public static readonly KernelProfileStatus NonsmoothOrigin = new(key: 2);
    public static readonly KernelProfileStatus OutsideSupport = new(key: 3);
}
```

```csharp
public readonly record struct KernelProfile(double Value, double FirstDerivative, double SecondDerivative, KernelProfileStatus Status) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(value: Value), ValidityClaim.Finite(value: FirstDerivative), ValidityClaim.Finite(value: SecondDerivative), Status is not null);
}
```

```csharp
public static readonly KernelKind Gaussian = new(key: 8, isCompact: false, origin: KernelProfileStatus.Smooth, derivativeSupremum: GaussianSupremum, polynomialOrder: 0,
    shape: static (q, r) => (Math.Exp(d: -(q * q)), -2.0 * q * Math.Exp(d: -(q * q)) / r, ((4.0 * q * q) - 2.0) * Math.Exp(d: -(q * q)) / (r * r)));
public static readonly KernelKind InverseMultiquadric = new(key: 10, isCompact: false, origin: KernelProfileStatus.Smooth, derivativeSupremum: InverseMultiquadricSupremum, polynomialOrder: 0,
    shape: static (q, r) => (1.0 / Math.Sqrt(d: 1.0 + (q * q)), -q / (Math.Pow(x: 1.0 + (q * q), y: 1.5) * r), ((2.0 * q * q) - 1.0) / (Math.Pow(x: 1.0 + (q * q), y: 2.5) * r * r)));
private static readonly double GaussianSupremum = Math.Sqrt(d: 2.0) * Math.Exp(d: -0.5);
private static readonly double InverseMultiquadricSupremum = Math.Pow(x: 1.5, y: -1.5) / Math.Sqrt(d: 2.0);
```

```csharp
[SmartEnum<int>]
public sealed partial class WeightKernelFamily {
    public static readonly WeightKernelFamily SmoothPoly = new(key: 0, interpolating: false, profile: static t => (1.0 - (t * t)) * (1.0 - (t * t)));
    public static readonly WeightKernelFamily WendlandC2 = new(key: 1, interpolating: false, profile: static t => Math.Pow(x: 1.0 - t, y: 4) * (1.0 + (4.0 * t)));
    public static readonly WeightKernelFamily Gaussian = new(key: 2, interpolating: false, profile: static t => Math.Exp(d: -(t * t) / GaussianBandwidthSquared));
    public static readonly WeightKernelFamily CompactExp = new(key: 3, interpolating: false, profile: static t => t >= 1.0 ? 0.0 : Math.Exp(d: -(t * t) / Math.Max(val1: 1.0 - (t * t), val2: EpsilonPolicy.ZeroTolerance)));
    public static readonly WeightKernelFamily Singular = new(key: 4, interpolating: true, profile: static t => 1.0 / Math.Max(val1: t * t, val2: EpsilonPolicy.SqrtEpsilon));
    public static readonly WeightKernelFamily Lanczos = new(key: 5, interpolating: true, profile: static t => (double)(ddouble.Sinc((ddouble)(2.0 * t), normalized: true) * ddouble.Sinc((ddouble)t, normalized: true)));
    private const double GaussianBandwidthSquared = 1.0 / 9.0;
    public bool Interpolating { get; }
```

```csharp
kernelCase: static (s, k) => k.Kind.Profile(distance: s.Distance, radius: k.Radius.Value, key: s.Key)
    .Map(static profile => profile.Status.Equals(KernelProfileStatus.OutsideSupport) ? 0.0 : Math.Abs(value: profile.FirstDerivative)),
```

### To

```csharp
[SmartEnum]
public sealed partial class KernelStatus {
    public static readonly KernelStatus Smooth = new();
    public static readonly KernelStatus SupportBoundary = new();
    public static readonly KernelStatus NonsmoothOrigin = new();
    public static readonly KernelStatus OutsideSupport = new();
}
```

```csharp
public readonly record struct KernelProfile(double Value, double FirstDerivative, double SecondDerivative, KernelStatus Status) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(Value), ValidityClaim.Finite(FirstDerivative), ValidityClaim.Finite(SecondDerivative), Status is not null);
}
```

```csharp
public static readonly KernelKind Gaussian = new(key: 8, isCompact: false, origin: KernelStatus.Smooth, derivativeSupremum: Math.Sqrt(2.0) * Math.Exp(-0.5), polynomialOrder: 0,
    shape: static (q, r) => (Math.Exp(-(q * q)), -2.0 * q * Math.Exp(-(q * q)) / r, ((4.0 * q * q) - 2.0) * Math.Exp(-(q * q)) / (r * r)));
public static readonly KernelKind InverseMultiquadric = new(key: 10, isCompact: false, origin: KernelStatus.Smooth, derivativeSupremum: Math.Pow(1.5, -1.5) / Math.Sqrt(2.0), polynomialOrder: 0,
    shape: static (q, r) => (1.0 / Math.Sqrt(1.0 + (q * q)), -q / (Math.Pow(1.0 + (q * q), 1.5) * r), ((2.0 * q * q) - 1.0) / (Math.Pow(1.0 + (q * q), 2.5) * r * r)));
```

```csharp
[SmartEnum]
public sealed partial class WeightKernel {
    public static readonly WeightKernel SmoothPoly = new(profile: static t => (1.0 - (t * t)) * (1.0 - (t * t)));
    public static readonly WeightKernel WendlandC2 = new(profile: static t => Math.Pow(1.0 - t, 4) * (1.0 + (4.0 * t)));
    public static readonly WeightKernel Gaussian = new(profile: static t => Math.Exp(-9.0 * t * t));
    public static readonly WeightKernel CompactExp = new(profile: static t => Math.Exp(-(t * t) / Math.Max(1.0 - (t * t), EpsilonPolicy.ZeroTolerance)));
    public static readonly WeightKernel Singular = new(profile: static t => 1.0 / Math.Max(t * t, EpsilonPolicy.SqrtEpsilon));
    public static readonly WeightKernel Lanczos = new(profile: static t => (double)(ddouble.Sinc((ddouble)(2.0 * t), normalized: true) * ddouble.Sinc((ddouble)t, normalized: true)));
```

```csharp
kernelCase: static (s, k) => k.Kind.Profile(s.Distance, k.Radius.Value, s.Key)
    .Map(static profile => Math.Abs(profile.FirstDerivative)),
```

### Effect

- Target fenced LOC: **-4**.
- Authored surface: **-4 members** (`Interpolating`, `GaussianBandwidthSquared`, `GaussianSupremum`, and `InverseMultiquadricSupremum`).
- Generated surface: both `WeightKernel` and `KernelStatus` retain process-local roster identity and generated total dispatch but lose unearned key lookup/conversion/parsing. A language enum is not admissible here: `KernelStatus` is an owned bounded vocabulary, not a foreign wire enum, ABI bit layout, or measured-kernel ordinal.
- Logic: both public `WeightKernel` evaluation entries, `Weight` and `Weights`, enforce the `t >= 1` cutoff before invoking the private row delegate, so `CompactExp` loses an unreachable duplicate cutoff. `KernelKind.Profiled` already writes a zero first derivative outside support, so `Falloff.Slope` projects its magnitude directly instead of re-reading the status to reconstruct the same zero. Three one-use bound/bandwidth constants become their row expressions, eliminating module indirection without duplicating a formula.
- Naming: `WeightKernel` names the reconstruction-weight vocabulary without the redundant `Family` suffix, and `KernelStatus` names the profile verdict without repeating the enclosing `KernelProfile` noun.

### API and consumer proof

Thinktecture keyless `[SmartEnum]` keeps the constructor delegate and roster identity `WeightKernel` actually needs. No `libs/dotnet/` consumer reads its key or `Interpolating`; MLS/APSS consume only the selected row's `Weight`. `KernelStatus` is a four-value process-local vocabulary: it has no earned key, but `docs/stacks/csharp/shapes.md#OWNER_CHOOSER` reserves language enums for foreign/ABI/kernel-ordinal boundaries, so keyless `[SmartEnum]` is the proper smaller owner and the validity fold remains the null rejection. `WeightKernel.Profile` is private and is reached only after `Weight` or `Weights` applies the support cutoff. Separately, `KernelKind.Profiled` constructs `OutsideSupport` with `(Value, FirstDerivative, SecondDerivative) = (0, 0, 0)`, so the slope projection needs no status branch. This deliberately does **not** make `KernelKind` keyless: `Rasm.Compute/.planning/Tensor/blas.md:673` persists `fit.Kernel.Key` as solve evidence.

### Ripples

- Same file: remove the claim that MLS dispatches on `Interpolating`; it does not. Rename every `KernelProfileStatus` use to `KernelStatus` and every `WeightKernelFamily` use to `WeightKernel`.
- `libs/dotnet/Rasm/.planning/Meshing/reconstruct.md`: rename the policy/member/parameter type from `WeightKernelFamily` to `WeightKernel`; the existing `WeightKernel` value/member name remains unchanged.
- `libs/dotnet/Rasm.Materials/.planning/Raster/filter.md` and `Raster/plane.md`: rename every code and prose reference from `WeightKernelFamily` to `WeightKernel`.
- No other `libs/dotnet/` file names either type.

## 4. Replace `SimplexBlend`, skip the unused sample, and repair the simplex coordinate transform

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:435-438`, anchors `SkewF3`, `UnskewG3`, `SupportRadiusSquared`, and `AmplitudeScale`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:485-503`, anchors `SkewedSimplexAt` and private `SimplexAt`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:514-519`, anchor `[SmartEnum<int>] public sealed partial class SimplexBlend`
- consumer `libs/dotnet/Rasm/.planning/Spatial/fields.md:154-158`, anchors `NoiseKind.Simplex` and `NoiseKind.SmoothSimplex`

### From

```csharp
private const double SkewF3 = 1.0 / 3.0;
private const double UnskewG3 = 1.0 / 6.0;
private const double SupportRadiusSquared = 0.6;
private const double AmplitudeScale = 32.0;
```

```csharp
internal static double SkewedSimplexAt(Point3d point, int seed, double frequency, SimplexBlend blend) {
    double stretch = (point.X + point.Y + point.Z) * SkewF3;
    Point3d skewed = new(x: point.X + stretch, y: point.Y + stretch, z: point.Z + stretch);
    return blend.Blended(
        primary: SimplexAt(point: skewed, seed: seed, frequency: frequency, channel: SimplexPrimary),
        rotated: SimplexAt(point: new Point3d(x: skewed.Y, y: skewed.Z, z: skewed.X), seed: seed, frequency: frequency, channel: SimplexRotated));
}
private static double SimplexAt(Point3d point, int seed, double frequency, long channel) {
    double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
    int i = (int)Math.Floor(px); int j = (int)Math.Floor(py); int k = (int)Math.Floor(pz);
    double x0 = px - i; double y0 = py - j; double z0 = pz - k;
```

```csharp
double n1 = SimplexCorner(hash: HashCell(i: i + i1, j: j + j1, k: k + k1, seed: seed, channel: channel),
    x: x0 - i1 + UnskewG3, y: y0 - j1 + UnskewG3, z: z0 - k1 + UnskewG3);
double n2 = SimplexCorner(hash: HashCell(i: i + i2, j: j + j2, k: k + k2, seed: seed, channel: channel),
    x: x0 - i2 + SkewF3, y: y0 - j2 + SkewF3, z: z0 - k2 + SkewF3);
```

```csharp
return AmplitudeScale * (n0 + n1 + n2 + n3);
private static double SimplexCorner(int hash, double x, double y, double z) {
    double t = SupportRadiusSquared - (x * x) - (y * y) - (z * z);
    return t <= 0.0 ? 0.0 : t * t * t * t * Grad(hash: hash, x: x, y: y, z: z);
}
```

```csharp
[SmartEnum<int>]
public sealed partial class SimplexBlend {
    public static readonly SimplexBlend Single = new(key: 0, blended: static (primary, _) => primary);
    public static readonly SimplexBlend Rotated = new(key: 1, blended: static (primary, rotated) => 0.5 * (primary + rotated));
    [UseDelegateFromConstructor] internal partial double Blended(double primary, double rotated);
}
```

```csharp
public static readonly NoiseKind Simplex = new(key: 1, declared: CapabilitySet<NoiseTrait>.Of(NoiseTrait.Differentiable),
    sample: static (p, seed, f) => FieldNoise.SkewedSimplexAt(point: p, seed: seed, frequency: f, blend: SimplexBlend.Single));
public static readonly NoiseKind SmoothSimplex = new(key: 2, declared: CapabilitySet<NoiseTrait>.Of(NoiseTrait.Differentiable),
    sample: static (p, seed, f) => FieldNoise.SkewedSimplexAt(point: p, seed: seed, frequency: f, blend: SimplexBlend.Rotated));
```

### To

```csharp
// The four simplex-only module constants are deleted.
```

```csharp
internal static double SimplexAt(Point3d point, int seed, double frequency, double rotationMix) {
    double primary = SampleSimplex(point, seed, frequency, SimplexPrimary);
    return rotationMix <= 0.0 ? primary : primary + (rotationMix *
        (SampleSimplex(new Point3d(point.Y, point.Z, point.X), seed, frequency, SimplexRotated) - primary));
}
private static double SampleSimplex(Point3d point, int seed, double frequency, long channel) {
    double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
    const double inverseSkew = 1.0 / 6.0;
    double skew = (px + py + pz) / 3.0;
    int i = (int)Math.Floor(px + skew); int j = (int)Math.Floor(py + skew); int k = (int)Math.Floor(pz + skew);
    double unskew = (i + j + k) * inverseSkew;
    double x0 = px - i + unskew; double y0 = py - j + unskew; double z0 = pz - k + unskew;
```

```csharp
double n1 = SimplexCorner(hash: HashCell(i: i + i1, j: j + j1, k: k + k1, seed: seed, channel: channel),
    x: x0 - i1 + inverseSkew, y: y0 - j1 + inverseSkew, z: z0 - k1 + inverseSkew);
double n2 = SimplexCorner(hash: HashCell(i: i + i2, j: j + j2, k: k + k2, seed: seed, channel: channel),
    x: x0 - i2 + (2.0 * inverseSkew), y: y0 - j2 + (2.0 * inverseSkew), z: z0 - k2 + (2.0 * inverseSkew));
```

```csharp
static double SimplexCorner(int hash, double x, double y, double z) {
    double t = 0.6 - (x * x) - (y * y) - (z * z);
    return t <= 0.0 ? 0.0 : t * t * t * t * Grad(hash, x, y, z);
}
return 32.0 * (n0 + n1 + n2 + n3);
```

```csharp
// SimplexBlend deleted.
```

```csharp
public static readonly NoiseKind Simplex = new(key: 1, declared: CapabilitySet<NoiseTrait>.Of(NoiseTrait.Differentiable),
    sample: static (p, seed, f) => FieldNoise.SimplexAt(p, seed, f, rotationMix: 0.0));
public static readonly NoiseKind RotatedSimplex = new(key: 2, declared: CapabilitySet<NoiseTrait>.Of(NoiseTrait.Differentiable),
    sample: static (p, seed, f) => FieldNoise.SimplexAt(p, seed, f, rotationMix: 0.5));
```

### Effect

- Target fenced LOC: **-9**.
- Authored surface: **-1 public type**, **-2 public rows**, **-1 internal delegate member**, and **-5 private module members** (the four constants plus `SimplexCorner`). The corner kernel remains a local function inside its only caller; the two noise methods and one `NoiseKind` row are renamed in place, so their symbol counts are unchanged.
- Runtime: the unrotated row no longer evaluates and discards the rotated lattice sample before the delegate ignores it.
- Correctness: each simplex cell is selected in skewed coordinates, then its corner offsets are measured back in the original Euclidean coordinates through `(i + j + k) / 6`. The current body instead uses the fractional part of the skewed coordinate as `x0/y0/z0`, so every corner distance and attenuation weight is wrong. The local `inverseSkew` names the quantity shared by the inverse transform and corner offsets; the one-use skew, support, and amplitude values remain at their exact operations instead of occupying module members.
- Naming: `SimplexAt`, `SampleSimplex`, `rotationMix`, and `RotatedSimplex` state the algorithm and policy in at most two words; `SkewedSimplexAt`, `SimplexBlend.Single`, and the unearned promise `SmoothSimplex` disappear.

### API and consumer proof

`SimplexBlend` has exactly two consumers, both definition-time `NoiseKind` rows, and no key, lookup, serialization, or independent dispatch consumer. Its `Blended(primary, rotated)` call eagerly evaluates both arguments, so `Single` currently pays the second hash lattice for a value its delegate discards. Variation is one numeric policy column already seated at the consuming row. The standard 3D simplex transform first floors `p + F3 * sum(p)` and then measures each corner from `p - (cell - G3 * sum(cell))`; the replacement transcribes `F3 = 1/3`, `G3 = 1/6`, the second-corner shift `2G3 = 1/3`, and the last-corner shift `3G3 = 1/2` directly. The six simplex orderings remain the canonical tetrahedron selection; only the incorrect coordinate frame around them changes.

### Ripples

- `libs/dotnet/Rasm/.planning/Spatial/fields.md`: apply the exact two-row replacement above and replace prose mentions of `SmoothSimplex` with `RotatedSimplex`.
- Same file: rewrite the NOISE card to name the rotation mix rather than a `SimplexBlend` row.
- No other consumer mentions either deleted row.

## 5. Replace the false permutation layer with one coordinate hash

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:431-452`, anchors `PermTable`, `Perm`, `Fade`, `Lerp`, `GradientTable`, and `Grad`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:453-490`, full `PerlinAt`, the three Worley jitter projections, and the simplex lane reads
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:499-510`, simplex corner hashes, `Jitter`, `HashCell`, and `SimplexCorner`

### From

```csharp
private static readonly int[] PermTable = [
    151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
];
private const long JitterX = 0L, JitterY = 1L, JitterZ = 2L, SimplexPrimary = 0L, SimplexRotated = 1L;

private static int Perm(int x, int seed) =>
    PermTable[(int)(Deterministic.Stream(lanes: [x, seed]) & 0xFF)];
private static double Fade(double t) => t * t * t * ((t * ((t * 6) - 15)) + 10);
private static double Lerp(double t, double a, double b) => a + (t * (b - a));
private static ReadOnlySpan<sbyte> GradientTable =>
    [1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 0,
     1, 0, 1, -1, 0, 1, 1, 0, -1, -1, 0, -1,
     0, 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1];
```

```csharp
private static double Grad(int hash, double x, double y, double z) {
    int seat = ((hash & 15) % 12) * 3;
    return (GradientTable[seat] * x) + (GradientTable[seat + 1] * y) + (GradientTable[seat + 2] * z);
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
```

```csharp
double primary = SampleSimplex(point, seed, frequency, SimplexPrimary);
return rotationMix <= 0.0 ? primary : primary + (rotationMix *
    (SampleSimplex(new Point3d(point.Y, point.Z, point.X), seed, frequency, SimplexRotated) - primary));
```

```csharp
double ddx = nx + (Jitter(x: nx, y: ny, z: nz, seed: seed, channel: JitterX) / 255.0) - px;
double ddy = ny + (Jitter(x: nx, y: ny, z: nz, seed: seed, channel: JitterY) / 255.0) - py;
double ddz = nz + (Jitter(x: nx, y: ny, z: nz, seed: seed, channel: JitterZ) / 255.0) - pz;
```

```csharp
private static double SampleSimplex(Point3d point, int seed, double frequency, long channel) {
```

```csharp
double n0 = SimplexCorner(hash: HashCell(i: i, j: j, k: k, seed: seed, channel: channel), x: x0, y: y0, z: z0);
double n1 = SimplexCorner(hash: HashCell(i: i + i1, j: j + j1, k: k + k1, seed: seed, channel: channel), x: x0 - i1 + inverseSkew, y: y0 - j1 + inverseSkew, z: z0 - k1 + inverseSkew);
double n2 = SimplexCorner(hash: HashCell(i: i + i2, j: j + j2, k: k + k2, seed: seed, channel: channel), x: x0 - i2 + (2.0 * inverseSkew), y: y0 - j2 + (2.0 * inverseSkew), z: z0 - k2 + (2.0 * inverseSkew));
double n3 = SimplexCorner(hash: HashCell(i: i + 1, j: j + 1, k: k + 1, seed: seed, channel: channel), x: x0 - 0.5, y: y0 - 0.5, z: z0 - 0.5);
```

```csharp
private static int Jitter(int x, int y, int z, int seed, long channel) =>
    (int)(Deterministic.Stream(lanes: [x, y, z, channel], seed: seed) & 0xFF);
private static int HashCell(int i, int j, int k, int seed, long channel) => Jitter(i, j, k, seed, channel);
```

### To

```csharp
// PermTable, Perm, and the five module-level lane constants are deleted.
```

```csharp
private static double Grad(int hash, double x, double y, double z) {
    ReadOnlySpan<sbyte> gradients =
        [1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 0,
         1, 0, 1, -1, 0, 1, 1, 0, -1, -1, 0, -1,
         0, 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1];
    int seat = (hash % 12) * 3;
    return (gradients[seat] * x) + (gradients[seat + 1] * y) + (gradients[seat + 2] * z);
}
internal static double PerlinAt(Point3d point, int seed, double frequency) {
    static double Fade(double t) => t * t * t * ((t * ((t * 6) - 15)) + 10);
    static double Lerp(double t, double a, double b) => a + (t * (b - a));
    double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
    int column = (int)Math.Floor(px); int row = (int)Math.Floor(py); int layer = (int)Math.Floor(pz);
    const long lane = 0L;
    double x = px - column; double y = py - row; double z = pz - layer;
    double u = Fade(x); double v = Fade(y); double w = Fade(z);
    return Lerp(w,
        Lerp(v,
            Lerp(u, Grad(LatticeHash(column, row, layer, seed, lane), x, y, z), Grad(LatticeHash(column + 1, row, layer, seed, lane), x - 1, y, z)),
            Lerp(u, Grad(LatticeHash(column, row + 1, layer, seed, lane), x, y - 1, z), Grad(LatticeHash(column + 1, row + 1, layer, seed, lane), x - 1, y - 1, z))),
        Lerp(v,
            Lerp(u, Grad(LatticeHash(column, row, layer + 1, seed, lane), x, y, z - 1), Grad(LatticeHash(column + 1, row, layer + 1, seed, lane), x - 1, y, z - 1)),
            Lerp(u, Grad(LatticeHash(column, row + 1, layer + 1, seed, lane), x, y - 1, z - 1), Grad(LatticeHash(column + 1, row + 1, layer + 1, seed, lane), x - 1, y - 1, z - 1))));
}
```

```csharp
const long xLane = 1L, yLane = 2L, zLane = 3L;
double ddx = nx + (LatticeHash(nx, ny, nz, seed, xLane) / 256.0) - px;
double ddy = ny + (LatticeHash(nx, ny, nz, seed, yLane) / 256.0) - py;
double ddz = nz + (LatticeHash(nx, ny, nz, seed, zLane) / 256.0) - pz;
```

```csharp
const long primaryLane = 4L, rotatedLane = 5L;
double primary = SampleSimplex(point, seed, frequency, primaryLane);
return rotationMix <= 0.0 ? primary : primary + (rotationMix *
    (SampleSimplex(new Point3d(point.Y, point.Z, point.X), seed, frequency, rotatedLane) - primary));
```

```csharp
private static double SampleSimplex(Point3d point, int seed, double frequency, long lane) {
```

```csharp
double n0 = SimplexCorner(LatticeHash(i, j, k, seed, lane), x0, y0, z0);
double n1 = SimplexCorner(LatticeHash(i + i1, j + j1, k + k1, seed, lane), x0 - i1 + inverseSkew, y0 - j1 + inverseSkew, z0 - k1 + inverseSkew);
double n2 = SimplexCorner(LatticeHash(i + i2, j + j2, k + k2, seed, lane), x0 - i2 + (2.0 * inverseSkew), y0 - j2 + (2.0 * inverseSkew), z0 - k2 + (2.0 * inverseSkew));
double n3 = SimplexCorner(LatticeHash(i + 1, j + 1, k + 1, seed, lane), x0 - 0.5, y0 - 0.5, z0 - 0.5);
```

```csharp
private static int LatticeHash(int column, int row, int layer, int seed, long lane) =>
    (int)(Deterministic.Stream(lanes: [column, row, layer, lane], seed: seed) & 0xFF);
```

### Effect

- Target fenced LOC: **-6**.
- Authored surface: **-11 private members** (`PermTable`, `Perm`, forwarding `HashCell`, `Fade`, `Lerp`, `GradientTable`, and the five module-level lane constants); `Jitter` is renamed in place to the algorithm-neutral `LatticeHash`. The fade/interpolation kernels, gradient table view, and six replacement lane values become local symbols inside the algorithms that own them.
- Correctness: the current `Perm` hashes `x` to eight bits before indexing the published table, so collisions make it a many-to-one hash, not a permutation; the 256-entry table and nested `A/AA/AB/B/BA/BB` chain therefore add indirection without preserving the property their names promise. Direct coordinate hashing states the algorithm actually implemented and removes the forced 256-cell period.
- Distribution: the twelve simplex directions are selected from the full low byte with `hash % 12`; `((hash & 15) % 12)` needlessly truncates to four bits and doubles directions 0-3 in every sixteen hashes. Worley maps the byte to `[0, 1)` with `/256.0`; `/255.0` admits the upper endpoint, allowing a cell's feature to sit exactly in its neighbour.
- Independence: six locally named lanes stop Perlin, the three Worley axes, and the two simplex samples from reading the same hash word at equal coordinates; the names use the branch's `Deterministic` lane vocabulary without growing module surface.

### API and consumer proof

`Deterministic.Stream(lanes, seed)` is the branch-owned stateless coordinate hash. `FieldNoise` has no output-parity or serialization consumer; `Spatial/fields` consumes only the total single-octave sample functions. All deleted material is private, and the replacement preserves Perlin's fade/interpolation and twelve published gradients while making its lattice key explicit.

The checked 256-entry `PermTable` literal is the canonical published transcription; the defect is not a bad entry. `Perm` first collapses each coordinate/seed pair through a many-to-one eight-bit hash and only then indexes that table, so the canonical permutation relation is already lost before the nested lookup begins. The replacement therefore deletes the now-misleading table rather than editing its bytes. `LatticeHash` returns the low byte `0..255`, making `hash % 12` nonnegative and allowing all twelve gradient rows; the six local lane ordinals are disjoint by algorithm and role.

### Ripples

- Same file: rewrite the NOISE owner/cases/auto/boundary prose to name one coordinate-hashed lattice substrate; remove the claims that `PermTable` is a live canonical permutation and that Perlin composes `Perm`.
- Outside target: none.

## 6. Use the Thinktecture validation mint and symbolic member names at `SolarSite` admission

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:555-568`, anchors `SiteKey`, `Seq<string> refused`, and the nested `string.Join`

### From

```csharp
private static readonly Op SiteKey = Op.Of(name: nameof(SolarSite));

static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double latitudeDeg, ref double longitudeDeg,
    ref Offset timezone, ref double elevationM) {
    if (double.IsFinite(longitudeDeg)) { longitudeDeg = Reduce.Floored(value: longitudeDeg + 180.0, period: 360.0) - 180.0; }
    Seq<string> refused = toSeq<(bool Held, string Axis)>([
        (double.IsFinite(latitudeDeg) && latitudeDeg is >= -90.0 and <= 90.0, "latitude"),
        (double.IsFinite(longitudeDeg), "longitude"),
        (double.IsFinite(elevationM) && elevationM is > -500.0 and <= 10000.0, "elevation"),
    ]).Filter(static clause => !clause.Held).Map(static clause => clause.Axis);
    validationError = refused.IsEmpty
        ? null
        : new ValidationError(string.Join(" | ", new object?[] { nameof(SolarSite), string.Join(separator: ", ", values: refused), Some(SiteKey) }));
}
```

### To

```csharp
static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double latitudeDeg, ref double longitudeDeg,
    ref Offset timezone, ref double elevationM) {
    if (double.IsFinite(longitudeDeg)) { longitudeDeg = Reduce.Floored(longitudeDeg + 180.0, 360.0) - 180.0; }
    Seq<string> invalid = toSeq<(bool Valid, string Name)>([
        (double.IsFinite(latitudeDeg) && latitudeDeg is >= -90.0 and <= 90.0, nameof(LatitudeDeg)),
        (double.IsFinite(longitudeDeg), nameof(LongitudeDeg)),
        (double.IsFinite(elevationM) && elevationM is > -500.0 and <= 10000.0, nameof(ElevationM)),
    ]).Choose(static clause => clause.Valid ? Option<string>.None : Some(clause.Name));
    validationError = invalid.IsEmpty ? null
        : ValidationError.Create($"{nameof(SolarSite)}: invalid {string.Join(", ", invalid)}");
}
```

### Effect

- Target fenced LOC: **-2**.
- Authored surface: **-1 private member** (`SiteKey`).
- Logic: one `Choose` performs the filter/project; symbolic argument names replace three parallel string literals; one message is minted through the package contract instead of an object array, nested joins, and an `Option<Op>` rendered into text.

### API and consumer proof

The Thinktecture catalogue exposes `ValidationError.Create(string)` and the generated complex-value-object hook takes the error by `ref`. LanguageExt `Seq.Choose` is the combined filter/map. The hook's only consumer is generated `SolarSite.Validate/Create`; `SiteKey` is not evidence carried beyond the ephemeral validation message.

### Ripples

- Same file: the admission prose remains semantically unchanged; it still accumulates every invalid numeric axis in one generated validation result.
- Outside target: none.

## 7. Replace the runtime string-keyed `SolarSeries` roster with one local Horner fold

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:535`, `using System.Collections.Immutable`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:593-616`, full `SolarSeries` owner
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:631-639,655-657`, coefficient reads in `SolarPosition.At`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:628-629`, `SolarPosition.At` entry where local `Polynomial` seats

### From

```csharp
using System.Collections.Immutable;
```

```csharp
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SolarSeries {
    public static readonly SolarSeries MeanLongitude    = new("mean-longitude",    clause: "Meeus, Astronomical Algorithms 2nd ed., ch. 25 (25.2)", coefficients: [280.46646, 36000.76983, 0.0003032]);
    public static readonly SolarSeries MeanAnomaly      = new("mean-anomaly",      clause: "Meeus ch. 25 (25.3)", coefficients: [357.52911, 35999.05029, -0.0001537]);
    public static readonly SolarSeries CentreFirst      = new("centre-first",      clause: "Meeus ch. 25, equation of centre, sin(M)", coefficients: [1.914602, -0.004817, -0.000014]);
    public static readonly SolarSeries CentreSecond     = new("centre-second",     clause: "Meeus ch. 25, equation of centre, sin(2M)", coefficients: [0.019993, -0.000101]);
    public static readonly SolarSeries CentreThird      = new("centre-third",      clause: "Meeus ch. 25, equation of centre, sin(3M)", coefficients: [0.000289]);
    public static readonly SolarSeries Apparent         = new("apparent",          clause: "Meeus ch. 25, apparent longitude correction", coefficients: [0.00569, 0.00478]);
    public static readonly SolarSeries NutationArgument = new("nutation-argument", clause: "Meeus ch. 22, omega argument", coefficients: [125.04, -1934.136]);
    public static readonly SolarSeries ObliquityBase    = new("obliquity-base",    clause: "Meeus ch. 22 (22.2), degree and arcminute base", coefficients: [23.0, 26.0]);
    public static readonly SolarSeries Obliquity        = new("obliquity",         clause: "Meeus ch. 22 (22.2), arcsecond tail", coefficients: [21.448, -46.815, -0.00059, 0.001813]);
    public static readonly SolarSeries Refraction       = new("refraction",        clause: "Saemundsson/Bennett, Meeus ch. 16 (16.4)", coefficients: [1.02, 10.3, 5.11]);

    public string Clause { get; }
    public ImmutableArray<double> Coefficients { get; }

    public double At(double t) {
        double accumulated = 0.0;
        for (int index = Coefficients.Length - 1; index >= 0; index--) { accumulated = Coefficients[index] + (t * accumulated); }
        return accumulated;
    }
    public double this[int index] => Coefficients[index];
}
```

```csharp
public static SunPosition At(SolarSite site, Instant instant) {
double meanLongitude = Wrap360(SolarSeries.MeanLongitude.At(t: t));
double meanAnomaly = SolarSeries.MeanAnomaly.At(t: t) * Radians;
double center = (Math.Sin(meanAnomaly) * SolarSeries.CentreFirst.At(t: t))
    + (Math.Sin(2.0 * meanAnomaly) * SolarSeries.CentreSecond.At(t: t))
    + (Math.Sin(3.0 * meanAnomaly) * SolarSeries.CentreThird.At(t: t));
double eclipticLongitude = (meanLongitude + center - SolarSeries.Apparent[0]
    - (SolarSeries.Apparent[1] * Math.Sin(SolarSeries.NutationArgument.At(t: t) * Radians))) * Radians;
double obliquity = (SolarSeries.ObliquityBase[0]
    + ((SolarSeries.ObliquityBase[1] + (SolarSeries.Obliquity.At(t: t) / 60.0)) / 60.0)) * Radians;
```

```csharp
double refractionDeg = altitudeDeg is > -1.0 and < 90.0
    ? pressureRatio * SolarSeries.Refraction[0]
      / Math.Tan((altitudeDeg + (SolarSeries.Refraction[1] / (altitudeDeg + SolarSeries.Refraction[2]))) * Radians) / 60.0
    : 0.0;
```

### To

```csharp
// System.Collections.Immutable import and SolarSeries deleted.
```

```csharp
public static SunPosition At(SolarSite site, Instant instant) {
static double Polynomial(double t, params ReadOnlySpan<double> coefficients) {
    double value = 0.0;
    for (int i = coefficients.Length - 1; i >= 0; i--) { value = coefficients[i] + (t * value); }
    return value;
}
```

```csharp
double meanLongitude = Wrap360(Polynomial(t, [280.46646, 36000.76983, 0.0003032]));
double meanAnomaly = Polynomial(t, [357.52911, 35999.05029, -0.0001537]) * Radians;
double center = (Math.Sin(meanAnomaly) * Polynomial(t, [1.914602, -0.004817, -0.000014]))
    + (Math.Sin(2.0 * meanAnomaly) * Polynomial(t, [0.019993, -0.000101]))
    + (Math.Sin(3.0 * meanAnomaly) * 0.000289);
double eclipticLongitude = (meanLongitude + center - 0.00569
    - (0.00478 * Math.Sin(Polynomial(t, [125.04, -1934.136]) * Radians))) * Radians;
double obliquity = (23.0 + ((26.0 + (Polynomial(t, [21.448, -46.815, -0.00059, 0.001813]) / 60.0)) / 60.0)) * Radians;
```

```csharp
double refractionDeg = altitudeDeg is > -1.0 and < 90.0
    ? pressureRatio * 1.02 / Math.Tan((altitudeDeg + (10.3 / (altitudeDeg + 5.11))) * Radians) / 60.0
    : 0.0;
```

### Effect

- Target fenced LOC: **-20**.
- Authored surface: **-1 public type** and **-14 authored members**: the ten public rows, two public properties, public evaluator, and public indexer disappear. `Polynomial` is a local function inside `At`, not a replacement module member.
- Generated surface: all keyed lookup, conversion, parse, item-roster, metadata, and dispatch members for `SolarSeries` disappear.
- Material: runtime citation strings, string keys, immutable-array owners, and per-row objects disappear. Coefficients remain next to the named astronomical quantities that consume them, and every polynomial shares one span-based Horner fold local to their sole consuming operation.

### API and consumer proof

No `libs/dotnet/` consumer outside this fence mentions `SolarSeries`; inside it no code reads `Key`, `Clause`, `Items`, lookup, conversion, or generated dispatch. The rows therefore are neither a vocabulary nor a policy owner—only indirect storage for coefficients. C# `params ReadOnlySpan<T>` accepts collection expressions without heap-array ownership, and Horner evaluation preserves the existing ascending coefficient order exactly.

The replacement transcribes every live coefficient exactly: mean longitude `[280.46646, 36000.76983, 0.0003032]`, mean anomaly `[357.52911, 35999.05029, -0.0001537]`, centre terms `[1.914602, -0.004817, -0.000014]`, `[0.019993, -0.000101]`, and `0.000289`, apparent longitude `0.00569/0.00478`, nutation `[125.04, -1934.136]`, mean obliquity `23/26` plus `[21.448, -46.815, -0.00059, 0.001813]`, and refraction `1.02/10.3/5.11`. Publication citations remain in the design card beside these named quantities; no runtime string/key/object is retained merely to carry provenance.

### Ripples

- Same file: remove `SolarSeries` from the owner/cases/packages prose. Keep the publication/equation citations as design-page facts, not runtime strings; state that `At` names each astronomical quantity and evaluates polynomial coefficient spans through `Polynomial`.
- Outside target: none.

## 8. Use NodaTime's Julian-date projection instead of re-deriving it from Unix ticks

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:623-625`, anchors `JulianUnixEpoch`, `JulianJ2000`, and `JulianCentury`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:629-630`, anchors `double jd` and `double t`

### From

```csharp
private const double JulianUnixEpoch = 2440587.5;
private const double JulianJ2000 = 2451545.0;
private const double JulianCentury = 36525.0;
```

```csharp
double jd = JulianUnixEpoch + instant.ToUnixTimeTicks() / (double)NodaConstants.TicksPerDay;
double t = (jd - JulianJ2000) / JulianCentury;
```

### To

```csharp
// The three module-level Julian constants are deleted.
```

```csharp
const double j2000 = 2451545.0, centuryDays = 36525.0;
double jd = instant.ToJulianDate();
double t = (jd - j2000) / centuryDays;
```

### Effect

- Target fenced LOC: **-2**.
- Authored surface: **-3 private constants**; the two quantities still needed by the century projection become locals in `At`.
- Logic: the semantic-time owner performs its own exact epoch projection; this page retains only the astronomical centuries-from-J2000 calculation it owns.

### API and consumer proof

The NodaTime catalogue exposes `Instant.ToJulianDate()` as the direct epoch export. The hand expression has one caller and duplicates the package's Unix-to-Julian correspondence. J2000 and days per Julian century remain named at their one equation without occupying module members.

### Ripples

- Same file: none beyond deleting the three constants and replacing the two lines.
- Outside target: none.

## 9. Name the fixed NodaTime value as the site's standard offset, not a time zone

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:550-553`, anchors `Timezone` and `TimezoneHours`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:557-558`, generated-hook parameter `timezone`
- every `SolarSite.Validate` named argument and every property read listed under Ripples

### From

```csharp
public Offset Timezone { get; }
public double TimezoneHours => Timezone.Seconds / (double)NodaConstants.SecondsPerHour;
```

```csharp
ref Offset timezone
```

### To

```csharp
public Offset StandardOffset { get; }
public double OffsetHours => StandardOffset.Seconds / (double)NodaConstants.SecondsPerHour;
```

```csharp
ref Offset standardOffset
```

### Effect

- Target fenced LOC and authored symbol count: **unchanged**; both members are renamed in place.
- Semantics: this value is the site's standard displacement from UTC, not a `DateTimeZone`; `StandardOffset` matches EPW's standard-time field, Rhino's separate daylight-saving column, and the AppUi equality against `ZoneInterval.StandardOffset`. The names stop promising DST rules, historical transitions, or zone identity the value cannot carry.
- Generated surface: the complex-value-object factory parameter becomes `standardOffset`, so named construction states the admitted carrier honestly; `OffsetHours` remains the compact derived projection used by wire and identity consumers.

### API and consumer proof

Every consumer treats the value as a standard fixed offset: local dates call `WithOffset`, the AppUi calendar check compares it to `ZoneInterval.StandardOffset`, EPW supplies its standard-time offset, and Rhino stores daylight-saving minutes separately. No consumer calls a time-zone rule or reads a zone id from it. The current Rhino named argument `timezoneHours: placement.TimeZone` is additionally invalid against the generated `Offset` parameter; the ripple below converts that raw host-hour value through the same `Offset.FromTicks(hours * TicksPerHour)` spelling already used by the daylight EPW ingress.

### Ripples

- `libs/dotnet/Rasm.Materials/.planning/Appearance/environment.md:421`: `timezone: Offset.Zero` -> `standardOffset: Offset.Zero`.
- `libs/dotnet/Rasm.Rhino/.planning/Render/settings.md:393-394,1037`: narrow the raw `SunPlacement.TimeZone` gate from `[-24,24]` to the already-declared engine interval `[-12,14]`, then replace the invalid `timezoneHours: placement.TimeZone` argument with `standardOffset: Offset.FromTicks((long)(placement.TimeZone * NodaConstants.TicksPerHour))`; the proved range keeps NodaTime construction non-throwing.
- `libs/dotnet/Rasm.AppUi/.planning/Analysis/context.md:151-153`: both `site.Timezone` reads -> `site.StandardOffset`.
- `libs/dotnet/Rasm.Compute/.planning/Analysis/daylight.md:84,173,197,344`: every `site.Timezone`/`admitted.Timezone` read -> `StandardOffset`.
- `libs/dotnet/Rasm.Compute/.planning/Analysis/assessment.md:210`: `site.TimezoneHours` -> `site.OffsetHours`.
- `libs/dotnet/Rasm.Rhino/.planning/Render/settings.md:1010-1011` and `Rasm.Rhino/.planning/Objects/lights.md:954`: every `TimezoneHours` read -> `OffsetHours`.
- `libs/dotnet/Rasm.Rhino/.planning/Viewport/capture.md:803`: replace “NodaTime `Offset` timezone”/“timezone ... required” with “fixed standard offset”/“standard offset ... required”; the page owns no zone rules.
- Same-file prose: replace claims of a timezone with a fixed standard offset and state that an actual `DateTimeZone` remains the calendar/application boundary's owner.

## 10. Restore the time-varying eccentricity and corrected obliquity in the solar fold

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:620-622`, anchors `Eccentricity`, `LapseRate`, and `LapseExponent`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:636-646`, the apparent-longitude, obliquity, and equation-of-time chain; apply after move 7's local `Polynomial` replacement
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:654-655`, the one-use atmosphere constants' pressure fold

### From

```csharp
private const double Eccentricity = 0.016708634;
private const double LapseRate = 2.25577e-5;
private const double LapseExponent = 5.25588;
```

```csharp
double eclipticLongitude = (meanLongitude + center - 0.00569
    - (0.00478 * Math.Sin(Polynomial(t, [125.04, -1934.136]) * Radians))) * Radians;
double obliquity = (23.0 + ((26.0 + (Polynomial(t, [21.448, -46.815, -0.00059, 0.001813]) / 60.0)) / 60.0)) * Radians;
double declination = Math.Asin(Math.Sin(obliquity) * Math.Sin(eclipticLongitude));
double y = Math.Tan(obliquity / 2.0) * Math.Tan(obliquity / 2.0);
double meanLonRad = meanLongitude * Math.PI / 180.0;
double equationOfTime = 4.0 * (180.0 / Math.PI) * (
    y * Math.Sin(2.0 * meanLonRad) - 2.0 * Eccentricity * Math.Sin(meanAnomaly)
    + 4.0 * Eccentricity * y * Math.Sin(meanAnomaly) * Math.Cos(2.0 * meanLonRad)
    - 0.5 * y * y * Math.Sin(4.0 * meanLonRad) - 1.25 * Eccentricity * Eccentricity * Math.Sin(2.0 * meanAnomaly));
```

```csharp
double pressureRatio = Math.Pow(1.0 - (LapseRate * site.ElevationM), LapseExponent);
```

### To

```csharp
// Eccentricity and the two atmosphere-only module constants are deleted.
```

```csharp
double nutationArgument = Polynomial(t, [125.04, -1934.136]) * Radians;
double eclipticLongitude = (meanLongitude + center - 0.00569 - (0.00478 * Math.Sin(nutationArgument))) * Radians;
double obliquity = (23.0 + ((26.0 + (Polynomial(t, [21.448, -46.815, -0.00059, 0.001813]) / 60.0)) / 60.0)
    + (0.00256 * Math.Cos(nutationArgument))) * Radians;
double declination = Math.Asin(Math.Sin(obliquity) * Math.Sin(eclipticLongitude));
double y = Math.Tan(obliquity / 2.0) * Math.Tan(obliquity / 2.0);
double eccentricity = Polynomial(t, [0.016708634, -0.000042037, -0.0000001267]);
double meanLonRad = meanLongitude * Radians;
double equationOfTime = 4.0 / Radians * (
    y * Math.Sin(2.0 * meanLonRad) - 2.0 * eccentricity * Math.Sin(meanAnomaly)
    + 4.0 * eccentricity * y * Math.Sin(meanAnomaly) * Math.Cos(2.0 * meanLonRad)
    - 0.5 * y * y * Math.Sin(4.0 * meanLonRad) - 1.25 * eccentricity * eccentricity * Math.Sin(2.0 * meanAnomaly));
```

```csharp
const double lapse = 2.25577e-5, pressurePower = 5.25588;
double pressureRatio = Math.Pow(1.0 - (lapse * site.ElevationM), pressurePower);
```

### Effect

- Target fenced LOC: **unchanged**; authored surface: **-3 private members** (`Eccentricity`, `LapseRate`, and `LapseExponent`).
- Correctness: the NOAA/Meeus equation of time uses the Julian-century eccentricity polynomial, not its J2000 constant term forever. Apparent declination uses corrected obliquity, adding `0.00256 cos(Ω)` to the mean obliquity; the current fold corrects apparent longitude with `Ω` but omits the paired obliquity correction.
- Logic: `nutationArgument` is evaluated once and read by both corrections; `Radians` owns both degree conversion directions already, removing three re-spellings of `π / 180` without adding another constant. The atmosphere values stay named beside their sole pressure equation instead of occupying two module members.

### API and consumer proof

This is an internal body repair on `SolarPosition.At`; every consumer already projects only its `SunPosition` result. The coefficient order remains ascending under move 7's Horner fold: `0.016708634 - 0.000042037T - 0.0000001267T²`. The apparent-longitude and corrected-obliquity terms share the same Meeus/NOAA nutation argument, so one local quantity is the single authority.

### Ripples

- Same file: change the solar Entry/Exemption prose from a “full nested obliquity expression” to the precise mean-obliquity polynomial plus the apparent-obliquity correction, and state that eccentricity is evaluated at the current Julian century.
- Outside target: none.

## 11. Cancel the UTC-offset algebra inside the instant-based ephemeris

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:647-648`, `fractionalDay` and `trueSolarMinutes`

### From

```csharp
double fractionalDay = jd - Math.Floor(jd) - 0.5 + site.OffsetHours / 24.0;
double trueSolarMinutes = Reduce.Floored((fractionalDay * 1440.0) + equationOfTime + (4.0 * site.LongitudeDeg) - (60.0 * site.OffsetHours), 1440.0);
```

### To

```csharp
double fractionalDay = jd - Math.Floor(jd) - 0.5;
double trueSolarMinutes = Reduce.Floored((fractionalDay * 1440.0) + equationOfTime + (4.0 * site.LongitudeDeg), 1440.0);
```

### Effect

- Target fenced LOC and authored symbol count: **unchanged**.
- Logic: `OffsetHours / 24 * 1440` is exactly `60 * OffsetHours`, so the two terms cancel. `At` consumes an absolute `Instant`; the standard offset remains correctly owned by `SolarSite` for local-day construction and host projection, not as a redundant intermediate in the instant equation.

### API and consumer proof

NodaTime `Instant` is an absolute timestamp. For fixed longitude and instant, changing the standard offset cannot change the physical solar position; retaining both offset terms merely obscures their exact cancellation. `OffsetHours` remains a live projection used by three external host/identity consumers, so it stays on the owner instead of duplicating the conversion at each caller.

### Ripples

- Same file: clarify that the standard offset selects local civil-day boundaries; `At(SolarSite, Instant)` is offset-invariant for a fixed instant and longitude.
- Outside target: none beyond move 9's rename ripples.

## 12. Delete the one-hop angular wrapper, one-use unit helper, and statement-bodied `SunPath` projection

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:588-590`, `SunPosition.OfUnit`
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:631`, mean-longitude wrap after move 7
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:662-668`, `SunPath` and `Wrap360`

### From

```csharp
public static Option<SunPosition> OfDirection(Vector3d direction) =>
    direction.Length switch {
        double length when direction.IsValid && length > 0.0 => Some(OfUnit(direction / length)),
        _ => None,
    };

static SunPosition OfUnit(Vector3d unit) =>
    new(AzimuthDeg: SolarPosition.Wrap360(Math.Atan2(unit.X, unit.Y) * 180.0 / Math.PI),
        AltitudeDeg: Math.Asin(Math.Clamp(unit.Z, -1.0, 1.0)) * 180.0 / Math.PI);
```

```csharp
double meanLongitude = Wrap360(Polynomial(t, [280.46646, 36000.76983, 0.0003032]));
```

```csharp
public static Seq<(Instant Instant, SunPosition Sun)> SunPath(SolarSite site, Instant midnight, Duration step, Dimension samples) =>
    toSeq(Enumerable.Range(0, samples.Value)).Map(i => {
        Instant at = midnight + step * i;
        return (at, At(site, at));
    });

internal static double Wrap360(double degrees) => Reduce.Floored(value: degrees, period: 360.0);
```

### To

```csharp
public static Option<SunPosition> OfDirection(Vector3d direction) =>
    from unit in (direction.Length switch {
        double length when direction.IsValid && double.IsFinite(length) && length > 0.0 => Some(direction / length),
        _ => None,
    })
    select new SunPosition(
        AzimuthDeg: Reduce.Floored(Math.Atan2(unit.X, unit.Y) * 180.0 / Math.PI, 360.0),
        AltitudeDeg: Math.Asin(Math.Clamp(unit.Z, -1.0, 1.0)) * 180.0 / Math.PI);
```

```csharp
double meanLongitude = Reduce.Floored(Polynomial(t, [280.46646, 36000.76983, 0.0003032]), 360.0);
```

```csharp
public static Seq<(Instant Instant, SunPosition Sun)> SunPath(SolarSite site, Instant midnight, Duration step, Dimension samples) =>
    from i in toSeq(Enumerable.Range(0, samples.Value))
    let at = midnight + (step * i)
    select (at, At(site, at));
```

### Effect

- Target fenced LOC: **-2**.
- Authored surface: **-2 internal members** (`OfUnit`, `Wrap360`).
- Logic: unit-direction admission maps directly to the position value, both angular callers resolve to the owning `Reduce.Floored` operation, and the path query's `let` computes each instant once without a statement body.

### API and consumer proof

`OfUnit` has one caller and adds no independent admission or policy; `Option`'s query projection keeps the admitted unit vector in the presence carrier while constructing the result inline. The same gate now rejects a non-finite derived length before division: Rhino `Vector3d.IsValid` rejects the host unset sentinel but proves component validity, not that a magnitude calculation over extreme finite components stayed finite. `Wrap360` forwards twice to `Reduce.Floored` and adds no admission, policy, or evidence. `SunPath` already returns LanguageExt `Seq`; its BCL `Enumerable.Range` result is correctly re-entered through `toSeq` before the query expression, and `let` retains the one-evaluation property of the deleted statement local.

### Ripples

- Same file: remove `Wrap360`/`OfUnit` from any prose spelling. No external consumer calls either member.

## 13. Enforce the declared nonnegative-distance domain at both span entries

### Location

- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:302-307`, the first and last `SpanProfile.Fill` admission claims
- `libs/dotnet/Rasm/.planning/Numerics/calculus.md:358-362`, the `Falloff.Weights` admission fold

### From

```csharp
Fin<Unit> admitted = Admit.Claims(key,
    (distances.Length >= 1, "distance-extent"),
    (destination.Length >= distances.Length, "destination-extent"),
    (ValidityClaim.Positive(value: scale), "scale"),
    (ValidityClaim.Finite(values: distances), "distances-finite"));
```

```csharp
if (!ValidityClaim.All(ValidityClaim.CountAtLeast(count: distances.Length, floor: 1),
        ValidityClaim.CountAtLeast(count: destination.Length, floor: distances.Length),
        ValidityClaim.Nonnegative(value: tolerance), ValidityClaim.Finite(values: distances))) {
```

### To

```csharp
Fin<Unit> admitted = Admit.Claims(key,
    (distances.Length >= 1 && TensorPrimitives.Min<double>(distances) >= 0.0, "distances"),
    (destination.Length >= distances.Length, "destination-extent"),
    (ValidityClaim.Positive(scale), "scale"),
    (ValidityClaim.Finite(distances), "distances-finite"));
```

```csharp
if (!ValidityClaim.All(distances.Length >= 1 && TensorPrimitives.Min<double>(distances) >= 0.0,
        ValidityClaim.CountAtLeast(destination.Length, distances.Length),
        ValidityClaim.Nonnegative(tolerance), ValidityClaim.Finite(distances))) {
```

### Effect

- Target fenced LOC and authored symbol count: **unchanged**.
- Correctness: the admitted scalar `KernelKind.Profile` and `Falloff.Weight` entries require nonnegative distance, while the current span paths let Gaussian rows square a negative value and compact rows evaluate a negative normalized coordinate. The `Weight` fast paths may still canonicalize trusted negative input to the origin, but the `Fin<Unit>` span entries now gate their raw spans instead of silently assigning a weight to an impossible input.
- Logic: the nonempty check and minimum fold share one short-circuiting expression, so `TensorPrimitives.Min` is never called on an empty span and no second traversal helper is introduced.

### API and consumer proof

`Admit.KernelInput` and `Admit.FalloffInput` are the scalar authority and both require `ValidityClaim.Nonnegative(distance)`. `TensorPrimitives.Min<double>(ReadOnlySpan<double>)` is already composed by the `PowerCase` arm in this same member; lifting it into admission makes the invariant common to every row before dispatch.

### Ripples

- Same file only; no consumer signature changes.

## Deliberate non-moves

- Keep `KernelKind` keyed: compute persists `Kernel.Key` as evidence.
- Keep `KernelStatus` as a keyless Thinktecture vocabulary, not a native enum: it is an owned process-local status family and therefore falls under the generated-owner chooser; the language-enum carve is boundary-only.
- Keep `KernelProfile` and both derivative slots: `FirstDerivative` remains live in `Falloff.Slope`; `SecondDerivative` is coherent calculus output but has no current consumer. When these moves land, correct the same-file `[03]-[WEIGHT_PROFILES]` Law sentence and `Rasm.Compute/.planning/Tensor/sampling.md:484,493` so they stop claiming a present Hessian/second-derivative consumer; do not erase the complete profile merely to improve a count.
- Keep `Falloff.Inverse` and `Falloff.InverseSquare`: they are standard named radial laws and valid public construction capability. Their current consumer count does not justify replacing the domain vocabulary with raw exponents.
- Keep the lattice statement kernels and `Reflect`/`Tap`: their span and border arithmetic is the declared hot-path exemption. `LatticeAxes` is shared by three operators, so inlining it would duplicate six taps.
- Do not replace `Falloff` with ad-hoc generic unions: its `KernelCase` and `MetricCase` have distinct evidence shapes but overlap payload types, so wrapper payload records would add types rather than remove them.
- Keep `SolarSite.StandardOffset` and `OffsetHours`: the standard offset is live in local-day construction and cross-package calendar checks, while the derived hours projection is reused by host/wire consumers. Only its algebraically cancelling use inside `At` disappears.

# [RASM_NUMERICS_CALCULUS]

`Rasm.Numerics` calculus is the sample-anywhere analytic-math floor: differential operators, weight-profile mathematics, procedural noise lattices, and the geodetic solar almanac — the field operators generic over a sampler, the almanac closed-form over a site and an instant — so no field, mesh, or cloud type reaches this floor.

Every operator threads `Op` and gates finite input through the Domain validation vocabulary, so admission composes upstream and this floor carries the mathematics alone; the `SlopeBound` and `DerivativeSupremum` slope evidence its kernels and profiles carry feeds the `Spatial/fields` Lipschitz fold downstream.

## [01]-[INDEX]

- [02]-[DIFFERENTIAL_STENCIL]: `Nabla` sampler-generic central-difference stencil and the differential operators folding through its one `SampleAxes` traversal.
- [03]-[WEIGHT_PROFILES]: compact-support kernels, reconstruction weights, and the radial-decay `Falloff` union with its metric-sampler anisotropic case.
- [04]-[NOISE_LATTICES]: `FieldNoise` deterministic Perlin, simplex, and Worley lattices over one seed-folded permutation substrate.
- [05]-[SOLAR_EPHEMERIS]: `SolarPosition` the branch's one NOAA/Meeus apparent-solar fold over a validated `SolarSite` and a NodaTime `Instant`.

## [02]-[DIFFERENTIAL_STENCIL]

- Owner: `Nabla` the `static` differential-calculus owner; `SampleAxes` evaluates the six axis-offset samples `f(p ± ε·eᵢ)` through one traversal every first- and second-order operator composes; `LatticeAxes` is its TOTAL lattice twin — six taps by index with the border reflected — that `LatticeGradientAt`/`LatticeLaplacianAt`/`LatticeHessianAt` read.
- Cases: gradient, curl, curl-noise, divergence, Laplacian, and strain-magnitude over the shared stencil, with the periodic `ToroidalWrap`; the lattice arm carries gradient, Laplacian, and packed-upper Hessian over a `CellLattice`-addressed value span.
- Entry: every ambient operator takes `(sampler, point, eps, key)`; `eps` is the caller's scale-derived stencil width, gated finite and above `EpsilonPolicy.ZeroTolerance` — this floor never guesses a scale. Every lattice operator takes `(values, grid, column, row, layer)` — non-`Fin` and allocation-free, the spacing read off `CellLattice.CellSize` per axis so an anisotropic lattice differentiates true.
- Auto: every ambient operator shares the one `SampleAxes` traversal, and a failed tap short-circuits the rail with the sampler's own typed fault; every lattice operator is total on an admitted lattice — the reflected border makes an out-of-census tap read its mirror cell, and a rank-2 lattice degenerates the Z taps so one body serves both ranks.
- Receipt: none — the operators are pure projections, evidence owned by the composing field or solver.
- Packages: LanguageExt.Core (`Fin`, query expressions), Rasm.Domain (`Op`), RhinoCommon (`Point3d`/`Vector3d` value structs).
- Growth: a new differential operator is one member over the `SampleAxes` stencil; a higher-order stencil is one alternative member the operators re-bind to, never a per-field re-implementation.
- Boundary: mesh-aware Laplacians over connectivity are `Meshing/mesh`'s, this page differentiating ambient ℝ³ samplers and `CellLattice`-addressed value spans alone; the lattice arm addresses and never stores — the value span is the consumer's, the lattice the `Numerics/atoms` owner; `ToroidalWrap` is a total pure fold over an admitted strictly-positive period the Domain `Period` guard gates upstream.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using Rasm.Domain;

namespace Rasm.Numerics;

// --- [OPERATIONS] -------------------------------------------------------------------------
public static class Nabla {
    // Bridson curl-noise: three gradient taps of one potential spread far apart, so the cross-assembly is divergence-free without lattice correlation.
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

    // --- [LATTICE_STENCIL]
    // TOTAL lattice arm beside the ambient sampler: a lattice-backed value differentiates by INDEX with the border
    // reflected — non-Fin and allocation-free, because a texel or voxel plane can neither supply a
    // Func<Point3d, Fin<T>> sampler nor afford one Fin allocation per tap. CellSize scales each axis independently,
    // so an anisotropic, rotated, or sheared lattice reads its own true spacing. Rank 2 degenerates the Z taps to
    // the centre so every operator is one body over both ranks.
    public static (double X1, double X0, double Y1, double Y0, double Z1, double Z0) LatticeAxes(
        ReadOnlySpan<double> values, CellLattice grid, int column, int row, int layer = 0) {
        (int columns, int rows, int layers) = (grid.Columns.Value, grid.Rows.Value, grid.Layers.Value);
        static int Reflect(int index, int count) =>
            count is 1 ? 0 : index < 0 ? -index : index >= count ? (2 * count) - index - 2 : index;
        double At(int c, int r, int l) =>
            values[(int)grid.Linear(column: Reflect(index: c, count: columns), row: Reflect(index: r, count: rows), layer: Reflect(index: l, count: layers))];
        return (X1: At(c: column + 1, r: row, l: layer), X0: At(c: column - 1, r: row, l: layer),
                Y1: At(c: column, r: row + 1, l: layer), Y0: At(c: column, r: row - 1, l: layer),
                Z1: At(c: column, r: row, l: layer + 1), Z0: At(c: column, r: row, l: layer - 1));
    }
    public static Vector3d LatticeGradientAt(ReadOnlySpan<double> values, CellLattice grid, int column, int row, int layer = 0) {
        (double x1, double x0, double y1, double y0, double z1, double z0) =
            LatticeAxes(values: values, grid: grid, column: column, row: row, layer: layer);
        Vector3d cell = grid.CellSize;
        return new Vector3d(x: (x1 - x0) / (2.0 * cell.X), y: (y1 - y0) / (2.0 * cell.Y),
            z: grid.Rank is 3 ? (z1 - z0) / (2.0 * cell.Z) : 0.0);
    }
    public static double LatticeLaplacianAt(ReadOnlySpan<double> values, CellLattice grid, int column, int row, int layer = 0) {
        (double x1, double x0, double y1, double y0, double z1, double z0) =
            LatticeAxes(values: values, grid: grid, column: column, row: row, layer: layer);
        double center = values[(int)grid.Linear(column: column, row: row, layer: layer)];
        Vector3d cell = grid.CellSize;
        double planar = ((x1 + x0 - (2.0 * center)) / (cell.X * cell.X)) + ((y1 + y0 - (2.0 * center)) / (cell.Y * cell.Y));
        return grid.Rank is 3 ? planar + ((z1 + z0 - (2.0 * center)) / (cell.Z * cell.Z)) : planar;
    }
    // Packed-upper (Xx, Xy, Xz, Yy, Yz, Zz) second differences — diagonal off the six-tap axes, mixed partials off
    // the four corner taps per pair — so a SymmetricMatrix admission downstream is repack-free. Eigenvalue
    // projection, physical scaling, and signed packing stay the consumer's.
    public static (double Xx, double Xy, double Xz, double Yy, double Yz, double Zz) LatticeHessianAt(
        ReadOnlySpan<double> values, CellLattice grid, int column, int row, int layer = 0) {
        (int columns, int rows, int layers) = (grid.Columns.Value, grid.Rows.Value, grid.Layers.Value);
        static int Reflect(int index, int count) =>
            count is 1 ? 0 : index < 0 ? -index : index >= count ? (2 * count) - index - 2 : index;
        double At(int c, int r, int l) =>
            values[(int)grid.Linear(column: Reflect(index: c, count: columns), row: Reflect(index: r, count: rows), layer: Reflect(index: l, count: layers))];
        (double x1, double x0, double y1, double y0, double z1, double z0) =
            LatticeAxes(values: values, grid: grid, column: column, row: row, layer: layer);
        double center = At(c: column, r: row, l: layer);
        Vector3d cell = grid.CellSize;
        double xy = (At(c: column + 1, r: row + 1, l: layer) - At(c: column + 1, r: row - 1, l: layer)
                   - At(c: column - 1, r: row + 1, l: layer) + At(c: column - 1, r: row - 1, l: layer)) / (4.0 * cell.X * cell.Y);
        (double xz, double yz, double zz) = grid.Rank is 3
            ? (Xz: (At(c: column + 1, r: row, l: layer + 1) - At(c: column + 1, r: row, l: layer - 1)
                  - At(c: column - 1, r: row, l: layer + 1) + At(c: column - 1, r: row, l: layer - 1)) / (4.0 * cell.X * cell.Z),
               Yz: (At(c: column, r: row + 1, l: layer + 1) - At(c: column, r: row + 1, l: layer - 1)
                  - At(c: column, r: row - 1, l: layer + 1) + At(c: column, r: row - 1, l: layer - 1)) / (4.0 * cell.Y * cell.Z),
               Zz: (z1 + z0 - (2.0 * center)) / (cell.Z * cell.Z))
            : (Xz: 0.0, Yz: 0.0, Zz: 0.0);
        return (Xx: (x1 + x0 - (2.0 * center)) / (cell.X * cell.X), Xy: xy, Xz: xz,
                Yy: (y1 + y0 - (2.0 * center)) / (cell.Y * cell.Y), Yz: yz, Zz: zz);
    }
}
```

## [03]-[WEIGHT_PROFILES]

- Owner: `KernelProfile` carries value, first and second derivative, and a `KernelProfileStatus` smoothness verdict, so a consumer reads a kernel's derivative off the profile instead of re-differencing; `KernelKind` mints the kernel bases in three bands — compact-support rows through one `SupportProfile` clamp, band-limited `Lanczos`/`Jinc` reconstruction rows whose profiles evaluate at 106-bit through the `ddouble` cardinal ladder and narrow once, and globally-supported RBF rows through the clamp-free `GlobalProfile` twin — each row carrying `DerivativeSupremum`, its dimensionless slope-bound numerator, and `PolynomialOrder`, the reproduction-tail order the conditionally-positive-definite bases demand; `WeightKernelFamily` mints the reconstruction-weight profiles with the `Interpolating` column the MLS dispatches on; `Falloff` the radial-decay `[Union]` whose anisotropic case takes a `SymmetricMatrix` metric sampler driving the Mahalanobis distance.
- Cases: the `KernelProfileStatus` verdicts, the compact, band-limited, and global `KernelKind` rows, the `WeightKernelFamily` weights including the band-limited interpolating row, and the `Falloff` decay cases including the metric-sampler anisotropic one.
- Entry: `KernelKind.Profile(distance, radius, key)` returns the full gated profile and `Weight` the bare fast path; `WeightKernelFamily.Weight` zeros outside support; `Falloff.Weight` discriminates bare distance, offset vector, and offset-plus-sample-point through one `WeightCore` gated by `Admit.FalloffInput`.
- Auto: `SupportProfile` is the one clamp/status fold every kernel shares, banded on the dimensionless `q = d/r` so classification is scale-invariant with exact zeros outside support; the metric falloff proves the sampled tensor SPD by leading principal minors before forming the quadratic, allocation-free, so an indefinite metric fails typed instead of producing `√negative`.
- Receipt: `KernelProfile` is the per-evaluation receipt — value, both derivatives, and status.
- Packages: Thinktecture.Runtime.Extensions (`[UseDelegateFromConstructor]` columns), LanguageExt.Core, `SymmetricMatrix` the metric carrier, TYoshimura.DoubleDouble (`ddouble.Sinc`/`CosPi`/`BesselJ` behind the band-limited rows), Rasm.Domain (`Op`, the `Admit.KernelInput`/`FalloffInput` gates, the `AcceptValidated<TVO>` bridge).
- Growth: a new kernel is one `KernelKind` row with its three delegate columns and `DerivativeSupremum`; a new reconstruction weight is one `WeightKernelFamily` row; a new decay law is one `Falloff` case, one `WeightCore` arm, and its `SlopeBound` column.
- Boundary: `Spatial/fields` wraps `Falloff.Metric` over its `TensorField` by passing the tensor sampler, so the tensor-field type never appears here; `Meshing/reconstruct` composes `KernelKind` and `WeightKernelFamily` for its RBF, MLS, and Levin windows — one profile mathematics, zero copies.

```csharp signature
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
    public static readonly KernelKind Wendland = new(key: 0, derivativeSupremum: 135.0 / 64.0, polynomialOrder: 0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => Pow1(q: q, power: 4) * (1.0 + (4.0 * q)), first: static (q, r) => ((-20.0 * q) + (60.0 * q * q) - (60.0 * q * q * q) + (20.0 * q * q * q * q)) / r, second: static (q, r) => (-20.0 + (120.0 * q) - (180.0 * q * q) + (80.0 * q * q * q)) / (r * r)));
    public static readonly KernelKind Quintic = new(key: 1, derivativeSupremum: 5.0, polynomialOrder: 0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => Pow1(q: q, power: 5), first: static (q, r) => -5.0 * Pow1(q: q, power: 4) / r, second: static (q, r) => 20.0 * Pow1(q: q, power: 3) / (r * r)));
    public static readonly KernelKind Cosine = new(key: 2, derivativeSupremum: Math.PI / 2.0, polynomialOrder: 0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => 0.5 * (1.0 + Math.Cos(d: Math.PI * q)), first: static (q, r) => -0.5 * Math.PI * Math.Sin(a: Math.PI * q) / r, second: static (q, r) => -0.5 * Math.PI * Math.PI * Math.Cos(d: Math.PI * q) / (r * r)));
    public static readonly KernelKind Cubic = new(key: 3, derivativeSupremum: 3.0, polynomialOrder: 0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => Pow1(q: q, power: 3), first: static (q, r) => -3.0 * Pow1(q: q, power: 2) / r, second: static (q, r) => 6.0 * (1.0 - q) / (r * r)));
    public static readonly KernelKind Linear = new(key: 4, derivativeSupremum: 1.0, polynomialOrder: 0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => 1.0 - q, first: static (_, r) => -1.0 / r, second: static (_, _) => 0.0));
    public static readonly KernelKind Epanechnikov = new(key: 5, derivativeSupremum: 2.0, polynomialOrder: 0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => 1.0 - (q * q), first: static (q, r) => -2.0 * q / r, second: static (_, r) => -2.0 / (r * r)));
    // Band-limited reconstruction rows — the sinc/jinc family every resampling tap table re-derives. Value and both
    // derivatives evaluate at 106-bit through the ddouble cardinal ladder and narrow ONCE at the row edge: the
    // (cos − sinc)/x near-zero cancellation that bars a double closed form is exactly what ddouble absorbs.
    public static readonly KernelKind Lanczos = new(key: 6, derivativeSupremum: 2.8097867788012820, polynomialOrder: 0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false,
        value: static (q, _) => (double)(Sinc(x: 2.0 * q) * Sinc(x: q)),
        first: static (q, r) => (double)((2.0 * SincPrime(x: 2.0 * q) * Sinc(x: q)) + (Sinc(x: 2.0 * q) * SincPrime(x: q))) / r,
        second: static (q, r) => (double)((4.0 * SincSecond(x: 2.0 * q) * Sinc(x: q)) + (4.0 * SincPrime(x: 2.0 * q) * SincPrime(x: q)) + (Sinc(x: 2.0 * q) * SincSecond(x: q))) / (r * r)));
    public static readonly KernelKind Jinc = new(key: 7, derivativeSupremum: 1.3791295785936520, polynomialOrder: 0, evaluate: static (d, r) => SupportProfile(distance: d, radius: r, nonsmoothAtOrigin: false,
        value: static (q, _) => q <= EpsilonPolicy.SqrtEpsilon ? 1.0 : (double)(2.0 * ddouble.BesselJ(1, (ddouble)(BesselFirstZero * q)) / (ddouble)(BesselFirstZero * q)),
        first: static (q, r) => q <= EpsilonPolicy.SqrtEpsilon ? -BesselFirstZero * BesselFirstZero * q / (4.0 * r) : (double)(-2.0 * ddouble.BesselJ(2, (ddouble)(BesselFirstZero * q)) / (ddouble)q) / r,
        second: static (q, r) => q <= EpsilonPolicy.SqrtEpsilon
            ? -BesselFirstZero * BesselFirstZero / (4.0 * r * r)
            : (double)(-(ddouble)(BesselFirstZero * BesselFirstZero) * ((((ddouble.BesselJ(1, (ddouble)(BesselFirstZero * q)) - ddouble.BesselJ(3, (ddouble)(BesselFirstZero * q))) * (ddouble)(BesselFirstZero * q)) - (2.0 * ddouble.BesselJ(2, (ddouble)(BesselFirstZero * q)))) / ((ddouble)(BesselFirstZero * q) * (ddouble)(BesselFirstZero * q)))) / (r * r)));
    // Globally-supported and conditionally-positive-definite RBF bases — no support clamp, so these rows evaluate
    // through GlobalProfile; PolynomialOrder is the reproduction-tail degree+1 the augmented design [Φ P; Pᵀ 0]
    // appends for conditional positive definiteness — 0 spells an unconditional basis.
    public static readonly KernelKind Gaussian = new(key: 8, derivativeSupremum: 0.8577638849607068, polynomialOrder: 0, evaluate: static (d, r) => GlobalProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => Math.Exp(d: -(q * q)), first: static (q, r) => -2.0 * q * Math.Exp(d: -(q * q)) / r, second: static (q, r) => ((4.0 * q * q) - 2.0) * Math.Exp(d: -(q * q)) / (r * r)));
    public static readonly KernelKind Multiquadric = new(key: 9, derivativeSupremum: 1.0, polynomialOrder: 1, evaluate: static (d, r) => GlobalProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => Math.Sqrt(d: 1.0 + (q * q)), first: static (q, r) => q / (r * Math.Sqrt(d: 1.0 + (q * q))), second: static (q, r) => 1.0 / (Math.Pow(x: 1.0 + (q * q), y: 1.5) * r * r)));
    public static readonly KernelKind InverseMultiquadric = new(key: 10, derivativeSupremum: 0.3849001794597505, polynomialOrder: 0, evaluate: static (d, r) => GlobalProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => 1.0 / Math.Sqrt(d: 1.0 + (q * q)), first: static (q, r) => -q / (Math.Pow(x: 1.0 + (q * q), y: 1.5) * r), second: static (q, r) => ((2.0 * q * q) - 1.0) / (Math.Pow(x: 1.0 + (q * q), y: 2.5) * r * r)));
    public static readonly KernelKind PolyharmonicCubic = new(key: 11, derivativeSupremum: double.PositiveInfinity, polynomialOrder: 2, evaluate: static (d, r) => GlobalProfile(distance: d, radius: r, nonsmoothAtOrigin: false, value: static (q, _) => q * q * q, first: static (q, r) => 3.0 * q * q / r, second: static (q, r) => 6.0 * q / (r * r)));
    public static readonly KernelKind ThinPlateSpline = new(key: 12, derivativeSupremum: double.PositiveInfinity, polynomialOrder: 2, evaluate: static (d, r) => GlobalProfile(distance: d, radius: r, nonsmoothAtOrigin: true, value: static (q, _) => q <= EpsilonPolicy.ZeroTolerance ? 0.0 : q * q * Math.Log(d: q), first: static (q, r) => q <= EpsilonPolicy.ZeroTolerance ? 0.0 : q * ((2.0 * Math.Log(d: q)) + 1.0) / r, second: static (q, r) => q <= EpsilonPolicy.ZeroTolerance ? 0.0 : ((2.0 * Math.Log(d: q)) + 3.0) / (r * r)));
    // DerivativeSupremum = sup_q|value′(q)|, the slope-bound numerator; compact rows bound on [0,1] (Wendland peaks at
    // q=1/4, odd-power kernels at the origin, cosine at q=1/2), global rows on [0,∞) — the polyharmonic pair carries
    // PositiveInfinity because no tolerance-free bound exists, and Falloff.SlopeBound degrades to None through it.
    public double DerivativeSupremum { get; }
    // Reproduction-tail order for the conditionally-positive-definite bases: BuildRbf appends the degree-(order-1)
    // polynomial block exactly when this column is nonzero.
    public int PolynomialOrder { get; }
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
    // First zero of J1 — the jinc row's support normalization, so value(1) is exactly the reconstruction null.
    private const double BesselFirstZero = 3.8317059702075123;
    // 106-bit cardinal-sine ladder: sinc'(x) = (cos(πx) − sinc(x))/x and sinc''(x) = −π²·sinc(x) − 2·sinc'(x)/x,
    // with the x→0 limits −π²x/3 and −π²/3 closing the removable singularity before the one narrowing to double.
    private static ddouble Sinc(double x) => ddouble.Sinc((ddouble)x, normalized: true);
    private static ddouble SincPrime(double x) => Math.Abs(value: x) <= EpsilonPolicy.SqrtEpsilon
        ? (ddouble)(-Math.PI * Math.PI / 3.0) * (ddouble)x
        : (ddouble.CosPi((ddouble)x) - Sinc(x: x)) / (ddouble)x;
    private static ddouble SincSecond(double x) =>
        (-(ddouble)(Math.PI * Math.PI) * Sinc(x: x)) - (2.0 * (Math.Abs(value: x) <= EpsilonPolicy.SqrtEpsilon
            ? (ddouble)(-Math.PI * Math.PI / 3.0)
            : SincPrime(x: x) / (ddouble)x));
    // Global twin of SupportProfile — no clamp, no boundary band: a globally-supported basis is finite everywhere.
    private static KernelProfile GlobalProfile(double distance, double radius, bool nonsmoothAtOrigin, Func<double, double, double> value, Func<double, double, double> first, Func<double, double, double> second) {
        double q = distance / radius;
        return new KernelProfile(Value: value(arg1: q, arg2: radius), FirstDerivative: first(arg1: q, arg2: radius), SecondDerivative: second(arg1: q, arg2: radius),
            Status: nonsmoothAtOrigin && q <= EpsilonPolicy.SqrtEpsilon ? KernelProfileStatus.NonsmoothOrigin : KernelProfileStatus.Smooth);
    }
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
    // Band-limited interpolating weight — sinc is 1 at the sample and 0 at every integer lattice offset, the one
    // MLS window that interpolates at bounded support without the singular row's pole.
    public static readonly WeightKernelFamily Lanczos = new(key: 5, interpolating: true, profile: static t => (double)(ddouble.Sinc((ddouble)(2.0 * t), normalized: true) * ddouble.Sinc((ddouble)t, normalized: true)));
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
    public sealed record MetricCase : Falloff { internal MetricCase(KernelKind Kind, Func<Point3d, Fin<SymmetricMatrix>> Metric, PositiveMagnitude Radius) { this.Kind = Kind; this.Metric = Metric; this.Radius = Radius; } public KernelKind Kind { get; } public Func<Point3d, Fin<SymmetricMatrix>> Metric { get; } public PositiveMagnitude Radius { get; } public override Option<double> SlopeBound => None; }

    // None where no tolerance-free bound exists: inverse laws steepen toward the degeneracy gate, the sampled metric's spectral radius unbounded.
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
                // Zero offset (query at source) is legal: quadratic 0 -> distance 0 -> kernel max; the
                // -ZeroTolerance band absorbs rounding of tiny offsets under an SPD-proven tensor.
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
    private static bool SpdByMinors(SymmetricMatrix tensor) {
        double a = tensor.At(i: 0, j: 0), b = tensor.At(i: 0, j: 1), c = tensor.At(i: 0, j: 2);
        double d = tensor.At(i: 1, j: 1), e = tensor.At(i: 1, j: 2), f = tensor.At(i: 2, j: 2);
        double det2 = (a * d) - (b * b);
        return a > 0.0 && det2 > 0.0 && (det2 * f) - (a * e * e) + (2.0 * b * c * e) - (d * c * c) > 0.0;
    }
}
```

## [04]-[NOISE_LATTICES]

- Owner: `FieldNoise` the `internal static` procedural-noise owner — classic Perlin gradient noise over the canonical permutation table, the 3D simplex lattice and its skew-transformed variant with optional two-tap smoothing, and Worley cellular noise, all over one hashed lattice substrate.
- Cases: Perlin, simplex, and Worley lattices over one `Perm`/`HashCell` substrate; `SkewedSimplexAt` carries the smooth flag and `SimplexAt` is the private skew-domain kernel both simplex modes ride.
- Entry: every lattice takes `(point, seed, frequency)`, deterministic for a given triple so noise-driven fields replay across processes; octave, persistence, and lacunarity admission is the consumer's policy through `Admit.NoiseInput`, the lattice itself total over finite input.
- Auto: `Perm(x, seed)` folds the seed into the table lookup so a seed relabels the lattice without a table copy; Worley hashes three decorrelated channels for its per-cell feature point.
- Receipt: none — pure deterministic functions.
- Packages: BCL only (`Math.Floor`, integer bit ops), RhinoCommon `Point3d` as the coordinate carrier.
- Growth: a new lattice is one member over the `Perm`/`HashCell` substrate; fractal octave sums (fBm, turbulence) are the consumer's fold over these single-octave taps, `Spatial/fields` owning the octave policy.
- Boundary: `PermTable` is the canonical published Perlin permutation, the one sanctioned literal table on this page; the noise vocabulary — `NoiseKind` rows with caution flags and sampler columns — is `Spatial/fields`', this page owning only the lattice mathematics those rows point at. `Rasm.Materials` `Appearance/texture#TEXTURE_UV` `ProceduralNoise` is a DELIBERATE second lattice family, split on differentiability-vs-parity: this owner hashes the canonical published permutation feeding `NoiseKind.ContinuouslyDifferentiable` (the `CurlNoise` admission gate and the `ScalarField.LipschitzBound` fold), while the Materials family holds FastNoiseLite byte-exactness for MaterialX category parity and the WGSL `f32` wrap law, with 2D arms, periodic-by-construction cell-index lattices, and the cellular return set this floor never needs — collapsing either end breaks the other's gating [branch RULINGS `[03]-[COLLAPSE]`].

```csharp signature
// --- [OPERATIONS] -------------------------------------------------------------------------
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
                             // Three INDEPENDENT per-axis jitter draws — each channel hashes the CELL under its
                             // own seed offset. The prior chain (hashY = Perm(hashX + 17), hashZ = Perm(hashY + 31))
                             // made y a function of x's hash and z of y's through one table, so the "decorrelated"
                             // feature-point jitter axes were correlated by construction.
                             let hashX = Perm(x: Perm(x: Perm(x: nx & 0xFF, seed: seed) + (ny & 0xFF), seed: seed) + (nz & 0xFF), seed: seed)
                             let hashY = Perm(x: Perm(x: Perm(x: nx & 0xFF, seed: seed + 17) + (ny & 0xFF), seed: seed + 17) + (nz & 0xFF), seed: seed + 17)
                             let hashZ = Perm(x: Perm(x: Perm(x: nx & 0xFF, seed: seed + 31) + (ny & 0xFF), seed: seed + 31) + (nz & 0xFF), seed: seed + 31)
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

## [05]-[SOLAR_EPHEMERIS]

- Owner: `SolarSite` the validated geodetic site; `SunPosition` the apparent azimuth/altitude result with its derived zenith, horizon predicate, and the survey-frame direction bijection `Direction`/`OfDirection`; `SolarPosition` the NOAA/Meeus closed-form apparent-solar fold — the branch's ONE solar ephemeris, every consumer a projection over it.
- Entry: `At(site, instant)` derives apparent azimuth/altitude — quadratic mean longitude, nutation-corrected ecliptic longitude, the full nested obliquity expression, and elevation-derived pressure-corrected refraction; `SunPath(site, midnight, step, samples)` samples that same total function across a day.
- Auto: the fold is total and effect-free — closed-form astronomy over finite admitted input carries no `Fin` rail; `SolarSite` gates latitude, longitude, timezone, and elevation once at admission.
- Receipt: none — pure deterministic functions; sweep evidence is the composing analysis' own.
- Packages: NodaTime (`Instant`/`Duration`/`NodaConstants` — the clock carrier; a `DateTime`-taking overload is the deleted form), Thinktecture.Runtime.Extensions (`[ComplexValueObject]`), LanguageExt.Core (`Seq`, `Option`), RhinoCommon (`Vector3d` — the kernel's ONE coordinate, `Numerics/atoms#VECTOR_ALGEBRA`'s carrier, whose `IsValid` screens the host unset sentinel a raw finiteness probe admits), BCL inbox (`Math`).
- Growth: an accuracy refinement (full SPA periodic-term tables over the truncated form) is a body change on the same two entries; a new consumer composes `At`/`SunPath`, never a duplicate almanac; zero new surface.
- Boundary: consumers project the ANGLES into their own world frame — `Rasm.Compute` `Analysis/daylight` folds them into its float clash coordinate at one `SurveyRay` narrowing, `Rasm.Materials` `Appearance/environment#SKY_MODEL` projects azimuth/altitude onto its `+X`-north `WorldDirection`, and `Rasm.AppUi` `Render/pathtrace#LIGHT_RIG` seats the angles on its Sun row — so the frame convention lives at each consuming edge and the almanac states angles alone; the geodetic datum, site CRS, and any reprojection stay the app-root edge's. Consumers holding a VECTOR rotate it into the survey frame at their own edge and re-read through `OfDirection` — `Rasm.Rhino` `Render/settings#SUN_ASTRONOMY` folds the host's north bearing and its sun-toward-scene sign there — so the sign and the north datum resolve where the producing frame is still known, never inside this almanac. `Direction`/`OfDirection` close on double throughout and the round trip holds `5.7e-14°` azimuth and `7.4e-13°` altitude across the whole sphere, matching the accuracy the fold is graded against; a single-precision carrier floors that inverse at `3.9e-6°` azimuth and `1.1e-3°` altitude, so a float engine takes the narrowing at ITS edge and the bijection keeps its digits.

```csharp signature
// --- [MODELS] -----------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class SolarSite {
    public double LatitudeDeg { get; }
    public double LongitudeDeg { get; }
    public double TimezoneHours { get; }
    public double ElevationM { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double latitudeDeg, ref double longitudeDeg,
        ref double timezoneHours, ref double elevationM) =>
        validationError = double.IsFinite(latitudeDeg) && latitudeDeg is >= -90.0 and <= 90.0
            && double.IsFinite(longitudeDeg) && longitudeDeg is >= -180.0 and <= 180.0
            && double.IsFinite(timezoneHours) && timezoneHours is >= -14.0 and <= 14.0
            && double.IsFinite(elevationM) && elevationM is > -500.0 and <= 10000.0
                ? null
                : new ValidationError(message: "<solar-site-invalid>");
}

// Azimuth from north clockwise (the survey convention); zenith and below-horizon derive, never stored.
public readonly record struct SunPosition(double AzimuthDeg, double AltitudeDeg) {
    public double ZenithDeg => 90.0 - AltitudeDeg;
    public bool AboveHorizon => AltitudeDeg > 0.0;

    // Unit sun direction in the +Y-north/+X-east SURVEY frame — the EPW/scene convention daylight shadow rays
    // cast toward, carried in the kernel's own double coordinate so `OfDirection` inverts it at the accuracy this
    // fold solves to; a +X-north consumer (the Materials environment frame) projects AzimuthDeg/AltitudeDeg
    // itself, and a float ray engine narrows at its own edge, where the lost digits are the traversal's.
    public Vector3d Direction {
        get {
            double alt = AltitudeDeg * Math.PI / 180.0, az = AzimuthDeg * Math.PI / 180.0;
            return new Vector3d(Math.Cos(alt) * Math.Sin(az), Math.Cos(alt) * Math.Cos(az), Math.Sin(alt));
        }
    }

    // `Direction` inverted across the same survey frame: a ray already pointing scene-toward-sun re-reads as the
    // angle pair, so a host publishing a vector converts ONCE here and no consuming edge re-derives `atan2` under
    // its own sign guess. Absence answers a ray carrying no direction — zero, non-finite, or the host
    // `RhinoMath.UnsetValue` sentinel whose finite magnitude a bare `double.IsFinite` probe reads as an ordinary
    // coordinate — because its `0°`/`0°` reading is a due-north horizon sun no consumer distinguishes from a
    // measured one.
    public static Option<SunPosition> OfDirection(Vector3d direction) =>
        direction.Length switch {
            double length when direction.IsValid && length > 0.0 => Some(OfUnit(direction / length)),
            _ => None,
        };

    // Azimuth wraps into `[0, 360)` and altitude clamps the `asin` domain against the round-off a unitized ray
    // carries at the zenith, where `Z` overshoots one by an ulp and the domain fault reads as a NaN altitude.
    static SunPosition OfUnit(Vector3d unit) =>
        new(AzimuthDeg: SolarPosition.Wrap360(Math.Atan2(unit.X, unit.Y) * 180.0 / Math.PI),
            AltitudeDeg: Math.Asin(Math.Clamp(unit.Z, -1.0, 1.0)) * 180.0 / Math.PI);
}

// --- [OPERATIONS] -------------------------------------------------------------------------
// Apparent-solar closed form at the branch's one truncation order: nutation on the ecliptic longitude, quadratic
// mean longitude, nested obliquity, and pressure-corrected refraction — the strongest of the two folds this owner
// collapsed, so every consumer reads one answer for one instant. The truncation holds arc-minute apparent
// position across the four centuries around J2000 and degrades outside; refraction dominates the error budget
// inside that span (worst near the horizon), which is why site elevation is a parameter and a higher-order
// ephemeris term is not.
public static class SolarPosition {
    public static SunPosition At(SolarSite site, Instant instant) {
        double jd = 2440587.5 + instant.ToUnixTimeTicks() / (double)NodaConstants.TicksPerDay;
        double t = (jd - 2451545.0) / 36525.0;
        double meanLongitude = Wrap360(280.46646 + t * (36000.76983 + t * 0.0003032));
        double meanAnomaly = (357.52911 + t * (35999.05029 - 0.0001537 * t)) * Math.PI / 180.0;
        double center = Math.Sin(meanAnomaly) * (1.914602 - t * (0.004817 + 0.000014 * t))
            + Math.Sin(2.0 * meanAnomaly) * (0.019993 - 0.000101 * t)
            + Math.Sin(3.0 * meanAnomaly) * 0.000289;
        double eclipticLongitude = (meanLongitude + center - 0.00569 - 0.00478 * Math.Sin((125.04 - 1934.136 * t) * Math.PI / 180.0)) * Math.PI / 180.0;
        double obliquity = (23.0 + (26.0 + (21.448 - t * (46.815 + t * (0.00059 - t * 0.001813))) / 60.0) / 60.0) * Math.PI / 180.0;
        double declination = Math.Asin(Math.Sin(obliquity) * Math.Sin(eclipticLongitude));
        double y = Math.Tan(obliquity / 2.0) * Math.Tan(obliquity / 2.0);
        double meanLonRad = meanLongitude * Math.PI / 180.0;
        double equationOfTime = 4.0 * (180.0 / Math.PI) * (
            y * Math.Sin(2.0 * meanLonRad) - 2.0 * 0.016708634 * Math.Sin(meanAnomaly)
            + 4.0 * 0.016708634 * y * Math.Sin(meanAnomaly) * Math.Cos(2.0 * meanLonRad)
            - 0.5 * y * y * Math.Sin(4.0 * meanLonRad) - 1.25 * 0.016708634 * 0.016708634 * Math.Sin(2.0 * meanAnomaly));
        double fractionalDay = jd - Math.Floor(jd) - 0.5 + site.TimezoneHours / 24.0;
        double trueSolarMinutes = Wrap(fractionalDay * 1440.0 + equationOfTime + 4.0 * site.LongitudeDeg - 60.0 * site.TimezoneHours, 1440.0);
        double hourAngle = ((trueSolarMinutes / 4.0) - 180.0) * Math.PI / 180.0;
        double phi = site.LatitudeDeg * Math.PI / 180.0;
        double altitude = Math.Asin(Math.Sin(phi) * Math.Sin(declination) + Math.Cos(phi) * Math.Cos(declination) * Math.Cos(hourAngle));
        double azimuth = Math.Atan2(Math.Sin(hourAngle), Math.Cos(hourAngle) * Math.Sin(phi) - Math.Tan(declination) * Math.Cos(phi));
        double altitudeDeg = altitude * 180.0 / Math.PI;
        double pressureRatio = Math.Pow(1.0 - 2.25577e-5 * Math.Max(site.ElevationM, -500.0), 5.25588);
        double refractionDeg = altitudeDeg is > -1.0 and < 90.0
            ? pressureRatio * 1.02 / Math.Tan((altitudeDeg + 10.3 / (altitudeDeg + 5.11)) * Math.PI / 180.0) / 60.0
            : 0.0;
        return new SunPosition(Wrap360(azimuth * 180.0 / Math.PI + 180.0), altitudeDeg + refractionDeg);
    }

    // One day's positions at the policy step — the sun-hours sweep, the viewport sun-path arc, and the sun-study
    // scrub all read this one sampler.
    public static Seq<(Instant Instant, SunPosition Sun)> SunPath(SolarSite site, Instant midnight, Duration step, int samples) =>
        toSeq(Enumerable.Range(0, samples)).Map(i => {
            Instant at = midnight + step * i;
            return (at, At(site, at));
        });

    internal static double Wrap360(double degrees) => Wrap(degrees, 360.0);
    static double Wrap(double value, double period) => value - period * Math.Floor(value / period);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

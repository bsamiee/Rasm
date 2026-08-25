# [RASM_NUMERICS_CALCULUS]

`Rasm.Numerics` calculus is the sample-anywhere analytic-math floor: differential operators, weight-profile mathematics, procedural noise lattices, and the geodetic solar almanac — the field operators generic over a sampler, the almanac closed-form over a site and an instant — so no field, mesh, or cloud type reaches this floor.

Every operator threads `Op` and gates finite input through the Domain validation vocabulary, so admission composes upstream and this floor carries the mathematics alone; the `SlopeBound`, `Slope`, and `DerivativeSupremum` slope evidence its kernels and profiles carry feeds the `Spatial/fields` Lipschitz fold downstream. Positive-definiteness is `Numerics/matrix`'s one verdict (`SymmetricMatrix.Definite`) and the non-zero-sum tap fold is `Numerics/transform`'s (`TapSeries.Convolve`); this page differentiates and weights, and re-spells neither.

## [01]-[INDEX]

- [02]-[DIFFERENTIAL_STENCIL]: `Nabla` sampler-generic central-difference stencil and the differential operators folding through its one `SampleAxes` traversal.
- [03]-[WEIGHT_PROFILES]: compact-support kernels, reconstruction weights, and the radial-decay `Falloff` union with its metric-sampler anisotropic case.
- [04]-[NOISE_LATTICES]: `FieldNoise` deterministic Perlin, simplex, and Worley lattices over one seed-folded permutation substrate.
- [05]-[SOLAR_EPHEMERIS]: `SolarPosition` the branch's one NOAA/Meeus apparent-solar fold over a validated `SolarSite` and a NodaTime `Instant`.

## [02]-[DIFFERENTIAL_STENCIL]

- Owner: `Nabla` the `static` differential-calculus owner; `SampleAxes` evaluates the six axis-offset samples `f(p ± ε·eᵢ)` through one traversal every first- and second-order operator composes; `LatticeAxes` is its TOTAL lattice twin — six taps by index with the border reflected through the one `Tap` reader — that `LatticeGradientAt`/`LatticeLaplacianAt`/`LatticeHessianAt` read.
- Cases: gradient, curl, curl-noise, divergence, Laplacian, and strain-magnitude over the shared stencil, with the periodic `ToroidalWrap`; the lattice arm carries gradient, Laplacian, and packed-upper Hessian over a `CellLattice`-addressed value span.
- Entry: every ambient operator takes `(sampler, point, eps, key)`; `eps` is the caller's scale-derived stencil width, gated finite and above `EpsilonPolicy.ZeroTolerance` — this floor never guesses a scale. Every lattice operator takes `(values, grid, column, row, layer)` — non-`Fin` and allocation-free, the spacing read off `CellLattice.CellSize` per axis so an anisotropic lattice differentiates true.
- Auto: every ambient operator shares the one `SampleAxes` traversal, and a failed tap short-circuits the rail with the sampler's own typed fault; every lattice operator is total on an ADMITTED pair — `Nabla.AdmitLattice` is that one gate, proving the value span against the census once so the loop below it carries no per-tap rail — `Tap` makes an out-of-census index read its mirror cell, and a rank-2 lattice degenerates the Z taps so one body serves both ranks.
- Exemption: the lattice arm is the page's one statement kernel — a texel or voxel plane can supply neither a `Func<Point3d, Fin<T>>` sampler nor one `Fin` allocation per tap, so the six-to-nineteen tap gathers stay index arithmetic under a declared exemption and the fence names the operator it refuses.
- Receipt: none — the operators are pure projections, evidence owned by the composing field or solver.
- Packages: LanguageExt.Core (`Fin`, query expressions), Rasm.Domain (`Op`, `Admit.Claims`), `Numerics/atoms` (`Reduce`, `CellLattice`, `EpsilonPolicy`), RhinoCommon (`Point3d`/`Vector3d` value structs).
- Growth: a new differential operator is one member over the `SampleAxes` stencil; a higher-order stencil is one alternative member the operators re-bind to, never a per-field re-implementation.
- Boundary: mesh-aware Laplacians over connectivity are `Meshing/mesh`'s, this page differentiating ambient ℝ³ samplers and `CellLattice`-addressed value spans alone; the lattice arm addresses and never stores — the value span is the consumer's, the lattice the `Numerics/atoms` owner. A ZERO-SUM tap series is a difference stencil and belongs here, a non-zero-sum one to `Numerics/transform#SPECTRAL`'s `TapSeries.Convolve`, which refuses a zero-sum series at its own mint — the two owners partition on the tap sum and neither carries the other's fold. `ToroidalWrap` is a total pure fold over an admitted strictly-positive period the Domain `Period` guard gates upstream, and it is the per-axis LIFT of `Numerics/atoms`' `Reduce.Centred` — the almanac's angular wrap reads that owner's floored twin, so the general reduction has one seat below both consumers instead of homing on whichever domain spelled one first.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using LanguageExt;
using Rasm.Domain;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Nabla {
    internal const double CurlDecorrelation2 = 137.0, CurlDecorrelation3 = -311.0;
    internal static Vector3d CurlOffset(double eps, double scale) =>
        new(x: scale * eps, y: scale * eps * 1.3, z: scale * eps * 0.7);
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
        from g2 in GradientAt(sampler: sampler, point: point + CurlOffset(eps: eps, scale: CurlDecorrelation2), eps: eps, key: key)
        from g3 in GradientAt(sampler: sampler, point: point + CurlOffset(eps: eps, scale: CurlDecorrelation3), eps: eps, key: key)
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
        new(x: Reduce.Centred(value: sample.X, period: period.X),
            y: Reduce.Centred(value: sample.Y, period: period.Y),
            z: Reduce.Centred(value: sample.Z, period: period.Z));

    // --- [LATTICE_STENCIL]
    public static Fin<Unit> AdmitLattice(ReadOnlySpan<double> values, CellLattice grid, Op key) =>
        Admit.Claims(key,
            (values.Length == grid.CellCount, "value-extent"),
            (grid.CellCount >= 1L, "lattice-census"));

    private static int Reflect(int index, int count) =>
        count is 1 ? 0 : index < 0 ? -index : index >= count ? (2 * count) - index - 2 : index;
    private static double Tap(ReadOnlySpan<double> values, CellLattice grid, int column, int row, int layer) =>
        values[(int)grid.Linear(
            column: Reflect(index: column, count: grid.Columns.Value),
            row: Reflect(index: row, count: grid.Rows.Value),
            layer: Reflect(index: layer, count: grid.Layers.Value))];
    public static (double X1, double X0, double Y1, double Y0, double Z1, double Z0) LatticeAxes(
        ReadOnlySpan<double> values, CellLattice grid, int column, int row, int layer = 0) =>
        (X1: Tap(values: values, grid: grid, column: column + 1, row: row, layer: layer),
         X0: Tap(values: values, grid: grid, column: column - 1, row: row, layer: layer),
         Y1: Tap(values: values, grid: grid, column: column, row: row + 1, layer: layer),
         Y0: Tap(values: values, grid: grid, column: column, row: row - 1, layer: layer),
         Z1: Tap(values: values, grid: grid, column: column, row: row, layer: layer + 1),
         Z0: Tap(values: values, grid: grid, column: column, row: row, layer: layer - 1));
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
        double center = Tap(values: values, grid: grid, column: column, row: row, layer: layer);
        Vector3d cell = grid.CellSize;
        double planar = ((x1 + x0 - (2.0 * center)) / (cell.X * cell.X)) + ((y1 + y0 - (2.0 * center)) / (cell.Y * cell.Y));
        return grid.Rank is 3 ? planar + ((z1 + z0 - (2.0 * center)) / (cell.Z * cell.Z)) : planar;
    }
    public static (double Xx, double Xy, double Xz, double Yy, double Yz, double Zz) LatticeHessianAt(
        ReadOnlySpan<double> values, CellLattice grid, int column, int row, int layer = 0) {
        (double x1, double x0, double y1, double y0, double z1, double z0) =
            LatticeAxes(values: values, grid: grid, column: column, row: row, layer: layer);
        double center = Tap(values: values, grid: grid, column: column, row: row, layer: layer);
        Vector3d cell = grid.CellSize;
        double xy = (Tap(values: values, grid: grid, column: column + 1, row: row + 1, layer: layer)
                   - Tap(values: values, grid: grid, column: column + 1, row: row - 1, layer: layer)
                   - Tap(values: values, grid: grid, column: column - 1, row: row + 1, layer: layer)
                   + Tap(values: values, grid: grid, column: column - 1, row: row - 1, layer: layer)) / (4.0 * cell.X * cell.Y);
        (double xz, double yz, double zz) = grid.Rank is 3
            ? (Xz: (Tap(values: values, grid: grid, column: column + 1, row: row, layer: layer + 1)
                  - Tap(values: values, grid: grid, column: column + 1, row: row, layer: layer - 1)
                  - Tap(values: values, grid: grid, column: column - 1, row: row, layer: layer + 1)
                  + Tap(values: values, grid: grid, column: column - 1, row: row, layer: layer - 1)) / (4.0 * cell.X * cell.Z),
               Yz: (Tap(values: values, grid: grid, column: column, row: row + 1, layer: layer + 1)
                  - Tap(values: values, grid: grid, column: column, row: row + 1, layer: layer - 1)
                  - Tap(values: values, grid: grid, column: column, row: row - 1, layer: layer + 1)
                  + Tap(values: values, grid: grid, column: column, row: row - 1, layer: layer - 1)) / (4.0 * cell.Y * cell.Z),
               Zz: (z1 + z0 - (2.0 * center)) / (cell.Z * cell.Z))
            : (Xz: 0.0, Yz: 0.0, Zz: 0.0);
        return (Xx: (x1 + x0 - (2.0 * center)) / (cell.X * cell.X), Xy: xy, Xz: xz,
                Yy: (y1 + y0 - (2.0 * center)) / (cell.Y * cell.Y), Yz: yz, Zz: zz);
    }
}
```

## [03]-[WEIGHT_PROFILES]

- Owner: `KernelProfile` carries value, first and second derivative, and a `KernelProfileStatus` smoothness verdict, so a consumer reads a kernel's derivative off the profile instead of re-differencing; `KernelSupport` is the two-row support regime carrying the dimensionless `Ceiling` a compact basis clamps at and a globally-supported one has none of; `KernelKind` mints the kernel bases in three bands — compact-support rows, band-limited `Lanczos`/`Jinc` reconstruction rows whose profiles evaluate at 106-bit through the `ddouble` cardinal ladder and narrow once, and globally-supported RBF rows — each row carrying its `Support` regime, its `Origin` status, `DerivativeSupremum`, its dimensionless slope-bound numerator, and `PolynomialOrder`, the reproduction-tail order the conditionally-positive-definite bases demand; `WeightKernelFamily` mints the reconstruction-weight profiles with the `Interpolating` column the MLS dispatches on; `Falloff` the radial-decay `[Union]` whose anisotropic case takes a `SymmetricMatrix` metric sampler driving the Mahalanobis distance.
- Cases: the `KernelProfileStatus` verdicts, the two `KernelSupport` regimes, the compact, band-limited, and global `KernelKind` rows, the `WeightKernelFamily` weights including the band-limited interpolating row, and the `Falloff` decay cases including the metric-sampler anisotropic one.
- Entry: `KernelKind.Profile(distance, radius, key)` returns the full gated profile and `Weight` the bare fast path; each of the three weight owners carries a SPAN arm beside its scalar one — `KernelKind.Weights`, `WeightKernelFamily.Weights`, `Falloff.Weights` — so a tap table or a design matrix fills in one call; `Falloff.Weight` discriminates bare distance, offset vector, and offset-plus-sample-point through one `WeightCore`, and `Falloff.Slope` is the LOCAL slope beside the family-wide `SlopeBound`.
- Auto: one `Profiled` body serves both support regimes — the row's `Ceiling` decides the clamp and its `Origin` the q→0 verdict, so neither a clamp flag nor a smoothness flag survives — banded on the dimensionless `q = d/r` so classification is scale-invariant with exact zeros outside support; the profile is gated by `Op.AcceptValue`, whose `IValidityEvidence` arm IS the finiteness proof, so the fold that hand-tested it deletes; the metric falloff proves the sampled tensor definite through `SymmetricMatrix.Definite`, so an indefinite metric fails typed instead of producing `√negative`.
- Law: `KernelProfile.FirstDerivative` and `KernelProfileStatus` are read by `Falloff.Slope` — the local Lipschitz bound `Spatial/fields` folds where the family-wide `SlopeBound` column answers `None` — and `SecondDerivative` by `Rasm.Compute` `Tensor/sampling#RECONSTRUCT`, which binds the row's value, both derivatives, `DerivativeSupremum`, and `PolynomialOrder` whole; no slot on the profile is unread.
- Receipt: `KernelProfile` is the per-evaluation receipt — value, both derivatives, and status — proving itself through the `IValidityEvidence` fold. `SpanProfile.Fill` is the ONE span-fill owner both profile families hand their row closure to; the guard, the q normalization, and the finiteness finalize have one seat.
- Packages: Thinktecture.Runtime.Extensions (`[UseDelegateFromConstructor]` columns, `[Union]`), LanguageExt.Core, System.Numerics.Tensors (`TensorPrimitives` — the span arms), `SymmetricMatrix` the metric carrier and its `Definite` verdict, TYoshimura.DoubleDouble (`ddouble.Sinc`/`CosPi`/`BesselJ` behind the band-limited rows), Rasm.Domain (`Op`, the `Admit.KernelInput`/`FalloffInput` gates, `AcceptValue`, the `AcceptValidated<TVO>` bridge).
- Growth: a new kernel is one `KernelKind` row with its `Shape` column, its support regime, and `DerivativeSupremum` — a closed-form supremum spells its derivation and a solved one states its solve; a new reconstruction weight is one `WeightKernelFamily` row; a new decay law is one `Falloff` case, one `WeightCore` arm, one `Slope` arm, and its `SlopeBound` column; a partial-support regime is one `KernelSupport` row.
- Boundary: `Spatial/fields` wraps `Falloff.Metric` over its `TensorField` by passing the tensor sampler, so the tensor-field type never appears here; `Meshing/reconstruct` composes `KernelKind` and `WeightKernelFamily` for its RBF, MLS, and Levin windows — one profile mathematics, zero copies. Positive-definiteness has ONE owner: `Numerics/matrix`'s `SymmetricMatrix.Definite` is the allocation-bounded verdict this page reads, and hand-rolled leading principal minors are the deleted form.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Numerics.Tensors;
using System.Runtime.InteropServices;
using DoubleDouble;
using LanguageExt;
using Rasm.Domain;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class KernelProfileStatus {
    public static readonly KernelProfileStatus Smooth = new(key: 0);
    public static readonly KernelProfileStatus SupportBoundary = new(key: 1);
    public static readonly KernelProfileStatus NonsmoothOrigin = new(key: 2);
    public static readonly KernelProfileStatus OutsideSupport = new(key: 3);
}

[SmartEnum<int>]
public sealed partial class KernelSupport {
    public static readonly KernelSupport Compact = new(key: 0, ceiling: Some(1.0));
    public static readonly KernelSupport Global = new(key: 1, ceiling: Option<double>.None);
    public Option<double> Ceiling { get; }
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct KernelProfile(double Value, double FirstDerivative, double SecondDerivative, KernelProfileStatus Status) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(value: Value), ValidityClaim.Finite(value: FirstDerivative), ValidityClaim.Finite(value: SecondDerivative), Status is not null);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class KernelKind {
    public static readonly KernelKind Wendland = new(key: 0, support: KernelSupport.Compact, origin: KernelProfileStatus.Smooth, derivativeSupremum: 135.0 / 64.0, polynomialOrder: 0,
        shape: static (q, r) => (Complement(q: q, power: 4) * (1.0 + (4.0 * q)), ((-20.0 * q) + (60.0 * q * q) - (60.0 * q * q * q) + (20.0 * q * q * q * q)) / r, (-20.0 + (120.0 * q) - (180.0 * q * q) + (80.0 * q * q * q)) / (r * r)));
    public static readonly KernelKind Quintic = new(key: 1, support: KernelSupport.Compact, origin: KernelProfileStatus.NonsmoothOrigin, derivativeSupremum: 5.0, polynomialOrder: 0,
        shape: static (q, r) => (Complement(q: q, power: 5), -5.0 * Complement(q: q, power: 4) / r, 20.0 * Complement(q: q, power: 3) / (r * r)));
    public static readonly KernelKind Cosine = new(key: 2, support: KernelSupport.Compact, origin: KernelProfileStatus.Smooth, derivativeSupremum: Math.PI / 2.0, polynomialOrder: 0,
        shape: static (q, r) => (0.5 * (1.0 + Math.Cos(d: Math.PI * q)), -0.5 * Math.PI * Math.Sin(a: Math.PI * q) / r, -0.5 * Math.PI * Math.PI * Math.Cos(d: Math.PI * q) / (r * r)));
    public static readonly KernelKind Cubic = new(key: 3, support: KernelSupport.Compact, origin: KernelProfileStatus.NonsmoothOrigin, derivativeSupremum: 3.0, polynomialOrder: 0,
        shape: static (q, r) => (Complement(q: q, power: 3), -3.0 * Complement(q: q, power: 2) / r, 6.0 * (1.0 - q) / (r * r)));
    public static readonly KernelKind Linear = new(key: 4, support: KernelSupport.Compact, origin: KernelProfileStatus.NonsmoothOrigin, derivativeSupremum: 1.0, polynomialOrder: 0,
        shape: static (q, r) => (1.0 - q, -1.0 / r, 0.0));
    public static readonly KernelKind Epanechnikov = new(key: 5, support: KernelSupport.Compact, origin: KernelProfileStatus.Smooth, derivativeSupremum: 2.0, polynomialOrder: 0,
        shape: static (q, r) => (1.0 - (q * q), -2.0 * q / r, -2.0 / (r * r)));
    public static readonly KernelKind Lanczos = new(key: 6, support: KernelSupport.Compact, origin: KernelProfileStatus.Smooth, derivativeSupremum: 2.8097867788012820, polynomialOrder: 0,
        shape: static (q, r) => (
            (double)(Sinc(x: 2.0 * q) * Sinc(x: q)),
            (double)((2.0 * SincPrime(x: 2.0 * q) * Sinc(x: q)) + (Sinc(x: 2.0 * q) * SincPrime(x: q))) / r,
            (double)((4.0 * SincSecond(x: 2.0 * q) * Sinc(x: q)) + (4.0 * SincPrime(x: 2.0 * q) * SincPrime(x: q)) + (Sinc(x: 2.0 * q) * SincSecond(x: q))) / (r * r)));
    public static readonly KernelKind Jinc = new(key: 7, support: KernelSupport.Compact, origin: KernelProfileStatus.Smooth, derivativeSupremum: 1.3791295785936520, polynomialOrder: 0,
        shape: static (q, r) => (
            (double)(2.0 * ddouble.Jinc((ddouble)(BesselFirstZero * q))),
            q <= EpsilonPolicy.SqrtEpsilon ? -BesselFirstZero * BesselFirstZero * q / (4.0 * r) : (double)(-2.0 * ddouble.BesselJ(2, (ddouble)(BesselFirstZero * q)) / (ddouble)q) / r,
            q <= EpsilonPolicy.SqrtEpsilon
                ? -BesselFirstZero * BesselFirstZero / (4.0 * r * r)
                : (double)(-(ddouble)(BesselFirstZero * BesselFirstZero) * ((((ddouble.BesselJ(1, (ddouble)(BesselFirstZero * q)) - ddouble.BesselJ(3, (ddouble)(BesselFirstZero * q))) * (ddouble)(BesselFirstZero * q)) - (2.0 * ddouble.BesselJ(2, (ddouble)(BesselFirstZero * q)))) / ((ddouble)(BesselFirstZero * q) * (ddouble)(BesselFirstZero * q)))) / (r * r)));
    public static readonly KernelKind Gaussian = new(key: 8, support: KernelSupport.Global, origin: KernelProfileStatus.Smooth, derivativeSupremum: GaussianSupremum, polynomialOrder: 0,
        shape: static (q, r) => (Math.Exp(d: -(q * q)), -2.0 * q * Math.Exp(d: -(q * q)) / r, ((4.0 * q * q) - 2.0) * Math.Exp(d: -(q * q)) / (r * r)));
    public static readonly KernelKind Multiquadric = new(key: 9, support: KernelSupport.Global, origin: KernelProfileStatus.Smooth, derivativeSupremum: 1.0, polynomialOrder: 1,
        shape: static (q, r) => (Math.Sqrt(d: 1.0 + (q * q)), q / (r * Math.Sqrt(d: 1.0 + (q * q))), 1.0 / (Math.Pow(x: 1.0 + (q * q), y: 1.5) * r * r)));
    public static readonly KernelKind InverseMultiquadric = new(key: 10, support: KernelSupport.Global, origin: KernelProfileStatus.Smooth, derivativeSupremum: InverseMultiquadricSupremum, polynomialOrder: 0,
        shape: static (q, r) => (1.0 / Math.Sqrt(d: 1.0 + (q * q)), -q / (Math.Pow(x: 1.0 + (q * q), y: 1.5) * r), ((2.0 * q * q) - 1.0) / (Math.Pow(x: 1.0 + (q * q), y: 2.5) * r * r)));
    public static readonly KernelKind PolyharmonicCubic = new(key: 11, support: KernelSupport.Global, origin: KernelProfileStatus.Smooth, derivativeSupremum: double.PositiveInfinity, polynomialOrder: 2,
        shape: static (q, r) => (q * q * q, 3.0 * q * q / r, 6.0 * q / (r * r)));
    public static readonly KernelKind ThinPlateSpline = new(key: 12, support: KernelSupport.Global, origin: KernelProfileStatus.NonsmoothOrigin, derivativeSupremum: double.PositiveInfinity, polynomialOrder: 2,
        shape: static (q, r) => q <= EpsilonPolicy.ZeroTolerance
            ? (0.0, 0.0, 0.0)
            : (q * q * Math.Log(d: q), q * ((2.0 * Math.Log(d: q)) + 1.0) / r, ((2.0 * Math.Log(d: q)) + 3.0) / (r * r)));

    public KernelSupport Support { get; }
    public KernelProfileStatus Origin { get; }
    public double DerivativeSupremum { get; }
    public int PolynomialOrder { get; }
    [UseDelegateFromConstructor] private partial (double Value, double First, double Second) Shape(double q, double radius);

    public Fin<KernelProfile> Profile(double distance, double radius, Op key) =>
        from _ in Admit.KernelInput(distance: distance, radius: radius, key: key)
        from profile in key.AcceptValue(value: Profiled(q: distance / radius, radius: radius))
        select profile;
    public double Weight(double distance, PositiveMagnitude radius) =>
        Profiled(q: Math.Max(val1: 0.0, val2: distance) / radius.Value, radius: radius.Value).Value;
    public Fin<Unit> Weights(ReadOnlySpan<double> distances, double radius, Span<double> destination, Op key) =>
        SpanProfile.Fill(distances: distances, scale: radius, destination: destination, row: q => Profiled(q: q, radius: radius).Value, key: key);

    private KernelProfile Profiled(double q, double radius) =>
        Support.Ceiling.Filter(edge => q > edge).IsSome
            ? new KernelProfile(Value: 0.0, FirstDerivative: 0.0, SecondDerivative: 0.0, Status: KernelProfileStatus.OutsideSupport)
            : Support.Ceiling.Filter(edge => Math.Abs(value: q - edge) <= EpsilonPolicy.SqrtEpsilon).IsSome
                ? new KernelProfile(Value: 0.0, FirstDerivative: 0.0, SecondDerivative: 0.0, Status: KernelProfileStatus.SupportBoundary)
                : Shape(q: q, radius: radius) switch {
                    (double value, double first, double second) => new KernelProfile(Value: value, FirstDerivative: first, SecondDerivative: second,
                        Status: q <= EpsilonPolicy.SqrtEpsilon ? Origin : KernelProfileStatus.Smooth),
                };
    private static double Complement(double q, int power) => Math.Pow(x: 1.0 - q, y: power);
    private static readonly double GaussianSupremum = Math.Sqrt(d: 2.0) * Math.Exp(d: -0.5);
    private static readonly double InverseMultiquadricSupremum = Math.Pow(x: 1.5, y: -1.5) / Math.Sqrt(d: 2.0);
    private const double BesselFirstZero = 3.8317059702075123;
    private static ddouble Sinc(double x) => ddouble.Sinc((ddouble)x, normalized: true);
    private static ddouble SincPrime(double x) => Math.Abs(value: x) <= EpsilonPolicy.SqrtEpsilon
        ? (ddouble)(-Math.PI * Math.PI / 3.0) * (ddouble)x
        : (ddouble.CosPi((ddouble)x) - Sinc(x: x)) / (ddouble)x;
    private static ddouble SincSecond(double x) =>
        (-(ddouble)(Math.PI * Math.PI) * Sinc(x: x)) - (2.0 * (Math.Abs(value: x) <= EpsilonPolicy.SqrtEpsilon
            ? (ddouble)(-Math.PI * Math.PI / 3.0)
            : SincPrime(x: x) / (ddouble)x));
}

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
    [UseDelegateFromConstructor] private partial double Profile(double t);
    public double Weight(double distance, PositiveMagnitude support) =>
        distance >= support.Value ? 0.0 : Profile(t: Math.Min(val1: Math.Max(val1: 0.0, val2: distance) / support.Value, val2: 1.0));
    public Fin<Unit> Weights(ReadOnlySpan<double> distances, double support, Span<double> destination, Op key) =>
        SpanProfile.Fill(distances: distances, scale: support, destination: destination, row: t => t >= 1.0 ? 0.0 : Profile(t: t), key: key);
}

internal static class SpanProfile {
    internal static Fin<Unit> Fill(ReadOnlySpan<double> distances, double scale, Span<double> destination, Func<double, double> row, Op key) {
        Fin<Unit> admitted = Admit.Claims(key,
            (distances.Length >= 1, "distance-extent"),
            (destination.Length >= distances.Length, "destination-extent"),
            (ValidityClaim.Positive(value: scale), "scale"),
            (ValidityClaim.Finite(values: distances), "distances-finite"));
        if (admitted.IsFail) { return admitted; }
        Span<double> lane = destination[..distances.Length];
        TensorPrimitives.Divide(distances, scale, lane);
        for (int i = 0; i < lane.Length; i++) { lane[i] = row(arg: lane[i]); }
        return TensorPrimitives.IsFiniteAll<double>(lane) ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: key.InvalidResult());
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Falloff {
    private Falloff() { }
    public sealed record ConstantCase : Falloff { internal ConstantCase() { } public override Option<double> SlopeBound => Some(0.0); }
    public sealed record PowerCase : Falloff { internal PowerCase(double Exponent) => this.Exponent = Exponent; public double Exponent { get; } public override Option<double> SlopeBound => None; }
    public sealed record GaussianCase : Falloff { internal GaussianCase(PositiveMagnitude Spread) => this.Spread = Spread; public PositiveMagnitude Spread { get; } public override Option<double> SlopeBound => Some(Math.Exp(-0.5) / Spread.Value); }
    public sealed record KernelCase : Falloff { internal KernelCase(KernelKind Kind, PositiveMagnitude Radius) { this.Kind = Kind; this.Radius = Radius; } public KernelKind Kind { get; } public PositiveMagnitude Radius { get; } public override Option<double> SlopeBound => Some(Kind.DerivativeSupremum / Radius.Value); }
    public sealed record MetricCase : Falloff { internal MetricCase(KernelKind Kind, Func<Point3d, Fin<SymmetricMatrix>> Metric, PositiveMagnitude Radius) { this.Kind = Kind; this.Metric = Metric; this.Radius = Radius; } public KernelKind Kind { get; } public Func<Point3d, Fin<SymmetricMatrix>> Metric { get; } public PositiveMagnitude Radius { get; } public override Option<double> SlopeBound => None; }

    public abstract Option<double> SlopeBound { get; }
    public static Falloff Constant => new ConstantCase();
    public static Fin<Falloff> Power(double exponent, Op key) =>
        key.AcceptValue(value: exponent).Map(static value => (Falloff)new PowerCase(Exponent: value));
    public static Falloff Inverse => new PowerCase(Exponent: -1.0);
    public static Falloff InverseSquare => new PowerCase(Exponent: -2.0);
    public static Fin<Falloff> Gaussian(double spread, Op key) =>
        key.AcceptValidated<PositiveMagnitude>(candidate: spread).Map(static value => (Falloff)new GaussianCase(Spread: value));
    public static Fin<Falloff> Kernel(KernelKind kind, double radius, Op key) =>
        from active in Optional(kind).ToFin(key.InvalidInput())
        from r in key.AcceptValidated<PositiveMagnitude>(candidate: radius)
        select (Falloff)new KernelCase(Kind: active, Radius: r);
    public static Fin<Falloff> Metric(KernelKind kind, Func<Point3d, Fin<SymmetricMatrix>> metric, double radius, Op key) =>
        from active in Optional(kind).ToFin(key.InvalidInput())
        from sampler in Optional(metric).ToFin(key.InvalidInput())
        from r in key.AcceptValidated<PositiveMagnitude>(candidate: radius)
        select (Falloff)new MetricCase(Kind: active, Metric: sampler, Radius: r);
    public Fin<double> Weight(double distance, double tolerance, Op key) =>
        WeightCore(distance: distance, distanceSquared: distance * distance, offset: Option<(Vector3d Offset, Point3d Sample)>.None, tolerance: tolerance, key: key);
    public Fin<double> Weight(Vector3d offset, Point3d sample, double tolerance, Op key) =>
        WeightCore(distance: offset.Length, distanceSquared: offset.SquareLength, offset: Some((Offset: offset, Sample: sample)), tolerance: tolerance, key: key);
    public Fin<double> Slope(double distance, double tolerance, Op key) =>
        Admit.FalloffInput(distance: distance, distanceSquared: distance * distance, tolerance: tolerance, key: key).Bind(_ => Switch(
            state: (Distance: distance, Tolerance: tolerance, Key: key),
            constantCase: static (_, _) => Fin.Succ(0.0),
            powerCase: static (s, p) => s.Distance > s.Tolerance
                ? s.Key.AcceptValue(value: Math.Abs(value: p.Exponent) * Math.Pow(x: s.Distance, y: p.Exponent - 1.0))
                : Fin.Fail<double>(s.Key.InvalidInput()),
            gaussianCase: static (s, g) => s.Key.AcceptValue(
                value: s.Distance * Math.Exp(d: -(s.Distance * s.Distance) / (2.0 * g.Spread.Value * g.Spread.Value)) / (g.Spread.Value * g.Spread.Value)),
            kernelCase: static (s, k) => k.Kind.Profile(distance: s.Distance, radius: k.Radius.Value, key: s.Key)
                .Map(static profile => profile.Status.Equals(KernelProfileStatus.OutsideSupport) ? 0.0 : Math.Abs(value: profile.FirstDerivative)),
            metricCase: static (s, _) => Fin.Fail<double>(s.Key.Unsupported(inputType: typeof(MetricCase), outputType: typeof(double)))));
    public Fin<Unit> Weights(ReadOnlySpan<double> distances, double tolerance, Span<double> destination, Op key) {
        if (!ValidityClaim.All(ValidityClaim.CountAtLeast(count: distances.Length, floor: 1),
                ValidityClaim.CountAtLeast(count: destination.Length, floor: distances.Length),
                ValidityClaim.Nonnegative(value: tolerance), ValidityClaim.Finite(values: distances))) {
            return Fin.Fail<Unit>(error: key.InvalidInput());
        }
        Span<double> lane = destination[..distances.Length];
        Fin<Unit> filled = Fin.Fail<Unit>(error: key.Unsupported(inputType: GetType(), outputType: typeof(Span<double>)));
        switch (this) {
            case ConstantCase: lane.Fill(1.0); filled = Fin.Succ(value: unit); break;
            case PowerCase power when TensorPrimitives.Min<double>(distances) > tolerance:
                TensorPrimitives.Pow(distances, power.Exponent, lane); filled = Fin.Succ(value: unit); break;
            case PowerCase: filled = Fin.Fail<Unit>(error: key.InvalidInput()); break;
            case GaussianCase gaussian:
                TensorPrimitives.Multiply(distances, distances, lane);
                TensorPrimitives.Multiply<double>(lane, -1.0 / (2.0 * gaussian.Spread.Value * gaussian.Spread.Value), lane);
                TensorPrimitives.Exp<double>(lane, lane); filled = Fin.Succ(value: unit); break;
            case KernelCase kernel:
                filled = kernel.Kind.Weights(distances: distances, radius: kernel.Radius.Value, destination: lane, key: key); break;
            case MetricCase:
                filled = Fin.Fail<Unit>(error: key.Unsupported(inputType: typeof(MetricCase), outputType: typeof(Span<double>))); break;
        }
        bool finite = TensorPrimitives.IsFiniteAll<double>(lane);
        return filled.Bind(_ => finite ? Fin.Succ(value: unit) : Fin.Fail<Unit>(error: key.InvalidResult()));
    }
    private Fin<double> WeightCore(double distance, double distanceSquared, Option<(Vector3d Offset, Point3d Sample)> offset, double tolerance, Op key) =>
        Admit.FalloffInput(distance: distance, distanceSquared: distanceSquared, tolerance: tolerance, key: key).Bind(_ => Switch(
            state: (Distance: distance, DistanceSquared: distanceSquared, Offset: offset, Tolerance: tolerance, Key: key),
            constantCase: static (_, _) => Fin.Succ(1.0),
            powerCase: static (s, p) => s.Distance > s.Tolerance
                ? s.Key.AcceptValue(value: Math.Pow(x: s.Distance, y: p.Exponent))
                : Fin.Fail<double>(s.Key.InvalidInput()),
            gaussianCase: static (s, g) => Fin.Succ(Math.Exp(-s.DistanceSquared / (2.0 * g.Spread.Value * g.Spread.Value))),
            kernelCase: static (s, k) => k.Kind.Profile(distance: s.Distance, radius: k.Radius.Value, key: s.Key).Map(static p => p.Value),
            metricCase: static (s, k) =>
                from m in s.Offset.ToFin(s.Key.Unsupported(inputType: typeof(MetricCase), outputType: typeof(double)))
                from tensor in k.Metric(arg: m.Sample)
                from _ in guard(tensor.Dimension.Value == 3, s.Key.InvalidInput()).ToFin()
                from definite in tensor.Definite(key: s.Key)
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
}
```

## [04]-[NOISE_LATTICES]

- Owner: `FieldNoise` the `internal static` procedural-noise owner — classic Perlin gradient noise over the canonical permutation table, the 3D simplex lattice and its skew-transformed variant with optional two-tap smoothing, and Worley cellular noise, all over one hashed lattice substrate.
- Cases: Perlin, simplex, and Worley lattices over one `Perm`/`HashCell` substrate; `SkewedSimplexAt` takes a `SimplexBlend` ROW carrying its own tap fold and `SimplexAt` is the private skew-domain kernel both blends ride.
- Entry: every lattice takes `(point, seed, frequency)`, deterministic for a given triple so noise-driven fields replay across processes; octave, persistence, and lacunarity admission is the consumer's policy through `Admit.NoiseInput`, the lattice itself total over finite input.
- Auto: `Perm(x, seed)` MIXES the seed through the branch's one splitmix64 owner, so a seed relabels the lattice rather than translating it — an additive fold makes `Perm(x, s+1)` and `Perm(x+1, s)` the same word — and Worley's three channels ride declared LANE ordinals, so the feature-point axes decorrelate instead of reading one stream at three fixed offsets.
- Exemption: `WorleyAt`'s 27-cell neighbourhood is a declared statement kernel — the running minimum accumulates over a per-texel hot path where a query fold allocates twenty-seven tuples and six bindings per sample.
- Receipt: none — pure deterministic functions.
- Packages: `Rasm.Domain` (`Deterministic.Stream` — the branch's ONE splitmix64 owner every kernel draw threads), Thinktecture.Runtime.Extensions (the `SimplexBlend` row and its tap column), BCL (`Math.Floor`, integer bit ops), RhinoCommon `Point3d` as the coordinate carrier.
- Growth: a new lattice is one member over the `Perm`/`HashCell` substrate; fractal octave sums (fBm, turbulence) are the consumer's fold over these single-octave taps, `Spatial/fields` owning the octave policy.
- Boundary: `PermTable` and `GradientTable` are the canonical published Perlin permutation and its twelve canonical edge vectors, the sanctioned literal tables on this page — a table with a defining publication transcribes verbatim, where an authored table with no defining sequence rides as a digest-pinned content-keyed asset. The noise VOCABULARY — `NoiseKind` rows and their `CapabilitySet<NoiseTrait>` columns — is `Spatial/fields`', this page owning only the lattice mathematics those rows point at. `Rasm.Materials` `Appearance/texture#TEXTURE_UV` `ProceduralNoise` is a DELIBERATE second lattice family, split on differentiability-vs-parity: this owner hashes the canonical published permutation feeding the `NoiseTrait.Differentiable` membership (the `CurlNoise` admission gate and the `ScalarField.LipschitzBound` fold), while the Materials family holds FastNoiseLite byte-exactness for MaterialX category parity and the WGSL `f32` wrap law, with 2D arms, periodic-by-construction cell-index lattices, and the cellular return set this floor never needs — collapsing either end breaks the other's gating [branch RULINGS `[03]-[COLLAPSE]`].

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using Rasm.Domain;
using Rhino.Geometry;
using Thinktecture;

namespace Rasm.Numerics;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class FieldNoise {
    private static readonly int[] PermTable = [
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103, 30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197, 62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125, 136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83, 111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54, 65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135, 130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124, 123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182, 189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101, 155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178, 185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162, 241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84, 204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72, 243, 141, 128, 195, 78, 66, 215, 61, 156, 180,
    ];
    private const double SkewF3 = 1.0 / 3.0;
    private const double UnskewG3 = 1.0 / 6.0;
    private const double SupportRadiusSquared = 0.6;
    private const double AmplitudeScale = 32.0;
    private const long JitterX = 0L, JitterY = 1L, JitterZ = 2L, SimplexPrimary = 0L, SimplexRotated = 1L;

    private static int Perm(int x, int seed) =>
        PermTable[(int)(Deterministic.Stream(lanes: [x, seed]) & 0xFF)];
    private static double Fade(double t) => t * t * t * ((t * ((t * 6) - 15)) + 10);
    private static double Lerp(double t, double a, double b) => a + (t * (b - a));
    private static ReadOnlySpan<sbyte> GradientTable =>
        [1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 0,
         1, 0, 1, -1, 0, 1, 1, 0, -1, -1, 0, -1,
         0, 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1];
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
    internal static double WorleyAt(Point3d point, int seed, double frequency) {
        double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
        int cx = (int)Math.Floor(d: px); int cy = (int)Math.Floor(d: py); int cz = (int)Math.Floor(d: pz);
        double nearest = double.PositiveInfinity;
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                for (int dz = -1; dz <= 1; dz++) {
                    int nx = cx + dx; int ny = cy + dy; int nz = cz + dz;
                    double ddx = nx + (Jitter(x: nx, y: ny, z: nz, seed: seed, channel: JitterX) / 255.0) - px;
                    double ddy = ny + (Jitter(x: nx, y: ny, z: nz, seed: seed, channel: JitterY) / 255.0) - py;
                    double ddz = nz + (Jitter(x: nx, y: ny, z: nz, seed: seed, channel: JitterZ) / 255.0) - pz;
                    nearest = Math.Min(val1: nearest, val2: (ddx * ddx) + (ddy * ddy) + (ddz * ddz));
                }
            }
        }
        return Math.Sqrt(d: nearest);
    }
    internal static double SkewedSimplexAt(Point3d point, int seed, double frequency, SimplexBlend blend) {
        double stretch = (point.X + point.Y + point.Z) * SkewF3;
        Point3d skewed = new(x: point.X + stretch, y: point.Y + stretch, z: point.Z + stretch);
        return blend.Blended(
            primary: SimplexAt(point: skewed, seed: seed, frequency: frequency, channel: SimplexPrimary),
            rotated: SimplexAt(point: new Point3d(x: skewed.Y, y: skewed.Z, z: skewed.X), seed: seed, frequency: frequency, channel: SimplexRotated));
    }
    private static double SimplexAt(Point3d point, int seed, double frequency, long channel) {
        double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
        int i = (int)Math.Floor(d: px); int j = (int)Math.Floor(d: py); int k = (int)Math.Floor(d: pz);
        double x0 = px - i; double y0 = py - j; double z0 = pz - k;
        (int i1, int j1, int k1, int i2, int j2, int k2) = x0 >= y0
            ? y0 >= z0 ? (1, 0, 0, 1, 1, 0) : x0 >= z0 ? (1, 0, 0, 1, 0, 1) : (0, 0, 1, 1, 0, 1)
            : y0 < z0 ? (0, 0, 1, 0, 1, 1) : x0 < z0 ? (0, 1, 0, 0, 1, 1) : (0, 1, 0, 1, 1, 0);
        double n0 = SimplexCorner(hash: HashCell(i: i, j: j, k: k, seed: seed, channel: channel), x: x0, y: y0, z: z0);
        double n1 = SimplexCorner(hash: HashCell(i: i + i1, j: j + j1, k: k + k1, seed: seed, channel: channel), x: x0 - i1 + UnskewG3, y: y0 - j1 + UnskewG3, z: z0 - k1 + UnskewG3);
        double n2 = SimplexCorner(hash: HashCell(i: i + i2, j: j + j2, k: k + k2, seed: seed, channel: channel), x: x0 - i2 + SkewF3, y: y0 - j2 + SkewF3, z: z0 - k2 + SkewF3);
        double n3 = SimplexCorner(hash: HashCell(i: i + 1, j: j + 1, k: k + 1, seed: seed, channel: channel), x: x0 - 0.5, y: y0 - 0.5, z: z0 - 0.5);
        return AmplitudeScale * (n0 + n1 + n2 + n3);
    }
    private static int Jitter(int x, int y, int z, int seed, long channel) =>
        (int)(Deterministic.Stream(lanes: [x, y, z, channel], seed: seed) & 0xFF);
    private static int HashCell(int i, int j, int k, int seed, long channel) => Jitter(x: i, y: j, z: k, seed: seed, channel: channel);
    private static double SimplexCorner(int hash, double x, double y, double z) {
        double t = SupportRadiusSquared - (x * x) - (y * y) - (z * z);
        return t <= 0.0 ? 0.0 : t * t * t * t * Grad(hash: hash, x: x, y: y, z: z);
    }
}

[SmartEnum<int>]
public sealed partial class SimplexBlend {
    public static readonly SimplexBlend Single = new(key: 0, blended: static (primary, _) => primary);
    public static readonly SimplexBlend Rotated = new(key: 1, blended: static (primary, rotated) => 0.5 * (primary + rotated));
    [UseDelegateFromConstructor] internal partial double Blended(double primary, double rotated);
}
```

## [05]-[SOLAR_EPHEMERIS]

- Owner: `SolarSite` the validated geodetic site; `SolarSeries` the published coefficient roster the fold evaluates by Horner; `SunPosition` the apparent azimuth/altitude result with its derived zenith, horizon predicate, and the survey-frame direction bijection `Direction`/`OfDirection`; `SolarPosition` the NOAA/Meeus closed-form apparent-solar fold — the branch's ONE solar ephemeris, every consumer a projection over it.
- Entry: `At(site, instant)` derives apparent azimuth/altitude — quadratic mean longitude, nutation-corrected ecliptic longitude, the full nested obliquity expression, and elevation-derived pressure-corrected refraction; `SunPath(site, midnight, step, samples)` samples that same total function across a day, `samples` admitted as `Dimension` so the sweep width is proved at construction and no public entry can raise on a negative count.
- Auto: the fold is total and effect-free — closed-form astronomy over finite admitted input carries no `Fin` rail; `SolarSite` ACCUMULATES its latitude and elevation clauses so a caller learns both, types its timezone as a NodaTime `Offset` whose own range forecloses the hand guard, and CANONICALIZES longitude into `[-180, 180)` by `ref`, so one meridian has one spelling and `179.9` and `-180.1` are not two values one wrap apart.
- Exemption: `At` is a declared statement kernel — one closed-form astronomical chain whose twenty intermediate terms each name a published quantity, and a query-expression spelling would rename every one of them. Every COEFFICIENT the chain reads is a `SolarSeries` row carrying its own chapter and equation, so a transcription error is visible at the roster rather than buried in an inline literal.
- Receipt: none — pure deterministic functions; sweep evidence is the composing analysis' own.
- Packages: NodaTime (`Instant`/`Duration`/`Offset`/`NodaConstants` — the clock carriers; a `DateTime`-taking overload is the deleted form), Thinktecture.Runtime.Extensions (`[ComplexValueObject]`, default `ValidationError`, `[SmartEnum<string>]`), LanguageExt.Core (`Seq`, `Option`, `Range`), `Numerics/atoms` (`Reduce.Floored`), RhinoCommon (`Vector3d` — the kernel's ONE coordinate, `Numerics/atoms#VECTOR_ALGEBRA`'s carrier, whose `IsValid` screens the host unset sentinel a raw finiteness probe admits), BCL inbox (`Math`).
- Growth: an accuracy refinement (full SPA periodic-term tables over the truncated form) is a body change on the same two entries; a new consumer composes `At`/`SunPath`, never a duplicate almanac; zero new surface.
- Boundary: consumers project the ANGLES into their own world frame — `Rasm.Compute` `Analysis/daylight` folds them into its float clash coordinate at one `SurveyRay` narrowing, `Rasm.Materials` `Appearance/environment#SKY_MODEL` projects azimuth/altitude onto its `+X`-north `WorldDirection`, and `Rasm.AppUi` `Render/pathtrace#LIGHT_RIG` seats the angles on its Sun row — so the frame convention lives at each consuming edge and the almanac states angles alone; the geodetic datum, site CRS, and any reprojection stay the app-root edge's. Consumers holding a VECTOR rotate it into the survey frame at their own edge and re-read through `OfDirection` — `Rasm.Rhino` `Render/settings#SUN_ASTRONOMY` folds the host's north bearing and its sun-toward-scene sign there — so the sign and the north datum resolve where the producing frame is still known, never inside this almanac. `Direction`/`OfDirection` close on double throughout and the round trip holds `5.7e-14°` azimuth and `7.4e-13°` altitude across the whole sphere, matching the accuracy the fold is graded against; a single-precision carrier floors that inverse at `3.9e-6°` azimuth and `1.1e-3°` altitude, so a float engine takes the narrowing at ITS edge and the bijection keeps its digits.

```csharp signature
// --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
using System;
using System.Collections.Immutable;
using LanguageExt;
using NodaTime;
using Rasm.Domain;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [MODELS] --------------------------------------------------------------------------
[ComplexValueObject]
public sealed partial class SolarSite {
    public double LatitudeDeg { get; }
    public double LongitudeDeg { get; }
    public Offset Timezone { get; }
    public double ElevationM { get; }

    public double TimezoneHours => Timezone.Seconds / (double)NodaConstants.SecondsPerHour;

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
}

public readonly record struct SunPosition(double AzimuthDeg, double AltitudeDeg) {
    public double ZenithDeg => 90.0 - AltitudeDeg;
    public bool AboveHorizon => AltitudeDeg > 0.0;

    public Vector3d Direction {
        get {
            double alt = AltitudeDeg * Math.PI / 180.0, az = AzimuthDeg * Math.PI / 180.0;
            return new Vector3d(Math.Cos(alt) * Math.Sin(az), Math.Cos(alt) * Math.Cos(az), Math.Sin(alt));
        }
    }

    public static Option<SunPosition> OfDirection(Vector3d direction) =>
        direction.Length switch {
            double length when direction.IsValid && length > 0.0 => Some(OfUnit(direction / length)),
            _ => None,
        };

    static SunPosition OfUnit(Vector3d unit) =>
        new(AzimuthDeg: SolarPosition.Wrap360(Math.Atan2(unit.X, unit.Y) * 180.0 / Math.PI),
            AltitudeDeg: Math.Asin(Math.Clamp(unit.Z, -1.0, 1.0)) * 180.0 / Math.PI);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SolarSeries {
    public static readonly SolarSeries MeanLongitude    = new("mean-longitude",    clause: "Meeus, Astronomical Algorithms 2nd ed., ch. 25 (25.2)", coefficients: [280.46646, 36000.76983, 0.0003032]);
    public static readonly SolarSeries MeanAnomaly      = new("mean-anomaly",      clause: "Meeus ch. 25 (25.3)",                                   coefficients: [357.52911, 35999.05029, -0.0001537]);
    public static readonly SolarSeries CentreFirst      = new("centre-first",      clause: "Meeus ch. 25, equation of centre, sin(M)",               coefficients: [1.914602, -0.004817, -0.000014]);
    public static readonly SolarSeries CentreSecond     = new("centre-second",     clause: "Meeus ch. 25, equation of centre, sin(2M)",              coefficients: [0.019993, -0.000101]);
    public static readonly SolarSeries CentreThird      = new("centre-third",      clause: "Meeus ch. 25, equation of centre, sin(3M)",              coefficients: [0.000289]);
    public static readonly SolarSeries Apparent         = new("apparent",          clause: "Meeus ch. 25, apparent longitude correction",            coefficients: [0.00569, 0.00478]);
    public static readonly SolarSeries NutationArgument = new("nutation-argument", clause: "Meeus ch. 22, omega argument",                           coefficients: [125.04, -1934.136]);
    public static readonly SolarSeries ObliquityBase    = new("obliquity-base",    clause: "Meeus ch. 22 (22.2), degree and arcminute base",         coefficients: [23.0, 26.0]);
    public static readonly SolarSeries Obliquity        = new("obliquity",         clause: "Meeus ch. 22 (22.2), arcsecond tail",                    coefficients: [21.448, -46.815, -0.00059, 0.001813]);
    public static readonly SolarSeries Refraction       = new("refraction",        clause: "Saemundsson/Bennett, Meeus ch. 16 (16.4)",               coefficients: [1.02, 10.3, 5.11]);

    public string Clause { get; }
    public ImmutableArray<double> Coefficients { get; }

    public double At(double t) {
        double accumulated = 0.0;
        for (int index = Coefficients.Length - 1; index >= 0; index--) { accumulated = Coefficients[index] + (t * accumulated); }
        return accumulated;
    }
    public double this[int index] => Coefficients[index];
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SolarPosition {
    private const double Eccentricity = 0.016708634;
    private const double LapseRate = 2.25577e-5;
    private const double LapseExponent = 5.25588;
    private const double JulianUnixEpoch = 2440587.5;
    private const double JulianJ2000 = 2451545.0;
    private const double JulianCentury = 36525.0;
    private const double Radians = Math.PI / 180.0;

    public static SunPosition At(SolarSite site, Instant instant) {
        double jd = JulianUnixEpoch + instant.ToUnixTimeTicks() / (double)NodaConstants.TicksPerDay;
        double t = (jd - JulianJ2000) / JulianCentury;
        double meanLongitude = Wrap360(SolarSeries.MeanLongitude.At(t: t));
        double meanAnomaly = SolarSeries.MeanAnomaly.At(t: t) * Radians;
        double center = (Math.Sin(meanAnomaly) * SolarSeries.CentreFirst.At(t: t))
            + (Math.Sin(2.0 * meanAnomaly) * SolarSeries.CentreSecond.At(t: t))
            + (Math.Sin(3.0 * meanAnomaly) * SolarSeries.CentreThird.At(t: t));
        double eclipticLongitude = (meanLongitude + center - SolarSeries.Apparent[0]
            - (SolarSeries.Apparent[1] * Math.Sin(SolarSeries.NutationArgument.At(t: t) * Radians))) * Radians;
        double obliquity = (SolarSeries.ObliquityBase[0]
            + ((SolarSeries.ObliquityBase[1] + (SolarSeries.Obliquity.At(t: t) / 60.0)) / 60.0)) * Radians;
        double declination = Math.Asin(Math.Sin(obliquity) * Math.Sin(eclipticLongitude));
        double y = Math.Tan(obliquity / 2.0) * Math.Tan(obliquity / 2.0);
        double meanLonRad = meanLongitude * Math.PI / 180.0;
        double equationOfTime = 4.0 * (180.0 / Math.PI) * (
            y * Math.Sin(2.0 * meanLonRad) - 2.0 * Eccentricity * Math.Sin(meanAnomaly)
            + 4.0 * Eccentricity * y * Math.Sin(meanAnomaly) * Math.Cos(2.0 * meanLonRad)
            - 0.5 * y * y * Math.Sin(4.0 * meanLonRad) - 1.25 * Eccentricity * Eccentricity * Math.Sin(2.0 * meanAnomaly));
        double fractionalDay = jd - Math.Floor(jd) - 0.5 + site.TimezoneHours / 24.0;
        double trueSolarMinutes = Reduce.Floored(value: (fractionalDay * 1440.0) + equationOfTime + (4.0 * site.LongitudeDeg) - (60.0 * site.TimezoneHours), period: 1440.0);
        double hourAngle = ((trueSolarMinutes / 4.0) - 180.0) * Math.PI / 180.0;
        double phi = site.LatitudeDeg * Math.PI / 180.0;
        double altitude = Math.Asin(Math.Clamp(value: (Math.Sin(phi) * Math.Sin(declination)) + (Math.Cos(phi) * Math.Cos(declination) * Math.Cos(hourAngle)), min: -1.0, max: 1.0));
        double azimuth = Math.Atan2(Math.Sin(hourAngle), Math.Cos(hourAngle) * Math.Sin(phi) - Math.Tan(declination) * Math.Cos(phi));
        double altitudeDeg = altitude * 180.0 / Math.PI;
        double pressureRatio = Math.Pow(1.0 - (LapseRate * site.ElevationM), LapseExponent);
        double refractionDeg = altitudeDeg is > -1.0 and < 90.0
            ? pressureRatio * SolarSeries.Refraction[0]
              / Math.Tan((altitudeDeg + (SolarSeries.Refraction[1] / (altitudeDeg + SolarSeries.Refraction[2]))) * Radians) / 60.0
            : 0.0;
        return new SunPosition(Wrap360(azimuth * 180.0 / Math.PI + 180.0), altitudeDeg + refractionDeg);
    }

    public static Seq<(Instant Instant, SunPosition Sun)> SunPath(SolarSite site, Instant midnight, Duration step, Dimension samples) =>
        Range(0, samples.Value).Map(i => {
            Instant at = midnight + step * i;
            return (at, At(site, at));
        }).ToSeq();

    internal static double Wrap360(double degrees) => Reduce.Floored(value: degrees, period: 360.0);
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)

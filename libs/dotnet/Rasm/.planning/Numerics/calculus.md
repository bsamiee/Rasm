# [RASM_NUMERICS_CALCULUS]

`Rasm.Numerics` calculus is the sample-anywhere analytic-math floor: differential operators, weight-profile mathematics, procedural noise lattices, and the geodetic solar almanac — the field operators generic over a sampler, the almanac closed-form over a site and an instant — so no field, mesh, or cloud type reaches this floor.

Every operator gates finite input through the Domain validation vocabulary, so admission composes upstream and this floor carries the mathematics alone; the `SlopeBound`, `Slope`, and `DerivativeSupremum` slope evidence its kernels and profiles carry feeds the `Spatial/fields` Lipschitz fold downstream. Positive-definiteness is `Numerics/matrix`'s one verdict (`SymmetricMatrix.Definite`) and the non-zero-sum tap fold is `Numerics/transform`'s (`TapSeries.Convolve`); this page differentiates and weights, and re-spells neither.

## [01]-[INDEX]

- [02]-[DIFFERENTIAL_STENCIL]: `Nabla` sampler-generic central-difference stencil and the differential operators folding through its one `SampleAxes` traversal.
- [03]-[WEIGHT_PROFILES]: compact-support kernels, reconstruction weights, and the radial-decay `Falloff` union with its metric-sampler anisotropic case.
- [04]-[NOISE_LATTICES]: `FieldNoise` deterministic Perlin, simplex, and Worley lattices over one coordinate-hashed lattice substrate.
- [05]-[SOLAR_EPHEMERIS]: `SunPosition.At` the branch's one NOAA/Meeus apparent-solar fold over a validated `SolarSite` and a NodaTime `Instant`.

## [02]-[DIFFERENTIAL_STENCIL]

- Owner: `Nabla` the `static` differential-calculus owner; `SampleAxes` evaluates the six axis-offset samples `f(p ± ε·eᵢ)` through one traversal every first- and second-order operator composes; `LatticeAxes` is its TOTAL lattice twin — six taps by index with the border reflected through the one `Tap` reader — that `LatticeGradientAt`/`LatticeLaplacianAt`/`LatticeHessianAt` read.
- Cases: gradient, curl, curl-noise, divergence, Laplacian, and strain-magnitude over the shared stencil; the lattice arm carries gradient, Laplacian, and packed-upper Hessian over a `CellLattice`-addressed value span.
- Entry: every ambient operator takes `(sampler, point, epsilon)`; `epsilon` is the caller's scale-derived `PositiveMagnitude`, admitted once before reaching this floor. Every lattice operator takes `(values, grid, column, row, layer)` — non-`Fin` and allocation-free, the spacing read off `CellLattice.CellSize` per axis so an anisotropic lattice differentiates true.
- Auto: every ambient operator shares the one `SampleAxes` traversal, accumulating all six independent sampler failures before returning; every lattice operator is total on its caller-owned span and admitted `CellLattice` — `Tap` makes an out-of-census index read its mirror cell, and a rank-2 lattice degenerates the Z taps so one body serves both ranks.
- Exemption: the lattice arm is the page's one statement kernel — a texel or voxel plane can supply neither a `Func<Point3d, Fin<T>>` sampler nor one `Fin` allocation per tap, so the six-to-nineteen tap gathers stay index arithmetic under a declared exemption and the fence names the operator it refuses.
- Packages: LanguageExt.Core (`Fin`, `Validation`, `Apply`, query expressions), Rasm.Domain (`Acceptance.Value`), `Numerics/atoms` (`PositiveMagnitude`, `CellLattice`), RhinoCommon (`Point3d`/`Vector3d` value structs).
- Growth: a new differential operator is one member over the `SampleAxes` stencil; a higher-order stencil is one alternative member the operators re-bind to, never a per-field re-implementation.
- Boundary: mesh-aware Laplacians over connectivity are `Meshing/mesh`'s, this page differentiating ambient ℝ³ samplers and `CellLattice`-addressed value spans alone; the lattice arm addresses and never stores — the value span is the consumer's, the lattice the `Numerics/atoms` owner. A ZERO-SUM tap series is a difference stencil and belongs here, a non-zero-sum one to `Numerics/transform#SPECTRAL`'s `TapSeries.Convolve`, which refuses a zero-sum series at its own mint — the two owners partition on the tap sum and neither carries the other's fold.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using LanguageExt;
using Rasm.Domain;
using Rhino.Geometry;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Nabla {
    private static Fin<(T X1, T X0, T Y1, T Y0, T Z1, T Z0)> SampleAxes<T>(Func<Point3d, Fin<T>> sampler, Point3d point, PositiveMagnitude epsilon) =>
        (sampler(point + (epsilon.Value * Vector3d.XAxis)).ToValidation(),
         sampler(point - (epsilon.Value * Vector3d.XAxis)).ToValidation(),
         sampler(point + (epsilon.Value * Vector3d.YAxis)).ToValidation(),
         sampler(point - (epsilon.Value * Vector3d.YAxis)).ToValidation(),
         sampler(point + (epsilon.Value * Vector3d.ZAxis)).ToValidation(),
         sampler(point - (epsilon.Value * Vector3d.ZAxis)).ToValidation())
            .Apply(static (xp, xm, yp, ym, zp, zm) => (X1: xp, X0: xm, Y1: yp, Y0: ym, Z1: zp, Z0: zm)).As().ToFin();
    public static Fin<Vector3d> GradientAt(Func<Point3d, Fin<double>> sampler, Point3d point, PositiveMagnitude epsilon) =>
        from samples in SampleAxes(sampler: sampler, point: point, epsilon: epsilon)
        let inv2eps = 1.0 / (2.0 * epsilon.Value)
        select new Vector3d(x: (samples.X1 - samples.X0) * inv2eps, y: (samples.Y1 - samples.Y0) * inv2eps, z: (samples.Z1 - samples.Z0) * inv2eps);
    public static Fin<Vector3d> CurlAt(Func<Point3d, Fin<Vector3d>> sampler, Point3d point, PositiveMagnitude epsilon) =>
        from samples in SampleAxes(sampler: sampler, point: point, epsilon: epsilon)
        let inv2eps = 1.0 / (2.0 * epsilon.Value)
        from curl in Acceptance.Value(value: new Vector3d(
            x: (samples.Y1.Z - samples.Y0.Z - (samples.Z1.Y - samples.Z0.Y)) * inv2eps,
            y: (samples.Z1.X - samples.Z0.X - (samples.X1.Z - samples.X0.Z)) * inv2eps,
            z: (samples.X1.Y - samples.X0.Y - (samples.Y1.X - samples.Y0.X)) * inv2eps))
        select curl;
    public static Fin<Vector3d> CurlNoiseAt(Func<Point3d, Fin<double>> sampler, Point3d point, PositiveMagnitude epsilon) =>
        new Vector3d(epsilon.Value, 1.3 * epsilon.Value, 0.7 * epsilon.Value) switch {
            var offset => (GradientAt(sampler, point, epsilon).ToValidation(),
                           GradientAt(sampler, point + (offset * 137.0), epsilon).ToValidation(),
                           GradientAt(sampler, point - (offset * 311.0), epsilon).ToValidation())
                .Apply(static (g1, g2, g3) => new Vector3d(g3.Y - g2.Z, g1.Z - g3.X, g2.X - g1.Y))
                .As().ToFin().Bind(static value => Acceptance.Value(value)),
        };
    public static Fin<double> DivergenceAt(Func<Point3d, Fin<Vector3d>> sampler, Point3d point, PositiveMagnitude epsilon) =>
        from samples in SampleAxes(sampler: sampler, point: point, epsilon: epsilon)
        let inv2eps = 1.0 / (2.0 * epsilon.Value)
        from value in Acceptance.Value(value: (samples.X1.X - samples.X0.X + samples.Y1.Y - samples.Y0.Y + samples.Z1.Z - samples.Z0.Z) * inv2eps)
        select value;
    public static Fin<double> LaplacianAt(Func<Point3d, Fin<double>> sampler, Point3d point, PositiveMagnitude epsilon) =>
        from samples in SampleAxes(sampler: sampler, point: point, epsilon: epsilon)
        from center in sampler(arg: point)
        let invEpsSq = 1.0 / (epsilon.Value * epsilon.Value)
        from value in Acceptance.Value(value: (samples.X1 + samples.X0 + samples.Y1 + samples.Y0 + samples.Z1 + samples.Z0 - (6.0 * center)) * invEpsSq)
        select value;
    public static Fin<double> StrainMagnitudeAt(Func<Point3d, Fin<Vector3d>> sampler, Point3d point, PositiveMagnitude epsilon) =>
        from samples in SampleAxes(sampler: sampler, point: point, epsilon: epsilon)
        let inv2eps = 1.0 / (2.0 * epsilon.Value)
        let sxx = (samples.X1.X - samples.X0.X) * inv2eps
        let syy = (samples.Y1.Y - samples.Y0.Y) * inv2eps
        let szz = (samples.Z1.Z - samples.Z0.Z) * inv2eps
        let sxy = 0.5 * (samples.Y1.X - samples.Y0.X + samples.X1.Y - samples.X0.Y) * inv2eps
        let sxz = 0.5 * (samples.Z1.X - samples.Z0.X + samples.X1.Z - samples.X0.Z) * inv2eps
        let syz = 0.5 * (samples.Z1.Y - samples.Z0.Y + samples.Y1.Z - samples.Y0.Z) * inv2eps
        from value in Acceptance.Value(value: Math.Sqrt(d: (sxx * sxx) + (syy * syy) + (szz * szz) + (2.0 * ((sxy * sxy) + (sxz * sxz) + (syz * syz)))))
        select value;
    // --- [LATTICE_STENCIL]
    private static int Reflect(long index, int count) {
        if (count is 1) { return 0; }
        long period = 2L * (count - 1L);
        long folded = ((index % period) + period) % period;
        return (int)(folded < count ? folded : period - folded);
    }
    private static double Tap(ReadOnlySpan<double> values, CellLattice grid, long column, long row, long layer) =>
        values[(int)grid.Linear(
            column: Reflect(index: column, count: grid.Columns.Value),
            row: Reflect(index: row, count: grid.Rows.Value),
            layer: Reflect(index: layer, count: grid.Layers.Value))];
    private static (double X1, double X0, double Y1, double Y0, double Z1, double Z0) LatticeAxes(
        ReadOnlySpan<double> values, CellLattice grid, int column, int row, int layer = 0) =>
        (X1: Tap(values: values, grid: grid, column: column + 1L, row: row, layer: layer),
         X0: Tap(values: values, grid: grid, column: column - 1L, row: row, layer: layer),
         Y1: Tap(values: values, grid: grid, column: column, row: row + 1L, layer: layer),
         Y0: Tap(values: values, grid: grid, column: column, row: row - 1L, layer: layer),
         Z1: Tap(values: values, grid: grid, column: column, row: row, layer: layer + 1L),
         Z0: Tap(values: values, grid: grid, column: column, row: row, layer: layer - 1L));
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
        double xy = (Tap(values: values, grid: grid, column: column + 1L, row: row + 1L, layer: layer)
                   - Tap(values: values, grid: grid, column: column + 1L, row: row - 1L, layer: layer)
                   - Tap(values: values, grid: grid, column: column - 1L, row: row + 1L, layer: layer)
                   + Tap(values: values, grid: grid, column: column - 1L, row: row - 1L, layer: layer)) / (4.0 * cell.X * cell.Y);
        (double xz, double yz, double zz) = grid.Rank is 3
            ? (Xz: (Tap(values: values, grid: grid, column: column + 1L, row: row, layer: layer + 1L)
                  - Tap(values: values, grid: grid, column: column + 1L, row: row, layer: layer - 1L)
                  - Tap(values: values, grid: grid, column: column - 1L, row: row, layer: layer + 1L)
                  + Tap(values: values, grid: grid, column: column - 1L, row: row, layer: layer - 1L)) / (4.0 * cell.X * cell.Z),
               Yz: (Tap(values: values, grid: grid, column: column, row: row + 1L, layer: layer + 1L)
                  - Tap(values: values, grid: grid, column: column, row: row + 1L, layer: layer - 1L)
                  - Tap(values: values, grid: grid, column: column, row: row - 1L, layer: layer + 1L)
                  + Tap(values: values, grid: grid, column: column, row: row - 1L, layer: layer - 1L)) / (4.0 * cell.Y * cell.Z),
               Zz: (z1 + z0 - (2.0 * center)) / (cell.Z * cell.Z))
            : (Xz: 0.0, Yz: 0.0, Zz: 0.0);
        return (Xx: (x1 + x0 - (2.0 * center)) / (cell.X * cell.X), Xy: xy, Xz: xz,
                Yy: (y1 + y0 - (2.0 * center)) / (cell.Y * cell.Y), Yz: yz, Zz: zz);
    }
}
```

## [03]-[WEIGHT_PROFILES]

- Owner: `KernelProfile` carries value, first and second derivative, and a `KernelStatus` smoothness verdict, so a consumer reads a kernel's derivative off the profile instead of re-differencing; `KernelKind` mints the kernel bases in three bands — compact-support rows, band-limited `Lanczos`/`Jinc` reconstruction rows whose profiles evaluate at 106-bit through the `ddouble` cardinal ladder and narrow once, and globally-supported RBF rows — each row privately carrying whether its normalized profile has compact support, its `Origin` status, `DerivativeSupremum`, its dimensionless slope-bound numerator, and `PolynomialOrder`, the reproduction-tail order the conditionally-positive-definite bases demand; `WeightKernel` mints the reconstruction-weight profiles; `Falloff` the radial-decay `[Union]` whose anisotropic case takes a `SymmetricMatrix` metric sampler driving the Mahalanobis distance.
- Cases: the `KernelStatus` verdicts, the compact, band-limited, and global `KernelKind` rows, the `WeightKernel` weights including the band-limited interpolating row, and the `Falloff` decay cases including the metric-sampler anisotropic one.
- Entry: `KernelKind.Profile(distance, radius)` returns the full gated profile and `Weight` the bare fast path; `WeightKernel.Weight` evaluates one reconstruction weight, `Falloff.Weight` consumes the offset and sample point its metric case requires, and `Falloff.Slope` is the local slope beside the family-wide `SlopeBound`.
- Auto: one `Profiled` body serves both support regimes — the row's private compact-support fact decides the `q = 1` clamp and its `Origin` the q→0 verdict — banded on the dimensionless `q = d/r` so classification is scale-invariant with exact zeros outside support; the profile is gated by `Acceptance.Value`, whose `IValidityEvidence` arm IS the finiteness proof, so the fold that hand-tested it deletes; the metric falloff proves the sampled tensor definite through `SymmetricMatrix.Definite`, so an indefinite metric fails typed instead of producing `√negative`.
- Law: `KernelProfile.FirstDerivative` is read by `Falloff.Slope` — the local Lipschitz bound `Spatial/fields` folds where the family-wide `SlopeBound` column answers `None` — and `SecondDerivative` is the profile's complete calculus output with no present consumer; `Rasm.Compute` `Tensor/sampling#RECONSTRUCT` binds the row's `Weight`, `DerivativeSupremum`, and `PolynomialOrder`.
- Output: `KernelProfile` is the per-evaluation reading — value, both derivatives, and status — proving itself through the `IValidityEvidence` fold.
- Packages: Thinktecture.Runtime.Extensions (`[UseDelegateFromConstructor]` columns, `[Union]`), LanguageExt.Core, `SymmetricMatrix` the metric carrier and its `Definite` verdict, TYoshimura.DoubleDouble (`ddouble.Sinc`/`CosPi`/`BesselJ` behind the band-limited rows), Rasm.Domain (the `Admit.KernelInput`/`FalloffInput` gates, `AcceptValue`, the `AcceptValidated<TVO>` bridge).
- Growth: a new kernel is one `KernelKind` row with its `Shape` column, its compact-support fact, and `DerivativeSupremum` — a closed-form supremum spells its derivation and a solved one states its solve; a new reconstruction weight is one `WeightKernel` row; a new decay law is one `Falloff` case, one `Weight` arm, one `Slope` arm, and its `SlopeBound` projection.
- Boundary: `Spatial/fields` wraps `Falloff.Metric` over its `TensorField` by passing the tensor sampler, so the tensor-field type never appears here; `Meshing/reconstruct` composes `KernelKind` and `WeightKernel` for its RBF, MLS, and Levin windows — one profile mathematics, zero copies. Positive-definiteness has ONE owner: `Numerics/matrix`'s `SymmetricMatrix.Definite` is the allocation-bounded verdict this page reads, and hand-rolled leading principal minors are the deleted form.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;
using DoubleDouble;
using LanguageExt;
using Rasm.Domain;
using Rhino.Geometry;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Numerics;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum]
public sealed partial class KernelStatus {
    public static readonly KernelStatus Smooth = new();
    public static readonly KernelStatus SupportBoundary = new();
    public static readonly KernelStatus NonsmoothOrigin = new();
    public static readonly KernelStatus OutsideSupport = new();
}

// --- [MODELS] --------------------------------------------------------------------------
[StructLayout(LayoutKind.Auto)]
public readonly record struct KernelProfile(double Value, double FirstDerivative, double SecondDerivative, KernelStatus Status) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        ValidityClaim.Finite(Value), ValidityClaim.Finite(FirstDerivative), ValidityClaim.Finite(SecondDerivative), Status is not null);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class KernelKind {
    public static readonly KernelKind Wendland = new(key: 0, isCompact: true, origin: KernelStatus.Smooth, derivativeSupremum: 135.0 / 64.0, polynomialOrder: 0,
        shape: static (q, r) => (Complement(q: q, power: 4) * (1.0 + (4.0 * q)), ((-20.0 * q) + (60.0 * q * q) - (60.0 * q * q * q) + (20.0 * q * q * q * q)) / r, (-20.0 + (120.0 * q) - (180.0 * q * q) + (80.0 * q * q * q)) / (r * r)));
    public static readonly KernelKind Quintic = new(key: 1, isCompact: true, origin: KernelStatus.NonsmoothOrigin, derivativeSupremum: 5.0, polynomialOrder: 0,
        shape: static (q, r) => (Complement(q: q, power: 5), -5.0 * Complement(q: q, power: 4) / r, 20.0 * Complement(q: q, power: 3) / (r * r)));
    public static readonly KernelKind Cosine = new(key: 2, isCompact: true, origin: KernelStatus.Smooth, derivativeSupremum: Math.PI / 2.0, polynomialOrder: 0,
        shape: static (q, r) => (0.5 * (1.0 + Math.Cos(d: Math.PI * q)), -0.5 * Math.PI * Math.Sin(a: Math.PI * q) / r, -0.5 * Math.PI * Math.PI * Math.Cos(d: Math.PI * q) / (r * r)));
    public static readonly KernelKind Cubic = new(key: 3, isCompact: true, origin: KernelStatus.NonsmoothOrigin, derivativeSupremum: 3.0, polynomialOrder: 0,
        shape: static (q, r) => (Complement(q: q, power: 3), -3.0 * Complement(q: q, power: 2) / r, 6.0 * (1.0 - q) / (r * r)));
    public static readonly KernelKind Linear = new(key: 4, isCompact: true, origin: KernelStatus.NonsmoothOrigin, derivativeSupremum: 1.0, polynomialOrder: 0,
        shape: static (q, r) => (1.0 - q, -1.0 / r, 0.0));
    public static readonly KernelKind Epanechnikov = new(key: 5, isCompact: true, origin: KernelStatus.Smooth, derivativeSupremum: 2.0, polynomialOrder: 0,
        shape: static (q, r) => (1.0 - (q * q), -2.0 * q / r, -2.0 / (r * r)));
    public static readonly KernelKind Lanczos = new(key: 6, isCompact: true, origin: KernelStatus.Smooth, derivativeSupremum: 2.8097867788012820, polynomialOrder: 0,
        shape: static (q, r) => (
            (double)(Sinc(x: 2.0 * q) * Sinc(x: q)),
            (double)((2.0 * SincPrime(x: 2.0 * q) * Sinc(x: q)) + (Sinc(x: 2.0 * q) * SincPrime(x: q))) / r,
            (double)((4.0 * SincSecond(x: 2.0 * q) * Sinc(x: q)) + (4.0 * SincPrime(x: 2.0 * q) * SincPrime(x: q)) + (Sinc(x: 2.0 * q) * SincSecond(x: q))) / (r * r)));
    public static readonly KernelKind Jinc = new(key: 7, isCompact: true, origin: KernelStatus.Smooth, derivativeSupremum: 1.3791295785936520, polynomialOrder: 0,
        shape: static (q, r) => (
            (double)(2.0 * ddouble.Jinc((ddouble)(BesselFirstZero * q))),
            q <= EpsilonPolicy.SqrtEpsilon ? -BesselFirstZero * BesselFirstZero * q / (4.0 * r) : (double)(-2.0 * ddouble.BesselJ(2, (ddouble)(BesselFirstZero * q)) / (ddouble)q) / r,
            q <= EpsilonPolicy.SqrtEpsilon
                ? -BesselFirstZero * BesselFirstZero / (4.0 * r * r)
                : (double)(-(ddouble)(BesselFirstZero * BesselFirstZero) * ((((ddouble.BesselJ(1, (ddouble)(BesselFirstZero * q)) - ddouble.BesselJ(3, (ddouble)(BesselFirstZero * q))) * (ddouble)(BesselFirstZero * q)) - (2.0 * ddouble.BesselJ(2, (ddouble)(BesselFirstZero * q)))) / ((ddouble)(BesselFirstZero * q) * (ddouble)(BesselFirstZero * q)))) / (r * r)));
    public static readonly KernelKind Gaussian = new(key: 8, isCompact: false, origin: KernelStatus.Smooth, derivativeSupremum: Math.Sqrt(2.0) * Math.Exp(-0.5), polynomialOrder: 0,
        shape: static (q, r) => (Math.Exp(-(q * q)), -2.0 * q * Math.Exp(-(q * q)) / r, ((4.0 * q * q) - 2.0) * Math.Exp(-(q * q)) / (r * r)));
    public static readonly KernelKind Multiquadric = new(key: 9, isCompact: false, origin: KernelStatus.Smooth, derivativeSupremum: 1.0, polynomialOrder: 1,
        shape: static (q, r) => (Math.Sqrt(d: 1.0 + (q * q)), q / (r * Math.Sqrt(d: 1.0 + (q * q))), 1.0 / (Math.Pow(x: 1.0 + (q * q), y: 1.5) * r * r)));
    public static readonly KernelKind InverseMultiquadric = new(key: 10, isCompact: false, origin: KernelStatus.Smooth, derivativeSupremum: Math.Pow(1.5, -1.5) / Math.Sqrt(2.0), polynomialOrder: 0,
        shape: static (q, r) => (1.0 / Math.Sqrt(1.0 + (q * q)), -q / (Math.Pow(1.0 + (q * q), 1.5) * r), ((2.0 * q * q) - 1.0) / (Math.Pow(1.0 + (q * q), 2.5) * r * r)));
    public static readonly KernelKind PolyharmonicCubic = new(key: 11, isCompact: false, origin: KernelStatus.Smooth, derivativeSupremum: double.PositiveInfinity, polynomialOrder: 2,
        shape: static (q, r) => (q * q * q, 3.0 * q * q / r, 6.0 * q / (r * r)));
    public static readonly KernelKind ThinPlateSpline = new(key: 12, isCompact: false, origin: KernelStatus.NonsmoothOrigin, derivativeSupremum: double.PositiveInfinity, polynomialOrder: 2,
        shape: static (q, r) => q <= EpsilonPolicy.ZeroTolerance
            ? (0.0, 0.0, 0.0)
            : (q * q * Math.Log(d: q), q * ((2.0 * Math.Log(d: q)) + 1.0) / r, ((2.0 * Math.Log(d: q)) + 3.0) / (r * r)));

    private bool IsCompact { get; }
    private KernelStatus Origin { get; }
    public double DerivativeSupremum { get; }
    public int PolynomialOrder { get; }
    [UseDelegateFromConstructor] private partial (double Value, double First, double Second) Shape(double q, double radius);

    public Fin<KernelProfile> Profile(double distance, double radius) =>
        from _ in Admit.KernelInput(distance: distance, radius: radius)
        from profile in Acceptance.Value(value: Profiled(q: distance / radius, radius: radius))
        select profile;
    public double Weight(double distance, PositiveMagnitude radius) =>
        Profiled(q: Math.Max(val1: 0.0, val2: distance) / radius.Value, radius: radius.Value).Value;
    private KernelProfile Profiled(double q, double radius) =>
        IsCompact && q > 1.0
            ? new(0.0, 0.0, 0.0, KernelStatus.OutsideSupport)
            : IsCompact && Math.Abs(q - 1.0) <= EpsilonPolicy.SqrtEpsilon
                ? new(0.0, 0.0, 0.0, KernelStatus.SupportBoundary)
                : Shape(q, radius) switch { var (value, first, second) => new KernelProfile(value, first, second,
                    q <= EpsilonPolicy.SqrtEpsilon ? Origin : KernelStatus.Smooth) };
    private static double Complement(double q, int power) => Math.Pow(x: 1.0 - q, y: power);
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

[SmartEnum]
public sealed partial class WeightKernel {
    public static readonly WeightKernel Biweight = new(profile: static t => (1.0 - (t * t)) * (1.0 - (t * t)));
    public static readonly WeightKernel WendlandC2 = new(profile: static t => Math.Pow(1.0 - t, 4) * (1.0 + (4.0 * t)));
    public static readonly WeightKernel Gaussian = new(profile: static t => Math.Exp(-9.0 * t * t));
    public static readonly WeightKernel Bump = new(profile: static t => Math.Exp(-(t * t) / Math.Max(1.0 - (t * t), EpsilonPolicy.ZeroTolerance)));
    public static readonly WeightKernel RegularizedInverseSquare = new(profile: static t => 1.0 / Math.Max(t * t, EpsilonPolicy.SqrtEpsilon));
    public static readonly WeightKernel Lanczos = new(profile: static t => (double)(ddouble.Sinc((ddouble)(2.0 * t), normalized: true) * ddouble.Sinc((ddouble)t, normalized: true)));
    [UseDelegateFromConstructor] private partial double Profile(double t);
    public double Weight(double distance, PositiveMagnitude support) =>
        distance >= support.Value ? 0.0 : Profile(t: Math.Min(val1: Math.Max(val1: 0.0, val2: distance) / support.Value, val2: 1.0));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record Falloff {
    private Falloff() { }
    public sealed record ConstantCase : Falloff { internal ConstantCase() { } }
    public sealed record PowerCase : Falloff { internal PowerCase(double Exponent) => this.Exponent = Exponent; public double Exponent { get; } }
    public sealed record GaussianCase : Falloff { internal GaussianCase(PositiveMagnitude Spread) => this.Spread = Spread; public PositiveMagnitude Spread { get; } }
    public sealed record KernelCase : Falloff { internal KernelCase(KernelKind Kind, PositiveMagnitude Radius) { this.Kind = Kind; this.Radius = Radius; } public KernelKind Kind { get; } public PositiveMagnitude Radius { get; } }
    public sealed record MetricCase : Falloff { internal MetricCase(KernelKind Kind, Func<Point3d, Fin<SymmetricMatrix>> Metric, PositiveMagnitude Radius) { this.Kind = Kind; this.Metric = Metric; this.Radius = Radius; } public KernelKind Kind { get; } public Func<Point3d, Fin<SymmetricMatrix>> Metric { get; } public PositiveMagnitude Radius { get; } }

    public Option<double> SlopeBound => Map<Option<double>>(
        constantCase: static _ => Some(0.0),
        powerCase: static p => p.Exponent switch { 0.0 => Some(0.0), 1.0 => Some(1.0), _ => Option<double>.None },
        gaussianCase: static g => Some(Math.Exp(-0.5) / g.Spread.Value),
        kernelCase: static k => double.IsFinite(k.Kind.DerivativeSupremum) ? Some(k.Kind.DerivativeSupremum / k.Radius.Value) : Option<double>.None,
        metricCase: static _ => Option<double>.None);
    public static Falloff Constant => new ConstantCase();
    public static Fin<Falloff> Power(double exponent) =>
        Acceptance.Value(value: exponent).Map(static value => (Falloff)new PowerCase(Exponent: value));
    public static Fin<Falloff> Gaussian(double spread) =>
        FactoryBridge.Accept<PositiveMagnitude>(candidate: spread).Map(static value => (Falloff)new GaussianCase(Spread: value));
    public static Fin<Falloff> Kernel(KernelKind kind, double radius) =>
        from active in Optional(kind).ToFin(new KernelFault.InvalidInput())
        from r in FactoryBridge.Accept<PositiveMagnitude>(candidate: radius)
        select (Falloff)new KernelCase(Kind: active, Radius: r);
    public static Fin<Falloff> Metric(KernelKind kind, Func<Point3d, Fin<SymmetricMatrix>> metric, double radius) =>
        from active in Optional(kind).ToFin(new KernelFault.InvalidInput())
        from sampler in Optional(metric).ToFin(new KernelFault.InvalidInput())
        from r in FactoryBridge.Accept<PositiveMagnitude>(candidate: radius)
        select (Falloff)new MetricCase(Kind: active, Metric: sampler, Radius: r);
    public Fin<double> Weight(Vector3d offset, Point3d sample, double tolerance) =>
        Admit.FalloffInput(distance: offset.Length, distanceSquared: offset.SquareLength, tolerance: tolerance).Bind(_ => Switch(
            state: (Distance: offset.Length, DistanceSquared: offset.SquareLength, Offset: offset, Sample: sample, Tolerance: tolerance),
            constantCase: static (_, _) => Fin.Succ(1.0),
            powerCase: static (s, p) => p.Exponent >= 0.0 || s.Distance > s.Tolerance
                ? Acceptance.Value(value: Math.Pow(x: s.Distance, y: p.Exponent))
                : Fin.Fail<double>(new KernelFault.InvalidInput()),
            gaussianCase: static (s, g) => Fin.Succ(Math.Exp(-s.DistanceSquared / (2.0 * g.Spread.Value * g.Spread.Value))),
            kernelCase: static (s, k) => k.Kind.Profile(distance: s.Distance, radius: k.Radius.Value).Map(static p => p.Value),
            metricCase: static (s, k) =>
                from tensor in k.Metric(arg: s.Sample)
                from _ in guard(tensor.Dimension.Value == 3, new KernelFault.InvalidInput())
                from definite in Try.lift(() => tensor.ToDense().Cholesky()).Run().Map(static _ => unit)
                from metricDistance in (s.Offset.X, s.Offset.Y, s.Offset.Z) switch {
                    (double x, double y, double z) when
                        (x * ((tensor.At(i: 0, j: 0) * x) + (tensor.At(i: 0, j: 1) * y) + (tensor.At(i: 0, j: 2) * z))) +
                        (y * ((tensor.At(i: 1, j: 0) * x) + (tensor.At(i: 1, j: 1) * y) + (tensor.At(i: 1, j: 2) * z))) +
                        (z * ((tensor.At(i: 2, j: 0) * x) + (tensor.At(i: 2, j: 1) * y) + (tensor.At(i: 2, j: 2) * z))) is double quadratic
                        && double.IsFinite(quadratic) && quadratic > -EpsilonPolicy.ZeroTolerance => Acceptance.Value(value: Math.Sqrt(d: Math.Max(val1: 0.0, val2: quadratic))),
                    _ => Fin.Fail<double>(new KernelFault.InvalidResult()),
                }
                from profile in k.Kind.Profile(distance: metricDistance, radius: k.Radius.Value)
                select profile.Value));
    public Fin<double> Slope(double distance, double tolerance) =>
        Admit.FalloffInput(distance: distance, distanceSquared: distance * distance, tolerance: tolerance).Bind(_ => Switch(
            state: (Distance: distance, Tolerance: tolerance),
            constantCase: static (_, _) => Fin.Succ(0.0),
            powerCase: static (s, p) => p.Exponent switch {
                0.0 => Fin.Succ(0.0),
                _ when p.Exponent >= 1.0 || s.Distance > s.Tolerance =>
                    Acceptance.Value(Math.Abs(p.Exponent) * Math.Pow(s.Distance, p.Exponent - 1.0)),
                _ => Fin.Fail<double>(new KernelFault.InvalidInput()),
            },
            gaussianCase: static (s, g) => Acceptance.Value(
                value: s.Distance * Math.Exp(d: -(s.Distance * s.Distance) / (2.0 * g.Spread.Value * g.Spread.Value)) / (g.Spread.Value * g.Spread.Value)),
            kernelCase: static (s, k) => k.Kind.Profile(s.Distance, k.Radius.Value)
                .Map(static profile => Math.Abs(profile.FirstDerivative)),
            metricCase: static (s, _) => Fin.Fail<double>(new KernelFault.Unsupported(InputType: typeof(MetricCase), OutputType: typeof(double)))));
}
```

## [04]-[NOISE_LATTICES]

- Owner: `FieldNoise` the `internal static` procedural-noise owner — classic Perlin gradient noise over its twelve canonical edge vectors, the 3D simplex lattice with an optional rotated second tap, and Worley cellular noise, all over one coordinate-hashed lattice substrate.
- Cases: Perlin, simplex, and Worley lattices over one `LatticeHash` substrate; `SimplexAt` takes a `rotationMix` scalar — `0.0` samples the primary lattice alone, a positive mix blends in the axis-rotated lattice by that fraction — and `SampleSimplex` is the private skew-domain kernel both taps ride, selecting the cell in skewed coordinates and measuring its corners back in Euclidean ones.
- Entry: every lattice takes `(point, seed, frequency)`, deterministic for a given triple so noise-driven fields replay across processes; octave, persistence, and lacunarity admission is the consumer's policy through `Admit.NoiseInput`, the lattice itself total over finite input.
- Auto: `LatticeHash(column, row, layer, seed, lane)` MIXES the seed through the branch's one splitmix64 owner, so a seed relabels the lattice rather than translating it — an additive fold makes `(x, s+1)` and `(x+1, s)` the same word — and every algorithm reads its own LANE ordinal, local to the member that owns it: Perlin one, Worley's three feature-point axes three, the two simplex taps two, so no two lattices share a hash word at equal coordinates and the Worley axes decorrelate instead of reading one stream at three fixed offsets.
- Exemption: `WorleyAt`'s 27-cell neighbourhood is a declared statement kernel — the running minimum accumulates over a per-texel hot path where a query fold allocates twenty-seven tuples and six bindings per sample.
- Packages: `Rasm.Domain` (`Deterministic.Stream` — the branch's ONE splitmix64 owner every kernel draw threads), BCL (`Math.Floor`, integer bit ops), RhinoCommon `Point3d` as the coordinate carrier.
- Growth: a new lattice is one member over the `LatticeHash` substrate reading its own lane ordinal; fractal octave sums (fBm, turbulence) are the consumer's fold over these single-octave taps, `Spatial/fields` owning the octave policy.
- Boundary: Perlin's twelve canonical edge vectors are the one literal table on this page — a table with a defining publication transcribes verbatim, where an authored table with no defining sequence rides as a digest-pinned content-keyed asset; the published permutation table does NOT enter, because the lattice key is already a many-to-one coordinate hash and a permutation indexed by it would promise a bijection the substrate never holds. The noise VOCABULARY — `NoiseKind` rows and their `CapabilitySet<NoiseTrait>` columns — is `Spatial/fields`', this page owning only the lattice mathematics those rows point at. `Rasm.Materials` `Appearance/texture#TEXTURE_UV` `ProceduralNoise` is a DELIBERATE second lattice family, split on differentiability-vs-parity: this owner hashes lattice coordinates feeding the `NoiseTrait.Differentiable` membership (the `CurlNoise` admission gate and the `ScalarField.LipschitzBound` fold), while the Materials family holds FastNoiseLite byte-exactness for MaterialX category parity and the WGSL `f32` wrap law, with 2D arms, periodic-by-construction cell-index lattices, and the cellular return set this floor never needs — collapsing either end breaks the other's gating [branch RULINGS `[03]-[COLLAPSE]`].

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using Rasm.Domain;
using Rhino.Geometry;

namespace Rasm.Numerics;

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class FieldNoise {
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
    internal static double WorleyAt(Point3d point, int seed, double frequency) {
        double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
        int cx = (int)Math.Floor(d: px); int cy = (int)Math.Floor(d: py); int cz = (int)Math.Floor(d: pz);
        const long xLane = 1L, yLane = 2L, zLane = 3L;
        double nearest = double.PositiveInfinity;
        for (int dx = -1; dx <= 1; dx++) {
            for (int dy = -1; dy <= 1; dy++) {
                for (int dz = -1; dz <= 1; dz++) {
                    int nx = cx + dx; int ny = cy + dy; int nz = cz + dz;
                    double ddx = nx + (LatticeHash(nx, ny, nz, seed, xLane) / 256.0) - px;
                    double ddy = ny + (LatticeHash(nx, ny, nz, seed, yLane) / 256.0) - py;
                    double ddz = nz + (LatticeHash(nx, ny, nz, seed, zLane) / 256.0) - pz;
                    nearest = Math.Min(val1: nearest, val2: (ddx * ddx) + (ddy * ddy) + (ddz * ddz));
                }
            }
        }
        return Math.Sqrt(d: nearest);
    }
    internal static double SimplexAt(Point3d point, int seed, double frequency, double rotationMix) {
        const long primaryLane = 4L, rotatedLane = 5L;
        double primary = SampleSimplex(point, seed, frequency, primaryLane);
        return rotationMix <= 0.0 ? primary : primary + (rotationMix *
            (SampleSimplex(new Point3d(point.Y, point.Z, point.X), seed, frequency, rotatedLane) - primary));
    }
    private static double SampleSimplex(Point3d point, int seed, double frequency, long lane) {
        double px = point.X * frequency; double py = point.Y * frequency; double pz = point.Z * frequency;
        const double inverseSkew = 1.0 / 6.0;
        double skew = (px + py + pz) / 3.0;
        int i = (int)Math.Floor(px + skew); int j = (int)Math.Floor(py + skew); int k = (int)Math.Floor(pz + skew);
        double unskew = (i + j + k) * inverseSkew;
        double x0 = px - i + unskew; double y0 = py - j + unskew; double z0 = pz - k + unskew;
        (int i1, int j1, int k1, int i2, int j2, int k2) = x0 >= y0
            ? y0 >= z0 ? (1, 0, 0, 1, 1, 0) : x0 >= z0 ? (1, 0, 0, 1, 0, 1) : (0, 0, 1, 1, 0, 1)
            : y0 < z0 ? (0, 0, 1, 0, 1, 1) : x0 < z0 ? (0, 1, 0, 0, 1, 1) : (0, 1, 0, 1, 1, 0);
        static double SimplexCorner(int hash, double x, double y, double z) {
            double t = 0.6 - (x * x) - (y * y) - (z * z);
            return t <= 0.0 ? 0.0 : t * t * t * t * Grad(hash, x, y, z);
        }
        double n0 = SimplexCorner(LatticeHash(i, j, k, seed, lane), x0, y0, z0);
        double n1 = SimplexCorner(LatticeHash(i + i1, j + j1, k + k1, seed, lane), x0 - i1 + inverseSkew, y0 - j1 + inverseSkew, z0 - k1 + inverseSkew);
        double n2 = SimplexCorner(LatticeHash(i + i2, j + j2, k + k2, seed, lane), x0 - i2 + (2.0 * inverseSkew), y0 - j2 + (2.0 * inverseSkew), z0 - k2 + (2.0 * inverseSkew));
        double n3 = SimplexCorner(LatticeHash(i + 1, j + 1, k + 1, seed, lane), x0 - 0.5, y0 - 0.5, z0 - 0.5);
        return 32.0 * (n0 + n1 + n2 + n3);
    }
    private static int LatticeHash(int column, int row, int layer, int seed, long lane) =>
        (int)(Deterministic.Stream(lanes: [column, row, layer, lane], seed: seed) & 0xFF);
}
```

## [05]-[SOLAR_EPHEMERIS]

- Owner: `SolarSite` the validated geodetic site; `SunPosition` the apparent azimuth/altitude result owning both the survey-frame `OfDirection` ingress and the NOAA/Meeus `At` factory — the branch's ONE solar ephemeris, every consumer a projection over it.
- Entry: `At(site, instant)` derives apparent azimuth/altitude — quadratic mean longitude, nutation-corrected ecliptic longitude, the mean-obliquity polynomial plus the apparent-obliquity correction `0.00256·cos Ω` off the same nutation argument, the orbital eccentricity evaluated at the current Julian century rather than frozen at its J2000 term, and elevation-derived pressure-corrected refraction.
- Auto: the fold is total and effect-free — closed-form astronomy over finite admitted input carries no `Fin` result, and `At(site, instant)` is OFFSET-INVARIANT for a fixed instant and longitude, since an `Instant` is absolute — the standard offset selects local civil-day boundaries and host projections, never a term inside the instant equation; `SolarSite` types its `StandardOffset` as a NodaTime `Offset` — the site's fixed standard displacement from UTC, whose own range forecloses the hand guard; a `DateTimeZone` with its daylight rules and transitions remains the calendar/application boundary's owner — and CANONICALIZES longitude into `[-180, 180)` by `ref`, so one meridian has one spelling and `179.9` and `-180.1` are not two values one wrap apart.
- Exemption: `At` is a declared statement kernel — one closed-form astronomical chain whose twenty intermediate terms each name a published quantity, and a query-expression spelling would rename every one of them. Every COEFFICIENT the chain reads sits beside the named quantity it feeds and evaluates through `MathNet.Numerics.Polynomial.Evaluate`: mean longitude and mean anomaly (Meeus, Astronomical Algorithms 2nd ed., ch. 25, 25.2 and 25.3), the three equation-of-centre terms and the apparent-longitude correction (ch. 25), the nutation argument Ω, the mean-obliquity arcsecond tail, and its `0.00256·cos Ω` apparent correction (ch. 22, 22.2), the Julian-century eccentricity polynomial (ch. 25, 25.4), and Saemundsson refraction (ch. 16, 16.4) — so a transcription error is visible at the quantity it corrupts rather than in a roster no equation reads.
- Packages: NodaTime (`Instant`/`Offset` — the clock carriers; a `DateTime`-taking overload is the deleted form), MathNet.Numerics (`Polynomial.Evaluate`, `Trig` angle conversions), Thinktecture.Runtime.Extensions (`[ComplexValueObject]`, default `ValidationError`), LanguageExt.Core (`Option`), `Numerics/atoms` (`Reduce.Floored`), RhinoCommon (`Vector3d` — the kernel's ONE coordinate, `Numerics/atoms#VECTOR_ALGEBRA`'s carrier, whose `IsValid` screens the host unset sentinel a raw finiteness probe admits), BCL inbox (`Math`).
- Growth: an accuracy refinement (full SPA periodic-term tables over the truncated form) is a body change on `At`; a new consumer composes that factory at its schedule-owning boundary, never a duplicate almanac; zero new surface.
- Boundary: consumers project the ANGLES into their own world frame — `Rasm.Compute` `Analysis/daylight` folds them into its float clash coordinate at one `SurveyRay` narrowing, `Rasm.Materials` `Appearance/environment#SKY_MODEL` projects azimuth/altitude onto its `+X`-north `WorldDirection`, and `Rasm.AppUi` `Render/pathtrace#LIGHT_RIG` seats the angles on its Sun row — so the frame convention lives at each consuming edge and the almanac states angles alone; the geodetic datum, site CRS, and any reprojection stay the app-root edge's. Consumers holding a VECTOR rotate it into the survey frame at their own edge and re-read through `OfDirection` — `Rasm.Rhino` `Render/settings#SUN_ASTRONOMY` folds the host's north bearing and its sun-toward-scene sign there — so the sign and the north datum resolve where the producing frame is still known, never inside this almanac.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
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
    public Offset StandardOffset { get; }
    public double ElevationM { get; }

    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref double latitudeDeg, ref double longitudeDeg,
        ref Offset standardOffset, ref double elevationM) {
        if (double.IsFinite(longitudeDeg)) { longitudeDeg = Reduce.Floored(longitudeDeg + 180.0, 360.0) - 180.0; }
        validationError = double.IsFinite(latitudeDeg) && latitudeDeg is >= -90.0 and <= 90.0
            && double.IsFinite(longitudeDeg)
            && double.IsFinite(elevationM) && elevationM is > -500.0 and <= 10000.0
                ? null : ValidationError.Create($"{nameof(SolarSite)} is invalid.");
    }
}

public readonly record struct SunPosition(double AzimuthDeg, double AltitudeDeg) {
    public static Option<SunPosition> OfDirection(Vector3d direction) =>
        from unit in (direction.Length switch {
            double length when direction.IsValid && double.IsFinite(length) && length > 0.0 => Some(direction / length),
            _ => None,
        })
        select new SunPosition(
            AzimuthDeg: Reduce.Floored(MathNet.Numerics.Trig.RadianToDegree(Math.Atan2(unit.X, unit.Y)), 360.0),
            AltitudeDeg: MathNet.Numerics.Trig.RadianToDegree(Math.Asin(Math.Clamp(unit.Z, -1.0, 1.0))));

    public static SunPosition At(SolarSite site, Instant instant) {
        const double j2000 = 2451545.0, centuryDays = 36525.0;
        double jd = instant.ToJulianDate();
        double t = (jd - j2000) / centuryDays;
        double meanLongitude = Reduce.Floored(MathNet.Numerics.Polynomial.Evaluate(t, [280.46646, 36000.76983, 0.0003032]), 360.0);
        double meanAnomaly = MathNet.Numerics.Trig.DegreeToRadian(MathNet.Numerics.Polynomial.Evaluate(t, [357.52911, 35999.05029, -0.0001537]));
        double center = (Math.Sin(meanAnomaly) * MathNet.Numerics.Polynomial.Evaluate(t, [1.914602, -0.004817, -0.000014]))
            + (Math.Sin(2.0 * meanAnomaly) * MathNet.Numerics.Polynomial.Evaluate(t, [0.019993, -0.000101]))
            + (Math.Sin(3.0 * meanAnomaly) * 0.000289);
        double nutationArgument = MathNet.Numerics.Trig.DegreeToRadian(MathNet.Numerics.Polynomial.Evaluate(t, [125.04, -1934.136]));
        double eclipticLongitude = MathNet.Numerics.Trig.DegreeToRadian(meanLongitude + center - 0.00569 - (0.00478 * Math.Sin(nutationArgument)));
        double obliquity = MathNet.Numerics.Trig.DegreeToRadian(23.0 + ((26.0 + (MathNet.Numerics.Polynomial.Evaluate(t, [21.448, -46.815, -0.00059, 0.001813]) / 60.0)) / 60.0)
            + (0.00256 * Math.Cos(nutationArgument)));
        double declination = Math.Asin(Math.Sin(obliquity) * Math.Sin(eclipticLongitude));
        double y = Math.Tan(obliquity / 2.0) * Math.Tan(obliquity / 2.0);
        double eccentricity = MathNet.Numerics.Polynomial.Evaluate(t, [0.016708634, -0.000042037, -0.0000001267]);
        double meanLonRad = MathNet.Numerics.Trig.DegreeToRadian(meanLongitude);
        double equationOfTime = 4.0 * MathNet.Numerics.Trig.RadianToDegree(
            y * Math.Sin(2.0 * meanLonRad) - 2.0 * eccentricity * Math.Sin(meanAnomaly)
            + 4.0 * eccentricity * y * Math.Sin(meanAnomaly) * Math.Cos(2.0 * meanLonRad)
            - 0.5 * y * y * Math.Sin(4.0 * meanLonRad) - 1.25 * eccentricity * eccentricity * Math.Sin(2.0 * meanAnomaly));
        double fractionalDay = jd - Math.Floor(jd) - 0.5;
        double trueSolarMinutes = Reduce.Floored((fractionalDay * 1440.0) + equationOfTime + (4.0 * site.LongitudeDeg), 1440.0);
        double hourAngle = MathNet.Numerics.Trig.DegreeToRadian((trueSolarMinutes / 4.0) - 180.0);
        double phi = MathNet.Numerics.Trig.DegreeToRadian(site.LatitudeDeg);
        double altitude = Math.Asin(Math.Clamp(value: (Math.Sin(phi) * Math.Sin(declination)) + (Math.Cos(phi) * Math.Cos(declination) * Math.Cos(hourAngle)), min: -1.0, max: 1.0));
        double azimuth = Math.Atan2(Math.Sin(hourAngle), Math.Cos(hourAngle) * Math.Sin(phi) - Math.Tan(declination) * Math.Cos(phi));
        double altitudeDeg = MathNet.Numerics.Trig.RadianToDegree(altitude);
        const double lapse = 2.25577e-5, pressurePower = 5.25588;
        double pressureRatio = Math.Pow(1.0 - (lapse * site.ElevationM), pressurePower);
        double refractionDeg = altitudeDeg is > -1.0 and < 90.0
            ? pressureRatio * 1.02 / Math.Tan(MathNet.Numerics.Trig.DegreeToRadian(altitudeDeg + (10.3 / (altitudeDeg + 5.11)))) / 60.0
            : 0.0;
        return new SunPosition(Reduce.Floored(MathNet.Numerics.Trig.RadianToDegree(azimuth) + 180.0, 360.0), altitudeDeg + refractionDeg);
    }
}
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

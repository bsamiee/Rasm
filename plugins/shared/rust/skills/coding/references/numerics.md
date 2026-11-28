# [NUMERICS]

## [01]-[INTEGERS]

Integer arithmetic states what happens past its type's range:
- Integer and `Decimal` arithmetic an input can overflow is `checked_*` mapped to a refusal naming that input, `saturating_*` where the domain clamps
- Operators stay where the type's range bounds every operand (an index below a slice's length)
- Unsigned division and remainder take a `NonZero` divisor, and `u64 / NonZero<u64>` has no panic path
- Signed division takes `checked_div`, `None` for a zero divisor and for `MIN / -1`, and `Div<NonZero>` exists for unsigned types alone
- Ranges reaching a type's maximum are `RangeInclusive` (`0..=u16::MAX`), where `0..u16::MAX` and a `u16` count stop one short
- Narrowing takes `try_from` where an out-of-range value is a refusal, and `saturating_cast` under `integer_casts` where the domain clamps
- Integers cross bytes in the wire's stated order (`from_le_bytes`, `to_be_bytes`), each field split off by `split_first_chunk::<N>()`
- Generic arithmetic bounds the `num_traits` trait it calls (`CheckedAdd`), and `Signed` where a difference can fall below zero

## [02]-[FLOATS]

Float code takes its order, conversions, and formulas from std and package members:
- `f64::total_cmp` orders floats nothing stores (`max_by(f64::total_cmp)`, `cmp::minmax_by(a, b, f64::total_cmp)` under `cmp_minmax`)
- Floats stored as keys are a `typed_floats` type implementing `Eq`, `Ord`, and `Hash`, or `NotNan<f64>` where infinities are valid keys
- `NotNan` arithmetic panics on a NaN result (`inf - inf`), and code computes on `into_inner` values and refuses NaN through `NotNan::new`
- Floats entering from a boundary (decoded, measured) are refused there when not finite
- Floats narrow through `cast::<f32>()`, and to integers through `to_int_checked` or `to_int_saturating` (NaN to zero), under `float_conversions`
- `to_int_checked` and `to_int_saturating` truncate toward zero, and a nearest integer calls `round` first
- Counts enter float arithmetic as `f64::from` of a `u32`, or through `nalgebra::convert::<usize, f64>` for a `usize`
- Wrapped values (an angle, a longitude) take `rem_euclid`, where `%` keeps the dividend's sign
- Formulas a package member names take it (`Point::lerp`, `Vector2::perp`, `Unit::try_new_and_get` returning a direction with its norm)
- Members returning NaN for no samples (`Data::quantile`, `WeightedMean::mean`) run after the caller refuses an empty input

## [03]-[TOLERANCES]

Tolerances and limits are values a caller passes, and comparisons read them:
- Computed floats compare within a tolerance parameter, never by `==` or a crate constant
- Tolerances pair an absolute part for values near zero with a relative part for values of any magnitude
- Comparisons call `approx::relative_eq!(next, last, epsilon = absolute, max_relative = relative)` in place of a hand-written bound
- Tests assert computed floats through `approx::assert_relative_eq!` with the tolerance their oracle states
- Solvers take the caller's tolerance and iteration limit through their setters (`with_abs_tol`, `with_max_iter`), never a package default
- Limits a callee derives from its input (a schedule's length) are `Option`, `None` where the input yields none
- Absent limits stay `None` through every read, a stand-in (`f64::INFINITY`, `u64::MAX`) appearing only as a foreign call's plain argument

## [04]-[UNITS]

Quantities cross a signature as `uom` values, and a dimension error fails the build:

```rust
#[must_use]
pub fn travel(distance: Length, speed: Velocity) -> Time { distance / speed }

#[must_use]
pub fn span<D>(low: Quantity<D, SI<f64>, f64>, high: Quantity<D, SI<f64>, f64>) -> Quantity<D, SI<f64>, f64>
where
    D: Dimension + ?Sized,
    D::Kind: marker::Sub,
{
    high - low
}
```

`Length`, `Velocity`, and `Time` come from `uom::si::f64`, and `travel` declared to return a `Length` fails to compile:
- `span` subtracts quantities of any dimension with a kind implementing `marker::Sub`, `TemperatureInterval` among them
- `ThermodynamicTemperature` operands fail the bound of `span`

Plain values meet quantities at fixed points:
- Kernels over `f64` read each quantity once through `get::<meter>()` and return through `Length::new::<meter>`
- Coordinates are one point type (`Point2<f64>`), never parallel `x` and `y` parameters
- Durations build through their unit's constructor (`Duration::from_mins`, `SignedDuration::from_hours`), never seconds times a factor

## [05]-[SOLVERS]

Roots, integrals, differential equations, factorizations, derivatives, statistics, transforms, neighbors, and programs are package members:

| [INDEX] | [PROBLEM]                 | [MEMBER]                                                               |
| :-----: | :------------------------ | :--------------------------------------------------------------------- |
|  [01]   | Bracketed scalar root     | `basin` `Toms748Root::solve`, `BrentRoot::solve`                       |
|  [02]   | Bounded smooth minimum    | `basin` `Lbfgsb` run by `Executor`                                     |
|  [03]   | Fixed-order integral      | `gauss_quad` `GaussLegendre::new(degree).integrate(a, b, f)`           |
|  [04]   | Integral within tolerance | `bilby` `AdaptiveIntegrator` with `with_abs_tol` and `with_rel_tol`    |
|  [05]   | Initial value problem     | `diffsol` `OdeBuilder`, then `bdf` when stiff, else `tsit45`           |
|  [06]   | Dense factorization       | `faer` `partial_piv_lu`, `qr`, `thin_svd`, `llt`, `self_adjoint_eigen` |
|  [07]   | Sparse factorization      | `faer` `SymbolicLlt::try_new`, `Llt::try_new_with_symbolic`            |
|  [08]   | Derivative                | `num_dual` `first_derivative`, `gradient`, `hessian`                   |
|  [09]   | Mean and variance         | `average` `Variance`, `WeightedMean` collected from an iterator        |
|  [10]   | Quantile                  | `statrs` `Data::quantile` of `OrderStatistics`                         |
|  [11]   | Fourier transform         | `rustfft` `FftPlanner`, `realfft` `RealFftPlanner` for real input      |
|  [12]   | Nearest points            | `kiddo` `ImmutableKdTree::new_from_slice`, `query(..).nearest_n(..)`   |
|  [13]   | Linear program            | `good_lp` `using(highs)` under its `highs` feature                     |
|  [14]   | Conic program             | `clarabel` `DefaultSolver`                                             |
|  [15]   | Nonlinear program         | `nlopt` `Nlopt::optimize`                                              |

- Fourier transforms of one length share a plan and one `get_inplace_scratch_len()` buffer through `process_with_scratch`, never `process`
- Fourier transform outputs are unnormalized, and code scales a forward then inverse round trip by `1 / len`
- Spatial indexes hold each point's position in the slice they were built from, and results join back to records through it

Sparse systems solved for many value sets over one pattern analyze the pattern once:

```rust
#[derive(Debug)]
pub struct Pattern {
    structure: SymbolicSparseColMat<usize>,
    order: Argsort<usize>,
    symbolic: SymbolicLlt<usize>,
}

impl Pattern {
    pub fn new(size: usize, entries: &[Pair<usize, usize>]) -> Result<Self, CreationError> {
        let (structure, order) = SymbolicSparseColMat::try_new_from_indices(size, size, entries)?;
        let symbolic = SymbolicLlt::try_new(structure.as_ref(), Side::Lower)?;
        Ok(Self { structure, order, symbolic })
    }

    pub fn solved(&self, values: &[f64], load: &Col<f64>) -> Result<Col<f64>, LltError> {
        let matrix = SparseColMat::new_from_argsort(self.structure.clone(), &self.order, values)?;
        let factor = Llt::try_new_with_symbolic(self.symbolic.clone(), matrix.as_ref(), Side::Lower)?;
        Ok(factor.solve(load))
    }
}
```

`Pattern::new` sorts entry positions and runs symbolic Cholesky analysis once, and `solved` factors each value set numerically alone:
- `solved` places `values` through the stored `Argsort`, and `values` follow the order of `entries`
- `SymbolicLlt` clones share one analysis through an `Arc`
- `faer::linalg::solvers::Solve` supplies `solve`
- Value sets that are not positive definite return `LltError`

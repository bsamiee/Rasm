# [PY_COMPUTE_API_INTERPAX]

`interpax` owns JAX-native differentiable interpolation and FFT resampling on regular grids for the compute interpolation rail. Every interpolant is an Equinox-style pytree module, so an interpolated field enters the JAX rail as a differentiable leaf inside an `equinox`/`optimistix` objective, a `quadax` integrand, or a `diffrax` ODE term with no boundary conversion.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: reusable interpolant objects — JAX pytree modules, `jit`/`vmap`/`grad`-compatible and re-evaluable at arbitrary query points after one construction; built via `Interpolator{1,2,3}D(x[, y[, z]], f, method, extrap, period)`, called with query points, `derivative` accepted on evaluation
- [INTERPOLANT]: `Interpolator1D` `Interpolator2D` `Interpolator3D`

[PUBLIC_TYPE_SCOPE]: `scipy.interpolate`-equivalent spline classes — JAX-differentiable drop-ins evaluated through the JAX-native kernels; constructors mirror the same-named `scipy.interpolate` signatures, `__call__(x, nu=0, extrapolate=None)` evaluates with `nu` the derivative order, every calculus method returning a new differentiable spline/`PPoly` or array

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]          | [CAPABILITY]                                |
| :-----: | :-------------------- | :--------------------- | :------------------------------------------ |
|  [01]   | `CubicSpline`         | cubic spline           | C2 cubic spline                             |
|  [02]   | `PchipInterpolator`   | monotonic cubic spline | shape-preserving PCHIP                      |
|  [03]   | `Akima1DInterpolator` | Akima spline           | Akima piecewise-cubic                       |
|  [04]   | `CubicHermiteSpline`  | Hermite cubic spline   | cubic Hermite from values + derivatives     |
|  [05]   | `PPoly`               | piecewise polynomial   | breakpoint/coefficient piecewise polynomial |

- [CALCULUS]: `<spline>.derivative(nu=1)` `<spline>.antiderivative(nu=1)` `<spline>.integrate(a, b)` `<spline>.roots()`

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: module-level evaluation functions — `interp{1,2,3}d` evaluate on a regular grid, `fft_interp{1,2}d` spectrally resample uniformly sampled periodic data through the trigonometric interpolant of the input samples
- interp carry: `method`, `derivative`, `extrap`, `period`; `method` selects the kernel — `"nearest"` `"linear"` `"cubic"` `"cubic2"` `"catmull-rom"` `"monotonic"` (PCHIP) `"monotonic-0"`, `extrap` sets out-of-bound behavior, `period` marks periodic axes.

| [INDEX] | [SURFACE]                                 | [CAPABILITY]                               |
| :-----: | :---------------------------------------- | :----------------------------------------- |
|  [01]   | `interp1d(xq, x, f)`                      | interpolate `f(x)` at query points `xq`    |
|  [02]   | `interp2d(xq, yq, x, y, f)`               | interpolate `f(x, y)` on a regular grid    |
|  [03]   | `interp3d(xq, yq, zq, x, y, z, f)`        | interpolate `f(x, y, z)` on a regular grid |
|  [04]   | `fft_interp1d(f, n, sx, dx)`              | FFT-interpolate periodic `f` to length `n` |
|  [05]   | `fft_interp2d(f, n1, n2, sx, sy, dx, dy)` | FFT-interpolate periodic 2D `f`            |

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- namespace: `interpax` — every family imports from top level.
- one-shot `interp{1,2,3}d` recompiles per call signature; `Interpolator{1,2,3}D` constructs once and re-evaluates, hoisting the coefficient fit out of the trace — the form for a hot loop or a re-queried grid.
- every interpolant is a JAX pytree, `jit`/`vmap`/`grad`-compatible w.r.t. BOTH the query points and the sampled values `f`, so gradients flow through an interpolated field back to the data that defined it.

[STACKING]:
- `equinox`(`.api/equinox.md`): an `Interpolator{1,2,3}D` is an Equinox-style pytree `Module`, so an interpolated field is a differentiable leaf inside an `equinox.Module` objective — `optax`/`optimistix` step it like a learned weight with no boundary conversion.
- `quadax`(`.api/quadax.md`): an interpolant or spline `__call__` is a valid `quadgk`/`quadcc` integrand; `<spline>.integrate(a, b)` returns the exact piecewise-polynomial integral when the integrand IS the spline.
- `diffrax`(`.api/diffrax.md`): an interpolant inside an `ODETerm` vector field supplies differentiable time-/space-varying forcing, and `<spline>.derivative()` its analytic derivative without finite differencing.
- `jax`(`.api/jax.md`): `interp{1,2,3}d` compose under `jax.vmap` to batch independent grids and under `jax.grad` for interpolated-quantity sensitivity; `fft_interp{1,2}d` is the periodic counterpart to `jax.numpy.fft` resampling.
- `findiff`(`.api/findiff.md`): the non-differentiable stencil fallback; the analytic `.derivative()` of a JAX interpolant supersedes a `findiff` stencil for the same field.
- `compute` solvers/field: an interpolated material or forcing field fits once outside the trace and threads as a differentiable leaf through the folder's quadrature and ODE rails.

[LOCAL_ADMISSION]:
- regular-grid interpolation, periodic resampling, and spline calculus route to `interpax`; a re-queried grid fits once outside the trace as an `Interpolator{1,2,3}D`, a single evaluation uses `interp{1,2,3}d`.

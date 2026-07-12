# [PY_COMPUTE_API_INTERPAX]

`interpax` supplies JAX-native, differentiable, `vmap`-friendly 1D/2D/3D interpolation and FFT-based resampling for the compute interpolation rail. The package owner routes regular-grid interpolation cases through `interp1d`/`interp2d`/`interp3d` for one-shot evaluation, the `Interpolator{1,2,3}D` callable objects for reusable JAX-differentiable interpolants, `fft_interp1d`/`fft_interp2d` for periodic spectral resampling, plus drop-in JAX-differentiable equivalents of `scipy.interpolate.CubicSpline`/`PchipInterpolator`/`Akima1DInterpolator`/`CubicHermiteSpline`/`PPoly` as first-class spline classes; it never re-implements a cubic-spline or PCHIP kernel the package already owns. Because every interpolant is an Equinox-style pytree module, it slots directly into the JAX rail: an interpolated field becomes a differentiable leaf inside an `equinox`/`optimistix` objective, the integrand of a `quadax` quadrature, or the right-hand side of a `diffrax` ODE term, with no boundary conversion.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `interpax`
- package: `interpax`
- import: `interpax`
- owner: `compute`
- rail: interpolation
- installed: `0.3.14`
- capability: JAX-native differentiable interpolation on regular grids — 1D/2D/3D linear/cubic/PCHIP/monotonic methods, reusable callable interpolant objects (`vmap`/`grad`/`jit`-compatible), and FFT-based periodic resampling

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: callable interpolant objects
- rail: interpolation
- JAX pytree-registered Equinox-style modules; each instance is differentiable, `jit`/`vmap`/`grad`-compatible, and re-evaluable at arbitrary query points after one construction.

| [INDEX] | [SYMBOL]         | [PACKAGE_ROLE]        | [CAPABILITY]                                               |
| :-----: | :--------------- | :-------------------- | :--------------------------------------------------------- |
|  [01]   | `Interpolator1D` | 1D interpolant object | reusable differentiable interpolant over a 1D regular grid |
|  [02]   | `Interpolator2D` | 2D interpolant object | reusable differentiable interpolant over a 2D regular grid |
|  [03]   | `Interpolator3D` | 3D interpolant object | reusable differentiable interpolant over a 3D regular grid |

[PUBLIC_TYPE_SCOPE]: scipy.interpolate-equivalent spline classes
- rail: interpolation
- JAX-differentiable, `jit`/`vmap`/`grad`-compatible drop-in replacements for the `scipy.interpolate` spline classes; same constructor and call signatures, evaluated through the JAX-native kernels instead of SciPy. Each is a pytree module: `__call__(x, nu=0, extrapolate=None)` evaluates (`nu` = derivative order), and the calculus methods (`.derivative(nu)`, `.antiderivative(nu)`, `.integrate(a, b)`, `.roots()`) return new spline/PPoly objects or arrays — all differentiable.

| [INDEX] | [SYMBOL]                                                            | [PACKAGE_ROLE]         | [CAPABILITY]                                                                                |
| :-----: | :------------------------------------------------------------------ | :--------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `CubicSpline(x, y, axis=0, bc_type='not-a-knot', extrapolate=None)` | cubic spline           | C2 cubic spline; drop-in for `scipy.interpolate.CubicSpline`                                |
|  [02]   | `PchipInterpolator(x, y, axis=0, extrapolate=None)`                 | monotonic cubic spline | shape-preserving PCHIP; drop-in for `scipy.interpolate.PchipInterpolator`                   |
|  [03]   | `Akima1DInterpolator(x, y, axis=0)`                                 | Akima spline           | Akima piecewise-cubic; drop-in for `scipy.interpolate.Akima1DInterpolator`                  |
|  [04]   | `CubicHermiteSpline(x, y, dydx, axis=0, extrapolate=None)`          | Hermite cubic spline   | cubic Hermite from values + derivatives; drop-in for `scipy.interpolate.CubicHermiteSpline` |
|  [05]   | `PPoly(c, x, extrapolate=None, axis=0)`                             | piecewise polynomial   | breakpoint/coefficient piecewise polynomial; drop-in for `scipy.interpolate.PPoly`          |
|  [06]   | `<spline>.derivative(nu=1)` / `.antiderivative(nu=1)`               | spline calculus        | returns a new spline of the differentiated/integrated polynomial                            |
|  [07]   | `<spline>.integrate(a, b, extrapolate=None)` / `.roots()`           | spline calculus        | definite integral over `[a,b]` / real roots of the piecewise polynomial                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: one-shot interpolation
- rail: interpolation
- `method` selects the kernel: `"nearest"`, `"linear"`, `"cubic"`, `"cubic2"`, `"catmull-rom"`, `"monotonic"` (PCHIP-equivalent), `"monotonic-0"`; `extrap` controls out-of-bound behavior; `period` enables periodic axes.

| [INDEX] | [SURFACE]                                                              | [ENTRY_FAMILY] | [RAIL]                                                |
| :-----: | :--------------------------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `interp1d(xq, x, f, method, derivative, extrap, period)`               | 1D evaluate    | interpolate `f(x)` at query points `xq`               |
|  [02]   | `interp2d(xq, yq, x, y, f, method, derivative, extrap, period)`        | 2D evaluate    | interpolate `f(x, y)` on a regular grid at `(xq, yq)` |
|  [03]   | `interp3d(xq, yq, zq, x, y, z, f, method, derivative, extrap, period)` | 3D evaluate    | interpolate `f(x, y, z)` on a regular grid            |

[ENTRYPOINT_SCOPE]: reusable interpolant construction
- rail: interpolation
- `method` accepts the same kernel vocabulary as the one-shot family; the constructed object is called with query points and supports a `derivative` keyword on evaluation.

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY] | [RAIL]                                                  |
| :-----: | :--------------------------------------------------- | :------------- | :------------------------------------------------------ |
|  [01]   | `Interpolator1D(x, f, method, extrap, period)`       | 1D construct   | build a reusable 1D interpolant; call with `xq`         |
|  [02]   | `Interpolator2D(x, y, f, method, extrap, period)`    | 2D construct   | build a reusable 2D interpolant; call with `xq, yq`     |
|  [03]   | `Interpolator3D(x, y, z, f, method, extrap, period)` | 3D construct   | build a reusable 3D interpolant; call with `xq, yq, zq` |

[ENTRYPOINT_SCOPE]: FFT-based periodic resampling
- rail: interpolation
- spectral resampling for uniformly sampled periodic data; query points are evaluated via the trigonometric interpolant of the input samples.

| [INDEX] | [SURFACE]                                 | [ENTRY_FAMILY] | [RAIL]                                                |
| :-----: | :---------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `fft_interp1d(f, n, sx, dx)`              | 1D spectral    | FFT-interpolate uniformly sampled periodic `f` to `n` |
|  [02]   | `fft_interp2d(f, n1, n2, sx, sy, dx, dy)` | 2D spectral    | FFT-interpolate uniformly sampled periodic 2D `f`     |

## [04]-[IMPLEMENTATION_LAW]

[INTERP_TOPOLOGY]:
- namespace: `interpax`; the `interp{1,2,3}d` one-shot family, the `Interpolator{1,2,3}D` reusable objects, the `fft_interp{1,2}d` spectral family, and the scipy-equivalent spline classes all live at top level.
- one-shot vs reusable: `interp1d(xq, x, f, method=...)` is a pure function recompiled per call signature; `Interpolator1D(x, f, method=...)` constructs ONCE and is then called with query points — preferred inside a hot loop or when the same grid is re-queried, because construction (coefficient fit) is hoisted out of the trace.
- the `method` vocabulary is shared across the one-shot and reusable families: `"nearest"`, `"linear"`, `"cubic"`, `"cubic2"`, `"catmull-rom"`, `"monotonic"` (PCHIP), `"monotonic-0"`; `extrap` controls out-of-bound behavior; `period` enables periodic axes.
- every interpolant is a JAX pytree module: it is `jit`/`vmap`/`grad`-compatible with respect to BOTH the query points and the sampled values `f`, so gradients flow through an interpolated field back to the data that defined it.

[SIBLING_INTEGRATION]:
- `equinox`: an `Interpolator{1,2,3}D` is an Equinox-style pytree module, so it is a differentiable leaf inside an `equinox.Module` objective — an interpolated material/field parameter is partitioned and stepped by `optax`/`optimistix` exactly like a learned weight, with no boundary conversion.
- `quadax`: an interpolant or spline `__call__` is a valid `quadax.quadgk`/`quadcc` integrand; `<spline>.integrate(a, b)` gives the exact piecewise-polynomial definite integral when the integrand IS the spline, avoiding a quadrature call.
- `diffrax`: an interpolant evaluated inside an `ODETerm` vector field supplies a differentiable time-/space-varying forcing; `<spline>.derivative()` supplies its analytic time-derivative without finite differencing.
- `jax`: `interp{1,2,3}d` compose under `jax.vmap` to batch over independent grids and under `jax.grad` for sensitivity of an interpolated quantity; `fft_interp{1,2}d` is the periodic-data counterpart to `jax.numpy.fft`-based resampling.
- `findiff`: the finite-difference rail is the NON-differentiable fallback; where the JAX interpolant is available, `interpax` (analytic derivatives via `.derivative()`) is preferred over a `findiff` stencil for the same field.

[LOCAL_ADMISSION]:
- a re-queried regular grid is fitted once into an `Interpolator{1,2,3}D` outside the trace; `interp{1,2,3}d` is the one-shot form for a single evaluation.
- periodic/uniformly-sampled data is resampled through `fft_interp{1,2}d`, not a manual FFT-pad-IFFT loop.
- a definite integral of a fitted spline uses `<spline>.integrate(a, b)` (exact); a `quadax` call is reserved for non-spline integrands.

[RAIL_LAW]:
- Package: `interpax`
- Owns: JAX-native differentiable interpolation on regular grids (linear/cubic/PCHIP/monotonic/Catmull-Rom), reusable callable interpolant pytree objects, drop-in differentiable `scipy.interpolate` spline classes with analytic calculus methods, and FFT-based periodic resampling
- Accept: regular-grid interpolation routed through `interp{1,2,3}d` (one-shot) or `Interpolator{1,2,3}D` (reusable `vmap`/`grad`/`jit`-compatible interpolant), `method` selecting the kernel, the scipy-equivalent spline classes for analytic derivative/antiderivative/integrate/roots, and `fft_interp{1,2}d` for periodic spectral resampling
- Usage: `[INTERPAX_QUADAX_USAGE]` (deferred, compute solvers/quadrature and solvers/field) — [BLOCKED]

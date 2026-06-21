# [PY_COMPUTE_API_INTERPAX]

`interpax` supplies JAX-native, differentiable, `vmap`-friendly 1D/2D/3D interpolation and FFT-based resampling for the compute interpolation rail. The package owner routes regular-grid interpolation cases through `interp1d`/`interp2d`/`interp3d` for one-shot evaluation, the `Interpolator{1,2,3}D` callable objects for reusable JAX-differentiable interpolants, `fft_interp1d`/`fft_interp2d` for periodic spectral resampling, plus drop-in JAX-differentiable equivalents of `scipy.interpolate.CubicSpline`/`PchipInterpolator`/`Akima1DInterpolator`/`CubicHermiteSpline`/`PPoly` as first-class spline classes; it never re-implements a cubic-spline or PCHIP kernel the package already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `interpax`
- package: `interpax`
- import: `interpax`
- owner: `compute`
- rail: interpolation
- installed: `0.3.14` authored from ledger ([04]-sourced; `assay api` resolution blocked by the workspace `opentelemetry-proto` `protobuf>=5,<7` ceiling against the `protobuf>=7.35` floor — a workspace dependency-graph conflict, not an interpax/interpreter/wheel fault); license MIT; pure-Python but `jax`-dependent and marker-gated `python_version<'3.15'` (jaxlib ships no cp315 wheel)
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
- JAX-differentiable, `jit`/`vmap`/`grad`-compatible drop-in replacements for the `scipy.interpolate` spline classes; same constructor and call signatures, evaluated through the JAX-native kernels instead of SciPy.

| [INDEX] | [SYMBOL]              | [PACKAGE_ROLE]         | [CAPABILITY]                                                                                |
| :-----: | :-------------------- | :--------------------- | :------------------------------------------------------------------------------------------ |
|  [01]   | `CubicSpline`         | cubic spline           | C2 cubic spline; drop-in for `scipy.interpolate.CubicSpline`                                |
|  [02]   | `PchipInterpolator`   | monotonic cubic spline | shape-preserving PCHIP; drop-in for `scipy.interpolate.PchipInterpolator`                   |
|  [03]   | `Akima1DInterpolator` | Akima spline           | Akima piecewise-cubic; drop-in for `scipy.interpolate.Akima1DInterpolator`                  |
|  [04]   | `CubicHermiteSpline`  | Hermite cubic spline   | cubic Hermite from values + derivatives; drop-in for `scipy.interpolate.CubicHermiteSpline` |
|  [05]   | `PPoly`               | piecewise polynomial   | breakpoint/coefficient piecewise polynomial; drop-in for `scipy.interpolate.PPoly`          |

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

[RAIL_LAW]:
- Package: `interpax`
- Owns: JAX-native differentiable interpolation on regular grids (linear/cubic/PCHIP/monotonic), reusable callable interpolant objects, and FFT-based periodic resampling
- Accept: regular-grid interpolation routed through `interp{1,2,3}d` for one-shot evaluation or `Interpolator{1,2,3}D` for a reusable `vmap`/`grad`/`jit`-compatible interpolant, `method` selecting the kernel, and `fft_interp{1,2}d` for periodic spectral resampling
- Reject: hand-rolled cubic-spline or PCHIP kernels; interpax in any product runtime path on cp315 (jaxlib ships no cp315 wheel); a non-differentiable interpolation shim where the JAX-native interpolant is available
- Usage: `[INTERPAX_QUADAX_USAGE]` (deferred, compute solvers/quadrature and solvers/field) — [BLOCKED]

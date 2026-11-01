# [NUMERICS]

Covers numeric passes through package members, from solvers and grid operators to buffer ownership, floating-point scopes, random streams, and tables.

## [01]-[PACKAGE_MEMBERS]

Numeric facts are members of referenced packages, and a pass calls its fact's member over a whole array:

| [INDEX] | [FACT]                                      | [MEMBER]                                                                         |
| :-----: | :------------------------------------------ | :------------------------------------------------------------------------------- |
|  [01]   | Root per array element                      | `elementwise.bracket_root`, then `elementwise.find_root`                         |
|  [02]   | Minimum, least squares                      | `sp.optimize.minimize`, `sp.optimize.least_squares`                              |
|  [03]   | Integral per array element                  | `sp.integrate.tanhsinh`                                                          |
|  [04]   | Vector or multivariate integral             | `sp.integrate.quad_vec`, `sp.integrate.cubature`                                 |
|  [05]   | Infinite series                             | `sp.integrate.nsum`                                                              |
|  [06]   | Initial value problem                       | `sp.integrate.solve_ivp`                                                         |
|  [07]   | Interpolating spline                        | `sp.interpolate.make_interp_spline`                                              |
|  [08]   | Recurrence `y[i] = a * x[i] + b * y[i - 1]` | `sp.signal.lfilter([a], [1, -b], x)`                                             |
|  [09]   | Prefix sum, running extreme                 | `np.cumulative_sum`, `ufunc.accumulate`                                          |
|  [10]   | Grouped sum or count                        | `np.bincount(keys, weights=..., minlength=n)`                                    |
|  [11]   | Window statistic                            | `sp.ndimage` filter, else a `np.lib.stride_tricks.sliding_window_view` reduction |
|  [12]   | Derivative at any order                     | `findiff.Diff(axis, spacing) ** order`                                           |

Loops that converge, bisect, step, or sum a series are calls to the member for their fact:
- Elementwise members (`find_root`, `tanhsinh`, `nsum`) take arrays of brackets and `args`, and solve every element in one call
- Elementwise results hold per-element `success` and `status` arrays, a failed element reads `nan` beside its solved siblings
- `elementwise` imports as `from scipy.optimize import elementwise`, every other member reads through `import scipy as sp`
- Code under `jax.jit`, `jax.grad`, or `jax.vmap` takes `optimistix.root_find`, `quadax.quadgk`, and `diffrax.diffeqsolve`
- Traced linear solves, scans, and grouped sums take `lineax.linear_solve`, `jax.lax.associative_scan`, and `jax.ops.segment_sum`
- Quantities with units enter as `pint.Quantity`, and `quantity.m_as(<unit>)` hands a member its magnitude

`Fault`, `Reason`, and `Outcome` are the package faults, and `crossing` reads both failures of a bracketed search through one builder:

```python
def crossing(curve: Callable[[float], float], low: float, high: float, /) -> Outcome[float]:
    missed = partial(Fault.of, Reason.ABSENT, "curve", member=f"root on [{low}, {high}]")
    return (
        catch(sp.optimize.root_scalar, exception=ValueError)(curve, bracket=(low, high))
        .map_error(missed)
        .filter_with(lambda found: found.converged, missed)
        .map(lambda found: found.root)
    )
```

`crossing(math.cos, 0.0, 3.0)` returns `Ok(1.5707963267948966)`. A bracket with no sign change raises `ValueError` and becomes its fault's cause, and a search that stops unconverged keeps its `RootResults` as cause.

## [02]-[OPERATORS]

Linear operators over a grid are package values, and evolution under one operator is one call. `Samples` is `npt.NDArray[np.float64]`, `LaplacianNd` builds the discrete Laplacian of a grid of any dimension, and `expm_multiply` applies its exponential at evenly spaced times:

```python
def diffused(heat: Samples, span: float, count: int, /) -> Samples:
    laplacian = sp.sparse.linalg.LaplacianNd(heat.shape, dtype=heat.dtype).tosparse()
    return sp.sparse.linalg.expm_multiply(laplacian, heat.ravel(), start=0.0, stop=span, num=count).reshape((count, *heat.shape))
```

`diffused(heat, span, count)` returns `count` snapshots from `heat` to `span`, diffusivity times time over squared spacing, for a rod, plate, or volume alike:
- `expm_multiply` holds its accuracy at any `span`, with no step size or stability bound
- `boundary_conditions="neumann"` (default) insulates every edge and conserves the total
- `boundary_conditions="dirichlet"` holds zero past every edge, and `"periodic"` joins opposite edges
- `tosparse()` hands `expm_multiply` the trace it reads, and a bare `LaplacianNd` estimates it with a warning

## [03]-[OWNERSHIP]

Basic slices are views of their base, and fancy and boolean indexing return copies. Passes return a new array, arrays a function receives stay unwritten, and `out=` targets a buffer the pass allocated:

```python
def filled(samples: Samples, /) -> Samples:
    known = np.flatnonzero(~np.isnan(samples))
    return np.interp(np.arange(samples.size), known, samples[known])
```

`filled` interpolates each `NaN` between its valid neighbors and returns every valid sample exactly, `samples[known]` reads a copy, and `samples` keeps its gaps. Each operation returns a view or a copy:

| [INDEX] | [OPERATION]                                | [RESULT]                |
| :-----: | :----------------------------------------- | :---------------------- |
|  [01]   | Basic slice (`a[1:]`, `a[:, ::2]`), `.T`   | View                    |
|  [02]   | `np.asarray` over an array of its dtype    | Same array              |
|  [03]   | Fancy or boolean index read                | Copy                    |
|  [04]   | `reshape`, `ravel`                         | View or copy by strides |
|  [05]   | `flatten`, `copy`, `astype`                | Copy                    |
|  [06]   | `astype(<dtype>, copy=False)` at its dtype | Same array              |
|  [07]   | `sliding_window_view`                      | Read-only view          |

Writes reach their base by form:
- Functions returning a view alias their caller's array, a later write by either side changes both
- Writes reach `a` through one index expression on `a` itself (`a[mask] = value`)
- Ufuncs buffer overlapping operands, `np.add(a[:-1], a[1:], out=a[1:])` over an owned `a` returns sums of the original values

## [04]-[FLOATING_POINT]

Floating-point failures warn by default and return `inf` or `nan`. Scopes nest per call, and a helper ignoring one condition keeps ignoring it inside a caller raising every other one:

```python
@np.errstate(under="ignore")
def _falloff(distance: Samples, /) -> Samples:
    return np.exp(-distance * distance)


@catch(exception=FloatingPointError)
@np.errstate(all="raise")
def _glow(distance: Samples, weight: Samples, /) -> Samples:
    return _falloff(distance) / weight
```

`_glow` returns `Ok` holding zero for a distant sample with an underflowing `exp` and an `Error` holding `FloatingPointError` for a zero weight. Each `errstate` keyword names one error kind:
- `divide` covers division by zero and `log(0)`, `invalid` covers `0 / 0`, `inf - inf`, and `sqrt` of a negative
- `over` covers float overflow and integer scalar overflow (`np.int8(127) + np.int8(1)`)
- Integer array arithmetic wraps under `all="raise"` and raises nothing
- `np.mean` over an empty array warns and raises `FloatingPointError` under `invalid="raise"`
- Scopes are context variables, anyio tasks and `to_thread.run_sync` calls run under their caller's scope

## [05]-[RANDOM_STREAMS]

Random draws read a `numpy.random.Generator` derived from what they depend on, a content digest as entropy and a path of integers as `spawn_key`:

```python
def stream(key: bytes, /, *path: int) -> np.random.Generator:
    return np.random.default_rng(np.random.SeedSequence(int.from_bytes(key), spawn_key=path))


def trials(key: bytes, points: Samples, scale: float, count: int, /) -> tuple[Samples, ...]:
    return tuple(stream(key, index).normal(points, scale) for index in range(count))
```

`trials(key, points, scale, 2)` equals `trials(key, points, scale, 3)[:2]`, and equal keys draw equal samples in any run and call order:
- `stream(key, *path)` equals the child `Generator.spawn` returns at that path, and `stream(key, 1, 0)` equals `stream(key, 1).spawn(1)[0]`
- `normal(points, scale)` broadcasts its location over `points`, one draw per element
- Calls drawing a whole shape read their stream in C order, `normal(size=(2, 3))` equals six scalar draws in any split
- Consumers take a `Generator` argument, and callers derive each stream from the key and path its draws depend on

## [06]-[TABLES]

Module tables hold one row per tier, and a pass indexes them with a whole array of keys. `np.digitize` returns each reading's tier from the floor column, and `weighted` reads its weight from column 1:

```python
TIERS: Samples = np.array(((-np.inf, 0.0), (3.2, 0.5), (3.6, 1.0)))
TIERS.setflags(write=False)


def weighted(volts: Samples, /) -> Samples:
    return TIERS[:, 1][np.digitize(volts, TIERS[:, 0]) - 1]
```

`weighted` maps each reading to its tier weight in one pass, and a value on a floor falls in that floor's tier:
- `NaN` readings sort past every floor and read the top tier's weight
- Writes into a locked table raise `ValueError` from item assignment, `out=`, and `fill` alike
- Views of a locked table (`TIERS[:, 0]`) are read-only, and `setflags(write=True)` on one raises `ValueError`
- `weighted` returns a writable copy its caller owns

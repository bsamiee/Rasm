# [NUMERICS]

Covers whole-array numeric passes.

## [01]-[PACKAGE_MEMBERS]

Select the member by the fact a pass computes:

| [INDEX] | [FACT]                                      | [MEMBER]                                                                                 |
| :-----: | :------------------------------------------ | :--------------------------------------------------------------------------------------- |
|  [01]   | Root or minimum per array element           | `elementwise.bracket_root` then `find_root`, `bracket_minimum` then `find_minimum`       |
|  [02]   | Derivative of a callable                    | `sp.differentiate.derivative` per element, `jacobian`, `hessian`                         |
|  [03]   | Derivative of samples at any order          | `findiff.Diff(axis, spacing) ** order`                                                   |
|  [04]   | Integral or series per array element        | `sp.integrate.tanhsinh`, `sp.integrate.nsum`                                             |
|  [05]   | Vector or multivariate integral             | `sp.integrate.quad_vec`, `sp.integrate.cubature`                                         |
|  [06]   | Running area under samples                  | `sp.integrate.cumulative_trapezoid`                                                      |
|  [07]   | Interpolating spline                        | `sp.interpolate.make_interp_spline`                                                      |
|  [08]   | Smoothing spline with no tuned penalty      | `sp.interpolate.make_smoothing_spline`                                                   |
|  [09]   | Polynomial fit, antiderivative              | `np.polynomial.Polynomial.fit`, `Polynomial.integ`                                       |
|  [10]   | Recurrence `y[i] = a * x[i] + b * y[i - 1]` | `sp.signal.lfilter([a], [1, -b], x)`                                                     |
|  [11]   | Recurrence with any other step              | `jax.lax.scan(step, carry, xs)` under `jax.jit`                                          |
|  [12]   | Sum over any span, running extreme          | `np.cumulative_sum(include_initial=True)` differenced at span bounds, `ufunc.accumulate` |
|  [13]   | Grouped sum or count                        | `np.bincount(keys, weights=..., minlength=n)`, keys from `np.unique_inverse(labels)`     |
|  [14]   | Grouped extreme                             | `np.maximum.at(<filled buffer>, keys, values)`                                           |
|  [15]   | Window statistic                            | Named `sp.ndimage` filter or `sp.ndimage.vectorized_filter(values, <reduction>, size=n)` |
|  [16]   | Row-wise dot, stacked matrix-vector product | `np.vecdot`, `np.matvec`                                                                 |

Loops that converge, bisect, step, or sum a series are calls to the member for their fact, and `roots` expands a bracket from each guess and refines every element in one call each:

```python
type Samples = npt.NDArray[np.float64]


def roots[*Ts](curve: Callable[[Samples, *Ts], Samples], guess: Samples, /, *args: *Ts) -> Outcome[Samples]:
    found = elementwise.find_root(curve, elementwise.bracket_root(curve, guess, args=args).bracket, args=args)
    return Ok(found).filter_with(lambda result: bool(result.success.all()), partial(Fault.of, Reason.ABSENT, "curve", member="root")).map(lambda result: np.asarray(result.x))
```

`roots(lambda x, level: np.cos(x) - level, np.zeros(3), levels)` solves `cos(x) = level` for every level at once, and a level past 1 fails the pass:
- Per-element parameters enter through `args`, broadcast against the guesses
- Results hold per-element `success` and `status`, a failed element reads `nan` beside its solved siblings
- Refused results stay the fault's cause
- `elementwise` imports as `from scipy.optimize import elementwise`, every other member reads through `import scipy as sp`
- Code under `jax.jit`, `jax.grad`, or `jax.vmap` takes `optimistix.root_find`, `quadax.quadgk`, `diffrax.diffeqsolve`, `lineax.linear_solve`, and `jax.ops.segment_sum`
- JAX computes `float64` input in `float32` until `jax.config.update("jax_enable_x64", True)` runs at startup
- Quantities with units enter as `pint.Quantity`, and `quantity.m_as(<unit>)` hands a member its magnitude

## [02]-[OPERATORS]

Linear operators over a grid are package values, and evolution under one operator is one call. `LaplacianNd` builds the discrete Laplacian of a grid of any dimension, and `expm_multiply` applies its exponential at evenly spaced times:

```python
def diffused(heat: Samples, span: float, count: int, /) -> Samples:
    laplacian = sp.sparse.linalg.LaplacianNd(heat.shape, dtype=heat.dtype).tosparse()
    return sp.sparse.linalg.expm_multiply(laplacian, heat.ravel(), start=0.0, stop=span, num=count).reshape((count, *heat.shape))
```

`diffused(heat, span, count)` returns `count` snapshots from `heat` to `span`, diffusivity times time over squared spacing, for a rod, plate, or volume alike:
- `expm_multiply` holds its accuracy at any `span`, with no step size or stability bound
- `boundary_conditions="neumann"` (default) insulates every edge and conserves the total, `"dirichlet"` holds zero past every edge, and `"periodic"` joins opposite edges
- `tosparse()` hands `expm_multiply` the trace it reads

## [03]-[OWNERSHIP]

Passes return a new array, arrays a function receives stay unwritten, and `out=` targets a buffer the pass allocated:

```python
def kernel(offsets: Samples, width: float, /) -> Samples:
    weights = np.exp(-0.5 * np.square(offsets / width))
    return np.divide(weights, weights.sum(axis=-1, keepdims=True), out=weights)
```

`kernel` divides the one array its `exp` allocated in place, each row of weights sums to 1, and `offsets` stays unwritten. Each call states whether its result aliases its input:
- `np.asarray(<x>, copy=False)` and `reshape(<shape>, copy=False)` return a view or raise `ValueError`, and the default `copy=None` copies only where it must
- `astype(<dtype>, copy=False)` returns the array itself when its dtype already matches
- `sliding_window_view` and `broadcast_to` return read-only views
- Writes reach `a` through one index expression on `a` itself (`a[mask] = value`)
- Ufuncs buffer overlapping operands, `np.add(a[:-1], a[1:], out=a[1:])` over an owned `a` returns sums of the original values

## [04]-[FLOATING_POINT]

Floating-point failures warn by default and return `inf` or `nan`. Scopes nest per call, and a helper ignoring one condition keeps ignoring it inside a caller raising every other one:

```python
@np.errstate(under="ignore")
def _falloff(distance: Samples, /) -> Samples:
    return np.exp(-distance * distance)


@caught(FloatingPointError)
@np.errstate(all="raise")
def glow(distance: Samples, weight: Samples, /) -> Samples:
    return _falloff(distance) / weight
```

`glow` returns `Ok` holding zero for a distant sample with an underflowing `exp` and fails with `glow failed. divide by zero encountered in divide` for a zero weight. Each `errstate` keyword names one error kind:
- `divide` covers division by zero and `log(0)`, `invalid` covers `0 / 0`, `inf - inf`, and `sqrt` of a negative
- `over` covers float overflow and integer scalar overflow (`np.int8(127) + np.int8(1)`)
- Integer array arithmetic wraps under `all="raise"` and raises nothing
- Scopes are context variables, anyio tasks and `to_thread.run_sync` calls run under their caller's scope

## [05]-[RANDOM_STREAMS]

Random draws read a `numpy.random.Generator` derived from what they depend on, a content digest as entropy and a path of integers as `spawn_key`:

```python
def stream(key: bytes, /, *path: int) -> np.random.Generator:
    return np.random.default_rng(np.random.SeedSequence(int.from_bytes(key), spawn_key=path))
```

`stream(key, index).normal(points, scale)` draws trial `index` alike under any trial count, and equal keys draw equal samples in any run and call order:
- `stream(key, *path)` equals the child `Generator.spawn` returns at that path, and `stream(key, 1, 0)` equals `stream(key, 1).spawn(1)[0]`
- Calls drawing a whole shape read their stream in C order, `normal(size=(2, 3))` equals six scalar draws in any split

## [06]-[TABLES]

Module-level arrays lock with `setflags(write=False)` once built, and a tier table holds one row per tier a pass indexes with a whole array of keys. `np.digitize` returns each reading's tier from the floor column, and `weighted` reads its weight from column 1:

```python
TIERS: Samples = np.array(((-np.inf, 0.0), (3.2, 0.5), (3.6, 1.0)))
TIERS.setflags(write=False)


def weighted(volts: Samples, /) -> Samples:
    return TIERS[:, 1][np.digitize(volts, TIERS[:, 0]) - 1]
```

`weighted` maps each reading to its tier weight in one pass, and a value on a floor falls in that floor's tier:
- `NaN` readings sort past every floor and read the top tier's weight
- Writes into a locked table or its views raise `ValueError` from item assignment, `out=`, `fill`, and `setflags(write=True)` alike
- `weighted` returns a writable copy its caller owns

# [NUMERICS]

Covers numpy array passes, from stencils over shifted slices and buffer ownership to floating-point scopes, random streams, and module tables.

## [01]-[STENCILS]

Stencils read each neighbor as a shifted slice of their input. Slices `heat[:-2]` and `heat[2:]` hold every left and right neighbor of `heat[1:-1]`, and in-place operators on an `inner` view write a buffer the pass allocated:

```python
type Samples = NDArray[np.float64]


def diffused(heat: Samples, ratio: float, /) -> Samples:
    following = heat.copy()
    inner = following[1:-1]
    np.add(heat[:-2], heat[2:], out=inner)
    inner -= 2.0 * heat[1:-1]
    inner *= ratio
    inner += heat[1:-1]
    return following
```

`diffused` takes one explicit diffusion step over a rod with fixed ends. `heat.copy()` keeps both boundary values in `following`, `out=inner` writes a neighbor sum with no temporary array, and `heat` stays unwritten. Each step reads its predecessor's whole result, a loop-carried recurrence, and steps run as a fold over whole-array passes:

```python
def evolved(heat: Samples, ratio: float, steps: int, /) -> Samples:
    return reduce(lambda state, _: diffused(state, ratio), range(steps), heat)
```

Recurrences and windows keep a whole-array form where a member states one:
- Recurrences inside one pass (`y[i] = a * x[i] + b * y[i - 1]`) stay a loop unless a package states them (`lfilter([a], [1, -b], x)`)
- Ufuncs buffer overlapping operands, `np.add(a[:-1], a[1:], out=a[1:])` returns sums of the original values
- Window reductions with no numpy or scipy member reduce `sliding_window_view(a, width)` over `axis=-1`
- `sliding_window_view` returns a read-only view of every window and copies nothing before its reduction

## [02]-[OWNERSHIP]

Basic slices are views of their base, and fancy and boolean indexing return copies. Arrays a function receives stay unwritten, a pass that writes allocates or copies its own buffer first, and `out=` targets its own buffer alone:

```python
def filled(samples: Samples, /) -> Samples:
    missing = np.isnan(samples)
    result = samples.copy()
    result[missing] = np.interp(np.flatnonzero(missing), np.flatnonzero(~missing), samples[~missing])
    return result
```

`filled` replaces each `NaN` by linear interpolation between its valid neighbors. `samples[~missing]` reads a copy, `result[missing] = ...` writes the owned copy in place, and `samples` keeps its gaps. `np.shares_memory(a, b)` proves aliasing, and each operation returns a view or a copy:

| [INDEX] | [OPERATION]                                | [RESULT]                       |
| :-----: | :----------------------------------------- | :----------------------------- |
|  [01]   | Basic slice (`a[1:]`, `a[:, ::2]`), `.T`   | View                           |
|  [02]   | `np.asarray` over an array of its dtype    | Same array                     |
|  [03]   | Fancy or boolean index read                | Copy                           |
|  [04]   | `reshape`, `ravel`                         | View where strides allow, copy |
|  [05]   | `flatten`, `copy`, `astype`                | Copy                           |
|  [06]   | `astype(<dtype>, copy=False)` at its dtype | Same array                     |
|  [07]   | `sliding_window_view`                      | Read-only view                 |

Writes through views and indexes reach their base by form:
- Functions returning a view alias their caller's array, a later write by either side changes both
- Chained indexing writes a temporary, `a[mask][0] = value` changes nothing in `a`
- Fancy-index updates apply each index once, `a[[0, 0]] += 1` adds 1 where `np.add.at(a, [0, 0], 1)` adds 2
- `np.bincount(indexes, minlength=n)` counts repeated indexes in one call

## [03]-[FLOATING_POINT]

Floating-point failures warn by default and return `inf` or `nan`. Scopes raising them are `np.errstate` on one named `def`, and `@catch` above it converts `FloatingPointError` into a `Result`. `Fault`, `Outcome`, and `Reason` are the package faults:

```python
@catch(exception=FloatingPointError)
@np.errstate(divide="raise", invalid="raise")
def ratios(numerator: Samples, denominator: Samples, /) -> Samples:
    return numerator / denominator


def efficiency(output: Samples, supply: Samples, /) -> Outcome[Samples]:
    return ratios(output, supply).map_error(lambda error: (Fault(reason=Reason.INVALID, subject="supply", cause=error),))
```

`np.errstate` sits below `catch`, and each call of `ratios` runs its body inside a fresh scope. `efficiency` returns `Ok` over nonzero supply and an `INVALID` fault for a zero. Each `errstate` keyword names one error kind:
- `divide` covers division by zero and `log(0)`, `invalid` covers `0 / 0`, `inf - inf`, and `sqrt` of a negative
- `over` covers float overflow and integer scalar overflow (`np.int8(127) + np.int8(1)`)
- Integer array arithmetic wraps unseen even under `all="raise"`
- `np.mean` over an empty array warns and raises `FloatingPointError` under `invalid="raise"`
- Scopes hold per thread, calls from a thread pool raise in their own call alone and leave `np.geterr()` unchanged

## [04]-[RANDOM_STREAMS]

Random draws take a `numpy.random.Generator` argument, and callers derive one child stream per independent consumer from a seed:

```python
def jittered(rng: np.random.Generator, points: Samples, scale: float, /) -> Samples:
    return points + rng.normal(0.0, scale, points.shape)


def trials(seed: int, points: Samples, scale: float, count: int, /) -> tuple[Samples, ...]:
    return tuple(jittered(stream, points, scale) for stream in np.random.default_rng(seed).spawn(count))
```

`trials(seed, points, scale, 2)` equals `trials(seed, points, scale, 3)[:2]`, and a child stream depends on its seed and index alone:
- `Generator.spawn` advances its parent, a second `spawn(1)` returns a new child, and draws from a parent change no child
- One call drawing a whole shape reads its stream in C order, `normal(size=(2, 3))` equals six scalar draws in any split
- Children spawned once at the root reach every consumer as arguments, a consumer spawns no stream of its own

Streams that stay fixed as consumers are added or reordered derive from a declared key through `SeedSequence(seed, spawn_key=...)`:

```python
class Axis(IntEnum):
    X = 0
    Y = 1
    Z = 2


def stream(seed: int, axis: Axis, /) -> np.random.Generator:
    return np.random.default_rng(np.random.SeedSequence(seed, spawn_key=(axis,)))
```

`stream(seed, Axis.Y)` equals `default_rng(seed).spawn(3)[1]`, and adding a member to `Axis` leaves every existing stream unchanged.

## [05]-[TABLES]

Module tables hold breakpoints and values that a pass indexes with a whole array of keys. `np.searchsorted(BREAKS, volts, side="right")` returns the index `bisect` returns for each value:

```python
BREAKS = np.array((3.2, 3.6))
WEIGHTS = np.array((0.0, 0.5, 1.0))
BREAKS.setflags(write=False)
WEIGHTS.setflags(write=False)


def weighted(volts: Samples, /) -> Samples:
    return WEIGHTS[np.searchsorted(BREAKS, volts, side="right")]
```

`weighted` maps each reading to its tier weight in one pass, and values on a breakpoint fall in the upper tier:
- Writes into a locked table raise `ValueError` (assignment destination is read-only), from item assignment, `out=`, and `fill` alike
- Views of a locked table are read-only, and `setflags(write=True)` on one raises `ValueError`
- Fancy indexing returns a writable copy, and `weighted` hands its caller an array the caller owns

# [PY_COMPUTE_API_ARRAY_API_EXTRA]

`array-api-extra` owns the Array API extension operations the base standard omits, the `at` write-side indexed-update builder, and the lazy-dispatch wrapper, every op parametric over an `xp` namespace so one call runs backend-agnostic across NumPy, JAX, Torch, and Dask. It rides the extension tier directly above the `xp` namespace `array-api-compat` resolves.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `array-api-extra`
- package: `array-api-extra` (MIT)
- module: `array_api_extra`
- rail: Array API extension tier over the resolved `xp` namespace

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: extension operations, the `at` builder, and test helpers

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [CAPABILITY]               |
| :-----: | :----------------- | :------------ | :------------------------- |
|  [01]   | `at`               | class         | indexed-update builder     |
|  [02]   | `apply_where`      | function      | masked two-branch apply    |
|  [03]   | `argpartition`     | function      | partial-sort indices       |
|  [04]   | `atleast_nd`       | function      | minimum-ndim broadcast     |
|  [05]   | `broadcast_shapes` | function      | broadcast shape            |
|  [06]   | `cov`              | function      | covariance matrix          |
|  [07]   | `create_diagonal`  | function      | diagonal from 1-D          |
|  [08]   | `default_dtype`    | function      | backend default dtype      |
|  [09]   | `expand_dims`      | function      | axis insertion             |
|  [10]   | `isclose`          | function      | approximate equality       |
|  [11]   | `isin`             | function      | membership test            |
|  [12]   | `kron`             | function      | Kronecker product          |
|  [13]   | `lazy_apply`       | function      | lazy graph dispatch        |
|  [14]   | `nan_to_num`       | function      | NaN/inf fill               |
|  [15]   | `nunique`          | function      | distinct count             |
|  [16]   | `one_hot`          | function      | one-hot encoding           |
|  [17]   | `pad`              | function      | constant-mode padding      |
|  [18]   | `partition`        | function      | partial sort along axis    |
|  [19]   | `searchsorted`     | function      | sorted-array binary search |
|  [20]   | `setdiff1d`        | function      | 1-D set difference         |
|  [21]   | `sinc`             | function      | normalized sinc            |
|  [22]   | `union1d`          | function      | sorted 1-D union           |
|  [23]   | `angle`            | function      | complex phase angle        |
|  [24]   | `testing`          | module        | lazy-backend test helpers  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: extension operations
- `xp` carry: every op takes a trailing `*, xp=None` namespace override, elided below; `default_dtype` alone takes `xp` positionally.

| [INDEX] | [SURFACE]                                                                   | [SHAPE] | [CAPABILITY]                      |
| :-----: | :-------------------------------------------------------------------------- | :------ | :-------------------------------- |
|  [01]   | `at(x, idx=UNDEF, /)` then `.<verb>(y, /, copy=None) -> Array`              | factory | JAX-style indexed write builder   |
|  [02]   | `apply_where(cond, args, f1, f2=None, /, *, fill_value=None, kwargs=None)`  | static  | mask-guarded two-branch dispatch  |
|  [03]   | `argpartition(a, kth, /, axis=-1)`                                          | static  | indirect partial sort             |
|  [04]   | `atleast_nd(x, /, *, ndim)`                                                 | static  | broadcast to minimum ndim         |
|  [05]   | `broadcast_shapes(*shapes) -> tuple[int \| None, ...]`                      | static  | broadcast shape computation       |
|  [06]   | `cov(m, /)`                                                                 | static  | covariance matrix                 |
|  [07]   | `create_diagonal(x, /, *, offset=0)`                                        | static  | diagonal matrix from 1-D          |
|  [08]   | `default_dtype(xp, kind='real floating', *, device=None)`                   | static  | backend canonical dtype query     |
|  [09]   | `expand_dims(a, /, *, axis=(0,))`                                           | static  | insert axes                       |
|  [10]   | `isclose(a, b, *, rtol=1e-5, atol=1e-8, equal_nan=False)`                   | static  | approximate equality              |
|  [11]   | `isin(a, b, /, *, assume_unique=False, invert=False, kind=None)`            | static  | element membership test           |
|  [12]   | `kron(a, b, /)`                                                             | static  | Kronecker product                 |
|  [13]   | `lazy_apply(func, *args, shape=None, dtype=None, as_numpy=False, **kwargs)` | static  | lazy dispatch for traced backends |
|  [14]   | `nan_to_num(x, /, *, fill_value=0.0)`                                       | static  | replace NaN/inf                   |
|  [15]   | `nunique(x, /)`                                                             | static  | count unique elements             |
|  [16]   | `one_hot(x, /, num_classes, *, dtype=None, axis=-1)`                        | static  | one-hot encoding                  |
|  [17]   | `pad(x, pad_width, mode='constant', *, constant_values=0)`                  | static  | constant-mode padding             |
|  [18]   | `partition(a, kth, /, axis=-1)`                                             | static  | partial sort along axis           |
|  [19]   | `searchsorted(x1, x2, /, *, side='left')`                                   | static  | sorted binary search              |
|  [20]   | `setdiff1d(x1, x2, /, *, assume_unique=False)`                              | static  | set difference                    |
|  [21]   | `sinc(x, /)`                                                                | static  | normalized sinc                   |
|  [22]   | `union1d(a, b, /)`                                                          | static  | sorted set union                  |
|  [23]   | `angle(z, /, *, deg=False)`                                                 | static  | complex phase angle               |

- `lazy_apply` returns `Array` or `tuple[Array, ...]` per the declared `shape` arity.

[ENTRYPOINT_SCOPE]: `testing` helpers, test scope only
- assert family shares: `*, err_msg='', verbose=True, check_dtype=True, check_shape=True, check_scalar=False, xp=None`, elided below.

| [INDEX] | [SURFACE]                                                                     | [SHAPE] | [CAPABILITY]                     |
| :-----: | :---------------------------------------------------------------------------- | :------ | :------------------------------- |
|  [01]   | `testing.assert_close(actual, desired, *, rtol=None, atol=0, equal_nan=True)` | static  | approximate-equality assert      |
|  [02]   | `testing.assert_equal(actual, desired)`                                       | static  | exact-equality assert            |
|  [03]   | `testing.assert_less(x, y)`                                                   | static  | strict-less assert               |
|  [04]   | `testing.assert_close_nulp(actual, desired, *, nulp=1)`                       | static  | ULP-based assert                 |
|  [05]   | `testing.lazy_xp_function(func, *, allow_dask_compute=False, jax_jit=True)`   | static  | wrap func for lazy-backend tests |
|  [06]   | `testing.patch_lazy_xp_functions(request, monkeypatch=None, *, xp)`           | static  | patch lazy dispatch; context mgr |
|  [07]   | `testing.jax_autojit(func)`                                                   | static  | auto-`jax.jit` lazy wrapper      |
|  [08]   | `testing.pickle_flatten(obj, cls)`                                            | static  | flatten lazy pytree to leaves    |
|  [09]   | `testing.pickle_unflatten(instances, rest)`                                   | static  | rebuild pytree from leaves       |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Every op takes an optional `xp`; omitting it runs `array_namespace(*arrays)` internally, so a kernel resolves `xp` once at the boundary and threads it into every call.
- `at(x, idx)` returns a write-only builder whose terminal verbs — `set` `add` `subtract` `multiply` `divide` `power` `min` `max` — each take `(y, /, copy=None, xp=None) -> Array`; the surface carries no read side.
- `copy=None` mutates a writeable backend in place and copies a read-only one (NumPy mutates, JAX and Dask copy); `copy=True` always copies; `copy=False` mutates and raises on a read-only buffer, so it rides behind `is_writeable_array(x)`.
- `lazy_apply(func, *args, shape=, dtype=, as_numpy=)` defers `func` into a traced graph (JAX `jit`, Dask), its declared output `shape`/`dtype` letting tracing succeed; `as_numpy=True` materializes to NumPy for a non-traceable body.
- `default_dtype(xp, kind=...)` resolves a backend's canonical `real floating`/`complex floating`/`integral`/`indexing` dtype for result-buffer allocation, replacing a hardcoded `xp.float64`.

[STACKING]:
- `array-api-compat`(`.api/array-api-compat.md`): `xp = array_namespace(*arrays)` resolves the namespace once, then `array_api_extra.<op>(..., xp=xp)` threads it — one backend-agnostic rail with no per-backend branch; `is_writeable_array(x)` gates the `at(...).set(..., copy=...)` mutate-vs-copy fork, and `is_lazy_array(x)` routes the traced path to `lazy_apply` and the eager path to the direct `xp` op, the same eager/lazy fork the `diffrax`/`equinox`/`optimistix` JAX rails consume.
- within-lib (compute kernels): `at(...).set(..., copy=...)` is the single indexed-update expression spanning JAX `.at[idx].set()` and NumPy in-place assignment, and `apply_where(cond, args, f1, f2, fill_value=)` evaluates each branch only on its live mask — the graph-safe substitute for `xp.where(cond, f1(x), f2(x))` where a branch errors off its masked domain (`log` of non-positive).

[LOCAL_ADMISSION]:
- Thread the resolved `xp` into every call; a hot path never re-dispatches.
- `lazy_apply` is required where `func`'s control flow resists graph tracing (JAX, Dask); its `shape`/`dtype` declaration is what makes tracing succeed.
- `testing.*` imports under test scope alone, never a production compute owner.

[RAIL_LAW]:
- Package: `array-api-extra`
- Owns: the Array API extension operations absent from the base standard, the `at` indexed-update builder, and the lazy-dispatch wrapper
- Accept: `xp`-parametric backend-agnostic calls, `at(...).set(..., copy=...)` gated by `is_writeable_array`, `lazy_apply` gated by `is_lazy_array`
- Reject: a single-backend hand-roll of any extension pattern, a vendor `.at[]` or in-place assignment where the `at` builder spans both, a `testing` import outside test scope

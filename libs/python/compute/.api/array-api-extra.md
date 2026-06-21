# [PY_COMPUTE_API_ARRAY_API_EXTRA]

`array-api-extra` supplies the Array API Standard extension functions absent from the base standard — `at`-style indexed updates, `apply_where`, `argpartition`, `one_hot`, `pad`, `searchsorted`, `setdiff1d`, `union1d`, and covariance/diagonal/sinc/nan utilities — operating against any `xp` namespace resolved by `array-api-compat`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `array-api-extra`
- package: `array-api-extra`
- version: `0.11.0`
- license: MIT
- module: `array_api_extra`
- wheel: `py3-none-any` (pure Python, no native ABI)
- marker: ungated (depends only on `array-api-compat`; rides cp315 core)
- asset: runtime library
- rail: array-api

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: extension operations and helper types
- rail: array-api

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [ROLE]                                              |
| :-----: | :----------------- | :------------ | :-------------------------------------------------- |
|  [01]   | `at`               | class         | indexed functional update builder (`at(x, idx)`)    |
|  [02]   | `apply_where`      | function      | conditional two-branch elementwise apply            |
|  [03]   | `argpartition`     | function      | indirect partial sort indices                       |
|  [04]   | `atleast_nd`       | function      | broadcast array to at least `ndim` dimensions       |
|  [05]   | `broadcast_shapes` | function      | compute broadcast shape for shape tuples            |
|  [06]   | `cov`              | function      | covariance matrix                                   |
|  [07]   | `create_diagonal`  | function      | diagonal matrix from 1-D array with optional offset |
|  [08]   | `default_dtype`    | function      | default dtype for a kind on a namespace             |
|  [09]   | `expand_dims`      | function      | insert axes at given positions                      |
|  [10]   | `isclose`          | function      | element-wise approximate equality                   |
|  [11]   | `isin`             | function      | element-wise membership test                        |
|  [12]   | `kron`             | function      | Kronecker product                                   |
|  [13]   | `lazy_apply`       | function      | apply func lazily for graph-traced backends         |
|  [14]   | `nan_to_num`       | function      | replace NaN/inf with finite fill values             |
|  [15]   | `nunique`          | function      | count distinct values                               |
|  [16]   | `one_hot`          | function      | one-hot encode integer array                        |
|  [17]   | `pad`              | function      | constant-mode padding                               |
|  [18]   | `partition`        | function      | partial sort along axis                             |

[PUBLIC_TYPE_SCOPE]: set and search operations
- rail: array-api

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY] | [ROLE]                                     |
| :-----: | :------------- | :------------ | :----------------------------------------- |
|  [01]   | `searchsorted` | function      | sorted-array binary search                 |
|  [02]   | `setdiff1d`    | function      | set difference of two 1-D arrays           |
|  [03]   | `sinc`         | function      | normalized sinc function                   |
|  [04]   | `union1d`      | function      | sorted union of two 1-D arrays             |
|  [05]   | `angle`        | function      | element-wise phase angle of complex array  |
|  [06]   | `testing`      | module        | test helpers for array-api-extra functions |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: core array extensions
- rail: array-api

| [INDEX] | [SURFACE]                                                                                                          | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :----------------------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `at(x, idx=UNDEF, /) -> at` then `.set/.add/.subtract/.multiply/.divide/.power/.min/.max(y, /, copy=None, xp=None) -> Array` | indexed update | JAX-style functional indexed update builder |
|  [02]   | `apply_where(cond, args, f1, f2=None, /, *, fill_value=None, kwargs=None, xp=None) -> Array`                       | conditional    | two-branch element-wise dispatch       |
|  [03]   | `argpartition(a, kth, /, axis=-1, *, xp=None)`                                                                     | sort           | indirect partial sort                  |
|  [04]   | `atleast_nd(x, /, *, ndim, xp=None)`                                                                               | shape          | broadcast to minimum ndim              |
|  [05]   | `broadcast_shapes(*shapes, xp=None) -> tuple[int \| None, ...]`                                                    | shape          | broadcast shape computation            |
|  [06]   | `cov(m, /, *, xp=None)`                                                                                            | statistics     | covariance matrix                      |
|  [07]   | `create_diagonal(x, /, *, offset=0, xp=None)`                                                                      | linear algebra | diagonal matrix from 1-D               |
|  [08]   | `default_dtype(xp, kind='real floating', *, device=None)`                                                          | dtype          | backend default dtype query            |
|  [09]   | `expand_dims(a, /, *, axis=(0,), xp=None)`                                                                         | shape          | insert axes                            |
|  [10]   | `isclose(a, b, *, rtol=1e-5, atol=1e-8, equal_nan=False, xp=None)`                                                 | comparison     | approximate equality                   |
|  [11]   | `isin(a, b, /, *, assume_unique=False, invert=False, kind=None, xp=None)`                                          | membership     | element membership test                |
|  [12]   | `kron(a, b, /, *, xp=None)`                                                                                        | linear algebra | Kronecker product                      |
|  [13]   | `lazy_apply(func, *args, shape=None, dtype=None, as_numpy=False, xp=None, **kwargs) -> Array \| tuple[Array, ...]` | lazy           | lazy dispatch for graph backends       |
|  [14]   | `nan_to_num(x, /, *, fill_value=0.0, xp=None)`                                                                     | cleaning       | replace NaN/inf                        |
|  [15]   | `nunique(x, /, *, xp=None)`                                                                                        | aggregation    | count unique elements                  |
|  [16]   | `one_hot(x, /, num_classes, *, dtype=None, axis=-1, xp=None)`                                                      | encoding       | one-hot encoding                       |
|  [17]   | `pad(x, pad_width, mode='constant', *, constant_values=0, xp=None)`                                                | shape          | constant-mode padding                  |
|  [18]   | `partition(a, kth, /, axis=-1, *, xp=None)`                                                                        | sort           | partial sort along axis                |

[ENTRYPOINT_SCOPE]: set, search, and testing
- rail: array-api

| [INDEX] | [SURFACE]                                                                             | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------------------------------------------------------------------ | :------------- | :------------------------------------- |
|  [01]   | `searchsorted(x1, x2, /, *, side='left', xp=None)`                                    | search         | binary search                          |
|  [02]   | `setdiff1d(x1, x2, /, *, assume_unique=False, xp=None)`                               | set            | set difference                         |
|  [03]   | `sinc(x, /, *, xp=None)`                                                              | math           | normalized sinc                        |
|  [04]   | `union1d(a, b, /, *, xp=None)`                                                        | set            | set union                              |
|  [05]   | `angle(z, /, *, deg=False, xp=None)`                                                  | complex        | phase angle                            |
|  [06]   | `testing.assert_close(actual, desired, *, rtol=None, atol=0, xp=None, ...)`           | test helper    | approximate equality assert            |
|  [07]   | `testing.assert_equal(actual, desired, *, xp=None, ...)`                              | test helper    | exact equality assert                  |
|  [08]   | `testing.assert_less(x, y, *, xp=None, ...)`                                          | test helper    | strict less assert                     |
|  [09]   | `testing.assert_close_nulp(actual, desired, *, nulp=1, xp=None, ...)`                 | test helper    | ULP-based assert                       |
|  [10]   | `testing.lazy_xp_function(func, *, allow_dask_compute=False, jax_jit=True, ...)`      | test fixture   | wrap function for lazy backend testing |
|  [11]   | `testing.patch_lazy_xp_functions(request, monkeypatch=None, *, xp) -> ContextManager` | test fixture   | patch lazy dispatch for tests          |
|  [12]   | `testing.jax_autojit(func)`                                                            | test fixture   | auto-`jax.jit` wrapper for lazy assertions |
|  [13]   | `testing.pickle_flatten(obj, leaf_types) / pickle_unflatten(leaves, aux)`              | test helper    | pickle round-trip flatten for lazy pytrees |

## [04]-[IMPLEMENTATION_LAW]

[ARRAY_API_EXTRA_TOPOLOGY]:
- all operations accept an optional `xp` keyword; omitting it triggers `array_namespace(*arrays)` resolution internally, so the dense rail resolves `xp` once at the boundary and threads it.
- `at(x, idx)` returns an `at` builder; the terminal verbs are exactly `set`, `add`, `subtract`, `multiply`, `divide`, `power`, `min`, `max`, each `(y, /, copy=None, xp=None) -> Array`. There is no `.get`; this is the write-side index algebra only.
- `copy=` is the load-bearing knob: `copy=None` (default) mutates in place for a writeable backend and copies for a read-only one (so JAX/Dask always copy, NumPy mutates); `copy=True` always copies; `copy=False` mutates and raises on a read-only buffer. Gate `copy=False` behind `array_api_compat.is_writeable_array(x)`.
- `lazy_apply(func, *args, shape=, dtype=, as_numpy=)` is the deferred entry for graph-execution backends (JAX `jit`, Dask), declaring output `shape`/`dtype` so the wrapped `func` participates in tracing; `as_numpy=True` materializes to NumPy inside the wrapper for non-traceable bodies.
- `default_dtype(xp, kind=...)` resolves the backend's canonical dtype for `'real floating'`/`'complex floating'`/`'integral'`/`'indexing'`; use it instead of a hardcoded `xp.float64` when allocating result buffers.

[STACKING]:
- `array-api-extra` sits directly on `array-api-compat`: the canonical compute kernel does `xp = array_namespace(*arrays)` then `array_api_extra.<op>(..., xp=xp)`, giving one backend-agnostic rail across NumPy/JAX/Torch/Dask without a per-backend branch.
- The `at(...).set(..., copy=...)` builder is the polymorphic substitute for JAX `.at[idx].set()` AND NumPy in-place assignment in one surface, so a kernel that must run under both `jax` (graduation/sampler study) and NumPy (eager analysis) writes one indexed-update expression.
- `lazy_apply` pairs with `array_api_compat.is_lazy_array`: the lazy guard decides whether to wrap in `lazy_apply` (traced backend) or call the op directly (eager), the same eager/lazy fork the `diffrax`/`equinox`/`optimistix` JAX rails depend on.
- `apply_where(cond, args, f1, f2, fill_value=)` is the safe two-branch elementwise primitive replacing `xp.where(cond, f1(x), f2(x))` when `f1`/`f2` would error on the masked-out domain (e.g. `log` of non-positive); it only evaluates each branch on its live mask, which graph backends require.

[LOCAL_ADMISSION]:
- Pass `xp` explicitly once it is resolved to avoid redundant dispatch inside hot paths.
- `lazy_apply` is required when `func` uses control flow incompatible with graph tracing (JAX/Dask); declare `shape`/`dtype` so tracing succeeds.
- `testing.*` (including `lazy_xp_function`, `patch_lazy_xp_functions`, `jax_autojit`, `pickle_flatten`) belongs in test scope only; never import in production compute owners.

[RAIL_LAW]:
- Package: `array-api-extra`
- Owns: Array API extension functions absent from the base standard plus the `at` indexed-update builder and lazy-dispatch wrapper
- Accept: `xp`-parametric calls that stay backend-agnostic, `at(...).set(..., copy=...)` gated by `is_writeable_array`, `lazy_apply` gated by `is_lazy_array`
- Reject: hand-rolling any of these extension patterns against a single backend; a vendor-specific `.at[]`/in-place assignment when the `at` builder spans both; importing `testing` outside test scope

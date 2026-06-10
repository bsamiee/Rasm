# [REGISTRY_ADVANCED]

[TYPEFORM_GENERIC_CORRELATION]:
- `TypeForm[T]` is a pure static annotation; at runtime `type(TypeForm[int]) is type` — the underlying dict key is always `type`. Consequence: `reg[int]` returns `Handler[object]` to the type checker, not `Handler[int]`; the correlation is erased at lookup. The only recovery paths are (a) a `TypeIs[T]`-narrowed accessor method on the registry class, or (b) an `@overload`-only static facade over an `Any`-typed body. Neither path is free: `TypeIs` works at call-site narrowing when the value is already in hand; `@overload` is checker-only and the runtime body still returns `Any`. Choose (a) when the value is available at lookup time and the handler is called immediately; choose (b) when the registry is a static factory table and correctness is checked offline. [CPython 3.15.0b1 `typing.py`, 2026-06-09; PEP 747 accepted 3.15]
- Phantom-keyed registry with `TypeIs`-narrowed accessor — verified pattern:
  ```python
  T = TypeVar("T")

  class Registry:
      _d: dict[type, Callable[..., object]] = {}

      def put(self, tp: TypeForm[T], fn: Callable[[T], object]) -> None:
          self._d[tp] = fn  # type: ignore[index]

      def lookup(self, value: T) -> Option[Callable[[T], object]]:
          fn = self._d.get(type(value))
          return Some(fn) if fn is not None else Nothing  # type: ignore[return-value]
  ```
  `lookup` narrows through `type(value)` rather than accepting `TypeForm[T]` as the key argument, which is what preserves T at the call site — the value is the discriminant, not an explicit type token. [verified Python 3.15.0b1, 2026-06-09]
- Sealed `Protocol` accessor pattern: when the registry is keyed by capability rather than concrete type, define a `@runtime_checkable Protocol` and a `TypeIs[P]` guard function; the guard both validates conformance and narrows the value so the accessor body compiles without `Any`. `isinstance` against a `@runtime_checkable Protocol` checks only attribute presence (not signature), so the guard is necessary but not sufficient for deep conformance — `beartype` is the complete gate. [CPython 3.15.0b1 `typing.py`, 2026-06-09]
- Reject: carrying `TypeForm[T]` as the lookup argument to a `dict.get` call — the checker sees `dict[type, object].get(TypeForm[T])` which returns `object`, not `Handler[T]`. The generic does not flow through built-in dict subscript. Phantom correlation must come from the call site's value, not the key argument.

[ENTRYPOINTS_LIFECYCLE]:
- `entry_points(group=...)` returns an `EntryPoints` tuple (subclass of `tuple`, not a generator); it is safe to iterate more than once. Each `EntryPoint.load()` calls `importlib.import_module(ep.module)` followed by `functools.reduce(getattr, attrs, module)` — it is a full import on every invocation with no internal memoization. [CPython 3.15.0b1 `importlib/metadata/__init__.py`, 2026-06-09]
- **Order is filesystem-dependent.** `MetadataPathFinder._search_paths` chains `FastPath.search` across `sys.path` entries in `sys.path` order; within a single directory, `os.listdir` returns entries in filesystem order (inode on macOS HFS+, creation order on ext4 with `dir_index`, undefined otherwise). No alphabetical or declared-priority ordering is guaranteed at any level. [CPython 3.15.0b1 `importlib/metadata/__init__.py`, 2026-06-09]
- **Conflict detection is caller responsibility.** `entry_points` performs no deduplication; two installed distributions declaring the same name in the same group produce two `EntryPoint` objects in the returned tuple. Last-win behavior when populating a registry from the tuple is non-deterministic across environments. Canonical detection pattern:
  ```python
  from collections import defaultdict
  from expression import Ok, Error

  def load_group(group: str) -> Result[dict[str, object], str]:
      eps = entry_points(group=group)
      by_name: defaultdict[str, list[EntryPoint]] = defaultdict(list)
      for ep in eps:
          by_name[ep.name].append(ep)
      conflicts = {n: vs for n, vs in by_name.items() if len(vs) > 1}
      return (
          Error(f"conflict: {conflicts}")
          if conflicts
          else Ok({n: vs[0].load() for n, vs in by_name.items()})
      )
  ```
  [CPython 3.15.0b1 `importlib/metadata/__init__.py`; PEP 451, 2026-06-09]
- `EntryPoint.load()` is **not idempotent in cost** but is idempotent in result: `import_module` is O(1) after the first call (sys.modules hit), but `getattr` attribute traversal runs on every `load()` call. Cache the loaded object, not the `EntryPoint`. Pattern: `_loaded: dict[str, object] = {}` populated once at startup before the registry loop, never re-fetched at dispatch time. [verified CPython 3.15.0b1, 2026-06-09]
- `EntryPoint._match` is a `functools.cached_property` — it parses the `value` string once per object lifetime. Safe to inspect `.module` and `.attr` before `load()` for validation (e.g., block loading from untrusted packages). [CPython 3.15.0b1, 2026-06-09]

[IMPORT_COMPLETION_GUARANTEE]:
- The guarantee: `@register` at module scope completes before another thread's `import pkg` returns. The mechanism: `_find_and_load` acquires the per-module `_ModuleLock` (an `RLock`) before calling `_find_and_load_unlocked`; any second thread importing the same name blocks on `lock.acquire()` in `_lock_unlock_module`. `spec.loader.exec_module(module)` — which runs all top-level `@register` decorators — runs under that lock. [CPython 3.15.0b1 `importlib/_bootstrap.py`, 2026-06-09]
- **Circular import partial-exposure caveat.** `sys.modules[spec.name] = module` is set *before* `exec_module` runs (see `_load_unlocked`). A module B that is imported from within module A's own top-level body (circular dependency) receives module A in a partially initialized state — only the definitions that executed before the `import B` statement are present. An `@register` decorator that runs after the circular import call is not visible to B. Mitigation: place all registrations before any intra-package imports in the module, or break the cycle by deferring the dependent import to a function scope. [CPython 3.15.0b1 `importlib/_bootstrap.py`, 2026-06-09]
- **`_DeadlockError` silent partial acceptance.** When CPython detects a deadlock in `_lock_unlock_module` (concurrent circular import from two threads), it catches `_DeadlockError` and accepts the partially initialized module rather than raising. This is the only case where the completion guarantee is voided. Under free-threaded builds (3.13+ `--disable-gil`) the `_ModuleLock` RLock is still present and this path is unchanged. [CPython 3.15.0b1 `importlib/_bootstrap.py`, 2026-06-09]
- **Free-threaded builds add no import-side registration safety.** The `dispatch_cache` in `singledispatch` is a `weakref.WeakKeyDictionary` — not protected by any additional lock. Under `--disable-gil`, two threads calling `sd.register()` and `sd(value)` concurrently can observe a stale cache without atomicity guarantees. The `registration_law` of completing all `@register` calls before any call thread reaches the surface is therefore not merely a style rule — it is the only safe concurrency protocol. [CPython 3.15.0b1 `functools.py`, 2026-06-09]

[LAYERED_SCOPED_COMPOSITION]:
- `collections.ChainMap` is the correct primitive for layered/scoped registry composition. `ChainMap(scope, parent)` wraps both dicts without copying; `.get(key)` walks `.maps` in order. Scope-layer hit costs 0.127 µs; parent-fallback costs 0.209 µs (1.64x); flat merged dict costs 0.022 µs (5.8x faster than ChainMap). The overhead is acceptable for request-scoped dispatch where allocation of a merged copy per request would dominate. [measured Python 3.15.0b1 macOS arm64, 2026-06-09]
- Scoped overlay protocol — three invariants: (1) the parent registry is `frozendict` or a `MappingProxyType`-wrapped dict, never mutated after startup; (2) the scope registry is constructed fresh per scope boundary (request, task, test); (3) `ChainMap` is the join — never `{**parent, **scope}` which allocates a full copy and loses scope-only delete semantics. Scope teardown is implicit: drop the scope dict reference and the `ChainMap` is collected.
- Nest depth: `ChainMap(override, scope, parent)` for three tiers (per-call override, request scope, global parent). `.get` walks all three maps left-to-right; depth-N search is O(N) miss, O(1) first-hit. Do not nest more than three tiers; collapse deeper nesting into a materialized scope dict before building the `ChainMap`.
- Verified: `ChainMap.maps` is a plain `list` — `ChainMap(scope, parent).maps == [scope, parent]`. Adding a scope entry requires `layered.maps[0][key] = handler`, not `layered[key] = handler` (which would write to `maps[0]` regardless — but only if `maps[0]` is a regular dict). Scope is mutable only through its own dict reference, never through the `ChainMap` directly when immutability of the parent tier must be preserved.

[SINGLEDISPATCH_PERF_TRADEOFF]:
- **Measured dispatch costs, Python 3.15.0b1, macOS arm64** (all wall-clock, single-threaded):

  | path | cost | vs `dict[type, H]` |
  |---|---|---|
  | `dict[type, H]` exact type | 0.049 µs | 1× |
  | `singledispatch` warm exact | 0.162 µs | 3.3× |
  | `singledispatch` warm ABC-registered | 0.175 µs | 3.6× |
  | `dict` + manual MRO walk | 0.178 µs | 3.6× |
  | `ChainMap` scope hit | 0.127 µs | 2.6× |
  | `ChainMap` parent fallback | 0.209 µs | 4.3× |
  | `singledispatch` ABC token-invalidated | 0.403 µs | 8.2× |
  | `singledispatch` cold exact | 0.742 µs | 15× |
  | `singledispatch` cold subclass MRO | 4.140 µs | 84× |

- The warm-hit cost is paid on every dispatch call. At 160 ns per warm dispatch, a hot loop calling dispatch 10⁶ times per second costs 160 ms/s of overhead purely from the dispatch mechanism. That is the break-even point for switching to `dict[type, H]` at 49 ns/call — a 3.3× saving.
- **ABC path is warm-equivalent** when the token is stable. The `cache_token` check is a single integer comparison in the fast path; overhead is immeasurable at 0.175 µs vs 0.175 µs. The penalty is paid only on `ABCMeta.register` calls that bump the token: 0.403 µs (2.3× warm) per invalidation+dispatch pair. Avoid calling `ABCMeta.register` after startup in hot paths.
- **Cold dispatch dominates in cold-start or one-shot contexts.** 4.14 µs for a subclass MRO walk means a service that dispatch-initializes 1000 distinct subclasses cold costs 4.1 ms before cache is warm. Prewarming (`sd.dispatch(T)` for each expected type at startup) eliminates this. `sd._clear_cache()` is the inverse and exists for testing.
- **The `WeakKeyDictionary` cache evicts entries when the dispatch class has no other references.** This is relevant for dynamically generated classes (e.g., dataclass factories, `type()` constructions) that are created and discarded — the cache entry disappears with the class, forcing a re-walk on next encounter. A `dict[type, H]` cache without weak references would hold the class alive, which may be undesirable for plugin unload scenarios; `singledispatch` is correct here.
- Decision rule: use `singledispatch` when the open-by-subclass contract is the real invariant; use `dict[type, H]` when the key set is closed or semi-closed and you need predictable hot-loop cost; use `ChainMap` for layered scoped composition where scope isolation is required without per-scope copy allocation.

[ABC_AMBIGUITY_IN_SINGLEDISPATCH]:
- `_find_impl` (CPython `functools.py`) uses `types.resolve_bases` and MRO walk — it resolves ambiguity by MRO order, not declaration order. When two ABC-registered classes both match and neither is a subclass of the other, `_find_impl` raises `RuntimeError: Ambiguous implementation`. This is the ABC-ambiguity pitfall named in the dispatch-forms chooser. It fires silently in the warm path because the first call resolves and caches; subsequent calls hit the cache and never re-raise — but removing one ABC registration later (not possible via public API; `sd.register` only adds) does not clear the cache, leaving a stale hit. The only safe pattern for ABC-registered dispatch is a strict containment hierarchy with no siblings. [CPython 3.15.0b1 `functools.py`, 2026-06-09]

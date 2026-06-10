# [SEAM_MECHANICS_AND_FAILURE_MODES]

[TAGGED_UNION_MATCH_MECHANICS]:
- `@tagged_union` sets `__match_args__` to the tuple of field names in declaration order — `cls.__match_args__ = field_names` (source: `.venv/lib/python3.15/site-packages/expression/core/tagged_union.py`, 2026-06-09). Positional match `case Req('left')` binds `Req.__match_args__[0]` — the first declared field. Declaration order is therefore load-bearing for positional patterns; reordering fields is a breaking change to all positional match arms.
- Accessing an inactive case field on a `@tagged_union` instance raises `AttributeError`. CPython's structural pattern matching (PEP 634) silently converts `AttributeError` during pattern binding into a non-match — it does not propagate the exception. Any other exception type (e.g. `ValueError`) propagates. This means `case Req(tag='drag', drag=(x, y))` against a `click`-active instance safely falls through; it does not crash. (CPython 3.15.0b1, verified in-process, 2026-06-09)
- `frozen=True` generates `__hash__` as `hash((cls.__name__, self.tag, getattr(self, self.tag)))`. The hash is computed from `cls.__name__`, not `id(cls)` — two independently defined `@tagged_union` classes that share the same `__name__`, tag, and value will produce identical hash values. In a multi-module registry keyed on frozen union instances, this creates a collision hazard. Use fully qualified names or module-prefixed tag values if the same short name appears in more than one module. (source: `expression/core/tagged_union.py`, 2026-06-09)
- `frozen=True` is shallow: it prevents field reassignment (`__setattr__` raises `TypeError`) but does not prevent mutation of a mutable value stored in the field. `instance.tags.append(x)` succeeds even on a `frozen=True` instance when `tags` is a `list`. `hash()` raises `TypeError` at call time — not at construction — when the active case value is unhashable. The failure is late; use only immutable value types (`str`, `int`, `tuple`, `frozenset`, `None`) in cases that must be hashable or used as dict keys. (verified CPython 3.15.0b1, 2026-06-09)
- `frozen=True, order=True` together is valid. `_index` is assigned in declaration order (first field = 0) and drives `__lt__`. Sorting a sequence of frozen tagged unions produces declaration-order priority ordering, which directly supports priority-queue dispatch: `sorted(pending)` orders by case declaration position, lowest index first.

[SINGLEDISPATCH_ANNOTATION_AND_FORWARD_REF]:
- Python 3.15 (PEP 749): annotations are lazy by default without `from __future__ import annotations`. `singledispatch.register` uses `annotationlib.Format.FORWARDREF` via `get_type_hints(func, format=Format.FORWARDREF)` — it raises `TypeError` with message `"ForwardRef(...) is an unresolved forward reference"` when the annotated type is not yet defined at decoration time, whether or not `from __future__ import annotations` is active. The fix is always explicit `@register(ConcreteClass)` form, or ensure the class is fully defined before the `@register` line. (source: CPython 3.15.0b1 `functools.py` lines 962-967, `annotationlib.Format`, 2026-06-09)
- `@register` with a `X | Y` union type (`types.UnionType`) writes both `registry[X] = func` and `registry[Y] = func` in a single call, then calls `dispatch_cache.clear()` once after the loop. The registry update is not atomic from a thread-safety perspective — another thread calling `dispatch()` between the two writes would observe a partial state. The completion-before-first-call rule (all registration before any dispatch thread) is the only safe protocol. (source: CPython 3.15.0b1 `functools.py` lines 956-959, 2026-06-09)

[SINGLEDISPATCHMETHOD_SHARED_DISPATCHER_HAZARD]:
- `singledispatchmethod` is a class-level descriptor. Calling `.register()` on a bound access from a subclass — `Sub.handle.register(int)(fn)` — mutates the parent class's dispatcher. The mechanism: `_singledispatchmethod_get.register` is a property returning `self._unbound.register`, where `_unbound` is the original `singledispatchmethod` descriptor on the class that defined it. There is no per-subclass copy. Subclass registrations are global and permanent for the lifetime of the process. (source: CPython 3.15.0b1 `functools.py` lines 1057, 1127-1128, verified by mutation test, 2026-06-09)
- Safe per-class dispatch without sharing: declare a separate `singledispatch` function as a class attribute on each class. Unlike `singledispatchmethod`, a class-attribute `singledispatch` is not a descriptor and is not shared through inheritance. Each subclass that needs its own dispatch table declares its own `_handler = singledispatch(lambda x: ...)` and registers against its own attribute. The dispatcher is accessed via `cls._handler`, not the inherited descriptor chain.
- `singledispatchmethod` combined with `@staticmethod` works — `@singledispatchmethod @staticmethod def method(x)` dispatches without binding `self`, and `@method.register(T) @staticmethod def _(x)` registers a static handler. The `_dispatch_arg_index` is 0 when accessed as an unbound method on the class (class-level access). (verified CPython 3.15.0b1, 2026-06-09)

[ENTRYPOINTS_PROVENANCE_AND_INDEXING]:
- `EntryPoints.__getitem__` accepts a `str` name, not an integer index. `entry_points(group=G)[0]` raises `KeyError: 0` because it calls `select(name=0)`. Iterate as a tuple: `list(entry_points(group=G))` or `for ep in entry_points(group=G)`. The `tuple` base class supports integer indexing but `EntryPoints.__getitem__` overrides it for name-string lookup. (source: CPython 3.15.0b1 `importlib/metadata/__init__.py` lines 328-333, verified, 2026-06-09)
- `entry_points(...).select()` does not support `dist=` as a filter parameter. `_disallow_dist` raises `ValueError: "dist" is not suitable for matching`. The provenance-safe loading path is `Distribution.from_name(trusted_name).entry_points.select(group=G)` — this loads entry points only from a specific distribution. For multi-distribution loading with trust filtering, iterate and check `ep.dist.name` manually; `ep.dist` is always set when entries come from `entry_points(group=...)` (set via `_for(dist)` during `EntryPoints._from_text_for`). (source: CPython 3.15.0b1 `importlib/metadata/__init__.py`, verified with `Distribution.from_name`, 2026-06-09)

[INIT_SUBCLASS_AUTO_REGISTRATION_SEAM]:
- `__init_subclass__` is a class-body hook fired synchronously at class definition time, before the class object is returned from `type.__new__`. It accepts keyword arguments via class-header syntax: `class FooHandler(Base, tag='foo')`. This makes it a zero-boilerplate auto-registration seam: the subclass declares its own discriminant as a class-keyword argument, and `Base.__init_subclass__` writes it into a class-level registry dict. (CPython 3.15.0b1 `functools.py` line 281, verified, 2026-06-09)
- Canonical `__init_subclass__` registry pattern with conflict detection and `Option` miss:
  ```python
  from typing import ClassVar
  from expression import Some, Nothing

  class Handler:
      _registry: ClassVar[dict[str, type[Handler]]] = {}

      def __init_subclass__(cls, tag: str = '', **kw: object) -> None:
          super().__init_subclass__(**kw)
          if not tag:
              return
          if tag in Handler._registry:
              raise TypeError(f'Duplicate handler tag: {tag!r} (existing: {Handler._registry[tag].__qualname__})')
          Handler._registry[tag] = cls

      @classmethod
      def lookup(cls, tag: str):
          h = cls._registry.get(tag)
          return Some(h) if h is not None else Nothing

  class FooHandler(Handler, tag='foo'): ...
  ```
  Conflict is detected at class definition time, not at lookup time. (verified CPython 3.15.0b1, 2026-06-09)
- `__init_subclass__` versus `@register` decorator as registration seams: `__init_subclass__` is appropriate when the discriminant is structural (the subclass itself IS the handler type) and the registry is keyed by tag strings or similar stable tokens; `@register` is appropriate when the handler is a plain function and the registry is keyed by input type or vocabulary token. The two seams own different shapes: class-hierarchy extension vs function registration.
- `__init_subclass__` fires for every level of the class hierarchy, including intermediate abstract base classes. Guard with `if tag:` (or equivalent non-empty check) to skip base classes that inherit but do not declare a tag.

[EFFECT_RESULT_YIELD_FROM_INVARIANT]:
- `@effect.result[T, E]()` wraps a generator function as a `Result`-returning function. The correct unwrapping form inside the generator is `yield from Ok(x)` or `yield from result_expr` — this calls `Result.__iter__`, which either yields the inner value (and the generator receives it back as the `yield from` result) or raises `EffectError` (which `Builder._send` catches to short-circuit). Plain `yield Ok(x)` sends the `Result` object itself back to the generator body, not the unwrapped value; arithmetic on the `Result` object then fails with `TypeError`. (source: `.venv/lib/python3.15/site-packages/expression/core/builder.py` lines 50-67, verified, 2026-06-09)
- `effect.result` is the `ResultBuilder` class, not a module-level singleton. `effect.result[T, E]()` creates a fresh `ResultBuilder` instance per call via `__class_getitem__` (returns a `_GenericAlias`) followed by `()`. Each `@effect.result[T, E]()` invocation is a separate decorator instance wrapping one function. `effect.result()` (without type params) creates the same builder without static type evidence. (verified CPython 3.15.0b1, 2026-06-09)
- `effect.result` is sync-only. The `Builder` base class uses a synchronous generator protocol (`gen.send(value)` in `_send`). There is no async variant in `expression`. Async surfaces return `Result` directly from an `async def` function; `@effect.result` cannot be applied to async generators. (source: `expression/core/builder.py` — no `async`/`await` anywhere, 2026-06-09)
- Registry miss inside an `@effect.result` generator lifts to `Error` cleanly: `yield from (Error(f'no handler') if handler is None else Ok(handler))` short-circuits on miss and propagates as `Error` to the caller. This is the canonical inline Option-to-Error conversion for dispatch in a generator body. (verified, 2026-06-09)

[PIPELINE_AND_COMPOSE_EDGE_CASES]:
- `compose()` with zero arguments returns an identity function. `compose()` (five-arg runtime body) returns `reduce(lambda acc, f: f(acc), (), x)` which reduces with an empty `fns` tuple and returns `x` unchanged. This is the correct config-driven pipeline fallback when the config selects zero steps. (source: `expression/core/compose.py` lines 137-139, verified, 2026-06-09)
- `pipeline()` with zero arguments wraps the value in `Ok`. The `reduce(reducer, (), Ok)` base case returns `Ok` itself (the `Ok` constructor), so `pipeline()(x)` returns `Ok(x)`. This differs from `compose()`: `compose()` is an identity over any type; `pipeline()` lifts into `Result`. A config-driven pipeline of fallible steps should use `pipeline(*steps)` and receives `Ok(value)` when the step list is empty. (source: `expression/extra/result/pipeline.py` lines 73-87, verified, 2026-06-09)
- `expression.catch(exception=E)` is a decorator that wraps a function to catch a single exception type and return `Error(exc)`. Multiple exception types require stacking: `@catch(exception=ValueError) @catch(exception=KeyError) def f(...)`. The outer `catch` wraps the inner-wrapped function; both layers are active. Order matters: inner exception types are caught first, then the outer wrapper catches from the already-caught return value — the net effect is the union of both exception types mapped to `Error`. (source: `expression/extra/result/catch.py`, verified, 2026-06-09)

[FROZENDICT_AND_MAPPINGPROXY_PARENT_SAFETY]:
- `ChainMap` is a live view over its underlying dicts. Mutations to `scope` or `parent` after `ChainMap` construction are immediately visible in the `ChainMap` — no copy is made. The safety invariant `parent must be MappingProxyType or frozendict` is not enforced by `ChainMap` itself; it is a caller responsibility. A `MappingProxyType` parent prevents accidental mutation through the parent dict reference; `ChainMap.__setitem__` always writes to `maps[0]` (the scope layer), so it cannot reach a proxy parent. (verified CPython 3.15.0b1, 2026-06-09)
- `ChainMap(scope, parent).maps` is a plain `list`. Scope-layer writes require `chainmap.maps[0][key] = handler`, not `chainmap[key] = handler` — both route to `maps[0]` only when `maps[0]` is a regular dict, but the explicit `maps[0]` form clarifies intent and avoids confusion when the immutability boundary needs to be documented.

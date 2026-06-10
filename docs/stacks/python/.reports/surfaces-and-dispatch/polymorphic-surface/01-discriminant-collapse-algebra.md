# [POLYMORPHIC_SURFACE_COLLAPSE]

[DISCRIMINANT_AXES]:
- The discriminant of a polymorphic surface is one of four orthogonal axes, each with a distinct mechanism and closure: a tag field on a tagged-union ADT (closed, O(1) positional match), runtime type identity (open, MRO/registry lookup), structural capability (open, `Protocol` + `TypeIs` narrowing), or a payload-value predicate (closed core, guard narrows within a known tag). [functools + expression, installed 3.15.0b1, 2026-06-09]
- Choosing the wrong axis leaks: a tag dispatch on an open type erases extensibility; a type registry on a closed in-repo vocabulary forfeits the static exhaustiveness proof.
- The densest surfaces project the discriminant once and fold over it; re-deriving the discriminant inside each arm is the smell that a single projection function is missing.

[TAGGED_UNION_ANATOMY]:
- `expression.@tagged_union` collapses N field sets into one class body; `tag()` and `case()` are identical `field(init=False, kw_only=True)` factories whose names are documentation only. [expression/core/tagged_union.py 5.6.0, 2026-06-09]
- `__init__` enforces exactly-one-active-case (`len(kwargs) != 1` rejects); reading an inactive case raises `AttributeError`; `__dataclass_fields__` is pruned per-instance to the active case plus `tag`, so `dataclasses.asdict()` sees only the active case and generic reflection on an instance must read `type(instance).__match_args__`, not `dataclasses.fields(instance)`.
- `__match_args__` places `tag` first, so `case Op('fetch', fetch=k)` is the positional form of `case Op(tag='fetch', fetch=k)`; an exhaustive match binds `tag` first so the checker proves totality.
- `frozen=True` adds `__hash__` and a `__setattr__` guard (no `__slots__`); the class is `@dataclass_transform()`-decorated so checkers see generated init/match semantics; `Generic[T, E]` flows type parameters into case fields, giving a parametric command ADT whose return type correlates with the variant.
- `order=True` sets `__lt__` by `(self._index, value)` where `_index` is the 1-based declaration position, so a tagged union is directly heap-orderable for priority dispatch without a separate comparator.

[N_TO_ONE_COLLAPSE]:
- The pressure point is three or more sibling callables sharing a name prefix and the same return rail; they collapse to one request ADT plus one total fold, after which a new modality costs one `case` field plus one match arm and zero new functions. [verified 4->1, 2026-06-09]
- The collapsed surface constructs directly at the call site (`fetch(FetchReq(one=k))`); no extractor, helper, or factory mediates.
- Guards narrow payload within a known tag but never provide exhaustiveness — a match falls through on a failed guard, so every tag needs an unconditional arm; guard-only discrimination produces gaps the checker cannot detect from match structure.

[CLOSED_VS_OPEN_ENCODING]:
- Encode a closed in-repo family as a tagged union (or a union of `@disjoint_base` frozen records) with a total `match` + `assert_never`; encode a family extended by foreign code as a `singledispatch` or `TypeForm`-registry surface. The two are not interchangeable dispatch forms.
- `typing.disjoint_base` is a runtime-only marker (`__disjoint_base__ = True`) that lets checkers prove non-overlap between hierarchies and flag unreachable arms; it does not seal the hierarchy. For three or more same-module variants a tagged union is always denser than N disjoint records. [typing 3.15.0b1, 2026-06-09]
- A `singledispatch` base registered as one arm wrapping an internal `match` keeps the closed core total while the base stays open to foreign subclasses — the canonical open/closed hybrid.

[OPEN_GAP]:
- `singledispatch` and `@tagged_union` both dispatch on `args[0].__class__`/tag and cannot distinguish parameterized generic aliases (`Container[int]` and `Container[str]` both route to the origin); per-parameter behavior requires a discriminant field, not generic-alias dispatch.

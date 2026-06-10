# [DISPATCH_FORM_SELECTION]

[DECISION_TREE]:
- Select the form by exclusive questions, not a feature list: owned-and-closed variants -> `match` over an ADT (static exhaustiveness, stop); structural capability rather than type identity -> `Protocol` + `TypeIs`; bounded string or enum vocabulary key -> `frozendict[K, Callable]` value table; different return type per input type -> `@overload` stubs plus one `match` body; open-by-type third-party extension -> `singledispatch`; two or more independent positional type axes with no collapsing ADT or `Protocol` -> multiple dispatch.

[SINGLEDISPATCH_INTERNALS]:
- The ABC-ambiguity `RuntimeError` fires at DISPATCH time, not registration, and only when two competing ABCs are both virtual (neither in `cls.__mro__`); explicit inheritance resolves it via MRO. Resolution: register the concrete type, or make one ABC subclass the other.
- `@f.register(int | str)` expands to two independent registry keys (`registry[int]`, `registry[str]`); the union is validated but never stored, and each key overrides separately.
- ABC cache invalidation is automatic via a `cache_token` tied to `ABCMeta._abc_invalidation_counter`, so no manual clear is needed after `ABC.register(T)`; each ABC registration triggers an MRO re-walk and cache rebuild, which is why registration must complete at import time.
- `@singledispatchmethod` must be the outermost decorator; applying `@classmethod`/`@staticmethod` before it silently defeats registration (the decorated object is no longer a function) — put `@classmethod` on the registered arms, not the base.

[OVERLOAD_STATIC_TO_RUNTIME_BRIDGE]:
- `@overload` earns checker value only for return-type variance per input type; with a uniform return type it adds nothing — collapse to `match`. At runtime the overload stubs are inert (`...` bodies) and the single implementation runs unconditionally.
- `typing.get_overloads(fn)` returns the stub functions; `annotationlib.get_annotations(stub, format=Format.VALUE)` resolves their types, building a runtime dispatch or validation table from static overloads with no third-party library.
- Trap: `isinstance(x, list[int])` raises `TypeError`; extract `typing.get_origin(tp) or tp` for the runtime check and keep the full parameterized type only as validation metadata.

[MULTIPLE_DISPATCH_PLUM]:
- `plum-dispatch` is the only true multiple-dispatch library; it dispatches on all positional args via beartype, so it validates argument 2+ that `singledispatch` passes through unchecked. Keyword args bypass dispatch entirely and raise `NotFoundLookupError`; all dispatch args must be positional.
- `AmbiguousLookupError` fires at dispatch time for unresolvable N×M cells (e.g. `Dog×Dog` when only `Pet×Animal` and `Animal×Pet` are registered); resolution is an explicit arm for the ambiguous product.
- Break-even: if all argument types share a `Protocol` or collapse to a tagged union, that collapse is always denser; plum is justified only for cross-library type matrices with no shared base.

[VALUE_TABLE_FORM]:
- The bounded-vocabulary form is `frozendict[K, Callable]` builtin or a module-level dict; constraining the key type to a `StrEnum`/`Literal` makes a miss a domain-invariant violation rather than an expected branch.

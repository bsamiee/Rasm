# [REGISTRY_AND_SEAM]

[REGISTRY_TAXONOMY]:
- Four registry forms by discriminating contract: a `frozendict[StrEnum|Literal, V]` closed table (immutable at module scope, miss is an invariant violation); a `dict[type, Handler[T]]` keyed by `TypeForm[T]` open registry (populated by `@register` decorators, sealed after import, miss returns `Option`); `functools.singledispatch` as a type-hierarchy registry (MRO walk, `.registry` is a read-only `MappingProxyType`, base impl is the miss branch); and `importlib.metadata.entry_points` as the install-time foreign-plugin seam (load once at startup, `.load()` each, register before the loop starts). [functools 3.15.0b1 + importlib.metadata, 2026-06-09]

[REGISTRATION_LAW]:
- Rows are built at DECORATION time: the decorator runs synchronously, writes the row, and returns the ORIGINAL handler unchanged (never a wrapper) — which makes it stackable with `@beartype`, picklable by `__qualname__`, and identity-stable for override detection.
- Module-scope registration completes before first use: CPython's per-module `_ModuleLock` guarantees a module's top-level code (where `@register` runs) completes before other threads read `sys.modules[name]`. [importlib._bootstrap 3.15.0b1, 2026-06-09]
- Reject: post-definition registry mutation outside decoration scope, scattered `.append()`, lazy first-call population, registration from threads or tasks after startup, and decorators that return a wrapper instead of the original.

[OPEN_CLOSED_SEAM]:
- The line is editability of the owner: closed when adding a member edits the owner; open when foreign code adds members without touching it.
- `match` owns closed in-repo domains (totality by `assert_never`); a `TypeForm[T]`-keyed registry owns the in-process open boundary (multi-module contributors import `register`; the owner names only `Handler[T]`); `singledispatch` owns the type-hierarchy open boundary (foreign subclassing; base impl returns `Error('<unhandled>')`); `entry_points` bridges the install-time axis into either registry.

[RAIL_ON_MISS]:
- Registry lookup returns `Option[Handler[T]]` and never raises: `Some(reg[k]) if k in reg else Nothing` then `.to_result(Fault()).bind(lambda h: h(event))` keeps the whole pipeline on the `Result` rail with no branching. For `singledispatch` the base impl is the miss branch, so no `Option` wrapper is needed. [expression 5.6.0 Option.to_result, 2026-06-09]

[OPEN_GAPS]:
- A `TypeForm[T]`-keyed `dict[type, Handler]` erases the per-key generic — `reg[ClickEvent]` types as `Handler[object]`; carry the correlation through a `TypeIs`-narrowed accessor or an explicit `# type: ignore[return-value]` at the lookup boundary.
- `entry_points` iteration order is discovery order, not declared or alphabetical; two plugins registering the same key make last-wins non-deterministic across installs — mandate explicit conflict detection (`Error('<conflict>')`) when populating from entry points.

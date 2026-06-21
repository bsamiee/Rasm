# [PY_BRANCH_API_BEARTYPE]

`beartype` supplies near-O(1) runtime type checking via the `@beartype` decorator and `beartype.claw` import hooks, the procedural DOOR API (`beartype.door`) for inline `isinstance`-style hint checks and subhint reasoning, declarative `Annotated` validators (`beartype.vale`), a typed exception/warning hierarchy (`beartype.roar`) whose violations can be redirected to domain exception types, and a PEP-faithful drop-in typing namespace (`beartype.typing`).

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `beartype`
- package: `beartype`
- module: `beartype`
- version: `0.23.0` (floor `>=0.22.2`; pinned to a git rev in the manifest for the cp315-clean PEP 749 path)
- license: MIT
- wheel: pure-python (`py3-none-any`), no ABI tag; resolves clean on cp315
- marker: none (core dependency)
- asset: runtime library
- rail: type-enforcement
- namespaces: `beartype`, `beartype.claw`, `beartype.door`, `beartype.vale`, `beartype.roar`, `beartype.typing`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration and strategy
- rail: type-enforcement

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [RAIL]                                       |
| :-----: | :--------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `BeartypeConf`               | config class   | immutable, hashable, cached decorator/hook config |
|  [02]   | `BeartypeStrategy`           | strategy enum  | `O0` (no-op), `O1` (constant), `Ologn` (logarithmic), `On` (linear) check depth |
|  [03]   | `BeartypeDecorPlace`         | place enum     | `FIRST`, `LAST`, `LAST_BEFORE_DECOR_HOSTILE` claw decoration position |
|  [04]   | `BeartypeViolationVerbosity` | verbosity enum | error message detail level                   |
|  [05]   | `FrozenDict`                 | immutable dict | hashable mapping value object (used by `hint_overrides`) |

[PUBLIC_TYPE_SCOPE]: door type-hint wrappers (`beartype.door`)
- rail: type-enforcement
- `TypeHint(hint)` wraps any PEP-compatible hint into an object-oriented, comparable, iterable façade; subscripted/union/callable/literal/etc. hints resolve to the concrete subclass.

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [RAIL]                                       |
| :-----: | :---------------------------------------- | :------------ | :------------------------------------------- |
|  [01]   | `TypeHint`                                | hint wrapper  | OO façade; `.args`, `.is_subhint`, `.is_bearable`, `.is_ignorable` |
|  [02]   | `UnionTypeHint` / `SubscriptedTypeHint`   | hint wrapper  | union members / generic args iteration       |
|  [03]   | `CallableTypeHint` / `ClassTypeHint`      | hint wrapper  | callable param/return, bare class            |
|  [04]   | `LiteralTypeHint` / `AnnotatedTypeHint`   | hint wrapper  | `Literal[...]` / `Annotated[...]` decomposition |
|  [05]   | `TupleFixedTypeHint` / `TupleVariableTypeHint` | hint wrapper | fixed vs `tuple[T, ...]` variadic        |
|  [06]   | `GenericTypeHint` / `TypeVarTypeHint` / `NewTypeTypeHint` | hint wrapper | PEP 484/585 generic, typevar, newtype |

[PUBLIC_TYPE_SCOPE]: vale annotated validators (`beartype.vale`)
- rail: type-enforcement
- compose into `Annotated[T, ...]`; `&`/`|`/`~` build boolean validator algebra checked in O(1) at the decorated boundary.

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY] | [RAIL]                                              |
| :-----: | :----------- | :------------ | :-------------------------------------------------- |
|  [01]   | `Is`         | validator     | `Is[lambda x: predicate]` arbitrary boolean constraint |
|  [02]   | `IsAttr`     | validator     | `IsAttr['attr', sub-validator]` attribute constraint |
|  [03]   | `IsEqual`    | validator     | `IsEqual[value]` equality constraint                |
|  [04]   | `IsInstance` | validator     | `IsInstance[cls]` runtime isinstance constraint     |
|  [05]   | `IsSubclass` | validator     | `IsSubclass[cls]` runtime issubclass constraint     |

[PUBLIC_TYPE_SCOPE]: roar exception/warning hierarchy (principal types)
- rail: type-enforcement
- all violations are catchable as `BeartypeCallHintViolation`; the full tree spans ~90 PEP-keyed subtypes — catch the canonical roots below at boundaries, not the leaves.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]  | [RAIL]                                       |
| :-----: | :-------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `BeartypeException`               | base exception | root of every beartype exception             |
|  [02]   | `BeartypeCallHintViolation`       | violation      | runtime call-boundary type violation (root) |
|  [03]   | `BeartypeCallHintParamViolation`  | violation      | parameter type-check failure                 |
|  [04]   | `BeartypeCallHintReturnViolation` | violation      | return type-check failure                    |
|  [05]   | `BeartypeDoorHintViolation`       | violation      | `die_if_unbearable`/`TypeHint.die_if_unbearable` failure |
|  [06]   | `BeartypeDecorException`          | decor error    | invalid decoration target or hint at decor time |
|  [07]   | `BeartypeConfException`           | config error   | invalid `BeartypeConf` parameter             |
|  [08]   | `BeartypeClawException`           | hook error     | import-hook installation/AST failure         |
|  [09]   | `BeartypeWarning`                 | base warning   | root of every beartype warning               |
|  [10]   | `BeartypeDecorHintPep585DeprecationWarning` | warning | deprecated PEP 585 hint form                |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: decorator and configuration
- rail: type-enforcement

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :--------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `beartype(obj=None, *, conf=BeartypeConf())` | decorator | type-check a callable or class (every method) |
|  [02]   | `beartype(conf=BeartypeConf(...))` | config mode    | return a configured decorator factory        |
|  [03]   | `BeartypeConf(*, strategy=O1, is_pep484_tower=False, is_pep557_fields=False, is_color=None, is_debug=False, is_random=True, violation_type=None, violation_param_type=None, violation_return_type=None, violation_door_type=None, violation_verbosity=DEFAULT, hint_overrides=FrozenDict({}), claw_skip_package_names=())` | config | build the immutable, cached config object |

[ENTRYPOINT_SCOPE]: door procedural type checks (`beartype.door`)
- rail: type-enforcement

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `is_bearable(obj, hint, *, conf=BeartypeConf()) -> TypeIs[T]` | type guard | narrowing boolean isinstance-for-any-hint    |
|  [02]   | `die_if_unbearable(obj, hint, *, conf=BeartypeConf()) -> None` | assertion | raise `BeartypeDoorHintViolation` on mismatch |
|  [03]   | `is_subhint(subhint, superhint) -> bool`                 | subtype query  | decidable hint subtyping (variance-aware)    |
|  [04]   | `TypeHint(hint).is_bearable(obj)` / `.die_if_unbearable(obj)` / `.is_subhint(other)` | OO check | object-oriented mirror of the procedural API |

[ENTRYPOINT_SCOPE]: claw import hooks (`beartype.claw`)
- rail: type-enforcement

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]  | [RAIL]                                       |
| :-----: | :------------------------------------ | :-------------- | :------------------------------------------- |
|  [01]   | `beartype_this_package(*, conf=...)`  | hook installer  | AST-hook the calling package at import time  |
|  [02]   | `beartype_package(package, *, conf=...)` | hook installer | hook one named package                       |
|  [03]   | `beartype_packages(packages, *, conf=...)` | hook installer | hook multiple named packages                 |
|  [04]   | `beartype_all(*, conf=...)`           | hook installer  | hook all subsequently imported modules       |
|  [05]   | `beartyping(*, conf=...)`             | context manager | transient `with`-scoped import hook          |

## [04]-[IMPLEMENTATION_LAW]

[BEARTYPE_TOPOLOGY]:
- `@beartype` rewrites the wrapped callable with generated, branch-minimal wrapper code; under `BeartypeStrategy.O1` (default) it samples one item from each container, so checking a million-element list stays constant-time. `Ologn`/`On` deepen container traversal; `O0` makes the decorator a no-op for opt-out hot paths.
- `is_random=True` randomizes the sampled container index per call so repeated calls cover different elements; set `is_random=False` for deterministic checks in tests.
- `BeartypeConf` is immutable, hashable, and memoized: identical kwargs return the same singleton, so create once and reuse across all decorations and claw hooks. It IS the configuration surface — never branch on environment to pick between decorators.
- `violation_type` / `violation_param_type` / `violation_return_type` / `violation_door_type` redirect raised violations to a caller-supplied `Exception` subtype; this is the first-class hook for mapping a type violation straight onto a domain error rail without a try/except adapter.
- `is_pep484_tower=True` admits the numeric tower (an `int` satisfies a `float`/`complex` hint, a `bytearray` satisfies `bytes`); `is_pep557_fields=True` type-checks `@dataclass` field assignments.
- `beartype.claw` installs an import-path AST transformer that decorates every callable/class in the hooked package at import — whole-package enforcement with zero per-function decoration. `claw_skip_package_names` excludes subpackages.
- the DOOR API (`beartype.door`) is the procedural counterpart: `is_bearable(obj, hint)` returns a `TypeIs[T]` (so a `True` result narrows the static type), `die_if_unbearable` asserts, and `is_subhint(a, b)` answers decidable hint subtyping for dispatch tables and registry validation.
- `beartype.vale` validators (`Is`, `IsAttr`, `IsEqual`, `IsInstance`, `IsSubclass`) subscript into `Annotated[T, validator]` and compose with `&`/`|`/`~`; the constraint is compiled into the same O(1) boundary check, so a refinement type costs nothing beyond the base hint.

[STACKS_WITH]:
- `pydantic`/`msgspec` (`.api/pydantic.md`, `.api/msgspec.md`): keep schema validation (coercion, JSON, settings) in pydantic/msgspec at the wire boundary; use `@beartype` for the *internal* call-boundary contract on domain functions that never touch the wire, and `beartype.vale.Is[...]` to express refinement predicates inside `Annotated` aliases that both beartype and a pydantic `Field`/`msgspec.Meta` annotation can share on one alias. `BeartypeConf(violation_type=DomainError)` folds a violation directly onto the domain error type.
- `expression` (`.api/expression.md`): at a boundary adapter, wrap the `@beartype`d call in a try-builder (`effect.try_`) and catch `BeartypeCallHintViolation`, lifting it into `Result.Error`; use `door.is_bearable(value, hint)` as the `TypeIs` guard that narrows before constructing an `Ok`. `door.is_subhint(...)` validates discriminated-union registry rows at startup.
- `beartype.typing`: a 106-symbol drop-in for `typing` (`Protocol`, `TypeVar`, `Annotated`, `TypedDict`, `Self`, `override`, `runtime_checkable`, the `List`/`Dict` aliases, ...) tuned so `Protocol`/`TypeVar` participate correctly in beartype-checked call sites across the supported interpreter band. Import typing names from here only when a hint is consumed at a `@beartype` boundary; otherwise prefer stdlib `typing`.

[LOCAL_ADMISSION]:
- Apply `@beartype` at service and domain-boundary entry points; use `beartype.claw` hooks (`beartype_this_package`) for whole-package enforcement at import time.
- Map violations onto domain errors via `BeartypeConf(violation_type=...)` rather than catching `BeartypeCallHintViolation` in every adapter; reserve the catch for the one egress boundary that lifts into `Result`.
- Use `door.is_bearable(obj, hint)` for the narrowing guard and `door.is_subhint(a, b)` for registry/dispatch validation; never hand-roll `isinstance` trees for parametrized hints.
- Encode refinement constraints once as `Annotated[T, beartype.vale.Is[...]]` aliases shared across the call site and any schema annotation.

[RAIL_LAW]:
- Package: `beartype`
- Owns: near-O(1) runtime type enforcement, import-hook activation, procedural DOOR checks + subhint reasoning, `Annotated` validator algebra, violation→domain-error redirection
- Accept: `@beartype` + a shared cached `BeartypeConf`, `beartype.claw` whole-package hooks, `beartype.door` guards, `beartype.vale` refinement annotations, `violation_type=` error mapping
- Reject: hand-rolled `isinstance`/`issubclass` guards at internal call sites, per-decoration ad-hoc `BeartypeConf` construction, catching `BeartypeException` broadly instead of the canonical violation roots

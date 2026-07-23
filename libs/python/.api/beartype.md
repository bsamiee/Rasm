# [PY_BRANCH_API_BEARTYPE]

`beartype` mints near-O(1) runtime type enforcement: `@beartype` rewrites a callable into branch-minimal validation code and `beartype.claw` hooks it across a whole package at import. It holds the internal call-boundary contract on domain functions the wire never touches, `BeartypeConf(violation_type=...)` folds every violation onto a domain error rail, and the DOOR API answers `isinstance`-for-any-hint checks and decidable subhint queries inline.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `beartype`
- package: `beartype` (MIT)
- module: `beartype`
- asset: runtime library
- rail: type-enforcement
- namespaces: `beartype`, `beartype.claw`, `beartype.door`, `beartype.vale`, `beartype.roar`, `beartype.typing`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration and strategy

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [CAPABILITY]                                          |
| :-----: | :--------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `BeartypeConf`               | config class   | immutable, hashable, cached decorator/hook config     |
|  [02]   | `BeartypeStrategy`           | strategy enum  | container-check depth: `O0`/`O1`/`Ologn`/`On`         |
|  [03]   | `BeartypeDecorPlace`         | place enum     | claw slot: `FIRST`/`LAST`/`LAST_BEFORE_DECOR_HOSTILE` |
|  [04]   | `BeartypeViolationVerbosity` | verbosity enum | error-message detail level                            |
|  [05]   | `FrozenDict`                 | immutable dict | hashable mapping value object (`hint_overrides`)      |

[PUBLIC_TYPE_SCOPE]: door type-hint wrappers (`beartype.door`)

`TypeHint(hint)` wraps any PEP-compatible hint into a comparable, iterable, indexable façade exposing `.args`, `.hint`, `.is_bearable`, `.is_subhint`, `.is_ignorable`; a subscripted, union, callable, or literal hint resolves to its concrete subclass.

[DOOR_SUBCLASSES]: `UnionTypeHint` `SubscriptedTypeHint` `CallableTypeHint` `ClassTypeHint` `LiteralTypeHint` `AnnotatedTypeHint` `TupleFixedTypeHint` `TupleVariableTypeHint` `GenericTypeHint` `TypeVarTypeHint` `NewTypeTypeHint`

[PUBLIC_TYPE_SCOPE]: vale annotated validators (`beartype.vale`)

Validators subscript into `Annotated[T, ...]` and compose with `&`/`|`/`~` into a boolean algebra compiled into the same O(1) boundary check, so a refinement type costs nothing beyond the base hint.

| [INDEX] | [SYMBOL]     | [CAPABILITY]                                           |
| :-----: | :----------- | :----------------------------------------------------- |
|  [01]   | `Is`         | `Is[lambda x: predicate]` arbitrary boolean constraint |
|  [02]   | `IsAttr`     | `IsAttr['attr', sub-validator]` attribute constraint   |
|  [03]   | `IsEqual`    | `IsEqual[value]` equality constraint                   |
|  [04]   | `IsInstance` | `IsInstance[cls]` runtime isinstance constraint        |
|  [05]   | `IsSubclass` | `IsSubclass[cls]` runtime issubclass constraint        |

[PUBLIC_TYPE_SCOPE]: roar exception/warning hierarchy

Every violation catches as `BeartypeCallHintViolation`; catch the canonical roots below at boundaries, never the PEP-keyed leaves.

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY]  | [CAPABILITY]                                             |
| :-----: | :------------------------------------------ | :------------- | :------------------------------------------------------- |
|  [01]   | `BeartypeException`                         | base exception | root of every beartype exception                         |
|  [02]   | `BeartypeCallHintViolation`                 | violation      | runtime call-boundary type violation (root)              |
|  [03]   | `BeartypeCallHintParamViolation`            | violation      | parameter type-check failure                             |
|  [04]   | `BeartypeCallHintReturnViolation`           | violation      | return type-check failure                                |
|  [05]   | `BeartypeDoorHintViolation`                 | violation      | `die_if_unbearable`/`TypeHint.die_if_unbearable` failure |
|  [06]   | `BeartypeDecorException`                    | decor error    | invalid decoration target or hint at decor time          |
|  [07]   | `BeartypeConfException`                     | config error   | invalid `BeartypeConf` parameter                         |
|  [08]   | `BeartypeClawException`                     | hook error     | import-hook installation/AST failure                     |
|  [09]   | `BeartypeWarning`                           | base warning   | root of every beartype warning                           |
|  [10]   | `BeartypeDecorHintPep585DeprecationWarning` | warning        | deprecated PEP 585 hint form                             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: decorator and configuration

| [INDEX] | [SURFACE]                                    | [SHAPE] | [CAPABILITY]                   |
| :-----: | :------------------------------------------- | :------ | :----------------------------- |
|  [01]   | `beartype(obj=None, *, conf=BeartypeConf())` | factory | type-check a callable or class |
|  [02]   | `beartype(conf=BeartypeConf(...))`           | factory | configured decorator factory   |
|  [03]   | `BeartypeConf(*, ...)`                       | ctor    | immutable cached config        |

- `BeartypeConf(*, strategy, is_pep484_tower, is_pep557_fields, is_color, is_debug, is_random, violation_type, violation_param_type, violation_return_type, violation_door_type, violation_verbosity, hint_overrides, claw_skip_package_names)`

[ENTRYPOINT_SCOPE]: door procedural type checks (`beartype.door`)

`is_bearable`/`die_if_unbearable` also take `*, conf=BeartypeConf()`; `TypeHint(hint)` mirrors all three as instance methods.

| [INDEX] | [SURFACE]                                | [SHAPE] | [CAPABILITY]                                  |
| :-----: | :--------------------------------------- | :------ | :-------------------------------------------- |
|  [01]   | `is_bearable(obj, hint) -> TypeIs[T]`    | static  | narrowing isinstance-for-any-hint guard       |
|  [02]   | `die_if_unbearable(obj, hint) -> None`   | static  | raise `BeartypeDoorHintViolation` on mismatch |
|  [03]   | `is_subhint(subhint, superhint) -> bool` | static  | decidable variance-aware hint subtyping       |

[ENTRYPOINT_SCOPE]: claw import hooks (`beartype.claw`)

| [INDEX] | [SURFACE]                                  | [SHAPE] | [CAPABILITY]                                |
| :-----: | :----------------------------------------- | :------ | :------------------------------------------ |
|  [01]   | `beartype_this_package(*, conf=...)`       | static  | AST-hook the calling package at import time |
|  [02]   | `beartype_package(package, *, conf=...)`   | static  | hook one named package                      |
|  [03]   | `beartype_packages(packages, *, conf=...)` | static  | hook multiple named packages                |
|  [04]   | `beartype_all(*, conf=...)`                | static  | hook all subsequently imported modules      |
|  [05]   | `beartyping(*, conf=...)`                  | factory | transient `with`-scoped import hook         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `@beartype` rewrites the callable into branch-minimal wrapper code; `O1` (default) samples one item per container, holding a check constant-time regardless of container size, `Ologn`/`On` deepen traversal, `O0` makes the decorator a no-op for opt-out hot paths.
- `is_random=True` (default) varies the sampled container index per call so repeated calls cover different elements; `is_random=False` fixes it for deterministic tests.
- `BeartypeConf` memoizes on its kwargs: identical kwargs return one singleton, minted once and reused across every decoration and claw hook.
- `violation_type`/`violation_param_type`/`violation_return_type`/`violation_door_type` redirect a raised violation to a caller-supplied `Exception` subtype — the first-class hook mapping a type violation onto a domain error rail with no try/except adapter.
- `is_pep484_tower=True` admits the numeric tower (`int` satisfies `float`/`complex`, `bytearray` satisfies `bytes`); `is_pep557_fields=True` type-checks `@dataclass` field assignments.
- `beartype.claw` installs an import-path AST transformer decorating every callable and class in a hooked package at import; `claw_skip_package_names` excludes subpackages.

[STACKING]:
- `pydantic`/`msgspec` (`.api/pydantic.md`, `.api/msgspec.md`): schema validation (coercion, JSON, settings) stays in pydantic/msgspec at the wire boundary while `@beartype` holds the internal call-boundary contract on domain functions off the wire; a `beartype.vale.Is[...]` predicate inside an `Annotated` alias is shared by a pydantic `Field`/`msgspec.Meta` annotation on the same alias, and `BeartypeConf(violation_type=DomainError)` folds a violation onto the domain error type.
- `expression` (`.api/expression.md`): a boundary adapter wraps the `@beartype`d call in `effect.try_` and catches `BeartypeCallHintViolation`, lifting it into `Result.Error`; `door.is_bearable(value, hint)` is the `TypeIs` guard narrowing before an `Ok`, and `door.is_subhint(...)` validates discriminated-union registry rows at startup.
- `beartype.typing`: re-exports `typing` tuned so `Protocol`/`TypeVar` participate correctly at `@beartype`-checked call sites; import a typing name from here only when the hint is consumed at a `@beartype` boundary, stdlib `typing` everywhere else.

[LOCAL_ADMISSION]:
- Apply `@beartype` at service and domain-boundary entry points, `beartype.claw` hooks (`beartype_this_package`) for whole-package enforcement at import.
- Map a violation onto a domain error via `BeartypeConf(violation_type=...)`, reserving the `BeartypeCallHintViolation` catch for the one egress boundary that lifts into `Result`.
- Use `door.is_bearable(obj, hint)` for the narrowing guard and `door.is_subhint(a, b)` for registry and dispatch validation.
- Encode a refinement constraint once as an `Annotated[T, beartype.vale.Is[...]]` alias shared across the call site and its schema annotation.

[RAIL_LAW]:
- Package: `beartype`
- Owns: near-O(1) runtime type enforcement, import-hook activation, procedural DOOR checks and subhint reasoning, `Annotated` validator algebra, violation→domain-error redirection
- Accept: `@beartype` with a shared cached `BeartypeConf`, `beartype.claw` whole-package hooks, `beartype.door` guards, `beartype.vale` refinement annotations, `violation_type=` error mapping
- Reject: hand-rolled `isinstance`/`issubclass` guards at internal call sites, per-decoration ad-hoc `BeartypeConf`, catching `BeartypeException` broadly instead of the canonical violation roots

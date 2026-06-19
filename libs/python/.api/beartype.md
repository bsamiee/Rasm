# [PY_BRANCH_API_BEARTYPE]

`beartype` supplies O(1) runtime type checking via the `@beartype` decorator, import-hook enforcement via `beartype.claw`, a typed exception hierarchy in `beartype.roar`, and a PEP-compatible extended `beartype.typing` namespace that adds `Protocol`, `TypeVar`, and `Annotated` with beartype-aware semantics.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `beartype`
- package: `beartype`
- module: `beartype`
- asset: runtime library
- rail: type-enforcement

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration and strategy
- rail: type-enforcement

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY]  | [RAIL]                          |
| :-----: | :--------------------------- | :------------- | :------------------------------ |
|  [01]   | `BeartypeConf`               | config class   | decorator/hook configuration    |
|  [02]   | `BeartypeStrategy`           | strategy enum  | check depth / sampling strategy |
|  [03]   | `BeartypeDecorPlace`         | place enum     | decoration placement flags      |
|  [04]   | `BeartypeViolationVerbosity` | verbosity enum | error message detail level      |
|  [05]   | `FrozenDict`                 | immutable dict | hashable dict value object      |

[PUBLIC_TYPE_SCOPE]: claw import hooks
- rail: type-enforcement

| [INDEX] | [SYMBOL]                | [TYPE_FAMILY]   | [RAIL]                           |
| :-----: | :---------------------- | :-------------- | :------------------------------- |
|  [01]   | `beartyping`            | context manager | transient import-hook scope      |
|  [02]   | `beartype_this_package` | hook installer  | hook current package at import   |
|  [03]   | `beartype_package`      | hook installer  | hook one package by name         |
|  [04]   | `beartype_packages`     | hook installer  | hook multiple packages by name   |
|  [05]   | `beartype_all`          | hook installer  | hook all future imports globally |

[PUBLIC_TYPE_SCOPE]: roar exception hierarchy (principal types)
- rail: type-enforcement

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY]  | [RAIL]                          |
| :-----: | :-------------------------------- | :------------- | :------------------------------ |
|  [01]   | `BeartypeException`               | base exception | all beartype exceptions root    |
|  [02]   | `BeartypeCallHintParamViolation`  | violation      | parameter type check failure    |
|  [03]   | `BeartypeCallHintReturnViolation` | violation      | return type check failure       |
|  [04]   | `BeartypeCallHintViolation`       | violation      | general call hint violation     |
|  [05]   | `BeartypeDecorException`          | decor error    | invalid decoration target       |
|  [06]   | `BeartypeConfException`           | config error   | invalid BeartypeConf            |
|  [07]   | `BeartypeClawException`           | hook error     | import hook failure             |
|  [08]   | `BeartypeDoorHintViolation`       | door violation | isinstance-check hint violation |
|  [09]   | `BeartypeWarning`                 | base warning   | all beartype warnings root      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: decorator
- rail: type-enforcement

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                              |
| :-----: | :--------------------------------- | :------------- | :---------------------------------- |
|  [01]   | `beartype(obj=None, *, conf=...)`  | decorator      | type-check callable or class        |
|  [02]   | `beartype(conf=BeartypeConf(...))` | config mode    | return configured decorator factory |

[ENTRYPOINT_SCOPE]: import hooks
- rail: type-enforcement

| [INDEX] | [SURFACE]                             | [ENTRY_FAMILY]  | [RAIL]                         |
| :-----: | :------------------------------------ | :-------------- | :----------------------------- |
|  [01]   | `beartype_this_package(conf=...)`     | hook installer  | hook calling package at import |
|  [02]   | `beartype_package(package, conf=...)` | hook installer  | hook named package             |
|  [03]   | `beartype_packages(*pkgs, conf=...)`  | hook installer  | hook multiple named packages   |
|  [04]   | `beartype_all(conf=...)`              | hook installer  | hook all future imports        |
|  [05]   | `beartyping(conf=...)`                | context manager | transient package-scoped hook  |

## [04]-[IMPLEMENTATION_LAW]

[BEARTYPE_TOPOLOGY]:
- namespaces: `beartype` (decorator + config), `beartype.claw` (import hooks), `beartype.roar` (exceptions + warnings), `beartype.typing` (extended typing namespace)
- `@beartype` checks types at call boundaries in O(1) time using generated wrapper code, not full traversal
- `BeartypeConf` is immutable and hashable; create once and reuse across all decorations
- `beartype.typing` re-exports all `typing` module members with beartype-aware `Protocol`, `TypeVar`, and `Annotated` variants that integrate with the O(1) checking strategy

[LOCAL_ADMISSION]:
- Apply `@beartype` at service and boundary entry points; use `beartype.claw` hooks for whole-package enforcement at import time.
- Catch `BeartypeCallHintParamViolation` / `BeartypeCallHintReturnViolation` at boundary adapters to convert to domain error types.
- Use `beartype.typing.Protocol` instead of `typing.Protocol` when protocols participate in beartype-checked call sites.

[RAIL_LAW]:
- Package: `beartype`
- Owns: O(1) runtime type enforcement, import-hook activation, violation exception hierarchy
- Accept: `@beartype` decorator, `BeartypeConf`, `beartype.claw` hooks, `beartype.typing` namespace
- Reject: hand-rolled isinstance guards at internal call sites when `@beartype` provides the same check

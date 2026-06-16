# [PY_RUNTIME_API_BEARTYPE]

`beartype` supplies near-constant-time runtime type checking: a decorator and import hook enforcing annotations at call boundaries, a configurable strategy/conf surface, and the DOOR procedural type-hint API for programmatic hint inspection and subhint checks. It is the runtime owner for boundary type enforcement.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `beartype`
- package: `beartype`
- import: `beartype`
- version: `0.23.0`
- owner: `runtime`
- rail: validation
- namespaces: `beartype`, `beartype.door`, `beartype.vale`, `beartype.claw`, `beartype.typing`, `beartype.roar`
- capability: runtime annotation enforcement, configurable checking strategy, validator metadata (`Is`), import-time hooking, procedural type-hint API

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: configuration family
- rail: validation

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `BeartypeConf` | config | checking configuration object |
| [2] | `BeartypeStrategy` | enum | check-time complexity strategy |
| [3] | `BeartypeDecorPlace` | enum | decoration placement policy |
| [4] | `BeartypeViolationVerbosity` | enum | violation message verbosity |
| [5] | `FrozenDict` | value | immutable mapping for conf |

[PUBLIC_TYPE_SCOPE]: DOOR type-hint family
- rail: validation

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `door.TypeHint` | hint wrapper | procedural type-hint object |
| [2] | `door.UnionTypeHint` | hint wrapper | union hint |
| [3] | `door.CallableTypeHint` | hint wrapper | callable hint |
| [4] | `door.LiteralTypeHint` | hint wrapper | literal hint |
| [5] | `door.AnnotatedTypeHint` | hint wrapper | annotated hint |
| [6] | `door.TupleFixedTypeHint` | hint wrapper | fixed-arity tuple hint |
| [7] | `vale.Is` | validator | predicate-based constraint |
| [8] | `vale.IsAttr` | validator | attribute-value constraint |
| [9] | `vale.IsEqual` | validator | equality constraint |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: validation

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `roar.BeartypeCallHintViolation` | fault | runtime type-check violation |
| [2] | `roar.BeartypeCallHintParamViolation` | fault | parameter-type violation |
| [3] | `roar.BeartypeCallHintReturnViolation` | fault | return-type violation |
| [4] | `roar.BeartypeDecorHintViolation` | fault | decoration-time hint error |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: enforcement operations
- rail: validation

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `beartype` | decorator | enforce annotations on callable |
| [2] | `BeartypeConf` | config build | strategy/verbosity configuration |
| [3] | `claw.beartype_this_package` | import hook | enforce whole-package annotations |
| [4] | `claw.beartype_package` | import hook | enforce named-package annotations |
| [5] | `door.is_bearable` | predicate | boolean hint conformance check |
| [6] | `door.die_if_unbearable` | guard | raise on hint nonconformance |
| [7] | `door.is_subhint` | relation | subtype relation between hints |
| [8] | `door.TypeHint` | wrap | build procedural hint object |

## [4]-[IMPLEMENTATION_LAW]

[VALIDATION_TOPOLOGY]:
- enforcement law: public boundary callables carry `@beartype`; whole-package enforcement is one `beartype_this_package()` import hook in the package root, never per-function repetition where a hook suffices.
- strategy law: the checking strategy is configured once on a shared `BeartypeConf` (O(1) sampling for hot paths, O(n) where exhaustive); strategy is a conf knob, never a per-call argument.
- constraint law: value constraints beyond nominal types are `Annotated[T, Is[...]]` validator metadata, never imperative assertions.
- introspection law: programmatic hint comparison uses the DOOR `is_subhint`/`is_bearable` surface, never reflective `typing` parsing.
- division of labor: beartype enforces nominal annotation shape at the call boundary; pydantic/msgspec own data coercion and structured validation. The two are layered, never duplicated.

[LOCAL_ADMISSION]:
- The boundary-conversion surface treats a `BeartypeCallHintViolation` as a boundary fault lifted into `Error(BoundaryFault(...))`; it never escapes into domain logic.
- One shared `BeartypeConf` is the single configuration owner; modules do not mint per-call configs.

[RAIL_LAW]:
- Package: `beartype`
- Owns: runtime annotation enforcement, the checking strategy/conf surface, validator metadata, import-time hooks, and the procedural type-hint API
- Accept: `@beartype` boundaries, a single import hook, shared `BeartypeConf`, `Annotated[..., Is[...]]` constraints, DOOR introspection
- Reject: per-function strategy arguments, imperative type assertions, reflective `typing` parsing, violation escape into domain logic

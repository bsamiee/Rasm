# [PY_RUNTIME_API_PYDANTIC]

`pydantic` v2 supplies validated domain models, field/model validators, computed fields, typed constraints, network/DSN value types, JSON-schema generation, and the `TypeAdapter` standalone-validation surface. It is the runtime owner for rich validated models and boundary admission where msgspec's struct surface is insufficient.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pydantic`
- package: `pydantic`
- import: `pydantic`
- version: `2.13.4`
- owner: `runtime`
- rail: validation
- namespaces: `pydantic`, `pydantic.dataclasses`, `pydantic.networks`, `pydantic.types`
- capability: validated models, field/model validators, computed fields, typed constraints, network/DSN types, JSON-schema generation, standalone validation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model and config family
- rail: validation

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY] | [RAIL]                          |
| :-----: | :--------------- | :------------ | :------------------------------ |
|   [1]   | `BaseModel`      | model base    | validated domain model          |
|   [2]   | `RootModel`      | model base    | single-root-value model         |
|   [3]   | `ConfigDict`     | config        | model behavior knobs            |
|   [4]   | `Field`          | field spec    | constraint/metadata descriptor  |
|   [5]   | `PrivateAttr`    | field spec    | non-validated private attribute |
|   [6]   | `AliasChoices`   | alias         | multi-name input alias          |
|   [7]   | `AliasPath`      | alias         | nested-path input alias         |
|   [8]   | `AliasGenerator` | alias         | programmatic alias generation   |
|   [9]   | `TypeAdapter`    | adapter       | standalone type validator       |

[PUBLIC_TYPE_SCOPE]: validator and serializer family
- rail: validation

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                         |
| :-----: | :----------------- | :------------ | :----------------------------- |
|   [1]   | `field_validator`  | decorator     | per-field validation hook      |
|   [2]   | `model_validator`  | decorator     | whole-model validation hook    |
|   [3]   | `computed_field`   | decorator     | derived serialized property    |
|   [4]   | `field_serializer` | decorator     | per-field serialization hook   |
|   [5]   | `model_serializer` | decorator     | whole-model serialization hook |
|   [6]   | `AfterValidator`   | annotation    | post-parse validator           |
|   [7]   | `BeforeValidator`  | annotation    | pre-parse validator            |
|   [8]   | `PlainValidator`   | annotation    | replacing validator            |
|   [9]   | `WrapValidator`    | annotation    | wrapping validator             |
|  [10]   | `PlainSerializer`  | annotation    | replacing serializer           |
|  [11]   | `WrapSerializer`   | annotation    | wrapping serializer            |
|  [12]   | `validate_call`    | decorator     | function-signature validation  |

[PUBLIC_TYPE_SCOPE]: constraint and network family
- rail: validation

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                  |
| :-----: | :------------------ | :------------ | :---------------------- |
|   [1]   | `StringConstraints` | constraint    | annotated string bounds |
|   [2]   | `Strict`            | constraint    | strict-coercion marker  |
|   [3]   | `InstanceOf`        | constraint    | isinstance validation   |
|   [4]   | `WithJsonSchema`    | constraint    | schema override         |
|   [5]   | `AnyUrl`            | network       | generic URL value       |
|   [6]   | `HttpUrl`           | network       | HTTP(S) URL value       |
|   [7]   | `PostgresDsn`       | network       | Postgres DSN value      |
|   [8]   | `RedisDsn`          | network       | Redis DSN value         |
|   [9]   | `IPvAnyAddress`     | network       | IPv4/IPv6 address value |
|  [10]   | `SecretStr`         | secret        | redacted string value   |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: validation

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :------------------------------ | :------------ | :---------------------------- |
|   [1]   | `ValidationError`               | fault         | aggregated validation failure |
|   [2]   | `PydanticUserError`             | fault         | model-misuse error            |
|   [3]   | `PydanticSchemaGenerationError` | fault         | schema-build failure          |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model operations
- rail: validation

| [INDEX] | [SURFACE]                          | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :--------------------------------- | :------------- | :------------------------------ |
|   [1]   | `BaseModel.model_validate`         | parse          | validate from mapping/object    |
|   [2]   | `BaseModel.model_validate_json`    | parse          | validate from JSON bytes/str    |
|   [3]   | `BaseModel.model_validate_strings` | parse          | validate string-typed input     |
|   [4]   | `BaseModel.model_construct`        | build          | trusted no-validation construct |
|   [5]   | `BaseModel.model_dump`             | serialize      | model to dict                   |
|   [6]   | `BaseModel.model_dump_json`        | serialize      | model to JSON bytes             |
|   [7]   | `BaseModel.model_copy`             | copy           | shallow/deep copy with updates  |
|   [8]   | `BaseModel.model_json_schema`      | schema         | JSON schema for model           |
|   [9]   | `BaseModel.model_rebuild`          | schema         | resolve deferred references     |
|  [10]   | `create_model`                     | factory        | dynamic model construction      |
|  [11]   | `TypeAdapter.validate_python`      | parse          | standalone python validation    |
|  [12]   | `TypeAdapter.validate_json`        | parse          | standalone JSON validation      |
|  [13]   | `TypeAdapter.dump_python`          | serialize      | standalone serialization        |

## [4]-[IMPLEMENTATION_LAW]

[VALIDATION_TOPOLOGY]:
- model law: rich validated models with validators, computed fields, or DSN/network types are `BaseModel`/`RootModel` with `ConfigDict(frozen=True, extra="forbid")`; pure wire structs without validation logic stay on msgspec.
- validator law: per-field rules are `@field_validator`; cross-field rules are `@model_validator(mode="after")`; the deprecated v1 `@validator` is banned.
- constraint law: bounded fields use `Annotated[T, Field(...)]` or `StringConstraints`, never imperative checks in `__init__`.
- boundary law: ingress validation calls `model_validate_json`; the raised `ValidationError` is lifted into `Error(BoundaryFault(...))` at the edge, never propagated as an exception into domain logic.
- standalone law: validating a non-model type uses one `TypeAdapter`, never a hand-rolled coercion function.

[LOCAL_ADMISSION]:
- Settings models compose `pydantic-settings.BaseSettings`; this page owns the model/validation surface those settings extend.
- Network and DSN fields use the typed `*Url`/`*Dsn` values, never raw `str` with manual parsing.
- `model_construct` is used only for trusted internal round-trips; all boundary admission goes through `model_validate*`.

[RAIL_LAW]:
- Package: `pydantic`
- Owns: validated domain models, validators, computed fields, typed constraints, network/DSN types, and standalone validation
- Accept: `BaseModel` with validators/computed fields, `Annotated` constraints, `TypeAdapter`, frozen `ConfigDict`
- Reject: v1 `@validator`, imperative `__init__` validation, raw-string DSNs, exception propagation past the boundary

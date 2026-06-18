# [PY_BRANCH_API_PYDANTIC]

`pydantic` supplies Python model validation via `BaseModel` and `RootModel`, a `TypeAdapter` for validating arbitrary annotated types, `ConfigDict` for model-level behaviour control, decorator-based `field_validator`/`model_validator`/`field_serializer`/`computed_field`, alias routing via `AliasPath`/`AliasChoices`, typed network/URL/secret primitives, and `validate_call` for function-level argument validation.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pydantic`
- package: `pydantic`
- module: `pydantic`
- asset: runtime library
- rail: validation

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model base types
- rail: validation

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]    | [RAIL]                                 |
| :-----: | :------------ | :--------------- | :------------------------------------- |
|   [1]   | `BaseModel`   | model base       | field-declared validated data model    |
|   [2]   | `RootModel`   | root model       | single-field typed model               |
|   [3]   | `TypeAdapter` | adapter class    | validate/serialize non-BaseModel types |
|   [4]   | `ConfigDict`  | config TypedDict | model configuration options            |

[PUBLIC_TYPE_SCOPE]: field construction
- rail: validation

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [RAIL]                            |
| :-----: | :--------------- | :--------------- | :-------------------------------- |
|   [1]   | `Field`          | field factory    | field default, alias, constraints |
|   [2]   | `PrivateAttr`    | private factory  | non-validated model attribute     |
|   [3]   | `computed_field` | property deco    | typed computed property on model  |
|   [4]   | `AliasPath`      | alias descriptor | dotted / indexed alias path       |
|   [5]   | `AliasChoices`   | alias descriptor | multiple acceptable alias forms   |
|   [6]   | `AliasGenerator` | alias generator  | callable-based alias strategy     |

[PUBLIC_TYPE_SCOPE]: validators and serializers
- rail: validation

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                               |
| :-----: | :----------------- | :------------ | :----------------------------------- |
|   [1]   | `field_validator`  | decorator     | field-level validator function       |
|   [2]   | `model_validator`  | decorator     | model-level validator (before/after) |
|   [3]   | `field_serializer` | decorator     | field-level serializer override      |
|   [4]   | `PlainValidator`   | annotation    | override entire field validation     |
|   [5]   | `BeforeValidator`  | annotation    | pre-process annotation validator     |
|   [6]   | `AfterValidator`   | annotation    | post-process annotation validator    |
|   [7]   | `WrapValidator`    | annotation    | wrap-mode annotation validator       |
|   [8]   | `PlainSerializer`  | annotation    | override field serialization         |
|   [9]   | `WrapSerializer`   | annotation    | wrap-mode field serializer           |

[PUBLIC_TYPE_SCOPE]: error types
- rail: validation

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]    | [RAIL]                           |
| :-----: | :------------------------------ | :--------------- | :------------------------------- |
|   [1]   | `ValidationError`               | validation error | structured field error container |
|   [2]   | `PydanticUserError`             | config error     | invalid model configuration      |
|   [3]   | `PydanticSchemaGenerationError` | schema error     | JSON Schema generation failure   |
|   [4]   | `PydanticImportError`           | import error     | missing optional dependency      |

[PUBLIC_TYPE_SCOPE]: network / constrained types (selection)
- rail: validation

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]  | [RAIL]                         |
| :-----: | :------------ | :------------- | :----------------------------- |
|   [1]   | `AnyUrl`      | annotated type | any URL string with validation |
|   [2]   | `HttpUrl`     | annotated type | HTTP/HTTPS URL                 |
|   [3]   | `AnyHttpUrl`  | annotated type | any HTTP URL                   |
|   [4]   | `PostgresDsn` | annotated type | PostgreSQL connection string   |
|   [5]   | `SecretStr`   | secret type    | str hidden in repr             |
|   [6]   | `SecretBytes` | secret type    | bytes hidden in repr           |
|   [7]   | `Json`        | annotated type | JSON-encoded string field      |
|   [8]   | `EmailStr`    | annotated type | validated email address        |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model operations
- rail: validation

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY]  | [RAIL]                             |
| :-----: | :-------------------------------------------------- | :-------------- | :--------------------------------- |
|   [1]   | `BaseModel(**data)`                                 | constructor     | validate and construct model       |
|   [2]   | `BaseModel.model_validate(obj, *, strict, context)` | class method    | validate dict or object            |
|   [3]   | `BaseModel.model_validate_json(json_data)`          | class method    | validate JSON bytes or string      |
|   [4]   | `BaseModel.model_dump(*, mode, exclude, include)`   | instance method | serialize model to dict            |
|   [5]   | `BaseModel.model_dump_json(*, indent, exclude)`     | instance method | serialize model to JSON bytes      |
|   [6]   | `BaseModel.model_copy(*, update, deep)`             | instance method | copy with optional field overrides |
|   [7]   | `BaseModel.model_fields_set`                        | property        | set of fields explicitly provided  |
|   [8]   | `BaseModel.model_json_schema(*, mode)`              | class method    | JSON Schema dict for model         |

[ENTRYPOINT_SCOPE]: TypeAdapter and validate_call
- rail: validation

| [INDEX] | [SURFACE]                                            | [ENTRY_FAMILY]  | [RAIL]                               |
| :-----: | :--------------------------------------------------- | :-------------- | :----------------------------------- |
|   [1]   | `TypeAdapter(type, *, config)`                       | constructor     | adapter for arbitrary annotated type |
|   [2]   | `TypeAdapter.validate_python(value, *, strict)`      | instance method | validate Python object               |
|   [3]   | `TypeAdapter.validate_json(data, *, strict)`         | instance method | validate JSON bytes                  |
|   [4]   | `TypeAdapter.dump_python(instance, *, mode)`         | instance method | serialize to Python                  |
|   [5]   | `TypeAdapter.json_schema(*, mode)`                   | instance method | JSON Schema for the type             |
|   [6]   | `validate_call(func, /, *, config, validate_return)` | decorator       | validate function arguments          |
|   [7]   | `create_model(model_name, /, **field_definitions)`   | factory         | programmatic BaseModel subclass      |

[ENTRYPOINT_SCOPE]: decorator hooks
- rail: validation

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                           |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------- |
|   [1]   | `field_validator(field, /, *fields, mode, check_fields)` | decorator      | attach field validator to model  |
|   [2]   | `model_validator(*, mode)`                               | decorator      | attach model-level validator     |
|   [3]   | `field_serializer(field, /, *fields, mode, return_type)` | decorator      | attach field serializer to model |
|   [4]   | `computed_field(func, /, *, alias, title, return_type)`  | decorator      | declare computed property field  |

## [4]-[IMPLEMENTATION_LAW]

[PYDANTIC_TOPOLOGY]:
- namespaces: `pydantic` (all public API), `pydantic.dataclasses` (dataclass integration)
- `BaseModel` fields are declared as class annotations; validation runs at `__init__` time
- `ConfigDict` keys: `strict`, `frozen`, `populate_by_name`, `use_enum_values`, `arbitrary_types_allowed`, `model_serializer`, `json_schema_extra`, `alias_generator`, `validate_default`, `extra`
- `TypeAdapter` wraps any annotated type (including `Annotated[T, ...]` and plain `list[T]`) for standalone validate/dump
- `ValidationError` carries a `errors()` list of error dicts with `loc`, `type`, `msg`, and `input` fields

[LOCAL_ADMISSION]:
- Define wire and domain models as `BaseModel` subclasses with `ConfigDict`; use `TypeAdapter` for validation of types that do not warrant a model class.
- Catch `ValidationError` at boundary intake and map to domain error types before entering domain logic.
- Use `AliasPath` / `AliasChoices` for wire-to-domain field name mapping; keep internal code using canonical names.
- Use `model_dump(mode='json')` when the output must be JSON-safe primitives without explicit serialization.

[RAIL_LAW]:
- Package: `pydantic`
- Owns: model validation, field validation/serialization, type adapters, JSON Schema generation
- Accept: `BaseModel`, `TypeAdapter`, `Field`, `model_validator`, `field_validator`, `validate_call`
- Reject: hand-rolled dict-key validation, manual isinstance cascades replacing `TypeAdapter.validate_python`

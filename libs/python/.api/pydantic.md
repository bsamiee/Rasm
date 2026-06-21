# [PY_BRANCH_API_PYDANTIC]

`pydantic` supplies pydantic-core-backed model validation: `BaseModel`/`RootModel` field-declared records, a `TypeAdapter` that validates any annotated type without a model class, `ConfigDict` model-behaviour control, the four-mode validator/serializer annotation algebra (`Plain`/`Before`/`After`/`Wrap`), callable-tag discriminated unions (`Discriminator`/`Tag`), alias routing (`AliasPath`/`AliasChoices`/`AliasGenerator`), a deep constrained/network/secret type catalogue, `validate_call` argument validation, and an experimental transform `pipeline` plus the `MISSING` presence sentinel.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pydantic`
- package: `pydantic`
- module: `pydantic`
- version: `2.13.4` (floor `>=2.13.4`)
- license: `MIT`
- asset: pure-Python validation layer over the `pydantic-core` Rust extension (`pydantic_core._pydantic_core`); `pydantic` itself is `py3-none-any`, but the resolved environment carries the per-interpreter `pydantic-core` binary wheel that owns the validation engine
- abi: `pydantic` pure-Python (`py3-none-any`), `Requires-Python >=3.9`; validation speed and the compiled `SchemaValidator`/`SchemaSerializer` come from the `cp315`-tagged `pydantic-core`
- rail: validation
- namespaces: `pydantic` (all public API), `pydantic.dataclasses` (stdlib-dataclass integration), `pydantic.json_schema` (`GenerateJsonSchema`), `pydantic.experimental.pipeline`, `pydantic.experimental.missing_sentinel`, `pydantic.experimental.arguments_schema`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model base types
- rail: validation

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]    | [RAIL]                                                                   |
| :-----: | :------------ | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `BaseModel`   | model base       | field-declared validated data model; subclass `model_config` drives behaviour |
|  [02]   | `RootModel`   | root model       | single `root`-field typed model; wraps a non-dict top-level shape         |
|  [03]   | `TypeAdapter` | adapter class    | validate/serialize/schema any annotated type without a `BaseModel`        |
|  [04]   | `ConfigDict`  | config TypedDict | model configuration options applied via `model_config`                   |
|  [05]   | `with_config` | decorator        | attach a `ConfigDict` to a stdlib `TypedDict`/`dataclass` for adapter use |
|  [06]   | `BaseConfig`  | legacy config    | deprecated v1 class-based config; superseded by `ConfigDict`              |

[PUBLIC_TYPE_SCOPE]: field construction and discrimination
- rail: validation

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [RAIL]                                                          |
| :-----: | :--------------- | :--------------- | :------------------------------------------------------------- |
|  [01]   | `Field`          | field factory    | default, alias, constraint, `discriminator`, `json_schema_extra` |
|  [02]   | `PrivateAttr`    | private factory  | non-validated model attribute (leading-underscore field)        |
|  [03]   | `computed_field` | property deco    | typed computed property serialized into model output            |
|  [04]   | `AliasPath`      | alias descriptor | dotted / indexed alias path into nested wire input              |
|  [05]   | `AliasChoices`   | alias descriptor | ordered list of acceptable alias forms                          |
|  [06]   | `AliasGenerator` | alias generator  | distinct callables for validation vs serialization aliases      |
|  [07]   | `Discriminator`  | union tag        | callable/field discriminant for a tagged union                 |
|  [08]   | `Tag`            | union member tag | labels a union member resolved by a callable `Discriminator`    |

[PUBLIC_TYPE_SCOPE]: validator / serializer annotation algebra
- rail: validation

| [INDEX] | [SYMBOL]           | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :----------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `field_validator`  | decorator     | field-level validator function (`mode='before'/'after'/'wrap'/'plain'`) |
|  [02]   | `model_validator`  | decorator     | model-level validator (`mode='before'/'after'/'wrap'`)        |
|  [03]   | `field_serializer` | decorator     | field-level serializer override (`mode='plain'/'wrap'`)       |
|  [04]   | `model_serializer` | decorator     | whole-model serializer override                              |
|  [05]   | `PlainValidator`   | annotation    | replace entire field validation with the callable            |
|  [06]   | `BeforeValidator`  | annotation    | run before inner validation, transforms raw input            |
|  [07]   | `AfterValidator`   | annotation    | run after inner validation, transforms parsed value          |
|  [08]   | `WrapValidator`    | annotation    | wrap inner validation with a handler callable                |
|  [09]   | `PlainSerializer`  | annotation    | replace field serialization with the callable                |
|  [10]   | `WrapSerializer`   | annotation    | wrap inner serialization with a handler callable             |
|  [11]   | `ValidationInfo`   | context arg   | validator second arg: `data`/`context`/`field_name`/`mode`/`config` |
|  [12]   | `SerializationInfo`| context arg   | serializer second arg: `mode`/`context`/`by_alias`/`exclude_*` |
|  [13]   | `FieldSerializationInfo` | context arg | field-serializer info carrying `field_name` plus serialization flags |

[PUBLIC_TYPE_SCOPE]: schema control and validation modifiers
- rail: validation

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]    | [RAIL]                                                       |
| :-----: | :-------------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `WithJsonSchema`      | annotation       | override the JSON Schema emitted for an annotated field      |
|  [02]   | `GetPydanticSchema`   | annotation       | supply a custom core schema for a type without a model       |
|  [03]   | `GetCoreSchemaHandler`| handler protocol | `__get_pydantic_core_schema__` inner-schema accessor         |
|  [04]   | `GetJsonSchemaHandler`| handler protocol | `__get_pydantic_json_schema__` inner-schema accessor         |
|  [05]   | `InstanceOf`          | annotation       | accept any instance of T without re-validating its fields    |
|  [06]   | `SkipValidation`      | annotation       | bypass validation for the annotated field                    |
|  [07]   | `SerializeAsAny`      | annotation       | duck-type serialization using the runtime value's own schema |
|  [08]   | `Strict`              | annotation       | force strict (no-coercion) validation on the field           |
|  [09]   | `StringConstraints`   | annotation       | `strip_whitespace`/`to_lower`/`pattern`/`min_length` bundle  |
|  [10]   | `AllowInfNan`         | annotation       | permit/forbid `inf`/`nan` on a float field                   |
|  [11]   | `FailFast`            | annotation       | stop sequence validation at the first failing element        |
|  [12]   | `OnErrorOmit`         | annotation       | drop a failing element from a collection instead of erroring |

[PUBLIC_TYPE_SCOPE]: error types
- rail: validation

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]    | [RAIL]                                                  |
| :-----: | :------------------------------ | :--------------- | :------------------------------------------------------ |
|  [01]   | `ValidationError`               | validation error | structured field error container; `.errors()` / `.json()` |
|  [02]   | `PydanticUserError`             | config error     | invalid model/usage configuration                       |
|  [03]   | `PydanticSchemaGenerationError` | schema error     | core schema could not be generated for a type           |
|  [04]   | `PydanticInvalidForJsonSchema`  | schema error     | type has no JSON Schema representation                  |
|  [05]   | `PydanticUndefinedAnnotation`   | schema error     | a forward-ref annotation is unresolved at build time    |
|  [06]   | `PydanticImportError`           | import error     | missing optional dependency (e.g. `email-validator`)    |
|  [07]   | `PydanticForbiddenQualifier`    | usage error      | disallowed `ClassVar`/`Final` qualifier on a field      |

[PUBLIC_TYPE_SCOPE]: constrained / network / secret / encoded types (selection)
- rail: validation

| [INDEX] | [SYMBOL]                                   | [TYPE_FAMILY]   | [RAIL]                                                  |
| :-----: | :----------------------------------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `Strict{Bool,Int,Float,Str,Bytes}`         | strict scalars  | no-coercion typed scalars                               |
|  [02]   | `Positive{Int,Float}` / `NonNegative{Int,Float}` / `Negative*` / `NonPositive*` | sign-bounded | sign-constrained numeric aliases   |
|  [03]   | `FiniteFloat`                              | constrained     | rejects `inf`/`nan`                                     |
|  [04]   | `conint`/`confloat`/`condecimal`/`constr`/`conbytes`/`condate`/`conlist`/`conset`/`confrozenset` | constraint factory | callable constructors for ad-hoc bounded types |
|  [05]   | `AnyUrl`/`HttpUrl`/`AnyHttpUrl`/`FileUrl`/`FtpUrl`/`WebsocketUrl`/`AnyWebsocketUrl` | URL types | parsed/validated URL value objects             |
|  [06]   | `PostgresDsn`/`MySQLDsn`/`MariaDBDsn`/`CockroachDsn`/`MongoDsn`/`RedisDsn`/`KafkaDsn`/`AmqpDsn`/`NatsDsn`/`ClickHouseDsn`/`SnowflakeDsn` | DSN types | scheme-checked connection strings    |
|  [07]   | `UrlConstraints`                           | annotation      | scheme/host/port constraints for custom URL types       |
|  [08]   | `IPvAnyAddress`/`IPvAnyInterface`/`IPvAnyNetwork` | IP types | validated IPv4/IPv6 forms                          |
|  [09]   | `SecretStr`/`SecretBytes`/`Secret`         | secret types    | value hidden in `repr`/serialization; `.get_secret_value()` |
|  [10]   | `Json`/`JsonValue`                         | JSON types      | parse-on-validate JSON field / recursive JSON value type |
|  [11]   | `Base64Bytes`/`Base64Str`/`Base64UrlBytes`/`Base64UrlStr`/`EncodedBytes`/`EncodedStr`/`EncoderProtocol`/`Base64Encoder` | encoded | transparent encode/decode on validate/serialize |
|  [12]   | `EmailStr`/`NameEmail`                     | email types     | RFC email validation (needs `email-validator`)          |
|  [13]   | `AwareDatetime`/`NaiveDatetime`/`PastDatetime`/`FutureDatetime`/`PastDate`/`FutureDate` | temporal | tz-aware / temporal-bound datetime constraints |
|  [14]   | `FilePath`/`DirectoryPath`/`NewPath`/`SocketPath`/`ImportString` | path/import | existence-checked paths / dotted-path importer  |
|  [15]   | `UUID1`..`UUID8`/`PaymentCardNumber`/`ByteSize` | domain scalars | version-checked UUID / Luhn card / human byte size  |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model operations
- rail: validation

| [INDEX] | [SURFACE]                                                                            | [ENTRY_FAMILY]  | [RAIL]                                          |
| :-----: | :----------------------------------------------------------------------------------- | :-------------- | :----------------------------------------------- |
|  [01]   | `BaseModel(**data)`                                                                  | constructor     | validate and construct model                     |
|  [02]   | `BaseModel.model_validate(obj, *, strict, from_attributes, context, by_alias, by_name)` | class method | validate dict or attribute object                |
|  [03]   | `BaseModel.model_validate_json(json_data, *, strict, context, by_alias, by_name)`    | class method    | validate JSON bytes/str through pydantic-core    |
|  [04]   | `BaseModel.model_validate_strings(obj, *, strict, context)`                          | class method    | validate an all-string mapping (env/query coercion) |
|  [05]   | `BaseModel.model_dump(*, mode, include, exclude, by_alias, exclude_unset, exclude_defaults, exclude_none, round_trip, context, serialize_as_any, fallback, warnings)` | instance method | serialize to dict |
|  [06]   | `BaseModel.model_dump_json(*, indent, include, exclude, by_alias, exclude_*, round_trip, context, serialize_as_any, fallback)` | instance method | serialize to JSON string (core serializer) |
|  [07]   | `BaseModel.model_copy(*, update, deep)`                                              | instance method | copy with field overrides                        |
|  [08]   | `BaseModel.model_construct(_fields_set, **values)`                                   | class method    | construct WITHOUT validation (trusted data fast path) |
|  [09]   | `BaseModel.model_json_schema(*, by_alias, ref_template, schema_generator, mode)`     | class method    | JSON Schema dict for the model                   |
|  [10]   | `BaseModel.model_rebuild(*, force, raise_errors, _types_namespace)`                  | class method    | resolve deferred forward refs after definition   |
|  [11]   | `BaseModel.model_post_init(context, /)`                                              | hook            | post-construction hook (overridable)             |
|  [12]   | `BaseModel.model_fields` / `model_computed_fields` / `model_fields_set` / `model_extra` / `model_config` | introspection | field map / computed map / set fields / extras / config |

[ENTRYPOINT_SCOPE]: TypeAdapter, validate_call, create_model
- rail: validation

| [INDEX] | [SURFACE]                                                                       | [ENTRY_FAMILY]  | [RAIL]                                          |
| :-----: | :------------------------------------------------------------------------------ | :-------------- | :----------------------------------------------- |
|  [01]   | `TypeAdapter(type, *, config, _parent_depth)`                                   | constructor     | adapter for any annotated type                   |
|  [02]   | `TypeAdapter.validate_python(obj, /, *, strict, from_attributes, context, experimental_allow_partial, by_alias, by_name)` | instance method | validate Python object; `experimental_allow_partial` decodes truncated streams |
|  [03]   | `TypeAdapter.validate_json(data, /, *, strict, context, experimental_allow_partial)` | instance method | validate JSON bytes/str                          |
|  [04]   | `TypeAdapter.validate_strings(obj, /, *, strict, context)`                      | instance method | validate all-string mapping                      |
|  [05]   | `TypeAdapter.dump_python(instance, /, *, mode, include, exclude, by_alias, round_trip, context, serialize_as_any)` | instance method | serialize to Python                              |
|  [06]   | `TypeAdapter.dump_json(instance, /, *, indent, by_alias, exclude_*, context)`   | instance method | serialize to JSON bytes                          |
|  [07]   | `TypeAdapter.json_schema(*, by_alias, ref_template, schema_generator, mode)`    | instance method | JSON Schema for the type                         |
|  [08]   | `TypeAdapter.json_schemas(inputs, *, by_alias, ref_template, schema_generator)` | static method   | combined `$defs` schema for many adapters        |
|  [09]   | `validate_call(func, /, *, config, validate_return)`                            | decorator       | validate function arguments (and return)         |
|  [10]   | `create_model(model_name, /, *, __config__, __base__, __validators__, **field_definitions)` | factory | programmatic `BaseModel` subclass                |

[ENTRYPOINT_SCOPE]: decorator hooks and experimental rails
- rail: validation

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [RAIL]                                              |
| :-----: | :----------------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `field_validator(field, /, *fields, mode, check_fields)`                 | decorator      | attach field validator to model                      |
|  [02]   | `model_validator(*, mode)`                                               | decorator      | attach model-level validator                         |
|  [03]   | `field_serializer(field, /, *fields, mode, return_type, when_used)`      | decorator      | attach field serializer to model                     |
|  [04]   | `model_serializer(f, /, *, mode, return_type, when_used)`                | decorator      | attach whole-model serializer                        |
|  [05]   | `computed_field(func, /, *, alias, title, return_type, repr, when_used)` | decorator      | declare computed property field                      |
|  [06]   | `pydantic.experimental.pipeline.validate_as(tp)`                         | builder        | fluent `.transform()`/`.constrain()`/`.gt()` `Annotated` validation pipeline |
|  [07]   | `pydantic.experimental.missing_sentinel.MISSING`                         | sentinel       | distinguishes "absent" from `None` on optional fields (omitted from dump) |
|  [08]   | `pydantic.experimental.arguments_schema.generate_arguments_schema(func, *, schema_type, parameters_callback)` | builder | core schema for a callable's argument signature      |

## [04]-[IMPLEMENTATION_LAW]

[PYDANTIC_TOPOLOGY]:
- validation engine lives in the compiled `pydantic-core` (`SchemaValidator`/`SchemaSerializer`); a `BaseModel` subclass builds its core schema once at class creation, and every `model_validate*`/`model_dump*` call dispatches into that compiled validator — never a Python-level field loop
- the validator/serializer algebra has exactly four modes — `Before`/`After`/`Wrap`/`Plain` — exposed twice: as `Annotated[T, BeforeValidator(...)]` annotations (composable, reusable across fields and `TypeAdapter`s) and as `@field_validator(mode=...)` / `@model_validator(mode=...)` decorators (model-scoped); both compile to the same core schema, so prefer the `Annotated` form for type-level reuse and the decorator form for cross-field model invariants
- callable discriminated unions: `Annotated[A | B, Discriminator(callable)]` with members tagged `Annotated[A, Tag('a')]` selects the member by the callable's return value, generalizing the field-name `Field(discriminator='kind')` form; the discriminant is resolved inside `pydantic-core`, not by a Python `match`
- `ConfigDict` keys: `strict`, `frozen`, `populate_by_name`/`validate_by_name`/`validate_by_alias`, `use_enum_values`, `arbitrary_types_allowed`, `extra` (`ignore`/`forbid`/`allow`), `alias_generator`, `validate_default`, `validate_assignment`, `revalidate_instances`, `ser_json_bytes`/`ser_json_timedelta`, `json_schema_extra`, `json_schema_mode_override`, `coerce_numbers_to_str`, `defer_build`, `model_title_generator`
- `TypeAdapter` builds and caches a core schema for any annotation (`list[T]`, `dict[K, V]`, `Annotated[T, ...]`, unions, `TypedDict`); it is the standalone validator for types that do not warrant a model class, and `validate_python(experimental_allow_partial='trailing-strings')` decodes truncated/streaming JSON
- `ValidationError.errors(include_url=, include_context=, include_input=)` yields error dicts with `type`/`loc`/`msg`/`input`/`ctx`/`url`; `.json()` emits the same as a JSON string; the `loc` tuple is the field path used to map a wire failure to a domain field
- `model_construct` skips validation entirely (trusts the caller) — the fast path for re-hydrating already-validated data; `model_copy(update=...)` does NOT re-validate the updated fields

[STACKS_WITH]:
- msgspec at the boundary: `msgspec` owns the zero-copy wire decode and `pydantic` owns rich cross-field/business validation. A `msgspec.Struct` tagged union (`tag=`, `tag_field=`) decodes the transport frame; `msgspec.to_builtins(struct)` projects it to JSON-safe builtins that feed `TypeAdapter.validate_python` (or `model_validate`), where a `Discriminator`/`Tag` union re-expresses the same discriminant for the validated domain model. A custom scalar resolved by a msgspec `Decoder(dec_hook=...)` is the same scalar annotated `Annotated[T, BeforeValidator(...)]` on the pydantic side — one discriminant vocabulary and one scalar codec span both validators, no parallel DTO.
- beartype co-enforcement: `@beartype` guards call-site arg/return shape in O(1) at every internal boundary, while `pydantic` owns coercion-and-constraint validation at the data edge. `validate_call(validate_return=True)` is the pydantic mirror of `@beartype` for functions whose arguments need coercion (URL/DSN/secret parsing) rather than nominal checking; keep `@beartype` for hot internal functions and `validate_call` only where boundary coercion is actually required, never both on one function.
- structlog error rendering: catch `ValidationError` at intake and feed `exc.errors(include_url=False)` into the structlog event dict as a structured `errors` key (a list of `{type, loc, msg}` records) rather than rendering `str(exc)`; under a `JSONRenderer` pipeline the per-field failure path stays machine-queryable, and `loc` becomes the field-path key correlated with the request's bound context.
- opentelemetry attributes: `model_dump(mode='json', exclude_none=True)` yields the JSON-primitive map whose flat scalar entries are admissible `Span.set_attributes` values; a `computed_field` projects a derived scalar (status, size class) that becomes a span attribute without a separate flattener. Map a caught `ValidationError` to `span.set_status(StatusCode.ERROR)` plus `span.record_exception` carrying the structured `errors()` payload.
- anyio task scope: `TypeAdapter` instances are immutable and thread-safe once built — construct them at module import and reuse across `anyio` worker tasks; validation itself is CPU-bound in `pydantic-core`, so offload large-batch `validate_json` through `anyio.to_thread.run_sync` to keep the event loop responsive rather than awaiting validation inline.

[LOCAL_ADMISSION]:
- Define domain models as `BaseModel` subclasses with an explicit `model_config = ConfigDict(...)`; reserve `TypeAdapter` for collection/union/`TypedDict` shapes that do not warrant a class, and build the adapter once at module scope.
- Use the `Annotated[T, BeforeValidator/AfterValidator/...]` annotation form for reusable cross-model validation/serialization; reserve the `@field_validator`/`@model_validator` decorators for invariants that span multiple fields of one model.
- Model tagged wire variants with `Annotated[A | B, Discriminator(...)]` + `Tag(...)` (or `Field(discriminator='kind')` for a literal field tag); never branch a `match` over a `kind` string after construction.
- Catch `ValidationError` at boundary intake and map `.errors()` to domain error rails before entering domain logic; use `model_construct` only to re-hydrate data already validated upstream.
- Route wire-to-domain field renaming through `AliasPath`/`AliasChoices`/`AliasGenerator`; keep internal code on canonical field names and emit with `by_alias=True` only at egress.
- Use `model_dump(mode='json')` for JSON-safe primitives without per-field serializers; use `SerializeAsAny` only where a base-typed field must serialize via the runtime subclass schema.

[RAIL_LAW]:
- Package: `pydantic`
- Owns: model/field validation, the validator/serializer mode algebra, discriminated-union resolution, type adapters, constrained/network/secret types, JSON Schema generation, function-argument validation
- Accept: `BaseModel`, `RootModel`, `TypeAdapter`, `Field`, `Discriminator`/`Tag`, `model_validator`/`field_validator`, `Annotated[..., BeforeValidator/AfterValidator/WrapValidator/PlainValidator]`, `validate_call`
- Reject: hand-rolled dict-key validation, manual isinstance cascades replacing `TypeAdapter.validate_python`, post-construction `match` over a discriminant field, parallel DTO types for one wire shape, `str(ValidationError)` where `.errors()` carries the structured path

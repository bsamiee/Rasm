# [PY_BRANCH_API_PYDANTIC]

`pydantic` owns model-class validation and serialization, compiling a per-class core schema once at class creation and dispatching every validate/dump into the C-extension `pydantic-core`. It binds `BaseModel`/`RootModel` records, a `TypeAdapter` validating any annotated type with no model class, the four-mode validator/serializer algebra, callable-tag discriminated unions, alias routing, and a constrained/network/secret scalar catalogue. It is the validation rail's model owner — a cross-field-validated record mints as a `BaseModel`, a schemaless shape as a `TypeAdapter`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `pydantic`
- package: `pydantic` (MIT)
- module: `pydantic`
- namespaces: `pydantic` (public API), `pydantic.dataclasses` (stdlib-dataclass integration), `pydantic.json_schema` (`GenerateJsonSchema`), `pydantic.experimental.pipeline`, `pydantic.experimental.missing_sentinel`, `pydantic.experimental.arguments_schema`
- rail: validation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: model base types

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]    | [CAPABILITY]                                                                  |
| :-----: | :------------ | :--------------- | :---------------------------------------------------------------------------- |
|  [01]   | `BaseModel`   | model base       | field-declared validated data model; subclass `model_config` drives behaviour |
|  [02]   | `RootModel`   | root model       | single `root`-field typed model; wraps a non-dict top-level shape             |
|  [03]   | `TypeAdapter` | adapter class    | validate/serialize/schema any annotated type without a `BaseModel`            |
|  [04]   | `ConfigDict`  | config TypedDict | model configuration options applied via `model_config`                        |
|  [05]   | `with_config` | decorator        | attach a `ConfigDict` to a stdlib `TypedDict`/`dataclass` for adapter use     |

[PUBLIC_TYPE_SCOPE]: field construction and discrimination

| [INDEX] | [SYMBOL]         | [TYPE_FAMILY]    | [CAPABILITY]                                                     |
| :-----: | :--------------- | :--------------- | :--------------------------------------------------------------- |
|  [01]   | `Field`          | field factory    | default, alias, constraint, `discriminator`, `json_schema_extra` |
|  [02]   | `PrivateAttr`    | private factory  | non-validated model attribute (leading-underscore field)         |
|  [03]   | `computed_field` | property deco    | typed computed property serialized into model output             |
|  [04]   | `AliasPath`      | alias descriptor | dotted / indexed alias path into nested wire input               |
|  [05]   | `AliasChoices`   | alias descriptor | ordered list of acceptable alias forms                           |
|  [06]   | `AliasGenerator` | alias generator  | distinct callables for validation vs serialization aliases       |
|  [07]   | `Discriminator`  | union tag        | callable/field discriminant for a tagged union                   |
|  [08]   | `Tag`            | union member tag | labels a union member resolved by a callable `Discriminator`     |

[PUBLIC_TYPE_SCOPE]: validator / serializer annotation algebra

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                            |
| :-----: | :----------------------- | :------------ | :---------------------------------------------------------------------- |
|  [01]   | `field_validator`        | decorator     | field-level validator function (`mode='before'/'after'/'wrap'/'plain'`) |
|  [02]   | `model_validator`        | decorator     | model-level validator (`mode='before'/'after'/'wrap'`)                  |
|  [03]   | `field_serializer`       | decorator     | field-level serializer override (`mode='plain'/'wrap'`)                 |
|  [04]   | `model_serializer`       | decorator     | whole-model serializer override                                         |
|  [05]   | `PlainValidator`         | annotation    | replace entire field validation with the callable                       |
|  [06]   | `BeforeValidator`        | annotation    | run before inner validation, transforms raw input                       |
|  [07]   | `AfterValidator`         | annotation    | run after inner validation, transforms parsed value                     |
|  [08]   | `WrapValidator`          | annotation    | wrap inner validation with a handler callable                           |
|  [09]   | `PlainSerializer`        | annotation    | replace field serialization with the callable                           |
|  [10]   | `WrapSerializer`         | annotation    | wrap inner serialization with a handler callable                        |
|  [11]   | `ValidationInfo`         | context arg   | validator second arg: `data`/`context`/`field_name`/`mode`/`config`     |
|  [12]   | `SerializationInfo`      | context arg   | serializer second arg: `mode`/`context`/`by_alias`/`exclude_*`          |
|  [13]   | `FieldSerializationInfo` | context arg   | field-serializer info carrying `field_name` plus serialization flags    |

[PUBLIC_TYPE_SCOPE]: schema control and validation modifiers

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]    | [CAPABILITY]                                                 |
| :-----: | :--------------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `WithJsonSchema`       | annotation       | override the JSON Schema emitted for an annotated field      |
|  [02]   | `GetPydanticSchema`    | annotation       | supply a custom core schema for a type without a model       |
|  [03]   | `GetCoreSchemaHandler` | handler protocol | `get_pydantic_core_schema` inner-schema accessor             |
|  [04]   | `GetJsonSchemaHandler` | handler protocol | `get_pydantic_json_schema` inner-schema accessor             |
|  [05]   | `InstanceOf`           | annotation       | accept any instance of T without re-validating its fields    |
|  [06]   | `SkipValidation`       | annotation       | bypass validation for the annotated field                    |
|  [07]   | `SerializeAsAny`       | annotation       | duck-type serialization using the runtime value's own schema |
|  [08]   | `Strict`               | annotation       | force strict (no-coercion) validation on the field           |
|  [09]   | `StringConstraints`    | annotation       | `strip_whitespace`/`to_lower`/`pattern`/`min_length` bundle  |
|  [10]   | `AllowInfNan`          | annotation       | permit/forbid `inf`/`nan` on a float field                   |
|  [11]   | `FailFast`             | annotation       | stop sequence validation at the first failing element        |
|  [12]   | `OnErrorOmit`          | annotation       | drop a failing element from a collection instead of erroring |

[PUBLIC_TYPE_SCOPE]: error types

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY]    | [CAPABILITY]                                              |
| :-----: | :------------------------------ | :--------------- | :-------------------------------------------------------- |
|  [01]   | `ValidationError`               | validation error | structured field error container; `.errors()` / `.json()` |
|  [02]   | `PydanticUserError`             | config error     | invalid model/usage configuration                         |
|  [03]   | `PydanticSchemaGenerationError` | schema error     | type admits no core schema                                |
|  [04]   | `PydanticInvalidForJsonSchema`  | schema error     | type has no JSON Schema representation                    |
|  [05]   | `PydanticUndefinedAnnotation`   | schema error     | a forward-ref annotation is unresolved at build time      |
|  [06]   | `PydanticImportError`           | import error     | missing optional dependency (e.g. `email-validator`)      |
|  [07]   | `PydanticForbiddenQualifier`    | usage error      | disallowed `ClassVar`/`Final` qualifier on a field        |

[PUBLIC_TYPE_SCOPE]: constrained numeric and domain scalars

| [INDEX] | [SYMBOL]                                                   | [TYPE_FAMILY]      | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------------- | :----------------- | :----------------------------------------- |
|  [01]   | `Strict{Bool,Int,Float,Str,Bytes}`                         | strict scalars     | no-coercion typed scalars                  |
|  [02]   | `{Positive,NonNegative,Negative,NonPositive}{Int,Float}`   | sign-bounded       | sign-constrained numeric aliases           |
|  [03]   | `FiniteFloat`                                              | constrained        | rejects `inf`/`nan`                        |
|  [04]   | `con{int,float,decimal,str,bytes,date,list,set,frozenset}` | constraint factory | callable ad-hoc bounded-type constructors  |
|  [05]   | `UUID1`..`UUID8` / `PaymentCardNumber` / `ByteSize`        | domain scalars     | version-checked UUID, Luhn card, byte size |

[PUBLIC_TYPE_SCOPE]: network, DSN, and IP types

| [INDEX] | [SYMBOL]                                                                                 | [TYPE_FAMILY] | [CAPABILITY]               |
| :-----: | :--------------------------------------------------------------------------------------- | :------------ | :------------------------- |
|  [01]   | `{Any,Http,AnyHttp,File,Ftp,Websocket,AnyWebsocket}Url`                                  | URL types     | parsed/validated URLs      |
|  [02]   | `{Postgres,MySQL,MariaDB,Cockroach,Mongo,Redis,Kafka,Amqp,Nats,ClickHouse,Snowflake}Dsn` | DSN types     | scheme-checked connections |
|  [03]   | `UrlConstraints`                                                                         | annotation    | scheme/host/port bounds    |
|  [04]   | `IPvAny{Address,Interface,Network}`                                                      | IP types      | validated IPv4/IPv6 forms  |

[PUBLIC_TYPE_SCOPE]: secret, encoded, JSON, email, temporal, and path types

| [INDEX] | [SYMBOL]                                                                           | [TYPE_FAMILY] | [CAPABILITY]                       |
| :-----: | :--------------------------------------------------------------------------------- | :------------ | :--------------------------------- |
|  [01]   | `Secret` / `Secret{Str,Bytes}`                                                     | secret types  | `.get_secret_value()`, repr-hidden |
|  [02]   | `Json` / `JsonValue`                                                               | JSON types    | parse-on-validate / recursive JSON |
|  [03]   | `Base64{Bytes,Str,UrlBytes,UrlStr,Encoder}`/`Encoded{Bytes,Str}`/`EncoderProtocol` | encoded       | encode/decode on validate          |
|  [04]   | `EmailStr` / `NameEmail`                                                           | email types   | RFC email; needs `email-validator` |
|  [05]   | `{Aware,Naive,Past,Future}Datetime` / `{Past,Future}Date`                          | temporal      | tz-aware/temporal-bound datetimes  |
|  [06]   | `{File,Directory,New,Socket}Path` / `ImportString`                                 | path/import   | existence path; dotted-path import |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: model operations on `BaseModel`
- `model_validate*` carry: `strict, from_attributes, context, by_alias, by_name`; `model_dump*` carry: `mode, include, exclude, by_alias, exclude_*, round_trip, context, serialize_as_any, fallback, warnings`
- `model_json_schema` carries: `by_alias, ref_template, schema_generator, mode`; `model_rebuild` carries: `force, raise_errors, _types_namespace`

| [INDEX] | [SURFACE]                                                | [SHAPE]  | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------- | :------- | :---------------------------------------------------- |
|  [01]   | `BaseModel(**data)`                                      | ctor     | validate and construct model                          |
|  [02]   | `model_validate(obj, *, ...)`                            | static   | validate dict or attribute object                     |
|  [03]   | `model_validate_json(json_data, *, ...)`                 | static   | validate JSON bytes/str through pydantic-core         |
|  [04]   | `model_validate_strings(obj, *, ...)`                    | static   | validate all-string mapping (env/query coercion)      |
|  [05]   | `model_dump(*, mode, ...)`                               | instance | serialize to dict                                     |
|  [06]   | `model_dump_json(*, indent, ...)`                        | instance | serialize to JSON string (core serializer)            |
|  [07]   | `model_copy(*, update, deep)`                            | instance | copy with field overrides                             |
|  [08]   | `model_construct(_fields_set, **values)`                 | static   | construct WITHOUT validation (trusted data fast path) |
|  [09]   | `model_json_schema(*, ...)`                              | static   | JSON Schema dict for the model                        |
|  [10]   | `model_rebuild(*, ...)`                                  | static   | resolve deferred forward refs after definition        |
|  [11]   | `model_post_init(context, /)`                            | hook     | post-construction hook (overridable)                  |
|  [12]   | `model_{fields,computed_fields,fields_set,extra,config}` | property | field/computed/set-fields/extra/config maps           |

[ENTRYPOINT_SCOPE]: TypeAdapter, validate_call, create_model
- `TypeAdapter.validate_*` carry: `strict, from_attributes, context, experimental_allow_partial, by_alias, by_name`; `dump_*` carry: `mode, include, exclude, by_alias, round_trip, context, serialize_as_any`; `json_schema*` carry: `by_alias, ref_template, schema_generator, mode`; `create_model` closes on `validators, **field_definitions`

| [INDEX] | [SURFACE]                                            | [SHAPE]   | [CAPABILITY]                              |
| :-----: | :--------------------------------------------------- | :-------- | :---------------------------------------- |
|  [01]   | `TypeAdapter(type, *, config, _parent_depth)`        | ctor      | adapter for any annotated type            |
|  [02]   | `validate_python(obj, /, *, ...)`                    | instance  | validate Python object                    |
|  [03]   | `validate_json(data, /, *, ...)`                     | instance  | validate JSON bytes/str                   |
|  [04]   | `validate_strings(obj, /, *, ...)`                   | instance  | validate all-string mapping               |
|  [05]   | `dump_python(instance, /, *, ...)`                   | instance  | serialize to Python                       |
|  [06]   | `dump_json(instance, /, *, ...)`                     | instance  | serialize to JSON bytes                   |
|  [07]   | `json_schema(*, ...)`                                | instance  | JSON Schema for the type                  |
|  [08]   | `json_schemas(inputs, *, ...)`                       | static    | combined `$defs` schema for many adapters |
|  [09]   | `validate_call(func, /, *, config, validate_return)` | decorator | validate function arguments (and return)  |
|  [10]   | `create_model(model_name, /, *, config, base, ...)`  | factory   | programmatic `BaseModel` subclass         |

[ENTRYPOINT_SCOPE]: decorator hooks and active rails
- serializers and `computed_field` carry: `return_type, when_used`; `computed_field` adds `alias, title, repr`; `field_validator` adds `check_fields`; `generate_arguments_schema` carries `schema_type, parameters_callback`
- rows [06]-[08] live under `pydantic.experimental.{pipeline,missing_sentinel,arguments_schema}`

| [INDEX] | [SURFACE]                                                  | [SHAPE]   | [CAPABILITY]                                    |
| :-----: | :--------------------------------------------------------- | :-------- | :---------------------------------------------- |
|  [01]   | `field_validator(field, /, *fields, mode, check_fields)`   | decorator | attach field validator to model                 |
|  [02]   | `model_validator(*, mode)`                                 | decorator | attach model-level validator                    |
|  [03]   | `field_serializer(field, /, *fields, mode, ...)`           | decorator | attach field serializer to model                |
|  [04]   | `model_serializer(f, /, *, mode, ...)`                     | decorator | attach whole-model serializer                   |
|  [05]   | `computed_field(func, /, *, ...)`                          | decorator | declare computed property field                 |
|  [06]   | `pipeline.validate_as(tp)`                                 | builder   | fluent `Annotated` transform/constrain pipeline |
|  [07]   | `missing_sentinel.MISSING`                                 | sentinel  | absent-vs-`None` sentinel, omitted from dump    |
|  [08]   | `arguments_schema.generate_arguments_schema(func, *, ...)` | builder   | core schema for a callable's argument signature |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- A `BaseModel` subclass compiles its core schema once at class creation into the C-extension `pydantic-core` (`SchemaValidator`/`SchemaSerializer`); every `model_validate*`/`model_dump*` dispatches into that compiled validator, never a Python-level field loop.
- Four modes — `Before`/`After`/`Wrap`/`Plain` — form the validator/serializer algebra, exposed as `Annotated[T, BeforeValidator(...)]` annotations (reusable across fields and `TypeAdapter`s) and as `@field_validator`/`@model_validator(mode=...)` decorators (model-scoped); both compile to one core schema, the annotation form carrying type-level reuse and the decorator form cross-field invariants.
- `Annotated[A | B, Discriminator(callable)]` with members `Annotated[A, Tag('a')]` selects the union member by the callable's return, generalizing the field-name `Field(discriminator='kind')` form; the discriminant resolves inside `pydantic-core`, never a Python `match`.
- `ConfigDict` keys: `strict`, `frozen`, `populate_by_name`/`validate_by_name`/`validate_by_alias`, `use_enum_values`, `arbitrary_types_allowed`, `extra` (`ignore`/`forbid`/`allow`), `alias_generator`, `validate_default`, `validate_assignment`, `revalidate_instances`, `ser_json_bytes`/`ser_json_timedelta`, `json_schema_extra`, `json_schema_mode_override`, `coerce_numbers_to_str`, `defer_build`, `model_title_generator`.
- `TypeAdapter` builds and caches a core schema for any annotation (`list[T]`, `dict[K, V]`, `Annotated[T, ...]`, unions, `TypedDict`), the standalone validator for types not warranting a model class; `validate_python(experimental_allow_partial='trailing-strings')` decodes truncated/streaming JSON.
- `ValidationError.errors(include_url=, include_context=, include_input=)` yields dicts with `type`/`loc`/`msg`/`input`/`ctx`/`url` and `.json()` emits them as a string; the `loc` tuple is the field path mapping a wire failure to a domain field.
- `model_construct` skips validation and trusts the caller — the fast path re-hydrating already-validated data; `model_copy(update=...)` does NOT re-validate the updated fields.

[STACKING]:
- `msgspec`(`.api/msgspec.md`): `msgspec` owns the zero-copy wire decode, `pydantic` owns cross-field validation — a `Struct` tagged union decodes the frame, `to_builtins(struct)` feeds `TypeAdapter.validate_python` where a `Discriminator`/`Tag` union re-expresses the discriminant, and a `Decoder(dec_hook=...)` scalar is the same scalar annotated `Annotated[T, BeforeValidator(...)]`; one discriminant vocabulary and one scalar codec span both validators.
- `beartype`(`.api/beartype.md`): `@beartype` guards call-site arg/return shape in O(1) at internal boundaries, `pydantic` owns coercion-and-constraint validation at the data edge; `validate_call(validate_return=True)` is the pydantic mirror where arguments need coercion (URL/DSN/secret parsing) rather than nominal checking — one function takes one guard.
- `structlog`(`.api/structlog.md`): catch `ValidationError` at intake and feed `exc.errors(include_url=False)` into the event dict as a structured `errors` key (a `{type, loc, msg}` list), never `str(exc)`; under `JSONRenderer` the per-field failure path stays machine-queryable and `loc` is the field-path key correlated with the bound request context.
- `opentelemetry-api`(`.api/opentelemetry-api.md`): `model_dump(mode='json', exclude_none=True)` yields the flat JSON-primitive map `Span.set_attributes` accepts, and a `computed_field` projects a derived scalar (status, size class) into a span attribute with no flattener; a caught `ValidationError` maps to `span.set_status(StatusCode.ERROR)` and `span.record_exception` carrying the `errors()` payload.
- `anyio`(`.api/anyio.md`): `TypeAdapter` instances are immutable and thread-safe once built — construct at module import and reuse across worker tasks; validation is CPU-bound in `pydantic-core`, so offload large-batch `validate_json` through `anyio.to_thread.run_sync` rather than awaiting inline.

[LOCAL_ADMISSION]:
- Domain models are `BaseModel` subclasses carrying an explicit `model_config = ConfigDict(...)`; `TypeAdapter` covers collection/union/`TypedDict` shapes not warranting a class, built once at module scope.
- `Annotated[T, BeforeValidator/AfterValidator/...]` carries reusable cross-model validation/serialization; the `@field_validator`/`@model_validator` decorators carry invariants spanning multiple fields of one model.
- Tagged wire variants model as `Annotated[A | B, Discriminator(...)]` + `Tag(...)` or `Field(discriminator='kind')` for a literal field tag; a discriminant resolves at construction, never a post-construction `match`.
- Boundary intake catches `ValidationError` and maps `.errors()` to domain error rails before domain logic; `model_construct` re-hydrates only data validated upstream.
- Wire-to-domain field renaming routes through `AliasPath`/`AliasChoices`/`AliasGenerator`; internal code holds canonical field names and emits `by_alias=True` at egress alone.
- `model_dump(mode='json')` yields JSON-safe primitives without per-field serializers; `SerializeAsAny` serializes a base-typed field via the runtime subclass schema.

[RAIL_LAW]:
- Package: `pydantic`
- Owns: model/field validation, the validator/serializer mode algebra, discriminated-union resolution, type adapters, constrained/network/secret types, JSON Schema generation, function-argument validation
- Accept: `BaseModel`, `RootModel`, `TypeAdapter`, `Field`, `Discriminator`/`Tag`, `model_validator`/`field_validator`, `Annotated[..., BeforeValidator/AfterValidator/WrapValidator/PlainValidator]`, `validate_call`
- Reject: hand-rolled dict-key validation, `isinstance` cascades replacing `TypeAdapter.validate_python`, a post-construction `match` over a discriminant field, a parallel DTO type for one wire shape, `str(ValidationError)` where `.errors()` carries the structured path

# [TYPES]

Types reach pydantic through a core schema, built from an annotation, from `Annotated` hooks, or from the type's own `__get_pydantic_core_schema__`.

## [01]-[PYDANTIC_TYPES]

Values with a format pydantic states take its type, and a type it lacks comes from one pydantic-extra-types module (`coordinate`, `timezone_name`, `mime_types`, `phone_numbers`, `currency_code`), a module over a backend (`pycountry`, `phonenumbers`) raising `RuntimeError` naming it when absent:
- `HttpUrl` normalizes with a trailing `/`, `ByteSize` reads `"1 KiB"` as `1024`, and `Json[T]` parses text before it validates `T`
- `InstanceOf[T]` checks by `isinstance` and publishes the schema of `T`, `ImportString` imports a dotted path
- Constraint aliases (`PositiveInt`, `FiniteFloat`, `FutureDate`, `FilePath`, `UUID7`) compose like any `Annotated` alias
- `EmailStr` and `NameEmail` read through `email-validator`, which the `pydantic[email]` extra supplies
- `Coordinate` reads `"41.4,-81.9"` or a pair and dumps `{"latitude", "longitude"}`
- `MimeType` accepts types the `mimetypes` registry knows alone, `image/x-exr` fails with `mime_type`
- `PhoneNumberValidator(default_region=, number_format="E164")` in `Annotated[str, ...]` validates and formats a number
- Extra types report their own error types (`TimeZoneName`, `InvalidCurrency`, `mime_type`)

## [02]-[CUSTOM_TYPES]

`Annotated` hooks serve a type the code does not own: `AfterValidator`, `PlainSerializer`, `WithJsonSchema`, `ValidateAs(FromType, build)`, and `GetPydanticSchema(lambda source, handler: ...)` for a one-off core schema. Records that parse from text and write back as text own their schema through `__get_pydantic_core_schema__(cls, source, handler)`. `json_or_python_schema` accepts text from JSON and an instance or text from Python, a pattern on its `str_schema` refuses text before `parsed` reads it, and `serialization=` writes each record back as text. `Version` orders by its parts:

```python
class Version(msgspec.Struct, frozen=True, order=True):
    parts: tuple[int, ...]

    @override
    def __str__(self) -> str:
        return ".".join(map(str, self.parts))

    @classmethod
    def parsed(cls, text: str, /) -> Self:
        return cls(tuple(map(int, text.split("."))))

    @classmethod
    def __get_pydantic_core_schema__(cls, source: object, handler: GetCoreSchemaHandler) -> CoreSchema:
        text = core_schema.no_info_after_validator_function(cls.parsed, core_schema.str_schema(pattern=r"^\d+(\.\d+)*$"))
        return core_schema.json_or_python_schema(
            json_schema=text,
            python_schema=core_schema.union_schema([core_schema.is_instance_schema(cls), text], custom_error_type="version", custom_error_message="input is no version"),
            serialization=core_schema.to_string_ser_schema(),
        )
```

JSON input `"1.x"` fails as `string_pattern_mismatch`, and the validation schema publishes its pattern with no `__get_pydantic_json_schema__`:
- Python-side unions report one error per member, `custom_error_type` on `union_schema` collapses them into one `version` error
- `to_string_ser_schema()` writes `str(value)` in JSON mode and keeps the instance in Python mode
- `plain_serializer_function_ser_schema(fn, return_schema=)` writes `fn(value)` and types its serialization schema
- `__get_pydantic_json_schema__` refines the node `handler(schema)` builds, a plain validator function has no JSON schema until it supplies one
- `handler(source)` or `handler.generate_schema(inner)` delegates to the schema pydantic builds, a generic custom type reads its parameter there
- `core_schema.no_info_after_validator_function`, `chain_schema`, `union_schema`, and `is_instance_schema` compose like `Annotated` modes
- `SchemaValidator(core_schema.typed_dict_schema(...))` and `SchemaSerializer` validate and dump shapes no annotation spells
- Ruff `PLW3201` reports both hooks until `lint.pylint.allow-dunder-method-names` lists them

## [03]-[STRUCT_FIELDS]

Struct records validate inside pydantic models through one `__get_pydantic_core_schema__` on their base:

```python
class Record(msgspec.Struct, frozen=True, kw_only=True):
    @classmethod
    def __get_pydantic_core_schema__(cls, source: object, handler: GetCoreSchemaHandler) -> CoreSchema:
        return core_schema.no_info_plain_validator_function(partial(msgspec.convert, type=cls), serialization=core_schema.plain_serializer_function_ser_schema(msgspec.to_builtins))

    @classmethod
    def __get_pydantic_json_schema__(cls, schema: CoreSchema, handler: GetJsonSchemaHandler) -> JsonSchemaValue:
        _, components = msgspec.json.schema_components((cls,))
        component: JsonSchemaValue = components[cls.__name__]
        return component
```

`msgspec.convert` validates builtins and `msgspec.to_builtins` dumps each record:
- Subclasses inherit the hook, and their `Meta` bounds and `__post_init__` run inside `msgspec.convert`
- Struct failures arrive as one `value_error` holding msgspec's path (`Expected int >= 1 - at $.id`) beside the model's own errors
- JSON schema inlines the Struct's component, a Struct field holding another Struct emits a `$ref` pydantic never registered and raises `KeyError`
- `ValidationError.from_exception_data(title, [InitErrorDetails(...)])` folds failures found outside pydantic into one pydantic error

## [04]-[CORE_VALUES]

Values pydantic-core exports serve a boundary that reads no model:
- `from_json(data, allow_partial=)` with `True` or `"trailing-strings"` parses truncated JSON into builtins
- `to_json(value, fallback=repr)` and `to_jsonable_python(value)` serialize untyped values, sets as lists and datetimes as ISO text
- `Url("https://user@host:8080/p?q=1")` exposes `host`, `port`, `path`, and `query_params()`, `Url.build(scheme=, host=, path=)` builds one

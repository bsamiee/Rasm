# [TYPES]

Types reach pydantic through a core schema, built from an annotation, from `Annotated` hooks, or from the type's own `__get_pydantic_core_schema__`.

## [01]-[CUSTOM_TYPES]

Records that parse from text and write back as text declare `__get_pydantic_core_schema__(cls, source, handler)` as a classmethod. `json_or_python_schema` accepts text from JSON and an instance or text from Python, and `serialization=` writes the record back. `Version` orders by its parts:

```python
class Version(msgspec.Struct, frozen=True, order=True):
    parts: tuple[int, ...]

    @override
    def __str__(self) -> str:
        return ".".join(map(str, self.parts))

    @classmethod
    def parsed(cls, text: str, /) -> Self:
        if not all(part.isdecimal() for part in text.split(".")):
            raise PydanticCustomError("version", "{value} is no dotted version", {"value": text})
        return cls(tuple(map(int, text.split("."))))

    @classmethod
    def __get_pydantic_core_schema__(cls, source: object, handler: GetCoreSchemaHandler) -> CoreSchema:
        text = core_schema.no_info_after_validator_function(cls.parsed, core_schema.str_schema())
        return core_schema.json_or_python_schema(
            json_schema=text,
            python_schema=core_schema.union_schema([core_schema.is_instance_schema(cls), text], custom_error_type="version", custom_error_message="input is no version"),
            serialization=core_schema.to_string_ser_schema(),
        )

    @classmethod
    def __get_pydantic_json_schema__(cls, schema: CoreSchema, handler: GetJsonSchemaHandler) -> JsonSchemaValue:
        return handler(schema) | {"pattern": r"^\d+(\.\d+)*$"}
```

Python input reaches `parsed` through a union, and JSON input reaches it alone:
- Python-side unions report one error per member, `custom_error_type` on `union_schema` collapses them into one line
- JSON input `"1.x"` reports the `PydanticCustomError` message from `parsed`
- `to_string_ser_schema()` writes `str(value)` in JSON mode and keeps the instance in Python mode
- `plain_serializer_function_ser_schema(fn, return_schema=)` writes `fn(value)` and types its serialization schema
- `__get_pydantic_json_schema__` refines the node `handler(schema)` builds, a plain validator function has no JSON schema until it supplies one
- `handler(source)` or `handler.generate_schema(inner)` delegates to the schema pydantic builds, a generic custom type reads its parameter there
- `core_schema.no_info_after_validator_function`, `chain_schema`, `union_schema`, and `is_instance_schema` compose like `Annotated` modes
- `SchemaValidator(core_schema.typed_dict_schema(...))` and `SchemaSerializer` validate and dump shapes no annotation spells

## [02]-[RECORD_BRIDGE]

Struct records join pydantic models through one `__get_pydantic_core_schema__` on their base, which validates through `msgspec.convert` and dumps through `msgspec.to_builtins`:

```python
class Record(msgspec.Struct, frozen=True, kw_only=True):
    @classmethod
    def validated(cls, value: object, /) -> Self:
        return value if isinstance(value, cls) else msgspec.convert(value, cls)

    @classmethod
    def __get_pydantic_core_schema__(cls, source: object, handler: GetCoreSchemaHandler) -> CoreSchema:
        return core_schema.no_info_plain_validator_function(cls.validated, serialization=core_schema.plain_serializer_function_ser_schema(msgspec.to_builtins))

    @classmethod
    def __get_pydantic_json_schema__(cls, schema: CoreSchema, handler: GetJsonSchemaHandler) -> JsonSchemaValue:
        _, components = msgspec.json.schema_components((cls,))
        component: JsonSchemaValue = components[cls.__name__]
        return component
```

- Every subclass validates inside a model, its `Meta` bounds and `__post_init__` running through `msgspec.convert`
- Struct failures arrive as one `value_error` line holding msgspec's path (`Expected int >= 1 - at $.id`) beside the model's own errors
- JSON schema inlines the Struct's component, a Struct field holding another Struct emits a `$ref` pydantic never registered and raises `KeyError`
- `ValidationError.from_exception_data(title, [InitErrorDetails(...)])` folds failures found outside pydantic into one pydantic error

## [03]-[CORE_VALUES]

Values pydantic-core exports serve a boundary that reads no model:
- `from_json(data, allow_partial=True)` parses truncated JSON, `"trailing-strings"` keeps a cut final string
- `to_json(value, fallback=repr)` and `to_jsonable_python(value)` serialize untyped values, sets as lists and datetimes as ISO text
- `Url("https://user@host:8080/p?q=1")` exposes `host`, `port`, `path`, and `query_params()`, `Url.build(scheme=, host=, path=)` builds one
- `MISSING` is the one sentinel, and `pydantic` re-exports it

## [04]-[EXTRA_TYPES]

pydantic-extra-types holds one module per type, each importing its backend from a named extra and raising `RuntimeError` naming the backend when absent:

| [INDEX] | [MODULE]                                    | [TYPES]                                       | [EXTRA]        |
| :-----: | :------------------------------------------ | :-------------------------------------------- | :------------- |
|  [01]   | `coordinate`                                | `Latitude`, `Longitude`, `Coordinate`         | None           |
|  [02]   | `timezone_name`                             | `TimeZoneName`                                | None           |
|  [03]   | `color`                                     | `Color`                                       | None           |
|  [04]   | `mime_types`                                | `MimeType`                                    | None           |
|  [05]   | `country`, `currency_code`, `language_code` | `CountryAlpha2`, `Currency`, `LanguageAlpha2` | `pycountry`    |
|  [06]   | `phone_numbers`                             | `PhoneNumber`, `PhoneNumberValidator`         | `phonenumbers` |
|  [07]   | `semantic_version`                          | `SemanticVersion`                             | `semver`       |
|  [08]   | `ulid`                                      | `ULID`                                        | `python-ulid`  |
|  [09]   | `pendulum_dt`                               | `DateTime`, `Date`, `Duration`                | `pendulum`     |

- `Coordinate` reads `"41.4,-81.9"` or a pair and dumps `{"latitude", "longitude"}`, `Latitude` and `Longitude` validate to `float`
- Out-of-range `Latitude(91)` reports one error per member of its `float | Decimal` union
- `Color` parses CSS forms (`hsl(...)`, names, hex), and `as_hex()` writes hex
- `MimeType` accepts types the `mimetypes` registry knows alone, `image/x-exr` fails with `mime_type`
- `PhoneNumberValidator(default_region=, number_format="E164")` in `Annotated[str, ...]` validates and formats a number

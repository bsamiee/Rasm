---
name: python-coding-pydantic
description: "Use when writing or reviewing pydantic models, adapters, validators, serializers, or settings, covering validation and schemas at untrusted boundaries"
---

# [PYTHON_CODING_PYDANTIC]

Covers pydantic with pydantic-core, annotated-types, pydantic-settings, and pydantic-extra-types, from untrusted input to published schema. Examples use the `Fault`, `Faults`, `Outcome`, `Reason`, and `Quota` that `python-coding` declares.

[REFERENCES]:
- [01]-[SETTINGS](references/settings.md): Settings sources, precedence, environment and dotenv parsing, secrets, lifetime, and custom sources
- [02]-[TYPES](references/types.md): Custom types through core schema, the record bridge, pydantic-core values, and extra types

## [01]-[BOUNDARIES]

Wire models validate untrusted input once, a `ValidationError` becomes faults at that call, and the domain record reads the validated model. `fault` derives each fault from one error line, and `faulted` keeps every line of one input:

```python
class Wire(BaseModel, frozen=True, extra="forbid"):
    name: Annotated[str, StringConstraints(strip_whitespace=True, min_length=1)]
    limit: PositiveInt | None = None


def fault(title: str, line: ErrorDetails, /) -> Fault:
    match line["type"], line["loc"]:
        case "missing", (*parent, field):
            return Fault(reason=Reason.ABSENT, subject=".".join(map(str, parent)) or title, member=str(field))
        case _, loc:
            return Fault(reason=Reason.INVALID, subject=".".join(map(str, loc)) or title, cause=line["msg"])


def faulted(error: ValidationError, /) -> Faults:
    match tuple(fault(error.title, line) for line in error.errors(include_url=False)):
        case (head, *rest):
            return (head, *rest)
        case _:
            return (Fault(reason=Reason.INVALID, subject=error.title, cause=error),)


def quota(raw: bytes, /) -> Outcome[Quota]:
    return catch(Wire.model_validate_json, exception=ValidationError)(raw).map_error(faulted).map(lambda wire: msgspec.convert(wire, Quota, from_attributes=True))
```

`b'{"limit": 0, "x": 1}'` returns a fault per line (`x is invalid. Extra inputs are not permitted`, `Wire holds no name`, `limit is invalid. Input should be greater than 0`), and malformed JSON returns one `json_invalid` fault with an empty `loc`, which `title` names:
- `model_validate_json` parses and validates bytes or text in one pass, `model_validate` reads builtins or (`from_attributes=True`) objects
- Error `type` strings (`missing`, `extra_forbidden`, `greater_than`, a custom error's type) are stable keys a fault builder matches, `msg` is prose
- `msgspec.convert(model, Record, from_attributes=True)` reads a validated model into its record, the wire model holding every bound
- `ValidationError` and `SettingsError` subclass `ValueError`, a validator raising `TypeError` or `KeyError` escapes validation as a defect
- Wire fields hold `T | None` for a nullable value, an `Option[T]` or `Result[T, E]` field validates as an unchecked dataclass and fails at dump
- Constraints stay with their library: msgspec ignores `annotated_types` and `Field` constraints, pydantic ignores `msgspec.Meta`
- Model classes fail mypy `disallow_any_explicit` as explicit `Any` until `pydantic.mypy` joins `plugins`

## [02]-[MODELS]

Hosts differ by what validation returns:

| [INDEX] | [HOST]           | [USE]                                                                    |
| :-----: | :--------------- | :----------------------------------------------------------------------- |
|  [01]   | `BaseModel`      | Record with validators, computed fields, and a schema title              |
|  [02]   | `TypedDict`      | Plain `dict` output, `closed=True` rejects unknown keys, `@with_config`  |
|  [03]   | `TypeAdapter(T)` | Unions, aliases, collections, and every type that has no model           |
|  [04]   | `RootModel[T]`   | Model API over one non-object value, a second field fails at class build |

Configuration takes class keywords (`class Wire(BaseModel, frozen=True, extra="forbid")`), which mypy and ty read and pydantic merges into `model_config`:
- `frozen=True` rejects assignment with `frozen_instance` and hashes field values, a frozen model holding a `list` raises `TypeError` on `hash`
- `extra="forbid"` reports an unknown key as `extra_forbidden`, the default `"ignore"` drops it, and `extra=` on one validate call overrides it
- `alias`, `default`, and `default_factory` take assignment form (`key: int = Field(alias="id")`), mypy reads an `Annotated` alias as the field name
- Field metadata (`alias`, `exclude`, `deprecated`) sits on the whole annotation, inside a union arm it warns `UnsupportedFieldAttributeWarning`
- `default_factory=lambda data: ...` receives earlier validated fields as a `dict`
- `T | MISSING = MISSING` marks a key input can omit, dumps leave it out, and the schema drops it from `required`
- `model_construct`, `model_copy(update=)`, and `copy.replace` skip validation, `Model.model_validate(model.model_dump() | changes)` validates
- `validate_assignment=True` keeps a value its model validator refused on the instance, frozen models change by validated copy
- `revalidate_instances="always"` holds on an instance's own class, set on a holding model it leaves a nested `model_construct` value unchecked
- `@computed_field` over `@property` enters dumps and serialization schema as `readOnly`, and mypy reports its line as `prop-decorator`
- `functools.cached_property` works on a frozen model and stays out of dumps
- Field names of a `model_*` member raise `ValueError` at class build, other `BaseModel` names (`copy`, `schema`) warn

### [02.1]-[CONSTRAINTS]

Constraints are type metadata, `Field`, `annotated_types`, and `StringConstraints` compile into one schema node, and a validator serves a rule none of them states. Patterns over transformed text run in an `AfterValidator`, `StringConstraints` checks `pattern` before its case change:

```python
CODE: re.Pattern[str] = re.compile(r"[A-Z]{3}")


def coded(text: str) -> str:
    if CODE.fullmatch(text) is None:
        raise PydanticCustomError("code_shape", "{code} is not three letters", {"code": text})
    return text


type Code = Annotated[str, StringConstraints(strip_whitespace=True, to_upper=True), AfterValidator(coded)]
```

- `type` aliases over `Annotated` publish a named `$defs` entry, an assignment alias inlines its node
- `Predicate` rejects with `predicate_failed` and stays out of the JSON schema
- Predicate aliases (`IsDigits`, `LowerCase`, `IsFinite`) take their argument, `at.IsDigits[str]` enforces where `at.IsDigits` accepts `"12a"`
- `AwareDatetime` and `NaiveDatetime` require timezone state, pydantic ignores `at.Timezone` and `at.doc`
- Metadata order decides input: `Annotated[int, AfterValidator(f), Field(gt=5)]` checks output of `f`, the reverse order checks raw input
- `pattern` compiles with the Rust `regex` crate, a lookaround raises `SchemaError` at class build until `regex_engine="python-re"`

### [02.2]-[STRICTNESS]

Lax mode coerces by a fixed table, strict mode accepts exact types, and JSON input holds its own strict table:
- Lax `int` accepts `"1"` and `1.0` and rejects `1.5` with `int_from_float`, lax `list[int]` accepts a `tuple` or `set`
- `strict=True` as a class keyword makes every field strict, `Annotated[T, Strict(False)]` relaxes one field
- Strict Python input rejects strings for `date`, lists for `tuple`, and raw values for an enum, strict JSON accepts each one's JSON encoding
- `Sequence[str]` rejects a `str` with `sequence_str` and keeps the input's collection type, `tuple[T, ...]` yields one type
- Enum fields hold members, `use_enum_values=True` stores raw values in their place

## [03]-[VALIDATORS]

Validators sit in `Annotated` beside their type, travel with a `type` alias, and attach by mode:

| [INDEX] | [MODE]   | [INPUT]                 | [CONTROL]                                        |
| :-----: | :------- | :---------------------- | :----------------------------------------------- |
|  [01]   | `after`  | Value of the field type | Returns the value or raises, type-safe           |
|  [02]   | `before` | Raw input of any shape  | Reshapes input before core validation            |
|  [03]   | `wrap`   | Raw input and `handler` | Calls, skips, or recovers around core validation |
|  [04]   | `plain`  | Raw input               | Replaces core validation, the type check is gone |

Validation runs model `before`, decorator field `before`, `Annotated` `before` and `wrap` right to left, core, `Annotated` `after` left to right, decorator field `after`, then model `after`. Cross-field rules are model `after` validators, which run once every field validated and never after a field failed:

```python
type Bound = Annotated[int, Field(ge=0)]


class Span(BaseModel, frozen=True, extra="forbid"):
    lo: Bound
    hi: Bound

    @model_validator(mode="after")
    def ordered(self) -> Self:
        if self.lo > self.hi:
            raise PydanticCustomError("span_order", "{lo} exceeds {hi}", {"lo": self.lo, "hi": self.hi})
        return self
```

- `@model_validator(mode="after")` is an instance method returning `Self`, a `@classmethod` form warns deprecation
- `@field_validator(*names, mode=)` stacks over `@classmethod`, a model `before` validator receives any input object, a `dict` or not
- Field validators reading `info.data["lo"]` raise a raw `KeyError` once `lo` failed
- `ValueError` and `PydanticCustomError(type, template, ctx)` become error lines, the custom `type` is the key `fault` matches
- `PydanticCustomError` templates format `{name}` from `ctx` and leave `{name!r}` literal, ruff `RUF027` reports a placeholder naming a local
- `PydanticKnownError("greater_than", {"gt": 5})` raises a built-in error type with its stock message
- `ValidationInfo` holds `data` (earlier fields that passed), `context` from `context=` on the call, `mode`, and `field_name`
- Wrap validators call `handler(value)`, raising `PydanticUseDefault` takes the field default
- `PydanticOmit` from an item validator drops that item, `OnErrorOmit[T]` drops each invalid item
- `Annotated[list[T], FailFast()]` stops at the first invalid item, the default reports every item
- Validator signatures resolve at class build, a function over `*args` raises `PydanticUserError`
- `validation_error_cause=True` sets `__cause__` to an `ExceptionGroup` of the raised Python errors
- `hide_input_in_errors=True` drops input values from error text on models that read secrets

## [04]-[SERIALIZERS]

Serializers attach in `Annotated` as `PlainSerializer(fn, return_type=, when_used=)` or `WrapSerializer(fn)`, whose `fn` receives `(value, handler, info)` and branches on `info.mode_is_json()`:

```python
type Cents = Annotated[int, PlainSerializer(lambda cents: str(Decimal(cents).scaleb(-2)), return_type=str, when_used="json")]
```

- `model_dump()` keeps `150`, JSON output writes `"1.50"`, and `return_type` types the serialization schema as a string
- `@model_serializer(mode="wrap")` calls `handler(self)`, a `TypedDict` return keeps a typed schema and `dict[str, Any]` an open object
- `Field(exclude=True)` removes a field from every dump, `Field(exclude_if=predicate)` removes it when the predicate holds for its value
- `model_dump_json()` returns `str`, `TypeAdapter.dump_json()` returns `bytes`, `model_dump(mode="json")` returns JSON builtins
- `SecretStr` dumps as itself in Python mode and as `"**********"` in JSON, `get_secret_value()` reads it at its one use
- Temporal values write ISO 8601 and round-trip, `ser_json_temporal="milliseconds"` reads back seconds and a naive `datetime` as UTC
- `ser_json_bytes="base64"` pairs with `val_json_bytes="base64"`, alone it reads base64 text back as bytes of that text

### [04.1]-[SUBCLASSES]

Fields serialize by declared type: a field typed `Base` holding a `Sub` dumps `Base` fields, and dict input validates as `Base` with `Sub` keys dropped. Subclass shapes take a form that names them:
- Tagged unions keep each case's shape in both directions
- `class Holder[T: Base](BaseModel)` parametrized as `Holder[Sub]` validates and dumps `Sub`
- `polymorphic_serialization=True` on a base's configuration dumps held instances by runtime class and leaves validation as declared
- `SerializeAsAny[Base]` widens one field, `serialize_as_any=True` on a dump call widens every value

## [05]-[UNIONS]

Tagged unions select a member by a `Literal` field, one member can claim more than one tag, and one module-level adapter reads a collection:

```python
class Opened(BaseModel, frozen=True, extra="forbid"):
    kind: Literal["opened", "resumed"]
    key: str


class Closed(BaseModel, frozen=True, extra="forbid"):
    kind: Literal["closed"]
    key: str
    reason: str


type Event = Annotated[Opened | Closed, Field(discriminator="kind")]
EVENTS: TypeAdapter[tuple[Event, ...]] = TypeAdapter(tuple[Event, ...])
```

- Unknown tags report `union_tag_invalid`, a missing tag `union_tag_not_found`, and member errors take a `loc` led by the tag (`(1, "closed", "key")`)
- Tag fields typed `str` raise `PydanticUserError` code `discriminator-needs-literal`, a duplicate tag raises `TypeError` at build
- JSON schema emits `oneOf` with `discriminator.propertyName` and a `mapping` entry per tag

Unions chosen by a function of the input take `Discriminator(fn)` with a `Tag` on each member, and `fn` receives raw input or a member instance (`Circle` holds `radius`, `Rect` holds `width` and `height`):

```python
def shape(value: object) -> str | None:
    match value:
        case {"radius": _} | Circle():
            return "circle"
        case {"width": _, "height": _} | Rect():
            return "rect"
        case _:
            return None


type Shape = Annotated[Annotated[Circle, Tag("circle")] | Annotated[Rect, Tag("rect")], Discriminator(shape, custom_error_type="shape", custom_error_message="input matches no shape")]
```

- `None` from `fn` raises the custom error type with its message
- Untagged unions run smart mode: an exact type wins, `int | str` keeps `"1"` a `str` and `date | str` keeps an ISO string
- Untagged model unions take a member that validates, input meant for one member can validate as another through lax coercion
- Untagged unions report one error per member at every level of a recursive alias
- `Field(union_mode="left_to_right")` takes the first member that validates

## [06]-[ADAPTERS]

`TypeAdapter(T)` builds a validator and serializer at construction, each adapter is one module-level value, and a construction per call repeats schema generation:
- Members: `validate_json` (`str` or `bytes`), `validate_python`, `validate_strings`, `dump_python`, `dump_json`, `json_schema`
- `validate_strings` reads a string or a mapping of strings (environment, query) and rejects a list with `string_type`
- `TypeAdapter.json_schemas([(key, mode, adapter), ...])` emits one shared `$defs`
- `config=` sets configuration on types without their own, a model or `TypedDict` target raises `PydanticUserError`
- `defer_build=True` moves schema build to first use, `pydantic_complete` reports the state
- `experimental_allow_partial=True` on adapter JSON validation completes a truncated `list`, `dict`, or `TypedDict`, a model field stays required
- Partial validation drops a truncated last element that fails, `"trailing-strings"` keeps a truncated last string

### [06.1]-[CALLS]

`@validate_call` validates arguments at each call against the signature, reports every argument error at once with a positional argument's index as `loc`, and raises `ValidationError` a host entry converts:
- `**kwargs: Unpack[TypedDict]` validates each keyword against its item
- `validate_return=True` validates the return, on an `async def` after the await
- Methods take `@validate_call` directly, under `@classmethod` it sits innermost
- PEP 695 type parameters left unbound validate as `Any`
- `config=ConfigDict(strict=True)` makes every argument strict, `raw_function` holds the undecorated function

## [07]-[TYPING]

Generic models take PEP 695 class parameters and parametrize at the boundary (`Page[Item].model_validate_json(raw)`):

```python
class Page[T](BaseModel, frozen=True, extra="forbid"):
    items: tuple[T, ...]
    cursor: str | None = None
```

- Each parametrization is one cached subclass named `Page[Item]`, which titles its schema
- Annotations resolve at class build, a class defined later in the module resolves with no `model_rebuild()`
- Unparametrized generics validate a free parameter as `Any`, a bound (`[T: int]`) or default (`[T = int]`) validates against itself
- `type Tree = int | list[Tree]` validates recursive input, a quoted `TypeAlias` recursion raises `RecursionError` at build
- Fields typed `Self` validate a recursive model
- `TypedDict(closed=True)` rejects unknown keys, `extra_items=T` validates them, `NotRequired` drops a key from `required`
- `ReadOnly` items validate with a `UserWarning` that pydantic leaves mutation open
- `frozendict[K, V]` validates to a `frozendict`, `NewType` validates as its base
- `LiteralString`, `TypeIs`, and sentinels other than `MISSING` raise `PydanticSchemaGenerationError` at build
- `__init_subclass__` runs before pydantic collects a class's own fields, `__pydantic_init_subclass__(cls, **kwargs)` runs with them built
- `__pydantic_on_complete__` runs once a class's schema completes, and both hooks suit a registry reading fields

## [08]-[SCHEMAS]

Published schemas come from `model_json_schema(mode=)` and `TypeAdapter.json_schema(mode=)` in Draft 2020-12, `"validation"` describing accepted input and `"serialization"` output:
- Serialization mode holds computed fields and serializer return types
- `json_schema_serialization_defaults_required=True` marks defaulted fields required in serialization mode
- `models_json_schema([(Model, mode), ...], ref_template="#/components/schemas/{model}")` shares one `$defs` across models
- Models and named `type` aliases land in `$defs`, a one-value `Literal` emits `const`
- `Field(description=, examples=, json_schema_extra=)`, configuration `json_schema_extra`, and `use_attribute_docstrings=True` add keys
- Validators, `Predicate`, and model serializer logic stay out of the schema, `WithJsonSchema` and `__get_pydantic_json_schema__` replace a node
- `alias_generator=to_camel` with `validate_by_name=True`, `validate_by_alias=True`, and `serialize_by_alias=True` maps both directions
- Field titles derive from the alias (`itemCount` titles `Itemcount`)
- `AliasChoices` and `AliasPath("names", 0)` select validation input, `serialization_alias` names output under `by_alias=True`

## [09]-[SETTINGS]

Processes read settings once at the composition root through one `BaseSettings` root composing nested `BaseModel` groups. `settings` returns it as an outcome, and `refused` reuses `faulted` for validation errors and keeps an undecodable source value as cause:

```python
class Store(BaseModel, frozen=True, extra="forbid"):
    host: str
    port: Annotated[int, Field(gt=0, lt=65536)]


class Settings(BaseSettings, frozen=True, env_prefix="APP_", env_nested_delimiter="__", toml_file="settings.toml"):
    token: SecretStr
    store: Store

    @override
    @classmethod
    def settings_customise_sources(
        cls,
        settings_cls: type[BaseSettings],
        init_settings: PydanticBaseSettingsSource,
        env_settings: PydanticBaseSettingsSource,
        dotenv_settings: PydanticBaseSettingsSource,
        file_secret_settings: PydanticBaseSettingsSource,
    ) -> tuple[PydanticBaseSettingsSource, ...]:
        return (init_settings, env_settings, dotenv_settings, file_secret_settings, TomlConfigSettingsSource(settings_cls))


def refused(error: ValueError, /) -> Faults:
    match error:
        case ValidationError():
            return faulted(error)
        case _:
            return (Fault(reason=Reason.INVALID, subject=Settings.__name__, cause=error),)


def settings() -> Outcome[Settings]:
    return catch(Settings, exception=ValueError)().map_error(refused)
```

- `settings_customise_sources` returns sources highest priority first, a source left out of the tuple reads nothing
- File sources (TOML, pyproject, JSON, YAML) enter through the hook alone, a file key without its source warns and goes unread
- Construction raises `ValidationError` for invalid values and `SettingsError` for an undecodable source value
- `BaseSettings` defaults to `extra="forbid"` and `validate_default=True`, unknown file and dotenv keys fail as `extra_forbidden`
- Without the mypy plugin, `frozen=True` fails as a frozen dataclass over a non-frozen one and `Settings()` fails `call-arg`

## [10]-[TYPES]

Values with a format pydantic states take its type, a type it lacks comes from pydantic-extra-types, and a domain type owns its schema through `__get_pydantic_core_schema__`:
- `SecretStr`, `HttpUrl` (normalized with a trailing `/`), `ByteSize` (`"1 KiB"` to `1024`), `Json[T]`, `InstanceOf[T]`, and `ImportString`
- `PositiveInt`, `NonNegativeFloat`, `FiniteFloat`, `FutureDate`, `FilePath`, and `UUID7` are constraint aliases
- `EmailStr` and `NameEmail` read through `email-validator`, which the `pydantic[email]` extra supplies
- `Annotated` hooks (`AfterValidator`, `PlainSerializer`, `WithJsonSchema`, `ValidateAs(FromType, build)`) serve a type the code does not own
- `GetPydanticSchema(lambda source, handler: ...)` builds a one-off core schema inside `Annotated`
- Extra types report their own error types (`TimeZoneName`, `InvalidCurrency`, `mime_type`), which `fault` reads as `INVALID`

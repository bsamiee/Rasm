---
name: python-coding-pydantic
description: "Use when writing or reviewing pydantic models, adapters, validators, serializers, or settings, covering validation and schemas at untrusted boundaries"
---

# [PYTHON_CODING_PYDANTIC]

Covers pydantic with pydantic-core, annotated-types, and pydantic-settings, from untrusted input to published schema.

[REFERENCES]:
- [01]-[SETTINGS](references/settings.md): Settings sources, precedence, environment and dotenv parsing, secrets, lifetime, and custom sources
- [02]-[TYPES](references/types.md): Pydantic types, custom types through core schema, Struct fields, and pydantic-core values

## [01]-[BOUNDARIES]

Frozen models validate untrusted input once and are records of the data they hold. One `faulted` serves every pydantic boundary: an error's `type` selects its reason, `loc` or `error.title` names its subject, and a `SettingsError` becomes one fault's cause:

```python
class Window(BaseModel, frozen=True, extra="forbid"):
    name: Annotated[str, StringConstraints(strip_whitespace=True, min_length=1)]
    limit: PositiveInt | None = None


def fault(title: str, detail: ErrorDetails, /) -> Fault:
    match detail["type"], detail["loc"]:
        case "missing", (*parent, field):
            return Fault(reason=Reason.ABSENT, subject=".".join(map(str, parent)) or title, member=str(field))
        case _, loc:
            return Fault(reason=Reason.INVALID, subject=".".join(map(str, loc)) or title, cause=detail["msg"])


def faulted(error: ValueError, /) -> Faults:
    match error:
        case ValidationError() if details := error.errors(include_url=False):
            head, *rest = (fault(error.title, detail) for detail in details)
            return (head, *rest)
        case _:
            return Fault.of(Reason.INVALID, type(error).__name__, error)
```

`catch(Window.model_validate_json, exception=ValueError)(raw).map_error(faulted)` over `b'{"limit": 0, "x": 1}'` returns a fault per error (`x is invalid. Extra inputs are not permitted`, `Window holds no name`, `limit is invalid. Input should be greater than 0`), and malformed JSON returns one `json_invalid` fault on `Window`:
- `ValidationError` and `SettingsError` subclass `ValueError`, a validator raising `TypeError` or `KeyError` escapes validation as a defect
- `title` names a model by its class and an adapter by its type (`list[int]`)
- `loc` names a field by its validation alias (`("id",)` for `key: int = Field(alias="id")`), `loc_by_alias=False` names it by attribute
- Error `type` strings (`missing`, `extra_forbidden`, `greater_than`, a custom error's type) are stable keys a fault builder matches, `msg` is prose
- `ctx` holds each refused constraint (`{"gt": 0}`) and `input` the refused value
- `TypeAdapter(Annotated[Record, ValidateAs(Wire, build)])` validates a wire model differing from an existing record and builds the record in one call
- Record bounds the wire model lacks fail as one `value_error` at `loc` `()`, and records encode through their own codec
- Wire fields hold `T | None` for a nullable value, pydantic validates an `Option[T]` field as a dataclass and refuses every mapping input
- Constraints stay with their library: msgspec ignores `annotated_types` and `Field` constraints, pydantic ignores `msgspec.Meta`

## [02]-[MODELS]

Class keywords (`class Wire(BaseModel, frozen=True, extra="forbid")`) set configuration that mypy and ty read and pydantic merges into `model_config`. Subclasses inherit configuration, a family's base states its policy and each case its fields alone:
- `frozen=True` hashes field values, a frozen model holding a `list` raises `TypeError` on `hash`
- `extra="forbid"` reports an unknown key as `extra_forbidden`, the default `"ignore"` drops it, and `extra=` on one validate call overrides it
- `alias`, `default`, and `default_factory` take assignment form (`key: int = Field(alias="id")`), mypy reads an `Annotated` alias as the field name
- Field metadata (`alias`, `exclude`, `deprecated`) sits on the whole annotation, inside a union arm it warns `UnsupportedFieldAttributeWarning`
- `default_factory=lambda data: ...` receives earlier validated fields as a `dict`
- `T | MISSING = MISSING` with `MISSING` from `pydantic` marks a key input can omit and dumps leave out
- `model_construct`, `model_copy(update=)`, and `copy.replace` skip validation, `Model.model_validate(model.model_dump() | changes)` validates
- Dumps of a value off its field type warn `UserWarning`, and `warnings="error"` on the dump call raises `PydanticSerializationError`
- `validate_assignment=True` keeps a value its model validator refused on the instance, frozen models change by validated copy
- `revalidate_instances="always"` holds on an instance's own class, set on a holding model it leaves a nested `model_construct` value unchecked
- `@computed_field` over `@property` enters dumps and serialization schema as `readOnly`, and mypy reports its line as `prop-decorator`
- `functools.cached_property` works on a frozen model and stays out of dumps
- Field names equal to a `BaseModel` method (`model_dump`) raise `ValueError` at class build, names shadowing an attribute (`copy`) warn

### [02.1]-[CONSTRAINTS]

Constraints are type metadata: `Field`, `annotated_types`, and `StringConstraints` compile into one schema node, a validator serves a rule none of them states, and a generic alias (`NonEmpty[T]`) holds one constraint over any element type:

```python
type Unit = Annotated[float, Field(ge=0.0, le=1.0)]
type Positive = Annotated[float, Field(gt=0.0)]
type NonEmpty[T] = Annotated[tuple[T, ...], Field(min_length=1)]
type Code = Annotated[str, AfterValidator(str.upper), StringConstraints(strip_whitespace=True, pattern=r"^[A-Z]{3}$")]


class Knot[X: float, Y: float](BaseModel, frozen=True, extra="forbid"):
    x: X
    y: Y


class Curve(BaseModel, frozen=True, extra="forbid"):
    code: Code
    knots: NonEmpty[Knot[Unit, Positive]]
```

`{"code": "abcd", "knots": [{"x": 1.5, "y": 0}]}` fails at `code`, `("knots", 0, "x")`, `("knots", 0, "y")`, and `knots`:
- Length bounds count items that validated
- Metadata after a validator checks its output, `Code` uppercases before its strip and pattern, and `StringConstraints` alone checks `pattern` before `to_upper`
- `PlainValidator` discards every validator and constraint to its left, `Annotated[int, Gt(0), PlainValidator(int)]` accepts `-5`
- Constraints after a validator drop out of the JSON schema, `Code` publishes a plain string
- `type` aliases over `Annotated` publish a named `$defs` entry, an assignment alias inlines its node
- `AwareDatetime` and `NaiveDatetime` require timezone state, pydantic ignores `at.Timezone` and `at.doc`
- `pattern` compiles with the Rust `regex` crate, a lookaround raises `SchemaError` at class build until `regex_engine="python-re"`

### [02.2]-[STRICTNESS]

Lax mode coerces by a fixed table, strict mode accepts exact types, and JSON input holds its own strict table:
- Lax `int` accepts `"1"` and `1.0` and rejects `1.5` with `int_from_float`, lax `list[int]` accepts a `tuple` or `set`
- `strict=True` as a class keyword makes every field strict, `Annotated[T, Strict(False)]` relaxes one field
- Strict Python input rejects strings for `date`, lists for `tuple`, and raw values for an enum, strict JSON accepts each one's JSON encoding
- `Sequence[str]` rejects a `str` with `sequence_str` and keeps the input's collection type, `tuple[T, ...]` yields one type
- `float` accepts `nan` and `inf` from Python, text, and JSON literals (`NaN`, `Infinity`), `allow_inf_nan=False` or `FiniteFloat` refuses them

## [03]-[VALIDATORS]

Validators attach by mode in `Annotated` beside their type, and a `type` alias holds them with it. Validation runs model `before`, decorator field `before`, `Annotated` `before` and `wrap` right to left, core, `Annotated` `after` left to right, decorator field `after`, then model `after`. Model `after` validators hold cross-field rules and run once every field validated, a failed field skips them:

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
- Model `before` validators receive any input object, a `dict` or not
- Field validators reading `info.data["lo"]` raise a raw `KeyError` once `lo` failed
- `PydanticCustomError` templates format `{name}` from `ctx` and leave `{name!r}` literal, ruff `RUF027` reports a placeholder naming a local
- `PydanticKnownError("greater_than", {"gt": 5})` raises a built-in error type with its default message
- Wrap validators call `handler(value)`, raising `PydanticUseDefault` takes the field default
- `PydanticOmit` from an item validator drops the item, `OnErrorOmit[T]` drops each invalid item
- `Annotated[list[T], FailFast()]` stops at its first invalid item, and every item reports by default
- `validation_error_cause=True` sets `__cause__` to an `ExceptionGroup` of the raised Python errors
- `hide_input_in_errors=True` drops input values from error text on models that read secrets, `errors()` keeps each `input`

### [03.1]-[CONTEXT]

Data a rule reads per call (a catalog, a unit scale, a caller's limits) enters through `context=` on a validate call, and `ValidationInfo[C]` types it for both checkers. One model serves every caller with no closure, global, or model per catalog:

```python
class Catalog(msgspec.Struct, frozen=True, kw_only=True):
    names: frozenset[str]


def known(name: str, info: ValidationInfo[Catalog | None], /) -> str:
    match info.context:
        case Catalog(names=names) if name in names:
            return name
        case Catalog():
            raise PydanticCustomError("unknown_name", "{entry} names no catalog entry", {"entry": name})
        case None:
            raise TypeError(name)


type Ref = Annotated[str, AfterValidator(known)]


class Link(BaseModel, frozen=True, extra="forbid"):
    source: Ref
    targets: tuple[Ref, ...]
```

`Link.model_validate_json(raw, context=catalog)` reports each unknown name at its index (`("targets", 1)`), and a call without a catalog raises `TypeError`:
- `context` reaches every nested model and item, and `model_dump(context=)` hands serializers their own through `SerializationInfo`
- Constructor calls pass no context, a model reading one validates through `model_validate` or an adapter
- `ValidationInfo` holds `data` (earlier fields that passed), `mode` (`"python"` or `"json"`), and `field_name` beside `context`

## [04]-[SERIALIZERS]

Types with a wire form pydantic states take no serializer: `Decimal` under `Field(decimal_places=2)` holds exact cents, dumps as JSON text, and reads back from text or a number. Serializers serve output no type states and attach in `Annotated` as `PlainSerializer(fn, return_type=, when_used=)` or `WrapSerializer(fn)` with `fn(value, handler, info)` branching on `info.mode_is_json()`. Sets dump in an iteration order string hashing varies per process, and `Names` writes sorted JSON:

```python
type Names = Annotated[frozenset[str], PlainSerializer(sorted, return_type=list[str], when_used="json")]
```

- `model_dump()` keeps a `frozenset`, JSON output validates back to an equal set, and `return_type` types the serialization schema as an array
- `@model_serializer(mode="wrap")` calls `handler(self)`, a `TypedDict` return keeps a typed schema and `dict[str, Any]` an open object
- `Field(exclude_if=predicate)` removes a field from dumps when the predicate holds for its value
- `model_dump_json()` returns `str`, `TypeAdapter.dump_json()` returns `bytes`, `model_dump(mode="json")` returns JSON builtins
- `SecretStr` dumps as a masked `SecretStr` in Python mode and as `"**********"` in JSON, `get_secret_value()` reads it at its one use
- `round_trip=True` dumps a `Json[T]` field back to JSON text, the default writes its parsed value
- Temporal values write ISO 8601 and round-trip, `ser_json_temporal="milliseconds"` with `val_temporal_unit="milliseconds"` round-trips a `datetime`
- `timedelta` reads numbers as seconds under `val_temporal_unit="milliseconds"`, and a millisecond dump reads back 1000 times longer
- Non-finite floats dump as JSON `null` a `float` field refuses, `ser_json_inf_nan="constants"` writes `NaN` and `Infinity` that read back
- `ser_json_bytes="base64"` pairs with `val_json_bytes="base64"`, alone it reads base64 text back as bytes of the text

### [04.1]-[SUBCLASSES]

Fields serialize by declared type: a field typed `Base` holding a `Sub` dumps `Base` fields, and dict input validates as `Base` with `Sub` keys dropped. Subclass shapes take a form that names them:
- Tagged unions keep each case's shape in both directions
- `class Holder[T: Base](BaseModel)` parametrized as `Holder[Sub]` validates and dumps `Sub`
- `polymorphic_serialization=True` on a base's configuration dumps held instances by runtime class and leaves validation as declared
- `SerializeAsAny[Base]` widens one field, `serialize_as_any=True` on a dump call widens every value

## [05]-[UNIONS]

Tagged unions select a member by a `Literal` field, and one member can hold more than one tag:

```python
class _Answer(BaseModel, frozen=True, extra="forbid"):
    stdout: str = ""
    stderr: str = ""


class Done(_Answer):
    status: Literal["ok"]


class Failed(_Answer):
    status: Literal["error", "raised"]
    message: str


type Answer = Annotated[Done | Failed, Field(discriminator="status")]
ANSWERS = TypeAdapter(tuple[Answer, ...])
```

- Unknown tags report `union_tag_invalid`, a missing tag `union_tag_not_found`, and member errors take a `loc` led by the tag (`(1, "error", "message")`)
- Tag fields typed `str` raise `PydanticUserError` code `discriminator-needs-literal`, a duplicate tag raises `TypeError` at build
- JSON schema emits `oneOf` with `discriminator.propertyName` and a `mapping` entry per tag

Unions chosen by a function of input take `Discriminator(fn)`, and `fn` receives raw input or a member instance and returns the class name each member's `Tag` holds (`Circle` holds `radius`, `Rect` holds `width` and `height`):

```python
def shape(value: object) -> str | None:
    match value:
        case {"radius": _} | Circle():
            return Circle.__name__
        case {"width": _, "height": _} | Rect():
            return Rect.__name__
        case _:
            return None


type Shape = Annotated[Annotated[Circle, Tag(Circle.__name__)] | Annotated[Rect, Tag(Rect.__name__)], Discriminator(shape, custom_error_type="shape", custom_error_message="input matches no shape")]
```

- `None` from `fn` raises the custom error type with its message
- Untagged unions run smart mode: an exact type wins, `int | str` keeps `"1"` a `str`, and `date | str` keeps an ISO string from Python and reads a `date` from JSON
- Untagged model unions take a member that validates, input meant for one member can validate as another through lax coercion
- Untagged unions report one error per member at every level of a recursive alias
- `Field(union_mode="left_to_right")` takes the first member that validates

## [06]-[ADAPTERS]

`TypeAdapter(T)` builds a validator and serializer once and gives any type the validate, dump, and schema calls of a model:
- `TypeAdapter` types its value from its argument under both checkers, an `Annotated` or `type` alias included
- `validate_strings` reads a string or a mapping of strings (environment, query) and rejects a list with `string_type`
- `from_attributes=True` on a validate call or in configuration reads a host object and its nested objects by attribute
- `config=` sets configuration on types without their own, a model or `TypedDict` target raises `PydanticUserError`
- `experimental_allow_partial=True` on adapter JSON validation completes a truncated `list`, `dict`, or `TypedDict`, a model field stays required
- Partial validation drops a truncated last element that fails, `"trailing-strings"` keeps a truncated last string

### [06.1]-[CALLS]

`@validate_call` validates arguments at each call against the signature, reports every argument error at once with a positional argument's index as `loc`, and raises `ValidationError` a host entry converts:
- `**kwargs: Unpack[TypedDict]` validates each keyword against its item
- `validate_return=True` validates each return, on an `async def` after its await
- Methods take `@validate_call` directly, under `@classmethod` it sits innermost
- PEP 695 type parameters left unbound validate as `Any`
- `config=ConfigDict(strict=True)` makes every argument strict, `raw_function` holds the undecorated function

## [07]-[TYPING]

Generic models take PEP 695 class parameters and parametrize at the boundary (`Knot[Unit, Positive].model_validate_json(raw)`):
- Each parametrization is one cached subclass, `Knot[Unit, Positive]` titling its schema and `Knot_Unit_Positive_` keying its `$defs` entry
- Annotations naming a class defined later leave a model incomplete until its first validate, dump, or schema call rebuilds it
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
- `json_schema_serialization_defaults_required=True` marks defaulted fields required in serialization mode
- `models_json_schema([(Model, mode), ...], ref_template="#/components/schemas/{model}")` shares one `$defs` across models
- `TypeAdapter.json_schemas([(key, mode, adapter), ...])` shares one `$defs` across adapters
- One-value `Literal` fields emit `const`
- `Field(description=, examples=, json_schema_extra=)`, configuration `json_schema_extra`, and `use_attribute_docstrings=True` add keys
- Validators and model serializer logic stay out of the schema, `WithJsonSchema` and `__get_pydantic_json_schema__` replace a node
- `alias_generator=to_camel` with `validate_by_name=True`, `validate_by_alias=True`, and `serialize_by_alias=True` maps both directions
- Field titles derive from the alias (`itemCount` titles `Itemcount`)
- `AliasChoices` and `AliasPath("names", 0)` select validation input, `serialization_alias` names output under `by_alias=True`

## [09]-[COST]

Validation cost sits in one schema build per type and one Rust pass per call, and declarations decide both:
- Adapters build once at module scope, an adapter built per call repeats schema generation
- `model_validate_json` parses and validates bytes in one pass under the JSON strict table, a msgspec or `json` decode before `model_validate` parses twice
- Discriminated unions read one tag, smart unions try each member, and a callable discriminator runs once per node of a recursive union
- `list`, `tuple`, and `dict` validate directly, `Sequence` and `Mapping` run an `isinstance` test and try each concrete type
- Nested `TypedDict` structures validate faster than nested models, a hot path holding no methods takes them
- `SkipValidation[T]` keeps a trusted value's type and skips its work, `defer_build=True` moves schema build to first use
- Use `python-coding-msgspec` for owned wire formats and domain records

## [10]-[SETTINGS]

Composition roots read settings through one `BaseSettings` class composing nested `BaseModel` groups. `settings_cached()` builds one instance per process, returns `Self`, and leaves a failed build uncached:

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


def settings() -> Outcome[Settings]:
    return catch(Settings.settings_cached, exception=ValueError)().map_error(faulted)
```

TOML `[store] host` and `APP_STORE__PORT=70000` return `store.port is invalid. Input should be less than 65536` beside `Settings holds no token`:
- `settings_customise_sources` returns sources highest priority first, a source left out of the tuple reads nothing
- File sources (TOML, pyproject, JSON, YAML) enter through the hook alone, a file key without its source warns and goes unread
- `BaseSettings` defaults to `extra="forbid"` and `validate_default=True`
- Use `python-coding-cyclopts` for command lines

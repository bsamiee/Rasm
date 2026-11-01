---
name: python-coding-msgspec
description: "Use when declaring msgspec records or codecs, covering struct options, constraints, validation, hooks, and conversion"
---

# [PYTHON_CODING_MSGSPEC]

[REFERENCES]:
- [01]-[MESSAGEPACK](references/messagepack.md): Extension types for out-of-band references, values MessagePack encodes unlike JSON

## [01]-[STRUCT_OPTIONS]

Subclasses inherit every struct option, one base class declares encoding options every union member shares:

```python
class Event(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True, rename="camel", tag_field="kind", tag=str.lower): ...
```

`Closed(Event, frozen=True, kw_only=True)` decodes `{"kind": "closed", "itemId": 1}`, unknown keys fail (`` Object contains unknown field `extra` ``):
- `tag=True` and tag callables read `__qualname__`, a nested class tags as `Outer.Inner`
- Enum tags take `<Member>.value`, a `StrEnum` member passes type checkers and raises `TypeError` at class creation
- Decoders of one tagged struct accept a missing tag, union decoders require it
- `__init_subclass__` runs before `__struct_fields__` exists, and a class keyword msgspec lacks raises `TypeError`
- Unions hold tagged structs alone or one dict-like type (`Struct`, `dict`, `TypedDict`, dataclass)
- Unions hold one str-like type (`str`, enum, `datetime`, `UUID`, `Decimal`, `bytes`), and `Literal["a"] | str` decodes

Options beyond `frozen` and `kw_only` change encoding or memory under a condition:

| [INDEX] | [OPTION]                     | [EFFECT]                             | [CONDITION]                                                        |
| :-----: | :--------------------------- | :----------------------------------- | :----------------------------------------------------------------- |
|  [01]   | `rename=`                    | Wire names by convention or mapping  | Callables returning `None` keep a name, `field(name=...)` wins     |
|  [02]   | `forbid_unknown_fields=True` | Decode refuses keys a record lacks   | Schema emits `additionalProperties: false`                         |
|  [03]   | `omit_defaults=True`         | Encode drops fields at their default | `frozendict()` default omits, `default_factory=frozendict` encodes |
|  [04]   | `array_like=True`            | Positional array on the wire         | Error paths read `$[<index>]`                                      |
|  [05]   | `cache_hash=True`            | Hash computed once                   | Requires `frozen=True`                                             |
|  [06]   | `gc=False`                   | GC skips instances with containers   | `dict=True` or `weakref=True` raises `ValueError`, a cycle leaks   |

## [02]-[CONSTRAINTS]

Constraints take limits from the module that states them:

```python
type Finite = Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max)]
type Key = Annotated[str, msgspec.Meta(pattern=r"^[a-z][a-z0-9_]*$")]
```

`Finite` refuses `nan` and `inf` from `convert`, and JSON decodes refuse an overflowing number without a constraint (`Number out of range`):
- Float `gt=0.0` accepts every subnormal, `ge=sys.float_info.min` excludes them
- `max_length` counts code points on `str` and bytes on `bytes`, a limit a host states in bytes bounds the encoded text
- Each side takes one bound across nested `Annotated` layers, a second raises `TypeError`, and one `Meta` holding `gt` and `ge` fails type checkers
- `pattern` searches anywhere in a value, `^` and `$` anchor it
- Numeric constraints on types besides `int` and `float` raise `TypeError` at first decode or `convert`, `Decimal` bounds go in `__post_init__`

## [03]-[VALIDATION]

`convert` over `to_builtins` reruns every constraint and `__post_init__` once, and one merge with a decoded object patches any record:

```python
@catch(exception=msgspec.DecodeError)
def _patched[S: msgspec.Struct](record: S, raw: bytes, /) -> S:
    patched: S = msgspec.convert(msgspec.to_builtins(record) | msgspec.json.decode(raw, type=dict[str, object]), type(record))
    return patched


def patched[S: msgspec.Struct](record: S, raw: bytes, /) -> Outcome[S]:
    return _patched(record, raw).map_error(partial(Fault.of, Reason.INVALID, type(record).__name__))
```

Over a `rename="camel"` record, `{"closingNote": null}` clears `closing_note`, absent keys keep their field, and `{"title": ""}` fails at `$.title`. Python name `closing_note` fails under `forbid_unknown_fields=True` and drops unread without it:
- `structs.force_setattr(self, "<field>", <canonical>)` in frozen `__post_init__` gives decode, `convert`, `replace`, `==`, and `cache_hash` one form
- `copy.deepcopy` runs `__post_init__`, `copy.copy` skips it, and `convert(record, T)` returns `record` unchecked
- `structs.replace` takes `**changes: Any`, a change of wrong type passes type checkers and runtime
- `__post_init__` errors besides `TypeError` and `ValueError` (`KeyError`) propagate raw from decode and `convert`
- Constructors store arguments unconverted, an `int` passed to a `float` field encodes as `1` where a decoded record writes `1.0`
- `T | UnsetType = UNSET` tells absence from `null`, encode and `to_builtins` drop it, `structs.asdict` keeps it, and `is UNSET` narrows its type

## [04]-[SCHEMA_EVOLUTION]

Readers and writers on different record releases exchange data when every change keeps the old shape decodable:
- New fields take a default, array-like records append them, and older readers skip trailing items
- Readers of a newer writer leave `forbid_unknown_fields` off, the option refuses new fields (`` Expected `array` of at most length 2 ``)
- Python renames keep the wire name through `field(name="<wire>")`, and a wire rename is a new field
- Field types stay fixed, and a type change is a new field beside the old one
- Union decoders refuse a tag they lack (`` Invalid value 'c' - at `$.kind` ``), and readers add a new member before any writer sends it

## [05]-[CODECS]

Records declaring only fields a consumer reads skip the rest of a document unallocated, `msgspec.Raw` fields hold bytes undecoded:

```python
class Routed(msgspec.Struct, frozen=True, kw_only=True):
    topic: str
    body: msgspec.Raw


ROUTED = msgspec.json.Decoder(Routed)
```

`ROUTED` refuses a malformed body unbuilt, encoders write its bytes verbatim, and `msgspec.json.decode(routed.body, type=T)` decodes it for known `T`. Format modules (`json`, `msgpack`, `yaml`, `toml`) share one encoder and decoder API:
- `bytes(<record>.body)` copies a body out, `Raw.copy()` detaches it from its source buffer, and `convert` refuses a `Raw` target
- `Encoder.encode_lines` and `Decoder.decode_lines` write and read newline-delimited JSON, refused lines fail at `$[<line>]`
- `msgspec.yaml` needs PyYAML, `msgspec.toml` decodes through `tomllib` and encodes through `tomli_w`
- `order="deterministic"` sorts dict keys and set items a default encode writes in per-process hash order, `"sorted"` adds struct fields
- `encode_into(value, buffer, offset)` returns `None` and resizes `buffer` to end at its last written byte, `-1` appends
- `Sequence` and `Mapping` annotations decode to `list` and `dict`
- `decimal_format` writes `Decimal` as `"string"`, `"number"`, or a callable's `str`, and `Decoder(float_hook=Decimal)` reads untyped floats exactly
- `NaN` and infinities encode as JSON `null`, and a `float` field refuses `null` on decode

## [06]-[HOOKS]

`enc_hook` receives values msgspec cannot encode, `dec_hook` annotations it cannot decode, and both raise `NotImplementedError` for unsupported types. `TEXTUAL` serves `enc_hook`, `dec_hook`, and `schema_hook` for types with a text form:

```python
TEXTUAL: frozendict[type, frozendict[str, str]] = frozendict({Path: frozendict({"type": "string"}), IPv4Address: frozendict({"type": "string", "format": "ipv4"})})


def encoded(value: object, /) -> str:
    if not isinstance(value, tuple(TEXTUAL)):
        raise NotImplementedError(type(value))
    return str(value)


def decoded(kind: type, raw: object, /) -> object:
    if kind not in TEXTUAL:
        raise NotImplementedError(kind)
    if not isinstance(raw, str):
        raise TypeError(f"Expected `str`, got `{type(raw).__name__}`")
    return kind(raw)


def schema(kind: type, /) -> dict[str, object]:
    if (node := TEXTUAL.get(kind)) is None:
        raise NotImplementedError(kind)
    return dict(node)
```

`Encoder(enc_hook=encoded)` writes and `Decoder(<Record>, dec_hook=decoded)` reads every `Path` and `IPv4Address` field, a new type adds one entry:
- `enc_hook` receives subclass instances `isinstance` matches (`PosixPath` for `Path`), `dec_hook` any wire type (`IPv4Address(5)` builds `0.0.0.5`)
- `ValueError` and `TypeError` from a hook report at their path (`` Octet 256 (> 255) not permitted in '10.0.0.256' - at `$.host` ``)
- `NotImplementedError` and every other class propagate raw, and a `ValidationError` from a `convert` inside `dec_hook` loses its path
- Natively decoded types (`Decimal`, `datetime`, `UUID`) never reach `dec_hook`
- Custom types share a union with `None` alone

## [07]-[CONVERSION]

`convert` and every decoder coerce by input and target, and `strict=False` on either adds the text and number coercions a text source needs:

| [INDEX] | [INPUT]          | [TARGET]                | [STRICT] | [LAX]                      |
| :-----: | :--------------- | :---------------------- | :------- | :------------------------- |
|  [01]   | `"1"`, `"1.0"`   | `int`                   | Refused  | `1`                        |
|  [02]   | `1.0`            | `int`                   | Refused  | `1`                        |
|  [03]   | `"true"`, `"0"`  | `bool`                  | Refused  | `True`, `False`            |
|  [04]   | `1`              | `float`                 | `1.0`    | `1.0`                      |
|  [05]   | `"nan"`, `"inf"` | `float`                 | Refused  | `nan`, `inf`               |
|  [06]   | `60`, `"60"`     | `datetime`, `timedelta` | Refused  | UTC epoch seconds, seconds |
|  [07]   | `1`              | `str`                   | Refused  | Refused                    |
|  [08]   | `"1.5"`, `1.5`   | `int`                   | Refused  | Refused                    |

Lax unions holding `str` keep text (`"1"` into `int | str` stays `"1"`), and `Finite` refuses the `nan` and `inf` lax text yields. Options decide which names, keys, and native values a conversion reads:
- Mapping input reads wire names, and a Python name under `rename` fails as a missing field
- `from_attributes=True` reads Python names through nested objects
- `str_keys=True` decodes text keys into `int` or `UUID` keys, and `builtin_types` passes listed native values through
- `to_builtins` applies renames, tags, omitted defaults, and `UNSET`, and writes `Decimal`, `datetime`, `UUID`, `bytes` as text strict `convert` reads
- `structs.asdict` copies Python names one level deep

## [08]-[INSPECTION]

`msgspec.inspect.type_info` resolves a type once into a node tree holding wire names, defaults, tags, and expanded `Meta` constraints. One walk over the tree derives what each shape consumer reads (environment names, flags, columns). `leaves` lists every field path by wire name with its node:

```python
def leaves(info: msgspec.inspect.Type, prefix: tuple[str, ...] = (), /) -> tuple[tuple[tuple[str, ...], msgspec.inspect.Type], ...]:
    match info:
        case msgspec.inspect.StructType(fields=fields):
            return tuple(*leaves(field.type, (*prefix, field.encode_name)) for field in fields)
        case msgspec.inspect.Metadata(type=inner):
            return leaves(inner, prefix)
        case _:
            return ((prefix, info),)
```

Under `rename="camel"`, `leaves(msgspec.inspect.type_info(<Record>))` yields `(("dataStore", "port"), IntType(...))`. One `convert(..., strict=False)` coerces text values nested at those paths. `forbid_unknown_fields=True` refuses unknown keys at their parent (`` Object contains unknown field `prt` - at `$.dataStore` ``):
- Constraints expand into their node (`IntType(ge=1, le=65535)`), and schema or `extra` keys wrap it in the `Metadata` node `leaves` unwraps
- Optional fields arrive as `UnionType` holding `NoneType()`, and a tagged union as `UnionType` of `StructType` nodes holding `tag_field` and `tag`
- `multi_type_info((A, B))` analyzes types together and shares one node per record

## [09]-[SCHEMAS]

`msgspec.json.schema(T)` and `schema_components(types, ref_template=)` emit JSON Schema 2020-12 from the declarations decode reads. `schema_hook` supplies the node of each type `dec_hook` serves:

```python
(HOST,), COMPONENTS = msgspec.json.schema_components((Host,), schema_hook=schema, ref_template="#/components/schemas/{name}")
```

Over `Host` with `address: IPv4Address` and `port` bounded by `Meta(ge=1, le=65535)`, `HOST` is `{"$ref": "#/components/schemas/Host"}`. `COMPONENTS` holds `Host` for an OpenAPI `components` table, `address` as an `ipv4` node and `port` with `minimum` and `maximum`:
- Wire names, `Meta` constraints, tags as one-value `enum`, defaults, and `required` follow the declaration, and docstrings become `description`
- Tagged unions emit `anyOf` with a `discriminator` mapping each tag to its member
- `__post_init__` rules and hook logic stay out of the schema
- Custom types without a `schema_hook` raise `TypeError`, and `Meta(extra_json_schema=...)` on an alias supplies one type's node

## [10]-[TYPING]

Decoders expand `type` aliases, PEP 695 parameters, and unquoted forward references at first use, and unparametrized parameters decode as their bound. Forms msgspec reads in part take a replacement:

| [INDEX] | [FORM]                                  | [BEHAVIOR]                          | [REPLACEMENT]                    |
| :-----: | :-------------------------------------- | :---------------------------------- | :------------------------------- |
|  [01]   | PEP 696 default `[T = int]`             | Unparametrized `T` decodes as `Any` | Parametrized target (`Box[int]`) |
|  [02]   | Recursive `type` alias                  | `RecursionError` at type analysis   | Recursive struct field           |
|  [03]   | `ReadOnly` TypedDict item               | `TypeError` at type analysis        | Frozen struct                    |
|  [04]   | TypedDict `closed=True`, `extra_items=` | Extra keys dropped unvalidated      | `forbid_unknown_fields=True`     |

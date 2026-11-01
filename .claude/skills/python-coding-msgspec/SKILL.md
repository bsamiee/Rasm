---
name: python-coding-msgspec
description: "Use when declaring msgspec records or codecs, covering struct options, constraints, validation, hooks, and conversion"
---

# [PYTHON_CODING_MSGSPEC]

Covers msgspec records and codecs, from struct options and constraints to validation, schema evolution, hooks, conversion, inspection, and schemas.

[REFERENCES]:
- [01]-[MESSAGEPACK](references/messagepack.md): Out-of-band references through extension types and values MessagePack encodes differently from JSON

## [01]-[STRUCT_OPTIONS]

Subclasses inherit every struct option, and one base class declares the encoding options every union member shares:

```python
class Event(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True, rename="camel", tag_field="kind", tag=str.lower): ...
```

`Closed(Event, frozen=True, kw_only=True)` holding `item_id` decodes `{"kind": "closed", "itemId": 1}` and refuses an unknown key (`` Object contains unknown field `extra` ``):
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

Constraints take their limits from the module that states them:

```python
type Finite = Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max)]
type Key = Annotated[str, msgspec.Meta(pattern=r"^[a-z][a-z0-9_]*$")]
```

`Finite` refuses `nan` and `inf` from `convert`, and JSON decodes refuse an overflowing number without a constraint (`Number out of range`):
- Float `gt=0.0` accepts every subnormal, `ge=sys.float_info.min` excludes them
- `max_length` counts code points on `str` and bytes on `bytes`, a limit a host states in bytes bounds the encoded text
- Each side takes one bound across nested `Annotated` layers, a second raises `TypeError`, and one `Meta` holding `gt` and `ge` fails type checkers
- `pattern` searches anywhere in a value, `^` and `$` anchor it
- Numeric constraints on types other than `int` and `float` (`str`, `Decimal`) build the class and raise `TypeError` at first decode or `convert`, and `Decimal` constraints sit in `__post_init__`

## [03]-[VALIDATION]

`convert` over `to_builtins` revalidates a record against every constraint and `__post_init__` once, and one merge of a decoded object patches any record:

```python
@catch(exception=msgspec.DecodeError)
def _patched[S: msgspec.Struct](record: S, raw: bytes, /) -> S:
    patched: S = msgspec.convert(msgspec.to_builtins(record) | msgspec.json.decode(raw, type=dict[str, object]), type(record))
    return patched


def patched[S: msgspec.Struct](record: S, raw: bytes, /) -> Outcome[S]:
    return _patched(record, raw).map_error(partial(Fault.of, Reason.INVALID, type(record).__name__))
```

Over a `rename="camel"` record holding `title` and `closing_note`, `{"closingNote": null}` clears the note, an absent key keeps its field, `{"title": ""}` fails at `$.title`, and `forbid_unknown_fields=True` refuses a Python name `closing_note` that a record without it drops unread:
- `msgspec.structs.force_setattr(self, "<field>", <canonical>)` in a frozen record's `__post_init__` stores one canonical form (sorted, deduplicated) through construction, decode, `convert`, and `replace`, so equality and `cache_hash` read it
- `copy.deepcopy` runs `__post_init__`, `copy.copy` skips it, and `convert(record, T)` returns `record` unchecked
- `structs.replace` takes `**changes: Any`, and a change of wrong type passes type checkers and runtime
- `__post_init__` raises other than `TypeError` and `ValueError` (`KeyError`) propagate raw from decode and `convert`
- Constructors store arguments unconverted, an `int` passed to a `float` field encodes as `1` where a decoded record writes `1.0`
- Records that tell absence from `null` type the field `T | msgspec.UnsetType = msgspec.UNSET`, encode and `to_builtins` drop it unset, `is msgspec.UNSET` narrows it for type checkers, and `structs.asdict` keeps `UNSET`

## [04]-[SCHEMA_EVOLUTION]

Readers and writers on different releases of a record exchange data when every change keeps the old shape decodable:
- New fields take a default, and array-like records add them at the end, where an older reader skips the trailing item
- Records a newer writer feeds leave `forbid_unknown_fields` off, the option refuses a newer writer's field (`` Expected `array` of at most length 2 ``)
- Python renames keep the wire name through `field(name="<wire>")`, and a wire rename is a new field
- Field types stay fixed, and a type change is a new field beside the old one
- Union decoders refuse a tag they lack (`` Invalid value 'c' - at `$.kind` ``), and readers add a new member before any writer sends it

## [05]-[CODECS]

Records declaring only the fields a consumer reads skip the rest of a document unallocated, and `msgspec.Raw` fields hold their bytes undecoded:

```python
class Routed(msgspec.Struct, frozen=True, kw_only=True):
    topic: str
    body: msgspec.Raw


ROUTED = msgspec.json.Decoder(Routed)
```

`ROUTED` refuses a malformed body without building it, an encoder writes the body's bytes unchanged with whitespace kept, and `msgspec.json.decode(routed.body, type=T)` decodes it where a consumer knows `T`. Format modules (`json`, `msgpack`, `yaml`, `toml`) share one encoder and decoder API:
- `bytes(<record>.body)` copies a body out, `Raw.copy()` detaches it from its source buffer, and `convert` refuses a `Raw` target
- `Encoder.encode_lines` and `Decoder.decode_lines` write and read newline-delimited JSON, a refused line failing at `$[<line>]`
- `msgspec.yaml` needs PyYAML, and `msgspec.toml` decodes through `tomllib` and encodes through `tomli_w`
- `order="deterministic"` sorts dict keys and the sets a default encode writes in per-process hash order, and `"sorted"` sorts struct fields with them
- `encode_into(value, buffer, offset)` returns `None` and resizes `buffer` to end at its last written byte, `-1` appends
- `Sequence` and `Mapping` annotations decode to `list` and `dict`
- `decimal_format` writes `Decimal` as `"string"`, `"number"`, or a callable's return (a quantized `str`), and `Decoder(float_hook=Decimal)` reads untyped floats exactly
- `NaN` and infinities encode as JSON `null`, and a `float` field refuses `null` on decode

## [06]-[HOOKS]

`enc_hook` receives each value msgspec cannot encode and `dec_hook` each annotation it cannot decode, and both raise `NotImplementedError` for an unsupported type. `TEXTUAL` serves `enc_hook`, `dec_hook`, and `schema_hook` for types with a text form:

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

`msgspec.json.Encoder(enc_hook=encoded)` writes and `msgspec.json.Decoder(<Record>, dec_hook=decoded)` reads every `Path` and `IPv4Address` field, and a new type adds one entry:
- `enc_hook` receives a value of its runtime class (`PosixPath` for `Path`) that `isinstance` matches, and `dec_hook` receives the annotation and a decoded value of any wire type, where `IPv4Address(5)` builds `0.0.0.5`
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

Unions holding `str` keep text as `str` under lax coercion (`"1"` into `int | str` stays `"1"`), and a `Finite` constraint refuses the `nan` and `inf` lax text yields. Options decide which names, keys, and native values a conversion reads:
- Mapping input reads wire names, and a Python name under `rename` fails as a missing field
- `from_attributes=True` reads Python names through nested objects
- `str_keys=True` decodes text keys into `int` or `UUID` keys, and `builtin_types` passes listed native values through
- `to_builtins` applies renames, tags, omitted defaults, and `UNSET`, and writes `Decimal`, `datetime`, `UUID`, and `bytes` as the text a strict `convert` reads back
- `structs.asdict` copies Python names one level deep

## [08]-[INSPECTION]

`msgspec.inspect.type_info` resolves a type once into a tree of nodes holding wire names, defaults, tags, and expanded `Meta` constraints, and one walk over that tree derives what every consumer of a record's shape reads (environment names, flags, columns). `leaves` lists every field path by wire name with its node:

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

`leaves(msgspec.inspect.type_info(<Record>))` yields `(("dataStore", "port"), IntType(...))` under `rename="camel"`, and text values nested at those paths convert in one `convert(..., strict=False)` that refuses an unknown key at its parent (`` Object contains unknown field `prt` - at `$.dataStore` ``) under `forbid_unknown_fields=True`:
- Constraints expand into their node (`IntType(ge=1, le=65535)`), and schema or `extra` keys wrap it in the `Metadata` node `leaves` unwraps
- Optional fields arrive as `UnionType` holding `NoneType()`, and a tagged union as `UnionType` of `StructType` nodes holding `tag_field` and `tag`
- `multi_type_info((A, B))` analyzes types together and shares one node per record

## [09]-[SCHEMAS]

`msgspec.json.schema(T)` and `schema_components(types, ref_template=)` emit JSON Schema 2020-12 from the declarations decode reads, and `schema_hook` supplies the node of each type `dec_hook` serves:

```python
(HOST,), COMPONENTS = msgspec.json.schema_components((Host,), schema_hook=schema, ref_template="#/components/schemas/{name}")
```

Over `Host` holding an `IPv4Address` `address` and a `port` bounded `Meta(ge=1, le=65535)`, `HOST` is `{"$ref": "#/components/schemas/Host"}`, and `COMPONENTS` holds `Host` for an OpenAPI `components` table with `address` as an `ipv4` node and `port` with `minimum` and `maximum`:
- Wire names, `Meta` constraints, tags as one-value `enum`, defaults, and `required` follow the declaration, and docstrings become `description`
- Tagged unions emit `anyOf` with a `discriminator` mapping each tag to its member
- `__post_init__` rules and hook logic stay out of the schema
- Custom types without a `schema_hook` raise `TypeError`, and `Meta(extra_json_schema=...)` on an alias supplies one type's node

## [10]-[TYPING]

Decoders expand `type` aliases, PEP 695 parameters, and unquoted references to later classes at first use, and an unparametrized parameter decodes as its bound. Forms msgspec reads in part take a replacement:

| [INDEX] | [FORM]                                  | [BEHAVIOR]                          | [REPLACEMENT]                    |
| :-----: | :-------------------------------------- | :---------------------------------- | :------------------------------- |
|  [01]   | PEP 696 default `[T = int]`             | Unparametrized `T` decodes as `Any` | Parametrized target (`Box[int]`) |
|  [02]   | Recursive `type` alias                  | `RecursionError` at type analysis   | Recursive struct field           |
|  [03]   | `ReadOnly` TypedDict item               | `TypeError` at type analysis        | Frozen struct                    |
|  [04]   | TypedDict `closed=True`, `extra_items=` | Extra keys dropped unvalidated      | `forbid_unknown_fields=True`     |

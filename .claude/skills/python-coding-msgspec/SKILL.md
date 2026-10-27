---
name: python-coding-msgspec
description: "Use when declaring msgspec records or codecs, covering struct families, bounds as types, validation paths, hooks, and conversion"
---

# [PYTHON_CODING_MSGSPEC]

Covers msgspec records and codecs past python-coding's record rules, from a family's wire policy and bounds as types to validation paths, schema evolution, hooks, conversion, inspection, and schemas. Snippets build on python-coding's `Fault`, `Outcome`, and `Reason`.

[REFERENCES]:
- [01]-[MESSAGEPACK](references/messagepack.md): Out-of-band references through extension types and values MessagePack encodes apart from JSON

## [01]-[FAMILIES]

Subclasses inherit every struct option, and `Event` states one wire policy for each case:

```python
class Event(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True, rename="camel", tag_field="kind", tag=str.lower): ...


class Opened(Event, frozen=True, kw_only=True):
    item_id: int


class Closed(Event, frozen=True, kw_only=True):
    item_id: int
    closed_by: str | None = None


type Change = Opened | Closed

CHANGES = msgspec.json.Decoder(tuple[Change, ...])
```

`CHANGES` decodes `[{"kind": "closed", "itemId": 1}]` as `(Closed(item_id=1),)`, and an unknown key fails at its path (`` Object contains unknown field `extra` - at `$[0]` ``):
- `tag=True` and a tag callable read `__qualname__`, a nested case tags as `Outer.Inner`
- Enum tags pass `<Member>.value`, a `StrEnum` member passes both checkers and raises `TypeError` at class build
- Decoders over one case accept a missing tag, and a union decoder requires it
- `__init_subclass__` runs before `__struct_fields__` exists, and a class keyword msgspec lacks raises `TypeError`
- Unions hold tagged structs alone or one dict-like type (`Struct`, `dict`, `TypedDict`, dataclass)
- Str-like types (`str`, enum, `datetime`, `UUID`, `Decimal`, `bytes`) take one place in a union, and `Literal["a"] | str` decodes

Options past `frozen` and `kw_only` set a wire or memory property under a constraint:

| [INDEX] | [OPTION]                     | [EFFECT]                                  | [CONSTRAINT]                                               |
| :-----: | :--------------------------- | :---------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `rename=`                    | Wire names from a convention or a mapping | `field(name=...)` wins for its field                       |
|  [02]   | `forbid_unknown_fields=True` | Decode refuses keys a record lacks        | Schema emits `additionalProperties: false`                 |
|  [03]   | `omit_defaults=True`         | Encode drops fields at their default      | `default_factory` other than a builtin constructor encodes |
|  [04]   | `array_like=True`            | Positional array on the wire              | Error paths read `$[<index>]`                              |
|  [05]   | `cache_hash=True`            | Hash computed once                        | Requires `frozen=True`                                     |
|  [06]   | `gc=False`                   | Collector skips instances with containers | Excludes `dict=True`, and a cycle among instances leaks    |
|  [07]   | `order=True`                 | Comparison by field tuple                 | Declaration order decides                                  |

## [02]-[BOUNDS]

Bound limits derive from the module that states them, and a limit a host states at runtime is a `Meta` value one `convert` applies. Tuple targets validate parts of different types in one `convert`:

```python
type Finite = Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max)]
type Positive = Annotated[float, msgspec.Meta(ge=sys.float_info.min, le=sys.float_info.max)]
type Key = Annotated[str, msgspec.Meta(pattern=r"^[a-z][a-z0-9_]*$")]


class Form(msgspec.Struct, frozen=True, kw_only=True):
    template: str
    constraint: msgspec.Meta


@catch(exception=msgspec.ValidationError)
def _checked(parts: tuple[str, bytes], constraint: msgspec.Meta, /) -> tuple[str, bytes]:
    checked: tuple[str, bytes] = msgspec.convert(parts, tuple[Key, Annotated[bytes, constraint]])
    return checked


def named(form: Form, key: str, /) -> Outcome[str]:
    name = form.template.format(key=key)
    return _checked((key, name.encode()), form.constraint).map(lambda _: name).map_error(partial(Fault.of, Reason.INVALID, name))
```

`named` returns the formatted name, or an `INVALID` fault holding `` Expected `bytes` of length <= <n> - at `$[1]` `` for a long name and the pattern failure at `$[0]` for a malformed key:
- `max_length` counts code points on `str` and bytes on `bytes`, a limit a host states in bytes bounds the encoded text
- `Finite` refuses `nan` and `inf` from `convert`, and a JSON decode refuses an overflowing number on its own
- Float `gt=0.0` and `lt=0.0` compare as `ge` and `le` on the smallest subnormal, and Blender's flush to zero admits `0.0`
- `Positive` refuses `0.0` in every process
- Each side takes one bound across layers, a second raises `TypeError`, and one `Meta` holding `gt` and `ge` fails both checkers
- `pattern` searches anywhere in a value, `^` and `$` anchor it
- Numeric bounds on types other than `int` and `float` (`str`, `Decimal`) build the class and raise `TypeError` at first decode or `convert`
- `Decimal` bounds sit in `__post_init__`

## [03]-[VALIDATION]

`convert` over `to_builtins` revalidates a record against every bound and `__post_init__` once. Patches tell an absent key from `null` through `UNSET`, and `to_builtins` drops each unset field:

```python
type Title = Annotated[str, msgspec.Meta(min_length=1)]


class Draft(msgspec.Struct, frozen=True, kw_only=True):
    title: Title
    note: str | None = None


class Patch(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True):
    title: Title | msgspec.UnsetType = msgspec.UNSET
    note: str | msgspec.UnsetType | None = msgspec.UNSET


@catch(exception=msgspec.ValidationError)
def _patched(draft: Draft, patch: Patch, /) -> Draft:
    return msgspec.convert(msgspec.to_builtins(draft) | msgspec.to_builtins(patch), Draft)


def patched(draft: Draft, patch: Patch, /) -> Outcome[Draft]:
    return _patched(draft, patch).map_error(partial(Fault.of, Reason.INVALID, Patch.__name__))
```

`{"note": null}` clears `note`, `{"title": "b"}` keeps it, and a `Patch(title="")` built in process fails the merge at `$.title`. In-process paths validate apart from decode:
- `copy.deepcopy` runs `__post_init__`, `copy.copy` skips it, and `convert(record, T)` returns `record` unchecked
- `structs.replace` takes `**changes: Any`, and a change of wrong type passes both checkers and the runtime
- `__post_init__` raises other than `TypeError` and `ValueError` (`KeyError`) propagate raw from decode and `convert`
- `to_builtins` writes `Decimal`, `datetime`, `UUID`, and `bytes` as text a strict `convert` reads back
- `is msgspec.UNSET` and `case msgspec.UNSET:` narrow a field for both checkers, and `structs.asdict` keeps `UNSET`
- Constructors store arguments unconverted, an `int` passed to a `float` field encodes as `1` where a decoded record writes `1.0`

## [04]-[EVOLUTION]

Readers and writers built from different releases of a record share one wire when every change keeps the old shape decodable. `ReadingNext` appends a defaulted field to `Reading`:

```python
class Reading(msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    sensor: str
    value: float


class ReadingNext(msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    sensor: str
    value: float
    unit: str = "K"
```

`Reading` decodes `["t", 1.0, "K"]` and skips the trailing item, and `ReadingNext` decodes `["t", 1.0]` with `unit="K"`:
- New fields take a default, and array-like records add them at the end
- `forbid_unknown_fields=True` refuses a new field (`` Expected `array` of at most length 2 ``) and stays off records a newer writer feeds
- Python renames keep the wire name through `field(name="<wire>")`, and a wire rename is a new field
- Field types stay fixed, and a type change is a new field beside the old one
- Union decoders refuse a tag they lack (`` Invalid value 'C' - at `$.type` ``), and a reader learns a new case before any writer sends it

## [05]-[CODECS]

Records declaring only the fields a consumer reads skip the rest of a document unallocated, and `msgspec.Raw` fields hold their bytes undecoded:

```python
class Routed(msgspec.Struct, frozen=True, kw_only=True):
    topic: str
    body: msgspec.Raw


ROUTED = msgspec.json.Decoder(Routed)


def routed(raw: bytes, /) -> Outcome[Routed]:
    return catch(ROUTED.decode, exception=msgspec.DecodeError)(raw).map_error(partial(Fault.of, Reason.INVALID, Routed.__name__))
```

Encoders write a `Routed` body's bytes unchanged, whitespace kept, and a malformed body fails the decode. Format modules (`json`, `msgpack`, `yaml`, `toml`) share one codec model:
- `bytes(<record>.body)` copies a body out, `Raw.copy()` detaches it from its source buffer, and `convert` refuses a `Raw` target
- `msgspec.yaml` needs PyYAML, and `msgspec.toml` decodes through `tomllib` and encodes through `tomli_w`
- Default order writes sets in hash order, and `PYTHONHASHSEED` changes it
- `order="deterministic"` sorts dict keys and sets, and `"sorted"` sorts struct fields with them
- `encode_into(value, buffer, offset)` returns `None` and resizes `buffer` to end at its last written byte, `-1` appends
- `Sequence` and `Mapping` annotations decode to `list` and `dict`
- `Decimal` encodes as text unless `decimal_format="number"`, and `Decoder(float_hook=Decimal)` reads untyped floats exactly
- `NaN` and infinities encode as JSON `null`, and a `float` field refuses `null` on decode
- Naive `datetime` encodes without an offset, aware UTC with `Z`, `timedelta` as an ISO duration, and `bytes` as base64

## [06]-[HOOKS]

`enc_hook` receives each value msgspec cannot encode and `dec_hook` each annotation it cannot decode, and both raise `NotImplementedError` for a type they lack. `TEXTUAL` serves `enc_hook`, `dec_hook`, and `schema_hook` for types with a text form:

```python
TEXTUAL: frozendict[type, frozendict[str, str]] = frozendict({Path: frozendict({"type": "string"}), IPv4Address: frozendict({"type": "string", "format": "ipv4"})})


def encoded(value: object, /) -> str:
    if not isinstance(value, tuple(TEXTUAL)):
        raise NotImplementedError(type(value))
    return str(value)


def decoded(kind: type, raw: object, /) -> object:
    if kind not in TEXTUAL:
        raise NotImplementedError(kind)
    return kind(raw)


def schema(kind: type, /) -> dict[str, object]:
    if (node := TEXTUAL.get(kind)) is None:
        raise NotImplementedError(kind)
    return dict(node)


ENCODER = msgspec.json.Encoder(enc_hook=encoded)
```

`msgspec.json.Decoder(<Record>, dec_hook=decoded)` reads every `Path` and `IPv4Address` field, and a new type adds one row:
- `enc_hook` receives a value of its runtime class (`PosixPath` for `Path`) that `isinstance` matches, and `dec_hook` receives the annotation
- Constructor raises of `ValueError` and `TypeError` report at their path (`` Octet 256 (> 255) not permitted in '10.0.0.256' - at `$.host` ``)
- `NotImplementedError` and every other class propagate raw, and a `ValidationError` from a `convert` inside `dec_hook` loses its path
- Natively decoded types (`Decimal`, `datetime`, `UUID`) never reach `dec_hook`
- Custom types share a union with `None` alone

## [07]-[CONVERSION]

`convert` and every decoder coerce by input and target, and `strict=False` on either adds the text and number coercions a text source needs:

| [INDEX] | [INPUT]                     | [TARGET]                | [STRICT] | [LAX]    |
| :-----: | :-------------------------- | :---------------------- | :------- | :------- |
|  [01]   | `"1"`, `"1.0"`              | `int`                   | Refused  | `1`      |
|  [02]   | `1.0`                       | `int`                   | Refused  | `1`      |
|  [03]   | `"true"`                    | `bool`                  | Refused  | `True`   |
|  [04]   | `1`                         | `float`                 | `1.0`    | `1.0`    |
|  [05]   | ISO text, UUID text, base64 | `date`, `UUID`, `bytes` | Accepted | Accepted |
|  [06]   | `1`                         | `str`                   | Refused  | Refused  |
|  [07]   | `"1.5"`, `1.5`              | `int`                   | Refused  | Refused  |

Options decide which names, keys, and native values a conversion reads:
- Mapping input reads wire names, and a python name under `rename` fails as a missing field
- `from_attributes=True` reads python names through nested objects
- `str_keys=True` decodes text keys into `int` or `UUID` keys, and `builtin_types` passes listed native values through
- `to_builtins` applies renames, tags, omitted defaults, and `UNSET`, and `structs.asdict` copies python names one level deep

## [08]-[INSPECTION]

`msgspec.inspect.type_info` resolves a type once into a tree of type nodes holding wire names, defaults, tags, and expanded `Meta` bounds, and a walk over that tree derives a codec for any record. `leaves` lists every field path by wire name, and `overridden` sets text values at those paths before one lax `convert`:

```python
type KeyPath = tuple[str, ...]


def leaves(info: msgspec.inspect.Type, prefix: KeyPath = (), /) -> tuple[tuple[KeyPath, msgspec.inspect.Type], ...]:
    match info:
        case msgspec.inspect.StructType(fields=fields):
            return tuple(*leaves(field.type, (*prefix, field.encode_name)) for field in fields)
        case msgspec.inspect.Metadata(type=inner):
            return leaves(inner, prefix)
        case _:
            return ((prefix, info),)


def _placed(tree: object, path: KeyPath, value: str, /) -> object:
    match path, tree:
        case (head, *rest), dict():
            return tree | {head: _placed(tree.get(head), tuple(rest), value)}
        case (head, *rest), _:
            return {head: _placed(None, tuple(rest), value)}
        case _:
            return value


@catch(exception=msgspec.ValidationError)
def _converted[T: msgspec.Struct](record: T, pairs: Mapping[KeyPath, str], /) -> T:
    converted: T = msgspec.convert(reduce(lambda tree, pair: _placed(tree, *pair), pairs.items(), msgspec.to_builtins(record)), type(record), strict=False)
    return converted


def overridden[T: msgspec.Struct](record: T, pairs: Mapping[KeyPath, str], /) -> Outcome[T]:
    subject = type(record).__name__
    known = frozenset(path for path, _ in leaves(msgspec.inspect.type_info(type(record))))
    return (
        Ok(pairs)
        .filter_with(lambda held: held.keys() <= known, lambda held: Fault.of(Reason.OUTSIDE, subject, ", ".join(".".join(path) for path in held.keys() - known), member="its field paths"))
        .bind(lambda held: _converted(record, held).map_error(partial(Fault.of, Reason.INVALID, subject)))
    )
```

`{("store", "port"): "8"}` sets a nested `int` through `strict=False`, `"0"` fails the field's `Meta` at `$.store.port`, and a python name under `rename` fails as outside the paths:
- Bounds expand into their node (`IntType(ge=1, le=65535)`), and schema or `extra` keys wrap it in the `Metadata` node `leaves` unwraps
- Optional fields arrive as `UnionType` holding `NoneType()`, and a tagged union as `UnionType` of `StructType` nodes holding `tag_field` and `tag`
- `multi_type_info((A, B))` analyzes types together and shares one node per record

## [09]-[SCHEMAS]

`msgspec.json.schema(T)` and `schema_components(types, ref_template=)` emit JSON Schema 2020-12 from the declarations decode reads, and `schema_hook` supplies the node of each type `dec_hook` serves:

```python
class Host(msgspec.Struct, frozen=True, kw_only=True):
    address: IPv4Address
    port: Annotated[int, msgspec.Meta(ge=1, le=65535)]


(HOST,), COMPONENTS = msgspec.json.schema_components((Host,), schema_hook=schema, ref_template="#/components/schemas/{name}")
```

`HOST` is `{"$ref": "#/components/schemas/Host"}`, and `COMPONENTS` holds `Host` for an OpenAPI `components` table with `address` as an `ipv4` node and `port` with `minimum` and `maximum`:
- Wire names, `Meta` bounds, tags as one-value `enum`, defaults, and `required` follow the declaration, and docstrings become `description`
- Tagged unions emit `anyOf` with a `discriminator` mapping each tag to its case
- `__post_init__` rules and hook logic stay out of the schema
- Custom types without a `schema_hook` raise `TypeError`, and `Meta(extra_json_schema=...)` on an alias supplies one type's node

## [10]-[TYPING]

Decoders expand `type` aliases, PEP 695 parameters, and unquoted references to later classes at first use, and an unparametrized parameter decodes as its bound. Forms msgspec reads in part take a replacement:

| [INDEX] | [FORM]                      | [BEHAVIOR]                          | [REPLACEMENT]                    |
| :-----: | :-------------------------- | :---------------------------------- | :------------------------------- |
|  [01]   | PEP 696 default `[T = int]` | Unparametrized `T` decodes as `Any` | Parametrized target (`Box[int]`) |
|  [02]   | Recursive `type` alias      | `RecursionError` at type analysis   | Recursive struct field           |
|  [03]   | `ReadOnly` TypedDict item   | `TypeError` at type analysis        | Frozen struct                    |
|  [04]   | `TypedDict(closed=True)`    | Extra keys dropped                  | `forbid_unknown_fields=True`     |

---
name: python-coding-msgspec
description: "Use when declaring msgspec records or codecs, covering struct families, bounds as types, validation paths, hooks, and conversion"
---

# [PYTHON_CODING_MSGSPEC]

Covers msgspec records and codecs past python-coding's record rules, from a family's wire policy and bounds as types to validation paths, hooks, and conversion. Snippets build on python-coding's `Fault`, `Outcome`, and `Reason`.

## [01]-[FAMILIES]

Struct options are class keywords each subclass inherits, so one base states a family's wire policy and each case states its fields alone. `Event` fixes the tag key, derives each tag from its class name, renames fields to camel case, and refuses unknown keys for every case:

```python
class Event(msgspec.Struct, frozen=True, kw_only=True, forbid_unknown_fields=True, rename="camel", tag_field="kind", tag=str.lower): ...


class Opened(Event, frozen=True, kw_only=True):
    item_id: int


class Closed(Event, frozen=True, kw_only=True):
    item_id: int
    closed_by: str | None = None


type Change = Opened | Closed

CHANGES: msgspec.json.Decoder[tuple[Change, ...]] = msgspec.json.Decoder(tuple[Change, ...])
```

`CHANGES` decodes `{"kind": "closed", "itemId": 1}` as `Closed(item_id=1)`, and an unknown key fails at its path (`` Object contains unknown field `extra` - at `$[0]` ``). Families hold by these rules:
- `tag=True` and a tag callable read `__qualname__`, a nested case tags as `Outer.Inner`
- Bases holding `tag_field` are cases under their own tag, and the union alias lists the leaves alone
- Enum tags pass `<Member>.value`, a `StrEnum` member passes both checkers and raises `TypeError` at class build
- Decoders over one case accept a missing tag, and a union decoder requires it
- `__init_subclass__` runs before `__struct_fields__` exists, and a class keyword msgspec lacks raises `TypeError`
- Unions hold one dict-like type and one str-like type (`str`, enum, `datetime`, `UUID`, `Decimal`, `bytes`), `Literal["a"] | str` decodes

Options past `frozen` and `kw_only` buy a wire or memory property under a constraint:

| [INDEX] | [OPTION]                     | [EFFECT]                                  | [CONSTRAINT]                                               |
| :-----: | :--------------------------- | :---------------------------------------- | :--------------------------------------------------------- |
|  [01]   | `rename=`                    | Wire names from a convention or a mapping | `field(name=...)` wins for its field                       |
|  [02]   | `forbid_unknown_fields=True` | Decode refuses keys a record lacks        | Producers adding a field break every reader                |
|  [03]   | `omit_defaults=True`         | Encode drops fields at their default      | `default_factory` other than a builtin constructor encodes |
|  [04]   | `array_like=True`            | Positional array on the wire              | New fields append with defaults                            |
|  [05]   | `cache_hash=True`            | Hash computed once                        | Requires `frozen=True`                                     |
|  [06]   | `gc=False`                   | Collector skips instances with containers | Excludes `dict=True`, and a cycle among instances leaks    |
|  [07]   | `order=True`                 | Comparison by field tuple                 | Declaration order decides                                  |

## [02]-[BOUNDS]

Bounds are types: an `Annotated` alias states a domain once, and a generic record takes one bounded type argument per field. `Finite` and `Positive` derive their limits from `sys.float_info`, `Knot` takes a domain per axis, and `Form` holds a bound as data a call applies:

```python
type Finite = Annotated[float, msgspec.Meta(ge=-sys.float_info.max, le=sys.float_info.max)]
type Positive = Annotated[float, msgspec.Meta(ge=sys.float_info.min, le=sys.float_info.max)]
type Unit = Annotated[float, msgspec.Meta(ge=0.0, le=1.0)]


class Knot[X: float, Y: float](msgspec.Struct, frozen=True, kw_only=True, array_like=True):
    x: X
    y: Y


CURVE = msgspec.json.Decoder(tuple[Knot[Unit, Finite], ...])


class Form(msgspec.Struct, frozen=True, kw_only=True):
    template: str
    constraint: msgspec.Meta


@catch(exception=msgspec.ValidationError)
def _fitted(text: str, constraint: msgspec.Meta, /) -> str:
    fitted: str = msgspec.convert(text, Annotated[str, constraint])
    return fitted


def named(form: Form, key: str, /) -> Outcome[str]:
    return _fitted(name := form.template.format(key=key), form.constraint).map_error(
        lambda error: (Fault(reason=Reason.INVALID, subject=repr(name), cause=error),)
    )
```

`CURVE` refuses an `x` outside `Unit` at `$[0][0]` and encodes each knot as `[x, y]`. `Finite` refuses `nan` and `inf` from `convert`, and a JSON decode refuses an overflowing number on its own. `named` returns the formatted name or an `INVALID` fault holding `` Expected `str` of length <= <n> ``, and each row of a table keyed by kind states its own limit. Bounds hold by these rules:
- `gt=0.0` on a `float` stores a subnormal `ge` that Blender's build flushes to zero, admitting `0.0`, and `lt=0.0` admits it the same way
- `Positive` with `ge=sys.float_info.min` holds in every build, `le=sys.float_info.max` refuses `inf`, and no upper `lt` sits beside it
- Each side takes one bound across layers, a second raises `TypeError`, and one `Meta` holding `gt` and `ge` fails both checkers
- `pattern` searches anywhere in a value, `^` and `$` anchor it
- Numeric bounds apply to `int` and `float` alone, and a `Decimal` bound sits in `__post_init__`
- Bounds on a wrong type (`ge` on `str`) build the class and raise `TypeError` at the first decode or `convert`
- Decoders read `msgspec.Meta` alone, `annotated_types` and pydantic `Field` bounds pass every value, and pydantic ignores `Meta`

## [03]-[VALIDATION]

Records revalidate through `convert` over `to_builtins`, which applies every bound and `__post_init__` once. Patches tell an absent key from `null` through `UNSET`, `to_builtins` drops each unset field, and the merge keeps the draft's value for every absent key:

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
    return _patched(draft, patch).map_error(lambda error: (Fault(reason=Reason.INVALID, subject=Patch.__name__, cause=error),))
```

`{"note": null}` clears `note`, `{"title": "b"}` keeps it, and a `Patch(title="")` built in process fails the merge at `$.title`. In-process paths differ from decode:
- `copy.deepcopy` runs `__post_init__`, `copy.copy` skips it, and `convert(record, T)` returns `record` unchecked
- `structs.replace` takes `**changes: Any`, and a change of wrong type passes both checkers and the runtime
- `__post_init__` raising `TypeError` reports as a `ValidationError` like `ValueError`, and every other class propagates raw
- `to_builtins` writes `Decimal`, `datetime`, `UUID`, and `bytes` as text a strict `convert` reads back
- `is msgspec.UNSET` and `case msgspec.UNSET:` narrow a field for both checkers, and `structs.asdict` keeps `UNSET`

## [04]-[CODECS]

Format modules share one codec model, and options decide output bytes:
- `msgspec.yaml` needs PyYAML, and `msgspec.toml` decodes through `tomllib` and encodes through `tomli_w`
- `order="deterministic"` sorts dict keys and sets for byte-stable output, `"sorted"` sorts struct fields with them
- `encode_into(value, buffer, offset)` returns `None` and resizes `buffer` to end at its last written byte, `-1` appends
- `Sequence` and `Mapping` annotations decode to `list` and `dict`, and frozen records annotate `tuple[T, ...]`, `frozenset`, and `frozendict`
- `Decimal` encodes as text unless `decimal_format="number"`, and `Decoder(float_hook=Decimal)` reads untyped floats exactly
- `NaN` and infinities encode as `null`, which a `float` field refuses on decode
- Naive `datetime` encodes without an offset, aware UTC with `Z`, `timedelta` as an ISO duration, and `bytes` as base64
- `msgspec.Raw` defers a body a sibling field types, `convert` refuses it, and `Raw.copy()` detaches it from its source buffer
- `Decoder(A | B)` types as `Decoder[Any]`, and an annotation on the codec restores the union

## [05]-[HOOKS]

`enc_hook` receives each value msgspec cannot encode and `dec_hook` each annotation it cannot decode, and both raise `NotImplementedError` for a type they lack. One table keyed by type serves both hooks, and a type with a text form pairs `str` with its constructor:

```python
class Shape[T](msgspec.Struct, frozen=True, kw_only=True):
    encoded: Callable[[T], object]
    decoded: Callable[[Any], T]


SHAPES: frozendict[type, Shape[Any]] = frozendict({kind: Shape(encoded=str, decoded=kind) for kind in (Path, IPv4Address)})


def encoded(value: object, /) -> object:
    if (shape := next((SHAPES[base] for base in type(value).__mro__ if base in SHAPES), None)) is None:
        raise NotImplementedError(type(value))
    return shape.encoded(value)


def decoded(kind: type, raw: object, /) -> object:
    if (shape := SHAPES.get(kind)) is None:
        raise NotImplementedError(kind)
    return shape.decoded(raw)


ENCODER = msgspec.json.Encoder(enc_hook=encoded)
```

`msgspec.json.Decoder(<Record>, dec_hook=decoded)` reads every `Path` and `IPv4Address` field, and a new type adds one row. Hooks see these values:
- `enc_hook` receives a runtime type (`PosixPath` for `Path`) and walks `__mro__`, `dec_hook` receives the annotation itself
- Constructor raises of `ValueError` and `TypeError` report at their path (`` Octet 256 (> 255) not permitted in '10.0.0.256' - at `$.host` ``)
- `NotImplementedError` and every other class propagate raw, and a `ValidationError` from a `convert` inside `dec_hook` loses its path
- Natively decoded types (`Decimal`, `datetime`, `UUID`) never reach `dec_hook`
- Custom types share a union with `None` alone, and `msgspec.json.schema` over one needs `schema_hook`

## [06]-[CONVERSION]

`convert` reads parsed builtins strictly by default, and `strict=False` coerces text sources:

| [INDEX] | [INPUT]                     | [TARGET]                | [STRICT] | [LAX]    |
| :-----: | :-------------------------- | :---------------------- | :------- | :------- |
|  [01]   | `"1"`, `"1.0"`              | `int`                   | Refused  | `1`      |
|  [02]   | `1.0`                       | `int`                   | Refused  | `1`      |
|  [03]   | `"true"`                    | `bool`                  | Refused  | `True`   |
|  [04]   | `1`                         | `float`                 | `1.0`    | `1.0`    |
|  [05]   | ISO text, UUID text, base64 | `date`, `UUID`, `bytes` | Accepted | Accepted |
|  [06]   | `1`                         | `str`                   | Refused  | Refused  |
|  [07]   | `"1.5"`, `1.5`              | `int`                   | Refused  | Refused  |

- Mapping input reads wire names, and a python name under `rename` fails as a missing field
- `from_attributes=True` reads python names through nested objects
- `str_keys=True` decodes text keys into `int` or `UUID` keys, and `builtin_types` passes listed native values through
- `to_builtins` applies renames, tags, omitted defaults, and `UNSET`, and `structs.asdict` copies python names one level deep

## [07]-[TYPING]

Decoders expand `type` aliases, PEP 695 parameters, and unquoted references to later classes at first use, a bound standing in for an unparametrized parameter. Newer forms decode by these rules:

| [INDEX] | [FORM]                      | [BEHAVIOR]                          | [REPLACEMENT]                    |
| :-----: | :-------------------------- | :---------------------------------- | :------------------------------- |
|  [01]   | PEP 696 default `[T = int]` | Unparametrized `T` decodes as `Any` | Parametrized target (`Box[int]`) |
|  [02]   | Recursive `type` alias      | `RecursionError` at type analysis   | Recursive struct field           |
|  [03]   | `ReadOnly` TypedDict item   | `TypeError` at type analysis        | Frozen struct                    |
|  [04]   | `TypedDict(closed=True)`    | Extra keys dropped                  | `forbid_unknown_fields=True`     |

# [MESSAGEPACK]

Covers MessagePack extension types for out-of-band references and the values MessagePack encodes apart from JSON.

## [01]-[REFERENCES]

Records holding live handles (host objects, open resources) encode each handle as an extension value naming its index in a side table, and decode resolves the index against the table. `linked` builds one entry per distinct handle during one encode and returns the table as a tuple beside the bytes:

```python
class Linked[H: Hashable](msgspec.Struct, frozen=True, kw_only=True):
    code: ClassVar[int] = 1
    data: bytes
    handles: tuple[H, ...]


def _referenced[H: Hashable](kind: type[H], table: dict[H, int], value: object, /) -> msgspec.msgpack.Ext:
    if not isinstance(value, kind):
        raise NotImplementedError(type(value))
    return msgspec.msgpack.Ext(Linked.code, table.setdefault(value, len(table)).to_bytes(4))


def linked[H: Hashable](kind: type[H], record: msgspec.Struct, /) -> Linked[H]:
    table: dict[H, int] = {}
    data = msgspec.msgpack.encode(record, enc_hook=partial(_referenced, kind, table), order="deterministic")
    return Linked(data=data, handles=tuple(table))


def _resolved[H: Hashable](handles: tuple[H, ...], code: int, data: memoryview, /) -> H | msgspec.msgpack.Ext:
    return handles[int.from_bytes(data)] if code == Linked.code else msgspec.msgpack.Ext(code, bytes(data))


@catch(exception=msgspec.DecodeError)
def _unlinked[T, H: Hashable](form: type[T], held: Linked[H], /) -> T:
    record: T = msgspec.msgpack.decode(held.data, type=form, ext_hook=partial(_resolved, held.handles))
    return record


def unlinked[T, H: Hashable](form: type[T], held: Linked[H], /) -> Outcome[T]:
    return _unlinked(form, held).map_error(partial(Fault.of, Reason.INVALID, form.__name__))
```

`unlinked(<Record>, linked(<Handle>, record))` returns a record holding the same handle objects, and a second table of equal length rebinds every field to its handles:
- Codes 0 to 127 belong to applications, and negative codes to MessagePack (`-1` for timestamps)
- Fields typed as the handle class refuse an `ext_hook` result of another class at their path with no `dec_hook`
- `ext_hook` raises propagate raw, and a table shorter than its indexes raises `IndexError` past `catch`
- Codes other than `Linked.code` decode as `msgspec.msgpack.Ext` values, as in a decode with no `ext_hook`

## [02]-[ENCODING]

MessagePack writes integers, datetimes, and non-finite floats apart from JSON:
- Integers outside `-2**63` to `2**64 - 1` raise `OverflowError`, and JSON writes them whole
- Aware `datetime` encodes as the timestamp extension, and naive `datetime` as ISO text
- `NaN` and infinities encode as floats and decode back unchanged

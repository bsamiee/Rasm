# [MESSAGEPACK]

## [01]-[EXTENSIONS]

Records holding live handles (host objects, open resources) encode each as an extension value naming its side-table index, and decode resolves it. `linked` adds one entry per distinct handle during one encode and returns the table as a tuple beside the bytes:

```python
LINK = 1


def linked[H: Hashable](kind: type[H], record: msgspec.Struct, /) -> tuple[bytes, tuple[H, ...]]:
    table: dict[H, int] = {}

    def referenced(value: object, /) -> msgspec.msgpack.Ext:
        if not isinstance(value, kind):
            raise NotImplementedError(type(value))
        return msgspec.msgpack.Ext(LINK, table.setdefault(value, len(table)).to_bytes(4))

    return msgspec.msgpack.encode(record, enc_hook=referenced, order="deterministic"), tuple(table)


def resolved[H: Hashable](handles: tuple[H, ...], code: int, data: memoryview, /) -> H | msgspec.msgpack.Ext:
    return handles[int.from_bytes(data)] if code == LINK else msgspec.msgpack.Ext(code, bytes(data))
```

On `linked` output, `msgspec.msgpack.decode(data, type=<Record>, ext_hook=partial(resolved, handles))` returns a record holding the same handles. A second table of equal length rebinds every field to its handles:
- Codes 0 to 127 belong to applications, and negative codes to MessagePack (`-1` for timestamps)
- Fields typed as the handle class refuse an `ext_hook` result of another class at their path (`` Expected `Handle`, got `Other` - at `$.handle` ``)
- `ext_hook` raises propagate raw, and a table shorter than its indexes raises `IndexError` past a `DecodeError` catch
- Codes other than `LINK` decode as `msgspec.msgpack.Ext` values, as in a decode with no `ext_hook`

## [02]-[ENCODING]

MessagePack differs from JSON on integers, binary, datetimes, UUIDs, and non-finite floats:
- Integers outside `-2**63` to `2**64 - 1` raise `OverflowError`, and JSON writes them whole
- `bytes` fields refuse `str` items, and a `memoryview` target views `bin` data in the input buffer, which the view keeps alive
- Aware `datetime` encodes as the timestamp extension, and naive `datetime` as ISO text
- `Encoder(uuid_format="bytes")` writes a `UUID` as its 16 bytes, an option the JSON encoder lacks
- `NaN` and infinities encode as floats and decode back unchanged

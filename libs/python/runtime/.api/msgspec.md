# [PY_RUNTIME_API_MSGSPEC]

`msgspec` supplies the runtime's in-memory frame layer: `Struct`, a C-extension record type with zero-copy JSON/MessagePack encode/decode, `Annotated`-constraint validation, and the `to_builtins`/`convert` lowering/raising pair that bridges the canonical `Struct` shapes interior code holds to the wire mappings the `transport/wire` codec and the `transport/serve` capability decode project. It is the canonical-shape owner the wire codec, the CRDT op-log decode, the capability invoke, the admission settings, and the metric/clock receipts compose; it never hand-rolls JSON validation or a parallel DTO type for the same wire shape.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `msgspec`
- package: `msgspec`
- import: `msgspec`
- owner: `runtime`
- rail: serialization
- namespaces: `msgspec`, `msgspec.json`, `msgspec.msgpack`, `msgspec.structs`, `msgspec.inspect`
- installed: `0.21.1`; license MIT; cp315-CLEAN, core-direct (no environment marker)
- capability: `Struct` record definition, JSON/MessagePack encode-decode, `Annotated`-constraint validation, builtins lowering/duck-typed conversion, struct introspection

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core record type
- rail: serialization

| [INDEX] | [SYMBOL] | [TYPE_FAMILY]    | [RAIL]                        |
| :-----: | :------- | :--------------- | :---------------------------- |
|  [01]   | `Struct` | base class       | typed serializable record     |
|  [02]   | `Raw`    | bytes wrapper    | deferred/opaque encoded value |
|  [03]   | `Meta`   | constraint class | `Annotated` field constraint  |

[PUBLIC_TYPE_SCOPE]: error types
- rail: serialization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [RAIL]                             |
| :-----: | :---------------- | :-------------- | :--------------------------------- |
|  [01]   | `MsgspecError`    | base exception  | all msgspec exceptions root        |
|  [02]   | `DecodeError`     | decode failure  | malformed or type-mismatch input   |
|  [03]   | `ValidationError` | validation fail | constraint violation during decode |

[PUBLIC_TYPE_SCOPE]: codec types
- rail: serialization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [RAIL]                             |
| :-----: | :---------------- | :------------- | :--------------------------------- |
|  [01]   | `json.Decoder`    | stateful codec | reusable typed JSON decoder        |
|  [02]   | `msgpack.Decoder` | stateful codec | reusable typed MessagePack decoder |
|  [03]   | `msgpack.Ext`     | extension type | MessagePack extension payload      |

[PUBLIC_TYPE_SCOPE]: struct introspection
- rail: serialization

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                                        |
| :-----: | :------------------------- | :------------ | :------------------------------------------------------------ |
|  [01]   | `structs.StructConfig`     | config record | per-class struct configuration                                |
|  [02]   | `Struct.__struct_config__` | config handle | per-class `StructConfig` (`tag`, `tag_field`, `frozen`, `gc`) |
|  [03]   | `Struct.__struct_fields__` | name tuple    | declared field names in declaration order                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level encode/decode/convert
- rail: serialization

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `msgspec.json.encode(obj, *, enc_hook, order)`           | encode         | object to JSON bytes            |
|  [02]   | `msgspec.json.decode(buf, *, type, strict, dec_hook)`    | decode         | JSON bytes to typed object      |
|  [03]   | `msgspec.msgpack.encode(obj, *, enc_hook)`               | encode         | object to msgpack bytes         |
|  [04]   | `msgspec.msgpack.decode(buf, *, type, strict, dec_hook)` | decode         | msgpack bytes to typed object   |
|  [05]   | `msgspec.convert(obj, type, *, strict, from_attributes)` | conversion     | duck-typed object coercion      |
|  [06]   | `msgspec.to_builtins(obj, *, str_keys, enc_hook)`        | projection     | struct to plain Python builtins |
|  [07]   | `msgspec.field(*, default, default_factory, name)`       | field factory  | struct field with metadata      |

[ENTRYPOINT_SCOPE]: stateful codecs
- rail: serialization

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [RAIL]                         |
| :-----: | :------------------------------------------------ | :------------- | :----------------------------- |
|  [01]   | `json.Encoder(*, enc_hook, order)`                | codec          | reusable JSON encoder          |
|  [02]   | `json.Decoder(type, *, strict, dec_hook)`         | codec          | reusable typed JSON decoder    |
|  [03]   | `json.schema(type, *, schema_hook, ref_template)` | schema gen     | JSON Schema dict for a type    |
|  [06]   | `msgpack.Encoder(*, enc_hook)`                    | codec          | reusable msgpack encoder       |
|  [07]   | `msgpack.Decoder(type, *, strict, dec_hook)`      | codec          | reusable typed msgpack decoder |

[ENTRYPOINT_SCOPE]: struct utilities
- rail: serialization

| [INDEX] | [SURFACE]                            | [ENTRY_FAMILY] | [RAIL]                      |
| :-----: | :----------------------------------- | :------------- | :-------------------------- |
|  [01]   | `structs.asdict(struct)`             | projection     | struct to dict              |
|  [02]   | `structs.astuple(struct)`            | projection     | struct to tuple             |
|  [03]   | `structs.replace(struct, **changes)` | copy-with      | struct with field overrides |

## [04]-[IMPLEMENTATION_LAW]

[MSGSPEC_TOPOLOGY]:
- namespaces: `msgspec` (core), `msgspec.json`, `msgspec.msgpack`, `msgspec.structs`, `msgspec.inspect`
- `Struct` is a C-extension class; field types are resolved at class creation time, not at decode time
- `Struct` subclass keywords configure the record: `frozen`, `tag`/`tag_field` (tagged-union discriminant), `array_like`, `omit_defaults`, `rename`, `forbid_unknown_fields`, and `gc` — `gc=False` opts a leaf struct holding only non-container fields out of the cyclic garbage collector's tracked set, removing per-instance GC overhead on high-allocation paths; each keyword surfaces on the per-class `structs.StructConfig`
- `to_builtins` lowers a `Struct` to a JSON-compatible mapping of plain builtins and `convert` raises an unvalidated mapping back into a typed `Struct`; the pair is the canonical lowering/raising bridge to and from the wire-codec mapping
- `msgpack.Decoder(type)` decodes a tagged-union `Struct` (`tag_field`/`array_like=True`, integer `tag=n`) from the msgpack tag-keyed array envelope position-for-position
- `json.Decoder`/`json.Decoder` instances are reusable; prefer them over per-call `decode` in hot paths

[LOCAL_ADMISSION]:
- the runtime defines every canonical wire/frame shape as a `Struct` subclass; `transport/wire` `WireProtoCodec` composes `to_builtins`/`convert` across the protobuf seam and `transport/wire` `CrdtOpDecode` composes `msgpack.Decoder` over the op-log delta
- catch `DecodeError`/`ValidationError` only inside the `reliability/faults#FAULT` `boundary` conversion, never in domain flow
- `gc=False` is applied only to leaf cells (`clock#CLOCK` `Hlc`/`ElementId`) holding no container field

[RAIL_LAW]:
- Package: `msgspec`
- Owns: `Struct` definition, JSON/MessagePack encode-decode, validation, builtins lowering/conversion, struct introspection
- Accept: `Struct` subclasses, `to_builtins`/`convert` boundary bridge, `msgpack.Decoder`/`json.Decoder` reusable codecs, `Meta` constraints
- Reject: hand-rolled JSON validation, manual isinstance guards replacing `convert`, separate DTO types for the same wire shape

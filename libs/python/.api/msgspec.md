# [PY_BRANCH_API_MSGSPEC]

`msgspec` owns wire serialization and decode-time validation on `Struct`, a C-extension record whose field schema resolves at class creation. It binds zero-copy JSON and MessagePack codecs, `Annotated[T, Meta(...)]` field constraints validated during decode, runtime struct construction via `defstruct`, `msgspec.inspect` type-node introspection, and JSON Schema emission from Python types. It is the serialization rail's sole wire-model owner — every cross-boundary payload mints as a `Struct`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `msgspec`
- package: `msgspec` (BSD-3-Clause)
- module: `msgspec`
- namespaces: `msgspec`, `msgspec.json`, `msgspec.msgpack`, `msgspec.toml`, `msgspec.yaml`, `msgspec.structs`, `msgspec.inspect`
- abi: C-extension `_core` (compiled, not pure-Python)
- rail: serialization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core record type

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]    | [CAPABILITY]                                                      |
| :-----: | :----------- | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `Struct`     | base class       | typed serializable record; subclass kwargs drive wire behaviour   |
|  [02]   | `StructMeta` | metaclass        | struct class construction; resolves field types at class creation |
|  [03]   | `Raw`        | bytes wrapper    | deferred/opaque encoded value; defers sub-tree decode             |
|  [04]   | `UnsetType`  | sentinel type    | type of singleton `UNSET`; absent-field marker on optional fields |
|  [05]   | `Meta`       | constraint class | `Annotated[T, Meta(...)]` field constraint + schema metadata      |
|  [06]   | `UNSET`      | sentinel value   | the `UnsetType` singleton; field value meaning "client omitted"   |
|  [07]   | `NODEFAULT`  | sentinel value   | distinct singleton meaning "field has no default" (not `UNSET`)   |

[PUBLIC_TYPE_SCOPE]: error types

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [CAPABILITY]                       |
| :-----: | :---------------- | :-------------- | :--------------------------------- |
|  [01]   | `MsgspecError`    | base exception  | all msgspec exceptions root        |
|  [02]   | `DecodeError`     | decode failure  | malformed or type-mismatch input   |
|  [03]   | `EncodeError`     | encode failure  | unencodable value                  |
|  [04]   | `ValidationError` | validation fail | constraint violation during decode |

[PUBLIC_TYPE_SCOPE]: JSON codec

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]  | [CAPABILITY]                                                   |
| :-----: | :------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `json.Encoder` | stateful codec | reusable JSON encoder; `encode_into` writes into a `bytearray` |
|  [02]   | `json.Decoder` | stateful codec | reusable typed JSON decoder; `decode_lines` for NDJSON streams |

[PUBLIC_TYPE_SCOPE]: MessagePack codec

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [CAPABILITY]                       |
| :-----: | :---------------- | :------------- | :--------------------------------- |
|  [01]   | `msgpack.Encoder` | stateful codec | reusable MessagePack encoder       |
|  [02]   | `msgpack.Decoder` | stateful codec | reusable typed MessagePack decoder |
|  [03]   | `msgpack.Ext`     | extension type | MessagePack extension payload      |

[PUBLIC_TYPE_SCOPE]: struct introspection

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [CAPABILITY]                                                  |
| :-----: | :------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `structs.FieldInfo`        | field metadata | name, encode_name, type, default                              |
|  [02]   | `structs.StructConfig`     | config record  | struct class configuration                                    |
|  [03]   | `Struct.__struct_config__` | config handle  | per-class `StructConfig` (`tag`, `tag_field`, `frozen`, `gc`) |
|  [04]   | `Struct.__struct_fields__` | name tuple     | declared field names in declaration order                     |

[PUBLIC_TYPE_SCOPE]: inspect type nodes (selection)

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY]  | [CAPABILITY]                                                       |
| :-----: | :----------------------- | :------------- | :----------------------------------------------------------------- |
|  [01]   | `inspect.Type`           | type node base | abstract base of all type nodes                                    |
|  [02]   | `inspect.StructType`     | type node      | struct type descriptor                                             |
|  [03]   | `inspect.Field`          | field node     | `name`/`encode_name`/`type`/`required`/`default`/`default_factory` |
|  [04]   | `inspect.Metadata`       | meta node      | annotated metadata descriptor                                      |
|  [05]   | `inspect.UnionType`      | type node      | union descriptor; `tagged`/`tag_field` discriminant metadata       |
|  [06]   | `inspect.ListType`       | type node      | list type descriptor                                               |
|  [07]   | `inspect.DictType`       | type node      | dict type descriptor                                               |
|  [08]   | `inspect.EnumType`       | type node      | enum type descriptor                                               |
|  [09]   | `inspect.CustomType`     | type node      | custom enc/dec hook type                                           |
|  [10]   | `inspect.TypedDictType`  | type node      | `TypedDict` schema descriptor                                      |
|  [11]   | `inspect.NamedTupleType` | type node      | `NamedTuple` schema descriptor                                     |
|  [12]   | `inspect.DateTimeType`   | type node      | datetime descriptor with `tz` constraint flag                      |
|  [13]   | `inspect.RawType`        | type node      | `Raw` deferred-payload descriptor                                  |
|  [14]   | `inspect.IntType`        | scalar node    | int descriptor; `gt`/`ge`/`lt`/`le`/`multiple_of` bounds           |
|  [15]   | `inspect.FloatType`      | scalar node    | float descriptor; `gt`/`ge`/`lt`/`le`/`multiple_of` bounds         |
|  [16]   | `inspect.StrType`        | scalar node    | str descriptor; `min_length`/`max_length`/`pattern`                |
|  [17]   | `inspect.BytesType`      | scalar node    | bytes descriptor; `min_length`/`max_length`                        |
|  [18]   | `inspect.BoolType`       | scalar node    | bool descriptor                                                    |
|  [19]   | `inspect.NoneType`       | scalar node    | none/null descriptor                                               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level encode/decode/convert

| [INDEX] | [SURFACE]                                                | [SHAPE] | [CAPABILITY]                                       |
| :-----: | :------------------------------------------------------- | :------ | :------------------------------------------------- |
|  [01]   | `msgspec.json.encode(obj, *, enc_hook, order)`           | static  | object to JSON bytes                               |
|  [02]   | `msgspec.json.decode(buf, *, type, strict, dec_hook)`    | static  | JSON bytes to typed object                         |
|  [03]   | `msgspec.msgpack.encode(obj, *, enc_hook)`               | static  | object to msgpack bytes                            |
|  [04]   | `msgspec.msgpack.decode(buf, *, type, strict, dec_hook)` | static  | msgpack bytes to typed object                      |
|  [05]   | `msgspec.convert(obj, type, *, ...)`                     | static  | duck-typed object coercion with validation         |
|  [06]   | `msgspec.to_builtins(obj, *, ...)`                       | static  | struct to JSON-safe Python builtins                |
|  [07]   | `msgspec.field(*, default, default_factory, name)`       | factory | struct field with metadata                         |
|  [08]   | `msgspec.defstruct(name, fields, *, ...)`                | factory | create Struct subclass at runtime with full config |
|  [09]   | `msgspec.Meta(*, ...)`                                   | ctor    | `Annotated` field constraint                       |

Constructor signatures the table abbreviates as `...`; the numeric and non-numeric `Meta` families are mutually exclusive per `[TOPOLOGY]`:
- [05]-[CONVERT]: `msgspec.convert(obj, type, *, strict, from_attributes, dec_hook, builtin_types, str_keys)`
- [06]-[TO_BUILTINS]: `msgspec.to_builtins(obj, *, str_keys, builtin_types, enc_hook, order)`
- [08]-[DEFSTRUCT]: `msgspec.defstruct(name, fields, *, bases, module, namespace, tag, tag_field, rename, omit_defaults, forbid_unknown_fields, frozen, eq, order, kw_only, repr_omit_defaults, array_like, gc, weakref, dict, cache_hash)`
- [09]-[META]: `msgspec.Meta(*, gt, ge, lt, le, multiple_of, pattern, min_length, max_length, tz, title, description, examples, extra_json_schema, extra)`

[ENTRYPOINT_SCOPE]: stateful codecs

| [INDEX] | [SURFACE]                                                     | [SHAPE]  | [CAPABILITY]                                    |
| :-----: | :------------------------------------------------------------ | :------- | :---------------------------------------------- |
|  [01]   | `json.Encoder(...)`                                           | ctor     | reusable encoder instance                       |
|  [02]   | `json.Decoder(type, *, strict, dec_hook, float_hook)`         | ctor     | reusable typed decoder instance                 |
|  [03]   | `json.Encoder.encode_into(obj, buffer, offset)`               | instance | encode into a caller `bytearray`, no alloc      |
|  [04]   | `json.Decoder.decode_lines(buf)`                              | instance | decode newline-delimited JSON to `list[type]`   |
|  [05]   | `json.format(buf, *, indent)`                                 | static   | pretty-print JSON bytes                         |
|  [06]   | `json.schema(type, *, schema_hook, ref_template)`             | static   | JSON Schema dict for a type                     |
|  [07]   | `json.schema_components(types, *, schema_hook, ref_template)` | static   | component schemas + `$defs`                     |
|  [08]   | `msgpack.Encoder(...)`                                        | ctor     | reusable msgpack encoder                        |
|  [09]   | `msgpack.Decoder(type, *, strict, dec_hook, ext_hook)`        | ctor     | typed msgpack decoder; `ext_hook` decodes `Ext` |
|  [10]   | `msgpack.Encoder.encode_into(obj, buffer, offset)`            | instance | encode msgpack into a caller `bytearray`        |

- `Encoder(order="deterministic")`: raises `TypeError` on any non-`str`-keyed dict; `frozenset`/`ndarray` values reach `enc_hook`, never native encoding.
- `enc_hook`: fires only for types the codec lacks natively — a `Struct` always encodes natively (a `Struct`-keyed hook is dead), and a native int outside `[-2**63, 2**64-1]` raises `OverflowError` before the hook, so a u128-bearing key projects or nulls before the msgpack preimage.

[ENTRYPOINT_SCOPE]: struct utilities

| [INDEX] | [SURFACE]                                    | [SHAPE] | [CAPABILITY]                |
| :-----: | :------------------------------------------- | :------ | :-------------------------- |
|  [01]   | `structs.fields(type_or_instance)`           | static  | tuple of FieldInfo          |
|  [02]   | `structs.asdict(struct)`                     | static  | struct to dict              |
|  [03]   | `structs.astuple(struct)`                    | static  | struct to tuple             |
|  [04]   | `structs.replace(struct, **changes)`         | static  | struct with field overrides |
|  [05]   | `structs.force_setattr(struct, name, value)` | static  | bypass frozen struct guard  |

[ENTRYPOINT_SCOPE]: inspect introspection

| [INDEX] | [SURFACE]                        | [SHAPE] | [CAPABILITY]                       |
| :-----: | :------------------------------- | :------ | :--------------------------------- |
|  [01]   | `inspect.type_info(type)`        | static  | type node tree for a single type   |
|  [02]   | `inspect.multi_type_info(types)` | static  | type node trees for multiple types |
|  [03]   | `inspect.is_struct(obj)`         | static  | True if instance is a Struct       |
|  [04]   | `inspect.is_struct_type(type)`   | static  | True if type is a Struct subclass  |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `Struct` is a C-extension class; field types resolve at class creation, never at decode, so a malformed annotation fails at subclass definition rather than on first payload.
- `Meta` numeric bounds (`gt`/`ge`/`lt`/`le`/`multiple_of`) and non-numeric bounds (`pattern`/`min_length`/`max_length`/`tz`) cannot coexist on one instance; `title`/`description`/`examples`/`extra_json_schema` feed `json.schema` output.
- integer `Meta` bounds must fit int64: a `gt`/`ge`/`lt`/`le` past `2**63 - 1` raises `ValueError` at constraint build, so a full-`uint64` slot carries the `ge=0` floor alone and enforces its ceiling in the producer domain.
- `UNSET` (an `UnsetType` singleton) and `NODEFAULT` are distinct singletons (`UNSET is NODEFAULT` is `False`): a field typed `T | UnsetType = UNSET` models tri-state presence and round-trips as absent under `omit_defaults`, while `NODEFAULT` surfaces only in `FieldInfo.default`.
- `gc=False` drops a leaf struct holding only non-container fields from the cyclic GC set, removing per-instance tracking overhead on high-allocation paths; each subclass keyword surfaces on the per-class `structs.StructConfig`.
- `Struct.__struct_config__` recovers the tagged-union discriminant off an instance with no `match` (`.tag` value, `.tag_field` key); `Struct.__struct_fields__` is the declaration-order name tuple, and `structs.fields` returns the richer `FieldInfo` tuple.

[STACKING]:
- `pydantic`(`.api/pydantic.md`): a `Struct` tagged union is the wire discriminant, `to_builtins` projects it to builtins feeding `TypeAdapter.validate_python`, and `Decoder(dec_hook=...)` resolves the shared custom scalars — one discriminant vocabulary spans both validators, no parallel DTO.
- `opentelemetry-api`(`.api/opentelemetry-api.md`): `to_builtins(struct, str_keys=True)` yields the `str | bool | int | float | Sequence[...]` builtins `Span.set_attributes` accepts directly, so a decoded wire struct flows into a trace attribute map with no hand-rolled flattener.
- `grpcio`(`.api/grpcio.md`): a `Struct` is the application message decoded from the gRPC `bytes` body — `Decoder(type=T).decode` validates at the servicer boundary and `Encoder.encode_into` writes the response into a reused buffer, keeping the wire and the domain record on one `Struct`.
- `numpy`(`.api/numpy.md`): a `Raw`-typed field defers a numeric block so `numpy.asarray` reconstructs only when the consumer needs the buffer, never eagerly decoding a large numeric sub-tree.
- `protobuf`(`.api/protobuf.md`): the `transport/wire#WireProtoCodec` encode leg lowers a `Struct` via `to_builtins(struct, order="deterministic")` into the mapping `json_format.ParseDict` raises into the `*_pb2` message; decode runs `MessageToDict(preserving_proto_field_name=True)` then `convert(dict, StructType, strict=False)`, lax `convert` coercing the widened proto3 scalars back — the one departure from the strict default.
- transport/wire: `CrdtOpDecode` caches one `msgpack.Decoder` whose `ext_hook` reconstructs the binary lattice clock from a `msgspec.Ext` payload; a `Raw`-typed envelope field defers the inner op decode until the tag routes it, and `Raw.copy()` detaches the buffer view so the outer input releases.

[LOCAL_ADMISSION]:
- Wire models are `Struct` subclasses; field constraints ride `Annotated[T, Meta(...)]`, and `gc=False` marks leaf structs holding only non-container fields.
- `msgspec.convert` coerces unvalidated dicts/objects into typed `Struct` at boundary intake — `from_attributes=True` ingests ORM/attribute objects, `dec_hook` reconstructs custom scalars.
- `DecodeError`/`ValidationError`/`EncodeError` catch only at I/O boundaries and map to domain error types; `ValidationError` carries the JSON-pointer path of the offending field and is terminal, never retried.
- runtime wire shapes mint once in `transport/shapes` with `frozen=True` the default, `structs.force_setattr` reserved for the decode-time post-init hook; the `clock#CLOCK` `Hlc`/`ElementId` leaf cells carry `gc=False`, and `faults#FAULT` `boundary` lifts a caught fault to a `BoundaryFault` once at egress.
- `json.schema`/`json.schema_components` emit JSON Schema from `Struct` types for OpenAPI/contract surfaces, `schema_hook` covering custom-typed fields.

[RAIL_LAW]:
- Package: `msgspec`
- Owns: `Struct` definition, JSON/MessagePack encode-decode, decode-time validation, schema generation, struct introspection
- Accept: `Struct`, `msgspec.json`/`msgspec.msgpack` `encode`/`decode`, `convert`, `Meta`, `defstruct`
- Reject: hand-rolled JSON validation, `isinstance` guards replacing `convert`, a separate DTO type for a wire shape a `Struct` already owns

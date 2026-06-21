# [PY_BRANCH_API_MSGSPEC]

`msgspec` supplies a high-performance serialization and validation library built on `Struct`, a C-extension record type. It provides zero-copy JSON/MessagePack encode/decode, `Annotated`-constraint validation on `Struct` fields, dynamic struct construction via `defstruct`, type-level schema introspection through `msgspec.inspect`, and JSON Schema generation from Python types.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `msgspec`
- package: `msgspec`
- module: `msgspec`
- version: `0.21.1` (floor `>=0.21.1`)
- license: `BSD-3-Clause`
- asset: C-extension runtime library (`_core` compiled module; not pure-Python)
- abi: per-interpreter binary wheel (`cp315`-tagged), `Requires-Python >=3.10`
- rail: serialization
- namespaces: `msgspec`, `msgspec.json`, `msgspec.msgpack`, `msgspec.toml`, `msgspec.yaml`, `msgspec.structs`, `msgspec.inspect`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core record type
- rail: serialization

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]    | [RAIL]                                                            |
| :-----: | :------------ | :--------------- | :---------------------------------------------------------------- |
|  [01]   | `Struct`      | base class       | typed serializable record; subclass kwargs drive wire behaviour   |
|  [02]   | `StructMeta`  | metaclass        | struct class construction; resolves field types at class creation |
|  [03]   | `Raw`         | bytes wrapper    | deferred/opaque encoded value; defers sub-tree decode             |
|  [04]   | `UnsetType`   | sentinel type    | type of singleton `UNSET`; absent-field marker on optional fields |
|  [05]   | `Meta`        | constraint class | `Annotated[T, Meta(...)]` field constraint + schema metadata      |
|  [06]   | `UNSET`       | sentinel value   | the `UnsetType` singleton; field value meaning "client omitted"   |
|  [07]   | `NODEFAULT`   | sentinel value   | distinct singleton meaning "field has no default" (not `UNSET`)   |

[PUBLIC_TYPE_SCOPE]: error types
- rail: serialization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [RAIL]                             |
| :-----: | :---------------- | :-------------- | :--------------------------------- |
|  [01]   | `MsgspecError`    | base exception  | all msgspec exceptions root        |
|  [02]   | `DecodeError`     | decode failure  | malformed or type-mismatch input   |
|  [03]   | `EncodeError`     | encode failure  | unencodable value                  |
|  [04]   | `ValidationError` | validation fail | constraint violation during decode |

[PUBLIC_TYPE_SCOPE]: JSON codec
- rail: serialization

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]  | [RAIL]                                                          |
| :-----: | :------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `json.Encoder` | stateful codec | reusable JSON encoder; `encode_into` writes into a `bytearray` |
|  [02]   | `json.Decoder` | stateful codec | reusable typed JSON decoder; `decode_lines` for NDJSON streams |

[PUBLIC_TYPE_SCOPE]: MessagePack codec
- rail: serialization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [RAIL]                             |
| :-----: | :---------------- | :------------- | :--------------------------------- |
|  [01]   | `msgpack.Encoder` | stateful codec | reusable MessagePack encoder       |
|  [02]   | `msgpack.Decoder` | stateful codec | reusable typed MessagePack decoder |
|  [03]   | `msgpack.Ext`     | extension type | MessagePack extension payload      |

[PUBLIC_TYPE_SCOPE]: struct introspection
- rail: serialization

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY]  | [RAIL]                                                        |
| :-----: | :------------------------- | :------------- | :------------------------------------------------------------ |
|  [01]   | `structs.FieldInfo`        | field metadata | name, encode_name, type, default                              |
|  [02]   | `structs.StructConfig`     | config record  | struct class configuration                                    |
|  [03]   | `Struct.__struct_config__` | config handle  | per-class `StructConfig` (`tag`, `tag_field`, `frozen`, `gc`) |
|  [04]   | `Struct.__struct_fields__` | name tuple     | declared field names in declaration order                     |

[PUBLIC_TYPE_SCOPE]: inspect type nodes (selection)
- rail: serialization

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY]  | [RAIL]                          |
| :-----: | :------------------- | :------------- | :------------------------------ |
|  [00]   | `inspect.Type`       | type node base | abstract base of all type nodes |
|  [01]   | `inspect.StructType` | type node      | struct type descriptor          |
|  [02]   | `inspect.Field`      | field node     | struct field descriptor         |
|  [03]   | `inspect.Metadata`   | meta node      | annotated metadata descriptor   |
|  [04]   | `inspect.UnionType`  | type node      | union descriptor; `tagged`/`tag_field` discriminant metadata |
|  [05]   | `inspect.ListType`   | type node      | list type descriptor            |
|  [06]   | `inspect.DictType`   | type node      | dict type descriptor            |
|  [07]   | `inspect.EnumType`   | type node      | enum type descriptor            |
|  [08]   | `inspect.CustomType` | type node      | custom enc/dec hook type        |
|  [09]   | `inspect.TypedDictType` | type node   | `TypedDict` schema descriptor   |
|  [10]   | `inspect.NamedTupleType`| type node   | `NamedTuple` schema descriptor  |
|  [11]   | `inspect.DateTimeType`  | type node   | datetime descriptor with `tz` constraint flag |
|  [12]   | `inspect.RawType`    | type node      | `Raw` deferred-payload descriptor |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level encode/decode/convert
- rail: serialization

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `msgspec.json.encode(obj, *, enc_hook, order)`           | encode         | object to JSON bytes              |
|  [02]   | `msgspec.json.decode(buf, *, type, strict, dec_hook)`    | decode         | JSON bytes to typed object        |
|  [03]   | `msgspec.msgpack.encode(obj, *, enc_hook)`               | encode         | object to msgpack bytes           |
|  [04]   | `msgspec.msgpack.decode(buf, *, type, strict, dec_hook)` | decode         | msgpack bytes to typed object     |
|  [05]   | `msgspec.convert(obj, type, *, strict, from_attributes, dec_hook, builtin_types, str_keys)` | conversion | duck-typed object coercion with validation |
|  [06]   | `msgspec.to_builtins(obj, *, str_keys, builtin_types, enc_hook, order)` | projection | struct to plain Python builtins (round-trips through `convert`) |
|  [07]   | `msgspec.field(*, default, default_factory, name)`       | field factory  | struct field with metadata        |
|  [08]   | `msgspec.defstruct(name, fields, *, bases, module, namespace, tag, tag_field, rename, omit_defaults, forbid_unknown_fields, frozen, eq, order, kw_only, repr_omit_defaults, array_like, gc, weakref, dict, cache_hash)` | dynamic ctor | create Struct subclass at runtime with full config |
|  [09]   | `msgspec.Meta(*, gt, ge, lt, le, multiple_of, pattern, min_length, max_length, tz, title, description, examples, extra_json_schema, extra)` | constraint ctor | `Annotated` constraint; numeric and non-numeric constraint families are mutually exclusive |

[ENTRYPOINT_SCOPE]: stateful codecs
- rail: serialization

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `json.Encoder(*, enc_hook, decimal_format, uuid_format, order)` | codec          | reusable encoder instance       |
|  [02]   | `json.Decoder(type, *, strict, dec_hook, float_hook)`           | codec          | reusable typed decoder instance |
|  [03]   | `json.Encoder.encode_into(obj, buffer, offset)`                 | zero-alloc     | encode into a caller `bytearray`, no intermediate alloc |
|  [04]   | `json.Decoder.decode_lines(buf)`                                | streaming      | decode newline-delimited JSON to `list[type]` |
|  [05]   | `json.format(buf, *, indent)`                                   | formatter      | pretty-print JSON bytes         |
|  [06]   | `json.schema(type, *, schema_hook, ref_template)`               | schema gen     | JSON Schema dict for a type     |
|  [07]   | `json.schema_components(types, *, schema_hook, ref_template)`   | schema gen     | component schemas + `$defs`     |
|  [08]   | `msgpack.Encoder(*, enc_hook, decimal_format, uuid_format, order)` | codec       | reusable msgpack encoder        |
|  [09]   | `msgpack.Decoder(type, *, strict, dec_hook, ext_hook)`          | codec          | reusable typed msgpack decoder; `ext_hook` decodes `Ext` payloads |
|  [10]   | `msgpack.Encoder.encode_into(obj, buffer, offset)`              | zero-alloc     | encode msgpack into a caller `bytearray` |

[ENTRYPOINT_SCOPE]: struct utilities
- rail: serialization

| [INDEX] | [SURFACE]                                    | [ENTRY_FAMILY]  | [RAIL]                      |
| :-----: | :------------------------------------------- | :-------------- | :-------------------------- |
|  [01]   | `structs.fields(type_or_instance)`           | reflection      | tuple of FieldInfo          |
|  [02]   | `structs.asdict(struct)`                     | projection      | struct to dict              |
|  [03]   | `structs.astuple(struct)`                    | projection      | struct to tuple             |
|  [04]   | `structs.replace(struct, **changes)`         | copy-with       | struct with field overrides |
|  [05]   | `structs.force_setattr(struct, name, value)` | mutation escape | bypass frozen struct guard  |

[ENTRYPOINT_SCOPE]: inspect introspection
- rail: serialization

| [INDEX] | [SURFACE]                        | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `inspect.type_info(type)`        | reflection     | type node tree for a single type   |
|  [02]   | `inspect.multi_type_info(types)` | reflection     | type node trees for multiple types |
|  [03]   | `inspect.is_struct(obj)`         | type guard     | True if instance is a Struct       |
|  [04]   | `inspect.is_struct_type(type)`   | type guard     | True if type is a Struct subclass  |

## [04]-[IMPLEMENTATION_LAW]

[MSGSPEC_TOPOLOGY]:
- namespaces: `msgspec` (core), `msgspec.json`, `msgspec.msgpack`, `msgspec.toml`, `msgspec.yaml`, `msgspec.structs`, `msgspec.inspect`
- `Struct` is a C-extension class; field types are resolved at class creation time, not at decode time
- `Struct` subclass keywords configure the record: `frozen`, `tag`/`tag_field` (tagged-union discriminant), `array_like`, `omit_defaults`, `rename`, `forbid_unknown_fields`, and `gc` — `gc=False` opts a leaf struct holding only non-container fields out of the cyclic garbage collector's tracked set, removing per-instance GC overhead on high-allocation paths; each keyword surfaces on the per-class `structs.StructConfig`
- `Meta` carries constraint metadata used inside `Annotated[T, Meta(...)]` and validated during decode; numeric (`gt`/`ge`/`lt`/`le`/`multiple_of`) and non-numeric (`pattern`/`min_length`/`max_length`/`tz`) constraint families cannot mix on one `Meta`, and `title`/`description`/`examples`/`extra_json_schema` feed `json.schema` output
- `UNSET` is the `UnsetType` singleton meaning "the client omitted this field" (round-trips as absent under `omit_defaults`); `NODEFAULT` is a *distinct* singleton meaning "this field declares no default" — the two are not identical (`UNSET is NODEFAULT` is `False`), so a field typed `int | UnsetType = UNSET` models tri-state presence while `NODEFAULT` only surfaces in `FieldInfo.default`
- `json.Encoder`/`json.Decoder` instances are reusable; prefer them over per-call `encode`/`decode` in hot paths, and `Encoder.encode_into(obj, buffer, offset)` writes directly into a reused `bytearray` for zero intermediate allocation, while `Decoder.decode_lines` decodes an NDJSON frame stream in one C pass
- `defstruct` creates a `Struct` subclass at runtime; field names and types are provided as a sequence of tuples
- `Struct.__struct_config__` exposes the per-class `structs.StructConfig`; `.tag` recovers the tagged-union discriminant value and `.tag_field` its key, read directly off an instance with no `match`
- `Struct.__struct_fields__` is the declaration-order tuple of field names; `structs.fields(type)` returns the richer `FieldInfo` tuple carrying name, encode_name, type, and default
- `inspect.Type` is the abstract base of every `inspect.*Type` node; `type_info`/`multi_type_info` return trees of these nodes typed at the base

[STACKS_WITH]:
- pydantic discriminated unions: a `Struct` tagged union (`Struct, tag=...`, `tag_field=...`) is the wire discriminant; when a pydantic `BaseModel` consumes the same payload, `msgspec.to_builtins` projects the struct to JSON-safe builtins that feed `TypeAdapter.validate_python`, and `Decoder(dec_hook=...)` resolves custom scalar types that pydantic also annotates — one discriminant vocabulary spans both validators with no parallel DTO.
- otel span attributes: `msgspec.to_builtins(struct, str_keys=True)` yields the `str | bool | int | float | Sequence[...]` builtins that `Span.set_attributes` accepts directly, so a decoded wire struct flows into a trace attribute map without a hand-rolled flattener.
- grpc payloads: a `Struct` is the application-level message shape decoded from a gRPC `bytes` body; `Decoder(type=T).decode(message_bytes)` validates at the servicer boundary and `Encoder.encode_into` writes the response into a reused buffer, keeping the gRPC wire and the domain record on one `Struct`.
- numpy hand-off: arrays cross the wire as `Raw` (deferred) or as list payloads; a `Struct` field typed `Raw` defers the numeric block so `numpy.asarray` reconstructs only when the consumer needs the buffer, avoiding eager decode of large numeric sub-trees.

[LOCAL_ADMISSION]:
- Define wire models as `Struct` subclasses; use `Annotated[T, Meta(...)]` for validated constraints on fields, and `gc=False` on leaf structs holding only non-container fields to drop them from the tracked GC set on high-allocation paths.
- Use `msgspec.convert` to coerce unvalidated dicts/objects into typed `Struct` instances at boundary intake; pass `from_attributes=True` to ingest ORM/attribute objects and `dec_hook` for custom scalar reconstruction.
- Catch `DecodeError` / `ValidationError` at I/O boundaries and map to domain error types before propagating; `ValidationError` carries the JSON-pointer-style path of the offending field.
- Use `json.schema` / `json.schema_components` to generate JSON Schema from `Struct` types for OpenAPI / contract surfaces, with `schema_hook` covering custom-typed fields.

[RAIL_LAW]:
- Package: `msgspec`
- Owns: Struct definition, JSON/MessagePack encode-decode, validation, schema generation, struct introspection
- Accept: `Struct`, `msgspec.json.encode`/`decode`, `msgspec.convert`, `Meta`, `defstruct`
- Reject: hand-rolled JSON validation, manual isinstance guards replacing `msgspec.convert`, separate DTO types for the same wire shape

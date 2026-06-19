# [PY_BRANCH_API_MSGSPEC]

`msgspec` supplies a high-performance serialization and validation library built on `Struct`, a C-extension record type. It provides zero-copy JSON/MessagePack encode/decode, `Annotated`-constraint validation on `Struct` fields, dynamic struct construction via `defstruct`, type-level schema introspection through `msgspec.inspect`, and JSON Schema generation from Python types.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `msgspec`
- package: `msgspec`
- module: `msgspec`
- asset: runtime library
- rail: serialization

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core record type
- rail: serialization

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]    | [RAIL]                             |
| :-----: | :----------- | :--------------- | :--------------------------------- |
|  [01]   | `Struct`     | base class       | typed serializable record          |
|  [02]   | `StructMeta` | metaclass        | struct class construction          |
|  [03]   | `Raw`        | bytes wrapper    | deferred/opaque encoded value      |
|  [04]   | `UnsetType`  | sentinel type    | absence marker for optional fields |
|  [05]   | `Meta`       | constraint class | `Annotated` field constraint       |

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

| [INDEX] | [SYMBOL]       | [TYPE_FAMILY]  | [RAIL]                      |
| :-----: | :------------- | :------------- | :-------------------------- |
|  [01]   | `json.Encoder` | stateful codec | reusable JSON encoder       |
|  [02]   | `json.Decoder` | stateful codec | reusable typed JSON decoder |

[PUBLIC_TYPE_SCOPE]: MessagePack codec
- rail: serialization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]  | [RAIL]                             |
| :-----: | :---------------- | :------------- | :--------------------------------- |
|  [01]   | `msgpack.Encoder` | stateful codec | reusable MessagePack encoder       |
|  [02]   | `msgpack.Decoder` | stateful codec | reusable typed MessagePack decoder |
|  [03]   | `msgpack.Ext`     | extension type | MessagePack extension payload      |

[PUBLIC_TYPE_SCOPE]: struct introspection
- rail: serialization

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY]  | [RAIL]                           |
| :-----: | :--------------------- | :------------- | :------------------------------- |
|  [01]   | `structs.FieldInfo`    | field metadata | name, encode_name, type, default |
|  [02]   | `structs.StructConfig` | config record  | struct class configuration       |

[PUBLIC_TYPE_SCOPE]: inspect type nodes (selection)
- rail: serialization

| [INDEX] | [SYMBOL]             | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :------------------- | :------------ | :---------------------------- |
|  [01]   | `inspect.StructType` | type node     | struct type descriptor        |
|  [02]   | `inspect.Field`      | field node    | struct field descriptor       |
|  [03]   | `inspect.Metadata`   | meta node     | annotated metadata descriptor |
|  [04]   | `inspect.UnionType`  | type node     | union type descriptor         |
|  [05]   | `inspect.ListType`   | type node     | list type descriptor          |
|  [06]   | `inspect.DictType`   | type node     | dict type descriptor          |
|  [07]   | `inspect.EnumType`   | type node     | enum type descriptor          |
|  [08]   | `inspect.CustomType` | type node     | custom enc/dec hook type      |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level encode/decode/convert
- rail: serialization

| [INDEX] | [SURFACE]                                                | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :------------------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `msgspec.json.encode(obj, *, enc_hook, order)`           | encode         | object to JSON bytes              |
|  [02]   | `msgspec.json.decode(buf, *, type, strict, dec_hook)`    | decode         | JSON bytes to typed object        |
|  [03]   | `msgspec.msgpack.encode(obj, *, enc_hook)`               | encode         | object to msgpack bytes           |
|  [04]   | `msgspec.msgpack.decode(buf, *, type, strict, dec_hook)` | decode         | msgpack bytes to typed object     |
|  [05]   | `msgspec.convert(obj, type, *, strict, from_attributes)` | conversion     | duck-typed object coercion        |
|  [06]   | `msgspec.to_builtins(obj, *, str_keys, enc_hook)`        | projection     | struct to plain Python builtins   |
|  [07]   | `msgspec.field(*, default, default_factory, name)`       | field factory  | struct field with metadata        |
|  [08]   | `msgspec.defstruct(name, fields, **opts)`                | dynamic ctor   | create Struct subclass at runtime |

[ENTRYPOINT_SCOPE]: stateful codecs
- rail: serialization

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `json.Encoder(*, enc_hook, decimal_format, uuid_format, order)` | codec          | reusable encoder instance       |
|  [02]   | `json.Decoder(type, *, strict, dec_hook, float_hook)`           | codec          | reusable typed decoder instance |
|  [03]   | `json.format(buf, *, indent)`                                   | formatter      | pretty-print JSON bytes         |
|  [04]   | `json.schema(type, *, schema_hook, ref_template)`               | schema gen     | JSON Schema dict for a type     |
|  [05]   | `json.schema_components(types, *, schema_hook, ref_template)`   | schema gen     | component schemas + defs        |
|  [06]   | `msgpack.Encoder(*, enc_hook)`                                  | codec          | reusable msgpack encoder        |
|  [07]   | `msgpack.Decoder(type, *, strict, dec_hook)`                    | codec          | reusable typed msgpack decoder  |

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
- `Meta` carries constraint metadata used inside `Annotated[T, Meta(...)]` and validated during decode
- `UnsetType` (singleton `NODEFAULT`) marks fields as having no default; absent in encoded output when `omit_defaults=True`
- `json.Encoder`/`json.Decoder` instances are reusable; prefer them over per-call `encode`/`decode` in hot paths
- `defstruct` creates a `Struct` subclass at runtime; field names and types are provided as a sequence of tuples

[LOCAL_ADMISSION]:
- Define wire models as `Struct` subclasses; use `Annotated[T, Meta(...)]` for validated constraints on fields.
- Use `msgspec.convert` to coerce unvalidated dicts/objects into typed `Struct` instances at boundary intake.
- Catch `DecodeError` / `ValidationError` at I/O boundaries and map to domain error types before propagating.
- Use `json.schema` to generate JSON Schema from `Struct` types for OpenAPI / contract surfaces.

[RAIL_LAW]:
- Package: `msgspec`
- Owns: Struct definition, JSON/MessagePack encode-decode, validation, schema generation, struct introspection
- Accept: `Struct`, `msgspec.json.encode`/`decode`, `msgspec.convert`, `Meta`, `defstruct`
- Reject: hand-rolled JSON validation, manual isinstance guards replacing `msgspec.convert`, separate DTO types for the same wire shape

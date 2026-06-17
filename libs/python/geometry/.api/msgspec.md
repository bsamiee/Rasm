# [PY_RUNTIME_API_MSGSPEC]

`msgspec` supplies zero-cost typed serialization: `Struct` models with slots and frozen/tagged options, JSON/MessagePack/TOML/YAML encoders and decoders with schema validation, `Meta` constraint annotations, `Raw` deferred payloads, the `inspect` schema-introspection surface, and `convert`/`to_builtins` transforms. It is the runtime owner for wire structs and fast codecs.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `msgspec`
- package: `msgspec`
- import: `msgspec`
- version: `0.21.1`
- owner: `runtime`
- rail: serialization
- namespaces: `msgspec`, `msgspec.json`, `msgspec.msgpack`, `msgspec.toml`, `msgspec.yaml`, `msgspec.structs`, `msgspec.inspect`
- capability: typed `Struct` models, JSON/MessagePack/TOML/YAML codecs, schema validation, constraint metadata, deferred raw payloads, schema introspection, builtin conversion

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: struct and metadata family
- rail: serialization

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]  | [RAIL]                          |
| :-----: | :----------- | :------------- | :------------------------------ |
|   [1]   | `Struct`     | model base     | slotted typed wire struct       |
|   [2]   | `StructMeta` | metaclass      | struct construction metaclass   |
|   [3]   | `Meta`       | annotation     | field constraint metadata       |
|   [4]   | `field`      | field spec     | default/rename field descriptor |
|   [5]   | `Raw`        | deferred       | un-decoded payload holder       |
|   [6]   | `UnsetType`  | sentinel       | absent-field marker type        |
|   [7]   | `UNSET`      | sentinel value | absent-field sentinel           |
|   [8]   | `NODEFAULT`  | sentinel value | no-default marker               |

[PUBLIC_TYPE_SCOPE]: codec family
- rail: serialization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                       |
| :-----: | :---------------- | :------------ | :--------------------------- |
|   [1]   | `json.Encoder`    | encoder       | reusable JSON encoder        |
|   [2]   | `json.Decoder`    | decoder       | typed reusable JSON decoder  |
|   [3]   | `msgpack.Encoder` | encoder       | reusable MessagePack encoder |
|   [4]   | `msgpack.Decoder` | decoder       | typed MessagePack decoder    |
|   [5]   | `inspect.Type`    | schema node   | introspected type node       |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: serialization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :---------------- | :------------ | :-------------------------- |
|   [1]   | `MsgspecError`    | fault         | base codec error            |
|   [2]   | `DecodeError`     | fault         | malformed-input error       |
|   [3]   | `EncodeError`     | fault         | unencodable-value error     |
|   [4]   | `ValidationError` | fault         | schema-constraint violation |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: codec operations
- rail: serialization

| [INDEX] | [SURFACE]                           | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :---------------------------------- | :------------- | :------------------------- |
|   [1]   | `json.encode`                       | encode         | value to JSON bytes        |
|   [2]   | `json.decode`                       | decode         | JSON bytes to typed value  |
|   [3]   | `json.Decoder.decode`               | decode         | reusable typed decode      |
|   [4]   | `json.Decoder.decode_lines`         | decode         | NDJSON line decode         |
|   [5]   | `json.Encoder.encode_into`          | encode         | encode into a buffer       |
|   [6]   | `json.Encoder.encode_lines`         | encode         | NDJSON line encode         |
|   [7]   | `msgpack.encode` / `msgpack.decode` | codec          | MessagePack round-trip     |
|   [8]   | `toml.encode` / `toml.decode`       | codec          | TOML round-trip            |
|   [9]   | `yaml.encode` / `yaml.decode`       | codec          | YAML round-trip            |
|  [10]   | `convert`                           | transform      | builtins to typed value    |
|  [11]   | `to_builtins`                       | transform      | typed value to builtins    |
|  [12]   | `defstruct`                         | factory        | runtime struct definition  |
|  [13]   | `json.schema`                       | schema         | JSON schema for a type     |
|  [14]   | `inspect.type_info`                 | schema         | introspect a type's schema |

## [4]-[IMPLEMENTATION_LAW]

[SERIALIZATION_TOPOLOGY]:
- struct law: wire and cross-boundary payloads are `Struct` subclasses with `frozen=True`, `kw_only=True`, and `forbid_unknown_fields=True` where strictness applies; parallel near-identical structs collapse into one tagged-union struct hierarchy via the `tag`/`tag_field` knobs.
- codec law: hot paths reuse a single configured `Decoder(Type)`/`Encoder()` instance per type, never re-calling module-level `decode`/`encode` with the type each call.
- constraint law: field bounds are `Annotated[T, Meta(...)]`; constraints live in the annotation, never in imperative post-init checks.
- deferred law: payloads decoded lazily use `Raw`; absent-versus-null fields use `UNSET`/`UnsetType`, never `None` overloading.
- streaming law: line-delimited streams use `decode_lines`/`encode_lines`; no manual newline splitting.
- division of labor: msgspec owns fast typed wire structs and codecs; pydantic owns rich validation, computed fields, and DSN/network types. A struct gains no validator logic; that promotes it to a pydantic model.

[LOCAL_ADMISSION]:
- The content-identity owner hashes the canonical `to_builtins`/`json.encode` byte image; identity is computed once over the encoded bytes, never over an ad hoc repr.
- Decode/validation faults are lifted into `Error(BoundaryFault(...))` at ingress; codecs are not invoked inside domain logic with try/except.

[RAIL_LAW]:
- Package: `msgspec`
- Owns: typed wire structs, JSON/MessagePack/TOML/YAML codecs, constraint metadata, deferred payloads, and builtin conversion
- Accept: frozen `Struct` models, reused `Decoder`/`Encoder` instances, `Annotated[..., Meta(...)]` constraints, tagged-union structs
- Reject: parallel near-identical structs, per-call module decode/encode in hot paths, imperative field checks, `None`-as-absent overloading

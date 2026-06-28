# [PY_RUNTIME_API_MSGSPEC]

`msgspec` supplies the runtime's in-memory frame layer: `Struct`, a C-extension record type with zero-copy JSON/MessagePack encode/decode, `Annotated`-`Meta` constraint validation, the `to_builtins`/`convert` lowering/raising pair that bridges canonical `Struct` shapes to wire mappings, reusable stateful `Encoder`/`Decoder` codecs with `enc_hook`/`dec_hook`/`ext_hook`/`float_hook` extension points, JSON-Lines batch framing, buffer-reuse encode, and struct introspection. It is the canonical-shape owner the `transport/wire` codec, the CRDT op-log decode, the capability invoke, the admission settings, and the metric/clock receipts compose; it never hand-rolls JSON validation or a parallel DTO type for the same wire shape.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `msgspec`
- package: `msgspec`
- import: `msgspec`
- owner: `runtime`
- rail: serialization
- namespaces: `msgspec`, `msgspec.json`, `msgspec.msgpack`, `msgspec.structs`, `msgspec.inspect`, `msgspec.toml`, `msgspec.yaml`
- installed: `0.21.1`
- asset: `_core.cpython-315-darwin.so` C extension; `json`/`msgpack`/`structs`/`inspect`/`toml`/`yaml` are thin Python facades over the one compiled core
- capability: `Struct` record definition, JSON/MessagePack encode-decode, `Meta` constraint validation, builtins lowering/duck-typed conversion, reusable codecs with hooks, JSON-Lines framing, struct introspection, JSON-Schema generation, runtime struct definition, TOML/YAML codecs over the same type machinery

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: core record and value types
- rail: serialization

| [INDEX] | [SYMBOL]     | [TYPE_FAMILY]    | [RAIL]                                                              |
| :-----: | :----------- | :--------------- | :----------------------------------------------------------------- |
|  [01]   | `Struct`     | base class       | typed serializable record (`metaclass=StructMeta`)                 |
|  [02]   | `Raw`        | `bytes` subclass | deferred/opaque pre-encoded value; round-trips a sub-message un-decoded, `.copy()` detaches the buffer view from the parent input |
|  [03]   | `Meta`       | constraint class | `Annotated` field constraint carrier (`gt`/`ge`/`lt`/`le`/`multiple_of`/`pattern`/`min_length`/`max_length`/`tz`/`title`/`description`/`examples`/`extra_json_schema`/`extra`) |
|  [04]   | `UnsetType`  | sentinel enum    | `UNSET` marker distinguishing absent from `None` on optional fields |
|  [05]   | `UNSET`      | sentinel value   | the singleton `UnsetType.UNSET`; omitted from encode output        |

[PUBLIC_TYPE_SCOPE]: error types
- rail: serialization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]   | [RAIL]                                                  |
| :-----: | :---------------- | :-------------- | :------------------------------------------------------ |
|  [01]   | `MsgspecError`    | base exception  | root of all msgspec exceptions                          |
|  [02]   | `EncodeError`     | encode failure  | object not representable in the target wire form        |
|  [03]   | `DecodeError`     | decode failure  | malformed input (`ValidationError` subclasses this)     |
|  [04]   | `ValidationError` | validation fail | `Meta` constraint or type-mismatch violation on decode  |

[PUBLIC_TYPE_SCOPE]: codec types
- rail: serialization

| [INDEX] | [SYMBOL]          | [TYPE_FAMILY]    | [RAIL]                                                       |
| :-----: | :---------------- | :--------------- | :----------------------------------------------------------- |
|  [01]   | `json.Encoder`    | stateful codec   | reusable JSON encoder (`enc_hook`, `order`, `decimal_format`) |
|  [02]   | `json.Decoder`    | stateful codec   | reusable typed JSON decoder (`dec_hook`, `float_hook`)        |
|  [03]   | `msgpack.Encoder` | stateful codec   | reusable msgpack encoder (`enc_hook`, `order`)               |
|  [04]   | `msgpack.Decoder` | stateful codec   | reusable typed msgpack decoder (`dec_hook`, `ext_hook`)      |
|  [05]   | `msgpack.Ext`     | extension type   | MessagePack extension payload (`code: int`, `data: bytes \| bytearray \| memoryview`) |

[PUBLIC_TYPE_SCOPE]: struct introspection
- rail: serialization

| [INDEX] | [SYMBOL]                   | [TYPE_FAMILY] | [RAIL]                                                                          |
| :-----: | :------------------------- | :------------ | :------------------------------------------------------------------------------ |
|  [01]   | `structs.StructConfig`     | config record | per-class struct config (`frozen`, `eq`, `order`, `array_like`, `gc`, `tag`, `tag_field`, `forbid_unknown_fields`, `omit_defaults`, `weakref`, `dict`, `cache_hash`) |
|  [02]   | `structs.FieldInfo`        | field record  | per-field descriptor (`name`, `encode_name`, `type`, `default`, `default_factory`) |
|  [03]   | `Struct.__struct_config__` | config handle | the per-class `StructConfig`                                                    |
|  [04]   | `Struct.__struct_fields__` | name tuple    | declared field names in declaration order                                       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: top-level encode/decode/convert/define
- rail: serialization

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `msgspec.json.encode(obj, *, enc_hook, order)`                                             | encode         | object to JSON bytes                            |
|  [02]   | `msgspec.json.decode(buf, *, type, strict, dec_hook)`                                      | decode         | JSON bytes/str to typed object                  |
|  [03]   | `msgspec.msgpack.encode(obj, *, enc_hook, order)`                                          | encode         | object to msgpack bytes                         |
|  [04]   | `msgspec.msgpack.decode(buf, *, type, strict, dec_hook, ext_hook)`                         | decode         | msgpack bytes to typed object                   |
|  [05]   | `msgspec.convert(obj, type, *, strict, from_attributes, dec_hook, builtin_types, str_keys)` | conversion     | duck-typed object/builtins coercion to `Struct` |
|  [06]   | `msgspec.to_builtins(obj, *, str_keys, builtin_types, enc_hook, order)`                    | projection     | `Struct` to plain builtins; `order` is `None`/`"deterministic"`/`"sorted"` |
|  [07]   | `msgspec.field(*, default, name)` / `field(*, default_factory, name)` / `field(*, name)`   | field factory  | three overloads: scalar default, zero-arg factory default, or rename-only (mutually exclusive) |
|  [08]   | `msgspec.defstruct(name, fields, *, bases, module, namespace, tag, tag_field, rename, omit_defaults, forbid_unknown_fields, frozen, eq, order, kw_only, repr_omit_defaults, array_like, gc, weakref, dict, cache_hash)` | runtime define | build a fully-configured `Struct` subclass at runtime |
|  [09]   | `Meta(*, gt, ge, lt, le, multiple_of, pattern, min_length, max_length, tz, title, description, examples, extra_json_schema, extra)` | constraint     | `Annotated[T, Meta(...)]` field validation carrier; all fields `Final` |

[ENTRYPOINT_SCOPE]: stateful codecs and hooks
- rail: serialization
- `Encoder`/`Decoder` instances are reusable; prefer them over per-call `encode`/`decode` on hot paths.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                                          |
| :-----: | :------------------------------------------------------------------------- | :------------- | :---------------------------------------------- |
|  [01]   | `json.Encoder(*, enc_hook, decimal_format="string", uuid_format="canonical", order=None)` | codec | reusable JSON encoder; `decimal_format` is `"string"`/`"number"`, `uuid_format` is `"canonical"`/`"hex"`, `order` is `None`/`"deterministic"`/`"sorted"` |
|  [02]   | `json.Encoder.encode(obj)` / `.encode_into(obj, buffer, offset)`           | encode         | encode to new bytes or into a reused `bytearray` |
|  [03]   | `json.Encoder.encode_lines(items)`                                         | batch encode   | newline-delimited JSON (JSON Lines) frame       |
|  [04]   | `json.Decoder(type, *, strict, dec_hook, float_hook)`                      | codec          | reusable typed JSON decoder                     |
|  [05]   | `json.Decoder.decode(buf)` / `.decode_lines(buf)`                          | decode         | one value or a `list[T]` of JSON-Lines records  |
|  [06]   | `json.schema(type, *, schema_hook)` / `json.schema_components(types, *, ref_template)` | schema gen     | JSON-Schema dict / shared `$defs` component map |
|  [07]   | `json.format(buf, *, indent)`                                              | reformat       | pretty/compact an already-encoded JSON buffer   |
|  [08]   | `msgpack.Encoder(*, enc_hook, decimal_format="string", uuid_format="canonical", order=None)` | codec | reusable msgpack encoder; `uuid_format` additionally admits `"bytes"` for compact 16-byte UUID extension payloads |
|  [09]   | `msgpack.Encoder.encode_into(obj, buffer, offset=0)`                       | encode         | encode into a reused `bytearray` at `offset`    |
|  [10]   | `msgpack.Decoder(type, *, strict, dec_hook, ext_hook)`                     | codec          | reusable typed msgpack decoder                  |
|  [11]   | `toml.encode(obj, *, enc_hook, order)` / `toml.decode(buf, *, type, strict, dec_hook)` | codec | TOML codec over the same `Struct`/`Meta` machinery (config seam) |
|  [12]   | `yaml.encode(obj, *, enc_hook, order)` / `yaml.decode(buf, *, type, strict, dec_hook)` | codec | YAML codec over the same type machinery (requires `PyYAML`) |

[ENTRYPOINT_SCOPE]: struct utilities and introspection
- rail: serialization

| [INDEX] | [SURFACE]                              | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `structs.replace(struct, **changes)`   | copy-with      | immutable struct with field overrides        |
|  [02]   | `structs.asdict(struct)`               | projection     | struct to `dict[str, Any]`                   |
|  [03]   | `structs.astuple(struct)`              | projection     | struct to positional `tuple`                 |
|  [04]   | `structs.fields(type_or_instance)`     | introspection  | tuple of `FieldInfo` (each exposing `.required`) for a struct type |
|  [05]   | `structs.force_setattr(struct, name, value)` | escape hatch | mutate a `frozen=True` struct field in place |
|  [06]   | `inspect.type_info(type)` / `inspect.type_info(types)`  | type model | structured `inspect` schema of any annotation; the canonical reflection surface for generating custom validators or wire descriptors off a `Struct` |

## [04]-[IMPLEMENTATION_LAW]

[MSGSPEC_TOPOLOGY]:
- `Struct` is a C-extension class; field types are resolved at class creation time, not at decode time. Subclass keywords configure the record — `frozen`, `eq`, `order`, `kw_only`, `tag`/`tag_field` (tagged-union discriminant), `rename` (`"lower"`/`"upper"`/`"camel"`/`"pascal"`/`"kebab"` or a `Callable`/`Mapping` for boundary field-name mapping at the edge), `array_like`, `omit_defaults`, `repr_omit_defaults`, `forbid_unknown_fields`, `weakref`, `dict`, `cache_hash`, and `gc` — each surfacing on the per-class `structs.StructConfig`. `gc=False` opts a leaf struct holding only non-container fields out of the cyclic GC's tracked set, removing per-instance GC overhead on high-allocation paths. `rename` is the canonical boundary field-mapping mechanism: internal canonical field names map to wire names at the codec, never a parallel DTO.
- constraint law: `Annotated[T, Meta(...)]` carries declarative validation enforced at decode time by the same C core that decodes the type — numeric bounds (`gt`/`ge`/`lt`/`le`/`multiple_of`), string `pattern`/`min_length`/`max_length`, collection length, and `tz`-aware datetime. A violation raises `ValidationError` with the failing constraint path; constraints are never re-checked in domain code after a typed decode. `title`/`description`/`examples`/`extra_json_schema` flow into `json.schema` output, so one annotated `Struct` is both the validator and the schema source.
- tagged-union decode: a `Decoder(type=A | B | C)` over `Struct` subclasses carrying `tag`/`tag_field` discriminates by the tag field on the JSON object or, with `array_like=True` and integer `tag=n`, position-for-position from the msgpack tag-keyed array envelope; this is the canonical wire discriminator, never an `isinstance` ladder.
- `UNSET`/`UnsetType` is the third state on an optional field: a field typed `int | UnsetType = UNSET` distinguishes "absent" from an explicit `None`, and `UNSET` is dropped from encode output independent of `omit_defaults`.
- deferred-decode law: a field typed `Raw` captures a sub-message's raw bytes without decoding it, so a routing decode reads only the discriminant/envelope and defers the expensive inner decode to a second `Decoder` selected by the tag; `Raw.copy()` detaches the buffer view from the parent input so the outer buffer can be released. This is the canonical lazy/partial-decode path for the op-log envelope, never a manual byte-slice.
- hook seam: `enc_hook(obj) -> builtins` lowers an unsupported type on encode; `dec_hook(type, obj) -> value` raises it on decode; `float_hook` (JSON) intercepts every float for `Decimal`-exact paths; `ext_hook(code, data)` (msgpack) decodes a `msgpack.Ext` extension payload. The hooks are the single extension point — one codec instance owns the type's lift/lower, never a parallel converter.
- batch/buffer law: `encode_lines`/`decode_lines` frame a JSON-Lines stream (one record per line) for op-log and event batches; `encode_into(obj, buffer, offset)` reuses a caller-owned `bytearray` to avoid per-message allocation on the hot wire path.
- `to_builtins` lowers a `Struct` to a JSON-compatible mapping of plain builtins (`order="deterministic"` for canonical byte output) and `convert` raises an unvalidated mapping back into a typed `Struct` (`from_attributes=True` reads object attributes, `builtin_types`/`str_keys` widen the accepted input); the pair is the canonical lowering/raising bridge to and from the wire-codec mapping.

[LOCAL_ADMISSION]:
- the runtime defines every canonical wire/frame shape as a `Struct` subclass; `transport/wire` `WireProtoCodec` composes `to_builtins`/`convert` across the protobuf seam (msgspec lowers the canonical `Struct` to builtins, `json_format.ParseDict` raises it into the `*_pb2` message, and the reverse on decode), and `transport/wire` `CrdtOpDecode` composes a cached `msgpack.Decoder` over the op-log delta with an `ext_hook` for the binary lattice clock.
- integration rail: the canonical stack is `msgspec.Decoder(type=<tagged Struct union>, dec_hook=<lift>)` feeding a validated frame, retried under a `stamina` `retry_context` (transport faults only — `ValidationError` is terminal, never retried), inside an `opentelemetry-api` span; the same `Struct` then admits through `pydantic-settings`-validated config without a second model, because msgspec owns the wire shape and pydantic owns the config shape at distinct seams.
- catch `DecodeError`/`ValidationError`/`EncodeError` only inside the `reliability/faults#FAULT` `boundary` conversion, never in domain flow; `ValidationError` carries the constraint path and is lifted to `BoundaryFault` once at egress.
- `gc=False` is applied only to leaf cells (`clock#CLOCK` `Hlc`/`ElementId`) holding no container field; `frozen=True` is the default for every wire `Struct`, with `force_setattr` reserved for the decode-time post-init hook.

[RAIL_LAW]:
- Package: `msgspec`
- Owns: `Struct` definition, JSON/MessagePack encode-decode, `Meta` validation, builtins lowering/conversion, reusable hook-bearing codecs, JSON-Lines framing, struct introspection
- Accept: `Struct` subclasses with tag/array-like discriminants, `rename` boundary field-mapping, `to_builtins`/`convert` boundary bridge, cached `Encoder`/`Decoder` with `enc_hook`/`dec_hook`/`ext_hook`/`float_hook`, `encode_into` buffer reuse, `encode_lines`/`decode_lines` batch frames, `Raw` deferred sub-message decode, `UNSET` tri-state, `Annotated[T, Meta(...)]` constraints, `defstruct` for runtime-derived shapes, `json.schema`/`json.schema_components` off the same annotated `Struct`, `inspect.type_info` reflection
- Reject: hand-rolled JSON validation, manual field-name remapping where `rename` applies, manual byte-slicing where a `Raw` field defers decode, manual `isinstance` guards replacing `convert` or tag dispatch, separate DTO types for the same wire shape, a second schema source duplicating an annotated `Struct`, per-call `encode`/`decode` on hot paths where a cached codec applies, retrying a terminal `ValidationError`

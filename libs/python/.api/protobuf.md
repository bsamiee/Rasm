# [PY_BRANCH_API_PROTOBUF]

`protobuf` supplies `Message`, the descriptor schema model, the descriptor-pool/symbol-database registries, and binary/JSON/text serialization for `protoc`-generated messages. `_pb2.py` classes derive from `google.protobuf.message.Message` and register into the default `DescriptorPool`. Backend selection happens at import (`upb` native C, `cpp`, pure `python`) via `api_implementation`; the fast path is native by default. Dense design composes generated messages through `proto`-module encode/decode and `json_format`/`text_format` at boundaries, well-known types as first-class value carriers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `protobuf`
- package: `protobuf`
- version: `6.33.6`
- license: `3-Clause BSD` (BSD-3-Clause)
- module: `google.protobuf`
- asset: runtime library (native extension)
- rail: transport
- namespaces: `google.protobuf.message`, `...descriptor`, `...descriptor_pool`, `...symbol_database`, `...message_factory`, `...json_format`, `...text_format`, `...proto`, `...internal.api_implementation`, `...internal.well_known_types`, `...any_pb2` / `timestamp_pb2` / `duration_pb2` / `struct_pb2` / `field_mask_pb2` / `wrappers_pb2` / `empty_pb2`, `...compiler.plugin_pb2`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message family
- rail: transport

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [RAIL]                                |
| :-----: | :------------ | :------------ | :------------------------------------ |
|  [01]   | `Message`     | abstract base | root of all generated message classes |
|  [02]   | `DecodeError` | exception     | malformed binary wire input           |
|  [03]   | `EncodeError` | exception     | unserializable message state          |

[PUBLIC_TYPE_SCOPE]: descriptor and registry family
- rail: transport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [RAIL]                                 |
| :-----: | :-------------------- | :------------- | :------------------------------------- |
|  [01]   | `Descriptor`          | message schema | schema for a single message type       |
|  [02]   | `FieldDescriptor`     | field schema   | field type, label, number, and options |
|  [03]   | `EnumDescriptor`      | enum schema    | enum type schema                       |
|  [04]   | `EnumValueDescriptor` | enum value     | enum value name-number pair            |
|  [05]   | `FileDescriptor`      | file schema    | single `.proto` file schema            |
|  [06]   | `ServiceDescriptor`   | service schema | RPC service schema                     |
|  [07]   | `MethodDescriptor`    | method schema  | single RPC method schema               |
|  [08]   | `OneofDescriptor`     | oneof schema   | oneof field group schema               |
|  [09]   | `DescriptorPool`      | registry       | schema registry for cross-file lookups |
|  [10]   | `SymbolDatabase`      | registry       | type-name -> message-class resolver    |

[PUBLIC_TYPE_SCOPE]: backend selection and well-known types
- rail: transport

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------------------------ | :------------ | :--------------------------------------------- |
|  [01]   | `internal.api_implementation.Type()`        | function      | active backend: `'upb'` / `'cpp'` / `'python'` |
|  [02]   | `any_pb2.Any`                               | well-known    | type-URL-tagged embedded message               |
|  [03]   | `timestamp_pb2.Timestamp`                   | well-known    | seconds+nanos UTC instant                      |
|  [04]   | `duration_pb2.Duration`                     | well-known    | signed seconds+nanos span                      |
|  [05]   | `struct_pb2.Struct` / `Value` / `ListValue` | well-known    | dynamic JSON-like object                       |
|  [06]   | `field_mask_pb2.FieldMask`                  | well-known    | set of field paths for partial updates         |
|  [07]   | `wrappers_pb2.*Value` / `empty_pb2.Empty`   | well-known    | nullable scalar wrappers / empty message       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Message instance operations
- rail: transport
- defined on `google.protobuf.message.Message`; backed by the native `_message` extension under the `upb`/`cpp` backends.

| [INDEX] | [SURFACE]                                       | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :---------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `Message.SerializeToString(deterministic=None)` | serialize      | binary wire encoding to `bytes`           |
|  [02]   | `Message.ParseFromString(data)`                 | deserialize    | clear + decode binary wire bytes in place |
|  [03]   | `Message.MergeFromString(data)`                 | deserialize    | merge binary wire bytes in place          |
|  [04]   | `Message.SerializePartialToString()`            | serialize      | encode even if required fields missing    |
|  [05]   | `Message.CopyFrom(other_msg)`                   | mutation       | replace all fields from another message   |
|  [06]   | `Message.MergeFrom(other_msg)`                  | mutation       | merge fields from another message         |
|  [07]   | `Message.Clear()`                               | mutation       | reset all fields to defaults              |
|  [08]   | `Message.ClearField(field_name)`                | mutation       | reset one field to default                |
|  [09]   | `Message.SetInParent()`                         | mutation       | mark a sub-message present (empty)        |
|  [10]   | `Message.HasField(field_name)`                  | query          | test singular/message/oneof presence      |
|  [11]   | `Message.WhichOneof(oneof_name)`                | query          | name of populated oneof member or None    |
|  [12]   | `Message.ListFields()`                          | query          | list set `(FieldDescriptor, value)` pairs |
|  [13]   | `Message.ByteSize()`                            | query          | encoded size in bytes (caches)            |
|  [14]   | `Message.IsInitialized()`                       | query          | all required fields populated             |
|  [15]   | `Message.UnknownFields()`                       | query          | preserved unknown-field set               |
|  [16]   | `Message.DiscardUnknownFields()`                | mutation       | drop preserved unknown fields             |
|  [17]   | `Message.FromString(data)` (classmethod)        | construction   | decode and return new message             |
|  [18]   | `Message.DESCRIPTOR`                            | metadata       | the message's `Descriptor`                |

[ENTRYPOINT_SCOPE]: functional encode/decode (proto module)
- rail: transport
- `from google.protobuf import proto`; the functional, non-mutating mirror of the instance methods.

| [INDEX] | [SURFACE]                                                       | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :-------------------------------------------------------------- | :------------- | :------------------------------------- |
|  [01]   | `proto.serialize(message, deterministic=None) -> bytes`         | serialize      | encode message to `bytes`              |
|  [02]   | `proto.parse(message_class, payload) -> message`                | deserialize    | decode bytes to a NEW message instance |
|  [03]   | `proto.serialize_length_prefixed(message, output: io.BytesIO)`  | streaming      | write varint-length-prefixed frame     |
|  [04]   | `proto.parse_length_prefixed(message_class, input: io.BytesIO)` | streaming      | read one varint-length-prefixed frame  |
|  [05]   | `proto.byte_size(message) -> int`                               | query          | encoded size without serializing       |
|  [06]   | `proto.clear_message(message)`                                  | mutation       | reset all fields                       |
|  [07]   | `proto.clear_field(message, field_name)`                        | mutation       | reset one field                        |

[ENTRYPOINT_SCOPE]: JSON and text format
- rail: transport
- Each surface carries further keyword options beyond those shown; each shown option defaults `False`, and `preserving_proto_field_name` governs both `json_format` serializers.

| [INDEX] | [SURFACE]                                                                  | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :------------------------------------------------------------------------- | :------------- | :--------------------------------------- |
|  [01]   | `json_format.MessageToJson(message)`                                       | serialize      | encode to JSON string                    |
|  [02]   | `json_format.MessageToDict(message, always_print_fields_with_no_presence)` | serialize      | encode to dict                           |
|  [03]   | `json_format.Parse(text, message, ignore_unknown_fields)`                  | deserialize    | decode JSON string into existing message |
|  [04]   | `json_format.ParseDict(js_dict, message, ignore_unknown_fields)`           | deserialize    | decode dict into existing message        |
|  [05]   | `text_format.MessageToString(message, as_one_line)`                        | serialize      | encode to proto text format string       |
|  [06]   | `text_format.MessageToBytes(message)`                                      | serialize      | encode to proto text format bytes        |
|  [07]   | `text_format.Parse(text, message, allow_unknown_field)`                    | deserialize    | decode text format into existing message |
|  [08]   | `text_format.Merge(text, message)`                                         | deserialize    | merge text format into existing message  |

[ENTRYPOINT_SCOPE]: registries and dynamic message classes
- rail: transport

| [INDEX] | [SURFACE]                                                           | [ENTRY_FAMILY] | [RAIL]                                      |
| :-----: | :------------------------------------------------------------------ | :------------- | :------------------------------------------ |
|  [01]   | `descriptor_pool.Default() -> DescriptorPool`                       | registry       | process-wide default pool                   |
|  [02]   | `DescriptorPool.FindMessageTypeByName(full_name) -> Descriptor`     | lookup         | resolve a registered message schema         |
|  [03]   | `DescriptorPool.FindFileByName(file_name) -> FileDescriptor`        | lookup         | resolve a registered `.proto` file          |
|  [04]   | `DescriptorPool.AddSerializedFile(serialized_pb) -> FileDescriptor` | registry       | register a `FileDescriptorProto` at runtime |
|  [05]   | `symbol_database.Default() -> SymbolDatabase`                       | registry       | default symbol resolver                     |
|  [06]   | `SymbolDatabase.GetSymbol(full_name) -> type[Message]`              | lookup         | resolve a generated class by full name      |
|  [07]   | `message_factory.GetMessageClass(descriptor) -> type[Message]`      | factory        | message class for a runtime `Descriptor`    |
|  [08]   | `message_factory.GetMessageClassesForFiles(files, pool) -> dict`    | factory        | all message classes across given files      |

[ENTRYPOINT_SCOPE]: well-known type operations
- rail: transport
- methods on the `_pb2` well-known message instances (mixed in from `internal.well_known_types`).

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] | [RAIL]                          |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :------------------------------ |
|  [01]   | `Any.Pack(msg, type_url_prefix=...)` / `Any.Unpack(msg) -> bool`              | any            | embed / extract a typed message |
|  [02]   | `Any.Is(descriptor) -> bool` / `Any.TypeName() -> str`                        | any            | type discrimination on the URL  |
|  [03]   | `Timestamp.GetCurrentTime()` / `ToDatetime(tzinfo=None)` / `FromDatetime(dt)` | timestamp      | now / `datetime` round-trip     |
|  [04]   | `Timestamp.ToJsonString()` / `FromJsonString(v)`                              | timestamp      | RFC3339 round-trip              |
|  [05]   | `Timestamp.ToNanoseconds()` / `FromNanoseconds(n)`                            | timestamp      | nanos round-trip                |
|  [06]   | `Duration.ToJsonString()` / `FromJsonString(v)`                               | duration       | JSON span round-trip            |
|  [07]   | `Duration.ToNanoseconds()` / `FromTimedelta(td)`                              | duration       | nanos / timedelta round-trip    |
|  [08]   | `Struct.update(dict)` / `Struct.keys()` / `Struct.items()` / `Struct[k] = v`  | struct         | dynamic JSON-like object access |
|  [09]   | `FieldMask.FromJsonString(v)` / `ToJsonString()` / `MergeMessage(src, dst)`   | field mask     | partial-update path set         |

[ENTRYPOINT_SCOPE]: FieldDescriptor constants
- rail: transport
- wire-type prefix `FieldDescriptor.TYPE_`; cardinality prefix `FieldDescriptor.LABEL_`.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                     |
| :-----: | :------------------------------ | :------------ | :------------------------- |
|  [01]   | `TYPE_DOUBLE` / `TYPE_FLOAT`    | wire type     | 64/32-bit float fields     |
|  [02]   | `TYPE_INT32` / `TYPE_INT64`     | wire type     | varint signed int fields   |
|  [03]   | `TYPE_UINT32` / `TYPE_UINT64`   | wire type     | varint unsigned int fields |
|  [04]   | `TYPE_SINT32` / `TYPE_SINT64`   | wire type     | zigzag signed int fields   |
|  [05]   | `TYPE_FIXED32` / `TYPE_FIXED64` | wire type     | fixed-width int fields     |
|  [06]   | `TYPE_SFIXED32`/`TYPE_SFIXED64` | wire type     | fixed-width signed fields  |
|  [07]   | `TYPE_BOOL`                     | wire type     | boolean field              |
|  [08]   | `TYPE_STRING` / `TYPE_BYTES`    | wire type     | UTF-8 string / raw bytes   |
|  [09]   | `TYPE_ENUM`                     | wire type     | enum-typed field           |
|  [10]   | `TYPE_MESSAGE` / `TYPE_GROUP`   | wire type     | embedded message / group   |
|  [11]   | `LABEL_OPTIONAL`                | cardinality   | singular optional field    |
|  [12]   | `LABEL_REQUIRED`                | cardinality   | singular required field    |
|  [13]   | `LABEL_REPEATED`                | cardinality   | repeated field             |

## [04]-[IMPLEMENTATION_LAW]

[BACKEND_TOPOLOGY]:

[WIRE_TOPOLOGY]:
- generated message classes auto-register in `descriptor_pool.Default()`; `symbol_database.Default().GetSymbol(full_name)` and `message_factory.GetMessageClass(descriptor)` resolve a class by name/descriptor — the dynamic-message path when the `_pb2` import is not statically known.
- `SerializeToString(deterministic=True)` produces a stable byte order for hashing/caching; default order is implementation-defined. `ParseFromString` clears the message first; `MergeFromString` overlays onto current state.
- unknown fields are preserved by default in proto3; `UnknownFields()` reads them, `DiscardUnknownFields()` drops them, `json_format`/`text_format` parse can `ignore_unknown_fields`.
- `HasField` applies to message-typed fields and proto3 `optional`/oneof members; plain proto3 scalar fields have no presence. `WhichOneof` returns the populated oneof member name or `None`.

[INTEGRATION_LAW]:
- Stack with `opentelemetry-exporter-otlp-proto-http`: the OTLP exporter encodes SDK telemetry into `opentelemetry-proto` generated `_pb2` messages and ships their `SerializeToString()` bytes. This package is the encode engine under that exporter; the design never hand-builds OTLP wire bytes.
- Boundary intake/emit: `json_format.ParseDict` at intake from JSON config/payloads into a typed message; `MessageToDict(preserving_proto_field_name=True)` at emit to keep snake_case keys. Well-known types are the canonical carriers — `Timestamp.FromDatetime` at the time boundary, `Struct.update` for dynamic JSON-ish bodies, `Any.Pack`/`Unpack` for heterogeneous typed envelopes.
- Streaming frames: `proto.serialize_length_prefixed`/`parse_length_prefixed` are the length-delimited stream helpers — the functional path for record-per-frame transports.

[LOCAL_ADMISSION]:
- Generated `_pb2.py` files are the only source of concrete message classes; never instantiate `Message` directly. For runtime-resolved schemas use `message_factory.GetMessageClass` against the pool.
- Use `proto.serialize`/`proto.parse` for the non-mutating functional path; reserve `ParseFromString` for in-place reuse of a pre-allocated message.

[RAIL_LAW]:
- Package: `protobuf`
- Owns: message serialization (binary/JSON/text), descriptor schema, pool/symbol-database/message-factory registries, well-known type carriers, and the native `upb` encode/decode backend
- Accept: generated `_pb2.py` messages, `proto.serialize`/`parse` functional API, `json_format.ParseDict` at intake, `MessageToDict(preserving_proto_field_name=True)` at emit, well-known types as value carriers, `message_factory` for dynamic schemas
- Reject: hand-rolled binary encoding, direct `Message` instantiation, `ParseFromString` expecting merge semantics, forcing the pure-`python` backend when the native extension is available

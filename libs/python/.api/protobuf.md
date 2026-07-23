# [PY_BRANCH_API_PROTOBUF]

`protobuf` owns `protoc`-generated message serialization across binary wire, JSON, and text, over the descriptor schema model and the descriptor-pool/symbol-database/message-factory registries that resolve a message class by name or descriptor. `_pb2.py` classes derive from `Message` and auto-register into the default `DescriptorPool`. It is the transport rail's wire-encode engine — generated messages compose through the `proto` functional codec and `json_format`/`text_format` at boundaries, well-known types as first-class value carriers.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `protobuf`
- package: `protobuf` (`BSD-3-Clause`)
- module: `google.protobuf`
- namespaces: `...message`, `...descriptor`, `...descriptor_pool`, `...symbol_database`, `...message_factory`, `...json_format`, `...text_format`, `...proto`, `...internal.api_implementation`, `...internal.well_known_types`, `...<wkt>_pb2`, `...compiler.plugin_pb2`
- abi: native `upb` C extension by default, `cpp` or pure `python` selected at import via `api_implementation`
- rail: transport

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message family

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY] | [CAPABILITY]                          |
| :-----: | :------------ | :------------ | :------------------------------------ |
|  [01]   | `Message`     | abstract base | root of all generated message classes |
|  [02]   | `DecodeError` | exception     | malformed binary wire input           |
|  [03]   | `EncodeError` | exception     | unserializable message state          |

[PUBLIC_TYPE_SCOPE]: descriptor and registry family

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY]  | [CAPABILITY]                           |
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

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                   |
| :-----: | :------------------------------------------ | :------------ | :--------------------------------------------- |
|  [01]   | `internal.api_implementation.Type()`        | function      | active backend: `'upb'` / `'cpp'` / `'python'` |
|  [02]   | `any_pb2.Any`                               | well-known    | type-URL-tagged embedded message               |
|  [03]   | `timestamp_pb2.Timestamp`                   | well-known    | seconds+nanos UTC instant                      |
|  [04]   | `duration_pb2.Duration`                     | well-known    | signed seconds+nanos span                      |
|  [05]   | `struct_pb2.Struct` / `Value` / `ListValue` | well-known    | dynamic JSON-like object                       |
|  [06]   | `field_mask_pb2.FieldMask`                  | well-known    | set of field paths for partial updates         |
|  [07]   | `wrappers_pb2.*Value` / `empty_pb2.Empty`   | well-known    | nullable scalar wrappers / empty message       |

[PUBLIC_TYPE_SCOPE]: FieldDescriptor constants
- wire-type prefix `FieldDescriptor.TYPE_`; cardinality prefix `FieldDescriptor.LABEL_`.

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [CAPABILITY]               |
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

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Message instance operations

| [INDEX] | [SURFACE]                                       | [SHAPE]  | [CAPABILITY]                              |
| :-----: | :---------------------------------------------- | :------- | :---------------------------------------- |
|  [01]   | `Message.SerializeToString(deterministic=None)` | instance | binary wire encoding to `bytes`           |
|  [02]   | `Message.ParseFromString(data)`                 | instance | clear + decode binary wire bytes in place |
|  [03]   | `Message.MergeFromString(data)`                 | instance | merge binary wire bytes in place          |
|  [04]   | `Message.SerializePartialToString()`            | instance | encode even if required fields missing    |
|  [05]   | `Message.CopyFrom(other_msg)`                   | instance | replace all fields from another message   |
|  [06]   | `Message.MergeFrom(other_msg)`                  | instance | merge fields from another message         |
|  [07]   | `Message.Clear()`                               | instance | reset all fields to defaults              |
|  [08]   | `Message.ClearField(field_name)`                | instance | reset one field to default                |
|  [09]   | `Message.SetInParent()`                         | instance | mark a sub-message present (empty)        |
|  [10]   | `Message.HasField(field_name)`                  | instance | test singular/message/oneof presence      |
|  [11]   | `Message.WhichOneof(oneof_name)`                | instance | name of populated oneof member or None    |
|  [12]   | `Message.ListFields()`                          | instance | list set `(FieldDescriptor, value)` pairs |
|  [13]   | `Message.ByteSize()`                            | instance | encoded size in bytes (caches)            |
|  [14]   | `Message.IsInitialized()`                       | instance | all required fields populated             |
|  [15]   | `Message.UnknownFields()`                       | instance | preserved unknown-field set               |
|  [16]   | `Message.DiscardUnknownFields()`                | instance | drop preserved unknown fields             |
|  [17]   | `Message.FromString(data)`                      | factory  | decode and return new message             |
|  [18]   | `Message.DESCRIPTOR`                            | property | the message's `Descriptor`                |

[ENTRYPOINT_SCOPE]: functional encode/decode (proto module)
- `from google.protobuf import proto`: non-mutating mirror of the instance methods — `parse` returns a NEW message where `ParseFromString` decodes in place.

| [INDEX] | [SURFACE]                                                       | [SHAPE] | [CAPABILITY]                           |
| :-----: | :-------------------------------------------------------------- | :------ | :------------------------------------- |
|  [01]   | `proto.serialize(message, deterministic=None) -> bytes`         | static  | encode message to `bytes`              |
|  [02]   | `proto.parse(message_class, payload) -> message`                | static  | decode bytes to a NEW message instance |
|  [03]   | `proto.serialize_length_prefixed(message, output: io.BytesIO)`  | static  | write varint-length-prefixed frame     |
|  [04]   | `proto.parse_length_prefixed(message_class, input: io.BytesIO)` | static  | read one varint-length-prefixed frame  |
|  [05]   | `proto.byte_size(message) -> int`                               | static  | encoded size without serializing       |
|  [06]   | `proto.clear_message(message)`                                  | static  | reset all fields                       |
|  [07]   | `proto.clear_field(message, field_name)`                        | static  | reset one field                        |

[ENTRYPOINT_SCOPE]: JSON and text format
- shown options default `False`; `preserving_proto_field_name` governs both `json_format` serializers.

| [INDEX] | [SURFACE]                                                                  | [SHAPE] | [CAPABILITY]                             |
| :-----: | :------------------------------------------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `json_format.MessageToJson(message)`                                       | static  | encode to JSON string                    |
|  [02]   | `json_format.MessageToDict(message, always_print_fields_with_no_presence)` | static  | encode to dict                           |
|  [03]   | `json_format.Parse(text, message, ignore_unknown_fields)`                  | static  | decode JSON string into existing message |
|  [04]   | `json_format.ParseDict(js_dict, message, ignore_unknown_fields)`           | static  | decode dict into existing message        |
|  [05]   | `text_format.MessageToString(message, as_one_line)`                        | static  | encode to proto text format string       |
|  [06]   | `text_format.MessageToBytes(message)`                                      | static  | encode to proto text format bytes        |
|  [07]   | `text_format.Parse(text, message, allow_unknown_field)`                    | static  | decode text format into existing message |
|  [08]   | `text_format.Merge(text, message)`                                         | static  | merge text format into existing message  |

[ENTRYPOINT_SCOPE]: registries and dynamic message classes

| [INDEX] | [SURFACE]                                                           | [SHAPE]  | [CAPABILITY]                                |
| :-----: | :------------------------------------------------------------------ | :------- | :------------------------------------------ |
|  [01]   | `descriptor_pool.Default() -> DescriptorPool`                       | static   | process-wide default pool                   |
|  [02]   | `DescriptorPool.FindMessageTypeByName(full_name) -> Descriptor`     | instance | resolve a registered message schema         |
|  [03]   | `DescriptorPool.FindFileByName(file_name) -> FileDescriptor`        | instance | resolve a registered `.proto` file          |
|  [04]   | `DescriptorPool.AddSerializedFile(serialized_pb) -> FileDescriptor` | instance | register a `FileDescriptorProto` at runtime |
|  [05]   | `symbol_database.Default() -> SymbolDatabase`                       | static   | default symbol resolver                     |
|  [06]   | `SymbolDatabase.GetSymbol(full_name) -> type[Message]`              | instance | resolve a generated class by full name      |
|  [07]   | `message_factory.GetMessageClass(descriptor) -> type[Message]`      | static   | message class for a runtime `Descriptor`    |
|  [08]   | `message_factory.GetMessageClassesForFiles(files, pool) -> dict`    | static   | all message classes across given files      |

[ENTRYPOINT_SCOPE]: well-known type operations

| [INDEX] | [SURFACE]                                                                     | [SHAPE]  | [CAPABILITY]                    |
| :-----: | :---------------------------------------------------------------------------- | :------- | :------------------------------ |
|  [01]   | `Any.Pack(msg, type_url_prefix=...)` / `Any.Unpack(msg) -> bool`              | instance | embed / extract a typed message |
|  [02]   | `Any.Is(descriptor) -> bool` / `Any.TypeName() -> str`                        | instance | type discrimination on the URL  |
|  [03]   | `Timestamp.GetCurrentTime()` / `ToDatetime(tzinfo=None)` / `FromDatetime(dt)` | instance | now / `datetime` round-trip     |
|  [04]   | `Timestamp.ToJsonString()` / `FromJsonString(v)`                              | instance | RFC3339 round-trip              |
|  [05]   | `Timestamp.ToNanoseconds()` / `FromNanoseconds(n)`                            | instance | nanos round-trip                |
|  [06]   | `Duration.ToJsonString()` / `FromJsonString(v)`                               | instance | JSON span round-trip            |
|  [07]   | `Duration.ToNanoseconds()` / `FromTimedelta(td)`                              | instance | nanos / timedelta round-trip    |
|  [08]   | `Struct.update(dict)` / `Struct.keys()` / `Struct.items()` / `Struct[k] = v`  | instance | dynamic JSON-like object access |
|  [09]   | `FieldMask.FromJsonString(v)` / `ToJsonString()` / `MergeMessage(src, dst)`   | instance | partial-update path set         |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `api_implementation.Type()` reports the active `upb`/`cpp`/`python` backend chosen at import; native `upb` is the default fast path and the `_message` extension backs the instance methods.
- generated message classes auto-register in `descriptor_pool.Default()`; `symbol_database.Default().GetSymbol(full_name)` and `message_factory.GetMessageClass(descriptor)` resolve a class by name/descriptor — the dynamic-message path when the `_pb2` import is not statically known.
- `SerializeToString(deterministic=True)` produces a stable byte order for hashing/caching; default order is implementation-defined. `ParseFromString` clears the message first; `MergeFromString` overlays onto current state.
- proto3 preserves unknown fields by default: `UnknownFields()` reads them, `DiscardUnknownFields()` drops them, and `json_format`/`text_format` parse under `ignore_unknown_fields`.
- `HasField` applies to message-typed fields and proto3 `optional`/oneof members; plain proto3 scalar fields carry no presence. `WhichOneof` returns the populated oneof member name or `None`.
- `runtime_version.ValidateProtobufRuntimeVersion` at generated-module import gates gencode/runtime skew.

[STACKING]:
- `opentelemetry-exporter-otlp-proto-http`(`.api/opentelemetry-exporter-otlp-proto-http.md`): the OTLP exporter encodes SDK telemetry into `opentelemetry-proto` `_pb2` messages and ships their `SerializeToString()` bytes; this package is the encode engine under it.
- `msgspec`(`.api/msgspec.md`): the msgspec `Struct` envelope owns the schema and holds the protobuf frame as its opaque `bytes` value; `proto.serialize(message, deterministic=True)`/`proto.parse(message_class, payload)` produce and consume that frame, `message_factory.GetMessageClass(descriptor)` drives descriptor dispatch, and `Any` `@type`-keyed envelopes decode as shapes-vocabulary `Packed` mappings on the discriminant.
- `grpcio`(`.api/grpcio.md`): generated message classes are the request/response types of the `grpc.aio` stubs with `proto.serialize`/`proto.parse` as the channel serializer/deserializer.
- boundary intake/emit: `json_format.ParseDict` raises JSON payloads into a typed message at intake, `MessageToDict(preserving_proto_field_name=True)` keeps snake_case keys at emit.
- well-known carriers: `Timestamp` at the time boundary (`FromDatetime`/`ToDatetime` truncate to microseconds, so a 100-ns clock reconstructs from its own carrier slots), `Struct.update` for dynamic bodies, `Any.Pack`/`Unpack` for heterogeneous typed envelopes.
- stream frames: `proto.serialize_length_prefixed`/`parse_length_prefixed` carry length-delimited record-per-frame transports.
- descriptor drift-gate: the runtime `transport/shapes#aligned` pass resolves `DescriptorPool.FindMessageTypeByName(message.DESCRIPTOR.full_name)` per vocabulary row — a pool read, never an `AddSerializedFile` re-registration — cross-checking the compiled `Descriptor` against the struct's `msgspec.inspect` field set for absent slots, phantom slots, and the `TYPE_*64` floor; the gate proves structure, byte parity stays with the seed-reproduction corpus.

[LOCAL_ADMISSION]:
- Generated `_pb2.py` files are the only source of concrete message classes; a runtime-resolved schema builds through `message_factory.GetMessageClass` against the pool, and the `google.protobuf.internal.builder` path stays inside the generated modules alone.
- `proto.serialize`/`proto.parse` carry the non-mutating functional path; `ParseFromString` is reserved for in-place reuse of a pre-allocated message.

[RAIL_LAW]:
- Package: `protobuf`
- Owns: message serialization (binary/JSON/text), descriptor schema, pool/symbol-database/message-factory registries, well-known type carriers, and the native `upb` encode/decode backend
- Accept: generated `_pb2.py` messages, `proto.serialize`/`parse`, `json_format.ParseDict` at intake, `MessageToDict(preserving_proto_field_name=True)` at emit, well-known types as value carriers, `message_factory` for dynamic schemas
- Reject: hand-rolled binary encoding, direct `Message` instantiation, `ParseFromString` expecting merge semantics, the pure-`python` backend where the native extension is available

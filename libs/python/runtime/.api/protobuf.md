# [PY_RUNTIME_API_PROTOBUF]

`protobuf` supplies the Google Protocol Buffers Python message runtime: the descriptor / descriptor-pool reflection layer, a message factory that builds message classes from descriptors, the JSON and text wire codecs, the well-known wrapper types (`Any`, `Timestamp`, `Duration`, `Struct`, `Value`, `ListValue`, `Empty`), the symbol database, and the runtime-version guard. It is the runtime owner the wire `WireProtoCodec` composes to encode/decode the `CrdtOp*` union frames; it never hand-rolls varint framing or message reflection the C++/upb core already owns.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `protobuf`
- package: `protobuf`
- import: `from google import protobuf`
- owner: `runtime`
- rail: wire
- namespaces: `google.protobuf`, `google.protobuf.descriptor`, `google.protobuf.descriptor_pool`, `google.protobuf.message_factory`, `google.protobuf.json_format`, `google.protobuf.text_format`, `google.protobuf.symbol_database`, `google.protobuf.runtime_version`
- installed: `6.33.6`; license Protocol-Buffers (BSD-3-style); wheels `cp310-abi3` plus `py3-none-any` => cp315-CLEAN, core-direct (no environment marker); version-pinned `<7.0` (opentelemetry-proto caps `protobuf<7`; the pin lifts when OTel admits 7.x)
- capability: message descriptor reflection, descriptor-pool registration, message-class factory, JSON/dict and text-format codecs, well-known wrapper types, symbol database, runtime-version guard

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message runtime and reflection
- rail: wire

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [RAIL]                                        |
| :-----: | :------------------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `message.Message`                | message base  | base class of all generated message types     |
|  [02]   | `descriptor.Descriptor`          | descriptor    | message-type descriptor (fields, options)     |
|  [03]   | `descriptor.FieldDescriptor`     | descriptor    | single-field descriptor (type, label, number) |
|  [04]   | `descriptor.EnumDescriptor`      | descriptor    | enum-type descriptor                          |
|  [05]   | `descriptor.FileDescriptor`      | descriptor    | `.proto` file descriptor                      |
|  [06]   | `descriptor_pool.DescriptorPool` | pool          | descriptor registry / resolver                |
|  [07]   | `symbol_database.SymbolDatabase` | database      | descriptor-to-message-class index             |

[PUBLIC_TYPE_SCOPE]: well-known wrapper types
- rail: wire
- namespace `google.protobuf.<name>_pb2`; the runtime ships the compiled `*_pb2` modules for the well-known set.

| [INDEX] | [SYMBOL]                  | [TYPE_FAMILY] | [RAIL]                                |
| :-----: | :------------------------ | :------------ | :------------------------------------ |
|  [01]   | `any_pb2.Any`             | wrapper       | type-URL-tagged packed message        |
|  [02]   | `timestamp_pb2.Timestamp` | wrapper       | RFC-3339 instant (`seconds`, `nanos`) |
|  [03]   | `duration_pb2.Duration`   | wrapper       | signed span (`seconds`, `nanos`)      |
|  [04]   | `struct_pb2.Struct`       | wrapper       | string-keyed dynamic map of `Value`   |
|  [05]   | `struct_pb2.Value`        | wrapper       | dynamic JSON scalar/struct/list union |
|  [06]   | `struct_pb2.ListValue`    | wrapper       | ordered list of `Value`               |
|  [07]   | `empty_pb2.Empty`         | wrapper       | zero-field unit message               |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message factory and registries
- rail: wire

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY]   | [RAIL]                                         |
| :-----: | :------------------------------------------------ | :--------------- | :--------------------------------------------- |
|  [01]   | `message_factory.GetMessageClass(descriptor)`     | factory          | message class for a `Descriptor`               |
|  [02]   | `descriptor_pool.Default()`                       | pool             | the process-default `DescriptorPool`           |
|  [03]   | `DescriptorPool.Add(file_descriptor_proto)`       | pool register    | register a `FileDescriptorProto`               |
|  [04]   | `DescriptorPool.FindMessageTypeByName(full_name)` | pool resolve     | resolve a `Descriptor` by fully-qualified name |
|  [05]   | `symbol_database.Default()`                       | database         | the process-default `SymbolDatabase`           |
|  [06]   | `SymbolDatabase.GetSymbol(full_name)`             | database resolve | message class by fully-qualified name          |

[ENTRYPOINT_SCOPE]: message instance operations
- rail: wire
- defined on `message.Message`; the wire codec encodes/decodes through these on every generated message.

| [INDEX] | [SURFACE]                | [ENTRY_FAMILY] | [RAIL]                                   |
| :-----: | :----------------------- | :------------- | :--------------------------------------- |
|  [01]   | `SerializeToString()`    | encode         | deterministic binary wire bytes          |
|  [02]   | `ParseFromString(data)`  | decode         | populate from binary wire bytes          |
|  [03]   | `MergeFrom(other)`       | merge          | merge another message into this one      |
|  [04]   | `CopyFrom(other)`        | copy           | overwrite with another message           |
|  [05]   | `Clear()`                | reset          | reset all fields to default              |
|  [06]   | `ByteSize()`             | size           | serialized byte length                   |
|  [07]   | `WhichOneof(oneof_name)` | oneof read     | name of the set field in a `oneof` group |
|  [08]   | `DESCRIPTOR`             | descriptor     | the message's `Descriptor`               |

[ENTRYPOINT_SCOPE]: JSON / dict codec
- rail: wire

| [INDEX] | [SURFACE]                                         | [ENTRY_FAMILY] | [RAIL]                                 |
| :-----: | :------------------------------------------------ | :------------- | :------------------------------------- |
|  [01]   | `json_format.MessageToJson(message, **opts)`      | encode         | message to canonical proto-JSON string |
|  [02]   | `json_format.MessageToDict(message, **opts)`      | encode         | message to JSON-shaped `dict`          |
|  [03]   | `json_format.Parse(text, message, **opts)`        | decode         | proto-JSON string into a message       |
|  [04]   | `json_format.ParseDict(js_dict, message, **opts)` | decode         | JSON-shaped `dict` into a message      |

[ENTRYPOINT_SCOPE]: text-format codec
- rail: wire

| [INDEX] | [SURFACE]                                      | [ENTRY_FAMILY] | [RAIL]                            |
| :-----: | :--------------------------------------------- | :------------- | :-------------------------------- |
|  [01]   | `text_format.MessageToString(message, **opts)` | encode         | message to text-format string     |
|  [02]   | `text_format.Parse(text, message, **opts)`     | decode         | text-format string into a message |

[ENTRYPOINT_SCOPE]: runtime-version guard
- rail: wire

| [INDEX] | [SURFACE]                                                                                                           | [ENTRY_FAMILY] | [RAIL]                                         |
| :-----: | :------------------------------------------------------------------------------------------------------------------ | :------------- | :--------------------------------------------- |
|  [01]   | `runtime_version.ValidateProtobufRuntimeVersion(gen_domain, gen_major, gen_minor, gen_patch, gen_suffix, location)` | guard          | assert gencode/runtime version compatibility   |
|  [02]   | `runtime_version.Domain`                                                                                            | enum           | `PUBLIC` / `GOOGLE_INTERNAL` validation domain |

## [04]-[IMPLEMENTATION_LAW]

[WIRE_TOPOLOGY]:
- message law: every wire frame is a `message.Message` subclass resolved from its `Descriptor`; generated `*_pb2` classes are the canonical message owners, and dynamic classes come from `message_factory.GetMessageClass(descriptor)`, never a hand-built class shim.
- descriptor law: descriptors live in one `descriptor_pool.Default()` pool indexed by `symbol_database.Default()`; a new message type is one `FileDescriptorProto` added to the pool plus one `GetMessageClass`, never a parallel registry.
- binary law: the canonical wire is `SerializeToString()` / `ParseFromString(data)`; the `WireProtoCodec` encode/decode path frames the `CrdtOp*` union through these, and varint/tag framing is never re-implemented locally.
- codec law: `json_format` is the boundary codec for proto-JSON I/O and `text_format` for human-readable diagnostics; both round-trip through the same message instance, so neither is a second message model.
- well-known law: time and dynamic-structure fields use the well-known wrappers — `Timestamp`/`Duration` for instants and spans, `Struct`/`Value`/`ListValue` for dynamic JSON payloads, `Any` for type-URL-tagged embedded messages, `Empty` for unit replies; a parallel local time or dynamic-value struct is deleted.
- version law: gencode/runtime skew is caught by `runtime_version.ValidateProtobufRuntimeVersion` at import of generated modules; the guard is the contract, never a silent version assumption.

[LOCAL_ADMISSION]:
- the runtime transport admits `protobuf` as the message reflection and wire layer behind `WireProtoCodec`; the codec composes `SerializeToString`/`ParseFromString` for the `CrdtOp*` frames and `GetMessageClass` for descriptor-driven dispatch.
- protobuf imports on the cp315 core directly (core-direct promotion, no marker); no companion subprocess lane and no gated band apply.
- the internal builder path (`google.protobuf.internal.builder`) is implementation detail consumed only by generated `*_pb2` modules; runtime owners compose the public `message_factory` / `descriptor_pool` surface, never the internal builder directly.

[RAIL_LAW]:
- Package: `protobuf`
- Owns: Protocol Buffers message reflection, descriptor-pool registration, binary/JSON/text wire codecs, well-known wrapper types, and the runtime-version guard for the wire transport
- Accept: `SerializeToString`/`ParseFromString` binary framing of the `CrdtOp*` union, `message_factory.GetMessageClass` descriptor dispatch, one `descriptor_pool.Default()` + `symbol_database.Default()` registry, `json_format`/`text_format` boundary codecs, the well-known wrappers, `ValidateProtobufRuntimeVersion` at gencode import
- Reject: hand-rolled varint/tag framing, a parallel message-class registry beside the default pool, a local time/dynamic-value struct duplicating the well-known wrappers, direct use of `internal.builder` outside generated modules, silent gencode/runtime version skew

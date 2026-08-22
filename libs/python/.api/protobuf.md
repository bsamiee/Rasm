# [PY_BRANCH_API_PROTOBUF]

`protobuf` owns the `google.protobuf` message runtime beneath the two foreign IRs the branch decodes — the Substrait plan and the ONNX model — whose `_pb2` classes derive from `Message`. It folds binary, JSON, and text codecs over those messages beside the well-known value carriers, and hands the estate's own wire vocabulary to `protobuf-py`.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `protobuf`
- package: `protobuf` (BSD-3-Clause)
- module: `google.protobuf`
- namespaces: `...message`, `...proto`, `...json_format`, `...text_format`, `...unknown_fields`, `...runtime_version`, `...internal.api_implementation`, `...<wkt>_pb2`
- abi: native `upb` C extension by default, `cpp` or pure `python` elected at import through `api_implementation`
- rail: data + compute IR

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message family

| [INDEX] | [SYMBOL]      | [TYPE_FAMILY]  | [CAPABILITY]                                                     |
| :-----: | :------------ | :------------- | :--------------------------------------------------------------- |
|  [01]   | `Message`     | abstract base  | root every `_pb2` message class derives from                     |
|  [02]   | `Error`       | exception base | binary-codec refusal root, disjoint from the JSON and text roots |
|  [03]   | `DecodeError` | exception      | malformed binary wire input                                      |
|  [04]   | `EncodeError` | exception      | message state the binary encoder refuses                         |

[PUBLIC_TYPE_SCOPE]: projection exception roots

| [INDEX] | [SYMBOL]                                | [TYPE_FAMILY]  | [CAPABILITY]                                                   |
| :-----: | :-------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `json_format.Error`                     | exception base | JSON-projection refusal root, subclassing `Exception` direct   |
|  [02]   | `json_format.ParseError`                | exception      | `Parse`/`ParseDict` refusal carrying the offending field path  |
|  [03]   | `json_format.EnumStringValueParseError` | exception      | unknown enum string name, re-wrapped as `ParseError` on escape |
|  [04]   | `json_format.SerializeToJsonError`      | exception      | `MessageToJson`/`MessageToDict` refusal on unrenderable state  |
|  [05]   | `text_format.Error`                     | exception base | text-projection refusal root, subclassing `Exception` direct   |
|  [06]   | `text_format.ParseError`                | exception      | `Parse`/`Merge` refusal carrying line and column               |

[PUBLIC_TYPE_SCOPE]: well-known value carriers

| [INDEX] | [SYMBOL]                                    | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :------------------------------------------ | :------------ | :------------------------------------------------------- |
|  [01]   | `any_pb2.Any`                               | well-known    | type-URL-tagged embedded message                         |
|  [02]   | `timestamp_pb2.Timestamp`                   | well-known    | seconds + nanos UTC instant                              |
|  [03]   | `duration_pb2.Duration`                     | well-known    | signed seconds + nanos span, both slots sharing the sign |
|  [04]   | `struct_pb2.Struct` / `Value` / `ListValue` | well-known    | dynamic JSON-shaped object, scalar cell, and sequence    |
|  [05]   | `field_mask_pb2.FieldMask`                  | well-known    | snake_case field-path set over a message tree            |
|  [06]   | `wrappers_pb2.*Value` / `empty_pb2.Empty`   | well-known    | presence-bearing scalar wrappers and the empty message   |

[PUBLIC_TYPE_SCOPE]: runtime introspection

| [INDEX] | [SYMBOL]                         | [TYPE_FAMILY] | [CAPABILITY]                                                        |
| :-----: | :------------------------------- | :------------ | :------------------------------------------------------------------ |
|  [01]   | `unknown_fields.UnknownFieldSet` | field set     | iterable of `field_number`/`wire_type`/`data` records a decode kept |
|  [02]   | `runtime_version.VersionError`   | exception     | gencode-to-runtime skew refusal                                     |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: Message instance operations

| [INDEX] | [SURFACE]                                            | [SHAPE]  | [CAPABILITY]                                        |
| :-----: | :--------------------------------------------------- | :------- | :-------------------------------------------------- |
|  [01]   | `Message.SerializeToString(*, deterministic)`        | instance | encode to binary wire `bytes`                       |
|  [02]   | `Message.SerializePartialToString(*, deterministic)` | instance | encode without the required-field check             |
|  [03]   | `Message.ParseFromString(serialized)`                | instance | clear, then decode binary wire bytes in place       |
|  [04]   | `Message.MergeFromString(serialized)`                | instance | overlay binary wire bytes onto current state        |
|  [05]   | `Message.FromString(s)`                              | factory  | decode bytes into a new message                     |
|  [06]   | `Message.CopyFrom(other_msg)`                        | instance | replace every field from another message            |
|  [07]   | `Message.MergeFrom(other_msg)`                       | instance | overlay set fields from another message             |
|  [08]   | `Message.Clear()` / `Message.ClearField(field_name)` | instance | reset every field / one field to its default        |
|  [09]   | `Message.HasField(field_name)`                       | instance | presence of a message, oneof member, or `optional`  |
|  [10]   | `Message.WhichOneof(oneof_group)`                    | instance | name of the populated oneof member, else `None`     |
|  [11]   | `Message.ListFields()`                               | instance | set `(FieldDescriptor, value)` pairs in field order |
|  [12]   | `Message.ByteSize()`                                 | instance | encoded size in bytes, caching the result           |
|  [13]   | `Message.IsInitialized()`                            | instance | every proto2 required field populated               |
|  [14]   | `Message.DiscardUnknownFields()`                     | instance | erase the preserved unknown-field set               |
|  [15]   | `Message.SetInParent()`                              | instance | mark an empty submessage present on its parent      |
|  [16]   | `Message.HasExtension(field_descriptor)`             | instance | proto2 extension presence                           |
|  [17]   | `Message.ClearExtension(field_descriptor)`           | instance | reset one proto2 extension                          |
|  [18]   | `Message.DESCRIPTOR`                                 | property | the message's `Descriptor`                          |

- `Message.HasField`: raises `ValueError` on an implicit-presence proto3 scalar and on a name the message never declares.
- `Message.UnknownFields`: `unknown_fields.UnknownFieldSet(message)` reads that set where the `upb` backend raises `NotImplementedError`.

[ENTRYPOINT_SCOPE]: functional codec (`google.protobuf.proto`)
- `proto` mirrors the instance methods without mutation: `parse` returns a NEW message where `ParseFromString` decodes in place.

| [INDEX] | [SURFACE]                                                                 | [SHAPE] | [CAPABILITY]                                 |
| :-----: | :------------------------------------------------------------------------ | :------ | :------------------------------------------- |
|  [01]   | `proto.serialize(message, deterministic) -> bytes`                        | static  | encode a message to `bytes`                  |
|  [02]   | `proto.parse(message_class, payload) -> message`                          | static  | decode bytes into a new message instance     |
|  [03]   | `proto.serialize_length_prefixed(message, output)`                        | static  | write one varint-length-prefixed frame       |
|  [04]   | `proto.parse_length_prefixed(message_class, input_bytes)`                 | static  | read one varint-length-prefixed frame        |
|  [05]   | `proto.byte_size(message) -> int`                                         | static  | encoded size without materializing the bytes |
|  [06]   | `proto.clear_message(message)` / `proto.clear_field(message, field_name)` | static  | reset every field / one field to its default |

[ENTRYPOINT_SCOPE]: JSON projection
- serializer carry: `preserving_proto_field_name`, `use_integers_for_enums`, `always_print_fields_with_no_presence`, `descriptor_pool`, `float_precision`
- parser carry: `ignore_unknown_fields`, `descriptor_pool`, `max_recursion_depth`

| [INDEX] | [SURFACE]                                                             | [SHAPE] | [CAPABILITY]                                |
| :-----: | :-------------------------------------------------------------------- | :------ | :------------------------------------------ |
|  [01]   | `json_format.MessageToJson(message, indent, sort_keys, ensure_ascii)` | static  | render to a JSON string                     |
|  [02]   | `json_format.MessageToDict(message)`                                  | static  | project to a `dict` of JSON-shaped builtins |
|  [03]   | `json_format.Parse(text, message)`                                    | static  | decode a JSON string into `message`         |
|  [04]   | `json_format.ParseDict(js_dict, message)`                             | static  | decode a mapping into `message`             |

[ENTRYPOINT_SCOPE]: text projection
- printer carry: `as_utf8`, `as_one_line`, `use_short_repeated_primitives`, `pointy_brackets`, `use_index_order`, `use_field_number`, `float_format`, `double_format`, `indent`, `message_formatter`, `print_unknown_fields`, `force_colon`, `descriptor_pool`
- parser carry: `allow_unknown_field`, `allow_unknown_extension`, `allow_field_number`, `descriptor_pool`

| [INDEX] | [SURFACE]                                                         | [SHAPE] | [CAPABILITY]                                     |
| :-----: | :---------------------------------------------------------------- | :------ | :----------------------------------------------- |
|  [01]   | `text_format.MessageToString(message) -> str`                     | static  | render to the proto text format                  |
|  [02]   | `text_format.MessageToBytes(message) -> bytes`                    | static  | render that same text as encoded bytes           |
|  [03]   | `text_format.Parse(text, message)` / `ParseLines(lines, message)` | static  | overlay a text block, refusing a repeated scalar |
|  [04]   | `text_format.Merge(text, message)` / `MergeLines(lines, message)` | static  | overlay a text block, the last scalar winning    |

[ENTRYPOINT_SCOPE]: well-known type operations

| [INDEX] | [SURFACE]                                                                   | [SHAPE]  | [CAPABILITY]                                 |
| :-----: | :-------------------------------------------------------------------------- | :------- | :------------------------------------------- |
|  [01]   | `Any.Pack(msg, type_url_prefix, deterministic)` / `Any.Unpack(msg) -> bool` | instance | embed a typed message / extract it           |
|  [02]   | `Any.Is(descriptor) -> bool` / `Any.TypeName() -> str`                      | instance | discriminate on the type URL                 |
|  [03]   | `Timestamp.GetCurrentTime()` / `ToDatetime(tzinfo)` / `FromDatetime(dt)`    | instance | now / `datetime` round-trip                  |
|  [04]   | `Timestamp.ToJsonString()` / `FromJsonString(value)`, same on `Duration`    | instance | RFC3339 instant / `<n>s` span round-trip     |
|  [05]   | `Duration.ToTimedelta()` / `FromTimedelta(td)`                              | instance | `timedelta` round-trip                       |
|  [06]   | `Timestamp.ToNanoseconds()` / `Duration.FromNanoseconds(nanos)`, 4 rungs    | instance | integral ladder shared by both carriers      |
|  [07]   | `Struct.update(dictionary)` / `keys()` / `values()` / `items()` / `[key]`   | instance | dynamic object read and bulk write           |
|  [08]   | `Struct.get_or_create_struct(key)` / `Struct.get_or_create_list(key)`       | instance | mint or reach a nested container             |
|  [09]   | `ListValue.append(value)` / `ListValue.extend(elem_seq)`                    | instance | sequence append                              |
|  [10]   | `ListValue.add_struct()` / `ListValue.add_list()`                           | instance | mint a nested container in place             |
|  [11]   | `FieldMask.FromJsonString(value)` / `FieldMask.ToJsonString()`              | instance | lowerCamel path-list round-trip              |
|  [12]   | `FieldMask.AllFieldsFromDescriptor(d)` / `IsValidForDescriptor(d)`          | instance | seed a mask from a descriptor / validate one |
|  [13]   | `FieldMask.Union(mask1, mask2)` / `Intersect(mask1, mask2)`                 | instance | set algebra into the receiver                |
|  [14]   | `FieldMask.CanonicalFormFromMask(mask)`                                     | instance | canonicalize an overlapping path set         |
|  [15]   | `FieldMask.MergeMessage(source, destination, ...)`                          | instance | copy the masked paths across messages        |

- [15]-[MERGE_MESSAGE]: `FieldMask.MergeMessage(source, destination, replace_message_field, replace_repeated_field)`

[ENTRYPOINT_SCOPE]: backend and gencode gates

| [INDEX] | [SURFACE]                                                   | [SHAPE] | [CAPABILITY]                             |
| :-----: | :---------------------------------------------------------- | :------ | :--------------------------------------- |
|  [01]   | `api_implementation.Type() -> str`                          | static  | active `upb`, `cpp`, or `python` backend |
|  [02]   | `unknown_fields.UnknownFieldSet(message)`                   | ctor    | iterate the unknown fields a decode kept |
|  [03]   | `ValidateProtobufRuntimeVersion(gen_domain, ..., location)` | static  | refuse gencode skew                      |

- [03]-[GENCODE_GATE]: `runtime_version.ValidateProtobufRuntimeVersion(gen_domain, gen_major, gen_minor, gen_patch, gen_suffix, location)`

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `api_implementation.Type()` reports the backend elected at import and the native `upb` extension backs every instance method; that backend refuses `Message.UnknownFields()`, so `unknown_fields.UnknownFieldSet(message)` is the branch's one unknown-field reader.
- `deterministic` rides both binary encoders by KEYWORD alone — a positional argument refuses — and pins map and unknown-field ordering so a hash or cache key over the bytes reproduces; the default order is implementation-defined.
- `ParseFromString` clears the message then merges, `MergeFromString` overlays onto current state, and `proto.parse` returns a new message leaving its input untouched.
- `ByteSize()` and `proto.byte_size` return exactly the length `SerializeToString()` produces and cache it, so a size gate never pays a second encode.
- Binary round-trips carry unknown fields through untouched while `MessageToDict` and `MessageToString` drop them, so a JSON transcode hop erases every field the local descriptor never learned.
- `HasField` answers presence for message-typed fields, oneof members, and proto2 or `optional` fields, raising `ValueError` on an implicit-presence proto3 scalar; `WhichOneof` names the populated member or `None`.
- `ignore_unknown_fields=True` swallows an unknown enum STRING name beside an unknown field name and leaves that field at its default, so a lax intake drops enum data the strict default refuses.
- `always_print_fields_with_no_presence` fills implicit-presence proto3 fields alone, and a proto2 or `optional` field carrying explicit presence stays absent from the projection.
- Three refusal roots subclass `Exception` directly and none subclasses another — `message.Error` on the binary leg, `json_format.Error` on the JSON leg, `text_format.Error` on the text leg.
- `SerializeToString` raises `EncodeError` on a proto2 message missing a required field where `SerializePartialToString` emits the partial bytes; `IsInitialized()` reads that state ahead of either.
- `text_format.Parse` and `Merge` both overlay without clearing, `Parse` refusing a second assignment to one scalar where `Merge` takes the last.
- Every `datetime` and `timedelta` crossing truncates to microseconds — `FromDatetime`, `ToDatetime`, `FromTimedelta`, `ToTimedelta`, and `GetCurrentTime` alike — where `FromNanoseconds`/`ToNanoseconds` and the JSON string forms carry the full nanosecond slot.
- `FieldMask` stores snake_case paths while `ToJsonString` emits lowerCamel, so the mask's own JSON form and a `preserving_proto_field_name` projection spell one path two ways.
- `ValidateProtobufRuntimeVersion` runs at every generated-module import and raises `VersionError` on skew, so the `substrait` and `onnx` `_pb2` modules set the runtime floor this branch satisfies.

[STACKING]:
- `substrait`(`libs/python/data/.api/substrait.md`): `substrait.proto.Plan` is a `_pb2` `Message`; the admission gate runs `Plan.ParseFromString(wire)` and answers `message.DecodeError` as the `PlanRefusal.UNPARSEABLE` row, reads `relations`/`extension_urns` off the parsed message through `HasField`/`WhichOneof`, and re-emits with `SerializeToString`/`ByteSize`.
- `onnx`(`libs/python/compute/.api/onnx.md`): `ModelProto` and `GraphProto` are `_pb2` messages; `onnx.load_model_from_string(s)` lands the model through `ParseFromString`, and `model.SerializeToString()` is the byte handoff `onnxruntime.InferenceSession` consumes past the checker gate.
- `confluent-kafka`(`.api/confluent-kafka.md`): `ProtobufSerializer(msg_type, schema_registry_client)` calls `Message.SerializeToString()` beneath its magic-byte and message-index framing while `ProtobufDeserializer(message_type)` calls `ParseFromString` and answers a bad frame with `message.DecodeError`; the branch hands it messages and never frames bytes itself.
- `opentelemetry-exporter-otlp-proto-http`(`.api/opentelemetry-exporter-otlp-proto-http.md`): `OTLPSpanExporter.export(spans)` encodes the SDK batch into an OTLP `_pb2` request and POSTs its `SerializePartialToString()` bytes, so this runtime is the encode engine under every signal exporter.
- `protobuf-py`(`.api/protobuf-py.md`): the two runtimes meet on descriptor bytes alone — a `FileDescriptorSet.SerializeToString()` from this runtime feeds `protobuf.wkt.FileDescriptorSet.from_binary(...).to_registry()` so a Substrait or ONNX schema reads as `DescMessage` views on the estate rail; no estate fence transcodes between the two message families.

[LOCAL_ADMISSION]:
- `_pb2` classes arrive from the admitted IR distributions alone; no first-party `.proto` compiles against this runtime, and `protobuf-py` mints every estate wire message.
- `proto.serialize`/`proto.parse` carry the non-mutating path, and `ParseFromString` earns its call only where a caller reuses a pre-allocated message.
- Codec fences name every refusal root their leg touches, since `message.Error` and `json_format.Error` stand disjoint and a catch naming one alone lets the other past the rail.
- Emit reads `MessageToDict(preserving_proto_field_name=True)` so the mapping keys match the proto field names the interior model already spells.
- Unknown-field census rides `unknown_fields.UnknownFieldSet`, and `DiscardUnknownFields()` marks the deliberate erase a re-emit declares.

[RAIL_LAW]:
- Package: `protobuf`
- Owns: `google.protobuf` binary, JSON, and text codecs with the well-known value carriers beneath the Substrait plan and ONNX model IRs, on the native `upb` backend
- Accept: `_pb2` messages from `substrait` and `onnx`, `proto.serialize`/`parse` and their length-prefixed pair, `json_format.ParseDict` at intake, `MessageToDict(preserving_proto_field_name=True)` at emit, well-known types as value carriers
- Reject: first-party `_pb2` emission, `google.protobuf` on the estate wire rail, hand-rolled binary encoding, a positional `deterministic`, `Message.UnknownFields()` where the `upb` backend refuses it, the pure-`python` backend where the native extension loads

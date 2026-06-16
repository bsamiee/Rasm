# [PY_RUNTIME_API_PROTOBUF]

`protobuf` supplies the Python Protocol Buffers runtime backing the companion gRPC server: the `Message` base, the descriptor and descriptor-pool surfaces, the symbol database, JSON/text format conversion, the message factory, and the well-known wrapper types. It is the runtime owner for protobuf message handling under the gRPC seam.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `protobuf`
- package: `protobuf`
- import: `google.protobuf`
- version: `6.33.6`
- owner: `runtime`
- rail: transport
- namespaces: `google.protobuf.message`, `google.protobuf.descriptor`, `google.protobuf.descriptor_pool`, `google.protobuf.symbol_database`, `google.protobuf.json_format`, `google.protobuf.text_format`, `google.protobuf.message_factory`, `google.protobuf.timestamp_pb2`, `google.protobuf.duration_pb2`, `google.protobuf.struct_pb2`, `google.protobuf.any_pb2`, `google.protobuf.wrappers_pb2`, `google.protobuf.field_mask_pb2`, `google.protobuf.empty_pb2`
- capability: message base, descriptors/pools, symbol database, JSON/text conversion, message factory, well-known types
- admission note: pinned with `python_version<'3.13'` in the root manifest; under `requires-python='>=3.15'` it is present only as a marker-free transitive dependency of `specklepy` at major 6 (the manifest names `protobuf>=7.35.1`). First-class admission for the companion server requires the floor/lock-scope decision recorded in the suite TASKLOG.

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: message and descriptor family
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `message.Message` | message base | generated-message base class |
| [2] | `descriptor.Descriptor` | descriptor | message descriptor |
| [3] | `descriptor.FieldDescriptor` | descriptor | field descriptor |
| [4] | `descriptor.EnumDescriptor` | descriptor | enum descriptor |
| [5] | `descriptor.FileDescriptor` | descriptor | file descriptor |
| [6] | `descriptor.ServiceDescriptor` | descriptor | service descriptor |
| [7] | `descriptor_pool.DescriptorPool` | pool | descriptor registry |
| [8] | `symbol_database.SymbolDatabase` | registry | symbol-to-message map |
| [9] | `message_factory.MessageFactory` | factory | descriptor-to-class factory |

[PUBLIC_TYPE_SCOPE]: well-known and fault family
- rail: transport

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [RAIL] |
| :-----: | :------- | :------------ | :----- |
| [1] | `timestamp_pb2.Timestamp` | well-known | RFC-3339 timestamp |
| [2] | `duration_pb2.Duration` | well-known | signed duration |
| [3] | `struct_pb2.Struct` | well-known | dynamic struct |
| [4] | `any_pb2.Any` | well-known | type-erased message |
| [5] | `wrappers_pb2.StringValue` | well-known | boxed scalar |
| [6] | `field_mask_pb2.FieldMask` | well-known | field selector |
| [7] | `empty_pb2.Empty` | well-known | empty message |
| [8] | `message.DecodeError` | fault | malformed wire bytes |
| [9] | `message.EncodeError` | fault | unencodable message |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: message operations
- rail: transport

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [RAIL] |
| :-----: | :-------- | :------------- | :----- |
| [1] | `Message.SerializeToString` | encode | message to wire bytes |
| [2] | `Message.ParseFromString` | decode | wire bytes into a message |
| [3] | `Message.MergeFrom` | merge | merge another message |
| [4] | `Message.CopyFrom` | copy | replace contents |
| [5] | `Message.WhichOneof` | oneof | active oneof field |
| [6] | `json_format.MessageToJson` | convert | message to JSON |
| [7] | `json_format.Parse` | convert | JSON to message |
| [8] | `text_format.MessageToString` | convert | message to text format |
| [9] | `message_factory.GetMessageClass` | factory | class from a descriptor |
| [10] | `Timestamp.FromDatetime` / `ToDatetime` | well-known | datetime conversion |

## [4]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- message law: wire messages are generated classes from the C# `.proto` descriptors; the runtime never hand-writes a `Message` subclass.
- conversion law: the seam converts protobuf messages to the branch's canonical msgspec/pydantic shapes at ingress and back at egress; protobuf types never leak into interior domain logic.
- time law: protobuf `Timestamp`/`Duration` convert to the branch's semantic time values at the seam, never propagated as raw protobuf time.
- fault law: `DecodeError`/`EncodeError` are lifted into `Error(BoundaryFault(...))` at the seam.
- well-known law: dynamic payloads use `Struct`/`Any` only at the seam; interior code carries typed shapes.

[LOCAL_ADMISSION]:
- protobuf backs the `grpcio` companion server; this page owns the message-handling surface, `.api/api-grpcio.md` owns the transport surface.
- Generated message classes arrive from the proto compilation step; the runtime composes serialize/parse/convert over them.

[RAIL_LAW]:
- Package: `protobuf`
- Owns: protobuf message handling for the companion gRPC seam — generated messages, serialization, JSON/text conversion, and well-known types
- Accept: generated `Message` classes, serialize/parse, seam conversion to canonical shapes, well-known time conversion, boundary-lifted faults
- Reject: hand-written `Message` subclasses, protobuf-type leakage into domain logic, raw protobuf time propagation

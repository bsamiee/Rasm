# [PY_RUNTIME_API_PROTOBUF]

`protobuf` supplies the Protocol Buffers message runtime — descriptor-pool reflection, the `proto.*` functional codec façade, `json_format`/`text_format`, and the well-known wrapper types. Integration overlay over the canonical branch catalog `libs/python/.api/protobuf.md`, which owns the full `Message`/descriptor/`proto`/`json_format`/well-known/`FieldDescriptor` surface and the generic OTLP/boundary/streaming law. This overlay carries only the runtime-specific delta: the `WireProtoCodec` `CrdtOp*` composition, the `descriptor_pool` drift-gate, and the msgspec-envelope seam.

## [01]-[LOCAL_ADMISSION]

[LOCAL_ADMISSION]:
- The `transport/wire` surface admits `protobuf` as the message reflection and inner-op wire layer behind `WireProtoCodec`; the runtime owns no second message reflection path.
- Generated `rasm.runtime._pb2` modules are the only concrete message-class source; runtime owners compose the public `message_factory`/`descriptor_pool` surface. The `google.protobuf.internal.builder` path is banned outside the generated modules.
- `runtime_version.ValidateProtobufRuntimeVersion` at generated-module import is the gencode/runtime skew gate — never a silent version assumption.

## [02]-[RUNTIME_DELTA]

[DELTA_SCOPE]: `CrdtOp*` frame codec (`transport/wire#WireProtoCodec`)
- The codec frames the `CrdtOp*` union through one `proto.serialize(message, deterministic=True)`/`proto.parse(message_class, payload)` call surface keyed by message class; `message_factory.GetMessageClass(descriptor)` supplies descriptor-driven dispatch, and `proto.serialize_length_prefixed`/`parse_length_prefixed` frame a stream of ops over one buffered channel.

[DELTA_SCOPE]: descriptor-pool drift-gate
- The generated `rasm.runtime._pb2` package registers into `descriptor_pool.Default()`; the offline-wire drift gate resolves `DescriptorPool.FindMessageTypeByName(full_name)` and asserts descriptor parity against the C#-minted `EvidenceBundle` golden fixture (the compute-campaign codegen drift proof) — a schema-drift assertion, never a runtime encode path.

[INTEGRATION_STACK]:
- msgspec seam: the msgspec `Struct` envelope holds the op id/actor/lamport and the protobuf frame is the opaque `bytes` value `proto.serialize`/`proto.parse` produce and consume — msgspec owns the envelope schema, protobuf owns the inner op wire, never two parallel schemas for one op.
- grpc seam: protobuf message classes are the request/response types of the `grpc.aio` stubs, with `proto.serialize`/`proto.parse` the channel serializer/deserializer; the `opentelemetry-instrumentation-grpc` interceptor's `request_hook`/`response_hook` receive these message instances for span enrichment.
- well-known time seam: `Timestamp.FromDatetime`/`ToDatetime` and `Duration.FromTimedelta`/`ToTimedelta` bridge the wire to the host clock, and `Any.Pack`/`Any.Is`/`Any.Unpack` carry a polymorphic embedded op resolved by descriptor, never a stringly type tag.

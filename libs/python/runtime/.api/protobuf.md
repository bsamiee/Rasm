# [PY_RUNTIME_API_PROTOBUF]

Full surface and stacking: `libs/python/.api/protobuf.md` (shared-tier canonical owner).

`protobuf` supplies the Protocol Buffers message runtime: descriptor-pool reflection, the `proto.*` functional codec facade, `json_format`/`text_format`, and the well-known wrapper types. This overlay carries only the runtime-specific delta: the `WireProtoCodec` `CrdtOp*` composition, the `descriptor_pool` drift-gate, and the msgspec-envelope seam.

## [01]-[LOCAL_ADMISSION]

[LOCAL_ADMISSION]:
- The `transport/wire` surface admits `protobuf` as the message reflection and inner-op wire layer behind `WireProtoCodec`; the runtime owns no second message reflection path.
- Generated `rasm.runtime._pb2` modules are the only concrete message-class source; runtime owners compose the public `message_factory`/`descriptor_pool` surface. The `google.protobuf.internal.builder` path is banned outside the generated modules.
- `runtime_version.ValidateProtobufRuntimeVersion` at generated-module import is the gencode/runtime skew gate — never a silent version assumption.

## [02]-[RUNTIME_DELTA]

[DELTA_SCOPE]: `CrdtOp*` frame codec (`transport/wire#WireProtoCodec`)
- The codec frames the `CrdtOp*` union through one `proto.serialize(message, deterministic=True)`/`proto.parse(message_class, payload)` call surface keyed by message class; `message_factory.GetMessageClass(descriptor)` supplies descriptor-driven dispatch, and `proto.serialize_length_prefixed`/`parse_length_prefixed` frame a stream of ops over one buffered channel.

[DELTA_SCOPE]: descriptor-pool drift-gate (`transport/shapes#aligned`)
- Importing the generated `rasm.runtime._pb2.channels_pb2` registers its serialized file into `descriptor_pool.Default()`; `transport/shapes#aligned` — run once by the daemon composition root before serve binds — resolves `DescriptorPool.FindMessageTypeByName(message.DESCRIPTOR.full_name)` per `PROTO_VOCABULARY` row (a pool read, never an `AddSerializedFile` re-registration) and cross-checks the compiled `Descriptor` against the struct's `msgspec.inspect` field set: absent slot (a producer field the companion silently drops), phantom slot (a slot that decodes to its default forever), and the 64-bit floor (every `TYPE_*64` slot keyed off the `FieldDescriptor` type constants lands on an `inspect.IntType` carrying the `WireU64` `ge=0` floor).
- The gate proves STRUCTURE, never byte/value parity — round-trip byte-stability is the `evidence/reproduction#SEED_REPRODUCTION` corpus's, whose `evidence-bundle` row holds the C#-minted golden bundle backing the compute campaign's offline codegen proof.

[INTEGRATION_STACK]:
- msgspec seam: the msgspec `Struct` envelope holds the op id/actor/lamport and the protobuf frame is the opaque `bytes` value `proto.serialize`/`proto.parse` produce and consume — msgspec owns the envelope schema, protobuf owns the inner op wire, never two parallel schemas for one op.
- grpc seam: protobuf message classes are the request/response types of the `grpc.aio` stubs, with `proto.serialize`/`proto.parse` the channel serializer/deserializer; the `opentelemetry-instrumentation-grpc` interceptor's `request_hook`/`response_hook` receive these message instances for span enrichment.
- well-known time seam: the Timestamp JSON mapping renders `hlc_physical` as the RFC 3339 `Stamp` slot `json_format.MessageToDict` emits; the causal lift stays `clock#CLOCK`'s, reconstructing `Hlc` at full 100-ns tick fidelity from its own carrier slots — a `Timestamp.FromDatetime`/`ToDatetime` bridge truncates to microseconds and is the deleted form. `Any` `@type`-keyed envelopes decode as `transport/shapes#VOCABULARY` `Packed` mappings the document verb owner dispatches on the `@type` discriminant, never an `Any.Unpack` re-materialization beside the shapes vocabulary.

# [PY_RUNTIME_API_MSGSPEC]

`msgspec` supplies the runtime's in-memory frame layer — `Struct` records, hook-bearing JSON/MessagePack codecs, `Meta` constraint validation, and the `to_builtins`/`convert` lowering-raising pair. Integration overlay over the canonical branch catalog `libs/python/.api/msgspec.md`, which owns the full `Struct`/codec/`Meta`/`defstruct`/`inspect` surface and the generic pydantic/otel/grpc/numpy stacks. This overlay carries only the runtime-specific delta: the `WireProtoCodec` proto3-JSON bridge, the `CrdtOpDecode` msgpack `ext_hook` leg, and the fault-boundary + leaf-cell law the transport and clock cells compose.

## [01]-[LOCAL_ADMISSION]

[LOCAL_ADMISSION]:
- The runtime defines every canonical wire/frame shape as a `Struct` subclass minted once in `transport/shapes`; no page hand-rolls JSON validation or a parallel DTO for a shape `shapes` already owns.
- `frozen=True` is the default for every wire `Struct`; `structs.force_setattr` is reserved for the decode-time post-init hook. `gc=False` applies only to leaf cells holding no container field — `clock#CLOCK`'s `Hlc`/`ElementId` — dropping them from the tracked GC set on the high-allocation clock path.
- Catch `DecodeError`/`ValidationError`/`EncodeError` only inside the `faults#FAULT` `boundary` conversion, never in domain flow; `ValidationError` carries the constraint path, is terminal (never retried under `stamina`), and lifts to a `BoundaryFault` once at egress.

## [02]-[RUNTIME_DELTA]

[DELTA_SCOPE]: proto3-JSON bridge (`transport/wire#WireProtoCodec`)
- The proto3-JSON seam pairs msgspec lowering with protobuf's `json_format`: encode lowers the canonical `Struct` via `to_builtins(struct, order="deterministic")` into a builtins mapping that `json_format.ParseDict` raises into the `*_pb2` message; decode runs `MessageToDict(msg, preserving_proto_field_name=True)` then `convert(dict, StructType, strict=False)`.
- `strict=False` on the raise leg is the runtime-specific law: proto3 canonical JSON widens scalars (int64 as string, enum as name), and lax `convert` coerces those widened forms back into the typed `Struct` — the one place the runtime departs from the branch tier's strict default.

[DELTA_SCOPE]: op-log decode (`transport/wire#CrdtOpDecode`)
- A cached `msgpack.Decoder` decodes the op-log delta with an `ext_hook` reconstructing the binary lattice clock from a `msgpack.Ext` payload; a `Raw`-typed envelope field defers the inner op decode until the tag routes it, and `Raw.copy()` detaches the buffer view so the outer input releases.

[INTEGRATION_STACK]:
- The canonical decode is `msgspec.Decoder(type=<tagged Struct union from shapes>, dec_hook=<lift>)` feeding a validated frame, retried under a `stamina` `retry_context` for transport faults only, inside an `opentelemetry-api` span; the same `Struct` admits through `pydantic-settings`-validated config at a distinct seam, so msgspec owns the wire shape and pydantic owns the config shape with no second model.

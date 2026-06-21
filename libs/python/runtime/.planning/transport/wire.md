# [PY_RUNTIME_WIRE]

The single wire-codec owner the companion transport composes. `WireProtoCodec` transcodes the canonical `msgspec` `Struct` shapes interior code holds to the generated protobuf `Message` the C# gRPC services carry and back — `protobuf 6.33`-backed through the message-agnostic `google.protobuf.proto` functional codec façade (`proto.serialize`/`proto.parse`), never a hand-rolled varint; `WIRE_REGISTRY` is the one data table binding each canonical `(Struct, Message)` pair under its semantic name so the seventeen-message family is rows, not seventeen hand clients. `CrdtOp` is the field-less `array_like` tagged root and `CrdtArm` the closed ten-arm union (`SetOp`…`LeaveOp`, tag 0-9) the companion decodes from the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` owner — each arm dense and append-only, carrying the flat producer `[Key(k)]` slots and exposing the `clock#CLOCK` `Hlc` cell and `ElementId` it reconstructs as derived causal views, so the canonical op IS the wire arm and no parallel wire-vs-canonical hierarchy or hand-written lift match survives. `CrdtOpDecode` is the one decode surface dispatching on the `array_like` integer tag through the msgspec tagged-union core, lifting the MessagePack delta the `OpLogEntry.Payload` carries for `column-family=crdt` rows; the op-log crosses as MessagePack under a `Lz4BlockArray` envelope, distinct from the gRPC proto wire — `decompress` is a dependency-injected `DecompressFn` seam the decode closes over, never a hardwired `lz4` import (LZ4 is gated `python_version<'3.15'` and the compressed-envelope decode is deferred). Both decodes ride the one `Decode` aspect — `railed` scopes the buffered decode under an `opentelemetry-api` CONSUMER span inside the `reliability/faults#FAULT` `boundary` fence, and the network-fed `acquired` leg delegates the `reliability/resilience#RESILIENCE` `RetryClass.WIRE` retried acquisition to the resilience owner's `guarded` aspect before binding `railed`, so span and fault conversion are one woven flow on every ingress and retry is the resilience owner's concern reached by composition, never a loop repeated inline per codec. The page owns the companion's decode of every C#-minted wire shape and mints no wire vocabulary; `msgspec` is the in-memory frame layer and `protobuf` the wire. `WireProtoCodec`/`CrdtOp*`/`CrdtOpDecode` formerly mis-lived in `transport/serve`; they collapse here so the serve lifecycle references one codec owner rather than carrying the full codec body inline.

## [01]-[INDEX]

- [01]-[WIRE_RAIL]: the one traced-railed `Decode` aspect every wire ingress composes — a CONSUMER OTel span + `boundary` fault fence over `msgspec.Decoder`/`proto.parse` for the buffered leg, the network-fed leg delegating retry to `reliability/resilience#RESILIENCE` `guarded(RetryClass.WIRE)` rather than re-implementing the loop, woven once.
- [02]-[PROTO_TRANSCODE]: the registry-driven msgspec-canonical to protobuf-wire codec at the gRPC seam, the `WIRE_REGISTRY` row table over the seventeen-message family, the inbound `FaultDetail` decode carrying the `hlc_physical`/`hlc_logical`/`tenant` fields the serve owner lifts.
- [03]-[CRDT_DECODE]: the MessagePack op-log `CrdtArm` discriminated union over the `CrdtOp` array-like tagged root with derived `Hlc`/`ElementId` views, the tag-dispatched decoder, the single-op and op-stream output modalities, and the dependency-injected decompress seam (the durability codec, NOT protobuf).

## [02]-[WIRE_RAIL]

- Owner: `Decode` — the one cross-cutting wire-ingress aspect every codec on this page composes, folding the four wire concerns into a single woven rail: a reusable `msgspec.Decoder` (or the protobuf `proto.parse` façade) carries the bytes-to-`Struct` decode, the `reliability/resilience#RESILIENCE` `RetryClass.WIRE` row retries the transient transport leg, an `opentelemetry-api` CONSUMER span scopes the decode, and the `reliability/faults#FAULT` `boundary("wire", ...)` conversion lands a malformed frame as `Error(BoundaryFault(boundary=("wire", <ExcType>)))`. The aspect is the AOP collapse of the `msgspec.md` IMPLEMENTATION_LAW integration rail — `msgspec.Decoder(type=<tagged union>, dec_hook=<lift>)` feeding a validated frame, retried under a `stamina` `retry_context` (transport faults only, `ValidationError` terminal), inside a span — so retry, telemetry, and fault conversion are declared once and reused by both the proto transcode and the CRDT decode, never repeated inline per codec.
- Entry: `Decode.railed(subject, decode)` is the synchronous fold for the already-buffered proto/CRDT payload — it opens the `trace.get_tracer("rasm.wire").start_as_current_span(f"wire.decode.{subject}", kind=SpanKind.CONSUMER)` scope, runs the pure `decode` thunk inside the one `boundary("wire", ...)` fence, and on the `Error` arm enriches the span through `Span.set_status(Status(StatusCode.ERROR))` before returning the rail; `Decode.acquired(subject, fetch, decode)` is the awaitable fold for the network-fed leg — it delegates the retried acquisition wholesale to `reliability/resilience#RESILIENCE` `guarded(RetryClass.WIRE, fetch, subject="wire")`, which already fuses the `WIRE`-row retry (`attempts=5`/`timeout=15.0`/`on=(ConnectionError,)`, the only retried surface), the retry span, and the one-shot `async_boundary` terminal-fault lift into a single `RuntimeRail[bytes]`, then `bind`s the railed `decode` so the buffered-frame leg crosses the same `railed` CONSUMER-span/`boundary` fence; the transient connection fault is retried by the resilience owner and the terminal decode `ValidationError` rides the `railed` boundary on the first decode without a retry. `acquired` mints no second try/except, no second span open, and no second `record_exception` — the retry-and-acquire concern is the resilience owner's `guarded` aspect and the decode-and-annotate concern is `railed`, composed by `bind` rather than re-implemented. The two entries share one span-naming, one fault subject, and one decoder seam — the sync/awaitable split is the runtime axis, never a parallel telemetry or fault rail.
- Auto: the span attributes seed from the decoded `Struct` with no manual flattening — `Decode.annotated(span, frame)` calls `span.set_attributes(msgspec.to_builtins(frame, str_keys=True))` (the `opentelemetry-api.md` INTEGRATION line: `to_builtins(struct, str_keys=True)` produces exactly the `str | bool | int | float | Sequence[...]` mapping `Span.set_attributes` accepts), so a flame graph carries the wire frame's primitive fields per decode; the retry hook is the branch-wide `reliability/resilience#RESILIENCE` `instrument()` `StructlogOnRetryHook` registered once, never a per-codec retry log; the `RetryClass.WIRE` row is the sole retry policy this page reads, so a new wire retry geometry is one column on the `reliability/resilience#RESILIENCE` `Retry` row, never a knob here.
- Packages: `msgspec` (`msgpack.Decoder`/`json.Decoder` reusable codecs ENTRYPOINTS [09]/[02], `to_builtins(str_keys=True)` projection ENTRYPOINTS [06], `dec_hook` seam, `ValidationError`/`DecodeError` PUBLIC_TYPES [04]/[03]), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span(kind=SpanKind.CONSUMER)`/`Span.set_attributes`/`Span.set_status`/`SpanKind`/`Status`/`StatusCode` — branch catalog `libs/python/.api/opentelemetry-api.md` trace ENTRYPOINTS [03]/[10]/[12]/[15] + PUBLIC_TYPES [05]/[06]/[07]), `reliability/resilience#RESILIENCE` (`guarded(RetryClass.WIRE)` — the retried-traced-railed acquisition the network-fed leg delegates to; the `WIRE` row, the `stamina` caller, the retry span, and the terminal-fault lift are all the resilience owner's, never re-spelled here), `reliability/faults#FAULT` (`boundary`/`RuntimeRail`).
- Growth: a new wire ingress (a new proto pair, a new durability arm, a future op-stream batch) composes `Decode.railed`/`Decode.acquired` and inherits the span, the `RetryClass.WIRE` retry, and the `boundary` fault with zero new cross-cutting code; a new retried transport class is one `reliability/resilience#RESILIENCE` `RetryClass` row, never a second retry path here; a new span dimension is one `set_attributes` key in `annotated`.
- Boundary: the wire-ingress concern is one aspect — a per-codec inline `try`/`except`, a per-codec hand-rolled `retry` loop (the `stamina` Reject-row manual-loop violation), a per-codec span open without the shared naming, a `RetryClass.WIRE` reconstruction instead of the `guarded` delegation, an `acquired` that re-implements the retry/span/`async_boundary` triplet inline rather than delegating to the resilience owner's `guarded` aspect (the duplicated-rail violation), and a retried `ValidationError` (the `msgspec`/`stamina` terminal-validation Reject-row) are the deleted forms; the buffered leg crosses only the `railed` span/`boundary` fence and the network-fed leg crosses `guarded` then `railed`, so the retried transient exception the `WIRE` row names is the resilience owner's concern and the terminal decode fault converts exactly once at the `boundary` fence, never a bare exception across the servicer and never a second async rail.

```python signature
from collections.abc import Awaitable, Callable
from typing import assert_never

import msgspec
from expression import Error, Ok
from opentelemetry import trace
from opentelemetry.trace import Span, SpanKind, Status, StatusCode

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.resilience import RetryClass, guarded

_TRACER = trace.get_tracer("rasm.wire")


class Decode:
    @staticmethod
    def annotated(span: Span, frame: object) -> None:
        span.set_attributes(msgspec.to_builtins(frame, str_keys=True))

    @classmethod
    def railed[T](cls, subject: str, decode: Callable[[], T]) -> RuntimeRail[T]:
        with _TRACER.start_as_current_span(f"wire.decode.{subject}", kind=SpanKind.CONSUMER) as span:
            match boundary("wire", decode):
                case Ok(frame) as ok:
                    cls.annotated(span, frame)
                    return ok
                case Error(fault) as err:
                    span.set_status(Status(StatusCode.ERROR, fault.tag))
                    return err
                case _ as unreachable:
                    assert_never(unreachable)

    @classmethod
    async def acquired[T](cls, subject: str, fetch: Callable[[], Awaitable[bytes]], decode: Callable[[bytes], T]) -> RuntimeRail[T]:
        acquired = await guarded(RetryClass.WIRE, fetch, subject="wire")
        return acquired.bind(lambda payload: cls.railed(subject, lambda: decode(payload)))
```

## [03]-[PROTO_TRANSCODE]

- Owner: `WireProtoCodec` — the one boundary codec generic over the canonical `msgspec` `Struct` type and its paired generated protobuf `Message` class, transcoding interior shapes to the wire and back across the gRPC seam through the message-agnostic `google.protobuf.proto` functional façade so interior code never touches a `Message` and the wire never sees a `Struct`; it mints no second wire vocabulary, owning only the projection. `WIRE_REGISTRY` is the one data table binding each canonical pair under its semantic name — the seventeen-message family is rows the codec resolves by name, never seventeen hand clients.
- Entry: `WireProtoCodec.encode` projects a canonical `Struct` to wire bytes — `msgspec.to_builtins(value, str_keys=True)` lowers the struct to a JSON-compatible mapping, `json_format.ParseDict` fills a fresh generated `Message` (snake_case field names preserved against the proto schema), and `proto.serialize(message, deterministic=True)` emits the canonical deterministic protobuf binary (the `protobuf.md` binary-law façade, the bound `SerializeToString` it delegates to never called directly); `WireProtoCodec.decode` reverses it on the `[02]-[WIRE_RAIL]` `Decode.railed` rail — `proto.parse(self._message, payload)` constructs and populates a fresh `Message` from the wire bytes, `json_format.MessageToDict(message, preserving_proto_field_name=True)` projects it to a mapping, and `msgspec.convert(mapping, self._struct)` validates that mapping into the typed canonical `Struct`, a raised `msgspec.ValidationError` or protobuf decode error crossing the one `reliability/faults#FAULT` `boundary("wire", ...)` so the codec failure lands as `Error(BoundaryFault(boundary=("wire", <ExcType>)))`. The module-level `codec(name)` resolves the named pair to its `WireProtoCodec` on the rail through `WIRE_REGISTRY.try_find(name).to_result(BoundaryFault(wire=(name, 0)))` (an absent name a typed code-carrying `wire` fault, the `reliability/faults#FAULT` `wire` case reserved for the explicit numeric-code construction), and `WireProtoCodec.encode`/`.decode` is the one polymorphic transcode every servicer and the capability invoke read.
- Auto: one polymorphic `encode`/`decode` pair discriminates on the type arguments `[S: Struct, M: Message]`, never a per-message hand client; `to_builtins`/`convert` are the `msgspec` lowering/raising pair keeping the canonical shape the single source of truth, and `ParseDict`/`MessageToDict(preserving_proto_field_name=True)` hold the snake_case field parity the proto schema and the `Struct` share; the binary leg is the `proto.serialize`/`proto.parse` message-agnostic façade keyed by message class (one codec call surface, the `protobuf.md` RAIL_LAW per-message-encode-helper Reject-row deleted), and `proto.serialize_length_prefixed`/`proto.parse_length_prefixed` is the one framed-stream path a future multi-frame channel reads, never a hand-rolled size header. The generated `_pb2` `Message` classes arrive from `grpcio-tools` `command.build_package_protos(strict_mode=True)` compiling the C# `.proto` descriptors AOT into a tracked package (the `grpcio-tools.md` LOCAL_ADMISSION build-time-only law, gated `python_version<'3.15'`, never compiled in the production server), the descriptors resolving in the one `descriptor_pool.Default()` pool `symbol_database.Default()` indexes; the codec authors no message shape, decoding through the message-class `proto.parse` factory. `WIRE_REGISTRY` is the row table the `transport/serve#SERVE` servicers and `transport/serve#CAPABILITY_INVOKE` resolve by name — the concrete pairs decode the producer messages field-for-field across the C# `Rasm.Compute/Runtime/channels#PROTO_VOCABULARY` owner's eighteen-rpc/seventeen-message family: `TransactionRequest`/`TransactionReceipt` (the `DocumentService.ExecuteTransaction` parity seam), `QueryRequest`/`QueryResponse`, `InferRequest`/`InferResponse`, `SolveRequest`/`SolveResponse`, `GenerateRequest`/`TokenChunk`, `GraphDiffRequest`/`GraphDiffResponse`, `SubtreeFetchRequest`/`GraphChunk`, `ArtifactFrame`, and `FaultDetail`.
- Packages: `protobuf` (`proto.serialize(deterministic=True)` functional-façade ENTRYPOINTS [01] / `proto.parse` ENTRYPOINTS [02] / `proto.serialize_length_prefixed`/`parse_length_prefixed` ENTRYPOINTS [03]/[04] / `json_format.ParseDict` JSON-codec ENTRYPOINTS [04] / `json_format.MessageToDict(preserving_proto_field_name=True)` JSON-codec ENTRYPOINTS [02] / `descriptor_pool.Default` registries ENTRYPOINTS [02] / `message.Message` PUBLIC_TYPES [01]), `msgspec` (`to_builtins(str_keys=True)` ENTRYPOINTS [06] / `convert` ENTRYPOINTS [05] / `ValidationError` PUBLIC_TYPES [04] / `Struct` PUBLIC_TYPES [01]), `grpcio-tools` (`command.build_package_protos(strict_mode=True)` build ENTRYPOINTS [03] — AOT codegen on the `python_version<'3.15'` band, never imported on the cp315 core), `reliability/faults#FAULT` (the `[02]-[WIRE_RAIL]` `Decode.railed` rail).
- Growth: a new wire message is one `WIRE_REGISTRY` row binding the `(Struct, Message)` pair under its name; `WireProtoCodec.encode`/`.decode` already transcodes it and `Decode.railed` already rails it, zero new surface and no second codec.
- Boundary: the codec transcodes only — no second wire vocabulary, no message shape authored here, no JSON-as-wire-format on the production path (the deterministic protobuf binary from `proto.serialize` is the gRPC wire, `json_format` the boundary projection only); a per-message hand client, a per-message-type encode helper beside the `proto.*` façade (the `protobuf.md` Reject-row), a `Message` leaking into interior code, a `Struct` crossing the wire unprojected, and a hand-rolled varint/tag framing the protobuf core already owns are the deleted forms. The `FaultDetail` decode is the inbound complement of the C# `WireFault.Decode(FaultDetail)` arm — one `WIRE_REGISTRY` row whose decoded `Struct` carries `(package, code, case, message, evidence, correlation, hlc_physical, hlc_logical, tenant)` read off the receipt slot or the `grpc-status-details-bin` trailer, the typed conflict the retry owner decodes, never a re-derived status string; the `clock#CLOCK` `CausalFrame.of` lift onto the `execution/admission#CONTEXT` `RuntimeContext.causal` is performed by the inbound owner `transport/serve#SERVE` `ServerHost.inbound`, NOT by `WireProtoCodec.decode` (whose body is the pure generic `Struct`↔`Message` transcode returning `msgspec.convert(mapping, self._struct)`); the two-half order rides the value-level physical-high/logical-low order the `clock#CLOCK` owner reconstructs from the `evidence/identity#SEED_REPRODUCTION` `HLC_TWO_HALF` corpus row [6] (DESIGN-PIN), an integer pack/unpack distinct from a byte-serialized little-endian claim.

```python signature
from typing import Final

import msgspec
from expression.collections import Map
from google.protobuf import json_format, proto
from google.protobuf.message import Message
from msgspec import Struct

from rasm.runtime._pb2 import channels_pb2
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.shapes import (
    ArtifactFrame, FaultDetail, GenerateRequest, GraphChunk, GraphDiffRequest, GraphDiffResponse,
    InferRequest, InferResponse, QueryRequest, QueryResponse, SolveRequest, SolveResponse,
    SubtreeFetchRequest, TokenChunk, TransactionReceipt, TransactionRequest,
)


class WireProtoCodec[S: Struct, M: Message]:
    def __init__(self, struct: type[S], message: type[M]) -> None:
        self._struct, self._message = struct, message

    def encode(self, value: S) -> RuntimeRail[bytes]:
        return Decode.railed(
            self._struct.__name__,
            lambda: proto.serialize(json_format.ParseDict(msgspec.to_builtins(value, str_keys=True), self._message()), deterministic=True),
        )

    def decode(self, payload: bytes) -> RuntimeRail[S]:
        def project() -> S:
            mapping = json_format.MessageToDict(proto.parse(self._message, payload), preserving_proto_field_name=True)
            return msgspec.convert(mapping, self._struct)

        return Decode.railed(self._struct.__name__, project)


_PROTO_VOCABULARY: Final[tuple[tuple[str, type[Struct], type[Message]], ...]] = (
    ("execute_transaction", TransactionRequest, channels_pb2.TransactionRequest),
    ("transaction_receipt", TransactionReceipt, channels_pb2.TransactionReceipt),
    ("query", QueryRequest, channels_pb2.QueryRequest),
    ("query_response", QueryResponse, channels_pb2.QueryResponse),
    ("infer", InferRequest, channels_pb2.InferRequest),
    ("infer_response", InferResponse, channels_pb2.InferResponse),
    ("solve", SolveRequest, channels_pb2.SolveRequest),
    ("solve_response", SolveResponse, channels_pb2.SolveResponse),
    ("generate", GenerateRequest, channels_pb2.GenerateRequest),
    ("token_chunk", TokenChunk, channels_pb2.TokenChunk),
    ("graph_diff", GraphDiffRequest, channels_pb2.GraphDiffRequest),
    ("graph_diff_response", GraphDiffResponse, channels_pb2.GraphDiffResponse),
    ("subtree_fetch", SubtreeFetchRequest, channels_pb2.SubtreeFetchRequest),
    ("graph_chunk", GraphChunk, channels_pb2.GraphChunk),
    ("artifact_frame", ArtifactFrame, channels_pb2.ArtifactFrame),
    ("fault_detail", FaultDetail, channels_pb2.FaultDetail),
)

WIRE_REGISTRY: Final[Map[str, WireProtoCodec[Struct, Message]]] = Map.of_seq(
    (name, WireProtoCodec(struct, message)) for name, struct, message in _PROTO_VOCABULARY
)


def codec(name: str) -> RuntimeRail[WireProtoCodec[Struct, Message]]:
    return WIRE_REGISTRY.try_find(name).to_result(BoundaryFault(wire=(name, 0)))
```

## [04]-[CRDT_DECODE]

- Owner: `CrdtOp` — the field-less `array_like` tagged-union root the companion decodes from the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` `CrdtOpWire` `[MessagePack.Union]` owner, and `CrdtArm` the closed ten-arm union (`SetOp`…`LeaveOp`, tag 0-9) the decoder targets so callers `match`/`assert_never` over the explicit set rather than the open base; each arm is the array-like flat decode whose fields ARE the producer `[Key(k)]` slots and which exposes the `clock#CLOCK` `Hlc` cell and `ElementId` it carries as DERIVED causal views (`@property cell` / `id`), so the canonical op and the wire arm are one struct and no parallel wire-vs-canonical hierarchy or hand-written lift match survives; `CrdtOpDecode` the one decode surface reading the MessagePack delta the `OpLogEntry.Payload` carries for `column-family=crdt` rows; `DecompressFn` the dependency-injected decompress protocol the decode closes over. The op-log crosses as MessagePack under `Lz4BlockArray`, NOT protobuf — the durability codec the C# snapshot ladder and the wire seam share, distinct from the gRPC proto wire at `[3]`.
- Cases: ten op arms mirror the producer union tag-for-tag — `set`(0), `write`(1), `add`(2), `remove`(3), `increment`(4), `insert_after`(5), `delete`(6), `maintain`(7), `beat`(8), `leave`(9); LWW survives only as the `set` arm reconstructing the `LwwRegister`, the `beat`/`leave` arms carry the `EphemeralMap` presence delta the late-joining companion reconstructs from the op-log prefix. Each arm's field positions decode the producer `[Key]` order exactly as the array-like tagged-union envelope: `set`/`beat`/`leave` carry flat `physical_ticks`/`logical` at sibling positions and expose the `(physical_ticks, logical) -> Hlc` cell as the derived `cell` property; `add`/`delete`/`insert_after` carry flat `*_origin`/`*_logical` pairs and expose the `(origin, logical) -> ElementId` as the derived `id`/`predecessor`/`element_id` property; `remove` carries the `list[tuple[origin, logical]]` observed-tag pairs and exposes the `observed -> list[ElementId]` projection; `write` adds the `(origin, seq)[]` version-vector `context`. The derived views replace the prior parallel `CrdtOpWire*` hierarchy and the ten-arm `lift` match — the reconstruction is per-arm `@property` on the one struct family, so interior code reads `op.cell`/`op.id` exactly as before while the wire shape stays the flat producer envelope.
- Entry: `CrdtOpDecode.decode` reads the decompressed MessagePack payload through one reusable `msgspec.msgpack.Decoder(CrdtArm)` (the closed ten-arm union over the `CrdtOp` array-like tagged root) into the canonical op directly — the `array_like=True` integer-`tag` envelope discriminates position-for-position against the producer `[Key(k)]` order, validated arm-for-arm by the same C core that decodes the type, so a producer op the companion does not decode is a typed `ValidationError` at the decoder, never a silent drop, and because `CrdtArm` is the explicit closed `|`-union (not the open `CrdtOp` base) an `assert_never` over it in any caller `match` is a typed build failure on an unhandled arm rather than a silently-admitted subclass; `CrdtOpDecode.stream` is the op-stream output modality reading the many-row `column-family=crdt` prefix the late-joining companion replays — it decodes the whole op sequence in one C pass through a reusable `msgspec.msgpack.Decoder(list[CrdtArm])` into a `Block[CrdtArm]`, validating every arm against the same closed union in a single decode rather than a per-op Python loop over an intermediate `Raw` list, so the single-op `decode` and the prefix-replay `stream` share the same arm union and the one `railed` rail while each owns the decoder shape its payload arity needs (`CrdtArm` versus `list[CrdtArm]`). The `decompress: DecompressFn` injected at the seam chooses WHAT decompresses (identity over a `MessagePackCompression.None` companion lane, or the published `Lz4BlockArray`-envelope reader once the producer framing lands and the cp315 `lz4` wheel arrives) without a body rewrite; the whole decode rides the `[02]-[WIRE_RAIL]` `Decode.railed` rail, so a decode that fails validation crosses the one `reliability/faults#FAULT` `boundary("wire", ...)` conversion under the wire span and lands as `Error(BoundaryFault(boundary=("wire", <ExcType>)))`, never a partial op.
- Auto: the wire is MessagePack — `msgspec.msgpack.Decoder` decodes the producer's tag-keyed array envelope where the C# `[MessagePack.Union(n, typeof(Case))]` integer `n` is the msgpack union tag (the `tag=n` integer discriminant on the `array_like=True` `Struct`) and each `[property: Key(k)]` is the array position, the decoder reused across every op rather than a per-op `msgspec.msgpack.decode` (the `msgspec.md` cached-codec law). The envelope framings are NOT identical and the parity is a producer-side seam obligation, not a settled identity: MessagePack-csharp `[Union]` emits a two-element `[tag, sub-object]` array (the integer tag then a nested map/array of the case body), whereas a msgspec `array_like` tagged `Struct` decodes a FLAT `[tag, field0, field1, ...]` array. The `array_like=True` flat decode the page models holds only if the producer flattens the union case body into the tag array (a keyed-flat-array emission, not the default nested-union `MessagePackSerializer` shape); that producer framing decision is the cross-agent `CRDT_OPLOG_WIRE_AMENDMENT` obligation on `csharp:Rasm.Persistence/Version/commits#CRDT_WIRE`, and if the producer keeps the default nested two-element union the decode resolves through a `dec_hook` unwrapping the one intermediate-array layer (the per-case body under the integer-tagged envelope) onto the same flat `CrdtOp` arm — the `dec_hook` seam absorbs the nesting WITHOUT a parallel wire hierarchy, settled against the published producer framing and never asserted ahead of it. The companion authors no op kind the wire does not carry and re-mints no op vocabulary, the C# owner being the sole mint per the single-mint invariant; `physical_ticks` is the C# `Instant.ToUnixTimeTicks()` 100-ns count the `clock#CLOCK` `Hlc` raises back to the NodaTime-equivalent instant, and the `(origin, logical)` `ElementId` is the HLC-stable peer-local identity the RGA and OR-set address by, never positional; the `set` arm is the LWW `Adjudicate` survivor and the ten-arm union is the strict superset the C# `Crdt.Merge` join-semilattice converges, so a companion decoding the prefix reconstructs the identical materialized state the producer holds.
- Packages: `msgspec` (`msgpack.Decoder(dec_hook=...)` reusable codec ENTRYPOINTS [09] / `Raw` deferred sub-message decode PUBLIC_TYPES [03] / `msgpack.Ext` PUBLIC_TYPES [05] / `Struct` array-like tagged union / `ValidationError` PUBLIC_TYPES [04]), `expression` (`Block` the op-stream carrier the prefix replay folds into), `lz4` (`block.decompress` ENTRYPOINTS [02] — the per-chunk codec inside the `Lz4BlockArray` msgpack-`ext` envelope, not the self-describing `frame` codec; injected through `DecompressFn` on the `python_version<'3.15'` companion band only, never imported here), `clock#CLOCK` (`Hlc`/`ElementId` the derived `cell`/`id` views reconstruct), `reliability/faults#FAULT` (the `[02]-[WIRE_RAIL]` `Decode.railed` rail).
- Growth: a new op kind is one `CrdtOp` tagged-union arm carrying its flat producer slots plus its derived `cell`/`id` view — the producer adds the wire tag, the companion adds the one arm, never ahead of the wire and never a paired wire-vs-canonical class; a new op-log output modality (a windowed prefix, a checkpoint range) is one `CrdtOpDecode` entry over the same decoder; zero new surface, no second op vocabulary.
- Boundary: the op-log delta is the C#-owned MessagePack wire — a protobuf framing of the op-log (the prior mismodel), a typeless payload, a JSON-array delta, a re-minted op union, and a parallel `CrdtOpWire*` hierarchy with a hand-written ten-arm lift match (the prior over-modeling, collapsed here onto derived views) are the deleted forms; the decode is read-only, the companion never encoding an op the C# owner did not mint; `decompress` is a `DecompressFn` protocol seam, never a hardwired `lz4` import — the `Lz4BlockArray` envelope is the MessagePack-csharp block-array framing (a msgpack `ext` envelope wrapping independently-LZ4-block-compressed chunks), so the LZ4 decompression leg gates on the cp315 `lz4` wheel and the producer's `MessagePackCompression.None` companion-lane decision, and a hardwired LZ4 import that breaks the cp315-core import is the deleted form; the `Hlc`/`ElementId` reconstruction reads the one `clock#CLOCK` owner through the derived views and re-mints no stamp.

```python signature
from typing import Protocol

import msgspec
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.clock import ElementId, Hlc
from rasm.runtime.faults import RuntimeRail


class DecompressFn(Protocol):
    def __call__(self, payload: bytes) -> bytes: ...


class CrdtOp(Struct, frozen=True, tag_field="tag", array_like=True):
    # field-less tagged-union root: with `array_like=True` every base field would occupy a
    # leading array slot in every arm and shift the producer `[Key(k)]` positions, so the
    # base carries the discriminant only and each arm's first declared field IS wire slot 1.
    pass


class SetOp(CrdtOp, tag=0, gc=False):
    value: bytes
    physical_ticks: int
    logical: int
    origin: bytes

    @property
    def cell(self) -> Hlc:
        return Hlc(self.physical_ticks, self.logical)


class WriteOp(CrdtOp, tag=1):
    value: bytes
    context: list[tuple[bytes, int]]
    physical_ticks: int
    logical: int
    origin: bytes

    @property
    def cell(self) -> Hlc:
        return Hlc(self.physical_ticks, self.logical)


class AddOp(CrdtOp, tag=2, gc=False):
    element: bytes
    tag_origin: bytes
    tag_logical: int

    @property
    def id(self) -> ElementId:
        return ElementId(self.tag_origin, self.tag_logical)


class RemoveOp(CrdtOp, tag=3):
    element: bytes
    observed: list[tuple[bytes, int]]

    @property
    def observed_tags(self) -> Block[ElementId]:
        return Block.of_seq(ElementId(origin, logical) for origin, logical in self.observed)


class IncrementOp(CrdtOp, tag=4, gc=False):
    origin: bytes
    delta: int


class InsertAfterOp(CrdtOp, tag=5, gc=False):
    pred_origin: bytes
    pred_logical: int
    id_origin: bytes
    id_logical: int
    value: bytes

    @property
    def predecessor(self) -> ElementId:
        return ElementId(self.pred_origin, self.pred_logical)

    @property
    def id(self) -> ElementId:
        return ElementId(self.id_origin, self.id_logical)


class DeleteOp(CrdtOp, tag=6, gc=False):
    id_origin: bytes
    id_logical: int

    @property
    def id(self) -> ElementId:
        return ElementId(self.id_origin, self.id_logical)


class MaintainOp(CrdtOp, tag=7):
    quiescent: list[tuple[bytes, int]]


class BeatOp(CrdtOp, tag=8, gc=False):
    origin: bytes
    state: bytes
    physical_ticks: int
    logical: int

    @property
    def cell(self) -> Hlc:
        return Hlc(self.physical_ticks, self.logical)


class LeaveOp(CrdtOp, tag=9, gc=False):
    origin: bytes
    physical_ticks: int
    logical: int

    @property
    def cell(self) -> Hlc:
        return Hlc(self.physical_ticks, self.logical)


type CrdtArm = SetOp | WriteOp | AddOp | RemoveOp | IncrementOp | InsertAfterOp | DeleteOp | MaintainOp | BeatOp | LeaveOp


class CrdtOpDecode:
    _decoder: msgspec.msgpack.Decoder[CrdtArm] = msgspec.msgpack.Decoder(CrdtArm)
    _prefix: msgspec.msgpack.Decoder[list[CrdtArm]] = msgspec.msgpack.Decoder(list[CrdtArm])

    @classmethod
    def decode(cls, payload: bytes, decompress: DecompressFn) -> RuntimeRail[CrdtArm]:
        return Decode.railed("crdt", lambda: cls._decoder.decode(decompress(payload)))

    @classmethod
    def stream(cls, payload: bytes, decompress: DecompressFn) -> RuntimeRail[Block[CrdtArm]]:
        return Decode.railed("crdt.prefix", lambda: Block.of_seq(cls._prefix.decode(decompress(payload))))
```

## [05]-[RESEARCH]

- [WIRE_RAIL]: [COMPLETE] (design) — the `[02]-[WIRE_RAIL]` `Decode` aspect is the AOP collapse of the `.api/msgspec.md` IMPLEMENTATION_LAW integration rail (`msgspec.Decoder(type=<tagged union>, dec_hook=<lift>)` retried under a `stamina` `retry_context` transport-faults-only, inside an `opentelemetry-api` span) and the `.api/opentelemetry-api.md` INTEGRATION lines (`to_builtins(struct, str_keys=True)` feeds `Span.set_attributes` with no manual flattening; a `stamina.retry_context` loop wraps each attempt in a child span; `RpcError.code()` maps to `Status(StatusCode.ERROR)` on the final failed span via `set_status`+`record_exception`). The wire concerns fold into `Decode.railed` (sync, already-buffered proto/CRDT payload: CONSUMER span over the `reliability/faults#FAULT` `boundary` fence) and `Decode.acquired` (awaitable, network-fed leg) — `railed` is reused directly by `WireProtoCodec.decode` and `CrdtOpDecode.decode`/`stream`, while `acquired` delegates the retried acquisition to `reliability/resilience#RESILIENCE` `guarded(RetryClass.WIRE, fetch, subject="wire")` and `bind`s `railed`, so the retry loop, the retry span, and the terminal `async_boundary` lift are the resilience owner's aspect rather than a second triplet re-spelled here. The `RetryClass.WIRE` `POLICY` row (`reliability/resilience#RESILIENCE` `Policy(attempts=5, timeout=15.0, target=(ConnectionError,))`) is the sole retry policy this page reads — the prior wire page composed `msgspec`+`protobuf` flat and left the `WIRE` row unused, and the prior `acquired` re-implemented `guard(RetryClass.WIRE)` plus an inline `try`/`except (ConnectionError, TimeoutError)`/`record_exception` triplet that duplicated `guarded` and retried a `TimeoutError` the `WIRE` `target` never names; the collapse routes the leg through `guarded` so the page realizes the catalogue retried-traced-railed wire stack by composition. `SpanKind.CONSUMER` is the decode-of-inbound kind per the `.api/opentelemetry-api.md` PUBLIC_TYPES [07] `SpanKind` set. Settled on the cp315 core (`msgspec`/`opentelemetry-api`/`stamina` all cp315-clean).
- [PROTO_TRANSCODE]: [COMPLETE] — reflection-confirmed against the runtime catalogs `.api/protobuf.md` and `.api/msgspec.md` and the branch catalogs `libs/python/.api/protobuf.md`/`grpcio-tools.md`. The binary leg is the message-agnostic `proto.serialize(message, deterministic=True)` (ENTRYPOINTS [01]) / `proto.parse(message_class, payload)` (ENTRYPOINTS [02]) functional façade the `protobuf.md` binary-law mandates and the RAIL_LAW per-message-encode-helper Reject-row requires — the prior page called the bound `Message.SerializeToString`/`ParseFromString` directly, which the rebuilt catalog now names as the underlying methods the `proto.*` façade delegates to (a per-message bound-method path beside the façade is the deleted form); `json_format.ParseDict` (JSON-codec ENTRYPOINTS [04]) / `json_format.MessageToDict(preserving_proto_field_name=True)` (JSON-codec ENTRYPOINTS [02]) hold the snake_case projection, and `msgspec.to_builtins(str_keys=True)`/`msgspec.convert` (ENTRYPOINTS [06]/[05]) are the canonical lowering/raising pair. `WIRE_REGISTRY` is the `expression` `Map[str, WireProtoCodec[Struct, Message]]` row table over the seventeen-message family — the prior page listed the pairs as prose with no registry, so growth was a per-instantiation `WireProtoCodec(Struct, Message)` rather than a named row; the registry is the data-table collapse the `transport/serve#SERVE`/`#CAPABILITY_INVOKE` consumers resolve by name. The generated `_pb2` classes arrive from `grpcio-tools` `command.build_package_protos(strict_mode=True)` (build ENTRYPOINTS [03]) AOT into a tracked package per the `grpcio-tools.md` LOCAL_ADMISSION build-time-only law (gated `python_version<'3.15'`, the cp315 core never imports `grpc_tools`); `protobuf 6.33.6` is cp315-clean core-direct (no environment marker), so the proto codec rides the core floor. The codec is settled and awaits only the landed `.proto` descriptors compiled to `_pb2`.
- [CRDT_COLLAPSE]: [COMPLETE] (design) — the prior page declared a parallel two-hierarchy model: ten canonical `CrdtOp*` arms (`SetOp`…`LeaveOp`) PLUS ten wire `CrdtOpWire*` arms (`SetWire`…`LeaveWire`) PLUS a hand-written ten-arm `CrdtOpDecode.lift` match reconstructing `Hlc`/`ElementId` field-by-field — 22 classes and a 10-arm switch for one concept, the explicit wire.md parallel-class anti-pattern. The collapse keeps ONE `CrdtOp` arm family whose fields ARE the flat producer `[Key(k)]` slots (`physical_ticks`/`logical`/`origin` at sibling positions) and exposes the `clock#CLOCK` `Hlc` cell and `ElementId` as DERIVED `@property` views (`cell`/`id`/`predecessor`/`observed_tags`), so interior code reads `op.cell`/`op.id` exactly as before while the wire shape stays the flat array-like envelope — 22 classes -> 11 (one base + ten arms), and the ten-arm `lift` match is deleted entirely (the `msgspec.msgpack.Decoder(CrdtOp)` decodes straight to the canonical arm, no second projection). The msgspec `array_like` tagged-union with integer `tag=n` decodes the producer's tag-keyed array against the `[property: Key(k)]` order, validated arm-for-arm; the flat `[tag, field0, ...]` decode matching the producer holds only if `CRDT_WIRE` emits the keyed-flat-array framing (the `[MessagePack.Union]` default nests the case body as a two-element `[tag, sub-object]` array, so the flat-vs-nested framing is the `CRDT_OPLOG_WIRE_AMENDMENT` producer-seam obligation), and if the producer keeps the default nesting the `dec_hook` seam (`.api/msgspec.md` ENTRYPOINTS [09]) unwraps the one intermediate-array layer onto the same flat arm WITHOUT reintroducing a parallel wire hierarchy. `set`/`beat`/`leave` split the `(physical_ticks, logical)` pair the `cell` view reads, `add`/`delete`/`insert_after` carry the `(origin, logical)` pairs the `id`/`predecessor` views read. The decode rides the `[02]-[WIRE_RAIL]` `Decode.railed` rail, so an unmapped wire arm is a typed `ValidationError` lifted to `BoundaryFault` under the wire span, never a raw-wire leak. The collapse and the canonical lift-as-derived-view are settled on the cp315 core.
- [CRDT_OUTPUT_MODALITY]: [COMPLETE] (design) — the prior `CrdtOpDecode.decode` returned `RuntimeRail[CrdtOpArm]` for a SINGLE op, but the op-log is a STREAM (`OpLogEntry.Payload` for many `column-family=crdt` rows, the prefix the late-joining companion replays). The output is parameterized over single-op (`decode`) and op-stream (`stream`) modalities sharing one reusable decoder — `stream` reads the `Raw`-enveloped op sequence (the `.api/msgspec.md` deferred-decode law: a `Raw` field captures each sub-message's bytes so the routing decode reads the envelope and defers the per-op decode, the canonical lazy/partial-decode path for the op-log envelope, never a manual byte-slice) and folds the per-op decodes into an `expression` `Block[CrdtOp]`. The single-op and prefix-replay legs share `_decoder` and the `Decode.railed` rail, so the modality is one entry over the same decoder, never a parallel stream codec. Settled on the cp315 core; the modality choice is the caller's, not a `streaming=True` knob.
- [CRDT_DECODE_LZ4]: [UPSTREAM-BLOCKED] — the C# producer compresses the op-log payload under `MessagePackCompression.Lz4BlockArray`, the MessagePack-csharp block-array framing (a msgpack `ext` type wrapping independently-LZ4-block-compressed chunks), not a raw LZ4 frame. The `CrdtOpDecode.decode(cls, payload, decompress)` seam injects the `decompress: DecompressFn` protocol, so the resolution is purely WHICH callable fills it, never a body rewrite — (a) a `MessagePackCompression.None` companion lane fills `decompress` with the identity function over the uncompressed delta the settled `msgspec.msgpack.Decoder(CrdtOp)` leg already decodes, needing no LZ4 dependency on the core; or (b) the published `Lz4BlockArray` envelope reader (per-chunk `.api/lz4.md` `block.decompress` ENTRYPOINTS [02] over each block inside the msgpack-`ext` envelope, the raw-block codec the catalog block-law scopes to an outer envelope, NOT the self-describing `frame` codec) the three runtimes share, gated on the cp315 `lz4` wheel. Two genuinely-upstream gates remain: (1) `lz4` is manifest-declared `lz4; python_version<'3.15'` with NO cp315 wheel synced, so the LZ4 decompression rides the `python_version<'3.15'` companion band exactly like `xxhash`; (2) the producer `Lz4BlockArray`-envelope framing publication (the cross-agent `CRDT_OPLOG_WIRE_AMENDMENT` delta on `csharp:Rasm.Persistence/Version/commits#CRDT_WIRE`, where LZ4 stays C#-internal and the `MessagePackCompression.None` lane serves cross-runtime consumers). The `msgspec.msgpack` decode of the uncompressed delta is proven on the cp315 core; the compressed-envelope link stays gated through the `DecompressFn` seam, never a hardwired `lz4` import that breaks the cp315-core import.

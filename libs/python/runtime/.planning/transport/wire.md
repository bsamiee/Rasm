# [PY_RUNTIME_WIRE]

The single wire-codec owner the companion transport composes, decoding every C#-minted wire shape and minting no wire vocabulary of its own. `msgspec` is the in-memory frame layer and `protobuf` the wire; the page owns the projection between them and the CRDT op-log decode.

`WireProtoCodec` transcodes the canonical `msgspec` `Struct` shapes interior code holds to the generated protobuf `Message` the C# gRPC services carry and back, `protobuf 6.33`-backed through the message-agnostic `google.protobuf.proto` functional façade (`proto.serialize`/`proto.parse`), never a hand-rolled varint. `WIRE_REGISTRY` is the one data table binding each canonical `(Struct, Message)` pair under its semantic name, so the sixteen-message family is rows rather than sixteen hand clients.

`CrdtOp` is the field-less `array_like` tagged root and `CrdtArm` the closed ten-arm union (`SetOp`…`LeaveOp`, tag 0-9) the companion decodes from the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` owner. Each arm is dense and append-only, carrying the flat producer `[Key(k)]` slots and exposing the `clock#CLOCK` `Hlc` cell and `ElementId` it reconstructs as derived causal views, so the canonical op IS the wire arm and no parallel wire-vs-canonical hierarchy or hand-written lift match survives. `CrdtOpDecode` dispatches on the `array_like` integer tag through the `msgspec` tagged-union core, lifting the MessagePack delta the `OpLogEntry.Payload` carries for `column-family=crdt` rows. The op-log crosses as MessagePack under a `Lz4BlockArray` envelope distinct from the gRPC proto wire, and `decompress` is a dependency-injected `DecompressFn` seam the decode closes over, never a hardwired `lz4` import (LZ4 is worker and the compressed-envelope decode is deferred).

Every transcode rides the one `Decode` aspect's direction-parameterized `_traced` fold: `railed` scopes an inbound decode under an `opentelemetry-api` CONSUMER span and `routed` an outbound encode under a PRODUCER span, both inside the one `reliability/faults#FAULT` `boundary` fence. The network-fed `acquired` leg delegates the `reliability/resilience#RESILIENCE` `RetryClass.WIRE` retried acquisition to the resilience owner's `guarded` aspect before binding `railed`, so span and fault conversion are one woven flow on every crossing, the fault-status span egress stays the faults owner's `_convert` weave, and retry is reached by composition rather than a loop or a status re-set repeated inline per codec.

`WireProtoCodec`/`CrdtOp*`/`CrdtOpDecode` formerly mis-lived in `transport/serve`; they collapse here so the serve lifecycle references one codec owner rather than carrying the full codec body inline.

## [01]-[INDEX]

- [01]-[WIRE_RAIL]: the one traced-railed `Decode` aspect every wire boundary composes — a direction-parameterized OTel span (`railed` CONSUMER decode, `routed` PRODUCER encode) + `boundary` fault fence over `msgspec.Decoder`/`proto.parse`/`proto.serialize` for the buffered leg, the network-fed leg delegating retry to `reliability/resilience#RESILIENCE` `guarded(RetryClass.WIRE)` rather than re-implementing the loop, woven once.
- [02]-[PROTO_TRANSCODE]: the registry-driven msgspec-canonical to protobuf-wire codec at the gRPC seam, the `WIRE_REGISTRY` row table over the sixteen-message family, the inbound `FaultDetail` decode carrying the `hlc_physical`/`hlc_logical`/`tenant` fields the serve owner lifts.
- [03]-[CRDT_DECODE]: the MessagePack op-log `CrdtArm` discriminated union over the `CrdtOp` array-like tagged root with derived `Hlc`/`ElementId` views, the tag-dispatched decoder, the single-op and op-stream output modalities over the `Framing` flat/nested producer-seam axis (the `_unnest` two-stage `Raw` re-frame for the default `[Union]` nesting), and the dependency-injected decompress seam (the durability codec, NOT protobuf).

## [02]-[WIRE_RAIL]

- Owner: `Decode` — the one cross-cutting wire-boundary aspect every codec on this page composes, folding the wire concerns into a single woven rail over a `_traced(verb, kind, subject, run, *, annotate)` core whose `(verb, kind, annotate)` row is the only direction axis: a reusable `msgspec.Decoder` (or the protobuf `proto.parse`/`proto.serialize` façade) carries the bytes↔`Struct` projection, the `reliability/resilience#RESILIENCE` `RetryClass.WIRE` row retries the transient transport leg, an `opentelemetry-api` span scopes the projection (`SpanKind.CONSUMER` for the inbound `railed` decode, `SpanKind.PRODUCER` for the outbound `routed` encode), and the `reliability/faults#FAULT` `boundary("wire", ...)` conversion lands a malformed frame as `Error(BoundaryFault(boundary=("wire", <ExcType>)))`. The aspect is the AOP collapse of the `msgspec.md` IMPLEMENTATION_LAW integration rail — `msgspec.Decoder(type=<tagged union>, dec_hook=<lift>)` feeding a validated frame, retried under a `stamina` `retry_context` (transport faults only, `ValidationError` terminal), inside a span — so retry, telemetry, and fault conversion are declared once and reused by the proto transcode (both legs) and the CRDT decode, never repeated inline per codec and never a CONSUMER-kind span mis-scoping an egress encode.
- Entry: `Decode.railed(subject, decode)` and `Decode.routed(subject, encode)` are the two thin direction verbs over the one `_traced(verb, kind, subject, run, *, annotate)` fold — `railed` is the inbound decode (`verb="decode"`, `SpanKind.CONSUMER`, `annotate=True`) `WireProtoCodec.decode` and `CrdtOpDecode` compose, `routed` the outbound encode (`verb="encode"`, `SpanKind.PRODUCER`, `annotate=False`) `WireProtoCodec.encode` composes so a producer-side serialize rides a `wire.encode.{subject}` PRODUCER span rather than a `wire.decode` CONSUMER span mis-naming the verb and inverting the kind. `_traced` opens the `trace.get_tracer("rasm.wire").start_as_current_span(f"wire.{verb}.{subject}", kind=kind)` scope and runs the pure `run` thunk inside the one `boundary("wire", ...)` fence, mapping the `Ok` frame through the `annotated[T](span, frame) -> T` passthrough projector when `annotate` and returning the `Error` arm verbatim — `annotated` is identity-after-side-effect (it seeds the span attributes then returns the frame unchanged), so the success projection is one `rail.map(lambda frame: cls.annotated(span, frame))` arm rather than a `None`-returning statement spliced back through a tuple index. The malformed-frame span egress (`record_exception` + `Status(StatusCode.ERROR)`) is the `reliability/faults#FAULT` `_convert` weave on the active span the fence already owns, so the fold adds only the success-side `annotated` projection the faults egress does not perform and never re-sets a status the conversion already set (a second `Span.set_status` on the same span is the faults-owner trample, deleted); the egress `routed` leaves `annotate` off because its `bytes` result carries no field signal the active producer span lacks. `Decode.acquired(subject, fetch, decode)` is the awaitable fold for a future network-fed leg — it delegates the retried acquisition wholesale to `reliability/resilience#RESILIENCE` `guarded(RetryClass.WIRE, fetch, subject="wire")`, which already fuses the `WIRE`-row retry (`attempts=5`/`timeout=15.0`/`target=(ConnectionError,)`, the only retried surface), the retry span, and the one-shot `async_boundary` terminal-fault lift into a single `RuntimeRail[bytes]`, then `bind`s the buffered `railed` decode so the frame crosses the same CONSUMER-span/`boundary` fence; the transient connection fault is retried by the resilience owner and the terminal decode `ValidationError` rides the `railed` boundary on the first decode without a retry. `acquired` is exactly `guarded` composed with `railed` by `bind` — no second try/except, no second span open, no second `record_exception`, no inline retry triplet reconstructing what `guarded` owns. The two entries share one span-naming, one fault subject, and one decoder seam; the sync/awaitable split is the runtime axis, never a parallel telemetry or fault rail. (Every current ingress is buffered: the servicer hands `WireProtoCodec.decode` the raw `bytes` it received and the durability decode reads the op-log payload, so `railed` is the consumed entry and `acquired` is the growth surface a future wire-internal fetch composes — held co-located rather than re-derived at that leg.)
- Auto: the span attributes seed from the decoded frame through one projection. `Decode.annotated(span, frame)` lowers via `msgspec.structs.asdict(frame)` (ENTRYPOINTS struct utilities [02], the field-NAME-keyed projection) and folds each value through `Decode._attr`, the one total match keeping the OTLP-conformant domain `Span.set_attributes` admits — `str | bool | int | float` and a flat scalar `list`/`tuple`. `asdict` is the correct projector precisely because the `array_like` `CrdtArm` arms project under their declared field names (`physical_ticks`/`logical`/`origin`) rather than the meaningless positional `0`/`1` indices `to_builtins(array_like=True)` returns as a bare `list` with no `.items()`; one named projection serves the `array_like` CRDT arms and the named `WireProtoCodec` `Struct` shapes alike. `asdict` keeps raw `bytes` (unlike the base64-lowering `to_builtins`), so `_attr`'s `bytes` arm renders the dominant CRDT byte slots (`SetOp.value`, `origin`, `element`) as fixed-width `.hex()` the exporter's `str` domain admits; a nested `list[tuple[...]]` field (the `RemoveOp.observed` tag list, the `WriteOp.context` vector clock `asdict` keeps as `list[tuple[bytes, int]]`) fails the flat-scalar guard and folds to `None`, riding the receipt. The retry hook is the branch-wide `reliability/resilience#RESILIENCE` `install(RetryMode.EMIT)` stacked `(RetryReceiptHook, Metrics.retry_hook(), StructlogOnRetryHook)` registered once, never a per-codec retry log; the `RetryClass.WIRE` row is the sole retry policy this page reads, so a new wire retry geometry is one column on the `reliability/resilience#RESILIENCE` `Policy` row, never a knob here.
- Packages: `msgspec` (`msgpack.Decoder`/`json.Decoder` reusable codecs ENTRYPOINTS [10]/[04], `structs.asdict` ENTRYPOINTS struct utilities [02] the field-name-keyed span-attribute projection over both `array_like` and named structs, `ValidationError`/`DecodeError` PUBLIC_TYPES [04]/[03] the terminal decode fault), `opentelemetry-api` (`trace.get_tracer`/`Tracer.start_as_current_span(kind=SpanKind.CONSUMER)`/`Span.set_attributes`/`SpanKind` — branch catalog `libs/python/.api/opentelemetry-api.md` trace ENTRYPOINTS [03]/[10]/[12] + PUBLIC_TYPES [07], `SpanKind.CONSUMER`/`SpanKind.PRODUCER` the two direction kinds; the `Status`/`StatusCode`/`record_exception` egress is the faults owner's `_convert` on the active span, never re-spelled here), `reliability/resilience#RESILIENCE` (`guarded(RetryClass.WIRE, fetch, subject="wire")` — the retried-traced-railed acquisition the network-fed leg delegates to; the `WIRE` row, the `stamina` caller, the retry span, and the terminal-fault lift are all the resilience owner's, never re-spelled here), `reliability/faults#FAULT` (`boundary`/`RuntimeRail`).
- Growth: a new wire boundary (a new proto pair, a new durability arm, a future op-stream batch) composes `Decode.railed`/`Decode.routed`/`Decode.acquired` and inherits the span, the `RetryClass.WIRE` retry, and the `boundary` fault with zero new cross-cutting code; a new transport direction is one `(verb, kind, annotate)` row on `_traced`, never a fourth method body; a new retried transport class is one `reliability/resilience#RESILIENCE` `RetryClass` row, never a second retry path here; a new span dimension is one `set_attributes` key in `annotated`.
- Boundary: the wire-boundary concern is one aspect — a per-codec inline `try`/`except`, a per-codec hand-rolled `retry` loop (the `stamina` Reject-row manual-loop violation), a per-codec span open without the shared naming, a CONSUMER-kind decode span mis-scoping an egress encode where `routed` mints the PRODUCER kind, a `RetryClass.WIRE` reconstruction instead of the `guarded` delegation, an `acquired` that re-implements the retry/span/`async_boundary` triplet inline rather than delegating to the resilience owner's `guarded` aspect (the duplicated-rail violation), a `railed` Error-arm `Span.set_status`/`record_exception` re-annotating the span the `reliability/faults#FAULT` `_convert` conversion already annotated (the faults-owner-egress trample, the conversion owning the malformed-frame span status exactly once), and a retried `ValidationError` (the `msgspec`/`stamina` terminal-validation Reject-row) are the deleted forms; the buffered leg crosses only the `railed` span/`boundary` fence and the network-fed leg crosses `guarded` then `railed`, so the retried transient exception the `WIRE` row names is the resilience owner's concern and the terminal decode fault converts exactly once at the `boundary` fence, never a bare exception across the servicer and never a second async rail.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable

import msgspec
from opentelemetry import trace
from opentelemetry.trace import Span, SpanKind

from rasm.runtime.faults import RuntimeRail, boundary
from rasm.runtime.resilience import RetryClass, guarded

_TRACER = trace.get_tracer("rasm.wire")

# --- [OPERATIONS] -----------------------------------------------------------------------


class Decode:
    @staticmethod
    def annotated[T](span: Span, frame: T) -> T:
        # passthrough projector composing as one success `.map` arm. `structs.asdict` yields a
        # field-NAME-keyed `dict` for both `array_like` and named `Struct` (the positional `to_builtins`
        # list loses the names a flame graph reads), so the `array_like` `CrdtArm` arms project under
        # `physical_ticks`/`logical`/`origin` rather than meaningless integer indices. Each value folds
        # through `_attr`, which lowers a `bytes` slot to fixed-width hex and drops the nested vector clock.
        admitted = msgspec.structs.asdict(frame).items()
        span.set_attributes({k: a for k, v in admitted if (a := Decode._attr(v)) is not None})
        return frame

    @staticmethod
    def _attr(value: object) -> str | int | float | bool | tuple[str | int | float | bool, ...] | list[str | int | float | bool] | None:
        # OTLP admits a scalar or a flat scalar sequence; `structs.asdict` keeps raw `bytes` (unlike the
        # base64-lowering `to_builtins`), so the `bytes` arm renders a byte slot (`SetOp.value`, `origin`,
        # `element`) as fixed-width `.hex()` the exporter's `str` domain accepts. A nested `list[tuple[...]]`
        # (the `RemoveOp.observed`/`WriteOp.context` vector clock `asdict` keeps as `list[tuple[bytes, int]]`)
        # fails the flat-scalar guard and folds to `None`, riding the receipt rather than crashing the exporter.
        match value:
            case bool() | int() | float() | str():
                return value
            case bytes() | bytearray():
                return value.hex()
            case list() | tuple() if all(isinstance(e, str | bool | int | float) for e in value):
                return value
            case _:
                return None

    @classmethod
    def _traced[T](cls, verb: str, kind: SpanKind, subject: str, run: Callable[[], T], *, annotate: bool) -> RuntimeRail[T]:
        # one direction-parameterized fold: the `(verb, kind, annotate)` row is the only axis, the
        # CONSUMER decode and the PRODUCER encode sharing the span open and the one `boundary("wire", ...)`
        # fence (whose `_convert` owns the Error-arm span egress) — never a second rail per direction.
        # `annotate` maps the success frame through `annotated` (the decode signal the egress encode's
        # `bytes` result lacks); the Error arm returns verbatim so the faults `_convert` owns the span status.
        with _TRACER.start_as_current_span(f"wire.{verb}.{subject}", kind=kind) as span:
            rail = boundary("wire", run)
            return rail.map(lambda frame: cls.annotated(span, frame)) if annotate else rail

    @classmethod
    def railed[T](cls, subject: str, decode: Callable[[], T]) -> RuntimeRail[T]:
        return cls._traced("decode", SpanKind.CONSUMER, subject, decode, annotate=True)

    @classmethod
    def routed[T](cls, subject: str, encode: Callable[[], T]) -> RuntimeRail[T]:
        return cls._traced("encode", SpanKind.PRODUCER, subject, encode, annotate=False)

    @classmethod
    async def acquired[T](cls, subject: str, fetch: Callable[[], Awaitable[bytes]], decode: Callable[[bytes], T]) -> RuntimeRail[T]:
        return (await guarded(RetryClass.WIRE, fetch, subject="wire")).bind(lambda payload: cls.railed(subject, lambda: decode(payload)))
```

## [03]-[PROTO_TRANSCODE]

- Owner: `WireProtoCodec` — the one boundary codec generic over the canonical `msgspec` `Struct` type and its paired generated protobuf `Message` class, transcoding interior shapes to the wire and back across the gRPC seam through the message-agnostic `google.protobuf.proto` functional façade so interior code never touches a `Message` and the wire never sees a `Struct`; it mints no second wire vocabulary, owning only the projection. `WIRE_REGISTRY` is the one data table binding each canonical pair under its semantic name — the sixteen-message family is rows the codec resolves by name, never sixteen hand clients.
- Entry: `WireProtoCodec.encode` projects a canonical `Struct` to wire bytes on the `[02]-[WIRE_RAIL]` `Decode.routed` PRODUCER rail — `msgspec.to_builtins(value, str_keys=True)` lowers the struct to a proto-JSON-compatible mapping (base64-lowering every `bytes` slot to the `str` `json_format.ParseDict` expects for a proto `bytes` field, so the byte payloads round-trip through the proto-JSON contract without a manual `b64encode`), `json_format.ParseDict` fills a fresh generated `Message` (snake_case field names preserved against the proto schema), and `proto.serialize(message, deterministic=True)` emits the canonical deterministic protobuf binary (the `protobuf.md` binary-law façade, the bound `SerializeToString` it delegates to never called directly); `WireProtoCodec.decode` reverses it on the `Decode.railed` CONSUMER rail — `proto.parse(self._message, payload)` constructs and populates a fresh `Message` from the wire bytes, `json_format.MessageToDict(message, preserving_proto_field_name=True)` projects it to a mapping (the proto `bytes` fields arriving as base64 `str`, and every 64-bit `uint64`/`int64`/`fixed64` field as a decimal `str` per the proto3-JSON precision contract), and `msgspec.convert(mapping, self._struct, strict=False)` raises that mapping into the typed canonical `Struct` (base64-decoding each `str` back to the `bytes`-typed field AND coercing each 64-bit decimal `str` to its typed `int` field under the field's own `Meta` bound). `strict=False` is load-bearing on this leg: a strict convert rejects every 64-bit field as `Expected int, got str` because proto3 JSON never emits a 64-bit integer as a JSON number, and it is the same coercion the `clock#CLOCK` `CausalFrame.decode` runs on its carrier-string HLC halves. Both legs cross the one `reliability/faults#FAULT` `boundary("wire", ...)` fence, so a raised `msgspec.ValidationError`/`EncodeError` or protobuf codec error lands as `Error(BoundaryFault(boundary=("wire", <ExcType>)))` whichever direction raised. The module-level `codec(name)` resolves the named pair to its `WireProtoCodec` on the rail through `WIRE_REGISTRY.try_find(name).to_result(BoundaryFault(wire=(name, 0)))` (an absent name a typed code-carrying `wire` fault, the `reliability/faults#FAULT` `wire` case reserved for the explicit numeric-code construction), and `WireProtoCodec.encode`/`.decode` is the one polymorphic transcode every servicer and the capability invoke read.
- Growth: a new wire message is one `WIRE_REGISTRY` row binding the `(Struct, Message)` pair under its name; `WireProtoCodec.encode`/`.decode` already transcodes it and `Decode.routed`/`Decode.railed` already rail both directions, zero new surface and no second codec.
- Boundary: the codec transcodes only — no second wire vocabulary, no message shape authored here, no JSON-as-wire-format on the production path (the deterministic protobuf binary from `proto.serialize` is the gRPC wire, `json_format` the boundary projection only); a per-message hand client, a per-message-type encode helper beside the `proto.*` façade (the `protobuf.md` Reject-row), a `Message` leaking into interior code, a `Struct` crossing the wire unprojected, and a hand-rolled varint/tag framing the protobuf core already owns are the deleted forms. The `FaultDetail` decode is the inbound complement of the C# `WireFault.Decode(FaultDetail)` arm — one `WIRE_REGISTRY` row whose decoded `Struct` carries `(package, code, case, message, evidence, correlation, hlc_physical, hlc_logical, tenant)` read off the receipt slot or the `grpc-status-details-bin` trailer, the typed conflict the retry owner decodes, never a re-derived status string; the `clock#CLOCK` `CausalFrame.of` lift onto the `execution/admission#CONTEXT` `RuntimeContext.causal` is performed by the inbound owner `transport/serve#SERVE` `ServerHost.inbound`, NOT by `WireProtoCodec.decode` (whose body is the pure generic `Struct`↔`Message` transcode returning `msgspec.convert(mapping, self._struct)`); the two-half order rides the value-level physical-high/logical-low order the `clock#CLOCK` owner reconstructs from the `evidence/identity#SEED_REPRODUCTION` `HLC_TWO_HALF` corpus row [6] (DESIGN-PIN), an integer pack/unpack distinct from a byte-serialized little-endian claim.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Final

import msgspec
from expression.collections import Map
from google.protobuf import json_format, proto
from google.protobuf.message import Message
from msgspec import Struct

from rasm.runtime._pb2 import channels_pb2
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.shapes import (
    ArtifactFrame,
    FaultDetail,
    GenerateRequest,
    GraphChunk,
    GraphDiffRequest,
    GraphDiffResponse,
    InferRequest,
    InferResponse,
    QueryRequest,
    QueryResponse,
    SolveRequest,
    SolveResponse,
    SubtreeFetchRequest,
    TokenChunk,
    TransactionReceipt,
    TransactionRequest,
)

# --- [MODELS] ---------------------------------------------------------------------------


class WireProtoCodec[S: Struct, M: Message]:
    def __init__(self, struct: type[S], message: type[M]) -> None:
        self._struct, self._message = struct, message

    def encode(self, value: S) -> RuntimeRail[bytes]:
        return Decode.routed(
            self._struct.__name__,
            lambda: proto.serialize(json_format.ParseDict(msgspec.to_builtins(value, str_keys=True), self._message()), deterministic=True),
        )

    def decode(self, payload: bytes) -> RuntimeRail[S]:
        def project() -> S:
            mapping = json_format.MessageToDict(proto.parse(self._message, payload), preserving_proto_field_name=True)
            # `strict=False`: the proto3-JSON contract emits a 64-bit (`uint64`/`int64`/`fixed64`)
            # field as a DECIMAL STRING to survive JS number precision, so `MessageToDict` returns the
            # HLC halves and any 64-bit count as `str`. The coercion raises them to the typed `int`
            # field AND runs the field's own `Meta` bound in the C core (the same `strict=False` the
            # `clock#CLOCK` `CausalFrame.decode` uses for its carrier-string halves); a strict convert
            # would reject every 64-bit field as `Expected int, got str`.
            return msgspec.convert(mapping, self._struct, strict=False)

        return Decode.railed(self._struct.__name__, project)


# --- [TABLES] ---------------------------------------------------------------------------

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

# --- [OPERATIONS] -----------------------------------------------------------------------


def codec(name: str) -> RuntimeRail[WireProtoCodec[Struct, Message]]:
    return WIRE_REGISTRY.try_find(name).to_result(BoundaryFault(wire=(name, 0)))
```

## [04]-[CRDT_DECODE]

- Owner: `CrdtOp` — the field-less `array_like` tagged-union root the companion decodes from the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` `CrdtOpWire` `[MessagePack.Union]` owner, and `CrdtArm` the closed ten-arm union (`SetOp`…`LeaveOp`, tag 0-9) the decoder targets so callers `match`/`assert_never` over the explicit set rather than the open base; each arm is the array-like flat decode whose fields ARE the producer `[Key(k)]` slots and which exposes the `clock#CLOCK` `Hlc` cell and `ElementId` it carries as DERIVED causal views through two field-less property mixins — `_Stamped.cell` lifts `(physical_ticks, logical) -> Hlc` for the five HLC-stamped arms and `_Identified.id` lifts `(id_origin, id_logical) -> ElementId` for the three element-id arms, so the canonical op and the wire arm are one struct, no parallel wire-vs-canonical hierarchy or hand-written lift match survives, and the `cell`/`id` reconstruction is one accessor per concept rather than the same `@property` body re-spelled per arm (a field-less intermediate `Struct` adds no `array_like` slot, so each arm's first declared field stays wire slot 1 under the integer-`tag` union); `CrdtOpDecode` the one decode surface reading the MessagePack delta the `OpLogEntry.Payload` carries for `column-family=crdt` rows; `Framing` the closed two-member `StrEnum` (`FLAT`/`NESTED`) the producer publication selects so the decode reads the wire-array shape as data; `DecompressFn` the dependency-injected decompress protocol the decode closes over and `_Nested` the one `[tag, body]` envelope row the `NESTED` path re-decodes through a `msgspec.Raw` body when the producer keeps the default `[MessagePack.Union]` nesting. The op-log crosses as MessagePack under `Lz4BlockArray`, NOT protobuf — the durability codec the C# snapshot ladder and the wire seam share, distinct from the gRPC proto wire at `[03]`.
- Cases: ten op arms mirror the producer union tag-for-tag — `set`(0), `write`(1), `add`(2), `remove`(3), `increment`(4), `insert_after`(5), `delete`(6), `maintain`(7), `beat`(8), `leave`(9); LWW survives only as the `set` arm reconstructing the `LwwRegister`, the `beat`/`leave` arms carry the `EphemeralMap` presence delta the late-joining companion reconstructs from the op-log prefix. Each arm's field positions decode the producer `[Key]` order exactly as the array-like tagged-union envelope: `set`/`write`/`beat`/`leave` carry flat `physical_ticks`/`logical` at sibling positions and inherit the `_Stamped.cell` lift of `(physical_ticks, logical) -> Hlc`; `add`/`delete`/`insert_after` carry flat `id_origin`/`id_logical` pairs and inherit the `_Identified.id` lift of `(id_origin, id_logical) -> ElementId` (`insert_after` adding the `predecessor` view over its `pred_*` pair); `remove` carries the `observed: list[tuple[bytes, U64]]` tag pairs and exposes the `observed -> Block[ElementId]` projection as the derived `observed_tags` property; `write` adds the `context: list[tuple[bytes, U64]]` version-vector pairs. The derived views replace the prior parallel `CrdtOpWire*` hierarchy and the ten-arm `lift` match — the reconstruction is two field-less property mixins plus the per-arm `observed_tags`/`predecessor` views on the one struct family, so interior code reads `op.cell`/`op.id` exactly as before while the wire shape stays the flat producer envelope and the shared lift is one accessor, not five `cell` and three `id` bodies.
- Auto: the wire is MessagePack — `msgspec.msgpack.Decoder` decodes the producer's tag-keyed array envelope where the C# `[MessagePack.Union(n, typeof(Case))]` integer `n` is the msgpack union tag (the `tag=n` integer discriminant on the `array_like=True` `Struct`) and each `[property: Key(k)]` is the array position, the decoder reused across every op rather than a per-op `msgspec.msgpack.decode` (the `msgspec.md` cached-codec law). The envelope framings are NOT identical and the parity is a producer-side seam obligation, not a settled identity: MessagePack-csharp `[Union]` emits a two-element `[tag, sub-object]` array (the integer tag then a nested array of the case body), whereas a msgspec `array_like` tagged `Struct` decodes a FLAT `[tag, field0, field1, ...]` array. The flat path is the settled target (the `CRDT_OPLOG_WIRE_AMENDMENT` keyed-flat emission `_arm`/`_prefix` decode straight through), and the nested fallback is a two-stage `Raw` decode rather than a reshaping hook: `_tagged` decodes the `_Nested` `[tag, body]` row whose `body: msgspec.Raw` captures the inner array un-decoded (the `.api/msgspec.md` deferred-decode law), then `_unnest` re-frames the row into the flat `[tag, *body]` array — `msgspec.msgpack.encode((nested.tag, *msgspec.msgpack.decode(nested.body)))` splices the deferred body fields after the tag — and the one `_arm` codec re-decodes that flat array against the matching `CrdtArm` arm, so the NESTED case validates through the SAME closed union the FLAT path uses. A `dec_hook` is the wrong mechanism here — msgspec invokes `dec_hook(type, obj)` only to lift an unknown target type from a builtin, never to restructure the wire array of a known tagged-`Struct` union, so the nested-vs-flat decision is the `Framing`-keyed `Raw` re-decode, never a hook on the union and never a second `CrdtOpWire*` family. The producer framing decision (`csharp:Rasm.Persistence/Version/commits#CRDT_WIRE`) selects the `Framing` member the call site passes, settled against the published producer framing and never asserted ahead of it. The companion authors no op kind the wire does not carry and re-mints no op vocabulary, the C# owner being the sole mint per the single-mint invariant; every HLC/element half rides the wire-local `WireU64` floor on the arm field itself — `physical_ticks`/`logical`, the `id_logical`/`pred_logical` element-id halves, and the unsigned half of the `(origin, logical)` version-vector (`WriteOp.context`), quiescent (`MaintainOp.quiescent`), and observed-tag (`RemoveOp.observed`) pairs — so the `WireU64` unsigned floor (`Annotated[int, Meta(ge=0)]`) is enforced by the msgspec C core at decode (a negative half a typed `ValidationError` lifted to `BoundaryFault`), NOT only when the `_Stamped.cell`/`_Identified.id` `@property` later constructs the `clock#CLOCK` `Hlc`/`ElementId` (msgspec runs `Meta` validation only on decode, never on the direct `Hlc(...)`/`ElementId(...)` `__init__` the views call, so a plain-`int` arm slot would admit a negative half the pack silently sign-mangles). The arm slot CANNOT carry the `clock#CLOCK` `U64` (`Annotated[int, Meta(ge=0, lt=2**64)]`) directly: the msgspec C core rejects an integer bound that overflows int64 at `Decoder` construction (`ValueError`), so a `clock.U64`-typed slot crashes the module-level `Decoder(CrdtArm)` build. `WireU64` keeps the `ge=0` floor the core admits while still admitting the full unsigned domain (`2**64-1` round-trips), and the `<2**64` ceiling stays the `clock#CLOCK` owner's emission/`Hlc` construction guard, re-narrowing when `_Stamped.cell`/`_Identified.id` constructs the canonical cell. `IncrementOp.delta` stays plain `int` — a signed PN-counter increment, not a `U64` half. `physical_ticks` is the C# `Instant.ToUnixTimeTicks()` 100-ns count the `clock#CLOCK` `Hlc` raises back to the NodaTime-equivalent instant, and the `(origin, logical)` `ElementId` is the HLC-stable peer-local identity the RGA and OR-set address by, never positional; the `set` arm is the LWW `Adjudicate` survivor and the ten-arm union is the strict superset the C# `Crdt.Merge` join-semilattice converges, so a companion decoding the prefix reconstructs the identical materialized state the producer holds.
- Growth: a new op kind is one tagged-union arm carrying its flat producer slots and inheriting `_Stamped` or `_Identified` for the `cell`/`id` view (a novel causal view is one mixin or one per-arm `@property`, never a re-spelled accessor) — the producer adds the wire tag, the companion adds the one arm, never ahead of the wire and never a paired wire-vs-canonical class; a new op-log output modality (a windowed prefix, a checkpoint range) is one `CrdtOpDecode` entry over the same `_arm`/`_unnest` seam; a new producer wire-array shape is one `Framing` member plus one arm on `decode`/`stream`, never a parallel decoder class; zero new surface, no second op vocabulary.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from enum import StrEnum
from typing import Annotated, Protocol

import msgspec
from expression.collections import Block
from msgspec import Meta, Struct

from rasm.runtime.clock import ElementId, Hlc
from rasm.runtime.faults import RuntimeRail

# --- [TYPES] ----------------------------------------------------------------------------

# the arm-slot decode floor. The `clock#CLOCK` `U64` carries `Meta(ge=0, lt=2**64)`, but the
# msgspec C core rejects an integer bound that overflows int64 at `Decoder` construction
# (`ValueError: Integer bounds constraints that don't fit in an int64 are currently not
# supported`), so an arm slot typed `clock.U64` would crash the module-level `Decoder(CrdtArm)`
# build. `WireU64` carries only the `ge=0` floor the C core admits — it still rejects a negative
# half as a typed `ValidationError` at decode AND admits the full unsigned domain (`2**64-1`
# round-trips), with the `<2**64` ceiling staying the `clock#CLOCK` emission/`Hlc` construction
# guard rather than an unconstructible decode bound. The `_Stamped`/`_Identified` views construct
# `clock.Hlc`/`ElementId` from these plain ints, so the canonical domain re-narrows at the view.
type WireU64 = Annotated[int, Meta(ge=0)]


class DecompressFn(Protocol):
    def __call__(self, payload: bytes) -> bytes: ...


class Framing(StrEnum):
    # the producer-seam framing axis: `FLAT` is the `CRDT_OPLOG_WIRE_AMENDMENT` keyed-flat
    # `[tag, field0, ...]` array the closed `CrdtArm` union decodes straight through; `NESTED`
    # is the MessagePack-csharp default `[Union(n, Case)]` `[tag, body]` two-element array the
    # `_Nested` `Raw` re-decode stages. The producer publication (`csharp:Rasm.Persistence/Version/
    # commits#CRDT_WIRE`) selects the member; it is data the decode reads, not a body fork.
    FLAT = "flat"
    NESTED = "nested"


# --- [MODELS] ---------------------------------------------------------------------------


class CrdtOp(Struct, frozen=True, tag_field="tag", array_like=True):
    # field-less tagged-union root: with `array_like=True` every base field would occupy a
    # leading array slot in every arm and shift the producer `[Key(k)]` positions, so the
    # base carries the discriminant only and each arm's first declared field IS wire slot 1.
    pass


class _Stamped(CrdtOp):
    # field-less HLC-view mixin: the five HLC-stamped arms declare `physical_ticks`/`logical` at
    # their own producer slots and inherit the one `cell` accessor, never re-spelling the lift
    # per arm. A field-less intermediate `Struct` adds no `array_like` slot, so the arm's first
    # declared field stays wire slot 1 and the integer-`tag` union still decodes position-for-position.
    @property
    def cell(self) -> Hlc:
        return Hlc(self.physical_ticks, self.logical)  # type: ignore[attr-defined]


class _Identified(CrdtOp):
    # field-less element-id-view mixin: `add`/`delete`/`insert_after` declare `id_origin`/`id_logical`
    # at their own producer slots and inherit the one `id` accessor, so the `(origin, logical) ->
    # ElementId` reconstruction is one member, not three re-spelled `@property` bodies.
    @property
    def id(self) -> ElementId:
        return ElementId(self.id_origin, self.id_logical)  # type: ignore[attr-defined]


class SetOp(_Stamped, tag=0, gc=False):
    value: bytes
    physical_ticks: WireU64
    logical: WireU64
    origin: bytes


class WriteOp(_Stamped, tag=1):
    value: bytes
    context: list[tuple[bytes, WireU64]]
    physical_ticks: WireU64
    logical: WireU64
    origin: bytes


class AddOp(_Identified, tag=2, gc=False):
    element: bytes
    id_origin: bytes
    id_logical: WireU64


class RemoveOp(CrdtOp, tag=3):
    element: bytes
    observed: list[tuple[bytes, WireU64]]

    @property
    def observed_tags(self) -> Block[ElementId]:
        return Block.of_seq(ElementId(origin, logical) for origin, logical in self.observed)


class IncrementOp(CrdtOp, tag=4, gc=False):
    origin: bytes
    delta: int


class InsertAfterOp(_Identified, tag=5, gc=False):
    pred_origin: bytes
    pred_logical: WireU64
    id_origin: bytes
    id_logical: WireU64
    value: bytes

    @property
    def predecessor(self) -> ElementId:
        return ElementId(self.pred_origin, self.pred_logical)


class DeleteOp(_Identified, tag=6, gc=False):
    id_origin: bytes
    id_logical: WireU64


class MaintainOp(CrdtOp, tag=7):
    quiescent: list[tuple[bytes, WireU64]]


class BeatOp(_Stamped, tag=8, gc=False):
    origin: bytes
    state: bytes
    physical_ticks: WireU64
    logical: WireU64


class LeaveOp(_Stamped, tag=9, gc=False):
    origin: bytes
    physical_ticks: WireU64
    logical: WireU64


type CrdtArm = SetOp | WriteOp | AddOp | RemoveOp | IncrementOp | InsertAfterOp | DeleteOp | MaintainOp | BeatOp | LeaveOp


class _Nested(Struct, array_like=True):
    # the MessagePack-csharp default `[Union(n, Case)]` envelope: a two-element `[tag, body]` array
    # where `body` is the nested case array. This is the ONE intermediate shape the `NESTED`-framing
    # path decodes before re-decoding `body` against the tag-selected arm — NOT a `dec_hook` (msgspec
    # never invokes `dec_hook` to reshape the wire array of a known tagged-`Struct` union; the hook
    # lifts an unknown target type from a builtin, it does not restructure a native union's input),
    # and NOT a parallel `CrdtOpWire*` family (one envelope row, not ten wire arms).
    tag: int
    body: msgspec.Raw


# --- [OPERATIONS] -----------------------------------------------------------------------


class CrdtOpDecode:
    # one decoder family keyed by output arity (single arm vs prefix list) AND producer framing
    # (`FLAT` straight-through vs `NESTED` two-stage `Raw`); the reusable codecs over the closed
    # `CrdtArm` union are the shared seam, arity and framing the only axes — input is the one
    # `CrdtArm` closed union, output is `CrdtArm` or `Block[CrdtArm]`, never a per-op hand decoder.
    # `_arm`/`_prefix` decode the keyed-flat array; `_tagged` reads the `_Nested` `[tag, body]` row
    # whose `Raw` body the per-tag `_arm` re-decode resolves against the tag-selected arm.
    _arm: msgspec.msgpack.Decoder[CrdtArm] = msgspec.msgpack.Decoder(CrdtArm)
    _prefix: msgspec.msgpack.Decoder[list[CrdtArm]] = msgspec.msgpack.Decoder(list[CrdtArm])
    _tagged: msgspec.msgpack.Decoder[_Nested] = msgspec.msgpack.Decoder(_Nested)
    _stream: msgspec.msgpack.Decoder[list[_Nested]] = msgspec.msgpack.Decoder(list[_Nested])

    @classmethod
    def _unnest(cls, nested: _Nested) -> CrdtArm:
        # second `Raw` stage: re-frame the captured `[tag, body]` row into the flat `[tag, *body]`
        # array the closed union decodes, so the one `_arm` codec validates the NESTED case against
        # the same arm union the FLAT path uses — never a tenth-arm reshaping hook, never a wire family.
        return cls._arm.decode(msgspec.msgpack.encode((nested.tag, *msgspec.msgpack.decode(nested.body))))

    @classmethod
    def decode(cls, payload: bytes, decompress: DecompressFn, framing: Framing = Framing.FLAT) -> RuntimeRail[CrdtArm]:
        def run() -> CrdtArm:
            raw = decompress(payload)
            return cls._arm.decode(raw) if framing is Framing.FLAT else cls._unnest(cls._tagged.decode(raw))

        return Decode.railed("crdt", run)

    @classmethod
    def stream(cls, payload: bytes, decompress: DecompressFn, framing: Framing = Framing.FLAT) -> RuntimeRail[Block[CrdtArm]]:
        def run() -> Block[CrdtArm]:
            raw = decompress(payload)
            return Block.of_seq(cls._prefix.decode(raw) if framing is Framing.FLAT else (cls._unnest(n) for n in cls._stream.decode(raw)))

        return Decode.railed("crdt.prefix", run)
```

## [05]-[RESEARCH]

# [PY_RUNTIME_WIRE]

The single wire-codec owner the companion transport composes, decoding every C#-minted wire shape and minting no wire vocabulary of its own. `msgspec` is the in-memory frame layer and `protobuf` the wire; the page owns the projection between them and the CRDT op-log decode. The vocabulary itself — the sixteen canonical `Struct` shapes, the tessellation pair, and the `PROTO_VOCABULARY` binding table — is `transport/shapes#REGISTRY_AND_DRIFT`'s; this page imports the rows and owns only transcode machinery.

`WireProtoCodec` transcodes the canonical `msgspec` `Struct` shapes interior code holds to the generated protobuf `Message` the C# gRPC services carry and back, `protobuf`-backed through the message-agnostic `google.protobuf.proto` functional façade (`proto.serialize`/`proto.parse` for the unary legs, `proto.serialize_length_prefixed`/`proto.parse_length_prefixed` for the streamed frame legs), never a hand-rolled varint. `WIRE_REGISTRY` derives one codec instance per `PROTO_VOCABULARY` row, so the message family is rows rather than hand clients.

`CrdtOp` is the field-less `array_like` tagged root and `CrdtArm` the closed ten-arm union (`SetOp`…`LeaveOp`, tag 0-9) the companion decodes from the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` owner. Each arm is dense and append-only, carrying the flat producer `[Key(k)]` slots and exposing the `clock#CLOCK` `Hlc` cell and `ElementId` it reconstructs as derived causal views, so the canonical op IS the wire arm and no parallel wire-vs-canonical hierarchy or hand-written lift match survives. `CrdtOpDecode` dispatches on the `array_like` integer tag through the `msgspec` tagged-union core, lifting the MessagePack delta the `OpLogEntry.Payload` carries for `column-family=crdt` rows. The op-log crosses as MessagePack under a `Lz4BlockArray` envelope distinct from the gRPC proto wire, and `decompress` is a dependency-injected `DecompressFn` seam the decode closes over, never a hardwired `lz4` import (LZ4 is worker and the compressed-envelope decode is deferred).

Every transcode rides the one `Decode` aspect's direction-parameterized `_traced` fold: `railed` scopes an inbound decode under an `opentelemetry-api` CONSUMER span and `routed` an outbound encode under a PRODUCER span, both inside the one `reliability/faults#FAULT` `boundary` fence. The network-fed `acquired` leg delegates the `reliability/resilience#RESILIENCE` `RetryClass.WIRE` retried acquisition to the resilience owner's `guarded` aspect before binding `railed`, so span and fault conversion are one woven flow on every crossing, the fault-status span egress stays the faults owner's `_convert` weave, and retry is reached by composition rather than a loop or a status re-set repeated inline per codec.

## [01]-[INDEX]

- [02]-[WIRE_RAIL]: the one traced-railed `Decode` aspect every wire boundary composes — a direction-parameterized OTel span (`railed` CONSUMER decode, `routed` PRODUCER encode) + `boundary` fault fence over `msgspec.Decoder`/`proto.parse`/`proto.serialize` for the buffered leg, the network-fed leg delegating retry to `reliability/resilience#RESILIENCE` `guarded(RetryClass.WIRE)` rather than re-implementing the loop, woven once.
- [03]-[PROTO_TRANSCODE]: the registry-driven msgspec-canonical to protobuf-wire codec at the gRPC seam — `WIRE_REGISTRY` derived from the imported `transport/shapes#REGISTRY_AND_DRIFT` `PROTO_VOCABULARY`, the unary transcode pair, and the varint-length-prefixed frame pair the streamed legs ride.
- [04]-[CRDT_DECODE]: the MessagePack op-log `CrdtArm` discriminated union over the `CrdtOp` array-like tagged root with derived `Hlc`/`ElementId` views, the tag-dispatched decoder over the keyed-FLAT producer contract, and the dependency-injected decompress seam (the durability codec, NOT protobuf).

## [02]-[WIRE_RAIL]

- Owner: `Decode` — the one cross-cutting wire-boundary aspect every codec on this page composes, folding the wire concerns into a single woven rail over a `_traced(verb, kind, subject, run, *, annotate)` core whose `(verb, kind, annotate)` row is the only direction axis: a reusable `msgspec.Decoder` (or the protobuf `proto.parse`/`proto.serialize` façade) carries the bytes↔`Struct` projection, the `reliability/resilience#RESILIENCE` `RetryClass.WIRE` row retries the transient transport leg, an `opentelemetry-api` span scopes the projection (`SpanKind.CONSUMER` for the inbound `railed` decode, `SpanKind.PRODUCER` for the outbound `routed` encode), and the `reliability/faults#FAULT` `boundary("wire", ...)` conversion lands a malformed frame as `Error(BoundaryFault(boundary=("wire", <ExcType>)))`. Retry, telemetry, and fault conversion are declared once and reused by the proto transcode (both legs, unary and framed) and the CRDT decode, never repeated inline per codec and never a CONSUMER-kind span mis-scoping an egress encode.
- Entry: `Decode.railed(subject, decode)` and `Decode.routed(subject, encode)` are the two thin direction verbs over the one `_traced(verb, kind, subject, run, *, annotate)` fold — `railed` the inbound decode (`verb="decode"`, `SpanKind.CONSUMER`, `annotate=True`) `WireProtoCodec.decode`/`decode_frames` and `CrdtOpDecode` compose, `routed` the outbound encode (`verb="encode"`, `SpanKind.PRODUCER`, `annotate=False`) `WireProtoCodec.encode`/`encode_frames` composes, so a producer-side serialize rides a `wire.encode.{subject}` PRODUCER span rather than a CONSUMER span inverting the kind. `_traced` opens the `_TRACER.start_as_current_span(f"wire.{verb}.{subject}", kind=kind)` scope — the tracer minted once from the `reliability/faults#FAULT` `SCOPES[Scope.WIRE]` row, never a page-local literal — and runs the pure `run` thunk inside the one `boundary("wire", ...)` fence, mapping the `Ok` frame through the `annotated[T](span, frame) -> T` passthrough projector when `annotate` and returning the `Error` arm verbatim; `annotated` is identity-after-side-effect (it seeds the span attributes then returns the frame unchanged), so the success projection is one `rail.map(...)` arm. The malformed-frame span egress (`record_exception` + `Status(StatusCode.ERROR)`) is the faults owner's `_convert` weave on the active span the fence already owns, so the fold adds only the success-side `annotated` projection and never re-sets a status the conversion already set; the egress `routed` leaves `annotate` off because its `bytes` result carries no field signal the active producer span lacks. `Decode.acquired(subject, fetch, decode)` is the awaitable network-fed fold: it delegates the retried acquisition wholesale to `guarded(RetryClass.WIRE, fetch, subject="wire")` — which fuses the `WIRE`-row retry, the retry span, and the one-shot `async_boundary` terminal-fault lift into one `RuntimeRail[bytes]` — then `bind`s the buffered `railed` decode, so the transient connection fault retries under the resilience owner and the terminal decode `ValidationError` rides the `railed` boundary on the first decode without a retry. The two sync entries and the awaitable one share one span naming, one fault subject, and one decoder seam; the sync/awaitable split is the runtime axis, never a parallel telemetry or fault rail. (Every current ingress is buffered — the servicer hands `decode` the raw `bytes` it received and the durability decode reads the op-log payload — so `railed` is the consumed entry and `acquired` the growth surface a future wire-internal fetch composes.)
- Auto: the span attributes seed from the decoded frame through one projection. `Decode.annotated(span, frame)` lowers via `msgspec.structs.asdict(frame)` — the field-NAME-keyed projection that serves the `array_like` CRDT arms (`physical_ticks`/`logical`/`origin`, not the meaningless positional indices `to_builtins(array_like=True)` returns) and the named `Struct` shapes alike — and folds each value through `Decode._attr`, the one total match keeping the OTLP-conformant domain `Span.set_attributes` admits: `str | bool | int | float` and a flat scalar `list`/`tuple`. `asdict` keeps raw `bytes` (unlike the base64-lowering `to_builtins`), so the `bytes` arm renders the dominant CRDT byte slots as fixed-width `.hex()`; a nested `list[tuple[...]]` field (the `RemoveOp.observed` tag list, the `WriteOp.context` vector clock) fails the flat-scalar guard and folds to `None`, riding the receipt. `annotated` is TOTAL over the frame shape — a `Struct` lowers through `asdict`, a `Block` frame batch (`decode_frames`, the CRDT `stream` prefix) seeds only its `frames` cardinality because `asdict` raises `TypeError` on a non-`Struct`, and any other frame passes through unannotated — so the framed legs ride the same `railed` entry with no crash and no second annotate-free entry. The retry hook is the branch-wide `reliability/resilience#RESILIENCE` `install(RetryMode.EMIT)` stacked hook set registered once, never a per-codec retry log; the `RetryClass.WIRE` row is the sole retry policy this page reads, so a new wire retry geometry is one column on the resilience `Policy` row, never a knob here.
- Packages: `msgspec` (`msgpack.Decoder`/`json.Decoder` reusable codecs ENTRYPOINTS [10]/[04], `structs.asdict` struct utilities [02], `ValidationError`/`DecodeError` PUBLIC_TYPES [04]/[03] the terminal decode fault), `protobuf` (`proto.serialize`/`proto.parse` binary façade [01]/[02], `proto.serialize_length_prefixed`/`proto.parse_length_prefixed` streaming [03]/[04], `json_format.ParseDict`/`MessageToDict(preserving_proto_field_name=True)` the boundary projection), `opentelemetry-api` (`trace.get_tracer`/`start_as_current_span(kind=)`/`Span.set_attributes`/`SpanKind` — the `Status`/`StatusCode`/`record_exception` egress is the faults owner's `_convert`, never re-spelled here), `reliability/resilience#RESILIENCE` (`guarded(RetryClass.WIRE, ...)` the retried-traced-railed acquisition), `reliability/faults#FAULT` (`boundary`/`RuntimeRail`/`SCOPES`/`Scope` — the `Scope.WIRE` row mints the one tracer).
- Growth: a new wire boundary composes `Decode.railed`/`routed`/`acquired` and inherits the span, the `WIRE` retry, and the `boundary` fault with zero new cross-cutting code; a new transport direction is one `(verb, kind, annotate)` row on `_traced`, never a fourth method body; a new retried transport class is one resilience `RetryClass` row; a new span dimension is one `set_attributes` key in `annotated`.
- Boundary: the wire-boundary concern is one aspect — a per-codec inline `try`/`except`, a hand-rolled retry loop, a per-codec span open without the shared naming, a CONSUMER-kind decode span on an egress encode, an `acquired` that re-implements the retry/span/`async_boundary` triplet inline, a `railed` Error-arm `Span.set_status`/`record_exception` re-annotating the span the faults `_convert` already annotated, a page-local `trace.get_tracer("<literal>")` beside the `SCOPES` row, and a retried `ValidationError` are the deleted forms; the buffered leg crosses only the `railed` span/`boundary` fence, the network-fed leg crosses `guarded` then `railed`, and the terminal decode fault converts exactly once at the `boundary` fence, never a bare exception across the servicer and never a second async rail.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Awaitable, Callable

import msgspec
from expression.collections import Block
from opentelemetry import trace
from opentelemetry.trace import Span, SpanKind

from rasm.runtime.faults import SCOPES, RuntimeRail, Scope, boundary
from rasm.runtime.resilience import RetryClass, guarded

_TRACER = trace.get_tracer(SCOPES[Scope.WIRE])

# --- [OPERATIONS] -----------------------------------------------------------------------


class Decode:
    @staticmethod
    def annotated[T](span: Span, frame: T) -> T:
        # total passthrough projector composing as one success `.map` arm: `asdict` rejects a
        # non-Struct, so the framed Block legs carry only their cardinality and never crash it.
        match frame:
            case msgspec.Struct():
                span.set_attributes({k: a for k, v in msgspec.structs.asdict(frame).items() if (a := Decode._attr(v)) is not None})
            case Block():
                span.set_attributes({"frames": len(frame)})
            case _:
                pass
        return frame

    @staticmethod
    def _attr(value: object) -> str | int | float | bool | tuple[str | int | float | bool, ...] | list[str | int | float | bool] | None:
        # OTLP admits a scalar or a flat scalar sequence; a `bytes` slot renders fixed-width hex,
        # a nested list-of-tuples (vector clock, observed tags) folds to None and rides the receipt.
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
        # one direction-parameterized fold: the `(verb, kind, annotate)` row is the only axis; the
        # Error arm returns verbatim so the faults `_convert` owns the span status exactly once.
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

- Owner: `WireProtoCodec` — the one boundary codec generic over the canonical `msgspec` `Struct` type and its paired generated protobuf `Message` class, transcoding interior shapes to the wire and back across the gRPC seam through the message-agnostic `google.protobuf.proto` functional façade so interior code never touches a `Message` and the wire never sees a `Struct`; it mints no wire vocabulary, owning only the projection. `WIRE_REGISTRY` derives one codec per imported `transport/shapes#REGISTRY_AND_DRIFT` `PROTO_VOCABULARY` row — the vocabulary, the binding rows, and the drift gate live one tier down, so this page holds zero shape knowledge and a registry re-mint here is the deleted `shapes -> wire` back-edge.
- Entry: `WireProtoCodec.encode` projects a canonical `Struct` to wire bytes on the `[02]-[WIRE_RAIL]` `Decode.routed` PRODUCER rail — `msgspec.to_builtins(value, str_keys=True)` lowers the struct to a proto-JSON-compatible mapping (base64-lowering every `bytes` slot to the `str` `json_format.ParseDict` expects for a proto `bytes` field), `json_format.ParseDict` fills a fresh generated `Message` (snake_case field names preserved against the proto schema), and `proto.serialize(message, deterministic=True)` emits the canonical deterministic protobuf binary; `WireProtoCodec.decode` reverses it on the `Decode.railed` CONSUMER rail — `proto.parse(self._message, payload)` constructs a fresh `Message` from the wire bytes, `json_format.MessageToDict(message, preserving_proto_field_name=True)` projects it to a mapping (proto `bytes` fields arriving as base64 `str`, every 64-bit `uint64`/`int64`/`fixed64` field as a decimal `str` per the proto3-JSON precision contract), and `msgspec.convert(mapping, self._struct, strict=False)` raises the mapping into the typed canonical `Struct` — base64-decoding each `str` back to its `bytes` slot AND coercing each 64-bit decimal `str` to its typed `int` slot under the field's own `Meta` bound, the `transport/shapes#VOCABULARY` `WireU64` decode-floor contract. `strict=False` is load-bearing on this leg: a strict convert rejects every 64-bit field as `Expected int, got str`, the same coercion the `clock#CLOCK` `CausalFrame.decode` runs on its carrier-string HLC halves. Both legs cross the one `boundary("wire", ...)` fence, and the module-level `codec(name)` resolves a named pair through `WIRE_REGISTRY.try_find(name).to_result(BoundaryFault(wire=(name, 0)))` — an absent name a typed code-carrying `wire` fault.
- Entry: `WireProtoCodec.encode_frames`/`decode_frames` are the varint-length-prefixed frame pair the streamed proto legs ride (`TokenChunk`, `GraphChunk`, `ArtifactFrame` — the server-stream and bidi contracts whose record-per-frame boundary a bare per-message `proto.serialize` concatenation loses): `encode_frames` folds a `Block[S]` through the unary lowering into one `io.BytesIO` via `proto.serialize_length_prefixed(message, buffer)` on the `Decode.routed` rail, and `decode_frames` drains a delimited buffer through `proto.parse_length_prefixed(self._message, buffer)` until exhaustion, raising each frame through the same `MessageToDict`/`convert` projection into `Block[S]` on the `Decode.railed` rail — one framing owner for every streamed leg, never ad hoc per-message encode with no frame boundary and never a hand-rolled varint.
- Growth: a new wire message is one `PROTO_VOCABULARY` row in `transport/shapes#REGISTRY_AND_DRIFT`; `WireProtoCodec` already transcodes it, `Decode` already rails both directions, and the frame pair already streams it — zero new surface here, no second codec.
- Boundary: the codec transcodes only — no wire vocabulary, no message shape, no JSON-as-wire-format on the production path (the deterministic protobuf binary is the gRPC wire, `json_format` the boundary projection only); a per-message hand client, a per-message-type encode helper beside the `proto.*` façade, a `Message` leaking into interior code, a `Struct` crossing the wire unprojected, a hand-rolled varint/tag framing, and a registry or `Struct` declaration re-minted here beside the shapes owner are the deleted forms. The `fault_detail` row's producer/consumer obligations are `transport/serve#SERVE`'s (`settle` packs the `grpc-status-details-bin` trailer, the invoke ingress decodes it); the `clock#CLOCK` `CausalFrame.of` lift onto the admitted context is the inbound owner's — `WireProtoCodec.decode` stays the pure generic `Struct`↔`Message` transcode.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from typing import Final

import msgspec
from expression.collections import Block, Map
from google.protobuf import json_format, proto
from google.protobuf.message import Message
from msgspec import Struct

from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.shapes import PROTO_VOCABULARY

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
            # `strict=False`: proto3 JSON emits 64-bit fields as DECIMAL STRINGS; the coercion raises
            # them onto the typed slot under the shapes-owned `WireU64` floor in the msgspec C core.
            mapping = json_format.MessageToDict(proto.parse(self._message, payload), preserving_proto_field_name=True)
            return msgspec.convert(mapping, self._struct, strict=False)

        return Decode.railed(self._struct.__name__, project)

    def encode_frames(self, values: Block[S]) -> RuntimeRail[bytes]:
        def framed() -> bytes:
            buffer = io.BytesIO()
            for value in values:  # Exemption: serialize_length_prefixed writes into one caller-owned BytesIO, the platform's streaming seam.
                proto.serialize_length_prefixed(json_format.ParseDict(msgspec.to_builtins(value, str_keys=True), self._message()), buffer)
            return buffer.getvalue()

        return Decode.routed(f"{self._struct.__name__}.frames", framed)

    def decode_frames(self, payload: bytes) -> RuntimeRail[Block[S]]:
        def drained() -> Block[S]:
            buffer, frames = io.BytesIO(payload), []
            # Exemption: parse_length_prefixed drains one caller-owned BytesIO, `None` the EOF signal — the platform's streaming seam.
            while (message := proto.parse_length_prefixed(self._message, buffer)) is not None:
                mapping = json_format.MessageToDict(message, preserving_proto_field_name=True)
                frames.append(msgspec.convert(mapping, self._struct, strict=False))
            return Block.of_seq(frames)

        return Decode.railed(f"{self._struct.__name__}.frames", drained)


# --- [TABLES] ---------------------------------------------------------------------------

WIRE_REGISTRY: Final[Map[str, WireProtoCodec[Struct, Message]]] = Map.of_seq(
    (name, WireProtoCodec(struct, message)) for name, struct, message in PROTO_VOCABULARY
)

# --- [OPERATIONS] -----------------------------------------------------------------------


def codec(name: str) -> RuntimeRail[WireProtoCodec[Struct, Message]]:
    return WIRE_REGISTRY.try_find(name).to_result(BoundaryFault(wire=(name, 0)))
```

## [04]-[CRDT_DECODE]

- Owner: `CrdtOp` — the field-less `array_like` tagged-union root the companion decodes from the C# `Rasm.Persistence/Version/commits#CRDT_WIRE` `CrdtOpWire` owner, and `CrdtArm` the closed ten-arm union (`SetOp`…`LeaveOp`, tag 0-9) the decoder targets so callers `match`/`assert_never` over the explicit set rather than the open base; each arm is the array-like flat decode whose fields ARE the producer `[Key(k)]` slots and which exposes the `clock#CLOCK` `Hlc` cell and `ElementId` it carries as DERIVED causal views through two field-less property mixins — `_Stamped.cell` lifts `(physical_ticks, logical) -> Hlc` for the HLC-stamped arms and `_Identified.id` lifts `(id_origin, id_logical) -> ElementId` for the element-id arms — so the canonical op and the wire arm are one struct and the reconstruction is one accessor per concept (a field-less intermediate `Struct` adds no `array_like` slot, so each arm's first declared field stays wire slot 1 under the integer-`tag` union). `CrdtOpDecode` is the one decode surface reading the MessagePack delta the `OpLogEntry.Payload` carries for `column-family=crdt` rows; `DecompressFn` the dependency-injected decompress protocol the decode closes over. The op-log crosses as MessagePack under `Lz4BlockArray`, NOT protobuf — the durability codec, distinct from the gRPC proto wire at `[03]`.
- Cases: ten op arms mirror the producer union tag-for-tag — `set`(0), `write`(1), `add`(2), `remove`(3), `increment`(4), `insert_after`(5), `delete`(6), `maintain`(7), `beat`(8), `leave`(9); LWW survives only as the `set` arm reconstructing the `LwwRegister`, the `beat`/`leave` arms carry the `EphemeralMap` presence delta the late-joining companion reconstructs from the op-log prefix. `set`/`write`/`beat`/`leave` carry flat `physical_ticks`/`logical` at sibling positions and inherit `_Stamped.cell`; `add`/`delete`/`insert_after` carry flat `id_origin`/`id_logical` pairs and inherit `_Identified.id` (`insert_after` adding the `predecessor` view over its `pred_*` pair); `remove` carries `observed: list[tuple[bytes, U64-half]]` tag pairs with the derived `observed_tags` projection; `write` adds the `context` version-vector pairs. Interior code reads `op.cell`/`op.id` while the wire shape stays the flat producer envelope.
- Auto: the wire is MessagePack — the reusable `msgspec.msgpack.Decoder` decodes the producer's keyed-FLAT `[tag, field0, field1, ...]` array per the `CRDT_OPLOG_WIRE_AMENDMENT`, where the C# `[MessagePack.Union(n, typeof(Case))]` integer `n` is the msgpack union tag and each `[property: Key(k)]` the array position (the cached-codec law, never a per-op `msgspec.msgpack.decode`). FLAT is the SOLE realized decode path: the amendment deprecates the MessagePack-csharp default two-element `[tag, sub-object]` nesting, so no standing nested-envelope machinery survives here — the deprecated framing is a proven-need Growth row below, entering only if a producer publishes it. Every HLC/element half rides the `transport/shapes#VOCABULARY` `WireU64` floor on the arm slot itself — `physical_ticks`/`logical`, the `id_logical`/`pred_logical` halves, and the unsigned half of the `context`/`quiescent`/`observed` pairs — so a negative half is a typed `ValidationError` lifted to `BoundaryFault` at decode by the msgspec C core (never only when the `@property` later constructs the canonical cell); the `<2**64` ceiling is unspellable on the slot (a `lt=2**64` `Meta` bound overflows the msgspec C-core int64 at `Decoder` build) and rides the single-mint producer domain — `_Stamped.cell`/`_Identified.id` construct `Hlc`/`ElementId` through the unchecked interior `__init__` exactly as the `clock#CLOCK` `tick` law licenses over already-domain-valid halves, `Meta` running only at `convert`-decode. `IncrementOp.delta` stays plain `int` — a signed PN-counter increment. `physical_ticks` is the C# `Instant.ToUnixTimeTicks()` 100-ns count the `clock#CLOCK` `Hlc` raises back to the NodaTime-equivalent instant; the `set` arm is the LWW `Adjudicate` survivor and the ten-arm union the strict superset the C# `Crdt.Merge` join-semilattice converges, so a companion decoding the prefix reconstructs the identical materialized state the producer holds. The companion authors no op kind the wire does not carry and re-mints no op vocabulary, the C# owner being the sole mint.
- Growth: a new op kind is one tagged-union arm carrying its flat producer slots and inheriting `_Stamped` or `_Identified` — the producer adds the wire tag, the companion adds the one arm, never ahead of the wire; a new op-log output modality (a windowed prefix, a checkpoint range) is one `CrdtOpDecode` entry over the same cached codecs; the deprecated NESTED producer framing re-enters as one framing member plus one `msgspec.Raw` re-frame row IF a producer ever publishes the `[tag, body]` envelope — proven-need, never standing machinery; an `Ext`-typed producer slot (a custom MessagePack scalar) enters as one `ext_hook=` seam on the cached `msgpack.Decoder` beside the injected `DecompressFn`, one constructor argument, never a parallel decoder; zero new surface, no second op vocabulary.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Protocol

import msgspec
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.clock import ElementId, Hlc
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.shapes import WireU64

# --- [TYPES] ----------------------------------------------------------------------------


class DecompressFn(Protocol):
    def __call__(self, payload: bytes) -> bytes: ...


# --- [MODELS] ---------------------------------------------------------------------------


class CrdtOp(Struct, frozen=True, tag_field="tag", array_like=True):
    # field-less tagged-union root: with `array_like=True` every base field would occupy a
    # leading array slot in every arm and shift the producer `[Key(k)]` positions, so the
    # base carries the discriminant only and each arm's first declared field IS wire slot 1.
    pass


class _Stamped(CrdtOp):
    # field-less HLC-view mixin: the stamped arms declare `physical_ticks`/`logical` at their
    # own producer slots and inherit the one `cell` accessor; the `WireU64` slot floor already
    # rejected a negative half at decode, and the ceiling rides the single-mint producer domain —
    # the lift is the unchecked interior construction clock's `tick` law licenses.
    @property
    def cell(self) -> Hlc:
        return Hlc(self.physical_ticks, self.logical)  # type: ignore[attr-defined]


class _Identified(CrdtOp):
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


# --- [OPERATIONS] -----------------------------------------------------------------------


class CrdtOpDecode:
    # one decoder family keyed by output arity (single arm vs prefix list) over the keyed-FLAT
    # producer contract; the reusable codecs over the closed `CrdtArm` union are the shared seam,
    # arity the only axis — never a per-op hand decoder.
    _arm: msgspec.msgpack.Decoder[CrdtArm] = msgspec.msgpack.Decoder(CrdtArm)
    _prefix: msgspec.msgpack.Decoder[list[CrdtArm]] = msgspec.msgpack.Decoder(list[CrdtArm])

    @classmethod
    def decode(cls, payload: bytes, decompress: DecompressFn) -> RuntimeRail[CrdtArm]:
        return Decode.railed("crdt", lambda: cls._arm.decode(decompress(payload)))

    @classmethod
    def stream(cls, payload: bytes, decompress: DecompressFn) -> RuntimeRail[Block[CrdtArm]]:
        return Decode.railed("crdt.prefix", lambda: Block.of_seq(cls._prefix.decode(decompress(payload))))
```

# [PY_RUNTIME_WIRE]

One wire-codec owner serves the companion transport: it decodes every C#-minted wire shape, mints no wire vocabulary of its own, and owns the `msgspec`-interior-to-`protobuf`-wire projection and the CRDT op-log decode. Vocabulary and binding table are `transport/shapes#REGISTRY_AND_DRIFT`'s — this page imports the rows and owns only transcode machinery, so a registry re-mint here is the deleted `shapes -> wire` back-edge.

Every transcode rides the one `Decode` aspect — a direction-parameterized OTel span with the `reliability/faults#FAULT` `boundary` fence — and a network fetch stays its transport owner's retry concern, handing this aspect only the acquired bytes. CRDT op-log bytes cross as MessagePack under a `Lz4BlockArray` envelope distinct from the gRPC proto wire, and `decompress` is a dependency-injected `DecompressFn` seam — never a hardwired `lz4` import, LZ4 being worker-gated with the envelope decode deferred.

## [01]-[INDEX]

- [01]-[WIRE_RAIL]: the traced-railed `Decode` aspect every wire boundary composes.
- [02]-[PROTO_TRANSCODE]: the registry-driven `Struct`-to-`Message` codec and its length-prefixed frame pair.
- [03]-[CRDT_DECODE]: the MessagePack op-log union with derived causal views and the injected decompress seam.

## [02]-[WIRE_RAIL]

- Owner: `Decode` is the one cross-cutting wire-boundary aspect every codec on this page composes — telemetry and fault conversion declared once and reused by the proto transcode and the CRDT decode, never repeated inline per codec and never a CONSUMER-kind span mis-scoping an egress encode.
- Entry: every ingress is buffered — the servicer hands `decode` the raw bytes and the durability decode reads the op-log payload — so `railed` and `routed` are the two entries, and the terminal decode `ValidationError` rides the `railed` boundary on the first decode, never a retry.
- Auto: `annotated` lowers through `msgspec.structs.asdict` — the field-NAME-keyed projection serving the `array_like` CRDT arms (the positional indices `to_builtins(array_like=True)` returns are meaningless) — keeping raw `bytes` for the fixed-width `.hex()` render, unlike the base64-lowering `to_builtins`.
- Packages: `msgspec`, `protobuf`, `opentelemetry-api`, and the faults/resilience rails per the fence imports; the `Status`/`record_exception` egress is the faults owner's `_convert`, never re-spelled here.
- Growth: a new wire boundary composes `Decode.railed`/`routed` and inherits span and fault with zero new cross-cutting code; a new transport direction is one `(verb, kind, annotate)` row on `_traced`.
- Boundary: every leg crosses the `railed`/`routed` span-and-`boundary` fence and the terminal decode fault converts exactly once — never a bare exception across the servicer and never a second async rail.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable

import msgspec
from expression.collections import Block
from opentelemetry import trace
from opentelemetry.trace import Span, SpanKind

from rasm.runtime.faults import SCOPES, RuntimeRail, Scope, boundary

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
```

## [03]-[PROTO_TRANSCODE]

- Owner: `WireProtoCodec` is generic over the `(Struct, Message)` pair through the message-agnostic `google.protobuf.proto` façade, so interior code never touches a `Message` and the wire never sees a `Struct`; `WIRE_REGISTRY` derives one codec per imported `PROTO_VOCABULARY` row, so the message family is rows rather than hand clients and this page holds zero shape knowledge.
- Entry: the frame pair exists because a bare per-message `proto.serialize` concatenation loses the record-per-frame boundary the server-stream and bidi contracts need — one framing owner for every streamed leg, never a hand-rolled varint.
- Growth: a new wire message is one `PROTO_VOCABULARY` row in `transport/shapes#REGISTRY_AND_DRIFT` — the codec, both rails, and the frame pair already carry it; zero new surface here.
- Boundary: the deterministic protobuf binary is the gRPC wire and `json_format` the boundary projection only — never JSON-as-wire-format on the production path. `fault_detail` trailer obligations are `transport/serve#SERVE`'s, and the `clock#CLOCK` `CausalFrame.of` lift is the inbound owner's — `decode` stays the pure generic transcode.

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

- Owner: the canonical op IS the wire arm — each arm's fields are the producer `[Key(k)]` slots, and the `clock#CLOCK` `Hlc`/`ElementId` reconstructions are derived property views through the field-less `_Stamped`/`_Identified` mixins, so no parallel wire-vs-canonical hierarchy or hand-written lift match survives. Interior code reads `op.cell`/`op.id` while the wire shape stays the flat producer envelope; `CrdtArm` closes the union so callers `match`/`assert_never` over the explicit set.
- Cases: LWW survives only as the `set` arm reconstructing the `LwwRegister`; `beat`/`leave` carry the `EphemeralMap` presence delta a late-joining companion reconstructs from the op-log prefix; `IncrementOp.delta` stays plain `int` — a signed PN-counter increment.
- Auto: FLAT is the SOLE realized decode path — the `CRDT_OPLOG_WIRE_AMENDMENT` deprecates the MessagePack-csharp default `[tag, sub-object]` nesting, so no standing nested-envelope machinery survives here. `physical_ticks` is the C# `Instant.ToUnixTimeTicks()` 100-ns count; the `set` arm is the LWW `Adjudicate` survivor and the union the strict superset the C# `Crdt.Merge` join-semilattice converges, so a companion decoding the prefix reconstructs the identical materialized state the producer holds — the companion authors no op kind the wire does not carry, the C# owner being the sole mint.
- Growth: a new op kind is one tagged-union arm inheriting `_Stamped` or `_Identified` — the producer adds the wire tag first, the companion follows, never ahead of the wire; the deprecated NESTED framing re-enters as one framing member with one `msgspec.Raw` re-frame row only if a producer publishes it; an `Ext`-typed producer slot enters as one `ext_hook=` seam on the cached decoder, never a parallel decoder.

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
    # `WireU64`'s slot floor already rejected a negative half at decode and the ceiling rides the single-mint producer
    # domain, so the lift is the unchecked interior construction the clock `tick` law licenses.
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
    # one decoder family keyed by output arity over the keyed-FLAT producer contract; the reusable cached codecs are the
    # shared seam — never a per-op `msgspec.msgpack.decode`.
    _arm: msgspec.msgpack.Decoder[CrdtArm] = msgspec.msgpack.Decoder(CrdtArm)
    _prefix: msgspec.msgpack.Decoder[list[CrdtArm]] = msgspec.msgpack.Decoder(list[CrdtArm])

    @classmethod
    def decode(cls, payload: bytes, decompress: DecompressFn) -> RuntimeRail[CrdtArm]:
        return Decode.railed("crdt", lambda: cls._arm.decode(decompress(payload)))

    @classmethod
    def stream(cls, payload: bytes, decompress: DecompressFn) -> RuntimeRail[Block[CrdtArm]]:
        return Decode.railed("crdt.prefix", lambda: Block.of_seq(cls._prefix.decode(decompress(payload))))
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

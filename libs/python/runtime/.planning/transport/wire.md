# [PY_RUNTIME_WIRE]

One wire owner serves the companion transport: it transcodes every contract wire shape, sources its vocabulary from the branch registry rather than declaring a second, and owns the `msgspec`-interior-to-`protobuf`-wire projection, the CRDT op-log codec in both directions, and the five converging state owners an op-log prefix materializes into. Vocabulary and binding table are `transport/shapes#REGISTRY_AND_DRIFT`'s — this page imports the rows and owns only transcode machinery, so a registry re-mint here is the deleted `shapes -> wire` back-edge.

Every transcode rides the one `Decode` aspect — a direction-parameterized OTel span with the `reliability/faults#FAULT` `boundary` fence — and a network fetch stays its transport owner's retry concern, handing this aspect only the acquired bytes. CRDT op-log bytes cross as MessagePack under a `Lz4BlockArray` envelope distinct from the gRPC proto wire, and both directions inject their envelope codec — `DecompressFn` and `CompressFn` — never a hardwired `lz4` import, LZ4 being worker-gated with the envelope crossing deferred.

## [01]-[INDEX]

- [02]-[WIRE_RAIL]: the traced-railed `Decode` aspect every wire boundary composes.
- [03]-[PROTO_TRANSCODE]: the registry-driven `Struct`-to-`Message` codec with its length-prefixed frame pair, and the decode-only mirror codec over the positional-record family.
- [04]-[CRDT_CODEC]: the MessagePack op-log union with derived causal views, the encode mirror, and the injected compress seams.
- [05]-[CRDT_STATE]: the five converging state owners and the one `converged` fold every replica materializes an op-log prefix through.

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

from rasm.runtime.faults import SCOPES, RuntimeRail, Scope, boundary, scoped

_TRACER = scoped(trace.get_tracer, SCOPES[Scope.WIRE])

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
- Owner: `WireMirrorCodec` is the second transcode arm the second schema authority earns — the appearance documents cross as the producer's positional integer-keyed MessagePack record and hold no descriptor by ruling, so a proto codec over them transcodes against a message that does not exist. It is DECODE-ONLY on the family's own single-producer law: an encode arm here would be the python-side lowering that law names as the drift defect, so the mirror carries no egress and the arity twin the proto codec grows has nothing to mirror.
- Entry: the frame pair exists because a bare per-message `proto.serialize` concatenation loses the record-per-frame boundary the server-stream and bidi contracts need — one framing owner for every streamed leg, never a hand-rolled varint.
- Auto: the mirror decode is a roster ZIP, not a positional struct decode — `array_like=True` would decode the whole nested tree in the C core, and `WireProvenance` forecloses it by crossing on BOTH wires: the same leaf `convert`s from a proto-derived MAPPING for the set documents, which an array-shaped struct rejects outright. So the zip stands the array's slots against `MIRROR_ORDER` and recurses on `MIRROR_NESTED`, and a SHORT array default-fills by construction — the roster outruns the slots and the missing tail never enters the mapping — which is exactly the producer's own version tolerance for a column appended past its frozen block.
- Growth: a new descriptor-backed message is one `PROTO_VOCABULARY` row in `transport/shapes#REGISTRY_AND_DRIFT` — the codec, both rails, and the frame pair already carry it; a new appearance document is one `MIRROR_VOCABULARY` row with its nested slots on `MIRROR_NESTED`; zero new surface here for either.
- Boundary: the deterministic protobuf binary is the gRPC wire and `json_format` the boundary projection only — never JSON-as-wire-format on the production path. `fault_detail` trailer obligations are `transport/serve#SERVE`'s, and the `evidence/clock#CLOCK` `CausalFrame.of` lift is the inbound owner's — `decode` stays the pure generic transcode. The producer's Web-camelCase JSON leg over the same records is the host-side debug projection and reaches no python decode: one wire per family, and the mirror reads the compact one the corpus contract names its authority.

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
from rasm.runtime.shapes import MIRROR_NESTED, MIRROR_ORDER, MIRROR_VOCABULARY, PROTO_VOCABULARY

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


class WireMirrorCodec[S: Struct]:
    # DECODE-ONLY by the appearance family's single-producer law: this branch reads the producer's positional record
    # and authors none, so no encode arm exists to fork the key order the producer pins.
    _ARRAY: msgspec.msgpack.Decoder[list[object]] = msgspec.msgpack.Decoder(list[object])

    def __init__(self, name: str, struct: type[S]) -> None:
        self._name, self._struct = name, struct

    def decode(self, payload: bytes) -> RuntimeRail[S]:
        def project() -> S:
            return msgspec.convert(_keyed(self._name, self._ARRAY.decode(payload)), self._struct, strict=False)

        return Decode.railed(self._struct.__name__, project)


# --- [TABLES] ---------------------------------------------------------------------------

WIRE_REGISTRY: Final[Map[str, WireProtoCodec[Struct, Message]]] = Map.of_seq(
    (name, WireProtoCodec(struct, message)) for name, struct, message in PROTO_VOCABULARY
)

MIRROR_REGISTRY: Final[Map[str, WireMirrorCodec[Struct]]] = Map.of_seq((name, WireMirrorCodec(name, struct)) for name, struct, _ in MIRROR_VOCABULARY)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _keyed(name: str, slots: list[object]) -> dict[str, object]:
    # the positional array as the name-keyed mapping `convert` admits: `zip` without `strict` IS the version tolerance
    # — a producer's older record runs short and its tail never enters the mapping, so every absent slot takes the
    # struct's own default exactly as the producer's trailing-growth contract promises. A nested slot recurses by NAME
    # rather than by value shape, because a nested record and a `repeated` scalar both arrive as bare lists and only
    # the declared correspondence tells them apart.
    order = MIRROR_ORDER[name]
    return {
        slot: _keyed(MIRROR_NESTED[slot], value) if slot in MIRROR_NESTED and isinstance(value, list) else value
        for slot, value in zip(order, slots)  # Exemption: the short-array default-fill IS the producer's version tolerance
    }


def codec(name: str) -> RuntimeRail[WireProtoCodec[Struct, Message]]:
    # a roster miss is `config`, never `wire`: the `wire` case is reserved for a NUMERIC protocol code as the
    # discriminant, and a name absent from the registry carries none — a `0` in that slot spells a real protocol
    # status the lookup never observed, and a reader gating on the code reads a fabricated zero as one.
    return WIRE_REGISTRY.try_find(name).to_result(BoundaryFault(config=(name, "unregistered-proto-codec")))


def mirror(name: str) -> RuntimeRail[WireMirrorCodec[Struct]]:
    # the mirror twin of `codec`: two registries, two lookups, one fault CLASS — a caller naming a descriptor row here
    # (or a mirror row there) gets the same caller-repairable refusal rather than a codec transcoding against nothing,
    # while the detail names WHICH roster refused so the repair is one lookup rather than a two-registry search.
    return MIRROR_REGISTRY.try_find(name).to_result(BoundaryFault(config=(name, "unregistered-mirror-codec")))
```

## [04]-[CRDT_CODEC]

- Owner: the canonical op IS the wire arm — each arm's fields are the producer `[Key(k)]` slots, and the `evidence/clock#CLOCK` `Hlc`/`ElementId` reconstructions are derived property views through the field-less `_Stamped`/`_Identified` mixins, so no parallel wire-vs-canonical hierarchy or hand-written lift match survives. Interior code reads `op.cell`/`op.id` while the wire shape stays the flat producer envelope; `CrdtArm` closes the union so callers `match`/`assert_never` over the explicit set.
- Cases: LWW survives only as the `set` arm reconstructing the `LwwRegister`; `beat`/`leave` carry the `EphemeralMap` presence delta a late-joining companion reconstructs from the op-log prefix; `IncrementOp.delta` stays plain `int` — a signed PN-counter increment.
- Auto: FLAT is the SOLE realized codec path — the `CRDT_OPLOG_WIRE_AMENDMENT` deprecates the MessagePack-csharp default `[tag, sub-object]` nesting, so no standing nested-envelope machinery survives here. `physical_ticks` is the C# `Instant.ToUnixTimeTicks()` 100-ns count; the `set` arm is the LWW `Adjudicate` survivor and the union the join-semilattice `[05]`'s `converged` fold materializes, so a peer decoding the prefix reconstructs the identical state any minter holds.
- Auto: `CrdtOpEncode` is the exact mirror of `CrdtOpDecode` — one cached encoder over the same closed union at both arities — so this owner AUTHORS ops as well as merging them and the `crdt-op` corpus contract becomes a round-trip claim rather than a read-only one. A minted op and a decoded op therefore agree on the keyed-FLAT layout by construction; an encode path spelled per call site would fork exactly the field order the producer pins.
- Growth: a new op kind is one tagged-union arm inheriting `_Stamped` or `_Identified`, one `converged` arm, and one `CrdtState` column where it opens a new convergence family — the producer adds the wire tag first, the companion follows, never ahead of the wire; the deprecated NESTED framing re-enters as one framing member with one `msgspec.Raw` re-frame row only if a producer publishes it; an `Ext`-typed producer slot enters as one `ext_hook=`/`enc_hook=` seam on the cached codecs, never a parallel decoder.

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


class CompressFn(Protocol):
    # the egress mirror of the decompress seam: LZ4 stays worker-gated, so the envelope codec is injected at both
    # directions and this page imports no compressor. A default identity thunk is the rejected form — it would ship
    # an uncompressed frame under a `Lz4BlockArray` envelope the peer decompressor then rejects.
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


class CrdtOpEncode:
    # the mirror of `CrdtOpDecode` at both arities over ONE cached encoder: msgspec writes the tag field and the
    # declared field order the producer's `[Key(k)]` slots pin, so an authored op is byte-compatible with a decoded one
    # by construction and the `crdt-op` corpus fixture grades a round trip. The prefix arm encodes the whole `Block` as
    # one MessagePack array, exactly the shape `_prefix` drains, so a per-op concatenation cannot fork the envelope.
    _arm: msgspec.msgpack.Encoder = msgspec.msgpack.Encoder()

    @classmethod
    def encode(cls, op: CrdtArm, compress: CompressFn) -> RuntimeRail[bytes]:
        return Decode.routed("crdt", lambda: compress(cls._arm.encode(op)))

    @classmethod
    def stream(cls, ops: Block[CrdtArm], compress: CompressFn) -> RuntimeRail[bytes]:
        return Decode.routed("crdt.prefix", lambda: compress(cls._arm.encode(list(ops))))
```

## [05]-[CRDT_STATE]

- Owner: `CrdtState` is the materialized replica — one column per convergence family, `LwwRegister`, `OrSet`, `Rga`, `PnCounter`, and `EphemeralMap` — and `converged(state, ops)` the one fold every replica replays an op-log prefix through. Each family owns its own absorb law as a method on its own shape, so the fold's arms carry routing alone and no arm re-derives another family's merge; the whole state is frozen, so a replay returns a successor rather than mutating a cell two readers share.
- Cases: the ten `CrdtArm` members close onto five families — `set` and `write` both land the register, `add`/`remove` the observed set, `insert_after`/`delete` the sequence, `increment` the counter, `beat`/`leave` the presence map, and `maintain` prunes what its quiescence list declares settled. Two register arms are ONE owner because they answer every discriminant identically and differ only in whether the writer offered a causal context; a second register type beside the first would fork the survivor law the whole branch converges on.
- Law: every survivor decision on this page — the register's and the presence map's alike — reads the `evidence/clock#CLOCK` owner's `compare` and folds its `Ordering`, never a raw operator or a re-derived sign at the adjudication seam, because an operator discloses its equality behaviour only through the direction of the sign and two families adjudicating one clock by two spellings drift the instant either bound flips. The register's `equal` arm breaks the tie on origin bytes, since two replicas legitimately stamp one cell and an arbitrary choice there diverges the two materializations permanently; the presence arms resolve `equal` toward the beat, so a leave never evicts the event its own cell names.
- Law: a causally-contexted `write` survives on DOMINANCE first — a version vector covering every entry the held one carries happened strictly after, so no stamp comparison runs — and only a genuinely concurrent pair falls back to the cell tiebreak. An unconditional last-writer-wins over the same pair silently drops a causal successor whose physical clock lagged its predecessor's.
- Law: every arm is idempotent and order-insensitive under the tags the wire already carries — an `add` re-adds one tag to a set, a `remove` tombstones exactly the tags it observed so a re-delivered add stays dead, a `delete` tombstones an id whether or not its insert landed yet, and a `beat` loses to a later cell — so a redelivered prefix converges to the same state and no fold counts a duplicate twice. `increment` is the one arm carrying no id, so its at-most-once property is the op-log's own content-keyed append rather than this fold's; per-origin bucketing is what keeps the sum order-insensitive regardless.
- Entry: an `insert_after` naming a predecessor no arriving prefix defined is the fold's one refusal — the transport delivers an ordered prefix, so a gap is a defect rather than a normal out-of-order arrival, and inserting at the head instead would silently reorder the sequence every peer holds.
- Growth: a new convergence family is one `CrdtState` column with its own absorb method and its arms' routing rows; a new op on a standing family is one arm and no column; a new tiebreak axis is one field on the family that needs it, reaching the fold through its own method.
- Boundary: this owner materializes and never transports — the codec above owns the bytes, the clock owner the comparison algebra, and the durable op-log its own persistence. No column carries a wall-clock instant: ordering is the `Hlc` cell alone, so a host whose clock drifts still converges.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Final, assert_never

from expression import Error, Ok
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace

from rasm.runtime.clock import ElementId, Hlc
from rasm.runtime.faults import BoundaryFault, RuntimeRail
from rasm.runtime.shapes import WireU64

# `CrdtArm` and its ten members are this module's [04]-[CRDT_CODEC] owners — one module, two declaration regions.

# --- [CONSTANTS] --------------------------------------------------------------------------

# the sequence's own zero: a producer names it as the predecessor of a head insert, so the head case is a VALUE the
# insert arm already handles rather than a nullable predecessor slot every reader re-checks.
_ROOT: Final[ElementId] = ElementId(b"", 0)

# --- [MODELS] ---------------------------------------------------------------------------


class LwwRegister(Struct, frozen=True):
    # one register for both write arms: `cell` is the stamp, `origin` the tiebreak the equal arm reads, and `context`
    # the version vector a causally-contexted write offers — empty for the bare `set` arm, which is exactly why an
    # uncontexted write never claims dominance over a contexted one it cannot order.
    value: bytes = b""
    cell: Hlc = Hlc(0, 0)
    origin: bytes = b""
    context: Map[bytes, int] = Map.empty()

    def absorbed(self, candidate: "LwwRegister") -> "LwwRegister":
        # the survivor decision, read through ONE `fold` call site on the clock owner's own verdict — never a
        # re-derived sign comparison here. `equal` breaks on origin bytes: two replicas can legitimately stamp one
        # cell, and choosing arbitrarily there leaves the two materializations permanently divergent.
        return self.cell.compare(candidate.cell).fold(
            before=lambda: candidate, equal=lambda: candidate if candidate.origin > self.origin else self, after=lambda: self
        )

    def written(self, candidate: "LwwRegister") -> "LwwRegister":
        # dominance FIRST, stamp second: a context covering the held vector happened strictly after, so the physical
        # halves never enter the decision. Only a concurrent pair — neither dominating — reaches the tiebreak, which is
        # the one place an unconditional LWW drops a causal successor whose clock lagged.
        return (
            candidate
            if _dominates(candidate.context, self.context)
            else self
            if _dominates(self.context, candidate.context)
            else self.absorbed(candidate)
        )


class OrSet(Struct, frozen=True):
    # observed-remove set: live tags per element beside the tombstoned tag set. Both halves are required for
    # order-insensitivity — a remove carrying the tags it observed must stay effective when its add arrives later,
    # which a bare element-set cannot express and a re-delivered add would silently resurrect.
    tags: Map[bytes, Block[ElementId]] = Map.empty()
    tombstones: Block[ElementId] = Block.empty()

    def added(self, element: bytes, tag: ElementId) -> "OrSet":
        held = self.tags.try_find(element).default_value(Block.empty())
        return OrSet(tags=self.tags.add(element, held if tag in held else held.append(Block.singleton(tag))), tombstones=self.tombstones)

    def removed(self, observed: Block[ElementId]) -> "OrSet":
        # tombstoning exactly the OBSERVED tags is what makes the remove commute with a later add of a fresh tag —
        # clearing the element's whole slot instead would erase a concurrent add the remover never saw.
        fresh = observed.filter(lambda tag: tag not in self.tombstones)
        return OrSet(tags=self.tags, tombstones=self.tombstones.append(fresh))

    def members(self) -> Block[bytes]:
        return Block.of_seq(
            element for element, held in self.tags.items() if any(tag not in self.tombstones for tag in held)
        )


class Rga(Struct, frozen=True):
    # replicated growable array as an insertion TREE flattened on read: `after` maps each predecessor id to the ids
    # inserted directly after it, held in the synthesized `ElementId` order, so two concurrent inserts against one
    # predecessor sort identically on every replica. A positional index cannot carry that — the same offset names
    # different elements on two replicas the instant either inserts — and the tombstone set is separate from `values`
    # so a delete arriving ahead of its insert still suppresses it.
    values: Map[ElementId, bytes] = Map.empty()
    after: Map[ElementId, Block[ElementId]] = Map.empty()
    tombstones: Block[ElementId] = Block.empty()

    def inserted(self, predecessor: ElementId, identity: ElementId, value: bytes) -> "Rga":
        siblings = self.after.try_find(predecessor).default_value(Block.empty())
        return Rga(
            values=self.values.add(identity, value),
            after=self.after.add(predecessor, siblings if identity in siblings else siblings.append(Block.singleton(identity)).sort()),
            tombstones=self.tombstones,
        )

    def deleted(self, identity: ElementId) -> "Rga":
        # a delete tombstones whether or not its insert landed yet, so a prefix delivering the pair in either order
        # converges; dropping the value outright would let a later insert of that id resurrect it.
        return Rga(
            values=self.values,
            after=self.after,
            tombstones=self.tombstones if identity in self.tombstones else self.tombstones.append(Block.singleton(identity)),
        )

    def defined(self, identity: ElementId) -> bool:
        return self.values.try_find(identity).is_some()


class PnCounter(Struct, frozen=True):
    # per-origin buckets rather than one running total: summing on READ makes the fold order-insensitive, where a
    # single accumulator would depend on delivery order the instant two origins interleave. The arm carries no id, so
    # at-most-once delivery is the op-log's own content-keyed append and never a dedup this fold could perform.
    buckets: Map[bytes, int] = Map.empty()

    def incremented(self, origin: bytes, delta: int) -> "PnCounter":
        return PnCounter(buckets=self.buckets.add(origin, self.buckets.try_find(origin).default_value(0) + delta))

    @property
    def value(self) -> int:
        return sum(self.buckets.values())


class EphemeralMap(Struct, frozen=True):
    # presence: one stamped state per origin, a later cell winning and a `leave` clearing only when its own cell
    # dominates the held beat — an unconditional clear would let a stale leave evict a live replica that beat after it.
    beats: Map[bytes, tuple[bytes, Hlc]] = Map.empty()

    def beaten(self, origin: bytes, state: bytes, cell: Hlc) -> "EphemeralMap":
        return EphemeralMap(beats=self.beats.add(origin, self._survivor(origin, state, cell)))

    def left(self, origin: bytes, cell: Hlc) -> "EphemeralMap":
        # the clear folds the clock owner's own verdict exactly as the register's adjudication does: a stale leave
        # arriving after its origin beat again must not evict a live replica, and the `equal` arm KEEPS the beat
        # because one cell names one event the beat already published.
        cleared = self.beats.try_find(origin).map(
            lambda held: held[1].compare(cell).fold(before=lambda: True, equal=lambda: False, after=lambda: False)
        )
        return EphemeralMap(beats=self.beats.remove(origin) if cleared.default_value(False) else self.beats)

    def pruned(self, quiescent: Block[bytes]) -> "EphemeralMap":
        return EphemeralMap(beats=Map.of_seq((origin, held) for origin, held in self.beats.items() if origin not in quiescent))

    def _survivor(self, origin: bytes, state: bytes, cell: Hlc) -> tuple[bytes, Hlc]:
        # the held beat survives only when it happened strictly after, read through the SAME `compare`/`fold` seam the
        # register uses — a raw `>` discloses its equality behaviour only through the direction of the sign, so two
        # families adjudicating one clock by two spellings drift the instant either flips a bound.
        return (
            self.beats.try_find(origin)
            .map(lambda held: held[1].compare(cell).fold(before=lambda: (state, cell), equal=lambda: (state, cell), after=lambda: held))
            .default_value((state, cell))
        )


class CrdtState(Struct, frozen=True):
    # the materialized replica: one column per convergence family, each carrying its own absorb law, so `converged`
    # routes and never merges. Frozen whole — a replay returns a successor, so a reader holding the prior state keeps
    # a consistent snapshot rather than watching a cell shift under it.
    register: LwwRegister = LwwRegister()
    observed: OrSet = OrSet()
    sequence: Rga = Rga()
    counter: PnCounter = PnCounter()
    presence: EphemeralMap = EphemeralMap()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _dominates(candidate: Map[bytes, int], held: Map[bytes, int]) -> bool:
    # strict causal dominance: every entry the held vector carries is matched or exceeded, and at least one is
    # exceeded. Non-strict would report two identical vectors as ordered and route a genuinely concurrent pair past
    # the tiebreak that exists for it.
    return all(candidate.try_find(origin).default_value(0) >= counter for origin, counter in held.items()) and any(
        candidate.try_find(origin).default_value(0) > held.try_find(origin).default_value(0) for origin in candidate.keys()
    )


def _vector(context: list[tuple[bytes, WireU64]]) -> Map[bytes, int]:
    return Map.of_seq((origin, int(counter)) for origin, counter in context)


def converged(state: CrdtState, ops: Block[CrdtArm]) -> RuntimeRail[CrdtState]:
    # the ONE fold every replica materializes an op-log prefix through: each arm routes to its family's own absorb
    # method and the total `match` proves the ten arms are covered, so a new op kind breaks here at type-check rather
    # than converging silently onto the wrong column. The fold threads the rail so the single causal refusal below can
    # stop the replay at the op that broke it, naming the predecessor rather than the whole prefix.
    def stepped(rail: RuntimeRail[CrdtState], op: CrdtArm) -> RuntimeRail[CrdtState]:
        return rail.bind(lambda held: _applied(held, op))

    return ops.fold(stepped, Ok(state))


def _applied(state: CrdtState, op: CrdtArm) -> RuntimeRail[CrdtState]:
    match op:
        case SetOp():
            return Ok(replace(state, register=state.register.absorbed(LwwRegister(value=op.value, cell=op.cell, origin=op.origin))))
        case WriteOp():
            candidate = LwwRegister(value=op.value, cell=op.cell, origin=op.origin, context=_vector(op.context))
            return Ok(replace(state, register=state.register.written(candidate)))
        case AddOp():
            return Ok(replace(state, observed=state.observed.added(op.element, op.id)))
        case RemoveOp():
            return Ok(replace(state, observed=state.observed.removed(op.observed_tags)))
        case IncrementOp():
            return Ok(replace(state, counter=state.counter.incremented(op.origin, op.delta)))
        case InsertAfterOp():
            # the fold's ONE refusal: the transport delivers an ordered prefix, so an unknown predecessor is a gap in
            # that prefix rather than a normal out-of-order arrival, and head-inserting instead would reorder the
            # sequence every peer holds. The root id is the sequence's own zero, always defined. `boundary` and never
            # `wire`: the gap is a seam classification of delivered material carrying no protocol code, and a `0` in
            # the `wire` slot would publish a status this fold never read.
            return (
                Ok(replace(state, sequence=state.sequence.inserted(op.predecessor, op.id, op.value)))
                if op.predecessor == _ROOT or state.sequence.defined(op.predecessor)
                else Error(
                    BoundaryFault(
                        boundary=("crdt.insert_after", f"unknown-predecessor:{op.predecessor.origin.hex()}:{op.predecessor.logical}")
                    )
                )
            )
        case DeleteOp():
            return Ok(replace(state, sequence=state.sequence.deleted(op.id)))
        case BeatOp():
            return Ok(replace(state, presence=state.presence.beaten(op.origin, op.state, op.cell)))
        case LeaveOp():
            return Ok(replace(state, presence=state.presence.left(op.origin, op.cell)))
        case MaintainOp():
            # quiescence is a declaration a producer publishes, so the prune reads it as data: presence rows for
            # settled origins retire and the sequence and set keep every tombstone, because a tombstone is what makes
            # a redelivered add or insert stay dead and reclaiming it re-opens exactly that hole.
            return Ok(replace(state, presence=state.presence.pruned(Block.of_seq(origin for origin, _ in op.quiescent))))
        case _ as unreachable:
            assert_never(unreachable)
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

# [PY_RUNTIME_WIRE]

One wire owner serves the companion transport: it transcodes every contract wire shape, sources its vocabulary from the branch registry rather than declaring a second, and owns the `msgspec`-interior-to-`protobuf`-wire projection, the CRDT op-log codec in both directions, and the five converging state owners an op-log prefix materializes into. Vocabulary and binding table are `transport/shapes#REGISTRY_AND_DRIFT`'s — this page imports the rows and owns only transcode machinery, so a registry re-mint here is the deleted `shapes -> wire` back-edge.

Every transcode rides the one `Decode` aspect — a direction-parameterized OTel span with the `reliability/faults#FAULT` `boundary` fence — and a network fetch stays its transport owner's retry concern, handing this aspect only the acquired bytes. Every lift on this page names the provider classes it reaches and takes its subject from a `RuntimeLeg.WIRE` roster row, so no codec raise crosses as a bare-`Exception` funnel and no refusal spells a coordinate the roster never declared. CRDT op-log bytes cross as MessagePack under a `Lz4BlockArray` envelope distinct from the gRPC proto wire, and both directions inject one `Envelope` — the codec beside the provider class it raises — never a hardwired `lz4` import, LZ4 being worker-gated with the envelope crossing deferred.

## [01]-[INDEX]

- [02]-[WIRE_RAIL]: `Decode` — the traced-railed aspect every wire boundary composes.
- [03]-[PROTO_TRANSCODE]: registry-driven `Struct`-to-`Message` codec with its length-prefixed frame pair, beside the decode-only mirror codec over the positional-record family.
- [04]-[CRDT_CODEC]: MessagePack op-log union with derived causal views, the encode mirror, and the one injected envelope seam.
- [05]-[CRDT_STATE]: five converging state owners, each publishing its own read, beside the one `converged` fold a replica materializes an op-log prefix through.

## [02]-[WIRE_RAIL]

- Owner: `Decode` is the one cross-cutting wire-boundary aspect every codec on this page composes — telemetry and fault conversion declared once and reused by the proto transcode and the CRDT decode, never repeated inline per codec and never a CONSUMER-kind span mis-scoping an egress encode.
- Entry: every ingress is buffered — the servicer hands `decode` the raw bytes and the durability decode reads the op-log payload — so `railed` and `routed` are the two entries, and the terminal decode `ValidationError` rides the `railed` boundary on the first decode, never a retry.
- Auto: `annotated` lowers through `msgspec.structs.asdict` — the field-NAME-keyed projection serving the `array_like` CRDT arms (the positional indices `to_builtins(array_like=True)` returns are meaningless) — keeping raw `bytes` for the fixed-width `.hex()` render, unlike the base64-lowering `to_builtins`.
- Law: the fault coordinate is a `RuntimeLeg.WIRE` roster row and never a free subject string, so `railed` and `routed` differ by ROW rather than by a verb literal and the SAME row names the span head — one declaration fixes what a trace shows and what `facts()` publishes, where two spellings drift the moment either moves. The row's own posture is what makes a codec fault terminal: re-decoding identical bytes fails identically, so a re-drive gate reading the ingress class alone would re-offer a refusal no retry can clear.
- Law: `catch` is REQUIRED at both entries and every composer names its provider ROOTS rather than a leaf list — `msgspec.MsgspecError` covers decode, encode, and constraint validation, `message.Error` the binary frame, `json_format.Error` the mapping projection — so a defect raised inside a thunk propagates as the defect it is instead of railing as a codec fault. The one plane this aspect cannot import is the INJECTED envelope codec, which therefore carries its own raise class on the value rather than being guessed at here.
- Packages: `msgspec`, `protobuf`, `opentelemetry-api`, and the faults/resilience rails per the fence imports; the `Status`/`record_exception` egress is the faults owner's `_convert`, never re-spelled here.
- Growth: a new wire boundary composes `Decode.railed`/`routed` and inherits span and fault with zero new cross-cutting code; a new transport direction is one `(row, kind, annotate)` triple on `_traced`, its row supplying span head and fault coordinate at once.
- Boundary: every leg crosses the `railed`/`routed` span-and-`boundary` fence and the terminal decode fault converts exactly once — never a bare exception across the servicer and never a second async rail.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable

import msgspec
from expression.collections import Block
from opentelemetry import trace
from opentelemetry.trace import Span, SpanKind

from rasm.runtime.faults import SCOPES, WIRE_DECODE, WIRE_ENCODE, Catch, FaultRow, RuntimeRail, Scope, boundary, scoped

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
    def _traced[T](cls, at: FaultRow, kind: SpanKind, subject: str, run: Callable[[], T], catch: Catch, *, annotate: bool) -> RuntimeRail[T]:
        # one direction-parameterized fold: the `(row, kind, annotate)` triple is the only axis, and the row carries BOTH
        # names at once — `at.subject` heads the span and coordinates the fault — so a direction cannot be spelled one way
        # in a trace and another on the rail. The Error arm returns verbatim so `_convert` owns the span status once.
        with _TRACER.start_as_current_span(f"{at.subject}.{subject}", kind=kind) as span:
            rail = boundary(at, run, catch=catch)
            return rail.map(lambda frame: cls.annotated(span, frame)) if annotate else rail

    @classmethod
    def railed[T](cls, subject: str, decode: Callable[[], T], *, catch: Catch) -> RuntimeRail[T]:
        return cls._traced(WIRE_DECODE, SpanKind.CONSUMER, subject, decode, catch, annotate=True)

    @classmethod
    def routed[T](cls, subject: str, encode: Callable[[], T], *, catch: Catch) -> RuntimeRail[T]:
        return cls._traced(WIRE_ENCODE, SpanKind.PRODUCER, subject, encode, catch, annotate=False)
```

## [03]-[PROTO_TRANSCODE]

- Owner: `WireProtoCodec` is generic over the `(Struct, Message)` pair through the message-agnostic `google.protobuf.proto` façade, so interior code never touches a `Message` and the wire never sees a `Struct`; `WIRE_REGISTRY` derives one codec per imported `PROTO_VOCABULARY` row, so the message family is rows rather than hand clients and this page holds zero shape knowledge.
- Owner: `WireMirrorCodec` is the second transcode arm the second schema authority earns — the appearance documents cross as the producer's positional integer-keyed MessagePack record and hold no descriptor by ruling, so a proto codec over them transcodes against a message that does not exist. It is DECODE-ONLY on the family's own single-producer law: an encode arm here IS the python-side lowering that law names as the drift defect, so the mirror carries no egress and the arity twin the proto codec grows has nothing to mirror.
- Entry: the frame pair exists because a bare per-message `proto.serialize` concatenation loses the record-per-frame boundary the server-stream and bidi contracts need — one framing owner for every streamed leg, never a hand-rolled varint.
- Auto: the mirror decode is a roster ZIP, not a positional struct decode — `array_like=True` decodes the whole nested tree in the C core, and `WireProvenance` forecloses that by crossing on BOTH wires: the same leaf `convert`s from a proto-derived MAPPING for the set documents, which an array-shaped struct rejects outright. Zipping therefore stands the array's slots against `MIRROR_ORDER` and recurses on `MIRROR_NESTED`, and a SHORT array default-fills by construction — the roster outruns the slots and the missing tail never enters the mapping — which is exactly the producer's own version tolerance for a column appended past its frozen block.
- Growth: a new descriptor-backed message is one `PROTO_VOCABULARY` row in `transport/shapes#REGISTRY_AND_DRIFT` — the codec, both rails, and the frame pair already carry it; a new appearance document is one `MIRROR_VOCABULARY` row with its nested slots on `MIRROR_NESTED`; zero new surface here for either.
- Boundary: deterministic protobuf binary IS the gRPC wire and `json_format` the boundary projection only — never JSON-as-wire-format on the production path. `fault_detail` trailer obligations are `transport/serve#SERVE`'s, and the `evidence/clock#CLOCK` `CausalFrame.of` lift is the inbound owner's — `decode` stays the pure generic transcode. Producer-side Web-camelCase JSON over the same records is the host debug projection and reaches no python decode: one wire per family, and the mirror reads the compact one the corpus contract names its authority.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
from typing import Final

import msgspec
from expression.collections import Block, Map
from google.protobuf import json_format, message, proto
from google.protobuf.message import Message
from msgspec import Struct

from rasm.runtime.faults import WIRE_CODEC, Catch, RuntimeRail
from rasm.runtime.shapes import MIRROR_NESTED, MIRROR_ORDER, MIRROR_VOCABULARY, PROTO_VOCABULARY

# --- [CONSTANTS] --------------------------------------------------------------------------

# the provider planes each leg reaches, named at their ROOTS: one class per provider covers that provider's whole raise
# surface, where a leaf roster (`ValidationError`, `ParseError`) silently omits the sibling the next release adds. The
# mirror leg reaches `msgspec` alone — no descriptor and no proto3 JSON hop exist on that wire — so naming the other two
# there would claim a plane the codec never touches.
_PROTO_RAISES: Final[Catch] = (msgspec.MsgspecError, message.Error, json_format.Error)
_MIRROR_RAISES: Final[Catch] = msgspec.MsgspecError

# --- [MODELS] ---------------------------------------------------------------------------


class WireProtoCodec[S: Struct, M: Message]:
    def __init__(self, struct: type[S], message: type[M]) -> None:
        self._struct, self._message = struct, message

    def encode(self, value: S) -> RuntimeRail[bytes]:
        return Decode.routed(
            self._struct.__name__,
            lambda: proto.serialize(json_format.ParseDict(msgspec.to_builtins(value, str_keys=True), self._message()), deterministic=True),
            catch=_PROTO_RAISES,
        )

    def decode(self, payload: bytes) -> RuntimeRail[S]:
        def project() -> S:
            # `strict=False`: proto3 JSON emits 64-bit fields as DECIMAL STRINGS; the coercion raises
            # them onto the typed slot under the shapes-owned `WireU64` floor in the msgspec C core.
            mapping = json_format.MessageToDict(proto.parse(self._message, payload), preserving_proto_field_name=True)
            return msgspec.convert(mapping, self._struct, strict=False)

        return Decode.railed(self._struct.__name__, project, catch=_PROTO_RAISES)

    def encode_frames(self, values: Block[S]) -> RuntimeRail[bytes]:
        def framed() -> bytes:
            buffer = io.BytesIO()
            for value in values:  # Exemption: serialize_length_prefixed writes into one caller-owned BytesIO, the platform's streaming seam.
                proto.serialize_length_prefixed(json_format.ParseDict(msgspec.to_builtins(value, str_keys=True), self._message()), buffer)
            return buffer.getvalue()

        return Decode.routed(f"{self._struct.__name__}.frames", framed, catch=_PROTO_RAISES)

    def decode_frames(self, payload: bytes) -> RuntimeRail[Block[S]]:
        def drained() -> Block[S]:
            buffer, frames = io.BytesIO(payload), []
            # Exemption: parse_length_prefixed drains one caller-owned BytesIO, `None` the EOF signal — the platform's streaming seam.
            while (message := proto.parse_length_prefixed(self._message, buffer)) is not None:
                mapping = json_format.MessageToDict(message, preserving_proto_field_name=True)
                frames.append(msgspec.convert(mapping, self._struct, strict=False))
            return Block.of_seq(frames)

        return Decode.railed(f"{self._struct.__name__}.frames", drained, catch=_PROTO_RAISES)


class WireMirrorCodec[S: Struct]:
    # DECODE-ONLY by the appearance family's single-producer law: this branch reads the producer's positional record
    # and authors none, so no encode arm exists to fork the key order the producer pins.
    _ARRAY: msgspec.msgpack.Decoder[list[object]] = msgspec.msgpack.Decoder(list[object])

    def __init__(self, name: str, struct: type[S]) -> None:
        self._name, self._struct = name, struct

    def decode(self, payload: bytes) -> RuntimeRail[S]:
        def project() -> S:
            return msgspec.convert(_keyed(self._name, self._ARRAY.decode(payload)), self._struct, strict=False)

        return Decode.railed(self._struct.__name__, project, catch=_MIRROR_RAISES)


# --- [TABLES] ---------------------------------------------------------------------------

WIRE_REGISTRY: Final[Map[str, WireProtoCodec[Struct, Message]]] = Map.of_seq(
    (name, WireProtoCodec(struct, message)) for name, struct, message in PROTO_VOCABULARY
)

MIRROR_REGISTRY: Final[Map[str, WireMirrorCodec[Struct]]] = Map.of_seq((name, WireMirrorCodec(name, struct)) for name, struct, _ in MIRROR_VOCABULARY)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _keyed(name: str, slots: list[object]) -> dict[str, object]:
    # positional array as the name-keyed mapping `convert` admits: `zip` without `strict` IS the version tolerance —
    # a producer's older record runs short and its tail never enters the mapping, so every absent slot takes the
    # struct's own default exactly as the producer's trailing-growth contract promises. A nested slot recurses by NAME
    # rather than by value shape, because a nested record and a `repeated` scalar both arrive as bare lists and
    # only their declared correspondence tells them apart.
    order = MIRROR_ORDER[name]
    return {
        slot: _keyed(MIRROR_NESTED[slot], value) if slot in MIRROR_NESTED and isinstance(value, list) else value
        for slot, value in zip(order, slots)  # Exemption: the short-array default-fill IS the producer's version tolerance
    }


def codec(name: str) -> RuntimeRail[WireProtoCodec[Struct, Message]]:
    # a roster miss is `config`, never `wire`: the `wire` case is reserved for a NUMERIC protocol code as the
    # discriminant, and a name absent from the registry carries none — a `0` in that slot spells a real protocol
    # status the lookup never observed, and a reader gating on the code reads a fabricated zero as one. The requested
    # name rides the row's DETAIL and never the subject, since a subject minted from caller input keys the fault series
    # on that input and one misspelt roster then reads as a thousand distinct defects; `_with` keeps the mint on the
    # ABSENT branch alone, so the registry hit that every served call takes pays nothing for the refusal it never makes.
    return WIRE_REGISTRY.try_find(name).to_result_with(lambda: WIRE_CODEC.raised("proto", name))


def mirror(name: str) -> RuntimeRail[WireMirrorCodec[Struct]]:
    # mirror twin of `codec` through the SAME row: two registries, two lookups, one refusal whose `roster` slot names
    # which one answered, so a caller naming a descriptor row here (or a mirror row there) repairs in one lookup rather
    # than a two-registry search, and a second row would fork one defect into two a reader must know to check for.
    return MIRROR_REGISTRY.try_find(name).to_result_with(lambda: WIRE_CODEC.raised("mirror", name))
```

## [04]-[CRDT_CODEC]

- Owner: the canonical op IS the wire arm — each arm's fields are the producer `[Key(k)]` slots, and the `evidence/clock#CLOCK` `Hlc`/`ElementId` reconstructions are derived property views through the field-less `_Stamped`/`_Identified` mixins, so no parallel wire-vs-canonical hierarchy or hand-written lift match survives. Interior code reads `op.cell`/`op.id` while the wire shape stays the flat producer envelope; `CrdtArm` closes the union so callers `match`/`assert_never` over the explicit set.
- Law: a frozen wire's slot SET, slot ORDER, and field numbers are INVARIANT under an interior-owner migration. `_Stamped`/`_Identified` are field-less views, so a moved `evidence/clock#CLOCK` owner re-targets by re-pointing one property body while not a single wire slot moves, and the migration lands as that re-point in ONE unit. Deleting the round trip before the re-target lands is the BARRED order: the window between them strands every peer decoder mid-flight, and the decode-only mirror family — single-producer by ruling — cannot re-emit to close a window it never wrote. This is the COMPLEMENT of the additive rule at `transport/shapes#VOCABULARY`, never the same ordering: additive growth is producer-first because the wire must carry a column before a reader names it, while a re-target moves no column and therefore admits no gap at all. A slot genuinely seated at the wrong index still rides the roster spelling `shapes#REGISTRY_AND_DRIFT`'s mirror census catches, never a tear.
- Law: the union root carries `field`, the one slot every producer arm leads with, so slot 1 is fixed for all ten by construction and each arm's own roster starts at slot 2; a per-arm declaration lets one arm omit it and shift every slot behind it by one, which decodes as silent corruption rather than a refusal.
- Cases: LWW survives only as the `set` arm reconstructing the `LwwRegister`; `beat`/`leave` carry the `EphemeralMap` presence delta a late-joining companion reconstructs from the op-log prefix; `increment` carries the producer's `(sequence, positive, negative)` cumulative triple, so the counter absorbs by ordinal and a replayed op re-lands the same total.
- Cases: `OpLogEntry` is the pinned envelope and its slot order IS the producer record's declaration order — sequence, identity, model, entity key, lane, verb, payload, payload content key, trace slot, closure, actor, then the two HLC halves — so every column the changefeed writes survives the crossing and no positional reader shifts. Short envelopes are the corruption an `array_like` decode cannot report: MessagePack hands the reader the first N slots, so `family` reads the model guid, `kind` reads the entity key, and the payload reads the lane string, each as a well-typed value nothing refuses. `seq` resumes a drain and orders nothing, `id` orders and dedups, `content_key` names the payload bytes while `id` names the operation, `trace` carries the changefeed's own 16-byte trace-id slot rather than a propagation fold, `closure` carries the descendant keys a transfer differences against what a peer holds, and the `Raw` payload lets a non-`crdt` lane cross this reader untouched.
- Law: the trace slot is a TOP-LEVEL envelope column beside `content_key` and never a key inside `payload`, so a lane whose payload this reader never opens still continues its producing trace, and `closure` stays a declared key set the transfer differences rather than a manifest a consumer walks. Both columns cross for every lane, so no arm reads a column another lane omits.
- Law: `OperationId` carries the `(origin, counter)` dot beside the frontier its minter observed, and identity NEVER derives from payload content — two peers writing identical bytes are two operations, so a content-keyed log reports the second as a duplicate and discards a real edit. Vector slots — `context` here, `WriteOp.context` and `MaintainOp.quiescent` alike — arrive ASCENDING by origin bytes, the producer's canonical order, because a hash-ordered slot list gives one causal position a different digest per runtime and per insertion history, which is what keeps the shared fixture unfreezable.
- Auto: FLAT is the SOLE realized codec path — the `CRDT_OPLOG_WIRE_AMENDMENT` deprecates the MessagePack-csharp default `[tag, sub-object]` nesting, so no standing nested-envelope machinery survives here. `physical_ticks` is the C# `Instant.ToUnixTimeTicks()` 100-ns count; the `set` arm is the LWW `Adjudicate` survivor and the union the join-semilattice `[05]`'s `converged` fold materializes, so a peer decoding the prefix reconstructs the identical state any minter holds.
- Auto: `CrdtOpEncode` is the exact mirror of `CrdtOpDecode` — one cached encoder over the same closed union at both arities — so this owner AUTHORS ops as well as merging them and the `crdt-op` corpus contract becomes a round-trip claim rather than a read-only one. Minted and decoded ops therefore agree on the keyed-FLAT layout by construction, where an encode path spelled per call site forks exactly the field order the producer pins.
- Growth: a new envelope column is one `OpLogEntry` field the producer pins first, never a sibling struct; a new op kind is one tagged-union arm inheriting `_Stamped` or `_Identified`, one `converged` arm, and one `CrdtState` column where it opens a new convergence family — the producer adds the wire tag first, the companion follows, never ahead of the wire; the deprecated NESTED framing re-enters as one framing member with one `msgspec.Raw` re-frame row only if a producer publishes it; an `Ext`-typed producer slot enters as one `ext_hook=`/`enc_hook=` seam on the cached codecs, never a parallel decoder; a new envelope compressor is one `Envelope` value carrying its own `raises`, never a `catch` widened here to cover a class this page cannot import.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Final

import msgspec
from expression.collections import Block, Map
from msgspec import Struct

from rasm.runtime.clock import ElementId, Hlc
from rasm.runtime.faults import Catch, RuntimeRail
from rasm.runtime.shapes import WireU64

# --- [CONSTANTS] --------------------------------------------------------------------------

_CRDT_LANE: Final[str] = "crdt"

# the MessagePack plane both directions reach, named at its root so decode, encode, and constraint validation ride one
# class; the envelope's own raise arrives WITH the injected codec, a seam naming a compressor it never imports catching
# nothing it claims to.
_MSGPACK_RAISES: Final[Catch] = msgspec.MsgspecError

# --- [MODELS] ---------------------------------------------------------------------------


class Envelope(Struct, frozen=True):
    # ONE injected seam for both directions over the `Lz4BlockArray` layer alone — `OpLogEntry` is the ENTRY record and
    # never this shape. `apply` is the codec and `raises` the provider classes it can throw; the two travel together
    # because the lift's `catch` cannot name a compressor this page never imports, LZ4 staying worker-gated with the
    # crossing deferred. The two direction-named Protocols this replaces were structurally IDENTICAL, so a compressor
    # already satisfied the decompressor's contract and the split bought no safety whatever — direction is recovered
    # from the parameter each value fills. `raises` bounds at `Exception` for the reason the fault owner does: a
    # cancellation is scope-owned flow control, never an ingress class. An identity `apply` default is the rejected
    # form, since it ships an uncompressed frame under an envelope the peer decompressor then rejects, and an EMPTY
    # `raises` is the honest declaration for a codec that cannot fail rather than a stand-in for one nobody read.
    apply: Callable[[bytes], bytes]
    raises: tuple[type[Exception], ...]


class CrdtOp(Struct, frozen=True, tag_field="tag", array_like=True):
    # tagged-union root carrying the ONE slot every producer arm leads with: `array_like=True` lays base fields ahead
    # of each arm's own, so declaring `field` here puts it at wire slot 1 on all ten arms by construction and the arm's
    # first declared field lands at slot 2. Declaring it per arm instead lets one arm forget it and shift every slot
    # after it by one, which decodes as total corruption rather than a refusal. It carries no default: the producer
    # writes the slot on every arm, and a default would force one onto every field of every arm behind it.
    field: str


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
    # cumulative per-origin halves under the producer's own ordinal, never a signed delta: the halves are monotone, so
    # a redelivered op re-absorbs to the identical total where a delta fold counts it twice.
    origin: bytes
    sequence: WireU64
    positive: WireU64
    negative: WireU64


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
    # two retirement declarations in one op: named origins the producer settled, plus an ABSOLUTE liveness horizon on
    # its own tick axis that every older presence row retires against.
    quiescent: list[tuple[bytes, WireU64]]
    liveness_ticks: WireU64


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


class OperationId(Struct, frozen=True, array_like=True):
    # operation identity: the `(origin, counter)` dot names one operation across every runtime, and `context` — the
    # frontier its minter had observed — answers happened-before between two ids with no feed walk. Keyed on payload
    # content instead, two peers writing identical bytes collapse into one operation and the second edit vanishes.
    origin: bytes
    counter: WireU64
    context: list[tuple[bytes, WireU64]]

    @property
    def frontier(self) -> Map[bytes, int]:
        # this dot INCLUDED: what a replica joins once the entry lands, and what the next id at this origin carries
        # as its own context.
        return Map.of_seq((origin, int(counter)) for origin, counter in self.context).add(self.origin, int(self.counter))

    def applied(self, frontier: Map[bytes, int]) -> bool:
        return frontier.try_find(self.origin).default_value(0) >= int(self.counter)


class TraceSlot(Struct, frozen=True, array_like=True):
    # changefeed trace SLOT and never a propagation fold: the producer READS its ambient activity once and stores the
    # 16-byte trace-id beside the tracestate bytes, so this branch continues that parent and re-mints no propagator.
    # Absence is the EMPTY slot the producer already writes for a correlation that decoded as nothing, so `parented`
    # tests the sixteen bytes rather than a nullable column every reader re-checks.
    trace_id: bytes = b""
    tracestate: bytes = b""

    @property
    def parented(self) -> bool:
        return len(self.trace_id) == 16


class OpLogEntry(Struct, frozen=True, array_like=True):
    # pinned entry envelope, positional, and its slot order IS the producer record's own declaration order —
    # `[seq, [origin, counter, context], model, entity, family, kind, <raw>, content_key, [trace_id, tracestate],
    # closure, actor, physical_ticks, logical]`. Declaring fewer slots than the producer writes is SILENT corruption
    # rather than a refusal: an `array_like` decode reads the leading N and every column behind the truncation shifts
    # into a neighbour that happens to type-check, so the envelope tracks the producer whole and a column the producer
    # gains lands here before any consumer reads it. `seq` orders and resumes the producer's own drain and NOTHING
    # else; `id` is what dedups and orders causally, because a transport ordinal diverges the moment two peers resume
    # from different frontiers. `content_key` names the PAYLOAD bytes where `id` names the operation, so two peers
    # writing identical bytes stay two operations. `payload` stays `Raw` so a non-`crdt` lane crosses this reader
    # untouched — decoding every entry as an op misreads a scalar row as a union tag, and re-framing per lane is one
    # `Raw` decode rather than a second envelope.
    seq: WireU64
    id: OperationId
    model: bytes
    entity: str
    family: str
    kind: str
    payload: msgspec.Raw
    content_key: bytes
    trace: TraceSlot
    closure: list[bytes]
    actor: str
    physical_ticks: WireU64
    logical: WireU64

    @property
    def stamp(self) -> Hlc:
        # producer's `Instant.ToUnixTimeTicks()` 100-ns count beside its logical half, reconstructed exactly as the
        # op arms' `_Stamped` mixin does, so the envelope and its payload read one clock spelling.
        return Hlc(self.physical_ticks, self.logical)

    @property
    def origin(self) -> bytes:
        # store identity is the DOT's own origin and never a second stored column — the producer derives it the same
        # way, so a peer resuming a feed and a peer ordering an operation read one value.
        return self.id.origin

    def missing(self, held: Callable[[bytes], bool], /) -> Block[bytes]:
        # transfer is a SET DIFFERENCE over the declared closure rather than a tree walk, which is why the column
        # crosses at all: a consumer asks what it lacks without opening the payload it has not fetched yet.
        return Block.of_seq(key for key in self.closure if not held(key))


# --- [OPERATIONS] -----------------------------------------------------------------------


class CrdtOpDecode:
    # one decoder family keyed by output arity over the keyed-FLAT producer contract; the reusable cached codecs are the
    # shared seam — never a per-op `msgspec.msgpack.decode`.
    _arm: msgspec.msgpack.Decoder[CrdtArm] = msgspec.msgpack.Decoder(CrdtArm)
    _prefix: msgspec.msgpack.Decoder[list[OpLogEntry]] = msgspec.msgpack.Decoder(list[OpLogEntry])

    @classmethod
    def decode(cls, payload: bytes, envelope: Envelope) -> RuntimeRail[CrdtArm]:
        return Decode.railed("crdt", lambda: cls._arm.decode(envelope.apply(payload)), catch=(_MSGPACK_RAISES, *envelope.raises))

    @classmethod
    def stream(cls, payload: bytes, envelope: Envelope) -> RuntimeRail[Block[OpLogEntry]]:
        return Decode.railed(
            "crdt.prefix", lambda: Block.of_seq(cls._prefix.decode(envelope.apply(payload))), catch=(_MSGPACK_RAISES, *envelope.raises)
        )

    @classmethod
    def ops(cls, entries: Block[OpLogEntry]) -> RuntimeRail[Block[tuple[OperationId, CrdtArm]]]:
        # lane filter and second-stage `Raw` decode in one pass: a foreign-lane entry drops here rather than reaching
        # `converged`, and each surviving op keeps its id, since the fold's dedup and its compaction gate both read
        # that id, which no op recovers on its own. No envelope crosses: the `Raw` slot is already decompressed
        # material the prefix decode handed over, so re-declaring one here would name a codec this leg never calls.
        crdt = entries.filter(lambda entry: entry.family == _CRDT_LANE)
        return Decode.railed(
            "crdt.ops", lambda: Block.of_seq((entry.id, cls._arm.decode(bytes(entry.payload))) for entry in crdt), catch=_MSGPACK_RAISES
        )


class CrdtOpEncode:
    # mirror of `CrdtOpDecode` at both arities over ONE cached encoder: msgspec writes the tag field and the
    # declared field order the producer's `[Key(k)]` slots pin, so an authored op is byte-compatible with a decoded one
    # by construction and the `crdt-op` corpus fixture grades a round trip. The prefix arm encodes the whole `Block` as
    # one MessagePack array, exactly the shape `_prefix` drains, so a per-op concatenation cannot fork the envelope.
    _arm: msgspec.msgpack.Encoder = msgspec.msgpack.Encoder()

    @classmethod
    def encode(cls, op: CrdtArm, envelope: Envelope) -> RuntimeRail[bytes]:
        return Decode.routed("crdt", lambda: envelope.apply(cls._arm.encode(op)), catch=(_MSGPACK_RAISES, *envelope.raises))

    @classmethod
    def stream(cls, entries: Block[OpLogEntry], envelope: Envelope) -> RuntimeRail[bytes]:
        return Decode.routed(
            "crdt.prefix", lambda: envelope.apply(cls._arm.encode(list(entries))), catch=(_MSGPACK_RAISES, *envelope.raises)
        )
```

## [05]-[CRDT_STATE]

- Owner: `CrdtState` is the materialized replica — one column per convergence family, `LwwRegister`, `OrSet`, `Rga`, `PnCounter`, and `EphemeralMap` — and `converged(state, ops)` the one fold every replica replays an op-log prefix through. Each family owns its own absorb law as a method on its own shape, so the fold's arms carry routing alone and no arm re-derives another family's merge; the whole state is frozen, so a replay returns a successor rather than mutating a cell two readers share.
- Cases: ten `CrdtArm` members close onto five families — `set` and `write` both land the register, `add`/`remove` the observed set, `insert_after`/`delete` the sequence, `increment` the counter, `beat`/`leave` the presence map, and `maintain` retires the presence rows its quiescence list names and its liveness horizon expires. Two register arms are ONE owner because they answer every discriminant identically and differ only in whether the writer offered a causal context; a second register type beside the first forks the survivor law the whole branch converges on.
- Law: every survivor decision on this page — the register's and the presence map's alike — reads the `evidence/clock#CLOCK` owner's `compare` and folds its `Ordering`, never a raw operator or a re-derived sign at the adjudication seam, because an operator discloses its equality behaviour only through the direction of the sign and two families adjudicating one clock by two spellings drift the instant either bound flips. `LwwRegister.absorbed` breaks its `equal` arm on origin bytes, since two replicas legitimately stamp one cell and an arbitrary choice there diverges the two materializations permanently; the presence arms resolve `equal` toward the beat, so a leave never evicts the event its own cell names.
- Law: a causally-contexted `write` survives on DOMINANCE first — a version vector covering every entry the held one carries happened strictly after, so no stamp comparison runs — and only a genuinely concurrent pair falls back to the cell tiebreak. Unconditional last-writer-wins over the same pair silently drops a causal successor whose physical clock lagged its predecessor's.
- Law: every arm is idempotent and order-insensitive under the tags the wire already carries — an `add` re-adds one tag to a set, a `remove` tombstones exactly the tags it observed so a re-delivered add stays dead, a `delete` tombstones an id whether or not its insert landed yet, and a `beat` loses to a later cell — so a redelivered prefix converges to the same state and no fold counts a duplicate twice. `increment` carries the producer's per-origin ordinal in place of an id: the bucket absorbs the highest ordinal whole over cumulative halves, so a replay re-lands the same total, and per-origin bucketing keeps the sum order-insensitive across origins.
- Law: `converged` folds the op column; `OpLogEntry.seq` orders and resumes the delivery the codec drains, and no arm reads it, because a state keyed on a transport ordinal diverges the moment two peers resume from different frontiers.
- Law: `replayed` is the entry-level fold and holds the two gates the op-level fold structurally cannot: a dot already under the applied frontier skips as a redelivery, where content equality reports a second genuine edit of identical bytes as that same skip and loses it; and a `maintain` whose minter never observed the horizon it declares refuses, since reclaiming a tombstone a concurrent insert still needs resurrects a deleted element. Both gates read `OperationId`, so the arms below stay identity-free and a replica that drains ops without their ids keeps neither guarantee.
- Law: every convergence column publishes its own READ on the shape that owns the state — `LwwRegister.value`, `OrSet.members`, `Rga.ordered`, `PnCounter.value`, `EphemeralMap.beats` — because a write-only column converges a state no replica can project back out, and a reader seated anywhere else re-derives an ordering law the family already holds. `Rga` alone reads by WALK rather than by field, so it alone takes the shared `reliability/faults#FAULT` `Depth` and rails a typed exhaustion where the other four are total and take no bound. No graph substrate backs that walk: `networkx` names `data` and `geometry` as its owners, and `libs/python/.planning/RULINGS.md` keeps graph reducers plural precisely because each owns a node index a merged owner re-keys — re-keying an adjacency whose determinism IS the synthesized `ElementId` order onto a foreign index is the ruled defect, not the repair.
- Entry: an `insert_after` naming a predecessor no arriving prefix defined is the fold's one refusal — the transport delivers an ordered prefix, so a gap is a defect rather than a normal out-of-order arrival, and inserting at the head instead silently reorders the sequence every peer holds.
- Growth: a new convergence family is one `CrdtState` column with its own absorb method and its arms' routing rows; a new op on a standing family is one arm and no column; a new tiebreak axis is one field on the family that needs it, reaching the fold through its own method; a new family projection is one method on the family that owns its state, never a reader seated on `CrdtState` or re-derived at a consumer.
- Boundary: this owner materializes and never transports — the codec above owns the bytes, the clock owner the comparison algebra, and the durable op-log its own persistence. No column carries a wall-clock instant: ordering is the `Hlc` cell alone, so a host whose clock drifts still converges.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Final, assert_never

from expression import Error, Ok, Option
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace

from rasm.runtime.clock import ElementId, Hlc
from rasm.runtime.faults import WIRE_INSERT, WIRE_MAINTAIN, WIRE_ORDERED, Depth, RuntimeRail
from rasm.runtime.shapes import WireU64

# `CrdtArm`, its ten members, and `OpLogEntry` are this module's [04]-[CRDT_CODEC] owners — one module, two regions.

# --- [CONSTANTS] --------------------------------------------------------------------------

# sequence's own zero: a producer names it as the predecessor of a head insert, so the head case is a VALUE the
# insert arm already handles rather than a nullable predecessor slot every reader re-checks.
_ROOT: Final[ElementId] = ElementId(b"", 0)

# --- [MODELS] ---------------------------------------------------------------------------


class LwwRegister(Struct, frozen=True):
    # one register for both write arms: `cell` is the stamp, `origin` the tiebreak the equal arm reads, and
    # `context` the version vector a causally-contexted write offers — empty for the bare `set` arm, which is why an
    # uncontexted write never claims dominance over a contexted one it cannot order.
    value: bytes = b""
    cell: Hlc = Hlc(0, 0)
    origin: bytes = b""
    context: Map[bytes, int] = Map.empty()

    def absorbed(self, candidate: "LwwRegister") -> "LwwRegister":
        # survivor decision, read through ONE `fold` call site on the clock owner's own verdict — never a
        # re-derived sign comparison here. `equal` breaks on origin bytes: two replicas can legitimately stamp one
        # cell, and choosing arbitrarily there leaves the two materializations permanently divergent.
        return self.cell.compare(candidate.cell).fold(
            before=lambda: candidate, equal=lambda: candidate if candidate.origin > self.origin else self, after=lambda: self
        )

    def written(self, candidate: "LwwRegister") -> "LwwRegister":
        # dominance FIRST, stamp second: a context covering the held vector happened strictly after, so the physical
        # halves never enter the decision. Only a concurrent pair — neither dominating — reaches the tiebreak, and
        # that is the one place an unconditional LWW drops a causal successor whose clock lagged.
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
    # replicated growable array as an insertion TREE its own `ordered` flattens: `after` maps each predecessor id to
    # the ids inserted directly after it, held in the synthesized `ElementId` order, so two concurrent inserts against
    # one predecessor sort identically on every replica. A positional index cannot carry that — the same offset names
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

    def ordered(self, *, bound: Depth) -> RuntimeRail[Block[bytes]]:
        # the READ half this family owes, and the reason `after` holds sorted siblings at all: a preorder from the
        # sequence's own zero emits a node ahead of the ids inserted after it, siblings in the synthesized `ElementId`
        # order `inserted` already sorted, so two replicas holding one op set flatten to the identical `Block` and no
        # positional index ever enters. A tombstoned id suppresses its own VALUE and never its DESCENT — a delete
        # removes an element, not the elements inserted after it — and `_ROOT` needs no arm, carrying no value to find.
        # The frontier is EXPLICIT because depth here IS length: a sequence typed left to right is one chain, so a
        # native descent trades this bound's typed refusal for an interpreter `RecursionError` no rail can carry, and
        # `docs/stacks/python/iteration.md` `[CATAMORPHISM]` licenses this streaming frontier for exactly that reason.
        # `bound` counts LEVELS with the root as the first, so a flat sequence needs two and the declared floor's one
        # admits the root alone. The adjacency is ACYCLIC by the fold's own gates — a predecessor is defined before its
        # successor lands and an id mints once at its origin — so `fixpoint` terminates and the bound is a DEPTH
        # ceiling, never a cycle guard; a producer breaking id uniqueness trips that ceiling instead of hanging.
        frontier: Block[tuple[ElementId, Depth]] = Block.singleton((_ROOT, bound))
        emitted: list[bytes] = []
        while not frontier.is_empty():  # Exemption: streaming preorder frontier — chain-shaped depth forfeits the recursive form
            (identity, depth), frontier = frontier.head(), frontier.tail()
            if identity not in self.tombstones:
                emitted.extend(self.values.try_find(identity).to_list())
            children = self.after.try_find(identity).default_value(Block.empty())
            if not children.is_empty():
                match depth.stepped():
                    case Option(tag="some", some=stepped):
                        # the whole child block pushes in FRONT and IN ORDER, so heads pop left to right and the walk
                        # is preorder without a reversal the sibling sort already paid for.
                        frontier = Block.of_seq((child, stepped) for child in children).append(frontier)
                    case Option(tag="none"):
                        # typed exhaustion and never a short `Block`: a truncated read certifies a sequence every peer
                        # holds longer as complete. The DECLARED bound spells the refusal, not the remainder that spent it.
                        return Error(bound.exhausted(WIRE_ORDERED))
                    case _ as unreachable:
                        assert_never(unreachable)
        # the emission seals ONCE: the persistent carrier concatenates in linear time, so growing it per element is
        # quadratic in the sequence — the one place this walk holds a mutable accumulator, and it never escapes.
        return Ok(Block.of_seq(emitted))


class PnCounter(Struct, frozen=True):
    # per-origin CUMULATIVE halves under the producer's ordinal, not a running total: summing on READ makes the fold
    # order-insensitive where a single accumulator depends on delivery order the instant two origins interleave, and
    # absorbing by sequence makes it idempotent where a delta fold double-counts every redelivery.
    buckets: Map[bytes, tuple[int, int, int]] = Map.empty()

    def incremented(self, origin: bytes, sequence: int, positive: int, negative: int) -> "PnCounter":
        # highest ordinal per origin wins WHOLE: both halves are monotone at the producer, so a lower ordinal
        # carries a prefix of what the held pair already absorbed and re-applying it walks the total backwards.
        candidate = (sequence, positive, negative)
        survivor = self.buckets.try_find(origin).map(lambda held: held if held[0] >= sequence else candidate).default_value(candidate)
        return PnCounter(buckets=self.buckets.add(origin, survivor))

    @property
    def value(self) -> int:
        return sum(positive - negative for _, positive, negative in self.buckets.values())


class EphemeralMap(Struct, frozen=True):
    # presence: one stamped state per origin, a later cell winning and a `leave` clearing only when its own cell
    # dominates the held beat — an unconditional clear would let a stale leave evict a live replica that beat after it.
    beats: Map[bytes, tuple[bytes, Hlc]] = Map.empty()

    def beaten(self, origin: bytes, state: bytes, cell: Hlc) -> "EphemeralMap":
        return EphemeralMap(beats=self.beats.add(origin, self._survivor(origin, state, cell)))

    def left(self, origin: bytes, cell: Hlc) -> "EphemeralMap":
        # clearing folds the clock owner's own verdict exactly as the register's adjudication does: a stale leave
        # arriving after its origin beat again must not evict a live replica, and the `equal` arm KEEPS the beat
        # because one cell names one event the beat already published.
        cleared = self.beats.try_find(origin).map(
            lambda held: held[1].compare(cell).fold(before=lambda: True, equal=lambda: False, after=lambda: False)
        )
        return EphemeralMap(beats=self.beats.remove(origin) if cleared.default_value(False) else self.beats)

    def pruned(self, quiescent: Block[bytes], horizon: int) -> "EphemeralMap":
        # two retirement rules in one pass: a declared origin retires by name, and any beat whose physical half sits at
        # or below the liveness horizon retires by age. The horizon is absolute on the producer's tick axis, so every
        # replica compacts the identical set — a locally derived window prunes by whichever host clock read it and
        # leaves two replicas holding different presence maps for the same prefix.
        return EphemeralMap(
            beats=Map.of_seq(
                (origin, held)
                for origin, held in self.beats.items()
                if origin not in quiescent and held[1].physical_ticks > horizon
            )
        )

    def _survivor(self, origin: bytes, state: bytes, cell: Hlc) -> tuple[bytes, Hlc]:
        # held beats survive only where they happened strictly after, read through the SAME `compare`/`fold` seam the
        # register uses — a raw `>` discloses its equality behaviour only through the direction of the sign, so two
        # families adjudicating one clock by two spellings drift the instant either flips a bound.
        return (
            self.beats.try_find(origin)
            .map(lambda held: held[1].compare(cell).fold(before=lambda: (state, cell), equal=lambda: (state, cell), after=lambda: held))
            .default_value((state, cell))
        )


class CrdtState(Struct, frozen=True):
    # materialized replica: one column per convergence family, each carrying its own absorb law and its own read, so
    # `converged` routes and never merges and no consumer re-derives a projection here. Frozen whole — a replay returns
    # a successor, so a reader holding the prior state keeps a consistent snapshot rather than watching a cell shift.
    register: LwwRegister = LwwRegister()
    observed: OrSet = OrSet()
    sequence: Rga = Rga()
    counter: PnCounter = PnCounter()
    presence: EphemeralMap = EphemeralMap()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _dominates(candidate: Map[bytes, int], held: Map[bytes, int]) -> bool:
    # strict causal dominance: every entry the held vector carries is matched or exceeded, and at least one is
    # exceeded. Non-strict reports two identical vectors as ordered and routes a genuinely concurrent pair past the
    # tiebreak that exists for it.
    return all(candidate.try_find(origin).default_value(0) >= counter for origin, counter in held.items()) and any(
        candidate.try_find(origin).default_value(0) > held.try_find(origin).default_value(0) for origin in candidate.keys()
    )


def _vector(context: list[tuple[bytes, WireU64]]) -> Map[bytes, int]:
    return Map.of_seq((origin, int(counter)) for origin, counter in context)


def replayed(
    state: CrdtState, frontier: Map[bytes, int], entries: Block[tuple[OperationId, CrdtArm]]
) -> RuntimeRail[tuple[CrdtState, Map[bytes, int]]]:
    # entry-level fold, threading the applied frontier beside the state. Two gates the op-level fold cannot run: one
    # dot already under the frontier is a REDELIVERY and skips — content equality would report a second genuine edit
    # of identical bytes as that same skip and lose it — and a `maintain` whose minter never observed the horizon it
    # declares refuses, since applying it reclaims a tombstone a concurrent insert still needs and resurrects a
    # deleted element. Both gates read the id, which is exactly why the op carries one.
    def stepped(
        rail: RuntimeRail[tuple[CrdtState, Map[bytes, int]]], entry: tuple[OperationId, CrdtArm]
    ) -> RuntimeRail[tuple[CrdtState, Map[bytes, int]]]:
        identity, op = entry
        return rail.bind(
            lambda held: Ok(held)
            if identity.applied(held[1])
            else _admissible(identity, op).bind(
                lambda _: _applied(held[0], op).map(lambda moved: (moved, _joined(held[1], identity.frontier)))
            )
        )

    return entries.fold(stepped, Ok((state, frontier)))


def _admissible(identity: OperationId, op: CrdtArm) -> RuntimeRail[None]:
    return (
        Error(WIRE_MAINTAIN.raised(identity.origin.hex()))
        if isinstance(op, MaintainOp) and not _dominates(identity.frontier, _vector(op.quiescent))
        else Ok(None)
    )


def _joined(held: Map[bytes, int], advanced: Map[bytes, int]) -> Map[bytes, int]:
    return Map.of_seq(
        (origin, max(held.try_find(origin).default_value(0), advanced.try_find(origin).default_value(0)))
        for origin in set(held.keys()) | set(advanced.keys())
    )


def converged(state: CrdtState, ops: Block[CrdtArm]) -> RuntimeRail[CrdtState]:
    # ONE fold every replica materializes an op-log prefix through: each arm routes to its family's own absorb
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
            return Ok(replace(state, counter=state.counter.incremented(op.origin, op.sequence, op.positive, op.negative)))
        case InsertAfterOp():
            # ONE fold refusal: the transport delivers an ordered prefix, so an unknown predecessor is a gap in
            # that prefix rather than a normal out-of-order arrival, and head-inserting instead would reorder the
            # sequence every peer holds. The root id is the sequence's own zero, always defined. The row's arm is
            # `boundary` and never `wire`: the gap is a seam classification of delivered material carrying no protocol
            # code, and a `0` there publishes a status this fold never read.
            return (
                Ok(replace(state, sequence=state.sequence.inserted(op.predecessor, op.id, op.value)))
                if op.predecessor == _ROOT or state.sequence.defined(op.predecessor)
                else Error(WIRE_INSERT.raised(op.predecessor.origin.hex(), str(op.predecessor.logical)))
            )
        case DeleteOp():
            return Ok(replace(state, sequence=state.sequence.deleted(op.id)))
        case BeatOp():
            return Ok(replace(state, presence=state.presence.beaten(op.origin, op.state, op.cell)))
        case LeaveOp():
            return Ok(replace(state, presence=state.presence.left(op.origin, op.cell)))
        case MaintainOp():
            # quiescence and the liveness horizon are both declarations a producer publishes, so the prune reads them
            # as data: presence rows for settled or expired origins retire and the sequence and set keep every
            # tombstone, because a tombstone is what makes a redelivered add or insert stay dead and reclaiming it
            # re-opens exactly that hole.
            quiescent = Block.of_seq(origin for origin, _ in op.quiescent)
            return Ok(replace(state, presence=state.presence.pruned(quiescent, op.liveness_ticks)))
        case _ as unreachable:
            assert_never(unreachable)
```

## [06]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)

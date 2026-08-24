# [PY_RUNTIME_WIRE]

One wire owner serves the positional MessagePack op-log envelope and the six converging state owners its CRDT prefix materializes into. The envelope admits its payload slot as `bytes`: msgspec removes the MessagePack bin token while leaving the lane payload itself opaque, so scalar, geometry, presence, commit, branch, and attest bytes cross untouched; only a `crdt` row admits those bytes as the generated `rasm.contracts.crdt.CrdtOpWire` proto message. Every peer-decoded CRDT payload shape is therefore generated — protobuf-py owns binary encoding and decoding, protovalidate owns its constraints, and no msgspec op hierarchy or tag roster stands beside the descriptor. Vocabulary and binding table are `transport/shapes#BOOT_CENSUS`'s; this page imports the rows and owns only codec machinery, so a registry re-mint here is the deleted `shapes -> wire` back-edge.

Every decode rides the one `Decode` aspect — a direction-parameterized OTel span with the `reliability/faults#FAULT` `boundary` fence — and a network fetch stays its transport owner's retry concern, handing this aspect only the acquired bytes. Every lift on this page names the provider classes it reaches and takes its subject from a `RuntimeLeg.WIRE` roster row, so no codec raise crosses as a bare-`Exception` funnel and no refusal spells a coordinate the roster never declared. Op-log entries cross as ordinary explicit MessagePack arrays, distinct from the generated CRDT payload and the Connect wire; compression belongs to the carrying transport, so no peer must imitate MessagePack-CSharp's private `Lz4BlockArray` wrapper.

## [01]-[INDEX]

- [02]-[WIRE_RAIL]: `Decode` — the traced-railed aspect every wire boundary composes.
- [03]-[CRDT_CODEC]: generic MessagePack envelope admission beside generated-protobuf CRDT payload admission.
- [04]-[CRDT_STATE]: six field-keyed converging state owners, each publishing its own read, beside the one identity-aware `replayed` fold a replica materializes an op-log prefix through.

## [02]-[WIRE_RAIL]

- Owner: `Decode` is the one cross-cutting wire-boundary aspect every codec on this page composes — telemetry and fault conversion declared once and reused by both CRDT legs, never repeated inline per codec and never a CONSUMER-kind span mis-scoping an egress encode.
- Entry: every ingress is buffered — the durability decode reads the op-log payload whole — so `railed` and `routed` are the two entries, and the terminal decode `ValidationError` rides the `railed` boundary on the first decode, never a retry.
- Auto: `annotated` lowers through `msgspec.structs.asdict` — the field-NAME-keyed projection serving the `array_like` CRDT arms (the positional indices `to_builtins(array_like=True)` returns are meaningless) — keeping raw `bytes` for the fixed-width `.hex()` render, unlike the base64-lowering `to_builtins`.
- Law: the fault coordinate is a `RuntimeLeg.WIRE` roster row and never a free subject string, so `railed` and `routed` differ by ROW rather than by a verb literal and the SAME row names the span head — one declaration fixes what a trace shows and what `facts()` publishes, where two spellings drift the moment either moves. The row's own posture is what makes a codec fault terminal: re-decoding identical bytes fails identically, so a re-drive gate reading the ingress class alone would re-offer a refusal no retry can clear.
- Law: `catch` is REQUIRED at both entries and every composer names its provider ROOTS rather than a leaf list — `msgspec.MsgspecError` covers decode, encode, and constraint validation, and the msgpack ENCODER raises `OverflowError` on an int past the 64-bit wire before any hook runs, so the encode arm names it beside the root — while a generated class validates at ENCODE alone (`to_binary`/`to_json` raise `TypeError`, `ValueError`, `OverflowError`; construction and assignment check nothing) and no fence on this page encodes one. A defect raised inside a thunk therefore propagates as the defect it is instead of railing as a codec fault. The one plane this aspect cannot import is the INJECTED envelope codec, which therefore carries its own raise class on the value rather than being guessed at here.
- Packages: `msgspec`, `opentelemetry-api`, and the faults/resilience rails per the fence imports; the `Status`/`record_exception` egress is the faults owner's `_convert`, never re-spelled here.
- Growth: a new wire boundary composes `Decode.railed`/`routed` and inherits span and fault with zero new cross-cutting code; a new transport direction is one `(row, kind, annotate)` triple on `_traced`, its row supplying span head and fault coordinate at once; a new cross-language wire is a corpus message and imports its generated class — it earns no codec here.
- Boundary: every leg crosses the `railed`/`routed` span-and-`boundary` fence and the terminal decode fault converts exactly once — never a bare exception across a handler and never a second async rail.

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

## [03]-[CRDT_CODEC]

- Law: MessagePack survives as the thirteen-slot `OpLogEntry` envelope ALONE. Its `payload` is the decoded bin value, never `msgspec.Raw` — `Raw` includes the bin tag and length prefix, so hashing or protobuf-decoding it changes the payload. A non-`crdt` lane's bytes remain opaque; a `crdt` row alone calls `CrdtOpWire.from_binary`, and no `Any`, string bag, MessagePack arm tag, or second payload family enters the envelope.
- Owner: generated `crdt_pb.CrdtOpWire` is the canonical wire vocabulary. Its required `arm` oneof closes the ten cases, its root `field` carries the shared coordinate once, its nested `clock.Hlc` carries the 100-ns cell, and `ElementId`/`VectorSlot` carry exact 16-byte identities and unsigned counters. The convergence fold dispatches on `Oneof.field` and reads the generated arm value directly; no local wire-vs-canonical hierarchy survives.
- Law: the positional envelope slot SET and ORDER stay invariant under the payload conversion. The generated proto bytes occupy only the seventh position (`Payload`, index 6) for `family == "crdt"`; every column before and after it remains at the producer's declared position, and every non-CRDT payload remains opaque.
- Cases: LWW survives only as the `set` arm reconstructing the `LwwRegister`; `beat`/`leave` carry the `EphemeralMap` presence delta a late-joining companion reconstructs from the op-log prefix; `increment` carries the producer's `(sequence, positive, negative)` cumulative triple, so the counter absorbs by ordinal and a replayed op re-lands the same total.
- Cases: `OpLogEntry` pins the producer's declaration order — sequence, identity, model, entity key, lane, verb, payload, content key, trace, closure, actor, then HLC halves. `msgspec` rejects short or overlong records; `_admit_entry` rejects malformed widths, non-canonical context order, or any dot whose counter is not exactly one above its own context slot before a lane opens payload. `seq` resumes a drain, `id` orders and dedups, and `content_key` names payload bytes.
- Law: the trace slot is a TOP-LEVEL envelope column beside `content_key` and never a key inside `payload`, so a lane whose payload this reader never opens still continues its producing trace, and `closure` stays a declared key set the transfer differences rather than a manifest a consumer walks. Both columns cross for every lane, so no arm reads a column another lane omits.
- Law: `OperationId` carries the `(origin, counter)` dot beside the frontier its minter observed, and identity NEVER derives from payload content — two peers writing identical bytes are two operations, so a content-keyed log reports the second as a duplicate and discards a real edit. Vector slots — `context` here, `WriteOp.context` and `MaintainOp.quiescent` alike — arrive ASCENDING by origin bytes, the producer's canonical order, because a hash-ordered slot list gives one causal position a different digest per runtime and per insertion history, which is what keeps the shared fixture unfreezable.
- Auto: `OpLogCodec` admits the thirteen-position MessagePack envelope and verifies seed-zero XxHash128 for every payload before encode or after decode. `CrdtOpCodec` then serializes and admits only the generated payload through the class's `to_binary`/`from_binary` pair and runs `protovalidate.validate` on both sides. Vector and observed-tag rows must already be strict ascending producer order; the generated descriptor is the sole arm/field authority and the adapter owns no operation-arm mirror roster.
- Entry: `OpLogCodec.decode` admits the native outer envelope; `CrdtOpCodec.ops` selects its CRDT lane and decodes the generated payload while retaining the outer operation id. `CrdtOpCodec.encode` independently serializes an already-authored generated message and is not a semantic minter.
- Growth: a new envelope column is one `OpLogEntry` field the producer pins first, never a sibling struct; a new op kind is one corpus oneof arm, one identity-aware `replayed` arm, and one `CrdtState` column where it opens a new convergence family — the producer adds the descriptor member first, the companion regenerates, never ahead of the wire; an `Ext`-typed producer slot enters as one `ext_hook=`/`enc_hook=` seam on the cached codecs, never a parallel decoder. Compression remains a transport concern and never changes this message shape.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable
from typing import Final, Literal

import msgspec
from expression import Some
from expression.collections import Block, Map
from msgspec import Struct
from protobuf import Oneof
from protovalidate import CompilationError, EvaluationError, ValidationError, validate

from rasm.contracts.rasm.contracts.crdt import crdt_pb
from rasm.runtime.clock import ElementId, Hlc
from rasm.runtime.faults import Catch, RuntimeRail
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.shapes import WireI63

# --- [CONSTANTS] --------------------------------------------------------------------------

_CRDT_LANE: Final[str] = "crdt"
_CRDT_PAYLOAD_LIMIT: Final[int] = 1 << 20

_MSGPACK_RAISES: Final[Catch] = (msgspec.MsgspecError, OverflowError)
_PROTO_RAISES: Final[Catch] = (TypeError, ValueError, OverflowError, CompilationError, EvaluationError, ValidationError)

# --- [MODELS] ---------------------------------------------------------------------------


type ColumnFamily = Literal["scalar", "crdt", "geometry", "presence", "commit", "branch", "attest"]
type SyncOpKind = Literal["upsert", "delete", "truncate", "presence"]


class OperationId(Struct, frozen=True, array_like=True, forbid_unknown_fields=True):
    # operation identity: the `(origin, counter)` dot names one operation across every runtime, and `context` — the
    # frontier its minter had observed — answers happened-before between two ids with no feed walk. Keyed on payload
    # content instead, two peers writing identical bytes collapse into one operation and the second edit vanishes.
    origin: bytes
    counter: WireI63
    context: list[tuple[bytes, WireI63]]

    @property
    def observed(self) -> Map[bytes, int]:
        return Map.of_seq((origin, int(counter)) for origin, counter in self.context)

    @property
    def frontier(self) -> Map[bytes, int]:
        # this dot INCLUDED: what a replica joins once the entry lands, and what the next id at this origin carries
        # as its own context. Compaction admission reads `observed`, never this post-operation frontier.
        return self.observed.add(self.origin, int(self.counter))

    def applied(self, frontier: Map[bytes, int]) -> bool:
        return frontier.try_find(self.origin).default_value(0) >= int(self.counter)


class TraceSlot(Struct, frozen=True, array_like=True, forbid_unknown_fields=True):
    # changefeed trace SLOT and never a propagation fold: the producer READS its ambient activity once and stores the
    # 16-byte trace-id beside the tracestate bytes, so this branch continues that parent and re-mints no propagator.
    # Absence is the EMPTY slot the producer already writes for a correlation that decoded as nothing, so `parented`
    # tests the sixteen bytes rather than a nullable column every reader re-checks.
    trace_id: bytes
    tracestate: bytes

    @property
    def parented(self) -> bool:
        return len(self.trace_id) == 16


class OpLogEntry(Struct, frozen=True, array_like=True, forbid_unknown_fields=True):
    # pinned entry envelope, positional, and its slot order IS the producer record's own declaration order —
    # `[seq, [origin, counter, context], model, entity, family, kind, <raw>, content_key, [trace_id, tracestate],
    # closure, actor, physical_ticks, logical]`. The `array_like` shape requires every declared slot and refuses an
    # extra one, then `_admit_entry` proves fixed-width identity cells and strict context order before any lane opens
    # the payload, so the envelope tracks the producer whole and a column the producer gains lands here before any
    # consumer reads it. `seq` orders and resumes the producer's own drain and NOTHING
    # else; `id` is what dedups and orders causally, because a transport ordinal diverges the moment two peers resume
    # from different frontiers. `content_key` names the PAYLOAD bytes where `id` names the operation, so two peers
    # writing identical bytes stay two operations. `payload` is the MessagePack bin VALUE, not `Raw` (which also
    # retains the bin token and length prefix), so its content key hashes the exact producer payload and a non-CRDT
    # lane crosses without decoding its inner codec.
    seq: WireI63
    id: OperationId
    model: bytes
    entity: str
    family: ColumnFamily
    kind: SyncOpKind
    payload: bytes
    content_key: bytes
    trace: TraceSlot
    closure: list[bytes]
    actor: str
    physical_ticks: WireI63
    logical: WireI63

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
        return Block.of_seq(key for key in (self.content_key, *self.closure) if not held(key))


# --- [OPERATIONS] -----------------------------------------------------------------------


class CrdtOpCodec:
    # The generated protobuf class and descriptor rules own only the CRDT payload; MessagePack envelope ownership
    # remains in `OpLogCodec`.

    @classmethod
    def decode(cls, payload: bytes) -> RuntimeRail[crdt_pb.CrdtOpWire]:
        return Decode.railed("crdt", lambda: _op(payload), catch=_PROTO_RAISES)

    @classmethod
    def encode(cls, op: crdt_pb.CrdtOpWire) -> RuntimeRail[bytes]:
        return Decode.routed("crdt", lambda: _binary(op), catch=_PROTO_RAISES)

    @classmethod
    def ops(cls, entries: Block[OpLogEntry]) -> RuntimeRail[Block[tuple[OperationId, crdt_pb.CrdtOpWire]]]:
        # lane filter and second-stage protobuf decode in one pass: a foreign-lane entry drops here rather than reaching
        # `replayed`, and each surviving op keeps its id, since dedup, causal writes, and compaction all read
        # that id, which no op recovers on its own. No envelope crosses: each binary slot is already the payload value
        # `OpLogCodec.decode` handed over, so re-declaring one here would name a codec this leg never calls.
        crdt = entries.filter(lambda entry: entry.family == _CRDT_LANE)
        return Decode.railed(
            "crdt.ops", lambda: Block.of_seq((entry.id, _op(entry.payload)) for entry in crdt), catch=_PROTO_RAISES
        )


class OpLogCodec:
    _entry: msgspec.msgpack.Decoder[OpLogEntry] = msgspec.msgpack.Decoder(OpLogEntry)
    _envelope: msgspec.msgpack.Encoder = msgspec.msgpack.Encoder()

    @classmethod
    def decode(cls, payload: bytes) -> RuntimeRail[OpLogEntry]:
        return Decode.railed(
            "oplog.entry", lambda: _admit_entry(cls._entry.decode(payload)), catch=(*_MSGPACK_RAISES, ValueError)
        )

    @classmethod
    def encode(cls, entry: OpLogEntry) -> RuntimeRail[bytes]:
        return Decode.routed(
            "oplog.entry", lambda: cls._envelope.encode(_admit_entry(entry)), catch=(*_MSGPACK_RAISES, ValueError)
        )


def _op(payload: bytes, /) -> crdt_pb.CrdtOpWire:
    if len(payload) > _CRDT_PAYLOAD_LIMIT:
        raise ValueError("<crdt-payload-limit>")
    op = crdt_pb.CrdtOpWire.from_binary(payload)
    validate(op)
    _ordered(op)
    return op


def _binary(op: crdt_pb.CrdtOpWire, /) -> bytes:
    validate(op)
    _ordered(op)
    payload = op.to_binary()
    if len(payload) > _CRDT_PAYLOAD_LIMIT:
        raise ValueError("<crdt-payload-limit>")
    return payload


def _admit_entry(entry: OpLogEntry, /) -> OpLogEntry:
    context = entry.id.context
    prior = next((int(counter) for origin, counter in context if origin == entry.id.origin), 0)
    fixed = (
        len(entry.id.origin) == 16
        and len(entry.model) == 16
        and len(entry.content_key) == 16
        and len(entry.trace.trace_id) in (0, 16)
        and all(len(origin) == 16 for origin, _ in context)
        and all(len(key) == 16 for key in entry.closure)
    )
    context_ordered = all(left[0] < right[0] for left, right in zip(context, context[1:], strict=False))
    closure_ordered = all(left < right for left, right in zip(entry.closure, entry.closure[1:], strict=False))
    content_matches = ContentIdentity.key("oplog.payload", entry.payload, seed=Some(0)).value == int.from_bytes(
        entry.content_key, "big"
    )
    if (
        not fixed
        or not context_ordered
        or not closure_ordered
        or entry.content_key in entry.closure
        or int(entry.id.counter) != prior + 1
        or not content_matches
    ):
        raise ValueError("<oplog-envelope-contract>")
    return entry


def _ordered(op: crdt_pb.CrdtOpWire, /) -> None:
    match op.arm:
        case Oneof(field="write", value=crdt_pb.WriteOp(context=rows)) | Oneof(
            field="maintain", value=crdt_pb.MaintainOp(quiescent=rows)
        ) if any(left.origin >= right.origin for left, right in zip(rows, rows[1:], strict=False)):
            raise ValueError("<crdt-vector-order>")
        case Oneof(field="remove", value=crdt_pb.RemoveOp(observed_tags=rows)) if any(
            (left.origin, left.logical) >= (right.origin, right.logical) for left, right in zip(rows, rows[1:], strict=False)
        ):
            raise ValueError("<crdt-tag-order>")
        case _:
            return
```

## [04]-[CRDT_STATE]

- Owner: `CrdtState` is the materialized replica — one field-keyed column per convergence family, `LwwRegister`, `MvRegister`, `OrSet`, `Rga`, `PnCounter`, and `EphemeralMap` — and `replayed(state, frontier, entries)` the one fold every replica materializes an op-log prefix through. The outer `OperationId` stays paired with its generated `CrdtOpWire` through every arm because redelivery, causal-write dominance, and safe maintenance all read that dot; an identity-free op fold is not a lawful consumer. Each family owns its own absorb law as a method on its own shape, so routing alone remains in the fold and replay returns a frozen successor rather than mutating a cell two readers share.
- Cases: the generated `CrdtOpWire.arm` oneof's ten members close onto six families — `set` lands the single-value LWW register, `write` the causal multi-value register, `add`/`remove` the observed set, `insert_after`/`delete` the sequence, `increment` the counter, `beat`/`leave` the presence map, and `maintain` compacts the already-seated sequence or presence family at its named field. Every arm spends the root `field`, and one field may seat in exactly one family column; a family-changing operation refuses rather than duplicating one logical field across independent maps.
- Law: every clock-adjudicated survivor decision reads `evidence/clock#CLOCK` `compare` and folds its `Ordering`. LWW equal cells break on origin; the same cell and origin with different bytes refuses as a fork. Presence equal cells prefer beat, then bytes, so the join is permutation-independent. Multi-value survival is causal and reads no clock.
- Law: a causally-contexted `write` compares its generated per-register context against each held write's outer dot. A candidate removes exactly the version dots that context covers; genuinely concurrent writes all survive. The outer operation context remains the replica-wide frontier used by replay and maintenance, while `WriteOp.context` is the register-specific observation set — neither substitutes for the other. HLC, payload `origin`, dot, and value order only the surviving read canonically and never elect one concurrent value over another, because that would silently turn the multi-value register into last-writer-wins.
- Law: every arm is idempotent and order-insensitive under the tags the wire already carries — an `add` re-adds one tag to a set, a `remove` tombstones exactly the tags it observed so a re-delivered add stays dead, a `delete` tombstones an id whether or not its insert landed yet, and presence retains stamped live and left states under a monotone maintenance horizon — so every permutation and replay converges. `increment` carries the producer's per-origin ordinal in place of an id: a higher ordinal admits only monotone cumulative halves, an equal ordinal must repeat both halves exactly, and a fork refuses instead of making arrival order the winner.
- Law: `replayed` is the only materialization fold. A dot already under the applied frontier skips as a redelivery, where content equality would report a second genuine edit of identical bytes as the same skip and lose it; a `maintain` whose minter never observed its horizon refuses; and the same dot reaches the multi-value register as the version its causal context needs. `OpLogEntry.seq` resumes the producer drain and no arm reads it, because a state keyed on a transport ordinal diverges the moment peers resume from different frontiers.
- Law: observed-set removal spends both generated coordinates: `element` selects the member and `observed_tags` tombstones only tags observed for that member. A global tag tombstone discards the element coordinate the descriptor deliberately carries and lets malformed input remove a different member's add.
- Law: RGA compaction removes retired value bytes without cutting or reordering the insertion tree. A retired identity becomes a value-free routing tombstone: its predecessor edge and child adjacency remain, so preorder still visits its descendants at the exact former position. Reparenting those children and sorting them among the retired node's siblings changes order; dropping the adjacency loses them outright.
- Law: every convergence column publishes its own READ on the shape that owns the state — `LwwRegister.value`, `MvRegister.values`, `OrSet.members`, `Rga.ordered`, `PnCounter.value`, `EphemeralMap.beats` — because a write-only column converges a state no replica can project back out, and a reader seated anywhere else re-derives an ordering law the family already holds. `Rga` alone reads by WALK rather than by field, so it alone takes the shared `reliability/faults#FAULT` `Depth` and rails a typed exhaustion where the other five are total and take no bound. No graph substrate backs that walk: `networkx` names `data` and `geometry` as its owners, and `libs/python/.planning/RULINGS.md` keeps graph reducers plural precisely because each owns a node index a merged owner re-keys — re-keying an adjacency whose determinism IS the synthesized `ElementId` order onto a foreign index is the ruled defect, not the repair.
- Entry: `maintain` requires one already-seated sequence or presence family; absence and cross-family ambiguity refuse because the wire carries no family discriminant to resolve them. `insert_after` refuses an undefined predecessor, the zero root as an element identity, a self predecessor, and any reuse of an identity with different predecessor or value; those gates keep the retained routing tree acyclic and make replay of the same insert the only legal reuse.
- Acceptance: fold each causally eligible ordering of an admitted op multiset, then fold that ordering again; state and frontier must be identical. Presence permutations include maintain-before-beat and leave-before-beat, RGA permutations delete-before-insert and maintain-before-replay, and PN ties include both exact replay and divergent fork refusal. A delivery whose outer context is not under the held frontier refuses as a causal gap rather than advancing that frontier and later misclassifying the missing operation as replay.
- Growth: a new convergence family is one `CrdtState` column with its own absorb method and its arms' routing rows; a new op on a standing family is one arm and no column; a new tiebreak axis is one field on the family that needs it, reaching the fold through its own method; a new family projection is one method on the family that owns its state, never a reader seated on `CrdtState` or re-derived at a consumer.
- Boundary: this owner materializes and never transports — the codec above owns the bytes, the clock owner the comparison algebra, and the durable op-log its own persistence. No column carries a wall-clock instant: ordering is the `Hlc` cell alone, so a host whose clock drifts still converges.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from typing import Final, assert_never

from expression import Error, Ok, Option, Some
from expression.collections import Block, Map
from msgspec import Struct
from msgspec.structs import replace
from protobuf import Oneof

from rasm.contracts.rasm.contracts.clock import hlc_pb
from rasm.contracts.rasm.contracts.crdt import crdt_pb
from rasm.runtime.clock import ElementId, Hlc
from rasm.runtime.faults import WIRE_INSERT, WIRE_MAINTAIN, WIRE_ORDERED, Depth, RuntimeRail
from rasm.runtime.identity import ContentIdentity
# `CrdtOpWire` is generated at [03]-[CRDT_CODEC]; this region owns only materialized state and its fold.

# --- [CONSTANTS] --------------------------------------------------------------------------

# sequence's own zero: a producer names it as the predecessor of a head insert, so the head case is a VALUE the
# insert arm already handles rather than a nullable predecessor slot every reader re-checks.
_ROOT: Final[ElementId] = ElementId(bytes(16), 0)

# --- [MODELS] ---------------------------------------------------------------------------


class LwwRegister(Struct, frozen=True):
    # The uncontexted `set` register: `cell` is the stamp and `origin` the equal-cell tiebreak.
    value: bytes = b""
    cell: Hlc = Hlc(0, 0)
    origin: bytes = b""

    def absorbed(self, candidate: "LwwRegister") -> RuntimeRail["LwwRegister"]:
        # survivor decision, read through ONE `fold` call site on the clock owner's own verdict — never a
        # re-derived sign comparison here. `equal` breaks on origin bytes: two replicas can legitimately stamp one
        # cell, and choosing arbitrarily there leaves the two materializations permanently divergent.
        return self.cell.compare(candidate.cell).fold(
            before=lambda: Ok(candidate),
            equal=lambda: Ok(candidate if candidate.origin > self.origin else self)
            if candidate.origin != self.origin or candidate.value == self.value
            else Error(WIRE_ORDERED.raised("<lww-cell-fork>")),
            after=lambda: Ok(self),
        )

class MvEntry(Struct, frozen=True):
    # A held value keeps the OUTER operation version whole. `version.observed` is what its minter had seen and
    # `version.frontier` adds this write's own dot; the generated context separately states which register versions the
    # write observed. Storing only that context makes every first concurrent write look like the same empty version.
    value: bytes
    version: OperationId
    context: Map[bytes, int]
    cell: Hlc
    origin: bytes

    @property
    def order(self) -> tuple[int, int, bytes, bytes, int, bytes]:
        # Canonical READ order only. Concurrent entries all survive; HLC and origins make their projection independent
        # of replay order without turning those coordinates into a winner election.
        return (
            self.cell.physical_ticks,
            self.cell.logical,
            self.origin,
            self.version.origin,
            int(self.version.counter),
            self.value,
        )


class MvRegister(Struct, frozen=True):
    # The causal anti-chain: a dominating write removes the values it observed, while concurrent writes survive
    # together. HLC never adjudicates survival because doing so silently turns `write` back into the `set` arm.
    values: Block[MvEntry] = Block.empty()

    def written(self, candidate: MvEntry) -> RuntimeRail["MvRegister"]:
        joined = [*self.values, candidate]
        if any(
            left.version.origin == right.version.origin
            and left.version.counter == right.version.counter
            and left != right
            for index, left in enumerate(joined)
            for right in joined[index + 1 :]
        ):
            return Error(WIRE_ORDERED.raised("<mv-dot-fork>"))
        survivors: list[MvEntry] = []
        for held in joined:
            if any(
                (other.version.origin, other.version.counter)
                != (held.version.origin, held.version.counter)
                and (
                    other.context.try_find(held.version.origin)
                    .map(lambda counter: counter >= int(held.version.counter))
                    .default_value(False)
                )
                for other in joined
            ):
                continue
            if all(
                (other.version.origin, other.version.counter)
                != (held.version.origin, held.version.counter)
                for other in survivors
            ):
                survivors.append(held)
        return Ok(MvRegister(Block.of_seq(sorted(survivors, key=lambda entry: entry.order))))


class OrSet(Struct, frozen=True):
    # observed-remove set: live tags per element beside tombstones keyed by that SAME element. Both halves are required for
    # order-insensitivity — a remove carrying the tags it observed must stay effective when its add arrives later,
    # while the generated `RemoveOp.element` prevents those tags from deleting a different member.
    tags: Map[bytes, Block[ElementId]] = Map.empty()
    tombstones: Map[bytes, Block[ElementId]] = Map.empty()

    def added(self, element: bytes, tag: ElementId) -> "OrSet":
        held = self.tags.try_find(element).default_value(Block.empty())
        return OrSet(
            tags=self.tags.add(element, held if tag in held else held.append(Block.singleton(tag)).sort()),
            tombstones=self.tombstones,
        )

    def removed(self, element: bytes, observed: Block[ElementId]) -> "OrSet":
        # tombstoning exactly the OBSERVED tags is what makes the remove commute with a later add of a fresh tag —
        # clearing the element's whole slot erases a concurrent add, while a global tombstone discards `element` and
        # lets malformed input remove a tag from a different member.
        held = self.tombstones.try_find(element).default_value(Block.empty())
        fresh = observed.filter(lambda tag: tag not in held)
        return OrSet(tags=self.tags, tombstones=self.tombstones.add(element, held.append(fresh).sort()))

    def members(self) -> Block[bytes]:
        return Block.of_seq(
            element
            for element, held in self.tags.items()
            if any(tag not in self.tombstones.try_find(element).default_value(Block.empty()) for tag in held)
        ).sort()


class Rga(Struct, frozen=True):
    # replicated growable array as an insertion TREE its own `ordered` flattens: `after` maps each predecessor id to
    # the ids inserted directly after it, held in the synthesized `ElementId` order, so two concurrent inserts against
    # one predecessor sort identically on every replica. A positional index cannot carry that — the same offset names
    # different elements on two replicas the instant either inserts — and the tombstone set is separate from `values`
    # so a delete arriving ahead of its insert still suppresses it.
    values: Map[ElementId, bytes] = Map.empty()
    after: Map[ElementId, Block[ElementId]] = Map.empty()
    tombstones: Block[ElementId] = Block.empty()
    routing: Block[ElementId] = Block.empty()
    fingerprints: Map[ElementId, int] = Map.empty()

    def inserted(self, predecessor: ElementId, identity: ElementId, value: bytes) -> RuntimeRail["Rga"]:
        parents = Block.of_seq(parent for parent, children in self.after.items() if identity in children)
        if identity == _ROOT or predecessor == identity or len(parents) > 1:
            return Error(WIRE_INSERT.raised(identity.origin.hex(), str(identity.logical)))
        fingerprint = ContentIdentity.key("crdt-rga", value, seed=Some(0)).value
        if not parents.is_empty():
            held = self.values.try_find(identity)
            retained = self.fingerprints.try_find(identity).default_value(
                ContentIdentity.key("crdt-rga", held.default_value(b""), seed=Some(0)).value
            )
            if parents.head() != predecessor or retained != fingerprint:
                return Error(WIRE_INSERT.raised(identity.origin.hex(), str(identity.logical)))
            return Ok(self)
        if self.values.try_find(identity).is_some():
            return Error(WIRE_INSERT.raised(identity.origin.hex(), str(identity.logical)))
        siblings = self.after.try_find(predecessor).default_value(Block.empty())
        return Ok(
            Rga(
                values=self.values if identity in self.routing else self.values.add(identity, value),
                after=self.after.add(
                    predecessor,
                    siblings if identity in siblings else siblings.append(Block.singleton(identity)).sort(),
                ),
                tombstones=self.tombstones,
                routing=self.routing,
                fingerprints=self.fingerprints.add(identity, fingerprint),
            )
        )

    def deleted(self, identity: ElementId) -> "Rga":
        # a delete tombstones whether or not its insert landed yet, so a prefix delivering the pair in either order
        # converges; dropping the value outright would let a later insert of that id resurrect it.
        return Rga(
            values=self.values,
            after=self.after,
            tombstones=(
                self.tombstones
                if identity in self.tombstones
                else self.tombstones.append(Block.singleton(identity)).sort()
            ),
            routing=self.routing,
            fingerprints=self.fingerprints,
        )

    def pruned(self, quiescent: Map[bytes, int]) -> "Rga":
        # A retired value may still locate live descendants. Keep its topology as an explicit value-free routing
        # tombstone: reparenting its children and sorting them among the retired node's siblings changes preorder, while
        # deleting its adjacency loses the subtree. Quiescence retires the value, not the position descendants name.
        retired = self.tombstones.filter(
            lambda identity: self.values.try_find(identity).is_some()
            and quiescent.try_find(identity.origin).default_value(0) >= identity.logical
        )
        return Rga(
            values=Map.of_seq(
                (identity, value) for identity, value in self.values.items() if identity not in retired
            ),
            after=self.after,
            tombstones=self.tombstones.filter(lambda identity: identity not in retired).sort(),
            routing=self.routing.append(retired.filter(lambda identity: identity not in self.routing)).sort(),
            fingerprints=self.fingerprints,
        )

    def defined(self, identity: ElementId) -> bool:
        # A compacted predecessor remains DEFINED as topology even after its payload bytes retire, so a concurrent
        # descendant arriving after maintenance attaches at the original position instead of refusing as a false gap.
        return self.values.try_find(identity).is_some() or identity in self.routing

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
        # successor lands and conflicting id reuse refuses at insertion — so `fixpoint` terminates and the bound is a
        # DEPTH ceiling, never a cycle guard.
        frontier: Block[tuple[ElementId, Depth]] = Block.singleton((_ROOT, bound))
        emitted: list[bytes] = []
        while not frontier.is_empty():  # Exemption: streaming preorder frontier — chain-shaped depth forfeits the recursive form
            (identity, depth), frontier = frontier.head(), frontier.tail()
            if identity not in self.tombstones and identity not in self.routing:
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

    def incremented(self, origin: bytes, sequence: int, positive: int, negative: int) -> RuntimeRail["PnCounter"]:
        # highest ordinal per origin wins WHOLE: both halves are monotone at the producer, so a lower ordinal
        # carries a prefix of what the held pair already absorbed and re-applying it walks the total backwards.
        candidate = (sequence, positive, negative)
        held = self.buckets.try_find(origin)
        if held.is_none():
            return Ok(PnCounter(buckets=self.buckets.add(origin, candidate)))
        prior = held.default_value(candidate)
        if candidate == prior:
            return Ok(self)
        if sequence < prior[0] and positive <= prior[1] and negative <= prior[2]:
            return Ok(self)
        if sequence == prior[0] or positive < prior[1] or negative < prior[2]:
            return Error(WIRE_ORDERED.raised(f"<counter-fork:{origin.hex()}:{sequence}>"))
        if sequence < prior[0]:
            return Error(WIRE_ORDERED.raised(f"<counter-fork:{origin.hex()}:{sequence}>"))
        return Ok(PnCounter(buckets=self.buckets.add(origin, candidate)))

    @property
    def value(self) -> int:
        return sum(positive - negative for _, positive, negative in self.buckets.values())


class PresenceCell(Struct, frozen=True):
    state: Option[bytes]
    cell: Hlc

    def absorbed(self, candidate: "PresenceCell") -> "PresenceCell":
        def tied() -> "PresenceCell":
            if self.state.is_some() != candidate.state.is_some():
                return self if self.state.is_some() else candidate
            held = self.state.default_value(b"")
            offered = candidate.state.default_value(b"")
            return candidate if offered > held else self

        return self.cell.compare(candidate.cell).fold(before=lambda: candidate, equal=tied, after=lambda: self)


class EphemeralMap(Struct, frozen=True):
    # Presence is a stamped add-wins register per origin. Left cells remain as tombstones, while the retained horizon
    # compacts old cells and rejects their late replay without depending on maintenance delivery order.
    cells: Map[bytes, PresenceCell] = Map.empty()
    horizon: int = 0

    @property
    def beats(self) -> Map[bytes, tuple[bytes, Hlc]]:
        return Map.of_seq(
            (origin, (cell.state.default_value(b""), cell.cell))
            for origin, cell in self.cells.items()
            if cell.state.is_some()
        )

    def beaten(self, origin: bytes, state: bytes, cell: Hlc) -> "EphemeralMap":
        if cell.physical_ticks < self.horizon:
            return self
        candidate = PresenceCell(state=Option.Some(state), cell=cell)
        survivor = self.cells.try_find(origin).map(lambda held: held.absorbed(candidate)).default_value(candidate)
        return EphemeralMap(cells=self.cells.add(origin, survivor), horizon=self.horizon)

    def left(self, origin: bytes, cell: Hlc) -> "EphemeralMap":
        if cell.physical_ticks < self.horizon:
            return self
        candidate = PresenceCell(state=Option.Nothing(), cell=cell)
        survivor = self.cells.try_find(origin).map(lambda held: held.absorbed(candidate)).default_value(candidate)
        return EphemeralMap(cells=self.cells.add(origin, survivor), horizon=self.horizon)

    def pruned(self, horizon: int) -> "EphemeralMap":
        # Presence compaction reads the physical liveness horizon alone; causal quiescence belongs to RGA tombstone
        # reclamation. The horizon is absolute on the producer's tick axis, so every replica compacts the identical
        # set — a locally derived window prunes by whichever host clock read it and leaves two replicas divergent.
        advanced = max(self.horizon, horizon)
        return EphemeralMap(
            cells=Map.of_seq(
                (origin, held)
                for origin, held in self.cells.items()
                if held.cell.physical_ticks >= advanced
            ),
            horizon=advanced,
        )


class CrdtState(Struct, frozen=True):
    # Every generated operation spends its root `field` against exactly one family map. A singleton family value here
    # collapses two unrelated fields before their algebra runs; field-keyed maps preserve the producer coordinate and
    # keep each family's own read. Frozen whole — replay returns a successor rather than mutating a shared cell.
    register: Map[str, LwwRegister] = Map.empty()
    multi: Map[str, MvRegister] = Map.empty()
    observed: Map[str, OrSet] = Map.empty()
    sequence: Map[str, Rga] = Map.empty()
    counter: Map[str, PnCounter] = Map.empty()
    presence: Map[str, EphemeralMap] = Map.empty()

    def families(self, field: str) -> RuntimeRail[Block[str]]:
        seated = Block.of_seq(
            family
            for family, column in (
                ("register", self.register),
                ("multi", self.multi),
                ("observed", self.observed),
                ("sequence", self.sequence),
                ("counter", self.counter),
                ("presence", self.presence),
            )
            if column.try_find(field).is_some()
        )
        return Error(WIRE_ORDERED.raised(f"<crdt-field-family:{field}>")) if len(seated) > 1 else Ok(seated)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _covers(candidate: Map[bytes, int], held: Map[bytes, int]) -> bool:
    return all(candidate.try_find(origin).default_value(0) >= counter for origin, counter in held.items())


def _vector(context: list[crdt_pb.VectorSlot]) -> Map[bytes, int]:
    return Map.of_seq((slot.origin, slot.sequence) for slot in context)


def _id(identity: crdt_pb.ElementId) -> ElementId:
    return ElementId(identity.origin, identity.logical)


def _cell(stamp: hlc_pb.Hlc) -> Hlc:
    return Hlc(stamp.physical, stamp.logical)


def _seated(state: CrdtState, field: str, family: str) -> RuntimeRail[None]:
    return state.families(field).bind(
        lambda held: Ok(None)
        if held.is_empty() or held.head() == family
        else Error(WIRE_ORDERED.raised(f"<crdt-field-family:{field}:{held.head()}:{family}>"))
    )


def replayed(
    state: CrdtState, frontier: Map[bytes, int], entries: Block[tuple[OperationId, crdt_pb.CrdtOpWire]]
) -> RuntimeRail[tuple[CrdtState, Map[bytes, int]]]:
    # The ONLY materialization fold, threading the applied frontier beside state and keeping each generated payload
    # paired with its outer dot. That dot gates redelivery and maintenance, and becomes the complete held version for
    # a causal write; an op-only twin necessarily loses at least one of those guarantees.
    def stepped(
        rail: RuntimeRail[tuple[CrdtState, Map[bytes, int]]], entry: tuple[OperationId, crdt_pb.CrdtOpWire]
    ) -> RuntimeRail[tuple[CrdtState, Map[bytes, int]]]:
        identity, op = entry
        return rail.bind(
            lambda held: Ok(held)
            if identity.applied(held[1])
            else _admissible(held[1], identity, op).bind(
                lambda _: _applied(held[0], identity, op).map(
                    lambda moved: (moved, _joined(held[1], identity.frontier))
                )
            )
        )

    return entries.fold(stepped, Ok((state, frontier)))


def _admissible(
    frontier: Map[bytes, int], identity: OperationId, op: crdt_pb.CrdtOpWire
) -> RuntimeRail[None]:
    if not _covers(frontier, identity.observed):
        return Error(WIRE_ORDERED.raised(f"<crdt-causal-gap:{identity.origin.hex()}:{identity.counter}>"))
    match op.arm:
        case Oneof(field="maintain", value=crdt_pb.MaintainOp(quiescent=quiescent)) if not _covers(
            identity.observed, _vector(quiescent)
        ):
            return Error(WIRE_MAINTAIN.raised(identity.origin.hex()))
        case _:
            return Ok(None)


def _joined(held: Map[bytes, int], advanced: Map[bytes, int]) -> Map[bytes, int]:
    return Map.of_seq(
        (origin, max(held.try_find(origin).default_value(0), advanced.try_find(origin).default_value(0)))
        for origin in set(held.keys()) | set(advanced.keys())
    )


def _applied(
    state: CrdtState, identity: OperationId, op: crdt_pb.CrdtOpWire
) -> RuntimeRail[CrdtState]:
    # Private routing arm under `replayed`: the outer identity is mandatory even where a particular family spends no
    # dot field, so adding an identity-dependent arm cannot be bypassed through a second public fold.
    match op.arm:
        case Oneof(field="set", value=crdt_pb.SetOp(value=value, stamp=hlc_pb.Hlc() as stamp, origin=origin)):
            held = state.register.try_find(op.field).default_value(LwwRegister())
            return _seated(state, op.field, "register").bind(
                lambda _: held.absorbed(LwwRegister(value=value, cell=_cell(stamp), origin=origin)).map(
                    lambda advanced: replace(
                        state,
                        register=state.register.add(op.field, advanced),
                    )
                )
            )
        case Oneof(field="write", value=crdt_pb.WriteOp(value=value, context=context, stamp=hlc_pb.Hlc() as stamp, origin=origin)):
            held = state.multi.try_find(op.field).default_value(MvRegister())
            candidate = MvEntry(
                value=value,
                version=identity,
                context=_vector(context),
                cell=_cell(stamp),
                origin=origin,
            )
            return _seated(state, op.field, "multi").bind(
                lambda _: held.written(candidate).map(
                    lambda advanced: replace(state, multi=state.multi.add(op.field, advanced))
                )
            )
        case Oneof(field="add", value=crdt_pb.AddOp(element=element, tag=crdt_pb.ElementId() as tag)):
            held = state.observed.try_find(op.field).default_value(OrSet())
            return _seated(state, op.field, "observed").map(
                lambda _: replace(state, observed=state.observed.add(op.field, held.added(element, _id(tag))))
            )
        case Oneof(field="remove", value=crdt_pb.RemoveOp(element=element, observed_tags=tags)):
            held = state.observed.try_find(op.field).default_value(OrSet())
            return _seated(state, op.field, "observed").map(
                lambda _: replace(
                    state,
                    observed=state.observed.add(
                        op.field, held.removed(element, Block.of_seq(_id(tag) for tag in tags))
                    ),
                )
            )
        case Oneof(field="increment", value=crdt_pb.IncrementOp(origin=origin, sequence=sequence, positive=positive, negative=negative)):
            held = state.counter.try_find(op.field).default_value(PnCounter())
            return _seated(state, op.field, "counter").bind(
                lambda _: held.incremented(origin, sequence, positive, negative).map(
                    lambda advanced: replace(state, counter=state.counter.add(op.field, advanced))
                )
            )
        case Oneof(field="insert_after", value=crdt_pb.InsertAfterOp(
            predecessor=crdt_pb.ElementId() as predecessor, id=crdt_pb.ElementId() as identity, value=value
        )):
            # ONE fold refusal: the transport delivers an ordered prefix, so an unknown predecessor is a gap in
            # that prefix rather than a normal out-of-order arrival, and head-inserting instead would reorder the
            # sequence every peer holds. The root id is the sequence's own zero, always defined. The row's arm is
            # `boundary` and never `wire`: the gap is a seam classification of delivered material carrying no protocol
            # code, and a `0` there publishes a status this fold never read.
            after, item = _id(predecessor), _id(identity)
            held = state.sequence.try_find(op.field).default_value(Rga())
            if after != _ROOT and not held.defined(after):
                return Error(WIRE_INSERT.raised(after.origin.hex(), str(after.logical)))
            return _seated(state, op.field, "sequence").bind(
                lambda _: held.inserted(after, item, value).map(
                    lambda advanced: replace(state, sequence=state.sequence.add(op.field, advanced))
                )
            )
        case Oneof(field="delete", value=crdt_pb.DeleteOp(id=crdt_pb.ElementId() as identity)):
            held = state.sequence.try_find(op.field).default_value(Rga())
            item = _id(identity)
            if item == _ROOT:
                return Error(WIRE_INSERT.raised(item.origin.hex(), str(item.logical)))
            return _seated(state, op.field, "sequence").map(
                lambda _: replace(state, sequence=state.sequence.add(op.field, held.deleted(item)))
            )
        case Oneof(field="beat", value=crdt_pb.BeatOp(origin=origin, state=value, stamp=hlc_pb.Hlc() as stamp)):
            held = state.presence.try_find(op.field).default_value(EphemeralMap())
            return _seated(state, op.field, "presence").map(
                lambda _: replace(
                    state,
                    presence=state.presence.add(op.field, held.beaten(origin, value, _cell(stamp))),
                )
            )
        case Oneof(field="leave", value=crdt_pb.LeaveOp(origin=origin, stamp=hlc_pb.Hlc() as stamp)):
            held = state.presence.try_find(op.field).default_value(EphemeralMap())
            return _seated(state, op.field, "presence").map(
                lambda _: replace(
                    state,
                    presence=state.presence.add(op.field, held.left(origin, _cell(stamp))),
                )
            )
        case Oneof(field="maintain", value=crdt_pb.MaintainOp(quiescent=rows, liveness_ticks=horizon)):
            # The wire has no maintenance-family discriminant, so only an already-seated sequence or presence cell can
            # resolve this arm. Guessing sequence for absence or preferring presence in an ambiguous state is order law.
            return state.families(op.field).bind(
                lambda families: (
                    Ok(
                        replace(
                            state,
                            sequence=state.sequence.add(
                                op.field,
                                state.sequence.try_find(op.field).default_value(Rga()).pruned(_vector(rows)),
                            ),
                        )
                    )
                    if not families.is_empty() and families.head() == "sequence"
                    else Ok(
                        replace(
                            state,
                            presence=state.presence.add(
                                op.field,
                                state.presence.try_find(op.field).default_value(EphemeralMap()).pruned(horizon),
                            ),
                        )
                    )
                    if not families.is_empty() and families.head() == "presence"
                    else Error(WIRE_MAINTAIN.raised(op.field))
                )
            )
        case None:
            return Error(WIRE_ORDERED.raised("<crdt-arm-unset>"))
```

## [05]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
